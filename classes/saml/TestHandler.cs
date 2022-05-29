using Com.Eudonet.Internal;
using EudoQuery;



using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Selectors;
using System.IO;
using System.IO.Compression;
using System.Linq;

using System.Security.Policy;
using System.ServiceModel;

using System.Text;
using System.Web;
using System.Web.SessionState;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{

    /// <summary>
    /// Permet de construire une requete d'authentification SAML pour l'IDP
    /// </summary>
    public class TestHandler : IHttpHandler, IRequiresSessionState
    {
        /// <summary> Id de la langue </summary>
        private int _iLang = 0;


        /// <summary> Token de la base </summary>
        private DbToken _cDbToken;

        /// <summary> token abonné/mdp de la base</summary>
        private SubscriberToken _cSubscriberToken;

        /// <summary> lang de l'utilisateur en cours </summary>
        private string _lang;

        /// <summary> Utilitaire pour récup des params qurystring et form</summary>
        private eRequestTools _requestTools;

     

        /// <summary> Préf sql pour la base</summary>
        private ePrefSQL _prefSQL;

        /// <summary>
        /// Générer la requete avec le contexte
        /// </summary>
        /// <param name="context">Contxt http</param>
        public void ProcessRequest(HttpContext context)
        {
            try
            {
                _requestTools = new eRequestTools(context);              
                
                if (!IsSAMLResponse())
                {
                    SaveTokensInSession(context);
                   
                }
                else
                {
                    GetTokensFromSession();
                  
                }

                context.Response.Redirect("https://localhost/xrm.dev/mgr/saml/eLogin.ashx");

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
        /// Permet de charger la config saml
        /// </summary>
        /// <returns>Config saml2 pour la base en cours </returns>
        private void LoadSaml2Config()
        {/*
            // Avec les metadata du 
            Saml2Configuration config = new Saml2Configuration();
            config.SigningCertificate = CertificateUtil.Load(@"D:\md\sp\e2017_sign.pfx");
            config.Issuer = new Uri("http://localhost/xrm.dev");
            config.AllowedAudienceUris.Add(config.Issuer);
            config.RevocationMode = X509RevocationMode.NoCheck;
            config.CertificateValidationMode = X509CertificateValidationMode.None;
            config.DecryptionCertificate = CertificateUtil.Load(@"D:\md\sp\e2017_decrypt.pfx");

            // Infos IDP
            EntityDescriptor entityDescriptor = new EntityDescriptor();
            entityDescriptor.ReadIdPSsoDescriptorFromFile(@"D:\md\idp\okta.xml");

            config.SingleSignOnDestination = entityDescriptor.IdPSsoDescriptor.SingleSignOnServices.First().Location;
            config.SignatureValidationCertificates.AddRange(entityDescriptor.IdPSsoDescriptor.SigningCertificates);

            return config;
            */
        }

        /// <summary>
        /// Savoir si c'est une réponse IDP
        /// </summary>
        /// <returns>vrai si c'est un retour IDP sinon faux</returns>
        private bool IsSAMLResponse()
        {
            if (!string.IsNullOrWhiteSpace(_requestTools.GetRequestFormKeyS("SAMLResponse")))
                return true;

            if (!string.IsNullOrWhiteSpace(_requestTools.GetRequestQSKeyS("SAMLResponse")))
                return true;

            return false;
        }

        /// <summary>
        /// Sauvegarde des tokens dans la session
        /// </summary>
        /// <param name="context"></param>
        private void SaveTokensInSession(HttpContext context)
        {
            string dbToken = _requestTools.GetRequestQSKeyS("dbt");
            string sbToken = _requestTools.GetRequestQSKeyS("st");
            string lang = _requestTools.GetRequestQSKeyS("l");

            // Ressources et langue
            if (string.IsNullOrWhiteSpace(_lang))
            {
                _lang = eTools.GetCookie("langue", context.Request);
                eLibTools.GetLangFromUserPref(_lang, out _lang, out _iLang);
            }

            _requestTools.SetSessionKeyS("dbToken", dbToken);
            _requestTools.SetSessionKeyS("sbToken", sbToken);
            _requestTools.SetSessionKeyS("lang", _iLang.ToString());
        }

        /// <summary>
        /// Récupe des variables de session
        /// </summary>
        private void GetTokensFromSession()
        {
            int.TryParse(_requestTools.GetSessionKeyS("lang"), out _iLang);
            string dbToken = _requestTools.GetSessionKeyS("dbToken");
            string sbToken = _requestTools.GetSessionKeyS("sbToken");

            _cDbToken = new DbToken();
            _cDbToken.LoadTokenCrypted(dbToken);
            _cDbToken.IsSSOEnabled = true;

            _cSubscriberToken = new SubscriberToken();
            _cSubscriberToken.LoadTokenCrypted(sbToken);

            _prefSQL = GetPrefSql();
        }

        /// <summary>
        /// Permet de rediriger l'authentification vers l'IDP
        /// </summary>
        /// <param name="context">Context http demande d'authentification</param>
        /// <param name="protocol">type de binding la requete est ajouté à l'url ou posté dans un formulaire</param>
        private void RequestToLogin(HttpContext context, Uri protocol)
        {
            /*
            _config.SignAuthnRequest = true;
            if (protocol.OriginalString.Equals(ProtocolBindings.HttpPost.OriginalString))
            {
                // Post binding
                Saml2PostBinding binding = new Saml2PostBinding();
                binding = binding.Bind(new Saml2AuthnRequest(_config));
                context.Response.Write(binding.PostContent);
            }
            else if (protocol.OriginalString.Equals(ProtocolBindings.HttpRedirect.OriginalString))
            {
                // Redirect binding  
                Saml2RedirectBinding binding = new Saml2RedirectBinding();
                binding = binding.Bind(new Saml2AuthnRequest(_config));
                context.Response.Redirect(binding.RedirectLocation.OriginalString, true);
            }
            else
            {   // Autres binding non supportés
                throw new ActionNotSupportedException($"Binding {protocol.OriginalString} non supporté ");
            }
            */
        }

        /// <summary>
        /// Après autentification, callback de l'IDP
        /// </summary>
        /// <param name="context">Context http demande d'authentification</param>
        /// <param name="protocol">type de binding la requete est ajouté à l'url ou posté dans un formulaire</param>
        private void HandleResponse(HttpContext context, Uri protocol)
        {
            /*
            IEnumerable<eSaml2Attribute> attributes = GetAttributes(context, _config, protocol);

            // Cnversion des attribut en champ eudo
            IEnumerable<eSaml2Field> fields = Convert(attributes);

            UserToken cUserToken = BuildUserToken(fields);

            // Recuperer un eLoginOL
            eLoginOL login = eSaml2LoginOL.GetSaml2LoginObject(_prefSQL, _cSubscriberToken, cUserToken, fields);

            AUTH_USER_RES res = login.AuthUser(_cDbToken, false, true);
            if (res == AUTH_USER_RES.SUCCESS)
            {
                login.SetSessionVars();
                context.Response.Redirect("eMain.aspx", true);
            }
            else
            {
#if DEBUG
                throw new Exception($" FAIL : {login.ErrMsg} ");
#else
#endif
            }
            */
        }

        /// <summary>
        /// Construit 
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        private UserToken BuildUserToken(IEnumerable<eSaml2Field> fields)
        {
            UserToken cUserToken = new UserToken();
            cUserToken.Login = "UNKNOWN";
            cUserToken.SSOLogin = "UNKNOWN";
            cUserToken.LangId = _iLang;
            var logins = fields.Where(e => e.DescId == (int)UserField.LOGIN || e.DescId == (int)UserField.UserLoginExternal);
            if (logins.Count() > 0)
            {
                cUserToken.SSOLogin = logins.First().ColumnValue.ToString();
                cUserToken.Login = cUserToken.SSOLogin;
            }

            return cUserToken;
        }

        /// <summary>
        /// Retourne une liste de champ mappés avec leur valeur recupérée depuis les attributs
        /// </summary>
        /// <param name="attributes"></param>
        /// <returns></returns>
        private IEnumerable<eSaml2Field> Convert(IEnumerable<eSaml2Attribute> attributes)
        {
            eSaml2Settings settings = new eSaml2Settings(_prefSQL);
            settings.Init();
            IEnumerable<eSaml2Field> fields = settings.Convert(attributes);
            return fields;
        }

        /// <summary>
        /// Récuprer l'objet pref sql 
        /// </summary>
        /// <returns></returns>
        private ePrefSQL GetPrefSql()
        {
            ePrefSQL prefSQL = eLoginOL.GetBasePrefSQL();
            prefSQL = new ePrefSQL(_cDbToken.SqlServerInstanceName, _cDbToken.DbDirectory, prefSQL.GetSqlUser, prefSQL.GetSqlPassword, prefSQL.GetSqlApplicationName);
            return prefSQL;
        }

        /// <summary>
        /// Récupère les attribut de l'IDP
        /// </summary>
        /// <param name="context">Context http demande d'authentification</param>
        /// <param name="config">Config saml client</param>
        /// <param name="protocol">type de binding la requete est ajouté à l'url ou posté dans un formulaire</param>
        /*
        private List<eSaml2Attribute> GetAttributes(HttpContext context, Saml2Configuration config, Uri protocol)
        {
            ITfoxtec.Identity.Saml2.Http.HttpRequest request = new ITfoxtec.Identity.Saml2.Http.HttpRequest()
            {
                Form = context.Request.Form,
                Method = context.Request.HttpMethod,
                Query = context.Request.QueryString,
                QueryString = context.Request.Url.Query
            };

            Saml2AuthnResponse saml2AuthnResponse = new Saml2AuthnResponse(config);

            if (protocol.OriginalString.Equals(ProtocolBindings.HttpPost.OriginalString))
            {
                // Post binding
                Saml2PostBinding binding = new Saml2PostBinding();
                binding.Unbind(request, saml2AuthnResponse);
            }
            else if (protocol.OriginalString.Equals(ProtocolBindings.HttpRedirect.OriginalString))
            {
                // Redirect binding
                Saml2RedirectBinding binding = new Saml2RedirectBinding();
                binding.Unbind(request, saml2AuthnResponse);
            }
            else
            {   // Autres binding non supportés
                throw new ActionNotSupportedException($"Binding {protocol.OriginalString} non supporté ");
            }

            // En cas d'echec de l'IDP on redirige sur la page d'erreur
            if (saml2AuthnResponse.Status != Saml2StatusCodes.Success)
            {
                context.Response.Redirect("eADFSERROR.html", true);
            }

            // Attribut retournés par l'IDP
            List<eSaml2Attribute> attributes = new List<eSaml2Attribute>();
            foreach (Claim cl in saml2AuthnResponse.ClaimsIdentity.Claims)
            {
                attributes.Add(new eSaml2Attribute() { AttributeName = cl.Type, AttributeValue = cl.Value });
            }

            return attributes;
        }

    */
        /// <summary>
        /// Réutilisable
        /// </summary>
        public bool IsReusable { get { return true; } }
    }
}