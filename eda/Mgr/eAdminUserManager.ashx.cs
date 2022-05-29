using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda.Mgr
{
    /// <summary>
    /// Manager de eAdminUser
    /// </summary>
    public class eAdminUserManager : eAdminManager
    {

        /// <summary>
        /// Gestion des utilisateurs
        /// </summary>
        protected override void ProcessManager()
        {
            int nAction = 0;
            string sUserName = String.Empty;
            int nUserId = 0;
            int nReplacementUserId = 0;

            string sErrorMsg = String.Empty;
            string sUserDelWarnMsg = String.Empty;

            eudoDAL dal = eLibTools.GetEudoDAL(_pref);

            try
            {
                if (!dal.IsOpen)
                    dal.OpenDatabase();

                // Récupération des paramètres
                nAction = _requestTools.GetRequestFormKeyI("action") ?? 0; // Action à effectuer (0 = récupération d'infos, 1 = SUPPRESSION)
                nUserId = _requestTools.GetRequestFormKeyI("userId") ?? 0; // UserId à supprimer
                sUserName = _requestTools.GetRequestFormKeyS("userName") ?? String.Empty; // Nom de l'utilisateur concerné
                nReplacementUserId = _requestTools.GetRequestFormKeyI("replacementUserId") ?? 0; // UserID de l'utilisateur auquel réaffecter les fiches de l'utilisateur supprimé

                // Récupération de la liste des utilisateurs actuelle
                StringBuilder sbErrorMsg = new StringBuilder();
                List<eUser.UserListItem> lstUsers = eModelTools.GetUser(dal, new List<string>() { nUserId.ToString() }, _pref, true, false, ref sbErrorMsg);
                if (sbErrorMsg.Length > 0)
                    sErrorMsg = sbErrorMsg.ToString();

                // Vérification des paramètres en fonction de l'action et des groupes existants
                switch (nAction)
                {
                    // Récupération d'informations
                    case 0:
                        // Vérification de la conformité des paramètres
                        if (nUserId < 1)
                            sErrorMsg = eResApp.GetRes(_pref, 7573); // "Vous devez indiquer l'utilisateur à supprimer.";
                        if (!lstUsers.Any(user => user.ItemCode == nUserId.ToString()))
                            sErrorMsg = eResApp.GetRes(_pref, 7679); // "L'utilisateur à supprimer n'existe pas.";
                        if (sErrorMsg.Length == 0)
                        {
                            // Récupération de l'objet Utilisateur existant
                            eUser.UserListItem userToDelete = lstUsers.Find(existingUser => existingUser.ItemCode == nUserId.ToString());
                            // Récupération des dépendances pour affichage de l'avertissement à l'utilisateur
                            sUserDelWarnMsg = eUser.GetLinkedDataFilesDescription(_pref, userToDelete, out sErrorMsg);
                        }
                        break;
                    // SUPPRESSION
                    case 1:
                        // Vérification de la conformité des paramètres
                        if (nUserId < 1)
                            sErrorMsg = eResApp.GetRes(_pref, 7673); // "Vous devez indiquer l'utilisateur à supprimer.";
                        if (!lstUsers.Any(user => user.ItemCode == nUserId.ToString()))
                            sErrorMsg = eResApp.GetRes(_pref, 7679); // "L'utilisateur à supprimer n'existe pas.";
                        if (sErrorMsg.Length == 0)
                        {
                            // Récupération de l'objet Utilisateur existant
                            eUser.UserListItem userToDelete = lstUsers.Find(existingUser => existingUser.ItemCode == nUserId.ToString());
                            // MAJ en base
                            List<string> deletionResults = new List<string>();
                            eUser.Delete(_pref, userToDelete, sUserName, nReplacementUserId, out deletionResults, out sErrorMsg);
                            // Affichage des résultats à l'utilisateur s'il s'agit d'un superadministrateur, ou en debug - TOCHECK: proposition à valider
#if DEBUG
                            bool showDeletionResults = true;
#else
                            bool showDeletionResults = _pref.User.UserLevel == UserLevel.LEV_USR_SUPERADMIN.GetHashCode();
#endif
                            if (showDeletionResults)
                                sUserDelWarnMsg = String.Join(Environment.NewLine, deletionResults.ToArray());
                        }
                        break;
                }
            }
            catch (EudoException ee)
            {
                sErrorMsg = ee.UserMessage;
            }
            catch (Exception ex)
            {
                sErrorMsg = ex.Message;
            }
            finally
            {
                if (dal != null && dal.IsOpen)
                    dal.CloseDatabase();
            }

            // Renvoi du flux JSON à l'appelant
            JSONReturnUser res = new Mgr.JSONReturnUser();

            if (sErrorMsg.Length > 0)
            {
                ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6237), sErrorMsg); // Une erreur est survenue lors de la mise à jour
                LaunchError();
            }
            else
            {
                res.Success = sErrorMsg.Length == 0;

                res.DelWarning = sUserDelWarnMsg;

                RenderResult(RequestContentType.SCRIPT, delegate () { return SerializerTools.JsonSerialize(res); });
            }

        }
    }

    public class JSONReturnUser : JSONReturnGeneric
    {
    

        public string DelWarning = String.Empty;

  
    }
}