using Com.Eudonet.Common.Cryptography;
using Com.Eudonet.Internal;

namespace Com.Eudonet.Xrm.classes
{
    /// <summary>
    /// Classe statique pour hashé une chaine de caractère selon l'algorithme Argon2
    /// </summary>
    public static class eArgon2Hash
    {
        /// <summary>
        /// Hash la chaine passé en paramètre selon l'algorithme Argon2
        /// </summary>
        /// <param name="stringToHash">Chaine à hasher</param>
        /// <returns>Cahine de caractère hashé</returns>
        public static string GetArgon2Hash(string stringToHash)
        {

            return HashArgon2.GetHash(stringToHash);

        }
    }
}