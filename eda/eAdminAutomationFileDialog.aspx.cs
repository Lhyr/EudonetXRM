
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
    public partial class eAdminAutomationFileDialog : eAdminPage
    {

        // DescId de la table sur laquelle l'automatisme est appliqué
        Int32? nTab = 0;

        // DescId de la rubrique sur laquelle l'automatisme est appliqué
        Int32? nField = 0;

        // id de l'automatismes
        Int32? nFileId= 0;

        //Id de la modal
        String sIdModal;

        // Type de l'automatisme : Notification, Rappel Workflow..etc
        AutomationType autoType = AutomationType.ALL;

        // DescId de la table
        Int32? nWidth = 0;

        // Rubrique ciblée par l'automatisme
        Int32? nHeight = 0;

        // Type de l'automatisme
        AutomationType automationType = AutomationType.ALL;

        protected void Page_Load(object sender, EventArgs e)
        {

            #region ajout des css 
            PageRegisters.AddCss("eAdminFile");
            PageRegisters.AddCss("eAdmin");           
            PageRegisters.AddCss("eAdminMenu");
            PageRegisters.AddCss("eFile");
            PageRegisters.AddCss("eIcon");
            PageRegisters.AddCss("eControl");
            PageRegisters.AddCss("eTitle");
            PageRegisters.AddCss("eActions");
            PageRegisters.AddCss("eContextMenu");
            PageRegisters.AddCss("eMemoEditor");


            if (Request.Browser.MajorVersion == 7 && Request.Browser.Browser == "IE")
                PageRegisters.AddCss("ie7-styles");

            #endregion

            #region add js
            PageRegisters.AddScript("eMain");
            PageRegisters.AddScript("eButtons");
            PageRegisters.AddScript("eFile");
            PageRegisters.AddScript("ePopup");
            PageRegisters.AddScript("eTools");
            PageRegisters.AddScript("eUpdater");
            PageRegisters.AddScript("eExpressFilter");           
            PageRegisters.AddScript("eContextMenu");
            PageRegisters.AddScript("eModalDialog");
            PageRegisters.AddScript("eEvent");           
            PageRegisters.AddScript("ckeditor/ckeditor");
            PageRegisters.AddScript("eGrapesJSEditor");
            PageRegisters.AddScript("eMemoEditor");
            PageRegisters.RegisterAdminIncludeScript("eAdmin");
            PageRegisters.RegisterAdminIncludeScript("eAdminPicto");
            PageRegisters.RegisterAdminIncludeScript("eAdminAutomationFile");
            PageRegisters.RegisterAdminIncludeScript("eAdminConditions");

            #endregion

            try
            {

                nTab = _requestTools.GetRequestFormKeyI("tab") ?? 0;
                nField = _requestTools.GetRequestFormKeyI("field") ?? 0;
                nFileId = _requestTools.GetRequestFormKeyI("id") ?? 0;
                nWidth = _requestTools.GetRequestFormKeyI("width") ?? eConst.DEFAULT_WINDOW_WIDTH;
                nHeight = _requestTools.GetRequestFormKeyI("height") ?? eConst.DEFAULT_WINDOW_HEIGHT;
                automationType = (AutomationType)(_requestTools.GetRequestFormKeyI("type") ?? 0);
                sIdModal = _requestTools.GetRequestFormKeyS("_parentiframeid").Substring("frm_".Length);
              
                eRenderer renderer = eAdminRendererFactory.CreateAdminAutomationFileRenderer(_pref, nTab.Value, nField.Value, nFileId.Value, nWidth.Value, nHeight.Value,  automationType, sIdModal);
                if (renderer.ErrorNumber != EudoQuery.QueryErrorType.ERROR_NONE)
                {
                    if (renderer.InnerException != null)
                        throw new Exception(renderer.ErrorMsg + " " + renderer.ErrorNumber.ToString(), renderer.InnerException);  
                    else
                        throw new Exception(renderer.ErrorMsg + " " + renderer.ErrorNumber.ToString());
                }

                // Rendu de la popup              
                fileContent.Controls.Add(renderer.PgContainer);

                //Ajout du callback script
                PageRegisters.RawScrip.AppendLine(renderer.GetCallBackScript);
            }
            catch (Exception exc)
            {

#if DEBUG
                ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), exc.Message, eResApp.GetRes(_pref, 6236), exc.StackTrace);
#else
        ErrorContainer = eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 72),
                        eResApp.GetRes(_pref, 6236),
                        eResApp.GetRes(_pref, 72),
                        String.Concat("Erreur création du renderer dans eAdminAutomationListDialog : ", exc.StackTrace));      

#endif

                //Arrete le traitement et envoi l'erreur
                try
                {
                    LaunchError();
                }
                catch (eEndResponseException) { }
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
 