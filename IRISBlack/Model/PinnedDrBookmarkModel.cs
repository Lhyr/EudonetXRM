using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static Com.Eudonet.Core.Model.ePrefConst;

namespace Com.Eudonet.Xrm.IRISBlack.Model
{
    /// <summary>
    /// Permet d'associer un modèle d'envoi pour le front
    /// </summary>
    public class PinnedDrBookmarkModel
    {
        /// <summary>
        /// Opération à effectuer lors de l'appel à certaines méthodes (POST, notamment)
        /// </summary>
        public enum PinnedBookmarkControllerOperation
        {
            /// <summary>Aucune action ne sera effectuée si Operation est passée à -1 (par le processus lui-même suite à une erreur ou une demande incohérente, ou par l'appelant) </summary>
            NONE = -1,
            /// <summary>Opération par défaut, ou si non précisée : mettre à jour le signet épinglé dans la liste des signets épinglés de l'onglet souhaité : le retire s'il est déjà présent, l'ajoute s'il ne l'est pas. Ou si une liste est passée : REMPLACE (6)</summary>
            UPDATE = 0,
            /// <summary>Ajoute le signet épinglé dans la liste des signets épinglés de l'onglet souhaité. S'il existe déjà dans la liste, ne fait rien</summary>
            ADD = 1,
            /// <summary>Supprime le signet épinglé de la liste des signets épinglés de l'onglet souhaité. S'il ne figure pas dans la liste, ne fait rien</summary>
            DELETE = 2,
            /// <summary>Déplace le signet épinglé donné à la position indiquée dans la liste des signets épinglés de l'onglet souhaité : (ex : 100;200;300;400 devient 100;200;400;300 si cette opération est utilisée pour le signet 300 avec l'index 2). Si le signet donné est déjà à ladite position, ne fait rien</summary>           
            MOVE_POSITION = 3,
            /// <summary>Déplace le signet épinglé donné d'un rang vers la gauche dans la liste des signets épinglés de l'onglet souhaité : (ex : 100;200;300;400 devient 100;300;200;400 si cette opération est utilisée pour le signet 300). Si le signet donné est déjà en premier dans la liste, ne fait rien</summary>           
            MOVE_LEFT = 4,
            /// <summary>Déplace le signet épinglé donné d'un rang vers la droite dans la liste des signets épinglés de l'onglet souhaité : (ex : 100;200;300;400 devient 100;200;400;300 si cette opération est utilisée pour le signet 300). Si le signet donné est déjà en dernier dans la liste, ne fait rien</summary>           
            MOVE_RIGHT = 5,
            /// Remplacer la liste de signets épinglés existante par la liste donnée en paramètre (permet aussi de remettre à zéro une préférence foireuse)
            REPLACE = 6
        }

        /// <summary>
        /// L'onglet
        /// </summary>
        public int nTab { get; set; }
        /// <summary>
        /// Le fichier
        /// </summary>
        public int nFileId { get; set; }
        /// <summary>
        /// Le signet à ajouter ou enlever (sauf si une liste entière est précisée)
        /// </summary>
        public int nPinnedBookmarkDescId { get; set; }
        /// <summary>
        /// Liste de tous les signets épinglés à utiliser en tant que nouvelle liste
        /// </summary>
        public int[] aPinnedBookmarkDescIdList { get; set; }
        /// <summary>
        /// Opération à effectuer pour le signet et l'onglet donné (0 = UPDATE soit ajouter ou retirer le signet de la liste, 1 = ADD soit ajouter le signet, 2 = DELETE soit supprimer le signet, 3 = MOVE_POSITION soit le déplacer, 4 = MOVE_LEFT soit le déplacer à gauche dans la liste, 5 = MOVE_RIGHT soit le déplacer à droite dans la liste)
        /// </summary>
        public PinnedBookmarkControllerOperation oOperation { get; set; }
        /// <summary>
        /// L'index (position) à laquelle insérer/positionner le signet épinglé pour les opérations ADD et MOVE_POSITION
        /// </summary>
        public int nTargetPosition { get; set; } = -1;
        /// <summary>
        /// Mode de visualisation pour lequel la préférence doit être modifiée
        /// </summary>
        public BKMVIEWMODE oViewMode { get; set; }
    }
}