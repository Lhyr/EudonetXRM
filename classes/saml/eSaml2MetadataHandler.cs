using Com.Eudonet.Internal;
using EudoQuery;
using ITfoxtec.Identity.Saml2;
using ITfoxtec.Identity.Saml2.Schemas;
using ITfoxtec.Identity.Saml2.Schemas.Metadata;
using ITfoxtec.Identity.Saml2.Util;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using System.Configuration;
using Com.Eudonet.Xrm;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{

    /// <summary>
    /// Permet de construire une requete d'authentification SAML pour l'IDP
    /// </summary>
    public class eSaml2MetadataHandler : eEudoManagerReadOnly
    {
        eAuditLogging logger;
        /// <summary>
        /// Générer la requete avec le contexte
        /// </summary>    
        protected override void ProcessManager()
        {
            string metadata = string.Empty;
            try
            {
                eRequestTools tools = new eRequestTools(HttpContext.Current);

                string directory;
                string url = GetAppUrl(_pref.DatabaseUid, out directory).TrimEnd('/');

                eSaml2Settings settings = new eSaml2Settings(ePrefTools.GetDefaultPrefSql(directory));
                eSaml2DatabaseConfig dbconfig;
                try
                {
                    settings.Init();
                    dbconfig = settings.DbConfig;
                }
                catch
                {
                    dbconfig = new eSaml2DatabaseConfig();
                }

                logger = new eAuditLogging(_context.Session?.SessionID, _pref.User.UserLogin, _pref.GetBaseName, dbconfig.AuditLogging ? 0 : 2);

                Saml2Configuration config = GetConfig(url, _pref.DatabaseUid);
                config.SignAuthnRequest = dbconfig.SignRequest && config.SignAuthnRequest;
                config.SignatureAlgorithm = eSignAlgorithm.From(dbconfig.SignAlgorithm);


                EntityDescriptor entityDescriptor = new EntityDescriptor(config);



                entityDescriptor.SPSsoDescriptor = new SPSsoDescriptor();
                entityDescriptor.SPSsoDescriptor.AuthnRequestsSigned = dbconfig.SignRequest;
                entityDescriptor.SPSsoDescriptor.CertificateIncludeOption = X509IncludeOption.EndCertOnly;
                entityDescriptor.SPSsoDescriptor.WantAssertionsSigned = dbconfig.WantAssertionsSigned;


                // La validité des metadonnées repose sur la validité du certificat
                if (config.SigningCertificate != null)
                {
                    DateTime now = DateTime.Now; DateTime future;
                    if (DateTime.TryParse(config.SigningCertificate.GetExpirationDateString(), out future) && future > now)
                        entityDescriptor.ValidUntil = (int)future.Subtract(DateTime.Now).TotalDays;

                    entityDescriptor.SPSsoDescriptor.SigningCertificates = new X509Certificate2[] { config.SigningCertificate };
                }

                if (config.DecryptionCertificate != null)
                    entityDescriptor.SPSsoDescriptor.EncryptionCertificates = new X509Certificate2[] { config.DecryptionCertificate };


                entityDescriptor.SPSsoDescriptor.NameIDFormats = new Uri[] { new Uri(dbconfig.NameIDFormat) };


                // Mapping user
                List<RequestedAttribute> requestedAttributes = new List<RequestedAttribute>();
                foreach (var i in dbconfig.MappingAttributes)
                    if (!string.IsNullOrWhiteSpace(i.Saml2Attribute))
                        requestedAttributes.Add(new RequestedAttribute(i.Saml2Attribute, i.IsKey));

                entityDescriptor.SPSsoDescriptor.AttributeConsumingServices = new AttributeConsumingService[]
               {
                    new AttributeConsumingService()
                    {
                        ServiceName = new ServiceName(dbconfig.ServiceName, "fr"),
                        RequestedAttributes = requestedAttributes
                    }
               };

                entityDescriptor.SPSsoDescriptor.AssertionConsumerServices = new AssertionConsumerService[]
                {
                    new AssertionConsumerService  { Binding = ProtocolBindings.HttpPost, Location = new Uri($"{url}/sp/{_pref.DatabaseUid.ToLower()}/login")},
                    new AssertionConsumerService { Binding = ProtocolBindings.HttpRedirect, Location = new Uri($"{url}/sp/{_pref.DatabaseUid.ToLower()}/login") }
                };




                logger.Info($"Génération du fichier de méta-données SP", "CONFIG");
                Saml2Metadata saml2Metadata = new Saml2Metadata(entityDescriptor);

                metadata = saml2Metadata.CreateMetadata().ToXml();
                _context.Response.Clear();
                _context.Response.ContentType = "text/xml";
                _context.Response.ContentEncoding = Encoding.UTF8;
                _context.Response.Write(metadata);
            }
            catch (Exception ex)
            {
                logger.Error($"Une erreur est survenue { ex.GetType()} {ex.Message}", "INTERNAL");


#if DEBUG
                metadata = $"Une erreur est survenue <br />{ex.Message}<br /> {ex.StackTrace}";
#else
                metadata = $"Une erreur est survenue <br />{ex.Message}";
#endif

                _context.Response.Clear();
                _context.Response.ContentType = "text/html";
                _context.Response.ContentEncoding = Encoding.UTF8;
                _context.Response.Write(metadata);
            }
        }

        private Saml2Configuration GetConfig(string url, string dbUid)
        {
            // Avec les metadata du 
            Saml2Configuration config = new Saml2Configuration();
            try
            {
                config.SigningCertificate = eSaml2DatabaseConfig.LoadCertificate("Saml2:SigningCertificate");
                config.DecryptionCertificate = eSaml2DatabaseConfig.LoadCertificate("Saml2:EncryptionCertificate");
            }
            catch (Exception ex)
            {
                logger.Warn($"Certificats introuvables <br />{ex.Message}", "INTERNAL");

                config.SigningCertificate = null;
                config.DecryptionCertificate = null;
                config.SignAuthnRequest = false;
            }

            config.Issuer = new Uri($"{url}/sp/{dbUid.ToLower()}");
            config.AllowedAudienceUris.Add(config.Issuer);

            if (config.SigningCertificate != null)
                config.SignAuthnRequest = true;

            return config;
        }

        /// <summary>
        /// Retourne l'appurl de la base
        /// </summary>
        /// <param name="dbUid"></param>
        /// <param name="directory"></param>
        /// <returns></returns>
        internal static string GetAppUrl(string dbUid, out string directory)
        {

            directory = string.Empty;
            string url = eLibTools.GetAppUrl(HttpContext.Current.Request);
            using (eudoDAL dal = eLibTools.GetEudoDAL(ePrefTools.GetDefaultPrefSql("EUDOLOG")))
            {
                RqParam rq = new RqParam("Select AppUrl, AppExternalUrl, Directory from [DATABASES] where uid=@uid");
                rq.AddInputParameter("@uid", System.Data.SqlDbType.VarChar, dbUid);
                dal.OpenDatabase();

                string error;
                DataTableReaderTuned dtrTuned = dal.Execute(rq, out error);
                if (!string.IsNullOrWhiteSpace(error) || dtrTuned == null || !dtrTuned.Read())
                {
                    throw new Exception($"Error identification : {error}");
                }

                directory = dtrTuned.GetString("Directory");
                if (string.IsNullOrEmpty(directory))
                {
                    throw new Exception($"Error interne");
                }

                string appUrl = dtrTuned.GetString("AppUrl");
                if (string.IsNullOrEmpty(appUrl))
                {
                    appUrl = url;
                }

                string appExternalUrl = dtrTuned.GetString("AppExternalUrl");
                if (string.IsNullOrEmpty(appExternalUrl))
                {
                    appExternalUrl = appUrl;
                }

                if (!string.IsNullOrEmpty(appExternalUrl))
                {
                    url = appExternalUrl;
                }
            }

            return url;
        }


        /// <summary>
        /// Réutilisable
        /// </summary>
        public new bool IsReusable { get { return true; } }
    }
}