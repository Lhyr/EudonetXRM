using System;
using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.mgr;
using EudoQuery;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Description résumée de eAdminFileMenuManager
    /// </summary>
    public class eAdminFileMenuManager : eFileMenuManager
    {

        protected virtual void CheckAdminRight()
        {

            //   Gestion des droits Admin
            if (_pref == null || !_pref.IsAdminEnabled || _pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
               eLibConst.MSG_TYPE.CRITICAL,
               eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
               String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
               eResApp.GetRes(_pref, 72),  //   titre
               String.Concat("Droits insuffisants."));
                LaunchError();
            }

 

            //Id du process - Check Anti XSRF - 
            // L'id est par session, il faudrait améliorer cela
            string sIdProcess = _context?.Request?.Headers?["X-EUPDATER-SID"] ?? "";
            string sBase = _context.Session["_uidupdater"]?.ToString() ?? "";
            if (sBase != sIdProcess)
            {
                //TODO : avant de thrower une erreur, s'assuer que le _uiupdater est tjs fourni
                //string serror = "fail check";
            }


        }

        /// <summary>
        /// Chargement de la page
        /// </summary>
        protected override void LoadSession()
        {
            base.LoadSession();
            CheckAdminRight();
        }
    }
}