using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using EudoQuery;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Renderer de la liste des modules disponibles en administration
    /// </summary>
    public class eAdminExtensionListRenderer : eAdminExtensionRenderer
    {
        const int EXTENSIONSPERPAGE = 10;
        const int EXTENSIONSTABID = 0; // TODO ?


        IDictionary<string, string> _categories = new Dictionary<string, string>();
        IDictionary<string, double?> _notations = new Dictionary<string, double?>();
        string _currentSearch = String.Empty;
        int _currentPage = 1;
        int _extensionsPerPage = EXTENSIONSPERPAGE;
        int _displayedExtensionsCount = 0;
        int _totalExtensionsCount = 0;
        string _userFontSize = String.Empty;

        private double? _currentFilterNotation;
        private string _currentFilterCategory = String.Empty;


        /// <summary>
        /// Indique si un filtre de recherche est activé sur la liste
        /// </summary>
        public bool SearchEnabled
        {
            get
            {
                return CurrentSearch != null && CurrentSearch.Trim().Length > 0;
            }
        }

        /// <summary>
        /// Renvoi la liste des extensions affichés en mode liste
        /// </summary>
        public List<eAdminExtension> ExtensionList
        {
            get
            {
                return _includedExtensionList;
            }
        }

        /// <summary>
        /// Indique si un filtre de catégorie est activé sur la liste
        /// </summary>
        public bool FilterCategoryEnabled
        {
            get
            {
                return CurrentFilterCategory != null && CurrentFilterCategory.Trim().Length > 0;
            }
        }

        /// <summary>
        /// Indique si un filtre de classement (notes) est activé sur la liste
        /// </summary>
        public bool FilterNotationEnabled
        {
            get
            {
                return CurrentFilterNotation != null;
            }
        }

        /// <summary>
        /// Indique le nombre d'extensions à afficher par page
        /// </summary>
        public int ExtensionsPerPage
        {
            get
            {
                return _extensionsPerPage;
            }

            set
            {
                if (value > 0)
                    _extensionsPerPage = value;
                else
                    _extensionsPerPage = EXTENSIONSPERPAGE;
            }
        }

        public int DisplayedExtensionsCount
        {
            get
            {
                return _displayedExtensionsCount;
            }
            private set
            {
                _displayedExtensionsCount = value;
            }
        }

        public int TotalExtensionsCount
        {
            get
            {
                return _totalExtensionsCount;
            }
            private set
            {
                _totalExtensionsCount = value;
            }
        }

        public int CurrentPage
        {
            get
            {
                return _currentPage;
            }

            set
            {
                if (value == 0)
                    _currentPage = 1;
                else
                    _currentPage = value;
            }
        }

        public string CurrentSearch
        {
            get
            {
                return _currentSearch;
            }

            set
            {
                _currentSearch = value;
            }
        }

        public string CurrentFilterCategory
        {
            get
            {
                return _currentFilterCategory;
            }

            set
            {
                _currentFilterCategory = value;
            }
        }

        public double? CurrentFilterNotation
        {
            get
            {
                return _currentFilterNotation;
            }

            set
            {
                _currentFilterNotation = value;
            }
        }

        public int PageCount
        {
            get
            {
                return (int)Math.Ceiling(((decimal)TotalExtensionsCount) / ExtensionsPerPage);
            }
        }

        public string UserFontSize
        {
            get
            {
                return _userFontSize;
            }

            set
            {
                _userFontSize = value;
            }
        }

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public eAdminExtensionListRenderer(ePref pref, int nPage, int nRows, string sSearch, string sFilterCategory, double? dFilterNotation)
            : base(pref)
        {
            Pref = pref;
            ExtensionsPerPage = nRows;
            CurrentPage = nPage;
            CurrentSearch = sSearch;
            CurrentFilterCategory = sFilterCategory;
            CurrentFilterNotation = dFilterNotation;
        }


        public static eAdminExtensionListRenderer CreateAdminExtensionListRenderer(ePref pref, int nPage, int nRows, string sSearch, string sFilterCategory, double? dFilterNotation)
        {
            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();

            return new eAdminExtensionListRenderer(pref, nPage, nRows, sSearch, sFilterCategory, dFilterNotation);
        }

        /// <summary>
        /// Initialisation des params
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            if (!eFeaturesManager.IsFeatureAvailable(Pref, eConst.XrmFeature.AdminExtensions))
                return false;

            _userFontSize = eTools.GetUserFontSize(Pref);

            _includedExtensionList = new List<eAdminExtension>(); // Extensions correspondant aux critères de recherche ET à la pagination
            _hiddenExtensionList = new List<eAdminExtension>(); // Extensions correspondant aux critères de recherche, mais hors pagination
            _excludedExtensionList = new List<eAdminExtension>(); // Extensions ne correspondant ni aux critères de recherche ni à la pagination
            List<eAdminExtension> potentiallyIncludedExtensionsList = new List<eAdminExtension>();
            List<eAdminExtension> extensionFromJson = new List<eAdminExtension>();

            bool bStoreAccessOk = false;

            #region Ajout des extensions à partir de la liste référencée sur HotCom

            if (!bNoInternet)
            {
                eAPIExtensionStoreAccess storeAccess = new eAPIExtensionStoreAccess(Pref);
                _categories = storeAccess.GetExtensionCategories().ToDictionary(k => k.Id.ToString(), k => k.Label);
                _includedExtensionList = storeAccess.GetExtensionList_Old(CurrentPage, ExtensionsPerPage, CurrentFilterCategory, CurrentSearch, out _totalExtensionsCount);
                if (storeAccess.ApiErrors.Trim().Length == 0)
                {
                    bStoreAccessOk = true;
                }
            }

            #endregion

            #region En cas d'échec : ajout des extensions natives
            if (!bStoreAccessOk)
            {
                //Chargement de la liste extension depuis le json
                extensionFromJson = eAdminExtension.initListExtensionFromJson(Pref);

                //Gestion pagination et recherche

                for (int i = 0; i < extensionFromJson.Count; i++)
                {
                    eAdminExtension currentExtension = extensionFromJson[i];
                    if (
                        // Filtre sur les termes de recherche
                        (
                            !SearchEnabled ||
                            (
                                    //CurrentSearch.Trim().Length > 0 && ( //CNA - Inutile car déjà testé dans FilterCategoryEnabled
                                    (!String.IsNullOrEmpty(currentExtension.Infos.Title) && currentExtension.Infos.Title.ToLower().Contains(CurrentSearch.ToLower())) ||
                                    (!String.IsNullOrEmpty(currentExtension.Infos.Summary) && currentExtension.Infos.Summary.ToLower().Contains(CurrentSearch.ToLower())) ||
                                    (!String.IsNullOrEmpty(currentExtension.Infos.Description) && currentExtension.Infos.Description.ToLower().Contains(CurrentSearch.ToLower())) ||
                                    (!String.IsNullOrEmpty(currentExtension.Infos.Author) && currentExtension.Infos.Author.ToLower().Contains(CurrentSearch.ToLower())) ||
                                    (!String.IsNullOrEmpty(currentExtension.Infos.Tooltip) && currentExtension.Infos.Tooltip.ToLower().Contains(CurrentSearch.ToLower()))
                            //)
                            )
                        )
                        // Filtre sur les catégories
                        &&
                        (
                            !FilterCategoryEnabled ||
                            (
                                    //CurrentFilterCategory.Trim().Length > 0 && ( //CNA - Inutile car déjà testé dans FilterCategoryEnabled
                                    (currentExtension.Infos.Categories != null && currentExtension.Infos.Categories.Keys.Contains(CurrentFilterCategory, StringComparer.OrdinalIgnoreCase)) ||
                                    (CurrentFilterCategory == "-1" && (currentExtension.Infos.Categories == null || currentExtension.Infos.Categories.Keys.Contains("-1")))
                            //)
                            )
                        )

                        // Filtre sur les notes
                        &&
                        (
                            !FilterNotationEnabled ||
                            (
                                CurrentFilterNotation == currentExtension.Infos.Notation.Value || (CurrentFilterNotation == -1 && currentExtension.Infos.Notation.Value == null)
                            )
                        )
                    )
                    {
                        potentiallyIncludedExtensionsList.Add(currentExtension);
                    }
                    else
                        _excludedExtensionList.Add(currentExtension);
                }

                // Comptage du nombre d'extensions affichables avant pagination
                TotalExtensionsCount = potentiallyIncludedExtensionsList.Count;

                // Correction des compteurs - -1 : dernière page demandée
                if (CurrentPage == -1 || CurrentPage > PageCount)
                    CurrentPage = PageCount;
                if (CurrentPage < 1)
                    CurrentPage = 1;

                // Définition des bornes de pagination à partir des compteurs actualisés
                int firstExtensionIndex = (CurrentPage * ExtensionsPerPage) - ExtensionsPerPage;
                int lastExtensionIndex = firstExtensionIndex + ExtensionsPerPage - 1;

                // Application du filtre
                for (int i = 0; i < potentiallyIncludedExtensionsList.Count; i++)
                {
                    if (i >= firstExtensionIndex && i <= lastExtensionIndex)
                        _includedExtensionList.Add(potentiallyIncludedExtensionsList[i]);
                    else
                        _hiddenExtensionList.Add(potentiallyIncludedExtensionsList[i]);
                }

            }


            #endregion

            return base.Init();
        }

        /// <summary>
        /// Génération du contenu
        /// </summary>
        /// <returns>true si le contenu a été généré avec succès</returns>
        protected override bool Build()
        {
            Panel pnlContainer = new Panel();
            pnlContainer.CssClass = "admin_cont";
            _pgContainer.Controls.Add(pnlContainer);

            pnlContainer.Controls.Add(GetExtensionsCounter());

            pnlContainer.Controls.Add(GetExtensionsFiltersBar());

            pnlContainer.Controls.Add(GetExtensionsHeader());

            pnlContainer.Controls.Add(GetExtensionsListPanel());

            #region Ajout des JS permettant d'initialiser le rendu
            AddCallBackScript(
                String.Concat(
                    "nsAdmin.extensionsPerPage = ", ExtensionsPerPage.ToString(), "; ",
                    "nsAdmin.currentExtensionPage = ", CurrentPage.ToString(), "; ",
                    "nsAdmin.currentExtensionSearch = '", CurrentSearch.Replace("'", "\\'"), "'; ",
                    "nsAdmin.currentExtensionFilterNotation = '", CurrentFilterNotation.ToString(), "'; ",
                    "nsAdmin.currentExtensionFilterCategory = '", CurrentFilterCategory.Replace("'", "\\'"), "'; ",
                    "setPaging(", EXTENSIONSTABID, "); "
                )
            );
            #endregion

            return true;
        }

        public override Panel GetExtensionPanel(eAdminExtension specificExtension)
        {
            return GetExtensionPanel(Pref, specificExtension, true);
        }

        /// <summary>
        /// Retourne le rendu de la liste des extensions
        /// </summary>
        /// <returns></returns>
        public Panel GetExtensionsListPanel()
        {
            Panel pnlBlocks = new Panel();
            pnlBlocks.ID = "blockListExtensions";


            var n = 0;
            foreach (var e in _includedExtensionList)
            {
                pnlBlocks.Controls.Add(GetExtensionPanel(e));
            }

            return pnlBlocks;
        }

        /// <summary>
        /// Correspond à la partie "nb d'extensions"
        /// </summary>
        /// <returns></returns>
        Panel GetExtensionsCounter()
        {
            Panel pnlInfos = new Panel();
            pnlInfos.ID = "infos";
            pnlInfos.CssClass = "infosExtensions";

            HtmlGenericControl icon, element;

            #region Icône filtre (si recherche ou filtre actifs)
            if (FilterCategoryEnabled || FilterNotationEnabled)
            {
                icon = new HtmlGenericControl();
                icon.ID = "iconeFilter";
                icon.Attributes.Add("class", "icon-list_filter");
                icon.Attributes.Add("onmouseover", "stfilter(event);");
                icon.Attributes.Add("onclick", "nsAdmin.resetExtFilters();");
                pnlInfos.Controls.Add(icon);
            }
            #endregion

            #region Nb de fiches
            Panel label = new Panel();
            label.CssClass = "lib";

            element = new HtmlGenericControl();
            element.ID = "SpanNbElem";
            element.InnerText = getActiveExtensions().Count.ToString();
            label.Controls.Add(element);

            element = new HtmlGenericControl();
            element.ID = "SpanLibElem";
            element.InnerHtml = String.Concat(" ", eResApp.GetRes(Pref, 7996));
            label.Controls.Add(element);

            element = new HtmlGenericControl();
            element.ID = "SpanLibElemSep";
            element.InnerText = " / ";
            label.Controls.Add(element);

            element = new HtmlGenericControl();
            element.ID = "SpanNbElem2";
            element.InnerText = TotalExtensionsCount.ToString();
            label.Controls.Add(element);

            element = new HtmlGenericControl();
            element.ID = "SpanLibElem2";
            element.InnerHtml = String.Concat(" ", eResApp.GetRes(Pref, 7997));
            label.Controls.Add(element);

            pnlInfos.Controls.Add(label);
            #endregion

            return pnlInfos;
        }

        /// <summary>
        /// Correspond à la barre des filtres
        /// </summary>
        /// <returns></returns>
        Panel GetExtensionsFiltersBar()
        {
            Panel pListFilter = new Panel();
            pListFilter.ID = "listfiltres";
            pListFilter.CssClass = "listfiltres";

            Panel pFiltre = new Panel();
            pFiltre.CssClass = "qckFiltre";

            Label lblFilter = new Label();
            lblFilter.Text = eResApp.GetResWithColon(Pref, 7998);
            lblFilter.CssClass = "qckfltlbl";
            pFiltre.Controls.Add(lblFilter);

            HtmlSelect htmlList = new HtmlSelect();     // Objet liste "<SELECT>"
            htmlList.ID = String.Concat("QuickF_Category");
            htmlList.Attributes.Add("size", "1");
            htmlList.Attributes.Add("onchange", "nsAdmin.setExtFilter(this);");
            htmlList.Items.AddRange(getCategoryFilter().ToArray());

            // Remplacement du taquet vert moche
            if (FilterCategoryEnabled)
                htmlList.Attributes.Add("class", "activeQF");

            pFiltre.Controls.Add(htmlList);

            pListFilter.Controls.Add(pFiltre);

            pFiltre = new Panel();
            pFiltre.CssClass = "qckFiltre";

            lblFilter = new Label();
            lblFilter.Text = eResApp.GetResWithColon(Pref, 7999);
            lblFilter.CssClass = "qckfltlbl";
            pFiltre.Controls.Add(lblFilter);

            htmlList = new HtmlSelect();     // Objet liste "<SELECT>"
            htmlList.ID = String.Concat("QuickF_Notation");
            htmlList.Attributes.Add("size", "1");
            htmlList.Attributes.Add("onchange", "nsAdmin.setExtFilter(this);");
            htmlList.Items.AddRange(getNotationFilter().ToArray());

            if (FilterNotationEnabled)
                htmlList.Attributes.Add("class", "activeQF");

            pFiltre.Controls.Add(htmlList);

            pListFilter.Controls.Add(pFiltre);

            return pListFilter;
        }

        /// <summary>
        /// Retourne les extensions actives
        /// </summary>
        /// <returns>Liste des extensions actives</returns>
        private List<eAdminExtension> getActiveExtensions()
        {
            return _includedExtensionList.Where(x => x.Infos.IsEnabled).ToList();
        }

        /// <summary>
        /// Comprend la partie "Pagination" et la barre de recherche
        /// </summary>
        /// <returns></returns>
        Panel GetExtensionsHeader()
        {
            bool bIsPaginationEnabled = true;
            bool bDrawSearchField = true; // si false, le contrôle n'est pas généré
            bool bHideSearchField = true; // si false, le contrôle n'est pas affiché, même si généré

            string[] pageFunctionsParameters = new string[] { EXTENSIONSTABID.ToString() };
            string[] selectPageFunctionParameters = new string[] { EXTENSIONSTABID.ToString(), "this" };
            string firstPageFct = "nsAdmin.extPageFirst";
            string lastPageFct = "nsAdmin.extPageLast";
            string prevPageFct = "nsAdmin.extPagePrev";
            string nextPageFct = "nsAdmin.extPageNext";
            string selectPageFct = "nsAdmin.extPageSelect";

            Panel pListHeader = new Panel();
            Panel pListactions = new Panel();

            pListHeader.ID = "listheader";
            pListHeader.CssClass = "listheader listHeaderExtensions";
            pListHeader.CssClass += " fs_" + UserFontSize + "pt";

            // Contrôles requis par les scripts d'initialisation de la pagination
            HtmlGenericControl fakeControl = new HtmlGenericControl("span");
            fakeControl.ID = String.Concat("mt_", EXTENSIONSTABID);
            fakeControl.Attributes.Add("cnton", "1");
            fakeControl.Attributes.Add("cPage", CurrentPage.ToString());
            fakeControl.Attributes.Add("nbPage", PageCount.ToString());
            fakeControl.Attributes.Add("top", ((CurrentPage - 1) * ExtensionsPerPage).ToString());
            fakeControl.Attributes.Add("bof", CurrentPage <= 1 ? "1" : "0");
            fakeControl.Attributes.Add("eof", CurrentPage >= PageCount ? "1" : "0");
            fakeControl.Attributes.Add("eNbCnt", eNumber.FormatNumber(Pref, _includedExtensionList.Count, 0, true));
            fakeControl.Attributes.Add("eNbTotal", eNumber.FormatNumber(Pref, _includedExtensionList.Count, 0, true));
            fakeControl.Attributes.Add("eHasCount", "1");
            pListHeader.Controls.Add(fakeControl);

            //Div listactions 1
            pListHeader.Controls.Add(pListactions);
            pListactions.ID = "actions";
            pListactions.CssClass = "listactions";

            System.Web.UI.WebControls.Table paginationSearchTable = new System.Web.UI.WebControls.Table();
            TableRow tr1 = new TableRow();

            if (bIsPaginationEnabled)
            {
                paginationSearchTable = GetPagination(firstPageFct, prevPageFct, nextPageFct, lastPageFct, selectPageFct, pageFunctionsParameters, selectPageFunctionParameters);
                tr1 = paginationSearchTable.Rows[0];
            }
            else
            {
                paginationSearchTable.Rows.Add(tr1);
            }

            pListactions.Controls.Add(paginationSearchTable);

            // Champ de recherche
            tr1.Cells.Add(GetSearch(bDrawSearchField, bHideSearchField, "nsAdmin.extSearch"));

            return pListHeader;
        }

        /// <summary>
        /// Fonction générique générant les contrôles de pagination
        /// </summary>
        /// <param name="firstPageFct"></param>
        /// <param name="prevPageFct"></param>
        /// <param name="nextPageFct"></param>
        /// <param name="lastPageFct"></param>
        /// <param name="selectPageFct"></param>
        /// <param name="navPageFctParam"></param>
        /// <param name="selectPageFctParam"></param>
        /// <returns></returns>
        private System.Web.UI.WebControls.Table GetPagination(
            string firstPageFct = "firstpage",
            string prevPageFct = "prevpage",
            string nextPageFct = "nextpage",
            string lastPageFct = "lastpage",
            string selectPageFct = "selectpage",
            string[] navPageFctParam = null,
            string[] selectPageFctParam = null
        )
        {
            string strNavPageFunctionsParameters = navPageFctParam != null ? String.Join(", ", navPageFctParam) : String.Empty;
            string strSelectPageFunctionParameters = selectPageFctParam != null ? String.Join(", ", selectPageFctParam) : String.Empty;

            System.Web.UI.WebControls.Table tbListAction = new System.Web.UI.WebControls.Table();
            tbListAction.CssClass = "listactionstab";

            TableRow tr1 = new TableRow();
            tr1.TableSection = TableRowSection.TableBody;
            tbListAction.Rows.Add(tr1);

            //Première cellule de remplissage (ex : Actions) - ISO mode liste
            TableCell tcFirstPaddingCell = new TableCell();
            tcFirstPaddingCell.Attributes.Add("width", "20%");
            tr1.Cells.Add(tcFirstPaddingCell);

            //Seconde cellule de remplissage (ex : Sélection) - ISO mode liste
            TableCell tcSecondPaddingCell = new TableCell();
            tcSecondPaddingCell.Attributes.Add("width", "22%");
            tr1.Cells.Add(tcSecondPaddingCell);

            //first Page
            TableCell tcFirst = new TableCell();
            tcFirst.Attributes.Add("width", "20px");
            tr1.Cells.Add(tcFirst);

            Panel pFirst = new Panel();
            tcFirst.Controls.Add(pFirst);
            pFirst.ID = "idFirst";
            pFirst.CssClass = "icon-edn-first icnListAct";
            pFirst.Attributes.Add("onclick", String.Concat(firstPageFct, "(", strNavPageFunctionsParameters, ")"));

            // précédente
            TableCell tcPrev = new TableCell();
            tcPrev.Attributes.Add("width", "15px");
            tr1.Cells.Add(tcPrev);
            Panel pPrev = new Panel();
            tcPrev.Controls.Add(pPrev);
            pPrev.ID = "idPrev";
            pPrev.CssClass = "icon-edn-prev fLeft icnListAct";
            pPrev.Attributes.Add("onclick", String.Concat(prevPageFct, "(", strNavPageFunctionsParameters, ")"));

            //Num Page
            TableCell tcNumPage = new TableCell();
            tcNumPage.Attributes.Add("width", "8%");
            tcNumPage.Attributes.Add("class", "numpage");
            tcNumPage.Attributes.Add("align", "center");
            tr1.Cells.Add(tcNumPage);

            System.Web.UI.WebControls.Table tbNum = new System.Web.UI.WebControls.Table();
            tcNumPage.Controls.Add(tbNum);

            tbNum.Attributes.Add("width", "100%");
            tbNum.Attributes.Add("cellpadding", "0");
            tbNum.Attributes.Add("cellspacing", "0");
            tbNum.Attributes.Add("border", "0");

            TableRow trNP = new TableRow();
            tbNum.Rows.Add(trNP);
            TableCell tcNp = new TableCell();
            trNP.Cells.Add(tcNp);
            tcNp.Style.Add("text-align", "right");
            tcNp.Style.Add("width", "50%");

            HtmlGenericControl span = new HtmlGenericControl("span");
            tcNp.Controls.Add(span);
            HtmlInputText inpt = new HtmlInputText("text");
            span.Controls.Add(inpt);
            inpt.Attributes.Add("class", "pagInput");
            inpt.ID = "inputNumpage";
            inpt.Name = "inputNumpage";
            inpt.Size = 1;
            inpt.Style.Add("display", "none");
            string strSelectPageFctWithParam = String.Concat(selectPageFct, "(", strSelectPageFunctionParameters, ")");
            inpt.Attributes.Add("onblur", strSelectPageFctWithParam);
            inpt.Attributes.Add("onkeypress", String.Concat("if (isValidationKey(event)) { ", strSelectPageFctWithParam, "; }"));

            TableCell tcNpD = new TableCell();
            trNP.Cells.Add(tcNpD);
            tcNpD.Style.Add("text-align", "left");
            tcNpD.Style.Add("width", "50%");
            tcNpD.ID = "tdNumpage";

            Panel pDivNumPage = new Panel();
            tcNpD.Controls.Add(pDivNumPage);
            pDivNumPage.ID = "divNumpage";

            //Num Next
            TableCell tcNext = new TableCell();
            tcNext.Attributes.Add("width", "15px");
            tr1.Cells.Add(tcNext);

            Panel pNext = new Panel();
            tcNext.Controls.Add(pNext);
            pNext.ID = "idNext";
            pNext.CssClass = "icon-edn-next fRight icnListAct";
            pNext.Attributes.Add("onclick", String.Concat(nextPageFct, "(", strNavPageFunctionsParameters, ")"));

            // #39926 CRU : Si on n'est pas sur la dernière page, l'icône next ne doit pas être "actif"
            //if (_nPage < Pref.Context.Paging.NbPage)
            //   pNext.Style.Add("background-color", Pref.ThemeXRM.Color);

            //Last Page
            TableCell tcLast = new TableCell();
            tcLast.Attributes.Add("width", "20px");
            tr1.Cells.Add(tcLast);
            Panel pLast = new Panel();
            tcLast.Controls.Add(pLast);
            pLast.ID = "idLast";
            pLast.CssClass = "icnListAct icon-edn-last";
            pLast.Attributes.Add("onclick", String.Concat(lastPageFct, "(", strNavPageFunctionsParameters, ")"));

            return tbListAction;
        }

        private TableCell GetSearch(bool bDrawSearchField, bool bHideSearchField, string searchFct = "launchSearch")
        {
            TableCell tcSearch = new TableCell();
            tcSearch.CssClass = "eFSTD";
            tcSearch.Style.Add("width", "42%");

            if (bDrawSearchField)
            {
                Panel pSearch = new Panel();
                tcSearch.Controls.Add(pSearch);
                pSearch.ID = "eFS";
                pSearch.CssClass = "eFSContainer";

                HtmlInputText inpt = new HtmlInputText("text");
                pSearch.Controls.Add(inpt);

                inpt.Attributes.Add("onfocus", " searchFocus();");
                inpt.Attributes.Add("onblur", " searchBlur();");
                inpt.Attributes.Add("title", "");
                inpt.Attributes.Add("class", "eFSInput");
                inpt.Attributes.Add("onkeyup", String.Concat(searchFct, "(this.value, event);"));
                inpt.Attributes.Add("maxlength", "100");
                inpt.Name = "q";
                inpt.ID = "eFSInput";
                inpt.Value = CurrentSearch;

                Panel pStatus = new Panel();
                pSearch.Controls.Add(pStatus);
                pStatus.ID = "eFSStatus";

                //Affichage du champ de recherche
                if (!bHideSearchField)
                    pSearch.Style.Add("display", "none");
            }
            else
                tcSearch.Controls.Add(new LiteralControl("&nbsp;"));

            return tcSearch;
        }

        /// <summary>
        /// Recuperer le filtre notation
        /// </summary>
        /// <returns></returns>
        private List<ListItem> getNotationFilter()
        {
            // Alimentation de la liste des notations à partir de celles définies sur les extensions
            IDictionary<string, double?> extensionNotationsFilter = new Dictionary<string, double?>();
            foreach (eAdminExtension extension in _includedExtensionList)
            {
                if (extension.Infos.Notation.Key != null &&
                    extension.Infos.Notation.Value != null &&
                    !extensionNotationsFilter.ContainsKey(extension.Infos.Notation.Key.Trim()))
                    extensionNotationsFilter.Add(extension.Infos.Notation);
            }
            foreach (eAdminExtension extension in _hiddenExtensionList)
            {
                if (extension.Infos.Notation.Key != null &&
                    extension.Infos.Notation.Value != null &&
                    !extensionNotationsFilter.ContainsKey(extension.Infos.Notation.Key.Trim()))
                    extensionNotationsFilter.Add(extension.Infos.Notation);
            }
            foreach (eAdminExtension extension in _excludedExtensionList)
            {
                if (extension.Infos.Notation.Key != null &&
                    extension.Infos.Notation.Value != null &&
                    !extensionNotationsFilter.ContainsKey(extension.Infos.Notation.Key.Trim()))
                    extensionNotationsFilter.Add(extension.Infos.Notation);
            }

            // Alimentation de la liste des catégories à partir de celles définies sur l'objet Catalogue lui-même (récupérées via l'API)
            foreach (KeyValuePair<string, double?> catalogValue in _notations)
            {
                if (!extensionNotationsFilter.ContainsKey(catalogValue.Key.Trim()))
                    extensionNotationsFilter.Add(catalogValue);
            }

            // Ajout de la notation <Aucune>
            if (!extensionNotationsFilter.ContainsKey("-1"))
                extensionNotationsFilter.Add(new KeyValuePair<string, double?>("-1", null));

            List<ListItem> listItemNotation = new List<ListItem>();
            ListItem item;

            //Creation select notation
            //Note null sur une extension = pas de note encore attribuée = -1 dans le SELECT
            //La valeur TOUS étant représentée par value="" dans le SELECT
            //On utilise un booléen pour savoir si un élément de la liste correspond au filtre en cours, afin de déterminer s'il faudra se positionner sur <TOUS>
            //par défaut
            bool elementSelected = false;
            foreach (KeyValuePair<string, double?> notation in extensionNotationsFilter)
            {
                item = new ListItem();
                item.Text = notation.Value == null ? String.Concat("<", eResApp.GetRes(Pref, 436), ">") : notation.Value.ToString().Trim(); // 436 = Aucun
                item.Value = notation.Key == null ? "-1" : notation.Key.ToString();
                item.Selected = !elementSelected && CurrentFilterNotation != null && (notation.Value == CurrentFilterNotation || notation.Value == null && CurrentFilterNotation == -1);
                elementSelected = item.Selected ? true : elementSelected; // ne pas remettre cette variable à false si elle a précédemment été mise à true
                listItemNotation.Add(item);
            }
            listItemNotation = listItemNotation.OrderBy(x => x.Text).ToList();
            listItemNotation.Add(quickFilterItemAll(!elementSelected));
            return listItemNotation;
        }

        /// <summary>
        /// Retourne le filtre category 
        /// </summary>
        /// <returns></returns>
        private List<ListItem> getCategoryFilter()
        {
            // Alimentation de la liste des catégories à partir de celles définies sur les extensions
            IDictionary<string, string> extensionCategoriesFilter = new Dictionary<string, string>();
            foreach (eAdminExtension extension in _includedExtensionList)
            {
                foreach (KeyValuePair<string, string> extensionCategory in extension.Infos.Categories)
                {
                    if (!extensionCategoriesFilter.ContainsKey(extensionCategory.Key.Trim()))
                        extensionCategoriesFilter.Add(extensionCategory);
                }
            }
            foreach (eAdminExtension extension in _hiddenExtensionList)
            {
                foreach (KeyValuePair<string, string> extensionCategory in extension.Infos.Categories)
                {
                    if (!extensionCategoriesFilter.ContainsKey(extensionCategory.Key.Trim()))
                        extensionCategoriesFilter.Add(extensionCategory);
                }
            }
            foreach (eAdminExtension extension in _excludedExtensionList)
            {
                foreach (KeyValuePair<string, string> extensionCategory in extension.Infos.Categories)
                {
                    if (!extensionCategoriesFilter.ContainsKey(extensionCategory.Key.Trim()))
                        extensionCategoriesFilter.Add(extensionCategory);
                }
            }

            // Alimentation de la liste des catégories à partir de celles définies sur l'objet Catalogue lui-même (récupérées via l'API)
            foreach (KeyValuePair<string, string> catalogValue in _categories)
            {
                if (!extensionCategoriesFilter.ContainsKey(catalogValue.Key.Trim()))
                    extensionCategoriesFilter.Add(catalogValue);
            }

            // Ajout de la catégorie <Aucune>
            if (!extensionCategoriesFilter.ContainsKey("-1"))
                extensionCategoriesFilter.Add(new KeyValuePair<string, string>("-1", "-1"));

            List<ListItem> listItemCategory = new List<ListItem>();
            ListItem item;

            //Creation select catégories
            //On utilise un booléen pour savoir si un élément de la liste correspond au filtre en cours, afin de déterminer s'il faudra se positionner sur <TOUS>
            //par défaut
            bool elementSelected = false;
            foreach (KeyValuePair<string, string> category in extensionCategoriesFilter)
            {
                item = new ListItem();
                item.Text = category.Value == "-1" ? String.Concat("<", eResApp.GetRes(Pref, 238), ">") : category.Value.Trim(); // 238 = Aucune
                item.Value = category.Key.ToString();
                item.Selected = !elementSelected && CurrentFilterCategory != null && category.Key.Trim() == CurrentFilterCategory.Trim();
                elementSelected = item.Selected ? true : elementSelected; // ne pas remettre cette variable à false si elle a précédemment été mise à true
                listItemCategory.Add(item);
            }
            listItemCategory = listItemCategory.OrderBy(x => x.Value).ToList();
            listItemCategory.Add(quickFilterItemAll(!elementSelected));
            return listItemCategory;
        }

        /// <summary>
        /// Renvoi le filtre all
        /// </summary>
        /// <param name="selected">Vrai si Tous est selectionné</param>
        /// <returns>Le listItem tous</returns>
        private ListItem quickFilterItemAll(Boolean selected)
        {
            ListItem item = new ListItem();
            item.Text = eResApp.GetRes(Pref, 22).ToUpper();
            item.Attributes.Add("title", item.Text);
            item.Value = "";
            item.Selected = selected;
            item.Attributes.Add("style", String.Concat("background-color:", eConst.COL_USR_SPEC, ";"));
            return item;
        }
    }
}