using Com.Eudonet.Xrm.IRISBlack.Model.DataFields;
using System.Collections.Generic;

namespace Com.Eudonet.Xrm.IRISBlack.Model
{
    public interface IRecordModel
    {
        string BGColor { get; set; }
        string Color { get; set; }
        string Icon { get; set; }
        int MainFileId { get; set; }
        string MainFileLabel { get; set; }
        PJUploadInfoModel PJInfo { get; set; }
        bool RightIsDeletable { get; set; }
        bool RightIsUpdatable { get; set; }
        IEnumerable<IDataFieldModel> LstDataFields { get; set; }
        FieldsMenuShortcutModel MenuShortcut { get; set; }
        /// <summary>
        /// US #4315 - Droit de modification de la zone Assistant du nouveau mode Fiche Eudonet X
        /// </summary>
        bool CanUpdateWizardBar { get; set; }

    }
}