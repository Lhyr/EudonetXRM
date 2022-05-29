using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model
{
    /// <summary>
    /// paramétrage avancé des valeurs de catalogues que l'on souhaite obtenir
    /// </summary>
    public class CatalogValuesRequestModel
    {
        /// <summary>descid du catalog</summary>
        public int DescId { get; set; }

        /// <summary>Type de Catalogue </summary>
        public int PopupType { get; set; }

        /// <summary>Indique s'il faut afficher les valeurs désactivées </summary>
        public bool ShowHiddenValue { get; set; }

        /// <summary>
        /// Indique si l'arborescence est activée
        /// </summary>
        public bool Treeview { get; set; }

        /// <summary>Recherche sur le catalogue </summary>
        public string SearchPattern { get; set; }

        /// <summary>Id de la valeur dans la rubrique parente (catalogues liés)</summary>
        public int ParentId { get; set; }
    }
}