using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using Com.Eudonet.Merge;
using EudoExtendedClasses;
using EudoQuery;
using Com.Eudonet.Core.Model;
using System.IO;
using System.Web.UI.HtmlControls;

namespace Com.Eudonet.Xrm
{
    /// <className>eExternalPage</className>
    /// <summary>Classe parente des pages à usage externe, type Générateur de formulaires ou Tracking</summary>
    /// <purpose>Permet la centralisation de la gestion d'erreur des page ASPX</purpose>
    /// <authors>xxx</authors>
    /// <date>2014-06-27</date>
    public partial class eExternalPage<Loader> : Page where Loader : ILoadQueryString
    {
        private const string DefaultTitlePage = "Eudonet XRM";

        /// <summary>Gestion des inscriptions de CSS et/ou de SCRIPT</summary>
        protected eWebUiPageRegisters PageRegisters { get; set; }

        /// <summary>Theme par defaut</summary>
        public ePrefLite.Theme DefaultTheme { get; private set; }

        /// <summary>Objet Pref de la base et du user d'accès externe (EDN_TRACKS ou EDN_FORMULAR)</summary>
        protected ePref _pref = null;
        /// <summary>Objet temporaire pour la description de connexion à SQL</summary>
        protected ePrefSQL _prefSqlClient = null;
        /// <summary>Connexion a la base client</summary>
        protected eudoDAL _dalClient = null;

        /// <summary>Objet de gestion de la Query string</summary>
        protected Loader DataParam { get; set; }

        /// <summary>Type de rendu de la page</summary>
        public ExternalPageRendType RendType { get; set; }
        /// <summary>Titre de la page. Par defaut : Eudonet XRM</summary>
        public string PageTitle { get; set; }

        /// <summary>Utilitaire du request</summary>
        public eRequestTools _requestTools;
        /// <summary>Informations sur l'URL de la page externalisée</summary>
        internal ExternalPageQueryString _pageQueryString;

        /// <summary>
        /// Dictionnaire de paramètres suplémentaires, utilisé pour la externalPage
        /// </summary>
        protected Dictionary<string, string> dicAddedParam = new Dictionary<string, string>();

        /// <summary>
        /// Permet de modifier le HttpResponse (redirection, type, etc...) après avoir envoyer les erreurs silencieuses ou pas
        /// </summary>
        protected List<Action<HttpResponse>> _finishCmd = new List<Action<HttpResponse>>();

        #region gestion d'erreur

        /// <summary>
        /// La page est-elle en erreur ?
        /// </summary>
        protected bool _anError = false;
        /// <summary>
        /// Message d'erreur éventuel à afficher sur le conteneur de la page ASPX
        /// </summary>
        public string _panelErrorMsg = string.Empty;
        /// <summary>
        /// Stockage des messages d'erreur
        /// </summary>
        protected List<string> _msgFeedback = new List<string>();

        #endregion

        #region variables de gestion de cache

        /// <summary>
        /// Date de début de génération
        /// </summary>
        private DateTime _dtStart = new DateTime();

        #endregion

        /// <summary>
        /// Permet d'appeler le Page_Load de base en plus (et avant) de celui de la méthode fille
        /// </summary>
        public eExternalPage()
        {
            this.PageTitle = DefaultTitlePage;
            this.DefaultTheme = ePrefLite.Theme.GetDefaultTheme();
            this.Load += new EventHandler(this.Page_Load);
        }

        /// <summary>
        /// Insert le temps de génération dans les entete http
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoadComplete(EventArgs e)
        {
            base.OnLoadComplete(e);
            if (_dtStart != null)
                HttpContext.Current.Response.Headers.Add("X-time", (DateTime.Now - _dtStart).TotalMilliseconds.ToString());
        }

        /// <summary>
        /// Ajoute des actions personnalisé sur le prerender de la page
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            this.PageRegisters = new eWebUiPageRegisters(this.Server, GetHeadPlaceHolder());

            base.OnInit(e);
            this.Page.PreRender += new EventHandler(PagePrerender);
        }

        /// <summary>
        /// Crée un label suivant un ID, un text et les classes.
        /// </summary>
        /// <param name="sID"></param>
        /// <param name="sText"></param>
        /// <param name="sClass"></param>
        /// <param name=""></param>
        /// <returns></returns>
        private Label CreateLabel(string sID, string sText, string sClass)
        {
            Label lbl = new Label();
            lbl.CssClass = sClass;
            lbl.ID = sID;
            lbl.Text = sText;

            return lbl;
        }

