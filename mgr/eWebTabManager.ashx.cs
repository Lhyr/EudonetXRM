using System;
using System.Collections.Generic;
using Com.Eudonet.Internal;


namespace Com.Eudonet.Xrm
{
    /// <className>eWebTabManager</className>
    /// <summary>Permet l'appel au renderer des bkm</summary>
    /// <purpose>Rafraichi  l'affichage d'un/de plusieurs signet(s)</purpose>
    /// <authors>SPH</authors>
    /// <date>2011-05-10</date>
    public class eWebTabManager : eEudoManager
    {
        /// <summary>
        /// Met à jour le Rendu d'un BookMark ou de la liste des BookMarks
        /// </summary>
        protected override void ProcessManager()
        {
            eRequestTools tools = new eRequestTools(_context);

            Int32? specifId = tools.GetRequestFormKeyI("specifid");
            Int32? tab = tools.GetRequestFormKeyI("activetab");

            Int32? height = tools.GetRequestFormKeyI("H");
            Int32? width = tools.GetRequestFormKeyI("W");


            eRenderer renderer = eRendererFactory.CreateWebTabRenderer(_pref, specifId.HasValue ? specifId.Value : 0,
                width.HasValue ? width.Value : 1200,
                height.HasValue ? height.Value : 800,
                _context.Request.Url.Scheme.ToLower().Equals("https")
                );

            if (renderer.ErrorMsg.Length > 0)
            {
                LaunchError(
                    eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL,                   
                    string.Concat("Impossible de charger le sous-onglet web demandé ! <br />", "id:"),
                     "Merci de contacter votre administrateur",
                    eResApp.GetRes(_pref, 72),
                    string.Concat("Specif Id : ", specifId.Value , "Message : ", renderer.ErrorMsg,"<br />", "StackTrace : ", renderer.InnerException.StackTrace)));
           
                return;
            }

            RenderResultHTML(renderer.PgContainer);
        }
    }
}