using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model
{
    /// <summary>
    /// Modele pour les fichiers dont on veut vérifier
    /// la capacité à être copier.
    /// </summary>
    public class CheckFileModel
    {
        /// <summary>
        /// l'echec ou le succes de la copie.
        /// </summary>
        public bool Successfull { get; set; }

        /// <summary>
        /// le véritable nom du fichier
        /// </summary>
        public string RealName { get; set; }

        /// <summary>
        /// Le nom suggéré pour le fichier.
        /// </summary>
        public string SuggestedName { get; set; }

        /// <summary>
        /// Le titre de la fenêtre qui devra s'afficher
        /// </summary>
        public string WindowsTitle { get; set; }

        /// <summary>
        /// la description de la fenêtre qui devra s'afficher
        /// </summary>
        public string WindowsDescription { get; set; }
    }
}