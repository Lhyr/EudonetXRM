using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
using System.Collections.ObjectModel;
using System.Linq;
using Com.Eudonet.Core.Model;
using System.Net.Mail;
using System.IO;
using Newtonsoft.Json;

namespace Com.Eudonet.Xrm
{

    /// <summary>
    /// Renderer décidé à l'assistant Emailing depuis le mode liste
    /// </summary>
    public class eMailingWizardRenderer : eRenderer
    {
        // Taile de l'écran
        /// <summary>Hauteur de l'écran</summary>
        private Int32 _iheight = eConst.DEFAULT_WINDOW_WIDTH;
        /// <summary>Largeur de l'écran</summary>
        private Int32 _iwidth = eConst.DEFAULT_WINDOW_HEIGHT;
        //Consenetement
        /// <summary>Consentement OptIn</summary>
        private bool _optInEnabled = true;
        /// <summary>Consentement NoConsentEnabled</summary>
        private bool _noConsentEnabled = true;
        /// <summary>Consentement OptOut</summary>
        private bool _optOutEnabled = false;
        /// <summary>Enlever les doublons</summary>
        private bool _noRemoveDoubleEnable = true;
        /// <summary>type de média et type de campagne</summary>
        private Int32 _mediaType, _campaignType;
        /// <summary> les status des Adresse emails de destinataire </summary>
        public AdressStatusParam _adressStatusParam = new AdressStatusParam();
        /// <summary>
        /// type de mailing depuis contacts, event, cible etendu ou invit
        /// </summary>
        private TypeMailing _mailingType;

        private IRightTreatment _oRightManager;





        /// <summary>
        /// Nombre d'étape dans le wizard
        /// </summary>
        protected Int32 _iTotalStep;


        /// <summary>
        /// Liste des étapes du wizard
        /// </summary>
        public List<eWizardStep> LstWizardStep = new List<eWizardStep>();



        /// <summary>
        /// Fiche de la campagne
        /// </summary>
        protected eFile _efCampaignFile;

        private Boolean _bFromBkm = false;
        private String _strAddressFileLabel = String.Empty;

        /// <summary> Objet metier de l'emailing </summary>
        protected eMailing _mailing = null;


        /// <summary>Table de liaison</summary>
        protected Int32 _iTab;

        /// <summary>
        /// Objet ,étier
        /// </summary>
        protected eMailingWizard _mailingWizard;

        private IDictionary<eLibConst.CONFIGADV, string> _dicEmailAdvConfig;
        private IDictionary<eLibConst.PREFADV, string> _dicPrefAdv;

        private Int32 _defaultMailTplID;

        /// <summary>
        /// Constante temporaire pour activer ou non les catégories de désinscription RGPD
        /// </summary>
        private bool _useNewUnsubscribeMethod = false;

        /// <summary>
        /// Indique si une table EventStep a été créé pour la table parente, et donc si les envois récurrents peuvent être utilisés
        /// </summary>
        protected bool _eventStepEnabled = false;

        /// <summary>
        /// indique si le marketting automation est activé.
        /// </summary>
        public bool IsEventStepEnabled
        {
            get
            {
                return _eventStepEnabled;
            }
        }

        /// <summary>
        /// La fiche event de l'emailing a le marketting automation activé et est en pause
        /// </summary>
        protected bool _bOnHold = false;

        /// <summary>
        /// DescId de la table EventStep
        /// </summary>
        protected int _eventStepDescId = 0;

        /// <summary>
        /// Objet de rendu
        /// </summary>
        protected eRenderer _efRend = null;

        /// <summary>Objet eMailing associé au renderer s'il y a lieu</summary>
        public eMailing Mailing
        {
            get { return _mailing; }
            set { _mailing = value; }
        }


        #region Constructeurs

        /// <summary>
        /// Constructeur protegé
        /// </summary>
        /// <param name="ePref">Preferences Utilisateur</param>
        /// <param name="height">Hauteur de la fenêtre</param>
        /// <param name="width">Largeur de la fenêtre</param>
        /// <param name="mailingType">Type de rapport</param>
        /// <param name="tab">Onglet en cours</param>
        protected eMailingWizardRenderer(ePref ePref, Int32 height, Int32 width, eMailing mailing, TypeMailing mailingType, Int32 tab)
        {
            _iheight = height;
            _iwidth = width;
            Pref = ePref;
            _mailingType = mailingType;
            _tab = tab;
            if (_tab == TableType.ADR.GetHashCode())
                _iTab = TableType.PP.GetHashCode();
            else
                _iTab = _tab;
            _mailing = mailing;

            _mailingWizard = new eMailingWizard(Pref, _tab, _mailing, mailingType, _bFromBkm);

            if (!String.IsNullOrEmpty(_mailingWizard.Error))
                this._sErrorMsg = _mailingWizard.Error;

            //if (_mailingWizard.CurrentListCount == 0 && _mailingWizard.CurrentCheckedListCount == 0 && TypeMailing.MAILING_FROM_CAMPAIGN != mailingType && TypeMailing.MAILING_FOR_MARKETING_AUTOMATION != mailingType)
            //{
            //    this._sErrorMsg = "eMailingWizardRenderer . Pas de destinataires sélectionnés";
            //    this._nErrorNumber = QueryErrorType.ERROR_NUM_NO_RECIPIENTS_FOUND;
            //}

            LstWizardStep = _mailingWizard.GetWizardStep(this);
            if (Mailing.MailingType == TypeMailing.SMS_MAILING_FROM_BKM)
            {
                _iTotalStep = LstWizardStep.Count;
            }
            else
                _iTotalStep = _mailingWizard.GetTotalSteps(this._mailingType);



            _rType = RENDERERTYPE.MailingWizard;

            _oRightManager = new eRightMailTemplate(ePref);
        }

        #endregion

        #region Méthodes Statiques pour le rendu

        /// <summary>
        /// Génère un renderer paramétrés pour l'assistant Emailing et le retourne
        /// </summary>
        /// <param name="ePref">Préférences de l'utilisateur</param>
        /// <param name="height">hauteur de l'interface</param>
        /// <param name="width">largeur de l'interface</param>
        /// <param name="mailingType">Type de rapport pour l'assistant</param>
        /// <param name="tab">Fichier d'origine</param>
        /// <returns>Renderer contenant l'interface graphique de l'assistant</returns>
        public static eMailingWizardRenderer GetMailingWizardRenderer(ePref ePref, Int32 height, Int32 width, eMailing mailing, TypeMailing mailingType, Int32 tab)
        {
            return new eMailingWizardRenderer(ePref, height, width, mailing, mailingType, tab);
        }

        /// <summary>
        /// Construit le tableau Javascript de paramètres de l'objet Mailing.
        /// </summary>
        /// <param name="mailing">Objet eMailing</param>
        /// <returns>Chaine de construction de la variable javascript</returns>
        public String BuildJavascriptMailingParams(eMailing mailing = null)
        {
            StringBuilder javascriptString = new StringBuilder(" paramMailingArray = ");

            if (mailing == null)
                javascriptString.Append("{};");
            else
                javascriptString.Append(eTools.JavaScriptSerialize(mailing.MailingParams)).Append(";");


            return javascriptString.ToString();
        }


        #endregion

        #region Méthodes héritées de eRenderer, process de création du renderer

        /// <summary>
        /// Initialisation du renderer en cas d'ajout de nouveau mailing
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            //Chargement de configadv pour les options spécial (tracking externalisé...)
            _dicEmailAdvConfig = eLibTools.GetConfigAdvValues(this.Pref,
                new HashSet<eLibConst.CONFIGADV> {
                    eLibConst.CONFIGADV.MAILINGSENDTYPE,
                    eLibConst.CONFIGADV.EXTERNAL_TRACKING_ENABLED,
                    eLibConst.CONFIGADV.EXTERNAL_TRACKING_INTERNAL_URL,
                    eLibConst.CONFIGADV.EXTERNAL_TRACKING_SERIAL,
                    eLibConst.CONFIGADV.EXTERNAL_TRACKING_URL,
                    eLibConst.CONFIGADV.EXTERNAL_TRACKING_WS_PATH,
                    eLibConst.CONFIGADV.EXTERNAL_TRACKING_XRM_PATH,
                    eLibConst.CONFIGADV.USE_NEW_UNSUBSCRIBE_METHOD
            });

            // Récupération de l'ID du modèle par défaut
            _dicPrefAdv = eLibTools.GetPrefAdvValues(this.Pref,
                        new HashSet<eLibConst.PREFADV>() {
                            eLibConst.PREFADV.DEFAULT_EMAILINGTEMPLATE
                        }, this.Pref.UserId, _tab
                    );
            String valueDefaultTplID = String.Empty;
            _dicPrefAdv.TryGetValue(eLibConst.PREFADV.DEFAULT_EMAILINGTEMPLATE, out valueDefaultTplID);
            _defaultMailTplID = eLibTools.GetNum(valueDefaultTplID);

            //Demande #59961 - Suppression des annexes non rattachées
            if (_mailing?.Id == 0)
                DeleteUnlinkedPJs();

            string useNewUnsubscribeMethodValue = String.Empty;
            if (_dicEmailAdvConfig.TryGetValue(eLibConst.CONFIGADV.USE_NEW_UNSUBSCRIBE_METHOD, out useNewUnsubscribeMethodValue) && useNewUnsubscribeMethodValue == "1")
                _useNewUnsubscribeMethod = true;

