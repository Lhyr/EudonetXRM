using Com.Eudonet.Common.Cryptography;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Engine.ORM;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.UI;

namespace Com.Eudonet.Xrm
{

    /// <summary>
    /// Page principale de XRM
    /// C'est cette page qui contient tout le reste de l'appli
    /// </summary>
    public partial class eMain : eEudoPage
    {
#if DEBUG
        public const string DEBUG = "1";
#else
        public const string DEBUG = "0";
#endif


        /// <summary>ressources utilisées dans la page</summary>
        public eRes res;

        /// <summary>Ressources utilisées par le code javascript</summary>
        public string resAppJS;

        /// <summary>Modale à propos de (numéro de version</summary>
        public string sJsAbout = string.Empty;

        /// <summary>Titre de la page ASPX</summary>
        public string Titre = string.Empty;

        /// <summary>Logo de la base (paramétré en V7) </summary>
        public string LogoName = string.Empty;

        public Boolean bDisplayCarto = false;

        public string LogoWebPath
        {
            get { return string.Concat(eLibTools.GetWebDatasPath(eLibConst.FOLDER_TYPE.CUSTOM, _pref.GetBaseName), @"/", LogoName); }
        }

        /// <summary>
        /// ID du theme
        /// </summary>
        public string themeId;

        /// <summary>affecte la fonction doUnload qui permet de renseigner la déconnexion de l'utilisateur dans les logs de connexion</summary>
        public string statLogApp;

        /// <summary>Enregistre d'éventuelles erreurs lors de la connexion (montée de version, etc...)</summary>
        public string AppLoadingLog = string.Empty;

        private int defaultTab = 0; // table par défaut à ajouter

        /// <summary>Depuis le serveur</summary>
        public Boolean IsLocal = false;

        /// <summary>demande 36826, IsCtiEnabled</summary>
        public Boolean IsCtiEnabled = false;

        /// <summary>demande 41250, IsSpecifCtiEnabled</summary>
        public Boolean IsSpecifCtiEnabled = false;

        public int CtiSpecifId = 0;

        /// <summary>numero de version de la base de la base de donnees</summary>
        private string _sBddVersion = string.Empty;

        /// <summary>
        /// Dernière newsletter 'masquér'  par l'utilisateur
        /// </summary>
        private int _nDisplayMsg = 0;

        /// <summary>
        /// Id de redirection utilisé par eGoToFile
        /// </summary>
        private string _redirTabID = "";

        public string _jsFileRedir = string.Empty;

        /// <summary>
        /// retourne la table à afficher
        /// (accueil par défaut)
        /// </summary>
        public int DefaultTab
        {
            get { return defaultTab; }
        }

        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return scriptHolder;
        }

        /// <summary>
        /// Chargement de la page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            #region Variables de session

            int groupId = _pref.User.UserGroupId;
            int userLevel = _pref.User.UserLevel;

            int lang = _pref.LangId;

            EudoCommonHelper.EudoHelpers.SaveCookie("langue", "LANG_" + lang.ToString().PadLeft(2, '0'), DateTime.MaxValue, Response);

            int userId = _pref.User.UserId;
            string instance = _pref.GetSqlInstance;
            string baseName = _pref.GetBaseName;

            if (_pref.ThemeXRM.FontSizeMax != null && _pref.ThemeXRM.FontSizeMax.Count > 0)
            {
                int nMax = _pref.ThemeXRM.FontSizeMax.Max();
                int nCurrent;
                if (int.TryParse(_pref.FontSize, out nCurrent))
                {

                    if (nCurrent > nMax)
                        _pref.FontSize = nMax.ToString();
                }

            }

            IsLocal = Request.IsLocal;
            _pref.AdminMode = false;
            #endregion

            #region Montée de version de la base
            eUpgrader eUpg = new eUpgrader(_pref, "version");
            try
            {
                eUpg.Process();
            }
            catch (XRMUpgradeException XRMUE)
            {
#if DEBUG
                AppLoadingLog = XRMUE.Message;
#else
                AppLoadingLog = eResApp.GetRes(_pref, 6754);    //Erreur de montée de version
#endif
                eErrorContainer errC = new eErrorContainer()
                {
                    AppendDebug = string.Concat(eResApp.GetRes(_pref, 6754), " : ", XRMUE.Message),
                    AppendMsg = eResApp.GetRes(_pref, 72),
                    AppendTitle = eResApp.GetRes(_pref, 6754),
                    AppendDetail = AppLoadingLog
                };
                eFeedbackXrm.LaunchFeedbackXrm(errC, _pref);

                AppLoadingLog = eTools.GetJsAlert(errC, 500, 200);
            }
            #endregion

