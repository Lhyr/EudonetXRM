using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using EudoQuery;
using Com.Eudonet.Common.Enumerations;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// JBE : Classe de rendu modes fiche planning
    /// </summary>
    public class ePlanningFileRenderer : eMainFileRenderer
    {
        #region PROPRIETES

        //List<String> _sFieldsDescId = new List<string>();
        private Int32 _nHeight = 0;
        private Int32 _nWidth = 0;
        private string _date;
        private string _endDate;

        private bool _bForceType = false;
        private Int32 _npType = 0;
        private Dictionary<Int32, String> _dicValues;

        // existe sur la classe parente
        //private ExtendedDictionary<String, Object> _dicParams;


        const int MAX_HEAD_COLSPAN = 7;

        private int _concernedUser = 0;
        private CalendarViewMode _viewMode;
        private string sOpenSeries = "0";
        bool _calendarEnabled = false;
        Boolean _bTooltipEnabled = false;

        #endregion

        /// <summary>
        /// Mode du planning
        /// </summary>
        public CalendarViewMode ViewMode
        {
            get { return _viewMode; }
            set { _viewMode = value; }
        }

        #region CONSTRUCTEUR

        /// <summary>
        /// Rendu de la fiche planning
        /// </summary>
        /// <param name="pref">Préférence de l'utilisateur en cours</param>
        /// <param name="nTab"></param>
        /// <param name="nFileId"></param>
        /// <param name="nHeight">Hauteur de la pop up</param>
        /// <param name="nWidth">Largeur de la pop up</param>
        /// <param name="date">date de début</param>
        /// <param name="endDate">date de fin</param>
        /// <param name="isPostback"></param>
        /// <param name="concernedUser">Id de l'utilisateur sélectionné lors de la création de la fiche Planning (ex : lors d'un double clic sur la cellule d'un emplacement planning graphique en mode Jour = Id de l'utilisateur dont la colonne a été cliquée)</param>
        public ePlanningFileRenderer(ePref pref, Int32 nTab, Int32 nFileId, Int32 nWidth, Int32 nHeight, string date, string endDate, bool isPostback, int concernedUser)
        {
            Pref = pref;
            _tab = nTab;
            _nFileId = nFileId;
            _nHeight = nHeight - 130;   //  130 = les boutons d'options du haut + boutons de validation du bas
            _nWidth = nWidth;    //A réduire si necessaire
            _date = date;
            _isPostback = isPostback;
            _concernedUser = concernedUser;
            _endDate = endDate;
            _rType = RENDERERTYPE.PlanningFile;
            _bPopupDisplay = true;
        }

        #endregion

        /// <summary>
        /// Date/heure debut - date/heure fin - toute la journée - périodicité - couleur - alerte - Condifentielle
        /// </summary>
        private class PlanningHeaderRecordFields
        {
            internal eFieldRecord FldDone { get; set; }
            internal eFieldRecord FldBeginTime { get; set; }
            internal eFieldRecord FldEndTime { get; set; }
            internal eFieldRecord FldColor { get; set; }
            internal eFieldRecord FldConfid { get; set; }
            internal eFieldRecord FldMainUser { get; set; }
            internal eFieldRecord FldOtherUsers { get; set; }
            internal eFieldRecord FldType { get; set; }
            internal eFieldRecord FldSchedule { get; set; }
            internal eFieldRecord FldNotes { get; set; }
            internal eFieldRecord FldUserVisu { get; set; }
            internal eFieldRecord FldAlert { get; set; }
            internal eFieldRecord FldAlertDate { get; set; }
            internal eFieldRecord FldAlertSound { get; set; }
            internal eFieldRecord FldAlertTime { get; set; }
            internal eFieldRecord FldHisto { get; set; }

            internal void SetHeaderRecordFields(eFile myFile, List<eFieldRecord> headerFields)
            {
                Int32 shortDescid;

                foreach (eFieldRecord fld in headerFields)
                {
                    shortDescid = fld.FldInfo.Descid % 100;

                    if (fld.FldInfo.Descid == Math.Abs(myFile.HistoInfo.Descid))
                        FldHisto = fld;

                    else if (fld.FldInfo.Descid == myFile.DateDescId)
                        FldBeginTime = fld;
                    else if (shortDescid == PlanningField.DESCID_TPL_END_TIME.GetHashCode())
                        FldEndTime = fld;

                    else if (shortDescid == PlanningField.DESCID_ALERT.GetHashCode())
                        FldAlert = fld;
                    else if (shortDescid == PlanningField.DESCID_ALERT_DATE.GetHashCode())
                        FldAlertDate = fld;
                    else if (shortDescid == PlanningField.DESCID_ALERT_SOUND.GetHashCode())
                        FldAlertSound = fld;
                    else if (shortDescid == PlanningField.DESCID_ALERT_TIME.GetHashCode())
                        FldAlertTime = fld;

                    else if (shortDescid == AllField.CONFIDENTIAL.GetHashCode())
                        FldConfid = fld;
                    else if (shortDescid == AllField.USER_VISIBLE.GetHashCode())
                        FldUserVisu = fld;

                    else if (shortDescid == PlanningField.DESCID_CALENDAR_COLOR.GetHashCode())
                        FldColor = fld;
                    else if (fld.FldInfo.Descid == myFile.ViewMainTable.GetOwnerDescId())
                        FldMainUser = fld;
                    else if (fld.FldInfo.Descid == myFile.ViewMainTable.GetMultiOwnerDescId(false))
                        FldOtherUsers = fld;
                    else if (shortDescid == PlanningField.DESCID_CALENDAR_ITEM.GetHashCode())
                        FldType = fld;
                    else if (shortDescid == PlanningField.DESCID_SCHEDULE_ID.GetHashCode())
                        FldSchedule = fld;

                    else if (shortDescid == AllField.MEMO_NOTES.GetHashCode())
                        FldNotes = fld;
                }
            }
        }

        private PlanningHeaderRecordFields headerRecordFields = new PlanningHeaderRecordFields();

        /// <summary>
        /// Création et initialisation de l'objet eFile
        /// </summary>
        /// <returns></returns>
        protected override Boolean Init()
        {
            //TODO REFACTO --> heriter de eEditFileRenderer
            try
            {
                _myFile = eFileMain.CreateMainFile(Pref, _tab, _nFileId, 0, _dicParams);

                _dicParams.TryGetValueConvert("openseries", out sOpenSeries, String.Empty);
                _dicParams.TryGetValueConvert("globalaffect", out GlobalAffect, false);
                _dicParams.TryGetValueConvert("readonly", out _bReadOnly, false);
                if (_dicParams.ContainsKey("ptype"))
                {
                    _dicParams.TryGetValueConvert("ptype", out _npType);
                    if (_npType >= 0 && _npType <= 2)
                        _bForceType = true;
                }

                _bTooltipEnabled = Pref.GetConfig(eLibConst.PREF_CONFIG.TOOLTIPTEXTENABLED) == "1";

                if (_myFile.ErrorMsg.Length > 0)
                {
                    _eException = _myFile.InnerException;
                    _sErrorMsg = String.Concat("ePlanningFileRenderer.Init ", Environment.NewLine, _myFile.ErrorMsg);
                    if (_myFile.InnerException != null && _myFile.InnerException.GetType() == typeof(EudoFileNotFoundException))
                    {
                        _nErrorNumber = QueryErrorType.ERROR_NUM_FILE_NOT_FOUND;
                    }
                    else
                    {
                        _nErrorNumber = QueryErrorType.ERROR_NUM_DEFAULT;
                    }

                    return false;
                }

                //Désactivation de la fiche si ouverture d'une série
                if (sOpenSeries.Equals("1") || _bReadOnly)
                {
                    _myFile.Record.RightIsUpdatable = false;

                    foreach (eFieldRecord fld in _myFile.Record.GetFields)
                    {
                        fld.RightIsUpdatable = false;
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                _sErrorMsg = String.Concat("ePlanningFileRenderer.Init ", Environment.NewLine, e.Message);
                _nErrorNumber = QueryErrorType.ERROR_NUM_DEFAULT;
                _eException = e;
                return false;
            }
        }

        /// <summary>
        /// Rubrique en entête
        /// </summary>
        /// <returns></returns>
        private HashSet<Int32> GetPlanningHeaderFields()
        {
            HashSet<Int32> headerFields = new HashSet<Int32>();
            // Fait : 1
            //headerFields.Add(1);
            // Type : 83
            headerFields.Add(PlanningField.DESCID_CALENDAR_ITEM.GetHashCode());
            // Date debut  : ??
            headerFields.Add(_myFile.DateDescId % 100);
            // Date fin  : 89
            headerFields.Add(PlanningField.DESCID_TPL_END_TIME.GetHashCode());
            // A faire par : 99
            headerFields.Add(AllField.OWNER_USER.GetHashCode());
            // Particiants : 92
            headerFields.Add(AllField.TPL_MULTI_OWNER.GetHashCode());
            // Confidentiel : 84    
            headerFields.Add(AllField.CONFIDENTIAL.GetHashCode());
            // Couleur : 80
            headerFields.Add(PlanningField.DESCID_CALENDAR_COLOR.GetHashCode());

            headerFields.Add(AllField.MEMO_NOTES.GetHashCode());
            headerFields.Add(AllField.USER_VISIBLE.GetHashCode());
            headerFields.Add(PlanningField.DESCID_SCHEDULE_ID.GetHashCode());

            headerFields.Add(PlanningField.DESCID_ALERT.GetHashCode());
            headerFields.Add(PlanningField.DESCID_ALERT_TIME.GetHashCode());
            headerFields.Add(PlanningField.DESCID_ALERT_SOUND.GetHashCode());
            headerFields.Add(PlanningField.DESCID_ALERT_DATE.GetHashCode());

            return headerFields;
        }

        /// <summary>
        /// Construction des objets HTML
        /// </summary>
        /// <returns></returns>
        protected override Boolean Build()
        {
            // Div de champ caché
            _divHidden = new HtmlGenericControl("div");
            _divHidden.Style.Add("visibility", "hidden");
            _divHidden.Style.Add("display", "none");
            _divHidden.ID = String.Concat("hv_", _myFile.ViewMainTable.DescId);
            PgContainer.Controls.Add(_divHidden);

            Int32 nbColByLine = _myFile.ViewMainTable.ColByLine;

            //Creation d'une input contenant les titres séparateurs de page  fermer lors de l'ouverture de la page
            _closedSep = new HtmlInputHidden();
            _closedSep.ID = String.Concat("ClosedSep_", _tab);
            _divHidden.Controls.Add(_closedSep);

            #region fiche PLANNING

            HashSet<Int32> lHeaderFields = GetPlanningHeaderFields();

            List<eFieldRecord> sortedFields = new List<eFieldRecord>();
            List<eFieldRecord> headerFields = new List<eFieldRecord>();
            foreach (eFieldRecord recordFld in _myFile.GetFileFields)
            {
                //Les champs en disporder 0 sont des champs systeme à ne pas afficher

                if (recordFld.FldInfo.Descid - _myFile.ViewMainTable.DescId != 1)
                {
                    if (recordFld.FldInfo.Table.DescId == _myFile.ViewMainTable.DescId
                    && lHeaderFields.Contains(recordFld.FldInfo.Descid - _myFile.ViewMainTable.DescId))
                        headerFields.Add(recordFld);

                    else if (recordFld.FldInfo.PosDisporder != 0)
                        sortedFields.Add(recordFld);
                }
                else
                {
                    // #43516 CRU :
                    // Si on est sur le TPL01 et que la rubrique historique est paramétrée sur le champ, on le place dans les Headers
                    // Sinon le champ prendra la position paramétrée en Admin
                    if (_myFile.HistoInfo.Descid - _myFile.ViewMainTable.DescId == 1)
                        headerFields.Add(recordFld);
                    else
                        sortedFields.Add(recordFld);
                }

            }

            //Tri les listes par disporder
            headerFields.Sort(eFieldRecord.CompareByDisporder);
            sortedFields.Sort(eFieldRecord.CompareByDisporder);

            HtmlGenericControl fileTabMain = new HtmlGenericControl("div");
            fileTabMain.ID = "ftm_" + _tab.ToString();
            _pgContainer.Controls.Add(fileTabMain);
            fileTabMain.Controls.Add(GetPlanningFileMain(headerFields, sortedFields, nbColByLine));

            #endregion

            //Pour le traitement de masse
            if (GlobalAffect)
            {
                HtmlInputHidden hideGlobalAffect = new HtmlInputHidden();
                _divHidden.Controls.Add(hideGlobalAffect);
                hideGlobalAffect.ID = String.Concat("ga_", _tab);
                hideGlobalAffect.Attributes.Add("value", "1");
            }
            _divHidden.Controls.Add(GetPropertiesTable());

            String sCssIconPosition = " 0 0 ";
            String sCSSStdIcon = String.Concat("background:url(themes/",
                Pref.ThemePaths.GetImageWebPath("/images/iFileIcon/" + _myFile.ViewMainTable.GetIcon.Replace(".jpg", ".png"))
                , ") ", sCssIconPosition, " no-repeat !important  ");
            HtmlInputHidden inptDefIconCss = new HtmlInputHidden();
            inptDefIconCss.ID = "ICON_DEF_" + _myFile.ViewMainTable.DescId;
            inptDefIconCss.Attributes.Add("etype", "css");
            inptDefIconCss.Attributes.Add("ecssname", String.Concat("iconDef_", _myFile.ViewMainTable.DescId));
            inptDefIconCss.Attributes.Add("ecssclass", sCSSStdIcon);
            _divHidden.Controls.Add(inptDefIconCss);

            //histodescid pour la fonction historiser et créer
            HtmlInputHidden iptHistoDescid = new HtmlInputHidden();
            iptHistoDescid.ID = String.Concat("hdid_" + _myFile.ViewMainTable.DescId);
            iptHistoDescid.Value = _myFile.HistoInfo.Descid.ToString();
            _divHidden.Controls.Add(iptHistoDescid);

            return true;
        }



        #region FICHE PLANNING

        /// <summary>
        /// fiche planning
        /// </summary>
        /// <returns></returns>
        private Control GetPlanningFileMain(List<eFieldRecord> headerFields, List<eFieldRecord> sortedFields, Int32 nbColByLine)
        {
            Panel pnlPlanningMain = new Panel();
            pnlPlanningMain.ID = "md_pl-base";
            pnlPlanningMain.CssClass = "md_pl-base";
            pnlPlanningMain.Style.Add(HtmlTextWriterStyle.Height, string.Concat(_nHeight, "px"));
            pnlPlanningMain.Style.Add(HtmlTextWriterStyle.OverflowY, "auto");
            pnlPlanningMain.Style.Add(HtmlTextWriterStyle.OverflowX, "hidden");
            _pgContainer.Controls.Add(pnlPlanningMain);

            //Partie haute
            headerRecordFields.SetHeaderRecordFields(_myFile, headerFields);
            Panel plHead = GetPlanningHeader(headerFields);
            if (plHead != null)
                pnlPlanningMain.Controls.Add(plHead);

            //Création de la partie basse
            System.Web.UI.WebControls.Table fileTabMain = CreateHtmlTable(sortedFields, nbColByLine, String.Concat("SEP_", _myFile.ViewMainTable.DescId, "_0"));
            fileTabMain.Rows.AddAt(1, GetSystemSeparator(nbColByLine, eResApp.GetRes(Pref, 6289).Replace("<FILE>", _myFile.ViewMainTable.Libelle), String.Concat("SEP_", _myFile.ViewMainTable.DescId, "_0")));

            pnlPlanningMain.Controls.Add(fileTabMain);

            return pnlPlanningMain;
        }

        /// <summary>
        /// Entête de la fiche planning
        /// </summary>
        /// <returns></returns>
        private Control GetPlanningFileTop()
        {
            System.Web.UI.WebControls.Table tabTop = new System.Web.UI.WebControls.Table();
            tabTop.Style.Add("width", "50%");
            System.Web.UI.WebControls.TableRow trTop = new TableRow();
            tabTop.Controls.Add(trTop);

            _calendarEnabled = Pref.GetPref(_myFile.ViewMainTable.DescId, ePrefConst.PREF_PREF.CALENDARENABLED).Equals("1");

            tabTop.Attributes.Add("class", "md_pl_head table_values");

            TableCell tc1 = new TableCell();
            TableCell tc2 = new TableCell();

            if (_calendarEnabled)
            {
                //    842 -   Tâche
                //    843 -   Agenda
                RadioButton lstType1 = new RadioButton();
                lstType1.CssClass = "table_labels";
                lstType1.EnableViewState = false;
                lstType1.ViewStateMode = ViewStateMode.Disabled;
                lstType1.AutoPostBack = false;
                lstType1.ID = "1";
                lstType1.GroupName = "ItemType";
                lstType1.Text = eResApp.GetRes(Pref, 843);
                //On set les style
                eTools.SetHTMLControlStyle(headerRecordFields.FldType, lstType1);


                RadioButton lstType2 = new RadioButton();
                lstType2.ID = "0";
                lstType2.EnableViewState = false;
                lstType1.AutoPostBack = false;
                lstType2.ViewStateMode = ViewStateMode.Disabled;
                lstType2.GroupName = "ItemType";
                lstType2.Text = eResApp.GetRes(Pref, 842);
                lstType2.CssClass = "table_labels";
                //On set les style
                eTools.SetHTMLControlStyle(headerRecordFields.FldType, lstType2);

                tc1.ID = eTools.GetFieldValueCellName(_myFile.Record, headerRecordFields.FldType);
                tc1.Attributes.Add("did", headerRecordFields.FldType.FldInfo.Descid.ToString());

                tc1.Controls.Add(lstType1);
                tc2.Controls.Add(lstType2);
                trTop.EnableViewState = false;
                trTop.ViewStateMode = ViewStateMode.Disabled;

                trTop.Controls.Add(tc1);
                trTop.Controls.Add(tc2);

                if (_nFileId == 0 && !_isPostback)
                    headerRecordFields.FldType.Value = headerRecordFields.FldType.FldInfo.DefaultValue;

                if (headerRecordFields.FldType.Value.Length == 0)
                    headerRecordFields.FldType.Value = "0";

                _viewMode = (CalendarViewMode)eLibTools.GetNum(Pref.GetPref(_myFile.ViewMainTable.DescId, ePrefConst.PREF_PREF.VIEWMODE));

                if (_nFileId == 0 && (this._viewMode == CalendarViewMode.VIEW_CAL_WORK_WEEK || this._viewMode == CalendarViewMode.VIEW_CAL_DAY_PER_USER) && !_isPostback)
                {
                    if (_bForceType && _npType != 0)
                    {
                        headerRecordFields.FldType.Value = PlanningType.CALENDAR_ITEM_APPOINTMENT.GetHashCode().ToString();
                    }
                    else
                        headerRecordFields.FldType.Value = PlanningType.CALENDAR_ITEM_APPOINTMENT.GetHashCode().ToString();
                }
                else if (_nFileId == 0 && !_isPostback && _bForceType)
                {
                    if (_npType == 0)
                        headerRecordFields.FldType.Value = PlanningType.CALENDAR_ITEM_TASK.GetHashCode().ToString();
                    else if (_npType == 1)
                        headerRecordFields.FldType.Value = PlanningType.CALENDAR_ITEM_APPOINTMENT.GetHashCode().ToString();
                    else if (_npType == 2)
                        headerRecordFields.FldType.Value = PlanningType.CALENDAR_ITEM_EVENT.GetHashCode().ToString();
                    else
                        headerRecordFields.FldType.Value = PlanningType.CALENDAR_ITEM_APPOINTMENT.GetHashCode().ToString();
                }

                lstType2.Checked = (PlanningType)eLibTools.GetNum(headerRecordFields.FldType.Value) == PlanningType.CALENDAR_ITEM_TASK;

                lstType1.Checked = !lstType2.Checked;


                TextBox txtType = new TextBox();
                txtType.ID = eTools.GetFieldValueCellId(_myFile.Record, headerRecordFields.FldType);
                txtType.Attributes.Add("ename", eTools.GetFieldValueCellName(_myFile.Record, headerRecordFields.FldType));
                txtType.Attributes.Add("efld", "1");
                txtType.Text = headerRecordFields.FldType.Value;

                txtType.Style.Add(HtmlTextWriterStyle.Display, "none");
                tc1.Controls.Add(txtType);

                if (headerRecordFields.FldType.RightIsUpdatable)
                {
                    lstType1.Attributes.Add("onclick", string.Concat("setItemType(1,'", txtType.ID, "',", _myFile.ViewMainTable.DescId, ");"));
                    lstType2.Attributes.Add("onclick", string.Concat("setItemType(0,'", txtType.ID, "',", _myFile.ViewMainTable.DescId, ");"));
                }
                else
                {
                    lstType1.Enabled = false;
                    lstType2.Enabled = false;

                    txtType.Attributes.Add("readonly", "readonly");
                    txtType.Attributes.Add("ero", "1");
                }

                if (headerRecordFields.FldType.RightIsUpdatable || _myFile.Record.MainFileid == 0 && headerRecordFields.FldType.Value.Length > 0)
                    FieldsDescId.Add(headerRecordFields.FldType.FldInfo.Descid.ToString());

                if (headerRecordFields.FldType.RightIsUpdatable)
                    this.AllowedFieldsDescId.AddContains(headerRecordFields.FldType.FldInfo.Descid.ToString());
            }

            //PJ - TODO - Recharger les PJ existantes
            TextBox txtPj = new TextBox();
            txtPj.Text = ""; //TODO - IDs des PJ attachées à la fiche
            txtPj.Style.Add("display", "none");
            tc1.Controls.Add(txtPj);

            //Fait/terminé
            if (headerRecordFields.FldHisto != null && headerRecordFields.FldHisto.RightIsVisible)
            {
                System.Web.UI.WebControls.Table tabHisto = new System.Web.UI.WebControls.Table();
                tabHisto.Rows.Add(GetPlanningFieldDraw(headerRecordFields.FldHisto));
                TableCell tc3 = new TableCell();
                trTop.Controls.Add(tc3);
                tc3.Controls.Add(tabHisto);

                if (headerRecordFields.FldHisto.RightIsUpdatable || _myFile.Record.MainFileid == 0 && headerRecordFields.FldHisto.Value.Length > 0)
                    FieldsDescId.AddContains(headerRecordFields.FldHisto.FldInfo.Descid.ToString());


                if (headerRecordFields.FldHisto.RightIsUpdatable)
                    AllowedFieldsDescId.AddContains(headerRecordFields.FldHisto.FldInfo.Descid.ToString());
            }

            //CalendarInterval
            TextBox txtOpenSeries = new TextBox();
            txtOpenSeries.ID = "OpenSeries";
            txtOpenSeries.Text = sOpenSeries;
            txtOpenSeries.Style.Add("display", "none");
            tc1.Controls.Add(txtOpenSeries);

            //CalendarInterval
            int nMinInter = eLibTools.GetNum(Pref.GetPref(_myFile.ViewMainTable.DescId, ePrefConst.PREF_PREF.CALENDARMINUTESINTERVAL));
            if (nMinInter == 0)
                nMinInter = eLibConst.CALENDAR_MINUTES_INTERVAL;

            TextBox txtInterval = new TextBox();
            txtInterval.ID = "CalendarInterval";
            txtInterval.Text = nMinInter.ToString();
            txtInterval.Style.Add("display", "none");
            tc1.Controls.Add(txtInterval);

            return tabTop;
        }

        /// <summary>
        /// Retourne l'entête du planning
        /// </summary>
        /// <returns></returns>
        private Panel GetPlanningHeader(List<eFieldRecord> headerFields)
        {
            Panel pnlMain = new Panel();
            Panel pnlLeft = new Panel();

            pnlMain.Controls.Add(GetPlanningFileTop());

            pnlMain.Controls.Add(pnlLeft);
            pnlLeft.CssClass = "tabLeft";

            //Dates

            //Date Fin
            int nDefDuration = eLibTools.GetNum(Pref.GetPref(_myFile.ViewMainTable.DescId, ePrefConst.PREF_PREF.CALENDARITEMDEFAULTDURATION));
            if (nDefDuration == 0)
                nDefDuration = eLibConst.CALENDAR_ITEM_DEFAULT_DURATION;

            int nMinInter = eLibTools.GetNum(Pref.GetPref(_myFile.ViewMainTable.DescId, ePrefConst.PREF_PREF.CALENDARMINUTESINTERVAL));
            if (nMinInter == 0)
                nMinInter = eLibConst.CALENDAR_MINUTES_INTERVAL;

            TextBox txtDuration = new TextBox();
            txtDuration.ID = "DefaultDuration";
            txtDuration.Text = nDefDuration.ToString();
            txtDuration.Style.Add("display", "none");
            pnlLeft.Controls.Add(txtDuration);

            if (_nFileId == 0 && !_isPostback)
            {
                //Valeurs par défaut
                string sDefaultBeginDate = headerRecordFields.FldBeginTime.FldInfo.DefaultValue;

                DateTime defBeginDate = DateTime.Now;
                DateTime defEndDate = DateTime.Now;

                if (!String.IsNullOrEmpty(sDefaultBeginDate) && !_isPostback)
                {
                    sDefaultBeginDate = sDefaultBeginDate.Replace(" ", "");
                    bool bNoYear = sDefaultBeginDate.Contains("[NOYEAR]");
                    sDefaultBeginDate = sDefaultBeginDate.Replace("[NOYEAR]", string.Empty);

                    if (sDefaultBeginDate == "<DATE>")
                    {
                        TimeSpan tsWorkHourBegin = new TimeSpan(9, 0, 0);

                        // Récupération du paramètre "date de début de journée de travail" pour préremplir le champ "Heure" avec cette heure lorsque le champ Date de début est paramétré
                        // en tant que <Date du jour> (comportement ISO v7)
                        // Si le paramètre a été stocké dans le corps de la page (par eMainList.DoPlanning()) et passé en JavaScript à la page, utilisation de cette variable.
                        // Sinon, instanciation d'ePlanning qui nécessite une connexion supplémentaire à la base via EudoQuery
                        string sWorkHourBegin = String.Empty;
                        _dicParams.TryGetValueConvert("workhourbegin", out sWorkHourBegin, String.Empty);
                        if (String.IsNullOrEmpty(sWorkHourBegin))
                        {
                            eudoDAL dal = eLibTools.GetEudoDAL(Pref);
                            dal.OpenDatabase();
                            ePlanning cal = new ePlanning(dal, Pref, _tab, _width, _height, null, true);
                            tsWorkHourBegin = cal.WorkHourBegin;
                            dal.CloseDatabase();
                        }
                        else
                        {
                            tsWorkHourBegin = eTools.GetTimeFromString(sWorkHourBegin);
                        }

                        defBeginDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, tsWorkHourBegin.Hours, 0, 0);
                    }
                    else
                        if (sDefaultBeginDate == "<DATETIME>")
                    {
                        //recherche de l'intervalle entier le plus proche
                        defBeginDate = DateTime.Now;

                        DateTime defCloserDate = DateTime.Now.Date;

                        //recherche aprochante
                        Int32 nMaxBcl = 0; // sécurité de sortie de boucle
                        while (defBeginDate > defCloserDate && nMaxBcl < (24 * 60))
                        {
                            if (defCloserDate.DayOfYear == defBeginDate.DayOfYear)
                                defCloserDate = defCloserDate.AddMinutes(nMinInter);
                            else
                                break;

                            nMaxBcl++;
                        }

                        Double nA = Math.Abs(defCloserDate.Subtract(defBeginDate).TotalMinutes);
                        Double nB = Math.Abs(defCloserDate.AddMinutes(-nMinInter).Subtract(defBeginDate).TotalMinutes);

                        //vérification de l'intervalle précédant
                        if (nB < nA)
                            defCloserDate = defCloserDate.AddMinutes(-nMinInter);

                        defBeginDate = defCloserDate;
                    }
                    else
                            if (sDefaultBeginDate.Contains("<DATE>"))
                    {
                        int nSign = 1;
                        int nPlace = sDefaultBeginDate.IndexOf("+");

                        if (nPlace == -1)
                        {
                            nSign = -1;
                            nPlace = sDefaultBeginDate.IndexOf("-");
                        }

                        if (nPlace != 0)
                        {
                            int decalage = nSign * eLibTools.GetNum(sDefaultBeginDate.Substring(nPlace + 1));
                            defBeginDate = DateTime.Now.AddDays(decalage);
                        }
                    }
                }
                //
                if (!String.IsNullOrEmpty(_date))
                    DateTime.TryParse(_date, out defBeginDate);
                headerRecordFields.FldBeginTime.Value = defBeginDate.ToString();

                defEndDate = defBeginDate.AddMinutes(nDefDuration);
                if (!String.IsNullOrEmpty(_endDate))
                {
                    DateTime.TryParse(_endDate, out defEndDate);
                    //Ajout d'un intervalle
                    defEndDate = defEndDate.AddMinutes(nMinInter);
                }

                headerRecordFields.FldEndTime.Value = defEndDate.ToString();
            }

            System.Web.UI.WebControls.Table tab = new System.Web.UI.WebControls.Table();
            tab.Style.Add(HtmlTextWriterStyle.Width, "100%");
            pnlLeft.Controls.Add(tab);

            if (headerRecordFields.FldBeginTime != null && headerRecordFields.FldBeginTime.RightIsVisible)
            {
                if (String.IsNullOrEmpty(headerRecordFields.FldBeginTime.Value))
                {
                    headerRecordFields.FldBeginTime.Value = new DateTime(1900, 01, 01, 12, 0, 1).ToString();
                }

                tab.Controls.Add(GetPlanningDatetimeField(headerRecordFields.FldBeginTime, true, (PlanningType)eLibTools.GetNum(headerRecordFields.FldType.Value) == PlanningType.CALENDAR_ITEM_EVENT ? false : true));
            }

            if (headerRecordFields.FldEndTime != null && headerRecordFields.FldEndTime.RightIsVisible && _calendarEnabled)
            {
                if (String.IsNullOrEmpty(headerRecordFields.FldEndTime.Value))
                {


                    DateTime dBegin;
                    if (DateTime.TryParse(headerRecordFields.FldBeginTime.Value, out dBegin))
                    {
                        DateTime dEnd = dBegin.AddMinutes(nDefDuration);
                        headerRecordFields.FldEndTime.Value = dEnd.ToString();
                    }
                    else
                    {
                        headerRecordFields.FldBeginTime.Value = new DateTime(1900, 01, 01, 12, 0, 1).ToString();
                        headerRecordFields.FldEndTime.Value = new DateTime(1900, 01, 01, 12, 0, 1).AddMinutes(nDefDuration).ToString();

                    }
                }

                tab.Controls.Add(GetPlanningDatetimeField(headerRecordFields.FldEndTime, (PlanningType)eLibTools.GetNum(headerRecordFields.FldType.Value) == PlanningType.CALENDAR_ITEM_TASK ? false : true, (PlanningType)eLibTools.GetNum(headerRecordFields.FldType.Value) == PlanningType.CALENDAR_ITEM_EVENT ? false : true));
            }





            HtmlGenericControl hoursPopup = new HtmlGenericControl("div");
            hoursPopup.ID = "HoursPopup";
            hoursPopup.Attributes.Add("class", "HoursPopup");
            hoursPopup.Attributes.Add("style", "display: none");
            pnlLeft.Controls.Add(hoursPopup);

            //Ligne toute la journée/périodicité/couleur
            TableRow trOpt1 = new TableRow();
            tab.Rows.Add(trOpt1);

            if (headerRecordFields.FldType.RightIsVisible)
            {
                TableCell tcAllDays = new TableCell();
                tcAllDays.CssClass = "table_labels";
                trOpt1.Controls.Add(tcAllDays);

                TableCell tcAllDaysLabel = new TableCell();
                tcAllDaysLabel.Controls.Add(eTools.GetCheckBoxOption(eResApp.GetRes(Pref, 841), "chkAllDays_" + this._myFile.ViewMainTable.DescId.ToString(), headerRecordFields.FldType.Value == PlanningType.CALENDAR_ITEM_EVENT.GetHashCode().ToString(), !headerRecordFields.FldType.RightIsUpdatable, "", "onAllDaysCheckOption"));
                tcAllDaysLabel.CssClass = "table_values";
                tcAllDaysLabel.ColumnSpan = 2;
                trOpt1.Controls.Add(tcAllDaysLabel);

                if ((PlanningType)eLibTools.GetNum(headerRecordFields.FldType.Value) == PlanningType.CALENDAR_ITEM_TASK)
                {
                    tcAllDaysLabel.Style.Add("visibility", "hidden");
                    tcAllDays.Style.Add("visibility", "hidden");
                }

                if (headerRecordFields.FldType.RightIsUpdatable || _myFile.Record.MainFileid == 0 && headerRecordFields.FldType.Value.Length > 0)
                    this.FieldsDescId.AddContains(headerRecordFields.FldType.FldInfo.Descid.ToString());

                if (headerRecordFields.FldType.RightIsUpdatable)
                    this.AllowedFieldsDescId.AddContains(headerRecordFields.FldType.FldInfo.Descid.ToString());
            }
            else
            {
                trOpt1.Controls.Add(new TableCell());
            }

            HtmlInputHidden txtDeteDescId = new HtmlInputHidden();
            txtDeteDescId.ID = "BeginDateDescId";
            txtDeteDescId.Value = _myFile.DateDescId.ToString();
            pnlLeft.Controls.Add(txtDeteDescId);

            Boolean bSchedule = false;
            if (headerRecordFields.FldSchedule != null && headerRecordFields.FldSchedule.RightIsVisible)
            {
                bSchedule = eLibTools.GetNum(headerRecordFields.FldSchedule.Value) > 0;
                headerRecordFields.FldSchedule.DisplayValue = headerRecordFields.FldSchedule.Value;

                TableCell tcSchedule1 = new TableCell();
                trOpt1.Cells.Add(tcSchedule1);
                tcSchedule1.CssClass = "table_labels";

                HtmlGenericControl chkSchedule = eTools.GetCheckBoxOption(eResApp.GetRes(Pref, 1049), "chkSchedule", bSchedule, !_myFile.Record.RightIsUpdatable, "", "ShowScheduleParameter", "div", headerRecordFields.FldSchedule);

                TableCell tcSchedule2 = new TableCell();
                trOpt1.Cells.Add(tcSchedule2);
                tcSchedule2.Controls.Add(chkSchedule);
                tcSchedule2.CssClass = "table_values";

                TableCell tcSchedule3 = new TableCell();
                tcSchedule3.ColumnSpan = 3;
                trOpt1.Cells.Add(tcSchedule3);

                TableRow scheduleTr = GetPlanningFieldDraw(headerRecordFields.FldSchedule);
                scheduleTr.Style.Add(HtmlTextWriterStyle.Display, "none");
                tab.Rows.Add(scheduleTr);

                if (headerRecordFields.FldSchedule.RightIsUpdatable || _myFile.Record.MainFileid == 0 && headerRecordFields.FldSchedule.Value.Length > 0)
                    FieldsDescId.AddContains(headerRecordFields.FldSchedule.FldInfo.Descid.ToString());

                if (headerRecordFields.FldSchedule.RightIsUpdatable)
                    AllowedFieldsDescId.AddContains(headerRecordFields.FldSchedule.FldInfo.Descid.ToString());

                HtmlGenericControl param = new HtmlGenericControl("div");
                tcSchedule3.Controls.Add(param);
                param.InnerText = eResApp.GetRes(Pref, 181);
                param.Attributes.Add("class", "gofile ");
                param.ID = "ScheduleParamsLnk";
                param.Attributes.Add("onclick", "ShowScheduleParameter()");
                if (!bSchedule)
                    param.Style.Add(HtmlTextWriterStyle.Display, "none");
                if (_nFileId > 0)
                {
                    param.Style.Add("display", "none");
                    chkSchedule.Style.Add("display", "none");
                }

                if (eLibTools.GetNum(headerRecordFields.FldSchedule.Value) > 0)
                {
                    TableRow trSchedule = new TableRow();
                    tab.Rows.Add(trSchedule);
                    trSchedule.Cells.Add(new TableCell());
                    TableCell tcSchedule4 = new TableCell();
                    trSchedule.Cells.Add(tcSchedule4);
                    tcSchedule4.ColumnSpan = MAX_HEAD_COLSPAN - 1;

                    string err = string.Empty;

                    int scheduleId = 0;
                    int planningTyp = 0;
                    if (int.TryParse(headerRecordFields.FldSchedule.Value, out scheduleId)
                        && int.TryParse(headerRecordFields.FldType.Value, out planningTyp))
                    {
                        try
                        {
                            string scheduleInfo = eScheduleInfos.GetScheduleLabelFromBase(_myFile.ViewMainTable.DescId, Pref,
                                scheduleId, (PlanningType)planningTyp);

                            tcSchedule4.Controls.Add(new LiteralControl(scheduleInfo));
                        }
                        catch
                        {
                            tcSchedule4.Controls.Add(new LiteralControl(err));
                        }
                    }

                    tcSchedule4.ID = "PeriodiciteInfo";
                    tcSchedule4.Attributes.Add("class", "PeriodiciteInfo");
                }
            }
            else
            {
                trOpt1.Controls.Add(new TableCell());
            }

            TableRow trColor = new TableRow();
            trColor.ID = eTools.GetFieldValueCellName(_myFile.Record, headerRecordFields.FldColor);
            trColor.Attributes.Add("did", headerRecordFields.FldColor.FldInfo.Descid.ToString());
            tab.Rows.Add(trColor);

            if (headerRecordFields.FldColor.RightIsVisible)
            {
                trColor.Cells.Add(new TableCell());

                TableCell tcColorPicker = new TableCell();
                tcColorPicker.ColumnSpan = MAX_HEAD_COLSPAN - 1;
                trColor.Controls.Add(tcColorPicker);

                Panel pnlColorPicker = new Panel();
                pnlColorPicker.CssClass = "colPicker";
                tcColorPicker.Controls.Add(pnlColorPicker);

                String colName = eTools.GetFieldValueCellName(_myFile.Record, headerRecordFields.FldColor);
                String divId = eTools.GetFieldValueCellId(String.Concat(colName, "_COLOR"), _myFile.Record.MainFileid, _nFileId);

                pnlColorPicker.Controls.Add(eTools.GetColorPicker(Pref, headerRecordFields.FldColor.Value,
                    divId,
                    eTools.GetFieldValueCellId(_myFile.Record, headerRecordFields.FldColor),
                    colName, headerRecordFields.FldColor.RightIsUpdatable, headerRecordFields.FldColor.FldInfo.ReadOnly));

                if (headerRecordFields.FldColor.RightIsUpdatable || _myFile.Record.MainFileid == 00 && headerRecordFields.FldColor.Value.Length > 0)
                    FieldsDescId.AddContains(headerRecordFields.FldColor.FldInfo.Descid.ToString());

                if (headerRecordFields.FldColor.RightIsUpdatable)
                    AllowedFieldsDescId.AddContains(headerRecordFields.FldColor.FldInfo.Descid.ToString());



                if (headerRecordFields.FldColor.RightIsUpdatable)
                    AllowedFieldsDescId.AddContains(headerRecordFields.FldColor.FldInfo.Descid.ToString());


                HtmlGenericControl spanLbl = new HtmlGenericControl("span");
                if (!String.IsNullOrEmpty(headerRecordFields.FldColor.FldInfo.StyleForeColor))
                    spanLbl.Style.Add("color", headerRecordFields.FldColor.FldInfo.StyleForeColor);
                spanLbl.Attributes.Add("class", "table_labels");
                spanLbl.Attributes.Add("title", String.Concat(headerRecordFields.FldColor.FldInfo.Libelle, (_bTooltipEnabled) ? String.Concat(Environment.NewLine, headerRecordFields.FldColor.FldInfo.ToolTipText) : ""));
                spanLbl.InnerText = headerRecordFields.FldColor.FldInfo.Libelle;

                pnlColorPicker.Controls.Add(spanLbl);
                if ((PlanningType)eLibTools.GetNum(headerRecordFields.FldType.Value) == PlanningType.CALENDAR_ITEM_TASK)
                    pnlColorPicker.Style.Add("display", "none");
            }
            else
            {
                trColor.Controls.Add(new TableCell());
            }

            // Confidentiel
            if (headerRecordFields.FldConfid.RightIsVisible)
            {
                TableRow trConfid = GetPlanningFieldDraw(headerRecordFields.FldConfid);
                tab.Controls.Add(trConfid);

                TableCell tc81 = new TableCell();
                tc81.ColumnSpan = 4;
                trConfid.Controls.Add(tc81);
                System.Web.UI.WebControls.Table t81 = new System.Web.UI.WebControls.Table();
                t81.CssClass = "tabconfid";
                t81.ID = "Fld_81";
                tc81.Controls.Add(t81);

                TableRow tr81 = GetPlanningFieldDraw(headerRecordFields.FldUserVisu);
                t81.Rows.Add(tr81);
                if (headerRecordFields.FldConfid.Value == "0")
                    t81.Style.Add(HtmlTextWriterStyle.Visibility, "hidden");

                TableCell tcSep = new TableCell();
                tcSep.CssClass = "tdDatSep";
                tr81.Cells.Add(tcSep);

                //Construit la liste des champs affichés en mode fiche
                if (headerRecordFields.FldConfid.RightIsUpdatable || _myFile.Record.MainFileid == 0 && headerRecordFields.FldConfid.Value.Length > 0)
                    FieldsDescId.AddContains(headerRecordFields.FldConfid.FldInfo.Descid.ToString());

                if (headerRecordFields.FldConfid.RightIsUpdatable)
                    AllowedFieldsDescId.AddContains(headerRecordFields.FldConfid.FldInfo.Descid.ToString());

            }

            // Alertes                
            if (_myFile.ViewMainTable.AlertEnabled && headerRecordFields.FldAlert.RightIsVisible && headerRecordFields.FldAlert != null && headerRecordFields.FldAlertDate != null && headerRecordFields.FldAlertSound != null && headerRecordFields.FldAlertTime != null)
            {
                if (headerRecordFields.FldAlert.RightIsUpdatable || _myFile.Record.MainFileid == 0 && headerRecordFields.FldAlert.Value.Length > 0)
                    FieldsDescId.AddContains(headerRecordFields.FldAlert.FldInfo.Descid.ToString());
                if (headerRecordFields.FldAlertDate.RightIsUpdatable || _myFile.Record.MainFileid == 0 && headerRecordFields.FldAlertDate.Value.Length > 0)
                    FieldsDescId.AddContains(headerRecordFields.FldAlertDate.FldInfo.Descid.ToString());
                if (headerRecordFields.FldAlertSound.RightIsUpdatable || _myFile.Record.MainFileid == 0 && headerRecordFields.FldAlertSound.Value.Length > 0)
                    FieldsDescId.AddContains(headerRecordFields.FldAlertSound.FldInfo.Descid.ToString());
                if (headerRecordFields.FldAlertTime.RightIsUpdatable || _myFile.Record.MainFileid == 0 && headerRecordFields.FldAlertTime.Value.Length > 0)
                    FieldsDescId.AddContains(headerRecordFields.FldAlertTime.FldInfo.Descid.ToString());


                if (headerRecordFields.FldAlert.RightIsUpdatable)
                    AllowedFieldsDescId.AddContains(headerRecordFields.FldAlert.FldInfo.Descid.ToString());

                if (headerRecordFields.FldAlertDate.RightIsUpdatable)
                    AllowedFieldsDescId.AddContains(headerRecordFields.FldAlertDate.FldInfo.Descid.ToString());

                if (headerRecordFields.FldAlertSound.RightIsUpdatable)
                    AllowedFieldsDescId.AddContains(headerRecordFields.FldAlertSound.FldInfo.Descid.ToString());

                if (headerRecordFields.FldAlertTime.RightIsUpdatable)
                    AllowedFieldsDescId.AddContains(headerRecordFields.FldAlertTime.FldInfo.Descid.ToString());


                TableRow trAlert0 = GetPlanningFieldDraw(headerRecordFields.FldAlert);
                HtmlGenericControl param = new HtmlGenericControl("div");
                param.InnerText = eResApp.GetRes(Pref, 181);
                param.Attributes.Add("class", "gofile");
                param.Attributes.Add("onclick", "ShowAlertParameter();");
                param.ID = "AlertParamLnk";
                param.Style.Add("display", headerRecordFields.FldAlert.Value == "1" ? "block" : "none");
                TableCell tc = new TableCell();
                tc.Controls.Add(param);
                tc.ColumnSpan = 4;
                trAlert0.Cells.Add(tc);

                TableRow trAlert1 = new TableRow();
                trAlert1.Style.Add("display", "none");
                TableCell tcAlert = new TableCell();
                trAlert1.Cells.Add(tcAlert);
                tcAlert.Controls.Add(GetPlanningHiddenField(headerRecordFields.FldAlertDate));
                tcAlert.Controls.Add(GetPlanningHiddenField(headerRecordFields.FldAlertSound));

                // Valeur par défaut à la demande GBO - Revue de sprint R&D 10.213 du 05/01/2017
                // La valeur définit dans la colonne default de desc est prioritaire  
                if (_myFile.FileId == 0 && String.IsNullOrEmpty(headerRecordFields.FldAlertTime.Value))
                    headerRecordFields.FldAlertTime.Value = eConst.REMINDER_DEFAULT_TIME.ToString();

                tcAlert.Controls.Add(GetPlanningHiddenField(headerRecordFields.FldAlertTime));

                tab.Rows.Add(trAlert0);
                tab.Rows.Add(trAlert1);

                trAlert1.Controls.Add(new TableCell());
                trAlert1.Controls.Add(new TableCell());
                trAlert1.Controls.Add(new TableCell());
            }

            //Champ notes
            TableRow trOpt2 = new TableRow();
            tab.Rows.Add(trOpt2);

            if (headerRecordFields.FldNotes != null && headerRecordFields.FldNotes.RightIsVisible)
            {
                TableRow trMemo = GetPlanningFieldDraw(headerRecordFields.FldNotes);
                if (trMemo.Cells.Count > 1)
                {


                    Int32 nRow = headerRecordFields.FldNotes.FldInfo.PosRowSpan;





                    trMemo.Cells[1].Height = Unit.Pixel(eConst.FILE_LINE_HEIGHT * nRow);
                    trMemo.Cells[1].ColumnSpan = 5;
                }
                tab.Rows.Add(trMemo);
            }

            //Séparateur
            Panel pnlsep = new Panel();
            pnlMain.Controls.Add(pnlsep);
            pnlsep.CssClass = "md_pl-base-Vsep";

            //Partie droite
            Panel pnlRight = new Panel();

            pnlMain.Controls.Add(pnlRight);
            pnlRight.CssClass = "tabRight";

            System.Web.UI.WebControls.Table tabRight = new System.Web.UI.WebControls.Table();
            tabRight.Style.Add(HtmlTextWriterStyle.Width, "100%");
            pnlRight.Controls.Add(tabRight);
            HtmlGenericControl hr = new HtmlGenericControl("hr");
            hr.Attributes.Add("class", "separatorRight");
            pnlRight.Controls.Add(hr);

            #region Affectation des champs TPL99 (Appartient à) et TPL92 (A faire par)
            if (headerRecordFields.FldMainUser != null && headerRecordFields.FldOtherUsers != null)
            {
                if (_nFileId == 0 && !_isPostback)
                {
                    #region Choix du mode d'affectation
                    // Demande #37 836 - En mode Jour, lorsqu'on crée une fiche Planning sur la colonne d'un utilisateur précis,
                    // on affecte la fiche Planning à cet utilisateur
                    bool bSetUsersFieldsFromConcernedUser = (this._viewMode == CalendarViewMode.VIEW_CAL_DAY_PER_USER && _concernedUser > 0);
                    // TODO: Décision a été prise de mettre cet utilisateur en Appartient à et de vider le champ A faire par (iso-v7) sans
                    // possibilité de paramétrage. Pourra éventuellement être rendu paramétrable en récupérant la valeur du booléen depuis un
                    // paramètre issu de CONFIGADV par exemple.
                    // A mettre à false pour adopter un comportement identique à Outlook (l'utilisateur connecté est mis en Appartient à,
                    // et l'utilisateur concerné en A faire par)
                    bool bConcernedUserAsMainUserOnly = true;
                    #endregion

                    #region Affectation en mode Jour ISO-v7 : utilisateur sélectionné en Appartient à
                    if (bSetUsersFieldsFromConcernedUser && bConcernedUserAsMainUserOnly)
                    {
                        // On remplit le champ Appartient à avec le UserID concerné
                        headerRecordFields.FldMainUser.Value = _concernedUser.ToString();
                        // Envoi d'une requête séparée pour récupérer le login de l'utilisateur concerné
                        // Il n'est pas nécessaire de le faire pour "A faire par" (FldOtherUsers) plus bas, car le rendu de ce champ est 
                        // effectué par GetPlanningMultiOwnerField() qui appelle GetUserLogin() - GetPlanningMultiOwnerField() étant
                        // elle-même appellée par GetPlanningFieldDraw() qui s'en charge
                        string concernedUserLogin = String.Empty;

                        eLibDataTools.GetUserLogin(Pref, headerRecordFields.FldMainUser.Value).TryGetValue(headerRecordFields.FldMainUser.Value, out concernedUserLogin);
                        headerRecordFields.FldMainUser.DisplayValue = concernedUserLogin;

                        // On vide le champ A faire par
                        headerRecordFields.FldOtherUsers.Value = String.Empty;
                        headerRecordFields.FldOtherUsers.DisplayValue = String.Empty;
                    }
                    #endregion
                    #region Autres affectations (mode Semaine, Mois, ou Jour ISO-XRM/Outlook avec utilisateur connecté en Appartient à)
                    else
                    {
                        string sMenuUserId = Pref.GetPref(_myFile.ViewMainTable.DescId, ePrefConst.PREF_PREF.MENUUSERID);

                        //#37206 - on vérifie si le filtre est activé
                        bool bEnabled = Pref.GetPref(_myFile.ViewMainTable.DescId, ePrefConst.PREF_PREF.MENUUSERENABLED) == "1";

                        if (sMenuUserId.Length > 0)
                        {
                            string[] aMainUser = sMenuUserId.Split(';');
                            if (bEnabled && eLibTools.GetNum(aMainUser[0]) > 0)
                            {
                                IDictionary<String, String> dicoUserMain = eLibDataTools.GetUserLogin(Pref, aMainUser[0]);

                                headerRecordFields.FldMainUser.Value = String.Join(";", dicoUserMain.Keys);
                                headerRecordFields.FldMainUser.DisplayValue = String.Join(", ", dicoUserMain.Values);

                                //headerRecordFields.FldMainUser.Value = aMainUser[0];
                            }


                            // Si bSetUsersFieldsFromConcernedUser est à true alors qu'on se trouve dans ce else { },
                            // bConcernedUserAsMainUserOnly est à false.
                            // Par conséquent, on adopte le comportement iso-Outlook et iso-XRM initial : on met l'utilisateur connecté en
                            // Appartient à, et l'utilisateur concerné en A faire par
                            if (bSetUsersFieldsFromConcernedUser)
                            {
                                // A faire par = utilisateur concerné
                                IDictionary<String, String> dicoUser = eLibDataTools.GetUserLogin(Pref, _concernedUser.ToString());
                                headerRecordFields.FldOtherUsers.Value = String.Join(";", dicoUser.Keys);
                                headerRecordFields.FldOtherUsers.DisplayValue = String.Join(", ", dicoUser.Values);


                            }
                            // Sinon, pour tous les autres cas/modes, le A faire est rempli avec l'ensemble des utilisateurs sélectionnés
                            // ... s'il n'y a pas de valeur par défaut sur le champ "A faire par" (#56948)
                            else if ((aMainUser.Length > 1 || sMenuUserId.IndexOf("G") > -1)
                                && string.IsNullOrEmpty(headerRecordFields.FldOtherUsers.FldInfo.DefaultValue))
                            {

                                IDictionary<String, String> dicoUserAll = eLibDataTools.GetUserLogin(Pref, sMenuUserId);

                                //headerRecordFields.FldOtherUsers.Value = sMenuUserId;
                                headerRecordFields.FldOtherUsers.Value = String.Join(";", dicoUserAll.Keys);
                                headerRecordFields.FldOtherUsers.DisplayValue = String.Join(", ", dicoUserAll.Values);

                                if (String.IsNullOrEmpty(headerRecordFields.FldMainUser.Value))
                                {
                                    string sAllId = eDataTools.GetAllUserId(sMenuUserId, Pref);

                                    String[] lstAllId = sAllId.Split(';');
                                    if (lstAllId.Length > 0)
                                    {
                                        IDictionary<String, String> dicoUserMain = eLibDataTools.GetUserLogin(Pref, lstAllId[0]);
                                        headerRecordFields.FldMainUser.Value = String.Join(";", dicoUserMain.Keys);
                                        headerRecordFields.FldMainUser.DisplayValue = String.Join(", ", dicoUserMain.Values);
                                    }



                                }
                            }
                        }



                    }
                    #endregion
                }

                if (headerRecordFields.FldMainUser.RightIsVisible)
                {
                    TableRow trMainUser = GetPlanningFieldDraw(headerRecordFields.FldMainUser);
                    trMainUser = GetPlanningFieldDraw(headerRecordFields.FldMainUser);
                    trMainUser.CssClass = String.Concat(trMainUser.CssClass, " trPlanningMainUser");
                    tabRight.Controls.Add(trMainUser);
                }

                if (headerRecordFields.FldOtherUsers.RightIsVisible)
                {
                    TableRow tdOtherUsers = GetPlanningFieldDraw(headerRecordFields.FldOtherUsers);
                    tdOtherUsers.CssClass = String.Concat(tdOtherUsers.CssClass, " trPlanningOtherUser");
                    tabRight.Controls.Add(tdOtherUsers);
                }
            }
            #endregion

            AddPlanningParent(pnlRight);

            return pnlMain;
        }


        protected virtual void AddPlanningParent(Panel container)
        {
            eRenderer footRenderer = eRendererFactory.CreateParenttInFootRenderer(Pref, this);
            Panel pgC = null;
            if (footRenderer.ErrorMsg.Length > 0)
            {
                this._sErrorMsg = footRenderer.ErrorMsg;    //On remonte l'erreur
            }
            if (footRenderer != null)
                pgC = footRenderer.PgContainer;

            container.Controls.Add(pgC);
        }



        /// <summary>
        /// Retourne un champs utilisateur (99/92)
        /// </summary>
        /// <param name="fld"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        private HtmlGenericControl GetPlanningMultiOwnerField(eFieldRecord fld, eRecord row)
        {
            HtmlGenericControl webControl = new HtmlGenericControl("div");
            if (!fld.RightIsVisible)
                return webControl;



            if (fld.RightIsUpdatable || row.MainFileid == 0 && fld.Value.Length > 0)
                FieldsDescId.AddContains(fld.FldInfo.Descid.ToString());

            if (fld.RightIsUpdatable)
            {
                AllowedFieldsDescId.AddContains(fld.FldInfo.Descid.ToString());
            }



            webControl.Attributes.Add("class", "multiusers");

            // Id de la fiche du field
            Int32 fieldFileId = fld.FileId;

            webControl.Attributes.Add("dbv", fld.Value);
            webControl.Attributes.Add("planningmultiuser", "1");
            webControl.Attributes.Add("showcurrentuser", "1");

            // Id de la table principale
            Int32 nMasterFileId = row.MainFileid;

            // ID de la cellule
            webControl.ID = eTools.GetFieldValueCellId(row, fld);
            webControl.Attributes.Add("ename", eTools.GetFieldValueCellName(row, fld));
            webControl.Attributes.Add("mult", fld.FldInfo.Multiple ? "1" : "0");

            if (fld.RightIsUpdatable && row.RightIsUpdatable)
                webControl.Attributes.Add("eaction", "LNKCATUSER");
            else
                webControl.Attributes["class"] = webControl.Attributes["class"] + " readonly";

            webControl.Attributes.Add("efld", "1");
            webControl.Attributes.Add("fulluserlist", fld.FldInfo.IsFullUserList ? "1" : "0");
            webControl.Attributes.Add("calconflict", "1");
            // #47770
            if (fld.IsMandatory)
                webControl.Attributes.Add("obg", "1");

            if (fld.Value != "")
            {
                webControl.Attributes.Add("onmouseover", String.Concat("ste(event, '", webControl.ID, "');"));
                webControl.Attributes.Add("onmouseout", "ht();");
            }

            Control usrFld = new Control();
            // HLA - ID inutile ?
            usrFld.ID = string.Concat(_myFile.ViewMainTable.DescId, "_", _myFile.ViewMainTable.DescId + fld.FldInfo.Descid);
            webControl.Controls.Add(usrFld);

            Int32 cnt = 0;
            IDictionary<String, String> dicoUser = eLibDataTools.GetUserLogin(Pref, fld.Value);
            foreach (KeyValuePair<String, String> keyValue in dicoUser)
            {
                cnt++;

                HtmlGenericControl ctrl = new HtmlGenericControl("span");
                usrFld.Controls.Add(ctrl);
                ctrl.ID = String.Concat(fld.FldInfo.Alias, "_", keyValue.Key.Replace(" ", ""));
                ctrl.Attributes.Add("dbv", keyValue.Key);
                ctrl.Attributes.Add("class", "participant conflict_free");

                ctrl.InnerText = String.Concat(keyValue.Value, (cnt == dicoUser.Count ? String.Empty : ", "));
            }

            TextBox txtConflict = new TextBox();
            txtConflict.ID = "conflictIndicator";
            txtConflict.Text = "0";
            txtConflict.Style.Add("display", "none");
            webControl.Controls.Add(txtConflict);

            return webControl;
        }

        /// <summary>
        /// Retourne un champs invisible
        /// </summary>
        /// <param name="myField"></param>
        /// <returns></returns>
        protected Control GetPlanningHiddenField(eFieldRecord myField)
        {
            HtmlGenericControl div = new HtmlGenericControl("div");

            HtmlGenericControl myLabel = new HtmlGenericControl("div");

            myLabel.Attributes.Add("ID", eTools.GetFieldValueCellName(_myFile.Record, myField));
            myLabel.Attributes.Add("did", myField.FldInfo.Descid.ToString());
            myLabel.Attributes.Add("lib", myField.FldInfo.Libelle);
            myLabel.Attributes.Add("mult", myField.FldInfo.Multiple ? "1" : "0");
            myLabel.Attributes.Add("tree", myField.FldInfo.PopupDataRend == PopupDataRender.TREE ? "1" : "0");

            myLabel.Attributes.Add("rul", myField.FldInfo.IsInRules ? "1" : "0");
            myLabel.Attributes.Add("mf", myField.FldInfo.HasMidFormula ? "1" : "0");

            myLabel.Attributes.Add("Tab", "tab");
            myLabel.Attributes.Add("fld", "fld");

            myLabel.Attributes.Add("pop", myField.FldInfo.Popup.GetHashCode().ToString());
            myLabel.Attributes.Add("popid", myField.FldInfo.PopupDescId.ToString());
            myLabel.Attributes.Add("bndId", myField.FldInfo.BoundDescid.ToString());

            TextBox txt = new TextBox();
            txt.Text = myField.Value;
            txt.Attributes.Add("ename", eTools.GetFieldValueCellName(_myFile.Record, myField));
            txt.ID = eTools.GetFieldValueCellId(_myFile.Record, myField);

            if (!myField.RightIsUpdatable)
            {
                txt.Attributes.Add("readonly", "readonly");
                txt.Attributes.Add("ero", "1");
            }

            div.Controls.Add(myLabel);
            div.Controls.Add(txt);

            return div;
        }

        /// <summary>
        /// Crée et insère un nchamps dans planning
        /// </summary>
        /// <returns></returns>
        protected virtual TableRow GetPlanningFieldDraw(eFieldRecord myField)
        {
            //Création du cartouche d'entête
            TableRow myTr;

            myTr = new TableRow();

            if (!myField.RightIsVisible)
                return myTr;

            Boolean bDisplayBtn = _bIsEditRenderer && myField.RightIsUpdatable;

            TableCell myValue = new TableCell();

            TableCell myLabel = new TableCell();
            this.GetFieldLabelCell(myLabel, _myFile.Record, myField);
            myLabel.Attributes.Add("class", String.Concat("table_labels", myField.IsMandatory ? " mandatory_Label" : ""));
            myTr.Cells.Add(myLabel);

            TableCell myValueCell = null;
            if (myField.FldInfo.Descid == _myFile.ViewMainTable.GetOwnerDescId() && _nFileId == 0)
            {
                if (String.IsNullOrEmpty(myField.Value) && !_isPostback)
                {
                    myField.Value = Pref.GetPref(_myFile.ViewMainTable.DescId, ePrefConst.PREF_PREF.DEFAULTOWNER);


                    IDictionary<String, String> dicoUser = eLibDataTools.GetUserLogin(Pref, myField.Value);
                    myField.Value = String.Join(";", dicoUser.Keys);
                    myField.DisplayValue = String.Join(", ", dicoUser.Values);

                }

                /* * MOU 26/09/2013  Traitement de masse ; affectation d une nouvelle fiche
                 * 
                 * le "appartient a" est egale a l'utilisateur connecté (userlogin) si on affecte un template (hors planning)
                 * sinon  <identique a la fiche @table> ou table est egale a PP , PM, EVT ou EVT_XX
                 * 
                 * */

                //si premier chargement-> owener est -1 
                if (GlobalAffect && !_isPostback && myField.FldInfo.Table.EdnType == EdnType.FILE_PLANNING)
                    myField.Value = "-1";

                if (GlobalAffect && myField.FldInfo.Table.EdnType == EdnType.FILE_PLANNING && myField.Value.Equals("-1"))
                {
                    //Traitement de masse
                    //Sur les fiches créees le 'Appartient à' a la valeur '<Identique à la fiche Contacts>'    
                    myField.DisplayValue = String.Concat("<", eResApp.GetRes(Pref, 819), " ", eLibTools.GetPrefName(Pref, _myFile.FileContext != null ? _myFile.FileContext.TabFrom : 0), ">");
                }
            }

            if (myField.FldInfo.Descid == _myFile.ViewMainTable.DescId + AllField.TPL_MULTI_OWNER.GetHashCode()
                  || myField.FldInfo.Descid == _myFile.ViewMainTable.DescId + AllField.USER_VISIBLE.GetHashCode())
            {
                HtmlGenericControl valueContainer = GetPlanningMultiOwnerField(myField, _myFile.Record);

                myValueCell = new TableCell();
                myValueCell.CssClass += " table_values";
                myValueCell.Controls.Add(valueContainer);
            }
            else
                myValueCell = (TableCell)GetFieldValueCell(_myFile.Record, myField, 0, Pref);

            if ((myField.FldInfo.PopupDescId == 201 && myField.FldInfo.Descid != 201) && (!String.IsNullOrEmpty(Pref.GetConfigDefault(new eLibConst.CONFIG_DEFAULT[] { eLibConst.CONFIG_DEFAULT.VCARDMAPPING })[eLibConst.CONFIG_DEFAULT.VCARDMAPPING])))
            {
                myValueCell.Attributes.Add("onmouseover", String.Concat("shvc(this, 1)"));
                myValueCell.Attributes.Add("onmouseout", String.Concat("shvc(this, 0)"));
            }

            myValueCell.RowSpan = myValue.RowSpan;
            myValueCell.ColumnSpan = myValue.ColumnSpan;

            // TODO : taille de cellule à mettre en const
            if (myField.FldInfo.Format == FieldFormat.TYP_MEMO)
            {
                Int32 nRow = myValueCell.RowSpan;
                if (nRow == 0)
                    nRow++;

                myValueCell.Height = eConst.FILE_LINE_HEIGHT * nRow;
            }
            else
                myValueCell.Height = eConst.FILE_LINE_HEIGHT * myValueCell.RowSpan;

            myTr.Cells.Add(myValueCell);

            TableCell cell = GetButtonCell(myValueCell, bDisplayBtn);
            myTr.Cells.Add(cell);

            if (myField.FldInfo.Descid == _myFile.ViewMainTable.DescId + AllField.TPL_MULTI_OWNER.GetHashCode()
                || myField.FldInfo.Descid == _myFile.ViewMainTable.DescId + AllField.USER_VISIBLE.GetHashCode())
            {
                cell.Attributes.Add("calconflict", "1");
            }

            // #43772
            if (myField.RightIsUpdatable || _myFile.Record.MainFileid == 0 && myField.Value.Length > 0)
                this.FieldsDescId.AddContains(myField.FldInfo.Descid.ToString());

            if (myField.RightIsUpdatable)
                AllowedFieldsDescId.AddContains(myField.FldInfo.Descid.ToString());

            return myTr;
        }

        /// <summary>
        /// Surcharge de la méthode pour effectué l'affichage des options des rubrique Alert et/ou Confidentiel
        /// </summary>
        /// <param name="rowRecord">Ligne de la liste a afficher</param>
        /// <param name="fieldRecord">Le champ binaire</param>
        /// <param name="sClassAction">classe CSS choisi pour l'element</param>
        /// <returns>Retourne le control généré pour la rubrique de type BIT (retourne un eCheckBoxCtrl)</returns>
        protected override WebControl RenderBitFieldFormat(eRecord rowRecord, eFieldRecord fieldRecord, ref string sClassAction)
        {


            WebControl webCtrl = base.RenderBitFieldFormat(rowRecord, fieldRecord, ref sClassAction);
            if (webCtrl == null)
                return null;

            eCheckBoxCtrl ctrl = (eCheckBoxCtrl)webCtrl;

            if (fieldRecord == headerRecordFields.FldConfid)
                ctrl.AddClick(headerRecordFields.FldConfid.RightIsUpdatable ? String.Concat("OnConfidClick(this,", _myFile.ViewMainTable.DescId, ");") : String.Empty);
            else if (fieldRecord == headerRecordFields.FldAlert)
                ctrl.AddClick(headerRecordFields.FldAlert.RightIsUpdatable ? String.Concat("OnAlertClick(this,", _myFile.ViewMainTable.DescId, ");") : String.Empty);
            return ctrl;
        }

        /// <summary>
        /// Insère un champ date de planning
        /// </summary>
        /// <returns></returns>
        private TableRow GetPlanningDatetimeField(eFieldRecord fld, bool bVisible, bool isFldHourVisible)
        {

            TableRow tr = new TableRow();

            String cellAlias = eTools.GetFieldValueCellName(_myFile.Record, fld);

            TableCell tcLbl = new TableCell();
            tr.Cells.Add(tcLbl);
            tcLbl.CssClass = "table_labels";
            tcLbl.ID = eTools.GetFieldValueCellName(_myFile.Record, fld);
            tcLbl.Attributes.Add("did", fld.FldInfo.Descid.ToString());
            if (fld.FldInfo.CancelLastValueAllowed)
                tcLbl.Attributes.Add("cclval", "1");
            if (fld.FldInfo.Format == FieldFormat.TYP_DATE)
            {   //GCH - #36012/#35859 - Internationnalisation - Planning
                tcLbl.Attributes.Add("frm", fld.FldInfo.Format.GetHashCode().ToString());
                Engine.ORM.OrmMappingInfo ormInfo = eLibTools.OrmLoadAndGetMapWeb(_ePref);
                bool mf = fld.FldInfo.HasMidFormula || ormInfo.GetAllValidatorDescId.Contains(fld.FldInfo.Descid);
                tcLbl.Attributes.Add("mf", mf ? "1" : "0");
                tcLbl.Attributes.Add("fid", fld.FileId.ToString());
            }
            
            HtmlGenericControl lbl = new HtmlGenericControl("div");
            lbl.InnerText = fld.FldInfo.Libelle;
            lbl.Attributes.Add("title", String.Concat(fld.FldInfo.Libelle, (_bTooltipEnabled) ? String.Concat(Environment.NewLine, fld.FldInfo.ToolTipText) : ""));
            //BBA - Ajout des styles sur le les libelles 
            eTools.SetHTMLControlStyle(fld, lbl);

            tcLbl.Controls.Add(lbl);

            lbl.ID = eTools.GetFieldValueCellId(String.Concat(cellAlias, "_LBL"), _myFile.Record.MainFileid, fld.FileId);

            TextBox txtDate = new TextBox();

            // le champs date n'est pas modifiable
            if (!fld.RightIsUpdatable)
            {
                // On ajoute le readonly fonctionnel et le visuel
                // Et on supprime les action js
                txtDate.ReadOnly = true;
                txtDate.CssClass = "readonly";

            }
            else
            {
                txtDate.CssClass = "LNKFREETEXT edit";
                txtDate.Attributes.Add("ename", eTools.GetFieldValueCellName(_myFile.Record, fld));
                txtDate.Attributes.Add("onchange", String.Concat("doOnchangeDate(", _myFile.ViewMainTable.DescId, ");"));
            }


            txtDate.ID = eTools.GetFieldValueCellId(String.Concat(cellAlias, "_D"), _myFile.Record.MainFileid, fld.FileId);

            TableCell tcText = new TableCell();
            tr.Cells.Add(tcText);
            tcText.Controls.Add(txtDate);

            TableCell tcBtn = new TableCell();
            tr.Cells.Add(tcBtn);

            if (!fld.RightIsUpdatable)
            {
                tcBtn.CssClass = "";
            }
            else
            {
                tcBtn.Attributes.Add("onclick", "selectDate('" + txtDate.ID + "')");
                tcBtn.CssClass = "icon-agenda btnIe8 ";
            }

            tcBtn.ID = eTools.GetFieldValueCellId(String.Concat(cellAlias, "_D_BTN"), _myFile.Record.MainFileid, fld.FileId);

            TextBox txtHours = new TextBox();

            // Definition de l'element en dehors de la condition
            txtHours.ID = eTools.GetFieldValueCellId(String.Concat(cellAlias, "_H"), _myFile.Record.MainFileid, fld.FileId);

            if (!fld.RightIsUpdatable)
            {
                txtHours.ReadOnly = true;
                txtHours.CssClass = "CalHour readonly";

            }
            else
            {
                txtHours.CssClass = "CalHour";
                txtHours.Attributes.Add("onchange", String.Concat("doOnchangeDate(", _myFile.ViewMainTable.DescId, ");"));
            }



            //Champ caché de contenu
            TextBox txtHidden = new TextBox();
            txtHidden.ID = eTools.GetFieldValueCellId(_myFile.Record, fld);
            txtHidden.Attributes.Add("ename", cellAlias);
            tcText.Controls.Add(txtHidden);
            //GCH - #36012 - Internationnalisation - Planning
            txtHidden.Text = eDate.ConvertBddToDisplay(Pref.CultureInfo, fld.Value);
            txtHidden.Style.Add(HtmlTextWriterStyle.Display, "none");
            if (!fld.RightIsUpdatable)
            {
                txtHidden.Attributes.Add("readonly", "readonly");
                txtHidden.Attributes.Add("ero", "1");
            }

            TableCell tcSeparator = new TableCell();
            tcSeparator.CssClass = "sep_dates";
            tr.Cells.Add(tcSeparator);

            TableCell tcHour = new TableCell();
            tcHour.Style.Add("visibility", isFldHourVisible ? "visible" : "hidden");
            tr.Cells.Add(tcHour);
            tcHour.Controls.Add(txtHours);




            if (!String.IsNullOrEmpty(fld.Value))
            {
                DateTime dt = new DateTime();
                DateTime.TryParse(fld.Value, out dt);
                //GCH - #36012 - Internationnalisation - Planning
                txtDate.Text = eDate.ConvertBddToDisplay(Pref.CultureInfo, dt.ToString("dd/MM/yyyy"));
                txtHours.Text = dt.ToString("HH:mm");
            }
            else if (fld.FldInfo.Descid.ToString().EndsWith("02") || fld.FldInfo.Descid.ToString().EndsWith("89"))
            {
                DateTime dt = new DateTime(DateTime.Now.Ticks);
                txtDate.Text = eDate.ConvertBddToDisplay(Pref.CultureInfo, dt.ToString("dd/MM/yyyy"));
                txtHours.Text = dt.ToString("HH:mm");
            }

            txtDate.Attributes.Add("oldvalue", txtDate.Text);
            txtHours.Attributes.Add("oldvalue", txtHours.Text);

            // Création du bouton de selection de plage horaire.
            TableCell tcHourBtn = new TableCell();

            //Toute cette partie ne s'active que si les valeurs sont modifiables.
            if (fld.RightIsUpdatable)
            {
                Panel pnlHoursChoice = new Panel();
                pnlHoursChoice.ID = txtHours.ID + "_CHOICE";
                DateTime currentTime = DateTime.Now.Date;
                pnlHoursChoice.Style.Add(HtmlTextWriterStyle.Position, "absolute");

                for (int i = 0; i < 24 * 60; i += _myFile.CalendarInterval)
                {
                    currentTime = DateTime.Now.Date.AddMinutes(i);
                    string sHour = String.Concat(currentTime.Hour.ToString().PadLeft(2, '0'), ":", currentTime.Minute.ToString().PadLeft(2, '0'));
                    HtmlGenericControl interval = new HtmlGenericControl("div");
                    interval.InnerHtml = sHour;
                    interval.Attributes.Add("onclick", string.Concat("var obj=document.getElementById('", txtHours.ID, "'); obj.setAttribute('oldvalue',obj.value); obj.value='", sHour, "';document.getElementById('", pnlHoursChoice.ID, "').style.display='none'; validDateFields(obj);"));
                    pnlHoursChoice.Controls.Add(interval);
                }

                pnlHoursChoice.CssClass = "hours_choice";
                pnlHoursChoice.Style.Add(HtmlTextWriterStyle.Display, "none");
                tcHour.Controls.Add(pnlHoursChoice);
                tcHourBtn.Style.Add("visibility", isFldHourVisible ? "visible" : "hidden");
                tr.Cells.Add(tcHourBtn);
                tcHourBtn.CssClass = "icon-input btn";
                tcHourBtn.ID = eTools.GetFieldValueCellId(String.Concat(cellAlias, "_H_BTN"), _myFile.Record.MainFileid, fld.FileId);

                if (fld.RightIsUpdatable)
                    tcHourBtn.Attributes.Add("onclick", "showHourPopup(" + (fld.FldInfo.Descid == (_myFile.ViewMainTable.DescId + PlanningField.DESCID_TPL_END_TIME.GetHashCode()) ? "true" : "false") + ", this," + _myFile.ViewMainTable.DescId + ")");// "selectHour('" + txtHours.ID + "')");

            }

            //JAS déplacement des champs heure "au plus près" des champs dates.
            TableCell tcSep = new TableCell();
            tcSep.CssClass = "tdDatSep";
            tr.Cells.Add(tcSep);

            if (fld.RightIsUpdatable || _myFile.Record.MainFileid == 0 && fld.Value.Length > 0)
                FieldsDescId.AddContains(fld.FldInfo.Descid.ToString());

            if (fld.RightIsUpdatable)
                AllowedFieldsDescId.AddContains(fld.FldInfo.Descid.ToString());

            if (!bVisible)
            {
                txtDate.Style.Add("visibility", "hidden");
                txtHours.Style.Add("visibility", "hidden");
                tcBtn.Style.Add("visibility", "hidden");
                tcHourBtn.Style.Add("visibility", "hidden");
                tcLbl.Style.Add("visibility", "hidden");
            }

            FieldsDescId.AddContains(fld.FldInfo.Descid.ToString());

            return tr;
        }

        /// <summary>
        /// Action spécifique au Site web
        /// </summary>
        /// <param name="fieldRow">Information du champ courant</param>
        /// <param name="ednWebControl">Controle qui va être ajoute</param>
        /// <returns></returns>
        public override String GetFieldValueCell_TYP_WEB(EdnWebControl ednWebControl, eFieldRecord fieldRow)
        {
            String sClassAction = String.Empty;
            if (fieldRow.RightIsUpdatable)
                sClassAction = "LNKWEBSIT";

            GetValueContentControl(ednWebControl, fieldRow.DisplayValue);
            return sClassAction;
        }

        #endregion


        /// <summary>
        /// champs composant les propriétés : je fais ma liste de courses
        /// </summary>
        /// <param name="PtyFieldsDescId">Liste des descid de champs à afficher</param>
        /// <returns></returns>
        protected override List<Int32> GetPropertiesFields(ref List<Int32> PtyFieldsDescId)
        {
            if (PtyFieldsDescId == null)
                PtyFieldsDescId = new List<Int32>();

            // Créé par, Créé le
            PtyFieldsDescId.Add(_myFile.ViewMainTable.DescId + AllField.DATE_CREATE.GetHashCode());
            PtyFieldsDescId.Add(_myFile.ViewMainTable.DescId + AllField.USER_CREATE.GetHashCode());

            // Modifié le, Modifié par
            PtyFieldsDescId.Add(_myFile.ViewMainTable.DescId + AllField.DATE_MODIFY.GetHashCode());
            PtyFieldsDescId.Add(_myFile.ViewMainTable.DescId + AllField.USER_MODIFY.GetHashCode());

            // Géo
            PtyFieldsDescId.Add(_myFile.ViewMainTable.DescId + AllField.GEOGRAPHY.GetHashCode());

            return PtyFieldsDescId;
        }

    }
}