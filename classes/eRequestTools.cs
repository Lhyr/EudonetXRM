using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EudoExtendedClasses;
using EudoQuery;
using Com.Eudonet.Internal;

namespace Com.Eudonet.Xrm
{
    /// <className>eRequestTools</className>
    /// <summary>Classe outils pour la manipulation des Request.Form et Request.QueryString</summary>
    /// <purpose></purpose>
    /// <authors>HLA</authors>
    /// <date>2015-06-15</date>
    public class eRequestTools
    {
        /// <summary>HashSet de toutes les clés de Request.Form</summary>
        public HashSet<string> AllKeys { get; private set; }
        /// <summary>HashSet de toutes les clés de Request.QueryString</summary>
        public HashSet<string> AllKeysQS { get; private set; }

        /// <summary>pour la gestion d'erreur : indique s'il faut envoyer l'erreur précise pour le debogage</summary>
        public bool IsLocalOrEudo { get; private set; }
        /// <summary>Context de la page</summary>
        private HttpContext _context = null;

        /// <summary>
        /// Construteur
        /// </summary>
        /// <param name="context">http context de la page</param>
        public eRequestTools(HttpContext context)
        {
            this._context = context;

            // Charge les valeurs de request
            AllKeys = new HashSet<string>(_context.Request.Form.AllKeys, StringComparer.OrdinalIgnoreCase);
            AllKeysQS = new HashSet<string>(_context.Request.QueryString.AllKeys, StringComparer.OrdinalIgnoreCase);
            IsLocalOrEudo = eLibTools.IsLocalOrEudoMachine(_context);
        }

        /// <summary>
        /// Récupère les informations de liaisons parente sur le post de la page
        /// </summary>
        /// <returns>id des fiches parent via l'objet eFileTools.eParentFileId</returns>
        public eFileTools.eParentFileId GetRequestFormDidParent()
        {
            int? lnkPpId = null, lnkPmId = null, lnkAdrId = null, lnkEvtId = null;
            int lnkEvtDescId = 0;

            int key = 0, val = 0;
            foreach (string reqKey in AllKeys)
            {
                if (!(int.TryParse(reqKey, out key) && key > 0 && key % 100 == 0))
                    continue;

                val = GetRequestFormKeyI(reqKey) ?? 0;

                if (val <= 0)
                    continue;

                if (key == TableType.PP.GetHashCode())
                    lnkPpId = val;
                else if (key == TableType.PM.GetHashCode())
                    lnkPmId = val;
                else if (key == TableType.ADR.GetHashCode())
                    lnkAdrId = val;
                else if (lnkEvtId == null)        // On reprend uniquement le premier descid venu
                {
                    lnkEvtDescId = key;
                    lnkEvtId = val;
                }
            }

            return new eFileTools.eParentFileId(lnkPpId, lnkPmId, lnkEvtId, lnkAdrId, lnkEvtDescId);
        }

        /// <summary>
        /// Récupère les informations de liaisons parente sur le post de la page via la clé lnkid
        /// </summary>
        /// <returns>id des fiches parent via l'objet eFileTools.eParentFileId</returns>
        public eFileTools.eParentFileId GetRequestFormLnkIds()
        {
            string lnkids = string.Empty;
            eFileTools.eParentFileId efPrtId = null;

            if (GetRequestFormKey("lnkid", out lnkids) && lnkids != null)
            {
                int lnkPpId = 0, lnkPmId = 0, lnkAdrId = 0, lnkEvtId = 0;
                int lnkEvtDescId = 0;

                var lnks = lnkids.ConvertToListKeyValInt(";", "=");
                foreach (KeyValuePair<int, int> keyValue in lnks)
                {
                    if (keyValue.Key == TableType.PP.GetHashCode())
                        lnkPpId = keyValue.Value;
                    else if (keyValue.Key == TableType.PM.GetHashCode())
                        lnkPmId = keyValue.Value;
                    else if (keyValue.Key == TableType.ADR.GetHashCode())
                        lnkAdrId = keyValue.Value;
                    else if (lnkEvtId == 0)        // On reprend uniquement le premier descid venu
                    {
                        lnkEvtDescId = keyValue.Key;
                        lnkEvtId = keyValue.Value;
                    }
                }

                efPrtId = new eFileTools.eParentFileId(lnkPpId, lnkPmId, lnkEvtId, lnkAdrId, lnkEvtDescId);
            }
            else
            {
                efPrtId = new eFileTools.eParentFileId();
            }

            return efPrtId;
        }

