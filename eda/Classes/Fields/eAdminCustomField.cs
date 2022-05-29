using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminPictoField : eAdminField
    {
        String _color;

        public eAdminPictoField(int descid, String label, String tooltiptext = "", String value = "", String color = "")
            : base(descid, label, eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.ICONFONT.GetHashCode(), tooltiptext, value)
        {

    
            _color = color;
        }

        protected override Boolean Build(Panel panel)
        {
            if (base.Build(panel))
            {
                HtmlGenericControl htmlLabel = new HtmlGenericControl("label");
                htmlLabel.InnerText = this.Label;
                this.FieldLabel = htmlLabel;

                HtmlGenericControl btn = new HtmlGenericControl("div");
                btn.ID = "btnSelectPicto";
                btn.Attributes.Add("onclick", String.Concat("nsAdmin.openPictoPopup(", DescID, ");"));

                HtmlGenericControl icon = new HtmlGenericControl();
                icon.ID = "selectedPicto";
                eFontIcons.FontIcons font = eFontIcons.GetFontIcon(this.Value);
                icon.Attributes.Add("class", font.CssName);
                if (!String.IsNullOrEmpty(_color))
                    icon.Style.Add("color", _color);

                btn.Controls.Add(icon);

                this.FieldControl = icon;

                this.PanelField.Controls.Add(htmlLabel);
                this.PanelField.Controls.Add(btn);


                return true;
            }
            return false;
        }
    }
}