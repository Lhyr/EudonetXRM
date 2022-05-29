const FieldType = {
    /// <summary>Aucune correspondance </summary>
    Undefined: 0,
    /// <summary>Caractère</summary>
    Character: 1,
    /// <summary>Catalogue </summary>
    Catalog: 2,
    /// <summary>Addresse email </summary>
    MailAddress: 3,
    /// <summary>Téléphone </summary>
    Phone: 4,
    /// <summary>Relation </summary>
    Relation: 5,
    /// <summary>Réseau sociaux </summary>
    SocialNetwork: 6,
    /// <summary>Géolocalisation </summary>
    Geolocation: 7,
    /// <summary>Alias </summary>
    Alias: 8,
    /// <summary>Alias d'en-tête (Champs de liaison système placé dans le corps de la page) </summary>
    AliasRelation: 9,
    /// <summary>Bouton </summary>
    Button: 10,
    /// <summary>Case à cocher </summary>
    Logic: 11,
    /// <summary>Compteur automatique </summary>
    AutoCount: 13,
    /// <summary>Date </summary>
    Date: 14,
    /// <summary>Etiquette </summary>
    Label: 15,
    /// <summary>Séparateur de page </summary>
    Separator: 16,
    /// <summary>Memo </summary>
    Memo: 17,
    /// <summary>Numérique </summary>
    Numeric: 18,
    /// <summary>Monetaire </summary>
    Money: 19,
    /// <summary>Image </summary>
    Image: 20,
    /// <summary>Graphique </summary>
    Chart: 21,
    /// <summary>Page Web (Iframe) </summary>
    WebPage: 22,
    /// <summary>Lien web </summary>
    HyperLink: 23,
    /// <summary>Fichier </summary>
    File: 24,
    /// <summary>Utilisateur </summary>
    User: 25,
    /// <summary>Mot de passe </summary>
    Password: 26,
    /// <summary>PJ</summary>
    PJ: 27,
    /// <summary>Type caché.</summary>
    Hidden: 28,
    /// <summary>varbinary et toutes ces sortes de choses.</summary>
    Binary: 29,

};

const FolderType = {
    /// <summary>ANNEXES</summary>
    ANNEXES: 0,
    /// <summary>CUSTOM</summary>
    CUSTOM: 1,
    /// <summary>MODELES</summary>
    MODELES: 2,
    /// <summary>DATASOURCE</summary>
    DATASOURCE: 3,
    /// <summary>REPORTS</summary>
    REPORTS: 4,
    /// <summary>FOLDERS</summary>
    FOLDERS: 5,
    /// <summary>HOMEPAGE</summary>
    HOMEPAGE: 6,
    /// <summary>IMAGES</summary>
    IMAGES: 7,
    /// <summary>UPLOAD</summary>
    UPLOAD: 8,
    /// <summary>FILES</summary>
    FILES: 9,
    /// <summary>CUSTOM/BANNER</summary>
    CUSTOM_BANNER: 10,
    /// <summary>AVATARS</summary>
    AVATARS: 11,
    /// <summary>VCARD</summary>
    VCARD: 12,
    /// <summary>Racine de DATAS</summary>
    ROOT: 13,
    /// <summary>IMAGES/Notifications</summary>
    NOTIFICATION: 14,
    /// <summary>WIDGET</summary>
    WIDGET: 15,
    /// <summary>IMPORT</summary>
    IMPORT: 16,
    /// <summary>MACROS PUBLIPOSTAGE</summary>
    MACROS: 17
};

/**
 * Pour le focusInput, on a une condition bizarre sur 3 booléen.
 * Convertie en Enum. G.L.
 * */
const PropType = {
    Defaut: 0,
    Assistant: 1,
    Detail: 2,
    Liste: 3,
    Head: 4
};


