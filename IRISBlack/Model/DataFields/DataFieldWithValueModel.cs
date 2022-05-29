using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model.DataFields
{
    /// <summary>
    /// Retourne des objets représentant les champs typés qui contienne une valeur
    /// <seealso cref="DataFieldModel"/>
    /// </summary>
    public abstract class DataFieldWithValueModel : DataFieldModel
    {
        [DefaultValue(false)]
        public bool ReadOnly { get; set; }

        [DefaultValue(false)]
        public bool Required { get; set; }

        [DefaultValue(0)]
        public int FileId { get; set; }

        protected DataFieldWithValueModel(eFieldRecord f) : base(f)
        {
            if (IsVisible)
                Value = f.Value;
            else
                Value = "";

            if (f.FldInfo.Table.EdnType == EdnType.FILE_HISTO)
                DisplayValue = f.DisplayValue;

            ReadOnly = !f.RightIsUpdatable || f.FldInfo.ReadOnly;
            Required = f.IsMandatory;
            FileId = f.FileId;
        }


    }

}