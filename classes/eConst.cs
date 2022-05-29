#pragma warning disable
using System;
namespace Com.Eudonet.Xrm
{
    /// <summary>Classe qui contient toutes les constantes EudonetXRM</summary>
    /// <classname>eConst</classname>    
    /// <authors>SPH</authors>
    /// <date>2011-08-31</date>
    public static class eConst
    {
        /// <summary>Nom de version</summary>
        public const String VERSIONNAME = "Version Dev.";

        /// <summary>Numéro de version</summary>
        public const String VERSION = "10.999.000";

        /// <summary>Numéro de révision</summary>
        public const String REVISION = "2021/05/17-11:32:39";


        /// <summary>
        /// numéro de news letter
        /// </summary>
        public const int NEWSLETTER = 3;

        /// <summary>
        /// numéro de news letter pourutilisateur
        /// </summary>
        public const int NEWSLETTER_USR = 2;





        /// <summary> Url de la newsletter qui ne depend pas du numéro </summary>
        public static string NEWSLETTER_URL = "https://fr.eudonet.com/news-admin/News-Admin.html";

        /// <summary> Url de la newsletter qui ne depend pas du numéro </summary>
        public static string NEWSLETTER_USR_URL = "https://fr.eudonet.com/news-admin/News-Admin.html";



        /// <summary> Url du support pour téléchargement TeamViewer</summary>
        public static string HELP_DESK_URL = "https://fr.eudonet.com/support/default.htm";

        /// <summary>GCH - 2014/09/29 - #33619 : tant que le dev n'est pas terminé on ajoute une variable pour activer/désactiver les formulaires</summary>
        public const Boolean FORMULARENABLED = true;

        /// <summary>
        /// Fichier de licence pour les licences tierces.
        /// </summary>
        public const string LICENSE_FILE = "license.txt";


        /// <summary></summary>
        public const int MAX_LANG = 10;

        /// <summary>Nombre maximale de tentative de connexions</summary>
        public const int NB_MAX_CNX = 4;

        ///Port https par défaut
        public const int DEFAULT_HTTPS_PORT = 443;

        /// <summary>Nombre de lignes maximales du mode liste des filtres express</summary>
        public const int MAX_ROWS_EXPRESS = 100;

        /// <summary>Nombre de cellules dédiées à chaque champ</summary>
        public const int NB_COL_BY_FIELD = 3;
        /// <summary>hauteur des cellules dans le Mode fiche</summary>
        public const int FILE_LINE_HEIGHT = 24;
        /// <summary>hauteur des cellules Header dans le Mode fiche</summary>
        public const int HEADER_LINE_HEIGHT = 16;

        /// <summary>
        /// Taille minimale pour chaque colonnes
        /// Remarque : Modifier egalement la fonction javascript "ResizeDoubleClick" dans eList.cs
        /// </summary>
        public const int MIN_COL_WIDTH = 60;


        /// <summary>Largueur de la fenetre par defaut</summary>
        public const int DEFAULT_WINDOW_WIDTH = 1024;
        /// <summary>Hauteur de la fenetre par defaut</summary>
        public const int DEFAULT_WINDOW_HEIGHT = 740;

        /// <summary>
        /// Taille du Signet "DETAILS" - Basé sur la css eFile.css.bkmDtlsSel 
        /// </summary>
        /// 
        public const int BKM_DTLS_WIDTH = 106;
        public const int BKM_PLUS_WIDTH = 30;

        public const int BKM_VIEWMODE_FILE_THRESHOLD = 100;

        /// <summary>
        /// Largeur du menu droite - basé sur la classe css rightMenuWidth - créée dynamiquement dans eMain.css
        /// A mettre à jour en même temps que les variables correspondantes dans eMain.adjustDivContainer() et eTools.createCalendarPopup(), ainsi que eMain.css .rightMenu
        /// </summary>
        public const int RIGHT_MENU_WIDTH = 170;
        public const int RIGHT_MENUCAL_WIDTH = 190;

        /// <summary>Couleur de fond des utilisateurs spéciaux (Tous, Vide, En Cours...)</summary>
        public const string COL_CAL_WHITE = "#DDDDDD";

        /// <summary>Couleur d'une plage par défaut du planning</summary>
        public const string COL_USR_SPEC = "#dcdcdc";

