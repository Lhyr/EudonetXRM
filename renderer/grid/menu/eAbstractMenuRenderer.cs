using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Objet permettant d'afficher le contenu de la page d'accueil
    /// </summary>
    public abstract class eAbstractMenuRenderer : eRenderer
    {
        /// <summary>
        /// The file
        /// </summary>
        protected eFile _file;
        /// <summary>
        /// Visible
        /// </summary>
        protected bool _isVisible;
        /// <summary>
        /// Contexte grille-widget
        /// </summary>
        protected eXrmWidgetContext _context;

        /// <summary>
        /// TODO Besoin d'un objet metier de la table
        /// </summary>     
        public eAbstractMenuRenderer(bool isVisible, eFile file, eXrmWidgetContext context)
        {
            _isVisible = isVisible;
            _file = file;
            _context = context;
        }
        /// <summary>
        /// Appel l'objet métier
        /// eList/eFiche (l'appel a EudoQuery est fait dans cet appel ainsi que l'appel et le parcours du dataset)
        /// </summary>
        /// <returns></returns>
        protected override bool Init() { return true; }

        /// <summary>
        /// Lance le rendu de menu
        /// </summary>
        /// <returns></returns>
        protected override bool Build() { return true; }


        /// <summary>
        /// Construit des objets html annexes/place des appel JS d'apres chargement
        /// </summary>
        /// <returns></returns>
        protected override bool End() { return true; }

        /// <summary>
        /// Lance la génération Generate() du renderer et return PgContainer
        /// </summary>
        /// <returns></returns>
        public Control BuildMenu()
        {
            if (Generate())
                return _pgContainer;

            throw new Exception("Impossible de générer le menu");
        }

        /// <summary>
        /// Construit le contenu des 3 tabs
        /// </summary>
        /// <returns></returns>
        protected Control RenderMenu(string tabId, string title, string subTitle)
        {
            _pgContainer.ID = tabId;
            _pgContainer.CssClass = "paramBlock";

            if (_isVisible)
                _pgContainer.Style.Add("display", "block");

            HtmlGenericControl h3 = new HtmlGenericControl("h3");
            h3.InnerHtml = title;
            _pgContainer.Controls.Add(h3);

            HtmlGenericControl p = new HtmlGenericControl("p");
            p.InnerHtml = subTitle;
            p.Attributes.Add("class", "info");
            _pgContainer.Controls.Add(p);

            Panel paramBlockContent = new Panel();
            paramBlockContent.CssClass = "paramBlockContent";

            RenderContentMenu(paramBlockContent);

            _pgContainer.Controls.Add(paramBlockContent);

            return _pgContainer;
        }

        /// <summary>
        /// recupère le contenu du menu
        /// </summary>
        /// <returns></returns>
        protected virtual void RenderContentMenu(Panel paramBlockContent)
        {
            paramBlockContent.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 8148)));
        }

        /// <summary>
        /// Construit un element d'un menu
        /// </summary>
        /// <returns></returns>
        protected Control RenderMenuItem(int index, string menuItemTitle, bool dataActive, Control menuItemContent)
        {
            Panel paramPart = new Panel();
            paramPart.ID = "paramPart_" + index;
            paramPart.CssClass = "paramPart";

            bool bShowHeader = !String.IsNullOrEmpty(menuItemTitle);
            if (bShowHeader)
            {
                HtmlGenericControl header = new HtmlGenericControl("header");
                Panel caret = new Panel();
                caret.CssClass = dataActive ? "icon-caret-down" : "icon-caret-right";
                header.Controls.Add(caret);

                HtmlGenericControl h4 = new HtmlGenericControl("h4");
                h4.InnerHtml = menuItemTitle;
                header.Controls.Add(h4);

                paramPart.Controls.Add(header);
            }

            Panel paramPartContent = new Panel();
            paramPartContent.CssClass = "paramPartContent";
            paramPartContent.Attributes.Add("index", index.ToString());


            paramPartContent.Attributes.Add("data-active", dataActive ? "1" : "0");
            paramPartContent.Attributes.Add("eactive", dataActive ? "1" : "0");


            paramPartContent.Controls.Add(menuItemContent);

            if (!bShowHeader)
                return paramPartContent;

            paramPart.Controls.Add(paramPartContent);

            return paramPart;
        }

        /// <summary>
        /// Construit un champ text
        /// </summary>
        /// <param name="label">The label.</param>
        /// <param name="field">The field.</param>
        /// <param name="mapAttribute">The map attribute.</param>
        /// <param name="bDisabled">if set to <c>true</c> [b disabled].</param>
        /// <param name="paramName">Name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <param name="onchange">The onbluronchange.</param>
        /// <param name="toBeTranslated">if set to <c>true</c> [to be translated].</param>
        /// <param name="containerId">The container identifier.</param>
        /// <param name="isVisible">if set to <c>true</c> [is visible].</param>
        /// <param name="icon">The icon.</param>
        /// <param name="reloadHeader">if set to <c>true</c> [reload header].</param>
        /// <returns></returns>
        protected Control BuildInputField(String label, eFieldRecord field,
            string mapAttribute = "", bool bDisabled = false, string paramName = null, string value = null, string onchange = null, bool toBeTranslated = false, string containerId = "", bool isVisible = true,
            HtmlGenericControl icon = null, bool reloadHeader = false)
        {
            HtmlGenericControl fieldDiv = new HtmlGenericControl("div");
            fieldDiv.Attributes.Add("data-active", "1");
            if (!String.IsNullOrEmpty(containerId))
                fieldDiv.Attributes.Add("id", containerId);
            if (!isVisible)
                fieldDiv.Style.Add("display", "none");

            HtmlGenericControl p = new HtmlGenericControl("p");
            p.InnerText = label;
            fieldDiv.Attributes.Add("class", "info");
            fieldDiv.Controls.Add(p);

            Control input = BuildInputFieldInnerControl(field, mapAttribute, bDisabled, paramName, value, onchange, toBeTranslated);
            if (icon != null)
                ((HtmlGenericControl)input).Attributes.Add("ehasbtn", "1");

            // Rechargement de la liste des grilles
            if (reloadHeader)
                ((HtmlGenericControl)input).Attributes.Add("data-reloadgrids", "1");


            fieldDiv.Controls.Add(input);

            if (icon != null)
                fieldDiv.Controls.Add(icon);

            return fieldDiv;
        }

        /// <summary>
        /// Contruit le champ html input text pour BuildInputField et BuildColorPicker
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="mapAttribute">The map attribute.</param>
        /// <param name="bDisabled">if set to <c>true</c> [b disabled].</param>
        /// <param name="paramName">Name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <param name="onchange">The onchange.</param>
        /// <param name="toBeTranslated">if set to <c>true</c> [to be translated].</param>
        /// <param name="isColorPickerInput">if set to <c>true</c> [is color picker input].</param>
        /// <returns></returns>
        private Control BuildInputFieldInnerControl(eFieldRecord field, string mapAttribute = "", bool bDisabled = false, string paramName = null, string value = null, string onchange = null, bool toBeTranslated = false, bool isColorPickerInput = false)
        {
            HtmlGenericControl input = new HtmlGenericControl("input");
            input.Attributes.Add("type", "text");
            input.Attributes.Add("value", value == null ? field.DisplayValue : value);

            if (!field.RightIsUpdatable || bDisabled)
                input.Attributes.Add("disabled", "true");

            if (!String.IsNullOrEmpty(paramName))
            {
                input.Attributes.Add("paramName", paramName);
                input.Attributes.Add("paramValue", value ?? "");
            }

            if (!String.IsNullOrEmpty(onchange))
            {
                input.Attributes.Add("onchange", onchange);
            }

            // Si le champ doit être traduit, on ajoute l'attribut "lang"
            if (toBeTranslated)
                input.Attributes.Add("lang", Pref.LangId.ToString());

            if (isColorPickerInput)
                input.Attributes.Add("class", "txtColor");

            AddSystemAttributes(field, input, mapAttribute, paramName: paramName);

            return input;
        }


        protected Control BuildColorPicker(eFieldRecord field, string label, string tooltip, string mapAttribute = "", bool bDisabled = false, string paramName = null, string value = null, string onblur = null, bool toBeTranslated = false)
        {
            Panel PanelField = new Panel();
            PanelField.CssClass = "field";
            PanelField.Attributes.Add("data-active", "1");

            if (!String.IsNullOrEmpty(tooltip))
            {
                PanelField.ToolTip = tooltip;
            }

            Panel colorWrapper = new Panel();
            colorWrapper.CssClass = "colorWrapper";

            #region Textbox
            Control textbox = BuildInputFieldInnerControl(field, mapAttribute, bDisabled, paramName, value, onblur, toBeTranslated, true);
            #endregion

            string _txtColorID = textbox.ID;
            string _colorPickerID = String.Concat(_txtColorID, "_colorPicker");

            #region Colorpicker
            Panel colorPickerWrapper = new Panel();
            colorPickerWrapper.CssClass = "colorPickerWrapper";
            colorPickerWrapper.Attributes.Add("onclick", String.Concat("pickColor(document.getElementById('", _colorPickerID, "'), document.getElementById('", _txtColorID, "')", !String.IsNullOrEmpty(onblur) ? String.Concat(", function() {document.getElementById('", _txtColorID, "').onblur();}") : String.Empty, ");"));

            HtmlGenericControl colorPicker = new HtmlGenericControl();
            colorPicker.ID = _colorPickerID;
            colorPicker.Attributes.Add("class", "colorPicker");
            if (!String.IsNullOrEmpty(value))
            {
                colorPicker.Style.Add("background-color", value);
            }

            colorPickerWrapper.Controls.Add(colorPicker);
            #endregion

            colorWrapper.Controls.Add(textbox);
            colorWrapper.Controls.Add(colorPickerWrapper);

            HtmlGenericControl labelField = new HtmlGenericControl("p");
            labelField.Attributes.Add("class", "info");
            labelField.InnerText = label;

            PanelField.Controls.Add(labelField);
            PanelField.Controls.Add(colorWrapper);

            return PanelField;
        }

        protected Control BuildInput(String label, String value, String inputId = "", String onchange = "", string sToolTip = "")
        {
            HtmlGenericControl fieldDiv = new HtmlGenericControl("div");

            HtmlGenericControl p = new HtmlGenericControl("p");
            p.InnerText = label;
            fieldDiv.Attributes.Add("class", "info");
            fieldDiv.Controls.Add(p);

            HtmlGenericControl input = new HtmlGenericControl("input");
            input.ID = inputId;
            input.Attributes.Add("type", "text");
            input.Attributes.Add("value", value);
            input.Attributes.Add("onchange", onchange);

            fieldDiv.Controls.Add(input);


            if (sToolTip.Length > 0)
                fieldDiv.Attributes.Add("title", sToolTip);
            return fieldDiv;
        }

        /// <summary>
        /// Construit un bouton avec les proprité du champ correspondant
        /// </summary>
        /// <param name="label">The label.</param>
        /// <param name="tooltip">The tooltip.</param>
        /// <param name="iconField">The icon field.</param>
        /// <param name="colorField">The color field.</param>
        /// <param name="displayDefaultIfEmpty">if set to <c>true</c> [display default if empty].</param>
        /// <returns></returns>
        protected Control BuildPictoField(string label, string tooltip, eFieldRecord iconField, eFieldRecord colorField, bool displayDefaultIfEmpty = true)
        {
            eFontIcons.FontIcons icon;
            if (String.IsNullOrEmpty(iconField.Value) && !displayDefaultIfEmpty)
                icon = new eFontIcons.FontIcons();
            else
                icon = eFontIcons.GetFontIcon(iconField.Value);

            HtmlGenericControl fieldDiv = new HtmlGenericControl("div");
            fieldDiv.Attributes.Add("data-active", "1");
            fieldDiv.Attributes.Add("class", "field");
            fieldDiv.Attributes.Add("title", tooltip);

            HtmlGenericControl pictoLabel = new HtmlGenericControl("label");
            pictoLabel.InnerText = label;
            fieldDiv.Controls.Add(pictoLabel);

            HtmlGenericControl btnSelectPicto = new HtmlGenericControl("div");
            btnSelectPicto.ID = "btnSelectPicto";
            btnSelectPicto.Attributes.Add("onclick", "oAdminGridMenu.openPicto(this);");

            fieldDiv.Controls.Add(btnSelectPicto);

            HtmlGenericControl selectedPicto = new HtmlGenericControl("span");
            selectedPicto.ID = "selectedPicto";
            selectedPicto.Attributes.Add("class", icon.CssName);

            if (!string.IsNullOrEmpty(colorField.Value))
                selectedPicto.Attributes.Add("style", "color:" + colorField.Value);

            // Necessaire pour la popup
            selectedPicto.Attributes.Add("picto-key", iconField.Value);
            selectedPicto.Attributes.Add("picto-color", colorField.Value);
            selectedPicto.Attributes.Add("picto-class", icon.CssName);

            btnSelectPicto.Controls.Add(selectedPicto);

            HtmlGenericControl iconFieldLib = new HtmlGenericControl("input");
            iconFieldLib.Attributes.Add("type", "hidden");
            fieldDiv.Controls.Add(iconFieldLib);
            AddSystemAttributes(iconField, iconFieldLib);

            HtmlGenericControl colorFieldLib = new HtmlGenericControl("input");
            colorFieldLib.Attributes.Add("type", "hidden");
            fieldDiv.Controls.Add(colorFieldLib);
            AddSystemAttributes(colorField, colorFieldLib);

            btnSelectPicto.Attributes.Add("color-header", colorFieldLib.ID);
            btnSelectPicto.Attributes.Add("icon-header", iconFieldLib.ID);

            return fieldDiv;
        }


        /// <summary>
        /// Construit un bouton avec les proprité du champ correspondant
        /// </summary>
        /// <param name="label"></param>
        /// <param name="tooltip"></param>
        /// <param name="field"></param>
        /// <param name="clientClick"></param>
        /// <returns></returns>
        protected Control BuildBtnField(string label, string tooltip = "", eFieldRecord field = null, string clientClick = "", string paramName = null, string value = null, string containerId = "", bool isVisible = true, string btnId = "")
        {

            Panel btn = new Panel();
            btn.CssClass = "field linkButton";
            if (!String.IsNullOrEmpty(containerId))
                btn.Attributes.Add("id", containerId);
            if (!isVisible)
                btn.Style.Add("display", "none");

            HtmlGenericControl link = new HtmlGenericControl("a");
            link.Attributes.Add("href", "#");
            link.Attributes.Add("onclick", clientClick);
            link.InnerText = label;
            link.Attributes.Add("title", tooltip);
            if (!String.IsNullOrEmpty(btnId))
                link.ID = btnId;

            if (!String.IsNullOrEmpty(paramName))
            {
                link.Attributes.Add("paramName", paramName);
                link.Attributes.Add("paramValue", value);
            }

            AddSystemAttributes(field, link, paramName: paramName);

            btn.Controls.Add(link);
            return btn;
        }

        /// <summary>
        /// Builds the hidden field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="paramName">Name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        protected Control BuildHiddenField(eFieldRecord field, string paramName, string value)
        {
            HtmlGenericControl hidden = new HtmlGenericControl("input");
            hidden.Attributes.Add("type", "hidden");
            hidden.Attributes.Add("paramName", paramName);
            hidden.Attributes.Add("paramValue", value);
            AddSystemAttributes(field, hidden, paramName: paramName);
            return hidden;
        }


        /// <summary>
        /// Construit un bouton avec les proprité du champ correspondant
        /// </summary>
        /// <param name="label">The label.</param>
        /// <param name="field">The field.</param>
        /// <param name="clientClick">The client click.</param>
        /// <param name="sYesLabel">The s yes label.</param>
        /// <param name="sNoLabel">The s no label.</param>
        /// <param name="sToolTipYes">The s tool tip yes.</param>
        /// <param name="sToolTipeNo">The s tool tipe no.</param>
        /// <param name="bCarriageReturn">if set to <c>true</c> [b carriage return].</param>
        /// <param name="paramName">Name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <param name="sYesValue">The s yes value.</param>
        /// <param name="sNoValue">The s no value.</param>
        /// <returns></returns>
        protected Control BuildYesNoOptionField(string label, eFieldRecord field, string clientClick, string sYesLabel = null, string sNoLabel = null, string sToolTipYes = null, string sToolTipeNo = null, bool bCarriageReturn = false, String paramName = null, String value = null, String sYesValue = null, String sNoValue = null)
        {
            if (sYesValue == null)
                sYesValue = "1";

            if (sNoValue == null)
                sNoValue = "0";

            bool yesChecked = false;
            if (value == null)
                yesChecked = field.Value == sYesValue || field.Value.ToLower() == "true";
            else
                yesChecked = value == sYesValue || value.ToLower() == "true";


            if (String.IsNullOrEmpty(sYesLabel))
                sYesLabel = eResApp.GetRes(Pref, 58);
            if (String.IsNullOrEmpty(sNoLabel))
                sNoLabel = eResApp.GetRes(Pref, 59);


            HtmlGenericControl fieldDiv = new HtmlGenericControl("div");
            fieldDiv.Attributes.Add("data-active", "1");

            HtmlGenericControl p = new HtmlGenericControl("p");
            p.InnerText = label;
            fieldDiv.Attributes.Add("class", "info");
            fieldDiv.Controls.Add(p);

            HtmlGenericControl optionContainer = new HtmlGenericControl("div");
            optionContainer.Attributes.Add("class", "option-container");
            optionContainer.Attributes.Add("onclick", String.IsNullOrEmpty(clientClick) ? "oAdminGridMenu.setBoolValue(this, event);" : clientClick);
            if (paramName != null)
                optionContainer.Attributes.Add("paramName", paramName);
            optionContainer.Attributes.Add("paramValue", yesChecked ? sYesValue : sNoValue);

            HtmlGenericControl yesRadioBtn = new HtmlGenericControl("input");
            yesRadioBtn.ID = field.FldInfo.Descid.ToString() + "_" + _file.FileId.ToString() + "_y";
            yesRadioBtn.Attributes.Add("type", "radio");
            yesRadioBtn.Attributes.Add("value", sYesValue);
            yesRadioBtn.Attributes.Add("name", field.FldInfo.Descid.ToString() + "_" + _file.FileId.ToString());

            if (yesChecked)
            {
                yesRadioBtn.Attributes.Add("checked", "checked");
            }


            optionContainer.Controls.Add(yesRadioBtn);

            HtmlGenericControl yesLabel = new HtmlGenericControl("label");
            yesLabel.Attributes.Add("for", yesRadioBtn.ID);
            yesLabel.InnerText = sYesLabel;
            yesLabel.Attributes.Add("title", String.IsNullOrEmpty(sToolTipYes) ? "" : sToolTipYes);
            optionContainer.Controls.Add(yesLabel);

            if (bCarriageReturn)
            {
                HtmlGenericControl br = new HtmlGenericControl("br");
                optionContainer.Controls.Add(br);
            }

            HtmlGenericControl noRadioBtn = new HtmlGenericControl("input");
            noRadioBtn.ID = field.FldInfo.Descid.ToString() + "_" + _file.FileId.ToString() + "_n";
            noRadioBtn.Attributes.Add("type", "radio");
            noRadioBtn.Attributes.Add("value", sNoValue);
            noRadioBtn.Attributes.Add("name", field.FldInfo.Descid.ToString() + "_" + _file.FileId.ToString());


            if (!yesChecked)
            {
                noRadioBtn.Attributes.Add("checked", "checked");
            }

            optionContainer.Controls.Add(noRadioBtn);

            HtmlGenericControl noLabel = new HtmlGenericControl("label");
            noLabel.Attributes.Add("for", noRadioBtn.ID);
            noLabel.InnerText = sNoLabel;
            noLabel.Attributes.Add("title", String.IsNullOrEmpty(sToolTipYes) ? "" : sToolTipeNo);
            optionContainer.Controls.Add(noLabel);

            AddSystemAttributes(field, optionContainer, paramName: paramName);
            fieldDiv.Controls.Add(optionContainer);

            return fieldDiv;
        }

        /// <summary>
        /// Builds the checkbox field.
        /// </summary>
        /// <param name="label">The label.</param>
        /// <param name="field">The field.</param>
        /// <param name="paramName">Name of the parameter.</param>
        /// <param name="paramValue">The parameter value.</param>
        /// <param name="onClick">JS function on click</param>
        /// <returns></returns>
        protected Control BuildCheckboxField(string label, eFieldRecord field, string paramName, string paramValue, string onClick)
        {
            HtmlGenericControl fieldDiv = new HtmlGenericControl("div");
            fieldDiv.Attributes.Add("data-active", "1");
            fieldDiv.Attributes.Add("class", "info");

            eCheckBoxCtrl chk = new eCheckBoxCtrl((paramValue == "1"), false);
            chk.AddClick(onClick);
            chk.AddText(label);
            chk.Attributes.Add("paramName", paramName);
            chk.Attributes.Add("paramValue", paramValue);
            chk.Attributes.Add("wid", _context.WidgetId.ToString());
            // Ajout de l'ID
            String fieldID = String.Concat("fld_", eLibTools.GetTabFromDescId(field.FldInfo.Descid), "_", field.FldInfo.Descid, "_", _file.FileId);
            if (!String.IsNullOrEmpty(paramName))
                fieldID = String.Concat(fieldID, "_", paramName);
            chk.ID = fieldID;

            fieldDiv.Controls.Add(chk);


            return fieldDiv;
        }

        /// <summary>
        /// Construit un element select 
        /// </summary>
        /// <returns></returns>
        protected Control BuildSelectOptionField(string label, string tooltip, eFieldRecord field, List<Tuple<string, string>> data, string onChange,
            string paramName = null, string value = null, string containerId = "", bool isVisible = true, bool disableNoValue = false, HtmlGenericControl htmlSelect = null)
        {
            if (value == null)
                value = field.Value;

            HtmlGenericControl fieldDiv = new HtmlGenericControl("div");
            fieldDiv.Attributes.Add("data-active", "1");
            if (!String.IsNullOrEmpty(containerId))
                fieldDiv.Attributes.Add("id", containerId);
            if (!isVisible)
                fieldDiv.Style.Add("display", "none");

            HtmlGenericControl p = new HtmlGenericControl("p");
            p.InnerText = label;
            fieldDiv.Attributes.Add("class", "info");
            fieldDiv.Attributes.Add("title", tooltip);
            fieldDiv.Controls.Add(p);

            if (htmlSelect == null)
            {
                HtmlGenericControl select = new HtmlGenericControl("select");

                HtmlGenericControl option;

                if (!disableNoValue)
                {
                    option = new HtmlGenericControl("option");
                    option.Attributes.Add("value", "");
                    option.InnerText = eResApp.GetRes(Pref, 8166);
                    select.Controls.Add(option);
                }

                foreach (var tuple in data)
                {
                    option = new HtmlGenericControl("option");
                    option.Attributes.Add("value", tuple.Item1.ToString());
                    option.InnerText = tuple.Item2;
                    if (value == tuple.Item1.ToString())
                        option.Attributes.Add("selected", "true");

                    select.Controls.Add(option);
                }

                htmlSelect = select;
            }



            string sOnChange = String.IsNullOrEmpty(onChange) ? "" : onChange;

            if (!String.IsNullOrEmpty(paramName))
            {
                htmlSelect.Attributes.Add("paramName", paramName);
                htmlSelect.Attributes.Add("paramValue", String.IsNullOrEmpty(value) && data.Count > 0 ? data[0].Item1.ToString() : value);
                if (String.IsNullOrEmpty(onChange))
                    sOnChange = "oAdminGridMenu.updateParam(this);";
            }
            else
            {
                if (String.IsNullOrEmpty(onChange))
                    sOnChange = "oAdminGridMenu.selectChanged(this);";
            }

            htmlSelect.Attributes.Add("onchange", sOnChange);
            fieldDiv.Controls.Add(htmlSelect);

            AddSystemAttributes(field, htmlSelect, paramName: paramName);

            return fieldDiv;
        }

        /// <summary>
        /// Création d'un catalogue de valeurs multiples
        /// </summary>
        /// <param name="label">Libellé du champ param</param>
        /// <param name="field">eFieldRecord</param>
        /// <param name="descid">DescID de la rubrique catalogue</param>
        /// <param name="paramName">Nom du paramètre</param>
        /// <param name="value">Valeurs séparées par des ;</param>
        /// <param name="displayValue">Valeurs affichées</param>
        /// <returns></returns>
        protected Control BuildMultiCatalog(string label, eFieldRecord field, int descid, string paramName, string value, string displayValue)
        {
            HtmlGenericControl fieldDiv = new HtmlGenericControl("div");
            fieldDiv.Attributes.Add("data-active", "1");

            HtmlGenericControl p = new HtmlGenericControl("p");
            p.InnerText = label;
            fieldDiv.Attributes.Add("class", "info");
            fieldDiv.Controls.Add(p);

            HtmlGenericControl tb = new HtmlGenericControl("input");
            tb.Attributes.Add("type", "text");
            tb.Attributes.Add("class", "txtCatalog");
            tb.Attributes.Add("paramName", paramName);
            tb.Attributes.Add("paramValue", value);
            tb.Attributes.Add("ehasbtn", "1");
            tb.Attributes.Add("opencatdid", descid.ToString());
            tb.Attributes.Add("readonly", "readonly");
            tb.Attributes.Add("value", displayValue);
            tb.Attributes.Add("title", displayValue);
            AddSystemAttributes(field, tb, "", paramName);
            fieldDiv.Controls.Add(tb);

            HtmlGenericControl icon = new HtmlGenericControl();
            icon.Attributes.Add("class", "icon-catalog adminFieldParamBtn");
            icon.Attributes.Add("ebtnparam", "1");
            icon.Attributes.Add("opencatdid", descid.ToString());
            icon.Attributes.Add("ename", tb.ID);
            fieldDiv.Controls.Add(icon);

            return fieldDiv;
        }

        /// <summary>
        /// Construit une zone avec drag'n'drop permettant d'uploader une image
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        protected Control BuildImageArea(eFieldRecord field, string id = "", bool isVisible = true)
        {
            HtmlGenericControl container = new HtmlGenericControl("div");
            container.Attributes.Add("class", "field-container");
            if (!String.IsNullOrEmpty(id))
                container.Attributes.Add("id", id);
            if (!isVisible)
                container.Style.Add("display", "none");

            //Champ image
            HtmlGenericControl file = new HtmlGenericControl("input");
            file.Attributes.Add("type", "file");
            file.Attributes.Add("class", "widgetImg");
            // Ne pas faire apparaitre le bouton           
            file.Attributes.Add("onchange", "oAdminGridMenu.saveImage(this);");
            // Tout type d'image             
            file.Attributes.Add("accept", "image/*");
            container.Controls.Add(file);
            AddSystemAttributes(field, file);

            //Champ cache propriete => TODO pour refacto propre du JS
            /*HtmlGenericControl property = new HtmlGenericControl("input");
            file.Attributes.Add("type", "hidden");
            file.Attributes.Add("class", "widgetImgProperty");
            container.Controls.Add(file);
            AddSystemAttributes(_file.GetField((int)XrmWidgetField.ContentParam), property);*/

            HtmlGenericControl dashed = new HtmlGenericControl("div");
            dashed.Attributes.Add("class", "dashed");
            container.Controls.Add(dashed);

            // Ajout d'evenement de dragNdrop d'image
            dashed.Attributes.Add("ondragover", "UpFilDragOver(this, event);return false;");
            dashed.Attributes.Add("ondrop", "oAdminGridMenu.dropImage(event)");
            dashed.Attributes.Add("ondragleave", "UpFilDragLeave(this); return false;");

            HtmlGenericControl label = new HtmlGenericControl("label");
            label.Attributes.Add("for", file.ID);
            label.InnerText = eResApp.GetRes(Pref, 8004);
            dashed.Controls.Add(label);

            return container;
        }

        /// <summary>
        /// Ajout des attributs systèmes
        /// </summary>
        /// <param name="field"></param>
        /// <param name="target"></param>
        protected void AddSystemAttributes(eFieldRecord field, HtmlGenericControl target, string mapAttribute = "", string paramName = "")
        {
            if (field == null)
                return;

            String fieldID = String.Concat("fld_", eLibTools.GetTabFromDescId(field.FldInfo.Descid), "_", field.FldInfo.Descid, "_", _file.FileId);
            if (!String.IsNullOrEmpty(paramName))
                fieldID = String.Concat(fieldID, "_", paramName);
            target.ID = fieldID;
            target.Attributes.Add("dbv", field.Value);
            target.Attributes.Add("dis", field.RightIsUpdatable ? "1" : "0");
            target.Attributes.Add("wid", _file.FileId.ToString());
            target.Attributes.Add("fmt", field.FldInfo.Format.GetHashCode().ToString());
            target.Attributes.Add("attr", mapAttribute);
            target.Attributes.Add("did", field.FldInfo.Descid.ToString());
        }

        /// <summary>
        /// Crée un textbox permettant de mettre à jour un param de widget (XrmWidgetParam)
        /// </summary>
        /// <param name="wid">The wid.</param>
        /// <param name="label">The label.</param>
        /// <param name="paramName">Name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <param name="hasResCode">Indique si la valeur est traduisible (RESCODE)</param>
        /// <param name="rCode">ResCode</param>
        /// <param name="rLoc">Emplacement du ResCode</param>
        /// <returns></returns>
        protected Panel BuildWidgetParamTextbox(int wid, string label, string paramName, string value, bool hasResCode = false, int rCode = 0, eResLocation rLoc = null)
        {
            Panel pField = new Panel();
            pField.CssClass = "info";

            HtmlGenericControl pLabel = new HtmlGenericControl("p");
            pLabel.InnerText = label;
            pField.Controls.Add(pLabel);

            value = value ?? "";

            TextBox input = new TextBox();
            input.Text = value;

            if (!String.IsNullOrEmpty(paramName))
            {
                input.Attributes.Add("paramName", paramName);
                input.Attributes.Add("paramValue", value);
            }

            input.Attributes.Add("onchange", $"oAdminGridMenu.updateParamValue(this, {wid})");

            // Si le champ doit être traduit
            if (hasResCode)
            {
                input.Attributes.Add("data-hasrcode", "1");

                if (rCode > 0)
                {
                    input.Attributes.Add("data-rcode", rCode.ToString());
                }

                if (rLoc != null)
                {
                    rLoc.CreateAttribute(input, paramName);
                }
            }

            pField.Controls.Add(input);

            return pField;
        }

        /// <summary>
        /// Construit un champ permettant de sélectionner une fiche existante
        /// </summary>
        /// <param name="wid">The wid.</param>
        /// <param name="label">The label.</param>
        /// <param name="paramName">Name of the parameter.</param>
        /// <param name="dbValue">The database value.</param>
        /// <param name="displayValue">The display value.</param>
        /// <param name="tab">The tab.</param>
        /// <returns></returns>
        protected Panel BuildWidgetParamFileSelect(int wid, string label, string paramName, string dbValue, string displayValue, int tab)
        {
            Panel pField = new Panel();
            pField.CssClass = "info";

            HtmlGenericControl pLabel = new HtmlGenericControl("p");
            pLabel.InnerText = label;
            pField.Controls.Add(pLabel);

            if (String.IsNullOrEmpty(dbValue))
            {
                displayValue = string.Empty;
            }

            TextBox input = new TextBox();
            input.CssClass = "inputSelectFile";
            input.Text = displayValue;
            input.Attributes.Add("data-dbv", dbValue);
            input.ReadOnly = true;

            if (!String.IsNullOrEmpty(paramName))
            {
                input.Attributes.Add("paramName", paramName);
                input.Attributes.Add("paramValue", dbValue);
            }

            input.Attributes.Add("onchange", $"oAdminGridMenu.updateParamValue(this, {wid}, getAttributeValue(this, 'paramvalue'))");
            pField.Controls.Add(input);

            HtmlGenericControl icon = new HtmlGenericControl();
            icon.Attributes.Add("class", "icnFileBtn icon-hyperlink btnSelectFile");
            icon.Attributes.Add("data-tab", tab.ToString());
            pField.Controls.Add(icon);

            return pField;
        }
    }
}