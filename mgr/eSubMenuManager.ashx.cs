using System;
using Com.Eudonet.Internal;
using EudoQuery;

namespace Com.Eudonet.Xrm
{
    /// <className>eSubMenuManager</className>
    /// <summary>Permet l'appel au renderer de sous-menu des grilles</summary>
    /// <purpose>Rafraichi  l'affichage de sous-menu</purpose>
    /// <authors>MOU</authors>
    /// <date>2017-07-12</date>
    public class eSubMenuManager : eEudoManager
    {
        /// <summary>
        /// Récupère le sous-menu des grilles
        /// </summary>
        protected override void ProcessManager()
        {
            eRequestTools tools = new eRequestTools(_context);
            Int32? gridId = tools.GetRequestFormKeyI("nGrid") ?? 0;
            Int32? tab = tools.GetRequestFormKeyI("nTab") ?? 0;           

            string error;
            TableLite tabInfos = null;
            using (eudoDAL edal = eLibTools.GetEudoDAL(_pref))
            {
                edal.OpenDatabase();
                tabInfos = new TableLite(tab.Value);
               
                tabInfos.ExternalLoadInfo(edal, out error);
            }

            if (string.IsNullOrEmpty(error))
            {
                eXrmGridSubMenuRenderer subMenuRenderer = new eXrmGridSubMenuRenderer(_pref, tabInfos, gridId.Value);
                if (subMenuRenderer.Init())
                    RenderResultHTML(subMenuRenderer.BuildSubMenu());
                else
                    subMenuRenderer.sError += "Impossible d'initialiser le renderer eWebTabSubMenuRenderer";

                error = subMenuRenderer.sError;
            }

            if (error.Length > 0)
            {
                //Avec exception
                String sDevMsg = String.Concat("Erreur sur la page : ", System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine);
                sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Exception Message : ", error, Environment.NewLine);
                LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 8652) , 
                    eResApp.GetRes(_pref, 8653),
                    eResApp.GetRes(_pref, 72),
                    string.Concat("Grid Id : ", gridId.Value, sDevMsg)));              
            }
        }
    }
}