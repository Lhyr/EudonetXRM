using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model.TypedFields
{
    /// <summary>
    /// retourne les champs de type Compteur automatique
    /// </summary>
    public class AutoCountFieldInfos : FldTypedInfosModel
    {

        internal AutoCountFieldInfos(Field f) : base(f)
        {
            Format = FieldType.AutoCount;
        }

    }
}