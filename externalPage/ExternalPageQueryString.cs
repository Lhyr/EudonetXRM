using System.Collections.Generic;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Informations sur l'URL des pages externalisées
    /// </summary>
    /// <className>ExternalPageQueryString</className>
    /// <summary>Informations sur l'URL des pages externalisées</summary>
    /// <purpose></purpose>
    /// <authors>HLA</authors>
    /// <date>2017-12-12</date>
    internal class ExternalPageQueryString
    {
        internal const string LOG_KEY = "log";
        internal const string LOG_DOS_KEY = "dos";

        /// <summary>
        /// Représente l'UID de la base
        /// </summary>
        public string Token { get; private set; }
        /// <summary>
        /// Token de sécurité
        /// </summary>
        public string Cs { get; private set; }
        /// <summary>
        /// Param de sécurité
        /// </summary>
        public string P { get; private set; }

        /// <summary>
        /// UID de la base
        /// </summary>
        public string UID { get { return Token; } }

        /// <summary>
        /// Indicateur si on active les traces de la page
        /// => s'active uniquement sur la page
        /// </summary>
        public bool Log { get; private set; }
        /// <summary>
        /// Indicateur transmis au gestionnaire DOS si on demande l'activation ou la désactivation des logs
        /// Si null, alors on conserve le fait que les logs soient actif ou pas
        ///  => s'active de manière global au serveur
        /// </summary>
        public bool? LogDOS { get; private set; }

        public static ExternalPageQueryString GetNewByQueryString(eRequestTools requestTools)
        {
            return new ExternalPageQueryString()
            {
                Token = requestTools.GetRequestQSKeyS("tok"),
                Cs = requestTools.GetRequestQSKeyS("cs"),
                P = requestTools.GetRequestQSKeyS("p"),

                Log = requestTools.GetRequestQSKeyB(LOG_KEY) ?? false,
                LogDOS = requestTools.GetRequestQSKeyB(LOG_DOS_KEY)
            };
        }
        public static ExternalPageQueryString GetNewByForm(eRequestTools requestTools)
        {
            return new ExternalPageQueryString()
            {
                Token = requestTools.GetRequestFormKeyS("tok"),
                Cs = requestTools.GetRequestFormKeyS("cs"),
                P = requestTools.GetRequestFormKeyS("p"),

                Log = requestTools.GetRequestFormKeyB(LOG_KEY) ?? false,
                LogDOS = requestTools.GetRequestFormKeyB(LOG_DOS_KEY)
            };
        }

        public IEnumerable<KeyValuePair<string, string>> GetConserveInfo()
        {
            // Inutile de conserver LogDOS, c'est activé au global !
            return new Dictionary<string, string>()
            {
                { "tok", Token },
                { "cs", Cs },
                { "p", P },
                { LOG_KEY, Log ? "1" : "0" }
            };
        }
    }
}