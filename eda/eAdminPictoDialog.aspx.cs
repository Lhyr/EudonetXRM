using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Com.Eudonet.Xrm.eda
{
    public partial class eAdminPictoDialog : eAdminPage
    {
        


        protected void Page_Load(object sender, EventArgs e)
        {
            #region Ajout des CSS / Scripts
            PageRegisters.AddCss("eudoFont");
            PageRegisters.AddCss("eColorPictoPicker");


            #endregion

            #region Paramètres
            int nTab = 0;
            int nDescId = 0;

            if (Request.Form.AllKeys.Contains("tab") && !String.IsNullOrEmpty(Request.Form["tab"]))
                nTab = eLibTools.GetNum(Request.Form["tab"].ToString());

            if (Request.Form.AllKeys.Contains("descid") && !String.IsNullOrEmpty(Request.Form["descid"]))
                nDescId = eLibTools.GetNum(Request.Form["descid"].ToString());

            string color = _requestTools.GetRequestFormKeyS("color");         
            eFontIcons.FontIcons icon = eFontIcons.GetFontIcon(_requestTools.GetRequestFormKeyS("iconkey"));
            

            #endregion


            try
            {
                eAdminRenderer renderer = eAdminRendererFactory.CreateAdminPictoDialogRenderer(_pref, nTab, nDescId, icon, color);
                if (renderer.ErrorNumber != EudoQuery.QueryErrorType.ERROR_NONE)
                {
                    if (renderer.InnerException != null)
                        throw renderer.InnerException;
                    else
                        throw new Exception(renderer.ErrorMsg);


                }

                formAdminPicto.Controls.Add(renderer.PgContainer);
            }
            catch (Exception exc)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 72),
                        eResApp.GetRes(_pref, 6236),
                        eResApp.GetRes(_pref, 72),
                        String.Concat("Erreur création du renderer dans eAdminPictoDialog : ", exc.Message)
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