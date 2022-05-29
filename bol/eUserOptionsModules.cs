using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using Com.Eudonet.Core.Model;
using EudoQuery;

namespace Com.Eudonet.Xrm
{
    /// Classe abstraite de gestion des modules affichés en administration et sur l'écran des options utilisateur
    public abstract class eUserOptionsModules
    {
        /// <summary>
        /// Liste des modules d'options utilisateur et administration
        /// /!\ CETTE ENUM EST RÉPLIQUÉE CÔTÉ JAVASCRIPT DANS eMain.js, ET DOIT ÊTRE DONC MISE À JOUR EN MÊME TEMPS /!\
        /// </summary>
        public enum USROPT_MODULE
        {
            /// <summary>Indéfini</summary>
            UNDEFINED = 0,
            /// <summary>Mon Eudonet</summary>
            MAIN = 1,
            /// <summary>Mes préférences</summary>
            PREFERENCES = 2,
            /// <summary>Choix de la langue</summary>
            PREFERENCES_LANGUAGE = 3,
            /// <summary>Ajouter/modifier ma signature</summary>
            PREFERENCES_SIGNATURE = 4,
            /// <summary>Modifier mon mot de passe</summary>
            PREFERENCES_PASSWORD = 5,
            /// <summary>Ajouter/modifier un mémo</summary>
            PREFERENCES_MEMO = 6,
            /// <summary>Mes options avancées</summary>
            ADVANCED = 7,
            /// <summary>Rapport d'exports</summary>
            ADVANCED_EXPORT = 8,
            /// <summary>Administration</summary>
            ADMIN = 9,
            /// <summary>Paramètres généraux</summary>
            ADMIN_GENERAL = 10,
            /// <summary>Accès</summary>
            ADMIN_ACCESS = 11,
            /// <summary>Groupes et utilisateurs</summary>
            ADMIN_ACCESS_USERGROUPS = 12,
            /// <summary>Sécurité</summary>
            ADMIN_ACCESS_SECURITY = 13,
            /// <summary>Préférences utilisateurs</summary>
            ADMIN_ACCESS_PREF = 14,
            /// <summary>Onglets</summary>
            ADMIN_TABS = 15,
            /// <summary>Accueil</summary>
            ADMIN_HOME = 16,
            /// <summary>Pages d'accueil V7</summary>
            ADMIN_HOME_V7_HOMEPAGES = 17,
            /// <summary>Pages d'accueil XRM</summary>
            ADMIN_HOME_XRM_HOMEPAGES = 18,
            /// <summary>Messages express</summary>
            ADMIN_HOME_EXPRESS_MESSAGE = 19,
            /// <summary>Extensions</summary>
            ADMIN_EXTENSIONS = 20,
            /// <summary>Tableau de bord</summary>
            ADMIN_DASHBOARD = 21,
            /// <summary>Paramètres généraux - Logo</summary>
            ADMIN_GENERAL_LOGO = 22,
            /// <summary>Paramètres généraux - Navigation</summary>
            ADMIN_GENERAL_NAVIGATION = 23,
            /// <summary>Paramètres généraux - Paramètres régionaux</summary>
            ADMIN_GENERAL_LOCALIZATION = 24,
            /// <summary>Paramètres généraux - Supervision</summary>
            ADMIN_GENERAL_SUPERVISION = 25,
            /// <summary>Paramètres généraux - Paramètres avancés</summary>
            ADMIN_GENERAL_CONFIGADV = 26,
            /// <summary>Planning</summary>
            ADVANCED_PLANNING = 27,
            /// <summary>Extensions - Mobile</summary>
            ADMIN_EXTENSIONS_MOBILE = 28,
            /// <summary>Extensions - Synchro</summary>
            ADMIN_EXTENSIONS_SYNCHRO = 29,
            /// <summary>Extensions - SMS</summary>
            ADMIN_EXTENSIONS_SMS = 30,
            /// <summary>Extensions - CTI</summary>
            ADMIN_EXTENSIONS_CTI = 31,
            /// <summary>Extensions - API</summary>
            ADMIN_EXTENSIONS_API = 32,
            /// <summary>Extensions - Emailings externes</summary>
            ADMIN_EXTENSIONS_EXTERNALMAILING = 33,
            /// <summary>Extensions - VCard</summary>
            ADMIN_EXTENSIONS_VCARD = 34,
            /// <summary>Extensions - Snapshot</summary>
            ADMIN_EXTENSIONS_SNAPSHOT = 35,
            /// <summary>Extensions - Emailing</summary>
            ADMIN_EXTENSIONS_EMAILING = 36,
            /// <summary>
            /// Administration d'un fichier
            /// </summary>
            ADMIN_TAB = 37,

            /// <summary>
            /// Admin d'un utilisateur (mode fiche "modif" en   admin)
            /// </summary>
            ADMIN_TAB_USER = 38,
            /// <summary>Accueil</summary>
            HOME = 39,
            /// <summary>Préférences - Taille de la police de caractère</summary>
            PREFERENCES_FONTSIZE = 40,
            /// <summary>Préférences - Activer la suggestion des valeurs récentes</summary>
            PREFERENCES_MRUMODE = 41,
            /// <summary>Extensions du store</summary>
            ADMIN_EXTENSIONS_FROMSTORE = 42,
            /// <summary>Administration grille d'un onglet</summary>
            ADMIN_TAB_GRID = 43,
            /// <summary>Extensions - Grille</summary>
            ADMIN_EXTENSIONS_GRID = 44,
            /// <summary>Administration - Pages d'accueil</summary>
            ADMIN_HOME_XRM_HOMEPAGE = 45,
            /// <summary>Administration - Pages d'accueil - Grille</summary>
            ADMIN_HOME_XRM_HOMEPAGE_GRID = 46,
            /// <summary>Extensions - Notifications</summary>
            ADMIN_EXTENSIONS_NOTIFICATIONS = 47,
            /// <summary>Extensions - Cartographie</summary>
            ADMIN_EXTENSIONS_CARTO = 48,
            /// <summary>Préférences - Thème</summary>
            PREFERENCES_THEME = 49,
            /// <summary>Tableau de bord - Gestion de l'offre</summary>
            ADMIN_DASHBOARD_OFFERMANAGER = 50,
            /// <summary>Tableau de bord - Journal des traitements RGPD</summary>
            ADMIN_DASHBOARD_RGPD = 51,
            /// <summary>
            /// Extension Synchro Agenda Office 365
            /// </summary>
            ADMIN_EXTENSIONS_SYNCHRO_OFFICE365 = 52,
            /// <summary>
            /// Référentiel Sirene
            /// </summary>
            ADMIN_EXTENSIONS_SIRENE = 53,
            /// <summary>
            /// Les logs des traitements RGPD
            /// </summary>
            ADMIN_DASHBOARD_RGPDTREATMENTLOG = 54,
            /// <summary>
            /// Extension Power BI
            /// </summary>
            ADMIN_EXTENSIONS_POWERBI = 55,
            /// <summary>
            /// Extension Add-in Outlook
            /// </summary>
            ADMIN_EXTENSIONS_OUTLOOKADDIN = 56,
            /// <summary>
            /// Backlog #330 - Extension interface de comptabilité - Business Soft (attention, y'a plein de SSSSSSSS)
            /// </summary>
            ADMIN_EXTENSIONS_ACCOUNTING_BUSINESSSOFT = 57,
            /// <summary>
            /// Backlog #331 - Extension interface de comptabilité - Cegid
            /// </summary>
            ADMIN_EXTENSIONS_ACCOUNTING_CEGID = 58,
            /// <summary>
            /// Backlog #332 - Extension interface de comptabilité - Sage
            /// </summary>
            ADMIN_EXTENSIONS_ACCOUNTING_SAGE = 59,
            /// <summary>
            /// Backlog #333 - Extension interface de comptabilité - EBP
            /// </summary>
            ADMIN_EXTENSIONS_ACCOUNTING_EBP = 60,
            /// <summary>
            /// Backlog #334 - Extension interface de comptabilité - Sigma
            /// </summary>
            ADMIN_EXTENSIONS_ACCOUNTING_SIGMA = 61,
            /// <summary>
            /// Backlog #344 - Extension Demandes entrantes Ubiflow
            /// </summary>
            ADMIN_EXTENSIONS_IN_UBIFLOW = 62,
            /// <summary>
            /// Backlog #345 - Extension Demandes entrantes HBS
            /// </summary>
            ADMIN_EXTENSIONS_IN_HBS = 63,
            /// <summary>
            /// Backlog #348 - Extension DocuSign
            /// </summary>
            ADMIN_EXTENSIONS_DOCUSIGN = 64,
            /// <summary>
            /// Backlog #216 - Extension SMS Netmessage
            /// </summary>
            ADMIN_EXTENSIONS_SMS_NETMESSAGE = 65,
            /// <summary>
            /// Backlog #335 - Extension Synchro Exchange 2016 on Premise
            /// </summary>
            ADMIN_EXTENSIONS_SYNCHRO_EXCHANGE2016ONPREMISE = 66,

            /// <summary>
            /// Extension ZAPIER
            /// </summary>
            ADMIN_EXTENSIONS_ZAPIER = 67,

            /// <summary>
            /// Tableau de bord - Volumétrie (US #411 et #412)
            /// </summary>
            ADMIN_DASHBOARD_VOLUME = 68,

            /// <summary>
            /// Extension EXTRANET
            /// </summary>
            ADMIN_EXTENSIONS_EXTRANET = 69,

            //SHA : tâche #1 873
            /// <summary>
            /// Extension FORMULAIRE AVANCE
            /// </summary>
            ADMIN_EXTENSIONS_ADVANCED_FORM = 70,

            /// <summary>
            /// Extension IP DEDIEE
            /// </summary>
            ADMIN_EXTENSIONS_DEDICATED_IP = 71,

            /// <summary>
            ///Block ORM
            /// </summary>
            ADMIN_ORM = 72,

            /// <summary>
            /// Extension de vérification des statuts des adresses mail
            /// </summary>
            ADMIN_EXTENSIONS_MAIL_STATUS_VERIFICATION = 73,

            /// <summary>
            /// Extension LINKEDIN
            /// </summary>
            ADMIN_EXTENSIONS_LINKEDIN = 74,

            /// <summary>
            /// Extension PAIEMENT WORLDLINE
            /// </summary>
            ADMIN_EXTENSIONS_WORLDLINE_PAYMENT = 75,

            /// <summary>Ajouter/modifier un profil utilisateur de préférence
            /// Add/Modify user profile's preference
            /// </summary>
            PREFERENCES_PROFILE = 76
        }

        /// <summary>
        /// Contexte depuis lequel est appelé le module de changement de mot de passe
        /// </summary>
        public enum PREFERENCES_PASSWORD_CONTEXT
        {
            /// <summary>
            /// Non défini
            /// </summary>
            UNDEFINED,
            /// <summary>
            /// Modification d'un mot de passe expiré depuis la page de connexion
            /// </summary>
            LOGIN_EXPIRED,
            /// <summary>
            /// Modification du mot de passe depuis les options utilisateur
            /// </summary>
            USEROPTIONS_PASSWORD,
            /// <summary>
            /// Modification du mot de passe par un administrateur depuis la liste des utilisateurs
            /// </summary>
            ADMIN_USERS
        }