        #region GetRequestFormKey

        /// <summary>
        /// Retourne la valeur venant de la request form
        /// </summary>
        /// <param name="key">id de la valeur à recup du form</param>
        /// <param name="value">valeur recupéré par la fonction</param>
        /// <returns>vrai si la valeur est trouvé</returns>
        public bool GetRequestFormKey(string key, out string value)
        {
            value = string.Empty;

            if (!AllKeys.Contains(key) || string.IsNullOrEmpty(_context.Request.Form[key]))
                return false;

            value = _context.Request.Form[key];
            return true;
        }

        /// <summary>
        /// Retourne la valeur venant de la request form
        /// </summary>
        /// <param name="key">id de la valeur à recup du form</param>
        /// <param name="value">valeur recupéré par la fonction</param>
        /// <returns>vrai si la valeur est trouvé et est valid</returns>
        public bool GetRequestFormKey(string key, out int value)
        {
            value = 0;

            if (!AllKeys.Contains(key) || string.IsNullOrEmpty(_context.Request.Form[key]))
                return false;

            return int.TryParse(_context.Request.Form[key], out value);
        }

        /// <summary>
        /// Retourne la valeur venant de la request form
        /// </summary>
        /// <param name="key">id de la valeur à recup du form</param>
        /// <param name="value">valeur recupéré par la fonction</param>
        /// <returns>vrai si la valeur est trouvé et est valid</returns>
        public bool GetRequestFormKey(string key, out bool value)
        {
            value = false;

            if (!AllKeys.Contains(key) || string.IsNullOrEmpty(_context.Request.Form[key]))
                return false;

            value = _context.Request.Form[key] == "1";

            return true;
        }

        /// <summary>
        /// Retourne la valeur venant de la request form
        /// </summary>
        /// <param name="key">id de la valeur à recup du form</param>
        /// <returns>valeur recupéré par la fonction</returns>
        public string GetRequestFormKeyS(string key)
        {
            string val;
            if (!GetRequestFormKey(key, out val))
                return null;
            return val;
        }

        /// <summary>
        /// Retourne la valeur venant de la request form
        /// </summary>
        /// <param name="key">id de la valeur à recup du form</param>
        /// <returns>valeur recupéré par la fonction</returns>
        public int? GetRequestFormKeyI(string key)
        {
            int val;
            if (!GetRequestFormKey(key, out val))
                return null;
            return val;
        }

        /// <summary>
        /// Retourne l'enum correspondant à au param fourni
        /// Retourne la valeur par défaut ou envoie une execption (voir parap return default) si
        /// la clée est absente/non valide
        /// usage :
        ///  eAdminUpdateProperty.CATEGORY cat =_requestTools.GetRequestFormEnum&lt;eAdminUpdateProperty.CATEGORY&gt;("cat", false) 
        /// </summary>
        /// <typeparam name="T">Type de l'enum</typeparam>
        /// <param name="sKey">clé dans request.form</param>
        /// <param name="returnDefault">Indique de retourner la valeur par def de l'enum (la 1er valeur) si clé invalide</param>
        /// <returns></returns>
        public T GetRequestFormEnum<T>(string sKey, bool returnDefault = true) where T : struct
        {
            Type enumType = typeof(T);
            int? nKey = GetRequestFormKeyI(sKey);
            if (nKey == null)
            {
                if (returnDefault)
                    return default(T);

                throw new InvalidCastException(string.Concat("Paramètre ", sKey, " vide."));
            }

            return eLibTools.GetEnumFromCode<T>(nKey ?? 0, returnDefault);

        }




        /// <summary>
        /// Retourne la valeur venant de la request form
        /// </summary>
        /// <param name="key">id de la valeur à recup du form</param>
        /// <returns>valeur recupéré par la fonction</returns>
        public bool? GetRequestFormKeyB(string key)
        {
            bool val;
            if (!GetRequestFormKey(key, out val))
                return null;
            return val;
        }

        /// <summary>
        /// Récupere une liste d'entier a partir de la valeur de la clé, contenant des entiers séparés par un 'sep'
        /// </summary>
        /// <param name="separator">Séparateur</param>
        /// <param name="key">clé recherchée</param>
        /// <returns></returns>
        public List<int> GetRequestIntListFormKeyS(string separator, string key)
        {
            string param = GetRequestFormKeyS(key);
            if (string.IsNullOrEmpty(param))
                return new List<int>();

            return param.ConvertToListInt(separator);
        }

        #endregion

