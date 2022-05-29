using Com.Eudonet.Internal;
using System;
using System.Web.UI;

namespace Com.Eudonet.Xrm.eda
{
    public partial class eAdminRightsTreatmentDialog : eAdminPage
    {
        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            #region Ajout des css
            PageRegisters.AddCss("eMain");
            PageRegisters.AddCss("eButtons");
            PageRegisters.AddCss("eAdminMenu");
            #endregion

            #region Ajout de js
            PageRegisters.AddScript("eModalDialog");
            PageRegisters.RegisterAdminIncludeScript("eAdminRights");
            #endregion

            try
            {
                eAdminRenderer renderer = eAdminRendererFactory.CreateAdminRightsTreatmentDialogRenderer(_pref);
                formAdminRightsTreatment.Controls.Add(renderer.PgContainer);
            }
            catch (Exception exc)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 72),
                        eResApp.GetRes(_pref, 6236),
                        eResApp.GetRes(_pref, 72),
                        String.Concat("Erreur création du renderer dans eAdminRightsTreatmentDialog : ", exc.Message)
                    );

                //Arrete le traitement et envoi l'erreur
                LaunchError();
            }
            finally
            {

            }
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