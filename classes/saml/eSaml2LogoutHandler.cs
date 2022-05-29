using Com.Eudonet.Internal;
using ITfoxtec.Identity.Saml2;
using ITfoxtec.Identity.Saml2.Schemas;
using ITfoxtec.Identity.Saml2.Schemas.Metadata;
using ITfoxtec.Identity.Saml2.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Security;
using System.Text;
using System.Web;
using System.Web.SessionState;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{

    /// <summary>
    /// Permet de construire une requete d'authentification SAML pour l'IDP
    /// </summary>
    public class eSaml2LogoutHandler : IHttpHandler, IRequiresSessionState
    {
        /// <summary>
        /// Id de la langue
        /// </summary>
        private int _iLang = 0;


        /// <summary>
        /// Générer la requete avec le contexte
        /// </summary>
        /// <param name="context">Contxt http</param>
        public void ProcessRequest(HttpContext context)
        {
            try
            {
                // Avec les metadata du 
                Saml2Configuration config = new Saml2Configuration();
                config.SigningCertificate = CertificateUtil.Load(@"D:\md\sp\e2017_sign.pfx");

                config.Issuer = new Uri("http://localhost/xrm.dev");
                config.AllowedAudienceUris.Add(config.Issuer);

                config.RevocationMode = X509RevocationMode.NoCheck;
                config.CertificateValidationMode = X509CertificateValidationMode.None;

                config.DecryptionCertificate = CertificateUtil.Load(@"D:\md\sp\e2017_decrypt.pfx");

                //  config.SignatureValidationCertificates.Add(CertificateUtil.Load(@"D:\md\idp\okta.cert"));
                //  config.SingleSignOnDestination = new Uri("https://dev-429577.oktapreview.com/app/eudodev429577_e2017saml20testapp_1/exkemczzl2XyUjqh60h7/sso/saml");


                // Infos IP
                EntityDescriptor entityDescriptor = new EntityDescriptor();

               // Choix depuis un fichier ou l'url ou manuel
               //  entityDescriptor.ReadIdPSsoDescriptorFromUrl(new Uri("https://dev-429577.oktapreview.com/app/exkemczzl2XyUjqh60h7/sso/saml/metadata"));
               entityDescriptor.ReadIdPSsoDescriptorFromFile(@"D:\md\idp\okta.xml");

                if (entityDescriptor.IdPSsoDescriptor != null)
                {
                    config.SingleSignOnDestination = entityDescriptor.IdPSsoDescriptor.SingleSignOnServices.First().Location;                  
                    config.SignatureValidationCertificates.AddRange(entityDescriptor.IdPSsoDescriptor.SigningCertificates);                  
                }
                else
                {
                    throw new Exception("Identity Provider inconnu !");
                }
                

                if (context.Request.Form != null && context.Request.Form["SAMLResponse"] != null)
                {
                    LoginResponsePost(context, config);
                }
                else
                {
                 //   LoginRequestPost(context, config);
                    LoginRequestRedirect(context, config);
                }

            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("Massage : " + ex.Message).Append("<br />");
                sb.Append("StackTrace : ").Append("<br />");
                sb.Append(ex.StackTrace);

                context.Response.Write(sb.ToString());
                context.Response.End();
            }
        }

        /// <summary>
        /// Permet de rediriger l'authentification vers l'IDP
        /// </summary>
        /// <param name="context">Context http demande d'authentification</param>
        /// <param name="config">Config</param>
        private void LoginRequestPost(HttpContext context, Saml2Configuration config)
        {
            config.SignAuthnRequest = true;

            Debug.Listeners.Add(new TextWriterTraceListener(@"D:\md\sp\sp.log"));

            Saml2PostBinding binding = new Saml2PostBinding();


            eRequestTools tools = new eRequestTools(context);
            string width = tools.GetRequestQSKeyS("w");
            string height = tools.GetRequestQSKeyS("h");
            string dbToken = tools.GetRequestQSKeyS("dbt");
            string sbToken = tools.GetRequestQSKeyS("st");
            string lang = tools.GetRequestQSKeyS("l");

            if (string.IsNullOrWhiteSpace(lang))
            {  // Ressources et langue
                lang = eTools.GetCookie("langue", context.Request);
                eLibTools.GetLangFromUserPref(lang, out lang, out _iLang);
            }

            binding.SetRelayStateQuery(new Dictionary<string, string>
            {
                { "w", width },
                { "h", height },
                { "dbt", dbToken },
                { "st", sbToken },
                { "l", lang }
            });

            binding = binding.Bind(new Saml2AuthnRequest(config));
            context.Response.Write(binding.PostContent);
        }

        /// <summary>
        /// Permet de rediriger l'authentification vers l'IDP
        /// </summary>
        /// <param name="context">Context http demande d'authentification</param>
        /// <param name="config">Config</param>
        private void LoginRequestRedirect(HttpContext context, Saml2Configuration config)
        {
            config.SignAuthnRequest = true;

            Debug.Listeners.Add(new TextWriterTraceListener(@"D:\md\sp\sp.log"));

            Saml2RedirectBinding binding = new Saml2RedirectBinding();


            eRequestTools tools = new eRequestTools(context);
            string width = tools.GetRequestQSKeyS("w");
            string height = tools.GetRequestQSKeyS("h");
            string dbToken = tools.GetRequestQSKeyS("dbt");
            string sbToken = tools.GetRequestQSKeyS("st");
            string lang = tools.GetRequestQSKeyS("l");

            if (string.IsNullOrWhiteSpace(lang))
            {  // Ressources et langue
                lang = eTools.GetCookie("langue", context.Request);
                eLibTools.GetLangFromUserPref(lang, out lang, out _iLang);
            }

            binding.SetRelayStateQuery(new Dictionary<string, string>
            {
                { "w", width },
                { "h", height },
                { "dbt", dbToken },
                { "st", sbToken },
                { "l", lang }
            });
                       

            binding = binding.Bind(new Saml2AuthnRequest(config));

            string url = binding.RedirectLocation.OriginalString;


            Debug.Flush();

            context.Response.Redirect(url, true);

        }

        /// <summary>
        /// Après autentification, callback de l'IDP
        /// </summary>
        /// <param name="context">Context http demande d'authentification</param>
        /// <param name="config">Config saml client</param>
        private void LoginResponsePost(HttpContext context, Saml2Configuration config)
        {
            Saml2PostBinding postBinding = new Saml2PostBinding();
            Saml2AuthnResponse saml2AuthnResponse = new Saml2AuthnResponse(config);
           
            ITfoxtec.Identity.Saml2.Http.HttpRequest request = new ITfoxtec.Identity.Saml2.Http.HttpRequest()
            {
                Form = context.Request.Form,
                Method = context.Request.HttpMethod,
                Query = context.Request.QueryString,
                QueryString = context.Request.Url.Query
            };
                     
            postBinding.Unbind(request, saml2AuthnResponse);

            if (saml2AuthnResponse.Status != Saml2StatusCodes.Success)
            {
                context.Response.Redirect("eADFSERROR.html", true);
            }                    

            string width = postBinding.GetRelayStateQuery()["w"];
            string height = postBinding.GetRelayStateQuery()["h"];
            string dbToken = postBinding.GetRelayStateQuery()["dbt"];
            string sbToken = postBinding.GetRelayStateQuery()["st"];
            string lang = postBinding.GetRelayStateQuery()["l"];

            SubscriberToken cSubscriberToken = new SubscriberToken();
            DbToken cDbToken = new DbToken();
            UserToken cUserToken = new UserToken();

            cSubscriberToken.LoadTokenCrypted(sbToken);
            cDbToken.LoadTokenCrypted(dbToken);
          
            //Faire un mapping avec les attribut
            cUserToken.Userid = 1;
            cUserToken.SSOLogin = "toto";
            cUserToken.Login = "toto";
            
            // Create or update user

            // Recuperer un eLoginOL
            eLoginOL login = eLoginOL.GetLoginObject(cSubscriberToken, cUserToken);
            
            AUTH_USER_RES res = login.AuthUser(cDbToken, false, true);
            if (res == AUTH_USER_RES.SUCCESS)
            {
                login.SetSessionVars();
                context.Response.Redirect("eMain.aspx", true);
            }
            else
            {
                context.Response.Redirect("eADFSERROR.html", true);
            }           
        }
           
        /// <summary>
        /// Réutilisable
        /// </summary>
        public bool IsReusable { get { return true; } }
    }
}