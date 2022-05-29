using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;

namespace Com.Eudonet.Xrm.eda.Mgr
{
    /// <summary>
    /// Description résumée de eAdminFieldsListManager
    /// </summary>
    public class eAdminFieldsListManager : eAdminManager
    {
        protected override void ProcessManager()
        {

            int tab = _requestTools.GetRequestFormKeyI("tab") ?? -1;
            String action = _requestTools.GetRequestFormKeyS("action");

            if (action == "updTabindex")
            {
                eudoDAL eDal = eLibTools.GetEudoDAL(_pref);
                try
                {
                    eDal.OpenDatabase();

                    eSqlDesc.SetTabIndex(eDal, tab);
                }
                catch (Exception exc)
                {
                    ErrorContainer = eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6237), "La mise à jour de l'ordre de saisie a échoué",
                        eResApp.GetRes(_pref, 416), exc.Message);
                    LaunchError();
                }
                finally
                {
                    eDal.CloseDatabase();
                }
            }

            eAdminRenderer renderer = eAdminRendererFactory.CreateAdminFieldsListRenderer(_pref, tab);
            if (renderer.ErrorMsg.Length > 0)
            {
                ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6237), renderer.ErrorMsg);
                LaunchError();
            }
            else
            {
                RenderResultHTML(renderer.PgContainer);
            }

        }
    }
}