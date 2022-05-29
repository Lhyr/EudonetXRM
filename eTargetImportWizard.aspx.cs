using Com.Eudonet.Internal;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Data;
using System.Threading;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// wizard d'import des cibles étendues
    /// </summary>
    public partial class eTargetImportWizard : eEudoPage
    {
        /// <summary>
        /// Table des cibles étendues
        /// </summary>
        public Int32 nTab = 0;

        /// <summary>
        /// Event des cibles étendues
        /// </summary>
        public Int32 nTabFrom = 0;

        /// <summary>
        /// Id de l'event
        /// </summary>
        public Int32 nEvtId = 0;

        /// <summary>
        /// largeur de la fenêtre
        /// </summary>
        public Int32 nWidth = eConst.DEFAULT_WINDOW_WIDTH;

        /// <summary>
        /// largeur de la fenêtre
        /// </summary>
        public Int32 nHeight = eConst.DEFAULT_WINDOW_HEIGHT;

        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return scriptHolder;
        }

        /// <summary>
        /// chargement de la page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {            
            LoadRequestParam();
            RenderWizard();
            AppendCssFiles();
            AppendJavaScriptFiles();
        }



        /// <summary>
        /// Récupère les paramètres de la request
        /// </summary>
        private void LoadRequestParam()
        {
            nTab = _requestTools.GetRequestFormKeyI("tab") ?? 0;
            nTabFrom = _requestTools.GetRequestFormKeyI("tabfrom") ?? 0;
            nEvtId = _requestTools.GetRequestFormKeyI("evtId") ?? 0;
            nWidth = _requestTools.GetRequestFormKeyI("width") ?? eConst.DEFAULT_WINDOW_WIDTH;
            nHeight = _requestTools.GetRequestFormKeyI("height") ?? eConst.DEFAULT_WINDOW_HEIGHT;
        }

        /// <summary>
        /// Fait le rendu du Wizard
        /// </summary>
        private void RenderWizard()
        {
            eRenderer render = eRendererFactory.CreateTargetImportWizardRenderer(_pref, nTab, nTabFrom, nEvtId, nWidth, nHeight);
            
            if (render.ErrorMsg.Length > 0)
            {
                String sDevMsg = String.Concat("Erreur sur la page : ", System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine);

                if (render.InnerException != null)
                    sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Exception Message : ", render.InnerException.Message, Environment.NewLine, "Exception StackTrace :", render.InnerException.StackTrace);

                if (render.ErrorMsg.Length > 0)
                    sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Error Message : ", render.ErrorMsg);

                ErrorContainer = eErrorContainer.GetDevUserError(
                eLibConst.MSG_TYPE.CRITICAL,
                eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                eResApp.GetRes(_pref, 72),  //   titre
                String.Concat(sDevMsg)

                );

                LaunchError();
            }

            TabTrg.Controls.Add(render.PgContainer);
        }

        /// <summary>
        /// Ajoutes des fichiers CSS
        /// </summary>
        private void AppendCssFiles()
        {
            PageRegisters.AddCss("eWizard");
            PageRegisters.AddCss("eTargetImport");
        }

        /// <summary>
        /// Ajoutes des fichiers JS
        /// </summary>
        private void AppendJavaScriptFiles()
        {
            PageRegisters.AddScript("eTools");
            PageRegisters.AddScript("eUpdater");
            PageRegisters.AddScript("eMain");
            PageRegisters.AddScript("eModalDialog");
            PageRegisters.AddScript("eTargetImportWizard");
        }  
    }
}