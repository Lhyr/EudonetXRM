using EudoGraphTeams.Authentication;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;


namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Description résumée de AzureLogout
    /// </summary>
    public class AzureLogout : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {

            context.Request.GetOwinContext().Authentication.SignOut(
                OpenIdConnectAuthenticationDefaults.AuthenticationType);
        }


        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}