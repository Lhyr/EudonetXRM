using System;
using Com.Eudonet.Internal;
using EudoQuery;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Description résumée de eEudoAdminManager
    /// </summary>
    public abstract class eAdminManager : eEudoManager
    {


        /// <summary>
        /// vérification des droits d'admin pour l'accès aux pages d'admin
        /// </summary>
        protected virtual void CheckAdminRight()
        {
            Boolean adminAvailable = eFeaturesManager.IsFeatureAvailable(_pref, eConst.XrmFeature.Admin);

            if (_pref == null
                || !_pref.IsAdminEnabled 
                || !_pref.AdminMode
                || _pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode()
                || !adminAvailable)
            {

                ErrorContainer = eErrorContainer.GetDevUserError(
               eLibConst.MSG_TYPE.EXCLAMATION,
               eResApp.GetRes(_pref, 1733),   // Message En-tête : Une erreur est survenue
                eResApp.GetRes(_pref, 8207),  //  Vous n'avez pas les droits suffisants pour effectuer cette action.
               eResApp.GetRes(_pref, 1733),  //   titre
                eResApp.GetRes(_pref, 6834)); // Droits insuffisants

                LaunchError();
            }



            //Id du process - Check Anti XSRF - 
            // L'id est par session, il faudrait améliorer cela
            string sIdProcess = _context?.Request?.Headers?["X-EUPDATER-SID"] ?? "";
            string sBase = _context.Session["_uidupdater"]?.ToString() ?? "";
            if (sBase != sIdProcess)
            {
                //TODO : avant de thrower une erreur, s'assuer que le _uiupdater est tjs fourni
                string serror = "fail check";
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