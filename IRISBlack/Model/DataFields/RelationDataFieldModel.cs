using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model.DataFields
{
    /// <summary>
    /// retourne les champs de type Relation
    /// </summary>
    public class RelationDataFieldModel : DataFieldWithDisplayModel
    {

        internal RelationDataFieldModel(eFieldRecord f) : base(f)
        {
        }

    }
}