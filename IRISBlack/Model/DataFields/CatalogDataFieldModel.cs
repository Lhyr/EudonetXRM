using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model.DataFields
{
    /// <summary>
    /// retourne les champs de type caractère
    /// </summary>
    public class CatalogDataFieldModel : DataFieldWithDisplayModel
    {
        public string Pdbv { get; set; }

        internal CatalogDataFieldModel(eFieldRecord f) : base(f)
        {
            Pdbv = f.BoundFieldValue;
        }
    }
}