        #region properties

        /// <summary>
        /// Inner Exception
        /// </summary>
        protected Exception _eInnerException = null;

        /// <summary>
        /// Exception rencontrée
        /// </summary>
        public Exception InnerException
        {
            get { return _eInnerException; }
        }


        /// <summary>
        /// Message d'erreur
        /// </summary>
        protected String _sErrorMsg = "";
        /// <summary>
        /// 
        /// </summary>
        public String ErrorMsg
        {
            get { return _sErrorMsg; }
        }


        private ePref _ePref;

        /// <summary>
        /// Objet ePref
        /// </summary>
        public ePref Pref
        {
            get { return _ePref; }

        }




        #endregion


        /// <summary>
        /// Constructeur
        /// </summary>
        public eUserOptionsModules(ePref pref)
        {
            _ePref = pref;
        }

        /// <summary>
        /// Renvoie le renderer correspondant à un module/options utilisateur
        /// Utilisable pour l'affichage du module
        /// Pour tester si le renderer associé a été implémenté, utiliser plutôt ModuleExists() pour des questions de performances
        /// /!\ PAR CONSEQUENT, BIEN PENSER A METTRE A JOUR LA METHODE ModuleExists() LORS DE LA MODIFICATION DE CETTE METHODE /!\
        /// </summary>
        /// <param name="module">Module pour lequel renvoyer le renderer</param>
        /// <param name="pref">Objet ePref</param>
        /// <returns>Renderer de type eAdminRenderer correspondant au module, ou null si erreur/inexistant/non câblé</returns>
        public static eUserOptionsRenderer GetModuleRenderer(USROPT_MODULE module, ePref pref)
        {
            eUserOptionsRenderer er = null;

            switch (module)
            {
                // Pour tous les renderers de "section" (page principale ou autres), on fait appel au renderer racine qui affichera les blocs
                // de liens correspondant à la section en question
                // Administration
                case USROPT_MODULE.ADMIN:
                // Options utilisateur
                case USROPT_MODULE.MAIN:
                case USROPT_MODULE.PREFERENCES:
                case USROPT_MODULE.ADVANCED:
                    er = eRendererFactory.CreateUserOptionsRenderer(pref, module);
                    break;
                case USROPT_MODULE.PREFERENCES_FONTSIZE:
                    er = eRendererFactory.CreateUserOptionsPrefFontSizeRenderer(pref);
                    break;
                case USROPT_MODULE.PREFERENCES_MRUMODE:
                    er = eRendererFactory.CreateUserOptionsPrefMruModeRenderer(pref);
                    break;
                case USROPT_MODULE.PREFERENCES_LANGUAGE:
                    er = eRendererFactory.CreateUserOptionsPrefLangRenderer(pref);
                    break;
                case USROPT_MODULE.PREFERENCES_PROFILE:
                    er = eRendererFactory.CreateUserOptionsPrefProfileRenderer(pref);
                    break;
                case USROPT_MODULE.PREFERENCES_SIGNATURE:
                    er = eRendererFactory.CreateUserOptionsPrefSignRenderer(pref);
                    break;
                case USROPT_MODULE.PREFERENCES_MEMO:
                    er = eRendererFactory.CreateUserOptionsPrefMemoRenderer(pref);
                    break;
                case USROPT_MODULE.PREFERENCES_PASSWORD:
                    er = eRendererFactory.CreateUserOptionsPrefPwdRenderer(pref, pref.UserId, PREFERENCES_PASSWORD_CONTEXT.USEROPTIONS_PASSWORD);
                    break;
                case USROPT_MODULE.ADVANCED_EXPORT:
                    er = eRendererFactory.CreateAdminUserOptionsExportRenderer(pref);
                    break;
                // Autres cas : renderer non implémenté
                case USROPT_MODULE.UNDEFINED:
                default:
                    er = null;
                    break;
            }

            return er;
        }

        /// <summary>
        /// Renvoie le renderer correspondant à un module/options utilisateur dont le nom est passé en chaîne
        /// Utilisable pour l'affichage du module
        /// Pour tester si le renderer associé a été implémenté, utiliser plutôt ModuleExists() pour des questions de performances
        /// </summary>
        /// <param name="moduleName">Chaîne (constante) désignant le module pour lequel renvoyer le renderer</param>
        /// <param name="pref">Objet ePref</param>
        /// <returns>Renderer de type eAdminRenderer correspondant au module, ou null si erreur/inexistant/non câblé</returns>
        public static eUserOptionsRenderer GetModuleRenderer(string moduleName, ePref pref)
        {
            USROPT_MODULE module = USROPT_MODULE.UNDEFINED;

            Enum.TryParse(moduleName, out module);

            return GetModuleRenderer(module, pref);
        }

        /// <summary>
        /// Indique si un renderer est implémenté pour le module donné, sans instancier celui-ci
        /// /!\ BIEN MAINTENIR CETTE METHODE A JOUR LORS DE LA MODIFICATION DE GetModuleRENDERER() /!\
        /// </summary>
        /// <param name="module">Chaîne (constante) désignant le module pour lequel renvoyer l'information</param>
        /// <returns>
        /// true si un renderer est référencé pour le module indiqué, false sinon
        /// </returns>
        public static bool ModuleExists(USROPT_MODULE module)
        {

            switch (module)
            {
                // Administration
                case USROPT_MODULE.ADMIN:
                case USROPT_MODULE.ADMIN_GENERAL:
                case USROPT_MODULE.ADMIN_ORM:
                case USROPT_MODULE.ADMIN_ACCESS:
                case USROPT_MODULE.ADMIN_ACCESS_USERGROUPS:
                case USROPT_MODULE.ADMIN_ACCESS_SECURITY:
                case USROPT_MODULE.ADMIN_ACCESS_PREF:
                case USROPT_MODULE.ADMIN_TABS:
                case USROPT_MODULE.ADMIN_HOME:
                case USROPT_MODULE.ADMIN_HOME_V7_HOMEPAGES:
                case USROPT_MODULE.ADMIN_HOME_XRM_HOMEPAGES:
                case USROPT_MODULE.ADMIN_HOME_EXPRESS_MESSAGE:
                case USROPT_MODULE.ADMIN_EXTENSIONS:
                case USROPT_MODULE.ADMIN_EXTENSIONS_FROMSTORE:
                case USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO:
                case USROPT_MODULE.ADMIN_EXTENSIONS_MOBILE:
                case USROPT_MODULE.ADMIN_EXTENSIONS_OUTLOOKADDIN:
                case USROPT_MODULE.ADMIN_EXTENSIONS_LINKEDIN:
                case USROPT_MODULE.ADMIN_EXTENSIONS_SIRENE:
                case USROPT_MODULE.ADMIN_EXTENSIONS_SMS:
                case USROPT_MODULE.ADMIN_EXTENSIONS_CTI:
                case USROPT_MODULE.ADMIN_EXTENSIONS_API: //TODO : Verrouiller pour superuser et les adresses ip internes des agences Eudo 
                case USROPT_MODULE.ADMIN_EXTENSIONS_EXTERNALMAILING:
                case USROPT_MODULE.ADMIN_EXTENSIONS_VCARD:
                case USROPT_MODULE.ADMIN_EXTENSIONS_SNAPSHOT: // TODO/TOCHECK : Verrouiller pour superuser et les adresses ip internes des agences Eudo ?
                case USROPT_MODULE.ADMIN_EXTENSIONS_EMAILING:
                case USROPT_MODULE.ADMIN_EXTENSIONS_CARTO:
                case USROPT_MODULE.ADMIN_EXTENSIONS_GRID:
                case USROPT_MODULE.ADMIN_TAB_GRID:
                case USROPT_MODULE.ADMIN_EXTENSIONS_NOTIFICATIONS:
                case USROPT_MODULE.ADMIN_EXTENSIONS_POWERBI:
                case USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_BUSINESSSOFT:
                case USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_CEGID:
                case USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_SAGE:
                case USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_EBP:
                case USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_SIGMA:
                case USROPT_MODULE.ADMIN_EXTENSIONS_IN_UBIFLOW:
                case USROPT_MODULE.ADMIN_EXTENSIONS_IN_HBS:
                case USROPT_MODULE.ADMIN_EXTENSIONS_DOCUSIGN:
                case USROPT_MODULE.ADMIN_EXTENSIONS_SMS_NETMESSAGE:
                case USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO_EXCHANGE2016ONPREMISE:
                case USROPT_MODULE.ADMIN_EXTENSIONS_EXTRANET:
                //SHA : tâche #1 873
                case USROPT_MODULE.ADMIN_EXTENSIONS_ADVANCED_FORM:
                case USROPT_MODULE.ADMIN_EXTENSIONS_DEDICATED_IP:
                case USROPT_MODULE.ADMIN_EXTENSIONS_MAIL_STATUS_VERIFICATION:
                case USROPT_MODULE.ADMIN_EXTENSIONS_WORLDLINE_PAYMENT:
                // Options utilisateur / "Mon Eudonet"
                case USROPT_MODULE.MAIN:
                case USROPT_MODULE.HOME:
                case USROPT_MODULE.PREFERENCES:
                case USROPT_MODULE.PREFERENCES_FONTSIZE:
                case USROPT_MODULE.PREFERENCES_MRUMODE:
                case USROPT_MODULE.PREFERENCES_THEME:
                case USROPT_MODULE.PREFERENCES_LANGUAGE:
                case USROPT_MODULE.PREFERENCES_PROFILE:
                case USROPT_MODULE.PREFERENCES_SIGNATURE:
                case USROPT_MODULE.PREFERENCES_MEMO:
                case USROPT_MODULE.PREFERENCES_PASSWORD:
                case USROPT_MODULE.ADVANCED_EXPORT:
                case USROPT_MODULE.ADVANCED_PLANNING:
                case USROPT_MODULE.ADMIN_GENERAL_LOGO:
                case USROPT_MODULE.ADMIN_GENERAL_NAVIGATION:
                case USROPT_MODULE.ADMIN_GENERAL_LOCALIZATION:
                case USROPT_MODULE.ADMIN_GENERAL_SUPERVISION:
                case USROPT_MODULE.ADMIN_GENERAL_CONFIGADV:
                case USROPT_MODULE.ADMIN_DASHBOARD:
                case USROPT_MODULE.ADMIN_DASHBOARD_RGPDTREATMENTLOG:
                case USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO_OFFICE365:

                    return true;
                // Autres cas : renderer non implémenté
                case USROPT_MODULE.ADMIN_DASHBOARD_RGPD: //Demande #64791 - On n'affiche plus cette entrée pour le moment
                case USROPT_MODULE.UNDEFINED:
                default:
                    return false;
            }
        }

        /// <summary>
        /// Indique si un renderer est implémenté pour le module donné, sans instancier celui-ci
        /// /!\ BIEN MAINTENIR CETTE FONCTION A JOUR LORS DE LA MODIFICATION DE GetModuleRENDERER() /!\
        /// </summary>
        /// <param name="moduleName">Chaîne (constante) désignant le module pour lequel renvoyer l'information</param>
        /// <returns>true si un renderer est référencé pour le module indiqué, false sinon</returns>
        public static bool ModuleExists(string moduleName)
        {
            USROPT_MODULE module = USROPT_MODULE.UNDEFINED;

            Enum.TryParse(moduleName, out module);

            return ModuleExists(module);
        }


