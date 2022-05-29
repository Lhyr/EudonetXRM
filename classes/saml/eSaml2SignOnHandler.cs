using Com.Eudonet.Internal;
using Com.Eudonet.Xrm;
using EudoQuery;
using ITfoxtec.Identity.Saml2;
using ITfoxtec.Identity.Saml2.Schemas;
using ITfoxtec.Identity.Saml2.Schemas.Metadata;
using ITfoxtec.Identity.Saml2.Util;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Text;
using System.Web;
using System.Web.SessionState;
using System.Security.Cryptography;
using ITfoxtec.Identity.Saml2.Cryptography;
using System.Diagnostics;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using System.Web.Configuration;
using System.Data;

namespace Com.Eudonet.Xrm
{

    /// <summary>
    /// Permet de construire une requete d'authentification SAML pour l'IDP
    /// </summary>
    public class eSaml2SignOnHandler : IHttpHandler, IRequiresSessionState
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

        /// <summary> Config de saml2</summary>
        private Saml2Configuration _saml2Configuration;


        /// <summary> Config de saml2</summary>
        private eSaml2DatabaseConfig _databaseConfiguration;



        /// <summary> Préf sql pour la base</summary>
        private ePrefSQL _prefSQL;

        private eAuditLogging _logger;

        /// <summary>
        /// Générer la requete avec le contexte
        /// </summary>
        /// <param name="context">Contxt http</param>
        public void ProcessRequest(HttpContext context)
        {

            if (ProtectFromDDoS())
            {
                // Protection déclenché, on return
                HttpContext.Current.Response.Redirect($"{eLibTools.GetAppUrl(HttpContext.Current.Request).TrimEnd('/')}/blank.htm");
                return;
            }

            try
            {
                _requestTools = new eRequestTools(context);



                if (!IsSAMLResponse())
                {
                    SaveTokensInSession(context);
                    RequestToLogin(context);
                }
                else
                {
                    Flush(GetResponse("Récupération des informations de connexion..."));
                    GetTokensFromSession();
                    HandleResponse(context, ProtocolBindings.HttpPost);
                }
            }
            catch (eSaml2Exception samlException)
            {
                _logger?.Warn($"Exception {samlException.Message} \n {samlException.StackTrace}", "INTERNAL");
                Flush(UpdateState(samlException.UserMessage, samlException.UserDetailMessage));
                context.ApplicationInstance.CompleteRequest();
            }
            catch (Exception ex)
            {

                _logger?.Error($"Exception {ex.Message} \n {ex.StackTrace}", "INTERNAL");
                Flush(UpdateState(ex.Message, "Si le problème persiste, merci de contacter votre administrateur"));
                context.ApplicationInstance.CompleteRequest();

            }
        }

        /// <summary>
        /// Envoi la réponse au client
        /// </summary>
        /// <param name="htmlContent"></param>        
        private void Flush(string htmlContent)
        {
            HttpContext.Current.Response.Write(htmlContent);
            HttpContext.Current.Response.Flush();
        }

        /// <summary>
        /// Page html d'erreur
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private string GetResponse(string message)
        {
            return $@"  <html>
                        <head>
                        <style>  
                            body {{ font-family: Verdana, 'Trebuchet MS'; font-size:14px;}}    
                            div {{ display:block;width:600px;}}                       
                            div p {{ margin-left: 12px;font-size:12px;}}                       
                        </style>
                        </head>
                        <body> 
                             {UpdateState(message)} 
                         </body>
                        </html>";
        }

        /// <summary>
        /// Page html d'erreur
        /// </summary>
        /// <param name="message"></param>
        /// <param name="detail"></param>
        /// <returns></returns>
        private string UpdateState(string message, string detail = "")
        {
            if (!string.IsNullOrEmpty(detail))
                detail = $"<p>{detail}</p>";

            // return $"<script type='text/javascript'>document.getElementById('content').innerHTML = \"{message}\";</script>";
            return $"<div>{message}{detail}</div>";
        }
        /// <summary>
        /// Page html d'erreur
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private string Redirect(string url)
        {
            return $"<script type='text/javascript'>window.location.replace( \"{ url} \");</script>";
        }