        /// <summary>Image transparente pour les images par </summary>
        public const string GHOST_IMG = "ghost.gif";
        /// <summary>Couleur de l'utilisateur désactivé</summary>
        public const string COL_USR_DISABLED = "#a9a9a9";

        ///<summary>Fait : 1;Type : 83;date : 2 ;fin  : 89;A faire par : 99;Particiants : 92;Confidentiel : 84;Couleur : 80</summary>
        [Obsolete]
        public const string PLANNING_HEADER_FIELDS = "1;83;2;89;99;92;84;80";

        /// <summary>
        /// Liste (DescIDs) des champs système des tables E-mail
        /// 9 : format HTML/texte brut ; 3 : De ; 4 : A ; 5 : Cc ; 6 : Cci ; 7 : Sujet ; 8 : Corps ; 1 : Historisé
        /// </summary>
        public const string MAIL_HEADER_FIELDS = "9;3;4;5;6;7;8;1";


        /// <summary>
        /// Liste (DescIDs) des champs système des tables SMS
        /// 3:De ; 4:A ; 7:Sujet ; 8:Corps ; 1:Historisé ; 29:status sms ; 2:date envoi; 26:displayname
        /// </summary>
        public const string SMS_HEADER_FIELDS = "1;2;3;4;7;8;26;29";

        /// <summary>
        /// Liste (DescIDs) des champs système des tables Campaign
        /// </summary>
        public const string CAMPAIGN_HEADER_FIELDS = "2;3;5;4;6;7;11;12;14";

        /// <summary>Largeur de l'avatar</summary>
        public const int AVATAR_IMG_WIDTH = 90;
        /// <summary>Hauteur de l'avatar</summary>
        public const int AVATAR_IMG_HEIGHT = 90;


        public const int LOGO_IMG_WIDTH = 60;
        public const int LOGO_IMG_HEIGHT = 40;
        public const int LOGO_SIZE = 5;

        /// <summary>En mode liste, nombre de filtre users avancé sauvegardés</summary>
        public const int NBR_MAX_ADVANCED_USR = 3;

        /// <summary>Rapport : nom de la base</summary>
        public const string STR_REPORT_DB = "EUDOTRAIT";


        /// <summary>EMAILING : Durée de vie  des liens trackings</summary>
        public const int LIFE_TIME_IN_MONTH = 3;
        /// <summary>EMAILING : Durée necessaire pour  la purge des liens tracking </summary>
        public const int PURGE_TIME_IN_MONTH = 12;

        /// <summary>
        /// Selectionner les 300 premières fiches
        /// </summary>
        public const int MAP_NB_MAX_FILES = 300;

        /// <summary>
        /// Taille de la page d'accueil
        /// </summary>
        public static int XRM_HOMEPAGE_GRID_WIDTH = 12;


        /// <summary>
        /// Code d'erreur de retour
        /// </summary>
        public enum ERROR_CODE
        {
            ERR_SESSION_LOST = 1,
            ERR_LIST = 2,
            ERR_FILE = 3,
            ERR_MENU = 4,
            ERR_NAVBAR = 5,
            ERR_SQL = 6,
            ERR_DEFAULT = 999
        }


        #region Page d'accueil
        /// <summary>
        /// Type de contenu d'une eudopart
        /// </summary>
        public enum EUDOPART_CONTENT_TYPE
        {
            /// <summary>VIDE</summary>
            TYP_CONTENT_EMPTY = 0,
            /// <summary>Contenu Html</summary>
            TYP_CONTENT_HTML = 1,
            /// <summary>Contenu Rapport en mode liste</summary>
            TYP_CONTENT_REPORT = 2,
            /// <summary>Contenu Rapport Fusion chart</summary>
            TYP_CONTENT_CHART = 3,
            /// <summary>Contenu PageWeb depuis une URL</summary>
            TYP_CONTENT_WEBPAGE = 4,
            /// <summary>Contenu Flux Rss</summary>
            TYP_CONTENT_RSS = 5,
            /// <summary>Contenu Flux Rss</summary>
            TYP_CONTENT_MRU = 6,
            /// <summary>Contenu Notes/Post-it</summary>
            TYP_CONTENT_NOTE_POSTIT = 7,
            /// <summary>Spécifs XRM</summary>
            TYP_CONTENT_SPECIF = 8
        }


