using System;
using System.Collections.Generic;
using Com.Eudonet.Internal.eda;
using Com.Eudonet.Internal;
using System.Web;
using EudoQuery;
using System.Text;
using System.IO;
using System.Web.UI;
using System.Configuration;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Description résumée de eAdminConfigManager
    /// </summary>
    public class eAdminSamlManager : eAdminManager
    {
        eAuditLogging logger;

        /// <summary>
        /// Manager des demandes liées aux paramétrages de saml2
        /// </summary>
        protected override void ProcessManager()
        {
            eSaml2DatabaseConfig dbconfig = eSaml2DatabaseConfig.GetDefault(_pref);

            // Pour debugger on trace toutes les modifs admin
            logger = new eAuditLogging(_context.Session?.SessionID, _pref.User.UserLogin, _pref.GetBaseName, 0);

            eSamlJsonResult result = new eSamlJsonResult() { Success = true };
            string action = _requestTools.GetRequestFormKeyS("action");
            switch (action)
            {
                case "reset-metadata":
                    result = SetConfig(dbconfig);
                    logger.Info("Initialisation des paramètres", "CONFIG");
                    break;
                case "idp-metadata":
                    result = ManageLocalMatadataFile();

                    logger.Info("Chargement des méta-données de IDP", "CONFIG");
                    break;
                case "sp-activation":
                    eLibConst.AuthenticationMode mode = _requestTools.GetRequestFormKeyS("saml2enable") == "1" ? eLibConst.AuthenticationMode.SAML2 : eLibConst.AuthenticationMode.EUDONET;
                    result = SetConfig(dbconfig);
                    SetMode(result, mode);

                    logger.Info($"Activation du mode de connexion {mode.ToString()}", "CONFIG");
                    break;
                case "sp-config-trace":

                    string key = _requestTools.GetRequestFormKeyS("id") ?? "";
                    string value = _requestTools.GetRequestFormKeyS("value") ?? "";

                    logger.Info($"Modification du paramétrage {key}:{value}", "CONFIG");
                    break;
                default:
                    break;
            }
            result.Action = action;
            RenderResult(RequestContentType.SCRIPT, delegate () { return SerializerTools.JsonSerialize(result); });
        }

        /// <summary>
        /// Mis a jour le mode d'authentification dans databases si mode Intranet
        /// </summary>
        /// <param name="result"></param>
        /// <param name="mode"></param>
        private void SetMode(eSamlJsonResult result, eLibConst.AuthenticationMode mode)
        {          

            try
            {
                //  On mis a jour   Database            
                eLibTools.SetDatabaseAuthnMode(_pref, mode);
            }
            catch (Exception ex)
            {
                 result.Success = false;
                result.ErrorTitle = eResApp.GetRes(_pref, 961);// "Mise à jour";
                result.ErrorMsg = eResApp.GetRes(_pref, 6172);// "Une erreur est survenue lors de la mise à jour"
                result.DebugDetail = ex.Message;

                logger.Error($"impossible de mettre à jour le mode: {ex.Message} \n  {ex.StackTrace}", "CONFIG");
            }
        }


        /// <summary>
        /// Mise a jour la configuration de saml
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        private eSamlJsonResult SetConfig(eSaml2DatabaseConfig config)
        {
            eSamlJsonResult result = new eSamlJsonResult() { Success = true };
            try
            {
                // Mise a jour de configadv
                string jsonValue = SerializerTools.JsonSerialize(config);
                eLibTools.AddOrUpdateConfigAdv(_pref, eLibConst.CONFIGADV.AUTHENTICATION_SETTINGS, jsonValue, eLibConst.CONFIGADV_CATEGORY.SYSTEM);


                // rendu de la nouvelle config
                eAdminSaml2PartRenderer renderer = new eAdminSaml2PartRenderer(_pref, config, jsonValue);
                if (renderer.Generate())
                    result.Html = renderer.ToString();
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorTitle = eResApp.GetRes(_pref, 961);// "Mise à jour";
                result.ErrorMsg = eResApp.GetRes(_pref, 6172);// "Une erreur est survenue lors de la mise à jour"
#if DEBUG
                result.DebugMsg = ex.Message;
                result.DebugDetail = ex.StackTrace;
#endif

                logger.Error($"impossible de mettre à jour la config: {ex.Message} \n  {ex.StackTrace}", "CONFIG");

            }

            return result;
        }

        /// <summary>
        /// Récupère le fichier uploadé et construit la config saml
        /// </summary>
        /// <returns></returns>
        private eSamlJsonResult ManageLocalMatadataFile()
        {
            // présence du fichier
            if (_context.Request.Files == null || _context.Request.Files["file"] == null)
                return new eSamlJsonResult() { Success = false, ErrorTitle = eResApp.GetRes(_pref, 776), ErrorMsg = eResApp.GetRes(_pref, 660) };

            HttpPostedFile file = _context.Request.Files["file"];

            // Foarmat xml accepté
            if (!file.FileName.EndsWith(".xml"))
            {
                logger.Warn($"Fichier '{file.FileName}' non valide", "CONFIG");
                return new eSamlJsonResult() { Success = false, ErrorTitle = eResApp.GetRes(_pref, 8461), ErrorMsg = $"{ eResApp.GetRes(_pref, 8462)} xml" };
            }

            // Contient des données
            if (file.ContentLength == 0)
            {
                logger.Warn($"Fichier '{file.FileName}' est vide", "CONFIG");
                return new eSamlJsonResult() { Success = false, ErrorTitle = eResApp.GetRes(_pref, 8461), ErrorMsg = $"'{file.FileName}' {eResApp.GetRes(_pref, 2010)}" };
            }

            try
            {
                // Mise a jour de config
                string content = GetContent(file);
                eSaml2DatabaseConfig config = BuildConfig(content);
                eSamlJsonResult result = SetConfig(config);
                return result;
            }
            catch (Exception ex)
            {
                eSamlJsonResult result = new eSamlJsonResult();
                result.Success = false;
                result.ErrorTitle = eResApp.GetRes(_pref, 961);// "Mise à jour";
                result.ErrorMsg = eResApp.GetRes(_pref, 6172);// "Une erreur est survenue lors de la mise à jour"
#if DEBUG
                result.DebugMsg = ex.Message;
                result.DebugDetail = ex.StackTrace;
#endif

                logger.Error($"impossible de lire/mettre à jour les metadonnées: {ex.Message} \n  {ex.StackTrace}", "CONFIG");
                return result;
            }
        }

        /// <summary>
        /// Récupère le contenu du fichier posté
        /// </summary>
        /// <param name="file">fichier posté</param>
        /// <returns></returns>
        private string GetContent(HttpPostedFile file)
        {
            using (StreamReader streamReader = new StreamReader(file.InputStream, Encoding.Default))
            {
                return streamReader.ReadToEnd();
            }
        }

        /// <summary>
        /// Construit la config
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        private eSaml2DatabaseConfig BuildConfig(string metadata)
        {
            var json = eLibTools.GetConfigAdvValues(_pref, eLibConst.CONFIGADV.AUTHENTICATION_SETTINGS)[eLibConst.CONFIGADV.AUTHENTICATION_SETTINGS];
            eSaml2DatabaseConfig cfg = SerializerTools.JsonDeserialize<eSaml2DatabaseConfig>(json);
            if (cfg == null)
                cfg = eSaml2DatabaseConfig.GetDefault(_pref);

            eSaml2SignOnHandler.LoadMetadata(cfg, metadata);
            return cfg;
        }

        private eSamlJsonResult ManageRemoteMatadataFile()
        {
            return new eSamlJsonResult() { Success = true }; ;
        }

        /// <summary>
        /// Json de saml pour le retour client
        /// </summary>
        public class eSamlJsonResult : JSONReturnGeneric
        {
            /// <summary>
            /// Action demandé
            /// </summary>
            public string Action { get; set; } = string.Empty;

            /// <summary>
            /// Contenu html
            /// </summary>
            public string Html { get; set; } = string.Empty;

            /// <summary>
            /// Détail du message d'erreur
            /// </summary>
            public string Json { get; set; } = string.Empty;

            /// <summary>
            /// Message de débug
            /// </summary>
            public string DebugMsg { get; set; } = string.Empty;

            /// <summary>
            /// Stacktrace
            /// </summary>
            public string DebugDetail { get; set; } = string.Empty;
        }
    }
}