        /// <summary>
        /// Indique si un module peut/doit être référencé dans la navbar et les menus. Vérifie également que le module est disponible pour l'offre et la version.
        /// /!\ BIEN MAINTENIR CETTE FONCTION A JOUR LORS DE LA MODIFICATION DE GetModuleRENDERER() /!\
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="module">Module pour lequel renvoyer l'information</param>
        /// <returns>
        /// true s'il faut référencer le module indiqué, false sinon
        /// </returns>
        public static bool ModuleCanAppearInNavbar(ePref pref, USROPT_MODULE module)
        {

            switch (module)
            {
                // Administration
                case USROPT_MODULE.ADMIN:
                case USROPT_MODULE.ADMIN_GENERAL:
                case USROPT_MODULE.ADMIN_ACCESS:
                case USROPT_MODULE.ADMIN_ACCESS_USERGROUPS:
                case USROPT_MODULE.ADMIN_ACCESS_SECURITY:
                case USROPT_MODULE.ADMIN_ACCESS_PREF:
                case USROPT_MODULE.ADMIN_TAB_GRID:
                case USROPT_MODULE.ADMIN_TABS:
                case USROPT_MODULE.ADMIN_HOME:
                case USROPT_MODULE.ADMIN_HOME_V7_HOMEPAGES:
                case USROPT_MODULE.ADMIN_HOME_XRM_HOMEPAGE:
                case USROPT_MODULE.ADMIN_HOME_XRM_HOMEPAGES:
                case USROPT_MODULE.ADMIN_HOME_EXPRESS_MESSAGE:
                case USROPT_MODULE.ADMIN_EXTENSIONS:
                case USROPT_MODULE.ADMIN_EXTENSIONS_FROMSTORE:
                case USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO:
                case USROPT_MODULE.ADMIN_EXTENSIONS_CARTO:
                case USROPT_MODULE.ADMIN_EXTENSIONS_MOBILE:
                case USROPT_MODULE.ADMIN_EXTENSIONS_OUTLOOKADDIN:
                case USROPT_MODULE.ADMIN_EXTENSIONS_LINKEDIN:
                case USROPT_MODULE.ADMIN_EXTENSIONS_SIRENE:
                case USROPT_MODULE.ADMIN_EXTENSIONS_SMS:
                case USROPT_MODULE.ADMIN_EXTENSIONS_CTI:
                case USROPT_MODULE.ADMIN_EXTENSIONS_API:
                case USROPT_MODULE.ADMIN_EXTENSIONS_EXTERNALMAILING:
                case USROPT_MODULE.ADMIN_EXTENSIONS_VCARD:
                case USROPT_MODULE.ADMIN_EXTENSIONS_SNAPSHOT: // TODO/TOCHECK : Verrouiller pour superuser et les adresses ip internes des agences Eudo ?
                case USROPT_MODULE.ADMIN_EXTENSIONS_EMAILING:
                case USROPT_MODULE.ADMIN_DASHBOARD:
                case USROPT_MODULE.ADMIN_DASHBOARD_RGPD:
                case USROPT_MODULE.ADMIN_DASHBOARD_RGPDTREATMENTLOG:
                case USROPT_MODULE.ADMIN_GENERAL_LOGO:
                case USROPT_MODULE.ADMIN_GENERAL_NAVIGATION:
                case USROPT_MODULE.ADMIN_GENERAL_LOCALIZATION:
                case USROPT_MODULE.ADMIN_GENERAL_SUPERVISION:
                case USROPT_MODULE.ADMIN_GENERAL_CONFIGADV:
                case USROPT_MODULE.ADMIN_EXTENSIONS_GRID:
                case USROPT_MODULE.ADMIN_EXTENSIONS_NOTIFICATIONS:
                case USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO_OFFICE365:
                case USROPT_MODULE.ADMIN_EXTENSIONS_POWERBI:
                case USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_BUSINESSSOFT:
                case USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_CEGID:
                case USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_SAGE:
                case USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_EBP:
                case USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_SIGMA:
                case USROPT_MODULE.ADMIN_EXTENSIONS_IN_UBIFLOW:
                case USROPT_MODULE.ADMIN_EXTENSIONS_IN_HBS:
                case USROPT_MODULE.ADMIN_EXTENSIONS_DOCUSIGN:
                case USROPT_MODULE.ADMIN_EXTENSIONS_SMS_NETMESSAGE:
                case USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO_EXCHANGE2016ONPREMISE:
                case USROPT_MODULE.ADMIN_EXTENSIONS_EXTRANET:
                case USROPT_MODULE.ADMIN_EXTENSIONS_ADVANCED_FORM:
                case USROPT_MODULE.ADMIN_EXTENSIONS_DEDICATED_IP:
                case USROPT_MODULE.ADMIN_EXTENSIONS_MAIL_STATUS_VERIFICATION:
                case USROPT_MODULE.ADMIN_EXTENSIONS_WORLDLINE_PAYMENT:
                //SHA : tâche #1 873
                // Options utilisateur / "Mon Eudonet"
                case USROPT_MODULE.MAIN:
                case USROPT_MODULE.HOME:
                case USROPT_MODULE.PREFERENCES:
                case USROPT_MODULE.PREFERENCES_THEME:
                case USROPT_MODULE.PREFERENCES_LANGUAGE:
                case USROPT_MODULE.PREFERENCES_PROFILE:
                case USROPT_MODULE.PREFERENCES_SIGNATURE:
                case USROPT_MODULE.PREFERENCES_MEMO:
                case USROPT_MODULE.PREFERENCES_PASSWORD:
                case USROPT_MODULE.ADVANCED_EXPORT:
                case USROPT_MODULE.ADVANCED_PLANNING:
                    eConst.XrmFeature feature = eUserOptionsModules.GetModuleFeature(module);
                    return (eFeaturesManager.IsFeatureAvailable(pref, feature));
                case USROPT_MODULE.ADMIN_ORM:
                    feature = eUserOptionsModules.GetModuleFeature(module);
                    return (pref.User.UserLevel >= (int)UserLevel.LEV_USR_SUPERADMIN && eFeaturesManager.IsFeatureAvailable(pref, feature));
                // Autres cas : lien non affiché                
                case USROPT_MODULE.UNDEFINED:
                default:
                    return false;
            }
        }

        /// <summary>
        /// Indique si un module peut/doit être référencé dans la navbar et les menus
        /// /!\ BIEN MAINTENIR CETTE FONCTION A JOUR LORS DE LA MODIFICATION DE GetModuleRENDERER() /!\
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="moduleName">Chaîne (constante) désignant le module pour lequel renvoyer l'information</param>
        /// <returns>
        /// true s'il faut référencer le module indiqué, false sinon
        /// </returns>
        public static bool ModuleCanAppearInNavbar(ePref pref, string moduleName)
        {
            USROPT_MODULE module = USROPT_MODULE.UNDEFINED;

            Enum.TryParse(moduleName, out module);

            return ModuleCanAppearInNavbar(pref, module);
        }

