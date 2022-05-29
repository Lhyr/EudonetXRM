using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model.DataFields
{
    /// <summary>
    /// retourne les champs de type Compteur automatique
    /// </summary>
    public class AutoCountDataFieldModel : DataFieldWithDisplayModel
    {

        internal AutoCountDataFieldModel(eFieldRecord f) : base(f)
        {
            ReadOnly = true;
        }

    }
}