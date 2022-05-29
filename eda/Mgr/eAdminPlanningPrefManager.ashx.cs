using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Description résumée de eAdminPlanningPrefDialogManager
    /// </summary>
    public class eAdminPlanningPrefManager : eEudoManager
    {
        Int32 nTab;
        Int32 nUserId;

        protected override void ProcessManager()
        {
            if (_requestTools.AllKeys.Contains("tab") && !String.IsNullOrEmpty(_context.Request.Form["tab"]))
                Int32.TryParse(_context.Request.Form["tab"].ToString(), out nTab);

            if (_requestTools.AllKeys.Contains("userid") && !String.IsNullOrEmpty(_context.Request.Form["userid"]))
                Int32.TryParse(_context.Request.Form["userid"].ToString(), out nUserId);

            
            try
            {
                eRenderer renderer = eAdminRendererFactory.CreateAdminPlanningPrefRenderer(_pref, nTab, nUserId);
                RenderResultHTML(renderer.PgContainer);

            }
            catch (eEndResponseException) { }
            catch (EudoAdminInvalidRightException exc) {
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
                        String.Concat("Erreur création du renderer dans eAdminPlanningPrefDialogRenderer : ", exc.Message)
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

    }
}