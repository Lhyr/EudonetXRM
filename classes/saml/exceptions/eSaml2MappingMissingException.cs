using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Manque de champ mappé
    /// </summary>
    public class eSaml2MappingMissingException : Exception
    {
        /// <summary>
        /// Constructeur de l'excpetion
        /// </summary>
        /// <param name="message">Raison de l'exception</param>       
        public eSaml2MappingMissingException(string message) : base(message)
        {
        }
    }
}