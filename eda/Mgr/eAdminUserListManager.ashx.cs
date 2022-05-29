using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.eda.Mgr
{
    /// <summary>
    /// Description résumée de eAdminUserListManager
    /// </summary>
    public class eAdminUserListManager : eAdminManager
    {

        /// <summary>
        /// Gestion de la demande de rendu du menu d'admin
        /// </summary>
        /// <param name="context"></param>
        protected override void ProcessManager()
        {

            int nWidth;
            int nHeight;
            int nPage;
            bool bFullRenderer = false;

            int nRows;

            //Initialisation
            nHeight = _requestTools.GetRequestFormKeyI("h") ?? 600;
            nWidth = _requestTools.GetRequestFormKeyI("w") ?? 800;
            nPage = _requestTools.GetRequestFormKeyI("p") ?? 1;
            bFullRenderer = _requestTools.GetRequestFormKeyS("f") == "1";
            nRows = _requestTools.GetRequestFormKeyI("r") ?? 0;

            JSONReturnUserList res = new Mgr.JSONReturnUserList();
            

            eAdminUsersListRenderer rdr = eAdminUsersListRenderer.CreateAdminUsersListRenderer(_pref, bFullRenderer, nPage, nRows, nHeight, nWidth);
            rdr.Generate();
            if (rdr.ErrorMsg.Length > 0)
            {
                ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6237), rdr.ErrorMsg);
                LaunchError();
            }
            else
            {

                res.Success = true;
                
                res.HtmlContentUser = GetResultHTML(rdr.GetPanelUserList, true);
               // res.CallBack = "alert(oRes);";
                RenderResult(RequestContentType.SCRIPT, delegate () { return SerializerTools.JsonSerialize(res); });
            }

        }
    }


    public class JSONReturnUserList
    {

        public bool Success = false;

        public string ErrorMsg = String.Empty;

        public string HtmlMainContener = String.Empty;

        public string HtmlContentGroup = String.Empty;

        public string HtmlContentUser = String.Empty;

        public string CallBack = String.Empty;
         
    }
}