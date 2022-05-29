using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using System.Xml;
//using EudoQuery;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.sso;
using EudoQuery;
using Microsoft.IdentityModel.Protocols.WSFederation;
using Microsoft.IdentityModel.Web;
using Com.Eudonet.Internal.counters;
using System.Data.SqlClient;
using Com.Eudonet.Common.Cryptography;

namespace Com.Eudonet.Xrm
{
    /// <className>eLoginMgr</className>
    /// <summary>Manager pour la gestion des connexions</summary>
    /// <purpose>Authentification abonné, utilisateur et liste user et database</purpose>
    /// <authors>JBE</authors>
    /// <date>2011-MM-JJ</date>
    public class eLoginMgr : eEudoManager
    {
        String _langue = String.Empty;
        Int32 _iLang = 0;
        Boolean _bIsSSOApp = false;
        Boolean _bIsADFS = false;
        Boolean _bIsIntranet = false;
        Boolean _bRememberInfo = false;

        String _sLocalURL = String.Empty;

        //DbToken _cDbToken;

        /// <summary>
        /// Chargement de la page
        /// </summary>
        protected override void ProcessManager()
        {


            if (eLibTools.GetServerConfig("DOSENABLED") == "1")
            {
                Int32 nLimit;
                if (!Int32.TryParse(eLibTools.GetServerConfig("DOSLIMIT", eLibConst.DOS_DEFAULT_LIMIT.ToString()), out nLimit) || nLimit <= 0)
                    nLimit = eLibConst.DOS_DEFAULT_LIMIT;

                try
                {
                    if (eDoSProtection.CheckDDOS(new WCFCall("LOGIN_MGR", nLimit), true) <= 0)
                    {
                        GetReturnXML(false, "userauthentication", "authuser", "", "");
                        return;
                    }
                }
                catch (eEudoDoSException)
                {
                    // Protection déclenché, on return
                    GetReturnXML(false, "userauthentication", "authuser", "", "");
                    return;

                }
                catch (eEudoDosConcurentException)
                {
                    //access concurent, on laisse passer

                }
                catch (Exception)
                {
                    // le systeme de protection ne doit pas être source d'erreur
                }
            }


            bool bFromXrm = _context.Session["CHECKXRM"] != null;
            bool bFromEudoAdmin = !bFromXrm && _context.Request.Headers["X-FROMEUDOADMIN"] != null
                && _context.Request.Form["rememberme"] != null
                && _context.Request.Form["rememberme"].ToString().Length == 16
                ;

            if (!bFromXrm && !bFromEudoAdmin)
            {
                //   throw new eEndResponseException();
            }


            DbToken cDbToken = new DbToken();
            SubscriberToken cSubscriberToken = new SubscriberToken();
            UserToken cUserToken = new UserToken();

            String sRedirectURL = String.Empty;
            Boolean bRedirect = false;

            eConst.LOGIN_ACTION eAction = eConst.LOGIN_ACTION.UNDEFINED;

            #region Action demandée

            if (!_requestTools.AllKeysQS.Contains("action"))
                return;

            String sAction = _context.Request.QueryString["action"].ToString();
            Enum.TryParse<eConst.LOGIN_ACTION>(sAction.ToUpper(), out eAction);

            if (eAction == eConst.LOGIN_ACTION.UNDEFINED)
            {
                //retourner un flux d'erreur
                return;
            }

            #endregion

            #region autres paramètres commun

            //Base URL du manager
            _sLocalURL = String.Concat(_context.Request.Url.Scheme, "://", _context.Request.Url.Authority, _context.Request.ApplicationPath.TrimEnd('/'), "/");


            if (_context.Request.Form["rememberme"] != null)
            {
                _bRememberInfo = _context.Request.Form["rememberme"].ToString() == "1";
                eTools.SaveCookie("rememberme", _bRememberInfo ? "1" : "0", DateTime.MaxValue, _context.Response, false);
            }
            else
                _bRememberInfo = eTools.GetCookie("rememberme", _context.Request, false) == "1";

            //Vide les cookies si mémorisation désactivé
            if (!_bRememberInfo)
                ClearCookies();

            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings.Get("IntranetApplication")))
                _bIsIntranet = (ConfigurationManager.AppSettings.Get("IntranetApplication") == "1");


            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings.Get("SSOApplication")))
                _bIsSSOApp = (ConfigurationManager.AppSettings.Get("SSOApplication") == "1");



            _bIsADFS = eLibTools.GetServerConfig("ADFSApplication", "0") == "1";

            // Ressources et langue
            _langue = eTools.GetCookie("langue", _context.Request);
            eLibTools.GetLangFromUserPref(_langue, out _langue, out _iLang);

            #endregion

            #region Gestion des Actions

            try
            {
                switch (eAction)
                {
                    case eConst.LOGIN_ACTION.UNLOAD:

                        //On se contente de loger l'heure mais on ne ferme pas la session
                        //en effet on ne peut pas différencier un reload d''une vrai déco

                        eLoginOL.LogLogout();

                        break;

                    case eConst.LOGIN_ACTION.GETUSERLIST:

                        #region Liste User
                        if (!_requestTools.AllKeys.Contains("db"))
                            throw new Exception("Token de base non fourni.");

                        if (!_requestTools.AllKeys.Contains("st"))
                            throw new Exception("Token abonné non fourni.");


                        cSubscriberToken.LoadTokenCrypted(_context.Request.Form["st"].ToString());
                        cDbToken.LoadTokenCrypted(_context.Request.Form["db"].ToString());

                        RenderResult(RequestContentType.XML,
                            delegate () { return GetUserList(cSubscriberToken, cDbToken).OuterXml; });
                        break;
                    #endregion

                    case eConst.LOGIN_ACTION.SSOSASPARAM:

                        #region SSO SAS Actif Retourne les informations du SSO SAS, s'il est actif et si oui les informations de ce SSO
                        if (!_requestTools.AllKeys.Contains("db"))
                            throw new Exception("Token de base non fourni.");

                        if (!_requestTools.AllKeys.Contains("st"))
                            throw new Exception("Token abonné non fourni.");


                        cSubscriberToken.LoadTokenCrypted(_context.Request.Form["st"].ToString());
                        cDbToken.LoadTokenCrypted(_context.Request.Form["db"].ToString());



                        RenderResult(RequestContentType.XML,
                            delegate ()
                            {
                                return GetSSOParam(cSubscriberToken, cDbToken).OuterXml;
                            }
                            );
                        break;
                    #endregion

                    case eConst.LOGIN_ACTION.AUTHSUBSCRIBER:

                        #region Authentification Abonné

                        if (_bIsIntranet)
                        {
                            cSubscriberToken.Login = eLibConst.DEFAULT_USERNAME;
                            cSubscriberToken.Pwd = eLibConst.DEFAULT_USERNAME;
                        }
                        else
                        {

                            if (!_requestTools.AllKeys.Contains("SubscriberLogin") || !_requestTools.AllKeys.Contains("SubscriberPassword"))
                                throw (new Exception("Paramètres de login abonné non disponible"));


                            cSubscriberToken.Login = _context.Request.Form["SubscriberLogin"].ToString();
                            cSubscriberToken.Pwd = _context.Request.Form["SubscriberPassword"].ToString();
                        }


                        RenderResult(RequestContentType.XML, delegate () { return AuthSubscribers(cSubscriberToken, eAction).OuterXml; });

                        break;

                    #endregion

                    case eConst.LOGIN_ACTION.AUTHUSER:
                    case eConst.LOGIN_ACTION.FROMREDIRECTION:
                    case eConst.LOGIN_ACTION.AUTHCAS:	//Authentification eudonet avec les infos SSO de type SAS

                        #region Authentification Utilisateur

                        //Récupération des tokens     
                        if (!LoadToken(cDbToken, cSubscriberToken, eAction))
                        {
                            eTools.SaveCookie("UserLoginXrm", String.Empty, DateTime.MaxValue, _context.Response);
                            throw (new Exception("#Erreur : token invalides "));
                        }

                        /* Vérification des redirection */
                        // Gestion de la redirection            
                        sRedirectURL = String.Concat(cDbToken.AppUrl.TrimEnd('/'), '/');

                        //Redirection si url de redirection (appurl) non vide et différente de url en cours
                        bRedirect = cDbToken.RedirectEnabled && cDbToken.AppUrl.Length != 0 && !_sLocalURL.ToLower().Equals(sRedirectURL.ToLower());

                        //GCH - Important : définition de Login SSO avant authCAS car définit pour le cas juste en dessous 
                        if (_context.Request.ServerVariables["LOGON_USER"] != null)
                            cUserToken.SSOLogin = _context.Request.ServerVariables["LOGON_USER"].ToString();

                        //Si une authentification depuis un SSO est demandée, on récupère les paramètre du SSO
                        //	Définit le userlogin avec le login remonté par le SSO
                        if (eAction == eConst.LOGIN_ACTION.AUTHCAS)
                        {
                            String sError = String.Empty;
                            //Si authentification SSO CAS
                            eSSO_CAS sso = new eSSO_CAS(_pref, cDbToken, _context.Request);
                            if (!sso.Init(out sError))
                                throw (new Exception(String.Concat("#Erreur : sso init en erreur : ", sError)));

                            String sReturn;
                            if (!sso.GetUserLogin_SSOInformation(_context.Request.Form["casticket"] as string, out sReturn))
                            {
                                //Erreur d'authentification au niveau du SSO
                                RenderResult(RequestContentType.XML, delegate ()
                                {
                                    return GetReturnXML(false, "userauthentication", "authuser",
                                      eResApp.GetRes(_iLang, 72),
                                        /*eResApp.GetRes(_iLang, 5) + "<!--" + */sReturn /*+ "-->"*/, "INVALID_CAS_TICKECT"
                                      ).OuterXml;
                                });
                            }

                            cUserToken.SSOLogin = sReturn;
                        }
                        else if (eAction == eConst.LOGIN_ACTION.AUTHUSER)
                        {


                            if (_context.Session["SIVUT"] != null && _context.Request.Form["ut"] != null && cUserToken.LoadTokenCrypted(_context.Request.Form["ut"], _context.Session["SIVUT"].ToString()))
                            {
                                // IDentification via token - Pour cas de redirection avec changement de pwd
                            }
                            else
                            {
                                //Si authentification utilisateur
                                cUserToken.Login = _context.Request.Form["UserLogin"].ToString().ToUpper();

                                //JAS #40069 : retrait du toUpper qui corromp le mot de passe lors d'une vérification LDAP.
                                cUserToken.Pwd = _context.Request.Form["UserPassword"].ToString();//.ToUpper();
                            }


                        }
                        //Si redirection
                        else
                        {
                            string userToken = _context.Request.Form["ut"].ToString();
                            cUserToken.LoadTokenCrypted(userToken);
                        }


                        //Affectation de la langue choisie
                        cUserToken.Lang = _langue;

                        if (_bIsADFS)
                        {

                            cUserToken.SSOLogin = eTools.GetEudoExternalLoginFromExternalAuth();

                        }

                        if (bRedirect)
                        {
                            //IP ORIGINAL
                            String strRemoteAdr = eLibTools.GetUserIPV4();
                            RenderResult(RequestContentType.XML, delegate () { return AuthUserDistant(cSubscriberToken, cDbToken, cUserToken).OuterXml; });
                        }
                        else
                            RenderResult(RequestContentType.XML, delegate () { return AuthUser(cSubscriberToken, cDbToken, cUserToken, eAction).OuterXml; });

                        break;

                    #endregion

                    //quickLog - TODO
                    case eConst.LOGIN_ACTION.QUICKLOG:
                        {
                            break;
                        }

                    case eConst.LOGIN_ACTION.UNLOADADFS:
                        eLoginOL.LogLogout();
                        if (_bIsADFS)
                        {

                            try
                            {
                                //Vide les token/cookie
                                FederatedAuthentication.SessionAuthenticationModule.SignOut(); FederatedAuthentication.SessionAuthenticationModule.DeleteSessionTokenCookie(); FederatedAuthentication.SessionAuthenticationModule.CookieHandler.Delete();

                                //récupèere l'url de logout
                                WSFederationAuthenticationModule fedAuthenticationModule = FederatedAuthentication.WSFederationAuthenticationModule;
                                fedAuthenticationModule.SignOut(false);
                                SignOutRequestMessage signOutRequestMessage = new SignOutRequestMessage(new Uri(fedAuthenticationModule.Issuer), fedAuthenticationModule.Realm);


                                string queryString = signOutRequestMessage.WriteQueryString();
                                RenderResult(RequestContentType.TEXT, delegate () { return queryString; });

                            }
                            catch (eEndResponseException) { }
                            catch (ThreadAbortException) { }
                            catch (Exception)
                            {
                                //déconnexion impoissible
                                RenderResult(RequestContentType.TEXT, delegate () { return "0"; });
                            }



                        }

                        break;

                    case eConst.LOGIN_ACTION.FORGOTPASSWORD:

                        if (!LoadToken(cDbToken, cSubscriberToken, eAction))
                        {
                            eTools.SaveCookie("UserLoginXrm", String.Empty, DateTime.MaxValue, _context.Response);
                            throw (new Exception("#Erreur : token invalides "));
                        }

                        RenderResult(RequestContentType.XML, delegate () { return ForgotPassword(cSubscriberToken, cDbToken).OuterXml; });


                        break;

                    case eConst.LOGIN_ACTION.CHANGEPASSWORD:

                        #region Changement de mot de passe

                        String newPassword = null;
                        if (_requestTools.AllKeys.Contains("txtNewPassword"))
                            newPassword = _context.Request.Form["txtNewPassword"].ToString();

                        //SPH 2020 - 14/05/2020
                        // TODO : Il faut rapidement revoir le fonctionnement ci-après
                        // il est extrêmement dangereux de ce baser sur ce qui est décrit pour des aspects de sécurité.
                        // En l'état, il sufit de retire du dom le champ "ancien" password pour by passer la vérification...
                        //-----------------
                        // Si l'ancien mot de passe n'a pas été passé au manager, on considère qu'il n'a pas été demandé sur l'interface
                        // (champ de saisie absent). Dans ce cas, il sera à null et la méthode ChangePassword saura qu'il ne faut pas le
                        // vérifier. Si le champ est présent mais non rempli par l'utilisateur, la chaîne sera déclarée, mais vide, ce qui
                        // permettra de faire la différence
                        String oldPassword = null;
                        if (_requestTools.AllKeys.Contains("txtOldPassword"))
                            oldPassword = _context.Request.Form["txtOldPassword"].ToString();




                        RenderResult(RequestContentType.XML, delegate () { return ChangePassword(oldPassword, newPassword).OuterXml; });

                        break;

                    #endregion

                    //Autre
                    default:
                        {
                            break;
                        }
                }
            }
            catch (eEndResponseException) { }
            catch (ThreadAbortException) { }
            catch (XRMUpgradeException x)
            {
                LoginTraceLog(x.ToString());
                RenderResult(RequestContentType.XML, delegate () { return GetReturnXML(false, sAction, String.Empty, String.Empty, eResApp.GetRes(_iLang, 6754), String.Empty).OuterXml; });

            }
            catch (Exception e)
            {

                LoginTraceLog(e.ToString());
                RenderResult(RequestContentType.XML, delegate () { return GetReturnXML(false, sAction, String.Empty, String.Empty, eResApp.GetRes(_iLang, 72), String.Empty).OuterXml; });
            }

            #endregion
        }

        /// <summary>
        /// Changement de mot de passe après validation de différentes stratégies
        /// </summary>
        /// <param name="txtOldPassword">ancien mot de passe, si demandé et précisé sur l'interface (null si non demandé)</param>
        /// <param name="txtNewPassword">nouveau mot de passe</param>
        /// <returns>Document xml résultant</returns>
        public XmlDocument ChangePassword(String txtOldPassword, String txtNewPassword)
        {
            #region validation de nouveau mot de passe

            ePref pref = (ePref)_context.Session["Pref"];
            pref.ResetTranDal();


            int nUserId = _requestTools.GetRequestFormKeyI("userid") ?? pref.UserId;
            if (pref.UserId != nUserId && pref.User.UserLevel < (int)UserLevel.LEV_USR_ADMIN)
                throw new EudoAdminInvalidRightException();

            ePasswordManager passwordManager = new ePasswordManager(pref, txtOldPassword, txtNewPassword, nUserId);

            bool bMustChange = false;
            bool bNeverExpire = false;

            //seul les admin peuvent redéfinir ces options
            if (pref.User.UserLevel >= (int)UserLevel.LEV_USR_ADMIN)
            {
                bMustChange = _requestTools.GetRequestFormKeyB("changepwd") ?? false;
                bNeverExpire = _requestTools.GetRequestFormKeyB("noexpire") ?? false;
            }

            //A vérifier ! le never expire doit-il vraiment bypasser la stratégie de mdp ??
            Boolean bSuccess = bNeverExpire || passwordManager.IsValidPassword();

            // 6726 - Mot de passe incorrect
            String title = (bSuccess) ? eResApp.GetRes(pref, 7960) : eResApp.GetRes(pref, 6726);
            if (bSuccess)
            {
                bSuccess = passwordManager.Save(bNeverExpire, bMustChange);
                if (bSuccess)
                {
                    // 6727 - Votre nouveau mot de passe est correct
                    if (nUserId == pref.UserId)
                        title = eResApp.GetRes(pref, 6727);
                    else
                        title = eResApp.GetRes(pref, 7961);
                }
                else
                {
                    title = eResApp.GetRes(pref, 6237);
                }
            }

            // Message après la validation
            String message = passwordManager.GetUserMessage();

            // Connexion standard
            String sRedirUrl = "eMain.aspx";

            #endregion

            #region Rendu XML

            // Préparation du flux xml de retour
            XmlDocument xmlResult = GetXmlReturn();

            XmlNode xDetailsNode = xmlResult.CreateElement("changepassword");
            xmlResult.AppendChild(xDetailsNode);

            // Renvoi du résultat
            XmlNode xResultNode = xmlResult.CreateElement("result");
            xResultNode.InnerText = bSuccess ? "SUCCESS" : "FAILURE";
            xDetailsNode.AppendChild(xResultNode);

            XmlNode titleNode = xmlResult.CreateElement("title");
            titleNode.InnerText = title;
            xDetailsNode.AppendChild(titleNode);

            XmlNode msgNode = xmlResult.CreateElement("msg");
            msgNode.InnerText = message;
            xDetailsNode.AppendChild(msgNode);

            XmlNode urlNode = xmlResult.CreateElement("url");
            urlNode.InnerText = sRedirUrl;
            xDetailsNode.AppendChild(urlNode);

            return xmlResult;

            #endregion
        }

        /// <summary>
        /// Gestion du retour du formulaire de mot de passe oublié
        ///  - envoie du mail
        ///  - message erreur (pas le bon login/addresse mail
        /// </summary>
        /// <param name="cSubscriberToken">Token abonné</param>
        /// <param name="cDbToken">Token base de données</param>
        /// <returns>Flux XML de résultat, analysé par XRM pour redirection/affichage d'un message</returns>
        public XmlDocument ForgotPassword(SubscriberToken cSubscriberToken, DbToken cDbToken)
        {
            #region Initialisation des paramètres

            Boolean bFromRedirect = false;
            bFromRedirect = _context.Request.Headers.AllKeys.Contains("X-FROMREDIRECT") && _context.Request.Headers["X-FROMREDIRECT"] == "1";

            String sLogin = String.Empty;
            String sEmail = String.Empty;

            // Login
            if (_requestTools.AllKeys.Contains("UserLogin") && _context.Request.Form["UserLogin"].ToString().Trim().Length > 0)
                sLogin = _context.Request.Form["UserLogin"].ToString();
            else
                return GetReturnXML(false, "forgotpassword", "mail", String.Empty, eResApp.GetRes(_iLang, 516).Replace("<EMAIL> ", String.Empty));


            // Email
            if (_requestTools.AllKeys.Contains("UserEmail") && _context.Request.Form["UserEmail"].ToString().Trim().Length > 0)
                sEmail = _context.Request.Form["UserEmail"].ToString();
            else
                return GetReturnXML(false, "forgotpassword", "mail", String.Empty, eResApp.GetRes(_iLang, 6252).Replace("<EMAIL> ", String.Empty));

            // Si connexion directe, pas de captcha
            if (!bFromRedirect)
            {
                //Test captcha
                if (_context.Request.Form["captcha"] == null || _context.Request.Form["captcha"].ToString().Trim().Length == 0)
                    return GetReturnXML(false, "forgotpassword", "captcha", String.Empty, eResApp.GetRes(_iLang, 6223));

                if (!_context.Request.Form["captcha"].ToString().Trim().Equals(_context.Session["Captcha"].ToString()))
                    return GetReturnXML(false, "forgotpassword", "captcha", String.Empty, eResApp.GetRes(_iLang, 6223));
            }

            /* Vérification des redirection */
            // Gestion de la redirection            
            String sRedirectURL = String.Concat(cDbToken.AppUrl.TrimEnd('/'), '/');

            // Redirection si url de redirection (appurl) non vide et différente de url en cours
            Boolean bRedirect = cDbToken.RedirectEnabled && cDbToken.AppUrl.Length != 0 && _sLocalURL.ToLower().CompareTo(sRedirectURL.ToLower()) != 0;

            #endregion

            if (!bRedirect)
            {
                #region Connexion direct




                UserToken ut = new UserToken();
                ut.Lang = "LANG_00";

                string lang = EudoCommonHelper.EudoHelpers.GetCookie("langue");

                if (!string.IsNullOrEmpty(lang))
                {
                    int nUserLang;
                    EudoCommonHelper.EudoHelpers.GetLangFromUserPref(lang, out lang, out nUserLang);
                    ut.Lang = lang;
                    ut.LangId = nUserLang;
                    
                }



                eLoginOL login = eLoginOL.GetLoginObject(cSubscriberToken, ut);

                String sError = String.Empty;

                try
                {
                    //Vérification du couple login/mail
                    if (!login.ValidUserInfos(cDbToken, sLogin, sEmail, out sError))
                        return GetReturnXML(false, "forgotpassword", "mail", sError, String.Empty);

                    String sIpClient = eLibTools.GetUserIPV4();
                    //Envoi du mail
                    if (!login.SendConfirmationMail(cDbToken, sLogin, sEmail, sIpClient, out sError))
                    {
                        return GetReturnXML(false, "forgotpassword", "mail", sError, String.Empty);
                    }

                    //Comptabilisation de l'envoi de mail dans la table des compteurs
                    eudoDAL dalEudo = null;
                    try
                    {
                        ePrefSQL pref = eLoginOL.GetBasePrefSQL();
                        dalEudo = new eudoDAL(cDbToken.SqlServerInstanceName, cDbToken.DbDirectory, pref.GetSqlUser, pref.GetSqlPassword, pref.GetSqlApplicationName);
                        dalEudo.OpenDatabase();

                        RqParam query = null;
                        eEdnMsgCmpt.IncrementCounter(dalEudo, eEdnMsgCmpt.CounterType.FORGOTPASSWORD, DateTime.Now, out query, 1, true, false); // chiffrage toujours activé pour ce compteur
                    }
                    catch
                    {
                        throw;
                    }
                    finally
                    {
                        if (dalEudo != null)
                            dalEudo.CloseDatabase();
                    }
                }
                catch (EudoException ee)
                {
                    LoginTraceLog("FORGOTPASSWORD");
                    LoginTraceLog(ee.Message);
                    LoginTraceLog(ee.StackTrace);
                    return GetReturnXML(false, "forgotpassword", "mail", eResApp.GetRes(_iLang, 72), ee.UserMessage);
                }
                catch (Exception e)
                {
                    LoginTraceLog("FORGOTPASSWORD");
                    LoginTraceLog(e.Message);
                    LoginTraceLog(e.StackTrace);
                    return GetReturnXML(false, "forgotpassword", "mail", eResApp.GetRes(_iLang, 72), String.Empty);
                }

                //Préparation du flux xml de retour
                XmlDocument xmlResult = GetXmlReturn();

                XmlNode xDetailsNode = xmlResult.CreateElement("forgotpassword");
                xmlResult.AppendChild(xDetailsNode);

                // Renvoi du résultat
                XmlNode xResultNode = xmlResult.CreateElement("result");
                xResultNode.InnerText = "SUCCESS";
                xDetailsNode.AppendChild(xResultNode);

                XmlNode msgNode = xmlResult.CreateElement("msg");
                //NHA : US#1005 :BL#1735:En erreur comme en succès afficher le meme message
                msgNode.InnerText = eResApp.GetRes(_iLang, 6252);
                xDetailsNode.AppendChild(msgNode);

                return xmlResult;

                #endregion
            }
            else
            {
                #region Appel via redirection

                String sOrigIP = eLibTools.GetUserIPV4();

                //Boucle de redirection
                if (bFromRedirect)
                    throw new Exception("Boucle de redirection.");

                UserToken cUserToken = new UserToken();
                cUserToken.Lang = _langue;

                // Construction des paramètres à poster
                StringBuilder postData = new StringBuilder()
                    .Append(eLoginOLGeneric.GetQSToken(cSubscriberToken, cDbToken, cUserToken))
                    .Append("&UserLogin=").Append(sLogin)
                    .Append("&UserEmail=").Append(sEmail)
                    .Append("&OrigIP=").Append(sOrigIP);


                if (cDbToken.Version.ToLower().StartsWith("v7"))
                    sRedirectURL = String.Concat(sRedirectURL, "dotnet/");

                sRedirectURL = String.Concat(sRedirectURL, "mgr/eLoginMgr.ashx?action=forgotpassword");

                /***********************/
                /* Envoi de la requête */
                /***********************/
                StreamWriter writer = null;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(sRedirectURL);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = postData.ToString().Length;

                //AJoute un header de redirection pour éviter les boucles de redirections
                request.Headers.Add("X-FROMREDIRECT", "1");

                //Ajoute l'ip du demandeur initial
                request.Headers.Add("X-FROMIP", eLibTools.GetUserIPV4());

                try
                {
                    writer = new StreamWriter(request.GetRequestStream());
                    writer.Write(postData.ToString());
                }
                finally
                {
                    if (writer != null)
                        writer.Close();
                }

                // Lecture de la réponse et renvoie du flux
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader stream = new StreamReader(response.GetResponseStream());
                XmlDocument remoteXMLResult = new XmlDocument();
                remoteXMLResult.LoadXml(stream.ReadToEnd());

                return remoteXMLResult;

                #endregion
            }
        }

        /// <summary>
        /// Charge les token Abonné et base de donnée a partir de Request.Form
        /// </summary>
        /// <param name="cDbToken"></param>
        /// <param name="cSubscriberToken"></param>
        /// <param name="ec">Type d'action demandé</param>
        /// <returns></returns>
        private Boolean LoadToken(DbToken cDbToken, SubscriberToken cSubscriberToken, eConst.LOGIN_ACTION ec)
        {
            String sIV = String.Empty;
            Int32 nTExpire = 0;

            bool fromEudoAdmin = _context.Request.Headers.AllKeys.Contains("X-FROMEUDOADMIN");

            if (fromEudoAdmin)
            {
                sIV = _context.Request.Form["rememberme"].ToString();
                nTExpire = 5;
            }
            else if (_context.Session["SIV"] != null)
            {
                sIV = _context.Session["SIV"].ToString();
            }

            //Récupération des tokens   

            /* Token base de donnée */
            String databaseToken = _context.Request.Form["dbt"].ToString();


            /* Token Subscribers */
            String sSubscribersToken = _context.Request.Form["st"].ToString();


            return cDbToken.LoadTokenCrypted(databaseToken, sIV, nTExpire) && cSubscriberToken.LoadTokenCrypted(sSubscribersToken, sIV, nTExpire, fromEudoAdmin);


        }

        /// <summary>
        /// Vide tous les cookies de l'appli
        /// </summary>
        private void ClearCookies()
        {
            //Token Subscribers
            eTools.SaveCookie("ts", String.Empty, DateTime.MaxValue, _context.Response, false);

            eTools.SaveCookie("db", String.Empty, DateTime.MaxValue, _context.Response);

            //Token User
            eTools.SaveCookie("UserLoginXrm", String.Empty, DateTime.MaxValue, _context.Response);
        }

        /// <summary>
        /// Identifie un abonné et retourne le flux XML des bases autorisées pour cet utilisateur
        /// </summary>
        /// <param name="cSubscriberToken">Objet encapuslant les informations subscribers</param>
        /// <param name="eAction">Type d'action</param>
        /// <returns>Flux XML de résultat, analysé par XRM pour redirection/affichage d'un message</returns>
        public XmlDocument AuthSubscribers(SubscriberToken cSubscriberToken, eConst.LOGIN_ACTION eAction)
        {

            //Préparation du flux xml de retour
            XmlDocument xmlResult = GetXmlReturn();

            XmlNode xDetailsNode = xmlResult.CreateElement("subscriberauthentication");
            xmlResult.AppendChild(xDetailsNode);

            try
            {

                //Token pour fournir la langue du user
                UserToken ut = new UserToken();
                ut.Lang = _langue;

                eLoginOL login = eLoginOL.GetLoginObject(cSubscriberToken, ut);


                //Test sur les démande provenant d'une redirection : l'ip d'origine doit être spécifié
                Boolean bFromRedirect = _context.Request.Headers.AllKeys.Contains("X-FROMREDIRECT") && _context.Request.Headers["X-FROMREDIRECT"] == "1";
                Boolean ss = _requestTools.AllKeys.Contains("OrigIP");

                String _err = string.Empty;

                //Authentification subscriber KO
                if (!login.AuthSubscribers())
                {


                    if (VerifMaxConnectionNumber(eAction))
                    {

                        // ClearCookies();
                        return GetReturnXML(false, "subscriberauthentication", "AuthSubscriber", String.Empty, eResApp.GetRes(_iLang, 692), "NBR_MAX_CONN");
                    }
                    else
                        return GetReturnXML(false, "subscriberauthentication", "AuthSubscriber", String.Empty, eResApp.GetRes(_iLang, 6210), "INVALID_SUB");

                }
                else
                {

                    String sBaseUID = String.Empty;
                    if (_bRememberInfo)
                    {
                        eTools.SaveCookie("ts", cSubscriberToken.GetTokenCrypted(), DateTime.MaxValue, _context.Response, false);
                        sBaseUID = eTools.GetCookie("db", _context.Request);
                    }



                    // récupère la liste des bases autorisée
                    List<DbTokenGeneric> lst = login.GetAllowedDB();

                    //Si erreur de getallowedDB, lève une exception
                    if (login.ErrMsg.Length > 0)
                    {
                        if (login.InnerException != null)
                            throw login.InnerException;
                        else
                            throw new Exception(login.ErrMsg);
                    }


                    //Liste des bases
                    XmlNode xMainDbNode = xmlResult.CreateElement("databases");
                    xDetailsNode.AppendChild(xMainDbNode);

                    //Boucle sur toutes les bases pour les ajouter dans le flux
                    Boolean bIsSel = false;
                    bool bExistV7 = false;
                    foreach (DbToken db in lst)
                    {
                        //Ajout au xml
                        XmlNode xDbNode = xmlResult.CreateElement("db");
                        xMainDbNode.AppendChild(xDbNode);

                        XmlNode xDbInfosNode = xmlResult.CreateElement("dbtoken");
                        xDbInfosNode.InnerText = db.GetTokenCrypted();
                        xDbNode.AppendChild(xDbInfosNode);

                        XmlNode xDbNameNode = xmlResult.CreateElement("longname");
                        xDbNameNode.InnerText = db.DbName;
                        xDbNode.AppendChild(xDbNameNode);

                        XmlNode xDbUserListNode = xmlResult.CreateElement("userlistenabled");
                        xDbUserListNode.InnerText = (db.UserListEnabled ? "1" : "0");
                        xDbNode.AppendChild(xDbUserListNode);

                        XmlNode xDbAppUrlListNode = xmlResult.CreateElement("appurl");
                        xDbAppUrlListNode.InnerText = (db.AppUrl.Length > 0) ? "1" : "0";
                        xDbNode.AppendChild(xDbAppUrlListNode);

                        //SSO en Intranet
                        XmlNode xDbSsoNode = xmlResult.CreateElement("ssoenabled");
                        xDbSsoNode.InnerText = ((db.IsSSOEnabled && _bIsSSOApp && _bIsIntranet) ? "1" : "0");
                        xDbNode.AppendChild(xDbSsoNode);

                        //SSO en SAS (pour le CAS principalement)
                        XmlNode xDbSsoSasNode = xmlResult.CreateElement("SSOSASEnabled");
                        xDbSsoSasNode.InnerText = ((db.IsSSOEnabled && !_bIsSSOApp) ? "1" : "0");
                        xDbNode.AppendChild(xDbSsoSasNode);

                        // POur afficher le bloque de connexion utilisateur ou le masquer
                        XmlNode xDbModeAuthnNode = xmlResult.CreateElement("AuthnMode");
                        xDbModeAuthnNode.InnerText = GetClientMode(db.AuthenticationMode);
                        xDbNode.AppendChild(xDbModeAuthnNode);


                        if (!bIsSel && sBaseUID == db.BaseUid)
                        {
                            bIsSel = true;
                            XmlNode xDbSelected = xmlResult.CreateElement("isselected");
                            xDbSelected.InnerText = "1";
                            xDbNode.AppendChild(xDbSelected);
                        }

                        XmlNode xV7Node = xmlResult.CreateElement("v7");
                        xV7Node.InnerText = db.Version.ToLower().StartsWith("v7") ? "1" : "0";
                        xDbNode.AppendChild(xV7Node);

                        if (db.Version.ToLower().StartsWith("v7"))
                            bExistV7 = true;

                    }

                    if (bExistV7)
                    {
                        XmlNode xV7URL = xmlResult.CreateElement("V7URL");
                        xV7URL.InnerText = eModelTools.GetBaseUrlV7();
                        xDetailsNode.AppendChild(xV7URL);
                    }

                    XmlNode xResultNode = xmlResult.CreateElement("result");
                    xResultNode.InnerText = "SUCCESS";
                    xDetailsNode.AppendChild(xResultNode);

                    XmlNode xErrDesc = xmlResult.CreateElement("errordescription");
                    xErrDesc.InnerText = eResApp.GetRes(_iLang, 1712);
                    xDetailsNode.AppendChild(xErrDesc);

                    XmlNode xTokenNode = xmlResult.CreateElement("subscribertoken");
                    xTokenNode.InnerText = cSubscriberToken.GetTokenCrypted();
                    xDetailsNode.AppendChild(xTokenNode);

                    XmlNode _ssoNode = xmlResult.CreateElement("applicationsso");
                    _ssoNode.InnerText = (_bIsSSOApp ? "1" : "0");
                    xDetailsNode.AppendChild(_ssoNode);

                }


            }
            catch (Exception e1)
            {
                XmlNode resultNode = xmlResult.CreateElement("result");
                resultNode.InnerText = "FAILURE";
                xDetailsNode.AppendChild(resultNode);

                XmlNode tokentNode = xmlResult.CreateElement("token");
                xDetailsNode.AppendChild(resultNode);

                XmlNode errDesc = xmlResult.CreateElement("errordescription");

                if (_requestTools.IsLocalOrEudo)
                    errDesc.InnerText = String.Concat(e1.Message, Environment.NewLine, e1.StackTrace);

                xDetailsNode.AppendChild(errDesc);


            }


            return xmlResult;
        }

        /// <summary>
        /// Mode de connexion compris par le js
        /// </summary>
        /// <param name="authenticationMode"></param>
        /// <returns></returns>
        private string GetClientMode(eLibConst.AuthenticationMode authenticationMode)
        {
            switch (authenticationMode)
            {
                case eLibConst.AuthenticationMode.SAML2: return "saml";
                default: return "form";
            }
        }

        /// <summary>
        /// Retourne les informations de SSO de type CAS
        /// </summary>
        /// <param name="cSubscriberToken">Information d'authentification de l'abonné</param>
        /// <param name="cDbToken">Information d'authentification de la base de donnée sélectionnée</param>
        public XmlDocument GetSSOParam(SubscriberToken cSubscriberToken, DbToken cDbToken)
        {
            Boolean bSuccess = true;
            String sError = String.Empty;


            XmlDocument xmlResult = GetXmlReturn();

            XmlNode xDetailsNode = xmlResult.CreateElement("sso");
            xmlResult.AppendChild(xDetailsNode);

            if (IsRedirect(cDbToken))
            {
                String sXmlNode = String.Empty;
                if ((!GetRedirectNode(cSubscriberToken, cDbToken, "sso", true, out sXmlNode, out sError)) || sXmlNode == null)
                {
                    LoginTraceLog(eResApp.GetRes(_iLang, 1273), sError);
                }
                else
                {
                    xDetailsNode.InnerXml = sXmlNode;
                    bSuccess = true;
                }
            }
            else
            {
                try
                {


                    eSSO_CAS sso = new eSSO_CAS(_pref, cDbToken, _context.Request);
                    if (!sso.Init(out sError))
                    {
                        bSuccess = false;
                        LoginTraceLog(eResApp.GetRes(_iLang, 1273), String.Concat("Erreur de récupération des informations de SSO > GetSSOInfo (CONFIGADV - SSO_CAS) : ", sError));
                        sError = "Erreur de récupération des informations de SSO > GetSSOInfo (CONFIGADV - SSO_CAS) : La base est peut-être dans une version trop ancienne";
                        return xmlResult;
                    }

                    XmlNode xSsoNode = xmlResult.CreateElement("CAS");  //SSO Actif ?
                    xSsoNode.InnerText = sso.Enabled ? "1" : "0";
                    xDetailsNode.AppendChild(xSsoNode);

                    xSsoNode = xmlResult.CreateElement("CAS_URL");
                    xSsoNode.InnerText = sso.UrlLogin;  //Url du SSO
                    xDetailsNode.AppendChild(xSsoNode);

                    xSsoNode = xmlResult.CreateElement("XRM_RETURNURL");
                    xSsoNode.InnerText = sso.UrlReturnXRM;  //Url vers laquelle le SSO va rediriger
                    xDetailsNode.AppendChild(xSsoNode);
                }
                catch (Exception ex)
                {
                    bSuccess = false;
                    LoginTraceLog(eResApp.GetRes(_iLang, 1273), ex.ToString());
                    sError = ex.Message;
                }
                finally
                {
                    // Renvoi du résultat
                    XmlNode xResultNode = xmlResult.CreateElement("result");
                    xResultNode.InnerText = (bSuccess ? "SUCCESS" : "FAILURE");
                    xDetailsNode.AppendChild(xResultNode);

                    XmlNode _errDesc = xmlResult.CreateElement("errordescription");
                    _errDesc.InnerText = sError;
                    xDetailsNode.AppendChild(_errDesc);
                }
            }
            return xmlResult;
        }



        /// <summary>
        /// Création des informations de redirection pour connexion cross serveur
        /// Fourni un noeud xml qui sera interprété en js
        /// </summary>
        /// <param name="cSubscriberToken">Token abonné</param>
        /// <param name="cDbToken">Token base de données</param>
        /// <param name="sNodeToGet">chemin</param>
        /// <param name="bIsCompatibleV7">Base avec une v7 disponible</param>
        /// <param name="returnXNode">chaine xml du noeud de redirection</param>
        /// <param name="sError">Message d'erreur éventuel</param>
        /// <returns></returns>
        public Boolean GetRedirectNode(SubscriberToken cSubscriberToken, DbToken cDbToken, String sNodeToGet, Boolean bIsCompatibleV7, out String returnXNode, out String sError)
        {
            sError = String.Empty;
            returnXNode = String.Empty;
            try
            {
                String sRedirectURL = String.Concat(cDbToken.AppUrl.TrimEnd('/'), '/');

                Boolean bFromRedirect = _context.Request.Headers.AllKeys.Contains("X-FROMREDIRECT") && _context.Request.Headers["X-FROMREDIRECT"] == "1";

                //Boucle de redirection
                if (bFromRedirect)
                    throw new Exception("Boucle de redirection.");

                // URL de redirection 
                /*
                /* Redirection en utilisant une requête POST pou renvoyer toutes les informations
                 * Suivant la version de la base, redirige vers le manager v7 ou xrm
                 * Le manager XRM peut récupérer les user v7 si la base est sur le même serveur que lui
                /* 
                /**/
                if (cDbToken.Version.ToLower().StartsWith("v7"))
                {
                    if (bIsCompatibleV7)
                    {
                        sError = "mode non compatible v7";
                        return false;
                    }
                    sRedirectURL = string.Concat(sRedirectURL, "dotnet/");
                }

                sRedirectURL = String.Concat(sRedirectURL, "mgr/eLoginMgr.ashx?action=", _context.Request.QueryString["action"].ToString());    //getuserlist

                //
                XmlDocument remoteXMLResult = new XmlDocument();

                // On doit autoriser l'accès cross-domain
                _context.Response.AppendHeader("Access-Control-Allow-Origin", "*");

                // Construction des paramètres à poster
                StringBuilder postData = new StringBuilder();
                postData.Append("st").Append("=").Append(HttpUtility.UrlEncode(cSubscriberToken.GetTokenCrypted())).Append("&");
                postData.Append("db").Append("=").Append(HttpUtility.UrlEncode(cDbToken.GetTokenCrypted())).Append("&");

                /* Envoi de la requête */
                StreamWriter writer = null;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(sRedirectURL);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = postData.ToString().Length;

                //AJoute un header de redirection pour éviter les boucles de redirections
                request.Headers.Add("X-FROMREDIRECT", "1");

                try
                {
                    writer = new StreamWriter(request.GetRequestStream());
                    writer.Write(postData.ToString());
                }
                finally
                {
                    if (writer != null)
                        writer.Close();
                }

                // Lecture de la réponse et extraction des informations
                HttpWebResponse response = null;
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch (System.Net.WebException ex)
                {
                    if (((System.Net.WebException)ex).Response != null)
                    {


                        throw new EudoException(
                             sMessage: "Appurl invalide pour la base : " + sRedirectURL,
                             sUserMessage: eResApp.GetRes(_pref, 2656),
                             innerExcp: ex
                             );


                    }
                    else
                        throw;
                }

                StreamReader stream = new StreamReader(response.GetResponseStream());

                remoteXMLResult.LoadXml(stream.ReadToEnd());
                XmlNode remoteXMLMainUserNode = remoteXMLResult.SelectSingleNode(sNodeToGet); //userlist/users
                if (remoteXMLMainUserNode != null)
                {
                    returnXNode = remoteXMLMainUserNode.InnerXml;
                    return true;
                }
                else
                {

                    XmlNode NodeErr = remoteXMLResult.SelectSingleNode("errordescription");
                    if (NodeErr != null)
                        sError = NodeErr.InnerText;
                    else
                        sError = "une erreur est survenue"; //TODO récupérer l'erreur du serveur distant
                    return false;
                }

            }
            catch (EudoException)
            {
                throw;
            }
            // Toute erreur d'appel de l'URL distante (404, analyse du flux de retour incorrecte...) renvoie une erreur générique
            catch (Exception ex)
            {
                sError = ex.ToString();
                return false;
            }

        }

        /// <summary>
        /// Génération d'un XmlDocument avec declaration par défaut
        /// </summary>
        /// <returns></returns>
        public XmlDocument GetXmlReturn()
        {
            XmlDocument xmlResult = new XmlDocument();
            XmlNode xLMainNode = xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null);
            xmlResult.AppendChild(xLMainNode);
            return xmlResult;

        }
        /// <summary>
        /// Retourne la liste des utilisateurs de la base
        /// </summary>
        public XmlDocument GetUserList(SubscriberToken cSubscriberToken, DbToken cDbToken)
        {

            // Init
            bool bSuccess = false;
            string strUserListError = String.Empty;
            String sRememberMe = String.Empty;


            //Préparation du flux xml de retour
            XmlDocument xmlResult = GetXmlReturn();
            XmlNode xDetailsNode = xmlResult.CreateElement("userlist"); //"userlist"
            xmlResult.AppendChild(xDetailsNode);

            try
            {

                #region REDIRECTION

                //Récupération de la liste des utilisateurs
                //- soit en appelant directement l'URL inscrite dans AppUrl et en interprétant son propre retour XML
                //- soit directement en base si l'on a pas demandé à appeler une URL externe
                // Redirection vers l'URL indiquée dans AppUrl afin de récupérer les informations de connexion depuis cet autre serveur externe
                // S'il s'agit bien d'un serveur XRM, l'appel renverra les informations souhaitées.
                // Si l'appel échoue (URL inexistante, URL v7, serveur non autorisé...), on renverra un flux XML d'erreur avec "Connexion à la base non autorisée

                if (IsRedirect(cDbToken))
                {
                    String xmlNodeUser = String.Empty;
                    String sError = String.Empty;

                    if ((!GetRedirectNode(cSubscriberToken, cDbToken, "userlist/users", true, out xmlNodeUser, out sError)) || xmlNodeUser == null)
                    {
                        LoginTraceLog(eResApp.GetRes(_iLang, 1273), sError);
                        strUserListError = eResApp.GetRes(_iLang, 1273); // Connexion à la base non autorisée
                    }
                    else
                    {

                        //Liste des users
                        XmlNode xMainUserNode = xmlResult.CreateElement("users");

                        xMainUserNode.InnerXml = xmlNodeUser;

                        xDetailsNode.AppendChild(xMainUserNode);
                        bSuccess = true;
                    }


                }
                #endregion
                else
                {
                    #region GENERATION DU FLUX XML DES UTILISATEURS
                    /*
                     * La liste des user peut être récupérer par le LoginMgr de XRM même si la base est une base V7
                     * si elle est sur le même serveur que le manager.
                     * 
                     */

                    eLoginOL login = eLoginOL.GetLoginObject(cSubscriberToken);
                    List<UserToken> list = new List<UserToken>();
                    if (!login.AuthSubscribers())
                        throw new Exception("Authentification subscribers invalide pour getUserList.");

                    try
                    {
                        list = login.GetUserList(cDbToken);
                    }
                    catch (SqlException e)
                    {
                        // si base inacessible et RedirectEnabled a false,
                        if (e.Number == 4060 && !cDbToken.RedirectEnabled)
                        {
                            cDbToken.RedirectEnabled = true;

                            return GetUserList(cSubscriberToken, cDbToken);
                        }

                        throw new EudoException(eResApp.GetRes(_iLang, 2789), eResApp.GetRes(_iLang, 2789), e);
                    }
                    if (list.Count == 0)
                        throw new Exception(eResApp.GetRes(_iLang, 2790));

                    //Liste des users
                    XmlNode xMainUserNode = xmlResult.CreateElement("users");
                    xDetailsNode.AppendChild(xMainUserNode);

                    //Boucle sur tous les users
                    foreach (UserToken ut in list)
                    {
                        //Ajout au xml
                        XmlNode usrNode = xmlResult.CreateElement("user");
                        xMainUserNode.AppendChild(usrNode);


                        XmlNode userIdNode = xmlResult.CreateElement("userid");
                        userIdNode.InnerText = ut.Userid.ToString();
                        usrNode.AppendChild(userIdNode);

                        XmlNode userLoginNode = xmlResult.CreateElement("userlogin");
                        userLoginNode.InnerText = ut.Login.ToUpper();
                        usrNode.AppendChild(userLoginNode);

                        XmlNode userNameNode = xmlResult.CreateElement("username");
                        userNameNode.InnerText = ut.UserDisplayName.ToUpper();
                        usrNode.AppendChild(userNameNode);
                    }



                    bSuccess = true;

                    #endregion
                }
            }
            // Erreur générique
            catch (EudoException eeex)
            {
                strUserListError = eeex.UserMessage;
                bSuccess = false;
            }
            // Erreur générique
            catch (Exception _eUser)
            {
                strUserListError = _eUser.Message;
                bSuccess = false;
            }

            // Renvoi du résultat
            XmlNode xResultNode = xmlResult.CreateElement("result");
            xResultNode.InnerText = (bSuccess ? "SUCCESS" : "FAILURE");
            xDetailsNode.AppendChild(xResultNode);

            XmlNode _errDesc = xmlResult.CreateElement("errordescription");
            _errDesc.InnerText = strUserListError;
            xDetailsNode.AppendChild(_errDesc);

            return xmlResult;


        }

        /// <summary>
        /// Indique si l'on doit rediriger le client vers un autre serveur et donc repasser les paramètre à ce serveur
        /// </summary>
        /// <param name="cDbToken">Informations de connexion à la base du client</param>
        /// <returns>Vrai : on doit rediriger vers une autre URL</returns>
        private bool IsRedirect(DbToken cDbToken)
        {


            String sRedirectURL = String.Concat(cDbToken.AppUrl.TrimEnd('/'), '/');



            //Redirection si url de redirection (appurl) non vide et différente de url en cours et redirect enabled activé 
            return cDbToken.RedirectEnabled && cDbToken.AppUrl.Length != 0 && _sLocalURL.ToLower().CompareTo(sRedirectURL.ToLower()) != 0;

        }

        /// <summary>
        /// Appel distant pour authentifier un user
        /// Réalise un appel au manager sur le serveur hébergeant la base
        /// sur la base de l'url fourni dans le token de base
        /// </summary>
        /// <param name="cSubscriberToken">Token Abonné</param>
        /// <param name="cDBToken">Token de base de données cible</param>
        /// <param name="cUserToken">Token User</param>
        /// <returns>Flux XML de retour en provenance du AuthUser du eLoginMgr de l'url de la base cible</returns>
        public XmlDocument AuthUserDistant(SubscriberToken cSubscriberToken, DbToken cDBToken, UserToken cUserToken)
        {
            //Préparation du flux xml de retour
            XmlDocument xmlResult;
            xmlResult = new XmlDocument();
            XmlNode xLMainNode = xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null);
            xmlResult.AppendChild(xLMainNode);

            Boolean bFromRedirect = false;
            bFromRedirect = _context.Request.Headers.AllKeys.Contains("X-FROMREDIRECT") && _context.Request.Headers["X-FROMREDIRECT"] == "1";

            //Boucle de redirection
            if (bFromRedirect)
                throw new Exception("Boucle de redirection.");

            // URL de redirection 

            /*
            /* Redirection en utilisant une requête POST pou renvoyer toutes les informations
             * Suivant la version de la base, redirige vers le manager v7 ou xrm
             * Le manager XRM peut récupérer les user v7 si la base est sur le même serveur que lui
            /* 
            /**/

            String sRedirectURL = String.Concat(cDBToken.AppUrl.TrimEnd('/'), '/');
            if (cDBToken.Version.ToLower().StartsWith("v7"))
                sRedirectURL = String.Concat(sRedirectURL, "dotnet/");

            sRedirectURL = String.Concat(sRedirectURL, "mgr/eLoginMgr.ashx?action=fromredirection");


            // On doit autoriser l'accès cross-domain
            _context.Response.AppendHeader("Access-Control-Allow-Origin", "*");

            String sOrigIP = eLibTools.GetUserIPV4();


            // Construction des paramètres à poster
            StringBuilder postData = new StringBuilder()
                .Append(eLoginOLGeneric.GetQSToken(cSubscriberToken, cDBToken, cUserToken))
                .Append("&Width=").Append(_context.Request.Form["Width"].ToString())
                .Append("&Height=").Append(_context.Request.Form["Height"].ToString())
                .Append("&OrigIP=").Append(sOrigIP)
                .Append("&rememberme=").Append(_bRememberInfo ? "1" : "0");




            /***********************/
            /* Envoi de la requête */
            /***********************/
            StreamWriter writer = null;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(sRedirectURL);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postData.ToString().Length;

            //AJoute un header de redirection pour éviter les boucles de redirections
            request.Headers.Add("X-FROMREDIRECT", "1");

            //Ajoute l'ip du demandeur initial
            request.Headers.Add("X-FROMIP", eLibTools.GetUserIPV4());

            try
            {
                writer = new StreamWriter(request.GetRequestStream());
                writer.Write(postData.ToString());
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }

            // Lecture de la réponse et extraction des informations
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader stream = new StreamReader(response.GetResponseStream());
            XmlDocument remoteXMLResult = new XmlDocument();
            remoteXMLResult.LoadXml(stream.ReadToEnd());

            //Test le succes/echec ce l'authentification
            XmlNode remoteXMLMainUserNode = remoteXMLResult.SelectSingleNode("userauthentication/result");
            if (remoteXMLMainUserNode.InnerText == "SUCCESS" && _bRememberInfo)
            {
                eTools.SaveCookie("db", cDBToken.BaseUid.ToString(), DateTime.MaxValue, _context.Response);
                eTools.SaveCookie("UserLoginXrm", cUserToken.Login, DateTime.MaxValue, _context.Response);
            }


            return remoteXMLResult;
        }

        /// <summary>
        /// Authentification d'un user
        /// </summary>
        /// <param name="cSubscriberToken">Token Abonné</param>
        /// <param name="cDBToken">Token de base de données cible</param>
        /// <param name="cUserToken">Token User</param>
        /// <param name="eAction">Type d'action </param>
        /// <returns>Flux XML indiquant le resultat de l'authentification et l'url surlaquelle rediriger l'utilisateur en fonction de ce résultat</returns>
        public XmlDocument AuthUser(SubscriberToken cSubscriberToken, DbToken cDBToken, UserToken cUserToken, eConst.LOGIN_ACTION eAction)
        {
            //Préparation du flux xml de retour
            XmlDocument xmlResult = GetXmlReturn();

            XmlNode detailsNode = xmlResult.CreateElement("userauthentication");
            xmlResult.AppendChild(detailsNode);

            try
            {
                String sResult = "FAILURE";
                String sErrorMsg = String.Empty;
                String sTokenV7 = String.Empty;

                AUTH_USER_RES res = AUTH_USER_RES.FAIL;

                //Test sur les démande provenant d'une redirection : l'ip d'origine doit être spécifié
                Boolean bFromRedirect = _context.Request.Headers.AllKeys.Contains("X-FROMREDIRECT") && _context.Request.Headers["X-FROMREDIRECT"] == "1";
                if (bFromRedirect && !_requestTools.AllKeys.Contains("OrigIP"))
                    return GetReturnXML(false, "userauthentication", "", String.Empty, eResApp.GetRes(_iLang, 72));

                #region EUDOADMIN

                Boolean bFromEudoAdmin = _context.Request.Headers.AllKeys.Contains("X-FROMEUDOADMIN");

                String sIV = String.Empty;
                if (bFromEudoAdmin)
                {
                    String sToken = _context.Request.Headers["X-FROMEUDOADMIN"];
                    sIV = _context.Request.Form["rememberme"].ToString();

                    sToken = CryptoTripleDES.Decrypt(sToken, CryptographyConst.KEY_CRYPT_LINK1, sIV);

                    if (!sToken.Contains(cUserToken.Login))
                        return GetReturnXML(false, "userauthentication", "AuthUser", String.Empty, eResApp.GetRes(_iLang, 5), "INVALID_USER");

                    //Le token de demande a une durée de vie de 2 minutes
                    String sTicks = sToken.Substring(sToken.IndexOf(cUserToken.Login) + cUserToken.Login.Length);
                    long nTick;
                    long.TryParse(sTicks, out nTick);
                    TimeSpan ts = new TimeSpan(DateTime.Now.Ticks - nTick); // TS du token - Test d'expiration

                    if (ts.Minutes > 2)
                        return GetReturnXML(false, "userauthentication", "AuthUser", String.Empty, eResApp.GetRes(_iLang, 5), "INVALID_USER");

                    // Verification si le userlogin et userpawd corresponde au ut également
                    String userToken = _requestTools.GetRequestFormKeyS("ut");

                    // Pas de user token, connexion invalide !
                    if (userToken == null)
                        return GetReturnXML(false, "userauthentication", "AuthUser", String.Empty, eResApp.GetRes(_iLang, 5), "INVALID_USER");

                    UserToken tempUserToken = new UserToken();
                    tempUserToken.LoadTokenCrypted(userToken, fromEudoAdmin: true);

                    if (!cUserToken.Login.Equals(tempUserToken.Login) || !cUserToken.Pwd.Equals(tempUserToken.Pwd))
                        return GetReturnXML(false, "userauthentication", "AuthUser", String.Empty, eResApp.GetRes(_iLang, 5), "INVALID_USER");
                }

                #endregion

                //Authentification de l'utilisateur
                eLoginOL login = eLoginOL.GetLoginObject(cSubscriberToken, cUserToken);
                res = login.AuthUser(bFromEudoAdmin, cDBToken, bFromEudoAdmin, eAction == eConst.LOGIN_ACTION.AUTHCAS);

                //Enregistrement des cookies 
                if (res != AUTH_USER_RES.SUCCESS)
                {
                    if (res == AUTH_USER_RES.IP_FAIL)
                    {
                        //Erreur D'adresse IP
                        return GetReturnXML(false, "userauthentication", "AuthUser", String.Empty, eResApp.GetRes(_iLang, 515).Replace("<IP>", eLoginOL.GetCurrentIPAddress(_context)), "INVALID_USER");
                    }
                    else if (res == AUTH_USER_RES.SUB_FAIL)
                    {

                        return GetReturnXML(false, "userauthentication", "AuthUser", _requestTools.IsLocalOrEudo ? login.ErrMsg : String.Empty, eResApp.GetRes(_iLang, 6210), "INVALID_SUB");
                    }
                    else if (res == AUTH_USER_RES.USER_LOCKED)
                    {

                        return GetReturnXML(false, "userauthentication", "AuthUser", String.Empty, login.UserErrorMsg, "INVALID_USER");
                    }
                    else if (res == AUTH_USER_RES.CODE_FAIL)
                    {
                        //Erreur nb essai dépassé
                        if (VerifMaxConnectionNumber(eAction))
                        {
                            return GetReturnXML(false, "userauthentication", "AuthUser", String.Empty, eResApp.GetRes(_iLang, 8887), "NBR_MAX_CONN");
                        }

                        //Erreur authentification
                        return GetReturnXML(false, "userauthentication", "AuthUser", String.Empty, eResApp.GetRes(_iLang, 8892), "INVALID_USER");
                    }
                    else if (res == AUTH_USER_RES.EXCEPTION)
                    {
                        if (login.InnerException != null)
                        {
                            LoginTraceLog(login.InnerException.Message, Environment.NewLine, login.InnerException.StackTrace);
                        }

                        return GetReturnXML(false, "userauthentication", "AuthUser", _requestTools.IsLocalOrEudo ? login.ErrMsg : String.Empty, eResApp.GetRes(_iLang, 1872), "AUTH_EXCEPTION");
                    }
                    else if (res != AUTH_USER_RES.PWD_EXPIRED)
                    {
                        //Erreur nb essai dépassé
                        if (VerifMaxConnectionNumber(eAction))
                        {
                            //ClearCookies();
                            return GetReturnXML(false, "userauthentication", "AuthUser", String.Empty, eResApp.GetRes(_iLang, 692), "NBR_MAX_CONN");
                        }



                        //Erreur authentification
                        return GetReturnXML(false, "userauthentication", "AuthUser", String.Empty, eResApp.GetRes(_iLang, 5), "INVALID_USER");
                    }
                }

                #region Connexion Succes

                sResult = "SUCCESS"; //Success 

                //Sauvegarde des cookies
                if (_bRememberInfo)
                {
                    eTools.SaveCookie("UserLoginXrm", cUserToken.Login, DateTime.MaxValue, _context.Response);
                    eTools.SaveCookie("db", cDBToken.BaseUid.ToString(), DateTime.MaxValue, _context.Response);
                }

                //Pour les v7, on doit générer un token sur la base
                if (cDBToken.Version.ToLower().StartsWith("v7"))
                    sTokenV7 = login.SetSessionToken();
                else if (!bFromRedirect) // Si Connexion XRM sans redirec, set les var de session
                    login.SetSessionVars();

                String sRedirUrl = String.Empty;

                //Adaptation du retour en fonction du type de connexion (direct, v7, sur un autre serveur)...
                if (bFromRedirect) // 
                {
                    // 
                    //L'appel a été fait depuis un autre serveur (via AuthUserDistant) de XRM à XRM
                    // Dans ce cas, l'url de retour doit contenir un jeu de token
                    // permettant l'ouverture de session sur le serveur concerné

                    sRedirUrl = String.Concat(cDBToken.AppUrl.TrimEnd('/'), "/eLogin.aspx?XM=1");

                    //Ajout des token de connexion
                    StringBuilder postData = new StringBuilder()
                    .Append(eLoginOLGeneric.GetQSToken(cSubscriberToken, cDBToken, cUserToken, sIV))
                    .Append("&r=").Append(_bRememberInfo ? "1" : "0");

                    //Ajout du hash
                    String sHash = HashSHA.GetHashSHA1(String.Concat(cSubscriberToken.Login, "|", cUserToken.Login, "|", cDBToken.DbDirectory, "|", eLibTools.GetUserIPV4()));
                    postData.Append("&h=").Append(HttpUtility.UrlEncode(sHash));

                    sRedirUrl = String.Concat(sRedirUrl, postData.ToString());
                }
                else if (cDBToken.Version.ToLower().StartsWith("v7")) // Redirection XRM -> V7
                {
                    //La base demandée est une V7.
                    //Il faut rediriger vers l'url V7 avec un jeu de token permettant l'ouverture de la session

                    sRedirUrl = String.Concat(cDBToken.AppUrl.TrimEnd('/'), "/app/LogDatabaseTreatment.asp?XM=1");

                    //Ajout des token de connexion
                    StringBuilder postData = new StringBuilder()
                    .Append(eLoginOLGeneric.GetQSToken(cSubscriberToken, cDBToken, cUserToken))
                    .Append("&r=").Append(_bRememberInfo ? "1" : "0")
                    .Append("&s=").Append(sTokenV7);

                    //Ajout du hash
                    String sHash = HashSHA.GetHashSHA1(String.Concat(cSubscriberToken.Login, "|", cUserToken.Login, "|", cDBToken.DbDirectory, "|", eLibTools.GetUserIPV4()));
                    postData.Append("&h=").Append(HttpUtility.UrlEncode(sHash));

                    sRedirUrl = String.Concat(sRedirUrl, postData.ToString());
                }
                else if (res == AUTH_USER_RES.SUCCESS)
                {
                    //Connexion standard
                    sRedirUrl = String.Concat(sRedirUrl, "eMain.aspx");
                }
                else if (res == AUTH_USER_RES.PWD_EXPIRED)
                {
                    //Page de changement de mdp
                    sRedirUrl = String.Concat(sRedirUrl, "eUserPassword.aspx");
                }

                XmlNode urlNode = xmlResult.CreateElement("url");
                urlNode.InnerText = sRedirUrl;
                detailsNode.AppendChild(urlNode);

                XmlNode resultNode = xmlResult.CreateElement("result");
                resultNode.InnerText = sResult;
                detailsNode.AppendChild(resultNode);

                #endregion
            }
            catch (XRMUpgradeException xupg)
            {
                throw xupg;

            }
            catch (Exception ex)
            {
                LoginTraceLog(ex.Message, Environment.NewLine, ex.StackTrace);


#if DEBUG
                return GetReturnXML(false, "userauthentication", "authuser", eResApp.GetRes(_iLang, 72), String.Concat(ex.Message, Environment.NewLine, ex.StackTrace));
#else
                if (_requestTools.IsLocalOrEudo)
                    return GetReturnXML(false, "userauthentication", "authuser", eResApp.GetRes(_iLang, 72), String.Concat(ex.Message, Environment.NewLine, ex.StackTrace));
                else
                    return GetReturnXML(false, "userauthentication", "authuser", eResApp.GetRes(_iLang, 72), "");
#endif
            }

            return xmlResult;
        }

        /// <summary>
        /// Nombre max de connexions
        /// </summary>
        /// <returns>Vrai si le nombre max de tentative a été atteint</returns>
        /// <param name="action">Type d'acion à effectuer</param>
        private Boolean VerifMaxConnectionNumber(eConst.LOGIN_ACTION action)
        {
            //Nombre de connexions
            int nNb = 0;


            switch (action)
            {
                case eConst.LOGIN_ACTION.AUTHSUBSCRIBER:

                    //Incrémente le nombre de tentative
                    if (_context.Session["SubscriberConnections"] != null)
                        Int32.TryParse(_context.Session["SubscriberConnections"].ToString(), out nNb);

                    _context.Session["SubscriberConnections"] = nNb;

                    if (nNb >= eConst.NB_MAX_CNX)
                        return true;

                    break;


                case eConst.LOGIN_ACTION.AUTHUSER:

                    //Incrémente le nombre de tentative
                    if (_context.Session["UserConnections"] != null)
                        Int32.TryParse(_context.Session["UserConnections"].ToString(), out nNb);

                    nNb++;
                    _context.Session["UserConnections"] = nNb;


                    if (nNb >= eConst.NB_MAX_CNX)
                        return true;

                    break;
            }

            return false;
        }

        /// <summary>
        /// Retour flux XML de retour avec des nodes basiques
        /// <param name="bSuccess">Echec ou succes de l'Action</param>
        /// <param name="sBaseNode">Action de départ</param>
        /// <param name="sSource">Sous action de départ</param>
        /// <param name="sMsg">Message de résulat</param>
        /// <param name="sErrorDescription"></param>
        /// <param name="sErrorCode">Code d'erreur</param>
        /// </summary>
        /// <returns></returns>
        private XmlDocument GetReturnXML(Boolean bSuccess, String sBaseNode, String sSource, String sMsg, String sErrorDescription, String sErrorCode = "")
        {

            XmlDocument xmlResult = GetXmlReturn();

            if (sBaseNode.Length == 0)
                sBaseNode = "eLoginMgr";

            XmlNode detailsNode = xmlResult.CreateElement(sBaseNode);
            xmlResult.AppendChild(detailsNode);

            XmlNode resultNode = xmlResult.CreateElement("result");
            resultNode.InnerText = bSuccess ? "SUCCESS" : "FAILURE";
            detailsNode.AppendChild(resultNode);

            XmlNode srcNode = xmlResult.CreateElement("src");
            srcNode.InnerText = sSource;
            detailsNode.AppendChild(srcNode);

            XmlNode errDesc = xmlResult.CreateElement("errordescription");
            errDesc.InnerText = sErrorDescription;
            detailsNode.AppendChild(errDesc);

            XmlNode errCode = xmlResult.CreateElement("errcode");
            errCode.InnerText = sErrorCode;
            detailsNode.AppendChild(errCode);

            XmlNode msgNode = xmlResult.CreateElement("msg");
            msgNode.InnerText = sMsg;
            detailsNode.AppendChild(msgNode);

            return xmlResult;
        }

        /// <summary>
        /// Redéfinit car LoadSession utilise de base la variable de session pref qui n'est pas encore définie
        /// </summary>
        protected override void LoadSession()
        {
            // Charge les valeurs de request
            _allKeys = new HashSet<String>(_context.Request.Form.AllKeys, StringComparer.OrdinalIgnoreCase);
            _allKeysQS = new HashSet<String>(_context.Request.QueryString.AllKeys, StringComparer.OrdinalIgnoreCase);

            _requestTools = new eRequestTools(this._context);
        }

        /// <summary>
        /// Initialise les variables de session
        /// </summary>
        private void InitSessionVars()
        {
            //Session.Abandon();
            _context.Session["Pref"] = null;
        }

        /// <summary>
        /// Log de l'erreur SQL dans les DATAS
        /// </summary>
        /// <param name="chr"></param>
        private void LoginTraceLog(params String[] chr)
        {
            try
            {
                if (chr.Length == 0)
                    return;


                StringBuilder sb = new StringBuilder();
                sb.Append(" - ").Append(DateTime.Now).Append(" : ");

                foreach (String s in chr)
                    sb.Append(s);

                eModelTools.EudoTraceLog(sb.ToString());
            }
            catch (Exception)
            {
            }
        }
    }
}