using Com.Eudonet.Internal;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Common.Enumerations;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Objet Agenda complet
    /// TODO : cette classe mélange classe métier et classe de rendu
    /// sans utiliser l'existant et lance toute la logique dans le constructeur.
    /// A refaire :
    /// il faut donc das l'attente être prudent, son utilisation
    /// peut être très couteuse (par exemple, l'appel a new va par défaut exécuter une requête sur le mode courrant du planning....)
    /// </summary>
     [Obsolete("Classe a revoir globalement. A utiliser avec précaution")]
    public class ePlanning
    {
        #region VARIABLES PRIVEES
        /// <summary>Objet des preferences XRM de l'user en cours</summary>
        ePref _pref;
        DateTime _calendarDate;
        /// <summary>Propriété de la table planning affichée</summary>
        private Int32 _nTab;

        /// <summary>Objet d'accès aux données</summary>
        public eList _list;



        /// <summary>Liste des objets jours de la semaine en cours</summary>
        List<ePlanningDay> _days;
        /// <summary>Largeur de la zone d'affichage du planning</summary>
        Int32 _globalCalWidth = eConst.DEFAULT_WINDOW_WIDTH;
        /// <summary>Hauteur de la zone d'affichage du planning</summary>
        Int32 _globalCalHeight = eConst.DEFAULT_WINDOW_HEIGHT;
        /// <summary>Largeur de la fenêtre</summary>
        Int32 _allScreenWidth = eConst.DEFAULT_WINDOW_WIDTH;
        /// <summary>Liste des éléments HTML pouvant contenir des RDV</summary>
        Dictionary<String, HtmlGenericControl> _divIntervals;
        /// <summary>Objet eudoquery</summary>
        EudoQuery.EudoQuery queryCal;
        /// <summary>Objet d'accès à la Base de donnée</summary>
        public eudoDAL _dal;

        /// <summary>Largeur de la conne heure (Semaine et Jours)</summary>
        const int HOURS_COLUMN_WIDTH = 51;
        /// <summary>Largeur d'un item (Semaine et Jours)</summary>
        const int CALENDAR_ITEM_WIDTH = 250;
        /// <summary>Largeur d'un item (Semaine et Jours)</summary>
        const int CALENDAR_ITEM_MIN_WIDTH = 70;
        /// <summary>Espace entre 2 items (Semaine et Jours)</summary>
        const int CALENDAR_ITEM_SEPARATOR = 7;
        /// <summary>Marge droite (Semaine et Jours)</summary>
        const int CALENDAR_MARGIN_RIGHT = 15;
        /// <summary>Espace entre le calendrier mode jour et la liste des tâches (Semaine et Jours)</summary>
        const int CALENDAR_CAL_TASK_SEPARATOR = 55;
        #endregion

        #region CONSTRUCTEUR
        /// <summary>
        /// Constructeur.
        /// Attention au paramètre bNoLoad. Pour maintenir l'historique il a été créé avec une valeur a false par défaut
        /// mais cela signifie que cela lance directement une requete sur un mode liste planning qui suivant le contexte
        /// peut être non filtré (donc sur tous les planning de la base)
        /// </summary>
        /// <param name="dal">Objet d'accès à la BDD XRM dont la connexion a déjà été ouverte</param>
        /// <param name="pref">Preférences de l'utilisateur en cours</param>
        /// <param name="tab">Fichier Planning (descid)</param>
        /// <param name="width">largeur</param>
        /// <param name="height">hauteur</param>
        /// <param name="parentEQ">Eudoquery parent dans le cas ou il a déjà été chargé on ne le recharge pas de nouveau</param>
        /// <param name="bNoLoad">Indique qu'il ne faut pas lancer la requête de récupération de planning. Utilisé pour récupérer uniquement les préférence</param>
        public ePlanning(eudoDAL dal, ePref pref, Int32 tab, Int32 width, Int32 height, EudoQuery.EudoQuery parentEQ, bool bNoLoad = false)
        {
            _globalCalWidth = width - 20;   // 10 : bordure de droite
            _allScreenWidth = width - 60;   // 50 : bordure de droite
            _globalCalHeight = height;	//Les cas particulier de dimensions sont traités dans le init

            _pref = pref;
            _nTab = tab;
            _dal = dal;
            String err = string.Empty;

            //Initialisation            
            if (!this.Initialize(parentEQ, out err, bNoLoad))
                throw (new Exception("ePlanning.Initialize : " + err));
            //chargement des élements  Base --> objets .Net
            if (parentEQ == null && !bNoLoad)
                if (!this.LoadItems(out err))
                    throw (new Exception("ePlanning.LoadItems : " + err));

            _calendarDate = queryCal.GetCalendarDate.Date;
            if (parentEQ == null)
                queryCal.CloseQuery();
        }
        #endregion

        #region METHODES PRIVEES

        /// <summary>
        /// Préparation des informations pour les tooltips des filtres
        /// TODO - MUTUALISER AVEC eListMain !!!
        /// </summary>
        protected void LoadFilterInfo()
        {
            FilterTipInfo result = null;

            string charindex = _pref.GetPref(MainTable.DescId, ePrefConst.PREF_PREF.CHARINDEX);
            MarkedFilesSelection marked = null;
            _pref.Context.MarkedFiles.TryGetValue(MainTable.DescId, out marked);

            //Filtre avancé en cours
            FilterSel filterSel = null;
            _pref.Context.Paging.Tab = MainTable.DescId;
            _pref.Context.Filters.TryGetValue(MainTable.DescId, out filterSel);

            bool expressEnabled = _lstActiveExpressFilter.Count > 0;           // Filtre express en cours
            bool quickEnabled = _lstActiveQuickFilter.Count > 0;               // Filtre quick en cours
            bool charIndexEnabled = !String.IsNullOrEmpty(charindex);    // Filtre index
            bool defaultEnabled = this.queryCal.GetDefaultFilterEnabled;        // Filtre par defaut
            bool alwaysActiveDefaultFilter = this.queryCal.GetIsAlwaysActiveDefaultFilter; // US #2147 - Tâche #3127 - Demande #70 070 - Le filtre par défaut est désormais dénommé "Filtre permanent" s'il n'est pas désactivable/remplaçable par un autre filtre avancé
            //Boolean histoEnabled = HistoInfo.Actived;                      // Filtre historique
            bool markedFileEnabled = marked != null && marked.Enabled && !String.IsNullOrEmpty(marked.Name);     // Filtre fiche marquées
            bool bAdvFilterEnabled = (filterSel != null && filterSel.FilterSelId > 0);
            bool _displayFlagFilter = markedFileEnabled || quickEnabled || expressEnabled || charIndexEnabled
                /*|| histoEnabled */|| defaultEnabled || bAdvFilterEnabled;

            if (!_displayFlagFilter)
                return;

            #region Filtres fiche marquées // OK

            if (markedFileEnabled)
            {
                result = new FilterTipInfo();
                result.Label = marked.Name;
                result.Type = FilterTipType.MARKEDFILE;

                _filterTipInfo.Add(result);
            }

            #endregion

            #region Filtre avancé

            if (bAdvFilterEnabled)
            {
                result = new FilterTipInfo();
                result.Label = filterSel.FilterName;
                result.Type = FilterTipType.ADVANCED;
                _filterTipInfo.Add(result);
            }

            #endregion

            #region Filtres par défaut / Filtre CharIndex / Filtre en cours / Filtre historique
            //dans le cas du planning graphique, l'historisation n'est pas prise en compte
            if (defaultEnabled || charIndexEnabled /*|| histoEnabled*/)
            {
                if (charIndexEnabled)
                {
                    result = new FilterTipInfo();
                    result.Label = String.Concat(eResApp.GetRes(_pref, 6218), "&nbsp;(", charindex, ")");
                    result.Type = FilterTipType.CHARINDEX;

                    _filterTipInfo.Add(result);
                }


                //dans le cas du planning graphique, l'historisation n'est pas prise en compte
                //if (histoEnabled)
                //{
                //    result = new FilterInfo();
                //    result.label = String.Concat(eResApp.GetRes(_pref.Lang, 182), "&nbsp;", eResApp.GetRes(_pref.Lang, 17));
                //    result.type = FilterTipType.HISTO;

                //    _filterTipInfo.Add(result);
                //}

                if (queryCal.GetRandomEnabled)
                {
                    result = new FilterTipInfo();
                    result.Label = String.Concat(eResApp.GetRes(_pref, 1414), "&nbsp;", queryCal.GetTopRecord);
                    result.Type = FilterTipType.RANDOM;

                    _filterTipInfo.Add(result);
                }


                if (defaultEnabled)
                {
                    result = new FilterTipInfo();
                    result.Label = String.Concat(alwaysActiveDefaultFilter ? eResApp.GetRes(_pref, 2678) : eResApp.GetRes(_pref, 1102), "&nbsp;", eResApp.GetRes(_pref, 1206)); // US #2147 - Tâche #3127 - Demande #70 070 - Le filtre par défaut est dénommé Filtre permanent s'il n'est pas désactivable/remplaçable par un autre filtre avancé
                    result.Type = FilterTipType.DEFAULT;

                    _filterTipInfo.Add(result);
                }

            }
            #endregion

            #region Filtre rapides

            // Plus d'actualité sur l'infobulle
            if (quickEnabled && false)
                _filterTipInfo.AddRange(_lstActiveQuickFilter);

            #endregion

            #region Filtres express

            if (expressEnabled)
                _filterTipInfo.AddRange(_lstActiveExpressFilter);

            #endregion
        }

        /// <summary>
        /// Initialisation du planning
        /// </summary>
        /// <param name="parentEQ">Eudoquery parent dans le cas ou il a déjà été chargé on ne le recharge pas de nouveau</param>
        /// <param name="bNoLoad">Indique qu'il ne faut pas lancer la requete et la récupération des éléments de planning. voir demande #54166/54056.
        /// ePlanning charge en effet tous les éléments de planning du mode en cours, soit possiblement plusieurs milliers alors qu'il est parfois juste utilisé pour
        /// récupérer une préférence...
        /// Il faudrait refactoriser/refaire entièrement toute ces parties !
        /// </param>
        /// <param name="err">si une erreur se produite elel est renvoyée ici</param>
        /// <returns>Vrai si tout se passe bien</returns>
        internal bool Initialize(EudoQuery.EudoQuery parentEQ, out string err, bool bNoLoad = false)
        {
            #region Eudoquery

            err = string.Empty;
            if (parentEQ != null)
                queryCal = parentEQ;

            queryCal = eLibTools.GetEudoQuery(_pref, _nTab, ViewQuery.LIST);
            if (queryCal.GetError.Length > 0)
            {
                err = queryCal.GetError;
                queryCal.CloseQuery();
                return false;
            }
            queryCal.SetModeCalendarGraph = true;   //mode graphique


            //Filtre actif
            if (Pref.Context.Filters.ContainsKey(_nTab))
                queryCal.SetFilterId = Pref.Context.Filters[_nTab].FilterSelId;

            queryCal.LoadRequest();



            if (queryCal.GetError.Length > 0)
            {
                err = queryCal.GetError;
                queryCal.CloseQuery();
                return false;
            }



            if (!bNoLoad)
                queryCal.BuildRequest();

            if (queryCal.GetError.Length > 0)
            {
                err = queryCal.GetError;
                queryCal.CloseQuery();
                return false;
            }
            _divIntervals = new Dictionary<string, HtmlGenericControl>();
            _mainTable = queryCal.GetMainTable;
            #endregion

            #region Filtres rapides et Filtres express

            _lstQuickFilter = new List<Field>();
            _lstActiveQuickFilter = new List<FilterTipInfo>();
            _lstActiveExpressFilter = new List<FilterTipInfo>();
            foreach (Field field in queryCal.GetFieldHeaderList)
            {
                // Filtre rapides
                // Remarque : un filtre rapide peut ne pas être affiché en mode liste - traitement a effectué avant le test sur drawfield
                // On n'affiche pas dans l'infobulle les filtres rapides non appliqués (actifs)

                if (field.QuickFilterInfo.Has)
                {
                    _lstQuickFilter.Add(field);

                    if (field.QuickFilterInfo.Actived)
                    {
                        // Filtre rapide actif
                        FilterTipInfo inf = FilterTools.GetQuickFilterInfo(Pref, field);
                        if (inf != null)
                            _lstActiveQuickFilter.Add(inf);
                    }
                }

                if (!String.IsNullOrEmpty(field.ExpressFilterActived))
                {
                    FilterTipInfo inf = FilterTools.GetExpressFilterInfo(Pref, field);
                    if (inf != null)
                        _lstActiveExpressFilter.Add(inf);
                }

            }
            #endregion

            #region Filtres info

            _filterTipInfo = new List<FilterTipInfo>();

            LoadFilterInfo();

            #endregion

            #region Historique
            //Histo

            _histoInfo = new FillerHistoInfo();
            _histoInfo.Descid = queryCal.GetMainTable.HistoDescId;
            if (_histoInfo.Has)
            {
                _histoInfo.BgColor = queryCal.GetParam("HistoBackGround");
                _histoInfo.Color = queryCal.GetParam("HistoColor");
                _histoInfo.Icon = queryCal.GetParam("HistoIcon");
                _histoInfo.Actived = (queryCal.GetParam("Histo") == "1");
            }

            #endregion

            #region Préférences
            ViewMode = queryCal.GetPrefViewMode;
            TaskMode = queryCal.GetTaskMode;
            DateTarget = queryCal.GetCalendarDate;

            _nDateBeginDescId = queryCal.GetDateDescId;
            _nDateEndDescId = _nTab + EudoQuery.PlanningField.DESCID_TPL_END_TIME.GetHashCode();


            string sTime = queryCal.GetGraphParam("CalendarWorkHourBegin");
            WorkHourBegin = String.IsNullOrEmpty(sTime) ? new TimeSpan(9, 0, 0) : eTools.GetTimeFromString(sTime);
            sTime = queryCal.GetGraphParam("CalendarWorkHourEnd");
            WorkHourEnd = String.IsNullOrEmpty(sTime) ? new TimeSpan(18, 0, 0) : eTools.GetTimeFromString(sTime);
            sTime = queryCal.GetGraphParam("CalendarViewHourBegin");
            _dViewHourBegin = String.IsNullOrEmpty(sTime) ? new TimeSpan(9, 0, 0) : eTools.GetTimeFromString(sTime);
            sTime = queryCal.GetGraphParam("CalendarViewHourEnd");
            _dViewHourEnd = String.IsNullOrEmpty(sTime) ? new TimeSpan(18, 0, 0) : eTools.GetTimeFromString(sTime);
            _nMinutesInterval = eLibTools.GetNum(queryCal.GetGraphParam("CalendarMinutesInterval"));
            if (_nMinutesInterval == 0)
                _nMinutesInterval = 30;
            ItemDefaultDuration = eLibTools.GetNum(queryCal.GetGraphParam("CalendarItemDefaultDuration"));
            if (ItemDefaultDuration <= 0)
                ItemDefaultDuration = 120;
            #endregion

            #region Preference spécifique au mode MOIS
            sTime = queryCal.GetGraphParam("CalendarMonthViewHourBegin");
            _dMonthViewHourBegin = String.IsNullOrEmpty(sTime) ? ViewHourBegin : eTools.GetTimeFromString(sTime);
            sTime = queryCal.GetGraphParam("CalendarMonthViewHourEnd");
            _dMonthViewHourEnd = String.IsNullOrEmpty(sTime) ? ViewHourEnd : eTools.GetTimeFromString(sTime);
            _nMonthMinutesInterval = eLibTools.GetNum(queryCal.GetGraphParam("CalendarMonthIntervalDuration"));
            if (_nMonthMinutesInterval <= 0)
                _nMonthMinutesInterval = _nMinutesInterval;
            //_nMonthMinutesInterval = 120;

            #endregion

            NbHourToDisplayExact = (_dViewHourEnd - _dViewHourBegin).TotalHours;
            NbHourToDisplayRound = (Int32)NbHourToDisplayExact;
            NbIntervals = (Int32)((ViewMode == CalendarViewMode.VIEW_CAL_MONTH) ?
                    (((Int32)Math.Ceiling((decimal)(MonthViewHourEnd - MonthViewHourBegin).TotalHours)) / (MonthMinutesInterval / 60.0)) + 1 // On rajoute 1 en plus pour compenser l'arrondi
                    : (60.0 / _nMinutesInterval));
            if (ViewMode != CalendarViewMode.VIEW_CAL_MONTH)
                if (NbHourToDisplayExact > NbHourToDisplayRound)
                    NbIntervalSupplementaires = (Int32)((NbHourToDisplayExact - (double)NbHourToDisplayRound) * 60 / _nMinutesInterval);

            #region Calcul jours sur une semaine

            _workingDays = queryCal.GetGraphParam("CalendarWorkingDays")?.Trim();
            // HLA - On reprend la valeur par defaut
            if (String.IsNullOrEmpty(_workingDays))
                _workingDays = "2;3;4;5;6";
            string[] aDays = _workingDays.Split(';');

            _days = new List<ePlanningDay>();
            //Calcul de la première date de la semaine
            DayOfWeek firstDay = (DayOfWeek)(eLibTools.GetNum(aDays[0]) - 1);
            DateTime firstDayInWeek = DateTarget.Date;
            while (firstDayInWeek.DayOfWeek != firstDay)
                firstDayInWeek = firstDayInWeek.AddDays(-1);
            FirstDateOfWeek = firstDayInWeek;

            Int32[] aAllDays = { 0, 1, 2, 3, 4, 5, 6 };
            Int32 dayIdx = eLibTools.GetNum(aDays[0]);
            for (int i = 0; i < 7; i++)
            {
                aAllDays[i] = dayIdx;
                if (dayIdx < 6)
                    dayIdx++;
                else
                    dayIdx = 0;
            }

            /*for (int i = 0; i < aAllDays.Length; i++)
            {
                if (aDays.Contains((FirstDateOfWeek.AddDays(i).DayOfWeek.GetHashCode() + 1).ToString()))
                {
                    _days.Add(new ePlanningDay(this, aAllDays[i], FirstDateOfWeek.AddDays(i), 0));
                }

            }*/
            #endregion

            string sMenuUserId = queryCal.GetParam("MenuUserId");

            if (sMenuUserId == "-1") //Tous les users
                sMenuUserId = eDataTools.GetAllUserIdNotHidden(_pref);
            else
                sMenuUserId = eDataTools.GetAllUserId(sMenuUserId, _pref, true);

            if (string.IsNullOrEmpty(sMenuUserId))
                sMenuUserId = Pref.User.UserId.ToString();

            String[] aUsers = sMenuUserId.Split(";");

            #region Calcul jours sur un mois
            if (ViewMode == CalendarViewMode.VIEW_CAL_MONTH)
            {
                _globalCalHeight -= 150;    //Haut de page
                //Calcul de la première et dernière date du mois
                _dFirstDayInMonth = DateTarget.Date;
                _dFirstDayInMonth = new DateTime(_dFirstDayInMonth.Year, _dFirstDayInMonth.Month, 1);
                _dLastDayInMonth = _dFirstDayInMonth.AddMonths(1).AddDays(-1);  //dernier jours du mois : ajout de 1 mois puis retrait de 1 jours
                Int32 nbMonthDay = DateTime.DaysInMonth(_dFirstDayInMonth.Year, _dFirstDayInMonth.Month);
                #region Récupération des jours du moi affiché seulement et classement par semaines
                DateTime currentDT = _dFirstDayInMonth;

                //Int32 nCurrentWeekN = 0;    //Numéro de semaine courante
                Int32 nCurrentDayInWk = 0;    //Numéro de jours courant
                _days = new List<ePlanningDay>();
                for (int i = 0; i < nbMonthDay; i++)
                {
                    currentDT = new DateTime(_dFirstDayInMonth.Year, _dFirstDayInMonth.Month, i + 1);   //Date courante

                    //Si journée de semaine à afficher on stock cette date
                    nCurrentDayInWk = (currentDT.DayOfWeek.GetHashCode() + 1);
                    if (aDays.Contains<String>(nCurrentDayInWk.ToString()))
                    {
                        for (int idxUsr = 0; idxUsr < aUsers.Length; idxUsr++)
                        {
                            ePlanningDay ePlD = new ePlanningDay(_dal, this, nCurrentDayInWk, currentDT, eLibTools.GetNum(aUsers[idxUsr]));
                            _days.Add(ePlD); //Ajout du jours dans la liste de jours du mois
                        }


                    }
                }
                #endregion
            }
            #endregion
            else
                if (ViewMode == CalendarViewMode.VIEW_CAL_DAY || ViewMode == CalendarViewMode.VIEW_CAL_DAY_PER_USER)
            {
                _globalCalHeight -= 200;//// (aUsers.Length > 1) ? 220 : 300;    //Haut de page différent selon multi user ou non
                for (int idxUsr = 0; idxUsr < aUsers.Length; idxUsr++)
                {
                    for (int i = 0; i < aAllDays.Length; i++)
                    {
                        if (aDays.Contains((FirstDateOfWeek.AddDays(i).DayOfWeek.GetHashCode() + 1).ToString()))
                        {
                            _days.Add(new ePlanningDay(_dal, this, aAllDays[i], FirstDateOfWeek.AddDays(i), eLibTools.GetNum(aUsers[idxUsr])));
                        }

                    }
                }
            }
            else
            {
                _globalCalHeight -= 200;
                for (int i = 0; i < aAllDays.Length; i++)
                {
                    if (aDays.Contains((FirstDateOfWeek.AddDays(i).DayOfWeek.GetHashCode() + 1).ToString()))
                    {
                        _days.Add(new ePlanningDay(_dal, this, aAllDays[i], FirstDateOfWeek.AddDays(i), Pref.User.UserId));
                    }
                }
            }


            UserDisplayed = new List<int>();
            for (int i = 0; i < aUsers.Length; i++)
            {
                UserDisplayed.Add(eLibTools.GetNum(aUsers[i]));
            }
            // Couleurs
            GripMultiOwnerColor = queryCal.GetGraphParam("CalendarGripMultiOwnerColor");
            GripUserOwnerColor = queryCal.GetGraphParam("CalendarGripUserOwnerColor");
            GripConfidentialColor = queryCal.GetGraphParam("CalendarGripConfidentialColor");
            GripPublicColor = queryCal.GetGraphParam("CalendarGripPublicColor");
            GripOtherConfidentialColor = queryCal.GetGraphParam("CalendarGripOtherConfidentialColor");
            _nCalendarItemOverLap = eLibTools.GetNum(queryCal.GetGraphParam("CalendarItemOverLap"));
            if (_nCalendarItemOverLap == 0)
                _nCalendarItemOverLap = eLibConst.CALENDAR_DEFAULT_ITEM_OVERLAP;
            this.TaskMode = queryCal.GetTaskMode;
            DispAllIntervalHour = queryCal.GetGraphParam("CalendarDispAllInterval") == "1";
            CellHeight = eLibTools.GetNum(queryCal.GetGraphParam("CalendarCellHeight"));

            //Largeur de la journée
            if (ViewMode == CalendarViewMode.VIEW_CAL_DAY_PER_USER || ViewMode == CalendarViewMode.VIEW_CAL_DAY)
            {
                if (Pref.CalendarDayItemWidth == 0)
                    CellWidth = CALENDAR_ITEM_WIDTH;
                else if (Pref.CalendarDayItemWidth < CALENDAR_ITEM_MIN_WIDTH)
                    CellWidth = CALENDAR_ITEM_MIN_WIDTH;
                else
                    CellWidth = Pref.CalendarDayItemWidth;
                GlobalCalWidth = (int)CellWidth * UserDisplayed.Count + HOURS_COLUMN_WIDTH;
            }
            else
                CellWidth = (GlobalCalWidth - HOURS_COLUMN_WIDTH) / _days.Count;


            //Hauteur cellule
            Int32 nNbHourToDisplay = (Int32)(ViewHourEnd - ViewHourBegin).TotalHours;

            if (CellHeight == 0)
                CellHeight = (Int32)(GlobalCalHeight / (1 + (nNbHourToDisplay * 60 / MinutesInterval)));

            return true;
        }

        /// <summary>
        /// Permet de générer le CSS dynamique du planning (avec les balise style)
        /// </summary>
        /// <returns>Chaine de CSS générée</returns>
        public String GetCss()
        {
            StringBuilder sbCss = new StringBuilder();

            sbCss.Append("pgc|;|") //Planning-Grid-Cell
                .Append(String.Concat("height :", CellHeight, "px;"));


            if (ViewMode == CalendarViewMode.VIEW_CAL_MONTH)    //CSS calculé du rendu mode mois
            {
                sbCss.Append(SEPARATOR.LVL1)	//CSS d'un jours vide
                    .Append("PMD|;|")
                    .Append("height:100%;")
                    .Append("width:").Append(this.CellWidth).Append("%;");
                foreach (KeyValuePair<String, String> currentClass in _listCSS)
                {
                    sbCss.Append(SEPARATOR.LVL1)
                        .Append(currentClass.Value).Append(SEPARATOR.LVL2)
                        .Append("cursor: pointer;")
                        .Append("height:100%;")
                        .Append("width:").Append(this.CellWidth).Append("%;")
                        .Append(currentClass.Key).Append(";");
                }
            }
            else
            {
                foreach (KeyValuePair<String, String> currentClass in _listCSS)
                {
                    sbCss.Append(SEPARATOR.LVL1)
                        .Append(currentClass.Value).Append(SEPARATOR.LVL2);
                    foreach (String s in currentClass.Key.Split(SEPARATOR.LVL1))
                    {
                        sbCss.Append(s).Append(";");
                    }
                }
            }
            return sbCss.ToString();
        }

        /// <summary>
        /// Permet de récupérer le numero de la semaine passée en paramètre
        /// </summary>
        /// <param name="date">Date</param>
        /// <returns>Numéro de semaine</returns>
        public static Int32 GetWeekOfYear(DateTime date)
        {
            System.Globalization.GregorianCalendar cal = new System.Globalization.GregorianCalendar();
            return cal.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);    //Numéro de semaine courante
        }

        /// <summary>
        /// Charge les éléments RDV du calendrier
        /// </summary>
        /// <param name="err"></param>
        /// <returns></returns>
        private bool LoadItems(out string err)
        {
            err = string.Empty;

            DataTableReaderTuned dtr = _dal.Execute(new RqParam(queryCal.EqQuery), out err);
            try
            {
                if (err.Length != 0)
                {
                    return false;
                }

                while (dtr.Read())
                {
                    List<ePlanningItem> addedItems = new List<ePlanningItem>();
                    ePlanningItem calItm = new ePlanningItem(queryCal, this, dtr, 0, out addedItems);

                    foreach (ePlanningItem itmTmp in addedItems)
                    {
                        //cas spécifiques
                        if (itmTmp.DateBegin >= itmTmp.DateEnd)
                            continue;
                        //TODO-ajouter message si datte deb > date fin
                        if (itmTmp.TypePlanning == PlanningType.CALENDAR_ITEM_TASK)
                            continue;

                        //Cas d'un item sur plusieurs jours
                        if (itmTmp.DateBegin.Date != itmTmp.DateEnd.Date)
                        {
                            itmTmp.IsBottomResizeEnabled = false;
                            itmTmp.IsTopResizeEnabled = false;
                            itmTmp.IsMovable = false;
                            for (int i = 0; i <= (itmTmp.DateEnd.Date - itmTmp.DateBegin.Date).TotalDays; i++)
                            {
                                DateTime dTmpBegin = new DateTime();
                                if (i == 0)
                                    dTmpBegin = itmTmp.DateBegin;
                                else
                                    dTmpBegin = itmTmp.DateBegin.Date.AddDays(i);
                                DateTime dTmpEnd;
                                if (i == (itmTmp.DateEnd.Date - itmTmp.DateBegin.Date).TotalDays)
                                    dTmpEnd = itmTmp.DateBegin.Date.AddDays(i).AddHours(itmTmp.DateEnd.Hour).AddMinutes(itmTmp.DateEnd.Minute).AddSeconds(itmTmp.DateEnd.Second);
                                else
                                    dTmpEnd = itmTmp.DateBegin.Date.AddDays(i).AddHours(23).AddMinutes(59);

                                ePlanningItem calItmTmp = itmTmp.Clone(i, _pref);
                                calItmTmp.DateBegin = dTmpBegin;
                                calItmTmp.DateEnd = dTmpEnd;
                                calItmTmp.SetPosition(this);
                            }
                        }
                        else
                        {
                            itmTmp.SetPosition(this);
                        }
                    }

                }
            }
            finally
            {
                dtr?.Dispose();
            }
            return true;
        }

        #endregion

        #region PROPS PRIVEES
        /// <summary>Type de mode planning sélectionné (Mois, semaine, jours, jorus multi user...)</summary>
        CalendarViewMode _nViewMode;
        /// <summary>Heure de début affichée dans une journée</summary>
        private TimeSpan _dViewHourBegin;
        /// <summary>Heure de fin affichée dans une journée</summary>
        private TimeSpan _dViewHourEnd;

        /// <summary>Intervalle entre chaque ligne du planning</summary>
        private Int32 _nMinutesInterval;

        /// <summary>Heure de début affichée dans une journée (mode MOIS)</summary>
        private TimeSpan _dMonthViewHourBegin;
        /// <summary>Heure de fin affichée dans une journée (mode MOIS)</summary>
        private TimeSpan _dMonthViewHourEnd;
        /// <summary>Intervalle entre chaque ligne du planning (mode MOIS)</summary>
        private Int32 _nMonthMinutesInterval;

        /// <summary>Nombre maximal de chevauchement</summary>
        private Int32 _nCalendarItemOverLap;
        /// <summary>Type d'affiche de la tâche</summary>
        private CalendarTaskMode _nTaskMode;
        /// <summary>Affiche tous les intervalles du planning</summary>
        private Boolean _bDispAllIntervalHour;
        /// <summary>Heure de début d'une journée de travail</summary>
        private TimeSpan _dWorkHourBegin;
        /// <summary>Heure de fin d'une journée de travail</summary>
        private TimeSpan _dWorkHourEnd;
        /// <summary>Liste des utilisateurs affichés</summary>
        private List<Int32> _userDisplayed;
        /// <summary>Date du premier jour du planning</summary>
        private DateTime _dFirstDateOfWeek;
        /// <summary>Date du premier jours du mois courant affiché par l'utilisateur</summary>
        private DateTime _dFirstDayInMonth;
        /// <summary>Date du dernier jours du mois courant affiché par l'utilisateur</summary>
        private DateTime _dLastDayInMonth;

        /// <summary>Date courante</summary>
        private DateTime _dDateTarget;
        /// <summary>Couleur Appartenance (TPL99)</summary>
        private String _strGripUserOwnerColor;
        /// <summary>Couleur Multi - Appartenance (TPL92)</summary>
        private String _strGripMultiOwnerColor;
        /// <summary>Couleur Confidentialité (TPL84)</summary>
        private String _strGripConfidentialColor;
        /// <summary>Couleur Confidentialité (TPL84)</summary>
        private String _strGripOtherConfidentialColor;
        /// <summary>Couleur Public</summary>
        private String _strGripPublicColor;
        /// <summary>Largeur d'une journée</summary>
        private Double _nCellWidth;
        /// <summary>Hauteur d'un intervalle</summary>
        private Double _nCellHeight;
        /// <summary>Taille de chaque jours du mois en cours</summary>
        private float _monthWidthPerDays;
        /// <summary>Liste des couleurs de CSS à intégrer en mode MOIS</summary>
        private Dictionary<String, String> _listCSS = new Dictionary<String, String>();
        /// <summary>en mn, la durée par défaut</summary>
        private Int32 _nItemDefaultDuration;
        //private Boolean _bFilePrint;			        //Impression du mode graphique
        /// <summary>Jours travaillé</summary>
        private String _workingDays;
        /// <summary>Propriété de la table planning affichée</summary>
        private EudoQuery.Table _mainTable;
        // Liste de tous les filtres (rapide, expres, default, histo, ...) en ToolTip
        private List<FilterTipInfo> _filterTipInfo = null;

        // Liste des filtres rapide
        private List<Field> _lstQuickFilter = null;

        // Liste des filtres rapide en ToolTip
        private List<FilterTipInfo> _lstActiveQuickFilter = null;
        private List<FilterTipInfo> _lstActiveExpressFilter = null;

        /// <summary>Gestion de l'instorique</summary>
        protected FillerHistoInfo _histoInfo = null;
        /// <summary>DescId date début</summary>
        Int32 _nDateBeginDescId = 0;
        /// <summary>DescId date fin</summary>
        Int32 _nDateEndDescId = 0;


        #endregion

        #region ACCESSEURS

        /// <summary>Liste des éléments HTML pouvant contenir des RDV</summary>
        public Dictionary<String, HtmlGenericControl> DivIntervals
        {
            get { return _divIntervals; }
        }
        /// <summary>Propriété de la table planning affichée</summary>
        public EudoQuery.Table MainTable
        {
            get { return _mainTable; }
            set { _mainTable = value; }
        }
        /// <summary>Objet des preferences XRM de l'user en cours</summary>
        public ePref Pref
        {
            get { return _pref; }
            set { _pref = value; }
        }
        /// <summary>Propriété de la table planning affichée</summary>
        public Int32 Tab
        {
            get { return _nTab; }
            set { _nTab = value; }
        }
        /// <summary>Type de mode planning sélectionné (Mois, semaine, jours, jorus multi user...)</summary>
        public CalendarViewMode ViewMode
        {
            get { return _nViewMode; }
            set { _nViewMode = value; }
        }
        /// <summary>Heure de début affichée dans une journée</summary>
        public TimeSpan ViewHourBegin
        {
            get { return _dViewHourBegin; }
        }
        /// <summary>Heure de fin affichée dans une journée</summary>
        public TimeSpan ViewHourEnd
        {
            get { return _dViewHourEnd; }
        }
        /// <summary>Intervalle entre chaque ligne du planning</summary>
        public Int32 MinutesInterval
        {
            get { return _nMinutesInterval; }
        }
        /// <summary>Nombre d'intervalles par jours</summary>
        public Int32 NbIntervals { get; private set; }

        /// <summary>Nombre dheure à afficher dans le planning</summary>
        public Int32 NbHourToDisplayRound { get; private set; }

        /// <summary>Nombre dheure à afficher dans le planning à la virgule près</summary>
        public Double NbHourToDisplayExact { get; private set; }

        /// <summary>Nombre d'intervals supplémentaires entrele nombre d'heures à afficher et le nombre d'heure à la virgule près</summary>
        public Int32 NbIntervalSupplementaires { get; private set; }

        /// <summary>Heure de début affichée dans une journée (mode MOIS)</summary>
        public TimeSpan MonthViewHourBegin
        {
            get { return _dMonthViewHourBegin; }
        }
        /// <summary>Heure de fin affichée dans une journée (mode MOIS)</summary>
        public TimeSpan MonthViewHourEnd
        {
            get { return _dMonthViewHourEnd; }
        }
        /// <summary>Intervalle entre chaque ligne du planning (mode MOIS)</summary>
        public Int32 MonthMinutesInterval
        {
            get { return _nMonthMinutesInterval; }
        }

        /// <summary>Nombre maximal de chevauchement</summary>
        public Int32 CalendarItemOverLap
        {
            get { return _nCalendarItemOverLap; }
        }
        /// <summary>Type d'affiche de la tâche</summary>
        public CalendarTaskMode TaskMode
        {
            get { return _nTaskMode; }
            set { _nTaskMode = value; }
        }
        /// <summary>Affiche tous les intervalles du planning</summary>
        public Boolean DispAllIntervalHour
        {
            get { return _bDispAllIntervalHour; }
            set { _bDispAllIntervalHour = value; }
        }
        /// <summary>Heure de début d'une journée de travail</summary>
        public TimeSpan WorkHourBegin
        {
            get { return _dWorkHourBegin; }
            set { _dWorkHourBegin = value; }
        }
        /// <summary>Heure de fin d'une journée de travail</summary>
        public TimeSpan WorkHourEnd
        {
            get { return _dWorkHourEnd; }
            set { _dWorkHourEnd = value; }
        }
        /// <summary>Liste des objets jours de la semaine en cours</summary>
        public List<ePlanningDay> Days
        {
            get { return _days; }
            set { _days = value; }
        }
        /// <summary>Liste des utilisateurs affichés</summary>
        public List<Int32> UserDisplayed
        {
            get { return _userDisplayed; }
            set { _userDisplayed = value; }
        }
        /// <summary>Date du premier jour du planning</summary>
        public DateTime FirstDateOfWeek
        {
            get { return _dFirstDateOfWeek; }
            set { _dFirstDateOfWeek = value; }
        }

        /// <summary>Date du premier jours du mois courant affiché par l'utilisateur</summary>
        public DateTime FirstDayInMonth
        {
            get { return _dFirstDayInMonth; }
        }
        /// <summary>Date du dernier jours du mois courant affiché par l'utilisateur</summary>
        public DateTime LastDayInMonth
        {
            get { return _dLastDayInMonth; }
        }
        /// <summary>Date courante</summary>
        public DateTime DateTarget
        {
            get { return _dDateTarget; }
            set { _dDateTarget = value; }
        }
        /// <summary>Couleur Appartenance (TPL99)</summary>
        public String GripUserOwnerColor
        {
            get { return _strGripUserOwnerColor; }
            set { _strGripUserOwnerColor = value; }
        }

        /// <summary>Couleur Multi - Appartenance (TPL92)</summary>
        public String GripMultiOwnerColor
        {
            get { return _strGripMultiOwnerColor; }
            set { _strGripMultiOwnerColor = value; }
        }
        /// <summary>Couleur Confidentialité (TPL84)</summary>
        public String GripConfidentialColor
        {
            get { return _strGripConfidentialColor; }
            set { _strGripConfidentialColor = value; }
        }
        /// <summary>Couleur Public</summary>
        public String GripPublicColor
        {
            get { return _strGripPublicColor; }
            set { _strGripPublicColor = value; }
        }
        /// <summary>Couleur Confidentialité (TPL84)</summary>
        public String GripOtherConfidentialColor
        {
            get { return _strGripOtherConfidentialColor; }
            set { _strGripOtherConfidentialColor = value; }
        }

        /// <summary>Largeur d'une journée</summary>
        public Double CellWidth
        {
            get { return _nCellWidth; }
            set { _nCellWidth = value; }
        }
        /// <summary>Hauteur d'un intervalle</summary>
        public Double CellHeight
        {
            get { return _nCellHeight; }
            set { _nCellHeight = value; }
        }
        /// <summary>Liste des couleurs de CSS à intégrer en mode MOIS</summary>
        public Dictionary<String, String> CSS
        {
            get { return _listCSS; }
            set { _listCSS = value; }
        }

        /// <summary>en mn, la durée par défaut</summary>
        public Int32 ItemDefaultDuration
        {
            get { return _nItemDefaultDuration; }
            set { _nItemDefaultDuration = value; }
        }


        /// <summary>Hauteur de la zone d'affichage du planning</summary>
        public Int32 GlobalCalHeight
        {
            get { return _globalCalHeight; }
            set { _globalCalHeight = value; }
        }

        /// <summary>Largeur de la zone d'affichage du planning</summary>
        public Int32 GlobalCalWidth
        {
            get { return _globalCalWidth; }
            set { _globalCalWidth = value; }
        }
        /// <summary>Jours travaillé</summary>
        public String WorkingDays
        {
            get { return _workingDays; }
            set { _workingDays = value; }
        }
        /// <summary>DescId date début</summary>
        public Int32 DateBeginDescId
        {
            get { return _nDateBeginDescId; }
        }

        /// <summary>DescId date fin</summary>
        public Int32 DateEndDescId
        {
            get { return _nDateEndDescId; }
        }

        #endregion

        #region METHODES PUBLIC
        /// <summary>
        /// Retourne le rendu semaine
        /// </summary>
        /// <returns></returns>
        public HtmlGenericControl GetWeekRender()
        {
            HtmlGenericControl divMain = new HtmlGenericControl("div");
            AddFilterTip(divMain);

            divMain.Attributes.Add("intervals", (NbIntervals * NbHourToDisplayRound).ToString());
            divMain.Attributes.Add("dateDescId", _nDateBeginDescId.ToString());
            divMain.ID = "CalDivMain";
            divMain.Attributes.Add("calmode", _nViewMode.GetHashCode().ToString());
            divMain.Attributes.Add("onclick", "onCalClick(event)");
            divMain.Attributes.Add("ondblclick", "onCalDblClick(event)");
            divMain.Attributes.Add("interval", this.MinutesInterval.ToString());
            divMain.Attributes.Add("maxovl", _nCalendarItemOverLap.ToString());
            divMain.Attributes.Add("workingdays", this.WorkingDays);
            divMain.Attributes.Add("prevdate", _dFirstDateOfWeek.AddDays(-7).ToString("dd/MM/yyyy"));
            divMain.Attributes.Add("nextdate", _dFirstDateOfWeek.AddDays(7).ToString("dd/MM/yyyy"));
            divMain.Attributes.Add("prevmonth", _dFirstDateOfWeek.AddMonths(-1).ToString("dd/MM/yyyy"));
            divMain.Attributes.Add("nextmonth", _dFirstDateOfWeek.AddMonths(1).ToString("dd/MM/yyyy"));
            divMain.Attributes.Add("prevlabel", eLibTools.GetCase(EudoQuery.CaseField.CASE_CAPITALIZE, _dFirstDateOfWeek.AddDays(-7).ToString("dddd dd MMMM yyyy")));
            divMain.Attributes.Add("nextlabel", eLibTools.GetCase(EudoQuery.CaseField.CASE_CAPITALIZE, _dFirstDateOfWeek.AddDays(7).ToString("dddd dd MMMM yyyy")));
            divMain.Style.Add(HtmlTextWriterStyle.Width, _globalCalWidth + "px");
            HtmlGenericControl tabMain = new HtmlGenericControl("table");
            divMain.Controls.Add(tabMain);
            //EVT javascript
            //tabMain.Attributes.Add("onclick", "cal_fldLClick(event,'DIV');");
            tabMain.ID = "cal_mt_" + _nTab;
            //ELAIZ - demande 76959  - rajout du type de calendrier pour cibler en css
            tabMain.Attributes.Add("period", "week");
            tabMain.Attributes.Add("nbi", ((NbHourToDisplayRound * NbIntervals) + NbIntervalSupplementaires).ToString());

            //ajout du nombre d'intervalle pris par un nouveau rdb par défaut
            int nDefDuration = eLibTools.GetNum(_pref.GetPref(_nTab, ePrefConst.PREF_PREF.CALENDARITEMDEFAULTDURATION));
            if (nDefDuration == 0)
                nDefDuration = eLibConst.CALENDAR_ITEM_DEFAULT_DURATION;

            //Nombre d'intervals pris par la durée par défaut d'un planning
            int nNbDefInterDuration = nDefDuration / this.MinutesInterval;
            if (nNbDefInterDuration < 0)
                nNbDefInterDuration = 1;
            else if (nNbDefInterDuration > ((NbHourToDisplayRound * NbIntervals) + NbIntervalSupplementaires))
            {
                nNbDefInterDuration = 1;
            }

            tabMain.Attributes.Add("nbibyrdv", nNbDefInterDuration.ToString());

            tabMain.Attributes.Add("onmousemove", "onTabMouseMove(event);");
            tabMain.Attributes.Add("onmousedown", "onTabMouseDown(event);");
            tabMain.Attributes.Add("onmouseup", "onTabMouseUp(event);");
            tabMain.Style.Add(HtmlTextWriterStyle.Width, _globalCalWidth + "px");
            tabMain.Style.Add(HtmlTextWriterStyle.Position, "relative");
            tabMain.Style.Add(HtmlTextWriterStyle.Height, _globalCalHeight + "px");
            tabMain.Style.Add(HtmlTextWriterStyle.VerticalAlign, "top");
            //Un seul Tr
            HtmlGenericControl trMain = new HtmlGenericControl("tr");
            tabMain.Controls.Add(trMain);

            //Premiere colonne - Colonne des heures
            Control colHours = GetHoursColumn(false);
            trMain.Controls.Add(colHours);

            //Boucle sur toutes les journées de la semaine
            int dayIdx = 0;

            foreach (ePlanningDay day in _days)
            {
                trMain.Controls.Add(day.GetDayColumn(_pref, dayIdx));
                dayIdx++;

            }

            divMain.Attributes.Add("cellheight", this.CellHeight.ToString());

            //Demande #36 492 - Dernière colonne - Colonne des heures dupliquée
            Control secColHours = GetHoursColumn(true);
            trMain.Controls.Add(secColHours);

            return divMain;
        }


        /// <summary>
        /// Retourne le rendu mois user multiple
        /// </summary>
        /// <returns></returns>
        public HtmlGenericControl GetMonthMultUserRender(eudoDAL dal)
        {
            Int32 nbWorkingDays = this.WorkingDays.Split(";").Length;   //nombre de jours affiché dans la semaine
            String sLblWeek = eResApp.GetRes(Pref, 821);   //821   :   Semaine

            HtmlGenericControl divMain = new HtmlGenericControl("div");
            divMain.ID = "CalDivMain";
            divMain.Attributes.Add("dateDescId", _nDateBeginDescId.ToString());
            divMain.Attributes.Add("calmode", _nViewMode.GetHashCode().ToString());
            divMain.Style.Add(HtmlTextWriterStyle.Height, _globalCalHeight + "px");
            divMain.Style.Add("overflow-y", "auto");

            AddFilterTip(divMain);




            #region boutons de navigation (Mois par mois)
            divMain.Attributes.Add("prevdate", FirstDayInMonth.AddDays(-1).ToString("dd/MM/yyyy"));
            divMain.Attributes.Add("nextdate", LastDayInMonth.AddDays(1).ToString("dd/MM/yyyy"));
            divMain.Attributes.Add("prevlabel", eLibTools.GetCase(EudoQuery.CaseField.CASE_CAPITALIZE, FirstDayInMonth.AddDays(-1).ToString("MMMM yyyy")));
            divMain.Attributes.Add("nextlabel", eLibTools.GetCase(EudoQuery.CaseField.CASE_CAPITALIZE, FirstDayInMonth.AddMonths(1).ToString("MMMM yyyy")));
            #endregion

            System.Web.UI.WebControls.Table tabMain = new System.Web.UI.WebControls.Table();
            divMain.Controls.Add(tabMain);
            //EVT javascript
            tabMain.Attributes.Add("onclick", "cal_fldLClick(event,'TD');");
            tabMain.ID = "cal_mt_" + _nTab;
            tabMain.Style.Add(HtmlTextWriterStyle.Width, "100%");
            //Un seul Tr
            TableRow trMain = new TableRow();
            tabMain.Controls.Add(trMain);
            TableCell tdMain = new TableCell();
            trMain.Controls.Add(tdMain);
            tdMain.Style.Add(HtmlTextWriterStyle.VerticalAlign, "top");

            System.Web.UI.WebControls.Table tabMonth = new System.Web.UI.WebControls.Table();
            tdMain.Controls.Add(tabMonth);
            tabMonth.CellSpacing = 0;
            tabMonth.CellPadding = 0;
            tabMonth.Style.Add(HtmlTextWriterStyle.Width, "100%");

            TableRow trWeekLbl = new TableRow();
            tabMonth.Controls.Add(trWeekLbl);
            trWeekLbl.CssClass = "plM-HESWL";
            TableCell tdWeekLbl;
            TableRow trWeekDayNum = new TableRow();
            TableRow trWeekDayLabel = new TableRow();

            TableCell tdWeekDayNum, tdWeekDayLabel;

            tabMonth.Controls.Add(trWeekDayLabel);
            tabMonth.Controls.Add(trWeekDayNum);

            Int32 nWorkingDay = WorkingDays.Split(";").Length;

            #region
            tdWeekLbl = new TableCell();
            trWeekLbl.Controls.Add(tdWeekLbl);
            tdWeekLbl.Text = "&nbsp;";
            tdWeekLbl.CssClass = "plM-HESDL";

            // Nom du jour
            trWeekDayLabel.CssClass = "plM-dayLabel";
            tdWeekDayLabel = new TableCell();
            tdWeekDayLabel.Text = "&nbsp;";
            trWeekDayLabel.Controls.Add(tdWeekDayLabel);

            // Numéro du jour
            tdWeekDayNum = new TableCell();
            tdWeekDayNum.Text = "&nbsp;";
            trWeekDayNum.Controls.Add(tdWeekDayNum);

            #region Vide entre chaque User
            TableRow trEmptySpaceBetweenUser;

            TableCell tdEmptySpaceBetweenUser;
            #endregion
            #endregion
            Int32 nOldWeekN = 0;
            Int32 nCurrentNbDayInWk = 0;
            Int32 nNbDayInMth = 0;
            Int32 FirstUserId = 0;
            Dictionary<String, TableRow> UserRender = new Dictionary<String, TableRow>();   //Liste des lignes de rendu par utilisateurs
            List<String> listSUser = new List<String>();
            Dictionary<String, TableRow> userEmptySpacesRender = new Dictionary<String, TableRow>();   //Liste des lignes des espacements de rendu par utilisateurs
            TableRow trUserRow;
            TableCell tdUserRow;
            foreach (ePlanningDay currentPlD in this.Days)
            {
                #region On récupère le rendu jours de la personne
                if (!UserRender.ContainsKey(currentPlD.ConcernedUserId.ToString()))    //si pas encore de ligne affecté on la crée et l'ajoute au dico
                {
                    trUserRow = new TableRow(); //Création de la lisgne pour l'utilisateur
                    UserRender.Add(currentPlD.ConcernedUserId.ToString(), trUserRow);  //Affectation de la ligne à l'utilisateur

                    listSUser.Add(currentPlD.ConcernedUserId.ToString());    //Ajout de l'utilisateur à la LISTE DES UTILISATEURS

                    #region espacement entre les noms des USER
                    trEmptySpaceBetweenUser = new TableRow();
                    userEmptySpacesRender.Add(currentPlD.ConcernedUserId.ToString(), trEmptySpaceBetweenUser);
                    #endregion
                }
                tdUserRow = new TableCell();
                tdUserRow.CssClass = "plM-D";
                Control ctrl = currentPlD.GetDayForMonthRender(Pref); //RENDU ! récupération du rendu de la journée en cours;
                if (ctrl != null)
                    tdUserRow.Controls.Add(ctrl);
                UserRender[currentPlD.ConcernedUserId.ToString()].Controls.Add(tdUserRow);

                #region espacement entre les lignes des USER
                tdEmptySpaceBetweenUser = new TableCell();
                tdEmptySpaceBetweenUser.CssClass = "plM-DES";
                tdEmptySpaceBetweenUser.Text = "&nbsp;";
                userEmptySpacesRender[currentPlD.ConcernedUserId.ToString()].Controls.Add(tdEmptySpaceBetweenUser); //récupération du rendu de l'espace de la journée en cours
                #endregion

                #endregion
                if (FirstUserId != currentPlD.ConcernedUserId && FirstUserId > 0)
                {
                    continue;
                }


                if (currentPlD.WeekNumber != nOldWeekN)
                {
                    if (nCurrentNbDayInWk > 0)  //pas pour le premier tour
                    {
                        InitWeekInCell(sLblWeek, tdWeekLbl, nCurrentNbDayInWk, nOldWeekN);
                    }
                    else
                        FirstUserId = currentPlD.ConcernedUserId;

                    tdWeekDayNum = new TableCell(); //JOURS : TD libelle num jours
                    tdWeekDayNum.CssClass = "plM-NDL";  //JOURS : NUM JOURS bordure gauche et fond

                    tdWeekDayLabel = new TableCell();

                    nCurrentNbDayInWk = 0;  //remise à zéro du nb de jours de la semaine
                    //SEMAINE : NOUVELLE SEMAINE
                    tdWeekLbl = new TableCell();

                    // Freecode CRU 14122015 : Numéros des semaines cliquables en mode "Mois"
                    try
                    {
                        tdWeekLbl.Attributes.Add("onclick", String.Concat("setCalViewMode(", EudoQuery.CalendarViewMode.VIEW_CAL_WORK_WEEK.GetHashCode(), ", ",
                                                _nTab.ToString(), ", ",
                                                currentPlD.ConcernedUserId, ", ",
                                                "'", currentPlD.DayDate.ToString("dd/MM/yyyy"), "');"));
                    }
                    catch (Exception) { }

                    trWeekLbl.Controls.Add(tdWeekLbl);
                }
                else
                {
                    tdWeekDayLabel = new TableCell();

                    tdWeekDayNum = new TableCell(); //JOURS : TD libelle num jours
                    tdWeekDayNum.CssClass = "plM-NDM";  //JOURS : NUM JOURS bordure haute et fond
                }

                tdWeekDayLabel = new TableCell();
                tdWeekDayLabel.Text = eLibTools.GetDayLabel(Pref, currentPlD.DayDate.DayOfWeek.GetHashCode()).Substring(0, 2); // Jour de la semaine

                tdWeekDayNum.Text = currentPlD.DayDate.Day.ToString();  //JOURS : Numéro du jours

                trWeekDayLabel.Controls.Add(tdWeekDayLabel);
                trWeekDayNum.Controls.Add(tdWeekDayNum);    //JOURS : ajout du td au tr

                nCurrentNbDayInWk++;    //on incrémente le nb de jours dans la semaine en cours
                nNbDayInMth++;    //on incrémente le nb de jours du mois en cours
                nOldWeekN = currentPlD.WeekNumber;
            }
            //Initialisations de dernier tour
            tdWeekDayNum.CssClass = "plM-NDR";  //NUM JOURS bordure droite et fond
            InitWeekInCell(sLblWeek, tdWeekLbl, nCurrentNbDayInWk, nOldWeekN);  //Pour initialiser le dernier libellé de semaine sur la dernière colonne

            _monthWidthPerDays = (float)100 / (nNbDayInMth + nbWorkingDays);    //Largeur de chaque jours affiché pour un affichage iso

            #region init des largeurs de chaque jours :
            int counterTemp = 0;
            foreach (TableCell currentTC in trWeekDayNum.Cells)
            {
                if (counterTemp <= 0)   //première colonne (libellé des USERS) compte pour une semaine entière visuellement
                    currentTC.Width = Unit.Percentage(_monthWidthPerDays * (float)nbWorkingDays);
                else
                    currentTC.Width = Unit.Percentage(_monthWidthPerDays);
                counterTemp++;
            }
            #endregion


            Dictionary<Int32, eUser.UserListItem> listUser = new Dictionary<Int32, eUser.UserListItem>();   //Dictionnaire Userid et Info de l'utilisateur

            #region obtention des info sur les utilisateurs (séparé pour n'avoir qu'une seul requête)
            List<eUser.UserListItem> listULIUser = new List<eUser.UserListItem>();
            StringBuilder sbError = new StringBuilder();

            listULIUser = eModelTools.GetUser(dal, listSUser, Pref, false, true, ref sbError);

            if (sbError.Length > 0)
                throw (new Exception(String.Concat("ePlanning.GetMonthMultUserRender erreur : ", sbError.ToString())));
            #endregion

            String lastGroupLevel = String.Empty;
            List<TableRow> trRender = new List<TableRow>();
            GetMonthGroupRender(listULIUser, nNbDayInMth, UserRender, userEmptySpacesRender, ref trRender);

            foreach (TableRow currentUserRender in trRender)    //Affichage des rendu Group et User
            {
                tabMonth.Controls.Add(currentUserRender); //ajout du rendu de la personne au tableau

            }


            return divMain;

        }
        /// <summary>
        /// Obtention du rendu par groupe des Utilisateurs passé en paramètres
        /// </summary>
        /// <param name="userList">Liste des groupes et utilisateurs</param>
        /// <param name="nNbDayInMth">Constante du Nombre de jours dans le mois affiché</param>
        /// <param name="userRender">Liste des rendu déja calculé par UserId</param>
        /// <param name="userEmptySpacesRender">Liste des rendu d'espacement déja calculé par UserId</param>
        /// <param name="trRender">RETOUR : ajout à la fin du tableau les ligne de Rendu récupéré</param>
        private void GetMonthGroupRender(List<eUser.UserListItem> userList, Int32 nNbDayInMth, Dictionary<String, TableRow> userRender, Dictionary<String, TableRow> userEmptySpacesRender, ref List<TableRow> trRender)
        {
            Int32 counter = 0;
            foreach (eUser.UserListItem currentULI in userList)
            {
                if (currentULI.Type == eUser.UserListItem.ItemType.GROUP)   //RENDU LIGNE groupe
                {
                    TableRow trGroup = new TableRow();
                    TableCell tcGroup = new TableCell();
                    trGroup.Controls.Add(tcGroup);
                    tcGroup.Text = currentULI.Libelle;
                    tcGroup.CssClass = "plM-LG";
                    tcGroup = new TableCell();
                    trGroup.Controls.Add(tcGroup);
                    tcGroup.Text = "&nbsp;";
                    tcGroup.CssClass = "plM-GC";
                    tcGroup.ColumnSpan = nNbDayInMth;

                    trRender.Add(trGroup); //ajout du rendu du groupe au tableau
                }
                if (currentULI.Type == eUser.UserListItem.ItemType.USER)
                {
                    #region rendu des espacements   (N'est pas afficher à la première ligne)
                    TableRow trEmptySpaceBetweenUser = userEmptySpacesRender[currentULI.ItemCode];  //rendu des espacements

                    #region espace pour le libellé du nom   (Colonne vide du nom)
                    TableCell tdEmptySpaceBetweenUser = new TableCell();
                    tdEmptySpaceBetweenUser.Text = "<div>&nbsp;</div>";
                    tdEmptySpaceBetweenUser.CssClass = "plM-UES";
                    trEmptySpaceBetweenUser.Controls.AddAt(0, tdEmptySpaceBetweenUser);
                    #endregion

                    trRender.Add(trEmptySpaceBetweenUser);  //ajout du rendu de l'espacement sur la personne au tableau
                    #endregion


                    #region affichage du nom de l'utilisateur en cours
                    TableCell tdUserRow = new TableCell();

                    TableRow trUserRow = userRender[currentULI.ItemCode];
                    trUserRow.Controls.AddAt(0, tdUserRow);
                    tdUserRow.Text = currentULI.Libelle;
                    tdUserRow.CssClass = "plM-LU";
                    trRender.Add(trUserRow);  //ajout du rendu de la personne au tableau
                    #endregion

                }

                GetMonthGroupRender(currentULI.ChildrensUserListItem, nNbDayInMth, userRender, userEmptySpacesRender, ref trRender);
                counter++;
            }


        }

        private void InitWeekInCell(String sLblWeek, TableCell tdWeekLbl, Int32 nCurrentNbDayInWk, Int32 nWeekNumber)
        {

            tdWeekLbl.ColumnSpan = nCurrentNbDayInWk;   //SEMAINE : on affecte le nb de jours en taille de colonne du TD précédent
            //SEMAINE : on affiche Semaine suivie du numéro mais si moins de 2 jours on affiche S.
            tdWeekLbl.Text = String.Concat(
                String.IsNullOrEmpty(sLblWeek) ? "" : (
                (nCurrentNbDayInWk >= 3) ? sLblWeek : String.Concat(sLblWeek.Substring(0, 1), ".")
                )
                , " "
                , nWeekNumber);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public HtmlGenericControl GetDayRender(int nRows, int nPage)
        {
            HtmlGenericControl divMain = new HtmlGenericControl("div");

            AddFilterTip(divMain);


            divMain.Attributes.Add("intervals", ((NbIntervals * NbHourToDisplayRound) + NbIntervalSupplementaires).ToString());
            divMain.Attributes.Add("dateDescId", _nDateBeginDescId.ToString());
            divMain.ID = "CalDivMain";
            divMain.Attributes.Add("calmode", _nViewMode.GetHashCode().ToString());
            divMain.Attributes.Add("onclick", "onCalClick(event)");
            divMain.Attributes.Add("ondblclick", "onCalDblClick(event)");
            divMain.Attributes.Add("interval", this.MinutesInterval.ToString());
            divMain.Attributes.Add("maxovl", _nCalendarItemOverLap.ToString());
            divMain.Attributes.Add("cellheight", this.CellHeight.ToString());
            divMain.Attributes.Add("workingdays", this.WorkingDays);
            divMain.Attributes.Add("class", "D-elm");

            //next date
            DateTime dNextDate = DateTarget.AddDays(1);
            int nextDecal = 1;
            while (!WorkingDays.Split(";").Contains((dNextDate.DayOfWeek.GetHashCode() + 1).ToString()))
            {
                dNextDate = dNextDate.AddDays(nextDecal);
                nextDecal++;
            }

            divMain.Attributes.Add("nextdate", DateTarget.AddDays(nextDecal).ToString("dd/MM/yyyy"));
            divMain.Attributes.Add("nextlabel", eLibTools.GetCase(EudoQuery.CaseField.CASE_CAPITALIZE, DateTarget.AddDays(nextDecal).ToString("dddd dd MMMM yyyy")));

            //prev date
            DateTime dPrevDate = DateTarget.AddDays(-1);
            int prevDecal = -1;
            while (!WorkingDays.Split(";").Contains((dPrevDate.DayOfWeek.GetHashCode() + 1).ToString()))
            {
                dPrevDate = dPrevDate.AddDays(prevDecal);
                prevDecal--;
            }
            divMain.Attributes.Add("prevdate", DateTarget.AddDays(prevDecal).ToString("dd/MM/yyyy"));
            divMain.Attributes.Add("prevlabel", eLibTools.GetCase(EudoQuery.CaseField.CASE_CAPITALIZE, DateTarget.AddDays(prevDecal).ToString("dddd dd MMMM yyyy")));


            divMain.Style.Add(HtmlTextWriterStyle.Width, "100%");

            HtmlGenericControl tabMain = new HtmlGenericControl("table");
            divMain.Controls.Add(tabMain);


            //tabMain.Attributes.Add("class", "cal-tab-main"); // width : 100% - border = 0 cellspacing = 0
            tabMain.ID = "cal_mt_" + _nTab;
            //ELAIZ - demande 76959  - rajout du type de calendrier pour cibler en css
            tabMain.Attributes.Add("period", "day");
            tabMain.Attributes.Add("nbi", ((NbIntervals * NbHourToDisplayRound) + NbIntervalSupplementaires).ToString());
            tabMain.Style.Add(HtmlTextWriterStyle.Height, _globalCalHeight + "px");
            tabMain.Style.Add(HtmlTextWriterStyle.VerticalAlign, "top");
            tabMain.Attributes.Add("onmousemove", "onTabMouseMove(event);");
            tabMain.Attributes.Add("onmousedown", "onTabMouseDown(event);");
            tabMain.Attributes.Add("onmouseup", "onTabMouseUp(event);");
            //Un seul Tr
            HtmlGenericControl trMain = new HtmlGenericControl("tr");
            tabMain.Controls.Add(trMain);
            //EVT javascript
            //tabMain.Attributes.Add("onclick", "cal_fldLClick(event,'DIV');");
            //Premiere colonne - Colonne des heures
            Control colHours = GetHoursColumn(false);
            trMain.Controls.Add(colHours);
            //Boucle sur toutes les journées de la semaine
            int dayIdx = 0;
            if (ViewMode == CalendarViewMode.VIEW_CAL_DAY_PER_USER || ViewMode == CalendarViewMode.VIEW_CAL_DAY)
                CellWidth = CellWidth - 4;

            foreach (ePlanningDay day in _days)
            {
                if (day.DayDate.Date == _calendarDate)
                {
                    HtmlGenericControl tdSep = new HtmlGenericControl("td");
                    tdSep.Attributes.Add("class", "plgcellsep");
                    tdSep.InnerHtml = "&nbsp;";
                    trMain.Controls.Add(tdSep);
                    trMain.Controls.Add(day.GetDayColumn(_pref, dayIdx));
                    dayIdx++;
                    if (dayIdx % 5 == 0)
                        trMain.Controls.Add(GetHoursColumn(false));
                }
            }

            //mode liste
            if (UserDisplayed.Count == 1 && _nTaskMode != CalendarTaskMode.CALENDAR_ITEM_NO_TASK)
            {
                // En mode mixte, EudoQuery construit la requête pour le mode liste (listes des tâches).
                // Pour le mode graphique, construit plus haut, EudoQuery reçoit le paramètre ModeCalendarGraph à "true"
                eListMainRenderer _myMainList = (eListMainRenderer)eRendererFactory.CreateMainListRenderer(_pref, _nTab, nPage, nRows, 0, _allScreenWidth - _globalCalWidth);

                Panel panel;

                if (_myMainList.ErrorMsg.Length == 0)
                {
                    //mode lixte graphique/liste
                    divMain.Attributes.Add("mixtemode", "1");

                    panel = _myMainList.PgContainer;
                    HtmlGenericControl tdSepDay = new HtmlGenericControl("td");
                    tdSepDay.Attributes.Add("class", "tdSepDay");
                    trMain.Controls.Add(tdSepDay);

                    HtmlGenericControl td = new HtmlGenericControl("td");
                    td.Style.Add("width", (_allScreenWidth - _globalCalWidth) + "px");
                    td.Style.Add(HtmlTextWriterStyle.PaddingLeft, "5px");
                    td.Style.Add(HtmlTextWriterStyle.VerticalAlign, "top");

                    td.Controls.Add(panel);
                    trMain.Controls.Add(td);
                }
                else
                    panel = _myMainList.PnlError;

            }

            return divMain;
        }



        /// <summary>
        /// Ajoute des proprité au bouton historique le bouton d'historique
        /// </summary>
        /// <returns></returns>
        public void BuildHistoButton(HtmlGenericControl btnContainer)
        {
            eTools.BuildHistoBtn(this._pref, btnContainer, this._histoInfo.Has, this._histoInfo.Actived, "mainlist");
        }


        /// <summary>
        /// Peuple de le conteneur donnée en paramètre des filtre rapides défini en admin
        /// </summary>
        /// <param name="conteneur"></param>
        public void AddQuickFilter(HtmlTable conteneur)
        {
            Int32 idx = 0;
            if (this._lstQuickFilter == null)
                return;

            // Liste trié par la clé (index de la uservalue)
            SortedList<Int32, Control> controls = new SortedList<Int32, Control>();
            // Construteur du filtre
            eQuickFilter htmlField = new eQuickFilter(null, Pref, null);
            htmlField.MainTable = this.MainTable;
            htmlField.AllQuickFieldFilter = _lstQuickFilter;
            try
            {


                // Construit les listBox
                foreach (Field fld in this._lstQuickFilter)
                {
                    htmlField.SetField = fld;

                    Control ctrl = htmlField.RendQuickFilterList(out idx);
                    if (ctrl != null)
                    {
                        while (controls.ContainsKey(idx))
                            idx++;
                        controls.Add(idx, ctrl);
                    }
                }
            }
            finally
            {
                htmlField.Close();
            }

            HtmlTableRow rowContent = new HtmlTableRow();
            conteneur.Rows.Add(rowContent);
            // Ajoute les listBox dans l'ordre
            foreach (KeyValuePair<Int32, Control> keyValue in controls)
            {
                HtmlTableCell cellContent = new HtmlTableCell();
                rowContent.Cells.Add(cellContent);

                cellContent.Controls.Add(keyValue.Value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private void AddFilterTip(Control divMain)
        {
            FilterTipType lastInfTyp = FilterTipType.NONE;

            if (this._filterTipInfo.Count <= 0)
                return;

            HtmlGenericControl divHidden = new HtmlGenericControl("div");
            divHidden.Style.Add("visibility", "hidden");
            divHidden.Style.Add("display", "none");
            divHidden.ID = String.Concat("hv_", this.MainTable.DescId);
            divMain.Controls.Add(divHidden);


            HtmlGenericControl divTip = new HtmlGenericControl("div");
            divTip.ID = String.Concat("filterTip_", this.MainTable.DescId);
            divTip.Attributes.Add("class", "filterTip");
            divHidden.Controls.Add(divTip);

            foreach (FilterTipInfo inf in this._filterTipInfo)
            {
                if (lastInfTyp != inf.Type)
                {
                    // Libellé de la catégorie
                    switch (inf.Type)
                    {
                        case FilterTipType.MARKEDFILE:
                            AddFilterTipDesc(divTip, eResApp.GetRes(_pref, 5061), 0, inf.Type);
                            break;
                        case FilterTipType.CHARINDEX:
                        case FilterTipType.HISTO:
                            AddFilterTipDesc(divTip, eResApp.GetRes(_pref, 397), 0, inf.Type);
                            break;
                        case FilterTipType.RANDOM:
                        case FilterTipType.DEFAULT:
                            AddFilterTipDesc(divTip, eResApp.GetRes(_pref, 397), 0, inf.Type);
                            break;
                        case FilterTipType.QUICK:
                            AddFilterTipDesc(divTip, eResApp.GetRes(_pref, 727), 0, inf.Type);
                            break;
                        case FilterTipType.EXPRESS:
                            AddFilterTipDesc(divTip, eResApp.GetRes(_pref, 5038), 0, inf.Type);
                            break;
                        case FilterTipType.ADVANCED:
                            AddFilterTipDesc(divTip, eResApp.GetRes(_pref, 6191), 0, inf.Type);
                            break;
                        default:
                            break;
                    }

                    lastInfTyp = inf.Type;
                }

                Boolean activeJs = inf.Type == FilterTipType.MARKEDFILE
                    || inf.Type == FilterTipType.CHARINDEX
                    || inf.Type == FilterTipType.HISTO
                    || inf.Type == FilterTipType.EXPRESS
                    || inf.Type == FilterTipType.ADVANCED;

                // Valeurs
                AddFilterTipDesc(divTip, inf.Label, 1, inf.Type, activeJs, inf.Value, inf.DivId);
            }
        }

        /// <summary>
        /// Ajout une description de filter au parent passé en paramétre avec une simulation de puce
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="innerHtml"></param>
        /// <param name="level"></param>
        /// <param name="filter"></param>
        /// <param name="act"></param>
        /// <param name="value"></param>
        /// <param name="divId"></param>
        private void AddFilterTipDesc(Control parent, String innerHtml, Int32 level, FilterTipType filter, Boolean act = false, String value = "", String divId = "")
        {
            HtmlGenericControl ctrl = eTools.GetHtmlGenericControl("div", String.Concat("&nbsp;", innerHtml));
            ctrl.Attributes.Add("tab", this.MainTable.DescId.ToString());
            ctrl.Attributes.Add("typ", filter.GetHashCode().ToString());
            ctrl.Attributes.Add("lvl", level.ToString());
            ctrl.Attributes.Add("val", value.ToString());
            if (!String.IsNullOrEmpty(divId))
                ctrl.ID = divId;
            ctrl.Attributes.Add("act", act ? "1" : "0");

            parent.Controls.Add(ctrl);
        }



        /// <summary>
        /// TODO - construit la colonne des heures avec tous les intervalles
        /// </summary>
        /// <param name="bRightColumn">Indique si la colonne est affichée à droite de l'écran</param>
        /// <returns></returns>
        private Control GetHoursColumn(bool bRightColumn)
        {
            HtmlGenericControl colHours = new HtmlGenericControl("td");
            colHours.Style.Add(HtmlTextWriterStyle.Width, HOURS_COLUMN_WIDTH + "px");
            colHours.Style.Add(HtmlTextWriterStyle.VerticalAlign, "top");
            colHours.Style.Add(HtmlTextWriterStyle.TextAlign, "right");
            //Ajout de l'entête
            HtmlGenericControl divToAdd = new HtmlGenericControl("div");
            divToAdd.Attributes.Add("class", "DH-cell");
            //texte pour être aligné avec les jours de la semaine
            //if ((ViewMode == CalendarViewMode.VIEW_CAL_DAY_PER_USER) || (ViewMode == CalendarViewMode.VIEW_CAL_DAY))
            //    divToAdd.InnerHtml += "&nbsp;<br/>&nbsp;";
            //else
            divToAdd.InnerHtml = "&nbsp;";

            colHours.Controls.Add(divToAdd);
            //Séparateur
            divToAdd = new HtmlGenericControl("div");
            colHours.Controls.Add(divToAdd);
            divToAdd.Attributes.Add("class", "D-sep");

            //Hauteur du planning
            Int32 nTotal = _globalCalHeight;// _pref.Context.ScreenHeight;


            //Rendu pour chaque heures
            for (int i = 0; i < NbHourToDisplayRound; i++)
            {
                for (int j = 1; j <= NbIntervals; j++)
                {
                    GetHoursColumnInterval(colHours, i, j, bRightColumn);
                }

                divToAdd = new HtmlGenericControl("div");
                colHours.Controls.Add(divToAdd);
                divToAdd.Attributes.Add("class", "D-sep");
            }
            //Si l'heure exact de fin avec minutes dépase l'heure de fin sans les minutes, rajouter le nombres d'intervalles necessaires à les voir visuellement.
            if (NbIntervalSupplementaires > 0)
            {
                for (int j = 1; j <= NbIntervalSupplementaires; j++)
                {
                    GetHoursColumnInterval(colHours, NbHourToDisplayRound, j, bRightColumn);
                }
                divToAdd = new HtmlGenericControl("div");
                colHours.Controls.Add(divToAdd);
                divToAdd.Attributes.Add("class", "D-sep");
            }
            return colHours;
        }
        /// <summary>
        /// Rendu pour un interval d'heure
        /// </summary>
        /// <param name="colHours">colonne des heures ou l'on ajoute le rendu de l'interval</param>
        /// <param name="nCurHour">heure demandée</param>
        /// <param name="nCurInterval">interval demandé</param>
        private void GetHoursColumnInterval(HtmlGenericControl colHours, int nCurHour, int nCurInterval, bool bRightColumn)
        {
            //Heure
            HtmlGenericControl divToAdd = new HtmlGenericControl("div");
            divToAdd.Style.Add(HtmlTextWriterStyle.Height, string.Concat(_nCellHeight, "px"));
            colHours.Controls.Add(divToAdd);
            divToAdd.Attributes.Add("class", "D-h pgc");
            if (nCurInterval == 1)
                divToAdd.InnerText = string.Concat((_dViewHourBegin.Hours + nCurHour).ToString().PadLeft(2, '0'), ":", _dViewHourBegin.Minutes.ToString().PadLeft(2, '0'));


            //Heure en cours

            TimeSpan tsBeginInterval = new TimeSpan(ViewHourBegin.Hours + nCurHour, ViewHourBegin.Minutes + ((nCurInterval - 1) * MinutesInterval), 0);
            TimeSpan tsEndInterval = tsBeginInterval.Add(new TimeSpan(0, MinutesInterval, 0));
            if (DateTime.Now.TimeOfDay >= tsBeginInterval && DateTime.Now.TimeOfDay <= tsEndInterval)
            {
                TimeSpan currentTs = DateTime.Now.TimeOfDay;
                HtmlGenericControl currentTsDiv = new HtmlGenericControl("div");
                currentTsDiv.Attributes.Add("class", bRightColumn ? "right-arrow-now" : "left-arrow-now");
                currentTsDiv.Attributes.Add("title", string.Concat(eResApp.GetRes(_pref, 143), "-", DateTime.Now.ToLongDateString()));
                divToAdd.Controls.Add(currentTsDiv);
                double topCurrentTsDiv = (currentTs - tsBeginInterval).TotalMinutes * ((double)CellHeight / (double)MinutesInterval);
                currentTsDiv.Style.Add(HtmlTextWriterStyle.Top, (Int32)Math.Abs(topCurrentTsDiv) + "px");
            }

            //séparateur
            if (nCurInterval < NbIntervals)
            {
                divToAdd = new HtmlGenericControl("div");
                colHours.Controls.Add(divToAdd);
                divToAdd.Attributes.Add("class", "D-sep-empty");
            }
        }
        #endregion

        #region METHODES STATIQUES


        /// <summary>
        /// Retourne les actions en mode liste
        /// </summary>
        /// <param name="vm">Mode de visualisation du planning</param>
        /// <param name="pref">préférences user</param>
        /// <returns></returns>
        public static Panel GetPlanningActions(CalendarViewMode vm, TimeSpan workHourBegin, ePref pref)
        {
            HtmlGenericControl item, a;
            Panel panel = new Panel();
            panel.ID = "planningModes";
            panel.CssClass = "subTabDiv";
            panel.Attributes.Add("planning_cvm", vm.GetHashCode().ToString());
            if (workHourBegin.Ticks > 0)
                panel.Attributes.Add("planning_whb", workHourBegin.ToString());

            // Jour
            item = new HtmlGenericControl();
            a = new HtmlGenericControl("a");
            a.Attributes.Add("class", "subTab" + ((vm == CalendarViewMode.VIEW_CAL_DAY || vm == CalendarViewMode.VIEW_CAL_DAY_PER_USER) ? " selected" : ""));
            a.Attributes.Add("onclick", String.Concat("setCalViewMode(", EudoQuery.CalendarViewMode.VIEW_CAL_DAY_PER_USER.GetHashCode(), ");"));
            a.InnerText = eResApp.GetRes(pref, 822);
            item.Controls.Add(a);
            panel.Controls.Add(item);

            // Semaine
            item = new HtmlGenericControl();
            a = new HtmlGenericControl("a");
            a.Attributes.Add("class", "subTab" + ((vm == CalendarViewMode.VIEW_CAL_WORK_WEEK) ? " selected" : ""));
            a.Attributes.Add("onclick", String.Concat("setCalViewMode(", EudoQuery.CalendarViewMode.VIEW_CAL_WORK_WEEK.GetHashCode(), ");"));
            a.InnerText = eResApp.GetRes(pref, 821);
            item.Controls.Add(a);
            panel.Controls.Add(item);

            // Mois
            item = new HtmlGenericControl();
            a = new HtmlGenericControl("a");
            a.Attributes.Add("class", "subTab" + ((vm == CalendarViewMode.VIEW_CAL_MONTH) ? " selected" : ""));
            a.Attributes.Add("onclick", String.Concat("setCalViewMode(", EudoQuery.CalendarViewMode.VIEW_CAL_MONTH.GetHashCode(), ");"));
            a.InnerText = eResApp.GetRes(pref, 405);
            item.Controls.Add(a);
            panel.Controls.Add(item);

            // Tâche
            item = new HtmlGenericControl();
            a = new HtmlGenericControl("a");
            a.Attributes.Add("class", "subTab" + ((vm == CalendarViewMode.VIEW_CAL_TASK) ? " selected" : ""));
            a.Attributes.Add("onclick", String.Concat("setCalViewMode(", EudoQuery.CalendarViewMode.VIEW_CAL_TASK.GetHashCode(), ");"));
            a.InnerText = eResApp.GetRes(pref, 842);
            item.Controls.Add(a);
            panel.Controls.Add(item);

            return panel;
        }




        #endregion
    }



    /// <summary>
    /// Objet pour une journée
    /// </summary>
    public class ePlanningDay
    {
        #region Vars et proptriétés
        /// <summary>Objet d'accès à la BDD XRM</summary>
        eudoDAL _dal;
        /// <summary>Objet Calendrier parent</summary>
        ePlanning _parentCalendar;
        /// <summary>Date du jours</summary>
        DateTime _dayDate;
        /// <summary>numéro de jours dans la semaine</summary>
        Int32 _dayRange;
        /// <summary>Liste des RDV appartenant à notre objet journée</summary>
        List<ePlanningItem> _items;
        /// <summary>Utilisateur concerné par la journée en question</summary>
        Int32 _concernedUserId = 0;
        /// <summary>Numéro de la semaine du jours</summary>
        Int32 _weekNumber = 0;

        /// <summary>Liste des RDV appartenant à notre objet journée</summary>
        public List<ePlanningItem> Items
        {
            get { return _items; }
            set { _items = value; }
        }
        /// <summary>Date du jours</summary>
        public DateTime DayDate
        {
            get { return _dayDate; }
            set { _dayDate = value; }
        }
        /// <summary>numéro de jours dans la semaine</summary>
        public Int32 DayRange
        {
            get { return _dayRange; }
            set { _dayRange = value; }
        }
        /// <summary>Objet Calendrier parent</summary>
        public ePlanning ParentCalendar
        {
            get { return _parentCalendar; }
            set { _parentCalendar = value; }
        }
        /// <summary>Utilisateur concerné par la journée en question</summary>
        public Int32 ConcernedUserId
        {
            get { return _concernedUserId; }
            set { _concernedUserId = value; }
        }
        /// <summary>Numéro de la semaine du jours</summary>
        public Int32 WeekNumber
        {
            get { return _weekNumber; }
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dal">Objet d'accès à la BDD XRM</param>
        /// <param name="cal">objet ePlanning parent</param>
        /// <param name="dayRange">numéro de jours dans la semaine</param>
        /// <param name="dayDate">Date du jours courant</param>
        /// <param name="concernedUserId">Utilisateur de la plage du jours</param>
        public ePlanningDay(eudoDAL dal, ePlanning cal, Int32 dayRange, DateTime dayDate, Int32 concernedUserId)
        {
            _dal = dal;
            _dayDate = dayDate;
            _dayRange = dayRange;
            _parentCalendar = cal;
            _items = new List<ePlanningItem>();
            _concernedUserId = concernedUserId;
            _weekNumber = ePlanning.GetWeekOfYear(dayDate);
        }

        /// <summary>
        /// Récupère le rendu d'une journée pour le mode mois
        /// </summary>
        /// <param name="pref">Objet des pref du l'user en cours XRM</param>
        /// <returns>Le control de rendu</returns>
        internal Control GetDayForMonthRender(ePref pref)
        {
            HtmlGenericControl tabDay = new HtmlGenericControl("table");
            tabDay.Attributes.Add("eDay", DayDate.Day.ToString());

            HtmlGenericControl trDay = new HtmlGenericControl("tr");
            tabDay.Controls.Add(trDay);
            tabDay.Attributes.Add("empty", "1");    //MARKER indiquant que la journée n'a pas de RDV
            //Nombre d'heure à afficher
            //Int32 nNbHourToDisplay = (Int32)(_parentCelndar.MonthViewHourEnd - _parentCelndar.MonthViewHourBegin).TotalHours;

            //Nombre d'intervalles sur la plage donnée
            //_nNbIntervals = (int)Math.Ceiling((decimal)nNbHourToDisplay / (_parentCelndar.MonthMinutesInterval / 60));

            ParentCalendar.CellWidth = (100 / ParentCalendar.NbIntervals);    //Taille en pct d'un intervalle de jours (dépend du nombre d'intervals)
            HtmlGenericControl tdHour;
            //Création des intervalles
            for (int idxInterval = 0; idxInterval < ParentCalendar.NbIntervals; idxInterval++)
            {
                tdHour = new HtmlGenericControl("td");
                tdHour.Attributes["class"] = "PMD"; //class de cellule vide
                trDay.Controls.Add(tdHour);
                //Ajout de l'intervalle
                if (!_parentCalendar.DivIntervals.ContainsKey(String.Concat(ConcernedUserId, "_", _dayDate.Day, "_", idxInterval)))
                    _parentCalendar.DivIntervals.Add(String.Concat(ConcernedUserId, "_", _dayDate.Day, "_", idxInterval), tdHour);
            }
            //ajout des élements
            //encapsulation 
            setItemsPosition();
            //Affectation des éléments à leur position dans la journée
            foreach (ePlanningItem thisItm in _items)
            {
                if (!String.IsNullOrEmpty(thisItm.ParentIntervalId))    //Se produit lorque un RDV n'est pas comprit dans la plage horaire visible par l'utilisateur
                    foreach (string sId in thisItm.ParentIntervalId.Split(";"))
                    {
                        if (!ParentCalendar.DivIntervals.ContainsKey(sId)) //TODO : ce produit dans certains cas mais lequel !
                            continue;

                        tdHour = ParentCalendar.DivIntervals[sId];  //Recupération de l'interval
                        if (String.IsNullOrEmpty(tdHour.Attributes["fid"])) //Si intervalle non affecté on l'initialise pour un RDV
                        {
                            #region FACTORISATION des STYLES en CSS courtes
                            string ClassName;
                            string col = String.Concat("background-color:", thisItm.Color);
                            //on récupère la classe si déjà existante sinon on ajoute la CSS à la liste de CSS
                            if (ParentCalendar.CSS.ContainsKey(col))
                                ClassName = ParentCalendar.CSS[col];
                            else
                            {
                                ClassName = string.Concat("pl", ParentCalendar.CSS.Count);
                                ParentCalendar.CSS.Add(col, ClassName);
                            }
                            tdHour.Attributes["class"] = ClassName;    //Attribuer le CSS calculé du RDV
                            #endregion
                            tdHour.Attributes.Add("fid", thisItm.FileId.ToString());    //FileId 
                        }
                        else if (!String.Concat(";", tdHour.Attributes["fid"], ";").Contains(String.Concat(";", thisItm.FileId.ToString(), ";")))
                        {   //Si intervalle déja affecté, on place l'intervalle en conflit et ajoute le FileId de la fiche en conflit.
                            tdHour.Attributes["fid"] += String.Concat(";", thisItm.FileId.ToString());
                            tdHour.Attributes["class"] += " plM-C";
                            tdHour.Style.Remove("background-color");
                        }

                        ((HtmlGenericControl)tdHour.Parent.Parent).Attributes.Remove("empty");  //Suppression du MARKER indiquant que la journée est vide du tableau

                        HtmlContainerControl inputInfos = new HtmlGenericControl("input");
                        inputInfos.Attributes.Add("type", "hidden");
                        inputInfos.Attributes.Add("fid", thisItm.FileId.ToString());    //FileId
                        inputInfos.Attributes.Add("_db", thisItm.DateBegin.ToString("dd-MM-yyyy-HH-mm-ss"));
                        inputInfos.Attributes.Add("_de", thisItm.DateEnd.ToString("dd-MM-yyyy-HH-mm-ss"));
                        inputInfos.Attributes.Add("owner", thisItm.OwnerId);
                        inputInfos.Attributes.Add("multiowner", thisItm.MultiOwnerId);
                        tdHour.Controls.Add(inputInfos);
                    }
            }
            if (!String.IsNullOrEmpty(tabDay.Attributes["empty"]))
                tabDay = null;  //Si tous les intervalles sont vide dans la journée alors ne pas renvoyer les intervalles (à l'aide du MARKER)
            else
                tabDay.Attributes.Remove("empty");  //Sinon supprimé le MARKER car n'est plus utile par la suite

            return tabDay;
        }

        internal Control GetDayColumn(ePref pref, Int32 dayIdx)
        {
            HtmlGenericControl colDay = new HtmlGenericControl("td");
            colDay.ID = String.Concat(ConcernedUserId, "_", dayIdx);
            colDay.Attributes.Add("onmouseout", "OCMO(this);");
            colDay.Attributes.Add("onmousemove", "OCMM(this);");
            colDay.Attributes.Add("dIdx", dayIdx.ToString());
            colDay.Attributes.Add("date", DayDate.ToString("dd-MM-yyyy") + "-" + _parentCalendar.ViewHourBegin.Hours + "-" + _parentCalendar.ViewHourBegin.Minutes);
            colDay.Attributes.Add("dRange", DayRange.ToString());

            //colDay.Style.Add(HtmlTextWriterStyle.Width, _parentCelndar.CellWidth + "px");
            //colDay.Style.Add(HtmlTextWriterStyle.VerticalAlign, "top");

            foreach (string s in String.Concat("width:", _parentCalendar.CellWidth + "px", SEPARATOR.LVL1, "vertical-align:", "top").Split(SEPARATOR.LVL1))    //une CSS courte par style
            {
                #region Récupération ou création de la classe pour le style correspondant et affectation du style
                string ClassName;
                if (ParentCalendar.CSS.ContainsKey(s))
                    ClassName = ParentCalendar.CSS[s];
                else
                {
                    ClassName = string.Concat("pl", (ParentCalendar.CSS.Count));
                    ParentCalendar.CSS.Add(s, ClassName);
                }
                if (colDay.Attributes["class"] == null)
                    colDay.Attributes.Add("class", "");
                if (!colDay.Attributes["class"].Contains(String.Concat(" ", ClassName, " ")))
                    colDay.Attributes["class"] += String.Concat(" ", ClassName);    //Attribuer le CSS calculé du RDV
                #endregion
            }

            //Ajout de l'entête
            HtmlGenericControl divToAdd = new HtmlGenericControl("div");
            divToAdd.Attributes.Add("class", "phd");
            divToAdd.Style.Add("width", _parentCalendar.CellWidth + "px");

            List<String> tmpUsrLst = new List<String>();
            tmpUsrLst.Add(ConcernedUserId.ToString());
            StringBuilder sbError = new StringBuilder();
            List<eUser.UserListItem> ULI = eModelTools.GetUser(_dal, tmpUsrLst, pref, true, false, ref sbError);

            try
            {
                if (ParentCalendar.UserDisplayed.Count > 1)
                {
                    divToAdd.Attributes.Add("onclick", String.Concat("setCalViewMode(", EudoQuery.CalendarViewMode.VIEW_CAL_DAY_PER_USER.GetHashCode(), ", ",
                                                    ParentCalendar.Tab.ToString(), ", ",
                                                    "null, ",
                                                    "'", DayDate.ToString("dd/MM/yyyy"), "');"));
                }
                else if (ParentCalendar.UserDisplayed.Count == 1)
                {
                    //ALISTER => Demande 81 250 utilisation de CalendarViewMode.VIEW_CAL_DAY_PER_USER au lieu de CalendarViewMode.VIEW_CAL_DAY
                    //pour garder le même affichage que lorsqu'on clique sur le mode jour directement (sans passer par semaine)
                    divToAdd.Attributes.Add("onclick", String.Concat("setCalViewMode(", EudoQuery.CalendarViewMode.VIEW_CAL_DAY_PER_USER.GetHashCode(), ", ",
                                                   ParentCalendar.Tab.ToString(), ", ",
                                                   ParentCalendar.UserDisplayed[0], ", ",
                                                   "'", DayDate.ToString("dd/MM/yyyy"), "');"));
                }
            }
            catch (Exception) { }


            if (_parentCalendar.ViewMode == CalendarViewMode.VIEW_CAL_DAY || _parentCalendar.ViewMode == CalendarViewMode.VIEW_CAL_DAY_PER_USER)
            {
                if (ULI.Count > 0)
                {
                    divToAdd.InnerHtml = ULI[0].Libelle;
                }
            }
            else
            {
                divToAdd.InnerHtml = eLibTools.DisplayDate(pref, (Int32)DayDate.DayOfWeek, DayDate.Day, DayDate.Month);
            }

            colDay.Controls.Add(divToAdd);
            //Séparateur
            divToAdd = new HtmlGenericControl("div");
            colDay.Controls.Add(divToAdd);
            divToAdd.Attributes.Add("class", "D-sep");


            //Nombre d'intervalles par heure
            //_nNbIntervals = 60 / _parentCelndar.MinutesInterval;

            int idxInterval = 0;
            //Rendu de chaques heures et chaques intervalles
            for (int i = 0; i < ParentCalendar.NbHourToDisplayRound; i++)
            {
                for (int j = 1; j <= ParentCalendar.NbIntervals; j++)
                {
                    GetDayIntervalRender(pref, dayIdx, colDay, ref idxInterval, i, j);
                }
                divToAdd = new HtmlGenericControl("div");
                colDay.Controls.Add(divToAdd);
                divToAdd.Attributes.Add("class", "D-sep");
            }
            //Si l'heure exact de fin avec minutes dépase l'heure de fin sans les minutes, rajouter le nombres d'intervalles necessaires à les voir visuellement.
            if (ParentCalendar.NbIntervalSupplementaires > 0)
            {
                for (int j = 1; j <= ParentCalendar.NbIntervalSupplementaires; j++)
                {
                    GetDayIntervalRender(pref, dayIdx, colDay, ref idxInterval, ParentCalendar.NbHourToDisplayRound, j);
                }
            }

            //ajout des élements

            List<string> addedIntervalsItems = new List<string>();
            List<ePlanningItem> itemsToRemove = new List<ePlanningItem>();

            setItemsPosition();
            foreach (ePlanningItem thisItm in _items)
            {
                if (addedIntervalsItems.Contains(String.Concat(thisItm.ParentIntervalId, "_", thisItm.DivElement.ID)))
                {
                    continue;
                }
                addedIntervalsItems.Add(String.Concat(thisItm.ParentIntervalId, "_", thisItm.DivElement.ID));

                thisItm.DivElement.Attributes.Add("_db", thisItm.DateBegin.ToString("dd-MM-yyyy-HH-mm-ss"));
                thisItm.DivElement.Attributes.Add("_de", thisItm.DateEnd.ToString("dd-MM-yyyy-HH-mm-ss"));
                thisItm.DivElement.Attributes.Add("owner", thisItm.OwnerId);

                //TODO : Verifier la construction de DivInternals et que faire tous les traitements de la boucle
                //  est bien nécessaire si on a pas ParentCalendar.DivIntervals.ContainsKey(thisItm.ParentIntervalId)
                if (ParentCalendar.DivIntervals.ContainsKey(thisItm.ParentIntervalId))
                    thisItm.DivElement.Attributes.Add("pint", ParentCalendar.DivIntervals[thisItm.ParentIntervalId].ID);
                thisItm.DivElement.Attributes.Add("multiowner", thisItm.MultiOwnerId);
                thisItm.DivElement.Attributes.Add("mov", thisItm.IsMovable ? "1" : "0");



                if (thisItm.Left <= 0)
                    thisItm.DivElement.Style.Add("visibility", "hidden");
                else
                {
                    int nWidth = ((int)_parentCalendar.CellWidth - 10) / thisItm.MaxLeft;
                    int nDayLeft = 0;
                    int nLeft = nDayLeft + (thisItm.Left - 1) * nWidth;
                    thisItm.DivElement.Style.Add(HtmlTextWriterStyle.Width, (nWidth - 10) + "px");
                    thisItm.DivElement.Style.Add(HtmlTextWriterStyle.Left, nLeft + "px");
                    #region FACTORISATION des style en CSS court
                    //Récupération des style factorisé en CSS
                    string col = thisItm.DivElement.Style[HtmlTextWriterStyle.BackgroundColor];
                    string width = thisItm.DivElement.Style[HtmlTextWriterStyle.Width];
                    string left = thisItm.DivElement.Style[HtmlTextWriterStyle.Left];
                    string height = thisItm.DivElement.Style[HtmlTextWriterStyle.Height];
                    foreach (string s in String.Concat("background-color:", col, SEPARATOR.LVL1, "width:", width, SEPARATOR.LVL1, "left:", left, SEPARATOR.LVL1, "height:", height).Split(SEPARATOR.LVL1))    //une CSS courte par style
                    {
                        #region Récupération ou création de la classe pour le style correspondant et affectation du style
                        string ClassName;
                        if (ParentCalendar.CSS.ContainsKey(s))
                            ClassName = ParentCalendar.CSS[s];
                        else
                        {
                            ClassName = string.Concat("pl", (ParentCalendar.CSS.Count));
                            ParentCalendar.CSS.Add(s, ClassName);
                        }
                        if (!thisItm.DivElement.Attributes["class"].Contains(String.Concat(" ", ClassName, " ")))
                            thisItm.DivElement.Attributes["class"] += String.Concat(" ", ClassName);    //Attribuer le CSS calculé du RDV
                        #endregion
                    }
                    //suppression des style factorisé en CSS
                    thisItm.DivElement.Style.Remove(HtmlTextWriterStyle.BackgroundColor);
                    thisItm.DivElement.Style.Remove(HtmlTextWriterStyle.Width);
                    thisItm.DivElement.Style.Remove(HtmlTextWriterStyle.Left);
                    thisItm.DivElement.Style.Remove(HtmlTextWriterStyle.Height);
                    #endregion
                }

                if (ParentCalendar.DivIntervals.ContainsKey(thisItm.ParentIntervalId))
                    ParentCalendar.DivIntervals[thisItm.ParentIntervalId].Controls.Add(thisItm.DivElement);

            }

            return colDay;
        }

        /// <summary>
        /// Rendu d'un interval d'heure de RDV
        /// </summary>
        /// <param name="pref">pref</param>
        /// <param name="dayIdx">interval de jours demandée</param>
        /// <param name="colDay">element ou est ajouté l'interval</param>
        /// <param name="idxInterval">dernier interval ajouté</param>
        /// <param name="nCurHour">interval d'heure demandée</param>
        /// <param name="nCurInterval">interval demandée</param>
        private void GetDayIntervalRender(ePref pref, Int32 dayIdx, HtmlGenericControl colDay, ref int idxInterval, int nCurHour, int nCurInterval)
        {
            HtmlGenericControl divToAdd = new HtmlGenericControl("div");

            //Heure
            divToAdd = new HtmlGenericControl("div");
            divToAdd.Style.Add(HtmlTextWriterStyle.Height, string.Concat(_parentCalendar.CellHeight, "px"));
            colDay.Controls.Add(divToAdd);
            if (_parentCalendar.ViewHourBegin.Add(new TimeSpan(nCurHour, 0, 0)) < _parentCalendar.WorkHourBegin || _parentCalendar.ViewHourBegin.Add(new TimeSpan(nCurHour, 0, 0)) >= _parentCalendar.WorkHourEnd)
                divToAdd.Attributes.Add("class", "i-D_b pgc");
            else
                divToAdd.Attributes.Add("class", "i-D pgc");
            divToAdd.ID = string.Concat(_concernedUserId, "_", dayIdx, "_", idxInterval);
            divToAdd.Attributes.Add("isinter", "1");
            //if (j == 1)
            //divToAdd.InnerHtml = "&nbsp;";

            //Ajout de l'intervalle
            if (!_parentCalendar.DivIntervals.ContainsKey(String.Concat(ConcernedUserId, "_", _dayRange, "_", idxInterval)))
                _parentCalendar.DivIntervals.Add(String.Concat(ConcernedUserId, "_", _dayRange, "_", idxInterval), divToAdd);
            idxInterval++;

            //Heure en cours
            if (_dayDate.Date == DateTime.Now.Date)
            {
                TimeSpan tsBeginInterval = new TimeSpan(ParentCalendar.ViewHourBegin.Hours + nCurHour, ParentCalendar.ViewHourBegin.Minutes + ((nCurInterval - 1) * ParentCalendar.MinutesInterval), 0);
                TimeSpan tsEndInterval = tsBeginInterval.Add(new TimeSpan(0, ParentCalendar.MinutesInterval, 0));
                if (DateTime.Now.TimeOfDay >= tsBeginInterval && DateTime.Now.TimeOfDay <= tsEndInterval)
                {
                    TimeSpan currentTs = DateTime.Now.TimeOfDay;
                    HtmlGenericControl currentTsDiv = new HtmlGenericControl("div");
                    currentTsDiv.Attributes.Add("class", "bar-now");
                    divToAdd.Controls.Add(currentTsDiv);
                    currentTsDiv.Attributes.Add("title", string.Concat(eResApp.GetRes(pref, 143), "-", DateTime.Now.ToLongDateString()));
                    currentTsDiv.Style.Add(HtmlTextWriterStyle.Width, _parentCalendar.CellWidth + "px");
                    double topCurrentTsDiv = (currentTs - tsBeginInterval).TotalMinutes * ((double)_parentCalendar.CellHeight / (double)_parentCalendar.MinutesInterval);
                    currentTsDiv.Style.Add(HtmlTextWriterStyle.Top, (Int32)Math.Abs(topCurrentTsDiv) + "px");
                }

            }

            //séparateur
            if (nCurInterval < ParentCalendar.NbIntervals)
            {
                divToAdd = new HtmlGenericControl("div");
                colDay.Controls.Add(divToAdd);
                divToAdd.Attributes.Add("class", "D-sep-dot");
            }
        }

        /// <summary>
        /// Positionne les éléments
        /// </summary>
        private void setItemsPosition()
        {


            List<Int32> aPosBusy = new List<int>();


            // Vide les positions et les positions max de chaque item
            for (var i = 0; i < this.Items.Count; i++)
            {
                var itm = this.Items[i];
                itm.Left = 0;
                itm.MaxLeft = 0;
            }

            for (int nPos = 0; nPos <= ParentCalendar.CalendarItemOverLap; nPos++)
                aPosBusy.Add(0);


            // Recherche les positions pour chaque item
            for (int nCurIdx = 0; nCurIdx < this.Items.Count; nCurIdx++)
            {
                var itm = this.Items[nCurIdx];

                //if (!itm.isVisible)
                //    continue;

                int nCntPosBusy = 0;
                int nCurMaxPos = 0;

                // Tableau des item matcher par l'item en cours
                List<ePlanningItem> aItemMatch = new List<ePlanningItem>();

                for (int nPos = 1; nPos <= ParentCalendar.CalendarItemOverLap; nPos++)
                    aPosBusy[nPos] = 0;

                // Recherche les positions déjà utilisé pour l'item en cours
                for (int nI = 0; nI < this.Items.Count; nI++)
                {
                    var itmBis = this.Items[nI];
                    // Sauf l'Item en cours et si l'item a déjà été positionné
                    if (nCurIdx != nI && itmBis.Left > 0)
                    {
                        if (itm.IsOverLapWith(itmBis))
                        {
                            // Tableau des item matcher
                            aItemMatch.Add(itmBis);
                            // Compte le nb de position occupé
                            nCntPosBusy++;
                            // Enregistre la position occupé
                            aPosBusy[itmBis.Left] = 1;
                            // Recupère la position max
                            if (nCurMaxPos < itmBis.Left)
                                nCurMaxPos = itmBis.Left;
                        }
                    }

                    if (nCntPosBusy >= ParentCalendar.CalendarItemOverLap)
                        break;
                }

                // Indique pour l'item en cours la position libre pour lui même (-1 : aucunes disponibles de position)
                if (nCntPosBusy >= ParentCalendar.CalendarItemOverLap)
                {
                    itm.Left = -1;
                }
                else
                {
                    for (int nPos = 1; nPos <= ParentCalendar.CalendarItemOverLap; nPos++)
                    {
                        if (aPosBusy[nPos] == 0)
                        {
                            itm.Left = nPos;

                            // Reprendre le position en cours si c'est la position max
                            if (nCurMaxPos < nPos)
                                nCurMaxPos = nPos;
                            // Ajout l'item en cours
                            aItemMatch.Add(itm);
                            // Réinitialise les pos max des items matcher et de l'item en cours
                            for (int nI = 0; nI < aItemMatch.Count; nI++)
                            {
                                var itmBis2 = aItemMatch[nI];
                                if (itmBis2.MaxLeft < nCurMaxPos)
                                    itmBis2.MaxLeft = nCurMaxPos;
                            }
                            break;
                        }
                    }
                }
            }

            // Recherche les positions maximal pour chaque item
            for (int nCurIdx = 0; nCurIdx < this.Items.Count; nCurIdx++)
            {
                var itm = this.Items[nCurIdx];
                int nPosMax = 0;

                for (int nI = 0; nI < this.Items.Count; nI++)
                {
                    var itmBis3 = this.Items[nI];
                    // Si l'item en cours ou Sauf l'Item en cours et match avec l'item de parcouru
                    if (nCurIdx == nI || (nCurIdx != nI && itm.IsOverLapWith(itmBis3)))
                    {
                        if (nPosMax < itmBis3.MaxLeft)
                            nPosMax = itmBis3.MaxLeft;

                        if (nPosMax >= ParentCalendar.CalendarItemOverLap)
                            break;
                    }
                }

                itm.MaxLeft = nPosMax;
            }
        }
    }
}