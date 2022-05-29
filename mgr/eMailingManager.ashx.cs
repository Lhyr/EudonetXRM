using Com.Eudonet.Internal;
using Com.Eudonet.Merge;
using EudoQuery;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.mgr
{
    /// <className>eMailingManager</className>
    /// <summary>
    /// Manager qui permet :
    ///     - les actions d'e-mailing Envoi d'un e-Mailing, Annulation d'un e-mailing
    ///     - La création/Mise à jours de la Campagne
    /// </summary>
    /// <authors>
    /// GCH : CANCEL
    /// MOU : 
    /// </authors>
    /// <date>2014-01-14</date>
    public class eMailingManager : eEudoManager
    {
        protected eMailing oMailing;
        /// <summary>
        /// Action d'e-mailing possible
        /// </summary>
        protected eMailing.MailingAction _action = eMailing.MailingAction.NONE;
        protected ExtendedDictionary<String, String> _param = new ExtendedDictionary<string, string>();
        protected XmlDocument _xmlDocReturn = new XmlDocument();

        protected Int32 _nMailingId = 0;
        protected TypeMailing _mailingType = TypeMailing.MAILING_FROM_LIST;
        protected Int32 _nTab = 0;

        protected Int32 _nParentTab = 0;
        protected Int32 _nParentFileId = 0;
        protected String _pjIds = String.Empty;

        protected Int32 lifeTimeInMonth;
        protected Int32 purgeTimeInMonth;

        protected Int32 countUntrackedLinks;

        protected Int32 _nEventStepFileId = 0;

        protected eudoDAL _edal;

        //Gestion des erreurs
        protected eMailingException _innerException;

        //gérer les erreurs mailTester
        private bool _ErrorOnMailTester = false;
        private string _ErrorMailTester = string.Empty;

        /// <summary>
        /// Constructeur du manager, constructeur ASHX standard.
        /// </summary>
        protected override void ProcessManager()
        {
            try
            {
                _edal = eLibTools.GetEudoDAL(_pref);
                _edal.OpenDatabase();

                RetrieveParams();
                CheckValidParams();
                ProcessAction();
            }
            catch (eMailingException ex)
            {
                this._innerException = ex;
            }
            finally
            {
                if (_edal != null)
                    _edal.CloseDatabase();
            }

            RenderXmlResult();
        }

        /// <summary>
        /// Récupère les parametres depuis le formulaire de request
        /// </summary>
        protected virtual void RetrieveParams()
        {


            if (_allKeys.Contains("operation") && _context.Request.Form["operation"] != null)
                _action = (eMailing.MailingAction)Int32.Parse(_context.Request.Form["operation"]);

            if (_allKeys.Contains("typeMailing") && _context.Request.Form["typeMailing"] != null)
                _mailingType = (TypeMailing)Int32.Parse(_context.Request.Form["typeMailing"]);

            if (_allKeys.Contains("tab") && _context.Request.Form["tab"] != null)
                Int32.TryParse(_context.Request.Form["tab"].ToString(), out _nTab);

            if (_allKeys.Contains("parenttab") && _context.Request.Form["parenttab"] != null)
                Int32.TryParse(_context.Request.Form["parenttab"].ToString(), out _nParentTab);

            if (_allKeys.Contains("parentfileid") && _context.Request.Form["parentfileid"] != null)
                Int32.TryParse(_context.Request.Form["parentfileid"].ToString(), out _nParentFileId);

            if (_allKeys.Contains("fileid") && _context.Request.Form["fileid"] != null)
                Int32.TryParse(_context.Request.Form["fileid"].ToString(), out _nMailingId);

            if (_allKeys.Contains("description") && _context.Request.Form["description"] != null)
                this._param["description"] = _context.Request.Form["description"].ToString();

            if (_allKeys.Contains("templateId") && _context.Request.Form["templateId"] != null)
                this._param["templateId"] = _context.Request.Form["templateId"].ToString();

            if (_allKeys.Contains("libelle") && _context.Request.Form["libelle"] != null)
                this._param["libelle"] = _context.Request.Form["libelle"].ToString();

            if (_allKeys.Contains("mediaType") && _context.Request.Form["mediaType"] != null)
                this._param["mediaType"] = _context.Request.Form["mediaType"].ToString();

            if (_allKeys.Contains("category") && _context.Request.Form["category"] != null)
                this._param["category"] = _context.Request.Form["category"].ToString();

            //TODO INTERNALISATION EMAILING #36007
            if (_allKeys.Contains("trackLnkLifeTime") && _context.Request.Form["trackLnkLifeTime"] != null)
                this._param["trackLnkLifeTime"] = _context.Request.Form["trackLnkLifeTime"].ToString();

            //TODO INTERNALISATION EMAILING #36007
            if (_allKeys.Contains("trackLnkPurgedDate") && _context.Request.Form["trackLnkPurgedDate"] != null)
                this._param["trackLnkPurgedDate"] = _context.Request.Form["trackLnkPurgedDate"].ToString();

            //TODO INTERNALISATION EMAILING #36007
            if (_allKeys.Contains("sendingDate") && _context.Request.Form["sendingDate"] != null)
                this._param["sendingDate"] = _context.Request.Form["sendingDate"].ToString();

            if (_allKeys.Contains("immediateSending") && _context.Request.Form["immediateSending"] != null)
                this._param["immediateSending"] = _context.Request.Form["immediateSending"].ToString();

            if (_allKeys.Contains("recurrentSending") && _context.Request.Form["recurrentSending"] != null)
                this._param["recurrentSending"] = _context.Request.Form["recurrentSending"].ToString();

            if (_allKeys.Contains("scheduleUpdated") && _context.Request.Form["scheduleUpdated"] != null)
                this._param["scheduleUpdated"] = _context.Request.Form["scheduleUpdated"].ToString();

            if (_allKeys.Contains("eventStepDescId") && _context.Request.Form["eventStepDescId"] != null)
                this._param["eventStepDescId"] = _context.Request.Form["eventStepDescId"].ToString();

            if (_allKeys.Contains("eventStepFileId") && _context.Request.Form["eventStepFileId"] != null)
                this._param["eventStepFileId"] = _context.Request.Form["eventStepFileId"].ToString();

            if (_allKeys.Contains("scheduleId") && _context.Request.Form["scheduleId"] != null)
                this._param["scheduleId"] = _context.Request.Form["scheduleId"].ToString();

            if (_allKeys.Contains("recipientsFilterId") && _context.Request.Form["recipientsFilterId"] != null)
                this._param["recipientsFilterId"] = _context.Request.Form["recipientsFilterId"].ToString();

            if (_allKeys.Contains("sendingTimeZone") && _context.Request.Form["sendingTimeZone"] != null)
                this._param["sendingTimeZone"] = _context.Request.Form["sendingTimeZone"].ToString();

            if (_allKeys.Contains("RequestMode") && _context.Request.Form["RequestMode"] != null)
                this._param["RequestMode"] = _context.Request.Form["RequestMode"].ToString();

            if (_allKeys.Contains("mailTabDescId") && _context.Request.Form["mailTabDescId"] != null)
                this._param["mailTabDescId"] = _context.Request.Form["mailTabDescId"].ToString();

            if (_allKeys.Contains("markedFiles") && _context.Request.Form["markedFiles"] != null)
                this._param["markedFiles"] = _context.Request.Form["markedFiles"].ToString();

            if (_allKeys.Contains("mainAdress") && _context.Request.Form["mainAdress"] != null)
                this._param["mainAdress"] = _context.Request.Form["mainAdress"].ToString();

            if (_allKeys.Contains("activeAdress") && _context.Request.Form["activeAdress"] != null)
                this._param["activeAdress"] = _context.Request.Form["activeAdress"].ToString();

            if (_allKeys.Contains("templateType") && _context.Request.Form["templateType"] != null)
                this._param["templateType"] = _context.Request.Form["templateType"].ToString();

            if (_allKeys.Contains("bodyCss") && _context.Request.Form["bodyCss"] != null)
                this._param["bodyCss"] = _context.Request.Form["bodyCss"].ToString();

            if (_allKeys.Contains("recipientstest") && _context.Request.Form["recipientstest"] != null)
                this._param["recipientstest"] = _context.Request.Form["recipientstest"].ToString();

            if (_allKeys.Contains("pjids") && _context.Request.Form["pjids"] != null)
                _pjIds = _context.Request.Form["pjids"].ToString();

            // #39983 : ajout du sender
            if (_allKeys.Contains("sender") && _context.Request.Form["sender"] != null)
                this._param["sender"] = _context.Request.Form["sender"].ToString();

            // Dédoublonner les adresse mail
            if (_allKeys.Contains("removeDoubles") && _context.Request.Form["removeDoubles"] != null)
                this._param["removeDoubles"] = _context.Request.Form["removeDoubles"].ToString();

            //cf. 29277 on ne peut plus outrepasser le desbonnement
            //if (_allKeys.Contains("excludeUnsub") && _context.Request.Form["excludeUnsub"] != null)
            this._param["excludeUnsub"] = "1";//  _context.Request.Form["excludeUnsub"].ToString();

            if (_allKeys.Contains("ownerUserId") && _context.Request.Form["ownerUserId"] != null)
            {
                this._param["ownerUserId"] = _context.Request.Form["ownerUserId"].ToString();
                eUserInfo userInfo = new eUserInfo(eLibTools.GetNum(this._param["ownerUserId"]), _edal);
                this._param["ownerMainEmail"] = userInfo.UserMail;
            }
            // Modèle de mail choisi
            if (_allKeys.Contains("templateId") && _context.Request.Form["templateId"] != null)
            {
                this._param["templateId"] = _context.Request.Form["templateId"].ToString();
            }
            //Consentement
            if (_allKeys.Contains("OptInEnabled") && _context.Request.Form["OptInEnabled"] != null)
            {
                this._param["OptInEnabled"] = _context.Request.Form["OptInEnabled"].ToString();
            }

            if (_allKeys.Contains("OptOutEnabled") && _context.Request.Form["OptOutEnabled"] != null)
            {
                this._param["OptOutEnabled"] = _context.Request.Form["OptOutEnabled"].ToString();
            }

            if (_allKeys.Contains("NoConsentEnabled") && _context.Request.Form["NoConsentEnabled"] != null)
            {
                this._param["NoConsentEnabled"] = _context.Request.Form["NoConsentEnabled"].ToString();
            }

            //qualité adresse email    

            if (_allKeys.Contains("AdressEmailSwValideEnabled") && _context.Request.Form["AdressEmailSwValideEnabled"] != null)
                this._param["AdressEmailSwValideEnabled"] = _context.Request.Form["AdressEmailSwValideEnabled"].ToString();
            if (_allKeys.Contains("AdressEmailSwNotVerifiedEnabled") && _context.Request.Form["AdressEmailSwNotVerifiedEnabled"] != null)
                this._param["AdressEmailSwNotVerifiedEnabled"] = _context.Request.Form["AdressEmailSwNotVerifiedEnabled"].ToString();
            if (_allKeys.Contains("AdressEmailSwInvalideEnabled") && _context.Request.Form["AdressEmailSwInvalideEnabled"] != null)
                this._param["AdressEmailSwInvalideEnabled"] = _context.Request.Form["AdressEmailSwInvalideEnabled"].ToString();

            this._param["scoring"] = _requestTools.GetRequestFormKeyS("scoring");

            //On récupère le type du test pour lancer mail-tester
            this._param["MailTestType"] = _requestTools.GetRequestFormKeyS("MailTestType");
            if (this._param["MailTestType"] != null && this._param["MailTestType"] == "1" && eExtension.IsReadyStrict(_pref, "QUALITYMAIL", true))
            {
                this._param["MailTesterReportId"] = Internal.mailchecker.eMailCheckerTools.CreateMailTesterReportId(_pref);//on crée le rapport Id si l'extension mail tester est activé
                this._param["MailTesterToken"] = Internal.mailchecker.eMailCheckerTools.GetMailTesterSettings(_pref).DecryptedToken;//on ajoute le token mailtester
            }
            LoadMailFields();
        }

        /// <summary>
        /// Recupéré les champs du mail
        /// Note : Les champs sont serialisés par engine.js
        /// </summary>
        protected virtual void LoadMailFields()
        {
            //copie invisible
            this._param["ccrecipient"] = _requestTools.GetRequestFormKeyS("ccrecipient") ?? string.Empty;

            //Nom apparent
            this._param["displayName"] = _requestTools.GetRequestFormKeyS("displayName") ?? string.Empty;

            //Destinataires
            this._param["mailFieldDescId"] = _requestTools.GetRequestFormKeyS("mailFieldDescId") ?? string.Empty;

            //Nom de domaine
            this._param["senderAliasDomain"] = _requestTools.GetRequestFormKeyS("senderAliasDomain") ?? string.Empty;

            //Les autres champs du mail
            eUpdateField updField;
            foreach (String s in _requestTools.AllKeys)
            {
                // parse la chaine transmise pour obtenir les informations du champ
                // cf eEngine.js fldUpdEngine.GetSerialize()
                if (!s.StartsWith("field_"))
                    continue;

                updField = eUpdateField.GetUpdateFieldFromDesc(_context.Request.Form[s]);
                if (updField == null)
                    continue;

                if (eLibTools.GetTabFromDescId(updField.Descid) != TableType.CAMPAIGN.GetHashCode())
                    continue;

                switch ((CampaignField)updField.Descid)
                {
                    case CampaignField.BODY:
                        {
                            bool _MailBodyNcharPerLine = false;
                            var dicConfigAdv = eLibTools.GetConfigAdvValues(_pref, new System.Collections.Generic.List<eLibConst.CONFIGADV>() { eLibConst.CONFIGADV.SPLITTING_MAIL_BODY_NCHAR_PER_LINE });
                            if (dicConfigAdv.ContainsKey(eLibConst.CONFIGADV.SPLITTING_MAIL_BODY_NCHAR_PER_LINE) && !string.IsNullOrEmpty(dicConfigAdv[eLibConst.CONFIGADV.SPLITTING_MAIL_BODY_NCHAR_PER_LINE]))
                                _MailBodyNcharPerLine = dicConfigAdv[eLibConst.CONFIGADV.SPLITTING_MAIL_BODY_NCHAR_PER_LINE] == "1";
                            this._param["body"] = _MailBodyNcharPerLine ? eLibTools.GetMailBodyAbout70CharPerLine(HttpUtility.HtmlDecode(updField.NewValue)) : HttpUtility.HtmlDecode(updField.NewValue);
                        }
                        break;
                    case CampaignField.HISTO:
                        this._param["histo"] = updField.NewValue;
                        break;

                    case CampaignField.SENDER:
                        updField.NewValue = new eMailAddressConteneur(updField.NewValue).FirstAddress.Mail;
                        this._param["sender"] = updField.NewValue;
                        if (!eLibTools.IsEmailAddressValid(updField.NewValue))
                            throw new eMailingException(eErrorCode.INVALID_EMAIL_FORMAT, delegate (String res) { return res.Replace("<VALUE>", updField.NewValue).Replace("<FIELD>", updField.NewDisplay); });
                        break;

                    case CampaignField.REPLYTO:
                        eMailAddressConteneur mConteneur = new eMailAddressConteneur(updField.NewValue);
                        this._param["replyTo"] = updField.NewValue;
                        if (updField.NewValue.Length > 0 && (mConteneur.FirstAddress == null || mConteneur.InvalidAddress.Length > 0))
                            throw new eMailingException(eErrorCode.INVALID_EMAIL_FORMAT,
                                                        delegate (String res) { return res.Replace("<VALUE>", updField.NewValue).Replace("<FIELD>", updField.NewDisplay); });
                        break;

                    case CampaignField.SUBJECT:
                        this._param["subject"] = HttpUtility.HtmlDecode(updField.NewValue);
                        break;

                    //SHA : tâche #1 939
                    case CampaignField.PREHEADER:
                        this._param["preheader"] = HttpUtility.HtmlDecode(updField.NewValue);
                        break;

                    case CampaignField.ISHTML:
                        this._param["HtmlFormat"] = updField.NewValue;
                        break;
                    case CampaignField.BCCRECIPIENTS:
                        this._param["bccrecipients"] = updField.NewValue;
                        break;
                }
            }

        }




        /// <summary>
        /// Vérifie les parametres transmis depuis l assistant
        /// et les regles metier sur differente valeur (date d envoi > aujourd hui .....) 
        /// 
        /// </summary>
        protected virtual void CheckValidParams()
        {
            if (_action == eMailing.MailingAction.CANCEL)
                return;


            if (_action == eMailing.MailingAction.RESET_MAIL_TESTER)
                return;

            if (_action == eMailing.MailingAction.CHECK_LINKS)
            {
                eAnalyzerInfos infos = eMergeTools.AnalyzeBody(this._param["body"]);
                //int nbLinks = infos.linksData.CountLinks;
                countUntrackedLinks = infos.linksData.CountLinksWithoutField;

                if (countUntrackedLinks > 0)
                {
                    throw new eMailingException(eErrorCode.UNTRACKED_LINKS, "", countUntrackedLinks + " liens non liés à une rubrique");
                }
                return;
            }

            DateTime dateNow = DateTime.Now;

            //la date d'envoi est immediate
            DateTime sendingDate = dateNow;
            DateTime datePurge = new DateTime();
            DateTime dateLife = new DateTime();

            /*
             * lifeTimeInMonth et purgeTimeInMonth sont initialisées dans eConst, réunion GBO/HLA/GCH du 10/02/2014
             * si les valeurs par defaut sont amenées à evoluer et qu'on les définisse dans la base (ex: configadv), 
             * il suffit juste de les recupérer et de les affecter ICI.             
             *  
             */
            lifeTimeInMonth = eConst.LIFE_TIME_IN_MONTH;
            purgeTimeInMonth = eConst.PURGE_TIME_IN_MONTH;

            Boolean immediateSending = this._param["immediateSending"].EndsWith("1");
            Boolean recurrentSending = this._param["recurrentSending"].EndsWith("1");

            //On fait la vérification complète dans le cas de l'envoi
            Boolean btnSendClicked = _action == eMailing.MailingAction.SEND;

            //La date est envoyé au format ft (dd/mmYYYY)
            // Le try parse doit en tenir compte
            DateTime.TryParse(this._param["trackLnkLifeTime"], CultureInfo.CreateSpecificCulture("fr-Fr"), DateTimeStyles.None, out dateLife);
            DateTime.TryParse(this._param["trackLnkPurgedDate"], CultureInfo.CreateSpecificCulture("fr-Fr"), DateTimeStyles.None, out datePurge);


            if (this._param["subject"].Trim().Length == 0 || this._param["body"].Trim().Length == 0)
                throw new eMailingException(eErrorCode.EMPTY_SUBJECT_OR_BODY);

            //SHA : tâche #2 047
            if (this._param["preheader"].Length > eLibConst.MAX_PREHEADER_LENGTH)
                throw new eMailingException(eErrorCode.ERROR_MAX_LENGTH_PREHEADER);

            if (_mailingType == TypeMailing.MAILING_FOR_MARKETING_AUTOMATION)//on check pas les régles sur les dates pour le workflow
                return;

            if (!immediateSending && !recurrentSending)
            {
                DateTime.TryParse(this._param["sendingDate"], CultureInfo.CreateSpecificCulture("fr-Fr"), DateTimeStyles.None, out sendingDate);

                //On recupere le timezone d origine
                string sendingTimeZone = this._param["sendingTimeZone"];
                if (!String.IsNullOrEmpty(sendingTimeZone))
                {
                    //On enregistre le TimeZoneUser dans prefadv
                    eLibTools.AddOrUpdatePrefAdv(_pref, eLibConst.PREFADV.DEFAULT_TIMEZONE, sendingTimeZone, eLibConst.PREFADV_CATEGORY.MAIN, _pref.UserId);

                    TimeZoneInfo tzInfos = TimeZoneInfo.FindSystemTimeZoneById(sendingTimeZone);

                    //On converti la date dans le timezone du serveur
                    sendingDate = TimeZoneInfo.ConvertTime(sendingDate, tzInfos, TimeZoneInfo.Local);
                    //On modifie le param pour enregistrement en base 
                    this._param["sendingDate"] = sendingDate.ToString();
                }
            }


            //date d'envoi doit etre sup/egal a la date/heure actuelle
            if (sendingDate < dateNow)
                throw new eMailingException(eErrorCode.INVALID_SENDING_DATE);

            //la durée de vie des liens tracking doit etre sup/egal a la date/heure actuelle
            if (dateLife < dateNow)
                throw new eMailingException(eErrorCode.INVALID_LIFE_TRACKING_DATE);

            //la date de purge des liens tracking doit etre sup/egal a la date/heure actuelle
            if (datePurge < dateNow)
                throw new eMailingException(eErrorCode.INVALID_PURGING_DATE);

            //la durrée de vie doit etre infé a la date de purge
            if (dateLife > datePurge)
                throw new eMailingException(eErrorCode.INCONSISTANT_TRACKING_DATES);

            //la date de vie est limitée a X mois
            if (dateLife > sendingDate.AddMonths(lifeTimeInMonth))
                throw new eMailingException(eErrorCode.DATE_LIMITE_BIGGER, delegate (String res) { return res.Replace("<NUMBER>", lifeTimeInMonth.ToString()); });

            //la date de purge est limitée a Y mois
            if (datePurge > sendingDate.AddMonths(purgeTimeInMonth))
                throw new eMailingException(eErrorCode.DATE_PURGE_BIGGER, delegate (String res) { return res.Replace("<NUMBER>", purgeTimeInMonth.ToString()); });

            //la date d envoi doit etre infé à la date de vie
            if (sendingDate > dateLife)
                throw new eMailingException(eErrorCode.INCONSISTANT_SENDING_DATE);

        }

        /// <summary>
        /// On execute l'action demandée
        /// </summary>
        protected virtual void ProcessAction()
        {
            //oMailing = new eMailing(this._pref, this._nMailingId, this._mailingType);
            oMailing = new eMailing(_pref, this._nMailingId, this._nTab, _edal, this._mailingType);

            //oMailing.Tab = this._nTab;
            oMailing.ParentTab = this._nParentTab;
            oMailing.ParentFileId = this._nParentFileId;
            oMailing.PjIds = _pjIds;
            if (this._param.ContainsKey("templateId"))
                oMailing.MailTemplate = eLibTools.GetNum(this._param["templateId"]);
            oMailing.LoadFrom(_param);
            oMailing.Run(_action);

            this._nMailingId = oMailing.Id;
            //gérer les erreurs mailTester
            this._ErrorOnMailTester = oMailing.ErrorOnMailTester;
            this._ErrorMailTester = oMailing.ErrorMailTester;
        int.TryParse(oMailing.MailingParams["eventStepFileId"], out this._nEventStepFileId);
        }

        /// <summary>
        /// Fait un rendu du résultat
        /// </summary>
        protected virtual void RenderXmlResult()
        {
            String message = string.Empty;
            String detail = string.Empty;
            String devDetail = string.Empty;
            Boolean success = true;

            message = eResApp.GetRes(this._pref, 1676);

            if (_innerException != null)
            {


                if (_innerException.UserMessageDetails?.Length > 0)
                    throw _innerException;

                message = _innerException.UserMessageTitle?.Length > 0 ? _innerException.UserMessageTitle : eResApp.GetRes(this._pref, 6524);
                detail = _innerException.GetUserMessage(this._pref);



                success = false;
            }

            #region initialisation du retour XML (structure)

            XmlDocument xmlDocReturn = new XmlDocument();

            XmlNode xmlNodeEdnResult = xmlDocReturn.CreateElement("ednResult");
            XmlNode xmlNodeOperation = xmlDocReturn.CreateElement("operation");
            XmlNode xmlNodeSuccess = xmlDocReturn.CreateElement("success");
            XmlNode xmlNodeFileId = xmlDocReturn.CreateElement("id");
            XmlNode xmlNodeEventStepFileId = xmlDocReturn.CreateElement("eventStepFileId");
            XmlNode xmlNodeMessage = xmlDocReturn.CreateElement("message");
            XmlNode xmlNodeuserAddress = xmlDocReturn.CreateElement("useraddress");
            XmlNode xmlNodeuserMainEmail = xmlDocReturn.CreateElement("usermainemail");
            XmlNode xmlNodeDetail = xmlDocReturn.CreateElement("detail");
            XmlNode xmlNodeUntrackedLinks = xmlDocReturn.CreateElement("untrackedlinks");
            XmlNode xmlNodeMailTesterExtextionActivated = xmlDocReturn.CreateElement("mailTesterExtextionActivated");
            

            #endregion

            #region Valeurs de retours
            xmlNodeSuccess.InnerText = success ? "1" : "0";
            if (eExtension.IsReadyStrict(_pref, "QUALITYMAIL", true))
            {
                xmlNodeMailTesterExtextionActivated.InnerText = "1";
                if (this._param["MailTestType"] == "1")
                {
                    XmlNode xmlNodeMailTesterLink = xmlDocReturn.CreateElement("mailTesterLink");
                    xmlNodeMailTesterLink.InnerText = string.Concat("https://www.mail-tester.com/", this._param["MailTesterReportId"]);
                    xmlNodeEdnResult.AppendChild(xmlNodeMailTesterLink);

                    XmlNode xmlNodeMailReportId = xmlDocReturn.CreateElement("mailTesterReportId");
                    xmlNodeMailReportId.InnerText = this._param["MailTesterReportId"].ToString();
                    xmlNodeEdnResult.AppendChild(xmlNodeMailReportId);

                    //Ajouter les erreurs mailTester
                    XmlNode xmlNodeErrorOnMailTester = xmlDocReturn.CreateElement("errorOnMailTester");
                    xmlNodeErrorOnMailTester.InnerText = this._ErrorOnMailTester ? "1" : "0";
                    xmlNodeEdnResult.AppendChild(xmlNodeErrorOnMailTester);
                    XmlNode xmlNodeErrorMailTester = xmlDocReturn.CreateElement("errorMailTester");
                    xmlNodeErrorMailTester.InnerText = this._ErrorMailTester;
                    xmlNodeEdnResult.AppendChild(xmlNodeErrorMailTester);
                }
            }
            else
                xmlNodeMailTesterExtextionActivated.InnerText = "0";

            if(_mailingType == TypeMailing.MAILING_FOR_MARKETING_AUTOMATION)
            {
                XmlNode xmlNodeDescription = xmlDocReturn.CreateElement("description");
                if (_allKeys.Contains("description") && _context.Request.Form["description"] != null)
                {
                    xmlNodeDescription.InnerText = _context.Request.Form["description"].ToString();
                }
                else
                    xmlNodeDescription.InnerText = "";
                XmlNode xmlNodeTitle = xmlDocReturn.CreateElement("title");
                if (_allKeys.Contains("libelle") && _context.Request.Form["libelle"] != null)
                {
                    xmlNodeTitle.InnerText = _context.Request.Form["libelle"].ToString();
                }
                else
                    xmlNodeTitle.InnerText = "";
                xmlNodeEdnResult.AppendChild(xmlNodeDescription);
                xmlNodeEdnResult.AppendChild(xmlNodeTitle);
            }

            xmlNodeFileId.InnerText = this._nMailingId.ToString();
            xmlNodeEventStepFileId.InnerText = this._nEventStepFileId.ToString();

            xmlNodeMessage.InnerText = message;

            xmlNodeuserAddress.InnerText = this._param["sender"].ToString();

            String mainEmail = this._param["sender"].ToString();
            if (this._param.TryGetValue("ownerMainEmail", out mainEmail))
            {
                xmlNodeuserMainEmail.InnerText = this._param["ownerMainEmail"].ToString();
            }

            xmlNodeDetail.InnerText = detail;
            xmlNodeOperation.InnerText = _action.GetHashCode().ToString();
            if (_action == eMailing.MailingAction.CHECK_LINKS)
            {
                xmlNodeUntrackedLinks.InnerText = countUntrackedLinks.ToString();
            }

            xmlNodeEdnResult.AppendChild(xmlNodeSuccess);
            xmlNodeEdnResult.AppendChild(xmlNodeMailTesterExtextionActivated);

            xmlNodeEdnResult.AppendChild(xmlNodeFileId);
            xmlNodeEdnResult.AppendChild(xmlNodeEventStepFileId);
            xmlNodeEdnResult.AppendChild(xmlNodeDetail);
            xmlNodeEdnResult.AppendChild(xmlNodeMessage);
            xmlNodeEdnResult.AppendChild(xmlNodeuserAddress);
            xmlNodeEdnResult.AppendChild(xmlNodeOperation);
            xmlNodeEdnResult.AppendChild(xmlNodeuserMainEmail);
            xmlNodeEdnResult.AppendChild(xmlNodeUntrackedLinks);

            xmlDocReturn.AppendChild(xmlNodeEdnResult);

            #endregion

            RenderResult(RequestContentType.XML, delegate () { return xmlDocReturn.OuterXml; });


        }

    }

    /// <summary>
    /// Exceptions sur les eMailings
    /// </summary>
    public class eMailingException : EudoInternalException
    {
        //Code d'erreur     
        eErrorCode _errCode;

        /// <summary>Erreur à afficher à l'utilisateur</summary>
        private String _sUserMessage = String.Empty;
        //Pour remplacer des valeurs contenant dans les res ex : <VALUE>, <FIELD>
        public delegate String ResReplace(String res);
        ResReplace resReplace = null;

        /// <summary>
        /// Constructeur pour les exception d'eMailing
        /// </summary>
        /// <param name="errCode">Code d'erreur Eudo</param>
        /// <param name="devMessage">Message d'erreur</param>
        public eMailingException(eErrorCode errCode, String devMessage = "", String userMessage = "")
            : base(userMessage, devMessage)
        {
            this._errCode = errCode;
            _sUserMessage = userMessage;
        }

        /// <summary>
        /// Constructeur pour les exception d'eMailing
        /// </summary>
        /// <param name="errCode">Code d'erreur Eudo</param>
        /// <param name="devMessage">Message d'erreur de debuug</param>
        public eMailingException(eErrorCode errCode, ResReplace fnc, String devMessage = "")
            : base("", devMessage)
        {
            this.resReplace = fnc;
            this._errCode = errCode;
        }


        /// <summary>
        /// Message retourné à l'utilisateur
        /// </summary>
        /// <returns></returns>
        public String GetUserMessage(ePref pref)
        {
            String userMessage = String.Empty;
            switch (_errCode)
            {

                case eErrorCode.INVALID_PARSED_DATE:
                    userMessage = eResApp.GetRes(pref, 6637); //Date(s) invalide(s)
                    break;
                case eErrorCode.INVALID_PURGING_DATE:
                    userMessage = eResApp.GetRes(pref, 6638); //La date de purge des liens tracking est invalide
                    break;
                case eErrorCode.INVALID_LIFE_TRACKING_DATE:
                    userMessage = eResApp.GetRes(pref, 6639); //La date limite de durée de vie des liens tracking est invalide
                    break;
                case eErrorCode.INVALID_SENDING_DATE:
                    userMessage = eResApp.GetRes(pref, 6640); //La date d'envoi n'est pas valide
                    break;

                case eErrorCode.INVALID_EMAIL_FORMAT:
                    //SHA
                    //userMessage = eResApp.GetRes(pref, 2021);  //,"La valeur saisie ""<VALUE>"" ne correspond pas au format et/ou à la longueur de la rubrique <FIELD>"
                    userMessage = eResApp.GetRes(pref, 8533);  // "Aucun e-mail n'est défini pour l'utilisateur" (ou 7897 ou 1023 ou 578)
                    break;

                case eErrorCode.INCONSISTANT_TRACKING_DATES:
                    userMessage = eResApp.GetRes(pref, 6641); //La date de durée de vie des liens tracking incohérente avec la date de purge
                    break;

                case eErrorCode.INCONSISTANT_SENDING_DATE:
                    userMessage = eResApp.GetRes(pref, 6648); //Date(s) invalide(s)
                    break;

                case eErrorCode.ERROR_CAMPAIGN_INSERT:
                    if (_sUserMessage.Length > 0)
                        userMessage = _sUserMessage;
                    else
                        userMessage = String.Concat(eResApp.GetRes(pref, 6642), "<br/>",
                            eResApp.GetRes(pref, 6342)
                            ); //Impossible de créer une nouvelle campagne <br /> Veuillez contacter l'administrateur";
                    break;
                case eErrorCode.ERROR_CAMPAIGN_SELECTION_INSERT:
                    userMessage = userMessage = String.Concat(eResApp.GetRes(pref, 6642), "<br/>",
                                                             eResApp.GetRes(pref, 6342));
                    break;
                case eErrorCode.INVALID_SUBJECT_LINKS:
                    userMessage = eResApp.GetRes(pref, 6643); //L'objet du mail contient des liens ou des champs de fusion invalides
                    break;
                case eErrorCode.INVALID_BODY_LINKS:
                    userMessage = eResApp.GetRes(pref, 6644); //Le corps du mail contient des liens ou des champs de fusion invalides
                    break;

                case eErrorCode.ERROR_CAMPAIGN_DELETE:
                    userMessage = eResApp.GetRes(pref, 6645); //Impossible de supprimer la campagne
                    break;

                case eErrorCode.OUT_OF_CREDIT:
                    userMessage = eResApp.GetRes(pref, 6646); //Vous avez pas assez de credit pour envoyer la compagne
                    break;
                case eErrorCode.ERROR_WCF:
                    userMessage = eResApp.GetRes(pref, 6647); //
                    break;

                case eErrorCode.DATE_LIMITE_BIGGER:
                    userMessage = eResApp.GetRes(pref, 6662);//La durée de vie des liens tracking est limitée à <NUMBER> mois après la date d'envoi !
                    break;

                case eErrorCode.DATE_PURGE_BIGGER:
                    userMessage = eResApp.GetRes(pref, 6661);// "La durée de purge des liens tracking est limitée à <NUMBER> mois après la date d'envoi !
                    break;

                case eErrorCode.EMPTY_SUBJECT_OR_BODY:
                    userMessage = eResApp.GetRes(pref, 6860);
                    break;
                case eErrorCode.ERROR_CAMPAIGN_SENDMAILTEST_NOCONTACT:
                    userMessage = eResApp.GetRes(pref, 6862);
                    break;

                case eErrorCode.UNTRACKED_LINKS:
                    userMessage = eResApp.GetRes(pref, 6799);
                    break;
                case eErrorCode.ERROR_SENDING_WCF:
                    userMessage = eResApp.GetRes(pref, 2684);
                    break;
                case eErrorCode.SMS_DISABLED:
                    userMessage = eResApp.GetRes(pref, 6859);
                    break;
                case eErrorCode.SMS_INVALID_CONFIG:
                    userMessage = eResApp.GetRes(pref, 6861);
                    break;
                case eErrorCode.SMS_EMPTY_BODY:
                    userMessage = eResApp.GetRes(pref, 1161) + "<br />" + eResApp.GetRes(pref, 6342);
                    break;
                case eErrorCode.SMS_EMPTY_MAIL_FIELD:
                    userMessage = eResApp.GetRes(pref, 1161) + "<br />" + eResApp.GetRes(pref, 6342);
                    break;
                case eErrorCode.SMS_EMPTY_MAIL_TAB:
                    userMessage = eResApp.GetRes(pref, 1892) + "<br />" + eResApp.GetRes(pref, 6342); // Aucun fichier SMS paramétré
                    break;
                case eErrorCode.PJ_NOT_SAVED:
                case eErrorCode.PJ_NOT_FOUND:
                    userMessage = _sUserMessage;
                    break;
                default:

                    if (_sUserMessage?.Length > 0)
                        userMessage = _sUserMessage;
                    else
                        userMessage = eResApp.GetRes(pref, 72) + "<br />" + eResApp.GetRes(pref, 6236);
                    break;
            }

            if (resReplace != null)
                return resReplace(userMessage);

            return userMessage;
        }
    }


    /// <summary>
    /// Les codes d'erreur
    /// </summary>
    public enum eErrorCode
    {
        /// <summary>
        /// Date parsé invalide
        /// </summary>
        INVALID_PARSED_DATE = 0,

        /// <summary>
        /// date de tracking invalide
        /// </summary>
        INVALID_LIFE_TRACKING_DATE = 1,

        /// <summary>
        /// date de purge invalide
        /// </summary>
        INVALID_PURGING_DATE = 2,

        /// <summary>
        /// date d'envoi invalide
        /// </summary>
        INVALID_SENDING_DATE = 3,

        /// <summary>
        /// date de tracking incohérente (par exemple, date de fin de track  > date de purge)
        /// </summary>
        INCONSISTANT_TRACKING_DATES = 4,

        /// <summary>
        /// lien dans le corps invalide
        /// </summary>
        INVALID_BODY_LINKS = 5,

        /// <summary>
        /// lien dans le sujet invalide
        /// </summary>
        INVALID_SUBJECT_LINKS = 6,

        /// <summary>
        /// format de mail invalide
        /// </summary>
        INVALID_EMAIL_FORMAT = 7,

        /// <summary>
        /// impossible de créer la campagne
        /// </summary>
        ERROR_CAMPAIGN_INSERT = 8,

        /// <summary>
        /// impossible de maj la campagne
        /// </summary>
        ERROR_CAMPAIGN_UPDATE_BODY = 9,

        /// <summary>
        /// impossible de créer la selection des mail/inviter
        /// </summary>
        ERROR_CAMPAIGN_SELECTION_INSERT = 10,

        /// <summary>
        /// erreur inconnue
        /// </summary>
        ERROR_UNKNOWN = 11,

        /// <summary>
        /// impossible de supprimer la campagne
        /// </summary>
        ERROR_CAMPAIGN_DELETE = 12,

        /// <summary>
        /// plus de credit d'envoie
        /// </summary>
        OUT_OF_CREDIT = 13,

        /// <summary>
        /// erreur de wcf
        /// </summary>
        ERROR_WCF = 14,

        /// <summary>
        /// erreur d'envoi au wcf
        /// </summary>
        ERROR_SENDING_WCF = 15,

        /// <summary>
        /// date d'envoie incoherente avec ls autres dates
        /// </summary>
        INCONSISTANT_SENDING_DATE = 16,

        /// <summary>
        /// date de purge trop lointaine
        /// </summary>
        DATE_PURGE_BIGGER = 17,

        /// <summary>
        /// date de fin de purge trop lointaine
        /// </summary>
        DATE_LIMITE_BIGGER = 18,

        /// <summary>
        /// sujet ou corps vide
        /// </summary>
        EMPTY_SUBJECT_OR_BODY = 19,

        /// <summary>
        /// erreur d'envoi du mail de test
        /// </summary>
        ERROR_CAMPAIGN_SENDMAILTEST_FATALERROR = 20,


        /// <summary>
        /// erreur d'envoi du mail de test : pas de contact
        /// </summary>
        ERROR_CAMPAIGN_SENDMAILTEST_NOCONTACT = 21,


        /// <summary>
        /// impossible de recuperer un champ sur un lien
        /// </summary>
        ERROR_CAMPAIGN_GET_LINK_FIELD = 22,

        /// <summary>
        /// presence de lien non tracké
        /// </summary>
        UNTRACKED_LINKS = 23,

        /// <summary>
        /// les sms sont désactivé
        /// </summary>
        SMS_DISABLED = 24,

        /// <summary>
        /// config sms invalide
        /// </summary>
        SMS_INVALID_CONFIG = 25,

        /// <summary>
        /// table sms non specifié
        /// </summary>
        SMS_EMPTY_MAIL_TAB = 26,

        /// <summary>
        /// champ destinataire vide
        /// </summary>
        SMS_EMPTY_MAIL_FIELD = 27,

        /// <summary>
        /// corps de sms vide
        /// </summary>
        SMS_EMPTY_BODY = 28,

        /// <summary>
        /// pj non trouvé
        /// </summary>
        PJ_NOT_FOUND = 29,

        /// <summary>
        /// erreur de sauvegarde des pj
        /// </summary>
        PJ_NOT_SAVED = 30,

        /// <summary>
        /// impossible de charger l'etape marketting
        /// </summary>
        ERROR_CAMPAIGN_LOAD_EVENTSTEP = 31,

        /// <summary>
        /// maj impossible de etape marketting
        /// </summary>
        ERROR_CAMPAIGN_UPDATE_EVENTSTEP = 32,

        //SHA : tâche #2 047
        /// <summary>
        /// preheader trop long
        /// </summary>
        ERROR_MAX_LENGTH_PREHEADER = 33,

        //SHA : tâche #1 941
        /// <summary>
        /// lien de pre header invalide
        /// </summary>
        INVALID_PREHEADER_LINKS = 34,


        /// <summary>
        /// Type d'envoi (récurrent, immédiat..) invalide
        /// </summary>
        INVALID_SENDING_TYPE = 35,

        /// <summary>
        /// Type d'envoi  RECURRENT_FILTER mais pas de filtre
        /// </summary>
        RECURRING_CAMPAIGN_MISSING_FILTER = 36,

        /// <summary>
        /// Type d'envoi  Recurrent mais pas de schedule défini
        /// </summary>
        RECURRING_CAMPAIGN_MISSING_SCHEDULE = 37,

        /// <summary>
        /// Type d'envoi  Recurrent mais option marketting non actié
        /// </summary>
        RECURRING_CAMPAIGN_NO_MARKETTING_AUTOMATION = 38,

        /// <summary>
        /// Mode de requete invalide
        /// </summary>
        INVALID_REQUEST_MODE = 39,


        /// <summary>
        /// creq/modifi non autorise
        /// </summary>
        INVALID_RIGHT = 40
    }


}