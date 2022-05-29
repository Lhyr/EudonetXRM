using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Com.Eudonet.Xrm.eda
{
    public partial class eAdminNewExpressMessageDialog : eEudoPage
    {

        private int expressMessageId;
        private String sError;
        protected void Page_Load(object sender, EventArgs e)
        {
            #region Ajout des CSS

            PageRegisters.RegisterFromRoot = true;
            PageRegisters.AddCss("eMain");
            PageRegisters.AddCss("eButtons");
            PageRegisters.AddCss("eAdminMenu");
            PageRegisters.AddCss("eCatalog");
            PageRegisters.AddCss("eMain", "all");
            PageRegisters.AddCss("eControl");
            PageRegisters.AddCss("eMemoEditor");
            PageRegisters.AddCss("eButtons");
            #endregion

            #region Ajout des JS

            PageRegisters.AddScript("ckeditor/ckeditor");
            PageRegisters.AddScript("ePopup");
            PageRegisters.AddScript("eFieldEditor");
            PageRegisters.AddScript("eGrapesJSEditor");
            PageRegisters.AddScript("eMemoEditor");
            PageRegisters.AddScript("eUpdater");
            PageRegisters.AddScript("eTools");
            PageRegisters.AddScript("eModalDialog");
            PageRegisters.AddScript("eMain");
            PageRegisters.AddScript("eAdminHomePageCkInstance");

            #endregion

            #region Paramètres
            
            expressMessageId = 0;
            HashSet<String> allKeys = new HashSet<String>(Request.Form.AllKeys);

            if (allKeys.Contains("ident") && !String.IsNullOrEmpty(Request.Form["ident"]))
                expressMessageId = eLibTools.GetNum(Request.Form["ident"].ToString());

            eAdminHomepage hpExpressMessage = eAdminHomepage.GetExpressMesageById(_pref, expressMessageId, out sError);

            #endregion

            try
            {
                eAdminRenderer renderer = eAdminRendererFactory.CreateAdminNewExpressMessageDialogRenderer(_pref, hpExpressMessage);
                formAdminNewTab.Controls.Add(renderer.PgContainer);
            }
            catch (Exception exc)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 72),
                        eResApp.GetRes(_pref, 6236),
                        eResApp.GetRes(_pref, 72),
                        String.Concat("Erreur création du renderer dans eAdminNewExpressMessageDialog : ", exc.Message)
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