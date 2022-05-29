using System;
using System.Web.UI;

namespace Com.Eudonet.Xrm.test
{
    /// <summary>
    /// Classe de test d'appel CTI
    /// </summary>
    public partial class CTI : System.Web.UI.Page
    {
        /// <summary>
        /// Numéro à appeler
        /// </summary>
        public string PhoneNumber = string.Empty;

        /// <summary>
        /// Css spécifique au panel
        /// </summary>
        public string _css = string.Empty;

        /// <summary>
        /// Titre de la page
        /// </summary>
        public String PageTitle = "CTI";

        /// <summary>
        /// Chargement de la page
        /// </summary>
        protected void Page_Load(object sender, EventArgs e)
        {

            PhoneNumber = Request["pn"] ?? "";
            if (PhoneNumber.Length == 11 && PhoneNumber.StartsWith("33"))
                PhoneNumber = "0" + PhoneNumber.Substring(2);
        }

       
    }
}