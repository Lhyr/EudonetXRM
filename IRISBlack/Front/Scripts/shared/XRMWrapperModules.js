/*eslint no-undef:"warn" */

import EventBus from "../bus/event-bus.js?ver=803000";

/** Permet de ne plus afficher le message de news lorsque l'utilisateur clique sur Ne plus afficher
 */
function setNewsMessageStopDisplay() {
    /* Appel à la fonction de eDisplayNewsMessage */
    if (nsNewsMsg && nsNewsMsg.StopDisplay)
        nsNewsMsg.StopDisplay();
}

/**
Lance un état spécifique
*/
function setRunSpec(nSpecId, nTab, nFileId, nFieldDescId) {
    top.runSpec(nSpecId, nTab, nFileId, nFieldDescId);
}

/**
Renvoie la valeur passée en paramètre sans code HTML
Utilise la fonction removeHTML() d'eTools
*/
function getValueWithoutHTML(sValue) {
    return removeHTML(sValue);
}


/**
 Ajuste la taille du div container (contenant les modes fiche et liste)
 à la taille restant à l'écran sous les menus
*/
function adjustDivContainerXRM() {
    adjustDivContainer();
}

/**
 Fonction qui permet d'appeler un évènement IRIS depuis un code JS XRM
*/
top.emitIrisEvent = async function (eventName, options) {
    await EventBus.$emit(eventName, options);
}

/**
 Fonction qui permet d'appeler l'évènement IRIS de rechargement depuis un code JS XRM
*/
top.emitIrisLoadAll = async function (options) {
    await top.emitIrisEvent("emitLoadAll", options);
}

function getIrisPurpleActived(nTab) {
    return isIris(nTab, "dvIrisPurpleInput");
}

function openFileGuidedTyping(tab, fileid, mainFieldValue, nCallFrom, oParentInfo) {
    openPurpleFile(tab, fileid, mainFieldValue, nCallFrom, oParentInfo);
}

/**
 * Chargement des structures des Fiches.
 * On récupère le tout d'une classe externe au programme.
 * @returns {JSON}
 * */
function loadFileLayout() {
    return singletonFileLayout?.getInstance();

}

/**
 * Chargement des données des Fiches.
 * On récupère le tout d'une classe externe au programme.
 * @returns {JSON}
 * */
function loadFileDetail() {
    return singletonFileDetail?.getInstance();

}

/**
 * Chargement des données des signets.
 * On récupère le tout d'une classe externe au programme.
 * @returns {JSON}
 * */
function loadFileBookmark() {
    return singletonFileBookmark?.getInstance();

}

/**
 * Chargement des valeurs d'un catalogue utilisateur.
 * */
function loadFileUsers() {
    return singletonFileUsers?.getInstance();
}

/**
 * Chargement d'un catalogue.
 * On récupère le tout d'une classe externe au programme.
 * @returns {JSON}
 * */
function loadFileCatalog() {
    return singletonFileCatalog?.getInstance();
}

/**
 * Chargement pour la création et la mise à jour des données.
 * On récupère le tout d'une classe externe au programme.
 * @returns {JSON}
 * */
function loadFileCreateUpdate() {
    return singletonFileCreate?.getInstance();
}

/**
 * Chargement pour la récupération des MRU.
 * On récupère le tout d'une classe externe au programme.
 * @returns {JSON}
 * */
function loadFileMRU() {
    return singletonMRU?.getInstance();
}

/**
 * Chargement des sommes de colonnes calculées
 * */
function loadComputedValue() {
    return singletonComputedValue?.getInstance();
}

/**
 * Chargement pour la création et la mise à jour des données.
 * On récupère le tout d'une classe externe au programme.
 * @returns {JSON}
 * */
function loadFileUploadFile() {
    return singletonUploadFile?.getInstance();
}

/**
 * Chargement pour les signets épinglés.
 * On récupère le tout d'une classe externe au programme.
 * @returns {JSON}
 * */
function loadPinnedBookmark() {
    return singletonPinnedBookmark?.getInstance();
}

/** Status du nouveau mode fiche (actif, previsu, desactive) */
function loadStatusNewFile() {
    return singletonStatusNewFile?.getInstance();
}

/**
 * Chargement pour les menus du mode Fiche
 * @returns {JSON}
 * */
function loadIrisFileMenu() {
    return singletonFileMenu?.getInstance();
}


/** l'URL de base */
function getUrl() {
    let domaine = window.location.href.split("/");
    domaine.pop();
    return domaine.join("/");
}

/** les ressources linguistiques. */
function getRes(id) {
    return (top && top.hasOwnProperty("_res_" + id))
        ? top["_res_" + id]
        : "##[INVALID_RES_" + id + "]##";
}
/** Récupère l'id de la langue de l'utilisateur. */
function getUserLangID() {
    return (top && top.hasOwnProperty("_userLangId")) ? top._userLangId : 0;
}
/** récupère l'identifiant de l'utilisateur */
function getUserID() { return nGlobalCurrentUserid; }