        /// <summary>
        /// Protection DDoS
        /// </summary>
        private bool ProtectFromDDoS()
        {
            #region DDOS
            if (eTools.GetServerConfig("DOSENABLED") == "1")
            {
                Int32 nLimit;
                if (!Int32.TryParse(eTools.GetServerConfig("DOSLIMIT", eLibConst.DOS_DEFAULT_LIMIT.ToString()), out nLimit) || nLimit <= 0)
                    nLimit = eLibConst.DOS_DEFAULT_LIMIT;

                try
                {
                    if (eDoSProtection.CheckDDOS(new WCFCall("LOGIN_SAML", nLimit), true) <= 0)
                        return false;
                }
                catch (eEudoDoSException)
                {

                    return true;
                }
                catch (eEudoDosConcurentException)
                {
                    //access concurent, on laisse passer
                    return true;
                }
                catch (Exception)
                {
                    return true;
                    // le systeme de protection ne doit pas être source d'erreur
                }
            }

            return false;

            #endregion
        }



        /// <summary>
        /// Permet de charger la config saml
        /// </summary>
        /// <returns>Config saml2 pour la base en cours </returns>
        public void LoadSaml2Config()
        {
            _databaseConfiguration = new eSaml2DatabaseConfig();
            try
            {
                string settings = eLibTools.GetConfigAdvValues(_prefSQL, eLibConst.CONFIGADV.AUTHENTICATION_SETTINGS)[eLibConst.CONFIGADV.AUTHENTICATION_SETTINGS];
                if (string.IsNullOrWhiteSpace(settings))
                    throw new eSaml2Exception("Configuration SAML2 incomplète !", "Merci de contacter votre administrateur", "Pas de config SAML2");

                _databaseConfiguration = eSaml2DatabaseConfig.GetConfig(settings);
            }
            catch (eSaml2Exception)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.Warn($"Connexion à la base impossible : {ex.Message}", "INTERNAL");
                _logger.Warn($"{ex.StackTrace}", "INTERNAL");
                throw new eSaml2Exception("Impossible d'acceder à la base !", "Merci de contacter votre administrateur", "Pas de config SAML2");
            }


            // Avec les metadata du 
            _saml2Configuration = new Saml2Configuration();


            _saml2Configuration.Issuer = new Uri(_databaseConfiguration.ServiceProviderIssuer);
            _saml2Configuration.AllowedAudienceUris.Add(_saml2Configuration.Issuer);

            _saml2Configuration.SingleSignOnDestination = new Uri(_databaseConfiguration.SignOnDestination);

            if (!string.IsNullOrWhiteSpace(_databaseConfiguration.metadata))
            {
                EntityDescriptor entity = new EntityDescriptor();
                entity = entity.ReadIdPSsoDescriptor(_databaseConfiguration.metadata);

                // Validation des certificat
                if (entity.IdPSsoDescriptor != null && entity.IdPSsoDescriptor.SigningCertificates.Count() > 0)
                    _saml2Configuration.SignatureValidationCertificates.AddRange(entity.IdPSsoDescriptor.SigningCertificates);
            }

            try
            {
                _saml2Configuration.CertificateValidationMode = (X509CertificateValidationMode)Enum.Parse(typeof(X509CertificateValidationMode), _databaseConfiguration.CertificateValidationMode);
                _saml2Configuration.RevocationMode = (X509RevocationMode)Enum.Parse(typeof(X509RevocationMode), _databaseConfiguration.RevocationMode);

                // On autorise pas Custom.
                if (_saml2Configuration.CertificateValidationMode == X509CertificateValidationMode.Custom)
                    _saml2Configuration.CertificateValidationMode = X509CertificateValidationMode.ChainTrust;
            }
            catch (Exception ex)
            {
                _logger.Error($"Erreur de lecture des options de validation/revocation de certificat : {ex.Message}", "INTERNAL");
                _saml2Configuration.RevocationMode = X509RevocationMode.Online;
                _saml2Configuration.CertificateValidationMode = X509CertificateValidationMode.ChainTrust;
                _logger.Warn($"Utilisation X509RevocationMode.Online et X509CertificateValidationMode.ChainTrust", "INTERNAL");
            }