        /// <summary>
        /// Type d'organisation d'eudopart
        /// </summary>
        public enum HOMEPAGE_DISPO
        {
            /// <summary>1 Cadre</summary>
            HPG_DISPO_1 = 1,
            /// <summary>4 Cadres ( en carré)</summary>
            HPG_DISPO_2H2V = 2,
            /// <summary>2 Cadres  Horizontaux</summary>
            HPG_DISPO_2H = 3,
            /// <summary>2 Cadres  Verticaux</summary>
            HPG_DISPO_2V = 4,
            /// <summary>1 Cadre puis 2 cadres verticaux</summary>
            HPG_DISPO_12V = 5,
            /// <summary>2 cadres Horizontaux puis 1 Cadre</summary>
            HPG_DISPO_2H1 = 6,
            /// <summary>Custom</summary>
            HPG_DISPO_CST = 7
        }


        /// <summary>
        /// Type identifiants les différentes parties de la page d'accueil (hors eudopart)
        /// </summary>
        public enum HOMEPAGE_TYPE
        {
            /// <summary>Page d'accueil</summary>
            HPG_HOMEPAGE = 0,
            /// <summary>Liens favoris - Fichier local</summary>
            HPG_FAV_FILE = 1,
            /// <summary>Liens favoris - Adresse EMail</summary>
            HPG_FAV_MAIL = 2,
            /// <summary>Liens favoris - Site Web</summary>
            HPG_FAV_WEB = 3,
            /// <summary>Liens favoris - Site FTP</summary>
            HPG_FAV_FTP = 4,
            /// <summary>Message Express</summary>
            HPG_MSG_EXPRESS = 5,
            /// <summary>Liens favoris - Page asp (masquée)</summary>
            HPG_FAV_EDN = 6
        }
        #endregion


        #region NominatioN
        //Acces à Nomination - GCH : voir v7 si demande d'implémentation, le code n'est ici pas terminé

        /// <summary>Nom de la base NominatioN de référence</summary>
        public const string NOMINATION_DATABASE = "EUDO_NOMINATION";
        /// <summary>Serveur de la base NominatioN (Si Vide = Serveur en cours)</summary>
        public const string NOMINATION_SERVER = "213.41.75.32";
        /// <summary>Lien HTTP pour la mise à jour NominatioN</summary>
        public const string NOMINATION_HREF_VIEW = "https://www.nomination.fr/modules/partenaires/eudonet/<TYPE>.php?auth=valider&login=<LOGIN>&password=<PASSWORD>&num=<NUM>&ewtoken=<TOKEN>&ewserver=<SERVER>";
        /// <summary>HLA - Acces à Nomination - Dev #9448</summary>
        public const string NOMINATION_HREF_ACCESS = "http://www.nomination.fr/modules/xmlstream/checkuser.php?auth=mkey&partner=eudonet&user=<USERMAIL>&authkey=<USERKEY>&pkey=<PKEY>";
        public const string NOMINATION_HREF_ACCESVIEW = "http://www.nomination.fr/modules/nomination/<TYPE>.php?ewtoken=<TOKEN>&ewserver=<SERVER>&num=<NUM>&auth=mkey&partner=eudonet&user=<USERMAIL>&authkey=<USERKEY>&key=<NUMKEY>&pkey=<PKEY>";
        public const string NOMINATION_HREF_HOMEPG = "http://www.nomination.fr/modules/partenaires/eudonet/nomis.php?ewtoken=<TOKEN>&ewserver=<SERVER>&auth=mkey&partner=eudonet&user=<USERMAIL>&authkey=<USERKEY>&pkey=<PKEY>";
        public const string NOMINATION_TAG_TOKEN = "$NOMINATION_TOKEN$";
        public const string NOMINATION_TAG_SERVER = "$NOMINATION_SERVER$";

        #endregion


        #region CHAMPS DE FUSION POUR PUBLIPOSTAGE OU MAIL
        /// <summary>Début du tag</summary>
        public const string HTM_TAG_BEGIN = "<LABEL";
        /// <summary>Fin du tag</summary>
        public const string HTM_TAG_END = "</LABEL>";
        /// <summary>Tag correspondant au descid des champs de fusion</summary>
        public const string HTM_TAG_FIELDDESCID = "ednfielddescid";
        /// <summary>Tag correspondant au type des champs de fusion</summary>
        public const string HTM_TAG_FIELDTYPE = "ednfieldtype";
        /// <summary>Tag correspondant au nom des champs de fusion </summary>
        public const string HTM_TAG_FIELDNAME = "ednfieldname";
        /// <summary>Tag correspondant au form Début</summary>
        public const string HTM_TAG_FORM_BEGIN = "EDNFORMBEGIN";
        /// <summary>Tag correspondant au form Fin</summary>
        public const string HTM_TAG_FORM_END = "EDNFORMEND";
        /// <summary>Tag correspondant au name du form</summary>
        public const string HTM_TAG_FORM_ACTION = "EDNFORMACTION";

