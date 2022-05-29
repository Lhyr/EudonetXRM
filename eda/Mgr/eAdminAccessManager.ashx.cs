using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.eda.Mgr
{
    /// <summary>
    /// Manager de eAdminTabs
    /// </summary>
    public class eAdminAccessManager : eAdminManager
    {

        /// <summary>
        /// Gestion de la demande de rendu du menu d'admin
        /// </summary>
        /// <param name="context"></param>
        protected override void ProcessManager()
        {
            int nWidth = 800;
            int nHeight = 600;
            int nPage = 0;
            int nRows = 0;
            bool bFullRenderer = false;

            //Initialisation
            nHeight = _requestTools.GetRequestFormKeyI("h") ?? 600;
            nWidth = _requestTools.GetRequestFormKeyI("w") ?? 800;
            nPage = _requestTools.GetRequestFormKeyI("p") ?? 1;
            bFullRenderer = _requestTools.GetRequestFormKeyS("f") == "1";
            nRows = _requestTools.GetRequestFormKeyI("r") ?? 0;

            eUserOptionsModules.USROPT_MODULE targetModule = eUserOptionsModules.USROPT_MODULE.UNDEFINED;
            if (_context.Request.Form["module"] != null)
            {
                int nTargetModule = 0;
                Int32.TryParse(_context.Request.Form["module"].ToString(), out nTargetModule);
                targetModule = (eUserOptionsModules.USROPT_MODULE)nTargetModule;
            }

            JSONReturnAccess res = new Mgr.JSONReturnAccess();



            eAdminRenderer rdr = eAdminRendererFactory.CreateAdminAccessRenderer(_pref, targetModule, bFullRenderer, nPage, nRows, nWidth, nHeight);
            if (rdr.ErrorMsg.Length > 0)
            {
                ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6237), rdr.ErrorMsg);
                LaunchError();
            }
            else
            {

                res.Success = true;
                res.Html = GetResultHTML(rdr.GetContents());
                res.CallBack = rdr.GetCallBackScript;


                RenderResult(RequestContentType.SCRIPT, delegate () { return SerializerTools.JsonSerialize(res); });
            }

        }
    }


    public class JSONReturnAccess : JSONReturnHTMLContent
    {
       
 
    }
}