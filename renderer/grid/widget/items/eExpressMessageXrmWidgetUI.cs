using Newtonsoft.Json;
using System.Web;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    public class eExpressMessageXrmWidgetUI : eAbstractXrmWidgetUI
    {
        ePref _pref;

        public eExpressMessageXrmWidgetUI(ePref pref)
        {
            _pref = pref;
        }


        public override void Build(HtmlControl widgetContainer)
        {
            base.Build(widgetContainer);

            string context = HttpUtility.UrlEncode(JsonConvert.SerializeObject(_widgetContext));

            HtmlGenericControl iframe = new HtmlGenericControl("iframe");
            iframe.Attributes.Add("src", string.Concat("eXrmWidgetContentUI.aspx?wt=", (int)EudoQuery.XrmWidgetType.ExpressMessage, "&c=", context));

            widgetContainer.Controls.Add(iframe);

        }
    }
}