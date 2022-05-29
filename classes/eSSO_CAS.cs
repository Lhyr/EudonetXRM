using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;

namespace Com.Eudonet.Xrm.sso
{
    /// <className>eSSO_CAS</className>
    /// <summary>Classe d'utilisation du SSO de type CAS</summary>
    /// <purpose></purpose>
    /// <authors>GCH</authors>
    /// <date>2015-02-18</date>
    public class eSSO_CAS
    {
        /// <summary>Objet d'informations de connexion à la Base de donnée</summary>
        private ePrefSQL _prefSQL = null;
        /// <summary>Information de base de donnée sélectionnée</summary>
        private DbTokenGeneric _cDbToken = null;
        /// <summary>Objet de page</summary>
        private HttpRequest _httpRequest = null;
        /// <summary>Indique si le SSO est actif</summary>
        public Boolean Enabled { get; set; }
        /// <summary>Url du base du SSO CAS</summary>
        public String Url { get; private set; }
        /// <summary>Url de login du SSO CAS</summary>
        public String UrlLogin { get; private set; }
        /// <summary>Url de validation d'un ticket du SSO CAS</summary>
        public String UrlServiceValidate { get; private set; }
        /// <summary>Url vers laquelle le SSO va rediriger</summary>
        public String UrlReturnXRM { get; private set; }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="prefSQL">Objet d'informations de connexion à la Base de donnée</param>
        /// <param name="cDbToken">Information de base de donnée sélectionnée</param>
        /// <param name="httpRequest">Objet de page</param>
        public eSSO_CAS(ePrefSQL prefSQL, DbTokenGeneric cDbToken, HttpRequest httpRequest)
        {
            _httpRequest = httpRequest;
            _cDbToken = cDbToken;

            _prefSQL = prefSQL != null ? prefSQL : eLoginOL.GetBasePrefSQL();    //Si pref null on met le pref de base
            _prefSQL = new ePrefSQL(cDbToken.SqlServerInstanceName, cDbToken.DbDirectory, _prefSQL.GetSqlUser, _prefSQL.GetSqlPassword, _prefSQL.GetSqlApplicationName);

            Enabled = false;
            Url = String.Empty;

            UrlReturnXRM = cDbToken.AppUrl ?? String.Empty;
            if (UrlReturnXRM.Length <= 0)
                UrlReturnXRM = httpRequest.Path;
            if (UrlReturnXRM[UrlReturnXRM.Length - 1] != '/')
                UrlReturnXRM = UrlReturnXRM = String.Concat(UrlReturnXRM, '/');
        }

        /// <summary>
        /// Objet qui permet d'initialiser 
        /// </summary>
        /// <param name="sError">erreur de retour si Init renvoi faux</param>
        /// <returns>Vrai si initialisation sans problème et Faux si initialisation en erreur</returns>
        public Boolean Init(out String sError)
        {
            sError = String.Empty;

            #region Vérif SSO CAS
            try
            {
                IDictionary<eLibConst.CONFIGADV, String> dicConf = eLibTools.GetConfigAdvValues(_prefSQL,
                    new HashSet<eLibConst.CONFIGADV> {
                        eLibConst.CONFIGADV.SSO_CAS, eLibConst.CONFIGADV.SSO_CAS_URL
                    });
                Enabled = dicConf[eLibConst.CONFIGADV.SSO_CAS] == "1";
                Url = dicConf[eLibConst.CONFIGADV.SSO_CAS_URL];
                if (Enabled && Url.Trim().Length == 0) Enabled = false;
                if (Enabled)
                {
                    if (Url[Url.Length - 1] != '/')
                        Url = String.Concat(Url, '/');

                    UrlLogin = String.Concat(Url, "login");
                    UrlServiceValidate = String.Concat(Url, "serviceValidate");
                }
                //UrlReturnXRM = "http://eudonet.dauphine.fr/xrm/";   //TODO  pour debug
            }
            catch (Exception ex)
            {
                sError = ex.ToString();
                return false;
            }
            #endregion

            return true;
        }