        /// <summary>
        /// Retourne le libellé correspondant à un module/option utilisateur
        /// </summary>
        /// <param name="module">Module concerné</param>
        /// <param name="pref"></param>
        /// <returns>
        /// Libellé du module
        /// </returns>
        public static string GetModuleLabel(USROPT_MODULE module, ePrefLite pref)
        {
            switch (module)
            {
                // Options utilisateur / "Mon Eudonet"
                case USROPT_MODULE.MAIN:
                    return eResApp.GetRes(pref, 7174); // Options utilisateur / "Mon Eudonet"
                case USROPT_MODULE.HOME:
                    return eResApp.GetRes(pref, 551); // Accueil
                case USROPT_MODULE.PREFERENCES:
                    return eResApp.GetRes(pref, 6772); // Mes préférences
                case USROPT_MODULE.PREFERENCES_FONTSIZE:
                    return eResApp.GetRes(pref, 7982); // Taille de la police
                case USROPT_MODULE.PREFERENCES_MRUMODE:
                    return eResApp.GetRes(pref, 8014); // Activer les MRU
                case USROPT_MODULE.PREFERENCES_THEME:
                    return eResApp.GetRes(pref, 8175); // Choix du thème
                case USROPT_MODULE.PREFERENCES_LANGUAGE:
                    return eResApp.GetRes(pref, 6773); // Choix de la langue
                case USROPT_MODULE.PREFERENCES_PROFILE:
                    return eResApp.GetRes(pref, 8933); // Préférence d'affichage
                case USROPT_MODULE.PREFERENCES_SIGNATURE:
                    return eResApp.GetRes(pref, 6778); // Ajouter / modifier ma signature
                case USROPT_MODULE.PREFERENCES_PASSWORD:
                    return eResApp.GetRes(pref, 6781); // Modifier mon mot de passe
                case USROPT_MODULE.PREFERENCES_MEMO:
                    return eResApp.GetRes(pref, 6784); // Ajouter / modifier un mémo
                case USROPT_MODULE.ADVANCED:
                    return eResApp.GetRes(pref, 6774); // Mes options avancées
                case USROPT_MODULE.ADVANCED_EXPORT:
                    return eResApp.GetRes(pref, 6785); // Rapport d'exports
                case USROPT_MODULE.ADVANCED_PLANNING:
                    return eResApp.GetRes(pref, 830); // Planning
                                                      // Administration
                case USROPT_MODULE.ADMIN_GENERAL:
                    return eResApp.GetRes(pref, 181); // Paramètres généraux
                case USROPT_MODULE.ADMIN_ACCESS:
                    return eResApp.GetRes(pref, 7175); // Accès
                case USROPT_MODULE.ADMIN_ACCESS_USERGROUPS:
                    return eResApp.GetRes(pref, 7176); // Groupes et utilisateurs
                case USROPT_MODULE.ADMIN_ACCESS_SECURITY:
                    return eResApp.GetRes(pref, 206); // Sécurité
                case USROPT_MODULE.ADMIN_ACCESS_PREF:
                    return eResApp.GetRes(pref, 7708); // Préférences
                case USROPT_MODULE.ADMIN_TABS:
                    return eResApp.GetRes(pref, 217); // Onglets
                case USROPT_MODULE.ADMIN_HOME:
                    return eResApp.GetRes(pref, 551); // Accueil
                case USROPT_MODULE.ADMIN_HOME_V7_HOMEPAGES:
                    return eResApp.GetRes(pref, 8055); // Pages d'accueil V7
                case USROPT_MODULE.ADMIN_HOME_XRM_HOMEPAGES:
                    return eResApp.GetRes(pref, 8071); // Grilles XRM
                case USROPT_MODULE.ADMIN_HOME_EXPRESS_MESSAGE:
                    return eResApp.GetRes(pref, 350); // Message Express
                case USROPT_MODULE.ADMIN_EXTENSIONS:
                case USROPT_MODULE.ADMIN_EXTENSIONS_CARTO:
                case USROPT_MODULE.ADMIN_EXTENSIONS_FROMSTORE:
                    return eResApp.GetRes(pref, 2375); // Modules --> Extensions devient --> Eudostore. GLA.
                case USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO:
                    return eResApp.GetRes(pref, 1030); // EudoSync
                case USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO_OFFICE365:
                    return eResApp.GetRes(pref, 8489); // EudoSynchroExchange
                case USROPT_MODULE.ADMIN_EXTENSIONS_MOBILE:
                    return eResApp.GetRes(pref, 1554); // Eudonet Mobile
                case USROPT_MODULE.ADMIN_EXTENSIONS_SMS:
                    return eResApp.GetRes(pref, 7757); // Envoi de SMS
                case USROPT_MODULE.ADMIN_EXTENSIONS_CTI:
                    return eResApp.GetRes(pref, 7786); // CTI
                case USROPT_MODULE.ADMIN_EXTENSIONS_API:
                    return eResApp.GetRes(pref, 7783); // EudoAPI
                case USROPT_MODULE.ADMIN_EXTENSIONS_EXTERNALMAILING:
                    return eResApp.GetRes(pref, 7790); // Teradata
                case USROPT_MODULE.ADMIN_EXTENSIONS_NOTIFICATIONS:
                    return eResApp.GetRes(pref, 6885); // Notifications
                case USROPT_MODULE.ADMIN_EXTENSIONS_VCARD:
                    return eResApp.GetRes(pref, 7830); // vCard
                case USROPT_MODULE.ADMIN_EXTENSIONS_SNAPSHOT:
                    return eResApp.GetRes(pref, 7889); // Snapshot
                case USROPT_MODULE.ADMIN_EXTENSIONS_EMAILING:
                    return eResApp.GetRes(pref, 5045); // Emailing
                case USROPT_MODULE.ADMIN_EXTENSIONS_GRID:
                    return eResApp.GetRes(pref, 8065);
                case USROPT_MODULE.ADMIN_DASHBOARD:
                    return eResApp.GetRes(pref, 7178); // Tableau de bord
                case USROPT_MODULE.ADMIN_DASHBOARD_RGPD:
                    return eResApp.GetRes(pref, 8406); // Qualité des données (RGPD)
                case USROPT_MODULE.ADMIN_DASHBOARD_RGPDTREATMENTLOG:
                    return eResApp.GetRes(pref, 8744); // Journal des traitements RGPD
                case USROPT_MODULE.ADMIN_GENERAL_LOGO:
                    return eResApp.GetRes(pref, 333); // Logo
                case USROPT_MODULE.ADMIN_GENERAL_NAVIGATION:
                    return eResApp.GetRes(pref, 8072);
                case USROPT_MODULE.ADMIN_GENERAL_LOCALIZATION:
                    return eResApp.GetRes(pref, 8073);
                case USROPT_MODULE.ADMIN_GENERAL_SUPERVISION:
                    return eResApp.GetRes(pref, 8074);
                case USROPT_MODULE.ADMIN_GENERAL_CONFIGADV:
                    return eResApp.GetRes(pref, 6750);
                case USROPT_MODULE.ADMIN_TAB_USER:
                    return eResApp.GetRes(pref, 6); // Tableau de bord
                case USROPT_MODULE.ADMIN_EXTENSIONS_SIRENE:
                    return eResApp.GetRes(pref, 8544); // Référentiel Sirene
                case USROPT_MODULE.ADMIN_EXTENSIONS_POWERBI:
                    return eResApp.GetRes(pref, 8702); // Microsoft Power BI
                case USROPT_MODULE.ADMIN_EXTENSIONS_OUTLOOKADDIN:
                    return eResApp.GetRes(pref, 2027); // Add-in Outlook
                case USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_BUSINESSSOFT:
                    return eResApp.GetRes(pref, 2160).Replace("<SOFTWARENAME>", "Business Soft");
                case USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_CEGID:
                    return eResApp.GetRes(pref, 2160).Replace("<SOFTWARENAME>", "Cegid");
                case USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_SAGE:
                    return eResApp.GetRes(pref, 2160).Replace("<SOFTWARENAME>", "Sage");
                case USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_EBP:
                    return eResApp.GetRes(pref, 2160).Replace("<SOFTWARENAME>", "EBP");
                case USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_SIGMA:
                    return eResApp.GetRes(pref, 2160).Replace("<SOFTWARENAME>", "Sigma");
                case USROPT_MODULE.ADMIN_EXTENSIONS_IN_UBIFLOW:
                    return eResApp.GetRes(pref, 2162);
                case USROPT_MODULE.ADMIN_EXTENSIONS_IN_HBS:
                    return eResApp.GetRes(pref, 2163);
                case USROPT_MODULE.ADMIN_EXTENSIONS_DOCUSIGN:
                    return eResApp.GetRes(pref, 2164);
                case USROPT_MODULE.ADMIN_EXTENSIONS_SMS_NETMESSAGE:
                    return eResApp.GetRes(pref, 2165);
                case USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO_EXCHANGE2016ONPREMISE:
                    return eResApp.GetRes(pref, 2166);
                case USROPT_MODULE.ADMIN_EXTENSIONS_EXTRANET:
                    return eResApp.GetRes(pref, 2399);
                //SHA : tâche #1 873
                case USROPT_MODULE.ADMIN_EXTENSIONS_ADVANCED_FORM:
                    return eResApp.GetRes(pref, 1142) + " " + eResApp.GetRes(pref, 414); //Formulaire Avancé
                case USROPT_MODULE.ADMIN_EXTENSIONS_DEDICATED_IP:
                    return eResApp.GetRes(pref, 2808);// IP Dédiée
                case USROPT_MODULE.ADMIN_ORM:
                    return eResApp.GetRes(pref, 2949);// 
                case USROPT_MODULE.ADMIN_EXTENSIONS_MAIL_STATUS_VERIFICATION:
                    return eResApp.GetRes(pref, 2975);
                case USROPT_MODULE.ADMIN_EXTENSIONS_LINKEDIN:
                    return eResApp.GetRes(pref, 3045);
                case USROPT_MODULE.ADMIN_EXTENSIONS_WORLDLINE_PAYMENT:
                    return eResApp.GetRes(pref, 8784); // Paiement Worldline
                case USROPT_MODULE.ADMIN:
                case USROPT_MODULE.UNDEFINED:
                default:
                    return eResApp.GetRes(pref, 21); // Administration
            }
        }

        /// <summary>
        /// Retourne le libellé correspondant à un module/options utilisateur pour le menu de droite
        /// Peut retourner la même chaîne que GetModuleLabel() s'il n'est pas nécessaire d'utiliser un libellé différent
        /// </summary>
        /// <param name="module">Module concerné</param>
        /// <param name="pref"></param>
        /// <returns>
        /// Libellé du module
        /// </returns>
        public static string GetModuleRightMenuLabel(USROPT_MODULE module, ePref pref)
        {
            switch (module)
            {
                // Options utilisateur - Libellés spécifiques
                case USROPT_MODULE.PREFERENCES_SIGNATURE:
                    return eResApp.GetRes(pref, 6779); // Ma signature
                case USROPT_MODULE.PREFERENCES_MEMO:
                    return eResApp.GetRes(pref, 235); // Mémo
                case USROPT_MODULE.PREFERENCES_PASSWORD:
                    return eResApp.GetRes(pref, 6780); // Mon mot de passe
                                                       // Options utilisateur - Libellés identiques au libellé principal
                case USROPT_MODULE.MAIN:
                case USROPT_MODULE.HOME:
                case USROPT_MODULE.ADVANCED:
                case USROPT_MODULE.ADVANCED_EXPORT:
                case USROPT_MODULE.PREFERENCES:
                case USROPT_MODULE.PREFERENCES_THEME:
                case USROPT_MODULE.PREFERENCES_LANGUAGE:
                case USROPT_MODULE.PREFERENCES_PROFILE:
                // Administration - Libellés identiques au libellé principal
                case USROPT_MODULE.ADMIN_GENERAL:
                case USROPT_MODULE.ADMIN_ACCESS:
                case USROPT_MODULE.ADMIN_ACCESS_USERGROUPS:
                case USROPT_MODULE.ADMIN_ACCESS_SECURITY:
                case USROPT_MODULE.ADMIN_ACCESS_PREF:
                case USROPT_MODULE.ADMIN_TABS:
                case USROPT_MODULE.ADMIN_HOME:
                case USROPT_MODULE.ADMIN_HOME_V7_HOMEPAGES:
                case USROPT_MODULE.ADMIN_HOME_XRM_HOMEPAGES:
                case USROPT_MODULE.ADMIN_HOME_EXPRESS_MESSAGE:
                case USROPT_MODULE.ADMIN_EXTENSIONS_CARTO:
                case USROPT_MODULE.ADMIN_EXTENSIONS:
                case USROPT_MODULE.ADMIN_EXTENSIONS_FROMSTORE:
                case USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO:
                case USROPT_MODULE.ADMIN_EXTENSIONS_MOBILE:
                case USROPT_MODULE.ADMIN_EXTENSIONS_OUTLOOKADDIN:
                case USROPT_MODULE.ADMIN_EXTENSIONS_LINKEDIN:
                case USROPT_MODULE.ADMIN_EXTENSIONS_SIRENE:
                case USROPT_MODULE.ADMIN_EXTENSIONS_SMS:
                case USROPT_MODULE.ADMIN_EXTENSIONS_CTI:
                case USROPT_MODULE.ADMIN_EXTENSIONS_API:
                case USROPT_MODULE.ADMIN_EXTENSIONS_EXTERNALMAILING:
                case USROPT_MODULE.ADMIN_EXTENSIONS_EMAILING:
                case USROPT_MODULE.ADMIN_EXTENSIONS_VCARD:
                case USROPT_MODULE.ADMIN_EXTENSIONS_SNAPSHOT:
                case USROPT_MODULE.ADMIN_EXTENSIONS_GRID:
                case USROPT_MODULE.ADMIN_EXTENSIONS_NOTIFICATIONS:
                case USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO_OFFICE365:
                case USROPT_MODULE.ADMIN_EXTENSIONS_POWERBI:
                case USROPT_MODULE.ADMIN_EXTENSIONS_EXTRANET:
                //SHA : tâche #1 873
                case USROPT_MODULE.ADMIN_EXTENSIONS_ADVANCED_FORM:
                case USROPT_MODULE.ADMIN_EXTENSIONS_DEDICATED_IP:
                case USROPT_MODULE.ADMIN_EXTENSIONS_MAIL_STATUS_VERIFICATION:
                case USROPT_MODULE.ADMIN_DASHBOARD:
                case USROPT_MODULE.ADMIN_DASHBOARD_RGPD:
                case USROPT_MODULE.ADMIN_DASHBOARD_RGPDTREATMENTLOG:
                case USROPT_MODULE.ADMIN_GENERAL_LOGO:
                case USROPT_MODULE.ADMIN_GENERAL_NAVIGATION:
                case USROPT_MODULE.ADMIN_GENERAL_LOCALIZATION:
                case USROPT_MODULE.ADMIN_GENERAL_SUPERVISION:
                case USROPT_MODULE.ADMIN_GENERAL_CONFIGADV:
                case USROPT_MODULE.ADMIN:
                case USROPT_MODULE.UNDEFINED:
                default:
                    return GetModuleLabel(module, pref);
            }
        }

