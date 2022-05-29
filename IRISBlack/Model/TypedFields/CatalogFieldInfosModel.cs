using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model.TypedFields
{
    /// <summary>
    /// retourne les champs de type catalogue
    /// </summary>
    public class CatalogFieldInfos : FldTypedInfosModel
    {

        public int PopupDescId { get; set; }
        public int BoundDescId { get; set; }
        public int PopupType { get; set; }
        public bool Multiple { get; set; }
        public bool IsTree { get; set; }
        public bool IsTreeViewOnlyLastChildren { get; set; }
        /// <summary>
        /// Pour un catalogue étape, le champ représente t-il une suite de valeurs ?
        /// </summary>
        [DefaultValue(false)]
        public bool IsSequenceMode { get; set; }
        public string BoundFieldPopup { get; set; }
        public string EAction { get; set; }
        public string DataEnumT { get; set; }
        public string DataDescT { get; set; }
        /// <summary>
        /// format d'affichage du champs caractère
        /// </summary>
        /// <example>Première lettre en capitale.</example>
        /// <example>Lettres en capitale.</example>
        /// <example>Tout en minuscules</example>
        [DefaultValue(CaseField.CASE_NONE)]
        public string DisplayFormat { get; set; } = CaseField.CASE_NONE.ToString();
        /// <summary>
        /// Valeurs disponibles pour le catalogue, avec leurs infobulles
        /// </summary>
        public List<ICatalogValue> CatalogValues { get; set; }

        internal CatalogFieldInfos(Field f, List<ICatalogValue> cv) : base(f)
        {
            Field fBoundField = f.AliasSourceField?.BoundField ?? f.BoundField;

            Format = FieldType.Catalog;
            PopupDescId = f.PopupDescId > 0 ? f.PopupDescId : f.Descid;
            BoundDescId = f.BoundDescid;
            PopupType = (f.IsCatEnum) ? (int)EudoQuery.PopupType.ENUM : (f.IsCatDesc) ? (int)EudoQuery.PopupType.DESC : (int)f.Popup;
            Multiple = f.Multiple;
            IsTree = f.bTreeView;
            IsTreeViewOnlyLastChildren = f.IsTreeViewOnlyLastChildren;
            IsSequenceMode = f.SequenceMode;
            EAction = (f.IsCatEnum) ? Catalog_Type.LNKCATENUM.ToString() : (f.IsCatDesc) ? Catalog_Type.LNKCATDESC.ToString() : Catalog_Type.LNKDEFAULT.ToString();
            DataEnumT = null;
            DataDescT = null;
            DisplayFormat = f.Case.ToString();

            if (f.IsCatEnum)
            {

                DataEnumT = ((int)eCatalogEnum.GetCatalogEnumTypeFromField(f)).ToString();
            }
            else if (f.IsCatDesc)
            {
                if (f.Descid == (int)RGPDTreatmentsLogsField.TabsID
                    || f.Descid == (int)WorkflowScenarioField.WFTEVENTDESCID
                    || f.Descid == (int)WorkflowScenarioField.WFTTARGETDESCID
                    )
                    DataDescT = ((int)eCatalogDesc.DescType.Table).ToString();
                else if (f.Descid == (int)RGPDTreatmentsLogsField.FieldsID)
                    DataDescT = ((int)eCatalogDesc.DescType.Field).ToString();
                
            }

            if (fBoundField != null)
            {
                BoundFieldPopup = ((int)fBoundField.Popup).ToString();
            }

            CatalogValues = cv;
        }

    }
}