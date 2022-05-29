using Com.Eudonet.Internal;
using System;
using System.Web.UI;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// eAdminSynchroExchangeMappingDialog
    /// </summary>
    /// <seealso cref="Com.Eudonet.Xrm.eda.eAdminPage" />
    public partial class eAdminSynchroExchangeMappingDialog : eAdminPage
    {
        /// <summary>
        /// Page load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="Exception"></exception>
        protected void Page_Load(object sender, EventArgs e)
        {
            PageRegisters.RegisterFromRoot = true;
            #region Ajout des css
            PageRegisters.AddCss("eMain");
            PageRegisters.AddCss("eButtons");
            PageRegisters.AddCss("eControl");
            PageRegisters.AddCss("eAdminMenu");
            PageRegisters.AddCss("eAdminExtensions");
            #endregion

            #region Ajout des js
            PageRegisters.AddScript("eTools");
            PageRegisters.AddScript("eMain");
            PageRegisters.AddScript("eUpdater");
            PageRegisters.AddScript("eModalDialog");
            #endregion

            int tab = _requestTools.GetRequestFormKeyI("tab") ?? 0;

            try
            {
                eAdminRenderer renderer = eAdminRendererFactory.CreateAdminSynchroExchangeMappingDialogRenderer(_pref, tab);
                if (renderer.ErrorNumber != EudoQuery.QueryErrorType.ERROR_NONE)
                {
                    if (renderer.InnerException != null)
                        throw renderer.InnerException;
                    else
                        throw new Exception(renderer.ErrorMsg);
                }

                formAdminSynchroExchange.Controls.Add(renderer.PgContainer);
            }
            catch (Exception exc)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 72),
                        eResApp.GetRes(_pref, 6236),
                        eResApp.GetRes(_pref, 72),
                        String.Concat("Erreur création du renderer dans eAdminSynchroExchangeMappingDialog : ", exc.Message)
                    );

                //Arrete le traitement et envoi l'erreur
                try
                {
                    LaunchError();
                }
                catch (eEndResponseException) { }
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