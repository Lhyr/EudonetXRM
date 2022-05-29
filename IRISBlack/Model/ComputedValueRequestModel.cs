using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model
{
    /// <summary>
    /// paramétrage de la requête qui permets d'obtenir les valeurs disponibles pour les champs de type user
    /// </summary>
    public class ComputedValueRequestModel
    {
        /// <summary>
        /// Liste de DescIDs des colonnes concernées
        /// </summary>
        public int[] ListCol = new int[0];
        /// <summary>
        /// TabID du fichier concerné
        /// </summary>
        public int Tab = 0;
        /// <summary>
        /// TabID du fichier parent concerné
        /// </summary>
        public int ParentTab = 0;
        /// <summary>
        /// FileID du fichier parent concerné
        /// </summary>
        public int ParentFileId = 0;
        /// <summary>
        /// FileID de la fiche Sociétés (PM) parente
        /// </summary>
        public int ParentPMFileId = 0;
    }
}