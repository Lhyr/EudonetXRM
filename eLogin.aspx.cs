using Com.Eudonet.Common.Cryptography;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Page de connexion
    /// </summary>
    public partial class eLogin : System.Web.UI.Page
    {
        #region variables globales de la page

        /// <summary>
        /// Indique si les connexions sont forcé en https
        /// </summary>
        private Boolean _bHttpsOnly = false;


        /// <summary>Connexion en SSO</summary>
        public bool _isSSOApplication = false;


        /// <summary>
        /// Connexion via adfs
        /// </summary>
        public bool _bIsADFSApplication = false;

        /// <summary>Mode d install de l'appli, ASP ou intranet></summary>
        public bool _isIntranet = false;
        /// <summary>Ressources Javascript utilisées par la page de connexion</summary>
        public string _resAppJS;

        /// <summary>Langue de connexion</summary>
        public string _langueServ;

        /// <summary>index de la langue (0 : français, 1 : anglais, ...)</summary>
        public int _nLangServ;

        /// <summary>Depuis le serveur</summary>
        public Boolean IsLocal = false;

        /// <summary>
        /// Style à appliquer sur les identifiants abonné
        /// dans le cas d'un intranet ceux-ci sont masqués
        /// </summary>
        public string _visIntranetStyle = string.Empty;

        /// <summary>Abonné enregistré</summary>
        public string _defSubscriberLogin = string.Empty;

        /// <summary>Mot de passe abonné enregistré</summary>
        public string _defSubscriberPassword = string.Empty;

        /// <summary>Login enregistré</summary>
        public string _defUserLogin = string.Empty;


        /// <summary>
        /// Style à appliquer sur les identifiants Utilisateur
        /// dans le cas d'une connexion par sso ceux-ci sont masqués
        /// </summary>
        public string _visuUser = string.Empty;

        /// <summary>L'application doit-elle garder en mémoire les identifiants abonnés ainsi que le login de l'utilisateur?</summary>
        public string _rememberMe = string.Empty;

        /// <summary>L'application est elle en intranet?</summary>
        public string _isIntranetValue = string.Empty;

        #endregion

        /// <summary>
        /// Chargement de la page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            #region DDOS
            if (eLibTools.GetServerConfig("DOSENABLED") == "1")
            {
                Int32 nLimit;
                if (!Int32.TryParse(eLibTools.GetServerConfig("DOSLIMIT", eLibConst.DOS_DEFAULT_LIMIT.ToString()), out nLimit) || nLimit <= 0)
                    nLimit = eLibConst.DOS_DEFAULT_LIMIT;

                try
                {
                    if (eDoSProtection.CheckDDOS(new WCFCall("LOGIN", nLimit), true) <= 0)
                        return;
                }
                catch (eEudoDoSException)
                {
                    // Protection déclenché, on return
                    Response.Redirect("blank.htm");

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
            #endregion

            //AUTHENTIF Externe
            if (eLibTools.GetServerConfig("EXTERNALAUTH", "0") == "1")
            {
                //SAML
                if (eLibTools.GetServerConfig("EXTERNALAUTHTYPE", "0") == "2")
                {
                    HashSet<String> lstQS = new HashSet<String>(Request.QueryString.AllKeys);

                    if (lstQS.Contains("d") && Request.QueryString["d"].ToString() == "1")
                        Response.Redirect("LogOutSAML.ashx"); // Pseudo Page gérer par le module SAMLV2 via handler déclarer dans le web.config
                    else
                        Response.Redirect("LoginSAML.ashx"); // Pseudo Page gérer par le module SAMLV2 via handler déclarer dans le web.config
                }
            }

            Session.Clear();

            #region ADFS

            //Authentification ADFS
            //  - Un seul abonné
            //  - Une seule base
            _bIsADFSApplication = eLibTools.GetServerConfig("ADFSApplication", "0") == "1";

            Session["FROMXRM"] = "1";

            if (_bIsADFSApplication)
            {
                String sTimeStamp = String.Concat(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), " : ");
                try
                {

                    eModelTools.EudoTraceLogExternalConnexion("RECHERCHE DE LA CLE DE CONFIG BASEID START ", eModelConst.TypeLogExternalAuth.INFO);

                    // UID de base de données
                    String sDBID = eLibTools.GetServerConfig("ADFSDBID", "");
                    if (sDBID.Length == 0)
                    {
                        //
                        eModelTools.EudoTraceLogExternalConnexion(String.Concat(" -> CLE DE CONFIG ADFSDBID ABSENTE"));

                        //Echec de connexion
                        Response.Redirect("eADFSERROR.html");
                        return;
                    }

                    eModelTools.EudoTraceLogExternalConnexion(String.Concat(" -> CLE DE CONFIG DEFAUT ADFSDBID TROUVEE ", sDBID), eModelConst.TypeLogExternalAuth.INFO);

                    //Récupération du login Window de l'ADFS
                    eModelTools.EudoTraceLogExternalConnexion(String.Concat("RECHERCHE DU CLAIMS DE LOGIN"), eModelConst.TypeLogExternalAuth.INFO);
                    String sExternalLogin = eTools.GetEudoExternalLoginFromExternalAuth();
                    if (sExternalLogin.Length == 0)
                    {

                        eModelTools.EudoTraceLogExternalConnexion(String.Concat(" -> LOGIN NON RECUPERE DE L'ADFS"));
                        //Echec de connexion
                        Response.Redirect("eADFSERROR.html");
                        return;
                    }


                    DbTokenGeneric cDbToken = new DbTokenGeneric();
                    SubscriberToken cSubscriberToken = new SubscriberToken();
                    UserToken cUserToken = new UserToken();
                    eLoginOL login = null;
                    String sError = String.Empty;

                    eModelTools.EudoTraceLogExternalConnexion(String.Concat("GENERATION DU TOKEN DE CONNEXION"), eModelConst.TypeLogExternalAuth.INFO);
                    //Récupération des TOKEN a partir du login window
                    AUTH_USER_RES resADFS = eLoginOL.GetLoginObjectForExternalLogin(sDBID, sExternalLogin, out login, out sError);


                    //Connexion SUCCES
                    if (resADFS == AUTH_USER_RES.SUCCESS && login != null && !IsPostBack)
                    {
                        eModelTools.EudoTraceLogExternalConnexion(String.Concat("-> CONNEXION REUSSIE"), eModelConst.TypeLogExternalAuth.INFO);
                        login.SetSessionVars();
                        Response.Redirect("eMain.aspx");
                    }
                    else
                    {
                        eModelTools.EudoTraceLogExternalConnexion(String.Concat("-> CONNEXION ECHEC"), eModelConst.TypeLogExternalAuth.ERROR);
                    }


                }
                catch (eEndResponseException) { Response.End(); }
                catch (ThreadAbortException)
                {

                }
                catch (Exception ex)
                {


                    eModelTools.EudoTraceLogExternalConnexion(String.Concat("ERREUR CONNEXION PAGE_LOAD "));
                    eModelTools.EudoTraceLogExternalConnexion(String.Concat("ERREUR MESSAGE :", ex.Message));
                    eModelTools.EudoTraceLogExternalConnexion(String.Concat("ERREUR STACKTRACE :", ex.StackTrace));

                }



                Response.Redirect("eADFSERROR.html");
            }


            #endregion

            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings.Get("IntranetApplication")))
            {
                _isIntranet = (ConfigurationManager.AppSettings.Get("IntranetApplication") == "1");
                _isIntranetValue = "1";
            }

            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings.Get("SSOApplication")))
                _isSSOApplication = (ConfigurationManager.AppSettings.Get("SSOApplication") == "1");

            Session["CHECKXRM"] = "1";
            if (!IsPostBack)
            {
                Session["FromHTTPSRedirect"] = "0";
                Session["AcceptCookie"] = eLibTools.GetToken(10);

            }

            if (Request.UserAgent == null
                    || Request.UserAgent.ToLower().StartsWith("curl")
                    || Request.UserAgent.ToLower().StartsWith("sqlmap"))
            {
                Response.End();
            }


            Boolean bForceLang = false;

            //Ressources et langue
            _langueServ = eTools.GetCookie("langue", Request);

            if (string.IsNullOrEmpty(_langueServ))
                _langueServ = "LANG_00";
            else
            {

                //53590
                //Pour la connexion SSO, on ne force pas la langue en fonction du cookie
                bForceLang = !_isSSOApplication;
            }

            HashSet<String> lst = new HashSet<String>(Request.QueryString.AllKeys);

            if (lst.Contains("activeLang"))
            {
                _langueServ = Request.QueryString["activeLang"];

                if (!eLibConst.RegLang.IsMatch(_langueServ))
                    _langueServ = "LANG_00";

                bForceLang = true;
                eTools.SaveCookie("langue", _langueServ, DateTime.MaxValue, this.Context.Response);
            }
            Session["ednForceLng"] = bForceLang ? "1" : "0";

            var t = eTools.LoadLoginPageInfos();

            _nLangServ = 0;
            if (!Int32.TryParse(_langueServ.ToUpper().Replace("LANG_", string.Empty), out _nLangServ))
                eTools.SaveCookie("langue", String.Empty, DateTime.MaxValue, Response);

            if (Request.IsSecureConnection)
                spConnexion.InnerHtml = eResApp.GetRes(_nLangServ, 6206);
            else
                spConnexion.InnerHtml = eResApp.GetRes(_nLangServ, 5004);

            // Bienvenue dans l'Espace Abonné
            txtWelcomeTo.InnerText = eResApp.GetRes(_nLangServ, 7232);
            txtLoginSpace.InnerText = eResApp.GetRes(_nLangServ, 7233);

            #region Connexion from redirect / EudoAdmin

            /*  Rerirection depuis connexion autre serveur */
            if (lst.Contains("XM")
                    && Request.QueryString["XM"] == "1"
                    && lst.Contains("st")
                    && lst.Contains("ut")
                    && lst.Contains("dbt")
                    && lst.Contains("h")
                )
            {
                String st = Request.QueryString["st"];
                String ut = Request.QueryString["ut"];
                String dt = Request.QueryString["dbt"];
                String sOrigHash = Request.QueryString["h"];

                Boolean bRememeber = lst.Contains("r") && Request.QueryString["r"] == "1";
                Boolean bFromEudoAdmin = lst.Contains("fea") && Request.QueryString["fea"].Length > 0;

                String sIV = String.Empty;
                Int32 nTKExpire = 0;
                if (bFromEudoAdmin) //Si Token eudoadmin, expire en 5 minutes
                {
                    nTKExpire = 5;
                    sIV = Request.QueryString["fea"].ToString();
                }

                DbToken dbToken = new DbToken();
                UserToken utToken = new UserToken();
                SubscriberToken stToken = new SubscriberToken();

                try
                {
                    if (dbToken.LoadTokenCrypted(dt, sIV, nTKExpire) && utToken.LoadTokenCrypted(ut, sIV, nTKExpire) && stToken.LoadTokenCrypted(st, sIV, nTKExpire))
                    {
                        String sOrigIp = eLibTools.GetUserIPV4();
                        String sHash = HashSHA.GetHashSHA1(String.Concat(stToken.Login, "|", utToken.Login, "|", dbToken.DbDirectory, "|", sOrigIp));

                        sHash = String.Empty;
                        sOrigHash = String.Empty;

                        eLoginOL login = eLoginOL.GetLoginObject(stToken, utToken);
                        if (sHash == sOrigHash)
                        {
                            AUTH_USER_RES res = login.AuthUser(bFromEudoAdmin, dbToken, bFromEudoAdmin);

                            if ((res == AUTH_USER_RES.SUCCESS || res == AUTH_USER_RES.PWD_EXPIRED))
                            {
                                #region Données d'EudoAdmin

                                if (bFromEudoAdmin)
                                {
                                    // On recup la langue eventuellement définie par EudoAdmin
                                    if (lst.Contains("opl"))
                                    {
                                        String lang = Request.QueryString["opl"].ToString();
                                        if (!String.IsNullOrEmpty(lang))
                                            utToken.Lang = lang;
                                    }

                                    // TODO - Ouverture vers une fiche
                                    Int32 tab, fileid;
                                    if (lst.Contains("opt"))
                                        tab = eLibTools.GetNum(Request.QueryString["opt"].ToString());
                                    if (lst.Contains("opf"))
                                        fileid = eLibTools.GetNum(Request.QueryString["opf"].ToString());
                                }

                                #endregion

                                login.SetSessionVars();

                                //sauvegarde les cookies de connexion sur le serveur cible
                                if (bRememeber)
                                {
                                    eTools.SaveCookie("db", dbToken.BaseUid.ToString(), DateTime.MaxValue, Response);
                                    eTools.SaveCookie("UserLoginXrm", utToken.Login, DateTime.MaxValue, Response);
                                    eTools.SaveCookie("ts", stToken.GetTokenCrypted(), DateTime.MaxValue, Response, false);
                                    eTools.SaveCookie("rememberme", "1", DateTime.MaxValue, Response, false);
                                }

                                if (res == AUTH_USER_RES.SUCCESS)
                                    Response.Redirect("eMain.aspx");
                                else
                                {
                                    if (_isIntranet)
                                        _visIntranetStyle = "style='display:none;'";


                                    Session["SIVUT"] = eLibTools.GetToken(16);

                                    DBTokenFR.Value = dbToken.GetTokenCrypted();
                                    UserTokenFR.Value = utToken.GetTokenCrypted(Session["SIVUT"].ToString());
                                    SubscriberTokenFR.Value = stToken.GetTokenCrypted();

                                    HtmlGenericControl myIncludeVer = new HtmlGenericControl("script");
                                    myIncludeVer.Attributes.Add("type", "text/javascript");
                                    myIncludeVer.Attributes.Add("language", "javascript");
                                    myIncludeVer.InnerHtml = "authUserToken('" + dbToken.DbName + "' , '" + utToken.Login + "', '" + stToken.Login + "', '" + stToken.Pwd + "')";
                                    scriptHolder.Controls.Add(myIncludeVer);


                                    eCheckBoxCtrl chkSelRemm = new eCheckBoxCtrl((_rememberMe == "1"), false);
                                    chkSelRemm.AddClick("onChkRememberMe();");
                                    chkSelRemm.ToolTipChkBox = eResApp.GetRes(_nLangServ, 6205);
                                    chkSelRemm.AddText(eResApp.GetRes(_nLangServ, 6205));
                                    chkSelRemm.ID = "chkRememberMe";
                                    chkSelRemm.Style.Add(HtmlTextWriterStyle.Height, "18px");
                                    //chgChk
                                    divRememberMe.Controls.Add(chkSelRemm);

                                    //Remplir les langues
                                    FillPubDiv();
                                    FillLangDiv();
                                    FillFooterDiv();
                                    //Response.Redirect("eLogin.aspx");
                                }

                                return;
                            }
                        }
                    }
                }
                catch (eEndResponseException) { Response.End(); }
                catch (ThreadAbortException)
                {

                }
                catch
                {
                    // En cas d'erreur, la page de connexion normal est affichée

                }
            }
            #endregion

            _rememberMe = eTools.GetCookie("rememberme", Request, false);

            // Par défaut, à la première connexion, si aucun cookie n'est présent (chaîne vide), la case "Mémoriser mes identifiants" est cochée
            // Elle apparaîtra décochée au chargement de la page uniquement si l'utilisateur l'a réellement décochée (cookie existant avec valeur = "0")
            if (_rememberMe.Length == 0)
                _rememberMe = "1";

            // Si pas de mémorisation, on vide les cookies
            if (_rememberMe != "1")
            {
                // vide les cookies
                eTools.SaveCookie("ts", String.Empty, DateTime.MaxValue, Response, false);
                eTools.SaveCookie("db", String.Empty, DateTime.MaxValue, Response);
                eTools.SaveCookie("UserLoginXrm", String.Empty, DateTime.MaxValue, Response);
            }

            IsLocal = Request.IsLocal;
            // Chargement des RESSOURCES - Deplacé dans global.asax
            //eResApp.Load(_defaultInstanceName);



            #region Redirect HTTPS
            if ((eLibTools.GetServerConfig("httpsonly", "0") == "1") && !Request.IsSecureConnection)
            {


                //Si on vient déjà d'un redirect  
                if (Session["FromHTTPSRedirect"].ToString() == "1")
                {
                    Response.Write("Accès non sécurisé interdit");
                    Response.End();
                }


                Session["FromHTTPSRedirect"] = "1";

                Int32 nHTTSPort = eConst.DEFAULT_HTTPS_PORT;
                String sSecureURL = "#URLSCHEME#://#SERVERNAME##HTTPSPORT##ABSOLUTEURL#";


                if (!Int32.TryParse(eLibTools.GetServerConfig("SecureServerPort", eConst.DEFAULT_HTTPS_PORT.ToString()), out nHTTSPort))
                    nHTTSPort = eConst.DEFAULT_HTTPS_PORT;


                if (nHTTSPort != eConst.DEFAULT_HTTPS_PORT)
                    sSecureURL = sSecureURL.Replace("#URLSCHEME#", "http").Replace("#SERVERNAME#", Request.Url.Host).Replace("#HTTPSPORT#", String.Concat(":", nHTTSPort.ToString())).Replace("#ABSOLUTEURL#", Request.Url.AbsolutePath);
                else
                    sSecureURL = sSecureURL.Replace("#URLSCHEME#", "https").Replace("#SERVERNAME#", Request.Url.Host).Replace("#HTTPSPORT#", "").Replace("#ABSOLUTEURL#", Request.Url.AbsolutePath);


                //Si l'url sécurisé est différente de celle demandé, on redirige
                if (sSecureURL.ToLower() != Request.Url.OriginalString.ToLower())
                    Response.Redirect(sSecureURL);
                else
                {
                    Response.Write("Accès non sécurisé interdit");
                    Response.End();
                }
            }
            #endregion

            _visuUser = string.Empty;
            if (_isSSOApplication || _bIsADFSApplication)
                _visuUser = "style='display:none'";

            #region Gestion de la récupération des informations de connexions
            if (_isIntranet)
            {
                _visIntranetStyle = "style='display:none;'";
                _defSubscriberLogin = eLibConst.DEFAULT_USERNAME;
                _defSubscriberPassword = eLibConst.DEFAULT_USERPASSWORD;
                if (_rememberMe.Equals("1"))
                {
                    //vérifie le cookie de database
                    String sDBID = eTools.GetCookie("db", Request);

                    //Login USER
                    _defUserLogin = eTools.GetCookie("UserLoginXrm", Request);

                    if (_defUserLogin.Length > 0 && sDBID.Length > 0)
                        AutoConnect.Value = "1";
                    else
                        _defUserLogin = string.Empty;

                }
            }
            else
            {
                //Si "mémoriser mes identifiants" est coché
                // on essaye de charger les valeurs sauvées dans les cookies
                if (_rememberMe.Equals("1"))
                {
                    //Infos abonnés
                    String sSubsribersLogin = eTools.GetCookie("ts", Request, false);
                    if (sSubsribersLogin.Length > 0)
                    {
                        try
                        {
                            //Token abonné
                            SubscriberToken cSubscriberToken = new SubscriberToken();
                            if (!cSubscriberToken.LoadTokenCrypted(sSubsribersLogin))
                                throw new Exception("Token invalide");

                            _defSubscriberLogin = cSubscriberToken.Login;
                            _defSubscriberPassword = cSubscriberToken.Pwd;

                            //vérifie le cookoe de database
                            String sDBID = eTools.GetCookie("db", Request);

                            //Login USER
                            _defUserLogin = eTools.GetCookie("UserLoginXrm", Request);

                            //Flage la connection subscribers automatiques
                            if (sDBID.Length > 0 && _defUserLogin.Length > 0)
                                AutoConnect.Value = "1";
                            else
                                throw new Exception("Cookie de User ou Database invalide");
                        }
                        catch
                        {
                            //token invalide 

                        }
                    }



                }
                //SSO disponible uniquement en Intranet
                _isSSOApplication = false;
            }
            #endregion

            //vide les variables des session
            InitSessionVars();

            //Récupération de l'action
            string _action = string.Empty;

            // Perte de session
            if (Request.QueryString["err"] != null && Request.QueryString["err"].ToString() == "1")
            {
                errGlobal.Style["display"] = "block";
                labelErrGlobal.InnerHtml = eResApp.GetRes(_nLangServ, 6068);

            }

            eCheckBoxCtrl chkSelRem = new eCheckBoxCtrl((_rememberMe == "1"), false);
            chkSelRem.AddClick("onChkRememberMe();");
            chkSelRem.ToolTipChkBox = eResApp.GetRes(_nLangServ, 6205);
            chkSelRem.AddText(eResApp.GetRes(_nLangServ, 6205));
            chkSelRem.ID = "chkRememberMe";
            chkSelRem.Style.Add(HtmlTextWriterStyle.Height, "18px");
            //chgChk
            divRememberMe.Controls.Add(chkSelRem);

            //Remplir les langues
            FillPubDiv();
            FillLangDiv();
            FillFooterDiv();
        }

        void FillFooterDiv()
        {
            if (_isIntranet)
            {
                footerAskDemo.Attributes.Add("style", "display:none;");
                return;
            }

            string aHref;
            if (_nLangServ == 0)
                aHref = "https://www.eudonet.fr/eudonet-solutions/demonstration/";
            else
                aHref = "http://eudonet.co.uk/#openModal";

            HtmlGenericControl label = new HtmlGenericControl("div");
            label.ID = "textNotYetCust";
            label.InnerHtml = eResApp.GetRes(_nLangServ, 7628);

            HtmlGenericControl href = new HtmlGenericControl("a");
            href.ID = "button-demo";
            href.Attributes.Add("target", "_blank");
            href.Attributes.Add("href", aHref);
            href.InnerHtml = eResApp.GetRes(_nLangServ, 7629);

            footerAskDemo.Controls.Add(href);
            footerAskDemo.Controls.Add(label);
        }

        void FillPubDiv()
        {


            var t = eTools.LoadLoginPageInfos();


            //Texte de fond
            HtmlGenericControl p = new HtmlGenericControl("p")
            {
                InnerHtml = t.GetImgInfos(_langueServ).Text
            };
            pubDiv.Controls.Add(p);


            //Bouttons

            UrlInfos enUrlInf = t.GetBtnInfos(_langueServ);


            if (!(string.IsNullOrEmpty(enUrlInf.Text) || string.IsNullOrEmpty(enUrlInf.Url)))
            {
                HtmlGenericControl actuDiv = new HtmlGenericControl("div")
                {
                    InnerHtml = enUrlInf.Text
                };
                actuDiv.Attributes.Add("class", "button-rouge");
                actuDiv.Attributes.Add("onclick", "window.open('" + enUrlInf.Url + "','');");
                pubDiv.Controls.Add(actuDiv);
            }

            /*
            HtmlGenericControl img = new HtmlGenericControl("img");
            img.Attributes.Add("src", t.GetImgInfos(_langueServ).Url );
            img.Attributes.Add("width", "100%");
            img.Attributes.Add("alt", "");

            slogan.Controls.Add(img);
            */


            htmltag.Style.Add(HtmlTextWriterStyle.BackgroundImage, t.GetImgInfos(_langueServ).Url);
            htmltag.Style.Add("background", "no-repeat center fixed");
            htmltag.Style.Add("background-size", "cover;");
            htmltag.Style.Add("overflow", "hidden;");
        }

        void FillLangDiv()
        {
            HtmlGenericControl _ul = new HtmlGenericControl("ul");
            _ul.ID = "Flags";

            int nMaxLang = 5;
            for (int iLng = 0; iLng <= nMaxLang; iLng++)
            {
                HtmlGenericControl liLg = new HtmlGenericControl("li");
                liLg.ID = String.Concat("DIV_LANG_0", iLng);
                liLg.InnerHtml = GetLangName(iLng);
                liLg.Attributes.Add("class", "bt_flag");
                liLg.Attributes.Add("onclick", "setLang(this);");
                _ul.Controls.Add(liLg);

                HtmlGenericControl liSep = new HtmlGenericControl("li");
                liSep.InnerHtml = " | ";
                _ul.Controls.Add(liSep);
            }

            //Ajout d'un drapeau générique "6" - sans click
            HtmlGenericControl liGeneric = new HtmlGenericControl("li");
            liGeneric.ID = String.Concat("DIV_LANG_06");
            liGeneric.InnerHtml = "INTER";
            liGeneric.Attributes.Add("class", "bt_flag");
            liGeneric.Attributes.Add("onclick", "setLang(this);");
            _ul.Controls.Add(liGeneric);

            divLang.Controls.Add(_ul);
        }

        string GetLangName(int langIdx)
        {
            switch (langIdx)
            {
                case 0: return "FR";
                case 1: return "EN";
                case 2: return "DE";
                case 3: return "NL";
                case 4: return "ES";
                case 5: return "IT";
                default: return "";
            }
        }

        private void InitSessionVars()
        {
            Session["Pref"] = null;
        }


    }

}