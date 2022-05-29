using System;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Page de Mise à jour des RES
    /// </summary>
    public partial class ResManu : System.Web.UI.Page
    {
        /// <summary>
        /// PageLoad
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            String errRes = String.Empty;
            String defaultInstanceName = ePrefTools.GetAppDefaultInstance();

            Response.Clear();
            Response.ClearContent();
            eResourcesManager.ResUpdate(defaultInstanceName, out errRes);
            if (String.IsNullOrEmpty(errRes))
            {
                Response.Clear();
                Response.Write("<strong>Mise à jour des ressources effectuée avec succès</strong>");
            }
            else
            {
                Response.Clear();
                Response.Write("<strong>Une erreur est survenue Lors de la mise à jour des ressources :</strong> <br>" + errRes);
            }
        }
    }
}