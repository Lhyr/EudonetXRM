using Com.Eudonet.Xrm.IRISBlack.Controllers;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model.TypedFields
{
    /// <summary>
    /// retourne les champs de type Téléphone
    /// </summary>
    public class PhoneFieldInfos : FldTypedInfosModel
    {
        public bool DisplaySmsBtn { get; set; }

        internal PhoneFieldInfos(Field f, bool bDisplayBtn) : base(f)
        {
            Format = FieldType.Phone;
            DisplaySmsBtn = bDisplayBtn;
        }

    }
}