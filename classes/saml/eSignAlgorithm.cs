using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Algos supportés pour signature
    /// </summary>
    public class eSignAlgorithm
    {
        /// <summary>
        /// Liste des algo supporté par Eudonet
        /// </summary>
        public static IEnumerable<string> Supported = new string[] { "SHA1", "SHA256", "SHA512" };

        /// <summary> N'est pas recommandé</summary>
        public static string SHA1 = "http://www.w3.org/2000/09/xmldsig#rsa-sha1";
      
        /// <summary> Recommandé par défaut</summary>
        public static string SHA256 = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";

        /// <summary> Plus de sécurité</summary>
        public static string SHA384 = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha384";

        /// <summary> Encore plus de sécurité</summary>      
        public static string SHA512 = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha512";

        /// <summary>
        /// A partir du nom retourne l'url de défintion complete 
        /// </summary>
        /// <param name="algoName"></param>
        /// <returns></returns>
        public static string From(string algoName)
        {
            switch (algoName)
            {
                case "SHA1": return SHA1;
                case "SHA256": return SHA256;
                case "SHA512": return SHA512;
                default:
                    throw new Exception($"Algo {algoName} non supporté !");
            }
        }
        /// <summary>
        /// A partir du nom retourne l'url de défintion complete 
        /// </summary>
        /// <param name="algoRef"></param>
        /// <returns></returns>
        public static string GetName(string algoRef)
        {
            if (algoRef.Equals(SHA1)) return "SHA1";
            else if (algoRef.Equals(SHA256)) return "SHA256";
            else if (algoRef.Equals(SHA512)) return "SHA512";
        
          throw new Exception($"Algo {algoRef} non supporté !");
           
        }
    }
}