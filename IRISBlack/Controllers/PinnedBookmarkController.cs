using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.IRISBlack.Factories;
using Com.Eudonet.Xrm.IRISBlack.Model;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using static Com.Eudonet.Core.Model.ePrefConst;
using static Com.Eudonet.Internal.eLibConst;
using static Com.Eudonet.Xrm.IRISBlack.Model.FileDetailModel;
using static Com.Eudonet.Xrm.IRISBlack.Model.PinnedDrBookmarkModel;

namespace Com.Eudonet.Xrm.IRISBlack.Controllers
{
    /// <summary>
    /// Controleur spécifique aux Bookmarks épinglés
    /// </summary>
    public class PinnedBookmarkController : BookmarkController
    {
        #region Propriétés privées
        /// <summary>
        /// Onglet concerné par la MAJ
        /// </summary>
        private int Tab { get; set; }
        /// <summary>
        /// Fiche concernée par la MAJ
        /// </summary>
        private int FileId { get; set; }
        /// <summary>
        /// DescID du signet épinglé à mettre à jour
        /// </summary>
        private int PinnedBookmarkDescId { get; set; }
        /// <summary>
        /// Opération à effectuer par le contrôleur
        /// </summary>
        private PinnedBookmarkControllerOperation Operation { get; set; }
        /// <summary>
        /// Position souhaitée pour le signet épinglé transmis
        /// </summary>
        private int TargetPosition { get; set; }
        /// <summary>
        /// Mode d'affichage des signets épinglés (LIST ou FILE)
        /// </summary>
        private BKMVIEWMODE ViewMode { get; set; }
        /// <summary>
        /// Liste des onglets épinglés après mise à jour
        /// </summary>
        private List<int> PinnedBookmarkDescIdList { get; set; }
        #endregion

        /// <summary>
        /// retourrne la liste des bookmarks d'un onglet.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("{nTab:int=0}/{nFileId:int}")]
        public new async System.Threading.Tasks.Task<IHttpActionResult> Get(int nTab, int nFileId)
        {
            return await base.Get(nTab, nFileId);
        }

        /// <summary>
        /// retourne le détail d'un signet.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("{ParentTab:int/ParentFileId:int/Bkm:int/RowsPerPage:int/Page:int}")]
        public new async System.Threading.Tasks.Task<IHttpActionResult> Get([FromUri]BkmRequestModel request)
        {
            return await base.Get(request);
        }


        public override IHttpActionResult Delete(int id)
        {
            throw new NotImplementedException();
        }

        // POST api/<controller>
        /// <summary>
        /// Ajoute (ou retire) le signet passé en paramètre de la liste des signets épinglés pour l'onglet donné en paramètre
        /// </summary>
        /// <param name="oPdBkm">Objet contenant les paramètres de mise à jour des signets épinglés</param>
        /// <returns></returns>
        /// [AcceptVerbs("POST", "PUT")]
        [HttpPost]
        public async System.Threading.Tasks.Task<IHttpActionResult> Post(PinnedDrBookmarkModel oPdBkm)
        {
            if (!ModelState.IsValid)
                return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));

