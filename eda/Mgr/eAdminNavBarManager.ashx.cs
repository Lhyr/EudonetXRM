using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Description résumée de eAdminNavBarManager
    /// </summary>
    public class eAdminNavBarManager : eAdminManager
    {

        /// <summary>
        /// Hauteur écran
        /// </summary>
        Int32 _nHeight = 800; // Taile de l'écran

        /// <summary>
        /// Largeur ecran
        /// </summary>
        Int32 _nWidth = 600;

        /// <summary>
        /// Table à administrer
        /// </summary>
        Int32 _nActiveTab = 0;

        /// <summary>
        /// Signet à administrer
        /// </summary>
        Int32 _nActiveBkm = 0;

        /// <summary>
        /// Type de module Administration / Options utilisateur concerné par le menu
        /// </summary>
        eUserOptionsModules.USROPT_MODULE _targetModule = eUserOptionsModules.USROPT_MODULE.UNDEFINED;

        /// <summary>
        /// ID d'une extension si précisé (cas où le titre doit avoir été récupéré d'un appel depuis l'API)
        /// </summary>
        int _extensionFileId = 0;

        /// <summary>
        /// Titre spécifique d'une extension si précisé (cas où le titre doit avoir été récupéré d'un appel depuis l'API)
        /// </summary>
        string _extensionLabel = "";

        /// <summary>
        /// Code spécifique d'une extension si précisé (cas où le code doit avoir été récupéré d'un appel depuis l'API)
        /// </summary>
        string _extensionCode = "";

        /// <summary>
        /// Gestion de la demande de rendu pour la navbar d'administration.
        /// </summary>
        /// <param name="context"></param>
        protected override void ProcessManager()
        {

 


            //Initialisation
            //


            if ( _requestTools.AllKeys.Contains("W") && !String.IsNullOrEmpty(_context.Request.Form["W"]))
                Int32.TryParse(_context.Request.Form["W"].ToString(), out _nWidth);

            if (_requestTools.AllKeys.Contains("H") && !String.IsNullOrEmpty(_context.Request.Form["H"]))
                Int32.TryParse(_context.Request.Form["H"].ToString(), out _nHeight);

            if (_requestTools.AllKeys.Contains("tab") && !String.IsNullOrEmpty(_context.Request.Form["tab"]))
                Int32.TryParse(_context.Request.Form["tab"].ToString(), out _nActiveTab);

            if (_requestTools.AllKeys.Contains("bkm") && !String.IsNullOrEmpty(_context.Request.Form["bkm"]))
                Int32.TryParse(_context.Request.Form["bkm"].ToString().Replace("bkm", ""), out _nActiveBkm);

            // Type de module Administration / Options utilisateur concerné par la navBar
            _targetModule = eUserOptionsModules.USROPT_MODULE.UNDEFINED;
            if (_requestTools.AllKeys.Contains("module"))
            {
                int nTargetModule = 0;
                Int32.TryParse(_context.Request.Form["module"].ToString(), out nTargetModule);
                _targetModule = (eUserOptionsModules.USROPT_MODULE)nTargetModule;
            }

            // Titre spécifique d'un module si précisé (cas des extensions dont le titre doit avoir été récupéré d'un appel depuis l'API)
            if (_requestTools.AllKeys.Contains("extensionFileId") && !String.IsNullOrEmpty(_context.Request.Form["extensionFileId"]))
                Int32.TryParse(_context.Request.Form["extensionFileId"].ToString(), out _extensionFileId);

            // Titre spécifique d'un module si précisé (cas des extensions dont le titre doit avoir été récupéré d'un appel depuis l'API)
            _extensionLabel = _context.Request.Form["extensionLabel"];

            // Code spécifique d'un module si précisé (cas des extensions dont le code doit avoir été récupéré d'un appel depuis l'API)
            _extensionCode = _context.Request.Form["extensionCode"];

            eAdminRenderer rdr;

            switch (_targetModule)
            {
                case eUserOptionsModules.USROPT_MODULE.ADMIN_HOME_XRM_HOMEPAGE:
                case eUserOptionsModules.USROPT_MODULE.ADMIN_HOME_XRM_HOMEPAGE_GRID:
                    int pageId = _requestTools.GetRequestFormKeyI("pageId") ?? 0; 
                    int gridId = _requestTools.GetRequestFormKeyI("gridId") ?? 0;
                    string gridLabel = _requestTools.GetRequestFormKeyS("gridLabel");
                    rdr = eAdminRendererFactory.CreateAdminHomePageNavBarRenderer(_pref, _nActiveTab, pageId, gridId, gridLabel);
                    break;
                default:
                    rdr = eAdminRendererFactory.CreateAdminNavBarRenderer(_pref, _nActiveTab, _nActiveBkm, _targetModule, _extensionFileId, _extensionLabel, _extensionCode);
                    break;
            }           

            RenderResultHTML(rdr.PgContainer);

        }
 
    }
}