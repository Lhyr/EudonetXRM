using System;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminDashboardRGPDRenderer : eAdminModuleRenderer
    {
        Panel _pnlContents;
        /// <summary>Id de la grille</summary>
        private int _gridId = 0;
        private int _fileId = 1;
        private eList _listWidget;
        private eXrmWidgetPrefCollection _widgetPrefCollection;

        private eAdminDashboardRGPDRenderer(ePref pref, int w, int h) : base(pref)
        {
            _width = w;
            _height = h;
            _tab = 0;
        }

        public static eAdminDashboardRGPDRenderer CreateAdminDashboardRGPDRenderer(ePref pref, int w, int h)
        {
            return new eAdminDashboardRGPDRenderer(pref, w, h);
        }

        protected override bool Init()
        {
            if (base.Init())
            {
                if (!eFeaturesManager.IsFeatureAvailable(Pref, eConst.XrmFeature.AdminRGPD))
                    return false;

                eudoDAL eDal = eLibTools.GetEudoDAL(Pref);
                try
                {
                    eDal.OpenDatabase();
                    _gridId = eSqlXrmGrid.GetXrmGridId(eDal, 0, 1);
                    // La grille doit obligatoirement exister (montée de version)
                    if (_gridId == 0)
                        return false;
                }
                catch (Exception)
                {
                    return false;
                }
                finally
                {
                    eDal.CloseDatabase();
                }


                _listWidget = eListFactory.GetWidgetList(Pref, _gridId);
                _widgetPrefCollection = new eXrmWidgetPrefCollection(Pref, _gridId);

                return true;
            }


            return false;
        }


        /// <summary>
        /// Lance la construction de la grille
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            //_pgContainer.Controls.Add(BuildContainer());

            AddCallBackScript(String.Concat("nsAdmin.loadDashboardGrid(", "0", ", ", _fileId, ", ", _gridId, ");"));

            return true;
        }

        ///// <summary>
        ///// Construit le grille et le contenu
        ///// </summary>
        ///// <returns></returns>
        //private Control BuildContainer()
        //{
        //    Panel panel = new Panel();
        //    panel.CssClass = "gridContainer";

        //    HtmlGenericControl container = new HtmlGenericControl("div");
        //    container.ID = "widget-grid-container";
        //    container.Attributes.Add("class", "widget-grid-container");// TODO
        //    container.Style.Add("height", _height + "px");// on retire la taille de la navbar + titre + bas de page
        //    container.Style.Add("width", _width + "px");// on retire la taille de la navbar + titre + bas de page

        //    container.Attributes.Add("gid", _gridId.ToString());// TODO
        //    container.Attributes.Add("config", "0");

        //    BuildGrid(container);
        //    BuildWidgets(container);

        //    panel.Controls.Add(container);

        //    return panel;
        //}

        //private void BuildGrid(Control container)
        //{
        //    System.Web.UI.WebControls.Table table = new System.Web.UI.WebControls.Table();
        //    table.ID = "widget-grid";
        //    table.CssClass = "widget-grid grid-cell-" + _gridId;

        //    int tableWidth = _width - 15;

        //    // Hauteur/largeur de la cellule déduite de la largeur
        //    int cellSize = (tableWidth - 8 * eConst.XRM_HOMEPAGE_GRID_WIDTH) / eConst.XRM_HOMEPAGE_GRID_WIDTH;

        //    // Injection de la largeur et la hauteur pour toutes les td
        //    _dynamicStyle.Append(" .grid-cell-").Append(_gridId).Append(" td { width:").Append(cellSize).Append("px; height:").Append(cellSize).Append("px; }").AppendLine();

        //    table.Attributes.Add("w", tableWidth.ToString());
        //    table.Style.Add(HtmlTextWriterStyle.Width, tableWidth.ToString() + "px");
        //    table.Attributes.Add("config", "0"); // ConfigMode

        //    // Si le wiget est plus grand on prend plus
        //    int nb_row = _height / cellSize;

        //    TableRow row;
        //    for (int r = 0; r < nb_row; r++)
        //    {
        //        row = new TableRow();
        //        for (int c = 0; c < eConst.XRM_HOMEPAGE_GRID_WIDTH; c++)
        //            row.Controls.Add(new TableCell());

        //        table.Controls.Add(row);
        //    }

        //    container.Controls.Add(table);
        //}

        ///// <summary>
        ///// Rendu des widgets
        ///// </summary>
        ///// <returns></returns>
        //private void BuildWidgets(HtmlControl container)
        //{
        //    // Pas de widgets pour la grille actuelle
        //    if (_listWidget.ListRecords == null)
        //        return;

        //    IXrmWidgetUI IWidget;
        //    eXrmWidgetPref widgetPref;
        //    eXrmWidgetParam widgetParam;

        //    foreach (eRecord record in _listWidget.ListRecords)
        //    {
        //        // pref des widgets
        //        widgetPref = _widgetPrefCollection[record];

        //        widgetParam = new eXrmWidgetParam(Pref, record.MainFileid);

        //        // le widget n'est visibe sauf si explicite ou admin
        //        if (!Pref.AdminMode && !widgetPref.Visible)
        //            continue;



        //        // rendu du contenu de la div 
        //        IWidget = new eXrmWidgetContent(eXrmWidgetFactory.GetWidgetUI(Pref, record, true), Pref);

        //        // ajout de la div pour le déplacement
        //        // Ajout de la div pour redimensionner
        //        // ajout de container
        //        IWidget = new eXrmWidgetWrapper(new eXrmWidgetWithResize(new eXrmWidgetWithToolbar(Pref, IWidget, Pref.AdminMode), Pref), true);

        //        // Initilisation
        //        IWidget.Init(record, widgetPref, widgetParam);

        //        // Construction
        //        IWidget.Build(container);

        //        // ajout des script js
        //        IWidget.AppendScript(_dynamicScript);

        //        //ajout des style css
        //        IWidget.AppendStyle(_dynamicStyle);
        //    }
        //}


        ///// <summary>
        ///// On ajoute les class css générées dynamiquement
        ///// </summary>
        ///// <returns></returns>
        //protected override bool End()
        //{
        //    HtmlGenericControl style = new HtmlGenericControl("style");
        //    style.ID = "grid-cell-" + _gridId;
        //    style.Attributes.Add("type", "text/css");
        //    style.InnerHtml = _dynamicStyle.ToString();
        //    _pgContainer.Controls.Add(style);


        //    HtmlGenericControl script = new HtmlGenericControl("script");
        //    script.Attributes.Add("type", "text/javascript");
        //    script.InnerHtml = _dynamicScript.ToString();
        //    _pgContainer.Controls.Add(script);

        //    return true;
        //}



    }
}