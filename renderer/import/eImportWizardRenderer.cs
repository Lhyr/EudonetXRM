using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.import
{
    /// <summary>
    /// Class permettant la construction des étapes d'import
    /// </summary>
    public class eImportWizardRenderer : eBaseWizardRenderer
    {
        /// <summary>
        /// Constructeur du wizard avec les etapes
        /// </summary>
        /// <param name="pref">Préference utilisateur</param>
        /// <param name="importParams">Paramètre de l'assistant d'import</param>
        public eImportWizardRenderer(ePref pref, eImportWizardParam importParams)
        {
            this.Pref = pref;

            this._width = importParams.Width;
            this._height = importParams.Height;

            this.lstStep.Add(new WizStep(1, eResApp.GetRes(pref, 6713), eImportWizardStepFactory.GetStep(pref, 1, importParams).Init().Render)); // Import des données
            this.lstStep.Add(new WizStep(2, eResApp.GetRes(pref, 6343), eImportWizardStepFactory.GetEmptyStep().Init().Render));// Mise en correspondance
            this.lstStep.Add(new WizStep(3, eResApp.GetRes(pref, 444), eImportWizardStepFactory.GetEmptyStep().Init().Render));// eResApp.GetRes(pref, 8326) = Récapitulatif
            this.lstStep.Add(new WizStep(4, eResApp.GetRes(pref, 8326), eImportWizardStepFactory.GetEmptyStep().Init().Render)); // Récapitulatif
            this.lstStep.Add(new WizStep(5, eResApp.GetRes(pref, 8493), eImportWizardStepFactory.GetEmptyStep().Init().Render)); // Progression de l'import

            this._nbStep = lstStep.Count;
        }

        /// <summary>
        /// Construit le blocs de boutons d'étapes de la partie haute
        /// </summary>
        /// <param name="step">Numéro d'étape</param>
        /// <param name="isActive">étape active de l'assistant</param>
        /// <returns>Panel (div) de l'étape</returns>
        protected override Panel BuildHeaderStep(Int32 step, Boolean isActive)
        {

            if (!lstStep.Any(a => a.Step == step))
                throw new Exception("Etape du wizard invalide");

            Panel stepBloc = new Panel();
            stepBloc.ID = "step_" + step.ToString();

            Panel numberBloc = new Panel();
            numberBloc.ID = "txtnum_" + step.ToString();


            stepBloc.Attributes.Add("onclick", String.Concat("top.oImportWizard.Wizard.HeaderWizardClick('", step, "');"));
            numberBloc.Controls.Add(new LiteralControl(step.ToString()));

            Label lbl = new Label();
            lbl.Text = lstStep.Find(a => a.Step == step).Libelle;
            stepBloc.CssClass = isActive ? "state_grp-current" : "state_grp";
            stepBloc.Controls.Add(numberBloc);
            stepBloc.Controls.Add(lbl);

            return stepBloc;
        }
        /// <summary>
        /// Construit le javascript nécessaire au bon fonctionnement de l'assistant d'import
        /// </summary>     
        /// <returns>Code Javascript de la page.</returns>
        public override string GetInitJS()
        {
            // Chargement des onload des composants javascript

            String js = String.Concat(Environment.NewLine,
            "   var iCurrentStep = 1;", Environment.NewLine,
            "   var htmlTemplate = null;", Environment.NewLine,
            "   var htmlHeader = null;", Environment.NewLine,
            "   var htmlFooter = null;", Environment.NewLine,
            "   var iTotalSteps;", Environment.NewLine,
            "   function OnImportDoc() {", Environment.NewLine,
            "      initHeadEvents();", Environment.NewLine,
            "      iTotalSteps =", this.NB_TOTALSTEP, "; ", Environment.NewLine,
            "      Init('import');", Environment.NewLine,
            " }", Environment.NewLine);

            return js;
        }
    }
}