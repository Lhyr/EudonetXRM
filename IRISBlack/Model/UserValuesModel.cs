using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static Com.Eudonet.Internal.eUser;

namespace Com.Eudonet.Xrm.IRISBlack.Model
{

    /// <summary>
    /// Modele de présentation des données pour un champ de type user
    /// </summary>
    public class UserValuesModel
    {
        /// <summary>
        /// Indique le tri à appliquer sur la liste des groupes/utilisateurs
        /// Par défaut : Tri par utilisateurs puis par groupes, puis alphabétique sans tenir compte de l'ordre de création des groupes (en vigueur dès la v10.402 et la demande #57 656)
        /// </summary>
        public ListSortMode UserListSortMode = ListSortMode.USERSGROUPS_ALPHABETIC;

        /// <summary>
        /// Liste des valeurs
        /// </summary>
        public List<IUserValue> Values = new List<IUserValue>();

        /// <summary>
        /// Modele représentant une valeur de la liste
        /// </summary>
        public class Value : IUserValue
        {

            /// <summary>
            /// Nom du groupe ou de l'utilisateur
            /// </summary>
            public string Label { get; set; }
            /// <summary>
            /// Indique si l'userListItem est un Groupe ou un Utilisateur 0 pour user, 1 pour Groupe
            /// </summary>
            public int Type { get; set; }
            /// <summary>
            /// Indique si l'utilisateur est désactivé
            /// </summary>
            public bool Disabled { get; set; }
            /// <summary>
            /// Indique si l'utilisateur est caché
            /// </summary>
            public bool Hidden { get; set; }
            /// <summary>
            /// Id de l'UserListItem (UserId ou 'G'+GroupId)
            /// </summary>
            public string ItemCode { get; set; }

            /// <summary>
            /// Groupe Level de l'utilisateur ou du groupe courant (hierarchie)
            /// </summary>
            public string GroupLevel { get; set; }
            /// <summary>
            /// GroupId de l'utilisateur 
            /// </summary>
            public string GroupId { get; set; }
            /// <summary>
            /// Niveau de profondeur dans l'arborescence de l'utilisateur
            /// </summary>
            public int Level { get { return GroupLevel.ToString().Length / 4 - 1; } }

            /// <summary>Indique si l'utilisateur est sélectionné</summary>
            public bool Selected { get; set; }


            /// <summary>Liste des Utilisateurs enfants</summary>
            public List<IUserValue> ChildrensUserListItem { get; set; }

            /// <summary>Indique si l'utilisateur est un enfant d'un UserListItem</summary>
            public bool IsChild { get; set; }

        }
    }
}