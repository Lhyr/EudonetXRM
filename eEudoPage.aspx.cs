using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using Com.Eudonet.Core.Model;
using System.Linq;
using System.Text;

namespace Com.Eudonet.Xrm
{
    /// <className>eEudoPage</className>
    /// <summary>Classe parente des pages Eudo type ASPX</summary>
    /// <purpose>Permet la centralisation de la gestion d'erreur des page ASPX</purpose>
    /// <authors>SPH</authors>
    /// <date>2013-02-18</date>
    public partial class eEudoPage : System.Web.UI.Page, iRendererXMLHTML
    {
        /// <summary>Préférence d'application de l'utilisateur</summary>
        public ePref _pref = null;
        /// <summary>Gestion des inscriptions de CSS et/ou de SCRIPT</summary>
        protected eWebUiPageRegisters PageRegisters { get; private set; }

        /// <summary>
        /// Message d'erreur à afficher en cas de Theme incompatible
        /// </summary>
        protected StringBuilder sbMgErrorNav { get; set; } = new StringBuilder();


        protected string sMsgErrorCourtNav = string.Empty;

        /// <summary>
        /// Booleen pour le theme incompatible.
        /// </summary>
        protected bool bThemeIncompat { get; set; } = false;

        /// <summary>
        /// date de début de génération
        /// </summary>
        private DateTime _dtStart = new DateTime();

        /// <summary>Utilitaire du request</summary>
        protected eRequestTools _requestTools;

        /// <summary>
        /// HashSet de toutes les clés de Request.Form
        /// </summary>
        [Obsolete("use _requestTools.AllKeys")]
        protected HashSet<string> _allKeys;

        /// <summary>
        /// HashSet de toutes les clés de Request.QueryString
        /// </summary>
        [Obsolete("use _requestTools.AllKeysQS")]
        protected HashSet<string> _allKeysQS;

        private eError _error = null;

        /// <summary>
        /// Gestionnaire d'erreur
        /// </summary>
        public eError EudoError
        {
            get { return _error; }
            set { _error = value; }
        }

        /// <summary>
        /// Indique si la page a été appellée depuis un eUpdater
        /// </summary>
        protected Boolean _bFromeUpdater = false;

        private eRendererXMLHTML _renderXMLHTML = null;

