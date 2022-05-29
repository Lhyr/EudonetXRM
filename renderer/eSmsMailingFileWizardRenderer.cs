using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Com.Eudonet.Xrm
{

    /// <summary>
    /// Renderer  à l'assistant Emailing depuis le mode fiche (campaign ou signet(++ ou cible etendu))
    /// </summary>

    public class eSmsMailingFileWizardRenderer : eMailingWizardRenderer
    {
        #region constructor
        protected StringBuilder _strScriptBuilder = null;
        /// <summary>
        /// Constructeur 
        /// </summary>
        /// <param name="pref">Preferences Utilisateur</param>
        /// <param name="height">Hauteur de la fenêtre</param>
        /// <param name="width">Largeur de la fenêtre</param>
        /// <param name="mailingType">Type de rapport</param>
        /// <param name="mailing">Inforation concernant le smsing</param>
        /// <param name="_iWizardTotalStepNumber">Nombre d'etapes de l'assistant</param>
        /// <param name="tab">Onglet en cours</param>
        private eSmsMailingFileWizardRenderer(ePref pref, Int32 height, Int32 width, eMailing mailing, TypeMailing mailingType, Int32 tab, out Int32 _iWizardTotalStepNumber)
            : base(pref, height, width, mailing, mailingType, tab)
        {
            _iWizardTotalStepNumber = _iTotalStep;
            this._strScriptBuilder = new StringBuilder();
        }
        #endregion

        /// <summary>
        /// construit un bloc d'étapes
        /// </summary>
        /// <param name="wStep"></param>
        /// <returns></returns>
        protected override Panel BuildBodyStep(eWizardStep wStep)
        {


            Panel pEditDiv = new Panel();
            pEditDiv.ID = String.Concat("editor_", wStep.Order);
            Label lblFormat = new Label();


            switch (wStep.ID)
            {
                case "smsbody":
                    #region 1 - Sms Text
                    pEditDiv.CssClass = "editor-on";

                    Panel SMSBody = new Panel();
                    SMSBody.Attributes.Add("class", "edn--container");

                    HtmlGenericControl divSMSBody = new HtmlGenericControl("div");
                    divSMSBody.Attributes.Add("class", "edn--smsText");
                    SMSBody.Controls.Add(divSMSBody);

                    divSMSBody.Controls.Add(GetSmsBodyPanel());

                    pEditDiv.Controls.Add(SMSBody);

                    #endregion
                    break;


                case "cpgtyp":
                    #region 2 - Campaign Type
                    pEditDiv.CssClass = "editor-off";
                    Control CampaignTypePanel = new Panel();
                    pEditDiv.Controls.Add(CampaignTypePanel);
                    #endregion
                    break;


                case "sendingoption":
                    #region 3 - options envoi

                    pEditDiv.CssClass = "editor-off";
                    Panel sendingOption = new Panel();

                    HtmlGenericControl divSMSSelection = new HtmlGenericControl("div");
                    divSMSSelection.Attributes.Add("class", "edn--container");
                    sendingOption.Controls.Add(divSMSSelection);

                    HtmlGenericControl divSMSSending = new HtmlGenericControl("div");
                    divSMSSending.Attributes.Add("class", "edn--sending");
                    divSMSSelection.Controls.Add(divSMSSending);

                    #region radioButton
                    if (_eventStepEnabled)
                        divSMSSending.Controls.Add(BuildSendingOptionsPanel());


                    pEditDiv.Controls.Add(divSMSSelection);

                    RenderJavaScript(pEditDiv);
                    #endregion

                    HtmlGenericControl divChoiceTableSMS = new HtmlGenericControl("div");
                    divChoiceTableSMS.Attributes.Add("class", "block-SMS");
                    divSMSSending.Controls.Add(divChoiceTableSMS);

                    HtmlGenericControl labelChoiceTableEmail = new HtmlGenericControl("label");
                    divChoiceTableSMS.Controls.Add(labelChoiceTableEmail);
                    labelChoiceTableEmail.Attributes.Add("for", "name");
                    labelChoiceTableEmail.InnerHtml = eResApp.GetRes(Pref, 8756); // Choix table SMS

                    HtmlGenericControl divSelectRecipientTab = new HtmlGenericControl("div");
                    divSelectRecipientTab.Attributes.Add("class", "select-SMS");
                    divSMSSending.Controls.Add(divSelectRecipientTab);

                    //select 
                    HtmlSelect selectRecipientTab = new HtmlSelect();// HtmlGenericControl("select");  
                    selectRecipientTab.ID = "selectRecipientTab";
                    selectRecipientTab.Attributes.Add("class", "mailReply select-theme");

                    string err;
                    eTools.FillSMSFiles(Pref, selectRecipientTab, out err);
                    divSelectRecipientTab.Controls.Add(selectRecipientTab);

                    #endregion

                    break;
            }


            pEditDiv.Attributes.Add("stepName", wStep.ID);
            return pEditDiv;
        }

        /// <summary>
        /// Surcharge du BuildBodyStep standard.
        /// Génère les étapes du wizard de mailing
        /// </summary>
        /// <param name="step">Etape à générer</param>
        /// <returns></returns>
        protected override Panel BuildBodyStep(Int32 step)
        {

            //Assistant à 3 étapes
            Panel pEditDiv = new Panel();
            pEditDiv.ID = String.Concat("editor_", step);
            pEditDiv.CssClass = "editor-on";
            Label lblFormat = new Label();

            // Boolean bIsNewCampaign = _mailing.Id <= 0;
            String stepName = String.Empty;
            switch (step)
            {
                case 1:
                    #region 1 - Sms Text


                    Control bodyPanel = GetSmsBodyPanel();
                    pEditDiv.Controls.Add(bodyPanel);
                    /*
                    if (_eventStepEnabled)
                        bodyPanel.Controls.Add(BuildSendingOptionsPanel());
                    */
                    stepName = "mail";
                    #endregion
                    break;
                case 2:
                    #region 1 - Campaign Type


                    Control CampaignTypePanel = new Panel();
                    pEditDiv.Controls.Add(CampaignTypePanel);

                    stepName = "campaigntype";
                    #endregion
                    break;

                case 3:
                    #region option envoi
                    Panel sendingOption = new Panel();


                    HtmlSelect selectRecipientTab = new HtmlSelect();// HtmlGenericControl("select");  
                    selectRecipientTab.ID = "selectRecipientTab";
                    selectRecipientTab.Style.Add(HtmlTextWriterStyle.Width, "404px");
                    selectRecipientTab.Attributes.Add("class", "mailReply select-theme");

                    string err;
                    eTools.FillSMSFiles(Pref, selectRecipientTab, out err);
                    sendingOption.Controls.Add(selectRecipientTab);


                    if (_eventStepEnabled)
                        sendingOption.Controls.Add(BuildSendingOptionsPanel());


                    pEditDiv.Controls.Add(sendingOption);
                    #endregion
                    break;
            }
            pEditDiv.Attributes.Add("stepName", stepName);
            return pEditDiv;
        }

        /// <summary>
        /// Construit le contenu de l'edition de sms
        /// </summary>
        /// <returns></returns>
        private Control GetSmsBodyPanel()
        {
            return base.BuildMailBodyPanel();
        }

        /// <summary>
        /// Construit le blocs de boutons d'étapes de la partie haute
        /// </summary>
        /// <param name="step">Numéro d'étape</param>
        /// <param name="isActive">étape active de l'assistant</param>
        /// <returns>Panel (div) de l'étape</returns>
        protected override Panel BuildStepDiv(Int32 step, Boolean isActive)
        {
            return base.BuildStepDiv(step, isActive);
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
        public static eSmsMailingFileWizardRenderer GetSmsMailingFileWizardRenderer(ePref pref, int nHeight, int nWidth, eMailing mailing, TypeMailing mailingType, int nTab, out Int32 _iWizardTotalStepNumber)
        {
            return new eSmsMailingFileWizardRenderer(pref, nHeight, nWidth, mailing, mailingType, nTab, out _iWizardTotalStepNumber);
        }


        /// <summary>
        /// Bloc Option - Partie Envoi programmé
        /// </summary>
        /// <returns></returns>
        protected override Panel BuildSendingOptionsPanel()
        {

            Panel divschedule = new Panel();

            divschedule.Attributes.Add("class", "block--schedule");

            HtmlGenericControl labelsending = new HtmlGenericControl("h3");
            labelsending.InnerHtml = eResApp.GetRes(Pref, 8757); // Program your SMS campaign
            divschedule.Controls.Add(labelsending);

            HtmlGenericControl divFieldContainer = new HtmlGenericControl("div");
            divFieldContainer.Attributes.Add("class", "field-container");
            divschedule.Controls.Add(divFieldContainer);

            HtmlGenericControl labelSelect = new HtmlGenericControl("label");
            labelSelect.Attributes.Add("for", "name");
            labelSelect.InnerHtml = eResApp.GetRes(Pref, 8758); //Select the sending mode
            divFieldContainer.Controls.Add(labelSelect);

            //var z = BuildSendingOptionsPanel_DelayedMail();

            #region radio

            //div of radio
            HtmlGenericControl divfieldcontainerradio = new HtmlGenericControl("div");
            divfieldcontainerradio.Attributes.Add("class", "field-container ma-0");
            divschedule.Controls.Add(divfieldcontainerradio);

            //div of custom radio 
            HtmlGenericControl divradiocustom = new HtmlGenericControl("div");
            divradiocustom.Attributes.Add("Id", "radio--custom");
            divfieldcontainerradio.Controls.Add(divradiocustom);

            //div radio group
            HtmlGenericControl divradiogroup = new HtmlGenericControl("div");
            divradiogroup.Attributes.Add("class", "radio-title-group");
            divradiocustom.Controls.Add(divradiogroup);

            #region First radio Envoi Immédiat

            //div input container
            HtmlGenericControl divinputImmediat = new HtmlGenericControl("div");
            divinputImmediat.Attributes.Add("class", "input-container");
            divradiogroup.Controls.Add(divinputImmediat);

            //input 
            HtmlGenericControl inputimmediat = new HtmlGenericControl("input");
            inputimmediat.Attributes.Add("class", "radio-button");
            inputimmediat.Attributes.Add("Id", "immediateDispatch");
            inputimmediat.Attributes.Add("type", "radio");
            inputimmediat.Attributes.Add("name", "radio");
            inputimmediat.Attributes.Add("checked", "");
            inputimmediat.Attributes.Add("value", "Now");
            inputimmediat.Attributes.Add("onclick", "switchToContainer(this,event)");
            //inputimmediat.Attributes.Add("onclick", "oSmsing.OnSelectDelayed_Now();");
            divinputImmediat.Controls.Add(inputimmediat);

            // div icon/button
            HtmlGenericControl divradio = new HtmlGenericControl("div");
            divradio.Attributes.Add("class", "radio-title");
            divinputImmediat.Controls.Add(divradio);

            // div icon
            HtmlGenericControl divicon = new HtmlGenericControl("div");
            divicon.Attributes.Add("class", "icon walk-icon");
            divradio.Controls.Add(divicon);

            // href
            HtmlGenericControl linkicon = new HtmlGenericControl("i");
            linkicon.Attributes.Add("class", "icon-paper-plane-o");
            divicon.Controls.Add(linkicon);

            //label
            HtmlGenericControl labelimmediatsending = new HtmlGenericControl("label");
            labelimmediatsending.Attributes.Add("class", "radio-title-label");
            labelimmediatsending.Attributes.Add("for", "immediateDispatch");
            labelimmediatsending.InnerText = eResApp.GetRes(_ePref, 6409);
            divradio.Controls.Add(labelimmediatsending);

            bool bImmediateSending = Mailing.GetParamValue("immediateSending").Equals("1");
            MAILINGQUERYMODE rm = (MAILINGQUERYMODE)eLibTools.GetNum(Mailing.GetParamValue("RequestMode"));
            Boolean bReccurentSending = Mailing.GetParamValue("recurrentSending").Equals("1") && !_bOnHold;

            //div immediat
            HtmlGenericControl divfieldContainerContentImmediate = new HtmlGenericControl("div");
            divfieldContainerContentImmediate.Attributes.Add("class", "field-container--content immediateDispatch");
            //divfieldContainerContentImmediate.Attributes.Add("display", "block"); --- see in css
            divfieldContainerContentImmediate.Attributes.Add("for", "immediateDispatch");
            divschedule.Controls.Add(divfieldContainerContentImmediate);

            if (_eventStepEnabled && _bOnHold)
            {
                HtmlGenericControl divHoldSending = new HtmlGenericControl("div");
                divHoldSending.Attributes.Add("class", "field-container");
                divHoldSending.ID = "delayedMail_Now";
                divHoldSending.Attributes.Add("Name", "delayedMail");
                divschedule.Controls.Add(divHoldSending);

                HtmlGenericControl labelHoldsending = new HtmlGenericControl("label");
                labelHoldsending.InnerHtml = eResApp.GetRes(Pref, 2767); ;
                labelHoldsending.Attributes.Add("for", "Name");
                divHoldSending.Controls.Add(labelHoldsending);

                divfieldcontainerradio.Style.Add("display", "none");
                divFieldContainer.Style.Add("display", "none");
                divfieldContainerContentImmediate.Style.Add("display", "none");

            }


            #endregion


            #region second radio Envoi différé
            if (_eventStepEnabled)
            {
                // HtmlGenericControl liDelayedMailSendRecurrent = new HtmlGenericControl("div");
                //divFieldContainer.Controls.Add(liDelayedMailSendRecurrent);
                //liDelayedMailSendRecurrent.Attributes.Add("class", "li-opts-mailing");

                if (!_bOnHold)
                {
                    //div input container
                    HtmlGenericControl divinputrecurring = new HtmlGenericControl("div");
                    divinputrecurring.Attributes.Add("class", "input-container");
                    divradiogroup.Controls.Add(divinputrecurring);

                    //hiden input pour eventStepDescId
                    HtmlGenericControl inputEventDescHidden = new HtmlGenericControl("input");
                    inputEventDescHidden.Attributes.Add("name", "eventStepDescId");
                    inputEventDescHidden.Attributes.Add("id", "eventStepDescId");
                    inputEventDescHidden.Attributes.Add("type", "hidden");
                    inputEventDescHidden.Attributes.Add("value", _eventStepDescId.ToString());
                    divinputrecurring.Controls.Add(inputEventDescHidden);
                   


                    //input
                    HtmlGenericControl inputReccuring = new HtmlGenericControl("input");
                    inputReccuring.Attributes.Add("class", "radio-button");
                    inputReccuring.Attributes.Add("Id", "recurringSending");
                    inputReccuring.Attributes.Add("value", "recurrent");
                    inputReccuring.Attributes.Add("type", "radio");
                    inputReccuring.Attributes.Add("name", "radio");
                    //inputReccuring.Attributes.Add("onclick", "oSmsing.OnSelectDelayed_Recurrent(this);");

                    inputReccuring.Attributes.Add("onclick", "switchToContainer(this,event)");
                    divinputrecurring.Controls.Add(inputReccuring);

                    // div icon/button
                    HtmlGenericControl divradioReccuring = new HtmlGenericControl("div");
                    divradioReccuring.Attributes.Add("class", "radio-title");
                    divinputrecurring.Controls.Add(divradioReccuring);

                    // div icon
                    HtmlGenericControl diviconReccuring = new HtmlGenericControl("div");
                    diviconReccuring.Attributes.Add("class", "icon bike-icon");
                    divradioReccuring.Controls.Add(diviconReccuring);

                    // href
                    HtmlGenericControl linkiconReccuring = new HtmlGenericControl("i");
                    linkiconReccuring.Attributes.Add("class", "icon-clock-o");
                    diviconReccuring.Controls.Add(linkiconReccuring);

                    //label
                    HtmlGenericControl labelReccuringsending = new HtmlGenericControl("label");
                    labelReccuringsending.Attributes.Add("class", "radio-title-label");
                    labelReccuringsending.Attributes.Add("for", "recurringSending");
                    labelReccuringsending.InnerText = eResApp.GetRes(_ePref, 2707);
                    divradioReccuring.Controls.Add(labelReccuringsending);

                    #region Reccurent 
                    
                    // div reccurent
                    HtmlGenericControl divfieldContanerContentReccurentSending = new HtmlGenericControl("div");
                    divfieldContanerContentReccurentSending.Attributes.Add("class", "field-container--content recurringSending");
                    divfieldContanerContentReccurentSending.Attributes.Add("display", "none");
                    divfieldContanerContentReccurentSending.ID = "recurringSendingBlock";
                    divschedule.Controls.Add(divfieldContanerContentReccurentSending);

                    #region Envoi récurrent : fréquence

                    HtmlGenericControl divContainerPlanner = new HtmlGenericControl("div");
                    divContainerPlanner.Attributes.Add("class", "field-container");
                    divfieldContanerContentReccurentSending.Controls.Add(divContainerPlanner);

                    HtmlGenericControl labelReccurentSending = new HtmlGenericControl("label");
                    //labelReccurentSending.Attributes.Add("onclick", "oMailing.openScheduleParameter()");
                    labelReccurentSending.Attributes.Add("Id", "openschedule");
                    labelReccurentSending.Attributes.Add("class", "setting-planification");
                    divContainerPlanner.Controls.Add(labelReccurentSending);

                    HtmlGenericControl spanReccurentSending = new HtmlGenericControl("span");
                    spanReccurentSending.InnerHtml = eResApp.GetRes(_ePref, 6888);
                    spanReccurentSending.Attributes.Add("Id", "openschedule");
                    labelReccurentSending.Controls.Add(spanReccurentSending);

                    spanReccurentSending.Attributes.Add("onclick", "oSmsing.openScheduleParameter()");

                    HtmlGenericControl lnkScheduleInfo = new HtmlGenericControl("span");
                    lnkScheduleInfo.ID = "lnkScheduleInfo";
                    lnkScheduleInfo.InnerText = "";
                    lnkScheduleInfo.Style.Add(HtmlTextWriterStyle.Display, "none");
                    lnkScheduleInfo.Style.Add(HtmlTextWriterStyle.Color, "#9c9c9c");
                    lnkScheduleInfo.Style.Add(HtmlTextWriterStyle.FontStyle, "italic");
                    lnkScheduleInfo.Style.Add(HtmlTextWriterStyle.FontSize, "12px");
                    lnkScheduleInfo.Style.Add(HtmlTextWriterStyle.FontWeight, "300");
                    labelReccurentSending.Controls.Add(lnkScheduleInfo);

                    #endregion

                    #region radio palinification

                    HtmlGenericControl divContainerPlanner_Request = new HtmlGenericControl("div");
                    divContainerPlanner_Request.Attributes.Add("class", "field-container");
                    divfieldContanerContentReccurentSending.Controls.Add(divContainerPlanner_Request);

                    HtmlGenericControl radioContainerResultReccurentSending = new HtmlGenericControl("div");
                    radioContainerResultReccurentSending.Attributes.Add("class", "radio-container");
                    divContainerPlanner_Request.Controls.Add(radioContainerResultReccurentSending);

                    #region Envoi récurrent : classique

                    HtmlGenericControl divRecurrentRequestMode_normal = new HtmlGenericControl("div");
                    divRecurrentRequestMode_normal.Attributes.Add("class", "radio");
                    radioContainerResultReccurentSending.Controls.Add(divRecurrentRequestMode_normal);
                    //first radio
                    HtmlGenericControl firstradioReccurentSending = new HtmlGenericControl("input");
                    firstradioReccurentSending.Attributes.Add("id", "RequestMode_ReccurentAll");
                    firstradioReccurentSending.Attributes.Add("name", "RequestMode");
                    firstradioReccurentSending.Attributes.Add("type", "radio");

                    if (rm == MAILINGQUERYMODE.RECURRENT_ALL)
                        firstradioReccurentSending.Attributes.Add("checked", "");

                    firstradioReccurentSending.Attributes.Add("value", ((int)MAILINGQUERYMODE.RECURRENT_ALL).ToString());
                    //firstradioReccurentSending.Attributes.Add("onclick", "oMailing.OnSelectRequestMode(this)");
                    divRecurrentRequestMode_normal.Controls.Add(firstradioReccurentSending);

                    firstradioReccurentSending.Attributes.Add("onclick", "oSmsing.OnSelectRequestMode(this)");

                    HtmlGenericControl labelFirstRadioReccurentSending = new HtmlGenericControl("label");
                    labelFirstRadioReccurentSending.Attributes.Add("for", "RequestMode_ReccurentAll");
                    labelFirstRadioReccurentSending.Attributes.Add("class", "radio-label");
                    labelFirstRadioReccurentSending.InnerHtml = eResApp.GetRes(_ePref, 2706);
                    divRecurrentRequestMode_normal.Controls.Add(labelFirstRadioReccurentSending);

                    #endregion

                    #region Envoi récurrent : Rejouer la requête

                    HtmlGenericControl divRecurrentRequestMode_QueryRunAgain = new HtmlGenericControl("div");
                    divRecurrentRequestMode_QueryRunAgain.Attributes.Add("class", "radio");
                    radioContainerResultReccurentSending.Controls.Add(divRecurrentRequestMode_QueryRunAgain);
                    
                    //Second radio
                    HtmlGenericControl secondradioReccurentSending = new HtmlGenericControl("input");
                    secondradioReccurentSending.Attributes.Add("id", "RequestMode_ReccurentFilter");
                    secondradioReccurentSending.Attributes.Add("name", "RequestMode");
                    secondradioReccurentSending.Attributes.Add("type", "radio");

                    if (rm == MAILINGQUERYMODE.RECURRENT_FILTER)
                        secondradioReccurentSending.Attributes.Add("checked", "");

                    secondradioReccurentSending.Attributes.Add("onclick", "oSmsing.OnSelectRequestMode(this)");

                    secondradioReccurentSending.Attributes.Add("value", ((int)MAILINGQUERYMODE.RECURRENT_FILTER).ToString());
                    //secondradioReccurentSending.Attributes.Add("onclick", "oMailing.OnSelectRequestMode(this)");
                    divRecurrentRequestMode_QueryRunAgain.Controls.Add(secondradioReccurentSending);

                    HtmlGenericControl labelSecondRadioReccurentSending = new HtmlGenericControl("label");
                    labelSecondRadioReccurentSending.Attributes.Add("for", "RequestMode_ReccurentFilter");
                    labelSecondRadioReccurentSending.Attributes.Add("class", "radio-label");
                    labelSecondRadioReccurentSending.InnerHtml = eResApp.GetRes(_ePref, 2039);
                    divRecurrentRequestMode_QueryRunAgain.Controls.Add(labelSecondRadioReccurentSending);

                    HtmlGenericControl RecurrentRequestMode_Filter = new HtmlGenericControl("div");
                    RecurrentRequestMode_Filter.ID = "delayedMailRecurrent_Filter";
                    RecurrentRequestMode_Filter.Attributes.Add("class", "field-container");

                    if (rm != MAILINGQUERYMODE.RECURRENT_FILTER)
                        RecurrentRequestMode_Filter.Style.Add("display", "none");

                    divRecurrentRequestMode_QueryRunAgain.Controls.Add(RecurrentRequestMode_Filter);

                    HtmlGenericControl labellnkFilter = new HtmlGenericControl("label");
                    //labellnkFilter.Attributes.Add("onclick", "oMailing.openRecipientsFilterModal(" + _mailing.Tab + ")");
                    labellnkFilter.Attributes.Add("class", "setting-planification");
                    RecurrentRequestMode_Filter.Controls.Add(labellnkFilter);

                    HtmlGenericControl spanlnkFilter = new HtmlGenericControl("span");
                    spanlnkFilter.InnerHtml = eResApp.GetRes(_ePref, 8016);
                    labellnkFilter.Controls.Add(spanlnkFilter);

                    spanlnkFilter.Attributes.Add("onclick", "oSmsing.openRecipientsFilterModal(" + _mailing.Tab + ")");

                    HtmlGenericControl lnkFilterInfo = new HtmlGenericControl("label");
                    lnkFilterInfo.ID = "lnkFilterInfo";
                    lnkFilterInfo.InnerText = "";
                    lnkScheduleInfo.Style.Add(HtmlTextWriterStyle.Display, "none");
                    lnkFilterInfo.Style.Add(HtmlTextWriterStyle.Color, "#9c9c9c");
                    lnkFilterInfo.Style.Add(HtmlTextWriterStyle.FontStyle, "italic");
                    RecurrentRequestMode_Filter.Controls.Add(lnkFilterInfo);

                    #endregion

                    #endregion

                    #endregion
                }
            }

            #endregion

            #endregion

            return divschedule;
        }

        //On ajoute le script js
        private void AppendScript (StringBuilder strScriptBuilder)
        {
            if (!ReadonlyRenderer)
            {
                strScriptBuilder.Append(GetMergeAndTrackFields(this.Pref, this._tab, bGetOnlyTxtMergedField: true)).Append("; ");
                strScriptBuilder.Append(" var smsMergeFields = mailMergeFields; ");
            }
        }

        /// <summary>
        /// creation de la balise script
        /// </summary>
        
        private void RenderJavaScript (Panel pnlLeft)
        {
            // On crée un bloc JavaScript contenant les champs disponibles pour la fusion
            HtmlGenericControl javaScript = new HtmlGenericControl("script");
            javaScript.Attributes.Add("type", "text/javascript");
            javaScript.Attributes.Add("language", "javascript");

            AppendScript(this._strScriptBuilder);

            if (this._strScriptBuilder.Length > 0)
                javaScript.InnerHtml = this._strScriptBuilder.ToString();

            pnlLeft.Controls.Add(javaScript);
        }

    }
}