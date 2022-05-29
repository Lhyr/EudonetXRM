using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model.DataFields
{
    /// <summary>
    /// retourne les champs de type hyperlien
    /// </summary>
    public class HyperLinkDataFieldModel : DataFieldWithValueModel
    {

        internal HyperLinkDataFieldModel(eFieldRecord f) : base(f)
        {
        }

    }
}