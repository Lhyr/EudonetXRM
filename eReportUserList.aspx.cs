using Com.Eudonet.Internal;
using System;
using System.Threading;
using System.Web.UI;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// PAge de sélection des reports
    /// </summary>
    public partial class eReportUserListDialog : eEudoPage
    {
        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return scriptHolder;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            #region ajout des css

            PageRegisters.AddCss("eList");
            PageRegisters.AddCss("eCatalog");
            PageRegisters.AddCss("eMain", "all");
            PageRegisters.AddCss("eControl");
            PageRegisters.AddCss("eModalDialog");
            if (Request.Browser.MajorVersion == 7 && Request.Browser.Browser == "IE")
                PageRegisters.AddCss("ie7-styles");
            #endregion


            #region ajout des js



            PageRegisters.AddScript("eTools");
            PageRegisters.AddScript("eUpdater");
            PageRegisters.AddScript("eModalDialog");
            #endregion

            try
            {

                Boolean bArchiveOnly = false;
                if (_allKeys.Contains("archiveonly") && !string.IsNullOrEmpty(Request.Form["archiveonly"]))
                    bArchiveOnly = (Request.Form["archiveonly"] == "1");

                eReportUserListRenderer er = eReportUserListRenderer.GetUserListRenderer(_pref, bArchiveOnly);
                DivGlobal.Controls.Add(er.PgContainer);


            }
            catch (eEndResponseException) { Response.End(); }
            catch (ThreadAbortException) { }
            catch (Exception ex)
            {
                String sDevMsg = String.Concat("Erreur sur la page : ", System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine);


                sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Exception Message : ", ex.Message, Environment.NewLine, "Exception StackTrace :", ex.StackTrace);

                ErrorContainer = eErrorContainer.GetDevUserError(
                   eLibConst.MSG_TYPE.CRITICAL,
                   eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                   String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                   eResApp.GetRes(_pref, 72),  //   titre
                   String.Concat(sDevMsg));


                LaunchError();
            }


        }
    }
}