using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.Sms;
using Com.Eudonet.Xrm.import;
using EudoExtendedClasses;
using EudoQuery;
using Newtonsoft.Json;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// page d'affichage des assistants
    /// </summary>
    public partial class eWizard : eEudoPage
    {

        private eRenderer _mainRender;

        /// <summary>
        /// Table cible du traitment
        /// </summary>
        private Int32 _iTab = 0;

        private string _sPresta = "";

        /// <summary>
        /// Table d'origine du traitement
        /// </summary>
        private Int32 _iTabFrom = 0;

        private Int32 _iFileId = 0;

        /// <summary>
        /// Largeur du contenu de l'assistant
        /// </summary>
        private string _strWidth = "800";
        /// <summary>
        /// Hauteur du contenu de l'assistant
        /// </summary>
        private string _strHeight = "600";
        /// <summary>
        /// Largeur du contenu de l'assistant recalculée en pixels (si taille originale précisée en %)
        /// </summary>
        private decimal _nAbsWidth = 800;
        /// <summary>
        /// Hauteur du contenu de l'assistant recalculée en pixels (si taille originale précisée en %)
        /// </summary>
        private decimal _nAbsHeight = 600;
        /// <summary>
        /// Largeur de la fenêtre disponible pour l'affichage
        /// </summary>
        private string _strDocWidth = "1024";
        /// <summary>
        /// Hauteur de la fenêtre disponible pour l'affichage
        /// </summary>
        private string _strDocHeight = "768";

        private Int32 _iWizardTotalStepNumber = 0;
        private String _strErrorMessage = String.Empty;
        private String _strFrameId = String.Empty;
        private String _strParentFrameId = String.Empty;
        private Dictionary<Int32, String> _lstRelatedFilters;


        /// <summary>
        /// Indique si l'appel est pour la génération d'un wizard complet
        /// ou s'il correspond à un appel ajax de mise à jour
        /// TODO : les appels de maj devrait être fait via manager
        /// </summary>
        private Boolean _bFullWizard = true;



        #region variable publique communiquant avec l'ASPX
        /// <summary>
        /// Type d'assistant (report, mailing...)
        /// </summary>
        public String _strWizardType = "";


        /// <summary>
        /// Attributs du body html partagée avec l'aspx
        /// </summary>
        public String _strBodyAttributes = String.Empty;
        #endregion

        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return scriptHolder;
        }

        /// <summary>
        /// Chargement de la page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {



            //TODO : Ajouter en post un type de Wizard pour pouvoir exécuter les codes différemments en fonction de l'assistant eMailing, reporting, Graphiques, Homepage etc...
            #region variables de post génériques [commun à tous les Wizards]

            try
            {
                //onglet actif
                if (_requestTools.AllKeys.Contains("tab") && !String.IsNullOrEmpty(Request.Form["tab"]))
                {
                    _iTab = eLibTools.GetNum(Request.Form["tab"].ToString());
                    _iTabFrom = _iTab;
                }

                //Id de la frame appelante
                if (_requestTools.AllKeys.Contains("frmId") && !String.IsNullOrEmpty(Request.Form["frmId"]))
                    _strFrameId = Request.Form["frmId"].ToString();

                //id de la modale
                if (_requestTools.AllKeys.Contains("modalId") && !String.IsNullOrEmpty(Request.Form["modalId"]))
                    _strParentFrameId = Request.Form["modalId"].ToString();

                //Type d'assistant
                if (_requestTools.AllKeys.Contains("wizardtype") && !String.IsNullOrEmpty(Request.Form["wizardtype"]))
                    _strWizardType = Request.Form["wizardtype"].ToString();

                //Largeur du contenu de l'assistant
                if (_requestTools.AllKeys.Contains("width") && !String.IsNullOrEmpty(Request.Form["width"]))
                    _strWidth = Request.Form["width"].ToString();

                //Hauteur du contenu de l'assistant
                if (_requestTools.AllKeys.Contains("height") && !String.IsNullOrEmpty(Request.Form["height"]))
                    _strHeight = Request.Form["height"].ToString();

                //Largeur de la fenêtre
                if (_requestTools.AllKeys.Contains("docwidth") && !String.IsNullOrEmpty(Request.Form["docwidth"]))
                    _strDocWidth = Request.Form["docwidth"].ToString();

                //Hauteur de la fenêtre
                if (_requestTools.AllKeys.Contains("docheight") && !String.IsNullOrEmpty(Request.Form["docheight"]))
                    _strDocHeight = Request.Form["docheight"].ToString();

                // Calcul de la taille du contenu en pixels si la taille a été précisée en %
                if (_strWidth.Contains("%"))
                {
                    decimal widthPercentValue = eLibTools.GetNum(_strWidth.Replace("%", String.Empty).Split('.')[0]);
                    decimal containerWidth = eLibTools.GetNum(_strDocWidth.Split('.')[0]);
                    if (widthPercentValue > 0 && containerWidth > 0)
                        _nAbsWidth = containerWidth * (widthPercentValue / 100);
                }
                if (_strHeight.Contains("%"))
                {
                    decimal heightPercentValue = eLibTools.GetNum(_strHeight.Replace("%", String.Empty).Split('.')[0]);
                    decimal containerHeight = eLibTools.GetNum(_strDocHeight.Split('.')[0]);
                    if (heightPercentValue > 0 && containerHeight > 0)
                        _nAbsHeight = containerHeight * (heightPercentValue / 100);
                }
            }
            catch (eEndResponseException) { Response.End(); }
            catch (ThreadAbortException) { }
            catch
            {
                LaunchError(eErrorContainer.GetDevUserError(
                            eLibConst.MSG_TYPE.CRITICAL,
                            eResApp.GetRes(_pref, 72),
                            eResApp.GetRes(_pref, 6342),
                            eResApp.GetRes(_pref, 416),
                            "Paramètres invalides dans eWizard.aspx : "));
            }

            #endregion



            #region CSS & JS
            /* CSS Commun */

            PageRegisters.AddCss("eWizard");
            PageRegisters.AddCss("ePerm");
            PageRegisters.AddCss("eReportWizard");
            PageRegisters.AddCss("eControl");
            PageRegisters.AddCss("eButtons");
            PageRegisters.AddCss("eCatalog");
            PageRegisters.AddCss("eMain");
            PageRegisters.AddCss("eList");
            PageRegisters.AddCss("eFile");
            PageRegisters.AddCss("eIcon");
            PageRegisters.AddCss("eTitle");
            PageRegisters.AddCss("eActions");
            PageRegisters.AddCss("eActionList");
            PageRegisters.AddCss("eContextMenu");
            PageRegisters.AddCss("grapesjs/grapes.min");
            PageRegisters.AddCss("grapesjs/grapesjs-preset-newsletter");
            PageRegisters.AddCss("grapesjs/grapesjs-preset-webpage");
            PageRegisters.AddCss("eMemoEditor");
            PageRegisters.AddCss("theme");
            PageRegisters.AddCss("eudoFont");
            PageRegisters.AddCss("eTplMail");


            /* JS Commun */
            PageRegisters.AddScript("eTools");
            PageRegisters.AddScript("eModalDialog");
            PageRegisters.AddScript("eUpdater");
            PageRegisters.AddScript("ePerm");
            PageRegisters.AddScript("eWizard");
            PageRegisters.AddScript("eReport");
            PageRegisters.AddScript("eDrag");
            PageRegisters.AddScript("eTabsFieldsSelect");
            PageRegisters.AddScript("eGrapesJSEditor");
            PageRegisters.AddScript("eMemoEditor");
            PageRegisters.AddScript("eList");
            PageRegisters.AddScript("eExpressFilter");
            PageRegisters.AddScript("eContextMenu");
            PageRegisters.AddScript("eMain");
            PageRegisters.AddScript("ckeditor/ckeditor");
            PageRegisters.AddScript("grapesjs/grapes.min");
            PageRegisters.AddScript("grapesjs/grapesjs-plugin-ckeditor.min");
            PageRegisters.AddScript("grapesjs/grapesjs-blocks-basic.min");
            PageRegisters.AddScript("grapesjs/grapesjs-preset-newsletter.min");
            PageRegisters.AddScript("grapesjs/grapesjs-preset-webpage.min");

            //JS basique pour les wizard
            PageRegisters.RawScrip
                .Append("   var listIframe = top.document.getElementById(\"").Append(_strFrameId).Append("\");").AppendLine()
                .Append("   var wizardIframe = top.document.getElementById(\"").Append(_strParentFrameId).Append("\");").AppendLine();

            #endregion

            switch (_strWizardType)
            {
                case "duplitreat":
                    #region Traitement duplication
                    try
                    {

                        PageRegisters.AddScript("eTreatment");
                        PageRegisters.AddScript("eFieldEditor");
                        PageRegisters.AddScript("eCalendar");
                        PageRegisters.AddScript("eDuplicationWizard");
                        PageRegisters.AddScript("eFilterWizardLight");


                        PageRegisters.AddCss("eDuplicationTreatment");

                        if (_requestTools.AllKeys.Contains("tab") && !String.IsNullOrEmpty(Request.Form["tab"]))
                            _iTab = eLibTools.GetNum(Request.Form["tab"].ToString());


                        _mainRender = CreateDuplicationTreatmentdRenderer();
                    }
                    catch (eEndResponseException) { Response.End(); }
                    catch (ThreadAbortException) { }
                    catch (Exception ex)
                    {
                        LaunchError(eErrorContainer.GetDevUserError(
                                    eLibConst.MSG_TYPE.CRITICAL,
                                    eResApp.GetRes(_pref, 72),
                                    eResApp.GetRes(_pref, 6342),
                                    eResApp.GetRes(_pref, 416),
                                    "Paramètres invalides dans eWizard.aspx : "));
                    }
                    break;
                #endregion

                case "import":
                    #region Assistant d'import

                    //JS
                    PageRegisters.AddScript("eFieldEditor");
                    PageRegisters.AddScript("eEngine");
                    PageRegisters.AddScript("ePopup");
                    PageRegisters.AddScript("eFile");
                    PageRegisters.AddScript("eDictionary");
                    PageRegisters.AddScript("import/eImportWizardInternal");
                    PageRegisters.AddScript("import/eImportTemplateWizard");
                    //CSS
                    PageRegisters.AddCss("eControl");
                    PageRegisters.AddCss("eImport");

                    _mainRender = CreateImportWizardRenderer(new eImportWizardParam(_requestTools));

                    #endregion
                    break;
                case "formular":
                    #region Assistant formulaire

                    //JS
                    PageRegisters.AddScript("eFieldEditor");
                    PageRegisters.AddScript("eEngine");
                    PageRegisters.AddScript("ePopup");
                    PageRegisters.AddScript("eFile");
                    PageRegisters.AddScript("eFormular");
                    PageRegisters.AddScript("eFormularWizard");
                    //CSS
                    PageRegisters.AddCss("eFormularWizard");

                    Int32 formularId = 0;

                    // Depuis ++/cible etendu
                    if (_requestTools.AllKeys.Contains("tab") && !String.IsNullOrEmpty(Request.Form["tab"]))
                        _iTab = eLibTools.GetNum(Request.Form["tab"].ToString());

                    // Depuis ++/cible etendu
                    if (_requestTools.AllKeys.Contains("parentfileid") && !String.IsNullOrEmpty(Request.Form["parentfileid"]))
                        _iFileId = eLibTools.GetNum(Request.Form["parentfileid"].ToString());

                    // id de formulaire s'il existe sinon 0 pour en créer
                    if (_requestTools.AllKeys.Contains("formularid") && !String.IsNullOrEmpty(Request.Form["formularid"]))
                        formularId = eLibTools.GetNum(Request.Form["formularid"].ToString());


                    int width = 0;
                    int height = 0;

                    Int32.TryParse(_strHeight, out height);

                    //#tâche #2 752: KJE, on ajoute un test sur les droits de modif pour s'assurer que l'utilisateur a bien les droits de modif des formulaires
                    if (formularId > 0)
                    {
                        eRightFormular RightManager = new eRightFormular(_pref);
                        if (RightManager != null && !RightManager.HasRight(eLibConst.TREATID.UPDATE_FORMULAR))
                        {
                            ErrorContainer = eErrorContainer.GetUserError(
                            eLibConst.MSG_TYPE.QUESTION,
                            eResApp.GetRes(_pref, 2478),
                            eResApp.GetRes(_pref, 2479),
                            eResApp.GetRes(_pref, 5080));
                            try
                            {
                                LaunchError();
                            }
                            catch (eEndResponseException) { Response.End(); }

                            return;
                            //LaunchError();
                        }
                    }
                    _mainRender = CreateFormularXrmWizardRenderer(_iTab, _iFileId, formularId, height);

                    #endregion
                    break;
                case "report":
                    #region Assistant Reporting

                    PageRegisters.AddScript("eFilterReportListDialog");
                    PageRegisters.AddScript("eReportWizard");
                    PageRegisters.AddScript("ePopup");
                    PageRegisters.AddScript("eFilterWizardLight");
                    //CSS
                    PageRegisters.AddCss("eFieldsSelect");
                    PageRegisters.AddCss("eFilterWizard");

                    eReport report = null;
                    Int32 reportId = 0;
                    TypeReport reportType = TypeReport.NONE;
                    eReport.ReportOperation operationReport = eReport.ReportOperation.NONE;
                    #region Variable récupéré du post

                    //Type de rapport (Impression, export, publipostage...)
                    if (_allKeys.Contains("rtype") && !String.IsNullOrEmpty(Request.Form["rtype"]))
                    {
                        try
                        {
                            reportType = (TypeReport)eLibTools.GetNum(Request.Form["rtype"].ToString());
                        }
                        catch (eEndResponseException) { Response.End(); }
                        catch (ThreadAbortException) { }
                        catch
                        {
                            LaunchError(eErrorContainer.GetDevUserError(
                                        eLibConst.MSG_TYPE.CRITICAL,
                                        eResApp.GetRes(_pref, 6390),
                                        eResApp.GetRes(_pref, 6342),
                                        eResApp.GetRes(_pref, 416),
                                        eResApp.GetRes(_pref, 6518)));

                        }
                    }

                    //Opération ayant déclenché l'ouverture de cette page
                    if (_allKeys.Contains("operation") && !String.IsNullOrEmpty(Request.Form["operation"]))
                    {
                        try
                        {
                            operationReport = (eReport.ReportOperation)eLibTools.GetNum(Request.Form["operation"].ToString());
                        }
                        catch (eEndResponseException) { Response.End(); }
                        catch (ThreadAbortException) { }
                        catch
                        {
                            LaunchError(eErrorContainer.GetDevUserError(
                                        eLibConst.MSG_TYPE.CRITICAL,
                                        eResApp.GetRes(_pref, 6390),
                                        eResApp.GetRes(_pref, 6342),
                                        eResApp.GetRes(_pref, 416),
                                        "Assistant Reporting : Paramètres invalides [Operation] dans eWizard.aspx : "));

                        }
                    }
                    //Id du rapport édité
                    if (_allKeys.Contains("reportid") && !String.IsNullOrEmpty(Request.Form["reportid"]))
                    {
                        try
                        {
                            reportId = eLibTools.GetNum(Request.Form["reportid"].ToString());
                        }
                        catch (eEndResponseException) { Response.End(); }
                        catch (ThreadAbortException) { }
                        catch
                        {
                            LaunchError(eErrorContainer.GetDevUserError(
                                     eLibConst.MSG_TYPE.CRITICAL,
                                     eResApp.GetRes(_pref, 6390),
                                     eResApp.GetRes(_pref, 6342),
                                     eResApp.GetRes(_pref, 416),
                                     "Assistant Reporting : Paramètres invalides [reportid] dans eWizard.aspx : "));

                        }
                    }

                    #endregion

                    #region traitement de l'opération demandées

                    switch (operationReport)
                    {
                        case eReport.ReportOperation.UPDATE:
                            if (reportId > 0)
                            {
                                report = new eReport(_pref, reportId);
                                if (!report.LoadFromDB())
                                    _strErrorMessage = String.Concat("eWizard.report.Load() :", report.ErrorMessage);

                                reportType = report.ReportType;
                                _iTab = report.Tab;

                                //#77 224: KJE, on ajoute un test sur les droits de modif pour s'assurer que l'utilisateur a bien les droits de modif des rapports
                                eRightReport RightManagerReport = new eRightReport(_pref, report.ReportType);
                                if (RightManagerReport != null && !RightManagerReport.HasRightByFormat(eRightReport.RightByFormat.Operation.EDIT, report.Format))
                                {
                                    ErrorContainer = eErrorContainer.GetUserError(
                            eLibConst.MSG_TYPE.QUESTION,
                            eResApp.GetRes(_pref, 2478),
                                    eResApp.GetRes(_pref, 2479),
                                    eResApp.GetRes(_pref, 5080));
                                    try
                                    {
                                        LaunchError();
                                    }
                                    catch (eEndResponseException) { Response.End(); }

                                    return;
                                    //LaunchError();
                                }
                            }
                            else
                                _strErrorMessage = String.Concat("Assistant Reporting : Paramètres invalides [ID]");
                            break;
                        case eReport.ReportOperation.ADD:
                            if (reportType == TypeReport.CHARTS)
                            {
                                report = new eReport(_pref) { IsNew = true, ReportType = TypeReport.CHARTS };
                                report.LoadParams(eChartWizardRenderer.GetBasicParam(_iTab));
                            }
                            else
                                report = null;

                            break;
                        default:
                            break;
                    }

                    if (reportType.Equals(TypeReport.NONE))
                        _strErrorMessage = String.Concat("Assistant Reporting : Paramètres invalides [Type de rapport]");
                    else
                        _mainRender = CreateReportWizardRenderer(reportType, report);

                    if (!String.IsNullOrEmpty(_strErrorMessage) || !String.IsNullOrEmpty(_mainRender.ErrorMsg))
                    {
                        LaunchError(eErrorContainer.GetDevUserError(
                                 eLibConst.MSG_TYPE.CRITICAL,
                                 eResApp.GetRes(_pref, 6390),
                                 eResApp.GetRes(_pref, 6342),
                                 eResApp.GetRes(_pref, 416),
                                string.Concat(eResApp.GetRes(_pref, 6390), " - ", eResApp.GetRes(_pref, 6520), _strErrorMessage)));

                    }

                    #endregion

                    #endregion
                    break;
                case "mailing":
                    #region Assistant Emailing

                    //JS
                    PageRegisters.AddScript("ePerm");
                    PageRegisters.AddScript("eMailingTpl");
                    PageRegisters.AddScript("eMailingWizard");
                    PageRegisters.AddScript("eFieldEditor");

                    PageRegisters.AddScript("eEngine");
                    PageRegisters.AddScript("ePopup");
                    PageRegisters.AddScript("eFile");

                    //CSS
                    PageRegisters.AddCss("eMailing");

                    eMailing mailing = null;
                    Int32 mailingId = 0;
                    TypeMailing mailingType = TypeMailing.MAILING_UNDEFINED;
                    eMailing.MailingAction operationMailing = eMailing.MailingAction.NONE;

                    #region Variable récupéré du post


                    if (_allKeys.Contains("tab") && !String.IsNullOrEmpty(Request.Form["tab"]))
                        _iTab = eLibTools.GetNum(Request.Form["tab"].ToString());

                    if (_allKeys.Contains("parenttab") && !String.IsNullOrEmpty(Request.Form["parenttab"]))
                        _iTabFrom = eLibTools.GetNum(Request.Form["parenttab"].ToString());

                    if (_allKeys.Contains("parentfileid") && !String.IsNullOrEmpty(Request.Form["parentfileid"]))
                        _iFileId = eLibTools.GetNum(Request.Form["parentfileid"].ToString());

                    if (_allKeys.Contains("mtype") && !String.IsNullOrEmpty(Request.Form["mtype"]))
                    {
                        try
                        {
                            mailingType = (TypeMailing)eLibTools.GetNum(Request.Form["mtype"].ToString());
                            //depuis le mode liste contacts... 
                            if (mailingType == TypeMailing.MAILING_FROM_LIST)
                                _iTabFrom = 0;

                            if (mailingType == TypeMailing.SMS_MAILING_UNDEFINED || mailingType == TypeMailing.SMS_MAILING_FROM_BKM)
                            {
                      
                                SmsNetMessageSettingsClient settingsClientExtension = eLibTools.GetSerializedSMSSettingsExtension(_pref);
                                if (!string.IsNullOrEmpty(settingsClientExtension.ApiKeyRest))
                                    _sPresta = "LM_REST";

                                PageRegisters.AddScript("eSmsing");
                                PageRegisters.AddCss("eSMS");
                            }

                            else
                                PageRegisters.AddScript("eMailing");

                        }
                        catch (eEndResponseException) { Response.End(); }
                        catch (ThreadAbortException) { }
                        catch
                        {
                            LaunchError(eErrorContainer.GetDevUserError(
                                        eLibConst.MSG_TYPE.CRITICAL,
                                        eResApp.GetRes(_pref, 6391),
                                        eResApp.GetRes(_pref, 6342),
                                        eResApp.GetRes(_pref, 416),
                                        "Assistant Mailing : Paramètres invalides [Type] dans eWizard.aspx : "));

                        }
                    }


                    //Id du rapport édité
                    if (_allKeys.Contains("campId") && !String.IsNullOrEmpty(Request.Form["campId"]))
                    {
                        try
                        {
                            mailingId = eLibTools.GetNum(Request.Form["campId"].ToString());
                        }
                        catch (eEndResponseException) { Response.End(); }
                        catch (ThreadAbortException) { }
                        catch
                        {
                            LaunchError(eErrorContainer.GetDevUserError(
                                     eLibConst.MSG_TYPE.CRITICAL,
                                     eResApp.GetRes(_pref, 6391),
                                     eResApp.GetRes(_pref, 6342),
                                     eResApp.GetRes(_pref, 416),
                                     "Assistant Mailing : Paramètres invalides [mailingid] dans eWizard.aspx : "));

                        }
                    }
                    //Opération ayant déclenché l'ouverture de cette page

                    operationMailing = eMailing.MailingAction.INSERT;
                    if (mailingId > 0)
                        operationMailing = eMailing.MailingAction.UPDATE;


                    #endregion

                    #region traitement de l'opération demandée

                    switch (operationMailing)
                    {
                        case eMailing.MailingAction.UPDATE:
                            mailing = new eMailing(_pref, mailingId, mailingType);

                            if (mailingType == TypeMailing.MAILING_FROM_BKM || mailingType == TypeMailing.SMS_MAILING_FROM_BKM || mailingType == TypeMailing.MAILING_FOR_MARKETING_AUTOMATION)
                            {
                                mailing.ParentFileId = _iFileId;
                            }

                            if (mailingType == TypeMailing.MAILING_FROM_CAMPAIGN)
                            {
                                mailing.ParentTab = _iTabFrom;
                            }

                            break;
                        case eMailing.MailingAction.INSERT:
                            mailing = new eMailing(_pref, 0, _iTab, null, mailingType);

                            mailing.ParentTab = _iTabFrom;
                            if (mailingType == TypeMailing.MAILING_FROM_BKM || mailingType == TypeMailing.SMS_MAILING_FROM_BKM || mailingType == TypeMailing.MAILING_FOR_MARKETING_AUTOMATION)
                                mailing.ParentFileId = _iFileId;

                            break;
                        default:
                            break;
                    }


                    //Pour avertir l utilisateur que les champs de type mail n existe dans la table encours
                    if (mailing.EmailFields.Count == 0 && (mailingType != TypeMailing.MAILING_FROM_CAMPAIGN && mailingType != TypeMailing.SMS_MAILING_FROM_BKM))
                    {

                        eErrorContainer containerInfo = eErrorContainer.GetUserError(
                            eLibConst.MSG_TYPE.QUESTION,
                            eResApp.GetRes(_pref, 6487).Replace("<TABNAME>", eLibTools.GetPrefName(this._pref, TableType.CAMPAIGN.GetHashCode())),
                            eResApp.GetRes(_pref, 6486).Replace("<FIELDTYPE>", eResApp.GetRes(_pref, 110))
                            .Replace("<TABNAME>", eLibTools.GetPrefName(this._pref, this._iTab)), eResApp.GetRes(_pref, 5080));

                        try
                        {
                            LaunchError(containerInfo);
                        }
                        catch (eEndResponseException) { Response.End(); }

                        return;
                    }
                    else
                    {
                        try
                        {
                            _mainRender = CreateMailingWizardRenderer(mailingType, mailing);
                        }
                        catch (EudoException ee)
                        {
                            try
                            {

                                LaunchError(eErrorContainer.GetDevUserError(
                                    typ: eLibConst.MSG_TYPE.CRITICAL,
                                    msg: ee.UserMessage,
                                    detailMsg: ee.UserMessageDetails,
                                    title: ee.UserMessageTitle, exc: ee));



                            }

                            catch (eEndResponseException) { Response.End(); }

                            return;
                        }


                        if (mailingType == TypeMailing.SMS_MAILING_FROM_BKM)
                            PageRegisters.RawScrip.Append(BuildSmsMailingWizardScript(mailingType, mailing));
                        else
                            PageRegisters.RawScrip.Append(BuildMailingWizardScript(mailingType, mailing));
                    }

                    if (!String.IsNullOrEmpty(_mainRender.ErrorMsg))
                        String.Concat(_strErrorMessage, _mainRender.ErrorMsg);

                    if (!String.IsNullOrEmpty(_strErrorMessage))
                    {
                        try
                        {

                            LaunchError(eErrorContainer.GetDevUserError(
                                     eLibConst.MSG_TYPE.CRITICAL,
                                     eResApp.GetRes(_pref, 6519),
                                     eResApp.GetRes(_pref, 6342),
                                     eResApp.GetRes(_pref, 416),
                                    string.Concat(
                                        eResApp.GetRes(_pref, 6519), " - ", eResApp.GetRes(_pref, 6520), _strErrorMessage)
                                    ));
                        }
                        catch (eEndResponseException) { Response.End(); }
                    }

                    #endregion

                    #endregion
                    break;
                case "invit":
                    wizardform.Target = "fratmp";
                    #region Assistant Invitation (++/Xx)

                    //DescId Event
                    if (_allKeys.Contains("tabfrom") && !String.IsNullOrEmpty(Request.Form["tabfrom"]))
                        _iTabFrom = eLibTools.GetNum(Request.Form["tabfrom"].ToString());

                    //Id Event
                    if (_allKeys.Contains("fileidfrom") && !String.IsNullOrEmpty(Request.Form["fileidfrom"]))
                        _iFileId = eLibTools.GetNum(Request.Form["fileidfrom"].ToString());

                    //++ ou XX
                    Boolean bDelete = false;
                    if (_allKeys.Contains("delete") && !String.IsNullOrEmpty(Request.Form["delete"]))
                        bDelete = Request.Form["delete"].ToString() == "1";

                    //JS spécifique
                    PageRegisters.AddScript("eFilterReportListDialog");
                    PageRegisters.AddScript("eInvitWizard");
                    PageRegisters.AddScript("eTreatment");
                    PageRegisters.AddScript("ePopup");
                    PageRegisters.AddScript("eFieldEditor");
                    PageRegisters.AddScript("eEngine");
                    PageRegisters.AddScript("eMain");
                    PageRegisters.AddScript("eButton");
                    PageRegisters.AddScript("eContextMenu");

                    //CSS
                    PageRegisters.AddCss("eInvitWizard");
                    PageRegisters.AddCss("eActionList");

                    //Num de la page qu on souhaite afficher
                    Int32 nPage = 0;
                    if (_allKeys.Contains("nPage"))
                    {
                        Int32.TryParse(Request.Form["nPage"].ToString(), out nPage);
                        nPage = nPage == 0 ? 1 : nPage;
                    }


                    CreateInvitSelectWizardRenderer(bDelete);

                    #endregion
                    break;
                case "linkfile":
                    PageRegisters.AddScript("eReportWizard");
                    this.content.Controls.Add(BuildLinkFileRenderer());
                    _bFullWizard = false;
                    break;
                case "reloadlinkedfile":
                    #region reloadlinkedfile
                    //Appelé dans le cas d'un apprel via eUpdater pour mettre à jour en ajax une partie du wizard 

                    //rechargement du contenu de la dropdownlist des fichiers disponibles à lier
                    Response.Clear();
                    Response.ClearContent();
                    Response.Write(eTools.GetControlRender(GetLinkedFileList(_pref, _iTab, "editor_linkedfilelist")));
                    Response.End();

                    break;
                #endregion
                case "linkedfields":
                    //Appelé dans le cas d'un apprel via eUpdater pour mettre à jour en ajax une partie du wizard 
                    //  TODO : devrait être appelé via un manager - 
                    _bFullWizard = false;
                    #region Linked Field
                    Int32 fromTab = 0;
                    String fromTabLabel = String.Empty;
                    String tabLabel = String.Empty;
                    Boolean itemFound = false;
                    if (_allKeys.Contains("fromtab") && !String.IsNullOrEmpty(Request.Form["fromtab"]))
                    {
                        if (!Int32.TryParse(Request.Form["fromtab"], out fromTab))
                            _strErrorMessage = "Informations d'onglet de départ incorrectes";
                        if (fromTab == 0)
                            _strErrorMessage = "Informations d'onglet de départ incorrectes";
                    }
                    if (_strErrorMessage.Length > 0)
                    {
                        LaunchError(eErrorContainer.GetDevUserError(
                                 eLibConst.MSG_TYPE.CRITICAL,
                                 eResApp.GetRes(_pref, 6390),
                                 eResApp.GetRes(_pref, 6342),
                                 eResApp.GetRes(_pref, 416),
                                    string.Concat(eResApp.GetRes(_pref, 6390), " - ", eResApp.GetRes(_pref, 6520), _strErrorMessage)));

                    }

                    try
                    {
                        eRes res = new eRes(_pref, String.Concat(_iTab, ",", fromTab));
                        tabLabel = res.GetRes(_iTab, out itemFound);

                        if (!itemFound)
                            _strErrorMessage = "Le libellé du fichier sélectionné n'a pas été trouvé";
                        fromTabLabel = res.GetRes(fromTab, out itemFound);

                        if (!itemFound)
                            _strErrorMessage = "Le libellé du fichier de liaison n'a pas été trouvé";
                    }
                    catch (eEndResponseException) { Response.End(); }
                    catch (ThreadAbortException) { }
                    catch (Exception ex)
                    {
                        _strErrorMessage = ex.Message;

                    }
                    if (_strErrorMessage.Length == 0)
                    {
                        //rechargement du contenu de la dropdownlist des fichiers disponibles à lier
                        Response.Clear();
                        Response.ClearContent();
                        Response.Write(eTools.GetControlRender(eReportWizardRenderer.BuildLinkedList(_pref, tabLabel, _iTab, fromTab, fromTabLabel)));
                        Response.End();
                    }
                    else
                    {
                        LaunchError(eErrorContainer.GetDevUserError(
                                        eLibConst.MSG_TYPE.CRITICAL,
                                        eResApp.GetRes(_pref, 6390),
                                        eResApp.GetRes(_pref, 6342),
                                        eResApp.GetRes(_pref, 416),
                                        string.Concat(eResApp.GetRes(_pref, 6390), " - ", eResApp.GetRes(_pref, 6520), _strErrorMessage)));
                    }
                    break;

                #endregion
                case "groupsort":
                    //  TODO : devrait être appelé via un manager - 
                    _bFullWizard = false;
                    #region GroupSort

                    StringWriter sw = new StringWriter();
                    XmlTextWriter tw = new XmlTextWriter(sw);
                    String fieldList = String.Empty;

                    if (_allKeys.Contains("fieldlist") && !String.IsNullOrEmpty(Request.Form["fieldlist"]))
                        fieldList = Request.Form["fieldlist"];

                    if (_strErrorMessage.Length > 0)
                    {
                        LaunchError(eErrorContainer.GetDevUserError(
                                    eLibConst.MSG_TYPE.CRITICAL,
                                    eResApp.GetRes(_pref, 6390),
                                    eResApp.GetRes(_pref, 6342),
                                    eResApp.GetRes(_pref, 416),
                                   string.Concat(eResApp.GetRes(_pref, 6390), " - ", eResApp.GetRes(_pref, 6520), _strErrorMessage)));
                    }

                    Response.Clear();
                    Response.ClearContent();
                    Response.ContentType = "text/xml";

                    XmlDocument outPutDocument = eReportWizardRenderer.GetSortAndGroupAvailableFields(_pref, fieldList);
                    outPutDocument.WriteTo(tw);
                    Response.Write(sw.ToString());
                    Response.End();



                    break;

                #endregion
                case "selection":
                    wizardform.Target = "fratmp";

                    #region Assistant Sélections

                    // Descid du BKM
                    if (_allKeys.Contains("tab") && !String.IsNullOrEmpty(Request.Form["tab"]))
                        _iTab = eLibTools.GetNum(Request.Form["tab"].ToString());

                    // Descid table parente
                    if (_allKeys.Contains("tabfrom") && !String.IsNullOrEmpty(Request.Form["tabfrom"]))
                        _iTabFrom = eLibTools.GetNum(Request.Form["tabfrom"].ToString());

                    //JS spécifique
                    PageRegisters.AddScript("eSelectionWizard");
                    PageRegisters.AddScript("ePopup");
                    PageRegisters.AddScript("eFieldEditor");
                    PageRegisters.AddScript("eEngine");
                    PageRegisters.AddScript("eMain");
                    PageRegisters.AddScript("eButton");
                    PageRegisters.AddScript("eContextMenu");


                    //CSS
                    PageRegisters.AddCss("eSelectionWizard");
                    PageRegisters.AddCss("DrawingToolsModule");
                    PageRegisters.AddCss("WKTModule.min");

                    CreateSelectionWizardRenderer();

                    #endregion

                    break;
                default:

                    break;
            }


            //Rendu du wizard
            if (_bFullWizard)
            {
                #region Affectation du rendu dans le contenu de la page [commun à tous les Wizards]
                if (_mainRender != null && _mainRender.ErrorMsg.Length == 0)
                {
                    //déplace les éléments du conteneur généré (myMainList) vers le conteneur final (listcontent)
                    // On ne peut pas ajouter directement myMainList dans listcontent : il ne faut 
                    // pas ajouter le div englobant de myMainList (listContent.Controls.resp(_myMainList.PgContainer);)
                    // , cela perturbe js et css               
                    while (_mainRender.PgContainer.Controls.Count > 0)
                    {
                        this.content.Controls.Add(_mainRender.PgContainer.Controls[0]);
                    }

                }
                else
                {
                    LaunchError(eErrorContainer.GetDevUserError(
                                eLibConst.MSG_TYPE.CRITICAL,
                                eResApp.GetRes(_pref, 6390),
                                eResApp.GetRes(_pref, 6342),
                                eResApp.GetRes(_pref, 416),
                                string.Concat(eResApp.GetRes(_pref, 6390), " - ", eResApp.GetRes(_pref, 6520),
                                _mainRender == null ? eResApp.GetRes(_pref, 6521) : _mainRender.ErrorMsg)));
                }
                #endregion
            }

        }


        #region Méthodes de l'assistant Reporting
        /// <summary>
        /// Méthodes de la page pour générer le rendu de l'assistant Reporting
        /// </summary>
        /// <param name="reportType">Type de rapport</param>
        /// <param name="report">Rapport à ouvrir</param>
        /// <returns>ReportWizardRenderer pour cet assistant</returns>
        private eRenderer CreateReportWizardRenderer(TypeReport reportType, eReport report = null)
        {
            _strBodyAttributes = "onload=\"OnDocLoad();initDragOptRapport();\" onkeydown =\"OnKeyInput(event);\" class=\"report bodyWithScroll\"";
            _iWizardTotalStepNumber = eReportWizard.GetTotalSteps(reportType);

            int width = 0;
            int height = 0;
            Int32.TryParse(_strWidth, out width);
            Int32.TryParse(_strHeight, out height);

            eRenderer rdr = eRendererFactory.CreateReportWizardRenderer(_pref, width, height, _iTab, reportType, report);

            if (rdr.ErrorMsg.Length > 0)
            {
                String sDevMsg = String.Concat("Erreur sur la page : ", System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine);

                if (rdr.InnerException != null)
                    sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Exception Message : ", rdr.InnerException.Message, Environment.NewLine, "Exception StackTrace :", rdr.InnerException.StackTrace);

                if (rdr.ErrorMsg.Length > 0)
                    sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Error Message : ", rdr.ErrorMsg);

                ErrorContainer = eErrorContainer.GetDevUserError(
                eLibConst.MSG_TYPE.CRITICAL,
                eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                eResApp.GetRes(_pref, 72),  //   titre
                String.Concat(sDevMsg)

                );
                try
                {
                    LaunchError();
                }
                catch (eEndResponseException)
                {

                }
                catch (ThreadAbortException)
                {

                }
            }

            PageRegisters.RawScrip.Append(BuildReportWizardScript(reportType, report));


            return rdr;
        }

        /// <summary>
        /// Construit le javascript nécessaire au bon fonctionnement de l'assistant reporting
        /// </summary>
        /// <param name="reportType">Type de rapport</param>
        /// <param name="report">Objet Report pour le cas du chargement pour modification</param>
        /// <returns>Code Javascript de la page.</returns>
        private String BuildReportWizardScript(TypeReport reportType, eReport report = null)
        {



            Boolean bNewReport = report == null || report.IsNew;
            String js = String.Concat(
                        " var listIframe = top.document.getElementById(\"", _strFrameId, "\");", Environment.NewLine,
                        " var wizardIframe = top.document.getElementById(\"", _strParentFrameId, "\");", Environment.NewLine,
                        " var iCurrentStep = 1;", Environment.NewLine,
                        " var htmlTemplate = null;", Environment.NewLine,
                        " var htmlHeader = null;", Environment.NewLine,
                        " var htmlFooter = null;", Environment.NewLine,
                        " var iTotalSteps;", Environment.NewLine,
                        " var paramReportArray;", Environment.NewLine,
                        " var res_XXX;", Environment.NewLine,
                        " var oReport =  Object();", Environment.NewLine,
                        "function OnDocLoad() {",
                        Environment.NewLine,
                        "   res_XXX = \"Ajouter les rubriques du fichier @file\";",
                        Environment.NewLine,
                        "   iTotalSteps = @steps;",
                        Environment.NewLine,
                        "   ", eReportWizardRenderer.BuildJavascriptReportParams(report),
                        Environment.NewLine,
                        "   oReport =  new eReport(@reportType, @reportName, @userId, @tab, @reportId, @endProc);",
                        Environment.NewLine,
                        "   oReport.LoadParam(paramReportArray);",
                        Environment.NewLine,
                        String.Concat(" var aViewPerm = {\"id\" : @vpermid , \"mode\" : @vmode, \"level\" : @vlevel, \"user\" : @vuser};"),
                        Environment.NewLine,
                        String.Concat(" oReport.SetViewPerm(aViewPerm);"),
                        Environment.NewLine,
                        String.Concat(" var aUpdatePerm = {\"id\" : @upermid, \"mode\" : @umode, \"level\" : @ulevel, \"user\" : @uuser};"),
                        Environment.NewLine,

                        String.Concat(" oReport.SetUpdatePerm(aUpdatePerm);"),
                       Environment.NewLine,

                       String.Concat(" oReport.SetScheduleOrig(", !bNewReport && report.ScheduleParam.Length > 0 ? report.ScheduleParam : "null", ");"),
                       Environment.NewLine,

                       String.Concat(" oReport.SetSchedule(", !bNewReport && report.ScheduleParam.Length > 0 ? report.ScheduleParam : "null", ");"),
                        Environment.NewLine,

                        String.Concat(" oReport.SetIsScheduled(", !bNewReport && report.ScheduleParam.Length > 0 ? "1" : "0", ");"),
                        Environment.NewLine,



                        "   Init(", (reportType == TypeReport.CHARTS ? "'chart'" : "'report'"), ");",
                        Environment.NewLine,
                       "}"
                       );

            js = js.Replace("@steps", _iWizardTotalStepNumber.ToString());
            js = js.Replace("@tab", _iTab.ToString());

            js = js.Replace("@reportType", reportType.GetHashCode().ToString());

            js = js.Replace("@reportName", !bNewReport ? String.Concat("\"", HttpUtility.JavaScriptStringEncode(report.Name), "\"") : "\"\"");
            js = js.Replace("@userId", !bNewReport ? report.Owner.ToString() : "0");
            js = js.Replace("@reportId", bNewReport ? "0" : report.Id.ToString());
            js = js.Replace("@endProc", bNewReport ? "''" : String.Concat("\"", report.EndProcedure, "\""));

            js = js.Replace("@vmode", bNewReport ? "-1" : String.Concat("\"", report.ViewPerm?.PermMode.ToString() ?? "-1", "\""));
            js = js.Replace("@vlevel", bNewReport ? "0" : String.Concat("\"", report.ViewPerm?.PermLevel.ToString() ?? "0", "\""));
            js = js.Replace("@vuser", bNewReport ? "''" : String.Concat("\"", report.ViewPerm?.PermUser ?? "''", "\""));
            js = js.Replace("@vpermid", bNewReport ? "''" : String.Concat("\"", report.ViewPerm?.PermId.ToString() ?? "''", "\""));

            js = js.Replace("@umode", bNewReport ? "-1" : String.Concat("\"", report.UpdatePerm?.PermMode.ToString(), "\""));
            js = js.Replace("@ulevel", bNewReport ? "0" : String.Concat("\"", report.UpdatePerm?.PermLevel.ToString(), "\""));
            js = js.Replace("@uuser", bNewReport ? "''" : String.Concat("\"", report.UpdatePerm?.PermUser ?? "''", "\""));
            js = js.Replace("@upermid", bNewReport ? "''" : String.Concat("\"", report.UpdatePerm?.PermId.ToString() ?? "''", "\""));


            return js;
        }


        /// <summary>
        /// Retoure un controle de type dropdown contenant les tables liées
        /// à la table passé en paramètre du wizard
        /// </summary>
        /// <returns></returns>
        private HtmlGenericControl BuildLinkFileRenderer()
        {
            //PageRegisters.RawScrip.Append("return ;");
            _strBodyAttributes = "";
            HtmlGenericControl containerDiv = new HtmlGenericControl("div");
            HtmlGenericControl listDiv = new HtmlGenericControl("div");
            HtmlGenericControl fileListDiv = new HtmlGenericControl("div");
            HtmlGenericControl span = new HtmlGenericControl("span");
            DropDownList ddllinkedfrom = new DropDownList();

            containerDiv.Attributes.Add("class", "LinkFileResult");

            //Depuis
            span.InnerText = eResApp.GetRes(_pref, 535).ToCapitalize();
            listDiv.Controls.Add(span);
            ddllinkedfrom = GetLinkedFileList(_pref, _iTab, "editor_linkedfromlist");
            ddllinkedfrom.SelectedIndex = 0;
            ddllinkedfrom.Attributes.Add("onchange", "onChangeLinkFileTab(this);");
            listDiv.Controls.Add(ddllinkedfrom);
            containerDiv.Controls.Add(listDiv);

            listDiv = new HtmlGenericControl("div");

            span = new HtmlGenericControl("span");

            //Fichier
            fileListDiv.ID = "editor_filelist";
            fileListDiv.Style.Add("display", "inline");
            fileListDiv.Controls.Add(GetLinkedFileList(_pref, Int32.Parse(ddllinkedfrom.SelectedValue == "" ? "0" : ddllinkedfrom.SelectedValue), "editor_linkedfilelist"));
            fileListDiv.Style.Add(HtmlTextWriterStyle.Display, "inline");
            span.InnerText = eResApp.GetRes(_pref, 103);

            listDiv.Controls.Add(span);
            listDiv.Controls.Add(fileListDiv);
            containerDiv.Controls.Add(listDiv);

            return containerDiv;
        }




        /// <summary>
        /// Construit la liste déroulante des fichiers liés au tab transmis en paramètre
        /// </summary>
        /// <param name="pref">préférence de l'utilisateur en cours</param>
        /// <param name="selectedTab">Fichier cible</param>
        /// <param name="itemId">nom de la liste</param>
        /// <returns>DropDownList des fichiers liés</returns>
        private DropDownList GetLinkedFileList(ePref pref, int selectedTab, string itemId)
        {
            eudoDAL dal = eLibTools.GetEudoDAL(pref);
            dal.OpenDatabase();
            DropDownList ddllinkedFile = new DropDownList();
            ddllinkedFile.ID = itemId;
            ddllinkedFile.Attributes.Add("name", itemId);
            ddllinkedFile.SelectedIndex = 0;
            try
            {
                List<KeyValuePair<string, int>> liLinkedFileList = eReportWizard.GetLinkedFileList(dal, selectedTab, _pref);

                foreach (KeyValuePair<string, int> kvp in liLinkedFileList)
                    if (!selectedTab.Equals(kvp.Value))
                        ddllinkedFile.Items.Add(new ListItem(kvp.Key, kvp.Value.ToString()));
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                dal.CloseDatabase();
            }
            return ddllinkedFile;
        }
        #endregion

        #region Méthodes de l'assistant Mailing
        /// <summary>
        /// Méthodes de la page pour générer le rendu de l'assistant Mailing
        /// </summary>
        /// <param name="mailingType">Type de rapport</param>
        /// <param name="mailing">Rapport à ouvrir</param>
        /// <returns>MailingWizardRenderer pour cet assistant</returns>
        private eRenderer CreateMailingWizardRenderer(TypeMailing mailingType, eMailing mailing = null)
        {
            _strBodyAttributes = "onload=\"OnDocLoad();\" onkeydown =\"OnKeyInput(event);\"  class=\"bodyWithScroll\" ";


            if (mailingType == TypeMailing.SMS_MAILING_FROM_BKM || mailingType == TypeMailing.SMS_MAILING_UNDEFINED)
            {
                content.Attributes.Add("smsing", "1");
            }

            Int32 nWidth = -1, nHeight = -1;

            Int32.TryParse(_strHeight, out nHeight);
            Int32.TryParse(_strWidth, out nWidth);
            // le total d etape depends de certain parametres d'emailing
            //  _iWizardTotalStepNumber = eMailingWizard.GetTotalSteps(mailingType);
            eRenderer rdr = eRendererFactory.CreateMailingWizardRenderer(_pref, this._iTab, nHeight, nWidth, mailingType, out _iWizardTotalStepNumber, mailing);

            try
            {
                if (rdr.ErrorMsg.Length > 0)
                {
                    //if (rdr.ErrorNumber == QueryErrorType.ERROR_NUM_NO_RECIPIENTS_FOUND)
                    //{
                    //    ErrorContainer = eErrorContainer.GetUserError(
                    //        eLibConst.MSG_TYPE.QUESTION,
                    //        eResApp.GetRes(_pref, 6656),// "Aucune fiche destinataire trouvée !", 
                    //        eResApp.GetRes(_pref, 6657).Replace("<TAB>", eLibTools.GetPrefName(this._pref, this._iTab)),//  "Pour faire un e-mailing, vous devez ajouter des fiches <TAB>".Replace("<TAB>", eLibTools.GetPrefName(this._pref, this._iTab)), 
                    //        eResApp.GetRes(_pref, 5080));//Information
                    //    LaunchError();

                    //    return rdr;
                    //}




                    //Pas de fichier de type 'E-MAIL'
                    if (rdr.ErrorNumber == QueryErrorType.ERROR_NUM_MAIL_FILE_NOT_FOUND)
                    {
                        ErrorContainer = eErrorContainer.GetUserError(
                            eLibConst.MSG_TYPE.EXCLAMATION,
                            String.IsNullOrEmpty(rdr.ErrorMsg) ? eResApp.GetRes(_pref, 6341) : rdr.ErrorMsg, //Aucun fichier E-mail paramétré
                            eResApp.GetRes(_pref, 6342), //Veuillez contacter votre administrateur
                            eResApp.GetRes(_pref, 5080));//Information
                        LaunchError();

                    }
                    else
                    {

                        String sDevMsg = String.Concat("Erreur sur la page : ", System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine);

                        if (rdr.InnerException != null)
                            sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Exception Message : ", rdr.InnerException.Message, Environment.NewLine, "Exception StackTrace :", rdr.InnerException.StackTrace);

                        if (rdr.ErrorMsg.Length > 0)
                            sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Error Message : ", rdr.ErrorMsg);

                        //ALISTER => Demande/Requests 86 706
                        if (_pref.User.UserMail == "" && _pref.User.UserMailOther == "")
                        {
                            ErrorContainer = eErrorContainer.GetDevUserError(
                            eLibConst.MSG_TYPE.CRITICAL,
                            eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                            String.Concat(eResApp.GetRes(_pref, 2942), "<br>", eResApp.GetRes(_pref, 2943)),  //  Détail : pour améliorer...
                            eResApp.GetRes(_pref, 72),  //   titre
                            String.Concat(sDevMsg)

                            );

                        }
                        else
                        {
                            ErrorContainer = eErrorContainer.GetDevUserError(
                            eLibConst.MSG_TYPE.CRITICAL,
                            eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                            String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                            eResApp.GetRes(_pref, 72),  //   titre
                            String.Concat(sDevMsg)

                            );
                        }
                        LaunchError();
                    }

                }
            }
            catch (eEndResponseException) { Response.End(); }
            catch (ThreadAbortException)
            {

            }

            return rdr;
        }

        /// <summary>
        /// Construit le javascript nécessaire au bon fonctionnement de l'assistant Mailing par sms
        /// </summary>
        /// <param name="mailingType">Type de rapport</param>
        /// <param name="mailing">Objet Mailing pour le cas du chargement pour modification</param>
        /// <returns>Code Javascript de la page.</returns>
        private String BuildSmsMailingWizardScript(TypeMailing mailingType, eMailing mailing = null)
        {
            if (mailingType != TypeMailing.SMS_MAILING_FROM_BKM)
                throw new NotImplementedException("L'envoi de campagne sms est disponible uniquement en signet.");


            String js = String.Concat(
            " var _eCurrentSelectedMailTpl = null;", Environment.NewLine,
            " var _ePopupVNEditor;", Environment.NewLine,
            " var textareaevents = {'keyup' : \"oSmsing.UpdateSmsContent\" };", Environment.NewLine,
            " var listIframe = null; ", Environment.NewLine,
            " var wizardIframe = top.document.getElementById(\"", _strParentFrameId, "\");", Environment.NewLine,
            " var iCurrentStep = 1;", Environment.NewLine,
            " var htmlTemplate = null;", Environment.NewLine,
            " var htmlHeader = null;", Environment.NewLine,
            " var htmlFooter = null;", Environment.NewLine,
            " var iTotalSteps;", Environment.NewLine,
            "function OnDocLoad()", Environment.NewLine,
            "{", Environment.NewLine,
            "     iTotalSteps = ", ((eSmsMailingFileWizardRenderer)_mainRender).LstWizardStep.Count, Environment.NewLine,
            "     oSmsing.SetVersionPresta('@sPrestaSMS');", Environment.NewLine,
            "     Init('smsmailing');", Environment.NewLine,
            
            "     oSmsing.SetCampaignId(@nCampId);", Environment.NewLine,
            "     oSmsing.SetParentFileId(@nPrtFileId);", Environment.NewLine,
            "     oSmsing.SetParentTabId(@nPrtTab);", Environment.NewLine,
            "     oSmsing.SetParentBkmId(@nPrtBkm);", Environment.NewLine,
            "     oSmsing.SetBkmLabel('@sLabel');", Environment.NewLine,
            "     oSmsing.OnSelectDelayed_Now();", Environment.NewLine,

            " oSmsing.LstStep = '", HttpUtility.JavaScriptStringEncode(JsonConvert.SerializeObject(((eSmsMailingFileWizardRenderer)_mainRender).LstWizardStep)), "';", Environment.NewLine,


            "}");

            js = js.Replace("@nCampId", mailing == null ? "0" : mailing.Id.ToString());
            js = js.Replace("@nPrtBkm", this._iTab.ToString());
            js = js.Replace("@sPrestaSMS", this._sPresta.ToString());
            js = js.Replace("@nPrtFileId", this._iFileId.ToString());
            js = js.Replace("@nPrtTab", _iTabFrom.ToString());

            js = js.Replace("@sLabel", HttpUtility.JavaScriptStringEncode(eLibTools.GetPrefName(this._pref, this._iTab)))  ;


            return js;
        }


        /// <summary>
        /// Construit le javascript nécessaire au bon fonctionnement de l'assistant Mailing
        /// </summary>
        /// <param name="mailingType">Type de rapport</param>
        /// <param name="mailing">Objet Mailing pour le cas du chargement pour modification</param>
        /// <returns>Code Javascript de la page.</returns>
        private String BuildMailingWizardScript(TypeMailing mailingType, eMailing mailing = null)
        {
            Boolean bNewMailing = mailing == null;

            String sUserSignature;

            if (!eUser.GetFieldValue<String>(_pref, this._pref.User.UserId, "UserSignature", out sUserSignature))
                sUserSignature = String.Empty;




            Int32 nType = eConst.eFileType.FILE_CONSULT.GetHashCode();
            // Sauf si le FileId n'est pas renseigné, auquel cas on passe en mode CREATION
            if (this._iFileId == 0)
                nType = eConst.eFileType.FILE_CREA.GetHashCode();

            // Chargement des onload des composants javascript
            // ATTENTION : certains de ces éléments doivent être reportés dans eFilterReportListDialog.aspx.cs pour les modèles de mails
            // unitaires

            String js = String.Concat(
            "   var _eCurrentSelectedMailTpl = null;", Environment.NewLine,
            "   var _ePopupVNEditor;", Environment.NewLine,
            "   var _eMailTplNameEditor;", Environment.NewLine,

            "var listIframe = null; ",
            "var wizardIframe = top.document.getElementById(\"",
            _strParentFrameId,
            "\");",
            "   var iCurrentStep = 1;",
            "var htmlTemplate = null;",
            Environment.NewLine,
            "var htmlHeader = null;",
            Environment.NewLine,
            "var htmlFooter = null;",
            Environment.NewLine,
            " var iTotalSteps;",
            Environment.NewLine,
            " var paramMailingArray;",
             Environment.NewLine,
            " var dicMailFields;",
            Environment.NewLine,
            " var res_XXX;",
            Environment.NewLine,
            " var oMailing = Object();",
            Environment.NewLine,
            "function OnDocLoad() {",
            Environment.NewLine,
            "initHeadEvents();",
            Environment.NewLine,
            "var dicMailAdresses = ",
            eTools.JavaScriptSerialize(mailing.EmailFields),
            ";",
            Environment.NewLine,
            "res_XXX = \"Ajouter les rubriques du fichier @file\";",
            Environment.NewLine,
            "iTotalSteps = @steps;",
            Environment.NewLine,


            ((eMailingWizardRenderer)_mainRender).BuildJavascriptMailingParams(mailing),


            "oMailing =  new eMailing(@mailingId, @CampaignTab, @mailingType, @tab, @parentTab, @parentFileId);",
            Environment.NewLine,
            "oMailing.SetMailAdresses(dicMailAdresses);",
            Environment.NewLine,
            "oMailing.LoadParam(paramMailingArray);",
            Environment.NewLine,
            "Init('mailing');",
            Environment.NewLine,
            "var oParam = getParamWindow();",
            Environment.NewLine,
            "oParam.SetParam('UserSignature', '@UserSign');", Environment.NewLine,
            "if (oMailing.GetParam('immediateSending')!='1' && oMailing.GetParam('recurrentSending')!='1')", Environment.NewLine,
            "   oMailing.OnSelectDelayed_Later();", Environment.NewLine,
            "else if (oMailing.GetParam('immediateSending')!='1' && oMailing.GetParam('recurrentSending')=='1')", Environment.NewLine,
            "   oMailing.OnSelectDelayed_Recurrent();", Environment.NewLine,
            "else", Environment.NewLine,
            "   oMailing.OnSelectDelayed_Now();", Environment.NewLine,
            "   ", Environment.NewLine,
            "if ( @mailingId >0) { ", Environment.NewLine,
            "   if (@steps==3) ", Environment.NewLine,
            "       SwitchStep(2);", Environment.NewLine,
            " else { ", Environment.NewLine,
            "        SwitchStep(2);", Environment.NewLine,
            "       oMailing.SetCssExistingCampaign();", Environment.NewLine,
            "  }",
            "}",
            "}"
            );


            js = js.Replace("@steps", _iWizardTotalStepNumber.ToString());

            js = js.Replace("@UserSign", HttpUtility.JavaScriptStringEncode(sUserSignature));
            js = js.Replace("@mailingId", mailing.Id.ToString());
            js = js.Replace("@CampaignTab", TableType.CAMPAIGN.GetHashCode().ToString());
            js = js.Replace("@mailingType", mailingType.GetHashCode().ToString());
            js = js.Replace("@tab", this._iTab.ToString());

            if (mailingType == TypeMailing.MAILING_FROM_BKM || mailingType == TypeMailing.SMS_MAILING_FROM_BKM || mailingType == TypeMailing.MAILING_FOR_MARKETING_AUTOMATION)
                js = js.Replace("@parentFileId", this._iFileId.ToString());
            else
                js = js.Replace("@parentFileId", mailing.ParentFileId.ToString());

            if (mailingType == TypeMailing.MAILING_FROM_CAMPAIGN)
                js = js.Replace("@parentTab", mailing.ParentTab.ToString());
            else
                js = js.Replace("@parentTab", _iTabFrom.ToString());

            return js;
        }



        #endregion

        #region Méthodes de l'assistant de creation de formulaire

        /// <summary>
        /// Méthodes de la page pour générer le rendu de l'assistant formulaire
        /// </summary>
        /// <param name="nBkmTab">table ++/cible etendue</param>
        /// <param name="nParentFileId"></param>
        /// <param name="nFormularId"></param>
        /// <param name="nHeight">Hauteur de la popup de formulaire</param>
        /// <returns>FormularWizardRenderer pour cet assistant</returns>
        private eRenderer CreateFormularXrmWizardRenderer(Int32 nBkmTab, Int32 nParentFileId, Int32 nFormularId, Int32 nHeight)
        {

            //TODO MOU REFACTORING
            _strBodyAttributes = "onload=\"OnDocLoad();\" onkeydown =\"OnKeyInput(event);\" class=\"bodyWithScroll\"  ";

            _iWizardTotalStepNumber = eFormularXrmWizardRenderer.TOTAL_STEPS;



            Int32 nHeightParent;
            if (!Int32.TryParse(_strDocHeight, out nHeightParent))
            {
                nHeightParent = eConst.DEFAULT_WINDOW_HEIGHT;
            }


            eRenderer rdr = eRendererFactory.CreateFormularXrmWizardRenderer(_pref, nBkmTab, nParentFileId, nFormularId, nHeight, nHeightParent);

            if (!String.IsNullOrEmpty(rdr.ErrorMsg))
                _strErrorMessage = String.Concat(_strErrorMessage, rdr.ErrorMsg);

            if (!String.IsNullOrEmpty(_strErrorMessage))
            {
                if (rdr.InnerException != null && rdr.InnerException is eFormularException)
                {
                    eFormularException ex = (eFormularException)rdr.InnerException;
                    LaunchError(eErrorContainer.GetDevUserError(
                              eLibConst.MSG_TYPE.CRITICAL,
                              ex.UserMessage,//TODO MOU RES
                              "Pour plus d'information, veuillez contacter votre administrateur.",
                              "Permissions",//TODO MOU RES titre
                               ex.Message + "  ErrorCode = " + ex.Code.GetHashCode().ToString() + " " + ex.StackTrace));

                }
                else
                {

                    LaunchError(eErrorContainer.GetDevUserError(
                              eLibConst.MSG_TYPE.CRITICAL,
                              eResApp.GetRes(_pref, 6519),
                              eResApp.GetRes(_pref, 6342),
                              eResApp.GetRes(_pref, 416),
                             string.Concat(
                                 eResApp.GetRes(_pref, 6519), " - ", eResApp.GetRes(_pref, 6520), _strErrorMessage)
                             ));
                }
            }



            PageRegisters.RawScrip.AppendLine().Append(BuildFormularXrmWizardScript());

            return rdr;
        }


        /// <summary>
        /// Construit le javascript nécessaire au bon fonctionnement de l'assistant de formulaire
        /// </summary>
        /// <param name="mailingType">Type de rapport</param>
        /// <param name="mailing">Objet Mailing pour le cas du chargement pour modification</param>
        /// <returns>Code Javascript de la page.</returns>
        private String BuildFormularXrmWizardScript()
        {

            // Chargement des onload des composants javascript

            String js = String.Concat(Environment.NewLine,
            "   var _eCurrentSelectedMailTpl = null;", Environment.NewLine,
            "   var _ePopupVNEditor;", Environment.NewLine,
            "   var iCurrentStep = 1;", Environment.NewLine,
            "   var htmlTemplate = null;", Environment.NewLine,
            "   var htmlHeader = null;", Environment.NewLine,
            "   var htmlFooter = null;", Environment.NewLine,
            "   var iTotalSteps;", Environment.NewLine,
            "   var oFormular;", Environment.NewLine,
            "   function OnDocLoad() {", Environment.NewLine,

            "      listIframe.contentWindow.modalWizard = top['_md']['FormularWizard'];", Environment.NewLine,
            "      oFormular =  new eFormular();", Environment.NewLine,
            "      oFormular.LoadParam(oParams);", Environment.NewLine,
            "      oFormular.GetPerm().SetViewPerm(oViewPerm);", Environment.NewLine,
            "      oFormular.GetPerm().SetUpdatePerm(oUpdatePerm);", Environment.NewLine,
            "      initHeadEvents();", Environment.NewLine,
            "      iTotalSteps =", _iWizardTotalStepNumber, "; ", Environment.NewLine,
            "      Init('formular');", Environment.NewLine,
            "CKEDITOR.on('dialogDefinition', function (ev) {", Environment.NewLine,
            "if (ev.data.name == 'link') { ", Environment.NewLine,
             "ev.data.definition.getContents('target').get('linkTargetType')['default'] = '_blank';", Environment.NewLine,
             "}", Environment.NewLine,
             "});", Environment.NewLine,
             " }", Environment.NewLine

        );

            return js;
        }


        #endregion



        #region Méthodes de l'assistant d'import

        /// <summary>
        /// Création de l'assistant d'import
        /// </summary>
        /// <param name="importParams">Paramètres d'import</param>        
        /// <exception cref="ArgumentException">Si <see cref="eImportWizardParam.ImportTab"/> n'est pas un descid de la table</exception>
        /// <returns></returns>
        private eRenderer CreateImportWizardRenderer(eImportWizardParam importParams)
        {
            // la table de destination doit etre valide
            if (importParams.ImportTab <= 0 || importParams.ImportTab % 100 != 0)
                throw new ArgumentException("DescId de la table d'import n'est pas valide !");

            _iWizardTotalStepNumber = 5;
            _strBodyAttributes = "onload=\"OnImportDoc();\" onkeydown =\"OnKeyInput(event);\" class=\"bodyWithScroll\"  ";

            eBaseWizardRenderer rdr = (eBaseWizardRenderer)eRendererFactory.CreateImportWizardRenderer(_pref, importParams);

            PageRegisters.RawScrip.AppendLine().Append(rdr.GetInitJS());

            return rdr;
        }

        #endregion


        #region Wizard Select


        /// <summary>
        /// Génération du contenu d'un wizard ++
        /// </summary>
        /// <param name="bDelete">Mode suppression</param>
        private void CreateInvitSelectWizardRenderer(Boolean bDelete)
        {

            //Attributs de la balise BODY
            _strBodyAttributes = "onload=\"OnDocLoad();\" onkeydown =\"OnKeyInput(event);\" class=\"mailing bodyWithScroll\"";

            //RaZ des filtres sur la liste des filtres
            List<SetParam<ePrefConst.PREF_PREF>> prefFilter = new List<SetParam<ePrefConst.PREF_PREF>>();
            prefFilter.Add(new SetParam<ePrefConst.PREF_PREF>(ePrefConst.PREF_PREF.BKMFILTERCOL, FilterField.USERID.GetHashCode().ToString()));
            prefFilter.Add(new SetParam<ePrefConst.PREF_PREF>(ePrefConst.PREF_PREF.BKMFILTEROP, "8"));
            prefFilter.Add(new SetParam<ePrefConst.PREF_PREF>(ePrefConst.PREF_PREF.BKMFILTERVALUE, String.Concat("0;", this._pref.User.UserId)));
            this._pref.SetPref(TableType.FILTER.GetHashCode(), prefFilter);

            int width = 0;
            int height = 0;
            Int32.TryParse(_strWidth, out width);
            Int32.TryParse(_strHeight, out height);

            _mainRender = eRendererFactory.CreateSelectInvitWizardRenderer(_pref, _iTab, _iTabFrom, _iFileId, width, height, bDelete);

            #region erreur
            if (_mainRender.ErrorMsg.Length > 0)
            {


                String sDevMsg = String.Concat("Erreur sur la page : ", System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine);

                if (_mainRender.InnerException != null)
                    sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Exception Message : ", _mainRender.InnerException.Message, Environment.NewLine, "Exception StackTrace :", _mainRender.InnerException.StackTrace);

                if (_mainRender.ErrorMsg.Length > 0)
                    sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Error Message : ", _mainRender.ErrorMsg);

                ErrorContainer = eErrorContainer.GetDevUserError(
                eLibConst.MSG_TYPE.CRITICAL,
                eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                eResApp.GetRes(_pref, 72),  //   titre
                String.Concat(sDevMsg)

                );
                LaunchError();

            }

            #endregion

            //Chargment du script du wizard
            PageRegisters.RawScrip.AppendLine().Append(((eBaseWizardRenderer)_mainRender).GetInitJS());
        }

        #endregion

        #region Méthode de l'assistant de traitment de duplication


        /// <summary>
        /// Retourne le wizard de traitement de duplication
        /// </summary>
        /// <returns></returns>
        private eRenderer CreateDuplicationTreatmentdRenderer()
        {


            _strBodyAttributes = "onload=\"onLoadDupliTreat();\"  ";

            eRenderer rdr = eRendererFactory.CreateDuplicationTreatmentWizardRenderer(_pref, _iTab, 800, 600);

            if (rdr.ErrorMsg.Length > 0)
            {
                String sDevMsg = String.Concat("Erreur sur la page : ", System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine);

                if (rdr.InnerException != null)
                    sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Exception Message : ", rdr.InnerException.Message, Environment.NewLine, "Exception StackTrace :", rdr.InnerException.StackTrace);

                if (rdr.ErrorMsg.Length > 0)
                    sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Error Message : ", rdr.ErrorMsg);

                ErrorContainer = eErrorContainer.GetDevUserError(
                eLibConst.MSG_TYPE.CRITICAL,
                eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                eResApp.GetRes(_pref, 72),  //   titre
                String.Concat(sDevMsg)

                );
                LaunchError();
            }

            PageRegisters.RawScrip.AppendLine().Append(((eBaseWizardRenderer)rdr).GetInitJS());


            return rdr;
        }
        #endregion

        #region Méthodes de l'assistant de sélection des fiches à partir des critères

        private void CreateSelectionWizardRenderer()
        {
            _strBodyAttributes = "onload=\"OnDocLoad();\" onkeydown =\"OnKeyInput(event);\" class=\"bodyWithScroll\"";


            int width = 0;
            int height = 0;
            Int32.TryParse(_strWidth, out width);
            Int32.TryParse(_strHeight, out height);

            int tabSource = _iTabFrom;

            _mainRender = eRendererFactory.CreateSelectionWizardRenderer(_pref, width, height, _iTab, tabSource);

            #region erreur
            if (_mainRender.ErrorMsg.Length > 0)
            {


                String sDevMsg = String.Concat("Erreur sur la page : ", System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine);

                if (_mainRender.InnerException != null)
                    sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Exception Message : ", _mainRender.InnerException.Message, Environment.NewLine, "Exception StackTrace :", _mainRender.InnerException.StackTrace);

                if (_mainRender.ErrorMsg.Length > 0)
                    sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Error Message : ", _mainRender.ErrorMsg);

                ErrorContainer = eErrorContainer.GetDevUserError(
                eLibConst.MSG_TYPE.CRITICAL,
                eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                eResApp.GetRes(_pref, 72),  //   titre
                String.Concat(sDevMsg)

                );
                LaunchError();

            }

            #endregion

            //Chargment du script du wizard
            PageRegisters.RawScrip.AppendLine().Append(((eBaseWizardRenderer)_mainRender).GetInitJS());
            scriptHolder.Controls.Add(new LiteralControl("<script type='text/javascript' src='http://ecn.dev.virtualearth.net/mapcontrol/mapcontrol.ashx?v=7.0&mkt=fr-FR'></script>"));

        }

        #endregion
    }
}