using Com.Eudonet.Internal;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Common.Enumerations;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Objet Rendez-vous
    /// </summary>
    public class ePlanningItem
    {
        #region Vars et proptriétés
        /// <summary>Type de mode planning sélectionné (Mois, semaine, jours, jorus multi user...)</summary>
        CalendarViewMode _nViewMode;
        private ePlanningDay _parentDay;
        private PlanningType _typ;
        private DateTime _dateBegin;
        private DateTime _dateEnd;
        private String _innerHtml;
        /// <summary>Couleur de l'intérieur du RDV</summary>
        private String _color = string.Empty;
        /// <summary>Couleur de la bande à droite</summary>
        private String _gripColor;
        private eRecord _row;
        private List<eFieldRecord> _fieldsToDisplay;
        private HtmlGenericControl _divElement;
        private String _parentIntervalId;
        private String _toolTipInfos;
        private Boolean _bConfidential = false;
        private Int32 _concernedUserId;
        private ePlanning _parentCal;
        private Int32 _itemRange = 0;
        private HtmlGenericControl _scheduleDiv;
        private Int32 _left = -1;
        private Int32 _maxLeft = 0;




        /// <summary>
        /// Jour parent
        /// </summary>
        public ePlanningDay ParentDay
        {
            get { return _parentDay; }
            set { _parentDay = value; }
        }

        /// <summary>
        /// Liste des champs affichés
        /// </summary>
        public List<eFieldRecord> FieldsToDisplay
        {
            get { return _fieldsToDisplay; }
            set { _fieldsToDisplay = value; }
        }

        /// <summary>
        /// Infos Tooltip
        /// </summary>
        public String ToolTipInfos
        {
            get { return _toolTipInfos; }
            set { _toolTipInfos = value; }
        }

        /// <summary>
        /// L'élement HTML du RDV
        /// </summary>
        public HtmlGenericControl DivElement
        {
            get { return _divElement; }
        }

        /// <summary>
        /// ID de l'intervalle parent
        /// </summary>
        public String ParentIntervalId
        {
            get { return _parentIntervalId; }
        }
        /// <summary>
        /// DescId Date Début
        /// </summary>
        public int DateBeginDescid
        {
            get;
            private set;
        }
        /// <summary>
        /// Date Début
        /// </summary>
        public DateTime DateBegin
        {
            get { return _dateBegin; }
            set { _dateBegin = value; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is all day.
        /// </summary>
        public Boolean IsAllDay { get { return _typ == PlanningType.CALENDAR_ITEM_EVENT; } }

        /// <summary>
        /// Date fin
        /// </summary>
        public DateTime DateEnd
        {
            get { return _dateEnd; }
            set { _dateEnd = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public Int32 FileId
        {
            get { return _nFileId; }
            set { _nFileId = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public Int32 ConcernedUserId
        {
            get { return _concernedUserId; }
            set { _concernedUserId = value; }
        }

        /// <summary>
        /// Ordre de l'element si sur plusieur jours
        /// </summary>
        public Int32 ItemRange
        {
            get { return _itemRange; }
            set { _itemRange = value; }

        }


        /// <summary>
        /// Id de l'user principal
        /// </summary>
        public string OwnerId { get; set; }
        /// <summary>
        /// Ids des participants
        /// </summary>
        public string MultiOwnerId { get; set; }
        /// <summary>
        /// Gets or sets the schedule identifier.
        /// </summary>
        /// <value>
        /// The schedule identifier.
        /// </value>
        public int ScheduleId { get; set; }


        /// <summary>
        /// id de la fiche planning master dans le cas des récurrent
        /// </summary>
        public int MasterFileId { get; set; } = 0;

        #endregion

        #region mise à jour, redim et déplaceent

        /// <summary>Indique que le rendez-vous est visualisable</summary>
        private Boolean _isViewable = true;
        /// <summary>Indique si l'on peut redimensionner par le haut le RDV (cas de rdv sur plusieurs jours par exemple)</summary>
        private Boolean _isTopResizeEnabled = true;
        /// <summary>Indique si l'on peut redimensionner par le bas le RDV (cas de rdv sur plusieurs jours par exemple)</summary>
        private Boolean _isBottomResizeEnabled = true;
        /// <summary>Indique que le rendez-vous est déplacable</summary>
        private Boolean _isMoveEnabled = true;

        #endregion

        #region Variable en mode connectée (connexion à la bdd)
        /// <summary>Table courante</summary>
        private int _nTab = 0;
        /// <summary>Fiche courante si plusieurs se référer à listeFileId</summary>
        private int _nFileId = 0;
        /// <summary>Fiches courantes si plusieurs se référer à listeFileId</summary>
        private List<Int32> _listFileId = new List<Int32>();
        /// <summary>Objet eudoquery générant les requêtes XRM</summary>
        private EudoQuery.EudoQuery _queryCal = null;
        /// <summary>Conteneur des infos des données après chargement de la requete eudo query</summary>
        private DataTableReaderTuned _dtr = null;
        private ePref _pref = null;
        /// <summary>Objet d'accès à la BDD de XRM</summary>
        private eudoDAL _dal;
        /// <summary>Liste des Planning Item parallèle (cas par exemple du mode mois avec plusieur rdv sur le même Encart)</summary>
        List<ePlanningItem> _listEPI = new List<ePlanningItem>();

        /// <summary>Couleur de l'intérieur du RDV</summary>
        public String Color
        {
            get { return _color; }
        }
        /// <summary>Couleur de la bande à droite</summary>
        public String GripColor
        {
            get { return _gripColor; }
        }
        /// <summary>Pref XRM</summary>
        public ePref Pref
        {
            get { return _pref; }
            set { _pref = value; }
        }
        /// <summary>
        /// Gets or sets the left.
        /// </summary>
        public int Left
        {
            get { return _left; }
            set { _left = value; }
        }
        /// <summary>
        /// Gets or sets the maximum left.
        /// </summary>
        public int MaxLeft
        {
            get { return _maxLeft; }
            set { _maxLeft = value; }
        }
        /// <summary>Indique que le rendez-vous est déplacable</summary>
        public Boolean IsMovable
        {
            get { return _isMoveEnabled; }
            set { _isMoveEnabled = value; }
        }
        /// <summary>Indique si l'on peut redimensionner par le haut le RDV (cas de rdv sur plusieurs jours par exemple)</summary>
        public Boolean IsTopResizeEnabled
        {
            get { return _isTopResizeEnabled; }
            set { _isTopResizeEnabled = value; }
        }
        /// <summary>Indique si l'on peut redimensionner par le bas le RDV (cas de rdv sur plusieurs jours par exemple)</summary>
        public Boolean IsBottomResizeEnabled
        {
            get { return _isBottomResizeEnabled; }
            set { _isBottomResizeEnabled = value; }
        }
        /// <summary>
        /// Gets or sets the planning type.
        /// </summary>
        public PlanningType TypePlanning
        {
            get { return _typ; }
            set { _typ = value; }
        }
        #endregion

        #region CONSTRUCTEUR
        /// <summary>
        /// 
        /// </summary>
        public ePlanningItem()
        {

        }


        /// <summary>
        /// Constructeur pour la récupération pour un seul FILEID
        /// </summary>
        /// <param name="queryCal">Objet eudoquery générant les requêtes XRM</param>
        /// <param name="parentCal">RDV parent dans le cas de plusieurs RDV en un pour le mode mois</param>
        /// <param name="dtr"></param>
        /// <param name="itemRange"></param>
        /// <param name="addedItems"></param>
        public ePlanningItem(EudoQuery.EudoQuery queryCal, ePlanning parentCal, DataTableReaderTuned dtr, Int32 itemRange, out List<ePlanningItem> addedItems)
        {
            _nViewMode = parentCal.ViewMode;
            _nTab = parentCal.Tab;
            _pref = parentCal.Pref;
            _parentCal = parentCal;
            _itemRange = itemRange;
            addedItems = new List<ePlanningItem>();
            LoadItemInfos(queryCal, dtr, parentCal.UserDisplayed, out addedItems);
        }

        /// <summary>
        /// Constructeur pour la récupération pour un seul FILEID
        /// </summary>
        /// <param name="dal">Objet de connexion à la BDD XRM</param>
        /// <param name="pref">Preférences de l'utilisateur en cours</param>
        /// <param name="tab">Fichier Planning (descid)</param>
        /// <param name="nFileId">FileId de la fiche pointée</param>
        /// <param name="modeBkm">if set to <c>true</c> [mode BKM].</param>
        /// <exception cref="System.Exception">
        /// ePlanningItem.Initialize : " + err
        /// or
        /// ePlanningItem.LoadItems : " + err
        /// </exception>
        public ePlanningItem(eudoDAL dal, ePref pref, Int32 tab, Int32 nFileId, Boolean modeBkm = false)
        {
            _nViewMode = CalendarViewMode.VIEW_UNDEFIED;
            _dal = dal;
            _pref = pref;
            _nTab = tab;
            _nFileId = nFileId;
            String err = string.Empty;
            try
            {
                //Initialisation
                if (!this.Initialize(modeBkm, out err))
                    throw (new Exception("ePlanningItem.Initialize : " + err));
                //chargement des élements  Base --> objets .Net
                List<ePlanningItem> addedItems = new List<ePlanningItem>();
                if (!this.LoadItemInfos(out err, out addedItems))
                    throw (new Exception("ePlanningItem.LoadItems : " + err));
            }
            finally
            {
                _queryCal.CloseQuery();
            }
        }

        /// <summary>
        /// Constructeur pour la récupération pour une liste de FILEID (pour le mode mois et la possibilité de plusieur RDV en un en conflit)
        /// </summary>
        /// <param name="dal">Objet de connexion à la BDD XRM</param>
        /// <param name="pref">Preférences de l'utilisateur en cours</param>
        /// <param name="tab">Fichier Planning (descid)</param>
        /// <param name="listFileId">Liste de FileId des fiches pointée</param>
        public ePlanningItem(eudoDAL dal, ePref pref, Int32 tab, List<Int32> listFileId)
        {
            _nViewMode = CalendarViewMode.VIEW_UNDEFIED;
            _dal = dal;
            _pref = pref;
            _nTab = tab;
            _listFileId = listFileId;
            String err = string.Empty;
            //Initialisation
            if (!this.Initialize(false, out err))
                throw (new Exception("ePlanningItem.Initialize : " + err));
            //chargement des élements  Base --> objets .Net
            List<ePlanningItem> addedItems = new List<ePlanningItem>();
            if (!this.LoadItemInfos(out err, out addedItems))
                throw (new Exception("ePlanningItem.LoadItems : " + err));
        }
        /// <summary>
        /// Constructeur pour la récupération pour une liste de FILEID
        /// </summary>
        /// <param name="parentCal">RDV parent dans le cas de plusieurs RDV en un pour le mode mois</param>
        /// <param name="queryCal">RDV demandé</param>
        /// <param name="dtr">Conteneur des infos des données après chargement de la requete eudo query</param>
        /// <param name="pref">Objet pref de l'user en cours</param>
        /// <param name="tab">Fichier Planning (descid)</param>
        public ePlanningItem(ePlanning parentCal, EudoQuery.EudoQuery queryCal, DataTableReaderTuned dtr, ePref pref, Int32 tab)
        {
            _nViewMode = CalendarViewMode.VIEW_UNDEFIED;
            _parentCal = parentCal;
            _pref = pref;
            _nTab = tab;
            List<ePlanningItem> addedItems = new List<ePlanningItem>(); //UTILE pour le mode semaine seulement
            LoadItemInfos(queryCal, dtr, new List<int>(), out addedItems);
        }

        #endregion

        /// <summary>
        /// Initialise une requête de eudoquery pour une seule fiche
        /// </summary>
        /// <param name="modeBkm">if set to <c>true</c> [mode BKM].</param>
        /// <param name="err">The error.</param>
        /// <returns></returns>
        private bool Initialize(Boolean modeBkm, out string err)
        {
            err = string.Empty;
            //Chargement des preférences
            if (_nFileId > 0 && modeBkm)
            {
                _queryCal = eLibTools.GetEudoQuery(_pref, _nTab, ViewQuery.FILE);
            }
            else
            {
                _queryCal = eLibTools.GetEudoQuery(_pref, _nTab, ViewQuery.LIST);
            }

            if (_queryCal.GetError.Length > 0)
            {
                err = _queryCal.GetError;
                _queryCal.CloseQuery();
                return false;
            }
            _queryCal.SetModeCalendarGraph = true;   //mode graphique

            WhereCustom wcFileId = new WhereCustom("TPLID", Operator.OP_IN, (_nFileId > 0) ? _nFileId.ToString() : eLibTools.Join<Int32>(",", _listFileId));
            _queryCal.AddCustomFilter(wcFileId);

            _queryCal.SetListCol = _pref.GetPref(_nTab, ePrefConst.PREF_PREF.CALENDARCOL);

            _queryCal.LoadRequest();
            if (_queryCal.GetError.Length > 0)
            {
                err = _queryCal.GetError;
                _queryCal.CloseQuery();
                return false;
            }
            _queryCal.BuildRequest();
            if (_queryCal.GetError.Length > 0)
            {
                err = _queryCal.GetError;
                _queryCal.CloseQuery();
                return false;
            }
            //Création de l'objet ePlanning parent pour le mode fiche
            _parentCal = new ePlanning(_dal, Pref, _nTab, 0, 0, _queryCal);
            DateBeginDescid = _parentCal.DateBeginDescId;
            _dtr = _dal.Execute(new RqParam(_queryCal.EqQuery), out err);
            if (_queryCal.GetError.Length > 0)
            {
                err = _queryCal.GetError;
                _queryCal.CloseQuery();
                return false;
            }
            if (err.Length != 0)
            {
                _queryCal.CloseQuery();
                return false;
            }
            return true;
        }


        /// <summary>
        /// Charge les propriétés d'un ou plusieurs RDV de eudoquery
        /// </summary>
        /// <param name="err">si le retour est faux l'erreure est renseignées ici</param>
        /// <param name="addedItems">Récupérations des infos des RDV</param>
        /// <returns>si Vrai tout se passe bien</returns>
        private bool LoadItemInfos(out string err, out List<ePlanningItem> addedItems)
        {
            try
            {
                err = string.Empty;
                addedItems = new List<ePlanningItem>();
                if (!_dtr.Read())
                {
                    err = "Fiche introuvable";
                    return false;
                }
                else
                {
                    LoadItemInfos(_queryCal, _dtr, new List<Int32>(), out addedItems);

                    _listEPI.Add(this);
                    while (_dtr.Read())
                    {
                        _listEPI.Add(new ePlanningItem(_parentCal, _queryCal, _dtr, Pref, _nTab));
                    }
                }
            }
            finally
            {
                if (_dtr != null)
                    _dtr.Dispose();
            }
            return true;
        }

        /// <summary>
        /// Charge les propriétés d'un RDV
        /// </summary>
        /// <param name="queryCal">Objet eudoquery générant les requêtes XRM</param>
        /// <param name="dtr">Conteneur des infos des données après chargement de la requete eudo query</param>
        /// <param name="userDisplayed">Utilisateur affiché</param>
        /// <param name="addedItems">Liste des Item RDV retournés</param>
        private void LoadItemInfos(EudoQuery.EudoQuery queryCal, DataTableReaderTuned dtr, List<Int32> userDisplayed, out List<ePlanningItem> addedItems)
        {
            // Appel au départ de la fonction pour le chargement des Field de EudoQuery
            List<Field> lif = queryCal.GetFieldHeaderList;



            addedItems = new List<ePlanningItem>();
            List<eFieldRecord> fieldsToDisplay = new List<eFieldRecord>();

            eFieldRecord eFldRow = null;
            String boundFieldAlias = String.Empty, pdv = String.Empty;

            StringBuilder _sb = new StringBuilder();
            String sTabAlias = queryCal.GetMainTable.Alias;
            DateTime dBegin = DateTime.Now;
            DateTime dEnd = DateTime.Now;

            if (_parentCal.DateBeginDescId > 0)
                DateTime.TryParse(dtr.GetString(sTabAlias + "_" + _parentCal.DateBeginDescId), out dBegin);

            DateTime.TryParse(dtr.GetString(sTabAlias + "_" + _parentCal.DateEndDescId), out dEnd);

            // TODOHLA - Faire un datafillergeneric car pas toujours un eRecord !!
            _row = new eRecord();
            _row.FillComplemantaryInfos(queryCal, dtr, _pref);

            // TODOHLA - Optimiser les chargements de Field EudoQuery par de vrai objet !!
            string sMultiOwnerValue = dtr.GetString(sTabAlias + "_" + (_nTab + AllField.TPL_MULTI_OWNER.GetHashCode()));
            MultiOwnerId = sMultiOwnerValue;
            string sMultiOwnerFinalValue = dtr.GetString(sTabAlias + "_" + (_nTab + AllField.TPL_MULTI_OWNER.GetHashCode()) + "_A");

            string sListOwnerId = String.Empty;
            if (queryCal.GetQueryType != ViewQuery.FILE)
            {
                sListOwnerId = dtr.GetString(sTabAlias + "_" + (_nTab + AllField.TPL_MULTI_OWNER.GetHashCode()) + "_I");
            }


            string sOwnerValue = dtr.GetString(sTabAlias + "_" + (_nTab + AllField.OWNER_USER.GetHashCode()));
            OwnerId = sOwnerValue;
            string sOwnerFinalValue = dtr.GetString(sTabAlias + "_" + (_nTab + AllField.OWNER_USER.GetHashCode()) + "_A");
            string sVisibleBy = dtr.GetString(sTabAlias + "_" + (_nTab + AllField.USER_VISIBLE.GetHashCode()));

            //string sCalendarColor = String.Empty;
            if (queryCal.GetQueryType != ViewQuery.FILE)
            {
                _color = dtr.GetString(sTabAlias + "_" + (_nTab + PlanningField.DESCID_CALENDAR_COLOR.GetHashCode()));
            }
            _bConfidential = dtr.GetBoolean(sTabAlias + "_" + (_nTab + AllField.CONFIDENTIAL.GetHashCode()));
            bool bGroupNotAllowed = dtr.GetString("GROUP_" + sTabAlias) == "1";
            bool bGroupPublicNotAllowed = dtr.GetString("GROUP_PUBLIC_" + sTabAlias) == "1";
            bool bGroupPublicRestricted = false;
            ScheduleId = dtr.GetEudoNumeric(sTabAlias + "_" + (_nTab + PlanningField.DESCID_SCHEDULE_ID));

            if (dtr.IsColumnExists($"schedule_{_nTab}_masterfileid"))
                MasterFileId = (int)dtr.GetDecimal($"schedule_{_nTab}_masterfileid");

            // Appartient A
            bool bFileMainOwner = string.Concat(";", sOwnerValue, ";").Contains(String.Concat(";", _pref.User.UserId, ";"));

            // Me concerne
            bool bFileOwner = string.Concat(";", sOwnerValue, ";").Contains(String.Concat(";", _pref.User.UserId, ";"));
            bFileOwner = bFileOwner || string.Concat(";", sMultiOwnerValue, ";").Contains(String.Concat(";", _pref.User.UserId, ";"));
            bFileOwner = bFileOwner || string.Concat(";", sMultiOwnerValue, ";").Contains(String.Concat(";G", _pref.User.UserGroupId, ";"));

            // Planning visible
            bool bFileVisible = string.Concat(";", sVisibleBy, ";").Contains(String.Concat(";", _pref.User.UserId, ";"));
            bFileVisible = bFileVisible || string.Concat(";", sVisibleBy, ";").Contains(String.Concat(";G", _pref.User.UserGroupId, ";"));

            _nFileId = dtr.GetEudoNumeric(sTabAlias + "_ID");

            //if (lif.Exists(delegate (Field f) { return (f.Descid % 100 == (int)PlanningField.DESCID_CALENDAR_ITEM); }))
            //todo : vérifier pourquoi le DESCID_CALENDAR_ITEM n'est pas tjs ajouté et/ou pourquoi on ne fait pas le addfieldinheader sur le add, ce qui permetrait de 
            // le tester par la suite (cf request.cs.AddFieldPlanning)
            try
            {
                _typ = (PlanningType)dtr.GetEudoNumeric(sTabAlias + "_" + (_nTab + PlanningField.DESCID_CALENDAR_ITEM));
            }
            catch
            {
                _typ = PlanningType.CALENDAR_ITEM_TASK;
            }

            bool bDispAppointment = _typ != PlanningType.CALENDAR_ITEM_TASK;
            bool bAllDay = _typ == PlanningType.CALENDAR_ITEM_EVENT;

            if (_bConfidential && !bFileOwner && !bFileVisible && !bFileMainOwner)
            {
                _sb.Append(eResApp.GetRes(_pref, 1240));
            }
            else
            {
                if (bGroupNotAllowed && bGroupPublicNotAllowed &&
                    (Pref.GroupMode == SECURITY_GROUP.GROUP_EXCLUDING_READONLY || Pref.GroupMode == SECURITY_GROUP.GROUP_EXCLUDING))
                {
                    _sb.Append(eResApp.GetRes(_pref, 916)).Append("<br> ").Append(sOwnerFinalValue).Append("<br>").Append(sMultiOwnerFinalValue);
                    bGroupPublicRestricted = true;
                }
                else
                {
                    foreach (Field subFld in lif)
                    {
                        if (subFld.Descid == queryCal.GetDateDescId)// _nTab + PlanningField.DESCID_TPL_BEGIN_TIME.GetHashCode())
                        {
                            if (!eLibTools.AllowedView(subFld, dtr, Pref.GroupMode)
                                || !eLibTools.AllowedChange(subFld, dtr, Pref.GroupMode))
                            {
                                _isBottomResizeEnabled = false;
                                _isMoveEnabled = false;
                            }
                        }

                        if (!subFld.DrawField || subFld.Format == FieldFormat.TYP_BIT)
                            continue;



                        eFldRow = eDataFillerTools.GetFieldRecord(_pref, queryCal, dtr, subFld, _row, _pref.User, true, _parentCal?._dal);

                        if (eFldRow == null)
                            continue;

                        if (eFldRow.DisplayValue.Trim().Length > 0)
                        {

                            string sval = eFldRow.DisplayValue;

                            //on préserve le &lt; pour éviter le double encodage : &lt; => &amp;lt;
                            sval = sval.Replace("&lt;", "[[lt]]");
                            sval = sval.Replace("&gt;", "[[gt]]");
                            sval = sval.Replace("&quot;", "[[quot]]");
                            sval = HttpUtility.HtmlEncode(sval);
                            sval = sval.Replace("[[lt]]", "&lt;");
                            sval = sval.Replace("[[gt]]", "&gt;");
                            sval = sval.Replace("[[quot]]", "&quot;");


                            _sb.Append(sval).Append("<br>");

                        }

                        fieldsToDisplay.Add(eFldRow);
                    }
                }
            }

            if (bAllDay)
            {
                dBegin = new DateTime(dBegin.Year, dBegin.Month, dBegin.Day, 0, 0, 0);
                dEnd = new DateTime(dEnd.Year, dEnd.Month, dEnd.Day, 23, 59, 0);
                _isMoveEnabled = false;
                _isBottomResizeEnabled = false;
                _isTopResizeEnabled = false;
            }


            if ((ScheduleId > 0) && (queryCal.GetQueryType != ViewQuery.FILE))
            {
                string scheduleLabel = "";

                try
                {
                    scheduleLabel = eScheduleInfos.GetScheduleLabel(queryCal.GetMainTable.DescId, dtr, Pref);
                }
                catch { }
                _scheduleDiv = new HtmlGenericControl("div");
                _scheduleDiv.Attributes.Add("title", scheduleLabel);
                _scheduleDiv.Attributes.Add("class", "icon-reload");
                _isMoveEnabled = false;
            }

            //Gestion des couleurs selon l'appartenance
            string gripColor = "blue";

            if (_bConfidential && !bFileOwner && !bFileVisible)
            {
                gripColor = _parentCal.GripConfidentialColor;
                _color = _parentCal.GripOtherConfidentialColor;
                _isViewable = false;
                _isTopResizeEnabled = false;
                _isBottomResizeEnabled = false;
                _isMoveEnabled = false;
            }
            else if (bGroupPublicRestricted)
            {
                gripColor = _parentCal.GripOtherConfidentialColor;
                _isViewable = false;
                _isTopResizeEnabled = false;
                _isBottomResizeEnabled = false;
                _isMoveEnabled = false;
            }
            else if (_bConfidential)
                gripColor = _parentCal.GripConfidentialColor;
            else if (bFileMainOwner)
                gripColor = _parentCal.GripUserOwnerColor;
            else if (bFileOwner)
                gripColor = _parentCal.GripMultiOwnerColor;
            else
                gripColor = _parentCal.GripPublicColor;

            if ((_parentCal.ViewMode == CalendarViewMode.VIEW_CAL_MONTH) && (String.IsNullOrEmpty(_color)))
                _color = eConst.COL_CAL_WHITE;  //Pour le mode mois par défaut on utilise la couleurs de la constante

            if ((_parentCal.ViewMode == CalendarViewMode.VIEW_CAL_DAY_PER_USER || _parentCal.ViewMode == CalendarViewMode.VIEW_CAL_MONTH || _parentCal.ViewMode == CalendarViewMode.VIEW_CAL_DAY) && (queryCal.GetQueryType != ViewQuery.FILE) && (userDisplayed.Count > 0) && (_nViewMode != CalendarViewMode.VIEW_UNDEFIED))
            {
                String[] aListOwnerId = sListOwnerId.Split(";");

                for (int i = 0; i < aListOwnerId.Length; i++)
                {
                    int concernedUser = eLibTools.GetNum(aListOwnerId[i]);
                    if (userDisplayed.Contains(concernedUser))
                    {
                        ePlanningItem itmTmp = new ePlanningItem();
                        //Schedule div
                        if (_scheduleDiv != null)
                        {
                            HtmlGenericControl tmpDiv = new HtmlGenericControl("div");
                            tmpDiv.Attributes.Add("title", _scheduleDiv.Attributes["title"]);
                            tmpDiv.Attributes.Add("class", "icon-reload");
                            itmTmp._scheduleDiv = tmpDiv;
                        }
                        itmTmp.IsMovable = _isMoveEnabled;
                        itmTmp.IsTopResizeEnabled = _isTopResizeEnabled;
                        itmTmp.IsBottomResizeEnabled = _isBottomResizeEnabled;
                        itmTmp._nTab = _nTab;
                        itmTmp._bConfidential = this._bConfidential;
                        itmTmp.InitPlanningItem(_nFileId, dBegin, dEnd, _sb.ToString(), _typ, _row, fieldsToDisplay, null, _color, gripColor, concernedUser, Pref);

                        if (_parentCal.ViewMode == CalendarViewMode.VIEW_CAL_MONTH)
                        {
                            itmTmp.OwnerId = OwnerId;
                            itmTmp.MultiOwnerId = MultiOwnerId;
                        }

                        // HLA - Le SetPosition est lancé par la suite par ePlanning.LoadItems Cela evite d'avoir 2 fois les div gt_ , gb_ et gl_
                        //itmTmp.SetPosition(_parentCal);
                        addedItems.Add(itmTmp);
                    }
                }
            }
            else
            {
                //mode semaine uniquement
                InitPlanningItem(_nFileId, dBegin, dEnd, _sb.ToString(), _typ, _row, fieldsToDisplay, null, _color, gripColor, Pref.User.UserId, Pref);
                addedItems.Add(this);
            }
        }

        /// <summary>
        /// Initilise un éléments planning
        /// </summary>
        private void InitPlanningItem(Int32 tplId, DateTime dBegin, DateTime dEnd, String innerHtml, PlanningType typ, eRecord row, List<eFieldRecord> fieldsToDisplay, String toolTipInfos, string color, string gripColor, int concernedUser, ePref pref)
        {
            _nFileId = tplId;
            _dateBegin = dBegin;
            _dateEnd = dEnd;
            _innerHtml = innerHtml;
            _typ = typ;
            _fieldsToDisplay = fieldsToDisplay;
            _color = string.IsNullOrEmpty(color) || string.IsNullOrEmpty(color.Trim()) ? "#ffffff" : color.Trim();
            _concernedUserId = concernedUser;
            _gripColor = string.IsNullOrEmpty(gripColor) ? "blue" : (string.IsNullOrEmpty(gripColor.Trim()) ? "blue" : gripColor);
            _divElement = new HtmlGenericControl("div");

            // schedule

            if (_scheduleDiv != null)
                _divElement.Controls.Add(_scheduleDiv);

            _divElement.Style.Add(HtmlTextWriterStyle.BackgroundColor, _color);
            _divElement.Attributes.Add("clr", _color);

            _divElement.Attributes.Add("class", "calElm");
            if (pref.Context.CuttedItems.ContainsKey(_nTab) && pref.Context.CuttedItems[_nTab] == FileId)
                _divElement.Attributes["class"] += " calElmCutted";
            if (pref.Context.CopiedItems.ContainsKey(_nTab) && pref.Context.CopiedItems[_nTab] == FileId)
                _divElement.Attributes["class"] += " calElmCopied";

            _divElement.Attributes.Add("sel", "0");
            //_divElement.Attributes.Add("onclick", "OIC(event,this)");
            //_divElement.Attributes.Add("onclick", "onCalClick(event)");

            if (ScheduleId > 0)
            {
               
                _divElement.Attributes.Add("ondblclick", string.Concat("selectOpenSeries('", _nTab, "','", _nFileId, "','", eResApp.GetRes(pref, 151), $"',{MasterFileId})"));
            }
            else
                _divElement.Attributes.Add("ondblclick", string.Concat("showTpl(", _nFileId, ");"));
            //_divElement.Attributes.Add("ondblclick", string.Concat("showTplPlanning('", _nTab, "','", _nFileId, "',null,'", eResApp.GetRes(pref.Lang, 151), "');"));


            _divElement.Attributes.Add("oncontextmenu", "showttid(this,event);");

            _divElement.ID = String.Concat(concernedUser, "_", FileId, "_", _itemRange);

            //ajout du rste du contenu
            HtmlGenericControl divContent = new HtmlGenericControl("div");
            divContent.Attributes.Add("class", "elmCnt");
            divContent.Attributes.Add("eCalCnt", "1");

            string[] parts = innerHtml.Split("<br>");
            foreach (var part in parts)
                divContent.InnerHtml += (part) + "<br>";



            _divElement.Controls.Add(divContent); ;

            if (toolTipInfos != null)
            {
                _toolTipInfos = toolTipInfos.ToString();
                _divElement.Attributes.Add("tti", HttpUtility.HtmlAttributeEncode(_toolTipInfos));
            }
            _divElement.Attributes.Add("fid", _nFileId.ToString());
            _divElement.Attributes.Add("eCf", this._bConfidential ? "1" : "0");
        }

        /// <summary>
        /// Teste de chevauchement
        /// </summary>
        /// <param name="itm">Rendez-vous a tester</param>
        /// <returns>Vrai si une autre RDV le chevauche</returns>
        public bool IsOverLapWith(ePlanningItem itm)
        {
            if (this._concernedUserId != itm._concernedUserId)
                return false;

            return ((this.DateBegin >= itm.DateBegin && this.DateBegin < itm.DateEnd) || (this.DateEnd > itm.DateBegin && this.DateEnd <= itm.DateEnd)) || (this.DateBegin < itm.DateEnd && this.DateEnd > itm.DateBegin);
        }

        /// <summary>
        /// Retourne le rendu TOOLTIP à afficher
        /// </summary>
        /// <param name="pref">Objet pref pour la langue des res à afficher</param>
        /// <param name="divId">Id du RDV appelant</param>
        /// <returns></returns>
        public Control GetToolTipRenderer(ePref pref, string divId)
        {
            Panel ctrl = new Panel();

            #region entete de titre
            System.Web.UI.WebControls.Table TabTitle = new System.Web.UI.WebControls.Table();
            TabTitle.CssClass = "pl_tt_head_table";
            TabTitle.CellPadding = 0;
            TabTitle.CellSpacing = 0;

            TableRow mainTabTR = new TableRow();
            TabTitle.Controls.Add(mainTabTR);

            TableCell mainTabTDgl = new TableCell();
            mainTabTR.Controls.Add(mainTabTDgl);
            mainTabTDgl.CssClass = "pl_tt_gl";

            TableCell mainTabTD = new TableCell();
            mainTabTR.Controls.Add(mainTabTD);

            mainTabTD.Text = eResApp.GetRes(pref, 6288);

            mainTabTD = new TableCell();
            mainTabTR.Controls.Add(mainTabTD);
            /*  Croix FERMER*/
            Label titleSpan = new Label();
            mainTabTD.Controls.Add(titleSpan);
            titleSpan.Text = "&nbsp;";
            titleSpan.CssClass = "icon-close icnPlanTlt";
            titleSpan.Attributes.Add("onclick", "top['_eToolTipModal'].hide()");
            /*******/

            ctrl.Controls.Add(TabTitle);
            #endregion

            #region Affichage des rubriques et de leur valeurs

            Panel ctrlVal = new Panel();
            ctrlVal.CssClass = "pl_tt_mid_tab pl_tt_fields";

            Boolean bUpdatable = false;
            int counter = 0;
            int countUpdatable = _listEPI.Where(epi => epi._row.RightIsUpdatable && epi._isViewable).Count();
            foreach (ePlanningItem currentEPI in _listEPI)
            {
                if (counter > 0)
                    ctrlVal.Controls.Add(new HtmlGenericControl("HR"));
                else
                {
                    TabTitle.Style.Add("background-color", currentEPI.Color);
                    mainTabTDgl.Style.Add("background-color", currentEPI.GripColor);
                }
                GetToolTipRDVRender(ctrlVal, currentEPI);
                bUpdatable = currentEPI._row.RightIsUpdatable && currentEPI._isViewable;

                if (_parentCal.ViewMode == CalendarViewMode.VIEW_CAL_MONTH && bUpdatable && countUpdatable > 1)
                {
                    ctrlVal.Controls.Add(GetToolTipButtonsRenderer(pref, currentEPI, divId));
                }

                counter++;
            }

            ctrl.Controls.Add(ctrlVal);
            #endregion

            //Les boutons de modif et autres n'apparraissent que lorsque la fiche est modifiable
            if ((_parentCal.ViewMode != CalendarViewMode.VIEW_CAL_MONTH || (_parentCal.ViewMode == CalendarViewMode.VIEW_CAL_MONTH && countUpdatable <= 1)) && bUpdatable)
            {
                ctrl.Controls.Add(GetToolTipButtonsRenderer(pref, this, divId));
            }
            else
                ctrlVal.Height = Unit.Pixel(200);

            return ctrl;
        }

        private Control GetToolTipButtonsRenderer(ePref pref, ePlanningItem item, string divId)
        {
            #region Ajout des boutons du bas
            Panel ctrlBtn = new Panel();



            System.Web.UI.WebControls.Table ctrlTableBtn = new System.Web.UI.WebControls.Table();
            ctrlTableBtn.CssClass = "pl_tt_bot_icon";

            TableRow ctrlTRBtn = new TableRow();

            TableCell ctrlTCBtn = new TableCell();


            ctrlTCBtn = new TableCell();
            ctrlTCBtn.Style.Add("width", "9px");
            ctrlTRBtn.Controls.Add(ctrlTCBtn);

            #region Bouton Supprimer
            ctrlTCBtn = new TableCell();
            //ctrlTCBtn.Text = "&nbsp;";
            if (item._row.RightIsUpdatable)
            {
                ctrlTCBtn.CssClass = "icon-delete";
                //ctrlTCBtn.Controls.Add(link);
                ctrlTCBtn.Attributes.Add("title", eResApp.GetRes(pref, 19)); // Couper
                ctrlTCBtn.Attributes.Add("onclick", string.Concat("parent.deleteCalByToolTip(", item._nTab, ",", item._nFileId, ",'" + divId + "');"));
            }
            else
                ctrlTCBtn.CssClass = "icon-delete disable";
            //link.Text = " ";
            ctrlTRBtn.Controls.Add(ctrlTCBtn);
            #endregion

            #region Bouton Imprimer
            /*
            ctrlTCBtn = new TableCell();
            ctrlTCBtn.CssClass = "pl_tt_opt_print";
            ctrlTCBtn.Attributes.Add("title", eResApp.GetRes(pref, 13)); // Couper
            ctrlTCBtn.Attributes.Add("onclick", "parent.printTpl('" + divId + "');");                
            ctrlTRBtn.Controls.Add(ctrlTCBtn);
             * */
            #endregion


            #region Bouton Déplacer
            ctrlTCBtn = new TableCell();
            //ctrlTCBtn.Text = "&nbsp;";
            if (item._row.RightIsUpdatable)
            {

                ctrlTCBtn.CssClass = "icon-deplacer-vers";
                ctrlTCBtn.Attributes.Add("title", eResApp.GetRes(pref, 6794)); // Déplacer vers une autre date

                if (item._parentCal.ViewMode != CalendarViewMode.VIEW_CAL_MONTH)
                    ctrlTCBtn.Attributes.Add("onclick", "parent.moveDuplTpl(false, '" + divId + "');");
                else
                    ctrlTCBtn.Attributes.Add("onclick", "parent.moveDuplTpl(false, '" + item._nFileId + "', true);");
            }
            else
            {

                ctrlTCBtn.CssClass = "icon-deplacer-vers disable";
            }
            //link.Text = " ";
            ctrlTRBtn.Controls.Add(ctrlTCBtn);
            #endregion

            #region Bouton Dupliquer
            ctrlTCBtn = new TableCell();
            //ctrlTCBtn.Text = "&nbsp;";
            if (item._row.RightIsUpdatable)
            {
                ctrlTCBtn.CssClass = "icon-copier-vers";
                ctrlTCBtn.Attributes.Add("title", eResApp.GetRes(pref, 6793)); // Copier (dupliquer) vers une autre date
                if (item._parentCal.ViewMode != CalendarViewMode.VIEW_CAL_MONTH)
                    ctrlTCBtn.Attributes.Add("onclick", "parent.moveDuplTpl(true, '" + divId + "');");
                else
                    ctrlTCBtn.Attributes.Add("onclick", "parent.moveDuplTpl(true, '" + item._nFileId + "', true);");
            }
            else
            {
                ctrlTCBtn.CssClass = "icon-copier-vers disable";
            }
            //link.Text = " ";
            ctrlTRBtn.Controls.Add(ctrlTCBtn);
            #endregion

            //Les boutons de couper et copier n'apparraissent pas pour le mode mois
            if (item._parentCal.ViewMode != CalendarViewMode.VIEW_CAL_MONTH)
            {
                #region Bouton Copier
                //link = new HyperLink();
                //link.Attributes.Add("href", "#");
                ctrlTCBtn = new TableCell();
                ctrlTCBtn.Text = "&nbsp;";
                if (item._row.RightIsUpdatable)
                {
                    //ctrlTCBtn.Controls.Add(link);
                    ctrlTCBtn.CssClass = "icon-duplicate";


                    if (item._parentCal.ViewMode == CalendarViewMode.VIEW_CAL_DAY_PER_USER && item._parentCal.UserDisplayed.Count > 1)
                    {
                        ctrlTCBtn.Attributes.Add("title", eResApp.GetRes(pref, 8187));
                        ctrlTCBtn.Style.Add(HtmlTextWriterStyle.Cursor, "not-allowed");
                    }
                    else
                    {
                        ctrlTCBtn.Attributes.Add("title", eResApp.GetRes(pref, 1387)); // Couper
                        ctrlTCBtn.Attributes.Add("onclick", "parent.copyTpl('" + divId + "');");
                    }

                }
                else
                    ctrlTCBtn.CssClass = "icon-duplicate disable";
                //link.Text = " ";
                ctrlTRBtn.Controls.Add(ctrlTCBtn);
                #endregion

                #region Bouton Couper
                /*link = new HyperLink();
                link.Attributes.Add("href", "#");*/
                ctrlTCBtn = new TableCell();
                if (item._row.RightIsUpdatable)
                {
                    ctrlTCBtn.CssClass = "icon-cut";
                    ctrlTCBtn.Text = "&nbsp;";
                    //ctrlTCBtn.Controls.Add(link);
                    //HDJ RDV récurent, bloquer le couper coller
                    if (item.ScheduleId > 0 || (item._parentCal.ViewMode == CalendarViewMode.VIEW_CAL_DAY_PER_USER && item._parentCal.UserDisplayed.Count > 1))
                    {
                        ctrlTCBtn.Attributes.Add("title", eResApp.GetRes(pref, 6375));
                        ctrlTCBtn.Style.Add(HtmlTextWriterStyle.Cursor, "not-allowed");
                    }
                    else
                    {
                        ctrlTCBtn.Attributes.Add("title", eResApp.GetRes(pref, 1389)); // Couper
                        ctrlTCBtn.Attributes.Add("onclick", "parent.cutTpl('" + divId + "');");
                    }
                }
                else
                    ctrlTCBtn.CssClass = "icon-cut disable";
                //link.Text = " ";
                ctrlTRBtn.Controls.Add(ctrlTCBtn);
                #endregion
            }

            #region Bouton Modifier
            ctrlTCBtn = new TableCell();
            if (item._row.RightIsUpdatable)
            {
                //ctrlTCBtn.Controls.Add(link);
                
                ctrlTCBtn.CssClass = "icon-edn-pen";
                //ctrlTCBtn.Text = "&nbsp;";
                //HDJ Ajouter pour bloquer le couper coller dans une occurence récurrente
                if (ScheduleId > 0)
                {                    
                    ctrlTCBtn.Attributes.Add("onclick", string.Concat("parent.selectOpenSeries('", item._nTab, "','", item._nFileId, "','", eResApp.GetRes(pref, 151), $"',{MasterFileId});"));
                }
                else
                    ctrlTCBtn.Attributes.Add("onclick", string.Concat("parent.showTpl(", item._nFileId, ");"));
                ctrlTCBtn.Attributes.Add("title", eResApp.GetRes(pref, 151)); // Modifier
            }
            else
                ctrlTCBtn.CssClass = "icon-edn-pen disable";
            //link.Text = " ";
            ctrlTRBtn.Controls.Add(ctrlTCBtn);
            #endregion


            ctrlTCBtn = new TableCell();
            ctrlTCBtn.Style.Add("width", "9px");
            ctrlTRBtn.Controls.Add(ctrlTCBtn);


            ctrlTableBtn.Controls.Add(ctrlTRBtn);

            ctrlBtn.Controls.Add(ctrlTableBtn);
            #endregion

            return ctrlBtn;
        }

        /// <summary>
        /// Contenu intérieure du ToolTip pour un RDV
        /// </summary>
        /// <param name="ctrlVal">Div conteneur</param>
        /// <param name="ePI">RDV à afficher</param>
        private void GetToolTipRDVRender(Panel ctrlVal, ePlanningItem ePI)
        {
            int i = 0;
            System.Web.UI.WebControls.Table ctrlTableval;
            TableRow ctrlTRval;
            TableCell ctrlTCval;

            ctrlTableval = new System.Web.UI.WebControls.Table();
            ctrlTableval.CssClass = "pl_tt_content_table";

            if (ePI._parentCal.ViewMode == CalendarViewMode.VIEW_CAL_MONTH)
            {
                ctrlTRval = new TableRow();
                ctrlTableval.Controls.Add(ctrlTRval);
                ctrlTCval = new TableCell();
                ctrlTCval.ColumnSpan = 2;
                if (ePI.DateBegin.ToString("dd/MM/yyyy") == ePI.DateEnd.ToString("dd/MM/yyyy"))   //si même jours on affiche que l'heure de début et de fin sinon on affiche la date complète
                    ctrlTCval.Text = String.Concat(ePI.DateBegin.ToString("HH:mm"), " - ", ePI.DateEnd.ToString("HH:mm"));
                else
                    ctrlTCval.Text = String.Concat(ePI.DateBegin.ToString("dd/MM/yyyy HH:mm"), " - ", ePI.DateEnd.ToString("dd/MM/yyyy HH:mm"));

                ctrlTCval.Attributes.Add("class", "pl_tt_flblM");
                ctrlTRval.Controls.Add(ctrlTCval);
            }
            if (!ePI._isViewable)
            {
                ctrlTRval = new TableRow();
                ctrlTableval.Controls.Add(ctrlTRval);
                ctrlTCval = new TableCell();
                ctrlTRval.Controls.Add(ctrlTCval);
                //Si RDV confidentiel, on affiche le libellé Confidentiel mais n'affiche pas les libellé ni les boutons de traitements.
                ctrlTCval.Text = eResApp.GetRes(_pref, 5083);    //5083 : Confidentiel;

            }
            else
                foreach (eFieldRecord fieldsDisplayed in ePI._fieldsToDisplay)
                {
                    //Ne pas afficher les champs vides.
                    if (String.IsNullOrEmpty(fieldsDisplayed.Value))
                        continue;

                    #region libellé du champ
                    ctrlTRval = new TableRow();
                    ctrlTCval = new TableCell();
                    ctrlTCval.Text = (String.IsNullOrEmpty(fieldsDisplayed.FldInfo.Libelle)) ? " " : String.Concat(HttpUtility.HtmlEncode(fieldsDisplayed.FldInfo.Libelle), " :");
                    ctrlTCval.Attributes.Add("class", "pl_tt_flbl");
                    ctrlTRval.Controls.Add(ctrlTCval);
                    #endregion

                    #region Rendu par type de champs
                    ctrlTCval = new TableCell();

                    HtmlGenericControl hgcInField;

                    // Suivant le format du champ,
                    // récupération de l'action associée au champ et des valeurs réelles/affichables du champ
                    switch (fieldsDisplayed.FldInfo.Format)
                    {
                        #region TYP MEMO
                        case FieldFormat.TYP_MEMO:
                            //ouvrir le popup de champs memo
                            hgcInField = new HtmlGenericControl("a");
                            hgcInField.InnerText = HttpUtility.HtmlDecode(fieldsDisplayed.DisplayValue);

                            hgcInField.ID = eTools.GetFieldValueCellId(_row, fieldsDisplayed);
                            String colName = eTools.GetFieldValueCellName(_row, fieldsDisplayed);
                            hgcInField.Attributes.Add("ename", colName);

                            if (fieldsDisplayed.RightIsUpdatable)
                            {
                                hgcInField.Attributes.Add("class", "pl_tt_txt_link");
                                hgcInField.Attributes.Add("eaction", "LNKOPENMEMO");
                                hgcInField.Attributes.Add("html", fieldsDisplayed.FldInfo.IsHtml ? "1" : "0");
                                hgcInField.Attributes.Add("efld", "1");
                                hgcInField.Attributes.Add("href", string.Concat("javascript:parent.PlShMemo(document.getElementById('", hgcInField.ID, "'));"));

                                ctrlTCval.ID = colName;
                                ctrlTCval.Attributes.Add("did", fieldsDisplayed.FldInfo.Descid.ToString());
                                ctrlTCval.Attributes.Add("lib", fieldsDisplayed.FldInfo.Libelle);
                            }

                            ctrlTCval.Controls.Add(hgcInField);
                            break;
                        #endregion

                        #region TYPE CHAR
                        case FieldFormat.TYP_CHAR:
                            //S'il s'agit d'un champ principal de PP, CONTACT ou EVENT, on ajoute un liens vers la fiche

                            Int32 nTargentTab = (fieldsDisplayed.FldInfo.Popup == PopupType.SPECIAL && fieldsDisplayed.FldInfo.PopupDescId > 0)
                                ? eLibTools.GetTabFromDescId(fieldsDisplayed.FldInfo.PopupDescId) : fieldsDisplayed.FldInfo.Table.DescId;

                            if ((fieldsDisplayed.FileId > 0)
                                &&
                                (
                                    ((fieldsDisplayed.FldInfo.Table.TabType == EudoQuery.TableType.EVENT
                                    && fieldsDisplayed.FldInfo.Descid == fieldsDisplayed.FldInfo.Table.MainFieldDescId)
                                    || fieldsDisplayed.FldInfo.Descid == 301
                                    )
                                ||
                                    (
                                    fieldsDisplayed.FldInfo.Popup == PopupType.SPECIAL && fieldsDisplayed.FldInfo.PopupDescId > 0 && fieldsDisplayed.FldInfo.PopupDescId != 201
                                    )

                                )

                                )
                            {

                                hgcInField = new HtmlGenericControl("a");

                                hgcInField.InnerHtml = eLibTools.RemoveHTML((String.IsNullOrEmpty(fieldsDisplayed.DisplayValue)) ? " " : HttpUtility.HtmlEncode(fieldsDisplayed.DisplayValue));

                                //CNA - Affichage Minifiche
                                string error = string.Empty;
                                if (eFilemapPartner.IsMiniFileMappingEnabled(_pref, nTargentTab, out error) && String.IsNullOrEmpty(error))
                                {
                                    hgcInField.Attributes.Add("vcMiniFileTab", nTargentTab.ToString());
                                    hgcInField.Attributes.Add("onmouseover",
                                            String.Concat("javascript:parent.showvcpl(this, 1, ", fieldsDisplayed.FileId, ");"));
                                    hgcInField.Attributes.Add("onmouseout",
                                        String.Concat("javascript:parent.showvcpl(this, 0, ", fieldsDisplayed.FileId, ");"));
                                }

                                if (!String.IsNullOrEmpty(fieldsDisplayed.DisplayValue))
                                {
                                    hgcInField.Attributes.Add("class", "pl_tt_txt_link");
                                    hgcInField.Attributes.Add("href",
                                        String.Concat("javascript:parent.PlLoadFile(false,", nTargentTab, ", ", eTools.GetLnkId(fieldsDisplayed), ");"));
                                }


                                ctrlTCval.Controls.Add(hgcInField);
                            }
                            else
                                if (fieldsDisplayed.FileId > 0
                                    &&
                                        (
                                        fieldsDisplayed.FldInfo.Descid == 201
                                        ||
                                        (fieldsDisplayed.FldInfo.Popup == PopupType.SPECIAL && fieldsDisplayed.FldInfo.PopupDescId > 0)
                                        )
                                    )
                            {
                                hgcInField = new HtmlGenericControl("a");

                                hgcInField.InnerHtml = eLibTools.RemoveHTML((String.IsNullOrEmpty(fieldsDisplayed.DisplayValue)) ? " " : HttpUtility.HtmlEncode(fieldsDisplayed.DisplayValue));

                                Boolean bVCARD = false;
                                bVCARD = ((!String.IsNullOrEmpty(fieldsDisplayed.DisplayValue)) && (!String.IsNullOrEmpty(
                                    _pref.GetConfigDefault(new eLibConst.CONFIG_DEFAULT[] { eLibConst.CONFIG_DEFAULT.VCARDMAPPING })[eLibConst.CONFIG_DEFAULT.VCARDMAPPING]
                                    )));
                                if (bVCARD)
                                {
                                    //over sur contact : ouvre la VCARD
                                    hgcInField.Attributes.Add("class", "pl_tt_txt_link");
                                    hgcInField.Attributes.Add("onmouseover",
                                        String.Concat("javascript:parent.showvcpl(this, 1, ", fieldsDisplayed.FileId, ");"));
                                    hgcInField.Attributes.Add("onmouseout",
                                        String.Concat("javascript:parent.showvcpl(this, 0, ", fieldsDisplayed.FileId, ");"));
                                }


                                //Clique sur contact : redirection vers fiche ferme la VCARD et ferme la ToolTip
                                hgcInField.Attributes.Add("href",
                                    String.Concat("javascript:parent.PlLoadFile(", bVCARD ? "true" : "false", ",", nTargentTab, ", ", eTools.GetLnkId(fieldsDisplayed), ");"));

                                ctrlTCval.Controls.Add(hgcInField);
                            }
                            else
                                ctrlTCval.Text = (String.IsNullOrEmpty(fieldsDisplayed.DisplayValue)) ? " " : HttpUtility.HtmlEncode(fieldsDisplayed.DisplayValue);
                            break;
                        #endregion
                        #region TYP EMAIL - TODO    //TODO : aller au mode MAIL
                        case FieldFormat.TYP_EMAIL:
                            //Ajout d'un liens de type mailto pour les e-mails
                            hgcInField = new HtmlGenericControl("a");

                            hgcInField.InnerHtml = eLibTools.RemoveHTML((String.IsNullOrEmpty(fieldsDisplayed.DisplayValue)) ? " " : HttpUtility.HtmlEncode(fieldsDisplayed.DisplayValue));
                            if (!String.IsNullOrEmpty(fieldsDisplayed.DisplayValue))
                            {
                                hgcInField.Attributes.Add("class", "pl_tt_txt_link");
                                hgcInField.Attributes.Add("href", string.Concat("javascript:alert('mailto:", fieldsDisplayed.DisplayValue.Replace("'", "\\'"), "');"));
                                //hgcInField.Attributes.Add("href", string.Concat("javascript:parent.shFileInPopup(", fieldsDisplayed.FldInfo.Descid, ",0,'TODO - Titre du fichier', null, null, 1);"));
                                //TODO : ouvrir popup de création de mail mais si plusieurs template mail proposer d'en sélectionner un avant d'envoyer un mail
                            }

                            ctrlTCval.Controls.Add(hgcInField);
                            break;
                        #endregion
                        #region TYP FILE - OK
                        case FieldFormat.TYP_FILE:
                        #endregion
                        #region TYP NUMERIC - OK
                        case FieldFormat.TYP_AUTOINC:
                        case FieldFormat.TYP_MONEY:
                        case FieldFormat.TYP_NUMERIC:
                        #endregion
                        #region TYPE PHONE - OK
                        case FieldFormat.TYP_PHONE:
                        #endregion
                        #region TYP DATE - OK
                        case FieldFormat.TYP_DATE:
                        #endregion
                        #region TYP BIT - OK
                        case FieldFormat.TYP_BIT:
                        #endregion
                        #region TYP WEB - OK
                        case FieldFormat.TYP_WEB:
                        #endregion
                        #region TYP SOCIALNETWORK - OK
                        case FieldFormat.TYP_SOCIALNETWORK:
                        #endregion
                        #region TYP IFRAME - OK
                        case FieldFormat.TYP_IFRAME:
                        #endregion
                        #region TYP CHART - OK
                        case FieldFormat.TYP_CHART:
                        #endregion
                        #region TYP USER/GROUP - OK
                        case FieldFormat.TYP_USER:
                        case FieldFormat.TYP_GROUP:
                        #endregion
                        #region TYP IMAGE
                        case FieldFormat.TYP_IMAGE:
                        #endregion
                        #region TYP AUTRE  - OK
                        default:
                            //On affiche le texte sans possibilité d'action particulière
                            ctrlTCval.Text = String.IsNullOrEmpty(fieldsDisplayed.DisplayValue) ? " " : fieldsDisplayed.DisplayValue;
                            break;

                            #endregion
                    }
                    ctrlTRval.Controls.Add(ctrlTCval);
                    #endregion

                    ctrlTableval.Controls.Add(ctrlTRval);
                    i++;
                }

            ctrlVal.Controls.Add(ctrlTableval);
        }

        /// <summary>
        /// Recherche le jour du rendez-vous
        /// </summary>
        /// <param name="parentCal">Objet Calendrier jours dont il est issu</param>
        internal void SetPosition(ePlanning parentCal)
        {

            //Rechercher le jour

            foreach (ePlanningDay day in parentCal.Days)
            {
                if (day.DayDate.Date == this._dateBegin.Date && day.ConcernedUserId == this._concernedUserId)
                {
                    //heures début et fin en dehors des plages affichées
                    bool bNotInPlage =
                        this.DateBegin >= day.DayDate + parentCal.ViewHourEnd && this.DateEnd >= day.DayDate + parentCal.ViewHourEnd ||
                        this.DateBegin <= day.DayDate + parentCal.ViewHourBegin && this.DateEnd <= day.DayDate + parentCal.ViewHourBegin;


                    if (!bNotInPlage && !day.Items.Contains(this))
                    {
                        this._parentDay = day;
                        day.Items.Add(this);
                    }

                    break;
                }
            }

            if (_parentDay == null)
                return;
            //Taille et position

            if (
                ((this._parentCal != null)
                && (this._parentCal.ViewMode == CalendarViewMode.VIEW_CAL_MONTH))    //Rendu pour le mode mois
                ||
                ((this._parentDay.ParentCalendar != null) && (_parentDay.ParentCalendar.ViewMode == CalendarViewMode.VIEW_CAL_MONTH))
                )
            {
                //Nombre d'heure à afficher
                Int32 nNbHourToDisplay = (Int32)(_parentDay.ParentCalendar.MonthViewHourEnd - _parentDay.ParentCalendar.MonthViewHourBegin).TotalHours;

                //Heure du premier intervalle
                int nbrIntervalTop = (int)(new TimeSpan(DateBegin.Hour, DateBegin.Minute, 0) - parentCal.MonthViewHourBegin).TotalMinutes / _parentDay.ParentCalendar.MonthMinutesInterval;
                if (nbrIntervalTop < 0)
                    nbrIntervalTop = 0;

                DateTime tmpDateBegin = DateBegin;
                DateTime tmpDateEnd = DateEnd;

                //RDV sur plusieurs jours : Date fin
                if (this.DateEnd > this.ParentDay.DayDate + parentCal.MonthViewHourEnd)
                    tmpDateEnd = this.ParentDay.DayDate + parentCal.MonthViewHourEnd;

                //RDV sur plusieurs jours : Date début
                if (this.DateBegin < this.ParentDay.DayDate + parentCal.MonthViewHourBegin)
                    tmpDateBegin = this.ParentDay.DayDate + parentCal.MonthViewHourBegin;

                //Nb d'intervals couvert par le RDV
                int nbrIntervals = (int)Math.Ceiling((tmpDateEnd - tmpDateBegin).TotalMinutes / parentCal.MonthMinutesInterval);

                //La plage couverte par un RDV ne peut dépasser la plage affiché 
                if (nbrIntervals > parentCal.NbIntervals)
                    nbrIntervals = parentCal.NbIntervals;
                //Affectation de toutes les intervalles ou se trouve un RDV sur une journée
                for (int idxInterval = nbrIntervalTop; idxInterval < nbrIntervals + nbrIntervalTop; idxInterval++)
                {
                    //Recherche des itnervalles couverts par le RDV
                    String currentId = String.Concat(_concernedUserId, "_", _parentDay.DayDate.Day, "_", (idxInterval));
                    if (String.IsNullOrEmpty(_parentIntervalId) || (!String.Concat(";", _parentIntervalId, ";").Contains(String.Concat(";", currentId, ";"))))
                    {
                        if (!String.IsNullOrEmpty(_parentIntervalId))
                            _parentIntervalId += ";";
                        _parentIntervalId += currentId;
                    }
                }


            }
            else    //Rendu pour les autres modes
            {
                //Hauteur du planning
                Int32 nTotal = _parentDay.ParentCalendar.GlobalCalHeight;// pref.Context.ScreenHeight;

                //Nombre d'heure à afficher
                Int32 nNbHourToDisplay = (Int32)(_parentDay.ParentCalendar.ViewHourEnd - _parentDay.ParentCalendar.ViewHourBegin).TotalHours;



                if (parentCal.CellHeight == 0)
                    parentCal.CellHeight = (Int32)(nTotal / ((nNbHourToDisplay * 60 / parentCal.MinutesInterval)));

                //Postion par rapport au premier interval
                int nbrIntervalTop = (int)(new TimeSpan(DateBegin.Hour, DateBegin.Minute, 0) - parentCal.ViewHourBegin).TotalMinutes / _parentDay.ParentCalendar.MinutesInterval;
                if (nbrIntervalTop < 0)
                    nbrIntervalTop = 0;

                _parentIntervalId = String.Concat(_concernedUserId, "_", _parentDay.DayRange, "_", nbrIntervalTop);

                DateTime tmpDateBegin = DateBegin;
                DateTime tmpDateEnd = DateEnd;
                //RDV sur plusieurs jours : Date fin
                if (this.DateEnd > this.ParentDay.DayDate + parentCal.ViewHourEnd)
                {
                    tmpDateEnd = this.ParentDay.DayDate + parentCal.ViewHourEnd;
                    _isBottomResizeEnabled = false;
                }

                //RDV sur plusieurs jours : Date début
                if (this.DateBegin < this.ParentDay.DayDate + parentCal.ViewHourBegin)
                {
                    _parentIntervalId = _concernedUserId + "_" + _parentDay.DayRange + "_0";
                    tmpDateBegin = this.ParentDay.DayDate + parentCal.ViewHourBegin;
                }

                //Nb d'intervals couvert par un RDV
                int nbrIntervals = (int)Math.Ceiling((tmpDateEnd - tmpDateBegin).TotalMinutes / parentCal.MinutesInterval);
                int itemHeight = (int)(nbrIntervals * parentCal.CellHeight) + nbrIntervals - 1;
                _divElement.Style.Add(HtmlTextWriterStyle.Height, itemHeight + "px");
                _divElement.Attributes.Add("int", nbrIntervals.ToString());
                if (IsAllDay)
                    _divElement.Attributes.Add("ad", "1");

                #region divs de déplacement et des divs de redimentionnement
                //Top

                //Ajout des divs de déplacement et des divs de redimentionnement

                //Top
                if (_isBottomResizeEnabled && _isTopResizeEnabled)
                {
                    HtmlGenericControl divGrip1 = new HtmlGenericControl("div");
                    divGrip1.ID = string.Concat("gt_", _concernedUserId, "_", _nFileId, "_", _itemRange);
                    divGrip1.Attributes.Add("class", "griptop");
                    divGrip1.Attributes.Add("onmousedown", "IOR(this,event);"); //TODO EVT dynamique !
                    _divElement.Controls.Add(divGrip1);

                    HtmlGenericControl divGrip2 = new HtmlGenericControl("div");
                    divGrip2.ID = string.Concat("gb_", _concernedUserId, "_", _nFileId, "_", _itemRange);
                    divGrip2.Attributes.Add("class", "griptop");
                    divGrip2.Style.Add("top", (itemHeight - 5) + "px");
                    divGrip2.Attributes.Add("onmousedown", "IOR(this,event);"); //TODO EVT dynamique !
                    _divElement.Controls.Add(divGrip2);
                }
                else
                    _isMoveEnabled = false;

                //left
                HtmlGenericControl divGrip3 = new HtmlGenericControl("div");
                divGrip3.ID = string.Concat("gl_", _concernedUserId, "_", _nFileId, "_", _itemRange);
                divGrip3.Attributes.Add("class", "gripleft");
                divGrip3.Style.Add("background-color", _gripColor);
                if (_isMoveEnabled)
                    divGrip3.Attributes.Add("onmousedown", "IOMD(this,event);"); //TODO EVT dynamique !
                else
                    divGrip3.Style.Add(HtmlTextWriterStyle.Cursor, "not-allowed");
                _divElement.Controls.Add(divGrip3);

                #endregion
            }
        }

        /// <summary>
        /// Clone un RDV
        /// </summary>
        /// <param name="itmRange">Rang de l'élement</param>
        /// <param name="pref">Preference de l'user en cours</param>
        /// <returns>Le rdv clonné</returns>
        internal ePlanningItem Clone(Int32 itmRange, ePref pref)
        {
            ePlanningItem itm = new ePlanningItem();
            itm.ItemRange = itmRange;
            itm.IsMovable = _isMoveEnabled;
            itm.IsBottomResizeEnabled = _isBottomResizeEnabled;
            itm.IsTopResizeEnabled = _isTopResizeEnabled;
            itm._nTab = _nTab;
            itm.InitPlanningItem(this.FileId, this.DateBegin, this.DateEnd, this._innerHtml, this._typ, this._row, this.FieldsToDisplay, this.ToolTipInfos, this._color, this._gripColor, this.ConcernedUserId, pref);
            return itm;
        }

    }
}