using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Com.Eudonet.Xrm.eda
{
    public partial class eAdminRelationsDialog : eAdminPage
    {
        protected int _nTab;

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Ajout des css
            PageRegisters.RegisterFromRoot = true;

            PageRegisters.AddCss("eMain");
            PageRegisters.AddCss("eButtons");
            PageRegisters.AddCss("eControl");
            PageRegisters.AddCss("eFieldsSelect");
            PageRegisters.AddCss("eAdminMenu");
            PageRegisters.AddCss("eAdmin");
            #endregion

            #region Ajout de js
            PageRegisters.AddScript("eTools");
            PageRegisters.AddScript("eUpdater");
            PageRegisters.AddScript("eModalDialog");
            PageRegisters.AddScript("eFieldsSelect");
            //PageRegisters.AddScript("eAdmin");
            PageRegisters.RegisterAdminIncludeScript("eAdminRelations");
            #endregion

            #region Paramètres
            _nTab = 0;

            HashSet<String> allKeys = new HashSet<String>(Request.Form.AllKeys);

            if (allKeys.Contains("tab") && !String.IsNullOrEmpty(Request.Form["tab"]))
                _nTab = eLibTools.GetNum(Request.Form["tab"].ToString());

            #endregion

            String error = String.Empty;
            try
            {

                eAdminRelationsDialogRenderer renderer = eAdminRendererFactory.CreateAdminRelationsDialogRenderer(_pref, _nTab);
                if (renderer.ErrorNumber != EudoQuery.QueryErrorType.ERROR_NONE)
                {
                    if (renderer.InnerException != null)
                        throw renderer.InnerException;
                    else
                        throw new Exception(renderer.ErrorMsg);
                }

                formAdminRelations.Controls.Add(renderer.PgContainer);
            }
            catch (Exception exc)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 72),
                        eResApp.GetRes(_pref, 6236),
                        eResApp.GetRes(_pref, 72),
                        String.Concat("Erreur création du renderer dans eAdminRelationsDialog : ", exc.Message)
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