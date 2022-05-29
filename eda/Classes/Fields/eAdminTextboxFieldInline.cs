using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal.eda;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminTextboxFieldInline : eAdminField
    {
        Int32 _nbRows;
        AdminFieldType _fieldType;
        int _inputWidth;
        String _prefixText;
        String _suffixText;
        LabelType _labelType;

        public enum LabelType
        {
            ABOVE,
            BELOW
        }

        public eAdminTextboxFieldInline(int descid, String label, eAdminUpdateProperty.CATEGORY propCat, Int32 propCode, AdminFieldType type = AdminFieldType.ADM_TYPE_CHAR, String tooltiptext = "", String value = "", String idBlock = "", String idField = "", int inputWidth = 0, String prefixtext = "", String suffixtext = "", LabelType labelType = LabelType.ABOVE)
            : base(descid, label, propCat, propCode, tooltiptext, value)
        {
            _nbRows = 1;
            _fieldType = type;
            _inputWidth = inputWidth;
            _prefixText = prefixtext;
            _suffixText = suffixtext;
            _labelType = labelType;
            this.FieldID = idBlock;
            this.FieldControlID = idField;

            // Equivalent en FieldFormat pour gérer la validation avec eValidator côté JS
            switch (type)
            {
                case AdminFieldType.ADM_TYPE_MEMO:
                case AdminFieldType.ADM_TYPE_CHAR: Format = FieldFormat.TYP_CHAR; break;
                case AdminFieldType.ADM_TYPE_NUM: Format = FieldFormat.TYP_NUMERIC; break;
            }
        }

        protected override Boolean Build(Panel panel)
        {
            if (base.Build(panel))
            {
                TextBox textbox = new TextBox();
                textbox.Attributes.Add("dsc", this.AttrDsc);
                textbox.Text = this.Value;
                if (_fieldType == AdminFieldType.ADM_TYPE_NUM)
                {
                    textbox.CssClass = "numInput";
                }
                if (_inputWidth > 0)
                {
                    textbox.Style.Add("width", String.Concat(_inputWidth, "px"));
                }

                //Optionnel
                List<eLibConst.RESADV_TYPE> lst = new List<eLibConst.RESADV_TYPE>() {
                    eLibConst.RESADV_TYPE.TOOLTIP
                    , eLibConst.RESADV_TYPE.WATERMARK
                    , eLibConst.RESADV_TYPE.RESULT_LABEL_SINGULAR
                    , eLibConst.RESADV_TYPE.RESULT_LABEL_PLURAL
                    , eLibConst.RESADV_TYPE.UNIT
                };
                if (PropCategory == eAdminUpdateProperty.CATEGORY.RESADV && lst.Select(t => (int)t).Contains(PropCode))
                    textbox.Attributes.Add("opt", "1");

                this.FieldControl = textbox;

                HtmlGenericControl htmlInfo = new HtmlGenericControl("p");
                htmlInfo.InnerText = this.Label;
                htmlInfo.Attributes.Add("class", "info");
                this.FieldLabel = htmlInfo;

                if (_labelType == LabelType.ABOVE)
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

                return true;
            }

            return false;

        }

        protected override Boolean End()
        {
            if (base.End())
            {
                this.PanelField.CssClass = String.Concat(this.PanelField.CssClass, " ", "fieldInline");
            }

            return true;
        }
    }
}