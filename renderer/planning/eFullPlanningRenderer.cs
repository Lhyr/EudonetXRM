using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Classe de rendu des modes plannings
    /// Il s'agit d'un "transfert" basique depuis eFullListRenderer pour eviter
    /// le mélange des spécificité de planning dans une classe générique
    /// TODO : Cette classe est à revoir :
    ///   > La classe "ePlanning" fait le rendu graphique et mélange couche métier/rendu/dal, ce qui rend le tout peu maniable/évolutif
    ///   voir méthode "BuildPlanningAction"
    ///   Cela entraine également un accès aux données dans le rendu, ce qu'il faudrait limiter
    /// </summary>
    public class eFullPlanningRenderer : eFullMainListRenderer
    {

        /// <summary>
        /// Le mode "calendar" est effectivement actif.
        /// Il arrive que ce ne soit pas le cas, la table ayant été créé en type "calendar"
        /// uniquement pour bénéficier de l'historiation. Ce n'est plus nécessaire depuis
        /// x années mais il reste des bases clients utilisant cela
        /// </summary>
        bool _bIsCalendarEnabled = false;

        /// <summary>
        /// Calendrier graphique
        /// </summary>
        bool _bIsCalendarGraphEnabled = false;

        /// <summary>
        /// Mixte d'un mode graphique et d'une liste 
        /// </summary>
        bool _bIsMixedMode = false;

        /// <summary>
        /// Mode jour
        /// </summary>
        bool _bIsDayMode = false;


        /// <summary>
        /// Plusieurs utilisateurs sélectionné : change l'affichage des modes graphiques
        /// </summary>
        bool _bisMultiUsersDayMode = false;


        CalendarViewMode _calViewMode = CalendarViewMode.VIEW_UNDEFIED;


        CalendarTaskMode _calTaskMode = CalendarTaskMode.CALENDAR_ITEM_NO_TASK;

        ePlanning _cal = null;


        /// <summary>
        /// Constructeur standard
        /// </summary>
        /// <param name="pref">Pref uer</param>
        /// <param name="nTab">DescId de la table planning</param>
        /// <param name="height">hauteur fenêtre</param>
        /// <param name="width">Largeur Fenêtre</param>
        /// <param name="nPage">Numéro de page</param>
        /// <param name="nRow">Ligne par page</param>
        private eFullPlanningRenderer(ePref pref, int nTab, int height, int width, int nPage, int nRow) : base(pref, nTab, height, width, nPage, nRow)
        {
        }


        /// <summary>
        /// Date de début jour de la semaine
        /// </summary>
        public DateTime dateBeginWeek = DateTime.Now;


        /// <summary>
        /// Instancie un planning renderer
        /// </summary>
        /// <param name="pref">prf user</param>
        /// <param name="height">hauteur de la fenetre d'affichage</param>
        /// <param name="width">largeur fenêtre d'affichage</param>
        /// <param name="nPage">Numéro de page</param>
        /// <param name="nRow">Ligne par page</param>
        /// <param name="tab">Table lite préchargé</param>
        /// <returns></returns>
        public static eFullPlanningRenderer GetFullPlanningRenderer(ePref pref, int height, int width, int nPage, int nRow, TableLite tab)
        {
            eFullPlanningRenderer rdr = new eFullPlanningRenderer(pref, tab.DescId, height, width, nPage, nRow);
            rdr._ednTab = tab;
            return rdr;
        }


        #region override des méthodes standard

        /// <summary>
        /// bouton action
        /// Dans le cas mode graphique,  bouton de paging entre jours
        /// </summary>
        /// <param name="tr1"></param>
        protected override void BuildAction(TableRow tr1)
        {
            if (_bIsCalendarGraphEnabled || _bIsDayMode)
            {
                //Pasd de bouton d'action sur les agendas graphique
                _bHideActionSelection = true;

                TableCell tcCal = new TableCell();
                tr1.Cells.Add(tcCal);

                Panel pCalHeader = new Panel();
                tcCal.Controls.Add(pCalHeader);
                pCalHeader.ID = "calendarMainHeader";
                pCalHeader.CssClass = "calheader";

                Panel pPlHt = new Panel();
                pCalHeader.Controls.Add(pPlHt);
                pPlHt.CssClass = "pl_ht";
                if (_bIsMixedMode)
                    pPlHt.Style.Add("margin-left", "15%");

                System.Web.UI.WebControls.Table tb2 = new System.Web.UI.WebControls.Table();
                pPlHt.Controls.Add(tb2);

                TableRow tr = new TableRow();
                tr.TableSection = TableRowSection.TableBody;
                tb2.Rows.Add(tr);

                //Btn Set Date today
                TableCell tcCal2 = new TableCell();
                tcCal2.HorizontalAlign = HorizontalAlign.Right;
                tr.Cells.Add(tcCal2);
                HtmlGenericControl spanToday = new HtmlGenericControl("span");
                tcCal2.Controls.Add(spanToday);
                spanToday.Attributes.Add("onclick", "setCalendarDate('" + _nTab + "','" + DateTime.Now + "');");
                spanToday.Attributes.Add("class", "head-today");
                spanToday.InnerHtml = eResApp.GetRes(Pref, 143);


                //Prev date
                TableCell tcCal3 = new TableCell();
                tr.Cells.Add(tcCal3);
                Panel divD1 = new Panel();
                divD1.ID = "DIV1";
                tcCal3.Controls.Add(divD1);
                divD1.Attributes.Add("onclick", "setPrevCalDate();");
                divD1.Attributes.Add("class", "icon-edn-prev fLeft icnListAct");

                //Week Label
                TableCell tcWeekLabel = new TableCell();
                tcWeekLabel.HorizontalAlign = HorizontalAlign.Center;
                tr.Cells.Add(tcWeekLabel);
                HtmlGenericControl spanWeekLabel = new HtmlGenericControl("span");
                spanWeekLabel.ID = "WeekLabel";
                spanWeekLabel.Attributes.Add("class", "head_title-text");
                tcWeekLabel.Controls.Add(spanWeekLabel);


                //Next date
                TableCell tcNextDate = new TableCell();
                tr.Cells.Add(tcNextDate);
                Panel divD2 = new Panel();
                divD2.ID = "DIV2";
                tcNextDate.Controls.Add(divD2);
                divD2.Attributes.Add("onclick", "setNextCalDate();");
                divD2.Attributes.Add("class", "icon-edn-next fRight icnListAct");

                TableCell tcVide = new TableCell();
                tr.Cells.Add(tcVide);
                tcVide.Text = "&nbsp;";


            }

            //Autres boutons
            base.BuildAction(tr1);
        }


        /// <summary>
        /// Ajoute le pagging pour certains mode de planning
        /// </summary>
        protected override void AddPagging(TableRow tr1, bool withForceReload = false)
        {
            if (!_bIsCalendarGraphEnabled || _bIsMixedMode)
                base.AddPagging(tr1, _bIsCalendarGraphEnabled);
        }


        /// <summary>
        /// Filtre rapide planning
        /// > Le planning graphique n'utilise pas un renderer "classique" et n'a donc pas accès
        /// au fonctionnaligé "classiqu".
        /// Pour éviter comme pour les version &lt; 402 de construre un renderer complet uniquement pour ces fonctions,
        /// on utilise/étend les propriété de ePlanning existante
        /// TODO : Refaire tout ça avec un renderer classique !!
        /// </summary>
        /// <param name="tbQuickFilter"></param>
        protected override void AddQuickFilter(HtmlControl tbQuickFilter)
        {
            if (_bIsCalendarGraphEnabled)
                _cal.AddQuickFilter((HtmlTable)tbQuickFilter);
            else
                base.AddQuickFilter(tbQuickFilter);

        }


        /// <summary>
        /// Label de la liste
        /// </summary>
        /// <param name="element"></param>
        protected override void ContentLabel(HtmlGenericControl element)
        {

            {
                String filterName = String.Empty;
                if (Pref.Context.Filters.ContainsKey(_nTab) && Pref.Context.Filters[_nTab] != null)
                {
                    // Affichage de la description du filtre
                    string error = string.Empty;
                    string filterDescription = AdvFilter.GetDescription(Pref, Pref.Context.Filters[_nTab].FilterSelId, out filterName, out error);
                    if (String.IsNullOrEmpty(error))
                    {
                        eTools.DisplayFilterTooltip(element, filterDescription);
                    }
                    filterName = String.Concat(" - ", filterName);
                }


                element.InnerText = string.Concat(_cal?.MainTable?.Libelle ?? "", filterName);
            }
        }


        /// <summary>
        /// Initialisation du renderer de planning
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            // RaZ compteur de page
            Pref.Context.Paging.Tab = _nTab;
            Pref.Context.Paging.resetInfo();


            _width -= 100; //taille pour le calendrier du menu : TODO devrait être en css, pas en dur

            _bIsCalendarEnabled = Pref.GetPref(_nTab, ePrefConst.PREF_PREF.CALENDARENABLED).Equals("1");


            _calViewMode = _bIsCalendarEnabled ? (EudoQuery.CalendarViewMode)eLibTools.GetNum(Pref.GetPref(_nTab, ePrefConst.PREF_PREF.VIEWMODE)) : EudoQuery.CalendarViewMode.VIEW_CAL_LIST;

            _calTaskMode = (EudoQuery.CalendarTaskMode)eLibTools.GetNum(Pref.GetPref(_nTab, ePrefConst.PREF_PREF.CALENDARTASKMODE));


            _bIsCalendarGraphEnabled = _bIsCalendarEnabled &&
                            (_calViewMode == CalendarViewMode.VIEW_CAL_DAY
                                || _calViewMode == CalendarViewMode.VIEW_CAL_WORK_WEEK
                                || _calViewMode == CalendarViewMode.VIEW_CAL_MONTH
                                || _calViewMode == CalendarViewMode.VIEW_CAL_DAY_PER_USER);


            _bIsDayMode = _bIsCalendarEnabled && (
                        _calViewMode == CalendarViewMode.VIEW_CAL_DAY
                        || _calViewMode == CalendarViewMode.VIEW_CAL_DAY_PER_USER);


            //Mode graphique
            if (_bIsCalendarGraphEnabled)
            {

                //Dans le cas de planning, la classe ePlanning est à la fois un objet métier et un objet de rendu
                // l'essentiel de la construction est dans le constructeur, il faut donc être prudent, son utilisation
                // peut être très couteuse (par défaut, l'appel a new va exécuter une requête sur le mode courrant du planning....)
                eudoDAL dal = eLibTools.GetEudoDAL(Pref);
                try
                {
                    dal.OpenDatabase();
                    _cal = new ePlanning(dal, Pref, _nTab, _width, _height, null);
                }
                finally
                {
                    dal?.CloseDatabase();
                }


                if (_cal.ViewMode == CalendarViewMode.VIEW_CAL_WORK_WEEK)
                    dateBeginWeek = _cal.FirstDateOfWeek;
                else
                    dateBeginWeek = _cal.DateTarget;

                if ((_cal.ViewMode == CalendarViewMode.VIEW_CAL_DAY || _cal.ViewMode == CalendarViewMode.VIEW_CAL_DAY_PER_USER) && _cal.UserDisplayed.Count > 1)
                    _bisMultiUsersDayMode = true;

                if ((_cal.ViewMode == CalendarViewMode.VIEW_CAL_DAY || _cal.ViewMode == CalendarViewMode.VIEW_CAL_DAY_PER_USER || _cal.ViewMode == CalendarViewMode.VIEW_CAL_TASK) && !_bisMultiUsersDayMode)
                    _bIsMixedMode = true;


            }
            else
            {
                //Mode liste
                return base.Init();
            }

            return true;

        }


        /// <summary>
        /// Construction du divinfos
        /// </summary>
        /// <param name="p"></param>
        protected override void BuildDivInfos(Panel p)
        {

            //Menu actions
            if (_bIsCalendarEnabled)
            {
                TimeSpan workHourBegin = (_cal != null ? _cal.WorkHourBegin : new TimeSpan(0));
                p.Controls.Add(ePlanning.GetPlanningActions(_calViewMode, workHourBegin, Pref));
            }
        }

        /// <summary>
        /// Pour les planning, pas de champ de recherche
        /// </summary>
        /// <param name="tr1"></param>
        protected override void AddSearchBtn(TableRow tr1)
        {
            TableCell tcSearch = new TableCell();
            tr1.Cells.Add(tcSearch);

            tcSearch.Controls.Add(new LiteralControl("&nbsp;"));
        }

        /// <summary>
        /// //pas de bouton historique
        /// </summary>
        /// <param name="tc2"></param>
        protected override void AddBtnHisto(HtmlControl tc2)
        {
            if (!_bIsCalendarGraphEnabled || _bIsMixedMode)
                base.AddBtnHisto(tc2);

        }

        /// <summary>
        /// Création du rendu de planning
        /// </summary>
        /// <param name="pnl"></param>
        protected override bool AddSpecialListContent(Panel pnl)
        {
            BuildPlanningAction(pnl);

            // #75 981 - ELAIZ/MABBE - Ajout d'un attribut pour cibler en CSS le planning graphique depuis des éléments parents de CalDivMain
            if (_bIsCalendarGraphEnabled)
                pnl.Attributes.Add("contenttype", "calendar");

            return !_bIsCalendarGraphEnabled;
        }
        #endregion




        /// <summary>
        /// Construction des boutons du mode planning ET DES RENDERER GRAPHIQUE
        /// </summary>
        /// <param name="pListContent">Contenaire du rendu</param>
        protected void BuildPlanningAction(Panel pListContent)
        {

            HtmlGenericControl calRender = new HtmlGenericControl("div");
            HtmlGenericControl mainDiv = null;

            // de dimanche a samedi avec eudores.app.id=44->dimanche
            Int32 ResIdDay = 44;
            Int32 ResIdMonth = 31;

            String beginDayName = eResApp.GetRes(Pref, ResIdDay + (Int32)dateBeginWeek.DayOfWeek);
            String beginMonthName = eResApp.GetRes(Pref, ResIdMonth + dateBeginWeek.Month);

            String weekLabelText = String.Empty;

            switch (_calViewMode)
            {
                case CalendarViewMode.VIEW_CAL_WORK_WEEK:

                    DateTime dateEndWeek = _cal.Days[_cal.Days.Count - 1].DayDate;

                    String endDayName = eResApp.GetRes(Pref.LangId, ResIdDay + (Int32)dateEndWeek.DayOfWeek);
                    String endMonthName = eResApp.GetRes(Pref.LangId, ResIdMonth + dateEndWeek.Month);

                    if (dateEndWeek.Year != dateBeginWeek.Year)
                    {
                        weekLabelText = String.Concat(dateBeginWeek.Day, " ", beginMonthName, " ", dateBeginWeek.ToString("yyyy"), " - ", dateEndWeek.Day, " ", endMonthName, " ", dateEndWeek.ToString("yyyy"));
                    }
                    else if (dateEndWeek.Month != dateBeginWeek.Month)
                    {
                        weekLabelText = String.Concat(dateBeginWeek.Day, " ", beginMonthName, " - ", dateEndWeek.Day, " ", endMonthName);
                    }
                    else
                    {
                        weekLabelText = String.Concat(dateBeginWeek.Day, " - ", dateEndWeek.Day, " ", endMonthName);
                    }


                    mainDiv = _cal.GetWeekRender();
                    break;
                case CalendarViewMode.VIEW_CAL_DAY:
                case CalendarViewMode.VIEW_CAL_DAY_PER_USER:
                    weekLabelText = String.Concat(eLibTools.DisplayDate(Pref, (Int32)dateBeginWeek.DayOfWeek, dateBeginWeek.Day, dateBeginWeek.Month, true), " ", dateBeginWeek.ToString("yyyy"));
                    mainDiv = _cal.GetDayRender(_nRow, _nPage);
                    break;
                case CalendarViewMode.VIEW_CAL_MONTH:
                    weekLabelText = String.Concat(beginMonthName, " ", dateBeginWeek.ToString("yyyy"));
                    eudoDAL dal = eLibTools.GetEudoDAL(Pref);
                    try
                    {
                        dal.OpenDatabase();
                        mainDiv = _cal.GetMonthMultUserRender(dal);
                    }
                    catch
                    {
                        throw;
                    }
                    finally
                    {
                        dal?.CloseDatabase();
                    }
                    break;
            }

            if (weekLabelText.Length > 0)
            {
                Control WeekLabel = FindControlRecursive(PgContainer, "WeekLabel");

                if (WeekLabel != null)
                    ((HtmlGenericControl)WeekLabel).InnerHtml = eLibTools.GetCase(EudoQuery.CaseField.CASE_CAPITALIZE, weekLabelText);

            }


            if (mainDiv != null)
                calRender.Controls.Add(mainDiv);

            HtmlInputHidden fldCss = new HtmlInputHidden();
            fldCss.ID = "CalendarCustomCss";
            fldCss.Value = _cal?.GetCss() ?? "";
            pListContent.Controls.Add(fldCss);
            pListContent.Controls.Add(calRender);

        }


    }
}