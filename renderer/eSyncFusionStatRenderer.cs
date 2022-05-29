using Com.Eudonet.Internal;
using System;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.renderer
{
    public class eSyncFusionStatRenderer : eReportWizardRenderer
    {
        private Int32 _nDescId;
        private eCommunChart.TypeChart _typeChart;
        private int _nTabFrom;
        private int _nIdFrom;


        public eSyncFusionStatRenderer(ePref pref, Int32 nTab, Int32 nDescId, int nTabFrom = 0, int nIdFrom = 0) : base(pref, nTab)
        {
            _nDescId = nDescId;
            _typeChart = eCommunChart.TypeChart.STATCHART;
            _nTabFrom = (nTabFrom == 0) ? nTab : nTabFrom;
            _nIdFrom = nIdFrom;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public eSyncFusionStatRenderer getStatisticalChartRenderer()
        {
            bool bVisible = true;
            //Recherche d'information sur le chart
            String param = String.Concat("&addcurrentfilter=1&logo=nologo&public=0&saveas=&valuesfile=", _iTab, "&valuesfield=0&title=&valuesoperation=COUNT&seriestype=0&seriesfile=", _iTab, "&seriesfield=", 0, "&etiquettesfile=", _iTab, "&etiquettesfield=", _nDescId, "&etiquettesgroup=day&w=800&typechart=1|2&h=600&displayvalues=1&displayx=0&hidelegend=0&hidetitle=0&displayvaluespercent=0&displaygrid=1&DisplayGridnb=1&sortorder=0&sortenabled=0");

            _report = new eReport(Pref) { IsNew = false };
            _report.LoadParams(param);

            String onChange = String.Concat("UpdateGraph('ValuesOperation',this.value);");

            //Titre Sélectionnez la rubrique de regroupement des séries (X) :
            HtmlGenericControl liLabel = CreateHtmlTag("div", "rbqValsLabel ", String.Concat(eResApp.GetRes(Pref, 8248), " :"), "statFilterLabel");

            HtmlGenericControl liMultiLines = CreateHtmlTag("div", "liAddLine", "", "liMultiLines");

            DescItem folder = new DescItem();
            ChartSpecificParams wizardParam = new ChartSpecificParams();
            wizardParam.CategoryChart = _typeChart;
            //Contenu
            FillFileAndFields(liMultiLines, "ValuesFile", "ValuesField", wizardParam, out folder, out bVisible);

            RenderAgregateFuncionsIntoContainer(liMultiLines, eResApp.GetRes(Pref, 437), onChange);


            if (bVisible)
            {
                this._pgContainer.Controls.Add(liLabel);
                this._pgContainer.Controls.Add(liMultiLines);
            }

            //Div Conteneur

            HtmlGenericControl divContainer = new HtmlGenericControl("div");
            divContainer.Attributes.Add("class", "exportChartMenu  ");

            HtmlGenericControl divChart = new HtmlGenericControl("div");
            HtmlGenericControl divExportChart = new HtmlGenericControl("div");

            //HtmlGenericControl spanTitle = new HtmlGenericControl("span");
            //spanTitle.InnerText = eResApp.GetRes(_ePref, 8230);
            //spanTitle.Attributes.Add("class", "tooltiptext");

            //divExportChart.Controls.Add(spanTitle);

            divExportChart.ID = "exPortChart0";
            divExportChart.Attributes.Add("load", "DivChart_canvas");
            divExportChart.Attributes.Add("class", "icon-ellipsis-v advExportChartMenu  ");
            divExportChart.Attributes.Add("fHome", "0");
            divExportChart.Attributes.Add("onmouseover", "top.dispExportMenu(this,top.modalCharts.getIframe());");

            divChart.ID = "DivChart";
            divChart.Attributes.Add("class", "divChart");

            liLabel.Controls.Add(divExportChart);
            divContainer.Controls.Add(divChart);


            this._pgContainer.Controls.Add(divContainer);


            //Paramètre du chart à générer
            HtmlGenericControl divHidden = new HtmlGenericControl("div");
            divHidden.Style.Add("visibility", "hidden");
            divHidden.Style.Add("display", "none");
            divHidden.Attributes.Add("ednChartParam", "0");
            this._pgContainer.Controls.Add(divHidden);


            HtmlInputHidden inptChartParam = new HtmlInputHidden();
            inptChartParam.Style.Add("visibility", "hidden");
            inptChartParam.Style.Add("display", "none");
            inptChartParam.Attributes.Add("ednchartparam", "0");
            inptChartParam.Attributes.Add("ednreportparam", "0");

            divHidden.Controls.Add(inptChartParam);
            String s = _report.GetParamValue("w");
            inptChartParam.Attributes.Add("w", s.Length > 0 ? s : "800");
            s = _report.GetParamValue("h");
            inptChartParam.Attributes.Add("h", s.Length > 0 ? s : "600");
            inptChartParam.Attributes.Add("fs", "1");
            inptChartParam.Attributes.Add("id", "0");
            inptChartParam.Attributes.Add("tab", _iTab.ToString());
            inptChartParam.Attributes.Add("tabFrom", _nTabFrom.ToString());
            inptChartParam.Attributes.Add("idFrom", _nIdFrom.ToString());
            inptChartParam.Attributes.Add("descid", _nDescId.ToString());
            inptChartParam.Attributes.Add("chart", "pie3D");
            inptChartParam.Attributes.Add("displaygrid", _report.GetParamValue("DisplayGrid"));
            inptChartParam.Attributes.Add("displaygridnb", _report.GetParamValue("DisplayGridnb"));
            inptChartParam.Attributes.Add("filterid", _report.GetParamValue("filterid"));
            inptChartParam.Attributes.Add("addcurrentfilter", _report.GetParamValue("addcurrentfilter"));



            return this;
        }

        protected override bool Init()
        {
            return true;
        }

        protected override bool Build()
        {
            return true;
        }
        protected override bool End()
        {
            return true;
        }
    }
}