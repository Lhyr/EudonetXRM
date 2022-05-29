using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model.TypedFields
{
    /// <summary>
    /// retourne les champs de type Etiquette
    /// </summary>
    public class SeparatorFieldInfos : FldTypedInfosModel
    {

        internal SeparatorFieldInfos(Field f) : base(f)
        {
            Format = FieldType.Separator;
        }

    }
}