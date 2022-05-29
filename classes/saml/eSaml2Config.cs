using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography.X509Certificates;
using System.Configuration;
using ITfoxtec.Identity.Saml2.Util;

namespace Com.Eudonet.Xrm
{

    /// <summary>
    /// Configuration saml2 de la base
    /// </summary>
    public class eSaml2DatabaseConfig
    {
        /// <summary>
        /// Taille maxi du nom de service
        /// </summary>
        public const int SERVICE_NAME_SIZE = 40;

        /// <summary>
        /// Définit la source et l'entityId du service provider
        /// </summary>
        public string ServiceProviderIssuer { get; set; } = string.Empty;

        /// <summary>
        /// Formats des identifiants
        /// </summary>
        public string AllowedAudienceUri { get; set; } = string.Empty;

        /// <summary>
        /// Définit l'identité de fournisseur d'identité
        /// </summary>
        public string IdentityProviderIssuer { get; set; } = string.Empty;

        /// <summary>
        /// Définit l'url de la page d'authentification du founisseur d'identity
        /// </summary>
        public string SignOnDestination { get; set; } = string.Empty;

        /// <summary>
        /// Définit le mode du binding : POST ou redirecte
        /// </summary>
        public string SignOnDestinationBinding { get; set; } = "HTTP-Redirect";

        /// <summary>
        /// Définit l'url de la page de déconnexion du founisseur d'identity
        /// </summary>
        public string LogoutDestination { get; set; } = string.Empty;

        /// <summary>
        /// Définit le mode du binding : POST ou redirecte
        /// </summary>
        public string AssertionConsumerServiceBinding { get; set; } = "HTTP-POST";

        /// <summary>
        /// Pour inclure dans la demande, le souhait que l'utilisaeur s'authentifie
        /// </summary>
        public bool ForceAuthen { get; set; } = false;

        /// <summary>
        /// L'assertion devera être signée
        /// </summary>
        public bool WantAssertionsSigned { get; set; } = false;

        /// <summary>
        /// Pour inclure une signature de la requete d'authentification dans l'url
        /// </summary>
        public bool SignRequest { get; set; } = false;

        /// <summary>
        /// Définit l'algorithme de hashage de la signature
        /// </summary>
        public string SignAlgorithm { get; set; } = "SHA256";

        /// <summary>
        /// Clé public de l'idp pour vérifier les signatures
        /// </summary>
        public string X509Certificate { get; set; } = string.Empty;

        /// <summary>
        /// Format d'identifiant du subject
        /// </summary>
        public string NameIDFormat { get; set; } = "urn:oasis:names:tc:SAML:1.1:nameid-format:unspecified";

        /// <summary>
        /// Formats des identifiants de clé de dédublonnage
        /// </summary>
        public List<eSaml2Map> MappingAttributes { get; set; } = new List<eSaml2Map>();

        /// <summary>
        /// Pour auditer les connexion
        /// </summary>
        public bool AuditLogging { get; set; } = false;

        /// <summary>
        /// Mode de validation de certificat
        /// </summary>
        public string CertificateValidationMode { get; set; } = "ChainTrust";

        /// <summary>
        /// Mode de revocation de certificat
        /// </summary>
        public string RevocationMode { get; set; } = "Online";

        /// <summary>
        /// Metadonnée de fournisseur d'identité
        /// </summary>
        public string ServiceName { get; set; } = string.Empty;

        /// <summary>
        /// Metadonnée de fournisseur d'identité
        /// </summary>
        public string metadata { get; set; } = string.Empty;

        /// <summary>
        /// Retorne une config minimale
        /// </summary>
        /// <param name="pref">pref de user</param>
        /// <returns></returns>
        public static eSaml2DatabaseConfig GetDefault(ePref pref)
        {
            return new eSaml2DatabaseConfig()
            {
                ServiceProviderIssuer = new Uri(string.Concat(pref.AppExternalUrl.TrimEnd('/'), "/sp/", pref.DatabaseUid.ToLower())).ToString(),
                AllowedAudienceUri = new Uri(string.Concat(pref.AppExternalUrl.TrimEnd('/'), "/sp/", pref.DatabaseUid.ToLower())).ToString(),
                MappingAttributes = new List<eSaml2Map>() { new eSaml2Map(), new eSaml2Map(), new eSaml2Map(), new eSaml2Map(), new eSaml2Map() }
            };
        }
        /// <summary>
        /// Construit un objet config de la base depuis le json
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public static eSaml2DatabaseConfig GetConfigOrDefault(ePref pref, string json)
        {
            eSaml2DatabaseConfig config = SerializerTools.JsonDeserialize<eSaml2DatabaseConfig>(json);
            if (config == null)
                config = GetDefault(pref);

            Correct(config);

            return config;
        }

        /// <summary>
        /// Construit un objet config de la base depuis le json execption si param invalide
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static eSaml2DatabaseConfig GetConfig(string json)
        {
            eSaml2DatabaseConfig config = SerializerTools.JsonDeserialize<eSaml2DatabaseConfig>(json);
            if (config == null)
                throw new eSaml2Exception("Configuration SAML2 incomplète !", "Merci de contacter votre administrateur", "Pas de config SAML2");

            Correct(config);

            return config;
        }

        /// <summary>
        /// Vérification et corrige les params
        /// </summary>
        /// <param name="config"></param>
        private static void Correct(eSaml2DatabaseConfig config)
        {
            // La taile du nom de service est limité 
            if (!string.IsNullOrEmpty(config.ServiceName) && config.ServiceName.Length > SERVICE_NAME_SIZE)
                config.ServiceName = config.ServiceName.Substring(0, SERVICE_NAME_SIZE);
        }

        /// <summary>
        /// Récupère le certificat depuis le magasin
        /// </summary>
        /// <param name="key">clé web config</param>
        /// <returns></returns>
        public static X509Certificate2 LoadCertificate(string key)
        {
            try
            {
                string thumbprint = ConfigurationManager.AppSettings.Get(key);
                if (!string.IsNullOrWhiteSpace(thumbprint))
                {
                    return CertificateUtil.Load(StoreName.My, StoreLocation.LocalMachine, X509FindType.FindByThumbprint, thumbprint);
                }
            }
            catch (Exception) {

                throw;
            }

           return null;
        }
    }
}