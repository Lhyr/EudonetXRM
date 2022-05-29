using Com.Eudonet.Internal;
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
    /// Encapsule les infos de l'attribut
    /// </summary>
    public class eSaml2Attribute
    {  
        /// <summary>
        /// Nom de l'attribut
        /// </summary>        
        public string AttributeName;

        /// <summary>
        /// Valeur de l'idp
        /// </summary>
        public string AttributeValue;
    }
}