            //Fonction envoie récurrent disponible uniquement depuis une cible étendue ou ++ => TypeMailing.MAILING_FROM_BKM
            //ou lorsqu'on ré-ouvre une campagne déjà créée => TypeMailing.MAILING_FROM_CAMPAIGN
            if (_mailingWizard.Tab != 0 && (
                _mailingType == TypeMailing.MAILING_FROM_BKM
                || _mailingType == TypeMailing.MAILING_FROM_CAMPAIGN
                || _mailingType == TypeMailing.SMS_MAILING_FROM_BKM
                )
                && eFeaturesManager.IsFeatureAvailable(this.Pref, eConst.XrmFeature.AdminEventStep))
            {
                eudoDAL dal = eLibTools.GetEudoDAL(this.Pref);
                try
                {
                    dal.OpenDatabase();
                    TableLite tabInfos = new TableLite(_mailingWizard.Tab);
                    string sError;
                    if (!tabInfos.ExternalLoadInfo(dal, out sError))
                        throw new Exception(String.Format("eMailingWizardRenderer.Init() - Erreur lors du chargement des infos de la table {0} : {1}", _mailingWizard.Tab, sError), tabInfos.InnerException);

                    if (tabInfos.TabType == TableType.TEMPLATE)
                    {
                        DescAdvDataSet descAdv = new DescAdvDataSet();
                        descAdv.LoadAdvParams(dal, new int[] { _mailingWizard.InterEventNum });

                        _eventStepEnabled = descAdv.GetAdvInfoValue(_mailingWizard.InterEventNum, DESCADV_PARAMETER.EVENT_STEP_ENABLED, "0") == "1";
                        Int32.TryParse(descAdv.GetAdvInfoValue(_mailingWizard.InterEventNum, DESCADV_PARAMETER.EVENT_STEP_DESCID, "0"), out _eventStepDescId);


                        //si la campagne est en pause, on n'autorise pas les actions différé/récurrente
                        if (_eventStepEnabled && _mailing != null && _mailing.ParentTab > 0)
                        {
                            eFile objFile = eFileForBkm.CreateFileForBkmBar(_ePref, _mailing.ParentTab, _mailing.ParentFileId);
                            if (objFile.Record != null)
                                _bOnHold = ((eRecordEvent)objFile.Record).OnBreak == 1;

                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    dal.CloseDatabase();
                }
            }



            int nType = eConst.eFileType.FILE_CREA.GetHashCode();
            string strMailTo = String.Empty;
            bool bMailForward = false;
            bool bMailDraft = false;
            bool bCkeditor = false;

            // Paramètres supplémentaires transmis au constructeur de rendu
            ExtendedDictionary<String, Object> param = new ExtendedDictionary<String, Object>();
            eFileTools.eFileContext ef = new eFileTools.eFileContext(new eFileTools.eParentFileId(), Pref.User, TableType.CAMPAIGN.GetHashCode(), 0);
            param.Add("filecontext", ef);
            param.Add("mailforward", bMailForward);
            param.Add("maildraft", bMailDraft);
            param.Add("mailto", strMailTo);
            param.Add("ntabfrom", this._tab);
            param.Add("useckeditor", bCkeditor ? "1" : "0");

            _efRend = eRendererFactory.CreateEditMailingRenderer(Pref, TableType.CAMPAIGN.GetHashCode(), Mailing, _iwidth, _iheight, strMailTo, bMailForward, bMailDraft, param);
            // #48 903 - Si une des rubriques du fichier E-mail n'est pas accessible en écriture, on le remonte à l'utilisateur
            if (_efRend.ErrorNumber == QueryErrorType.ERROR_NUM_MAIL_FILE_NOT_FOUND)
            {
                SetError(
                    _efRend.ErrorNumber,
                    eResApp.GetRes(Pref, 2020).Replace("<FIELD>", _efRend.ErrorMsg) //Vous n'avez pas les droits suffisants pour effectuer la mise à jour de la rubrique <FIELD>
                );
                return false;
            }
            else if (!String.IsNullOrEmpty(_efRend.ErrorMsg) || _efRend.InnerException != null)
            {
                SetError(QueryErrorType.ERROR_NUM_FIELD_NOT_FOUND, String.Concat("Erreur Renderer de MailingWizardRenderer, BuildMailBodyPanel : ", _efRend.ErrorMsg, (_efRend.InnerException != null ? _efRend.InnerException.StackTrace : String.Empty)));
                return false;
            }

            return true;
        }

        private void DeleteUnlinkedPJs()
        {
            List<int> listTab = new List<int>()
            {
                (int)TableType.CAMPAIGN,
                (int)TableType.MAIL_TEMPLATE
            };

            foreach (int tab in listTab)
            {
                string error;
                List<int> listPJiDs = ePJ.GetUnlinkedPJiDs(this.Pref, tab, out error);

                if (!String.IsNullOrEmpty(error))
                    throw new Exception(String.Concat("DeleteUnlinkedPJs : ", error));

                foreach (int pjId in listPJiDs)
                {
                    String fileName = "";
                    int fileType;
                    eErrorContainer errorContainer;

                    ePJTraitements.DeletefromPj(this.Pref, pjId, 0, tab, out fileName, out fileType, out errorContainer);

                    if (errorContainer != null)
                    {
                        if (errorContainer.DebugMsg.Length > 0)
                            throw new Exception(String.Concat("DeleteUnlinkedPJs : ", errorContainer.DebugMsg));

                        if (errorContainer.Msg.Length > 0)
                            throw new Exception(String.Concat("DeleteUnlinkedPJs : ", errorContainer.Msg));
                    }

                    // Suppression du fichier sur le disque
                    if (fileName.Length > 0 && ePJ.IsPhysicalFile(fileType))
                    {
                        error = ePJTraitements.DeletePjFromDisk(this.Pref.GetBaseName, fileName);

                        if (!String.IsNullOrEmpty(error))
                            throw new Exception(String.Concat("DeleteUnlinkedPJs : ", error));
                    }
                }
            }
        }

        /// <summary>
        /// Construit le corps de l'assistant, composé d'un div de param et d'un div par étape d'assistant
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {

            Panel Header;
            Panel Editor;



            //
            if (_mailingType == TypeMailing.SMS_MAILING_FROM_BKM)
            {

                Header = this.BuildHeaderFromStepList();
                Editor = this.BuildBodyFromStepList();

            }
            else
            {
                Header = this.BuildHeader();
                Editor = this.BuildBody();
            }

            this.PgContainer.Controls.Add(Header);
            this.PgContainer.Controls.Add(Editor);

            return true;
        }
        #endregion

        #region Construction du header de la page

        /// <summary>
        /// Construit la liste des étapes a partir de la liste d'étapes
        /// </summary>
        /// <returns></returns>
        protected virtual Panel BuildHeaderFromStepList()
        {
            Panel header = new Panel();
            header.ID = "wizardheader";
            header.CssClass = "wizardheader";
            Panel stepGroup = new Panel();
            stepGroup.CssClass = String.Concat("states_placement", _rType == RENDERERTYPE.ChartWizard ? " stpPlcmtChrt" : "");
            foreach (eWizardStep wStep in LstWizardStep)
            {
                if (wStep.IsDisabled)
                    continue;

                var myResult = BuildStepDiv(wStep);
                if (myResult != null)
                {
                    stepGroup.Controls.Add(myResult);
                    if (wStep.Order < LstWizardStep.Max(z => z.Order))
                    {
                        stepGroup.Controls.Add(BuildSeparatorDiv());
                    }
                }
            }
            header.Controls.Add(stepGroup);
            return header;
        }

        /// <summary>
        /// Construit les blocs d'étapes a partir de la liste d'étapes
        /// </summary>
        /// <param name="wStep"></param>
        /// <returns></returns>
        protected virtual Panel BuildBodyStep(eWizardStep wStep)
        {

            int step = wStep.Order;

            Panel pEditDiv = new Panel();
            pEditDiv.ID = String.Concat("editor_", step);
            pEditDiv.CssClass = step == 1 ? "editor-on" : "editor-off";
            Label lblFormat = new Label();

            String stepName = String.Empty;

            switch (step)
            {
                case 1:
                    #region Première Page
                    pEditDiv.Controls.Add(this.BuildSelectFieldsPanel());
                    stepName = "recipient";
                    #endregion
                    break;
                case 2:
                    #region Seconde Page
                    pEditDiv.Controls.Add(this.BuildSelectTemplatesPanel());
                    stepName = "template";
                    #endregion
                    break;
                case 3:
                    #region Troisième Page
                    pEditDiv.Controls.Add(this.BuildMailBodyPanel());
                    stepName = "mail";
                    #endregion
                    break;
                case 4:
                    #region Quatrième Page
                    if (
                            _ePref.ClientInfos.ClientOffer == 0
                            || eTools.IsMSBrowser
                            || !eFeaturesManager.IsFeatureAvailable(_ePref, eConst.XrmFeature.HTMLTemplateEditor)
                        )
                    {
                        pEditDiv.Controls.Add(this.BuildInfosCampaignPanel());
                        stepName = "infosCampaign";

                    }
                    else
                        stepName = "mailck";
                    #endregion
                    break;
                case 5:
                    #region cinquieme Page
                    if (_ePref.ClientInfos.ClientOffer == 0 || eTools.IsMSBrowser
                        || !eFeaturesManager.IsFeatureAvailable(_ePref, eConst.XrmFeature.HTMLTemplateEditor))
                    {
                        pEditDiv.Controls.Add(this.BuildControlBeforeSendPanel());
                        stepName = "controlBeforeSend";
                    }
                    else
                    {
                        pEditDiv.Controls.Add(this.BuildInfosCampaignPanel());
                        stepName = "infosCampaign";
                    }
                    #endregion
                    break;
                case 6:
                    #region Sixième Page
                    if (_ePref.ClientInfos.ClientOffer == 0 || eTools.IsMSBrowser
                         || !eFeaturesManager.IsFeatureAvailable(_ePref, eConst.XrmFeature.HTMLTemplateEditor))
                    {
                        pEditDiv.Controls.Add(this.BuildSendingOptionsPanel());
                        stepName = "send";
                    }
                    else
                    {
                        pEditDiv.Controls.Add(this.BuildControlBeforeSendPanel());
                        stepName = "controlBeforeSend";
                    }
                    #endregion
                    break;
                case 7:
                    #region Septième Page
                    if (_ePref.ClientInfos.ClientOffer == 0 || eTools.IsMSBrowser)
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
        /// Constuit la partie Haute de l'assistant Emailing
        /// Contenant les boutons et libellés des différentes étapes.
        /// </summary>
        /// <returns>Div conteneur de la partie haute de l'assistant</returns>
        private Panel BuildHeader()
        {
            Panel header = new Panel();
            header.ID = "wizardheader";
            header.CssClass = "wizardheader";
            Panel stepGroup = new Panel();
            stepGroup.CssClass = String.Concat("states_placement", _rType == RENDERERTYPE.ChartWizard ? " stpPlcmtChrt" : "");
            //stepGroup.CssClass = "states_placement";
            Int32 nActiveStep = 1;

            int nRealStep = _iTotalStep;
            if (
                            _ePref.ClientInfos.ClientOffer == 0
                            || eTools.IsMSBrowser
                            || !eFeaturesManager.IsFeatureAvailable(_ePref, eConst.XrmFeature.HTMLTemplateEditor)
                        )
            {
                nRealStep--;

            }

            for (Int32 i = 1; i <= _iTotalStep; i++)
            {

                var myResult = BuildStepDiv(i, i == nActiveStep);
                if (myResult != null)
                {
                    stepGroup.Controls.Add(myResult);
                    if (i < nRealStep)
                    {
                        stepGroup.Controls.Add(BuildSeparatorDiv());
                    }
                }
            }
            header.Controls.Add(stepGroup);
            return header;
        }


        /// <summary>
        /// retourne un panel de choix d'étape 
        /// </summary>
        /// <param name="ws"></param>
        /// <returns></returns>
        protected virtual Panel BuildStepDiv(eWizardStep ws)
        {

            int step = ws.Order;

            Panel stepBloc = new Panel();
            stepBloc.ID = "step_" + step.ToString();
            Panel numberBloc = new Panel();
            numberBloc.ID = "txtnum_" + step.ToString();

            stepBloc.Attributes.Add("onclick", String.Concat("StepClick('", step, "');"));

            numberBloc.Controls.Add(new LiteralControl(step.ToString()));
            Label lbl = new Label();
            lbl.Text = ws.Label;

            stepBloc.CssClass = ws.IsActive ? "state_grp-current" : "state_grp";// : _mailing == null ? "state_grp" : "state_grp-validated";
            stepBloc.Controls.Add(numberBloc);
            stepBloc.Controls.Add(lbl);


            return stepBloc;
        }

        /// <summary>
        /// Construit le blocs de boutons d'étapes de la partie haute
        /// </summary>
        /// <param name="step">Numéro d'étape</param>
        /// <param name="isActive">étape active de l'assistant</param>
        /// <returns>Panel (div) de l'étape</returns>
        protected virtual Panel BuildStepDiv(Int32 step, Boolean isActive)
        {
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
                    lbl.Text = eResApp.GetRes(Pref, 6400);
                    break;
                case 2:
                    lbl.Text = eResApp.GetRes(Pref, 6401);
                    break;
                case 3:
                    if (
         _ePref.ClientInfos.ClientOffer == 0
         || eTools.IsMSBrowser
         || !eFeaturesManager.IsFeatureAvailable(_ePref, eConst.XrmFeature.HTMLTemplateEditor)
        )
                        lbl.Text = eResApp.GetRes(Pref, 6402);
                    else
                        lbl.Text = eResApp.GetRes(Pref, 2227);
                    break;
                case 4:
                    if (
                            _ePref.ClientInfos.ClientOffer == 0
                            || eTools.IsMSBrowser
                            || !eFeaturesManager.IsFeatureAvailable(_ePref, eConst.XrmFeature.HTMLTemplateEditor)
                        )
                        lbl.Text = eResApp.GetRes(Pref, 2891);
                    else
                        lbl.Text = eResApp.GetRes(Pref, 2226);//"Contenus et liens"
                    break;
                case 5:
                    if (_ePref.ClientInfos.ClientOffer == 0 || eTools.IsMSBrowser
                            || !eFeaturesManager.IsFeatureAvailable(_ePref, eConst.XrmFeature.HTMLTemplateEditor))
                        lbl.Text = eResApp.GetRes(Pref, 2884);
                    else
                        lbl.Text = eResApp.GetRes(Pref, 2891);
                    break;
                case 6:
                    if (_ePref.ClientInfos.ClientOffer == 0 || eTools.IsMSBrowser
                            || !eFeaturesManager.IsFeatureAvailable(_ePref, eConst.XrmFeature.HTMLTemplateEditor))
                        lbl.Text = eResApp.GetRes(Pref, 6403);
                    else
                        lbl.Text = eResApp.GetRes(Pref, 2884);
                    break;
                case 7:
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
        /// Construit le bloc de séparation entre deux boutons d'étape.
        /// </summary>
        /// <returns>Panel (div) de Séparation entre deux étapes</returns>
        private Panel BuildSeparatorDiv()
        {
            Panel sepBloc = new Panel();
            sepBloc.CssClass = "state_sep";
            return sepBloc;
        }
        #endregion

        #region construction du body

        #region Bloc de construction principal
        /// <summary>
        /// Construit le div englobant le contenu de l'éditeur et y ajoute le contenu
        /// </summary>
        private Panel BuildBody()
        {
            Panel wizardBody = new Panel();
            wizardBody.ID = "wizardbody";
            wizardBody.CssClass = "wizardbody wizardbodyemailing";

            //Dès qu'il y a une erreur, on arrete la  construction de l'assistant
            for (int i = 1; i <= this._iTotalStep && ErrorMsg.Length == 0; i++)
            {
                var res = BuildBodyStep(i);
                if (res != null)
                    wizardBody.Controls.Add(res);
            }


            return wizardBody;
        }


        private Panel BuildBodyFromStepList()
        {

            Panel wizardBody = new Panel();
            wizardBody.ID = "wizardbody";
            wizardBody.CssClass = "wizardbody wizardbodyemailing";


            foreach (eWizardStep ws in LstWizardStep)
            {
                var res = BuildBodyStep(ws);
                if (res != null)
                    wizardBody.Controls.Add(res);
            }

            return wizardBody;
        }




        /// <summary>
        /// Construit le bloc div d'une étape donnée de l'assitant et le retourne
        /// </summary>
        /// <param name="step">Numéro d'étape de l'assistant</param>
        /// <returns>Panel(div) de l'étape demandée</returns>
        protected virtual Panel BuildBodyStep(Int32 step)
        {
            Panel pEditDiv = new Panel();
            pEditDiv.ID = String.Concat("editor_", step);
            pEditDiv.CssClass = step == 1 ? "editor-on" : "editor-off";
            Label lblFormat = new Label();

            String stepName = String.Empty;

            switch (step)
            {
                case 1:
                    #region Première Page
                    pEditDiv.Controls.Add(this.BuildSelectFieldsPanel());
                    stepName = "recipient";
                    #endregion
                    break;
                case 2:
                    #region Seconde Page
                    pEditDiv.Controls.Add(this.BuildSelectTemplatesPanel());
                    stepName = "template";
                    #endregion
                    break;
                case 3:
                    #region Troisième Page
                    pEditDiv.Controls.Add(this.BuildMailBodyPanel());
                    stepName = "mail";
                    #endregion
                    break;
                case 4:
                    #region Quatrième Page
                    if (
                            _ePref.ClientInfos.ClientOffer == 0
                            || eTools.IsMSBrowser
                            || !eFeaturesManager.IsFeatureAvailable(_ePref, eConst.XrmFeature.HTMLTemplateEditor)
                        )
                    {
                        pEditDiv.Controls.Add(this.BuildInfosCampaignPanel());
                        stepName = "infosCampaign";

                    }
                    else
                        stepName = "mailck";
                    #endregion
                    break;
                case 5:
                    #region cinquieme Page
                    if (_ePref.ClientInfos.ClientOffer == 0 || eTools.IsMSBrowser
                        || !eFeaturesManager.IsFeatureAvailable(_ePref, eConst.XrmFeature.HTMLTemplateEditor))
                    {
                        pEditDiv.Controls.Add(this.BuildControlBeforeSendPanel());
                        stepName = "controlBeforeSend";
                    }
                    else
                    {
                        pEditDiv.Controls.Add(this.BuildInfosCampaignPanel());
                        stepName = "infosCampaign";
                    }
                    #endregion
                    break;
                case 6:
                    #region Sixième Page
                    if (_ePref.ClientInfos.ClientOffer == 0 || eTools.IsMSBrowser
                         || !eFeaturesManager.IsFeatureAvailable(_ePref, eConst.XrmFeature.HTMLTemplateEditor))
                    {
                        pEditDiv.Controls.Add(this.BuildSendingOptionsPanel());
                        stepName = "send";
                    }
                    else
                    {
                        pEditDiv.Controls.Add(this.BuildControlBeforeSendPanel());
                        stepName = "controlBeforeSend";
                    }
                    #endregion
                    break;
                case 7:
                    #region Septième Page
                    if (_ePref.ClientInfos.ClientOffer == 0 || eTools.IsMSBrowser)
                        return null;

                    pEditDiv.Controls.Add(this.BuildSendingOptionsPanel());
                    stepName = "send";
                    #endregion
                    break;

            }

            pEditDiv.Attributes.Add("stepName", stepName);
            return pEditDiv;
        }
        #endregion

        #region blocs de construction annexes

        /// <summary>
        /// Construit le corps de page de l'étape Options d’envoi
        ///   Etape 4
        /// </summary>
        /// <returns>Panel de type DIV contenant le code HTML de l'étape</returns>
        protected virtual Panel BuildSendingOptionsPanel()
        {
            Panel panelSchedule = new Panel();
            panelSchedule.CssClass = "edn--container";

            HtmlGenericControl divsending = new HtmlGenericControl("div");
            divsending.Attributes.Add("Id", "edn--envoi");
            panelSchedule.Controls.Add(divsending);

            if(_mailingType == TypeMailing.MAILING_FOR_MARKETING_AUTOMATION)
            {
                divsending.Controls.Add(BuildSendingOptionsPanel_MarketingAutomation());
            }
            else
            {
                divsending.Controls.Add(BuildSendingOptionsPanel_DelayedMail());

                divsending.Controls.Add(BuildSendingOptionsPanel_Filtering());
            }

            

            return panelSchedule;
        }

        /// <summary>
        /// Construit le corps de page de l'étape Contrôle avant d'envoi
        ///   Etape 5
        /// </summary>
        /// <returns>Panel de type DIV contenant le code HTML de l'étape</returns>
        protected virtual Panel BuildControlBeforeSendPanel()
        {
            Panel panelControlebeforeSend = new Panel();
            panelControlebeforeSend.CssClass = "edn--container";

            HtmlGenericControl divControle = new HtmlGenericControl("div");
            divControle.Attributes.Add("Id", "edn--controle");
            panelControlebeforeSend.Controls.Add(divControle);

            divControle.Controls.Add(BuildSendingValidateBon());
            divControle.Controls.Add(BuildCompaignType());
            divControle.Controls.Add(BuildDelivrabilityScore());
            //block of Quality addresses email
            divControle.Controls.Add(BuildQualityEmailAdresse());
            //bloc of recepient count
            divControle.Controls.Add(BuildRecepientCountBloc());

            return panelControlebeforeSend;
        }
        /// <summary>
        /// construit le corps de l'étape Infos campagne
        /// etape 6
        /// </summary>
        /// <returns></returns>
        protected virtual Panel BuildInfosCampaignPanel()
        {
            Panel panelInfosCampagnOptions = new Panel();
            panelInfosCampagnOptions.CssClass = "edn--container";
            // Form pour encadré des informations de campagne
            HtmlGenericControl formCampaignInfo = new HtmlGenericControl("div");
            formCampaignInfo.ID = "edn--contact";
            panelInfosCampagnOptions.Controls.Add(formCampaignInfo);

            #region Section infos campagn
            // DIV pour encadré des informations de campagne
            HtmlGenericControl divCampaignInfo = new HtmlGenericControl("div");
            divCampaignInfo.Attributes.Add("class", "block--emailing");
            formCampaignInfo.Controls.Add(divCampaignInfo);

            //label de section Infos Campagn
            HtmlGenericControl label = new HtmlGenericControl("h3");
            label.InnerHtml = eResApp.GetRes(Pref, 2882);
            divCampaignInfo.Controls.Add(label);

            #region Info de la camapgne
            //Date de la campagne
            HtmlGenericControl divCampaignDateInfo = new HtmlGenericControl("div");
            divCampaignDateInfo.Attributes.Add("class", "field-container");
            divCampaignInfo.Controls.Add(divCampaignDateInfo);

            HtmlGenericControl labelCampaignInfoLabel = new HtmlGenericControl("label");
            labelCampaignInfoLabel.InnerHtml = eResApp.GetRes(Pref, 6407); // Campagne :
            divCampaignDateInfo.Controls.Add(labelCampaignInfoLabel);

            HtmlGenericControl labelCampaignDateInfo = new HtmlGenericControl("h4");
            labelCampaignDateInfo.InnerHtml = String.Concat(' ', this._mailing.MailingParams["libelle"]);
            divCampaignDateInfo.Controls.Add(labelCampaignDateInfo);

            //Description 
            HtmlGenericControl divCampaignDescriptionInfo = new HtmlGenericControl("div");
            divCampaignDescriptionInfo.Attributes.Add("class", "field-container");
            divCampaignInfo.Controls.Add(divCampaignDescriptionInfo);

            HtmlGenericControl labelCampaignDescriptionLabel = new HtmlGenericControl("label");
            labelCampaignDescriptionLabel.InnerHtml = eResApp.GetRes(Pref, 6410); // Description
            divCampaignDescriptionInfo.Controls.Add(labelCampaignDescriptionLabel);

            HtmlInputText txtCampaignInfoDescription = new HtmlInputText();
            divCampaignDescriptionInfo.Controls.Add(txtCampaignInfoDescription);
            txtCampaignInfoDescription.Value = Mailing.GetParamValue("description");
            txtCampaignInfoDescription.Attributes.Add("onblur", "oMailing.SetParam('description', this.value);");
            txtCampaignInfoDescription.Attributes.Add("class", "mailingCampaignInfoDescription");
            #endregion

            //divCampaignInfo.Controls.Add(BuildSendingOptionsPanel_CampaignInfo());
            //panelInfosCampagnOptions.Controls.Add(divCampaignInfo);
            #endregion

            #region sending information  
            ((eEditMailingRenderer)_efRend).RenderFldSendinginfos(formCampaignInfo);
            #endregion

            return panelInfosCampagnOptions;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fldFrom"></param>
        /// <param name="mail"></param>
        /// <returns></returns>
        protected virtual bool MailIsSelected(eFieldRecord fld, string mail)
        {
            return fld.Value.Trim().Equals(mail.Trim());
        }

        /// <summary>
        /// Bloc Option - Partie Info de la campagne (Nom de la campagne, Description...)
        /// </summary>
        /// <returns></returns>
        private WebControl BuildSendingOptionsPanel_CampaignInfo()
        {
            Boolean bIsCategoryMandatory = false;
            if (_efCampaignFile != null)
            {
                eFieldRecord fldCat = _efCampaignFile.GetField(CampaignField.CATEGORY.GetHashCode());
                if (fldCat != null)
                    bIsCategoryMandatory = fldCat.IsMandatory;
            }

            // UL
            eUlCtrl ulCampaignInfo = new eUlCtrl();

            #region libellé
            eLiCtrl liCampaignInfoLabel = ulCampaignInfo.AddLi();

            HtmlGenericControl labelCampaignInfoLabel = new HtmlGenericControl("label"); liCampaignInfoLabel.Controls.Add(labelCampaignInfoLabel);
            labelCampaignInfoLabel.Attributes.Add("class", "mailingCampaignInfoLabel label");
            labelCampaignInfoLabel.InnerHtml = eResApp.GetRes(Pref, 6407); // Campagne :
            HtmlGenericControl labelCampaignInfoLabelValue = new HtmlGenericControl("span"); liCampaignInfoLabel.Controls.Add(labelCampaignInfoLabelValue);
            labelCampaignInfoLabelValue.Attributes.Add("class", "mailingCampaignInfoLabelValue");
            labelCampaignInfoLabelValue.InnerHtml = this._mailing.MailingParams["libelle"]; // Libellé de la campagne (non modifiable)
            #endregion

            #region Description
            eLiCtrl liCampaignInfoDescription = ulCampaignInfo.AddLi();
            HtmlGenericControl labelCampaignInfoDescription = new HtmlGenericControl("label"); liCampaignInfoDescription.Controls.Add(labelCampaignInfoDescription);

            labelCampaignInfoDescription.InnerHtml = eResApp.GetRes(Pref, 6410); // Description
            labelCampaignInfoDescription.Attributes.Add("class", "mailingCampaignInfoDescription label");

            HtmlInputText txtCampaignInfoDescription = new HtmlInputText(); liCampaignInfoDescription.Controls.Add(txtCampaignInfoDescription);
            txtCampaignInfoDescription.Value = Mailing.GetParamValue("description");
            txtCampaignInfoDescription.Attributes.Add("onblur", "oMailing.SetParam('description', this.value);");
            txtCampaignInfoDescription.Attributes.Add("class", "mailingCampaignInfoDescription");
            #endregion

            #region Nombre de mail a envoyer

            //eLiCtrl liCampaingInfoNbMail = ulCampaignInfo.AddLi();

            //HtmlGenericControl labelCampaingInfoLabelNbMail = new HtmlGenericControl("label"); liCampaingInfoNbMail.Controls.Add(labelCampaingInfoLabelNbMail);
            //labelCampaingInfoLabelNbMail.InnerHtml = eResApp.GetRes(Pref, 6620); //Nombre d'emails à envoyer
            //labelCampaingInfoLabelNbMail.Attributes.Add("class", "mailingCampaignInfoLabel label");

            //HtmlGenericControl labelCampaingInfoNbMail = new HtmlGenericControl("input"); 
            //liCampaingInfoNbMail.Controls.Add(labelCampaingInfoNbMail);
            //labelCampaingInfoNbMail.Attributes.Add("type", "button");
            //labelCampaingInfoNbMail.Attributes.Add("value", eResApp.GetRes(Pref, 6674));
            //labelCampaingInfoNbMail.Attributes.Add("onclick", "callUpdtCmptNbMail();");
            //labelCampaingInfoNbMail.Attributes.Add("class", "mailingCampaingInfoNbMail");
            //labelCampaingInfoNbMail.ID = "CampaingInfoNbMail";

            #endregion

            #region Catégorie
            //eLiCtrl liCampaingInfoTitleCategory = ulCampaignInfo.AddLi();

            //if (_useNewUnsubscribeMethod)
            //    liCampaingInfoTitleCategory.Attributes.Add("class", "hide");

            ////Label 
            //HtmlGenericControl labelCategory = new HtmlGenericControl("label"); liCampaingInfoTitleCategory.Controls.Add(labelCategory);
            //labelCategory.Attributes.Add("class", "labelTitle");
            //if (!bIsCategoryMandatory)
            //    labelCategory.InnerText = eResApp.GetRes(Pref, 6665);//Voulez-vous proposer une catégorie de désinscription ?;
            //else
            //    labelCategory.InnerText = String.Concat(eResApp.GetRes(Pref, 6728), " :");//Unsubscribe Category;


            //eLiCtrl liCampaingInfoRadioBtn = ulCampaignInfo.AddLi();

            //ulCampaignInfo.Attributes.Add("class", "mailingCampaignInfo");

            //if (bIsCategoryMandatory)
            //{
            //    liCampaingInfoRadioBtn.Style.Add("display", "none");
            //}

            //liCampaingInfoRadioBtn.Attributes.Add("class", "li-opts-mailing rdmailing");

            //if (_useNewUnsubscribeMethod)
            //    liCampaingInfoRadioBtn.Attributes["class"] = String.Concat(liCampaingInfoRadioBtn.Attributes["class"], " hide");





            //Boolean bWithoutUnsubCat = eLibTools.GetNum(Mailing.GetParamValue("category")) == 0 && !bIsCategoryMandatory;
            ////Radio button oui non
            //HtmlInputRadioButton noBtn = new HtmlInputRadioButton();
            //noBtn.ID = "rd_no";
            //noBtn.Checked = bWithoutUnsubCat;
            //noBtn.Attributes.Add("onclick", "oMailing.DisplayCat(false);");
            //liCampaingInfoRadioBtn.Controls.Add(noBtn);
            //noBtn.Attributes.Add("name", "catYesNo");

            //HtmlGenericControl noLabel = new HtmlGenericControl("label");
            //noLabel.InnerHtml = eResApp.GetRes(Pref, 59); //Non
            //noLabel.Attributes.Add("for", noBtn.ID);
            //liCampaingInfoRadioBtn.Controls.Add(noLabel);

            //HtmlInputRadioButton yesBtn = new HtmlInputRadioButton();
            //yesBtn.ID = "rd_yes";


            //yesBtn.Checked = !bWithoutUnsubCat;
            //yesBtn.Attributes.Add("onclick", "oMailing.DisplayCat(true);");
            //liCampaingInfoRadioBtn.Controls.Add(yesBtn);
            //yesBtn.Attributes.Add("name", "catYesNo");

            //HtmlGenericControl yesLabel = new HtmlGenericControl("label");
            //yesLabel.InnerHtml = eResApp.GetRes(Pref, 58); //Oui
            //yesLabel.Attributes.Add("for", yesBtn.ID);
            //liCampaingInfoRadioBtn.Controls.Add(yesLabel);

            //// ASY : ajout de la categorie
            //eLiCtrl liCampaignInfoCategory = ulCampaignInfo.AddLi();
            //liCampaignInfoCategory.ID = "catUnsub";
            //liCampaignInfoCategory.Attributes.Add("class", "li-opts-mailing mailingFiltering");
            //if (_useNewUnsubscribeMethod)
            //    liCampaignInfoCategory.Attributes["class"] = String.Concat(liCampaignInfoCategory.Attributes["class"], " hide");

            //// ASY : ajout de la categorie 
            //HtmlGenericControl labelCampaignInfoCategory = new HtmlGenericControl("label"); liCampaignInfoCategory.Controls.Add(labelCampaignInfoCategory);

            //// ASY : ajout de la categorie
            //labelCampaignInfoCategory.InnerHtml = eResApp.GetRes(Pref, 6479); // Catégory Emailing:

            //if (bIsCategoryMandatory)
            //{
            //    //la ressource comporte le ":" 
            //    labelCampaignInfoCategory.InnerHtml = labelCampaignInfoCategory.InnerHtml.Replace(" :", "<span class='MndAst'>*</span> :");
            //}
            //// ASY : ajout de la categorie
            //labelCampaignInfoCategory.Attributes.Add("class", "mailingCampaignInfoLabel label");


            //HtmlInputText txtCampaignInfoCategoryValue = new HtmlInputText(); liCampaignInfoCategory.Controls.Add(txtCampaignInfoCategoryValue);
            //txtCampaignInfoCategoryValue.Value = Mailing.GetParamValue("categoryLabel");
            //txtCampaignInfoCategoryValue.Attributes.Add("class", "mailingCampaignInfoCategory readonly");

            //if (bIsCategoryMandatory)
            //{
            //    txtCampaignInfoCategoryValue.Attributes["class"] += " obg";
            //}

            //txtCampaignInfoCategoryValue.Attributes.Add("readonly", "true");

            //#region Recup infos du champs category


            //String error;
            //eudoDAL dal = eLibTools.GetEudoDAL(Pref);

            //FieldLite fldCategory = new FieldLite((int)CampaignField.CATEGORY);
            //FieldLite fldMediaType = new FieldLite((int)CampaignField.MEDIATYPE);
            //int mediaTypeEmailCatId = 0;

            //try
            //{
            //    dal.OpenDatabase();

            //    fldCategory.ExternalLoadInfo(dal, out error);
            //    if (error.Length != 0)
            //        LaunchError(error);

            //    fldMediaType.ExternalLoadInfo(dal, out error);
            //    if (error.Length != 0)
            //        LaunchError(error);

            //    if (Mailing.Id != 0)
            //    {
            //        Int32.TryParse(Mailing.GetParamValue("mediaType"), out mediaTypeEmailCatId);
            //    }
            //    else
            //    {
            //        eCatalog catMediaType = new eCatalog(dal, Pref, fldMediaType.Popup, Pref.User, fldMediaType.PopupDescId);
            //        eCatalog.CatalogValue emailCatValue = catMediaType.Values.FirstOrDefault(c => c.Data == "email");
            //        mediaTypeEmailCatId = emailCatValue != null ? emailCatValue.Id : 0;

            //        if (mediaTypeEmailCatId != 0)
            //            Mailing.SetParamValue("mediaType", mediaTypeEmailCatId.ToString());
            //    }
            //}
            //finally
            //{
            //    dal.CloseDatabase();
            //}

            //#endregion

            //txtCampaignInfoCategoryValue.Attributes.Add("edndescid", fldCategory.Descid.ToString());
            //txtCampaignInfoCategoryValue.Attributes.Add("popup", fldCategory.Popup.GetHashCode().ToString());
            //txtCampaignInfoCategoryValue.Attributes.Add("bounddescid", fldMediaType != null ? fldMediaType.Descid.ToString() : "");
            //txtCampaignInfoCategoryValue.Attributes.Add("boundpopup", fldMediaType != null ? ((int)fldMediaType.Popup).ToString() : "");
            //txtCampaignInfoCategoryValue.Attributes.Add("multi", (fldCategory.Multiple ? "1" : "0"));
            //txtCampaignInfoCategoryValue.Attributes.Add("ednvalue", Mailing.GetParamValue("category"));
            //txtCampaignInfoCategoryValue.Attributes.Add("boundvalue", mediaTypeEmailCatId != 0 ? mediaTypeEmailCatId.ToString() : "");

            //Int32 tabIndex = TableType.CAMPAIGN.GetHashCode();
            //Int32 lineIndex = fldCategory.Descid; // ?
            //txtCampaignInfoCategoryValue.ID = String.Concat("value_", tabIndex, "_", lineIndex);


            //eGenericWebControl categoryCatalogIconButton = new eGenericWebControl(HtmlTextWriterTag.Span);
            //categoryCatalogIconButton.CssClass = "icon-catalog btn icnFileBtn";

            //categoryCatalogIconButton.Attributes.Add("onclick", String.Concat("showCategoryCat(\"", tabIndex, "\", \"", lineIndex, "\");"));
            //categoryCatalogIconButton.ID = "catId";
            //liCampaignInfoCategory.Controls.Add(categoryCatalogIconButton);

            //txtCampaignInfoCategoryValue.Attributes.Add("onclick", "document.getElementById('catId').click();");

            ////  eLiCtrl liFilteringRemoveUnsubscribed = new eLiCtrl(); ulCampaignInfo.Controls.Add(liFilteringRemoveUnsubscribed);
            //// liFilteringRemoveUnsubscribed.ID = "filtreUnsub";
            ////MOU cf. 29277 on ne peut plus outrepasser le desbonnement
            //// liFilteringRemoveUnsubscribed.Attributes.Add("class", "li-opts-mailing mailingFiltering");

            #endregion

            return ulCampaignInfo;
        }

        private void LaunchError(string error)
        {
            //Arrete le traitement et envoi l'erreur
            string sDevMsg = string.Concat("Erreur sur la page : ", System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine
            , Environment.NewLine);
            sDevMsg = string.Concat(sDevMsg, Environment.NewLine, "Exception : ", error);

            eErrorContainer ErrorContainer = eErrorContainer.GetDevUserError(
            eLibConst.MSG_TYPE.CRITICAL,
            eResApp.GetRes(Pref, 72),   // Message En-tête : Une erreur est survenue
            String.Concat(eResApp.GetRes(Pref, 422), "<br>", eResApp.GetRes(Pref, 544)),  //  Détail : pour améliorer...
            eResApp.GetRes(Pref, 72),  //   titre 
            sDevMsg);

            eFeedbackXrm.LaunchFeedbackXrm(ErrorContainer, Pref);
        }

        private HtmlGenericControl BuildSendingOptionsPanel_MarketingAutomation()
        {

            HtmlGenericControl divSending = new HtmlGenericControl("div");
            divSending.Attributes.Add("class", "block--schedule");

            HtmlGenericControl labelsending = new HtmlGenericControl("h3");
            labelsending.InnerHtml = eResApp.GetRes(Pref, 6403);
            divSending.Controls.Add(labelsending);

            HtmlGenericControl contentMarketingAutomation = new HtmlGenericControl("div");
            contentMarketingAutomation.Attributes.Add("class", "contentSending");

            HtmlGenericControl contentPicto = new HtmlGenericControl("img");
            contentPicto.Attributes.Add("src", "./IRISBlack/Front/Assets/CSS/flowy/assets/robot_emailing.png"); //D:\GIT\EUDONET\CRM\EudonetXRM\XRM\IRISBlack\Front\Assets\CSS\flowy\assets\robot_emailing.png
            contentPicto.Attributes.Add("alt", "robot_emailing.png");
            contentMarketingAutomation.Controls.Add(contentPicto);
            
            HtmlGenericControl contentSpan = new HtmlGenericControl("span");
            contentSpan.InnerHtml = eResApp.GetRes(Pref, 8855);
            contentMarketingAutomation.Controls.Add(contentSpan);
            
            divSending.Controls.Add(contentMarketingAutomation);

            #region  Onglet où seront enregistrés les couriels envoyés

            divSending.Controls.Add(BuildSendingOptionsPanel_EmailSelection());
            #endregion

            return divSending;
        }

        /// <summary>
        /// Bloc Option - Partie Liens de tracking (Date de purge...)
        /// </summary>
        /// <returns></returns>
        private HtmlGenericControl BuildSendingOptionsPanel_Filtering()
        {

            HtmlGenericControl divFiltering = new HtmlGenericControl("div");
            divFiltering.Attributes.Add("class", "block--schedule");

            HtmlGenericControl labelsending = new HtmlGenericControl("h3");
            labelsending.InnerHtml = eResApp.GetRes(Pref, 6663);
            divFiltering.Controls.Add(labelsending);

            #region duré de vie
            HtmlGenericControl divMailingLifeTrack = new HtmlGenericControl("div");
            divMailingLifeTrack.Attributes.Add("class", "field-container");
            divFiltering.Controls.Add(divMailingLifeTrack);


            string sLabelLifw = eResApp.GetRes(_ePref, 2781) + Environment.NewLine + eResApp.GetRes(_ePref, 2787);
            HtmlGenericControl labelMailingLifeTrack = new HtmlGenericControl("label");
            labelMailingLifeTrack.Attributes.Add("for", "name");
            labelMailingLifeTrack.InnerHtml = eResApp.GetRes(_ePref, 6482);
            labelMailingLifeTrack.Attributes.Add("title", sLabelLifw);
            divMailingLifeTrack.Controls.Add(labelMailingLifeTrack);

            HtmlGenericControl divContentTableMailingLifeTrack = new HtmlGenericControl("div");
            divContentTableMailingLifeTrack.Attributes.Add("class", "content-table");
            divMailingLifeTrack.Controls.Add(divContentTableMailingLifeTrack);

            //input for date
            HtmlInputText divContentInputMailingLifeTrack = new HtmlInputText("input");
            divContentInputMailingLifeTrack.Attributes.Add("class", "content-input");
            divContentInputMailingLifeTrack.Attributes.Add("readonly", "true");
            divContentInputMailingLifeTrack.Attributes.Add("type", "text");
            divContentInputMailingLifeTrack.ID = "MailingLifeTrack_Date";
            divContentInputMailingLifeTrack.Name = divContentInputMailingLifeTrack.ID;
            divContentInputMailingLifeTrack.Value = eDate.ConvertBddToDisplay(Pref.CultureInfo, Mailing.GetParamValue("trackLnkLifeTime"));
            divContentInputMailingLifeTrack.Attributes.Add("title", sLabelLifw);
            divContentInputMailingLifeTrack.Attributes.Add("onClick", "document.getElementById('imgLinkLifeTime').click();");

            divContentTableMailingLifeTrack.Controls.Add(divContentInputMailingLifeTrack);

            //calendrier
            HtmlGenericControl buttonMailingLifeTrack = new HtmlGenericControl("span");
            buttonMailingLifeTrack.Attributes.Add("onclick", String.Concat("onSelectMailingDate('", divContentInputMailingLifeTrack.ID, "', 'trackLnkLifeTime');"));
            buttonMailingLifeTrack.Attributes.Add("Id", "imgLinkLifeTime");

            divContentTableMailingLifeTrack.Controls.Add(buttonMailingLifeTrack);

            HtmlGenericControl iconDelayedSending = new HtmlGenericControl("i");
            iconDelayedSending.Attributes.Add("class", "icon-calendar2");
            buttonMailingLifeTrack.Controls.Add(iconDelayedSending);
            #endregion
            #region duré de purge
            HtmlGenericControl divDatePurgeofLinkTracking = new HtmlGenericControl("div");
            divDatePurgeofLinkTracking.Attributes.Add("class", "field-container");
            divFiltering.Controls.Add(divDatePurgeofLinkTracking);


            string sLabelPurge = eResApp.GetRes(_ePref, 2782) + Environment.NewLine + eResApp.GetRes(_ePref, 2788);
            HtmlGenericControl labelDatePurgeofLinkTracking = new HtmlGenericControl("label");
            labelDatePurgeofLinkTracking.Attributes.Add("for", "name");
            labelDatePurgeofLinkTracking.InnerHtml = eResApp.GetRes(_ePref, 6483);
            labelDatePurgeofLinkTracking.Attributes.Add("title", sLabelPurge);
            divDatePurgeofLinkTracking.Controls.Add(labelDatePurgeofLinkTracking);

            HtmlGenericControl divContentTableDatePurgeofLinkTracking = new HtmlGenericControl("div");
            divContentTableDatePurgeofLinkTracking.Attributes.Add("class", "content-table");
            divDatePurgeofLinkTracking.Controls.Add(divContentTableDatePurgeofLinkTracking);

            //input for date
            HtmlInputText divContentInputDatePurgeofLinkTracking = new HtmlInputText("input");
            divContentInputDatePurgeofLinkTracking.Attributes.Add("class", "content-input");
            divContentInputDatePurgeofLinkTracking.Attributes.Add("readonly", "true");
            divContentInputDatePurgeofLinkTracking.Attributes.Add("type", "text");
            divContentInputDatePurgeofLinkTracking.ID = "purgeLinkTrack_Date";
            divContentInputDatePurgeofLinkTracking.Name = divContentInputDatePurgeofLinkTracking.ID;
            divContentInputDatePurgeofLinkTracking.Value = eDate.ConvertBddToDisplay(Pref.CultureInfo, Mailing.GetParamValue("trackLnkPurgedDate"));
            divContentInputDatePurgeofLinkTracking.Attributes.Add("title", sLabelPurge);
            divContentInputDatePurgeofLinkTracking.Attributes.Add("onClick", "document.getElementById('imgLinkLifeTime').click();");

            divContentTableDatePurgeofLinkTracking.Controls.Add(divContentInputDatePurgeofLinkTracking);

            //calendrier
            HtmlGenericControl buttonDatePurgeofLinkTracking = new HtmlGenericControl("span");
            buttonDatePurgeofLinkTracking.Attributes.Add("onclick", String.Concat("document.getElementById('delayedSending').checked = true; onSelectMailingDate('", divContentInputDatePurgeofLinkTracking.ID, "', 'trackLnkPurgedDate');"));
            buttonDatePurgeofLinkTracking.Attributes.Add("Id", "purgeLinkTrack");

            divContentTableDatePurgeofLinkTracking.Controls.Add(buttonDatePurgeofLinkTracking);

            HtmlGenericControl iconDatePurgeofLinkTracking = new HtmlGenericControl("i");
            iconDatePurgeofLinkTracking.Attributes.Add("class", "icon-calendar2");
            buttonDatePurgeofLinkTracking.Controls.Add(iconDatePurgeofLinkTracking);

            #endregion

            return divFiltering;
        }

        /// <summary>
        /// Bloc Option - Partie Envoi programmé
        /// </summary>
        /// <returns></returns>
        protected HtmlGenericControl BuildSendingOptionsPanel_DelayedMail()
        {
            #region schedule your delivery      

            HtmlGenericControl divschedule = new HtmlGenericControl("div");
            divschedule.Attributes.Add("class", "block--schedule");


            HtmlGenericControl labelsending = new HtmlGenericControl("h3");
            labelsending.InnerHtml = eResApp.GetRes(Pref, 6664);
            divschedule.Controls.Add(labelsending);

            HtmlGenericControl divFieldContainer = new HtmlGenericControl("div");
            divFieldContainer.Attributes.Add("class", "field-container");
            divschedule.Controls.Add(divFieldContainer);

            HtmlGenericControl labelSelect = new HtmlGenericControl("label");
            labelSelect.Attributes.Add("for", "name");
            labelSelect.InnerHtml = eResApp.GetRes(Pref, 8758);
            divFieldContainer.Controls.Add(labelSelect);

            #region radio

            //div of radio
            HtmlGenericControl divfieldcontainerradio = new HtmlGenericControl("div");
            divfieldcontainerradio.Attributes.Add("class", "field-container ma-0");
            divschedule.Controls.Add(divfieldcontainerradio);
            //div raio cutom
            HtmlGenericControl divradiocustom = new HtmlGenericControl("div");
            divradiocustom.Attributes.Add("Id", "radio--custom");
            divfieldcontainerradio.Controls.Add(divradiocustom);
            //div radio group
            HtmlGenericControl divradiogroup = new HtmlGenericControl("div");
            divradiogroup.Attributes.Add("class", "radio-title-group");
            divradiocustom.Controls.Add(divradiogroup);

            #region First radio Envoi immédiat 
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
            #region immediat
            //div immediat
            HtmlGenericControl divfieldContainerContentImmediate = new HtmlGenericControl("div");
            divfieldContainerContentImmediate.Attributes.Add("class", "field-container--content immediateDispatch");
            divfieldContainerContentImmediate.Attributes.Add("display", "block");
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

            #endregion

            #region second radio Envoi différé             
            //div input container
            HtmlGenericControl divinputDelayed = new HtmlGenericControl("div");
            divinputDelayed.Attributes.Add("class", "input-container");
            divradiogroup.Controls.Add(divinputDelayed);

            //input
            HtmlGenericControl inputDelayed = new HtmlGenericControl("input");
            inputDelayed.Attributes.Add("class", "radio-button");
            inputDelayed.Attributes.Add("Id", "delayedSending");
            inputDelayed.Attributes.Add("value", "later");
            inputDelayed.Attributes.Add("type", "radio");
            inputDelayed.Attributes.Add("name", "radio");
            inputDelayed.Attributes.Add("onclick", "switchToContainer(this,event)");
            divinputDelayed.Controls.Add(inputDelayed);

            // div icon/button
            HtmlGenericControl divradioDelayed = new HtmlGenericControl("div");
            divradioDelayed.Attributes.Add("class", "radio-title");
            divinputDelayed.Controls.Add(divradioDelayed);

            // div icon
            HtmlGenericControl diviconDelayed = new HtmlGenericControl("div");
            diviconDelayed.Attributes.Add("class", "icon bike-icon");
            divradioDelayed.Controls.Add(diviconDelayed);

            // href
            HtmlGenericControl linkiconDelayed = new HtmlGenericControl("i");
            linkiconDelayed.Attributes.Add("class", "icon-clock-o");
            diviconDelayed.Controls.Add(linkiconDelayed);

            //label
            HtmlGenericControl labelidelayedsending = new HtmlGenericControl("label");
            labelidelayedsending.Attributes.Add("class", "radio-title-label");
            labelidelayedsending.Attributes.Add("for", "delayedSending");
            labelidelayedsending.InnerText = eResApp.GetRes(_ePref, 820);
            divradioDelayed.Controls.Add(labelidelayedsending);
            #region delayed
            // div delayed
            HtmlGenericControl divfieldContanerContentDelayedSending = new HtmlGenericControl("div");
            divfieldContanerContentDelayedSending.Attributes.Add("class", "field-container--content delayedSending");

            divfieldContanerContentDelayedSending.Attributes.Add("display", "none");
            divschedule.Controls.Add(divfieldContanerContentDelayedSending);

            #region Date et Heure d'envoi
            HtmlGenericControl divfieldContainerDelayedSending = new HtmlGenericControl("div");
            divfieldContainerDelayedSending.Attributes.Add("class", "field-container");
            divfieldContanerContentDelayedSending.Controls.Add(divfieldContainerDelayedSending);

            eLabelCtrl labelDelayedSending = new eLabelCtrl();
            labelDelayedSending.For = "delayedMail_Date";
            labelDelayedSending.InnerText = eResApp.GetRes(_ePref, 6422);
            divfieldContainerDelayedSending.Controls.Add(labelDelayedSending);

            HtmlGenericControl divContentTableDelayedSending = new HtmlGenericControl("div");
            divContentTableDelayedSending.Attributes.Add("class", "content-table");
            divfieldContanerContentDelayedSending.Controls.Add(divContentTableDelayedSending);

            //input for date
            HtmlInputText divContentInputDelayedSending = new HtmlInputText("input");
            divContentInputDelayedSending.Attributes.Add("class", "content-input");
            divContentInputDelayedSending.Attributes.Add("readonly", "");
            divContentInputDelayedSending.Attributes.Add("type", "text");
            divContentInputDelayedSending.ID = "delayedMail_Date";
            divContentInputDelayedSending.Name = divContentInputDelayedSending.ID;

            divContentInputDelayedSending.Attributes.Add("onClick", "document.getElementById('delayedMailDate').click();");

            divContentTableDelayedSending.Controls.Add(divContentInputDelayedSending);

            if (!bImmediateSending && !bReccurentSending)
                divContentInputDelayedSending.Value = Mailing.GetParamValue("sendingDate");

            //calendrier
            HtmlGenericControl buttonDelayedSending = new HtmlGenericControl("span");
            buttonDelayedSending.Attributes.Add("onclick", String.Concat("document.getElementById('delayedSending').checked = true; onSelectMailingDate('", divContentInputDelayedSending.ID, "', 'sendingDate');"));
            buttonDelayedSending.Attributes.Add("Id", "delayedMailDate");

            divContentTableDelayedSending.Controls.Add(buttonDelayedSending);

            HtmlGenericControl iconDelayedSending = new HtmlGenericControl("i");
            iconDelayedSending.Attributes.Add("class", "icon-calendar2");
            buttonDelayedSending.Controls.Add(iconDelayedSending);
            #endregion

            #region Timezone  
            // Liste des fuseaux horaires
            ReadOnlyCollection<TimeZoneInfo> tz = TimeZoneInfo.GetSystemTimeZones();
            List<ListItem> _timezonesItemsList = tz.Select(t => new ListItem(t.DisplayName, t.Id)).ToList();

            //Recuperation index fuseau horaire server (voir pour fuseau client)
            string prefTimeZone = eLibTools.GetPrefAdvValues(Pref, new List<eLibConst.PREFADV>() { eLibConst.PREFADV.DEFAULT_TIMEZONE }, Pref.UserId)?.First().Value;
            int idxCurrentTz = 0;
            TimeZoneInfo orginialTimeZone = null;

            if (!string.IsNullOrEmpty(prefTimeZone))
            {
                try
                {

                    orginialTimeZone = TimeZoneInfo.FindSystemTimeZoneById(prefTimeZone);

                }
                catch (Exception ee)
                {
                    //Si on ne peut pas lire le fuseau horaire, on le réinitialise et on envoie un feedback
                    eLibTools.AddOrUpdatePrefAdv(Pref, eLibConst.PREFADV.DEFAULT_TIMEZONE, "", eLibConst.PREFADV_CATEGORY.MAIN, Pref.UserId);
                    throw new EudoInvalidTimeZoneException(ee, true);
                }
            }

            // On recupere le fuseau par defaut si defini et si on est en création, I.E heure vide
            if (string.IsNullOrEmpty(divContentInputDelayedSending.Value) && !string.IsNullOrEmpty(prefTimeZone) && orginialTimeZone != null)
            {
                //  orginialTimeZone = null; // test feedback
                try
                {

                    idxCurrentTz = _timezonesItemsList.FindIndex(item => item.Value == tz.First(time => time.DisplayName == orginialTimeZone.DisplayName).Id);
                }
                catch (Exception e)
                {
                    //Si on ne peut pas lire le fuseau horaire, on le réinitialise et on envoie un feedback
                    eLibTools.AddOrUpdatePrefAdv(Pref, eLibConst.PREFADV.DEFAULT_TIMEZONE, "", eLibConst.PREFADV_CATEGORY.MAIN, Pref.UserId);
                    throw new EudoInvalidTimeZoneException(e, true);
                }
            }
            else //Sinon on recupere le fuseau du serveur
                idxCurrentTz = _timezonesItemsList.FindIndex(item => item.Value == tz.First(time => time.StandardName == TimeZone.CurrentTimeZone.StandardName).Id);// TimeZone.CurrentTimeZone

            HtmlGenericControl divContentTimeZone = new HtmlGenericControl("div");
            divContentTimeZone.Attributes.Add("class", "field-container");
            divfieldContanerContentDelayedSending.Controls.Add(divContentTimeZone);

            HtmlGenericControl labelTimeZone = new HtmlGenericControl("label");
            labelTimeZone.Attributes.Add("for", "name");
            labelTimeZone.InnerHtml = eResApp.GetRes(_ePref, 8683);
            divContentTimeZone.Controls.Add(labelTimeZone);


            HtmlSelect timeZoneSelect = new HtmlSelect();
            divContentTimeZone.Controls.Add(timeZoneSelect);
            timeZoneSelect.ID = "delayedMail_TimeZone";
            timeZoneSelect.Items.AddRange(_timezonesItemsList.ToArray());
            //emailFileSelect.DataBind();
            timeZoneSelect.SelectedIndex = idxCurrentTz;

            if (!string.IsNullOrEmpty(prefTimeZone) && !string.IsNullOrEmpty(divContentInputDelayedSending.Value))
            {

                DateTime serverDate = DateTime.Parse(divContentInputDelayedSending.Value);

                DateTime originalDate = TimeZoneInfo.ConvertTime(serverDate, TimeZoneInfo.Local, orginialTimeZone);

                eDivCtrl liInfoTimeZone = new eDivCtrl();
                divfieldContanerContentDelayedSending.Controls.Add(liInfoTimeZone);
                liInfoTimeZone.CssClass = "field-container";
                eLabelCtrl labelOrginialTimeZone = new eLabelCtrl();
                liInfoTimeZone.Controls.Add(labelOrginialTimeZone);
                labelOrginialTimeZone.InnerText = String.Format(eResApp.GetRes(Pref, 8734),
                originalDate.ToString(), orginialTimeZone?.DisplayName);
            }


            #endregion

            #region radio Button

            HtmlGenericControl divRadioContainerTimeZone = new HtmlGenericControl("div");
            divRadioContainerTimeZone.Attributes.Add("class", "radio-container");
            divfieldContanerContentDelayedSending.Controls.Add(divRadioContainerTimeZone);

            #region Envoi différé : classique
            HtmlGenericControl divRequestMode_normal = new HtmlGenericControl("div");
            divRequestMode_normal.Attributes.Add("class", "radio");
            divRadioContainerTimeZone.Controls.Add(divRequestMode_normal);

            HtmlGenericControl firstradioDelaySendingInputTimeZone = new HtmlGenericControl("input");
            firstradioDelaySendingInputTimeZone.Attributes.Add("id", "RequestMode_normal");
            firstradioDelaySendingInputTimeZone.Attributes.Add("Name", "RequestMode");
            firstradioDelaySendingInputTimeZone.Attributes.Add("type", "radio");
            if (rm == MAILINGQUERYMODE.NORMAL)
                firstradioDelaySendingInputTimeZone.Attributes.Add("checked", "");

            firstradioDelaySendingInputTimeZone.Attributes.Add("value", ((int)MAILINGQUERYMODE.NORMAL).ToString());
            firstradioDelaySendingInputTimeZone.Attributes.Add("onclick", "oMailing.OnSelectRequestMode(this)");
            divRequestMode_normal.Controls.Add(firstradioDelaySendingInputTimeZone);

            HtmlGenericControl labelFirstRadioTimeZone = new HtmlGenericControl("label");
            labelFirstRadioTimeZone.Attributes.Add("for", "RequestMode_normal");
            labelFirstRadioTimeZone.Attributes.Add("class", "radio-label");
            labelFirstRadioTimeZone.InnerHtml = eResApp.GetRes(_ePref, 6736);
            divRequestMode_normal.Controls.Add(labelFirstRadioTimeZone);
            #endregion

            #region Envoi différé : Rejouer la requête
            HtmlGenericControl divRequestMode_QueryRunAgain = new HtmlGenericControl("div");
            divRequestMode_QueryRunAgain.Attributes.Add("class", "radio");
            divRadioContainerTimeZone.Controls.Add(divRequestMode_QueryRunAgain);

            HtmlGenericControl secondradioDelaySendingInputTimeZone = new HtmlGenericControl("input");
            secondradioDelaySendingInputTimeZone.Attributes.Add("id", "RequestMode_QueryRunAgain");
            secondradioDelaySendingInputTimeZone.Attributes.Add("name", "RequestMode");
            secondradioDelaySendingInputTimeZone.Attributes.Add("type", "radio");
            secondradioDelaySendingInputTimeZone.Attributes.Add("value", ((int)MAILINGQUERYMODE.QUERY_RUN_AGAIN).ToString());
            if (rm == MAILINGQUERYMODE.QUERY_RUN_AGAIN)
                secondradioDelaySendingInputTimeZone.Attributes.Add("checked", "");
            secondradioDelaySendingInputTimeZone.Attributes.Add("onclick", "oMailing.OnSelectRequestMode(this)");
            divRequestMode_QueryRunAgain.Controls.Add(secondradioDelaySendingInputTimeZone);

            HtmlGenericControl labelSecondRadioTimeZone = new HtmlGenericControl("label");
            labelSecondRadioTimeZone.Attributes.Add("for", "RequestMode_QueryRunAgain");
            labelSecondRadioTimeZone.Attributes.Add("class", "radio-label");
            labelSecondRadioTimeZone.InnerHtml = eResApp.GetRes(_ePref, 6737);
            divRequestMode_QueryRunAgain.Controls.Add(labelSecondRadioTimeZone);
            #endregion

            #endregion

            #endregion

            #endregion

            #region third radio recurrents
            //div input container
            if (_mailing == null || _mailing.ParentTab == 0 || _ePref.ThemeXRM.Version < 2 || GetAdminTableInfos(_ePref, _mailing.ParentTab)?.EudonetXIrisBlackStatus != EUDONETX_IRIS_BLACK_STATUS.ENABLED)
            {
                HtmlGenericControl divinputrecurring = new HtmlGenericControl("div");
                divinputrecurring.Attributes.Add("class", "input-container");
                divradiogroup.Controls.Add(divinputrecurring);

                if (!_eventStepEnabled || _bOnHold)
                    divinputrecurring.Style.Add("display", "none");

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
                linkiconReccuring.Attributes.Add("class", "icon-refresh");
                diviconReccuring.Controls.Add(linkiconReccuring);

                //label
                HtmlGenericControl labelReccuringsending = new HtmlGenericControl("label");
                labelReccuringsending.Attributes.Add("class", "radio-title-label");
                labelReccuringsending.Attributes.Add("for", "recurringSending");
                labelReccuringsending.InnerText = eResApp.GetRes(_ePref, 2037);
                divradioReccuring.Controls.Add(labelReccuringsending);

                #region Reccurent           
                // div reccurent
                HtmlGenericControl divfieldContanerContentReccurentSending = new HtmlGenericControl("div");
                divfieldContanerContentReccurentSending.Attributes.Add("class", "field-container--content recurringSending");
                divfieldContanerContentReccurentSending.Attributes.Add("display", "none");
                divschedule.Controls.Add(divfieldContanerContentReccurentSending);

                #region Envoi récurrent : fréquence
                HtmlGenericControl divContainerPlanner = new HtmlGenericControl("div");
                divContainerPlanner.Attributes.Add("class", "field-container");
                divfieldContanerContentReccurentSending.Controls.Add(divContainerPlanner);

                HtmlGenericControl labelReccurentSending = new HtmlGenericControl("label");
                labelReccurentSending.Attributes.Add("onclick", "oMailing.openScheduleParameter()");
                labelReccurentSending.Attributes.Add("Id", "openschedule");
                labelReccurentSending.Attributes.Add("class", "setting-planification");
                divContainerPlanner.Controls.Add(labelReccurentSending);

                HtmlGenericControl spanReccurentSending = new HtmlGenericControl("span");
                spanReccurentSending.InnerHtml = eResApp.GetRes(_ePref, 6888);
                spanReccurentSending.Attributes.Add("Id", "openschedule");
                labelReccurentSending.Controls.Add(spanReccurentSending);

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
                firstradioReccurentSending.Attributes.Add("onclick", "oMailing.OnSelectRequestMode(this)");
                divRecurrentRequestMode_normal.Controls.Add(firstradioReccurentSending);

                HtmlGenericControl labelFirstRadioReccurentSending = new HtmlGenericControl("label");
                labelFirstRadioReccurentSending.Attributes.Add("for", "RequestMode_ReccurentAll");
                labelFirstRadioReccurentSending.Attributes.Add("class", "radio-label");
                labelFirstRadioReccurentSending.InnerHtml = eResApp.GetRes(_ePref, 2038);
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
                secondradioReccurentSending.Attributes.Add("value", ((int)MAILINGQUERYMODE.RECURRENT_FILTER).ToString());
                secondradioReccurentSending.Attributes.Add("onclick", "oMailing.OnSelectRequestMode(this)");
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
                labellnkFilter.Attributes.Add("onclick", "oMailing.openRecipientsFilterModal(" + _mailing.Tab + ")");
                labellnkFilter.Attributes.Add("class", "setting-planification");
                RecurrentRequestMode_Filter.Controls.Add(labellnkFilter);

                HtmlGenericControl spanlnkFilter = new HtmlGenericControl("span");
                spanlnkFilter.InnerHtml = eResApp.GetRes(_ePref, 8016);
                labellnkFilter.Controls.Add(spanlnkFilter);


                HtmlGenericControl lnkFilterInfo = new HtmlGenericControl("label");
                lnkFilterInfo.ID = "lnkFilterInfo";
                lnkFilterInfo.InnerText = "";
                lnkFilterInfo.Style.Add(HtmlTextWriterStyle.Display, "none");
                lnkFilterInfo.Style.Add(HtmlTextWriterStyle.Color, "#9c9c9c");
                lnkFilterInfo.Style.Add(HtmlTextWriterStyle.FontStyle, "italic");
                RecurrentRequestMode_Filter.Controls.Add(lnkFilterInfo);

                #endregion

                #endregion
                #endregion
            }
            #endregion

            #endregion

            #region  Onglet où seront enregistrés les couriels envoyés

            divschedule.Controls.Add(BuildSendingOptionsPanel_EmailSelection());
            #endregion
            #endregion

            return divschedule;
        }

        /// <summary>
        /// Bloc de validation du bon à tirer
        /// </summary>
        /// <returns></returns>
        private HtmlGenericControl BuildSendingValidateBon()
        {

            HtmlGenericControl bonATirer = new HtmlGenericControl("div");
            bonATirer.Attributes.Add("class", "blocControleEnvoi");
            bonATirer.Attributes.Add("id", "dvBonATirer");

            #region score
            bonATirer.Controls.Add(BuildScorePanel("mailtester", "0"));
            #endregion

            #region texte
            HtmlGenericControl bonATirerLibelle = new HtmlGenericControl("div");
            bonATirerLibelle.ID = "blocMailTestLibelleSend";
            bonATirerLibelle.Attributes.Add("class", "blocTexteControleEnvoi");
            bonATirerLibelle.InnerText = eResApp.GetRes(_ePref, 2892);
            bonATirer.Controls.Add(bonATirerLibelle);

            HtmlGenericControl bonATirerLibelleResult = new HtmlGenericControl("div");
            bonATirerLibelleResult.ID = "blocMailTestLibelleResult";
            bonATirerLibelleResult.Attributes.Add("class", "blocTexteControleEnvoi");
            bonATirerLibelleResult.InnerText = eResApp.GetRes(_ePref, 2898);
            bonATirerLibelleResult.Style.Add("display", "none");
            bonATirer.Controls.Add(bonATirerLibelleResult);

            HtmlGenericControl bonATirerLibelleResultDate = new HtmlGenericControl("div");
            bonATirerLibelleResultDate.ID = "blocMailTestLibelleResultDate";
            bonATirerLibelleResultDate.Attributes.Add("class", "blocTexteControleEnvoi");
            bonATirerLibelleResultDate.Style.Add("display", "none");
            bonATirer.Controls.Add(bonATirerLibelleResultDate);

            HtmlGenericControl bonATirerLibelleResultDateSpan = new HtmlGenericControl("span");
            bonATirerLibelleResultDateSpan.ID = "blocMailTestLibelleResultDateSpan";
            bonATirerLibelleResultDateSpan.InnerText = "";
            bonATirerLibelleResultDate.Controls.Add(bonATirerLibelleResultDateSpan);
            //MailTesTer bloc
            if (eExtension.IsReadyStrict(_ePref, "QUALITYMAIL", true))
            {
                HtmlGenericControl mailTesterReportWaitMessage = new HtmlGenericControl("span");
                mailTesterReportWaitMessage.ID = "mailTesterReportWaitMessage";
                mailTesterReportWaitMessage.InnerText = eResApp.GetRes(_ePref, 3044);
                mailTesterReportWaitMessage.Style.Add("margin-left", "20px");
                bonATirerLibelleResultDate.Controls.Add(mailTesterReportWaitMessage);

                HtmlGenericControl mailTesterReportlink = new HtmlGenericControl("a");
                mailTesterReportlink.ID = "blocMailTesterReportlink";
                mailTesterReportlink.Attributes.Add("href", "");
                mailTesterReportlink.Attributes.Add("target", "_blank");
                mailTesterReportlink.InnerText = eResApp.GetRes(_ePref, 3025);
                mailTesterReportlink.Style.Add("margin-left", "20px");
                mailTesterReportlink.Style.Add("text-decoration", "underline");
                mailTesterReportlink.Style.Add("color", "blue");
                mailTesterReportlink.Style.Add("display", "none");
                bonATirerLibelleResultDate.Controls.Add(mailTesterReportlink);
            }

            #endregion

            #region mail tester envoi
            HtmlGenericControl bonATirerBtnCtrl = new HtmlGenericControl("div");
            bonATirerBtnCtrl.Attributes.Add("class", "centre");
            bonATirerBtnCtrl.ID = "mailTesterSendContainer";
            bonATirer.Controls.Add(bonATirerBtnCtrl);


            HtmlGenericControl bonATirerBtn = new HtmlGenericControl("input");
            bonATirerBtn.Attributes.Add("type", "button");
            bonATirerBtn.Attributes.Add("value", eResApp.GetRes(_ePref, 2894));
            bonATirerBtn.Attributes.Add("onclick", "oMailing.SendTestMail(1)");
            bonATirerBtn.Attributes.Add("class", "campaingBonATrBtn");
            bonATirerBtn.ID = "campaingBonATrBtn";
            bonATirerBtnCtrl.Controls.Add(bonATirerBtn);


            #endregion

            #region mail tester resultt
            HtmlGenericControl bonATirerBtnValidateCtrl = new HtmlGenericControl("div");
            bonATirerBtnValidateCtrl.Attributes.Add("class", "centre");
            bonATirerBtnValidateCtrl.ID = "mailTesterValidateContainer";
            bonATirerBtnValidateCtrl.Style.Add("display", "none");
            bonATirer.Controls.Add(bonATirerBtnValidateCtrl);
            //Div date


            //btnKO
            HtmlGenericControl bonATirerKOBtn = new HtmlGenericControl("input");
            bonATirerKOBtn.Attributes.Add("type", "button");
            bonATirerKOBtn.Attributes.Add("value", eResApp.GetRes(_ePref, 3064));
            bonATirerKOBtn.Attributes.Add("onclick", "oMailing.ValidDesign(0)");
            bonATirerKOBtn.Attributes.Add("class", "bonATirerKOBtn");
            bonATirerKOBtn.ID = "bonATirerKOBtn";
            bonATirerBtnValidateCtrl.Controls.Add(bonATirerKOBtn);

            //btn ok
            HtmlGenericControl bonATirerOKBtn = new HtmlGenericControl("input");
            bonATirerOKBtn.Attributes.Add("type", "button");
            bonATirerOKBtn.Attributes.Add("value", eResApp.GetRes(_ePref, 3063));
            bonATirerOKBtn.Attributes.Add("onclick", "oMailing.ValidDesign(1)");
            bonATirerOKBtn.Attributes.Add("class", "bonATirerOKBtn");
            bonATirerOKBtn.ID = "bonATirerOKBtn";
            bonATirerBtnValidateCtrl.Controls.Add(bonATirerOKBtn);


            #endregion

            #region mailtester success
            HtmlGenericControl bonATirerBtnCtrlSuccess = new HtmlGenericControl("div");
            bonATirerBtnCtrlSuccess.Attributes.Add("class", "centre");
            bonATirerBtnCtrlSuccess.ID = "mailTesterSuccessContainer";
            bonATirerBtnCtrlSuccess.Style.Add("display", "none");
            bonATirer.Controls.Add(bonATirerBtnCtrlSuccess);


            HtmlGenericControl bonATirerBtnCtrlSuccessLabel = new HtmlGenericControl("div");
            bonATirerBtnCtrlSuccessLabel.ID = "mailTesterSuccessContainerLabel";
            bonATirerBtnCtrlSuccessLabel.InnerHtml = eResApp.GetRes(_ePref, 2900);
            bonATirerBtnCtrlSuccess.Controls.Add(bonATirerBtnCtrlSuccessLabel);

            HtmlGenericControl bonATirerBtnReset = new HtmlGenericControl("input");
            bonATirerBtnReset.ID = "bonATirerBtnReset";
            bonATirerBtnReset.Attributes.Add("type", "button");
            bonATirerBtnReset.Attributes.Add("value", eResApp.GetRes(_ePref, 2901)); //revalider
            bonATirerBtnReset.Attributes.Add("onclick", "oMailing.ResetMailTester()");
            bonATirerBtnReset.Attributes.Add("class", "bonATirerBtnReset");

            bonATirerBtnCtrlSuccess.Controls.Add(bonATirerBtnReset);

            #endregion


            return bonATirer;
        }

        /// <summary>
        /// Bloc type de campagne
        /// </summary>
        /// <returns></returns>
        private HtmlGenericControl BuildCompaignType()
        {
            HtmlGenericControl campaignTypePanel = new HtmlGenericControl("div");
            campaignTypePanel.Attributes.Add("class", "blocCampaignType");
            campaignTypePanel.Attributes.Add("id", "dvCampaignType");

            #region Score
            campaignTypePanel.Controls.Add(BuildScorePanel("campaignType", "0"));
            #endregion

            #region Texte
            HtmlGenericControl bonCampaignTypeTexte = new HtmlGenericControl("div");
            bonCampaignTypeTexte.Attributes.Add("class", "blocTexteCampaignType");
            bonCampaignTypeTexte.InnerText = eResApp.GetRes(_ePref, 2895);
            #endregion
            campaignTypePanel.Controls.Add(bonCampaignTypeTexte);


            #region Type de Media, Type de campagne
            HtmlGenericControl compaignTypeCtrl = new HtmlGenericControl("div");
            compaignTypeCtrl.Attributes.Add("class", "compaignTypeCtrl");

            List<eCatalog.CatalogValue> lsCatalogMediaType = GetEmailCatalgueValues((int)TableType.INTERACTION, (int)InteractionField.MediaType);
            //Type de média                    
            Panel pnlFilterMediaType = new Panel();
            pnlFilterMediaType.ID = "FilterMediaType";
            pnlFilterMediaType.CssClass = "FilterMediaType";
            compaignTypeCtrl.Controls.Add(pnlFilterMediaType);
            if (lsCatalogMediaType.Count > 1)
            {
                pnlFilterMediaType.Controls.Add(new LiteralControl(string.Concat(eResApp.GetRes(Pref, 1839), " "))); //Type de média
            }

            DropDownList selectMediaType = new DropDownList();
            selectMediaType.ID = "campaginSelectMediaType";
            selectMediaType.CssClass = "campaginSelectMediaType";
            selectMediaType.Attributes.Add("onchange", "ChangeMediaTypeSelect(this);");

            if (lsCatalogMediaType.Count > 1)
            {
                selectMediaType.Items.Add(new ListItem(eResApp.GetRes(Pref, 8166), "0")); //Aucune valeur
            }

            foreach (eCatalog.CatalogValue catalogValue in lsCatalogMediaType)
            {
                if (string.IsNullOrEmpty(catalogValue.Data))
                    continue;
                selectMediaType.Items.Add(new ListItem(catalogValue.DisplayValue, catalogValue.Id.ToString()));
            }

            if (_mediaType > 0)
                selectMediaType.SelectedValue = _mediaType.ToString();

            pnlFilterMediaType.Controls.Add(selectMediaType);

            if (lsCatalogMediaType.Count == 1)
            {
                pnlFilterMediaType.Style.Add("display", "none");
            }

            //Type de campagne
            Panel pnlFilterCampaignType = new Panel();
            pnlFilterCampaignType.ID = "FilterCampaignType";
            pnlFilterCampaignType.CssClass = "FilterCampaignType";

            compaignTypeCtrl.Controls.Add(pnlFilterCampaignType);
            pnlFilterCampaignType.Controls.Add(new LiteralControl(string.Concat(eResApp.GetRes(Pref, 8713), " "))); //Type de campagne

            DropDownList selectCampaignType = new DropDownList();
            selectCampaignType.ID = "selectCampaignType";
            selectCampaignType.CssClass = "selectCampaignType";
            selectCampaignType.Attributes.Add("onchange", "FilterTargetCampaign()");
            selectCampaignType.Items.Add(new ListItem(eResApp.GetRes(Pref, 8166), "0")); //Aucune valeur

            if (lsCatalogMediaType.Count == 1 || _mediaType > 0)
            {
                List<eCatalog.CatalogValue> lsCampaigntype = GetCampaignTypeCatalogValues(_mediaType > 0 ? _mediaType : lsCatalogMediaType.First().Id);
                foreach (eCatalog.CatalogValue catalogValue in lsCampaigntype)
                {
                    selectCampaignType.Items.Add(new ListItem(catalogValue.DisplayValue, catalogValue.Id.ToString()));
                }

                if (_campaignType > 0)
                    selectCampaignType.SelectedValue = _campaignType.ToString();
            }

            pnlFilterCampaignType.Controls.Add(selectCampaignType);
            #endregion

            if (_useNewUnsubscribeMethod)
            {
                #region Consentement
                //Status Consentement
                Panel pnlCampaignConsentStatus = new Panel();
                pnlCampaignConsentStatus.ID = "dvCampaignConsentStatus";
                pnlCampaignConsentStatus.CssClass = "dvCampaignConsentStatus";

                compaignTypeCtrl.Controls.Add(pnlCampaignConsentStatus);

                HtmlGenericControl dvCampaignConsentStatusText = new HtmlGenericControl("div");
                dvCampaignConsentStatusText.Attributes.Add("class", "dvCampaignConsentStatusText");
                dvCampaignConsentStatusText.Attributes.Add("id", "dvCampaignConsentStatusText");
                dvCampaignConsentStatusText.InnerHtml = eResApp.GetRes(Pref, 2902);
                pnlCampaignConsentStatus.Controls.Add(dvCampaignConsentStatusText);

                BuildConsentSwitch(pnlCampaignConsentStatus, "campaignSwOptin", eResApp.GetRes(Pref, 2903), _optInEnabled, 0); //Opt-in
                BuildConsentSwitch(pnlCampaignConsentStatus, "campaignSwNoopt", eResApp.GetRes(Pref, 2904), _noConsentEnabled, 0); //Aucun consentement enregistré
                BuildConsentSwitch(pnlCampaignConsentStatus, "campaignSwOptout", eResApp.GetRes(Pref, 2905), _optOutEnabled, 0); //Opt-out   
                if (_mediaType == 0)//On masque la panel de consentement par défaut ou bien s'il n'y pas de campagne type
                    pnlCampaignConsentStatus.Attributes.Add("style", "display:none;");
                #endregion

                campaignTypePanel.Controls.Add(compaignTypeCtrl);

            }
            else
            {
                Panel pnlCampaignConsentStatus = new Panel();
                pnlCampaignConsentStatus.ID = "dvCampaignConsentStatus";
                pnlCampaignConsentStatus.CssClass = "dvCampaignConsentStatus";

                compaignTypeCtrl.Controls.Add(pnlCampaignConsentStatus);

                HtmlGenericControl dvCampaignConsentStatusText = new HtmlGenericControl("div");
                dvCampaignConsentStatusText.Attributes.Add("class", "dvCampaignConsentStatusText");
                dvCampaignConsentStatusText.Attributes.Add("id", "dvCampaignConsentStatusText");
                dvCampaignConsentStatusText.InnerHtml = eResApp.GetRes(Pref, 2909); //Si le consentement n'est pas activé
                pnlCampaignConsentStatus.Controls.Add(dvCampaignConsentStatusText);

                campaignTypePanel.Controls.Add(compaignTypeCtrl);
            }

            return campaignTypePanel;
        }

        /// <summary>
        /// Composant pour le score 
        /// </summary>
        /// <param name="elementId"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private HtmlGenericControl BuildScorePanel(string elementId, string defaultValue)
        {
            HtmlGenericControl elementScore = new HtmlGenericControl("div");
            string _progressClass = "circle-progress";
            if (eTools.IsMSBrowser)
                _progressClass = string.Concat(_progressClass, " ie");
            elementScore.Attributes.Add("class", _progressClass);

            HtmlGenericControl elementScoreSvg = new HtmlGenericControl("svg");
            HtmlGenericControl elementScoreCircle1 = new HtmlGenericControl("circle");
            elementScoreCircle1.Attributes.Add("class", "background-circle");
            elementScoreCircle1.Attributes.Add("cx", "25");
            elementScoreCircle1.Attributes.Add("cy", "25");
            elementScoreCircle1.Attributes.Add("r", "25");
            elementScore.Controls.Add(elementScoreSvg);
            elementScoreSvg.Controls.Add(elementScoreCircle1);
            HtmlGenericControl elementScoreCircle2 = new HtmlGenericControl("circle");
            elementScoreCircle2.Attributes.Add("class", "percent-circle");
            elementScoreCircle2.Attributes.Add("stroke-dasharray", "158");
            elementScoreCircle2.Attributes.Add("stroke-dashoffset", "0");
            elementScoreCircle2.Attributes.Add("cx", "25");
            elementScoreCircle2.Attributes.Add("cy", "25");
            elementScoreCircle2.Attributes.Add("r", "25");
            elementScoreSvg.Controls.Add(elementScoreCircle2);

            HtmlGenericControl elementScoreDiv = new HtmlGenericControl("div");
            elementScoreDiv.Attributes.Add("class", "number");
            HtmlGenericControl elementScoreSpn = new HtmlGenericControl("span");
            elementScoreSpn.Attributes.Add("class", "progress-val");
            elementScoreSpn.Attributes.Add("id", string.Concat(elementId, "ScoreValue"));
            elementScoreSpn.InnerHtml = defaultValue;
            elementScoreDiv.Controls.Add(elementScoreSpn);

            elementScore.Controls.Add(elementScoreDiv);

            return elementScore;
        }

        /// <summary>
        /// Bloc Nombre de destinataires
        /// </summary>
        /// <returns></returns>
        private HtmlGenericControl BuildRecepientCountBloc()
        {
            HtmlGenericControl recepientCountPanel = new HtmlGenericControl("div");
            recepientCountPanel.Attributes.Add("class", "blocRecepientCount");
            recepientCountPanel.Attributes.Add("id", "dvRecepientCount");

            #region Score
            recepientCountPanel.Controls.Add(BuildScorePanel("recepientCount", "0"));
            #endregion

            #region Texte
            HtmlGenericControl recepientCountTexte = new HtmlGenericControl("div");
            recepientCountTexte.Attributes.Add("class", "blocTexteRecepientCount");
            recepientCountTexte.InnerText = eResApp.GetRes(_ePref, 2937);
            #endregion
            recepientCountPanel.Controls.Add(recepientCountTexte);


            #region dedoublonnage
            //Status dedoublonnage
            Panel pnlCampaignDedoublonnageStatus = new Panel();
            pnlCampaignDedoublonnageStatus.ID = "dvCampaignDedoublonnage";
            pnlCampaignDedoublonnageStatus.CssClass = "dvCampaignDedoublonnage";

            //Si Mapp Digital est actif alors ne pas afficher le switch et forcer le dédoublonnage. 
            bool isMappActivated = false;
            eExtension eRegisteredExt = eExtension.GetExtensionByCode(Pref, Internal.eLibConst.EXTENSION.EXTERNALMAILING.ToString());
            if (eRegisteredExt != null && eRegisteredExt.Status == EXTENSION_STATUS.STATUS_READY)
            {
                isMappActivated = true;
                _noRemoveDoubleEnable = true;
            }
            //switch dedoublonnage
            BuildConsentSwitch(pnlCampaignDedoublonnageStatus, "campaignSwDedoublonnage", eResApp.GetRes(Pref, 2938), _noRemoveDoubleEnable, -1, isMappActivated); //RemoveDoubleEnable

            #endregion
            recepientCountPanel.Controls.Add(pnlCampaignDedoublonnageStatus);

            #region dedoublonnage result
            //Status dedoublonnage
            Panel pnlCampaignDedoublonnageResult = new Panel();
            pnlCampaignDedoublonnageResult.CssClass = "dvCampaignDedoublonnage";
            //result dedoublonnage
            //mail to Send Number
            HtmlGenericControl dvTotalRecipientAfterRemoveDoublon = new HtmlGenericControl("div");
            dvTotalRecipientAfterRemoveDoublon.Attributes.Add("id", "totalAfterRemoveDoublonContainer");
            dvTotalRecipientAfterRemoveDoublon.Attributes.Add("class", "totalAfterRemoveDoublonContainer doublonContainerCounter");
            pnlCampaignDedoublonnageResult.Controls.Add(dvTotalRecipientAfterRemoveDoublon);

            HtmlGenericControl spnTotalRecipientAfterRemoveDoublontValue = new HtmlGenericControl("span");
            spnTotalRecipientAfterRemoveDoublontValue.Attributes.Add("id", "spnTotalRecipientAfterRemoveDoublontValue");
            spnTotalRecipientAfterRemoveDoublontValue.Style.Add("color", "green");
            spnTotalRecipientAfterRemoveDoublontValue.InnerHtml = "0";
            dvTotalRecipientAfterRemoveDoublon.Controls.Add(spnTotalRecipientAfterRemoveDoublontValue);

            HtmlGenericControl spnTotalRecipientAfterRemoveDoublontText = new HtmlGenericControl("span");
            spnTotalRecipientAfterRemoveDoublontText.Attributes.Add("class", "spnToTalRecepientText");
            spnTotalRecipientAfterRemoveDoublontText.InnerHtml = eResApp.GetRes(_ePref, 2939);
            dvTotalRecipientAfterRemoveDoublon.Controls.Add(spnTotalRecipientAfterRemoveDoublontText);

            //Total Recipient
            HtmlGenericControl dvTotalRecipient = new HtmlGenericControl("div");
            dvTotalRecipient.Attributes.Add("id", "totalWithoutRemoveDoublonContainer");
            dvTotalRecipient.Attributes.Add("class", "totalWithoutRemoveDoublonContainer doublonContainerCounter");
            pnlCampaignDedoublonnageResult.Controls.Add(dvTotalRecipient);

            HtmlGenericControl spnToTalRecepientValue = new HtmlGenericControl("span");
            spnToTalRecepientValue.Attributes.Add("id", "spnToTalRecepientValue");
            spnToTalRecepientValue.Style.Add("color", "green");
            spnToTalRecepientValue.InnerHtml = "0";
            dvTotalRecipient.Controls.Add(spnToTalRecepientValue);

            HtmlGenericControl spnToTalRecepientText = new HtmlGenericControl("span");
            spnToTalRecepientText.Attributes.Add("class", "spnToTalRecepientText");
            spnToTalRecepientText.InnerHtml = eResApp.GetRes(_ePref, 2940);
            dvTotalRecipient.Controls.Add(spnToTalRecepientText);
            #endregion
            recepientCountPanel.Controls.Add(pnlCampaignDedoublonnageResult);

            return recepientCountPanel;
        }

        /// <summary>
        /// Bloc Quality EmailAdresses
        /// </summary>
        /// <returns></returns>
        private HtmlGenericControl BuildQualityEmailAdresse()
        {
            HtmlGenericControl qualityEmailAdressesPanel = new HtmlGenericControl("div");
            qualityEmailAdressesPanel.Attributes.Add("class", "blocQualityEmailAdresses");
            qualityEmailAdressesPanel.Attributes.Add("id", "dvQualityEmailAdresses");


            #region Score
            qualityEmailAdressesPanel.Controls.Add(BuildScorePanel("qualityEmailAdresses", "0"));
            #endregion

            #region Texte
            HtmlGenericControl bonQualityEmailAdressesTexte = new HtmlGenericControl("div");
            bonQualityEmailAdressesTexte.Attributes.Add("class", "blocTexteQualityEMailAdresses");
            bonQualityEmailAdressesTexte.InnerText = eResApp.GetRes(_ePref, 2915);
            qualityEmailAdressesPanel.Controls.Add(bonQualityEmailAdressesTexte);
            #endregion

            #region Switch 
            Panel pnldresseEmailSwitch = new Panel();
            pnldresseEmailSwitch.ID = "dvAdresseEmailSwitch";
            pnldresseEmailSwitch.CssClass = "dvAdresseEmailSwitch";

            BuildConsentSwitch(pnldresseEmailSwitch, "qualityAdressEmailSwValide", eResApp.GetRes(Pref, 2916), _adressStatusParam.ValidAdress, 0); //valide
            BuildConsentSwitch(pnldresseEmailSwitch, "qualityAdressEmailSwNotVerified", eResApp.GetRes(Pref, 2917), _adressStatusParam.NotVerifiedAdress, 0); //non vérifiée
            BuildConsentSwitch(pnldresseEmailSwitch, "qualityAdressEmailSwInvalide", eResApp.GetRes(Pref, 2918), _adressStatusParam.InvalidAdress, 0); //email invalide            

            #endregion
            qualityEmailAdressesPanel.Controls.Add(pnldresseEmailSwitch);
            return qualityEmailAdressesPanel;
        }

        /// <summary>
        /// Permet de créer un switch panel 
        /// </summary>
        /// <param name="pnlParent">Panel parent</param>
        /// <param name="id">id du checkbox et du label</param>
        /// <param name="label">Libellé</param>
        /// <param name="enabled">coché par défaut</param>
        /// <param name="nbrDest">nombre de destinataire</param>
        /// <param name="hideElement"></param>
        private void BuildConsentSwitch(Panel pnlParent, string id, string label, bool enabled, int nbrDest, bool hideElement = false)
        {
            HtmlGenericControl dvSwitch = new HtmlGenericControl("div");
            if (!string.IsNullOrEmpty(id))
                dvSwitch.Attributes.Add("id", string.Concat(id, "Container"));
            if (hideElement)
                dvSwitch.Attributes.Add("style", "display:none;");
            string switchClass = "dvConsentStatusSwitch switch-new-theme-wrap ";
            dvSwitch.Attributes.Add("class", switchClass);
            pnlParent.Controls.Add(dvSwitch);

            HtmlGenericControl inptSwitch = new HtmlGenericControl("input");
            inptSwitch.Attributes.Add("id", string.Concat("sw", id));
            inptSwitch.Attributes.Add("type", "checkbox");
            inptSwitch.Attributes.Add("class", "switch-new-theme");
            if (enabled)
                inptSwitch.Attributes.Add("checked", "true");
            inptSwitch.Attributes.Add("onclick", string.Concat("oMailing.ChangeSwitch(this,'", id, "');"));
            dvSwitch.Controls.Add(inptSwitch);

            HtmlGenericControl lblSwitch = new HtmlGenericControl("label");
            lblSwitch.Attributes.Add("class", "lbSwitch");
            lblSwitch.Attributes.Add("onclick", string.Concat("oMailing.ChangeSwitch(this,'", id, "');"));
            lblSwitch.Attributes.Add("htmlFor", string.Concat("sw", id));
            lblSwitch.Attributes.Add("title", label);
            dvSwitch.Controls.Add(lblSwitch);

            HtmlGenericControl spnSwitch = new HtmlGenericControl("span");
            spnSwitch.Attributes.Add("id", string.Concat("spnNw", id));
            spnSwitch.Style.Add("color", "black");
            spnSwitch.Attributes.Add("title", label);
            spnSwitch.InnerHtml = label;
            dvSwitch.Controls.Add(spnSwitch);

            if (nbrDest >= 0)//Dans le cas ou le nombre est négatif, on n'ajoute pas le nombre
            {
                HtmlGenericControl spnNbrDest = new HtmlGenericControl("span");
                spnNbrDest.Attributes.Add("id", string.Concat("spnNbrDest", id));
                spnNbrDest.Attributes.Add("class", "spnNbrDest");
                spnNbrDest.InnerHtml = nbrDest.ToString();
                dvSwitch.Controls.Add(spnNbrDest);
            }
        }

        /// <summary>
        /// Bloc score total de la dévrabilité
        /// </summary>
        /// <returns></returns>
        private HtmlGenericControl BuildDelivrabilityScore()
        {
            HtmlGenericControl delivrabilityScore = new HtmlGenericControl("div");
            delivrabilityScore.Attributes.Add("class", "blocDelivrabilityScore progress-wrapper");
            delivrabilityScore.Attributes.Add("id", "dvDelivrabilityScore");


            HtmlGenericControl delivrabilityScoreSpanLabel = new HtmlGenericControl("label");
            delivrabilityScore.Controls.Add(delivrabilityScoreSpanLabel);
            delivrabilityScoreSpanLabel.Attributes.Add("for", "file-progress");
            delivrabilityScoreSpanLabel.InnerHtml = eResApp.GetRes(_ePref, 2896) + "&nbsp;:&nbsp;";


            HtmlGenericControl delivrabilityScoreSpanValue = new HtmlGenericControl("span");
            delivrabilityScoreSpanValue.ID = "delivrabilityScoreSpanValue";
            delivrabilityScoreSpanValue.Attributes.Add("class", "progress-value");
            delivrabilityScoreSpanValue.InnerHtml = "0";
            delivrabilityScoreSpanLabel.Controls.Add(delivrabilityScoreSpanValue);

            HtmlGenericControl delivrabilityScoreSpanValueOver = new HtmlGenericControl("span");
            delivrabilityScoreSpanValueOver.InnerHtml = eResApp.GetRes(_ePref, 2897);
            delivrabilityScoreSpanLabel.Controls.Add(delivrabilityScoreSpanValueOver);

            HtmlGenericControl delivrabilityScoreProgress = new HtmlGenericControl("progress");
            delivrabilityScore.Controls.Add(delivrabilityScoreProgress);
            delivrabilityScoreProgress.Attributes.Add("id", "file-progress");
            delivrabilityScoreProgress.Attributes.Add("max", "100");

            return delivrabilityScore;
        }

        /// <summary>
        /// Bloc Option - Partie Envoi programmer
        /// </summary>
        /// <returns></returns>
        private WebControl BuildSendingOptionsPanel_AdvancedOptions()
        {
            eUlCtrl ulAdvOptionsMail = new eUlCtrl();
            if (Mailing.SendeType != MAILINGSENDTYPE.EUDONET)
            {
                ulAdvOptionsMail.Attributes.Add("style", "display:none;");
                return ulAdvOptionsMail;
            }

            ulAdvOptionsMail = new eUlCtrl();
            ulAdvOptionsMail.Attributes.Add("class", "mailingDelayedMail");

            eLiCtrl liLine = ulAdvOptionsMail.AddLi();
            liLine.Attributes.Add("class", "mailingSeparator");

            // Autres
            HtmlGenericControl line = new HtmlGenericControl("hr"); liLine.Controls.Add(line);

            eLiCtrl liAdvOptionsTitle = ulAdvOptionsMail.AddLi();

            //Label Programmer votre envoi
            eLabelCtrl scheduleSending = new eLabelCtrl();
            liAdvOptionsTitle.Controls.Add(scheduleSending);
            scheduleSending.CssClass = "labelTitle";
            scheduleSending.InnerText = eResApp.GetRes(Pref, 6801); //"Sélectionner les options d'envoi :"

            eCheckBoxCtrl checkbox = new eCheckBoxCtrl(Mailing.GetParamValue("removeDoubles") == "1", false);
            checkbox.AddText(eResApp.GetRes(Pref, 1699));
            checkbox.AddClick("oMailing.SetParam('removeDoubles', getAttributeValue(this,'chk'));");
            checkbox.AddClass("rdmailing");

            liLine = ulAdvOptionsMail.AddLi();
            liLine.Controls.Add(checkbox);
            ulAdvOptionsMail.Controls.Add(liLine);

            return ulAdvOptionsMail;
        }

        /// <summary>
        /// Bloc Option - Partie Choix de la table E-Mail
        /// </summary>
        /// <returns></returns>
        private HtmlGenericControl BuildSendingOptionsPanel_EmailSelection()
        {

            HtmlGenericControl divEmailSelection = new HtmlGenericControl("dif");
            divEmailSelection.Attributes.Add("class", "field-container");

            //MOU Choix email
            HtmlGenericControl labelChoiceTableEmail = new HtmlGenericControl("label");
            divEmailSelection.Controls.Add(labelChoiceTableEmail);
            labelChoiceTableEmail.Attributes.Add("for", "name");
            labelChoiceTableEmail.InnerHtml = eResApp.GetRes(Pref, 6408); // Choix table Email

            HtmlSelect EmailFileSelect = new HtmlSelect();
            divEmailSelection.Controls.Add(EmailFileSelect);

            #region Remplissage du EmailFileSelect

            if (FillEmailFiles(EmailFileSelect))
            {
                int nMailTab = 0;
                Int32.TryParse(Mailing.GetParamValue("mailTabDescId"), out nMailTab);

                if (nMailTab == 0)
                    EmailFileSelect.SelectedIndex = 0;
                else
                    //Si on a précedement sélectionné un sous-fichier email 
                    EmailFileSelect.Value = EmailFileSelect.Items.FindByValue(Mailing.GetParamValue("mailTabDescId")).Value;

                Mailing.SetParamValue("mailTabDescId", EmailFileSelect.Value.ToString());
            }


            #endregion

            EmailFileSelect.Attributes.Add("onchange", "oMailing.SetParam('mailTabDescId', this.value);");

            if (EmailFileSelect.Items.Count == 0)
            {
                this._sErrorMsg = "Erreur, pas de table mail définie ou pas de droit de modifs";
                _nErrorNumber = QueryErrorType.ERROR_NUM_MAIL_FILE_NOT_FOUND;

            }

            //s'il y a un seul fichier E-mail, on n'affiche pas l'option
            if (EmailFileSelect.Items.Count == 1)
            {
                divEmailSelection.Style.Add("display", "none");

            }



            //HtmlGenericControl labelChoiceTableEmail = new HtmlGenericControl("label"); 
            //liChoiceTableEmail.Controls.Add(labelChoiceTableEmail);
            return divEmailSelection;
        }

        /// <summary>
        /// Remplit le select par les fichiers Email
        /// </summary>
        /// <param name="select"></param>
        protected Boolean FillEmailFiles(HtmlSelect select)
        {
            String err;
            if (eTools.FillEmailFiles(Pref, select, out err))
                return true;


            _eException = new Exception(err);
            return false;
        }

        /// <summary>
        /// Construit le corps de page de l'étape de sélection des champs du rapport
        /// </summary>
        /// <returns>Panel de type DIV contenant le code HTML de l'étape</returns>
        protected virtual Panel BuildSelectFieldsPanel()
        {

            #region Initialisation

            Int32 _nCount = this._mailingWizard.CurrentListCount;
            Int32 _nCheckedCount = this._mailingWizard.CurrentCheckedListCount;
            Int32 _nbMailAddress = this._mailing.EmailFields.Count;
            Boolean bHasCurrentListMultipleAddress = this._mailingWizard.HasCurrentListMultipleAddress;
            Boolean bHasMarkedListMultipleAddress = this._mailingWizard.HasCheckedListMultipleAddress;
            Boolean bIsAnInvitation = this._mailingWizard.bIsInvit;
            Boolean bProspectEnabled = this._mailingWizard.bProspectEnabled;

            //Container de l'étape
            Panel pFieldSelectDiv = new Panel();
            pFieldSelectDiv.ID = "divSelectMailAdr";
            pFieldSelectDiv.CssClass = "mailing_container";

            #endregion

            //Block destinataire
            CreateContainerBlock(pFieldSelectDiv, "ul",
                delegate (ref HtmlGenericControl container)
                {
                    #region Vérification des conditions du rendu

                    /*
                     *   On ne fait pas du rendu de block dans les cas suivants :                     
                     *      1- Pas de fiches marquées ou pas de fiche en cours
                     *      2- Nombre de fiche marquées est egale au nombre de fiche de la liste en cours                     * 
                     */

                    if (_nCheckedCount == 0 || _nCount == 0 || _nCheckedCount == _nCount)
                    {
                        container = null;
                        return;
                    }
                    #endregion

                    #region Contenu du block

                    Dictionary<int, string> listRes = eLibTools.GetRes(Pref, String.Concat(_tab, ";", TableType.ADR.GetHashCode()), Pref.Lang, out _sErrorMsg);

                    //Titre Confirmez les destinataires
                    CreateHeaderLine(eResApp.GetRes(Pref, 6392), container);

                    //Uniquement les fiches cochées
                    CreateContentLine(eResApp.GetRes(Pref, 6393), container,
                    delegate (HtmlGenericControl line, HtmlGenericControl input, HtmlGenericControl label)
                    {
                        #region Personnalisation du rendu de la ligne

                        //radio
                        input.Attributes.Add("type", "radio");
                        input.Attributes.Add("name", "confirmRecipients");
                        input.Attributes.Add("value", "onlychecked");
                        input.Attributes.Add("id", "confirmRecipients_onlyChecked");
                        input.Attributes.Add("onclick",
                            new StringBuilder()
                            .Append("oMailing.SetParam('markedFiles','1');")
                            .Append("oMailing.SetParam('nMailCount',").Append(_nCheckedCount).Append(");")
                            .Append("Display('liMsgInfo', ").Append(bHasMarkedListMultipleAddress.ToString().ToLower()).Append(");")
                            .ToString());

                        //Par défaut c'est "fiches marque" qui cochée
                        input.Attributes.Add("checked", "checked");

                        //label
                        label.Attributes.Add("for", input.Attributes["id"]);
                        label.InnerHtml = string.Concat(label.InnerHtml, " (", _nCheckedCount, ")"); // Fiches cochées 

                        #endregion
                    });

                    // <TAB> en cours
                    CreateContentLine(eResApp.GetRes(Pref, 6394), container,
                    delegate (HtmlGenericControl line, HtmlGenericControl input, HtmlGenericControl label)
                    {
                        #region Personnalisation du rendu de la ligne

                        //radio
                        input.Attributes.Add("type", "radio");
                        input.Attributes.Add("name", "confirmRecipients");
                        input.Attributes.Add("value", "currentlist");
                        input.Attributes.Add("id", "confirmRecipients_currentList");
                        input.Attributes.Add("onclick", new StringBuilder()
                                                     .Append("oMailing.SetParam('markedFiles','0');")
                                                     .Append("oMailing.SetParam('nMailCount',").Append(_nCount).Append(");")
                                                     .Append("Display('liMsgInfo', ").Append(bHasCurrentListMultipleAddress.ToString().ToLower()).Append(");")
                                                     .ToString());

                        //label
                        label.Attributes.Add("for", input.Attributes["id"]);
                        label.InnerHtml = string.Concat(label.InnerText.Replace("<TAB>", listRes[_tab]), " (", _nCount, ")"); // Fiches en cours
                        #endregion
                    });

                    //Votre sélection contient des adresses multiples ...
                    CreateContentLine(eResApp.GetRes(Pref, 6453), container,
                    delegate (HtmlGenericControl line, HtmlGenericControl input, HtmlGenericControl label)
                    {
                        #region Personnalisation du rendu de la ligne

                        //On a pas besoin de radio pour ce cas, juste une information
                        line.Controls.Remove(input);
                        input = null;

                        //on definit l'id et on ajout de class css (IMPORTANT : concatener avec l'existant)
                        line.Attributes.Add("id", "liMsgInfo");
                        line.Attributes.Add("class", string.Concat(line.Attributes["class"], " ", "mail-alert-warn"));

                        //ajout de la class css pour décaler l'image
                        label.Attributes.Add("class", string.Concat(label.Attributes["class"], " ", "mail-label-warn"));

                        label.InnerHtml = label.InnerText.Replace("<TAB>", listRes[_tab])
                                                     .Replace("<ADR>", listRes[TableType.ADR.GetHashCode()]);
                        label.InnerHtml = String.Concat("<img class='mail-logo-warn' src='ghost.gif'/>", label.InnerHtml);

                        //Le message est affiché que si on de multiple address
                        if (!bHasMarkedListMultipleAddress)
                            line.Attributes.Add("style", "display:none;");

                        #endregion
                    });

                    #endregion

                });



            if (_bOnHold)
            {
                CreateContainerBlock(pFieldSelectDiv, "ul",

                    delegate (ref HtmlGenericControl container)
                    {
                        CreateTextLine(eResApp.GetRes(Pref, 2767), container, sCss: "");
                    });
            }

            //Block des adresses mail
            CreateContainerBlock(pFieldSelectDiv, "ul",
                delegate (ref HtmlGenericControl container)
                {
                    #region Vérification des conditions du rendu

                    /***
                     * On n'affiche pas le block dans ces cas :
                     *  1 - S'il n' y a qu'une seule adresse mail 
                     *  
                     ***/
                    Int32 descid;
                    if (Int32.TryParse(_mailing.GetParamValue("mailFieldDescId"), out descid))
                    {
                        if (_mailing.EmailFields.Count == 1)
                        {
                            container = null;
                            return;
                        }
                    }

                    #endregion

                    #region Contenu du block

                    //Titre sèlectionnez l'adresse email qui sera utiliser lors de l'envoi
                    CreateHeaderLine(eResApp.GetRes(Pref, 6395), container);

                    //Ajout des adresses mail
                    CreateMultipleLines(container, _mailing.EmailFields,
                    delegate (KeyValuePair<int, string> item, HtmlGenericControl line, HtmlGenericControl input, HtmlGenericControl label)
                    {
                        #region Personnalisation du rendu pour chaque entrée du dictionnaire
                        Int32 nTab = eLibTools.GetTabFromDescId(item.Key);
                        //Radio
                        input.Attributes.Add("type", "radio");
                        input.Attributes.Add("name", "mailSelect");
                        input.Attributes.Add("id", "rdo_" + item.Key);
                        input.Attributes.Add("onclick", String.Concat("oMailing.SetMailField('", item.Key, "');"));

                        //On coche le radio s'il est dans le dictionnaire des params
                        if (_mailing.MailingParams.ContainsKey("mailFieldDescId") && _mailing.MailingParams["mailFieldDescId"].Equals(item.Key.ToString()))
                            input.Attributes.Add("checked", "checked");

                        //Libellé de la ligne
                        label.InnerHtml = item.Value;
                        label.Attributes.Add("for", input.Attributes["id"]);

                        #endregion
                    });

                    #endregion
                });

            //Block "ne retenir que les adresses principales ou actives"            
            CreateContainerBlock(pFieldSelectDiv, "ul",
                delegate (ref HtmlGenericControl container)
                {
                    #region Vérification des conditions du rendu

                    /*
                    *   On ne fait pas du rendu de block dans les cas suivants :                     
                    *      1- On vient pas de PP/PM #26967
                    *      2- si on vient de Cible etendu ou Invitations
                    */

                    //Pas de rendu pour cible etendu ou Invitation
                    if (bProspectEnabled || bIsAnInvitation)
                    {
                        container = null;
                        return;
                    }

                    //Pas de rendu pour les liste hors pp/pm
                    if (_tab != TableType.PP.GetHashCode() && _tab != TableType.PM.GetHashCode())
                    {
                        container = null;
                        return;
                    }

                    #endregion

                    #region Contenu du block
                    //Titre ne : Ne retenir que les fiches adresses ...
                    CreateHeaderLine(eResApp.GetRes(Pref, 156), container,
                    delegate (HtmlGenericControl line, HtmlGenericControl label)
                    {
                        #region Personnalisation du rendu de la ligne

                        //On vire la class par défaut
                        line.Attributes["class"] = String.Empty;
                        line.Attributes.Add("id", "mainActive-container");

                        //#29350                    
                        eRes res = new eRes(Pref, String.Concat(TableType.ADR.GetHashCode(), ",", AdrField.PRINCIPALE.GetHashCode(), ",", AdrField.ACTIVE.GetHashCode()));

                        bool bFoundOk = false;
                        label.InnerHtml = label.InnerText.Replace("<PREFNAME>", res.GetRes(TableType.ADR.GetHashCode(), out bFoundOk));

                        bFoundOk = false;
                        eCheckBoxCtrl ckAddressMain = new eCheckBoxCtrl(false, false);
                        ckAddressMain.AddText(res.GetRes(AdrField.PRINCIPALE.GetHashCode(), out bFoundOk));
                        ckAddressMain.ID = "selectMailAddress_main";
                        ckAddressMain.AddClick("oMailing.SetParam('mainAdress',  this.attributes[\"chk\"].value);");
                        ckAddressMain.AddClass("adr-main-active");

                        line.Controls.Add(ckAddressMain);

                        bFoundOk = false;
                        eCheckBoxCtrl ckAddressActive = new eCheckBoxCtrl(false, false);
                        ckAddressActive.AddText(res.GetRes(AdrField.ACTIVE.GetHashCode(), out bFoundOk));
                        ckAddressActive.ID = "selectMailAddress_mainActive";
                        ckAddressActive.AddClick("oMailing.SetParam('activeAdress',  this.attributes[\"chk\"].value);");

                        line.Controls.Add(ckAddressActive);
                        #endregion
                    });
                    #endregion
                });

            return pFieldSelectDiv;
        }

        /// <summary>
        /// Redifinition du delegate Action afin qu'elle passe le seule argment en paramètre
        /// </summary>
        /// <typeparam name="TKey">Type de l'argmenet</typeparam>
        /// <param name="container">Argent de la fonction</param>
        public delegate void Action<TKey>(ref TKey container);

        /// <summary>
        /// Créee un container enfant et l'ajoute au container parent
        /// </summary>
        /// <param name="container"> Control parent</param>
        /// <param name="innerContainerTag">Tag html</param>
        /// <param name="customizer">délegate pour personaliser le container enfant</param>
        /// <returns>Container enfant</returns>
        private HtmlGenericControl CreateContainerBlock(Control container, string innerContainerTag,
                                    Action<HtmlGenericControl> customizer = null)
        {
            HtmlGenericControl innerContainer = new HtmlGenericControl(innerContainerTag);

            // On donne la possibilité à l'appelant de personaliser la ligne le block         
            if (customizer != null)
                customizer.Invoke(ref innerContainer);

            //on ajoute le controle que si le container parent et le control fils ne sont pas nuls
            if (container != null && innerContainer != null)
                container.Controls.Add(innerContainer);

            return innerContainer;
        }

        /// <summary>
        /// Crée un titre de block 
        /// </summary>
        /// <param name="displayTitle">Le titre à afficher</param>
        /// <param name="container">Le control parent</param>
        /// <param name="customizer">délegate pour personaliser les controls de la ligne</param>
        private void CreateHeaderLine(String displayTitle, HtmlGenericControl container,
                                    Action<HtmlGenericControl, HtmlGenericControl> customizer = null)
        {
            //Pas de père,  pas de fils
            if (container == null)
                return;

            //Creation d'une ligne title
            CreateContentLine(displayTitle, container,
                delegate (HtmlGenericControl line, HtmlGenericControl input, HtmlGenericControl label)
                {
                    #region On personnaliser le rendu de la ligne header

                    line.Controls.Remove(input);
                    input = null;

                    line.Attributes["class"] = "mailing_Header";

                    if (customizer != null)
                        customizer.Invoke(line, label);

                    #endregion
                });
        }

        /// <summary>
        /// Parcourt la collection et pour chaque element on fait un rendu de type ligne 
        /// </summary>
        /// <param name="container">Le control parent</param>
        /// <param name="collection"> Collection d'element à afficher en ligne</param>
        /// <param name="customizer">délegate pour personaliser les controls de la ligne</param>
        private void CreateMultipleLines<TKey>(HtmlGenericControl container, IEnumerable<TKey> collection,
                                    Action<TKey, HtmlGenericControl, HtmlGenericControl, HtmlGenericControl> customizer = null)
        {
            if (container == null)
                return;

            foreach (TKey item in collection)
            {
                //Creation d'une ligne corespond à l'element courant de la collection
                CreateContentLine(String.Empty, container,
                    delegate (HtmlGenericControl line, HtmlGenericControl input, HtmlGenericControl label)
                    {
                        #region On passe la main à l'apppelant pour personnaliser le rendu de la ligne

                        if (customizer != null)
                            customizer.Invoke(item, line, input, label);

                        #endregion
                    });
            }
        }

        /// <summary>
        /// Construit une ligne contenant une li avec un couple(input, label), on peut utiliser customizer pour personaliser
        /// l'ordre ou ajouter des controls specifiques à la ligne
        /// </summary>
        /// <param name="displayText">Text a afficher à coté de l'input</param>
        /// <param name="container">le control parent</param>
        /// <param name="customizer">délegate pour personaliser les controls de la ligne</param>
        /// <param name="sCss">css </param>
        private void CreateContentLine(String displayText, HtmlGenericControl container,
                                    Action<HtmlGenericControl, HtmlGenericControl, HtmlGenericControl> customizer = null, string sCss = "mailing_Opt1")
        {
            //Pas de père,  pas de fils
            if (container == null)
                return;

            HtmlGenericControl line = new HtmlGenericControl("li");
            container.Controls.Add(line);

            HtmlGenericControl input = new HtmlGenericControl("input");
            line.Controls.Add(input);

            HtmlGenericControl label = new HtmlGenericControl("label");
            line.Controls.Add(label);

            label.InnerHtml = HttpUtility.HtmlEncode(displayText);
            if (!string.IsNullOrEmpty(sCss))
                line.Attributes.Add("class", "");

            //On donne la possibilité à l'appelant de  personnaliser la ligne           
            if (customizer != null)
                customizer.Invoke(line, input, label);
        }


        /// <summary>
        /// Construit une ligne contenant une li avec un label, on peut utiliser customizer pour personaliser
        /// l'ordre ou ajouter des controls specifiques à la ligne
        /// </summary>
        /// <param name="displayText">Text a afficher à coté de l'input</param>
        /// <param name="container">le control parent</param>
        /// <param name="customizer">délegate pour personaliser les controls de la ligne</param>
        /// <param name="sCss">css </param>
        private void CreateTextLine(String displayText, HtmlGenericControl container,
                                    Action<HtmlGenericControl, HtmlGenericControl> customizer = null, string sCss = "")
        {
            //Pas de père,  pas de fils
            if (container == null)
                return;

            HtmlGenericControl line = new HtmlGenericControl("li");
            container.Controls.Add(line);

            HtmlGenericControl label = new HtmlGenericControl("label");
            line.Controls.Add(label);

            label.InnerHtml = HttpUtility.HtmlEncode(displayText);
            if (!string.IsNullOrEmpty(sCss))
                line.Attributes.Add("class", "");

            //On donne la possibilité à l'appelant de  personnaliser la ligne           
            if (customizer != null)
                customizer.Invoke(line, label);
        }

        /// <summary>
        /// Construit le corps de page de l'étape de choix de modele d'emailing
        /// </summary>
        /// <returns>Panel de type DIV contenant le code HTML de l'étape</returns>
        protected Panel BuildSelectTemplatesPanel()
        {
            Panel container = new Panel();
            container.ID = "mainDiv";
            container.CssClass = "template_container";
            container.Attributes.Add("ednType", "mailing");
            container.Attributes.Add("filterTab", _iTab.ToString());

            if (_bOnHold && _iTotalStep == 4)
            {
                CreateContainerBlock(container, "ul",

                    delegate (ref HtmlGenericControl container2)
                    {
                        CreateTextLine(eResApp.GetRes(Pref, 2767), container2, sCss: "");
                    });
            }

            HtmlGenericControl ul = null;
            HtmlGenericControl li = null;



            #region  Modèles enregistrés ou modèles personnalisables
            ul = new HtmlGenericControl("ul");
            ul.Attributes.Add("class", "templates-select");

            container.Controls.Add(ul);

            li = new HtmlGenericControl("li");
            ul.Controls.Add(li);




            RadioButton rbCustomTemplates = new RadioButton();
            li.Controls.Add(rbCustomTemplates);

            rbCustomTemplates.ID = "rbCustomTemplates";
            rbCustomTemplates.GroupName = "gnTemplates";
            rbCustomTemplates.Text = eResApp.GetRes(Pref, 6405);
            rbCustomTemplates.Attributes.Add("onclick", "oTemplate.DisplayBlock('0');");
            if (_mailing.MailingParams["templateType"] == "0")
            {
                rbCustomTemplates.Checked = true;
            }


            li = new HtmlGenericControl("li");
            ul.Controls.Add(li);

            if (_dicEmailAdvConfig == null || _dicEmailAdvConfig[eLibConst.CONFIGADV.EXTERNAL_TRACKING_ENABLED] != "1")
            {
                RadioButton rbMyTemplates = new RadioButton();
                li.Controls.Add(rbMyTemplates);

                rbMyTemplates.ID = "rbMyTemplates";
                rbMyTemplates.GroupName = "gnTemplates";
                rbMyTemplates.Text = eResApp.GetRes(Pref, 6404);
                rbMyTemplates.Attributes.Add("onclick", "oTemplate.DisplayBlock('1');");
                if (_mailing.MailingParams["templateType"] == "1")
                {
                    rbMyTemplates.Checked = true;
                }
            }


            li = new HtmlGenericControl("li");
            ul.Controls.Add(li);

            RadioButton rbNoTemplates = new RadioButton();
            li.Controls.Add(rbNoTemplates);

            rbNoTemplates.ID = "rbNoTemplate";
            rbNoTemplates.GroupName = "gnTemplates";
            rbNoTemplates.Text = eResApp.GetRes(Pref, 6406);
            rbNoTemplates.Attributes.Add("onclick", "oTemplate.DisplayBlock('2');");
            if (_mailing.MailingParams["templateType"] == "2")
            {
                rbNoTemplates.Checked = true;
            }

            #endregion

            #region Modèles personalisables

            HtmlGenericControl div = new HtmlGenericControl("div");
            container.Controls.Add(div);
            div.ID = "divCustomTemplates";
            if (_mailing.MailingParams["templateType"] != "0")
            {
                div.Attributes.Add("style", "display:none;");
            }

            #region Filter Eudonet Templates

            eMailingTemplateSettings mailingTemplateSettings = null;
            using (StreamReader r = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "\\Emailing\\templates.json"))
            {
                try
                {
                    string json = r.ReadToEnd();
                    mailingTemplateSettings = JsonConvert.DeserializeObject<eMailingTemplateSettings>(json, new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All
                    });
                }
                catch (Exception ex)
                {
                    string sDevMsg = string.Concat("Erreur sur la page : ", System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine
                         , Environment.NewLine);
                    sDevMsg = string.Concat(sDevMsg, Environment.NewLine, "Exception : ", ex.ToString());
                    string sUsrMsg = string.Concat("<br>", eResApp.GetRes(this.Pref, 422), "<br>", eResApp.GetRes(Pref, 544));

                    eErrorContainer eErrCont = eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(Pref, 72),   // Message En-tête : Une erreur est survenue
                    sUsrMsg,  //  Détail : pour améliorer...
                    eResApp.GetRes(Pref, 72),  //   titre
                    sDevMsg);
                    eFeedbackXrm.LaunchFeedbackXrm(eErrCont, Pref);
                }
            }

            if (mailingTemplateSettings != null)
            {
                Panel pnlFilterActivitySectorAndTheme = new Panel();
                pnlFilterActivitySectorAndTheme.ID = "filterActivitySectorAndTheme";
                pnlFilterActivitySectorAndTheme.CssClass = "filterActivitySectorAndTheme";
                div.Controls.Add(pnlFilterActivitySectorAndTheme);

                //Filtre Secteur d'activité
                HtmlGenericControl activitySectorText = new HtmlGenericControl("div");
                activitySectorText.Attributes.Add("class", "activitySectorText");
                activitySectorText.InnerText = eResApp.GetRes(_ePref, 2972);

                DropDownList selectActivitySector = new DropDownList();
                selectActivitySector.ID = "selectActivitySector";
                selectActivitySector.CssClass = "selectActivitySector";
                selectActivitySector.Attributes.Add("onchange", "FilterThemeOnActivitySelect(this);");

                selectActivitySector.Items.Add(new ListItem(eResApp.GetRes(_ePref, 22), "0"));
                foreach (eMailingTemplateSettings.ActivityArea activityArea in mailingTemplateSettings.activityArea.OrderBy(a => a.GetRes(Pref.LangId)))
                {
                    if (string.IsNullOrEmpty(activityArea.code))
                        continue;
                    selectActivitySector.Items.Add(new ListItem(activityArea.GetRes(Pref.LangId), activityArea.code));
                }

                pnlFilterActivitySectorAndTheme.Controls.Add(activitySectorText);
                pnlFilterActivitySectorAndTheme.Controls.Add(selectActivitySector);

                //Filtre Thematic
                HtmlGenericControl themeText = new HtmlGenericControl("div");
                themeText.Attributes.Add("class", "themeText");
                themeText.InnerText = eResApp.GetRes(_ePref, 2973);

                DropDownList selectTheme = new DropDownList();
                selectTheme.ID = "selectTheme";
                selectTheme.CssClass = "selectTheme";
                selectTheme.Attributes.Add("onchange", "FilterActivityOnThemeSelect(this);");

                selectTheme.Items.Add(new ListItem(eResApp.GetRes(_ePref, 22), "0"));
                foreach (eMailingTemplateSettings.Themathic theme in mailingTemplateSettings.thematic.OrderBy(t => t.GetRes(Pref.LangId)))
                {
                    if (string.IsNullOrEmpty(theme.code))
                        continue;
                    selectTheme.Items.Add(new ListItem(theme.GetRes(Pref.LangId), theme.code));
                }

                pnlFilterActivitySectorAndTheme.Controls.Add(themeText);
                pnlFilterActivitySectorAndTheme.Controls.Add(selectTheme);


                #endregion

                #region Le label de selection

                HtmlGenericControl divTextList = new HtmlGenericControl("div");
                div.Controls.Add(divTextList);

                divTextList.Attributes.Add("class", "lable-title");

                HtmlGenericControl spanTextList = new HtmlGenericControl("span");
                divTextList.Controls.Add(spanTextList);

                spanTextList.InnerText = String.Concat(eResApp.GetRes(this.Pref, 6412));

                #endregion

                #region table de modèles

                HtmlGenericControl divContentTemplates = new HtmlGenericControl("div");
                divContentTemplates.ID = "templateContainer";
                divContentTemplates.Attributes.Add("style", "height:" + (this._iheight - 180) + "px;"); // 180 la taille des boutons + le header                
                divContentTemplates.Attributes.Add("class", "template-ul");
                div.Controls.Add(divContentTemplates);

                //Type de Template
                HtmlGenericControl divTemplate = new HtmlGenericControl("div");
                divTemplate.Attributes.Add("onclick", "oTemplate.selectCustomTpl(event)");
                divTemplate.Attributes.Add("ondblclick", string.Concat("oTemplate.dblclckSysTemplate(event);return false;"));
                divTemplate.Attributes.Add("class", "template-ml");

                divContentTemplates.Controls.Add(divTemplate);


                int nCpt = 0;

                HtmlGenericControl ulTemplates = new HtmlGenericControl("ul");
                ulTemplates.ID = "tblTemplates";
                divTemplate.Controls.Add(ulTemplates);

                HtmlGenericControl liTemplates = new HtmlGenericControl("li");

                string err = string.Empty;
                string strSelectedItemId = string.Empty;

                foreach (var template in mailingTemplateSettings.templates)
                {
                    Int32 nTemplateId;
                    Int32 nResId;
                    String sImgSrc;
                    String sImgZoom;

                    try
                    {
                        nTemplateId = template.id;

                        sImgSrc = String.Concat(@"Emailing/Img/", template.imageName);
                        sImgZoom = String.Concat(@"Emailing/Img/", template.imageZoom);
                    }
                    catch (Exception ex)
                    {
                        string sDevMsg = string.Concat("Erreur sur la page : ", System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine
                           , Environment.NewLine);
                        sDevMsg = string.Concat(sDevMsg, Environment.NewLine, "Attributs n'existent pas dans le document xml: ", ex.ToString());
                        string sUsrMsg = string.Concat("<br>", eResApp.GetRes(this.Pref, 422), "<br>", eResApp.GetRes(Pref, 544));

                        eErrorContainer eErrCont = eErrorContainer.GetDevUserError(
                           eLibConst.MSG_TYPE.CRITICAL,
                           eResApp.GetRes(Pref, 72),   // Message En-tête : Une erreur est survenue
                           sUsrMsg,  //  Détail : pour améliorer...
                           eResApp.GetRes(Pref, 72),  //   titre
                           sDevMsg);
                        eFeedbackXrm.LaunchFeedbackXrm(eErrCont, Pref);

                        break;
                    }

                    if (nCpt % 1 == 0)
                    {
                        liTemplates = new HtmlGenericControl("li");
                        liTemplates.ID = "liTemplates";
                        ulTemplates.Controls.Add(liTemplates);
                    }

                    HtmlGenericControl divContentTemplate = new HtmlGenericControl("div");
                    liTemplates.Controls.Add(divContentTemplate);

                    divContentTemplate.Style.Add("background-image", sImgSrc);
                    divContentTemplate.ID = String.Concat("tpl_", nTemplateId);

                    //on ajoute la liste des activités
                    divContentTemplate.Attributes.Add("activity", String.Join(";", template.activityArea));
                    //on ajoute la liste des thèmes
                    divContentTemplate.Attributes.Add("theme", String.Join(";", template.thematic));

                    HtmlGenericControl divZoom = new HtmlGenericControl("div");
                    divZoom.Attributes.Add("class", "divZoom");
                    divZoom.ID = "divZoom";
                    divContentTemplate.Controls.Add(divZoom);

                    HtmlGenericControl titleTemplate = new HtmlGenericControl("h4");
                    divContentTemplate.Controls.Add(titleTemplate);

                    titleTemplate.InnerText = template.GetRes(Pref.LangId);

                    divZoom.Attributes.Add("onclick", "popup(\"" + titleTemplate.InnerText.ToString() + "\",'" + sImgZoom + "');");

                    //if (_mailing.MailingParams["templateId"] == nTemplateId.ToString()) { 
                    //    tc.CssClass = "graphCadre graphCadreSel";
                    //    strSelectedItemId = tc.ID;

                    //}
                    //else { 

                    //    strSelectedItemId = string.Empty;
                    //}

                    divContentTemplate.Attributes.Add("class", "graphCadre tplElement");

                    nCpt++;
                }
            }

            #endregion

            #endregion

            #region Modèles utilisateurs

            div = new HtmlGenericControl("div");
            div.ID = "divUserTemplates";
            if (_mailing.MailingParams["templateType"] != "1")
            {
                div.Attributes.Add("style", "display:none;");
            }
            container.Controls.Add(div);

            #region boutons ajouter et imprimer

            //Création des elements Html
            HtmlGenericControl divCatDivContainer = new HtmlGenericControl("div");

            HtmlGenericControl ulCatToolPrint = new HtmlGenericControl("ul");
            HtmlGenericControl ulAddNewModele = new HtmlGenericControl("ul");
            HtmlGenericControl liCatToolPrint = new HtmlGenericControl("li");
            HtmlGenericControl liAddNewModele = new HtmlGenericControl("li");

            HtmlGenericControl printlink = new HtmlGenericControl("a");
            HtmlGenericControl NModele = new HtmlGenericControl("a");

            HtmlGenericControl addSpan = new HtmlGenericControl("span");
            HtmlGenericControl icnSpan = new HtmlGenericControl("span");
            HtmlGenericControl DivLabel = new HtmlGenericControl("span");


            //Composition de l interface
            div.Controls.Add(divCatDivContainer);

            divCatDivContainer.Controls.Add(ulCatToolPrint);


            bool bcanadd = _oRightManager.CanAddNewItem();
            if (bcanadd)
                divCatDivContainer.Controls.Add(ulAddNewModele);

            ulCatToolPrint.Controls.Add(liCatToolPrint);
            ulAddNewModele.Controls.Add(liAddNewModele);

            liCatToolPrint.Controls.Add(printlink);
            liAddNewModele.Controls.Add(NModele);
            NModele.Controls.Add(icnSpan);

            //Ajout des attributs            
            divCatDivContainer.Attributes.Add("class", "divCatDivContainer");
            divCatDivContainer.ID = "";

            ulCatToolPrint.Attributes.Add("class", "catTool");

            liCatToolPrint.Attributes.Add("class", "icon-print2");

            ulAddNewModele.Attributes.Add("class", "catToolAdd");

            //liAddNewModele.Attributes.Add("class", "catToolAddLib");

            //************************************
            NModele.ID = "linkAddNewModele";
            NModele.Attributes.Add("class", "buttonAdd");
            //container.Attributes.Add("canadd", bcanadd ? "1" : "0");

            Mailing.MailingParams.Add("canadd", bcanadd ? "1" : "0");


            NModele.Attributes.Add("Href", String.Concat("javascript:AddNewModele(null, ", TypeMailTemplate.MAILTEMPLATE_EMAILING.GetHashCode(), ", ", eFeaturesManager.IsFeatureAvailable(Pref, eConst.XrmFeature.HTMLTemplateEditor) ? "true" : "false", ", ", _useNewUnsubscribeMethod ? "true" : "false", ");"));
            icnSpan.Attributes.Add("class", "icon-add");
            NModele.Controls.Add(DivLabel);
            DivLabel.Attributes.Add("class", "catToolAddLibSp");
            DivLabel.InnerText = eResApp.GetRes(Pref, 327);

            //TODO implementer la  fonction js qui sera appelée au click le lien href "AddMailTemplate();" dans 
            printlink.Attributes.Add("href", "javascript:alert('Impression...![eMailingWizardRendrer.cs fonction:BuildSelectTemplatesPanel()]');");

            #endregion fin boutons ajouter et imprimer

            #region Contenu de la liste des modeles d emailing
            Panel listContent = new Panel();

            div.Controls.Add(listContent);

            listContent.ID = "listContent";
            listContent.CssClass = "list-content";

            //Recuperation de la liste des templates complete epuis le renderer
            LoadListFromRenderer(Pref, _iTab, listContent, TypeMailTemplate.MAILTEMPLATE_EMAILING);

            #endregion


            HiddenField hidDefaultMailTpl = new HiddenField();
            hidDefaultMailTpl.ID = "HidDefaultTplID";
            hidDefaultMailTpl.Value = _defaultMailTplID.ToString();
            div.Controls.Add(hidDefaultMailTpl);

            #region Bouton "Modèle par défaut"
            eCheckBoxCtrl checkbox = new eCheckBoxCtrl(false, false);
            checkbox.ID = "DefaultTemplate";
            checkbox.Attributes.Add("mtid", "");
            checkbox.AddText(eResApp.GetRes(Pref, 6900));
            checkbox.AddClick("onDefaultTemplateCheck(this, " + _mailing.MailingParams["templateType"] + ")");
            div.Controls.Add(checkbox);
            #endregion

            #endregion

            return container;
        }

        /// <summary>
        /// Charge les paramètres adaptés à la liste affichée
        /// </summary>
        protected void LoadListFromRenderer(ePref pref, Int32 nTab, Panel listContent, TypeMailTemplate nType)
        {
            // RAZ le filtre sur la liste de filtre : remet le filtre sur l'utilisateur en cours
            List<SetParam<ePrefConst.PREF_PREF>> prefFilter = new List<SetParam<ePrefConst.PREF_PREF>>();
            prefFilter.Add(new SetParam<ePrefConst.PREF_PREF>(ePrefConst.PREF_PREF.BKMFILTERCOL
                , (TableType.MAIL_TEMPLATE.GetHashCode() + AllField.OWNER_USER.GetHashCode()).ToString()));
            prefFilter.Add(new SetParam<ePrefConst.PREF_PREF>(ePrefConst.PREF_PREF.BKMFILTEROP
                , EudoQuery.Operator.OP_EQUAL.GetHashCode().ToString()));        // egale à
            prefFilter.Add(new SetParam<ePrefConst.PREF_PREF>(ePrefConst.PREF_PREF.BKMFILTERVALUE
                , pref.User.UserId.ToString())); // utilisateur en cours
            if (!pref.SetPref(TableType.MAIL_TEMPLATE.GetHashCode(), prefFilter))
                throw new Exception("Erreur de PREF");  //TODO

            eRightMailTemplate oRightManager = new eRightMailTemplate(pref);
            eRenderer myMainList = eRendererFactory.CreateMailTemplateListRenderer(pref, _iwidth, _iheight, nTab, nType, oRightManager, 1);

            if (myMainList.ErrorMsg.Length == 0)
            {
                //déplace les éléments du conteneur généré (myMainList) vers le conteneur final (listcontent)
                // On ne peut pas ajouter directement myMainList dans listcontent : il ne faut 
                // pas ajouter le div englobant de myMainList (listContent.Controls.resp(_myMainList.PgContainer);)
                // , cela perturbe js et css               
                while (myMainList.PgContainer.Controls.Count > 0)
                {
                    listContent.Controls.Add(myMainList.PgContainer.Controls[0]);
                }
            }
            else
            {
                #region Feedback d'erreur
                String sDevMsg = String.Concat("Erreur sureMailingWizardRendrer - Chargement de la liste : \n", myMainList.ErrorMsg);

                if (myMainList.InnerException != null)
                {
                    sDevMsg = String.Concat(sDevMsg, Environment.NewLine, Environment.NewLine, "Message Exception : ", myMainList.InnerException.Message,
                        Environment.NewLine, "Exception StackTrace :", myMainList.InnerException.StackTrace
                        );
                }
                //"\n Message Exception

                eErrorContainer ErrorContainer = eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(Pref, 72),   // Message En-tête : Une erreur est survenue
                    String.Concat(eResApp.GetRes(Pref, 422), "<br>", eResApp.GetRes(Pref, 544)),  //  Détail : pour améliorer...
                    eResApp.GetRes(Pref, 72),  //   titre
                 sDevMsg);

                eFeedbackXrm.LaunchFeedbackXrm(ErrorContainer, Pref);
                listContent.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 72)));
                listContent.Controls.Add(new HtmlGenericControl("br"));
                listContent.Controls.Add(new LiteralControl(String.Concat(eResApp.GetRes(Pref, 422), " ", eResApp.GetRes(Pref, 544))));

