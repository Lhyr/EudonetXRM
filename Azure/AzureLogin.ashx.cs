using Com.Eudonet.Internal;
using EudoGraphTeams.Authentication;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Configuration;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Description résumée de AzureLogin
    /// </summary>
    public class AzureLogin : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            SessionTokenStore tokenStore = new SessionTokenStore(null, context, ClaimsPrincipal.Current);
            string redirectUri = ConfigurationManager.AppSettings["ida:RedirectUri"];

            if (!tokenStore.HasUser())
            {

                AuthenticationProperties authenticationProperties = new AuthenticationProperties
                {
                    RedirectUri = redirectUri,          //$"/{eLibTools.GetServerConfig("xrmdir", "XRM")}/Azure/AzureLogin.ashx",
                    IsPersistent = true,
                    AllowRefresh = true

                };


                //On récupère le nom du cookie de session
                SessionStateSection sessionStateSection = (SessionStateSection)ConfigurationManager.GetSection("system.web/sessionState");

                string cookieName = sessionStateSection.CookieName;


                // Signal OWIN to send an authorization request to Azure
                IOwinContext owinContext = context.Request.GetOwinContext();

                owinContext.Response.Cookies.Append(cookieName, context.Session.SessionID, new CookieOptions()
                {
                    SameSite = Microsoft.Owin.SameSiteMode.None,
                    Secure = true
                });

                owinContext.Authentication
                    .Challenge(authenticationProperties,
                        OpenIdConnectAuthenticationDefaults.AuthenticationType);
            }

            else
            {
                context.Response.Redirect("AzureCheckAuthentication.aspx");
            }
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