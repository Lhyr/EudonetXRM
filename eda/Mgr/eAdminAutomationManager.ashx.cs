using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Web;
using System.Xml;
using Com.Eudonet.Core.Model.eda;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Manager qui gère le paramètrage des automatismes
    /// </summary>
    public class eAdminAutomationManager : eAdminManager
    {
        /// <summary>
        /// Processes the manager.
        /// </summary>
        protected override void ProcessManager()
        {
            try
            {
                Int32? _fileId, _tab;
                String _action;
                Dictionary<int, String> values = new Dictionary<int, string>();
                Dictionary<int, String> texts = new Dictionary<int, string>();

                _fileId = _requestTools.GetRequestFormKeyI("fileid") ?? 0;
                _tab = _requestTools.GetRequestFormKeyI("tab") ?? 0;
                _action = _requestTools.GetRequestFormKeyS("action");

                eAdminAutomationData automationData = new eAdminAutomationData(_pref, _tab.Value, _fileId.Value);

                switch (_action)
                {
                    case "delete":
                        automationData.Delete();
                        break;
                    case "save":
                    default:

                        // On récupère les valeur de la fiche trigger
                        int descid;
                        foreach (NotificationTriggerField fld in Enum.GetValues(typeof(NotificationTriggerField)))
                        {
                            descid = (int)fld;
                            if (_requestTools.AllKeys.Contains("fld_" + descid))
                                values.Add(descid, GetKeyValFromRequest("fld_" + descid));
                        }

                        // Ajout toutes les valeurs
                        foreach (NotificationTriggerResField fld in Enum.GetValues(typeof(NotificationTriggerResField)))
                        {
                            descid = (int)fld;
                            if (_requestTools.AllKeys.Contains("fld_" + descid))
                                texts.Add(descid, HttpUtility.UrlDecode(GetKeyValFromRequest("fld_" + descid)));
                            else
                                texts.Add(descid, "");
                        }

                        // Mise à jour de la base                  
                        automationData.Resources = texts;
                        automationData.Values = values;

                        if (automationData.Validate())
                            automationData.Update();

                        break;
                }

                Render(automationData.Success, automationData.UserDisplayError);

            }
            catch (eEndResponseException) { }
            catch (Exception ex)
            {
#if DEBUG
                ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, ex.Message, ex.StackTrace, eResApp.GetRes(_pref, 6237));
#else
                ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, "", eResApp.GetRes(_pref, 6237));
#endif
                LaunchError();
            }
        }

        private string GetKeyValFromRequest(string key)
        {
            string val = _requestTools.GetRequestFormKeyS(key);
            if (String.IsNullOrEmpty(val))
                val = "";

            return val;
        }

        /// <summary>
        /// Renders the result in XML
        /// </summary>
        /// <param name="bSucces">Success</param>
        /// <param name="sError">Error</param>
        protected void Render(bool bSucces, String sError = "")
        {

            //RenduXML
            XmlDocument xmlResult = new XmlDocument();
            xmlResult.AppendChild(xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null));
            XmlNode baseResultNode = xmlResult.CreateElement("result");
            xmlResult.AppendChild(baseResultNode);

            XmlNode successResultNode = xmlResult.CreateElement("success");
            successResultNode.InnerText = bSucces ? "1" : "0";
            baseResultNode.AppendChild(successResultNode);

            XmlNode errorResultNode = xmlResult.CreateElement("error");
            errorResultNode.InnerText = sError;
            baseResultNode.AppendChild(errorResultNode);

            RenderResult(RequestContentType.XML, delegate () { return xmlResult.OuterXml; });
        }
    }
}