        /// <summary>
        /// Css Global au texte.
        /// </summary>
        /// <param name="dynamicClassStyle"></param>
        private void CreateStyleResponse(CssStyleCollection dynamicClassStyle)
        {
            dynamicClassStyle.Add("display", "flex");
            dynamicClassStyle.Add("justify-content", "center");
            dynamicClassStyle.Add("padding", "75px");

        }

        /// <summary>
        /// Le css des titres
        /// </summary>
        /// <param name="dynamicClassStyle"></param>
        private void CreateStyleTitle(CssStyleCollection dynamicClassStyle)
        {
            CreateStyleResponse(dynamicClassStyle);
            dynamicClassStyle.Add("font-family", "lato-bold");
            dynamicClassStyle.Add("font-size", "1.5rem");
            dynamicClassStyle.Add("color", "#bb1515");
            dynamicClassStyle.Add("margin", "0px 15px");
        }

        /// <summary>
        /// Le css des texte
        /// </summary>
        /// <param name="dynamicClassStyle"></param>
        private void CreateStyleText(CssStyleCollection dynamicClassStyle)
        {
            CreateStyleResponse(dynamicClassStyle);
            dynamicClassStyle.Add("font-family", "lato-regular");
            dynamicClassStyle.Add("font-size", "1rem");
        }

        /// <summary>
        /// Css de l'image
        /// </summary>
        private void CreateStyleImage(CssStyleCollection dynamicClassStyle)
        {
            CreateStyleResponse(dynamicClassStyle);
            dynamicClassStyle.Add("height", "450px");
            dynamicClassStyle.Add("margin", "auto");
        }

        /// <summary>
        /// Permet d'ajouter une liste de controles à un control parent.
        /// </summary>
        /// <param name="ctrlParent"></param>
        /// <param name="lstControls"></param>
        private void AddControlsToParent(ControlCollection ctrlParent, IList<Control> lstControls)
        {
            foreach (Control control in lstControls)
                ctrlParent.Add(control);
        }

