using Com.Eudonet.Internal;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Xml;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm {
    /// <summary>
    /// Description résumée de eBaseEudoManager
    /// </summary>
    public abstract class eBaseEudoManager : IStepTimer, iRendererXMLHTML, IHttpHandler {



        /// <summary>
        /// Liste du temps consommé par étape
        /// </summary>
        protected double _timeTaken = 0;

        /// <summary>
        /// Document XML de résultat de la page
        /// </summary>
        protected XmlDocument _xmlResult;

        /// <summary>Préférences utilisateur de la session</summary>
        protected ePref _pref;

        /// <summary>Utilitaire du request</summary>
        protected eRequestTools _requestTools;

        /// <summary>Toutes les clé de request.form</summary>
        [Obsolete("use _requestTools.AllKeys")]
        protected HashSet<string> _allKeys;

        /// <summary>
        /// toues les clés de la querystring
        /// </summary>
        [Obsolete("use _requestTools.AllKeysQS")]
        protected HashSet<string> _allKeysQS;

        /// <summary>
        /// Erreur rencontrée
        /// </summary>
        protected Exception _eInnerException = null;

        /// <summary>
        /// Message d'erreur
        /// </summary>
        protected string _sMsgError = "";


        /* timer de génération */

        /// <summary>
        /// TimeSpan de la durée de génération
        /// </summary>
        protected TimeSpan ts = new TimeSpan();


        /// <summary>
        /// Date de début de génération
        /// </summary>
        protected DateTime dtStart;

        /// <summary>
        /// Date de fin de génération
        /// </summary>
        protected DateTime dtEnd;


        /// <summary>
        /// objet de gestion de rendu
        /// </summary>
        protected eRendererXMLHTML _renderXMLHTML;

        // Les variables suivantes ne sont pas utilisée au sein de la classe maitre, elles doivent être redéfini dans les filles
        private eError _error = null;

        /// <summary>
        /// Gestionnaire d'erreur
        /// </summary>
        public eError EudoError {
            get { return _error; }
            set { _error = value; }
        }

        /// <summary>
        /// Accesseur directeur au conteur d'erreur
        /// Si une erreur est set, l'appel a rendererror est lancé
        /// </summary>
        protected eErrorContainer ErrorContainer {
            get { return EudoError.Container; }
            set { EudoError.Container = value; }
        }

        /// <summary>Description de l'erreur à l'utilisateur</summary>
        [Obsolete("use => ErrorContainer")]
        protected string _errorMsg = string.Empty;

        /// <summary>Context de la page</summary>
        protected HttpContext _context = null;


        /// <summary>Si à true rajoute le doctype et les balises HTML, HEAD et BODY dans le rendu</summary>
        public Boolean AddHeadAndBody { get; set; }

        /// <summary>Classe à ajouter à l'élément BODY </summary>
        public string BodyCssClass { get; set; }

        /// <summary>méthode javascript à éxecuter lors du chargement </summary>
        public string OnLoadBody { get; set; }


        /// <summary>Gestion des inscriptions de CSS et/ou de SCRIPT</summary>
        private eWebUiPageRegisters _pageRegisters;

        /// <summary>Gestion des inscriptions de CSS et/ou de SCRIPT</summary>
        protected eWebUiPageRegisters PageRegisters {
            get {
                if (_pageRegisters == null) {
                    _ctrlHead = new System.Web.UI.WebControls.PlaceHolder();
                    _pageRegisters = new eWebUiPageRegisters(_context != null ? _context.Server : null, _ctrlHead);
                    _pageRegisters.SetTheme(_pref.ThemeXRM);
                }
                return _pageRegisters;
            }
        }

        private Control _ctrlHead { get; set; }








        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public virtual void ProcessRequest(HttpContext context) {
            try {

                //traitement de la demande
                dtStart = DateTime.Now;

                //Initialisation des objets de gestions d'erreur 
                _error = eError.getError();
                _renderXMLHTML = eRendererXMLHTML.GetRenderXMLHTML(_error);

                //Affectation du contexte pour avoir un acces direct
                _context = context;


                //Charge la session
                LoadSession();


                //Initialise l'objet d'erreur avec les pref courantes
                _error.SetPref(_pref);

                //Par défaut on ajoute pas les balises HEAD, BODY...
                AddHeadAndBody = false;

                ProcessManager();
            }
            catch (eEndResponseException) { }
            catch (ThreadAbortException) { }    // Laisse passer le response.end du RenderResult}
            catch (eFileLayout.eFileLayoutException e) {
                eFeedbackXrm.LaunchFeedbackXrm(e.ErrorContainer, _pref);
            }
            catch (SpecialTestException) {
                throw;
            }
            catch (EudoException ee) {
                ErrorContainer = eErrorContainer.GetErrorContainerFromEudoException(eLibConst.MSG_TYPE.CRITICAL, ee);

                try { LaunchError(); } catch (eEndResponseException) { }
            }
            catch (Exception genEx) {

                string sDevMsg = string.Concat("Erreur sur la page : ", System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine);


                sDevMsg = string.Concat(sDevMsg, Environment.NewLine, "Exception Message : ", genEx.Message, Environment.NewLine, "Exception StackTrace :", genEx.StackTrace);

                Int32 iLang = 0;
                if (_pref != null)
                    iLang = _pref.LangId;

                try {
                    ErrorContainer = eErrorContainer.GetDevUserError(
                       eLibConst.MSG_TYPE.CRITICAL,
                       eResApp.GetRes(iLang, 72),   // Message En-tête : Une erreur est survenue
                       string.Concat(eResApp.GetRes(iLang, 422), "<br>", eResApp.GetRes(iLang, 544)),  //  Détail : pour améliorer...
                       eResApp.GetRes(iLang, 72),  //   titre
                       string.Concat(sDevMsg));

                    LaunchError();
                }
                catch (eEndResponseException) {

                }
            }
            finally {
                // On informe le cache d'une action en administration
                if (_pref?.AdminMode ?? false)
                    StaticBaseUseCache.BaseUseCache.ActionAdmin(_pref.GetBaseName);
            }
        }

        /// <summary>
        /// Chargement de la page
        /// </summary>
        protected abstract void ProcessManager();

        /// <summary>
        /// Methodes system
        /// </summary>
        public bool IsReusable {
            get {
                return false;
            }
        }

        #region GetRequestFormKey
        /*
        /// <summary>
        /// Retourne la valeur venant de la request form
        /// </summary>
        /// <param name="key">id de la valeur à recup du form</param>
        /// <param name="value">valeur recupéré par la fonction</param>
        /// <returns>vrai si la valeur est trouvé</returns>
        protected Boolean GetRequestFormKey(string key, out string value)
        {
            value = string.Empty;

            if (!_allKeys.Contains(key))
                return false;

            value = _context.Request.Form[key];
            return true;
        }

        /// <summary>
        /// Retourne la valeur venant de la request form
        /// </summary>
        /// <param name="key">id de la valeur à recup du form</param>
        /// <param name="value">valeur recupéré par la fonction</param>
        /// <returns>vrai si la valeur est trouvé et est valid</returns>
        protected Boolean GetRequestFormKey(string key, out Int32 value)
        {
            value = 0;

            if (!_allKeys.Contains(key) || _context.Request.Form[key].Length == 0)
                return false;

            return Int32.TryParse(_context.Request.Form[key], out value);
        }

        /// <summary>
        /// Retourne la valeur venant de la request form
        /// </summary>
        /// <param name="key">id de la valeur à recup du form</param>
        /// <param name="value">valeur recupéré par la fonction</param>
        /// <returns>vrai si la valeur est trouvé et est valid</returns>
        protected Boolean GetRequestFormKey(string key, out Boolean value)
        {
            value = false;

            if (!_allKeys.Contains(key) || _context.Request.Form[key].Length == 0)
                return false;

            value = _context.Request.Form[key] == "1";

            return true;
        }

        /// <summary>
        /// Retourne la valeur venant de la request form
        /// </summary>
        /// <param name="key">id de la valeur à recup du form</param>
        /// <returns>valeur recupéré par la fonction</returns>
        protected string GetRequestFormKeyS(string key)
        {
            string val;
            GetRequestFormKey(key, out val);
            return val;
        }

        /// <summary>
        /// Retourne la valeur venant de la request form
        /// </summary>
        /// <param name="key">id de la valeur à recup du form</param>
        /// <returns>valeur recupéré par la fonction</returns>
        protected Int32 GetRequestFormKeyI(string key)
        {
            Int32 val;
            GetRequestFormKey(key, out val);
            return val;
        }

        /// <summary>
        /// Retourne la valeur venant de la request form
        /// </summary>
        /// <param name="key">id de la valeur à recup du form</param>
        /// <returns>valeur recupéré par la fonction</returns>
        protected Boolean GetRequestFormKeyB(string key)
        {
            Boolean val;
            GetRequestFormKey(key, out val);
            return val;
        }
        */
        #endregion

        #region Rendu de retour




        /// <summary>
        /// Retourne le résultat sous forme de HTML a partir d'un control Web.UI
        /// </summary>
        /// <typeparam name="T">Objet venant de la classe System.Web.UI.Control</typeparam>
        /// <param name="monPanel">Control représentant le rendu à retourner</param>
        /// <param name="bStripTop">Indique si le conteneur parent (monPanel doit être rendu</param>
        public void RenderResultHTML<T>(T monPanel, Boolean bStripTop = false) where T : Control {
            RenderResult(RequestContentType.HTML, delegate () { return GetResultHTML(monPanel, bStripTop); });
        }


        public string GetResultHTML<T>(T monPanel, Boolean bStripTop = false) where T : Control {
            IEnumerable<Control> lstControl;
            StringBuilder sb = null;
            lstControl = (!bStripTop) ? new HashSet<Control> { monPanel } : monPanel.Controls.Cast<Control>();

            if (AddHeadAndBody) {
                //AJOUT DES BALISES HTML, HEAD, BODY et du doctype
                HtmlGenericControl ctrlHtml = new HtmlGenericControl("HTML");
                HtmlGenericControl ctrlHead = new HtmlGenericControl("HEAD");

                if (_ctrlHead != null && _pageRegisters != null) {
                    PageRegisters.RegisterIncludeScript();
                    PageRegisters.RegisterScript();
                    PageRegisters.RegisterCSS();
                }
                if (_ctrlHead != null)
                    ctrlHead.Controls.Add(_ctrlHead);
                ctrlHtml.Controls.Add(ctrlHead);
                HtmlGenericControl ctrlBody = new HtmlGenericControl("BODY");
                // 41590 CRU : Ajout d'une classe sur Body pour cibler en CSS
                if (!string.IsNullOrEmpty(BodyCssClass)) {
                    ctrlBody.Attributes.Add("class", BodyCssClass);
                }
                if (!string.IsNullOrEmpty(OnLoadBody)) {
                    ctrlBody.Attributes.Add("onload", OnLoadBody);
                }
                ctrlHtml.Controls.Add(ctrlBody);
                //---Ajout du contenu de la page
                foreach (Control c in lstControl)
                    ctrlBody.Controls.Add(c);
                //---
                sb = new StringBuilder()
                    .Append("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 //EN\">")
                    .Append(GetControlRender((Control)ctrlHtml)); //Récupération du contenu de la page au format Chaine
            }
            else {
                sb = GetControlRender(lstControl);  //Récupération du contenu de la page au format Chaine
            }

            return sb.ToString();
        }




        /// <summary>
        /// Transformation d'une liste de controls en chaine de caractère html
        /// </summary>
        /// <param name="ctrToConvert">controls à convertir</param>
        /// <returns>rendu html</returns>
        private static StringBuilder GetControlRender(IEnumerable<Control> listCtrToConvert) {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            HtmlTextWriter tw = new HtmlTextWriter(sw);
            foreach (Control ctr in listCtrToConvert) {
                ctr.RenderControl(tw);
            }
            return sb;
        }


        /// <summary>
        /// Transformation d'un control en chaine de caractère html
        /// </summary>
        /// <param name="ctrToConvert">control à convertir</param>
        /// <returns>rendu html</returns>
        private static StringBuilder GetControlRender(Control ctrToConvert) {
            return GetControlRender(new HashSet<Control> { ctrToConvert });
        }



        #endregion

        #region INTERFACE PUBLIC

        /// <summary>
        /// Gestion d'erreur depuis les eUpdater.JS
        /// </summary>
        /// <param name="rqCt">ContentType de l'erreur - Si non défini XML</param>
        /// <param name="err">Error container - Si non défini, utilise celui défini précédement </param>
        public void LaunchError(eErrorContainer err = null, RequestContentType rqCt = RequestContentType.XML) {
            LogResult(err);

            _renderXMLHTML.LaunchError(err, rqCt);
        }

        /// <summary>
        /// Retourne le flux de retour
        /// </summary>
        /// <param name="contentTyp">Type de retour HTML, Text, XML, etc.</param>
        /// <param name="func">Fonction retournant le contenu en string du rendu à retourner</param>
        public void RenderResult(RequestContentType contentTyp, Func<string> func) {

            LogResult();

            HttpContext.Current.Response.Headers.Add("X-time", (DateTime.Now - dtStart).TotalMilliseconds.ToString());


            _renderXMLHTML.RenderResult(contentTyp, func);



        }

        /// <summary>
        /// Lance un rendu HTML de l'erreur
        /// Il s'agit d'un appel js a eAlert.
        /// </summary>
        /// <param name="bWithHeader">Indique si les entete html complet doivent être générés</param>
        /// <param name="err">erreurContainer. Par défaut, utilise celui du eError de la page</param>
        /// <param name="sCallBack">Callback sur erreur</param>
        public void LaunchErrorHTML(bool bWithHeader, eErrorContainer err = null, string sCallBack = "") {
            _renderXMLHTML.LaunchErrorHTML(bWithHeader, err, sCallBack);
        }

        #endregion

        /// <summary>
        /// Log des informations sur les temps de réponses
        /// </summary>
        /// <param name="err">erreur éventuelle</param>
        protected virtual void LogResult(eErrorContainer err = null) {
            _timeTaken = (DateTime.Now - dtStart).TotalMilliseconds;


#if DEBUG
            if (LstTimedSteps.Count > 0 && !HttpContext.Current.Response.Headers.AllKeys.Contains("X-STEP")) {

                LstTimedSteps.Insert(0, eTimedStep.GetSep("TOTAL TIME", _timeTaken));

                JsonSerializerSettings serializer = new JsonSerializerSettings {
                    NullValueHandling = NullValueHandling.Ignore,
                    FloatParseHandling = FloatParseHandling.Decimal,
                };

                string sRes = JsonConvert.SerializeObject(LstTimedSteps, Newtonsoft.Json.Formatting.None, serializer);

                HttpContext.Current.Response.Headers.Add("X-STEP", sRes);
            }
#endif
        }

        /// <summary>
        /// Charge les variables de session et le dico de request.form
        /// </summary>
        protected virtual void LoadSession() {
            #region Variables de session

            if (_context.Session["Pref"] == null) {
                //Perte de Session             

                //503; // votre session a expiré...
                //6068; // votre session a expiré...détail
                int iLng = EudoCommonHelper.EudoHelpers.GetUserIdLangFromCookie();
                this.ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.EXCLAMATION, eResApp.GetRes(iLng, 503), eResApp.GetRes(iLng, 6068));
                this.ErrorContainer.IsSessionLost = true;
                LaunchError();
            }

            try {
                _pref = (ePref)_context.Session["Pref"];
                _pref.ResetTranDal();

            }
            catch {
                //Perte de Session
                //503; // votre session a expiré...
                //6068; // votre session a expiré...détail
                int iLng = EudoCommonHelper.EudoHelpers.GetUserIdLangFromCookie();
                this.ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.EXCLAMATION, eResApp.GetRes(iLng, 503), eResApp.GetRes(iLng, 6068));
                this.ErrorContainer.IsSessionLost = true;
                LaunchError();
            }

            //UserId = 0
            if (_pref.User.UserId == 0) {
                // Userid invalide         
                int iLng = EudoCommonHelper.EudoHelpers.GetUserIdLangFromCookie();
                this.ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.EXCLAMATION, eResApp.GetRes(iLng, 503), eResApp.GetRes(iLng, 6068));
                this.ErrorContainer.IsSessionLost = true;
                LaunchError();
            }

            //Déjà loggé
            string strRemoteAdr = _context.Request.ServerVariables["remote_addr"];
            if (!_pref.CheckSimultLog(_context.Session.SessionID, strRemoteAdr)) {
                //Déjà Loggé
                this.ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, string.Concat(eResApp.GetRes(_pref, 503) + " - " + eResApp.GetRes(_pref, 504)), "");
                this.ErrorContainer.IsSessionLost = true;
                LaunchError();
            }

            // Charge les valeurs de request
            _allKeys = new HashSet<string>(_context.Request.Form.AllKeys, StringComparer.OrdinalIgnoreCase);
            _allKeysQS = new HashSet<string>(_context.Request.QueryString.AllKeys, StringComparer.OrdinalIgnoreCase);

            _requestTools = new eRequestTools(_context);

            // INPROGRESS protection anti XSRF (cross site request forgery)
            // a gerer/tester les formulaires, les liens de tracking,... avant d'activer            


            if (_context.Request.HttpMethod.ToUpper() == "POST") {
                if (_context.Request.Headers.HasKeys() && _context.Request.Headers["X-FROM-EUPDATER"] == "1") // test à retirer une fois assurer que tous les acceès manager passer le processid
                {
                    // TODO : Il y a des formulaire en POST/redirect... qui ne passe pas de processid, il faudrait le faire puis retirer le test de header
                    //  il y a au moins  -> etargetprocessmanager,eloginmgr,,evcardmgr...

                    if (_requestTools.GetRequestFormKeyS("_processid") != _context.Session["_uidupdater"]?.ToString()) {

                        // 
                        //  this.ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.EXCLAMATION, "Utilisation invalide", "");
                        //  LaunchError();
                    }
                }
				//NHA : COrrection bug 73329
				//[SAFARI 12.1.1]-- Import Impossible : Blocage au niveau l'étape 1 sur  Import 
                else if (_context.Request.UrlReferrer !=null && _context.Request.UrlReferrer.Host != _context.Request.Url.Host) {
                    // on ne peut pas se baser uniquement sur le host (cas des redirection de serveur), il faudrait se baser sur le *.eudonet.com 
                    //  this.ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.EXCLAMATION, "Utilisation invalide", "");
                    //  LaunchError();
                }
            }
            else if (_context.Request.HttpMethod.ToUpper() == "GET") {

                // TODO : rechercher les GET pour les gérer autrement ( au moin echartmanager et eresmanager)
                // a noter que pour echartmanager, la fusion de fucsion chart utilisé (3.3.1-) n'encode pas correctement les certains caractère des paramètres de l'url (les + par exemple), ce qui pose pb pour récupérer les jetons
                // meme en 3.10 il faut encodeURI les param pouvant contenir des caract_res spéciaux puis escape l'url complete
                if (_context.Request.QueryString["_processid"] != _context.Session["_uidupdater"]?.ToString()) {

                    // 
                    //  this.ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.EXCLAMATION, "Utilisation invalide", "");
                    //  LaunchError();
                }
            }


            if (_pref.User.UserLevel < (int)UserLevel.LEV_USR_ADMIN)
                _pref.AdminMode = false;


            if (_pref.User.UserLevel < (int)UserLevel.LEV_USR_SUPERADMIN && eLibTools.IsUpgradeInProcess(_pref)) {
                ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.EXCLAMATION, eResApp.GetRes(_pref, 494), " ", title: eResApp.GetRes(0, 495));

                ErrorContainer.IsSessionLost = true;

                // Arrete le traitement et envoi l'erreur
                LaunchError();
            }


            #endregion
        }


        #region Implémentation de IStepTimer
        /// <summary>
        /// Pour debug - timer pour chronométrer les temps intermédiares des étapes du manager
        /// attention stopwatch n'est pas "précis"
        /// </summary>
        private eStepTimer _eStep = EudoQuery.eStepTimer.GetStepTime();


        /// <summary>
        /// liste des étapes chronométré
        /// </summary>
        /// <returns></returns>
        public List<eTimedStep> LstTimedSteps {
            get {
                return _eStep.LstTimedSteps;
            }
        }


        /// <summary>
        /// démare un timer d'étape
        /// </summary>
        /// <param name="sStepName">Nom de l'étape</param>
        /// <param name="sMethName">Nom de la méthode</param>     
        /// <param name="sClassPath">Chemin du fichier contenant l'appel</param>
        /// <param name="nLine">Ligne de l'appel</param>
        /// 
        public void StartTimerStep(string sStepName,
            [System.Runtime.CompilerServices.CallerMemberName]string sMethName = "",
            [System.Runtime.CompilerServices.CallerFilePath]string sClassPath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int nLine = 0
            ) {
            _eStep.StartTimerStep(sStepName, sMethName, sClassPath, nLine);
        }

        /// <summary>
        /// stop le timer d'étape
        /// </summary>
        public void StopTimerStep() {
            _eStep.StopTimerStep();
        }



        /// <summary>
        /// Ajoute une liste d'étape interne "encadrée" sur le timer en cours.
        /// En créé un s'il n'y en a pas
        /// Réinitialise la liste des étapes "interne" après
        /// </summary>
        ///<param name="bStopTimer">Indique si le timer en cours doit être stopé </param>
        /// <param name="step">IStep interne a "intégrer"</param>
        /// <param name="sInnerStepName">Nom de l'étape interne</param>
        /// <param name="sMethName">Nom de la méthode(auto-généré)</param>     
        /// <param name="sClassPath">Chemin du fichier contenant l'appel</param>        
        public void AddTimerInnerRange(IStepTimer step, string sInnerStepName = "------------",
            bool bStopTimer = true,
            [System.Runtime.CompilerServices.CallerMemberName]string sMethName = "",
            [System.Runtime.CompilerServices.CallerFilePath]string sClassPath = "") {
            try {
                _eStep.AddTimerInnerRange(step, sInnerStepName, bStopTimer, sMethName, sClassPath);
            }
            finally { }
        }


        #endregion

    }

    /// <summary>
    /// Classe de représentation de  retour au format JSON
    /// </summary>
    public class JSONReturnHTMLContent : JSONReturnGeneric {
        /// <summary>
        /// Remplacement global d'un Panel
        /// </summary>
        public bool Full = false;

        /// <summary>
        /// HTML de retour (cas mono bloc)
        /// </summary>
        public string Html = string.Empty;

        /// <summary>
        /// Javascript de retour
        /// </summary>
        public string CallBack = string.Empty;


        /// <summary>
        /// Contnu multi part
        /// </summary>
        public List<PartContent> MultiPartContent;


        /// <summary>
        /// Constructeur pour l'objet json de retour pour de l'HTML
        /// Initialise la liste
        /// </summary>
        public JSONReturnHTMLContent() {
            MultiPartContent = new List<PartContent>();
        }
    }


    /// <summary>
    /// Content sous forme de Json , à utiliser en admin => chargement des modules
    /// </summary>
    public class ModuleJSONReturnHTMLContent : JSONReturnHTMLContent {

        /// <summary>
        /// Le nombre global des modules disponibles dans le Store
        /// </summary>
        public int iCountModule;

        /// <summary>
        /// Le nombre total de pages
        /// </summary>
        public int iPagesModules;
    }

    /// <summary>
    /// Contenu d'un bloc de retour JSON
    /// </summary>
    public class PartContent {
        /// <summary>
        /// Nom du bloc
        /// </summary>
        public string Name = "";

        /// <summary>
        /// Id du bloc
        /// </summary>
        public string ID = "";


        /// <summary>
        /// Contenu du bloc
        /// </summary>
        public string Content = "";

        /// <summary>
        /// Javascript de retour a executer
        /// </summary>
        public string CallBack = "";

        /// <summary>
        /// Permet d'indiqué si on souhaite remplacer le contenu total ou si on souhait l'enrichir. Par defaut, remplacement du contenu
        /// </summary>
        public int Mode;

    }
}