        #region GetRequestQSKey

        /// <summary>
        /// Retourne la valeur venant de la request QueryString
        /// </summary>
        /// <param name="key">id de la valeur à recup du QueryString</param>
        /// <param name="value">valeur recupéré par la fonction</param>
        /// <returns>vrai si la valeur est trouvé</returns>
        public bool GetRequestQSKey(string key, out string value)
        {
            value = string.Empty;

            if (!AllKeysQS.Contains(key) || string.IsNullOrEmpty(_context.Request.QueryString[key]))
                return false;

            value = _context.Request.QueryString[key];
            return true;
        }

        /// <summary>
        /// Retourne la valeur venant de la request QueryString
        /// </summary>
        /// <param name="key">id de la valeur à recup du QueryString</param>
        /// <param name="value">valeur recupéré par la fonction</param>
        /// <returns>vrai si la valeur est trouvé et est valid</returns>
        public bool GetRequestQSKey(string key, out int value)
        {
            value = 0;

            if (!AllKeysQS.Contains(key) || string.IsNullOrEmpty(_context.Request.QueryString[key]))
                return false;

            return int.TryParse(_context.Request.QueryString[key], out value);
        }

        /// <summary>
        /// Retourne la valeur venant de la request QueryString
        /// </summary>
        /// <param name="key">id de la valeur à recup du QueryString</param>
        /// <param name="value">valeur recupéré par la fonction</param>
        /// <returns>vrai si la valeur est trouvé et est valid</returns>
        public bool GetRequestQSKey(string key, out bool value)
        {
            value = false;

            if (!AllKeysQS.Contains(key) || string.IsNullOrEmpty(_context.Request.QueryString[key]))
                return false;

            value = _context.Request.QueryString[key] == "1";

            return true;
        }

        /// <summary>
        /// Retourne la valeur venant de la request QueryString
        /// </summary>
        /// <param name="key">id de la valeur à recup du QueryString</param>
        /// <returns>valeur recupéré par la fonction</returns>
        public string GetRequestQSKeyS(string key)
        {
            string val;
            if (!GetRequestQSKey(key, out val))
                return null;
            return val;
        }

        /// <summary>
        /// Retourne la valeur venant de la request QueryString
        /// </summary>
        /// <param name="key">id de la valeur à recup du QueryString</param>
        /// <returns>valeur recupéré par la fonction</returns>
        public int? GetRequestQSKeyI(string key)
        {
            int val;
            if (!GetRequestQSKey(key, out val))
                return null;
            return val;
        }


        /// <summary>
        /// Retourne la valeur venant de la request form
        /// </summary>
        /// <param name="key">id de la valeur à recup du form</param>
        /// <returns>valeur recupéré par la fonction</returns>
        public bool? GetRequestQSKeyB(string key)
        {
            bool val;
            if (!GetRequestQSKey(key, out val))
                return null;
            return val;
        }



        #endregion

        #region Session tools

        /// <summary>
        /// Retourne la valeur dans la session
        /// </summary>
        /// <param name="key">id de la valeur à recup dans la session</param>
        /// <param name="value">valeur recupéré par la fonction</param>
        /// <returns>vrai si la valeur est trouvé</returns>
        internal bool GetSessionKey(string key, out string value)
        {
            value = string.Empty;

            if (ReferenceEquals(_context.Session[key], null))
                return false;

            value = _context.Session[key].ToString();
            return true;
        }

        /// <summary>
        /// Retourne la valeur dans la Session
        /// </summary>
        /// <param name="key">id de la valeur à recup dans la Session</param>
        /// <param name="defaultValue">valeur par defaut la clé n'est pas trouvé</param>
        /// <returns>valeur recupéré par la fonction</returns>
        internal string GetSessionKeyS(string key, string defaultValue = "")
        {
            string val;
            if (!GetSessionKey(key, out val))
                return defaultValue;
            return val;
        }

        /// <summary>
        /// Mis à jour la valeur dans la Session
        /// </summary>
        /// <param name="key">id de la valeur dans la Session</param>
        /// <param name="value">nouvelle valeur</param>
        /// <returns>valeur recupéré par la fonction</returns>
        internal void SetSessionKeyS(string key, string value)
        {
            _context.Session[key] = value;
        }


        #endregion

        /// <summary>
        /// Retourne l'encodage envoyé par le navigateur
        /// </summary>
        /// <returns></returns>
        public System.Text.Encoding GetContentEncoding()
        {
            return _context.Request.ContentEncoding;
        }
    }
}