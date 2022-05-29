using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public abstract class eAdminField
    {
        /// <summary>
        /// Descid
        /// </summary>
        public Int32 DescID { get; private set; }
        /// <summary>
        /// Libellé
        /// </summary>
        public String Label { get; private set; }
        /// <summary>
        /// Catégorie de mise à jour de la propriété d'admin (DESC/PREF/RES...)
        /// </summary>
        public eAdminUpdateProperty.CATEGORY PropCategory { get; private set; }
        /// <summary>
        /// Hashcode de la propriété à mettre à jour
        /// </summary>
        public Int32 PropCode { get; private set; }

        /// <summary>
        /// Infobulle
        /// </summary>
        public String TooltipText { get; set; }
        /// <summary>
        /// Valeur du champ
        /// </summary>
        public String Value { get; set; }
        public String AttrDsc { get; private set; }
        public Panel PanelField { get; protected set; }
        public Control FieldLabel { get; protected set; }
        public Control FieldControl { get; protected set; }
        public String FieldControlID { get; protected set; }
        public String FieldID { get; protected set; }
        public FieldFormat Format { get; protected set; }
        public Boolean IsLabelBefore { get; set; }
        public Boolean ReadOnly { get; set; }
        public Boolean IsOptional { get; set; }
        public Boolean IsDisplayed { get; set; }
        public Boolean NoUpdate { get; set; }
        public Boolean? Mandatory { get; set; }
        /// <summary>place holder exemple de saisie </summary>
        public string PlaceHolder { get; set; }


        public ePref Pref { get; protected set; }

        public eAdminField(int descid, String label, eAdminUpdateProperty.CATEGORY propCat, Int32 propCode, String tooltiptext = "", String value = "", Boolean mandatory = false)
        {
            this.DescID = descid;
            this.Label = label;
            this.PropCategory = propCat;
            this.PropCode = propCode;
            this.TooltipText = tooltiptext;
            this.Value = value;
            this.AttrDsc = String.Concat(this.PropCategory.GetHashCode(), "|", this.PropCode);
            this.FieldControlID = String.Empty;
            this.ReadOnly = false;
            this.IsOptional = false;
            this.IsDisplayed = true;
            this.NoUpdate = false;
            this.Mandatory = mandatory;
        }

        public eAdminField(int descid, String label, String tooltiptext = "")
        {
            this.DescID = descid;
            this.Label = label;
            this.TooltipText = tooltiptext;
            this.IsDisplayed = true;
        }

        /// <summary>
        /// Ajoute à l'attribut "dsc" la valeur de la catégorie de CONFIGADV
        /// </summary>
        /// <param name="configAdvCat">CONFIGADV_CATEGORY</param>
        public void SetConfigAdvCategory(eLibConst.CONFIGADV_CATEGORY configAdvCat)
        {
            this.AttrDsc = String.Concat(this.AttrDsc, "|", (int)configAdvCat);
        }

        public void SetFieldControlID(String id)
        {
            this.FieldControlID = id;
        }

        public virtual Boolean Generate(Panel panel)
        {
            if (Init()
                && Build(panel)
                && End())
            {
                return true;
            }
            return false;
        }

        protected virtual Boolean Init()
        {
            return true;
        }


        public Panel ParentPanel { get; private set; } = new Panel();

        protected virtual Boolean Build(Panel panel)
        {

            this.PanelField = new Panel();
            ParentPanel = PanelField;

            this.PanelField.CssClass = "field";
            this.PanelField.Attributes.Add("data-active", IsDisplayed ? "1" : "0");
            if (!String.IsNullOrEmpty(FieldID))
                this.PanelField.ID = FieldID;

            if (!String.IsNullOrEmpty(this.TooltipText))
            {
                this.PanelField.ToolTip = this.TooltipText;
            }
            panel.Controls.Add(this.PanelField);

            return true;
        }

        protected virtual Boolean End()
        {
            if (FieldControl != null)
            {
                if (FieldControl is TextBox)
                {
                    TextBox textbox = ((TextBox)FieldControl);
                    textbox.Attributes.Add("did", this.DescID.ToString());
                    textbox.Attributes.Add("format", this.Format.GetHashCode().ToString());
                    if (this.ReadOnly)
                        textbox.Enabled = false;
                    if (this.IsOptional)
                        textbox.Attributes.Add("opt", "1");
                    if (this.NoUpdate)
                        textbox.Attributes.Add("noupdate", "1");

                    if (!String.IsNullOrEmpty(PlaceHolder))
                        textbox.Attributes.Add("placeholder", PlaceHolder);
                }
                else if (FieldControl is CheckBox)
                {
                    CheckBox checkbox = ((CheckBox)FieldControl);
                    checkbox.InputAttributes.Add("did", this.DescID.ToString());
                    checkbox.InputAttributes.Add("format", this.Format.GetHashCode().ToString());

                    if (this.ReadOnly)
                        checkbox.Enabled = false;
                    if (this.IsOptional)
                        checkbox.Attributes.Add("opt", "1");
                    if (this.NoUpdate)
                        checkbox.Attributes.Add("noupdate", "1");
                }
                else if (FieldControl is eCheckBoxCtrl)
                {
                    eCheckBoxCtrl checkbox = ((eCheckBoxCtrl)FieldControl);
                    checkbox.Attributes.Add("did", this.DescID.ToString());

                    checkbox.Attributes.Add("format", this.Format.GetHashCode().ToString());
                    //if (this.ReadOnly)
                    //    checkbox.Enabled = false;
                    if (this.IsOptional)
                        checkbox.Attributes.Add("opt", "1");
                    if (this.NoUpdate)
                        checkbox.Attributes.Add("noupdate", "1");
                }
                else if (FieldControl is DropDownList)
                {
                    DropDownList ddl = ((DropDownList)FieldControl);
                    ddl.Attributes.Add("did", this.DescID.ToString());

                    ddl.Attributes.Add("format", this.Format.GetHashCode().ToString());
                    if (this.ReadOnly)
                        ddl.Enabled = false;
                    if (this.IsOptional)
                        ddl.Attributes.Add("opt", "1");
                    if (this.NoUpdate)
                        ddl.Attributes.Add("noupdate", "1");
                }
                else if (FieldControl is RadioButton)
                {
                    RadioButton rb = ((RadioButton)FieldControl);
                    rb.InputAttributes.Add("did", this.DescID.ToString());


                    rb.InputAttributes.Add("format", this.Format.GetHashCode().ToString());
                    if (this.ReadOnly)

                        rb.Enabled = false;
                    if (this.IsOptional)
                        rb.Attributes.Add("opt", "1");
                    if (this.NoUpdate)
                        rb.Attributes.Add("noupdate", "1");
                }

                if (!String.IsNullOrEmpty(this.FieldControlID))
                {
                    this.FieldControl.ID = this.FieldControlID;
                }

            }

            if (this is eAdminButtonField)
            {
                this.PanelField.CssClass = String.Concat(this.PanelField.CssClass, " linkButton");
            }
            return true;
        }

        public void HideField()
        {
            this.PanelField.Visible = false;
        }

        /// <summary>
        /// Ajoute un attribut au contrôle
        /// A exécuter après le Build
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="value"></param>
        public void SetControlAttribute(String attribute, String value)
        {
            if (FieldControl != null)
            {
                if (FieldControl is TextBox)
                {
                    TextBox textbox = ((TextBox)FieldControl);
                    textbox.Attributes.Add(attribute, value);
                }
                else if (FieldControl is CheckBox)
                {
                    CheckBox checkbox = ((CheckBox)FieldControl);
                    checkbox.Attributes.Add(attribute, value);
                }
                else if (FieldControl is eCheckBoxCtrl)
                {
                    eCheckBoxCtrl checkbox = ((eCheckBoxCtrl)FieldControl);
                    checkbox.Attributes.Add(attribute, value);
                }
                else if (FieldControl is DropDownList)
                {
                    DropDownList ddl = ((DropDownList)FieldControl);
                    ddl.Attributes.Add(attribute, value);
                }
                else if (FieldControl is RadioButton)
                {
                    RadioButton rb = ((RadioButton)FieldControl);
                    rb.Attributes.Add(attribute, value);
                }
                else if (FieldControl is HyperLink)
                {
                    HyperLink hl = ((HyperLink)FieldControl);
                    hl.Attributes.Add(attribute, value);
                }
            }
        }

        /// <summary>
        /// Ajoute un attribut au bloc "field"
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="value"></param>
        public void SetFieldAttribute(String attribute, String value)
        {
            this.PanelField.Attributes.Add(attribute, value);
        }

        /// <summary>
        /// Ajoute un attribut au contrôle
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="value"></param>
        protected void SetFieldControlAttribute(String attribute, String value)
        {
            ((WebControl)this.FieldControl).Attributes.Add(attribute, value);
        }

    }
}