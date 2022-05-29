using EudoQuery;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model
{
    /// <summary>
    /// Modèle de données pour les caractéristiques RGPD d'un champ
    /// </summary>
    public class RGPDModel
    {
        /// <summary>
        /// Nature de la donnée RGPD : Personnelle, Sensible
        /// </summary>
        public DESCADV_RGPD_NATURE NatureType { get; set; }
        /// <summary>
        /// Nature de la donnée RGPD : Personnelle, Sensible
        /// </summary>
        public string NatureLabel { get; set; }
        /// <summary>
        /// Type de la catégorie RGPD (Personnelle ou Sensible)
        /// </summary>
        public string CategoryType { get; set; }
        /// <summary>
        /// Libellé de la catégorie RGPD (qu'il s'agisse d'une donnée Sensible ou Personnelle)
        /// </summary>
        public string CategoryLabel{ get; set; }
        /// <summary>
        /// La gestion RGPD est-elle activée pour ce champ ?
        /// </summary>
		[DefaultValue(1)]
        public bool Enabled { get; set; }		
    }
}