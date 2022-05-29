using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model.TypedFields
{
    /// <summary>
    /// retourne les champs de type numerique
    /// </summary>
    public class PJFieldInfos : FldTypedInfosModel
    {

        internal PJFieldInfos(Field f) : base(f)
        {
            Format = FieldType.PJ;
        }

    }
}