            // En cas de chiffrement des assertions
            try
            {
                _saml2Configuration.DecryptionCertificate = eSaml2DatabaseConfig.LoadCertificateFromDB(_databaseConfiguration.EncryptionCertificate);
            }
            catch (Exception ex)
            {
                _logger.Warn($"Certificats introuvables <br />{ex.Message}<br /> {ex.StackTrace}", "INTERNAL");
                _saml2Configuration.DecryptionCertificate = null;
            }


            if (_databaseConfiguration.SignRequest)
                _saml2Configuration.SignAuthnRequest = true;

            // Signature
            try
            {
                _saml2Configuration.SigningCertificate = eSaml2DatabaseConfig.LoadCertificateFromDB(_databaseConfiguration.SigningCertificate);
            }
            catch (Exception ex)
            {
                _logger.Warn($"Certificats introuvables <br />{ex.Message}<br /> {ex.StackTrace}", "INTERNAL");
                _saml2Configuration.SigningCertificate = null;
            }

            _saml2Configuration.SignatureAlgorithm = eSignAlgorithm.From(_databaseConfiguration.SignAlgorithm);



            if (_databaseConfiguration.AuditLogging)
                _requestTools.SetSessionKeyS("dl", "0");
        }

        /// <summary>
        /// COnstruit un fichier de metadonnées
        /// </summary>
        /// <param name="cfg"></param>

        public static void LoadMetadata(eSaml2DatabaseConfig cfg)
        {
            if (string.IsNullOrEmpty(cfg.metadata))
                throw new Exception("Metadonnées idp non trouvé");

            LoadMetadata(cfg, cfg.metadata);
        }

        /// <summary>
        /// COnstruit un fichier de metadonnées
        /// </summary>
        /// <param name="cfg"></param>
        /// <param name="metadata"></param>
        public static void LoadMetadata(eSaml2DatabaseConfig cfg, string metadata)
        {
            // Infos IDP
            EntityDescriptor entity = new EntityDescriptor();
            entity = entity.ReadIdPSsoDescriptor(metadata);
            cfg.IdentityProviderIssuer = entity.EntityId.ToString();
            cfg.SignAlgorithm = "SHA256";

            var ssos = entity.IdPSsoDescriptor.SingleSignOnServices.Where(s => s.Binding == ProtocolBindings.HttpRedirect).FirstOrDefault();
            if (ssos == null)
                ssos = entity.IdPSsoDescriptor.SingleSignOnServices.Where(s => s.Binding == ProtocolBindings.HttpPost).FirstOrDefault();

            if (ssos == null)
                throw new Exception("Bindings non supporté(s)"); //TODORES

            cfg.SignOnDestination = ssos.Location.ToString();
            cfg.SignOnDestinationBinding = ssos.Binding.ToString();
            //#88 361 : La balise NameIdFormat n'est pas toujours fournit dans le flux de metadata (notamment Azure AD). Dans ce cas on ajoute un noeud par défaut avec l'email en format
            if (entity.IdPSsoDescriptor.NameIDFormats.Count() == 0)
            {
                cfg.NameIDFormat = "urn:oasis:names:tc:SAML:1.1:nameid-format:emailAddress";
            }
            else
            {
                cfg.NameIDFormat = entity.IdPSsoDescriptor.NameIDFormats.First().ToString();
            }
            cfg.NameIDFormat = entity.IdPSsoDescriptor.NameIDFormats.First().ToString();
            cfg.SignRequest = entity.Config?.SignAuthnRequest ?? true;

            if (entity.Config != null)
                cfg.SignAlgorithm = eSignAlgorithm.GetName(entity.Config.SignatureAlgorithm);

            cfg.metadata = metadata;
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
            // Protection contre ddos
            if (!string.IsNullOrWhiteSpace(_requestTools.GetSessionKeyS("dbToken")) &&
                !string.IsNullOrWhiteSpace(_requestTools.GetSessionKeyS("sbToken")))
            {
                // Protction contre des callbacks       
                _requestTools.SetSessionKeyS("dbToken", null);
                _requestTools.SetSessionKeyS("sbToken", null);
                _requestTools.SetSessionKeyS("lang", null);
                _requestTools.SetSessionKeyS("dl", null);

                _logger?.Warn($"tentative d'initialiser une Session SAML2 déjà initialisée", "IN");
                throw new eSaml2Exception("Opération interdite", "Une autre requête est en cours de traitement pour la même session", "Protection DDoS - tentative d'initialiser une Session SAML2 déjà initialisée");//TODORES
            }
            else
            {


                string dbToken = _requestTools.GetRequestQSKeyS("dbt");
                string sbToken = _requestTools.GetRequestQSKeyS("st");
                string lang = _requestTools.GetRequestQSKeyS("l");
                string debugLevel = _requestTools.GetRequestQSKeyS("dl");

                // Ressources et langue
                int debug;
                if (!(int.TryParse(debugLevel, out debug) && debug >= 0 && debug <= 2))
                    debug = 2;

                // Ressources et langue
                if (string.IsNullOrWhiteSpace(_lang))
                {
                    _lang = eTools.GetCookie("langue", context.Request);
                    eLibTools.GetLangFromUserPref(_lang, out _lang, out _iLang);
                }

                _requestTools.SetSessionKeyS("dbToken", dbToken);
                _requestTools.SetSessionKeyS("sbToken", sbToken);
                _requestTools.SetSessionKeyS("lang", _iLang.ToString());
                _requestTools.SetSessionKeyS("dl", debug.ToString());

                LoadTokensAndPref(dbToken, sbToken, debug);
            }
        }

        /// <summary>
        /// Récupe des variables de session
        /// </summary>
        private void GetTokensFromSession()
        {
            int.TryParse(_requestTools.GetSessionKeyS("lang"), out _iLang);

            int debug;
            int.TryParse(_requestTools.GetSessionKeyS("dl"), out debug);

            string dbToken = _requestTools.GetSessionKeyS("dbToken");
            string sbToken = _requestTools.GetSessionKeyS("sbToken");

            if (string.IsNullOrEmpty(dbToken) || string.IsNullOrEmpty(sbToken))
                throw new eSaml2Exception("Opération invalide", "Paramètre(s) manquants ou invalide(s)", "Pas de dbToken et sbToken transmis");//TODORES

            // Protction contre des callbacks       
            _requestTools.SetSessionKeyS("dbToken", null);
            _requestTools.SetSessionKeyS("sbToken", null);
            _requestTools.SetSessionKeyS("lang", null);
            _requestTools.SetSessionKeyS("dl", null);

            LoadTokensAndPref(dbToken, sbToken, debug);
        }

        /// <summary>
        /// Récupère les tocken et charge les pref
        /// </summary>
        /// <param name="dbToken"></param>
        /// <param name="sbToken"></param>
        /// <param name="debugLevel">0:info, 1:warn, 2:error(par defaut)</param>
        private void LoadTokensAndPref(string dbToken, string sbToken, int debugLevel)
        {
            _cDbToken = new DbToken();
            _cDbToken.LoadTokenCrypted(dbToken);
            _cDbToken.IsSSOEnabled = true;

            _cSubscriberToken = new SubscriberToken();
            _cSubscriberToken.LoadTokenCrypted(sbToken);

            _prefSQL = GetPrefSql();

            _logger = new eAuditLogging(HttpContext.Current?.Session?.SessionID, "ANONYME", _prefSQL.GetBaseName, debugLevel);

            LoadSaml2Config();
        }

        /// <summary>
        /// Permet de rediriger l'authentification vers l'IDP
        /// </summary>
        /// <param name="context">Context http demande d'authentification</param>
        /// <param name="protocol">type de binding la requete est ajouté à l'url ou posté dans un formulaire</param>
        private void RequestToLogin(HttpContext context)
        {
            try
            {
                SetSessionCookieForSameSiteStrategy(context);
                if (_databaseConfiguration.SignOnDestinationBinding.Contains("HTTP-Redirect"))
                    _databaseConfiguration.protocolBindingsURI = ProtocolBindings.HttpRedirect;
                else if(_databaseConfiguration.SignOnDestinationBinding.Contains("HTTP-POST"))
                    _databaseConfiguration.protocolBindingsURI = ProtocolBindings.HttpPost;

                if (_databaseConfiguration.protocolBindingsURI.OriginalString.Equals(ProtocolBindings.HttpPost.OriginalString))
                {
                    _logger.Info($"POST {_saml2Configuration.SingleSignOnDestination}", "OUT");

                    // Post binding
                    Saml2PostBinding binding = new Saml2PostBinding();
                    binding = binding.Bind(new Saml2AuthnRequest(_saml2Configuration) { ForceAuthn = _databaseConfiguration.ForceAuthen });


                    _logger.Info($"SamlRequest {binding.PostContent}", "OUT");
                    context.Response.Write(binding.PostContent);
                    context.ApplicationInstance.CompleteRequest();
                }
                else if (_databaseConfiguration.protocolBindingsURI.OriginalString.Equals(ProtocolBindings.HttpRedirect.OriginalString))
                {
                    _logger.Info($"REDIRECT {_saml2Configuration.SingleSignOnDestination}", "OUT");

                    // Redirect binding  
                    Saml2RedirectBinding binding = new Saml2RedirectBinding();
                    binding = binding.Bind(new Saml2AuthnRequest(_saml2Configuration) { ForceAuthn = _databaseConfiguration.ForceAuthen, });
                    _logger.Info($"SamlRequest {binding.XmlDocument.InnerXml}", "OUT");

                    _logger.Info($"SamlRequest {binding.RedirectLocation.OriginalString}", "OUT");


                    context.Response.Redirect(binding.RedirectLocation.OriginalString, false);
                    context.ApplicationInstance.CompleteRequest();

                }
                else
                {
                    _logger.Warn($"Methode Non supportée {_databaseConfiguration.protocolBindingsURI.ToString()} ", "OUT");
                    //TODORES
                    throw new eSaml2Exception("Opération non supportée", $"Bindings acceptés : {ProtocolBindings.HttpRedirect.OriginalString} et {ProtocolBindings.HttpPost.OriginalString}", $"Binding {_databaseConfiguration.protocolBindingsURI.OriginalString} non supporté");
                }
            }
            catch (eSaml2Exception) { throw; }
            catch (Exception ex)
            {
                _logger.Error($"Error lors de la construction de la requête : {ex.GetType().ToString()}", "OUT");
                _logger.Error($"{ex.Message} \n {ex.StackTrace}", "OUT");

                throw new eSaml2Exception("Une erreur est survenue ! ", ex.Message.Replace("\n", "<br />"), ex.StackTrace);

            }
        }

        /// <summary>
        /// Après autentification, callback de l'IDP
        /// </summary>
        /// <param name="context">Context http demande d'authentification</param>
        /// <param name="protocol">type de binding la requete est ajouté à l'url ou posté dans un formulaire</param>
        private void HandleResponse(HttpContext context, Uri protocol)
        {
            try
            {

                IEnumerable<eSaml2Attribute> attributes = GetAttributes(context, _saml2Configuration, protocol);

                // Cnversion des attribut en champ eudo
                IEnumerable<eSaml2Field> fields = Convert(attributes);

                UserToken cUserToken = BuildUserToken(fields);

                var keys = fields.Where(f => f.IsKey);
                if (keys.Count() == 0)
                {
                    _logger.Warn($"Pas de clé définit pour identifier l'utilisateur dans la base {_prefSQL.GetBaseName}", "IN");

                    throw new eSaml2Exception("Configuration incomplète ou incorrecte", $"Pas de clés définies pour identifier l'utilisateur. Merci de contacter votre administrateur.", $"Mapping incorrect");
                }

                _logger.Info($"Identification de l'utilisateur dans la base {_prefSQL.GetBaseName}", "INTERNAL");

                Flush(UpdateState(eResApp.GetRes(_iLang, 8877)));

                // Recuperer un eLoginOL
                eLoginOL login = eSaml2LoginOL.GetSaml2LoginObject(_prefSQL, _cSubscriberToken, cUserToken, keys);

                AUTH_USER_RES res = login.AuthUser(_cDbToken, false, true);
                
                if (res == AUTH_USER_RES.SUCCESS)
                {
                    _logger.Info($"Succès : utilisateur {keys.First().ColumnValue} trouvé en base", "INTERNAL");
                    login.SetSessionVars();

                    string appUrl = eLibTools.GetAppUrl(HttpContext.Current.Request).TrimEnd('/');

                    _logger.Info($"Redirection vers la page {appUrl}/eMain.aspx", "INTERNAL");

                    Flush(UpdateState(eResApp.GetRes(_iLang, 8878)));
                    Flush(UpdateState(eResApp.GetRes(_iLang, 8884)));
                    Flush(Redirect($"{appUrl}/eMain.aspx"));

                    //context.Response.Redirect($"{appUrl}/eMain.aspx", false);
                    context.ApplicationInstance.CompleteRequest();

                }
                else
                {
                    eudoDAL dal = eLibTools.GetEudoDAL(_prefSQL);

                    if (_databaseConfiguration.AutoCreateUser)
                    {
                        Flush(UpdateState(eResApp.GetRes(_iLang, 8879)));
                        //int existUserResult = 0;
                        
                        if (fields.Count() > 0)
                        {
                            //ALISTER => On ne gère pas le cas de multiple utilisateur. Le cas des utilisateurs désactivés est déjà géré.
                            //A décommenter si l'on veut modifier le comportement
                            //We don't handle the case where there is multiple users. Same as multiple user, we don't handle the case of disabled user, it seems it's already handled.
                            //Uncomment it if we want to change the behavior

                            /*existUserResult = UserExistsSAML(dal, fields.First().ColumnValue.ToString());
                            switch (existUserResult)
                            {
                                case 0:*/
                                    Flush(UpdateState(eResApp.GetRes(_iLang, 411) + $" {keys.First().ColumnValue} " + eResApp.GetRes(_iLang, 8880)));
                                    Flush(UpdateState(eResApp.GetRes(_iLang, 8886)));

                                    Dictionary<UserField, string> DicAddedValue = null;

                                    if (DicAddedValue == null)
                                        DicAddedValue = new Dictionary<UserField, string>();

                                    DicAddedValue[UserField.LOGIN] = fields.First().ColumnValue.ToString();
                                    DicAddedValue[UserField.EMAIL] = fields.First().ColumnValue.ToString();
                                    DicAddedValue[UserField.EMAIL_OTHER] = fields.First().ColumnValue.ToString();
                                    try
                                    {
                                        dal.OpenDatabase();
                                        eUserInfo userInfo = eLoginOLGeneric.CreateUser(dal, _prefSQL, fields.First().ColumnValue.ToString(), eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.ROOT), DicAddedValue, 0, eLibTools.GetToken(30));
                                        if (userInfo != null && userInfo.UserId > 0)
                                        {
                                            HandleResponse(context, protocol);
                                        }
                                    }
                                    finally
                                    {
                                        dal.CloseDatabase();
                                    }
                            /*        break;
                                case 1:
                                    throw new eSaml2Exception("Connexion à la base impossible", $"Plusieurs utilisateurs ont le même identifiant. Pour plus d'information, veuillez contacter votre administrateur", $"Connexion à la base echouée.");
                                    break;
                                case 2:
                                    throw new eSaml2Exception("Connexion à la base impossible", $"L'utilisateur est désactivé. Pour plus d'information, veuillez contacter votre administrateur", $"Connexion à la base echouée.");
                                    break;
                                default:
                                    Flush(UpdateState($"Identification échoué !"));

                                    _logger.Warn($"Utilisateur {keys.First().ColumnValue} non trouvé. {login.UserErrorMsg}", "INTERNAL");
                                    //TODORES
                                    throw new eSaml2Exception("Connexion à la base impossible", $"Utilisateur non reconnu. Pour plus d'information, veuillez contacter votre administrateur", $"Connexion à la base echouée.");

                                    break;
                            }*/
                        }
                        else
                        {
                            Flush(UpdateState(eResApp.GetRes(_iLang, 8885)));

                            _logger.Warn(eResApp.GetRes(_iLang, 411) + $" {keys.First().ColumnValue} " + eResApp.GetRes(_iLang, 8880) + $" {login.UserErrorMsg}", "INTERNAL");
                            //TODORES
                            throw new eSaml2Exception(eResApp.GetRes(_iLang, 8881), eResApp.GetRes(_iLang, 8882) + $" " + eResApp.GetRes(_iLang, 6721), eResApp.GetRes(_iLang, 8883));
                        }

                    }
                    else
                    {
                        Flush(UpdateState(eResApp.GetRes(_iLang, 8885)));
                        _logger.Warn(eResApp.GetRes(_iLang, 411) + $" {keys.First().ColumnValue} " + eResApp.GetRes(_iLang, 8880) + $" {login.UserErrorMsg}", "INTERNAL");
                        //TODORES
                        throw new eSaml2Exception(eResApp.GetRes(_iLang, 8881), eResApp.GetRes(_iLang, 8882) + $" " + eResApp.GetRes(_iLang, 6721), eResApp.GetRes(_iLang, 8883));
                    }
                }
            }
            catch (CryptographicException cex)
            {

                _logger.Error($"{cex.GetType().ToString()} Error au niveau décryptage de document", "IN");
                _logger.Error($"{cex.Message} \n {cex.StackTrace}", "IN");

                throw new eSaml2Exception("Impossible de décrypter le document !", $"Erreur : \"{cex.Message}\" <br />Merci de contacter votre administrateur", $"{cex.Message.Replace("\n", "<br />")} - { cex.StackTrace}");
            }
            catch (Saml2RequestException s)
            {
                _logger.Error($"{s.GetType().ToString()} Error au niveau document", "IN");
                _logger.Error($"{s.Message} \n {s.StackTrace}", "IN");

                throw new eSaml2Exception("Impossible de vérifier le document !", $"Erreur : \"{s.Message}\" <br />Merci de contacter votre administrateur", s.StackTrace);
            }
            catch (eSaml2Exception) { throw; }
            catch (Exception ex)
            {
                _logger.Error($"{ex.GetType().ToString()} Error lors de la construction de la réponse ", "IN");
                _logger.Error($"{ex.Message} \n {ex.StackTrace}", "IN");

                throw new eSaml2Exception("Une erreur est survenue ! ", ex.Message.Replace("\n", "<br />"), ex.StackTrace);
            }
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
                _logger.Info($"Mehode POST", "IN");

                // Post binding
                Saml2PostBinding binding = new Saml2PostBinding();
                binding.Unbind(request, saml2AuthnResponse);

                _logger.Info($"SamlResponse {saml2AuthnResponse.XmlDocument.InnerXml}", "IN");
            }
            else if (protocol.OriginalString.Equals(ProtocolBindings.HttpRedirect.OriginalString))
            {
                _logger.Info($"Methode REDIRECT", "IN");

                // Redirect binding
                Saml2RedirectBinding binding = new Saml2RedirectBinding();
                binding.Unbind(request, saml2AuthnResponse);

                _logger.Info($"SamlResponse {saml2AuthnResponse.XmlDocument.InnerXml}", "IN");
            }
            else
            {
                _logger.Info($"Mehode non supportée {protocol.OriginalString.ToString()}", "IN");

                //TODORES
                throw new eSaml2Exception("Opération non supportée", $"Bindings acceptés : {ProtocolBindings.HttpRedirect.OriginalString} et {ProtocolBindings.HttpPost.OriginalString}", $"Binding {protocol.OriginalString} non supporté");

            }

            // En cas d'echec de l'IDP on redirige sur la page d'erreur
            if (saml2AuthnResponse.Status != Saml2StatusCodes.Success)
            {
                _logger.Warn($"SamlResponse, Login echoué,  Status {saml2AuthnResponse.Status}", "IN");

                Flush(UpdateState("Authentification distante echouée !"));

                throw new eSaml2Exception("Impossible de se connecter à la base", $"Echec d'authentification depuis le service distant. Pour plus d'information, merci de contacter votre administratuer", $"Echec IDP");

            }

            Flush(UpdateState("Authentification distante réussie"));

            _logger.Info($"SamlResponse, Login réussi", "IN");

            // Attribut retournés par l'IDP
            List<eSaml2Attribute> attributes = new List<eSaml2Attribute>();
            foreach (Claim cl in saml2AuthnResponse.ClaimsIdentity.Claims)
            {
                attributes.Add(new eSaml2Attribute() { AttributeName = cl.Type, AttributeValue = cl.Value });
                _logger.Info($"SamlResponse, attributeName:{cl.Type}, attributeValue:{cl.Value}", "IN");
            }



            return attributes;
        }


        /// <summary>
        /// Les requêtes cross-domain, depuis les mises à jours sécuritaire de CHrome 80, bloque la récupération des cookies depuis le navigateurs
        /// apres un échange cross-domain. Ainsi apres l'allez-retour avec l'IDP la session est perdue.
        /// Pour corriger celà on paramètres le cookie de session en conséquence pour signale explicitement qu'on souhaite conserver la corresponance
        /// de cookie de session.
        /// #86 072
        /// </summary>
        /// <param name="context">Contexte Http de la réponse</param>
        private void SetSessionCookieForSameSiteStrategy(HttpContext context)
        {
            //On récupère le nom du cookie de session
            SessionStateSection sessionStateSection = (System.Web.Configuration.SessionStateSection)ConfigurationManager.GetSection("system.web/sessionState");

            string cookieName = sessionStateSection.CookieName;
            //on génère la chaine de paramètre permettant de set le cookie de session avec les attributs SameSite=None et Secure.
            string SamesiteParameter = String.Format("{0}={1}; path=/; Samesite={2}; Secure;", cookieName, context.Session.SessionID, "None");

            //Ajout dans les header du set Cookie
            context.Response.Headers.Add("set-cookie", SamesiteParameter);

        }

        /// <summary>
        /// Permet de vérifier si un utilisateur existe déjà.
        /// Allows to check if a user already exists.
        /// </summary>
        /// /// <param name="sExistedUser">Info sur l'utilisateur permettant de vérifier son existence</param>
        private int UserExistsSAML(eudoDAL dal, string sExistedUser)
        {
            string sSql = "SELECT [USERID], [USERDISABLED] FROM [USER] WHERE UserLogin = @USEREXIST OR UserMail = @USEREXIST OR UserMailOther = @USEREXIST";
            RqParam rq = new RqParam();
            rq.SetQuery(sSql);
            rq.AddInputParameter("@USEREXIST", SqlDbType.VarChar, sExistedUser);
            string sErrMsg;
            DataTableReaderTuned dtr = null;
            List<eUserInfo> userList = new List<eUserInfo>();
            List<bool> userDisabled = new List<bool>();
            try
            {
                dal.OpenDatabase();

                dtr = dal.Execute(rq, out sErrMsg);

                if (sErrMsg.Length > 0 || dtr == null)
                    throw new eSaml2Exception("Impossible de se connecter à la base", $"Echec d'authentification depuis le service distant. Pour plus d'information, merci de contacter votre administratuer", $"Echec IDP");


                //Vérification USER	
                if (string.IsNullOrEmpty(sErrMsg))
                {
                    while (dtr.Read())
                    {
                        userList.Add(new eUserInfo(dtr.GetEudoNumeric(0), dal, ""));
                    }

                    if(userList.Count > 1)
                    {
                        return 1;
                    }

                    for (int i = 0; i < userList.Count; i++)
                    {
                        if (userList[i].UserDisabled)
                        {
                            return 2;
                        }
                    }
                }
            }
            finally
            {
                dtr?.Dispose();

                if (dal != null)
                    dal.CloseDatabase();
            }

            return 0;
        }

        /// <summary>
        /// Réutilisable
        /// </summary>
        public bool IsReusable { get { return true; } }

        public object SignedXml { get; private set; }
    }
}