/// <summary>
/// Type de Fichier Eudonet
/// </summary>
const EdnType = {
    /// <summary>Fichier Principal</summary>
    FILE_MAIN: 0,
    /// <summary>Sous-Fichier Planning</summary>
    FILE_PLANNING: 1,
    /// <summary>Sous-Fichier Standard</summary>
    FILE_STANDARD: 2,
    /// <summary>Sous-Fichier Email</summary>
    FILE_MAIL: 3,
    /// <summary>Sous-Fichier SMS</summary>
    FILE_SMS: 4,
    /// <summary>Sous-Fichier Historique</summary>
    FILE_HISTO: 5,
    /// <summary>Sous-Fichier Relation</summary>
    FILE_RELATION: 6,
    /// <summary>Sous-Fichier Message Vocal</summary>
    FILE_VOICING: 7,
    /// <summary>user</summary>
    FILE_USER: 8,
    /// <summary>group</summary>
    FILE_GROUP: 9,
    /// <summary>Adresse</summary>
    FILE_ADR: 10,
    /// <summary>PJ</summary>
    FILE_PJ: 11,
    /// <summary>Filtres</summary>
    FILE_FILTER: 12,
    /// <summary>Rapports</summary>
    FILE_REPORT: 13,
    /// <summary>Modèles de mails</summary>
    FILE_MAILTEMPLATE: 15,
    /// <summary>Signet de type page Web</summary>
    FILE_BKMWEB: 16,
    /// <summary>TODO - notifications -- On utilise un fichier de type Main</summary>
    // FILE_NOTIFICATIONS: 17,
    /// <summary>Formulaire XRM</summary>
    FILE_FORMULARXRM: 18,
    /// <summary>Onglet de type page Web</summary>
    FILE_WEBTAB: 19,
    /// <summary>Signet de type Discussion</summary>
    FILE_DISCUSSION: 20,
    /// <summary>Signet de type Cibles étendues</summary>
    FILE_TARGET: 21,
    /// <summary>Page d'accueil XRM - Grilles</summary>
    FILE_HOMEPAGE: 22,
    /// <summary>Widget XRM</summary>
    FILE_WIDGET: 23,
    /// <summary>Onglet Grille ou Signet Grille</summary>
    FILE_GRID: 24,
    /// <summary>Type plus utilisé</summary>
    FILE_OBSOLETE: 25,
    /// <summary>Modèles d'import</summary>
    FILE_IMPORTTEMPLATE: 26,
    /// <summary>Sous-Requête</summary>
    FILE_SUBQUERY: 94,
    /// <summary>Systeme (SCHEDULE...)</summary>
    FILE_SYSTEM: 99,

    /// <summary>Non défini</summary>
    FILE_UNDEFINED: 999
}

const expressFilterEnum = {
    /// <summary>Filtre express type date</summary>
    DATE: 0,
    /// <summary>Filtre express type caractere</summary>
    CARACTERE: 1,
    /// <summary>Filtre express type numerique</summary>
    NUMERIQUE: 2,
    /// <summary>Filtre express type logique</summary>
    LOGIQUE: 3
};


const Operator = {
    /// <summary>Egal à  </summary>
    OP_EQUAL: 0,
    /// <summary>Inférieur à</summary>
    OP_LESS: 1,
    /// <summary>Inférieur ou égal à </summary>
    OP_LESS_OR_EQUAL: 2,
    /// <summary>Supérieur à </summary>
    OP_GREATER: 3,
    /// <summary>Supérieur ou égal à </summary>
    OP_GREATER_OR_EQUAL: 4,
    /// <summary>Différent de </summary>
    OP_DIFFERENT: 5,
    /// <summary>Députe par </summary>
    OP_START_WITH: 6,
    /// <summary>Finit par </summary>
    OP_END_WITH: 7,
    /// <summary>Est dans la liste </summary>
    OP_IN_LIST: 8,
    /// <summary>Contient </summary>
    OP_CONTAIN: 9,
    /// <summary>Est vide </summary>
    OP_IS_EMPTY: 10,
    /// <summary>N'est pas vide </summary>
    OP_IS_NOT_EMPTY: 17
};

