using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model
{
    /// <summary>
    /// Liste des valeurs d'un catalogue
    /// </summary>
    public class CatalogValuesModel
    {
        public List<ICatalogValue> Values = new List<ICatalogValue>();
        public class Value : ICatalogValue
        {
            public string DbValue { get; set; }
            public string DisplayLabel { get; set; }
            public string Code { get; set; }
            public bool Hidden { get; set; }
            [DefaultValue(0)]
            public int ParentId { get; set; }
            /// <summary>
            /// Infobulle de la valeur de catalogue, présentée dans la langue de l'utilisateur
            /// </summary>
            public string ToolTipText { get; set; }
        }
    }
}