/** récupère l'onglet actif */
function getCurrentTab() { return nGlobalActiveTab; }

/** Version à mettre à la suite des link css pour le cache */
function getVersionCSS() {
    return (top && top.hasOwnProperty("_CssVer")) ? top._CssVer : "";
}

/** Version à mettre à la suite des scripts js pour le cache */
function getVersionJs() {
    return (top && top.hasOwnProperty("_jsVer")) ? top._jsVer : "";
}

/** fonction permettant de savoir si on est sur tablette. */
function getIsTablet() {
    if (typeof (isTablet) == 'function')
        return isTablet();
    else if (typeof (top.isTablet) == 'function')
        return top.isTablet();
}

/** Retourne l'objet document via l'Iframe des params */
function getParamWindow() {
    let eParamWindow = top.document.getElementById('eParam');

    if (eParamWindow
        && eParamWindow.contentWindow
        && typeof (eParamWindow.contentWindow.GetParam) == "function")
        eParamWindow = eParamWindow.contentWindow;

    return eParamWindow;
};

function setMruParams(descId, evtId, evt01){
    let paramMruValue = `${evtId}$;$${evt01}$;$99$|UPDATE|$`

    let oeParam = getParamWindow();
    oeParam.SetMruParam(descId, paramMruValue);
}

/** retourne le format des dates sélectionné. */
function getInfoDate() {
    let dtCulture;
    let oeParam = getParamWindow();

    if (typeof (oeParam.GetParam) != "undefined" && oeParam.GetParam('CultureInfoDate') != '')
        dtCulture = oeParam.GetParam('CultureInfoDate');

    return dtCulture;
};

/** Permet de récupérer dans eParamIframe les données utilisateur. */
function getUserInfos() {
    let oUser;
    let oeParam = getParamWindow();

    if (typeof (oeParam.GetParam) != "undefined" && oeParam.GetParam('CultureInfoDate') != '')
        oUser = oeParam.GetParam('User');

    return oUser;
}

/**
 * Permet de récupérer le storage suivant son type.
 * */
function getStorage() {
    return async function (typeStorage) {
        let eStoragesHelper = await import("../helpers/eStoragesHelper.js");
        return new eStoragesHelper(typeStorage);
    }
};


/**
 * Wrapping pour sauvegarder les préf utilisateur dans XRM.
 * @param {any} updatePref
 * @param {any} callback
 */
function setUserBkmPref(updatePref) {

    let callback = () => { };

    if (updatePref.callback && typeof updatePref.callback == "function")
        callback = updatePref.callback

    top.updateUserBkmPref(updatePref.updatePref, callback);
}

/**
 * Wrapping pour envoyer à top le contexte de vue pour pouvoir traiter l'exception dans eEngine lorsque
        l'on modifie des valeur depuis la modale des signets
 * @param {any} updatePref
 * @param {any} callback
 */
function setIrisUpdateValFromBkmModal(objUpdateVal) {
    top._IrisUpdateValFromBkmModal = objUpdateVal;
}

/**
* Permet d'avoir la couleur active sur le bouton
* @param {any} button
*/
function getColor (button, nTab) {
    let oeParam = getParamWindow();
    let dvIrisBlackInput;
    let dvIrisBlackInputPreview;

    if (typeof (oeParam.GetParam) != "undefined" && oeParam.GetParam('dvIrisBlackInput') != '')
        dvIrisBlackInput = oeParam.GetParam('dvIrisBlackInput');

    if (typeof (oeParam.GetParam) != "undefined" && oeParam.GetParam('dvIrisBlackInputPreview') != '')
        dvIrisBlackInputPreview = oeParam.GetParam('dvIrisBlackInputPreview');

    let tabActive = dvIrisBlackInput?.split(";").map(x => parseInt(x)).includes(nTab);
    let tabPreview = dvIrisBlackInputPreview?.split(";").map(x => parseInt(x)).includes(nTab);

    if (tabActive && button == "activeNewFileMode") {
        return "primary"
    } else if (tabPreview && button == "activeNewFileModePreview") {
        return "primary"
    } else if (!tabPreview && !tabActive && button == "desactiveNewFileMode") {
        return "primary"
    } else {
        return "#3a454b"
    }

}


/**
 * Permet de forcer l'activation du switch Eudo.
 */
function fnForceChckNwThm() {
    let chk = document.querySelector("#chckNwThm");

    if (!chk.checked) {
        chk.checked = true;
        fnChgChckNwThm(chk, true);
    }
}


/**
 * Permet de forcer l'activation ou la désactivation du switch Eudo.
 * @param {any} toChecked
 */