        /// <summary>
        /// Retourne le libellé correspondant à un module/options utilisateur pour le titre de milieu de page
        /// Peut retourner la même chaîne que GetModuleLabel() s'il n'est pas nécessaire d'utiliser un libellé différent
        /// </summary>
        /// <param name="module">Module concerné</param>
        /// <param name="pref"></param>
        /// <returns>
        /// Libellé du module
        /// </returns>
        public static string GetModuleMiddleTitleLabel(USROPT_MODULE module, ePref pref)
        {
            switch (module)
            {
                // Options utilisateur - Libellés spécifiques
                case USROPT_MODULE.PREFERENCES_LANGUAGE:
                    return eResApp.GetRes(pref, 4); // Langue
                case USROPT_MODULE.PREFERENCES_PROFILE:
                    return eResApp.GetRes(pref, 8933); // Préférence d'affichage
                case USROPT_MODULE.PREFERENCES_FONTSIZE:
                    return eResApp.GetRes(pref, 7982); // Taille de la police de caractère
                case USROPT_MODULE.PREFERENCES_MRUMODE:
                    return eResApp.GetRes(pref, 8014); // Activer les MRU
                case USROPT_MODULE.PREFERENCES_SIGNATURE:
                    return eResApp.GetRes(pref, 6779); // Ma signature
                case USROPT_MODULE.PREFERENCES_MEMO:
                    return eResApp.GetRes(pref, 235); // Mémo
                case USROPT_MODULE.PREFERENCES_PASSWORD:
                    return eResApp.GetRes(pref, 2); // Mot de passe
                                                    // Options utilisateur - Libellés identiques au libellé principal
                case USROPT_MODULE.MAIN:
                case USROPT_MODULE.HOME:
                case USROPT_MODULE.ADVANCED:
                case USROPT_MODULE.ADVANCED_EXPORT:
                case USROPT_MODULE.ADVANCED_PLANNING:
                case USROPT_MODULE.PREFERENCES:
                // Administration - Libellés identiques au libellé principal
                case USROPT_MODULE.ADMIN_GENERAL:
                case USROPT_MODULE.ADMIN_ACCESS:
                case USROPT_MODULE.ADMIN_ACCESS_USERGROUPS:
                case USROPT_MODULE.ADMIN_ACCESS_SECURITY:
                case USROPT_MODULE.ADMIN_ACCESS_PREF:
                case USROPT_MODULE.ADMIN_TABS:
                case USROPT_MODULE.ADMIN_HOME:
                case USROPT_MODULE.ADMIN_HOME_V7_HOMEPAGES:
                case USROPT_MODULE.ADMIN_HOME_XRM_HOMEPAGES:
                case USROPT_MODULE.ADMIN_HOME_EXPRESS_MESSAGE:
                case USROPT_MODULE.ADMIN_EXTENSIONS:
                case USROPT_MODULE.ADMIN_EXTENSIONS_FROMSTORE:
                case USROPT_MODULE.ADMIN_EXTENSIONS_CARTO:
                case USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO:
                case USROPT_MODULE.ADMIN_EXTENSIONS_MOBILE:
                case USROPT_MODULE.ADMIN_EXTENSIONS_OUTLOOKADDIN:
                case USROPT_MODULE.ADMIN_EXTENSIONS_LINKEDIN:
                case USROPT_MODULE.ADMIN_EXTENSIONS_SIRENE:
                case USROPT_MODULE.ADMIN_EXTENSIONS_SMS:
                case USROPT_MODULE.ADMIN_EXTENSIONS_CTI:
                case USROPT_MODULE.ADMIN_EXTENSIONS_API:
                case USROPT_MODULE.ADMIN_EXTENSIONS_EXTERNALMAILING:
                case USROPT_MODULE.ADMIN_EXTENSIONS_VCARD:
                case USROPT_MODULE.ADMIN_EXTENSIONS_SNAPSHOT:
                case USROPT_MODULE.ADMIN_EXTENSIONS_EMAILING:
                case USROPT_MODULE.ADMIN_EXTENSIONS_GRID:
                case USROPT_MODULE.ADMIN_EXTENSIONS_NOTIFICATIONS:
                case USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO_OFFICE365:
                case USROPT_MODULE.ADMIN_EXTENSIONS_POWERBI:
                case USROPT_MODULE.ADMIN_EXTENSIONS_EXTRANET:
                case USROPT_MODULE.ADMIN_EXTENSIONS_MAIL_STATUS_VERIFICATION:
                //SHA : tâche #1 873
                case USROPT_MODULE.ADMIN_EXTENSIONS_ADVANCED_FORM:
                case USROPT_MODULE.ADMIN_EXTENSIONS_DEDICATED_IP:
                case USROPT_MODULE.ADMIN_DASHBOARD:
                case USROPT_MODULE.ADMIN:
                case USROPT_MODULE.PREFERENCES_THEME:
                case USROPT_MODULE.UNDEFINED:
                default:
                    return GetModuleLabel(module, pref);
            }
        }

        /// <summary>
        /// Retourne le sous-libellé descriptif correspondant à un module/options utilisateur pour les tuiles
        /// </summary>
        /// <param name="module">Module concerné</param>
        /// <param name="pref"></param>
        /// <returns>
        /// Libellé du module
        /// </returns>
        public static string GetModuleSubLabel(USROPT_MODULE module, ePref pref)
        {
            switch (module)
            {
                // Administration
                case USROPT_MODULE.ADMIN_GENERAL:
                    return eResApp.GetRes(pref, 7179); // Paramètres généraux
                case USROPT_MODULE.ADMIN_ACCESS:
                    return eResApp.GetRes(pref, 7180); // Utilisateurs, groupes et sécurité
                case USROPT_MODULE.ADMIN_ACCESS_USERGROUPS:
                    return eResApp.GetRes(pref, 7176); // Groupes et utilisateurs
                case USROPT_MODULE.ADMIN_ACCESS_SECURITY:
                    return eResApp.GetRes(pref, 206); // Sécurité
                case USROPT_MODULE.ADMIN_ACCESS_PREF:
                    return eResApp.GetRes(pref, 7708); // Préférences
                case USROPT_MODULE.ADMIN_TABS:
                    return eResApp.GetRes(pref, 7181); // Rubriques et signets
                case USROPT_MODULE.ADMIN_HOME:
                    return eResApp.GetRes(pref, 21); // Administration
                case USROPT_MODULE.ADMIN_EXTENSIONS:
                case USROPT_MODULE.ADMIN_EXTENSIONS_FROMSTORE:
                    return eResApp.GetRes(pref, 7177); // Fonctionnalités optionnelles : eResApp.GetRes(pref, 7182); devient Extensions. GLA
                case USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO:
                    return eResApp.GetRes(pref, 7832); // Paramétrage des outils de synchronisation entre Eudonet et d'autres systèmes (tels que Microsoft Exchange ou Outlook) - TOCHECKRES
                case USROPT_MODULE.ADMIN_EXTENSIONS_MOBILE:
                    return eResApp.GetRes(pref, 7833); // Correspondance entre les rubriques de votre base Eudonet et l'application mobile Eudonet XRM - TOCHECKRES
                case USROPT_MODULE.ADMIN_EXTENSIONS_OUTLOOKADDIN:
                    return eResApp.GetRes(pref, 2031); // Correspondance entre les rubriques de votre base Eudonet et l'add-in Outlook - TOCHECKRES
                case USROPT_MODULE.ADMIN_EXTENSIONS_LINKEDIN:
                    return eResApp.GetRes(pref, 3046);
                case USROPT_MODULE.ADMIN_EXTENSIONS_SMS:
                    return eResApp.GetRes(pref, 7758); // Configuration des paramètres d'envoi de SMS
                case USROPT_MODULE.ADMIN_EXTENSIONS_CTI:
                    return eResApp.GetRes(pref, 7787); // Couplage Téléphonie Informatique
                case USROPT_MODULE.ADMIN_EXTENSIONS_API:
                    return eResApp.GetRes(pref, 7784); // Configuration des paramètres de l'API
                case USROPT_MODULE.ADMIN_EXTENSIONS_EXTERNALMAILING:
                    return eResApp.GetRes(pref, 7791);
                case USROPT_MODULE.ADMIN_EXTENSIONS_VCARD:
                    return eResApp.GetRes(pref, 7831); // Configuration des mappings de la vCard
                case USROPT_MODULE.ADMIN_EXTENSIONS_SNAPSHOT:
                    return eResApp.GetRes(pref, 7890); // Réalisation d'une base de recette/tests à partir de cette base - TOCHECKRES
                case USROPT_MODULE.ADMIN_EXTENSIONS_EMAILING:
                    return eResApp.GetRes(pref, 7909);
                case USROPT_MODULE.ADMIN_EXTENSIONS_GRID:
                    return eResApp.GetRes(pref, 8066);
                case USROPT_MODULE.ADMIN_EXTENSIONS_NOTIFICATIONS:
                    return eResApp.GetRes(pref, 7366); // Liste des notifications - TOCHECKRES
                case USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO_OFFICE365:
                    return eResApp.GetRes(pref, 7832);// Paramétrage des outils de synchronisation entre Eudonet et d'autres systèmes (tels que Microsoft Exchange ou Outlook) - TOCHECKRES
                case USROPT_MODULE.ADMIN_EXTENSIONS_SIRENE:
                    return eResApp.GetRes(pref, 8545); // Paramétrage du référentiel Sirene
                case USROPT_MODULE.ADMIN_EXTENSIONS_POWERBI:
                    return eResApp.GetRes(pref, 1902); // Paramétrage du module Power BI
                case USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_BUSINESSSOFT:
                    return eResApp.GetRes(pref, 2161).Replace("<SOFTWARENAME>", "Business Soft"); // Paramétrage de l'interface comptable avec <SOFTWARENAME> - TOCHECKRES
                case USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_CEGID:
                    return eResApp.GetRes(pref, 2161).Replace("<SOFTWARENAME>", "Cegid"); // Paramétrage de l'interface comptable avec <SOFTWARENAME> - TOCHECKRES
                case USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_SAGE:
                    return eResApp.GetRes(pref, 2161).Replace("<SOFTWARENAME>", "Sage"); // Paramétrage de l'interface comptable avec <SOFTWARENAME> - TOCHECKRES
                case USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_EBP:
                    return eResApp.GetRes(pref, 2161).Replace("<SOFTWARENAME>", "EBP"); // Paramétrage de l'interface comptable avec <SOFTWARENAME> - TOCHECKRES
                case USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_SIGMA:
                    return eResApp.GetRes(pref, 2161).Replace("<SOFTWARENAME>", "Sigma"); // Paramétrage de l'interface comptable avec <SOFTWARENAME> - TOCHECKRES
                case USROPT_MODULE.ADMIN_EXTENSIONS_IN_UBIFLOW:
                    return eResApp.GetRes(pref, 2167); // Paramétrage du module Demandes Entrantes Ubiflow
                case USROPT_MODULE.ADMIN_EXTENSIONS_IN_HBS:
                    return eResApp.GetRes(pref, 2168); // Paramétrage du module Demandes Entrantes HBS
                case USROPT_MODULE.ADMIN_EXTENSIONS_DOCUSIGN:
                    return eResApp.GetRes(pref, 2169); // Paramétrage du module Signature Electronique DocuSign
                case USROPT_MODULE.ADMIN_EXTENSIONS_SMS_NETMESSAGE:
                    return eResApp.GetRes(pref, 2170); // Paramétrage du module SMS Netmessage
                case USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO_EXCHANGE2016ONPREMISE:
                    return eResApp.GetRes(pref, 2171); // Paramétrage du module Synchronisation Exchange 2016 on Premise
                case USROPT_MODULE.ADMIN_EXTENSIONS_ADVANCED_FORM:
                    return eResApp.GetRes(pref, 2400).Replace("<EXTENSIONNAME>", eResApp.GetRes(pref, 1142) + ' ' + eResApp.GetRes(pref, 414)); // Paramétrage du module Formulaire Avancé
                case USROPT_MODULE.ADMIN_EXTENSIONS_DEDICATED_IP:
                    return eResApp.GetRes(pref, 2400).Replace("<EXTENSIONNAME>", eResApp.GetRes(pref, 2808)); // Paramétrage du module IP Dédié
                case USROPT_MODULE.ADMIN_EXTENSIONS_MAIL_STATUS_VERIFICATION:
                    return eResApp.GetRes(pref, 2400).Replace("<EXTENSIONNAME>", eResApp.GetRes(pref, 2975));
                case USROPT_MODULE.ADMIN_EXTENSIONS_WORLDLINE_PAYMENT:
                    return eResApp.GetRes(pref, 2400).Replace("<EXTENSIONNAME>", eResApp.GetRes(pref, 8784)); // Paramétrage du module Paiement Worldline
                case USROPT_MODULE.ADMIN_DASHBOARD:
                    return eResApp.GetRes(pref, 7183); // Volumétrie et performance
                case USROPT_MODULE.ADMIN_ORM:
                    return eResApp.GetRes(pref, 2950);
                default:
                    return String.Empty;
            }
        }

