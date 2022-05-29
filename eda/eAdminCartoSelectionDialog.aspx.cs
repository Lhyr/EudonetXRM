using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using EudoQuery;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Classe permettant de gérer le paramétrage de la selection cartographique
    /// </summary>
    public partial class eAdminCartoSelectionDialog : eAdminPage
    {
       

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
            PageRegisters.RegisterAdminIncludeScript("eAdminCartoSelection");
            #endregion>

           int widgetId = _requestTools.GetRequestFormKeyI("wId") ?? 0;
           int height = _requestTools.GetRequestFormKeyI("wHeight") ?? 600;
            
            try
            {
                CartoConfig.Style.Add(HtmlTextWriterStyle.Height, $"{height}px");

                var config = eAdminCartoSelection.Create(_pref, widgetId).GetConfig().Config;
                if (config != null)
                    CartoConfig.Value = SerializerTools.JsonSerialize(config, true);
                
            }
            catch (Exception exc)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 72),
                        eResApp.GetRes(_pref, 6236),
                        eResApp.GetRes(_pref, 72),
                        String.Concat("Erreur création du renderer dans eAdminCartoSelectionDialog : ", exc.Message)
                    );

                //Arrete le traitement et envoi l'erreur
                try { LaunchError(); } catch (eEndResponseException) { }
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