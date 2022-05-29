using System;
using System.Web.UI;
using Com.Eudonet.Internal;
using EudoQuery;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Autocomplétion sur addresse
    /// </summary>
    public partial class eAdminAutocompleteAddress : eAdminPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            #region Ajout des CSS / Scripts
            PageRegisters.RegisterFromRoot = true;
            PageRegisters.AddScript("eTools");
            PageRegisters.AddScript("eMain");
            PageRegisters.AddScript("eUpdater");
            PageRegisters.AddScript("eModalDialog");
            //PageRegisters.AddScript("eAdminAutocompleteAddress");
            PageRegisters.RegisterAdminIncludeScript("eAdminAutocompleteAddress");
            #endregion

            #region Vérification niveau de l'utilisateur
            if (_pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
            {
                ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.EXCLAMATION, eResApp.GetRes(0, 6441), eResApp.GetRes(0, 6342));
                LaunchErrorHTML(true, ErrorContainer);
            }
            else
            {
                // Affichage
            }
            #endregion
        }

        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return scriptHolder;
        }
    }
}