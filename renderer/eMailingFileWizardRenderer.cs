using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using EudoQuery;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{

    /// <summary>
    /// Renderer  à l'assistant Emailing depuis le mode fiche (campaign ou signet(++ ou cible etendu))
    /// </summary>

    public class eMailingFileWizardRenderer : eMailingWizardRenderer
    {

        /// <summary>
        /// Constructeur 
        /// </summary>
        /// <param name="pref">Preferences Utilisateur</param>
        /// <param name="height">Hauteur de la fenêtre</param>
        /// <param name="width">Largeur de la fenêtre</param>
        /// <param name="mailingType">Type de rapport</param>
        /// <param name="tab">Onglet en cours</param>
        private eMailingFileWizardRenderer(ePref pref, Int32 height, Int32 width, eMailing mailing, TypeMailing mailingType, Int32 tab, out Int32 _iWizardTotalStepNumber)
         : base(pref, height, width, mailing, mailingType, tab)
        {
            _iWizardTotalStepNumber = _iTotalStep;

        }


        /// <summary>
        /// Surcharge du BuildBodyStep standard.
        /// Génère les étapes du wizard de mailing
        /// </summary>
        /// <param name="step">Etape à générer</param>
        /// <returns></returns>
        protected override Panel BuildBodyStep(Int32 step)
        {
            //Assistant à 7 étapes
            if (_iTotalStep == 7)
                return base.BuildBodyStep(step);


            //Assistant à 6 étapes

            Panel pEditDiv = new Panel();
            pEditDiv.ID = String.Concat("editor_", step);
            pEditDiv.CssClass = step == 1 ? "editor-on" : "editor-off";
            Label lblFormat = new Label();

            // Boolean bIsNewCampaign = _mailing.Id <= 0;
            String stepName = String.Empty;
            switch (step)
            {

                case 1:
                    #region 1 Page
                    pEditDiv.Controls.Add(base.BuildSelectTemplatesPanel());
                    stepName = "template";
                    #endregion
                    break;
                case 2:
                    #region 2 Page
                    pEditDiv.Controls.Add(base.BuildMailBodyPanel());
                    stepName = "mail";
                    #endregion
                    break;
                case 3:
                    #region 3 Page CkEditor
                    if (
                 _ePref.ClientInfos.ClientOffer == 0
                 || eTools.IsMSBrowser
                 || !eFeaturesManager.IsFeatureAvailable(_ePref, eConst.XrmFeature.HTMLTemplateEditor)
             )
                    {
                        pEditDiv.Controls.Add(base.BuildInfosCampaignPanel());
                        stepName = "infosCampaign";
                    }
                    else
                        stepName = "mailck";
                    #endregion
                    break;
                case 4:
                    #region 4eme Page
                    if (_ePref.ClientInfos.ClientOffer == 0 || eTools.IsMSBrowser
                        || !eFeaturesManager.IsFeatureAvailable(_ePref, eConst.XrmFeature.HTMLTemplateEditor))
                    {
                        stepName = "controlBeforeSend";
                        pEditDiv.Controls.Add(this.BuildControlBeforeSendPanel());
                    }
                    else
                    {
                        pEditDiv.Controls.Add(this.BuildInfosCampaignPanel());
                        stepName = "infosCampaign";
                    }


                    #endregion
                    break;
                case 5:
                    #region 5eme Page
                    if (_ePref.ClientInfos.ClientOffer == 0 || eTools.IsMSBrowser
                        || !eFeaturesManager.IsFeatureAvailable(_ePref, eConst.XrmFeature.HTMLTemplateEditor))
                    {
                        pEditDiv.Controls.Add(this.BuildSendingOptionsPanel());
                        stepName = "send";
                    }
                    else
                    {
                        stepName = "controlBeforeSend";
                        pEditDiv.Controls.Add(this.BuildControlBeforeSendPanel());
                    }
                    #endregion
                    break;
                case 6:
                    #region 6eme Page
                    if (_ePref.ClientInfos.ClientOffer == 0 || eTools.IsMSBrowser
                        || !eFeaturesManager.IsFeatureAvailable(_ePref, eConst.XrmFeature.HTMLTemplateEditor))
                        return null;

                    pEditDiv.Controls.Add(this.BuildSendingOptionsPanel());
                    stepName = "send";
                    #endregion
                    break;
            }
            pEditDiv.Attributes.Add("stepName", stepName);
            return pEditDiv;
        }

        /// <summary>
        /// Construit le blocs de boutons d'étapes de la partie haute
        /// </summary>
        /// <param name="step">Numéro d'étape</param>
        /// <param name="isActive">étape active de l'assistant</param>
        /// <returns>Panel (div) de l'étape</returns>
        protected override Panel BuildStepDiv(Int32 step, Boolean isActive)
        {
            if (_iTotalStep == 7)
                return base.BuildStepDiv(step, isActive);

            Panel stepBloc = new Panel();
            stepBloc.ID = "step_" + step.ToString();
            Panel numberBloc = new Panel();
            numberBloc.ID = "txtnum_" + step.ToString();

            stepBloc.Attributes.Add("onclick", String.Concat("StepClick('", step, "');"));

            numberBloc.Controls.Add(new LiteralControl(step.ToString()));
            Label lbl = new Label();

            switch (step)
            {

                case 1:
                    lbl.Text = eResApp.GetRes(Pref, 6401);
                    break;
                case 2:
                    if (
                             _ePref.ClientInfos.ClientOffer == 0
                             || eTools.IsMSBrowser
                             || !eFeaturesManager.IsFeatureAvailable(_ePref, eConst.XrmFeature.HTMLTemplateEditor)
                            )
                        lbl.Text = eResApp.GetRes(Pref, 6402);
                    else
                        lbl.Text = eResApp.GetRes(Pref, 2227);
                    break;
                case 3:
                    if (
                _ePref.ClientInfos.ClientOffer == 0
                || eTools.IsMSBrowser
                || !eFeaturesManager.IsFeatureAvailable(_ePref, eConst.XrmFeature.HTMLTemplateEditor)
            ) //Dans ce cas, l'étape 3 est la "dernière"
                    {
                        lbl.Text = eResApp.GetRes(Pref, 2891);//6403
                    }
                    else
                        lbl.Text = eResApp.GetRes(Pref, 2226);//"Contenus et liens"
                    break;

                case 4:
                    if (_ePref.ClientInfos.ClientOffer == 0 || eTools.IsMSBrowser
                        || !eFeaturesManager.IsFeatureAvailable(_ePref, eConst.XrmFeature.HTMLTemplateEditor))
                        lbl.Text = eResApp.GetRes(Pref, 2884); //2882
                    else
                        lbl.Text = eResApp.GetRes(Pref, 2891);
                    break;
                case 5:
                    if (_ePref.ClientInfos.ClientOffer == 0 || eTools.IsMSBrowser
                        || !eFeaturesManager.IsFeatureAvailable(_ePref, eConst.XrmFeature.HTMLTemplateEditor))
                        lbl.Text = eResApp.GetRes(Pref, 6403);
                    else
                        lbl.Text = eResApp.GetRes(Pref, 2884);
                    break;
                case 6:
                    if (_ePref.ClientInfos.ClientOffer == 0 || eTools.IsMSBrowser
                        || !eFeaturesManager.IsFeatureAvailable(_ePref, eConst.XrmFeature.HTMLTemplateEditor))
                        return null;

                    lbl.Text = eResApp.GetRes(Pref, 6403);
                    break;
                default:
                    lbl.Text = String.Concat(eResApp.GetRes(Pref, 1617), " ", step);
                    break;
            }
            stepBloc.CssClass = isActive ? "state_grp-current" : "state_grp";// : _mailing == null ? "state_grp" : "state_grp-validated";
            stepBloc.Controls.Add(numberBloc);
            stepBloc.Controls.Add(lbl);


            return stepBloc;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nHeight"></param>
        /// <param name="nWidth"></param>
        /// <param name="mailingType"></param>
        /// <param name="nTab"></param>
        /// <returns></returns>
        public static eMailingFileWizardRenderer GetMailingFileWizardRenderer(ePref pref, int nHeight, int nWidth, eMailing mailing, TypeMailing mailingType, int nTab, out Int32 _iWizardTotalStepNumber)
        {
            return new eMailingFileWizardRenderer(pref, nHeight, nWidth, mailing, mailingType, nTab, out _iWizardTotalStepNumber);
        }
    }
}