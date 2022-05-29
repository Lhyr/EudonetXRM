using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using System.Web.SessionState;
using Com.Eudonet.Internal;
using System.Diagnostics;
using System.Threading;
using System.IO;
using Com.Eudonet.Core.Model;
using System.Web.Http;
using Newtonsoft.Json;
using Com.Eudonet.Xrm.externalctrl;
using Com.Eudonet.Common.Cryptography;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Evénements de chargement, démarrage, déconnexion, etc... de l'application
    /// </summary>
    public class Global : System.Web.HttpApplication
    {

        private readonly List<string> lWhiteListFiles = new List<string>()
        {
            "edaDOSListCheck.ashx",
        };

        /// <summary>
        /// L' événement PostAuthorizeRequest indique que ASP.net a autorisé la requête actuelle.
        /// L’abonnement à l' événement PostAuthorizeRequest garantit l’authentification et l’autorisation
        /// de la demande avant le traitement du module attaché ou du gestionnaire d’événements.
        /// Ici il définit le type de comportement d'état de session requis afin de prendre en charge une requête HTTP.
        /// La lecture-écriture complet est activé pour la requête. Ce paramètre procède à une substitution,
        /// quel que soit le comportement de la session déterminé par l'inspection du gestionnaire pour la requête.
        /// </summary>
        protected void Application_PostAuthorizeRequest()
        {
            if (HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath.StartsWith(IRISBlack.XRMRouteConfig.UrlPrefixRelative)) {
                HttpContext.Current.SetSessionStateBehavior(SessionStateBehavior.Required);

                if(HttpContext.Current.Request.HttpMethod.Equals("GET"))
                    HttpContext.Current.SetSessionStateBehavior(SessionStateBehavior.ReadOnly);

            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Application_Start(object sender, EventArgs e)
        {
            /** Ajout du support pour les formdata dans la web api. */

           // GlobalConfiguration.Configuration.EnableCors();
            GlobalConfiguration.Configuration.Formatters.Add(new IRISBlack.Model.Formatter.FormMultipartEncodedMediaTypeFormatter());

            /* Routage par défaut pour l'incorporation de la Web Api dans XRM. */
            Com.Eudonet.Xrm.IRISBlack.XRMRouteConfig.Register(RouteTable.Routes);
            ExternalApiRoutes.Register(RouteTable.Routes);
            

            /* Par défaut, on indente le JSON et on enlève les valeurs null */
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings { 
            //    Formatting = Formatting.Indented, 
                NullValueHandling = NullValueHandling.Ignore,
               // DefaultValueHandling = DefaultValueHandling.Ignore
            };
            

            /* if (!EventLog.SourceExists("E2017"))
                 EventLog.CreateEventSource("E2017", "Eudonet");
                 */
            string sqlUser = eLibTools.GetServerConfig("EudoLogin", eLibConst.DEFAULT_USERNAME);
            string sqlPassword = eLibTools.GetServerConfig("EudoPassword", "");


            if (sqlPassword.Length > 0)
                sqlPassword = CryptoEudonet.Decrypt(sqlPassword, CryptographyConst.KEY_CRYPT_LINK6, true);
            else
                sqlPassword = eLibConst.DEFAULT_USERPASSWORD;

            eResApp.Load(new ePrefSQL(ePrefTools.GetAppDefaultInstance(), "EUDORES", sqlUser, sqlPassword, ePrefConst.XRM_SQL_APPLICATIONNAME));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Session_Start(object sender, EventArgs e)
        {
            Session["FromHTTPSRedirect"] = "0";
            Session["SubscriberConnections"] = 0;
            Session["UserConnections"] = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Application_BeginRequest(object sender, EventArgs e)
        {

            string sClientIP = "";
            string sF = "main.aspx";
            try
            {

                try
                {
                    sClientIP = eLibTools.GetUserIPV4();
                    sF = Path.GetFileName(Request.PhysicalPath);
                }
                catch
                {
                    sClientIP = " Ip non disponible";

                }


                if (!lWhiteListFiles.Contains(sF, StringComparer.OrdinalIgnoreCase)
                    && !eLibConst.ADR_IP_EUDOWEB.ContainsKey(sClientIP)
                    )
                {

                    int nb = eDoSProtection.CheckError(true);

                }
            }
            catch (eEudoDoSException) { throw; }
            catch (Exception ee)
            {
                try
                {
                    EventLog.WriteEntry("Eudonet", "Ip Client : " + sClientIP + Environment.NewLine + "Erreur :" + Environment.NewLine + ee.Message, EventLogEntryType.Error);
                }
                catch { }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        private void application_EndRequest(object sender, EventArgs e)
        {

            // catch de l'erreur de taille maximum pour l'upload de fichiers
            //HttpRequest request = HttpContext.Current.Request;
            //HttpResponse response = HttpContext.Current.Response;


            //if ((request.HttpMethod == "POST") &&
            //    (response.StatusCode == 404 && response.SubStatusCode == 13))
            //{
            // TODO GESTION D'erreur en cas de fichier trop volumineux
            //// Clear the response header but do not clear errors and transfer back to requesting page to handle error
            ////response.ClearHeaders();
            //response.StatusCode = 200;

            //response.Redirect("ePjAddFromTpl.aspx?err=1");
            //  }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Session_End(object sender, EventArgs e)
        {
            try
            {
                eLoginOL.LogLogout();
            }
            catch
            {
            }

        }

 


        /// <summary>
        /// Gestion des erreurs applicative non gérée bycode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Application_Error(object sender, EventArgs e)
        {
            Exception exc = Server.GetLastError();

            //Erreur "fin d page"
            if (exc is eEndResponseException || exc is ThreadAbortException)
            {
                Server.ClearError();
                return;
            }

            //404 aspx
            if (exc is HttpException)
            {
                int httpCode = ((HttpException)exc).GetHttpCode();

                if (httpCode == 404)
                {

                    Response.StatusCode = 404;
                    Response.SubStatusCode = 0;
                    Response.TrySkipIisCustomErrors = true;
                    Server.ClearError();
                    
                    Server.Transfer("~/customError/404.html");
                    
                }
                
                return;
            }

            try
            {
                string _sClientIP = "";
                try
                {
                    _sClientIP = eLibTools.GetUserIPV4();
                }
                catch
                {
                    _sClientIP = " Ip non disponible";
                }



                Exception myExc = exc;
                if (exc.InnerException != null)
                    myExc = exc.InnerException;


                EventLog.WriteEntry("Eudonet", "IP :" + _sClientIP + Environment.NewLine + "Error : " + myExc.Message, EventLogEntryType.Error);

                try
                {
                    //Trop d'erreurs consécutives
                    if (eDoSProtection.CheckError() <= 0)
                    {
                        Response.Clear();
                        Response.Write("To Many Errors");
                        Response.End();
                        EventLog.WriteEntry("Eudonet", "IP BlackListe", EventLogEntryType.Error);
                    }


                }
                catch (eEudoDoSException)
                {
                    //DDOS (via check erreur)

                    EventLog.WriteEntry("Eudonet", "IP BlackListe", EventLogEntryType.Error);

                    //Blackliste l'ip
                    Response.Clear();
                    Response.Write("To Many Errors");
                    Response.End();

                }

            }
            catch (eEndResponseException) { }
            catch (ThreadAbortException) { }
            catch (Exception)
            {

            }
        }

    }

}