using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model.TypedFields
{
    /// <summary>
    /// retourne les champs de type hyperlien
    /// </summary>
    public class HyperLinkFieldInfos : FldTypedInfosModel
    {

        internal HyperLinkFieldInfos(Field f) : base(f) {
            Format = FieldType.HyperLink;
        }

    }
}