        /// <summary>/// Couleur de la date passée/// </summary>
        public const string COL_DATE_PAST = "#ff0000";
        /// <summary>/// Couleur de la date future/// </summary>
        public const string COL_DATE_FUTURE = "#696969";
        /// <summary>/// Couleur de la date du jour/// </summary>
        public const string COL_DATE_TODAY = "#000000";
        /// <summary>/// Couleur d'historisation/// </summary>
        public const string COL_HISTO = "#E7F680";


        // Rappel de rendez-vous {REMINDER_DEFAULT_TIME} minutes avant
        public static int REMINDER_DEFAULT_TIME = 15;

        /*
Const HTM_TAG_BEGIN_MARKED ="##&lt;LABEL"
Const HTM_TAG_END_MARKED ="&lt;LABEL&gt;##"

Const HTM_MRG_FLD_BEGIN = "{"		'{		'Début du champ de fusion à remplacer par la valeur de fusion
Const HTM_MRG_FLD_END = "}"			'}		'Fin du champ de fusion à remplacer par la valeur de fusion
*/
        #endregion


        /// <summary>
        /// Enum des action possible pour le eLoginMGR
        /// </summary>
        public enum LOGIN_ACTION
        {
            UNDEFINED = 0,
            UNLOAD = 1,
            GETUSERLIST = 2,
            AUTHSUBSCRIBER = 3,
            AUTHUSER = 4,
            FROMREDIRECTION = 5,
            FORGOTPASSWORD = 6,
            FORGOTPASSWORDFROMREDIRECTION = 7,
            QUICKLOG = 8,
            EUDOADMIN = 9,
            CHANGEPASSWORD = 10,
            /// <summary>Action qui retourne les informations de paramétrage du SSO en mode SAS et indique s'il est actif. (SSO CAS)</summary>
            SSOSASPARAM = 11,
            /// <summary>Action qui permet l'authentification du SSO en mode CAS sur Eudonet</summary>
            AUTHCAS = 12,

            /// <summary>Deconnecte une session adfs</summary>
            UNLOADADFS = 13,
        }





        /// <summary>
        /// Mode de filtre actif sur le charindex
        /// </summary>
        public enum CHARINDEX_MODE
        {
            ALL,
            ALPHABET,
            NUMERIC
        }


        public enum eFileType
        {
            /// <summary>Mode accueil</summary>
            ACCUEIL = 0,
            /// <summary>Mode liste</summary>
            LIST = 1,
            /// <summary>Mode consultation</summary>
            FILE_CONSULT = 2,
            /// <summary>Mode modification</summary>
            FILE_MODIF = 3,
            /// <summary>Mode Vcard</summary>
            FILE_VCARD = 4,
            /// <summary>Mode création</summary>
            FILE_CREA = 5,
            /// <summary>Mode Impression</summary>
            FILE_PRINT = 6,
            /// <summary>Mode Administration/Options Utilisateur</summary>
            ADMIN = 7,
            /// <summary>Administration du mode fiche</summary>
            ADMIN_FILE = 8,
            /// <summary>Paramètrage des pages d'accueil XRM</summary>
            XRM_HOMEPAGE_PARAM = 9

        }






        public enum ParentInfoFormat
        {
            /// <summary>pas d'information parentes</summary>
            NONE,
            /// <summary>informations parentes en entête</summary>
            HEADER,
            /// <summary>information parentes en pied de page</summary>
            FOOTER
        }

        public enum ExportMode
        {
            /// <summary>
            /// Rapport uniquement
            /// </summary>
            STANDARD = 0,
            /// <summary>
            /// Export qui sera envoyé par mail
            /// </summary>
            MAIL_ONLY = 1,
            /// <summary>
            /// Export par mail et un Rapport
            /// </summary>
            EXPORT_CHOICE = 2
        }




