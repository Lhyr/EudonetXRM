using Com.Eudonet.Internal;
using System;
using System.Web.UI;

namespace Com.Eudonet.Xrm.eda
{
    public partial class eAdminFieldsRGPDListDialog : eAdminPage
    {
        public int Tab { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Ajout des css
            PageRegisters.AddCss("eList");
            PageRegisters.AddCss("eButtons");
            PageRegisters.AddCss("eAdmin");
            PageRegisters.AddCss("eAdminMenu");
            #endregion

            #region Ajout de js
            PageRegisters.AddScript("eMain");
            PageRegisters.AddScript("eList");
            PageRegisters.AddScript("eModalDialog");
            PageRegisters.AddScript("eUpdater");
            PageRegisters.RegisterAdminIncludeScript("eAdminFieldsList");
            PageRegisters.RegisterAdminIncludeScript("eAdminFieldsRGPDList");
            #endregion

            try
            {
                Tab = _requestTools.GetRequestFormKeyI("tab") ?? -1;

                eAdminRenderer renderer = eAdminRendererFactory.CreateAdminFieldsRGPDListDialogRenderer(_pref, Tab);
                formFieldsRGPDList.Controls.Add(renderer.PgContainer);
            }
            catch (Exception exc)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 72),
                        eResApp.GetRes(_pref, 6236),
                        eResApp.GetRes(_pref, 72),
                        String.Concat("Erreur création du renderer dans eAdminFieldsRGPDListDialog : ", exc.Message)
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