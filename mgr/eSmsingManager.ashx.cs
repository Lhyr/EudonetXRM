using System;
using EudoQuery;
using System.Xml;
using Com.Eudonet.Internal;
using System.Web;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.mgr
{
    /// <className>eSmsingManager</className>
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
    public class eSmsingManager : eMailingManager
    {


        /// <summary>
        /// Vérifie les parametres transmis depuis l assistant
        /// et les regles metier sur differente valeur (date d envoi > aujourd hui .....) 
        /// </summary>
        protected override void CheckValidParams()
        {
            if (this._param["mailTabDescId"].Trim().Length == 0)
                throw new eMailingException(eErrorCode.SMS_EMPTY_MAIL_TAB);

            //TODO
            //if (this._param["body"].Trim().Length == 0)
            //   throw new eMailingException(eErrorCode.SMS_EMPTY_BODY);

            if (this._param["mailFieldDescId"].Trim().Length == 0)
                throw new eMailingException(eErrorCode.SMS_EMPTY_MAIL_FIELD);

            Boolean immediateSending = this._param["immediateSending"].EndsWith("1");
            Boolean recurrentSending = this._param["recurrentSending"].EndsWith("1");

            //Incohérence sur les type d'envoi
            if (immediateSending && recurrentSending)
                throw new eMailingException(eErrorCode.INVALID_SENDING_TYPE);

            if (recurrentSending)
            {
                // vérification scheduleid, requestmode, filter, eventstep
                int nParam;

                //Descid de la table Etape Mareketting
                if (!this._param.ContainsKey("eventStepDescId") || !int.TryParse(this._param["eventStepDescId"], out nParam))
                    throw new eMailingException(eErrorCode.RECURRING_CAMPAIGN_NO_MARKETTING_AUTOMATION);

                //id du schedule                           
                if (!this._param.ContainsKey("scheduleId") || !int.TryParse(this._param["scheduleId"], out nParam))
                    throw new eMailingException(eErrorCode.RECURRING_CAMPAIGN_MISSING_SCHEDULE);

                //request mode
                if (!this._param.ContainsKey("RequestMode") || !int.TryParse(this._param["RequestMode"], out nParam))
                    throw new eMailingException(eErrorCode.INVALID_REQUEST_MODE);

                if (nParam == (int)MAILINGQUERYMODE.RECURRENT_FILTER)
                {
                    //request mode
                    if (!this._param.ContainsKey("recipientsFilterId") || !int.TryParse(this._param["recipientsFilterId"], out nParam))
                        throw new eMailingException(eErrorCode.RECURRING_CAMPAIGN_MISSING_FILTER);
                }
                else if (nParam != (int)MAILINGQUERYMODE.RECURRENT_ALL)
                    throw new eMailingException(eErrorCode.INVALID_SENDING_TYPE);

            }

        }

        /// <summary>
        /// Recupéré les champs du mail
        /// Note : Les champs sont serialisés par engine.js
        /// </summary>
        protected override void LoadMailFields()
        {

            //Les autres champs du mail
            eUpdateField updField;

            this._param["HtmlFormat"] = "0";

            foreach (String s in _allKeys)
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
                        this._param["body"] = HttpUtility.HtmlDecode(updField.NewValue);
                        break;
                    case CampaignField.RECIPIENTTABID:
                        this._param["mailTabDescId"] = updField.NewValue;
                        break;
                    case CampaignField.MAILADDRESSDESCID:
                        this._param["mailFieldDescId"] = HttpUtility.HtmlDecode(updField.NewValue);
                        break;
                    case CampaignField.ISHTML:
                        break;

                }
            }

            this._param["displayName"] = eLibTools.GetEmailSenderDisplayName(_pref, _pref.User);
            this._param["sender"] = _pref.User.UserMail;
        }

        /// <summary>
        /// On execute l'action demandée
        /// </summary>
        protected override void ProcessAction()
        {
            oMailing = new eSmsing(this._pref, this._nMailingId, this._mailingType);

            oMailing.Tab = this._nTab;
            oMailing.ParentTab = this._nParentTab;
            oMailing.ParentFileId = this._nParentFileId;
            oMailing.PjIds = _pjIds;
            oMailing.LoadFrom(_param);
            oMailing.Run(_action);

            this._nMailingId = oMailing.Id;
        }

        /// <summary>
        /// Fait un rendu du résultat
        /// </summary>
        protected override void RenderXmlResult()
        {
            String message = string.Empty;
            String detail = string.Empty;
            String devDetail = string.Empty;
            Boolean success = true;

            message = eResApp.GetRes(this._pref, 1676);

            if (_innerException != null)
            {
                message = eResApp.GetRes(this._pref, 2685);
                detail = _innerException.GetUserMessage(this._pref);
                devDetail = _innerException.Message;
                success = false;
            }

            #region initialisation du retour XML (structure)

            XmlDocument xmlDocReturn = new XmlDocument();

            XmlNode xmlNodeEdnResult = xmlDocReturn.CreateElement("ednResult");
            XmlNode xmlNodeOperation = xmlDocReturn.CreateElement("operation");
            XmlNode xmlNodeSuccess = xmlDocReturn.CreateElement("success");
            XmlNode xmlNodeFileId = xmlDocReturn.CreateElement("id");
            XmlNode xmlNodeMessage = xmlDocReturn.CreateElement("message");
            XmlNode xmlNodeuserAddress = xmlDocReturn.CreateElement("useraddress");
            XmlNode xmlNodeuserMainEmail = xmlDocReturn.CreateElement("usermainemail");
            XmlNode xmlNodeDetail = xmlDocReturn.CreateElement("detail");
            XmlNode xmlNodeDevDetail = xmlDocReturn.CreateElement("devDetail");


            #endregion

            #region Valeurs de retours
            xmlNodeSuccess.InnerText = success ? "1" : "0";
            xmlNodeFileId.InnerText = this._nMailingId.ToString();

            xmlNodeMessage.InnerText = message;

            xmlNodeuserAddress.InnerText = this._param["sender"].ToString();

            String mainEmail = this._param["sender"].ToString();
            if (this._param.TryGetValue("ownerMainEmail", out mainEmail))
            {
                xmlNodeuserMainEmail.InnerText = this._param["ownerMainEmail"].ToString();
            }
            else
            {
                xmlNodeuserMainEmail.InnerText = _pref.User.UserMail;
            }

            xmlNodeDetail.InnerText = detail;
            xmlNodeDevDetail.InnerText = devDetail;
            xmlNodeOperation.InnerText = _action.GetHashCode().ToString();


            xmlNodeEdnResult.AppendChild(xmlNodeSuccess);
            xmlNodeEdnResult.AppendChild(xmlNodeFileId);
            xmlNodeEdnResult.AppendChild(xmlNodeDetail);
            xmlNodeEdnResult.AppendChild(xmlNodeDevDetail);
            xmlNodeEdnResult.AppendChild(xmlNodeMessage);
            xmlNodeEdnResult.AppendChild(xmlNodeuserAddress);
            xmlNodeEdnResult.AppendChild(xmlNodeOperation);
            xmlNodeEdnResult.AppendChild(xmlNodeuserMainEmail);

            xmlDocReturn.AppendChild(xmlNodeEdnResult);

            #endregion

            RenderResult(RequestContentType.XML, delegate () { return xmlDocReturn.OuterXml; });

        }
    }
}