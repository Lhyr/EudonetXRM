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
    /// Information complete sur le champ mapping 
    /// </summary>
    public class eSaml2Field
    {
        /// <summary>
        /// Descid du champ
        /// </summary>
        public int DescId;

        /// <summary>
        /// La colonne sql correspondant à l'attribut mappé
        /// </summary>        
        public string ColumnName;

        /// <summary>
        /// La valeur en chaine de l'attribut
        /// </summary>
        public object ColumnValue;

        /// <summary>
        /// Le chamm est utilisé comme clé
        /// </summary>
        public bool IsKey;

        /// <summary>
        /// Type de la colonne sql
        /// </summary>
        public SqlDbType SqlDataType = SqlDbType.Char;      
    }
}