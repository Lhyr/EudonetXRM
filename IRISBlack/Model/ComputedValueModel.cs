using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model
{
    /// <summary>
    /// Modele de présentation des données pour la somme des colonnes
    /// </summary>
    public class ComputedValueModel
    {
        /// <summary>
        /// Alias de la colonne concernée
        /// </summary>
        public string Alias { get; set; }
        /// <summary>
        /// DescId de la colonne concernée
        /// </summary>
        public int DescId { get; set; }
        /// <summary>
        /// Valeur calculée (formatée pour l'affichage)
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// Nombre de décimales à utiliser pour DecimalValue
        /// </summary>
        public int DecimalCount { get; set; }
        /// <summary>
        /// Valeur calculée (décimale)
        /// </summary>
        public decimal DecimalValue { get; set; }
    }
}