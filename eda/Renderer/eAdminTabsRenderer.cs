using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using Com.Eudonet.Internal.tools.coremodel;
using EudoQuery;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Renderer de l'administration des onglets
    /// </summary>
    public class eAdminTabsRenderer : eAdminModuleRenderer
    {
        Panel _pnlContents;
        HtmlGenericControl _pnlSubTitle;
        Dictionary<Int32, String> _tabList;
        Dictionary<Int32, String> _deletedOrVirtualTabList;
        eudoDAL _dal;
        const string _TABLIST_TABLE = "tabListTable";
        /// <summary>
        /// Liste des produits disponibles
        /// </summary>
        List<eProduct> _productsList = new List<eProduct>();

        /// <summary>
        /// Liste des tables non supprimables
        /// </summary>
        HashSet<Int32> _tabListNotDeletable = new HashSet<Int32>();

        /// <summary>
        /// Colonne sur laquelle porte le tri
        /// </summary>
        public string SortCol { get; internal set; }
        /// <summary>
        /// Sens du tri (0 = Ascendant, 1 = Descendant)
        /// </summary>
        public int Sort { get; internal set; }
        /// <summary>
        /// Filtre de recherche à appliquer sur la liste
        /// </summary>
        public string Search { get; internal set; }

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public eAdminTabsRenderer(ePref pref, int nWidth, int nHeight)
            : base(pref)
        {
            _width = nWidth - 30;
            _height = nHeight - 60; // marge pour tenir compte de l'affichage du titre et de la barre de recherche
            SortCol = "LABEL";
            Sort = 0;
            Search = String.Empty;
        }

        /// <summary>
        /// Génération du renderer de la liste des onglets en admin
        /// </summary>
        /// <param name="pref">Objet ePref</param>
        /// <param name="nWidth">Largeur disponible pour le rendu</param>
        /// <param name="nHeight">Hauteur disponible pour le rendu</param>
        /// <returns></returns>
        public static eAdminTabsRenderer CreateAdminTabsRenderer(ePref pref, int nWidth, int nHeight)
        {
            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();

            return new eAdminTabsRenderer(pref, nWidth, nHeight);
        }

        /// <summary>
        /// Initialisation des params
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            try
            {

                if (!eFeaturesManager.IsFeatureAvailable(Pref, eConst.XrmFeature.AdminTabs))
                    return false;

                _dal = eLibTools.GetEudoDAL(Pref);
                _dal.OpenDatabase();

                string error = String.Empty;
                List<EdnType> fileTypes = new List<EdnType>()
                {
                    EdnType.FILE_ADR,
                    EdnType.FILE_MAIL,
                    EdnType.FILE_MAIN,
                    EdnType.FILE_DISCUSSION,
                    EdnType.FILE_PLANNING,
                    EdnType.FILE_RELATION,
                    EdnType.FILE_SMS,
                    EdnType.FILE_STANDARD,
                    EdnType.FILE_TARGET,
                    EdnType.FILE_HISTO, /* #51211 */
                    EdnType.FILE_PJ, /*#51 628, #50 700*/                                        
                    EdnType.FILE_BKMWEB, // suprimer de la liste des signet visible. du coup, on peut plus non plus les supprimer
                    //EdnType.FILE_GRID
                };
                _tabList = eDataTools.GetFiles(_dal, Pref, fileTypes, out error, ref _deletedOrVirtualTabList, SortCol, Sort, true);

                var lstDescAdvInfos = eLibTools.GetDescAdvInfo(Pref, new List<int>(_tabList.Keys), new List<DESCADV_PARAMETER>() {
                    DESCADV_PARAMETER.NOAMDMIN, DESCADV_PARAMETER.HIDDEN_TAB_PRODUCT
                });

                //liste des tables non administrable
                IEnumerable<int> ieNoAdmin =
                        // récupération du descadv NOADMIN
                        lstDescAdvInfos
                        //Filtre sur ceux à "1"
                        .Where(myKvpWhere => myKvpWhere.Value.Find(descAdvInfo => descAdvInfo.Item1 == DESCADV_PARAMETER.NOAMDMIN && descAdvInfo.Item2 == "1") != null)
                        //on a besoin que du descid
                        .Select(myKvpSelect => myKvpSelect.Key);

                //liste des tables non administrable
                IEnumerable<int> ieHiddenProduct =
                           // récupération du descadv NOADMIN
                           lstDescAdvInfos
                           //Filtre sur ceux à "1"
                           .Where(myKvpWhere => myKvpWhere.Value.Find(descAdvInfo => descAdvInfo.Item1 == DESCADV_PARAMETER.HIDDEN_TAB_PRODUCT && descAdvInfo.Item2 == "1") != null)
                           //on a besoin que du descid
                           .Select(myKvpSelect => myKvpSelect.Key);

                if (Pref.User.UserLevel < (int)UserLevel.LEV_USR_SUPERADMIN)
                {
                    foreach (int k in ieHiddenProduct)
                        _tabList.Remove(k);
                }

                //_tabList = _tabList.Where(a => !lNoAdmin.Contains(a.Key)).ToDictionary(z => z.Key, z => z.Value);
                foreach (int k in ieNoAdmin)
                    _tabList.Remove(k);


                if (_deletedOrVirtualTabList == null)
                    _deletedOrVirtualTabList = new Dictionary<int, string>();


                _tabList.Remove((int)TableType.XRMGRID);
                _tabList.Remove((int)TableType.XRMWIDGET);
                _tabList.Remove((int)TableType.PRODUCT);
                if (Pref.User.UserLevel < (int)UserLevel.LEV_USR_SUPERADMIN)
                    _tabList.Remove((int)TableType.PAYMENTTRANSACTION);

                // On ajoute les tables orphelines et virtuelles (déclarées dans DESC mais inexistantes en base) dans le tableau pour affichage
                // spécifique, invitant l'administrateur à faire le ménage. On fusionne les 2 tableaux pour que l'affichage se fasse
                // au même niveau avec possibilités de tri ou recherche
                foreach (KeyValuePair<Int32, String> orphanedTab in _deletedOrVirtualTabList)
                    _tabList.Add(orphanedTab.Key, orphanedTab.Value);

                // Récup liste des produits
                _productsList = eProduct.GetProductsList(this.Pref, _dal);


                //liste des tables event Étapes
                //liste des tables avec Mode opération
                IEnumerable<int> ieEventStep =
                        // récupération du descadv NOADMIN
                        eLibTools.GetDescAdvInfo(Pref, new List<int>(_tabList.Keys), new List<DESCADV_PARAMETER>() { DESCADV_PARAMETER.EVENT_STEP_ENABLED, DESCADV_PARAMETER.IS_EVENT_STEP })
                        //Filtre sur ceux à "1"
                        .Where(myKvpWhere => myKvpWhere.Value.Find(descAdvInfo => descAdvInfo.Item1 == DESCADV_PARAMETER.IS_EVENT_STEP && descAdvInfo.Item2 == "1") != null)
                        //on a besoin que du descid
                        .Select(myKvpSelect => myKvpSelect.Key);

                _tabListNotDeletable.UnionWith(ieEventStep);
            }
            finally
            {
                _dal.CloseDatabase();
            }

            return base.Init();
        }

        /// <summary>
        /// Génération du contenu
        /// </summary>
        /// <returns>true si le contenu a été généré avec succès</returns>
        protected override bool Build()
        {
            BuildSubTitle(String.Concat(_tabList.Count, " ", eResApp.GetRes(Pref, 217)));

            BuildSearchBar();

            InitInnerContainer();

            BuildTabList();

            return true;
        }

        /// <summary>
        /// On instancie le container 
        /// </summary>
        private void InitInnerContainer()
        {
            _pnlContents = new Panel();
            _pnlContents.CssClass = "adminCntnt adminCntntTabs";
            _pnlContents.Style.Add(HtmlTextWriterStyle.Width, String.Concat(_width, "px"));
            _pnlContents.Style.Add(HtmlTextWriterStyle.Height, String.Concat(_height.ToString(), "px"));
            _pgContainer.Controls.Add(_pnlContents);
        }

        /// <summary>
        /// Construit un sous titre
        /// </summary>
        /// <param name="subTitle"></param>
        private void BuildSubTitle(String subTitle)
        {
            _pnlSubTitle = new HtmlGenericControl("div");
            _pnlSubTitle.Attributes.Add("class", "adminCntntTtl adminCntntTtlTabs");
            _pnlSubTitle.InnerText = subTitle;
            _pgContainer.Controls.Add(_pnlSubTitle);
        }
        /// <summary>
        /// Construit la barre de recherche (côté client)
        /// </summary>
        private void BuildSearchBar()
        {

            Panel eFS = eAdminTools.CreateSearchBar("eFSContainerAdminTabs", _TABLIST_TABLE, onKeyup: String.Concat("nsAdminTabsList.search(this.value, event, '", _TABLIST_TABLE, "', false)"), initialValue: Search);

            _pgContainer.Controls.Add(eFS);
        }

        /// <summary>
        /// Construit la liste des onglets
        /// </summary>
        private void BuildTabList()
        {


            // Génération des données du tableau
            List<string> tabTableColumnHeaders = new List<string>()
            {
                String.Empty, // Icone de l'onglet
                eResApp.GetRes(Pref, 223), // Libellé
                eResApp.GetRes(Pref, 105), // Type
                eResApp.GetRes(Pref, 130), // Info-bulle
                eResApp.GetRes(Pref, 8481), // Fonctionnalité
                eResApp.GetRes(Pref, 7614), // Réf. (DescID)
                eResApp.GetRes(Pref, 3121), // Nouvel Eudonet X
                //,eResApp.GetRes(Pref, 7586), // Catégorie
                //eResApp.GetRes(Pref, 5078) // Créé par
            };

            if (Pref.User.UserLevel > (int)UserLevel.LEV_USR_ADMIN)
            {
                tabTableColumnHeaders.Add(eResApp.GetRes(Pref, 1432));
            }

            List<List<string>> tabTableRowCellIcon = new List<List<string>>();
            List<List<string>> tabTableRowCellLabels = new List<List<string>>();
            List<List<string>> tabTableRowCellLinks = new List<List<string>>();
            List<List<string>> tabTableRowCellTooltips = new List<List<string>>();
            List<List<AttributeCollection>> tabTableRowCellAttributes = new List<List<AttributeCollection>>();
            List<string> tabTableRowLinks = new List<string>();
            List<string> tabTableRowTooltips = new List<string>();
            List<AttributeCollection> tabTableRowAttributes = new List<AttributeCollection>();

            string strError = String.Empty;
            List<eAdminTableInfos> liTabs = new List<eAdminTableInfos>();
            foreach (KeyValuePair<Int32, String> tab in _tabList)
            {
                eAdminTableInfos tableInfos = null;
                if (tab.Key > 0)
                {
                    try
                    {
                        tableInfos = new eAdminTableInfos(Pref, tab.Key);
                    }
                    catch
                    {
                        tableInfos = null;
                    }
                }



                // Si les informations de table ne peuvent pas être chargées pour l'onglet actuel (ex : Accueil, table système non référencée...), on ne
                // l'affiche pas dans la liste, car il sera alors impossible de l'administrer
                // TODO: l'afficher, mais le matérialiser ? Comme un onglet supprimé ?
                if (tableInfos == null)
                    continue;

                if (liTabs.Contains(tableInfos) || tableInfos.EdnType == EdnType.FILE_GRID)
                    continue;

                liTabs.Add(tableInfos);
            }


            //on applique le tri sur l'ensemble des tables, virtuelles ou non
            switch (SortCol.ToLower())
            {
                case "label":
                    if (Sort == 1)
                        liTabs = liTabs.OrderByDescending(tab => tab.TableLabel).ToList<eAdminTableInfos>();
                    else
                        liTabs = liTabs.OrderBy(tab => tab.TableLabel).ToList<eAdminTableInfos>();

                    break;
                case "type":
                    if (Sort == 1)
                        liTabs = liTabs.OrderByDescending(tab => tab.EdnType).ToList<eAdminTableInfos>();
                    else
                        liTabs = liTabs.OrderBy(tab => tab.EdnType).ToList<eAdminTableInfos>();
                    break;
                case "feature":
                    if (Sort == 1)
                        liTabs = liTabs.OrderByDescending(tab => tab.Feature).ToList<eAdminTableInfos>();
                    else
                        liTabs = liTabs.OrderBy(tab => tab.Feature).ToList<eAdminTableInfos>();
                    break;
                case "tooltip":
                    if (Sort == 1)
                        liTabs = liTabs.OrderByDescending(tab => tab.ToolTipText).ToList<eAdminTableInfos>();
                    else
                        liTabs = liTabs.OrderBy(tab => tab.ToolTipText).ToList<eAdminTableInfos>();
                    break;
                case "descid":
                    if (Sort == 1)
                        liTabs = liTabs.OrderByDescending(tab => tab.DescId).ToList<eAdminTableInfos>();
                    else
                        liTabs = liTabs.OrderBy(tab => tab.DescId).ToList<eAdminTableInfos>();
                    break;
                case "eudonetx":
                    if (Sort == 1)
                        liTabs = liTabs.OrderByDescending(tab => tab.EudonetXIrisBlackStatus).ToList<eAdminTableInfos>();
                    else
                        liTabs = liTabs.OrderBy(tab => tab.EudonetXIrisBlackStatus).ToList<eAdminTableInfos>();
                    break;
                default:
                    break;
            }

            string feature = string.Empty;
            string productName = string.Empty;
            eProduct product;

            foreach (eAdminTableInfos tableInfos in liTabs)
            {

                // On matérialise les tables orphelines (référencées dans DESC mais inexistantes en base)
                // Sauf dans le cas des signets et onglets Web qui sont virtuels (référencés dans DESC mais non créés en tant que tables SQL)
                bool isWebTabOrBkm = tableInfos.EdnType == EdnType.FILE_BKMWEB || tableInfos.EdnType == EdnType.FILE_WEBTAB || tableInfos.EdnType == EdnType.FILE_GRID;
                bool isDeletedTab = _deletedOrVirtualTabList.ContainsKey(tableInfos.DescId) && !isWebTabOrBkm;

                string label = HtmlTools.StripHtml(tableInfos.TableLabel);
                string link = String.Empty;
                if (!isDeletedTab /*&& !isWebTabOrBkm*/ && tableInfos.EdnType != EdnType.FILE_GRID)
                    link = String.Concat("nsAdmin.loadAdminFile(", tableInfos.DescId, ");");
                else
                    link = "eAlert(2, 'Administration', \"L'administration des onglets et signets Web n'est pas encore disponible.\");";
                string tooltip = String.Empty;
                string tabType = eAdminTools.GetTabTypeName(tableInfos, Pref, out tooltip);
                if (isDeletedTab)
                    tabType = String.Concat(tabType, " (", eResApp.GetRes(Pref, 776).ToUpper(), ")"); // "(INTROUVABLE)"

                feature = tableInfos.Feature;

                productName = string.Empty;
                if (tableInfos.ProductID > 0)
                {
                    product = _productsList.FirstOrDefault(p => p.ProductID == tableInfos.ProductID);
                    if (product != null)
                        productName = product.ProductCode;
                }


                string descId = String.Concat(tableInfos.TabName, " (", tableInfos.DescId.ToString(), ")", ((!String.IsNullOrEmpty(productName)) ? "  <span>" + productName + "</span>" : ""));
                tooltip = tableInfos.ToolTipText;
                // TODO VALEUR A CONVERTIR EN INT
                string eudonetX = String.Empty;
                switch (tableInfos.EudonetXIrisBlackStatus)
                {

                    case EUDONETX_IRIS_BLACK_STATUS.DISABLED:
                    default:
                        eudonetX = eResApp.GetRes(Pref, 1459); // Désactivé
                        break;
                    case EUDONETX_IRIS_BLACK_STATUS.ENABLED: eudonetX = eResApp.GetRes(Pref, 1460); break; // Activé
                    case EUDONETX_IRIS_BLACK_STATUS.PREVIEW: eudonetX = eResApp.GetRes(Pref, 3122); break; // Prévisualisation
                }
                //string category = String.Empty; // TODO
                //string createdBy = String.Empty; // TODO

                //tabTableRowLinks.Add(link); // on ajoute le lien sur l'intégralité de la ligne;
                tabTableRowTooltips.Add(tooltip); // on ajoute le tooltip sur l'intégralité de la ligne               

                List<string> tabCellLabels = new List<string> {
                    String.Empty,
                    label,
                    tabType,
                    tooltip,
                    feature,
                    descId,
                    eudonetX
                };

                List<string> tabCellLinks = new List<string> {
                    link,
                    link,
                    String.Empty,
                    String.Empty,
                    string.Empty,
                    String.Empty,
                    String.Empty
            };

                List<string> tabCellTooltips = new List<string>(); // TODO ?

                AttributeCollection tabCellIconAttributes = new AttributeCollection(new StateBag());
                tabCellIconAttributes.Add("class", "cell adminTabsIcon");

                AttributeCollection tabCellLabelAttributes = new AttributeCollection(new StateBag());
                tabCellLabelAttributes.Add("class", "cell adminTabsLabel");

                AttributeCollection tabCellTabTypeAttributes = new AttributeCollection(new StateBag());
                tabCellTabTypeAttributes.Add("class", "cell adminTabsTabType");
                AttributeCollection tabCellTooltipAttributes = new AttributeCollection(new StateBag());
                tabCellTooltipAttributes.Add("class", "cell adminTabsTooltip");
                AttributeCollection tabCellFeatureAttributes = new AttributeCollection(new StateBag());
                tabCellFeatureAttributes.Add("class", "cell adminTabsFeature");
                AttributeCollection tabCellDescIdAttributes = new AttributeCollection(new StateBag());
                tabCellDescIdAttributes.Add("class", "cell adminTabsDescId");
                AttributeCollection tabCellEudonetXAttributes = new AttributeCollection(new StateBag());
                tabCellEudonetXAttributes.Add("class", "cell adminTabsEudonetX");
                tabCellEudonetXAttributes.Add("onclick", "nsAdminTabsList.edit(event, this)");

                List<AttributeCollection> tabCellAttributes = new List<AttributeCollection>()
                {
                    tabCellIconAttributes,
                    tabCellLabelAttributes,
                    tabCellTabTypeAttributes,
                    tabCellTooltipAttributes,
                    tabCellFeatureAttributes,
                    tabCellDescIdAttributes,
                    tabCellEudonetXAttributes
                };



                //Si super admin ou + ajout de l'admin du status "produit caché"
                if (Pref.User.UserLevel > (int)UserLevel.LEV_USR_ADMIN)
                {
                    tabCellLabels.Add("");
                    tabCellLinks.Add(String.Concat("nsAdmin.UpdateProductStatus(this);"));



                    AttributeCollection tabCellHiddenProuctsAttributes = new AttributeCollection(new StateBag());
                    tabCellHiddenProuctsAttributes.Add("class", "cell adminHiddenTabProduct icon-square");
                    tabCellHiddenProuctsAttributes.Add("ednval", tableInfos.TabHiddenProduct ? "1" : "0");
                    tabCellAttributes.Add(tabCellHiddenProuctsAttributes);

                }


                tabTableRowCellLabels.Add(tabCellLabels);
                tabTableRowCellLinks.Add(tabCellLinks);
                tabTableRowCellTooltips.Add(tabCellTooltips);
                tabTableRowCellAttributes.Add(tabCellAttributes);

                AttributeCollection tabRowAttribute = new AttributeCollection(new StateBag());
                tabRowAttribute.Add("did", tableInfos.DescId.ToString());
                tabRowAttribute.Add("tabtype", ((int)tableInfos.TabType).ToString());
                tabRowAttribute.Add("edntype", ((int)tableInfos.EdnType).ToString());
                tabRowAttribute.Add("icon", tableInfos.Icon);
                if (isDeletedTab)
                    tabRowAttribute.Add("deleted", "1");
                // #56 749 - On masque les lignes ne correspondant pas au critère de recherche mémorisé, si défini
                // Les lignes doivent être physiquement présentes dans le DOM afin de pouvoir être réaffichées en JS sans recharger la page si on change le critère de
                // recherche
                if (String.IsNullOrEmpty(Search) || label.Contains(Search) || tabType.Contains(Search) || tooltip.Contains(Search) || feature.Contains(Search) || descId.Contains(Search))
                    tabTableRowAttributes.Add(tabRowAttribute);
            }

            HtmlTable tabTable = GetTable(
                _TABLIST_TABLE,
                tabTableColumnHeaders,

                tabTableRowCellLabels,
                tabTableRowCellLinks,
                tabTableRowCellTooltips,
                tabTableRowCellAttributes,

                tabTableRowLinks,
                tabTableRowTooltips,
                tabTableRowAttributes,
                "adminTabsList",
                encodeRowCellLabels: false
            );

            #region Icônes de tri et identifiants des colonnes
            // Ajout des 6 premières colonnes sans condition
            List<string> tableHeaderIDs = new List<string> { "Icon", "Label", "Type", "Tooltip", "Feature", "DescId" };
            // Ajout de la colonne n°7 ("Produit") sous condition
            if (Pref.User.UserLevel >= (int)UserLevel.LEV_USR_SUPERADMIN)
                tableHeaderIDs.Add("HiddenProduct");
            // Puis ajout de la dernière colonne, en 7ème ou 8ème position selon le cas précédent
            tableHeaderIDs.Add("EudonetX");

            // Ciblage de la ligne du tableau correspondant aux en-têtes (th)
            HtmlTableRow tr = tabTable.Rows[0];
            // Puis parcours de chaque en-tête
            for (int i = 0; i < tr.Cells.Count; i++)
            {
                // Ajout de l'ID pour le ciblage JavaScript dans la fonction de tri (eAdmin.js -> nsAdminTabsList.sort())
                tr.Cells[i].ID = String.Concat("tabListTableHeader", tableHeaderIDs[i]);
                // Ajout des icônes de tri, sauf pour la première colonne (icône)
                if (i > 0)
                    AddSortIconsToTableHeader(tr.Cells[i], String.Concat("nsAdminTabsList.sort('", tableHeaderIDs[i], "', <SORTORDER>, nsAdminTabsList.currentSearch, false);"));
                // US #4057 - TK #6528 - Ajout du paramètre correspondant dans DESCADV pour l'édition en ligne de la valeur, si applicable
                switch (tableHeaderIDs[i]) { 
                    case "EudonetX":
                        tr.Cells[i].Attributes.Add("descadvparameter", DESCADV_PARAMETER.ERGONOMICS_IRIS_BLACK.GetHashCode().ToString());
                    break;
                }
            }
            #endregion

            #region Autres icônes sur le contenu du tableau
            // On démarre à 1 pour exclure la ligne d'en-tête précédemment traitée
            for (int i = 1; i < tabTable.Rows.Count; i++)
            {
                tr = tabTable.Rows[i];
                //icone de l'onglet
                HtmlGenericControl icn = new HtmlGenericControl("div");
                tr.Cells[0].Controls.Add(icn);
                icn.Attributes.Add("class", eFontIcons.GetFontClassName(tr.Attributes["icon"]));
                tr.Attributes.Remove("icon");

                Int32 did = eLibTools.GetNum(tr.Attributes["did"]);
                if (did > 1000 && did < (int)TableType.HISTO && !_tabListNotDeletable.Contains(did))
                {
                    //icone de suppression
                    HtmlGenericControl divDel = new HtmlGenericControl("div");
                    tr.Cells[1].Controls.Add(divDel);
                    divDel.Attributes.Add("class", "icon-delete");
                    divDel.Attributes.Add("title", eResApp.GetRes(Pref, 19));
                    divDel.Attributes.Add("onclick", "nsAdmin.deleteTable(event, this);");
                    divDel.Attributes.Add("ednType", ((int)liTabs[i - 1].EdnType).ToString());
                }

            }
            #endregion

            _pnlContents.Controls.Add(tabTable);
        }


    }
}