using System;
using System.Web.UI;
using Com.Eudonet.Internal;
using EudoQuery;

namespace Com.Eudonet.Xrm.eda
{
    public partial class eAdminRGPDConditions : eAdminPage
    {
        /// <summary>
        /// DescID de la table
        /// </summary>
        protected int _tab = 0;
        protected RGPDRuleType _ruleType = RGPDRuleType.Archiving;

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Ajout des css
            PageRegisters.RegisterFromRoot = true;

            PageRegisters.AddCss("eMain");
            PageRegisters.AddCss("eButtons");
            PageRegisters.AddCss("eControl");
            PageRegisters.AddCss("eAdminMenu");
            PageRegisters.AddCss("eAdmin");
            #endregion

            #region Ajout de js
            PageRegisters.AddScript("eTools");
            PageRegisters.AddScript("eUpdater");
            PageRegisters.AddScript("eModalDialog");
            //PageRegisters.AddScript("eAdmin");
            #endregion

            #region Paramètres

            _tab = _requestTools.GetRequestFormKeyI("tab") ?? 0;
            _ruleType = (RGPDRuleType)(_requestTools.GetRequestFormKeyI("ruleType") ?? 0);

            #endregion

            String error = String.Empty;
            try
            {
                eAdminRenderer renderer = eAdminRendererFactory.CreateAdminRGPDConditionsDialogRenderer(_pref, _tab, _ruleType);
                if (renderer.ErrorNumber != EudoQuery.QueryErrorType.ERROR_NONE)
                {
                    if (renderer.InnerException != null)
                        throw renderer.InnerException;
                    else
                        throw new Exception(renderer.ErrorMsg);
                }

                formAdminRGPDConditions.Controls.Add(renderer.PgContainer);
            }
            catch (Exception exc)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 72),
                        eResApp.GetRes(_pref, 6236),
                        eResApp.GetRes(_pref, 72),
                        String.Concat("Erreur création du renderer dans eAdminRGPDConditions : ", exc.Message)
                    );

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