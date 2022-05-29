using System;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal.eda;
namespace Com.Eudonet.Xrm.eda
{
    public class eAdminColorField : eAdminField
    {
        String _colorPickerID;
        String _txtColorID;

        public eAdminColorField(int descid, String label, eAdminUpdateProperty.CATEGORY prop, int propCode, String colorPickerID = "", String txtColorID = "", String tooltiptext = "", String value = "")
            : base(descid, label, prop, propCode, tooltiptext, value)
        {
            _colorPickerID = (colorPickerID != "") ? colorPickerID : "colorPicker";
            _txtColorID = (txtColorID != "") ? txtColorID : "txtColor";
            this.IsOptional = true;
            this.Format = EudoQuery.FieldFormat.TYP_CHAR;
        }

        protected override Boolean Build(Panel panel)
        {
            if (base.Build(panel))
            {
                Panel colorWrapper = new Panel();
                colorWrapper.CssClass = "colorWrapper";

                #region Textbox
                TextBox textbox = new TextBox();
                textbox.ID = _txtColorID;
                textbox.CssClass = "txtColor";
                textbox.Attributes.Add("dsc", this.AttrDsc);
                textbox.Text = this.Value;

                this.FieldControl = textbox;
                #endregion

                #region Colorpicker
                Panel colorPickerWrapper = new Panel();
                colorPickerWrapper.CssClass = "colorPickerWrapper";
                colorPickerWrapper.Attributes.Add("onclick", "top.nsAdmin.openColorPicker(document.getElementById('" + _colorPickerID + "'), document.getElementById('" + _txtColorID + "'));");

                HtmlGenericControl colorPicker = new HtmlGenericControl();
                colorPicker.ID = _colorPickerID;
                colorPicker.Attributes.Add("class", "colorPicker");
                if (!String.IsNullOrEmpty(this.Value))
                {
                    colorPicker.Style.Add("background-color", this.Value);
                }

                colorPickerWrapper.Controls.Add(colorPicker);
                #endregion


                colorWrapper.Controls.Add(textbox);
                colorWrapper.Controls.Add(colorPickerWrapper);

                HtmlGenericControl label = new HtmlGenericControl("p");
                label.Attributes.Add("class", "info");
                label.InnerText = this.Label;
                this.FieldLabel = label;

                this.PanelField.Controls.Add(label);
                this.PanelField.Controls.Add(colorWrapper);

                panel.Controls.Add(this.PanelField);

                return true;
            }

            return false;

        }
    }
}