                #endregion
            }
        }


        /// <summary>
        /// Construction du div pour le corps du mail
        /// </summary>
        /// <returns></returns>
        protected Panel BuildMailBodyPanel(bool bCkeditor = false)
        {
            Panel container = new Panel();
            container.CssClass = "mailing_body_container";



            //conserve une référence sur le filerenderer de la campagne
            if (_efRend.RendererType == RENDERERTYPE.EditMailing || _efRend.RendererType == RENDERERTYPE.EditSMSMailing)
            {
                _efCampaignFile = ((eEditMailingRenderer)_efRend).File;
                eRecordCampaign _campaign = _efCampaignFile.Record as eRecordCampaign;
                if (_campaign != null && _campaign.MainFileid > 0)//Ajouter les infos de la campagne
                {
                    _optInEnabled = _campaign.OptInEnabled;
                    _optOutEnabled = _campaign.OptOutEnabled;
                    _noConsentEnabled = _campaign.NoConsentEnabled;
                    _noRemoveDoubleEnable = _campaign.GetFieldByAlias(string.Format("{0}_{1}", (int)TableType.CAMPAIGN, (int)CampaignField.REMOVEDOUBLES)).Value == "1";
                    if (_campaign.AdressStatusParam != null)
                    {
                        _adressStatusParam.ValidAdress = _campaign.AdressStatusParam.ValidAdress;
                        _adressStatusParam.NotVerifiedAdress = _campaign.AdressStatusParam.NotVerifiedAdress;
                        _adressStatusParam.InvalidAdress = _campaign.AdressStatusParam.InvalidAdress;
                    }

                    eFieldRecord fldMed = _efCampaignFile.GetField(CampaignField.MEDIATYPE.GetHashCode());
                    if (fldMed != null)
                        Int32.TryParse(fldMed.Value, out _mediaType);

                    eFieldRecord fldCmpType = _efCampaignFile.GetField(CampaignField.CATEGORY.GetHashCode());
                    if (fldCmpType != null)
                        Int32.TryParse(fldCmpType.Value, out _campaignType);
                }
            }

            container = _efRend.PgContainer;

            return container;
        }


        #endregion
        #endregion


    }


}