        /// <summary>
        /// Type d'action pour le manager eUserInfosManager
        /// </summary>
        public enum USERS_INFO_ACTION
        {
            UNDEFINED = 0,
            RENDER_POPUP_COPYPREF = 1,
            UPDATE = 2,
            RENDER_POPUP_CHANGEP_WD = 3,
            RENDER_POPUP_CHANGE_SIG = 4,
            RENDER_POPUP_CHANGE_MEMO = 5,

        }

        public enum OPTIONS_TYPE
        {
            /// <summary> CONFIG = 1</summary>
            CONFIG = 1,
            /// <summary> PREF = 2</summary>
            PREF = 2,
            /// <summary> PREF_ADV = 3</summary>
            PREF_ADV = 3,
            /// <summary> CONFIG_ADV = 4</summary>
            CONFIG_ADV = 4,
            /// <summary> USER = 5</summary>
            USER = 5,
            /// <summary> UNDEFINED = 99</summary>
            UNDEFINED = 99
        }

        /// <summary>
        /// Option Utilisateur
        /// </summary>
        public enum OPTIONS_USER
        {
            LANG = 1, // Langue utilisateur
            SIGNATURE = 2, // Signature du mail
            MEMO = 3, // Message utilisateur
            EXPORT = 4, // Rapport d'exports
            STOPNEWS = 5, // Ne plus afficher la newsletter
            FONT_SIZE = 6, // Taille de police
            MRUMODE = 7, // Activer les MRU
            USER_PROFILE = 8 // Profil utilisateur / User profile
        }

        /// <summary>
        /// Type de sélection
        /// </summary>
        public enum SELECTION_TYPE
        {
            FILTERED_SEL = 0
        }



        public enum SYNC_PARAMETER
        {
            FORBID_IMPORT_FIELD_VALUE, //Empêcher la mise à jour de rubrique
        }


        /// <summary>
        /// Liste des fonctionnalités XRM
        /// A compléter
        /// </summary>
        public enum XrmFeature
        {
            Undefined,
            /* Administration */
            Admin,
            AdminParameters,
            AdminParameters_Logo,
            AdminParameters_Navigation,
            AdminParameters_Localization,
            AdminParameters_Supervision,
            AdminParameters_Advanced,
            AdminAccess,
            AdminAccess_GroupsAndUsers,
            AdminAccess_Security,
            AdminAccess_Preferences,
            AdminTabs,
            AdminHome,
            AdminExtensions,
            AdminDashboard,


            /// <summary>
            /// Administration des tables produits
            /// </summary>
            AdminProduct, // MOTEUR - Plateforme - Administrateur produit (#57297/#57301)

            /// <summary>
            /// Administration RGPD
            /// </summary>
            AdminRGPD, // MOTEUR

            /// <summary>
            /// Activation Marketting Automations
            /// </summary>
            AdminEventStep, //Activation table Étapes

            /* Grilles et widgets */
            Grid_WebSubtabInFirstPosition, // SOUS-ONGLET WEB EN PREMIERE POSITION (#52757)
            Grid_ListModeReturn, // MODE LISTE : RETOUR A L’ETAT PRECEDENT (#53413)
            Grid_RightsManager, // GRILLE : GESTION DES DROITS (#53829)


            /* Mode fiche */
            File_CancelLastEntries, // ANNULATION DES DERNIERES SAISIES (#55703)
            File_RefSIRET, // REFERENTIEL DONNEES LEGALES ENTREPRISES (SIRET)

            /* Rubriques */
            Field_Association, // Associé à
            /* Carto */
            Maps_InternationalPredictiveAddr, // ADRESSE PREDICTIVE INTERNATIONALE 
            /* Widgets */
            Widget_Tile, // TUILES
            Widget_Kanban, // KANBAN
            // Import en mode onglet et signet (#57334)
            Import,
            // Signet grille (#57256)
            Bkm_Grid,
            //graphique Filtre Express
            GraphExpressFilter,
            /// <summary>
            /// #68 13x et Backlog #375 - Editeur de templates HTML avancé (grapesjs)
            /// </summary>
            HTMLTemplateEditor,

            /// <summary>GraphTeams </summary>
            GraphTeams,
        }