const EngineConfirmModes = {
    /// <summary>pas de demande de confirmation</summary>
    NONE: 0,
    /// <summary>lors de la demande de mise à jour des adresses non identique à la PM associée</summary>
    ADDRESS_CHECK: 1,
    /// <summary>lors de la demande de confirmation ou d'information sur une formule du milieu</summary>
    MIDDLE_PROC: 2,
    /// <summary>fenêtre de confirmation de suppression</summary>
    DELETE: 3,
    /// <summary>fenêtre de confirmation de suppression avec choix de suppression en cascade des PP depuis PM</summary>
    DELETE_PM_PP: 4,
    /// <summary>Confirmation de suppression sur un planning multiowner</summary>
    DELETE_PLANNING_MULTI_OWNER: 5,
    /// <summary>fenêtre de confirmation de fusion</summary>
    MERGE: 6,
    /// <summary>url de la fenêtre de confirmation de l'ORM</summary>
    ORM_CONFIRM: 7,
    /// <summary>l'ORM demande l'annulation de l'operation</summary>
    ORM_CANCEL: 8
};

const XrmCruAction =
{
    NONE: 0,
    /// <summary>Mise à jour de rubrique (avant la mise à jour en base, vérification des adresses identiques et vérification de formule de milieu)</summary>
    UPDATE: 1,
    /// <summary>Exécution après avoir validé la formule du milieu</summary>
    CHECK_MIDDLE_OK: 2,
    /// <summary>Exécution après avoir validé la mise à jour des adresses</summary>
    CHECK_ADR_OK: 3,
    /// <summary>Vérifie uniquement les formules du milieu</summary>
    CHECK_ONLY_MIDDLE: 4
};

const MsgType = {
    /// <summary>message critique</summary>
    CRITICAL: 0,
    /// <summary>demande de confirmation</summary>
    QUESTION: 1,
    /// <summary>message d'exclamation</summary>
    EXCLAMATION: 2,
    /// <summary>message d'info</summary>
    INFOS: 3,
    /// <summary>message de success</summary>
    SUCCESS: 4
};

/**
 * Les langues possibles.
 * */
const Lang = [
    "fr",
    "en",
    "de",
    "nl",
    "es",
    "it",
];

const LangJson = {
    "fr": "lang_00",
    "en": "lang_01",
    "de": "lang_02",
    "nl": "lang_03",
    "es": "lang_04",
    "it": "lang_05"
};

const LangList = [
    "LocalFR",
    "LocalEN",
    "LocalDE",
    "LocalNL",
    "LocalES",
    "LocalIT",
    "LocalINTER"
]

/// <summary>
/// Type d'image à gérer
/// </summary>
const IMAGE_TYPE = {
    /// <summary>Image pour champ de type Image/Photo</summary>
    IMAGE_FIELD: 0,
    /// <summary>Image à destination d'un champ Mémo (e-mailing ou autre)</summary>
    MEMO: 1,
    /// <summary>Image pour avatar utilisateur connecté</summary>
    AVATAR: 2,
    /// <summary>Image pour avatar d'une fiche PP ou PM</summary>
    AVATAR_FIELD: 3,
    /// <summary>Image à afficher directement à partir d'une URL (pour la visionneuse d'annexes)</summary>
    URL: 4,
    /// <summary>Image logo Eudonet</summary>
    LOGO: 5,
    /// <summary></summary>
    IMAGE_WIDGET: 6,
    /// <summary></summary>
    TXT_URL: 7,
    /// <summary>
    /// Image pour avatar utilisateur (depuis l'admin)
    /// </summary>
    USER_AVATAR_FIELD: 8,
    /// <summary>Backlog #315 - Image à destination de la fenêtre d'édition d'image d'un champ Mémo CKEditor (e-mailing ou autre)</summary>
    MEMO_SETDIALOGURL: 9
};

const dateFormat = {
    'D d M YYYY': 'ddd D MMM YYYY HH:mm',
    'D dd/mm/yyyy': 'ddd DD/MM/YYYY HH:mm',
    'DD d MM YYYY': 'dddd D MMMM YYYY HH:mm',
    'dd/MM/yyyy': 'DD/MM/YYYY HH:mm',
    'MM/dd/yyyy': 'MM/DD/YYYY HH:mm',
    'yyyy/MM/dd': 'YYYY/MM/DD HH:mm',
    'yyyy-MM-dd': 'YYYY-MM-DD HH:mm'
};

