using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Com.Eudonet.Xrm.eda
{

    /// <summary>
    /// Pour gérer les différentes source de la permission, depuis desc ou grille ou autre
    /// </summary>
    public interface IAdminTreatmentRight : IComparable<IAdminTreatmentRight>
    {
        /// <summary>
        /// Id de traitement ou Id de la fiche auquelle est associée cette perm (cas des grilles)
        /// </summary>
        int TraitID { get; }
        /// <summary>
        /// DescId de la rubrique ou la table cible du traitement
        /// </summary>
        int DescID { get; set; }


        /// <summary>
        /// Type de traitement
        /// </summary>
        eTreatmentType Type { get; }

        /// <summary>
        /// Permission définit sur la table, la rubrique, la grille ou autre 
        /// </summary>
        ePermission Perm { get; }

        /// <summary>
        /// Libellé de la table
        /// </summary>
        string TabLabel { get; }

        /// <summary>
        /// Libellé de la table vu depuis
        /// </summary>
        string TabFromLabel { get; }

        /// <summary>
        /// Libellé du champ
        /// </summary>
        string FieldLabel { get; }

        /// <summary>
        /// Libellé du traitement
        /// </summary>
        string TraitLabel { get; }

        /// <summary>
        /// Libelle du type de filtre
        /// </summary>
        string TypeLabel { get; }

        /// <summary>
        /// Localisation du traitement
        /// </summary>
        eLibConst.TREATMENTLOCATION TreatLoc { get; set; }

    } 
}