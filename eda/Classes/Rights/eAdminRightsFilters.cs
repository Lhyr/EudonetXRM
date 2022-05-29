using System.Collections.Generic;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Classe représentant les filtres en entête des droits
    /// </summary>
    public class eAdminRightsFilters
    {
        /// <summary>
        /// Onglet
        /// </summary>
        public int Tab { get; set; } = 0;
        /// <summary>
        /// Vu depuis
        /// </summary>
        public int From { get; set; } = 0;
        /// <summary>
        /// Rubrique
        /// </summary>
        public int Field { get; set; } = 0;
        /// <summary>
        /// Type
        /// </summary>
        public HashSet<eTreatmentType> TreatTypes { get; set; } = new HashSet<eTreatmentType>();
        /// <summary>
        /// Fonction
        /// </summary>
        public string Function { get; set; } = string.Empty;
    }
}