using Com.Eudonet.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model
{
    /// <summary>
    /// Model pour la fonction get de recherche de MruController
    /// </summary>
    public class MRUGetSearchResultModel
    {
        /// <summary>
        /// Table sur laquelle on recherche
        /// </summary>
        public int TargetTab { get; set; }
        /// <summary>
        /// DescID du champ d'origine affichant les MRU
        /// </summary>
        public int nDesc { get; set; } = 0;
        /// <summary>
        /// Chaîne de texte recherchée
        /// </summary>
        public string sSearch { get; set; } = "";
        /// <summary>
        /// Table appelante
        /// </summary>
        public int nTabFrom { get; set; } = 0;
        /// <summary>
        /// ID de la fiche appelante (0 si création)
        /// </summary>
        public int nFileId { get; set; } = 0;
        /// <summary>
        /// Liste des infos des champs affiché sur la fiche (hors BDD) pour Uservalue (IsFound$|$Parameter$|$Value$|$Label)
        /// </summary>
        public string lstUserVal { get; set; }
        /// <summary>
        /// IDs de fiches à afficher en MRU si le terme de recherche est vide (ignoré si recherche ou Recherche étendue)</param>
        /// </summary>
        public string lstDispVal { get; set; }
        /// <summary>
        /// Option Rechercher sur toutes les fiches TABLE cochée
        /// </summary>
        public bool bSearchAllUserDefined { get; set; } = false;

        /// <summary>
        /// liste des descid des colonnes à afficher.
        /// </summary>
        public int[] lstDescDeduplicate { get; set; }
    }
}