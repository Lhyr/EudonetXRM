using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using EudoExtendedClasses;
using EudoQuery;

namespace Com.Eudonet.Xrm.mgr
{
    /// <className>eNavBarManager</className>
    /// <summary>Génération du XML de la navbar</summary>
    /// <purpose>Génère un xml pour la création de la barre de navigation
    /// Ne s'appelle pas indépendamment : Cette page ne rend que du XML. 
    /// format :
    /// <result>
    ///     <error></error>
    ///     <errorMsg></errorMsg>
    ///     <content></content>
    ///  </result>
    /// </purpose>
    /// <authors>SPH</authors>
    /// <date>2011-09-14</date>
    public class eNavBarManager : eEudoManager
    {

        const int TAB_LEFT_MARG = 15;
        const int TAB_RIGHT_MARG = 15;
        const int TAB_FONT_SIZE = 11;
        const string TAB_FONT_FAMILLY = "Verdana";


        bool _bServerSideXSLT = true;

        /// <summary>Document XML de retour</summary>
        private XmlNode _baseResultNode;

        /// <summary>Document NAVBAR</summary>
        private XmlDocument _xmlNavBar = new XmlDocument();
        private XmlNode _baseNavBarNode;



        /// <summary>
        /// Action au lancement de la page
        /// </summary>
        protected override void ProcessManager()
        {
            string error = string.Empty;

            #region initialisation

            // BASE DU XML DE RETOUR 
            XmlDocument xmlResult = new XmlDocument();
            xmlResult.AppendChild(xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null));
            _baseResultNode = xmlResult.CreateElement("result");
            xmlResult.AppendChild(_baseResultNode);


            // XML NAVABAR           
            _baseNavBarNode = _xmlNavBar.CreateElement("navbar");
            _xmlNavBar.AppendChild(_baseNavBarNode);

            //
            int nHeight = 800; // Taile de l'écran
            int _nWidth = 600;
            int _nActiveTab = 0;

            if (_allKeys.Contains("W") && !string.IsNullOrEmpty(_context.Request.Form["W"]))
                int.TryParse(_context.Request.Form["W"].ToString(), out _nWidth);

            if (_allKeys.Contains("H") && !string.IsNullOrEmpty(_context.Request.Form["H"]))
                int.TryParse(_context.Request.Form["H"].ToString(), out nHeight);

            if (_allKeys.Contains("activeTab") && !string.IsNullOrEmpty(_context.Request.Form["activeTab"]))
                int.TryParse(_context.Request.Form["activeTab"].ToString(), out _nActiveTab);

            if (_allKeys.Contains("xsltserver") && !string.IsNullOrEmpty(_context.Request.Form["xsltserver"]))
            {
                if (_context.Request.Form["xsltserver"].ToString() == "0")
                    _bServerSideXSLT = false;
            }

            #endregion

            int groupId = _pref.User.UserGroupId;
            int userLevel = _pref.User.UserLevel;

            string lang = _pref.Lang;

            int userId = _pref.User.UserId;
            string instance = _pref.GetSqlInstance;
            string baseName = _pref.GetBaseName;

