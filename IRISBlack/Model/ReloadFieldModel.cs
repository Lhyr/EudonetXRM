using Com.Eudonet.Engine.Result.Data;
using Com.Eudonet.Xrm.IRISBlack.Factories;
using Com.Eudonet.Xrm.IRISBlack.Model.TypedFields;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model
{
    public class Record
    {
        public int FileId { get; set; }
        public string Value { get; set; }
        public string ParentValue { get; set; }
        public string DisplayValue { get; set; }
    }

    public class ReloadFieldModel
    {
        public int DescId { get; set; }
        public int Format { get; set; }
        public bool IsLink { get; set; }

        public List<Record> Records { get; set; }

        public ReloadFieldModel(ListRefreshFieldNewValue refreshFieldNewValue)
        {
            DescId = refreshFieldNewValue.Field.Descid;
            Format = (int)FldTypedInfosFactory.InitFldTypedInfosFactory((Field)refreshFieldNewValue.Field).GetFieldType();
            IsLink = refreshFieldNewValue.Field.Popup == PopupType.SPECIAL;

            Records = refreshFieldNewValue.List.Select(newVal => new Record {
                FileId = newVal.FileId,
                DisplayValue = newVal.DisplayValue,
                Value = newVal.DbValue,
                ParentValue = newVal.ParentDbValue
            }).ToList();

        }

    }
}