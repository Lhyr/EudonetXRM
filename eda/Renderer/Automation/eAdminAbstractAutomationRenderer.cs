using Com.Eudonet.Engine.Notif;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Classe abstraite de rendu de l'administration des automatismes
    /// </summary>
    /// <seealso cref="Com.Eudonet.Xrm.eda.eAdminRenderer" />
    public abstract class eAdminAbstractAutomationRenderer : eAdminRenderer
    {
        #region propriétés       

        protected Int32 _nTab;
        protected String _tabName;

        protected Panel _panelHidden;
        protected Panel _panelHeader;
        protected Panel _panelContent;
        protected Panel _panelSections;
        #endregion

        /// <summary>
        /// Modification activée
        /// </summary>
        /// <returns></returns>
        protected abstract Boolean EditEnabled();

        /// <summary>
        /// constructeur par défaut
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        public eAdminAbstractAutomationRenderer(ePref pref, int nTab)
        {
            Pref = pref;
            _nTab = nTab;
        }

        /// <summary>
        /// Initialisation des params
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            if (Pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();

            eAdminTableInfos _tabInfos = new eAdminTableInfos(Pref, _nTab);
            _tabName = _tabInfos.TableLabel;

            // Le rendu du renderer
            _pgContainer.ID = "automatismsAdminModalContent";
            _pgContainer.Attributes.Add("class", "adminModalContent");
            _pgContainer.Attributes.Add("onscroll", "nsAdmin.adminAutomatismsScroll(this);");

            // Header principal
            _panelHeader = new Panel();

            // conteneur principal
            _panelContent = new Panel();
            _panelContent.CssClass = "edaFormulaContent";
            _pgContainer.Controls.Add(_panelContent);

            // Conteneur des sections
            _panelSections = new Panel();

            // panel contenant des champs cachés
            _panelHidden = new Panel();
            _panelHidden.Style.Add("display", "none");
            _pgContainer.Controls.Add(_panelHidden);

            return true;
        }

        /// <summary>
        /// Construit le html de l'objet demandé
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            try
            {
                GenerateHeader();
                GenerateMainContent();
                GenerateFooter();
            }
            catch (Exception)
            {
                throw;
            }

            return base.Build();
        }

        /// <summary>
        /// Méthode qui permet de génrére le header 
        /// </summary>
        protected abstract void GenerateHeader();

        /// <summary>
        /// Création du contenu de la popup
        /// </summary>
        protected virtual void GenerateMainContent()
        {
            CreateAutomationTitleSection();
            CreateAutomationPropritiesSection();
            CreateAutomationStepsSection();
        }


        /// <summary>
        /// Génére la section du titre de l'automatisme
        /// </summary>
        private void CreateAutomationTitleSection()
        {
            Panel panelTitle = new Panel();

            HtmlGenericControl titleDiv = new HtmlGenericControl("div");
            titleDiv.Attributes.Add("class", "edaFormulaField");
            panelTitle.Controls.Add(titleDiv);

            string titleName = "edaFormulaTitle";

            HtmlGenericControl titleLabel = new HtmlGenericControl("label");
            titleLabel.Attributes.Add("for", titleName);
            titleLabel.Attributes.Add("class", titleName);
            titleLabel.InnerText = String.Concat(eResApp.GetRes(Pref, 7216), " :");
            titleDiv.Controls.Add(titleLabel);

            HtmlGenericControl titleTextbox = new HtmlGenericControl("input");
            RenderSystemAttributes(NotificationTriggerField.LABEL, titleTextbox);

            titleTextbox.Attributes.Add("name", titleName);
            titleTextbox.Attributes.Add("class", titleName);
            titleTextbox.Attributes.Add("type", "text");
            if (!EditEnabled())
                titleTextbox.Attributes.Add("disabled", "disabled");

            titleTextbox.Attributes.Add("onchange", "setAttributeValue(this, 'dbv', this.value);return false;");
            string val = GetFieldDisplayValue(NotificationTriggerField.LABEL);
            titleTextbox.Attributes.Add("value", val);

            titleDiv.Controls.Add(titleTextbox);


            HtmlGenericControl ongletDiv = new HtmlGenericControl("div");
            ongletDiv.Attributes.Add("class", "edaFormulaField");
            panelTitle.Controls.Add(ongletDiv);

            HtmlGenericControl ongletSpan = new HtmlGenericControl("span");
            ongletSpan.InnerText = eResApp.GetRes(Pref, 7217).Replace("<TAB>", _tabName);
            ongletDiv.Controls.Add(ongletSpan);


            HtmlGenericControl actifDiv = new HtmlGenericControl("div");
            actifDiv.Attributes.Add("class", "edaFormulaField");
            panelTitle.Controls.Add(actifDiv);

            string activeName = "edaFormulaActive";

            HtmlGenericControl actifLabel = new HtmlGenericControl("label");
            actifLabel.Attributes.Add("for", activeName);
            actifLabel.Attributes.Add("class", activeName);

            actifLabel.InnerText = String.Concat(eResApp.GetRes(Pref, 7218), " :");
            actifDiv.Controls.Add(actifLabel);

            HtmlGenericControl actifSelect = new HtmlGenericControl("select");

            RenderSystemAttributes(NotificationTriggerField.STATUS, actifSelect);

            actifSelect.Attributes.Add("name", activeName);
            actifSelect.Attributes.Add("class", activeName);
            if (!EditEnabled())
                actifSelect.Attributes.Add("disabled", "disabled");

            actifDiv.Controls.Add(actifSelect);


            bool active = GetFieldValue(NotificationTriggerField.STATUS) == "1" || !EditEnabled(); // Todo pourri
            HtmlGenericControl option = new HtmlGenericControl("option");
            if (active)
                option.Attributes.Add("selected", "selected");

            option.Attributes.Add("value", "1");
            option.InnerText = eResApp.GetRes(Pref, 7219);
            actifSelect.Controls.Add(option);

            option = new HtmlGenericControl("option");
            if (!active)
                option.Attributes.Add("selected", "selected");

            option.Attributes.Add("value", "0");
            option.InnerText = eResApp.GetRes(Pref, 7135);

            actifSelect.Attributes.Add("onchange", "setAttributeValue(this, 'dbv', this.value);return false;");
            actifSelect.Controls.Add(option);

            _panelSections.Controls.Add(panelTitle);

        }

        /// <summary>
        /// Récupère la valeur de la base du champs du trigger
        /// </summary>
        /// <param name="field"></param>
        /// <param name="control"></param>
        protected abstract void RenderSystemAttributes(NotificationTriggerField field, HtmlGenericControl control);

        /// <summary>
        /// Ajoute les attributs au webcontrol
        /// </summary>
        /// <param name="field">Champ de la table NotificationTriggerField</param>
        /// <param name="control">WebControl</param>
        protected abstract void RenderSystemAttributes(NotificationTriggerField field, WebControl control);

        /// <summary>
        /// Récupère la valeur de la base du champs du trigger
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        protected abstract string GetFieldValue(NotificationTriggerField field);

        /// <summary>
        /// Récupère la valeur a afficher du champs du trigger
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        protected abstract string GetFieldDisplayValue(NotificationTriggerField field);


        /// <summary>
        /// Génère la section des caractéristiques de l'automatisme
        /// </summary>
        private void CreateAutomationPropritiesSection()
        {
            Panel panelStep = new Panel();
            panelStep.CssClass = "divStep";



            panelStep.Controls.Add(GenerateStepTitle("1", eResApp.GetRes(Pref, 7220), true));

            HtmlGenericControl stepContent = new HtmlGenericControl("div");
            RenderSystemAttributes(NotificationTriggerField.TRIGGER_ACTION, stepContent);

            int targetDescID = eLibTools.GetNum(GetFieldValue(NotificationTriggerField.TRIGGER_TARGET_DESCID));
            int eventAction = eLibTools.GetNum(GetFieldValue(NotificationTriggerField.TRIGGER_ACTION));
            if (targetDescID == 0)
            {
                targetDescID = _nTab + 96;
                eventAction = (int)NotifConst.Action.UPDATE;
                stepContent.Attributes["dbv"] = eventAction.ToString();
            }

            stepContent.Attributes.Add("class", "stepContent");
            stepContent.Attributes.Add("data-active", "1");

            panelStep.Controls.Add(stepContent);

            HtmlGenericControl triggerLabelDiv = new HtmlGenericControl("div");
            stepContent.Controls.Add(triggerLabelDiv);
            HtmlGenericControl triggerLabel = new HtmlGenericControl("label");
            triggerLabel.InnerText = String.Concat(eResApp.GetRes(Pref, 7221), " :");
            triggerLabelDiv.Controls.Add(triggerLabel);


            string chbxCssClass = "edaFormulaStep1Chbx";

            HtmlGenericControl chbxCreateDiv = new HtmlGenericControl("div");
            stepContent.Controls.Add(chbxCreateDiv);

            HtmlGenericControl radioCreate = new HtmlGenericControl("input");
            radioCreate.ID = (int)NotificationTriggerField.TRIGGER_ACTION + "_" + ((int)NotifConst.Action.CREATE).ToString();
            radioCreate.Attributes.Add("type", "radio");
            radioCreate.Attributes.Add("onclick", "oAutomation.setDbvVal( '" + stepContent.ID + "', " + ((int)NotifConst.Action.CREATE).ToString() + ");");

            // En création le champ déclencheur le 95
            if (targetDescID == _nTab + 95)
                radioCreate.Attributes.Add("checked", "");

            radioCreate.Attributes.Add("name", "triggerevent");
            radioCreate.Attributes.Add("class", chbxCssClass);
            if (!EditEnabled())
                radioCreate.Attributes.Add("disabled", "disabled");

            chbxCreateDiv.Controls.Add(radioCreate);

            HtmlGenericControl label = new HtmlGenericControl("label");
            label.Attributes.Add("for", radioCreate.ID);
            label.InnerHtml = eResApp.GetRes(Pref, 7222);
            chbxCreateDiv.Controls.Add(label);

            HtmlGenericControl chbxUpdateDiv = new HtmlGenericControl("div");
            stepContent.Controls.Add(chbxUpdateDiv);

            HtmlGenericControl radioUpdate = new HtmlGenericControl("input");
            radioUpdate.ID = (int)NotificationTriggerField.TRIGGER_ACTION + "_" + ((int)NotifConst.Action.UPDATE).ToString();
            radioUpdate.Attributes.Add("type", "radio");
            radioUpdate.Attributes.Add("onclick", "oAutomation.setDbvVal( '" + stepContent.ID + "', " + ((int)NotifConst.Action.UPDATE).ToString() + ");");

            // En modification le champ déclencheur le 96
            if (targetDescID == _nTab + 96)
                radioUpdate.Attributes.Add("checked", "");

            radioUpdate.Attributes.Add("name", "triggerevent");
            radioUpdate.Attributes.Add("class", chbxCssClass);
            if (!EditEnabled())
                radioUpdate.Attributes.Add("disabled", "disabled");
            chbxUpdateDiv.Controls.Add(radioUpdate);

            label = new HtmlGenericControl("label");
            label.Attributes.Add("for", radioUpdate.ID);
            label.InnerHtml = eResApp.GetRes(Pref, 7224);
            chbxUpdateDiv.Controls.Add(label);


            HtmlGenericControl updateFieldDiv = new HtmlGenericControl("div");
            stepContent.Controls.Add(updateFieldDiv);

            HtmlGenericControl radioUpdateFld = new HtmlGenericControl("input");
            radioUpdateFld.ID = (int)NotificationTriggerField.TRIGGER_ACTION + "_" + ((int)(NotifConst.Action.CREATE | NotifConst.Action.UPDATE)).ToString();
            radioUpdateFld.Attributes.Add("type", "radio");
            radioUpdateFld.Attributes.Add("onclick", "oAutomation.setDbvVal('" + stepContent.ID + "', " + ((int)(NotifConst.Action.CREATE | NotifConst.Action.UPDATE)).ToString() + ");");

            // En modification de rubrique le champ déclencheur est ni le 95 ni le 96
            if (EditEnabled() && (targetDescID != _nTab + 95) && (targetDescID != _nTab + 96))
                radioUpdateFld.Attributes.Add("checked", "");

            radioUpdateFld.Attributes.Add("name", "triggerevent");
            radioUpdateFld.Attributes.Add("class", chbxCssClass + " displaySelect");
            if (!EditEnabled())
                radioUpdateFld.Attributes.Add("disabled", "disabled");


            updateFieldDiv.Controls.Add(radioUpdateFld);

            label = new HtmlGenericControl("label");
            label.Attributes.Add("for", radioUpdateFld.ID);
            label.InnerHtml = eResApp.GetRes(Pref, 7225);
            updateFieldDiv.Controls.Add(label);

            if (EditEnabled())
            {

                HtmlGenericControl updateFieldSelect = new HtmlGenericControl("select");
                RenderSystemAttributes(NotificationTriggerField.TRIGGER_TARGET_DESCID, updateFieldSelect);


                updateFieldSelect.Attributes.Add("class", "edaFormulaStep1UpdDdl");
                updateFieldDiv.Controls.Add(updateFieldSelect);

                // rempli le select par des champs de la table cible
                FillWithTabFields(updateFieldSelect, targetDescID);

                updateFieldSelect.Attributes.Add("onchange", "setAttributeValue(this, 'dbv', this.value);");
            }

            #region Perf : ne pas tenir compte des droits

            Panel pPerf = new Panel();

            eCheckBoxCtrl chkPerf = new eCheckBoxCtrl(false, false);
            chkPerf.AddText(eResApp.GetRes(_ePref, 1894));
            chkPerf.AddClick();
            RenderSystemAttributes(NotificationTriggerField.BYPASS_RIGHTS, chkPerf);

            pPerf.Controls.Add(chkPerf);

            stepContent.Controls.Add(pPerf);

            #endregion

            _panelSections.Controls.Add(panelStep);
        }


        /// <summary>
        /// Rempli le select par des rubriques de la table
        /// </summary>
        /// <param name="select">HtmlGenericControl select</param>
        /// <param name="targetDescId">Descid</param>
        protected virtual void FillWithTabFields(HtmlGenericControl select, Int32 targetDescId) { }

        /// <summary>
        /// Génère la section des étapes
        /// </summary>
        private void CreateAutomationStepsSection()
        {
            Panel panelStep = new Panel();
            panelStep.CssClass = "divStep";

            // 2
            panelStep.Controls.Add(GenerateStepTitle("2", eResApp.GetRes(Pref, 7226), true));

            // Contenu du block dans tous les cas
            AppendAllCasesContent(panelStep);

            // end
            HtmlGenericControl divEnd = new HtmlGenericControl("div");
            divEnd.Attributes.Add("class", "edaEnd");
            panelStep.Controls.Add(divEnd);

            HtmlGenericControl spanEnd = new HtmlGenericControl("span");
            spanEnd.InnerText = eResApp.GetRes(Pref, 271).ToUpper();
            divEnd.Controls.Add(spanEnd);

            _panelSections.Controls.Add(panelStep);

        }

        /// <summary>
        /// Ajoute le contenu de tous les cas
        /// </summary>
        /// <param name="PanelContent">Panel</param>
        protected abstract void AppendAllCasesContent(Panel PanelContent);

        /// <summary>
        /// Méthode qui permet de génrére le footer 
        /// </summary>
        protected abstract void GenerateFooter();


        /// <summary>
        /// Génère la partie titre de l'étape
        /// </summary>
        /// <param name="sNum"></param>
        /// <param name="title"></param>
        /// <param name="active"></param>
        /// <returns></returns>

        private HtmlGenericControl GenerateStepTitle(String sNum, String title, Boolean active)
        {
            String classActive = (active) ? " active" : String.Empty;

            HtmlGenericControl step = new HtmlGenericControl("div");
            step.ID = String.Concat("stepTitle", sNum);
            step.Attributes.Add("class", "paramStep" + classActive);

            HtmlGenericControl span = new HtmlGenericControl();
            span.InnerText = sNum;
            span.Attributes.Add("class", "stepNum");
            step.Controls.Add(span);

            span = new HtmlGenericControl();
            span.InnerText = title;
            span.Attributes.Add("class", "stepTitle");
            step.Controls.Add(span);

            GenerateActionStep(step, sNum);

            return step;
        }

        /// <summary>
        /// Génère l'action sur une etape
        /// </summary>
        /// <param name="span"></param>
        /// <param name="num">Numéro de l'étape</param>
        protected virtual void GenerateActionStep(HtmlGenericControl span, string num) { }


        /// <summary>
        /// Construit l'id de l'element html pour le js
        /// </summary>
        /// <param name="fld"></param>
        /// <returns></returns>
        protected string GetClientId(NotificationTriggerField fld)
        {
            return string.Concat("fld_", (int)TableType.NOTIFICATION_TRIGGER, "_", (int)fld);
        }

        /// <summary>
        /// Construit l'id de l'element html pour le js
        /// </summary>
        /// <param name="fld"></param>
        /// <returns></returns>
        protected string GetClientId(NotificationTriggerResField fld)
        {
            return string.Concat("fld_", (int)TableType.NOTIFICATION_TRIGGER_RES, "_", (int)fld);
        }
    }
}