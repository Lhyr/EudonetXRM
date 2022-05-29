using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model.TypedFields
{
    /// <summary>
    /// retourne les champs de type Adresse email
    /// </summary>
    public class MailFieldInfos : FldTypedInfosModel
    {

        internal MailFieldInfos(Field f) : base(f)
        {
            Format = FieldType.MailAddress;
        }

    }
}