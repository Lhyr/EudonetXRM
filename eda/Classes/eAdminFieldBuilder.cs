using EudoQuery;
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal.eda;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    [System.Obsolete("Utiliser les classes qui héritent de eAdminField")]
    public class eAdminFieldBuilder
    {
        /// <summary>Construction du champ</summary>
        /// <param name="panel">Paneau conteneur du champ</param>
        /// <param name="cat">Catégorie</param>
        /// <param name="fieldCode">Code du champ (sous catégorie)</param>
        /// <param name="descid">Descid table/champ</param>
        ///<param name="pref">pref user</param>
        ///<param name="tooltip">Tooltip</param>
        ///<param name="value">Valeur</param>
        /// <param name="format">Format</param>
        /// <param name="label">Libellé</param>
        /// <param name="btn">Bouton ajouté en fin de champ (pour type varchar et num) </param>
        /// <returns></returns>
        //     public static Control BuildField(Panel panel, AdminFieldType format, String label, eAdminUpdateProperty.CATEGORY cat, int fieldCode = 0, String tooltip = "", String value = "", HtmlGenericControl btn = null)
        public static Control BuildField(Panel panel, AdminFieldType format, String label, eAdminUpdateProperty.CATEGORY cat, int fieldCode = 0, String tooltip = "", String value = "", ePref pref = null, Int32 descid = 0, HtmlGenericControl btn = null)
        {
            Control c = new Control();

            String attrDsc = String.Concat(cat.GetHashCode(), "|", fieldCode);
            Panel divField = new Panel();
            divField.CssClass = "field";

            if (!String.IsNullOrEmpty(tooltip))
            {
                divField.ToolTip = tooltip;
            }

            panel.Controls.Add(divField);

            try
            {
                switch (format)
                {
                    case AdminFieldType.ADM_TYPE_BIT: c = BuildCheckbox(divField, label, attrDsc, value); break;
                    case AdminFieldType.ADM_TYPE_CHAR: c = BuildTextbox(divField, label, attrDsc, value, 1, btn); break;
                    case AdminFieldType.ADM_TYPE_NUM:
                        c = BuildTextbox(divField, label, attrDsc, value, 1, btn);
                        ((TextBox)c).CssClass = "numInput"; break;
                        ((TextBox)c).Attributes.Add("edec", "0");

                    case AdminFieldType.ADM_TYPE_MEMO: c = BuildTextbox(divField, label, attrDsc, value, 3); break;
                    case AdminFieldType.ADM_TYPE_PICTO: c = BuildPictoField(divField, label, attrDsc, value); break;
                    case AdminFieldType.ADM_TYPE_HIDDEN: c = BuildHiddenField(divField, attrDsc, value); break;
                    case AdminFieldType.ADM_TYPE_FIELDTYPE: c = BuildFieldTypesList(pref, divField, label, attrDsc, value); break;
                }

                if (descid != 0)
                {
                    if (c is TextBox)
                    {
                        ((TextBox)c).Attributes.Add("did", descid.ToString());
                    }
                    else if (c is CheckBox)
                    {
                        ((CheckBox)c).Attributes.Add("did", descid.ToString());
                    }
                    else if (c is DropDownList)
                    {
                        ((DropDownList)c).Attributes.Add("did", descid.ToString());
                    }
                }


            }
            catch (Exception)
            {
                // TODO: Exception à gérer
                return null;
            }



            return c;
        }

        private static TextBox BuildHiddenField(Panel divField, string attrDsc, string value)
        {
            TextBox hidField = new TextBox();
            hidField.Style.Add("display", "none");
            hidField.Attributes.Add("dsc", attrDsc);
            hidField.Text = value;

            divField.Controls.Add(hidField);

            return hidField;
        }

        /// <summary>Création du champ Picto</summary>
        /// <param name="panelField">Le bloc</param>
        /// <param name="label">Le libellé</param>
        private static HtmlGenericControl BuildPictoField(Panel panelField, string label, String attrDsc, String value)
        {
            HtmlGenericControl htmlLabel = new HtmlGenericControl("label");
            htmlLabel.InnerText = label;
            // TODO: à remplacer plus tard par un bouton ouvrant une popup
            HtmlGenericControl btn = new HtmlGenericControl();
            btn.Attributes.Add("class", value);
            btn.Attributes.Add("onclick", "nsAdmin.openPictoPopup();");
            panelField.Controls.Add(htmlLabel);
            panelField.Controls.Add(btn);

            return btn;
        }

        /// <summary>Construction du checkbox et de son libellé</summary>
        /// <param name="panelField">Le bloc dans lequel les éléments doivent être construits</param>
        /// <param name="label">Libellé</param>
        private static eCheckBoxCtrl BuildCheckbox(Panel panelField, String labelName, String attrDsc, String value)
        {
            String controlId = "chk" + attrDsc.Replace("|", "");

            eCheckBoxCtrl checkbox = new eCheckBoxCtrl(value == "1", false);
            checkbox.ID = controlId;
            checkbox.Attributes.Add("dsc", attrDsc);
            checkbox.AddClick("top.nsAdmin.onCheckboxClick(this)");
            checkbox.AddText(labelName);
            checkbox.ToolTip = labelName;
            checkbox.ToolTipChkBox = labelName;

            panelField.Controls.Add(checkbox);

            return checkbox;
        }


        /// <summary>Construction du champ texte</summary>
        /// <param name="panelField">Le bloc</param>
        /// <param name="label">Libellé</param>
        /// <param name="nbRows">Nombre de lignes (par défaut à 1)</param>
        /// <param name="btn">Bouton après input</param>
        private static TextBox BuildTextbox(Panel panelField, String label, String attrDsc, String value, int nbRows = 1, HtmlGenericControl btn = null)
        {
            TextBox textbox = new TextBox();
            textbox.Attributes.Add("dsc", attrDsc);
            if (nbRows > 1)
            {
                textbox.TextMode = TextBoxMode.MultiLine;
                textbox.Rows = nbRows;
            }

            if (btn != null)
                textbox.Attributes.Add("ehasbtn", "1");

            textbox.Text = value;

            HtmlGenericControl htmlInfo = new HtmlGenericControl("p");
            htmlInfo.InnerText = label;
            htmlInfo.Attributes.Add("class", "info");
            panelField.Controls.Add(textbox);
            if (btn != null)
                panelField.Controls.Add(btn);

            panelField.Controls.Add(htmlInfo);


            return textbox;
        }

        /// <summary>Construction d'un slider pour la performance</summary>
        /// <param name="wrapper"></param>
        /// <param name="id"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="step"></param>
        /// <param name="value"></param>
        /// <param name="removeVal">Valeurs à supprimer du slider.</param>
        /// <param name="panelField">The panel field.</param>
        public static HtmlGenericControl BuildRangeSlider(Control wrapper, String id, int min, int max, int step, int value)
        {
            HtmlGenericControl slider = new HtmlGenericControl("div");
            slider.ID = id;
            slider.Attributes.Add("class", "nouislider");
            slider.Attributes.Add("data-min", min.ToString());
            slider.Attributes.Add("data-max", max.ToString());
            slider.Attributes.Add("data-step", step.ToString());
            slider.Attributes.Add("data-value", value.ToString());

            wrapper.Controls.Add(slider);

            return slider;
        }

        /// <summary>Construction d'un petit séparateur</summary>
        /// <param name="panelField">The panel field.</param>
        /// <param name="label">The label.</param>
        public static Panel BuildSeparator(Panel panelField, String label)
        {
            Panel pTitle = new Panel();
            pTitle.CssClass = "subTitle";
            pTitle.Attributes.Add("onclick", "nsAdmin.showHideSubPart(this);");
            // Icône
            HtmlGenericControl icon = new HtmlGenericControl("span");
            icon.Attributes.Add("class", "icon-unvelop");
            pTitle.Controls.Add(icon);
            // Titre
            HtmlGenericControl title = new HtmlGenericControl("span");
            title.Attributes.Add("class", "sepLabel");
            title.InnerText = label;
            pTitle.Controls.Add(title);
            panelField.Controls.Add(pTitle);

            return pTitle;
        }


        /// <summary>
        /// Bouton pour champ de paramétre
        /// </summary>
        /// <param name="sID">Id du bouton</param>
        /// <param name="sCss">Icon css du bouton (sans le pr éfixe "icon-")</param>
        /// <param name="sAction">Action Javascript</param>
        /// <param name="sTooltip">Tooltip</param>
        /// <returns></returns>
        public static HtmlGenericControl GetButtonFieldParam(string sID, string sCss, string sAction = "", string sTooltip = "")
        {


            HtmlGenericControl btn = new HtmlGenericControl("span");
            btn.InnerText = "";
            btn.ID = sID;
            btn.Attributes.Add("class", String.Concat("icon-", sCss, " adminFieldParamBtn"));
            btn.Attributes.Add("ebtnparam", "1");
            btn.Attributes.Add("title", sTooltip);
            if (sAction.Length > 0)
                btn.Attributes.Add("onclick", sAction);

            return btn;
        }

        public static DropDownList BuildFieldTypesList(ePref pref, Panel panel, String label, String attr, String value)
        {
            HtmlGenericControl htmlLabel = new HtmlGenericControl("label");
            htmlLabel.InnerText = label;

            DropDownList ddl = new DropDownList();
            ddl.Attributes.Add("dsc", attr);
            CreateDdlItemForFieldType(pref, ddl, FieldFormat.TYP_HIDDEN);
            CreateDdlItemForFieldType(pref, ddl, FieldFormat.TYP_TITLE);
            CreateDdlItemForFieldType(pref, ddl, FieldFormat.TYP_CHAR);
            CreateDdlItemForFieldType(pref, ddl, FieldFormat.TYP_DATE);
            CreateDdlItemForFieldType(pref, ddl, FieldFormat.TYP_BIT);
            CreateDdlItemForFieldType(pref, ddl, FieldFormat.TYP_EMAIL);
            CreateDdlItemForFieldType(pref, ddl, FieldFormat.TYP_PHONE);
            CreateDdlItemForFieldType(pref, ddl, FieldFormat.TYP_WEB);
            CreateDdlItemForFieldType(pref, ddl, FieldFormat.TYP_USER);
            CreateDdlItemForFieldType(pref, ddl, FieldFormat.TYP_MEMO);
            CreateDdlItemForFieldType(pref, ddl, FieldFormat.TYP_IMAGE);
            CreateDdlItemForFieldType(pref, ddl, FieldFormat.TYP_FILE);
            CreateDdlItemForFieldType(pref, ddl, FieldFormat.TYP_IFRAME);
            CreateDdlItemForFieldType(pref, ddl, FieldFormat.TYP_CHART);
            CreateDdlItemForFieldType(pref, ddl, FieldFormat.TYP_NUMERIC);
            CreateDdlItemForFieldType(pref, ddl, FieldFormat.TYP_COUNT);
            CreateDdlItemForFieldType(pref, ddl, FieldFormat.TYP_SOCIALNETWORK);

            // Valeur sélectionnée
            if (!String.IsNullOrEmpty(value))
            {
                ListItem selectedListItem = ddl.Items.FindByValue(value);
                if (selectedListItem != null)
                {
                    selectedListItem.Selected = true;
                };
            }

            panel.Controls.Add(htmlLabel);
            panel.Controls.Add(ddl);

            return ddl;
        }

        public static HtmlGenericControl BuildRadioButtons(Panel panel, String label, String groupName, Dictionary<String, String> items)
        {
            Panel divField = new Panel();
            divField.CssClass = "field";

            HtmlGenericControl htmlLabel = new HtmlGenericControl("p");
            htmlLabel.Attributes.Add("class", "info");
            htmlLabel.InnerText = label;

            HtmlGenericControl listRb = new HtmlGenericControl("ul");
            listRb.Attributes.Add("class", "listRB");
            HtmlGenericControl rbItem;
            RadioButton rb;
            foreach (KeyValuePair<string, string> entry in items)
            {
                rbItem = new HtmlGenericControl("li");
                rb = new RadioButton();
                rb.GroupName = groupName;
                rb.Text = entry.Key;
                rb.Attributes.Add("value", entry.Value);
                rbItem.Controls.Add(rb);
                listRb.Controls.Add(rbItem);
            }

            divField.Controls.Add(listRb);
            divField.Controls.Add(htmlLabel);

            panel.Controls.Add(divField);

            return listRb;
        }

        private static void CreateDdlItemForFieldType(ePref pref, DropDownList ddl, FieldFormat format)
        {
            ListItem item = new ListItem(eAdminTools.GetFieldTypeLabel(pref, format), format.GetHashCode().ToString());
            ddl.Items.Add(item);
        }
    }

}