            #region Test de la validé du model de l'ORM

            string prefixDetailMsg = string.Empty;
            string ormExpMessage = null;
            try
            {
                eLibTools.OrmLoadAndGetMapAdv(_pref, new OrmGetParams() { CachePolicy = OrmMapCachePolicy.FORCE_REFRESH });
            }
            catch (OrmException ormExp)
            {
                // Le message à destination de l'administrateur est complèté par l'URL de l'ORM
                if (_pref.User.UserLevel >= (int)UserLevel.LEV_USR_ADMIN)
                    prefixDetailMsg = ormExp.OrmUrl;

                ormExpMessage = ormExp.Message;
            }
            catch (Exception exp)
            {
                ormExpMessage = exp.Message;
            }

            if (ormExpMessage != null)
            {
                eErrorContainer ormExp = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.INFOS,
                            eResApp.GetRes(_pref, 1823), $"<p>{prefixDetailMsg}</p><p>{eResApp.GetRes(_pref, 1824)}</p>", eResApp.GetRes(_pref, 6536),
                            ormExpMessage);

                // non bloquant, on affiche le msg et on continue
                AppLoadingLog = string.Concat(AppLoadingLog, Environment.NewLine, eTools.GetJsAlert(ormExp, 600, 250, "", "Orm"));
            }

            #endregion



            globalNav.Attributes["class"] += string.Concat(" ", eTools.GetClassNameFontSize(_pref));

            container.Attributes["class"] += string.Concat(" ", eTools.GetClassNameFontSize(_pref));




#if DEBUG
            globalNav.Attributes["class"] += string.Concat(" MODE_DEBUG");
#endif

            // quick test valeur adfs for debug
            /*
            System.Collections.Generic.Dictionary<UserField, string> mapAdfsEudo = eTools.GetADFSMappingValue(_pref);
            string s = eTools.GetWindowsAccountNameFromADFS();
            */

            try
            {
                // lance les tests d'intégrités
                (new eLauncher(_pref)).Do();

                eLoginOLGeneric.LaunchCspOnload(_pref, _pref.UserId);
            }
            catch (Exception exCsp)
            {
                //erreur SQL de lancement de la csp_onloadApp
                eErrorContainer cspExp = eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                    string.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                    eResApp.GetRes(_pref, 72),  //   titre
                    string.Concat("Erreur sur eMain.aspx - Erreur d'exécution de la csp_onLoadApp (", exCsp.Message, ")")
                );

                // non bloquant, on affiche le msg et on continue
                AppLoadingLog = string.Concat(AppLoadingLog, Environment.NewLine, eTools.GetJsAlert(cspExp, 500, 200, "", "CspOnLoadApp"));
            }

            #region ajout des css

            PageRegisters.AddCss("eCatalog");
            PageRegisters.AddCss("eMain");
            PageRegisters.AddCss("eButtons");
            PageRegisters.AddCss("eModalDialog");
            PageRegisters.AddCss("eContextMenu");
            PageRegisters.AddCss("eControl");
            PageRegisters.AddCss("eList");
            PageRegisters.AddCss("eHomepage");
            PageRegisters.AddCss("eTitle");
            PageRegisters.AddCss("eIcon");
            PageRegisters.AddCss("ePlanning");
            PageRegisters.AddCss("eCalendar");
            PageRegisters.AddCss("eMemoEditor");

            PageRegisters.AddCss("eFontSize_" + eTools.GetUserFontSize(_pref));

            // Les notifications systèmes ne sont pas dans l'extension
            PageRegisters.AddCss("eNotif");


            #endregion

            #region ajout des js
            PageRegisters.AddScript("eColorPicker");
            PageRegisters.AddScript("eTools");
            PageRegisters.AddScript("eMain");
            PageRegisters.AddScript("eNavBar");
            PageRegisters.AddScript("eModalDialog");
            PageRegisters.AddScript("eFieldEditor");
            PageRegisters.AddScript("eEngine");
            PageRegisters.AddScript("eUpdater");
            PageRegisters.AddScript("ePopup");
            PageRegisters.AddScript("eContextMenu");
            PageRegisters.AddScript("eExpressFilter");
            PageRegisters.AddScript("eList");
            PageRegisters.AddScript("eReportCommon");
            PageRegisters.AddScript("ePlanning");
            PageRegisters.AddScript("eTreatment");
            PageRegisters.AddScript("eFilterWizardLight");
            PageRegisters.AddScript("eHomePage");
            PageRegisters.AddScript("jquery.min");
            PageRegisters.AddScript("syncFusion/jsrender.min");
            PageRegisters.AddScript("syncFusion/ej.web.all.min");
            PageRegisters.AddScript("eLastValuesManager");
            PageRegisters.AddScript("eAutoCompletion");
            PageRegisters.AddScript("eFile");

