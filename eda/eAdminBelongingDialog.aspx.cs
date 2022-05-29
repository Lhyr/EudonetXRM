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
    /// Fenêtre modal appartenance de la fiche (admin)
    /// </summary>
    public partial class eAdminBelongingDialog : eAdminPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            #region Ajout des CSS / Scripts
            
            PageRegisters.AddCss("eList");
            PageRegisters.AddCss("eAdmin");
            PageRegisters.AddCss("eAdminMenu");
            PageRegisters.AddScript("eTools");
            PageRegisters.AddScript("eMain");
            PageRegisters.AddScript("eUpdater");
            PageRegisters.AddScript("eList");
            PageRegisters.AddScript("eModalDialog");
            PageRegisters.AddScript("eAdminFile");
            PageRegisters.RegisterAdminIncludeScript("eAdminBelonging");
            #endregion

            #region Paramètres
            int nTab = 0;
            int nWidth = 985;
            int nHeight = 650;

            if (Request.Form.AllKeys.Contains("tab") && !String.IsNullOrEmpty(Request.Form["tab"]))
                nTab = eLibTools.GetNum(Request.Form["tab"].ToString());

            if (Request.Form.AllKeys.Contains("w") && !String.IsNullOrEmpty(Request.Form["w"]))
                nWidth = eLibTools.GetNum(Request.Form["w"].ToString());

            if (Request.Form.AllKeys.Contains("h") && !String.IsNullOrEmpty(Request.Form["h"]))
                nHeight = eLibTools.GetNum(Request.Form["h"].ToString());


            #endregion


            try
            {
                eAdminRenderer renderer = eAdminRendererFactory.CreateAdminBelongingDialogRenderer(_pref, nTab, nWidth, nHeight);
                if (renderer.ErrorNumber != EudoQuery.QueryErrorType.ERROR_NONE)
                {
                    if (renderer.InnerException != null)
                        throw renderer.InnerException;
                    else
                        throw new Exception(renderer.ErrorMsg);


                }

                formAdminBelonging.Controls.Add(renderer.PgContainer);
            }
            catch (Exception exc)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 72),
                        eResApp.GetRes(_pref, 6236),
                        eResApp.GetRes(_pref, 72),
                        String.Concat("Erreur création du renderer dans eAdminBelongingDialog : ", exc.Message)
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