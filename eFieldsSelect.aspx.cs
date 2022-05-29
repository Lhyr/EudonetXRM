using Com.Eudonet.Internal;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;
using EudoExtendedClasses;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Page de sélection des rubriques
    /// TODO : A refaire intégralement.
    /// 
    /// </summary>
    public partial class eFieldsSelect : eEudoPage
    {
        int _nViewTab = 0;
        int _nCalledTab = 0;
        int _nParentTab = 0;

        /// <summary>
        /// table demandée depuis le manager, avant transformation éventuelle
        /// </summary>
        int _nOrigCalledTab = 0;

        bool _bFromInvit = false;
        bool _bFromGrid = false;
        bool _bTarget = false;
        bool _bDeleteMode = false;
        bool _bFileRelation = false;

        /// <summary>Liste de champs personnalisés demandé</summary>
        string _listCustomCol;

        ISet<int> _listTab = new HashSet<int>();

        /// <summary>Liste de descid qui ne doivent pas être affichés</summary>
        private List<int> _listColNotDisplayed = null;

        /// <summary>
        /// Type Eudonet de la table
        /// </summary>
        private EdnType _tabEdnType = EdnType.FILE_UNDEFINED;

        /// <summary>
        /// Action
        /// </summary>
        string _action = "init";

        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return scriptHolder;
        }

        /// <summary>
        /// Page de sélection des rubriques
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            #region ajout des css

            PageRegisters.AddCss("eFieldsSelect");

            #endregion

            #region add js



            PageRegisters.AddScript("eDrag");
            PageRegisters.AddScript("eTabsFieldsSelect");
            PageRegisters.AddScript("eUpdater");
            PageRegisters.AddScript("eTools");
            PageRegisters.AddScript("eModalDialog");


            #endregion

            _nCalledTab = _requestTools.GetRequestFormKeyI("Tab") ?? 0;

            if (_nCalledTab == 0)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 72),
                    eResApp.GetRes(_pref, 2024).Replace(" <PARAM> ", " "),
                    eResApp.GetRes(_pref, 72),
                    String.Concat(eResApp.GetRes(_pref, 2024).Replace("<PARAM>", "descid"), " (descid = ", _nCalledTab, ")")
                );

                try
                {
                    //Arrete le traitement et envoi l'erreur
                    LaunchError();
                }
                catch (eEndResponseException)
                { }
            }

            _nOrigCalledTab = _nCalledTab;
            _nParentTab = _requestTools.GetRequestFormKeyI("parentTab") ?? 0;
            _listCustomCol = _requestTools.GetRequestFormKeyS("listCol") ?? "";
            _listColNotDisplayed = _requestTools.GetRequestIntListFormKeyS(";", "listColNotDisplay");

            _bDeleteMode = (_requestTools.GetRequestFormKeyI("delete") ?? 0) == 1;

            // Traitement des signets spéciaux (PJ Doublons etc.) dont le tabdescid passé en paramètre n'est pas forcément celui de la table réelle.
            if (_nCalledTab % 100 == AllField.ATTACHMENT.GetHashCode())
                _nViewTab = TableType.PJ.GetHashCode();
            else if (_nCalledTab == TableType.DOUBLONS.GetHashCode() || _nCalledTab % 100 == AllField.BKM_PM_EVENT.GetHashCode())
                _nViewTab = _nParentTab;
            else
                _nViewTab = _nCalledTab;

            _action = (_requestTools.GetRequestFormKeyS("action") ?? "init").ToLower();

            _bFromGrid = _requestTools.GetRequestFormKeyB("bFromGrid") ?? false;



            Boolean bFromLnkFile = false;

            if (_action.ToLower() == "initivt")
            {

                // ++ et xx

                _nViewTab = TableType.PP.GetHashCode();
                _nCalledTab = TableType.PP.GetHashCode();
                _action = "init";

                _bFromInvit = true;
            }
            else if (_action.ToLower() == "initselection")
            {
                //Carto


                //_nViewTab = TableType.PP.GetHashCode();
                //_nCalledTab = TableType.PP.GetHashCode();
                _action = "init";
            }
            else if (_action.ToLower() == "initlnkfile")
            {
                bFromLnkFile = true;
                _action = "init";
            }

            string listColViewed = String.Empty;
            TableLite tab = null;
            bool isPlanning = false;
            bool isCalendarEnabled = false;
            bool isCalendarGraphEnabled = false;
            bool bForceListMode = (Request.Form["forcelst"] != null && Request.Form["forcelst"].ToString().Equals("1"));

            if (_action.ToLower() == "init"
                || _action.ToLower() == "loadtab"
                || _action.ToLower() == "initbkm")
            {
                #region informations de table

                string error = String.Empty;
                eudoDAL dal = eLibTools.GetEudoDAL(_pref);

                try
                {
                    dal.OpenDatabase();

                    tab = eLibTools.GetTableInfo(dal, _nViewTab, TableLite.Factory());
                    _tabEdnType = tab.EdnType;

                    // Si c'est une table relation, on se redirige vers la table parent
                    if (_action.ToLower() == "initbkm")
                    {
                        if (tab.EdnType == EdnType.FILE_RELATION)
                        {
                            _bFileRelation = true;
                            _nViewTab = _nParentTab;
                            tab = eLibTools.GetTableInfo(dal, _nViewTab, TableLite.Factory());
                        }
                        else
                        {
                            tab = eLibTools.GetTableInfo(dal, _nCalledTab, TableLite.Factory());
                        }

                    }

                    if (_bFromInvit)
                    {
                        eTableLiteMailing tabInvit = eLibTools.GetTableInfo(dal, _nOrigCalledTab, eTableLiteMailing.Factory(_pref));
                        _bTarget = tabInvit.ProspectEnabled;
                    }
                }
                catch (Exception exp)
                {
                    // TODO - Gestion d'erreur ?
                    error = exp.Message;
                }
                finally
                {
                    dal.CloseDatabase();
                }

                #endregion

                if (tab == null)
                {
                    // TODO - Gestion d'erreur ?
                    return;
                }

                #region informations de planning

                if (_action.ToLower() == "init" || _action.ToLower() == "loadtab" || _action.ToLower() == "initbkm")
                {
                    isPlanning = tab.EdnType == EdnType.FILE_PLANNING;
                    isCalendarEnabled = isPlanning && _pref.GetPref(_nViewTab, ePrefConst.PREF_PREF.CALENDARENABLED).Equals("1");

                    EudoQuery.CalendarViewMode calViewMode = (EudoQuery.CalendarViewMode)eLibTools.GetNum(_pref.GetPref(_nViewTab, ePrefConst.PREF_PREF.VIEWMODE));
                    EudoQuery.CalendarTaskMode calTaskMode = (EudoQuery.CalendarTaskMode)eLibTools.GetNum(_pref.GetPref(_nViewTab, ePrefConst.PREF_PREF.CALENDARTASKMODE));

                    if (_bFromGrid)
                        calViewMode = EudoQuery.CalendarViewMode.VIEW_CAL_LIST;

                    isCalendarGraphEnabled = isCalendarEnabled && !bForceListMode && (calViewMode == CalendarViewMode.VIEW_CAL_WORK_WEEK || calViewMode == CalendarViewMode.VIEW_CAL_MONTH
                                 || calViewMode == CalendarViewMode.VIEW_CAL_DAY_PER_USER);

                    // Planning non graphique : les rubriques spécialisé du planning ne doivent pas être affichées
                    //BSE : #50 733 , DATEDESCID et HISTODESCID Varie d'une table à l'autre, ne pas prendre le descid 1 et 2 par défault
                    if (tab.TabType == TableType.TEMPLATE && !isCalendarEnabled)
                    {
                        _listColNotDisplayed.AddRange((((IEnumerable<PlanningField>)Enum.GetValues(typeof(PlanningField))).Where(f => (int)f >= eLibConst.MAX_NBRE_FIELD).Select(i => _nViewTab + i.GetHashCode())).ToList());
                        //  _listColNotDisplayed.Add(eLibTools.GetNum(_pref.GetPref(_nViewTab, ePrefConst.PREF_PREF.HISTODESCID)));
                        //  _listColNotDisplayed.Add(eLibTools.GetNum(_pref.GetPref(_nViewTab, ePrefConst.PREF_PREF.DATEDESCID)));
                    }
                }

                #endregion
            }

            bool removeField01 = false;

            switch (_action.ToLower())
            {
                #region Chargement initial list
                case "init":

                    String listCol = String.Empty;
                    String lblSel = String.Empty;

                    // Selection active
                    Dictionary<ePrefConst.PREF_SELECTION, String> _prefSel;

                    HtmlGenericControl viewMode = new HtmlGenericControl("input");
                    viewMode.ID = "calviewmode";
                    viewMode.Style.Add(HtmlTextWriterStyle.Display, "none");
                    viewMode.Attributes.Add("value", isCalendarGraphEnabled ? "1" : "0");
                    DivViewMode.Controls.Add(viewMode);

                    HtmlGenericControl forceList = new HtmlGenericControl("input");
                    forceList.ID = "forcelistmode";
                    forceList.Style.Add(HtmlTextWriterStyle.Display, "none");
                    forceList.Attributes.Add("value", bForceListMode ? "1" : "0");
                    DivViewMode.Controls.Add(forceList);


                    if (_bFromInvit)
                    {

                        eColsPref colsPref;

                        if (_bTarget)
                            colsPref = new eColsPref(_pref, _nOrigCalledTab, _bDeleteMode ? ColPrefType.TARGETDELETE : ColPrefType.TARGETADD);
                        else
                            colsPref = new eColsPref(_pref, _nOrigCalledTab, _bDeleteMode ? ColPrefType.INVITDELETE : ColPrefType.INVITADD);



                        listCol = colsPref.GetColsPref(eLibConst.PREF_COLSPREF.Col);

                    }
                    else if (isCalendarGraphEnabled)
                    {
                        listCol = _pref.GetPref(_nViewTab, ePrefConst.PREF_PREF.CALENDARCOL);
                        lblSel = string.Empty;
                    }
                    else if (_listCustomCol != null)
                    {
                        //Liste de champs personnalisés demandé
                        listCol = _listCustomCol;
                    }
                    else
                    {
                        _prefSel = _pref.GetSelection(_nViewTab, new ePrefConst.PREF_SELECTION[] { ePrefConst.PREF_SELECTION.LISTCOL, ePrefConst.PREF_SELECTION.LABEL });
                        if (_prefSel.Count > 0)
                        {
                            listCol = _prefSel[ePrefConst.PREF_SELECTION.LISTCOL];
                            lblSel = _prefSel[ePrefConst.PREF_SELECTION.LABEL];

                            // #29532 - GMA - 20140407 : si aucune sélection de rubrique n'est présente, on charge les rubriques de l'utilisateur par défault (UserId = 0)
                            if (String.IsNullOrWhiteSpace(listCol))
                            {
                                ePref defaultUserPref = new ePref(_pref.GetSqlInstance, _pref.GetBaseName, _pref.GetSqlUser, _pref.GetSqlPassword, 0, _pref.Lang);
                                defaultUserPref.LoadConfig();
                                defaultUserPref.LoadPref();
                                // defaultUserPref.LoadTabs();
                                Dictionary<ePrefConst.PREF_SELECTION, String> _defaultPrefSel =
                                    defaultUserPref.GetSelection(_nViewTab, new ePrefConst.PREF_SELECTION[] { ePrefConst.PREF_SELECTION.LISTCOL, ePrefConst.PREF_SELECTION.LABEL });

                                if (_defaultPrefSel.Count > 0)
                                {
                                    listCol = _defaultPrefSel[ePrefConst.PREF_SELECTION.LISTCOL];
                                    lblSel = _defaultPrefSel[ePrefConst.PREF_SELECTION.LABEL];
                                }
                            }
                        }
                        else
                        {
                            listCol = string.Empty;
                            lblSel = string.Empty;
                        }
                    }

                    if (listCol.Length == 0 && !_bFromInvit && !bFromLnkFile && !_bFromGrid)
                    {
                        _pref.LoadPref();
                        _prefSel = _pref.GetSelection(_nViewTab, new ePrefConst.PREF_SELECTION[] { ePrefConst.PREF_SELECTION.LISTCOL, ePrefConst.PREF_SELECTION.LABEL });
                        if (_prefSel.Count > 0)
                        {
                            listCol = _prefSel[ePrefConst.PREF_SELECTION.LISTCOL];
                            lblSel = _prefSel[ePrefConst.PREF_SELECTION.LABEL];
                        }
                        else
                        {
                            listCol = string.Empty;
                            lblSel = string.Empty;
                        }
                    }

                    listColViewed = GetListColViewed(tab, listCol);

                    Int32 _selectId = eLibTools.GetNum(_pref.GetPref(_nViewTab, ePrefConst.PREF_PREF.SELECTID));

                    DivSelectionList.Controls.Add(FillSelectionList(_selectId));

                    DivMainFileList.Controls.Add(FillMainFileList());

                    removeField01 = RemoveField01(tab, listColViewed);

                    DivSourceList.Controls.Add(FillFieldList(_nViewTab, listColViewed, isCalendarGraphEnabled, removeField01));
                    DivTargetList.Controls.Add(FillSelectedFieldList(_nViewTab, listCol, removeField01));

                    break;
                #endregion

                #region Charger la liste des rubriques de type user uniquement sur la table (dans le cas d'un changement de table)
                case "tabuserfields":

                    _listTab.Add(_nViewTab);

                    DropDownList select = new DropDownList();

                    select.Attributes.Add("SelectedIndex", "0");
                    select.Items.Add(new ListItem("", _nViewTab.ToString()));
                    select.ID = string.Concat("MainFileList");
                    select.Style.Add("display", "none");

                    DivMainFileList.Controls.Add(select);

                    DivSourceList.Controls.Add(FillUserFieldList(_nViewTab, _listCustomCol));
                    DivTargetList.Controls.Add(FillSelectedFieldList(_nViewTab, _listCustomCol));
                    break;
                #endregion

                #region Charger la liste uniquement (dans le cas d'un changement de table)
                case "loadtab":

                    int tabToLoad = _requestTools.GetRequestFormKeyI("tabtoload") ?? 0;
                    String _itemused = Request.Form["itemused"].ToString();
                    HtmlGenericControl _lst = FillFieldList(tabToLoad, _itemused, isCalendarGraphEnabled, (tabToLoad == _nViewTab));

                    StringWriter sw = new StringWriter();
                    HtmlTextWriter w = new HtmlTextWriter(sw);
                    _lst.RenderControl(w);
                    Response.Clear();
                    Response.ClearContent();
                    Response.Write(sw.GetStringBuilder().ToString());
                    Response.End();
                    break;
                #endregion

                #region Charger la liste des rub selectionnées uniquement
                case "loadselectedtab":
                    String _itemusedSel = Request.Form["itemused"].ToString();
                    _listTab.UnionWith(_requestTools.GetRequestIntListFormKeyS(";", "listtab"));
                    HtmlGenericControl _lstSel = FillSelectedFieldList(_nViewTab, _itemusedSel, RemoveField01(tab, _itemusedSel));
                    StringWriter sw1 = new StringWriter();
                    HtmlTextWriter w1 = new HtmlTextWriter(sw1);
                    _lstSel.RenderControl(w1);
                    Response.Clear();
                    Response.ClearContent();
                    Response.Write(sw1.GetStringBuilder().ToString());
                    Response.End();
                    break;
                #endregion

                #region Enregistrer la vue
                case "savepref":
                    break;
                #endregion

                #region Chargement initial signet
                case "initbkm":
                    Int32 nBkmPrefBkm = _nCalledTab;

                    if (_nCalledTab % 100 == AllField.ATTACHMENT.GetHashCode())
                        nBkmPrefBkm = _nViewTab;

                    eBkmPref bkmPref = new eBkmPref(_pref, _nParentTab, nBkmPrefBkm);
                    String sBkmCol = bkmPref.GetBkmPref(ePrefConst.PREF_BKMPREF.BKMCOL);

                    bool removeField01fromSourceList = RemoveField01(tab, listColViewed);
                    bool removeField01fromTargetList = RemoveField01(tab, sBkmCol);
                    if (_bFileRelation)
                    {
                        listColViewed = sBkmCol;
                        if (String.IsNullOrEmpty(listColViewed))
                        {
                            listColViewed = (nBkmPrefBkm + 1).ToString();
                        }
                        removeField01fromSourceList = false;
                        removeField01fromTargetList = false;
                    }
                    else
                        listColViewed = GetListColViewed(tab, sBkmCol);

                    //SHA : tâche #2 089
                    if (_tabEdnType == EdnType.FILE_MAIL
                        && !_bFromGrid
                        && !_bFromInvit
                        && !_bFileRelation)
                        _listColNotDisplayed.Add(Convert.ToInt32(string.Format("{0}{1}", _nCalledTab / 100, (int)MailField.DESCID_MAIL_PREHEADER)));

                    DivMainFileList.Controls.Add(FillMainFileList());

                    // Pas de mode graphique planning en signet
                    DivSourceList.Controls.Add(FillFieldList(_nViewTab, listColViewed, false, removeField01fromSourceList));
                    DivTargetList.Controls.Add(FillSelectedFieldList(_nViewTab, sBkmCol, removeField01fromTargetList));

                    break;
                    #endregion
            }
        }

        private static String GetListColViewed(TableLite tab, String listCol)
        {
            String listColViewed = listCol;
            if (tab.EdnType == EudoQuery.EdnType.FILE_MAIN || tab.EdnType == EdnType.FILE_PJ)
            {
                if (!String.IsNullOrEmpty(listColViewed))
                    listColViewed = String.Concat(";", listColViewed);
                listColViewed = String.Concat(tab.MainFieldDescId, listColViewed);
            }
            return listColViewed;
        }

        /// <summary>
        /// récupère un ressourecs donnée
        /// </summary>
        /// <returns></returns>
        public string GetRes(int resId)
        {
            return eResApp.GetRes(_pref, resId);
        }

        /// <summary>
        /// Remplir la liste déroulante des fichiers liées à la table actuelle
        /// </summary>
        private DropDownList FillMainFileList()
        {
            string error = string.Empty;

            DropDownList lst = new DropDownList();
            lst.Attributes.Add("SelectedIndex", "0");
            lst.ID = string.Concat("MainFileList");

            _listTab.Clear();

            eudoDAL dal = eLibTools.GetEudoDAL(_pref);
            try
            {
                dal.OpenDatabase();

                //Dans le cas des ++/xx la liste des fiches est fixe (PP/PM/ADR + template Cible étendues si xx)
                if (!_bFromInvit
                    // && _nViewTab != (int)TableType.PP
                    // && _nViewTab != (int)TableType.PM
                    && _nViewTab != (int)TableType.ADR
                    )
                {
                    string sql = string.Concat("select res.", _pref.Lang, " as Libelle, * from cfc_getliaison(", _nViewTab, ") left join res on RelationFileDescId = res.resid where isnull(isrelation,0) = 1 ");

                    DataTableReaderTuned dtr = dal.Execute(new RqParam(sql), out error);

                    try
                    {
                        if (!string.IsNullOrEmpty(error))
                            throw new Exception("FillMainFileList : " + error);

                        if (dtr != null)
                        {
                            while (dtr.Read())
                            {
                                int tabDescId = dtr.GetEudoNumeric("RelationFileDescId");

                                if (tabDescId != 0)
                                    _listTab.Add(tabDescId);

                                if (tabDescId == TableType.ADR.GetHashCode())
                                    _listTab.Add((int)TableType.PM);
                            }
                        }
                    }
                    finally
                    {
                        dtr?.Dispose();
                    }
                }

                //ajout adr, pp et pm si nécessaire
                if ((_nViewTab == 200 || _nViewTab == 300 || _nViewTab == 400))
                {
                    _listTab.Add((int)TableType.PP);
                    _listTab.Add((int)TableType.PM);
                    _listTab.Add((int)TableType.ADR);
                }


                if (_bFromInvit && _bTarget && _bDeleteMode)
                    _listTab.Add(_nOrigCalledTab);

                //Ajout de la table principale
                _listTab.Add(_nViewTab);


                //Droits de visu


                using (DataTableReaderTuned dtrRights = eLibDataTools.GetRqViewRight(_pref.User.UserId, _pref.User.UserLevel, _pref.User.UserGroupId, _pref.User.UserLang, _listTab, dal))
                {
                    while (dtrRights.Read())
                    {
                        if (eLibDataTools.GetTabViewRight(dtrRights))
                        {
                            Int32 _descId = dtrRights.GetEudoNumeric("descid");
                            ListItem _itm = new ListItem(dtrRights.GetString("Libelle"), _descId.ToString());
                            if (_descId == _nViewTab)
                                _itm.Selected = true;
                            lst.Items.Add(_itm);
                        }
                    }
                }
            }
            finally
            {
                dal.CloseDatabase();
            }

            //Action Javascript
            lst.Attributes.Add("onchange", "setTabFields();");
            lst.Attributes.Add("MainTab", _nViewTab.ToString());
            return lst;
        }

        /// <summary>
        /// Retourne le contenu de la liste permettant de faire une sélection
        /// </summary>
        /// <param name="tabDescId">descid de la table des champs à afficher</param>
        /// <param name="itemUsed">Champs Sélectionnés</param>
        /// <param name="isCalendarGraphEnabled">Indique si on est en mode graphique de planning</param>
        /// <returns></returns>
        private HtmlGenericControl FillFieldList(int tabDescId, String itemUsed, Boolean isCalendarGraphEnabled, bool removeField01 = false)
        {
            String strId = String.Concat("FieldList_", tabDescId);
            String strOnClicFct = "doInitSearch";
            Boolean bSortList = false;
            String strParentDescIds = tabDescId.ToString();

            IEnumerable<int> list;
            RetrieveFields fields = null;
            // HLA - Si isCalendarGraphEnabled, alors on retire les rubriques logique qui ne sont pas représentées sur ce mode
            if (isCalendarGraphEnabled)
                list = RetrieveFields.GetDefault(_pref)
                    .AddOnlyThisTabs(new int[] { tabDescId })
                    .AddExcludeFormats(new FieldFormat[] { FieldFormat.TYP_BIT, FieldFormat.TYP_ALIASRELATION })
                    .ResultFieldsDid();
            else
            {
                fields = RetrieveFields.GetDefault(_pref)
                    .AddOnlyThisTabs(new int[] { tabDescId })
                    .AddExcludeFormats(new FieldFormat[] { FieldFormat.TYP_ALIASRELATION })

                    ;

                // #58316 : Exclusion du champ 01 pour la table principale si c'est un EVENT/PP/PM
                if (removeField01)
                {
                    fields = fields.AddExcludeDescId(new List<int> { tabDescId + 1 });
                }

                //Pour les tables sms, on vire les champs des tables Email
                if (this._tabEdnType == EdnType.FILE_SMS)
                {
                    List<string> arrMail = eConst.MAIL_HEADER_FIELDS.Split(';').ToList();
                    List<string> arrSMS = eConst.SMS_HEADER_FIELDS.Split(';').ToList();

                    List<int> onlyMail =
                        arrMail.Where(tt => !arrSMS.Contains(tt)).Select(d => tabDescId + int.Parse(d)).ToList();

                    onlyMail.Add(tabDescId + (int)MailField.DESCID_MAIL_BCC);
                    onlyMail.Add(tabDescId + (int)MailField.DESCID_MAIL_BROWSER);
                    onlyMail.Add(tabDescId + (int)MailField.DESCID_MAIL_BROWSERTYPE);
                    onlyMail.Add(tabDescId + (int)MailField.DESCID_MAIL_CC);
                    onlyMail.Add(tabDescId + (int)MailField.DESCID_MAIL_CLICKED_LINKS);
                    onlyMail.Add(tabDescId + (int)MailField.DESCID_MAIL_EDB_CATEGORY);
                    onlyMail.Add(tabDescId + (int)MailField.DESCID_MAIL_EDB_TYPE);
                    //onlyMail.Add(tabDescId + (int)MailField.DESCID_MAIL_HTML);
                    onlyMail.Add(tabDescId + (int)MailField.DESCID_MAIL_ISHTML);
                    onlyMail.Add(tabDescId + (int)MailField.DESCID_MAIL_PREHEADER);
                    onlyMail.Add(tabDescId + (int)MailField.DESCID_MAIL_READ);
                    onlyMail.Add(tabDescId + (int)MailField.DESCID_MAIL_REASON);
                    onlyMail.Add(tabDescId + (int)MailField.DESCID_MAIL_REPLY_TO);
                    onlyMail.Add(tabDescId + (int)MailField.DESCID_MAIL_SMTP);
                    onlyMail.Add(tabDescId + (int)MailField.DESCID_MAIL_USERAGENT);
                    onlyMail.Add(tabDescId + (int)MailField.DESCID_MAIL_FIRST_READING_DATE);
                    onlyMail.Add(tabDescId + (int)MailField.DESCID_MAIL_TRACK);

                    

                    onlyMail.Add(tabDescId + (int)MailField.DESCID_MAIL_STATUS);


                    fields = fields.AddExcludeDescId(onlyMail);
                }

                fields.AddExcludeNotMappedExtendedTarget(true);

                list = fields.ResultFieldsDid();
                //SHA : bug #70 966
                if (tabDescId == (int)TableType.USER)
                {
                    var tmp = list.ToList();
                    tmp.Remove((int)UserField.GroupId);
                    tmp.Add((int)UserField.GROUPNAME);
                    list = tmp;
                }
            }

            return FillAnyFieldList(false, eLibTools.Join(";", list), itemUsed, strParentDescIds, strId, strOnClicFct, bSortList);
        }


        /// <summary>
        /// Retourne le contenu de la liste permettant de faire une sélection des champs de type user
        /// </summary>
        /// <param name="tabDescId">descid de la table des champs à afficher</param>
        /// <param name="itemUsed">Champs Sélectionnés</param>    
        /// <returns></returns>
        private HtmlGenericControl FillUserFieldList(Int32 tabDescId, String itemUsed)
        {
            String strId = String.Concat("FieldList_", tabDescId);
            String strOnClicFct = "doInitSearch";
            Boolean bSortList = false;
            String strParentDescIds = tabDescId.ToString();

            IEnumerable<int> list = RetrieveFields.GetDefault(_pref)
                .AddOnlyThisTabs(new int[] { tabDescId })
                .AddOnlyThisFormats(new FieldFormat[] { FieldFormat.TYP_USER })
                .ResultFieldsDid();

            return FillAnyFieldList(false, eLibTools.Join(";", list), itemUsed, strParentDescIds, strId, strOnClicFct, bSortList);
        }

        /// <summary>
        /// Retourne le rendu des champs sélectionnés
        /// </summary>        
        /// <param name="viewTab">descid table principale</param>
        /// <param name="listCol">Champs à affichés (ici les champs sélectionnés)</param>
        /// <param name="removeField01">Champ 01 présent par défaut, non retirable</param>
        /// <returns></returns>
        private HtmlGenericControl FillSelectedFieldList(int viewTab, String listCol, bool removeField01 = false)
        {
            string strId = "ItemsUsed";
            string strOnClicFct = "doInitSearch";
            bool bSortList = true;
            string strParentDescIds = eLibTools.Join(",", _listTab);
            List<int> lidescids = new List<int>();

            if (!string.IsNullOrEmpty(listCol))
            {
                foreach (string s in listCol.Split(';'))
                {
                    try
                    {
                        lidescids.Add(int.Parse(s));
                    }
                    catch
                    {
                        continue;
                    }
                }
            }

            //#58116 : gestion des sélections vide -> AddOnlyThisDescIds ne filtre pas si vide
            string sLstCol = "";
            if (lidescids.Count > 0)
            {

                RetrieveFields fields = RetrieveFields.GetDefault(_pref)
                .AddExcludeFormats(new FieldFormat[] { FieldFormat.TYP_ALIASRELATION })
                .AddOnlyThisDescIds(lidescids);

                // #58316 : Exclusion du champ 01 pour la table principale si c'est un EVENT/PP/PM
                if (removeField01)
                {
                    fields = fields.AddExcludeDescId(new List<int> { viewTab + 1 });
                }

                IEnumerable<int> list = fields.ResultFieldsDid().OrderBy(d => lidescids.IndexOf(d));

                //SHA : bug #70 966
                if (viewTab == (int)TableType.USER)
                {
                    var tmp = list.ToList();
                    tmp.Remove((int)UserField.GroupId);
                    tmp.Add((int)UserField.GROUPNAME);
                    list = tmp;
                }

                sLstCol = eLibTools.Join(";", list);
            }

            return FillAnyFieldList(true, sLstCol, String.Empty, strParentDescIds, strId, strOnClicFct, bSortList);
        }



        /// <summary>
        /// Retourne le contenu de la liste permettant de faire une sélection
        /// </summary>
        /// <param name="listCol">liste des descids à afficher</param>
        /// <param name="itemUsed">Champs Sélectionnés</param>
        /// <param name="strParentDescIds">descid de la table des champs à afficher</param>
        /// <returns></returns>
        private HtmlGenericControl FillAnyFieldList(bool selectedFieldList, string listCol, string itemUsed, string strParentDescIds, string strId, string strOnClicFct, bool bSortList)
        {
            // Création du contrôle Liste (div)
            HtmlGenericControl lst = new HtmlGenericControl("div");
            lst.Attributes.Add("field_list", "");
            lst.Style.Add("width", "100%");
            lst.Style.Add("height", "100%");
            lst.ID = strId;
            lst.Attributes.Add("onclick", String.Concat(strOnClicFct, "(this, event);"));
            if (!selectedFieldList)
            {
                lst.Attributes.Add("SelectedIndex", "0"); // uniquement utilisé pour la liste des éléments disponibles
            }

            // Libellés des fichiers parents pour ajout en préfixe de la valeur
            bool bDisplayParentTablePrefix = selectedFieldList; // à mettre à true lorsqu'on aura statué sur la légitimité de la demande #23 608 :)
            string strLibelleParent = String.Empty;
            eRes _resLib = null;
            bool _bFound = false;

            _resLib = new eRes(_pref, strParentDescIds);
            if (!selectedFieldList)
            {
                int nParentDescId = 0;
                Int32.TryParse(strParentDescIds, out nParentDescId);
                strLibelleParent = _resLib.GetRes(nParentDescId, out _bFound);
            }

            // Filtrage de la liste en fonction des droits de visu et récupération des libellés
            Dictionary<Int32, String> fldList = null;
            if (listCol == null)
                fldList = new Dictionary<int, string>();
            else
                fldList = eLibTools.GetAllowedFieldsFromDescIds(_pref, _pref.User, listCol, bSortList);


            // Construction de l'objet HTML
            String optCss = "cell";
            HtmlGenericControl itm = null;
            String sShortLabel = String.Empty;
            String sLongLabel = String.Empty;

            foreach (KeyValuePair<Int32, String> descIdLabel in fldList)
            {
                Int32 _descId = descIdLabel.Key;
                if (!selectedFieldList && String.Concat(";", itemUsed, ";").Contains(String.Concat(";" + _descId + ";")))
                    continue;

                if (_listColNotDisplayed.Count > 0 && _listColNotDisplayed.Contains(_descId))
                    continue;

                Int32 _parentDescId = _descId;
                _parentDescId = (_descId / 100) * 100;
                if (_resLib != null)
                    strLibelleParent = _resLib.GetRes(_parentDescId, out _bFound);

                itm = new HtmlGenericControl("div");
                if (optCss.Equals("cell"))
                    optCss = "cell2";
                else
                    optCss = "cell";

                // #22845 
                // Si le champ est 01 d'une table principale ou qu'on vient de "loadtab", on met la rubrique en gras
                if (_descId % 100 == 1
                    && (_tabEdnType == EdnType.FILE_MAIN || _action == "loadtab" || (_action == "initbkm" && _bFileRelation)))
                {
                    itm.Attributes.Add("data-mainfield", "1");
                }

                itm.Attributes.Add("class", optCss);
                itm.Attributes.Add("oldCss", optCss);

                itm.Attributes.Add("value", _descId.ToString());

                sShortLabel = descIdLabel.Value;
                sLongLabel = String.Concat(strLibelleParent, ".", descIdLabel.Value);

                if (bDisplayParentTablePrefix)
                {
                    itm.InnerHtml = HttpUtility.HtmlEncode(sLongLabel);
                }
                else
                {
                    itm.InnerHtml = HttpUtility.HtmlEncode(sShortLabel);
                }

                itm.Attributes.Add("shlb", sShortLabel);
                itm.Attributes.Add("lglb", sLongLabel);

                itm.Attributes.Add("class", optCss);
                itm.Attributes.Add("oldCss", optCss);

                itm.Attributes.Add("edntab", _parentDescId.ToString());
                itm.Attributes.Add("ednParentTab", _parentDescId.ToString());

                itm.Attributes.Add("onclick", "setElementSelected(this);");
                itm.Attributes.Add("onmousedown", "strtDrag(event);");

                itm.Attributes.Add("DescId", _descId.ToString());
                itm.ID = "descId_" + _descId;

                lst.Controls.Add(itm);
            }

            // Création du guide de déplacement
            itm = new HtmlGenericControl("div");
            if (selectedFieldList)
                itm.ID = "SelectedListElmntGuidFS";
            else
                itm.ID = "AllListElmntGuidFS";
            itm.Attributes.Add("class", "dragGuideTab");
            itm.Attributes.Add("syst", "");
            itm.Style.Add("display", "none");
            lst.Controls.Add(itm);

            //Actions Javascript
            if (!selectedFieldList)
            {
                lst.Attributes.Add("ondblclick", "AddItem();");
                lst.Attributes.Add("ednType", "lstEdnTable");
                lst.Attributes.Add("ednDescId", strParentDescIds);
            }
            else
            {
                lst.Attributes.Add("ondblclick", "DelItem();");
            }
            return lst;
        }

        /// <summary>
        /// Faut-il supprimer la rubrique 01 ?
        /// </summary>
        /// <param name="tab">The tab.</param>
        /// <param name="colList">The col list.</param>
        /// <returns></returns>
        private bool RemoveField01(TableLite tab, string colList)
        {

            // Suppression des rubriques 01
            if (!String.IsNullOrEmpty(colList))
            {
                List<TableType> tabTypesWithoutField01Choice = new List<TableType> { TableType.EVENT, TableType.PM, TableType.PP };
                if (tabTypesWithoutField01Choice.Contains(tab.TabType))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Charge la liste des vues disponibles pour l'onglet en cours
        /// </summary>
        /// <param name="userSelectId"> Selection active</param>
        /// <returns></returns>
        private DropDownList FillSelectionList(Int32 userSelectId)
        {
            DropDownList lst = new DropDownList();
            lst.ID = string.Concat("AllSelections");
            lst.Style.Add("width", "50%");
            string _sql = String.Concat("select [SelectId], [Label], [ListCol] from [Selections] where [UserId] = ", _pref.User.UserId, " and [Tab] = ", _nViewTab, " order by [label]");

            eudoDAL _dal = eLibTools.GetEudoDAL(_pref);
            _dal.OpenDatabase();
            String _err = string.Empty;
            DataTableReaderTuned _dtr = _dal.Execute(new RqParam(_sql), out _err);
            try
            {
                _dal.CloseDatabase();

                if (!String.IsNullOrEmpty(_err))
                    throw new Exception("FillSelectionList : " + _err);

                if (_dtr != null)
                {
                    while (_dtr.Read())
                    {
                        Int32 _selectId = 0;
                        _selectId = _dtr.GetEudoNumeric(0);


                        String _lbl = _dtr.GetString(1);
                        ListItem _itm = new ListItem(_lbl, _selectId.ToString());
                        _itm.Selected = (_selectId == userSelectId);
                        _itm.Attributes.Add("listcol", _dtr.GetString(2));
                        lst.Items.Add(_itm);
                    }
                }
            }
            finally
            {
                if (_dtr != null)
                    _dtr.Dispose();
            }

            ListItem _itmNew = new ListItem(String.Concat("<", eResApp.GetRes(_pref, 5054), ">"), "0");

            _itmNew.Attributes.Add("listcol", String.Empty);
            lst.Items.Add(_itmNew);
            lst.Attributes.Add("onchange", "setSelection('" + _nViewTab + "');");

            return lst;
        }


    }
}