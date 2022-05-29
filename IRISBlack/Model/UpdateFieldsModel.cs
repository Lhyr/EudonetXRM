using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Com.Eudonet.Engine;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;

namespace Com.Eudonet.Xrm.IRISBlack.Model
{
    public class UpdateFieldsModel
    {
        public List<eUpdateField> Fields { get; set; }
        public int TabDescId { get; set; }
        public int FileId { get; set; } = 0;
        public int ParentTabDescId { get; set; }
        public int ParentFileId { get; set; }
        /// <summary>
        /// ATTENTION : côté JS, la variable se nomme "crtldescid" et non "ctrldescid")
        /// </summary>
        public string CtrlDescId { get; set; } 
        public XrmCruAction EngineAction { get; set; }
        public bool AutoComplete { get; set; }
        public string FieldEditorType { get; set; }
        public bool InsertCatalogValue { get; set; }
        public string FieldTrigger { get; set; }
        public string AddressToUpdate { get; set; }
        public string Refresh { get; set; }
        public string BkmIds;

        public bool TriggerOnBlurAction { get; set; }
        public bool TriggerOnValidFileAction { get; set; }

        public int CloneFileId { get; set; }

        public string NewDateBegin { get; set; }
        public string NewDateEnd { get; set; }

        public string MailDisplayName { get; set; }
        public string MailReplyTo { get; set; }
        public string MailCSS { get; set; }
        public string MailPJ { get; set; }
        public bool MailSaveAsDraft { get; set; }
        public bool MailIsDraft { get; set; }
        public bool MailIsText { get; set; }

        public int OrmId { get; set; }
        public string OrmUpdates { get; set; }
        public string OrmResponseObj { get; set; }
    }

    /// <summary>
    /// INNER CLASSES 
    /// Source : eUpdateFieldManager
    /// </summary>
    public class EdnUpdFldContext
    {
        internal eUpdateField Field { get; private set; }

        internal EdnUpdFldContext(eUpdateField fld)
        {
            Field = fld;
        }

        /// <summary>
        /// Convertit en représentation chaîne
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Concat(Field.ToString());
        }
    }

    /// <summary>
    /// Contexte de MAJ
    /// </summary>
    public class EdnUpdContext
    {

        #region propriétés

        internal UpdType EdnUpdTyp { get; set; }

        internal XrmCruAction CruAction { set; get; }

        internal int FileId { set; get; }
        internal int TabDescId { set; get; }
        internal int TabFromDescId { set; get; }
        internal int FileIdFrom { set; get; }

        internal string FieldEditorType { set; get; }
        internal bool InsertCatalogValue { set; get; }

        internal List<EdnUpdFldContext> LstUpdFld { get; private set; }
        internal eParameters Params { get; private set; }
        internal eParameters CloneParams { set; get; }

        internal bool AutoComplete;
        #endregion

        internal EdnUpdContext()
        {
            CruAction = XrmCruAction.UPDATE;
            EdnUpdTyp = UpdType.CLASSIC;
            LstUpdFld = new List<EdnUpdFldContext>();
            Params = new eParameters();
            CloneParams = new eParameters();
            AutoComplete = false;
        }

        internal enum UpdType
        {
            CLASSIC,
            CLONE_BKM,
            CLONE_PLANNING
        }
    }
}