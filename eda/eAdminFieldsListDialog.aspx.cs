using Com.Eudonet.Internal;
using System;
using System.Web.UI;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Page appelée à l'ouverture de modale pour la liste des rubriques d'un onglet
    /// </summary>
    public partial class eAdminFieldsListDialog : eAdminPage
    {
        /// <summary>DescId de la table demandée</summary>
        public int Tab { get; private set; }

        /// <summary>
        /// Appelé au chargement de la page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
            #endregion

            try
            {
                Tab = _requestTools.GetRequestFormKeyI("tab") ?? -1;

                eAdminRenderer renderer = eAdminRendererFactory.CreateAdminFieldsListDialogRenderer(_pref, Tab);
                //perte de la pile d'appel
                //if (renderer.InnerException != null)
                //    throw renderer.InnerException;

                if (renderer.ErrorMsg.Length > 0)
                    throw new Exception(renderer.ErrorMsg);

                formFieldsList.Controls.Add(renderer.PgContainer);
            }
            catch (Exception exc)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 72),
                        eResApp.GetRes(_pref, 6236),
                        eResApp.GetRes(_pref, 72),
                        String.Concat("Erreur création du renderer dans eAdminFieldsListDialog : ", exc.Message, Environment.NewLine, exc.StackTrace)
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