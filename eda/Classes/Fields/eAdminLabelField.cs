using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminLabelField : eAdminField
    {
        Dictionary<string, string> _customCheckboxStyleAttributes;
        Dictionary<string, string> _customPanelStyleAttributes;
        Dictionary<string, string> _customLabelStyleAttributes;
        string _customCheckboxCSSClasses;
        string _customPanelCSSClasses;
        string _customLabelCSSClasses;
        String _onClick;

        public eAdminLabelField(
            string label, String tooltiptext = ""
        )
            : base(0, label, 0, 0, tooltiptext)
        {

        }

        protected override Boolean Build(Panel panel)
        {
            if (base.Build(panel))
            {
                HtmlGenericControl label = new HtmlGenericControl("span");
                label.InnerText = this.Label;

                this.FieldLabel = label;
                this.PanelField.Controls.Add(label);
                
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
                    ((HtmlGenericControl)this.FieldControl).Attributes["class"] = String.Concat(((HtmlGenericControl)this.FieldControl).Attributes["class"], " ", _customCheckboxCSSClasses).Trim();
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