        /// <summary>
        /// Valide que le ticket retourné par le client est valide puis s'il est valide retourne true et le login CAS de l'utilisateur
        /// Si le ticket n'est pas valide, retourne faux et l'erreur qui s'est produite
        /// </summary>
        /// <param name="sCasTicket">Ticket retourné par le SSO CAS (attention il ne peut être validé qu'une fois)</param>
        /// <param name="sReturn">UserLogin si ticket valide ou l'erreur si ticket non valide</param>
        /// <returns>VRAI si ticket valide et FAUX si ticket non valide</returns>
        public Boolean GetUserLogin_SSOInformation(String sCasTicket, out String sReturn)
        {
            sReturn = String.Empty;

            //Attention il faut que le paramètre service (url ayant demandé le ticket au CAS)
            //  soit identique en comptant tous les querystring
            StringBuilder sTrueUrlRefer = new StringBuilder();
            foreach (String s in _httpRequest.UrlReferrer.Query.ToString().Split('&'))
            {
                if (!s.StartsWith("ticket"))
                    sTrueUrlRefer.Append(sTrueUrlRefer.Length == 0 ? "" : "&").Append(s);
            }
            sTrueUrlRefer.Insert(0, UrlReturnXRM);

            sTrueUrlRefer = new StringBuilder(HttpUtility.UrlEncode(sTrueUrlRefer.ToString()));
            //Rempalce les caractères encodés en ascii par des majuscule car côté Javascript la méthode encodeURIComponent encode avec des majuscules
            for (int i = 255; i > 0; i--)   //En décrémentant pour commencer avec les encryptions sur 2 caractères en premier.
            {
                String sCharToRep = String.Format("%{0:X}", i);
                sTrueUrlRefer.Replace(sCharToRep.ToLower(), sCharToRep.ToUpper());
            }
            String result = String.Empty;
            String url = String.Concat(UrlServiceValidate, "?ticket=", sCasTicket, "&service=", sTrueUrlRefer.ToString());

            try
            {
                XmlDocument xmlDoc = eLibTools.GetWebData(url);

                if (ConfigurationManager.AppSettings.Get("LogCASResponse") == "1")
                    eModelTools.EudoTraceLog(String.Concat(DateTime.Now, " : Authentification SSO CAS", Environment.NewLine, "Réponse du CAS : ", Environment.NewLine, xmlDoc.OuterXml));


                XmlNamespaceManager ns = new XmlNamespaceManager(xmlDoc.NameTable);
                ns.AddNamespace("cas", "http://www.yale.edu/tp/cas");

                XmlNode mainNode = xmlDoc.FirstChild;
                XmlNode nodeSucces = mainNode.SelectSingleNode("./cas:authenticationSuccess", ns);
                XmlNode nodeUserName = mainNode.SelectSingleNode("./cas:authenticationSuccess/cas:user", ns);
                if (nodeSucces != null && nodeUserName != null)
                {
                    sReturn = nodeUserName.InnerText; //Ticket valide : on retourne le userlogin
                    return true;
                }
                else
                {
                    //Ticket non valide
                    XmlNode nodeFailed = mainNode.SelectSingleNode("./cas:authenticationFailure", ns);
                    if (nodeFailed != null)
                    {
                        sReturn = HttpUtility.HtmlEncode(nodeFailed.InnerText); //on retourne l'erreur renvoyé par le SSO
                        eModelTools.EudoTraceLog(String.Format("===> {1} SSO non Validé  :{0}\turl referer : {2}{0}\tretourCAS : {3}{0}", Environment.NewLine, DateTime.Now, sTrueUrlRefer, xmlDoc.OuterXml));
                    }
                    else
                    {
                        //Pas d'erreur renvoyée par le SSO, on log le contenu qui a été renvoyé par le SSO
                        eModelTools.EudoTraceLog(String.Format("===> {1} SSO non Validé  :{0}\turl referer : {2}{0}\tretourCAS : {3}{0}", Environment.NewLine, DateTime.Now, sTrueUrlRefer, xmlDoc.OuterXml));
                        sReturn = "failed"; //on retourne une erreur
                    }
                    return false;   //erreur de validation du ticket
                }

            }
            catch (FileNotFoundException e)
            {
                sReturn = "Failed to locate XML";
                eModelTools.EudoTraceLog(String.Concat(DateTime.Now, " - ", sReturn, " : ", e.ToString()));
                return false;
            }
            catch (DirectoryNotFoundException e)
            {
                sReturn = "Failed to locate XML";
                eModelTools.EudoTraceLog(String.Concat(DateTime.Now, " - ", sReturn, " : ", e.ToString()));
                return false;
            }
            catch (WebException e)
            {
                sReturn = "Problem accessing the resource - could be a network problem";
                eModelTools.EudoTraceLog(String.Concat(DateTime.Now, " - ", sReturn, " : ", e.ToString()));
                return false;
            }
            catch (UriFormatException e)
            {
                sReturn = "The URL is not properly formed";
                eModelTools.EudoTraceLog(String.Concat(DateTime.Now, " - ", sReturn, " : ", e.ToString()));
                return false;
            }
            catch (XmlException e)
            {
                sReturn = "The XML is malformed";
                eModelTools.EudoTraceLog(String.Concat(DateTime.Now, " - ", sReturn, " : ", e.ToString()));
                return false;
            }
            catch (InvalidOperationException e)
            {
                sReturn = "Invalid Operation";
                eModelTools.EudoTraceLog(String.Concat(DateTime.Now, " - ", sReturn, " : ", e.ToString()));
                return false;
            }
        }


    }
}