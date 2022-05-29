using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Description résumée de eAdminFieldAliasRelationMgr
    /// </summary>
    public class eAdminFieldAliasRelationMgr : eAdminManager
    {
        protected override void ProcessManager()
        {
            int iTab = _requestTools.GetRequestFormKeyI("tab") ?? 0;
            int iDescId = _requestTools.GetRequestFormKeyI("did") ?? 0;
            eAdminFieldInfos field = null;

            if (iDescId > 0)
                field =   eAdminFieldInfos.GetAdminFieldInfos(_pref, iDescId);

            eAdminFieldAliasRelationRenderer.GetLinkedFieldsContext context = _requestTools.GetRequestFormEnum<eAdminFieldAliasRelationRenderer.GetLinkedFieldsContext>("ctxt");
            Dictionary<int, string> dicFields = new Dictionary<int, string>();
            eudoDAL dal = eLibTools.GetEudoDAL(_pref);
            dal.OpenDatabase();
            try
            {
                String sError = "";
                dicFields = eAdminFieldAliasRelationRenderer.GetLinkedFields(_pref, dal, iTab, out sError, context, field);
                if (sError.Length > 0)
                    throw new Exception("eAdminFieldAliasRelationRenderer.GetLinkedFields : " + sError);
            }
            catch (Exception e)
            {
                eFeedbackXrm.LaunchFeedbackXrm(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), eResApp.GetRes(_pref, 7990), " ", String.Concat(e.Message, Environment.NewLine, e.StackTrace)), _pref);
            }
            finally
            {
                dal?.CloseDatabase();
            }

            List<KeyValuePair<int, string>> li = new List<KeyValuePair<int, string>>();
            foreach (KeyValuePair<int, string> kvp in dicFields)
            {
                li.Add(kvp);
            }

            RenderResult(RequestContentType.TEXT, delegate ()
            {
                return JsonConvert.SerializeObject(li);
            });


        }


    }
}