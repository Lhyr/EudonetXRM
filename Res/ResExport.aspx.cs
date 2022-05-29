using System;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Génère le fichier XML à partir des RES
    /// </summary>
    public partial class ResExport : System.Web.UI.Page
    {
        /// <summary>
        /// PageLoad
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            string errRes = "";
            string defaultInstanceName = ePrefTools.GetAppDefaultInstance();

            Response.Clear();
            Response.ClearContent();
            Response.ContentType = "text/xml";
            Response.Write(eResourcesManager.ResExport(defaultInstanceName, out errRes));
        }
    }
}