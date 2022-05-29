using EudoQuery;
using Newtonsoft.Json;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.mgr.external
{
    /// <summary>
    /// Description résumée de eStaticDicoList
    /// </summary>
    public class eStaticDicoList : IHttpHandler
    {

        /// <summary>
        /// Liste les données/consommation ram des dico static
        /// </summary>
        /// <param name="context"></param>
        public void ProcessRequest(HttpContext context)
        {
            if (context.Request.QueryString["use"] == "0")
                StaticBaseUseCache.BaseUseCache.UseCache = false;

            if (context.Request.QueryString["use"] == "1")
                StaticBaseUseCache.BaseUseCache.UseCache = true;

            // A utiliser rarement car lock tout le memorycache de l'application
            bool showmem = false;
            if (context.Request.QueryString["showmem"] == "1")
                showmem = true;

            // A utiliser uniquement en cas d'urgence
            if (context.Request.QueryString["resetall"] == "1")
                StaticBaseUseCache.BaseUseCache.ClearBaseCache("ALL");

            if (!string.IsNullOrEmpty(context.Request.QueryString["reset"]))
                StaticBaseUseCache.BaseUseCache.ClearBaseCache(context.Request.QueryString["reset"]);


            context.Response.Write("Cache globale : " + (StaticBaseUseCache.BaseUseCache.UseCache ? "activé" : "désactivé") + "<br/>");

            context.Response.Write("----------------------------<br/><br/> BASES : <br/>");
            foreach (var kvp in StaticBaseUseCache.BaseUseCache.CacheInfo.OrderBy(zz => zz.Key))
            {
                context.Response.Write(string.Format("{0} ; E2017 : {1} ; Cache disponible : {2} ; LastAdminAction : {3}; InvalideCollate status : {4}<br>",
                    kvp.Key, (kvp.Value.IsE2017 ? "activé" : "désactivé"), (kvp.Value.IsAvailable ? "oui" : "non")
                    , kvp.Value.LastAdminAction?.ToString()
                    , kvp.Value.InvalidCollationStatus.ToString()
                    ));
            }
            context.Response.Write("----------------------------<br/><br/>");

            if (showmem)
            {
                context.Response.Write("----------------------------<br/><br/> MemCache 2017  : <br/>");
                var t = eMemoryCache.GetCache().OrderBy(zz => zz.Key);
                foreach (var kvp in t)
                {
                    context.Response.Write(kvp.Key + " : " + JsonConvert.SerializeObject(kvp.Value, Newtonsoft.Json.Formatting.None) + "<br/>");
                }
                context.Response.Write("----------------------------<br/><br/>");
            }
        }

        /// <summary>
        /// False
        /// </summary>
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}