function fnChckNwThm(toChecked) {
    let chk = document.querySelector("#chckNwThm");

    if (chk.checked != toChecked) {
        chk.checked = toChecked;
        fnChgChckNwThm(chk);
    }
}


/************************************************* Intégration de menu de droite *************************************************/
/**
 * Ouvre une fiche en popup
 * @param {any} nDescid
 * @param {any} fileid
 * @param {any} strTitle
 * @param {any} lWidth
 * @param {any} lHeight
 * @param {any} nMailMode
 * @param {any} mainFieldValue
 * @param {any} bApplyCloseOnly
 * @param {any} afterValidate
 * @param {any} nCallFrom
 * @param {any} sOrigFrameId
 * @param {any} oParentInfo
 * @param {any} callBack
 * @param {any} oOptions
 */
function shFileInPopup(nDescid, fileid, strTitle, lWidth, lHeight, nMailMode, mainFieldValue, bApplyCloseOnly, afterValidate, nCallFrom, sOrigFrameId, oParentInfo, callBack, oOptions) {
    top.shFileInPopup(nDescid, fileid, strTitle, lWidth, lHeight, nMailMode, mainFieldValue, bApplyCloseOnly, afterValidate, nCallFrom, sOrigFrameId, oParentInfo, callBack, oOptions);
}

/**
 * Ouvre le Finder pour créer une fiche
 * @param {any} nSearchType
 * @param {any} targetTab
 * @param {any} bBkm
 * @param {any} nCallFrom
 * @param {any} bNoLoadFile
 * @param {any} onCustomOk
 */
function openLnkFileDialog(nSearchType, targetTab, bBkm, nCallFrom, bNoLoadFile, onCustomOk) {
    top.openLnkFileDialog(nSearchType, targetTab, bBkm, nCallFrom, bNoLoadFile, onCustomOk);
}

/**
 * Demande de  fiche
 * @param {any} nTab
 * @param {any} nFileId
 * @param {any} nType
 * @param {any} bAsync
 * @param {any} loadFrom
 * @param {any} srcEltId
 * @param {any} srcElt
 */
function loadFile(nTab, nFileId, nType, bAsync, loadFrom, srcEltId, srcElt) {
    top.loadFile(nTab, nFileId, nType, bAsync, loadFrom, srcEltId, srcElt);
}

/**
 * Suppression de la fiche.
 * @param {any} nTab
 * @param {any} nFileId
 * @param {any} eModFile
 * @param {any} openSerie
 * @param {any} successCallBack
 * @param {any} bClose
 */
function deleteFile(nTab, nFileId, eModFile, openSerie, successCallBack, bClose) {
    top.deleteFile(nTab, nFileId, eModFile, openSerie, successCallBack, bClose)
}

/** Impression de la fiche en cours. */
function printFile() {
    top.printFile();
}

/**
 * Le rapport demandé.
 * @param {any} nReportType
 * @param {any} nTabBkm
 * @param {any} nSelectedReportId
 */
function reportList(nReportType, nTabBkm, nSelectedReportId) {
    top.reportList(nReportType, nTabBkm, nSelectedReportId);
}

/**
 * Assistant de création téléguidé
 * @param {any} tab
 * @param {any} fileid
 * @param {any} mainFieldValue
 * @param {any} nCallFrom
 * @param {any} oParentInfo
 */
function openPurpleFile(tab, fileid, mainFieldValue, nCallFrom, oParentInfo) {
    top.openPurpleFile(tab, fileid, mainFieldValue, nCallFrom, oParentInfo);
}

/**
 * Passe en mode liste
 * @param {any} nTab
 * @param {any} bReload
 * @param {any} callbackFct
 * @param {any} bUseHistory
 * @param {any} bForceList
 */
function goTabList(nTab, bReload, callbackFct, bUseHistory, bForceList) {
    top.goTabList(nTab, bReload, callbackFct, bUseHistory, bForceList)
}


/************************************************* Fin intégration de menu de droite *************************************************/

export {
    setNewsMessageStopDisplay, setRunSpec, getValueWithoutHTML, adjustDivContainerXRM, getIrisPurpleActived, openFileGuidedTyping, loadFileLayout, loadFileDetail,
    loadFileCreateUpdate, loadFileBookmark, loadFileUsers, loadFileCatalog, loadFileMRU, loadComputedValue, loadFileUploadFile, loadPinnedBookmark, loadIrisFileMenu,
    shFileInPopup, loadFile, deleteFile, printFile, reportList, openPurpleFile, goTabList, openLnkFileDialog, getUrl, getRes, getUserLangID, getUserID, getCurrentTab, getUserInfos, getVersionCSS,
    getVersionJs, getIsTablet, getParamWindow, getInfoDate, getStorage, setUserBkmPref, setIrisUpdateValFromBkmModal, getColor, loadStatusNewFile, fnChckNwThm, fnForceChckNwThm, setMruParams
}