using System;
using System.Collections.Generic;
using System.Web.UI;
using Com.Eudonet.Internal;
using EudoQuery;

namespace Com.Eudonet.Xrm.eda
{
    public partial class eAdminPlanningPrefDialog : eEudoPage
    {
        public int Tab;
        int UserId;
        protected void Page_Load(object sender, EventArgs e)
        {
            #region Ajout des css
            PageRegisters.RegisterFromRoot = true;

            PageRegisters.AddCss("eMain");
            PageRegisters.AddCss("eButtons");
            PageRegisters.AddCss("eControl");
            PageRegisters.AddCss("eAdminMenu");
            PageRegisters.AddCss("eAdminPrefPlanning");
            #endregion

            #region Ajout de js
            PageRegisters.AddScript("eMain");
            PageRegisters.AddScript("eTools");
            PageRegisters.AddScript("eUpdater");
            PageRegisters.AddScript("eModalDialog");
            PageRegisters.AddScript("eColorPicker");
            PageRegisters.RegisterAdminIncludeScript();
            PageRegisters.RegisterAdminIncludeScript("eAdminPrefPlanning");
            #endregion

            Tab = 0;
            UserId = 0;
            HashSet<String> allKeys = new HashSet<String>(Request.Form.AllKeys);
            if (allKeys.Contains("tab") && !String.IsNullOrEmpty(Request.Form["tab"]))
                Tab = eLibTools.GetNum(Request.Form["tab"].ToString());

            if (allKeys.Contains("userid") && !String.IsNullOrEmpty(Request.Form["userid"]))
                UserId = eLibTools.GetNum(Request.Form["userid"].ToString());

            try
            {
                if (UserId == 0)
                {
                    // Pref par défaut
                    if (this._pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                        throw new EudoAdminInvalidRightException();
                }

                eRenderer renderer = eAdminRendererFactory.CreateAdminPlanningPrefDialogRenderer(_pref, Tab, UserId);

                if (Tab == 0)
                    Tab = ((eAdminPlanningPrefDialogRenderer)renderer).GetCurrentTab;

                if (renderer.ErrorNumber != EudoQuery.QueryErrorType.ERROR_NONE)
                {
                    if (renderer.InnerException != null)
                        throw renderer.InnerException;
                    else
                        throw new Exception(renderer.ErrorMsg);


                }

                formAdminPlanningPref.Controls.Add(renderer.PgContainer);
            }
            catch (EudoAdminInvalidRightException exc)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.EXCLAMATION,
                        eResApp.GetRes(_pref, 6834),
                        exc.Message
                    );
            }
            catch (Exception exc)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 72),
                        eResApp.GetRes(_pref, 6236),
                        eResApp.GetRes(_pref, 72),
                        String.Concat("Erreur création du renderer dans eAdminPlanningPrefDialog : ", exc.Message)
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