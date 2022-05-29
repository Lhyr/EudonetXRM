using System;
using System.Collections.Generic;
using System.Data;

using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Merge;
using Com.Eudonet.Xrm.eda;
using EudoExtendedClasses;
using EudoQuery;

namespace Com.Eudonet.Xrm.mgr
{
    /// <className>eFileMenuManager</className>
    /// <summary>Génération du XML menu droite</summary>
    /// <purpose>Génère un xml pour la création du menu droite de l'appli.
    /// Ne s'appelle pas indépendamment : Cette page ne rend que du XML. Il doit 
    /// ensuite être traité pour être affiché. Soit par xslt via la xsl menuxsl.xml
    /// soit via parsing du xml. 
    /// Dans l'application, la transformation est faite via xslt. Cf eMain.Js
    /// </purpose>
    /// <authors>SPH</authors>
    /// <date>2011-09-11</date>
    public class eFileMenuManager : eEudoManagerReadOnly
    {
        XmlNode _baseResultNode;
        TableLite _tab;
        /// <summary>Document User Menu</summary>
        XmlDocument _xmlUserMenu = new XmlDocument();
        XmlNode _baseUserMenu;

        bool _bServerSideXSLT = true;
        bool _bReturnInXML = true;

        private static readonly int[] idThemeIris = { 12, 13, 14 };


        /// <summary>
        /// Enables processing of HTTP Web requests
        /// </summary>
        protected override void ProcessManager()
        {
            int groupId = _pref.User.UserGroupId;
            int userLevel = _pref.User.UserLevel;

            string lang = _pref.Lang;

            int userId = _pref.User.UserId;
            string instance = _pref.GetSqlInstance;
            string baseName = _pref.GetBaseName;


            /** Ici, on tente un tripatouillage pour passer outre le fait 
             * que le thème est mis à 0, y compris en base pour la partie admin. G.L 
             */
            int iThemeId = 0;
            if (!int.TryParse(_context.Request.Form["iThemeId"]?.ToString(), out iThemeId))
                iThemeId = _pref.ThemeXRM.Id;

            #region initialisation

            // BASE DU XML DE RETOUR
            XmlDocument xmlResult = new XmlDocument();
            xmlResult.AppendChild(xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null));
            _baseResultNode = xmlResult.CreateElement("result");
            xmlResult.AppendChild(_baseResultNode);


            // XML User Menu
            _baseUserMenu = _xmlUserMenu.CreateElement("menu");
            _xmlUserMenu.AppendChild(_baseUserMenu);


            XmlAttribute xAtt = _xmlUserMenu.CreateAttribute("admin");
            xAtt.Value = _pref.User.UserLevel >= UserLevel.LEV_USR_ADMIN.GetHashCode() ? "1" : "0";
            _baseUserMenu.Attributes.Append(xAtt);


            XmlAttribute xAttSSo = _xmlUserMenu.CreateAttribute("sso");
            xAttSSo.Value = (eLibTools.GetServerConfig("ADFSApplication", "0") == "1" || eLibTools.GetServerConfig("SSOApplication", "0") == "1") ? "1" : "0";
            _baseUserMenu.Attributes.Append(xAttSSo);



            XmlAttribute xAttAdfs = _xmlUserMenu.CreateAttribute("adfs");
            xAttAdfs.Value = (eLibTools.GetServerConfig("ADFSApplication", "0") == "1" ? "1" : "0");
            _baseUserMenu.Attributes.Append(xAttAdfs);

            int nDescid = 0;
            int nType = -1; // 0: liste / 1:fiche / 2:Accueil
            int nFileId = 0;

            #endregion