            if (userId == 0)
            {
                //Session Invalide
                LaunchError(eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), eResApp.GetRes(_pref, 6559), eResApp.GetRes(_pref, 72)));
            }

            string listTab = string.Empty;
            StringBuilder divNavBar = new StringBuilder();

            // Liste des tables à afficher
            listTab = _pref.GetTabs(ePrefConst.PREF_SELECTION_TAB.TABORDER);

            //
            Dictionary<int, string> dic = eLibTools.GetRes(_pref, listTab, _pref.Lang, out error);

            if (error.Length != 0)
                throw new Exception(error);

            List<int> tableaulistTab = listTab.ConvertToListInt(";");

            bool isAdminAvailable = eFeaturesManager.IsFeatureAvailable(_pref, eConst.XrmFeature.AdminTabs);
            List<int> lstNoAdmin;
            if (isAdminAvailable)
            {
                lstNoAdmin = eLibTools.GetDescAdvInfo(_pref, tableaulistTab, new List<DESCADV_PARAMETER>() { DESCADV_PARAMETER.NOAMDMIN }).Where(aa => aa.Value.Find(dd => dd.Item1 == DESCADV_PARAMETER.NOAMDMIN && dd.Item2 == "1") != null).Select(t => t.Key).ToList();
                lstNoAdmin.Add((int)TableType.PAYMENTTRANSACTION);
            }
            else
                lstNoAdmin = new List<int>();

            int _nIdx = 0;
            int _nSizeTab = 0;
            int _nSizeNavBar = 0;
            Double _nMaxSizeNavBar;

            //Max de tab 
            _nMaxSizeNavBar = (_nWidth) - 20;

            int _nTagPage = 1;
            eudoDAL dal = null;
            try
            {
                dal = eLibTools.GetEudoDAL(_pref);
                dal.OpenDatabase();

                string sError = string.Empty;

                IDictionary<int, bool> diPerm = GetAddPermission(tableaulistTab, dal, out sError);
                if (sError.Length > 0 || diPerm == null)
                {
                    LaunchError(eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 72),
                        eResApp.GetRes(_pref, 6560),
                        eResApp.GetRes(_pref, 72),
                        sError));
                }

                DescAdvDataSet descAdv = new DescAdvDataSet();
                descAdv.LoadAdvParams(eDal: dal,
                    listDescid: tableaulistTab,
                    searchedParams: new List<DESCADV_PARAMETER> { DESCADV_PARAMETER.PURPLE_ACTIVATED_FROM }
                    );

                eParam param = new eParam(_pref);

                if (!param.InitTabNelleErgo(dal, out sError))
                {
                    ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, string.Concat(eResApp.GetRes(_pref, 416)), sError);
                    LaunchError(ErrorContainer);
                }

                // Parcours des onglets à afficher
                int nSizeHomePageLabel = 0;
                foreach (int _nDescidTab in tableaulistTab)
                {
                    // Nom de la table
                    string sTabName = string.Empty;
                    if (!dic.TryGetValue(_nDescidTab, out sTabName))
                        continue;

                    if (_nDescidTab == TableType.XRMPRODUCT.GetHashCode())
                    {
                        if (!eFeaturesManager.IsFeatureAvailable(_pref, eConst.XrmFeature.AdminProduct))
                            continue;
                    }

                    eTableLiteNavBar etab = new eTableLiteNavBar(_nDescidTab);

                    if (_nDescidTab > 0)
                    {
                        error = string.Empty;

                        etab.ExternalLoadInfo(dal, out error);

                        if (error.Length != 0)
                            LaunchError(eErrorContainer.GetDevUserError(
                                eLibConst.MSG_TYPE.CRITICAL,
                                eResApp.GetRes(_pref, 72),
                                eResApp.GetRes(_pref, 6561),
                                eResApp.GetRes(_pref, 72), error));
                    }


                    int nfs = 11;
                    if (!int.TryParse(_pref.FontSize, out nfs) || nfs < 10)
                        nfs = 11;

                    System.Drawing.Font font = new Font(TAB_FONT_FAMILLY, nfs);
                    int nSizeBkm = eTools.MesureString(sTabName, font);

                    _nSizeTab = TAB_LEFT_MARG + TAB_RIGHT_MARG + nSizeBkm;

                    if (_nDescidTab == 0)
                        nSizeHomePageLabel = _nSizeTab;
                    _nSizeNavBar += _nSizeTab;

                    XmlNode _blocTab = _xmlNavBar.CreateElement("tab");

                    //Descid
                    XmlAttribute _xmlDescId = _xmlNavBar.CreateAttribute("descid");
                    _xmlDescId.Value = _nDescidTab.ToString();
                    _blocTab.Attributes.Append(_xmlDescId);

                    //Type
                    XmlAttribute _xmlType = _xmlNavBar.CreateAttribute("type");
                    _xmlType.Value = etab.EdnType.GetHashCode().ToString();
                    _blocTab.Attributes.Append(_xmlType);

                    //Libelle
                    XmlAttribute _xmlLabel = _xmlNavBar.CreateAttribute("label");
                    _xmlLabel.Value = sTabName;
                    _blocTab.Attributes.Append(_xmlLabel);

                    // Infobulle (#50289)
                    Dictionary<eLibConst.CONFIG_DEFAULT, string> defConfig = _pref.GetConfigDefault(new List<eLibConst.CONFIG_DEFAULT> { eLibConst.CONFIG_DEFAULT.TOOLTIPTEXTENABLED });
                    XmlAttribute _xmlTooltipEnabled = _xmlNavBar.CreateAttribute("tooltiptextenabled");
                    _xmlTooltipEnabled.Value = defConfig[eLibConst.CONFIG_DEFAULT.TOOLTIPTEXTENABLED];
                    _blocTab.Attributes.Append(_xmlTooltipEnabled);
                    XmlAttribute _xmlTooltip = _xmlNavBar.CreateAttribute("tooltiptext");
                    _xmlTooltip.Value = (!string.IsNullOrEmpty(etab.ToolTipText)) ? etab.ToolTipText : "";
                    _blocTab.Attributes.Append(_xmlTooltip);

                    if (_nActiveTab == _nDescidTab)
                    {
                        XmlAttribute _xmlActiveTab = _xmlNavBar.CreateAttribute("active");
                        _blocTab.Attributes.Append(_xmlActiveTab);
                    }

                    //si la taille de la navbar devient trop importante, on n'affiche pas les onglets
                    // dans la nav mais sous le bouton '+' (sauf si c'est la table active )
                    // On ajoute le tag "hidden" qui sera traité en xslt
                    XmlAttribute _xmlHiddenTab = _xmlNavBar.CreateAttribute("ednTabPage");
                    if (_nSizeNavBar > _nMaxSizeNavBar)
                    {
                        //Taille de départ de la page 2 : Taille du signet ayant fait dépasser + taille accueil
                        _nSizeNavBar = _nSizeTab + nSizeHomePageLabel;
                        _nTagPage++;
                    }

                    if (_nDescidTab == 0)
                        _xmlHiddenTab.Value = "0";
                    else
                        _xmlHiddenTab.Value = _nTagPage.ToString();

                    _blocTab.Attributes.Append(_xmlHiddenTab);

                    //Size lib
                    XmlAttribute _xmlSizeLabel = _xmlNavBar.CreateAttribute("sizelabel");
                    _xmlSizeLabel.Value = _nSizeTab.ToString();
                    _blocTab.Attributes.Append(_xmlSizeLabel);

                    if (_nDescidTab == 0)
                    {
                        // _baseNavBarNode.AppendChild(_blocTab);
                        //  _nIdx++;
                        //  continue;                        
                    }

                    /* ACTIONS */
                    XmlNode _blocActions = _xmlNavBar.CreateElement("actions");
                    XmlAttribute _xmlTitle = _xmlNavBar.CreateAttribute("title");
                    _xmlTitle.Value = eResApp.GetRes(_pref, 296); // Nom du menu Action
                    _blocActions.Attributes.Append(_xmlTitle);


                    if (_nDescidTab != 0 && etab.CalendarEnabled && etab.EdnType == EdnType.FILE_PLANNING)
                    {
                        //         _blocActions.AppendChild(addAction(eResApp.GetRes(_pref.Lang, 742), string.Concat("showTplPlanning(", _sDescidTab, ",0", ");")));
                        _blocActions.AppendChild(addAction(string.Concat(eResApp.GetRes(_pref, 1047), " (", eResApp.GetRes(_pref, 822), ")"), string.Concat("setCalViewMode(", EudoQuery.CalendarViewMode.VIEW_CAL_DAY_PER_USER.GetHashCode(), ",", _nDescidTab, ",", _pref.User.UserId, ",'", DateTime.Now.ToString("dd/MM/yyyy"), "');")));
                        _blocActions.AppendChild(addAction(string.Concat(eResApp.GetRes(_pref, 1047), " (", eResApp.GetRes(_pref, 821), ")"), string.Concat("setCalViewMode(", EudoQuery.CalendarViewMode.VIEW_CAL_WORK_WEEK.GetHashCode(), ",", _nDescidTab, ",", _pref.User.UserId, ",'", DateTime.Now.ToString("dd/MM/yyyy"), "');")));
                        _blocActions.AppendChild(addAction(eResApp.GetRes(_pref, 1048), string.Concat("setCalViewMode(", EudoQuery.CalendarViewMode.VIEW_CAL_TASK.GetHashCode(), ",", _nDescidTab, ",", _pref.User.UserId, ",'", DateTime.Now.ToString("dd/MM/yyyy"), "');")));
                    }


                    // Ajouter
                    //  -> Correspond au droit de création en mode liste
                    bool bAddAllowed = false;
                    diPerm.TryGetValue(_nDescidTab, out bAddAllowed);


                    if (_nDescidTab != 0 && bAddAllowed && etab.EdnType != EdnType.FILE_WEBTAB && etab.EdnType != EdnType.FILE_GRID)
                    {
                        if (etab.EdnType != EdnType.FILE_PLANNING)
                        {
                            ///on cherche là où le mode purple assistant à la création est activé
                            List<LOCATION_PURPLE_ACTIVATED> locations = descAdv.GetAdvInfoValue(_nDescidTab, DESCADV_PARAMETER.PURPLE_ACTIVATED_FROM)
                              .ConvertToList<LOCATION_PURPLE_ACTIVATED>(",", new Converter<string, LOCATION_PURPLE_ACTIVATED?>(
                                  delegate (string s)
                                  {
                                      LOCATION_PURPLE_ACTIVATED location = LOCATION_PURPLE_ACTIVATED.UNDEFINED;
                                      if (!Enum.TryParse<LOCATION_PURPLE_ACTIVATED>(s, out location))
                                          return LOCATION_PURPLE_ACTIVATED.UNDEFINED;

                                      return location;
                                  }));


                            //SHA : bug bouton "Nouveau" masqué dans le menu de droit et visible dans le menu navbar (ajout FILE_TARGET) pour les cibles étendues
                            if (etab.EdnType == EudoQuery.EdnType.FILE_STANDARD || etab.EdnType == EudoQuery.EdnType.FILE_MAIL || etab.EdnType == EudoQuery.EdnType.FILE_TARGET)
                            {

                            }
                            else if (etab.EdnType == EudoQuery.EdnType.FILE_HISTO)
                            {


                            }
                            else if (_nDescidTab == TableType.PP.GetHashCode() || _nDescidTab == TableType.PM.GetHashCode())
                            {

                                if (param.ParamTabNelleErgoGuided.Any(eg => eg == _nDescidTab)
                                    && !new List<int> { TableType.PP.GetHashCode(), TableType.PM.GetHashCode() }.Any(elm => elm == _nDescidTab))
                                {
                                    string strClosendApplyOnly = (etab.EdnType == EudoQuery.EdnType.FILE_MAIN) ? "true" : "false";  //Ne pas afficher Appliquer et fermer si EVENT
                                    string sJsAction = string.Concat("shFileInPopup(", _nDescidTab, ",0, '", eResApp.GetRes(_pref, 31).Replace("'", @"\'"), "',null,null,0,'',", strClosendApplyOnly, ",null, 1) ");
                                    _blocActions.AppendChild(addAction(eResApp.GetRes(_pref, 31), sJsAction));
                                }
                                else
                                {
                                    //SI PM ou PP on recherche avant de faire un ajout
                                    _blocActions.AppendChild(addAction(eResApp.GetRes(_pref, 31), string.Concat("openLnkFileDialog(", eFinderList.SearchType.Add.GetHashCode(), ",", _nDescidTab, ", null, 1) ")));

                                    if (param.ParamTabNelleErgoGuided.Any(eg => eg == _nDescidTab) &&
                                       locations.Contains(LOCATION_PURPLE_ACTIVATED.NAVBAR))
                                    {
                                        //3005 Assistant création
                                        _blocActions.AppendChild(addAction(eResApp.GetRes(_pref, 3005),
                                            string.Concat("openLnkFileDialog(", eFinderList.SearchType.Add.GetHashCode(), ",",
                                            _nDescidTab, ",",
                                            "null ,",
                                            (int)eConst.ShFileCallFrom.CallFromNavBarToPurple, //le menu est considéré comme une navbar
                                            ") ")));

                                    }

                                }
                            }
                            else if (_nDescidTab == TableType.CAMPAIGN.GetHashCode() || _nDescidTab == TableType.PJ.GetHashCode() || _nDescidTab == TableType.PAYMENTTRANSACTION.GetHashCode())
                            {
                                // pas de bouton Nouveau sur l'onglet Ajout
                            }
                            else
                            {
                                //   _blocActions.AppendChild(addAction(eResApp.GetRes(_pref.Lang, 742), "loadFile(" + _nDescidTab + ",0,5);"));

                                string sJsAction = string.Empty;
                                if (etab.AutoCreate)
                                    sJsAction = string.Concat("autoCreate(", _nDescidTab, ")");
                                else
                                {
                                    string strClosendApplyOnly = (etab.EdnType == EudoQuery.EdnType.FILE_MAIN) ? "true" : "false";  //Ne pas afficher Appliquer et fermer si EVENT
                                    sJsAction = string.Concat("shFileInPopup(", _nDescidTab, ",0, '", eResApp.GetRes(_pref, 31).Replace("'", @"\'"), "',null,null,0,'',", strClosendApplyOnly, ",null, 1) ");
                                }

                                _blocActions.AppendChild(addAction(eResApp.GetRes(_pref, 31), sJsAction));

                                #region Ajout depuis le nouveau modefiche téléguidé
                                if (param.ParamTabNelleErgoGuided.Any(eg => eg == _nDescidTab) &&
                                    locations.Contains(LOCATION_PURPLE_ACTIVATED.NAVBAR))
                                {
                                    //3005 Assistant création
                                    _blocActions.AppendChild(addAction(eResApp.GetRes(_pref, 3005), String.Format("openPurpleFile({0},{1},{2},{3})", _nDescidTab, 0, "''", (int)eConst.ShFileCallFrom.CallFromNavBar)));
                                }

                                #endregion
                            }
                        }
                    }

                    //Mode Liste
                    if (etab.EdnType == EdnType.FILE_PLANNING)
                        _blocActions.AppendChild(addAction(eResApp.GetRes(_pref, 1485), string.Concat("setCalViewMode(", EudoQuery.CalendarViewMode.VIEW_CAL_LIST.GetHashCode(), ", ", _nDescidTab, ");")));
                    else if (_nDescidTab != 0 && etab.EdnType != EdnType.FILE_WEBTAB && etab.EdnType != EdnType.FILE_GRID)
                        _blocActions.AppendChild(addAction(eResApp.GetRes(_pref, 1485), string.Concat("goTabList(", _nDescidTab, ", true, null, null, true);")));

                    //Recherche avancée (LOUPE)  seulement pour EVENT, PP et PM
                    if (_nDescidTab != 0 && ((etab.TabType == TableType.EVENT) || (etab.TabType == TableType.PP) || (etab.TabType == TableType.ADR) || (etab.TabType == TableType.PM)))
                    {
                        _blocActions.AppendChild(addAction(eResApp.GetRes(_pref, 983)
                            , string.Concat("openLnkFileDialog(", eFinderList.SearchType.Search.GetHashCode(), ",",
                            _nDescidTab, ",",
                            "null ,",
                            (int)eConst.ShFileCallFrom.CallFromNavBar,
                            ") ")));
                    }
                    // Ajoute le blocs d'action au bloc de l'onglet
                    _blocTab.AppendChild(_blocActions);

                    /*   MRU seulement pour EVENT, PP et PM et ADR*/
                    /*  Les mru sont chargés dynamiquement par la frame des params */
                    if (_nDescidTab != 0 && ((etab.TabType == TableType.EVENT) || (etab.TabType == TableType.PP) || (etab.TabType == TableType.ADR) || (etab.TabType == TableType.PM) || (etab.TabType == TableType.CAMPAIGN)))
                    {

                        //Vérification des droits de visu sur le champ principal

                        List<int> lstMainField = new List<int>();
                        lstMainField.Add(etab.MainFieldDescId);
                        Dictionary<int, bool> viewAllowed = ePermission.GetViewPermission(lstMainField, _pref, out sError);
                        if (sError.Length == 0)
                        {
                            bool allowed = false;
                            viewAllowed.TryGetValue(etab.MainFieldDescId, out allowed);

                            XmlNode _blocMrus = _xmlNavBar.CreateElement("mrus");
                            _xmlTitle = _xmlNavBar.CreateAttribute("title");
                            _xmlTitle.Value = eResApp.GetRes(_pref, 1721).Replace("<TAB>", sTabName); //mru
                            _blocMrus.Attributes.Append(_xmlTitle);
                            _blocTab.AppendChild(_blocMrus);
                        }
                    }

                    /*  ADMINISTRER */
                    /*KHA CLE config*/
                    if (_pref.IsAdminEnabled && isAdminAvailable)
                    {
                        if (!lstNoAdmin.Contains(etab.DescId))
                        {

                            XmlNode _blocAdmin = _xmlNavBar.CreateElement("admin");
                            _xmlTitle = _xmlNavBar.CreateAttribute("title");
                            _xmlTitle.Value = eResApp.GetRes(_pref, 326); //mru
                            _blocAdmin.Attributes.Append(_xmlTitle);
                            _blocTab.AppendChild(_blocAdmin);
                        }
                    }


                    _baseNavBarNode.AppendChild(_blocTab);
                    _nIdx++;
                }
            }
            catch (eEndResponseException) { }
            catch (ThreadAbortException ee)
            {

            }

            catch (Exception ex)
            {
                LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), "", eResApp.GetRes(_pref, 72), ex.ToString()));
            }
            finally
            {
                if (dal != null)
                    dal.CloseDatabase();
            }

            //Affichage bouton (+)
            XmlNode _blocPlus = _xmlNavBar.CreateElement("plus");

            /*  Vue enregistrée */
            XmlNode _blockPlusView = _xmlNavBar.CreateElement("views");
            XmlAttribute _xmlLabelPlusView = _xmlNavBar.CreateAttribute("label");
            _xmlLabelPlusView.Value = eResApp.GetRes(_pref, 6197);
            _blockPlusView.Attributes.Append(_xmlLabelPlusView);

            XmlNode _blockPlusTabs = _xmlNavBar.CreateElement("tabs");
            XmlAttribute _xmlLabelPlusTabs = _xmlNavBar.CreateAttribute("label");
            _xmlLabelPlusTabs.Value = eResApp.GetRes(_pref, 217); // Onglets
            _blockPlusTabs.Attributes.Append(_xmlLabelPlusTabs);

            _blockPlusTabs.AppendChild(addAction(eResApp.GetRes(_pref, 218), "setTabOrder()"));  // Choix des onglets
            _blockPlusTabs.AppendChild(addAction(eResApp.GetRes(_pref, 5054), "NewTab()")); // Nouvelle vue
            _blocPlus.AppendChild(_blockPlusTabs);

            /*  ADMINISTER */
            /*KHA CLE config*/

