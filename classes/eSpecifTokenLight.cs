using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Common.Cryptography;

namespace Com.Eudonet.Xrm
{
    public class eSpecifTokenLight
    {
        #region propriétés
        private String _sError = String.Empty;        //Erreur
        private ePref _ePref;               //préférence utilisateur
        private HttpContext _Context;       //Context Http de la demande
        private Exception _eInnerException = null;          // Exception rencontrée

        private String _sToken = String.Empty;              // Token 

        private Boolean _bIsInit = false;               //flag indiquant l'initialisation correcte
        private Boolean _bIsGenerated = false;          // flag indiquant la génération correcte
        #endregion

        #region accesseurs

        /// <summary>
        /// Indique si une erreur est survenue
        /// </summary>
        public Boolean IsError
        {
            get
            {
                return _sError.Length > 0;
            }
        }

        /// <summary>
        /// Token d'information permettant les appels aux spécifs asp/xrm
        /// </summary>
        public String Token
        {
            get { return _sToken; }
        }

        /// <summary>
        /// Retourne l'exception ayant arrêté le traitement
        /// </summary>
        public Exception InnerException
        {
            get
            {
                return _eInnerException;
            }
        }
        #endregion

        #region Génération instance eSpecifToken

        /// <summary>
        /// Constructeur privé pour la classe de  génération de token
        /// </summary>
        /// <param name="pref">Préférence utilisateur</param>
        /// <param name="context">Context http de la demande - nécessiaire pour récupérer des variables du client</param>
        private eSpecifTokenLight(ePref pref, HttpContext context = null)
        {

            //Si htttpcontextcurrent inaccessible et pas passé en param, erreur
            if (HttpContext.Current == null && context == null)
            {
                _sError = "Contexte d'appel invalide";
            }

            if (HttpContext.Current == null)
                _Context = context;
            else
                _Context = HttpContext.Current;

            _ePref = pref;
        }

        /// <summary>
        /// génère un token permettant aux specifs XRM d'atteindre la base
        /// </summary>
        /// <param name="pref">Préférence utilisateur</param>
        /// <param name="spec">Objet eSpec de la spécif</param>
        /// <param name="nTab">Table de la spécif</param>
        /// <param name="nFileId">Id de la fiche de la spécif</param>
        /// <param name="nParentTab">DescId de la table parente (mode signet)</param>
        /// <param name="nParentFileId">Id de la fiche parente (mode signet)</param>
        /// <param name="nDescId">Descid du champ déclencheur (mode champ déclencheur xrm=)</param>
        /// <returns></returns>
        public static eSpecifTokenLight GetSpecifTokenLight(ePref pref)
        {
            String sToken = String.Empty;

            eSpecifTokenLight estToken = new eSpecifTokenLight(pref);
            if (estToken.IsError)
                return estToken;

            //Initialisation et géneration
            Boolean bIsOk = estToken.InitTokenLight()
                    && estToken.GenerateToken();

            return estToken;
        }

        #endregion

        #region méthodes internes

        /// <summary>
        /// initie les paramètre du token
        /// </summary>
        /// <returns></returns>
        private Boolean InitTokenLight()
        {
            _bIsInit = true;
            return true;
        }

        /// <summary>
        /// Génère la chaine de token
        /// </summary>
        /// <returns></returns>
        private Boolean GenerateToken()
        {
            if (!_bIsInit)
            {
                _sError = "Le token n'a pas été initialisé";
                return false;
            }

            try
            {
                //Liste des 
                Dictionary<String, String> dicoParams = new Dictionary<String, String>();

                Random random = new Random();
                int randomNumber = random.Next(0, 100);
                dicoParams.Add(String.Concat("xx", randomNumber), String.Concat(DateTime.Now.Ticks.ToString(), randomNumber.ToString()));

                DateTime ts = new DateTime(1970, 1, 1, 0, 0, 0);

                dicoParams.Add("XTS", (DateTime.UtcNow - ts).TotalSeconds.ToString());

                // Construction des variables de sessions lié à "LogDatabaseTreatment"
                dicoParams.Add("Base", _ePref.GetBaseName);
                dicoParams.Add("BaseName", _ePref.EudoBaseName);
                dicoParams.Add("BaseUid", _ePref.DatabaseUid);                
                dicoParams.Add("ReadOnly", _ePref.ReadOnly ? "1" : "0");
                dicoParams.Add("ClientId", _ePref.ClientId.ToString());
                dicoParams.Add("SubscriberId", _ePref.LoginId.ToString());
                dicoParams.Add("Intranet", ConfigurationManager.AppSettings.Get("IntranetApplication") == "1" ? "1" : "0");

                dicoParams.Add("UserLogin", _ePref.User.UserLogin);
                dicoParams.Add("UserName", _ePref.User.UserName);
                dicoParams.Add("UserDisplayName", _ePref.User.UserDisplayName);                
                dicoParams.Add("UserLevel", _ePref.User.UserLevel.ToString());                
                dicoParams.Add("UserMail", _ePref.User.UserMail);
                dicoParams.Add("UserMailOther", _ePref.User.UserMailOther);                
                dicoParams.Add("UserLang", _ePref.User.UserLang);
                dicoParams.Add("UserLangId", _ePref.User.UserLangId.ToString());

                randomNumber = random.Next(0, 500);
                dicoParams.Add(String.Concat("xx", randomNumber), String.Concat(randomNumber, _Context.Timestamp.Ticks.ToString()));

                //Theme XRM
                dicoParams.Add("ThemeXrmColor", _ePref.ThemeXRM.Color);
                dicoParams.Add("ThemeXrmName", _ePref.ThemeXRM.Name);
                dicoParams.Add("ThemeXrmFolder", _ePref.ThemeXRM.Folder);

                randomNumber = random.Next(0, 500);
                dicoParams.Add(String.Concat("xx", randomNumber), String.Concat(randomNumber, _Context.Timestamp.Ticks.ToString()));

                //Configuration d'écran
                dicoParams.Add("ScreenWidth", _ePref.Context.ScreenWidth.ToString());
                dicoParams.Add("ScreenHeight", _ePref.Context.ScreenHeight.ToString());

                //Création du token
                String sToken = String.Empty;

                foreach (KeyValuePair<String, String> kv in dicoParams)
                {
                    String sKey = kv.Key;
                    String sValue = kv.Value;
                    String sKV = String.Concat(sKey, "#=#", kv.Value);

                    if (sToken.Length > 0)
                        sToken = String.Concat(sToken, "#&#");

                    sToken = String.Concat(sToken, sKV);
                }

                _sToken = CryptoTripleDES.Encrypt(sToken, CryptographyConst.KEY_CRYPT_LINK4);
                _bIsGenerated = true;

                return true;
            }
            catch (Exception e)
            {
                _eInnerException = e;
                return false;
            }


        }

        #endregion
    }
}