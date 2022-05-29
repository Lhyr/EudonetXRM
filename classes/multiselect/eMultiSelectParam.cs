using EudoExtendedClasses;
using System.Collections.Generic;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Dico des paramètres transmis depuis le js
    /// </summary>
    public class eMultiSelectParam : Dictionary<string, string>
    {
        /// <summary>
        /// Objet représentant la liste des paramètres disponibles
        /// </summary>
        /// <param name="param">les paramètres en brut</param>
        /// <param name="groupSeparator">saparateur des ensembles clé:valeur</param>
        /// <param name="valueSeparator">séparaeutr de clé:valeur</param>
        public eMultiSelectParam(string param, string groupSeparator = ";", string valueSeparator = ":")
        {
            if (param == null)
                throw new eMultiSelectException("Paramètre non définit");

            var keyValues = param.Split(groupSeparator);
            for (int i = 0; i < keyValues.Length; i++)
            {
                if (keyValues[i] == string.Empty)
                    continue;

                var kv = keyValues[i].Split(valueSeparator);
                if (kv.Length == 2 && kv[0] != string.Empty)
                    Add(kv[0], kv[1]);
            }
        }

        /// <summary>
        /// Permet de convertir les valeur entière
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue">valeur par defaut</param>
        /// <returns></returns>
        public int GetIntValue(string key, int defaultValue = 0)
        {
            int i;
            if (ContainsKey(key) && int.TryParse(this[key], out i))
                return i;

            return defaultValue;
        }

        /// <summary>
        /// Recupère la valeur dans le dico sinon valeur par defaut, ne lève pas l'exception
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue">valeur par defaut</param>
        /// <returns></returns>
        public string GetStrValue(string key, string defaultValue = "")
        {
            if (ContainsKey(key))
                return this[key];

            return defaultValue;
        }
    }
}