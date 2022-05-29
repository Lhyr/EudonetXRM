using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm
{

    /// <summary>
    /// Exception sur l'interogration de la base
    /// </summary>
    public class eSaml2SqlException : Exception
    {
        /// <summary>
        /// Constructeur de l'excpetion
        /// </summary>
        /// <param name="message">Raison de l'exception</param>       
        public eSaml2SqlException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Exception sur l'interogration de la base
    /// </summary>
    public class eSaml2Exception : Exception
    {
        /// <summary>
        /// Message utilisateur
        /// </summary>
        public string UserMessage;

        /// <summary>
        /// Message détaillé
        /// </summary>
        public string UserDetailMessage;

       /// <summary>
       /// Exception sur le gestion SAML
       /// </summary>
       /// <param name="userMessage">Message Utilisateur</param>
       /// <param name="userDetail">Détail de l'erreur</param>
       /// <param name="devMessage">Détail sur l'erreur technique</param>
        public eSaml2Exception(string userMessage, string userDetail, string devMessage) : base(devMessage)
        {
            UserMessage = userMessage;
            UserDetailMessage = userDetail;
        }
    }
}