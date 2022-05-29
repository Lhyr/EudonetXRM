using Com.Eudonet.Internal;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    internal class eFilterRenderer
    {
        /// <summary>
        /// Operations du wizard manager des filtres
        /// </summary>
        internal class WizardManager
        {


            internal static HtmlGenericControl GetEmptyLineValues(ePrefUser pref, AdvFilter filter, bool bFromTreat, eFilterRendererParams param = null)
            {
                if (param == null)
                    param = new eFilterRendererParams();

                HtmlGenericControl div = new HtmlGenericControl("div");
                eudoDAL dal = eLibTools.GetEudoDAL(pref);

                try
                {
                    dal.OpenDatabase();
                    AdvFilterContext filterContext = new AdvFilterContext(pref, dal, filter);
                    div = (HtmlGenericControl)eFilterLineRenderer.GetValuesList(filterContext, new AdvFilterLineIndex(0, 0), bFromTreat, param);
                }
                finally
                {
                    dal?.CloseDatabase();
                }

                return div;
            }

            internal static string GetEmptyTab(ePrefUser pref, AdvFilter filter, bool onlyFilesOpt)
            {
                StringBuilder sb = new StringBuilder();
                eudoDAL dal = eLibTools.GetEudoDAL(pref);

                try
                {
                    dal.OpenDatabase();

                    AdvFilterContext filterContext = new AdvFilterContext(pref, dal, filter);

                    sb.Append(eTools.GetControlRender(eFilterTabRenderer.GetTabRenderer(filterContext, new AdvFilterTabIndex(0), onlyFilesOpt)));
                }
                finally
                {
                    dal?.CloseDatabase();
                }

                return sb.ToString();
            }

            internal static string GetEmptyLine(ePrefUser pref, AdvFilter filter, bool withEndOperator, bool fromAdmin = false)
            {
                StringBuilder sb = new StringBuilder();
                eudoDAL dal = eLibTools.GetEudoDAL(pref);

                try
                {
                    AdvFilterTabIndex filterTabIndex = new AdvFilterTabIndex(0);
                    AdvFilterLineIndex filterLineIndex = new AdvFilterLineIndex(0, 0);

                    dal.OpenDatabase();

                    AdvFilterContext filterContext = new AdvFilterContext(pref, dal, filter, fromAdmin);

                    sb.Append(eTools.GetControlRender(eFilterLineRenderer.GetLineRenderer(filterContext, filterLineIndex)));

                    if (withEndOperator)
                    {
                        sb.Append(eTools.GetControlRender(eFilterLineRenderer.GetInterLineOperator(filterContext, filterLineIndex, true)));

                        sb.Append(eTools.GetControlRender(eFilterTabRenderer.GetGroupByBlock(filterContext, filterTabIndex, false)));
                    }
                }
                finally
                {
                    dal?.CloseDatabase();
                }

                return sb.ToString();
            }


            internal static HtmlGenericControl GetHtmlEMptyLine(ePrefUser pref, AdvFilter filter, bool withEndOperator, eFilterRendererParams param = null)
            {
                if (param == null)
                    param = new eFilterRendererParams();

                bool bFromChart = true;
                HtmlGenericControl div = new HtmlGenericControl("div");
                div.Style.Add(HtmlTextWriterStyle.Display, "inline-block");
                eudoDAL dal = eLibTools.GetEudoDAL(pref);
                try
                {
                    AdvFilterTabIndex filterTabIndex = new AdvFilterTabIndex(0);
                    AdvFilterLineIndex filterLineIndex = new AdvFilterLineIndex(0, 0);

                    dal.OpenDatabase();

                    AdvFilterContext filterContext = new AdvFilterContext(pref, dal, filter);
                    div.Controls.Add(eFilterLineRenderer.GetLineRenderer(filterContext, filterLineIndex, lineReportClass: "", param: param));

                    if (withEndOperator)
                    {
                        div.Controls.Add(eFilterLineRenderer.GetInterLineOperator(filterContext, filterLineIndex, true));
                        div.Controls.Add(eFilterTabRenderer.GetGroupByBlock(filterContext, filterTabIndex, false));
                    }

                }
                finally
                {
                    dal?.CloseDatabase();
                }

                return div;
            }
        }

        /// <summary>
        /// Operations du wizard des filtres
        /// </summary>
        internal class Wizard
        {
            internal static HtmlGenericControl GetNewFilterRender(ePref pref, TypeFilter filterType, int filterTab, bool adminMode = false, String filterName = "", XrmWidgetType widgetType = XrmWidgetType.Unknown)
            {
                // On simule un filtre vide
                AdvFilter filter = AdvFilter.GetNewFilter(pref, filterType, filterTab);
                if (filterName != "")
                    filter.FilterName = filterName;
                // Ajout d'un tab vide
                filter.FilterTabs.Add(AdvFilterTab.GetNewEmpty(0, filter.FilterTab.ToString()));

                eudoDAL dal = eLibTools.GetEudoDAL(pref);

                try
                {
                    dal.OpenDatabase();

                    AdvFilterContext filterContext = new AdvFilterContext(pref, dal, filter, adminMode);
                    if (widgetType == XrmWidgetType.Kanban)
                        filterContext.FilterIntro = eResApp.GetRes(pref, 8480);

                    return eFilterRenderer.GetFilterRender(filterContext);
                }
                finally
                {
                    dal?.CloseDatabase();
                }
            }

            /// <summary>
            /// Construit et renvoie le Div contenant le rendu HTML du filtre pour l'assistant création
            /// </summary>
            /// <param name="pref">Préférences de l'utilisateur en cours</param>
            /// <param name="filterId">id du filter a charger</param>
            /// <returns>Div contenant le rendu HTML</returns>
            internal static HtmlGenericControl GetFilterRender(ePref pref, int filterId, bool adminMode = false, XrmWidgetType widgetType = XrmWidgetType.Unknown)
            {
                string error = String.Empty;
                eudoDAL dal = eLibTools.GetEudoDAL(pref);

                try
                {
                    dal.OpenDatabase();

                    AdvFilter filter = new AdvFilter(pref, filterId);
                    // Si filterid = 0 => nouveau filtre, inutile de le charger
                    if (filterId > 0)
                    {
                        if (!filter.Load(dal, out error))
                            throw new Exception(error);
                    }


                    if (!filter.IsViewable || !filter.IsUpdatable || (filter.FilterType == TypeFilter.WIDGET && !adminMode))
                        throw new eFilterRightException(filterId);

                    AdvFilterContext filterContext = new AdvFilterContext(pref, dal, filter, adminMode);
                    if (widgetType == XrmWidgetType.Kanban)
                        filterContext.FilterIntro = eResApp.GetRes(pref, 8480);

                    return eFilterRenderer.GetFilterRender(filterContext);
                }
                finally
                {
                    dal?.CloseDatabase();
                }
            }

            /// <summary>
            /// Construit et retourne le bloc HTML de contenu du filtre lors de l'exécution d'un filtre question
            /// </summary>
            /// <param name="pref">Préférences de l'utilisateur en cours</param>
            /// <param name="filterId">id du filter a charger</param>
            /// <returns>Div contenant l'interface du filtre</returns>
            internal static HtmlGenericControl GetEmptyLinesRender(ePref pref, int filterId, int tabId)
            {
                string error = String.Empty;
                eudoDAL dal = eLibTools.GetEudoDAL(pref);

                if (filterId == 0)
                    throw new Exception("Impossible de charger un filtre question sans son filterid");

                try
                {
                    dal.OpenDatabase();

                    AdvFilter filter = new AdvFilter(pref, filterId);
                    if (!filter.Load(dal, out error))
                        throw new Exception(error);

                    string paramValues = AdvFilter.Question.GetParam(dal, pref.User.UserId, tabId, filterId, out error);
                    if (error.Length != 0)
                        throw new Exception(error);

                    AdvFilterContext filterContext = new AdvFilterContext(pref, dal, filter);

                    return eFilterRenderer.GetEmptyLinesRender(filterContext, paramValues);
                }
                finally
                {
                    dal?.CloseDatabase();
                }
            }
        }

        internal static HtmlGenericControl GetLogicalOperatorList(ePrefLite pref, string id, InterOperator selectedOp, string onChange, bool withOnlyOp, bool emptyLineRender = false, bool bFromChartReport = false)
        {
            HtmlGenericControl lst = new HtmlGenericControl("select");
            lst.ID = id;
            lst.Attributes.Add("name", id);
            lst.Attributes.Add("onchange", onChange);

            InterOperator interOp = InterOperator.OP_AND;
            HtmlGenericControl andOp = new HtmlGenericControl("option");
            andOp.InnerText = FilterTools.GetAndOrLabel(interOp, pref).ToUpper();
            andOp.Attributes.Add("value", ((int)interOp).ToString());
            lst.Controls.Add(andOp);

            interOp = InterOperator.OP_OR;
            HtmlGenericControl orOp = new HtmlGenericControl("option");
            orOp.InnerText = FilterTools.GetAndOrLabel(interOp, pref).ToUpper();
            orOp.Attributes.Add("value", ((int)interOp).ToString());
            lst.Controls.Add(orOp);

            HtmlGenericControl onlyOp = null;
            if (withOnlyOp)
            {
                interOp = InterOperator.OP_AND_NOT;
                onlyOp = new HtmlGenericControl("option");
                onlyOp.InnerText = FilterTools.GetAndOrLabel(interOp, pref).ToUpper();
                onlyOp.Attributes.Add("value", ((int)interOp).ToString());
                lst.Controls.Add(onlyOp);
            }

            HtmlGenericControl endOp = new HtmlGenericControl("option");
            //endOp.InnerText = String.Concat("<", eResApp.GetRes(pref.Lang, 271).ToUpper(), ">");
            endOp.InnerText = eResApp.GetRes(pref, 271).ToUpper();
            endOp.Attributes.Add("value", ((int)InterOperator.OP_NONE).ToString());
            lst.Controls.Add(endOp);

            switch (selectedOp)
            {
                case InterOperator.OP_AND:
                    andOp.Attributes.Add("selected", "selected");
                    break;
                case InterOperator.OP_OR:
                    orOp.Attributes.Add("selected", "selected");
                    break;
                case InterOperator.OP_AND_NOT:
                    if (onlyOp != null)
                        onlyOp.Attributes.Add("selected", "selected");
                    break;
                default:
                    endOp.Attributes.Add("selected", "selected");
                    break;
            }

            if (emptyLineRender)
                lst.Style.Add(HtmlTextWriterStyle.Visibility, "hidden");

            return lst;
        }

        /// <summary>
        /// Construit et renvoie le Div contenant le rendu HTML du filtre pour l'assistant création
        /// </summary>
        /// <param name="filterContext">informations sur le filtres et le context eudo</param>
        /// <returns>Div contenant le rendu HTML</returns>
        private static HtmlGenericControl GetFilterRender(AdvFilterContext filterContext)
        {
            ePrefLite pref = filterContext.Pref;
            AdvFilter filter = filterContext.Filter;

            HtmlGenericControl mainDiv = new HtmlGenericControl("div");
            mainDiv.ID = "MainFilterDiv";

            mainDiv.Controls.Add(GetInputText("TxtFilterId", filter.FilterId.ToString(), false));
            mainDiv.Controls.Add(GetInputText("TxtFilterType", ((int)filter.FilterType).ToString(), false));
            mainDiv.Controls.Add(GetInputText("TxtFilterName", filter.FilterName, false));
            mainDiv.Controls.Add(GetInputText("TxtFilterUserId", filter.UserId.ToString(), false));
            mainDiv.Controls.Add(GetInputText("TxtFilterParams", eMD5.EncryptMd5(filter.FilterParams.ToString()), false));

            string action = filter.FilterId > 0 ? "edit" : "new";
            mainDiv.Controls.Add(GetInputText("TxtFilterAction", action, false));

            #region Permissions

            ePermission permView = new ePermission(filter.ViewPermId, filterContext.Dal, filterContext.Pref);
            mainDiv.Controls.Add(GetInputText("ViewPerm", filter.ViewPermId > 0 ? "1" : "0", false));
            mainDiv.Controls.Add(GetInputText("TxtFilterViewPermId", permView.PermId.ToString(), false));
            mainDiv.Controls.Add(GetInputText("TxtFilterViewPermMode", permView.PermMode.GetHashCode().ToString(), false));
            mainDiv.Controls.Add(GetInputText("TxtFilterViewPermUsersId", permView.PermUser, false));
            mainDiv.Controls.Add(GetInputText("TxtFilterViewPermLevel", permView.PermLevel.ToString(), false));

            ePermission permUpdate = new ePermission(filter.UpdatePermId, filterContext.Dal, filterContext.Pref);
            mainDiv.Controls.Add(GetInputText("UpdatePerm", filter.UpdatePermId > 0 ? "1" : "0", false));
            mainDiv.Controls.Add(GetInputText("TxtFilterUpdatePermId", permUpdate.PermId.ToString(), false));
            mainDiv.Controls.Add(GetInputText("TxtFilterUpdatePermMode", permUpdate.PermMode.GetHashCode().ToString(), false));
            mainDiv.Controls.Add(GetInputText("TxtFilterUpdatePermUsersId", permUpdate.PermUser, false));
            mainDiv.Controls.Add(GetInputText("TxtFilterUpdatePermLevel", permUpdate.PermLevel.ToString(), false));

            #endregion

            HtmlGenericControl titleDiv = new HtmlGenericControl("div");
            titleDiv.ID = "FilterTitleDiv";
            titleDiv.Attributes.Add("class", "head-text");

            if (!String.IsNullOrEmpty(filterContext.FilterIntro))
            {
                titleDiv.InnerHtml = filterContext.FilterIntro;
            }
            else if (filter.FilterType == TypeFilter.DEFAULT)
            {
                titleDiv.InnerHtml = eResApp.GetRes(pref, 2777);
                titleDiv.Style.Add("margin-left", "8px");
            }
            else if (filter.FilterType == TypeFilter.DBL)
            {
                titleDiv.InnerHtml = eResApp.GetRes(pref, 7616);
                titleDiv.Style.Add("margin-left", "8px");
            }
            else if (filter.FilterType == TypeFilter.WIDGET)
            {
                titleDiv.InnerHtml = eResApp.GetRes(pref, 8267);
            }
            else
                titleDiv.InnerHtml = String.Concat(filter.FilterName);



            mainDiv.Controls.Add(titleDiv);

            HtmlGenericControl filterDiv = new HtmlGenericControl("div");
            filterDiv.Attributes.Add("class", "filtres");
            filterDiv.ID = "FilterTabsContainer";
            mainDiv.Controls.Add(filterDiv);
            if (filter.FilterType == TypeFilter.DEFAULT)
            {
                filterDiv.Attributes.Add("class", "filtres default-filter-filtres");
            }

            HtmlGenericControl tabDiv = new HtmlGenericControl("div");
            filterDiv.Controls.Add(tabDiv);

            int idx = 0;
            foreach (AdvFilterTab tab in filter.FilterTabs)
            {
                if (tab.Lines.Count == 0)
                    continue;

                //Opérateur inter Tabs
                if (!tab.TabOperator.Equals(InterOperator.OP_NONE))
                {
                    HtmlGenericControl opDiv = new HtmlGenericControl("div");
                    opDiv.Attributes.Add("class", "operateur_principal");
                    opDiv.ID = "operateur_principal_" + tab.TabIndex;
                    opDiv.Attributes.Add("name", "operateur_principal_" + tab.TabIndex);

                    opDiv.Controls.Add(GetLogicalOperatorList(pref,
                        string.Concat("link_", tab.TabIndex), tab.TabOperator, "onChangeFilterTabOp(this)", true));

                    filterDiv.Controls.Add(opDiv);
                }

                //Tabs
                filterDiv.Controls.Add(eFilterTabRenderer.GetTabRenderer(filterContext, new AdvFilterTabIndex(idx), tab.Importance));
                idx++;
            }

            // Opérateur de fin
            HtmlGenericControl opFin = GetLogicalOperatorList(pref, "end_operator_tab_", InterOperator.OP_NONE, "onChangeFilterTabOp(this)", true);
            opFin.Attributes.Add("class", "endOperator");
            filterDiv.Controls.Add(opFin);

            // Div options
            HtmlGenericControl optionsContainerDiv = new HtmlGenericControl("div");
            optionsContainerDiv.Attributes.Add("class", "adv-opt-block");
            mainDiv.Controls.Add(optionsContainerDiv);

            // Options avancées
            HtmlGenericControl optDiv = new HtmlGenericControl("div");
            optDiv.Attributes.Add("onclick", "showAdvancedfilterOptions()");
            optDiv.Style.Add(HtmlTextWriterStyle.Cursor, "pointer");

            HtmlGenericControl optBtnDiv = new HtmlGenericControl("div");
            optBtnDiv.ID = "OptBtnDiv";
            optBtnDiv.Attributes.Add("class", "icon-develop arrow-opt");
            optDiv.Controls.Add(optBtnDiv);

            HtmlGenericControl optDivLbl = new HtmlGenericControl("div");
            optDiv.Controls.Add(optDivLbl);
            optDivLbl.Attributes.Add("class", "text-opt_av");
            optDivLbl.InnerText = eResApp.GetRes(pref, 597);

            optionsContainerDiv.Controls.Add(optDiv);

            optionsContainerDiv.Controls.Add(GetOptionsBlock(pref, filter));

            if (filter.FilterType == TypeFilter.DEFAULT || filter.FilterType == TypeFilter.DBL || filter.FilterType == TypeFilter.RELATION || filter.FilterType == TypeFilter.RULES || filter.FilterType == TypeFilter.NOTIFICATION)
            {
                //masque les options "classiques"
                optionsContainerDiv.Style.Add("display", "none");
                //ajoute les options "spéciales" de filtre par défaut
                if (filter.FilterType == TypeFilter.DEFAULT)
                    mainDiv.Controls.Add(GetOptionsDefaultFilterBlock(pref, filter));
            }

            return mainDiv;
        }

        /// <summary>
        /// Construit et retourne le bloc HTML de contenu du filtre lors de l'exécution d'un filtre question
        /// </summary>
        /// <param name="filterContext">informations sur le filtres et le context eudo</param>
        /// <param name="paramValues">chaine de valeurs à prérenseigner (rappel synthaxe : value_indexTab_indexLine séparés par des esperluettes</param>
        /// <returns>Div contenant l'interface du filtre</returns>
        private static HtmlGenericControl GetEmptyLinesRender(AdvFilterContext filterContext, string paramValues)
        {
            ePrefLite pref = filterContext.Pref;
            AdvFilter filter = filterContext.Filter;

            HtmlGenericControl mainDiv = new HtmlGenericControl("div");
            mainDiv.ID = "MainFilterDiv";
            mainDiv.Attributes.Add("class", "emptyLinesDiv");

            HtmlGenericControl titleDiv = new HtmlGenericControl("div");
            titleDiv.ID = "FilterTitleDiv";
            titleDiv.Attributes.Add("class", "head-text");
            titleDiv.InnerHtml = String.Concat(filter.FilterName);

            if (filter.IsQuestionFilter)
            {
                HtmlGenericControl titleSpan = new HtmlGenericControl("span");
                titleSpan.Attributes.Add("class", "formularFilterWarning");
                titleSpan.InnerText = eResApp.GetRes(pref, 6336);
                titleDiv.Controls.Add(titleSpan);
            }
            mainDiv.Controls.Add(titleDiv);

            mainDiv.Controls.Add(GetInputText("TxtFilterId", filter.FilterId.ToString(), false));

            HtmlGenericControl filterDiv = new HtmlGenericControl("div");
            filterDiv.Attributes.Add("class", "filtres filtres-empty-scroll");
            filterDiv.ID = "FilterTabsContainer";

            if (filter.FilterType == TypeFilter.DEFAULT)
                filterDiv.Style.Add(HtmlTextWriterStyle.Height, "70%");

            mainDiv.Controls.Add(filterDiv);

            HtmlGenericControl tabDiv = new HtmlGenericControl("div");
            filterDiv.Controls.Add(tabDiv);

            #region Refactorisation de la string de filterquestion sauvegardé en dictionnaire
            Dictionary<string, string> DicoParamValues = new Dictionary<string, string>();
            string[] tabParamValues = paramValues.Split("&");
            string sCurrentValue = string.Empty;
            for (int cpt = 0; cpt < tabParamValues.Length; cpt++)
            {
                sCurrentValue = tabParamValues[cpt];
                if (String.IsNullOrEmpty(sCurrentValue))
                    continue;
                string[] tabvalue = sCurrentValue.Split("=");
                if (tabvalue.Length < 2)
                    continue;
                DicoParamValues.Add(tabvalue[0], tabvalue[1]);
            }
            #endregion

            AdvFilterTab tab;
            AdvFilterLine line;
            for (int arrayTabIdx = 0; arrayTabIdx < filter.FilterTabs.Count; arrayTabIdx++)
            {
                tab = filter.FilterTabs[arrayTabIdx];
                if (!tab.ContainsEmtyLines)
                    continue;

                HtmlGenericControl divTab = new HtmlGenericControl("div");
                string strFilterTabClass = "tbl_recap choix_filtres";
                if (arrayTabIdx == 0)
                    strFilterTabClass = String.Concat(strFilterTabClass, " choix_filtres_first");
                divTab.Attributes.Add("class", strFilterTabClass);
                filterDiv.Controls.Add(divTab);

                HtmlGenericControl divHead = new HtmlGenericControl("div");
                divHead.Attributes.Add("class", "tbl_recap-head");
                divTab.Controls.Add(divHead);
                divHead.InnerText = filterContext.CacheTableInfo.Get(tab.Table.DescId).Libelle;

                for (int arrayLineIdx = 0; arrayLineIdx < tab.Lines.Count; arrayLineIdx++)
                {
                    line = tab.Lines[arrayLineIdx];

                    if (String.IsNullOrEmpty(line.Value) && !filter.RAZ)  //Si valeur non prédéfinit pour ce champ et RAZ (remmettre à zero) n'est pas sélectionné
                    {
                        //Reprendre la valeur du uservalue de l'utilisateur si existant
                        string current_line = String.Concat("value_", line.TabLineIndex);
                        if (DicoParamValues.ContainsKey(current_line))
                            line.Value = HttpUtility.UrlDecode(DicoParamValues[current_line]);
                    }

                    if (line.IsQuestionField)
                    {
                        divTab.Controls.Add(eFilterLineRenderer.GetLineRenderer(filterContext, new AdvFilterLineIndex(arrayTabIdx, arrayLineIdx), true));
                    }

                }
            }

            HtmlGenericControl descDiv = new HtmlGenericControl("div");
            descDiv.Attributes.Add("class", "form_down_recap");
            descDiv.InnerHtml = String.Concat("<div>", filter.GetFilterDescription(), "</div>");
            mainDiv.Controls.Add(descDiv);

            return mainDiv;
        }

        private static HtmlGenericControl GetInputText(string id, string value, bool bVisible)
        {
            HtmlGenericControl inpt = new HtmlGenericControl("input");
            inpt.ID = id;
            inpt.Attributes.Add("value", value);
            inpt.Style.Add(HtmlTextWriterStyle.Display, bVisible ? "block" : "none");
            return inpt;
        }

        /// <summary>
        /// Retourne le control pour la liste des options
        /// </summary>
        private static HtmlGenericControl GetOptionsDefaultFilterBlock(ePrefLite pref, AdvFilter filter)
        {
            HtmlGenericControl optList = new HtmlGenericControl("div");
            optList.ID = "DivDefaultFilterOpt";

            optList.Attributes.Add("class", "option-list default-filter-option");

            Panel pnlApply = new Panel();
            pnlApply.ID = "applyDefaultFilter";
            pnlApply.CssClass = "default-filter-opt-apply";
            optList.Controls.Add(pnlApply);

            pnlApply.Controls.Add(new LiteralControl(eResApp.GetRes(pref, 8046)));



            pnlApply.Controls.Add(eTools.GetRadioButton("ApplyOpen0", "applyDefaultFilter", !filter.IsActiveDefaultFilter, eResApp.GetRes(pref, 8047), true, "nsFilterWizard.selectOptionDefaultFilter(this)", "0"));
            pnlApply.Controls.Add(eTools.GetRadioButton("ApplyOpen1", "applyDefaultFilter", filter.IsActiveDefaultFilter, eResApp.GetRes(pref, 8048), true, "nsFilterWizard.selectOptionDefaultFilter(this)", "1"));


            Panel pnlDisablable = new Panel();
            pnlDisablable.CssClass = "default-filter-opt-disable";
            pnlDisablable.ID = "disableDefaultFilter";

            if (!filter.IsActiveDefaultFilter)
                pnlDisablable.Style.Add(HtmlTextWriterStyle.Display, "none");

            optList.Controls.Add(pnlDisablable);


            pnlDisablable.Controls.Add(new LiteralControl(eResApp.GetRes(pref, 8049)));
            pnlDisablable.Controls.Add(eTools.GetRadioButton("deactivableDefaultFilter1", "alwaysActiveDefaultFilter", !filter.IsAlwaysActiveDefaultFilter, eResApp.GetRes(pref, 7617), true, "nsFilterWizard.selectOptionDefaultFilter(this)", "0"));
            pnlDisablable.Controls.Add(eTools.GetRadioButton("deactivableDefaultFiltere0", "alwaysActiveDefaultFilter", filter.IsAlwaysActiveDefaultFilter, eResApp.GetRes(pref, 7618), true, "nsFilterWizard.selectOptionDefaultFilter(this)", "1"));

            return optList;
        }

        /// <summary>
        /// Retourne le control pour la liste des options
        /// </summary>
        private static HtmlGenericControl GetOptionsBlock(ePrefLite pref, AdvFilter filter)
        {
            HtmlGenericControl optList = new HtmlGenericControl("div");
            optList.ID = "DivOptList";

            optList.Attributes.Add("class", "option-list");
            optList.Style.Add(HtmlTextWriterStyle.Display, "none");

            optList.Controls.Add(eTools.GetCheckBoxOption(eResApp.GetRes(pref, 342), "fileonly", filter.FileOnly, false, "opt-list-chk", "onCheckOption"));
            optList.Controls.Add(eTools.GetCheckBoxOption(eResApp.GetRes(pref, 1028), "negation", filter.Negation, false, "opt-list-chk", "onCheckOption"));
            optList.Controls.Add(eTools.GetCheckBoxOption(eResApp.GetRes(pref, 6150), "raz", filter.RAZ, false, "opt-list-chk", "onCheckOption"));

            HtmlGenericControl rnd = eTools.GetCheckBoxOption(String.Concat(eResApp.GetRes(pref, 1413), ""), "random", filter.Random > 0, false, "opt-list-chk", "onCheckOption");
            var a = rnd.Controls[0];
            HtmlInputText inpt = new HtmlInputText("text");
            inpt.ID = "TxtRandom";
            inpt.Name = "TxtRandom";
            inpt.Value = filter.Random.ToString();
            rnd.Controls.Add(inpt);
            optList.Controls.Add(rnd);

            optList.Controls.Add(eTools.GetCheckBoxOption(eResApp.GetRes(pref, 1251), "OnlyFilesOpt", filter.FilterTabs.FindAll(f => f.Importance == true)?.Count > 0, false, "opt-list-chk", "onCheckOption"));

            return optList;
        }

        /// <summary>
        /// Gestion des exceptions issues d'un pb de droit d'accès au filtre
        /// </summary>
        public class eFilterRightException : Exception
        {
            /// <summary>
            /// Constructeur
            /// </summary>
            /// <param name="msg"></param>
            public eFilterRightException(int filterId) :
                base(String.Concat("Droit invalide pour l'accès du filtre ", filterId))
            {

            }
        }

        /// <summary>
        /// Class pour spécifier les paramètres additionnels pour les filtres
        /// Utiliser principalement dans les filtres express sur les graphiques
        /// </summary>
        public class eFilterRendererParams
        {
            /// <summary>
            /// Inidque si on est en création d'un filtre(graphique)
            /// </summary>
            private Boolean _bInitialEfChart { get; set; }
            public Boolean bInitialEfChart
            {
                get { return _bInitialEfChart; }
            }

            /// <summary>
            /// Indique si on vient d'un graphique
            /// </summary>
            private static Boolean _bFromChartReport { get; set; }
            public Boolean bFromChartReport
            {
                get { return _bFromChartReport; }
            }

            /// <summary>
            /// Inidque si notre graphique est de type combiné
            /// </summary>
            public Boolean bCombinedExpressFilter
            {
                get
                {
                    return (
                      !string.IsNullOrEmpty(PrefixFilter) &&
                      (PrefixFilter.ToLower() == eLibConst.COMBINED_Y.ToLower() ||
                      PrefixFilter.ToLower() == eLibConst.COMBINED_Z.ToLower()));
                }
            }

            /// <summary>
            /// Prefix utiliser dans l'id du filtre
            /// </summary>
            private string _prefixFilter;
            public string PrefixFilter
            {
                get { return _prefixFilter; }
            }

            /// <summary>
            /// Représente la liste du type de rubrique à présenter dans le filtre
            /// </summary>
            private FieldFormat[] _lstFildsFormat { get; set; }
            public FieldFormat[] LstFildsFormat
            {
                get { return _lstFildsFormat; }
            }

            /// <summary>
            /// Ajouter une action spécifique sur le onchange de la liste des rubriques 
            /// </summary>
            private Boolean _bSetFieldAction { get; set; }
            public Boolean bSetFieldAction
            {
                get { return _bSetFieldAction; }
            }

            /// <summary>
            /// Inidque si on veut récupérer la liste des opérateur et des valeurs
            /// </summary>
            private Boolean _bGetFilterOpAndValue { get; set; }
            public Boolean bGetFilterOpAndValue
            {
                get { return _bGetFilterOpAndValue; }
            }

            /// <summary>
            /// Inidque quel format de rubrique on affiche
            /// </summary>
            private FieldFormat? _sDisplayedFiledsFmt { get; set; }
            public FieldFormat? sDisplayedFiledsFmt
            {
                get { return _sDisplayedFiledsFmt; }
                set { _sDisplayedFiledsFmt = value; }
            }

            /// <summary>
            /// Pour les catalogue : indique le popup descid
            /// </summary>
            private Int32? _nDisplayedFieldsPud { get; set; }
            public Int32? nDisplayedFieldsPud
            {
                get { return _nDisplayedFieldsPud; }
                set { _nDisplayedFieldsPud = value; }
            }

            public eFilterRendererParams(Boolean bInitialEfChart = false, Boolean bFromChartReport = false,
            Boolean bSpecialExpressFilter = false, string prefixFilter = "", FieldFormat[] lstFildsFormat = null,
            Boolean bSetFieldAction = true, Boolean bGetFilterOpAndValue = true, FieldFormat? sDisplayedFiledsFmt = FieldFormat.TYP_CHAR, Int32? nDisplayedFieldsPud = 0)
            {
                _bInitialEfChart = bInitialEfChart;
                _bFromChartReport = bFromChartReport;
                _prefixFilter = prefixFilter;
                _lstFildsFormat = lstFildsFormat;
                _bSetFieldAction = bSetFieldAction;
                _bGetFilterOpAndValue = bGetFilterOpAndValue;
                _sDisplayedFiledsFmt = sDisplayedFiledsFmt;
                _nDisplayedFieldsPud = nDisplayedFieldsPud;

            }

        }
    }
}