#if DEBUG
            if (userLevel >= UserLevel.LEV_USR_ADMIN.GetHashCode())
            {

                XmlNode _blocAdmin = _xmlNavBar.CreateElement("admin");
                XmlAttribute _xmlTitleAdmin = _xmlNavBar.CreateAttribute("title");
                _xmlTitleAdmin.Value = eResApp.GetRes(_pref, 326); //mru
                _blocAdmin.Attributes.Append(_xmlTitleAdmin);
                _blockPlusView.AppendChild(_blocAdmin);

            }
            else
            {
                /* Fin de bloc */
            }
#endif


            // Nombre de pages d'onglets
            XmlAttribute _xmlNbTabPage = _xmlNavBar.CreateAttribute("nbPageTab");
            _xmlNbTabPage.Value = _nTagPage.ToString();
            _baseNavBarNode.Attributes.Append(_xmlNbTabPage);


            XmlAttribute _xmlNFontsize = _xmlNavBar.CreateAttribute("fontsize");
            _xmlNFontsize.Value = eTools.GetClassNameFontSize(_pref);
            _baseNavBarNode.Attributes.Append(_xmlNFontsize);

            // Nombre d'onglets
            // A ne pas confondre avec l'attribut nbTab qui indique la page actuellement affichée (...)
            XmlAttribute _xmlNbTabs = _xmlNavBar.CreateAttribute("nbTabs");
            _xmlNbTabs.Value = tableaulistTab.Count.ToString();
            _baseNavBarNode.Attributes.Append(_xmlNbTabs);

            XmlAttribute _xmlAdminActive = _xmlNavBar.CreateAttribute("isAdminActive");
            /*KHA CLE config*/
