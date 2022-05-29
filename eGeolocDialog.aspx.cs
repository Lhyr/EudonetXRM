using Com.Eudonet.Internal;
using System;
using System.Web.UI;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Modale de saisie d'un champ Géo
    /// </summary>
    /// <seealso cref="Com.Eudonet.Xrm.eEudoPage" />
    public partial class eGeolocDialog : eEudoPage
    {
        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return scriptHolder;
        }

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            PageRegisters.AddScript("eTools");
            PageRegisters.AddScript("bingmaps/eBingInterface");

            PageRegisters.AddCss("eGeoloc");

            // Clé bing maps
            accessKey.Value = eLibConst.BING_MAPS_KEY;
            // WKT existant
            wkt.Value = _requestTools.GetRequestFormKeyS("wkt") ?? string.Empty;

            intro.InnerText = eResApp.GetRes(_pref, 8750);
        }

    }
}