using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.eda.Mgr
{
    /// <summary>
    /// Description résumée de eAdminPlanningPrefDialogManager
    /// </summary>
    public class eAdminPlanningPrefDialogManager : eAdminManager
    {

        protected override void ProcessManager()
        {
            //eAdminRenderer renderer = new eAdminRenderer();
            //try
            //{
            //    renderer = eAdminRendererFactory.CreateAdminPlanningPrefDialogRenderer(_pref);

            //}
            //catch (Exception exc)
            //{
            //    ErrorContainer = eErrorContainer.GetDevUserError(
            //            eLibConst.MSG_TYPE.CRITICAL,
            //            eResApp.GetRes(_pref, 72),
            //            eResApp.GetRes(_pref, 6236),
            //            eResApp.GetRes(_pref, 72),
            //            String.Concat("Erreur création du renderer dans eAdminPlanningPrefDialogRenderer : ", exc.Message)
            //        );

            //    //Arrete le traitement et envoi l'erreur
            //    LaunchError();
            //}
            //finally
            //{

            //}

            //RenderResultHTML(renderer.PgContainer);
        }

    }
}