const LongDateFormat = {
    LT: 'HH:mm',
    LTS: 'HH:mm:ss',
    L: 'DD/MM/YYYY',
    LL: 'D MMMM YYYY',
    LLL: 'D MMMM YYYY HH:mm',
    LLLL: 'dddd D MMMM YYYY HH:mm'
};

const Month = [
    'janvier',
    'février',
    'mars',
    'avril',
    'mai',
    'juin',
    'juillet',
    'août',
    'septembre',
    'octobre',
    'novembre',
    'décembre',
];

const MonthShort = [
    'janv.',
    'févr.',
    'mars',
    'avr.',
    'mai',
    'juin',
    'juil.',
    'août',
    'sept.',
    'oct.',
    'nov.',
    'déc.',
];

const Weekdays = [
    "dimanche",
    "lundi",
    "mardi",
    "mercredi",
    "jeudi",
    "vendredi",
    "samedi"
]

const WeekDaysShort = [
    "dim.",
    "lun.",
    "mar.",
    "mer.",
    "jeu.",
    "ven.",
    "sam."
]

const WeekdaysMin = [
    "Di",
    "Lu",
    "Ma",
    "Me",
    "Je",
    "Ve",
    "Sa"
]

const typeAlert = {
    Error: 0,
    Success: 1,
    Warning: 2,
    Standard: 3
}

/// <summary>
/// Champs système de la table ADDRESS
/// </summary>
const AdrField = {
    /// <summary>Rue 1 </summary>
    RUE1: 402,
    /// <summary>Pays </summary>
    PAYS: 403,
    /// <summary>Rue 2 </summary>
    RUE2: 404,
    /// <summary>Téléphone fixe </summary>
    TEL: 405,
    /// <summary>Fax</summary>
    FAX: 406,
    /// <summary>Rue 3 </summary>
    RUE3: 407,
    /// <summary>Code postal </summary>
    CP: 409,
    /// <summary>Ville </summary>
    VILLE: 410,

    /// <summary>Personnelle</summary>
    PERSO: 492,
    /// <summary>Principale</summary>
    PRINCIPALE: 412,
    /// <summary>Active</summary>
    ACTIVE: 411
}

/// <summary>
/// Champs système de la table PJ
/// </summary>
const PJField = {
    LIBELLE : 102001,
    DESCRIPTION : 102002,
    SIZE : 102004,
    NEW_TYPE : 102005,
    PJ_DATE : 102006,
    ADDED_BY : 102007,
    PJTEXT : 102008,
    /// <summary>Type d'annexe (fichier, web, ftp... constante PJ_TYPE)</summary>
    TYPE : 102009,
    TOOLTIPTEXT : 102010,
    FILE : 102011,
    FILEID : 102012,
    /// <summary>
    /// null : n'expire pas
    /// date : date à laquelle la pj expire
    /// </summary>
    EXPIRE_DAY : 102013,
    /// <summary>
    /// champs (factice) pour afficher les lien sécurisés
    /// </summary>
    SECURE_LINK : 102014
}


const MailMode = {
    MAIL_MODE_NO: 0, // ouverture en mode autre que mail
    MAIL_MODE_STD: 1, // ouverture en mode MAIL existant lecture seul/création
    MAIL_MODE_FWD: 2, // ouverture en mode Transfert mail 
    MAIL_MODE_DRAFT: 3, // ouverture en mode Brouillon mail
    MAIL_MODE_SMS: 4, // ouverture en mode sms lecture/crétion
}