#if DEBUG
            _xmlAdminActive.Value = "1";
#else
            _xmlAdminActive.Value = "0";
#endif
            _baseNavBarNode.Attributes.Append(_xmlAdminActive);

            _blocPlus.AppendChild(_blockPlusView);
            _baseNavBarNode.AppendChild(_blocPlus);

            RenderResult(RequestContentType.TEXT, delegate () { return eXSLT.NavBarHTML(_xmlNavBar); });
        }

        /// <summary>
        /// Génère un noeud d'entrée "action"
        /// </summary>
        /// <param name="sLibelle">Libellé de l'entrée</param>
        /// <param name="sAction">Action au click</param>
        /// <returns></returns>
        private XmlNode addAction(string sLibelle, string sAction)
        {
            XmlNode _entry = _xmlNavBar.CreateElement("action");
            //Ajout du node Label
            if (!string.IsNullOrEmpty(sLibelle))
            {
                XmlNode _label = _xmlNavBar.CreateElement("label");
                _label.AppendChild(_xmlNavBar.CreateTextNode(sLibelle));
                _entry.AppendChild(_label);

            }

            // Ajout du node action
            if (!string.IsNullOrEmpty(sAction))
            {
                XmlNode _action = _xmlNavBar.CreateElement("action");
                _action.AppendChild(_xmlNavBar.CreateTextNode(sAction));
                _entry.AppendChild(_action);
            }
            return _entry;

        }

        /// <summary>
        /// récupère dans un dictionnaire les droits d'ajout en mode liste
        /// </summary>
        /// <param name="listTab">liste des onglets dont on doit récupérer les droits d'ajout</param>
        /// <param name="edal"></param>
        /// <param name="sError"></param>
        /// <returns></returns>
        private IDictionary<int, bool> GetAddPermission(ICollection<int> listTab, eudoDAL edal, out string sError)
        {
            sError = string.Empty;

            if (listTab == null || listTab.Count == 0)
                return null;

            Dictionary<int, bool> diPerm = new Dictionary<int, bool>();

            RqParam rqPerm = new RqParam();

            StringBuilder sbQuery = new StringBuilder();
            sbQuery.AppendLine("SELECT [TRAIT].[TraitId] - @PermNum AS [TAB], isnull([PERM].[P],1) AS [PERM] ")
                .AppendLine("FROM [DESC] LEFT JOIN [TRAIT] ON [DESC].[DESCID] = [TRAIT].[TraitId] - @PermNum ")
                .AppendLine("LEFT JOIN [dbo].[cfc_getPermInfo](@userid, @userlevel, @groupid) [PERM] ON [TRAIT].[PermId] = [PERM].[permissionid] ")
                .AppendLine("WHERE [TRAIT].[TraitId] IN (");

            int i = 0;
            string sParam = string.Empty;

            foreach (int tabDescId in listTab)
            {
                // Autorise l'ajout sur la table si le trait de celle-ci n'a pas été défini
                if (diPerm.ContainsKey(tabDescId))
                    continue;

                if (i > 0)
                    sbQuery.AppendLine(", ");

                sParam = string.Concat("@value", i);

                sbQuery.Append(sParam);

                rqPerm.AddInputParameter(sParam, SqlDbType.Int, tabDescId + ProcessRights.PRC_RIGHT_ADD_LIST);

                i++;

                diPerm.Add(tabDescId, true);
            }

            sbQuery.Append(")");

            rqPerm.AddInputParameter("@userid", SqlDbType.Int, _pref.User.UserId);
            rqPerm.AddInputParameter("@userlevel", SqlDbType.Int, _pref.User.UserLevel);
            rqPerm.AddInputParameter("@groupid", SqlDbType.Int, _pref.User.UserGroupId);
            rqPerm.AddInputParameter("@PermNum", SqlDbType.Int, ProcessRights.PRC_RIGHT_ADD_LIST);

            rqPerm.SetQuery(sbQuery.ToString());

            DataTableReaderTuned dtrPerm = edal.Execute(rqPerm, out sError);
            try
            {
                if (sError.Length > 0)
                    return null;
                int nTab = 0;
                int nAddPerm = 0;
                while (dtrPerm.Read())
                {
                    if (!int.TryParse(dtrPerm.GetString(0), out nTab))
                        continue;

                    nAddPerm = dtrPerm.GetInt32(1);
                    diPerm[nTab] = nAddPerm > 0;
                }
            }
            finally
            {
                if (dtrPerm != null)
                    dtrPerm.Dispose();
            }

            return diPerm;
        }




    }
}