#if DEBUG
            PageRegisters.AddScript("~/IRISBlack/Front/Scripts/Libraries/vue/vue");
            PageRegisters.AddScript("~/IRISBlack/Front/Scripts/Libraries/vuex/vuex");
            PageRegisters.AddScript("~/IRISBlack/Front/Scripts/Libraries/axios/axios");
            PageRegisters.AddScript("~/IRISBlack/Front/scripts/Libraries/vuetify/vuetify.min");
            PageRegisters.AddScript("~/IRISBlack/Front/scripts/Libraries/eudofront/eudoFront.umd");

#else
            PageRegisters.AddScript("~/IRISBlack/Front/Scripts/Libraries/vue/vue.min");
            PageRegisters.AddScript("~/IRISBlack/Front/Scripts/Libraries/vuex/vuex.min");
            PageRegisters.AddScript("~/IRISBlack/Front/Scripts/Libraries/axios/axios.min");
            PageRegisters.AddScript("~/IRISBlack/Front/scripts/Libraries/vuetify/vuetify.min");
            PageRegisters.AddScript("~/IRISBlack/Front/scripts/Libraries/eudofront/eudoFront.umd.min");
#endif

            bool bThemeIncompat = (_pref.ThemeXRM.IsBrowserIncompatible(System.Web.HttpContext.Current.Request.Browser.Type.ToLower(), System.Web.HttpContext.Current.Request.UserAgent.ToLower())
                || (_pref.ThemeXRM.Version < 2));

            if (!bThemeIncompat)
                PageRegisters.AddScript("~/IRISBlack/Front/Scripts/InitComponentsIrisBlack");


            PageRegisters.RawScrip.AppendLine()
                .AppendFormat("EudoWebSite = '{0}';", eTools.GetEudonetWebSite(_pref));


            PageRegisters.RawScrip.AppendLine("").Append(string.Concat("top._CombinedZ = '", eLibConst.COMBINED_Z, "';"));
            PageRegisters.RawScrip.AppendLine("").Append(string.Concat("top._CombinedY = '", eLibConst.COMBINED_Y, "';"));
            PageRegisters.RawScrip.AppendLine("").Append("  top._resChart = 'en-US';");

            switch (_pref.LangServId)
            {
                case 0:
                    PageRegisters.AddScript("syncFusion/ej.culture.fr-FR.min");
                    PageRegisters.AddScript("syncFusion/ej.localetexts.fr-FR");
                    PageRegisters.RawScrip.AppendLine("").Append("top._resChart = 'fr-FR';");
                    break;
                case 1:
                    PageRegisters.AddScript("syncFusion/ej.localetexts.en-US");
                    PageRegisters.AddScript("syncFusion/ej.culture.en-US.min");
                    break;
                case 2:
                    PageRegisters.AddScript("syncFusion/ej.localetexts.de-DE");
                    PageRegisters.AddScript("syncFusion/ej.culture.de-DE.min");
                    PageRegisters.RawScrip.AppendLine("").Append("top._resChart = 'de-DE';");
                    break;
                case 3:
                    PageRegisters.AddScript("syncFusion/ej.localetexts.en-US");
                    PageRegisters.AddScript("syncFusion/ej.culture.en-US.min");
                    break;
                case 4:
                    PageRegisters.AddScript("syncFusion/ej.localetexts.es-ES");
                    PageRegisters.AddScript("syncFusion/ej.culture.es-ES.min");
                    PageRegisters.RawScrip.AppendLine("").Append("top._resChart = 'es-ES';");
                    break;
                case 5:
                    PageRegisters.AddScript("syncFusion/ej.localetexts.en-US");
                    PageRegisters.AddScript("syncFusion/ej.culture.en-US.min");
                    break;
                default:
                    PageRegisters.AddScript("syncFusion/ej.localetexts.en-US");
                    PageRegisters.AddScript("syncFusion/ej.culture.en-US.min");
                    break;
            }

            // Gestion des évenements          
            PageRegisters.AddScript("eEvent");

            // Point d'entrée vers les grilles 
            PageRegisters.AddScript("grid/eGridSystem");

            PageRegisters.AddScript("multiselect/eMultiSelect");

            // Pour démarrer l'assistant d'import
            // Il sert a communiquer avec eImportInternal de l'assistant
            PageRegisters.AddScript("import/eImportWizard");


            // carthographie           
            if (CanRunCartography())
                PageRegisters.AddScript("eCartography");


            if (Request.Browser.Browser == "IE" && Request.Browser.MajorVersion == 8)
                PageRegisters.AddScript("ie8-styles");

            if (Request.Browser.Browser == "IE" && Request.Browser.MajorVersion > 9)
                PageRegisters.AddScript("ie9-styles");


            PageRegisters.RawScrip.AppendLine("").Append("var _bISSOCnx =")
                    .Append((eLibTools.GetServerConfig("ADFSApplication", "0") == "1" || eLibTools.GetServerConfig("SSOApplication", "0") == "1") ? "1" : "0").AppendLine(";");


            //Id de process
            long d = DateTime.Now.Ticks;
            Session["_uidupdater"] = HashSHA.GetHashSHA1(_pref.UserId.ToString() + "##" + d + "##" + _pref.GetBaseName);
            Session["_uidupdaterd"] = d.ToString();

            PageRegisters.RawScrip.AppendLine("").Append("window['_uidupdater'] = '" + Session["_uidupdater"].ToString() + "';");

            #endregion



            InitConfig();

            if (CanRunBingAutoSuggest())
            {
                accessKey.Value = eLibConst.BING_MAPS_KEY;
            }

            #region Récupération du TabID de redirection dès maintenant (car la newsletter ne doit pas être affichée si on fait une redirection)
            // #59 724 - Utilisation de variables globales pour retrouver le même contexte (liste, fiche) lors du rechargement d'eParamIFrame
            // (résolution du conflit entre les devs #23 614 et #39 338)
            _redirTabID = Request.Form["redirTabID"];
            #endregion

            #region Affichage des news
            var myNewsLetter = eTools.LoadNewsLetterInfos();

            Boolean bNoInternet = eLibTools.GetServerConfig("ServerWithoutInternet", "0") == "1";

            string sDisplayMsg = _pref.GetConfig(eLibConst.PREF_CONFIG.DISPLAYVERMSG);

            sDisplayMsg = sDisplayMsg.Replace("10.", "");
            if (!int.TryParse(sDisplayMsg, out _nDisplayMsg))
                _nDisplayMsg = 0;


            bool bIntra = (ConfigurationManager.AppSettings.Get("IntranetApplication") == "1");

            bool bDisplay = _nDisplayMsg < ((_pref.User.UserLevel >= (int)UserLevel.LEV_USR_ADMIN) ? myNewsLetter.adminmsg.num : myNewsLetter.usermsg.num);

            // #89 330 - Si on effectue une redirection, il ne faut pas afficher la newsletter, qui perturbe l'affichage
            if (!String.IsNullOrEmpty(_redirTabID))
                bDisplay = false;

            //Vérification de l'accessibilité de la newsletter pour les intra
            if (bIntra && bDisplay)
            {

                string sBaseURL = eConst.NEWSLETTER_URL;

                // Si on est en intra et qu'il n'y a pas internet, on n'affiche pas
                if (bNoInternet)
                    bDisplay = false;
                else
                {
                    // Sinon on tente...
                    try
                    {
                        WebRequest request = WebRequest.Create(sBaseURL);
                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                            if (response == null || response.StatusCode != HttpStatusCode.OK)
                                bDisplay = false;
                        }
                    }
                    catch (Exception exc)
                    {
                        string test = exc.Message;
                        bDisplay = false;
                    }
                }

            }

            if (bDisplay)
            {
                PageRegisters.AddScript("eDisplayNewsMessage");

                _jsFileRedir = string.Concat(_jsFileRedir, Environment.NewLine, "top._newsLetterUrl =  '",
                   ((_pref.User.UserLevel >= (int)UserLevel.LEV_USR_ADMIN) ? myNewsLetter.adminmsg.GetUserUrl(_pref).Url : myNewsLetter.usermsg.GetUserUrl(_pref).Url).Replace("'", "\\'"),
                   "';", Environment.NewLine);

                _jsFileRedir = string.Concat(_jsFileRedir, Environment.NewLine, "top._newsLetterButtonText =  '",
                   ((_pref.User.UserLevel >= (int)UserLevel.LEV_USR_ADMIN) ? myNewsLetter.adminmsg.GetUserButton(_pref).Text : myNewsLetter.usermsg.GetUserButton(_pref).Text)?.Replace("'", "\\'"),
                   "';", Environment.NewLine);

                // TODO: alimenter top._newsLetterLinks

                _jsFileRedir = string.Concat(_jsFileRedir, Environment.NewLine, "top._newsLetterNum =  '",
                      ((_pref.User.UserLevel >= (int)UserLevel.LEV_USR_ADMIN) ? myNewsLetter.adminmsg.num : myNewsLetter.usermsg.num),
                    "';", Environment.NewLine);

                // US #2 244 - Demande #80 848 - Passage de la langue de l'utilisateur pour l'URL de la newsletter par défaut (IE)
                string userLang = eLibTools.ValidateLangue(_pref.User.UserLang) ? _pref.User.UserLang : "LANG_00";
                _jsFileRedir = string.Concat(_jsFileRedir, Environment.NewLine, "top._newsLetterLang = '", userLang, "';", Environment.NewLine);

            }
            #endregion

            #region Titres et version

            Titre = string.Concat("Eudonet XRM ", ((IsLocal) ? "Local" : ""), " - ", _pref.EudoBaseName);

            // About box
            StringBuilder aboutBoxContent = new StringBuilder()
                .Append(eResApp.GetRes(_pref, 8185)).Append(" : ").Append(eConst.VERSION).Append(" ")
                .Append("<a id='vr' eDisp='vn' style='display:inline;' onclick='DisplayAndHide(this);return false;' href='#'>(").Append(eConst.REVISION).Append(")</a>")
                .Append("<a id='vn' eDisp='vr' style='display:none;'  onclick='DisplayAndHide(this);return false;' href='#'>").Append(eConst.VERSIONNAME).Append("</a>");

            if (_sBddVersion != eConst.VERSION)
                aboutBoxContent.Append("<br/>").Append(eResApp.GetRes(_pref, 8184)).Append(" : ").Append(_sBddVersion);

