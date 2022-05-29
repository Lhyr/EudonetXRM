using EudoQuery;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Mapping entre l'attribut saml2 et le champ de la table user
    /// </summary>
    public class eSaml2Map
    {
        /// <summary>
        /// Nom de l'attribute saml2 dans l'assertion
        /// </summary>
        public string Saml2Attribute { get; set; } = string.Empty;

        /// <summary>
        /// Descid du champ 
        /// </summary>
        public string DescId { get; set; } = "0";

        /// <summary>
        /// Descid du champ 
        /// </summary>
        public bool IsKey { get; set; } = false;
    }
}