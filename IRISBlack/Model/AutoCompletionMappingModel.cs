using Com.Eudonet.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model
{
    /// <summary>
    /// Mapping permettant la mise à jour par autocompletion
    /// </summary>
    public class AutoCompletionMappingModel
    {
        /// <summary>
        /// Url du web service à interroger
        /// </summary>
        public String ProviderURL { get; set; }

        /// <summary>
        /// Liste des lignes de mapping
        /// </summary>
        public IList<MapLine> Mapping { get; set; } = new List<MapLine>();

        /// <summary>
        /// Champs déclencheur
        /// </summary>
        public IList<int> Triggers { get; set; } = new List<int>();

        /// <summary>
        /// constructeur vide
        /// </summary>
        public AutoCompletionMappingModel()
        {
        }

        /// <summary>
        /// Constructeur à partir d'un jeu de filemappartner
        /// </summary>
        /// <param name="sProviderURL"></param>
        /// <param name="liFmp"></param>
        public AutoCompletionMappingModel(string sProviderURL, IList<eFilemapPartner> liFmp, IList<int> liTriggers) {

            ProviderURL = sProviderURL;
            Mapping = liFmp.Where(fmp => fmp.DescId > 0).Select(fmp => new MapLine(fmp)).ToList();
            Triggers = liTriggers;
        }


        /// <summary>
        /// Représente une ligne du mapping
        /// </summary>
        public class MapLine
        {
            /// <summary>
            /// descid du champ mappé
            /// </summary>
            public int DescId { get; set; }

            /// <summary>
            /// Nom de la source
            /// </summary>
            public String Source { get; set; }

            /// <summary>
            /// Indique si on autorise la création de la valeur dans le champ s'il s'agit d'un catalogue
            /// </summary>
            public bool CreateCatalogValue { get; set; }

            /// <summary>
            /// Constructeur
            /// </summary>
            /// <param name="fmp"></param>
            public MapLine(eFilemapPartner fmp) {

                DescId = fmp.DescId;
                Source = fmp.Source;
                CreateCatalogValue = fmp.CreateCatalogValue;
            }

        }
    }
}