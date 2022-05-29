using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Factories
{
    /// <summary>
    /// Comparateur pour les champs.
    /// </summary>
    public class FieldComparer : IEqualityComparer<Field>
    {
        /// <summary>
        /// Les champs sont égaux par leur Descid
        /// </summary>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <returns></returns>
        public bool Equals(Field f1, Field f2)
        {

            if (Object.ReferenceEquals(f1, f2)) 
                return true;

            if (Object.ReferenceEquals(f1, null) || Object.ReferenceEquals(f2, null))
                return false;

            return f1.Descid == f2.Descid;
        }

        /// <summary>
        /// retourne le Descid
        /// </summary>
        /// <param name="fld"></param>
        /// <returns></returns>
        public int GetHashCode(Field fld)
        {
            if (Object.ReferenceEquals(fld, null)) return 0;

            return fld.Descid;
        }
    }
}