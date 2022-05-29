using System;

namespace Com.Eudonet.Xrm
{
    /// <className>ExternalPageExp</className>
    /// <summary>Classe d'exception pour les page externes à l'appli</summary>
    /// <purpose></purpose>
    /// <authors>HLA</authors>
    /// <date>2014-08-22</date>
    public abstract class ExternalPageExp : Exception
    {
        /// <summary>
        /// Indique si on doit envoyé le feedback Permet de lever des exceptions silencieuses
        /// </summary>
        public bool SendFeedback { get; private set; }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="message">message d'erreur</param>
        /// <param name="sendFeedback">Active l'envoi du feedback</param>
        public ExternalPageExp(string message, bool sendFeedback = true)
            : base(message)
        {
            this.SendFeedback = sendFeedback;
        }
    }

    /// <className>TrackExp</className>
    /// <summary>Classe d'exception pour la page de tracking</summary>
    /// <purpose></purpose>
    /// <authors>HLA</authors>
    /// <date>2014-03-13</date>
    class TrackExp : ExternalPageExp
    {
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="message">message d'erreur</param>
        /// <param name="sendFeedback">Active l'envoi du feedback</param>
        public TrackExp(string message, bool sendFeedback = true)
            : base(message, sendFeedback)
        {
        }
    }

    /// <className>FormularExp</className>
    /// <summary>Classe d'exception pour la page du générateur de formulaires</summary>
    /// <purpose></purpose>
    /// <authors>MAB</authors>
    /// <date>2014-06-30</date>
    class FormularExp : ExternalPageExp
    {
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="message">message d'erreur</param>
        /// <param name="sendFeedback">Active l'envoi du feedback</param>
        public FormularExp(string message, bool sendFeedback = true)
            : base(message, sendFeedback)
        {
        }
    }

    class PjExp : ExternalPageExp
    {
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="message">message d'erreur</param>
        public PjExp(string message)
            : base(message, false)
        {
        }
    }

}