        /// <summary>
        /// Permet à partir d'une liste de controles de faire le rendu html.
        /// </summary>
        /// <param name="ctrl"></param>
        /// <returns></returns>
        private string RenderHTMLToDisplay(IEnumerable<Control> ctrl)
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            using (HtmlTextWriter tw = new HtmlTextWriter(sw))
            {
                foreach (Control ctr in ctrl)
                {
                    ctr.RenderControl(tw);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Retourne tous les controles de la page.
        /// </summary>
        /// <returns></returns>
        private ControlCollection GetPageControls()
        {
            if (this.phReturn == null)
                this.phReturn = new PlaceHolder();

            return this.phReturn.Controls;
        }

        /// <summary>
        /// Action a mener dans le prerenderer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void PagePrerender(object sender, EventArgs e)
        {
            if (Page != null && GetHeadPlaceHolder() != null && !_anError)
            {
                PageRegisters.RegisterIncludeScript();
                PageRegisters.RegisterScript();
                PageRegisters.RegisterCSS();
            }
        }

        /// <summary>
        /// Chargement de la page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            _dtStart = DateTime.Now;

            try
            {

                _requestTools = new eRequestTools(this.Context);
                _pageQueryString = ExternalPageQueryString.GetNewByQueryString(_requestTools);

                // Active le mode trace ou pas
                if (_pageQueryString.LogDOS != null)
                    eDosProtection.GetInstance().ActiveTraceMode(_pageQueryString.LogDOS.Value);

#if DEBUG
                if (!eDosProtection.GetInstance().DemandConnect(Request))
                {
                    // Veuillez réessayer plus tard.
                    Response.Write(eResApp.GetRes(GetLangServId(), 1773));
                    Response.End();
                }
#else
                if (!eDosProtection.GetInstance().DemandConnect(Request))
                {
                    // Veuillez réessayer plus tard.
                    Response.Write(eResApp.GetRes(GetLangServId(), 1773));
                    Response.End();
                }
#endif

                LoadQueryString();
                if (DataParam == null || DataParam.InvalidQueryString())
                    Response.End();

                #region gestion du tracking externe

                //Traitement spécifique pour les requêtes externalisées
                // dans certains cas, un serveur centralisé peut être amené a prendre en charge des demandes,
                // comme pour le tracking externalisé.
                // ces demandes font donc l'objet d'un traitement spécialisé
                int nTypeExternal = 0;
                string cryptedParamExtTrack = _requestTools.GetRequestQSKeyS("g");
                if (cryptedParamExtTrack != null)
                {
                    try
                    {
                        string sAddedParam = ExternalUrlTools.GetDecrypt(cryptedParamExtTrack);
                        sAddedParam = sAddedParam.TrimEnd(ExternalUrlTools.SEPARATOR.ToArray());

                        dicAddedParam = sAddedParam.Replace("?", "")
                            .Split(ExternalUrlTools.SEPARATOR)
                            .ToDictionary(
                                x => x.Split(ExternalUrlTools.QS_EQUAL)[0],
                                x => x.Split(ExternalUrlTools.QS_EQUAL)[1]);


                        if (dicAddedParam.ContainsKey("tx") && int.TryParse(dicAddedParam["tx"], out nTypeExternal))
                        {
                            switch (nTypeExternal)
                            {
                                case 1:
                                    //Tracking externe - récupération & validation des valeurs
                                    eudoDAL eudodalEUDOTRAIT = null;

                                    try
                                    {
                                        //Ouverture d'une connexion sur eudotrait
                                        eudodalEUDOTRAIT = ePrefTools.GetDefaultEudoDal("EUDOTRAIT");
                                        eudodalEUDOTRAIT.OpenDatabase();
                                        _dalClient = eudodalEUDOTRAIT;

                                        // Pas de gestion de themes On prend les valeurs par defaut
                                        PageRegisters.SetTheme(ePrefLite.Theme.GetDefaultTheme());

                                        ProcessPage();
                                    }
                                    catch (Exception exx)
                                    {
                                        throw exx;
                                    }
                                    finally
                                    {
                                        if (eudodalEUDOTRAIT != null)
                                            eudodalEUDOTRAIT.CloseDatabase();

                                        EndPage();
                                    }
                                    return;

                                default:
                                    throw new NotImplementedException("Type de tracking externalisé" + nTypeExternal + " non implémenté");
                            }
                        }
                    }
                    catch (Exception)
                    {
                        //Log de l'erreur, affichage d'un message pour l'utilisateur
                        // Comme dans ce genre de cas, l'uid n'est pas disponible,
                        // on se contente pour l'instant de loger des informations basiques sur la requetes.

                        //TODO : message ?
                        Response.End();

                    }
                }

                #endregion

                LoadEudoLog dataEudoLog = new LoadEudoLog(Request, _pageQueryString.UID);

                if (dataEudoLog.Error.Length != 0)
                {
                    ControlCollection ctrlParent = GetPageControls();
                    HtmlGenericControl sep = new HtmlGenericControl("BR");
                    string sError = dataEudoLog.Error.Split(":").FirstOrDefault();

                    Image image = new Image();
                    image.ID = "imgBuzzyError";
                    image.ImageUrl = "~/IRISBlack/Front/Scripts/components/IrisPurpleFile/img/Buzzy_Error.png";
                    CreateStyleImage(image.Style);

                    Label lblTitle = CreateLabel("spTitleError", eResApp.GetRes(GetLangServId(), resId: 8419), "");
                    CreateStyleTitle(lblTitle.Style);

                    Label lblText = CreateLabel("spTextError", sError , "");
                    CreateStyleText(lblText.Style);

                    AddControlsToParent(ctrlParent, new List<Control>() { lblTitle, sep, image, sep, lblText });

                    ExtPgTrace(string.Concat("LoadEudoLog >> ", dataEudoLog.Error));

                    Response.Write(RenderHTMLToDisplay(ctrlParent.OfType<Control>()));

                    Response.End();

                }
                else if (dataEudoLog.RedirecUrl)
                {
                    string redirectUrl = GetRedirectUrl(dataEudoLog.AppExternalUrl);
                    ExtPgTrace(string.Concat("DataEudoLog - GetAppUrl != AppExternalUrl - GetAppUrl = ", eLibTools.GetAppUrl(Request)));
                    ExtPgTrace(string.Concat("DataEudoLog RedirecUrl >> ", redirectUrl));
                    Response.Redirect(redirectUrl, true);
                }

                // Objet temporaire pour la description de connexion à SQL
                _prefSqlClient = ePrefTools.GetDefaultPrefSql(dataEudoLog.Directory);
                _dalClient = eLibTools.GetEudoDAL(_prefSqlClient);
                _dalClient.OpenDatabase();

                // Chargement de PrefLite / UserInfo
                LoadInfos(_prefSqlClient);

                // Pas de gestion de themes On prend les valeurs par defaut
                PageRegisters.SetTheme(ePrefLite.Theme.GetDefaultTheme());

                #region on vérifie que la base n'est pas en cours de maintenance

                if (eLibTools.IsUpgradeInProcess(_pref))
                {
                    RendTitleAndErrorMsg(eResApp.GetRes(0, 495), eResApp.GetRes(_pref, 496));
                    throw new eEndResponseException();
                }

                #endregion



                ProcessPage();
            }
            catch (eEndResponseException)
            {
                // Response.End();
            }
            catch (System.Threading.ThreadAbortException)
            {
                //response.end
            }
            catch (ExternalPageExp exp)
            {
                string expMsg = eLibTools.GetExceptionMsg(exp);
                _anError = true;
                if (exp.SendFeedback)
                    _msgFeedback.Add(expMsg);
                ExtPgTrace(expMsg);
            }
            catch (Exception exp)
            {
                string expMsg = eLibTools.GetExceptionMsg(exp, 2, "Erreur non géré. ");
                _anError = true;
                _msgFeedback.Add(expMsg);
                ExtPgTrace(expMsg);
            }
            finally
            {
                if (_dalClient != null)
                    _dalClient.CloseDatabase();

                EndPage();
            }
        }

        /// <summary>
        /// Retourne le type (nom) de la page pour reconstruire l'URL
        /// </summary>
        /// <returns></returns>
        protected virtual ExternalUrlTools.PageName GetRedirectPageName()
        {
            // Par defaut pour tracking et formulaire
            return ExternalUrlTools.PageName.UT;
        }

        /// <summary>
        /// en cas de redirection, retourne l'url de redirection construite
        /// </summary>
        /// <param name="sAppExternalUrl"></param>
        /// <returns></returns>
        private string GetRedirectUrl(string sAppExternalUrl)
        {
            StringBuilder sb = new StringBuilder(
                ExternalUrlTools.GetExternalUrl(
                    sAppExternalUrl,
                    GetRedirectPageName(),
                    _pageQueryString.UID, HttpUtility.UrlEncode(_pageQueryString.Cs), HttpUtility.UrlEncode(_pageQueryString.P)));

            sb.Append(ComplementaryRedirectUrl());

            return sb.ToString();
        }

        /// <summary>
        /// Informations complementaires à l'URL
        /// </summary>
        /// <returns></returns>
        protected string ComplementaryRedirectUrl()
        {
            // On conserve la demande de log
            if (_pageQueryString?.Log ?? false)
                string.Concat("&", ExternalPageQueryString.LOG_KEY, "=1");
            return "";
        }

        /// <summary>
        /// Chargement des informations de contexte (Pref, UserInfos...)
        /// </summary>
        /// <param name="prefSql"></param>
        protected void LoadInfos(ePrefSQL prefSql)
        {
            eudoDAL dal = null;
            string error = string.Empty;

            try
            {
                _pref = eExternal.GetPref(PgTyp, prefSql, _dalClient, out error);
                if (_pref == null)
                    return;

                dal = eLibTools.GetEudoDAL(_pref);
                dal.OpenDatabase();

                // On propage la langue de l'utlisateur pour le serverlang
                int serverLangId;
                if (eLibTools.GetServerLang(dal, _pref.LangId, out serverLangId))
                    _pref.LangServId = serverLangId;


            }
            catch (Exception exp)
            {
                error = exp.Message;
            }
            finally
            {
                dal?.CloseDatabase();

                if (error.Length > 0)
                {
                    switch (PgTyp)
                    {
                        case eExternal.ExternalPageType.TRACKING:
                            throw new TrackExp(error);
                        case eExternal.ExternalPageType.FORMULAR:
                            throw new FormularExp(error);
                        default:
                            throw new Exception(error);
                    }
                }
            }
        }

        /// <summary>
        /// Gestion de l'affichage du message d'erreur à l'utilisateur si une erreur c'est produite
        /// </summary>
        /// <param name="title">facultatif : titre du message</param>
        /// <param name="msg">facultatif : Message</param>
        protected virtual void RendTitleAndErrorMsg()
        {
            int iLangServId = GetLangServId();
            // Une erreur est survenue.
            string title = eResApp.GetRes(iLangServId, 72);
            // Pour améliorer la qualité de l'application, cette erreur a été transmise à notre équipe technique.
            string msg = string.Concat(eResApp.GetRes(iLangServId, 422), Environment.NewLine, eResApp.GetRes(iLangServId, 544));

            RendTitleAndErrorMsg(title, msg);
        }

        /// <summary>
        /// Gestion de l'affichage du message d'erreur à l'utilisateur si une erreur c'est produite
        /// </summary>
        /// <param name="title"> titre du message</param>
        /// <param name="msg"> Message</param>
        protected virtual void RendTitleAndErrorMsg(string title, string msg)
        {
            int iLangServId = GetLangServId();
            if (title.Length == 0 && msg.Length == 0)
            {
                // Une erreur est survenue.
                title = eResApp.GetRes(iLangServId, 72);
                // Pour améliorer la qualité de l'application, cette erreur a été transmise à notre équipe technique.
                msg = string.Concat(eResApp.GetRes(iLangServId, 422), Environment.NewLine, eResApp.GetRes(iLangServId, 544));
            }
            RendType = ExternalPageRendType.ERROR;
            PageTitle = title;
            _panelErrorMsg = msg;
        }


        /// <summary>
        /// Gestion d'erreur en cas d'interruption brusque de la page
        /// </summary>
        public void EndPage()
        {
            // Si on a rencontré une erreur, on affiche l'erreur à l'utilisateur
            if (_anError)
                RendTitleAndErrorMsg();

            try
            {
                int iLangServId = GetLangServId();
                // Une erreur est survenue.
                string title = eResApp.GetRes(iLangServId, 72);
                // Pour améliorer la qualité de l'application, cette erreur a été transmise à notre équipe technique.
                string msg = string.Concat(eResApp.GetRes(iLangServId, 422), Environment.NewLine, eResApp.GetRes(iLangServId, 544));

                foreach (string msgFeedBack in _msgFeedback)
                {
                    eErrorContainer err = new eErrorContainer()
                    {
                        AppendTitle = title,
                        AppendMsg = msg,
                        AppendDebug = msgFeedBack
                    };

                    eFeedbackContext.LaunchFeedbackContext(errCont: err, prefSql: _pref, userInfo: _pref.User);
                }
            }
            catch (Exception exp)
            {
                // On appel directement la méthode trace de eTools pour tracer l'erreur même en mode release
                // Cela permet de tracer l'erreur rencontré lors de l'envoi du ou des feedback
                eModelTools.EudoTraceLog(eLibTools.GetExceptionMsg(exp));
            }
            finally
            {
                if (!_anError)
                {
                    foreach (Action<HttpResponse> cmd in _finishCmd)
                        cmd(Response);
                }
            }
        }

        #region Methodes à implementer

        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public virtual Control GetHeadPlaceHolder()
        {
            return Header;
        }

        /// <summary>
        /// Type d'external page
        /// </summary>
        protected virtual eExternal.ExternalPageType PgTyp { get { throw new NotImplementedException(); } }

        /// <summary>
        /// Chargement de la page
        /// </summary>
        protected virtual void ProcessPage() { }

        /// <summary>
        /// Charge les données du token de la querystring 
        /// </summary>
        /// <returns></returns>
        protected virtual void LoadQueryString() { throw new NotImplementedException(); }

        #endregion

        #region Boîte à outils

        /// <summary>
        /// Récupère la langue d'affichage
        /// </summary>
        /// <returns></returns>
        public int GetLangId()
        {
            return _pref != null ? _pref.LangId : 0;
        }


        /// <summary>
        /// Récupère la langue d'affichage
        /// </summary>
        /// <returns></returns>
        public int GetLangServId()
        {
            return _pref != null ? _pref.LangServId : 0;
        }

        /// <summary>
        /// Ecrit une trace dans les logs si en mode debug ou si la page recoit log=1
        /// </summary>
        /// <param name="msg">message</param>
        public void ExtPgTrace(string msg)
        {
            // On passe vide à chaque param car nous n'avons pas de pref mais nous passons un prefix pour identifier les log des externalpages
            Action fctTrace = () => { eModelTools.EudoTraceLog(msg, prefix: "external_page"); };
#if DEBUG
            fctTrace();
#else
            if (_pageQueryString.Log)
                fctTrace();
#endif
        }

        #endregion

        #region Enums


        /// <summary>
        /// Type de rendu de la page
        /// </summary>
        public enum ExternalPageRendType
        {
            /// <summary>E-mailing/Tracking uniquement : visualisation du mail</summary>
            TRACK_VISU,
            /// <summary>E-mailing/Tracking uniquement : sélection du type de désinscription</summary>
            TRACK_UNSUB_CHOICE,
            /// <summary>E-mailing/Tracking uniquement : valide la désinscription auprès de l'utilisateur</summary>
            TRACK_UNSUB_VALID,
            /// <summary>Générateur de formulaires : visualisation</summary>
            FORM_VISU,
            /// <summary>COMMUN : Message d'erreur</summary>
            ERROR
        }

        #endregion
    }
}