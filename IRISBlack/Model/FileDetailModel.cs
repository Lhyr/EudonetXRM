using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.IRISBlack.Factories;
using Com.Eudonet.Xrm.IRISBlack.Model.TypedFields;
using EudoQuery;

namespace Com.Eudonet.Xrm.IRISBlack.Model
{
    public class FileDetailModel
    {
        public RecordModel Data { get; set; }
        public StructureModel Structure { get; set; }
        public AutoCompletionMappingModel SireneMapping { get; set; }
        public AutoCompletionMappingModel PredictiveAddressMapping { get; set; }
        public PagingInfoModel pagingInfo { get; set; }
        public List<Exception> Errors { get; set; }
        public FileDetailFKLinksModel FdLinksId { get; set; }
        public string BaseName { get; set; }
        public int Revision { get; set; }
        public ActionDetailModel actDetail { get; set; }


        public class StructureModel
        {
            public StructFileModel StructFile { get; set; }
            public List<IFldTypedInfosModel> LstStructFields { get; set; } = new List<IFldTypedInfosModel>();
            public List<StructBkmModel> LstStructBkm { get; set; } = new List<StructBkmModel>();
            public string WebDataPath { get; set; }
        }

        public class StructFileModel
        {
            public int DescId { get; set; }
            public string Label { get; set; }
            public int EdnType { get; set; }
            /// <summary>
            /// Hash utilisé pour les liens eGotoFile
            /// </summary>
            public string FileHash { get; set; }
            /// <summary>
            /// Table des champs
            /// </summary>
            public TableInfosModel Table { get; set; }
        }

        public class StructBkmModel
        {
            public int DescId { get; set; }
            public string Label { get; set; }
            public int TableType { get; set; }
            public bool HistoricActived { get; set; }
            public bool ExpressFilterActived { get; set; }
            public int ViewMode { get; set; }
            public DisplayButtons Actions { get; set; }
            public string Error { get; set; }

            public int RelationFieldDescId { get; set; }

            /// <summary>
            /// Etapes marketting en pause
            /// </summary>
            public bool IsMarkettingStepHold { get; set; }
            /// <summary>
            /// Savoir si le bookmark est épinglé dans le new mode fiche.
            /// </summary>
            public bool IsPinned { get; set; }
            /// <summary>
            /// Ordre d'affichage du signet lorsqu'il a été épinglé
            /// </summary>
            public int PinnedOrder { get; set; }

            /// <summary>
            /// liste de table de type de cible étendu
            /// </summary>
            public List<KeyValuePair<int,string>> ListTargetScenario { get; set; }

            public class DisplayButtons
            {
                public bool Add { get; set; }
                public bool AddFromFilter { get; set; }
                public bool AddPurpleFile { get; set; }

                public bool Historic { get; set; }

                public bool DeleteFromFilter { get; set; }
                public bool Import { get; set; }
                public bool ImportTarget { get; set; }
                public bool Export { get; set; }

                public bool Merge { get; set; }
                public bool Chart { get; set; }
                public bool Print { get; set; }
                public bool Mailing { get; set; }
                public bool SMS { get; set; }
                public bool Formular { get; set; }
                //tâche #3 095 : bouton Formulaire Avancé
                public bool AdvFormular { get; set; }
                public bool SwitchViewFile { get; set; }
                public bool MarketingAutomation { get; set; }

                //bouton action markkting en pause
                public bool HoldMarkettingStep { get; internal set; }

                public bool EventTargetForScenario { get; internal set; }
            }
        }


    }
}