            #region verification et récupération des paramètres
            // Pas de paramètre ou paramètres invalide
            if (string.IsNullOrEmpty(_context.Request.Form["tab"])
                || !int.TryParse(_context.Request.Form["tab"].ToString(), out nDescid)
                || string.IsNullOrEmpty(_context.Request.Form["type"])
                || !int.TryParse(_context.Request.Form["type"].ToString(), out nType)
                )
            {
                this.ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 2024).Replace("<PARAM>", "tab/type"), eResApp.GetRes(_pref, 2024).Replace("<PARAM>", "tab/type"));
                LaunchError();
            }

            eConst.eFileType mgrType = (eConst.eFileType)nType;

            // Type de module Administration / Options utilisateur concerné par le menu
            eUserOptionsModules.USROPT_MODULE targetModule = eUserOptionsModules.USROPT_MODULE.UNDEFINED;
            if (_context.Request.Form["module"] != null)
            {
                int nTargetModule = 0;
                int.TryParse(_context.Request.Form["module"].ToString(), out nTargetModule);
                targetModule = (eUserOptionsModules.USROPT_MODULE)nTargetModule;
            }
            eUserOptionsModules.USROPT_MODULE rootParentModule = eUserOptionsModules.GetModuleParent(targetModule, true);

            //Id de la fiche
            if (_allKeys.Contains("fileid") && _context.Request.Form["fileid"].Length > 0)
                int.TryParse(_context.Request.Form["fileid"].ToString(), out nFileId);

            if (!string.IsNullOrEmpty(_context.Request.Form["xsltserver"]))
            {
                if (_context.Request.Form["xsltserver"].ToString() == "0")
                    _bServerSideXSLT = false;
            }


            int nbFavLnkPerCol = 0;
            if (mgrType == eConst.eFileType.ACCUEIL && _allKeys.Contains("nbFavLnkPerCol"))
                int.TryParse(_context.Request.Form["nbFavLnkPerCol"].ToString(), out nbFavLnkPerCol);


            // Les notifications systèmes ne sont pas dans l'extension
            bool bNotifsEnabled = true;
            int nNotificationsCountUnread = 0;
            //Nombre de notifications non lues
            if (bNotifsEnabled)
            {
                try
                {
                    eNotificationList _list = eListFactory.CreateListNotificationCountUnread(_pref) as eNotificationList;
                    nNotificationsCountUnread = _list.GetCountUnread;
                }
                catch (Exception e) { }
            }

            //Lien Aide
            bool helpExtranetEnabled = eLibTools.GetServerConfig("HelpDeskEnabled", "0") == "1";

            // retour dans un flux xml
            _bReturnInXML = false;

            #endregion

            eNav navRightMenu = new eNav(instance, baseName, userId, lang, _pref);
            eudoDAL _dal = eLibTools.GetEudoDAL(_pref);
            RqParam infoTab;
            DataTableReaderTuned dtrTabTreatmentRight = null;
            string sError;

            _tab = new TableLite(nDescid);
            eParam param = new eParam(_pref);
            if (!param.InitTabNelleErgo(_dal, out sError))
            {
                ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, string.Concat(eResApp.GetRes(_pref, 416)), sError);
                LaunchError(ErrorContainer);
            }

            DescAdvDataSet descAdv = new DescAdvDataSet();
            descAdv.LoadAdvParams(eDal: _dal,
                listDescid: new List<int> { nDescid },
                searchedParams: new List<DESCADV_PARAMETER> { DESCADV_PARAMETER.PURPLE_ACTIVATED_FROM }
                );

            List<int> lstNoAdmin = eLibTools.GetDescAdvInfo(_pref, new List<int>() { nDescid }, new List<DESCADV_PARAMETER>() { DESCADV_PARAMETER.NOAMDMIN }).Where(aa => aa.Value.Find(dd => dd.Item1 == DESCADV_PARAMETER.NOAMDMIN && dd.Item2 == "1") != null).Select(t => t.Key).ToList();
            lstNoAdmin.Add((int)TableType.PAYMENTTRANSACTION);

            bool bNoAdmin = lstNoAdmin != null && lstNoAdmin.Contains(nDescid);

            List<LOCATION_PURPLE_ACTIVATED> locationsPurpleActivated = descAdv.GetAdvInfoValue(nDescid, DESCADV_PARAMETER.PURPLE_ACTIVATED_FROM)
                  .ConvertToList<LOCATION_PURPLE_ACTIVATED>(",", new Converter<string, LOCATION_PURPLE_ACTIVATED?>(
                      delegate (string s)
                      {
                          LOCATION_PURPLE_ACTIVATED location = LOCATION_PURPLE_ACTIVATED.UNDEFINED;
                          if (!Enum.TryParse<LOCATION_PURPLE_ACTIVATED>(s, out location))
                              return LOCATION_PURPLE_ACTIVATED.UNDEFINED;

                          return location;
                      }));

            XmlNode xmlNbLnkPerCol = _xmlUserMenu.CreateElement("NbFavLnkPerCol");
            _baseUserMenu.AppendChild(xmlNbLnkPerCol);
            if (nbFavLnkPerCol == 0) { nbFavLnkPerCol = 10; }
            xmlNbLnkPerCol.InnerText = nbFavLnkPerCol.ToString();

            //Affichage ou non du bouton permettant de changer de theme.
            var requ = HttpContext.Current.Request;
            XmlNode xmlNavigateur = _xmlUserMenu.CreateElement("Navigateur");
            _baseUserMenu.AppendChild(xmlNavigateur);

            xmlNavigateur.InnerText = idThemeIris.Any(id => !_pref.GetThemeById(id).IsBrowserIncompatible(requ.Browser.Type.ToLower(), requ.UserAgent.ToLower()))
                /** On ajoute le fait que l'offre du client doit être au moins accès, ce qui inclus la partie admin. G.L */
                && _pref.ClientInfos.ClientOffer > eLibConst.ClientOffer.XRM ? "1" : "0";


            //Affichage ou non de l'icone de notifications
            XmlNode xmlNotifsEnabled = _xmlUserMenu.CreateElement("NotifsEnabled");
            _baseUserMenu.AppendChild(xmlNotifsEnabled);
            xmlNotifsEnabled.InnerText = (bNotifsEnabled ? "1" : "0");

            //Nombre de notifications non lues
            XmlNode xmlNotifsUnreadCount = _xmlUserMenu.CreateElement("NotifsUnreadCount");
            _baseUserMenu.AppendChild(xmlNotifsUnreadCount);
            xmlNotifsUnreadCount.InnerText = nNotificationsCountUnread.ToString();

            // Statut d'épinglage du menu
            XmlNode xmlRightMenuPinned = _xmlUserMenu.CreateElement("RightMenuPinned");
            _baseUserMenu.AppendChild(xmlRightMenuPinned);
            xmlRightMenuPinned.InnerText = (_pref.RightMenuPinned ? "1" : "0");

            //taille de la font
            XmlAttribute _xmlNFontsize = _xmlUserMenu.CreateAttribute("fontsize");
            _xmlNFontsize.Value = eTools.GetClassNameFontSize(_pref);
            _baseUserMenu.Attributes.Append(_xmlNFontsize);

            //Adminmode
            XmlNode xmlAdminMode = _xmlUserMenu.CreateElement("AdminMode");
            _baseUserMenu.AppendChild(xmlAdminMode);
            xmlAdminMode.InnerText = (_pref.AdminMode ? "1" : "0");

            //nTab
            XmlNode xmlGlobalActiveTab = _xmlUserMenu.CreateElement("tab");
            _baseUserMenu.AppendChild(xmlGlobalActiveTab);
            xmlGlobalActiveTab.InnerText = nDescid.ToString();

            XmlAttribute _menuType;
            XmlAttribute _iImg;
            XmlAttribute _onclick;
            XmlAttribute _title;

            List<eSpecif> lstSpecifs = new List<eSpecif>();



            // Statut d'affichage de la carte
            XmlNode xmlCartoDisplay = _xmlUserMenu.CreateElement("CartoDisplay");
            xmlCartoDisplay.InnerText = "0";
            _baseUserMenu.AppendChild(xmlCartoDisplay);


            Boolean bCartoEnabled = false;
            List<eUserOptionsModules.USROPT_MODULE> modules = null;
            switch (mgrType)
            {

                case eConst.eFileType.ACCUEIL:
                case eConst.eFileType.ADMIN:

                    #region LISTE DES RAPPORTS
                    /**********  Liste des rapports **********************/
                    if (mgrType == eConst.eFileType.ACCUEIL && navRightMenu.hasReport())
                    {
                        XmlNode _blocReport = _xmlUserMenu.CreateElement("blocMenu");
                        _blocReport.AppendChild(AddAction(eResApp.GetRes(_pref, 6055), "icon-edn-next icnMnuChip", "openUserReport()"));
                        _baseUserMenu.AppendChild(_blocReport);
                    }
                    /****************************************************/
                    #endregion

                    #region LISTE DES FAVORIS


                    /******************  Blocs des liens favoris ***********************/
                    if (mgrType == eConst.eFileType.ACCUEIL && navRightMenu.getFavoriteLink().Count > 0)
                    {
                        //Recherche et ajout des liens
                        XmlNode xmlBlocLink = null;
                        xmlBlocLink = _xmlUserMenu.CreateElement("blocMenu");
                        _baseUserMenu.AppendChild(xmlBlocLink);

                        XmlAttribute xattFavLnk = _xmlUserMenu.CreateAttribute("favlnk");
                        xattFavLnk.Value = eResApp.GetRes(_pref, 349); // Liens Favoris
                        xmlBlocLink.Attributes.Append(xattFavLnk);

                        //Attribut type de bloc menu
                        XmlAttribute menuType = _xmlUserMenu.CreateAttribute("endmenutype");
                        menuType.Value = "favlnk";
                        xmlBlocLink.Attributes.Append(menuType);

                        #region FAVORIS V7 & specifs XRM

                        foreach (KeyValuePair<int, HrefLink> keyValue in navRightMenu.getFavoriteLink())
                        {
                            if (keyValue.Value.type == 6)
                            {
                                string sRegSpec = string.Concat("specif/", _pref.GetBaseName, "/(.*)$");

                                Regex regExp = new Regex(sRegSpec, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                                MatchCollection mc;

                                mc = regExp.Matches(keyValue.Value.href);
                                // Url de type Spécif 

                                int nSpecId = 0;

                                if (mc.Count == 1)
                                {
                                    xmlBlocLink.AppendChild(
                                        AddAction(keyValue.Value.label, "", string.Concat("exportToLinkToV7(", keyValue.Value.id, ",0,1 )")));
                                }
                                else if (int.TryParse(keyValue.Value.href, out nSpecId) && nSpecId > 0)
                                {
                                    eSpecif spec = eSpecif.GetSpecif(_pref, nSpecId);
                                    if (!spec.IsViewable)
                                        continue;

                                    #region Appel d'un rapport depuis un lien favori de la page d'accueil
                                    if (spec.Url.ToLower().Contains("reportid="))
                                    {

                                        string[] sParams = spec.Url.ToLower().Split('&');
                                        string[] sParam;
                                        int reportid = 0, reportType = 0, nTab = 0, fid = 0, nTabBkm = 0, bFile = 0;
                                        foreach (string s in sParams)
                                        {
                                            sParam = s.Split('=');
                                            if (sParam.Length == 2)
                                            {
                                                switch (sParam[0])
                                                {
                                                    case "reportid":
                                                        int.TryParse(sParam[1], out reportid);
                                                        break;
                                                    case "reporttype":
                                                        int.TryParse(sParam[1], out reportType);
                                                        break;
                                                    case "tab":
                                                        int.TryParse(sParam[1], out nTab);
                                                        break;
                                                    case "fid":
                                                        int.TryParse(sParam[1], out fid);
                                                        break;
                                                    case "tabbkm":
                                                        int.TryParse(sParam[1], out nTabBkm);
                                                        break;
                                                    case "bfile":
                                                        int.TryParse(sParam[1], out bFile);
                                                        break;
                                                    default:
                                                        break;
                                                }
                                            }
                                        }
                                        xmlBlocLink.AppendChild(
                                            AddAction(keyValue.Value.label, "", string.Concat("runReportFromGlobal(", reportid, ", ", reportType, ", ", nTab, ", ", fid, ", ", nTabBkm, ", ", bFile, ")")));
                                    }
                                    #endregion
                                    else if (spec.Source == eLibConst.SPECIF_SOURCE.SRC_XRM
                                        || spec.Source == eLibConst.SPECIF_SOURCE.SRC_EXT
                                        )
                                    {
                                        var sUrl = spec.GetRelativeUrlFromRoot(_pref);
                                        string sAction = "";
                                        switch (spec.OpenMode)
                                        {
                                            case eLibConst.SPECIF_OPENMODE.NEW_WINDOW:
                                                string sEncode = ExternalUrlTools.GetCryptEncode(string.Concat("sid=", spec.SpecifId));
                                                sAction = string.Concat("window.open(\"eSubmitTokenXRM.aspx?t=", sEncode, "\");");
                                                break;
                                            case eLibConst.SPECIF_OPENMODE.MODAL:
                                            case eLibConst.SPECIF_OPENMODE.HIDDEN:
                                                sAction = string.Concat("runSpec(\"", spec.SpecifId, "\");");
                                                break;
                                            case eLibConst.SPECIF_OPENMODE.UNSPECIFIED:
                                            case eLibConst.SPECIF_OPENMODE.IFRAME:
                                            default:
                                                break;
                                        }
                                        xmlBlocLink.AppendChild(
                                            AddAction(keyValue.Value.label, "", sAction));

                                    }


                                }
                                else
                                {
                                    xmlBlocLink.AppendChild(AddLink(
                                        keyValue.Value.label,
                                        "",
                                        keyValue.Value.href,
                                        keyValue.Value.target,
                                        keyValue.Value.prefix));
                                }


                            }
                            else
                                xmlBlocLink.AppendChild(AddLink(
                                        keyValue.Value.label,
                                        "",
                                        keyValue.Value.href,
                                        keyValue.Value.target,
                                        keyValue.Value.prefix));
                        }

                        #endregion
                    }
                    /********************************************************/
                    #endregion

                    #region LISTE DES THEMES
                    /******************  Bloc des thèmes ***********************/

                    if (mgrType != eConst.eFileType.ACCUEIL && !(mgrType == eConst.eFileType.ADMIN && rootParentModule == eUserOptionsModules.USROPT_MODULE.ADMIN))
                    {

                        //Recherche et ajout des theme
                        XmlNode xmlBlocThemes = null;
                        xmlBlocThemes = _xmlUserMenu.CreateElement("blocThemes");
                        _baseUserMenu.AppendChild(xmlBlocThemes);

                        XmlAttribute xattTheme = _xmlUserMenu.CreateAttribute("theme");
                        xattTheme.Value = eResApp.GetRes(_pref, 6853); //eResApp.GetRes(_pref.Lang, 1); // thème
                        xmlBlocThemes.Attributes.Append(xattTheme);

                        string sFileNameTheme = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "themes", "themes.xml");
                        if (File.Exists(sFileNameTheme))
                        {
                            XmlDocument doc = null;

                            try
                            {
                                doc = new XmlDocument();
                                doc.Load(sFileNameTheme);


                                foreach (XmlNode themeNode in doc.SelectNodes("/themes/theme"))
                                {
                                    ePrefLite.Theme newTheme = ePrefLite.Theme.GetTheme(themeNode);

                                    if ((newTheme == null)
                                        //Si le thème n'est pas compatible avec le navigateur on ne l'affiche pas
                                        || (newTheme.IsBrowserIncompatible(HttpContext.Current.Request.Browser.Type.ToLower(), HttpContext.Current.Request.UserAgent.ToLower()))
                                        //Si skin réservé à un client on ne l'affiche que pour le client
                                        || (newTheme.DataBase.Count > 0 && !newTheme.DataBase.Contains(_pref.GetBaseName)))
                                    {
                                        continue;
                                    }


                                    // On affiche que les thèmes de la même version de theme que celui en cours
                                    if (_pref.ThemeXRM.Version != newTheme.Version)
                                    {
                                        continue;
                                    }


                                    XmlNode _entry = _xmlUserMenu.CreateElement("theme");

                                    //Ajout du node Label => pour le innerText et le title de l'element
                                    XmlNode _label = _xmlUserMenu.CreateElement("label");

                                    string sThemeName = newTheme.Name;
                                    if (newTheme.ResId != null)
                                        sThemeName = eResApp.GetRes(_pref, newTheme.ResId ?? 0);

                                    _label.AppendChild(_xmlUserMenu.CreateTextNode(sThemeName));
                                    _entry.AppendChild(_label);

                                    // Ajout du node Color
                                    XmlNode _color = _xmlUserMenu.CreateElement("color");
                                    _color.AppendChild(_xmlUserMenu.CreateTextNode(newTheme.Color));
                                    _entry.AppendChild(_color);

                                    if (!string.IsNullOrEmpty(newTheme.Color2))
                                    {
                                        // Ajout du node Color2 (couleur secondaire)
                                        XmlNode _color2 = _xmlUserMenu.CreateElement("color2");
                                        _color2.AppendChild(_xmlUserMenu.CreateTextNode(newTheme.Color2));
                                        _entry.AppendChild(_color2);
                                    }


                                    //Ajout du node ClassName
                                    XmlNode _className = _xmlUserMenu.CreateElement("className");
                                    _className.AppendChild(_xmlUserMenu.CreateTextNode("themeThumbnail"));
                                    _entry.AppendChild(_className);

                                    // Ajout du node id
                                    XmlNode _id = _xmlUserMenu.CreateElement("id");
                                    _id.AppendChild(_xmlUserMenu.CreateTextNode(newTheme.Id.ToString()));
                                    _entry.AppendChild(_id);

                                    // Ajout de la font size max
                                    XmlNode maxFontSize = _xmlUserMenu.CreateElement("mxfont");
                                    if (newTheme.FontSizeMax != null && newTheme.FontSizeMax.Count() > 0)
                                        maxFontSize.AppendChild(_xmlUserMenu.CreateTextNode(newTheme.FontSizeMax.Join(";")));
                                    _entry.AppendChild(maxFontSize);

                                    // Ajout du node action
                                    XmlNode _action = _xmlUserMenu.CreateElement("action");
                                    _action.AppendChild(_xmlUserMenu.CreateTextNode(string.Concat("applyTheme(", newTheme.Id, ", ", _pref.UserId, ", applyThemeWithoutReload);switchActiveThemeThumbnail(this);")));
                                    _entry.AppendChild(_action);

                                    //theme actif
                                    XmlNode _active = _xmlUserMenu.CreateElement("activeTheme");
                                    _active.AppendChild(_xmlUserMenu.CreateTextNode(_pref.ThemeXRM.Id == newTheme.Id ? "1" : "0"));
                                    _entry.AppendChild(_active);

                                    xmlBlocThemes.AppendChild(_entry);
                                }
                            }
                            catch { }   // TODO
                            finally
                            {
                                doc = null;
                            }
                        }

                        /********************************************************/
                    }
                    #endregion

                    #region OPTIONS UTILISATEUR/ADMINISTRATION
                    if (mgrType == eConst.eFileType.ADMIN)
                    {
                        #region Liens spécifiques à chaque module d'administration, en haut du menu

                        XmlNode xmlBlocLink = null;
                        xmlBlocLink = _xmlUserMenu.CreateElement("blocMenu");

                        //Attribut type de bloc menu
                        _menuType = _xmlUserMenu.CreateAttribute("endmenutype");
                        _menuType.Value = "rightMenuItem";
                        xmlBlocLink.Attributes.Append(_menuType);

                        // Nouveau
                        switch (targetModule)
                        {
                            case eUserOptionsModules.USROPT_MODULE.ADMIN_TABS:
                                xmlBlocLink.AppendChild(AddAction(eResApp.GetRes(_pref, 31), "icon-add", "nsAdmin.addNewTab();"));
                                break;
                            case eUserOptionsModules.USROPT_MODULE.ADMIN_HOME:
                            case eUserOptionsModules.USROPT_MODULE.ADMIN_HOME_V7_HOMEPAGES:
                            case eUserOptionsModules.USROPT_MODULE.ADMIN_HOME_XRM_HOMEPAGES:
                                // L'ajout d'une page d'accueil passe par le nouveau système
                                xmlBlocLink.AppendChild(AddAction(eResApp.GetRes(_pref, 31), "icon-add", "oGridController.page.new();"));

                                break;
                            case eUserOptionsModules.USROPT_MODULE.ADMIN_HOME_EXPRESS_MESSAGE:
                                xmlBlocLink.AppendChild(AddAction(eResApp.GetRes(_pref, 31), "icon-add", "nsAdminHomepages.addNewExpressMessage();"));
                                break;
                            default:
                                break;
                        }

                        _baseUserMenu.AppendChild(xmlBlocLink);

                        #endregion

                        #region Liens communs à l'ensemble de la zone "Administration" ou "Options utilisateur"

                        #region Lien chapeau/titre "Administration" ou "Options utilisateur" selon le contexte
                        // On affiche tout d'abord le lien titre Options utilisateur (celui du bas sera masqué par l'ajout d'un attribut hideuseroptions à destination du XSL, voir plus bas)
                        XmlNode _blocUserOptions = _xmlUserMenu.CreateElement("blocTitle");
                        string link = string.Concat("loadUserOption('", rootParentModule, "')");
                        if (rootParentModule == eUserOptionsModules.USROPT_MODULE.ADMIN)
                            link = string.Concat("nsAdmin.loadAdminModule('", rootParentModule, "')");
                        _blocUserOptions.AppendChild(AddAction(eUserOptionsModules.GetModuleLabel(rootParentModule, _pref), string.Concat("icon-", eUserOptionsModules.GetModuleIcon(rootParentModule)), link, sTooltip: eResApp.GetRes(_pref, 7204)));
                        _baseUserMenu.AppendChild(_blocUserOptions);
                        #endregion

                        bool bIsExtensionsList = targetModule == eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS;
                        bool bIsExtensionsFile = eUserOptionsModules.GetModuleParent(targetModule, false) == eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS;

                        //Nouveau Menu EudoStore
                        if (eAdminExtension.IsNewStore && (bIsExtensionsList || bIsExtensionsFile))
                        {
                            //TODO - Unifier les appels à GetConfigDefault
                            Dictionary<eLibConst.CONFIG_DEFAULT, string> dicoConfig = _pref.GetConfigDefault(new eLibConst.CONFIG_DEFAULT[] { eLibConst.CONFIG_DEFAULT.VERSION });

                            XmlNode xBlockStore = _xmlUserMenu.CreateElement("storeBlock");
                            _baseUserMenu.AppendChild(xBlockStore);

                            XmlAttribute xShowStoreMenu = _xmlUserMenu.CreateAttribute("showStoreMenu");
                            xShowStoreMenu.Value = "1";
                            xBlockStore.Attributes.Append(xShowStoreMenu);


                            #region Infos Version
                            XmlNode xInfoVersionLabel = _xmlUserMenu.CreateElement("infoVersionLabel");
                            xInfoVersionLabel.InnerText = eResApp.GetRes(_pref, 2249).Replace("<VERSION>", dicoConfig[eLibConst.CONFIG_DEFAULT.VERSION]).Replace("<OFFER>", _pref.ClientInfos.ClientOffer.ToString().ToCapitalize());
                            xBlockStore.AppendChild(xInfoVersionLabel);

                            /* En attente de spec du PO
                            XmlNode xInfoVersionTitle = _xmlUserMenu.CreateElement("infoVersionTitle");
                            xInfoVersionTitle.InnerText = "Vous avez 2 extensions actives sur 22 disponibles";
                            xBlockStore.AppendChild(xInfoVersionTitle);
                            */
                            #endregion

                            //Mode liste des extensions
                            if (bIsExtensionsList)
                            {
                                #region RES

                                XmlNode xCategoryTitleLabel = _xmlUserMenu.CreateElement("categoryTitleLabel");
                                xCategoryTitleLabel.InnerText = string.Concat(eResApp.GetRes(_pref, 7998), " :");
                                xBlockStore.AppendChild(xCategoryTitleLabel);

                                XmlNode xOfferFilterTitleLabel = _xmlUserMenu.CreateElement("offerFilterTitleLabel");
                                xOfferFilterTitleLabel.InnerText = string.Concat(eResApp.GetRes(_pref, 2250), " :");
                                xBlockStore.AppendChild(xOfferFilterTitleLabel);

                                XmlNode xOtherFilterTitleLabel = _xmlUserMenu.CreateElement("otherFilterTitleLabel");
                                xOtherFilterTitleLabel.InnerText = string.Concat(eResApp.GetRes(_pref, 2251), " :");
                                xBlockStore.AppendChild(xOtherFilterTitleLabel);

                                XmlNode xStatusFilterTitleLabel = _xmlUserMenu.CreateElement("statusFilterTitleLabel");
                                xStatusFilterTitleLabel.InnerText = string.Concat(eResApp.GetRes(_pref, 2252), " :");
                                xBlockStore.AppendChild(xStatusFilterTitleLabel);


                                #region Filtres Offre
                                /* filtre type offre */
                                XmlNode xOfferListe = _xmlUserMenu.CreateElement("OfferListe");
                                xBlockStore.AppendChild(xOfferListe);


                                IEnumerable<eLibConst.ClientOffer> lstOffers = ((eLibConst.ClientOffer[])Enum.GetValues(typeof(eLibConst.ClientOffer))).ToList()

                                        .Where(off => off != eLibConst.ClientOffer.XRM) // pas xrm  
                                        .OrderBy(off => eLibTools.SortClientOffer(off));      //trié par "niveau de l'offre"

                                foreach (eLibConst.ClientOffer offer in lstOffers)
                                {
                                    //images en fonction de l'offre - si pas d'image, on passe l'entrée
                                    Image img = eAdminStoreRenderer.GetOfferImage(offer, _pref);
                                    if (img == null)
                                        continue;

                                    //Offre
                                    XmlNode xMyOffer = _xmlUserMenu.CreateElement("Offer");
                                    xOfferListe.AppendChild(xMyOffer);

                                    //Enum
                                    XmlAttribute xOfferType = _xmlUserMenu.CreateAttribute("xOfferType");
                                    xOfferType.Value = offer.ToString().ToCapitalize();
                                    xMyOffer.Attributes.Append(xOfferType);

                                    //Label
                                    XmlAttribute xOfferLabel = _xmlUserMenu.CreateAttribute("xOfferLabel");
                                    xMyOffer.Attributes.Append(xOfferLabel);
                                    xOfferLabel.Value = eResApp.GetRes(_pref, 2315).Replace("<OFFER>", offer.ToString().ToCapitalize());

                                    //Altt
                                    XmlAttribute xOfferAlt = _xmlUserMenu.CreateAttribute("xOfferAlt");
                                    xMyOffer.Attributes.Append(xOfferAlt);
                                    xOfferLabel.Value = eResApp.GetRes(_pref, 2316).Replace("<OFFER>", offer.ToString().ToCapitalize());

                                    //Img URL
                                    XmlAttribute xOfferImg = _xmlUserMenu.CreateAttribute("xOfferImg");
                                    xMyOffer.Attributes.Append(xOfferImg);
                                    xOfferImg.Value = img.ImageUrl;

                                    //Img CSS
                                    XmlAttribute xOfferCSS = _xmlUserMenu.CreateAttribute("xOfferCSS");
                                    xMyOffer.Attributes.Append(xOfferCSS);
                                    xOfferCSS.Value = img.CssClass;

                                    //IdCat
                                    XmlAttribute xOfferCatId = _xmlUserMenu.CreateAttribute("xOfferCatId");
                                    xMyOffer.Attributes.Append(xOfferCatId);
                                    xOfferCatId.Value = ((int)ExtEnum.MapProductClientOffer(offer)).ToString();
                                }
                                #endregion

                                #region filtres autres critères

                                //
                                XmlNode xOtherFilterFreeTitle = _xmlUserMenu.CreateElement("otherFilterFreeTitle");
                                xOtherFilterFreeTitle.InnerText = eResApp.GetRes(_pref, 2317);
                                xBlockStore.AppendChild(xOtherFilterFreeTitle);

                                XmlNode xOtherFilterFreeLabel = _xmlUserMenu.CreateElement("otherFilterFreeLabel");
                                xOtherFilterFreeLabel.InnerText = eResApp.GetRes(_pref, 2253);
                                xBlockStore.AppendChild(xOtherFilterFreeLabel);

                                XmlNode xOtherFilterCompatibleTitle = _xmlUserMenu.CreateElement("otherFilterCompatibleTitle");
                                xOtherFilterCompatibleTitle.InnerText = eResApp.GetRes(_pref, 2318);
                                xBlockStore.AppendChild(xOtherFilterCompatibleTitle);

                                XmlNode xOtherFilterCompatibleLabel = _xmlUserMenu.CreateElement("otherFilterCompatibleLabel");
                                xOtherFilterCompatibleLabel.InnerText = eResApp.GetRes(_pref, 2254);
                                xBlockStore.AppendChild(xOtherFilterCompatibleLabel);

                                XmlNode xOtherFilterNewTitle = _xmlUserMenu.CreateElement("otherFilterNewTitle");
                                xOtherFilterNewTitle.InnerText = eResApp.GetRes(_pref, 2319);
                                xBlockStore.AppendChild(xOtherFilterNewTitle);

                                XmlNode xOtherFilterNewLabel = _xmlUserMenu.CreateElement("otherFilterNewLabel");
                                xOtherFilterNewLabel.InnerText = eResApp.GetRes(_pref, 1286);
                                xBlockStore.AppendChild(xOtherFilterNewLabel);


                                XmlNode xStatusAllTitle = _xmlUserMenu.CreateElement("statusAllTitle");
                                xStatusAllTitle.InnerText = eResApp.GetRes(_pref, 2320);
                                xBlockStore.AppendChild(xStatusAllTitle);

                                XmlNode xStatusAllLabel = _xmlUserMenu.CreateElement("statusAllLabel");
                                xStatusAllLabel.InnerText = eResApp.GetRes(_pref, 5059);
                                xBlockStore.AppendChild(xStatusAllLabel);

                                XmlNode xStatusInstalledTitle = _xmlUserMenu.CreateElement("statusInstalledTitle");
                                xStatusInstalledTitle.InnerText = eResApp.GetRes(_pref, 2321);
                                xBlockStore.AppendChild(xStatusInstalledTitle);

                                XmlNode xStatusIconInstalledTitle = _xmlUserMenu.CreateElement("statusIconInstalledTitle");
                                xStatusIconInstalledTitle.InnerText = eResApp.GetRes(_pref, 2311);
                                xBlockStore.AppendChild(xStatusIconInstalledTitle);

                                XmlNode xStatusInstalledLabel = _xmlUserMenu.CreateElement("statusInstalledLabel");
                                xStatusInstalledLabel.InnerText = eResApp.GetRes(_pref, 2256);
                                xBlockStore.AppendChild(xStatusInstalledLabel);

                                XmlNode xStatusInstallingTitle = _xmlUserMenu.CreateElement("statusInstallingTitle");
                                xStatusInstallingTitle.InnerText = eResApp.GetRes(_pref, 2322);
                                xBlockStore.AppendChild(xStatusInstallingTitle);

                                XmlNode xStatusIconInstallingTitle = _xmlUserMenu.CreateElement("statusIconInstallingTitle");
                                xStatusIconInstallingTitle.InnerText = eResApp.GetRes(_pref, 2309);
                                xBlockStore.AppendChild(xStatusIconInstallingTitle);

                                XmlNode xStatusInstallingLabel = _xmlUserMenu.CreateElement("statusInstallingLabel");
                                xStatusInstallingLabel.InnerText = eResApp.GetRes(_pref, 2257);
                                xBlockStore.AppendChild(xStatusInstallingLabel);

                                XmlNode xStatusProposedTitle = _xmlUserMenu.CreateElement("statusProposedTitle");
                                xStatusProposedTitle.InnerText = eResApp.GetRes(_pref, 2323);
                                xBlockStore.AppendChild(xStatusProposedTitle);

                                XmlNode xStatusProposedLabel = _xmlUserMenu.CreateElement("statusProposedLabel");
                                xStatusProposedLabel.InnerText = eResApp.GetRes(_pref, 2258);
                                xBlockStore.AppendChild(xStatusProposedLabel);


                                XmlNode xCategoryNoneLabel = _xmlUserMenu.CreateElement("categoryNoneLabel");
                                xCategoryNoneLabel.InnerText = string.Concat("<", eResApp.GetRes(_pref, 238), ">");
                                xBlockStore.AppendChild(xCategoryNoneLabel);

                                XmlNode xCategoryAllLabel = _xmlUserMenu.CreateElement("categoryAllLabel");
                                xCategoryAllLabel.InnerText = eResApp.GetRes(_pref, 435).ToUpper();
                                xBlockStore.AppendChild(xCategoryAllLabel);

                                #endregion

                                #region Catalogue Catégories

                                XmlNode xStoreCategories = _xmlUserMenu.CreateElement("storeCategories");
                                xBlockStore.AppendChild(xStoreCategories);

                                IDictionary<string, string> categories = GetStoreCategories();
                                foreach (KeyValuePair<string, string> category in categories)
                                {
                                    XmlNode xStoreCategory = _xmlUserMenu.CreateElement("category");
                                    xStoreCategories.AppendChild(xStoreCategory);

                                    XmlNode xCategoryValue = _xmlUserMenu.CreateElement("value");
                                    xCategoryValue.InnerText = category.Key;
                                    xStoreCategory.AppendChild(xCategoryValue);

                                    XmlNode xCategoryLabel = _xmlUserMenu.CreateElement("label");
                                    xCategoryLabel.InnerText = category.Value;
                                    xStoreCategory.AppendChild(xCategoryLabel);
                                }

                                #endregion
                            }

                            //Mode fiche des extensions
                            if (bIsExtensionsFile)
                            {
                                XmlAttribute xShowStoreFileMenu = _xmlUserMenu.CreateAttribute("showStoreFileMenu");
                                xShowStoreFileMenu.Value = "1";
                                xBlockStore.Attributes.Append(xShowStoreFileMenu);

                                XmlNode xBackToListLabel = _xmlUserMenu.CreateElement("backToListLabel");
                                xBackToListLabel.InnerText = eResApp.GetRes(_pref, 2262);
                                xBlockStore.AppendChild(xBackToListLabel);
                            }
                        }
                        else
                        {
                            #region Liste automatique des modules de la zone d'administration ou d'options utilisateur
                            // Liste des sections principales à afficher dans le menu
                            // Les sous-sections seront ajoutées plus bas
                            modules = new List<eUserOptionsModules.USROPT_MODULE>();
                            if (rootParentModule == eUserOptionsModules.USROPT_MODULE.ADMIN)
                            {
                                modules.Add(eUserOptionsModules.USROPT_MODULE.ADMIN_GENERAL);
                                modules.Add(eUserOptionsModules.USROPT_MODULE.ADMIN_ACCESS);
                                modules.Add(eUserOptionsModules.USROPT_MODULE.ADMIN_TABS);
                                modules.Add(eUserOptionsModules.USROPT_MODULE.ADMIN_HOME);
                                modules.Add(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS);
                                modules.Add(eUserOptionsModules.USROPT_MODULE.ADMIN_DASHBOARD);

                                if (_pref.User.UserLevel >= (int)UserLevel.LEV_USR_SUPERADMIN)
                                    modules.Add(eUserOptionsModules.USROPT_MODULE.ADMIN_ORM);
                            }
                            else
                            {
                                modules.Add(eUserOptionsModules.USROPT_MODULE.HOME);
                                modules.Add(eUserOptionsModules.USROPT_MODULE.PREFERENCES);
                                modules.Add(eUserOptionsModules.USROPT_MODULE.ADVANCED);
                            }

                            AddModules(modules, rootParentModule, targetModule);
                            #endregion
                        }
                        #endregion
                    }
                    #endregion

                    break;
                #endregion

                #region Mode Liste
                case eConst.eFileType.LIST:
                    //On affiche ou pas la carte
                    bCartoEnabled = eExtension.IsReady(_pref, ExtensionCode.CARTOGRAPHY) && eTools.BrowserSupportedByBing(_context.Request) && eFilemapPartner.IsCartoEnabled(_pref, nDescid);
                    xmlCartoDisplay.InnerText = bCartoEnabled ? "1" : "0";

                    // Ouverture de la cnx à la base
                    try
                    {
                        _dal.OpenDatabase();

                        /*  Droits de traitements sur la table */
                        infoTab = new RqParam();
                        infoTab.SetProcedure("xsp_getTableDescFromDescIdV2");
                        infoTab.AddInputParameter("@DescId", SqlDbType.Int, nDescid);
                        infoTab.AddInputParameter("@UserId", SqlDbType.Int, userId);
                        infoTab.AddInputParameter("@GroupId", SqlDbType.Int, groupId);
                        infoTab.AddInputParameter("@UserLevel", SqlDbType.Int, userLevel);
                        infoTab.AddInputParameter("@Lang", SqlDbType.VarChar, lang);

                        dtrTabTreatmentRight = _dal.Execute(infoTab, out sError);

                        //Gestion d'erreur
                        if (!string.IsNullOrEmpty(sError) || !dtrTabTreatmentRight.HasRows || !dtrTabTreatmentRight.Read())
                        {
                            _dal.CloseDatabase();

                            if (!string.IsNullOrEmpty(sError))
                            {
                                this.ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, string.Concat(eResApp.GetRes(_pref, 416)), sError);
                            }
                            else
                            {
                                this.ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 416), string.Concat("Table (", nDescid, ")  ou User (", userId, ") Inexistant"));
                            }
                            LaunchError();
                        }

                        #region CALENDRIER POUR LES PLANNINGS

                        eudoDAL dal = eLibTools.GetEudoDAL(_pref);
                        dal.OpenDatabase();
                        string err = string.Empty;
                        _tab.ExternalLoadInfo(dal, out err);


                        dal.CloseDatabase();
                        if (err.Length > 0)
                        {
                            this.ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 416), err);
                            LaunchError();
                        }


                        bool isPlanning = _tab.EdnType == EdnType.FILE_PLANNING;
                        bool isCalendarEnabled = isPlanning && _pref.GetPref(nDescid, ePrefConst.PREF_PREF.CALENDARENABLED).Equals("1");

                        EudoQuery.CalendarViewMode calViewMode = (EudoQuery.CalendarViewMode)eLibTools.GetNum(_pref.GetPref(nDescid, ePrefConst.PREF_PREF.VIEWMODE));
                        EudoQuery.CalendarTaskMode calTaskMode = (EudoQuery.CalendarTaskMode)eLibTools.GetNum(_pref.GetPref(nDescid, ePrefConst.PREF_PREF.CALENDARTASKMODE));
                        int nMenuUserId = eLibTools.GetNum(_pref.GetPref(nDescid, ePrefConst.PREF_PREF.MENUUSERID));


                        bool isCalendarGraphEnabled = isCalendarEnabled && (calViewMode == CalendarViewMode.VIEW_CAL_WORK_WEEK || calViewMode == CalendarViewMode.VIEW_CAL_MONTH
                                     || calViewMode == CalendarViewMode.VIEW_CAL_DAY_PER_USER || calViewMode == CalendarViewMode.VIEW_CAL_DAY);

                        if (isPlanning && isCalendarEnabled)
                        {
                            if (isCalendarGraphEnabled || calViewMode == CalendarViewMode.VIEW_CAL_TASK)
                            {
                                XmlNode _goList = _xmlUserMenu.CreateElement("blocTitle");
                                _goList.AppendChild(AddAction(eResApp.GetRes(_pref, 23), "icon-item_rem", string.Concat("setCalViewMode(", CalendarViewMode.VIEW_CAL_LIST.GetHashCode(), ")")));
                                _baseUserMenu.AppendChild(_goList);
                            }
                            else
                            {
                                XmlNode _goList = _xmlUserMenu.CreateElement("blocTitle");
                                _goList.AppendChild(AddAction(eResApp.GetRes(_pref, 1144), "icon-item_rem", string.Concat("setCalViewMode(", CalendarViewMode.VIEW_CAL_WORK_WEEK.GetHashCode(), ")")));
                                _baseUserMenu.AppendChild(_goList);
                            }
                        }
                        #endregion

                        IDictionary<eLibConst.TREATID, bool> globalRight = null;
                        try
                        {
                            globalRight = eLibDataTools.GetTreatmentGlobalRight(_dal, _pref.User);
                        }
                        catch (Exception e)
                        {
                            string msg = eResApp.GetRes(_pref, 6237);
                            string detail = string.Format("Chargement des droits globaux de la table ({0}) impossible.", nDescid);
                            string devMsg = e.Message + Environment.NewLine + e.StackTrace;
                            this.ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, msg, detail, "", devMsg);

                            LaunchError();
                        }

                        #region AJOUT
                        /*************** Nouvelle fiche : Mode liste et fiche  *************************/

                        // Mode Liste                    
                        if (dtrTabTreatmentRight.GetString("ADD_LIST_P") == "1")
                        {
                            XmlNode _blocReport = _xmlUserMenu.CreateElement("blocMenu");
                            /** Nouvelle ergo filoguidée G.L */
                            if (param.ParamTabNelleErgoGuided.Any(eg => eg == nDescid)
                                    && !new List<int> { TableType.PP.GetHashCode(), TableType.PM.GetHashCode() }.Any(elm => elm == nDescid))
                            {
                                string sJsAction = shFileInPopup(nDescid);
                                _blocReport.AppendChild(AddAction(eResApp.GetRes(_pref, 31), "icon-add", sJsAction));


                                #region ajout depuis purple

                                if (locationsPurpleActivated.Contains(LOCATION_PURPLE_ACTIVATED.MENU))
                                {
                                    //3005 Assistant création
                                    _blocReport.AppendChild(AddAction(eResApp.GetRes(_pref, 3005), "icon-add", String.Format("openPurpleFile({0},{1},{2},{3})", nDescid, 0, "''", (int)eConst.ShFileCallFrom.CallFromList)));
                                }

                                #endregion
                            }
                            else if (isPlanning)
                            {
                                _blocReport.AppendChild(AddAction(eResApp.GetRes(_pref, 31), "icon-add", string.Concat("showTplPlanning(", nDescid, ",0,null,'", eResApp.GetRes(_pref, 31), "') ")));
                            }
                            else if (_tab.TabType == TableType.PP || _tab.TabType == TableType.PM)
                            {
                                //SI PM ou PP on recherche avant de faire un ajout
                                _blocReport.AppendChild(AddAction(eResApp.GetRes(_pref, 31), "icon-add",
                                    string.Concat("openLnkFileDialog(",
                                    eFinderList.SearchType.Add.GetHashCode(), ",",
                                    nDescid, ",",
                                    "null ,",
                                    (int)eConst.ShFileCallFrom.CallFromNavBar, //le menu est considéré comme une navbar
                                    ") ")));

                                if (param.ParamTabNelleErgoGuided.Any(eg => eg == nDescid)
                                   && locationsPurpleActivated.Contains(LOCATION_PURPLE_ACTIVATED.MENU))
                                {
                                    //3005 Assistant création
                                    _blocReport.AppendChild(AddAction(eResApp.GetRes(_pref, 3005), "icon-add",
                                        string.Concat("openLnkFileDialog(", eFinderList.SearchType.Add.GetHashCode(), ",",
                                        nDescid, ",",
                                        "null ,",
                                        (int)eConst.ShFileCallFrom.CallFromMenuToPurple, //le menu est considéré comme une navbar
                                        ") ")));

                                }

                            }
                            else if (_tab.TabType == TableType.CAMPAIGN || _tab.TabType == TableType.UNSUBSCRIBEMAIL || _tab.TabType == TableType.BOUNCEMAIL || _tab.TabType == TableType.PJ
                                || _tab.TabType == TableType.PAYMENTTRANSACTION)
                            {
                                // dans le cas des campagne pas de bouton nouveau
                            }
                            // TOCHECK SMS
                            //SHA : bug bouton "Nouveau" masqué dans le menu de droit et visible dans le menu navbar (ajout FILE_TARGET) pour les cibles étendues
                            else if (_tab.EdnType == EdnType.FILE_MAIL || _tab.EdnType == EdnType.FILE_SMS || _tab.EdnType == EdnType.FILE_STANDARD || _tab.EdnType == EdnType.FILE_TARGET || _tab.EdnType == EdnType.FILE_MAIN
                                || ((_tab.EdnType == EdnType.FILE_USER || _tab.TabType == TableType.RGPDTREATMENTSLOGS) && _pref.User.UserLevel >= (int)UserLevel.LEV_USR_ADMIN)
                                )
                            {
                                string sJsAction = string.Empty;
                                if (_tab.AutoCreate)
                                {
                                    sJsAction = string.Concat("autoCreate(", _tab.DescId, ")");
                                }
                                else
                                {
                                    sJsAction = shFileInPopup(nDescid);

                                }
                                _blocReport.AppendChild(AddAction(eResApp.GetRes(_pref, 31), "icon-add", sJsAction));



                            }

                            if (_blocReport.ChildNodes.Count > 0)
                            {
                                //Attribut type de bloc menu
                                _menuType = _xmlUserMenu.CreateAttribute("endmenutype");
                                _menuType.Value = "rightMenuItem";
                                _blocReport.Attributes.Append(_menuType);

                                _baseUserMenu.AppendChild(_blocReport);
                            }
                        }

                        /***************************************************************/
                        #endregion

                        #region MODIFIER VUE - RUBRIQUE
                        /********** MODIFIER LA VUE : mode liste ***********************/

                        XmlNode _blocReportView = _xmlUserMenu.CreateElement("blocMenu");
                        if (isPlanning && calViewMode == CalendarViewMode.VIEW_CAL_DAY_PER_USER)
                            _blocReportView.AppendChild(AddAction(eResApp.GetRes(_pref, 20), "icon-rubrique", string.Concat("setPlgCol(", nDescid, ");")));
                        else
                            _blocReportView.AppendChild(AddAction(eResApp.GetRes(_pref, 20), "icon-rubrique", string.Concat("setCol(", nDescid, ")")));

                        //Attribut type de bloc menu
                        _menuType = _xmlUserMenu.CreateAttribute("endmenutype");
                        _menuType.Value = "rightMenuItem";
                        _blocReportView.Attributes.Append(_menuType);

                        _baseUserMenu.AppendChild(_blocReportView);

                        /****************************************************************/
                        #endregion

                        #region IMPRIMER
                        if (globalRight[eLibConst.TREATID.PRINT] && _tab.DescId != (int)TableType.USER)
                        {
                            AddPrintEntry();
                        }
                        #endregion

                        bool actionMenuAllowed = true;
                        actionMenuAllowed = dtrTabTreatmentRight.GetString("ACTION_MENU_P") == "1";
                        XmlNode _blocSpecif = null;

                        if (actionMenuAllowed
                            && !isCalendarGraphEnabled
                            //&& _tab.TabType != TableType.CAMPAIGNSTATSADV
                            && _tab.TabType != TableType.PJ)
                        {
                            Dictionary<eLibConst.CONFIG_DEFAULT, string> config = _pref.GetConfigDefault(new eLibConst.CONFIG_DEFAULT[] { eLibConst.CONFIG_DEFAULT.EmailServerEnabled, eLibConst.CONFIG_DEFAULT.FaxEnabled, eLibConst.CONFIG_DEFAULT.VoicingEnabled, eLibConst.CONFIG_DEFAULT.HideLinkEmailing, eLibConst.CONFIG_DEFAULT.HideLinkExport });

                            if (_tab.TabType != TableType.RGPDTREATMENTSLOGS)
                            {
                                #region MENU COMMUNICATION
                                /*************** COMMUNICATION  *********************************/
                                /*  publipostage / emailing / faxing / voicing                    */

                                bool hasCommunication = false;
                                XmlNode _blocCommunication = _xmlUserMenu.CreateElement("blocMenu");

                                //Attribut type de bloc menu
                                _menuType = _xmlUserMenu.CreateAttribute("endmenutype");
                                _menuType.Value = "rightMenuSubMenuItem";
                                _blocCommunication.Attributes.Append(_menuType);

                                _title = _xmlUserMenu.CreateAttribute("title");
                                _title.Value = eResApp.GetRes(_pref, 6854);
                                _blocCommunication.Attributes.Append(_title);

                                _iImg = _xmlUserMenu.CreateAttribute("className");
                                _iImg.Value = "icon-email";
                                _blocCommunication.Attributes.Append(_iImg);

                                _onclick = _xmlUserMenu.CreateAttribute("onclick");
                                _onclick.Value = "nsAdmin.toggleSubMenuTitle(this)";
                                _blocCommunication.Attributes.Append(_onclick);

                                // droit de publipostage
                                // right 70 : publipostage HTML
                                // right 60 : publipostage Word
                                // right 80 : publipostage PDF
                                if (globalRight[eLibConst.TREATID.PUBLIPOSTAGE_HTML] || globalRight[eLibConst.TREATID.PUBLIPOSTAGE_WORD] || globalRight[eLibConst.TREATID.PUBLIPOSTAGE_PDF])
                                {
                                    hasCommunication = true;
                                    _blocCommunication.AppendChild(AddAction(eResApp.GetRes(_pref, 438), "icon-edn-next icnMnuChip", "reportList(" + TypeReport.MERGE.GetHashCode().ToString() + ",0)"));

                                }

                                if (_tab.TabType != TableType.CAMPAIGN)
                                {

                                    // TODO : Unifier les getconfigdefault pour ne faire qu'un seul appel !
                                    // droit de mailing
                                    if ((_tab.TabType == TableType.PP || _tab.TabType == TableType.PM || _tab.TabType == TableType.EVENT || _tab.TabType == TableType.ADR
                                        || (_tab.TabType == TableType.USER && _pref.User.UserLevel >= (int)UserLevel.LEV_USR_ADMIN)
                                        ) && config[eLibConst.CONFIG_DEFAULT.EmailServerEnabled] == "1"
                                        && globalRight[eLibConst.TREATID.EMAILING]
                                        && config[eLibConst.CONFIG_DEFAULT.HideLinkEmailing] != "1")
                                    {
                                        hasCommunication = true;
                                        _blocCommunication.AppendChild(AddAction(eResApp.GetRes(_pref, 14), "icon-edn-next icnMnuChip", string.Concat("AddMailing(", _tab.DescId, ", " + TypeMailing.MAILING_FROM_LIST.GetHashCode().ToString() + ");"),
                                            string.Concat("mailing_", _tab.DescId), "mailing"));
                                    }

                                    // droit de Faxing
                                    if (config[eLibConst.CONFIG_DEFAULT.FaxEnabled] == "1" && globalRight[eLibConst.TREATID.FAXING])
                                    {
                                        hasCommunication = true;
                                        _blocCommunication.AppendChild(AddAction(eResApp.GetRes(_pref, 604), "icon-edn-next icnMnuChip", "alert('faxing')"));

                                    }

                                    // droit de Voicing - Voicing non fait sur xrm, l'entrée est masquée
                                    if (config[eLibConst.CONFIG_DEFAULT.VoicingEnabled] == "1111" && globalRight[eLibConst.TREATID.VOICING])
                                    {
                                        hasCommunication = true;
                                        _blocCommunication.AppendChild(AddAction(eResApp.GetRes(_pref, 1152), "icon-edn-next icnMnuChip", "alert('voicing')"));

                                    }
                                }
                                // si au moins une entrée, ajout du bloc
                                if (hasCommunication)
                                    _baseUserMenu.AppendChild(_blocCommunication);
                                /****************************************************************/
                                #endregion
                            }


                            #region MENU ANALYSE
                            /*************** ANALYSE  ***************************************/
                            bool hasAnalyse = false;
                            XmlNode _blocAnalyse = _xmlUserMenu.CreateElement("blocMenu");

                            //Attribut type de bloc menu
                            _menuType = _xmlUserMenu.CreateAttribute("endmenutype");
                            _menuType.Value = "rightMenuSubMenuItem";
                            _blocAnalyse.Attributes.Append(_menuType);

                            _iImg = _xmlUserMenu.CreateAttribute("className");
                            _iImg.Value = "icon-stats";
                            _blocAnalyse.Attributes.Append(_iImg);

                            _title = _xmlUserMenu.CreateAttribute("title");
                            _title.Value = eResApp.GetRes(_pref, 6606);
                            _blocAnalyse.Attributes.Append(_title);

                            _onclick = _xmlUserMenu.CreateAttribute("onclick");
                            _onclick.Value = "nsAdmin.toggleSubMenuTitle(this)";
                            _blocAnalyse.Attributes.Append(_onclick);

                            // droit d'export
                            if (globalRight[eLibConst.TREATID.EXPORT] && config[eLibConst.CONFIG_DEFAULT.HideLinkExport] != "1")
                            {
                                hasAnalyse = true;
                                _blocAnalyse.AppendChild(AddAction(eResApp.GetRes(_pref, 6303), "icon-edn-next icnMnuChip", "reportList(" + TypeReport.EXPORT.GetHashCode().ToString() + ",0)",
                                    string.Concat("export_", _tab.DescId), "export"));

                            }

                            // droit d'impression
                            if (globalRight[eLibConst.TREATID.PRINT])
                            {
                                hasAnalyse = true;
                                _blocAnalyse.AppendChild(AddAction(eResApp.GetRes(_pref, 398), "icon-edn-next icnMnuChip", "reportList(" + TypeReport.PRINT.GetHashCode().ToString() + ",0)"));

                            }

                            // droit d'accès aux graphiques
                            if (globalRight[eLibConst.TREATID.GRAPHIQUE])
                            {
                                hasAnalyse = true;
                                _blocAnalyse.AppendChild(AddAction(eResApp.GetRes(_pref, 1005), "icon-edn-next icnMnuChip", string.Concat("reportList(", TypeReport.CHARTS.GetHashCode(), ",0);")));
                            }

                            // si au moins une entrée, ajout du bloc


                            if (hasAnalyse)
                                _baseUserMenu.AppendChild(_blocAnalyse);
                            /****************************************************************/
                            #endregion

                            #region TRAITEMENT
                            /*************** TRAITEMENT  ***************************************/

                            XmlNode _blocReport = _xmlUserMenu.CreateElement("blocMenu");

                            if (
                                    _tab.TabType != TableType.CAMPAIGN
                                && _tab.TabType != TableType.CAMPAIGNSTATSADV
                                && _tab.TabType != TableType.PAYMENTTRANSACTION

                                && dtrTabTreatmentRight.GetString("GLOBAL_LINK_P") == "1"
                                && (dtrTabTreatmentRight.GetString("MODIF_MULTI_P") == "1"
                                    || dtrTabTreatmentRight.GetString("DEL_MULTI_P") == "1"
                                    || dtrTabTreatmentRight.GetString("DUPLI_MULTI_P") == "1"))
                            {
                                StringBuilder actions = new StringBuilder();

                                //On ajoute que les actions sur laquelle on a les droits
                                if (dtrTabTreatmentRight.GetString("MODIF_MULTI_P") == "1")
                                {
                                    actions.Append("modify|");
                                }
                                if (dtrTabTreatmentRight.GetString("DEL_MULTI_P") == "1")
                                {
                                    actions.Append("delete|");
                                }
                                if (dtrTabTreatmentRight.GetString("DUPLI_MULTI_P") == "1")
                                {
                                    actions.Append("duplicate");
                                }

                                //[MOU 03/09/2013 cf. 22318] En mode liste sur PP ou PM, on affiche une fenetre intermediaire pour demander à l utilisateur de choisir 
                                //le fichier cible parmi (PP ou Adresse), (PM ou Adresse).
                                if (nDescid == TableType.PP.GetHashCode() || nDescid == TableType.PM.GetHashCode())
                                {
                                    _blocReport.AppendChild(
                                        AddAction(eResApp.GetRes(_pref, 295), "icon-lab", string.Concat("SetTargetTab(", TableType.ADR.GetHashCode(), ", ", nDescid, ", treatment)"),
                                    string.Concat("treatment_", _tab.DescId), actions.ToString()));
                                }
                                else if (nDescid != TableType.USER.GetHashCode())
                                {
                                    _blocReport.AppendChild(AddAction(eResApp.GetRes(_pref, 295), "icon-lab", "treatment(0)",
                                    string.Concat("treatment_", _tab.DescId), actions.ToString()));
                                }

                                bool importAllowed = dtrTabTreatmentRight.GetString("IMPORT_TAB_P") == "1";
                                bool addAllowed = dtrTabTreatmentRight.GetString("ADD_LIST_P") == "1";
                                bool updateAllowed = dtrTabTreatmentRight.GetString("MODIF_P") == "1";


                                //#57334 Droit d'import sur l'onglet necessite que la fonctionalité soit activée et avoir le droit sur l'ajout et la  modifi

                                if (!(_tab.DescId == (int)TableType.USER && _pref.User.UserLevel < (int)UserLevel.LEV_USR_ADMIN) // import user uniquement pour admin
                                    && importAllowed
                                    && addAllowed
                                    && updateAllowed
                                    && eFeaturesManager.IsFeatureAvailable(_pref, eConst.XrmFeature.Import))
                                {

                                    _blocReport.AppendChild(AddAction(eResApp.GetRes(_pref, 6340), "icon-import", string.Concat("oImportWizard.ShowTabWizard(", _tab.DescId, ");"),
                                       string.Concat("import_", _tab.DescId), actions.ToString()));
                                }

                                //  _blocReport.AppendChild(AddAction("Gantt Admin", "icon-tab", "widgetNS.GanttAdmin(1)", "5", ""));
                            }

                            if (bCartoEnabled)
                                _blocReport.AppendChild(AddAction(eResApp.GetRes(_pref, 7105), "icon-map-marker", "oCartography.openMap();", "6", ""));

                            if (_blocReport.HasChildNodes)
                            {
                                //Attribut type de bloc menu
                                _menuType = _xmlUserMenu.CreateAttribute("endmenutype");
                                _menuType.Value = "rightMenuItem";
                                _blocReport.Attributes.Append(_menuType);

                                _baseUserMenu.AppendChild(_blocReport);
                            }

                            /*******************************************************************/
                            #endregion

                            #region SPECIF
                            /*************  LIENS DES SPECIFS ****************************************/
                            if (_tab.TabType != TableType.CAMPAIGN
                                  && _tab.TabType != TableType.CAMPAIGNSTATSADV
                                )
                            {
                                #region Carto
                                //NBA 14-09-2012 BingMaps afficher le lien de la carte pour differentes tables
                                //GCH : reprise du code v7
                                string latitudefield, longitudefield;
                                int topOfCount;
                                bool mapsEnable = _pref.GetConfigDefault(new eLibConst.CONFIG_DEFAULT[] { eLibConst.CONFIG_DEFAULT.GOOGLEMAPSENABLED }, "0")
                                    [eLibConst.CONFIG_DEFAULT.GOOGLEMAPSENABLED].Equals("1");

                                if (mapsEnable)
                                {
                                    Geocodage.GetMapping(_pref, nDescid, out latitudefield, out longitudefield, out topOfCount, out sError);
                                    if (sError.Length > 0)
                                        throw new Exception(string.Concat("cartoliste - ", sError));
                                    if (latitudefield.Length > 0 && longitudefield.Length > 0)
                                    {
                                        if (_blocSpecif == null)
                                            _blocSpecif = _xmlUserMenu.CreateElement("blocMenu");

                                        _blocSpecif.AppendChild(
                                            AddAction(eResApp.GetRes(_pref, 7105), "icon-edn-next icnMnuChip", string.Concat("ShowCartoMulti(", nDescid, ", ", nFileId, ")")));    //cartographie

                                    }
                                }

                                // Freecode CRU : Carte de la France zonée (SVG)
                                IDictionary<eLibConst.CONFIGADV, string> configadv = eLibTools.GetConfigAdvValues(_pref,
                                    new HashSet<eLibConst.CONFIGADV> {
                                    eLibConst.CONFIGADV.ZONE_MAP_ENABLED
                                    });

                                string zoneMapEnabled = "0";
                                if (configadv.TryGetValue(eLibConst.CONFIGADV.ZONE_MAP_ENABLED, out zoneMapEnabled))
                                {
                                    if (zoneMapEnabled == "1")
                                    {
                                        if (_blocSpecif == null)
                                            _blocSpecif = _xmlUserMenu.CreateElement("blocMenu");

                                        _blocSpecif.AppendChild(
                                            AddAction("Répartition par départements", "icon-edn-next icnMnuChip", string.Concat("ShowCartoSVG(", nDescid, ")")));
                                    }
                                }
                                #endregion

                                // TODO SPH : PAS DE SQL DANS LES MANAGERS
                                // les liens spécifs génère en fait un post vers la spécif avec des paramètres additionnels
                                RqParam rSpecif = new RqParam();

                                rSpecif.SetQuery("SELECT PREFID, URL,URLNAME,URLPARAM FROM [PREF] WHERE USERID=0 AND TAB=@TAB AND URL<>''");
                                rSpecif.AddInputParameter("@TAB", SqlDbType.Int, nDescid);
                                DataTableReaderTuned dtrURL = _dal.Execute(rSpecif, out sError);
                                try
                                {

                                    if (string.IsNullOrEmpty(sError) && dtrURL.HasRows)
                                    {
                                        if (_blocSpecif == null)
                                            _blocSpecif = _xmlUserMenu.CreateElement("blocMenu");

                                        while (dtrURL.Read())
                                        {
                                            string label = dtrURL.GetString("urlname");
                                            int nURLID = dtrURL.GetEudoNumeric("PREFID");
                                            _blocSpecif.AppendChild(AddAction(label, "icon-edn-next icnMnuChip", string.Concat("exportToLinkToV7(", nURLID.ToString(), ",0,2)")));
                                        }

                                    }
                                }
                                finally
                                {
                                    if (dtrURL != null)
                                        dtrURL.Dispose();
                                }

                                #region Specifs V2 XRM
                                List<eSpecif> liSpecV2 = eSpecif.GetSpecifList(_pref, eLibConst.SPECIF_TYPE.TYP_LIST, nDescid);
                                foreach (eSpecif spec in liSpecV2)
                                {
                                    if (!spec.IsViewable)
                                        continue;
                                    string sAction = string.Empty;
                                    if (spec.Source == eLibConst.SPECIF_SOURCE.SRC_V7)
                                    {
                                        //les spécifs V7 sont toujours gérées depuis les pref (voir ci-dessus)
                                        continue;
                                    }

                                    #region Appel d'un rapport depuis le menu de droite (mode liste)
                                    if (spec.Url.ToLower().Contains("reportid="))
                                    {

                                        string[] sParams = spec.Url.ToLower().Split('&');
                                        string[] sParam;
                                        int reportid = 0, reportType = 0, nTab = 0, fid = 0, nTabBkm = 0, bFile = 0;
                                        foreach (string s in sParams)
                                        {
                                            sParam = s.Split('=');
                                            if (sParam.Length == 2)
                                            {
                                                switch (sParam[0])
                                                {
                                                    case "reportid":
                                                        int.TryParse(sParam[1], out reportid);
                                                        break;
                                                    case "reporttype":
                                                        int.TryParse(sParam[1], out reportType);
                                                        break;
                                                    //case "tab":
                                                    //    int.TryParse(sParam[1], out nTab);
                                                    //    break;
                                                    //case "fid":
                                                    //    int.TryParse(sParam[1], out fid);
                                                    //    break;
                                                    case "tabbkm":
                                                        int.TryParse(sParam[1], out nTabBkm);
                                                        break;
                                                    case "bfile":
                                                        int.TryParse(sParam[1], out bFile);
                                                        break;
                                                    default:
                                                        break;
                                                }
                                            }
                                        }
                                        sAction = string.Concat("runReportFromGlobal(", reportid, ", ", reportType, ", ", nDescid, ", ", 0, ", ", nTabBkm, ", ", 0, ")");
                                    }
                                    #endregion
                                    else if (spec.Source == eLibConst.SPECIF_SOURCE.SRC_XRM
                                        || spec.Source == eLibConst.SPECIF_SOURCE.SRC_EXT
                                        )
                                    {
                                        var sUrl = spec.GetRelativeUrlFromRoot(_pref);
                                        switch (spec.OpenMode)
                                        {
                                            case eLibConst.SPECIF_OPENMODE.NEW_WINDOW:
                                                string sEncode = ExternalUrlTools.GetCryptEncode(string.Concat("sid=", spec.SpecifId, "&tab=", nDescid, "&fid=", nFileId));
                                                sAction = string.Concat("window.open(\"eSubmitTokenXRM.aspx?t=", sEncode, "\");");
                                                break;
                                            case eLibConst.SPECIF_OPENMODE.MODAL:
                                            case eLibConst.SPECIF_OPENMODE.HIDDEN:
                                                sAction = string.Concat("runSpec(\"", spec.SpecifId, "\");");
                                                break;
                                            case eLibConst.SPECIF_OPENMODE.UNSPECIFIED:
                                            case eLibConst.SPECIF_OPENMODE.IFRAME:
                                            default:
                                                break;
                                        }
                                    }

                                    if (_blocSpecif == null)
                                        _blocSpecif = _xmlUserMenu.CreateElement("blocMenu");

                                    _blocSpecif.AppendChild(AddAction(spec.Label, "icon-edn-next icnMnuChip", sAction));
                                }
                                #endregion



                                if (_blocSpecif != null)
                                {
                                    //Attribut type de bloc menu
                                    _menuType = _xmlUserMenu.CreateAttribute("endmenutype");
                                    _menuType.Value = "rightMenuSubMenuItem lnkspecif";
                                    _blocSpecif.Attributes.Append(_menuType);

                                    _title = _xmlUserMenu.CreateAttribute("title");
                                    _title.Value = eResApp.GetRes(_pref, 1500);
                                    _blocSpecif.Attributes.Append(_title);

                                    _iImg = _xmlUserMenu.CreateAttribute("className");
                                    _iImg.Value = "icon-hyperlink";
                                    _blocSpecif.Attributes.Append(_iImg);

                                    _baseUserMenu.AppendChild(_blocSpecif);
                                }
                                /*************************************************************************/
                            }
                            #endregion
                        }



                        //BSE #52 239
                        // module d'administration pour les utilisateur et groupe
                        if (_pref.User.UserLevel >= UserLevel.LEV_USR_ADMIN.GetHashCode()
                            && (targetModule == eUserOptionsModules.USROPT_MODULE.ADMIN_ACCESS_USERGROUPS || targetModule == eUserOptionsModules.USROPT_MODULE.ADMIN_DASHBOARD_RGPDTREATMENTLOG))
                        {
                            modules = new List<eUserOptionsModules.USROPT_MODULE>();
                            modules.Add(eUserOptionsModules.USROPT_MODULE.ADMIN_GENERAL);
                            modules.Add(eUserOptionsModules.USROPT_MODULE.ADMIN_ACCESS);
                            modules.Add(eUserOptionsModules.USROPT_MODULE.ADMIN_TABS);
                            modules.Add(eUserOptionsModules.USROPT_MODULE.ADMIN_HOME);
                            modules.Add(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS);
                            modules.Add(eUserOptionsModules.USROPT_MODULE.ADMIN_DASHBOARD);
                            AddModules(modules, rootParentModule, targetModule);
                        }

                        #region Menu calendrier
                        if (isCalendarGraphEnabled)
                        {
                            //Affichage du calendrier
                            XmlNode nodeCalendar = _xmlUserMenu.CreateElement("blocMenu");
                            XmlAttribute attCalendar = _xmlUserMenu.CreateAttribute("calendar");
                            attCalendar.Value = "1";
                            nodeCalendar.Attributes.Append(attCalendar);
                            _baseUserMenu.AppendChild(nodeCalendar);


                            //nTab
                            attCalendar = _xmlUserMenu.CreateAttribute("tab");
                            attCalendar.Value = nDescid.ToString();
                            nodeCalendar.Attributes.Append(attCalendar);


                            //Working days
                            string workingDays = _pref.GetPref(nDescid, ePrefConst.PREF_PREF.CALENDARWORKINGDAYS);
                            attCalendar = _xmlUserMenu.CreateAttribute("wd");
                            attCalendar.Value = workingDays;
                            nodeCalendar.Attributes.Append(attCalendar);


                            //Calendar date

                            string calDate = _pref.GetPref(nDescid, ePrefConst.PREF_PREF.CALENDARDATE);
                            DateTime dCalDate = new DateTime();
                            DateTime.TryParse(calDate, out dCalDate);

                            attCalendar = _xmlUserMenu.CreateAttribute("year");
                            attCalendar.Value = dCalDate.Year.ToString();
                            nodeCalendar.Attributes.Append(attCalendar);


                            attCalendar = _xmlUserMenu.CreateAttribute("month");
                            attCalendar.Value = dCalDate.Month.ToString();
                            nodeCalendar.Attributes.Append(attCalendar);


                            attCalendar = _xmlUserMenu.CreateAttribute("day");
                            attCalendar.Value = dCalDate.Day.ToString();
                            nodeCalendar.Attributes.Append(attCalendar);

                            attCalendar = _xmlUserMenu.CreateAttribute("calmode");
                            attCalendar.Value = calViewMode.GetHashCode().ToString();
                            nodeCalendar.Attributes.Append(attCalendar);




                            //spécif mode calendar -> todo factoriser avec bloc standard
                            List<eSpecif> liSpecV2 = eSpecif.GetSpecifList(_pref, eLibConst.SPECIF_TYPE.TYP_SPECIF_CALENDAR_MODE, nDescid);
                            foreach (eSpecif spec in liSpecV2)
                            {
                                if (!spec.IsViewable)
                                    continue;
                                string sAction = string.Empty;
                                if (spec.Source == eLibConst.SPECIF_SOURCE.SRC_V7)
                                {
                                    //les spécifs V7 sont toujours gérées depuis les pref (voir ci-dessus)
                                    continue;
                                }

                                #region Appel d'un rapport depuis le menu de droite (mode liste)
                                if (spec.Url.ToLower().Contains("reportid="))
                                {

                                    string[] sParams = spec.Url.ToLower().Split('&');
                                    string[] sParam;
                                    int reportid = 0, reportType = 0, nTab = 0, fid = 0, nTabBkm = 0, bFile = 0;
                                    foreach (string s in sParams)
                                    {
                                        sParam = s.Split('=');
                                        if (sParam.Length == 2)
                                        {
                                            switch (sParam[0])
                                            {
                                                case "reportid":
                                                    int.TryParse(sParam[1], out reportid);
                                                    break;
                                                case "reporttype":
                                                    int.TryParse(sParam[1], out reportType);
                                                    break;
                                                //case "tab":
                                                //    int.TryParse(sParam[1], out nTab);
                                                //    break;
                                                //case "fid":
                                                //    int.TryParse(sParam[1], out fid);
                                                //    break;
                                                case "tabbkm":
                                                    int.TryParse(sParam[1], out nTabBkm);
                                                    break;
                                                case "bfile":
                                                    int.TryParse(sParam[1], out bFile);
                                                    break;
                                                default:
                                                    break;
                                            }
                                        }
                                    }
                                    sAction = string.Concat("runReportFromGlobal(", reportid, ", ", reportType, ", ", nDescid, ", ", 0, ", ", nTabBkm, ", ", 0, ")");
                                }
                                #endregion
                                else if (spec.Source == eLibConst.SPECIF_SOURCE.SRC_XRM
                                    || spec.Source == eLibConst.SPECIF_SOURCE.SRC_EXT
                                    )
                                {
                                    var sUrl = spec.GetRelativeUrlFromRoot(_pref);
                                    switch (spec.OpenMode)
                                    {
                                        case eLibConst.SPECIF_OPENMODE.NEW_WINDOW:

                                            string sEncode = ExternalUrlTools.GetCryptEncode(string.Concat("sid=", spec.SpecifId, "&tab=", nDescid, "&fid=", nFileId));
                                            sAction = string.Concat("window.open(\"eSubmitTokenXRM.aspx?t=", sEncode, "\");");
                                            break;
                                        case eLibConst.SPECIF_OPENMODE.MODAL:
                                        case eLibConst.SPECIF_OPENMODE.HIDDEN:
                                            sAction = string.Concat("runSpec(\"", spec.SpecifId, "\");");
                                            break;
                                        case eLibConst.SPECIF_OPENMODE.UNSPECIFIED:
                                        case eLibConst.SPECIF_OPENMODE.IFRAME:
                                        default:
                                            break;
                                    }
                                }


                                if (_blocSpecif == null)
                                    _blocSpecif = _xmlUserMenu.CreateElement("blocMenu");

                                _blocSpecif.AppendChild(AddAction(spec.Label, "icon-edn-next icnMnuChip", sAction));
                            }



                            if (_blocSpecif != null)
                            {
                                //Attribut type de bloc menu
                                _menuType = _xmlUserMenu.CreateAttribute("endmenutype");
                                _menuType.Value = "rightMenuSubMenuItem lnkspecif";
                                _blocSpecif.Attributes.Append(_menuType);

                                _title = _xmlUserMenu.CreateAttribute("title");
                                _title.Value = eResApp.GetRes(_pref, 1500);
                                _blocSpecif.Attributes.Append(_title);

                                _iImg = _xmlUserMenu.CreateAttribute("className");
                                _iImg.Value = "icon-hyperlink";
                                _blocSpecif.Attributes.Append(_iImg);

                                _baseUserMenu.AppendChild(_blocSpecif);
                            }
                        }
                        #endregion
                    }
                    finally
                    {
                        // Libération des ressources
                        if (dtrTabTreatmentRight != null)
                            dtrTabTreatmentRight.Dispose();

                        // Fermeture de la connexion
                        _dal.CloseDatabase();
                    }
                    break;

                #endregion

                #region Mode Fiche
                // TODO VERIFIER LES INFOS A AFFICHER OU A NE PAS AFFICHER EN MODE FICHE
                case eConst.eFileType.FILE_CONSULT:
                case eConst.eFileType.FILE_CREA:
                case eConst.eFileType.FILE_MODIF:

                    // Ouverture de la cnx à la base
                    _dal = eLibTools.GetEudoDAL(_pref);
                    _dal.OpenDatabase();



                    _tab = new TableLite(nDescid);
                    string errorLoad = string.Empty;
                    _tab.ExternalLoadInfo(_dal, out errorLoad);

                    if (errorLoad.Length > 0)
                        throw _dal.InnerException ?? new EudoException(errorLoad);

                    bool isTemplate = _tab.TabType == TableType.TEMPLATE;

                    // Onglets

                    infoTab = new RqParam();
                    /*  Droits de traitements sur la table */
                    infoTab = new RqParam();
                    infoTab.SetProcedure("xsp_getTableDescFromDescIdV2");
                    infoTab.AddInputParameter("@DescId", SqlDbType.Int, nDescid);
                    infoTab.AddInputParameter("@UserId", SqlDbType.Int, userId);
                    infoTab.AddInputParameter("@GroupId", SqlDbType.Int, groupId);
                    infoTab.AddInputParameter("@UserLevel", SqlDbType.Int, userLevel);
                    infoTab.AddInputParameter("@Lang", SqlDbType.VarChar, lang);

                    dtrTabTreatmentRight = _dal.Execute(infoTab, out sError);

                    try
                    {
                        //Gestion d'erreur
                        if (!string.IsNullOrEmpty(sError) || !dtrTabTreatmentRight.HasRows || !dtrTabTreatmentRight.Read())
                        {
                            _dal.CloseDatabase();

                            if (!string.IsNullOrEmpty(sError))
                                this.ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6237), sError);
                            else
                                this.ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6237), string.Concat("Table (", nDescid, ")  ou User (", userId, ") Inexistant"));

                            LaunchError();
                        }

                        IDictionary<eLibConst.TREATID, bool> globalRightFile = null;
                        try
                        {
                            globalRightFile = eLibDataTools.GetTreatmentGlobalRight(_dal, _pref.User);
                        }
                        catch (Exception e)
                        {
                            string msg = eResApp.GetRes(_pref, 6237);
                            string detail = string.Format("Chargement des droits globaux de la table ({0}) impossible.", nDescid);
                            string devMsg = e.Message + Environment.NewLine + e.StackTrace;
                            this.ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, msg, detail, "", devMsg);

                            LaunchError();
                        }

                        #region Navigation de fiche en fiche
                        XmlNode _browseFileBloc = _xmlUserMenu.CreateElement("browsingBloc");
                        _browseFileBloc.AppendChild(addBrowsingMenuElement());
                        _baseUserMenu.AppendChild(_browseFileBloc);
                        #endregion

                        //Ajout du titre 
                        XmlNode _blocTitle = _xmlUserMenu.CreateElement("blocTitle");

                        if (_pref.AdminMode && nDescid == (int)TableType.USER)
                            _blocTitle.AppendChild(AddAction(eResApp.GetRes(_pref, 23), "icon-item_rem", string.Concat("nsAdmin.loadAdminModule('ADMIN_ACCESS_USERGROUPS');")));
                        else
                            _blocTitle.AppendChild(AddAction(eResApp.GetRes(_pref, 23), "icon-item_rem", string.Concat("goTabList(", nDescid, ", true, null, goTabListFromDifferentTab(", nDescid, "), true);"), "goTabListRightMenuLink"));

                        _baseUserMenu.AppendChild(_blocTitle);


                        #region AJOUT
                        /*************** Nouvelle fiche : Mode fiche  *************************/
                        if (!isTemplate && dtrTabTreatmentRight.GetString("ADD_LIST_P") == "1")
                        {
                            XmlNode _blocReportAdd = _xmlUserMenu.CreateElement("blocMenu");

                            /** Nouvelle ergo filoguidée G.L */
                            if (param.ParamTabNelleErgoGuided.Any(eg => eg == nDescid)
                                    && !new List<int> { TableType.PP.GetHashCode(), TableType.PM.GetHashCode() }.Any(elm => elm == nDescid))
                            {
                                string sJsAction = shFileInPopup(nDescid);
                                _blocReportAdd.AppendChild(AddAction(eResApp.GetRes(_pref, 31), "icon-add", sJsAction));

                                if (_pref.ThemeXRM.Version > 1)
                                    if (locationsPurpleActivated.Contains(LOCATION_PURPLE_ACTIVATED.MENU))
                                    {
                                        _blocReportAdd.AppendChild(
                                            AddAction(
                                                eResApp.GetRes(_pref, 3005), //Assistant de création
                                                "icon-add",
                                                String.Format("openPurpleFile({0},{1},{2},{3})", nDescid, 0, "''", (int)eConst.ShFileCallFrom.CallFromMenuToPurple)
                                                )
                                            );
                                    }


                            }

                            else if (eLibTools.GetTabFromDescId(nDescid) == TableType.PP.GetHashCode() || eLibTools.GetTabFromDescId(nDescid) == TableType.PM.GetHashCode())
                            {
                                //SI PM ou PP on recherche avant de faire un ajout
                                _blocReportAdd.AppendChild(AddAction(eResApp.GetRes(_pref, 31), "icon-add",
                                    string.Concat("openLnkFileDialog(", eFinderList.SearchType.Add.GetHashCode(), ",",
                                    nDescid, ",",
                                    "null ,",
                                    (int)eConst.ShFileCallFrom.CallFromNavBar, //le menu est considéré comme une navbar
                                    ") ")));

                                if (_pref.ThemeXRM.Version > 1)
                                    if (locationsPurpleActivated.Contains(LOCATION_PURPLE_ACTIVATED.MENU))
                                    {
                                        //3005 Assistant création
                                        _blocReportAdd.AppendChild(AddAction(eResApp.GetRes(_pref, 3005), "icon-add",
                                            string.Concat("openLnkFileDialog(", eFinderList.SearchType.Add.GetHashCode(), ",",
                                            nDescid, ",",
                                            "null ,",
                                            (int)eConst.ShFileCallFrom.CallFromMenuToPurple, //le menu est considéré comme une navbar
                                            ") ")));

                                    }


                            }
                            else if (eLibTools.GetTabFromDescId(nDescid) == TableType.CAMPAIGN.GetHashCode() || eLibTools.GetTabFromDescId(nDescid) == TableType.PAYMENTTRANSACTION.GetHashCode())
                            {
                                //pas de bouton nouveau pour les campagnes
                            }
                            else
                            {
                                //Sur les fichiers de type Principal (Event), pas de bouton Appliquer/Fermer, seulement Valider
                                string strApplyCloseOnly = "false";
                                if (_tab.EdnType == EdnType.FILE_MAIN)
                                    strApplyCloseOnly = "true";

                                string sJsAction = string.Empty;

                                if (_tab.AutoCreate)
                                    sJsAction = string.Concat("autoCreate(", nDescid, ")");
                                else
                                {
                                    if (_tab.TabType != TableType.USER)
                                    {
                                        // #41277 : Modification paramètre nCallFrom 3 -> 1 pour ne pas garder de liaison PP/PM
                                        sJsAction = string.Concat("shFileInPopup(", nDescid, ",0, '", eResApp.GetRes(_pref, 31).Replace("'", @"\'"), "',null,null,0,'',", strApplyCloseOnly, ",null,1)");
                                    }
                                    else
                                    {
                                        sJsAction = string.Concat("shFileInPopup(", nDescid, ", 0, '", eResApp.GetRes(_pref, 31).Replace("'", @"\'"), "', null, null , 0,  null, false , null, 2,null,null , nsAdminUsers.InitDefault) ");
                                    }
                                }
                                _blocReportAdd.AppendChild(AddAction(eResApp.GetRes(_pref, 31), "icon-add", sJsAction));

                            }
                            if (_blocReportAdd.ChildNodes.Count > 0)
                            {
                                //Attribut type de bloc menu
                                _menuType = _xmlUserMenu.CreateAttribute("endmenutype");
                                _menuType.Value = "rightMenuItem";
                                _blocReportAdd.Attributes.Append(_menuType);

                                _baseUserMenu.AppendChild(_blocReportAdd);
                            }
                        }
                        /***************************************************************/
                        #endregion

                        #region MODIFICATION
                        if (!isTemplate && mgrType == eConst.eFileType.FILE_CONSULT)
                        {
                            // TODO tester les droits de modif
                            XmlNode _blocReportModfif = _xmlUserMenu.CreateElement("blocMenu");
                            _blocReportModfif.AppendChild(AddAction(eResApp.GetRes(_pref, 151), "eMain_modifier", string.Concat("loadFile(", nDescid, ",", nFileId, ",", eConst.eFileType.FILE_MODIF.GetHashCode(), ");")));

                            //Attribut type de bloc menu key.UserId
                            _menuType = _xmlUserMenu.CreateAttribute("endmenutype");
                            _menuType.Value = "rightMenuItem";
                            _blocReportModfif.Attributes.Append(_menuType);

                            _baseUserMenu.AppendChild(_blocReportModfif);
                        }
                        #endregion

                        #region DUPLIQUER

                        if (!isTemplate && dtrTabTreatmentRight.GetString("DUPLI_P") == "1" && _tab.TabType != TableType.CAMPAIGN && _tab.TabType != TableType.USER &&
                            _tab.TabType != TableType.PAYMENTTRANSACTION)
                        {
                            XmlNode _blocReportClone = _xmlUserMenu.CreateElement("blocMenu");
                            _blocReportClone.AppendChild(AddAction(eResApp.GetRes(_pref, 534), "icon-duplicate", string.Concat("shFileInPopup(", nDescid, ",", nFileId, ", '", eResApp.GetRes(_pref, 534).Replace("'", @"\'"), "',null,null,0,'',true,null,6)")));

                            //Attribut type de bloc menu
                            _menuType = _xmlUserMenu.CreateAttribute("endmenutype");
                            _menuType.Value = "rightMenuItem";
                            _blocReportClone.Attributes.Append(_menuType);

                            _baseUserMenu.AppendChild(_blocReportClone);
                        }
                        #endregion

                        #region SUPPRIMER
                        if (dtrTabTreatmentRight.GetString("DEL_P") == "1")
                        {

                            XmlNode _blocReportDel = _xmlUserMenu.CreateElement("blocMenu");
                            if (_pref.AdminMode && _tab.TabType == TableType.USER)
                            {

                                if (nFileId != _pref.UserId)
                                {

                                    _blocReportDel = _xmlUserMenu.CreateElement("blocMenu");

                                    //deleteFile(nTab, nFileId, eModFile, openSerie, successCallBack, bClose)


                                    _blocReportDel.AppendChild(AddAction(eResApp.GetRes(_pref, 19), "icon-delete",
                                        string.Concat("nsAdminUsers.userDelete(", nFileId, ")")));
                                    //Attribut type de bloc menu
                                    _menuType = _xmlUserMenu.CreateAttribute("endmenutype");
                                    _menuType.Value = "rightMenuItem";
                                    _blocReportDel.Attributes.Append(_menuType);

                                    _baseUserMenu.AppendChild(_blocReportDel);
                                }

                            }
                            else if(_tab.TabType != TableType.PAYMENTTRANSACTION)
                            {
                                _blocReportDel.AppendChild(AddAction(eResApp.GetRes(_pref, 19), "icon-delete", string.Concat("deleteFile('", nDescid, "', '", nFileId, "');")));

                                //Attribut type de bloc menu
                                _menuType = _xmlUserMenu.CreateAttribute("endmenutype");
                                _menuType.Value = "rightMenuItem";
                                _blocReportDel.Attributes.Append(_menuType);

                                _baseUserMenu.AppendChild(_blocReportDel);
                            }

                        }

                        #endregion


                        #region IMPRIMER
                        if (!isTemplate && globalRightFile[eLibConst.TREATID.PRINT] && _tab.DescId != (int)TableType.USER)
                        {
                            AddPrintEntry(true);
                        }
                        #endregion


                        // MCR 40359: masquer le menu a gauche pour le mode liste, si on est en mode fiche, ne pas prendre en compte la permission : "ACTION_MENU_P"
                        Dictionary<eLibConst.CONFIG_DEFAULT, string> config = _pref.GetConfigDefault(new eLibConst.CONFIG_DEFAULT[] { eLibConst.CONFIG_DEFAULT.EmailServerEnabled, eLibConst.CONFIG_DEFAULT.FaxEnabled, eLibConst.CONFIG_DEFAULT.VoicingEnabled, eLibConst.CONFIG_DEFAULT.HideLinkEmailing, eLibConst.CONFIG_DEFAULT.HideLinkExport });

                        if (!isTemplate)
                        {

                            #region MENU COMMUNICATION
                            /*************** COMMUNICATION  *********************************/
                            /*  publipostage / emailing / faxing / voicing                    */
                            if (_tab.TabType != TableType.USER && !isTemplate)
                            {
                                bool hasCommunication = false;
                                XmlNode _blocCommunication = _xmlUserMenu.CreateElement("blocMenu");

                                //Attribut type de bloc menu
                                _menuType = _xmlUserMenu.CreateAttribute("endmenutype");
                                _menuType.Value = "rightMenuSubMenuItem";
                                _blocCommunication.Attributes.Append(_menuType);

                                _title = _xmlUserMenu.CreateAttribute("title");
                                _title.Value = eResApp.GetRes(_pref, 6854); // communication

                                _blocCommunication.Attributes.Append(_title);

                                _iImg = _xmlUserMenu.CreateAttribute("className");
                                _iImg.Value = "icon-email";
                                _blocCommunication.Attributes.Append(_iImg);

                                _onclick = _xmlUserMenu.CreateAttribute("onclick");
                                _onclick.Value = "nsAdmin.toggleSubMenuTitle(this)";
                                _blocCommunication.Attributes.Append(_onclick);

                                // droit de publipostage
                                // #49366 : Test des droits de publipostage -> Si un des types de publipostage est autorisé, le lien est visible
                                if (globalRightFile[eLibConst.TREATID.PUBLIPOSTAGE_HTML] ||
                                    globalRightFile[eLibConst.TREATID.PUBLIPOSTAGE_WORD] ||
                                    globalRightFile[eLibConst.TREATID.PUBLIPOSTAGE_PDF])
                                {
                                    hasCommunication = true;
                                    _blocCommunication.AppendChild(AddAction(eResApp.GetRes(_pref, 438), "icon-edn-next icnMnuChip", "reportList(3,0);"));
                                }

                                // si au moins une entrée, ajout du bloc
                                if (hasCommunication)
                                    _baseUserMenu.AppendChild(_blocCommunication);

                                /****************************************************************/
                            }
                            #endregion

                            #region MENU ANALYSE
                            /*************** ANALYSE  ***************************************/

                            if (!isTemplate)
                            {
                                bool hasAnalyse = false;
                                XmlNode _blocAnalyse = _xmlUserMenu.CreateElement("blocMenu");

                                //Attribut type de bloc menu
                                _menuType = _xmlUserMenu.CreateAttribute("endmenutype");
                                _menuType.Value = "rightMenuSubMenuItem";
                                _blocAnalyse.Attributes.Append(_menuType);

                                _title = _xmlUserMenu.CreateAttribute("title");
                                _title.Value = eResApp.GetRes(_pref, 6606);
                                _blocAnalyse.Attributes.Append(_title);

                                _iImg = _xmlUserMenu.CreateAttribute("className");
                                _iImg.Value = "icon-stats";
                                _blocAnalyse.Attributes.Append(_iImg);

                                _onclick = _xmlUserMenu.CreateAttribute("onclick");
                                _onclick.Value = "nsAdmin.toggleSubMenuTitle(this)";
                                _blocAnalyse.Attributes.Append(_onclick);

                                // droit d'export
                                if (globalRightFile[eLibConst.TREATID.EXPORT] && config[eLibConst.CONFIG_DEFAULT.HideLinkExport] != "1")
                                {
                                    hasAnalyse = true;
                                    _blocAnalyse.AppendChild(AddAction(eResApp.GetRes(_pref, 6303), "icon-edn-next icnMnuChip", "reportList(2,0);"));
                                }

                                // droit d'impression
                                if (globalRightFile[eLibConst.TREATID.PRINT])
                                {
                                    hasAnalyse = true;
                                    _blocAnalyse.AppendChild(AddAction(eResApp.GetRes(_pref, 398), "icon-edn-next icnMnuChip", "reportList(0,0);"));
                                }

                                // droit d'accès aux graphiques
                                if (globalRightFile[eLibConst.TREATID.GRAPHIQUE])
                                {
                                    hasAnalyse = true;
                                    _blocAnalyse.AppendChild(AddAction(eResApp.GetRes(_pref, 1005), "icon-edn-next icnMnuChip", string.Concat("reportList(", TypeReport.CHARTS.GetHashCode(), ",0);")));
                                }

                                if (_pref.AdminMode && _tab.TabType == TableType.USER)
                                    hasAnalyse = false;

                                // si au moins une entrée, ajout du bloc
                                if (hasAnalyse)
                                    _baseUserMenu.AppendChild(_blocAnalyse);
                                /****************************************************************/
                            }

                            #endregion



                            if (_tab.TabType == TableType.USER)
                            {
                                #region TRAITEMENT

                                eUserInfo ui = new eUserInfo(nFileId, _dal);

                                //SHA : correction bug #71 997
                                //HLA : je généralise la correction #71997
                                if (_pref.User.UserLevel >= (int)UserLevel.LEV_USR_ADMIN
                                    && (_pref.User.UserLevel >= ui.UserLevel || _pref.User.UserLevel >= (int)UserLevel.LEV_USR_SUPERADMIN))
                                {
                                    AddActionEntry(eResApp.GetRes(_pref, 445), "icon-duplicate", eResApp.GetRes(_pref, 7956), "nsAdminUsers.replacePref(" + nFileId + ")"); // préférences
                                    AddActionEntry(eResApp.GetRes(_pref, 2), "icon-key2", "", "nsAdminUsers.chgPwd(" + nFileId + ")"); //Mot de passe
                                    AddActionEntry(eResApp.GetRes(_pref, 235), "icon-sticky-note", "", "nsAdminUsers.chgMemo(" + nFileId + ")"); // chg mémo
                                    AddActionEntry(eResApp.GetRes(_pref, 575), "icon-pen", "", "nsAdminUsers.chgSig(" + nFileId + ")"); // chg sig



                                    var lst = new List<eUserOptionsModules.USROPT_MODULE>()
                                    {
                                        eUserOptionsModules.USROPT_MODULE.ADMIN_GENERAL,
                                        eUserOptionsModules.USROPT_MODULE.ADMIN_ACCESS,
                                        eUserOptionsModules.USROPT_MODULE.ADMIN_TABS,
                                        eUserOptionsModules.USROPT_MODULE.ADMIN_HOME,
                                        eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS,
                                        eUserOptionsModules.USROPT_MODULE.ADMIN_DASHBOARD,


                                     };


                                    if (_pref.User.UserLevel >= (int)UserLevel.LEV_USR_SUPERADMIN)
                                        lst.Add(eUserOptionsModules.USROPT_MODULE.ADMIN_ORM);

                                    AddModules(lst, rootParentModule, targetModule);
                                }
                                /*******************************************************************/


                                #endregion
                            }


                            #region SPECIF
                            /*************  LIENS DES SPECIFS ****************************************/

                            XmlNode _blocSpecif = null;

                            #region Organigramme

                            eOrganigramme org = new eOrganigramme(_pref, nDescid, nFileId);
                            if (org.GetUserValue() > 0)
                            {
                                if (_blocSpecif == null)
                                    _blocSpecif = _xmlUserMenu.CreateElement("blocMenu");
                                _blocSpecif.AppendChild(
                                    AddAction("Organigramme", "icon-edn-next icnMnuChip", string.Concat("ShowOrga(", nDescid, ", ", nFileId, ")")));    //TODORES - prendre le label du uservalue
                            }
                            #endregion

                            // les liens spécifs génère en fait un post vers la spécif avec des paramètres additionnels
                            RqParam rSpecif = new RqParam();

                            rSpecif.SetQuery("SELECT PREFID, URL,URLNAME,URLPARAM FROM [PREF] WHERE USERID=0 AND TAB=@TAB AND URL<>''");
                            rSpecif.AddInputParameter("@TAB", SqlDbType.Int, nDescid);
                            DataTableReaderTuned dtrURL = _dal.Execute(rSpecif, out sError);
                            try
                            {
                                if (string.IsNullOrEmpty(sError) && dtrURL.HasRows)
                                {
                                    if (_blocSpecif == null)
                                        _blocSpecif = _xmlUserMenu.CreateElement("blocMenu");

                                    while (dtrURL.Read())
                                    {
                                        string label = dtrURL.GetString("urlname");
                                        int nURLID = dtrURL.GetEudoNumeric("PREFID");
                                        _blocSpecif.AppendChild(AddAction(label, "icon-edn-next icnMnuChip", string.Concat("exportToLinkToV7(", nURLID.ToString(), ",", nFileId, ",2)")));

                                    }
                                }
                            }
                            finally
                            {
                                if (dtrURL != null)
                                    dtrURL.Dispose();
                            }

                            #region Specifs V2 XRM
                            List<eSpecif> liSpecV2 = eSpecif.GetSpecifList(_pref, eLibConst.SPECIF_TYPE.TYP_FILE, nDescid);


                            if (liSpecV2.Count > 0 && _blocSpecif == null)
                                _blocSpecif = _xmlUserMenu.CreateElement("blocMenu");

                            foreach (eSpecif spec in liSpecV2)
                            {
                                if (!spec.IsViewable)
                                    continue;

                                string sAction = string.Empty;
                                if (spec.Source == eLibConst.SPECIF_SOURCE.SRC_V7)
                                {
                                    //les spécifs V7 sont toujours gérées depuis les pref (voir ci-dessus)
                                    continue;
                                }

                                #region Appel d'un rapport depuis un lien favori de la page d'accueil
                                if (spec.Url.ToLower().Contains("reportid="))
                                {

                                    string[] sParams = spec.Url.ToLower().Split('&');
                                    string[] sParam;
                                    int reportid = 0, reportType = 0, nTab = 0, fid = 0, nTabBkm = 0, bFile = 0;
                                    foreach (string s in sParams)
                                    {
                                        sParam = s.Split('=');
                                        if (sParam.Length == 2)
                                        {
                                            switch (sParam[0])
                                            {
                                                case "reportid":
                                                    int.TryParse(sParam[1], out reportid);
                                                    break;
                                                case "reporttype":
                                                    int.TryParse(sParam[1], out reportType);
                                                    break;
                                                //case "tab":
                                                //    int.TryParse(sParam[1], out nTab);
                                                //    break;
                                                //case "fid":
                                                //    int.TryParse(sParam[1], out fid);
                                                //    break;
                                                case "tabbkm":
                                                    int.TryParse(sParam[1], out nTabBkm);
                                                    break;
                                                case "bfile":
                                                    int.TryParse(sParam[1], out bFile);
                                                    break;
                                                default:
                                                    break;
                                            }
                                        }
                                    }
                                    sAction = string.Concat("runReportFromGlobal(", reportid, ", ", reportType, ", ", nDescid, ", ", nFileId, ", ", nTabBkm, ", ", 1, ")");
                                }
                                #endregion
                                else if (spec.Source == eLibConst.SPECIF_SOURCE.SRC_XRM
                                    || spec.Source == eLibConst.SPECIF_SOURCE.SRC_EXT
                                    )
                                {
                                    var sUrl = spec.GetRelativeUrlFromRoot(_pref);
                                    switch (spec.OpenMode)
                                    {
                                        case eLibConst.SPECIF_OPENMODE.NEW_WINDOW:

                                            string sEncode = ExternalUrlTools.GetCryptEncode(string.Concat("sid=", spec.SpecifId, "&tab=", nDescid, "&fid=", nFileId));
                                            sAction = string.Concat("window.open(\"eSubmitTokenXRM.aspx?t=", sEncode, "\");");

                                            break;
                                        case eLibConst.SPECIF_OPENMODE.MODAL:
                                        case eLibConst.SPECIF_OPENMODE.HIDDEN:
                                            sAction = string.Concat("runSpec(\"", spec.SpecifId, "\");");
                                            break;
                                        default:
                                            break;
                                    }
                                }

                                _blocSpecif.AppendChild(AddAction(spec.Label, "icon-edn-next icnMnuChip", sAction));
                            }
                            #endregion

                            /*************************************************************************/
                            if (_blocSpecif != null)
                            {
                                //Attribut type de bloc menu
                                _menuType = _xmlUserMenu.CreateAttribute("endmenutype");
                                _menuType.Value = "rightMenuSubMenuItem lnkspecif";
                                _blocSpecif.Attributes.Append(_menuType);

                                _title = _xmlUserMenu.CreateAttribute("title");
                                _title.Value = eResApp.GetRes(_pref, 1500);
                                _blocSpecif.Attributes.Append(_title);

                                _iImg = _xmlUserMenu.CreateAttribute("className");
                                _iImg.Value = "icon-hyperlink";
                                _blocSpecif.Attributes.Append(_iImg);

                                _baseUserMenu.AppendChild(_blocSpecif);
                            }
                            #endregion

                        }
                    }
                    finally
                    {
                        if (dtrTabTreatmentRight != null)
                            dtrTabTreatmentRight.Dispose();
                        // Fermeture de la connexion
                        _dal.CloseDatabase();
                    }

                    break;
                    #endregion

            }

            //Lien Aide
            XmlNode xnodeHelp = _xmlUserMenu.CreateElement("HelpExtranetLink");
            _baseUserMenu.AppendChild(xnodeHelp);
            xnodeHelp.InnerText = helpExtranetEnabled ? "1" : "0";


            //Menu Utilisateur
            XmlNode xnodeUser = _xmlUserMenu.CreateElement("userBlock");
            _baseUserMenu.AppendChild(xnodeUser);

            #region User infos
            //UserId
            XmlNode xUserid = _xmlUserMenu.CreateElement("userid");
            xnodeUser.AppendChild(xUserid);
            xUserid.InnerText = userId.ToString();

            //Username
            XmlNode xUserName = _xmlUserMenu.CreateElement("username");
            xnodeUser.AppendChild(xUserName);
            xUserName.InnerText = _pref.User.UserDisplayName;

            // Userlogin
            XmlNode xUserLogin = _xmlUserMenu.CreateElement("userlogin");
            xnodeUser.AppendChild(xUserLogin);
            xUserLogin.InnerText = _pref.User.UserLogin;

            // Usergroupname
            XmlNode xUserGroupName = _xmlUserMenu.CreateElement("usergroupname");
            xnodeUser.AppendChild(xUserGroupName);
            xUserGroupName.InnerText = _pref.User.UserGroupName;

            // Usermail
            XmlNode xUserMail = _xmlUserMenu.CreateElement("usermail");
            xnodeUser.AppendChild(xUserMail);
            xUserMail.InnerText = _pref.User.UserMail;

            // UserLevel
            XmlNode xUserLevel = _xmlUserMenu.CreateElement("userlevel");
            xnodeUser.AppendChild(xUserLevel);
            xUserLevel.InnerText = eLibTools.GetUserLevelLabel(_pref, (UserLevel)_pref.User.UserLevel);


            #endregion


            //Avatar URL
            XmlNode xImgUrl = _xmlUserMenu.CreateElement("avatarurl"); ;
            xnodeUser.AppendChild(xImgUrl);
            xImgUrl.InnerText = eImageTools.GetAvatar(_pref);

            //Dimension            
            XmlAttribute xHeight = _xmlUserMenu.CreateAttribute("height");
            xHeight.Value = eConst.AVATAR_IMG_HEIGHT.ToString();
            xImgUrl.Attributes.Append(xHeight);

            XmlAttribute xWidth = _xmlUserMenu.CreateAttribute("width");
            xWidth.Value = eConst.AVATAR_IMG_WIDTH.ToString();
            xImgUrl.Attributes.Append(xWidth);

            //Node ressources
            XmlNode xnodeRes = _xmlUserMenu.CreateElement("res"); ;
            _baseUserMenu.AppendChild(xnodeRes);

            //444 Options utilisateur / "Mon Eudonet" - on affiche Accueil à cet endroit si on se trouve déjà sur la zone Options utilisateur / "Mon Eudonet"
            if (mgrType != eConst.eFileType.ADMIN || rootParentModule == eUserOptionsModules.USROPT_MODULE.ADMIN)
            {
                XmlNode x7174 = _xmlUserMenu.CreateElement("n7174");
                xnodeRes.AppendChild(x7174);
                x7174.InnerText = eResApp.GetRes(_pref, 7174);
                XmlAttribute xHideUserOptions = _xmlUserMenu.CreateAttribute("hideuseroptions");
                xHideUserOptions.Value = "0";
                _baseUserMenu.Attributes.Append(xHideUserOptions);
            }
            else
            {
                XmlAttribute xHideUserOptions = _xmlUserMenu.CreateAttribute("hideuseroptions");
                xHideUserOptions.Value = "1";
                _baseUserMenu.Attributes.Append(xHideUserOptions);
            }

            // 21 Administration - uniquement pour les utilisateurs Administrateurs
            if (_pref.User.UserLevel >= UserLevel.LEV_USR_ADMIN.GetHashCode() &&
                (mgrType != eConst.eFileType.ADMIN || rootParentModule == eUserOptionsModules.USROPT_MODULE.MAIN)
            )
            {


                XmlNode x21 = _xmlUserMenu.CreateElement("n21");
                xnodeRes.AppendChild(x21);
                x21.InnerText = eResApp.GetRes(_pref, 21);
                XmlAttribute xHideAdmin = _xmlUserMenu.CreateAttribute("hideadmin");

                /*KHA CLE config*/

                if (_pref.IsAdminEnabled && eFeaturesManager.IsFeatureAvailable(_pref, eConst.XrmFeature.Admin) && !bNoAdmin)
                    xHideAdmin.Value = "0";
                else
                    xHideAdmin.Value = "1";

                _baseUserMenu.Attributes.Append(xHideAdmin);
            }
            else
            {
                XmlAttribute xHideAdmin = _xmlUserMenu.CreateAttribute("hideadmin");
                xHideAdmin.Value = "1";
                _baseUserMenu.Attributes.Append(xHideAdmin);
            }

            XmlNode xmlNewThmVal = _xmlUserMenu.CreateElement("NwThmVal");
            _baseUserMenu.AppendChild(xmlNewThmVal);
            xmlNewThmVal.InnerText = (idThemeIris.Contains(_pref.ThemeXRM.Id)) ? eResApp.GetRes(_pref, 2378) : eResApp.GetRes(_pref, 2376);

            XmlNode xttNewThmTtl = _xmlUserMenu.CreateElement("NwThmTtl");
            _baseUserMenu.AppendChild(xttNewThmTtl);
            xttNewThmTtl.InnerText = (idThemeIris.Contains(_pref.ThemeXRM.Id)) ? eResApp.GetRes(_pref, 2378) : eResApp.GetRes(_pref, 2376);

            XmlNode xmlchckNwThm = _xmlUserMenu.CreateElement("chckNwThm");
            _baseUserMenu.AppendChild(xmlchckNwThm);

            xmlchckNwThm.InnerText = (idThemeIris.Contains(iThemeId)) ? "1" : "0";

            //5008 Déconnexion
            XmlNode x5008 = _xmlUserMenu.CreateElement("n5008");
            xnodeRes.AppendChild(x5008);
            x5008.InnerText = eResApp.GetRes(_pref, 5008);

            //6179 Se déconnecter d'Eudonet
            XmlNode x6179 = _xmlUserMenu.CreateElement("n6179");
            xnodeRes.AppendChild(x6179);
            x6179.InnerText = eResApp.GetRes(_pref, 6179);

            //6180 Changer votre avatar
            XmlNode x6180 = _xmlUserMenu.CreateElement("n6180");
            xnodeRes.AppendChild(x6180);
            x6180.InnerText = eResApp.GetRes(_pref, 6180);

            //6181 Bonjour
            XmlNode x6181 = _xmlUserMenu.CreateElement("n6181");
            xnodeRes.AppendChild(x6181);
            x6181.InnerText = eResApp.GetRes(_pref, 6181);

            //6187 Aide
            XmlNode x6187 = _xmlUserMenu.CreateElement("n6187");
            x6187.InnerText = eResApp.GetRes(_pref, 6187);
            xnodeRes.AppendChild(x6187);

            //6853 Thème
            XmlNode x6853 = _xmlUserMenu.CreateElement("n6853");
            x6853.InnerText = eResApp.GetRes(_pref, 6853);
            xnodeRes.AppendChild(x6853);

            //7984 Extranet
            XmlNode x7984 = _xmlUserMenu.CreateElement("n7984");
            x7984.InnerText = eResApp.GetRes(_pref, 7984);
            xnodeRes.AppendChild(x7984);

            //2378 Le nouvel Eudonet
            XmlNode x2378 = _xmlUserMenu.CreateElement("n2378");
            x2378.InnerText = eResApp.GetRes(_pref, 2378);
            xnodeRes.AppendChild(x2378);


            if (_bReturnInXML)
            {
                #region afficher rendu encapsulé dans un flux XML
                // Num erreur
                XmlNode _errorNode = xmlResult.CreateElement("error");
                _errorNode.AppendChild(xmlResult.CreateTextNode("0"));
                _baseResultNode.AppendChild(_errorNode);

                // Msg Erreur
                XmlNode _errorMsgNode = xmlResult.CreateElement("errorMSG");
                _errorMsgNode.AppendChild(xmlResult.CreateTextNode(""));
                _baseResultNode.AppendChild(_errorMsgNode);


                XmlNode _ContentNode = xmlResult.CreateElement("Content");

                // Si la XSLT est 
                if (_bServerSideXSLT)
                {
                    //Envoi HTML
                    _ContentNode.AppendChild(xmlResult.CreateTextNode(eXSLT.UserMenuHTML(_xmlUserMenu)));
                }
                else
                {
                    //envoi XML
                    XmlNode copiedNode = xmlResult.ImportNode(_baseUserMenu, true);
                    _ContentNode.AppendChild(copiedNode);
                }


                // Content            
                _baseResultNode.AppendChild(_ContentNode);

                // Affiche le flux XML
                RenderResult(RequestContentType.XML, delegate () { return xmlResult.OuterXml; });
                #endregion
            }
            else
            {
                #region affiche rendu HTML Seul
                RenderResult(RequestContentType.TEXT, delegate () { return eXSLT.UserMenuHTML(_xmlUserMenu); });
                #endregion
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nDescid"></param>
        /// <returns></returns>
        private string shFileInPopup(int nDescid)
        {
            string sJsAction;
            string strPopupWidth = "null";
            string strPopupHeight = "null";
            // TOCHECK SMS
            if (_tab.EdnType == EdnType.FILE_MAIL || _tab.EdnType == EdnType.FILE_SMS)
            {
                strPopupWidth = "(document.body.scrollWidth - 25)";
                strPopupHeight = "(document.body.scrollHeight - 25)";
            }

            string strApplyCloseOnly = "false";
            if (_tab.EdnType == EdnType.FILE_MAIN)
                strApplyCloseOnly = "true";

            string sCallBack = "";
            if (_tab.EdnType == EdnType.FILE_USER)
                sCallBack = " , nsAdminUsers.InitDefault ";

            sJsAction = string.Concat("shFileInPopup(", nDescid, ", 0, '", eResApp.GetRes(_pref, 31).Replace("'", @"\'"), "', ", strPopupWidth, ", ", strPopupHeight, ", ", _tab.EdnType == EudoQuery.EdnType.FILE_MAIL ? "1" : "0", ", null, ", strApplyCloseOnly, ", null, 2,null,null", sCallBack, ") ");
            return sJsAction;
        }



        /// <summary>
        /// Ajoute une entrée "action" dans le menu
        /// </summary>
        /// <param name="sLibelle">Nom de l'action</param>
        /// <param name="sCssName">CSS</param>
        /// <param name="sTooltip">Tooltype</param>
        /// <param name="sAction">Javascript de l'action</param>        
        private void AddActionEntry(string sLibelle, string sCssName, string sTooltip, string sAction)
        {

            _baseUserMenu.AppendChild(AddSection(sLibelle, sCssName, sTooltip, sAction, false, ""));

        }

        /// <summary>
        /// Ajouter les modules d'administration au menu droit
        /// </summary>
        /// <param name="modules">Liste des modules</param>
        /// <param name="rootParentModule"></param>
        /// <param name="targetModule"></param>
        public void AddModules(List<eUserOptionsModules.USROPT_MODULE> modules, eUserOptionsModules.USROPT_MODULE rootParentModule, eUserOptionsModules.USROPT_MODULE targetModule)
        {
            // Pour chaque section, ajout de ses liens
            foreach (eUserOptionsModules.USROPT_MODULE module in modules)
            {
                // Récupération des modules admin enfants disponibles pour cette section
                // On récupère les enfants que si le module a le droit d'afficher ses enfants
                List<eUserOptionsModules.USROPT_MODULE> childModules = new List<eUserOptionsModules.USROPT_MODULE>();
                if (eUserOptionsModules.ModuleChildrenCanAppearInMenu(module))
                    childModules = eUserOptionsModules.GetModuleChildren(module);

                // Génération du lien
                string moduleLink = string.Concat("loadUserOption('", module, "')");
                if (rootParentModule == eUserOptionsModules.USROPT_MODULE.ADMIN)
                {
                    if (childModules.Count == 0)
                        moduleLink = string.Concat("nsAdmin.loadAdminModule('", module, "')");
                    else
                        moduleLink = "nsAdmin.toggleSubMenuTitle(this)";
                }
                else if (module == eUserOptionsModules.USROPT_MODULE.HOME)
                {
                    moduleLink = "javascript:onTabSelect( 0);";
                }

                // Création de la section avec lien
                if (eFeaturesManager.IsFeatureAvailable(_pref, eUserOptionsModules.GetModuleFeature(module)))
                {

                    XmlNode _blocAdmin = AddSection(
                        eUserOptionsModules.GetModuleLabel(module, _pref),
                        string.Concat("icon-", eUserOptionsModules.GetModuleIcon(module)),
                        eUserOptionsModules.GetModuleTooltip(module, _pref),
                        moduleLink,
                        module == targetModule,
                        childModules.Count > 0 ? "icon-caret-right" : string.Empty
                    );

                    // Puis on parcourt chaque module enfant implémenté pour afficher son lien
                    bool bContainsSelectedChild = false;
                    foreach (eUserOptionsModules.USROPT_MODULE childModule in childModules)
                    {

                        if (eFeaturesManager.IsFeatureAvailable(_pref, eUserOptionsModules.GetModuleFeature(childModule)))
                        {
                            bool bSelected = targetModule == childModule;
                            // On mémorise si au moins un enfant est le module actuellement affiché, afin d'ouvrir la section qui le contient et rendre la sélection visible
                            if (bSelected)
                                bContainsSelectedChild = true;

                            string childLink = string.Concat("loadUserOption('", childModule, "')");

                            // Pour la partie "Paramètres", 
                            if (module == eUserOptionsModules.USROPT_MODULE.ADMIN_GENERAL)
                            {
                                // si on n'est pas dans le module "Paramètres généraux", lien vers le module et l'ancre
                                if (targetModule != eUserOptionsModules.USROPT_MODULE.ADMIN_GENERAL)
                                {
                                    childLink = string.Concat("nsAdmin.loadAdminModule('", module, "', '", childModule, "')");

                                    _blocAdmin.AppendChild(
                                       AddAction(
                                           eUserOptionsModules.GetModuleRightMenuLabel(childModule, _pref),
                                           "icon-edn-next icnMnuChip",
                                           childLink,
                                           string.Empty,
                                           string.Empty,
                                           eUserOptionsModules.GetModuleTooltip(childModule, _pref),
                                           bSelected: bSelected
                                       )
                                    );
                                }
                                else
                                {
                                    // Sinon ancre vers la sous-partie correspondante
                                    _blocAdmin.AppendChild(
                                         AddLink(
                                             eUserOptionsModules.GetModuleRightMenuLabel(childModule, _pref),
                                             "icon-edn-next icnMnuChip",
                                             "#" + childModule,
                                             string.Empty,
                                             string.Empty
                                         )
                                     );
                                }
                            }
                            else
                            {
                                // Pour les autres parties, c'est toujours un lien vers le module
                                childLink = string.Concat("nsAdmin.loadAdminModule('", childModule, "')");

                                _blocAdmin.AppendChild(
                                    AddAction(
                                   eUserOptionsModules.GetModuleRightMenuLabel(childModule, _pref),
                                   "icon-edn-next icnMnuChip",
                                   childLink,
                                   string.Empty,
                                   string.Empty,
                                   eUserOptionsModules.GetModuleTooltip(childModule, _pref),
                                   bSelected: bSelected
                                    )
                                );
                            }

                        }


                    }

                    // Affichage de la section ouverte (par défaut) ou fermée (si ni elle, ni l'un de ses enfants n'est le module actuellement affiché)
                    if (!(module == targetModule || bContainsSelectedChild))
                        _blocAdmin.Attributes["endmenutype"].Value = string.Concat(_blocAdmin.Attributes["endmenutype"].Value, " closed");

                    // Pas d'affichage de la section si elle ne contient aucun module
                    // TODO/TOCHECK : désactivé pour l'admin pour le moment
                    //if (_blocUserOptionsNodes.ChildNodes.Count > 0)
                    _baseUserMenu.AppendChild(_blocAdmin);
                }



            }
        }


        /// <summary>
        /// Ajoute une entrée "lien"
        /// </summary>
        /// <param name="sLibelle">Libellé du lien</param>
        /// <param name="sClass">Classe CSS</param>
        /// <param name="sHref">url</param>
        /// <param name="sTarget">target du lien</param>
        /// <param name="sPrefix">prefix de l'url</param>
        /// <returns></returns>
        private XmlNode AddLink(string sLibelle, string sClass, string sHref, string sTarget, string sPrefix)
        {
            XmlNode _entry = _xmlUserMenu.CreateElement("entry");

            //Ajout du node Label
            if (!string.IsNullOrEmpty(sLibelle))
            {
                XmlNode _label = _xmlUserMenu.CreateElement("label");
                _label.AppendChild(_xmlUserMenu.CreateTextNode(sLibelle));
                _entry.AppendChild(_label);
            }

            // Ajout du node Class
            if (!string.IsNullOrEmpty(sClass))
            {
                XmlNode _class = _xmlUserMenu.CreateElement("className");
                _class.AppendChild(_xmlUserMenu.CreateTextNode(sClass));
                _entry.AppendChild(_class);
            }

            // Ajout du node action
            if (!string.IsNullOrEmpty(sHref))
            {
                XmlNode _action = _xmlUserMenu.CreateElement("link");
                _action.AppendChild(_xmlUserMenu.CreateTextNode(sHref));

                XmlAttribute _target = _xmlUserMenu.CreateAttribute("target");
                _target.Value = sTarget;
                _action.Attributes.Append(_target);

                XmlAttribute _prefix = _xmlUserMenu.CreateAttribute("prefix");
                _prefix.Value = sPrefix;
                _action.Attributes.Append(_prefix);

                _entry.AppendChild(_action);
            }

            return _entry;
        }

        /// <summary>
        /// Génère un noeud "section" de type "bloc menu", pouvant contenir d'autres liens de type "Action"
        /// </summary>
        /// <param name="sLibelle">Libellé de la section</param>
        /// <param name="className">Image associée</param>
        /// <param name="onclick">Eventuel lien/code JavaScript à ajouter au clic sur la section</param>
        /// <param name="selected">Indique si l'attribut "selected" doit être ajouté, pour inclure une classe CSS spécifique sur le rendu de la balise</param>
        /// <param name="sTooltip">Info-bulle éventuelle</param>
        /// <returns></returns>
        private XmlNode AddSection(string sLibelle, string className, string sTooltip = "", string sOnClick = "", bool selected = false, string sCaretClassName = "icon-caret-down")
        {
            XmlNode _blocMenu = _xmlUserMenu.CreateElement("blocMenu");

            XmlAttribute _menuType = _xmlUserMenu.CreateAttribute("endmenutype");
            _menuType.Value = "rightMenuSubMenuItem";
            _blocMenu.Attributes.Append(_menuType);

            // Ajout de l'attribut title (libellé et non info-bulle)
            if (!string.IsNullOrEmpty(sLibelle))
            {
                XmlAttribute _title = _xmlUserMenu.CreateAttribute("title");
                _title.Value = sLibelle;
                _blocMenu.Attributes.Append(_title);
            }

            //Ajout de l'attribut tooltip
            if (!string.IsNullOrEmpty(sTooltip))
            {
                XmlAttribute _tooltip = _xmlUserMenu.CreateAttribute("tooltip");
                _tooltip.Value = sTooltip;
                _blocMenu.Attributes.Append(_tooltip);
            }
            else if (!string.IsNullOrEmpty(sLibelle))
            {
                XmlAttribute _tooltip = _xmlUserMenu.CreateAttribute("tooltip");
                _tooltip.Value = sLibelle;
                _blocMenu.Attributes.Append(_tooltip);
            }

            //Ajout de l'attribut class
            if (!string.IsNullOrEmpty(className))
            {
                XmlAttribute _iImg = _xmlUserMenu.CreateAttribute("className");
                _iImg.Value = className;
                _blocMenu.Attributes.Append(_iImg);
            }

            //Ajout de l'attribut onclick (le préfixe javascript: est ajouté par xslmenu.xsl)
            if (!string.IsNullOrEmpty(sOnClick))
            {
                XmlAttribute _onclick = _xmlUserMenu.CreateAttribute("onclick");
                _onclick.Value = sOnClick;
                _blocMenu.Attributes.Append(_onclick);
            }

            //Ajout de l'attribut selected
            if (selected)
            {
                XmlAttribute _selected = _xmlUserMenu.CreateAttribute("selected");
                _selected.Value = "1";
                _blocMenu.Attributes.Append(_selected);
            }

            //Ajout de l'attribut caretClassName, vide ou non (icône optionnelle)
            XmlAttribute _caretClassName = _xmlUserMenu.CreateAttribute("caretClassName");
            _caretClassName.Value = sCaretClassName;
            _blocMenu.Attributes.Append(_caretClassName);

            return _blocMenu;
        }

        /// <summary>
        /// Génère un noeud d'entrée "action"
        /// </summary>
        /// <param name="sLibelle">Libellé de l'entrée</param>
        /// <param name="className">Image associée</param>
        /// <param name="sAction">Action au click</param>
        /// <param name="idAction">identifiant de l'action</param>
        /// <param name="strAction">nom system de l'action</param>
        /// <returns></returns>
        private XmlNode AddAction(string sLibelle, string className, string sAction, string idAction = "", string strAction = "", string sTooltip = "", string strHref = "", Boolean bSelected = false)
        {
            XmlNode _entry = _xmlUserMenu.CreateElement("entry");

            //Ajout du node Label
            if (!string.IsNullOrEmpty(sLibelle))
            {
                XmlNode _label = _xmlUserMenu.CreateElement("label");
                _label.AppendChild(_xmlUserMenu.CreateTextNode(sLibelle));
                _entry.AppendChild(_label);
            }

            // Ajout du node image
            if (!string.IsNullOrEmpty(className))
            {
                XmlNode _iImg = _xmlUserMenu.CreateElement("className");
                _iImg.AppendChild(_xmlUserMenu.CreateTextNode(className));
                _entry.AppendChild(_iImg);
            }

            // Ajout du node action
            if (!string.IsNullOrEmpty(sAction))
            {
                XmlNode _action = _xmlUserMenu.CreateElement("action");
                _action.AppendChild(_xmlUserMenu.CreateTextNode(sAction));
                _entry.AppendChild(_action);
            }

            // Ajout du node id action
            if (!string.IsNullOrEmpty(idAction))
            {
                XmlNode _action = _xmlUserMenu.CreateElement("idaction");
                _action.AppendChild(_xmlUserMenu.CreateTextNode(idAction));
                _entry.AppendChild(_action);
            }

            // Ajout du node nom system action
            if (!string.IsNullOrEmpty(strAction))
            {
                XmlNode _action = _xmlUserMenu.CreateElement("straction");
                _action.AppendChild(_xmlUserMenu.CreateTextNode(strAction));
                _entry.AppendChild(_action);
            }

            if (!string.IsNullOrEmpty(strHref))
            {
                XmlNode _link = _xmlUserMenu.CreateElement("link");
                _link.AppendChild(_xmlUserMenu.CreateTextNode(strAction));
                _entry.AppendChild(_link);
            }

            //Ajout du node tooltip
            if (!string.IsNullOrEmpty(sTooltip))
            {
                XmlNode _tooltip = _xmlUserMenu.CreateElement("tooltip");
                _tooltip.AppendChild(_xmlUserMenu.CreateTextNode(sTooltip));
                _entry.AppendChild(_tooltip);
            }

            // Entrée sélectionnée
            if (bSelected)
            {
                XmlAttribute _selected = _xmlUserMenu.CreateAttribute("selected");
                _selected.Value = "1";
                _entry.Attributes.Append(_selected);
            }

            return _entry;
        }

        /// <summary>
        /// Ajout le bloc de boutons de navigation en mode fiche
        /// </summary>
        /// <returns>Node XML pour mapping avec XSLT</returns>
        private XmlNode addBrowsingMenuElement()
        {
            XmlNode browsingEntry = _xmlUserMenu.CreateElement("browsingEntry");
            XmlNode iImg = null;
            XmlNode iId = null;
            XmlNode action = null;
            XmlNode entry = null;
            XmlNode enabled = null;

            /*bouton vers première fiche**********************************/
            entry = _xmlUserMenu.CreateElement("entry");
            browsingEntry.AppendChild(entry);

            iId = _xmlUserMenu.CreateElement("id");
            iId.AppendChild(_xmlUserMenu.CreateTextNode("BrowsingFirst"));
            entry.AppendChild(iId);

            // Par défaut disabled, l'activation se fait au chargement
            enabled = _xmlUserMenu.CreateElement("eenabled");
            enabled.AppendChild(_xmlUserMenu.CreateTextNode("0"));
            entry.AppendChild(enabled);

            iImg = _xmlUserMenu.CreateElement("className");
            iImg.AppendChild(_xmlUserMenu.CreateTextNode("icon-edn-first icnMnuVerBtn"));
            entry.AppendChild(iImg);

            // Ajout du node action
            action = _xmlUserMenu.CreateElement("eAction");
            action.AppendChild(_xmlUserMenu.CreateTextNode("2"));
            entry.AppendChild(action);

            string sActionBrowse = "BrowseFile(this);return false;";
            if (_pref.AdminMode && _tab.TabType == TableType.USER)
                sActionBrowse = "nsAdminUsers.browseFile(this);return false;";



            action = _xmlUserMenu.CreateElement("action");
            action.AppendChild(_xmlUserMenu.CreateTextNode(sActionBrowse));
            entry.AppendChild(action);
            ///////////////////////

            /*bouton vers fiche précédente********************************/
            entry = _xmlUserMenu.CreateElement("entry");
            browsingEntry.AppendChild(entry);

            iId = _xmlUserMenu.CreateElement("id");
            iId.AppendChild(_xmlUserMenu.CreateTextNode("BrowsingPrevious"));
            entry.AppendChild(iId);

            // Par défaut disabled, l'activation se fait au chargement
            enabled = _xmlUserMenu.CreateElement("eenabled");
            enabled.AppendChild(_xmlUserMenu.CreateTextNode("0"));
            entry.AppendChild(enabled);

            iImg = _xmlUserMenu.CreateElement("className");
            iImg.AppendChild(_xmlUserMenu.CreateTextNode("icon-edn-prev icnMnuVerBtn"));
            entry.AppendChild(iImg);

            // Ajout du node action
            action = _xmlUserMenu.CreateElement("eAction");
            action.AppendChild(_xmlUserMenu.CreateTextNode("1"));
            entry.AppendChild(action);

            action = _xmlUserMenu.CreateElement("action");
            action.AppendChild(_xmlUserMenu.CreateTextNode(sActionBrowse));
            entry.AppendChild(action);
            ///////////////////////

            /*bouton vers fiche suivante**********************************/
            entry = _xmlUserMenu.CreateElement("entry");
            browsingEntry.AppendChild(entry);

            iId = _xmlUserMenu.CreateElement("id");
            iId.AppendChild(_xmlUserMenu.CreateTextNode("BrowsingNext"));
            entry.AppendChild(iId);

            // Par défaut disabled, l'activation se fait au chargement
            enabled = _xmlUserMenu.CreateElement("eenabled");
            enabled.AppendChild(_xmlUserMenu.CreateTextNode("0"));
            entry.AppendChild(enabled);

            iImg = _xmlUserMenu.CreateElement("className");
            iImg.AppendChild(_xmlUserMenu.CreateTextNode("icon-edn-next icnMnuVerBtn"));
            entry.AppendChild(iImg);

            // Ajout du node action
            action = _xmlUserMenu.CreateElement("eAction");
            action.AppendChild(_xmlUserMenu.CreateTextNode("0"));
            entry.AppendChild(action);

            action = _xmlUserMenu.CreateElement("action");
            action.AppendChild(_xmlUserMenu.CreateTextNode(sActionBrowse));
            entry.AppendChild(action);
            ///////////////////////

            /*bouton vers dernière fiche*********************************/
            entry = _xmlUserMenu.CreateElement("entry");
            browsingEntry.AppendChild(entry);

            iId = _xmlUserMenu.CreateElement("id");
            iId.AppendChild(_xmlUserMenu.CreateTextNode("BrowsingLast"));
            entry.AppendChild(iId);

            // Par défaut disabled, l'activation se fait au chargement
            enabled = _xmlUserMenu.CreateElement("eenabled");
            enabled.AppendChild(_xmlUserMenu.CreateTextNode("0"));
            entry.AppendChild(enabled);

            iImg = _xmlUserMenu.CreateElement("className");
            iImg.AppendChild(_xmlUserMenu.CreateTextNode("icon-edn-last icnMnuVerBtn"));
            entry.AppendChild(iImg);

            // Ajout du node action
            action = _xmlUserMenu.CreateElement("eAction");
            action.AppendChild(_xmlUserMenu.CreateTextNode("3"));
            entry.AppendChild(action);

            action = _xmlUserMenu.CreateElement("action");
            action.AppendChild(_xmlUserMenu.CreateTextNode(sActionBrowse));
            entry.AppendChild(action);
            ///////////////////////

            return browsingEntry;
        }

        /// <summary>
        /// Ajout de l'item du menu permettant d'imprimer la liste ou la fiche en cours
        /// </summary>
        /// <returns></returns>
        private void AddPrintEntry(bool isFile = false)
        {
            string labelPrint;
            string jsFunction;
            if (isFile)
            {
                labelPrint = eResApp.GetRes(_pref, 7978);
                jsFunction = "printFile()";
            }
            else
            {
                labelPrint = eResApp.GetRes(_pref, 6190);
                jsFunction = "printCurrentList()";
            }

            XmlNode blockPrint = _xmlUserMenu.CreateElement("blocMenu");

            blockPrint.AppendChild(AddAction(labelPrint, "icon-print2", jsFunction));

            //Attribut type de bloc menu
            XmlAttribute menuAttr = _xmlUserMenu.CreateAttribute("endmenutype");
            menuAttr.Value = "rightMenuItem";
            blockPrint.Attributes.Append(menuAttr);

            _baseUserMenu.AppendChild(blockPrint);
        }


        /// <summary>
        /// Charge les catégories du store
        /// </summary>
        private IDictionary<string, string> GetStoreCategories()
        {
            bool bStoreAccessOk = false;
            bool bNoInternet = eLibTools.GetServerConfig("ServerWithoutInternet", "0") == "1";
            IDictionary<string, string> categories = new Dictionary<string, string>();

            if (!bNoInternet)
            {
                eAPIExtensionStoreAccess storeAccess = new eAPIExtensionStoreAccess(_pref);
                categories = storeAccess.GetExtensionCategories().ToDictionary(k => k.Id.ToString(), k => k.Label);
                if (storeAccess.ApiErrors.Trim().Length == 0)
                {
                    bStoreAccessOk = true;
                }
            }

            if (!bStoreAccessOk)
            {
                //Chargement de la liste extension depuis le json
                List<eAdminExtension> extensionFromJson = eAdminExtension.initListExtensionFromJson(_pref);

                foreach (eAdminExtension extension in extensionFromJson)
                {
                    foreach (KeyValuePair<string, string> kvp in extension.Infos.Categories)
                    {
                        if (!categories.ContainsKey(kvp.Key))
                            categories.Add(kvp.Key, kvp.Value);
                    }
                }
            }

            return categories;
        }

    }

}