        /// <summary>
        /// Retourne l'info-bulle correspondant à un module/option utilisateur
        /// </summary>
        /// <param name="module">Module concerné</param>
        /// <param name="pref"></param>
        /// <returns>
        /// Libellé du module
        /// </returns>
        public static string GetModuleTooltip(USROPT_MODULE module, ePref pref)
        {
            switch (module)
            {
                case USROPT_MODULE.HOME:
                    return eResApp.GetRes(pref, 7981);
                // Administration
                case USROPT_MODULE.ADMIN:
                    return eResApp.GetRes(pref, 7204); // "Cliquer pour retourner à la page principale de l’administration";
                case USROPT_MODULE.ADMIN_GENERAL:
                case USROPT_MODULE.ADMIN_GENERAL_CONFIGADV:
                case USROPT_MODULE.ADMIN_GENERAL_LOCALIZATION:
                case USROPT_MODULE.ADMIN_GENERAL_LOGO:
                case USROPT_MODULE.ADMIN_GENERAL_NAVIGATION:
                case USROPT_MODULE.ADMIN_GENERAL_SUPERVISION:
                    return eResApp.GetRes(pref, 7205); // "Cliquer pour administrer les paramètres généraux de votre Eudonet, tels que son nom, son logo ou le comportement lors de la navigation";
                case USROPT_MODULE.ADMIN_ACCESS:
                    return eResApp.GetRes(pref, 7206); // "Cliquer pour administrer les utilisateurs, les groupes et leurs préférences d’affichage ainsi que les paramètres d’accès à votre Eudonet tels que la stratégie de mots de passe ou les adresses IP autorisées";
                case USROPT_MODULE.ADMIN_ACCESS_USERGROUPS:
                    return eResApp.GetRes(pref, 7207); // "Cliquer pour administrer les utilisateurs, les groupes et leurs préférences d’affichage";
                case USROPT_MODULE.ADMIN_ACCESS_SECURITY:
                    return eResApp.GetRes(pref, 7208); // "Cliquer pour administrer les paramètres d’accès à votre Eudonet tels que la stratégie de mots de passe ou les adresses IP autorisées";
                case USROPT_MODULE.ADMIN_ACCESS_PREF:
                    return String.Empty;
                case USROPT_MODULE.ADMIN_TABS:
                    return eResApp.GetRes(pref, 7209); // "Cliquer pour administrer l’ensemble des paramètres des onglets, des signets et des rubriques de votre Eudonet";
                case USROPT_MODULE.ADMIN_HOME:
                case USROPT_MODULE.ADMIN_HOME_V7_HOMEPAGES:
                    return eResApp.GetRes(pref, 7210); // "Cliquer pour administrer les pages d’accueil et les liens favoris de votre Eudonet"; 
                case USROPT_MODULE.ADMIN_HOME_XRM_HOMEPAGES:
                    return eResApp.GetRes(pref, 8057); // Cliquer pour administrer les grilles XRM
                case USROPT_MODULE.ADMIN_HOME_EXPRESS_MESSAGE:
                    return eResApp.GetRes(pref, 7558); // "Cliquer pour administrer l’ensemble des paramètres des messages express de votre Eudonet"; 
                case USROPT_MODULE.ADMIN_EXTENSIONS:
                case USROPT_MODULE.ADMIN_EXTENSIONS_FROMSTORE:
                case USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO:
                case USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO_OFFICE365:
                case USROPT_MODULE.ADMIN_EXTENSIONS_EMAILING:
                case USROPT_MODULE.ADMIN_EXTENSIONS_MOBILE:
                case USROPT_MODULE.ADMIN_EXTENSIONS_OUTLOOKADDIN:
                case USROPT_MODULE.ADMIN_EXTENSIONS_LINKEDIN:
                case USROPT_MODULE.ADMIN_EXTENSIONS_SIRENE:
                case USROPT_MODULE.ADMIN_EXTENSIONS_SMS:
                case USROPT_MODULE.ADMIN_EXTENSIONS_CTI:
                case USROPT_MODULE.ADMIN_EXTENSIONS_API:
                case USROPT_MODULE.ADMIN_EXTENSIONS_EXTERNALMAILING:
                case USROPT_MODULE.ADMIN_EXTENSIONS_VCARD:
                case USROPT_MODULE.ADMIN_EXTENSIONS_SNAPSHOT:
                case USROPT_MODULE.ADMIN_EXTENSIONS_GRID:
                case USROPT_MODULE.ADMIN_EXTENSIONS_NOTIFICATIONS:
                case USROPT_MODULE.ADMIN_EXTENSIONS_POWERBI:
                case USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_BUSINESSSOFT:
                case USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_CEGID:
                case USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_SAGE:
                case USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_EBP:
                case USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_SIGMA:
                case USROPT_MODULE.ADMIN_EXTENSIONS_IN_UBIFLOW:
                case USROPT_MODULE.ADMIN_EXTENSIONS_IN_HBS:
                case USROPT_MODULE.ADMIN_EXTENSIONS_DOCUSIGN:
                case USROPT_MODULE.ADMIN_EXTENSIONS_SMS_NETMESSAGE:
                case USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO_EXCHANGE2016ONPREMISE:
                case USROPT_MODULE.ADMIN_EXTENSIONS_EXTRANET:
                //SHA : tâche #1 873
                case USROPT_MODULE.ADMIN_EXTENSIONS_ADVANCED_FORM:
                case USROPT_MODULE.ADMIN_EXTENSIONS_DEDICATED_IP:
                case USROPT_MODULE.ADMIN_EXTENSIONS_MAIL_STATUS_VERIFICATION:
                case USROPT_MODULE.ADMIN_EXTENSIONS_WORLDLINE_PAYMENT:
                    return eResApp.GetRes(pref, 7211); // "Cliquer pour administrer les extensions"; // TOCHECK SPEC
                case USROPT_MODULE.ADMIN_DASHBOARD:
                    return eResApp.GetRes(pref, 7212); // "Cliquer pour accéder aux outils permettant de mesurer la volumétrie et les performances de votre base"; // TOCHECK SPEC
                case USROPT_MODULE.ADMIN_ORM:
                    return "Cliquer pour accéder aux paramètres de votre ORM"; ;
                default:
                    return String.Empty;
            }
        }

        /// <summary>
        /// Retourne le libellé correspondant à un module/options utilisateur dont le nom est passé en chaîne
        /// </summary>
        /// <param name="moduleName">Chaîne (constante) désignant le module concerné</param>
        /// <param name="pref"></param>
        /// <returns>
        /// Libellé du module
        /// </returns>
        public static string GetModuleLabel(string moduleName, ePref pref)
        {
            USROPT_MODULE module = USROPT_MODULE.UNDEFINED;

            Enum.TryParse(moduleName, out module);

            return GetModuleLabel(module, pref);
        }

        /// <summary>
        /// Retourne le libellé correspondant à un module/options utilisateur pour le menu de droite dont le nom est passé en chaîne
        /// </summary>
        /// <param name="moduleName">Chaîne (constante) désignant le module concerné</param>
        /// <param name="pref"></param>
        /// <returns>
        /// Libellé du module
        /// </returns>
        public static string GetModuleRightMenuLabel(string moduleName, ePref pref)
        {
            USROPT_MODULE module = USROPT_MODULE.UNDEFINED;

            Enum.TryParse(moduleName, out module);

            return GetModuleRightMenuLabel(module, pref);
        }

        /// <summary>
        /// Retourne le libellé correspondant à un module/options utilisateur pour le titre de milieu de page dont le nom est passé en chaîne
        /// </summary>
        /// <param name="moduleName">Chaîne (constante) désignant le module concerné</param>
        /// <param name="pref"></param>
        /// <returns>
        /// Libellé du module
        /// </returns>
        public static string GetModuleMiddleTitleLabel(string moduleName, ePref pref)
        {
            USROPT_MODULE module = USROPT_MODULE.UNDEFINED;

            Enum.TryParse(moduleName, out module);

            return GetModuleMiddleTitleLabel(module, pref);
        }

        /// <summary>
        /// Retourne le sous-libellé descriptif correspondant à un module/options utilisateur pour les tuiles dont le nom est passé en chaîne
        /// </summary>
        /// <param name="moduleName">Chaîne (constante) désignant le module concerné</param>
        /// <param name="pref"></param>
        /// <returns>
        /// Libellé du module
        /// </returns>
        public static string GetModuleSubLabel(string moduleName, ePref pref)
        {
            USROPT_MODULE module = USROPT_MODULE.UNDEFINED;

            Enum.TryParse(moduleName, out module);

            return GetModuleSubLabel(module, pref);
        }

        /// <summary>
        /// Retourne l'info-bulle correspondant à un module/options utilisateur dont le nom est passé en chaîne
        /// </summary>
        /// <param name="moduleName">Chaîne (constante) désignant le module concerné</param>
        /// <param name="pref"></param>
        /// <returns>
        /// Libellé du module
        /// </returns>
        public static string GetModuleTooltip(string moduleName, ePref pref)
        {
            USROPT_MODULE module = USROPT_MODULE.UNDEFINED;

            Enum.TryParse(moduleName, out module);

            return GetModuleTooltip(module, pref);
        }