/** Les tables fixes de la base de données ... */
const TableType =
{
    /// <summary>Gestion des doublons</summary>
    DOUBLONS: 2,
    /// <summary>Personne physique (Contact)</summary>
    PP: 200,
    /// <summary>Personne morale (Société)</summary>
    PM: 300,
    /// <summary>Adresse</summary>
    ADR: 400,
    /// <summary>reliquat antérieur V5 </summary>
    PRODUCT: 500,
    /// <summary>Historique</summary>
    HISTO: 100000,
    /// <summary>Utilisateur</summary>
    USER: 101000,
    /// <summary>Extension</summary>
    EXTENSION: 101100,
    /// <summary>Annexe</summary>
    PJ: 102000,
    /// <summary>Groupe</summary>
    GROUP: 103000,
    /// <summary>Filtre</summary>
    FILTER: 104000,
    /// <summary>Rapport</summary>
    REPORT: 105000,
    /// <summary>Fichier principal XRM : Campagne de mail</summary>
    CAMPAIGN: 106000,
    /// <summary>Modèle de mail</summary>
    MAIL_TEMPLATE: 107000,
    /// <summary>Sous-Fichier XRM : TrackLink</summary>
    TRACKLINK: 108000,
    /// <summary>Sous-Fichier XRM : TrackLinkLog</summary>
    TRACKLINKLOG: 109000,
    /// <summary>Sous-Fichier XRM : UnsubscribeMail</summary>
    UNSUBSCRIBEMAIL: 110000,
    /// <summary>Sous-Fichier XRM : Statistiques de campagne</summary>
    CAMPAIGNSTATS: 111000,

    /// <summary>Sous-Fichier XRM : Statistiques de campagne adv - version "en colonne des statistiques cf us 2579)</summary>
    CAMPAIGNSTATSADV: 111100,

    /// <summary>Sous-Fichier XRM : BounceMail</summary>
    BOUNCEMAIL: 112000,
    /// <summary>Sous-Fichier XRM : FormularXRM</summary>
    FORMULARXRM: 113000,
    /// <summary>Notification XRM</summary>
    NOTIFICATION: 114000,
    /// <summary>Resources des notification</summary>
    NOTIFICATION_TRIGGER_RES: 114100,
    /// <summary>Déclancheaurs de notification XRM</summary>
    NOTIFICATION_TRIGGER: 114200,
    /// <summary>Les désabonnée des notifications XRM</summary>
    NOTIFICATION_UNSUBSCRIBER: 114300,
    /// <summary>Les grilles XRM </summary>
    XRMGRID: 115000,
    /// <summary>Les widgets XRM</summary>
    XRMWIDGET: 115100,
    /// <summary>Les pages d'accueil XRM</summary>
    XRMHOMEPAGE: 115200,
    /// <summary>Table "Produits" (voir enum XrmProductField pour les champs)</summary>
    XRMPRODUCT: 116000,
    /// <summary>Table de logs des traitements RGPD</summary>
    RGPDTREATMENTSLOGS: 117000,
    /// <summary>Table interaction</summary>
    INTERACTION: 118000,
    /// <summary>Modèle d'import</summary>
    IMPORT_TEMPLATE: 119000,
    /// <summary>Fichier principal XRM : Transactions de paiement</summary>
    PAYMENTTRANSACTION : 119500
}

const BtnSpecificAction =
{
    /// <summary>
    /// Bouton normal
    /// </summary>
    Undefined: 0,
    /// <summary>
    /// création / mise à jour de l'evenement sur teams
    /// </summary>
    CreateSaveTeamsEvent: 1,
    /// <summary>
    /// suppression de l'évèneemnt sur teams
    /// </summary>
    DeleteTeamsEvent: 2


};

const caseFormat = 
{
    'CASE_UPPER':'text-uppercase',
    'CASE_LOWER':'text-lowercase',
    'CASE_NONE':'',
    'CASE_CAPITALIZE': 'text-capitalize'
}

const arrTabForbiddenPinFile = [EdnType.FILE_MAIL, EdnType.FILE_PJ, EdnType.FILE_DISCUSSION, EdnType.FILE_HISTO, EdnType.FILE_RELATION, EdnType.FILE_PLANNING, EdnType.FILE_SMS];
const arrDescIdForbiddenPinFile = [TableType.DOUBLONS, TableType.CAMPAIGN];

const typeBkm = {
    'bkm':1,
    'grid':2,
    'pinned':3
}

