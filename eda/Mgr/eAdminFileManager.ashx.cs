using Com.Eudonet.Internal;
using System;
using System.Text;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Description résumée de eAdminFileManager
    /// </summary>
    public class eAdminFileManager : eAdminManager
    {

        Int32 _nActiveTab;

        /// <summary>
        /// Gestion de la demande de rendu du menu d'admin
        /// </summary>
        protected override void ProcessManager()
        {


            bool showSystemFields = false;

            //Initialisation
            if (_requestTools.AllKeys.Contains("tab") && !String.IsNullOrEmpty(_context.Request.Form["tab"]))
                Int32.TryParse(_context.Request.Form["tab"].ToString(), out _nActiveTab);

            if (_requestTools.AllKeys.Contains("sys") && !String.IsNullOrEmpty(_context.Request.Form["sys"]))
                showSystemFields = _context.Request.Form["sys"].ToString() == "1";


            try
            {



                eAdminFileRenderer rdr = eAdminFileRenderer.CreateAdminFileRenderer(_pref, _nActiveTab, showSystemFields);
                rdr.ShowFileProp = showSystemFields;
                RenderResultHTML(rdr.PgContainer);
            }
            catch (eFileLayout.eFileLayoutException e)
            {
                LaunchError(e.ErrorContainer);
            }
            catch (eEndResponseException) { }
            catch (EudoInternalException exx)
            {
                LaunchError(exx.ErrorContainer);
            }
            catch (EudoQuery.EudoException ex)
            {
                eErrorContainer err = eErrorContainer.GetErrorContainerFromEudoException(eLibConst.MSG_TYPE.CRITICAL,
                    sTitle: eResApp.GetRes(_pref, 72),
                    sShortUserMsg: "Chargement de l'interface d'administration impossible",
                    eudoEx: ex);

                LaunchError(err);

            }
            catch (Exception e)
            {
                StringBuilder sbDevMsg = new StringBuilder();
                if (e.InnerException != null)
                {
                    sbDevMsg.Append(e.InnerException.Message).AppendLine().Append(e.InnerException.StackTrace).AppendLine();
                }
                sbDevMsg.Append(e.Message).AppendLine().Append(e.StackTrace);
                eErrorContainer error = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), "Lors de la génération de l'affichage de la fiche", devMsg: sbDevMsg.ToString());
                LaunchError(error);
            }

        }

    }
}