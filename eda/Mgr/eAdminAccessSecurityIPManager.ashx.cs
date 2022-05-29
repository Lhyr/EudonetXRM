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
    public class eAdminAccessSecurityIPManager : eAdminManager
    {

        /// <summary>
        /// Gestion de la demande de rendu du menu d'admin
        /// </summary>
        /// <param name="context"></param>
        protected override void ProcessManager()
        {
            string strError = String.Empty;

            //Initialisation
            int nWidth = _requestTools.GetRequestFormKeyI("width") ?? 800;
            int nHeight = _requestTools.GetRequestFormKeyI("height") ?? 600;

            string strAction = _requestTools.GetRequestFormKeyS("action") ?? "update";
            int nIPId = _requestTools.GetRequestFormKeyI("ipid") ?? -1; // -1 : création
            string strIPLabel = _requestTools.GetRequestFormKeyS("iplabel") ?? String.Empty;
            string strIPAddress = _requestTools.GetRequestFormKeyS("ipaddress") ?? String.Empty;
            string strIPMask = _requestTools.GetRequestFormKeyS("ipmask") ?? String.Empty;
            int nPermId = _requestTools.GetRequestFormKeyI("permid") ?? 0;
            int lvalue = _requestTools.GetRequestFormKeyI("lvalue") ?? -1;
            string uvalue = _requestTools.GetRequestFormKeyS("uvalue") ?? String.Empty;
            bool bUpdateLevel = _requestTools.GetRequestFormKeyB("updatelevel") ?? false;
            bool bUpdateUsers = _requestTools.GetRequestFormKeyB("updateusers") ?? false;
            bool bSetIPAccessValue = _requestTools.GetRequestFormKeyB("setipaccessvalue") ?? false;

            // TODO / TOCHECK : à récupérer de quelque part ? A mutualiser ? Cas de USER_OR_LEVEL ?
            PermissionMode permMode = PermissionMode.MODE_NONE;
            if (bUpdateLevel && bUpdateUsers && lvalue > 0 && uvalue.Length > 0)
                permMode = PermissionMode.MODE_USER_AND_LEVEL;
            else if (bUpdateLevel && lvalue > 0)
                permMode = PermissionMode.MODE_LEVEL_ONLY;
            else if (bUpdateUsers && uvalue.Length > 0)
                permMode = PermissionMode.MODE_USER_ONLY;

                JSONReturnAccessSecurityIP res = new Mgr.JSONReturnAccessSecurityIP();

            eAdminAccessSecurityIPData ipData = new eAdminAccessSecurityIPData(nIPId, strIPLabel, strIPAddress, strIPMask, uvalue, lvalue, nPermId, permMode);

            switch (strAction)
            {
                case "setIPAccessEnabled":
                    bool bSetIPAccessSuccess = eAdminAccessSecurityIP.SetIPAccessEnabled(_pref, bSetIPAccessValue);
                    if (!bSetIPAccessSuccess)
                    {
                        ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6237), "La mise à jour de CONFIG a échoué");
                        LaunchError();
                    }
                    else
                    {
                        res.Success = true;
                        res.Html = bSetIPAccessValue ? "1" : "0";
                        RenderResult(RequestContentType.SCRIPT, delegate () { return SerializerTools.JsonSerialize(res); });
                    }
                    break;
                case "update":
                case "add":
                    int nUpdatedRowsOrAddedId = eAdminAccessSecurityIP.UpdateOrInsertIPAddress(_pref, ipData, out strError);
                    if (strError.Length > 0)
                    {
                        ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6237), strError);
                        LaunchError();
                    }
                    else
                    {
                        res.Success = true;
                        res.Html = nUpdatedRowsOrAddedId.ToString();
                        RenderResult(RequestContentType.SCRIPT, delegate () { return SerializerTools.JsonSerialize(res); });
                    }
                    break;
                case "delete":
                    eAdminAccessSecurityIPData updatedIP = new eAdminAccessSecurityIPData(nIPId, strIPLabel, strIPAddress, strIPMask, uvalue, lvalue, 0, PermissionMode.MODE_NONE);
                    bool bDeleteSuccess = eAdminAccessSecurityIP.DeleteIPAddress(_pref, ipData, out strError);
                    if (!bDeleteSuccess || strError.Length > 0)
                    {
                        ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6237), strError);
                        LaunchError();
                    }
                    else
                    {
                        res.Success = bDeleteSuccess;
                        res.Html = String.Empty;
                        RenderResult(RequestContentType.SCRIPT, delegate () { return SerializerTools.JsonSerialize(res); });
                    }
                    break;
                // TODO: si jamais le renderer d'affichage des IP est externalisé dans un fichier séparé d'eAdminAccessSecurityRenderer, et appelé via JS, le câbler ici.
                default:
                case "list":
                    eAdminRenderer rdr = eAdminRendererFactory.CreateAdminAccessSecurityRenderer(_pref, 800, 600);
                    if (rdr.ErrorMsg.Length > 0)
                    {
                        ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6237), rdr.ErrorMsg);
                        LaunchError();
                    }
                    else
                    {

                        res.Success = true;
                        res.Html = GetResultHTML(rdr.GetContents());

                        RenderResult(RequestContentType.SCRIPT, delegate () { return SerializerTools.JsonSerialize(res); });
                    }
                    break;
            }
        }
    }

    public class JSONReturnAccessSecurityIP : JSONReturnHTMLContent
    {

     
    }
}