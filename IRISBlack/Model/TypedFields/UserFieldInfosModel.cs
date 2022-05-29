using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model.TypedFields
{
    /// <summary>
    /// retourne les champs de type utilisateur
    /// </summary>
    public class UserFieldInfos : FldTypedInfosModel
    {
        public bool Multiple { get; set; }
        public bool FullUserList { get; set; }
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
        /// <summary>
        /// Indique si le catalogue est arborescent (correspond à TreeViewUserList pour les catalogues utilisateur)
        /// </summary>
        public bool IsTree { get; set; }

        internal UserFieldInfos(Field f) : base(f)
        {
            Format = FieldType.User;
            Multiple = f.Multiple;
            FullUserList = f.IsFullUserList;
            ShowEmptyGroup = f.Format == FieldFormat.TYP_GROUP;
            ShowOnlyGroup = f.Format == FieldFormat.TYP_GROUP;
            IsTree = f.bTreeView/* || f.IsTreeViewUserList*/; // TK #5 673 - TODO / TOCHECK : Désactivation de la prise en compte de TreeViewUserList pour l'instant : le tri opéré côté front (cf. fromListToTree dans EntryFieldsUsers.js) ne supporte pas la transformation des données opérée par eUser.GetUserArbo

            if (f.Format == FieldFormat.TYP_GROUP)
                Multiple = true;
        }

    }
}