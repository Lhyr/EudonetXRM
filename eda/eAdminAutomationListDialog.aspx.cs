
using Com.Eudonet.Engine.Notif;
using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Fenetre liste des automatisme (admin)
    /// </summary>
    public partial class eAdminAutomationListDialog : eAdminPage
    {

        // DescId de la table
        Int32? nTab = 0;

        // Rubrique ciblée par l'automatisme
        Int32? nField = 0;

        // DescId de la table
        Int32? nWidth = 0;

        // Rubrique ciblée par l'automatisme
        Int32? nHeight = 0;

        // Type de l'automatisme
        AutomationType automationType = AutomationType.ALL;

        protected void Page_Load(object sender, EventArgs e)
        {
            #region ajout des css
            PageRegisters.AddCss("eMain");
            PageRegisters.AddCss("eAdmin");
            PageRegisters.AddCss("eList");
            PageRegisters.AddCss("eIcon");
            PageRegisters.AddCss("eControl");
            PageRegisters.AddCss("eTitle");
            PageRegisters.AddCss("eActions");
            PageRegisters.AddCss("eContextMenu");
            PageRegisters.AddCss("eActionList");


            if (Request.Browser.MajorVersion == 7 && Request.Browser.Browser == "IE")
                PageRegisters.AddCss("ie7-styles");

            #endregion

            #region add js
            PageRegisters.AddScript("eMain");
            PageRegisters.AddScript("eButtons");
            PageRegisters.AddScript("eList");
            PageRegisters.AddScript("ePopup");
            PageRegisters.AddScript("eTools");
            PageRegisters.AddScript("eUpdater");
            PageRegisters.AddScript("eExpressFilter");
            PageRegisters.AddScript("eContextMenu");
            PageRegisters.AddScript("eModalDialog");
            PageRegisters.AddScript("eEvent");
            PageRegisters.RegisterAdminIncludeScript("eAdmin");
            PageRegisters.RegisterAdminIncludeScript("eAdminAutomationList");
            #endregion

            try
            {
                #region Paramètres

                nTab = _requestTools.GetRequestFormKeyI("tab") ?? 0;
                nField = _requestTools.GetRequestFormKeyI("field") ?? 0;
                nWidth = _requestTools.GetRequestFormKeyI("width") ?? eConst.DEFAULT_WINDOW_WIDTH;
                nHeight = _requestTools.GetRequestFormKeyI("height") ?? eConst.DEFAULT_WINDOW_HEIGHT;
                automationType = (AutomationType)(_requestTools.GetRequestFormKeyI("type") ?? 0);

                #endregion


                eRenderer renderer = eAdminRendererFactory.CreateAdminAutomationListDialogRenderer(_pref, nTab.Value, nField.Value, nWidth.Value, nHeight.Value, 1, automationType);
                if (renderer.ErrorNumber != EudoQuery.QueryErrorType.ERROR_NONE)
                {
                    if (renderer.InnerException != null)
                        throw new Exception(renderer.ErrorMsg + " " + renderer.ErrorNumber.ToString(), renderer.InnerException); 
                    else
                        throw new Exception(renderer.ErrorMsg + " " + renderer.ErrorNumber.ToString());
                }

                // Rendu de la liste
                listContent.Controls.Add(renderer.PgContainer);
            }
            catch (Exception exc)
            {

#if DEBUG
                ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, exc.Message, exc.StackTrace, eResApp.GetRes(_pref, 72), exc.StackTrace);
#else
        ErrorContainer = eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 72),
                        eResApp.GetRes(_pref, 6236),
                        eResApp.GetRes(_pref, 72),
                        String.Concat("Erreur création du renderer dans eAdminAutomationListDialog : ", exc.StackTrace));      

#endif

                //Arrete le traitement et envoi l'erreur
                LaunchError();
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
