using Newtonsoft.Json;
using System.Text;
using System.Web;
using System.Web.UI.HtmlControls;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Classe de rendu du widget Kanban
    /// </summary>
    /// <seealso cref="Com.Eudonet.Xrm.eAbstractXrmWidgetUI" />
    public class eKanbanXrmWidgetUI : eAbstractXrmWidgetUI
    {
        /// <summary>
        /// Fait un rendu du widget dans le container
        /// </summary>
        /// <param name="widgetContainer">Container du widget</param>
        public override void Build(HtmlControl widgetContainer)
        {
            string context = HttpUtility.UrlEncode(JsonConvert.SerializeObject(_widgetContext));

            HtmlGenericControl iframe = new HtmlGenericControl("iframe");
            iframe.Attributes.Add("class", "kbWidgetIframe");
            iframe.ID = "widgetIframe_" + _widgetRecord.MainFileid;
            iframe.Attributes.Add("src", string.Concat("eXrmWidgetContentUI.aspx?wt=", (int)EudoQuery.XrmWidgetType.Kanban, "&wid=", _widgetRecord.MainFileid, "&c=", context));

            widgetContainer.Controls.Add(iframe);

            base.Build(widgetContainer);
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
        /// <param name="styleBuilder">builder de styles</param>
        public override void AppendStyle(StringBuilder styleBuilder)
        {
            base.AppendStyle(styleBuilder);
        }
    }
}