/// <summary>
/// Opération à effectuer lors de l'appel à certaines méthodes (POST, notamment)
/// </summary>
const PinnedBookmarkControllerOperation = {
    /// <summary>Aucune action ne sera effectuée si Operation est passée à -1 (par le processus lui-même suite à une erreur ou une demande incohérente, ou par l'appelant) </summary>
    NONE: -1,
    /// <summary>Opération par défaut, ou si non précisée : mettre à jour le signet épinglé dans la liste des signets épinglés de l'onglet souhaité : le retire s'il est déjà présent, l'ajoute s'il ne l'est pas. Ou si une liste est passée : REMPLACE (6)</summary>
    UPDATE: 0,
    /// <summary>Ajoute le signet épinglé dans la liste des signets épinglés de l'onglet souhaité. S'il existe déjà dans la liste, ne fait rien</summary>
    ADD: 1,
    /// <summary>Supprime le signet épinglé de la liste des signets épinglés de l'onglet souhaité. S'il ne figure pas dans la liste, ne fait rien</summary>
    DELETE: 2,
    /// <summary>Déplace le signet épinglé donné à la position indiquée dans la liste des signets épinglés de l'onglet souhaité : (ex : 100;200;300;400 devient 100;200;400;300 si cette opération est utilisée pour le signet 300 avec l'index 2). Si le signet donné est déjà à ladite position, ne fait rien</summary>           
    MOVE_POSITION: 3,
    /// <summary>Déplace le signet épinglé donné d'un rang vers la gauche dans la liste des signets épinglés de l'onglet souhaité : (ex : 100;200;300;400 devient 100;300;200;400 si cette opération est utilisée pour le signet 300). Si le signet donné est déjà en premier dans la liste, ne fait rien</summary>           
    MOVE_LEFT: 4,
    /// <summary>Déplace le signet épinglé donné d'un rang vers la droite dans la liste des signets épinglés de l'onglet souhaité : (ex : 100;200;300;400 devient 100;200;400;300 si cette opération est utilisée pour le signet 300). Si le signet donné est déjà en dernier dans la liste, ne fait rien</summary>           
    MOVE_RIGHT: 5,
    /// Remplacer la liste de signets épinglés existante par la liste donnée en paramètre (permet aussi de remettre à zéro une préférence foireuse)
    REPLACE: 6
}



/// <summary>
/// Enum des modes d'affichages de signet disponibles (BKMPREF.ViewMode)
/// </summary>
const BKMVIEWMODE = {
    LIST: 0,
    FILE: 1
}


/// <summary>
/// Le type de logic que l'on choisit.
/// </summary>
const LOGIC_DISPLAY_TYPE = {
    CHECKBOX: 0,
    SWITCH: 1,
}

const emptyValue = ['',undefined,null];


/// <summary>
/// NIVEAU ET PERM UTILISATEUR
/// </summary>
const UserLevel = {
    /// <summary>
    /// Aucun niveau
    /// </summary>
    LEV_USR_NONE: -1,
    /// <summary>Lecture Seule</summary>
    LEV_USR_READONLY: 0,
    /// <summary>Niveau 1</summary>
    LEV_USR_1: 1,
    /// <summary>Niveau 2</summary>
    LEV_USR_2: 2,
    /// <summary>Niveau 3</summary>
    LEV_USR_3: 3,
    /// <summary>Niveau 4</summary>
    LEV_USR_4: 4,
    /// <summary>Niveau 5</summary>
    LEV_USR_5: 5,
    /// <summary>Niveau 99 : ADMIN</summary>
    LEV_USR_ADMIN: 99,
    /// <summary>Niveau 100 : SUPERADMIN !!!</summary>
    LEV_USR_SUPERADMIN: 100,
    /// <summary>Niveau 200 : Administrateur produit</summary>
    LEV_USR_PRODUCT: 200
}

export {
    FieldType, FolderType, PropType, EdnType, expressFilterEnum, Operator, EngineConfirmModes, MsgType, Lang, IMAGE_TYPE,
    dateFormat, XrmCruAction, typeAlert, AdrField, Month, MonthShort, Weekdays, WeekDaysShort, WeekdaysMin,
    LongDateFormat, MailMode, TableType, LangJson, BtnSpecificAction,caseFormat,arrTabForbiddenPinFile, 
    arrDescIdForbiddenPinFile, PinnedBookmarkControllerOperation, BKMVIEWMODE, typeBkm, LangList, PJField, emptyValue, LOGIC_DISPLAY_TYPE, UserLevel
};
