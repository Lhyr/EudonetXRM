using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.wcfs.data.import;

namespace Com.Eudonet.Xrm.import
{
    /// <summary>
    /// Class pour faire un rendu de l'étape du résulta d'import
    /// </summary>
    public class eResultStepRenderer : eComunImportRenderer
    {

        public eResultStepRenderer(ePref pref, eImportWizardParam wizardParam, eImportSourceInfosCallReturn result)
            : base(pref, wizardParam, result)
        {

        }

        /// <summary>
        /// Initialisation de l'etape
        /// </summary>
        /// <returns></returns>
        public override IWizardStepRenderer Init() { return this; }


        /// <summary>
        /// Execute l'opération du rendu
        /// </summary>
        /// <returns></returns>
        public override Panel Render()
        {
            Panel ctn = new Panel();

            HtmlGenericControl container = new HtmlGenericControl("div");
            container.Attributes.Add("class", "data-source-step");
            container.Controls.Add(DrawProgression());
            ctn.Controls.Add(container);
            return ctn;
        }


        /// <summary>
        /// Déssine 
        /// </summary>
        /// <returns></returns>
        public HtmlGenericControl DrawProgression()
        {
            string nbFileFormated = eNumber.FormatNumber(this.ResulWWcf.SourceInfos.LineCount, new eNumber.DecimalParam(Pref) { NumberDigitMin = 0 }, new eNumber.SectionParam(Pref));

            string resNbFile = this.ResulWWcf.SourceInfos.LineCount == 1 ? eResApp.GetRes(Pref, 8509) : eResApp.GetRes(Pref, 8483).Replace("<NBFILES>", nbFileFormated);

            string param = "M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831";
            HtmlGenericControl singleChart = new HtmlGenericControl("div");
            singleChart.Attributes.Add("class", "single-chart");

            HtmlGenericControl svg = new HtmlGenericControl("svg");
            svg.Attributes.Add("class", "circular-chart");
            svg.Attributes.Add("viewBox", "0 0 36 36");
            singleChart.Controls.Add(svg);

            HtmlGenericControl path_A = new HtmlGenericControl("path");
            path_A.Attributes.Add("class", "circle-bg");
            path_A.Attributes.Add("d", param);
            svg.Controls.Add(path_A);

            HtmlGenericControl path_B = new HtmlGenericControl("path");
            path_B.Attributes.Add("class", "circle pathSvg");
            path_B.Style.Add("stroke", Pref.ThemeXRM.Color);
            path_B.Attributes.Add("d", param);
            path_B.Attributes.Add("stroke-dasharray", "0,100");
            path_B.ID = "pathCircle";
            svg.Controls.Add(path_B);

            HtmlGenericControl txt = new HtmlGenericControl("text");
            txt.Attributes.Add("class", "percentage");
            txt.Attributes.Add("x", "18");
            txt.Attributes.Add("y", "20.35");
            txt.ID = "percent";
            txt.InnerHtml = "0%";
            svg.Controls.Add(txt);

            HtmlGenericControl importInfos = new HtmlGenericControl("div");
            importInfos.Attributes.Add("class", "importInfos");
            importInfos.ID = "importInfos";
            importInfos.Style.Add(HtmlTextWriterStyle.Display, "none");
            importInfos.InnerHtml = string.Concat(resNbFile, "<br/>", eResApp.GetRes(Pref, 8484), "<br/>", eResApp.GetRes(Pref, 8485));
            singleChart.Controls.Add(importInfos);
            return singleChart;

        }
    }
}
