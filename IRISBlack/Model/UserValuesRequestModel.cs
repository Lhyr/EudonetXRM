using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model
{
    /// <summary>
    /// paramétrage de la requête qui permets d'obtenir les valeurs disponibles pour les champs de type user
    /// </summary>
    public class UserValuesRequestModel
    {
        /// <summary>Indique s'il s'agit d'un catalogue Utilisateur multiple ou simple</summary>
        public bool Multiple { get; set; } = false;
        /// <summary>Descid du catalogue Utilisateur</summary>
        public int DescId { get; set; } = 0;
        /// <summary>Indique si l'on doit afficher tous les utilisateurs en passant outre la gestion des droits de la base</summary>
        public bool FullUserList { get; set; } = false;
        /// <summary>Indique si l'on doit afficher les Groupe sans utilisateurs</summary>
        public bool ShowEmptyGroup { get; set; } = false;
        /// <summary>Indique si l'on doit afficher les utilisateurs seulement (sans les groupes)</summary>
        public bool ShowUserOnly { get; set; } = false;
        /// <summary> n'afficher que les groupes </summary>
        public bool ShowOnlyGroup { get; set; } = false;
        /// <summary>Permet d'autoriser la sélection d'un groupe en catalogue à choix simple</summary>
        public bool UseGroup { get; set; } = false;
        /// <summary>Affiche les profils utilisateurs </summary>
        public bool DisplayProfile { get; set; } = false;
        /// <summary>Ne montre que les profils </summary>
        public bool ShowOnlyProfile { get; set; } = false;
        
        /// <summary>Chaine de recherche</summary>
        public string SearchPattern { get; set; } = "";
        /// <summary>Indique si la présentation en arborescence est requise </summary>
        public bool TreeView { get; set; } = false;



    }
}