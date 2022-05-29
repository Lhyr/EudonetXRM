using Com.Eudonet.Core.Model;
using Com.Eudonet.Xrm.IRISBlack.Model.TypedFields;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model
{
    public class ListDetailModel
    {
        public List<IRecordModel> Data { get; set; }
        public StructureModel Structure { get; set; }

        public PagingInfoModel PagingInfo { get; set; }


        /// <summary>
        /// nombre de fiches renvoyées par la requête
        /// </summary>
        public int NbResults { get; set; }

        public class StructureModel
        {
            public List<IFldTypedInfosModel> LstStructFields { get; set; } = new List<IFldTypedInfosModel>();
            public string WebDataPath { get; set; }
            public int MainFieldId { get; set; }
            public int ViewMainTab { get; set; }
            public bool ExpressFilterActivated { get; set; }
            public EdnType StructType { get; set; }

            public string Label { get; set; }

            /// <summary>Libellé pour un seul résultat dans le compte d'enregistrement</summary>
            public string SingularResultLabel { get; set; }
            /// <summary>Libellé pour plusieurs résultats dans le compte d'enregistrement</summary>
            public string PluralResultsLabel { get; set; }

            /// <summary>Libellé du filtre avancé actif sur la liste</summary>
            public string AdvancedFilterLabel { get; set; }
            /// <summary>Libellé de la sélection de fiches marquées actives sur la liste</summary>
            public string MarkedFilesSelectionLabel { get; set; }

        }
    }


}