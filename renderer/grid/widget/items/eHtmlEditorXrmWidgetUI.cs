using Newtonsoft.Json;
using System.Text;
using System.Web;
using System.Web.UI.HtmlControls;

namespace Com.Eudonet.Xrm
{
    public class eHtmlEditorXrmWidgetUI : eAbstractXrmWidgetUI
    {
        public eHtmlEditorXrmWidgetUI()
        {

        }

        /// <summary>
        /// Fait un rendu du widget dans le container
        /// </summary>
        /// <param name="widgetContainer">Container du widget</param>
        public override void Build(HtmlControl widgetContainer)
        {
            base.Build(widgetContainer);

            string context = HttpUtility.UrlEncode(JsonConvert.SerializeObject(_widgetContext));

            HtmlGenericControl iframe = new HtmlGenericControl("iframe");
            iframe.Attributes.Add("src", string.Concat("eXrmWidgetContentUI.aspx?wt=", (int)EudoQuery.XrmWidgetType.Editor, "&wid=", _widgetRecord.MainFileid, "&c=", context));

            widgetContainer.Controls.Add(iframe);
        }

        /// <summary>
        /// Ajoute des scripts js
        /// </summary>
        /// <param name="scriptBuilder">builder de script</param>
        public override void AppendScript(StringBuilder scriptBuilder)
        {
            base.AppendScript(scriptBuilder);
        }

        /// <summary>
        /// Ajoute des style css
        /// </summary>
        /// <param name="scriptBuilder">builder de styles</param>
        public override void AppendStyle(StringBuilder styleBuilder)
        {
            base.AppendStyle(styleBuilder);
        }
    }
}