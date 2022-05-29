using System;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminInfoField : eAdminField
    {
        string _infoText = string.Empty;

        public eAdminInfoField(int tab, string label, string text) : base(tab, label)
        {
            _infoText = text;
        }

        protected override Boolean Build(Panel panel)
        {
            this.PanelField = new Panel();
            this.PanelField.CssClass = "field";

            HtmlGenericControl htmlInfo = new HtmlGenericControl();
            htmlInfo.InnerText = this.Label;
            htmlInfo.Attributes.Add("class", "info");
            this.FieldLabel = htmlInfo;

            this.PanelField.Controls.Add(htmlInfo);

            HtmlGenericControl info = new HtmlGenericControl();
            info.Attributes.Add("class", "infoText");
            info.InnerText = _infoText;
            PanelField.Controls.Add(info);

            panel.Controls.Add(this.PanelField);

            return true;
        }

        protected override bool End()
        {
            return true;
        }
    }
}