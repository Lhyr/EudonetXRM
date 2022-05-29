using System;

namespace Com.Eudonet.Xrm.Import
{
    /// <summary>
    /// Représentation objet d'un colonne des données à importer
    /// </summary>
    public class eImportContentColumn
    {
        /// <summary>
        /// Index de la colonne (debute par 0)
        /// </summary>
        public Int32 Index { get; set; }
        /// <summary>
        /// Descid de la rubrique eudo
        /// </summary>
        public Int32 DescId { get; set; }
        /// <summary>
        /// Nom de la rub eudo
        /// </summary>
        public String EudoColLabel { get; set; }
        /// <summary>
        /// Nom de la colonne du fichier
        /// </summary>
        public String FileColLabel { get; set; }
        /// <summary>
        /// Indicateur si on dédoublonne sur la colonne en question
        /// </summary>
        public Boolean IsKey { get; set; }

        /// <summary>
        /// Constructeur
        /// </summary>
        public eImportContentColumn()
        {
            this.EudoColLabel = String.Empty;
            this.FileColLabel = String.Empty;
        }
    }
}