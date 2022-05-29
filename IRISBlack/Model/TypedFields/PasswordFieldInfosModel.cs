using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model.TypedFields
{
    /// <summary>
    /// retourne les champs de type caractère
    /// </summary>
    public class PasswordFieldInfos : FldTypedInfosModel
    {

        internal PasswordFieldInfos(Field f) : base(f) {
            Format = FieldType.Password;
        }

    }
}