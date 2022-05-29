using Com.Eudonet.Internal;
using System;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Rendu du widget de type graphique
    /// </summary>
    /// <seealso cref="Com.Eudonet.Xrm.eAbstractXrmWidgetUI" />
    public class eChartXrmWidgetUI : eAbstractXrmWidgetUI
    {
        private ePref _pref;
        private int _chartId;

        /// <summary>
        /// Initializes a new instance of the <see cref="eChartXrmWidgetUI"/> class.
        /// </summary>
        /// <param name="pref">The preference.</param>
        public eChartXrmWidgetUI(ePref pref)
        {
            this._pref = pref;
        }

        /// <summary>
        /// Fait un rendu du widget dans le container
        /// </summary>
        /// <param name="widgetContainer">Container du widget</param>
        public override void Build(HtmlControl widgetContainer)
        {
            int.TryParse(_widgetRecord.GetFieldByAlias(eXrmWidgetTools.GetAlias(EudoQuery.XrmWidgetField.ContentSource)).Value, out _chartId);
            if (_chartId > 0)
            {
                eChartRenderer chart = eChartRenderer.GetChartRenderer(_pref, string.Concat(_chartId, "_", _widgetRecord.MainFileid), false, 1, 1, bFromHomepage: true);
                chart.PgContainer.ID = String.Concat("mainChart_", string.Concat(_chartId, "_", _widgetRecord.MainFileid));
                chart.PgContainer.CssClass = "fileChartMain";

                //BSE => US 944 Afficher les Filtres rapides sur les widgets Graphiques // Attente validation PO
                //if (eFeaturesManager.IsFeatureAvailable(_pref, eConst.XrmFeature.GraphExpressFilter))
                //{
                //    eChartRenderer er = (eChartRenderer)eRendererFactory.CreateFiltreEXpressChartRenderer(_pref, _chartId);

                //    if (er.ErrorMsg.Length == 0)
                //    {
                //        while (er.PgContainer.Controls.Count > 0)
                //        {
                //            widgetContainer.Controls.Add(er.PgContainer.Controls[0]);
                //        }
                //    }
                //    else
                //        throw new Exception("Impossible de charger la liste des filtres express", er.InnerException);
                //}

                chart.PgContainer.Controls.Add(renderer.eSyncFusionChartRenderer.GetHtmlChart(_pref, string.Concat(_chartId, "_", _widgetRecord.MainFileid), false));
                widgetContainer.Controls.Add(chart.PgContainer);
            }
            else
            {

                HtmlGenericControl div = new HtmlGenericControl("div");
                div.Attributes.Add("class", "xrm-widget-waiter");
                div.Controls.Add(new LiteralControl(eResApp.GetRes(_pref, 8146)));
                widgetContainer.Controls.Add(div);
            }

            base.Build(widgetContainer);
        }
    }
}