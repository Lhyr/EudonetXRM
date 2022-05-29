using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.eda;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Type de menu a afficher 
    /// Garder la meme enum que dans l'objet js oXrmHomeMenu
    /// </summary>
    public enum MenuPart
    {
        /// <summary>
        /// load all
        /// </summary>
        LOAD_ALL = 0,
        /// <summary>
        /// load content
        /// </summary>
        LOAD_CONTENT = 1,
        /// <summary>
        /// load widget configuration
        /// </summary>
        LOAD_WIDGET_CONFIG = 2,
        /// <summary>
        /// load page configuration
        /// </summary>
        LOAD_PAGE_CONFIG = 3,
    }

    /// <summary>
    /// Menu de droite pour paramétrer la page d'accueil
    /// </summary>
    public class eXrmHomeMenuParamManager : eAdminManager
    {

        /// <summary>
        /// Fait un rendu de menu de droite pour paramètrage 
        /// </summary>
        protected override void ProcessManager()
        {
            eRequestTools tools = new eRequestTools(_context);

            int tab = tools.GetRequestFormKeyI("tab") ?? 0;
            int gid = tools.GetRequestFormKeyI("gid") ?? 0;
            int fileId = tools.GetRequestFormKeyI("fileid") ?? 0; // 0  : création
            int height = tools.GetRequestFormKeyI("height") ?? 800;
            int width = tools.GetRequestFormKeyI("width") ?? 250;
            int menuPart = tools.GetRequestFormKeyI("part") ?? 0;

            int parentTab = tools.GetRequestFormKeyI("parenttab") ?? 0;
            int parentFileId = tools.GetRequestFormKeyI("parentfid") ?? 0;

            int gridLocation = tools.GetRequestFormKeyI("gridlocation") ?? 0;

            eXrmWidgetContext context = new eXrmWidgetContext(gid, parentTab, parentFileId, (eXrmWidgetContext.eGridLocation)gridLocation, fileId);

            eRenderer renderer = eRendererFactory.CreateXrmHomeMenuParamManager(_pref, tab, fileId, height, width, (MenuPart)menuPart, context);

            if (renderer.ErrorMsg.Length > 0)
            {
                LaunchError(
                    eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL,
                    string.Concat(eResApp.GetRes(_pref, 8174)),
                     "Merci de contacter votre administrateur", // TODORES
                    eResApp.GetRes(_pref, 72),
                    string.Concat("Menu droite -> Page id : ", fileId, " Message : ", renderer.ErrorMsg, "<br />", "StackTrace : ", renderer.InnerException.StackTrace)));

                return;
            }

            RenderResultHTML(renderer.PgContainer);
        }
    }
}