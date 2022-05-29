using System;
using System.Collections.Generic;

namespace Com.Eudonet.Xrm.Import
{
    /// <summary>
    /// Représentation objet d'un ligne des données à importer
    /// </summary>
    public class eImportContentLine
    {
        /// <summary>
        /// Index de la ligne (debute par 0)
        /// </summary>
        public Int32 Index { get; set; }
        /// <summary>
        /// Valeurs de la ligne
        /// </summary>
        public IList<String> Values { get; set; }

        /// <summary>
        /// Constructeur
        /// </summary>
        public eImportContentLine()
        {
            this.Values = new List<String>();
        }
    }
}