using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;


namespace Com.Eudonet.Xrm.eda
{
    public class eAdminTextboxField : eAdminField
    {
        Int32 _nbRows;
        AdminFieldType _fieldType;
        String _prefixText;
        String _suffixText;
        LabelType _labelType;
        Dictionary<string, string> _customTextboxStyleAttributes;
        Dictionary<string, string> _customPanelStyleAttributes;
        Dictionary<string, string> _customLabelStyleAttributes;
        string _customTextboxCSSClasses;
        string _customPanelCSSClasses;
        string _customLabelCSSClasses;
        bool _passwordField;
        string _sId = "";
        eAdminIconField _icon;
        bool _optional;
        string _onChange;
        bool? _mandatory;
        List<string> _autocompletionList = new List<string>();


        public enum LabelType
        {
            INLINE,
            ABOVE,
            BELOW
        }

        /// <summary>
        /// Constructeur eAdminTextboxField : pour l'affichage d'un champ texte
        /// </summary>
        /// <param name="descid"></param>
        /// <param name="label"></param>
        /// <param name="propCat"></param>
        /// <param name="propCode"></param>
        /// <param name="type"></param>
        /// <param name="tooltiptext"></param>
        /// <param name="value"></param>
        /// <param name="nbRows"></param>
        /// <param name="customTextboxStyleAttributes"></param>
        /// <param name="customTextboxCSSClasses"></param>
        /// <param name="customPanelStyleAttributes"></param>
        /// <param name="customPanelCSSClasses"></param>
        /// <param name="customLabelStyleAttributes"></param>
        /// <param name="customLabelCSSClasses"></param>
        /// <param name="prefixText"></param>
        /// <param name="suffixText"></param>
        /// <param name="labelType"></param>
        /// <param name="passwordField"></param>
        /// <param name="id"></param>
        /// <param name="icon">eAdminIconField non généré, afin d'afficher une icône à côté du champ texte</param>
        public eAdminTextboxField(
            int descid, String label, eAdminUpdateProperty.CATEGORY propCat, Int32 propCode, AdminFieldType type = AdminFieldType.ADM_TYPE_CHAR, String tooltiptext = "", String value = "", int nbRows = 1,
            Dictionary<string, string> customTextboxStyleAttributes = null, string customTextboxCSSClasses = "",
            Dictionary<string, string> customPanelStyleAttributes = null, string customPanelCSSClasses = "",
            Dictionary<string, string> customLabelStyleAttributes = null, string customLabelCSSClasses = "",
            String prefixText = "", String suffixText = "", LabelType labelType = LabelType.ABOVE, bool passwordField = false, String id = "", eAdminIconField icon = null, bool readOnly = false, bool optional = false, string onChange = "", List<string> autocplList = null, bool mandatory = false)
            : base(descid, label, propCat, propCode, tooltiptext, value)
        {
            _nbRows = nbRows;
            _fieldType = type;
            _customTextboxStyleAttributes = customTextboxStyleAttributes;
            _customTextboxCSSClasses = customTextboxCSSClasses;
            _customPanelStyleAttributes = customPanelStyleAttributes;
            _customPanelCSSClasses = customPanelCSSClasses;
            _customLabelStyleAttributes = customLabelStyleAttributes;
            _customLabelCSSClasses = customLabelCSSClasses;
            _prefixText = prefixText;
            _suffixText = suffixText;
            _labelType = labelType;
            _passwordField = passwordField;
            _sId = id;
            _icon = icon;
            this.ReadOnly = readOnly;
            _optional = optional;
            _onChange = onChange;
            _mandatory = mandatory;
            _autocompletionList = (autocplList == null) ? new List<string>() : autocplList;

            // Equivalent en FieldFormat pour gérer la validation avec eValidator côté JS
            switch (type)
            {
                case AdminFieldType.ADM_TYPE_MEMO:
                case AdminFieldType.ADM_TYPE_CHAR: Format = FieldFormat.TYP_CHAR; break;
                case AdminFieldType.ADM_TYPE_NUM: Format = FieldFormat.TYP_NUMERIC; break;
                case AdminFieldType.ADM_TYPE_MAIL: Format = FieldFormat.TYP_EMAIL; break;
            }
        }

        public void SetAutocompleteList(List<string> dataList)
        {

        }

