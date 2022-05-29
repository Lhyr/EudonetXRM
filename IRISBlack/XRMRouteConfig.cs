using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace Com.Eudonet.Xrm.IRISBlack
{
    /// <summary>
    /// Classe statique pour la configuration des routes de la Web API
    /// </summary>
    public static class XRMRouteConfig
    {
        /// <summary>
        /// 
        /// </summary>
        public static string UrlPrefix { get { return "api"; } }
        /// <summary>
        /// 
        /// </summary>
        public static string UrlPrefixRelative { get { return "~/api"; } }


 

        /// <summary>
        /// Routage par défaut pour l'incorporation de la Web Api dans XRM.
        /// </summary>
        /// <param name="routes"></param>
        public static void Register(RouteCollection routes)
        {
            routes.RouteExistingFiles = true;

            /** Pour la route. Kampaï */
            routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: UrlPrefix + "/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional });

        }

    }
}