        /// <summary>
        /// Retourne la classe d'icône (CSS) correspondant à un module/option utilisateur
        /// </summary>
        /// <param name="module">Module concerné</param>
        /// <returns>Classe CSS du module, à préfixer avec icon-</returns>
        public static string GetModuleIcon(USROPT_MODULE module)
        {
            switch (module)
            {
                // Options utilisateur / "Mon Eudonet"
                case USROPT_MODULE.MAIN:
                    return "buzz"; // l'icône "buzz" n'est pas visible sur fond gris
                case USROPT_MODULE.HOME:
                    return "item_rem";
                case USROPT_MODULE.PREFERENCES:
                case USROPT_MODULE.PREFERENCES_THEME:
                case USROPT_MODULE.PREFERENCES_LANGUAGE:
                case USROPT_MODULE.PREFERENCES_PROFILE:
                case USROPT_MODULE.PREFERENCES_SIGNATURE:
                case USROPT_MODULE.PREFERENCES_PASSWORD:
                case USROPT_MODULE.PREFERENCES_MEMO:
                    return "mes_prefs";
                case USROPT_MODULE.ADVANCED:
                    return "mes_options";
                // Administration
                case USROPT_MODULE.ADMIN_GENERAL:
                    return "sliders";
                case USROPT_MODULE.ADMIN_ACCESS:
                    return "lock2";
                case USROPT_MODULE.ADMIN_ACCESS_USERGROUPS:
                    return "users";
                case USROPT_MODULE.ADMIN_ACCESS_SECURITY:
                    return "lock2";
                case USROPT_MODULE.ADMIN_ACCESS_PREF:
                    return "users";
                case USROPT_MODULE.ADMIN_TABS:
                    return "sitemap"; // également possible : th-large
                case USROPT_MODULE.ADMIN_HOME:
                    return "home";
                case USROPT_MODULE.ADMIN_EXTENSIONS:
                case USROPT_MODULE.ADMIN_EXTENSIONS_FROMSTORE:
                    return "cloud-download";
                case USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO:
                    return "sync";
                case USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO_OFFICE365:
                    return "sync";
                case USROPT_MODULE.ADMIN_EXTENSIONS_MOBILE:
                    return "mobile";
                case USROPT_MODULE.ADMIN_EXTENSIONS_SIRENE:
                    return "sirene";
                case USROPT_MODULE.ADMIN_EXTENSIONS_SMS:
                    return "sms";
                case USROPT_MODULE.ADMIN_EXTENSIONS_CTI:
                    return "phone3";
                case USROPT_MODULE.ADMIN_EXTENSIONS_API:
                    return "file-code-o";
                case USROPT_MODULE.ADMIN_EXTENSIONS_EXTERNALMAILING:
                    return "database2";
                case USROPT_MODULE.ADMIN_EXTENSIONS_VCARD:
                    return "vcard";
                case USROPT_MODULE.ADMIN_EXTENSIONS_SNAPSHOT:
                    return "camera";
                case USROPT_MODULE.ADMIN_EXTENSIONS_EMAILING:
                    return "envelope-o";
                case USROPT_MODULE.ADMIN_EXTENSIONS_GRID:
                    return "envelope-o";
                case USROPT_MODULE.ADMIN_EXTENSIONS_OUTLOOKADDIN:
                    return "envelope-o";
                case USROPT_MODULE.ADMIN_EXTENSIONS_NOTIFICATIONS:
                    return "bell";
                case USROPT_MODULE.ADMIN_DASHBOARD:
                case USROPT_MODULE.ADMIN_DASHBOARD_RGPD:
                case USROPT_MODULE.ADMIN_DASHBOARD_RGPDTREATMENTLOG:
                    return "dashboard"; // également possible : heartbeat
                case USROPT_MODULE.ADMIN_EXTENSIONS_POWERBI:
                    return "power-bi";
                case USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_BUSINESSSOFT:
                case USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_CEGID:
                case USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_SAGE:
                case USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_EBP:
                case USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_SIGMA:
                    return "stats";
                case USROPT_MODULE.ADMIN_EXTENSIONS_IN_UBIFLOW:
                    return "arrow-circle-o-down"; // TOCHECK
                case USROPT_MODULE.ADMIN_EXTENSIONS_IN_HBS:
                    return "arrow-circle-o-down"; // TOCHECK
                case USROPT_MODULE.ADMIN_EXTENSIONS_DOCUSIGN:
                    return "quill"; // TOCHECK
                case USROPT_MODULE.ADMIN_EXTENSIONS_SMS_NETMESSAGE:
                    return "sms";
                case USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO_EXCHANGE2016ONPREMISE:
                    return "exchange";
                case USROPT_MODULE.ADMIN_EXTENSIONS_EXTRANET:
                    return "extranet";
                //SHA : tâche #1 873
                case USROPT_MODULE.ADMIN_EXTENSIONS_ADVANCED_FORM:
                    return "advanced-form";
                case USROPT_MODULE.ADMIN_EXTENSIONS_DEDICATED_IP:
                    return "dedicated-ip";
                case USROPT_MODULE.ADMIN_EXTENSIONS_WORLDLINE_PAYMENT:
                    return "worldline-payment";
                case USROPT_MODULE.ADMIN:
                case USROPT_MODULE.UNDEFINED:
                default:
                    return "administrafion"; // (C) PNO...
            }
        }

        /// <summary>
        /// Retourne le module parent
        /// </summary>
        /// <param name="module">Module concerné</param>
        /// <param name="getRootParentModule">Si passé à true, renvoie le module parent racine et non le parent immédiat (exemple si true dans le cas d'ADMIN_ACCESS_USERGROUPS : renvoie ADMIN au lieu de ADMIN_ACCESS)</param>
        /// <returns>Le module parent</returns>
        public static USROPT_MODULE GetModuleParent(USROPT_MODULE module, bool getRootParentModule)
        {
            USROPT_MODULE parentModule = USROPT_MODULE.UNDEFINED;

            switch (module)
            {
                // Options utilisateur / "Mon Eudonet"
                case USROPT_MODULE.MAIN:
                case USROPT_MODULE.HOME:
                case USROPT_MODULE.PREFERENCES:
                case USROPT_MODULE.ADVANCED:
                    parentModule = USROPT_MODULE.MAIN;
                    break;
                case USROPT_MODULE.PREFERENCES_THEME:
                case USROPT_MODULE.PREFERENCES_FONTSIZE:
                case USROPT_MODULE.PREFERENCES_MRUMODE:
                case USROPT_MODULE.PREFERENCES_LANGUAGE:
                case USROPT_MODULE.PREFERENCES_PROFILE:
                case USROPT_MODULE.PREFERENCES_SIGNATURE:
                case USROPT_MODULE.PREFERENCES_MEMO:
                case USROPT_MODULE.PREFERENCES_PASSWORD:
                    parentModule = USROPT_MODULE.PREFERENCES;
                    break;
                case USROPT_MODULE.ADVANCED_EXPORT:
                case USROPT_MODULE.ADVANCED_PLANNING:
                    parentModule = USROPT_MODULE.ADVANCED;
                    break;
                // Administration
                case USROPT_MODULE.ADMIN_GENERAL:
                case USROPT_MODULE.ADMIN_ACCESS:
                case USROPT_MODULE.ADMIN_TABS:
                case USROPT_MODULE.ADMIN_HOME:
                case USROPT_MODULE.ADMIN_EXTENSIONS:
                case USROPT_MODULE.ADMIN_DASHBOARD:
                    parentModule = USROPT_MODULE.ADMIN;
                    break;
                case USROPT_MODULE.ADMIN_DASHBOARD_RGPD:
                case USROPT_MODULE.ADMIN_DASHBOARD_RGPDTREATMENTLOG:
                    parentModule = USROPT_MODULE.ADMIN_DASHBOARD;
                    break;
                case USROPT_MODULE.ADMIN_TAB_GRID:
                    parentModule = USROPT_MODULE.ADMIN_TABS;
                    break;
                case USROPT_MODULE.ADMIN_TAB:
                    parentModule = USROPT_MODULE.ADMIN_TABS;
                    break;

                case USROPT_MODULE.ADMIN_TAB_USER:
                    parentModule = USROPT_MODULE.ADMIN_ACCESS_USERGROUPS;
                    break;
                case USROPT_MODULE.ADMIN_ACCESS_USERGROUPS:
                case USROPT_MODULE.ADMIN_ACCESS_SECURITY:
                case USROPT_MODULE.ADMIN_ACCESS_PREF:
                    parentModule = USROPT_MODULE.ADMIN_ACCESS;
                    break;
                case USROPT_MODULE.ADMIN_GENERAL_LOGO:
                case USROPT_MODULE.ADMIN_GENERAL_NAVIGATION:
                case USROPT_MODULE.ADMIN_GENERAL_SUPERVISION:
                case USROPT_MODULE.ADMIN_GENERAL_LOCALIZATION:
                case USROPT_MODULE.ADMIN_GENERAL_CONFIGADV:
                    parentModule = USROPT_MODULE.ADMIN_GENERAL;
                    break;
                case USROPT_MODULE.ADMIN_HOME_V7_HOMEPAGES:
                case USROPT_MODULE.ADMIN_HOME_XRM_HOMEPAGES:
                case USROPT_MODULE.ADMIN_HOME_EXPRESS_MESSAGE:
                    parentModule = USROPT_MODULE.ADMIN_HOME;
                    break;
                case USROPT_MODULE.ADMIN_EXTENSIONS_FROMSTORE:
                case USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO:
                case USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO_OFFICE365:
                case USROPT_MODULE.ADMIN_EXTENSIONS_MOBILE:
                case USROPT_MODULE.ADMIN_EXTENSIONS_OUTLOOKADDIN:
                case USROPT_MODULE.ADMIN_EXTENSIONS_LINKEDIN:
                case USROPT_MODULE.ADMIN_EXTENSIONS_SIRENE:
                case USROPT_MODULE.ADMIN_EXTENSIONS_SMS:
                case USROPT_MODULE.ADMIN_EXTENSIONS_CTI:
                case USROPT_MODULE.ADMIN_EXTENSIONS_API:
                case USROPT_MODULE.ADMIN_EXTENSIONS_EXTERNALMAILING:
                case USROPT_MODULE.ADMIN_EXTENSIONS_VCARD:
                case USROPT_MODULE.ADMIN_EXTENSIONS_SNAPSHOT:
                case USROPT_MODULE.ADMIN_EXTENSIONS_EMAILING:
                case USROPT_MODULE.ADMIN_EXTENSIONS_GRID:
                case USROPT_MODULE.ADMIN_EXTENSIONS_NOTIFICATIONS:
                case USROPT_MODULE.ADMIN_EXTENSIONS_CARTO:
                case USROPT_MODULE.ADMIN_EXTENSIONS_POWERBI:
                case USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_BUSINESSSOFT:
                case USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_CEGID:
                case USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_SAGE:
                case USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_EBP:
                case USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_SIGMA:
                case USROPT_MODULE.ADMIN_EXTENSIONS_IN_UBIFLOW:
                case USROPT_MODULE.ADMIN_EXTENSIONS_IN_HBS:
                case USROPT_MODULE.ADMIN_EXTENSIONS_DOCUSIGN:
                case USROPT_MODULE.ADMIN_EXTENSIONS_SMS_NETMESSAGE:
                case USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO_EXCHANGE2016ONPREMISE:
                case USROPT_MODULE.ADMIN_EXTENSIONS_ZAPIER:
                case USROPT_MODULE.ADMIN_EXTENSIONS_EXTRANET:
                //SHA : tâche #1 873
                case USROPT_MODULE.ADMIN_EXTENSIONS_ADVANCED_FORM:
                case USROPT_MODULE.ADMIN_EXTENSIONS_DEDICATED_IP:
                case USROPT_MODULE.ADMIN_EXTENSIONS_MAIL_STATUS_VERIFICATION:
                case USROPT_MODULE.ADMIN_EXTENSIONS_WORLDLINE_PAYMENT:
                    parentModule = USROPT_MODULE.ADMIN_EXTENSIONS;
                    break;
                default:
                    parentModule = USROPT_MODULE.ADMIN;
                    break;
            }

            if (getRootParentModule)
            {
                while (GetModuleParent(parentModule, false) != parentModule)
                    parentModule = GetModuleParent(parentModule, true);
            }

            return parentModule;
        }

