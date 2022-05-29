using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model.DataFields
{
    /// <summary>
    /// retourne les champs de type Etiquette
    /// </summary>
    public class SeparatorDataFieldModel : DataFieldWithValueModel
    {

        internal SeparatorDataFieldModel(eFieldRecord f) : base(f)
        {
            if (String.IsNullOrEmpty(f.Value))
                Value = "0";
        }
    }
}