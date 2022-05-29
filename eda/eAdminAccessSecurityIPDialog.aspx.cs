using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Com.Eudonet.Xrm.eda
{
    public partial class eAdminAccessSecurityIPDialog : eAdminPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            #region Ajout des css
            PageRegisters.AddCss("eMain");
            PageRegisters.AddCss("eButtons");
            PageRegisters.AddCss("eAdmin");
            PageRegisters.AddCss("eAdminMenu");
            #endregion

            #region Ajout de js
            #endregion

            try
            {
                int ipId = _requestTools.GetRequestFormKeyI("ipId") ?? -1;
                string fieldToFocus = _requestTools.GetRequestFormKeyS("fieldToFocus") ?? String.Empty;

                eAdminRenderer renderer = eAdminRendererFactory.CreateAdminAccessSecurityIPDialogRenderer(_pref, ipId, fieldToFocus);
                formAdminAccessSecurityIP.Controls.Add(renderer.PgContainer);
            }
            catch (Exception exc)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 72),
                        eResApp.GetRes(_pref, 6236),
                        eResApp.GetRes(_pref, 72),
                        String.Concat("Erreur création du renderer dans eAdminAccessSecurityIPDialog : ", exc.Message)
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