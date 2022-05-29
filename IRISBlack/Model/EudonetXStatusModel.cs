using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model
{
    /// <summary>
    /// Modèle représentant les status d'activation des différentes fonctionnalités Eudonet X (Mode Fiche IRIS Black, Mode Liste IRIS Crimson, Saisie Guidée IRIS Purple...) pour un onglet donné
    /// </summary>
    public class EudonetXStatusModel
    {
        /// <summary>
        /// TabID (DescID) de l'onglet concerné
        /// </summary>
        public int Tab { get; set; }
        /// <summary>
        /// Indique le statut d'activation du mode Fiche IRIS Black pour l'onglet
        /// </summary>
        public EUDONETX_IRIS_BLACK_STATUS EudonetXIrisBlackStatus { get; set; } = EUDONETX_IRIS_BLACK_STATUS.UNDEFINED;
        /// <summary>
        /// Indique le statut d'activation du mode Liste IRIS Crimson pour l'onglet
        /// </summary>
        public EUDONETX_IRIS_CRIMSON_LIST_STATUS EudonetXIrisCrimsonListStatus { get; set; } = EUDONETX_IRIS_CRIMSON_LIST_STATUS.UNDEFINED;
        /// <summary>
        /// Indique le statut d'activation du mode Saisie Guidée IRIS Purple pour l'onglet
        /// </summary>
        public EUDONETX_IRIS_PURPLE_GUIDED_STATUS EudonetXIrisPurpleGuidedStatus { get; set; } = EUDONETX_IRIS_PURPLE_GUIDED_STATUS.UNDEFINED;
    }
}