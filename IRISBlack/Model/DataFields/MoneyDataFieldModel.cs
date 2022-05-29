using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model.DataFields
{
    /// <summary>
    /// retourne les champs de type numeraire
    /// </summary>
    public class MoneyDataFieldModel : DataFieldWithDisplayModel
    {

        internal MoneyDataFieldModel(eFieldRecord f) : base(f)
        {
        }

    }
}