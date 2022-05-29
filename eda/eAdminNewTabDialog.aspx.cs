using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Boite de dialog création d'un nouvel onglet
    /// </summary>
    public partial class eAdminNewTabDialog : eAdminPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            #region Ajout des CSS
            PageRegisters.AddCss("eMain");
            PageRegisters.AddCss("eButtons");
            PageRegisters.AddCss("eAdminMenu");
            #endregion

            #region Ajout des JS
            #endregion

            try
            {
                eAdminRenderer renderer = eAdminRendererFactory.CreateAdminNewTabDialogRenderer(_pref);
                formAdminNewTab.Controls.Add(renderer.PgContainer);
            }
            catch (Exception exc)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 72),
                        eResApp.GetRes(_pref, 6236),
                        eResApp.GetRes(_pref, 72),
                        String.Concat("Erreur création du renderer dans eAdminNewTabDialog : ", exc.Message)
                    );

                //Arrete le traitement et envoi l'erreur
                LaunchError();
            }
            finally
            {
              
            }
        }

        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et scripts de celle-ci
        /// </summary>
        /// <returns>Retourne le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return scriptHolder;
        }
    }
}