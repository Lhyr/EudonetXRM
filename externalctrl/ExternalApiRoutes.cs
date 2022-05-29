using System.Web.Http;
using System.Web.Routing;

namespace Com.Eudonet.Xrm.externalctrl
{
    public static class ExternalApiRoutes
    {
        /// <summary>
        /// 
        /// </summary>
        public static string ExternalUrlPrefix { get { return "extapi"; } }
        /// <summary>
        /// 
        /// </summary>
        public static string ExternalUrlPrefixRelative { get { return "~/extapi"; } }

        /// <summary>
        /// Routage par défaut pour l'incorporation de la Web Api dans XRM.
        /// </summary>
        /// <param name="routes"></param>
        public static void Register(RouteCollection routes)
        {        
            routes.RouteExistingFiles = true;
        
            routes.MapHttpRoute(
                name: "ExternalApiORM",
                routeTemplate: ExternalUrlPrefix + "/orm",
                defaults: new { controller = "Orm" });

            routes.MapHttpRoute(
                name: "ExternalAuth",
                routeTemplate: "extranet/auth",
                defaults: new { controller = "ExtranetAuth" });

            routes.MapHttpRoute(
                name: "Import",
                routeTemplate: ExternalUrlPrefix + "/import",
                defaults: new { controller = "eImport"  });

            routes.MapHttpRoute(
                name: "Report",
                routeTemplate: ExternalUrlPrefix + "/report",
                defaults: new { controller = "eReport" });

            routes.MapHttpRoute(
                name: "Notification",
                routeTemplate: ExternalUrlPrefix + "/notif",
                defaults: new { controller = "eNotification" });
        }
    }
}