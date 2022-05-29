using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Format json n'est pas valide
    /// </summary>
    public class eSaml2JsonFormatException : Exception
    {
        /// <summary>
        /// Constructeur de l'excpetion
        /// </summary>
        /// <param name="message">Raison de l'exception</param>
        /// <param name="innerException">Excetion interne</param>
        public eSaml2JsonFormatException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}