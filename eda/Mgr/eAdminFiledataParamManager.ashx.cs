using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.eda.Mgr
{
    /// <summary>
    /// Description résumée de eAdminFiledataParamManager
    /// </summary>
    public class eAdminFiledataParamManager : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "texte/brut";
            context.Response.Write("Hello World");
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}