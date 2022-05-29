using Com.Eudonet.Internal;
using Newtonsoft.Json;
using System.Text;
using System.Web;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    public class eListXrmWidgetUI : eAbstractXrmWidgetUI
    {
        private ePref _pref;
        private int _tab;

        /// <summary>
        /// Fait un rendu du widget dans le container
        /// </summary>
        /// <param name="widgetContainer">Container du widget</param>
        public override void Build(HtmlControl widgetContainer)
        {
            _tab = eLibTools.GetNum(_widgetRecord.GetFieldByAlias(eXrmWidgetTools.GetAlias(EudoQuery.XrmWidgetField.ContentSource)).Value);

            string context = HttpUtility.UrlEncode(JsonConvert.SerializeObject(_widgetContext));

            HtmlGenericControl iframe = new HtmlGenericControl("iframe");
            iframe.Attributes.Add("class", "listWidgetIframe");
            iframe.ID = "widgetIframe_" + _widgetRecord.MainFileid;
            iframe.Attributes.Add("src", string.Concat("eXrmWidgetContentUI.aspx?wt=", (int)EudoQuery.XrmWidgetType.List, "&wid=", _widgetRecord.MainFileid, "&tab=", _tab, "&c=", context));

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
        /// <param name="scriptBuilder">builder de styles</param>
        public override void AppendStyle(StringBuilder styleBuilder)
        {
            base.AppendStyle(styleBuilder);
        }
    }
}