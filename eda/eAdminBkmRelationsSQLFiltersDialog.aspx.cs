using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Web.UI;

namespace Com.Eudonet.Xrm.eda
{
    public partial class eAdminBkmRelationsSQLFiltersDialog : eAdminPage
    {
        protected int _nTab;
        protected int _nBkmTab;

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
            PageRegisters.AddScript("eUpdater");
            PageRegisters.AddScript("eModalDialog");
            #endregion

            #region Paramètres
            _nTab = 0;
            _nBkmTab = 0;

            HashSet<String> allKeys = new HashSet<String>(Request.Form.AllKeys);

            if (allKeys.Contains("tab") && !String.IsNullOrEmpty(Request.Form["tab"]))
                _nTab = eLibTools.GetNum(Request.Form["tab"].ToString());
            if (allKeys.Contains("bkmTab") && !String.IsNullOrEmpty(Request.Form["bkmTab"]))
                _nBkmTab = eLibTools.GetNum(Request.Form["bkmTab"].ToString());

            try
            {
                eAdminRenderer renderer = eAdminRendererFactory.CreateAdminBkmRelationsSQLFiltersDialogRenderer(_pref, _nBkmTab);
                if (renderer.ErrorNumber != EudoQuery.QueryErrorType.ERROR_NONE)
                {
                    if (renderer.InnerException != null)
                        throw renderer.InnerException;
                    else
                        throw new Exception(renderer.ErrorMsg);
                }

                formAdminBkmRelationsSQLFilters.Controls.Add(renderer.PgContainer);
            }
            catch (Exception exc)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 72),
                        eResApp.GetRes(_pref, 6236),
                        eResApp.GetRes(_pref, 72),
                        String.Concat("Erreur création du renderer dans eAdminBkmRelationsSQLFiltersDialog : ", exc.Message)
                    );

                //Arrete le traitement et envoi l'erreur
                LaunchError();
            }
            finally
            {

            }

            #endregion
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