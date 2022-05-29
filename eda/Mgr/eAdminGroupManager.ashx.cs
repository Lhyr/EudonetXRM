using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda.Mgr
{
    /// <summary>
    /// Manager de eAdminTabs
    /// </summary>
    public class eAdminGroupManager : eAdminManager
    {

        /// <summary>
        /// Gestion des groupes
        /// </summary>
        /// <param name="context"></param>
        protected override void ProcessManager()
        {
            int nAction = 0;
            string sGroupName = String.Empty;
            int nGroupId = 0;
            bool bGroupPublic = false;
            int nParentGroupId = 0;

            string sErrorMsg = String.Empty;

            try
            {
                // Récupération des paramètres
                nAction = _requestTools.GetRequestFormKeyI("action") ?? 0; // Action à effectuer (0 = AJOUT, 1 = MODIFICATION, 2 = SUPPRESSION)
                nGroupId = _requestTools.GetRequestFormKeyI("groupId") ?? 0; // GroupId à modifier/supprimer (MODIFICATION/SUPPRESSION)
                sGroupName = _requestTools.GetRequestFormKeyS("groupName") ?? String.Empty; // Nom du groupe à créer ou modifier (AJOUT/MODIFICATION)
                bGroupPublic = _requestTools.GetRequestFormKeyB("groupPublic") ?? false; // Groupe public ou non
                nParentGroupId = _requestTools.GetRequestFormKeyI("parentGroupId") ?? 0; // GroupId du groupe parent sélectionné (MODIFICATION)

                // Récupération de la liste des groupes actuelle
                List<eGroup> lstGroups = eGroup.GetGroupList(_pref);

                // Vérification des paramètres en fonction de l'action et des groupes existants
                switch (nAction)
                {
                    // AJOUT
                    case 0:
                    default:
                        // Vérification de la conformité des paramètres
                        if (sGroupName == null || sGroupName.Trim().Length == 0)
                            sErrorMsg = eResApp.GetRes(_pref, 7566); // "Vous devez saisir le nom du groupe à créer."
                        if (sErrorMsg.Length == 0)
                        {
                            // Génération du prochain GroupLevel utilisable pour ce nouveau groupe
                            string sNextGroupLevel = eGroup.GetNextGroupLevel(lstGroups, nParentGroupId);
                            // Création de l'objet Groupe
                            eGroup newGroup = new eGroup(0, sGroupName, sNextGroupLevel, bGroupPublic, 0);
                            // MAJ en base
                            newGroup.Insert(_pref, out sErrorMsg);
                        }
                        break;
                    // MODIFICATION
                    case 1:
                        // Vérification de la conformité des paramètres
                        if (sGroupName == null || sGroupName.Trim().Length == 0)
                            sErrorMsg = eResApp.GetRes(_pref, 7566); // "Vous devez saisir le nouveau nom à donner au groupe.";
                        if (nGroupId == 0)
                            sErrorMsg = eResApp.GetRes(_pref, 7567); // "Vous devez indiquer le groupe à modifier.";
                        if (!lstGroups.Any(targetGroup => targetGroup.GroupId == nGroupId))
                            sErrorMsg = eResApp.GetRes(_pref, 7568); // "Le groupe à modifier n'existe pas.";
                        if (sErrorMsg.Length == 0)
                        {
                            // Récupération de l'objet Groupe existant
                            eGroup groupToUpdate = lstGroups.Find(existingGroup => existingGroup.GroupId == nGroupId);
                            // MAJ de l'objet à partir des nouvelles informations reçues

                            #region kha old code refacto pour utilisation dans producttools
                            //groupToUpdate.GroupName = sGroupName;
                            //groupToUpdate.GroupPublic = bGroupPublic;
                            //// Liste des groupes enfants actuels à mettre éventuellement à jour (dans le cas d'un déplacement)
                            //List<eGroup> groupChildren = eGroup.GetGroupChildren(lstGroups, groupToUpdate);
                            //string sChildrenErrorMsg = String.Empty;
                            //// Génération du prochain GroupLevel utilisable pour le rattachement du groupe au parent sélectionné si celui-ci est différent de l'actuel
                            //eGroup parentGroup = lstGroups.Find(existingGroup => existingGroup.GroupLevel == groupToUpdate.GroupLevel.Remove(groupToUpdate.GroupLevel.Length - 4));
                            //int nCurrentParentGroupId = parentGroup != null ? parentGroup.GroupId : 0;
                            //if (nCurrentParentGroupId != nParentGroupId)
                            //{
                            //    string sNextGroupLevel = eGroup.GetNextGroupLevel(lstGroups, nParentGroupId);
                            //    // On commence à mettre à jour le GroupLevel des enfants, en remplaçant la partie racine correspondant à l'ancien GroupLevel du parent,
                            //    // par son nouveau GroupLevel. On fait une extraction de chaîne plutôt qu'un Replace afin d'être sûr de remplacer le début de la chaîne
                            //    // correspondant à la référence parente, et non une partie en milieu de chaîne qui pourrait être identique mais correspondre à une toute
                            //    // autre chose
                            //    // Exemple :
                            //    // - GroupLevel du parent avant modification : 0001 (premier groupe racine)
                            //    // - GroupLevel du parent après modification : 00020003 (déplacé en troisième position sous le groupe 2)
                            //    // - GroupLevel du premier enfant avant modification : 00010001
                            //    // - GroupLevel à lui réaffecter après modification : 000200030001
                            //    // => Faire un Replace de 0001 par 00020003 sur le GroupLevel source (00010001) donnerait un GroupLevel incorrect
                            //    // de 0002000300020003 au lieu de 000200030001
                            //    foreach (eGroup child in groupChildren)
                            //    {
                            //        string parentGroupLevelPart = child.GroupLevel.Substring(0, groupToUpdate.GroupLevel.Length);
                            //        string childGroupLevelPart = child.GroupLevel.Substring(parentGroupLevelPart.Length);
                            //        child.GroupLevel = String.Concat(sNextGroupLevel, childGroupLevelPart);
                            //        // Puis on envoie la MAJ du groupe enfant en base
                            //        string sChildErrorMsg = String.Empty;
                            //        child.Update(_pref, out sChildrenErrorMsg);
                            //        if (sChildrenErrorMsg.Trim().Length > 0)
                            //            sChildrenErrorMsg = String.Concat(sChildrenErrorMsg, Environment.NewLine, sChildErrorMsg);
                            //    }
                            //     // Puis celui du parent
                            //    groupToUpdate.GroupLevel = sNextGroupLevel;
                            //}

                            #endregion
                            // Et dans tous les cas, MAJ en base
                            groupToUpdate.Update(_pref, out sErrorMsg, sGroupName: sGroupName, nParentGroupId: nParentGroupId, bGroupPublic: bGroupPublic, lstGroups: lstGroups);
                           // sErrorMsg = String.Concat(sErrorMsg, sChildrenErrorMsg);
                        }
                        break;
                    // SUPPRESSION
                    case 2:
                        // Vérification de la conformité des paramètres
                        if (nGroupId == 0)
                            sErrorMsg = eResApp.GetRes(_pref, 7569); // "Vous devez indiquer le groupe à supprimer.";
                        if (!lstGroups.Any(group => group.GroupId == nGroupId))
                            sErrorMsg = eResApp.GetRes(_pref, 7570); // "Le groupe à supprimer n'existe pas.";
                        if (sErrorMsg.Length == 0)
                        {
                            // Récupération de l'objet Groupe existant
                            eGroup groupToDelete = lstGroups.Find(existingGroup => existingGroup.GroupId == nGroupId);
                            // MAJ en base
                            groupToDelete.Delete(_pref, out sErrorMsg);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                sErrorMsg = ex.Message;
            }

            // Renvoi du flux JSON à l'appelant
            JSONReturnGroup res = new Mgr.JSONReturnGroup();

            if (sErrorMsg.Length > 0)
            {
                ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6237), sErrorMsg); // Une erreur est survenue lors de la mise à jour
                LaunchError();
            }
            else
            {
                res.Success = true;

                RenderResult(RequestContentType.SCRIPT, delegate () { return SerializerTools.JsonSerialize(res); });
            }

        }
    }

    public class JSONReturnGroup : JSONReturnGeneric
    {

 
    }
}