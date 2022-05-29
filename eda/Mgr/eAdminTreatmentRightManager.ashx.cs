using System;
using System.IO;
using Com.Eudonet.Internal;
using Newtonsoft.Json;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Description résumée de eAdminTreatmentRightManager
    /// </summary>
    public class eAdminTreatmentRightManager : eAdminManager
    {

        /// <summary>
        /// Traitement demandé au manager
        /// </summary>
        protected override void ProcessManager()
        {

            String sError;

            StreamReader sr = new StreamReader(_context.Request.InputStream);
            string s = sr.ReadToEnd();
            Container ctner;
            // Capsule caps;

            try
            {
                ctner = JsonConvert.DeserializeObject<Container>(s);
            }
            catch (JsonReaderException exc)
            {
                throw exc;
            }




            if (!ePermission.UpdateTreatment(_pref, ctner, eModelTools.GetRootPhysicalDatasPath(), out sError))
            {
                this.ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), eResApp.GetRes(_pref, 6237), devMsg: sError);

                LaunchError();
            }
            else
            {
                RenderResult(RequestContentType.TEXT, delegate ()
                {
                    return String.Concat("{",
                        "'succes':true"
                    , "}");
                });
            }

        }

    }
}