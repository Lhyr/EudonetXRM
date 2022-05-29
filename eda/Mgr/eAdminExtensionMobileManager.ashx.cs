using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda.Mgr
{
    /// <summary>
    /// Manager de eAdminTabs
    /// </summary>
    public class eAdminExtensionMobileManager : eAdminManager
    {

        /// <summary>
        /// Gestion de la demande de rendu du menu d'admin
        /// </summary>

        protected override void ProcessManager()
        {
            string strError = String.Empty;

            //Initialisation
            string strAction = _requestTools.GetRequestFormKeyS("action") ?? "changeField";
            string strSection = _requestTools.GetRequestFormKeyS("section") ?? String.Empty;
            eLibConst.MOBILE key = eLibConst.MOBILE.UNDEFINED;
            if (_context.Request.Form["key"] != null)
            {
                int nKey = 0;
                Int32.TryParse(_context.Request.Form["key"].ToString(), out nKey);
                key = (eLibConst.MOBILE)nKey;
            }
            int nUserId = _requestTools.GetRequestFormKeyI("userid") ?? 0;
            string strTab = _requestTools.GetRequestFormKeyS("tab") ?? String.Empty;
            List<int> tabList = new List<int>();
            foreach (string tab in strTab.Split(';'))
                tabList.Add(eLibTools.GetNum(tab));
            int nField = _requestTools.GetRequestFormKeyI("field") ?? 0;
            int nDescId = _requestTools.GetRequestFormKeyI("descid") ?? 0;
            bool bReadOnly = _requestTools.GetRequestFormKeyB("readonly") ?? false;
            bool bCustomized = _requestTools.GetRequestFormKeyB("customized") ?? false;

            eMobileMapping mapping = new eMobileMapping(
                                key,
                                nUserId,
                                tabList,
                                nField,
                                nDescId,
                                bReadOnly,
                                bCustomized
                            );


            eudoDAL dal = eLibTools.GetEudoDAL(_pref);
            if (!dal.IsOpen)
                dal.OpenDatabase();

            JSONReturnExtensionMobile res = new Mgr.JSONReturnExtensionMobile();
            int result = 0;

            switch (strAction)
            {
                case "changeField":
                    result = mapping.SaveMobileMapping(dal);
                    if (result < 1)
                    {
                        ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6237), eResApp.GetRes(_pref, 7854)); // "La mise à jour du mapping a échoué"
                        LaunchError();
                    }
                    else
                    {
                        res.Action = "changeField";
                        res.Section = strSection;
                        res.Key = key.ToString();
                        res.Success = true;
                        res.Result = "1";
                        RenderResult(RequestContentType.SCRIPT, delegate () { return SerializerTools.JsonSerialize(res); });
                    }
                    break;
                case "changeTab":
                    result = 0;
                    int nTab = _requestTools.GetRequestFormKeyI("tab") ?? 0;
                    strSection = _requestTools.GetRequestFormKeyS("type") ?? "";

                    res.Success = mapping.ReinitMobileMappingTab(dal, strSection, nTab);

                    res.Action = "changeTab";
                    res.Section = strSection;
                    res.Key = nTab.ToString();

                    res.Result = result.ToString();

                    RenderResult(RequestContentType.SCRIPT, delegate () { return SerializerTools.JsonSerialize(res); });
                    break;
            }

            dal.CloseDatabase();
        }
    }

    public class JSONReturnExtensionMobile : JSONReturnGeneric
    {



        public string Action = String.Empty;

        public string Section = String.Empty;

        public string Key = eLibConst.MOBILE.UNDEFINED.ToString();

        public string Result = String.Empty;


    }
}