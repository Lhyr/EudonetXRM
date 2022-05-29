using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda.Mgr
{
    /// <summary>
    /// Description résumée de eAdminExtensionExternalEmailingManager
    /// </summary>
    public class eAdminExtensionExternalEmailingManager : eAdminManager
    {
        protected override void ProcessManager()
        {
            JSONReturnExtensionExternalEmailing res = new JSONReturnExtensionExternalEmailing() { Success = false };
            try
            {
                string sCategory = _requestTools.GetRequestFormKeyS("category") ?? String.Empty;
                string sAction = _requestTools.GetRequestFormKeyS("action") ?? String.Empty;

                switch (sCategory)
                {
                    case "senderAliasDomain":
                        switch (sAction)
                        {
                            case "add":
                                string newKey = this.AddSenderAliasDomain();
                                res.SenderAliasDomainPanel = this.BuildSenderAliasDomain(newKey);
                                break;
                            case "update":
                                string sParameterUpdate = _requestTools.GetRequestFormKeyS("parameter") ?? String.Empty;
                                string sValueUpdate = _requestTools.GetRequestFormKeyS("value") ?? String.Empty;
                                this.UpdateSenderAliasDomain(sParameterUpdate, sValueUpdate);
                                res.Value = sValueUpdate;
                                break;
                            case "delete":
                                string sParameterDelete = _requestTools.GetRequestFormKeyS("parameter") ?? String.Empty;
                                this.DeleteSenderAliasDomain(sParameterDelete);
                                break;
                            default:
                                throw new Exception("action not implemented");
                                break;
                        }
                        break;
                    default:
                        throw new Exception("category not implemented");
                        break;
                }

                res.Success = true;
            }
            catch(Exception ex)
            {
                res.Success = false;
                res.ErrorMsg = String.Concat("eAdminExtensionExternalEmailingManager error : ", ex.Message);
            }

            RenderResult(RequestContentType.SCRIPT, delegate () { return SerializerTools.JsonSerialize(res); });
        }

        private string AddSenderAliasDomain()
        {
            Dictionary<string, string> dicoServerAlias = eLibTools.GetConfigAdvPrefAdvValuesFromPrefix(_pref, eLibConst.CONFIGADV.EUDOMAILING_SENDER_ALIAS_DOMAIN.ToString());

            string newKey = String.Empty;
            for (int i = 1; i <= int.MaxValue; ++i)
            {
                newKey = String.Concat(eLibConst.CONFIGADV.EUDOMAILING_SENDER_ALIAS_DOMAIN.ToString(), "_", i.ToString());
                if (dicoServerAlias.ContainsKey(newKey))
                    continue;

                eLibTools.AddOrUpdateConfigAdv(_pref, newKey, String.Empty, eLibConst.CONFIGADV_CATEGORY.MAIN);
                break;
            }

            return newKey;
        }

        private void UpdateSenderAliasDomain(string parameter, string value)
        {
            eLibTools.AddOrUpdateConfigAdv(_pref, parameter, value, eLibConst.CONFIGADV_CATEGORY.MAIN);
        }

        private void DeleteSenderAliasDomain(string parameter)
        {
            eLibTools.DeleteConfigAdvValue(_pref, parameter);
        }

        private string BuildSenderAliasDomain(string parameter)
        {
            Panel SenderAliasDomainSubPanel = eAdminExtensionExternalMailingRenderer.BuildSenderAliasDomain(_pref, parameter);

            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            HtmlTextWriter tw = new HtmlTextWriter(sw);
            SenderAliasDomainSubPanel.RenderControl(tw);
            return sb.ToString();
        }

        private class JSONReturnExtensionExternalEmailing : JSONReturnGeneric
        {
            public string Value = String.Empty;
            public string SenderAliasDomainPanel = String.Empty;
        }
    }
}