            try
            {
                #region Récupération et validation des paramètres d'entrée et existants
                GetParameters(oPdBkm); // Récupération et validation des paramètres d'entrée
                if ((PinnedBookmarkDescIdList?.Count ?? 0) == 0)
                    PinnedBookmarkDescIdList = StructureBkmModelFactory.GetPinnedBookmarkList(_pref, Tab); // Récupération de la liste existante (si non passée en paramètre)
                CheckOperation(); // Validation de l'opération à effectuer et correction si nécessaire
                #endregion

                #region Vérification de la cohérence de l'opération demandée par rapport à la situation actuelle du signet épinglé pour l'onglet ciblé, et correction si nécessaire
                if (Operation != PinnedBookmarkControllerOperation.NONE)
                {
                    UpdatePinnedBookmarkList(); // Mise à jour de la liste des signets épinglés
                    UpdateBkmViewMode(); // Mise à jour du mode de visualisation
                }
                #endregion

                #region Renvoi de la réponse à l'appelant
                if (Operation != PinnedBookmarkControllerOperation.NONE)
                    return Ok(PinnedBookmarkDescIdList);
                else
                    return Ok(eResApp.GetRes(_pref, 3019)); // Pas de mise à jour de la fiche
                #endregion
            }
            catch (EudoException ex)
            {
                //return InternalServerError(ex);
                return Ok(ExceptionFactory.InitExceptionFactory(eLibTools.IsLocalOrEudoMachine()).GetExceptionModel(ex));
            }
            catch (Exception ex)
            {
                return Ok(ExceptionFactory.InitExceptionFactory(eLibTools.IsLocalOrEudoMachine()).GetExceptionModel(ex));
                //return InternalServerError(ex);
            }
            finally
            {
            }
        }

        /// <summary>
        /// Mise à jour de la liste selon l'opération souhaitée
        /// L'opération étant réputée comme ayant déjà été ajustée selon l'existence (ou non) du signet dans la liste, ce point ne sera pas revérifié ici
        /// On insèrera l'élément à la position demandée si MOVE_POSITION (qui sera vérifiée et corrigée plus bas si besoin) ou à la fin (ADD)
        /// </summary>
        private void UpdatePinnedBookmarkList()
        {
            if (Operation != PinnedBookmarkControllerOperation.REPLACE)
            {
                int insertIndex = Operation == PinnedBookmarkControllerOperation.MOVE_POSITION ? TargetPosition : (PinnedBookmarkDescIdList?.Count ?? 0);
                switch (Operation)
                {
                    case PinnedBookmarkControllerOperation.MOVE_LEFT:
                        insertIndex = PinnedBookmarkDescIdList.IndexOf(PinnedBookmarkDescId) - 1;
                        break;
                    case PinnedBookmarkControllerOperation.MOVE_RIGHT:
                        insertIndex = PinnedBookmarkDescIdList.IndexOf(PinnedBookmarkDescId) + 1;
                        break;
                }
                // Quelle que soit l(opération, il faut supprimer l'éventuel existant (on utilise RemoveAll pour supprimer les doublons s'il en existe en base, sait-on jamais)
                PinnedBookmarkDescIdList.RemoveAll(bkm => bkm == PinnedBookmarkDescId);
                // Puis, on rajoute le signet à l'emplacement souhaité, sauf dans le cas d'une suppression (DELETE)
                if (
                    Operation == PinnedBookmarkControllerOperation.ADD |
                    Operation == PinnedBookmarkControllerOperation.MOVE_POSITION ||
                    Operation == PinnedBookmarkControllerOperation.MOVE_LEFT ||
                    Operation == PinnedBookmarkControllerOperation.MOVE_RIGHT
                )
                {
                    // Correction de l'index avant insertion s'il est devenu hors limites suite à la suppression précédente
                    if (insertIndex < 0)
                        insertIndex = 0;
                    else if (insertIndex > PinnedBookmarkDescIdList.Count)
                        insertIndex = PinnedBookmarkDescIdList.Count;
                    // Puis insertion à l'emplacement souhaité/calculé
                    PinnedBookmarkDescIdList.Insert(insertIndex, PinnedBookmarkDescId);
                }
            }

            // On filtre la liste des DescIDs passés en
            // - autorisant uniquement les DescIDs dûment déclarés dans la base (présents dans DESC et ayant une table SQL correspondante)
            Dictionary<int, string> oAllowedDescIds = GetAllowedBookmarks();
            if (PinnedBookmarkDescIdList != null && PinnedBookmarkDescIdList.Count > 0)
            {
                PinnedBookmarkDescIdList.RemoveAll(bkm => !oAllowedDescIds.ContainsKey(bkm) && !(bkm % 100 ==  (int)AllField.ATTACHMENT));
            }

            // Mise à jour en base avec la liste modifiée (ou d'origine si REPLACE)
            eLibTools.AddOrUpdatePrefAdv(_pref, eLibConst.PREFADV.LISTBKMPINNED, String.Join(";", PinnedBookmarkDescIdList?.Distinct() ?? new List<int>()), eLibConst.PREFADV_CATEGORY.MAIN, _pref.UserId, Tab);
        }

        /// <summary>
        /// Met à jour BKMPREF.ViewMode
        /// </summary>
        private void UpdateBkmViewMode()
        {
            // Mise à jour du mode de visualisation (Liste ou Fiche)

            List<int> aDescIdsToUpdate = new List<int>();

            // On le fait pour PinnedBookmarkDescId (si un seul est passé)
            if (Operation != PinnedBookmarkControllerOperation.REPLACE)
                aDescIdsToUpdate.Add(PinnedBookmarkDescId);
            else if (PinnedBookmarkDescIdList != null)
                aDescIdsToUpdate.AddRange(PinnedBookmarkDescIdList);

            foreach (int descId in aDescIdsToUpdate)
            {
                // Remarque : Si la valeur n'est pas dans l'Enum (ce qui est techniquement accepté : (BKMVIEWMODE)666 ne provoquerait pas d'erreur), on insère null en base
                eBkmPref daoBkmPref = new eBkmPref(_pref, Tab, descId);
                List<SetParam<ePrefConst.PREF_BKMPREF>> prefBkm = new List<SetParam<ePrefConst.PREF_BKMPREF>>();
                prefBkm.Add(new SetParam<ePrefConst.PREF_BKMPREF>(ePrefConst.PREF_BKMPREF.VIEWMODE, Enum.IsDefined(typeof(BKMVIEWMODE), ViewMode) ? ((int)ViewMode).ToString() : null));
                daoBkmPref.SetBkmPref(prefBkm);
            }
        }

        /// <summary>
        /// Récupère et vérifie les paramètres passées en entrée au contrôleur
        /// </summary>
        /// <param name="oPdBkm"></param>
        private void GetParameters(PinnedDrBookmarkModel oPdBkm)
        {
            // Tab : DescID de l'onglet pour lequel mettre à jour la liste des signets épinglés</param>
            // FileId : FileID du fichier concerné
            // TargetPinnedBookmarkDescId : DescID du signet à épingler ou "désépingler"
            // TargetPosition : position à lui donner dans la liste
            // Operation : Opération à effectuer pour le signet et l'onglet donné (0 = UPDATE soit ajouter ou retirer le signet de la liste, 1 = ADD soit ajouter le signet, 2 = DELETE soit supprimer le signet, 3 = MOVE_POSITION soit le déplacer, 4 = MOVE_LEFT soit le déplacer à gauche dans la liste, 5 = MOVE_RIGHT soit le déplacer à droite dans la liste)</param>
            // ViewMode : LIST ou FILE
            Tab = oPdBkm.nTab;
            FileId = oPdBkm.nFileId;
            PinnedBookmarkDescId = oPdBkm.nPinnedBookmarkDescId;
            PinnedBookmarkDescIdList = oPdBkm.aPinnedBookmarkDescIdList?.ToList();
            TargetPosition = oPdBkm.nTargetPosition;
            Operation = oPdBkm.oOperation;
            ViewMode = oPdBkm.oViewMode;
        }

        /// <summary>
        /// Vérifie si l'opération de mise à jour demandée au contrôleur est cohérente avec les paramètres passés, et la corrige en fonction
        /// </summary>
        private void CheckOperation()
        {
            // Si aucun DescID exploitable n'est passé en entrée, on abandonne
            if (PinnedBookmarkDescId < 1 && PinnedBookmarkDescIdList?.Any() != true)
            {
                Operation = PinnedBookmarkControllerOperation.NONE;
                return;
            }

            // Si l'opération est UPDATE (par défaut), on décide si on fait un REPLACE (si une liste est précisée) ou autre chose (si un DescID seul est précisé)
            if (Operation == PinnedBookmarkControllerOperation.UPDATE && PinnedBookmarkDescId < 1 && PinnedBookmarkDescIdList?.Count > 0)
            {
                Operation = PinnedBookmarkControllerOperation.REPLACE;
                return;
            }

            // Dans les autres cas, vérification de la cohérence de l'opération demandée par rapport à la situation actuelle du signet épinglé pour l'onglet ciblé, et correction si nécessaire
            if (PinnedBookmarkDescIdList != null && !PinnedBookmarkDescIdList.Contains(PinnedBookmarkDescId))
            {
                // On ne peut pas supprimer ou déplacer le signet épinglé s'il n'est pas dans la liste
                if (Operation == PinnedBookmarkControllerOperation.DELETE || Operation == PinnedBookmarkControllerOperation.MOVE_LEFT || Operation == PinnedBookmarkControllerOperation.MOVE_RIGHT)
                    Operation = PinnedBookmarkControllerOperation.NONE;
                // Si UPDATE d'un signet non présent dans la liste = on ajoute le signet (inversion de statut)
                else if (Operation == PinnedBookmarkControllerOperation.UPDATE)
                    Operation = PinnedBookmarkControllerOperation.ADD;
            }
            else
            {
                // On ne peut pas ajouter un signet déjà présent dans la liste
                if (Operation == PinnedBookmarkControllerOperation.ADD)
                    Operation = PinnedBookmarkControllerOperation.NONE;
                // Si UPDATE d'un signet déjà présent dans la liste = on retire le signet (inversion de statut)
                else if (Operation == PinnedBookmarkControllerOperation.UPDATE)
                    Operation = PinnedBookmarkControllerOperation.DELETE;
            }
            if (Operation == PinnedBookmarkControllerOperation.ADD && TargetPosition > -1 && TargetPosition < PinnedBookmarkDescIdList.Count)
                Operation = PinnedBookmarkControllerOperation.MOVE_POSITION;
        }

        /// <summary>
        /// Retourne la liste des DescIDs autorisés à être épinglés (en fonction de leur type, ou autre critère inclusif, notamment leur existence dans DESC)
        /// L'utilisateur doit également avoir les droits de VISUALISATION sur les signets en question pour pouvoir les épingler
        /// </summary>
        /// <returns>Un dictionnaire (int: DesCid, string: Libellé) contenant les signets à exclure</returns>
        private Dictionary<int, string> GetAllowedBookmarks()
        {
            #region Pour information : types non autorisés
            /* Cette fonction renvoie les types AUTORISES, les types NON autorisés sont donc les restants :
                /// <summary>user</summary>
                EdnType.FILE_USER,
                /// <summary>group</summary>
                EdnType.FILE_GROUP,
                /// <summary>Filtres</summary>
                EdnType.FILE_FILTER,
                /// <summary>Rapports</summary>
                EdnType.FILE_REPORT,
                /// <summary>Modèles de mails</summary>
                EdnType.FILE_MAILTEMPLATE,
                /// <summary>Signet de type page Web</summary>
                EdnType.FILE_BKMWEB,
                /// <summary>TODO - notifications -- On utilise un fichier de type Main</summary>
                //EdnType.FILE_NOTIFICATIONS,
                /// <summary>Onglet de type page Web</summary>
                EdnType.FILE_WEBTAB,
                /// <summary>Signet de type Discussion</summary>
                EdnType.FILE_DISCUSSION,
                /// <summary>Page d'accueil XRM - Grilles</summary>
                EdnType.FILE_HOMEPAGE,
                /// <summary>Widget XRM</summary>
                EdnType.FILE_WIDGET,
                /// <summary>Onglet Grille ou Signet Grille</summary>
                EdnType.FILE_GRID,
                /// <summary>Type plus utilisé</summary>
                EdnType.FILE_OBSOLETE,
                /// <summary>Modèles d'import</summary>
                EdnType.FILE_IMPORTTEMPLATE,
                /// <summary>Sous-Requête</summary>
                EdnType.FILE_SUBQUERY,
                /// <summary>Systeme (SCHEDULE...)</summary>
                EdnType.FILE_SYSTEM,
                /// <summary>Non défini</summary>
                EdnType.FILE_UNDEFINED
            */
            #endregion

            // Informations renvoyées en sortie par la fonction : éventuelles erreurs, et liste des éléments invalides trouvés dans DESC (que l'on utilisera pas)
            string sError = String.Empty;
            Dictionary<int, string> oDeleted = new Dictionary<int, string>();

            // Types autorisés
            List<EdnType> oAllowedTypes = new List<EdnType>() {
                /// <summary>Fichier Principal</summary>
                EdnType.FILE_MAIN,         
                /// <summary>Sous-Fichier Planning</summary>
                EdnType.FILE_PLANNING,
                /// <summary>Sous-Fichier Standard</summary>
                EdnType.FILE_STANDARD,
                /// <summary>Sous-Fichier Email</summary>
                EdnType.FILE_MAIL,
                /// <summary>Sous-Fichier SMS</summary>
                EdnType.FILE_SMS,
                /// <summary>Sous-Fichier Historique</summary>
                EdnType.FILE_HISTO,
                /// <summary>Sous-Fichier Relation</summary>
                EdnType.FILE_RELATION,
                /// <summary>Sous-Fichier Message Vocal</summary>
                EdnType.FILE_VOICING,
                /// <summary>Adresse</summary>
                EdnType.FILE_ADR,
                /// <summary>PJ</summary>
                EdnType.FILE_PJ,
                /// <summary>Formulaire XRM</summary>
                EdnType.FILE_FORMULARXRM,
                /// <summary>Signet de type Cibles étendues</summary>
                EdnType.FILE_TARGET
            };

            // Retour
            var dicAuthorized =  eLibDataTools.GetFiles(null, _pref, _pref.User, oAllowedTypes, out sError, ref oDeleted, "", 0, false, true, false);
            dicAuthorized.Add((int)TableType.DOUBLONS, nameof(TableType.DOUBLONS));

            return dicAuthorized;
        }
    }
}
