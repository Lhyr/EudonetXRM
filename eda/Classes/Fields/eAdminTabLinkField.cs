using Com.Eudonet.Internal;
using System;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal.eda;
using EudoQuery;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminTabLinkField : eAdminField
    {
        string _icon;
        string _color;

        public eAdminTabLinkField(eAdminTableInfos tabInfos, String tooltiptext = "")
            : base(tabInfos.DescId, tabInfos.TableLabel, 0, 0, tooltiptext)
        {
            _icon = tabInfos.Icon;
            _color = tabInfos.IconColor;
        }

        protected override Boolean Build(Panel panel)
        {
            if (base.Build(panel))
            {
                HtmlGenericControl htmlLabel = new HtmlGenericControl("label");
                htmlLabel.InnerText = this.Label;
                htmlLabel.ID = "tabLinkFieldLabel";
                this.FieldLabel = htmlLabel;



                HtmlGenericControl link = new HtmlGenericControl("span");


                string iconClass = "icon-param-onglet";
                if (!String.IsNullOrEmpty(_icon))
                {
                    eFontIcons.FontIcons font = eFontIcons.GetFontIcon(_icon);
                    iconClass = font.CssName;
                }


                if (!this.DescID.ToString().EndsWith("91"))
                {
                    iconClass = String.Concat(iconClass, " linkAdminTab");                    
                    link.Attributes.Add("onclick", String.Concat("nsAdmin.loadAdminFile(", this.DescID, ")"));
                }
                else
                {
                    iconClass = String.Concat(iconClass, " cursor-noclick");
                }


                link.Attributes.Add("class", iconClass);

                if (!String.IsNullOrEmpty(_color))
                    link.Style.Add("color", _color);

                this.PanelField.Controls.Add(htmlLabel);
                this.PanelField.Controls.Add(link);

                return true;
            }
            return false;
        }
    }
}