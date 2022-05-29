using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model.DataFields
{
    /// <summary>
    /// retourne les champs de type relation d'en tete (liaison haute dans le corps de la page)
    /// </summary>
    public class SystemRelationDataFieldModel : DataFieldWithDisplayModel
    {

        internal SystemRelationDataFieldModel(eFieldRecord f) : base(f)
        {
            Value = f.FileId.ToString();
        }

    }
}