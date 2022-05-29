using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Com.Eudonet.Xrm
{

    /// <summary>
    /// Liste principale
    /// </summary>
    public partial class eMainList : eEudoPage
    {
        /// <summary>Un champ de recherche sur le champ principal est disponible</summary>
        Boolean _bViewSearchField = false;

        /// <summary>objet de rendu du mode liste</summary>
        public eListMainRenderer _myMainList;

        /// <summary>
        /// Table de la liste
        /// </summary>
        public Int32 _nTab = 0;

        /// <summary>
        /// HTML du tooltip des filtres de la liste
        /// </summary>
        public String _HtmlFilterTip = String.Empty;


        /// <summary>Date de début pour le début de semaine</summary>
        public DateTime dateBeginWeek = DateTime.Now;

        //HashSet<String> _allKeys;
        Int32 height;
        Int32 width;

        /// <summary>
        /// masque le bouton de sélection
        /// </summary>
        public bool bHideActionSelection = false;

        /// <summary>Planning mode graphique</summary>
        public bool isCalendarGraphEnabled;
        /// <summary>Mode Planning</summary>
        public bool isPlanning = false;

        /// <summary>Planning activé</summary>
        public bool isCalendarEnabled = false;

        /// <summary>Planning - Mode Jours</summary>
        public bool isDayMode = false;
        /// <summary>Planning - Mode Jours et tache</summary>
        public bool isMixedMode = false;
        /// <summary>Planning - Mode Jours Utilisateur multiple</summary>
        public bool isMultiUsersDayMode = false;
        /// <summary>Planning - Mode Semaine</summary>
        public bool isWeekMode = false;
        /// <summary>Planning - Mode Mois</summary>
        public bool isMonthMode = false;

        public String addedCss = string.Empty;
        public String sCalHeadNavCss = string.Empty;

        // Nombre de fiche marquées 
        public Int32 nbMarkedFile = 0;


        #region accesseurs

        /// <summary>Un champ de recherche sur le champ principal est disponible</summary>
        public Boolean ViewSearchField
        {
            get { return _bViewSearchField; }

        }

        #endregion

        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return null;
        }

        /// <summary>
        /// Chargement de la page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            _allKeys = new HashSet<String>(Request.Form.AllKeys);

            height = eConst.DEFAULT_WINDOW_HEIGHT;
            width = eConst.DEFAULT_WINDOW_WIDTH;

            if (_allKeys.Contains("divW") && !String.IsNullOrEmpty(Request.Form["divW"]))
                Int32.TryParse(Request.Form["divW"].ToString(), out width);

            if (_allKeys.Contains("divH") && !String.IsNullOrEmpty(Request.Form["divH"]))
                Int32.TryParse(Request.Form["divH"].ToString(), out height);

            // Chargement de param de session
            SECURITY_GROUP securityGroup = _pref.GroupMode;

            // par défaut accueil - TODO : VERIFIER SI LA PAGE PAR DEFAUT NE PEUT PAS ETRE PERSONNALISEE

            // Table
            if (!_allKeys.Contains("tab") || !Int32.TryParse(Request.Form["tab"].ToString(), out _nTab))
            {
                //TODO - supprimer cette ligne en fin de tests
                //_nTab = 200;

                ErrorContainer = eErrorContainer.GetDevUserError(
                eLibConst.MSG_TYPE.CRITICAL,
                eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                eResApp.GetRes(_pref, 72),  //   titre
                String.Concat("Erreur sur eMainList.aspx - Page_Load :Paramètre table non fourni =  ")

                );
                if (_bFromeUpdater)
                    LaunchError();
                else
                    LaunchErrorHTML(true);
            }

            //Mode d'affichage
            eudoDAL dal = eLibTools.GetEudoDAL(_pref);
            try
            {
                dal.OpenDatabase();

                TableLite tab = null;
                try
                {
                    tab = eLibTools.GetTableInfo(dal, _nTab, TableLite.Factory());
                }
                catch (Exception exp)
                {
                    dal.CloseDatabase();

                    ErrorContainer = eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                        String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                        eResApp.GetRes(_pref, 72),  //   titre
                        String.Concat("Erreur sur eMainList.aspx - Page_Load : récupération du table lite ntab = ->", _nTab, "<- \n", exp.Message)
                    );

                    if (_bFromeUpdater)
                        LaunchError();
                    else
                        LaunchErrorHTML(true);
                }

                bHideActionSelection = tab.TabType == TableType.PJ;

                isPlanning = tab.EdnType == EdnType.FILE_PLANNING;
                isCalendarEnabled = isPlanning && _pref.GetPref(_nTab, eConst.PREF_PREF.CALENDARENABLED).Equals("1");

                CalendarViewMode calViewMode = isCalendarEnabled ? (CalendarViewMode)eLibTools.GetNum(_pref.GetPref(_nTab, eConst.PREF_PREF.VIEWMODE)) : EudoQuery.CalendarViewMode.VIEW_CAL_LIST;
                CalendarTaskMode calTaskMode = (CalendarTaskMode)eLibTools.GetNum(_pref.GetPref(_nTab, eConst.PREF_PREF.CALENDARTASKMODE));
                Int32 nMenuUserId = eLibTools.GetNum(_pref.GetPref(_nTab, eConst.PREF_PREF.MENUUSERID));

                if (isCalendarEnabled && (calViewMode == CalendarViewMode.VIEW_CAL_DAY || calViewMode == CalendarViewMode.VIEW_CAL_WORK_WEEK || calViewMode == CalendarViewMode.VIEW_CAL_MONTH || calViewMode == CalendarViewMode.VIEW_CAL_DAY_PER_USER))
                    isCalendarGraphEnabled = true;

                if (isCalendarEnabled)
                {
                    if (calViewMode == CalendarViewMode.VIEW_CAL_DAY || calViewMode == CalendarViewMode.VIEW_CAL_DAY_PER_USER)
                        isDayMode = true;

                    if (calViewMode == CalendarViewMode.VIEW_CAL_WORK_WEEK)
                        isWeekMode = true;
                    if (calViewMode == CalendarViewMode.VIEW_CAL_MONTH)
                        isMonthMode = true;
                }

                //Menu
                bool isFilterList = false;
                if (Request.Form["type"] != null)
                    isFilterList = Request.Form["type"].Equals("filter");

                //Menu actions
                if (isPlanning && !isFilterList && isCalendarEnabled)
                    PlanningAction.Controls.Add(ePlanning.GetPlanningActions(calViewMode, new TimeSpan(0), _pref));

                //Type eudonet de la table
                Int32 ednType = 0;
                if (_allKeys.Contains("edntype"))
                    Int32.TryParse(Request.Form["edntype"].ToString(), out ednType); // Page

                //Pour l'instant, pas de gestion "fine" des erreurs sur le dolist/doplanning
                //  sauf pour la recherche de la préférence de sélection et le CreateMainListRenderer de dolist
                try
                {
                    if (!isCalendarGraphEnabled || isFilterList)
                    {
                        DoWebTab(ednType);
                        DoList(dal);
                    }
                    else
                    {
                        DoPlanning(dal, calViewMode);
                    }

                    //MOU-30/04/2014 nombre de fiche cochées
                    MarkedFilesSelection ms = null;
                    _pref.Context.MarkedFiles.TryGetValue(this._nTab, out ms);
                    if (ms != null)
                        this.nbMarkedFile = ms.NbFiles;
                }
                catch (eEndResponseException) { Response.End(); }
                catch (ThreadAbortException) { }
                catch (Exception e1)
                {
                    //Avec exception
                    String sDevMsg = String.Concat("Erreur sur la page : ", System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine);
                    sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Exception Message : ", e1.Message, Environment.NewLine, "Exception StackTrace :", e1.StackTrace);

                    ErrorContainer = eErrorContainer.GetDevUserError(
                       eLibConst.MSG_TYPE.CRITICAL,
                       eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                       String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                       eResApp.GetRes(_pref, 72),  //   titre
                       String.Concat(sDevMsg));

                    if (_bFromeUpdater)
                        LaunchError();
                    else
                        LaunchErrorHTML(true);
                }
            }
            finally
            {
                dal.CloseDatabase();
            }
        }

        /// <summary>
        /// Fait un rendu de l'onglet web/ des sous-onglet
        /// </summary>
        /// <param name="nType"></param>
        private void DoWebTab(int nType)
        {
            // Dev 38096 demandes parante: onglet web
            var subMenuRenderer = new eWebTabSubMenuRenderer(_pref, _nTab, nType, true);
            if (subMenuRenderer.Init())
            {
                if (subMenuRenderer.HasItems())
                    subMenuRenderer.Build(this.SubTabMenuCtnr);
            }
            else
            {
                subMenuRenderer.sError += "Impossible d'initialiser le renderer eWebTabSubMenuRenderer";
            }

            if (subMenuRenderer.sError.Length > 0)
            {
                //Avec exception
                String sDevMsg = String.Concat("Erreur sur la page : ", System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine);

                sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Exception Message : ", subMenuRenderer.sError, Environment.NewLine, "Exception StackTrace :", subMenuRenderer.innerException.StackTrace);

                ErrorContainer = eErrorContainer.GetDevUserError(
                   eLibConst.MSG_TYPE.CRITICAL,
                   eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                   String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                   eResApp.GetRes(_pref, 72),  //   titre
                   String.Concat(sDevMsg));


                if (_bFromeUpdater)
                    LaunchError();
            }
        }

        /// <summary>
        /// Crée le planning en mode semaine
        /// </summary>
        private void DoPlanning(eudoDAL dal, CalendarViewMode calViewMode)
        {
            width = width - 100;
            ePlanning cal = new ePlanning(dal, _pref, _nTab, width, height, null);
            if (cal.ViewMode == CalendarViewMode.VIEW_CAL_WORK_WEEK)
                dateBeginWeek = cal.FirstDateOfWeek;
            else
                dateBeginWeek = cal.DateTarget;

            if ((cal.ViewMode == CalendarViewMode.VIEW_CAL_DAY || cal.ViewMode == CalendarViewMode.VIEW_CAL_DAY_PER_USER) && cal.UserDisplayed.Count > 1)
                isMultiUsersDayMode = true;

            if ((cal.ViewMode == CalendarViewMode.VIEW_CAL_DAY || cal.ViewMode == CalendarViewMode.VIEW_CAL_DAY_PER_USER || cal.ViewMode == CalendarViewMode.VIEW_CAL_TASK) && !isMultiUsersDayMode)
                isMixedMode = true;

            // RaZ compteur de page
            _pref.Context.Paging.Tab = _nTab;
            _pref.Context.Paging.resetInfo();

            Int32 nRows = 0;
            Int32 nPage = 0;
            if (_allKeys.Contains("page"))
                Int32.TryParse(Request.Form["page"].ToString(), out nPage); // Page

            if (_allKeys.Contains("rows"))
                Int32.TryParse(Request.Form["rows"].ToString(), out nRows); // Rows

            if (nPage < 1)
                nPage = 1;

            // Ajout de l'information "Heure de début de la journée de travail" sur le contrôle d'entête de Planning (boutons de choix mode Jour/Semaine/Mois/Tâche)
            // Cela évitera à ePlanningFileRenderer de faire une requête EudoQuery (via un new ePlanning()) uniquement pour récupérer cette information.
            // Le renderer sera toutefois obligé de le faire pour les cas où on crée une fiche Planning à partir d'une page n'ayant pas eu besoin de faire appel à ePlanning
            // (exemple : mode Liste - le DoList() ne fait pas appel à cette fonction, et donc, ne récupère pas l'information)
            // CRU : Ceci n'est plus utilisé à priori
            //if (PlanningAction != null)
            //{
            //    Control planningActionPanel = PlanningAction.FindControl("plgAct");
            //    if (planningActionPanel != null && planningActionPanel.HasControls() && planningActionPanel.Controls[0].HasControls())
            //    {
            //        // Récupération du contrôle sur lequel ajouter l'attribut - hiérarchie : PlanningAction > Panel > Table > Rows (cF. ePlanning.GetPlanningActions)
            //        Control planningActionRow = planningActionPanel.Controls[0].FindControl("planningActionRow");
            //        if (planningActionRow != null && planningActionRow is HtmlTableRow)
            //            ((HtmlTableRow)planningActionRow).Attributes.Add("planning_whb", cal.WorkHourBegin.ToString());
            //    }
            //}

            HtmlGenericControl calRender = new HtmlGenericControl("div");
            HtmlGenericControl mainDiv = null;

            // de dimanche a samedi avec eudores.app.id=44->dimanche
            Int32 ResIdDay = 44;
            Int32 ResIdMonth = 31;

            String beginDayName = eResApp.GetRes(_pref.LangId, ResIdDay + (Int32)dateBeginWeek.DayOfWeek);
            String beginMonthName = eResApp.GetRes(_pref.LangId, ResIdMonth + dateBeginWeek.Month);

            String weekLabelText = String.Empty;

            switch (calViewMode)
            {
                case CalendarViewMode.VIEW_CAL_WORK_WEEK:

                    DateTime dateEndWeek = cal.Days[cal.Days.Count - 1].DayDate;

                    String endDayName = eResApp.GetRes(_pref.LangId, ResIdDay + (Int32)dateEndWeek.DayOfWeek);
                    String endMonthName = eResApp.GetRes(_pref.LangId, ResIdMonth + dateEndWeek.Month);

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

                    mainDiv = cal.GetWeekRender();
                    break;
                case CalendarViewMode.VIEW_CAL_DAY:
                case CalendarViewMode.VIEW_CAL_DAY_PER_USER:
                    weekLabelText = String.Concat(beginDayName.Substring(0, 3), ". ", dateBeginWeek.Day, " ", beginMonthName, " ", dateBeginWeek.ToString("yyyy"));
                    mainDiv = cal.GetDayRender(nRows, nPage);
                    break;
                case CalendarViewMode.VIEW_CAL_MONTH:
                    weekLabelText = String.Concat(beginMonthName, " ", dateBeginWeek.ToString("yyyy"));
                    mainDiv = cal.GetMonthMultUserRender(dal);
                    break;
            }

            if (weekLabelText.Length > 0)
                WeekLabel.InnerHtml = eLibTools.GetCase(CaseField.CASE_CAPITALIZE, weekLabelText);

            if (mainDiv != null)
                calRender.Controls.Add(mainDiv);

            HtmlInputHidden fldCss = new HtmlInputHidden();
            fldCss.ID = "CalendarCustomCss";
            fldCss.Value = cal.GetCss();
            listContent.Controls.Add(fldCss);
            listContent.Controls.Add(calRender);

            //Flêches de navigation date

            //Mode jour  - liste des tâches
            // Boutton de navigation
            String naviFctParam = _nTab.ToString();

            idFirst.Attributes.Add("onclick", String.Concat("firstpage(", naviFctParam, ", true)"));
            clsFirstB.Attributes.Add("onclick", String.Concat("firstpage(", naviFctParam, ", true)"));

            idPrev.Attributes.Add("onclick", String.Concat("prevpage(", naviFctParam, ", true)"));
            clsPrevB.Attributes.Add("onclick", String.Concat("prevpage(", naviFctParam, ", true)"));

            idNext.Attributes.Add("onclick", String.Concat("nextpage(", naviFctParam, ", true)"));

            // #39926 CRU : Si on n'est pas sur la dernière page, l'icône next ne doit pas être "actif"
            if (nPage < _pref.Context.Paging.NbPage)
                idNext.Style.Add("background-color", _pref.ThemeXRM.Color);

            clsNextB.Attributes.Add("onclick", String.Concat("nextpage(", naviFctParam, ", true)"));

            idLast.Attributes.Add("onclick", String.Concat("lastpage(", naviFctParam, ", true)"));
            clsLastB.Attributes.Add("onclick", String.Concat("lastpage(", naviFctParam, ", true)"));

            inputNumpage.Attributes.Add("onblur", String.Concat("selectpage(", naviFctParam, ",this, true)"));
            inputNumpageB.Attributes.Add("onblur", String.Concat("selectpage(", naviFctParam, ",this, true)"));
            inputNumpage.Attributes.Add("onkeypress", String.Concat("if(isValidationKey(event))selectpage(", naviFctParam, ",this, true);"));
            inputNumpageB.Attributes.Add("onkeypress", String.Concat("if(isValidationKey(event))selectpage(", naviFctParam, ",this, true);"));

            SpanLibElem.InnerText = eLibTools.GetPrefName(_pref, _nTab);//, " ", eResApp.GetRes(_pref.Lang, 5085), " ", _pref.User.UserDisplayName, _pref.Context.Filters.ContainsKey(_nTab) && _pref.Context.Filters[_nTab] != null ? string.Concat(" - ", _pref.Context.Filters[_nTab].FilterName) : string.Empty);
            SpanNbElem.InnerText = string.Empty;

            // ajoute au bouton historique, les propriétés du mode liste du planning
            cal.BuildHistoButton(histoFilter);

            eRightFilter eRF = new eRightFilter(_pref);
            if (eRF.HasRight(eLibConst.TREATID.FILTER))
            {
                bool canEditFilter = true;
                if (_pref.Context.Filters.ContainsKey(_nTab))
                {
                    FilterSel advFilterSel = _pref.Context.Filters[_nTab];
                    AdvFltMenu.Attributes["onclick"] = String.Concat("dispFltMenu(this,true,", advFilterSel.FilterSelId, ",'", advFilterSel.FilterName.Replace("'", @"\'"), "');");
                    AdvFltMenu.Attributes["class"] += " advFltActiv ";

                    AdvFilter advFilter = new AdvFilter(_pref, advFilterSel.FilterSelId);
                    string sError = String.Empty;
                    advFilter.Load(dal, out sError);
                    if (String.IsNullOrEmpty(sError))
                        canEditFilter = advFilter.FilterType != TypeFilter.DBL;
                    else
                        canEditFilter = false;
                }
                else
                {
                    AdvFltMenu.Attributes["onclick"] = String.Concat("dispFltMenu(this,false);");
                }
                AdvFltMenu.Attributes["onmouseover"] = AdvFltMenu.Attributes["onclick"];
                AdvFltMenu.Attributes.Add("ca", eRF.CanAddNewItem() ? "1" : "0");
                AdvFltMenu.Attributes.Add("ce", eRF.CanEditItem() && canEditFilter ? "1" : "0");

            }
            else
            {
                AdvFltMenu.Attributes["onclick"] = String.Concat("eAlert(3, top._res_6834, top._res_6837);");
            }

            cal.AddQuickFilter(listQuickFilters);

            if (isMixedMode)
                sCalHeadNavCss = "Style=\"margin-left:15%\""; ;
        }




        /// <summary>
        /// Crée le mode liste
        /// </summary>
        private void DoList(eudoDAL dal)
        {
            // 
            _pref.Context.Paging.Tab = _nTab;
            //charagement des descids des colonnes affichées.
            try
            {
                Dictionary<eConst.PREF_SELECTION, String> dicoSel = _pref.GetSelection(_nTab, new eConst.PREF_SELECTION[] { eConst.PREF_SELECTION.LISTCOL });
                if (dicoSel.ContainsKey(eConst.PREF_SELECTION.LISTCOL))
                    _pref.Context.DisplayedFields = dicoSel[eConst.PREF_SELECTION.LISTCOL];
            }
            catch (Exception e1)
            {
                //Avec exception
                String sDevMsg = String.Concat("Erreur sur la page : ", System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine);


                sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Exception Message : ", e1.Message, Environment.NewLine, "Exception StackTrace :", e1.StackTrace);

                ErrorContainer = eErrorContainer.GetDevUserError(
                   eLibConst.MSG_TYPE.CRITICAL,
                   eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                   String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                   eResApp.GetRes(_pref, 72),  //   titre
                   String.Concat(sDevMsg));


                if (_bFromeUpdater)
                    LaunchError();
                else
                    LaunchErrorHTML(true);
            }

            Int32 nRows = 0;
            Int32 nPage = 0;
            if (_requestTools.AllKeys.Contains("page"))
                Int32.TryParse(Request.Form["page"].ToString(), out nPage); // Page

            if (_requestTools.AllKeys.Contains("rows"))
                Int32.TryParse(Request.Form["rows"].ToString(), out nRows); // Rows

            if (nPage < 1)
                nPage = 1;

            DateTime dtStartMainList = DateTime.Now;

            _myMainList = (eListMainRenderer)eRendererFactory.CreateMainListRenderer(_pref, _nTab, nPage, nRows, height, width);
            //_myMainList.PgContainer.Attributes.Add("timegen", (DateTime.Now - dtStartMainList).TotalMilliseconds.ToString());

            Panel panel;

            if (_myMainList.ErrorMsg.Length == 0 || _myMainList.ErrorNumber == QueryErrorType.ERROR_NUM_FILTER_NOT_AVAILABLE)
                panel = _myMainList.PgContainer;
            else
            {
                Exception e1 = null;
                if (_myMainList.InnerException != null)
                    e1 = _myMainList.InnerException;

                //Avec exception
                String sDevMsg = String.Concat("Erreur sur la page : ", System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine);

                if (e1 != null)
                {
                    sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Message Exception : ", e1.Message,
                        Environment.NewLine, "Exception StackTrace :", e1.StackTrace
                        );
                }
                sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Error Message : ", _myMainList.ErrorMsg);

                ErrorContainer = eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                    String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                    eResApp.GetRes(_pref, 72),  //   titre
                    String.Concat(sDevMsg)
                    );

                if (_bFromeUpdater)
                    LaunchError();
                else
                    LaunchErrorHTML(true);

                return; // return pour éviter l'erreur de compilation sur la ligne "listContent.Controls.Add(panel);"
            }

            listContent.Controls.Add(panel);

            if (_myMainList.ErrorNumber == QueryErrorType.ERROR_NUM_FILTER_NOT_AVAILABLE)
            {
                // #35140 : Si filtre non disponible, on affiche le message suivant :
                SpanLibElem.InnerHtml = String.Concat(eResApp.GetRes(_pref, 815), ". <a href='#' onclick=\"cancelAdvFlt(", _nTab, ")\">", eResApp.GetRes(_pref, 1179), "</a>.");
                listheader.Visible = false;
            }
            else
            {
                Control ctrlCharIndex = _myMainList.GetCharIndex();
                if (ctrlCharIndex == null)
                    fltindex.Style["visibility"] = "hidden";
                else
                {
                    fltindex.Controls.Add(_myMainList.GetCharIndex());
                    fltindex.Style["visibility"] = "";
                }

                String filterName = String.Empty;
                if (_pref.Context.Filters.ContainsKey(_nTab) && _pref.Context.Filters[_nTab] != null)
                {
                    // Affichage de la description du filtre
                    string error = string.Empty;
                    string filterDescription = AdvFilter.GetDescription(_pref, _pref.Context.Filters[_nTab].FilterSelId, out filterName, out error);
                    if (String.IsNullOrEmpty(error))
                    {
                        eTools.DisplayFilterTooltip(SpanLibElem, filterDescription);
                    }
                    filterName = String.Concat(" - ", filterName);
                }

                SpanLibElem.InnerText = string.Concat(_myMainList.GetMainTableLibelle(), filterName);
                SpanNbElem.InnerText = "0";



                // On ajoute des propriétés au bouton Historique en fonction de  liste en cours
                _myMainList.BuildHistoButton(this.histoFilter);

                eRightFilter eRF = new eRightFilter(_pref);
                if (eRF.HasRight(eLibConst.TREATID.FILTER))
                {
                    bool canEditFilter = true;
                    if (_pref.Context.Filters.ContainsKey(_nTab))
                    {
                        FilterSel advFilterSel = _pref.Context.Filters[_nTab];
                        AdvFltMenu.Attributes["onclick"] = String.Concat("dispFltMenu(this,true,", advFilterSel.FilterSelId, ",'", advFilterSel.FilterName.Replace("'", @"\'"), "');");
                        AdvFltMenu.Attributes["class"] += " advFltActiv ";

                        AdvFilter advFilter = new AdvFilter(_pref, advFilterSel.FilterSelId);
                        string sError = String.Empty;
                        advFilter.Load(dal, out sError);
                        if (String.IsNullOrEmpty(sError))
                            canEditFilter = advFilter.FilterType != TypeFilter.DBL;
                        else
                            canEditFilter = false;
                    }
                    else
                    {
                        AdvFltMenu.Attributes["onclick"] = String.Concat("dispFltMenu(this,false);");
                    }
                    AdvFltMenu.Attributes["onmouseover"] = AdvFltMenu.Attributes["onclick"];
                    AdvFltMenu.Attributes.Add("ca", eRF.CanAddNewItem() ? "1" : "0");
                    AdvFltMenu.Attributes.Add("ce", eRF.CanEditItem() && canEditFilter ? "1" : "0");
                }
                else
                {
                    AdvFltMenu.Attributes["onclick"] = String.Concat("eAlert(3, top._res_6834, top._res_6837);");
                }

                //Affichage du champ de recherche
                if (!_myMainList.DrawSearchField)
                {
                    //TODO : a voir si on ne peut pas mettre le visible false coté serveur pour éviter du rendu inutile
                    eFS.Style.Add("display", "none");
                    //eFS.Visible = false;
                }


                _myMainList.AddQuickFilter(listQuickFilters);

                // Boutton de navigation
                String naviFctParam = _nTab.ToString();

                idFirst.Attributes.Add("onclick", String.Concat("firstpage(", naviFctParam, ")"));
                clsFirstB.Attributes.Add("onclick", String.Concat("firstpage(", naviFctParam, ")"));

                idPrev.Attributes.Add("onclick", String.Concat("prevpage(", naviFctParam, ")"));
                clsPrevB.Attributes.Add("onclick", String.Concat("prevpage(", naviFctParam, ")"));

                idNext.Attributes.Add("onclick", String.Concat("nextpage(", naviFctParam, ")"));
                clsNextB.Attributes.Add("onclick", String.Concat("nextpage(", naviFctParam, ")"));

                idLast.Attributes.Add("onclick", String.Concat("lastpage(", naviFctParam, ")"));
                clsLastB.Attributes.Add("onclick", String.Concat("lastpage(", naviFctParam, ")"));

                inputNumpage.Attributes.Add("onblur", String.Concat("selectpage(", naviFctParam, ",this)"));
                inputNumpageB.Attributes.Add("onblur", String.Concat("selectpage(", naviFctParam, ",this)"));
                inputNumpage.Attributes.Add("onkeypress", String.Concat("if(isValidationKey(event))selectpage(", naviFctParam, ",this);"));
                inputNumpageB.Attributes.Add("onkeypress", String.Concat("if(isValidationKey(event))selectpage(", naviFctParam, ",this);"));

            }


        }


    }
}