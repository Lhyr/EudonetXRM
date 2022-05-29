using Com.Eudonet.Internal;
using EudoQuery;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Objet permettant d'afficher une grille
    /// </summary>
    public class eXrmHomePageGridRenderer : eRenderer
    {
        /// <summary>Id de la grille</summary>
        private int _gridId = 0;
        private eFile _file;
        private eList _listWidget;
        private eXrmWidgetPrefCollection _WidgetPrefCollection;
        private StringBuilder _dynamicScript;
        private StringBuilder _dynamicStyle;
        /// <summary>
        /// Dans le cas où on est sur un signet Grille 
        /// </summary>
        private eXrmWidgetContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="eXrmHomePageGridRenderer" /> class.
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="gridId">Id de la grille</param>
        /// <param name="nWidth">Width</param>
        /// <param name="nHeight">Height</param>
        /// <param name="context">The context.</param>
        public eXrmHomePageGridRenderer(ePref pref, int gridId, int nWidth, int nHeight, eXrmWidgetContext context)
        {
            Pref = pref;
            _width = nWidth;
            _height = nHeight - 25;
            _gridId = gridId;
            _tab = (int)TableType.XRMGRID;
            _context = context;
            //if (context != null)
            //{
            //    _tab = context.ParentTab;
            //}


            _dynamicScript = new StringBuilder();
            _dynamicStyle = new StringBuilder();
        }

        /// <summary>
        /// Initialisation du builder -> instanciation des objets metiers
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            _file = eFileMain.CreateMainFile(Pref, (int)TableType.XRMGRID, _gridId, -2);
            _listWidget = eListFactory.GetWidgetList(Pref, _gridId);
            _WidgetPrefCollection = new eXrmWidgetPrefCollection(Pref, _gridId);

            //TableLite tab = new TableLite(_tab);

            //_ednTab = tab;

            return true;
        }

        /// <summary>
        /// Lance la construction de la grille
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            _pgContainer.Controls.Add(BuildContainer());

            return true;
        }


        /// <summary>
        /// Construit le grille et le contenu
        /// </summary>
        /// <returns></returns>
        private Control BuildContainer()
        {
            Panel panel = new Panel();
            panel.CssClass = "gridContainer";

            HtmlGenericControl container = new HtmlGenericControl("div");
            container.ID = "widget-grid-container";
            container.Attributes.Add("class", "widget-grid-container");// TODO
            container.Style.Add("height", _height + "px");// on retire la taille de la navbar + titre + bas de page
            container.Style.Add("width", _width + "px");// on retire la taille de la navbar + titre + bas de page



            container.Attributes.Add("gid", _gridId.ToString());// TODO
            container.Attributes.Add("config", Pref.AdminMode && _tab > 0 ? "1" : "0");
            
            // Affichage de la grille et des widgets
            BuildGrid(container);
            BuildWidgets(container);

            panel.Controls.Add(container);

            return panel;
        }

        /// <summary>
        /// Crée une grille vide qui doit être rechargée
        /// </summary>
        /// <param name="container">The container.</param>
        private void BuildEmptyBkmGrid(Control container)
        {
            Panel panel = new Panel();
            panel.ID = "emptyGridPanel";

            HtmlGenericControl text = new HtmlGenericControl();
            text.ID = "info";
            text.InnerHtml = $"{eResApp.GetRes(_ePref, 8573)} <span class='icon-forward' id='infoIcon'></span>";
            panel.Controls.Add(text);

            container.Controls.Add(panel);
        }


        /// <summary>
        /// rendu de la grille
        /// La première ligne on rensigne on renseigne la largeur des cellle
        /// La première colonne on rensigne la hauteur des cellule
        /// </summary>

        private void BuildGrid(Control container)
        {
            System.Web.UI.WebControls.Table table = new System.Web.UI.WebControls.Table();
            table.ID = "widget-grid";
            table.CssClass = "widget-grid grid-cell-" + _gridId;

            int tableWidth = _width - 15;

            // Hauteur/largeur de la cellule déduite de la largeur
            int cellSize = (tableWidth - 8 * eConst.XRM_HOMEPAGE_GRID_WIDTH) / eConst.XRM_HOMEPAGE_GRID_WIDTH;

            // Injection de la largeur et la hauteur pour toutes les td
            _dynamicStyle.Append(" .grid-cell-").Append(_gridId).Append(" td { width:").Append(cellSize).Append("px; height:").Append(cellSize).Append("px; }").AppendLine();

            table.Attributes.Add("w", tableWidth.ToString());
            table.Style.Add(HtmlTextWriterStyle.Width, tableWidth.ToString() + "px");
            table.Attributes.Add("config", Pref.AdminMode && _tab > 0 ? "1" : "0"); // ConfigMode

            // Si le wiget est plus grand on prend plus
            int nb_row = _height / cellSize;

            //il faut qu'il y ait au moins 1 rang dans la grid
            if (nb_row == 0)
                nb_row = 1;

            TableRow row; TableCell cell;
            for (int r = 0; r < nb_row; r++)
            {
                row = new TableRow();
                for (int c = 0; c < eConst.XRM_HOMEPAGE_GRID_WIDTH; c++)
                {
                    cell = new TableCell();
                    cell.Attributes.Add("wid", "0");
                    row.Controls.Add(cell);
                }
                table.Controls.Add(row);
            }

            container.Controls.Add(table);
        }

        /// <summary>
        /// Rendu des widgets
        /// </summary>
        /// <returns></returns>
        private void BuildWidgets(HtmlControl container)
        {
            // Pas de widgets pour la grille actuelle
            if (_listWidget.ListRecords == null)
                return;

            IXrmWidgetUI IWidget;
            eXrmWidgetPref widgetPref;
            eXrmWidgetParam widgetParam;


            foreach (eRecord record in _listWidget.ListRecords)
            {
                // pref des widgets
                widgetPref = _WidgetPrefCollection[record];

                widgetParam = new eXrmWidgetParam(Pref, record.MainFileid);

                // le widget n'est visibe sauf si explicite ou admin
                if (!Pref.AdminMode && !widgetPref.Visible)
                    continue;



                // rendu du contenu de la div 
                IWidget = new eXrmWidgetContent(eXrmWidgetFactory.GetWidgetUI(Pref, record, true), Pref);

                // ajout de la div pour le déplacement
                // Ajout de la div pour redimensionner
                // ajout de container
                IWidget = new eXrmWidgetWrapper(new eXrmWidgetWithResize(new eXrmWidgetWithToolbar(Pref, IWidget, (Pref.AdminMode && _tab > 0 && widgetParam.GetParamValue("noAdmin") != "1")), Pref), Pref, true);

                // Initilisation
                IWidget.Init(record, widgetPref, widgetParam, _context);

                // Construction
                IWidget.Build(container);

                // ajout des script js
                IWidget.AppendScript(_dynamicScript);

                //ajout des style css
                IWidget.AppendStyle(_dynamicStyle);
            }
        }



        /// <summary>
        /// On ajoute les class css générées dynamiquement
        /// </summary>
        /// <returns></returns>
        protected override bool End()
        {
            HtmlGenericControl style = new HtmlGenericControl("style");
            style.ID = "grid-cell-" + _gridId;
            style.Attributes.Add("type", "text/css");
            style.InnerHtml = _dynamicStyle.ToString();
            _pgContainer.Controls.Add(style);


            HtmlGenericControl script = new HtmlGenericControl("script");
            script.Attributes.Add("type", "text/javascript");
            script.InnerHtml = _dynamicScript.ToString();
            _pgContainer.Controls.Add(script);

            return true;
        }
    }
}