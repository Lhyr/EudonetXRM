/// <reference path="eTools.js" />
/// <reference path="eMemoEditor.js" />

Number.isInteger = Number.isInteger || function (value) {
    return typeof value === "number" &&
        isFinite(value) &&
        Math.floor(value) === value;
};

Number.isNaN = Number.isNaN || function (value) { return typeof (o) === 'number' && isNaN(o) };

var nsMain = nsMain || {};

var mainDebugMode = false; // mettre à true pour empêcher la disparition du menu si la souris n'est plus dessus et permettre le debug de son contenu sous Firebug ou autre
var waiterTime;
var nbCompt = 0;
var updateError = false;
var nThemeId = 0; /* Identifiant du thème actuellement utilisé */
var nGlobalCurrentUserid = 0;  /* Userid de l'utilisateur connecté */
var nGlobalActiveTab = 0;  /* Table Active - variable global disponible pour tous les JS */

var nActivePageTabs = 0;   /* Page d'onglet active - Pour la pagination de la liste des onglets affichés   */
var bIsParamLoaded = -1;
var bIsNavBarLoaded = -1;
var bIsIFrameLoaded = -1;
var oModalAdd;
var oModalFilterForm;
var eModFile;

var bReloadMRU = 0;
var bResizeNavBar = 0;

// Initialisation au chargement de la list
var nBaseFontSize = 0;
/** Pour l'instance Vuejs de Iris Black */
var oVueInstance;


var modalImage;
var globalModalFile = false; // stock la ModalDialog permettant d'afficher une fiche en mode Popup 
var bGblFromApplyRuleOnBlank = false; // Indique si une fiche a été mise à jour depuis un applyruleonblank (mise à jour des champs/règles sans enregistrement en base)
var bGlobalOffLine = top.location.search.indexOf("off=1") > -1;
var bPreventLoadBkmList = false;

var TAB_PP = 200;
var TAB_ADR = 400;
var TAB_PM = 300;
var TAB_HOME = 0;
var TAB_USER = 101000;
var TAB_PJ = 102000;
var TAB_CAMPAIGN = 106000;
var TAB_RGPDTREATMENTLOG = 117000;
var TAB_IMPORTTEMPLATE = 119000;
var USER_VISIBLE = 81;
var TYPE_PLANNING = 83;
var CONFIDENTIAL = 84;
var BKM_PM_EVENT = 87; // Signet EVENT des EVENT ayant le même PP
var MULTI_OWNER = 88;
var MEMO_DESCRIPTION = 89;
var ATTACHMENT = 91;
var TPL_MULTI_OWNER = 92;
var MEMO_INFOS = 93;
var MEMO_NOTES = 94;
var DATE_CREATE = 95;
var DATE_MODIFY = 96;
var USER_CREATE = 97;
var USER_MODIFY = 98;
var OWNER_USER = 99;

var FLD_MAIL_TO = 4;
var FLD_MAIL_FROM = 3;


var EDNTYPE_PLANNING = 1;
var EDNTYPE_MAIL = 3;
var EDNTYPE_SMS = 4;
var EDNTYPE_PJ = 11;
var EDNTYPE_WEBTAB = 19;
var EDNTYPE_GRID = 24;


var OpenMode_MODAL = "1";
var OpenMode_HIDDEN = "2";
var OpenMode_NEW_WINDOW = "4";

var FONTSIZES = {
    xSmall:8,
    small:10,
    medium:12,
    large:14,
    xLarge:16,
    xxLarge:18
}



/// Liste des modules d'options utilisateur
/// /!\ CETTE ENUM EST RÉPLIQUÉE CÔTÉ SERVEUR DANS eUserOptionsModules.cs, AINSI QUE DANS LES FONCTIONS getUserOptionsModuleHashCode et loadUserOption DE CE FICHIER.
/// ELLE DOIT ÊTRE DONC MISE À JOUR EN MÊME TEMPS, DANS LE MEME ORDRE (INDEX) QUE L'ENUM CS /!\
var USROPT_MODULE_UNDEFINED = "UNDEFINED";
var USROPT_MODULE_MAIN = "MAIN";
var USROPT_MODULE_HOME = "HOME";
var USROPT_MODULE_PREFERENCES = "PREFERENCES";
var USROPT_MODULE_PREFERENCES_FONTSIZE = "PREFERENCES_FONTSIZE";
var USROPT_MODULE_PREFERENCES_LANGUAGE = "PREFERENCES_LANGUAGE";
var USROPT_MODULE_PREFERENCES_PROFILE = "PREFERENCES_PROFILE";
var USROPT_MODULE_PREFERENCES_SIGNATURE = "PREFERENCES_SIGNATURE";
var USROPT_MODULE_PREFERENCES_PASSWORD = "PREFERENCES_PASSWORD";
var USROPT_MODULE_PREFERENCES_MEMO = "PREFERENCES_MEMO";
var USROPT_MODULE_ADVANCED = "ADVANCED";
var USROPT_MODULE_ADVANCED_EXPORT = "ADVANCED_EXPORT";
var USROPT_MODULE_ADMIN = "ADMIN";
var USROPT_MODULE_ADMIN_GENERAL = "ADMIN_GENERAL";
var USROPT_MODULE_ADMIN_ORM = "ADMIN_ORM";
var USROPT_MODULE_ADMIN_ACCESS = "ADMIN_ACCESS";
var USROPT_MODULE_ADMIN_ACCESS_USERGROUPS = "ADMIN_ACCESS_USERGROUPS";
var USROPT_MODULE_ADMIN_ACCESS_SECURITY = "ADMIN_ACCESS_SECURITY";
var USROPT_MODULE_ADMIN_ACCESS_PREF = "ADMIN_ACCESS_PREF";
var USROPT_MODULE_ADMIN_TABS = "ADMIN_TABS";
var USROPT_MODULE_ADMIN_HOME = "ADMIN_HOME";
var USROPT_MODULE_ADMIN_HOME_V7_HOMEPAGES = "ADMIN_HOME_V7_HOMEPAGES";
var USROPT_MODULE_ADMIN_HOME_XRM_HOMEPAGES = "ADMIN_HOME_XRM_HOMEPAGES";
var USROPT_MODULE_ADMIN_HOME_EXPRESS_MESSAGE = "ADMIN_HOME_EXPRESS_MESSAGE";
var USROPT_MODULE_ADMIN_EXTENSIONS = "ADMIN_EXTENSIONS";
var USROPT_MODULE_ADMIN_DASHBOARD = "ADMIN_DASHBOARD";
var USROPT_MODULE_ADMIN_DASHBOARD_OFFERMANAGER = "ADMIN_DASHBOARD_OFFERMANAGER";
var USROPT_MODULE_ADMIN_DASHBOARD_RGPD = "ADMIN_DASHBOARD_RGPD";
var USROPT_MODULE_ADMIN_DASHBOARD_RGPDTREATMENTLOG = "ADMIN_DASHBOARD_RGPDTREATMENTLOG";
var USROPT_MODULE_ADMIN_GENERAL_LOGO = "ADMIN_GENERAL_LOGO";
var USROPT_MODULE_ADMIN_GENERAL_NAVIGATION = "ADMIN_GENERAL_NAVIGATION";
var USROPT_MODULE_ADMIN_GENERAL_LOCALIZATION = "ADMIN_GENERAL_LOCALIZATION";
var USROPT_MODULE_ADMIN_GENERAL_SUPERVISION = "ADMIN_GENERAL_SUPERVISION";
var USROPT_MODULE_ADMIN_GENERAL_CONFIGADV = "ADMIN_GENERAL_CONFIGADV";
var USROPT_MODULE_ADVANCED_PLANNING = "ADVANCED_PLANNING";
var USROPT_MODULE_ADMIN_EXTENSIONS_MOBILE = "ADMIN_EXTENSIONS_MOBILE";
var USROPT_MODULE_ADMIN_EXTENSIONS_SYNCHRO = "ADMIN_EXTENSIONS_SYNCHRO";
var USROPT_MODULE_ADMIN_EXTENSIONS_SYNCHRO_OFFICE365 = "ADMIN_EXTENSIONS_SYNCHRO_OFFICE365";
var USROPT_MODULE_ADMIN_EXTENSIONS_SMS = "ADMIN_EXTENSIONS_SMS";
var USROPT_MODULE_ADMIN_EXTENSIONS_CTI = "ADMIN_EXTENSIONS_CTI";
var USROPT_MODULE_ADMIN_EXTENSIONS_API = "ADMIN_EXTENSIONS_API";
var USROPT_MODULE_ADMIN_EXTENSIONS_EXTERNALMAILING = "ADMIN_EXTENSIONS_EXTERNALMAILING";
var USROPT_MODULE_ADMIN_EXTENSIONS_VCARD = "ADMIN_EXTENSIONS_VCARD";
var USROPT_MODULE_ADMIN_EXTENSIONS_SNAPSHOT = "ADMIN_EXTENSIONS_SNAPSHOT";
var USROPT_MODULE_ADMIN_EXTENSIONS_EMAILING = "ADMIN_EXTENSIONS_EMAILING";
var USROPT_MODULE_ADMIN_TAB = "ADMIN_TAB";
var USROPT_MODULE_ADMIN_TAB_USER = "ADMIN_TAB_USER";
var USROPT_MODULE_HOME = "HOME";
var USROPT_MODULE_PREFERENCES_FONTSIZE = "PREFERENCES_FONTSIZE";
var USROPT_MODULE_PREFERENCES_MRUMODE = "PREFERENCES_MRUMODE";
var USROPT_MODULE_ADMIN_EXTENSIONS_FROMSTORE = "ADMIN_EXTENSIONS_FROMSTORE";
var USROPT_MODULE_ADMIN_TAB_GRID = "ADMIN_TAB_GRID";
var USROPT_MODULE_ADMIN_EXTENSIONS_GRID = "ADMIN_EXTENSIONS_GRID";
var USROPT_MODULE_ADMIN_HOME_XRM_HOMEPAGE = "ADMIN_HOME_XRM_HOMEPAGE";
var USROPT_MODULE_ADMIN_HOME_XRM_HOMEPAGE_GRID = "ADMIN_HOME_XRM_HOMEPAGE_GRID";
var USROPT_MODULE_ADMIN_EXTENSIONS_NOTIFICATIONS = "ADMIN_EXTENSIONS_NOTIFICATIONS";
var USROPT_MODULE_ADMIN_EXTENSIONS_CARTO = "ADMIN_EXTENSIONS_CARTO";
var USROPT_MODULE_PREFERENCES_THEME = "PREFERENCES_THEME";
var USROPT_MODULE_ADMIN_EXTENSIONS_SIRENE = "ADMIN_EXTENSIONS_SIRENE";
var USROPT_MODULE_ADMIN_EXTENSIONS_POWERBI = "ADMIN_EXTENSIONS_POWERBI";
var USROPT_MODULE_ADMIN_EXTENSIONS_OUTLOOKADDIN = "ADMIN_EXTENSIONS_OUTLOOKADDIN";
var USROPT_MODULE_ADMIN_EXTENSIONS_ACCOUNTING_BUSINESSSOFT = "ADMIN_EXTENSIONS_ACCOUNTING_BUSINESSSOFT";
var USROPT_MODULE_ADMIN_EXTENSIONS_ACCOUNTING_CEGID = "ADMIN_EXTENSIONS_ACCOUNTING_CEGID";
var USROPT_MODULE_ADMIN_EXTENSIONS_ACCOUNTING_SAGE = "ADMIN_EXTENSIONS_ACCOUNTING_SAGE";
var USROPT_MODULE_ADMIN_EXTENSIONS_ACCOUNTING_EBP = "ADMIN_EXTENSIONS_ACCOUNTING_EBP";
var USROPT_MODULE_ADMIN_EXTENSIONS_ACCOUNTING_SIGMA = "ADMIN_EXTENSIONS_ACCOUNTING_SIGMA";
var USROPT_MODULE_ADMIN_EXTENSIONS_IN_UBIFLOW = "ADMIN_EXTENSIONS_IN_UBIFLOW";
var USROPT_MODULE_ADMIN_EXTENSIONS_IN_HBS = "ADMIN_EXTENSIONS_IN_HBS";
var USROPT_MODULE_ADMIN_EXTENSIONS_DOCUSIGN = "ADMIN_EXTENSIONS_DOCUSIGN";
var USROPT_MODULE_ADMIN_EXTENSIONS_SMS_NETMESSAGE = "ADMIN_EXTENSIONS_SMS_NETMESSAGE";
var USROPT_MODULE_ADMIN_EXTENSIONS_SYNCHRO_EXCHANGE2016ONPREMISE = "ADMIN_EXTENSIONS_SYNCHRO_EXCHANGE2016ONPREMISE";
var USROPT_MODULE_ADMIN_EXTENSIONS_ZAPIER = "ADMIN_EXTENSIONS_ZAPIER";
var USROPT_MODULE_ADMIN_DASHBOARD_VOLUME = "ADMIN_DASHBOARD_VOLUME";
var USROPT_MODULE_ADMIN_EXTENSIONS_EXTRANET = "ADMIN_EXTENSIONS_EXTRANET";
//SHA : tâche #1 873
var USROPT_MODULE_ADMIN_EXTENSIONS_ADVANCED_FORM = "ADMIN_EXTENSIONS_ADVANCED_FORM";
var USROPT_MODULE_ADMIN_EXTENSIONS_DEDICATED_IP = "ADMIN_EXTENSIONS_DEDICATED_IP";
var USROPT_MODULE_ADMIN_EXTENSIONS_WORLDLINE_PAYMENT = "ADMIN_EXTENSIONS_WORLDLINE_PAYMENT";
var USROPT_MODULE_ADMIN_EXTENSIONS_MAIL_STATUS_VERIFICATION = "ADMIN_EXTENSIONS_MAIL_STATUS_VERIFICATION"
var USROPT_MODULE_ADMIN_EXTENSIONS_LINKEDIN = "ADMIN_EXTENSIONS_LINKEDIN";
// Finder : type de recherche
// Voir également eFinderList.SearchType
var FinderSearchType = {
    /// <summary>Chercher</summary>
    Search: 0,
    /// <summary>Ajouter</summary>
    Add: 1,
    /// <summary>Associer</summary>
    Link: 2,
    /// <summary>Ajouter depuis signet</summary>
    AddFromBkm: 3,
    /// <summary>Filtre avancé</summary>
    AdvFilter: 4,
    /// <summary>Liste CTI</summary>
    CTI: 5,
    /// <summary>Sélection d'une fiche (par ex pour un widget de type Tuile)</summary>
    SelectFile: 6
}

var oFltMenu = null;
var oExportMenu = null;
var instanceSyncfusionChart = null;
var instanceSyncfusionGrid = null;

var CHART_EXPORT_TYPE = {
    PDF: 0,
    PNG: 1,
    XLS: 2
}

/* Enumération des thèmes disponibles (pour les fonctions effectuant des bascules de thèmes comme applyTheme() */
var THEMES = {
    XRM: {
        Id: 0, Version: 1, Sombre: 0
    },
    TURQUOISE: {
        ID: 1, Version: 1, Sombre: 0
    },
    FUCHSIA: {
        Id: 2, Version: 1, Sombre: 1
    }, /* et non FUSCHIA ou FUSHIA :) */
    ROUGE: {
        Id: 3, Version: 1, Sombre: 0
    },
    VERT: {
        Id: 4, Version: 1, Sombre: 0
    },
    ORANGE: {
        Id: 5, Version: 1, Sombre: 0
    },
    BLEU: {
        Id: 6, Version: 1, Sombre: 0
    },
    /* Thème spécifique client : 7 */
    CONTRASTE: {
        Id: 9, Version: 1, Sombre: 0
    },
    /* Thème spécifique client : 10 */
    BLANCROUGE: {
        Id: 11, Version: 2, Sombre: 0
    },
    ROUGE2019: {
        Id: 12, Version: 2, Sombre: 0
    },
    BLEU2019: {
        Id: 13, Version: 2, Sombre: 0
    },
    VERT2019: {
        Id: 14, Version: 2, Sombre: 0
    }
};

var POPUPTYPE = {
    /// <summary>Pas de catalogue</summary>
    NONE: 0,
    /// <summary>Catalogue simple avec saisie libre</summary>
    FREE: 1,
    /// <summary>Catalogue simple sans saisie libre</summary>
    ONLY: 2,
    /// <summary>Catalogue avancé</summary>
    DATA: 3,
    /// <summary>Catalogue relation</summary>
    SPECIAL: 4,
    /// <summary>Catalogue DESC</summary>
    DESC: 5,
    /// <summary>Catalogue ENUM</summary>
    ENUM: 6
};

/***
 * Type de rendu d'un catalogue avancé
 * */
var POPUPDATARENDER = {
    /// <summary>Rendu non défini</summary>
    NONE: 0,
    /// <summary>Rendu standard</summary>
    STANDARD: 1,
    /// <summary>Rendu en catalogue arborescent</summary>
    TREE: 2,
    /// <summary>Rendu en catalogue Etape</summary>
    STEP: 3
}

/// <summary>
/// Définit le statut d'activation/affichage du nouveau mode Fiche Eudonet X (aka. IRIS Black)
/// </summary>
var EUDONETX_IRIS_BLACK_STATUS = {
    /// <summary>
    /// Désactivé (par défaut)
    /// </summary>
    DISABLED: 0,
    /// <summary>
    /// Activé
    /// </summary>
    ENABLED: 1,
    /// <summary>
    /// En prévisualisation
    /// </summary>
    PREVIEW: 2
}

/// <summary>
/// Définit le statut d'activation/affichage du nouveau mode Liste Eudonet X (aka. IRIS Crimson List)
/// </summary>
var EUDONETX_IRIS_CRIMSON_LIST_STATUS = {
    /// <summary>
    /// Désactivé (par défaut)
    /// </summary>
    DISABLED: 0,
    /// <summary>
    /// Activé
    /// </summary>
    ENABLED: 1
}

/// <summary>
/// Définit le statut d'activation/affichage de la saisie guidée Eudonet X (aka. IRIS Purple File)
/// </summary>
var EUDONETX_IRIS_PURPLE_GUIDED_STATUS = {
    /// <summary>
    /// Désactivé (par défaut)
    /// </summary>
    DISABLED: 0,
    /// <summary>
    /// Activé
    /// </summary>
    ENABLED: 1
}



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


function getPreventLoadBkmList() {
    return bPreventLoadBkmList;
}
function setPreventLoadBkmList(value) {
    bPreventLoadBkmList = value;
}

/*
 * Affiche la popup Infos Debug - Révisé pour IRIS Black - cf. #84 291
 Informations fonctionnelles :
    - "MIDFORMULA" est égal à 1 s'il y a une formule du milieu ou un automatisme ORM sur le champ
    - "BOTFORMULA" est à 1 s'il y a une formule du bas ([DESC].Formula)
    - Le "LINK TO FILE" est le même pour tous les champs d'une même fiche
 */
function showttid(obj, e) {

    try {
        var bisCtrl;
        if (!e) {
            e = window.event;
            bisCtrl = e.ctrlKey
        } else {
            bisCtrl = e.ctrlKey
        }

        if (bisCtrl) {
            var sLabel = "";
            var sTab = "";
            var sFileId = "";
            var sDescId = "";
            var sMidFormula = "";
            var sBotFormula = "";
            var sFileHash = "";

            var oIRISBlackParentContainer = findUpByClass(obj, "button--resume");
            if (!oIRISBlackParentContainer)
                oIRISBlackParentContainer = findUpByClass(obj, "fileInput");

            // Récupération des informations
            // -----------------------------

            // IRIS Black
            if (oIRISBlackParentContainer) {
                sDescId = getNumber(getAttributeValue(oIRISBlackParentContainer, "divdescid"));
                sTab = getTabDescid(sDescId); // getTabDescid renvoie un type Number
                sFileId = getNumber(getAttributeValue(oIRISBlackParentContainer, "fileid"));
                sMidFormula = getAttributeValue(oIRISBlackParentContainer, "mf");
                sBotFormula = getAttributeValue(oIRISBlackParentContainer, "bf");
                var oIRISBlackFileContainer = document.getElementById("mainContentWrap");
                if (oIRISBlackFileContainer) {
                    sFileHash = getAttributeValue(oIRISBlackFileContainer, "hsh"); // le hash est stocké dans les infos de structure du fichier
                }
                var oIRISBlackLabelContainer = oIRISBlackParentContainer.querySelector("[class*=label]");
                if (oIRISBlackLabelContainer)
                    sLabel = GetText(oIRISBlackLabelContainer).trim();
            }
            // E17
            else {
                var sEiD = getAttributeValue(obj, "eid");
                if (sEiD != "" && sEiD.split("_").length == 2) {
                    var sTab = getNumber(sEiD.split("_")[0]);
                    var sFileId = getNumber(sEiD.split("_")[1]);


                }
                else {
                    if (obj.tagName && obj.tagName.toLowerCase() == "div" && getAttributeValue(obj, "_db").length > 0) {
                        //Planning graphique
                        sTab = nGlobalActiveTab;
                        sFileId = getNumber(getAttributeValue(obj, "fid"))
                    }
                    else if (obj.id && obj.id.toLowerCase().indexOf("bkmhead") == 0 && obj.id.split("_").length == 2) {
                        //signet
                        sTab = getNumber(obj.id.split("_")[1]);

                    }
                    else if (obj.tagName && obj.tagName.toLowerCase() == "td" && getAttributeValue(obj, "fld") != null && obj.id.split("_").length == 3) {
                        //Fiche
                        sTab = getNumber(obj.id.split("_")[1]);

                        var od = document.getElementById("fileDiv_" + sTab);
                        if (od) {
                            sFileId = getNumber(getAttributeValue(od, "fid"));
                        }
                    }
                }

                // Récupération des DescID, MidFormula, BotFormula, FileHash
                sDescId = getNumber(getAttributeValue(obj, "did"));
                sMidFormula = getAttributeValue(obj, "mf");
                sBotFormula = getAttributeValue(obj, "bf");
                sFileHash = getAttributeValue(obj, "hsh");
                sLabel = getAttributeValue(obj, "lib");
            }

            // Affichage des informations
            // -----------------------------

            var sLogInfos = "";
            var sLogInfosHTML = "";
            var nLogInfosWidth = 350;
            var nLogInfosHeight = 175;
            try {
                if (!isNaN(sTab) && typeof (sTab) == "number")
                    sLogInfos += " [TAB = " + sTab + "] " + "\n";

                if (!isNaN(sFileId) && typeof (sFileId) == "number")
                    sLogInfos += " [FILEID = " + sFileId + "]" + "\n";

                if (!isNaN(sDescId) && typeof (sDescId) == "number")
                    sLogInfos += " [DESCID = " + sDescId + "]" + "\n";

                if (sMidFormula != "")
                    sLogInfos += " [MIDFORMULA = " + sMidFormula + "]" + "\n";

                if (sBotFormula != "")
                    sLogInfos += " [BOTFORMULA = " + sBotFormula + "]";

                if (sFileHash != "") {
                    var sGoUrl = window.location.protocol + "//" + window.location.host + window.location.pathname;
                    sGoUrl = sGoUrl.substring(0, sGoUrl.lastIndexOf("/") + 1);
                    sGoUrl += "eGoToFile.aspx?tab=" + sTab + "&fid=" + sFileId + "&hash=" + encodeURIComponent(sFileHash);

                    sLogInfos += "\n [LINK TO FILE = " + sGoUrl + "]\n";

                    var sCopyDebugInfosLink = navigator.clipboard ? "(<a onclick=\"copyInfosDebugURL('" + sGoUrl + "');\">" + top._res_1387 + "</a>)" : "";
                    var sLinkToFile = "<input type=\"text\" class=\"infosDebugURL\" readonly=\"readonly\" value=\"" + sGoUrl + "\" onclick=\"this.select();\"/>" + sCopyDebugInfosLink;
                    sLogInfosHTML = sLogInfos.replace(sGoUrl, sLinkToFile);

                    nLogInfosWidth += 200;
                }
                else
                    sLogInfosHTML = sLogInfos;
            }
            catch (e) {
            }

            if (sLogInfos.length > 0) {
                var sTitle = "Infos Debug";
                if (sLabel != "")
                    sTitle += " - " + sLabel;

                if (typeof (console) != "undefined" && console && typeof (console.log) != "undefined")
                    console.log(sTitle + "\n" + sLogInfos);

                var myInfos = top.eAlert(1, sTitle, "<span style='-webkit-user-select: text; -moz-user-select: text;-ms-user-select: text;user-select: text;'> " + sLogInfosHTML + "</span>", "", nLogInfosWidth, nLogInfosHeight, null, false, true);

                myInfos.adjustModalToContent();
            }


            if (e.stopPropagation)
                e.stopPropagation();

            if (e.preventDefault)
                e.preventDefault();

            e.cancelBubble = true;

            return false;

        }
    }
    catch (e) {
        //fonction de debugg utilitaire//en cas d'erreur, on ne plante pas l'appli

    }
}

function copyInfosDebugURL(sGoUrl) {
    if (navigator.clipboard) {
        navigator.clipboard.writeText(sGoUrl);
        var notifClipboard = {};
        notifClipboard.id = "shareFile";
        notifClipboard.title = top._res_2630; // Le lien a été copié
        notifClipboard.color = "green";
        // Contrairement à IRIS, le lien est proposé dans le champ <input> situé à côté de (Copier) dans la boîte de dialogue Infos Debug. On ne propose donc pas
        // de le copier lors du clic sur la notification.
        notifClipboard.selectOnClick = false;
        notifToast(notifClipboard);
    }
}

//Charge un doc XML
function loadXMLDoc(dname) {
    try {
        xmlHttp = new XMLHttpRequest();
        xmlHttp.open("GET", dname, false);
        xmlHttp.send();
        return xmlHttp.responseXML;
    }
    catch (e) {
        return null;
    }
}


var oWinExportToV7 = false;
var bInit = false;
/// Ouverture d'une spécif
///  type : 1 - Favori
///         2 - Spécif "classique", fiche ou liste
///         3 - sur déclencheur asp=
///         4 - EudoPart
///         5 - Champ type web
///         6 - Organigrame
///         7 - Export/Impression
function exportToLinkToV7(sUrl, nFileId, nType) {

    if (!nFileId)
        nFileId = 0;

    if (typeof sUrl == "undefined") {
        return;
    }

    //Ferme la fenetre si déjà ouvert
    if (oWinExportToV7 && !oWinExportToV7.closed)
        oWinExportToV7.close();

    //Option pour la fenêtre
    //Option pour la fenêtre
    if (typeof w == "undefined")
        var w = 200;

    if (typeof h == "undefined")
        var h = 200;

    var left = (screen.width / 2) - (w / 2);
    var top = (screen.height / 2) - (h / 2);
    var sOption = 'toolbar=no, location=no, directories=no, status=no, menubar=no, scrollbars=no, resizable=no, copyhistory=no, top=' + top + ', left=' + left;

    oWinExportToV7 = window.open('eExportToV7.aspx?id=' + encode(sUrl) + '&type=' + nType + '&tab=' + nGlobalActiveTab + '&fileid=' + nFileId, 'oWinExportToV7', sOption);


}




//lance une spécif XRMV2
// va chercher les informations nécessaires
// calcule le token
function runSpec(id, nTab, nFId, nDescId, callBack) {


    var oSpecUpdLc = new eUpdater("mgr/eGetSpecifToken.ashx");
    var nTabSpecif;

    oSpecUpdLc.addParam("sid", id, "post");

    var sView = top.getCurrentView();

    //Table de la spécif.
    // table en cours si non spécifié
    if (typeof (nTab) != "undefined" && nTab != null && nTab != "")
        nTabSpecif = nTab;
    else
        nTabSpecif = top.nGlobalActiveTab;

    oSpecUpdLc.addParam("tab", nTabSpecif, "post");

    /* Lancement depuis un mode fiche en modification  */
    //Fiche de lancement
    if (typeof (nFId) != "undefined" && nFId != null && nFId != "" && nFId != "0") {
        oSpecUpdLc.addParam("fid", nFId, "post");
    }
    else if (sView == "FILE_MODIFICATION" && nTabSpecif == top.nGlobalActiveTab) {
        oSpecUpdLc.addParam("fid", top.GetCurrentFileId(top.nGlobalActiveTab), "post");
    }

    if (sView == "FILE_MODIFICATION" && nTabSpecif != top.nGlobalActiveTab) {
        oSpecUpdLc.addParam("parenttab", top.nGlobalActiveTab, "post");
        oSpecUpdLc.addParam("parentfid", top.GetCurrentFileId(nGlobalActiveTab), "post");
    }


    //Champ de lancement
    if (typeof (nDescId) != "undefined" && nDescId != null && nDescId != "")
        oSpecUpdLc.addParam("descid", nDescId, "post");

    if (typeof callBack != "function")
        oSpecUpdLc.send(runSpec2);
    else
        oSpecUpdLc.send(callBack);
}

function runSpecFromWidgetTile(id) {
    var oSpecUpdLc = new eUpdater("mgr/eGetSpecifToken.ashx");
    oSpecUpdLc.addParam("sid", id, "post");

    oSpecUpdLc.send(runSpec2);
}

function runSpec2(oRes) {

    if (!oRes) {
        return;
    }

    if (getXmlTextNode(oRes.getElementsByTagName("success")[0]) != "1") {
        var sErr = getXmlTextNode(oRes.getElementsByTagName("error")[0]);
        if (sErr != "")
            eAlert(0, "", sErr);
        else
            eAlert(0, "", top._res_6700);

        return;
    }

    var sToken = getXmlTextNode(oRes.getElementsByTagName("token")[0]);
    var sUrl = getXmlTextNode(oRes.getElementsByTagName("url")[0]);
    var sUrlParam = getXmlTextNode(oRes.getElementsByTagName("urlparam")[0]);
    var sOpenMode = getXmlTextNode(oRes.getElementsByTagName("openmode")[0]);
    var bStatic = getXmlTextNode(oRes.getElementsByTagName("static")[0]) == "1";
    var sLabel = "";
    var width;
    var height;
    var framesizeauto = true;

    var n = sUrl.lastIndexOf("?");
    if (n == -1)
        sUrl += "?xedntimer=" + timeStamp();
    else
        sUrl += "&xedntimer=" + timeStamp();

    if (bStatic)
        sUrl += "&token=" + sToken;

    switch (sOpenMode) {
        case OpenMode_MODAL:
        case OpenMode_NEW_WINDOW:

            var bNewWin = sOpenMode == OpenMode_NEW_WINDOW;

            sLabel = getXmlTextNode(oRes.getElementsByTagName("label")[0]);
            var aUrlParams = sUrlParam.split("&");
            for (var i = 0; i < aUrlParams.length; i++) {
                var aUrlParam = aUrlParams[i].split("=");
                if (aUrlParam[0] == "w")
                    width = getNumber(aUrlParam[1]);
                else if (aUrlParam[0] == "h")
                    height = getNumber(aUrlParam[1]);
                else if (aUrlParam[0] == "framesizeauto")
                    framesizeauto = aUrlParam[1] != "0";
            }

            openSpecMd(sUrl, sToken, sLabel, width, height, bNewWin, framesizeauto, bStatic);

            break;
        case OpenMode_HIDDEN:
            launchHiddenSpec(sUrl, sToken);
            break;

        case OpenMode_NEW_WINDOW:
            openSpecWin(sUrl);
            break;
    }
}

//ouvre une spécif en sous marin
function launchHiddenSpec(sUrl, sToken) {
    top.setWait(true);
    var oSpecUpd = new eUpdater(sUrl);
    var sView = top.getCurrentView();
    if (sView == "FILE_MODIFICATION")
        oSpecUpd.addParam("fid", top.GetCurrentFileId(nGlobalActiveTab), "post");
    oSpecUpd.addParam("tab", nGlobalActiveTab, "post");
    oSpecUpd.addParam("t", sToken, "post");
    oSpecUpd.send(function () { top.setWait(false); });
}

//ouvre une specif dans une modaldialog
function openSpecMd(sUrl, sToken, sLabel, width, height, bNewWin, framesizeauto, bIsStatic) {

    var sView = top.getCurrentView();

    var mdSpec, iframeId;
    var frameActivMaxSize = false;
    if (!bNewWin) {

        var sURLOpen = "eblank.aspx";

        if (bIsStatic)
            sURLOpen = sUrl;

        mdSpec = new top.eModalDialog(sLabel, 0, sURLOpen, width, height);

        if (typeof width == "undefined" || width == null || typeof height == "undefined" || height == null)
            mdSpec.forceWindowMaxSize();

        iframeId = mdSpec.iframeId;
        mdSpec.Handle = 'specif_' + mdSpec.iframeId;
        top.window["_md"][mdSpec.Handle] = mdSpec;
        mdSpec.isSpecif = true;
        mdSpec.isStatic = bIsStatic;
    }
    else {
        if (!width) width = 800;
        if (!height) height = 900;

        iframeId = "view";

        var sURLOpen = "";

        if (bIsStatic)
            sURLOpen = sUrl;

        mdSpec = top.open(sURLOpen, iframeId, 'width=' + width + ', height=' + height + ', resizable=yes');
        if (bIsStatic)
            return;

    }

    //Création d'un formulaire
    var formToPost = document.createElement('form');
    formToPost.id = "submitForm_" + iframeId;
    formToPost.method = "POST";
    formToPost.action = sUrl;
    formToPost.target = iframeId;

    formToPost.appendChild(getInput("modalid", "hidden", iframeId));
    formToPost.appendChild(getInput("t", "hidden", sToken));
    formToPost.appendChild(getInput("tab", "hidden", nGlobalActiveTab));

    if (sView == "FILE_MODIFICATION")
        formToPost.appendChild(getInput("fid", "hidden", top.GetCurrentFileId(nGlobalActiveTab)));

    if (!bNewWin) {
        // Ouverture en modal
        mdSpec.show();
        mdSpec.addButtonFct(top._res_30, function () { mdSpec.hide(); }, 'button-green');

        if (bIsStatic)
            return;

        var oDiv = mdSpec.getDivContainer();
        oDiv.appendChild(formToPost);
        oDiv.style.overflowY = "hidden";

        mdSpec.onIframeLoadComplete = function () {
            var oFrame = mdSpec.getIframeTag();
            if (framesizeauto) {
                oFrame.style.overflow = "auto";
                oFrame.onload = function () {
                    mdSpec.adjustModalToContentIframe();

                    //le 1er appel n'a pas toujours les "bonnes" valeurs de taille
                    // une fois modifiée une 1er fois les tailles sont ensuite "correct" et l'ajustement peut se faire
                    setTimeout(mdSpec.adjustModalToContentIframe, 200);
                }
            }

            // On doit vider la fonction de onIframeLoadComplete pour ne pas qu'elle se lance en boucle
            // suite à la modification de son appel dans eModalDialog (onIframeLoadComplete se lance désormais sur le "vrai" load de l'iframe)
            mdSpec.onIframeLoadComplete = function () { };

            var oForm = top.document.getElementById("submitForm_" + iframeId);
            // On retransmet les params à la specif pour obtenir la taille de la fenêtre
            oForm.appendChild(getInput("height", "hidden", Math.round(mdSpec.absWidth)));
            oForm.appendChild(getInput("width", "hidden", Math.round(mdSpec.absHeight)));
            oForm.appendChild(getInput("divMainWidth", "hidden", mdSpec.getDivMainWidth()));
            oForm.appendChild(getInput("divMainHeight", "hidden", mdSpec.getDivMainHeight()));

            oForm.submit();
        }
    }
    else {
        // Ouverture en popup
        top.document.body.appendChild(formToPost);
        var oForm = top.document.getElementById("submitForm_" + iframeId);
        formToPost.submit();
    }
}

//ouvre une specif dans une nouvelle fenêtre
function openSpecWin(sUrl) {
    window.open(sUrl);
}

//objet XML to String
function xmlToString(xmlData) {
    if (window.ActiveXObject) {
        //for IE
        xmlDoc = new ActiveXObject("Microsoft.XMLDOM");
        xmlDoc.async = "false";
        xmlDoc.loadXML(xmlData);
        return xmlDoc;
    } else if (document.implementation && document.implementation.createDocument) {
        //for Mozila
        parser = new DOMParser();
        xmlDoc = parser.parseFromString(xmlData, "text/xml");
        return xmlDoc;
    }
}

/**********************************************************************/
/*          MENU ACTIONS                                                */
/**********************************************************************/
function shBtnAction(sBtn, bOn) {
    /*   */
    if (sBtn != "H")
        sBtn = "B";

    var btnD = document.getElementById("aD" + sBtn);
    var btnM = document.getElementById("aM" + sBtn);

    if (bOn) {
        if (btnD)
            switchClass(btnD, "aD" + sBtn, "aD" + sBtn + "A");

        if (btnM)
            btnM.setAttribute("eactif", "1");
    }
    else {
        if (btnM) {
            btnM.setAttribute("eactif", "0");
            omouA(sBtn);
        }
    }
}

function hideFltMenu(oFltBtnDiv) {

    var oFltMenuMouseOver = function () {

        var fltOut = setTimeout(
            function () {

                if (oFltMenu) {
                    oFltMenu.hide();
                    unsetEventListener(oFltMenu.mainDiv, "mouseout", oFltMenuMouseOver);

                    if (!oFltBtnDiv.className.indexOf(" advFltActiv ") > -1) {
                        removeClass(oFltBtnDiv, "advFltPressed");
                    }
                }
            }
            , 200);

        //Annule la disparition            
        if (oFltBtnDiv) {
            setEventListener(oFltBtnDiv, "mouseover", function () { clearTimeout(fltOut); });
        }
        if (oFltMenu) {
            setEventListener(oFltMenu.mainDiv, "mouseover", function () { clearTimeout(fltOut); });
        }
    };

    if (oFltBtnDiv) {
        setEventListener(oFltBtnDiv, "mouseout", oFltMenuMouseOver);
    }
}

function dispFltMenu(oFltBtnDiv, bCurrentFilter, currentFilterId, currentFilterName) {

    var bCA = getAttributeValue(oFltBtnDiv, "ca") == "1";
    var bCE = getAttributeValue(oFltBtnDiv, "ce") == "1";

    //Position initiale hors écran
    oFltMenu = new eContextMenu(null, -999, -999);
    if (oFltBtnDiv.className.indexOf(" advFltActiv ") > -1)
        addClass(oFltBtnDiv, "advFltActivPressed");
    else
        addClass(oFltBtnDiv, " advFltPressed ");

    if (bCurrentFilter && bCE) {

        //Modifier le filtre en cours
        oFltMenu.addItemFct(top._res_6246, function () { editFilter(currentFilterId, nGlobalActiveTab); }, 1, 0, "advFltItem ModifFlt", null, "icnFlt icon-edn-pen");
        oFltMenu.addSeparator(1);
    }

    // ajouter un filtre
    if (bCA) {
        oFltMenu.addItemFct(top._res_6431, function () { AddNewFilter(nGlobalActiveTab); }, 1, 0, "advFltItem NewFlt", null, "icnFlt icon-new_filter");
        oFltMenu.addSeparator(1);
    }

    if (bCurrentFilter) {
        // Filtre en cours
        oFltMenu.addItemFct(top._res_183, function () { cancelAdvFlt(nGlobalActiveTab); }, 1, 0, "advFltItem ActivFlt", null, "icnFlt icon-rem_filter del_state");
        oFltMenu.addSeparator(1);
    }

    // liste des filtres
    oFltMenu.addItemFct(top._res_6248, function () { filterList(); }, 1, 0, "advFltItem FltList", null, "icnFlt icon-list_filter");


    oFltMenu.alignElement(oFltBtnDiv, "UNDER", "0|5"); // Le menu apparait en dessous du bouton

    //  initAdvFilterBtn(oFltBtnDiv);

    if (!mainDebugMode) {
        var oFltMenuMouseOver = function () {

            var fltOut = setTimeout(
                function () {
                    //Masque le menu
                    if (oFltMenu) {

                        if (oFltBtnDiv.className.indexOf(" advFltActiv ") > -1)
                            oFltBtnDiv.className = oFltBtnDiv.className.replace(" advFltActivPressed ", "");
                        else
                            oFltBtnDiv.className = oFltBtnDiv.className.replace(" advFltPressed ", "");


                        oFltMenu.hide();
                        unsetEventListener(oFltMenu.mainDiv, "mouseout", oFltMenuMouseOver);
                    }
                }
                , 200);

            //Annule la disparition
            if (oFltMenu) {
                setEventListener(oFltMenu.mainDiv, "mouseover", function () { clearTimeout(fltOut) });
            }
            if (oFltBtnDiv) {
                setEventListener(oFltBtnDiv, "mouseover", function () { clearTimeout(fltOut); });
            }
        };

        //si on sort de la div de bouton ou de menu, on a 200ms pour se rattraper
        setEventListener(oFltBtnDiv, "mouseout", oFltMenuMouseOver);
        if (oFltMenu) {
            setEventListener(oFltMenu.mainDiv, "mouseout", oFltMenuMouseOver);
        }
    }

}

// #BEGIN REGION FILTER#

var oModalFilterList;
var oModalFilterForm;
function filterList(filterType) {
    filterListObjet(filterType);
}

/*
 
 filterListObjet(filterType, {
        tab: nGlobalActiveTab,
        onClose: onCloseFilterList,
        onApply: onApplySelectedFilter,
        onEdit: onEditSelectedFilter
    });  
 }
 */
function filterListObjet(filterType, options) {

    // Valeurs par défaut
    options = options || {};
    if (typeof (options.tab) == "undefined")
        options.tab = nGlobalActiveTab;

    if (typeof (options.value) == "undefined")
        options.value = "0";

    if (typeof (options.onClose) != "function")
        options.onClose = onCloseFilterList;

    if (typeof (options.onApply) != "function")
        options.onApply = onApplySelectedFilter;

    if (typeof (options.onEdit) != "function")
        options.onEdit = onEditSelectedFilter;

    if (typeof (options.deselectAllowed) == "undefined")
        options.deselectAllowed = false;

    if (typeof (options.adminMode) == "undefined")
        options.adminMode = false;

    if (typeof (options.selectFilterMode) == "undefined")
        options.selectFilterMode = false;

    var nPopupWidth = 927;
    var bTabletMode = isTablet();
    //var bTabletMode = true;
    if (bTabletMode) {
        if (Number(document.width) < 1024)
            nPopupWidth = 750;
    }
    if (!filterType)
        filterType = 0

    var nRows = GetNumRows(350);

    var modalLabel = ""

    switch (filterType) {
        case 0:
            modalLabel = top._res_6248;
            break;
        case 2:
            modalLabel = top._res_7171;
            break;
        default:

    }

    oModalFilterList = new eModalDialog(modalLabel, 0, 'eFilterReportListDialog.aspx', nPopupWidth, 500, "oModalFilterList");
    oModalFilterList.ErrorCallBack = launchInContext(oModalFilterList, oModalFilterList.hide);

    oModalFilterList.addParam("type", filterType, "post");
    oModalFilterList.addParam("rows", nRows, "post");
    oModalFilterList.addParam("page", 1, "post");
    oModalFilterList.addParam("tab", options.tab, "post");
    oModalFilterList.addParam("value", options.value, "post");
    oModalFilterList.addParam("frmId", oModalFilterList.iframeId, "post");
    oModalFilterList.addParam("lstType", 12, "post");
    oModalFilterList.addParam("deselectAllowed", options.deselectAllowed ? "1" : "0", "post");
    oModalFilterList.addParam("adminMode", options.adminMode ? "1" : "0", "post");
    oModalFilterList.addParam("selectFilterMode", options.selectFilterMode ? "1" : "0", "post");

    oModalFilterList.show();
    oModalFilterList.addButtonFct(top._res_29, options.onClose, 'button-gray');
    if (filterType == 8 || (options.adminMode == "1" && filterType != 2) || options.selectFilterMode)
        oModalFilterList.addButtonFct(top._res_28, function () { options.onApply(oModalFilterList); }, 'button-green', "btnValidFilter"); // Valider
    else if (filterType == 0) // user
        oModalFilterList.addButtonFct(top._res_219, function () { options.onApply(oModalFilterList); }, 'button-green', "btnValidFilter"); // Appliquer
    else
        oModalFilterList.addButtonFct(top._res_151, options.onEdit, 'button-gray', 'fltLstModBtn'); // Modifier
}

function onCloseFilterList() {
    oModalFilterList.hide();
}
function onApplySelectedFilter() {
    var winFilterList = oModalFilterList.getIframe();
    winFilterList.ApplySelectedFilter();
}

function onEditSelectedFilter() {
    var winFilterList = oModalFilterList.getIframe();
    winFilterList.EditSelectedFilter();
}

function reloadModalFilterList() {
    if (typeof (top.window['_md']['oModalFilterList']) != 'undefined') {
        modalFilterList = top.window['_md']['oModalFilterList'];
        loadList();
    }
}

function AddNewFilter(nTab, aBtnsArray, bAdminMode, nType, sFilterName, nWidgetType) {
    AddNewFilterV2(nTab, aBtnsArray, bAdminMode, false, nType, sFilterName, nWidgetType);
}

function AddNewFilterV2(nTab, aBtnsArray, bAdminMode, bSelectFilterMode, nType, sFilterName, nWidgetType) {

    if (typeof bAdminMode === "undefined")
        bAdminMode = false;

    if (typeof bSelectFilterMode === "undefined")
        bSelectFilterMode = false;

    var nPopupWidth = 1000;
    var bTabletMode = isTablet();
    //var bTabletMode = true;
    if (bTabletMode) {
        if (Number(document.width) < 1024)
            nPopupWidth = 750;
        else
            nPopupWidth = 1005; // 1005 pixels : largeur maximale autorisée pour les tablettes en 1024 x 768
    }

    if (typeof nType === "undefined") {
        var oMainDiv = document.getElementById("mainDiv");
        nType = getNumber(getAttributeValue(oMainDiv, "type"));
    }

    var modalLabel = top._res_6431;

    if (nType == 2)
        modalLabel = top._res_7173;
    else if (nType == 9)
        modalLabel = top._res_8266; // Filtre à l'ouverture

    oModalFilterWizard = new eModalDialog(modalLabel, 0, 'eFilterWizard.aspx', nPopupWidth, 600, 'oModalFilterWizard');
    oModalFilterWizard.addParam("tabletMode", (bTabletMode ? "1" : "0"), "post");
    oModalFilterWizard.addParam("popupWidth", nPopupWidth, "post");
    oModalFilterWizard.addParam("tab", nTab, "post");
    oModalFilterWizard.addParam("type", nType, "post");
    oModalFilterWizard.addParam("action", 'addnew', "post");
    oModalFilterWizard.addParam("adminMode", (bAdminMode ? "1" : "0"), "post");
    oModalFilterWizard.addParam("selectFilterMode", (bSelectFilterMode ? "1" : "0"), "post");
    if (typeof sFilterName !== 'undefined')
        oModalFilterWizard.addParam("name", sFilterName, "post");

    // Si c'est un filtre pour widget, on envoie le type de widget
    if (nType == 9) {
        if (typeof nWidgetType === "undefined")
            nWidgetType = 8; // Tuiles
        oModalFilterWizard.addParam("widgetType", nWidgetType, "post");
    }

    oModalFilterWizard.ErrorCallBack = launchInContext(oModalFilterWizard, oModalFilterWizard.hide);
    if (nType == 2) {
        oModalFilterWizard.onHideFunction = reloadModalFilterList;
    }

    oModalFilterWizard.show();
    if (typeof (aBtnsArray) == "object" && aBtnsArray != null) {
        oModalFilterWizard.addButtons(aBtnsArray);
    }
    else {

        oModalFilterWizard.addButton(top._res_29, cancelFilter, "button-gray", null);

        if (nType != 2 && nType != 8 && !bAdminMode && !bSelectFilterMode) // pas de apply pour les règles et les notifs et pour le mode admin
            oModalFilterWizard.addButton(top._res_219, applyFilter, "button-green", null);

        if (nType == 8 || bAdminMode || bSelectFilterMode) // pas d'enregistrer non plus pour les règles et les notifs              
            oModalFilterWizard.addButton(top._res_286, function (oRes) { oModalFilterWizard.getIframe().saveDb(0, null, false, notifFilterSaveDbReturn) }, "button-green", null);
        else
            oModalFilterWizard.addButton(top._res_286, saveFilter, "button-gray", null);

        // A la création enregistrer ouvre la fenetre de renommage
        //oModalFilterWizard.addButton(top._res_118, saveFilterAs, "button-gray", null);
    }
}

function notifFilterSaveDbReturn() {
    loadList();
    oModalFilterWizard.hide();
}

function getFilterWizardFromInvitBtns() {
    var aBtns = new Array();
    aBtns.push(new eModalButton(top._res_29, cancelFilter, "button-gray"));
    aBtns.push(new eModalButton(top._res_219, saveSelFilter, "button-green"));
    aBtns.push(new eModalButton(top._res_286, saveFilter, "button-gray"));
    aBtns.push(new eModalButton(top._res_118, saveFilterAs, "button-gray"));
    return aBtns;
}

//permet de Sauver le filtre et de le sélectionner dans la liste
function saveSelFilter() {

    var _ifrm = oModalFilterWizard.getIframe();
    _ifrm.saveDb(0, 0, true, function (oRes) { selFilter(oRes); });

}

function selFilter(oRes) {

    var filterid = getXmlTextNode(oRes.getElementsByTagName("filterid")[0]);
    var oMainDiv = document.getElementById("mainDiv");
    if (oMainDiv) {

        _activeFilter = filterid;
        // MCR/SPH 40260 : resize de la dernière colonne du tableau à 100%, sur une edition de filtres en ++, set variable globale : _activeFilterTab
        _activeFilterTab = getXmlTextNode(oRes.getElementsByTagName("filteridtab")[0]);


    }
    if (oModalFilterWizard && oModalFilterWizard != null) {
        oModalFilterWizard.hide();
    }
    if (top.modalWizard && top.modalWizard.getIframe() && top.modalWizard.getIframe().nsInvitWizard) {
        _eCurentSelectedFilter = null;

        var stepDiv = document.getElementById("step_2");
        removeClass(stepDiv, "state_grp");
        addClass(stepDiv, "state_grp-validated");

        StepClick('2');
    }
    else
        loadList();
}

function saveFilterAs() {
    var _ifrm = oModalFilterWizard.getIframe();
    _ifrm.saveFilterAs();
}

function saveFilter() {
    var _ifrm = oModalFilterWizard.getIframe();
    _ifrm.saveFilter();
}

function applyFilter() {
    var _ifrm = oModalFilterWizard.getIframe();
    _ifrm.applyFilter();

}

function cancelFilter() {
    oModalFilterWizard.hide();
}

function cancelLnkFile() {
    oModalLnkFile.hide();
}


function editFilter(filterId, nTab, aBtnsArray, nType, bAdminMode, widgetType) {
    editFilterV2(filterId, nTab, aBtnsArray, nType, bAdminMode, false, widgetType)
}

function editFilterV2(filterId, nTab, aBtnsArray, nType, bAdminMode, bSelectFilterMode, widgetType) {

    if (filterId.toString() == "0") {
        eAlert(0, top._res_6246, top._res_430);
        return;
    }

    if (!nType) {
        var oMainDiv = document.getElementById("mainDiv");
        nType = getNumber(getAttributeValue(oMainDiv, "type"));
    }

    if (typeof bAdminMode === "undefined")
        bAdminMode = false;

    if (typeof bSelectFilterMode === "undefined")
        bSelectFilterMode = false;

    //_aFilterWizarBtns est la liste des boutons de l'assistant filtre, fourni par l'assistant de sélection à partir d'un filtre
    if (typeof (aBtnsArray) != "object" && typeof (_aFilterWizarBtns) == "object") {
        aBtnsArray = _aFilterWizarBtns
    }
    var nPopupWidth = 1000;
    var bTabletMode = isTablet();
    //var bTabletMode = true;
    if (bTabletMode) {
        if (Number(document.width) < 1024)
            nPopupWidth = 750;
        else
            nPopupWidth = 1005; // 1005 pixels : largeur maximale autorisée pour les tablettes en 1024 x 768
    }

    var modalLabel = top._res_6246;
    switch (nType) {
        case 0:
            modalLabel = top._res_6246;
            break;
        case 2:
            modalLabel = top._res_7172;
            break;
        case 3:
            modalLabel = top._res_7622;
            break;
        default:
            break;
    }

    oModalFilterWizard = new eModalDialog(modalLabel, 0, 'eFilterWizard.aspx', nPopupWidth, 600, 'oModalFilterWizard');
    oModalFilterWizard.addParam("tabletMode", (bTabletMode ? "1" : "0"), "post");
    oModalFilterWizard.addParam("popupWidth", nPopupWidth, "post");
    oModalFilterWizard.addParam("tab", nTab, "post");
    oModalFilterWizard.addParam("filterid", filterId, "post");
    oModalFilterWizard.addParam("adminMode", (bAdminMode ? "1" : "0"), "post");
    oModalFilterWizard.addParam("selectFilterMode", (bSelectFilterMode ? "1" : "0"), "post");
    if (!(nType == null || typeof (nType) == "undefined"))
        oModalFilterWizard.addParam("type", nType, "post");

    // Si c'est un filtre pour widget, on envoie le type de widget
    if (nType == 9) {
        if (typeof nWidgetType === "undefined")
            nWidgetType = 8; // Tuiles
        oModalFilterWizard.addParam("widgetType", nWidgetType, "post");
    }

    oModalFilterWizard.addParam("action", 'edit', "post");
    oModalFilterWizard.ErrorCallBack = launchInContext(oModalFilterWizard, oModalFilterWizard.hide);

    oModalFilterWizard.show();
    if (typeof (aBtnsArray) == "object" && aBtnsArray != null) {
        oModalFilterWizard.addButtons(aBtnsArray);
    }
    else {

        oModalFilterWizard.addButton(top._res_29, cancelFilter, "button-gray", null);

        if (nType != 2 && nType != 8 && !bAdminMode && !bSelectFilterMode) // pas de apply pour les règles et les notifs 
        {
            var args = null;
            oModalFilterWizard.addButton(top._res_219, applyFilter, "button-green", args);
        }


        if (nType == 8 || bAdminMode || bSelectFilterMode) // uniquement le bouton enregistrer
        {
            oModalFilterWizard.addButton(top._res_286, function (oRes) { oModalFilterWizard.getIframe().saveDb(0, null, false, notifFilterSaveDbReturn); }, "button-green", null);
            oModalFilterWizard.addButton(top._res_86, saveFilterAs, "button-gray", null);
        }
        else {
            oModalFilterWizard.addButton(top._res_286, saveFilter, "button-green", null);
            oModalFilterWizard.addButton(top._res_118, saveFilterAs, "button-gray", null);
        }

    }
    return oModalFilterWizard;
}



function validFilter(nTab, filterId) {
    var updatePref = "tab=" + nTab + ";$;listfilterid=" + filterId;

    updateUserPref(updatePref, function () {

        var invitWizard = eTools.GetModal('InvitWizard');
        if (invitWizard) {

            var invitWizarfFrame = invitWizard.getIframe();


            var trLine = invitWizarfFrame.document.querySelector("tr[eid='104000_" + filterId + "']")
            if (trLine) {
                invitWizarfFrame.selectLine(trLine)
            }
            else {
                invitWizarfFrame.deselectLine()
                invitWizarfFrame._activeFilter = filterId
            }

            invitWizarfFrame.MoveStep(true, "invit");
            invitWizarfFrame.UpdatePPList();
        }
        else {
            loadList();
        }

    });



    try {
        var filterWizard = eTools.GetModal('oModalFilterWizard');

        if (filterWizard && filterWizard != null) {
            filterWizard.hide();
        }
    }
    catch (exp0) { }


    if (oModalFilterList && oModalFilterList != null) {
        try {
            if (oModalFilterList.getIframe().oModalFilterWizard && oModalFilterList.getIframe().oModalFilterWizard != null)
                oModalFilterList.getIframe().oModalFilterWizard.hide();
        }
        catch (exp1) { }

        try {
            if (oModalFilterList.getIframe().oModalFilterForm && oModalFilterList.getIframe().oModalFilterForm != null)
                oModalFilterList.getIframe().oModalFilterForm.hide();
        }
        catch (exp2) { }

        try {
            if (oModalFilterForm && oModalFilterForm != null)
                oModalFilterForm.hide();
        }
        catch (exp3) { }

        oModalFilterList.hide();

    }
}

var FROM_NOWHERE = 0;
var FROM_REPORT = 1;
var FROM_INVIT = 2;
var FROM_WIDGET = 3;

function doFormularFilter(nTab, filterId, iFrom, sAdditionalParams) {
    if (typeof (iFrom) == "undefined") {
        iFrom = FROM_NOWHERE;
    }

    if (filterId.toString() == "0") {
        eAlert(0, top._res_6246, top._res_430);
        return;
    }
    oModalFilterForm = new eModalDialog(top._res_6335, 0, 'eFilterWizard.aspx', 1000, 600);
    oModalFilterForm.addParam("tab", nTab, "post");
    oModalFilterForm.addParam("filterid", filterId, "post");
    oModalFilterForm.addParam("action", 'filterquestion', "post");
    oModalFilterForm.ErrorCallBack = launchInContext(oModalFilterForm, oModalFilterForm.hide);

    oModalFilterForm.show();

    var sParam = nTab + "|" + filterId + "|" + iFrom.toString();
    if (sAdditionalParams)
        sParam = sParam + "|" + sAdditionalParams;

    oModalFilterForm.addButton(top._res_29, cancelFilterFormular, "button-gray", sParam);
    oModalFilterForm.addButton(top._res_219, applyFilterformular, "button-green", sParam);
}

function cancelFilterFormular() {
    oModalFilterForm.hide();
}

function applyFilterformular(filterInfos) {
    var aInfos = filterInfos.split('|');
    var nTab = aInfos[0];
    var filterId = aInfos[1];
    if (aInfos.length >= 3)
        var iFrom = getNumber(aInfos[2]);

    var _ifrm = oModalFilterForm.getIframe();
    var sParams = _ifrm.getEmptyFilterParams();

    var upd = new eUpdater("mgr/eFilterWizardManager.ashx", 0);
    upd.addParam("filterid", filterId, "post");
    upd.addParam("maintab", nTab, "post");
    upd.addParam("emptyfilterparams", sParams, "post");
    upd.addParam("action", "filterquestionprocess", "post");
    upd.addParam("filtername", "libelle filtre", "post");

    //Gestion d'erreur : Fermeture de la modal
    upd.ErrorCallBack = launchInContext(oModalFilterForm, oModalFilterForm.hide);

    var fctAppy;
    switch (iFrom) {
        case FROM_NOWHERE:
            fctAppy = applyFilterformularTreatment;
            break;
        case FROM_REPORT:
            fctAppy = function () {
                if (oModalFilterForm && oModalFilterForm != null)
                    oModalFilterForm.hide();
                //RunReport();
                //Confirmer le titre si necessaire
                ConfirmTitle(_eCurentSelectedFilter);
            }
            break;
        case FROM_INVIT:
            // A priori, on ne passe par dans ce cas
            fctAppy = function () {
                if (oModalFilterForm && oModalFilterForm != null)
                    oModalFilterForm.hide();

                // Fermeture de la modale de filtre
                var filterWizard = eTools.GetModal('oModalFilterWizard');
                if (filterWizard && filterWizard != null) {
                    filterWizard.hide();
                }

                // Chargement du filtre
                var invitWizard = eTools.GetModal('InvitWizard');
                if (invitWizard) {
                    //invitWizard.getIframe().MoveStep(true, "invit");
                    invitWizard.getIframe().UpdatePPList();

                }
                //UpdatePPList();
            }
            break;
        case FROM_WIDGET:
            fctAppy = function () {
                if (oModalFilterForm && oModalFilterForm != null)
                    oModalFilterForm.hide();

                if (aInfos.length >= 4) {
                    var oAdditionalParams = JSON.parse(aInfos[3]);
                    goTabListWithFilter(nTab, filterId, oAdditionalParams.ClearAllFilters, oAdditionalParams.ForceList, oAdditionalParams.Histo);
                }
            }
            break;
        default:

    }
    //if (bFromReport) {

    //    fctAppy = function () {
    //        if (oModalFilterForm && oModalFilterForm != null)
    //            oModalFilterForm.hide();
    //        RunReport();
    //    }
    //}
    //else
    //    fctAppy = applyFilterformularTreatment;

    upd.send(fctAppy, filterInfos);
}

function applyFilterformularTreatment(oRes, filterInfos) {
    var aInfos = filterInfos.split('|');
    var nTab = aInfos[0];
    var filterId = aInfos[1];
    validFilter(nTab, filterId);
    if (oModalFilterForm && oModalFilterForm != null)
        oModalFilterForm.hide();
}
//Fonction appelée au clique sur la liste des formulaires
// 2 cas :    - depuis un signet on modifie le formulaire
//            - depuis un emailing on injecte le lien vers le formulaire
function DblClickFormular(oLineClicked) {

    selectLine(oLineClicked);
    var oMailing = top._md['MailingWizard'];

    //cas du wizard sms
    if (!oMailing)
        oMailing = top._md['SmsMailingWizard'];

    var oMailingValidFunction = null;
    if (oMailing && oMailing.getIframe() && oMailing.getIframe().onValidSelectedForm) {
        oMailingValidFunction = oMailing.getIframe().onValidSelectedForm;
    }
    if (typeof (oMailingValidFunction) == "function" && oMailingValidFunction) {
        //Inject dans le ckeditor
        oMailingValidFunction();
    }
    else {
        //Modification
        parent.onEditSelectedForm();
    }
}

// Fonction appelée au double-clic sur un filtre
function onFilterDblClick(oTr) {
    selectLine(oTr);
    var oModal = top._md['oModalFilterList'];
    var oModalValidFunction = null;

    var btnValid = top.document.getElementById("btnValidFilter");

    if (oModal && btnValid) {
        btnValid.click();
    }
}

function checkIfFormular(nTab, filterId, iFrom, sAdditionalParams) {
    var upd = new eUpdater("mgr/eFilterWizardManager.ashx", 0);
    upd.addParam("filterid", filterId, "post");
    upd.addParam("action", "checkfilterform", "post");
    upd.ErrorCallBack = function () { };

    if (iFrom == null)
        iFrom = FROM_NOWHERE;
    //if (top.eTools.GetModal("InvitWizard")) {
    //    iFrom = FROM_INVIT;
    //}

    var sParam = nTab + "|" + filterId + "|" + iFrom.toString();
    if (sAdditionalParams)
        sParam = sParam + "|" + sAdditionalParams;

    upd.send(checkIfFormularTreatment, sParam);
}



function checkIfFormularTreatment(oRes, filterInfos) {
    var aInfos = filterInfos.split('|');

    var iFrom = FROM_NOWHERE;
    if (aInfos.length >= 3) {
        iFrom = aInfos[2];
    }

    var ifFormular = getXmlTextNode(oRes.getElementsByTagName("isformular")[0]);
    if (ifFormular == "1") {
        var additionalParams = null;
        if (aInfos.length >= 4)
            additionalParams = aInfos[3];
        doFormularFilter(aInfos[0], aInfos[1], iFrom, additionalParams);
    }
    else {
        if (iFrom == FROM_WIDGET) {
            var nTab = aInfos[0];
            var filterId = aInfos[1];
            if (aInfos.length >= 4) {
                var oAdditionalParams = JSON.parse(aInfos[3]);
                goTabListWithFilter(nTab, filterId, oAdditionalParams.ClearAllFilters, oAdditionalParams.ForceList, oAdditionalParams.Histo);
            }
        }
        else
            validFilter(aInfos[0], aInfos[1], iFrom);
    }
}

function cancelAdvFlt(nTab) {
    var updatePref = "tab=" + nTab + ";$;canceladvfilter=0";

    // On créait un object pour renvoyer au setWait ou es ce qu'on veux aller
    var oParamGoTabList = {
        to: 1,
        nTab: nTab,
        context: "eMain.cancelAdvFlt"
    };
    setWait(true, undefined, undefined, isIris(top.getTabFrom()));


    updateUserPref(updatePref, function () { top.loadList(); setWait(false, undefined, undefined, isIris(top.getTabFrom()), oParamGoTabList); });

}


// #FIN REGION FILTER#

var oModalTreatment;
// #DEBUT REGION TRAITEMENTS#
// nTabBkm : si l'on viens d'un BKM cette variable est supérieur à 0 et est égale au TabId du BKM
function treatment(nTab) {

    var targetPageTitle = top._res_295;

    if (oModalTreatment != null)
        oModalTreatment.hide();

    var tab = nGlobalActiveTab;
    if (nTab > 0)
        tab = nTab;



    var nWitdh = 545;
    var nHeight = 500;
    if (top.eTools.GetFontSize() >= 18) {
        nWitdh = 850;
        nHeight = 650;
    }
    else if (top.eTools.GetFontSize() >= 12)
        nWitdh = 675;

    oModalTreatment = new eModalDialog(targetPageTitle, 0, 'eTreatmentDialog.aspx', nWitdh, nHeight, "Treatment");

    oModalTreatment.ErrorCallBack = launchInContext(oModalTreatment, oModalTreatment.hide);
    oModalTreatment.hideMaximizeButton = true;
    oModalTreatment.addParam("tab", nGlobalActiveTab, "post");
    oModalTreatment.addParam("targetTab", tab, "post");
    oModalTreatment.addParam("frmId", oModalTreatment.iframeId, "post");
    //  if (nTabBkm && typeof (nTabBkm) != "undefined")
    //    oModalTreatment.addParam("tabBKM", nTabBkm, "post");
    var oTab = document.getElementById("mt_" + nGlobalActiveTab);
    // on passe à la fenêtre les nombres totaux/affichés de fiches, en prenant soin de retirer l'espace séparateur de milliers (caractère ASCII 160)
    if (document.getElementById("markedFileId_" + nGlobalActiveTab)) {
        oModalTreatment.addParam("markedFileId", document.getElementById("markedFileId_" + nGlobalActiveTab).value, "post");
    }
    if (document.getElementById("markedNb_" + nGlobalActiveTab)) {
        oModalTreatment.addParam("markedFileNb", document.getElementById("markedNb_" + nGlobalActiveTab).value, "post")
    }
    oModalTreatment.addParam("total", getAttributeValue(oTab, "eNbTotal").replace(String.fromCharCode(160), ""), "post");
    closeRightMenu();

    oModalTreatment.show();
    oModalTreatment.addButtonFct(top._res_29, onCloseTreatment, 'button-gray');
    oModalTreatment.addButtonFct(top._res_28, onApplySelectedTreatment, 'button-green');

}

//Fermeture de la liste
function onCloseTreatment() {
    if (oModalTreatment)
        oModalTreatment.hide();
}

//ENREGISTREMENT DU TRAITEMENT DANS SERVERTREATMENT
function onApplySelectedTreatment() {
    var winTreatment = oModalTreatment.getIframe();
    winTreatment.CallExecuteInsertTreatment();

}

///summary
///[MOU 03/09/2013 cf. 22318] 
///Affiche une fenêtre intermediaire pour demander à l'utilisateur de choisir 
///le fichier cible parmi (PP ou Adresse), (PM ou Adresse).
///<param name="nTabAdr">DescId de l'Onglet Adresse</param>
///<param name="nFromTab">DescId de l'Onglet d'ou vient</param>
///summary
function SetTargetTab(nTabAdr, nFromTab, CallBackFct) {

    var MSG_QUESTION = 1;

    var hvFromTab = document.getElementById("hvFromTab_" + nFromTab);
    var hvAdrTab = document.getElementById("hvAdr_" + nTabAdr);

    var modal = eConfirm(MSG_QUESTION, top._res_295, top._res_649, "", 450, 200,

        function () { CallBackFct(modal.getSelectedValue()); }, null, true);

    modal.isMultiSelectList = false;
    if (nGlobalActiveTab == nFromTab) {
        modal.addSelectOption(top._res_6446 + " " + getAttributeValue(hvFromTab, "value"), nFromTab, true);
        modal.addSelectOption(top._res_6446 + " " + getAttributeValue(hvAdrTab, "value"), nTabAdr, false);
    }
    modal.createSelectListCheckOpt();

}

// # FIN REGION TRAITEMENTS #



var oModalReportList;
var oModalReportForm;
// #DEBUT REGION REPORT#
//nReportType : type de rapport demandé
//nTabBkm : si 0 > l'export est lancé depuis le tabid du signet indiqué
function reportList(nReportType, nTabBkm, nSelectedReportId) {
    var targetPageTitle = top._res_6299;
    var ReportTypeLabel = "";

    switch (parseInt(nReportType)) {
        case 99: // ASY : Tous
            ReportTypeLabel = top._res_22;
            break;

        case 0: // Impression
            // Impression mode fiche
            ReportTypeLabel = top._res_412;
            break;
        case 7:
            ReportTypeLabel = top._res_412; // TODO ? : Creer une Res pour les rapports personnalisables : Impression mode fiche
            break;
        case 1: // inexistant dans les constantes.
            break;
        case 2: // Exports
            ReportTypeLabel = top._res_413;
            break;
        case 3: // Publipostage
            ReportTypeLabel = top._res_438;
            break;

        case 4:  // Etats Spécifiques
            ReportTypeLabel = top._res_6405;
            break;
        case 5: // Faxing
            ReportTypeLabel = top._res_1109;
            break;
        case 6: // Graphiques
            ReportTypeLabel = top._res_1005;
            break;
        case 10: // Graphiques(paramétrage des champs de type Graphique)
            targetPageTitle = top._res_7696;
            break;

        case 8: // Rapports de Page d'accueil
            break;
    }

    var size = getWindowSize();
    var nPopupWidth = 927;
    var bTabletMode = isTablet();
    //var bTabletMode = true;
    if (bTabletMode) {
        if (Number(size.w) < 1024)
            nPopupWidth = 750;
    }

    var oModalReportListFct = function () {
        targetPageTitle = targetPageTitle.replace("<REPORTTYPE>", ReportTypeLabel);
        oModalReportList = new eModalDialog(targetPageTitle, 0, 'eFilterReportListDialog.aspx', nPopupWidth, 600, "ReportList");
        oModalReportList.ErrorCallBack = launchInContext(oModalReportList, oModalReportList.hide);
        oModalReportList.addParam("type", nReportType, "post");
        oModalReportList.addParam("width", nPopupWidth, "post");
        oModalReportList.addParam("height", 600, "post");
        oModalReportList.addParam("tab", nGlobalActiveTab, "post");
        oModalReportList.addParam("frmId", oModalReportList.iframeId, "post");
        oModalReportList.addParam("lstType", 13, "post");

        var bFromBKM = false;
        var bBkmFile = false;         // signet mode fiche

        if (nTabBkm && typeof (nTabBkm) != "undefined" && nTabBkm != "0" && nTabBkm != "-1") {
            bFromBKM = true;
            bBkmFile = isBkmFile(nTabBkm);
            oModalReportList.addParam("tabBKM", nTabBkm, "post");
        }

        var currentView = top.getCurrentView();
        var bFile = (currentView.indexOf("FILE_") == 0);

        var bkmFid = "";

        //Mode Fiche (hors signet)
        if (bFile && !bFromBKM) {
            fid = parent.document.getElementById("fileDiv_" + parent.nGlobalActiveTab).attributes["fid"].value;    //File Id du mode fiche en cours
            oModalReportList.addParam("fid", fid, "post");
        }
        // Signet mode fiche
        if (bBkmFile) {
            bkmFid = parent.document.getElementById("fileDiv_" + nTabBkm).attributes["fid"].value;    //File Id du mode fiche en cours
            oModalReportList.addParam("bkmfid", bkmFid, "post");
        }

        closeRightMenu();

        if (nReportType == 10) //paramétrage d'un champ de type graphique
            oModalReportList.onIframeLoadComplete = (function (modal, nReportId) {
                return function () { modal.getIframe().nsFilterReportList.SelectReport(nReportId) };
            })(oModalReportList, nSelectedReportId);

        oModalReportList.show();

        oModalReportList.addButtonFct(top._res_29, onCloseReportList, 'button-gray');
        if (nReportType == 10) //paramétrage d'un champ de type graphique
            oModalReportList.addButtonFct(top._res_28, (function (modal) { return function () { nsAdminField.GetSelectedChart(modal) }; })(oModalReportList), 'button-green');
        else
            oModalReportList.addButtonFct(top._res_219, onApplySelectedReport, 'button-green');

        oModalReportList.addButtonFct(top._res_151, onEditSelectedReport, 'button-gray', "editReport");
        // oModalReportList.addButtonFct("Planifier", function () { oModalReportList.getIframe().nsFilterReportList.onPlanifyList(oModalReportList); }, 'button-gray', "scheduleReport");

    }

    if (oModalReportList != null)
        oModalReportList.hide(oModalReportListFct);
    else
        oModalReportListFct();
}




function GetObjectFrameName(obj) {

    try {
        for (var i = 0; i < top.frames.length; i++) {
            if (top.frames[i].document == obj.ownerDocument) {
                if (top.frames[i].name)
                    return top.frames[i].name;
                else
                    return "";

            }
        }
    }
    catch (err) {
        return "";
    }


    return "";
}

//Marketing Automation
var modalWizard;
function addAutomation(tab, parentTab, parentFileId, scenarioId) {
    var sWidth, sHeight;
    var browser = new getBrowser();

    if (parentFileId == 0)
        parentFileId = Number(getAttributeValue(document.getElementById('mainDiv'), "pfid"));

    if (!browser.isIE && !browser.isEdge) {
        sWidth = '100%';
        sHeight = '100%';
        modalWizard = new eModalDialog(top._res_1908, 11, null, sWidth, sHeight);
        modalWizard.noButtons = true;

        var divContent = document.createElement("div");
        divContent.id = "app_assist_workflow";
        modalWizard.setElement(divContent);
        modalWizard.show(undefined, undefined, undefined, 0);

        if (scenarioId) {
            addScript("../IRISBlack/Front/scripts/components/workflowMarketing/appLoader/eInitWorkflow", "WORKFLOWMARKETING", function () {
                window.top.InitializeWorkflow(modalWizard, null, null, null, scenarioId);
            }, window.top.document);
        }
        else {
            addScript("../IRISBlack/Front/scripts/components/workflowMarketing/appLoader/eInitWorkflow", "WORKFLOWMARKETING", function () {
                window.top.InitializeWorkflow(modalWizard, tab, parentTab, parentFileId);
            }, window.top.document);
        }
        
    }
}

///Ouvre la liste des formulaires
///<param name="tab">descid de ++ ou cible etendu</param>
var oModalFormularList;
var oMemoJSVarName = null;
function ShowFormularList(tabBKM, parentFileId, oParamMemoJSVarName, formularType, fileRoot) {
    oMemoJSVarName = null;
    var nPopupWidth = 927;
    var bTabletMode = isTablet();
    //var bTabletMode = true;
    if (bTabletMode) {
        if (Number(document.width) < 1024)
            nPopupWidth = 750;
    }

    var nRows = GetNumRows(350);
    //SHA : Titre de la popup de la liste des formulaires selon le type de formulaire
    switch (formularType) {
        case null:
        case undefined:
        default:
        //Formulaire simple
        case -1:
        case 0:
            oModalFormularList = new eModalDialog(top._res_6127, 0, 'eFilterReportListDialog.aspx', nPopupWidth, 500, "oModalFormularList");
            oModalFormularList.addParam("type", 0, "post");
            break;
        //Formulaire Avancé
        case 1:
            oModalFormularList = new eModalDialog(top._res_2449, 0, 'eFilterReportListDialog.aspx', nPopupWidth, 500, "oModalFormularList");
            oModalFormularList.openMaximized = true
            oModalFormularList.addParam("type", 1, "post");//KJE: tâche 2076, on renseigne le type de formulaire (classique ou vancé)
            break;
    }

    oModalFormularList.ErrorCallBack = launchInContext(oModalFormularList, oModalFormularList.hide);


    oModalFormularList.addParam("rows", nRows, "post");
    oModalFormularList.addParam("page", 1, "post");
    oModalFormularList.addParam("tab", "113000", "post");
    oModalFormularList.addParam("tabBKM", tabBKM, "post");
    oModalFormularList.addParam("parentFileId", parentFileId, "post");
    oModalFormularList.addParam("frmId", oModalFormularList.iframeId, "post");
    oModalFormularList.addParam("lstType", 18, "post");
    //AAB tâche 1882
    oModalFormularList.addParam("formularType", formularType, "post");
    oModalFormularList.formularType = formularType;
    var bFromMailing = false;
    if (typeof (oParamMemoJSVarName) != "undefined" && oParamMemoJSVarName != 0) {
        bFromMailing = true;
        oModalFormularList.addParam("fromemailing", "1", "post");
        oModalFormularList._oMemoJSVarName = oParamMemoJSVarName;  //appel depuis les modèles d'emailings+emailings
        oMemoJSVarName = oParamMemoJSVarName;
    }


    oModalFormularList.show();


    if (bFromMailing) {
        oModalFormularList.addButtonFct(top._res_28, onValidSelectedForm, 'button-green', 'ok');
        oModalFormularList.addButtonFct(top._res_29, ()=> onCloseFormularList(fileRoot), 'button-gray');
    }
    else {
        //Modifier pas depuis emailing
        oModalFormularList.addButtonFct(top._res_29, ()=> onCloseFormularList(fileRoot), 'button-gray');
        oModalFormularList.addButtonFct(top._res_151, onEditSelectedForm, 'button-gray', 'fltLstModBtn');
    }
    oModalFormularList.AddCancelMethode(()=> onCloseFormularList(fileRoot) )
}
function onValidSelectedForm() {
    var oCurrentModalFormularList = top._md["oModalFormularList"];  //appel depuis les modèles d'emailings+emailings
    var winFormList = oCurrentModalFormularList.getIframe();
    if (winFormList.InjectMailingSelectedForm(oCurrentModalFormularList._oMemoJSVarName)) {
        if (oCurrentModalFormularList != null && oCurrentModalFormularList)
            oCurrentModalFormularList.hide();
    }
}
function onCloseFormularList(fileRoot) {
    if (fileRoot) reloadNewFileMode(fileRoot)
    if (oModalFormularList != null && oModalFormularList)
        oModalFormularList.hide();
}
function onEditSelectedForm() {
    var winFormList = top._md["oModalFormularList"].getIframe();
    winFormList.EditSelectedForm();
}

/** Permet de recharger le nouveau mode liste */
function reloadNewFileMode(fileRoot){
    if (fileRoot)
        fileRoot.bDisplayed = true;
}

//Fermeture de la liste
function onCloseReportList() {
    if (oModalReportList != null && oModalReportList)
        oModalReportList.hide();
}

//Edition du Rapport
function onEditSelectedReport() {
    var winReportList = oModalReportList.getIframe();
    winReportList.editReport();
}


///Ouvre la liste des modèles de mails dans une fenêtre séparée
var oModalMailTemplateList;
var oSubjectJSVarName = null;
// oMemoJSVarName est déjà définie plus haut (ShowFormularList)
// #68 13x et Backlog #375 - Editeur de templates HTML avancé (grapesjs) - Uniquement accessible en E2017 - Ajout du paramètre enableTemplateEditor
function ShowMailTemplateList(mtType, tabLabel, tabBKM, parentFileId, oParamSubjectJSVarName, oParamMemoJSVarName, enableTemplateEditor) {

    var nPopupWidth = 927;
    var bTabletMode = isTablet();
    //var bTabletMode = true;
    if (bTabletMode) {
        if (Number(document.width) < 1024)
            nPopupWidth = 750;
    }

    var nRows = GetNumRows(350);


    var sWindowTitle = mtType > 2 ? top._res_6318 : top._res_3048;

    if (tabLabel && tabLabel != '')
        sWindowTitle += ' - ' + tabLabel;

    //TODORES : "Choisir un modèle de mail" pas tout à fait adapté
    oModalMailTemplateList = new eModalDialog(sWindowTitle, 0, 'eFilterReportListDialog.aspx', nPopupWidth, 525, "oModalMailTemplateList");
    oModalMailTemplateList.ErrorCallBack = launchInContext(oModalMailTemplateList, oModalMailTemplateList.hide);

    oModalMailTemplateList._mtType = mtType; // pour le bon fonctionnement du bouton Modifier
    oModalMailTemplateList.addParam("type", mtType, "post");
    oModalMailTemplateList.addParam("rows", nRows, "post");
    oModalMailTemplateList.addParam("page", 1, "post");
    oModalMailTemplateList.addParam("tab", "107000", "post"); // MAILTEMPLATE
    oModalMailTemplateList.addParam("tabBKM", tabBKM, "post");
    oModalMailTemplateList.addParam("parentFileId", parentFileId, "post");
    oModalMailTemplateList.addParam("frmId", oModalMailTemplateList.iframeId, "post");
    oModalMailTemplateList.addParam("lstType", 15, "post"); // ednType.
    if (typeof (oParamMemoJSVarName) != "undefined") {
        if (typeof (oParamSubjectJSVarName) == "string") {
            oModalMailTemplateList._oSubjectJSVarName = eval(oParamSubjectJSVarName); // lien vers le champ Objet qu'il faudra mettre à jour avec le modèle
            oSubjectJSVarName = eval(oParamSubjectJSVarName);
        }
        else {
            oModalMailTemplateList._oSubjectJSVarName = oParamSubjectJSVarName; // lien vers le champ Mémo qu'il faudra mettre à jour avec le modèle
            oSubjectJSVarName = oParamSubjectJSVarName;
        }
        if (typeof (oParamMemoJSVarName) == "string") {
            oModalMailTemplateList._oMemoJSVarName = eval(oParamMemoJSVarName); // lien vers le champ Mémo qu'il faudra mettre à jour avec le modèle
            oMemoJSVarName = eval(oParamMemoJSVarName);
        }
        else {
            oModalMailTemplateList._oMemoJSVarName = oParamMemoJSVarName; // lien vers le champ Mémo qu'il faudra mettre à jour avec le modèle
            oMemoJSVarName = oParamMemoJSVarName;
        }
    }
    oModalMailTemplateList._enableTemplateEditor = enableTemplateEditor; // #68 13x et Backlog #375 - Editeur de templates HTML avancé (grapesjs) - Uniquement accessible en E2017

    oModalMailTemplateList.show();

    oModalMailTemplateList.addButtonFct(top._res_29, onCloseMailTemplateList, 'button-gray');
    oModalMailTemplateList.addButtonFct(top._res_219, onApplySelectedMailTemplate, 'button-green');

    if (mtType < 2)
        oModalMailTemplateList.addButtonFct(top._res_151, onEditSelectedMailTemplate, 'button-gray');
}
function onApplySelectedMailTemplate() {
    var oCurrentModalMailTemplateList = top._md["oModalMailTemplateList"];  //appel depuis les modèles d'emailings+emailings
    var winMailTemplateList = oCurrentModalMailTemplateList.getIframe();
    var nSelectedMailTplId = getNumber(getAttributeValue(winMailTemplateList._eCurrentSelectedMailTpl, "mtid"));
    // Comme sur les autres fenêtres (rapports/exports/etc.). Si aucun modèle n'est sélectionné, on ne fait rien. Autrement, ici, appeler LoadTemplate() sans sélection provoquerait l'application du premier modèle de mail de la liste.
    if (nSelectedMailTplId > 0) {
        var oMailing = winMailTemplateList.oMailing;
        if (oMailing)
            oMailing.LoadTemplate(oCurrentModalMailTemplateList);
        // La fermeture de la fenêtre sera gérée par LoadTemplate() et son eUpdater
    }
}
function onCloseMailTemplateList() {
    if (oModalMailTemplateList != null && oModalMailTemplateList)
        oModalMailTemplateList.hide();
}
function onEditSelectedMailTemplate() {
    var oCurrentModalMailTemplateList = top._md["oModalMailTemplateList"];
    var winMailTemplateList = oCurrentModalMailTemplateList.getIframe();
    var nSelectedMailTplId = getNumber(getAttributeValue(winMailTemplateList._eCurrentSelectedMailTpl, "mtid"));
    // Comme sur les autres fenêtres (rapports/exports/etc.). Si aucun modèle n'est sélectionné, on ne fait rien. Autrement, ici, appeler EditMailTemplate() avec un id à 0 serait équivalent à Ajouter un nouveau modèle.
    if (nSelectedMailTplId > 0)
        winMailTemplateList.EditMailTemplate(nSelectedMailTplId, oCurrentModalMailTemplateList._mtType, oCurrentModalMailTemplateList._enableTemplateEditor, oCurrentModalMailTemplateList._useNewUnsubscribeMethod); // #68 13x et Backlog #375 - Editeur de templates HTML avancé (grapesjs) - Uniquement accessible en E2017
}


///
/// <summary>
/// Types d'emailing
/// </summary>
TypeMailing = {
    /// <summary>Type d'emailing non encore défini</summary>
    MAILING_UNDEFINED: -1,
    //TODO - FIXME A mettre en commun avec l enum FROM de eCampaignSelection
    /// <summary>liste</summary>
    MAILING_FROM_LIST: 0,
    /// <summary>++ ou cible étendue</summary>
    MAILING_FROM_BKM: 1,
    /// <summary>Mode Fiche/Campagne</summary>
    MAILING_FROM_CAMPAIGN: 2,
    /// <summary>Envoi d'une campagne mailing sms depuis un ++</summary>
    SMS_MAILING_FROM_BKM: 3,
    /// <summary>Envoi de SMS non encore défini</summary>
    SMS_MAILING_UNDEFINED: 4,
    /// <summary>++ ou cible étendue pour marketing automation</summary>
    MAILING_FOR_MARKETING_AUTOMATION: 5,
};


/// Creation d'un nouveau message sms
function AddSmsMailing(nCalledTab, mailingType) {
    AddMailing(nCalledTab, mailingType, 0);
}

///summary
///Ouvre l'assistant emailing en mode création
///<param name="tab">Onglet en cours</param>
///<param name="mailingType">Type de mailing</param>
///summary
var FROM_BKM = 1;
function AddMailing(tab, mailingType, campId, parentTab, parentFileId) {
    var waitHandler = null;
    if (typeof (campId) == "undefined")
        campId = 0;

    if (!parentFileId) {
        parentFileId = 0;
        var currentFile = document.getElementById("mainDiv");
        if (currentFile) {
            var oC = currentFile.querySelector("#fileDiv_" + nGlobalActiveTab + "[did='" + nGlobalActiveTab + "']");
            parentFileId = getAttributeValue(oC, "fid");
        }
    }

    // On force MaxOrMinModal, ensuite, inutile de passer le width et height
    var strWidth = null;//'95%';
    var strHeight = null;//'95%';
    var strTilte = top._res_6391;
    if (mailingType == TypeMailing.SMS_MAILING_FROM_BKM) {
        strWidth = '95%';//900
        strHeight = '95%';//630
        strTilte = top._res_6858;
    }

    var wizardId = "MailingWizard";
    if (mailingType == TypeMailing.SMS_MAILING_FROM_BKM)
        wizardId = "SmsMailingWizard";

    modalWizard = new eModalDialog(strTilte, 0, "eWizard.aspx", strWidth, strHeight, wizardId);
    modalWizard.hideMaximizeButton = true;


    // TODO - Param pourri, ne pas prendre ne compte Faut regarder du côté des width/height/divMainWidth/divMainHeight déjà envoyé en param par l'objet modal
    // Attention, si resize, que fait-on ?
    modalWizard.addParam("width", strWidth, "post");
    modalWizard.addParam("height", strHeight, "post");
    modalWizard.addParam("docwidth", top.document.body.offsetWidth, "post");
    modalWizard.addParam("docheight", top.document.body.offsetHeight, "post");
    // Fin de param pourri

    modalWizard.addParam("wizardtype", "mailing", "post");
    modalWizard.addParam("tab", tab, "post");

    modalWizard.addParam("parenttab", (parentTab) ? parentTab : nGlobalActiveTab, "post");

    modalWizard.addParam("mtype", mailingType, "post");
    modalWizard.addParam("campId", campId, "post");
    modalWizard.addParam("parentfileid", parentFileId, "post");
    modalWizard.addParam("operation", 1, "post");
    modalWizard.addParam("frmId", modalWizard.iframeId, "post");
    modalWizard.addParam("modalId", modalWizard.iframeId, "post");
    modalWizard.addParam("iframeScrolling", "yes", "post");

    modalWizard.ErrorCallBack =
        function () {
            waitHandler.hide();
            modalWizard.hide();
        };

    //
    modalWizard.show();

    if (mailingType != TypeMailing.SMS_MAILING_FROM_BKM)
        modalWizard.MaxOrMinModal();

    modalWizard.addButton(top._res_6471, "modalWizard.getIframe().oMailing.SaveCampaign();", "button-gray", null, "savecampaign_btn", 'left');
    modalWizard.addButton(mailingType == TypeMailing.SMS_MAILING_FROM_BKM ? top._res_1880 : top._res_6163, "modalWizard.getIframe().oMailing.SendTestMail();", "button-gray", null, "mailtest_btn", 'left');

    modalWizard.addButton(top._res_6472,

        (function (obj) {
            return function () {
                obj.getIframe().oMailing.SaveAsTemplate(obj);
            }
        })(modalWizard)

        , "button-gray", null, "savemodel_btn", 'left');

    // max
    if (mailingType == TypeMailing.SMS_MAILING_FROM_BKM) {

        //Bouton de validation
        modalWizard.addButtonFct(top._res_30, function () { modalWizard.getIframe().oSmsing.Cancel(); }, "button-gray", "cancel_btn");
        modalWizard.addButtonFct(top._res_944, function () { modalWizard.getIframe().oSmsing.Send(); }, "button-green", "save_btn");

        //Bouton d'étapes
        modalWizard.addButtonFct(top._res_26, function () { modalWizard.getIframe().MoveStep(true, 'smsing'); }, "button-green-rightarrow", "next_btn");
        modalWizard.addButtonFct(top._res_25, function () { modalWizard.getIframe().MoveStep(false, 'smsing'); }, "button-gray-leftarrow", "previous_btn");

    } else {
        modalWizard.addButton(top._res_30, "CloseMailingWizard(modalWizard);", "button-gray", null, "cancel_btn", true);
        if (mailingType != TypeMailing.MAILING_FOR_MARKETING_AUTOMATION)
            modalWizard.addButton(top._res_944, "modalWizard.getIframe().oMailing.SendCampaign(modalWizard.getIframe().ControlWizardDisplay(modalWizard.getIframe().iCurrentStep, modalWizard.getIframe().strCurrentWizardType, modalWizard.getIframe().oCurrentWizard.GetType()));", "button-green", null, "save_btn");
        else
            modalWizard.addButton(top._res_5003, "modalWizard.getIframe().oMailing.SendCampaign(modalWizard.getIframe().ControlWizardDisplay(modalWizard.getIframe().iCurrentStep, modalWizard.getIframe().strCurrentWizardType, modalWizard.getIframe().oCurrentWizard.GetType()));", "button-green", null, "save_btn");


        modalWizard.addButton(top._res_26, "modalWizard.getIframe().MoveStep(true, 'mailing');modalWizard.getIframe().oMailing.ControlDefaultTemplate();", "button-green-rightarrow", null, "next_btn");
        modalWizard.addButton(top._res_25, "modalWizard.getIframe().MoveStep(false, 'mailing');", "button-gray-leftarrow", null, "previous_btn");
    }



    HideModalButtons();

    //Message d'attente au chargement
    waitHandler = DisplayWaitDialog(modalWizard);
}

///summary
///Ferméture du wizard de la campagne mail.
///<param name="modalWizard">La fenêtre à fermer</param>
///summary
function CloseMailingWizard(modalWizard) {

    if (modalWizard) {

        var me = modalWizard.getIframe().oMailing;
        // Pas oMailing dans le cas de SMS en mass
        if (me) {
            if (me._mailingId == 0)
                DeletePJ(me._nCampaignTab, 0, me._pjIds, false, null);

            if (me._oWaitDialog != null)
                me._oWaitDialog.hide();
        }
        modalWizard.hide();
    }
}

///summary
///Ouvre le contenu de l'email de la campagne en lecture seule.
///<param name="mailingType">Type de mailing</param>
///<param name="campId">id de la campagne</param>
///summary
function ReadMailing(mailingType, campId) {
    var tab = 106000;
    var title = top._res_14;

    setWait(true);

    //affichage de la popup
    shFileInPopup(tab, campId, title, 0, 0, 0, null, true, function () { setWait(false); }, CallCampaignMail);
}

function ReadSMSMailing(mailingType, campId) {
    var lWidth = 900;//'95%';
    var lHeight = 600;//'95%';
    var strTitle = top._res_6858;
    var tab = 106000;

    setWait(true);

    eModFile = new eModalDialog(strTitle, 0, "eFileDisplayer.aspx", lWidth, lHeight, "popupFile");
    eModFile.onHideFunction = function () { eModFile = null; };
    eModFile.CallFrom = tab;
    eModFile.fileId = campId;
    eModFile.addParam("fileid", campId, "post");
    eModFile.tab = tab;
    eModFile.addParam("tab", tab, "post");
    // Passage de la taille de la fenêtre en Request.Form pour adapter le contenu en fonction dans eFileDisplayer (template E-mail)
    eModFile.addParam("width", lWidth, "post");
    eModFile.addParam("height", lHeight, "post");
    eModFile.addParam("bSms", '1', "post");//sms   
    eModFile.addParam("iframeScrolling", "yes", "post");
    eModFile.addParam("campaignreadonly", "2", "post");
    eModFile.addParam("mailingType", 4, "post");
    eModFile.hideMaximizeButton = true;

    eModFile.ErrorCallBack = function () {
        top.setWait(false);
        eModFile.hide();
        eModFile = null;
    };
    eModFile.onIframeLoadComplete = function () {
        top.setWait(false);
    };

    eModFile.show();

    eModFile.addButton(top._res_30, function () { eModFile.hide(); }, "button-green");      // Fermer
}


///summary
/// Affiche la fenêtre d'attente
///(important l'ordre d'appel : DisplayWaitDialog est appelée apres que oModalDialog soit affiché - ceci est lié au z-index )
///<param name="modalHandler">Réference à la fenetre</param>
///summary
function DisplayWaitDialog(oModalDialog) {

    //Message d'attente
    var waitTitle = top._res_307; //Veuillez patientez 
    var waitMessage = top._res_644; //Chargement en cours ...
    var waitHandler = showWaitDialog(waitTitle, waitMessage); //depuis eTools.js

    //Le contenu de l'iframe est complement chargé, on ferme la fenêtre d'attente
    if (oModalDialog) {
        //attente 250ms afin de voir apparaitre le contenu
        oModalDialog.onIframeLoadComplete = function () { setTimeout(waitHandler.hide, 250); }

        //En cas de feremeture de la popup (cas d'erreur)on ferme la fenêtre d'attente
        addTrigger(oModalDialog, "hide", waitHandler.hide); //depuis eTools.js
    }

    return waitHandler;
}

///summary
///Ajout/Suppression de plusieurs destinataires à partir d’un filtre se déroule en 2 étapes
///<param name="tab">Onglet en cours</param>
///<param name="bDelete">Suppression depuis un filtre</param>
///summary
function ActionFromFilter(tab, sLibelle, bDelete) {


    if (typeof bDelete == "undefined")
        bDelete = false;

    // Fiche en cours
    var currentFile = document.getElementById("fileDiv_" + nGlobalActiveTab);
    if (currentFile && currentFile.tagName && currentFile.tagName.toLowerCase() != "div")
        return;

    //DescId & Id de la fiche en cours
    var nFileId = currentFile.getAttribute("fid");

    modalWizard = new eModalDialog(bDelete ? top._res_529 : top._res_428, 0, "eWizard.aspx", 950, 750, "InvitWizard"); // Supprimer/Ajouter à partir d'un filtre
    modalWizard.EudoType = ModalEudoType.WIZARD.toString(); // Type Wizard
    modalWizard.addParam("tab", tab, "post");                   // Descid des invitations
    modalWizard.addParam("tabfrom", nGlobalActiveTab, "post");  // DescId de l'event parent
    modalWizard.addParam("fileidfrom", nFileId, "post");        // Id de l'event parent
    modalWizard.addParam("operation", 1, "post");               // Opération de l'invitation à partir d'un filtre

    modalWizard.addParam("delete", bDelete ? "1" : "0", "post");    // 0 = Ajout       1 = Suppression

    modalWizard.addParam("type", 0, "post");
    modalWizard.addParam("nPage", 1, "post");
    modalWizard.addParam("wizardtype", "invit", "post");                // Type de wizard
    modalWizard.addParam("frmId", modalWizard.iframeId, "post");        // Id de la frame du wizard
    modalWizard.addParam("modalId", modalWizard.iframeId, "post");      // Id de la frame du wizard  TODO : Pourquoi en double ??
    modalWizard.addParam("iframeScrolling", modalWizard.EudoType == "0" ? "no" : "yes", "post");

    modalWizard.show();

    var fctValidate;
    if (!bDelete)
        fctValidate = function () {
            modalWizard.getIframe().nsInvitWizard.FileFromId = nFileId;
            modalWizard.getIframe().nsInvitWizard.AddInvit(tab, sLibelle);
        };
    else
        fctValidate = function () {
            modalWizard.getIframe().nsInvitWizard.SuppInvit(tab);
        };


    modalWizard.addButton(top._res_28, fctValidate, "button-green", null, "inviteValidate_btn");  //Valider    
    modalWizard.addButton(top._res_29, eTools.GetHideModalFct(modalWizard, false), "button-gray", null, "cancel_btn", true);
    modalWizard.addButton(top._res_26, function () { modalWizard.getIframe().MoveStep(true, 'invit'); }, "button-green-rightarrow", null, "next_btn");  //Suivant
    modalWizard.addButton(top._res_25, function () { modalWizard.getIframe().MoveStep(false, 'invit'); }, "button-gray-leftarrow", null, "previous_btn");  // Précédent


    HideModalButtons();
}


function openSelectionByFiltersModal(nTabDescId, nBkmDescId, sBkmLabel) {

    var modalWizard = new eModalDialog("Ajouter à partir des critères de la fiche " + sBkmLabel, 0, "eWizard.aspx", 1250, 900, "SelectionWizard");
    modalWizard.EudoType = ModalEudoType.WIZARD.toString();
    modalWizard.addParam("tab", nBkmDescId, "post");
    modalWizard.addParam("tabfrom", nTabDescId, "post");
    //modalWizard.addParam("fileidfrom", nFileId, "post");        // Id de l'event parent
    //modalWizard.addParam("operation", 1, "post");               // Opération de 
    modalWizard.addParam("type", 0, "post");
    modalWizard.addParam("nPage", 1, "post");
    modalWizard.addParam("wizardtype", "selection", "post");                // Type de wizard
    modalWizard.addParam("frmId", modalWizard.iframeId, "post");        // Id de la frame du wizard
    modalWizard.addParam("modalId", modalWizard.iframeId, "post");      // Id de la frame du wizard  TODO : Pourquoi en double ??
    modalWizard.addParam("iframeScrolling", "yes", "post");

    modalWizard.show();

    modalWizard.addButton(top._res_29, eTools.GetHideModalFct(modalWizard, false), "button-gray", null, "cancel_btn", true);
    modalWizard.addButton(top._res_26, function () { modalWizard.getIframe().MoveStep(true, 'selection'); }, "button-green-rightarrow", null, "next_btn");  //Suivant
    modalWizard.addButton(top._res_25, function () { modalWizard.getIframe().MoveStep(false, 'selection'); }, "button-gray-leftarrow", null, "previous_btn");  // Précédent


}

///summary
///Cache les boutons sur la modal afin qu'on ne les vois pas au premier chargement et qu'on affiche uniquement le nécessaire par la suite
/// TODO : Déplacer cette fonction dans eModal.js
///summary
/// [Obsolete utiliser eModalDialog.hideButtons]
function HideModalButtons() {
    var buttonModal = window.parent.document.getElementById("ButtonModal" + modalWizard.iframeId.replace("frm", ""));
    ///On parcours les div du conteneur des boutons, et pour chaque div ayant un ID de type ******_btn , donc un bouton.
    ///Et on les masque.
    var buttons = buttonModal.getElementsByTagName("div");
    for (iBtn = 0; iBtn < buttons.length; iBtn++) {
        if (buttons[iBtn].id.indexOf("_btn") > 0 && buttons[iBtn].id.indexOf("_btn") + 4 == buttons[iBtn].id.length)
            buttons[iBtn].style.display = "none";

    }
}



var modalCharts = null; //modalCharts est définie au niveau de la window principale

///Affiche une popup avec le rapport demandé
function displayChart(nChartId) {
    var size = top.getWindowSize();
    top.modalCharts = new eModalDialog(top._res_1005, 0, "eChartDialog.aspx", size.w, size.h, "myChart_" + nChartId);
    top.modalCharts.addParam("reportid", nChartId, "post");
    top.modalCharts.ErrorCallBack = function () { top.modalCharts.hide(); };
    top.modalCharts.hideMaximizeButton = true;
    top.modalCharts.show();
    top.modalCharts.addButton(top._res_30, function () { top.modalCharts.hide(); }, 'button-gray', null);
    //top.modalCharts.noButtons = true;
    top.modalCharts.MaxOrMinModal();
}

//ENREGISTREMENT DU RAPPORT DANS SERVERREPORTS
function onApplySelectedReport() {

    var winReportList = oModalReportList.getIframe();
    winReportList.CallExecuteInsert();
}




// #FIN REGION REPORT#

function hideActions(sBtn) {

    var btnD = document.getElementById("aD" + sBtn);
    var btnM = document.getElementById("aM" + sBtn);
    shBtnAction(sBtn, false);

    var oActionMenuMouseOver = function () {

        var actionOut = setTimeout(
            function () {

                if (oActionMenu) {
                    oActionMenu.hide();
                    oActionMenu = null;
                }
            }
            , 200);

        //Annule la disparition            
        if (btnD) {
            setEventListener(btnD, "mouseover", function () { clearTimeout(actionOut); });
        }
        if (btnM) {
            setEventListener(btnM, "mouseover", function () { clearTimeout(actionOut); });
        }
        if (oActionMenu) {
            setEventListener(oActionMenu.mainDiv, "mouseover", function () { clearTimeout(actionOut); });
        }
    };

    if (btnD) {
        setEventListener(btnD, "mouseout", oActionMenuMouseOver);
    }
    if (btnM) {
        setEventListener(btnM, "mouseout", oActionMenuMouseOver);
    }

}

var oActionMenu = null;
var oManuTraitment;
function displayActions(sBtn) {

    // Bouton apparence actif
    shBtnAction(sBtn, true);


    var btnD = document.getElementById("aD" + sBtn);
    var btnM = document.getElementById("aM" + sBtn);

    //Position initiale hors s
    oActionMenu = new eContextMenu(null, -999, -999, 'ActionMenu');

    //Fonctions sur les fiches marquées
    //  les boutons doivent déclenche la fermeture du bouon "action"
    var fctBuilder = function (nType) {
        return function () { shBtnAction(sBtn, false); eMFEObject.markedFile(nType, oActionMenu); }
    };

    var cancelMarkedFiles = function () {
        updateUserPref("tab=" + nGlobalActiveTab + ";$;cancelmarkedfile=0", loadList)
    };

    //
    var nId = 0;
    var sLabel;
    var nbMarkedFiles;

    if (typeof (nGlobalActiveTab) != "undefined") {
        var oMFId = document.getElementById("markedFileId_" + nGlobalActiveTab);
        if (oMFId) {

            var oMFLabel = document.getElementById("markedLabel_" + nGlobalActiveTab)
            var oMFNb = document.getElementById("markedNb_" + nGlobalActiveTab)

            nId = oMFId.value;
            sLabel = oMFLabel.value;
            nbMarkedFiles = oMFNb.value;

            if (sLabel == '')
                sLabel = '<i>' + top._res_6214 + '</i>';

            // Label
            if (sLabel != '' && nId > 0) {
                oActionMenu.addLabel(top._res_6259, 1, 0, "actionLabel"); //Sélections en cours
                oActionMenu.addItemFct(sLabel, cancelMarkedFiles, 1, 0, "actionItem currSel", null, "icon-rem_filter");
                oActionMenu.addSeparator(1);
            }
        }
    }

    // "Sélections"
    oActionMenu.addLabel(top._res_6260, 1, 0, "actionLabel");

    // Afficher la fenêtre de sélection des fiches marquées
    //top._res_587 Ouvrir
    oActionMenu.addItemFct(top._res_587, function () { shBtnAction(sBtn, false); eMFEObject.openDialog(1); }, 1, 0, "actionItem");


    // N'affiche les options que s'il y a une sélection active
    if (nId > 0) {
        // Enregistrer les sélections
        oActionMenu.addItemFct(top._res_118, function () { shBtnAction(sBtn, false); eMFEObject.openDialog(0); }, 1, 0, "actionItem");

        //Affichage
        oActionMenu.addLabel(top._res_1084, 1, -1, "actionLabel");

        // Toutes les lignes
        oActionMenu.addItemFct(top._res_6261, fctBuilder(0), 1, 0, "actionItem");

        // "Uniquement les lignes cochées"         
        oActionMenu.addItemFct(top._res_6262, fctBuilder(1), 1, 0, "actionItem");
    }





    var bHasTreatment = false;



    var bIsMarked = getNumber(nbMarkedFiles) > 0;

    //Objets pour les traitements de masse
    oManuTraitment = new eTraitementAction(nGlobalActiveTab, nGlobalActiveTab, false, bIsMarked);

    //Ajout des action de traitements au MENU ACTION
    oManuTraitment.addAction("treatment"); //traitement de masse delete duplicate....
    //oManuTraitment.addAction("export"); //report  //GCH masqué car non développé : #33634 - DEV - XRM - CANADA - AQESSS - Menu Actions sur table contact - export - à masquer
    oManuTraitment.addAction("mailing");
    oManuTraitment.Init();

    bHasTreatment = (nId > 0 && bIsMarked && oManuTraitment.bIsAllowed("delete") || oManuTraitment.bIsAllowed("export") || (nGlobalActiveTab == 200 && oManuTraitment.bIsAllowed("mailing")));

    if (bHasTreatment)
        oActionMenu.addLabel(top._res_295, 1, 0, "actionLabel");  //Traitements

    //On supprime que les fiches affichées et sélectionnées si l utilisateur a les droits
    if (nId > 0 && bIsMarked && oManuTraitment.bIsAllowed("delete")) {
        oActionMenu.addItemFct(top._res_6438, function () { oManuTraitment.SetTargetTab(function () { oManuTraitment.Delete(false); }) }, 1, 0, "actionItem");
    }
    if (oManuTraitment.bIsAllowed("export")) {
        oActionMenu.addItemFct(top._res_6303, oManuTraitment.Export, 1, 0, "actionItem");             // Export
    }
    if (nGlobalActiveTab == 200 && oManuTraitment.bIsAllowed("mailing")) {
        oActionMenu.addItemFct(top._res_14, oManuTraitment.AddMailing, 1, 0, "actionItem"); //depuis contacts    
    }

    //
    oActionMenu.hideEmpty();

    // TODO : masquer le css des ombres
    oActionMenu.menuAddClass("actionDrop");

    // Ajuste la position du menu sous ou haut dessus du bouton - alignement a drotie de 6px pour le décalage lié au découpage du bouton
    if (sBtn == "H")
        oActionMenu.alignElement(btnM, "UNDER", "0|0"); // Le menu apparait en dessous du bouton
    else
        oActionMenu.alignElement(btnM, "ABOVE", "1|-6");

    //Cache le bouton sur le mouseout - action mouseout attaché après un 1er mouseover
    if (!mainDebugMode) {
        var oActionMenuMouseOver = function () {
            var actionOut = setTimeout(
                function () {

                    if (oActionMenu) {
                        //Masque le menu
                        oActionMenu.hide();
                        // Bouton apparence inloa  
                        oActionMenu = null;

                    }
                    shBtnAction(sBtn, false);

                }
                , 200);

            //Annule la disparition
            if (oActionMenu) {
                setEventListener(oActionMenu.mainDiv, "mouseover", function () { clearTimeout(actionOut) });
            }
            setEventListener(btnD, "mouseover", function () { clearTimeout(actionOut); });

            setEventListener(btnM, "mouseover", function () { clearTimeout(actionOut); });
        };

        //Faire disparaitre le menu dans 200ms apres le mouseout
        if (oActionMenu)
            setEventListener(oActionMenu.mainDiv, "mouseout", oActionMenuMouseOver);
    }
}

///<Summary>
/// Objet de traitement de masse depuis le menu 'Actions'
/// Using Treament.js 
/// Using Tools.js
///<Summary>
function eTraitementAction(nTabFrom, nTargetTab, bAllFiles, bMarkedFile) {

    var me = this;
    this._Actions = new Array();
    this._dicRights = new Array();

    //Pour le traitement de masse
    this._nAction = 0;      // NONE
    this._dicActions = { "delete": 3, "duplicate": 4 };

    this._bAllFile = bAllFiles;
    this._bMarkedFile = bMarkedFile;
    this._nTabFrom = nTabFrom;
    this._nTargetTab = nTargetTab;
    this._upd = null;
    this._ADR = 400;
    this._PP = 200;
    this._PM = 300;
    this._TargetLib = ""
    this._AdrLib = "";
    this._PpLib = "";

    ///<summary>
    ///Ajouter une action
    ///<summary>
    this.addAction = function (sAction) {
        if (sAction != null && sAction.length > 0)
            if (!(sAction in me._Actions))
                me._Actions.push(sAction);
    }

    ///<summary>
    ///Vérification des droits
    ///<summary>
    this.bIsAllowed = function (sAction) {
        if (me._dicRights[sAction] != "undefined" && me._dicRights[sAction] != null)
            return me._dicRights[sAction];

        return false;
    }

    ///<summary>
    ///Initialise les propriétés de l 'objet
    ///<summary>
    this.Init = function () {

        //Droits de traitement
        me.RetrieveRights();

        //pour la suppression de masse
        _upd = new eUpdater("mgr/eTreatmentManager.ashx", 0)
        _upd.addParam("tabfrom", me._nTabFrom, "post");
        _upd.addParam("allfile", me._bAllFile ? "1" : "0", "post");
        _upd.addParam("markedfile", me._bMarkedFile ? "1" : "0", "post");

        //Dans Treatement.js on initialise la variable nTabFrom
        SetTabFromTreatement(me._nTabFrom);

        //Récupération des libellés
        me._TargetLib = getAttributeValue(document.getElementById("hvFromTab_" + me._nTabFrom), "value");
        me._AdrLib = getAttributeValue(document.getElementById("hvAdr_" + me._ADR), "value");
        me._PpLib = getAttributeValue(document.getElementById("hvPp_" + me._PP), "value");
    }

    ///<summary>
    ///Supprime les lignes sélctionnées
    ///<bConfirm> true on affiche la confirmation , false on fait le traitement
    ///<summary>
    this.Delete = function (bConfirmed, selectedOption) {
        //confirme la suprression  avec ou sans suppression de PP     
        if (!bConfirmed) {

            var targetName = me._nTargetTab == me._nTabFrom ? me._TargetLib : me._AdrLib;
            var oModCfmSup = eConfirm(1, top._res_68, top._res_832.replace("<ITEM>", targetName) + ".<br\>" + top._res_833, "", 550, 200,
                function () {
                    me.Delete(true, oModCfmSup.getSelectedValue());
                });

            oModCfmSup.isMultiSelectList = true;

            if (me._nTargetTab == 300) {
                oModCfmSup.addSelectOption(top._res_6447.replace("<PP>", me._PpLib).replace("<PM>", targetName), "deletePp", false);
                oModCfmSup.addSelectOption(top._res_6447.replace("<PP>", me._AdrLib).replace("<PM>", targetName), "deleteAdr", false);
            }
            oModCfmSup.createSelectListCheckOpt();

        } else {

            me._nAction = me._dicActions["delete"];
            _upd.addParam("operation", me._nAction, "post");

            if (me._nTargetTab == 300) {
                if (typeof (selectedOption) != "undefined" && selectedOption.indexOf("deletePp") > -1)
                    _upd.addParam("deletePp", "1", "post");
                else
                    _upd.addParam("deletePp", "0", "post");

                if (typeof (selectedOption) != "undefined" && selectedOption.indexOf("deleteAdr") > -1)
                    _upd.addParam("deleteAdr", "1", "post");
                else
                    _upd.addParam("deleteAdr", "0", "post");
            }

            _upd.addParam("cnt", document.getElementById("markedNb_" + nGlobalActiveTab).value, "post");
            me.RunTreatement();
        }
    }

    ///<summary>
    ///Exporter les lignes 
    ///<summary>
    this.Export = function () {
        alert("Export en cours de développement...!");
    }

    ///<summary>
    ///Faire un Emailing
    ///<summary>
    this.AddMailing = function () {
        //alert("AddMailing invalide parametre :: emain.js: 1123")
        AddMailing(me._nTabFrom, 0);
    }

    ///<summary>
    ///Demander a l utilisateur de choisir entre pp/pm ou adr, si on vient de pp ou pm
    ///Sinon on execute l'action demandée
    ///<summary>
    this.SetTargetTab = function (Run) {
        if (me._nTabFrom == me._PP || me._nTabFrom == me._PM)
            SetTargetTab(me._ADR, me._nTabFrom, function (value) { me._nTargetTab = value; Run(); });
        else
            Run();

    }

    ///<summary>
    ///PRIVATE : lancer le traitement
    ///<summary>
    this.RunTreatement = function () {
        if (me._nAction == 0) {
            alert("Pas de traitement definit... ");
            return;
        }
        _upd.addParam("targettab", me._nTargetTab, "post");
        // Créer une methode en cas d'une erreur il l'execute
        _upd.ErrorCallBack = function () { me.StopProcess(); top.setWait(false); };
        _upd.send(me.Feedback);

        top.setWait(true);
    }

    ///<summary>
    ///feedback sur l'encours du traitement
    ///depuis Treatement.js
    ///<summary>
    this.Feedback = function (oDoc) {
        ReturnRunTreatment(oDoc);
    }

    ///<summary>
    ///Arreter traitement
    ///<summary>
    this.StopProcess = function () {
        StopProcessTreatment();
    }

    ///<summary>
    ///Récupèrer les droits de traitement depuis le menu à droite
    ///<summary>
    this.RetrieveRights = function () {

        for (var i = 0; i < me._Actions.length; i++) {

            var input = document.getElementById(me._Actions[i] + "_" + me._nTabFrom);
            if (typeof input == "undefined" || input == null)
                continue;

            var actName = input.getAttribute("sact");

            var actNames = actName.split("|");
            for (var j = 0; j < actNames.length; j++) {
                if (actNames[j].length > 0)
                    me._dicRights[actNames[j]] = true;
            }
        }
    }
}

/************************************************************************/
/*                          MENU FILE (menu droite )                    */
/************************************************************************/
//Charge le xlm Menu droite
// tab : descid de la table (-2: mon eudonet // 0: accueil// )
// type : type de menu (0 : accueil // 1 : liste // 2 : fiche // 3 : Modification // 4 : vcard // 5 : creation // 6 : Impression // 7 : Administration // 8 : onglet web)
function loadFileMenu(tab, type, nFileId, callback, module) {

    var oParamGoTabList = null;
    // US #4291 - TK #6962 - Demande #94 697 (et #94 797 ?) - On n'affiche pas de skeleton si on recharge la même fiche ou même liste sur E17
    var bReloadsSameFile = (document.getElementById("fileDiv_" + tab) && getAttributeValue(document.getElementById("fileDiv_" + tab), "fid") == nFileId);
    var bReloadsSameList = (tab == nGlobalActiveTab);
    if (!bReloadsSameFile && !bReloadsSameList) {
        oParamGoTabList = {
            to: type,
            nTab: tab,
            context: "eMain.loadFileMenu"
        }
    }

    //Appel le waiter, y pas skeleton pour page Mon eudonet(tab : -2)
    if (tab != -2) {
        setWait(true, undefined, undefined, isIris(top.getTabFrom()), oParamGoTabList);
    }

    closeAllModals();
    closeActionMenu();

    if (tab == 101000 && (module == USROPT_MODULE_ADMIN_TAB_USER || module == USROPT_MODULE_ADMIN_ACCESS_USERGROUPS || module == USROPT_MODULE_ADMIN_ACCESS))
        var oFileMenuUpdater = new eUpdater("eda/mgr/eAdminFileMenuManager.ashx", 1);
    else
        var oFileMenuUpdater = new eUpdater("mgr/eFileMenuManager.ashx", 1);

    //Appel le update du menu

    oFileMenuUpdater.asyncFlag = false;
    oFileMenuUpdater.addParam("tab", tab, "post");
    oFileMenuUpdater.addParam("type", type, "post");
    oFileMenuUpdater.addParam("fileid", nFileId, "post");
    oFileMenuUpdater.addParam("xsltserver", 1, "post");

    paramWin = top.getParamWindow();
    if (!paramWin || typeof (paramWin.GetParam) != "function") {
        var retryFct = function () {
            loadFileMenu(tab, type, nFileId, callback, module);
        };
        top.loadFileMenuRetryTimeout = setTimeout(retryFct, 1000);
        return;
    }
    else if (top.loadFileMenuRetryTimeout) {
        clearTimeout(top.loadFileMenuRetryTimeout);
    }

    objThm = JSON.parse(paramWin.GetParam("currenttheme").toString());

    if (objThm.Id != nThemeId) {
        switchDefaultThemeToUserTheme();
    }

    oFileMenuUpdater.addParam("iThemeId", nThemeId, "post");

    if (type == 0) {
        var oeParam = getParamWindow();
        var nbFavLnkPerCol = 0;
        if (oeParam && typeof (oeParam.GetParam) != "undefined") {
            nbFavLnkPerCol = oeParam.GetParam("nbFavLnkPerCol");
        }
        if (nbFavLnkPerCol == 0) {
            nbFavLnkPerCol = GetNbFavLinks();
        }

        oFileMenuUpdater.addParam("nbFavLnkPerCol", nbFavLnkPerCol, "post");
    }
    // Rendu du menu "Admin"
    else if (type == 7) {
        // Options utilisateur / "Mon Eudonet"
        if (tab == -1)
            oFileMenuUpdater.addParam("module", 9, "post"); // USROPT_MODULE_ADMIN
        else
            oFileMenuUpdater.addParam("module", 1, "post"); // USROPT_MODULE_MAIN
    }
    else if (module == USROPT_MODULE_ADMIN_ACCESS_USERGROUPS || module == USROPT_MODULE_ADMIN_DASHBOARD_RGPDTREATMENTLOG)
        oFileMenuUpdater.addParam("module", getUserOptionsModuleHashCode(module), "post");

    oFileMenuUpdater.ErrorCallBack = errorMenu;



    oFileMenuUpdater.send(function (oRes) {

        if (nsMain) {
            switchActiveTab(tab, type);
            nsMain.SetGlobalActiveTab(tab);

        }

        updateMenu(oRes, "rightMenu", callback, type);

        oEvent.fire("right-menu-loaded", { tab: tab, type: type, fileId: nFileId });

        //ELAIZ : switch nouveau thème - vérifie si le switch est activé au chargement
        if (document.querySelector('#chckNwThm')) {
            if (document.querySelector('#chckNwThm').checked)
                document.querySelector('.switch-new-theme-wrap').classList.add('activated');
            else
                document.querySelector('.switch-new-theme-wrap').classList.remove('activated');
        }

        showHideSpnImgTheme();
        //SHA : backlog #1 588
        // MAB - #76 430 - ces fonctions étant dans eAdmin.js, elles ne sont pas chargées sur un profil utilisateur non admin
        if (typeof (hideSwitchNewTheme) == "function")
            hideSwitchNewTheme();
        //SHA : backlog #1 657
        // MAB - #76 430 - ces fonctions étant dans eAdmin.js, elles ne sont pas chargées sur un profil utilisateur non admin
        if (typeof (hideSpanImgNewTheme) == "function")
            hideSpanImgNewTheme();
    });
}

//SHA : backlog #1 647
function showHideSpnImgTheme() {
    var dvContent = document.querySelector(".switch-new-theme-wrap");
    var chk = document.querySelector("#chckNwThm");

    /** Dans le cas d'IE on n'affiche pas la checkbox, 
     * donc on se moque pas mal de la suite... G.L */
    if (!(chk || dvContent))
        return;

    var spnNwItm = document.querySelector("#chckNwThm ~ span");
    var imgNwItm = document.querySelector("#chckNwThm ~ img");

    /**
     * Si toutes les conditions sont réunies, on passe sur la nouvelle ergo.
     * Sinon, non.
     * */
    var paramWin = top.getParamWindow();
    var objThm;

    try {
        paramWin = top.getParamWindow();
        objThm = JSON.parse(paramWin.GetParam("currenttheme").toString());

        /** On parcourt les thèmes. Et si le thème est sombre,
         * On met une couleur claire pour la police. G.L. */
        for (obj in THEMES) {
            if (THEMES[obj].Id == objThm.Id && THEMES[obj].Sombre == 1) {
                /** Mettre le bon thème. */
                dvContent.classList.add('sombre');
            }
        }
    } catch (e) {
        /** En cas de plantages trop sévères ou récurrents, décommenter. G.L */
        /*eAlert(0, null, e.message);*/
    }


    if (chk.checked) {
        spnNwItm.style.visibility = 'hidden';
        spnNwItm.style.display = 'none';

        imgNwItm.setAttribute("title", top._res_2378);
        imgNwItm.setAttribute("alt", top._res_2378);
        imgNwItm.setAttribute("class", "switch-new-theme-img");
        imgNwItm.setAttribute("src", "themes/default/images/logo-eudonetx.jpg");
        imgNwItm.style.visibility = 'visible';
        imgNwItm.style.display = 'block';
    }
    else {
        imgNwItm.style.visibility = 'hidden';
        imgNwItm.style.display = 'none';

        spnNwItm.innerHTML = top._res_2376;
        spnNwItm.setAttribute("title", top._res_2376);
        spnNwItm.setAttribute("class", "switch-new-theme-spn");
        spnNwItm.style.visibility = 'visible';
        spnNwItm.style.display = 'block';
    }
};


// Gestion de session coté js
var oSession = (function () {

    // Singleton : On a un seul objet pour toute l'application XRM
    if (top && typeof (top.oSession) != 'undefined')
        return top.oSession;

    // par défaut (10 de DEFAULT_TIME_REMAINING, 20 de DEFAULT_TIMEOUT) = 20 minutes
    var DEFAULT_TIMEOUT = 20;
    var DEFAULT_TIME_REMAINING = 10;
    var bDebug = '0';

    var that = this;
    var bDisconnected = false;

    // on previent l'utilisateur "DEFAULT_TIME_REMAINING minutes" avant l'expiration
    var nSessionTimeout = (DEFAULT_TIMEOUT - DEFAULT_TIME_REMAINING) * 60 * 1000;
    var handler = null;

    //Une fois le message est affiché, on lance un timer dans 5 minutes
    var span = null;
    var timeRemining = DEFAULT_TIME_REMAINING * 60;
    var timeLeftHandler = null;

    // confirmation de garder la session active
    var messageBox = null;

    //Au chargement de la page, les events BeforUnload et Unload sont attachés 
    // a la fenetres principale ou au document de la page 
    var attachExitEvents = function () {

        var browser = new getBrowser();

        if (browser.isIE && browser.isIE8) {
            window.onbeforeunload = confirm;
            window.onunload = safeExit;
        } else {
            setWindowEventListener("beforeunload", confirm);
            setWindowEventListener("unload", safeExit);
        }
    };

    //Fenetre de confirmation pour fermeture
    var confirm = function (evt) {

        // si on a perdu la session pas la peine d'afficher 
        // le message ci-après
        if (bDisconnected)
            return;

        if (!evt)
            evt = window.event;

        var sDisplayMessage = top._res_6383; //Vous etes sur le point de quitter l appli
        evt.returnValue = sDisplayMessage;

        //stopEvent(evt);
        return sDisplayMessage;
    };

    //On reset le timer sur la durée de la session
    var setTimer = function () {
        reset();
        handler = setTimeout(warning, nSessionTimeout);
    };

    //confirmation de la session
    var warning = function () {

        var details = top._res_6910.replace("<KeepMySessionAlive>", top._res_6911);
        // on ajoute un id pour mettre a jour le compteur
        details = details.replace("<timeLeft>", "<span id='timeRemainigSpan' class='timeLeft'>--:--</span>");
        messageBox = eAdvConfirm({
            'criticity': 1,
            'title': top._res_6908, // 'Inactivité détectée',
            'message': top._res_6909, //'Eudonet XRM a détecté une inactivité.',
            'details': details, // 'Afin de conserver votre session active, merci de cliquer sur "<KeepMySessionAlive>".<br /> Sans réponse de votre part, votre session va être clôturée dans <timeLeft>',
            'okFct': keepSessionAlive,
            'cancelFct': logout,
            'bOkGreen': true,
            'bHtml': true,
            'resOk': top._res_6911, //'Maintenir ma session active',
            'resCancel': top._res_6912 //'Me déconnecter',
        });

        timeLeftHandler = setInterval(setTimeLeft, 1000);

        window.focus();
    };

    // si le temps est ecoulé, on déconnecte l'appli
    var setTimeLeft = function () {
        // le span est renouvellé a chaque affichage de la messageBox et non pas toutes les 1 secondes
        if (span == null)
            span = document.getElementById("timeRemainigSpan");

        timeRemining--;

        if (timeRemining < 0)
            logout();
        else
            SetText(span, getTimeLeft());
    };

    // retourne la durrée restant pour l'expiration de la session
    var getTimeLeft = function () {
        var minutes = Math.floor(timeRemining / 60);
        var secondes = timeRemining - minutes * 60;
        return "0" + minutes + ":" + (secondes >= 10 ? secondes : ("0" + secondes)) + "";
    }

    // Maintenir ma session active
    var keepSessionAlive = function () {
        // garde la session du serveur active
        var updater = new eUpdater("mgr/eBlank.ashx", null)
        updater.bKeepSessionAlive = true;
        // au retour serveur eUpdater fait appel a oSession.KeepAlive()
        updater.send();
    }

    // Me déconnecter
    var logout = function () {
        reset();
        bDisconnected = true;
        doDisco();
    };

    // arrete les timers
    var reset = function () {

        // pour le timer du temps restant affiché
        if (timeLeftHandler != null)
            clearInterval(timeLeftHandler);

        // pour le timer de la session session
        if (handler != null)
            clearTimeout(handler);

        bDisconnected = false;
        handler = null;
        span = null;
        timeRemining = DEFAULT_TIME_REMAINING * 60;
        timeLeftHandler = null;
        messageBox = null;
    }

    //Fermer la session proprement
    var safeExit = function (evt) { doUnload(); };

    return {
        // initilise la session
        Init: function (data) {

            // dans le cas de debug on affiche pas la confirmation et pas d'expiration non plus
            if (data && data.debug == '1') {
                bDebug = '1';
                return;
            }

            // Avant 5 minutes de la fin, on affiche un message d'avertissement
            // avec un compteur qui se décremente...
            if (data && data.timeout && data.timeout > DEFAULT_TIMEOUT)
                nSessionTimeout = (data.timeout - DEFAULT_TIME_REMAINING) * 60 * 1000;

            //Pour confirmer la fermeture de l'application    
            attachExitEvents();

            //Démarre la session
            setTimer();
        },

        // Garde la session active sauf en cas d'expiration
        // En debug,  la session n'expire pas
        KeepAlive: function () {

            if (bDisconnected || bDebug == '1')
                return;

            if (timeLeftHandler > 0) {
                if (messageBox != null)
                    messageBox.hide();
            }

            setTimer();
        },

        // la session est perdue
        Expire: function () {
            reset();
            bDisconnected = true;
        }
    }
})();

///Appel le manager d'unload
/// celui-ci met à jour la date de fin de connexion dans statlog
///
function doUnload() {

    var oLonOut = new eUpdater("mgr/eLoginMgr.ashx", 1);
    oLonOut.asyncFlag = false;
    oLonOut.addParam("action", "unload", "get");
    oLonOut.send(doUnloadPage, null);

}

function doUnloadPage() {
    try {
        // Si on utilise un thème autre que par défaut, on le sauvegarde au déchargement de la page
        // Permet notamment de restaurer le thème utilisateur personnalisé lorsqu'on quitte la page
        // après être allé en Administration (qui force le thème par défaut)
        if (nThemeId > 0 && bDefaultThemeForced)
            applyTheme(nThemeId, nGlobalCurrentUserid, function () { }, function () { });
    }
    catch (ex) { }
}


function doDisco() {
    doUnload();
    top.location.href = "eLogin.aspx?d=1";
}

function doDiscoAdfs() {
    var oLonOut = new eUpdater("mgr/eLoginMgr.ashx", 1);
    top.setWait(true);
    oLonOut.addParam("action", "unloadadfs", "get");
    oLonOut.ErrorCallbackFunction = function (str) { top.setWait(false); };
    oLonOut.send(function (str) {

        top.setWait(false);
        if (str + "" !== "0") {
            top.location.href = str;
        }
        else {
            //déconnexion impossible
            top.location.href = "eLogin.aspx?d=1";
        }

    }, null);
}

/************************************************************************/
/*                           BARRE DE NAVIGATION                        */
/************************************************************************/
//Charge la barre de navigation
// to = // LIST 1, FILE 2
function loadNavBar(to) {

    if (to == 1) {
        // On créait un object pour renvoyer au setWait ou es ce qu'on veux aller
        var oParamGoTabList = {
            to: 1,
            nTab: nGlobalActiveTab,
            context: "eMain.loadNavBar"
        }
    }

    if (to == 3) {
        // On créait un object pour renvoyer au setWait ou es ce qu'on veux aller
        var oParamGoTabList = {
            to: 3,
            nTab: nGlobalActiveTab,
            context: "eMain.loadNavBar"
        }
    }

    setWait(true, undefined, undefined, isIris(top.getTabFrom()), oParamGoTabList);
    top.bIsNavBarLoaded = 0;

    //Navbar (Mode HTML)
    var oNavBarUpdater = new eUpdater("mgr/eNavBarManager.ashx", 1);
    oNavBarUpdater.ErrorCallBack = function () { setWait(false); };
    var winSize = getWindowSize();

    // Si on revient vers l'accueil, on n'epingle pas le menu, on prend toute la largeur
    if (nGlobalActiveTab == 0)
        winSize.dw = winSize.w;

    oNavBarUpdater.addParam("H", winSize.h, "post");
    oNavBarUpdater.addParam("W", winSize.dw, "post");
    oNavBarUpdater.addParam("activetab", nGlobalActiveTab, "post");
    oNavBarUpdater.addParam("xsltserver", 1, "post");
    oNavBarUpdater.ErrorCallBack = errorMenu;
    oNavBarUpdater.send(updateMenu, "globalNav", undefined, to);
}


//Au resize on redimensionne la navbar sauf pour les tablettes car le (dé)zoom et considéré comme un resize
var _resizeTimer;
function resizeNavBar() {


    // SPH: désactiver pour l'instant #33105
    // le resize  de la navbar provoque le relancement de eParam
    // cela cause la réinitialisation de nombreuse variable de contexte (fiche en cours, paging, etc)
    // et provoque en conséquence des feedback type" eFileAsyncManager : pas de fileid"
    return;
    /*
    try {
        window.clearTimeout(_resizeTimer);
        _resizeTimer = window.setTimeout(doResizeNavBar, 100);
    }
    catch (e) {
    };
    */
}

// to LIST 2, FILE 3
function doResizeNavBar(to) {
    if (!isTablet()) {

        top.bReloadMRU = 1;
        loadNavBar(to);
    }
}



/************************************************************************/
/*                          MAJ MENU                                    */
/************************************************************************/

// Maj du menu "sContentElem" par "sContent"
// Utilisé pour la navbar et le menu fichier
function updateMenu(sContent, sContentElem, callback, typeMenu) {
    //Maj
    var oDiv = document.getElementById(sContentElem);
    if (oDiv) {
        // remove the class name of skeleton 
        let pnWaiterSkltonfile = document.getElementById('pnWaiterSkltonfile');
        if (pnWaiterSkltonfile && !isInAdminMode()) {
            removeClass(pnWaiterSkltonfile, "adminMode")
        }

        oDiv.innerHTML = sContent;
        if (sContentElem == "rightMenu") {
            var oRightMenu = document.getElementById("rightMenu");
            var oMenuPin = document.getElementById("menuPin");

            if (oRightMenu && oMenuPin) {
                // Icône d'ouverture du menu : sur tablettes, réagit au clic, sur PC, réagit au mouseover
                // Deux évènements différenciés permettent à certaines tablettes (ex : iPad) de ne pas confondre le clic sur
                // le menu, avec le clic sur les liens contenus dans ce menu
                if (isTablet()) {
                    oMenuPin.onclick = function (e) {
                        ocMenu(null, null);
                    }
                }
                else {
                    oMenuPin.onmouseover = function (e) {
                        ocMenu(true, e);
                    }
                    // IMPORTANT : l'évènement onMouseOut doit être mis sur l'ensemble du menu, et non sur le bouton seul.
                    // Sinon, le menu se refermerait dès son ouverture, vu que l'animation de transition fait bouger le
                    // bouton du menu, qui ne se retrouve alors plus sous le curseur
                    oRightMenu.onmouseout = function (e) {
                        ocMenu(false, e);
                    }
                }
                // Icône d'épinglage du menu : sur PC uniquement, et hors accueil
                if (nGlobalActiveTab != 0 && !isTablet()) {
                    var nUserId = getAttributeValue(oMenuPin, "userid");
                    oMenuPin.onclick = function () { pinMenu(nUserId); }
                    removeClass(oMenuPin, "icon-menu_btn_acc");
                    addClass(oMenuPin, "icon-menu_btn");
                    addClass(oMenuPin, "myPin");
                }
            }

            if (nGlobalActiveTab == null || nGlobalActiveTab == 0 || isTablet() || sContent.indexOf('pinned="1"') == -1) {
                removeClass(oDiv, "FavLnkOpen");
                addClass(oDiv, "FavLnkClosed");
            }
            else {
                removeClass(oDiv, "FavLnkClosed");
                addClass(oDiv, "FavLnkOpen");
            }
        }
    }

    // Génération paging de la nav bar
    if (sContentElem == "globalNav") {
        setPagingNavBar();

        //Gestion du chargement asymétrique entre la navbar et la frame de param
        top.bIsNavBarLoaded = "1";
        if (top.bIsIFrameLoaded == "1" && top.bIsParamLoaded != "1") {
            var eParamDoc = top.document.getElementById('eParam');
            if (eParamDoc && eParamDoc.contentWindow && eParamDoc.contentWindow.OnLoadParam)
                eParamDoc.contentWindow.OnLoadParam();
        }
        else {

            if (top.bReloadMRU && top.ReloadNavBarMRU) {

                top.ReloadNavBarMRU();
                top.bReloadMRU = 0;
            }
        }

        //Si la table active ( nGlobalActiveTab ) n'est pas dans les tables affiché, redirection sur l'accueil, sauf si on cherche
        //à afficher le mode Admin
        if (nGlobalActiveTab > -1) {
            var oTab = document.getElementById("tab_header_" + nGlobalActiveTab);
            if (!oTab)
                goTabList(0);
        }
    }

    // Chargement du menu pour les tablettes
    /*
    if (isTablet() && isTabletMenuEnabled() && sContentElem == "rightMenu") {
        loadTabletMenu();
    }
    */

    //Ajuste la taille du conteneur principale à la taille restant à l'écran
    if (sContentElem == "rightMenu") {
        adjustDivContainer();

        // On ajoute un évènement réagissant à la fin de la transition du menu
        setEventListener(oDiv, 'webkitTransitionEnd', afterRightMenuTransition, false);
    }
    var currentView = getCurrentView(document)

    if (!typeMenu && currentView == "LIST") {
        typeMenu = 1
    }

    var oParamGoTabList = {
        to: typeMenu,
        nTab: nGlobalActiveTab,
        context: "eMain.updateMenu"
    }

    //Ferme le waiter
    setWait(false, undefined, undefined, undefined, oParamGoTabList);

    //Calendrier
    updateCalendar();


    if (top.bResizeNavBar) {
        top.bResizeNavBar = 0;
        doResizeNavBar(typeMenu);
    }

    if (typeof (callback) == "function")
        callback();

    // US #1330 - Tâche #2911 - Repositionnement de la page active de la barre d'onglets après rechargement de celle-ci via un changement de vue
    switchActiveTab(nGlobalActiveTab, typeMenu);
}


function updateCalendar() {
    var divCal = document.getElementById("Calendar_" + nGlobalActiveTab);
    if (divCal == null)
        return;
    //  <div class="CalContainer" dc="@dc" wd="@wd" id="Calendar_{@tab}">
    var strWorkingDays = divCal.getAttribute("wd");
    var calYear = divCal.getAttribute("year");
    var calMonth = divCal.getAttribute("month");
    var calDay = divCal.getAttribute("day");
    var calmode = divCal.getAttribute("calmode");
    var bHighLight = true;
    var myDate = new Date(calYear, getNumber(calMonth) - 1, calDay);

    divCal.innerHTML = getCalendar("Calendar_" + nGlobalActiveTab, calMonth, myDate.getFullYear(), myDate, calmode, strWorkingDays, bHighLight);

}


///génère le paging des onglets dans la nav bar
function setPagingNavBar() {
    // Au lancement de l'application, activepage est à 1
    nActivePageTabs = 1;

    var oMenuNavBar = document.getElementById("menuNavBar");
    var nbPage = Number(oMenuNavBar.getAttribute("nbtab"));

    if (isTablet() && isTabletNavBarEnabled()) {
        document.getElementById("menuNavBar").style.width = nbPage * 100 + '%';
    }

    // création du paging des table
    if (nbPage > 1) {

        for (var k = 1; k <= nbPage; k++) {

            var igClass = 'icon-circle-thin imgInact';

            if (k == nActivePageTabs)
                igClass = 'icon-circle imgAct';

            // Création de l'élément img
            var oImg = document.getElementById('switch' + k);
            oImg.className = igClass;

            // affecte le click (appel d'une fct anno pour l'affectation de la fonction sur click pour préserver la valeur de la boucle)
            // si la fonction est directement affecté au click, la valeur de la boucle reste sur le contexte de la fonction et est donc
            // incrémenté même après affectation
            (function (nPage) { oImg.onclick = function () { switchActivePageTab(nPage); } })(k);

        }
    }

    loadTabletNavBar();
}



/************************************************************************/
/*                        GESTION ERREUR                                */
/************************************************************************/
//Erreur sur la mise à jour d'un menu
function errorMenu(objError) {
    setWait(false);
}


//Erreur sur la mise à jour d'une liste
// retourne à l'accueil
function errorList(objError) {
    if (typeof goTabList == "function")
        goTabList(0);
    setWait(false);
}


//Erreur sur le chargement de l'administration
function errorAdmin(objError) {
    setWait(false);
}

/************************************************************************************
 *                          DIV DE WAIT                                             *
 ************************************************************************************
 * Affiche/Masque le div d'attente
 * A chaque fois que l'on rafraîchit la page, si la surveillance des mouvements sur tablette est activée,
 * on met à jour un booléen indiquant de ne pas déclencher les fonctions liées à la détection des mouvements
 * pour ne pas qu'elles se déclenchent en boucle tant que le traitement n'est pas terminé
 * @param {any} bOn
 * @param {any} name
 * @param {any} upd
 * @param {any} fromIris on vient d'Iris, et on repassera par XRM
 * @param {any} GoTo
 */
function setWait(bOn, name, upd, fromIris, GoTo) {
    if (window.DeviceOrientationEvent) {
        window.preventTabletMoveFunctions = bOn;
    }

    var oWaiter = top.document.getElementById("waiter");
    var waitOff = 'waitOff';
    var waitOn = 'waitOn';


    var paramWin = top.getParamWindow();
    var objThm;
    if (paramWin && paramWin.GetParam != null) {
        objThm = JSON.parse(paramWin.GetParam("currenttheme").toString());
    }
    // (paramWin && paramWin.GetParam != null && objThm.Version == 2) c'est pour savoir si on est sur eudonet X ou pas
    if (GoTo && (paramWin && paramWin.GetParam != null && objThm.Version == 2)) {
        // GoTo.to : type de menu (0 : accueil // 1 : liste // 2 : fiche // 3 : Modification // 4 : vcard // 5 : creation // 6 : Impression // 7 : Administration // 8 : onglet web)
        var target = { to: 0, nTab: 0 };
        var source = { to: GoTo.to, nTab: GoTo.nTab };
        var returnedTarget = Object.assign(target, source);
        var typeOfPage = "HOME";
        var currentView = getCurrentView(document);
        
        if (returnedTarget.to == 1 && returnedTarget.nTab > 0 && currentView != 'CALENDAR') {
            typeOfPage = "LIST";
            oWaiter = top.document.getElementById('pnWaiterSklton' + typeOfPage.toLowerCase());
            waitOff = 'SktOff-' + typeOfPage.toLowerCase();
            waitOn = 'SktOn-' + typeOfPage.toLowerCase();
            
            oWaiter.style.width = (document?.body?.offsetWidth - GetRightMenuWidth()) + 'px';
        } else if (returnedTarget.to == 1 && returnedTarget.nTab > 0 && GoTo.nTab != top.nGlobalActiveTab && currentView == 'CALENDAR') {
            oWaiter = top.document.getElementById("waiter");
            waitOff = 'waitOff';
            waitOn = 'waitOn';
        } else if (returnedTarget.to == 3 || returnedTarget.to == 2 || returnedTarget.to == 7) {
            typeOfPage = "FILE";
            oWaiter = top.document.getElementById('pnWaiterSklton' + typeOfPage.toLowerCase());
            waitOff = 'SktOff-' + typeOfPage.toLowerCase();
            waitOn = 'SktOn-' + typeOfPage.toLowerCase();
            oWaiter.style.width = (document?.body?.offsetWidth - GetRightMenuWidth()) + 'px';
        } else {
            oWaiter = top.document.getElementById("waiter");
            waitOff = 'waitOff';
            waitOn = 'waitOn';
        }
    }

    // US #4261 - TK #6962 - Pour faciliter le traçage des skeletons, on ajoute un message dans la console si le mode Debug de eMain.js est activé en début de fichier
    if (mainDebugMode)
        console.log((bOn ? "AFFICHAGE" : "Masquage") + " d'un " + (waitOn.indexOf("Skt") > -1 ? "skeleton" : "waiter") + " en provenance de " + GoTo?.context);

    var maxZIndex = 100;	//zIndex min à 100 pour iso à eModal
    var sClassFinalizer = (fromIris) ? "FrmIris" : "";

    if (oWaiter) {
        addOrDelAttributeValue(oWaiter, "caller", name, bOn, true);

        /** On recherche la classe à remplacer, à l'aide d'un moyen
         * particulièrement ingénieux et novateur, ... une boucle for,
         * et un startwith... */
        var waitClass = oWaiter.className;
        var classToReplace = "";

        if (waitClass.length > 0) {
            var tabClass = waitClass.split(" ")
            for (var cls in tabClass) {
                if (tabClass[cls].toString().toLowerCase().indexOf(waitOff.toLowerCase()) > -1
                    || tabClass[cls].toString().toLowerCase().indexOf(waitOn.toLowerCase()) > -1) {
                    classToReplace = tabClass[cls].toString();
                }
            }
        }

        if (bOn) {

            clearTimeout(waiterTime);

            if (!isInt(top.nbCompt) || top.nbCompt < 0)
                top.nbCompt = 1;
            else
                top.nbCompt++;

            switchClass(oWaiter, classToReplace, waitOn);
            if (oWaiter.id == "waiter") {
                top.document.getElementById("contentWait").style.color = (sClassFinalizer != '') ? '#bb1515' : '#ffffff';
                top.document.getElementById("contentWait").style.display = "block";
            }

            var zIndex = GetMaxZIndex(top.document, maxZIndex) + 1;
            oWaiter.style.zIndex = zIndex;

            closeRightMenu();


        }
        else {
            top.nbCompt--;

            //Ne masque le wait que si tout est loadé
            if (top.nbCompt <= 0) {
                top.nbCompt = 0;
                waiterTime = setTimeout(function () {

                    // #93 670 - On désactive d'abord tous les waiters de type skeleton
                    let waiter = top.document.querySelectorAll('[id^="pnWaiterSklton"]');
                    if (waiter && waiter.length) {
                        let sklCls = 'SktOn-';
                        for (let i = 0; i < waiter.length; i++) {
                            let sklId = waiter[i].id.replace('pnWaiterSklton', '')
                            switchClass(waiter[i], sklCls + sklId, 'SktOff-' + sklId);
                        }
                    }
                    // Puis on désactive ensuite spécifiquement le waiter E17 dans tous les cas
                    waiter = top.document.getElementById("waiter");
                    if (waiter)
                        switchClass(waiter, 'waitOn', 'waitOff');
                    // Et enfin, on désactive le waiter explicitement ciblé par les paramètres d'appel à setWait, s'il n'a pas été désactivé par les deux cas précédents
                    if (oWaiter)
                        switchClass(oWaiter, classToReplace, waitOff);

                    top.document.getElementById("contentWait").style.display = "none";
                }, 1);

                if (isTablet()) {
                    var moActiveMenu = document.querySelector("ul.sbMenuActive");
                    switchClass(moActiveMenu, "sbMenuActive", "sbMenu");
                    addClass(moActiveMenu, "ul-tab-hidden");
                    var moTabletActiveTabTitle = document.querySelector("div.navTitleTabletFocused");
                    removeClass(moTabletActiveTabTitle, "navTitleTabletFocused");
                }
            }

        }
    }
}

/*addOrDelAttribute : 
// oTarg : objet cible
// sAttribute : attribut cible 
// name : valeur à ajouter dans l'attribut
// bAdd : Ajout/Suppression de la valeur dans l'attribut
// bNoMerging : Si vrai on peux ajouter plusieurs fois la même valeur
*/
function addOrDelAttributeValue(oTarg, sAttribute, name, bAdd, bNoMerging) {
    if (name && name != "") {   //Que si une valeur est demandée
        var sCaller = oTarg.attributes[sAttribute];
        var nPos = (";" + sCaller + ";").indexOf(";" + name + ";");
        if ((bAdd) && (nPos < 0 || bNoMerging)) {   //Ajout de la valeur pour cet attribut : si pas de fusion on vérifit qu'il n'est pas déjà présent
            if (sCaller != "")
                sCaller = sCaller + ";";
            sCaller = sCaller + name;
            oTarg.attributes[sAttribute] = sCaller;
        }
        else if (!bAdd && (nPos > 0)) {   //Suppression d'une des valeurs de l'attribut si présent
            if (sCaller != "") {
                sCaller = (";" + sCaller + ";").replace(";" + name + ";", ";;");
                //Suppression des ; rajouté autour
                sCaller = sCaller.substring(0, 1);
                sCaller = sCaller.substring(sCaller.length - 2, sCaller.length - 1);
                oTarg.attributes[sAttribute] = sCaller;
            }
        }
    }
}

/*****************************************************/
function getFileUpdater(nTab, nFileId, nType, bAsync, height) {

    var oeParam = getParamWindow();

    var eParam = top.document.getElementById('eParam').contentWindow.document;
    var eParamIdContainer = eParam.getElementById("LISTIDS");
    var bPageChange = eParamIdContainer != null ? (eParamIdContainer.getAttribute("pagechange") == "1" ? true : false) : true;

    var iPageIndex = 1;
    if (oeParam.GetParam('Page_' + nTab) != '')
        iPageIndex = parseInt(oeParam.GetParam('Page_' + nTab));
    /*On force le refresh de la liste d'id si l'on vient de la première fiche d'une deuxième page et qu'on est sur la dernière page de la page d'avant*/
    var nLastFileIdInPreviousPage = 0;
    var nLastFileIdInCurrentPage = 0;
    var pageDiv = eParam.getElementById("listidpage_" + iPageIndex);
    if (pageDiv != null) {
        nLastFileIdInPreviousPage = parseInt(getAttributeValue(pageDiv, "LastFileIdInPreviousPage")); //Id du dernier enregistrement (zi égal 0 il faut utiliser le dernier id de la liste)
        nLastFileIdInCurrentPage = parseInt(getAttributeValue(pageDiv, "LastFileIdInNextPage")); //Id du dernier enregistrement (zi égal 0 il faut utiliser le dernier id de la liste)
    }
    var bInList = false;    //Vrai : La fiche courante est dans la liste qui est en train d'être parcourue
    if (eParamIdContainer != null) {
        bInList = ((("$|$" + GetText(eParamIdContainer) + "$|$").indexOf("$|$" + nFileId + "$|$")) >= 0);
        var ListArray = GetText(eParamIdContainer).split("$|$");
        if (bInList && ListArray[ListArray.length - 1] != nLastFileIdInCurrentPage && nLastFileIdInCurrentPage > 0) //Elle n'est pas dans la liste s'il s'agit de l'id de la liste suivante
            bInList = (ListArray[ListArray.length - 1] != nFileId);

    }
    /**************************************************************************************************************************************************/
    if (bInList || nLastFileIdInPreviousPage == nFileId) {
        var iTmpPageIndex = eParamIdContainer != null ? parseInt(eParamIdContainer.getAttribute("pageindex")) : 1;
        iPageIndex = iTmpPageIndex;
    }

    var currentView = getCurrentView(document);

    var bIsIdsListFilled = bPageChange ? false :
        (
            ((currentView != "FILE_CONSULTATION" && currentView != "FILE_MODIFICATION" && currentView != "FILE_CREATION") == true) ? false : //on force le rafraichissement de la liste si l'on vient d'une liste car l'on suppose que l'utilisateur à peut-être modifié un de ses filtres
                (eParamIdContainer != null && GetText(eParamIdContainer) != "" && nTab == parseInt(eParamIdContainer.getAttribute("tab")) && bInList)
        );

    // Nombre de ligne en mode liste automatique - Pour que le calcul de changement de page soit correct
    var nRows = 0;
    if (oeParam.GetParam('Rows') != '')
        nRows = oeParam.GetParam('Rows');

    /*var oeParam = document.getElementById('eParam').contentWindow;
    if (oeParam.GetParam('Page_' + nTab) != '')
    nPage = oeParam.GetParam('Page_' + nTab);
    */
    var url = "mgr/eFileManager.ashx";
    if (bAsync)
        url = "mgr/eFileAsyncManager.ashx";

    var oFileUpdater = new eUpdater(url, 1);

    // En cas d'erreur, retourne au mode liste ou ferme le setwait si popup
    var bPopup = isPopup();

    var fileWidth = GetFileWidth();
    if (fileWidth > 0)
        oFileUpdater.addParam("width", fileWidth, "post");

    if (bPopup) {
        oFileUpdater.ErrorCallBack = function () { setWait(false); };
        //Dans le cas d'un appel sur popup on fait passer la taille de la pop up d'origine
        var currentModal = parent.document.getElementById(_parentIframeId);

        var nInnerHeight;
        if (parent.window.innerHeight)
            nInnerHeight = parent.window.innerHeight;
        else
            nInnerHeight = parent.document.documentElement.clientHeight;

        nInnerHeight = Math.round((9 / 10) * nInnerHeight);

        // #AABBA demande #75 573 le corps de mail  diminue lorsque on dissocie un rattachement
        if (typeof height === "number" && height != 0)
            oFileUpdater.addParam("height", height, "post");
        else
            oFileUpdater.addParam("height", nInnerHeight, "post");

        var fileDiv = document.getElementById("fileDiv_" + nGlobalActiveTab);
        if (fileDiv) {
            var parentTab = getAttributeValue(fileDiv, 'tabfrom');
            if (parentTab == '')
                parentTab = '0';
            oFileUpdater.addParam("parenttab", parentTab, "post");
        }

        ////*****************************************************************************
        ////On fait du traitement de masse - Affectation de masse
        ////KHA 11/05/2015 en doublon avec efieldEditor.js-applyRuleOnBlank 
        ////où la valeur est également transmise pour les ajouts depuis un filtre (Invitations ++)
        ////le serveur reçoit actuellement "true, 1" et ne l'interprète pas bien
        //if (document.getElementById('ga_' + nTab)) {
        //    var bIsGA = document.getElementById('ga_' + nTab).value == "1" ? true : false;
        //    oFileUpdater.addParam("globalaffect", bIsGA ? "1" : "0", "post");
        //}
    }
    else {
        oFileUpdater.ErrorCallBack = function () {
            setWait(false);
            goTabList(nGlobalActiveTab, 1);
        };
    }

    try {
        oFileUpdater.addParam("callby", arguments.callee.name.toString(), "post");
        oFileUpdater.addParam("callbyarguments", Array.from(arguments.callee.arguments).join(), "post");
        oFileUpdater.addParam("callbycaller", arguments.callee.caller.name.toString(), "post");
    }
    catch (e) {

    }

    oFileUpdater.addParam("tab", nTab, "post");
    oFileUpdater.addParam("fileid", nFileId, "post");
    if (nRows > 0)
        oFileUpdater.addParam("rows", nRows, "post");

    if (!bIsIdsListFilled) {
        if (eParamIdContainer)
            eParamIdContainer.parentNode.removeChild(eParamIdContainer);

        oFileUpdater.addParam("loadfileids", true, "post");
        oFileUpdater.addParam("pagechange", bPageChange, "post");
        oFileUpdater.addParam("pageindex", iPageIndex, "post");
    }
    if (nType != null)
        oFileUpdater.addParam("type", nType, "post");

    if (isPopup())
        oFileUpdater.addParam("popup", 1, "post");

    return oFileUpdater;
}

//liste non exhaustive à compléter
var LOADFILEFROM = { UNDEFINED: 0, TAB: 1, BKMFILE: 2, LINK: 3, ENGINE: 4, ADMIN: 5, REFRESH: 6, MODERESUME: 7, NOTIF: 8 };
//le contexte est-il compatible avec le mode fiche en signet?
function isBkmFileCompliant(loadFrom) {
    return loadFrom == LOADFILEFROM.BKMFILE
        || loadFrom == LOADFILEFROM.ENGINE;
}


/**
 * Indique si l'onglet passé en paramètre utilise la nouvelle ergonomie Iris, ou non.
 * @param {int} nTab onglet à tester
 * @param {string} type permet de définir si c'est le nouveau mode fiche ou le nouveau mode liste.
 */
function isIris(nTab, type) {
    /**
    * Si toutes les conditions sont réunies, on passe sur la nouvelle ergo et on renvoie true.
    * Sinon, non.
    * */
    type = type || "dvIrisBlackInput";

    var objThm;
    var objUser;

    var paramWin = top.getParamWindow();
    var bIsIris = false;

    // US #1330 - Tâches #2748, #2750 - Il arrive parfois que eParamIFrame ne soit
    // pas totalement disponible, notamment lors d'un rechargement en cours. Pour
    // éviter qu'un appel à isIris via setWait plante si GetParam est indisponible,
    // on préférera renvoyer null dans ce cas de figure, ce qui, dans la plupart
    // des utilisations de isIris(), sera équivalent à false, à moins de faire
    // explicitement la comparaison avec ===
    // Renvoyer null permet ainsi de distinguer le cas "ce n'est pas IRIS" du cas
    // "impossible de le déterminer avec exactitude"
    if (!paramWin || typeof (paramWin.GetParam) != "function")
        return null;

    try {
        objThm = JSON.parse(paramWin.GetParam("currenttheme").toString())?.Version > 1;
    } catch (e) {
        return false;
    }

    try {
        objUser = JSON.parse(paramWin.GetParam("User").toString());
    } catch (e) {
        objUser = { UserLevel: UserLevel.LEV_USR_1 };
    }


    if (!objThm)
        return false;

    var tabOngletsNelleErgo = paramWin.GetParam(type)?.split(";");
    var tabOngletsNelleErgoPreview = paramWin.GetParam(type + "Preview")?.split(";");

    if (tabOngletsNelleErgo?.length > 0 && tabOngletsNelleErgo.indexOf(nTab.toString()) > -1)
        bIsIris = true;

    if (tabOngletsNelleErgoPreview?.length > 0
        && tabOngletsNelleErgoPreview.indexOf(nTab.toString()) > -1
        && objUser?.userLevel > UserLevel.LEV_USR_5)
        bIsIris = true;
    
    return bIsIris;
};


/**
 * Interface pour administrer le nouveau mode fiche pour l'admin.
 * @param {any} nTab
 * @param {any} nFileId
 */
function initAdminSubInterface(nTab, nFileId, dir) {
    let paramWin;
    let objThm;

    try {
        paramWin = top.getParamWindow();
        objThm = JSON.parse(paramWin.GetParam("currenttheme").toString());
    } catch (e) {
        objThm = THEMES.BLANCROUGE;
    }

    if (objThm.Version > 1) {

        addScript("../IRISBlack/Front/Scripts/eInitNewErgo", "HOMEADMIN", function () {
            LoadIrisSubMenuAdmin(dir + "/IRISBlack/Front/Scripts/", { nTab: nTab, nFileId: nFileId });
        });
    }
}

/*********************************************************/
/*      FICHE                                            */
/*      loadfile ---> loadFileTrt ---> updateFile        */
/*********************************************************/

///Demande de  fiche
// nType: consultation = 2; Modification = 3; création = 5
async function loadFile(nTab, nFileId, nType, bAsync, loadFrom, srcEltId, srcElt) {
    // console.log("nTab : " + nTab + " | nFileId : " + nFileId + " | nType : " + nType + " | bAsync : " + bAsync + " | loadFrom : " + loadFrom + " | srcEltId : " + srcEltId);
    // avant chargement

    var oParamGoTabList = null;
    // US #4291 - TK #6962 - Demande #94 697 (et #94 797 ?) - On n'affiche pas de skeleton si on recharge la même fiche sur E17
    var bReloadsSameFile = (document.getElementById("fileDiv_" + nTab) && getAttributeValue(document.getElementById("fileDiv_" + nTab), "fid") == nFileId);
    if (!bReloadsSameFile) {
        oParamGoTabList = {
            to: nType,
            nTab: nTab,
            context: "eMain.loadFile"
        }
    }

    if (top.oNewFileMainVueJSApp)
        top.oNewFileMainVueJSApp.destroyVue();


    if (top.irisHomeAdminVue)
        top.irisHomeAdminVue.$destroy();

    var oMainDiv = document.getElementById("app-menu");

    if (oMainDiv && isIris(nTab)) {
        oMainDiv.remove();
    }

    if (!oMainDiv && !isIris(nTab)) {
        oMainDiv = document.createElement("div");
        oMainDiv.id = "app-menu";
        oMainDiv.innerHTML = "";
    }

    

    var loc = window.location.pathname;
    var dir = loc.substring(0, loc.lastIndexOf('/'));

    if (isIris(nTab) && loadFrom != LOADFILEFROM.BKMFILE) {

        var fnAddScript = addScript;

        if (window.top !== window.self) {
            fnAddScript = window.top.addScript;
        }

        var fnAddScriptCallback = function () {

            if (window.top !== window.self) {
                window.top.LoadIrisFile(nTab, nFileId, dir + "/IRISBlack/Front/Scripts/");
                return;
            }

            LoadIrisFile(nTab, nFileId, dir + "/IRISBlack/Front/Scripts/");
        }

        // Inutile de rajouter eInitNewErgo à chaque appel de shFileInPopup, s'il a déjà été inclus, et la fonction déjà disponible...
        if (typeof (window.top.LoadIrisFile) == "function")
            fnAddScriptCallback();
        else {
            fnAddScript("../IRISBlack/Front/Scripts/eInitNewErgo", "FILE", fnAddScriptCallback);
        }

        return;
    }

    initAdminSubInterface(nTab, nFileId, dir);
    //Appel le waiter
    setWait(true, undefined, undefined, nTab != nGlobalActiveTab, oParamGoTabList);

    // nType: consultation = 2; Modification = 3; création = 5
    // cf enum : eFileType
    if (typeof (nType) == "undefined" || nType == 2)
        nType = 3;

    if (typeof (loadFrom) == "undefined")
        loadFrom = LOADFILEFROM.UNDEFINED;

    if (!nTab > 0) {
        setWait(false, undefined, undefined, isIris(top.getTabFrom()));
        eAlert(0, top._res_416, top._res_6237, "undefined tab");
        return;
    }

    if (!nFileId > 0) {
        setWait(false, undefined, undefined, isIris(top.getTabFrom()));
        eAlert(0, top._res_416, top._res_6237, "undefined id");
        return;
    }

    if (loadFrom == LOADFILEFROM.NOTIF && srcElt) {
        findUpByClass(srcElt, "notifItem").querySelector(".notifTagRead").click();
    }

    //#53413 Et on sauvegarde la navigation vers le mode liste actuellement affiché, afin que le bouton "Mode Liste" redirige vers celui-ci
    oNavManager.SaveMode('List', nTab, nGlobalActiveTab);

    // Utile pour le retour de l'Engine
    modeList = false;

    // Maj de JS
    clearHeader("FILE", "JS");
    clearHeader("FICHEIRIS", "CSS");

    var tabScript = new Array();
    tabScript.push("eFile");
    tabScript.push("eAutoCompletion");
    tabScript.push("ePopup");
    tabScript.push("eFieldEditor");
    tabScript.push("eGrapesJSEditor");
    tabScript.push("eMemoEditor");
    tabScript.push("ckeditor/ckeditor");
    tabScript.push("grapesjs/grapes.min");
    tabScript.push("grapesjs/grapesjs-plugin-ckeditor.min"); // plugin d'interfaçage grapesjs <=> CKEditor
    tabScript.push("grapesjs/grapesjs-blocks-basic.min");
    tabScript.push("grapesjs/grapesjs-preset-newsletter.min");
    tabScript.push("grapesjs/grapesjs-preset-webpage.min");

    // Si on est sur le nouveau mode Fiche, pas d'inclusion des anciennes CSS
    //MAJ DES CSS 
    addCss("eFile", "FILE");
    addCss("grapesjs/grapes.min", "FILE");
    addCss("grapesjs/grapesjs-preset-newsletter", "FILE");
    addCss("grapesjs/grapesjs-preset-webpage", "FILE");
    addCss("eMemoEditor", "FILE");

    if (loadFrom == LOADFILEFROM.ADMIN) {
        var mainDiv = document.getElementById("mainDiv");
        mainDiv.innerHTML = "";
        clearHeader("ADMIN", "CSS");
        clearHeader("ADMINFILE", "CSS");
        switchDefaultThemeToUserTheme();
    }

    if (nTab == TAB_PP) {
        addCss("eVCard", "FILE");
    }
    else if (nTab == TAB_CAMPAIGN) {

        addCss("eStats", "FILE");
        tabScript.push("eStats");
    }

    if (nType == 3 || nType == 5) {
        addCss("eEditFile", "EDITFILE");
    }
    else {
        clearHeader("EDITFILE", "CSS");
    }

    // TODO IRISBLACK LEGACY
    // Les scripts énumérés plus haut sont chargés sur le nouveau et l'ancien mode Fiche, pour réutiliser les fonctions legacy en attendant leur nouvelle version
    // Mais le callback n'est pas le même
    var fileAddScriptsCallbackFunction = (function (paramnTab, paramnFileId, paramnType, parambAsync, paramLoadFrom, paramSrcEltId) {
        return function () {
            loadfiletrt(paramnTab, paramnFileId, paramnType, parambAsync, paramLoadFrom, paramSrcEltId);

        }
    })(nTab, nFileId, nType, bAsync, loadFrom, srcEltId);

    addScripts(tabScript, "FILE", fileAddScriptsCallbackFunction);

    // Chargement de l'ancien mode Fiche si le nouveau n'est pas activé
    oEvent.fire("mode-file", { tab: nTab, fileId: nFileId });

    setTimeout(function () {
        clearHeader("LISTIRIS", "ALL");
    }, 500);

}

/*********************************************************/
/*      FICHE                                            */
/*      loadFileTrt ---> updateFile        */
/*********************************************************/
function loadfiletrt(nTab, nFileId, nType, bAsync, loadFrom, srcEltId) {


    var bIsFileInBkm = false;
    if (!isPopup()) {
        //Si ActiveBkm == -1 (TOUS) : on met le mode asynchrone (bAsync=true) sinon le mode synchrone
        // KHA pour mettre en place la pagination automatique on doit passer par le mode asynchrone: 
        bAsync = true;

        var nActiveBkm = 0;
        var oeParam = getParamWindow();
        if (oeParam.GetParam('ActiveBkm_' + nTab) != '')
            nActiveBkm = parseInt(oeParam.GetParam('ActiveBkm_' + nTab));


        //le mode asynchrone n'est pas nécessaire si le signet sélectionné est un champ note
        if (nActiveBkm % 100 == MEMO_DESCRIPTION ||
            nActiveBkm % 100 == MEMO_NOTES ||
            nActiveBkm % 100 == MEMO_INFOS)
            bAsync = false;

        //Mode Fiche dans le signet 
        if (isBkmFile(nTab, nFileId) && isBkmFileCompliant(loadFrom)) {
            bAsync = false;
            bIsFileInBkm = true;
        }

        if (loadFrom == LOADFILEFROM.MODERESUME)
            bAsync = false;

        if (isBkmGrid(nActiveBkm))
            bAsync = true; // maintenir async à true si signet grille svp
    }


    try {
        var oFileUpdater = getFileUpdater(nTab, nFileId, nType, bAsync);
        if (bAsync) {
            oFileUpdater.addParam("part", 1, "post");
        }
        else {
            oFileUpdater.addParam("mainheight", document.getElementById("mainDiv").offsetHeight, "post");
            //pour les fiches en signet, on simule un affichage en popup
            if (bIsFileInBkm) {
                //NHA #1109 - Tache : Transformer Adresse Perso en adresse Pro (et inversement) 
                //en mode incrusté pas de popup 
                // SPH : provoque des effets indésirable. Une modif plus ciblé a é'te faire
                oFileUpdater.addParam("popup", 1, "post");
                oFileUpdater.addParam("bkmfile", 1, "post");
            }

            // Envoi du param pour le mode résumé
            if (loadFrom == LOADFILEFROM.MODERESUME && srcEltId) {
                oFileUpdater.addParam("modeResume", getAttributeValue(srcEltId, "rmode"), "post");
            }
        }
        var bPopUp = isPopup();

        if (bPopUp)
            oFileUpdater.ErrorCallBack = function () {

                if (top.eModFile && typeof (top.eModFile.hide) == "function") { top.eModFile.hide() }
            };
        else {
            //en cas d'erreur , on redige sur l'accueil
            oFileUpdater.ErrorCallBack = function () {

                top.setWait(false);
                try {
                    top.goTabList(nTab);
                }
                catch (eee) {

                    try {
                        top.goTabList(0);
                    }
                    catch (ee) { }

                }
            }
        }

        oFileUpdater.send(function (oRes) {

            if (loadFrom == LOADFILEFROM.MODERESUME)
                oEvent.fire("file-resume-changed", { 'tab': nTab, 'fileId': nFileId });

            updateFile(oRes, nTab, nFileId, nType, bAsync, srcEltId, null, false, loadFrom);


        });



        if (!bIsFileInBkm) {
            if (!bPopUp && nTab != 101000)
                loadFileMenu(nTab, nType, nFileId, InitNavigateButton);
            else
                InitNavigateButton();
        }

    }
    catch (e) {
        alert('eMain.loadFile');
        alert(e.description);
        setWait(false, undefined, undefined, isIris(top.getTabFrom()));
    }
}

// Savoir si le signet et un Signet Grille
function isBkmGrid(nBkm) {
    var EdnType_GRID = 24;// signet grille
    var bkm = document.getElementById("BkmHead_" + nBkm);
    return bkm && getAttributeValue(bkm, "edntype") * 1 == EdnType_GRID;
};

function getReadOnlyFld(oElem) {

    var browser = new getBrowser();
    if (browser.isIE && !browser.isIE10) {
        var tmp = oElem.querySelectorAll("td[ename],input[ename]")


        var arr = new Array();
        for (var nCmptFld = 0; nCmptFld < tmp.length; nCmptFld++) {
            if (getAttributeValue(tmp[nCmptFld], "ero") == "1")
                arr.push(tmp[nCmptFld]);
        }

        return arr;

    }
    else
        return oElem.querySelectorAll("td[ename][ero='1'],input[ename][ero='1']")

}


///Met à jour pour le mode fiche
// nType: consultation = 2; Modification = 3; création = 5
// eltId : element a mettre en évidence apres une maj
var aFieldLstBefore = [];



function updateFile(oRes, nTab, nFileId, nType, bAsync, eltId, eltIdY, bFromApplyRuleOnBlank, loadFrom) {

    // On créait un object pour renvoyer au setWait ou es ce qu'on veux aller
    var oParamGoTabList = {
        to: nType,
        nTab: nTab,
        context: "eMain.updateFile"
    }

    var paramWin = top.getParamWindow();
    var objThm = JSON.parse(paramWin.GetParam("currenttheme").toString());

    if (!nTab || (!nFileId && nFileId != 0))
        return;

    var oEltScroll;
    if (eltId)
        oEltScroll = document.getElementById(eltId);

    if (eltId && oEltScroll && !eltIdY) {
        try {
            eltIdY = getAbsolutePosition(oEltScroll).y;
        }
        catch (exp) { }
    }



    if (typeof (bFromApplyRuleOnBlank) != "boolean")
        bFromApplyRuleOnBlank = false;

    var sBkmToClone = "";
    var sBkmTitleBar = "";
    var oMainDiv = document.getElementById("mainDiv");

    //signet mode fiche incrusté
    var bkmTitle;
    var bBkmFile = isBkmFile(nTab, nFileId) && isBkmFileCompliant(loadFrom);
    if (bBkmFile) {
        oMainDiv = eTools.getBookmarkContainer(nTab);
        bkmTitle = document.getElementById("bkmTitle_" + nTab);
        var eltHead = document.getElementById("BkmHead_" + nTab)
        setAttributeValue(eltHead, "fid", nFileId);
        setAttributeValue(eltHead, "pg", getAttributeValue(bkmTitle, "pg"));
    }

    // #92 393 - Avant de rafraîchir le signet, on récupère le numéro de la page en cours pour pouvoir le passer à AddBkm() AVANT d'écraser le contenu (et donc de perdre l'information)
    var nCurrentBkmPage = 1;
    var nActivBkm = getActiveBkm(nTab);
    var eltBkmNumPage = document.getElementById("bkmNumPage_" + nActivBkm);
    if (eltBkmNumPage)
        nCurrentBkmPage = getNumber(eltBkmNumPage.value);

    if (oRes != null) {
        // Si on vient d'un applyrulesonblank, les champs qui sont passé en readonly du a cela doivent quand même
        // être fourni a l'engine. On doit donc les flagger en temps que tel
        if (bFromApplyRuleOnBlank && !bGblFromApplyRuleOnBlank) {
            aFieldLstBefore = getReadOnlyFld(oMainDiv);

        }

        bGblFromApplyRuleOnBlank = bFromApplyRuleOnBlank;

        var divBkmToclone = document.getElementById("BkmsCloneDiv")
        if (divBkmToclone)
            sBkmToClone = divBkmToclone.outerHTML;

        if (bkmTitle)
            sBkmTitleBar = bkmTitle.outerHTML;

        // Initialise les scripts pour les tablettes tactiles
        if (isTablet() && areTabletCustomScrollersEnabled()) {
            oMainDiv.innerHTML = '<div id="scroller">' + oRes + "</div>";
            loadTabletScrollers();
        }
        else {
            oMainDiv.innerHTML = sBkmTitleBar + oRes;
        }

        if (bFromApplyRuleOnBlank) {
            var aFieldLstAfterRules = getReadOnlyFld(oMainDiv);

            //recherche des champs passé en readonly par le rulesonblank
            for (var nCmptAfter = 0; nCmptAfter < aFieldLstAfterRules.length; nCmptAfter++) {

                var bFound = false;
                var fldA = aFieldLstAfterRules[nCmptAfter];


                for (var nCmptBefore = 0; nCmptBefore < aFieldLstBefore.length; nCmptBefore++) {
                    var fldB = aFieldLstBefore[nCmptBefore];
                    if (getAttributeValue(fldA, "ename") == getAttributeValue(fldB, "ename")) {
                        bFound = true;
                        break;
                    }
                }

                // passé en readonly a cause de l'applyrule
                if (!bFound) {

                    setAttributeValue(fldA, "readonlyonblank", "1");

                }
            }
        }

        if (nTab == TAB_CAMPAIGN && typeof (loadStatsCharts) == "function") {
            loadStatsCharts();
        }
    }
    else {
        //   oRes = document.getElementById("mainDiv").innerHTML;
    }

    var bDoUpdateMru = false;
    var oFileDiv = document.getElementById("fileDiv_" + nTab);
    bDoUpdateMru = (oFileDiv && oFileDiv.getAttribute("edntype") == "0" && nTab.toString() != "400");

    if (sBkmToClone != "" && oFileDiv) {
        oFileDiv.innerHTML += sBkmToClone;
    }

    fileInitObject(nTab, !bBkmFile, loadFrom, nType);

    // Masquer l'icône d'annulation de saisie si aucun champ n'autorise l'annulation
    var fieldsAllowingCancelling = document.querySelector(".table_labels[cclval='1']");
    if (!fieldsAllowingCancelling) {
        var btnCancel = document.getElementById("btnCancelLastModif_" + nTab);
        if (btnCancel)
            btnCancel.style.display = "none";
    }

    //Nettoie le header des css et script de LISTE
    clearHeader("LIST");
    clearHeader("HOMEPAGE");

    if (loadFrom == LOADFILEFROM.ADMIN)
        addCss("eEditFile", "EDITFILE");

    var bPopup = isPopup();
    // Chargement du menu de droite en mode fiche (2 ou 3)
    if (!bBkmFile) {

        /*if (!bPopup && nTab != 101000)
            loadFileMenu(nTab, nType, nFileId, InitNavigateButton);
        else*/
        InitNavigateButton();
    }

    loadCustomCss(nTab);

    //Titres séparateurs
    initSeparator(nTab);
    if (nTab == 200 && nFileId == 0)
        initSeparator(400);

    var oeParam = getParamWindow();

    var nOldFileId = oeParam.GetParam('FileId_' + nTab);

    // Recharge les mru
    //#36771 Et on sauvegarde la navigation vers le mode fiche en cours
    if (bDoUpdateMru && nFileId > 0)
        oeParam.RefreshMRU(nTab, function () {
            if (!bPopup)
                oNavManager.SaveMode('File', nTab, nFileId)
                    ;
        });

    if (!bAsync && !bPopup && !bBkmFile) {
        //Initialisation des bookmark
        initBkms(loadFrom);
    }


    // On teste s'il y a des champs Mémo à initialiser
    var oMemoIdsInpt = document.getElementById("memoIds_" + nTab);
    var bInitMemoFields = (oMemoIdsInpt && oMemoIdsInpt.value.length > 0);

    if (nTab == TAB_PP && nFileId == 0) {
        var oMemoAdrIdsInpt = document.getElementById("memoIds_" + TAB_ADR);
        if (oMemoAdrIdsInpt && oMemoAdrIdsInpt.value.length > 0)
            bInitMemoFields = true;
    }

    var oDivScroll;
    oEltScroll = document.getElementById(eltId);
    if (bPopup) {
        oDivScroll = document.getElementById("divDetailsBkms");

        if (!oDivScroll)
            oDivScroll = document.getElementById("md_pl-base");
    }
    else if (oEltScroll) {
        var parentTable = findUp(oEltScroll, "TABLE");
        if (parentTable && parentTable.id && parentTable.id.indexOf("ftdbkm_") == 0)
            oDivScroll = document.getElementById("divBkmPres");
    }
    if (!oDivScroll)
        oDivScroll = oMainDiv;


    //Ajuste la taille des conteneur & charge les bkm en async
    var adjustContainers = function (callbackFct) {


        if (bPopup)
            adjustDivContainer(callbackFct);
        else {
            var bkmAdded = false;

            //Ajuste la taille du div pour la scrollbar
            adjustScrollFile(nTab);

            // Important d'appeler la fonction suivante après ajustScrollFile
            if (!bAsync && !bBkmFile)
                AjustBkmMemoToContainer(nTab);

            var nActivBkm = getActiveBkm(nTab);
            if (bAsync) {
                var bFileTabInBkm = isFileTabInBkm();

                if (nActivBkm == -1 || (nActivBkm == 0 && !bFileTabInBkm)) {
                    var nFirstBkm = getFirstBkm(nTab);

                    if (nFirstBkm > 0) {
                        bkmAdded = true;
                        AddBkm(nTab, nFileId, nFirstBkm, nActivBkm == -1, null, nCurrentBkmPage);
                    }
                }
                else if (nActivBkm > 0) {
                    bkmAdded = true;
                    AddBkm(nTab, nFileId, nActivBkm, null, nCurrentBkmPage);
                }
            }

            // HLA - Si affichage pas d'affichage de signet, on cache le bloque des signets
            //GCH le 16/06/2014 - #31359 - Le signets des notes n'est plus sauvegardé : dans ce cas le signet des notes n'est pas asynchrone donc bkmAdded = false
            if (!bkmAdded && nActivBkm <= 0 && !bBkmFile)
                swFileBkm(0);

            if (typeof callbackFct === "function")
                callbackFct();
        }

        if (eltId && oEltScroll && eltIdY) {
            scrollEltToY(oDivScroll, oEltScroll, eltIdY)
        }
    }

    if (bInitMemoFields) {
        // Lors de l'initialisation des champs Mémo, on passe en paramètre un pointeur vers une fonction qui ajustera la taille des
        // conteneurs APRES que le champ Mémo aura été complètement chargé

        adjustContainers(function () {

            //BSE : #49849 problème de d'affichage du corp du mail sur Firefox
            setTimeout(function () {

                initMemoFields(nTab, nType, eltId, function (id) {
                    var textarea = document.getElementById("edt" + id);
                    if (!textarea)
                        return;

                    var td = findUp(textarea, "TD");
                    if (td) {
                        var h = textarea.style.height;
                        if (!h) {
                            textarea = td.querySelector(".cke_editable");
                            if (textarea)
                                h = textarea.style.height;
                        }
                        td.style.height = h;

                    }
                });
            }, 200);

            if (nTab == TAB_PP && nFileId == 0) {
                initMemoFields(TAB_ADR, nType, eltId);
            }
            if (bPopup) {
                initPjLogo(nTab, nFileId);
                InitToolbarButton(nTab);
            }
            if (bBkmFile) {
                InitBkmFileToolbarButton(nTab);
            }
        });


    }

    // Si le redimensionnement n'a pas été déclenché par l'initialisation de champs Mémo, on le
    // déclenche maintenant
    if (!bInitMemoFields) {

        adjustContainers();
        if (bPopup && nTab == 112000) //pour le moment, ne cibler que le signet "E-mails non remis" 
            initPjLogo(nTab, nFileId);
    }

    // #41074 CRU : Ajout d'un try/catch car erreur sur IE8 en mode de compatibilité
    try {
        // Si la page contient des charts, ajoutes les js nécessaire
        if (oMainDiv.querySelectorAll("div[ednchartparam]").length > 0) {
            //BSE : #55 421
            if (typeof ej != "function") {
                addScript("jquery.min", "FILE",
                    function () {
                        addScript("syncFusion/jsrender.min", "FILE",
                            function () {
                                addScript("syncFusion/ej.web.all.min", "FILE",
                                    function () {
                                        addScript("eCharts", "FILE", function () { loadSyncFusionChart(); });
                                    });
                            });
                    });
            }
            else {
                addScript("eCharts", "FILE", function () { loadSyncFusionChart(); });
            }
        }
    }
    catch (e) {
    }



    // TODO : ePlanning.js a été ajouté systématiquement
    // Ce test est donc tjs vrai ce qui fait que la fonction onPlanningFileLoadComplete est appelé pour tout type de fiche
    // normalement, il ne devrait se lancer que pour les type planning
    if (top.onPlanningFileLoadComplete) {
        onPlanningFileLoadComplete('updatefile');
    }


    var oBkmBar = document.getElementById("bkmBar_" + nGlobalActiveTab);

    if (getAttributeValue(oBkmBar, "activebkm") == "-1") {
        // pour ce cas la vérification des rubriques obligatoires se fait dans la méthode updateMultiBkmCtner dans eFile.js
    }
    else if (bBkmFile) {
        // en ne transmettant ni le tab ni le fileid, on cherche tous les champs obligatoires de la fiche
        chkExistsObligatFlds(null, null, null, window);
    }
    else if (getAttributeValue(oBkmBar, "abkmvm") != "1") {// abkmvm = ActiveBkm View Mode - Mode de Visualisation du signet actif : 0 : Liste, 1 : Fiche
        chkExistsObligatFlds(null, nTab, nFileId, window);
    }


    setWait(false, undefined, undefined, isIris(top.getTabFrom()), oParamGoTabList);

    try {
        var bIsTablet = false;
        try {
            if (typeof (isTablet) == 'function')
                bIsTablet = isTablet();
            else if (typeof (top.isTablet) == 'function')
                bIsTablet = top.isTablet();
        }
        catch (e) {

        }

        if (!bIsTablet) {
            //Si crntFocus est différente de nulle et l'id de l'élément ciblé est bien défini
            if (oEltScroll) {
                var oEltFocus = oEltScroll;
                if (getAttributeValue(oEltScroll, "eltvalid"))
                    oEltFocus = document.getElementById(getAttributeValue(oEltScroll, "eltvalid"));
                //On stock l'élement ciblé dans une variable 
                // var crntElement = document.getElementById(crntFocus);

                //Lorsque l'un des cas défini en dessous est défini, le focus se fait avec un ".focus" pour les autres cas ce sera ".click"
                switch (oEltFocus.getAttribute("eaction")) {
                    case 'LNKPHONE': // Champ téléphone
                    case 'LNKWEBSIT':   // Catalogue Site Web
                    case 'LNKSOCNET':   // Catalogue Reseau Social
                    case 'LNKMAIL': // Catalogue Adresse Mail
                    case 'LNKFREETEXT': // Champ saisie libre
                    case 'LNKNUM': // Edition champ numérique
                        oEltFocus.focus();
                        // #60 518 - si l'autocomplétion est activée sur le champ, on ne redéclenche pas le clic dessus après MAJ de la valeur (remontée d'ALEB)
                        if (!oAutoCompletion.enabled(oEltFocus))
                            oEltFocus.click();
                        setFldEditor(oEltFocus, oEltFocus, oEltFocus.getAttribute("eaction"), "LCLICK", null);
                        break;
                    default:
                        //on ne lance pas eFieldEditor on met juste le focus sur le champ pour pouvoir tabuler
                        oEltScroll.oEltFocus();
                        //oEltScroll.click();
                        break;
                }

            }
            else {
                // #40 254 - A l'ouverture de la fiche, on met le focus sur le premier champ.
                // MAIS, il faut également initialiser ce champ comme lorsqu'on effectue un clic gauche dessus,
                // pour que toute modification effectuée puisse être sauvegardée via eFieldEditor.onblur => validate => update
                // Sauf sur Planning, où le premier champ est un champ Date avec action LNKFREETEXT...
                var allFreeTextFields = document.querySelectorAll('input[eaction="LNKFREETEXT"],input[class="LNKFREETEXT edit"]');

                //BSE :#45 159 dans le cas où tout les champs de l'adresse sont en lecture seul, ne pas positionner le focus  
                var element = new Array();

                for (var i = 0, n = allFreeTextFields.length; i < n; i++) {
                    if (allFreeTextFields[i] && getAttributeValue(allFreeTextFields[i], "ename") && getAttributeValue(allFreeTextFields[i], "ename") === "COL_400_492")
                        continue;
                    element.push(allFreeTextFields[i]);
                }

                if (element.length > 0) {
                    var firstFreeTextField = element[0];
                    firstFreeTextField.focus();
                    if (!document.getElementById("md_pl-base"))
                        setFldEditor(firstFreeTextField, firstFreeTextField, "LNKFREETEXT", "LCLICK", null);
                }
            }
        }

    }
    catch (e) { }

    try {
        if (isPopup()) {
            var bAutoSave = getAttributeValue(document.getElementById("mainDiv"), 'autosv') == "1";
            var bReadOnly = getAttributeValue(document.getElementById("fileDiv_" + nTab), 'ro') == "1";
            if ((bAutoSave || bReadOnly) && nFileId > 0) {
                var aBtns = top.eModFile.Buttons;
                var oParent;
                for (var i = 0; i < aBtns.length; i++) {
                    var btn = aBtns[i];
                    if (btn.id == "save" || btn.id == "savenclose" || (bReadOnly && btn.id == "histocreate")) {
                        oParent = btn.parentNode;
                        if (oParent)
                            oParent.removeChild(btn);
                    }
                    else if (btn.id == "cancel") {
                        var oLabel = btn.querySelector("div[id='" + btn.id + "-mid']");
                        if (typeof (SetText) == "function")
                            SetText(oLabel, top._res_30);
                        else {
                            if (typeof (top.SetText) == "function")
                                top.SetText(oLabel, top._res_30);
                        }
                        for (var j = 0; j < btn.children.length; j++) {
                            btn.children[j].className = btn.children[j].className.replace("button-gray-", "button-green-");
                        }
                        btn.className = btn.className.replace("button-gray", "button-green");

                    }
                    // bouton "ouvrir dans l'onglet pour les popup ouvertes depuis une spécif
                    else if (btn.id == "openintab") {
                        if (oFileDiv.getAttribute("edntype") == "0" && nTab.toString() != "400") {
                            //s'il s'agit d'un fichier principal on laisse
                        }
                        else {
                            //dans le cas contraire on retire le bouton
                            var parent = btn.parentElement;
                            parent.removeChild(btn);
                        }
                    }
                }
            }
        }
    }
    catch (e) { }

    //ELAIZ KHO  - Mise en forme de l'avatar en rond - demande 75933

    if (objThm.Version > 1 && nTab == 200 && typeof (resizeAvatarAll) == "function")
        resizeAvatarAll();

}




//indique s'il s'agit d'une fiche incrustée en signet.
function isBkmFile(nTab) {
    if (nGlobalActiveTab != nTab) {
        var bkmTitle = document.getElementById("bkmTitle_" + nTab);
        if (bkmTitle && bkmTitle.getAttribute("vm") == "1") {
            var bkmDiv = eTools.getBookmarkContainer(nTab);
            if (bkmDiv) {
                return true;
            }
        }
    }
    return false;

}

//indique s'il s'agit d'une liste en signet.
function isBkmList(nTab) {
    if (nGlobalActiveTab != nTab) {
        var bkmTitle = document.getElementById("bkmTitle_" + nTab);
        if (bkmTitle && (!bkmTitle.hasAttribute("vm") || bkmTitle.getAttribute("vm") != "1")) {
            var bkmDiv = eTools.getBookmarkContainer(nTab);
            if (bkmDiv) {
                return true;
            }
        }
    }
    return false;

}

//indique s'il s'agit d'un signet de type discussion.
function isBkmDisc(nBkm, oBkm) {
    if (!nBkm && !oBkm)
        return false;

    if (oBkm == null)
        oBkm = eTools.getBookmarkContainer(nBkm);
    return getAttributeValue(oBkm, "disc") == "1";

}

// nTab : descid de la table
// nFileId : id de la fiche a sup
// eModFile : 
// openSerie : indique si c'est une fiche planning en mode série ou un si c'est une enregistrement unique qui est ouvert
// successCallBack : callBack après sucess
function deleteFile(nTab, nFileId, eModFile, openSerie, successCallBack, bClose) {

    if (!nFileId) {
        top.eAlert(2, top._res_806, top._res_78);
        return;
    }

    if (typeof (eModFile) == "undefined")
        eModFile = null;
    if (typeof (openSerie) == 'undefined')
        openSerie = false;

    var eEngineUpdater = new eEngine();
    eEngineUpdater.Init();
    eEngineUpdater.DeleteRecord = true;
    eEngineUpdater.AddOrSetParam('tab', nTab);
    eEngineUpdater.AddOrSetParam('fileId', nFileId);
    eEngineUpdater.AddOrSetParam('openSerie', openSerie ? "1" : "0");

    if (eModFile != null && typeof (eModFile) != "undefined" && eModFile.getIframe() != null && typeof (eModFile.getIframe()) != "undefined")
        eEngineUpdater.ModalDialog = { oModFile: eModFile, modFile: eModFile.getIframe(), pupClose: bClose };
    else
        eEngineUpdater.ModalDialog = null;

    if (typeof (successCallBack) != 'undefined')
        eEngineUpdater.SuccessCallbackFunction = successCallBack;
    else if (eModFile != null && typeof (eModFile) != "undefined")
        eEngineUpdater.SuccessCallbackFunction = function () { eModFile.hide(); };
    else
        eEngineUpdater.SuccessCallbackFunction = function () { };

    eEngineUpdater.UpdateLaunch();
}

// nTab : descid de la table
// nMasterFileId : id de la fiche master
// nDoublonFileId : id de la fiche en doublons
// mergeModal : modal de fusion
function mergeFile(nTab, nMasterFileId, nDoublonFileId, mergeModal) {
    var objMerge = null;

    try {
        // Sans la modal, on ne peux recuper les données
        if (!mergeModal) {
            alert('eMain.mergeFile - modale introuvable !');
            return;
        }

        var ifrm = mergeModal.getIframe();
        if (!ifrm || !ifrm.GetReturnValue) {
            alert('eMain.mergeFile - iframe introuvable !');
            return;
        }

        // On recupère les listes de params choisis
        objMerge = ifrm.GetReturnValue();
    }
    finally {
        // On referme la fenêtre
        mergeModal.hide();
    }

    var eEngineUpdater = new eEngine();
    eEngineUpdater.Init();
    eEngineUpdater.MergeRecord = true;
    eEngineUpdater.AddOrSetParam('tab', nTab);
    eEngineUpdater.AddOrSetParam('masterFileId', nMasterFileId);
    eEngineUpdater.AddOrSetParam('doublonFileId', nDoublonFileId);

    eEngineUpdater.AddOrSetParam('keepAllAdr', objMerge.keepAllAdr ? "1" : "0");
    eEngineUpdater.AddOrSetParam('overwriteAdrInfos', objMerge.overwriteAdrInfos ? "1" : "0");

    eEngineUpdater.AddOrSetParam('fieldChange', objMerge.fieldChange);
    eEngineUpdater.AddOrSetParam('fieldConcat', objMerge.fieldConcat);

    eEngineUpdater.UpdateLaunch();
}


//initialise les titres séparateurs de pages pour qu'ils soient ouvert ou fermés en fonction des choix de l'utilisateur.
// #41074 CRU : Ajout d'un try/catch
function initSeparator(nTab) {
    try {
        if (document.getElementById("ClosedSep_" + nTab)) {
            var closedSep = document.getElementById("ClosedSep_" + nTab).value.split(';');

            for (var i = 0; i < closedSep.length; i++) {

                if (closedSep[i] == "" || closedSep[i] === undefined)
                    continue;

                //if (closedSep[i] == "global") {

                //    var oTbSysFld = document.getElementById("fts_" + nTab);  // dans le cas de PM et PP, ce tableau contient les champs dont la mise en page est spécifique et ne peut être modifiée

                //    var oTbMain = document.getElementById("ftm_" + nTab);

                //    var bHidden = getAttributeValue(oTbSysFld, "h") == "1" || getAttributeValue(oTbMain, "h") == "1";
                //    if (bHidden)
                //        continue;

                //    OpenCloseSepAll(document.getElementById('opencloseall'), true);
                //    continue;
                //}

                var separator = document.getElementById("SEP_" + nTab + "_" + closedSep[i]);
                if (separator) {
                    OpenCloseSep(separator, false, true);
                }

            }
        }
    }
    catch (e) {
    }

}


//Initialisation du bouton de PJ
// nTab : descid de la table à affichaer en pop up
// nFileId : nFileId de la fiche à affichaer en pop up (0 si nouvelle fiche)
function initPjLogo(nTab, nFileId) {
    try {
        var ndescIdPJ = getNumber(nGlobalActiveTab) + getNumber(91);

        var oPjInfo_Inpt = document.getElementById("COL_" + nGlobalActiveTab + "_" + ndescIdPJ + "_" + nFileId + "_" + nFileId + "_0");
        var viewPJ = false;
        if (oPjInfo_Inpt)
            viewPJ = true;

        var modfile = null;
        if (parent.eModFile)
            modfile = parent.eModFile;
        //else if (parent.tplFileModal)         //tplFileModal est remplacé par eModFile
        //    modfile = parent.tplFileModal;

        if (modfile != null) {
            modfile.setToolBarVisible(modfile.ToolbarButtonType.PjButton, viewPJ);
            var nbPj = getAttributeValue(oPjInfo_Inpt, "nbpj");
            modfile.setButtonLabel(modfile.ToolbarButtonType.PjButton, "(" + nbPj + ")");
        }
    }
    catch (ex) {
        alert('initPjLogo : ' + ex.message + ' ' + ex.description);
    }

}

//Initialisation des boutons du hauts de template
function InitToolbarButton(nTab) {

    try {
        var oRightInfo = document.getElementById("rightInfo_" + nTab);
        // MCR 40833 sur un Email Existant, ajout du bouton supprimer pour donner la possibilite de supprimer le mail en mode fiche 
        // init du boolean a true au lieu de false 

        //BSE #49380 Cacher le bouton imprimer sur les template en mode fiche si on a pas les droits
        var bDel = true;
        var bPrint = true;
        var bCancelVal = false;
        if (oRightInfo) {
            bDel = getAttributeValue(oRightInfo, "del") == "1";
            bPrint = getAttributeValue(oRightInfo, "print") == "1";
            bCancelVal = getAttributeValue(oRightInfo, "cclval") == "1";
        }

        var modfile = null;
        if (parent.eModFile)
            modfile = parent.eModFile;
        else if (eTools.getModalFromWindowName(window.name, nTab))
            modfile = eTools.getModalFromWindowName(window.name, nTab);
        //else if (parent.tplFileModal)         //tplFileModal est remplacé par eModFile
        //    modfile = parent.tplFileModal;
        if (modfile != null) {
            modfile.setToolBarVisible(modfile.ToolbarButtonType.DeleteButton, bDel);
            modfile.setToolBarVisible(modfile.ToolbarButtonType.PrintButton, bPrint);
            modfile.setToolBarVisible(modfile.ToolbarButtonType.CancelLastValuesButton, bCancelVal);
        }


    }
    catch (ex) {
        alert('InitToolbarButton : ' + ex.message + ' ' + ex.description);
    }

}

//CNA - Initialisation des boutons du hauts de d'une fiche en mode signet inscrusté
function InitBkmFileToolbarButton(nTab) {

    try {
        var oRightInfo = document.getElementById("rightInfo_" + nTab);
        var bDel = true;
        if (oRightInfo)
            bDel = getAttributeValue(oRightInfo, "del") == "1";

        var oBtnDelete = document.getElementById("bkmButtonsBar_" + nTab + "_delete");
        if (oBtnDelete) {
            if (bDel) {
                oBtnDelete.style.removeProperty("display");
                oBtnDelete.style.visibility = "visible";
            } else {
                oBtnDelete.style.display = "none";
                oBtnDelete.style.visibility = "hidden";
            }
        }
    }
    catch (ex) {
        alert('InitBkmFileToolbarButton : ' + ex.message + ' ' + ex.description);
    }

}


/**
 * Fonction retournant l'ensemble des IDs des instances d'eMemoEditor se trouvant sur le document oDoc
 * Pour accéder ensuite aux instances en question, utiliser getMemoEditor(id, oDoc)
 * @param {any} Objet Document sur lequel cibler la recherche. Si null ou undefined, la vérification portera sur window.document
 */
nsMain.getAllMemoEditorIDs = function (oDoc) {
    if (!oDoc)
        oDoc = document;

    if (oDoc.aMemoEditors)
        return Object.keys(oDoc.aMemoEditors);
    else
        return new Array();
};

/**
 * Fonction retournant une instance d'eMemoEditor correspondant au nom ou à l'index donné en paramètre sur le document oDoc
 * @param {any} ID de l'instance à récupérer, soit une String, soit en numéro d'index/ordre sur le document (par ordre d'instanciation)
 * @param {any} Objet Document sur lequel cibler la recherche. Si null, false ou undefined, la vérification portera sur window.document
 */
nsMain.getMemoEditor = function (idOrIndex, oDoc) {
    if (!oDoc)
        oDoc = document;

    if (typeof (idOrIndex) == "string")
        return oDoc.aMemoEditors[idOrIndex];
    else if (typeof (idOrIndex) == "number")
        return oDoc.aMemoEditors[Object.keys(oDoc.aMemoEditors)[idOrIndex]];
    else
        return null;
};

/**
 * Ajoute/met à jour une instance d'eMemoEditor sur le document oDoc
 * Ajoute un message dans la console si l'instance est mise à jour avec une valeur null, false ou undefined
 * @param {string} ID de l'instance à ajouter ou mettre à jour
 * @param (any) Objet eMemoEditor à ajouter/mettre à jour
 * @param {any} Objet Document sur lequel cibler la mise à jour. Si null ou undefined, la vérification portera sur window.document
 */
nsMain.setMemoEditor = function (id, memoEditor, oDoc) {
    if (!oDoc)
        oDoc = document;

    if (!memoEditor) {
        console.log("Attention, remplacement/mise à jour d'une instance existante de champ Mémo avec une valeur null ou undefined : " + id);
    }

    oDoc.aMemoEditors[id] = memoEditor;
};

/**
 * Renvoie true si le document oDoc dispose d'une ou plusieurs instances d'eMemoEditor accessibles via getMemoEditor(), setMemoEditor() et getAllMemoEditorIDs().
 * Sinon, renvoie false (tableau null ou undefined, ou contenant aucun élément eMemoEditor)
 * @param {any} Objet Document sur lequel cibler la mise à jour. Si null ou undefined, la vérification portera sur window.document
 */
nsMain.hasMemoEditors = function (oDoc) {
    if (!oDoc)
        oDoc = document;

    return nsMain.getAllMemoEditorIDs(oDoc).length > 0;
};

/**
 * Initialise le tableau interne au document oDoc, référençant tous les objets eMemoEditor présents sur le document.
 * Si cette fonction est appelée alors qu'il existe déjà un tableau instancié et rempli, un message sera émis dans la console du navigateur.
 * @param {any} Objet Document sur lequel cibler la mise à jour. Si null ou undefined, la vérification portera sur window.document
 */
nsMain.initMemoEditorsArray = function (oDoc) {
    if (!oDoc)
        oDoc = document;

    if (nsMain.hasMemoEditors(oDoc)) {
        console.log('Attention, REINITIALISATION de tableau aMemoEditors déjà initialisé et rempli !');
    }

    oDoc.aMemoEditors = new Array();
};

var aMemosFldsStatus = new Array();
// nType : type de vue (consultation, modification...)
// strTrgEltId : id de l'éventuel élément appelant un update graphique de la fiche (ex : champ impliqué dans une règle de visu)
// afterEachMemoInit : callback après init de chaque mémo
// afterAllMemoInit : callback après init de tous les mémos
function initMemoFields(nTab, nType, strTrgEltId, afterEachMemoInit, afterAllMemoInit) {



    if (!(nType == 2 || nType == 3 || nType == 5))
        return;

    //champs memo
    var oMemoIdsInpt = document.getElementById("memoIds_" + nTab);

    var oMemoIdsInptLst = document.querySelectorAll("input[ednmemoIds_" + nTab + "='1']");
    if (oMemoIdsInptLst && oMemoIdsInptLst.length > 0) {
        var arrInpt = [];
        [].forEach.call(oMemoIdsInptLst, function (elem) {
            if (elem.value && elem.value.length > 0) {
                elem.value.split(";").forEach(function (val) {
                    if (arrInpt.indexOf(val) == -1)
                        arrInpt.push(val)
                })
            }
        });

        var arrInptDeDupe = arrInpt.filter(function (eleme, pos, curr) {
            return curr.indexOf(eleme) == pos;
        })
        if (oMemoIdsInpt)
            oMemoIdsInpt.value = arrInpt.join(';');
    }



    //Initialise le tableau destiné à contenir tous les objets de types eMemoEditor dans les cas de la modification et de la création
    // #83 082 : on ne le réinitialise pas s'il existe déjà. Permet de ne pas écraser le contexte aux yeux d'un champ Mémo enfant (ex : affiché via une fenêtre Plein écran) si une MAJ a lieu en arrière-plan, et provoque le réaffichage complet de toute l'application (RefreshFile global), rendant ainsi la Modal Dialog du champ Mémo Plein écran orpheline.
    // Charge au code qui se chargera de remplir ce tableau précédemment remis à zéro, de faire les MAJ appropriées sur un tableau existant
    //if (!nsMain.hasMemoEditors())	
    //nsMain.initMemoEditorsArray();

    if (oMemoIdsInpt && oMemoIdsInpt.value.length > 0) {
        var oBrowser = new getBrowser();

        var aMemosIds = oMemoIdsInpt.value.split(';');

        for (var i = 0; i < aMemosIds.length; i++) {
            if (aMemosIds[i].length == 0)
                continue;

            aMemosFldsStatus.push({ memoId: aMemosIds[i], status: 0 });
        }

        var currentView = getCurrentView(document);
        for (var i = 0; i < aMemosIds.length; i++) {
            if (aMemosIds[i].length == 0)
                continue;

            aMemosFldsStatus.push({ memoId: aMemosIds[i], status: 0 });

            //Consultation
            //On tente d'abord de charger directement le contenu du champ Mémo dans son iframe
            updateFileMemo(aMemosIds[i], currentView, strTrgEltId, afterEachMemoInit);


            //Puis on vérifie que le chargement ait bien eu lieu
            //Si le chargement a échoué parce que la fenêtre de l'iframe n'est pas prête, on affecte ce chargement sur son évènement onLoad
            var iFr = document.getElementById(aMemosIds[i] + "ifr"); //iframe destinée à contenir le champ memo
            if (iFr) {
                var oDoc = iFr.contentWindow || iFr.contentDocument;
                if (oDoc.document) {
                    oDoc = oDoc.document;
                }

                if (!oDoc || !oDoc.body || oDoc.body.className == '') {
                    var strMemoId = aMemosIds[i];
                    // Pour passer un paramètre (ici strMemoId) à l'intérieur d'une fonction anonyme, il faut l'encapsuler dans une variable JS
                    // qui sera un pointeur vers un appel de fonction comportant le paramètre en question
                    var fct = (function (Id) { return function () { updateFileMemo(Id, currentView, strTrgEltId, afterEachMemoInit); } })(strMemoId);
                    // Ajout d'un évènement au chargement de l'iframe pour récupérer son contenu
                    setEventListener(iFr.contentWindow, 'load', fct);

                }
            }

        }

        // A la fin de init de tous les mémos, on fait le callback
        // ca evite de charger le même signet autant de fois que les champ mémos
        if (typeof (afterAllMemoInit) == 'function')
            afterAllMemoInit();
    }
}

function completeMemo(sMemoId) {
    for (var i = 0; i < aMemosFldsStatus.length; i++) {
        if (aMemosFldsStatus[i].memoId == sMemoId) {
            aMemosFldsStatus[i].status = 1;
        }
    }
}

function isCompleteAllMemo() {
    var bAllComplete = true;
    for (var i = 0; i < aMemosFldsStatus.length; i++) {
        if (aMemosFldsStatus[i].status == 0) {
            var aMemoId = aMemosFldsStatus[i].memoId.split("_")
            var nTab = getNumber(aMemoId[1]);
            var nFldDescid = getNumber(aMemoId[2]);
            var nFldFileId = getNumber(aMemoId[aMemoId.length - 2]);

            //on ne prend pas en compte les champs memo en provenance des fichiers liés si la liaison est vide.
            if (nTab != getTabDescid(nFldDescid) && nFldFileId == 0)
                continue;
            return false;
        }
    }

    return bAllComplete;


}

// Tableau destiné à contenir tous les objets de types eMemoEditor dans les cas de la modification et de la création
// #83 082 : on ne le réinitialise pas s'il existe déjà. Permet de ne pas écraser le contexte aux yeux d'un champ Mémo enfant (ex : affiché via une fenêtre Plein écran) si une MAJ a lieu en arrière-plan, et provoque le réaffichage complet de toute l'application (RefreshFile global), rendant ainsi la Modal Dialog du champ Mémo Plein écran orpheline.
// Charge au code qui se chargera de remplir ce tableau précédemment remis à zéro, de faire les MAJ appropriées sur un tableau existant
if (!nsMain.hasMemoEditors())
    nsMain.initMemoEditorsArray();

// Met à jour les champs Mémo pour la consultation
// Peut être fait en différé par un timer pour donner le temps au navigateur de charger les iframes
//#36751 : innerText/innerHTML suivant le type de champ mémo
var currentMemoEditor;

function updateFileMemo(strMemoId, currentView, strTrgEltId, customOnShowFct) {
    var iFr = document.getElementById(strMemoId + "ifr"); //iframe destinée à contenir le champ memo
    var hid = document.getElementById(strMemoId + "hid"); //input cachée contenant la valeur du champ memo
    var isHtml = false;


    if (iFr && hid) {
        var oDoc = iFr.contentWindow || iFr.contentDocument;
        if (oDoc.document) {
            oDoc = oDoc.document;
        }

        var oParentCell = document.getElementById(strMemoId);
        if (oParentCell) {
            var editorType = getAttributeValue(oParentCell, "editortype");
            isHtml = (oParentCell.getAttribute("html") == 1);//&& (currentView != "FILE_CONSULTATION" || editorType != "mail");
        }

        if (oDoc.body && (hid.value == '' || (oDoc.body.innerHTML != hid.value))) {

            if (isHtml) {
                oDoc.body.innerHTML = hid.value;
            }
            else {
                //72744 SPH :  Voir la demande sur hotcom pour les commentaires sur cette modif 
                if (iFr.offsetHeight > 0 || iFr.offsetWidth > 0) {
                    oDoc.body.innerText = hid.value;
                }
                else {
                    try {
                        oDoc.body.textContent = hid.value;
                    }
                    catch (e) {
                        oDoc.body.innerText = hid.value;
                    }
                }
            }
        }
        if (oDoc.body && oDoc.body.className != 'eME') {
            // Ajout des CSS de l'application à l'iframe pour que son style (dont la police par défaut) soit le même que celle de l'application
            // On ajoute une classe au body de l'iframe pour pouvoir déclarer dans les CSS des styles à appliquer au corps de page sans écraser ceux de l'application (document courant)
            addCss("eMemoEditor", "MEMOEDITOR", oDoc);
            oDoc.body.className = 'eME';

            if (typeof (currentView) == "undefined")
                currentView = getCurrentView(document);

            if (currentView == "FILE_MODIFICATION" || currentView == "FILE_CREATION" || currentView == "ADMIN_FILE") {

                var value = (isHtml) ? oDoc.body.innerHTML : oDoc.body.innerText;

                var i = nsMain.getAllMemoEditorIDs().length;

                //Modification et création
                // Instanciation de l'objet eMemoEditor
                currentMemoEditor = new eMemoEditor(
                    'edt' + strMemoId,
                    isHtml,
                    oParentCell,
                    null,
                    value,
                    false,
                    'nsMain.getMemoEditor(\'edt' + strMemoId + '\')'
                );


                // On donne à l'objet nouvellement créé un lien vers la page parente si on se trouve dans une popup eModalDialog
                if (typeof (isPopup) != "undefined" && isPopup()) {
                    currentMemoEditor.parentFrameId = _parentIframeId;
                }

                // #43881 : Conservation du childDialog si l'instance existe déjà
                var childDialog;
                if (nsMain.getMemoEditor('edt' + strMemoId)) {
                    childDialog = nsMain.getMemoEditor('edt' + strMemoId).getChildDialog();
                }

                nsMain.setMemoEditor('edt' + strMemoId, currentMemoEditor);

                if (childDialog)
                    nsMain.getMemoEditor('edt' + strMemoId).setChildDialog(childDialog);

                // "Titre" du champ Mémo = libellé du champ (notamment pour le titre de la fenêtre en mode Zoom)
                var headerElement = document.getElementById(oParentCell.getAttribute("ename"));
                currentMemoEditor.title = getAttributeValue(headerElement, "lib");

                // Si la page actuelle définit un type d'éditeur, on l'indique à eMemoEditor
                currentMemoEditor.editorType = oParentCell.getAttribute("editortype");

                // #68 13x - Type de champ Mémo - Editeur de templates HTML avancé (grapesjs) ou CKEditor
                // Si la page actuelle indique que l'on doit instancier un éditeur de templates HTML avancé (ex : pour l'e-mailing), on l'indique à eMemoEditor
                currentMemoEditor.enableTemplateEditor = oParentCell.getAttribute("enabletemplateeditor") == "1";
                //currentMemoEditor.enableTemplateEditor = false;

                // Si la page actuelle définit un type de barre d'outils spécifique à afficher, on l'indique à eMemoEditor
                currentMemoEditor.toolbarType = oParentCell.getAttribute("toolbartype");

                //tracking externalisé activé
                currentMemoEditor.externalTrackingEnabled = getAttributeValue(oParentCell, "externaltracking") == "1";

                //Gestion des consentements
                currentMemoEditor.useNewUnsubscribeMethod = getAttributeValue(oParentCell, "newunsubmethod") == "1";

                // Si la page actuelle définit un DescId et/ou un FileId, on l'indique à eMemoEditor
                // Valeurs pouvant ensuite être utilisées par update() pour la mise à jour de la valeur en base
                // Exemple #47 223 : dans le cas où l'utilisateur change de signet, le curseur sort du champ et donc, la valeur du champ Mémo doit être sauvegardée.
                // Seulement, le changement de signet entraîne un rechargement de la page... Ce qui empêche le moteur de mise à jour (eMemoEditor.update)
                // de retrouver ces infos sur le contexte de la page et fait échouer la mise à jour
                // Même problème que pour la demande #27 563
                if (!isNaN(getNumber(oParentCell.getAttribute("descid"))))
                    currentMemoEditor.descId = oParentCell.getAttribute("descid");


                // SPH #48192 : Il ne faut pas réaffecter l'id si la table du champ n'est pas celui de la table de l'id en question
                // sinon, on met à jour une fiche qui ne corrspond pas.
                if (!isNaN(getNumber(oParentCell.getAttribute("fileid")))) {

                    try {
                        var atrId = strMemoId.split('_');
                        var nMainTab = getNumber(atrId[1]);

                        var nTabDescId = getNumber(currentMemoEditor.descId);
                        nTabDescId = nTabDescId - nTabDescId % 100;
                        if (nTabDescId == nMainTab) {


                            currentMemoEditor.fileId = oParentCell.getAttribute("fileid");
                        }
                    }
                    catch (ex) {


                    }
                }


                // Si la page actuelle a défini une liste de champs de fusion utilisables sur ce champ Mémo dans une variable JS, on
                // affecte le contenu de cette variable à la propriété mergeFields de eMemoEditor pour que son plugin puisse les prendre en charge

                try {

                    if (getAttributeValue(oParentCell, "mergefieldsjsvarname") != '') {
                        currentMemoEditor.mergeFields = eval(getAttributeValue(oParentCell, "mergefieldsjsvarname"));
                        /*
                        KHA le 29/04/2015 on n'affiche pas les champs mémo dans les champs de fusion pour les formulaires
                        exceptFileFormat est définie dans eFormularWizard.js
                        */
                        // champs mémo, c'est fait;
                        // if (typeof exceptMergeFieldFormat == "function"){
                        //     currentMemoEditor.mergeFields = exceptMergeFieldFormat(currentMemoEditor.mergeFields, 9);
                        //}
                    }
                }
                catch (ex) { }


                // Si la page actuelle a défini une liste de champs de fusion utilisables sur ce champ Mémo dans une variable JS, on
                // affecte le contenu de cette variable à la propriété mergeFields de eMemoEditor pour que son plugin puisse les prendre en charge
                try {
                    if (getAttributeValue(oParentCell, "trackfieldsjsvarname") != '')
                        currentMemoEditor.oTracking = eval(getAttributeValue(oParentCell, "trackfieldsjsvarname"));
                }
                catch (ex) { }


                try {
                    if (getAttributeValue(oParentCell, "mergehyperlinkfieldsjsvarname") != '')
                        currentMemoEditor.oMergeHyperLinkFields = eval(getAttributeValue(oParentCell, "mergehyperlinkfieldsjsvarname"));
                }
                catch (ex) { }

                // ajout des listeners au ckeditor
                try {
                    if (getAttributeValue(oParentCell, "listeners") != '') {

                        var listeners = eval(getAttributeValue(oParentCell, "listeners"));
                        var handle;
                        for (evt in listeners) {
                            handle = listeners[evt];
                            //Pour l'evaluation de la function
                            if (typeof (handle) == "string" && handle.indexOf("(") != 0)
                                handle = eval("(" + handle + ")");
                            currentMemoEditor.listeners.push({ "event": evt, "handle": handle });
                        }
                    }
                }
                catch (ex) {
                }


                // scrollIntoView = Positionnement de la page (ascenseur) au niveau d'un objet HTML spécifié si demandé
                // Permet de restaurer la position actuelle de la page après l'affichage du champ Mémo (pour lequel un espace n'aura pas forcément été réservé au départ)
                /*
                //Canceled by KHA le 31/01/2013 et remplacé par une fonction asynchrone cf emain.js updateFile
                if (strTrgEltId) {
                currentMemoEditor.scrollIntoViewId = strTrgEltId;
                currentMemoEditor.scrollOnShow = true;
                }
                */
                // Si on passe en paramètre une fonction à exécuter après affichage du champ Mémo (ex : redimensionnement d'éléments),
                // on l'affecte à l'objet eMemoEditor qui l'exécutera une fois que l'affichage sera complet

                var fctOnComplete;

                if (typeof (customOnShowFct) == 'function')
                    fctOnComplete = (function (id) { return function () { completeMemo(id); customOnShowFct(id); } })(strMemoId);
                else
                    fctOnComplete = (function (id) { return function () { completeMemo(id); } })(strMemoId);



                if (typeof (customOnShowFct) == 'function')
                    currentMemoEditor.customOnShow = fctOnComplete;

                // Taille par défaut du champ Mémo
                if (getNumber(oParentCell.style.width) != NaN && getNumber(oParentCell.style.width) > 0)
                    currentMemoEditor.config.width = oParentCell.style.width;
                else
                    currentMemoEditor.config.width = '99%';
                if (getNumber(oParentCell.style.height) != NaN && getNumber(oParentCell.style.height) > 0)
                    currentMemoEditor.config.height = oParentCell.style.height;
                else
                    currentMemoEditor.config.height = '100%';

                // En mode fiche hors Planning et E-mail, on affiche les champs Mémo sans bordures ni barres d'outils
                currentMemoEditor.borderlessMode = (getCurrentView(document) == "FILE_CREATION" || getCurrentView(document) == "FILE_MODIFICATION") && !nodeExist(document, "mailDiv") && !nodeExist(document, "formDiv") && !nodeExist(document, "admntCntnt");

                //Pour les sms on a besoin que la textarea, pas bordure ni de barres d'outils
                currentMemoEditor.borderlessMode = currentMemoEditor.borderlessMode || nodeExist(document, "tablesms");

                // En revanche, sur l'écran d'édition d'E-mail/E-mailing, on interdit l'affichage de l'éditeur avec une barre d'outils réduite
                if ((getCurrentView(document) == "FILE_CREATION" || getCurrentView(document) == "FILE_MODIFICATION") && nodeExist(document, "mailDiv")) {
                    currentMemoEditor.preventCompactMode = true;
                    currentMemoEditor.config.width = '100%';
                }

                // Grâce à CKEditor 4, on affiche même ces champs en mode inline editing, qui par essence, ne comporte pas de bordures
                var bInlineMode = false;
                try {
                    if (getAttributeValue(oParentCell, "inlinemode") == 1)
                        bInlineMode = true;
                }
                catch (ex) { }
                if (currentMemoEditor.borderlessMode || bInlineMode)
                    currentMemoEditor.inlineMode = true;

                // Mise à jour en base lors de la sortie du champ
                currentMemoEditor.updateOnBlur = isUpdateOnBlur();

                // Mode lecture seule ou écriture
                currentMemoEditor.readOnly = (oParentCell.getAttribute("ero") == "1");
                currentMemoEditor.uaoz = (oParentCell.getAttribute("uaoz") == "1");
                currentMemoEditor.fromParent = (oParentCell.getAttribute("fromparent") == "1");


                // Affichage du champ Mémo après paramétrage
                currentMemoEditor.show();
            }
            else {
                setEventListener(oDoc, 'click', function () { oParentCell.click(); });
            }
        }

    }
}

// Ajuste la taille du conteneur intérieur du menu droit (aka. "Gerard") en fonction du contenu, et notamment du nombre
// de thèmes affichés pour le menu Accueil
function addInnerRightMenuWidth(nTab, nDefaultRightMenuWidth) {
    oGerard = document.getElementById("Gerard");
    if (!oGerard)
        return;

    if (nTab == null || nTab == 0) {
        var nGerardWidth = nDefaultRightMenuWidth;
        // La taille minimale de Gerard doit être définie de façon à ce qu'il n'y ait qu'une seule ligne
        // de carrés de thèmes, pour éviter que le colorPicker ne prenne trop d'espace vertical.
        // On compte donc le nombre de thèmes affichés, et on le multiplie par la
        // largeur effective d'un carré + marge de 10 pixels (qui correspond au padding de 10 pixels présent
        // sur le colorPicker pour séparer les carrés entre eux)
        // Soit, pour 7 thèmes (défaut) : (43 + 10) * 7 = 371 pixels
        var oThemeThumbnails = document.querySelectorAll("li.themeThumbnail");
        var oColorPicker = document.getElementById("colorPick");
        var nColorPickerDefaultPadding = 10; // à mettre à jour en même temps que la classe d'eMain.css
        var nColorPickerPadding = nColorPickerDefaultPadding;
        try {
            if (oColorPicker) {
                // getComputedStyle renverra 2 fois le même padding, même s'il n'est défini que pour un seul
                // côté. Il faut donc le comptabiliser qu'une seule fois
                var nColorPickerPaddingLeft = getNumber(window.getComputedStyle(oColorPicker, null).getPropertyValue('padding-left'));
                var nColorPickerPaddingRight = getNumber(window.getComputedStyle(oColorPicker, null).getPropertyValue('padding-right'));
                if (!isNaN(nColorPickerPaddingLeft) && nColorPickerPaddingLeft > 0)
                    nColorPickerPadding = nColorPickerPaddingLeft;
                else if (!isNaN(nColorPickerPaddingRight) && nColorPickerPaddingRight > 0)
                    nColorPickerPadding = nColorPickerPaddingRight;
            }
        }
        catch (ex) {
            nColorPickerPadding = nColorPickerDefaultPadding;
        }
        if (isNaN(nColorPickerPadding) || nColorPickerPadding == 0)
            nColorPickerPadding = nColorPickerDefaultPadding;
        if (oThemeThumbnails && oThemeThumbnails.length > 0) {
            var nThemeThumbnailWidth = getNumber(oThemeThumbnails[0].clientWidth);
            if (isNaN(nThemeThumbnailWidth) || nThemeThumbnailWidth == 0)
                nThemeThumbnailWidth = 43; // à mettre à jour en même temps que la classe d'eMain.css
            nGerardWidth = (nThemeThumbnailWidth + nColorPickerPadding) * oThemeThumbnails.length;
        }

        // Largeur min-width de Gerard prend en compte le nombre de colonnes des liens favoris
        var favLikColumns = document.querySelectorAll("ul.rightMenuFavlnk");
        var innerWidth = favLikColumns && favLikColumns.length > 0 ? favLikColumns.length * 200 : nDefaultRightMenuWidth;
        //if (innerWidth > nGerardWidth)
        //  nGerardWidth = innerWidth;

        createCss("customCss", "gerardWidth", "min-width: " + (nGerardWidth) + "px !important;", true);
        addClass(oGerard, "gerardWidth");
    }

    return nGerardWidth;
}

/// Ajuste la taille du div container (contenant les modes fiche et liste)
/// à la taille restant à l'écran sous les menus
function adjustDivContainer(callbackFct) {
    try {
        //Si la fiche est une duplication, on laisse la fenête s'ajuster automatiquement
        if (getCurrentView(document) == "FILE_CREATION" && getAttributeValue(document.getElementById("fileDiv_" + nGlobalActiveTab), "fid0") > 0) {
            var divDetailsBkms = document.getElementById("divDetailsBkms");
            if (divDetailsBkms)
                divDetailsBkms.style.height = "auto";
        }


        var oDivcontainer = document.getElementById("container");
        var posContainer = getAbsolutePosition(oDivcontainer);


        // taille minimum, en pixels, requise pour afficher le menu de droite correctement (taille du calendrier)
        // A mettre à jour en même temps que la constante RIGHT_MENUCAL_WIDTH dans eConst.cs, et la variable dans eTools.createCalendarPopup()
        // Et ajuster le calcul ci-dessous en fonction
        var nMinimumCalendarWidth = 250;

        //JAS Largeur spécifique pour le menu droit en planning.
        var divCal = document.getElementById("Calendar_" + nGlobalActiveTab);
        var nDefaultRightMenuWidth = 250;

        var globalNav = document.getElementById("globalNav");

        var oeParam = top.getParamWindow();

        var fsize = "12";
        if (typeof oeParam.GetParam === "function")
            fsize = oeParam.GetParam('fontsize');
        if (fsize == "")
            fsize = "12";

        if (fsize == '8') {
            nDefaultRightMenuWidth = 250
        }
        else if (fsize == '10') {
            nDefaultRightMenuWidth = 250
        }
        else if (fsize == '12') {
            nDefaultRightMenuWidth = 250
        }
        else if (fsize == '14') {
            nDefaultRightMenuWidth = 350
        }
        else if (fsize == '16') {
            nDefaultRightMenuWidth = 350
        }
        else if (fsize == '18') {
            nDefaultRightMenuWidth = 350

        }
        else
            nDefaultRightMenuWidth = 250



        // if (divCal != null)
        //nDefaultRightMenuWidth += 20;

        // Calcul de la taille du menu de droite
        var nRightMenuWidth = 0;
        var oRightMenu = document.getElementById("rightMenu");
        if (oRightMenu) {

            //if (nGlobalActiveTab == null || nGlobalActiveTab == 0)
            //nDefaultRightMenuWidth = oRightMenu.offsetWidth;

            var nGerardWidth = addInnerRightMenuWidth(nGlobalActiveTab, nDefaultRightMenuWidth);
            //if (!isNaN(nGerardWidth) && nGerardWidth > 0)
            //nDefaultRightMenuWidth = nGerardWidth;

            strRightMenuWidth = nDefaultRightMenuWidth + "px";
            var winSize = getWindowSize();
            var nInnerWidth = winSize.w;
            var nInnerHeight = winSize.h;

            createCss("customCss", "rightMenuWidth", "width: " + strRightMenuWidth + " !important; right: -" + nDefaultRightMenuWidth + "px", true);
            addClass(oRightMenu, "rightMenuWidth");


            if (oRightMenu.className.indexOf("FavLnkOpen") != -1) {
                nRightMenuWidth = oRightMenu.offsetWidth; //100 / (nInnerWidth / nMinimumCalendarWidth);
                var nMenuBarWidth = 30;
                var oMenuBar = document.getElementById("menuBar");
                if (oMenuBar)
                    nMenuBarWidth = oMenuBar.offsetWidth;
                nRightMenuWidth += nMenuBarWidth;
            }
        }
        // 5 pixels de marge à droite (margin-right) appliqués en CSS sur rightMenu
        nRightMenuWidth += 5;

        var nMainDivMarginLeft = 5;
        var nMainDivMarginRight = isIris(nGlobalActiveTab) ? 3 : 1;  //+1 mais bizzare car il n'est définit nul part.
        var nMarginBorder = 2;
        var nMainDivWidth = nInnerWidth - nRightMenuWidth - nMainDivMarginLeft - nMainDivMarginRight - nMarginBorder; // application d'un écart de 1% entre le menu et le contenu principal (ex : 89%/10%)


        //ELAIZ - Modification du calcul de la largeur auto du conteneur pour le nouveau mode fiche car sinon on se retrouve avec une marge sur la droite inutile
        if (top.isIris(nGlobalActiveTab))
            nMainDivWidth = nInnerWidth - nRightMenuWidth;

        if (nGlobalActiveTab == null || nGlobalActiveTab == 0) {
            nMainDivWidth = nInnerWidth - nMainDivMarginLeft - nMainDivMarginRight - 2 * nMarginBorder;
        }

        createCss("customCss", "mainDivWidth", "width: " + nMainDivWidth + "px !important", true);
        /*}*/

        var winSize = getWindowSize();
        var nContainerHeight = winSize.h;

        if (nContainerHeight) {
            var oRuleLine = getCssSelector("eMain.css", ".contentMaster");
            if (oRuleLine) {
                oRuleLine.style.height = nContainerHeight + "px";
            }
            if (oDivcontainer) {
                oDivcontainer.style.height = nContainerHeight + "px";
            }
        }

        var mainDiv = document.getElementById('mainDiv');
        if (mainDiv) {
            mainDiv.style.height = (nContainerHeight - mainDiv.offsetTop - 2) + 'px';
        }

        var divTab = document.getElementById("div" + nGlobalActiveTab);
        if (divTab)
            // la dimension de la liste principale est changée
            oEvent.fire("list-bounds-changed", {
                width: nMainDivWidth,
                height: parseInt(divTab.style.height),
                offsetTop: divTab.offsetTop,
                bInitSizeList: false
            });

        if (typeof callbackFct === "function")
            callbackFct();
    }
    catch (e) {
        alert('adjustDivContainer');
        alert(e);
    }

}


//Ajuste la taille du div pour la scrollbar des signets
function adjustScrollFile(nTab) {

    if (!nTab)
        nTab = nGlobalActiveTab;

    // ajuste les scrollbars sur la partis signet 
    // ne doit pas être appelée dans les popup
    if (isPopup() || isBkmFile(nTab))
        return;

    //Ajuste la taille du div
    try {
        // Position du bloc détails
        var oFileDiv = document.getElementById("fileDiv_" + nTab);
        var pos = getAbsolutePosition(oFileDiv);

        // Recherche de la position du bas du div englobant
        var oMaindDiv = document.getElementById("mainDiv");
        var posMaindDiv = getAbsolutePosition(oMaindDiv);

        // Taille du bloc scrollable
        var nFileDivHeight = posMaindDiv.h - (pos.y - posMaindDiv.y);

        //Ajustement de la taille
        //  Modifie la css + l'attribut style.height

        var oRuleLine = getCssSelector("eFile.css", ".fileDiv");

        var divFilePart1 = document.getElementById("divFilePart1");

        //nFileDivHeight -= 65; // décale de 55px pour prendre en compte l'arrondi de bas de page

        oRuleLine.style.height = nFileDivHeight + "px";
        var divBkmCtner = document.getElementById("divBkmCtner");
        if (getAttributeValue(oFileDiv, "ftrdr") == "8" && getNumber(getAttributeValue(oFileDiv, "edntype")) > 0) {

            var nFileDivPart1Height = nFileDivHeight; //correspond au margin-top de maindiv dans eAdminFile.css
            var blockBkms = document.getElementById("blockBkms");
            if (blockBkms)
                nFileDivPart1Height -= blockBkms.offsetHeight;
            nFileDivPart1Height -= 30; // marge
            // var divFilePart1 = document.getElementById("divFilePart1");
            divFilePart1.style.height = nFileDivPart1Height + "px";
            divFilePart1.style.overflowY = "auto";
        }
        else {
            var oBkmBar = document.getElementById("bkmBar_" + nGlobalActiveTab);
            var oDivBkms = document.getElementById("divBkmPres");
            var nBkmHeight = nFileDivHeight;
            var bkmWeb = getCssSelector('eFile.css', '.bkmWeb');
            var bkmDiv = document.querySelector('.bkmdiv');
            var bkmTitle = document.querySelector('.bkmTitle');
            var bkmWebDiv = document.querySelector('.BkmWeb');
            var bkmWebFrm = document.querySelector('.BkmWebFrm');

            var paramWin = top.getParamWindow();
            var objThm = JSON.parse(paramWin.GetParam("currenttheme").toString());

            for (var i = 0; i < oFileDiv.children.length; i++) {
                var oElt = oFileDiv.children[i];
                if (oElt.id == "blockBkms")
                    continue;

                nBkmHeight -= oElt.clientHeight;
            }

            if (oBkmBar)
                nBkmHeight -= oBkmBar.clientHeight;

            if (objThm.Version > 1)
                nBkmHeight -= 65;
            else
                nBkmHeight -= 40;// il y a un margin-top de 15px sur blockBkms et on retire encore 5px pour l'arrondi de bas de page.

            /*ELAIZ - Régression 76477 - Si on est sur le nouveau thème on calcule la hauteur du conteneur de l'iframe par rapport à la taille de l'iframe ou de son 
              contenu afin de ne pas avoir d'ascenseur verticale notamment sur Timeline */

            //demande 81 384

            var margins = 140; // Ensemble des marges, padding mais aussi de la barre de nav des tabs

            if (objThm.Version > 1 && bkmDiv && !bkmDiv.classList.contains('BkmWeb')) {
                oDivBkms.classList.add('bkmBarAll');
            } else if (objThm.Version > 1 && bkmDiv && bkmDiv.classList.contains('BkmWeb')) {
                oDivBkms.classList.remove('bkmBarAll');
                oDivBkms.style.height = oFileDiv.offsetHeight - (divFilePart1.offsetHeight + margins) + 'px';
            } else if (oDivBkms) {
                oDivBkms.style.height = nBkmHeight + "px";
            }

        }
    }
    catch (e) {
        // une erreur sur le resize ne doit pas bloquer l'appli
        //  alert(e.Description);
    }

}

function adjustWebBkmHeight(bkmTitle, bkmWebFrm, bkmWeb, nBkmHeight, bkmWebDiv) {

    //if (bkmWebFrm.contentDocument.readyState === 'complete') {
    //    if (bkmWebFrm.contentDocument &&
    //        bkmWebFrm.contentDocument.children.length >= 1) {
    //        // On rajoute 39px qui correspond à la hauteur de la div bkmTitle au dessus de l'iframe
    //        bkmWeb.style.height = (bkmWebFrm.contentDocument.children[0].offsetHeight
    //            + bkmTitle.offsetHeight) + 'px';
    //    } else {
    //        bkmWeb.style.height = nBkmHeight + "px";
    //    }
    //    state = true;
    //}

    window.setTimeout(function () {
        if (bkmWebFrm.contentDocument &&
            bkmWebFrm.contentDocument.children.length > 0) {
            // On rajoute 39px qui correspond à la hauteur de la div bkmTitle au dessus de l'iframe
            bkmWeb.style.height = (bkmWebFrm.contentDocument.children[0].offsetHeight
                + bkmTitle.offsetHeight) + 'px';
        } else {
            bkmWeb.style.height = nBkmHeight + "px";
        }
    }, 500)


    //On vire la height mise en inline à 100%
    bkmWebDiv.style.removeProperty("height");

    if (bkmWebDiv.style.length < 1)
        bkmWebDiv.removeAttribute("style");
}

/************************************************************************************/
/*                          LISTE                                                   */
/*  Doit être dans main.js. Les fonctions peuvent être appellé sans que list.js     */
/*  soit chargé (appel d'un mode liste depuis mode fiche par exemple                */
/*  Recharge complète list + filtre + menu + entete                                 */
/*    loadlist ---> updatecontent                                                   */
/*  Recharge uniquement list                                                        */
/*    loadlist ---> updatelist                                                      */
/************************************************************************************/
// Charge le div de liste  ainsi que les jss et css
function loadList(nPage, bReload, addToTab) {
    if (typeof (bReload) == 'undefined')
        bReload = true;

    if (typeof (nPage) == 'undefined')
        nPage = 1;


    if (typeof (addToTab) == 'undefined')
        addToTab = false;

    // Utile pour le retour de l'Engine
    modeList = true;

    // Vide les nodes enfants et vide le contenu du div
    var oMainDiv = document.getElementById("mainDiv");
    var sType = oMainDiv.getAttribute("edntype");
    //Type de rapport en cours d'affichage sur la liste
    var bInModal = document["_ismodal"] == 1;

    var rType = oMainDiv.getAttribute("type");

    //Pour le type filtre pas de reload global
    if (sType != null && (sType == "invit"
        || sType == "filter"
        || sType == "report"
        || sType == "lnkfile"
        || sType == "mailing"
        || sType == "mailtemplate"
        || sType == "formular"
        || sType == "automation"
        || sType == "xrmhomepage"
        || sType == 'importtemplate'))
        bReload = false;



    //KHA le 23/06/2014 on vérifie s'il faut rajouter les filtres/rapports publics
    var bAddPubItem = false;
    if (sType == "filter" || sType == "report" || sType == 'importtemplate')
        bAddPubItem = getAttributeValue(document.getElementById("addpub"), "chk") == "1";


    var tabMainDivWH = GetMainDivWH();
    var height = tabMainDivWH[1];
    var divWidth = tabMainDivWH[0];

    // Nombre de ligne en mode liste automatique
    var nRows = 0;

    //Recuperation param
    var oeParam = getParamWindow();
    //reload le conteneur principal
    if (nGlobalActiveTab != 0) {
        // On crée un object pour indiquer au setWait où on souhaite aller
        // US #4261 - TK #6962 - Skeleton nécessaire pour le passage Admin > Utilisation mode Liste
        // Mais non désiré si on affiche la même liste qu'actuellement
        var oParamGoTabList = null;
        var bReloadsSameList = (document.getElementById("mt_" + nGlobalActiveTab) && getAttributeValue(document.getElementById("mt_" + nGlobalActiveTab), "cpage") == nPage);
        if (!bReloadsSameList) {
            oParamGoTabList = {
                to: 1,
                nTab: nGlobalActiveTab,
                context: "eMain.loadList"
            }
        }
        if (sType == 'mailing' || sType == 'formular') {
            setWait(true);
        } else {
            setWait(true, undefined, undefined, isIris(top.getTabFrom()), oParamGoTabList);
        }
        //Num page
        if (nPage < 1)
            nPage = 1;

        if (!bInModal) {

            // Nombre de ligne en mode liste automatique - si page != page accueil

            if (typeof (oeParam.GetParam) != "undefined" && oeParam.GetParam('Rows') != '') {

                nRows = oeParam.GetParam('Rows');
            }

            oeParam.SetParam('Page_' + nGlobalActiveTab, nPage);
        } else {

            var oContent = document.getElementById("listContent");
            if (oContent)
                var nRows = GetNumRows(oContent.offsetHeight);
            else
                var nRows = 30;

            nRows -= 2; //Une ligne au dessus pour le titre des colonnes/1 lignes dessous/arrondis limite

        }

        //Recharge le div de contenu intégralement
        if (bReload) {


            //Charge le contenu
            try {

                // Initialise CSS 
                var callBack = function () {

                    addScript("ckeditor/ckeditor", "LIST");
                    addScript("grapesjs/grapes.min");
                    addScript("grapesjs/grapesjs-plugin-ckeditor.min");
                    addScript("grapesjs/grapesjs-blocks-basic.min");
                    addScript("grapesjs/grapesjs-preset-newsletter.min");
                    addScript("grapesjs/grapesjs-preset-webpage.min");

                    var oListUpdater = new eUpdater("mgr/eListManager.ashx", 1);
                    oListUpdater.addParam("page", nPage, "post");
                    oListUpdater.addParam("tab", nGlobalActiveTab, "post");
                    oListUpdater.addParam("divH", height, "post");
                    oListUpdater.addParam("full", "1", "post");
                    if (divWidth != 0)
                        oListUpdater.addParam("divW", divWidth, "post");

                    oListUpdater.addParam("type", sType, "post");
                    oListUpdater.addParam("rows", nRows, "post");
                    oListUpdater.addParam("reloadpaging", "1", "post");
                    oListUpdater.ErrorCallBack = errorList;

                    var dt = new Date();

                    //  oListUpdater.send(updateContent, nGlobalActiveTab);


                    var myFunct = (function (nTab) {
                        return function (oRes) {
                            var res = JSON.parse(oRes);
                            updateContent(res, nTab)
                        }
                    })(nGlobalActiveTab);

                    oListUpdater.send(myFunct);
                }

                nsMain.InitList(callBack);


            }
            catch (e) {

                alert('eMain.loadList');
                alert(e.description);
            }
        }
        else {
            try {
                var oListUpdater = new eUpdater("mgr/eListManager.ashx", 1);

                oListUpdater.addParam("page", nPage, "post");

                //Ne renvoie que les lignes de tableaux
                oListUpdater.addParam("onlyrow", addToTab ? "1" : "0", "post");

                switch (sType) {
                    case "filter":
                        oListUpdater.addParam("tab", nGlobalActiveTab, "post");

                        var oFilterMainDiv = document.getElementById("mainDiv");

                        // deselectAllowed : permet de valider un choix vide
                        var isDeselectAllowed = getAttributeValue(oFilterMainDiv, "deselectAllowed") == "1";
                        oListUpdater.addParam("deselectAllowed", isDeselectAllowed ? "1" : "0", "post");

                        // adminMode : mode admin (on viens de l'admin
                        var isAdminMode = getAttributeValue(oFilterMainDiv, "adminMode") == "1";
                        oListUpdater.addParam("adminMode", isAdminMode ? "1" : "0", "post");

                        // selectFilterMode : mode selection d'un filtre sans l'appliquer
                        var isSelectFilterMode = getAttributeValue(oFilterMainDiv, "selectFilterMode") == "1";
                        oListUpdater.addParam("selectFilterMode", isSelectFilterMode ? "1" : "0", "post");

                        break;
                    case "invit":
                        var oWizardbody = document.getElementById("wizardbody");
                        var nBkm = getAttributeValue(oWizardbody, "bkm");
                        oListUpdater.addParam("tab", getNumber(nBkm), "post");

                        //SHA
                        if (oInvitWizard)
                            oListUpdater.addParam("tabfrom", getNumber(oInvitWizard.TabFrom), "post");

                        break;
                    case "report":
                        var nBkm = getAttributeValue(document.getElementById("mainDiv"), "tabBKM");
                        if (nBkm == "-1")
                            nBkm = getAttributeValue(document.getElementById("mainDiv"), "tab");

                        oListUpdater.addParam("tab", getNumber(nBkm), "post");

                        /// ABBA tache #75 614 affichage de <Liste en cours> en mode fiche quand je clique sur le filtre express
                        if (eTools.GetModal("ReportList") == null) {
                            var oFileDiv = top.document.getElementById("fileDiv_" + top.nGlobalActiveTab);

                            var fid = getAttributeValue(oFileDiv, "fid");
                        }
                        else {
                            var fid = eTools.GetModal("ReportList").getParam("fid")
                        }
                        oListUpdater.addParam("fid", fid, "post");
                        break;

                    case "formular":
                        var nBkm = getAttributeValue(document.getElementById("mainDiv"), "tabBKM");
                        if (nBkm == "-1")
                            nBkm = getAttributeValue(document.getElementById("mainDiv"), "tab");

                        oListUpdater.addParam("tab", getNumber(nBkm), "post");

                        //AAB tâche 1882
                        var formularType = eTools.GetModal("oModalFormularList").getParam("formularType");
                        if (formularType != null || typeof (formularType != "undefined"))
                            oListUpdater.addParam("formularType", formularType, "post");
                        break;
                    case "mailtemplate":
                    case "importtemplate":
                        var nBkm = getAttributeValue(document.getElementById("mainDiv"), "tabBKM");
                        if (nBkm == "-1")
                            nBkm = getAttributeValue(document.getElementById("mainDiv"), "tab");
                        oListUpdater.addParam("tab", getNumber(nBkm), "post");
                        break;
                    case "automation":
                        // la table cible
                        var target = getAttributeValue(oMainDiv, "tab");
                        if (!target) target = 0;
                        if (target == 0) return;

                        // le champ déclencheur de notif 
                        var fld = getAttributeValue(oMainDiv, "field");
                        if (!fld) fld = 0;

                        oListUpdater.addParam("tab", getNumber(target), "post");
                        oListUpdater.addParam("field", getNumber(fld), "post");

                        break;
                    case "xrmhomepage":
                        break;
                    default:
                        oListUpdater.addParam("tab", nGlobalActiveTab, "post");
                        break;
                }
                oListUpdater.addParam("divH", height, "post");
                oListUpdater.addParam("type", sType, "post");
                oListUpdater.addParam("rtype", rType, "post"); // ? pas utiliser

                if (sType == "filter" || sType == "report" || sType == 'importtemplate')
                    oListUpdater.addParam("addpub", bAddPubItem ? "1" : "0", "post");


                if (divWidth != 0)
                    oListUpdater.addParam("divW", divWidth, "post");

                oListUpdater.addParam("rows", nRows, "post");


                if (sType == "lnkfile") {

                    var nTargetTab = oMainDiv.getAttribute("tab");
                    var nTargetFrom = nGlobalActiveTab;

                    var nDescId = oMainDiv.getAttribute("did");

                    var nFileId = oMainDiv.getAttribute("fid");

                    //Table sur laquelle on recherche
                    oListUpdater.addParam("targetTab", nTargetTab, "post");
                    //id de la fiche de départ
                    oListUpdater.addParam("FileId", nFileId, "post");
                    //Champ catalogue sur la fiche de départ
                    oListUpdater.addParam("targetfield", nDescId, "post");
                    //Table de départ
                    oListUpdater.addParam("tabfrom", nTargetFrom, "post");
                }

                // Dans le cas des modèles de mails pour l'emailing, la table de filtrage est stockée dans un attribut spécifique sur le corps de l'Assistant.
                // Ce qui n'est pas le cas pour les modèles de mails unitaires, dont la liste est gérée par eFilterReportList
                if (sType == "mailing") {

                    var nTargetTab = oMainDiv.getAttribute("filterTab");
                    //Table sur laquelle on recherche
                    oListUpdater.addParam("filterTab", nTargetTab, "post");

                }
                oListUpdater.ErrorCallBack = errorList;
                oListUpdater.send(

                    function (oRes) {

                        updateList(oRes, nGlobalActiveTab, addToTab);

                    }
                );


            } catch (e) {

                alert('eMain.loadList');
                alert(e.description);

            }
        }

        // #57 043 - Dans le cas où on recharge la liste des utilisateurs en admin, on remet à jour la liste des UserIDs internes utilisés pour naviguer de fiche en fiche
        // (pagination du menu droit)
        if (top.nGlobalActiveTab == 101000 && typeof (nsAdminUsers) != "undefined") {
            nsAdminUsers.ListUserId = {
                Page: -1,
                ListId: []
            }

            //ALISTER Demande #80 701
            nsAdminUsers.loadUsersId(nPage);
        }
    }
    else {
        // Initialise les scripts pour les tablettes tactiles
        if (isTablet() && areTabletCustomScrollersEnabled()) {
            //TODORES
            oMainDiv.innerHTML = '<div id="scroller"><b><br /><br /> ACCUEIL </b></div>';
            loadTabletScrollers();
        }
        else {
            oMainDiv.innerHTML = " <b><br /><br /> ACCUEIL </b>";
        }
    }

    /** On sous-estime trop souvent la haute qualité qu'on peut donner en planquant
      * la poussière sous le tapis. On ne peut pas supprimer des css sans donner
      * un effet étrange à l'affichage de la page? Pas de problème, la suppression
      * se fait 1/3 de seconde après l'affichage... */
    setTimeout(function () {
        clearHeader("LISTIRIS", "ALL");
    }, 300);

}

nsMain.InitList = function (callback) {


    if (typeof (callback) != "function")
        callback = function () { };

    clearHeader("LIST");
    addCss("eActions", "LIST");

    //scritp JS
    addScript("eButtons", "LIST");
    addScript("ePopup", "LIST",
        function () {
            addScript("eFieldEditor", "LIST");
            addScript("eGrapesJSEditor", "LIST");
            addScript("eMemoEditor", "LIST");
            addScript("eMarkedFile", "LIST", callback)
        });
}

nsMain.SetGlobalActiveTab = function (nTab) {

    if (top.nGlobalActiveTab == nTab)
        return;

    nsMain.OldTab = top.nGlobalActiveTab;
    top.nGlobalActiveTab = nTab

}




///nTab : descid de la table à charger
/// nType : type de liste à charger
//      0 (default) : mainList
//      1 : bookmark
//
function listLoaded(nTab, nType) {

    if (typeof (nType) == "undefined")
        nType = 0;

    /* Initialise les click sur champ */
    try {
        initFldClick(nTab);

    }
    catch (e) {
        alert('err : initFldClick (' + nTab + ')');
    }

    /* Initialise les click sur entête */

    try {
        initHeadEvents();
    }
    catch (e) {
        alert('err : initHeadEvents ( )');
    }



    /*  CSS */
    try {
        loadCustomCss(nTab);
    }
    catch (e) {
        alert('err : loadCustomCss (' + nTab + ')');
        alert(e.description);
    }


    if (nType == 0) {
        try {


            //setPaging(nTab);     // déplacé dans le count   
            /* Paging  */
        }
        catch (e) {
            //alert('err : setPaging (' + nTab + ')');
        }


        try {
            setFilterTip(nTab);     /* Toolip de filtre  */
        }
        catch (e) {
            alert('err : setFilterTip (' + nTab + ')');
        }

        try {
            quickUserRefreshCombo();    /* Filtre rapide */

        }
        catch (e) {
            alert('err : setFilterTip (' + nTab + ')');
        }


        try {

            // SI MEMO DE LA TAILLE DE LA POLICE
            // if (typeof (nBaseFontSize) != "undefined" && nBaseFontSize > 0) {
            //    resizeFont(nBaseFontSize);
            // }

            // Champ de recherche
            var oMainSearchValue = document.getElementById("searchmainFld_" + nGlobalActiveTab);
            if (oMainSearchValue) {
                if (oMainSearchValue.value != "") {
                    var oMainSearchInput = document.getElementById("eFSInput");
                    if (oMainSearchInput) {
                        oMainSearchInput.value = oMainSearchValue.value;
                        setReset();
                    }
                }
            }

            adjustLastCol(nTab);

            var oMlFldInpt = document.getElementById("MLFld_" + nTab);
            if (oMlFldInpt && oMlFldInpt.value.length > 0) {
                var aMlFldIds = oMlFldInpt.value.split(";");
                for (var i = 0; i < aMlFldIds.length; i++) {
                    var oMlTh = document.getElementById(aMlFldIds[i]);
                    adjustBtnFieldWidth(oMlTh);
                }
            }
        }
        catch (e) {
            if (typeof (e.description) != 'undefined')
                alert('listloaded => ' + e.description);
            else
                alert('listloaded => ' + e.message);
        }
    }

    var browser = new getBrowser();
    if (browser.isGecko)
        createCss("customCss", "SumCols", "float: none !important;top: 0px !important", true);

    if (nType == 1)
        bkmMajCnt(nTab);


    if (oWebMgr) {
        var prevWebTabLoaded = oWebMgr.loadPrevWebTab(nTab, nType);
        //#53143 : On remet à zéro la navigation vers le mode fiche si un sous-onglet a été chargé
        if (prevWebTabLoaded)
            oNavManager.ResetMode('List', nTab);
    }



    // On créait un object pour renvoyer au setWait ou es ce qu'on veux aller
    var toPage = 0;
    if (getCurrentView(document) == "FILE_MODIFICATION") {
        toPage = 2
    } else if (getCurrentView(document) == "LIST") {
        toPage = 1
    } else if (getCurrentView(document) == "CALENDAR" || getCurrentView(document) == "CALENDAR_LIST") {
        toPage = 1
    }

    var oParamGoTabList = {
        to: toPage,
        nTab: nTab,
        context: "eMain.listLoaded"
    }
    setWait(false, undefined, undefined, isIris(top.getTabFrom()), oParamGoTabList);
}


function bkmMajCnt(nBkm) {
    //
    var nb = 0;
    var oBkmTab = document.querySelector("table[ednmode='bkm'][enbcnt][id='mt_" + nBkm + "']");
    var oDiv = document.querySelector("div[id='bkm_" + nBkm + "'][nofile='1']");


    if (oBkmTab != null)
        nb = getAttributeValue(oBkmTab, "enbcnt");

    if (oDiv || oBkmTab) {

        var oBkmLabel = document.getElementById("bkmCnt_" + nBkm);
        if (oBkmLabel) {
            oBkmLabel.innerHTML = nb + " ";
        }

        // Nom du filtre
        var oBkmFilter = document.getElementById("bkmCntFilter_" + nBkm);
        if (oBkmFilter) {
            var filterCountLabel = "";
            var filterCount = document.getElementById("bkmTabCnt_" + nBkm);
            if (filterCount) {
                filterCountLabel = " " + top._res_8257 + " " + getAttributeValue(filterCount.parentElement, "fltLbl");
            }
            oBkmFilter.innerText = filterCountLabel;
        }


        // Mise à jour du compteur de PJ
        if (nBkm % 100 == ATTACHMENT) {
            var oBkmTabLabel = document.getElementById("bkmTabCnt_pj");
            if (oBkmTabLabel) {
                oBkmTabLabel.innerHTML = "(" + nb + ")";
            }
        }

    }
}

/// Liste des codes (int/hashCode) des modules d'options utilisateur
/// /!\ CETTE ENUM EST RÉPLIQUÉE CÔTÉ SERVEUR DANS eUserOptionsModules.cs, ET CÔTÉ JS EN DEBUT DE FICHIER. ELLE DOIT ÊTRE DONC MISE À JOUR EN MÊME TEMPS, DANS LE MEME ORDRE (INDEX) QUE L'ENUM CS /!\
function getUserOptionsModuleHashCode(module) {
    switch (module) {
        case USROPT_MODULE_UNDEFINED: return 0;        case USROPT_MODULE_MAIN: return 1;        case USROPT_MODULE_PREFERENCES: return 2;        case USROPT_MODULE_PREFERENCES_LANGUAGE: return 3;        case USROPT_MODULE_PREFERENCES_SIGNATURE: return 4;        case USROPT_MODULE_PREFERENCES_PASSWORD: return 5;        case USROPT_MODULE_PREFERENCES_MEMO: return 6;        case USROPT_MODULE_ADVANCED: return 7;        case USROPT_MODULE_ADVANCED_EXPORT: return 8;        case USROPT_MODULE_ADMIN: return 9;        case USROPT_MODULE_ADMIN_GENERAL: return 10;        case USROPT_MODULE_ADMIN_ACCESS: return 11;        case USROPT_MODULE_ADMIN_ACCESS_USERGROUPS: return 12;        case USROPT_MODULE_ADMIN_ACCESS_SECURITY: return 13;        case USROPT_MODULE_ADMIN_ACCESS_PREF: return 14;        case USROPT_MODULE_ADMIN_TABS: return 15;        case USROPT_MODULE_ADMIN_HOME: return 16;        case USROPT_MODULE_ADMIN_HOME_V7_HOMEPAGES: return 17;        case USROPT_MODULE_ADMIN_HOME_XRM_HOMEPAGES: return 18;        case USROPT_MODULE_ADMIN_HOME_EXPRESS_MESSAGE: return 19;        case USROPT_MODULE_ADMIN_EXTENSIONS: return 20;        case USROPT_MODULE_ADMIN_DASHBOARD: return 21;        case USROPT_MODULE_ADMIN_GENERAL_LOGO: return 22;        case USROPT_MODULE_ADMIN_GENERAL_NAVIGATION: return 23;        case USROPT_MODULE_ADMIN_GENERAL_LOCALIZATION: return 24;        case USROPT_MODULE_ADMIN_GENERAL_SUPERVISION: return 25;        case USROPT_MODULE_ADMIN_GENERAL_CONFIGADV: return 26;        case USROPT_MODULE_ADVANCED_PLANNING: return 27;        case USROPT_MODULE_ADMIN_EXTENSIONS_MOBILE: return 28;        case USROPT_MODULE_ADMIN_EXTENSIONS_SYNCHRO: return 29;        case USROPT_MODULE_ADMIN_EXTENSIONS_SMS: return 30;        case USROPT_MODULE_ADMIN_EXTENSIONS_CTI: return 31;        case USROPT_MODULE_ADMIN_EXTENSIONS_API: return 32;        case USROPT_MODULE_ADMIN_EXTENSIONS_EXTERNALMAILING: return 33;        case USROPT_MODULE_ADMIN_EXTENSIONS_VCARD: return 34;        case USROPT_MODULE_ADMIN_EXTENSIONS_SNAPSHOT: return 35;        case USROPT_MODULE_ADMIN_EXTENSIONS_EMAILING: return 36;        case USROPT_MODULE_ADMIN_TAB: return 37;        case USROPT_MODULE_ADMIN_TAB_USER: return 38;        case USROPT_MODULE_HOME: return 39;        case USROPT_MODULE_PREFERENCES_FONTSIZE: return 40;        case USROPT_MODULE_PREFERENCES_MRUMODE: return 41;
        case USROPT_MODULE_ADMIN_EXTENSIONS_FROMSTORE: return 42;        case USROPT_MODULE_ADMIN_TAB_GRID: return 43;        case USROPT_MODULE_ADMIN_EXTENSIONS_GRID: return 44;        case USROPT_MODULE_ADMIN_HOME_XRM_HOMEPAGE: return 45;        case USROPT_MODULE_ADMIN_HOME_XRM_HOMEPAGE_GRID: return 46;        case USROPT_MODULE_ADMIN_EXTENSIONS_NOTIFICATIONS: return 47;        case USROPT_MODULE_ADMIN_EXTENSIONS_CARTO: return 48;        case USROPT_MODULE_PREFERENCES_THEME: return 49;        case USROPT_MODULE_ADMIN_DASHBOARD_OFFERMANAGER: return 50;        case USROPT_MODULE_ADMIN_DASHBOARD_RGPD: return 51;        case USROPT_MODULE_ADMIN_EXTENSIONS_SYNCHRO_OFFICE365: return 52;        case USROPT_MODULE_ADMIN_EXTENSIONS_SIRENE: return 53;        case USROPT_MODULE_ADMIN_DASHBOARD_RGPDTREATMENTLOG: return 54;        case USROPT_MODULE_ADMIN_EXTENSIONS_POWERBI: return 55;        case USROPT_MODULE_ADMIN_EXTENSIONS_OUTLOOKADDIN: return 56;        case USROPT_MODULE_ADMIN_EXTENSIONS_ACCOUNTING_BUSINESSSOFT: return 57;        case USROPT_MODULE_ADMIN_EXTENSIONS_ACCOUNTING_CEGID: return 58;        case USROPT_MODULE_ADMIN_EXTENSIONS_ACCOUNTING_SAGE: return 59;        case USROPT_MODULE_ADMIN_EXTENSIONS_ACCOUNTING_EBP: return 60;        case USROPT_MODULE_ADMIN_EXTENSIONS_ACCOUNTING_SIGMA: return 61;        case USROPT_MODULE_ADMIN_EXTENSIONS_IN_UBIFLOW: return 62;        case USROPT_MODULE_ADMIN_EXTENSIONS_IN_HBS: return 63;        case USROPT_MODULE_ADMIN_EXTENSIONS_DOCUSIGN: return 64;        case USROPT_MODULE_ADMIN_EXTENSIONS_SMS_NETMESSAGE: return 65;        case USROPT_MODULE_ADMIN_EXTENSIONS_SYNCHRO_EXCHANGE2016ONPREMISE: return 66;        case USROPT_MODULE_ADMIN_EXTENSIONS_ZAPIER: return 67;        case USROPT_MODULE_ADMIN_DASHBOARD_VOLUME: return 68;        case USROPT_MODULE_ADMIN_EXTENSIONS_EXTRANET: return 69;        //SHA : tâche #1 873
        case USROPT_MODULE_ADMIN_EXTENSIONS_ADVANCED_FORM: return 70; case USROPT_MODULE_ADMIN_EXTENSIONS_DEDICATED_IP: return 71; case USROPT_MODULE_ADMIN_ORM: return 72; case USROPT_MODULE_ADMIN_EXTENSIONS_MAIL_STATUS_VERIFICATION: return 73; case USROPT_MODULE_ADMIN_EXTENSIONS_LINKEDIN: return 74; case USROPT_MODULE_ADMIN_EXTENSIONS_WORLDLINE_PAYMENT: return 75; case USROPT_MODULE_PREFERENCES_PROFILE: return 76;    }
}

/* Mise à jour du conteneur principal avec spécificités éventuelles pour l'admin */
function updateContentAdmin(oContent, mainTab, pageId) {

    updateContent(oContent, mainTab);

    // En fonction de la page Admin affichée, déclenchement de scripts
    switch (pageId) {
        case USROPT_MODULE_ADVANCED_EXPORT:
            resizeUserOptExportContent();
            break;

        case USROPT_MODULE_PREFERENCES_SIGNATURE:
            InitMemoUsrOpt();
            break;

        case USROPT_MODULE_PREFERENCES_MEMO:
            InitMemoUsrOpt(
                function (eMemo) {
                    var subTitle = document.getElementById("memoSubTitle");
                    subTitle.innerHTML = subTitle.innerHTML + " (" + eMemo.toolbarButtonKeyStrokes["xrmusermessage"] + ")";
                });
            break;
        case USROPT_MODULE_PREFERENCES_FONTSIZE:
            fontSizeInit();
            break;

        case USROPT_MODULE_PREFERENCES_THEME:

            themeInit();
            break;

        default:
            break;
    }
}


///Initialise le memo puis appelle la fonction AfterInit
function InitMemoUsrOpt(AfterInit, nHeight) {
    var onShowUserSignMemo = function (Callback) {

        // On redimensionne dynamiquement le champ Mémo une fois que la taille de la fenêtre est connue
        var oMainDiv = document.getElementById('mainDiv');
        var oBodySignContainer = document.getElementById('BodySignMemoId');
        var oMemoEditor = nsMain.getMemoEditor('edtBodySignMemoId');
        if (oMemoEditor && oBodySignContainer && oMainDiv) {
            var oMainDivHeight = getNumber(oMainDiv.style.height);
            if (oMainDivHeight > 0) {

                var sNewHeight = oMainDivHeight - 350; //295; = 835-485
                oBodySignContainer.style.height = sNewHeight + 'px';
                oMemoEditor.resize('100%', sNewHeight);
            }

            if (typeof (Callback) == "function")
                Callback(oMemoEditor);
        }
        else if (typeof nHeight == "number" && oMemoEditor && oBodySignContainer) {
            oBodySignContainer.style.height = nHeight + 'px';
            oMemoEditor.resize('100%', nHeight + "");

        }

    }

    initMemoFields(0, 2, '', function () { onShowUserSignMemo(AfterInit); });
}



/* Mise à jour du conteneur principal  */
function updateContent(oContent, mainTab, bForceReplace) {

    var oMainDiv = document.getElementById("mainDiv");
    var sListCol = "";

    if (typeof (bForceReplace) === "undefined")
        bForceReplace = false;

    sListCol = updateComputedColList();


    // Initialise les scripts pour les tablettes tactiles
    if (isTablet() && areTabletCustomScrollersEnabled()) {
        oMainDiv.innerHTML = '<div id="scroller">' + oContent + "</div>";
        loadTabletScrollers();
    }
    else {
        if (typeof (oContent) == "object" && oContent.hasOwnProperty("Success")) {

            var parts = oContent.MultiPartContent;


            if (top.nGlobalActiveTab == 101000 && oMainDiv.querySelector("div#listGroup")) {
                oContent.Full = false;
            }



            if (oContent.Full) {
                //si complet, on remplace tout le contenu
                while (oMainDiv.hasChildNodes()) {
                    oMainDiv.removeChild(oMainDiv.lastChild);
                }
            }

            if (oContent.Html.length > 0) {
                oMainDiv.innerHTML = oContent.Html;

            } else {

                for (var key in parts) {
                    if (parts.hasOwnProperty(key)) {

                        var myPart = parts[key];
                        var oDivparts;

                        if (oContent.Full) {
                            oDivparts = document.createElement('div');
                            oDivparts.innerHTML = myPart.Content;
                            oMainDiv.appendChild(oDivparts.firstChild);
                        }
                        else {
                            /*
                            Rafraîchissement de la liste des utilisateurs : si on effectue l'opération via nsAdmin.loadContentUsers(), on rafraîchit toute la page
                            (clic sur le lien "Groupes et utilisateurs", ajout/modification/suppression de groupe... => bForceReplace est passé à true dans InitList())
                            Autrement, on ignore le rafraîchissement de la liste des groupes (cas du clic sur un groupe pour filter : permet de conserver la sélection)
                            */
                            if (myPart.ID == "listGroup" && !bForceReplace)
                                continue;

                            oDivparts = document.getElementById(myPart.ID);
                            oDivparts.outerHTML = myPart.Content;
                        }

                        if (myPart.CallBack) {
                            addScriptText(myPart.CallBack, top.document);
                        }
                    }
                }
            }


            //oMainDiv.appendChild(oDivGroup.firstChild);
            if (oContent.CallBack) {
                /* -> ici, on place le script dans le scope globale                    */
                addScriptText(oContent.CallBack, top.document);
            }

        }
        else
            oMainDiv.innerHTML = oContent;
    }



    //Nettoie le header
    clearHeader("FILE");
    clearHeader("HOMEPAGE");
    clearHeader("EDITFILE", "CSS");

    // Chargement initial
    try {
        initEditors();
        initMarkedFiles();
    }
    catch (e) {
        if (typeof (e.description) != 'undefined')
            alert('updateContent.initEditors => ' + e.description);
        else
            alert('updateContent.initEditors => ' + e.message);
    }

    // Chargement du tableau de liste
    listLoaded(mainTab);
    calendarLoaded(mainTab);
    updateCount(mainTab);
    if (sListCol != "")
        updtComputedCol(mainTab, sListCol, mainTab, 0);
    SetListIdParameter(mainTab);
    InitSizeList(oMainDiv, mainTab);

    ScrollIntoLastPosition(mainTab);

    //CNA - Demande #58259
    //Dans IE, cache l'appel à la vcard/minifiche après un filtre express
    shvc(null, 0);
}

//Initialise les dimensions de la liste pour que la barre de scrool soit correct en hauteur et largeur
function InitSizeList(oMainDiv, mainTab) {
    var tabMainDivWH = GetMainDivWH();
    var height = tabMainDivWH[1];
    var divWidth = tabMainDivWH[0];
    //Init conteneur liste
    if (height > 0)
        oMainDiv.style.height = height + "px";  //On force le dimensionnement de la div principale
    var oPlanning = document.getElementById("cal_mt_" + mainTab);
    //Init taille liste Pour limité à la zone de liste
    var oContentList = document.getElementById("div" + mainTab);
    if (oContentList) {
        var nLeftPos = 0;
        if (oPlanning)
            nLeftPos = getAbsolutePosition(oContentList).x;
        if (divWidth > 0)
            oContentList.style.width = (divWidth - nLeftPos) + "px";
        if (height > 0) {
            nHeightListContent = height;
            var oMainListContent = document.getElementById("mainListContent");
            if (oMainListContent) {
                var nHeightMainListContent = oMainListContent.offsetHeight;
                if (nHeightMainListContent > 0)
                    nHeightListContent = nHeightListContent - nHeightMainListContent;
            }
            var oActions = document.getElementById("actions");
            if (oActions) {
                var nHeightActions = oActions.offsetHeight;
                if (nHeightActions > 0)
                    nHeightListContent = nHeightListContent - nHeightActions;
            }
            var oactions2 = document.getElementById("actions2");
            if (oactions2) {
                var nHeightActions2 = oactions2.offsetHeight;

                if (nHeightActions2 > 0)
                    nHeightListContent = nHeightListContent - nHeightActions2;
            }
            var oFltindex = document.getElementById("fltindex");
            if (oFltindex) {
                var nFltindex = oFltindex.offsetHeight;
                if (nFltindex > 0)
                    nHeightListContent = nHeightListContent - nFltindex;
            }
            oContentList.style.height = nHeightListContent + "px";
            oContentList.style.overflow = "auto";

            // La dimension de la liste principale est changée
            oEvent.fire("list-bounds-changed", {
                width: divWidth - nLeftPos,
                height: nHeightListContent,
                offsetTop: oContentList.offsetTop,
                bInitSizeList: true
            });
        }
    }

}


/*  Mise à jour du conteneur du tableau de la liste */
function updateList(oList, mainTab, addToTab) {

    if (typeof (addToTab) == 'undefined')
        addToTab = false;

    var bInModal = document["_ismodal"] == 1;
    var oMainDiv = document.getElementById("mainDiv");
    var sType = oMainDiv.getAttribute("edntype");

    if (sType == "invit") {
        var oContent = document.getElementById("content");
    }
    else {
        var oContent = document.getElementById("listContent");
    }

    var sListCol = "";
    if (sType == "main" || sType == null) {
        sListCol = updateComputedColList();
    }

    if (addToTab) {
        var newElement = document.createElement("div");

        newElement.innerHTML = oList;
        var newTab = newElement.querySelector("#mt_" + mainTab);
        var cPage = getAttributeValue(newTab, "cpage");
        var eof = getAttributeValue(newTab, "eof");
        var myT = newElement.querySelector("#mt_" + mainTab + " > tbody").querySelectorAll("tr");
        var myCurrentTab = document.querySelector("#mt_" + mainTab + " > tbody");

        setAttributeValue(document.querySelector("#mt_" + mainTab), "cpage", cPage);
        setAttributeValue(document.querySelector("#mt_" + mainTab), "eof", eof);

        Array.prototype.slice.call(myT).forEach(function (elem) {
            myCurrentTab.appendChild(elem);
        });

    }
    else
        oContent.innerHTML = oList;

    // Chargement du tableau de liste
    if (sType == "main" || sType == null) {
        listLoaded(mainTab);
        updateCount(mainTab);

        if (sListCol != "")
            updtComputedCol(mainTab, sListCol, mainTab, 0);

        SetListIdParameter(mainTab);

        setWait(false);
    }
    else if (sType == "filter" || sType == "report" || sType == "invit" || sType == "mailing" || sType == "mailtemplate" || sType == "formular" || sType == "automation" || sType == "importtemplate") {

        initHeadEvents();       /* Initialise les click sur entête */

        if (sType == "filter" || sType == "invit") {
            initFilterList();
            if (sType == "filter" && bInModal) {
                updateCount(104000);
            }
        }
        else if (sType == "report") {
            initReportList();
        }
        else if (sType == "mailing") {
            initMailTpl();
        }
        else if (sType == "mailtemplate") {
            initMailTpl(true);
            updateCount(107000);
        } else if (sType == "automation") {
            if (typeof (nsAdminAutomation) != "undefined")
                nsAdminAutomation.updateRecordNotifCount();
        } else if (sType == "importtemplate") {
            initImportTpl();
            updateCount(TAB_IMPORTTEMPLATE);
        } else if (sType == "formular") {
            updateCount(113000);
        }
        // remove the skeleton
        setWait(false, undefined, undefined, isIris(top.getTabFrom()), {
            to: 1,
            nTab: nGlobalActiveTab,
        });
        setWait(false);
    }

    var bIsTablet = false;
    try {
        if (typeof (isTablet) == 'function')
            bIsTablet = isTablet();
        else if (typeof (top.isTablet) == 'function')
            bIsTablet = top.isTablet();
    }
    catch (e) {

    }

    if (!bIsTablet) {
        var oSearchInput = document.getElementById('eFSInput')

        if (oSearchInput && oSearchInput.value && oSearchInput.value.length > 0) {
            oSearchInput.focus();
        }
    }

    // Redimensionnement de la liste, sauf dans certains contextes
    // Notamment sur la fenêtre de filtres/rapports/modèles de mails unitaires/formulaires/etc.
    if (mainTab != 104000 && sType != "filter" &&
        mainTab != 113000 && sType != "formular" &&
        mainTab != 107000 && sType != "mailtemplate" &&
        mainTab != 114200 && sType != "automation" &&
        mainTab != TAB_IMPORTTEMPLATE && sType != "importtemplate"
    )
        InitSizeList(oMainDiv, mainTab);

}


//met à jour le compteur de liste et le pagging
function updateCount(mainTab) {

    var bInModal = document["_ismodal"] == 1;

    //
    var oTab = document.getElementById("mt_" + mainTab);

    if (oTab == null)
        return;

    //Pas de count sur les pj
    if (mainTab == TAB_PJ) {
        setPaging(mainTab);
        document.getElementById("SpanNbElem").innerHTML = "";
        return;
    }
    else if ((mainTab == 104000 || mainTab == 107000 || mainTab == 113000 || mainTab == 114200 || mainTab == TAB_IMPORTTEMPLATE) && bInModal) {
        setPaging(mainTab);
        return;
    }
    var nNb = (oTab.getAttribute("eNbCnt"));
    var nNbAll = (oTab.getAttribute("eNbTotal"));
    var nbPage = (oTab.getAttribute("nbpage"))


    if (oTab.getAttribute("eHasCount") == "1") {
        //compteur réalisé


        updatePagging(nNb + ";" + nbPage + ";" + nNbAll, mainTab);

    }
    else if ((oTab.getAttribute("eHasCount") == "0" && oTab.getAttribute("cnton") == "1")) {
        //Lancement async du countage
        cod(mainTab);

    }
    else {

        //lien pour lancement à la demande
        setPaging(mainTab);

        //[MOU 28/08/2013 cf.21709] Ajout d un "title" pour expliquer la fonctionnalité.
        var span = document.getElementById("SpanNbElem");
        if (span)
            span.innerHTML = " <a href='javascript:cod( " + mainTab + " );' title='" + top._res_5030 + "'>?</a> ";
    }
}

// Charge les CSS 
function loadCustomCss(mainTab) {

    var oDivHidden = document.getElementById("hv_" + mainTab);
    if (oDivHidden == undefined || oDivHidden == null)
        return;

    var oLstInpt = oDivHidden.getElementsByTagName("input");

    for (var j = 0; j < oLstInpt.length; j++) {

        var oElem = oLstInpt[j];

        if (oElem.getAttribute("etype") == "css") {

            var sCssName = oElem.getAttribute("ecssname");
            var sCssVal = oElem.getAttribute("ecssclass");

            createCss("customCss", sCssName, sCssVal);
        }
    }
}

//Choix des onglets
function setTabOrder(selId) {


    modalListTab = new eModalDialog(top._res_366, 0, "eTabsSelectDiv.aspx", 850, 550);
    modalListTab.addParam("modalname", "modalListTab", "post");
    if (selId != null)
        modalListTab.addParam("selid", selId, "post");
    modalListTab.bBtnAdvanced = true;
    modalListTab.ErrorCallBack = launchInContext(modalListTab, modalListTab.hide);
    modalListTab.show();

    modalListTab.addButton(top._res_29, onSetTabsAbort, "button-gray", null);
    modalListTab.addButton(top._res_28, onSetTabsOk, "button-green", null);

}


function onSetTabsOk(nTab, popupId) {

    var _frm = document.getElementById("frm_" + popupId);
    var _oDoc = _frm.contentWindow.document || _frm.contentDocument;

    var _itemsUsed = _oDoc.getElementById("TabSelectedList").getElementsByTagName("div");
    var strListCol = "";
    for (var i = 0; i < _itemsUsed.length; i++) {
        if (strListCol != "")
            strListCol = strListCol + ";";
        strListCol = strListCol + _itemsUsed[i].getAttribute("DescId");
    }

    strListCol = "0;" + strListCol;

    //Récupération du strListCol
    var selId = _oDoc.getElementById("DivSelectionListResultList").getAttribute("SelectedSel");
    //var updatePref = "taborder=" + strListCol;

    var updatePref = "taborderid=" + selId + ";$;taborder=" + strListCol;

    // US #1330 - Tâches #2748, #2750 - On reparamètre les variables utilisées par eParamIFrame.eParamOnLoad() pour recharger le TabID, FileID, type d'affichage actuels après rafraîchissement total via loadTabs()
    // Depuis le correctif de l'US #1330, ces variables reprennent le contexte courant (nGlobalActiveTab, getCurrentView()...) UNIQUEMENT si elles sont undefined, et non plus systématiquement comme avant (bug).
    // Donc, appeler la fonction ci-dessous sans paramètres va les remettre à undefined, ce qui forcera la fonction loadTabs() à les reparamétrer avec le contexte courant (nGlobalActiveTab, getCurrentView()...)
    nsMain.setParamIFrameReloadContext();

    //Callback d'erreur : si la sauvegarde des tabs a échouée, on tente de reloader l'existant quand meme
    updateUserTabs(updatePref, loadTabs, loadTabs);
    modalListTab.hide();

}


function onSetTabsAbort(v1, popupId) {
    modalListTab.hide();
}


function changeView(id) {
    var updatePref = "taborderid=" + id;
    //Callback d'erreur : si la sauvegarde des tabs a échouée, on tente de reloader l'existant quand meme
    updateUserTabs(updatePref, loadTabs, loadTabs);
}


function loadTabs() {

    loadNavBar();

    // TODO - NE PAS RECHARGER LA FRAME PARAM
    // Si FraParam est reload, il faut mettre bIsParamLoaded & bIsIFrameLoaded à 0 pour lancer le LoadParam
    top.bIsParamLoaded = 0;
    top.bIsIFrameLoaded = 0;
    /*GCH : commenté suite à demande RMA/CMO 31012013*/
    /*MOU : décommenté suite à demande RMA/CMO  cf. 23835 */
    setWait(true);

    // #23 614, #39 338, #59 724 - Mémorisation de l'onglet, du fichier et du mode d'affichage utilisés avant rechargement d'eParamIFrame, qui provoque un rechargement de la page
    // Code corrigé et repassé en revue pour la US #1330 et ses tâches liées (#2748, #2750)
    // Ces variables peuvent être mises à jour via un appel à nsMain.setParamIFrameReloadContext(), soit sans paramètres (auquel cas tout est mis à undefined, et donc réajusté ci-dessous), soit avec paramètres
    var currentView = getCurrentView(document);
    if (typeof (top.tabToLoadAfterParamIFrame) == "undefined" && typeof (top.nGlobalActiveTab) != "undefined")
        top.tabToLoadAfterParamIFrame = top.nGlobalActiveTab;
    if (typeof (top.viewToLoadAfterParamIFrame) == "undefined" && typeof (currentView) != "undefined")
        top.viewToLoadAfterParamIFrame = currentView;
    if (typeof (top.fileToLoadAfterParamIFrame) == "undefined" && document.getElementById("fileDiv_" + top.nGlobalActiveTab))
        top.fileToLoadAfterParamIFrame = getAttributeValue(document.getElementById("fileDiv_" + top.nGlobalActiveTab), "fid");
    // Ces variables sont mises à zéro dans tous les cas, car on ne recharge pas le contexte éventuellement utilisé à l'ouverture de session si on est venu d'eGotoFile.aspx
    //if (typeof (top.isTplMailToLoadAfterParamIFrame) == "undefined")
    top.isTplMailToLoadAfterParamIFrame = "0";
    //if (typeof (top.loadFileInPopupAfterParamIFrame) == "undefined")
    top.loadFileInPopupAfterParamIFrame = "0";
    top.document.getElementById('eParam').contentWindow.location.reload(true);
}


/* Ouverture de la liste des rapports de l'utlisateur */
function openUserReport() {


    var winSize = getWindowSize().scaleHW({ sh: 0.8, sw: 0.75 }).min({ w: 700, h: 500 });
    var oUserReportList = new eModalDialog(top._res_6055, 0, "eReportUserList.aspx", winSize.w, winSize.h, "_modalUserReportList");
    oUserReportList.ErrorCallBack = function () { setWait(false); }
    oUserReportList.noButtons = true;
    oUserReportList.show();
}

// Fonction permettant d'appeler la fenêtre d'ajout d'image (photo, avatar, vCard, champ Image, e-mailing...)
function doGetImage(oImg, strType) {


    var nFldDescId = 0;
    var bFileAvatar = false;
    var sName = getAttributeValue(oImg, "ename");
    var sFrom = oImg.hasOwnProperty('editorType') ? oImg.editorType : "";
    if (sFrom == "") {
        if (oImg.name == "sharingImage")
            sFrom = "sharringimage";
    }

    if (sName != "" && sName.length >= 4) {
        //Fini par 75
        bFileAvatar = new RegExp("_[1-9]+[0-9]*75$").test(sName);

        if (bFileAvatar) {
            if (getAttributeValue(oImg, "tab") == "101000")
                strType = 'USER_AVATAR_FIELD';
            else
                strType = 'AVATAR_FIELD';
        }

    }
    else if (oImg.id == "vcCadre") {
        strType = 'OLD_AVATAR_FIELD'; /* Avatar de PP/PM avant nouvelle mise en page */
    }


    switch (strType) {
        case 'IMAGE_FIELD':
        case 'MEMO':
        case 'MEMO_SETDIALOGURL':
        case 'URL':
        case 'TXT_URL':
        case 'LOGO':
        case 'AVATAR':
        case 'AVATAR_FIELD':
        case 'USER_AVATAR_FIELD':
        case 'OLD_AVATAR_FIELD':
            doGetImageGeneric(oImg, strType, sFrom);
            break;

        //doGetImageAvatar(oImg);
        //break;
    }
}

// Fonction permettant d'appeler la fenêtre d'ajout d'image (photo, avatar, champ Image, e-mailing...)
function doGetImageGeneric(oImg, strType, sFrom) {

    // CRU : Empêcher la modification de l'image si le champ est en lecture seule
    if (getAttributeValue(oImg, "readonly") == "readonly")
        return;

    var bDisplayDeleteBtn = true;
    var deleteImageFct = deleteImage;
    var objHeaderCell = null;
    var sName = "";
    top.setWait(true);
    try {
        /*********************** Titre de la modale ***********************/
        var strModalDialogTitle = top._res_6286; // Insérer une image depuis votre ordinateur
        if (strType == 'AVATAR')
            strModalDialogTitle = top._res_6180; // Télécharger votre avatar
        else if (strType == 'IMAGE_FIELD' || (strType == "AVATAR_FIELD")) {
            sName = oImg.getAttribute("ename") + "";
            objHeaderCell = document.getElementById(sName);
            strModalDialogTitle = getAttributeValue(objHeaderCell, "lib");
        }

        /*********************** Taille de la modale ***********************/
        // #32 312 - Taille de la fenêtre = 160 + marge pour afficher l'image "Introuvable" renvoyée par le navigateur si l'image est impossible à charger
        var initialWindowWidth = 460; // valeur à modifier également dans eImageDialog.aspx
        var initialWindowHeight = 180; // valeur à modifier également dans eImageDialog.aspx
        var windowWidth = initialWindowWidth;
        var windowHeight = initialWindowHeight;
        var browser = new getBrowser();
        if (browser.isIE)
            windowHeight += 28;
        else
            windowHeight += 20;

        top.modalImage = new eModalDialog(strModalDialogTitle, 0, "eImageDialog.aspx", windowWidth, windowHeight);
        // On mémorise la taille que l'on souhaitait initialement affecter à la fenêtre dans deux variables JS, que l'on rattachera en propriétés de
        // l'objet eModalDialog afin que la page chargée à l'intérieur puisse déclencher son redimensionnement si elle masque le conteneur d'image
        // dans le cas où il n'y a aucune image à charger
        top.modalImage.initialWindowWidth = initialWindowWidth;
        top.modalImage.initialWindowHeight = initialWindowHeight;
        top.modalImage.addParam("ImageType", strType, "post");
        top.modalImage.addParam("CalledFrom", sFrom, "post");


        /*********************** RECUPERATION DE CERTAINS PARAMETRES EN FONCTION DU TYPE D'IMAGE A GERER ***********************/

        var descId, fileId;

        // Contexte du champ Mémo à mettre à jour : pour l'insertion dans des champs Mémo uniquement

        if (strType == 'MEMO' || strType == 'MEMO_SETDIALOGURL') {

            top.modalImage.parentMemoEditor = oImg;
            descId = getAttributeValue(objHeaderCell, "did");
            // Récupération du fileId en mode Fiche
            fileId = imgGetCurrentFileId(oImg);

        }
        else if (strType == 'TXT_URL') {

            top.modalImage.parentMemoEditor = oImg;
            descId = getAttributeValue(objHeaderCell, "did");
            // Récupération du fileId en mode Fiche
            fileId = imgGetCurrentFileId(oImg);

            // Seule la suppression des images de diffusion d'un formulaire est gérée pour ce type d'image
            // L'action consiste simplement à vider le champ sur la fenêtre parente, et c'est à l'annulation ou à l'enregistrement de celle-ci que la suppression sera effectuée
            if (oImg.id == "sharingImage") {
                bDisplayDeleteBtn = true;
                deleteImageFct = function () {
                    if (oFormular) {
                        oFormular.onSharingImageChange(document.getElementById("sharingImage").value);
                    }
                    document.getElementById("sharingImage").value = '';
                };
            }
            else
                bDisplayDeleteBtn = false;

        }
        else if (strType == "AVATAR" || (strType == "OLD_AVATAR_FIELD") || strType == "USER_AVATAR_FIELD") {

            if (strType == "AVATAR" || strType == "USER_AVATAR_FIELD")
                descId = "101075";
            else
                descId = getAttributeValue(oImg, "did");

            fileId = getAttributeValue(oImg, "fid");

            top.modalImage.addParam("DescId", descId, "post");
            top.modalImage.addParam("FileId", fileId, "post");



        }
        // Contexte du champ à mettre à jour : pour les champs Image uniquement
        else if (strType == 'IMAGE_FIELD' || (strType == "AVATAR_FIELD")) {

            descId = getAttributeValue(objHeaderCell, "did");
            // Récupération du fileId en mode Fiche
            fileId = imgGetCurrentFileId(oImg);
            top.modalImage.addParam("DescId", descId, "post");
            top.modalImage.addParam("FileId", fileId, "post");

            // Taille de l'image à gérer : pour l'avatar et les champs Image
            // Récupération du contexte (mode Fiche/mode Liste/mode Signet)
            var strContext = '';
            if (!document.getElementById("fileDiv_" + nGlobalActiveTab))
                strContext = "list";
            else {
                var nTab = GetMainTableDescId(getAttributeValue(oImg, 'ename'));
                var oCurTab = document.getElementById("mt_" + nTab);
                if (getAttributeValue(oCurTab, "ednmode") == "bkm")
                    strContext = "bkm";
                else
                    strContext = "file";
            }
            // Récupération de la taille à partir des propriétés de l'image si existantes
            var width = NaN;
            var height = NaN;
            if (oImg.style) {
                width = getNumber(oImg.style.width);
                height = getNumber(oImg.style.height);
            }
            if ((isNaN(width) && strContext == "file")) {
                width = 0;
            }
            else if ((isNaN(width) || width == "") && oImg.firstChild && oImg.firstChild.tagName.toLowerCase() == "img") {
                width = oImg.firstChild.style.width || oImg.firstChild.width;
            }

            if ((isNaN(height) || height == "") && oImg.firstChild && oImg.firstChild.tagName.toLowerCase() == "img") {
                height = oImg.firstChild.style.height || oImg.firstChild.width;
            }
            // Si la taille n'a pas pu être récupérée, pour le mode Liste ou Signet, on utilise la taille Vignette
            if ((isNaN(width) || isNaN(height) || width == "" || height == "") && (strContext != "file")) {
                if ((isNaN(width) || width == ''))
                    width = 16;
                if ((isNaN(height) || height == ''))
                    height = 16;
            }
            top.modalImage.addParam("ImageWidth", width, "post");
            top.modalImage.addParam("ImageHeight", height, "post");
        }

        if (oImg.querySelector) {
            var myImpg = oImg.querySelector("img");
            if (getAttributeValue(myImpg, "isb64") == "1") {
                top.modalImage.addParam("isb64", "1", "post");
                top.modalImage.addParam("b64val", getAttributeValue(myImpg, "src"), "post");
            }
        }


        // AUTRES PARAMETRES

        top.modalImage.addParam("modalVarName", "modalImage", "post");
        top.modalImage.addParam("parentIsPopup", isPopup() ? "1" : "0", "post");
        top.modalImage.addParam("updateOnBlur", isUpdateOnBlur() ? "1" : "0", "post");
        top.modalImage.sourceObj = oImg;

    }
    finally {
        top.setWait(false);
    }
    top.setWait(true);
    top.modalImage.ErrorCallBack = function () {
        top.setWait(false);
    }
    top.modalImage.onIframeLoadComplete = function () {
        top.setWait(false);
    };
    top.modalImage.show();
    /*********************** BOUTONS ***********************/
    top.modalImage.addButton(top._res_29, onImageCancel, "button-gray", this.jsVarName, "btnCancel"); // Annuler

    if (bDisplayDeleteBtn)
        top.modalImage.addButton(top._res_19, deleteImageFct, "button-red", this.jsVarName, "btnDelete"); // Supprimer 

    top.modalImage.addButton(top._res_28, function () { sendImageDialogForm(oImg); }, "button-green", this.jsVarName, "btnSend"); // Valider

}

// Fonction permettant d'appeler la fenêtre d'ajout d'image (vCard...)
function doGetImageAvatar(oImg) {
    top.setWait(true);

    try {
        var nTab = getAttributeValue(oImg, "tab");
        // Récupération du fileId en mode Fiche
        var fileId = getAttributeValue(oImg, "fid");
        var keywordvcName = "";

        if (document.getElementById("vcName")) {
            //Conteneur mode vcard

            var eltVcName = document.getElementById("vcName");
            if (eltVcName.innerText) {
                keywordvcName = eltVcName.innerText;
            }
            else {
                keywordvcName = eltVcName.textContent;
            }
        }
        else if (document.getElementById("cplName")) {
            // dans le cas du mode édition et de la vcard fantome,
            // le nom complet se trouve dans une input cachée
            //  OBSOLETE
            var eltVcName = document.getElementById("cplName");
            keywordvcName = eltVcName.value;
        }
        else {
            //Cas de l'affichage depuis le mode Fiche*
            var sFileName = "fileName_" + nTab + "_" + (nTab + 1);
            if (document.getElementById(sFileName)) {
                // dans le cas du mode éedition et de la vcard fantome,
                // le nom complet se trouve dans une input cachée
                var eltVcName = document.getElementById(sFileName);
                keywordvcName = eltVcName.value;
            }
            else {
                //Mode liste

                var sId = getAttributeValue(oImg, "id");
                var oObj = document.getElementById(sId.replace("_" + (nTab + 75) + "_", "_" + (nTab + 1) + "_"));
                if (oObj) {
                    keywordvcName = oObj.innerText;
                }
            }
        }

        var bHadAvatar = oImg && (oImg.style.backgroundImage != "");

        var oTabWH = getWindowWH(top);
        var maxWidth = 500; //Taille max à l'écran (largeur)
        //var maxHeight = (bHadAvatar) ? 700 : 600; //Taille max à l'écran (hauteur)
        var maxHeight = (bHadAvatar) ? 400 : 350;
        var width = oTabWH[0];
        var height = oTabWH[1];
        if (width > maxWidth)   //si largeur de l'écran plus grande que largeur max alors il faut utiliser la largeur max
            width = maxWidth;
        else
            width = width - 10;   //marge de "sécurité"
        if (height > maxHeight)   //si hauteur de l'écran plus grande que largeur max alors il faut utiliser la largeur max
            height = maxHeight;
        else
            height = height - 10;   //marge de "sécurité"

        top.modalImage = new eModalDialog(top._res_6180, 0, "eGoogleImageGet.aspx", width, height);
        top.modalImage.addParam("keyword", encodeURI(keywordvcName), "query");
        top.modalImage.addParam("resultCount", 10, "query");
        top.modalImage.addParam("fileId", fileId, "query");
        top.modalImage.addParam("nTab", nTab, "query");
        top.modalImage.addParam("modalVarName", "modalImage", "query");

        //Callback d'erreur
        top.modalImage.ErrorCallBack = function () { top.modalImage.hide(); top.setWait(false); };
        //launchInContext(top.modalImage, top.modalImage.hide);
    }
    finally {
        top.setWait(false);
    }
    top.setWait(true);
    top.modalImage.onIframeLoadComplete = function () {
        top.setWait(false);
    };
    top.modalImage.show();
    top.modalImage.addButton(top._res_29, onImageCancel, "button-gray", this.jsVarName); // Annuler
    top.modalImage.addButton(top._res_6347, deleteImage, "button-red", this.jsVarName); // Supprimer la photo
    top.modalImage.addButton(top._res_28, function () { sendImageDialogForm(oImg); }, "button-green", this.jsVarName); // Valider
}

// #31 762 : déplacement de la fonction GetCurrentFileId de eFile.js vers eMain.js
// Cette fonction, au même titre que imgGetCurrentFileId, peut être appelée depuis d'autres contextes qu'un mode Fiche.
// Auquel cas elle renvoie donc vide et non plus une erreur JS de fonction indéfinie
function GetCurrentFileId(nTab) {
    // Current fiche


    var oeParam = getParamWindow();
    var fileId = oeParam.GetParam('FileId_' + nTab);

    // 39383 : MCR, Affichage signet invalide (message "Paramètres Invalides") lors de la consultation du signet depuis une Fiche dont l'Onglet n'est pas Affiché 
    // recuperer le fileId autrement : a partir de la div fileDiv, recuperation du fid
    // exemple hmtl genere : <div id="fileDiv" class="fileDiv" fid="21" did="13100" edntype="0" tabfrom="0" ftrdr="3">
    // HLA - Mis en commentaire car regression numéro #40185
    if (!fileId) {
        var oFileDiv = document.getElementById("fileDiv_" + nTab);
        if (oFileDiv != null) {
            fileId = getAttributeValue(oFileDiv, "fid");
        }
    }

    return fileId;

}

// Récupération du fileId en mode Fiche ou Liste (fonctionne avec le mode fiche popup) pour tous les types d'images
function imgGetCurrentFileId(oImg) {
    // Récupération du fileId en mode Liste, Signet, Fiche (champs Image)

    var fileId = GetFieldFileId(getAttributeValue(oImg, "id"));
    if (fileId != "" && !isNaN(fileId)) {
        fileId = getNumber(fileId);
        if (fileId > 0)
            return fileId;
    }

    // Récupération du fileId en mode Fiche (photo système sur PP, PM)
    var objFile = document.getElementById("fileDiv_" + nGlobalActiveTab);
    if (objFile != null && typeof (objFile) != 'undefined') {
        fileId = getAttributeValue(objFile, "fid");
    }
    else {
        objFile = document.getElementById("fileId");
        if (typeof (objFile) != 'undefined')
            fileId = getAttributeValue(objFile, "fid");
    }
    fileId = getNumber(fileId);
    // champ notes ouvert en grand
    if (isNaN(fileId))
        fileId = getNumber(oImg.fileId);

    if (isNaN(fileId))
        fileId = 0;
    return fileId;
}

function sendImageDialogForm(oImg) {

    var oFrm = top.modalImage.getIframe();
    // Suppression de l'image si le champ URL est vidé
    if (
        oFrm.window.document.getElementById('imageURL') &&
        oFrm.window.document.getElementById('imageURL').style.display != 'none' &&
        oFrm.window.document.getElementById('imageURL').value == '' &&
        oFrm.window.document.getElementById('btnDelete') &&
        oFrm.window.document.getElementById('btnDelete').style.display != 'none'
    )
        oFrm.window.deleteImage(true);
    // Sinon, envoi du formulaire
    else {

        if (
            //(oFrm.window.document.getElementById('FromGoogle') && oFrm.window.document.getElementById('FromGoogle').checked
            //&& oFrm.window.document.getElementById('tbImageGoogle') && oFrm.window.document.getElementById('tbImageGoogle').value != ""
            //)
            //||
            (oFrm.window.document.getElementById('filMyFile') && oFrm.window.document.getElementById('filMyFile').value != "")
            ||
            (oFrm.window.document.getElementById('imageURL') && oFrm.window.document.getElementById('imageURL').value != "")
            ||
            (oFrm.window.document.getElementById('imgPreview') && (oFrm.window.document.getElementById('imgPreview').src.indexOf("fid=-1") != -1 || getAttributeValue(oFrm.window.document.getElementById('imgPreview'), "session") == "1"))
        ) {
            top.setWait(true);

            /// 57 513 : On supprime l'éventuelle image de diffusion précédemment ajoutée (cas d'un formulaire en création ID 0 non encore validé, ou modification de l'image d'un formulaire existant)
            if (oImg && oImg.id == "sharingImage" && oFormular) {
                oFormular.DeleteSharingImage(null, false, false);
            }

            oFrm.window.document.getElementById("cmdSend").click();
        }
        else {
            top.onImageCancel();
        }
    }
}

function deleteImage(bFromImageDialog, type, descId, fileId) {
    if (bFromImageDialog) {

        var oFrmButton = null;
        if (top.modalImage && typeof (top.modalImage.getIframe) == "function") {
            var oFrm = top.modalImage.getIframe();
            if (oFrm && oFrm.window && oFrm.window.document) {
                oFrmButton = oFrm.window.document.getElementById("cmdDelete");
            }
        }

        if (oFrmButton) {
            //eConfirm(1, top._res_6347, top._res_6348, '', 500, 200, // eConfirm précédemment utilisé sur eGoogleImageGet avec des libellés différents - A VALIDER
            eConfirm(1, top._res_29, top._res_1225, '', 450, 200,
                function () {
                    top.setWait(true);
                    oFrmButton.click();
                },
                function () {
                    return false;
                });
        }
    }
    else if (typeof (fileId) != "undefined" && typeof (descId) != "undefined") {

        var url = "mgr/eImageManager.ashx";
        var oUpdater = new eUpdater(url, null);

        oUpdater.addParam("action", "DELETE", "post");
        oUpdater.addParam("fileId", fileId, "post");
        oUpdater.addParam("fieldDescId", descId, "post");
        oUpdater.addParam("imageType", type, "post");
        oUpdater.addParam("computeRealThumbnail", "0", "post");
        oUpdater.addParam("imageWidth", "16", "post");
        oUpdater.addParam("imageHeight", "16", "post");

        oUpdater.ErrorCallBack = function (oRes) { ErrorImageDeleteReturn(oRes); };
        oUpdater.asyncFlag = true;
        oUpdater.send(function (oRes) { ImageDeleteReturn(oRes); });
    }
}

function ErrorImageDeleteReturn(oRes) {
    top.setWait(false);
}

function ImageDeleteReturn(oRes, engineObject, afterUpload) {
    top.setWait(false);
}

/*Rafraichir l'image de l'avatar puis ferme la pop up de sélection d'un avatar*/
function onImageSubmit(bAddPicture, bStoredInSession, strType, strURL, strURLDbValue, nImgWidth, nImgHeight, imgAlt, bDeletePicture, lineHeight) {
    top.setWait(false);

    if (strType == 'MEMO' && bAddPicture) {
        if (top.window['modalImage'] && top.window['modalImage'].parentMemoEditor) {
            var parentMemoEditor = top.window['modalImage'].parentMemoEditor;
            // #42333 CRU : Ajout de l'attribut "alt" à l'élément img
            var sHtmlImgAlt = "";
            if (imgAlt) {
                sHtmlImgAlt = "alt='" + imgAlt.replace(/'/g, "\'") + "'";
            }
            var sImgHTML = '<img src="' + strURL + '" ' + sHtmlImgAlt + '>'; // TODO
            parentMemoEditor.focus();

            // le second paramètre booléen indique s'il faut insérer le code directement (IE) ou en créant un élément dans le DOM
            // avant insertion (ce qui fonctionne mieux avec les autres navigateurs)
            var brow = new getBrowser();
            if (brow.isIE) {
                parentMemoEditor.insertData(sImgHTML, false);
            }
            else {
                parentMemoEditor.insertData(sImgHTML, true);
            }
        }
    }

    if (strType == 'MEMO_SETDIALOGURL' && bAddPicture) {
        if (top.window['modalImage'] && top.window['modalImage'].parentMemoEditor) {
            var parentMemoEditor = top.window['modalImage'].parentMemoEditor;
            if (parentMemoEditor.imageDialog) {
                parentMemoEditor.imageDialog.getContentElement("info", "txtUrl").setValue(strURL);
                parentMemoEditor.imageDialog.getContentElement("info", "txtAlt").setValue(imgAlt);
                //parentMemoEditor.imageDialog.setState(CKEDITOR.DIALOG_STATE_IDLE); // à utiliser pour bloquer la boîte de dialogue tant que l'URL n'est pas récupérée, si souhaité
            }
        }
    }

    if (strType == 'TXT_URL' && bAddPicture) {
        if (top.window['modalImage'] && top.window['modalImage'].parentMemoEditor) {
            var parentMemoEditor = top.window['modalImage'].parentMemoEditor;
            // #42333 CRU : Ajout de l'attribut "alt" à l'élément img            
            parentMemoEditor.focus();
            parentMemoEditor.value = strURL;
        }
    }

    if (strType == 'URL' && bAddPicture) {
        if (top.window['modalImage'] && top.window['modalImage'].parentMemoEditor) {
            var parentMemoEditor = top.window['modalImage'].parentMemoEditor;
            // #42333 CRU : Ajout de l'attribut "alt" à l'élément img                
            parentMemoEditor.focus();
            parentMemoEditor.value = strURL;
        }
    }

    if ((strType == 'IMAGE_FIELD' || strType == 'USER_AVATAR_FIELD' || strType == 'AVATAR_FIELD') && bAddPicture) {
        if (bDeletePicture == null)
            bDeletePicture = false;

        if (top.window['modalImage'] && top.window['modalImage'].sourceObj) {

            var oSourceObj = top.window['modalImage'].sourceObj;

            // Mise à jour du champ Image source
            if (oSourceObj.tagName.toLowerCase() == 'img') {
                // US #1 904 - Tâche #2 712 - Contexte hors avatar IRISBlack
                if (top.window['modalImage'] && !(top.window['modalImage'].irisBlackCtx && (strType == 'USER_AVATAR_FIELD' || strType == 'AVATAR_FIELD'))) {
                    oTargetObj = oSourceObj;
                    oSourceObj = findUp(oTargetObj, "TD");
                }
                // US #1 904 - Tâche #2 712 - Contexte avatar IRISBlack
                else {
                    oTargetObj = oSourceObj;
                }
            }
            // OU Mise à jour des champs Image en mode fiche contenus dans des TD
            else if (oSourceObj.firstChild && (oSourceObj.firstChild.tagName.toLowerCase() == 'img' || oSourceObj.firstChild.tagName.toLowerCase() == 'div'))
                oTargetObj = oSourceObj.firstChild;

            if (oTargetObj) {

                // Suppression
                if (bDeletePicture) {
                    if (lineHeight == null)
                        lineHeight = 24;

                    // US #1 271 - Fonction partagée - Sur le nouveau mode Fiche, traitement spécifique
                    if (oTargetObj.hasAttribute("irisblackimg")) {
                        //oTargetObj.src = "IRISBlack/Imgs/default.jpg";
                        oTargetObj.src = document.location.origin + '/' + document.location.pathname.split('/')[1] + '/' + "IRISBlack/Front/Assets/Imgs/default.jpg";
                    }
                    else {
                        oTargetObj.parentElement.removeChild(oTargetObj);

                        /// Rendu identique fait dans eRenderer->GetEmptyImagePanel, à modifier en cas de modification ici
                        oTargetObj = document.createElement("div");
                        oTargetObj.className = "icon-picture-o emptyPictureArea";
                        oTargetObj.setAttribute("data-eEmptyPictureArea", "1");

                        var nbRow = 1;
                        if (oSourceObj.hasAttribute("rowspan"))
                            nbRow = parseInt(oSourceObj.getAttribute("rowspan"));
                        var nImgHeight = (lineHeight * nbRow) - 12; //12 = (border 2px + padding 2px + margin 2px) x2
                        oTargetObj.style.fontSize = nImgHeight + "px";

                        oSourceObj.appendChild(oTargetObj);
                        oSourceObj.style.textAlign = "center";
                    }
                }
                else {
                    if (oTargetObj.tagName.toLowerCase() == 'div') {
                        oSourceObj.removeChild(oTargetObj);

                        oTargetObj = document.createElement("img");
                        oTargetObj.style.maxHeight = "100%";
                        oTargetObj.style.maxWidth = "100%";

                        oSourceObj.appendChild(oTargetObj);
                        oSourceObj.style.textAlign = "center";
                    }

                    // Ajout du suffixe pour le rafraîchissement de l'image

                    var bAddTS = false;
                    if (strURL.indexOf("eImage.aspx") >= 0 || strURL.toLowerCase().indexOf("datas") >= 0) {
                        bAddTS = true;
                    }

                    if (bAddTS)
                        strURL = strURL + '?t=' + new Date().getTime();

                    oTargetObj.src = strURL;

                    // US #1 904 - Tâche #2 712 - IRIS uniquement - Mise à jour du nom de fichier de l'avatar affiché en zone Résumé
                    if (top.window['modalImage'] && top.window['modalImage'].irisBlackCtx && (strType == 'USER_AVATAR_FIELD' || strType == 'AVATAR_FIELD')) {
                        top.window['modalImage'].irisBlackCtx.dataInput.Value = strURL.substring(strURL.indexOf('/files/') + '/files/'.length)
                    }

                    // Ajustement de la taille de l'image en fonction de celle qui a été renvoyée

                    if (!isNaN(nImgWidth) && nImgWidth > 0)
                        oTargetObj.style.width = nImgWidth + 'px';
                    else
                        oTargetObj.style.width = '';
                    if (!isNaN(nImgHeight) && nImgHeight > 0)
                        oTargetObj.style.height = nImgHeight + 'px';
                    else
                        oTargetObj.style.height = '';

                    if (bStoredInSession)
                        setAttributeValue(oTargetObj, "session", "1");
                }
            }

            // US #1 271 - Fonction partagée - Sur le nouveau mode Fiche, oSourceObj n'est pas renseigné
            // TOCHECK: dans ce mode, la MAJ de dbv peut ne pas être requise. Toutefois, on conserve le code au cas où cet attribut serait ajouté sur la balise <img>
            if (!oSourceObj) {
                if (top.window['modalImage'] && top.window['modalImage'].sourceObj) {
                    oSourceObj = top.window['modalImage'].sourceObj;
                }
            }

            // Mise à jour de l'attribut dbv de l'élément parent s'il existe déjà
            if (oSourceObj.getAttribute('dbv') != null && typeof (oSourceObj.getAttribute('dbv') != "undefined")) {
                if (strURLDbValue == null)
                    strURLDbValue = "";

                // #59874 : On ne doit pas affecter le dbv si la fiche n'est pas encore créée
                var img = oSourceObj.querySelector("img");
                var fid = getAttributeValue(img, "fid");
                // #61 325 et #63 213 - Dans certains contextes, le fileId n'est pas renseigné en attribut du champ image source (ex : signet affiché en popup ou en liste incrustée)
                // On se retrouverait donc à ne pas affecter la dbv en pensant avoir affaire à une fiche non encore créée, ce qui n'est pas le cas. Provoquant ainsi l'absence d'enregistrement de l'image
                // On tente donc, dans ce cas précis, d'aller rechercher le fileId sur d'autres éléments du DOM
                // On se base sur l'ID du champ, qui contient le FileID dans la plupart des cas (dans le cas d'un signet affiché en fiche popup par ex, ou d'une fiche affichée en signet liste)
                // Si d'autres cas similaires se présentent avec un fid vide alors qu'on se trouve bien en modification de fiche existante, les rajouter ici
                if (fid == "" && getCurrentView(oSourceObj.ownerDocument) == "FILE_MODIFICATION") {
                    var parts = oSourceObj.id.split("_");
                    if (parts && parts.length > 4) {
                        fid = parts[3];
                    }
                }
                if (fid != "0" && fid != "") {
                    setAttributeValue(oSourceObj, 'dbv', strURLDbValue);
                }
            }
        }
    }

    top.window['modalImage'].hide();
}
/*Fermer la fenêtre au clique sur annuler*/
function onImageCancel() {
    if (top.setWait != null && typeof (top.setWait) == 'function')
        top.setWait(false);

    top.window['modalImage'].hide();
}

function NewTab() {

    var strTitre = top._res_5054;
    var strType = '2';
    var nWidth = 550;
    var nHeight = 200;
    var PromptLabel = top._res_6140 + ' : ';
    var PromptValue = top._res_5054;

    oModalAdd = new eModalDialog(strTitre, strType, null, nWidth, nHeight);
    oModalAdd.setPrompt(PromptLabel, PromptValue);
    oModalAdd.show();
    oModalAdd.addButton(top._res_29, onAddCancel, "button-gray", null); // Annuler
    oModalAdd.addButton(top._res_28, onAddOk, "button-green"); // Valider

}


function onAddCancel(val) {
    oModalAdd.hide();
}

function onAddOk(val) {

    url = "eTabsSelectDiv.aspx";
    var selectionname = oModalAdd.getPromptValue();

    var ednu = new eUpdater(url, 0);
    ednu.ErrorCallBack = function () { };
    ednu.addParam("action", "createempty", "post");
    ednu.addParam("selectionname", selectionname, "post");
    ednu.send(onAddOkTreatment);

}


function onAddOkTreatment(oDoc) {
    var success = getXmlTextNode(oDoc.getElementsByTagName("success")[0]);

    oModalAdd.hide();

    if (success != "1") {
        var errDesc = getXmlTextNode(oDoc.getElementsByTagName("error")[0]);
        eAlert(2, top._res_5054, '', errDesc);
    }
    else {
        var newId = getXmlTextNode(oDoc.getElementsByTagName("newid")[0]);

        setTabOrder(newId);

    }

}

/*
shFileInPopup : 
- tab : Table que l'on souhaite afficher en pop up
- fielid : Id de fiche à afficher mais pour une création à 0 
- lWidth : largeur de fenêtre (si à null une taille par  défaut est tout de même effectuer)
- lHeight : hauteur de fenêtre (si à null une taille par  défaut est tout de même effectuer)
- bMail : True : on souhaite afficher la pop up spécifique aux e-mails sinon affichage classique
- mainFieldValue : valeurs à affecter au champ principal
- bApplyCloseOnly : True : on souhaite ne voir apparraitre que les boutons Appliquer et le bouton Fermer (sans Appliquer et fermer)
- afterValidate : Methode à appeler lors de la validation de la fenêtre 
- nCallFrom : Point d'appel de la méthode (si modif, modifier également dans eConst.cs : ShFileCallFrom)
1 : Menu nouveau de navbar
2 : Bouton nouveau du mode fiche
3 : Bouton nouveau du mode liste
4 : Bouton nouveau de bookmark
5 : Finder
6 : Duplication de fiche
7 : Affectation globale
8 : Création invitaion ++
9 : Consultation Mode fiche du mail de la Campaigne (different du mode standard)
*/
var CallFromNavBar = 1;
var CallFromFile = 2;
var CallFromList = 3;
var CallFromBkm = 4;
var CallFromFinder = 5;
var CallFromDuplicate = 6;
var CallFromGlobalAffect = 7;
var CallFromGlobalInvit = 8;
var CallCampaignMail = 9;
var CallFromSpecif = 10;
var CallFromSendMail = 11;
var CallFromKanban = 12;
var CallFromTileWidget = 13;
var CallFromSendSMS = 14;
var CallFromWidgetCarto = 15;
var CallFromPurpleFile = 16;
/// <summary>appel de l'assistant à la création depuis la navbar</summary>
var CallFromNavBarToPurple = 17;
/// <summary>appel de l'assistant à la création depuis la le menu de droite</summary>
var CallFromMenuToPurple = 18;
/// <summary>appel de l'assistant à la création depuis un signet</summary>
var CallFromBkmToPurple = 19;

var MAIL_MODE_NO = 0; // ouverture en mode autre que mail
var MAIL_MODE_STD = 1; // ouverture en mode MAIL existant lecture seul/création
var MAIL_MODE_FWD = 2; // ouverture en mode Transfert mail 
var MAIL_MODE_DRAFT = 3; // ouverture en mode Brouillon mail
var MAIL_MODE_SMS = 4; // ouverture en mode sms lecture/crétion

/// <summary>
///Ouvre une fiche en popup
/// </summary>
/// <param name="tab">Descid de la table a ouvrir en popup</param>
/// <param name="fileid">Id de la fiche à ouvrir</param>
/// <param name="strTitle">Titre de la popup</param>
/// <param name="lWidth">Largeur de la popup</param>
/// <param name="lHeight">Hauteur de la popup</param>
/// <param name="nMailMode">paramètres d'affichage à appliquer sur la fenêtre popup
///                         0  (MAIL_MODE_NO): affichage d'un fichier Template autre qu'E-mail
///                         1  (MAIL_MODE_STD) : affichage d'un type E-mail, soit existant (hors brouillon) en lecture seule, ou en mode Création (nouvel e-mail ou reprise d'un brouillon)
///                         2  (MAIL_MODE_FWD) : affichage d'un type E-mail existant en mode création pour transfert</param>
/// <param name="mainFieldValue"> ?? </param>
/// <param name="bApplyCloseOnly"> Affiche les boutons "appliquer" et "fermer" (pas de bouton qui fait les deux : "appliquer et fermer" </param>
/// <param name="afterValidate">Fonction a lancer après la validation de la popup</param>
/// <param name="nCallFrom">POint d'appel de la méthode (cf ci-dessus)</param>
/// <param name="sOrigFrameId"> id de la frame ayant ouvert la popup</param>
/// <param name="oOptions">Object contenant des options complémentaires</param>
/// <returns></returns>
function shFileInPopup(tab, fileid, strTitle, lWidth, lHeight, nMailMode, mainFieldValue, bApplyCloseOnly, afterValidate, nCallFrom, sOrigFrameId, oParentInfo, callBack, oOptions) {

    top.setWait(true);



    if (typeof (nCallFrom) == "undefined" || nCallFrom == null)
        nCallFrom = 0;

    if (typeof (oOptions) == "undefined" || oOptions === null)
        oOptions = {};


    // mailMode indique les paramètres d'affichage à appliquer sur la fenêtre popup
    // 0 : affichage d'un fichier Template autre qu'E-mail
    // 1 : affichage d'un type E-mail, soit existant (hors brouillon) en lecture seule, ou en mode Création (nouvel e-mail ou reprise d'un brouillon)
    // 2 : affichage d'un type E-mail existant en mode création pour transfert
    if (typeof (nMailMode) == 'undefined')
        nMailMode = MAIL_MODE_NO;

    if (typeof (mainFieldValue) == "undefined" || mainFieldValue == null)
        mainFieldValue = "";

    var oTabWH = getWindowWH(top);

    if (!lWidth) {
        lWidth = oTabWH[0] - 150;
        //GCH commenté (et utilisation de la ligne ci-dessus) car dans le cas ou les signet ont un scroll et que l'on ajoute depuis un signet, la fenêtre et décalée voir non visible.
        //lWidth = top.document.body.scrollWidth - 150; //largeur SUR TOP pour que la dimension soit celle de la page principale et ne soit pas plus petite dans les frame
    }
    if (!lHeight) {
        lHeight = oTabWH[1] - 150;
        //GCH commenté (et utilisation de la ligne ci-dessus) car dans le cas ou les signet ont un scroll et que l'on ajoute depuis un signet, la fenêtre et décalée voir non visible.
        //lHeight = top.document.body.scrollHeight - 150; //hauteur SUR TOP pour que la dimension soit celle de la page principale et ne soit pas plus petite dans les frame
    }

    var sHandle = "popupFile";
    if (nCallFrom == 14)
        sHandle = "SmsMailingWizard";

    eModFile = new eModalDialog(strTitle, 0, "eFileDisplayer.aspx", lWidth, lHeight, sHandle);
    eModFile.onHideFunction = function () { eModFile = null; };
    eModFile.CallFrom = nCallFrom;
    globalModalFile = true;

    eModFile.fileId = fileid;
    eModFile.addParam("fileid", fileid, "post");
    eModFile.tab = tab;
    eModFile.addParam("tab", tab, "post");
    // Passage de la taille de la fenêtre en Request.Form pour adapter le contenu en fonction dans eFileDisplayer (template E-mail)
    eModFile.addParam("width", lWidth, "post");
    eModFile.addParam("height", lHeight, "post");
    eModFile.addParam("clone", nCallFrom == CallFromDuplicate ? "1" : "0", "post");
    eModFile.addParam("iframeScrolling", "yes", "post");

    //on transmet des valeurs par défaut prérenseignées
    if (nCallFrom == CallFromFinder) {
        try {
            eModFile.addParam("defvalues", document.getElementById("defValues").value, "post");
        }
        catch (e) {

        }
    }

    //MOU-24/04/2014 Mode en lecture seule de l'e-mail de la campaigne   
    if (nCallFrom == CallCampaignMail) {
        //1 : Afficher une fiche campaigne en mode standard (en lecture seule)
        //2 : Afficher uniquement les rubriques du mail de la fiche campaigne
        eModFile.addParam("campaignreadonly", "2", "post");
        eModFile.hideMaximizeButton = true;
    }

    // SMS
    if (nCallFrom == CallFromSendSMS) {
        eModFile.addParam("bSms", "1", "post");
    }

    eModFile.ErrorCallBack = function () {
        top.setWait(false);
        eModFile.hide();
        eModFile = null;
    };

    //Callback


    eModFile.onIframeLoadComplete = function () {
        top.setWait(false);

        if (typeof (callBack) == "function") {
            callBack(this);
        }

    }

    if (nMailMode == MAIL_MODE_NO) {
        if (mainFieldValue != "")
            eModFile.addParam("mainfieldvalue", mainFieldValue, "post");
    }
    else {
        if (mainFieldValue != "")
            eModFile.addParam("mailto", mainFieldValue, "post");
        if (nMailMode == MAIL_MODE_FWD)
            eModFile.addParam("mailforward", "1", "post");
        if (nMailMode == MAIL_MODE_DRAFT)
            eModFile.addParam("maildraft", "1", "post");
    }


    var parentTab = getTabFrom(eModFile.myOpenerWin);
    if (oOptions.hasOwnProperty("currentTab" || typeof oOptions.currentTab === "string" && oOptions.currentTab.length > 0))
        parentTab = oOptions.currentTab;

    var currentDoc = (nCallFrom == CallFromKanban) ? document : top.document;
    var currentView = getCurrentView(currentDoc);

    if (nCallFrom == CallFromSpecif) {
        eModFile.addParam("readonly", "1", "post");
    }


    if (fileid == 0 && (nCallFrom == CallFromGlobalAffect || nCallFrom == CallFromGlobalInvit)) {
        //Pour une affectation globale ou invitation
        //Information tab parent

        eModFile.addParam("globalaffect", "1", "post");
        eModFile.addParam("parenttab", top.nGlobalActiveTab, "post");
        if (top.GetCurrentFileId)
            eModFile.addParam("parentfileid", top.GetCurrentFileId(top.nGlobalActiveTab), "post");
        else
            eModFile.addParam("parentfileid", 0, "post");

        eModFile.addParam("globalinvit", (nCallFrom == CallFromGlobalInvit) ? "1" : "0", "post");
        eModFile.GlobalAffect = true;
        eModFile.GlobalInvit = (nCallFrom == CallFromGlobalInvit);
        eModFile.ParentTab = top.nGlobalActiveTab;
    }
    else if (fileid == 0 && (nCallFrom == CallFromSendMail || nCallFrom == CallFromSendSMS) && oParentInfo) {
        //var fldEngine = getFldEngFromElt(origElt);
        //var parentFileId = GetFieldFileId(origElt.id);
        //var parentTab = getTabDescid(fldEngine.descId);

        eModFile.addParam("parenttab", oParentInfo.parentTab, "post");
        eModFile.addParam("parentfileid", oParentInfo.parentFileId, "post");
    }
    else if (fileid == 0 && typeof (top.GetCurrentFileId) == "function"
        && nCallFrom != CallFromNavBar
        && (currentView == "FILE_MODIFICATION" || currentView == "FILE_CREATION")) {

        // Pour une création depuis un signet 
        // Informations fiches parentes
        // Correctif #50029 commenté 
        // var parentFileId = top.GetCurrentFileId(parentTab)
        eModFile.addParam("parenttab", parentTab, "post");

        // Correctif #50026 commenté 
        // eModFile.addParam("parentfileid", parentFileId, "post");
        eModFile.addParam("parentfileid", top.GetCurrentFileId(parentTab), "post");



        if (oOptions.hasOwnProperty("lnkid" || typeof oOptions.lnkid === "string" && oOptions.lnkid.length > 0)) {
            eModFile.addParam("lnkid", oOptions.lnkid, "post");
        }
        else {
            var olnkids = document.getElementById("lnkid_" + top.nGlobalActiveTab);
            if (olnkids == null || typeof (olnkids) == 'undefined')
                olnkids = document.getElementById("lnkid_finder");
            if (olnkids != null && typeof (olnkids) != 'undefined')
                eModFile.addParam("lnkid", getAttributeValue(olnkids, "value"), "post");
        }

        // pour les liaisons spéciales
        var oDivBkm = top.document.getElementById("BkmHead_" + tab);
        if (oDivBkm && oDivBkm.getAttribute("spclnk")) {
            eModFile.addParam("spclnk", oDivBkm.getAttribute("spclnk"), "post");
        }

        //rubriques associées
        if (oOptions.defaultValues != null) {
            eModFile.addParam("defvalues", JSON.stringify(oOptions.defaultValues), "post");
        }



    }
    else if (fileid == 0 && nCallFrom == CallFromFinder) {

        var oDoc = document;
        var oWin = window;
        if (sOrigFrameId && sOrigFrameId != "") {
            var oOrigFrm = top.document.getElementById(sOrigFrameId);
            if (oOrigFrm) {
                if (oOrigFrm.contentDocument)
                    oDoc = oOrigFrm.contentDocument;
                if (oOrigFrm.contentWindow)
                    oWin = oOrigFrm.contentWindow;
            }
        }
        eModFile.addParam("lnkid", getAttributeValue(oDoc.getElementById("lnkid_" + oWin.nGlobalActiveTab), "value"), "post");
    }


    // Options complémentaires
    if (oOptions) {
        // Pas de chargement de la fiche après la validation
        if (oOptions.noLoadFile) {
            eModFile.addParam("noloadfile", "1", "post");
        }

    }

    // Activation de la barre d'outils sur la fenêtre selon le type de fichier affiché
    // Fichier de type E-mail
    if (nMailMode != MAIL_MODE_NO) {
        // Nouvel E-mail (fileid = 0), transfert de mail existant (nMailMode = 2) ou brouillon (nMailMode = 3)
        // MCR 40833 sur un Email Existant, ajout du bouton supprimer pour donner la possibilite de supprimer le mail en mode fiche
        // if (fileid == 0 || nMailMode == MAIL_MODE_FWD || nMailMode == MAIL_MODE_DRAFT) {
        if (fileid == 0 || nMailMode == MAIL_MODE_FWD) {
        }
        // E-mail existant
        else {
            eModFile.noToolbar = false;
        }
    }
    else {
        eModFile.noToolbar = false;
    }

    closeRightMenu();

    eModFile.show();

    var ws = getWindowSize();

    // Agrandissement de la fenêtre après affichage selon le type de fichier demandé
    // Fichier de type E-mail (hors SMS), ou taille inférieure à 900/600
    if ((nMailMode != MAIL_MODE_NO && nCallFrom != CallFromSendSMS) || ws.w < 900 || ws.h < 600) {
        eModFile.MaxOrMinModal();
    }

    // Ajout des boutons sur la fenêtre selon le type de fichier affiché
    // Fichier de type Template hors E-Mail
    if (nMailMode == MAIL_MODE_NO) {

        //MOU-24/04/2014 mail de campaigne : ajout de bouton de fermeture
        if (nCallFrom == CallCampaignMail || nCallFrom == CallFromSpecif) {
            //Affichage d'une partie de la fiche campaigne en popup en lecture seule (les rubriques du mail utilisés) 
            //ATTENTION : à ne pas confondre avec un template E-mail)
            eModFile.addButton(top._res_30, function () { eModFile.hide(); }, "button-gray");      // Fermer
            if (nCallFrom == CallFromSpecif) {
                eModFile.addButton(top._res_8167, function () { eModFile.hide(); loadFile(tab, fileid); }, "button-green", null, "openintab");      // Ouvrir dans l'onglet
            }
        } else {

            var nValFileid = nCallFrom == CallFromDuplicate ? 0 : fileid;
            eModFile.addButtonFct(top._res_29, cancelTpl, "button-gray", "cancel");// Annuler
            if (bApplyCloseOnly) {
                if (nCallFrom == CallFromGlobalAffect || nCallFrom == CallFromGlobalInvit) {
                    eModFile.addButton(top._res_28, function () { afterValidate(eModFile); }, "button-green");     // Valider
                } else {
                    //on souhaite ne voir apparraitre que les boutons Appliquer et le bouton Fermer (sans Appliquer et fermer)
                    var bStopRefresh = false;
                    var fctValid = (function (pTab, pValFileid, pFields, pClose, pModFile, pafterValidate, pStopRefresh, pCallFrom) {
                        return function () {
                            validateFile(pTab, pValFileid, pFields, pClose, pModFile, pafterValidate, pStopRefresh, pCallFrom);
                        };
                    })(tab, nValFileid, null, true, eModFile, afterValidate, false, nCallFrom);

                    eModFile.addButton(top._res_28, function () { launchChkDbl(tab, nValFileid, eModFile, fctValid); }, "button-green", null, "save");   // Valider
                }
            }
            else {
                eModFile.addButton(top._res_286, function () { validateFile(tab, nValFileid, null, false, eModFile, afterValidate); }, "button-green", null, "save");   // Valider
                eModFile.addButton(top._res_869, function () { validateFile(tab, nValFileid, null, true, eModFile, afterValidate); }, "button-green", null, "savenclose");  // Appliquer et fermer
            }

            // Barre d'outils pour les fichiers de type Template
            eModFile.addTemplateButtons(tab, nValFileid);
            eModFile.setToolBarVisible(
                //eModFile.ToolbarButtonType.CancelLastValuesButton + ";" +
                eModFile.ToolbarButtonType.PropertiesButton + ";" +
                eModFile.ToolbarButtonType.MandatoryButton + ";" +
                eModFile.ToolbarButtonType.PjButton + ";" +
                eModFile.ToolbarButtonType.PrintButton + ";" +
                eModFile.ToolbarButtonType.SendMailButton +
                (fileid != 0 ? (";" + eModFile.ToolbarButtonType.DeleteButton) : '')
                , true
            );
        }
    }
    // Fichier de type E-mail
    else {
        // Nouvel E-mail (fileid = 0), transfert de mail existant (nMailMode = 2) ou brouillon (nMailMode = 3)
        if (fileid == 0 || nMailMode == MAIL_MODE_FWD || nMailMode == MAIL_MODE_DRAFT) {
            //Pour Engine.js, le transfert du mail est une création (fileid = 0)
            if (nMailMode == MAIL_MODE_FWD) {
                fileid = 0;
                eModFile.fileId = 0;
            }
            eModFile.addButtonFct(top._res_29, cancelTpl, "button-gray", "cancel");   // Annuler

            var sTestMailOrSMSRes = top._res_6163; // Envoyer un e-mail de test
            if (nCallFrom == CallFromSendSMS)
                sTestMailOrSMSRes = top._res_1880; // Envoyer un SMS de test

            eModFile.addButton(sTestMailOrSMSRes, function () {

                //TODO passer le descid de status depuis le serveur,
                var MailStatusDescId = parseInt(tab) + 85;

                // On indique que le bouton doit déclencher un envoi de mail de test
                eModFile.getIframe().document.getElementById('mailSaveAsDraft').value = "0";
                eModFile.getIframe().document.getElementById('mailIsTest').value = "1";

                getMailRecipientsBeforeSending(sTestMailOrSMSRes, tab, fileid, eModFile, true, function () { validateFile(tab, fileid, null, false, eModFile, afterValidate); });

            }, "button-gray", null, "mailtest_btn", "left");    // Envoyer un e-mail de test



            eModFile.addButton(top._res_944, function () {

                //TODO passer le descid de status depuis le serveur,
                var MailStatusDescId = parseInt(tab) + 85;

                // On indique que le bouton doit déclencher un envoi de mail usuel
                eModFile.getIframe().document.getElementById('mailSaveAsDraft').value = "0";
                eModFile.getIframe().document.getElementById('mailIsTest').value = "0";

                getMailRecipientsBeforeSending(sTestMailOrSMSRes, tab, fileid, eModFile, false, function () { validateFile(tab, fileid, null, true, eModFile, afterValidate); });

            }, "button-green");    // Envoyer


            if (nCallFrom != CallFromSendSMS) {
                eModFile.addButton(top._res_1582, function () {

                    // On indique que le bouton doit déclencher un enregistrement en brouillon
                    eModFile.getIframe().document.getElementById('mailSaveAsDraft').value = "1";
                    eModFile.getIframe().document.getElementById('mailIsTest').value = "0";

                    if (fileid == 0) {
                        //TODO recuperer le descid depuis le server
                        var MailStatusDescId = parseInt(tab) + 85;
                        eModFile.getIframe().document.getElementById("COL_" + tab + "_" + MailStatusDescId + "_0_0_0").value = "3";
                    }

                    validateFile(tab, fileid, null, true, eModFile, afterValidate);
                }, "button-gray");     // Brouillon
            }

            if (fileid != 0 && nMailMode == MAIL_MODE_DRAFT && currentView == "FILE_MODIFICATION") {

                eModFile.addTemplateButtons(tab, fileid);
                eModFile.setToolBarVisible(
                    //eModFile.ToolbarButtonType.CancelLastValuesButton + ";" +
                    eModFile.ToolbarButtonType.PropertiesButton + ";" +
                    eModFile.ToolbarButtonType.MandatoryButton + ";" +
                    eModFile.ToolbarButtonType.PjButton + ";" +
                    eModFile.ToolbarButtonType.PrintButton + ";" +
                    eModFile.ToolbarButtonType.SendMailButton +
                    (fileid != 0 ? (";" + eModFile.ToolbarButtonType.DeleteButton) : '')
                    , true
                );
            }
        }
        // E-mail existant
        // MCR 40833 sur un Email Existant, ajout du bouton supprimer pour donner la possibilite de supprimer le mail en mode fiche
        else {
            if (nGlobalActiveTab != TAB_CAMPAIGN) {

                //TODO : ADD CALLFROM
                //TODO : CHECK CALLBACK

                eModFile.addButton(top._res_1339, function () {
                    eModFile.hide();
                    top.shFileInPopup(tab, fileid, strTitle, lWidth, lHeight, 2, mainFieldValue, false, afterValidate);

                }, "button-gray");      // Transférer


                eModFile.addButton(top._res_869, function () { validateFile(tab, fileid, null, true, eModFile, afterValidate); }, "button-green");    // Appliquer et fermer
                // Barre d'outils pour les fichiers de type E-mail existant
                if (fileid != 0) {
                    eModFile.addTemplateButtons(tab, fileid);
                    eModFile.setToolBarVisible(eModFile.ToolbarButtonType.DeleteButton);
                }
            }
            else {
                eModFile.addButton(top._res_30, function () { eModFile.hide(); }, "button-green");      // Fermer
            }
        }
    }
}

//Assistant de création téléguidé
function openPurpleFile(tab, fileid, mainFieldValue, nCallFrom, oParentInfo) {

    // KHA le 23/03/2021
    // dans le cas d'une création de contact depuis société on ne passe pas par le nouveau mode fiche téléguidé
    // ce cas est à développer
    if (tab == TAB_PP && top.nGlobalActiveTab == TAB_PM && nCallFrom == CallFromBkm) {
        console.log("emain.js -> openPurpleFile");
        console.log("dans le cas d'une création de contact depuis société on ne passe pas par le nouveau mode fiche téléguidé");
        return false;
    }

    //le mode téléguidé avec la nouvelle ergonomie
    /** petit problème avec les signets du nouveau mode fiche
     * qui redirigent également là */

    if (!isIris(tab, "dvIrisPurpleInput")) {
        console.log("emain.js -> openPurpleFile");
        console.log("La saisie guidée n'est pas activée sur cette table ou la fiche n'est pas en création");
        return false;
    }


    setWait(false);

    var loc = window.location.pathname;
    var dir = loc.substring(0, loc.lastIndexOf('/'));

    var oCtxInfos;

    if (oParentInfo) {
        if (oParentInfo.parentTab && oParentInfo.parentFileId) {
            oCtxInfos = {
                values: [{
                    descid: oParentInfo.parentTab,
                    value: oParentInfo.parentFileId
                }]
            };
        }
        else if (oParentInfo.values && oParentInfo.values.length) {
            oCtxInfos = oParentInfo;
        }
    }

    if (mainFieldValue) {
        if (!oCtxInfos)
            oCtxInfos = { values: [] };
        if (!oCtxInfos.values)
            oCtxInfos.values = [];


        oCtxInfos.values.push({
            descid: getNumber(tab) + 1,
            value: mainFieldValue,
            changed: true,
        });
    }

    var fnAddScript = addScript;

    if (window.top !== window.self) {
        fnAddScript = window.top.addScript
    }

    var fnAddScriptCallback = function () {

        if (window.top !== window.self) {
            window.top.LoadIrisFileNew(tab, fileid, dir + "/IRISBlack/Front/Scripts/", oCtxInfos);

            var catalogObject = top.eTabLinkCatFileEditorObject[_parentIframeId];
            catalogObject.oModalLnkFile.hide();

            return;
        }

        LoadIrisFileNew(tab, fileid, dir + "/IRISBlack/Front/Scripts/", oCtxInfos);
    }

    // Inutile de rajouter eInitNewErgo à chaque appel de shFileInPopup, s'il a déjà été inclus, et la fonction déjà disponible...
    if (typeof (window.top.LoadIrisFileNew) == "function")
        fnAddScriptCallback();
    else {
        fnAddScript("../IRISBlack/Front/Scripts/eInitNewErgo", "Guided", fnAddScriptCallback);
    }

    return true;



}

function showAdvPopup(options) {
    if (typeof (options) == "undefined") {
        console.log("options non définit")
        return;
    }

    if (typeof (options.tab) == "undefined") {
        console.log("options.tab non définit")
        return;
    }

    var size = getWindowSize().scale(0.95);

    // la fiche de la popup est complètement chargée
    options.fileLoad = GetDefaultValueIfUndefined(options.fileLoad, function () { });
    options.cancel = GetDefaultValueIfUndefined(options.cancel, function () { });
    var internalCancel = function () { options.cancel(); eModFile.hide(); }

    options.afterSave = GetDefaultValueIfUndefined(options.afterSave, function () { });
    // Vérification intermédiare
    var internalAfterSave = function (oRecord) {
        if (oRecord && typeof (oRecord.tab) != "undefined" && typeof (oRecord.fid) != "undefined") {
            options.afterSave({
                "success": true,
                "tab": oRecord.tab,
                "fid": oRecord.fid
            });
        } else {
            options.afterSave({
                "success": false,
                "data": oRecord
            });
        }
    };

    options.width = GetDefaultValueIfUndefined(options.width, size.w);
    options.height = GetDefaultValueIfUndefined(options.height, size.h);
    options.fileId = GetDefaultValueIfUndefined(options.fileId, 0);
    options.scroll = GetDefaultValueIfUndefined(options.scroll, true);
    options.callFrom = GetDefaultValueIfUndefined(options.callFrom, CallFromFile);
    options.defaultValues = GetDefaultValueIfUndefined(options.defaultValues, "");
    options.title = GetDefaultValueIfUndefined(options.title, options.fileId == 0 ? top._res_31 : top._res_85);

    eModFile = new eModalDialog(options.title, options.fileId, "eFileDisplayer.aspx", options.width, options.height, "popupFile");
    eModFile.onHideFunction = function () { eModFile = null; };
    eModFile.CallFrom = options.CallFrom;

    eModFile.tab = options.tab;
    eModFile.fileId = options.fileId;
    eModFile.addParam("tab", options.tab, "post");
    eModFile.addParam("fileid", options.fileId, "post");
    eModFile.addParam("width", options.width, "post");
    eModFile.addParam("height", options.height, "post");
    eModFile.addParam("iframeScrolling", options.scroll ? "yes" : "no", "post");
    eModFile.addParam("defvalues", options.defaultValues, "post");
    eModFile.addParam("noloadfile", "1", "post");

    eModFile.ErrorCallBack = function () {
        top.setWait(false);
        internalCancel();
        eModFile = null;
    };
    eModFile.onIframeLoadComplete = function () {
        top.setWait(false);
        options.fileLoad(this);
    }

    eModFile.noToolbar = false;
    eModFile.show();

    eModFile.addButton(top._res_286, function () { validateFile(options.tab, options.fileId, null, true, eModFile, internalAfterSave, true, options.CallFrom, false); }, "button-green", null, "save");   // Valider
    eModFile.addButtonFct(top._res_29, function () { internalCancel(); }, "button-gray", "cancel"); // Anuller

}

/// Pour les valeur non définies
function GetDefaultValueIfUndefined(param, defaultValue) {
    if (typeof (param) == "undefined")
        return defaultValue;
    return param;
}

///summary
///Vérification des destinataires avant envoi d'un mail, et choix des destinataires dans le cas d'un mail de test
///summary
var oModalAddRecipient = null;
function getMailRecipientsBeforeSending(strTitle, nTab, nFileId, eModFile, bIsMailTest, afterValidateFctOrMailingObj) {

    var strType = '2';
    var nWidth = 460;
    var nHeight = 200;
    var PromptLabel = top._res_389; // Destinataire
    var PromptValue = "";
    var realRecipients = null;
    var mainPopupDiv;
    if (eModFile)
        mainPopupDiv = eModFile.getIframe().document.getElementById("fileDiv_" + nTab);
    var bIsSMS = false;
    var smsPhoneNumber = "";
    if (mainPopupDiv)
        bIsSMS = getAttributeValue(mainPopupDiv, "sms");

    // Cas mail ou SMS unitaire
    nTab = nTab * 1;
    if (eModFile != null) {
        // BSE: #51183 pour l'envoi d'amil de test, récuperer l'adresse mail de l'expéditeur
        //SHA : correction bug # 72 938
        //ajout d'alerte pour objet de SMS/mail vide, et pas d'envoi de SMS/mail
        var mailObject = "";
        if (eModFile.getIframe() != null && typeof (eModFile.getIframe()) != "undefined") {
            //Disable the buttons Send Test SMS and Send for unit SMS over 765 characters in the message 
            if (eModFile.Handle == "SmsMailingWizard" && eModFile.getIframe().oSmsing && eModFile.getIframe().oSmsing.IsMaxLengthMessageAttempted()) {
                eAlert(0, '', top._res_8768);
                return;
            }
            if (eModFile.getIframe().document.getElementsByClassName("mailSubject") && eModFile.getIframe().document.getElementsByClassName("mailSubject").length > 0)
                mailObject = eModFile.getIframe().document.getElementsByClassName("mailSubject")[0].getElementsByClassName("LNKFREETEXT edit")[0].value;
            else if (eModFile.getIframe().document.getElementsByClassName("eMemoEditor") && eModFile.getIframe().document.getElementsByClassName("eMemoEditor").length > 0)
                mailObject = eModFile.getIframe().document.getElementsByClassName("eMemoEditor")[0].value;
            if (typeof mailObject === 'undefined' || mailObject == null || mailObject == '') {
                if (bIsSMS)
                    eAlert(2, top._res_2238, top._res_2336);
                else
                    eAlert(2, top._res_2881, top._res_2336);
                return;
            }
        }

        //SHA : ajout d'alerte pour corps de SMS/mail vide, et pas d'envoi de SMS/mail
        var ckeTextareas = eModFile.getIframe().document.querySelectorAll("[textarea], [id^=cke_]");
        if (ckeTextareas.length > 0) {
            var iframe = eModFile.getIframe().document.getElementById("cke_edtCOL_" + nTab + "_" + (getNumber(nTab) + 8) + "_" + nFileId + "_" + nFileId + "_0").getElementsByTagName("iframe")[0];
            var iframeDocument = iframe.contentDocument || iframe.contentWindow.document;
            var smsBodyHtml = iframeDocument.getElementsByClassName("cke_contents_ltr")[0];
            var smsBody = smsBodyHtml.innerText.trim();
            if (typeof smsBody === 'undefined' || smsBody == null || smsBody == '') {
                if (bIsSMS)
                    eAlert(2, top._res_2238, top._res_2239);
                else
                    eAlert(2, top._res_2881, top._res_2239);
                return;
            }
        }
        else {
            var smsBodyHtml = eModFile.getIframe().document.getElementById("edtCOL_" + nTab + "_" + (getNumber(nTab) + 8) + "_" + nFileId + "_" + nFileId + "_0");
            var smsBody = smsBodyHtml.value;
            if (typeof smsBody === 'undefined' || smsBody == null || smsBody == '') {
                if (bIsSMS)
                    eAlert(2, top._res_2238, top._res_2239);
                else
                    eAlert(2, top._res_2881, top._res_2239);
                return;
            }
        }

        realRecipients = eModFile.getIframe().document.getElementById("COL_" + nTab + "_" + (nTab + FLD_MAIL_FROM) + "_" + nFileId + "_" + nFileId + "_0");
        if (!realRecipients) {
            if (mainPopupDiv) {
                nFileId = getAttributeValue(mainPopupDiv, "fid");
                realRecipients = eModFile.getIframe().document.getElementById("COL_" + nTab + "_" + (nTab + FLD_MAIL_FROM) + "_" + nFileId + "_" + nFileId + "_0");
            }
        }
        if (realRecipients) {
            realRecipients = realRecipients.childNodes[0];
            smsPhoneNumber = getAttributeValue(realRecipients, "phonenumber");
        }

        // Si les informations du champ Expéditeur ne sont pas exploitables, on utilise le champ Destinataires
        if (realRecipients == null || realRecipients.nodeType == 3 || realRecipients.tagName.toLowerCase() == "input" || realRecipients.options[realRecipients.selectedIndex].value == null || realRecipients.options[realRecipients.selectedIndex].value.trim() == "") {
            realRecipients = eModFile.getIframe().document.getElementById("COL_" + nTab + "_" + (nTab + FLD_MAIL_TO) + "_" + nFileId + "_" + nFileId + "_0");
            if (realRecipients.tagName.toLowerCase() == "td")
                realRecipients = realRecipients.childNodes[0];
            if (bIsSMS) {
                // Pour les SMS, on ne peut pas utiliser le DescID de la liste déroulante du champ à utiliser comme numéro de test. On utilisera donc toujours
                // smsPhoneNumber
                // Par contre, pour les envois réels, il faut tester que cette valeur de DescID soit passée. Donc, dans ce cas précis, on l'affecte à PromptValue
                // pour la passer à checkMailBeforeSending
                if (
                    (realRecipients.tagName.toLowerCase() == "input" && realRecipients.value.trim() == "") ||
                    (realRecipients.tagName.toLowerCase() == "td" && realRecipients.options[realRecipients.selectedIndex].value.trim() == "") ||
                    bIsMailTest
                )
                    PromptValue = smsPhoneNumber;
                else
                    PromptValue = realRecipients.value.trim();
            }
            else
                PromptValue = realRecipients.value + ";";
        }
        else {
            if (bIsSMS)
                PromptValue = smsPhoneNumber;
            else
                PromptValue = realRecipients.value + ";";
        }
    }

    // Cas E-mailing
    if (realRecipients == null) {
        realRecipients = document.getElementById("sender-opt");
        if (realRecipients != null)
            PromptValue = realRecipients.options[realRecipients.selectedIndex].value + ";";
    }

    if (bIsMailTest) {
        oModalAddRecipient = new eModalDialog(strTitle, strType, null, nWidth, nHeight);
        oModalAddRecipient.hideMaximizeButton = true;
        oModalAddRecipient.setPrompt(PromptLabel, PromptValue);
        oModalAddRecipient.show();
        oModalAddRecipient.addButton(top._res_29, function () { oModalAddRecipient.hide(); }, "button-gray", null); // Annuler
        oModalAddRecipient.addButton(top._res_944, function () { checkMailBeforeSending(oModalAddRecipient.getPromptValue(), true, bIsSMS, afterValidateFctOrMailingObj); }, "button-green"); // Valider
    }
    else {
        checkMailBeforeSending(PromptValue, false, bIsSMS, afterValidateFctOrMailingObj);
    }
};

///Validation des adresses e-mail avant envoi
function checkMailBeforeSending(values, bIsMailTest, bIsSMS, afterValidateFctOrMailingObj) {

    var invalidMails = "";

    //Pas d'adresse
    if (values.length == 0) {
        invalidMails = bIsSMS ? top._res_1882 : top._res_6738; // Vous devez renseigner au moins un email/numéro de téléphone pour le test

    } else {
        var mails = values.split(";");
        var mailCount = mails.length;
        var emptyMailCount = 0;

        for (var i = 0; i < mails.length; i++) {
            if (typeof (mails[i]) != "function") {
                var mailAddr = mails[i];
                // Si la chaîne à tester est de la forme NOM <adresse> ou NOM [adresse], on ne teste que adresse
                if (mailAddr.indexOf("<") > -1 && mailAddr.indexOf(">") > -1)
                    mailAddr = mailAddr.substring(mailAddr.indexOf("<") + 1, mailAddr.indexOf(">"));
                else if (mailAddr.indexOf("[") > -1 && mailAddr.indexOf("]") > -1)
                    mailAddr = mailAddr.substring(mailAddr.indexOf("[") + 1, mailAddr.indexOf("]"));

                //SHA : correction bug de la user story "SMS Net message > Envoi" à propos de l'alerte envoyée quand téléphone formaté (espace ou . ou - ou rien)
                //SHA : pas de limitation sur la regex de téléphone (vu avec HLA)
                //ALISTER => Demande 85 805/ Request 85 805 L'indicatif du numéro de téléphone
                if (mailAddr.length > 0 && (!bIsSMS && !eValidator.isEmail(mailAddr) ||
                    (bIsSMS && !eValidator.isPhone(mailAddr))))
                    invalidMails = invalidMails + mails[i] + "<br />";

                else if (mailAddr.length == 0)
                    emptyMailCount++;
            }
        }

        if (emptyMailCount == mailCount)
            invalidMails = invalidMails + "<br />" + (bIsSMS ? top._res_1882 : top._res_6738);
    }

    //Si erreur sur les mails on arrête l'envoi
    if (invalidMails.length > 0) {
        //on affiche la fenêtre en la décalant pour faire apparaitre la fenetre parente
        eAlert(1, top._res_6275, !bIsSMS ? top._res_1147 : top._res_1881, invalidMails, 410, 200)
            .moveBy(80, 100);
        return;
    }

    if (bIsMailTest) {
        // Cas mail unitaire : le dernier paramètre passé est la fonction afterValidate
        if (typeof (afterValidateFctOrMailingObj) == "function") {
            eModFile.getIframe().document.getElementById("mailTestRecipients").value = values;
            eModFile.getIframe().document.getElementById("mailTestRecipients").setAttribute("dbv", values);
            afterValidateFctOrMailingObj();
        }
        // Cas E-mailing : le dernier paramètre est l'objet eMailing.js ("me")
        else {

            if (oMailing && typeof oMailing.majEditor == "function")
                oMailing.majEditor();

            afterValidateFctOrMailingObj.SetParam("recipientstest", values);
            afterValidateFctOrMailingObj.AddParams();
            afterValidateFctOrMailingObj.Send();
        }

        oModalAddRecipient.hide();
        oModalAddRecipient = null;
    }
    else {
        if (typeof (afterValidateFctOrMailingObj) == "function") {
            afterValidateFctOrMailingObj();
        }
    }
}

/*Lance la création automatique d'une fiche*/
function autoCreate(nTab, aParentParams, bNotLoad, fctCallBack) {
    //alert('autocreate : ' + nTab);

    var oUpdater = new eUpdater("mgr/eAutoCreateManager.ashx");
    oUpdater.ErrorCallBack = function () { };
    oUpdater.addParam("tab", nTab, "post");
    oUpdater.addParam("parenttab", top.nGlobalActiveTab, "post");
    oUpdater.addParam("lnkid", getAttributeValue(document.getElementById("lnkid_" + top.nGlobalActiveTab), "value"), "post");

    //on ajoute les tables parentes (dans le cas d'adresse PP PM)
    if (typeof (aParentParams) != "undefined") {
        for (var i = 0; i < aParentParams.length; i++) {
            oParent = aParentParams[i];

            if (oParent.tab > 0 && oParent.fid > 0) {
                oUpdater.addParam(oParent.tab, oParent.fid, "post");
                if (oParent.spclnk)
                    oUpdater.addParam(oParent.tab + "_spclnk", oParent.spclnk, "post");
            }
        }
    }

    oUpdater.send(function (oRes) { autoCreateReturn(oRes, bNotLoad, fctCallBack); });
}


function autoCreateReturn(oRes, bNotLoad, fctCallBack) {


    if (!bNotLoad) {
        var oCreatedRecord = oRes.getElementsByTagName("createdrecord")[0];
        oCreatedRecord = { fid: oCreatedRecord.getAttribute('ids'), tab: oCreatedRecord.getAttribute('tab') };
        if (oCreatedRecord == null || oCreatedRecord.tab == null || oCreatedRecord.fid == null) {
            eAlert(0, top._res_416, top._res_6237, "Votre enregistrement semble avoir été créé mais ne peut être chargé automatiquement.");
        }

        LoadFileAfterCreation(oCreatedRecord);
    }

    if (typeof (fctCallBack) == "function") {
        fctCallBack(oRes, bNotLoad);
    }
}


/*
sendMailTo : prépare un envoi de mail unitaire en utilisant shFileInPopup
*/
function sendMailTo(mail, tab, fileName, oParentInfo, mailType) {
    var oTabWH = getWindowWH(top);
    var lWidth = 0;
    var lHeight = 0;
    if (!lWidth) {
        lWidth = oTabWH[0] - 100;
    }
    if (!lHeight) {
        lHeight = oTabWH[1] - 100;
    }

    var callFrom = CallFromSendMail;
    if (mailType == TypeMailing.SMS_MAILING_FROM_BKM || mailType == TypeMailing.SMS_MAILING_UNDEFINED)
        callFrom = CallFromSendSMS;

    shFileInPopup(tab, 0, fileName, lWidth, lHeight, 1, mail, false, null, callFrom, "", oParentInfo);

}



// Annulation de la fenetre des tamplate (mails)
function cancelTpl() {

    var iframeTpl = eModFile.getIframe();
    cancelAndDeletePJ(iframeTpl);
    cancelAndDeleteImages(iframeTpl);
    eModFile.hide();
}

// verification si l'utilisateur à ajouté une pièce jointe en mode création nouvelle fiche, afin de les supprimer
function cancelAndDeletePJ(oiframeTpl) {
    try {
        // Supprimer les Pjs ajoutées si annulation de nouvelle fiche (PjIds non vide et fileid = 0)
        var fileDiv = oiframeTpl.document.getElementById("fileDiv_" + oiframeTpl.nGlobalActiveTab);
        if (oiframeTpl && typeof fileDiv != "undefined") {
            var fileid = getAttributeValue(fileDiv, 'fid');
            if (fileid == 0) {
                var ndescIdPJ = getNumber(oiframeTpl.nGlobalActiveTab) + 91;
                var oPjInfo = oiframeTpl.document.getElementById("COL_" + oiframeTpl.nGlobalActiveTab + "_" + ndescIdPJ + "_0_0_0");
                if (oPjInfo) {
                    var pjIds = getAttributeValue(oPjInfo, 'PjIds');
                    if (pjIds != '') {
                        // Delete pj from table pj et supprimer physiquement le fichier
                        DeletePJ(ndescIdPJ, fileid, pjIds, false);
                    }
                }
            }
        }
    }
    catch (ex) {
        alert('cancelAndDeletePJ');
        alert(ex);
    }
}

// verification si l'utilisateur à ajouté des images en mode création nouvelle fiche, afin de les supprimer
function cancelAndDeleteImages(oiframeTpl) {
    try {
        // Supprimer les images ajoutées si annulation de nouvelle fiche (PjIds non vide et fileid = 0)
        var fileDiv = oiframeTpl.document.getElementById("fileDiv_" + oiframeTpl.nGlobalActiveTab);
        if (oiframeTpl && typeof fileDiv != "undefined") {
            var fileid = getAttributeValue(fileDiv, 'fid');
            if (fileid == 0) {
                var oImages = oiframeTpl.document.querySelectorAll("img");
                if (oImages) {
                    for (var i = 0; i < oImages.length; i++) {
                        if (oImages[i].src.indexOf("fid=-1") != -1 || getAttributeValue(oImages[i], "session") == "1") {
                            var imageFieldDescId = 0;

                            if (oiframeTpl.document.getElementById(getAttributeValue(oImages[i].parentNode, "ename")))
                                imageFieldDescId = getAttributeValue(oiframeTpl.document.getElementById(getAttributeValue(oImages[i].parentNode, "ename")), "did");

                            deleteImage(false, "IMAGE_FIELD", imageFieldDescId, 0);
                        }
                    }
                }
            }
        }
    }
    catch (ex) {
        alert('cancelAndDeleteImages');
        alert(ex);
    }
}

//Fermeture des modales
function closeActionMenu() {
    if (top.window['_cm']) {
        if (top.window['_cm']['ActionMenu'] && top.window['_cm']['ActionMenu'].IsContextMenu)
            top.window['_cm']['ActionMenu'].hide();
    }
}

//Fermeture des modales
//BSE #50 912 Fermer toutes les modals sauf les specifs
function closeAllModals() {
    if (top.window['_md'] && Array.isArray(top.window["_md"])) {


        for (var i in top.window['_md']) {
            var modal = top.window['_md'][i];
            if (modal.isModalDialog && !modal.isSpecif && !modal.NoGlobalCLose) {
                top.window['_md'][i].hide();
            }
        }
    }
}

//Indique le nombre de fenêtres modales ouvertes et déclarées (depuis le document indiqué, ou le document racine par défaut)
function getModalCount(oWindow) {
    if (!oWindow)
        oWindow = top.window;

    var modalCount = 0;
    if (oWindow['_md']) {
        for (var i in oWindow['_md']) {
            if (oWindow['_md'][i].isModalDialog) {
                modalCount++;
            }
        }
    }

    return modalCount;
}

//Indique si des fenêtres modales sont actuellement ouvertes (depuis le document racine)
function modalWindowsOpened() {
    var isCurrentDocModal = document["_ismodal"] == 1 && window.isPopup();
    var nModalCount = getModalCount();
    return nModalCount > 0 || isCurrentDocModal;
}

/// Action sur click gauche
function fldLClick(e) {

    if (!e) {
        return fldAction(e, "LCLICK");
    }
    else {
        try {
            // ajout test sur e
            if (e.srcElement && e.srcElement.document && e.srcElement.document.parentWindow) {
                oWin = e.srcElement.document.parentWindow;
                return oWin.fldAction(e, "LCLICK");
            }
            else
                return fldAction(e, "LCLICK");
        }
        catch (exx) {
            return fldAction(e, "LCLICK");
        }

    }
}
/// Action à l'appui sur une touche (principalement Entrée)
function fldKeyUp(e) {
    return fldAction(e, "KEYUP");
}
// Action lors de la sélection de la valeur d'un champ (généralement similaire au clic gauche)
function fldSelect(e) {
    return fldAction(e, "SELECT");
}


function fldInfoOnContext(e) {

    try {
        // Récupération de l'évènement
        if (!e)
            var e = window.event;


        if (e.altKey) {


        }

        //
        if (!e.ctrlKey)
            return;


        // Objet source
        var oSourceObj = e.target || e.srcElement;
        var oSourceObjOrig = oSourceObj;
        var topelement = "TABLE";

        try {
            while (
                oSourceObj.tagName != topelement
                && (!((oSourceObj.tagName == 'TR' && oSourceObj.getAttribute("eid") != null && oSourceObj.getAttribute("eid").length > 0))
                    && !((oSourceObj.tagName == 'TD' && oSourceObj.getAttribute("fld") != null))
                    && !((oSourceObj.tagName == 'TH' && oSourceObj.getAttribute("fld") != null))
                )

            ) {
                oSourceObj = oSourceObj.parentNode || oSourceObj.parentElement;
            }
        }
        catch (ee) {
            return;
        }

        if (oSourceObj) {
            showttid(oSourceObj, e);
        }
    }
    catch (eee) {
        // pas de crash erreur sur les fonctions utilitaires de debuggage
        return;

    }
}


function fldAction(e, strTrigger) {

    // Récupération de l'évènement
    if (!e)
        var e = window.event;

    var browser = new getBrowser();

    var srcElem = e.target || e.srcElement;
    var isFromBingAutoSuggestResultElement = false;

    // L'autocompletion sur l'element actif si activé
    if (typeof (oAutoCompletion) != "undefined") {
        // #66 489 : lorsqu'on clique sur un résultat issu de la recherche prédictive Bing Maps, l'évènement clic est d'abord propagé sur l'élément déclencheur 
        // (le champ de saisie) puis sur le résultat de recherche (onSelectSuggestion), et donc, la validation du contenu du champ se fait, alors qu'on ne souhaite
        // pas en tenir compte dans ce cas (on veut prendre en compte la valeur cliquée dans le résultat de recherche et non les termes de recherche)
        // On identifie donc ce cas en vérifiant si l'élément à l'origine du clic est un enfant de la structure DOM créée par Bing Maps, et on utilisera le résultat
        // pour court-circuiter tout traitement en conflit plus bas, tel que la validation du champ
        isFromBingAutoSuggestResultElement = oAutoCompletion.isBingAutoSuggestResultElement(srcElem);

        if (oAutoCompletion.enabled(srcElem))
            oAutoCompletion.search(e, strTrigger);
    }

    // Si appui sur une touche autre qu'Entrée et Tabulation: évènement ignoré

    if (strTrigger == "KEYUP" && e.keyCode != eTools.keyCode.TAB && e.keyCode != eTools.keyCode.ENTER) {
        return;
    }

    // CRU/MAB - #61111 et #62114 : Pour toute autre action que SELECT, on désélectionne le contenu pour ne pas redéclencher l'événement
    // SELECT derrière, et éviter des pertes de données liées au déclenchement simultané. cf. descriptif détaillé de ces 2 demandes
    if (strTrigger != "SELECT") {
        // https://stackoverflow.com/questions/3169786/clear-text-selection-with-javascript

        var objSelectedTextAndContainer = getSelectionTextAndContainerElement();
        var eltFirstSelected = getSelectionBoundaryElement(true);
        var eltLastSelected = getSelectionBoundaryElement(false);
        var srcElemContainer = srcElem.parentNode || srcElem.parentElement;

        // On vérifie si l'élément dans lequel se trouve la sélection est le même que l'objet cliqué ou son conteneur direct
        // On analyse 3 pistes : le conteneur, le premier élément sélectionné, et le dernier élément sélectionné
        // (en sachant que si la sélection se fait de droite à gauche, le premier élément sélectionné sera celui le plus à droite/en bas)
        // cf. descriptif des fonctions getSelectionTextAndContainerElement() et getSelectionBoundaryElement() dans eTools.js
        if (
            (objSelectedTextAndContainer && objSelectedTextAndContainer.containerElement != srcElem) &&
            (eltFirstSelected && eltFirstSelected != srcElem) &&
            (eltLastSelected && eltLastSelected != srcElem) &&
            (objSelectedTextAndContainer && objSelectedTextAndContainer.containerElement != srcElemContainer) &&
            (eltFirstSelected && eltFirstSelected != srcElemContainer) &&
            (eltLastSelected && eltLastSelected != srcElemContainer)
        ) {
            //if (!hasClass(e.target, 'cke_editable')) {
            if (window.getSelection) {

                var selection = window.getSelection();

                if (selection.toString().length > 0) {
                    if (selection.empty) {  // Chrome
                        // ALISTER Demande / Requests #86 298 => (En rajoutant strTrigger != "LCLICK", la situation est réglée /
                        // By adding strTrigger != "LCLICK" the situation seems fixed)
                        if (strTrigger != "LCLICK") {
                            selection.empty();
                        }
                    } else if (selection.removeAllRanges) {  // Firefox
                        selection.removeAllRanges();
                    }
                }
            }
        }

    }

    // Objet source
    var oSourceObj = e.target || e.srcElement;
    var oSourceObjOrig = oSourceObj;
    var topelement = "TABLE";
    try {
        while (
            oSourceObj.tagName != topelement
            && !((oSourceObj.tagName == 'TD' || oSourceObj.tagName == 'INPUT' || oSourceObj.tagName == 'DIV') && oSourceObj.getAttribute("efld") == "1")
        ) {
            oSourceObj = oSourceObj.parentNode || oSourceObj.parentElement;
        }
    }
    catch (ee) {
        return;
    }


    //SPH : ajout de test pour ne valider que les inlineeditor ouvert et différent de celui sur lequel le click a été fait
    // sinon, le validate se déclenche lors de click successif sur me même field editor
    // ajout de validateLaunch pour vérifier que le validate n'a pas été lancé pour éviter le double validate par le blur
    if (
        ePopupObject
        && ePopupObject.childElement
        && ePopupObject.childElement.type == "inlineEditor"
        && ePopupObject.childElement.validateLaunch == false
        && typeof (ePopupObject.childElement.validate) == "function") {

        //Si click successif, on ne doit pas valider le champ
        if (oSourceObj == ePopupObject.sourceElement) {
            return;
        }

        // #66 489 : lorsqu'on clique sur un résultat issu de la recherche prédictive Bing Maps, l'évènement clic est d'abord propagé sur l'élément déclencheur 
        // (le champ de saisie) puis sur le résultat de recherche (onSelectSuggestion), et donc, la validation du contenu du champ se fait, alors qu'on ne souhaite
        // pas en tenir compte dans ce cas (on veut prendre en compte la valeur cliquée dans le résultat de recherche et non les termes de recherche)
        // On identifie donc ce cas en vérifiant si l'élément à l'origine du clic est un enfant de la structure DOM créée par Bing Maps, et on utilisera le résultat
        // pour court-circuiter tout traitement en conflit plus bas, tel que la validation du champ
        if (isFromBingAutoSuggestResultElement)
            return;

        ePopupObject.childElement.validate();
    }

    // Met en surbrillance la ligne s'il y a eu un event sur un de ses rubrique    
    highlightLineIfFromList(srcElem);

    // Ce n'est pas un champ
    // ce champ n'a pas d'action affectee
    var sAction = oSourceObj.getAttribute("eaction");

    //SPH : ajout de test pour ne valider que les inlineeditor ouvert et différent de celui sur lequel le click a été fait
    // sinon, le validate se déclenche lors de click successif sur me même field editor
    // ajout de validateLaunch pour vérifier que le validate n'a pas été lancé pour éviter le double validate par le blur
    if (oSourceObj.getAttribute("efld") != "1" || sAction == "") {
        if (
            ePopupObject
            && ePopupObject.childElement
            && ePopupObject.childElement.validateLaunch == false
            && ePopupObject.childElement.type == "inlineEditor"
            && typeof (ePopupObject.childElement.validate) == "function") {

            ePopupObject.childElement.validate();
        }

        return;
    }




    // Cas particulier : sélection d'un champ de liaison au clavier
    // On ne redirige pas vers la fiche dans ce cas, mais on affiche le catalogue de champ de liaison
    if (strTrigger == "KEYUP" && sAction == "LNKGOFILE")
        sAction = "LNKCATFILE";
    // On vérifie si l'action affectée est prise en charge en fonction du déclencheur (ex : appui sur Entrée)
    var bEnableAction = false;
    if (strTrigger == "KEYUP") {
        switch (sAction) {
            case 'LNKCATUSER':  // catalogue user
            case 'LNKCATFILE': // Choix fiche
            case 'LNKADVCAT':   // Catalogue avancée
            case 'LNKCAT':  //catalogue simple
            case 'LNKCATDESC':   // Catalogue DESC
            case 'LNKCATENUM':   // Catalogue ENUM
            case 'LNKFREECAT':  //catalogue saisie libre
            case 'LNKPHONE': // Champ téléphone
            case 'LNKWEBSIT':   // Catalogue Site Web
            case 'LNKSOCNET':   // Catalogue Reseau Social
            case 'LNKMAIL': // Catalogue Adresse Mail
            case 'LNKFREETEXT': // Champ saisie libre
            case 'LNKNUM': // Edition champ numérique
                //KHA Attention : dans le cas d'un champ simple en mode fiche l'appui de la touche entrée ne doit pas entrainer d'action
                bEnableAction = !(oSourceObj.tagName == 'INPUT' && e.keyCode == 13);
                break;
            default:
                bEnableAction = false;
                break;
        }
    }
    else if (strTrigger == "SELECT") {

        switch (sAction) {
            case 'LNKFREECAT':  //catalogue saisie libre
            case 'LNKPHONE': // Champ téléphone
            case 'LNKWEBSIT':   // Catalogue Site Web
            case 'LNKSOCNET':   // Catalogue Reseau Social
            case 'LNKMAIL': // Catalogue Adresse Mail
            case 'LNKFREETEXT': // Champ saisie libre
            case 'LNKNUM': // Edition champ numérique
                // #61111 : Action que si quelque chose est sélectionné
                // #61960 : ne fonctionne pas sur IE
                if (browser.isIE && oSourceObj.selectionEnd - oSourceObj.selectionStart > 0)
                    bEnableAction = true;
                else if (window.getSelection && window.getSelection().toString().length == 0) {
                    bEnableAction = false;
                }
                else
                    bEnableAction = true;
                break;
            default:
                bEnableAction = false;
                break;
        }
    }
    else
        bEnableAction = true;

    if (!bEnableAction)
        return;

    // Action programmée sur le click : annule toutes les autres actions
    stopEvent(e);

    // si l'élement cliqué contient un attribut eActionTarget l'objet à editer n'est pas l'élément cliquable
    if (oSourceObj.getAttribute("eActTg") != null && oSourceObj.ownerDocument.getElementById(oSourceObj.getAttribute("eActTg")) != null)
        oSourceObj = oSourceObj.ownerDocument.getElementById(oSourceObj.getAttribute("eActTg"));
    try {
        //Forcer la fermeture de la vcard si elle existe
        if (vCardModal && vCardModal.hide) {
            clearTimeout(showVcardTimer);
            vCardModal.hide();
        }
    }
    catch (eVC) {
    }

    // #55378 : On masque l'infobulle si elle existe
    ht();

    setFldEditor(oSourceObj, oSourceObjOrig, sAction, strTrigger, e);
}

var modalFileViewer = null;
function setFldEditor(oSourceObj, oSourceObjOrig, sAction, strTrigger, e) {
    if (!strTrigger || strTrigger == "")
        strTrigger = "LCLICK";
    if (!sAction || sAction == "")
        sAction = getAttributeValue(oSourceObj, "eaction");

    var browser = new getBrowser();


    switch (sAction) {
        case 'LNKCATUSER':
            /*#region catalogue utilisateur*/
            if (strTrigger != 'KEYUP' || (strTrigger == "KEYUP" && e.keyCode == 13)) {

                if (!browser.isIE || !IsFieldEditing()) {

                    //[MOU- 23/08/2013 cf.24569] CAS SPECIAL:
                    //Si on clique sur le catalogue utilisateur simple de la fenetre proprieté de la fiche
                    //on ouvre directement la fentere du ctalogue sinon on affiche les MRU
                    if (oSourceObj.ownerDocument.getElementById("filePropContainer") && strTrigger != "LASTVALUE_CLICK")
                        eCatalogUserEditorObject.OpenUserDialog(oSourceObj);
                    else
                        eCatalogUserEditorObject.onClick(oSourceObj, null, strTrigger);

                } else {
                    //[MOU- 23/08/2013 cf.24569] CAS SPECIAL: Pareil pour IE
                    if (oSourceObj.ownerDocument.getElementById("filePropContainer") && strTrigger != "LASTVALUE_CLICK")
                        //Sur IE9 et IE10 le onclick et fait avant le onblur du champ précédent avec la tabulation donc on fait un set time out pour palier à cela.
                        setTimeout(function () { eCatalogUserEditorObject.OpenUserDialog(oSourceObj); }, 1000);
                    else
                        //Sur IE9 et IE10 le onclick et fait avant le onblur du champ précédent avec la tabulation donc on fait un set time out pour palier à cela.
                        setTimeout(function () { eCatalogUserEditorObject.onClick(oSourceObj, null, strTrigger); }, 1000);
                }

            }

            //#endregion
            break;

        case 'LNKGEO':
            if (strTrigger != 'KEYUP' || (strTrigger == "KEYUP" && e.keyCode == 13)) {
                if (!browser.isIE || !IsFieldEditing())
                    /*#region Edition d'un champ saisie libre*/
                    eGeolocEditorObject.openGeolocDialog(oSourceObj);
                else
                    setTimeout(function () { eGeolocEditorObject.openGeolocDialog(oSourceObj); }, 1000);
            }
            break;

        case 'LNKOPENMRG': // fusion de fiches
            var atrId = oSourceObj.parentNode.getAttribute('eid').split('_');
            var nBkmFileId = atrId[1];
            var nParentFileId = getAttributeValue(document.getElementById("fileDiv_" + nGlobalActiveTab), "fid")

            setFldMergeFile(nParentFileId, nBkmFileId);

            break;
        case 'LNKOPENFILEINBKM':
            var atrId = oSourceObj.parentNode.getAttribute('eid').split('_');
            var nBkm = atrId[0];
            var nBkmFileId = atrId[1];

            chgBkmViewMode(nGlobalActiveTab, nBkm, 1, nBkmFileId)
            break;
        case 'LNKOPENRGPDLOGFILE':
        case 'LNKOPENPOPUP':

            /*#region Ouverture d'un pop up depuis le bouton en début de ligne d'une liste (pour afficher un template par exemple)*/

            //var atrId = oSourceObj.parentNode.getAttribute('eid').split('_');
            var tab = getAttributeValue(oSourceObj, "ename").replace("HEAD_ICON_COL_", "");

            if (sAction == 'LNKOPENRGPDLOGFILE')
                tab = TAB_RGPDTREATMENTLOG;

            var fileid = getAttributeValue(oSourceObj, "lnkid");

            var strTitre = "";


            if (document.getElementById('SpanLibElem'))
                strTitre = document.getElementById('SpanLibElem').innerHTML;
            else if (document.getElementById('bkmLabel_' + tab)) {

                strTitre = getAttributeValue(document.getElementById('bkmLabel_' + tab), "bkmtitle");

                if (strTitre == "")
                    strTitre = document.getElementById('bkmLabel_' + tab).innerHTML;
                else
                    //CNA - Demandes #45265/#46175 - Pour afficher les accents correctement dans le titre de la popup
                    strTitre = decodeHTMLEntities(strTitre);
            }

            var lWidth = null;
            var lHeight = null;

            var nMailMode = MAIL_MODE_NO
            // Si le clic porte sur un fichier de type E-mail, on l'ouvre en mode Lecture seule/consultation
            if (document.getElementById('HEAD_' + tab)) {
                for (var i = 0; i < document.getElementById('HEAD_' + tab).childNodes.length; i++) {
                    if (document.getElementById('HEAD_' + tab).childNodes[i].getAttribute && document.getElementById('HEAD_' + tab).childNodes[i].getAttribute("tabtyp") == "3") {
                        i = document.getElementById('HEAD_' + tab).childNodes.length;
                        // Sauf s'il s'agit d'un mail de type Brouillon, auquel cas, on l'ouvre en édition (nMailMode = 3)
                        // #55002 : Ou bien un mail qui n'a pu être envoyé
                        if (oSourceObj.getAttribute("draft") == "1" || oSourceObj.getAttribute("notsent") == "1") {
                            nMailMode = MAIL_MODE_DRAFT;
                        }

                        // Sinon, ouverture en mode consultation (nMailMode = 1)
                        else {
                            nMailMode = MAIL_MODE_STD;
                            var oTabWH = getWindowWH(top);
                            lWidth = oTabWH[0] - 25;
                            lHeight = oTabWH[1] - 100;
                        }
                    }
                }
            }

            //Si la table est celle des doublons, on passe l'id de la table principale
            if (tab + "" == "2") {
                tab = nGlobalActiveTab;
            }
            else if (tab + "" == TAB_PJ || tab + "" == "COL_102000_102012") {
                // ajout du test sur COL_102000_102012 -  sph/mcr - 08/04/2015  #38147 - le ename dans le cas des listes de pj est COL_102000_102012 pas forcément  102000

                // sur la table des pj, on doit chercher la table sur la ligne de la pj
                tab = oSourceObj.getAttribute("lnkdid");
                fileid = oSourceObj.getAttribute("lnkid");
                var pjTabType = oSourceObj.getAttribute("pjtabtyp");

                if (pjTabType == 3) {
                    nMailMode = MAIL_MODE_STD;
                }

                strTitre = "";
            }


            //TODO : ADD CALLFROM
            //TODO : renseigner autres paramètres selon contexte
            var mainFieldValue = null;
            var bApplyCloseOnly = null;
            var afterValidate = null;
            var nCallFrom = null;
            var sOrigFrameId = null;
            var origElt = null;
            var callBack = null;
            // #50 353 : Administration des utilisateurs : appel d'une fonction spécifique dans eAdminUsers.js pour paramétrer certains comportements de la fenêtre après initialisation
            if (tab == "101000")
                callBack = function (oModFile) { if (typeof (nsAdminUsers) != "undefined") { nsAdminUsers.InitDefault(oModFile); } };

            shFileInPopup(tab, fileid, strTitre, lWidth, lHeight, nMailMode, mainFieldValue, bApplyCloseOnly, afterValidate, nCallFrom, sOrigFrameId, null, callBack)
            /*#endregion*/

            break;
        case 'LNKOPENCALPUP':
            /*#region Ouverture d'un pop up de template Planning depuis le bouton en début de ligne d'une liste*/
            var atrId = oSourceObj.parentNode.getAttribute('eid').split('_');
            var tab = atrId[0];
            var fileid = atrId[1];

            var strTitre = "";
            if (document.getElementById('SpanLibElem'))
                strTitre = document.getElementById('SpanLibElem').innerHTML;
            else if (document.getElementById('bkmLabel_' + tab)) {
                strTitre = getAttributeValue(document.getElementById('bkmLabel_' + tab), "bkmtitle");
            }

            if (tab + "" == TAB_PJ) {
                // sur la table des pj, on doit chercher la table sur la ligne de la pj
                tab = oSourceObj.getAttribute("lnkdid");
                fileid = oSourceObj.getAttribute("lnkid");
                strTitre = "";
            }

            // S'il s'agit d'une série de fiches, on demande si on ouvre une occurence ou la série
            var scheduleId = getAttributeValue(oSourceObj, "data-sid");
            if (scheduleId && scheduleId != "0")
                selectOpenSeries(tab, fileid, top._res_151);
            else
                showTplPlanning(tab, fileid, null, strTitre);
            /*#endregion*/

            break;
        case 'LNKOPENPJPTIES': // Annexe - PJ
        case 'LNKVIEWPJ': // Annexe - PJ - Visualisation
            var nPJId = getNumber(oSourceObj.getAttribute("lnkid"));
            if (nPJId > 0) {
                var atrId = oSourceObj.parentNode.getAttribute('eid').split('_');
                var nTab = getTabDescid(atrId[0]);
                var nFileId = GetTabFileId(nTab);

                var bReadOnly = "0";
                var bSupp = "1";

                if ((nTab == TAB_PJ) || (nTab % 100 > 0)) {
                    nTab = getNumber(getAttributeValue(document.getElementById("nTab"), "value"));
                    nFileId = getNumber(getAttributeValue(document.getElementById("nFileID"), "value"));
                }

                //KHA le 16/06/2015 on a trouvé comment le fournir
                //En mode liste d'annexe le fileid et manquant
                //if (isNaN(nFileId) || (nFileId <= 0)) {
                //    bReadOnly = 1;
                //}

                if ((nTab != TAB_PJ)) {
                    var bkmPj = document.getElementById("div" + (nTab + ATTACHMENT));
                    if (!bkmPj)
                        bkmPj = document.getElementById("div" + TAB_PJ);
                    if (getAttributeValue(bkmPj, "ro") == "1") {
                        bReadOnly = 1;
                    }
                    if (getAttributeValue(bkmPj, "sup") == "0") {
                        bSupp = "0";
                    }
                    else {
                        bSupp = "1";
                    }
                    var oTR = findUp(oSourceObj, "TR");
                    var nTrTab = getNumber(getAttributeValue(oTR, "tab"));
                    var nTrFid = getNumber(getAttributeValue(oTR, "fid"));
                    if (nTrTab > 0 && nTrFid >= 0 && isNaN(nTab) && isNaN(nFileId)) {
                        nTab = nTrTab;
                        nFileId = nTrFid;
                    }
                    else if (nTrTab > 0 && nTrFid >= 0 && nTab != nTrTab && nFileId != nTrFid) {
                        bReadOnly = "1";
                        bSupp = "0";
                    }
                }

                if (sAction == 'LNKVIEWPJ') {
                    var sFileUrl = SetFldPj(oSourceObj, true);
                    if (!sFileUrl || sFileUrl == "")
                        return;

                    var browser = new getBrowser();
                    top.modalFileViewer = new eModalDialog(top._res_1229, 0, sFileUrl + "&useviewer=1", 960, 540);
                    top.modalFileViewer.addParam("objectID", oSourceObj.id, "post");
                    top.modalFileViewer.addParam("isIE8", (browser.isIE && browser.isIE8) ? "1" : "0", "post");
                    top.modalFileViewer.show();
                    top.modalFileViewer.addButton(top._res_30, function () { top.modalFileViewer.hide(); }, "button-gray"); // fermer
                }
                else {
                    var strTitre = top._res_5042;


                    if (document.getElementById('SpanLibElem'))
                        strTitre = document.getElementById('SpanLibElem').innerHTML;
                    else if (document.getElementById('bkmLabel_' + tab)) {

                        strTitre = getAttributeValue(document.getElementById('bkmLabel_' + tab), "bkmtitle");

                        if (strTitre == "")
                            strTitre = document.getElementById('bkmLabel_' + tab).innerHTML;
                        else
                            //CNA - Demandes #45265/#46175 - Pour afficher les accents correctement dans le titre de la popup
                            strTitre = decodeHTMLEntities(strTitre);
                    }

                    shFileInPopup(TAB_PJ, nPJId, strTitre);


                    //var modPjPties = new eModalDialog(top._res_1108, 0, "ePJPties.aspx", 550, 450);

                    //modPjPties.addParam("pjid", nPJId, "post");
                    //modPjPties.addParam("ro", bReadOnly, "post");
                    //modPjPties.addParam("sup", bSupp, "post");
                    //modPjPties.show();
                    //modPjPties.onIframeLoadComplete = function () { AddPjPtiesBtns(modPjPties, nTab, nFileId, nPJId); };
                }
            }

            break;

        case "LNKGOUSERFILE":

            nsAdmin.loadAdminModule(USROPT_MODULE_ADMIN_TAB_USER, null, { tab: 101000, userid: getAttributeValue(oSourceObj, "lnkid") });

            break;
        case "LNKGOHOMEFILE":
            var nFileId = getAttributeValue(oSourceObj, "lnkid");
            if (typeof (oGridController) != "undefined")
                oGridController.page.click(nFileId, oSourceObjOrig);

            break;

        case 'LNKGOFILE': // Mode fiche 
            //Condition pour gérer le problème d'ajout de fiche, lorsque
            //l'on a fait une dissociation avant
            var fileid = getAttributeValue(oSourceObj, "lnkid");
            if (fileid == "") {
                oSourceObj.setAttribute("efld", 1);
                // Si la rubrique n'est pas editable alors le eaction d'une islink est vide
                oSourceObj.setAttribute("eaction", "LNKCATFILE");

                removeClass(oSourceObj, "gofile");
                removeClass(oSourceObj, "LNKGOFILE");
                addClass(oSourceObj, "LNKCATFILE");
                addClass(oSourceObj, "edit")
                addClass(oSourceObj, "readonly")

                nodeAction = getAttributeValue(oSourceObj, 'eaction');
                sAction = oSourceObj.getAttribute("eaction");
            }
            SetFldGoFile(oSourceObj);
            break;
        case 'LNKCATFILE':

            //#39838 Quand on ajoute une nouvelle liaison a cible etendu, on avertit l'utilisateur d'ecrasement des données saisies
            var bProspectEnabled = getAttributeValue(oSourceObj, "prospect") == "1";
            if (bProspectEnabled) {
                var id = getNumber(getAttributeValue(oSourceObj, "lnkid"));
                var bLinked = !isNaN(id) && id > 0;

                //Liaison pas encore définie
                if (!bLinked) {
                    eConfirm(1, top._res_201, top._res_6797, top._res_6741, 500, 200, function () { linkCatFile({ 'trigger': strTrigger, 'source': oSourceObj, 'evt': e }); }, function () { });
                    break;
                }
            }

            linkCatFile({ 'trigger': strTrigger, 'source': oSourceObj, 'evt': e });

            /*#endregion*/
            break;
        case 'LNKCATDESC':   // Catalogue DESC
        case 'LNKCATENUM':   // Catalogue ENUM
        case 'LNKADVCAT':   // Catalogue avancée
        case 'LNKCAT':  //catalogue simple
            /*#region Catalogue avancée, Catalogue texte libre, catalogue simple*/
            // Si appui sur Entrée : on affiche la popup d'édition (idem clic sur le bouton)
            if (strTrigger != 'KEYUP' || (strTrigger == "KEYUP" && e.keyCode == 13)) {
                if (!browser.isIE || !IsFieldEditing())
                    /*#region Edition d'un champ saisie libre*/
                    eCatalogEditorObject.onClick(oSourceObj, null, strTrigger);
                else
                    setTimeout(function () { eCatalogEditorObject.onClick(oSourceObj, null, strTrigger); }, 1000); //Sur IE9 et IE10 le onclick et fait avant le onblur du champ précédent avec la tabulation donc on fait un set time out pour palier à cela.
            }
            break;
        case 'LNKSTEPCAT': // Catalogue étape
            if (oSourceObjOrig) {
                if (oSourceObjOrig.tagName == "A" && !hasClass(oSourceObjOrig.parentElement, "disabledValue")) {
                    if (getAttributeValue(oSourceObj, "ero") != "1") {
                        eStepCatalogEditorObject.onClick(oSourceObj);
                        eStepCatalogEditorObject.validate(false, strTrigger == 'LASTVALUE_CLICK');
                    }
                }
            }

            break;
        case 'LNKFREECAT': // Catalogue saisie libre
            // Pour ce cas particulier (qui peut être édité aussi bien en popup qu'en saisie libre),
            // on indique quelle action a déclenché la fonction pour savoir comment gérer la validation
            // (en mode Texte libre ou catalogue)            
            if (!browser.isIE || !IsFieldEditing()) {
                /*#region Edition d'un champ saisie libre*/
                eCatalogEditorObject.onClick(oSourceObj, oSourceObjOrig, strTrigger, e);
            }
            else
                setTimeout(function () { eCatalogEditorObject.onClick(oSourceObj, oSourceObjOrig, strTrigger, e); }, 1000); //Sur IE9 et IE10 le onclick et fait avant le onblur du champ précédent avec la tabulation donc on fait un set time out pour palier à cela.
            /*#endregion*/
            break;
        /*#endregion*/
        case 'LNKDATE':   // Catalogue date
            /*#region Edition d'un champ date*/
            if (!browser.isIE || !IsFieldEditing())
                eDateEditorObject.onClick(oSourceObj);
            else
                setTimeout(function () { eDateEditorObject.onClick(oSourceObj); }, 1000); //Sur IE9 et IE10 le onclick et fait avant le onblur du champ précédent avec la tabulation donc on fait un set time out pour palier à cela.
            /*#endregion*/
            break;
        case 'LNKMAIL': // Catalogue Adresse Mail
            var curView = getCurrentView(document);
            //Email prefictif uniquement en creation et modification sinon cas inline editor classique
            if ((curView == 'FILE_CREATION' || curView == 'FILE_MODIFICATION') && oSourceObj.tagName == 'INPUT' && !oSourceObj.readOnly) {
                if (!browser.isIE || !IsFieldEditing()) {
                    eMailEditorObject.onClick(oSourceObj);
                }
                else
                    setTimeout(function () { eMailEditorObject.onClick(oSourceObj); }, 1000); //Sur IE9 et IE10 le onclick et fait avant le onblur du champ précédent avec la tabulation donc on fait un set time out pour palier à cela.
            } else {
                if (!browser.isIE || !IsFieldEditing()) {
                    eInlineEditorObject.onClick(oSourceObj);
                }
                else
                    setTimeout(function () { eInlineEditorObject.onClick(oSourceObj); }, 1000); //Sur IE9 et IE10 le onclick et fait avant le onblur du champ précédent avec la tabulation donc on fait un set time out pour palier à cela.
            }
            break;
        case 'LNKPHONE': // Champ téléphone
        case 'LNKWEBSIT':   // Catalogue Site Web        
        case 'LNKSOCNET':   // Catalogue Reseau Social        
        case 'LNKFREETEXT': // Champ saisie libre
        case 'LNKNUM': // Edition champ numérique
        case 'LNKOPENMAIL': //Ouverture client mail ou edition
        case 'LNKGEO': //Ouverture client mail ou edition
            /*#region Edition d'un champ saisie libre*/
            if (!browser.isIE || !IsFieldEditing()) {
                eInlineEditorObject.onClick(oSourceObj);
            }
            else
                setTimeout(function () { eInlineEditorObject.onClick(oSourceObj); }, 1000); //Sur IE9 et IE10 le onclick et fait avant le onblur du champ précédent avec la tabulation donc on fait un set time out pour palier à cela.
            /*#endregion*/

            break;
        case 'LNKCHECK':    // Case à cocher
            /*#region Coche/Décoche d'une case à cocher XRM*/
            //Navigation clavier le onclick s'execute sur le A et le clique de la souris sur le IMG (le princupal étant A on prendra toujours A)
            if (oSourceObjOrig.tagName == "IMG" || oSourceObjOrig.tagName == "SPAN")
                oSourceObjOrig = oSourceObjOrig.parentNode;

            //  Récupération de l'état de la checkbox
            if (oSourceObjOrig && oSourceObjOrig.tagName == "A" && (oSourceObjOrig.getAttribute("chk"))) {

                // on switch coché/pas coché
                //chgChk(oSourceObjOrig); attente dblval

                eCheckBoxObject.onClick(oSourceObj);
                var nChecked = oSourceObjOrig.getAttribute("chk");
                eCheckBoxObject.validate();
            }
            /*#endregion*/
            break;
        case 'LNKBITBUTTON':
            //  Récupération de l'état de la checkbox
            if (oSourceObjOrig && oSourceObjOrig.tagName == "A" && (oSourceObjOrig.getAttribute("chk"))) {

                eBitButtonObject.onClick(oSourceObj);
                var nChecked = oSourceObjOrig.getAttribute("chk");
                eBitButtonObject.validate();
            }
            break;
        case 'LNKOPENSOCNET':   // Ouverture lien Reseau Social            
            var strRootUrl;
            if (getAttributeValue(oSourceObj, "eRootUrl") != "")
                strRootUrl = getAttributeValue(oSourceObj, "eRootUrl");

            var strUrl;
            if (getAttributeValue(oSourceObj, "dbv") != "")
                strUrl = getAttributeValue(oSourceObj, "dbv");
            else if (oSourceObj.tagName == "INPUT")
                strUrl = oSourceObj.value;
            else if ((oSourceObj.tagName == "TD") && (GetText(oSourceObj) != "")) {
                strUrl = GetText(oSourceObj)
            }
            openUrlInNewTab(getSocialNetworkUrl(strUrl, strRootUrl));
            break;
        case 'LNKOPENWEB':   // Ouverture Site Web
            /*#region Ouverture champ de type Site Web*/
            var strUrl;
            if (getAttributeValue(oSourceObj, "dbv") != "")
                strUrl = getAttributeValue(oSourceObj, "dbv");
            else if (oSourceObj.tagName == "INPUT")
                strUrl = oSourceObj.value;
            else if ((oSourceObj.tagName == "TD") && (GetText(oSourceObj) != "")) {
                strUrl = GetText(oSourceObj)
            }
            openUrlInNewTab(strUrl);
            /*if (
                strUrl.indexOf("http://") != 0 &&
                strUrl.indexOf("https://") != 0 &&
                strUrl.indexOf("ftp://") != 0 &&
                strUrl.indexOf("ftps://") != 0 &&
                strUrl.indexOf("sftp://") != 0
            )
                strUrl = "http://" + strUrl;
            window.open(strUrl);*/
            /*#endregion*/
            break;
        case 'LNKOPENWEBSPECIF':   // Ouverture Site Web
            /*#region Ouverture champ de type Site Web*/

            var sSpecifURL = '';
            if (getAttributeValue(oSourceObj, "dbv") != "")
                sSpecifURL = getAttributeValue(oSourceObj, "dbv");
            else if (oSourceObj.tagName == "INPUT")
                sSpecifURL = oSourceObj.value;
            else if ((oSourceObj.tagName == "TD") && (GetText(oSourceObj) != ""))
                sSpecifURL = GetText(oSourceObj);

            if (sSpecifURL != '') {
                exportToLinkToV7(sSpecifURL, null, 5);
            }

            /*#endregion*/
            break;
        case 'LNKSENDMAIL': // ouvre une fiche email prérenseignée.
        case 'LNKSENDPHONE': // ouvre une fiche SMS prérenseignée (ou autre action liée à un numéro de téléphone.
            /*#region ouvre une fiche email/SMS prérenseignée*/
            var sMail = "";
            if (getAttributeValue(oSourceObj, "dbv") != "")
                sMail = getAttributeValue(oSourceObj, "dbv");
            else if (oSourceObj.tagName == "INPUT")
                sMail = oSourceObj.value;
            else if ((oSourceObj.tagName == "TD" || oSourceObj.tagName == "DIV") && (GetText(oSourceObj) != ""))
                sMail = GetText(oSourceObj);
            sMail = sMail.trim();

            var fldEngine = getFldEngFromElt(oSourceObj);
            var parentFileId = GetFieldFileId(oSourceObj.id);
            var parentTab = getTabDescid(fldEngine.descId);
            var objParentInfo = { parentFileId: parentFileId, parentTab: parentTab };

            if (sAction == "LNKSENDPHONE")
                selectFileMail(getParamWindow().document.getElementById("SMSFiles"), sMail, objParentInfo, TypeMailing.SMS_MAILING_UNDEFINED);
            else
                selectFileMail(getParamWindow().document.getElementById("MLFiles"), sMail, objParentInfo, TypeMailing.MAILING_UNDEFINED);

            /*#endregion*/
            break;
        case 'LNKOPENFILE': // Ouvre fichier
            var sFolder = getAttributeValue(oSourceObj, "pdbv");
            var sFileName = getAttributeValue(oSourceObj, "dbv");
            window.open("ePjDisplay.aspx?folder=" + sFolder + "&file=" + sFileName + "&dispFrom=fieldfiles");
            break;
        case 'LNKMNGFILE': // Choix fichier
            eFldFileEditorObject.onClick(oSourceObj, oSourceObjOrig, strTrigger, e);
            break;
        case 'LNKSEP': //Séparateur de page    
            /*#region Pli/dépli un Séparateur de page*/
            OpenCloseSep(oSourceObj);
            /*#endregion*/
            break;
        case 'LNKSEPALL': //Séparateur de page gloabal
            OpenCloseSepAll(oSourceObj);
            break;
        case 'LNKOPENUSERAVATAR':
        case 'LNKOPENAVATAR': // ouvre image
            var oImg = oSourceObj;
            oImg = findUp(oSourceObj, "TD");
            doGetImage(oImg, (sAction == "LNKOPENAVATAR") ? 'AVATAR_FIELD' : 'USER_AVATAR_FIELD');
            break;
        case 'LNKOPENIMG': // ouvre image
            /*#region ouvre une image*/
            doGetImage(oSourceObj, 'IMAGE_FIELD');
            /*#endregion*/
            break;
        case 'LNKCATIMG':  // choix image
            /*#region Séléction d'une image*/
            doGetImage(oSourceObj, 'IMAGE_FIELD');
            /*#endregion*/
            break;
        case 'LNKOPENMEMO': // mémo en mode Zoom/fenêtré (clic depuis mode Liste)
            /*#region mémo en mode Zoom/fenêtré (clic depuis mode Liste)*/
            eMemoEditorObject.onClick(oSourceObj);
            /*#endregion*/
            break;
        case 'LNKOPENMEMOPOPUP': // mémo en mode Zoom/fenêtré (bouton à droite des champs Mémo)
            /*#region mémo en mode Zoom/fenêtré (bouton à droite des champs Mémo)*/
            if (nsMain.hasMemoEditors()) {
                if (nsMain.getMemoEditor('edt' + getAttributeValue(oSourceObj, "ctrl"))) {
                    nsMain.getMemoEditor('edt' + getAttributeValue(oSourceObj, "ctrl")).isBeingZoomed = true;
                    nsMain.getMemoEditor('edt' + getAttributeValue(oSourceObj, "ctrl")).switchFullScreen(true);
                }
                else if (nsMain.getMemoEditor('edt' + oSourceObj.id)) {
                    nsMain.getMemoEditor('edt' + oSourceObj.id).switchFullScreen(true);
                }
            }
            /*#endregion*/
            break;
        case 'LNKOPENPJ': // pj

            SetFldPj(oSourceObj);
            break;
        case 'LNKSELPJ': // pj

            SetFldPj(oSourceObj, true);
            break;
        case 'LNKOPENPJDIALOG': // pj
            showPJFromList(oSourceObj);
            break;
        case 'LNKOPENCHART':
            var reportid = getAttributeValue(oSourceObj, "data-reportid");
            if (reportid != "")
                displayChart(reportid);
            break;
        default:
            break;
    }
}


///Lance l'action de liaison
//args : {'trigger':strTrigger, 'source' : oSourceObj, 'evt' : e }
function linkCatFile(args) {

    /*#region Champ de liaison : Sélection de la fiche liée*/
    // Si clic ou appui sur Entrée : on affiche la popup d'édition (idem clic sur le bouton)
    if (args.trigger != 'KEYUP' || (args.trigger == "KEYUP" && args.evt.keyCode == 13)) {

        var browser = new getBrowser();

        if (!browser.isIE || !IsFieldEditing())
            /*#region Edition d'un champ saisie libre*/
            eLinkCatFileEditorObject.onClick(args.source, null, args.trigger);
        else
            setTimeout(function () { eLinkCatFileEditorObject.onClick(args.source); }, 1000); //Sur IE9 et IE10 le onclick et fait avant le onblur du champ précédent avec la tabulation donc on fait un set time out pour palier à cela.
    }
}


// Met en surbrillance la ligne editée...
var oldLineObj = null; //ancienne ligne sélectionnée
var oldClassName = "";  //line1 ou line2
function highlightLineIfFromList(currentObj) {
    //on recupère la tr de la table
    var currentLine = findUp(currentObj, 'TR'); //eTools.js

    //on ne fait rien si on clique sur la meme ligne ou le champs n'est pas affiché en mode liste
    if (currentLine == null || currentLine == oldLineObj || getAttributeValue(currentLine, "eid").length == 0) //eTools.js
        return;

    var className = currentLine.className;

    //on retire la css de surbrillance de l'ancienne ligne en l'affectant à la nouvelle
    switchClass(oldLineObj, " highlight", oldClassName); //eTools.js
    switchClass(currentLine, className, className + " highlight");

    //sauvegarde pour la prochaine sélection
    oldLineObj = currentLine;
    oldClassName = className;
}

//setFldMergeFile : Permet l'ouverture de la fenêtre de dédoublonnage
// nParentFileId : Id de la fiche source
// nBkmFileId : Id de la fiche en doublon sélectionnée
function setFldMergeFile(nParentFileId, nBkmFileId) {
    var eModFileMrg = new eModalDialog(top._res_994, 0, "eMergeFiles.aspx", 700, 500);
    eModFileMrg.addParam("tab", nGlobalActiveTab, "post");
    eModFileMrg.addParam("fromfileid", nParentFileId, "post");
    eModFileMrg.addParam("bkmfileid", nBkmFileId, "post");

    eModFileMrg.ErrorCallBack = function () { eModFileMrg.hide(); };
    eModFileMrg.onIframeLoadComplete = function () { setFldMergeFile_onLoad(eModFileMrg); };
    eModFileMrg.show();

    eModFileMrg.addButton(top._res_29, null, "button-gray", null, "cancel"); // Annuler
    eModFileMrg.addButton(top._res_28, function () { mergeFile(nGlobalActiveTab, nParentFileId, nBkmFileId, eModFileMrg); }, "button-green", null, "ok"); // Valider

    document.getElementById("ImgControlBox_" + eModFileMrg.UID).onclick = function () { eModFileMrg.MaxOrMinModal(); eModFileMrg.getIframe().AdjustScrollDiv(); };
}
//Fonction appelée au chargement de la fenêtre de dédoublonnage
// eModFileMrg : objet modale de la fenêtre de dédoublonnage
function setFldMergeFile_onLoad(eModFileMrg) {
    var ifrm = eModFileMrg.getIframe();
    var oUlGlobal = ifrm.document.getElementById("ulGlobal");
    //Si ForceValid est vrai, on force la validation de la fenêtre
    var bForceValid = getAttributeValue(oUlGlobal, "eForceValid") == "1";
    if (bForceValid)
        eModFileMrg.CallOnOk();
}

function SetFldGoFile(oSourceObj) {


    /*#region Clique sur un champ de liaison (ou descid 01 d'un événement) > Redirige vers le mode fiche*/
    //LNKID dans le cas d'un champ principal 01 (fausse liaison) mais DBV dans le cas d'un champ de liaison vers une autre table
    var nFileId = oSourceObj.getAttribute("lnkid");
    if (typeof (nFileId) == 'undefined' || nFileId <= 0)
        nFileId = oSourceObj.getAttribute("dbv");

    var sName = oSourceObj.getAttribute("ename") + "";
    var objHeaderCell = document.getElementById(sName);
    var nFldDescId;
    var nTab;


    // gestion des catalogues spéciaux
    if (objHeaderCell.getAttribute("did")) {

        // Rubrique "Fiche" de la table historique
        if (objHeaderCell.getAttribute("did") && objHeaderCell.getAttribute("did") == "100005")
            nFldDescId = oSourceObj.getAttribute("lnkdid");
        // Rubrique "Fiche" de la table PJ
        else if (objHeaderCell.getAttribute("did") && objHeaderCell.getAttribute("did") == "102012")
            nFldDescId = oSourceObj.getAttribute("lnkdid");
        //else if (objHeaderCell.getAttribute("did") && (objHeaderCell.getAttribute("did") == "401" || objHeaderCell.getAttribute("did") == "400"))
        //    nFldDescId = 301;
        else if (objHeaderCell.getAttribute("popid") && objHeaderCell.getAttribute("special") && objHeaderCell.getAttribute("special") == "1")
            nFldDescId = objHeaderCell.getAttribute("popid");
        else
            nFldDescId = objHeaderCell.getAttribute("did");

        nTab = nFldDescId - (nFldDescId % 100);

        // 01/09/15 : Demande 40 869
        var eParamWindow = getParamWindow();
        if (eParamWindow != null && eParamWindow != undefined) {

            var tabName = eParamWindow.document.getElementById("TAB_MRU_" + nTab);
            if (!tabName) {
                nsMain.switchToHiddenTabUsingView(nTab, nFileId, "FILE_MODIFICATION");
                return;
            }
        }

        var oFileDiv = document.getElementById("fileDiv_" + nGlobalActiveTab);
        var bIsPopup = isPopup();
        if (bIsPopup && getAttributeValue(oFileDiv, "ro") != "1") {
            //if (top.getCurrentView() == "CALENDAR" || top.getCurrentView() == "CALENDAR_LIST" && typeof (onSaveAndCloseTplFiche) == 'function') {
            if (getAttributeValue(oFileDiv, "ecal") == "1" && typeof (onSaveAndCloseTplFiche) == 'function') {

                var afterSaveFunction = (function (nTabIdToReload, nFileIdToReload) {
                    return function () {
                        top.loadFile(nTabIdToReload, nFileIdToReload, 3);
                    }
                })(nTab, nFileId);

                afterSaveFunction.goFile = true;

                onSaveAndCloseTplFiche(afterSaveFunction);
            }
            else if (getAttributeValue(oFileDiv, "edntype") == "3") {


                var myFunc = function () {
                    var afterSaveFunction = (new function (engResult) { top.loadFile(nTab, nFileId, 3); });
                    afterSaveFunction.goFile = true;
                    top.validateFile(top.eModFile.tab, top.eModFile.fileId, null, true, top.eModFile, afterSaveFunction, true);
                };

                var ec = eConfirm(1, top._res_719, top._res_1586 + ' ?', '', 450, 200,
                    myFunc,
                    function () { });

                return;

            }
            else if (typeof (validateFile) == 'function') {


                // SPH
                //rempacement du "new function" qui ne retourne pas une fonction mais exécute immédiatement l'appel
                // par un return function
                // on doit fournir le "top" sinon, il est perdu entre les fonctions anonmye et closure...
                var afterSaveFunction = (function (wintop, nTabIdToReload, nFileIdToReload) {
                    return function () {
                        wintop.loadFile(nTabIdToReload, nFileIdToReload, 3);
                    }
                })(top, nTab, nFileId);

                afterSaveFunction.goFile = true;

                top.validateFile(top.eModFile.tab, top.eModFile.fileId, null, true, top.eModFile, afterSaveFunction, true);
            }
        }
        else {
            top.loadFile(nTab, nFileId, 3);
        }
    }
    else {
        var aName = sName.split("_");
        if (aName.length > 1) {

            nFldDescId = aName[aName.length - 1];
            nTab = nFldDescId - (nFldDescId % 100);

            // On fait disparaitre la carte de visite
            if (nFldDescId == '201')
                shvc(oSourceObj, 0);

            if (nTab == '400') {

                if (nGlobalActiveTab == '300') {
                    nTab = 200;
                }
                else if (nGlobalActiveTab == '200') {
                    nTab = 300;
                }
            }

            top.loadFile(nTab, nFileId, 2);
        }
    }
    /*#endregion*/
}

//Modication d'une PJ
var SelectedPjId = 0;
function SetFldPj(oSourceObj, bReturnUrl) {

    /*#region Modication d'une PJ*/
    var sAction = "";
    var sActionUrl = "";

    var sType = oSourceObj.getAttribute('eTyp');
    var url = oSourceObj.innerHTML;
    url = (url + "").trim();
    var idPj = oSourceObj.id.split("_")[3];
    if (!Number(idPj))
        idPj = oSourceObj.getAttribute("lnkid");

    //Sauvegarde la pj selectionnée.
    SelectedPjId = idPj;

    // On détermine le type de lien à construire en fonction du type d'annexe
    var sLnkType = '';
    switch (sType) {
        case "0":                 // Fichier
        case "6":                 // Fichier         
        case "7":                 // Fichier
            sLnkType = 'pjDisplay';
            break;
        case "1":                 //Fichier local : KO           
        case "5":                 //Repertoire ne marche que sur des répertoires partagés réseaux
            sLnkType = 'file';
            break;
        case "2":                 //Mail: ok
            sLnkType = 'mailto';
            break;
        case "3":                 //site web : ok
            sLnkType = 'http';
            break;
        case "4":                 //FTP : OK
            sLnkType = 'ftp';
            break;
        default:
            sLnkType = 'pjDisplay';
            break;
    }

    // Si on souhaite utilise un lien direct alors que l'URL n'est pas renseignée (cas du clic sur le bouton de la visionneuse : on ne peut
    // pas récupérer l'URL via innerHTML), on se repose sur pjDisplay
    if (sLnkType != 'pjDisplay' && url == '')
        sLnkType = 'pjDisplay';

    // Puis on construit le lien adapté
    switch (sLnkType) {

        case "pjDisplay":
            var file = document.getElementById("fileDiv_" + nGlobalActiveTab);
            var nTab = null;
            var fileId = null;
            var sDispFrom = 'pj';

            if (file != null && file != 'undefined' && Number(idPj)) {
                nTab = file.getAttribute("did");
                fileId = file.getAttribute("fid");
            }
            else {
                if (nGlobalActiveTab == TAB_PJ) {
                    var oTR = findUp(oSourceObj, "TR");
                    if (oTR) {
                        fileId = getAttributeValue(oTR, "fid");
                        nTab = getAttributeValue(oTR, "tab");
                    }
                    sDispFrom = 'pjtab';
                    if (fileId == "" || nTab == "") {
                        // sur la table des pj, on doit chercher la table sur la ligne de la pj
                        nTab = oSourceObj.getAttribute("lnkdid");
                        fileId = oSourceObj.getAttribute("lnkid");
                    }

                }
                else {
                    fileId = "";
                    nTab = "";

                    var oTR = findUp(oSourceObj, "TR");
                    if (oTR) {
                        fileId = getAttributeValue(oTR, "fid");
                        nTab = getAttributeValue(oTR, "tab");
                    }

                    if (fileId == "" || nTab == "") {
                        fileId = document.getElementById('nFileID');
                        nTab = document.getElementById('nTab');

                        if (fileId == null || nTab == null || typeof (fileId) == 'undefined' || typeof (nTab) == 'undefined')
                            return;

                        fileId = fileId.value;
                        nTab = nTab.value;
                    }
                }

                // On refuse l'affichage de la PJ si les ID passés ne sont pas des entiers.
                // On accepte toutefois qu'ils soient à 0 (ce qui équivaut à Number(nTab) == false) pour autoriser l'affichage de PJ de
                // fiches en cours de création.
                // C'est ePjDisplay, appelée par l'URL renvoyée ci-dessous, qui se chargera de refuser l'affichage si elle détermine que
                // le FileID est passé à 0 alors qu'il existe bien une fiche rattachée à l'annexe.
                if (Number(fileId) == 'NaN' || Number(nTab) == 'NaN')
                    return;
            }


            sAction = "windowOpen";
            if (oSourceObj.getAttribute('eAction') == 'LNKVIEWPJ')
                sActionUrl = "ePjDisplay.aspx?pj=" + idPj + "&descId=" + nTab + "&fileId=" + fileId + "&pjtype=" + sType + "&dispFrom=" + sDispFrom;
            else
                sActionUrl = getAttributeValue(oSourceObj, "srcPJ");;
            break;

        case "file":
            while (url.toLowerCase().indexOf(String.fromCharCode(92)) != -1)
                url = url.replace(String.fromCharCode(92), '/');

            if (url.toLowerCase().indexOf("file") != 0)
                url = "file:///" + url;
            sAction = "windowOpen";
            sActionUrl = url;
            break;

        case "mailto":                 //Mail: ok
            // todo : remplacer mailto par l'ouverture d'une fiche de type email
            sAction = "replaceUrl";
            sActionUrl = "mailto:" + url;
            break;

        case "http":                 /*site web et FTP : ok*/
        case "ftp":
            /*
            #40 252 : un lien http commençant par *ftp*:// est considéré comme valide et laissé tel quel.
            Idem pour un lien ftp commençant par http*://
            Ceci, pour éviter la construction de liens invalides de type http://ftp://eudonet.com
            Si aucun préfixe ne convient, on corrige simplement le lien en fonction du protocole sélectionné
            */

            //demande #70 625
            //on prend le textContent/innerText pour éviter l'encodage des & dans l'url du au innerHTML initial
            if (oSourceObj.textContent)
                url = oSourceObj.textContent
            else if (oSourceObj.innerText)
                url = oSourceObj.innerText;

            url = (url + "").trim();

            if (
                url.toLowerCase().indexOf("http://") != 0 &&
                url.toLowerCase().indexOf("https://") != 0 &&
                url.toLowerCase().indexOf("ftp://") != 0 &&
                url.toLowerCase().indexOf("ftps://") != 0 &&
                url.toLowerCase().indexOf("sftp://") != 0
            )
                url = sLnkType + "://" + url;

            sAction = "windowOpen";
            sActionUrl = url;
            break;
    }

    // Action à effectuer en fonction des paramètres
    if (bReturnUrl)
        return sActionUrl;
    else {
        switch (sAction) {
            case "windowOpen":
                window.open(sActionUrl);
                break;
            case "replaceUrl":
                location.href = sActionUrl;
                break;
        }
    }
    /*#endregion*/
}


function loadUserOption(pageId) {

    // Vide les nodes enfants et vide le contenu du div
    var oMainDiv = document.getElementById("mainDiv");
    var sType = oMainDiv.getAttribute("edntype");

    // Taille du conteneur
    var tabMainDivWH = GetMainDivWH();
    var height = tabMainDivWH[1];
    var divWidth = tabMainDivWH[0];

    var pageName = "xadm";
    var pageParameters = new Object();
    // Définir les paramètres sous la forme pageParameters["nomParametre"] = valeurParametre

    var loadUserOptions = true;
    // Reporter ici tous les modules de l'enum ADMIN_MODULE
    switch (pageId) {
        /* Modules "Options utilisateur" */
        case USROPT_MODULE_MAIN:
        case USROPT_MODULE_PREFERENCES:
        case USROPT_MODULE_PREFERENCES_THEME:
        case USROPT_MODULE_PREFERENCES_FONTSIZE:
        case USROPT_MODULE_PREFERENCES_LANGUAGE:
        case USROPT_MODULE_PREFERENCES_PROFILE:
        case USROPT_MODULE_PREFERENCES_SIGNATURE:
        case USROPT_MODULE_PREFERENCES_PASSWORD:
        case USROPT_MODULE_PREFERENCES_MEMO:
        case USROPT_MODULE_ADVANCED:
        case USROPT_MODULE_ADVANCED_EXPORT:
        case USROPT_MODULE_ADVANCED_PLANNING:
        case USROPT_MODULE_HOME:
        case USROPT_MODULE_PREFERENCES_FONTSIZE:
        case USROPT_MODULE_PREFERENCES_MRUMODE:
            loadUserOptions = true;
            break;
        /* Modules "Administration" */
        case USROPT_MODULE_ADMIN:
        case USROPT_MODULE_ADMIN_GENERAL:
        case USROPT_MODULE_ADMIN_ORM:
        case USROPT_MODULE_ADMIN_ACCESS:
        case USROPT_MODULE_ADMIN_ACCESS_USERGROUPS:
        case USROPT_MODULE_ADMIN_ACCESS_SECURITY:
        case USROPT_MODULE_ADMIN_ACCESS_PREF:
        case USROPT_MODULE_ADMIN_TABS:
        case USROPT_MODULE_ADMIN_HOME:
        case USROPT_MODULE_ADMIN_V7_HOMEPAGES:
        case USROPT_MODULE_ADMIN_XRM_HOMEPAGES:
        case USROPT_MODULE_ADMIN_HOME_EXPRESS_MESSAGE:
        case USROPT_MODULE_ADMIN_EXTENSIONS:
        case USROPT_MODULE_ADMIN_DASHBOARD:
        case USROPT_MODULE_ADMIN_DASHBOARD_RGPD:
        case USROPT_MODULE_ADMIN_DASHBOARD_RGPDTREATMENTLOG:
        case USROPT_MODULE_ADMIN_GENERAL_LOGO:
        case USROPT_MODULE_ADMIN_GENERAL_NAVIGATION:
        case USROPT_MODULE_ADMIN_GENERAL_LOCALIZATION:
        case USROPT_MODULE_ADMIN_GENERAL_SUPERVISION:
        case USROPT_MODULE_ADMIN_GENERAL_CONFIGADV:
        case USROPT_MODULE_ADMIN_EXTENSIONS_MOBILE:
        case USROPT_MODULE_ADMIN_EXTENSIONS_OUTLOOKADDIN:
        case USROPT_MODULE_ADMIN_EXTENSIONS_SIRENE:
        case USROPT_MODULE_ADMIN_EXTENSIONS_SYNCHRO:
        case USROPT_MODULE_ADMIN_EXTENSIONS_SYNCHRO_OFFICE365:
        case USROPT_MODULE_ADMIN_EXTENSIONS_SMS:
        case USROPT_MODULE_ADMIN_EXTENSIONS_CTI:
        case USROPT_MODULE_ADMIN_EXTENSIONS_API:
        case USROPT_MODULE_ADMIN_EXTENSIONS_EXTERNALMAILING:
        case USROPT_MODULE_ADMIN_EXTENSIONS_VCARD:
        case USROPT_MODULE_ADMIN_EXTENSIONS_SNAPSHOT:
        case USROPT_MODULE_ADMIN_EXTENSIONS_EMAILING:
        case USROPT_MODULE_ADMIN_EXTENSIONS_GRID:
        case USROPT_MODULE_ADMIN_EXTENSIONS_NOTIFICATIONS:
        case USROPT_MODULE_ADMIN_EXTENSIONS_FROMSTORE:
        case USROPT_MODULE_ADMIN_EXTENSIONS_POWERBI:
        case USROPT_MODULE_ADMIN_EXTENSIONS_ACCOUNTING_BUSINESSSOFT:
        case USROPT_MODULE_ADMIN_EXTENSIONS_ACCOUNTING_CEGID:
        case USROPT_MODULE_ADMIN_EXTENSIONS_ACCOUNTING_SAGE:
        case USROPT_MODULE_ADMIN_EXTENSIONS_ACCOUNTING_EBP:
        case USROPT_MODULE_ADMIN_EXTENSIONS_ACCOUNTING_SIGMA:
        case USROPT_MODULE.ADMIN_EXTENSIONS_IN_UBIFLOW:
        case USROPT_MODULE.ADMIN_EXTENSIONS_IN_HBS:
        case USROPT_MODULE.ADMIN_EXTENSIONS_DOCUSIGN:
        case USROPT_MODULE.ADMIN_EXTENSIONS_SMS_NETMESSAGE:
        case USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO_EXCHANGE2016ONPREMISE:
        case USROPT_MODULE_ADMIN_TAB:
        case USROPT_MODULE_ADMIN_TAB_USER:
        case USROPT_MODULE_ADMIN_TAB_GRID:
        //SHA : tâche #1 873
        case USROPT_MODULE_ADMIN_EXTENSIONS_ADVANCED_FORM:
        case USROPT_MODULE_ADMIN_EXTENSIONS_DEDICATED_IP:
        case USROPT_MODULE_ADMIN_EXTENSIONS_WORLDLINE_PAYMENT:
        case USROPT_MODULE_ADMIN_EXTENSIONS_LINKEDIN:
        case "":
        default:
            loadUserOptions = false;
            break;
    }

    //Charge le contenu pour la partie Options Utilisateur
    try {
        // Initialise CSS 
        clearHeader("ADMIN");
        addCss("eActions", "ADMIN");
        addCss("eAdmin", "ADMIN");

        addCss("eControl", "ADMIN");

        // JavaScripts
        addScript("eButtons", "ADMIN");
        addScript("ePopup", "ADMIN",
            function () {
                addScript("eGrapesJSEditor", "ADMIN");
                addScript("eMemoEditor", "ADMIN");
                addScript("eMarkedFile", "ADMIN",
                    function () {
                        addScript("ckeditor/ckeditor", "ADMIN");
                        addScript("eUserOptions", "ADMIN");

                        var oAdminUpdater = new eUpdater("xadminst/" + pageName + ".aspx", 1);

                        // Paramètres de base
                        oAdminUpdater.addParam("divH", height, "post");
                        if (divWidth != 0)
                            oAdminUpdater.addParam("divW", divWidth, "post");
                        oAdminUpdater.addParam("pageId", pageId, "post");

                        // Ajout des paramètres additionnels
                        for (var paramKey in pageParameters)
                            oAdminUpdater.addParam(paramKey, pageParameters[paramKey], "post");

                        // Appel
                        oAdminUpdater.ErrorCallBack = errorAdmin;
                        oAdminUpdater.send(updateContentAdmin, nGlobalActiveTab, pageId);
                    }
                );
            }
        );
    }
    catch (e) {

        alert('eMain.loadUserOption');
        alert(e.description);
    }
}


function ChangePasworAccessDenied() {
    eAlert(3, top._res_6781, top._res_6377, top._res_8499, null, null);
}

// Affichage de la modale d'administration des préférences du planning mode graphique
var planningPrefModal;
function showPlanningPrefDialog(nTab, nUserId) {
    planningPrefModal = new eModalDialog(top._res_7126, 0, "eda/eAdminPlanningPrefDialog.aspx", 1000, 900, "modalPlanningPref");

    planningPrefModal.noButtons = false;
    planningPrefModal.addParam("tab", nTab, "post");
    planningPrefModal.addParam("userid", nUserId, "post");
    planningPrefModal.show();

    planningPrefModal.addButton(top._res_30, function () { planningPrefModal.hide(); }, 'button-gray', null);
    planningPrefModal.addButton(top._res_28, function () { updatePlanningPref(nUserId); }, 'button-green', null);
}
// Mise à jour des préférences du planning mode graphique
function updatePlanningPref(nUserId) {
    var adminPlanningPref = planningPrefModal.getIframe().nsAdminPrefPlanning;
    if (adminPlanningPref) {
        adminPlanningPref.setCapsule();
        if (adminPlanningPref.updCaps.ListProperties.length > 0) {

            var json = JSON.stringify(adminPlanningPref.updCaps);

            var ashxUrl = (nUserId > 0) ? "eda/Mgr/eObjManager.ashx" : "eda/Mgr/eAdminDescManager.ashx";

            var upd = new eUpdater(ashxUrl, 1);
            upd.json = json;
            upd.send(function (oRes) {
                var res = JSON.parse(oRes);

                if (!res.Success) {
                    top.eAlert(1, top._res_416, res.UserErrorMessage);
                }
                else {
                    top.eAlert(4, top._res_7126, top._res_1761, '', null, null, function () {
                        planningPrefModal.hide();
                    });
                }
            });
        }
    }
    else {
        eAlert(1, "Mise à jour impossible", "Données non retrouvées");
    }
}
// Rafraîchissement des options : Demande de confirmation
function refreshPlanningPref(element, userid) {

    var tab = element.value;

    if (getAttributeValue(element, "lastvalid") != "0") {
        var conf = top.eConfirm(1, top._res_1264, top._res_7273, top._res_6741, null, null,
            function () {

                setAttributeValue(element, "lastvalid", tab);
                refreshPlanningPrefOK(tab, userid);
            },
            function () {
                conf.hide();
                element.value = getAttributeValue(element, "lastvalid");
            },
            true,
            true);
    }
    else {
        setAttributeValue(element, "lastvalid", tab);
        refreshPlanningPrefOK(tab, userid);
    }
}
// Appel du manager pour rafraîchissement des options planning
function refreshPlanningPrefOK(tab, userid) {
    top.setWait(true);

    var content = document.getElementById("planningPrefContent");
    var oUpdater = new eUpdater("eda/mgr/eAdminPlanningPrefManager.ashx", 1);
    oUpdater.addParam("tab", tab, "post");
    oUpdater.addParam("userid", userid, "post");
    oUpdater.ErrorCallBack = function () {
        top.setWait(false);
    };

    oUpdater.send(
        function (oRes) {
            top.setWait(false);
            content.innerHTML = oRes;

            loadPlanningPrefDialog(tab);
        });
}


//selectFileMail :
//Sélection de l'onglet email à afficher s'il y en a plusieurs
// S'il n'y en à qu'un => on ouvre directement l'envoi de mail unitaire
// S'il n'y en pas => Erreur utilisateur

function selectFileMail(oMLOrSMSFiles, sMailOrSMS, oParentInfo, mailType) {
    var mlFilesElts = oMLOrSMSFiles.children;
    if (!mlFilesElts || mlFilesElts.length == 0) {
        if (mailType == TypeMailing.SMS_MAILING_UNDEFINED) {
            // Aucun fichier SMS paramétré
            eAlert(2, top._res_1893, top._res_1892, top._res_6342);
        }
        else {
            // Aucun fichier E-mail paramétré
            eAlert(2, top._res_6184, top._res_6341, top._res_6342);
        }
        return;
    }
    if (mlFilesElts.length > 1) {
        //SHA : ajout RES 2237 pour Sélection de fichier de type SMS
        //ELAIZ - correction du titre de la modale "sélection de fichier " par sélection de signet ( retour  3022)
        // var sAction = getAttributeValue(oSrcElt, "eaction");
        if (mailType == TypeMailing.SMS_MAILING_UNDEFINED)
            var modChoiceFileMail = eConfirm(1, top._res_2655, top._res_2237, "", 450, 100, function () { onValidFileMail(modChoiceFileMail, sMailOrSMS, oParentInfo, mailType); }, null, true);
        else
            var modChoiceFileMail = eConfirm(1, top._res_2655, top._res_6694, "", 450, 100, function () { onValidFileMail(modChoiceFileMail, sMailOrSMS, oParentInfo, mailType); }, null, true);

        //Indique que l'on souhaite que les options soient des RadioBoutons (choix unique)
        modChoiceFileMail.isMultiSelectList = false;

        for (var i = 0; i < mlFilesElts.length; i++) {
            var tab = mlFilesElts[i].id.split("_");
            tab = tab[(tab.length > 1) ? 1 : 0];
            //RadioBouton 1 : ouvrir cette occurence
            modChoiceFileMail.addSelectOption(mlFilesElts[i].value, tab, (i == 0) ? true : false);
        }
        //Force l'ajout des radioboutons
        modChoiceFileMail.createSelectListCheckOpt();
        //Indique que la taille de la fenêtre doit s'ajuster au contenu
        modChoiceFileMail.adjustModalToContent(40);
    }
    else if (mlFilesElts.length == 1) {
        var tab = mlFilesElts[0].id.split("_");
        tab = tab[(tab.length > 1) ? 1 : 0];
        var fileName = mlFilesElts[0].value;
        sendMailTo(sMailOrSMS, tab, fileName, oParentInfo, mailType);
    }
}

//Méthode appelée à la confirmation de selectFileMail pour la sélection d'un mail
function onValidFileMail(modChoiceFileMail, sMail, oParentInfo, mailType) {
    if (!modChoiceFileMail) {
        eAlert(0, top._res_72, top._res_6658, '');
        throw "onValidFileMail - modal non trouvée.";
    }
    //Récupère les valeurs sélectionnées ici "0" ou "1"
    var result = modChoiceFileMail.getSelected();
    if (result && result.length > 0 && result[0].val) {
        sendMailTo(sMail, result[0].val, result[0].lib, oParentInfo, mailType);
    }
    else {
        eAlert(0, top._res_72, top._res_6658, '');
        throw "onValidFileMail - pas de table mail retournée !";
    }
}

//Indique si un champ est en cours d'édition ( is editing à true si onclick et a false si onblur)
function IsFieldEditing() {
    return (eInlineEditorObject.isEditing
        || eCatalogEditorObject.isEditing
        || eLinkCatFileEditorObject.isEditing
        || eCatalogUserEditorObject.isEditing
        || eDateEditorObject.isEditing
        || eCheckBoxObject.isEditing
        || eMemoEditorObject.isEditing
        || eBitButtonObject.isEditing
        || eStepCatalogEditorObject.isEditing
        || eMailEditorObject.isEditing
        || eGeolocEditorObject.isEditing
    );
}
//Retour le nom du jour de la date passé en paramètre
function getDayName(dt) {
    var tab_jour = new Array(top._res_44, top._res_45, top._res_46, top._res_47, top._res_48, top._res_49, top._res_50);    //dimanche, lundi, ..., samedi
    return tab_jour[dt.getDay()];
}
function getMonthString(date) {

}

function addDays(myDate, days) {
    return new Date(myDate.getTime() + days * 24 * 60 * 60 * 1000);
}

/*********************************************************/
/** GESTION EDITORS                                     **/
/*********************************************************/
var eFieldEditorLastEditedElement = null;
var ePopupObject = null;
var eInlineEditorObject = null;
var eMailEditorObject = null;
var eCatalogEditorObject = null;
var eCatalogUserEditorObject = null;
var eGeolocEditorObject = null;
var eStepCatalogEditorObject = null;
var eFldFileEditorObject = null;
/*FINDER*/
var eLinkCatFileEditorObject = null;
var eTabLinkCatFileEditorObject = null;
if (typeof (Dictionary) != "undefined") {
    eTabLinkCatFileEditorObject = new Dictionary(); //TABLEAU DES FINDER
}
/********/
/*USER*/
var eTabCatUserModalObject = null;
if (typeof (Dictionary) != "undefined") {
    eTabCatUserModalObject = new Dictionary(); //TABLEAU DES Catalogues utilisateurs
}
/********/
/*Catalog*/
var eTabCatModalObject = null;
if (typeof (Dictionary) != "undefined") {
    eTabCatModalObject = new Dictionary(); //TABLEAU DES Catalogues
}
/********/
var eCheckBoxObject = null;
var eBitButtonObject = null;
var eMemoEditorObject = null;


///Initialise les différents éditeurs
// (texte, catalogue )
var cntInitEditors = 0;
function initEditors() {

    if (typeof (ePopup) == 'undefined' || typeof (eFieldEditor) == 'undefined') {

        if (cntInitEditors < 10) {
            cntInitEditors++;
            setTimeout(initEditors, 250);
        }
        else {
            cntInitEditors = 0;
        }
        return;
    }

    try {
        ePopupObject = new ePopup('ePopupObject', 220, 250, 0, 0, document.body, false);
        ePopupObject.hide();
    }
    catch (e) {
        alert('Erreur : initEditors - ePopupObject - ' + e.message);
    }

    initEditorsOnScroll();

    try {
        eInlineEditorObject = new eFieldEditor('inlineEditor', ePopupObject, 'eInlineEditorObject', 'eInlineEditor');
    }
    catch (e) {
        alert('Erreur : initEditors - eInlineEditorObject - ' + e.message);
    }

    try {
        eMailEditorObject = new eFieldEditor('mailEditor', ePopupObject, 'eMailEditorObject', 'eMailEditor');
    }
    catch (e) {
        alert('Erreur : initEditors - eMailEditorObject - ' + e.message);
    }

    try {
        eCatalogEditorObject = new eFieldEditor('catalogEditor', ePopupObject, 'eCatalogEditorObject', 'eCatalogEditor');
    }
    catch (e) {
        alert('Erreur : initEditors - eCatalogEditorObject - ' + e.message);
    }

    try {
        eLinkCatFileEditorObject = new eFieldEditor('linkCatFileEditor', ePopupObject, 'eLinkCatFileEditorObject', 'eLinkCatFileEditor');
    }
    catch (e) {
        alert('Erreur : initEditors - eLinkCatFileEditorObject - ' + e.message);
    }

    try {
        eCatalogUserEditorObject = new eFieldEditor('catalogUserEditor', ePopupObject, 'eCatalogUserEditorObject', 'eCatalogUserEditor');
    }
    catch (e) {
        alert('Erreur : initEditors - eCatalogUserEditorObject - ' + e.message);
    }

    try {
        eGeolocEditorObject = new eFieldEditor('geolocEditor', ePopupObject, 'eGeolocEditorObject', 'eGeolocEditor');
    }
    catch (e) {
        alert('Erreur : initEditors - eGeolocEditorObject - ' + e.message);
    }

    try {
        eStepCatalogEditorObject = new eFieldEditor('stepCatalogEditor', ePopupObject, 'eStepCatalogEditorObject', 'eStepCatalogEditor');
    }
    catch (e) {
        alert('Erreur : initEditors - eStepCatalogEditorObject - ' + e.message);
    }

    try {
        eDateEditorObject = new eFieldEditor('dateEditor', ePopupObject, 'eDateEditorObject', 'eDateEditor');
    } catch (e) {
        alert('Erreur : initEditors - eDateEditorObject - ' + e.message);
    }

    try {
        eFldFileEditorObject = new eFieldEditor('fileEditor', ePopupObject, 'eFldFileEditorObject', 'eFldFileEditor');
    } catch (e) {
        alert('Erreur : initEditors - eFldFileEditorObject - ' + e.message);
    }


    try {
        eCheckBoxObject = new eFieldEditor('eCheckBox', ePopupObject, 'eCheckBoxObject', 'eCheckBox');
    } catch (e) {
        alert('Erreur : initEditors - eCheckBoxObject ' + e.message);
    }

    try {
        eBitButtonObject = new eFieldEditor('eBitButton', ePopupObject, 'eBitButtonObject', 'eBitButton');
    } catch (e) {
        alert('Erreur : initEditors - eBitButtonObject ' + e.message);
    }

    try {
        eMemoEditorObject = new eFieldEditor('memoEditor', ePopupObject, 'eMemoEditorObject', 'eMemoEditor');
    } catch (e) {
        alert('Erreur : initEditors - eMemoEditorObject - ' + e.message);
    }
}

function initEditorsOnScroll() {
    //lors de l'utilisation de la scrollbar la popup est fermée.
    try {
        var aDivId = new Array("divBkmPres", "mainDiv", "md_pl-base", "divDetailsBkms");

        for (var i = 0; i < aDivId.length; i++) {
            var oElt = document.getElementById(aDivId[i]);
            if (oElt) {
                oElt.onscroll = function () { ePopupObject.hide(); };
            }
        }
    }
    catch (e) {
    }

}
/*********************************************************/
/** FIN DE GESTION EDITORS                              **/
/*********************************************************/


/* Resize de font des listes */
/*  si bSettoval = true : ajuste la taille à la valeur nVal
sinon, incrémente la taille de la valeur nVal
*/
function resizeFont(nVal, bSettoval) {
    if (typeof (bSettoval) == "undefined")
        bSettoval = false;

    // Lignes
    resizeSelector("elist.css", ".mTab", nVal, bSettoval);
    resizeSelector("elist.css", ".hdTable", nVal, bSettoval);
    // Fiche
    resizeSelector("efile.css", ".table_labels, .table_lab_pty", nVal, bSettoval);
    resizeSelector("efile.css", "input[type=\"text\"]", nVal, bSettoval);

    // Redimensionnement des colonnes
    autoResizeColumns();
}

// Resize de la taille de police du sélecteur CSS en argument
function resizeSelector(cssFile, selector, nVal, bSettoval) {
    var oRule = getCssSelector(cssFile, selector);
    if (oRule) {

        var nSize = getNumber(oRule.style.fontSize);

        // si besoin de memo la size de la font
        if (typeof (nBaseFontSize) != "undefined" && nBaseFontSize == 0)
            nBaseFontSize = nSize;

        oRule.style.fontSize = nVal + "pt";
    }
}

//bReset lance un autoresize avant d'agrandir la dernière colonne
function adjustLastCol(nTab, oDivMain, bReset) {
    if (typeof (nTab) == "undefined" || nTab == "-1")
        var nTab = nGlobalActiveTab;

    var maTab = document.getElementById("mt_" + nTab);
    if (!maTab)
        return;

    // Redimensionnement de la dernière colonne
    var allTD = maTab.getElementsByTagName('td');
    for (var i = allTD.length - 1; i >= 0; i--) {
        if (allTD[i].className == 'hdResize') { //GCH pour IE8 utiliser className au lieu de getAttribute("class") au moins pour les td 
            if (bReset)
                thtabevt.resizeAuto(null, allTD[i], oDivMain);

            thtabevt.resizeLastMax(null, (allTD[i]), oDivMain);
            break;
        }
    }
}



// HLA - Attention cette méthode est couteuse (update pref pour chaque colonne) et dangeureuse car elle ecrase les pref user
function autoResizeColumns(nTab, oDivMain) {
    if (typeof (nTab) == "undefined")
        var nTab = nGlobalActiveTab;
    var maTab = document.getElementById("mt_" + nTab);

    if (!maTab)
        return;

    // Redimensionnement des colonnes
    var allTD = maTab.getElementsByTagName('td');
    // var lastTD = null;
    for (var i = 0; i < allTD.length; i++) {
        if (allTD[i].getAttribute('class') == 'hdResize') {
            thtabevt.resizeAuto(null, allTD[i]);
        }
    }

    adjustLastCol(nTab, oDivMain)
}

function ReloadCalendar() {
    goTabList(nGlobalActiveTab, 1);
}

function setCalendarDate(nTab, sDate) {
    var updatePref = "tab=" + nTab + ";$;calendardate=" + sDate;
    updateUserPref(updatePref, ReloadCalendar);
}

// Faire disparaitre la div Coller dans le planning
function deleteDivPaste() {

    //var myDiv = document.getElementById("control_contextmenu");
    //if (myDiv != null && myDiv != 'undefined') {
    //    myDiv.attributes.style.value = "display : none;";
    //}

    if (ePopupObject && typeof (ePopupObject.hide) == "function")
        ePopupObject.hide();
}

//Redirection vers les différents mode du plannings
//ViewMode (CalendarViewMode) : 
//    VIEW_CAL_LIST = 0,         //	Mode visu Liste
//    VIEW_CAL_DAY = 1,		    //	Mode visu Planning Jour
//    VIEW_CAL_TODAY = 2,	        //	Mode visu Planning du Jour
//    VIEW_CAL_WORK_WEEK = 3,   //  Mode visu Planning Semaine de travail
//    VIEW_CAL_TASK = 4,   //	Mode visu Planning des Tâches
//    VIEW_CAL_DAY_PER_USER = 5,  //	Mode visu Planning Jour par utilisateur
//    VIEW_CAL_MONTH = 6		    //  Mode visu Planning Mois
//nTab : Id de la table de planning à afficher
//nUserId : UserId de l'utilisateur dont le planning est à afficher
//sDate : date demandée
function setCalViewMode(viewMode, nTab, nUserId, sDate) {
    nTab = (nTab) ? nTab : nGlobalActiveTab;
    sUpdatePrefSupp = "";

    if (typeof (nUserId) != "undefined" && nUserId > 0)
        sUpdatePrefSupp += ";$;menuuserid=" + nUserId;


    if (typeof (sDate) != "undefined" && sDate != "")
        sUpdatePrefSupp += ";$;calendardate=" + sDate;

    var updatePref = "tab=" + nTab + ";$;viewmode=" + viewMode + sUpdatePrefSupp;

    updateUserPref(updatePref, function () { goTabList(nTab, 1); });
}



function calendarLoaded(nTab) {
    var oPlanning = document.getElementById("cal_mt_" + nTab);
    if (oPlanning) {
        initCalCss(nTab);
        loadTabletCalendar();
    }
}
// Methode qui retourne une chaine indiquant dans quel mode nous nous trouvons
// return : 
//        CALENDAR,
//        CALENDAR_LIST,
//        LIST,
//        FILE_CONSULTATION,
//        FILE_MODIFICATION,
//        FILE_CREATION
//      KANBAN
function getCurrentView(doc) {

    if (typeof doc === "undefined" || !doc) {
        doc = top.document;
    }

    if (doc.getElementById("hidWidgetType")) {
        if (doc.getElementById("hidWidgetType").value == "16")
            return "KANBAN";
    }

    var bCalMainDiv = nodeExist(doc, "CalDivMain");
    var oFileDiv = doc.getElementById("fileDiv_" + nGlobalActiveTab);
    if (oFileDiv == null || typeof (oFileDiv) == 'undefined') {
        var oAllFileDiv = doc.querySelectorAll('*[id^="fileDiv_"]');
        if (oAllFileDiv && oAllFileDiv.length > 0)
            oFileDiv = oAllFileDiv[0];
    }
    var bFileDiv = (oFileDiv != null && typeof (oFileDiv) != 'undefined');

    if (bCalMainDiv) {
        var mixteMode = doc.getElementById("CalDivMain").getAttribute("mixtemode");
        if (typeof (mixteMode) == 'undefined' || mixteMode != '1')
            return "CALENDAR";
        else
            return "CALENDAR_LIST";
    }

    if (!bFileDiv) {
        //if (document.getElementById("hidWidgetType"))
        //    return "LIST_WIDGET";
        //else
        return "LIST";
    }
    else {
        var fileType = oFileDiv.getAttribute("ftrdr");

        switch (fileType) {
            case "2":
                return "FILE_CONSULTATION";
            case "3":
                return "FILE_MODIFICATION";
            case "5":
                return "FILE_CREATION";
            case "8":
                return "ADMIN_FILE";
        }
    }
}

// renvoie si la fiche est affichée en popup 
function isPopup() {
    var mainDiv = document.getElementById('mainDiv');
    return (mainDiv && mainDiv.getAttribute('popup') == '1');
}

// doit on faire la mise à jour en sortie de champ?
function isUpdateOnBlur() {
    var curView = getCurrentView(document);

    // En mode Création de fichier ou Consultation (ex : consultation d'un e-mail existant), pas de mise à jour en sortie de champ
    if (curView == "FILE_CREATION" || curView == "FILE_CONSULTATION" || curView == "ADMIN_FILE")
        return false;

    // En mode Modification : sous conditions
    var mainDiv = document.getElementById('mainDiv');
    var bPopup = (mainDiv && mainDiv.getAttribute('popup') == '1');
    var bAutoSave = (mainDiv && mainDiv.getAttribute('autosv') == '1');
    if (curView == "FILE_MODIFICATION" && bPopup && !bAutoSave)
        return false;

    // Dans tous les autres cas : mise à jour en sortie de champ
    return true;
}

/// Construit un tableau de fldUpdEngine a partir des données d'un mode fiche

/// <param name="nTab">Table des champs à récupérer</param>
/// <param name="nFileId">FileId de la fiche</param>
/// <param name="lstDescId"> lites des descid qu'on veut recuperer leur fldEngine </param>
/// <returns></returns>
function getFieldsInfos(nTab, nFileId, lstDescId) {

    var doc = document;
    if ((typeof (lstDescId) == "undefined") || lstDescId == null) {
        lstDescId = "";
    }

    if (typeof (nFileId) == "undefined")
        nFileId = 0;

    var aFields = new Array();

    var oInputFields, oInputFieldsecond;
    if (lstDescId.length == 0) {
        if (doc.getElementById("fieldsId_InfosCampaign_" + nTab))
            oInputFields = doc.getElementById("fieldsId_InfosCampaign_" + nTab);
        else
            oInputFields = doc.getElementById("fieldsId_" + nTab);


        if ((typeof oInputFields == 'undefined' || oInputFields == null) && eModFile != null && typeof eModFile != 'undefined') {

            doc = eModFile.getIframe().document;
            oInputFields = doc.getElementById("fieldsId_" + nTab);

        }

    }


    if (oInputFields || lstDescId.length > 0) {

        var aFieldsDescid = (lstDescId.length > 0 ? lstDescId : oInputFields.value).split(";");

        for (var nCmptFld = 0; nCmptFld < aFieldsDescid.length; nCmptFld++) {
            var descid = aFieldsDescid[nCmptFld];

            if (lstDescId.length > 0 && (";" + lstDescId + ";").indexOf(";" + descid + ";") < 0) {
                continue;
            }

            var oField = null;
            if (getTabDescid(getNumber(descid)) != nTab && getNumber(descid) % 100 == 1)
                oField = doc.querySelector("[id^='COL_" + nTab + "_" + descid + "_'][efld='1']");
            else
                oField = doc.getElementById("COL_" + nTab + "_" + descid + "_" + nFileId + "_" + nFileId + "_0");

            if (oField == null) {
                oField = doc.getElementById("COL_" + nTab + "_" + descid + "_NAME_" + nFileId + "_" + nFileId + "_0");
                if (oField == null || oField.querySelector("img[isb64='1']") == null)
                    continue;
            }

            var fldEngine = getFldEngFromElt(oField);

            if (fldEngine != null) {
                var bFindDescid = false;
                for (var i = 0; i < aFields.length; i++) {
                    if (aFields[i].descId == fldEngine.descId) {
                        bFindDescid = true;
                        break;
                    }
                }

                if (!bFindDescid)
                    aFields.push(fldEngine);
            }
        }

    }

    //dans le cas d'une création  de contact, on récupère également les informations en provenance d'adresse.
    if (nTab == 200 && nFileId == 0) {

        // Dans le cas ou on ajoute un contact et qu'on a choisi "Sans adresse",
        // il faudrait de ne pas ajouter les champs de la fiche adresse.

        // Par défaut on les ajoute
        var oWithoutAdrRadio = doc.getElementById("COL_400_492_2");
        var withoutAdrRadioChecked = oWithoutAdrRadio != null && typeof (oWithoutAdrRadio) != "undefined" && oWithoutAdrRadio.tagName == "INPUT" && oWithoutAdrRadio.checked;
        if (!withoutAdrRadioChecked) {
            var adrFields = getFieldsInfos(400, 0, lstDescId);
            if (adrFields != null && adrFields.length > 0)
                aFields = aFields.concat(adrFields);
        }
    }

    // récupération de PPID, PMID ADRID ParentEVTID
    //var oIParentFields = document.getElementById("PrtTabs_" + nTab)
    //if (oIParentFields) {
    // var aParentInputsIds = oIParentFields.value.split(';');
    //Demande #58240 et #60137 - Cas particulier sur ADDRESS : il n'y a pas la section des rubriques parentes, on utilise donc l'ancien système
    var aParentLength = 0;
    var aParentInputsIds = null;
    var aParentFieldsLabels = null;
    if (nTab == 400) {
        var oIParentFields = doc.getElementById("PrtTabs_" + nTab)
        if (oIParentFields) {
            aParentInputsIds = oIParentFields.value.split(';');
            aParentLength = aParentInputsIds.length;
        }
    } else {
        aParentFieldsLabels = doc.querySelectorAll("table[id^='ftp_'] td[prt='1']");
        aParentLength = aParentFieldsLabels.length;
    }
    //var aParentFieldsLabels = document.querySelectorAll("table[id^='ftp_'] td[prt='1']");
    for (var i = 0; i < aParentLength; i++) {
        var d = NaN;
        if (nTab == 400)
            d = getNumber(aParentInputsIds[i]);
        else
            d = getNumber(getAttributeValue(aParentFieldsLabels[i], "did")) + 1;

        if (!isNaN(d)) {
            if (lstDescId.length > 0 && (";" + lstDescId + ";").indexOf(";" + d + ";") < 0) {
                continue;
            }

            var oField = GetField(nTab, d);
            if (oField == null) {
                continue;
            }

            var eFld = getFldEngFromElt(oField);
            if (eFld != null) {
                if (aFields.findIndex) { //IE12/Chrome/Firefox
                    if (aFields.findIndex(function (f) { return f.descId == eFld.descId }) == -1)
                        aFields.push(eFld);
                }
                else { //Version IE11
                    var fieldFound = false;
                    for (var fieldFoundIndex = 0; fieldFoundIndex < aFields.length && !fieldFound; ++fieldFoundIndex) {
                        if (aFields[fieldFoundIndex].descId == eFld.descId)
                            fieldFound = true;
                    }
                    if (!fieldFound)
                        aFields.push(eFld);
                }
            }
        }
    }
    //}

    return aFields;
}

function getFldEngFromElt(oFieldElt) {

    var headerElement = oFieldElt.ownerDocument.getElementById(oFieldElt.getAttribute("eName"));
    var fldEngine = null;
    var bDate = false;	//GCH - #35859 - Internationnalisation Date - Fiche
    var bNumerique = false; //GCH - #36022 - Internationalisation Numerique - Fiche
    var bMemoSpec = false;
    // Information en cellule d'entête
    if (headerElement) {
        fldEngine = new fldUpdEngine(headerElement.getAttribute("did"));

        fldEngine.isInRules = headerElement.getAttribute("rul") == "1";
        fldEngine.hasMidFormula = headerElement.getAttribute("mf") == "1";

        fldEngine.multiple = headerElement.getAttribute("mult") == "1";
        fldEngine.popId = headerElement.getAttribute("popid");
        fldEngine.popupType = headerElement.getAttribute("pop");

        fldEngine.boundDescId = headerElement.getAttribute("bndid");
        fldEngine.boundPopup = headerElement.getAttribute("bndPop");

        fldEngine.treeView = headerElement.getAttribute("tree") == "1";
        fldEngine.label = GetText(headerElement);

        fldEngine.cellId = oFieldElt.id;
        fldEngine.format = getAttributeValue(headerElement, "frm");
        fldEngine.lib = getAttributeValue(headerElement, "lib");

        //GCH - #35859 - Internationnalisation Date - on permet l'identification des champs au format date pour les convertir au format de la Base de données
        bDate = (fldEngine.format == "2");
        //GCH - #36022 - Internationalisation Numerique - Fiche
        bNumerique = isFormatNumeric(fldEngine.format);
        //
        bMemoSpec = getAttributeValue(headerElement, "efrmr") == "1";
    }
    else if (oFieldElt.getAttribute("did")) {
        fldEngine = new fldUpdEngine(oFieldElt.getAttribute("did"));
    }
    else
        return null;

    // #56970 : Si le champ est de type "Graphique", on retourne null pour que le champ ne soit pas mis à jour
    if (fldEngine.format == "17")
        return null;

    // Cas d'une rubrique MEMO
    var bMemo = false;
    if (oFieldElt.tagName == "TD" && oFieldElt.firstChild) {
        bMemo = oFieldElt.firstChild.tagName == "TEXTAREA";
        bMemo = bMemo || (oFieldElt.firstChild.tagName == "DIV" && oFieldElt.firstChild.id.indexOf("eMEG_") == 0);
        bMemo = bMemo || (oFieldElt.firstChild.tagName == "DIV" && oFieldElt.firstChild.className == "editor-row"); /* 68 13x - Détection du champ Mémo si utilisation de l'éditeur de templates HTML avancé (grapesjs) */
    }

    fldEngine.readOnlyBlank = oFieldElt.getAttribute("readonlyonblank") == "1";
    fldEngine.readOnly = (oFieldElt.getAttribute("ero") == "1");
    fldEngine.obligat = oFieldElt.getAttribute("obg") == "1";
    fldEngine.prevValue = getAttributeValue(oFieldElt, "oldval");


    // HLA - Gestion de l'autobuildname en mode création en popup - Dev #33529
    var chgedVal = getAttributeValue(oFieldElt, "chgedval");
    if (chgedVal != '')
        fldEngine.chgedVal = chgedVal == "1";

    var textType = 3;
    if (oFieldElt.tagName == "TD" && oFieldElt.firstChild && oFieldElt.firstChild.nodeType == textType) {
        fldEngine.newLabel = GetText(oFieldElt);
        fldEngine.newValue = getAttributeValue(oFieldElt, "dbv");


    }

    //TODO Enchainement des if_else_if_else 
    if (oFieldElt.tagName == "INPUT") {

        if (oFieldElt.getAttribute("eudofront") != null || hasClass(oFieldElt.parentElement, "v-text-field__slot")) {
            if (oFieldElt.type === 'checkbox')
                fldEngine.newValue = oFieldElt.checked ? "1" : "0";
            else if (bDate) {
                //Récupérer la valeur de la date pour un composant date 'edn-date'
                //TODO: modifer l'attribut du composant pour vérifier si c'est un input de type eudo-font au lieu d'utiliser la classe 'v-text-field__slot'
                var timeDate = "00:00";
                var timeElement = oFieldElt.ownerDocument.getElementById("tm" + fldEngine.cellId);
                if (timeElement)
                    timeDate = timeElement.value;
                fldEngine.newLabel = eDate.ConvertDisplayToBdd(oFieldElt.value + ' ' + timeDate);
                fldEngine.newValue = fldEngine.newLabel;
            }
            else if (bNumerique)
                fldEngine.newValue = eNumber.ConvertDisplayToBdd(oFieldElt.value);
            else
                fldEngine.newValue = oFieldElt.value;
        }
        else {
            fldEngine.boundValue = oFieldElt.getAttribute("pdbv");

            if (oFieldElt.getAttribute("dbv") == null || oFieldElt.getAttribute("dbv") == '') {
                if (oFieldElt.getAttribute("lnkid") == null || oFieldElt.getAttribute("lnkid") == '') {
                    //GCH - #35859 - Internationnalisation - Fiche
                    if (bDate)
                        fldEngine.newValue = eDate.ConvertDisplayToBdd(oFieldElt.value);
                    else if (bNumerique)
                        fldEngine.newValue = eNumber.ConvertDisplayToBdd(oFieldElt.value);
                    else
                        fldEngine.newValue = oFieldElt.value;
                }
                else {
                    fldEngine.newValue = oFieldElt.getAttribute("lnkid");
                    fldEngine.newLabel = oFieldElt.value;
                }
            }
            else {
                fldEngine.newValue = oFieldElt.getAttribute("dbv");
                //GCH - #35859 - Internationnalisation - Fiche
                if (bDate)
                    fldEngine.newLabel = eDate.ConvertDisplayToBdd(oFieldElt.value);
                else if (bNumerique)
                    fldEngine.newLabel = eNumber.ConvertDisplayToBdd(oFieldElt.value);
                else
                    fldEngine.newLabel = oFieldElt.value;
            }
        }



    }
    else if ((oFieldElt.tagName == "TD" || oFieldElt.tagName == "SPAN") && oFieldElt.firstChild && oFieldElt.firstChild.tagName == "A") {
        // cas de case à cocher
        var oChkBx = oFieldElt.firstChild;
        fldEngine.newValue = oChkBx.getAttribute("chk");
        fldEngine.newLabel = fldEngine.newValue == "1" ? top._res_58 : top._res_59; //oui/non
    }
    else if (oFieldElt.tagName == "TD" && oFieldElt.firstChild && oFieldElt.firstChild.tagName == "SELECT") {
        // cas d'une combobox
        var oSelect = oFieldElt.firstChild;
        // On verifie que la combobox n'est pas vide et est bien initialisée. GLA
        if (oSelect.selectedIndex > -1)
            fldEngine.newValue = oSelect.options[oSelect.selectedIndex].value;
    }
    else if (oFieldElt.tagName == "DIV" && oFieldElt.children[1] && oFieldElt.children[1].tagName == "SELECT") {
        // cas d'une combobox
        var oSelect = oFieldElt.children[1];
        // On verifie que la combobox n'est pas vide et est bien initialisée. GLA
        if (oSelect.selectedIndex > -1)
            fldEngine.newValue = oSelect.options[oSelect.selectedIndex].value;
    }
    else if (oFieldElt.tagName == "TD" && oFieldElt.firstChild && oFieldElt.firstChild.tagName == "DIV" && getAttributeValue(oFieldElt.firstChild, "eaction") == "LNKSTEPCAT") {
        var oSelected = oFieldElt.firstChild.querySelector("ul li.selectedValue a");
        fldEngine.newValue = getAttributeValue(oSelected, "dbv");
    }
    else if ((oFieldElt.firstChild && oFieldElt.tagName == "TD" && oFieldElt.firstChild.tagName == "IMG")
        || (oFieldElt.tagName == "TD" && isElementFirstChildEmptyPictureArea(oFieldElt))
        || (oFieldElt.firstChild && oFieldElt.tagName == "SPAN" && oFieldElt.firstChild.tagName == "IMG")) {
        // cas d'une rubrique image
        if (oFieldElt.firstChild.tagName == "IMG" && getAttributeValue(oFieldElt.firstChild, "isb64") == "1") {
            fldEngine.newValue = getAttributeValue(oFieldElt.firstChild, "src");
            fldEngine.isB64 = "1";
        }
        else
            fldEngine.newValue = oFieldElt.getAttribute("dbv");
    }
    else if (oFieldElt.tagName == "TD" && oFieldElt.firstChild && oFieldElt.firstChild.tagName == "DIV"
        && oFieldElt.firstChild.className != "eME_globl" && oFieldElt.firstChild.className != "eME_cglobl" && oFieldElt.firstChild.className != "editor-row") {
        // bouton radio 
        var oRadioDiv = oFieldElt.firstChild;
        fldEngine.newValue = oRadioDiv.getAttribute("rval");
    }
    else if (oFieldElt.tagName == "DIV" && oFieldElt.firstChild && oFieldElt.firstChild.tagName == "INPUT"
        && oFieldElt.firstChild.className != "eME_globl" && oFieldElt.firstChild.className != "eME_cglobl" && oFieldElt.firstChild.className != "editor-row") {
        // bouton radio        
        fldEngine.newValue = oFieldElt.getAttribute("rval");
    }
    else if ((oFieldElt.tagName == "DIV" && oFieldElt.getAttribute("planningmultiuser") == '1') || (oFieldElt.tagName == "SPAN" && oFieldElt.getAttribute("usrfrm") == '1')) {
        // planning multi-users ou le catalog des formulaire
        fldEngine.newValue = oFieldElt.getAttribute("dbv");
    }
    else if ((oFieldElt.tagName == "SPAN" || oFieldElt.tagName == "TD")
        && oFieldElt.getAttribute("PjIds") != null && oFieldElt.getAttribute("PjIds") != '') {
        // Cas de la rubrique ATTACHEMENT (XX91)
        fldEngine.newValue = getAttributeValue(oFieldElt, "PjIds");
    }
    // Les champs mémo Formulaire
    else if (oFieldElt.tagName == "DIV") {//KJE tâche 2 334
        fldEngine.newValue = oFieldElt.innerHTML;
    }
    // Les champs mémo Formulaire
    else if (oFieldElt.tagName == "TEXTAREA" && (getAttributeValue(oFieldElt, "efrmr") == "1" || bMemoSpec)) {
        fldEngine.newValue = oFieldElt.value;
    }
    else if (bMemo) {        // TYPE MEMO
        if (nsMain.hasMemoEditors() && nsMain.getMemoEditor("edt" + oFieldElt.id)) {
            var memoData = nsMain.getMemoEditor("edt" + oFieldElt.id).getData();
            fldEngine.newValue = memoData;
        }
    }
    //Demande #75 678: pour les champs de type Mémo Htm, on récupére le contenu de l'input
    else if (oFieldElt.tagName == "TD" && oFieldElt.firstChild && oFieldElt.firstChild.tagName == "INPUT"
        && oFieldElt.childNodes.length > 1 && oFieldElt.childNodes[1].tagName == "IFRAME" && oFieldElt.childNodes[1].className == "eME" && oFieldElt.firstChild.value) {
        fldEngine.newValue = oFieldElt.firstChild.value;
    }

    return fldEngine;
}

function getObligatFields(objParent) {
    var aFields = new Array();
    var obligats = objParent.querySelectorAll("input[efld='1'][obg='1']");
    for (var i = 0; i < obligats.length; i++) {
        var fldElt = obligats[i]
        aFields.push(getFldEngFromElt(fldElt));
    }

    return aFields;
}

/*
Fonction validateFile : Fonction appelée à la validation d'une fiche
nTab : table de la fiche validée
nFileId : FileId de la fiche mise à jours ou 0 si création
aFields :
bClose : indique si l'on doit fermer la fenêtre après traitement
eModFile :
afterValidate : Méthode appelée après validation
*/
function validateFile(nTab, nFileId, aFields, bClose, eModFile, afterValidate, bStopRefresh, nCallFrom, bConfirmed) {
    if (!bConfirmed) {
        launchChkTplDblFromModal(eModFile, function () { validateFile(nTab, nFileId, aFields, bClose, eModFile, afterValidate, bStopRefresh, nCallFrom, true); })
        return;
    }

    if (typeof (eModFile) == "undefined")
        eModFile = null;

    if (eModFile != null && typeof (eModFile) != "undefined" && eModFile.getIframe() != null && typeof (eModFile.getIframe()) != "undefined")
        oWin = eModFile.getIframe();
    else
        oWin = window;

    if (aFields == null) {
        if (nFileId == 0 || typeof (nFileId) == "undefined")
            nFileId = oWin.GetTabFileId(nTab);

        if (getNumber(getAttributeValue(oFileDiv, "edntype")) == EDNTYPE_MAIL) {
            //si multi editeur mail, met à jour le principale

        }

        aFields = oWin.getFieldsInfos(nTab, nFileId);
    }

    var isTypeMail = "0";
    var isTestMail = false;
    var oFileDiv = oWin.document.getElementById("fileDiv_" + nTab);
    if (getNumber(getAttributeValue(oFileDiv, "edntype")) == EDNTYPE_MAIL || getNumber(getAttributeValue(oFileDiv, "edntype")) == EDNTYPE_SMS) {

        isTypeMail = "1";

        // Vérification : le champ A doit être rempli, sauf dans le cas du mail de test
        if ((oWin.document.getElementById('mailIsTest') && oWin.document.getElementById('mailIsTest').value == '1') == false) {
            for (var i = 0; i < aFields.length; i++) {
                var fld = aFields[i];

                if (fld.descId != nTab + FLD_MAIL_TO) {
                    continue;
                }

                if (fld.newValue == "") {
                    eAlert(2, top._res_2881, top._res_6765);
                    return;
                }

                break;
            }
        }
        else {
            //Ajout des destinataires du mail de test dans un champ caché, sur lequel on recopie les attributs "Eudo" du champ "Destinataires" usuel de la fenêtre parente
            // (ename, dbv, etc.)
            //Cela permettra à getFldEngFromElt(oField) de récupérer les informations nécessaires au traitement du champ par Engine

            var realRecipients = oWin.document.getElementById("COL_" + nTab + "_" + (parseInt(nTab) + parseInt(FLD_MAIL_TO)) + "_" + nFileId + "_" + nFileId + "_0");
            var mailTestRecipients = oWin.document.getElementById("mailTestRecipients");

            if (mailTestRecipients != undefined) {
                for (var i = 0; i < realRecipients.attributes.length; i++) {
                    if (
                        realRecipients.attributes[i].name != "id" &&
                        realRecipients.attributes[i].name != "name" &&
                        realRecipients.attributes[i].name != "value" &&
                        realRecipients.attributes[i].name != "dbv" &&
                        realRecipients.attributes[i].name != "type" &&
                        realRecipients.attributes[i].name != "class" &&
                        realRecipients.attributes[i].name != "style" &&
                        realRecipients.attributes[i].name != "title" &&
                        realRecipients.attributes[i].name != "ero" &&
                        realRecipients.attributes[i].name != "readonly"
                    )
                        mailTestRecipients.setAttribute(realRecipients.attributes[i].name, realRecipients.attributes[i].value);
                }
                mailTestRecipients.setAttribute("ero", "0"); // readonly = false
                aFields.push(getFldEngFromElt(mailTestRecipients));
                isTestMail = true;
            }
        }
    }

    //on valide l'input   
    var eEngineUpdater = new eEngine();
    eEngineUpdater.Init();
    if (bStopRefresh)
        eEngineUpdater.ReloadNothing = true;

    if (typeof (nFileId) == "undefined")
        nFileId = 0;

    if (typeof (nCallFrom) !== "undefined")
        if (nCallFrom == CallFromKanban)
            eEngineUpdater.AddOrSetParam("fromKanban", "1");

    //Si l'on crée une fiche et que l'on clique sur "Enregistrer" il faut récupérer le fileid à jour
    if (nFileId == 0) {
        var nAltFileId = getNumber(getAttributeValue(oFileDiv, "fid"));
        if (nAltFileId > 0)
            nFileId = nAltFileId;
    }

    eEngineUpdater.AddOrSetParam('bTypeMail', isTypeMail);

    // Le FileId doit être passé à 0 si la fenêtre a été affichée via le bouton Transférer, afin que eEngine sache qu'il s'agit de créer un nouvel élément
    if (oWin.document.getElementById("mailForward") && oWin.document.getElementById("mailForward").value == "1")
        eEngineUpdater.AddOrSetParam('fileid', '0');
    else
        eEngineUpdater.AddOrSetParam('fileid', nFileId);

    // Le FileId doit être passé à 0  dans le cas d'envoie d'un email de test en unitaire
    //NHA : Correction bug #72930
    if (oWin.document.getElementById('mailIsTest') && oWin.document.getElementById('mailIsTest').value == "1") {
        eEngineUpdater.AddOrSetParam('fileid', '0');
    }

    // Duplication: Recup de l'id de la fiche à dupliquer
    if (eModFile != null && eModFile.getIframe() != null && eModFile.CallFrom == top.CallFromDuplicate) {
        eEngineUpdater.AddOrSetParam('fileid0', eModFile.fileId);
    }

    eEngineUpdater.AddOrSetParam('tab', nTab);

    var parentTab = 0;
    var parentFileId = 0;
    if (eModFile != null && typeof (eModFile) != "undefined" && eModFile.myOpenerDoc) {
        parentTab = getTabFrom(eModFile.myOpenerWin);
        parentFileId = getAttributeValue(eModFile.myOpenerDoc.getElementById("fileDiv_" + parentTab), "fid");
    }

    eEngineUpdater.AddOrSetParam('parenttab', parentTab);
    eEngineUpdater.AddOrSetParam('parentfileid', parentFileId);

    // HLA - On averti qu'on est en validation de fiche - Dev #45363
    eEngineUpdater.AddOrSetParam('onValideFileAction', '1');

    if (oWin.document.getElementById("COL_" + nTab + "_DN_" + nFileId + "_" + nFileId + "_0") || getAttributeValue(oWin.document.getElementById("fileDiv_" + nTab), "sms") == "1") {

        if (oWin.document.getElementById("COL_" + nTab + "_DN_" + nFileId + "_" + nFileId + "_0")) {
            eEngineUpdater.AddOrSetParam('mailDN', oWin.document.getElementById("COL_" + nTab + "_DN_" + nFileId + "_" + nFileId + "_0").value);
        }
        if (oWin.document.getElementById("COL_" + nTab + "_RT_" + nFileId + "_" + nFileId + "_0")) {
            eEngineUpdater.AddOrSetParam('mailRT', oWin.document.getElementById("COL_" + nTab + "_RT_" + nFileId + "_" + nFileId + "_0").value);
        }
        if (oWin.document.getElementById("COL_" + nTab + "_" + (getNumber(nTab) + 91) + "_" + nFileId + "_" + nFileId + "_0")) {
            eEngineUpdater.AddOrSetParam('mailPJ', oWin.document.getElementById("COL_" + nTab + "_" + (getNumber(nTab) + 91) + "_" + nFileId + "_" + nFileId + "_0").getAttribute("PjIds"));
        }
        if (oWin.document.getElementById('mailSaveAsDraft') && oWin.document.getElementById('mailSaveAsDraft').value == '1') {
            eEngineUpdater.AddOrSetParam('mailSaveAsDraft', "1");
        }
        if (oWin.document.getElementById('mailIsDraft') && oWin.document.getElementById('mailIsDraft').value == '1') {
            eEngineUpdater.AddOrSetParam('mailIsDraft', "1");
        }
        if (isTestMail) {
            eEngineUpdater.AddOrSetParam('mailIsTest', "1");
        }
        // Pour chaque champ de type Mémo trouvé sur la page, on envoie également le contenu de son éventuelle feuille de style personnalisée
        // Libre ensuite à la page effectuant le traitement derrière de gérer cette information ou non
        if (nsMain.hasMemoEditors(oWin.document)) {

            // Spécifique E-mail : envoi de l'information dans un paramètre plus simple d'accès
            var nameMemoEdit = ("edtCOL_" + nTab + "_" + (getNumber(nTab) + 8) + "_" + nFileId + "_" + nFileId + "_0");
            var oMemoEditor = nsMain.getMemoEditor(nameMemoEdit, oWin.document);
            var oMemoEditorCss = oMemoEditor.getCss();

            eEngineUpdater.AddOrSetParam('memoCSS_' + oMemoEditor.name, oMemoEditorCss);
            eEngineUpdater.AddOrSetParam('mailCSS', oMemoEditorCss);
        }
    }


    for (var i = 0; i < aFields.length; i++) {
        eEngineUpdater.AddOrSetField(aFields[i]);
    }

    if (afterValidate != "undefined" && afterValidate != null && (typeof (afterValidate) == "function" || typeof (afterValidate) == "object") && afterValidate.goFile) {
        eEngineUpdater.AddOrSetParam('callbackgofile', '1');
    }

    //Gestion des fields autorisée/bloquée suites a des applyrules 
    if (eModFile != null) {
        var oHV = eModFile.getIframe().document.querySelector("div#hv_" + nTab + " > input#ctrlId_" + nTab);
        if (oHV) {
            eEngineUpdater.AddOrSetParam("crtldescid", oHV.value);
        }

        if (eModFile.getParam("noloadfile") == "1") {
            eEngineUpdater.AddOrSetParam("noloadfile", "1");
        }
    }

    if (!chkExistsObligatFlds(aFields, null, null, oWin)) {
        if (eModFile != null && typeof (eModFile) != "undefined" && eModFile.getIframe() != null && typeof (eModFile.getIframe()) != "undefined")
            eEngineUpdater.ModalDialog = { oModFile: eModFile, modFile: eModFile.getIframe(), pupClose: bClose };
        else
            eEngineUpdater.ModalDialog = null;

        // #88 822 - Avant de fermer la fenêtre de création, on s'assure qu'il ne reste pas un waiter sur la racine, qui deviendrait orphelin à la fermeture si initié par eModFile. (Le setWait(false) final de eEngine.UpdateTreatmentReturn n'est plus exécutable une fois la fenêtre fermée)
        if (bClose) {
            var fctCloseModal = function () {
                var oParamGoTabList = {
                    to: 3,
                    nTab: top.getTabFrom(),
                    context: "eMain.validateFile"
                }

                if (top && typeof (top.setWait) == "function")
                    top.setWait(false, undefined, undefined, isIris(top.getTabFrom()), oParamGoTabList);
                eModFile.hide();
            };
            // Si aucune fonction post-validation n'est définie, on se contente de fermer le waiter, puis la fenêtre
            if (typeof (afterValidate) != 'function') {
                afterValidate = fctCloseModal;
            }
            // Sinon, on ajoute à la fonction appelée en paramètre la fonction de fermeture du waiter et de la fenêtre
            else {
                afterValidate =
                    (
                        function (afterValidateP, myFields) {
                            return function (params) {
                                fctCloseModal();
                                afterValidateP(params, myFields);//GCH #34808 - Ajout des fields afin de pouvoir avoir des retours spécifiques à leur contenu.
                            }
                        }
                    )(afterValidate, aFields);
            }
        }
        //si on n'a pas demandé à fermer la fenêtre, on clôture le setWait() en cours dans le cas du mail de test
        //TOCHECK : à faire dans les autres cas que le mail de test, aussi ?
        else if (isTestMail) {
            //on ajoute à la fonction appelée en paramètre la fonction de fermeture.
            if (typeof (afterValidate) != 'function') {
                afterValidate = function () {
                    setWait(false);
                };
            }
            else {
                afterValidate =
                    (
                        function (afterValidateP, myFields) {
                            return function (params) {
                                setWait(false);
                                afterValidateP(params, myFields);//GCH #34808 - Ajout des fields afin de pouvoir avoir des retours spécifiques à leur contenu.
                            }
                        }
                    )(afterValidate, aFields);


            }
        }

        if (typeof (afterValidate) == 'function')
            eEngineUpdater.SuccessCallbackFunction = afterValidate;

        // cas de la duplication unitaire :  on transmet les signets à dupliquer
        if (oWin.document.getElementById("bkms")) {
            eEngineUpdater.AddOrSetParam('bkmids', oWin.document.getElementById("bkms").value);
        }

        eEngineUpdater.UpdateLaunch();
    }
}
function launchChkDbl(nTab, nFileId, modFile, fctValidate) {

    nTab = getNumber(nTab);

    if (nFileId > 0 || (nTab != TAB_PP && nTab != TAB_PM)) {
        fctValidate();
        return;
    }

    var upd = new eUpdater('mgr/eChkDblMgr.ashx', 0);
    upd.addParam("Tab", nTab, "post");

    var oField = modFile.getIframe().document.getElementById("COL_" + nTab + "_" + (nTab + 1) + "_0_0_0");

    if (oField == null) {
        fctValidate();
        return;
    }
    var fld = getFldEngFromElt(oField);
    upd.addParam((nTab + 1), fld.newValue, "post");

    if (nTab == TAB_PP) {
        for (var i = 2; i <= 3; i++) {
            oField = modFile.getIframe().document.getElementById("COL_" + nTab + "_" + (nTab + i) + "_0_0_0");
            if (oField != null) {
                fld = getFldEngFromElt(oField);
                upd.addParam((nTab + i), fld.newValue, "post");
            }
        }
    }

    upd.send(function (oRes) { chkExistsDbl(oRes, fctValidate); });
}

function chkExistsDbl(oRes, fctValidate) {
    var nbHomonyms = oRes.getElementsByTagName("rec").length;
    var nTab = getNumber(GetText(oRes.getElementsByTagName("tab")[0]));
    var sIconStyle = GetText(oRes.getElementsByTagName("iconstyle")[0]);
    sIconStyle = "style='" + sIconStyle.replace(/'/g, "\\'") + "'";
    if (nbHomonyms > 0) {
        var details = top._res_918 + " : ";
        for (var i = 0; i < nbHomonyms; i++) {
            var sShVCard = nTab == TAB_PP ? "onmouseout='shvc(this, 0)' onmouseover='shvc(this, 1, " + getAttributeValue(oRes.getElementsByTagName("rec")[i], "id") + ")'" : "";
            details += "<p class='lstHmnm' " + sShVCard + " " + sIconStyle + ">" + getAttributeValue(oRes.getElementsByTagName("rec")[i], "label") + "</p>";
        }
        details += "<BR>" + top._res_594;
        top.eConfirm(eMsgBoxCriticity.MSG_QUESTION, top._res_602, nbHomonyms > 0 ? top._res_917.replace("<COUNT>", nbHomonyms) : top._res_593, details, 500, 300, fctValidate, null, false, true);
    }
    else
        fctValidate();
}

function launchChkTplDblFromModal(eModFile, fctValidate) {
    if (eModFile == null || eModFile.getIframe() == null) {
        fctValidate();
        return;
    }


    var tplDoc = eModFile.getIframe().document;
    var fileDiv = tplDoc.getElementById("fileDiv_" + eModFile.tab);
    var nFileId = getNumber(fileDiv.getAttribute("fid"));
    if (nFileId > 0) {
        fctValidate();
        return;
    }
    var nPPID = 0, nEVTID = 0;
    var bInterPP = false, bAdrJoin = false, bInterEvt = false;
    var LnkIds = tplDoc.getElementById("lnkid_" + eModFile.tab).value.split(";");
    for (var i = 0; i < LnkIds.length; i++) {
        var param = LnkIds[i].split("=");
        switch (param[0]) {
            case "200":
                bInterPP = true;
                nPPID = getNumber(param[1]);
                break;
            case "300":
                break;
            case "400":
                bAdrJoin = true;
                break;
            default:
                bInterEvt = true;
                nEVTID = getNumber(param[1]);
                break;
        }
    }

    if (!(bInterPP && bAdrJoin && bInterEvt) || (nPPID == 0 || nEVTID == 0)) {
        fctValidate();
        return;
    }

    launchChkTplDbl(eModFile.tab, nPPID, nEVTID, fctValidate);
}

function launchChkTplDbl(nTab, nPPID, nEVTID, fctValidate) {
    nTab = getNumber(nTab);

    if (nTab == 0 || nPPID == 0 || nEVTID == 0) {
        fctValidate();
        return;
    }

    var upd = new eUpdater('mgr/eChkTplDblMgr.ashx', 1);
    upd.addParam("tab", nTab, "post");
    upd.addParam("ppid", nPPID, "post");
    upd.addParam("evtid", nEVTID, "post");

    upd.send(function (oRes) { chkExistsTplDbl(oRes, fctValidate); });
}


function chkExistsTplDbl(oRes, fctValidate) {
    var result = JSON.parse(oRes);
    if (!result.IsDbl) {
        fctValidate();
    }
    else {
        eConfirm(eMsgBoxCriticity.MSG_QUESTION, result.Title, result.Msg, result.MsgDetail, undefined, undefined, fctValidate)
    }

    //test
}

function chkExistsObligatFlds(aFields, nTab, nFileId, oWin) {

    //BSE #51764 , ne pas contrôler les champs obligatoires sur la table campgane mail en consultation 
    if (nTab == 106000)
        return false;

    if (nFileId == 0 && oWin == window)
        return false;

    if (!oWin)
        oWin = window;

    if (typeof (aFields) == "undefined" || aFields == null) {
        if (nTab > 0 && nFileId >= 0)
            aFields = getFieldsInfos(nTab, nFileId);
        else
            aFields = getObligatFields(oWin.document);
    }
    var ObligatFields = new Array();
    var sFirstObgId = "";

    // on liste les champs obligatoires non renseignés
    // et on repère le premier pour mettre le focus dessus
    for (var i = 0; i < aFields.length; i++) {

        if (aFields[i].obligat && (aFields[i].newValue == "" || aFields[i].newValue == null)) {
            ObligatFields.push(aFields[i]);
            if (sFirstObgId == "") {
                sFirstObgId = aFields[i].cellId;
            }
        }

    }
    // on ouvre le titre séparateur dans lequel se trouve le peremier champ
    if (sFirstObgId != "") {
        var oFirstObgElt = oWin.document.getElementById(sFirstObgId);
        var oTRElt = oFirstObgElt;
        var oSepPageId;
        var oSepPageElt;
        while (oTRElt.tagName != "TR" && oTRElt.parentElement) {
            oTRElt = oTRElt.parentElement;
        }

        oSepPageId = getAttributeValue(oTRElt, "epagesep");

        if (oTRElt.tagName == "TR" && oSepPageId != "") {
            oSepPageElt = oWin.document.getElementById(oSepPageId);
            if (oSepPageElt) {
                var isSepOpen = (getAttributeValue(oSepPageElt, "eopen") == "1")
                if (!isSepOpen) {
                    oWin.OpenCloseSep(oSepPageElt, true);
                }
            }
        }


    }

    // s'il y a des champs obligatoires non renseignés, on affiche un message d'erreur
    // et en retour de celui-ci on met le focus sur le premier de ceux-ci
    if (ObligatFields.length > 0) {

        var strObligatFields = '';
        for (var i = 0; i < ObligatFields.length; i++) {
            strObligatFields += ObligatFields[i].lib.replace(/\n/g, "") + '<br>';
        }

        eAlert(0, top._res_372, top._res_1268 + " : ", strObligatFields, null, null, function () {

            // Test de la disponibilité du champ - s'il est masqué, son scrollHeight sera a 0 et il ne sera pas focusable
            if (oFirstObgElt && oFirstObgElt.scrollHeight && oFirstObgElt.scrollHeight > 0) {
                var sAction = getAttributeValue(oFirstObgElt, "eaction");
                oFirstObgElt.focus();
                if (sAction == '' || sAction != 'LNKGOFILE') {
                    oWin.setFldEditor(oFirstObgElt);
                }
            }

        });

        return true;
    }
    else {
        return false;
    }
}
var bHistoAndCreatePlanning = null;
var bClosePlanning = null;

function validatePlanningFile(nTab, nFileId, modal, bClose, bHistoAndCreate, afterSaveFunction, bNoLoadFile) {
    var oFrm = modal.getIframe();

    bHistoAndCreatePlanning = bHistoAndCreate;
    bClosePlanning = bClose;

    if (typeof bNoLoadFile === "undefined")
        bNoLoadFile = false;

    var usersDescId = Number(nTab) + 92;
    var usersFldId = "COL_" + nTab + "_" + usersDescId + "_" + nFileId + "_" + nFileId + "_0";

    var bConflicted = false;
    if (oFrm.document.getElementById("conflictIndicator"))
        bConflicted = oFrm.document.getElementById("conflictIndicator").value == "1";

    modal.addParam("noloadfile", bNoLoadFile ? "1" : "0");

    if (bConflicted) {
        eConfirm(1, '', top._res_6305, '', null, null
            , (function (paramnTab, paramnFileId, parambClose, parammodal, paramafterSaveFunction) {
                return function () {
                    savePlanning(paramnTab, paramnFileId, parambClose, parammodal, paramafterSaveFunction);
                }
            })(nTab, nFileId, bClose, modal, afterSaveFunction));
    }
    else
        savePlanning(nTab, nFileId, bClose, modal, afterSaveFunction);
}

function savePlanning(nTab, nFileId, bClose, modal, afterSaveFunction) {


    top.setWait(true);

    var oFrm = modal.getIframe();

    nTab = getNumber(nTab);
    var aFields = oFrm.getFieldsInfos(nTab, nFileId);

    var eEngineUpdater = new eEngine();


    eEngineUpdater.SuccessCallbackFunction = function () {
        top.setWait(false);
    };
    eEngineUpdater.ErrorCallbackFunction = function () {
        top.setWait(false);
    };

    eEngineUpdater.Init();

    if (typeof (nFileId) == "undefined")
        nFileId = 0;

    eEngineUpdater.AddOrSetParam('tab', nTab);
    eEngineUpdater.AddOrSetParam('fileId', nFileId);

    // HLA - On averti qu'on est en validation de fiche - Dev #45363
    eEngineUpdater.AddOrSetParam('onValideFileAction', '1');

    //Gestion des fields autorisée/bloquée suites a des applyrules
    if (eModFile != null) {
        var oHV = eModFile.getIframe().document.querySelector("div#hv_" + nTab + " > input#ctrlId_" + nTab);
        if (oHV) {
            eEngineUpdater.AddOrSetParam("crtldescid", oHV.value);
        }
    }

    if (afterSaveFunction != "undefined" && typeof (afterSaveFunction) == "function" && afterSaveFunction.goFile) {
        eEngineUpdater.AddOrSetParam('callbackgofile', '1');
    }

    var sView = getCurrentView(document);

    //KHA le 18/11/2014 le reload du planning est désormais géré directemnt par eEngine.js
    //if (sView == "CALENDAR" || sView == "CALENDAR_LIST")
    //    eEngineUpdater.ReloadNothing = true;

    var ObligatFields = new Array();

    if (modal != null && typeof (modal) != "undefined" && modal.getIframe() != null && typeof (modal.getIframe()) != "undefined") {
        eEngineUpdater.ModalDialog = { oModFile: modal, modFile: modal.getIframe(), pupClose: bClose, bPlanning: true, docTop: top };
        if (modal.getParam("noloadfile") == "1") {
            eEngineUpdater.AddOrSetParam("noloadfile", "1");
        }
    }
    else
        eEngineUpdater.ModalDialog = null;

    var oDoc = eEngineUpdater.ModalDialog != null ? oFrm.document : document;
    var histoDescId = getNumber(getAttributeValue(oDoc.getElementById("hdid_" + nTab), "value"));

    if (!histoDescId || histoDescId == 0)
        histoDescId = nTab + 1;


    for (var i = 0; i < aFields.length; i++) {

        if (bHistoAndCreatePlanning && getNumber(aFields[i].descId) == Math.abs(histoDescId)) {
            if (histoDescId > 0)
                aFields[i].newValue = "1";
            else
                aFields[i].newValue = "0";

        }

        if (aFields[i].obligat && aFields[i].newValue == "") {
            ObligatFields.push(aFields[i]);
        }

        if (bGblFromApplyRuleOnBlank && aFields[i].readOnlyBlank) {
            aFields[i]["readOnly"] = false;
        }

        //Si on est en création ou en modification d une fiche planning et que le champs n est pas en lecture seule
        //alors on ajoute le champs aux controles et a l enregistrement
        if (nFileId == 0 || (nFileId != 0 && !aFields[i].readOnly)) {
            eEngineUpdater.AddOrSetField(aFields[i]);
        }

    }

    if (chkExistsObligatFlds(aFields, null, null, modal.getIframe())) {
        top.setWait(false);
    }

    else {
        eEngineUpdater.SuccessCallbackFunction = function (engResult) {
            top.setWait(false);
            onPlanningValidateTreatment(engResult, modal, bClosePlanning, bHistoAndCreatePlanning, afterSaveFunction);

        };

        eEngineUpdater.UpdateLaunch();
    }
}


function onPlanningValidateTreatment(oRes, plgModal, bClosePlg, bHistoAndCreatePlg, afterSaveFunction) {

    var bClose = false;
    var bReopen = false;

    if (typeof (afterSaveFunction) == "function")
        afterSaveFunction();

    if (plgModal == null)
        return;

    if (plgModal != null && bClosePlg == true) {
        bClose = true;

        if (bHistoAndCreatePlg) {
            bReopen = true;
        }
    }



    if (bClose) {
        top.setWait(false);
        if (typeof (afterSaveFunction) != "function") {
            if (bReopen) {

                if (bHistoAndCreatePlg) {
                    try {
                        //Type du planning
                        var nType = plgModal.getIframe().document.getElementById("COL_" + plgModal.tab + "_" + (1 * plgModal.tab + 83)).querySelector("input[ename='" + "COL_" + plgModal.tab + "_" + (1 * plgModal.tab + 83) + "']").value;
                        if (oLinksInfo)
                            oLinksInfo.type = nType;
                    }
                    catch (e) {

                    }
                }

                if (plgModal && typeof plgModal.hide == "function") {
                    plgModal.hide();
                }

                var myPlanning = showTplPlanning(plgModal.tab, 0, null, top._res_31);
                //On garde la fenêtre ouverte pour gérer les reload en arrière plan si la fiche contient des règles
                myPlanning.NoGlobalCLose = bHistoAndCreatePlg

                return;
            }
        }
        if (plgModal && typeof plgModal.hide == "function") {
            plgModal.hide();
        }
    }
    else {
        if (typeof (afterSaveFunction) != "function")
            plgModal.getIframe().loadFile(oRes.tab, oRes.fid);
    }
}



// Ouvre la fenetre avec le contenu avant l'impression
// OBJET      : Function déclenchée au clic sur l'icone d'impression de la fiche
//              Cette fonction va appeler ePrintFile --> renderer pour rendre un contenu HTML à integrer dans une page
// AUTHEUR    : NBA 
// DATE       : 08/2012
// PARAMETRES :
function printFile() {
    var _nfileID = document.getElementById("fileDiv_" + nGlobalActiveTab).getAttribute("fid");
    var _nTab = nGlobalActiveTab;

    var _url = 'ePrintFile.aspx';
    var _Params = '?nTab=' + _nTab + '&nfileID=' + _nfileID;
    var _options = '';
    window.open(_url + _Params, "_blank");
}

// Lancement de l'impression du navigateur
function Print() {
    window.print();
}

//---------- Gestion des Annexes --------------

// Modaldialog pour la gestion des annexe
var oModalPJAdd;

// OBJET      : Function déclenchée au clic sur l'icone d'ajout d'annexe depuis une fiche planning ou template
//              Cette fonction va appeler l'assistant d'ajout de pièces jointes
// AUTHEUR    : NBA 
// DATE       : 12/11/2012
//  nSourceTpl :
//  nIdOfSpan : 
function showPJFromTpl(nSourceTpl, nIdOfSpan, bEmailing) {

    var _nFileID = 0;
    var fileDiv = document.getElementById("fileDiv_" + nGlobalActiveTab);
    if (typeof (fileDiv) == "undefined") {
        alert('Identifiant de la fiche incorrect');
        return;
    }
    else
        _nFileID = fileDiv.getAttribute('fid');

    var _nTab = nGlobalActiveTab;
    var _idsOfPj = '';
    var _pjSpan = null;
    var _pjListe = null;
    var _pjlisteDiv = null;

    // Récupération du span conernant les PJs
    if (typeof (nIdOfSpan) != "undefined") {
        if (nIdOfSpan != "") {
            var oPj = document.getElementById(nIdOfSpan);   //Cas du mail
            if (oPj == null)
                oPj = parent.document.getElementById(nIdOfSpan);    //Cas de tous les autres mode PopUp
            if (typeof (oPj) != "undefined" && oPj != null) {
                _pjSpan = oPj;
            }
        }
    }
    var ndescIdPJ = (_nTab * 1) + 91;
    var oPjInfo = document.getElementById("COL_" + _nTab + "_" + ndescIdPJ + "_" + _nFileID + "_" + _nFileID + "_0");
    if (oPjInfo != null) {
        _idsOfPj = getAttributeValue(oPjInfo, 'pjids');
    }

    // div qui contient le span de la Liste des noms des pièces jointes pour les tplMail
    if (document.getElementById('divlstPJMail')) {
        _pjlisteDiv = document.getElementById('divlstPJMail');
    }
    // Span qui contient la Liste des noms des pièces jointes pour les tplMail
    if (document.getElementById('spanlstPj')) {
        _pjListe = document.getElementById('spanlstPj');
    }
    //    var _parentModal;
    //    if (typeof (_parentIframeId) != "undefined")
    //        _parentModal = _parentIframeId;

    oModalPJAdd = showPJDialog(_nTab, _nFileID, nSourceTpl, _pjSpan, bEmailing, _idsOfPj);

    oModalPJAdd.listePj = _pjListe;
    oModalPJAdd.divPjList = _pjlisteDiv;

}


//GCH : attention pjSpan peut être null !
function showPJDialog(nTab, nFileId, nSourceTpl, pjSpan, bEmailing, idsOfPj) {


    var _titreFen = top._res_6316;
    var _width = 927;
    var _height = 550;

    oModalPJAdd = new eModalDialog(_titreFen, 0, 'ePjAddFromTpl.aspx', _width, _height, "modalPJAdd");

    oModalPJAdd.addParam("nTab", nTab, "post");
    oModalPJAdd.addParam("nFileID", nFileId, "post");
    //   oModalPJAdd.addParam("_sourceIframeId", _parentModal, "post");

    oModalPJAdd.addParam("iDsOfPj", idsOfPj, "post");

    oModalPJAdd.addParam("width", _width, "post");
    oModalPJAdd.addParam("height", _height, "post");

    if (idsOfPj && idsOfPj.length > 0 && idsOfPj != "0")
        oModalPJAdd.addParam("viewtype", "checkedonly", "post");


    oModalPJAdd.fileId = nFileId;
    oModalPJAdd.tab = nTab;

    //Ajout de pj depuis ckeditor de l'assistant d'emailing
    if (bEmailing != "undefined" && bEmailing && oMailing != null) {
        oModalPJAdd.addParam("parentEvtFileId", oMailing._nParentFileId, "post");
        oModalPJAdd.addParam("parentEvtTabId", oMailing._nParentTabId, "post");
        oModalPJAdd.addParam("selectonclick", "1", "post");
        oModalPJAdd.oMailing = oMailing;
    }
    else if (nSourceTpl == "tpl" || nSourceTpl == "tplmail") {

        oModalPJAdd.addParam("fromtpl", "1", "post");
        // div qui contient le span de la Liste des noms des pièces jointes pour les tplMail
        var forward = document.getElementById('mailForward');
        if (forward)
            oModalPJAdd.addParam("mailForward", forward.value, "post");

        AddTplContext(oModalPJAdd);
    }
    else if (nSourceTpl == "mailtemplate") {
        oModalPJAdd.addParam("frommailtemplate", "1", "post");
    }

    oModalPJAdd.pjSpan = pjSpan;
    oModalPJAdd.sourcePJ = nSourceTpl;

    oModalPJAdd.onHideFunction = function () { top.oModalPJAdd = null; };
    oModalPJAdd.ErrorCallBack = launchInContext(oModalPJAdd, oModalPJAdd.hide);

    oModalPJAdd.show();
    if (nSourceTpl == "tpl" || nSourceTpl == "tplmail") {
        oModalPJAdd.addButton(top._res_29, cancelPJAdd, "button-gray", null, "cancel");
        oModalPJAdd.addButton(top._res_5003, AddPjfromTpl, "button-green"); //Valider    return oModalPJAdd;
    }
    else {
        //  oModalPJAdd.addButton(top._res_29, cancelPJAdd, "button-gray",null);
        oModalPJAdd.addButton(top._res_5003, AddPjfromTpl, "button-green", "", "cancel"); //Valider    return oModalPJAdd;
    }

    top.oModalPJAdd = oModalPJAdd;
    return oModalPJAdd;
}

function AddTplContext(oModalPJAdd) {
    if (getCurrentView(document).indexOf("FILE_") == 0) {
        var ednType = getAttributeValue(document.getElementById("fileDiv_" + nGlobalActiveTab), "edntype");
        var parentDiv = document.getElementById("divPrt_" + oModalPJAdd.tab);

        if (parentDiv == null)
            return;

        var aTdParentHead = parentDiv.querySelectorAll("td[prt='1']");
        for (var i = 0; i < aTdParentHead.length; i++) {
            var oTr = aTdParentHead[i].parentElement;
            var inptValue = oTr.querySelector("input[ename=" + aTdParentHead[i].id + "]");
            var dbv = getAttributeValue(inptValue, "dbv");
            var did = getAttributeValue(aTdParentHead[i], "did");

            switch (getNumber(did)) {
                case TAB_PP:
                    oModalPJAdd.addParam("ppid", dbv, "post");
                    break;
                case TAB_PM:
                    oModalPJAdd.addParam("pmid", dbv, "post");
                    break;
                case TAB_ADR:
                    oModalPJAdd.addParam("adrid", dbv, "post");
                    break;
                default:
                    oModalPJAdd.addParam("parentEvtFileId", dbv, "post");
                    oModalPJAdd.addParam("parentEvtTabId", did, "post");
                    break;

            }
        }

    }

}


// Validation de la fenêtre d'ajout d'annexez à partir de templates
function AddPjfromTpl() {
    // Récuperation de l'iframe de la modaldialog
    var iframePJ = oModalPJAdd.getIframe(); // Iframe d'ajout de PJ
    var pjSpanTPL = oModalPJAdd.pjSpan; // Pointeur vers l'Input Span contenant le nombre de PJ et les Ids


    var ndescIdPJ = getNumber(nGlobalActiveTab) + 91;
    var pjInfo = document.getElementById("COL_" + oModalPJAdd.tab + "_" + ndescIdPJ + "_" + oModalPJAdd.fileId + "_" + oModalPJAdd.fileId + "_0"); // Pointeur vers l'Input Caché des info de Pj
    // Dans le cas de l'assistant d'emailing généré avec fileId 0 puis enregistré sans le fermer, dans ce cas l'input a toujours fileid=0
    if (pjInfo == null)
        pjInfo = document.getElementById("COL_" + oModalPJAdd.tab + "_" + ndescIdPJ + "_0_0_0");

    // Pointeur vers la div conteanant le span de la liste des pjs
    var pjDivLstMail = oModalPJAdd.divPjList;
    // Span liste pj
    // Pointeur vers le span de la liste de PJ pour les mails
    var pjSpanLst = oModalPJAdd.listePj;
    var nsource = oModalPJAdd.sourcePJ;

    if (nsource == "mailtemplate" || nsource == "mailingtemplate") {
        pjInfo = document.getElementById("btnPJ");
    }

    //KHA le 11/06/15 : refonte des pj depuis un mail unitaire
    // Pour les template de type mail on va récupérer la liste des PJ pour l'afficher
    //if (nsource == 'tplmail') {
    //    if (pjDivLstMail != null) {

    //        // Tableau associatif fabriqué en C# la key correspond au pjid et le contenu au nom du fichier à afficher
    //        var nArray = iframePJ.document.ListePj;

    //        if (nArray.length > 0 && oModalPJAdd.divPjList)
    //            pjDivLstMail.style.display = "block";
    //        else
    //            pjDivLstMail.style.display = "none";

    //        pjSpanLst.innerHTML = '';
    //        var nLink = '';

    //        // le key correspond a la PJId qui sera utilisée pour le lien de suppression
    //        for (var key in nArray) {
    //            if (pjSpanLst.innerHTML != '')
    //                pjSpanLst.innerHTML = pjSpanLst.innerHTML + ', ';
    //            nLink = "<a onclick=\"DeletePJ('" + nGlobalActiveTab + "', '" + oModalPJAdd.fileId + "', '" + key + "', true," + "'mail');\">&nbsp;<img class=\"imgdelpj\" src=\"ghost.gif\"></a> ";
    //            pjSpanLst.innerHTML = pjSpanLst.innerHTML + nArray[key] + nLink;
    //        }

    //    }
    //}
    var inptIdsPJ = iframePJ.document.getElementById('idspj');
    if (pjInfo != null && inptIdsPJ) {

        // Liste des PjIds de la liste des annexes validées
        var idsOfPjs = inptIdsPJ.value;

        // Mise à jour des PjIds de Pj liées à la fiche que si on est mode création et que la liste des pjids a été modifié (pour l'engine)
        if (idsOfPjs != getAttributeValue(pjInfo, "PjIds") || nsource == 'tplmail') {
            pjInfo.setAttribute("PjIds", idsOfPjs);
            var nbPj = iframePJ.document.getElementById('nbpj').value;
            pjInfo.setAttribute("nbpj", nbPj);
            pjInfo.textContent = '(' + nbPj + ')'
            // Mise à jour du nombre de PJ, selon si on est sur une fenêtre de type E-mail ou non
            if (pjSpanTPL != null) {
                if (document.getElementById('divlstPJMail')) {
                    pjSpanTPL.innerHTML = top._res_6349.replace('<NUMBER>', nbPj);
                }
                else {
                    pjSpanTPL.innerHTML = "(" + nbPj + ")";
                }
            }

            var dspCntPj = document.getElementById("dspCntPj_" + oModalPJAdd.tab);
            if (dspCntPj)
                dspCntPj.innerHTML = "(" + nbPj + ")";

        }

        //mise a jour des pj Emailing
        if (nsource == 'tplmailing') {

            var _oMailing = oModalPJAdd.oMailing;
            _oMailing.SetPjIds(idsOfPjs);

            var selectedPjId = oModalPJAdd.getIframe().SelectedPjId;
            if (typeof (selectedPjId) != 'undefined' && selectedPjId > 0) {
                var memo = _oMailing._oMemoEditor;
                memo.LoadPjInfoAndInsertPjLink(selectedPjId);
            }
        }
    }

    if (nsource == 'mailingtemplate') {

        var selectedPjId = oModalPJAdd.getIframe().SelectedPjId;
        // #71 875 - Ciblage de l'éditeur approprié sur la fenêtre des modèles de mail - CKEditor (selon si grapesjs est affiché ou non)		
        var targetMemoObject = null;
        // Cas où 2 éditeurs sont affichés (Firefox, Chrome, Safari et licence E17) : CKEditor est le second nommé eTplMailDialogEditorObjectCKe
        if (typeof (eTplMailDialogEditorObjectCKe) != 'undefined' && eTplMailDialogEditorObjectCKe != null && eTplMailDialogEditorObjectCKe.enableTemplateEditor === false)
            targetMemoObject = eTplMailDialogEditorObjectCKe;
        // Cas où 1 seul éditeur est affiché (IE, Edge ou licence XRM) : CKEditor est le premier et seul, nommé eTplMailDialogEditorObject
        else if (typeof (eTplMailDialogEditorObject) != 'undefined' && eTplMailDialogEditorObject != null && eTplMailDialogEditorObject.enableTemplateEditor === false)
            targetMemoObject = eTplMailDialogEditorObject;
        if (selectedPjId != 'undefined' && selectedPjId > 0 && targetMemoObject) {
            targetMemoObject.LoadPjInfoAndInsertPjLink(selectedPjId);
        }
    }

    if (inptIdsPJ && (nsource == "tpl" || nsource == "tplmail")) {
        // Met à jour les pj à partir des éléments sélectionnés dans les parents
        var updFromParent = new eUpdater("mgr/ePjManager.ashx", 0);
        updFromParent.ErrorCallBack = function () { };
        updFromParent.addParam("FileId", oModalPJAdd.fileId, "post");
        updFromParent.addParam("nTab", oModalPJAdd.tab, "post");
        updFromParent.addParam("action", "updatefromparent", "post");
        updFromParent.addParam("pjIds", inptIdsPJ.value, "post");
        updFromParent.fileId = oModalPJAdd.fileId;
        updFromParent.tab = oModalPJAdd.tab;
        AddTplContext(updFromParent);

        updFromParent.send();

    }

    //applyRuleOnBlank(nGlobalActiveTab, null, oModalPJAdd.fileId);

    oModalPJAdd.hide();
}

// #58059 On sélectionne la ligne qui vient d'être ajoutée pour les template mail et les campaigne mail
function selectPjLine(pjTab, pjId) {

    if (pjId == 0)
        return;

    var table = document.getElementById("mt_" + pjTab);
    if (table != null) {
        var selected = table.querySelector("tr[eid='" + pjTab + "_" + pjId + "']");
        if (selected) {
            highlightLineIfFromList(selected);
            if (typeof (SelectedPjId) != "undefined")
                SelectedPjId = pjId;
        }
    }
}

function showPJFromList(oElement) {
    var nTab = 0;
    var nFileId = 0;
    try {
        var ref = JSON.parse(getAttributeValue(oElement, "ref"));
        nTab = ref.t;
        nFileId = ref.i;

        showPJDialog(nTab, nFileId, 'tpl', oElement, false, '')

    }
    catch (e) { }

}
// OBJET      : Function déclenchée au clic sur l'icone d'ajout d'annexe depuis un signet
//              Cette fonction va appeler l'assistant d'ajout de pièces jointes
// AUTHEUR    : NBA 
// DATE       : 08/2012

//memoId : depuis un ckeditor
function showAddPJ(strMemoId, afterHideFunction, size = { width:500 , height:500 }) {
    var _nFileID = document.getElementById("fileDiv_" + nGlobalActiveTab).getAttribute('fid');
    var _nTab = nGlobalActiveTab;
    var _width = size.width;
    var _height = size.height;
    var _action = 'init';
    //Titre de la fenetre
    var _titreFen = top._res_6316 + ' ' + top._res_270.toLowerCase() + ' ' + top._res_6315;

    if (top && top.oModalPJAdd)
        top.oModalPJAdd.hide();

    oModalPJAdd = new eModalDialog(_titreFen, 0, 'ePjAdd.aspx', _width, _height, "oModalPJAdd");
    oModalPJAdd.addParam("nTab", _nTab, "post");
    oModalPJAdd.addParam("nFileID", _nFileID, "post");
    oModalPJAdd.addParam("action", _action, "post");
    oModalPJAdd.onHideFunction = function () { top.oModalPJAdd = null; if (typeof afterHideFunction == 'function') afterHideFunction(); };


    if (strMemoId != "undefined" && strMemoId != null)
        oModalPJAdd.addParam("memoid", strMemoId, "post");

    // En cas d'erreur de chargement des paramètres de la page, on ferme la fenetre
    oModalPJAdd.ErrorCallBack = launchInContext(oModalPJAdd, oModalPJAdd.hide);
    oModalPJAdd.show();
    oModalPJAdd.addButton(top._res_29, cancelPJAdd, "button-gray", "", "cancel");
    oModalPJAdd.addButton(top._res_5003, function () { AddPj() }, "button-green");

    top.oModalPJAdd = oModalPJAdd;
}

function cancelPJAdd() {
    oModalPJAdd.hide();
}

// Rafraichit la liste après l'ajout d'une annexe
function reloadBKMPJ() {

    if (getPreventLoadBkmList())
        return;

    var nDescIdAnnex = getNumber(nGlobalActiveTab) + 91;
    loadBkm(nDescIdAnnex);
}

// Retourne la value d'un élément avec son ID
function getValue(_idelement) {

    if (_idelement != '' && document.getElementById(_idelement))
        return document.getElementById(_idelement).value;
    else
        return '';
}

function trim(strTrim) {
    return strTrim.replace(/(^\s*)|(\s*$)/g, '');
}

// Demande de vérification que les PJ n'existent pas déjà
function callCheckPjExists(oFilesInfos, onSuccessFct, onValidNewNameFct) {
    var upd = new eUpdater("mgr/eCheckPJExists.ashx", 0);
    upd.addParam("files", JSON.stringify(oFilesInfos), "post");
    upd.addParam("action", "0", "post");
    upd.send(function (oRes) { checkPjExists(oRes, onSuccessFct, onValidNewNameFct, oFilesInfos); });
}


function checkPjExists(oRes, onSuccessFct, onValidNewNameFct, oFilesInfos) {
    if (oModalPJAdd) {
        // Récuperation de l'iframe de la modaldialog
        srcDocument = oModalPJAdd.getIframe().document;
    }
    else
        srcDocument = document;

    // Si les PJ n'existent pas, on lance la fonction onSuccessFct
    if (oRes != null && getXmlTextNode(oRes.getElementsByTagName("success")[0]) == "1") {
        if (typeof (onSuccessFct) == "function") {
            onSuccessFct(oFilesInfos);
        }
        else
            launchAddPj(srcDocument);
        return;
    }

    // Si au moins une des PJ existe, on affiche la modale
    var oJson = [];
    var xmlFiles = oRes.getElementsByTagName("file");
    for (var i = 0; i < xmlFiles.length; i++) {
        if (getXmlTextNode(xmlFiles[i].getElementsByTagName("success")[0]) == "1") {
            continue;
        }
        var filename = getAttributeValue(xmlFiles[i], "id");
        var sSuggestedName = getXmlTextNode(xmlFiles[i].getElementsByTagName("suggestedname")[0]);
        oJson.push({ filename: filename, saveas: sSuggestedName, action: 0 });
    }

    var fileid;
    //var modalPJAdd = top.eTools.GetModal("modalPJAdd");
    if (oModalPJAdd) {
        fileid = oModalPJAdd.fileId;
    }
    else if (top.eTools.GetModal("modalPJAdd")) {
        fileid = top.eTools.GetModal("modalPJAdd").fileId;
    }
    else {
        // #31 762 - Cas de l'ajout d'une PJ depuis le champ Mémo d'un fichier E-mail en cours d'édition (Assistant d'e-mailing), et peut-être d'autres
        // Si l'ajout de PJ renvoie une erreur car le nom de fichier est trop long, on fait appel à checkPjExists() pour obtenir un nom de fichier utilisable
        // Or, dans certains contextes comme celui-ci, on ne vient pas d'une fiche (eFile) mais d'une liste (eList), GetCurrentFileId() est alors indéfinie.
        // La fonction GetCurrentFileId() a donc été déplacée dans eMain, au même titre que imgGetCurrentFileId(). Elle renverra ainsi vide dans ce type de cas
        // Dans ce cas de figure, on passera la variable fileid à vide à eChecKPJExists, qui aura donc uniquement pour consigne de renvoyer un nom de fichier valide
        if (typeof (top.GetCurrentFileId) != "undefined")
            fileid = top.GetCurrentFileId(top.nGlobalActiveTab);
    }

    var modalPJChecker = new eModalDialog(getXmlTextNode(oRes.getElementsByTagName("title")[0]), 0, "mgr/eCheckPJExists.ashx", 600, 400, "modalPJChecker");
    modalPJChecker.addParam("action", "1", "post");
    modalPJChecker.addParam("files", JSON.stringify(oJson), "post");
    modalPJChecker.addParam("tab", top.nGlobalActiveTab, "post");
    modalPJChecker.addParam("fileid", fileid, "post");
    modalPJChecker.addParam("description", getXmlTextNode(oRes.getElementsByTagName("description")[0]), "post");
    modalPJChecker.show();
    modalPJChecker.addButton(top._res_29, function () { cancelNewPjName(modalPJChecker); }, "button-gray");
    modalPJChecker.addButton(top._res_28, function () { validNewPjName(modalPJChecker, oFilesInfos, onSuccessFct, onValidNewNameFct); }, "button-green");

}

function validNewPjName(modChgPjName, oJson, onSuccessFct, onValidNewNameFct) {

    if (modChgPjName) {
        srcDocument = modChgPjName.getIframe().document;
    }
    else
        srcDocument = document;

    var filesBlocks = srcDocument.querySelectorAll(".blockFile");
    var val, txtbox, oFile, label;
    var oReturn = [];
    for (var i = 0; i < filesBlocks.length; i++) {
        label = srcDocument.getElementById("labelFn" + i);
        val = srcDocument.querySelector('input[name="rbFile' + i + '"]:checked').value;
        txtbox = srcDocument.getElementById("txtSuggestedFn" + i);
        oFile = findObjectByKey(oJson, "filename", getAttributeValue(txtbox, "data-fn"));
        if (oFile) {
            oFile.action = (val == "rbRenameFile" + i) ? 0 : 1;
            oFile.saveas = txtbox.value;
            oFile.pjid = getAttributeValue(label, "data-pjid");
        }
    }

    if (typeof (onValidNewNameFct) == "function")
        onValidNewNameFct(oJson);
    else {
        var modalPJAdd = oModalPJAdd || eTools.GetModal("modalPJAdd") || eTools.GetModal("oModalPJAdd");
        if (modalPJAdd) {
            AddPj(modalPJAdd.getIframe().document, oJson);
        }

    }


    modChgPjName.hide();
}

function getFilesActionsFromInterface(doc) {
    var filesBlocks = doc.querySelectorAll(".blockFile");
    var val;
    var oReturn = [];
    for (var i = 0; i < filesBlocks.length; i++) {
        val = doc.querySelector('input[name="rbFile' + i + '"]:checked').value;
        oReturn.push({
            action: (val == "rbRenameFile" + i) ? 0 : 1,
            filename: GetText(doc.getElementById("labelFn" + i)),
            saveas: doc.getElementById("txtSuggestedFn" + i).value
        });
    }

    return oReturn;
}

function getFilesActionsFromResult(oRes) {
    var oReturn = [];
    var xmlFiles = oRes.getElementsByTagName("file");
    for (var i = 0; i < xmlFiles.length; i++) {
        var filename = getAttributeValue(xmlFiles[i], "id");
        var sSuggestedName = getXmlTextNode(xmlFiles[i].getElementsByTagName("suggestedname")[0]);
        oReturn.push({
            action: 0,
            filename: filename,
            saveas: sSuggestedName
        })
    }
    return oReturn;
}

function cancelNewPjName(modChgPjName) {
    modChgPjName.hide();
    var _fUpload = srcDocument.getElementById('FileToUpload');
    _fUpload.value = "";
}

// Validation de la fenêtre d'ajout d'annexe
function AddPj(srcDocument, oFileInfo) {
    if (!srcDocument || srcDocument == "undefined" || typeof (srcDocument) == "undefined") {
        if (oModalPJAdd) {
            // Récuperation de l'iframe de la modaldialog
            srcDocument = oModalPJAdd.getIframe().document;
        }
        else
            srcDocument = document;
    }

    //if (!srcDocument.getElementById('radPJFile') && srcDocument.forms && srcDocument.forms.length > 0) {
    //    document.forms[0].submit();
    //    return;
    //}

    // Bouton radio selection de fichier
    var _radfile = srcDocument.getElementById('radPJFile');
    // Bouton radio selection de lien
    var _radlink = srcDocument.getElementById('radPJLink');
    //input de type file 'upload'
    var _fUpload = srcDocument.getElementById('FileToUpload');
    var _inptNewName = srcDocument.getElementById('SaveAs');
    var inptUploadMode = srcDocument.getElementById('hUploadMode');

    if (_radlink && _radlink.checked) {

        launchAddPj(srcDocument);
        return;
    }

    if (!(!_radfile && !_radlink) && !(_radfile && _radfile.checked))
        return;


    if (!oFileInfo) {
        var origFilename;
        var aFilePath = _fUpload.value.split("\\");
        origFilename = aFilePath[aFilePath.length - 1];

        var filename = _inptNewName.value;
        if (filename == "") {
            filename = origFilename;
        }

        var oFileInfo = new Array();
        oFileInfo.push({ filename: origFilename, saveas: filename, action: 0 });

    }

    _inptNewName.value = oFileInfo[0].saveas;
    inptUploadMode.value = oFileInfo[0].action;

    callCheckPjExists(oFileInfo);
}


function launchAddPj(srcDocument) {
    setWait(true);
    if (!srcDocument || srcDocument == "undefined" || typeof (srcDocument) == "undefined") {
        if (oModalPJAdd) {
            // Récuperation de l'iframe de la modaldialog
            srcDocument = oModalPJAdd.getIframe().document;
        }
        else
            srcDocument = document;
    }

    if (!srcDocument.getElementById('radPJFile') && srcDocument.forms && srcDocument.forms.length > 0) {

        document.forms[0].submit();
        return;
    }

    // Récuperation des éléments
    // Bouton radio selection de fichier
    var _radfile = srcDocument.getElementById('radPJFile');
    // Bouton radio selection de lien
    var _radlink = srcDocument.getElementById('radPJLink');
    //input de type file 'upload'
    var _fUpload = srcDocument.getElementById('FileToUpload');
    // Value du lien 
    var _uploadlink = srcDocument.getElementById('uploadvalue');
    //input Infobulle 'txtToolTip'
    var _txtToolTip = srcDocument.getElementById('txtToolTip');
    //input description de l'annexe 'txtDesc'
    var _txtDesc = srcDocument.getElementById('txtDesc');

    // en cas de validation sans selection de fichier ou repertoire
    if ((_radfile.checked) || (_radlink != null && _radlink.checked)) {
        if (trim(_fUpload.value) == '' && trim(_uploadlink.value) == '') {
            eAlert(3, top._res_6316, top._res_589, '', null, null, null);
            _fUpload.focus();
            setWait(false);
            return;
        }
    }
    else if (_radlink != null && !_radlink.checked && !_radlink.checked) {
        eAlert(3, top._res_6316, top._res_589, '', null, null, null);
        setWait(false);
        return;
    }

    srcDocument.getElementById('lstTypeLink').value = _radfile.checked ? '1' : '0';
    if (_uploadlink != null)
        srcDocument.getElementById('UploadLink').value = encode(_uploadlink.value);

    srcDocument.getElementById('txtToolTipLink').value = encode(_txtToolTip.value);
    srcDocument.getElementById('txtDescLink').value = encode(_txtDesc.value);
    srcDocument.getElementById('herrorCallBack').value = (function (modal) { return function () { setWait(false); modal.hide(); } })(oModalPJAdd);

    //setWait(false); launchInContext(oModalPJAdd, oModalPJAdd.hide); 

    // On bascule au manager pour les traitements
    srcDocument.getElementById('action').value = 'AddPJ';
    var _form = srcDocument.getElementById("frmUpload");

    _form.submit();

    // le saitWait(false) est injecté par le fichier .cs

}

//renomme une annexe et modifie les propriétés
function renamePJ(modPjPties, nTab, nFileId, nPJId) {
    var sRenParams = ""

    try {
        sRenParams = modPjPties.getIframe().serializePjPties();
    }
    catch (e) {
        eAlert(0, top._res_72, top._res_6630);
        return;
    }
    var updRenPJ = new eUpdater("mgr/ePjManager.ashx", 0);
    updRenPJ.ErrorCallBack = function () { };
    updRenPJ.addParam("nTab", nTab, "post");
    updRenPJ.addParam("FileId", nFileId, "post");
    updRenPJ.addParam("pjid", nPJId, "post");
    updRenPJ.addParam("action", "rename", "post");


    var aPties = sRenParams.split("$|$");
    for (var i = 0; i < aPties.length; i++) {
        var aPty = aPties[i].split("$=$");
        updRenPJ.addParam(aPty[0], aPty[1], "post");

    }

    //todo - faire une fonction de retour pour fermer la fenêtre des propriétés en cas de succès
    updRenPJ.send(function (oRes) { onRenamePj(oRes, modPjPties); });


}


function onRenamePj(oRes, modPjPties) {
    if (oRes != null) {

        var strSuccess = getXmlTextNode(oRes.getElementsByTagName("success")[0]);

        if (strSuccess == "0") {

            var errortitle = getXmlTextNode(oRes.getElementsByTagName("error")[0]);
            var errorDetail = getXmlTextNode(oRes.getElementsByTagName("detail")[0]);
            var errorType = getXmlTextNode(oRes.getElementsByTagName("errTyp")[0]);

            //eAlert(criticity, title, message, details, width, height, function () { myFunction(arg1, arg2); });
            eAlert(errorType, errortitle, errortitle, errorDetail);
        }
        else {
            eAlert(4, top._res_6654, top._res_6653, '', null, null, function () { RefreshLstPj(); });
            modPjPties.hide();

        }
    }

}

// Suppression d'une ou plusieurs pj
function DeletePJ(nTab, nFileId, nIdsFordelete, bReturn, nSource) {
    //    alert("fileid : " + nFileId + " pjId : " + nIdsFordelete);
    // Delete pj from table pj et supprimer physiquement le fichier
    var deletePj = new eUpdater("mgr/ePjManager.ashx", 0);
    deletePj.ErrorCallBack = function () { };
    deletePj.addParam("nTab", nTab, "post");
    deletePj.addParam("FileId", nFileId, "post");
    deletePj.addParam("action", "delete", "post");
    deletePj.addParam("IdsForDelete", nIdsFordelete, "post");

    if (bReturn) {
        var nTitre = top._res_6588;
        var nMessage = top._res_6589;

        // En cas de supprrssion tpl mail on transfère à la fonction qui rafraichit la liste
        if (nSource != null) {
            // @deprecated
            if (nSource == "mail")
                // demande de confirmation avant la suppression todo getres
                eConfirm(1, nTitre, nMessage, '', null, null, function () { deletePj.send(function (oRes) { getListOfPj(oRes, nFileId, nGlobalActiveTab, nIdsFordelete); }); }, '');
        }
        else
            eConfirm(1, nTitre, nMessage, '', null, null, function () { deletePj.send(getPjDelReturn); }, '');


    }
    else {
        deletePj.send();
    }


}

// Retour du manager qui est chargé de supprimer une annexe
function getPjDelReturn(oRes) {
    if (oRes != null) {

        var strSuccess = getXmlTextNode(oRes.getElementsByTagName("success")[0]);

        if (strSuccess == "0") {

            var errortitle = getXmlTextNode(oRes.getElementsByTagName("error")[0]);
            var errorDetail = getXmlTextNode(oRes.getElementsByTagName("detail")[0]);
            var errorType = getXmlTextNode(oRes.getElementsByTagName("errTyp")[0]);

            //eAlert(criticity, title, message, details, width, height, function () { myFunction(arg1, arg2); });
            eAlert(errorType, errortitle, errortitle, errorDetail);
        }
        else {
            eAlert(4, top._res_6588, top._res_6590, '', null, null,
                function () {
                    RefreshLstPj();

                    // On rafrichit les champs sur la fiche principale pas en popup
                    if (typeof (fieldRefresh) != 'undefined' && !isPopup()) {
                        fieldRefresh.refreshFld = true
                        LoadChgFld(oRes, true, window);
                    }
                });

        }
    }
}


// Rafraîchit la liste de PJ en mode template suite à la suppression
function RefreshLstPj() {

    var reloadBkmBtn = document.getElementById("refreshirisbkm_" + (nGlobalActiveTab + 91));
    if (typeof (document.getElementById('btnrefreshLst')) != "undefined" && document.getElementById('btnrefreshLst') != null) {
        document.getElementById('btnrefreshLst').click();
    }
    else if (reloadBkmBtn) {
        reloadBkmBtn.click();
    }
    else if (getCurrentView(document) == "FILE_MODIFICATION" && !isPopup() && top) {
        RefreshBkm(nGlobalActiveTab + 91);
    }
    else if (getCurrentView(document) == "LIST" && nGlobalActiveTab == TAB_PJ) {
        loadList();
    }

}

// Récupération de la liste des Pj pour les tpl mail en Ajax
// @deprecated
function getListOfPj(oRes, nFileId, nTab, nIdsDeleted) {
    // On va vérifier si la pj a été supprimée pour mettre à jour les Ids de pj pour afficher une liste correcte pour une nouvelle fiche tpl
    if (oRes != null) {
        var strSuccess = getXmlTextNode(oRes.getElementsByTagName("success")[0]);
        if (strSuccess == "1") {

            // Liste de pjIds pour la liste à rafraichir
            var nPjids;
            // Mise à jour de la liste des Pjs
            if (oModalPJAdd.pjSpan) {
                nPjids = oModalPJAdd.pjSpan.getAttribute('PjIds');
                var tabPjIds = nPjids.split(';');
                nPjids = '';

                for (var i = 0; i < tabPjIds.length; i++) {
                    if (tabPjIds[i] != nIdsDeleted)
                        nPjids = nPjids + ';' + tabPjIds[i];

                }

                if (nPjids != "")
                    nPjids = nPjids.substring(1);
                //    oModalPJAdd.pjSpan.setAttribute('PjIds', nPjids);
                //   newPjIds = oModalPJAdd.pjSpan.getAttribute('PjIds')
            }

            // Si nPjids est vide on ne lance pas le manager et on close la div contenant la liste
            // On va cacher la div si aucune pj n'est visible
            if (nPjids == "") {
                if (document.getElementById("spanlstPj").innerHTML) {
                    if (typeof (document.getElementById('divlstPJMail')) != "undefined")
                        document.getElementById("divlstPJMail").style.display = "none";
                }
                //Mise à jour du nombre de pj
                if (oModalPJAdd.pjSpan) {
                    oModalPJAdd.pjSpan.innerHTML = "(0)";
                }
            }
            else {
                var getList = new eUpdater("mgr/ePjManager.ashx", 0);
                getList.ErrorCallBack = function () { };
                getList.addParam("FileId", nFileId, "post");
                getList.addParam("nTab", nTab, "post");
                getList.addParam("action", "list", "post");
                getList.addParam("pjIds", nPjids, "post");
                getList.send(writePjLst);
            }

        }
        else {
            //todo getres
            eAlert(4, top._res_8655, top._res_8654, top._res_8653, null, null, function () { RefreshLstPj(); }); //TODORES
        }
    }


}

// Exploitataion de la liste de PJ pour mise à jour suite à une suppression
// @deprecated
function writePjLst(oRes) {
    if (oRes != null) {

        var strSuccess = getXmlTextNode(oRes.getElementsByTagName("success")[0]);
        var listePJ = oRes.getElementsByTagName("attachment");

        if (strSuccess == "1") {
            var pjSpanLst;
            var nLink;
            var nodePj;
            var nKey;
            var nValue;
            if (typeof (document.getElementById("spanlstPj")) != "undefined") {
                // On va vider la liste des PJ
                document.getElementById("spanlstPj").innerHTML = "";
                // On boucle sur la liste des Pj renvoyées par le manager
                for (var j = 0; j < listePJ.length; j++) {

                    nodePj = listePJ[j];
                    var key = getXmlTextNode(nodePj.getElementsByTagName("pjid")[0]);
                    var nValue = getXmlTextNode(nodePj.getElementsByTagName("name")[0]);
                    nLink = "<a class=\"icon-delete\" onclick=\"DeletePJ('" + nGlobalActiveTab + "', '" + oModalPJAdd.fileId + "', '" + key + "', true," + "'mail');\">&nbsp;<img src=\"ghost.gif\"></a> ";
                    document.getElementById("spanlstPj").innerHTML = document.getElementById("spanlstPj").innerHTML + nValue + nLink;

                }
                // On va cacher la div si aucune pj n'est visible
                if (document.getElementById("spanlstPj").innerHTML == '') {
                    if (typeof (document.getElementById('divlstPJMail')) != "undefined")
                        document.getElementById("divlstPJMail").style.display = "none";
                }

                // Mise à jour du nombre de PJ, selon si on est sur une fenêtre de type E-mail ou non
                if (oModalPJAdd.pjSpan) {
                    if (document.getElementById('divlstPJMail')) {
                        oModalPJAdd.pjSpan.innerHTML = top._res_6349.replace('<NUMBER>', listePJ.length);
                    }
                    else {
                        oModalPJAdd.pjSpan.innerHTML = "(" + listePJ.length + ")";
                    }
                }
            }
        }
        else {
            //todo getres
            eAlert(4, top._res_8655, top._res_8654, top._res_8656, null, null, null);

        }
    }
}

// --------------Fin gestion Annexes ------------------


//Les quatres var en dessous sont pour le copier coller dans le planning
var conflictItem = null;
var conflictNewDateBegin = null;
var conflictNewDateEnd = null;
var typCopy = null;


/// <summary>
/// Vérifie si le calendrier est en conflit avec d'autre
/// Valide si pas de conflit, propose annulation sinon
/// </summary>
/// <param name="itm">Objet js calendrier (avec les date d'origine)</param>
/// <param name="newDateBegin">Nouvelle date de début</param>
/// <param name="newDateEnd">Nouvelle date de fin</param>
/// <param name="Copy">Vient d'un copier/coller</param>
/// <param name="bForceRefresh">Indique s'il faut forcer le rafraîchissement complet du planning (cas complexes)</param>
/// <param name="bFromResize">Vient d'un redimensionnement</param>
function checkConflictGraph(itm, newDateBegin, newDateEnd, Copy, bForceRefresh, bFromResize) {



    setWait(true);

    if (bForceRefresh == undefined || bForceRefresh == null)
        bForceRefresh = false;

    var db = new Date(newDateBegin);
    var de = new Date(newDateEnd);

    var upd = new eUpdater("mgr/ePlanningManager.ashx", 0);
    upd.addParam("tab", nGlobalActiveTab, "post");
    upd.addParam("fileid", Copy ? 0 : itm.FileId, "post");
    upd.addParam("scheduleid", 0, "post");
    //GCH - #36012 - Internationnalisation - Planning
    upd.addParam("begindate", eDate.Tools.GetStringFromDate(db), "post");
    upd.addParam("enddate", eDate.Tools.GetStringFromDate(de), "post");
    upd.addParam("owner", itm.Owner, "post");
    upd.addParam("multiowner", itm.MultiOwner, "post");
    upd.addParam("action", "conflict", "post");
    upd.addParam("begindatedescid", document.getElementById('CalDivMain').getAttribute('datedescid'), "post");
    upd.addParam("typeplanning", 1, "post"); //type rdv à priori cette fonction n'est lancée que depuis le planning graphique

    /* Gestion des cas de modification complexes (via les grip de resize, ou rendez-vous à cheval sur plusieurs jours) */
    /* Sauvegarde dans l'updater des paramètres nécessaire dans la suite du traitement */
    upd.myItem = itm;
    upd.ForceRefresh = bForceRefresh;
    upd.DB = newDateBegin;
    upd.DE = newDateEnd;
    upd.FromResize = bFromResize;

    /* ?? */
    conflictItem = itm;
    conflictNewDateBegin = newDateBegin;
    conflictNewDateEnd = newDateEnd;
    typCopy = Copy;

    /* envoie la demande */
    upd.ErrorCallBack = function () { setWait(false); };

    upd.send(updateConflictGraph, upd);
}




/// <summary>
/// Callback de la fonction de vérification de conflit
/// Sauvegarde le calendrier si pas de conflit, affiche une alerte sinon
/// </summary>
/// <param name="oRes">Flux de retour de la vérification de conflit</param>
/// <param name="updat">Objet JS updater ayant fait la demande initiale</param>
function updateConflictGraph(oRes, updat) {

    setWait(false);

    if (oRes.getElementsByTagName("record") == null)
        return;

    var bForceRefresh = (updat && updat.ForceRefresh);
    var bFromResize = (updat && updat.FromResize);

    var users = getXmlTextNode(oRes.getElementsByTagName("record")[0]);
    var aUsers = users.split(';');
    var isConflicted = false;
    for (var j = 0; j < aUsers.length; j++) {
        if (aUsers[j].toString() != "") {
            isConflicted = true;
            break;
        }
    }

    //criticity, title, message, details, width, height, okFct, cancelFct
    if (isConflicted == true) {
        eConfirm(1, '', top._res_6305, '', null, null,

            function () { validCalendarItemPos(conflictItem, conflictNewDateBegin, conflictNewDateEnd, null, bForceRefresh, bFromResize); },

            function () {

                //annulle le déplacement/coller du calendrier
                var oeParam = getParamWindow();

                if (typCopy == null || typCopy == 'undefined' || oeParam.GetParam("CuttedItem") != "") {
                    if (document.getElementById(conflictItem.Div.getAttribute("pint")) != null)
                        document.getElementById(conflictItem.Div.getAttribute("pint")).appendChild(conflictItem.Div);
                    else {
                        itm.Div.style.display = "none";
                    }
                }

                // Cas complexes nécessitant un rafraîchissement complet du planning (ex : déplacement via le grip haut/bas d'un RDV, RDV
                // à cheval sur plusieurs jours...)
                if (bForceRefresh && updat.myItem != null) {
                    // SPH : implémentation initiale
                    // #36 302 - MAB : application pour d'autres cas complexes (RDV à cheval sur plusieurs jours)
                    // il est parfois compliqué de replacer un planning à sa place initiale, les différents éléments et
                    // routines de placement du planning étant gérés séparément (grip haut, grip bas, div du planning, infos système liées).
                    // Dans ces cas, on recharge entièrement le planning plutôt que d'effectuer des déplacements hasardeux via JS
                    top.loadList(1, true);
                }
            });
    }
    else
        validCalendarItemPos(conflictItem, conflictNewDateBegin, conflictNewDateEnd, typCopy, bForceRefresh, bFromResize);
}

//Désactive l'option sur firefox - Sur firefox il est impossible de retrouver le chemin complet du fichier
function disableChkForFF() {
    document.getElementById("chkUpload").checked = true;
    document.getElementById("chkUpload").disabled = true;
}



function isHour(h) {

    var e = new RegExp("^([01]?[0-9]|2[0-3]):[0-5][0-9]$");

    return e.test(h);
}


function isDate(d) {


    if (d == "") // si la variable est vide on retourne faux
        return false;

    e = new RegExp("^[0-9]{1,2}\/[0-9]{1,2}\/([0-9]{2}|[0-9]{4})$");

    // On teste l'expression régulière pour valider la forme de la date
    if (!e.test(d))
        return false;

    // On sépare la date en 3 variables pour vérification, parseInt() converti du texte en entier
    j = parseInt(d.split("/")[0], 10); // jour
    m = parseInt(d.split("/")[1], 10); // mois
    a = parseInt(d.split("/")[2], 10); // année

    // Si l'année n'est composée que de 2 chiffres on complète automatiquement
    if (a < 1000) {
        if (a < 89) a += 2000; // Si a < 89 alors on ajoute 2000 sinon on ajoute 1900
        else a += 1900;
    }

    // Définition du dernier jour de février
    // Année bissextile si annnée divisible par 4 et que ce n'est pas un siècle, ou bien si divisible par 400
    if (a % 4 == 0 && a % 100 != 0 || a % 400 == 0)
        fev = 29;
    else
        fev = 28;

    // Nombre de jours pour chaque mois
    nbJours = new Array(31, fev, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31);

    // Enfin, retourne vrai si le jour est bien entre 1 et le bon nombre de jours, idem pour les mois, sinon retourn faux
    return (m >= 1 && m <= 12 && j >= 1 && j <= nbJours[m - 1]);
}
//--> 


// Compare deux dates et retourne le nombre de jours
function dateDiff(strInterval, dDateBegin, dDateEnd) {
    var d = (dDateEnd.getTime() - dDateBegin.getTime()) / 1000
    switch (strInterval.toLowerCase()) {
        case "yyyy": d /= 12; break;
        case "m": d *= 12 * 7 / 365.25; break;
        case "ww": d /= 7; break;
        case "d": d /= 24; break;
        case "h": d /= 60; break;
        case "n": d /= 60; break;
        case "s": d; break;
    }
    //return Math.round( d );
    return parseInt(d, 10);
}

/* Ajoute un interval a une date et retourne la date
d Day 
h Hour 
n Minute 
s Second 
*/
function dateAdd(strInterval, lNumber, dDate) {


    var dReturnDate = new Date(dDate);



    switch (strInterval.toLowerCase()) {
        case "h":
            dReturnDate.setHours(dDate.getHours() + lNumber);
            break;
        case "n":
            dReturnDate.setMinutes(dDate.getMinutes() + lNumber);
            break;
        case "s":
            dReturnDate.setSeconds(dDate.getSeconds() + lNumber);
            break;
        default:
            dReturnDate.setDate(dDate.getDate() + lNumber);
    }

    return dReturnDate;
}

// SPH : TODO - document.onkeydown est aussi mappé sur ePlanning.js - elles sont donc écrasée

document.onkeydown = OnDocKeyDown;
document.onkeyup = OnDocKeyUp;
document.onkeypressed = OnDocKeyPress;


function OnDocKeyPress(e) {

    var key;
    if (window.event) {
        //window.event.returnValue = false;
        key = window.event.keyCode
    } else {
        //e.preventDefault();
        if (e.which == 0 && e.keyCode != 0) {
            key = e.keyCode;
        } else {
            key = e.which;
        }
    }


    var ie = window.event;
    var firedField = (!ie) ? e.target : event.srcElement



}


function OnDocKeyUp(e) {
    var key;
    if (window.event) {
        //window.event.returnValue = false;
        key = window.event.keyCode
    } else {
        //e.preventDefault();
        if (e.which == 0 && e.keyCode != 0) {
            key = e.keyCode;
        } else {
            key = e.which;
        }
    }

    var ie = window.event;
    var firedField = (!ie) ? e.target : event.srcElement
}

function OnDocKeyDown(e) {
    var key;
    if (window.event) {
        //window.event.returnValue = false;
        key = window.event.keyCode
    } else {
        //e.preventDefault();
        if (e.which == 0 && e.keyCode != 0) {
            key = e.keyCode;
        } else {
            key = e.which;
        }
    }

    var ie = window.event;
    var firedField = (!ie) ? e.target : event.srcElement
}



//Catalogues
var catDlg;
function showCatDlg(oSource, oTarget) {
    if (!oTarget)
        return;

    // Propriétés du champ à afficher
    var bMulti = oSource.getAttribute("mult");
    var catDescId = oSource.getAttribute("did");
    var catPopupType = oSource.getAttribute('pop');
    var catBoundDescId = oSource.getAttribute('bndid');
    var catBoundPopup = oSource.getAttribute('popid');
    var catSpec = oSource.getAttribute("special");

    if (catSpec == "1") {
        // TODO: appel à showSpecialCat
        return;
    }

    var bAdvanced = oSource.getAttribute("advanced");
    var defValue = oSource.getAttribute("ednvalue");
    var treeView = oSource.getAttribute("tree");
    var bMailTemplate = oSource.getAttribute("mailtemplate");
    /*    p_bMulti, p_btreeView, p_defValue, p_sourceFldId
    , p_targetFldId, p_catDescId, p_catPopupType, p_catBoundDescId
    , p_catBoundPopup, p_catParentValue, p_CatTitle, p_bMailTemplate
    , p_partOfAfterValidate
    */
    showCatGeneric((bMulti == "1"), (treeView == "1"), defValue, oSource.id
        , oTarget.id, catDescId, catPopupType, catBoundDescId
        , catBoundPopup, null, "catalogue", "advancedDialog", (bMailTemplate == "1")
        , partOfValidateCatDlg
    );  //TODORES
}
//Catalogues : Partie de code éxécuté au clique sur valider juste avant la fermeture du catalogue avancé
function partOfValidateCatDlg(catalogDialog, srcId, trgId, tabSelectedLabels, tabSelectedValues, selectedIDs) {
    // Mise à jour de l'élément cible (concerné par l'ouverture de la fenêtre de catalogue) avec les valeurs sélectionnées
    // Ne se fait que si au moins un élément a été sélectionné
    if (selectedIDs != "") {
        document.getElementById(trgId).value = joinString(";", tabSelectedLabels);
        document.getElementById(trgId).setAttribute("ednvalue", joinString(";", tabSelectedValues));

        // Si la fenêtre de catalogue a servi à sélectionner un modèle de mail, on envoie le contenu du modèle sélectionné à la fenêtre parente
        if (getAttributeValue(document.getElementById(srcId), "mailTemplate") == "1") {
            var tmpTab = srcId.split("_");
            var sMemoId = "edtCOL_" + tmpTab[1] + "_" + (parseInt(tmpTab[1]) + 8) + "_0_0_0";
            getMemo("MAIL_TEMPLATE", updateMailWithTemplate, document.getElementById(srcId), null, null, null, null, selectedIDs, sMemoId);
        }
    }
}
// Effectue un appel AJAX pour récupérer la valeur d'un champ mémo et effectuer une action ensuite (en appelant la fonction passée en callback)
function getMemo(action, callback, srcElt, trgElt, tabId, descId, fileId, catId, memoId) {
    setWait(true);

    if (!descId)
        descId = getAttributeValue(srcElt, 'did');
    if (!tabId)
        tabId = getTabDescid(descId);
    if (!fileId && trgElt)
        fileId = GetFieldFileId(trgElt.id);

    var url = "mgr/eGetFieldManager.ashx";
    var memoValueGetter = new eUpdater(url, null);
    memoValueGetter.ErrorCallBack = getMemoError;

    memoValueGetter.addParam("action", action, "post");

    if (action == "MAIL_TEMPLATE") {
        memoValueGetter.addParam("catId", catId, "post");
    }
    else if (action == "FIELD_VALUE") {
        memoValueGetter.addParam("tabDescId", tabId, "post");
        memoValueGetter.addParam("fileId", fileId, "post");
    }
    memoValueGetter.addParam("fieldDescId", descId, "post");
    // On passe l'id du memoEditors pour le récupérer enseuite dans la methode "getMemoReturn"
    memoValueGetter.addParam("memoId", memoId, "post");

    memoValueGetter.send(getMemoReturn, callback);
}

function getMemoError(oError) {
    setWait(false);

}

function getMemoReturn(oRes, oCallback) {

    var strSuccess = getXmlTextNode(oRes.getElementsByTagName("success")[0]);

    if (strSuccess == "1") {
        var strAction = getXmlTextNode(oRes.getElementsByTagName("action")[0]);
        var strDescId = getXmlTextNode(oRes.getElementsByTagName("descId")[0]);
        var strMailTemplateId = getXmlTextNode(oRes.getElementsByTagName("catId")[0]);
        var strTargetId = getXmlTextNode(oRes.getElementsByTagName("targetId")[0]);
        var strMailTemplateName = getXmlTextNode(oRes.getElementsByTagName("name")[0]);
        var strMemoBody = getXmlTextNode(oRes.getElementsByTagName("body")[0]);
        var strMemoBodyIsHTML = getXmlTextNode(oRes.getElementsByTagName("bodyIsHTML")[0]);
        var strMemoBodyCSS = getXmlTextNode(oRes.getElementsByTagName("bodyCSS")[0]);
        // MemoId 
        var strMemoId = getXmlTextNode(oRes.getElementsByTagName("memoId")[0]);

        if (typeof (oCallback) == "function") {
            if (strAction == 'MAIL_TEMPLATE')
                oCallback(strDescId, strMailTemplateId, strTargetId, strMailTemplateName, strMemoBody, strMemoBodyIsHTML, strMemoBodyCSS, strMemoId);
            else
                oCallback(oRes);
        }

        setWait(false);
    }
    else {
        setWait(false);
        var sErrorMsg = getXmlTextNode(oRes.getElementsByTagName("message")[0]);
        eAlert(0, top._res_225, top._res_6235, sErrorMsg + '<br>' + top._res_6236);
    }

}

// Fonction spécifique à l'édition de modèles de mail
// Insère le contenu du modèle de mail récupéré via getMemo() dans la fenêtre d'édition de mail (champ Corps)
function updateMailWithTemplate(strDescId, strMailTemplateId, strTargetId, strMailTemplateName, strMemoBody, strMemoBodyIsHTML, strMemoBodyCSS, strMemoId) {
    var memoEditorObject = null;
    // les champs memo sont gérés avec des Ids (ex : edtCOL_1500_1508_0_0_0)
    if (nsMain.getMemoEditor(strMemoId))
        memoEditorObject = nsMain.getMemoEditor(strMemoId);

    if (memoEditorObject) {
        memoEditorObject.setData(strMemoBody, null, true); // Affectation de la valeur dans le contrôle Mémo - Backlog #619 - true en dernier paramètre pour RAZ la couleur de fond

        // TODO: injection de la CSS du modèle dans CKEditor
        if (strMemoBodyCSS.length > 0)
            memoEditorObject.injectCSS(strMemoBodyCSS);

        // Backlog #652 : injecter la couleur de fond sélectionnée dans grapesjs lors de l'édition du modèle
        memoEditorObject.setColor(memoEditorObject.getColorFromCSS(strMemoBodyCSS));
    }
}

//Met a jour le champ memo
function updateMemoData(strMemoId, data, css) {
    updateMailWithTemplate(null, null, null, null, data, null, css, strMemoId);
}

/*
Méthode permettant l'ouverture de la modal de recherche avancée
*/
function openLnkFileDialog(nSearchType, targetTab, bBkm, nCallFrom, bNoLoadFile, onCustomOk) {


    if (!eLinkCatFileEditorObject)  //N'est pas initialisé si l'on vient de la HomePage
        eLinkCatFileEditorObject = new eFieldEditor('linkCatFileEditor', ePopupObject, 'eLinkCatFileEditorObject', 'eLinkCatFileEditor');
    var SEPARATOR_LVL1 = "#|#";
    var SEPARATOR_LVL2 = "#$#";
    var paramSup = "AllRecord" + SEPARATOR_LVL2 + "1" + SEPARATOR_LVL1 + "EnabledSearchAll" + SEPARATOR_LVL2 + "0";
    var sSearch = "";
    var oMru = document.getElementById("mru_search_" + targetTab);
    if (oMru && oMru.value)
        sSearch = oMru.value;

    if (typeof onCustomOk === "undefined")
        onCustomOk = null;

    return eLinkCatFileEditorObject.openLnkFileDialog(nSearchType, targetTab, bBkm, onCustomOk, paramSup, sSearch, nCallFrom, bNoLoadFile);
};



/*********************************************************/
/** SCRIPTS ADDITIONNELS POUR L'AFFICHAGE SUR TABLETTES **/
/*********************************************************/
// Rafraîchit l'affichage lors d'une rotation d'écran (ex : sur tablette ou smartphone)
// Elle est déclarée en-dehors d'un contexte tablettes (isTablet()) pour qu'il soit possible d'appeler cette fonction sans forcément être sur tablettes, si besoin
function refreshAfterOrientation(bSkipOrientationCheck) {

    var bCanRefresh = bSkipOrientationCheck || ((window.orientation != null && typeof (window.orientation) != "undefined") && (window.orientation == 0 || window.orientation == 90 || window.orientation == -90 || window.orientation == 180));

    if (!bCanRefresh)
        return;

    // US #1330 - Tâches #2748, #2750 - On reparamètre les variables utilisées par eParamIFrame.eParamOnLoad() pour recharger le TabID, FileID, type d'affichage actuels après rafraîchissement total ci-dessous
    // Depuis le correctif de l'US #1330, ces variables reprennent le contexte courant (nGlobalActiveTab, getCurrentView()...) UNIQUEMENT si elles sont undefined, et non plus systématiquement comme avant (bug).
    // Donc, appeler la fonction setParamIFrameReloadContext() ci-dessous sans paramètres va les remettre à undefined, ce qui forcera le code ci-dessous à les reparamétrer avec le contexte qui lui convient
    top.nsMain.setParamIFrameReloadContext();

    // Rafraîchissement de la barre d'onglets
    loadNavBar();
    // TODO - NE PAS RECHARGER LA FRAME PARAM
    // Si FraParam est reload, il faut mettre bIsParamLoaded & bIsIFrameLoaded à 0 pour lancer le LoadParam
    top.bIsParamLoaded = 0;
    top.bIsIFrameLoaded = 0;

    // #23 614, #39 338, #59 724 - Mémorisation de l'onglet, du fichier et du mode d'affichage utilisés avant rechargement d'eParamIFrame, qui provoque un rechargement de la page
    // Code corrigé et repassé en revue pour la US #1330 et ses tâches liées (#2748, #2750)
    // Ces variables peuvent être mises à jour via un appel à nsMain.setParamIFrameReloadContext(), soit sans paramètres (auquel cas tout est mis à undefined, et donc réajusté ci-dessous), soit avec paramètres
    var currentView = getCurrentView(document);
    if (typeof (top.tabToLoadAfterParamIFrame) == "undefined" && typeof (top.nGlobalActiveTab) != "undefined")
        top.tabToLoadAfterParamIFrame = top.nGlobalActiveTab;
    if (typeof (top.viewToLoadAfterParamIFrame) == "undefined" && typeof (currentView) != "undefined")
        top.viewToLoadAfterParamIFrame = currentView;
    if (typeof (top.fileToLoadAfterParamIFrame) == "undefined" && document.getElementById("fileDiv_" + top.nGlobalActiveTab))
        top.fileToLoadAfterParamIFrame = getAttributeValue(document.getElementById("fileDiv_" + top.nGlobalActiveTab), "fid");
    // Ces variables sont mises à zéro dans tous les cas, car on ne recharge pas le contexte éventuellement utilisé à l'ouverture de session si on est venu d'eGotoFile.aspx
    //if (typeof (top.isTplMailToLoadAfterParamIFrame) == "undefined")
    top.isTplMailToLoadAfterParamIFrame = "0";
    //if (typeof (top.loadFileInPopupAfterParamIFrame) == "undefined")
    top.loadFileInPopupAfterParamIFrame = "0";
    top.document.getElementById('eParam').contentWindow.location.reload(true);

    // Rafraîchissement du contenu principal de la page
    switch (currentView) {
        case "LIST":

        case "CALENDAR_LIST":

            var oeParam = getParamWindow();
            if (oeParam) {
                var rows = oeParam.GetNumRows();
                if (oeParam.GetParam('Rows') != '')
                    oeParam.SetParam("Rows", rows);
            }
            ReloadList();

            break;
        case "CALENDAR":

            if (!globalModalFile == true)
                ReloadCalendar();

            break;
        case "FILE_CREATION":

        case "FILE_CONSULTATION":

        case "FILE_MODIFICATION":
            if (!globalModalFile == true) {
                RefreshFile(); break;
            }

    }
}

// Gestionnaire déclenchant des fonctions spécifiques lorsqu'on incline la tablette dans l'une des 4 directions
function tabletTiltListener(eventData) {

    if (window.DeviceOrientationEvent) {
        // Initialisation
        var movementDetectionThresholdLR = 10;
        var movementDetectionThresholdFB = 50;
        var minimumTimeBetweenMoves = 0.5; // temps minimum d'attente (en secondes) entre le déclenchement de deux mouvements

        // Récupération des valeurs d'inclinaison actuelles
        var leftToRightAngle = Math.round(eventData.beta);
        var frontToBackAngle = Math.round(eventData.gamma);
        var direction = Math.round(eventData.alpha);

        // Mémorisation de l'orientation actuelle de la tablette au chargement de la page, ou après réinitialisation des variables
        // Ces valeurs d'inclinaison deviennent le point de référence (la position initiale avec laquelle l'utilisateur tient la tablette au chargement de la page)
        // ce qui permet de détecter les mouvements à partir du moment où l'inclinaison change de manière significative
        if (!window.initialTabletOrientationLR) {
            window.initialTabletOrientationLR = leftToRightAngle;
        }
        if (!window.initialTabletOrientationFB) {
            window.initialTabletOrientationFB = frontToBackAngle;
        }
        if (!window.initialTabletOrientationDir) {
            window.initialTabletOrientationDir = direction;
        }

        // Calcul du temps écoulé entre deux déclenchements de fonctions
        var currentDate = new Date();
        var lastLeftTriggerDelta = 0;
        var lastRightTriggerDelta = 0;
        var lastUpTriggerDelta = 0;
        var lastDownTriggerDelta = 0;

        if (!window.lastTabletMoveLeftTriggerDate) { window.lastTabletMoveLeftTriggerDate = currentDate; }
        if (!window.lastTabletMoveRightTriggerDate) { window.lastTabletMoveRightTriggerDate = currentDate; }
        if (!window.lastTabletMoveUpTriggerDate) { window.lastTabletMoveUpTriggerDate = currentDate; }
        if (!window.lastTabletMoveDownTriggerDate) { window.lastTabletMoveDownTriggerDate = currentDate; }
        lastLeftTriggerDelta = dateDiff("s", window.lastTabletMoveLeftTriggerDate, currentDate);
        lastRightTriggerDelta = dateDiff("s", window.lastTabletMoveRightTriggerDate, currentDate);
        lastUpTriggerDelta = dateDiff("s", window.lastTabletMoveUpTriggerDate, currentDate);
        lastDownTriggerDelta = dateDiff("s", window.lastTabletMoveDownTriggerDate, currentDate);

        // On vérifie si le temps écoulé entre 2 déclenchements est suffisant, et si on empêche pas le déclenchement global (ex : lors d'un setWait())
        var canTriggerLeftFunction = !window.preventTabletMoveFunctions && lastLeftTriggerDelta > minimumTimeBetweenMoves;
        var canTriggerRightFunction = !window.preventTabletMoveFunctions && lastRightTriggerDelta > minimumTimeBetweenMoves;
        var canTriggerUpFunction = !window.preventTabletMoveFunctions && lastUpTriggerDelta > minimumTimeBetweenMoves;
        var canTriggerDownFunction = !window.preventTabletMoveFunctions && lastDownTriggerDelta > minimumTimeBetweenMoves;

        // On n'autorise pas plusieurs mouvements de suite si l'intervalle de temps entre chaque n'est pas suffisant
        var preventAnyTabletMove = (!canTriggerLeftFunction || !canTriggerRightFunction || !canTriggerUpFunction || !canTriggerDownFunction);
        if (preventAnyTabletMove) {
            window.initialTabletOrientationLR = null;
            window.initialTabletOrientationFB = null;
            window.lastTriggeredMovement = 'none';
            return;
        }

        // Détection de la rotation actuelle de l'écran
        var tabletOrientation = 'portrait';
        var tabletOrientationSide = 'normal';
        switch (window.orientation) {
            case 0:
                tabletOrientation = 'portrait';
                tabletOrientationSide = 'normal';
                break;
            case -90:
                tabletOrientation = 'landscape';
                tabletOrientationSide = 'reverse';
                break;
            case 90:
                tabletOrientation = 'landscape';
                tabletOrientationSide = 'normal';
                break;
            case 180:
                tabletOrientation = 'portrait';
                tabletOrientationSide = 'reverse';
                break;
        }

        // Détection des mouvements - initialisation
        var detectedLRMovement = 'none';
        var detectedFBMovement = 'none';
        var detectedMovement = 'none';
        var acceptedMovement = 'none';
        var leftToRightDelta = 0;
        var frontToBackDelta = 0;
        var delta = 0;

        // Détection des mouvements gauche/droite en fonction de l'amplitude de l'inclinaison actuelle par rapport à l'inclinaison initiale de la tablette
        if (window.initialTabletOrientationLR == leftToRightAngle) {
            leftToRightDelta = 0;
            detectedLRMovement = 'none';
        }
        else if (window.initialTabletOrientationLR > leftToRightAngle) {
            leftToRightDelta = window.initialTabletOrientationLR - leftToRightAngle;
            detectedLRMovement = (tabletOrientationSide == 'normal' ? 'left' : 'right');
        }
        else {
            leftToRightDelta = leftToRightAngle - window.initialTabletOrientationLR;
            detectedLRMovement = (tabletOrientationSide == 'normal' ? 'right' : 'left');
        }

        // Détection des mouvements avant/arrière en fonction de l'amplitude de l'inclinaison actuelle par rapport à l'inclinaison initiale de la tablette
        if (window.initialTabletOrientationFB == frontToBackAngle) {
            frontToBackDelta = 0;
            detectedFBMovement = 'none';
        }
        else if (window.initialTabletOrientationFB > frontToBackAngle) {
            frontToBackDelta = window.initialTabletOrientationFB - frontToBackAngle;
            detectedFBMovement = (tabletOrientationSide == 'normal' ? 'down' : 'up');
        }
        else {
            frontToBackDelta = frontToBackAngle - window.initialTabletOrientationFB;
            detectedFBMovement = (tabletOrientationSide == 'normal' ? 'up' : 'down');
        }

        // Détermination du mouvement réellement effectué en fonction de l'inclinaison de la tablette et de son orientation
        switch (tabletOrientation) {
            case 'portrait':
                if (detectedLRMovement == 'left') {
                    detectedMovement = 'up';
                    if (leftToRightDelta > movementDetectionThresholdLR)
                        acceptedMovement = detectedMovement;
                }
                if (detectedLRMovement == 'right') {
                    detectedMovement = 'down';
                    if (leftToRightDelta > movementDetectionThresholdLR)
                        acceptedMovement = detectedMovement;
                }
                if (detectedFBMovement == 'down') {
                    detectedMovement = 'left';
                    if (frontToBackDelta > movementDetectionThresholdFB) {
                        if (frontToBackDelta > leftToRightDelta)
                            acceptedMovement = detectedMovement;
                    }
                }
                if (detectedFBMovement == 'up') {
                    detectedMovement = 'right';
                    if (frontToBackDelta > movementDetectionThresholdFB) {
                        if (frontToBackDelta > leftToRightDelta)
                            acceptedMovement = detectedMovement;
                    }
                }
                break;
            case 'landscape':
                if (detectedLRMovement == 'left') {
                    detectedMovement = 'left';
                    if (leftToRightDelta > movementDetectionThresholdLR)
                        acceptedMovement = detectedMovement;
                }
                if (detectedLRMovement == 'right') {
                    detectedMovement = 'right';
                    if (leftToRightDelta > movementDetectionThresholdLR)
                        acceptedMovement = detectedMovement;
                }
                if (detectedFBMovement == 'down') {
                    detectedMovement = 'down';
                    if (frontToBackDelta > movementDetectionThresholdFB) {
                        if (frontToBackDelta > leftToRightDelta)
                            acceptedMovement = detectedMovement;
                    }
                }
                if (detectedFBMovement == 'up') {
                    detectedMovement = 'up';
                    if (frontToBackDelta > movementDetectionThresholdFB) {
                        if (frontToBackDelta > leftToRightDelta)
                            acceptedMovement = detectedMovement;
                    }
                }
                break;
        }

        // Lorsqu'on réalise deux mouvements opposés (un à gauche, puis à droite par ex.), il faut une notion de "point milieu" pour être sûr que, par exemple,
        // recentrer la tablette après un mouvement vers la gauche (en la réinclinant vers la droite), ne déclenche pas l'action vers la droite.
        // Si le mouvement détecté est à l'opposé de celui qui a été déclenché la dernière fois, il faut donc doubler l'amplitude d'action requise pour
        // que le mouvement soit accepté 
        // ex : tablette actuellement inclinée à -20 (soit à gauche)
        // - pour revenir au milieu : +20 = 0
        // - pour considérer que le mouvement est vers la droite = 0 + 20
        // - soit une amplitude de mouvement requise de 20 + 20 = 40 = threshold * 2
        var oppositeMovement = (
            (detectedMovement == 'left' && window.lastTriggeredMovement == 'right') ||
            (detectedMovement == 'right' && window.lastTriggeredMovement == 'left') ||
            (detectedMovement == 'up' && window.lastTriggeredMovement == 'down') ||
            (detectedMovement == 'down' && window.lastTriggeredMovement == 'up')
        );
        if (oppositeMovement) {
            switch (detectedMovement) {
                case 'left':
                case 'right':
                    leftToRightDelta -= movementDetectionThresholdLR;
                    preventAnyTabletMove = leftToRightDelta <= movementDetectionThresholdLR;
                case 'up':
                case 'down':
                    frontToBackDelta -= movementDetectionThresholdFB;
                    preventAnyTabletMove = frontToBackDelta <= movementDetectionThresholdFB;
            }
        }

        //document.title = leftToRightDelta + '.' + frontToBackDelta + '.' + detectedMovement + '.' + acceptedMovement + '.' + preventAnyTabletMove;

        // En fonction du mouvement détecté, si l'exécution de la fonction associée est autorisée,
        // on lance la fonction associée au mouvement
        if (!preventAnyTabletMove) {
            if (acceptedMovement == 'left') {
                //document.title = 'A gauche !';
                if (canTriggerLeftFunction && typeof (window.onTabletMoveLeft) == 'function') {
                    window.lastTabletMoveLeftTriggerDate = new Date();
                    window.initialTabletOrientationLR = null;
                    window.initialTabletOrientationFB = null;
                    window.lastTriggeredMovement = acceptedMovement;
                    window.onTabletMoveLeft(eventData);
                }
            }
            else if (acceptedMovement == 'right') {
                //document.title = 'A droite !';
                if (canTriggerRightFunction && typeof (window.onTabletMoveRight) == 'function') {
                    window.lastTabletMoveRightTriggerDate = new Date();
                    window.initialTabletOrientationLR = null;
                    window.initialTabletOrientationFB = null;
                    window.lastTriggeredMovement = acceptedMovement;
                    window.onTabletMoveRight(eventData);
                }
            }
            else if (acceptedMovement == 'up') {
                //document.title = 'En haut !';
                if (canTriggerUpFunction && typeof (window.onTabletMoveUp) == 'function') {
                    window.lastTabletMoveUpTriggerDate = new Date();
                    window.initialTabletOrientationLR = null;
                    window.initialTabletOrientationFB = null;
                    window.lastTriggeredMovement = detectedMovement;
                    window.onTabletMoveUp(eventData);
                }
            }
            else if (acceptedMovement == 'down') {
                //document.title = 'En bas !';
                if (canTriggerDownFunction && typeof (window.onTabletMoveDown) == 'function') {
                    window.lastTabletMoveDownTriggerDate = new Date();
                    window.initialTabletOrientationLR = null;
                    window.initialTabletOrientationFB = null;
                    window.lastTriggeredMovement = acceptedMovement;
                    window.onTabletMoveDown(eventData);
                }
            }
            else {
                //document.title = 'Eudonet XRM';
            }
        }
    }
}

// Actions à effectuer lorsque la tablette est inclinée dans une des quatre directions
function onTabletMoveLeft() {
    // Cette action ne doit pas se déclencher si une fenêtre popup est ouverte
    if (modalWindowsOpened())
        return;

    var currentView = top.getCurrentView();
    switch (currentView) {
        case "CALENDAR_LIST":
        case "LIST":
            prevpage(nGlobalActiveTab, (currentView == "CALENDAR_LIST"));
            break;
        case "CALENDAR":
            setPrevCalDate();
            break;
        case "FILE_MODIFICATION":
            BrowseFile(document.getElementById("BrowsingPrevious"));
            break;
    }
}
function onTabletMoveRight() {
    // Cette action ne doit pas se déclencher si une fenêtre popup est ouverte
    if (modalWindowsOpened())
        return;

    var currentView = top.getCurrentView();
    switch (currentView) {
        case "CALENDAR_LIST":
        case "LIST":
            nextpage(nGlobalActiveTab, (currentView == "CALENDAR_LIST"));
            break;
        case "CALENDAR":
            setNextCalDate();
            break;
        case "FILE_MODIFICATION":
            BrowseFile(document.getElementById("BrowsingNext"));
            break;
    }
}
function onTabletMoveUp() {
    // Cette action ne doit pas se déclencher si une fenêtre popup est ouverte
    if (modalWindowsOpened())
        return;

    var currentView = top.getCurrentView();
    switch (currentView) {
        case "CALENDAR_LIST":
        case "LIST":
            firstpage(nGlobalActiveTab, (currentView == "CALENDAR_LIST"));
            break;
        case "CALENDAR":
            setPrevMonthCalDate();
            break;
        case "FILE_MODIFICATION":
            BrowseFile(document.getElementById("BrowsingFirst"));
            break;
    }
}
function onTabletMoveDown() {
    // Cette action ne doit pas se déclencher si une fenêtre popup est ouverte
    if (modalWindowsOpened())
        return;

    var currentView = top.getCurrentView();
    switch (currentView) {
        case "CALENDAR_LIST":
        case "LIST":
            lastpage(nGlobalActiveTab, (currentView == "CALENDAR_LIST"));
            break;
        case "CALENDAR":
            setNextMonthCalDate();
            break;
        case "FILE_MODIFICATION":
            BrowseFile(document.getElementById("BrowsingLast"));
            break;
    }
}

// DECLARATION DES VARIABLES ET DES AUTOMATISMES GERANT LES OPTIMISATIONS TABLETTES

// Ajout des scripts externes sur la page
function setTabletScripts() {
    if (isTablet()) {
        // Ajout des scripts et des CSS pour tablettes
        addCss("mobile/mobile-styles", "MAIN"); // CSS générique
        //if (isTabletMenuEnabled()) { addCss("mobile/eMenu", "MAIN"); }
        if (isTabletNavBarEnabled()) { addCss("mobile/eNavBar", "MAIN"); }
        if (areTabletCustomScrollersEnabled()) { addCss("mobile/eScrollers", "MAIN"); }

        //addScript("mobile/slideinmenu", "MAIN");
        addScript("mobile/iscroll", "MAIN");
        addScript("mobile/eTouchManager", "MAIN");

        // Pointeurs vers les objets scripts pour tablettes
        document.touchManagerGlobalNav = null;
        //document.slideInMenuRight = null;
        document.scrollerMainDiv = null;

        setTabletHandlers();
    }
}

// Déclaration des gestionnaires et scripts pour tablettes
function setTabletHandlers() {
    if (isTablet()) {
        // SCRIPTS A INITIALISATION IMMEDIATE (NE NECESSITENT PAS D'APPEL AUX FONCTIONS LOADTABLET*() DEPUIS LES PAGES APPELANTES)

        // Suppression éventuelle de l'évènement touchMove natif du navigateur ; les gestes au doigt sont gérés par les scrollers personnalisés chargés via loadTabletScrollers()
        loadTabletNativeScrollChecker();

        // Gestionnaire d'orientation
        loadTabletOrientationListener();

        // Gestionnaire surveillant le fait que l'on penche la tablette
        loadTabletTiltListener();

        // SCRIPTS A INITIALISATION DIFFEREE (DECLARATIONS UNIQUEMENT, LES SCRIPTS EN QUESTION DOIVENT ETRE CHARGES VIA LES FONCTIONS LOADTABLET*() CORRESPONDANTES)

        // Gestes sur la barre de navigation (onglets)
        document.setNavBarTouchManager = function () {
            var oGlobalNav = document.getElementById('globalNav');
            var oNav = document.getElementById('nav');
            var oNavPage = document.getElementById("menuNavBar");
            var oNavTabPage = document.getElementById("navigTab");
            var oListFile = oNavPage.children;

            addClass(oNavPage, 'animTransition');

            if (!document.touchManagerGlobalNav && oGlobalNav) {
                document.touchManagerGlobalNav = new eTouchManager('en-tete');
            }

            // En mode tactile, déplacement de l'onglet "Accueil" dans une div séparée pour qu'il reste en position fixe
            if (!document.getElementById('navHome')) {
                // on masque le dernier onglet avant le + en mode tactile, car il est souvent "coupé" à l'affichage
                oNavPage.addEventListener('webkitTransitionEnd', tabletAfterNavBarTransition);

                // Création du bouton "Accueil" fixe
                var oHomeNav = document.createElement('div');
                for (var i = 0; i < oNav.attributes.length; i++)
                    oHomeNav.setAttribute(oNav.attributes[i].name, oNav.attributes[i].value);
                oHomeNav.id = "navHome";
                var oHomeNavPage = document.createElement('ul');
                for (var i = 0; i < oNavPage.attributes.length; i++) {
                    if (oNavPage.attributes[i].name != 'style')
                        oHomeNavPage.setAttribute(oNavPage.attributes[i].name, oNavPage.attributes[i].value);
                }
                oHomeNavPage.id = "menuNavBarHome";
                removeClass(oHomeNavPage, 'animTransition');
                oHomeNavPage.style.borderTop = '0px';
                var oExistingHomeLi = oListFile[0];
                oExistingHomeLi.id = "navOldHomeLi";
                var oNewHomeLi = document.createElement('li');
                // le contenu HTML du bouton "Accueil" existant sera recopié dans le nouveau "Accueil" après la transition,
                // le temps que tous les éléments insérés dynamiquement soient dans le DOM
                for (var i = 0; i < oExistingHomeLi.attributes.length; i++)
                    oNewHomeLi.setAttribute(oExistingHomeLi.attributes[i].name, oExistingHomeLi.attributes[i].value);
                oNewHomeLi.id = "navHomeLi";
                oHomeNav.style.zIndex = oNavPage.style.zIndex + 1;
                oHomeNavPage.appendChild(oNewHomeLi);
                oHomeNav.appendChild(oHomeNavPage);
                oNavPage.style.marginLeft = getNumber(oExistingHomeLi.clientWidth) + 'px';
                // Le bouton "Accueil" existant sera supprimé après la transition ; pour l'instant, on le masque
                oExistingHomeLi.style.display = 'none';
                oGlobalNav.insertBefore(oHomeNav, oNav);
                addClass(oHomeNav, "HomeFixed");

                // Création du bouton "+" fixe
                var oPlusNav = document.createElement('div');
                for (var i = 0; i < oNav.attributes.length; i++)
                    oPlusNav.setAttribute(oNav.attributes[i].name, oNav.attributes[i].value);
                oPlusNav.id = "navPlus";
                var oPlusNavPage = document.createElement('ul');
                for (var i = 0; i < oNavPage.attributes.length; i++) {
                    if (oNavPage.attributes[i].name != 'style')
                        oPlusNavPage.setAttribute(oNavPage.attributes[i].name, oNavPage.attributes[i].value);
                }
                oPlusNavPage.id = "menuNavBarPlus";
                removeClass(oPlusNavPage, 'animTransition');
                oPlusNavPage.style.borderTop = '0px';
                var oExistingPlusLi = oListFile[oListFile.length - 1];
                oExistingPlusLi.id = "navPlusLi";
                var oNewPlusLi = document.createElement('li');
                // le contenu HTML du "+" existant sera recopié dans le nouveau "+" après la transition,
                // le temps que tous les éléments insérés dynamiquement soient dans le DOM
                for (var i = 0; i < oExistingPlusLi.attributes.length; i++)
                    oNewPlusLi.setAttribute(oExistingPlusLi.attributes[i].name, oExistingPlusLi.attributes[i].value);
                oNewPlusLi.id = "navSecPlusLi";
                oPlusNavPage.appendChild(oNewPlusLi);
                oPlusNav.appendChild(oPlusNavPage);
                oGlobalNav.insertBefore(oPlusNav, oNav.nextSibling);
                addClass(oPlusNav, "PlusFixed");
            }

            var nNbPages = Number(oNavPage.getAttribute("nbtab"));
            if (nNbPages > 1) {
                // Le défilement des pages d'onglets sur tablettes se fait en imprimant le mouvement de défilement au-dessus de la navbar (partie blanche avec logo Eudonet et pastilles)
                // Ceci, pour éviter des conflits entre l'eTouchManager (qui capture tous les évènements tactiles) et la gestion native du navigateur sur les menus CSS
                document.touchManagerGlobalNav.onTouchStart = function (e) {
                    if (e && e.target && e.target.className && e.target.className == "header") {
                        var oMenuNavBar = document.getElementById("menuNavBar");
                        removeClass(oMenuNavBar, 'animTransition');
                    }
                }
                document.touchManagerGlobalNav.onTouchMove = function (e, moveX, moveY, strDirection) {
                    if (e && e.target && e.target.className && e.target.className == "header") {
                        var oMenuNavBar = document.getElementById("menuNavBar");
                        var oHomeWidth = getNumber(document.getElementById('navHomeLi').clientWidth);
                        document.touchManagerGlobalNav.previousMarginLeft = getNumber(oMenuNavBar.style.marginLeft);
                        if (isNaN(document.touchManagerGlobalNav.previousMarginLeft))
                            document.touchManagerGlobalNav.previousMarginLeft = oHomeWidth + 'px';

                        var moveOffset = 0;
                        if (strDirection == "left") { moveOffset = -1; }
                        if (strDirection == "right") { moveOffset = 1; }
                        var newPosition = document.touchManagerGlobalNav.previousMarginLeft + moveOffset;
                        var nNbPages = Number(oMenuNavBar.getAttribute("nbtab"));
                        if (newPosition <= oHomeWidth && nActivePageTabs < nNbPages)
                            oMenuNavBar.style.marginLeft = '' + newPosition + 'px';
                    }
                }
                document.touchManagerGlobalNav.onTouchEnd = function (e, moveX, moveY) {
                    if (e && e.target && e.target.className && e.target.className == "header") {
                        var oMenuNavBar = document.getElementById("menuNavBar");
                        addClass(oMenuNavBar, 'animTransition');
                        var nNbPages = Number(oMenuNavBar.getAttribute("nbtab"));
                        if (moveX > 5 && nActivePageTabs > 1)
                            switchActivePageTab(nActivePageTabs - 1);
                        else if (moveX < -5 && nActivePageTabs < nNbPages)
                            switchActivePageTab(nActivePageTabs + 1);
                    }
                }
            }
        };

        // Gestes sur le calendrier
        document.setCalendarTouchManager = function () {
            var oCalDivMain = document.getElementById('CalDivMain');

            //if (!document.touchManagerCalDivMain && oCalDivMain) {
            // Ici, on ne vérifie pas si le touchManager a déjà été instancié : il faut le réinstancier à chaque appel, car le rafraîchissement du calendrier rafraîchit les éléments tels que CalDivMain, sans pour autant détruire les objets JavaScript. Il faut donc recâbler le manager sur l'élément rafraîchi, même si le manager a déjà été précédemment instancié
            if (oCalDivMain) {
                document.touchManagerCalDivMain = new eTouchManager('CalDivMain');
                // On définit le seuil, en pixels, à partir duquel on considère qu'un geste est un mouvement du doigt.
                // Dans le cas de Planning, on considèrera que tout mouvement d'une amplitude inférieure à la hauteur d'une cellule
                // (intervalle) de planning graphique constitue un simple "clic" ("tap") et non un mouvement de "glisser" du doigt.
                // Comme sur PC, où le tracé d'un rendez-vous à la souris ne se fait que si l'amplitude de mouvement est supérieure à celle
                // d'une cellule/d'un intervalle.
                var oIntervals = document.querySelectorAll(".i-D");
                if (oIntervals.length > 0)
                    document.touchManagerCalDivMain.tapMoveThreshold = getNumber(oIntervals[0].style.height);
                document.touchManagerCalDivMain.longTapThreshold = 500; // nombre de millisecondes à partir duquel un appui est considéré comme long
                // Un deuxième seuil personnalisé est ensuite utilisé dans la fonction onLongTap pour effectuer d'autres "sous-actions"
            }
            document.touchManagerCalDivMain.onTap = function (e, tapTime) {
                if (e && e.target) {
                    // 39222
                    // Priorité au clic "normal" de l'élément
                    //if (e.target.click) {
                    //    e.target.click();
                    //}
                    //else {

                    //}

                    // Priorité au clic "custom"
                    try {

                        if (typeof (onCalClick) == "function") {
                            onCalClick(e);
                        }
                        // A défaut, déclenchement du double clic
                        else if (typeof (onCalDblClick) == "function") {
                            onCalDblClick(e, false);
                        }
                    }
                    catch (e) {

                        trace(e);
                    }
                }
            }
            document.touchManagerCalDivMain.onLongTap = function (e, tapTime) {
                var oeParam = getParamWindow();

                // Coller ou Copier en priorité si le presse-papiers est rempli, et si la longueur de tap est "intermédiaire"
                var bTriggeredClick = false;
                if (oeParam.GetParam("CuttedItem") != "" || oeParam.GetParam("CopiedItem") != "") {
                    if (tapTime < 750) {
                        if (e && e.target && typeof (onCalClick) == "function") {
                            bTriggeredClick = true;
                            onCalClick(e);
                        }
                    }
                }
                // Sinon, déclenchement du double-clic
                if (!bTriggeredClick && e && e.target && typeof (onCalDblClick) == "function") {
                    onCalDblClick(e, true);
                }
            }
        };

        // Menu escamotable
        /*
        document.setSlideInMenu = function () {
            //if (!document.slideInMenuRight) {
            document.slideInMenuRight = new slideInMenu('rightMenu', false, "right");
            document.slideInMenuRight.onBeforeOpen = function () {
                var arrowTurn = document.getElementById("arrowTurn");
                arrowTurn.style.webkitTransform = ("rotate(180deg)");
                var tabletHandle = document.getElementById("tabletHandle");
                addClass(tabletHandle, 'tabletHandleOpened');
            }
            document.slideInMenuRight.onAfterOpen = function () {
                var arrowTurn = document.getElementById("arrowTurn");
                arrowTurn.style.webkitTransform = ("rotate(180deg)");
                var tabletHandle = document.getElementById("tabletHandle");
                addClass(tabletHandle, 'tabletHandleOpened');
            }
            document.slideInMenuRight.onAfterClose = function () {
                var arrowTurn = document.getElementById("arrowTurn");
                arrowTurn.style.webkitTransform = ("rotate(0deg)");
                var tabletHandle = document.getElementById("tabletHandle");
                removeClass(tabletHandle, 'tabletHandleOpened');
            }
            document.slideInMenuRight.onBeforeClose = function () {
                var arrowTurn = document.getElementById("arrowTurn");
                arrowTurn.style.webkitTransform = ("rotate(0deg)");
                var tabletHandle = document.getElementById("tabletHandle");
                removeClass(tabletHandle, 'tabletHandleOpened');
            }
            //}
        };
        */

        // Fonctionnalités de défilement personnalisées sur certains éléments de la page (indépendantes du défilement natif du navigateur)
        document.setScrollers = function () {
            if (!document.scrollerMainDiv && document.getElementById('mainDiv') && document.getElementById('mainDiv').children[0])
                document.scrollerMainDiv = new iScroll('mainDiv');
        };
    }
}

// FONCTIONS D'INITIALISATION DES SCRIPTS POUR TABLETTES - CELLES FAISANT APPEL A DES SCRIPTS EXTERNES PEUVENT ETRE RECHARGEES AUTOMATIQUEMENT VIA SETTIMEOUT()

// Menu escamotable
function loadTabletMenu(timeout) {
    /*
    if (!timeout)
        timeout = 1000;

    if (isTablet() && isTabletMenuEnabled()) {
    */
    /* Pour que le menu puisse être affiché correctement sur tablettes :
    - le script slideInMenu doit être chargé
    - le grip de sélection du menu doit être chargé
    - le menu droit doit être chargé
    - le menu droit doit être passé dans les mains de adjustDivContainer()
      (qui ajoute la classe rightMenuWidth et le dimensionne correctement, pour que slideInMenu calcule sa position)
    */
    /*
        if (
            typeof (slideInMenu) != "undefined" &&
            document.getElementById("tabletHandle") &&
            document.getElementById("rightMenu") &&
            document.getElementById("rightMenu").className.indexOf('rightMenuWidth') > -1
        )
            document.setSlideInMenu();
        else {
            window.setTimeout(function () { loadTabletMenu(timeout); }, timeout);
        }
    }
    */
}
// Barre de navigation (onglets) optimisée pour tablettes
function loadTabletNavBar(timeout) {
    if (!timeout)
        timeout = 1000;

    if (isTablet() && isTabletNavBarEnabled()) {
        if (typeof (eTouchManager) != "undefined")
            document.setNavBarTouchManager();
        else
            window.setTimeout(function () { loadTabletNavBar(timeout); }, timeout);
    }
}
// Barre de navigation (onglets) optimisée pour tablettes
function loadTabletCalendar(timeout) {
    if (!timeout)
        timeout = 1000;

    if (isTablet() && isTabletCalendarEnabled()) {
        if (typeof (eTouchManager) != "undefined")
            document.setCalendarTouchManager();
        else
            window.setTimeout(function () { loadTabletCalendar(timeout); }, timeout);
    }
}
// Ajout de "scrollers" personnalisés sur certains éléments
function loadTabletScrollers(timeout) {
    if (!timeout)
        timeout = 1000;

    if (isTablet() && areTabletCustomScrollersEnabled()) {
        if (typeof (iScroll) != "undefined")
            document.setScrollers();
        else
            window.setTimeout(function () { loadTabletScrollers(timeout); }, timeout);
    }
}
// Surveillance d'un changement d'orientation d'écran
function loadTabletOrientationListener() {
    if (isTablet() && isTabletOrientationSupportEnabled()) {
        // Gestionnaire pour rafraîchir la page (liste) après un changement total d'orientation
        window.onorientationchange = refreshAfterOrientation;
    }
}

// Surveillance du gyroscope
function loadTabletTiltListener() {
    if (isTablet() && isTabletTiltSupportEnabled()) {
        // Gestionnaire pour passer à la page précédente/suivante lorsqu'on penche la tablette à gauche ou à droite
        //http://www.html5rocks.com/en/tutorials/device/orientation/
        if (window.DeviceOrientationEvent) {
            window.addEventListener('deviceorientation', tabletTiltListener, false);
        }
    }
}

// Désactivation éventuelle des gestes natifs gérés par le navigateur (ex : zoom à 2 doigts, défilement)
function loadTabletNativeScrollChecker() {
    if (isTablet() && !isTabletNativeScrollEnabled()) {
        document.addEventListener('touchmove', function (e) { e.preventDefault(); }, false);
    }
}

///summary
///Fonction transférant les liste d'id de navigation mode fiche
///dans eParamIframe. Si le conteneur principal n'existe pas (premier chargement)
///On le créé, sinon on ajoute juste son contenu, correspondant à la plage d'id qui viens d'être généré dans eRenderer
///summary
function SetListIdParameter(nTab) {

    var sSelector = '#mt_' + nTab + " tr[eid]";
    var aTrId = top.document.querySelectorAll(sSelector);
    var sListIds = "";

    if (aTrId.length > 0) {



        for (var nIcmptIds = 0; nIcmptIds < aTrId.length; nIcmptIds++) {


            if (aTrId[nIcmptIds].getAttribute("eid").indexOf(nTab) == 0) {

                var aEid = aTrId[nIcmptIds].getAttribute("eid").toString().split("_");
                var nId = aEid[aEid.length - 1];

                if (sListIds.length > 0 && (';' + sListIds + ';').indexOf(';' + nId + ';') != -1)
                    continue;

                if (sListIds.length > 0)
                    sListIds += ";";

                sListIds += nId;
            }
        }
    }

    if (sListIds.length == 0)
        return;


    var eParam = top.document.getElementById('eParam').contentWindow.document;
    var eParamIdContainer = eParam.getElementById("List_" + nTab);



    if (eParamIdContainer != null)
        eParamIdContainer.value = sListIds;


}



/*Fonction qui active ou désactive les boutons de navigation de la fiche*/
function InitNavigateButton() {

    var oFirst = document.getElementById("BrowsingFirst");
    var oPrev = document.getElementById("BrowsingPrevious");
    var oNext = document.getElementById("BrowsingNext");
    var oLast = document.getElementById("BrowsingLast");

    //Frame des param
    var eParam = getParamWindow();

    try {

        //récupération de la fiche en cours
        var currentFile = document.getElementById("fileDiv_" + nGlobalActiveTab);
        if (currentFile && currentFile.tagName && currentFile.tagName.toLowerCase() != "div")
            return;

        if (!currentFile) {
            setAttributeValue(oNext, "eEnabled", 1);
            setAttributeValue(oLast, "eEnabled", 1);
            setAttributeValue(oFirst, "eEnabled", 1);
            setAttributeValue(oPrev, "eEnabled", 1);
            return;
        }

        //DescId & Id de la ficher en cours
        var nFileId = currentFile.getAttribute("fid");
        var nTab = currentFile.getAttribute("did");
        var nPage = eParam.GetParam("Page_" + nTab);
        if (nPage == "")
            nPage = 1;


        var sIdsList = CleanListIds(eParam.GetParam("List_" + nTab));
        if (sIdsList.length > 0) {
            var adList = sIdsList.split(";");
            var nIdxCurrentId = getIdxArray(adList, nFileId);

            //désactive le paging à la 1er page < & <<
            if (nIdxCurrentId == 0 && nPage == 1) {
                //Désactive 1er fiche / dernière fiche

                setAttributeValue(oFirst, "eEnabled", 0);
                setAttributeValue(oPrev, "eEnabled", 0);

                setAttributeValue(oNext, "eEnabled", 1);
                setAttributeValue(oLast, "eEnabled", 1);

            }

            else {

                // MCR 37302 : si je suis en dernière fiche,  desactive next et  derniere fiche                
                if (nIdxCurrentId == adList.length - 1) {
                    setAttributeValue(oFirst, "eEnabled", 1);
                    setAttributeValue(oPrev, "eEnabled", 1);
                    setAttributeValue(oNext, "eEnabled", 0);
                    setAttributeValue(oLast, "eEnabled", 0);
                }

                else {
                    setAttributeValue(oFirst, "eEnabled", 1);
                    setAttributeValue(oPrev, "eEnabled", 1);
                    setAttributeValue(oNext, "eEnabled", 1);
                    setAttributeValue(oLast, "eEnabled", 1);
                }

            }
        }
        else {

            setAttributeValue(oNext, "eEnabled", 1);
            setAttributeValue(oLast, "eEnabled", 1);
            setAttributeValue(oFirst, "eEnabled", 1);
            setAttributeValue(oPrev, "eEnabled", 1);
        }

        // #53143 : si le bouton Mode Liste redirige vers le dernier sous-onglet affiché, on affiche son libellé à la place
        if (oNavManager.HasRecent('List', nTab)) {
            var nRecentTab = oNavManager.GetRecentId('List', nTab);
            // Ce libellé n'est utilisé que si on redirige vers le même onglet, ou si la redirection vers un autre onglet est activée
            if (goTabListFromDifferentTab(nTab) || nRecentTab == nTab) {
                var sCustomGoTabListLabel = eParam.GetParam("WebTabLabel_" + nRecentTab);
                var goTabListLink = document.getElementById("goTabListRightMenuLink");
                if (sCustomGoTabListLabel != '' && goTabListLink)
                    SetText(goTabListLink.querySelector(".rightMenuSpan_adjust"), sCustomGoTabListLabel);
            }
        }

    }
    catch (e) {
        // En cas de problème, les boutons sont activés.
        // 

        setAttributeValue(oNext, "eEnabled", 1);
        setAttributeValue(oLast, "eEnabled", 1);
        setAttributeValue(oFirst, "eEnabled", 1);
        setAttributeValue(oPrev, "eEnabled", 1);
    }

}




// Deprecated ?
function AddDelClassInImg(oBtn, bAdd, ClassToAdd) {
    if (!oBtn)
        return;
    var oImg = oBtn.getElementsByTagName("img");
    if (!oImg || oImg.length <= 0)
        return;
    if (oImg.length > 0)
        oImg = oImg[0];
    var sClass = oImg.className;
    if (bAdd) {
        if (sClass.indexOf(ClassToAdd) < 0)
            sClass = sClass + ClassToAdd;
        oImg.className = sClass;
    }
    else {
        if (sClass.indexOf("off") >= 0)
            sClass = sClass.replace(ClassToAdd, "");
        oImg.className = sClass;
    }
}


///summary
/// Fonction appelée par les boutons de navigations du menu du mode fiche pour naviguer d'une fiche à l'autre
///<param name="nAction">0 = suivant, 1 =  précédent</param>
///summary
function BrowseFile(oBtn) {
    //Frame des param
    var eParam = getParamWindow();
    if (!eParam)
        return;


    // Action
    // 0 : Fiche Suivante
    // 1 : Fiche précédente
    // 2 : 
    // 3 :
    var nAction = parseInt(getAttributeValue(oBtn, "eAction"));
    var bEnabled = getAttributeValue(oBtn, "eEnabled") == "1";
    if (!bEnabled)
        return;

    //récupération de la fiche en cours
    var currentFile = document.getElementById("fileDiv_" + nGlobalActiveTab);
    if (currentFile && currentFile.tagName && currentFile.tagName.toLowerCase() != "div")
        return;


    //DescId & Id de la ficher en cours
    var nFileId = currentFile?.getAttribute("fid");
    var nTab = currentFile?.getAttribute("did");

    if (!isNumeric(nFileId) || !isNumeric(nTab))
        return;


    //Page en cours
    var nPage = eParam.GetParam("Page_" + nTab);
    if (!isNumeric(nPage)) {
        nPage = 1;
        eParam.SetParam("Page_" + nTab, 1)
    }


    // Ids de la page en cours
    var sIdsList = "";
    if (eParam.GetParam("List_" + nTab) != "" && eParam.GetParam("List_" + nTab).length > 0) {
        sIdsList = CleanListIds(eParam.GetParam("List_" + nTab));
    }


    if (sIdsList.length == 0) {

        // Pas de liste d'id, on doit la charger
        // le chargement doit etre en synchrone pour que le rappel ne se fasse pas avant la maj
        if (!LoadIdsPage(nTab, nPage))
            return;

        sIdsList = CleanListIds(eParam.GetParam("List_" + nTab));

    }

    //Index de l'id en cours
    var adList = sIdsList.split(";");
    var nIdxCurrentId = getIdxArray(adList, nFileId);


    //Id pas dans la liste
    //  Indique que l'utilisateur ne vient pas d'une liste (par exemple d'un MRU).
    //  Dans ce cas, on part sur la liste page en cours, idx = 0
    if (nIdxCurrentId == -1) {

        if (isNumeric(adList[0]))
            loadFile(nTab, adList[0], 3);

        return;
    }


    var nTargetFileId = nFileId;
    var bChangePage = false;

    //
    if (nAction == 2) //1er enregistrement de la page
        nTargetFileId = adList[0];
    else if (nAction == 3)
        nTargetFileId = adList[adList.length - 1]; // dernier enregistrement de la liste
    else if (nAction == 1 || nAction == 0) {

        // enregistrement suivant ou précédent
        var nMaxSearch = 10;
        var nCurrSearch = 0;
        while (nTargetFileId == nFileId) {

            //Max search
            if (nCurrSearch > nMaxSearch)
                return;

            if (nAction == 1) {


                if (nIdxCurrentId > 0) {
                    nTargetFileId = adList[nIdxCurrentId - 1];
                    nIdxCurrentId--;
                }
                else {

                    // Load Page précédente
                    nPage--;


                    // 1er page, le bouton doit être désactivé
                    if (nPage == 0) {
                        // TODO : 1er fiche de la 1er page
                        setAttributeValue(oBtn, "eEnabled", 0);
                        return;
                    }



                    if (LoadIdsPage(nTab, nPage)) {

                        eParam.SetParam("Page_" + nTab, nPage);

                        sIdsList = CleanListIds(eParam.GetParam("List_" + nTab));
                        if (sIdsList.length == 0) {
                            // AUCUN ID
                            return;
                        }

                        adList = sIdsList.split(";");

                        //réinitialisation de l'indexe 
                        nIdxCurrentId = adList.length;



                    }
                    else {
                        //erreur de chargement des ids
                        // désactivation du bouton
                        setAttributeValue(oBtn, "eEnabled", 0);
                        return;
                    }
                }
            }
            else if (nAction == 0) {

                if (nIdxCurrentId < adList.length - 1) {
                    nTargetFileId = adList[nIdxCurrentId + 1];
                    nIdxCurrentId++;
                }
                else {
                    // Load Page suivante
                    nPage++;

                    // dernière page, le bouton doit être désactivé
                    if (nPage == 999) {
                        // TODO : dernière fiche de la dernière page
                        //  return;
                    }



                    if (LoadIdsPage(nTab, nPage)) {

                        eParam.SetParam("Page_" + nTab, nPage);
                        nIdxCurrentId = -1;
                        sIdsList = CleanListIds(eParam.GetParam("List_" + nTab));
                        if (sIdsList.length == 0) {
                            // AUCUN ID
                            return;
                        }

                        adList = sIdsList.split(";");

                    }
                    else {
                        //erreur de chargement des ids
                        setAttributeValue(oBtn, "eEnabled", 0);
                        return;
                    }
                }
            }
        }
    }


    if (nIdxCurrentId == 0 && nPage == 1) {
        //Désactive 1er fiche / dernière fiche
        var oFirst = document.getElementById("BrowsingFirst");
        var oPrev = document.getElementById("BrowsingPrev");

        setAttributeValue(oFirst, "eEnabled", 0);
        setAttributeValue(oPrev, "eEnabled", 0);

    }


    if (nTargetFileId != nFileId)
        loadFile(nTab, nTargetFileId, 3);
    else {
        // TODO : aucune fiche a charger !
    }

    return;

}


///Recherche les ID's des enregistrements du mode liste pour la page/table passé en param
//   pour le mode signet, fournir la Table parente et son Id
// Met à jour la fram param
function LoadIdsPage(nTab, nPage, nParentTab, nParentFileId) {


    //Liste d'id disponible pour le pagging de la table de la fiche en cours
    var oeParam = getParamWindow();
    if (!oeParam)
        return "";


    if (!isNumeric(nTab) || !isNumeric(nPage)) {
        return "";
    }

    var oListIdsUpdater = new eUpdater("mgr/eQueryManager.ashx", 1);
    oListIdsUpdater.ErrorCallBack = function () { };
    oListIdsUpdater.asyncFlag = false;
    oListIdsUpdater.addParam("type", 3, "post");

    var nRows = 0;
    if (typeof (oeParam.GetParam) != "undefined" && oeParam.GetParam('Rows') != '') {
        nRows = oeParam.GetParam('Rows');
    }

    oListIdsUpdater.addParam("rows", nRows, "post");
    oListIdsUpdater.addParam("tab", nTab, "post");
    oListIdsUpdater.addParam("page", nPage, "post");

    //
    if (typeof (nParentTab) != "undefined" && typeof (nParentFileId) != "undefined") {

        if (!isNumeric(nParentTab) || !isNumeric(nParentFileId)) {
            return "";
        }

        oListIdsUpdater.addParam("parenttab", nParentTab, "post");
        oListIdsUpdater.addParam("parentfileid", nParentFileId, "post");
    }


    oListIdsUpdater.send();


    //
    if (oListIdsUpdater.Error)
        return false;
    else {

        var sList = oListIdsUpdater.Result;

        if (sList == "-1") {
            // pas d'id pour la page demandé - numéro de page > page max
            return false;
        }

        sList = CleanListIds(sList);
        if (sList.length > 0) {
            return oeParam.SetParam("List_" + nTab, sList);
        }

        return false;
    }
}


///summary
///Fonction retournant la dernière page d'ids chargées en paramètres
///<param name="listContainerDiv">Div conteneur des pages d'ids</param>
///summary
function GetLastPage(listContainerDiv) {
    if (listContainerDiv == null)
        return null;

    var aListDiv = listContainerDiv.getElementsByTagName("div");

    if (aListDiv.length == 0)
        return null;
    else
        return aListDiv[aListDiv.length - 1];
}

///summary
///Fonction retournant la dernière id de fiche chargée en paramètres (dernier Id de la dernière page)
///<param name="listContainerDiv">Div conteneur des pages d'ids</param>
///summary
function GetLastId(listContainerDiv) {

    if (listContainerDiv == null)
        return null;

    var aListDiv = listContainerDiv.getElementsByTagName("div");

    if (aListDiv == null)
        return null;
    var lastPage = aListDiv[aListDiv.length - 1];
    var listArray = GetText(lastPage).split("$|$");

    return listArray[listArray.length - 1];
}


function traceDebug(s, clear) {
    if (s == null)
        s = "**null**";
    var deb;
    var sDeb = "";
    if (document.getElementById("debugdiv") != null) {
        deb = document.getElementById("debugdiv");
        sDeb = deb.innerHTML;
    }
    else {
        deb = document.createElement("div");
        deb.id = "debugdiv";
        deb.className = "debugdiv";
        document.body.appendChild(deb);
    }
    if (clear == true)
        sDeb = "";
    else
        sDeb = "<hr>" + sDeb;

    deb.style.top = (parseInt(document.body.offsetHeight) - 200) + "px";
    deb.style.left = (parseInt(document.body.offsetWidth) - 200) + "px";
    deb.innerHTML = Date() + " : <br>" + s + sDeb;

}

//Permet d'ouvrir et de refermer le menu escamotable
// bOpen : indique si le menu doit être ouvert (true) ou fermé (false). Si null ou non précisé = ouverture/fermeture auto
// e : évènement ayant déclenché l'appel à la fonction (issu d'un onclick, onmouseover, onmouseout...). Permet d'empêcher
//     l'exécution de la fonction selon le contexte (voir ci-dessous)
function ocMenu(bOpen, e) {

    var oRightMenu = document.getElementById('rightMenu');
    if (!oRightMenu)
        return;

    // Pas d'ouverture/fermeture si le menu est épinglé (sur PC) hors accueil
    var oMenuBar = document.getElementById('menuBar');
    if (!isTablet() && oMenuBar && getAttributeValue(oMenuBar, "pinned") == "1" && nGlobalActiveTab != 0)
        return;

    // Switch automatique ouvert/fermé si ce n'est pas précisé
    if (typeof (bOpen) == "undefined" || bOpen == null) {
        bOpen = oRightMenu.className.indexOf('touchOpen') == -1;
    }

    var bCursorIsOnMenu = false;
    var bEventIsOnClick = false;
    // Si la fonction reçoit en paramètre un évènement, on récupère la position de la souris
    if (!e && window.event)
        e = window.event;
    if (e) {
        var oMouseCursorPosition = GetTip(e);
        // On récupère le positionnement actuel et réel du menu droit et donc, de son contenu
        var oRightMenuPosition = getAbsolutePosition(oRightMenu);
        // Et on vérifie si le curseur se situe au sein de la zone du menu droit
        // Avec une marge correspondant à la largeur de la barre de menu
        var nMargin = 30;
        if (oMenuBar)
            nMargin = getAbsolutePosition(oMenuBar).w;
        if (oMouseCursorPosition && oRightMenuPosition &&
            oMouseCursorPosition.x >= oRightMenuPosition.x - nMargin &&
            oMouseCursorPosition.x <= (oRightMenuPosition.x + oRightMenuPosition.w) &&
            oMouseCursorPosition.y >= oRightMenuPosition.y &&
            oMouseCursorPosition.y <= (oRightMenuPosition.y + oRightMenuPosition.h)
        )
            bCursorIsOnMenu = true;

        bEventIsOnClick = e.type == "click";
    }

    // Si l'élément concerné a permis de déterminer que le curseur de la souris se trouve déjà sur un élément conteneur
    // du menu, on ne fait rien.
    // Cela permet d'éviter que le menu se referme immédiatement après ouverture, via l'évènement onmouseout
    // présent sur le bouton du menu (déclenché systématiquement, puisque l'ouverture du menu provoque,
    // de facto, le déplacement du bouton, qui ne se retrouve alors plus sous le curseur de la souris)
    // On ne tient pas compte de cela sur tablettes, où le clic doit systématiquement servir à ouvrir/fermer
    if (bCursorIsOnMenu && !bOpen && !bEventIsOnClick)
        return;

    // Ajout d'une classe pour matérialiser l'ouverture du menu (le bouton devient une barre de menu noire)
    // On réalise ensuite l'inversion de classes pour faire apparaître ou disparaître le menu
    if (bOpen) {
        addClass(oRightMenu, "touchOpen")
        switchClass(oRightMenu, "FavLnkClosed", "FavLnkOpen");

        //Masquer la liste des notifications
        if (notifListIsOpen()) {
            var divWrapper = document.getElementById("notifListWrapper");
            notifListHide(divWrapper);
        }
    }
    else {
        removeClass(oRightMenu, "touchOpen");
        switchClass(oRightMenu, "FavLnkOpen", "FavLnkClosed");
    }
}

//Permet de fixer ("épingler") le menu escamotable, sur PC et hors accueil
function pinMenu(nUserId) {

    var oMenuBar = document.getElementById("menuBar");
    var adminMode = getAttributeValue(oMenuBar, "adminmode") === "1";

    // Pas d'épinglage possible depuis la page d'accueil
    if (!nGlobalActiveTab || nGlobalActiveTab == 0 || adminMode)
        return;

    var oRightMenu = document.getElementById("rightMenu");

    if (!oRightMenu || !oMenuBar)
        return;

    // Pas d'épinglage possible si on est en mode tablettes et que le clic a été déclenché
    // pour ouvrir le menu (il est alors en mode "Fermé")
    // On se contente alors uniquement de réaliser l'inversion de classes pour l'affichage
    if (isTablet() && oRightMenu.className.indexOf("FavLnkClosed") != -1) {
        switchClass(oRightMenu, "FavLnkClosed", "FavLnkOpen");
        return;
    }

    var menuPinned = getAttributeValue(oMenuBar, "pinned") == "1";
    if (menuPinned) {
        switchClass(oRightMenu, "FavLnkOpen", "FavLnkClosed");
        menuPinned = false;
    }
    else {
        switchClass(oRightMenu, "FavLnkClosed", "FavLnkOpen");
        menuPinned = true;
    }

    //Masquer la liste des notifications
    if (notifListIsOpen()) {
        var divWrapper = document.getElementById("notifListWrapper");
        notifListHide(divWrapper);
    }

    // Mise à jour de l'attribut reflétant la pref
    setAttributeValue(oMenuBar, "pinned", (menuPinned ? "1" : "0"));

    // Mise à jour de la préférence en base et rafraîchissement de l'affichage
    var updatePref = "rightmenupinned=" + (menuPinned ? "1" : "0") + ";$;userid=" + nUserId + ";$;category=0";
    updateUserPrefGbl('mgr/ePrefAdvConfigAdvManager.ashx', updatePref, function () {
        // Réajustement du conteneur principal
        adjustDivContainer();

        // Réajustement du champ mémo/notes si actifs
        AjustBkmMemoToContainer(nGlobalActiveTab);

        // Réajustement de la dernière colonne en mode Liste
        adjustLastCol(nGlobalActiveTab);
        // Réajustement de la barre de navigation
        //onTabSelect(nGlobalActiveTab);

        var currentView = getCurrentView(document);
        top.bReloadMRU = 1;

        // Rafraîchissement du planning (spécifique)
        if (currentView == "LIST") {
            var to = 1 // LIST
            loadNavBar(to);
        } else {
            loadNavBar();
        }

        // Rafraîchissement du planning (spécifique)
        if (currentView == "CALENDAR") {
            ReloadCalendar();
        }

        oEvent.fire("right-menu-pinned", { 'state': (menuPinned ? "1" : "0") });

    }, function () { });

}

// Evènement déclenché lors de l'animation du menu droit
function afterRightMenuTransition() {
    // Sur tablettes, si la navbar optimisée est activée, on redimensionne le "+" de façon à ce qu'il se cale au menu droit
    refreshTabletNavBarWidth();
}

function refreshTabletNavBarWidth() {

    /*
    Il a été décidé de ne pas rendre le menu "épinglable" sur tablettes.
    Cette fonction de rafraîchissement devient donc caduque, sauf si on décide de rafraîchir la navbar tablettes lorsque
    le menu escamotable s'ouvre.
    */
    /*
    if (isTablet() && isTabletNavBarEnabled() && nGlobalActiveTab > 0) {
        var oPlusNav = document.getElementById("navPlus");
        var oRightMenu = document.getElementById("rightMenu");
        var nRightMenuWidth = GetRightMenuWidth();
        if (oPlusNav && nRightMenuWidth > 0) {
            var nPlusWidth = 35 + nRightMenuWidth + 10; // 35 pixels : taille par défaut du "+" + marge
            createCss("customCss", "PlusFixedWidth", "width: " + nPlusWidth + "px !important;", true);
            addClass(oPlusNav, "PlusFixedWidth");
        }
    }
    */
}

//Permet d'ouvrir et de fermer le colorPicker
//function OpClsColrPckr() {
//    var oClrPckr = document.getElementById("colorPicker");
//    if (!oClrPckr)
//        return;

//    if (oClrPckr.className.toLowerCase().indexOf("favlnkopen") > 0)
//        switchClass(oClrPckr, "FavLnkOpen", "FavLnkClosed");
//    else if (oClrPckr.className.toLowerCase().indexOf("favlnkclosed") > 0)
//        switchClass(oClrPckr, "FavLnkClosed", "FavLnkOpen");
//}

// Popup des template Emailing
function popup(Title, srcImg) {

    var img = new Image();
    img.src = srcImg;

    if (typeof (setEventListener) == "function") {
        setEventListener(img, 'load', function (evt, bIgnoreImageSize) {

            var source = null;
            if (evt && evt.srcElement)
                source = evt.srcElement;
            else
                if (evt && evt.target)
                    source = evt.target;

            if (source == null) {
                alert("Unknown image !");
                return;
            }

            var width = source.width;
            var height = source.height;

            var winSize = getWindowSize();
            var w = winSize.w;
            var h = winSize.h;

            if (w > width) w = width;
            if (h > height) h = height;

            var oModal = new eModalDialog(Title, 10, null, w, h);
            oModal.noButtons = true;

            //var div = document.createElement('div');
            //div.className = "imgZoom";
            var oIMG = document.createElement('IMG');
            oIMG.src = srcImg;
            oIMG.className = "imgZoom";
            // div.innerHTML = oIMG ;
            oModal.setElement(oIMG);
            oModal.show();


        }, false);
    }
}

function GetNbFavLinks() {
    // Taille de la fenêtre
    if (typeof (top.window.innerHeight) != 'undefined')
        var heightRemaining = top.window.innerHeight;
    else if (typeof (top.document.documentElement.clientHeight) != 'undefined')
        var heightRemaining = top.document.documentElement.clientHeight;
    else {
        alert('ANOMALIE');
        return;
    }

    // Hauteur du bouton liste des rapports
    heightRemaining -= 40;

    // Hauteur de la palette
    heightRemaining -= 103;

    // Hauteur de l'user infos
    heightRemaining -= 180;

    // Hauteur de chaque li 
    var rows = Math.floor(heightRemaining / 34);

    if (rows <= 0)
        rows = 10;

    return rows;
}
/*DEBUT - ORGANIGRAMME*******************************************************/
var modalOrga = null;
var modebetaOrga = false;    //si à true affiche le choix entre jquery et syncfusionfset
//Type du message
var OrgaModalType =
{
    ShowDialog: 0   //Page organigramme en popup
};
//Type du message
var OrgaType =
{
    jQuery: 0
};


function ShowOrga(nTab, nFileId, nTypeOrga) {
    if (modebetaOrga && (!nTypeOrga || typeof nTypeOrga == "undefined")) {
        selectOrgaType(nTab, nFileId);
        return;
    }
    setWait(true);

    if (!nTypeOrga || nTypeOrga == OrgaType.jQuery) {
        var sUrl = 'mgr/eOrganigrammeManager.ashx';

        modalOrga = new eModalDialog("Organigramme", 0, sUrl);    //TODORES
        modalOrga.forceWindowMaxSize();
        modalOrga.addParam("tab", nTab, "post");
        modalOrga.addParam("fileid", nFileId, "post");
        modalOrga.addParam("operation", OrgaModalType.ShowDialog, "post");

        modalOrga.onIframeLoadComplete = onModalOrgaLoad;

        modalOrga.show();

        modalOrga.addButton(top._res_30, null, 'button-gray', null, "cancel");   //Fermer
    }
}

//JS a chargé à l'ouverture du finder
function onModalOrgaLoad(iFrameId) {
    try {

        var oFrm = modalOrga.getIframe();
        var oFrmDoc = oFrm.document;
        var oFrmWin = oFrm.window;

        // Donne le focus à la textbox de recherche
        if (oFrmDoc.getElementById('eTxtSrch')) {
            oFrmDoc.getElementById('eTxtSrch').focus();
        }
        addCss("jquery.jOrgChart", "ORG", oFrmDoc);
        addCss("jquery.prettify", "ORG", oFrmDoc);
        addCss("eOrganigramme", "ORG", oFrmDoc);
        addCss("theme", "ORG", oFrmDoc);

        var tabScript = new Array();
        tabScript.push("eOrganigramme");
        tabScript.push("jquery.prettify");
        tabScript.push("jquery.min");
        tabScript.push("jquery-ui.min");
        tabScript.push("jquery.orgchart");

        var source = eTools.caller(arguments);
        addScripts(tabScript, "ORG", function () {
            /*if (modebetaOrga) {*/
            try {
                showJqueryGraph();
            }
            finally {
                setWait(false);
            }
            /*TODO
                } else {
                    ShowOrga(nTab, nFileId, OrgaType.jQuery);
                }*/
        }, oFrmDoc);


    }
    catch (exp) {
    }
}

function showJqueryGraph() {
    var oFrm = modalOrga.getIframe();
    var oFrmDoc = oFrm.document;
    var oFrmWin = oFrm.window;
    oFrm.$("#org").jOrgChart({
        chartElement: '#chart',
        dragAndDrop: false
    });
}



function selectOrgaType(nTab, nFileId) {

    var modChoiceOrga = eConfirm(1, ""/*titre*/, "Quel organigramme ?", "", 600, 200, function () { onValidOrgaType(modChoiceOrga, nTab, nFileId); });
    //Indique que l'on souhaite que les options soient des RadioBoutons
    modChoiceOrga.isMultiSelectList = false;
    //RadioBouton 1 : ouvrir cette occurence
    modChoiceOrga.addSelectOption("JQUERY", OrgaType.jQuery, true);
    //RadioBouton 2 : ouvrir la série
    modChoiceOrga.addSelectOption("SYNCFUSION", OrgaType.Syncfusion, false);
    //Force l'ajout des radioboutons
    modChoiceOrga.createSelectListCheckOpt();
}

//Méthode appelée à la confirmation de selectOpenSeries
function onValidOrgaType(modChoiceOrga, nTab, nFileId) {
    if (!modChoiceOrga) {
        eAlert(0, top._res_72, top._res_6658, '');
        throw "onValidOrgaType - modal non trouvée.";
    }
    //Récupère les valeurs sélectionnées ici "0" ou "1"
    var nTypeOrga = modChoiceOrga.getSelectedValue();
    ShowOrga(nTab, nFileId, nTypeOrga);
}

function cancelOrga() {
    if (modalOrga != null)
        modalOrga.hide();
}


/*FIN - ORGANIGRAMME*******************************************************/

/*Début - CARTO*******************************************************/
//Type du message
var CartoModalType =
{
    ShowDialog: 0   //Page organigramme en popup
};

var modalCarto;
function ShowCartoMulti(nTab) {
    var sUrl = 'eCartoMulti.aspx';

    setWait(true);
    modalCarto = new eModalDialog("Cartographie", 0, sUrl, "99%", "99%");    //TODORES

    modalCarto.addParam("tab", nTab, "post");
    modalCarto.addParam("operation", CartoModalType.ShowDialog, "post");

    modalCarto.onIframeLoadComplete = function () { top.setWait(false); };

    modalCarto.show();

    modalCarto.addButton(top._res_30, null, 'button-gray', null, "cancel");   //Fermer
}

function ShowCartoSVG(nTab) {
    var sUrl = 'eCartoSVG.aspx';

    setWait(true);
    modalCarto = new eModalDialog("Répartition par départements", 0, sUrl, "900px", "100%");    //TODORES

    modalCarto.addParam("tab", nTab, "post");
    modalCarto.addParam("operation", CartoModalType.ShowDialog, "post");
    modalCarto.addParam("iframeScrolling", "yes", "post");

    modalCarto.onIframeLoadComplete = function () { top.setWait(false); };

    modalCarto.show();

    modalCarto.addButton(top._res_30, null, 'button-gray', null, "cancel");   //Fermer
}

/*FIN - CARTO*******************************************************/

/*Début - THEMES*******************************************************/
// Fonction de callback par défaut appelée au changement de thème, si non précisée
function applyThemeDefaultCallback() {
    self.location.reload();
};

function applyTheme(nNewThemeId, nUserId, callback, errorCallback) {

    var updatePref = "theme=" + nNewThemeId + ";$;userid=" + nUserId + ";$;category=0";


    var switchWrap = document.querySelector('.switch-new-theme-wrap');
    if (switchWrap) {
        switchWrap.classList.remove('sombre');

        /** on parcourt les thèmes. et si le thème est sombre,
          * on met une couleur claire pour la police. g.l. */
        for (obj in THEMES) {

            if (THEMES[obj].Id == nNewThemeId && THEMES[obj].Sombre == 1) {
                /** mettre le bon thème. */
                switchWrap.classList.add('sombre');
            }
        }
    }

    if (typeof (callback) != "function")
        callback = applyThemeDefaultCallback;
    if (typeof (errorCallback) != "function")
        errorCallback = function () { }

    var currentFont = top.eTools.GetFontSize();
    var themeMaxFont = top.eTools.GetMaxFontSize(nNewThemeId);
    if (themeMaxFont > 0 && currentFont > themeMaxFont) {
        var cfg = {
            'criticity': 1,
            'title': top._res_2372,  //Mise à jour du thème
            'message': top._res_2373,
            'details': "", // .replace("{NEW_SIZE}", themeMaxFont), //Votre taille de police est trop importante, elle va être réduite à {NEW_SIZE}
            //'width': 500,
            //'height': 200,
            'okFct': function () { updateUserPrefGbl('mgr/ePrefAdvConfigAdvManager.ashx', updatePref, function () { self.location.reload(); }, errorCallback); },
            'cancelFct': function () { },
            'bOkGreen': true,
            //'bHtml': bHtml,
            'resOk': top._res_28,
            'resCancel': top._res_29
        }

        eAdvConfirm(cfg);
        return;
    }

    updateUserPrefGbl('mgr/ePrefAdvConfigAdvManager.ashx', updatePref, callback, errorCallback);
}

function applyThemeWithoutReload() {
    var upd = new eUpdater("mgr/eThemeManager.ashx", 0);
    upd.addParam("action", "getThemeCssUrl", "post");
    upd.send(applyThemeWithoutReloadReturn);
}

function applyThemeWithoutReloadReturn(oRes) {
    if (!oRes) {
        return;
    }

    if (getXmlTextNode(oRes.getElementsByTagName("success")[0]) == "1") {
        var themeId = parseInt(getXmlTextNode(oRes.getElementsByTagName("themeId")[0]));
        var themeVersion = parseInt(getXmlTextNode(oRes.getElementsByTagName("themeVersion")[0]));

        var oeParam = getParamWindow();
        oeParam.SetParam('currenttheme', JSON.stringify({ "Id": themeId, "Version": + themeVersion }));

        var url = getXmlTextNode(oRes.getElementsByTagName("url")[0]);
        var themeFolder = getXmlTextNode(oRes.getElementsByTagName("themeFolder")[0]);
        var themeColor = getXmlTextNode(oRes.getElementsByTagName("themeMainColor")[0]);

        var oldThemeLink = "themes/" + sTheme + "/css/theme.css";
        var newThemeLink = "themes/" + themeFolder + "/css/theme.css";

        // Pour le document en cours
        eTools.changeDocumentTheme(oldThemeLink, newThemeLink, themeColor);

        sTheme = themeFolder;
        nThemeId = themeId;

        if (typeof themeInit == "function")
            themeInit();
        //BSE: Bug #79 446 Recharger la fiche
        var fileId = getAttributeValue(document.getElementById("fileDiv_" + nGlobalActiveTab), "fid");
        if (fileId)
            loadFile(nGlobalActiveTab, fileId, 3, false);
    }
}


function switchActiveThemeThumbnail(oSource) {
    var colorPicker = document.getElementById("colorPick");
    if (colorPicker != null) {
        var oThumbs = colorPicker.querySelectorAll('li[class*="themeThumbnail"]');
        for (var i = 0; i < oThumbs.length; ++i) {
            removeClass(oThumbs[i], "activeTheme");

            if (oThumbs[i].contains(oSource))
                addClass(oThumbs[i], "activeTheme");
        }
    }
}
/*FIN - THEMES*******************************************************/

/* Début - handlerCTI 36826 : MCR/ RMA appel CTI *******************************************************/
function handlerCTI(e) {
    var initCTI = getCookie('initCTI');
    if (initCTI == '0')
        return;

    var mypn = localStorage.getItem('pn');

    if ((mypn == null || mypn == '' || mypn == 'eudonet') && e.newValue != '' && e.newValue != null && e.key == 'pn')
        mypn = e.newValue;

    // # MCR : 38964 : correctif CTI pour le navigateur IE
    if (mypn != null && mypn != 'eudonet') {

        // # MCR : 38964 : correctif CTI : si une fenetre modale est ouverte en mode fiche, le CTI n est pas active        
        // Cette action ne doit pas se déclencher si une fenêtre popup est ouverte
        var bIsPopupCTI = window.modalWindowsOpened();
        if (!bIsPopupCTI) {
            if (ctiSpecifId == 0) {

                // Appel à la fenêtre de recherche avec le n° de l'appelant et le type d'appel sAction=CTI
                if (!top.eLinkCatFileEditorObject)  //N'est pas initialisé si l'on vient de la HomePage
                {
                    top.eLinkCatFileEditorObject = new eFieldEditor('linkCatFileEditor', ePopupObject, 'eLinkCatFileEditorObject', 'eLinkCatFileEditor');
                }


                top.eLinkCatFileEditorObject.openLnkFileDialog(FinderSearchType.CTI, 200, null, null, null, mypn);
                localStorage.setItem('pn', 'eudonet');
                //localStorage.removeItem('pn');

            }
            else {
                // 41250 appel de la specif du CTI pour CCI77 renseigne dans la table SPECIFS pour un type de specif : TYP_SPECIF_CTI =13

                // appel de la specif CTI
                setCookie('pnini', mypn, false);
                sessionStorage.setItem("pnini", mypn);

                // ctiSpecifId : l id de la specif du CTI, renseigne par eMain.aspx en variable globale
                runSpec(ctiSpecifId);;
                // lancement de la specif 

                setCookie('initCTI', '0', false);

                localStorage.setItem('pn', 'eudonet');
                //localStorage.removeItem('pn');
            }
        }
        // else console.log("CTI, une fenetre modale est ouverte en mode fiche, le CTI n est pas active");
    }
}

//BSE : ajout du timeout pour laisser un temps de chargement et déclenchement de l'evenementment
function setHandlerCTI() {
    var x = setTimeout(handlerCTI, 100);
    clearTimeout(x);
}

/*FIN - handlerCTI 36286 : MCR/ RMA appel CTI *******************************************************/

function AddPjPtiesBtns(modPjPties, nTab, nFileId, nPJId) {
    var doc = modPjPties.getIframe().document;
    var inptRO = doc.getElementById("ro");
    var inptSupp = doc.getElementById("supp");
    var bReadOnly = true;
    var bSupp = false;

    if (inptRO)
        bReadOnly = inptRO.value == "1";

    if (inptSupp)
        bSupp = inptSupp.value == "1";

    if (!bReadOnly) {
        modPjPties.addButton(top._res_29, function () { modPjPties.hide(); }, "button-gray"); // Annuler
        if (bSupp)
            modPjPties.addButton(top._res_19, function () { DeletePJ(nTab, nFileId, nPJId, true); modPjPties.hide(); }, "button-red"); // Supprimer
        modPjPties.addButton(top._res_28, function () { renamePJ(modPjPties, nTab, nFileId, nPJId); }, "button-green"); // Valider
    }
    else
        modPjPties.addButton(top._res_30, function () { modPjPties.hide(); }, "button-gray"); // fermer

}


//Objet gérant les onglets/sous-onglets de type web
var oWebMgr = (function () {

    ItemType = {
        SPECIF: 0,
        GRID: 1,
        LIST: 2
    };
    var _nTab = nGlobalActiveTab;

    // FONCTIONS INTERNES/PRIVEES

    //Charge l'sous-onglet web
    function loadSpecif(specifId) {

        //Au clique sur "Liste" on recharge la liste de la table
        if (specifId == 0) {
            if (typeof (nGlobalActiveTab) != "undefined" && nGlobalActiveTab > 0)
                onTabSelect(nGlobalActiveTab);
            else
                onTabSelect(0);

            return;
        }

        setWait(true);

        var winSize = getWindowSize();
        var oUpdater = new eUpdater("mgr/eWebTabManager.ashx", 1);

        oUpdater.addParam("H", winSize.h, "post");
        oUpdater.addParam("W", winSize.w, "post");
        oUpdater.addParam("activetab", nGlobalActiveTab, "post");
        oUpdater.addParam("specifid", specifId, "post");
        oUpdater.ErrorCallBack = onError;
        oUpdater.async = true;
        oUpdater.send(onSuccess);
    }

    /// En cas de success
    function onSuccess(html) {

        if (html != null) {

            var mainDiv = document.getElementById("mainDiv");

            // On enleve la div des filtres
            var listFilters = document.getElementById("listfiltres");
            if (listFilters)
                listFilters.parentElement.removeChild(listFilters);

            // On remplace le conteneur principale par le contenu de la specifs

            var mainContainer;
            if (_nTab == 0)
                mainContainer = document.getElementById("HpContainer");
            else
                mainContainer = document.getElementById("listheader");

            try {
                mainContainer.innerHTML = html;
            } catch (ex) {
                eTools.log.inspect(ex);
            }

            // On garde les liens favoris pour l'accueil
            if (_nTab != 0)
                loadFileMenu(_nTab, 8, 0);

        }

        setWait(false);
    }

    /// En cas d'erreur
    function onError() {
        //eAlert(2, "Connexion au serveur", "Connexion au serveur impossible", "Merci de vérifier votre connexion");
        setWait(false);
    }

    // l'element sélectionné est en gras (avec la class selected)
    function switchSelected(evt) {
        var src = evt.srcElement || evt.target;
        return switchSelectedElt(src);
    }
    function switchSelectedId(id) {
        var allSrc = document.querySelectorAll('#SubTabMenuCtnr span a[gid="' + id + '"]');
        if (allSrc.length > 0)
            return switchSelectedElt(allSrc[0]);
    }

    function switchSelectedElt(src) {
        //switch SubTabMenuCtnr
        var oAllAnchrs = document.querySelectorAll('#SubTabMenuCtnr span a[class$="selected"]');
        for (var i = 0; i < oAllAnchrs.length; i++)
            removeClass(oAllAnchrs[i], "selected");

        addClass(src, "selected");
    }

    // On affiche pas la tools bar dans le cas d'une specif web
    function showOrHideToolBarItems(id) {
        if (_nTab == 0)
            return;

        var fontSizePrinter = document.getElementById("barre-outils");
        var iconFilter = document.getElementById("iconeFilter");
        var SpanNbElem = document.getElementById("SpanNbElem");
        var SpanLibElem = document.getElementById("SpanLibElem");

        if (id != 0) {
            addClass(fontSizePrinter, "hide");
            addClass(iconFilter, "hide");
            addClass(SpanNbElem, "hide");
            addClass(SpanLibElem, "hide");
        } else {
            removeClass(fontSizePrinter, "hide");
            removeClass(iconFilter, "hide");
            removeClass(SpanNbElem, "hide");
            removeClass(SpanLibElem, "hide");
        }
    }

    function updateWebTabContent(html) {
        var mainDiv = document.getElementById("mainDiv");
        mainDiv.innerHTML = html;
        switchActiveTab(_nTab);
        loadFileMenu(_nTab, 8, 0);


        //  oXrmHomeGrid.resetAll();
    }

    // FONCTIONS PUBLIQUES/ACCESSIBLES VIA oWebMgr.nomFonction

    return {
        onIframeLoaded: function (iframe, type) {

        },
        loadSpec: function (id, itemType, evt) {
            if (typeof (id) != "undefined" && id != null && isInt(id)) {
                _nTab = nGlobalActiveTab;
                if (itemType == ItemType.GRID) {
                    if ((typeof (oGridController) !== "undefined")) {
                        oGridController.config.updateFromWebTab(true);
                        //oGridController.grid.loadGrid(id);
                    }

                    if (typeof oGridToolbar !== "undefined") {
                        oGridToolbar.loadById(id, itemType, 0, evt);
                    }

                }
                else {
                    loadSpecif(id);
                }
                if (evt)
                    switchSelected(evt);
                else
                    switchSelectedId(id);

                showOrHideToolBarItems(id);
            }
            if (evt)
                stopEvent(evt);
        },
        loadTab: function (nTab) {
            _nTab = nTab
            var winSize = getWindowSize();
            var oUpdater = new eUpdater("eMainWebTab.aspx", 1);

            oUpdater.addParam("tab", nTab, "post");
            oUpdater.addParam("divH", winSize.h, "post");
            oUpdater.addParam("divW", winSize.w, "post");

            oUpdater.ErrorCallBack = onError;

            oUpdater.send(updateWebTabContent);
        },
        // Renvoie l'élément sélectionné
        getSelectedId: function () {
            //switch SubTabMenuCtnr
            var oAllAnchrs = document.querySelectorAll('#SubTabMenuCtnr span a[class$="selected"]');
            if (oAllAnchrs.length > 0)
                return oAllAnchrs[0].getAttribute("gid");
            else
                return 0; // gid à 0 = mode Liste
        },
        getSelectedItemType: function () {
            //switch SubTabMenuCtnr
            var oAllAnchrs = document.querySelectorAll('#SubTabMenuCtnr span a[class$="selected"]');
            if (oAllAnchrs.length > 0)
                return oAllAnchrs[0].getAttribute("itemtype");
            else
                return 0; // itemtype à 0 = ItemType.SPECIF
        },
        getSelectedLabel: function () {
            //switch SubTabMenuCtnr
            var oAllAnchrs = document.querySelectorAll('#SubTabMenuCtnr span a[class$="selected"]');
            if (oAllAnchrs.length > 0)
                return GetText(oAllAnchrs[0]);
            else
                return ''; // chaîne vide : utilisation du libellé par défaut
        },
        /// nType : type de liste à charger
        //      0 : mainList
        //      1 : bookmark
        //
        loadPrevWebTab: function (nTab, nType) {

            //#65359, Il y a pas d'ordre d'affichage dans le signet web/grille            
            if (typeof (nType) != "undefined" && nType == 1)
                return false;

            var oeParam = getParamWindow();
            if (typeof (oeParam) == "undefined" || !oeParam || typeof (oeParam.GetParam) != "function")
                return false;
            var nPreviousWebTabId = oeParam.GetParam('WebTabId_' + nTab);
            var nPreviousWebTabItemType = oeParam.GetParam('WebTabItemType_' + nTab);
            if (nPreviousWebTabId != '' && nPreviousWebTabItemType != '' && nPreviousWebTabId > 0) {
                oWebMgr.loadSpec(nPreviousWebTabId, nPreviousWebTabItemType, null);
                return true;
            }
            else
                return false;
        }
    }
}());

//Permet de télécharger la vCard sous format VCF
function exportVCard(vc) {
    window.open('mgr/eVCardManager.ashx?vc=' + vc, '_blank');
}


/*Début - NOTIFICATIONS*******************************************************/

/***** Fonctions Toasters *****/

//todo : déplacer les notifs dans un fichier séparé
var oNotifs = (function () {


    var NOTIFS_MAX_ERRORS = 5;

    var NOTIFS_BASE_REFRESH_TIME = 30 * 1000; //en ms
    var NOTIF_MYTIMEOUT = null;

    var currInterval = 30 * 1000;


    var notifToastCall = function () {

        if (oNotifs.StopRefresh)
            return;

        var oToastersUpdater = new eUpdater("mgr/eNotificationManager.ashx", 0);
        oToastersUpdater.bKeepSessionAlive = false;
        oToastersUpdater.addParam("action", "getToaster", "post");
        oToastersUpdater.addParam("post", "1", "post");

        //En cas d'erreur, gestion custom de l'erreur
        oToastersUpdater.ErrorCustomAlert = function () {

            //Incrémente le nb erreur
            oNotifs.NbError++;

            //prochaine recherche dans 3minutes min
            oNotifs.Refresh(3 * minute)
        }

        //oToastersUpdater.addParam("callinterval", notifToastCallInterval, "post");
        oToastersUpdater.send(notifToastCallProcessReturn);
    }

    return {

        //relance la recherche de notif dans nIntDelay ms
        Refresh: function (nIntDelay) {

            //Clear l'éventuel timeout en cours
            if (NOTIF_MYTIMEOUT)
                clearTimeout(NOTIF_MYTIMEOUT);
            //Delai min : NOTIFS_BASE_REFRESH_TIME
            if (typeof (nIntDelay) == "undefined")
                nIntDelay = NOTIFS_BASE_REFRESH_TIME

            nIntDelay = nIntDelay * 1;

            if (isNaN(nIntDelay))
                nIntDelay = NOTIFS_BASE_REFRESH_TIME

            currInterval = Math.max(nIntDelay, NOTIFS_BASE_REFRESH_TIME)

            //Moins de NOTIF_MAX_ERRORS & stoppée
            if (this.NbError < NOTIFS_MAX_ERRORS && !this.StopRefresh) {
                NOTIF_MYTIMEOUT = setTimeout(notifToastCall, this.TimoutInterval);
            }

            if (this.NbError >= NOTIFS_MAX_ERRORS) {
                this.StopRefresh = true;
                top.eAlert(0, top._res_491, top._res_492);
                return;
            }
        }
        ,

        //stop les refres
        StopRefresh: false,



        //Nombre d'erreur successive
        NbError: 0,

        //Nombre maxi d'erreur avant interruption des vérifs
        get Stop() { return this.NbError > NOTIFS_MAX_ERRORS; },

        //Interval entre 2 vérif - Nb fourni + nb erreur seconde
        get TimoutInterval() { return currInterval * 1.0 + Math.min(this.NbError, 10) * 60 * 1000 },  // ajoute une minute pour chaques erreurs successives

    }
})();


var notifNbMaxToasters = 5; //nombre max de toasters à l'ecran en même temps
var notifNbCurrentToasters = 0; //stocke le nombre actuel de toasters à l'ecran

var seconde = 1000;
var minute = 60 * seconde;




function notifToastCallLauncher() {
    oNotifs.Refresh(30 * seconde); //on attend 30 secondes que la page principale soit chargée
}




function notifToastCallProcessReturn(oRes) {

    if (!oRes) {
        oNotifs.Refresh(3 * minute); // si il y a une erreur, on relance la verification des notifs dans 3 minutes
        return;
    }


    //Pas d'erreur, on reset le compteur
    oNotifs.NbError = 0;

    //récupère le nombre de notifications non lues
    var sCountUnread = getXmlTextNode(oRes.getElementsByTagName("CountUnread")[0]);
    notifRefreshCountUnread(sCountUnread);

    //récupère l'interval pour le prochain appel
    var sCallInterval = getXmlTextNode(oRes.getElementsByTagName("CallInterval")[0]);
    if (sCallInterval != "" && sCallInterval != null) {
        oNotifs.Refresh(sCallInterval); //callinterval est en ms
    }
    else {
        oNotifs.Refresh(3 * minute); // si le paramètre est invalide, on relance la verification des notifs dans 3 minutes
    }

    //récupère la liste de notifications à toaster
    var sListNotifications = oRes.getElementsByTagName("ListNotifications")[0].getElementsByTagName("notification");

    if (sListNotifications.length > 0) {
        for (i = 0; i < sListNotifications.length; i++) {
            var obj = sListNotifications[i];

            var sId = getXmlTextNode(obj.getElementsByTagName("id")[0]);
            var nType = parseInt(getXmlTextNode(obj.getElementsByTagName("type")[0]));
            var sTitle = getXmlTextNode(obj.getElementsByTagName("title")[0]);
            //var sDescription = getXmlTextNode(obj.getElementsByTagName("description")[0]);
            var sIcon = getXmlTextNode(obj.getElementsByTagName("icon")[0]);
            var isPicto = getXmlTextNode(obj.getElementsByTagName("picto")[0]) == "1";
            var sColor = getXmlTextNode(obj.getElementsByTagName("color")[0]);
            var audioFile = getXmlTextNode(obj.getElementsByTagName("sound")[0]);

            var objNotif = {
                id: sId,
                type: nType,
                title: sTitle,
                //description: sDescription,
                icon: sIcon,
                color: sColor,
                isPicto: isPicto,
                audio: audioFile
            };

            notifToastTimeout(objNotif, i * seconde);

        }
    }
}


function notifToastTimeout(objNotif, timeout) {
    setTimeout(function () { notifToast(objNotif); }, timeout);
}

//gère la création et l'affichage d'un toaster		
function notifToast(objNotif) {
    if (notifNbCurrentToasters >= notifNbMaxToasters) {
        // si on a atteint le nombre max de toasters, on réessaie dans une seconde
        setTimeout(function () { notifToast(objNotif); }, 1000);
    }
    else {

        // On joue le son de la notif si paramétré
        if (typeof (objNotif.audio) != 'undefined' && objNotif.audio.length > 0)
            Sound.play(objNotif.audio);

        notifNbCurrentToasters++;

        //variables pour les temps d'apparation/affichage/disparition (en ms)
        var fadeInTime = 300;
        var fadeOutTime = 1000;
        var displayTime = 20000; // 20s

        var bHasIcon = typeof (objNotif.icon) != 'undefined';

        var divWrapper = document.getElementById("divNotifToastersWrapper");

        var divNotif = document.createElement("div");
        divNotif.id = "divNotif_" + objNotif.id;
        divNotif.className = "notifToaster";
        divNotif.style.display = "none";
        if (objNotif.selectOnClick)
            divNotif.onclick = function () { selectNotifToast(divNotif, objNotif.selectTitle, objNotif.selectLabel, objNotif.selectValue, true); }
        else
            divNotif.setAttribute("onclick", "closeNotifToast(this);");

        if (objNotif.type == 2) { //type Alert
            var imgNotif = document.createElement("span");
            imgNotif.className = "notifToasterImgAlert icon-bell2";
            divNotif.appendChild(imgNotif);
        }
        else if (bHasIcon) {
            var imgNotif;
            if (objNotif.isPicto) {
                imgNotif = document.createElement("span");
                imgNotif.className = "notifInfoToastSpan " + objNotif.icon;
                setAttributeValue(imgNotif, "style", "color:" + objNotif.color + " !important;");

            } else {
                imgNotif = document.createElement("img");
                imgNotif.className = "notifInfoImg";
                imgNotif.setAttribute("src", objNotif.icon);
            }

            imgNotif.setAttribute("alt", "");
            imgNotif.setAttribute("title", "");

            divNotif.appendChild(imgNotif);
        }

        var divTxtNotif = document.createElement("div");
        divTxtNotif.className = "notifToasterTxt" + (!bHasIcon ? " notifToasterTxtNoIcon" : "");
        divTxtNotif.innerHTML = objNotif.title;
        divNotif.appendChild(divTxtNotif);

        var linkCloseNotif = document.createElement("div");
        linkCloseNotif.className = "icon-times notifToasterClose"; //icon-edn-cross
        linkCloseNotif.setAttribute("onclick", "closeNotifToast(this.parentNode, event);");
        divNotif.appendChild(linkCloseNotif);


        divWrapper.appendChild(divNotif);

        notifFadeIn(divNotif, fadeInTime);

        //  setTimeout(function () { notifFadeOut(divNotif, fadeOutTime); }, fadeInTime + displayTime);
        setTimeout(function () {

            // Quand on ferme le toaster en cliquant sur la croix, divNotif n'est plus enfant de divWrapper, par ailleurs le setTimeout etait déjà lancé
            // il faut tester la parenté avant d'enlever l'enfant
            if (isParentElementOf(divNotif, divWrapper)) {
                divWrapper.removeChild(divNotif);
                notifNbCurrentToasters--;
            }

        }, fadeInTime + displayTime + fadeOutTime);
    }
}

//gère l'animation d'apparition d'un toaster
//delay en ms
function notifFadeIn(el, delay, display) {
    if (el != null) {
        var maxOpacity = 0.8;
        var startTime = new Date();

        el.style.opacity = 0;
        el.style.display = display || "inline-block";

        (function fade() {
            var now = new Date();
            var timeDiff = now - startTime;

            if (timeDiff >= delay) {
                el.style.opacity = maxOpacity;
            }
            else {
                var newOpacity = timeDiff / delay * maxOpacity;

                el.style.opacity = newOpacity;
                requestAnimationFrame(fade);
            }
        })();
    }
}

//gère l'animation de disparition d'un toaster
//delay en ms
function notifFadeOut(el, delay) {

    if (el != null) {
        var maxOpacity = 0.8;
        var startTime = new Date();

        el.style.opacity = maxOpacity;

        (function fade() {
            var now = new Date();
            var timeDiff = now - startTime;

            if (timeDiff >= delay) {
                el.style.opacity = 0;
                el.style.display = "none";
            }
            else {
                var newOpacity = maxOpacity - (timeDiff / delay * maxOpacity);

                el.style.opacity = newOpacity;
                requestAnimationFrame(fade);
            }
        })();
    }
}

function closeNotifToast(el, event) {
    if (event)
        stopEvent(event);

    if (el != null) {
        el.style.display = "none";
        if (el.parentNode != null)
            el.parentNode.removeChild(el);
    }
}

/* Sélectionne le texte de la notification au clic.
 * Si impossible (ex : sélection désactivée en CSS), on propose le texte de la notification au clic dans une eModalDialog (ou un autre texte si paramétré)
 */
function selectNotifToast(el, customTitle, customLabel, customValue, bSelectValue) {
    var width = 550;
    var height = 200;
    if (!customTitle)
        customTitle = top._res_7443; // Notification

    // Si un texte personnalisé a été paramétré au clic
    if (customLabel || customValue)
        ePrompt(customTitle, customLabel, customValue, width, height, bSelectValue, false); // Notification
    // Sinon, on utilise le texte de la notification
    else {
        // Si la sélection automatique est possible
        if (el && getComputedStyle(document.body).userSelect != "none")
            eTools.selectText(el);
        // Sinon, affichage dans une eModalDialog
        else
            ePrompt(top._res_7443, "", GetText(el), width, height, bSelectValue, false); // Notification
    }
}

/***** Fonctions Liste Notifications *****/

var bNotifShowUnread = false;
var notifNbRowsInit = 4;
var notifNbRows = notifNbRowsInit;
var bNotifRefreshing = false;
var notifIndexLowerInit = 1;
var notifIndexUpperInit = 10;
var notifIndexLower = notifIndexLowerInit;
var notifIndexUpper = notifIndexUpperInit;
var notifPreviousFetchCount = 0;
var notifNextFetchCount = 0;

function notifLinkToggleActive() {
    var linkActiveClassName = "notifHeaderLinkActive";
    var elLinkShow = document.getElementById("notifLinkShow");
    var elLinkShowUnread = document.getElementById("notifLinkShowUnread");

    if (elLinkShow != null) {
        removeClass(elLinkShow, linkActiveClassName);
        if (!bNotifShowUnread)
            addClass(elLinkShow, linkActiveClassName);
    }

    if (elLinkShowUnread != null) {
        removeClass(elLinkShowUnread, linkActiveClassName);
        if (bNotifShowUnread)
            addClass(elLinkShowUnread, linkActiveClassName);
    }
}

function notifShow() {
    notifNbRows = notifNbRowsInit;
    bNotifShowUnread = false;
    notifLinkToggleActive();
    notifListRefresh();
}

function notifShowUnread() {
    bNotifShowUnread = true;
    notifLinkToggleActive();
    notifListRefresh();
}

function notifListRefresh() {
    if (bNotifRefreshing != true) {
        bNotifRefreshing = true;
        //var nbRows = notifNbRows;

        var oListUpdater = new eUpdater("mgr/eNotificationManager.ashx", 0);
        oListUpdater.bKeepSessionAlive = false;
        oListUpdater.addParam("action", "getList", "post");
        oListUpdater.addParam("post", "1", "post");
        //oListUpdater.addParam("nbrows", nbRows, "post");
        oListUpdater.addParam("indexlower", notifIndexLower, "post");
        oListUpdater.addParam("indexupper", notifIndexUpper, "post");
        oListUpdater.addParam("unread", bNotifShowUnread == true ? "1" : "0", "post");
        oListUpdater.send(notifListRefreshProcessReturn);
    }
}

function notifListRefreshProcessReturn(oRes) {
    if (!oRes) {
        bNotifRefreshing = false;
        return;
    }

    //récupère le nombre de notifications non lues
    var sCountUnread = getXmlTextNode(oRes.getElementsByTagName("CountUnread")[0]);
    notifRefreshCountUnread(sCountUnread);

    var sPreviousFetchCount = getXmlTextNode(oRes.getElementsByTagName("PreviousFetchCount")[0]);
    if (sPreviousFetchCount != null && sPreviousFetchCount != "") {
        notifPreviousFetchCount = parseInt(sPreviousFetchCount);
    }

    var sNextFetchCount = getXmlTextNode(oRes.getElementsByTagName("NextFetchCount")[0]);
    if (sNextFetchCount != null && sNextFetchCount != "") {
        notifNextFetchCount = parseInt(sNextFetchCount);
    }

    var divNotifContents = document.getElementById("notifContents");
    if (divNotifContents != null) {
        //on clear la div
        while (divNotifContents.firstChild) {
            divNotifContents.removeChild(divNotifContents.firstChild);
        }

        //récupère les res
        //var sResTagRead = getXmlTextNode(oRes.getElementsByTagName("ResTagRead")[0]);
        //var sResNoNotification = getXmlTextNode(oRes.getElementsByTagName("ResNoNotification")[0]);

        //récupère la liste de notifications
        var sListNotifications = oRes.getElementsByTagName("ListNotifications")[0].getElementsByTagName("notification");

        if (sListNotifications.length > 0) {
            for (i = 0; i < sListNotifications.length; i++) {
                if (i > 0) {
                    var divNotifItemSeparator = document.createElement("div");
                    divNotifItemSeparator.className = "notifItemSeparator";

                    var divNotifItemSeparatorSub = document.createElement("div");
                    divNotifItemSeparator.appendChild(divNotifItemSeparatorSub);

                    divNotifContents.appendChild(divNotifItemSeparator);
                }


                var obj = sListNotifications[i];

                var sId = getXmlTextNode(obj.getElementsByTagName("id")[0]);
                var nType = parseInt(getXmlTextNode(obj.getElementsByTagName("type")[0]));
                //var sTitle = getXmlTextNode(obj.getElementsByTagName("title")[0]);
                var sDescription = getXmlTextNode(obj.getElementsByTagName("description")[0]);
                var sIcon = getXmlTextNode(obj.getElementsByTagName("icon")[0]);
                var isPicto = getXmlTextNode(obj.getElementsByTagName("picto")[0]) == "1";
                var color = getXmlTextNode(obj.getElementsByTagName("color")[0]);
                var sTimespan = getXmlTextNode(obj.getElementsByTagName("timespan")[0]);
                var bRead = getXmlTextNode(obj.getElementsByTagName("read")[0]) == "true" ? true : false;

                //renduHTML
                var divNotifItem = document.createElement("div");
                if (bRead == true) {
                    divNotifItem.className = "notifItem";
                }
                else {
                    divNotifItem.className = "notifItem notifItemUnread";
                }

                var divNotifInfo = document.createElement("div");
                divNotifInfo.className = "notifInfo";

                if (nType == 2) { //type Alert
                    var imgNotifInfo = document.createElement("span");
                    imgNotifInfo.className = "notifInfoImgAlert " + sIcon;
                    setAttributeValue(imgNotifInfo, "style", "color:" + color + " !important;");

                    divNotifInfo.appendChild(imgNotifInfo);
                }
                else {
                    if (sIcon != null && sIcon != "") {

                        var imgNotifInfo;
                        if (isPicto) {
                            imgNotifInfo = document.createElement("span");
                            imgNotifInfo.className = "notifInfoSpan " + sIcon;
                            setAttributeValue(imgNotifInfo, "style", "color:" + color + " !important;");

                        } else {
                            imgNotifInfo = document.createElement("img");
                            imgNotifInfo.className = "notifInfoImg";
                            imgNotifInfo.setAttribute("src", sIcon);
                        }

                        imgNotifInfo.setAttribute("alt", "Item " + sId);
                        imgNotifInfo.setAttribute("title", "Item " + sId);
                        divNotifInfo.appendChild(imgNotifInfo);
                    }
                }

                var divNotifInfoText = document.createElement("div");
                divNotifInfoText.className = "notifInfoText";
                divNotifInfoText.innerHTML = sDescription;
                divNotifInfo.appendChild(divNotifInfoText);

                if (bRead == false) {
                    var divNotifTagRead = document.createElement("div");
                    divNotifTagRead.className = "notifTagRead icon-check-circle";
                    divNotifTagRead.setAttribute("title", top._res_6877); //Marquer comme lu
                    divNotifTagRead.setAttribute("onclick", "notifTagRead(" + sId + ");");
                    divNotifInfo.appendChild(divNotifTagRead);
                }
                divNotifItem.appendChild(divNotifInfo);

                var divNotifUnsubscribe = document.createElement("div");
                divNotifUnsubscribe.className = "notifUnsubscribe icon-ellipsis-v";
                divNotifUnsubscribe.setAttribute("title", top._res_1883); // Se désabonner
                divNotifUnsubscribe.setAttribute("onclick", "notifUnsubscribe(" + sId + ");");
                //Demande #55402 - On masque ce menu pour le moment
                divNotifUnsubscribe.style.display = "none";
                divNotifInfo.appendChild(divNotifUnsubscribe);

                var divNotifTimeSpan = document.createElement("div");
                divNotifTimeSpan.className = "notifTimeSpan";
                divNotifTimeSpan.innerText = sTimespan;
                divNotifItem.appendChild(divNotifTimeSpan);

                divNotifContents.appendChild(divNotifItem);
            }
        }
        else {
            var divNotifNoItem = document.createElement("div");
            divNotifNoItem.className = "notifNoItem";

            var spanNotifNoItem = document.createElement("span");
            spanNotifNoItem.innerText = top._res_6880; //Aucune notification
            divNotifNoItem.appendChild(spanNotifNoItem);

            divNotifContents.appendChild(divNotifNoItem);
        }

        if (bRefreshNotifScrollTop == true) {
            bRefreshNotifScrollTop = false;

            var scrollHeight = divNotifContents.scrollHeight;
            var clientHeight = divNotifContents.clientHeight;
            divNotifContents.scrollTop = (scrollHeight - clientHeight) / 2;
        }
    }
    bNotifRefreshing = false;
}

function notifListHideGlobal(e) {
    if (!e)
        e = window.event;

    // Objet source
    var oSourceObj = e.target || e.srcElement;

    var continueLoop = true;
    do {
        if (oSourceObj != null) {
            if (!(oSourceObj.id != null && (oSourceObj.id == "notifListWrapper" || oSourceObj.id == "divNotifToggleContainer"))) {
                oSourceObj = oSourceObj.parentNode || oSourceObj.parentElement;
            }
            else {
                continueLoop = false;
            }
        }
        else {
            notifListToggle(e);
            continueLoop = false;
        }
    } while (continueLoop);
}

function notifListToggle(e) {

    try {
        if (!e)
            e = window.event;

        if (e && e.ctrlKey) {
            if (oNotifs) {

                oNotifs.StopRefresh = !oNotifs.StopRefresh;

                if (oNotifs.StopRefresh)
                    alert("les notifications sont désactivées");

                if (!oNotifs.StopRefresh) //relance les notifs
                    oNotifs.Refresh();

                return;
            }
        }
    }
    catch (ee) {

    }

    var divWrapper = document.getElementById("notifListWrapper");

    if (notifListIsOpen()) {
        notifListHide(divWrapper);
    }
    else {
        notifIndexLower = notifIndexLowerInit;
        notifIndexUpper = notifIndexUpperInit;

        //Demande #55402 - On affiche par defaut les notifications non lues
        //notifListRefresh();
        notifShowUnread();

        notifListShow(divWrapper);
    }
}

/**
 * Les actions a effectué lors du clic sur le changement de la checkbox source.
 * @param {any} source
 * @param {bool} bNoPopup
 */
function fnChgChckNwThm(source, bNoPopup = false) {
    //SHA : backlog #1 647
    var chk = document.querySelector("#chckNwThm");
    var spnNwItm = document.querySelector("#chckNwThm ~ span");
    var imgNwItm = document.querySelector("#chckNwThm ~ img");

    if (source && chk != null) {

        if (source.checked) {
            //imgNwItm = spnNwItm.replaceWith(($('<img/>').html($(this).html())));
            //var imgNwItm = document.querySelector("#chckNwThm ~ img");
            //imgNwItm.innerHTML = top._res_2378;
            imgNwItm.setAttribute("title", top._res_2378);
            imgNwItm.setAttribute("alt", top._res_2378);
            imgNwItm.setAttribute("class", "switch-new-theme-img");
            imgNwItm.setAttribute("src", "themes/default/images/logo-eudonetx.jpg");
            imgNwItm.style.visibility = 'visible';
            imgNwItm.style.display = 'block';
            spnNwItm.style.visibility = 'hidden';
            spnNwItm.style.display = 'none';
            document.querySelector('.switch-new-theme-wrap').classList.add('activated');

            if (bNoPopup) {
                var nNewThemeId = THEMES.ROUGE2019.Id;
                var oeParam = getParamWindow();
                oeParam.SetParam('currenttheme', JSON.stringify({ "Id": THEMES.ROUGE2019.Id, "Version": + THEMES.ROUGE2019.Version }));

                addCss("../../Theme2019/css/theme", "THEMEBASE");
                applyTheme(nNewThemeId, nGlobalCurrentUserid, function () { });
                eTools.switchBetaThemeCallback(nNewThemeId, true)
                return;
            }

            eTools.switchOnBetaTheme();
        }
        else {
            spnNwItm.innerHTML = top._res_2376;
            spnNwItm.setAttribute("title", top._res_2376);
            spnNwItm.style.visibility = 'visible';
            spnNwItm.style.display = 'block';
            imgNwItm.style.visibility = 'hidden';
            imgNwItm.style.display = 'none';
            document.querySelector('.switch-new-theme-wrap').classList.remove('activated');
            eTools.switchOfBetaTheme();
        }
    }
}

function notifListIsOpen() {
    var divWrapper = document.getElementById("notifListWrapper");
    if (divWrapper != null && divWrapper.className.indexOf("notifListShow") != -1) {
        return true;
    }
    return false;
}

function notifListHide(elem) {
    notifListShow(elem, false)
}

function notifListShow(elem, show) {
    if (show == null || show == undefined)
        show = true;

    if (show) {
        switchClass(elem, "notifListHide", "notifListShow");
        notifListMove(elem);
        switchClass(elem, "notifListAnimHide", "notifListAnimShow");
        setWindowEventListener('click', notifListHideGlobal);
    }
    else {
        switchClass(elem, "notifListAnimShow", "notifListAnimHide");
        setTimeout(function () { switchClass(elem, "notifListShow", "notifListHide"); }, 350);
        removeWindowEventListener('click', notifListHideGlobal);
    }
}

function notifListMove(divWrapper) {
    var divWrapperPos = getAbsolutePositionWithScroll(divWrapper);

    var notifCounter = document.getElementById("spanNotifToggle");
    var notifCounterPos = getAbsolutePositionWithScroll(notifCounter);

    var divArrow1 = document.getElementById("notifArrow1");
    var divArrow2 = document.getElementById("notifArrow2");
    var notifArrow1Pos = getAbsolutePositionWithScroll(divArrow1);
    var notifArrow2Pos = getAbsolutePositionWithScroll(divArrow2);

    divWrapper.style.top = (notifCounterPos.y + notifCounterPos.h + notifArrow1Pos.h + 2) + 'px';
    //divWrapper.style.left = (notifCounterPos.x - (divWrapperPos.w * 3 / 4)) + 'px';
    var divWrapperRightMargin = 15;
    divWrapper.style.right = divWrapperRightMargin + 'px';

    var divWrapperPos = getAbsolutePositionWithScroll(divWrapper);

    //var divArrowLeft = (divWrapperPos.w * 3 / 4) - (notifArrow1Pos.w / 2) + (notifCounterPos.w / 2);
    var divArrowLeft = notifCounterPos.x + (notifCounterPos.w / 2) - divWrapperPos.x - (notifArrow1Pos.w / 2);
    divArrow1.style.left = divArrowLeft + 'px';
    divArrow2.style.left = divArrowLeft + 'px';
}

function notifTagAllRead() {
    var oTagReadUpdater = new eUpdater("mgr/eNotificationManager.ashx", 0);

    oTagReadUpdater.addParam("action", "tagAllRead", "post");
    oTagReadUpdater.addParam("post", "1", "post");

    oTagReadUpdater.send(notifTagReadProcessReturn);

    notifListRefresh();
}

function notifTagRead(notifId) {
    var oTagReadUpdater = new eUpdater("mgr/eNotificationManager.ashx", 0);

    oTagReadUpdater.addParam("action", "tagRead", "post");
    oTagReadUpdater.addParam("post", "1", "post");
    oTagReadUpdater.addParam("notifid", notifId.toString(), "post");

    oTagReadUpdater.send(notifTagReadProcessReturn);

    notifListRefresh();
}

function notifTagReadProcessReturn(oRes) {
    if (!oRes) {
        return;
    }

    var success = getXmlTextNode(oRes.getElementsByTagName("success")[0]);
    var error = getXmlTextNode(oRes.getElementsByTagName("error")[0]);

    //Gestion d'erreur
    //TODO
}

function notifUnsubscribe(notifId) {
    var oTagReadUpdater = new eUpdater("mgr/eNotificationManager.ashx", 0);

    oTagReadUpdater.addParam("action", "unsubscribe", "post");
    oTagReadUpdater.addParam("post", "1", "post");
    oTagReadUpdater.addParam("notifid", notifId.toString(), "post");

    oTagReadUpdater.send(notifUnsubscribeProcessReturn);
}

function notifUnsubscribeProcessReturn(oRes) {
    if (!oRes) {
        return;
    }

    var success = getXmlTextNode(oRes.getElementsByTagName("success")[0]);
    var error = getXmlTextNode(oRes.getElementsByTagName("error")[0]);

    //Gestion d'erreur
    //TODO
}

var notifScrollTop = 0;
var bRefreshNotifScrollTop = false;

function notifScroll() {

    var divContents = document.getElementById("notifContents");

    if (divContents != null) {
        var scrollTop = divContents.scrollTop;
        var scrollHeight = divContents.scrollHeight;
        var clientHeight = divContents.clientHeight;
        var scrollBottom = scrollHeight - (scrollTop + clientHeight);

        /*
        var spanScroll = document.getElementById("notifScrollVal");
        if (spanScroll != null) {
            spanScroll.innerText = "Top: " + scrollTop.toString() + " Bottom: " + scrollBottom.toString();
        }
        */

        if (scrollBottom <= 0) {
            if (bNotifRefreshing != true && notifNextFetchCount > 0) {
                notifScrollTop = scrollTop;
                bRefreshNotifScrollTop = true;

                //notifNbRows += 10;
                if (notifIndexLower == 1 && notifIndexUpper == 10) {
                    notifIndexUpper += 10;

                    notifListRefresh();
                }
                else {
                    notifIndexLower += 10;
                    notifIndexUpper += 10;

                    notifListRefresh();
                }
            }
        }
        else if (scrollTop <= 0) {
            if (bNotifRefreshing != true) {
                notifScrollTop = scrollTop;
                bRefreshNotifScrollTop = true;

                //notifNbRows += 10;
                if (notifIndexLower == 1 && notifIndexUpper == 10) {
                    //do nothing
                }
                else if (notifIndexLower == 1 && notifIndexUpper == 20) {
                    notifIndexUpper -= 10;

                    notifListRefresh();
                }
                else {
                    notifIndexLower -= 10;
                    notifIndexUpper -= 10;

                    notifListRefresh();
                }
            }
        }
    }
}

/***** Fonctions Compteur Notif non lues *****/
function notifRefreshCountUnread(sCountUnread) {
    var counter = document.getElementById("spanNotifCounter");
    if (counter != null) {
        var nCountUnread = 0;
        if (sCountUnread != "") {
            nCountUnread = parseInt(sCountUnread);
        }

        if (nCountUnread != null && nCountUnread != 0) {
            counter.innerText = nCountUnread.toString();
            notifCountUnreadShow(counter);
        }
        else {
            notifCountUnreadHide(counter);
            counter.innerText = "0";
        }
    }

    if (notifListIsOpen()) {
        var divWrapper = document.getElementById("notifListWrapper");
        notifListMove(divWrapper);
    }
}

function notifCountUnreadHide(elem) {
    notifCountUnreadShow(elem, false);
}

function notifCountUnreadShow(elem, show) {
    if (show == null || show == undefined)
        show = true;

    if (show) {
        switchClass(elem, "notifCounterHide", "notifCounterShow");
    }
    else {
        switchClass(elem, "notifCounterShow", "notifCounterHide");
    }
}

/*Fin - NOTIFICATIONS*******************************************************/


/**************************************************************************/
/*DRAG AND DROP*/
/**************************************************************************/
//Drag AND Drop Actif sur le navigateur
var bDragFile = (typeof (FormData) != "undefined");
var modalProgress;

function SendFile(mesFichiers, filesInfos) {
    try {
        if (!bDragFile) //Drag and drop non compatible
            return;
        var formData = new FormData();
        var nTabId = -1;
        var nFileId = -1;
        if (getCurrentView(document) == "LIST") {
            //Ajout depuis pop up liste de PJ
            //Bug #36255, la liste des pj ne se rafraichit pas après le dragNdrop      
            nFileId = document.getElementById("nFileID").value;
            nTabId = document.getElementById("nTab").value;
            try {
                formData.append("fromtpl", document.getElementById("fromtpl").value);
                formData.append("viewtype", document.getElementById("viewtype").value);
                formData.append("parentEvtFileId", document.getElementById("parentEvtFileId").value);
                formData.append("parentEvtTabId", document.getElementById("parentEvtTabId").value);
                formData.append("ppid", document.getElementById("ppid").value);
                formData.append("pmid", document.getElementById("pmid").value);
            }
            catch (e) {

            }
        }
        else {
            //Ajout depuis signet de pj
            nFileId = top.GetCurrentFileId(nGlobalActiveTab);
            nTabId = nGlobalActiveTab;
        }

        if (nFileId < 0 || !nTabId || nTabId <= 0 || mesFichiers.length <= 0)  //tabid ne peut pas être vide ou <= à 0 et dileid n peut être < à 0
            return;
        var oIdsPj = document.getElementById("idspj");

        formData.append('nFileId', nFileId);
        formData.append('nTab', nTabId);
        if (filesInfos) {
            var filesNameList = "";
            for (var i = 0; i < filesInfos.length; i++) {
                if (filesNameList)
                    filesNameList += '|';
                filesNameList += filesInfos[i].filename + ":" + filesInfos[i].saveas;
            }

            formData.append("SaveAs", filesNameList);
            formData.append("UploadInfo", JSON.stringify(filesInfos));

        }

        if (filesNameList)
            if (oIdsPj && oIdsPj.value)
                formData.append('iDsOfPj', oIdsPj.value);

        for (var i = 0; i < mesFichiers.length; i++) {
            formData.append('file_' + i, mesFichiers[i]);
        };

        var modal = createProgress();
        var xhr = new XMLHttpRequest();

        xhr.open('POST', 'ePjAddFromTpl.aspx');

        xhr.onreadystatechange =
            (
                function (updter) {
                    if (xhr.readyState == 4) {
                        var objReturn;
                        var oErrorObj;
                        var okErrorFct = (function (oParam) {
                            return function () {
                                try {
                                    setWait(false);
                                }
                                catch (ex) {
                                    that.trace("setWait non utilisé : " + ex.message);
                                }
                                modal.hide();
                            };
                        })(objReturn);
                        if (xhr.status == 200) {

                            /********************/


                            //
                            var bError = xhr.getResponseHeader("X-EDN-ERRCODE") == "1";
                            var bSessionLost = xhr.getResponseHeader("X-EDN-SESSION") == "1";

                            objReturn = xhr.responseText;

                            //Gestion d'erreur
                            if (bError || bSessionLost) {
                                // si type de retour txt, transformation du flux txt en xml
                                oErrorObj = errorXML2Obj(createXmlDoc(objReturn), bSessionLost);
                                if (bSessionLost) {
                                    //Message d'avertissement - Valeur "par défaut"                         
                                    oErrorObj.Type = (oErrorObj.Type == "") ? "1" : oErrorObj.Type;
                                    oErrorObj.Title = (oErrorObj.Title == "") ? top._res_503 : oErrorObj.Title; // votre session a expiré...
                                    oErrorObj.Msg = (oErrorObj.Msg == "") ? top._res_6068 : oErrorObj.Msg; // votre session a expiré...détail
                                    oErrorObj.DetailMsg = oErrorObj.DetailMsg;
                                    oErrorObj.DetailDev = oErrorObj.DetailDev;

                                    // remplace le callback intial par un retour à l'accueil
                                    internalErrorCallBack = function () {
                                        top.document.location = "elogin.aspx";
                                    }
                                }
                            }

                            //Libération des ressources
                            delete xhr;
                            xhr = null;

                            if (bError || bSessionLost) {
                                //  Affichage de l'erreur
                                eAlertError(oErrorObj, okErrorFct);
                                return;

                            }

                            /********************/
                            //Transfert terminé
                            objReturn = objReturn;

                            //BSE #51 108 rafraîchir la liste des PJ dans le wizard pour activer la sélection 
                            //KHA régression 53 607 A la création d'une fiche PM, si j'ajoute une annexe en faisant un drag et drop, les annexes ne sont pas ajoutées (testé sur IE11).
                            //RefreshLstPj();
                            top.eAlert(4, top._res_6710, top._res_6710);
                            modal.hide();
                            if (getCurrentView(document) == "LIST") {
                                //Ajout depuis pop up liste de PJ
                                //maj compteur
                                var olstPJ = document.getElementById("divlstPJ");
                                olstPJ.innerHTML = objReturn;
                                initFldClick(TAB_PJ);
                                var osublstPJ = document.getElementById("subdivlstPJ");
                                var nbPj = getAttributeValue(osublstPJ, "nbpj");
                                var onbpj = document.getElementById("nbpj");
                                onbpj.value = nbPj;
                                var sidspj = getAttributeValue(osublstPJ, "idspj");
                                var oidspj = document.getElementById("idspj");
                                oidspj.value = sidspj;

                            }
                            else {
                                //Ajout depuis signet de pj
                                var nActiveBkm = getActiveBkm(nGlobalActiveTab);
                                if (nActiveBkm > 0 && !isPopup())
                                    loadBkmList(nActiveBkm);
                            }
                        }
                        else {

                            // Gestion des erreurs status != 200
                            bError = true;
                            oErrorObj = new Object();
                            oErrorObj.Type = "1";
                            oErrorObj.Title = top._res_416; // Erreur
                            oErrorObj.Msg = top._res_72; // Une erreur est survenue
                            oErrorObj.DetailMsg = top._res_544; // cette erreur a été transmise à notre équipe technique.
                            oErrorObj.DetailDev = "Code erreur " + xhr.status + " dans eUpdater.onreadystatechange\n" + xhr.responseText;

                            objReturn = "";

                            that.trace(oErrorObj.DetailDev);
                            eAlertError(oErrorObj, okErrorFct);

                        }
                    }
                }
            );

        xhr.upload.onprogress = function (event) {
            if (event.lengthComputable) {
                var currentState = (event.loaded / event.total * 100 | 0);
                progess(modal, currentState);
            }
        }
        xhr.send(formData);
    }
    catch (ex) {
        alert(ex);
    }
}

function sendImage(element, file, callBackFunction) {

    // Paramètres

    var type, tab, field, fileid;

    if (element.className == "hAvatar" || element.className == "hAvatarFromMyEudonet" || element.id == "DivCurrentPicture" || element.id == "vcImg" || getAttributeValue(element, "eavatar") == "1") {
        type = "AVATAR";
    } else if (element.className == "widgetImg") {
        type = "IMAGE_WIDGET";
    }
    else {
        if (getAttributeValue(element, "eaction") == "LNKOPENUSERAVATAR")
            type = "USER_AVATAR_FIELD";
        else
            type = "IMAGE_FIELD";
    }

    if (element.tagName == "TD") {
        var id = element.id;
        var arrID = id.split("_");
        tab = arrID[1];
        field = arrID[2];
        fileid = arrID[3];
    }
    else if (element.id == "vcImg" || element.id == "DivCurrentPicture") {
        tab = getAttributeValue(element, "tab");
        field = getAttributeValue(element, "did");
        fileid = getAttributeValue(element, "fid");
    }
    else if (element.id == "hAvatar" || element.id == "hAvatarFromMyEudonet") {

        tab = getAttributeValue(element, "tab");
        field = "101075";
        fileid = getAttributeValue(element, "fid");
    }
    else if (type == "IMAGE_WIDGET") {
        var parts = element.id.split("_");
        tab = parts[1] * 1;
        field = parts[2] * 1;
        fileid = parts[3] * 1;
    }

    // Request

    var modalP = createProgress();
    var xhr = new XMLHttpRequest();
    var formData = new FormData();

    formData.append("filMyFile", file);
    formData.append("fromDragAndDrop", "1");
    formData.append("action", "UPLOAD");
    formData.append("imageType", type);
    formData.append("fileId", fileid);
    formData.append("fieldDescId", field);
    formData.append("computeRealThumbnail", "0");
    formData.append("imageWidth", "0");
    formData.append("imageHeight", "0");
    formData.append("parentIsPopup", isPopup() ? "1" : "0");
    formData.append("updateOnBlur", isUpdateOnBlur() ? "1" : "0");

    xhr.open('POST', 'Mgr/eImageManager.ashx');

    xhr.onreadystatechange =
        (
            function (updter) {
                if (xhr.readyState == 4) {
                    var objReturn;
                    var xmlDoc;
                    var oErrorObj;
                    var okErrorFct = (function (oParam) {
                        return function () {
                            try {
                                setWait(false);
                            }
                            catch (ex) {
                                that.trace("setWait non utilisé : " + ex.message);
                            }
                            modalP.hide();
                        };
                    })(objReturn);

                    if (xhr.status == 200) {

                        var bError = xhr.getResponseHeader("X-EDN-ERRCODE") == "1";
                        var bSessionLost = xhr.getResponseHeader("X-EDN-SESSION") == "1";

                        objReturn = xhr.responseText;

                        //Gestion d'erreur
                        if (bError || bSessionLost) {
                            // si type de retour txt, transformation du flux txt en xml
                            oErrorObj = errorXML2Obj(createXmlDoc(objReturn), bSessionLost);
                            if (bSessionLost) {
                                //Message d'avertissement - Valeur "par défaut"                         
                                oErrorObj.Type = (oErrorObj.Type == "") ? "1" : oErrorObj.Type;
                                oErrorObj.Title = (oErrorObj.Title == "") ? top._res_503 : oErrorObj.Title; // votre session a expiré...
                                oErrorObj.Msg = (oErrorObj.Msg == "") ? top._res_6068 : oErrorObj.Msg; // votre session a expiré...détail
                                oErrorObj.DetailMsg = oErrorObj.DetailMsg;
                                oErrorObj.DetailDev = oErrorObj.DetailDev;

                                // remplace le callback intial par un retour à l'accueil
                                internalErrorCallBack = function () {
                                    top.document.location = "elogin.aspx";
                                }
                            }
                        }

                        //Libération des ressources
                        delete xhr;
                        xhr = null;

                        if (bError || bSessionLost) {
                            //  Affichage de l'erreur
                            eAlertError(oErrorObj, okErrorFct);
                            return;

                        }

                        /********************/

                        // Transfert terminé
                        //objReturn = objReturn;

                        xmlDoc = createXmlDoc(objReturn);
                        oErrorObj = errorXML2Obj(xmlDoc, false);
                        if (oErrorObj.Success == "0") {
                            eAlertError(oErrorObj, okErrorFct);
                        }
                        else {
                            top.eAlert(4, top._res_6710, top._res_6710);

                            var img;
                            var imgURL = getXmlTextNode(xmlDoc.getElementsByTagName("imageURL")[0]);
                            var imgName = getXmlTextNode(xmlDoc.getElementsByTagName("imageName")[0]);
                            var storedInSession = getXmlTextNode(xmlDoc.getElementsByTagName("storedInSession")[0]) == "1" ? true : false;

                            modalP.hide();

                            // Refresh
                            if (element.id == "vcImg") {
                                // Avatar PP/PM
                                element.children[0].src = imgURL;
                            }
                            else if (element.id == "DivCurrentPicture") {

                                if (tab == "101000") {
                                    // User avatar
                                    img = top.document.getElementById("UserAvatar");
                                    if (img && getAttributeValue(img, "userid") == fileid) {
                                        img.src = imgURL.replace("_thumb", "");
                                    }
                                }
                                else {
                                    // File avatar
                                    img = top.document.getElementById("vcImg");
                                    img.style.backgroundImage = "url('" + imgURL + "')";
                                }
                                top.modalImage.hide();
                            }
                            else if (type == "IMAGE_WIDGET") {
                                img = top.document.querySelector("#widget-wrapper-" + fileid + " .xrm-widget-content img");
                                if (img) {
                                    img.src = imgURL + "?" + new Date().getTime(); //Hack pour refresh image sans changement de src
                                    img.width = getXmlTextNode(xmlDoc.getElementsByTagName("imageWidth")[0]);
                                    img.height = getXmlTextNode(xmlDoc.getElementsByTagName("imageHeight")[0]);
                                    setAttributeValue(img, "dbv", imgName);
                                }

                                callBackFunction();
                            }
                            else {

                                if (tab == "101000" && element.getAttribute("eaction") != "LNKOPENUSERAVATAR") {

                                    if (element.className == "hAvatarFromMyEudonet" && isElementFirstChildEmptyPictureArea(element)) {
                                        element.removeChild(element.firstChild);
                                        img = document.createElement("img");
                                        element.append(img);
                                    }
                                    else {
                                        img = element.querySelector("img");
                                    }
                                    if (img == null) {
                                        var varoDiv = element.querySelector("div[data-eemptypicturearea]");
                                        element.removeChild(varoDiv);

                                        img = document.createElement("img");
                                        img.setAttribute("fid", fileid);
                                        img.setAttribute("tab", tab);
                                        img.style = "border-width:0px;max-height:100%;max-width:100%;";
                                        element.appendChild(img);

                                    }

                                    img.src = imgURL.replace("_thumb", "") + '?t=' + new Date().getTime();;
                                    img.style = "border-width:0px;max-height:100%;max-width:100%;"
                                    setAttributeValue(element, "dbv", imgName);

                                    var oAv = top.document.getElementById("UserAvatar");
                                    if (oAv) {
                                        var nCurrentUser = getAttributeValue(oAv, "fid");
                                        var nFileId = getAttributeValue(element, "fid");
                                        if (fileid == nCurrentUser && fileid != "") {
                                            oAv.src = imgURL + '?t=' + new Date().getTime();
                                        }

                                    }
                                }
                                else {
                                    if (
                                        (element.tagName == "TD" && element.hasAttribute("eaction")
                                            && (element.getAttribute("eaction") == "LNKCATIMG" || element.getAttribute("eaction") == "LNKOPENIMG" || element.getAttribute("eaction") == "LNKOPENUSERAVATAR" || element.getAttribute("eaction") == "LNKOPENAVATAR")
                                            && isElementFirstChildEmptyPictureArea(element))

                                    ) {
                                        element.removeChild(element.firstChild);

                                        img = document.createElement("img");
                                        img.style.maxHeight = "100%";
                                        img.style.maxWidth = "100%";

                                        element.appendChild(img);
                                        element.style.textAlign = "center";
                                    }
                                    else {
                                        img = element.querySelector("img");
                                    }
                                    img.src = imgURL + '?t=' + new Date().getTime();
                                    setAttributeValue(element, "dbv", imgName);
                                    if (storedInSession)
                                        setAttributeValue(img, "session", "1");
                                }
                            }

                        }


                    }
                    else {

                        // Gestion des erreurs status != 200
                        bError = true;
                        oErrorObj = new Object();
                        oErrorObj.Type = "1";
                        oErrorObj.Title = top._res_416; // Erreur
                        oErrorObj.Msg = top._res_72; // Une erreur est survenue
                        oErrorObj.DetailMsg = top._res_544; // cette erreur a été transmise à notre équipe technique.
                        oErrorObj.DetailDev = "Code erreur " + xhr.status + " dans eUpdater.onreadystatechange\n" + xhr.responseText;

                        objReturn = "";

                        that.trace(oErrorObj.DetailDev);
                        eAlertError(oErrorObj, okErrorFct);

                    }
                }
            }
        );

    xhr.upload.onprogress = function (event) {
        if (event.lengthComputable) {
            var currentState = (event.loaded / event.total * 100 | 0);
            progess(modalP, currentState);
        }
    }
    xhr.send(formData);
}

function findFieldImgElement(td) {
    var img = null;
    if (isElementFirstChildEmptyPictureArea(td)) {
        td.removeChild(td.firstChild);
        img = document.createElement("img");
        td.append(img);
    }
    else {
        img = td.querySelector("img");
    }
    return img;
}
//function imageDragEnter(event) {
//    var element = event.target;
//}

function containsFiles(event) {

    if (event && event.dataTransfer && event.dataTransfer.dropEffect != "copy") {
        event.dataTransfer.dropEffect = "copy";

    }

    if (event.dataTransfer.types) {
        for (var i = 0; i < event.dataTransfer.types.length; i++) {
            if (event.dataTransfer.types[i] == "Files") {
                return true;
            }
        }
    }
    return false;
}

function isElementFirstChildEmptyPictureArea(element) {
    return element.firstChild && element.firstChild.tagName == "DIV" && element.firstChild.hasAttribute("data-eEmptyPictureArea") && element.firstChild.getAttribute("data-eEmptyPictureArea") == "1";
}

function UpFilDragOver(oCall, e, nType) {

    if (e && e.dataTransfer && e.dataTransfer.dropEffect != "copy") {
        e.dataTransfer.dropEffect = "copy";

    }

    if (typeof nType == "undefined")
        nType = 0;




    if (!e)
        return;
    if (!containsFiles(e))
        return;

    if (!bDragFile || getAttributeValue(oCall, "readonly") == "readonly")
        return;

    if (isElementFirstChildEmptyPictureArea(oCall))
        addClass(oCall.firstChild, 'PjHover');
    else
        addClass(oCall, 'PjHover');
}

function UpFilDragLeave(oCall) {
    if (!bDragFile || getAttributeValue(oCall, "readonly") == "readonly")
        return;

    if (isElementFirstChildEmptyPictureArea(oCall))
        removeClass(oCall.firstChild, 'PjHover');
    else
        removeClass(oCall, 'PjHover');
}

function UpFilDrop(oCall, e, filesList, oFilesInfo, nType) {

    if (e && e.dataTransfer && e.dataTransfer.dropEffect != "copy") {
        e.dataTransfer.dropEffect = "copy";

    }

    if (typeof nType == "undefined")
        nType = 0;

    if (!bDragFile || getAttributeValue(oCall, "readonly") == "readonly")
        return;

    if (isElementFirstChildEmptyPictureArea(oCall))
        removeClass(oCall.firstChild, 'PjHover');
    else
        removeClass(oCall, 'PjHover');

    var mesfichiers;
    if (e && e.dataTransfer && e.dataTransfer.files)
        mesfichiers = e.dataTransfer.files;

    if (filesList)
        mesfichiers = filesList;

    if (!oFilesInfo)
        oFilesInfo = eTools.GetFilesInfos(mesfichiers);

    //Image
    if (nType == 1) {
        if (mesfichiers.length > 0) {
            sendImage(oCall, mesfichiers[0]);
        }
    }
    else if (nType == 2) {
        //Mails non remis
        if (mesfichiers.length < 1)
            return;

        var badExt = Array.prototype.slice.call(mesfichiers).some(function (mfile) {
            var sName = mfile.name.toLowerCase();
            var sL = sName.length

            return sName.substring(sL - "eml".length, sL) !== "eml" && sName.substring(sL - "msg".length, sL) !== "msg";
        });

        if (badExt) {
            eAlert(0, top._res_8612, mesfichiers.length > 1 ? top._res_2911 : top._res_2910, null, 600); // Pour accéder à cette fiche, vous devez préalablement choisir d’afficher l’onglet dans votre barre d’onglets.
            return;
        }
        else {

            bounceMailSender(oFilesInfo, mesfichiers)
        }
    }
    else {
        var sendFileFct = function (oFilesInfo) { SendFile(mesfichiers, oFilesInfo); };
        var validFct = function (oFilesInfo) { UpFilDrop(oCall, e, mesfichiers, oFilesInfo); };
        // onSuccessFct : sendFileFct, onValidNewNameFct: validFct
        callCheckPjExists(oFilesInfo, sendFileFct, validFct);
    }
}


function bounceMailSender(filesInfo, filesToTransfert) {

    top.setWait(true)

    var nFileId = top.GetCurrentFileId(nGlobalActiveTab);
    var nTabId = nGlobalActiveTab;

    var formData = new FormData();
    formData.append("FileId", nFileId);
    formData.append("Tab", nTabId);

    

    Array.from(filesToTransfert).forEach(function (fic, idx) {
        formData.append('fileCollection[' + idx + ']', fic, fic.saveas)
    }
    );

    var modal = createProgress();
    var xhr = new XMLHttpRequest();
    xhr.open('POST', 'mgr/eUploadFilesManager.ashx');

    xhr.onreadystatechange =
        (
            function (updter) {
                if (xhr.readyState == 4) {
                    var objReturn;
                    var oErrorObj;
                    var okErrorFct = (function (oParam) {
                        return function () {
                            try {
                                setWait(false);
                            }
                            catch (ex) {
                                that.trace("setWait non utilisé : " + ex.message);
                            }
                            modal.hide();
                        };
                    })(objReturn);
                    //if (xhr.readyState == 4) {
                    //    top.setWait(false);
                    //    modal.hide();
                    if (xhr.status == 200) {
                        var bError = getXmlTextNode(xhr.responseXML.getElementsByTagName("success")[0]) == "0";
                        var bEdnError = xhr.getResponseHeader("X-EDN-ERRCODE") == "1";
                        var bSessionLost = xhr.getResponseHeader("X-EDN-SESSION") == "1";
                        objReturn = xhr.responseText;
                        //Gestion d'erreur
                        if (bError || bEdnError || bSessionLost) {
                            // si type de retour txt, transformation du flux txt en xml
                            oErrorObj = errorXML2Obj(createXmlDoc(objReturn), bSessionLost);
                            if (bSessionLost) {
                                //Message d'avertissement - Valeur "par défaut"                         
                                oErrorObj.Type = (oErrorObj.Type == "") ? "1" : oErrorObj.Type;
                                oErrorObj.Title = (oErrorObj.Title == "") ? top._res_503 : oErrorObj.Title; // votre session a expiré...
                                oErrorObj.Msg = (oErrorObj.Msg == "") ? top._res_6068 : oErrorObj.Msg; // votre session a expiré...détail
                                oErrorObj.DetailMsg = oErrorObj.DetailMsg;
                                oErrorObj.DetailDev = oErrorObj.DetailDev;

                                // remplace le callback intial par un retour à l'accueil
                                internalErrorCallBack = function () {
                                    top.document.location = "elogin.aspx";
                                }
                            }

                            //Libération des ressources
                            delete xhr;
                            xhr = null;

                            if (bError || bEdnError || bSessionLost) {
                                //  Affichage de l'erreur
                                //var vMessage = getXmlTextNode(xhr.responseXML.getElementsByTagName("msg")[0]);
                                //top.eAlert(1, top._res_8538, vMessage);
                                eAlertError(oErrorObj, okErrorFct);
                                return;
                            }

                            //Transfert terminé
                            objReturn = objReturn;
                        }
                        top.eAlert(4, top._res_6710, top._res_6710);
                        modal.hide();

                        //On recharge la liste d'emails non remis même en cas d'erreur. 
                        var nActiveBkm = getActiveBkm(nGlobalActiveTab);
                        if (nActiveBkm > 0 && !isPopup())
                            loadBkmList(nActiveBkm);
                    }
                    else {
                        // Gestion des erreurs status != 200
                        bError = true;
                        oErrorObj = new Object();
                        oErrorObj.Type = "1";
                        oErrorObj.Title = top._res_416; // Erreur
                        oErrorObj.Msg = top._res_72; // Une erreur est survenue
                        oErrorObj.DetailMsg = top._res_544; // cette erreur a été transmise à notre équipe technique.
                        oErrorObj.DetailDev = "Code erreur " + xhr.status + " dans eUpdater.onreadystatechange\n" + xhr.responseText;
                        objReturn = "";
                        that.trace(oErrorObj.DetailDev);
                        eAlertError(oErrorObj, okErrorFct);
                    }
                }
            }
        );

    xhr.upload.onprogress = function (event) {
        if (event.lengthComputable) {
            var currentState = (event.loaded / event.total * 100 | 0);
            progess(modal, currentState);
        }
    }
    xhr.send(formData);
}

function createProgress() {
    modalProgress = null;
    prog = 0;
    modalProgress = new eModalDialog(top._res_6545, 4, top._res_6546, 550, 160, "modalProgress");
    modalProgress.noButtons = true;
    modalProgress.show();
    return modalProgress;
}

var prog = 0;
function progess(modal, prog) {
    if (prog > 100)
        return;
    modal.updateProgressBar(prog);
}

function checkDomElemVisible(elm) {

    if (elm && elm.getBoundingClientRect) {

        var rect = elm.getBoundingClientRect();
        var viewHeight = Math.max(document.documentElement.clientHeight, window.innerHeight);
        return !(rect.bottom < 0 || rect.top - viewHeight >= 0);
    }
    return true;
}

function resizeWidgetContent(widgetWrapper, maxW, maxH) {

    if (!widgetWrapper) {
        return;
    }

    var widgetContentDiv = widgetWrapper.querySelector(".xrm-widget-content");
    if (!widgetContentDiv)
        return;
    if (widgetContentDiv.children.length == 0)
        return;

    var element;

    var margin = 5;

    var widgetType = getAttributeValue(widgetWrapper, "t");

    if (!maxW || !maxH) {
        maxW = widgetContentDiv.offsetWidth;
        maxH = widgetContentDiv.offsetHeight;
    }

    if (widgetType == XrmWidgetType.Image) {

        element = widgetContentDiv.children[0];


        // On utilise la taille d'origine, mieux conservée 
        var sw = getAttributeValue(element, "w");
        var sh = getAttributeValue(element, "h");

        var ratioW = maxW / sw;
        var ratioH = maxH / sh;

        var ratio = Math.min(ratioW, ratioH);
        element.width = sw * ratio;
        element.height = sh * ratio;

    }
    else if (widgetType == XrmWidgetType.Chart) {


        element = widgetContentDiv.children[0];
        element.style.width = (maxW) + "px";
        element.style.height = (maxH) + "px";
        //element = widgetContentDiv.children[0];
        //var arr = element.id.split("_");
        // if (typeof (loadSyncFusionChart) == "function" && arr[1] != "0")
        //    redrawChart(arr[1]);

    }
    else if (widgetType == XrmWidgetType.List) {

        element = widgetContentDiv.children[0];

        element.style.width = (maxW - 10) + "px";
        element.style.height = widgetContentDiv.style.height;//  (maxH - 10) + "px";

        var divList = widgetContentDiv.querySelector(".divmTab");
        if (divList && widgetContentDiv.style.height != "") {
            divList.style.width = (maxW - 9) + "px";
            divList.style.height = (parseInt(widgetContentDiv.style.height) - 60) + 'px';
        }
    }
    //else {
    //    element.style.width = maxW - margin;
    //    element.style.height = maxH - margin;
    //}
}

function commitWidgetContent(widgetWrapper, maxW, maxH) {



    var widgetContentDiv = widgetWrapper.querySelector(".xrm-widget-content");
    if (!widgetContentDiv)
        return;
    if (widgetContentDiv.children.length == 0)
        return;

    var widgetType = getAttributeValue(widgetWrapper, "t");
    if (widgetType == XrmWidgetType.Chart) {

        //if (maxW < 300 || maxW < 300)        
        //    return;


        var element = widgetContentDiv.children[0];
        element.style.width = (maxW) + "px";
        element.style.height = (maxH) + "px";
        element = widgetContentDiv.children[0];
        var arr = element.id.split("_");
        if (typeof (redrawChart) == "function" && arr[1] != "0")
            redrawChart(arr[1]);
    }
    else if (widgetType == XrmWidgetType.Image) {

        var element = widgetContentDiv.children[0];

        // On utilise la taille d'origine, mieux conservée 
        var sw = getAttributeValue(element, "w");
        var sh = getAttributeValue(element, "h");

        var ratioW = maxW / sw;
        var ratioH = maxH / sh;

        var ratio = Math.min(ratioW, ratioH);
        element.width = sw * ratio;
        element.height = sh * ratio;

    }
}

function openUrlInNewTab(strUrl) {
    if (strUrl != null && strUrl != "") {
        var strUrlLower = strUrl.toLowerCase();

        if (strUrlLower.indexOf("mailto:") == 0) {
            window.location.href = strUrl;
        }
        else {
            if (
                strUrlLower.indexOf("http://") != 0 &&
                strUrlLower.indexOf("https://") != 0 &&
                strUrlLower.indexOf("ftp://") != 0 &&
                strUrlLower.indexOf("ftps://") != 0 &&
                strUrlLower.indexOf("sftp://") != 0
            )
                strUrl = "http://" + strUrl;
            window.open(strUrl);
        }
    }
}

function getSocialNetworkUrl(url, rootUrl) {
    var finalUrl = url;

    if (rootUrl != null && rootUrl != "") {
        finalUrl = rootUrl + finalUrl;
    }

    return finalUrl;
}

function sendMailOrSMSFromActionBar(oSourceObj, sMailOrSMS, mailType) {
    var sourceFiles = getParamWindow().document.getElementById("MLFiles");
    if (mailType == TypeMailing.SMS_MAILING_FROM_BKM)
        sourceFiles = getParamWindow().document.getElementById("SMSFiles");
    sMailOrSMS = sMailOrSMS.trim();
    selectFileMail(sourceFiles, sMailOrSMS, oSourceObj, mailType);
}


/******************************************* VCARD/MINI-FICHE *******************************************/

var showVcardTimer;
//Modal de mcard commune
var vCardModal;
//Dernier objet appelant qui a ouvert la VCARD
var oVCardCaller;
//Indique si l'on a ouvert la dernière VCARD sur un onclick
var vcFromClick = false;

/*
Affichage ou non d'une VCARD
obj : objet appelant qui a ouvert la VCARD
nOn : 1 pour Ouvrir ou 0 pour fermer la VCARD
nFileId : id de la fiche PP
parentObj : 
bFromClick : Indique si l'on ouvre la VCARD depuis l'evennement un onclick
*/
function shvc(obj, nOn, nFileId, parentObj, bFromClick) {



    if (nOn == 1) {
        // #76 232 et US #1 078 (Tâche #1 921) - Depuis le correctif #58 259 qui passe obj à null, il faut impérativement le vérifier avant d'agir dessus
        // obj étant utilisé pour définir la position de la vCard et le DescId lié, inutile de continuer dans le can nOn == 1 s'il n'est pas défini
        if (!obj)
            return;

        if (bFromClick)
            vcFromClick = true;

        // #76 232 et US #1 078 (Tâche #1 859) - Masquage de l'infobulle système après sauvegarde
        if (obj && obj.getAttribute("title_backup") == null) {
            setAttributeValue(obj, "title_backup", obj.title);
            obj.title = ''; // masquage
        }

        oVCardCaller = obj;

        //

        //Annule te timer de fade-out/fade-in
        clearTimeout(showVcardTimer);
        //LNKID dans le cas d'un champ principal 01 (fausse liaison) mais DBV dans le cas d'un champ de liaison vers une autre table
        if (typeof (nFileId) == 'undefined' || nFileId <= 0 || isNaN(nFileId))
            nFileId = obj.getAttribute("lnkid");
        if (typeof (nFileId) == 'undefined' || nFileId <= 0 || isNaN(nFileId))
            nFileId = obj.getAttribute("dbv");
        if (typeof (nFileId) == 'undefined' || nFileId <= 0 || isNaN(nFileId))
            nFileId = obj.getAttribute("id");

        if (typeof (nFileId) == 'undefined' || nFileId <= 0 || isNaN(nFileId))
            return;

        var tab = obj.getAttribute("vcMiniFileTab")
        if (typeof (tab) == 'undefined' || tab == null || nFileId <= 0)
            tab = 200;

        //Fonction de création de la modal
        var fct = function () {
            if (vCardModal && vCardModal.hide)
                vCardModal.hide();

            // #87 153 - On empêche l'affichage de la VCard si, après expiration de la temporisation, son élément déclencheur a disparu aux yeux de l'utilisateur entre temps
            // Exemples :
            // - menu d'onglet de la navbar qui a disparu
            // - pointage puis clic sur un élément du mode Liste amenant au mode Fiche
            if (!eTools.isInViewport(oVCardCaller, true))
                return;

            var existParent = parentObj != null;
            var width = 380;
            var height = 350;
            vCardModal = new eModalDialog("", 7, "mgr/eFileManager.ashx", width, height, "vcard", true);
            //vCardModal.addParam("tab", 200, "post");
            vCardModal.addParam("tab", tab, "post");
            vCardModal.addParam("fileid", nFileId, "post");
            vCardModal.addParam("type", 4, "post");
            vCardModal.addParam("bModal", 1, "post");
            vCardModal.addParam("bCroix", vcFromClick, "post"); // pour afficher ou non la croix
            vCardModal.noButtons = true;
            vCardModal.ErrorCallBack = launchInContext(vCardModal, vCardModal.hide);

            window.clearTimeout(RedimVCARD);
            vCardModal.onIframeLoadComplete = function () {
                //Redimensionnement automatique de la VCARD
                //Appelée aussi directement pour ne pas avoir d'effet visuel de redimensionnement
                RedimVCARD();
                window.setTimeout(RedimVCARD, 50); //Appelée avec un timer pour laisser le temps au contenu de la vcard de prendre sa taille finale


                //Affiche la croix de fermeture au mouseenter
                if (vCardModal.getIframe && !vcFromClick) {

                    var myDiv = vCardModal.getIframe();
                    if (myDiv && myDiv.document.getElementById("vcMain") && myDiv.document.getElementById("vcCross")) {
                        myDiv.document.getElementById("vcMain").onmouseenter = function () {
                            myDiv.document.getElementById("vcCross").style.visibility = "visible";
                        }
                    }
                }
            }

            if (existParent) {
                var obj_pos = getAbsolutePosition(parentObj);
                var child_pos = getAbsolutePosition(oVCardCaller);
            } else {
                var obj_pos = getAbsolutePosition(oVCardCaller);
            }

            // get column width
            let nCarWidth = obj_pos.w;
            if (!hasClass(oVCardCaller, "navLst") && !isIris(top.getTabFrom())) {
                try {
                    nCarWidth = GetText(oVCardCaller).length;
                    if (nCarWidth > 0 && nCarWidth < obj_pos.w)
                        nCarWidth = nCarWidth * 8;
                    else
                        nCarWidth = obj_pos.w;
                }
                catch (ex) {
                    nCarWidth = obj_pos.w;
                }
            }

            var posX = obj_pos.x + nCarWidth + 2;   //TailleFixe car si la colonne est trop grande la VCARD est tout à droite
            var posY = obj_pos.y;
            if (existParent && typeof (child_pos.y) != undefined && child_pos.y > 0) {
                posY += child_pos.y;
            }

            // Calcul à partir de la taille fictive de la modal
            var winSize = getWindowSize();
            if (posY + height > winSize.h)
                posY = winSize.h - height;

            //HDJ probleme d'affichage gauche droite carte de visite
            if (posX + width > winSize.w) {
                //si le champ est à droite de la fenêtre, on peut afficher la vCard à gauche du champ
                if (obj_pos.x - width - 2 > 0)
                    posX = obj_pos.x - width - 2;
                else
                    posX = winSize.w - (width * 2);
            }

            if (posY < 0)
                posY = 0;

            // US #1 078 (Tâche #1 859) - Positionnement par rapport au scroll
            if (posY < vCardModal.topScrollHeight)
                posY += vCardModal.topScrollHeight;

            vCardModal.show(posX, posY);




        };

        //ajoute un délai sur le mouseover 
        showVcardTimer = window.setTimeout(function () { fct() }, 400);
    }
    else {
        // #76 232 et US #1 078 (Tâche #1 859) - Restauration de l'infobulle système
        // Depuis le correctif #58 259 qui passe obj à null, il faut impérativement le vérifier avant d'agir dessus
        if (obj && obj.getAttribute("title_backup") != null) {
            obj.title = getAttributeValue(obj, "title_backup");
            obj.removeAttribute("title_backup");
        }

        //if (!debugModeList) {
        clearTimeout(showVcardTimer);

        if (vCardModal && vCardModal.hide) {
            vcFromClick = false;
            vCardModal.hide();
        }
        //}
    }


}
//Permet de redimensionner la taille de la frame conteneur de la VCARD en fonction de la div principale dans la frame pour que dans IE8 la marge blanche hors de la vcard ne se voit pas
function RedimVCARD() {
    var oFrame = vCardModal.getIframe();
    if (!oFrame)
        return;
    var oVC = oFrame.document.getElementById('vcMain');
    if (!oVC)
        return;
    var SizeObj = getAbsolutePosition(oVC);
    if (SizeObj.h > 0) {
        parent.document.getElementById(vCardModal.iframeId).style.height = SizeObj.h;
    }
    if (SizeObj.w > 0) {
        parent.document.getElementById(vCardModal.iframeId).style.width = SizeObj.w - 5;   //-5 pour supprimmer marge de droite
    }
}

/*
Action attachée à l'évènement onclick du document
Si le clic est effectué en dehors de la VCARD, la VCARD est fermée puis l'évènement sur le document est retiré
*/
function HideVcard(e) {


    if (!e)
        e = window.event;
    try {
        var src = (e.originalTarget || e.srcElement);
        var srcParentNode = src;
        if (src.parentNode) { srcParentNode = src.parentNode; }
        if (e && vCardModal &&
            !((src == oVCardCaller) || (srcParentNode == oVCardCaller) || this.SameParentFrameId(src, vCardModal.iframeId))
        ) {
            shvc(oVCardCaller, 0); //Cacher la vcard
            this.setWindowEventListener('click', null);
        }
    }
    catch (Exc) {
        this.setWindowEventListener('click', null);
    }
}

/* Affichage tooltip de filtre */

/// <summary>
/// Affiche la description du filtre
/// </summary>
/// <param name="elem">élément clicker</param>
var eventSave;
var toTT;

function shFilterDescriptionById(event, nFilterId) {
    eventSave = event;
    var url = "mgr/eFilterWizardManager.ashx";
    var ednu = new eUpdater(url, 0);
    ednu.addParam("action", "getdesc", "post");
    ednu.addParam("filterid", nFilterId, "post");

    //Masque le tool tip si erreur
    ednu.ErrorCallBack = ht;

    toTT = setTimeout(function () { ednu.send(onShowDescFilter); }, 100);
}

function shFilterDescription(event, sDescription) {
    if (sDescription) {
        sDescription = decodeHTMLEntities(sDescription);
        st(event, sDescription, "filterToolTip");
    }
}

function hideFilterDescription() {
    clearTimeout(toTT);
    ht();
}

/// <summary>
/// Gestion de l'affichage de la description
///   -> Show Tooltip
/// </summary>
/// <param name="oDoc">Flux XML de retour</param>
function onShowDescFilter(oDoc) {
    if (oDoc && oDoc.nodeType == 9) {

        var bSuccess = (getXmlTextNode(oDoc.getElementsByTagName("success")[0]) == "1");

        if (!bSuccess) {
            var nErrCode = getXmlTextNode(oDoc.getElementsByTagName("ErrorCode")[0]);
            var sErrDesc = getXmlTextNode(oDoc.getElementsByTagName("ErrorDescription")[0]);
            showWarning(top._res_1713, top._res_719 + " : ", "(" + nErrCode + ")  " + sErrDesc);
            // masque le tool tip
            ht();
        }
        else {

            var sDesc = getXmlTextNode(oDoc.getElementsByTagName("filterdescription")[0]);
            var nId = getXmlTextNode(oDoc.getElementsByTagName("filterid")[0]);



            st(eventSave, sDesc, "filterToolTip");

            if (window.event)
                var origNode = eventSave.srcElement;
            else
                var origNode = eventSave.target;
            //if (origNode != null)
            //    origNode.onmouseout = function () {
            //        hideTT = setTimeout(ht, 250);
            //    };
        }
    }
    else {
        //retour invalide, on masque le tooltip
        ht();
    }
}

// Vérifie qu'on est en mode admin
function isInAdminMode() {
    var bAdminMode = false;
    var divMenuBar = top.document.getElementById('menuBar');
    if (divMenuBar) {
        bAdminMode = getAttributeValue(divMenuBar, "adminmode") == "1" && document.getElementById("adminNav");
    }
    return bAdminMode;
};

// US #1330 - Tâches #2748, #2750 - Paramètre les variables utilisées par eParamIFrame pour recharger le TabID, FileID, type d'affichage actuels après rafraîchissement de la page via loadTabs() (ou autre)
// Depuis le correctif de l'US #1330, ces variables reprennent le contexte courant (nGlobalActiveTab, getCurrentView()) uniquement si elles sont undefined, et non plus systématiquement (par erreur).
nsMain.setParamIFrameReloadContext = function (nTabId, nFileId, view, isTplMail, loadFileInPopup, preventSwitchToHiddenTabUsingView) {
    top.tabToLoadAfterParamIFrame = nTabId;
    top.fileToLoadAfterParamIFrame = nFileId;
    top.viewToLoadAfterParamIFrame = view;
    // Attention, les deux variables ci-dessous sont très souvent forcées à 0 dans certaines fonctions (notamment loadTabs()), même si elles ne sont pas undefined, contrairement aux trois ci-dessus.
    top.isTplMailToLoadAfterParamIFrame = isTplMail;
    top.loadFileInPopupAfterParamIFrame = loadFileInPopup;
    top.preventSwitchToHiddenTabUsingView = preventSwitchToHiddenTabUsingView;
};

/** toutes les vues d'onglet dans le menu (+) */
nsMain.getViewTab = function () {

    var oeParam = top.document.getElementById('eParam').contentWindow;

    // Charge les listes des MRU sur le menu de chaque onglets
    if (!oeParam
        || typeof (oeParam) == "undefined"
        || typeof (oeParam.GetParam) != "function"
        || !oeParam.GetParam("ViewTab") != "")
        return false;

    var viewTab = oeParam.GetParam("ViewTab");

    return viewTab.split('$|$').map(
        function (viewTabInfo) {
            return viewTabInfo.split('$;$');
        }
    );
};

/**
 * Renvoie la liste des vues contenant le TabID donné
 * @param {any} nTab TabID cible
 */
nsMain.getViewsContainingTab = function (nTab) {
    var tabInViews = nsMain.getViewTab().map(
        function (x) {
            return x.map(
                function (y) {
                    return y.split(',').map(
                        function (viewTabInfo) {
                            return viewTabInfo.split(';');
                        }
                    );
                }
            );
        }
    );

    //Objet qui récupère les vues dont l'utilisateur dispose
    var viewObj = [];

    tabInViews.forEach(function (arr) {

        //vérifie que la vue a bien l'onglet en question

        var verif = arr[3].map(
            function (y) {
                return y.filter(
                    function (x) {
                        return x == nTab;
                    }
                );
            }
        );

        // Objet qui récupère les vues dont l'utilisateur dispose et qui dispose de l'onglet où l'utilisateur souhaite naviguer
        // Remarque : sur IE caca, la fonction flat() n'existe pas nativement, elle est assurée par un polyfill ajouté sur eTools.js
        verif.flat(1).length > 0 ? viewObj.push({
            "view": arr[0].toString(),
            "name": arr[1].toString(),
            "tabs": arr[3].flat(1)
        }) : ''


    });

    return viewObj;
}

nsMain.switchToHiddenTabUsingView = function (nTab, nFileId, viewType) {
    // US #1330 - Tâche #2908 - Empêcher le réaffichage du message si on vient justement de recharger la page pour procéder à une redirection
    if (top.preventSwitchToHiddenTabUsingView) {
        top.preventSwitchToHiddenTabUsingView = false; // après un refus, on autorisera de nouveau les demandes de redirection...
        return false; // mais on envoie bouler pour cette fois-ci :)
    }

    var viewObj = nsMain.getViewsContainingTab(nTab);

    //Objet qui récupère les vues dont l'utilisateur dispose et qui dispose de l'onglet où l'utilisateur souhaite naviguer
    //ElAIZ - On vérifie que l'on a l'onglet recherche dans une autre vue sinon on met une modale d'alerte
    if (viewObj.length < 1) {
        eAlert(3, top._res_5080, top._res_6368, null, 600); // Pour accéder à cette fiche, vous devez préalablement choisir d’afficher l’onglet dans votre barre d’onglets.
        return;
    } else {
        var modalTitle = "";
        if (viewObj.length > 1) {
            modalTitle = top._res_2591; //Cet onglet n'est pas affiché dans votre barre d'onglet, choississez la vue vers laquelle naviguer pour afficher l'onglet :
        }
        else {
            //Cet onglet n'est pas affiché dans votre barre d'onglet, souhaitez-vous naviguer vers la vue XXX pour afficher l'onglet ?;
            modalTitle = top._res_2592.replace("{0}", viewObj[0].name);
        }

        var ec = eConfirm(eMsgBoxCriticity.MSG_QUESTION, top._res_201, modalTitle, '', 475, 250,
            function () {
                var selectedChoice = null;
                var selectedView = null;
                if (ec) {
                    selectedChoice = ec.getSelected();
                }
                if (selectedChoice && selectedChoice.length > 0 && selectedChoice[0].val) {
                    selectedView = selectedChoice[0].val;
                }
                else {
                    if (viewObj.length > 0)
                        selectedView = viewObj[0].view; // par défaut, la première vue disposant de l'onglet (ou la seule)eAlert(0, top._res_72, top._res_6658, '');
                }

                // US #1330 - Tâches #2748, #2750 - On paramètre les variables utilisées par eParamIFrame.eParamOnLoad() pour recharger le TabID, FileID, type d'affichage actuels après rafraîchissement total via loadTabs()
                // qui est appelée par changeView()
                nsMain.setParamIFrameReloadContext(nTab, nFileId, viewType, undefined, undefined, true);
                // Et on déclenche la chaîne de fonctions : changeView => loadTabs => eParamOnLoad() => ...
                top.changeView(selectedView);
            },
            function () { if (ec != null) ec.hide(); }
        );

        //Indique que l'on souhaite que les options soient des RadioBoutons (choix unique)
        ec.isMultiSelectList = false;

        if (viewObj.length > 1) {
            for (var availableViewsIndex = 0; availableViewsIndex < viewObj.length; availableViewsIndex++) {
                ec.addSelectOption(viewObj[availableViewsIndex].name, viewObj[availableViewsIndex].view, availableViewsIndex == 0);
            }
            //Force l'ajout des radioboutons
            ec.createSelectListCheckOpt();
            ec.adjustModalToContent(40);
        }

        return;
    }
};

/**
 * Fonction qui va être appelée quand une image est en erreur.
 * Donc que le lien n'est pas fonctionnel.
 * On crée un élément de remplacement avec l'icone adéquat.
 * @param {any} cpt l'image appelante.
 */
function onErrorImg(cpt) {
    var spImgReplace = document.createElement("span");
    spImgReplace.classList.add("linkBroken", "icon-chain-broken");
    cpt.parentElement.appendChild(spImgReplace);
    cpt.remove();
}