using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Interface permetant de lancer la generation de la liste et la retourner
    /// </summary>
    public interface eMultiSelectDataSourceInterface
    {
        /// <summary>
        /// Titre de l'entete du multi-select 
        /// </summary>
        /// <returns></returns>
        string GetHeaderTitle();

        /// <summary>
        /// Titre de la partie des élements sources disponible
        /// </summary>
        /// <returns></returns>
        string GetSourceItemsTitle();

        /// <summary>
        /// Titre de la partie des elements ciblés sélectionnés
        /// </summary>
        /// <returns></returns>
        string GetTargetItemsTitle();

        /// <summary>
        /// Lance la génération des élements du multi-select
        /// </summary>
        /// <returns></returns>
        bool Generate();               

        /// <summary>
        /// Les elements sources disponibles
        /// </summary>
        /// <returns></returns>
        IEnumerable<eMultiSelectItem> GetSourceItems();

        /// <summary>
        /// Les elements sélectionnés cibles
        /// </summary>
        /// <returns></returns>
        IEnumerable<eMultiSelectItem> GetTargetItems();
    }
}