        /// <summary>
        /// Retourne la classe d'icône (CSS) correspondant à un module/options utilisateur dont le nom est passé en chaîne
        /// </summary>
        /// <param name="moduleName">Module concerné</param>
        /// <returns>Classe CSS du module, à préfixer avec icon-</returns>
        public static string GetModuleIcon(string moduleName)
        {
            USROPT_MODULE module = USROPT_MODULE.UNDEFINED;

            Enum.TryParse(moduleName, out module);

            return GetModuleIcon(module);
        }

        /// <summary>
        /// Retourne la liste des modules admin enfants du module indiqué en paramètre
        /// </summary>
        /// <param name="module">Module "parent"</param>
        /// <returns>La liste des modules enfants, si existants</returns>
        public static List<USROPT_MODULE> GetModuleChildren(USROPT_MODULE module)
        {
            List<USROPT_MODULE> childModules = new List<USROPT_MODULE>();
            Dictionary<string, int> insertPositions = new Dictionary<string, int>();

            // On commence par ajouter les enfants immédiats du module demandé (ex : ADMIN > ADMIN_GENERAL, mais pas ADMIN_GENERAL_LOCALIZATION) dans l'ordre de l'Enum
            foreach (USROPT_MODULE existingModule in Enum.GetValues(typeof(USROPT_MODULE)))
            {
                if (
                    existingModule != USROPT_MODULE.MAIN &&
                    existingModule != USROPT_MODULE.PREFERENCES &&
                    existingModule != USROPT_MODULE.UNDEFINED &&
                    existingModule != module &&
                    ModuleExists(existingModule)
                )
                {
                    if (
                        existingModule.ToString().StartsWith(module.ToString()) ||
                        existingModule.ToString().StartsWith("ADMIN_") && module == USROPT_MODULE.ADMIN
                    )
                    {
                        eUserOptionsModules.USROPT_MODULE parentModule = eUserOptionsModules.GetModuleParent(existingModule, false);
                        if (parentModule == module)
                            childModules.Add(existingModule);
                    }
                }
            }

            // Puis on ajoute les éventuels enfants des modules immédiats
            // Le faire en 2 fois permet d'insérer les enfants sous leurs parents, même si l'Enum n'est pas dans le bon ordre (si les enfants ne sont pas déclarés juste après les parents)
            // Les enfants sont toutefois insérés dans l'ordre de l'Enum à leur tour
            Dictionary<USROPT_MODULE, List<USROPT_MODULE>> grandChildModules = new Dictionary<USROPT_MODULE, List<USROPT_MODULE>>();
            foreach (USROPT_MODULE childModule in childModules)
                grandChildModules[childModule] = GetModuleChildren(childModule);
            // L'insertion doit se faire dans une boucle séparée pour ne pas modifier la collection énumérée
            foreach (USROPT_MODULE grandChildModule in grandChildModules.Keys)
                childModules.InsertRange(childModules.IndexOf(grandChildModule) + 1, grandChildModules[grandChildModule]);

            // Et on retourne enfin la liste complète et ordonnée
            return childModules;
        }

        /// <summary>
        /// Retourne la fonctionnalité correspondante au module
        /// </summary>
        /// <param name="module">USROPT_MODULE</param>
        /// <returns>eConst.XrmFeature</returns>
        public static eConst.XrmFeature GetModuleFeature(USROPT_MODULE module)
        {
            switch (module)
            {
                // Administration
                case USROPT_MODULE.ADMIN_GENERAL:
                    return eConst.XrmFeature.AdminParameters;
                case USROPT_MODULE.ADMIN_GENERAL_CONFIGADV:
                case USROPT_MODULE.ADMIN_ORM:
                    return eConst.XrmFeature.AdminParameters_Advanced;
                case USROPT_MODULE.ADMIN_GENERAL_LOCALIZATION:
                    return eConst.XrmFeature.AdminParameters_Localization;
                case USROPT_MODULE.ADMIN_GENERAL_LOGO:
                    return eConst.XrmFeature.AdminParameters_Logo;
                case USROPT_MODULE.ADMIN_GENERAL_NAVIGATION:
                    return eConst.XrmFeature.AdminParameters_Navigation;
                case USROPT_MODULE.ADMIN_GENERAL_SUPERVISION:
                    return eConst.XrmFeature.AdminParameters_Supervision;
                case USROPT_MODULE.ADMIN_ACCESS:
                    return eConst.XrmFeature.AdminAccess;
                case USROPT_MODULE.ADMIN_ACCESS_USERGROUPS:
                    return eConst.XrmFeature.AdminAccess_GroupsAndUsers;
                case USROPT_MODULE.ADMIN_ACCESS_SECURITY:
                    return eConst.XrmFeature.AdminAccess_Security;
                case USROPT_MODULE.ADMIN_ACCESS_PREF:
                    return eConst.XrmFeature.AdminAccess_Preferences;
                case USROPT_MODULE.ADMIN_TABS:
                    return eConst.XrmFeature.AdminTabs;
                case USROPT_MODULE.ADMIN_HOME:
                case USROPT_MODULE.ADMIN_HOME_V7_HOMEPAGES:
                case USROPT_MODULE.ADMIN_HOME_XRM_HOMEPAGES:
                case USROPT_MODULE.ADMIN_HOME_EXPRESS_MESSAGE:
                    return eConst.XrmFeature.AdminHome;
                case USROPT_MODULE.ADMIN_EXTENSIONS:
                    return eConst.XrmFeature.AdminExtensions;
                case USROPT_MODULE.ADMIN_EXTENSIONS_FROMSTORE:
                case USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO:
                case USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO_OFFICE365:
                case USROPT_MODULE.ADMIN_EXTENSIONS_EMAILING:
                case USROPT_MODULE.ADMIN_EXTENSIONS_MOBILE:
                case USROPT_MODULE.ADMIN_EXTENSIONS_OUTLOOKADDIN:
                case USROPT_MODULE.ADMIN_EXTENSIONS_LINKEDIN:
                case USROPT_MODULE.ADMIN_EXTENSIONS_SIRENE:
                case USROPT_MODULE.ADMIN_EXTENSIONS_SMS:
                case USROPT_MODULE.ADMIN_EXTENSIONS_CTI:
                case USROPT_MODULE.ADMIN_EXTENSIONS_API:
                case USROPT_MODULE.ADMIN_EXTENSIONS_EXTERNALMAILING:
                case USROPT_MODULE.ADMIN_EXTENSIONS_VCARD:
                case USROPT_MODULE.ADMIN_EXTENSIONS_SNAPSHOT:
                case USROPT_MODULE.ADMIN_EXTENSIONS_GRID:
                case USROPT_MODULE.ADMIN_EXTENSIONS_NOTIFICATIONS:
                case USROPT_MODULE.ADMIN_EXTENSIONS_POWERBI:
                case USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_BUSINESSSOFT:
                case USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_CEGID:
                case USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_SAGE:
                case USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_EBP:
                case USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_SIGMA:
                case USROPT_MODULE.ADMIN_EXTENSIONS_IN_UBIFLOW:
                case USROPT_MODULE.ADMIN_EXTENSIONS_IN_HBS:
                case USROPT_MODULE.ADMIN_EXTENSIONS_DOCUSIGN:
                case USROPT_MODULE.ADMIN_EXTENSIONS_SMS_NETMESSAGE:
                case USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO_EXCHANGE2016ONPREMISE:
                case USROPT_MODULE.ADMIN_EXTENSIONS_EXTRANET:
                case USROPT_MODULE.ADMIN_EXTENSIONS_ADVANCED_FORM:
                case USROPT_MODULE.ADMIN_EXTENSIONS_DEDICATED_IP:
                case USROPT_MODULE.ADMIN_EXTENSIONS_MAIL_STATUS_VERIFICATION:
                case USROPT_MODULE.ADMIN_EXTENSIONS_WORLDLINE_PAYMENT:
                case USROPT_MODULE.ADMIN_DASHBOARD:
                    return eConst.XrmFeature.AdminDashboard;
                case USROPT_MODULE.ADMIN_DASHBOARD_RGPD:
                case USROPT_MODULE.ADMIN_DASHBOARD_RGPDTREATMENTLOG:
                    return eConst.XrmFeature.AdminRGPD;
                default:
                    return eConst.XrmFeature.Undefined;
            }
        }

        /// <summary>
        /// Indique si les modules enfants d'un module peuvent être référencé dans la navbar
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        public static bool ModuleChildrenCanAppearInNavBar(USROPT_MODULE module)
        {
            switch (module)
            {
                case USROPT_MODULE.ADMIN_EXTENSIONS:
                    return false;
                default:
                    return true;
            }
        }

        /// <summary>
        /// Indique si les modules enfants d'un module peuvent être référencé dans le menu
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        public static bool ModuleChildrenCanAppearInMenu(USROPT_MODULE module)
        {
            switch (module)
            {
                case USROPT_MODULE.ADMIN_EXTENSIONS:
                    return false;
                default:
                    return true;
            }
        }

        /// <summary>
        /// Défini l'ordre d'affuchage des modules
        /// Define display order of module
        /// </summary>
        private static int GetModuleOrder(USROPT_MODULE module)
        {
            switch (module)
            {
                case USROPT_MODULE.PREFERENCES_THEME:
                    return GetModuleOrder(USROPT_MODULE.PREFERENCES_LANGUAGE) - 1;
                case USROPT_MODULE.PREFERENCES_PROFILE:
                    return GetModuleOrder(USROPT_MODULE.PREFERENCES_SIGNATURE) - 1;
                default:
                    return (int)module;
            }
        }

        /// <summary>
        /// Retourne la liste des modules ordonnée
        /// </summary>
        /// <param name="lstModules">Liste des modules</param>
        /// <returns></returns>
        public static List<USROPT_MODULE> GetOrderedModuleList(List<USROPT_MODULE> lstModules)
        {
            return lstModules.OrderBy(m => GetModuleOrder(m)).ToList();
        }
    }
}