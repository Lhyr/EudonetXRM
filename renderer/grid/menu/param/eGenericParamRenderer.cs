using EudoQuery;
using System.Web.UI;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Objet permettant d'afficher le menu "Paramètres du widget"
    /// </summary>
    public class eGenericParamRenderer : eAbstractMenuWidgetParamRenderer
    {
        XrmWidgetType _widgetType;

        /// <summary>
        /// Initializes a new instance of the <see cref="eGenericParamRenderer" /> class.
        /// </summary>
        /// <param name="pref">The preference.</param>
        /// <param name="isVisible">if set to <c>true</c> [is visible].</param>
        /// <param name="file">The file.</param>
        /// <param name="param">The parameter.</param>
        /// <param name="widgetType">Type of the widget.</param>
        /// <param name="context">The context.</param>
        public eGenericParamRenderer(ePref pref, bool isVisible, eFile file, eXrmWidgetParam param, int widgetType = -1, eXrmWidgetContext context = null) : base(pref, isVisible, file, param, context)
        {
            _widgetType = (XrmWidgetType)widgetType;
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            _pgContainer.ID = "widgetProperties";
            return base.Init();
        }

        /// <summary>
        /// Construction de la partie "Propriétés" suivant le type de widget
        /// </summary>
        /// <returns></returns>
        protected override Control BuildWidgetContentPart()
        {
            eWidgetSpecificParamRenderer rdr;
            switch (_widgetType)
            {
                case XrmWidgetType.Image:
                    rdr = new eImageParamRenderer(this.Pref, _isVisible, _file, _widgetParams); break;
                case XrmWidgetType.WebPage:
                    rdr = new eWebPageParamRenderer(this.Pref, _isVisible, _file, _widgetParams); break;
                case XrmWidgetType.Weather:
                    rdr = new eWeatherParamRenderer(this.Pref, _isVisible, _file, _widgetParams); break;
                case XrmWidgetType.Chart:
                    rdr = new eChartParamRenderer(this.Pref, _isVisible, _file, _widgetParams, _context); break;
                case XrmWidgetType.List:
                    rdr = new eListParamRenderer(this.Pref, _isVisible, _file, _widgetParams, _context); break;
                case XrmWidgetType.Specif:
                    rdr = new eSpecifParamRenderer(this.Pref, _isVisible, _file, _widgetParams, _context); break;
                case XrmWidgetType.Indicator:
                    rdr = new eIndicatorParamRenderer(this.Pref, _isVisible, _file, _widgetParams, _context); break;
                case XrmWidgetType.Tuile:
                    rdr = new eTileParamRenderer(this.Pref, _isVisible, _file, _widgetParams); break;
                case XrmWidgetType.Kanban:
                    rdr = new eKanbanParamRenderer(this.Pref, _isVisible, _file, _widgetParams, _context); break;
                case XrmWidgetType.RSS:
                    rdr = new eRSSParamRenderer(this.Pref, _isVisible, _file, _widgetParams); break;
                case XrmWidgetType.Carto_Selection:
                    rdr = new eCartoSelectionParamRenderer(this.Pref, _isVisible, _file, _widgetParams); break;

                default:
                    rdr = new eWidgetSpecificParamRenderer(this.Pref, _isVisible, _file, _widgetParams, true, _context); break;

            }
            if (!rdr.Generate())
            {
                throw new System.Exception(rdr.ErrorMsg);
            }

            rdr.PgContainer.Attributes.Add("widgettype", ((int)_widgetType).ToString());

            return rdr.PgContainer;
        }

    }
}