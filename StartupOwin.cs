using Com.Eudonet.Internal;
using EudoGraphTeams.Authentication;
using EudoGraphTeams.Helpers;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Notifications;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System;
using System.Configuration;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;

[assembly: OwinStartup(typeof(Com.Eudonet.Xrm.StartupOwin))]

namespace Com.Eudonet.Xrm
{
    public class StartupOwin
    {
        // Load configuration settings from PrivateSettings.config
        private static string appId = ConfigurationManager.AppSettings["ida:AppId"];
        private static string appSecret = ConfigurationManager.AppSettings["ida:AppSecret"];
        private static string redirectUri = ConfigurationManager.AppSettings["ida:RedirectUri"];
        private static string graphScopes = ConfigurationManager.AppSettings["ida:AppScopes"];
        private static string xrmdir = eLibTools.GetServerConfig("xrmdir");




        IAppBuilder _app;

        public void Configuration(IAppBuilder app)
        {
            // Pour plus d'informations sur la configuration de votre application, visitez https://go.microsoft.com/fwlink/?LinkID=316888
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions()
            {
                CookieSameSite = Microsoft.Owin.SameSiteMode.None,
                CookieSecure = CookieSecureOption.Always
            });

            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    ClientId = appId,
                    Authority = "https://login.microsoftonline.com/common/v2.0",
                    Scope = $"openid email profile offline_access {graphScopes}",
                    RedirectUri = redirectUri,
                    //PostLogoutRedirectUri = redirectUri,
                    TokenValidationParameters = new TokenValidationParameters
                    {
                        // For demo purposes only, see below
                        ValidateIssuer = false

                        // In a real multi-tenant app, you would add logic to determine whether the
                        // issuer was from an authorized tenant
                        //ValidateIssuer = true,
                        //IssuerValidator = (issuer, token, tvp) =>
                        //{
                        //  if (MyCustomTenantValidation(issuer))
                        //  {
                        //    return issuer;
                        //  }
                        //  else
                        //  {
                        //    throw new SecurityTokenInvalidIssuerException("Invalid issuer");
                        //  }
                        //}
                    },
                    Notifications = new OpenIdConnectAuthenticationNotifications
                    {
                        AuthenticationFailed = OnAuthenticationFailedAsync,
                        AuthorizationCodeReceived = OnAuthorizationCodeReceivedAsync
                    }
                }
            );
        }

        private static Task OnAuthenticationFailedAsync(AuthenticationFailedNotification<OpenIdConnectMessage,
                                OpenIdConnectAuthenticationOptions> notification)
        {
            notification.HandleResponse();

            string redirect = $"/{xrmdir}/Azure/AzureCheckAuthentication.aspx?step=3&message={notification.Exception.Message}";
            if (notification.ProtocolMessage != null && !string.IsNullOrEmpty(notification.ProtocolMessage.ErrorDescription))
            {
                redirect += $"&debug={notification.ProtocolMessage.ErrorDescription}";
            }
            notification.Response.Redirect(redirect);
            return Task.FromResult(0);
        }

        private async Task OnAuthorizationCodeReceivedAsync(AuthorizationCodeReceivedNotification notification)
        {
            notification.HandleCodeRedemption();

            IConfidentialClientApplication idClient = ConfidentialClientApplicationBuilder.Create(appId)
                .WithRedirectUri(redirectUri)
                .WithClientSecret(appSecret)
                .Build();

            ClaimsPrincipal signedInUser = new ClaimsPrincipal(notification.AuthenticationTicket.Identity);
            SessionTokenStore tokenStore = new SessionTokenStore(idClient.UserTokenCache, HttpContext.Current, signedInUser);

            try
            {
                string[] scopes = graphScopes.Split(' ');

                var result = await idClient.AcquireTokenByAuthorizationCode(
                    scopes, notification.Code).ExecuteAsync();

                CachedUser userDetails = await GraphHelper.GetUserDetailsAsync(result.AccessToken);

                tokenStore.SaveUserDetails(userDetails);
                notification.HandleCodeRedemption(null, result.IdToken);

            }
            catch (MsalException ex)
            {
                string message = "AcquireTokenByAuthorizationCodeAsync threw an exception";
                notification.HandleResponse();
                notification.Response.Redirect($"/{xrmdir}/Azure/AzureCheckAuthentication.aspx?step=3&message={message}&debug={ex.Message}");
            }
            catch (Microsoft.Graph.ServiceException ex)
            {
                string message = "GetUserDetailsAsync threw an exception";
                notification.HandleResponse();
                notification.Response.Redirect($"/{xrmdir}/Azure/AzureCheckAuthentication.aspx?step=3&message={message}&debug={ex.Message}");
            }
            catch (Exception ex)
            {
                notification.HandleResponse();
                notification.Response.Redirect($"/{xrmdir}/Azure/AzureCheckAuthentication.aspx?step=3&message={ex.Message}&debug={ex.StackTrace}");

            }
        }

    }
}