        /// <summary>Accesseur directeur au conteur d'erreur
        /// Si une erreur est set, l'appel a rendererror est lancé
        /// 
        /// </summary>
        protected eErrorContainer ErrorContainer
        {
            get
            {
                return EudoError.Container;
            }

            set
            {
                EudoError.Container = value;
            }
        }

        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public virtual Control GetHeadPlaceHolder()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Permet d'appeler le Page_Load de base en plus (et avant) de celui de la méthode fille
        /// </summary>
        public eEudoPage()
        {
            this.Load += new EventHandler(this.PageLoad);
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
        /// Action a mener dans le prerenderer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void PagePrerender(object sender, EventArgs e)
        {
            if (Page != null && GetHeadPlaceHolder() != null)
            {
                PageRegisters.RegisterIncludeScript();
                PageRegisters.RegisterScript();
                PageRegisters.RegisterCSS();

                if (_pref.User.UserLevel >= UserLevel.LEV_USR_ADMIN.GetHashCode())
                {
                    PageRegisters.RegisterAdminIncludeScript();

                }
            }

        }

        /// <summary>
        /// Initialise la gesetion d'erreur 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void PageLoad(object sender, EventArgs e)
        {
            try
            {
                _dtStart = DateTime.Now;
                _error = eError.getError();
                _renderXMLHTML = eRendererXMLHTML.GetRenderXMLHTML(_error);

                _bFromeUpdater = (Request != null && Request.Headers.HasKeys() && Request.Headers["X-FROM-EUPDATER"] == "1");

                if (!string.IsNullOrEmpty(Request.UserAgent))
                {
                    string s = Request.Browser.ToString();
                }

                #region Recupération des variables de sessions

                if (Session["Pref"] == null)
                {
                    int iLng =  EudoCommonHelper.EudoHelpers.GetUserIdLangFromCookie();

                    //  oErrorObj.Title = top._res_503; // votre session a expiré...
                    //                oErrorObj.Msg = top._res_6068; // votre session a expiré...détail
                    ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.EXCLAMATION, eResApp.GetRes(iLng, 503), eResApp.GetRes(iLng, 6068));

                    ErrorContainer.IsSessionLost = true;

                    // Arrete le traitement et envoi l'erreur
                    if (_bFromeUpdater)
                        LaunchError();
                    else
                        LaunchErrorHTML(true);
                }

                try
                {
                    _pref = (ePref)Session["Pref"];
                    _pref.ResetTranDal();

                    if (_pref.User.UserLevel < (int)UserLevel.LEV_USR_ADMIN)
                        _pref.AdminMode = false;

                    if (_pref.User.UserLevel < (int)UserLevel.LEV_USR_SUPERADMIN && eLibTools.IsUpgradeInProcess(_pref))
                    {

                        ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.EXCLAMATION, eResApp.GetRes(_pref, 494), " ", title: eResApp.GetRes(0, 495));

                        ErrorContainer.IsSessionLost = true;

                        // Arrete le traitement et envoi l'erreur
                        if (_bFromeUpdater)
                            LaunchError();
                        else
                            LaunchErrorHTML(true);
                    }



                }
                catch
                {
                    //  oErrorObj.Title = top._res_503; // votre session a expiré...
                    //                oErrorObj.Msg = top._res_6068; // votre session a expiré...détail
                    int iLng = EudoCommonHelper.EudoHelpers.GetUserIdLangFromCookie();
                    ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.EXCLAMATION, eResApp.GetRes(iLng, 503), eResApp.GetRes(iLng, 6068));

                    ErrorContainer.IsSessionLost = true;

                    // Arrete le traitement et envoi l'erreur
                    if (_bFromeUpdater)
                        LaunchError();
                    else
                        LaunchErrorHTML(true);
                }


                if (_pref.User.UserId == 0)
                {

                    int iLng = EudoCommonHelper.EudoHelpers.GetUserIdLangFromCookie();
                    ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.EXCLAMATION, eResApp.GetRes(iLng, 503), eResApp.GetRes(iLng, 6068));
                    ErrorContainer.IsSessionLost = true;

                    // Arrete le traitement et envoi l'erreur
                    if (_bFromeUpdater)
                        LaunchError();
                    else
                        LaunchErrorHTML(true);
                }

                //Set les pref de l'erreur
                EudoError.SetPref(_pref);
                //Set le theme du register
                bThemeIncompat = _pref.ThemeXRM.IsBrowserIncompatible(HttpContext.Current.Request.Browser.Type.ToLower(), HttpContext.Current.Request.UserAgent.ToLower());

                if (bThemeIncompat)
                {
                    //Vous avez été basculé sur un autre thème,  
                    //Votre navigateur n'est pas compatible avec le nouveau thème
                    sMsgErrorCourtNav = eResApp.GetRes(_pref, 2370); //+ _pref.ThemeXRM.Name;


                    //Pour cette session, vous avez été basculé sur un thème compatible avec votre navigateur.
                    //Vous pouvez utiliser les navigateurs Chrome, Firefox ou Safari pour avoir accès au nouveau thème.
                    StringBuilder sbNav = new StringBuilder();
                    for (int i = 0; i < _pref.ThemeXRM.Navigateurs?.Count; i++)
                    {
                        if (i == _pref.ThemeXRM.Navigateurs.Count - 1)
                            sbNav.Append(" ou ");
                        else if (i > 0)
                            sbNav.Append(", ");

                        sbNav.Append(_pref.ThemeXRM.Navigateurs[i]);
                    }

                    string sMgErrorNav = String.Format(eResApp.GetRes(_pref, 2369), sbNav);


                    //remplacement par le tème par défaut et ajustement de la police
                    List<int> lstIFont = _pref.ThemeXRM.FontSizeMax;
                    _pref.ReloadTheme(_pref.ThemeXRM.DefautThemeID);
                    _pref.ThemeXRM.FontSizeMax = lstIFont;

                    //vous avez été basculé sur le thème {0} pour cette session.
                    sbMgErrorNav.AppendFormat(eResApp.GetRes(_pref, 2368), _pref.ThemeXRM.Name).Append("<br/>").Append(sMgErrorNav);



                }

