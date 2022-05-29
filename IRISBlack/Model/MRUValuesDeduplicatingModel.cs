using Com.Eudonet.Xrm.IRISBlack.Model.TypedFields;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model
{
    /// <summary>
    /// Petite classe pour les éléments de déduplication des MRU
    /// </summary>
    public class MRUValuesDeduplicatingModel
    {
        public int DescId { get; set; }
        public string Label { get; set; }
        public string Value { get; set; }
        public string DisplayValue { get; set; }

        public FieldType Type { get; set; }

    }
}