using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Description résumée de eAdminRelationsFieldsManager
    /// </summary>
    public class eAdminRelationsFieldsManager : eAdminManager
    {
        protected override void ProcessManager()
        {
            int nTab = 0;
            String color = String.Empty;
            String icon = String.Empty;
            String error = String.Empty;
            String fontClassName = String.Empty;

            if (_requestTools.AllKeys.Contains("tab") && !String.IsNullOrEmpty(_context.Request.Form["tab"]))
                nTab = eLibTools.GetNum(_context.Request.Form["tab"].ToString());
            else
            {
                throw new Exception("Paramètre 'tab' nécessaire à la mise à jour.");
            }


            eAdminRelationsFieldsRenderer renderer = null;
            try
            {
                renderer = eAdminRendererFactory.CreateAdminRelationsFieldsRenderer(_pref, nTab);
                if (renderer.ErrorNumber != EudoQuery.QueryErrorType.ERROR_NONE)
                {
                    if (renderer.InnerException != null)
                        throw renderer.InnerException;
                    else
                        throw new Exception(renderer.ErrorMsg);

                }

            }
            catch (Exception exc)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 72),
                        eResApp.GetRes(_pref, 6236),
                        eResApp.GetRes(_pref, 72),
                        String.Concat("Erreur création du renderer dans eAdminRelationsFieldsManager : ", exc.Message)
                    );

                //Arrete le traitement et envoi l'erreur
                LaunchError();
            }
            finally
            {

            }

            RenderResultHTML(renderer.PgContainer);

        }
    }
}