                PageRegisters.SetTheme(_pref.ThemeXRM);

                // TODO: comportement spécifique en cas de plusieurs connexions avec
                // le même utilisateur.
                // Il faudrait changer le message (existe dans les ressources)
                // string.Concat(eResApp.GetRes(pref.Lang, 503) + " - " + eResApp.GetRes(pref.Lang, 504));
                string strRemoteAdr = Request.ServerVariables["remote_addr"];
                if (!_pref.CheckSimultLog(Session.SessionID, strRemoteAdr))
                {
                    ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, "Session identique", "Session identique", "Session identique");
                    ErrorContainer.IsSessionLost = true;

                    // Arrete le traitement et envoi l'erreur
                    if (_bFromeUpdater)
                        LaunchError();
                    else
                        LaunchErrorHTML(true);
                }

                // Charge les valeurs de request
                _allKeys = new HashSet<string>(Request.Form.AllKeys, StringComparer.OrdinalIgnoreCase);
                _allKeysQS = new HashSet<string>(Request.QueryString.AllKeys, StringComparer.OrdinalIgnoreCase);

                _requestTools = new eRequestTools(this.Context);

                #endregion
            }

            catch (eFileLayout.eFileLayoutException flex)
            {
                eFeedbackXrm.LaunchFeedbackXrm(flex.ErrorContainer, _pref);
            }
            catch (eEndResponseException)
            {

                Response.End();
            }
            finally
            {
                // On informe le cache d'une action en administration
                if (_pref?.AdminMode ?? false)
                    StaticBaseUseCache.BaseUseCache.ActionAdmin(_pref.GetBaseName);
            }
        }

        #region iRendererXMLHTML Membres

        /// <summary>
        /// Lance un rendu HTML de l'erreur
        /// Il s'agit d'un appel js a eAlert.
        /// </summary>
        /// <param name="bWithHeader">Indique si les entete html complet doivent être générés</param>
        /// <param name="err">erreurContainer. Par défaut, utilise celui du eError de la page</param>
        /// <param name="sCallBack">JS de callback</param>
        public void LaunchErrorHTML(bool bWithHeader, eErrorContainer err = null, string sCallBack = "")
        {
            _renderXMLHTML.LaunchErrorHTML(bWithHeader, err, sCallBack);
        }

        /// <summary>
        /// Méthode pour les rendus pour eEudoPage lorsque le rendu "standard" aspx n'est pas utilisé
        /// </summary>
        /// <param name="contentTyp">Content type de la réponse</param>
        /// <param name="func">Fonction générant la string de réposne</param>
        public void RenderResult(RequestContentType contentTyp, Func<string> func)
        {
            HttpContext.Current.Response.Headers.Add("X-time", (DateTime.Now - _dtStart).TotalMilliseconds.ToString());
            _renderXMLHTML.RenderResult(contentTyp, func);

        }

        /// <summary>
        /// Retourne le résultat sous forme de HTML a partir d'un control Web.UI
        /// </summary>
        /// <typeparam name="T">Objet venant de la classe System.Web.UI.Control</typeparam>
        /// <param name="monPanel">Control représentant le rendu à retourner</param>
        /// <param name="bStripTop">Indique si le conteneur parent (monPanel doit être rendu</param>
        public void RenderResultHTML<T>(T monPanel, Boolean bStripTop = false) where T : Control
        {
            _renderXMLHTML.RenderResultHTML<T>(monPanel, bStripTop);
        }

        /// <summary>
        /// Gestion d'erreur depuis les eUpdater.JS
        /// Lance l'erreur - Arrête le traitement - 
        /// </summary>
        /// <param name="rqCt">ContentType de l'erreur - Si non défini XML</param>
        /// <param name="err">Error container - Si non défini, utilise celui défini précédement </param>
        public void LaunchError(eErrorContainer err = null, RequestContentType rqCt = RequestContentType.XML)
        {
            _renderXMLHTML.LaunchError(err, rqCt);

        }

        #endregion
    }
}