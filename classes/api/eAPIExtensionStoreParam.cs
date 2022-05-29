using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// 
    /// </summary>
    public class ExtEnum
    {
        #region info sur les DESC

        /// <summary>
        /// Informations sur la table Produit
        /// </summary>
        public enum FIELDS_PRODUCT
        {
            /// <summary>Table Produits</summary>
            TAB = 4900,
            /// <summary>Produits.Title</summary>
            TITLE = 4901,
            /// <summary>Produits.Logo</summary>
            //LOGO = 4905,
            LOGO = 4923,
            /// <summary>Produits.Categories</summary>
            CATEGORIES = 4906,
            /// <summary>Produits.Auteur</summary>
            AUTHOR = 4907,
            /// <summary>Produits.URL Auteur</summary>
            AUTHOR_URL = 4908,
            /// <summary>Produits.Nb d'installations</summary>
            INSTALL_COUNT = 4909,
            /// <summary>Produits.ID Extension</summary>
            NATIVE_ID = 4918,
            /// <summary>Produits.Type</summary>
            TYPE = 4902,
            /// <summary>Produits.Publié sur le store</summary>
            AVAILABLE_ON_STORE = 4920,
            /// <summary>Produits.Publié en debug</summary>
            AVAILABLE_DEBUG = 4922,
            /// <summary>Produits.Offre</summary>
            OFFERS = 4919,

            /// <summary>
            /// Nouvelle extension
            /// </summary>
            ISNEW = 4924,
             
            /// <summary>
            /// 
            /// </summary>
            HASCUSTOMPARAM = 4930
        }

        /// <summary>
        /// Informations sur la table Produit Description
        /// </summary>
        public enum FIELDS_DESCRIPION
        {
            /// <summary>Table Produits Description</summary>
            TAB = 14400,
            /// <summary>Produits Description.Langue</summary>
            LANG = 14402,
            /// <summary>Produits Description.Résumé</summary>
            TEXTE_LOGO = 14406,
            /// <summary>Produits Description.Résumé</summary>
            SUMMARY = 14494,
            /// <summary>Produits Description.L essentiel</summary>
            DESCRIPTION = 14489,
            /// <summary>Produits Description.C est parti !</summary>
            INSTALLATION = 14403,
            /// <summary>Produits Description.créer le</summary>
            CREATE_AT = 14495
        }

        //SHA
        /// <summary>
        /// Informations sur le signet Doc. de la table Produit Description
        /// </summary>
        public enum FIELDS_DOC_PRODUCT_DESCRIPTION
        {
            ///<summary>Table Doc</summary>
            TAB = 2500,
            /// <summary>Produits Description.Titre</summary>
            TITLE = 2501,
            /// <summary>Produits Description.Produit Description</summary>
            PRODUCT_DESCRIPTION = 2511,
            /// <summary>Produits Description.Manuel/Tutoriel</summary>
            MANUAL_TUTO = 2527
        }

        /// <summary>
        /// Informations sur la table Prix extension
        /// </summary>
        public enum FIELDS_PRICE
        {
            /// <summary>Table Prix extension</summary>
            TAB = 31800,
            /// <summary>[Extension prix].[Offre]</summary>
            OFFER = 31802,
            /// <summary>[Extension prix].[Prix]</summary>
            PRICE = 31803,
            /// <summary>[Extension prix].[Unité]</summary>
            UNIT = 31804,
            /// <summary>[Extension prix].[Inclus dans l'offre]</summary>
            INCLUDED = 31805,
            /// <summary>[Extension prix].[créer le]</summary>
            CREATE_AT = 31895,
        }

        /// <summary>
        /// Informations sur la table Version produit new
        /// </summary>
        public enum FIELDS_VERSION
        {
            /// <summary>Table Versions produit New</summary>
            TAB = 14500,
            /// <summary>Versions produit New.Version</summary>
            VERSION = 14502,
            /// <summary>Versions produit New.Version Eudo Compatible</summary>
            VERSION_COMPATIBLE = 14503,
            /// <summary>Versions produit New.Environnement</summary>
            ENVIRONEMENT = 14504,
            /// <summary>Versions produit New.Développement Finalisé</summary>
            DEV_FINISH = 14505,
            /// <summary>Versions produit New.Date Homologation</summary>
            UPDATE = 14506,
            /// <summary>Versions produit New.Status</summary>
            STATUS = 14510,
            /// <summary>Versions produit New.N° Version</summary>
            NUM_VERSION = 14511,
            /// <summary>Versions produit New.Bandeau nouveau</summary>
            IS_NEW = 14512,
            /// <summary>Versions produit New.Description</summary>
            DESCRIPTION = 14513,
            /// <summary>Versions produit New.Notes</summary>
            NOTES = 14594,
        }

        /// <summary>
        /// Informations sur la table Produits ChangeLog
        /// </summary>
        public enum FIELDS_CHANGE_LOG
        {
            /// <summary>Table Produits ChangeLog</summary>
            TAB = 14600,
            /// <summary>Langue</summary>
            LANG = 14602,
            /// <summary>Nouveautés</summary>
            NEW = 14694,
            /// <summary>Créé le</summary>
            CREATE_AT = 14695,
        }

        /// <summary>
        /// Informations sur la table Produit visuels
        /// </summary>
        public enum FIELDS_VISU
        {
            /// <summary>Table Produits Visu</summary>
            TAB = 14700,
            /// <summary>Produits Visuels.Aperçu</summary>
            IMAGE = 14702,
            /// <summary>Produits Visuels.Légende</summary>
            LABEL = 14701,
            /// <summary>Produits Visuels.No Ordre</summary>
            ORDER = 14704,
            /// <summary>Produits Visuels.Langue</summary>
            LANG = 14703,
            /// <summary>Produits Visuels.Lien vers video </summary>
            VIDEOLINK = 14705
        }

        /// <summary>
        /// Informations sur la table Sociétés
        /// </summary>
        public enum FIELDS_PM
        {
            /// <summary>Table Sociétés</summary>
            TAB = 300,
            /// <summary>Sociétés.Societe</summary>
            TITLE = 301,
        }

        /// <summary>
        /// Informations sur la table Contrats
        /// </summary>
        public enum FIELDS_CONTRAT
        {
            /// <summary>TABLE Contrats</summary>
            TAB = 4200,
            /// <summary>Contrats.Contrat</summary>
            TITLE = 4201,
        }

        /// <summary>
        /// Informations sur la table Bases liées
        /// </summary>
        public enum FIELDS_BASE_LIEE
        {
            /// <summary>TABLE Bases liées</summary>
            TAB = 15700,
            /// <summary>Bases Liées.Intitulé</summary>
            TITLE = 15701,
            /// <summary>Bases Liées.Offre</summary>
            OFFER = 15702,
            /// <summary>Bases Liées.Nom SQL</summary>
            SQL_NAME = 15703,
            /// <summary>Bases Liées.Produit</summary>
            PRODUCT = 15704,
            /// <summary>Bases Liées.Payantes</summary>
            NB_PAID_SUBSCRIPTIONS = 15706,
            /// <summary>Bases Liées.Gratuites</summary>
            NB_FREE_SUBSCRIPTIONS = 15707,
            /// <summary>Bases Liées.Lights</summary>
            NB_LIGHT_SUBSCRIPTIONS = 15708,
            /// <summary>Bases Liées.Licence</summary>
            LICENSE_KEY = 15711,
        }

        /// <summary>
        /// Informations sur la table Extensions liées
        /// </summary>
        public enum FIELDS_LINKED_EXTENSION
        {
            /// <summary>TABLE Extensions liées</summary>
            TAB = 15800,
            /// <summary>Extensions Liées.Intitulé</summary>
            TITLE = 15801,
            /// <summary>Extensions Liées.Base</summary>
            BASE = 15802,
            /// <summary>Extensions Liées.Extension</summary>
            PRODUCT = 15803,
            /// <summary>Extensions Liées.Date</summary>
            DATE = 15806,
            /// <summary>Extensions Liées.Statut</summary>
            STATUT = 15811,
            /// <summary>Extensions Liées.Demandé par</summary>
            REQUEST_USER = 15807,
            /// <summary>Extensions Liées.Email Demandeur</summary>
            EMAIL = 15808,
            /// <summary>Extensions Liées.Tél Demandeur</summary>
            PHONE = 15809,
            /// <summary>Extensions Liées.Version</summary>
            VERSION = 15810,
        }

        #endregion

        #region info sur les filedata

        /// <summary>
        /// Valeurs du catalogue Produits.Offre
        /// </summary>
        /// TODO - Récupérer les valeurs de catalogues via API oui ou non ?
        public enum DATA_PRODUCT_OFFER
        {
            /// <summary>ACCESS</summary>
            ACCES = 1590,
            /// <summary>STANDARD</summary>
            STANDARD = 1591,
            /// <summary>PREMIER</summary>
            PREMIER = 1592,

            /// <summary>
            /// ID pour offre PRO
            /// </summary>
            PRO = 4258,

            /// <summary>
            /// ID offre essentiel
            /// </summary>
            ESSENTIEL = 4259


        }


 



        /// <summary>
        /// Renvoie un ExtEnum.DATA_PRODUCT_OFFER en fonction du eLibConst.ClientOffer passé en paramètre
        /// </summary>
        /// <param name="clientOffer">Enum Offre client</param>
        /// <returns>Enum Offre produit</returns>
        public static DATA_PRICE_OFFER MapPriceClientOffer(eLibConst.ClientOffer clientOffer)
        {
            switch (clientOffer)
            {
                case eLibConst.ClientOffer.ACCES:
                    return DATA_PRICE_OFFER.ACCES;
                case eLibConst.ClientOffer.STANDARD:
                    return DATA_PRICE_OFFER.STANDARD;
                case eLibConst.ClientOffer.PREMIER:
                    return DATA_PRICE_OFFER.PREMIER;
                case eLibConst.ClientOffer.PRO:
                    return DATA_PRICE_OFFER.PRO;
                case eLibConst.ClientOffer.ESSENTIEL:
                    return DATA_PRICE_OFFER.ESSENTIEL;
                case eLibConst.ClientOffer.XRM:
                default:
                    throw new System.Exception(string.Format("Correspondance impossible entre {0} et {1}", clientOffer.ToString(), typeof(DATA_PRICE_OFFER).ToString()));
            }
        }


        /// <summary>
        /// Renvoie un ExtEnum.DATA_PRODUCT_OFFER en fonction du eLibConst.ClientOffer passé en paramètre
        /// </summary>
        /// <param name="clientOffer">Enum Offre client</param>
        /// <returns>Enum Offre produit</returns>
        public static DATA_PRODUCT_OFFER MapProductClientOffer(eLibConst.ClientOffer clientOffer)
        {
            switch (clientOffer)
            {
                case eLibConst.ClientOffer.ACCES:
                    return DATA_PRODUCT_OFFER.ACCES;
                case eLibConst.ClientOffer.STANDARD:
                    return DATA_PRODUCT_OFFER.STANDARD;
                case eLibConst.ClientOffer.PREMIER:
                    return DATA_PRODUCT_OFFER.PREMIER;
                case eLibConst.ClientOffer.PRO:
                    return DATA_PRODUCT_OFFER.PRO;
                case eLibConst.ClientOffer.ESSENTIEL:
                    return DATA_PRODUCT_OFFER.ESSENTIEL;
                case eLibConst.ClientOffer.XRM:
                default:
                    throw new System.Exception(string.Format("Correspondance impossible entre {0} et {1}", clientOffer.ToString(), typeof(DATA_PRODUCT_OFFER).ToString()));
            }
        }

        /// <summary>
        /// Renvoie un eLibConst.ClientOffer en fonction du ExtEnum.DATA_PRODUCT_OFFER passé en paramètre
        /// </summary>
        /// <param name="productOffer">Enum Offre produit</param>
        /// <returns>Enum Offre client</returns>
        public static eLibConst.ClientOffer MapClientProductOffer( DATA_PRODUCT_OFFER productOffer)
        {
            switch (productOffer)
            {
                case DATA_PRODUCT_OFFER.ACCES:
                    return eLibConst.ClientOffer.ACCES;
                case DATA_PRODUCT_OFFER.STANDARD:
                    return eLibConst.ClientOffer.STANDARD;
                case DATA_PRODUCT_OFFER.PREMIER:
                    return eLibConst.ClientOffer.PREMIER;
                case DATA_PRODUCT_OFFER.PRO:
                    return eLibConst.ClientOffer.PRO;
                case DATA_PRODUCT_OFFER.ESSENTIEL:
                    return eLibConst.ClientOffer.ESSENTIEL;
                default:
                    return eLibConst.ClientOffer.XRM;
            }
        }

        /// <summary>
        /// Valeurs du catalogue Produits.Type
        /// </summary>
        /// TODO - Récupérer les valeurs de catalogues via API oui ou non ?
        public enum DATA_PRODUCT_TYPE
        {
            /// <summary>Extension</summary>
            EXTENSION = 1161,
        }

        /// <summary>
        /// Valeurs du catalogue [Prix extension].Offre
        /// </summary>
        /// TODO - Récupérer les valeurs de catalogues via API oui ou non ?
        public enum DATA_PRICE_OFFER
        {
            /// <summary>XRM</summary>
            XRM = 1700,
            /// <summary>ACCESS</summary>
            ACCES = 1701,
            /// <summary>STANDARD</summary>
            STANDARD = 1702,

            /// <summary>PREMIER</summary>
            PREMIER = 1703,

            /// <summary>PREMIER</summary>
            PRO = 1704,


            /// <summary>PREMIER</summary>
            ESSENTIEL = 1705,
        }

        /// <summary>
        /// Valeurs du catalogue Produit descriptions.Langue
        /// </summary>
        /// TODO - Récupérer les valeurs de catalogues via API oui ou non ?
        public enum DATA_DESCRIPION_LANG
        {
            /// <summary>Francais</summary>
            LANG_00 = 1489,
            /// <summary>Anglais</summary>
            LANG_01 = 1490,
            /// <summary>Allemand</summary>
            LANG_02 = 1491,
            /// <summary>Néerlandais</summary>
            LANG_03 = 1492,
            /// <summary>Espagnol</summary>
            LANG_04 = 1493,
            /// <summary>Italien</summary>
            LANG_05 = 1494,
        }

        /// <summary>
        /// Valeurs du catalogue Produits Visuels.Langue
        /// </summary>
        /// TODO - Récupérer les valeurs de catalogues via API oui ou non ?
        public enum DATA_VISU_LANG
        {
            /// <summary>Francais</summary>
            LANG_00 = 1489,
            /// <summary>Anglais</summary>
            LANG_01 = 1490,
            /// <summary>Allemand</summary>
            LANG_02 = 1491,
            /// <summary>Néerlandais</summary>
            LANG_03 = 1492,
            /// <summary>Espagnol</summary>
            LANG_04 = 1493,
            /// <summary>Italien</summary>
            LANG_05 = 1494,
        }

        /// <summary>
        /// Valeurs du catalogue Produits ChangeLog.Langue
        /// </summary>
        /// TODO - Récupérer les valeurs de catalogues via API oui ou non ?
        public enum DATA_CHANGE_LOG_LANG
        {
            /// <summary>Francais</summary>
            LANG_00 = 1489,
            /// <summary>Anglais</summary>
            LANG_01 = 1490,
            /// <summary>Allemand</summary>
            LANG_02 = 1491,
            /// <summary>Néerlandais</summary>
            LANG_03 = 1492,
            /// <summary>Espagnol</summary>
            LANG_04 = 1493,
            /// <summary>Italien</summary>
            LANG_05 = 1494,
        }

        #endregion
    }

    /// <summary>
    /// Classe de gestion des paramètres pour les appels à l'API
    /// </summary>
    public class eAPIExtensionStoreParam
    {
        /// <summary>
        /// Liste des codes du catalogue avancé ExtensionsLiées.Status dabs HOTCOM
        /// </summary>
        public static class LinkedExtensionStatutsCodes
        {
            /// <summary>
            /// Demande d'activation
            /// </summary>
            public const string EnableRequest = "A";

            /// <summary>
            /// Demande d'activation transmise à l'IC
            /// </summary>
            public const string EnableRequestTransmittedToIC = "B";

            /// <summary>
            /// Annulation d'activation
            /// </summary>
            public const string EnableRequestCancellation = "C";

            /// <summary>
            /// Proposition commerciale envoyée
            /// </summary>
            public const string CommercialProposalSent = "D";

            /// <summary>
            /// Activée
            /// </summary>
            public const string Enabled = "E";

            /// <summary>
            /// Demande de désactivation
            /// </summary>
            public const string DisableRequest = "F";

            /// <summary>
            /// Désactivée
            /// </summary>
            public const string Disabled = "G";

        }

        #region Propriétés

        private string apiBaseUrl = eLibConst.API_URL;
        private string apiSubscriberLogin = "";
        private string apiSubscriberPassword = "";
        private string apiBaseName = "";
        private string apiUserLogin = "";
        private string apiUserPassword = "";
        private string apiUserLang = "";
        private string apiProductName = ePrefConst.XRM_SQL_APPLICATIONNAME;

        #endregion

        #region Accesseurs

        /// <summary>
        /// Pref user
        /// </summary>
        public ePref Pref { get; private set; }

        /// <summary>
        /// Active ou non le mode debug sur les appels à l'API
        /// </summary>
        public bool ApiDebug { get; private set; }
        /// <summary>
        /// Indique si l'on doit envoyer les demandes d'activation/désactivation des extensions sur la base centrale de l'EudoStore (HotCom)
        /// Permet d'éviter l'envoi de ces demandes lors de tests sur des bases en local
        /// </summary>
        public bool ApiSendExtensionStoreRequests { get; private set; }

        /// <summary>
        /// URL de l'API à appeler
        /// </summary>
        public string ApiBaseUrl
        {
            get
            {
                return apiBaseUrl;
            }

            set
            {
                apiBaseUrl = value;
            }
        }

        /// <summary>
        /// Login Abonné
        /// </summary>
        public string ApiSubscriberLogin
        {
            get
            {
                return apiSubscriberLogin;
            }

            set
            {
                apiSubscriberLogin = value;
            }
        }

        /// <summary>
        /// Mot de passe abonné
        /// </summary>
        public string ApiSubscriberPassword
        {
            get
            {
                return apiSubscriberPassword;
            }

            set
            {
                apiSubscriberPassword = value;
            }
        }

        /// <summary>
        /// Nom SQL de la base de données
        /// </summary>
        public string ApiBaseName
        {
            get
            {
                return apiBaseName;
            }

            set
            {
                apiBaseName = value;
            }
        }

        /// <summary>
        /// Login utilisateur
        /// </summary>
        public string ApiUserLogin
        {
            get
            {
                return apiUserLogin;
            }

            set
            {
                apiUserLogin = value;
            }
        }

        /// <summary>
        /// Mot de passe utilisateur
        /// </summary>
        public string ApiUserPassword
        {
            get
            {
                return apiUserPassword;
            }

            set
            {
                apiUserPassword = value;
            }
        }

        /// <summary>
        /// Langue de connexion de l'utilisateur
        /// </summary>
        public string ApiUserLang
        {
            get
            {
                return apiUserLang;
            }

            set
            {
                apiUserLang = value;
            }
        }

        /// <summary>
        /// Nom du produit appelant
        /// </summary>
        public string ApiProductName
        {
            get
            {
                return apiProductName;
            }

            set
            {
                apiProductName = value;
            }
        }

        #endregion

        #region Constructeur

        /// <summary>
        /// Intancie un objet de paramètre pour les appels à l'API
        /// </summary>
        /// <param name="pref">Pref</param>
        public eAPIExtensionStoreParam(ePref pref)
        {
            Pref = pref;

            ApiDebug = false;
            ApiSendExtensionStoreRequests = true;

#if DEBUG
            // Décommenter la ligne code ci-dessous pour se connecter sur une API en local et ne pas récupérer la liste des extensions depuis HotCom.
            // Par défaut, on considèrera qu'on souhaite accéder aux infos du Store, même en local.
            // Seules les demandes d'activation/désactivation d'extensions ne sont pas transmises en local ; elles sont contrôlées par le paramètre séparé ApiSendExtensionStoreRequests
            ApiDebug = false;
            ApiSendExtensionStoreRequests = false;
#endif

            ApiBaseUrl = eLibConst.API_URL;
            ApiSubscriberLogin = "EUDOCOMM";
            ApiSubscriberPassword = "KOMODO80";
            ApiBaseName = "EUDO_HOTCOM_EUDOWEB";
            ApiUserLogin = "EDN_API";
            ApiUserPassword = "EUDOAPI";

            if (ApiDebug)
            {
                ApiBaseUrl = "http://localhost/eudoapi";
                ApiSubscriberLogin = "EUDONET";
                ApiSubscriberPassword = "EUDONET";
                ApiBaseName = "EUDO_HOTCOM_EUDOWEB";
                ApiUserLogin = "EDN_API";
                ApiUserPassword = "";
            }

            ApiUserLang = Pref.User.UserLang;
            ApiProductName = ePrefConst.XRM_SQL_APPLICATIONNAME;
        }

        #endregion
    }
}