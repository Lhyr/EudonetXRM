using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model
{
    /// <summary>
    /// Modele de présentation des MRU
    /// </summary>
    public class MRUModel
    {
        /// <summary>
        /// Liste des valeurs récemment utilisées;
        /// </summary>
        public List<Value> Values { get; set; } = new List<Value>();
        /// <summary>
        /// représente une valeur de MRU
        /// </summary>
        public class Value : IMruValue
        {
            /// <summary>
            /// valeur en base
            /// </summary>
            /// j'aurais bien mis un int mais pour les champs de type user multiple on peut avoir des valeurs de type G2
            public string DbValue { get; set; }
            /// <summary>
            /// valeur affichée dans l'interface
            /// </summary>
            public string DisplayLabel { get; set; }
            /// <summary>
            /// Liaison vers PP
            /// </summary>
            public IMruValue PP { get; set; }
            /// <summary>
            /// Liaison vers PM
            /// </summary>
            public IMruValue PM { get; set; }
            /// <summary>
            /// Liste des éléments de déduplication.
            /// </summary>
            public IEnumerable<MRUValuesDeduplicatingModel> ListDeduplicatingFields { get; set; }
        }
    }
}