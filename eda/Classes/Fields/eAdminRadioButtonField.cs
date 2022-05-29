using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;


namespace Com.Eudonet.Xrm.eda
{
    public class eAdminRadioButtonField : eAdminField
    {
        Dictionary<String, String> _listItems;
        String _rbGroupName;
        Dictionary<string, string> _customRadioButtonStyleAttributes;
        Dictionary<string, string> _customPanelStyleAttributes;
        Dictionary<string, string> _customLabelStyleAttributes;
        Dictionary<string, string> _customRadioButtonAttributes;
        string _customRadioButtonCSSClasses;
        string _customPanelCSSClasses;
        string _customLabelCSSClasses;
        String _onClick;
        String _onChange;

        public eAdminRadioButtonField(
            int descid, String label, eAdminUpdateProperty.CATEGORY propCat, Int32 propCode, String groupName, Dictionary<String, String> items, String tooltiptext = "", String value = "", FieldFormat valueFormat = FieldFormat.TYP_NUMERIC,
            Dictionary<string, string> customRadioButtonStyleAttributes = null, string customRadioButtonCSSClasses = "",
            Dictionary<string, string> customPanelStyleAttributes = null, string customPanelCSSClasses = "",
            Dictionary<string, string> customLabelStyleAttributes = null, string customLabelCSSClasses = "",
            String onclick = "",
            Dictionary<string, string> customRadioButtonAttributes = null,
            String onChange = "",
            bool readOnly = false
        )
            : base(descid, label, propCat, propCode, tooltiptext, value)
        {
            _listItems = items;
            _rbGroupName = groupName;
            Format = valueFormat;
            _customRadioButtonStyleAttributes = customRadioButtonStyleAttributes;
            _customRadioButtonCSSClasses = customRadioButtonCSSClasses;
            _customPanelStyleAttributes = customPanelStyleAttributes;
            _customPanelCSSClasses = customPanelCSSClasses;
            _customLabelStyleAttributes = customLabelStyleAttributes;
            _customLabelCSSClasses = customLabelCSSClasses;
            _customRadioButtonAttributes = customRadioButtonAttributes;
            _onClick = onclick;
            _onChange = onChange;
            this.ReadOnly = readOnly;
        }


        protected override Boolean Build(Panel panel)
        {
            if (base.Build(panel))
            {
                HtmlGenericControl htmlLabel = new HtmlGenericControl("p");
                htmlLabel.Attributes.Add("class", "info");
                htmlLabel.InnerHtml = this.Label;
                if (String.IsNullOrEmpty(this.Label))
                {
                    htmlLabel.Visible = false;
                }
                this.FieldLabel = htmlLabel;

                HtmlGenericControl listRb = new HtmlGenericControl("ul");
                listRb.Attributes.Add("class", "listRB");
                this.FieldControl = listRb; // on définit la liste entière comme contrôle renvoyé par la propriété. L'accès aux radio buttons enfants pourra ainsi se faire via .Controls

                HtmlGenericControl rbItem, label;

                foreach (KeyValuePair<string, string> entry in _listItems)
                {
                    rbItem = new HtmlGenericControl("li");
                    HtmlGenericControl rb = new HtmlGenericControl("input");

                    if (this.Value == entry.Key)
                        rb.Attributes.Add("checked", "1");

                    rb.ID = String.Concat(_rbGroupName, "_", entry.Key);
                    rb.Attributes.Add("type", "radio");
                    rb.Attributes.Add("name", _rbGroupName);
                    rb.Attributes.Add("value", entry.Key);
                    if (!String.IsNullOrEmpty(this._onClick))
                    {
                        rb.Attributes.Add("onclick", _onClick);
                    }
                    if (!String.IsNullOrEmpty(this._onChange))
                    {
                        rb.Attributes.Add("onchange", _onChange);
                    }



                    if (this.ReadOnly)
                    {
                        rb.Attributes.Add("disabled", "disabled");
                    }
                    rb.Attributes.Add("dsc", this.AttrDsc);
                    rb.Attributes.Add("did", this.DescID.ToString());

                    #region Ajout des attributs additionnels

                    if (_customRadioButtonAttributes != null && _customRadioButtonAttributes.Count > 0)
                    {
                        foreach (KeyValuePair<string, string> attribut in _customRadioButtonAttributes)
                            rb.Attributes.Add(attribut.Key, attribut.Value);
                    }

                    #endregion


                    label = new HtmlGenericControl("label");
                    label.Attributes.Add("for", String.Concat(_rbGroupName, "_", entry.Key));
                    label.InnerHtml = entry.Value;

                    rbItem.Controls.Add(rb);
                    rbItem.Controls.Add(label);
                    listRb.Controls.Add(rbItem);
                }




                if (this.IsLabelBefore)
                {
                    this.PanelField.Controls.Add(htmlLabel);
                    this.PanelField.Controls.Add(listRb);
                }
                else
                {
                    this.PanelField.Controls.Add(listRb);
                    this.PanelField.Controls.Add(htmlLabel);
                }

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
                if (_customRadioButtonStyleAttributes != null && _customRadioButtonStyleAttributes.Count > 0)
                {
                    foreach (KeyValuePair<string, string> style in _customRadioButtonStyleAttributes)
                        ((HtmlGenericControl)this.FieldControl).Style.Add(style.Key, style.Value);
                }

                if (!String.IsNullOrEmpty(_customRadioButtonCSSClasses))
                {
                    ((HtmlGenericControl)this.FieldControl).Attributes["class"] = String.Concat(((HtmlGenericControl)this.FieldControl).Attributes["class"], " ", _customRadioButtonCSSClasses).Trim();
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