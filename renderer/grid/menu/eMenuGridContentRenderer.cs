using Com.Eudonet.Internal;
using EudoQuery;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Objet permettant d'afficher le contenu de la page d'accueil
    /// </summary>
    public class eMenuGridContentRenderer : eAbstractMenuRenderer
    {

        /// <summary>
        /// TODO Besoin d'un objet metier de la table
        /// </summary>
        /// <param name="pref">The preference.</param>
        /// <param name="isVisible">if set to <c>true</c> [is visible].</param>
        /// <param name="file">The file.</param>
        /// <param name="context">The context.</param>
        public eMenuGridContentRenderer(ePref pref, bool isVisible, eFile file, eXrmWidgetContext context) : base(isVisible, file, context)
        {
            Pref = pref;
            _tab = (int)TableType.XRMGRID;
        }

        /// <summary>
        ///  Construit le menu
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            RenderMenu("paramTab1", eResApp.GetRes(Pref, 8149), eResApp.GetRes(Pref, 8150));
            return true;
        }

        /// <summary>
        /// On fait le rendu des items du menu dans le container
        /// </summary>
        /// <param name="paramBlockContent"></param>
        protected override void RenderContentMenu(Panel menuContainer)
        {
            // menuContainer.Controls.Add(RenderMenuItem("Widgets disponibles", false, BuildPredefinedWidgetPart()));
            menuContainer.Controls.Add(RenderMenuItem(2, eResApp.GetRes(Pref, 8155), true, BuildWidgetGridPart()));
        }




        /// <summary>
        /// Au click sur le bouton, on affiche la liste des wigets disponibles
        /// </summary>
        /// <returns></returns>
        private Control BuildPredefinedWidgetPart()
        {
            return BuildBtnField(eResApp.GetRes(Pref, 8152), eResApp.GetRes(Pref, 8153), null, "oAdminGridMenu.showWidgetList();");
        }

        /// <summary>
        /// Affiche les widgets disponibles
        /// </summary>
        /// <returns></returns>
        private Control BuildWidgetGridPart()
        {
            Panel container = new Panel();
            container.CssClass = "widget-menu-container";

            Panel widgetMenu = new Panel();
            // nécessaire pour initialiser les events js
            widgetMenu.ID = "menu-widget-ctn";

            widgetMenu.Controls.Add(BuildWidgetTypeIcon(eXrmWidgetTools.GetRes(Pref, XrmWidgetType.Image), XrmWidgetType.Image, "icon-image", 3, 3));

            widgetMenu.Controls.Add(BuildWidgetTypeIcon(eXrmWidgetTools.GetRes(Pref, XrmWidgetType.Editor), XrmWidgetType.Editor, "icon-edit", 4, 3));

            /** U.S #229 : A la demande générale, les webpages doivent être accessibles sur les signets. G.L */
            //if (_context.GridLocation != eXrmWidgetContext.eGridLocation.Bkm || Pref.User.UserLevel > 99)
            widgetMenu.Controls.Add(BuildWidgetTypeIcon(eXrmWidgetTools.GetRes(Pref, XrmWidgetType.WebPage), XrmWidgetType.WebPage, "icon-site_web", 4, 3));

            widgetMenu.Controls.Add(BuildWidgetTypeIcon(eXrmWidgetTools.GetRes(Pref, XrmWidgetType.Chart), XrmWidgetType.Chart, "icon-pie-chart2", 4, 3));

            widgetMenu.Controls.Add(BuildWidgetTypeIcon(eXrmWidgetTools.GetRes(Pref, XrmWidgetType.List), XrmWidgetType.List, "icon-list2", 4, 3));

            widgetMenu.Controls.Add(BuildWidgetTypeIcon(eXrmWidgetTools.GetRes(Pref, XrmWidgetType.Specif), XrmWidgetType.Specif, "icon-buzz", 6, 3));

            widgetMenu.Controls.Add(BuildWidgetTypeIcon(eXrmWidgetTools.GetRes(Pref, XrmWidgetType.Indicator), XrmWidgetType.Indicator, "icon-percent", 2, 2));

            if (_context.GridLocation != eXrmWidgetContext.eGridLocation.Bkm)
                widgetMenu.Controls.Add(BuildWidgetTypeIcon(eXrmWidgetTools.GetRes(Pref, XrmWidgetType.ExpressMessage), XrmWidgetType.ExpressMessage, "icon-flag-o", 3, 1));

            if (eFeaturesManager.IsFeatureAvailable(Pref, eConst.XrmFeature.Widget_Tile))
            {
                widgetMenu.Controls.Add(BuildWidgetTypeIcon(eXrmWidgetTools.GetRes(Pref, XrmWidgetType.Tuile), XrmWidgetType.Tuile, "icon-arrows-alt", 1, 1));
            }

            if (eFeaturesManager.IsFeatureAvailable(Pref, eConst.XrmFeature.Widget_Kanban))
            {
                widgetMenu.Controls.Add(BuildWidgetTypeIcon(eXrmWidgetTools.GetRes(Pref, XrmWidgetType.Kanban), XrmWidgetType.Kanban, "icon-kanban2", 10, 4));
            }

            if (_context.GridLocation != eXrmWidgetContext.eGridLocation.Bkm)
                widgetMenu.Controls.Add(BuildWidgetTypeIcon(eXrmWidgetTools.GetRes(Pref, XrmWidgetType.RSS), XrmWidgetType.RSS, "icon-rss3", 4, 3));

            if (eExtension.IsReady(Pref, ExtensionCode.CARTOGRAPHY))
            {
                if (_context.GridLocation != eXrmWidgetContext.eGridLocation.Bkm && Pref.User.UserLevel >= (int)UserLevel.LEV_USR_SUPERADMIN)
                    widgetMenu.Controls.Add(BuildWidgetTypeIcon(eXrmWidgetTools.GetRes(Pref, XrmWidgetType.Carto_Selection), XrmWidgetType.Carto_Selection, "icon-map-marker", 12, 7));
            }

            container.Controls.Add(widgetMenu);
            return container;
        }

        /// <summary>
        /// Construit un Panel pour un type de widget
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="type">The type.</param>
        /// <param name="icon">The icon.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns></returns>
        private Panel BuildWidgetTypeIcon(string title, XrmWidgetType type, string icon, int width = 1, int height = 1)
        {
            Panel panel = new Panel();
            panel.CssClass = "widgetTypeContainer";
            panel.Attributes.Add("t", ((int)type).ToString());
            panel.Attributes.Add("m", "1");// m : move 
            panel.Attributes.Add("c", "1");// c : clone
            panel.Attributes.Add("n", "1");// n : new
            panel.Attributes.Add("w", width.ToString());
            panel.Attributes.Add("h", height.ToString());

            HtmlGenericControl div = new HtmlGenericControl("div");
            panel.Controls.Add(div);
            div.Attributes.Add("class", icon);

            HtmlGenericControl span = new HtmlGenericControl("span");
            panel.Controls.Add(span);
            span.InnerHtml = title;

            return panel;
        }
    }
}