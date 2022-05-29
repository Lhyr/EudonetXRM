using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminAdvancedAutomationRenderer : eAdminAbstractAutomationRenderer
    {
        protected String _fieldName;
        eAdminFieldInfos _fieldInfos;
        FormulaType currentFormula = FormulaType.TOP;
        Int32 _nField;

        /// <summary>
        /// constructeur par défaut
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        public eAdminAdvancedAutomationRenderer(ePref pref, int nTab, int nField) : base(pref, nTab)
        {
            if (Pref.User.UserLevel < UserLevel.LEV_USR_SUPERADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();

            _nField = nField;
        }

        protected override bool Init()
        {

            _fieldInfos = eAdminFieldInfos.GetAdminFieldInfos(Pref, _nField);
            _fieldName = _fieldInfos.Labels[Pref.LangId];

            return base.Init();
        }


        protected override void GenerateHeader()
        {
            _panelHeader.CssClass = "edaFormulaHeader";
            _pgContainer.Controls.Add(_panelHeader);

            HtmlGenericControl span = new HtmlGenericControl("span");
            span.Attributes.Add("id", "spanAnchorTop");
            span.Attributes.Add("class", "spanAnchor active");
            _panelHeader.Controls.Add(span);
            HtmlGenericControl ancre = new HtmlGenericControl("a");
            ancre.Attributes.Add("edaDivId", "edaFormulaDivSpacerTop");
            ancre.InnerText = eResApp.GetRes(Pref, 528); //top
            ancre.Attributes.Add("onclick", "nsAdmin.adminAutomatismsAnchors(this);");
            span.Controls.Add(ancre);

            span = new HtmlGenericControl("span");
            span.Attributes.Add("id", "spanAnchorMiddle");
            span.Attributes.Add("class", "spanAnchor");
            _panelHeader.Controls.Add(span);
            ancre = new HtmlGenericControl("a");
            ancre.Attributes.Add("edaDivId", "edaFormulaDivSpacerMiddle");
            ancre.InnerText = eResApp.GetRes(Pref, 7214); // middlle
            ancre.Attributes.Add("onclick", "nsAdmin.adminAutomatismsAnchors(this);");
            span.Controls.Add(ancre);

            span = new HtmlGenericControl("span");
            span.Attributes.Add("id", "spanAnchorBottom");
            span.Attributes.Add("class", "spanAnchor");
            _panelHeader.Controls.Add(span);
            ancre = new HtmlGenericControl("a");
            ancre.Attributes.Add("edaDivId", "edaFormulaDivSpacerBottom");
            ancre.InnerText = eResApp.GetRes(Pref, 7215);//bottom
            ancre.Attributes.Add("onclick", "nsAdmin.adminAutomatismsAnchors(this);");
            span.Controls.Add(ancre);
        }


        /// <summary>
        /// Création du contenu de la popup
        /// </summary>
        protected override void GenerateMainContent()
        {

            foreach (FormulaType type in new List<FormulaType>() { FormulaType.TOP, FormulaType.MIDDLE, FormulaType.BOTTOM })
            {
                currentFormula = type;

                HtmlGenericControl divSpacer = new HtmlGenericControl("div");
                divSpacer.Attributes.Add("id", String.Concat("edaFormulaDivSpacer", GetName(currentFormula)));
                divSpacer.Attributes.Add("class", "edaFormulaDivSpacer");
                _panelContent.Controls.Add(divSpacer);

                _panelSections = new Panel();
                _panelContent.Controls.Add(_panelSections);

                base.GenerateMainContent();
            }
        }

        private enum FormulaType
        {
            TOP,
            MIDDLE,
            BOTTOM
        }

        private string GetName(FormulaType type)
        {
            string returnValue = String.Empty;
            switch (type)
            {
                case FormulaType.TOP:
                    returnValue = "Top";
                    break;
                case FormulaType.MIDDLE:
                    returnValue = "Middle";
                    break;
                case FormulaType.BOTTOM:
                    returnValue = "Bottom";
                    break;
                default:
                    returnValue = String.Empty;
                    break;
            }

            return returnValue;
        }

        /// <summary>
        /// Récupère la valeur du champs du trigger
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        protected override string GetFieldValue(NotificationTriggerField field)
        {

            if (field == NotificationTriggerField.LABEL)
                return GetTitle();

            return String.Empty;
        }

        /// <summary>
        /// Récupère la valeur à afficher du champs du trigger
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        protected override string GetFieldDisplayValue(NotificationTriggerField field)
        {
            return GetFieldValue(field);
        }



        protected string GetTitle()
        {
            string returnValue = String.Empty;
            switch (currentFormula)
            {
                case FormulaType.TOP:
                    returnValue = eResApp.GetRes(Pref, 528);
                    break;
                case FormulaType.MIDDLE:
                    returnValue = eResApp.GetRes(Pref, 7214);
                    break;
                case FormulaType.BOTTOM:
                    returnValue = eResApp.GetRes(Pref, 7215);
                    break;
                default:
                    returnValue = String.Empty;
                    break;
            }

            return returnValue;
        }

        private int GetFormulaMaxLength(FormulaType type)
        {
            int returnValue = 0;
            switch (type)
            {
                case FormulaType.TOP:
                    returnValue = 1024;
                    break;
                case FormulaType.MIDDLE:
                    returnValue = 0;
                    break;
                case FormulaType.BOTTOM:
                    returnValue = 1024;
                    break;
                default:
                    returnValue = 0;
                    break;
            }

            return returnValue;
        }

        private string GetValue(FormulaType type)
        {
            string returnValue = String.Empty;
            switch (type)
            {
                case FormulaType.TOP:
                    if (_fieldInfos.DefaultFormat)
                        returnValue = _fieldInfos.Default;
                    else
                        returnValue = String.Empty;
                    break;
                case FormulaType.MIDDLE:
                    returnValue = _fieldInfos.FormulaMiddle;
                    break;
                case FormulaType.BOTTOM:
                    returnValue = _fieldInfos.Formula;
                    break;
                default:
                    returnValue = String.Empty;
                    break;
            }

            return returnValue;
        }
        protected override bool EditEnabled()
        {
            return false;
        }


        protected override void GenerateFooter() {/* Ne rien faire */ }
        public static eRenderer CreateAdvancedAutomationRenderer(ePref pref, int nTab, int nField)
        {
            return new eAdminAdvancedAutomationRenderer(pref, nTab, nField);
        }

        protected override void AppendAllCasesContent(Panel panelStep)
        {
            HtmlGenericControl stepContent = new HtmlGenericControl("div");
            stepContent.Attributes.Add("class", "stepContent");
            stepContent.Attributes.Add("data-active", "1");
            // stepContent.Style.Add("display", "inline-block");
            panelStep.Controls.Add(stepContent);

            HtmlGenericControl labelAllCases = new HtmlGenericControl("div");
            labelAllCases.Attributes.Add("class", "edaAllCases");
            labelAllCases.InnerText = eResApp.GetRes(Pref, 7227).ToUpper();
            stepContent.Controls.Add(labelAllCases);

            string txtName = String.Concat("edaFormulaSQL", currentFormula.ToString().ToLower());

            HtmlGenericControl formuleDivContainer = new HtmlGenericControl("div");
            formuleDivContainer.Attributes.Add("class", "edaFormulaSQLContainer");
            stepContent.Controls.Add(formuleDivContainer);

            HtmlGenericControl formuleLabel = new HtmlGenericControl("label");
            formuleLabel.Attributes.Add("for", txtName);
            formuleLabel.Attributes.Add("class", "edaFormulaSQLLabel");
            formuleLabel.InnerText = String.Concat(eResApp.GetRes(Pref, 7228), " :");
            formuleDivContainer.Controls.Add(formuleLabel);

            HtmlGenericControl formuleTextarea = new HtmlGenericControl("textarea");
            formuleTextarea.Attributes.Add("id", txtName);
            formuleTextarea.Attributes.Add("name", txtName);
            formuleTextarea.Attributes.Add("class", "edaFormulaSQLText");
            formuleTextarea.Attributes.Add("rows", "5");
            int maxLength = GetFormulaMaxLength(currentFormula);
            if (maxLength > 0)
                formuleTextarea.Attributes.Add("maxlength", maxLength.ToString());
            formuleTextarea.InnerText = GetValue(currentFormula);
            formuleDivContainer.Controls.Add(formuleTextarea);

        }

        /// <summary>
        /// Rempli le select par des rubriques de la table
        /// </summary>
        protected override void FillWithTabFields(HtmlGenericControl select, Int32 targetDescId)
        {
            HtmlGenericControl fieldOption = new HtmlGenericControl("option");
            fieldOption.Attributes.Add("selected", "selected");
            fieldOption.Attributes.Add("value", _fieldInfos.DescId.ToString());
            fieldOption.InnerText = String.Concat(_tabName, ".", _fieldInfos, " (", _fieldInfos.DescId, ")");
            select.Controls.Add(fieldOption);
        }


        /// <summary>
        /// On ajoute des infos cachés
        /// </summary>
        /// <returns></returns>
        protected override bool End()
        {

            HtmlGenericControl inputFieldId = new HtmlGenericControl("input");
            inputFieldId.Attributes.Add("type", "hidden");
            inputFieldId.Attributes.Add("id", "edaFieldId");
            inputFieldId.Attributes.Add("name", "edaFieldId");
            inputFieldId.Attributes.Add("value", _fieldInfos.DescId.ToString());
            _panelHidden.Controls.Add(inputFieldId);

            return base.End();
        }

        protected override void RenderSystemAttributes(NotificationTriggerField field, HtmlGenericControl htmlControl)
        {
            htmlControl.ID = GetClientId(field);
        }

        protected override void RenderSystemAttributes(NotificationTriggerField field, WebControl control)
        {
            control.ID = GetClientId(field);
        }
    }
}