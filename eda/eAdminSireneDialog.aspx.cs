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
    /// Classe permettant de gérer le mapping de l'extension Sirene sur un fichier en particulier
    /// </summary>
    public partial class eAdminSireneDialog : eAdminPage
    {
        /// <summary>
        /// TabID du fichier sur lequel doit porter ce mapping Sirene
        /// </summary>
        protected int _nTab;

        /// <summary>
        /// Méthode exécutée au chargement de la page
        /// </summary>
        /// <param name="sender">Objet appelant</param>
        /// <param name="e">Arguments passés à la méthode</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            PageRegisters.RegisterFromRoot = true;
            #region Ajout des css
            PageRegisters.AddCss("eMain");
            PageRegisters.AddCss("eButtons");
            PageRegisters.AddCss("eControl");
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
                eAdminRenderer renderer = eAdminRendererFactory.CreateAdminSireneDialogRenderer(_pref, _nTab);
                if (renderer.ErrorNumber != EudoQuery.QueryErrorType.ERROR_NONE)
                {
                    if (renderer.InnerException != null)
                        throw renderer.InnerException;
                    else
                        throw new Exception(renderer.ErrorMsg);
                }

                formAdminSirene.Controls.Add(renderer.PgContainer);
            }
            catch (Exception exc)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 72),
                        eResApp.GetRes(_pref, 6236),
                        eResApp.GetRes(_pref, 72),
                        String.Concat("Erreur création du renderer dans eAdminSireneDialog : ", exc.Message)
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