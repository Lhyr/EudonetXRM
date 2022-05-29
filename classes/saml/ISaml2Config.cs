using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Interface permettant de définir le parametrage Saml 2.0
    /// </summary>
    public interface ISaml2Config
    {
        /// <summary>
        /// Définit la source et l'entityId du service provider
        /// </summary>
        string Issuer { get; }

        /// <summary>
        /// Définit l'url de la page d'authentification du founisseur d'identity
        /// </summary>
        string SignOnDestination{ get; }

        /// <summary>
        /// Définit le mode du binding : POST ou redirecte
        /// </summary>
        string SignOnDestinationBinding { get; }

        /// <summary>
        /// Définit l'url de la page de déconnexion du founisseur d'identity
        /// </summary>
        string LogoutDestination { get; }

        /// <summary>
        /// Définit l'url de la page qui sera rappelée avec l'assertion
        /// </summary>
        string AssertionConsumerServiceLocation { get; }

        /// <summary>
        /// Définit le mode du binding : POST ou redirecte
        /// </summary>
        string AssertionConsumerServiceBinding { get; }

        /// <summary>
        /// Définit l'algorithme de hashage de la signature
        /// </summary>
        string SignAlgorithm { get; }

        /// <summary>
        /// Pour inclure dans la demande, le souhait que l'utilisaeur s'authentifie
        /// </summary>
        bool ForceAuthen { get; }

        /// <summary>
        /// Pour inclure une signature de la requete d'authentification dans l'url
        /// </summary>
        bool SignRequest { get; }

        /// <summary>
        /// L'assertion devera être signée
        /// </summary>
        bool WantAssertionsSigned { get; }

        /// <summary>
        /// Formats des identifiants
        /// </summary>
        IEnumerable<string> NameIDFormats { get; }

        /// <summary>
        /// Formats des identifiants
        /// </summary>
        IEnumerable<string> AttributeStatments { get; }
    }
}