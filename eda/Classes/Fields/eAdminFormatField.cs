using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using System;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminFormatField : eAdminField
    {
        Boolean _bold;
        Boolean _italic;
        Boolean _underline;
        ePref _pref;

        public eAdminFormatField(ePref pref, int descid, String label, Boolean bBold, Boolean bItalic, Boolean bUnderline, String tooltiptext = "")
            : base(descid, label, tooltiptext)
        {
            _bold = bBold;
            _italic = bItalic;
            _underline = bUnderline;
            _pref = pref;
        }

        protected override Boolean Build(Panel panel)
        {
            if (base.Build(panel))
            {
                HtmlGenericControl ul = new HtmlGenericControl("ul");
                ul.Attributes.Add("class", "labelFormat");
                ul.Attributes.Add("did", this.DescID.ToString());

                HtmlGenericControl li = new HtmlGenericControl("li");
                li.ID = "buttonBold";
                ul.Controls.Add(li);
                li.Attributes.Add("class", "buttonBold " + (_bold ? "active" : ""));
                li.Attributes.Add("did", this.DescID.ToString());
                li.Attributes.Add("dsc", String.Concat(eAdminUpdateProperty.CATEGORY.DESC.GetHashCode(), "|", eLibConst.DESC.BOLD.GetHashCode()));
                li.Attributes.Add("dbvalue", _bold ? "1" : "0");
                li.InnerText = eResApp.GetRes(_pref, 7606);
                
                li = new HtmlGenericControl("li");
                li.ID = "buttonItalic";
                ul.Controls.Add(li);
                li.Attributes.Add("class", "buttonItalic " + (_italic ? "active" : ""));
                li.Attributes.Add("did", this.DescID.ToString());
                li.Attributes.Add("dsc", String.Concat(eAdminUpdateProperty.CATEGORY.DESC.GetHashCode(), "|", eLibConst.DESC.ITALIC.GetHashCode()));
                li.Attributes.Add("dbvalue", _italic ? "1" : "0");
                li.InnerText = eResApp.GetRes(_pref, 7607); 

                li = new HtmlGenericControl("li");
                li.ID = "buttonUnderline";
                ul.Controls.Add(li);
                li.Attributes.Add("class", "buttonUnderline " + (_underline ? "active" : ""));
                li.Attributes.Add("did", this.DescID.ToString());
                li.Attributes.Add("dsc", String.Concat(eAdminUpdateProperty.CATEGORY.DESC.GetHashCode(), "|", eLibConst.DESC.UNDERLINE.GetHashCode()));
                li.Attributes.Add("dbvalue", _underline ? "1" : "0");
                li.InnerText = eResApp.GetRes(_pref, 7608);

                //li = new HtmlGenericControl("li");
                //li.ID = "buttonColorPicker";
                //ul.Controls.Add(li);
                //HtmlGenericControl buttonColor = new HtmlGenericControl("input");
                //buttonColor.Attributes.Add("type", "color");
                //li.Controls.Add(buttonColor);

                HtmlGenericControl label = new HtmlGenericControl("p");
                label.Attributes.Add("class", "info");
                label.InnerText = this.Label;

                this.PanelField.Controls.Add(label);
                this.PanelField.Controls.Add(ul);

                panel.Controls.Add(this.PanelField);

                return true;
            }

            return false;

        }
    }
}