        /// <summary>
        /// Liste des extensions "Fonctionnalités"
        /// </summary>
        public enum XrmExtension
        {
            Undefined,
            Notifications,
            Cartographie,
            AdressesPredictives,
            API,
            CTI,
            SSO,
            Grilles,
            PublipostageAvance, // Publipostage avancé
            Timeline,
            QRCode,
            Enquetes, // Enquêtes
            DemandesEntrantes, // Demandes entrantes
            Workflow,
            Sepa,
            Compta, // Comptabilité
            Gescom,
            Scoring,
            Dedoublonnage,
            Objectifs,
            Mobile,
            EudoDrop,
            EudoImport,
            EudoSync,
            /// <summary>
            /// Add-in Outlook
            /// </summary>
            OutlookAddin,
            /// <summary>
            /// Interface comptable - Business Soft
            /// </summary>
            AccountingBusinessSoft,
            /// <summary>
            /// Interface comptable - Cegid
            /// </summary>
            AccountingCegid,
            /// <summary>
            /// Interface comptable - Sage
            /// </summary>
            AccountingSage,
            /// <summary>
            /// Interface comptable - EBP
            /// </summary>
            AccountingEBP,
            /// <summary>
            /// Interface comptable - EBP
            /// </summary>
            AccountingSigma,
            //SHA : tâche #1 873
            /// <summary>
            /// Editeur de formulaire avancé
            /// </summary>
            AdvancedForm
        }

        /// <summary>
        /// Référentiel utilisé pour les adresses prédictives
        /// /!\ Modifier eAutoCompletion.js si l'enum est modifiée
        /// </summary>
        public enum PredictiveAddressesRef
        {
            OpenDataGouvFr = 0,
            BingMapsV8 = 1
        }

        /// <summary>
        /// Position de l'unité affichée sur les widgets de type Indicateur
        /// </summary>
        public enum IndicatorWidgetUnitPosition
        {
            RightWithSpace = 0,
            RightWithoutSpace = 1,
            LeftWithSpace = 2,
            LeftWithoutSpace = 3
        }


        /// <summary>
        /// Type d'action au clic sur un widget de type Tuile
        /// En cas de modification, ne pas oublier de modifier aussi dans eWidjetViews.js
        /// </summary>
        public enum XrmWidgetTileAction
        {
            Unspecified = 0,
            OpenWebpage = 1,
            OpenSpecif = 2,
            OpenTab = 3,
            CreateFile = 4,
            GoToFile = 5
        }

        /// <summary>
        /// Comportement à la validation de la création d'une fiche via un widget Tuile
        /// </summary>
        public enum XrmWidgetTileCreateFileValidationBehaviour
        {
            /// <summary>
            /// Ouvrir la fiche
            /// </summary>
            OpenFile = 0,
            /// <summary>
            /// Revenir sur la grille
            /// </summary>
            StayOnGrid = 1
        }

        /// <summary>
        /// Comportement à l'ouverture d'une fiche via un widget Tuile
        /// </summary>
        public enum XrmWidgetTileFileOpenMode
        {
            /// <summary>
            /// Ouvrir la fiche dans son onglet
            /// </summary>
            Default = 0,
            /// <summary>
            /// Ouvrir la fiche en popup
            /// </summary>
            Popup = 1
        }

        /// <summary>
        /// Point d'appel de l'ouverture d'une fiche
        /// Si modif, modifier également dans eMain.js
        /// </summary>
        public enum ShFileCallFrom
        {
            Undefined = 0,
            CallFromNavBar = 1,
            CallFromFile = 2,
            CallFromList = 3,
            CallFromBkm = 4,
            CallFromFinder = 5,
            CallFromDuplicate = 6,
            CallFromGlobalAffect = 7,
            CallFromGlobalInvit = 8,
            CallCampaignMail = 9,
            CallFromSpecif = 10,
            CallFromSendMail = 11,
            CallFromKanban = 12,
            CallFromTileWidget = 13,
            CallFromSendSMS = 14,
            CallFromWidgetCarto = 15,
            /// <summary>Appel depuis le mode fiche téléguidé : utilisé principalement par eFinder pour ne pas afficher le bouton ajouter.</summary>
            CallFromPurpleFile = 16,
            /// <summary>appel de l'assistant à la création depuis la navbar</summary>
            CallFromNavBarToPurple = 17,
            /// <summary>appel de l'assistant à la création depuis la le menu de droite</summary>
            CallFromMenuToPurple = 18,
            /// <summary>appel de l'assistant à la création depuis un signet</summary>
            CallFromBkmToPurple = 19

        }


    }
}
