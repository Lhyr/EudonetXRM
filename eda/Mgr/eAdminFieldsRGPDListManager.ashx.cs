using Com.Eudonet.Internal;
using System;

namespace Com.Eudonet.Xrm.eda.Mgr
{
    /// <summary>
    /// Description résumée de eAdminFieldsRGPDListManager
    /// </summary>
    public class eAdminFieldsRGPDListManager : eAdminManager
    {
        protected override void ProcessManager()
        {
            int tab = _requestTools.GetRequestFormKeyI("tab") ?? -1;

            eAdminRenderer renderer = eAdminRendererFactory.CreateAdminFieldsRGPDListRenderer(_pref, tab);
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