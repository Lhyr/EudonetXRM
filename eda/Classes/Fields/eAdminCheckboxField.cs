using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
namespace Com.Eudonet.Xrm.eda
{
    public class eAdminCheckboxField : eAdminField
    {
        Dictionary<string, string> _customCheckboxStyleAttributes;
        Dictionary<string, string> _customPanelStyleAttributes;
        Dictionary<string, string> _customLabelStyleAttributes;
        string _customCheckboxCSSClasses;
        string _customPanelCSSClasses;
        string _customLabelCSSClasses;
        string _onClick;

        public eAdminCheckboxField(
            int descid, string label, eAdminUpdateProperty.CATEGORY propCat, int propCode, String tooltiptext = "", Boolean value = false, String chkID = "",
            Dictionary<string, string> customCheckboxStyleAttributes = null, string customCheckboxCSSClasses = "",
            Dictionary<string, string> customPanelStyleAttributes = null, string customPanelCSSClasses = "",
            Dictionary<string, string> customLabelStyleAttributes = null, string customLabelCSSClasses = "", String onclick = "", bool readOnly = false
        )
            : base(descid, label, propCat, propCode, tooltiptext, (value) ? "1" : "0")
        {
            Format = FieldFormat.TYP_BIT;
            FieldControlID = chkID;
            _customCheckboxStyleAttributes = customCheckboxStyleAttributes;
            _customCheckboxCSSClasses = customCheckboxCSSClasses;
            _customPanelStyleAttributes = customPanelStyleAttributes;
            _customPanelCSSClasses = customPanelCSSClasses;
            _customLabelStyleAttributes = customLabelStyleAttributes;
            _customLabelCSSClasses = customLabelCSSClasses;
            _onClick = onclick;
            this.ReadOnly = readOnly;
        }

        protected override Boolean Build(Panel panel)
        {
            if (base.Build(panel))
            {

                if (String.IsNullOrEmpty(FieldControlID))
                {
                    FieldControlID = String.Concat("chk", this.AttrDsc.Replace("|", ""));
                }
                eCheckBoxCtrl checkbox = new eCheckBoxCtrl(this.Value == "1", this.ReadOnly);
                checkbox.ID = FieldControlID;
                if (String.IsNullOrEmpty(_onClick))
                {
                    checkbox.AddClick("top.nsAdmin.onCheckboxClick(this)");
                }
                else
                {
                    checkbox.AddClick(_onClick);
                }
                checkbox.Attributes.Add("dsc", this.AttrDsc);
                checkbox.AddText(this.Label);
                checkbox.ToolTip = this.TooltipText;
                checkbox.ToolTipChkBox = this.TooltipText;
                if (this.ReadOnly)
                    checkbox.Attributes.Add("noupdate", "1");

                this.FieldLabel = checkbox.Controls[checkbox.Controls.Count - 1];
                this.FieldControl = checkbox;
                this.PanelField.Controls.Add(checkbox);

                return true;
            }

            return false;

        }

        protected override Boolean End()
        {
            if (base.End())
            {
                #region Ajout des styles et CSS additionnels

                #region Sur le contrôle
                if (_customCheckboxStyleAttributes != null && _customCheckboxStyleAttributes.Count > 0)
                {
                    foreach (KeyValuePair<string, string> style in _customCheckboxStyleAttributes)
                        ((HtmlGenericControl)this.FieldControl).Style.Add(style.Key, style.Value);
                }

                if (!String.IsNullOrEmpty(_customCheckboxCSSClasses))
                {
                    ((HtmlGenericControl)(this.FieldControl.Controls[0])).Attributes["class"] = String.Concat(((HtmlGenericControl)(this.FieldControl.Controls[0])).Attributes["class"], " ", _customCheckboxCSSClasses).Trim();
                }
                #endregion

                #region Sur le libellé
                if (_customLabelStyleAttributes != null && _customLabelStyleAttributes.Count > 0)
                {
                    foreach (KeyValuePair<string, string> style in _customLabelStyleAttributes)
                        ((HtmlGenericControl)this.FieldLabel).Style.Add(style.Key, style.Value);
                }

                if (!String.IsNullOrEmpty(_customLabelCSSClasses))
                {
                    ((HtmlGenericControl)this.FieldLabel).Attributes["class"] = String.Concat(((HtmlGenericControl)this.FieldLabel).Attributes["class"], " ", _customLabelCSSClasses).Trim();
                }
                #endregion

                #region Sur le conteneur
                if (_customPanelStyleAttributes != null && _customPanelStyleAttributes.Count > 0)
                {
                    foreach (KeyValuePair<string, string> style in _customPanelStyleAttributes)
                        this.PanelField.Style.Add(style.Key, style.Value);
                }

                if (!String.IsNullOrEmpty(_customPanelCSSClasses))
                {
                    this.PanelField.CssClass = String.Concat(this.PanelField.CssClass, " ", _customPanelCSSClasses).Trim();
                }
                #endregion

                #endregion
            }

            return true;
        }

    }
}