        protected override Boolean Build(Panel panel)
        {
            if (base.Build(panel))
            {
                TextBox textbox = new TextBox();
                textbox.Attributes.Add("dsc", this.AttrDsc);
                if (!String.IsNullOrEmpty(_sId))
                {
                    textbox.ID = _sId;
                }
                if (_nbRows > 1)
                {
                    textbox.TextMode = TextBoxMode.MultiLine;
                    textbox.Rows = _nbRows;
                }

                if (_passwordField)
                    textbox.TextMode = TextBoxMode.Password;

                textbox.Text = this.Value;

                if (_fieldType == AdminFieldType.ADM_TYPE_NUM)
                {
                    textbox.CssClass = "numInput";
                }

                //onChange
                if (!String.IsNullOrEmpty(_onChange))
                    textbox.Attributes.Add("onchange", _onChange);


                //Optionnel
                List<eLibConst.RESADV_TYPE> lst = new List<eLibConst.RESADV_TYPE>() {
                    eLibConst.RESADV_TYPE.TOOLTIP
                    , eLibConst.RESADV_TYPE.WATERMARK
                    , eLibConst.RESADV_TYPE.RESULT_LABEL_SINGULAR
                    , eLibConst.RESADV_TYPE.RESULT_LABEL_PLURAL
                    , eLibConst.RESADV_TYPE.UNIT
                };
                if ((PropCategory == eAdminUpdateProperty.CATEGORY.RESADV && lst.Select(t => (int)t).Contains(PropCode))
                    || _optional
                    )
                    textbox.Attributes.Add("opt", "1");

                this.FieldControl = textbox;

                HtmlGenericControl htmlInfo = new HtmlGenericControl(_labelType == LabelType.INLINE ? "label" : "p");
                htmlInfo.InnerText = this.Label;
                htmlInfo.Attributes.Add("class", "info");

                //Mandatory
                if (_mandatory.HasValue)
                {
                    htmlInfo.Attributes.Add("mandatory", _mandatory.Value ? "1" : "0");
                    if (_mandatory.Value)
                    {
                        HtmlGenericControl mandatoryParam = new HtmlGenericControl("span");
                        mandatoryParam.Attributes.Add("class", "mandatoryParam");
                        mandatoryParam.Style.Add(HtmlTextWriterStyle.Display, "contents");
                        mandatoryParam.InnerText = "*";
                        htmlInfo.Controls.Add(mandatoryParam);
                    }
                }


                this.FieldLabel = htmlInfo;

                if (_labelType != LabelType.BELOW)
                {
                    this.PanelField.Controls.Add(htmlInfo);
                }

                if (!String.IsNullOrEmpty(_prefixText))
                {
                    HtmlGenericControl labelPrefix = new HtmlGenericControl("label");
                    labelPrefix.Attributes.Add("class", "inlinePrefix");
                    labelPrefix.InnerText = _prefixText;
                    this.PanelField.Controls.Add(labelPrefix);
                }

                this.PanelField.Controls.Add(textbox);

                if (!String.IsNullOrEmpty(_suffixText))
                {
                    HtmlGenericControl labelSuffix = new HtmlGenericControl("label");
                    labelSuffix.Attributes.Add("class", "inlineSuffix");
                    labelSuffix.InnerText = _suffixText;
                    this.PanelField.Controls.Add(labelSuffix);
                }

                if (_labelType == LabelType.BELOW)
                {
                    this.PanelField.Controls.Add(htmlInfo);
                }

                if (_icon != null)
                {

                    _icon.Generate(this.PanelField);
                    ((HtmlGenericControl)(_icon.FieldControl)).Attributes.Add("ebtnparam", "1");
                    textbox.Attributes.Add("ehasbtn", "1");
                }

                if (_autocompletionList.Count > 0)
                {
                    HtmlGenericControl opt;
                    string txtboxID = String.IsNullOrEmpty(_sId) ? "txt" : _sId;
                    textbox.Attributes.Add("list", String.Concat(txtboxID, "List"));

                    HtmlGenericControl datalist = new HtmlGenericControl("datalist");
                    datalist.ID = String.Concat(txtboxID, "List");
                    foreach (string option in _autocompletionList)
                    {
                        opt = new HtmlGenericControl("option");
                        opt.Attributes.Add("value", option);
                        datalist.Controls.Add(opt);
                    }

                    this.PanelField.Controls.Add(datalist);
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
                if (_customTextboxStyleAttributes != null && _customTextboxStyleAttributes.Count > 0)
                {
                    foreach (KeyValuePair<string, string> style in _customTextboxStyleAttributes)
                        ((TextBox)this.FieldControl).Style.Add(style.Key, style.Value);
                }

                if (!String.IsNullOrEmpty(_customTextboxCSSClasses))
                {
                    ((TextBox)this.FieldControl).CssClass = String.Concat(((TextBox)this.FieldControl).CssClass, " ", _customTextboxCSSClasses).Trim();
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