#if DEBUG
            aboutBoxContent
                .Append("<br/>")
                .Append("[DEBUG Build, ")
                .Append(System.Web.HttpContext.Current.IsDebuggingEnabled.ToString().ToLower().Equals("true") ? "DEBUG" : "RELEASE").Append(" Configuration]")
                .Append("<br/>Date de build :")
                .Append(EudoCommonHelper.EudoHelpers.GetAssemblyDate(System.Reflection.Assembly.GetExecutingAssembly()));
#endif

            aboutBoxContent.Append("<br/>").Append(eResApp.GetRes(_pref, 1286)).Append(" : <a href='")

                .Append(((_pref.User.UserLevel >= (int)UserLevel.LEV_USR_ADMIN) ? myNewsLetter.adminmsg.GetUserUrl(_pref).Url : myNewsLetter.usermsg.GetUserUrl(_pref).Url))

                .Append("' target='_blank'>").Append(eResApp.GetRes(_pref, 542)).Append("</a>");

            // Dans le cas d'une base sans nom, on renseigne "_" pour premettre le clique pour obtenir le nom sql de la base
            string aboutBoxBaseName = string.IsNullOrEmpty(_pref.EudoBaseName) ? "_" : _pref.EudoBaseName;

            aboutBoxContent
                .Append("<br/>Base : ")
                .Append("<a id='bn' eDisp='bsql' style='display:inline;' onclick='DisplayAndHide(this);return false;' href='#'>").Append(aboutBoxBaseName).Append("</a>")
                .Append("<a id='bsql' eDisp='bn' style='display:none;'  onclick='DisplayAndHide(this);return false;' href='#'>").Append(_pref.GetBaseName).Append("</a>");

            aboutBoxContent
                .Append("<br/>").Append(eResApp.GetRes(_pref, 1011)).Append(" : ")
                .Append("<a id='bnn' style='display:inline;' href='").Append(eConst.HELP_DESK_URL).Append("' target='_blank'>").Append(eConst.HELP_DESK_URL).Append("</a>");

            aboutBoxContent.Append("<br />").Append(eResApp.GetRes(_pref, 8802)).Append(" : ")
                .Append("<a id='lcs' style='display:inline;' href='")
                .Append(eConst.LICENSE_FILE).Append("' target='_blank'>")
                .Append(eConst.LICENSE_FILE).Append("</a>");

            if (_pref.User.UserLevel >= UserLevel.LEV_USR_ADMIN.GetHashCode())
            {
                /*  aboutBoxContent
                  .Append("<br/>").Append(eResApp.GetRes(_pref, 6439)).Append(" : ")
                  .Append("<a id='bna' style='display:inline;' href='http://ww2.eudonet.com/v7plus/app/specif/EUDO_HOTCOM_EUDOWEB/FormulaireDemandeClient/Index.aspx' target='_blank'>").Append(eResApp.GetRes(_pref, 542)).Append("</a>");
                */
            }


            sJsAbout = eTools.GetJsAlert(eErrorContainer.GetUserError(eLibConst.MSG_TYPE.INFOS, aboutBoxContent.ToString(), "", eResApp.GetRes(_pref, 611)), 530, 210);

            #endregion

            // Mode debug
            _pref.ModeDebug = (_requestTools.GetRequestQSKeyS("m") ?? "").Equals("d", StringComparison.InvariantCultureIgnoreCase);

            #region Définition de la fonction JS de redirection au chargement de la page + Cas redirection depuis eGoToFile.aspx - #23 614, 39 338, #59 724

            // Mode par défaut à l'ouverture de session : accueil, "liste"
            string defaultView = "LIST"; // "KANBAN", "CALENDAR", "CALENDAR_LIST", "LIST", "FILE_CONSULTATION", "FILE_MODIFICATION", "FILE_CREATION", "ADMIN_FILE"
            string defaultFile = "0";
            string sFileInPopup = "0";
            string isTplMail = "0";

            // Si l'appel provient de eGotoFile.aspx : traitement de ses paramètres
            if (!string.IsNullOrEmpty(_redirTabID))
            {
                string redirFileID = Request.Form["redirFileID"];
                sFileInPopup = Request.Form["fileInPopup"];
                isTplMail = Request.Form["redirTPLMail"];
                //string redirBkmTabID = Request.Form["redirBkmTabID"];
                //string redirBkmFileID = Request.Form["redirBkmFileID"];

                defaultTab = eLibTools.GetNum(_redirTabID);

                if (!string.IsNullOrEmpty(redirFileID))
                {
                    if (eLibTools.GetNum(redirFileID) != 0)
                    {
                        defaultFile = redirFileID;

                        // Affichage simple d'une fiche depuis un onglet : on passe par goTabList, et des paramètres de callback y seront ajoutés (cf. plus bas)
                        if (sFileInPopup == "1")
                            defaultView = "LIST";
                        else
                            defaultView = "FILE_MODIFICATION";
                    }
                }
            }

            _jsFileRedir = string.Concat(_jsFileRedir, Environment.NewLine,
                // #59 724 - Paramètres initialisés au premier chargement d'eMain.aspx
                // Pouvant ensuite être modifiés en JS avant le rechargement d'eParamIFrame (ex : loadTabs, onTabSelect, refreshAfterOrientation) pour retrouver le même contexte
                "var tabToLoadAfterParamIFrame = ", defaultTab, ";", Environment.NewLine,
                "var viewToLoadAfterParamIFrame = '", defaultView, "';", Environment.NewLine,
                "var fileToLoadAfterParamIFrame = ", defaultFile, ";", Environment.NewLine,
                "var isTplMailToLoadAfterParamIFrame = ", isTplMail, ";", Environment.NewLine,
                "var loadFileInPopupAfterParamIFrame = ", sFileInPopup, ";", Environment.NewLine,
                "var preventSwitchToHiddenTabUsingView = false;", Environment.NewLine,
                // Puis définition de la fonction globale, qui est donc désormais 100% JavaScript et ne se base plus sur la valeur en dur des paramètres définis ici
                // Tout rechargement d'eParamIFrame pourra ainsi être effectué en retrouvant le contexte présent avant le rechargement
                "function eParamOnLoad() {", Environment.NewLine,
                    "switch (top.viewToLoadAfterParamIFrame) {", Environment.NewLine,
                        "case 'LIST':", Environment.NewLine,
                            "if (top.loadFileInPopupAfterParamIFrame == '1') {", Environment.NewLine,
                                "goTabList(top.tabToLoadAfterParamIFrame, true, ", Environment.NewLine,
                                "function() { ", Environment.NewLine,
                                    "shFileInPopup(top.tabToLoadAfterParamIFrame, top.fileToLoadAfterParamIFrame, top._res_151, 0, 0, top.isTplMailToLoadAfterParamIFrame); ", Environment.NewLine,
                                "});", Environment.NewLine,
                            "}", Environment.NewLine,
                            "else {", Environment.NewLine,
                                "goTabList(top.tabToLoadAfterParamIFrame);", Environment.NewLine,
                            "}", Environment.NewLine,
                            "break;", Environment.NewLine,
                        "case 'FILE_CONSULTATION':", Environment.NewLine,
                            "loadFile(top.tabToLoadAfterParamIFrame, top.fileToLoadAfterParamIFrame, 2);", Environment.NewLine,
                            "break;", Environment.NewLine,
                        "case 'FILE_MODIFICATION':", Environment.NewLine,
                            "loadFile(top.tabToLoadAfterParamIFrame, top.fileToLoadAfterParamIFrame, 3);", Environment.NewLine,
                            "break;",
                        "case 'FILE_CREATION':",
                            "loadFile(top.tabToLoadAfterParamIFrame, top.fileToLoadAfterParamIFrame, 5);",
                            "break;",
                    "}",
                "}");

            #endregion

            // Chargement des préférences
            try
            {
                if (!_pref.LoadPref() || !_pref.LoadTabs())
                    throw new Exception("Chargement des LoadPref et/ou LoadTabs vide !");
            }
            catch (Exception exp)
            {
                // Erreur bloquante, renvoie au login
                ErrorContainer = eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                    string.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                    eResApp.GetRes(_pref, 72),  //   titre
                    string.Concat("Erreur sur eMain.aspx - Page_Load - Impossible de charger les préférences et/ou les onglets (", exp.Message, ")")
                );

                // On considère la session comme perdue pour rediriger vers la page de login car l'application est inutilisable si
                // le chargement des préférences a échoué.
                ErrorContainer.IsSessionLost = true;
                ErrorContainer.ForceFeedback = true;

                try
                {
                    LaunchErrorHTML(true);
                }
                catch (eEndResponseException)
                {
                    Response.End();
                }
            }

            // Chargement des ressources application pour la page
            // 296  : Actions
            // 1621 : Dernière(s) fiche(s) <TAB> consultée(s)
            // 326  : Administrer
            // 1485 : Liste des fiches
            // 742  : Créer la fiche
            // 1040 : Rechercher
            // 644  : Chargement en cours

            _pref.Context.CuttedItems = new ExtendedDictionary<int, int>();
            _pref.Context.CopiedItems = new ExtendedDictionary<int, int>();
        }


        /// <summary>
        /// Savoir si le navigateur est supporter pour la cartographie
        /// 
        /// Au : 06/12/2016
        ///  Supported Browsers
        ///  
        ///        Bing
        ///  
        ///        The Bing Maps V8 Web Control is supported with most modern browsers that are HTML5-enabled, specifically:
        ///        Desktop
        ///         The current and previous version of Microsoft Edge(Windows)
        ///         Internet Explorer 11 (Windows)
        ///         The current and previous version of Firefox(Windows, Mac OS X, Linux)  
        ///         The current and previous version of Chrome(Windows, Mac OS X, Linux)
        ///         The current and previous version of Safari(Mac OS X)
        ///  
        ///        Note: Internet Explorer's Compatibility View is not supported.
        ///        https://msdn.microsoft.com/en-us/library/mt712867.aspx
        /// </summary>
        /// <returns></returns>
        public Boolean CanRunCartography()
        {
            return eTools.BrowserSupportedByBing(Request) && eExtension.IsReady(_pref, ExtensionCode.CARTOGRAPHY);

        }

        /// <summary>
        /// Savoie s'il y a un mapping sur la base
        /// </summary>
        /// <returns></returns>
        public Boolean CartographyEnabled()
        {
            return eFilemapPartner.IsCartoEnabled(_pref);
        }

        /// <summary>
        /// Peut-on activer l'autosuggestion BingMaps ?
        /// </summary>
        public Boolean CanRunBingAutoSuggest()
        {
            return eTools.CanRunBingAutoSuggest(_pref, Request);
        }

        /// <summary>
        /// Pour tester la double validation
        /// </summary>
        /// <returns></returns>
        public string DebugDoubleVal()
        {

            return eLibTools.GetConfigAdvValues(_pref, new List<eLibConst.CONFIGADV> { eLibConst.CONFIGADV.DEBUG_DOUBLE_VALIDATION })[eLibConst.CONFIGADV.DEBUG_DOUBLE_VALIDATION] == "1" ? "true" : "false";// js
        }

        private void InitConfig()
        {
            // MCR demande 40170: MCR test de la variable du device du CTI : CTIDevice, si n'est pas egale à 3 pour "Autre: le mode specif XRM pour le CTI "
            // pour le client CCI77, en plus de la variable CTIENABLED == "1"
            System.Collections.Generic.Dictionary<eLibConst.CONFIG_DEFAULT, string> dicoConfig = _pref.GetConfigDefault(new eLibConst.CONFIG_DEFAULT[]
            { eLibConst.CONFIG_DEFAULT.CTIENABLED,
                eLibConst.CONFIG_DEFAULT.CTIDevice,
                eLibConst.CONFIG_DEFAULT.VERSION,
                eLibConst.CONFIG_DEFAULT.LOGONAME ,


            });
            IsCtiEnabled = (dicoConfig[eLibConst.CONFIG_DEFAULT.CTIENABLED] == "1" && !(dicoConfig[eLibConst.CONFIG_DEFAULT.CTIDevice] == "3"));

            //demande 41250: pour la CCI77 set de la variable IsSpecifCtiEnabled si CTiEnabled == true et CTIDevice == 3 je suis en mode Specif CTI 
            IsSpecifCtiEnabled = (dicoConfig[eLibConst.CONFIG_DEFAULT.CTIENABLED] == "1" && dicoConfig[eLibConst.CONFIG_DEFAULT.CTIDevice] == "3");


            // 41250 : recupération de l'id de specif pour le CTI de la TYP_SPECIF_CTI = 13
            if (IsSpecifCtiEnabled)
            {
                eudoDAL dal = eLibTools.GetEudoDAL(_pref);

                try
                {
                    string sError = string.Empty;
                    dal.OpenDatabase();

                    string sSQL = "SELECT TOP 1 [SpecifId] FROM [SPECIFS] where [SPECIFType] = @SPECIFTYPE order by SpecifId desc";

                    RqParam rq = new RqParam(sSQL);
                    rq.AddInputParameter("@SPECIFTYPE", SqlDbType.Int, eLibConst.SPECIF_TYPE.TYP_SPECIF_CTI);


                    CtiSpecifId = dal.ExecuteScalar<int>(rq, out sError);


                    if (sError.Length != 0)
                        CtiSpecifId = 0;


                }
                finally
                {
                    dal.CloseDatabase();
                }
            }

            _sBddVersion = dicoConfig[eLibConst.CONFIG_DEFAULT.VERSION];
            LogoName = dicoConfig[eLibConst.CONFIG_DEFAULT.LOGONAME];

        }
    }
}
