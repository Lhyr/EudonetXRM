using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Classe de rendu d'une liste, pour un widget de type Liste
    /// </summary>
    /// <seealso cref="Com.Eudonet.Xrm.eListMainRenderer" />
    public class eListFilesXrmWidgetRenderer : eListMainRenderer
    {
        /// <summary>
        /// The columns list separated with ;
        /// </summary>
        protected String _listCol;
        private int _filterId = 0;
        private List<eListWidget.ExpressFilterInfo> _filterInfo = null;
        private bool _histo = true;
        private eXrmWidgetContext _context = new eXrmWidgetContext(0);
        /// <summary>
        /// Gets the nb total rows.
        /// </summary>
        /// <value>
        /// The nb total rows.
        /// </value>
        public int DisplayedTotalRows { get; private set; }
        /// <summary>
        /// Renvoie le nombre réel de lignes du fichier (pouvant être supérieur au nombre affiché)
        /// </summary>
        /// <value>
        /// The nb total rows.
        /// </value>
        public int FileTotalRows { get; private set; }
        /// <summary>
        /// Indique si le nombre total de fiches indiqué a été calculé
        /// </summary>
        public bool TotalRowsAvailable { get; private set; }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pref">The preference.</param>
        /// <param name="height">The height.</param>
        /// <param name="width">The width.</param>
        /// <param name="tab">The tab.</param>
        /// <param name="page">The page.</param>
        /// <param name="rows">The rows.</param>
        /// <param name="listCol">The list col.</param>
        /// <param name="filterId">The filter identifier.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="histo">if set to <c>true</c> [histo].</param>
        /// <param name="context">The context.</param>
        private eListFilesXrmWidgetRenderer(ePref pref, int height, int width, int tab, int page, int rows, String listCol, int filterId, List<eListWidget.ExpressFilterInfo> filter, bool histo, eXrmWidgetContext context) : base(pref)
        {
            _rType = RENDERERTYPE.ListRendererMain;
            _height = height;
            _width = width;
            _page = page;
            _rows = (rows == 0) ? eLibConst.MAX_ROWS : rows;
            _rowsCalculated = rows;
            _tab = tab;
            _bFullList = false;
            _filterId = filterId;

            _filterInfo = filter;
            _listCol = (String.IsNullOrEmpty(listCol)) ? (_tab + 1).ToString() : String.Concat((_tab + 1), ";", listCol);
            _histo = histo;

            if (context != null)
                _context = context;
        }

        /// <summary>
        /// Retourne le Renderer correspondant à un mode liste pour widget
        /// </summary>
        /// <param name="pref">The preference.</param>
        /// <param name="height">The height.</param>
        /// <param name="width">The width.</param>
        /// <param name="tab">The tab.</param>
        /// <param name="page">The page.</param>
        /// <param name="rows">The rows.</param>
        /// <param name="listCol">The list col.</param>
        /// <param name="filterId">The filter identifier.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="histo">if set to <c>true</c> [histo].</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        internal static eListFilesXrmWidgetRenderer GetListFilesXrmWidgetRenderer(ePref pref, int height, int width, int tab, int page, int rows, String listCol, int filterId = 0, List<eListWidget.ExpressFilterInfo> filter = null, bool histo = true, eXrmWidgetContext context = null)
        {
            eListFilesXrmWidgetRenderer rdr = new eListFilesXrmWidgetRenderer(pref, height, width, tab, page, rows, listCol, filterId, filter, histo, context);
            return rdr;
        }

        /// <summary>
        /// Génère l'objet _list du renderer
        /// </summary>
        /// <returns></returns>
        protected override void GenerateList()
        {
            _list = eListWidget.CreateListWidget(this.Pref, _tab, _rows, _page, _filterId, true, _filterInfo, _histo, _context);
            ((eListWidget)_list).ListCol = _listCol;
            _list.Generate();
            if (_list.ErrorMsg.Length > 0)
            {
                throw new Exception("GenerateList => " + _list.ErrorMsg);
            }
        }

        /// <summary>
        /// Construit la structure HTML de l'élément
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            if (base.Build())
            {
                // On détermine si le nombre total de fiches a bien été calculé
                // Idéalement, il faudrait pouvoir accéder à _list.Paging.HasCount, mais l'information n'est pas mise à disposition.
                // On se base donc sur le fait que le total soit supérieur à 0, ou non
                TotalRowsAvailable = _list.NbTotalRows > 0;
                int totalRows =
                    TotalRowsAvailable
                    ? _list.NbTotalRows // Cas où le comptage a été effectué (à la demande ou non) : NbTotalRows est > à 0
                    : _list.ListRecords.Count; // Cas où le comptage n'a pas été effectué (total indisponible)
                DisplayedTotalRows = (totalRows > _rows) ? _rows : totalRows;
                FileTotalRows = totalRows;

                this.PgContainer.Attributes.Add("onclick", "fldLClick(event)");

                return true;
            }
            return false;
        }

        /// <summary>
        /// Init de eListRendererMain
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            if (base.Init())
            {
                //_jsSortAsc = "oListWidget.sort(this, true)";
                //_jsSortDesc = "oListWidget.sort(this, false)";
                _jsDoFilter = "oListWidget.doFilter(this)";
                _isTableFilterable = true;

                return true;
            }
            return false;
        }
        /// <summary>
        /// On affiche la recherche
        /// </summary>
        public override Boolean DrawSearchField
        {
            get { return false; }
        }

        /// <summary>
        /// ajoute la cellule d'en tete contenant la case à cocher "selectionner tout"
        /// </summary>
        /// <param name="headerRow"></param>
        protected override void AddSelectCheckBoxHead(TableRow headerRow)
        {

        }

        /// <summary>
        /// TODO déplacer ca dans l'objet metier
        /// </summary>
        /// <param name="field"></param>
        protected override void BeforeRenderFieldHeaderIcon(Field field)
        {
            field.IsSortable = !Pref.AdminMode;
            //field.IsComputable = false;
            field.IsFiltrable = field.Format != FieldFormat.TYP_MEMO && !Pref.AdminMode; // Pas de filtre sur champ Mémo
            field.IsMovable = false;
        }

        /// <summary>
        /// Pas de checkbox pour les pages d'accueil
        /// </summary>
        /// <param name="row"></param>
        /// <param name="trDataRow"></param>
        /// <param name="sAltLineCss"></param>
        protected override void AddSelectCheckBox(eRecord row, TableRow trDataRow, String sAltLineCss)
        {
            return;
        }




        /// <summary>
        /// Traitement de fin de génération
        /// </summary>
        /// <returns></returns>
        protected override bool End()
        {
            base.End();

            AddFilterTip();

            _tblMainList.Attributes.Add("ednmode", "listwidget");
            //_tblMainList.Attributes.Add("fromHomepage", "1");
            return true;
        }

        /// <summary>
        /// identifie les paramètres de pagination
        /// Les listes filtre
        /// </summary>
        protected override void SetPagingInfo()
        {
            /*
            _tblMainList.Attributes.Add("eNbCnt", _list.NbTotalRows.ToString());
            _tblMainList.Attributes.Add("eNbTotal", _list.NbTotalRows.ToString());

            _tblMainList.Attributes.Add("eHasCount", "1");
            _tblMainList.Attributes.Add("nbPage", _list.NbPage.ToString());
            _tblMainList.Attributes.Add("cnton", "0");
            */
        }

        /// <summary>
        /// Ajoute les actions affichant la vcard au control passé en parametre
        /// </summary>
        /// <param name="webControl"></param>
        public override void VCMouseActionAttributes(WebControl webControl)
        {
            webControl.Attributes.Add("onmouseover", String.Concat("oListWidget.showHideMiniFile(this, 1)"));
            webControl.Attributes.Add("onmouseout", String.Concat("oListWidget.showHideMiniFile(this, 0)"));
        }

        /// <summary>
        /// Ajoute les actions affichant la minifiche au control passé en parametre
        /// </summary>
        /// <param name="webControl"></param>
        /// <param name="tab"></param>
        public override void VCMiniFileMouseActionAttributes(WebControl webControl, int tab)
        {
            webControl.Attributes.Add("vcMiniFileTab", tab.ToString());
            webControl.Attributes.Add("onmouseover", String.Concat("oListWidget.showHideMiniFile(this, 1)"));
            webControl.Attributes.Add("onmouseout", String.Concat("oListWidget.showHideMiniFile(this, 0)"));
        }

        /// <summary>
        /// Ajoute les attributes/classes sur l'icône "filtre" permettant de déterminer si le filtre est actif ou non
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="imgFilterCol">The img filter col.</param>
        protected override void SetFilterIconActive(Field field, eIconCtrl imgFilterCol)
        {
            if (_filterInfo != null)
            {
                eListWidget.ExpressFilterInfo filter = _filterInfo.FirstOrDefault(f => f.Descid == field.Descid);
                if (filter != null)
                {
                    imgFilterCol.AddClass("FilterEnabled");
                    imgFilterCol.Attributes.Add("actif", "1");

                    return;
                }

            }

            imgFilterCol.AddClass("Filter");
            imgFilterCol.Style.Add("visibility", "hidden");
            imgFilterCol.Attributes.Add("actif", "0");

        }
        ///// <summary>
        ///// Sets the sort icon active.
        ///// </summary>
        ///// <param name="field">The field.</param>
        ///// <param name="imgSort">The img sort.</param>
        ///// <param name="order">The order.</param>
        //protected override void SetSortIconActive(Field field, eIconCtrl imgSort, SortOrder order)
        //{


        //    if (_sortInfo != null)
        //    {
        //        if (_sortInfo.DescId == field.Descid)
        //        {
        //            SortOrder currentOrder = _sortInfo.AscOrder ? SortOrder.ASC : SortOrder.DESC;

        //            if (currentOrder != order)
        //                imgSort.Style.Add("visibility", "hidden");

        //            imgSort.Attributes.Add("actif", currentOrder == order ? "1" : "0");

        //            return;
        //        }


        //    }

        //    imgSort.Style.Add("visibility", "hidden");
        //}
    }
}