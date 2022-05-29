using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Threading;

namespace Com.Eudonet.Xrm.eda
{
    public partial class eAdminExtendedTargetMappingsDialog : eAdminPage
    {
        protected int _nTab;

        protected void Page_Load(object sender, EventArgs e)
        {
            PageRegisters.RegisterFromRoot = true;
            #region Ajout des css
            PageRegisters.AddCss("eMain");
            PageRegisters.AddCss("eButtons");
            PageRegisters.AddCss("eControl");
            PageRegisters.AddCss("eAdminFile");
            PageRegisters.AddCss("eAdminMenu");
            #endregion

            #region Ajout des js
            PageRegisters.AddScript("eTools");
            PageRegisters.AddScript("eMain");
            PageRegisters.AddScript("eUpdater");
            PageRegisters.AddScript("eModalDialog");
            #endregion

            _nTab = 0;

            HashSet<String> allKeys = new HashSet<String>(Request.Form.AllKeys);

            if (allKeys.Contains("tab") && !String.IsNullOrEmpty(Request.Form["tab"]))
                _nTab = eLibTools.GetNum(Request.Form["tab"].ToString());
            try
            {
                eAdminRenderer renderer = eAdminRendererFactory.CreateAdminExtendedTargetMappingsDialogRenderer(_pref, _nTab);
                if (renderer.ErrorNumber != EudoQuery.QueryErrorType.ERROR_NONE)
                {
                    if (renderer.InnerException != null)
                        throw renderer.InnerException;
                    else
                        throw new Exception(renderer.ErrorMsg);
                }

                formExtendedTargetMappings.Controls.Add(renderer.PgContainer);
            }
            catch (Exception exc)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 72),
                        eResApp.GetRes(_pref, 6236),
                        eResApp.GetRes(_pref, 72),
                        String.Concat("Erreur création du renderer dans eAdminExtendedTargetMappingsDialog : ", exc.Message)
                    );

                //Arrete le traitement et envoi l'erreur
                try
                {
                    LaunchError();
                }
                catch (eEndResponseException) { }
                catch (ThreadAbortException) { }
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