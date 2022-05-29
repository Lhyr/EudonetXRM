var beginDateDescIdTpl;
if (document.getElementById("BeginDateDescId") != null)
    beginDateDescIdTpl = document.getElementById("BeginDateDescId").value;
var endDateDescIdTpl = 89;
document.isTplLoading = false;

var _parentIframeId = "";

var arrLastValues;


//Initialise les objets
function fileInitObject(nTab, bSwTab, loadFrom, to) {


    var toFileType;
    if (to) {
        toFileType = to // FILE
    } else {
        toFileType = 3 // FILE
    }

    if (loadFrom != LOADFILEFROM.REFRESH) {
        initEditors();
        if (loadFrom != LOADFILEFROM.MODERESUME)
            arrLastValues = [];
    }
    else {
        initEditorsOnScroll(); //cette instruction fait partie de initEditors mais dans le cas où on vient d'un refresh, on ne fait pas appel à initEditors
    }



    if (typeof (bSwTab) == "undefined")
        bSwTab = true;

    if (bSwTab && typeof (switchActiveTab) == "function") {
        switchActiveTab(nTab, toFileType);
    }

    initFileField(nTab);

    loadAutoSuggestModule();

}

function loadAutoSuggestModule() {
    var fields = document.querySelectorAll("[data-provider='1'][autocpl='1']");
    if (fields.length > 0) {

        var bingKey = "";

        var accessKeyElt = document.getElementById("accessKey");

        // MAB - 2018-01-29 : si la clé ne se trouve pas sur la page en cours (ex : ouverture de la fiche en popup), on la recherche sur la page parente
        if (!accessKeyElt)
            accessKeyElt = top.document.getElementById("accessKey");

        if (accessKeyElt)
            bingKey = accessKeyElt.value;

        if (typeof Microsoft !== "undefined" && typeof Microsoft.Maps !== "undefined" && bingKey) {

            try {
                Microsoft.Maps.loadModule('Microsoft.Maps.AutoSuggest', {
                    callback: function () { oAutoCompletion.loadBingAutoSuggest(fields); },
                    //errorCallback: onError,
                    credentials: bingKey
                })
            }
            catch (ex) {

            }
        }

    }
}

//initialise un tableau contenant des editeurs
function initFileTb(objId) {
    var blockElt;

    try {
        blockElt = document.getElementById(objId);
        if (blockElt) {
            blockElt.onclick = fldLClick;
            blockElt.onkeyup = fldKeyUp;
            blockElt.onselect = fldSelect;

            blockElt.oncontextmenu = fldInfoOnContext;
        }
    }
    catch (e) {
        alert('Initialisation de la fiche invalide : ' + objId);
    }

}

/// Initialise l'event click et à la pression d'une touche sur les cellules 
function initFileField(nTab) {


    // cas des fiches dont la mise en page des premiers champs est fixe (PP PM Adresses)
    try {
        initFileTb("fts_" + nTab);
    }
    catch (e) {
        alert('Initialisation de la fiche invalide initFileField 2');
    }


    //tableau principal
    try {
        initFileTb("ftm_" + nTab);
    }
    catch (e) {
        alert('Initialisation de la fiche invalide ');
    }



    // cas des notes en fiches template
    try {
        initFileTb("ftn_" + nTab);
    }
    catch (e) {
        alert('Initialisation de la fiche invalide : cas des notes en fiches template');
    }


    // cas des fiches dont une partie est affichée en signet
    try {
        initFileTb("ftdbkm_" + nTab);
    }
    catch (e) {
        alert('Initialisation de la fiche invalide : cas des fiches dont une partie est affichée en signet');
    }


    if (nTab == 200) {
        try {
            if (document.getElementById("ftm_" + 400))
                initFileTb("ftm_" + 400);

            if (document.getElementById("fts_" + 400))
                initFileTb("fts_" + 400);

            if (document.getElementById("ftn_" + 400))
                initFileTb("ftn_" + 400);
        }
        catch (e) {
            alert('Initialisation de la fiche invalide initFileFieldClick sur adresse');
        }
    }

    // informations parentes en en-tête
    try {
        initFileTb("ftp_" + nTab);
    }
    catch (e) {
        alert('Initialisation de la fiche invalide : l\'initialisation du cadre des informations parentes a échoué');
    }
}

function OpenCloseSep(separator, bSave, bInit) {

    var oTableSep = separator;

    if (typeof (bSave) == "undefined")
        bSave = true;

    if (oTableSep.tagName == "TD") {
        while (oTableSep.tagName != "TABLE") {
            oTableSep = oTableSep.parentNode || oTableSep.parentElement;
        }
    }

    var oRows = oTableSep.rows;
    var sTag = separator.id;

    for (var i = 0; i < oRows.length; i++) {

        var oRow = oRows[i];

        if (oRow.getAttribute("epagesep") != sTag)
            continue;

        if (separator.getAttribute("eOpen") == "1")
            oRow.style.display = "none";
        else {
            //oRow.style.removeProperty("display");
            oRow.removeAttribute("style");
        }
    }

    var isClosed;
    if (separator.getAttribute("eOpen") == "1") {
        isClosed = 1;
        separator.setAttribute("eOpen", "0");
        switchClass(separator.querySelector("span[id=sepIcon]"), "icon-title_sep", "icon-edn-next");
    }
    else {
        isClosed = 0;
        separator.setAttribute("eOpen", "1");
        switchClass(separator.querySelector("span[id=sepIcon]"), "icon-edn-next", "icon-title_sep");
    }

    var tab = 0;
    var descid = separator.id.split('_');
    var descid = descid[descid.length - 1];
    if (getNumber(descid) >= 100)
        tab = descid - (descid % 100);

    if (descid >= 100 && bSave) {
        var updUsrVal = "type=11;$;tab=" + tab + ";$;descid=" + descid + ";$;enabled=" + isClosed;
        //Aucune action hors du message
        updateUserValue(updUsrVal, null, function () { })
    }

    // on actualise le signet pour ajuster la pagination
    var nActiveBkm = getActiveBkm(tab);
    if (!bInit && nActiveBkm > 0 && !isPopup())
        loadBkmList(nActiveBkm);

    adjustScrollFile();
}

///Ouvre ou ferme le séparateur "globale" du mode fiche
function OpenCloseSepAll(separator, bNoReload) {

    //recharge ou pas les signets pour s'adapter à la page
    // dans le cas d'un appel via initSeparator, il ne faut pas relancer le calcul du signet, sinon, il est charger 2 fois (le chargement initial + celui ci dessous
    // et apparait en double dans les signets
    if (typeof (bNoReload) == "undefined" || bNoReload == null)
        bNoReload = false;

    // Pas de close all en mode popup
    if (typeof (isPopup) != "undefined" && isPopup())
        return;

    var oTbSysFld = document.getElementById("fts_" + nGlobalActiveTab);  // dans le cas de PM et PP, ce tableau contient les champs dont la mise en page est spécifique et ne peut être modifiée
    var oTbMain = document.getElementById("ftm_" + nGlobalActiveTab);
    var bHidden = getAttributeValue(oTbSysFld, "h") == "1" || getAttributeValue(oTbMain, "h") == "1";

    UpdateTableState(oTbSysFld, bHidden, 3);
    UpdateTableState(oTbMain, bHidden, oTbSysFld ? 0 : 3);

    SwitchSepState(separator, bHidden);

    if (!bNoReload)
        UpdtUsrValAndReloadBkmList(bHidden);

    adjustScrollFile();
}

function switchModeResume(element) {

    setWait(true);
    var bModeResume;
    if (getAttributeValue(element, "rmode") == "1") {
        bModeResume = false;
        element.className = "icon-uncollapse";
        element.title = top._res_6878;
        setAttributeValue(element, "rmode", "0");
    }
    else {
        bModeResume = true;
        element.className = "icon-collapse";
        element.title = top._res_6291;
        setAttributeValue(element, "rmode", "1");
    }

    // Mise à jour de la PREF d'affichage ou non 
    var updUsrVal = "type=22;$;tab=" + nGlobalActiveTab + ";$;value=" + (bModeResume ? "1" : "0");
    updateUserValue(updUsrVal);

    if (!isInAdminMode())
        loadfiletrt(nGlobalActiveTab, getAttributeValue(element, "fileid"), 3, false, LOADFILEFROM.MODERESUME, element);
    else
        nsAdmin.loadAdminModule(USROPT_MODULE_ADMIN_TAB, null, { tab: nGlobalActiveTab });

    //OpenCloseSepAll(document.getElementById('opencloseall'));
    return;

    /*
    // Mise à jour de la PREF d'affichage ou non 
    //var updUsrVal = "type=22;$;tab=" + nGlobalActiveTab + ";$;value=" + (bModeResume ? "1" : "0");
    //updateUserValue(updUsrVal);

    var bl = document.getElementById("resumeBreakline");
    if (bl) {
        var divParent = findUp(bl.parentElement, "DIV");
        var arrTR = new Array();
        var nextTr = bl.nextSibling;
        var clonedTr;

        var listTR = divParent.querySelectorAll("table tr");
        var arrListTR = [].slice.call(listTR);

        // Recherche de la TR breakline
        var trBL = findBreakLineTr(arrListTR);
        if (trBL) {
            //bl.style.display = (bModeResume) ? "none" : "";

            var beginIndex = arrListTR.indexOf(trBL) + 1;
            for (var i = beginIndex; i < arrListTR.length; i++) {
                nextTr = arrListTR[i];
                if (bModeResume) {
                    if (nextTr.className != "emptyrow") {
                        // On clone les TR pour les afficher dans détails et on les masque
                        clonedTr = nextTr.cloneNode(true);
                        //nextTr.style.display = "none";
                        addClass(nextTr, "hidden");
                        arrTR.push(clonedTr);
                    }
                }
                else {
                    // On réaffiche les TR
                    //nextTr.style.display = "";
                    removeClass(nextTr, "hidden");
                }
            }
        }

        // Dans le cas où on passe les champs dans le signet...
        if (bModeResume) {
            var newTable = document.createElement("table");
            newTable.id = "tmpTable";
            newTable.className = "mTab mTabFile";

            var details = document.getElementById("ftdbkm_" + nGlobalActiveTab);
            if (details) {
                // Ajout de la ligne vide pour la taille des colonnes
                var emptyrow = details.querySelector("tr.emptyrow").cloneNode(true);
                // Suppression du td "vcCadre"
                var vcCadre = emptyrow.querySelector("#vcCadre");
                if (vcCadre) {
                    emptyrow.removeChild(vcCadre);
                }
                newTable.appendChild(emptyrow);
                // Ajout de toutes les lignes
                for (var i = 0; i < arrTR.length; i++) {
                    newTable.appendChild(arrTR[i]);
                }
                details.parentElement.insertBefore(newTable, details);
            }
        }
        else {
            var newTable = document.getElementById("tmpTable");
            if (newTable)
                newTable.remove();
        }
    }
    */


}

function findBreakLineTr(arrTR) {

    var bl = null;
    for (var i = 0; i < arrTR.length; i++) {
        bl = arrTR[i].querySelector("#resumeBreakline");
        if (bl != null) {
            return arrTR[i];
        }
    }
    return null;
}

///Met à jour les ligne de la table  en fonction de l etat actuel du separateur de la fiche
function UpdateTableState(htmlTable, bHidden, startPos) {

    if (htmlTable) {
        var rowPrevious = null, rowCurrent, bOpen = true;
        for (var i = startPos; i < htmlTable.rows.length; i++) {

            rowPrevious = rowCurrent;
            rowCurrent = htmlTable.rows[i];

            //La ligne est un separateur
            if (bHidden && rowPrevious != null && getAttributeValue(rowPrevious, "hassep") == "1") {
                //On cherche l'etat actuel du separateur première TD
                bOpen = getAttributeValue(rowPrevious.children[0], "eopen") == "1";
            }


            //#49 891, quand on clique sur 'Tout Masquer' pour afficher le titre séparateur , il ne faut pas se baser sur le champ précédent, il faut se baser sur le titre pour l'afficher ou le cacher
            if (getAttributeValue(rowCurrent, "hassep") == "1") {
                rowCurrent.style.display = bHidden ? "" : "none";
            }
            else {
                rowCurrent.style.display = bHidden && bOpen ? "" : "none";
            }

        }

        htmlTable.setAttribute("h", bHidden ? "0" : "1");
    }
}

///Change l'etat de l icon separateur
function SwitchSepState(sep, bHidden) {
    if (bHidden) {

        switchClass(sep, "icon-collapse", "icon-uncollapse");
        sep.title = top._res_6291; // tout masquer

    }
    else {

        switchClass(sep, "icon-uncollapse", "icon-collapse");
        sep.title = top._res_6878; // tout afficher

    }
}

///Recharge la liste des signet et met a jour le user value
function UpdtUsrValAndReloadBkmList(bHidden) {
    if (bHidden == true || bHidden == false) {
        var updUsrVal = "type=22;$;tab=" + nGlobalActiveTab + ";$;value=" + (bHidden ? "0" : "1");
        updateUserValue(updUsrVal);
    }

    // on actualise le signet pour ajuster la pagination
    var nActiveBkm = getActiveBkm(nGlobalActiveTab);
    if (nActiveBkm > 0 && !isPopup())
        loadBkmList(nActiveBkm);
}

// #31 762 - La fonction GetCurrentFileId(nTab) se trouve désormais dans eMain.js
function RefreshFile(ownerWindow, srcEltId, tabToRefresh) {

    // sph- #32628
    // ownerWindow est nécessaire pour les refresh en popup
    // si non, le loadfile est appelé sur la fenêtre du contexte, qui peut être différente
    // il est aussi nécessaire d'aller chercher l'id de la fiche a rafraichir sur cet objet
    // en effet, RefreshFile est parfois appelé "directement" sans préciser la fenêtre d'appel :
    //  RefreshFile(obj) voir RefreshFile() au lieu de modalFile.RefreshFile()
    // suivant le contexte, "document" peut ne pas être celui de la fenêtre a rafraichir et donc prendre le mauvais id 
    if (!(typeof (ownerWindow) != "undefined" && ownerWindow != null)) {
        ownerWindow = window;
    }

    var bPopup = (typeof (ownerWindow.isPopup) == "function" && ownerWindow.isPopup());

    if (bPopup) {
        if (tabToRefresh != null && tabToRefresh != ownerWindow.nGlobalActiveTab)
            return;

        var fileId = getAttributeValue(ownerWindow.document.getElementById("fileDiv_" + ownerWindow.nGlobalActiveTab), "fid");
    }
    else {
        if (tabToRefresh != null && tabToRefresh != nGlobalActiveTab)
            return;

        var fileId = top.GetCurrentFileId(nGlobalActiveTab);
    }

    // relance toute la fiche avec ces signets
    if (fileId != '') {
        if (bPopup) {
            var nType;
            switch (getCurrentView(ownerWindow.document)) {
                case "CALENDAR":
                    nType = 5;
                    break;
                case "FILE_MODIFICATION":
                    nType = 3;
                    break;
                default:
                    nType = 2;
            }
            ownerWindow.loadFile(ownerWindow.nGlobalActiveTab, fileId, nType, false, LOADFILEFROM.REFRESH, srcEltId);
        }
        else {
            switch (getCurrentView(document)) {
                case "CALENDAR":
                    nType = 5;
                    break;
                case "FILE_MODIFICATION":
                    nType = 3;
                    break;
                default:
                    nType = 2;
            }
            loadFile(nGlobalActiveTab, fileId, nType, false, LOADFILEFROM.REFRESH, srcEltId);
        }
    }
}

function RefreshHeader() {
    if (isPopup())
        return;

    var nTab = nGlobalActiveTab;
    var nFileId = GetCurrentFileId(nTab);

    // #90 916 : sur le nouveau mode Fiche, on émet un signal pour le rafraîchissement de l'entête seul        
    if (top.isIris(nTab)) {
        top.emitIrisLoadAll({
            reloadSignet: false,
            reloadHead: true,
            reloadAssistant: false,
            reloadAll: false,
        });
        return;
    }

    // TODO relancer uniquement la fiche et pas les signets (sur E17)
    if (nFileId != '') {
        var nType = getCurrentView(document) == "FILE_MODIFICATION" ? 3 : 2;
        loadFilePart1(nTab, nFileId);
        loadFilePart2(nTab, nFileId);

    }
}

function RefreshFileHeader() {
    if (isPopup())
        return;

    var fileId = GetCurrentFileId(nGlobalActiveTab);
    if (fileId != '')
        rldPrtInfo(nGlobalActiveTab, fileId);
}

function RefreshAllBkm() {

    // actualiser l'affichage en dessous de la barre des signets
    var nActivBkm = getActiveBkm(nGlobalActiveTab);
    var oHeadDtls = document.getElementById("bkmDtls");

    if (oHeadDtls && nActivBkm == 0) {
        resetBkmCtner();
    }
    else {
        loadBkmList(nActivBkm, false);
    }
}

function RefreshBkm(nTab) {
    // Lorsque l'on vient de la table contact et que l'on veut rafraichir la table société ou l'inverse, on force le rafraichissement de la table adresse. Demande 31540.
    if ((top.nGlobalActiveTab == 300 && nTab == 200) || (top.nGlobalActiveTab == 200 && nTab == 300))
        nTab = 400;

    //ELAIZ - Tâche 2451 - Ajouter icône et action associée pour ouvrir la fiche en Popup - supression de fiche

    if (document.querySelector('#refreshirisbkm_' + nTab)) {
        document.querySelector('#refreshirisbkm_' + nTab).click();
    }

    // relancer l'affichage du signet de la table demandé
    var nActivBkm = getActiveBkm(nGlobalActiveTab);
    var eltNumPage;
    if (nActivBkm == -1 || nActivBkm == nTab) {
        if (isBkmFile(nTab)) {
            var nCurrentFilePos = 0;
            var nCurrentFileId = 0;
            var nCurrentPage = 0;
            eltNumPage = document.getElementById("bkmNumFilePage_" + nTab)
            if (eltNumPage) {
                nCurrentFilePos = getNumber(eltNumPage.value) - 1;
                nCurrentPage = getNumber(getAttributeValue(document.getElementById("bkmTitle_" + nTab), "pg"));
            }
            else {
                nCurrentFileId = GetCurrentFileId(nTab)
            }
            loadBkm(nTab, nCurrentPage, null, null, nCurrentFileId, nCurrentFilePos)
        }
        else {
            var nCurrentPage = 1;
            eltNumPage = document.getElementById("bkmNumPage_" + nTab)
            if (eltNumPage)
                nCurrentPage = getNumber(eltNumPage.value);

            loadBkm(nTab, nCurrentPage);
        }
    }
}

//fait appel à eFileAsyncManager.ashx
//nReq partie de la fiche demandée
var REQ_PART_NONE = 0;           //Pas d'info (dans ce cas on arretera la page)
var REQ_PART_PART1_WITHBACKBONE = 1;          //Partie haute de la fiche avec le squelette
var REQ_PART_PART2 = 2;         //Partie de la fiche reportée dans les signets
var REQ_PART_BKMBAR = 3;         //Barre des signets
var REQ_PART_BKM = 4;            //Signet individuel
var REQ_PART_BKMALLBUT = 5;      //Tous les signets de la fiche sauf 1
var REQ_PART_PART1_ONLY = 6;          //Partie haute de la fiche sans le squelette
var REQ_PART_PROPERTIES = 7;          //Partie haute de la fiche sans le squelette
var REQ_PART_BKMBLOCK = 8;          //Block entier des bkm
var REQ_PART_BKMBLOCK = 8;          //Block entier des bkm
var REQ_PART_DISC_COMM = 9;          //Signet discussion : rafraichissement d'un commentaire unique
var REQ_PART_XRM_HOMEPAGE_GRID = 10; // grille pour les pages d'accueil

function getFileAsyncUpd(nTab, nFileId, nReq) {
    var url = "mgr/eFileAsyncManager.ashx";
    var oFileUpdater = new eUpdater(url, 1);

    try {
        oFileUpdater.addParam("callby", arguments.callee.name.toString(), "post");
        oFileUpdater.addParam("callbyarguments", arguments.callee.arguments.toString(), "post");
    }
    catch (e) {

    }
    // En cas d'erreur, retourne au mode liste
    oFileUpdater.ErrorCallBack = function () {

        setWait(false);
    }

    oFileUpdater.addParam("tab", nTab, "post");
    oFileUpdater.addParam("fileid", nFileId, "post");
    oFileUpdater.addParam("part", nReq, "post");

    var fileWidth = GetFileWidth();
    if (fileWidth > 0)
        oFileUpdater.addParam("width", fileWidth, "post");

    // #92 393 - Passage du numéro de page actuel du signet, pour le transmettre au contrôleur et réafficher la même page après rafraîchissement
    var oeParam = getParamWindow();
    var iPageIndex = 1;
    if (oeParam.GetParam('Page_' + nTab) != '') {
        iPageIndex = parseInt(oeParam.GetParam('Page_' + nTab));
        oFileUpdater.addParam("pageindex", iPageIndex, "post");
    }

    return oFileUpdater;

}

function loadBkmBar(nTab, nFileId, bLoadBkmBlock, options) {
    setWait(true);
    var oFileUpdater;
    if (bLoadBkmBlock)
        oFileUpdater = getFileAsyncUpd(nTab, nFileId, REQ_PART_BKMBLOCK);
    else
        oFileUpdater = getFileAsyncUpd(nTab, nFileId, REQ_PART_BKMBAR);

    oFileUpdater.addParam("bftbkm", isFileTabInBkm() ? "1" : "0", "post");

    if (bLoadBkmBlock)
        oFileUpdater.send(function (oRes) { updateBkmList(oRes, null, true, options); });
    else
        oFileUpdater.send(function (oRes) { updateBkmBar(oRes, nTab, nFileId, options); });

}

function updateBkmBar(oRes, nTab, nFileId, options) {

    options = Object.assign({}, { noReload: false, uptCmptTab: 0 }, options)


    var oBkmBar = document.getElementById("bkmBar_" + nTab);

    if (!oBkmBar) {
        eAlert(0, top._res_72, top._res_6573, top._res_6574);
        setWait(false);
        return;
    }

    oBkmBar.outerHTML = oRes;


    if (options.uptCmptTab > 0) {

        var div = document.createElement("div")
        div.innerHTML = oRes


        var lblmaj = div.querySelector("td#BkmHead_" + options.uptCmptTab + " > a[fltlbl]")

        if (lblmaj) {
            var ctn = document.querySelector("span#bkmCntFilter_" + options.uptCmptTab)
            if (ctn) {
                ctn.innerHTML = " " + top._res_8257 + " " + getAttributeValue(lblmaj, "fltlbl")
            }
        }

    }

    if (options.noReload) {
        setWait(false);
        return;
    }

    resetBkmCtner();

    // #92 393 - Avant de rafraîchir le signet, on récupère le numéro de la page en cours pour pouvoir le passer à AddBkm() AVANT d'écraser le contenu (et donc de perdre l'information)
    var nCurrentBkmPage = 1;
    var nActivBkm = getActiveBkm(nTab);
    var eltBkmNumPage = document.getElementById("bkmNumPage_" + nActivBkm);
    if (eltBkmNumPage)
        nCurrentBkmPage = getNumber(eltBkmNumPage.value);

    var bFileTabInBkm = isFileTabInBkm();

    if (nActivBkm == -1 || (nActivBkm == 0 && !bFileTabInBkm)) {
        var nFirstBkm = getFirstBkm(nTab);

        if (nFirstBkm > 0)
            AddBkm(nTab, nFileId, nFirstBkm, nActivBkm == -1, nCurrentBkmPage);

    }
    else if (nActivBkm == 0 && bFileTabInBkm) {
        //loadFilePart2(nTab, nFileId);
        swFileBkm(0)
    }
    else {
        AddBkm(nTab, nFileId, nActivBkm, null, nCurrentBkmPage);
    }

    setWait(false);
}

function getFirstBkm(nTab) {
    var oBkmTab = document.getElementById('bkmTab_' + nTab);
    if (!oBkmTab || !oBkmTab.rows || oBkmTab.rows.length == 0 || oBkmTab.rows[0].cells.length == 0) {
        eAlert(0, top._res_72, top._res_6573, top._res_6574);
        return;
    }

    var oCells = oBkmTab.rows[oBkmTab.rows.length - 1].cells;
    var nFirstBkm = 0;
    for (var i = 0; i < oCells.length; i++) {
        var id = oCells[i].id.split("_");

        if (id[0] != "BkmHead")
            continue;


        nFirstBkm = id[1];

        if (isNaN(nFirstBkm))
            continue;

        break;

    }

    return nFirstBkm;
}

function loadFilePart1(nTab, nFileId) {
    setWait(true);
    var oFileUpdater = getFileAsyncUpd(nTab, nFileId, REQ_PART_PART1_ONLY);
    oFileUpdater.send(function (oRes) { updateFilePart1(oRes, nTab, nFileId); });

}

function updateFilePart1(oRes, nTab, nFileId) {
    var oDiv = document.getElementById("divFilePart1");

    if (!oDiv) {
        eAlert(0, top._res_72, top._res_6573, top._res_6574);
        setWait(false);
        return;
    }

    oDiv.outerHTML = oRes;

    //Titres séparateurs
    initSeparator(nTab);
    initFileField(nTab);
    initMemoFields(nTab, 2);
    loadAutoSuggestModule();

    adjustScrollFile();

    setWait(false);
}

function loadFilePart2(nTab, nFileId) {
    setWait(true);
    var oFileUpdater = getFileAsyncUpd(nTab, nFileId, REQ_PART_PART2);
    oFileUpdater.send(function (oRes) { updateFilePart2(oRes, nTab, nFileId); });

}

function updateFilePart2(oRes, nTab, nFileId) {
    var oDiv = document.getElementById("FlDtlsBkm");

    if (!oDiv) {
        eAlert(0, top._res_72, top._res_6573, top._res_6574);
        setWait(false);
        return;
    }

    oDiv.outerHTML = oRes;

    //Titres séparateurs
    initSeparator(nTab);
    initFileTb("ftdbkm_" + nTab);
    initMemoFields(nTab, 2);
    loadAutoSuggestModule();


    //swFileBkm(0)

    adjustScrollFile();

    setWait(false);

}

//Retourne le contenu d'un bookmark
//  bDisplayAll indique s'il faut charger les autres bookmark après celui initialement demandé.
function AddBkm(nTab, nFileId, nBkm, bDisplayAll, nPage) {

    var oParamGoTabList = {
        to: 3,
        nTab: nTab
    }

    //Appel le waiter
    setWait(true, undefined, undefined, isIris(top.getTabFrom()));

    if (!isBkmGrid(nBkm))
        hideBkmGrid();

    // Cas de signet Grille
    if (bDisplayAll && isBkmGrid(nBkm)) {
        // on exclute le signet grille dans le signet Tous 
        AddAllBkmBut(nTab, nFileId, nBkm);
        return;
    }


    var oFileUpdater = getFileAsyncUpd(nTab, nFileId, REQ_PART_BKM);
    oFileUpdater.addParam("bkm", nBkm, "post");
    if (isBkmFile(nBkm)) {
        var eltNumPage = document.getElementById("bkmNumFilePage_" + nBkm);
        var nBkmFilePos = 0;
        if (eltNumPage) {
            nBkmFilePos = getNumber(eltNumPage.value) - 1;
            oFileUpdater.addParam("bkmfilepos", nBkmFilePos, "post");
        }
        else {
            oFileUpdater.addParam("bkmfileid", getNumber(GetCurrentFileId(nBkm)), "post");
        }
    }

    if (!bDisplayAll) {
        nBkmRows = getBkmNbRows(nTab);
        oFileUpdater.addParam("rows", nBkmRows, "post");
    }

    // #92 393 - Passage du numéro de page à afficher si transmis
    if (typeof (nPage) == "undefined")
        var nPage = 1;
    oFileUpdater.addParam("bkmPage", getNumber(nPage), "post");

    oFileUpdater.send(function (oRes) { updateBkmCtner(oRes, nTab, nFileId, nBkm, bDisplayAll); });
}

function getBkmNbRows(nTab) {

    var paramWin = top.getParamWindow();
    var objThm = JSON.parse(paramWin.GetParam("currenttheme").toString());
    var nfileDivHeight = getNumber(document.getElementById("fileDiv_" + nGlobalActiveTab).offsetHeight);
    var nDivFilePart1 = getNumber(document.getElementById("divFilePart1").offsetHeight);
    var nBkmBarHeight = getNumber(document.getElementById("bkmBar_" + nTab).offsetHeight);
    var nBkmHeight = nfileDivHeight - nDivFilePart1 - nBkmBarHeight;

    //ELAIZ - demande 76404 - on met tous le temps 20 lignes sur le noyuveau thème car il y a un scroll sur le conteneur principal

    if (objThm.Version == 2)
        nBkmHeight = 20;

    nBkmHeight -= 15; // margin-top au-dessus du block des signets (blockBkms)
    nBkmHeight -= 15; // on enlève 15 pour le margin-top de bkmDiv
    nBkmHeight -= 7; // somme des border, padding-bottom sur bkmdiv
    nBkmHeight -= (23 + 5 + 2); // barre de titre du signet + padding top + border
    nBkmHeight -= 44; // entete des colonne qd on active les sommes sur une ou plusieurs colonnes
    nBkmHeight -= 20; // marge dans le cas où il y a un scrolling horizontal

    return GetNumRows(nBkmHeight);
}

///Met à jour le contenu HTML des bookmarks
function updateBkmCtner(oRes, nTab, nFileId, nBkm, bDisplayAll) {


    var paramWin = top.getParamWindow();
    var objThm = JSON.parse(paramWin.GetParam("currenttheme").toString());

    var oDivBkmCtner = document.getElementById("divBkmCtner");
    var divBkmPres = document.querySelector('#divBkmPres')
    if (!oDivBkmCtner) {
        eAlert(0, top._res_72, top._res_6573, top._res_6574);
        setWait(false);
        return;
    }

    swFileBkm(1);

    oDivBkmCtner.innerHTML += oRes;

    if (bDisplayAll)
        AddAllBkmBut(nTab, nFileId, nBkm);
    else {
        adjustScrollFile();
        initBkms();
        AjustBkmMemoToContainer();

        if (isBkmGrid(nBkm))
            showBkmGrid(nBkm);
    }

    var oParamGoTabList = {
        to: 3,
        nTab: nTab,
        context: "eFile.updateBkmCtner"
    }

    //Appel le waiter
    setWait(false, undefined, undefined, isIris(top.getTabFrom()), oParamGoTabList);

    //ELAIZ - demande 77681

    if (objThm.Version > 1 && divBkmPres.classList.contains('divBkmGrid')) {
        document.querySelector('.fileDiv').style.overflow = "hidden"
        document.querySelector('#divFilePart1').classList.add('grid-mode');
    } else {
        document.querySelector('.fileDiv').style.overflow = ""
        document.querySelector('#divFilePart1').classList.remove('grid-mode');
    }

    ///ELAIZ - demande 77681


}



/// Savoir si c'est un signet grille
function isBkmGrid(nBkm) {
    var bkm = document.getElementById("BkmHead_" + nBkm);
    return bkm && getAttributeValue(bkm, "edntype") == "24";
}

// Lance le chargement de la grille
function showBkmGrid(nBkm) {
    var divBkmPres = document.querySelector(".divBkmPres");

    divBkmPres.classList.add('divBkmGrid');
    AjustContentSize(nBkm);

    var fid = 0;
    if (typeof (top.GetCurrentFileId) == "function)")
        fid = top.GetCurrentFileId(top.nGlobalActiveTab);

    oGridManager.showBkmGrid(nBkm, top.nGlobalActiveTab, fid);
}

function hideBkmGrid() {
    var divBkmPres = document.querySelector(".divBkmPres");

    oGridManager.refreshVisibility(0);
    divBkmPres.classList.remove('divBkmGrid');
}

/// On masque les grilles affichées et on demande à charger le signet grille
function loadBkmGrid(nBkm) {
    oGridManager.refreshVisibility(0);
    loadBkmList(nBkm);
}

// Met à jour la taille du contenu de la grille
function AjustContentSize(nBkm) {
    var paramWin = top.getParamWindow();
    var objThm = JSON.parse(paramWin.GetParam("currenttheme").toString());
    var bkmDiv = eTools.getBookmarkContainer(nBkm);
    var bkmBar = document.querySelector(".bkmBar");
    var divBkmPres = document.getElementById("divBkmPres");

    //ELAIZ - demande 77681
    if (objThm.Version > 1 && bkmDiv && bkmBar && divBkmPres.classList.contains('divBkmGrid')) {
        bkmDiv.style.height = (document.querySelector("#mainDiv").offsetHeight - document.querySelector("#divFilePart1").offsetHeight - 135) + 'px';
    } else if (bkmDiv) {
        bkmDiv.style.height = (parseInt(divBkmPres.style.height) - 45) + "px";
    }

    //ELAIZ - demande 77681
}




/*
permet de charger tous les signets sauf un
*/
function AddAllBkmBut(nTab, nFileId, nBkm) {
    //setWait(true);

    var oFileUpdater = getFileAsyncUpd(nTab, nFileId, REQ_PART_BKMALLBUT);
    oFileUpdater.addParam("bkm", nBkm, "post");
    oFileUpdater.send(function (oRes) { updateMultiBkmCtner(oRes, nTab, nFileId); });

}


/*
permet de charger tous les signets sauf un
*/
function updateMultiBkmCtner(oRes, nTab, nFileId) {

    var oDivBkmCtner = document.getElementById("divBkmCtner");
    if (!oDivBkmCtner) {
        eAlert(0, top._res_72, top._res_6573, top._res_6574);
        //setWait(false);
        return;
    }

    swFileBkm(1);

    oDivBkmCtner.innerHTML += oRes;
    // les multisignets sont fournis dans un container dont il faut les sortir.
    var oDivTmp = document.getElementById("tmpctner");
    if (oDivTmp) {
        oDivTmp.outerHTML = oDivTmp.innerHTML;
    }


    adjustScrollFile();
    initBkms();
    chkExistsObligatFlds(null, null, null, window);

    //setWait(false);

}

function resetBkmCtner() {

    var oDivBkmCtner = document.getElementById("divBkmCtner");
    if (!oDivBkmCtner) {
        eAlert(0, top._res_72, top._res_6573, top._res_6574);
        return;
    }


    oDivBkmCtner.innerHTML = "";

}


/// Appel le manager pour actualiser le bloc complet des signets
///  "nBkm" représente le signet à mettre en focus
/// Utiliser pour afficher tous les signets ou changer de signet
//    nBkm = -1: afficher TOUS les signets
//    nBkm = 0 : afficher le 1er signet 
// bRldBkmBar : indique s'il faut rafraichir la barre des signet
function loadBkmList(nBkm, bRldBkmBar) {

    var paramWin = top.getParamWindow();
    var objThm = JSON.parse(paramWin.GetParam("currenttheme").toString());

    //ELAIZ - demande 77681

    if (objThm.Version > 1 && nBkm == '-1') {
        document.querySelector('.fileDiv').style.overflow = ""
        document.querySelector('#divFilePart1').classList.remove('grid-mode');

        for (let i = 0; i < document.querySelectorAll('.gw-grid').length; i++) {
            document.querySelectorAll('.gw-grid')[i].setAttribute('active', 0);
        }
    }

    //ELAIZ - demande 77681


    // #47 223 : Si un champ Mémo HTML (CKEditor) est marqué comme étant en cours d'édition (focus), on empêche le changement d'onglet
    // pour autoriser la sauvegarde de son contenu, jusqu'à ce que celle-ci soit effectuée (eMemoEditor.validate puis eMemoEditor.update)

    // si double validation est désactivé, on execute ce code  
    if (typeof (getPreventLoadBkmList) == "function") {
        if (getPreventLoadBkmList())
            return;
    }


    if (typeof (bRldBkmBar) == "undefined")
        var bRldBkmBar = false;

    //var nFileId = GetCurrentFileId(nGlobalActiveTab);
    var nFileId = getAttributeValue(document.getElementById("fileDiv_" + nGlobalActiveTab), "fid");


    if (bRldBkmBar) {
        loadBkmBar(nGlobalActiveTab, nFileId, false);
        return;
    }

    var nOldActiveBkm = -2;
    if (nBkm >= 0 || nBkm == -1) {
        swFileBkm(1);

        nOldActiveBkm = getActiveBkm(nGlobalActiveTab);
    }


    var nBkmToLoad = nBkm;
    var nActiveBkm = nBkm;

    if (nBkm == -1 || nBkm == 0) {


        nBkmToLoad = getFirstBkm(nGlobalActiveTab);

        if (nBkm == 0)
            nActiveBkm = nBkmToLoad;
    }

    setActiveBkm(nGlobalActiveTab, nActiveBkm);

    switchBkmSelection(nOldActiveBkm, nActiveBkm);

    if (nBkm == -1 || nBkm >= 0) {
        // #90 916 : sur le nouveau mode Fiche, on émet un signal pour le rafraîchissement du signet        
        if (top.isIris(nGlobalActiveTab)) {
            top.emitIrisLoadAll({
                reloadSignet: true,
                reloadHead: false,
                reloadAssistant: false,
                reloadAll: false,
            });
            return;
        }
        else {
            // #92 393 - Avant de rafraîchir le signet, on récupère le numéro de la page en cours pour pouvoir le passer à AddBkm() AVANT d'écraser le contenu (et donc de perdre l'information)
            var nCurrentBkmPage = 1;
            var eltBkmNumPage = document.getElementById("bkmNumPage_" + nActiveBkm);
            if (eltBkmNumPage)
                nCurrentBkmPage = getNumber(eltBkmNumPage.value);
            AddBkm(nGlobalActiveTab, nFileId, nBkmToLoad, nBkm == -1, nCurrentBkmPage);
            resetBkmCtner();
            return;
        }
    }
    else {
        resetBkmCtner();
    }
    setWait(true);

    var bfileTabInBkm = isFileTabInBkm();

    try {

        var oBkmUpdater = new eUpdater("mgr/eBookmarkManager.ashx", 1);
        oBkmUpdater.addParam("parenttab", nGlobalActiveTab, "post");
        oBkmUpdater.addParam("parentfileid", nFileId, "post");
        if (nBkm != null) {
            oBkmUpdater.addParam("bkm", nBkm, "post");
        }
        oBkmUpdater.addParam("type", 0, "post");
        oBkmUpdater.addParam("displayall", nBkm != -1 ? 0 : 1, "post");

        var fileWidth = GetFileWidth();
        if (fileWidth > 0)
            oBkmUpdater.addParam("width", fileWidth, "post");
        // KHA le 25022013 les deux lignes suivantes sont désormais inutiles car le reloade complet de la partie signet est géré de la même façon que dans le mode asynchrone.
        //oBkmUpdater.addParam("rldbkmbar", bRldBkmBar, "post");
        //oBkmUpdater.addParam("btabinbkm", bfileTabInBkm ? 1 : 0, "post");  
        oBkmUpdater.ErrorCallBack = function () { setWait(false); };
        oBkmUpdater.send(function (oRes) { updateBkmList(oRes, nBkm, bRldBkmBar); });

    }
    catch (ex) {
        setWait(false);
    }
}

/// Met en sélection le nouveau bkm et désactive l'ancien
function switchBkmSelection(oldBkm, newBkm) {
    switchBkmClass(oldBkm, "bkmHeadSel", "bkmHead");
    switchBkmClass(newBkm, "bkmHead", "bkmHeadSel");
}

// Active/désactive la css de sélection du BKM
// valeurs possible pour oldClass et newClass : "bkmHeadSel", "bkmHead"
function switchBkmClass(nBkm, oldClass, newClass) {
    var oActivBkmHead;
    if (nBkm > 0) {
        oActivBkmHead = document.getElementById("BkmHead_" + nBkm);
    }
    else if (nBkm == -1) {
        oActivBkmHead = document.getElementById("bkmAll");
    }

    switchClass(oActivBkmHead, oldClass, newClass);
}

/// Met à jour le bloc complet des signet suite à un appel via loadBkmList
function updateBkmList(oRes, nBkm, bRldBkmBar) {
    try {

        if (typeof (nBkm) == "undefined" || nBkm == null)
            nBkm = getActiveBkm(nGlobalActiveTab);


        var bDisplayAll = (nBkm == -1);

        var sDivName = "divBkmCtner";
        if (bRldBkmBar == 1)
            sDivName = "blockBkms";

        var fileTabInBkm = document.getElementById("FlDtlsBkm")
        var sFileTabInBkm = "";
        var bFileTabInBkm = isFileTabInBkm();

        if (fileTabInBkm) {
            sFileTabInBkm = fileTabInBkm.outerHTML;
        }
        if (!bRldBkmBar) {
            var nOldActiveBkm = getActiveBkm(nGlobalActiveTab);

            if (nOldActiveBkm > 0) {
                var oOldActivBkmHead = document.getElementById("BkmHead_" + nOldActiveBkm);
                switchClass(oOldActivBkmHead, "bkmHeadSel", "bkmHead");
            }
        }

        var oBkmDiv = document.getElementById(sDivName);
        if (typeof (oBkmDiv.outerHTML) == "undefined") {
            altOuterHTML(oBkmDiv, oRes);
        }
        else {
            oBkmDiv.outerHTML = oRes;
        }


        oBkmDiv = document.getElementById(sDivName);
        var oDivBkmBar = document.getElementById("bkmBar_" + nGlobalActiveTab);


        if (bRldBkmBar && bFileTabInBkm) {
            document.getElementById("divBkmPres").innerHTML += sFileTabInBkm;
        }

        oDivBkmBar.setAttribute("activebkm", nBkm);

        if (nBkm == 0 && bFileTabInBkm) {
            swFileBkm(0);
        }
        else {
            swFileBkm(1);
        }

        if (nBkm > 0 && !bRldBkmBar) {
            var oActivBkmHead = document.getElementById("BkmHead_" + nBkm);
            switchClass(oActivBkmHead, "bkmHeadSel", "bkmHead");
        }


        var obkmAll = document.getElementById("bkmAll");

        // réactualise l'objet oBkmDiv après la maj de son contenu
        oBkmDiv = document.getElementById(sDivName);

        if (!bDisplayAll) {

            oActivBkmHead = document.getElementById("BkmHead_" + nBkm);
            switchClass(oActivBkmHead, "bkmHead", "bkmHeadSel");
            switchClass(obkmAll, "bkmHeadSel", "bkmHead");

        }
        else {

            switchClass(obkmAll, "bkmHead", "bkmHeadSel");

        }

        adjustScrollFile();

        initBkms();

        AjustBkmMemoToContainer();

        if (isBkmGrid(nBkm))
            showBkmGrid(nBkm);


    }
    finally {
        setWait(false);
    }
}

// affiche le signet contenant le detail de la fiche
// masque la div contenant les autres signets.
// i : 0 pour afficher le detail de la fiche
// i : 1 pour afficher le bkm
function swFileBkm(i) {
    var divBkmBar = document.getElementById("bkmBar_" + nGlobalActiveTab);
    var divBkmContent = document.getElementById("divBkmCtner");
    var divFlDtlsInBkm = document.getElementById("FlDtlsBkm");
    var divBkmDtlsHd = document.getElementById("bkmDtls");
    var divBkmPres = document.querySelector(".divBkmPres");
    var oImg;
    if (divBkmDtlsHd) {
        oImg = divBkmDtlsHd.getElementsByTagName("img");
        if (oImg && oImg.length)
            oImg = oImg[0];
    }
    if (!divFlDtlsInBkm) {
        return;
    }

    //demande 81 384
    divBkmPres.classList.remove('bkmBarAll');

    if (i == 0) {

        oEvent.fire("file-detail-display");

        if (divBkmContent)
            divBkmContent.style.display = "none";

        if (divFlDtlsInBkm)
            divFlDtlsInBkm.style.display = "";

        var nOldActiveBkm = getActiveBkm(nGlobalActiveTab);
        if (nOldActiveBkm > 0) {
            var divOldActivBkmHead = document.getElementById("BkmHead_" + nOldActiveBkm);
            switchClass(divOldActivBkmHead, "bkmHeadSel", "bkmHead");
        }
        //switchClass(divBkmDtlsHd, "bkmHead", "bkmHeadSel");
        if (divBkmDtlsHd)
            switchClass(divBkmDtlsHd, "bkmDtls", "bkmDtlsSel");

        if (oImg)
            switchClass(oImg, "bkmDtlsImg", "bkmDtlsImgSel");

        divBkmBar.setAttribute("activebkm", "0");

        var obkmAll = document.getElementById("bkmAll");
        switchClass(obkmAll, "bkmHeadSel", "bkmHead");


        //demande 81 384
        if (document.querySelector('.bkmBar').getAttribute('activebkm') == 0)
            divBkmPres.classList.add('bkmBarAll');

        setActiveBkm(nGlobalActiveTab, 0);



        //BSE:#57 304
        Array.prototype.slice.apply(top.document.querySelectorAll("div[class='StatsRendBlock']")).forEach(
            function (arrElem) {
                resizeSyncFusionChart(arrElem);
            }
        );

    }
    else {
        if (divBkmContent)
            divBkmContent.style.display = "";

        if (divFlDtlsInBkm)
            divFlDtlsInBkm.style.display = "none";
        //switchClass(divBkmDtlsHd, "bkmHeadSel", "bkmHead");

        if (divBkmDtlsHd)
            switchClass(divBkmDtlsHd, "bkmDtlsSel", "bkmDtls");
        if (oImg)
            switchClass(oImg, "bkmDtlsImgSel", "bkmDtlsImg");

    }

}


/// Appel le manager pour actualiser le signet "nBkm" à la page "nPg"
///  utilisé uniquement pour actualiser un signet déjà affiché.
//   nBkm : signet à afficher
//   nPg  : page à afficher
//   bResizeCol : reajuster les colonnes après avoir rafraichi les données (true qd modification de la selection des colonnes.)
//	 bDisplayAllRecord : afficher tous les enregistrements en ignorant le paramétrage de paging du signet (bouton Afficher tout)
//   nBkmFileId : Mode fiche en signet : id de la fiche à afficher
function loadBkm(nBkm, nPg, bResizeCol, bDisplayAllRecord, nBkmFileId, nBkmFilePos) {

    var oDoc = document;
    if (typeof (nPg) == "undefined")
        var nPg = 1;

    if (typeof (bResizeCol) == "undefined")
        var bResizeCol = false;

    var nFileId = GetCurrentFileId(nGlobalActiveTab);
    var bDisplayAll = getActiveBkm(nGlobalActiveTab) == "-1";

    if (typeof (bDisplayAllRecord) == "undefined")
        var bDisplayAllRecord = false;
    //NHA #85 108 : régression Ajout annexe/ Ajout annexe doublon on charge le context dans oDoc
    var oBkmDiv = eTools.getBookmarkContainer(nBkm, oDoc);
    if (oBkmDiv == null || typeof (oBkmDiv) == 'undefined')
        return;

    var oTab = document.getElementById("mt_" + nBkm);
    //Dans le nouveau mode fiche l'attribue bkm_Rows n'existe pas on le charge préalablement pour tester ça nullité avant d'appeler tostring
    var nbRows = oBkmDiv.getAttribute("bkm_Rows_" + nBkm);
    //BSE #50 364 , Récuperer le nombre de page à afficher pour le signet en mode TOUS
    if (nbRows != null)
        nbRows = oBkmDiv.getAttribute("bkm_Rows_" + nBkm).toString(); //getBkmNbRows(nGlobalActiveTab);



    var oBkmUpdater = new eUpdater("mgr/eBookmarkManager.ashx", 1);
    oBkmUpdater.addParam("parenttab", nGlobalActiveTab, "post");
    oBkmUpdater.addParam("parentfileid", nFileId, "post");
    oBkmUpdater.addParam("bkm", nBkm, "post");
    oBkmUpdater.addParam("page", nPg, "post");
    if (nbRows > 0)
        oBkmUpdater.addParam("rows", nbRows, "post");

    if (nBkmFileId > 0)
        oBkmUpdater.addParam("bkmfileid", nBkmFileId, "post");
    if (nBkmFilePos > 0)
        oBkmUpdater.addParam("bkmfilepos", nBkmFilePos, "post");


    oBkmUpdater.addParam("type", 1, "post");
    oBkmUpdater.addParam("displayall", bDisplayAll ? 1 : 0, "post");
    oBkmUpdater.addParam("displayallrecord", bDisplayAllRecord ? 1 : 0, "post");

    setWait(true);
    oBkmUpdater.ErrorCallBack = function () { setWait(false); }
    oBkmUpdater.send(function (oRes) { updateBkm(oRes, nBkm, nPg, bResizeCol); setWait(false); });

}

/// Met à jour le HTML pour le signet nBkm suite à l'appel loadBkm
function updateBkm(oRes, nBkm, nPg, bResizeCol) {


    var oBkmDiv = eTools.getBookmarkContainer(nBkm);
    if (oBkmDiv) {
        if (!oBkmDiv.outerHTML)
            altOuterHTML(oBkmDiv, oRes);
        else
            oBkmDiv.outerHTML = oRes;
    }

    if (isBkmFile(nBkm)) {
        updateFile(null, nBkm, getBkmCurrentFileId(nBkm), 3, false, null, null, false, LOADFILEFROM.BKMFILE);
    }
    else if (isBkmDisc(null, oBkmDiv)) {
        nBkm = getNumber(nBkm);
        var nCommDid = getNumber(getAttributeValue(oBkmDiv, "did"));
        if (document.getElementById('eBkmMemoEditorContainer_' + (nCommDid))) {
            loadMemoBkm(nCommDid);
            aBkmMemoEditors[nCommDid].fileId = 0;
            aBkmMemoEditors[nCommDid].disc = true;
            aBkmMemoEditors[nCommDid].dtdid = getNumber(getAttributeValue(oBkmDiv, "dtdid"));
            aBkmMemoEditors[nCommDid].usrdid = getNumber(getAttributeValue(oBkmDiv, "usrdid"));
        }
    }
    else {
        listLoaded(nBkm, 1);

        //Iso v7 : les colonnes calculé ne sont pas recalculé au paging/filtre express...
        //updateComputedCol(nBkm);

        if (bResizeCol)
            autoResizeColumns(nBkm);
        else
            adjustLastCol(nBkm);

        ScrollIntoLastPosition(nBkm);
    }
}


// Charge le champ Mémo/Notes sur un signet Notes
// Les instances créées sont conservées dans un tableau d'objets

var aBkmMemoEditors = [];
function loadMemoBkm(nBkm) {
    var bHTML = (getAttributeValue(document.getElementById('eBkmMemoEditorContainer_' + nBkm), 'html') == '1');
    var bCompactMode = false;
    var oBkmHead = document.getElementById("BkmHead_" + nBkm);

    // Création et instanciation du champ Mémo, ou rappel de l'instance existante
    aBkmMemoEditors[nBkm] = new eMemoEditor(
        'eBkmMemoEditor_' + nBkm,
        bHTML,
        document.getElementById('eBkmMemoEditorContainer_' + nBkm),
        null,
        GetText(document.getElementById('eBkmMemoEditorValue_' + nBkm)),
        bCompactMode,
        "aBkmMemoEditors[" + nBkm + "]"
    );
    if (oBkmHead) {

        if (oBkmHead.lastChild != null && oBkmHead.lastChild.nodeName.toLowerCase() == "a") {
            aBkmMemoEditors[nBkm].title = oBkmHead.lastChild.innerHTML;
        }
        else
            aBkmMemoEditors[nBkm].title = oBkmHead.innerHTML;


    }

    // Demande #27 563 - On précise le DescID et le FileID du champ Mémo associé pour la mise à jour de la valeur en base
    // Car dans le cas où l'utilisateur change de signet, le curseur sort du champ et donc, la valeur du champ Mémo doit être sauvegardée.
    // Seulement, le changement de signet entraîne un rechargement de la page ! Ce qui empêche le moteur de mise à jour (eMemoEditor.update)
    // de retrouver ces infos sur le contexte de la page et fait échouer la mise à jour
    aBkmMemoEditors[nBkm].descId = nBkm;
    aBkmMemoEditors[nBkm].fileId = getAttributeValue(document.getElementById("fileDiv_" + nGlobalActiveTab), "fid");

    // Mise à jour de la configuration de base du champ Mémo (mode HTML) avec les propriétés ci-dessus
    if (bHTML) {
        aBkmMemoEditors[nBkm].setSkin('eudonet');
        aBkmMemoEditors[nBkm].setToolBarDisplay(true, true);
        if (
            aBkmMemoEditors[nBkm].toolbarType == "mail" ||
            aBkmMemoEditors[nBkm].toolbarType == "mailing" ||
            aBkmMemoEditors[nBkm].toolbarType == "mailtemplate" ||
            aBkmMemoEditors[nBkm].toolbarType == "mailingtemplate" ||
            aBkmMemoEditors[nBkm].toolbarType == "formular"
        ) {
            aBkmMemoEditors[nBkm].setStatusBarEnabled(true);
        }
    }

    var isBkmActive = getActiveBkm(nGlobalActiveTab) == nBkm;

    // On prend toute la heauteur si champ memo/notes est actif
    // On prend 222px si signet tous
    var ctn = eTools.getBookmarkContainer(nBkm);
    if (ctn) {
        ctn.style.height = isBkmActive ? "99%" : "222px";

        var BkmMemo = document.getElementById('eBkmMemoEditorContainer_' + nBkm);
        BkmMemo.style.position = "relative";
        //BkmMemo.style.left = "-1px";
        BkmMemo.style.height = "100%";
        BkmMemo.style.width = "100%";


        aBkmMemoEditors[nBkm].config.height = (getActiveBkm(nGlobalActiveTab) == nBkm) ? "100%" : "162px";
        aBkmMemoEditors[nBkm].config.width = '100%';
    }
    else {
        aBkmMemoEditors[nBkm].config.width = '99%';
    }
    aBkmMemoEditors[nBkm].inlineMode = false;

    // Mise à jour de la configuration commune (HTML ET texte)
    aBkmMemoEditors[nBkm].updateOnBlur = true; // en mode signet, comme en mode modification, on sauvegarde le contenu du champ lorsqu'on en sort

    // ne pas mettre de focus sur l'ouverture en signet
    aBkmMemoEditors[nBkm].focusOnShow = false;

    // Mode lecture seule ou écriture
    aBkmMemoEditors[nBkm].readOnly = (getAttributeValue(document.getElementById('eBkmMemoEditorContainer_' + nBkm), 'ero') == '1');

    aBkmMemoEditors[nBkm].frominnertext = (getAttributeValue(document.getElementById('eBkmMemoEditorContainer_' + nBkm), 'frominnertext') == '1');

    aBkmMemoEditors[nBkm].nbRows = (getAttributeValue(document.getElementById('eBkmMemoEditorContainer_' + nBkm), "nbrows"));

    // Création du <textarea> dans le container passé à l'initialisation de eMemoEditor et affichage
    aBkmMemoEditors[nBkm].show();
}

/// Adapte la taille du champ mémo/notes à la taille du conteneur dans le signet
function AjustBkmMemoToContainer(nTab) {
    // Pas de mémo 
    if (aBkmMemoEditors.length == 0)
        return;

    if (typeof (nTab) == "undefined")
        nTab = nGlobalActiveTab;

    // Signet actuellement affiché
    var activeBkm = getActiveBkm(nTab);

    // Signet  Détail - rien à faire
    if (activeBkm == 0)
        return;


    // Taille des mémos et notes dans le signet TOUS
    var size = {};
    if (activeBkm == -1) {

        var divBkmCtner = document.getElementById("divBkmCtner");
        if (divBkmCtner == null)
            return;

        // Mise à jour de la taille du chaque mémo/notes dans le signet TOUS
        for (var i in aBkmMemoEditors) {
            var bkmCtn = divBkmCtner.querySelector('#bkm_' + aBkmMemoEditors[i].descId);
            if (bkmCtn == null || getAttributeValue(bkmCtn, "memobkm") != "1")
                continue;

            var memo = document.getElementById('eBkmMemoEditorContainer_' + aBkmMemoEditors[i].descId);
            memo.style.width = "100%";

            size.w = bkmCtn.offsetWidth - 2;
            size.h = bkmCtn.offsetHeight - 40;

            aBkmMemoEditors[i].resize(size.w, size.h);
        }
    }


    //Pas de conteneur pn quitte du memo
    var ctn = eTools.getBookmarkContainer(activeBkm);
    if (ctn == null || getAttributeValue(ctn, "memobkm") != "1")
        return;

    var BkmMemo = document.getElementById('eBkmMemoEditorContainer_' + activeBkm);
    BkmMemo.style.width = "100%";

    // Conteneur du signet memo/notes affiché
    size.w = ctn.offsetWidth - 5;
    size.h = ctn.offsetHeight - 27;

    // Mise à jour de la taille du mémo
    for (var i in aBkmMemoEditors) {
        if (aBkmMemoEditors[i].descId == activeBkm) {
            aBkmMemoEditors[i].resize(size.w, size.h);
            break;
        }
    }
}


var modalListBkm;
function setBkmOrder(nTab) {

    modalListBkm = new eModalDialog(top._res_366, 0, "eSelectBkm.aspx", 850, 520);
    modalListBkm.bBtnAdvanced = true;

    modalListBkm.addParam("tab", nTab, "post");
    modalListBkm.ErrorCallBack = launchInContext(modalListBkm, modalListBkm.hide);
    modalListBkm.show();

    //modalListBkm.addButtonFct(top._res_29, function () { modalListBkm.hide(); }, "button-gray");
    modalListBkm.addButtonFct(top._res_29, launchInContext(modalListBkm, modalListBkm.hide), "button-gray");
    modalListBkm.addButtonFct(top._res_28, function () { onSetBkmOk(nTab); }, "button-green");

}


function onSetBkmOk(nTab) {

    var oDoc = modalListBkm.getIframe().document;
    var _itemsUsed = oDoc.getElementById("TabSelectedList").getElementsByTagName("div");

    var strListCol = "";
    for (var i = 0; i < _itemsUsed.length; i++) {

        var nDescId = getNumber(_itemsUsed[i].getAttribute("DescId"));
        if (nDescId <= 0 || isNaN(nDescId))
            continue;

        if (strListCol != "")
            strListCol = strListCol + ";";
        strListCol = strListCol + nDescId;
    }


    var updatePref = "tab=" + nTab + ";$;bkmorder=" + strListCol;
    updateUserPref(updatePref, function () { loadBkmList(null, 1); });

    modalListBkm.hide();

}

function setActiveBkmPref(nTab, nBkm) {
    var updatePref = "tab=" + nTab + ";$;activebkm=" + nBkm;
    updateUserPref(updatePref);

}

function getActiveBkm(nTab) {
    return getAttributeValue(document.getElementById("bkmBar_" + nTab), "activebkm");
}

function setActiveBkm(nTab, nActiveBkm) {
    setActiveBkmPref(nTab, nActiveBkm);
    // #90 916 - Sur le nouveau mode Fiche, il n'y a plus de barre de signets à mettre à jour (pour l'instant...)
    if (!top.isIris(nTab)) {
        var oBkmBar = document.getElementById("bkmBar_" + nTab);
        oBkmBar.setAttribute("activebkm", nActiveBkm);
    }
    //On met à jours eParam
    var oeParam = top.getParamWindow();
    oeParam.SetParam('ActiveBkm_' + nTab, nActiveBkm);
}

var modalBkmCol;
function importTargets(tabFom, tabFromFileId, tplTab, onHideFunc) {
    modalBkmCol = new eModalDialog(top._res_6713, 0, "eTargetImportWizard.aspx", 1020, 600);
    modalBkmCol.addParam("tab", tplTab, "post");
    modalBkmCol.addParam("tabfrom", nGlobalActiveTab, "post");
    modalBkmCol.addParam("evtid", tabFromFileId, "post");
    modalBkmCol.nTab = tplTab;
    modalBkmCol.ErrorCallBack = function () { };
    //BSE: Bug #79 427: recharger la liste après import signet ++ 
    if (typeof onHideFunc == 'function')
        modalBkmCol.onHideFunction = onHideFunc;

    modalBkmCol.show();
    modalBkmCol.addButton(top._res_26, onImportTargetsNext, "button-green-rightarrow", tplTab, "ImportTargetsNext");
    modalBkmCol.addButton(top._res_25, onImportTargetsPrevious, "button-gray-leftarrow", tplTab, "ImportTargetsPrev");

}
function onImportTargetsPrevious() { modalBkmCol.getIframe().doPrevious(modalBkmCol.nTab); }
function onImportTargetsNext() { modalBkmCol.getIframe().doNext(modalBkmCol.nTab); }




function setBkmCol(nBkmTab) {

    modalBkmCol = new eModalDialog(top._res_96, 0, "eFieldsSelect.aspx", 850, 550);
    modalBkmCol.ErrorCallBack = function () { setWait(false); }

    modalBkmCol.addParam("tab", nBkmTab, "post");
    modalBkmCol.addParam("parenttab", nGlobalActiveTab, "post");
    modalBkmCol.addParam("action", "initbkm", "post");

    modalBkmCol.bBtnAdvanced = true;
    modalBkmCol.show();
    modalBkmCol.addButton(top._res_29, onSetBkmColAbort, "button-gray", nBkmTab);
    modalBkmCol.addButton(top._res_28, onSetBkmColOk, "button-green", nBkmTab);

}

function onSetBkmColOk(nTab, popupId) {

    var _frm = window.frames["frm_" + popupId];
    var strBkmCol = _frm.getSelectedDescId();

    //Récupération du strBkmCol
    _frm = document.getElementById("frm_" + popupId);
    var _oDoc = _frm.contentWindow.document || _frm.contentDocument;
    var cbo = _oDoc.getElementById("AllSelections");

    var updatePref = "tab=" + nGlobalActiveTab
        + ";$;bkmcol=" + strBkmCol
        + ";$;bkm=" + nTab;

    updateUserBkmPref(updatePref, function () { loadBkm(nTab, 1); });

    modalBkmCol.hide();

}


function onChngBkmHisto(nTab) {
    var updatePref = "tab=" + nGlobalActiveTab + ";$;bkmhisto" + ";$;bkm=" + nTab;

    // HLA - Il ne faut pas ecraser les pref de l'utilisateur avec des valeurs auto-resize
    updateUserBkmPref(updatePref, function () { loadBkm(nTab, 1); });
}

function onBreak(nTab, status) {

    try {

        var url = "mgr/eOnBreakCampaignMgr.ashx";
        var eUp = new eUpdater(url, 1);

        eUp.addParam("CampaignDescId", nTab, "post");
        eUp.addParam("Status", status, "post");

        eUp.ErrorCallBack = function () { };
        eUp.send(function (oRes) { afterOnBreakCampaign(oRes, status); });

    }
    catch (e) {
        eAlert(0, top._res_2712, top._res_2715, e.ErrorMsg, 500, 200);
    }

}

///Met en pause ou réactive toutes les étapes marketting récurrentes
// annule les campagnes différées
/// Status : 1 pour  mettre en pause - 0 : pour réactiver
function onBreakEvent(nParentTabId, nParentFileId, status) {
    try {


        var fct = function () {
            top.setWait(true)
            var url = "mgr/eOnBreakEventStepMgr.ashx";
            var eUp = new eUpdater(url, 1);


            eUp.addParam("ParentTabId", nParentTabId, "post");
            eUp.addParam("ParentFileId", nParentFileId, "post");
            eUp.addParam("Status", status, "post");

            eUp.json = {
                Status: status,
                ParentTabId: nParentTabId,
                ParentFileId: nParentFileId
            }

            eUp.ErrorCallBack = function () { top.setWait(false) };
            eUp.send(function (oRes) { top.setWait(false); afterOnBreakCampaign(oRes, status, nParentTabId); });
        }

        var hvFromTab = document.getElementById("fileName_" + nParentTabId + "_" + (nParentTabId + 1));
        if (hvFromTab)
            var sTabName = getAttributeValue(hvFromTab, "value")
        else
            var sTabName = ""


        var z = eConfirm(1,
            top._res_645, // titre
            top._res_719, // message
            status == 1 ? top._res_2764.replace("<FILENAME>", sTabName) : top._res_2763.replace("<FILENAME>", sTabName), // détail
            null, // width
            null, //height
            fct);  // fctOK

        z.adjustModalToContent(100);


    }
    catch (e) {
        eAlert(0, top._res_2712, top._res_2715, e.ErrorMsg, 500, 200);
    }
}

function afterOnBreakCampaign(res, status, nParentTabId) {
    try {




        var result = JSON.parse(res);
        if (result.Success) {
            var al = eAlert(4, top._res_2712, status == 1 ? top._res_2716 : top._res_2765, '');
            al.adjustModalToContent(0);

            var iconBase = "";
            var iconNew = "";
            var onClickNewValue = "";

            if (status == 1) {
                iconBase = "icon-pause";
                iconNew = "icon-play-circle-o";
                onClickNewValue = "0";

            } else {
                iconBase = "icon-play-circle-o";
                iconNew = "icon-pause";
                onClickNewValue = "1";
            }
            var principalButton = document.getElementsByClassName(iconBase)[0];
            replaceClass(principalButton, iconBase, iconNew);
            setOnClickAttrForOnBreakCampaign(principalButton, onClickNewValue);
        }
        else {
            eAlert(0, top._res_2712, top._res_2715, result.ErrorMsg, 500, 200);
        }

        // on actualise le signet pour ajuster la pagination
        var nActiveBkm = getActiveBkm(nParentTabId);
        if (!bInit && nActiveBkm > 0 && !isPopup())
            loadBkmList(nActiveBkm);

        adjustScrollFile();
    }
    catch (e) {
        eAlert(0, top._res_2712, top._res_2715, e, 500, 200);
    }
}

function replaceClass(element, oldClass, newClass) {
    removeClass(element, oldClass);
    addClass(element, newClass);
    return element;
}

function setOnClickAttrForOnBreakCampaign(element, setValue) {
    var onClickAttr = getAttributeValue(element, "onclick");
    onClickAttr = onClickAttr.split(",");
    onClickAttr[1] = setValue + ");";
    onClickAttr = onClickAttr.join(",");
    setAttributeValue(element, "onclick", onClickAttr);
    return element;
}

function onSetBkmColAbort(v1, popupId) {
    modalBkmCol.hide();
}

function firstPageBkm(bkmTab) {
    loadBkm(bkmTab, 1);
}

function prevPageBkm(bkmTab) {
    var oTable = document.getElementById("mt_" + bkmTab);
    var cPage = Number(oTable.getAttribute("cPage"));
    var bof = Number(oTable.getAttribute("bof"));

    if (bof != "1")
        cPage--
    else
        return;

    loadBkm(bkmTab, cPage);
}

function nextPageBkm(bkmTab) {

    var oTable = document.getElementById("mt_" + bkmTab);
    var cPage = Number(oTable.getAttribute("cPage"));
    var eof = Number(oTable.getAttribute("eof"));

    if (eof != "1")
        cPage++
    else
        return;

    loadBkm(bkmTab, cPage);
}

function lastPageBkm(bkmTab) {
    var oTable = document.getElementById("mt_" + bkmTab);
    var nbPage = Number(oTable.getAttribute("nbPage"));

    loadBkm(bkmTab, nbPage);
}

///Sélection de page de signet
function selectBkmPage(bkmTab, oNewPage) {

    var oTable = document.getElementById("mt_" + bkmTab);
    var newPage = Number(oNewPage.value);
    var cPage = Number(oTable.getAttribute("cPage"));
    var nbpage = Number(oTable.getAttribute("nbPage"));

    //changement de page seulement si la page demandée n'est pas la page actuellement affichée, page demandée > 1 et page demandée < au nombre de page de la liste en cours
    if ((newPage == cPage) || (newPage < 1) || (newPage > nbpage) || !isNumeric(newPage)) {
        oNewPage.value = cPage;
        return;
    }

    loadBkm(bkmTab, newPage);
}

function switchActivePageBkm(nBkmPage) {
    var oBkmTr = document.getElementById("bkmtr");
    var oBkmTds = oBkmTr.getElementsByTagName("td");

    if (window.navigator.userAgent.search('MSIE 9.0') != -1) {
        oBkmTr.parentNode.style.borderCollapse = 'separate';
    }

    for (var k = 0; k < oBkmTds.length; k++) {
        var oBkmTd = oBkmTds[k];

        // pour les td correspondants à TOUS + et la celule vide de fin de ligne on ne masque pas lors du changement de page
        if (oBkmTd.getAttribute("ednbkmpage") == 0) {
            continue;
        }
        else {
            if (oBkmTd.getAttribute("ednbkmpage") == nBkmPage) {
                oBkmTd.style.display = "";
            }
            else {

                oBkmTd.style.display = "none";

            }
        }
    }

    if (window.navigator.userAgent.search('MSIE 9.0') != -1) {
        oBkmTr.parentNode.style.borderCollapse = 'collapse';
    }

    var oPagBtns = document.getElementById("divBkmPaging").getElementsByTagName("SPAN");
    for (var k = 0; k < oPagBtns.length; k++) {
        switchClass(oPagBtns[k], "icon-circle", "icon-circle-thin");
        switchClass(oPagBtns[k], "imgAct", "imgInact");
    }

    var bullet = document.getElementById("swBkmPg" + nBkmPage);
    switchClass(bullet, "icon-circle-thin", "icon-circle");
    switchClass(bullet, "imgInact", "imgAct");
}
// permet d'afficher une autre Vcard
function switchVCard(nPage, nFileid) {
    var updVcard = new eUpdater("mgr/eFileManager.ashx", 1);
    updVcard.addParam("tab", 200, "post");
    updVcard.addParam("pg", nPage, "post");
    updVcard.addParam("fileid", nFileid, "post");
    updVcard.addParam("type", 4, "post"); // 4 correspond à eConst.eFileType.FILE_VCARD
    updVcard.addParam("bCroix", getAttributeValue(document.getElementById("Cx"), "c"), "post"); // 4 correspond à eConst.eFileType.FILE_VCARD
    updVcard.ErrorCallBack = function () { };
    updVcard.send(function (oRes) { updateVCard(oRes); });

}

function updateVCard(oRes) {
    document.getElementById('vcMain').innerHTML = oRes;
}

function showHideHeadInfo() {
    var tbHeadInfo = document.getElementById("ftp_" + nGlobalActiveTab);
    var imgCtrl = document.getElementById("imgHideHeaderInfos");

    if (tbHeadInfo.getAttribute("shown") == "0") {
        for (i = 1; i < tbHeadInfo.rows.length; i++)
            tbHeadInfo.rows[i].style.display = "";

        tbHeadInfo.setAttribute("shown", "1");
        switchClass(imgCtrl, "hiddenHead", "shownHead");
    }
    else {
        for (i = 1; i < tbHeadInfo.rows.length; i++)
            tbHeadInfo.rows[i].style.display = "none";

        tbHeadInfo.setAttribute("shown", "0");
        switchClass(imgCtrl, "shownHead", "hiddenHead");
    }


    //Ajuste la taille du div pour la scrollbar
    adjustScrollFile()

}


//GCH - #36012 - Internationnalisation - Planning - ok
function setConflictInfos(afterGetConflict, nTab, fileId, nRow) {

    // Table
    if (!nTab)
        nTab = Number(nGlobalActiveTab);

    // File id
    if (!fileId) {
        var fileDiv = document.getElementById("fileDiv_" + nTab);
        if (fileDiv != null)
            fileId = fileDiv.getAttribute("fid");
    }

    // Ligne
    if (!nRow) {
        nRow = "0";
    }

    // 99 : Appartient à
    var descIdOwner = nTab + 99;
    var ownerId = "COL_" + nTab + "_" + descIdOwner + "_" + fileId + "_" + fileId + "_" + nRow;

    var elementOwner = document.getElementById(ownerId);

    if (elementOwner == null) {
        top.setWait(false);
        return;
    }

    var owner = elementOwner.getAttribute("dbv");
    if (owner == null) {
        top.setWait(false);
        return;
    }

    // 92 : A faire par
    var scheduleId = 0;
    var descIdMultiOwner = nTab + 92;
    var multiOwnerId = nTab + "_" + descIdMultiOwner;
    var multiOwner = "";
    var aUsers = new Array();
    var aUsersField = document.getElementById("COL_" + nTab + "_" + descIdMultiOwner + "_" + fileId + "_" + fileId + "_" + nRow);
    if (aUsersField && aUsersField.childNodes && aUsersField.childNodes.length > 0 && aUsersField.childNodes[0].getElementsByTagName) {
        aUsers = aUsersField.getElementsByTagName("span");
        for (var i = 0; i < aUsers.length; i++) {
            multiOwner += ";" + aUsers[i].getAttribute("dbv");
        }
    }
    else {
        multiOwner += getAttributeValue(aUsersField, "dbv");
    }

    // 82 : Date et heure de début 
    var descIdSchedule = nTab + 82;
    if (document.getElementById("COL_" + nTab + "_" + descIdSchedule + "_" + fileId + "_" + fileId + "_" + nRow) != null)
        scheduleId = document.getElementById("COL_" + nTab + "_" + descIdSchedule + "_" + fileId + "_" + fileId + "_" + nRow).value;

    if (document.getElementById("BeginDateDescId") != null)
        var descIdBeginDate = document.getElementById("BeginDateDescId").value;

    var beginDateId = "COL_" + nTab + "_" + descIdBeginDate + "_D_" + fileId + "_" + fileId + "_" + nRow;
    var beginHourId = "COL_" + nTab + "_" + descIdBeginDate + "_H_" + fileId + "_" + fileId + "_" + nRow;

    if (document.getElementById(beginDateId) == null) {
        top.setWait(false);
        return;
    }

    var beginDate = eDate.ConvertDisplayToBdd(document.getElementById(beginDateId).value + " " + document.getElementById(beginHourId).value);

    // Date et heure de fin
    var descIdEndDate = nTab + endDateDescIdTpl;
    var endDateId = "COL_" + nTab + "_" + descIdEndDate + "_D_" + fileId + "_" + fileId + "_" + nRow;
    var endHourId = "COL_" + nTab + "_" + descIdEndDate + "_H_" + fileId + "_" + fileId + "_" + nRow;

    if (document.getElementById(endDateId) == null) {
        top.setWait(false);
        return;
    }
    var endDate = eDate.ConvertDisplayToBdd(document.getElementById(endDateId).value + " " + document.getElementById(endHourId).value);

    // Type planning
    var descIdType = nTab + TYPE_PLANNING;
    var nTypePlanning = -1;
    if (document.getElementById("COL_" + nTab + "_" + descIdType + "_" + fileId + "_" + fileId + "_" + nRow) != null)
        nTypePlanning = document.getElementById("COL_" + nTab + "_" + descIdType + "_" + fileId + "_" + fileId + "_" + nRow).value;

    document.isTplLoading = true;

    var upd = new eUpdater("mgr/ePlanningManager.ashx", 0);
    upd.ErrorCallBack = function () { };

    upd.addParam("tab", nTab, "post");
    upd.addParam("fileid", fileId, "post");
    upd.addParam("scheduleid", scheduleId, "post");
    upd.addParam("begindate", beginDate, "post");
    upd.addParam("enddate", endDate, "post");
    upd.addParam("owner", owner, "post");
    upd.addParam("multiowner", multiOwner, "post");
    upd.addParam("action", "conflict", "post");
    upd.addParam("begindatedescid", document.getElementById('BeginDateDescId').value, "post");
    upd.addParam("typeplanning", nTypePlanning, "post");


    upd.send(function (oRes) { setConflictInfosTreatment(oRes, afterGetConflict, false, fileId); });
}

function setConflictInfosInModeList(afterGetConflict, nTab, nDescid, fileId, newValue, elementId) {
    document.isTplLoading = true;

    var upd = new eUpdater("mgr/ePlanningManager.ashx", 0);
    upd.ErrorCallBack = function () { };

    // 99 : Appartient à (si la colonne est présente)
    //var descIdOwner = nTab + 99;
    //var elementOwner = document.getElementById("COL_" + nTab + "_" + descIdOwner + "_" + fileId + "_" + fileId + "_" + nRow);
    //if (elementOwner != null) {
    //}
    var changedParam = "";
    if (nDescid == nTab + 99) {
        changedParam = "owner";
    }
    else if (nDescid == nTab + 92) {
        changedParam = "multiowner";
    }
    else if (nDescid == nTab + 02) {
        changedParam = "begindate";
        newValue = eDate.ConvertDisplayToBdd(newValue);
    }
    else if (nDescid == nTab + 89) {
        changedParam = "enddate";
        newValue = eDate.ConvertDisplayToBdd(newValue);
    }

    if (changedParam != "")
        upd.addParam(changedParam, newValue, "post");
    upd.addParam("tab", nTab, "post");
    upd.addParam("fileid", fileId, "post");
    upd.addParam("action", "conflict", "post");
    upd.addParam("modelist", "1", "post");

    upd.send(function (oRes) { setConflictInfosTreatment(oRes, afterGetConflict, true, fileId, elementId); });
}


function setConflictInfosTreatment(oRes, afterGetConflict, modeList, nFileId, elementId) {

    try {

        var tabReturn = document.createElement("table");
        tabReturn.className = "conflictTable";

        var trHeader = document.createElement("tr");
        trHeader.className = "hdBgCol";
        tabReturn.appendChild(trHeader);

        var aFields = oRes.getElementsByTagName("header")[0].getElementsByTagName("field");
        if (!nFileId)
            nFileId = getAttributeValue(document.getElementById("fileDiv"), "fid")

        for (var i = 0; i < aFields.length; i++) {
            var td = document.createElement("td");
            td.innerHTML = getXmlTextNode(aFields[i]);
            td.className = "hdName";
            trHeader.appendChild(td);
        }

        var aRecords = oRes.getElementsByTagName("datas")[0].getElementsByTagName("record");

        var bConflicted = false;

        // #42356 CRU : On peut venir du planning ou du mode liste en signet
        if (!modeList) {
            var cssCell = "cell";
            var cssLine = "line1";

            for (var i = 0; i < aRecords.length; i++) {
                var trDatas = document.createElement("tr");
                tabReturn.appendChild(trDatas);
                if (cssLine == "line1")
                    cssLine = "line2";
                else
                    cssLine = "line1";
                trDatas.className = cssLine;

                var aFields = aRecords[i].getElementsByTagName("field");

                for (var idx = 0; idx < aFields.length; idx++) {
                    var td = document.createElement("td");
                    td.innerHTML = getXmlTextNode(aFields[idx]);
                    td.className = cssCell;
                    trDatas.appendChild(td);
                }

            }

            var conflictSpan = document.getElementById("ConflictSpan");
            if (conflictSpan != null) {
                conflictSpan.appendChild(tabReturn);
                conflictSpan.style.display = "none";
                conflictSpan.style.position = "absolute";
            }
            //Participants
            var users = oRes.getElementsByTagName("users") == null ? "" : getXmlTextNode(oRes.getElementsByTagName("users")[0]);
            var aUsers = users.split(';');

            var nMainUserId = 0;

            var sInptName = "COL_" + nGlobalActiveTab + "_" + (nGlobalActiveTab + 99);

            var inptMainOwner = document.querySelector("input[ename='" + sInptName + "']");
            if (inptMainOwner)
                nMainUserId = getAttributeValue(inptMainOwner, "dbv");

            var descId = Number(nGlobalActiveTab) + 92;
            var eltMultiOwner = document.getElementById("COL_" + nGlobalActiveTab + "_" + descId + "_" + nFileId + "_" + nFileId + "_0");
            var sMultiOwner = ";" + getAttributeValue(eltMultiOwner, "dbv") + ";";

            for (var j = 0; j < aUsers.length; j++) {
                if (aUsers[j] == "")
                    continue;

                var elm = document.getElementById(nGlobalActiveTab + "_" + descId + "_" + aUsers[j]);
                if (elm != null) {
                    elm.className = "participant conflict_conflicted";
                    bConflicted = true;
                }
                if (sMultiOwner.indexOf(";" + aUsers[j] + ";") != -1)
                    bConflicted = true;

                //Main Owner
                if (nMainUserId == Number(aUsers[j])) {
                    bConflicted = true;
                    inptMainOwner.className = inptMainOwner.className + " conflict_conflicted";
                }
            }
        }
        else {
            if (aRecords.length > 0) {
                bConflicted = true;
            }
        }


        if (document.getElementById("conflictIndicator"))
            document.getElementById("conflictIndicator").value = bConflicted == true ? "1" : "0";

        if (document.getElementById(elementId)) {
            document.getElementById(elementId).setAttribute("conflictIndicator", bConflicted == true ? "1" : "0");
        }


        document.isTplLoading = false;
    }
    catch (exp) { }

    if (typeof (afterGetConflict) == "function")
        afterGetConflict();

}

function showConflictList(bShow) {
    //TODO - à réactiver en cas de besoin

    return;
    /*
    var conflictSpan = document.getElementById("ConflictSpan");
    if (bShow)
        conflictSpan.style.display = "block";
    else {
        conflictSpan.style.display = "none";
    }*/

}

// Tableau destiné à contenir tous les objets de types eMemoEditor dans les cas de la modification et de la création
// #83 082 : on ne le réinitialise pas s'il existe déjà. Permet de ne pas écraser le contexte aux yeux d'un champ Mémo enfant (ex : affiché via une fenêtre Plein écran) si une MAJ a lieu en arrière-plan, et provoque le réaffichage complet de toute l'application (RefreshFile global), rendant ainsi la Modal Dialog du champ Mémo Plein écran orpheline.
// Charge au code qui se chargera de remplir ce tableau précédemment remis à zéro, de faire les MAJ appropriées sur un tableau existant

// nsMain (eMain.js) pouvant ne pas forcément être toujours accessible (notamment quand eFile.js est inclus avant eMain.js dans une iframe), on utilise à défaut la fonction de la page racine
// On lui passe alors le document en cours pour que l'initialisation se fasse sur celui-ci, et non pas sur .top
// ATTENTION, ne surtout pas initialiser nsMain ici (avec un var nsMain = ...)
// Car sinon, cette initialisation serait prioritaire sur celle effectuée par eMain.js (qui ne l'instancie que si elle ne l'est pas déjà) dans les cas où eMain.js est inclus après eFile.js.
// On utilise donc une variable avec un nom différent, mais pas en portée locale (let) pour la compatibilité IE
// Le typeof étant nécessaire ici car, contrairement à eMain (où on utilise la même variable avant et après le =), on aura une erreur de variable undefined ici si on utilise nsMain sans tester au préalable.
var nsMainFile = typeof (nsMain) == "object" ? nsMain : top.nsMain;
if (nsMainFile) {
    if (!nsMainFile.hasMemoEditors(document))
        nsMainFile.initMemoEditorsArray(document);
}
// Ou, vraiment à défaut, une initialisation directe (comme avant la 10.612)
else {
    if (!document.aMemoEditors)
        document.aMemoEditors = new Array();
}

var modalDate = null;
function selectDate(id) {
    modalDate = createCalendarPopUp("ValidSelectDate", 1, 1, top._res_5017, top._res_5003, "SelectDateOk", top._res_29, null, null, _parentIframeId, id, document.getElementById(id).value);
    modalDate.activeInputId = id;
}

function setItemType(nType, fldId, nTab) {
    if (!nTab)
        nTab = Number(nGlobalActiveTab);

    var eltToValid = document.getElementById(fldId);
    eltToValid.value = nType;
    var descIdEndDate = Number(nTab) + endDateDescIdTpl;

    var fileId = 0;
    var fileDiv = document.getElementById("fileDiv_" + nTab);
    if (fileDiv != null)
        fileId = fileDiv.getAttribute("fid");

    var endDateId = "COL_" + nTab + "_" + descIdEndDate;
    //Couleur
    var descIdColor = Number(nTab) + 80;
    var colId = "COL_" + nTab + "_" + descIdColor;

    //Identifie les champs
    var descIdBeginDate = document.getElementById("BeginDateDescId").value;
    var beginHourId = "COL_" + nTab + "_" + descIdBeginDate + "_H_" + fileId + "_" + fileId + "_0";
    var beginDateId = "COL_" + nTab + "_" + descIdBeginDate + "_D_" + fileId + "_" + fileId + "_0";
    var beginHourBtnId = "COL_" + nTab + "_" + descIdBeginDate + "_H_BTN_" + fileId + "_" + fileId + "_0";

    var descIdEndDate = Number(nTab) + endDateDescIdTpl;
    var endHourId = "COL_" + nTab + "_" + descIdEndDate + "_H_" + fileId + "_" + fileId + "_0";
    var endDateId = "COL_" + nTab + "_" + descIdEndDate + "_D_" + fileId + "_" + fileId + "_0";
    var endHourBtnId = "COL_" + nTab + "_" + descIdEndDate + "_H_BTN_" + fileId + "_" + fileId + "_0";
    var endDateBtnId = "COL_" + nTab + "_" + descIdEndDate + "_D_BTN_" + fileId + "_" + fileId + "_0";
    var endDateLblId = "COL_" + nTab + "_" + descIdEndDate + "_LBL_" + fileId + "_" + fileId + "_0";



    var visu = "visible";
    if (nType == 0) //Tâche
    {
        visu = "hidden";
        //affiche l'heure debut si le champs est masqué
        document.getElementById(beginHourId).style.visibility = "visible";
        document.getElementById(beginHourBtnId).style.visibility = "visible";
        document.getElementById(colId).style.visibility = "hidden";

    }
    else {
        //affiche l'heure fin si le champs est masqué
        document.getElementById(endHourId).style.visibility = "visible";
        //SHA : demande #77 937
        if (document.getElementById(endHourBtnId) != null && typeof (document.getElementById(endHourBtnId)) != "undefined")
            document.getElementById(endHourBtnId).style.visibility = "visible";
        document.getElementById(colId).style.visibility = "visible";
    }
    document.getElementById(endHourId).style.visibility = visu;
    //SHA : demande #77 937
    if (document.getElementById(endHourBtnId) != null && typeof (document.getElementById(endHourBtnId)) != "undefined")
        document.getElementById(endHourBtnId).style.visibility = visu;
    document.getElementById(endDateId).style.visibility = visu;
    document.getElementById(endDateBtnId).style.visibility = visu;
    document.getElementById(endDateLblId).style.visibility = visu;

    var bAutoSave = isAutoSave(nTab);

    if (bAutoSave) {
        //enregistrement de la valeur
        var fldToValid = getFldEngFromElt(eltToValid);

        var eEngineUpdater = new eEngine();
        eEngineUpdater.Init();


        //gestion planning popup
        var bPlan = getAttributeValue(document.getElementById("fileDiv_" + this.tab), "ecal") == "1";

        //le top.eModFile n'est pas renseigné lorsque la popup a été ouverte depuis une popup de recherche
        // eModFile est en effet renseigné lors de l'ouverture de la fiche en modal dialog et dans ce cas, la variable est attachée à la fenêtre de recherche
        // et plus à la fenêtre principale (top)
        var myModFile = eTools.getModalFromWindowName(window.name, nTab);
        if (myModFile) {
            if (top.eModFile && top.eModFile.isModalDialog)
                myModFile = top.eModFile;
            else
                myModFile = top.window['_md']["popupFile"];
        }

        var myModalDialog = null;
        if (myModFile != null)
            myModalDialog = { oModFile: myModFile, modFile: myModFile.getIframe(), pupClose: false, bPlanning: true, docTop: top };

        eEngineUpdater.ModalDialog = myModalDialog;
        eEngineUpdater.AddOrSetParam('tab', nTab);
        eEngineUpdater.AddOrSetParam('fileId', fileId);

        // HLA - On averti qu'on est en sorti de champs - Dev #45363
        eEngineUpdater.AddOrSetParam('onBlurAction', '1');

        eEngineUpdater.AddOrSetField(fldToValid);
        eEngineUpdater.SuccessCallbackFunction = function (engResult) {
            if (nTab == top.nGlobalActiveTab)
                top.loadList();
        };

        eEngineUpdater.UpdateLaunch();
    }
    else {
        ///déclenchement des règles
        applyRuleOnBlank(nTab, null, fileId, 5, fldId);
    }
}

function ValidSelectDate(val, operator, nodeId, frmId) {
    var oFld = document.getElementById(modalDate.activeInputId);
    oFld.setAttribute("oldvalue", oFld.value);
    oFld.value = val;
    modalDate.hide();
    var aNodeId = nodeId.split('_');
    var nTab;
    if (aNodeId.length >= 2)
        nTab = getNumber(aNodeId[1]);
    validDateFields(nTab, null, null, nodeId);
}

function SelectDateOk(val) {
    alert("SelectDateOk");
}

function isValidDate(d) {
    if (Object.prototype.toString.call(d) !== "[object Date]")
        return false;
    return !isNaN(d.getTime());
}

//GCH - #36012 - Internationnalisation - Planning - ok
function validDateFields(nTab, bUpdateType, bFromLastValue, nodeId) {
    if (!nTab)
        nTab = Number(nGlobalActiveTab);

    if (typeof bFromLastValue === "undefined")
        bFromLastValue = false;

    var fileId = 0;
    var fileDiv = document.getElementById("fileDiv_" + nTab);
    if (fileDiv != null)
        fileId = fileDiv.getAttribute("fid");

    var bAutoSave = isAutoSave(nTab);

    var descIdBeginDate = document.getElementById("BeginDateDescId").value;
    var beginDateElt = document.getElementById("COL_" + nTab + "_" + descIdBeginDate + "_D_" + fileId + "_" + fileId + "_0");
    var beginHourElt = document.getElementById("COL_" + nTab + "_" + descIdBeginDate + "_H_" + fileId + "_" + fileId + "_0");
    var beginDateOnly = eDate.ConvertDisplayToBdd(beginDateElt.value);
    var beginHour = beginHourElt.value;
    var beginDate = beginDateOnly + " " + beginHour;

    var descIdEndDate = nTab + endDateDescIdTpl;
    var endDateId = "COL_" + nTab + "_" + descIdEndDate + "_D_" + fileId + "_" + fileId + "_0";
    var endHourId = "COL_" + nTab + "_" + descIdEndDate + "_H_" + fileId + "_" + fileId + "_0";

    var eltDateBegin = document.getElementById("COL_" + nTab + "_" + descIdBeginDate + "_" + fileId + "_" + fileId + "_0");
    var eltDateEnd = document.getElementById("COL_" + nTab + "_" + descIdEndDate + "_" + fileId + "_" + fileId + "_0");

    if (document.getElementById(endDateId) != null) {
        var dbeginDate = null;
        var dendDate = null;

        var endDateElt = document.getElementById(endDateId);
        var endDateHour = document.getElementById(endHourId);

        var endDateOnly = eDate.ConvertDisplayToBdd(endDateElt.value);
        var endHour = endDateHour.value;
        var endDate = endDateOnly + " " + endHour;
        if (isDate(beginDateOnly) && isHour(beginHour)) dbeginDate = eDate.Tools.GetDateFromString(beginDate);
        if (isDate(endDateOnly) && isHour(endHour)) dendDate = eDate.Tools.GetDateFromString(endDate);

        if (document.getElementById(endDateId).style.visibility == "hidden") {
            endDate = "01/01/1900 00:00";
            dendDate = eDate.Tools.GetDateFromString(endDate);
        }

        if (!isValidDate(dbeginDate) || !isValidDate(dendDate)) {
            eAlert(0, top._res_846, top._res_846);
            resetDateValue(nTab);
            return;
        }


        if (!bFromLastValue) {

            // Ancienne date de début
            if (getAttributeValue(document.getElementById("COL_" + nTab + "_" + descIdBeginDate), "cclval") == "1") {

                var oldBeginDate = getAttributeValue(beginDateElt, "oldvalue") + " " + getAttributeValue(beginHourElt, "oldvalue");
                var oldBeginDateLabel = eDate.ConvertBddToDisplay(oldBeginDate);

                if (oldBeginDateLabel != beginDate) {
                    setAttributeValue(eltDateBegin, "oldvalue", oldBeginDateLabel);
                    LastValuesManager.addValue(eltDateBegin, nTab, descIdBeginDate, document.getElementById("COL_" + nTab + "_" + descIdBeginDate + "_LBL_" + fileId + "_" + fileId + "_0").innerText, oldBeginDateLabel, oldBeginDateLabel, true);
                }
            }

            // Ancienne date de fin
            if (getAttributeValue(document.getElementById("COL_" + nTab + "_" + descIdEndDate), "cclval") == "1") {

                var oldEndDate = getAttributeValue(endDateElt, "oldvalue") + " " + getAttributeValue(endDateHour, "oldvalue");
                var oldEndDateLabel = eDate.ConvertBddToDisplay(oldEndDate);

                if (oldEndDateLabel != endDate) {
                    setAttributeValue(eltDateEnd, "oldvalue", eDate.ConvertBddToDisplay(oldEndDate));
                    LastValuesManager.addValue(eltDateEnd, nTab, descIdEndDate, document.getElementById("COL_" + nTab + "_" + descIdEndDate + "_LBL_" + fileId + "_" + fileId + "_0").innerText, oldEndDateLabel, oldEndDateLabel, true);
                }
            }
        }



        //if (getAttributeValue(document.getElementById("COL_" + nTab + "_" + descIdEndDate), "cclval") == "1") {


        //}

        eltDateBegin.value = eDate.ConvertBddToDisplay(beginDate);    //On le retransforme en format affichage car les fields sont retransformé après dans le parcours de tous les fields
        if (dendDate <= dbeginDate) {
            setEndDate(nTab);
            //return;
        }
        else {
            eltDateEnd.value = eDate.ConvertBddToDisplay(endDate);    //On le retransforme en format affichage car les fields sont retransformé après dans le parcours de tous les fields
        }
    }

    //gestion planning popup
    var bPlan = getAttributeValue(document.getElementById("fileDiv_" + this.tab), "ecal") == "1";

    //le top.eModFile n'est pas renseigné lorsque la popup a été ouverte depuis une popup de recherche
    // eModFile est en effet renseigné lors de l'ouverture de la fiche en modal dialog et dans ce cas, la variable est attachée à la fenêtre de recherche
    // et plus à la fenêtre principale (top)
    var myModFile = eTools.getModalFromWindowName(window.name, nTab);

    if (myModFile == null) {
        if (top.eModFile && top.eModFile.isModalDialog)
            myModFile = top.eModFile;
        else if (typeof (top.window['_md']) != "undefined" && typeof (top.window['_md']["popupFile"]) != "undefined")
            myModFile = top.window['_md']["popupFile"];

    }

    var myModalDialog = null;
    if (myModFile != null)
        myModalDialog = { oModFile: myModFile, modFile: myModFile.getIframe(), pupClose: false, bPlanning: true, docTop: top };


    if (bAutoSave) {
        var eltConflictIndicator = document.getElementById("conflictIndicator")
        top.setWait(true);

        setConflictInfos(function () {



            //enregistrement de la valeur
            var fldEngBegin = getFldEngFromElt(eltDateBegin);
            var fldEngEnd = getFldEngFromElt(eltDateEnd);



            var eEngineUpdater = new eEngine();
            eEngineUpdater.Init();

            eEngineUpdater.ModalDialog = myModalDialog;

            eEngineUpdater.AddOrSetParam('tab', nTab);
            eEngineUpdater.AddOrSetParam('fileId', fileId);

            // HLA - On averti qu'on est en sorti de champs - Dev #45363
            eEngineUpdater.AddOrSetParam('onBlurAction', '1');

            eEngineUpdater.AddOrSetField(fldEngBegin);
            eEngineUpdater.AddOrSetField(fldEngEnd);


            if (bUpdateType) {
                var eType = document.getElementById("COL_" + nTab + "_" + (nTab + 83) + "_" + fileId + "_" + fileId + "_0");
                var fldEngType = getFldEngFromElt(eType);
                eEngineUpdater.AddOrSetField(fldEngType);
            }

            eEngineUpdater.SuccessCallbackFunction = function (engResult) {
                //BSE : #52 433 problème de scroll sur fiche plannig en mode incrustée
                RefreshFile(window);// KHA le 12/08/15: déjà effectué par eEngine.js
                if (nTab == top.nGlobalActiveTab)
                    top.loadList();
            };

            top.setWait(false);

            var bConflicted = false;
            if (eltConflictIndicator)
                bConflicted = eltConflictIndicator.value == "1";

            if (bConflicted) {
                eConfirm(1, '', top._res_6305, '', null, null, function () { eEngineUpdater.UpdateLaunch(); }, function () { top.setWait(false); });
            }
            else {
                eEngineUpdater.UpdateLaunch();
            }
        }, nTab);
    }
    else {
        ///déclenchement des règles
        applyRuleOnBlank(nTab, null, fileId, 5, null, null, LOADFILEFROM.REFRESH);
        //déclenchement des ORM
        if (nodeId) {
            //on recupere l'id du bloc info
            var srcElement = document.getElementById(nodeId);
            var oldValue = getAttributeValue(srcElement, "oldvalue");
            var newValue = srcElement.value;
            if (newValue == oldValue)
                return;
            nodeId = nodeId.replace('D', 'LBL').replace('H', 'LBL');
            var headerElement = document.getElementById(nodeId);
            if (headerElement && headerElement.parentElement && headerElement.parentElement.getAttribute('mf') == "1") {
                var eEngineUpdater = new eEngine();
                eEngineUpdater.Init();
                headerElement = headerElement.parentElement;

                eEngineUpdater.SuccessCallbackFunction = function (engResult) {
                }

                eEngineUpdater.AddOrSetParam('fldEditorType', '');
                eEngineUpdater.AddOrSetParam('catNewVal', false);
                eEngineUpdater.AddOrSetParam('jsEditorVarName', '');
                var descid = getAttributeValue(headerElement, 'did');
                var tab = getTabDescid(descid);
                // On indique la table à l'engine car le premier Field peut ne pas correspondre à la table de la fiche en cours de création
                eEngineUpdater.AddOrSetParam('tab', tab);
                eEngineUpdater.AddOrSetParam('fileId', fileId);
                eEngineUpdater.AddOrSetParam('onBlurAction', '1');
                eEngineUpdater.AddOrSetParam('engAction', 4);

                // Indique à l'engine sur quelle rubrique la vérification de la formule du milieu doit s'executer
                eEngineUpdater.AddOrSetParam('fieldTrigger', descid);

                var aFld = getFieldsInfos(tab, fileId);
                for (var i = 0; i < aFld.length; i++) {
                    var fld = aFld[i];
                    if (fld.descId == getAttributeValue(this.headerElement, 'did') && nodeSrcElement.tagName == 'INPUT') {
                        fld.cellId = nodeSrcElement.id;
                        fld.oldValue = oldValue;
                        fld.oldLabel = oldLabel;
                        fld.prevValue = oldValue;
                    }

                    eEngineUpdater.AddOrSetField(fld);
                }
                eEngineUpdater.UpdateLaunch();
            }
        }
        
    }


}

function flagEltAsEdited(Elt) {
    var editedClass = "eFieldEditorEdited";
    addClass(Elt, editedClass);
    // Utilisation d'un timer pour faire disparaître l'effet au bout de X milli secondes
    window.setTimeout(function () { removeClass(Elt, editedClass); }, 500);

}
//GCH - #36012 - Internationnalisation - Planning - ok
function setEndDate(nTab) {
    if (!nTab)
        nTab = Number(nGlobalActiveTab);
    var fileId = 0;
    var fileDiv = document.getElementById("fileDiv_" + nTab);
    if (fileDiv != null)
        fileId = fileDiv.getAttribute("fid");

    var descIdBeginDate = document.getElementById("BeginDateDescId").value;
    var beginDateId = "COL_" + nTab + "_" + descIdBeginDate + "_D_" + fileId + "_" + fileId + "_0";
    var beginHourId = "COL_" + nTab + "_" + descIdBeginDate + "_H_" + fileId + "_" + fileId + "_0";
    var beginDate = eDate.ConvertDisplayToBdd(document.getElementById(beginDateId).value + " " + document.getElementById(beginHourId).value);

    var descIdEndDate = nTab + endDateDescIdTpl;
    var endDateId = "COL_" + nTab + "_" + descIdEndDate + "_D_" + fileId + "_" + fileId + "_0";
    var endHourId = "COL_" + nTab + "_" + descIdEndDate + "_H_" + fileId + "_" + fileId + "_0";

    if (document.getElementById(beginDateId) != null && document.getElementById(endDateId) != null) {

        // #43323 CRU : On garde l'heure de fin définie par l'utilisateur 

        var dbeginDate = eDate.Tools.GetDateFromString(beginDate);
        var endHour = document.getElementById(endHourId).value;
        var defDurationM = document.getElementById("DefaultDuration").value;

        var endDate, dendDate;
        // MAB - #56 235 - Après réinitialisation de la date avec l'heure de fin définie précédemment, revérifier si la date de fin est toujours postérieure à la date
        // de début. Si ce n'est plus le cas, on la réinitialise avec l'intervalle par défaut
        var resetEndDateWithDefDuration = false;

        // cf. descriptif de la demande #43323 :
        // "On va garder l'intervalle horaire si le rendez-vous est enregistré."
        // "Si le rdv est pas créé on n'en tient pas compte."
        // D'où la vérification de fileId != 0 pour valider la reprise de l'horaire uniquement sur un RDV déjà enregistré en base
        if (endHour && fileId != 0 && fileId != "0") {
            endDate = eDate.ConvertDisplayToBdd(document.getElementById(beginDateId).value + " " + endHour);
            dendDate = eDate.Tools.GetDateFromString(endDate);
        }
        // Dans tous les autres cas (date de fin non définie ou création de nouveau RDV), on utilise l'algorithme initialement défini avec la durée par défaut
        else {
            resetEndDateWithDefDuration = true;
        }

        // Si, en conservant l'heure de fin, la date de fin redevient antérieure à la date de début, on recorrige avec l'intervalle par défaut
        if (dendDate && dendDate <= dbeginDate)
            resetEndDateWithDefDuration = true;

        if (resetEndDateWithDefDuration) {
            // Ancien algorithme utilisé avant la mise en place du correctif #43 323
            dendDate = new Date(dbeginDate.getTime() + defDurationM * 60000);
            endDate = eDate.Tools.GetStringFromDate(dendDate, false, false);
        }

        var endDateHidden = "COL_" + nTab + "_" + descIdEndDate + "_" + fileId + "_" + fileId + "_0";

        document.getElementById(endDateHidden).value = eDate.ConvertBddToDisplay(endDate);
        //applyRuleOnBlank(nTab, null, fileId, 5, null); //modif par kha 12/08/15 le champ date de fin ne tient pas compte de l'autosave.

    }

}

//GCH - #36012 - Internationnalisation - Planning - ok
function doOnchangeDate(nTab) {

    if (!nTab)
        nTab = Number(nGlobalActiveTab);

    var fileId = 0;
    var fileDiv = document.getElementById("fileDiv_" + nTab);
    if (fileDiv != null)
        fileId = fileDiv.getAttribute("fid");

    var descIdBeginDate = document.getElementById("BeginDateDescId").value;
    var beginDateId = "COL_" + nTab + "_" + descIdBeginDate + "_D_" + fileId + "_" + fileId + "_0";
    var beginHourId = "COL_" + nTab + "_" + descIdBeginDate + "_H_" + fileId + "_" + fileId + "_0";
    var beginDate = document.getElementById(beginDateId).value + " " + document.getElementById(beginHourId).value;

    var beginDateHidden = "COL_" + nTab + "_" + descIdBeginDate + "_" + fileId + "_" + fileId + "_0";
    document.getElementById(beginDateHidden).value = beginDate;


    var descIdEndDate = nTab + endDateDescIdTpl;
    var endDateId = "COL_" + nTab + "_" + descIdEndDate + "_D_" + fileId + "_" + fileId + "_0";
    var endHourId = "COL_" + nTab + "_" + descIdEndDate + "_H_" + fileId + "_" + fileId + "_0";
    var endDate = document.getElementById(endDateId).value + " " + document.getElementById(endHourId).value;

    var endDateHidden = "COL_" + nTab + "_" + descIdEndDate + "_" + fileId + "_" + fileId + "_0";
    document.getElementById(endDateHidden).value = endDate;

    validDateFields(nTab);
}

//GCH - #36012 - Internationnalisation - Planning - ok
function resetDateValue(nTab) {

    if (!nTab)
        nTab = Number(nGlobalActiveTab);
    var fileId = 0;
    var fileDiv = document.getElementById("fileDiv_" + nTab);
    if (fileDiv != null)
        fileId = fileDiv.getAttribute("fid");

    var descIdBeginDate = document.getElementById("BeginDateDescId").value;
    var beginDateId = "COL_" + nTab + "_" + descIdBeginDate + "_D_" + fileId + "_" + fileId + "_0";
    var beginHourId = "COL_" + nTab + "_" + descIdBeginDate + "_H_" + fileId + "_" + fileId + "_0";
    var beginDate = document.getElementById(beginDateId).getAttribute("oldvalue") + " " + document.getElementById(beginHourId).getAttribute("oldvalue");

    var descIdEndDate = nTab + endDateDescIdTpl;
    var endDateId = "COL_" + nTab + "_" + descIdEndDate + "_D_" + fileId + "_" + fileId + "_0";
    var endHourId = "COL_" + nTab + "_" + descIdEndDate + "_H_" + fileId + "_" + fileId + "_0";
    var endDate = document.getElementById(endDateId).getAttribute("oldvalue") + " " + document.getElementById(endHourId).getAttribute("oldvalue");

    document.getElementById(endDateId).value = document.getElementById(endDateId).getAttribute("oldvalue");
    document.getElementById(beginDateId).value = document.getElementById(beginDateId).getAttribute("oldvalue");

    /*var dbeginDate = eDate.Tools.GetDateFromString(beginDate);
    var nCalendarMinutesInterval = Number(document.getElementById("CalendarInterval").value);
    var dendDate = dateAdd("n", nCalendarMinutesInterval, dbeginDate)
    var endDate = eDate.Tools.GetStringFromDate(dendDate, false, false);*/
    var beginDateHidden = "COL_" + nTab + "_" + descIdBeginDate + "_" + fileId + "_" + fileId + "_0";
    var endDateHidden = "COL_" + nTab + "_" + descIdEndDate + "_" + fileId + "_" + fileId + "_0";
    document.getElementById(endDateHidden).value = endDate;
    document.getElementById(beginDateHidden).value = beginDate;
    applyRuleOnBlank(nTab, null, fileId, 5, null);

}
/* KHA le 12/08/15 ne semble plus utilisé
function selectHour(id, nTab) {
    //Cacher les autres menus s'ils existent

    if (!nTab)
        nTab = Number(nGlobalActiveTab);

    var fileId = 0;
    var fileDiv = document.getElementById("fileDiv_" + nTab);
    if (fileDiv != null)
        fileId = fileDiv.getAttribute("fid");



    var descIdBeginDate = document.getElementById("BeginDateDescId").value;
    var descIdEndDate = nTab + endDateDescIdTpl;

    var idBeginH = "COL_" + nTab + "_" + descIdBeginDate + "_H_" + fileId + "_" + fileId + "_0" + "_CHOICE";
    var idEndH = "COL_" + nTab + "_" + descIdEndDate + "_H_" + fileId + "_" + fileId + "_0" + "_CHOICE";

    document.getElementById(idBeginH).style.display = "none";
    document.getElementById(idEndH).style.display = "none";

    var o = document.getElementById(id);
    //document.getElementById();
    var obj_pos = getAbsolutePosition(o);


    var oDateMenuMouseOver = function () {

        var FltOut = setTimeout(
            function () {

                //Masque le menu
                document.getElementById(id + "_CHOICE").style.display = "none";
            }
            , 200);

        //Annule la disparition
        setEventListener(document.getElementById(id + "_CHOICE"), "mouseover", function () { clearTimeout(FltOut) });
    };

    //Faire disparaitre le menu
    setEventListener(document.getElementById(id + "_CHOICE"), "mouseout", oDateMenuMouseOver);
    document.getElementById(id + "_CHOICE").style.display = "block";
}
*/
function OnConfidClick(elem, nTab) {
    if (!nTab)
        nTab = Number(nGlobalActiveTab);

    if (elem == null || typeof (elem) == "undefined")
        return;

    var attributeChecked = elem.getAttribute("chk");
    var val = attributeChecked == "1" ? "0" : "1";
    var fldVisu = attributeChecked == "1" ? "visible" : "hidden";

    var fileId = 0;
    var fileDiv = document.getElementById("fileDiv_" + nTab);
    if (fileDiv != null)
        fileId = fileDiv.getAttribute("fid");

    var fldConfidDescId = Number(nTab) + 84;
    var fldConfidId = "COL_" + nTab + "_" + fldConfidDescId + "_" + fileId + "_" + fileId + "_0";

    if (document.getElementById(fldConfidId) != null)
        document.getElementById(fldConfidId).value = val;
    document.getElementById("Fld_81").style.visibility = fldVisu;
}


function onAllDaysCheckOption(srcId) {

    var fldVisu = "visible";
    if (document.getElementById(srcId).checked) {
        document.getElementById(srcId).checked = false;
    }
    else {
        document.getElementById(srcId).checked = true;
        fldVisu = "hidden";
    }

    var fileId = 0;
    var nTab = Number(srcId.split('_')[1]);
    var fileDiv = document.getElementById("fileDiv_" + nTab);
    if (fileDiv != null)
        fileId = fileDiv.getAttribute("fid");


    var fldTypeDescId = Number(nTab) + TYPE_PLANNING;
    var fldTypeId = "COL_" + nTab + "_" + fldTypeDescId + "_" + fileId + "_" + fileId + "_0";

    if (document.getElementById(srcId).checked)
        document.getElementById(fldTypeId).value = 2; //AddDayEvent
    else
        document.getElementById(fldTypeId).value = 1; //AddDayEvent


    var descIdBeginDate = document.getElementById("BeginDateDescId").value;
    var beginDateId = "COL_" + nTab + "_" + descIdBeginDate + "_D_" + fileId + "_" + fileId + "_0";
    var beginHourId = "COL_" + nTab + "_" + descIdBeginDate + "_H_" + fileId + "_" + fileId + "_0";
    var beginHourBtnId = "COL_" + nTab + "_" + descIdBeginDate + "_H_BTN_" + fileId + "_" + fileId + "_0";

    document.getElementById(beginHourId).value = "00:00";
    document.getElementById(beginHourId).style.visibility = fldVisu;
    document.getElementById(beginHourBtnId).style.visibility = fldVisu;

    var descIdEndDate = nTab + endDateDescIdTpl;
    var endDateId = "COL_" + nTab + "_" + descIdEndDate + "_D_" + fileId + "_" + fileId + "_0";
    var endHourId = "COL_" + nTab + "_" + descIdEndDate + "_H_" + fileId + "_" + fileId + "_0";
    var endHourBtnId = "COL_" + nTab + "_" + descIdEndDate + "_H_BTN_" + fileId + "_" + fileId + "_0";

    document.getElementById(endHourId).value = "23:00";
    document.getElementById(endHourId).style.visibility = fldVisu;
    document.getElementById(endHourBtnId).style.visibility = fldVisu;

    ///déclenchement des règles
    //applyRuleOnBlank(nTab, null, fileId, 5, null);

    validDateFields(nTab, true);

}


function onScheduleCheckOption(srcId, nTab) {



    if (document.getElementById(srcId).checked) {
        document.getElementById(srcId).checked = false;
        document.getElementById("ScheduleParamsLnk").style.display = "none";
        var fileId = 0;
        var fileDiv = document.getElementById("fileDiv_" + nGlobalActiveTab);
        if (fileDiv != null)
            fileId = fileDiv.getAttribute("fid");

        var descIdScheduleId = Number(nGlobalActiveTab) + 82;
        var scheduleId = "COL_" + nGlobalActiveTab + "_" + descIdScheduleId + "_" + fileId + "_" + fileId + "_0";

        //TODO - Updater pour supprimer le schedule
        var scheduleUpd = new eUpdater("mgr/eScheduleManager.ashx", 1);
        scheduleUpd.addParam("action", "delete", "post");
        scheduleUpd.addParam("scheduleid", document.getElementById(scheduleId).value, "post");

        // 
        scheduleUpd.ErrorCallBack = function () { };

        scheduleUpd.send();


        document.getElementById(scheduleId).value = "";


        ///déclenchement des règles
        applyRuleOnBlank(nGlobalActiveTab, null, fileId, 5, null);

    }
    else {

        document.getElementById(srcId).checked = true;
        document.getElementById("ScheduleParamsLnk").style.display = "block";
    }



}

//GCH - #36012 - Internationnalisation - Planning - ok
var modalSchedule = null;
function ShowScheduleParameter() {
    //if( ! isAllowedScheduling() )
    //{
    //    document.getElementById('Schedule').checked = false;
    //    document.getElementById('ScheduleParameter').style.visibility = 'hidden';
    //    return;
    //}

    if (document.getElementById("chk_chkSchedule").getAttribute("chk") == "0") {
        var PeriodiciteInfo = document.getElementById("PeriodiciteInfo");
        if (PeriodiciteInfo != null) {
            PeriodiciteInfo.style.display = 'none';
            PeriodiciteInfo.style.display = 'none';
        }
        setScheduleId(0);
        return;

    }
    var fileId = 0;
    var fileDiv = document.getElementById("fileDiv_" + nGlobalActiveTab);
    if (fileDiv != null)
        fileId = fileDiv.getAttribute("fid");

    var descIdBeginDate = document.getElementById("BeginDateDescId").value;
    var descIdEndDate = Number(nGlobalActiveTab) + endDateDescIdTpl;
    var descIdScheduleId = Number(nGlobalActiveTab) + 82;
    var descIdTypeId = Number(nGlobalActiveTab) + TYPE_PLANNING;

    var beginDateId = "COL_" + nGlobalActiveTab + "_" + descIdBeginDate + "_" + fileId + "_" + fileId + "_0";
    var endDateId = "COL_" + nGlobalActiveTab + "_" + descIdEndDate + "_" + fileId + "_" + fileId + "_0";
    var scheduleId = "COL_" + nGlobalActiveTab + "_" + descIdScheduleId + "_" + fileId + "_" + fileId + "_0";
    var endTypeId = "COL_" + nGlobalActiveTab + "_" + descIdTypeId + "_" + fileId + "_" + fileId + "_0";

    var nScheduleId = document.getElementById(scheduleId).value.replace(" ", "");
    var strBeginDate = document.getElementById(beginDateId).value;
    var strEndDate = document.getElementById(endDateId).value;
    var nAppType = document.getElementById(endTypeId).value;
    var nNew = 0;
    if (fileId != 0)
        nNew = 1;



    modalSchedule = new eModalDialog(top._res_1049, 0, "eSchedule.aspx", 450, 500);

    modalSchedule.addParam("scheduletype", 0, "post");
    modalSchedule.addParam("New", nNew, "post");
    modalSchedule.addParam("DateDescId", descIdBeginDate, "post");
    modalSchedule.addParam("EndDate", eDate.ConvertDisplayToBdd(strEndDate), "post");
    modalSchedule.addParam("BeginDate", eDate.ConvertDisplayToBdd(strBeginDate), "post");
    modalSchedule.addParam("ScheduleId", nScheduleId, "post");
    modalSchedule.addParam("Tab", nGlobalActiveTab, "post");
    modalSchedule.addParam("Workingday", "TODO", "post");
    modalSchedule.addParam("calleriframeid", _parentIframeId, "post");
    modalSchedule.addParam("AppType", nAppType, "post");
    modalSchedule.addParam("FileId", fileId, "post");

    modalSchedule.ErrorCallBack = ShowScheduleParameterCancel;

    modalSchedule.show();
    modalSchedule.addButtonFct(top._res_29, ShowScheduleParameterCancel, "button-gray", 'cancel');
    modalSchedule.addButtonFct(top._res_28, ShowScheduleParameterValid, "button-green");
}

function setScheduleId(id) {

    modalSchedule.hide();

    var fileId = 0;
    var fileDiv = document.getElementById("fileDiv_" + nGlobalActiveTab);
    if (fileDiv != null)
        fileId = fileDiv.getAttribute("fid");

    var descIdScheduleId = Number(nGlobalActiveTab) + 82;
    var scheduleId = "COL_" + nGlobalActiveTab + "_" + descIdScheduleId + "_" + fileId + "_" + fileId + "_0";


    document.getElementById(scheduleId).value = id;

    ///déclenchement des règles
    applyRuleOnBlank(nGlobalActiveTab, null, fileId, 5, null);

}

function ShowScheduleParameterValid() {
    modalSchedule.getIframe().Valid();

}


// ASY : #26 833 : [bug] - Rdv Récurrent - Ecran de paramètres
// Function permettant de recuperer le scheduleID
function getScheduleID() {

    var nScheduleId = 0;
    var fileId = 0;
    var fileDiv = document.getElementById("fileDiv_" + nGlobalActiveTab);
    if (fileDiv != null)
        fileId = fileDiv.getAttribute("fid");
    var descIdScheduleId = Number(nGlobalActiveTab) + 82;
    var scheduleId = "COL_" + nGlobalActiveTab + "_" + descIdScheduleId + "_" + fileId + "_" + fileId + "_0";

    if ((document.getElementById(scheduleId) != null) && (document.getElementById(scheduleId).value != ""))
        nScheduleId = document.getElementById(scheduleId).value.replace(" ", "");

    return nScheduleId;
}

function ShowScheduleParameterCancel() {

    modalSchedule.hide();

    // ASY : #26 833 : [bug] - Rdv Récurrent - Ecran de paramètres
    var nScheduleId = getScheduleID();
    //BSE:#72 283 Décocher la case Périodicité si on annule la période
    if (nScheduleId == 0)
        chgChk(document.getElementById("chk_chkSchedule"));


}


var modalAlertParams;
//GCH - #36012 - Internationnalisation - Planning - ok
//---- Affiche la fenetre de parametres
function ShowAlertParameter() {
    var fileId = 0;

    var fileDiv = document.getElementById("fileDiv_" + nGlobalActiveTab);
    if (fileDiv != null)
        fileId = fileDiv.getAttribute("fid");

    var descIdAlertDate = Number(nGlobalActiveTab) + 78;
    var descIdAlertTime = Number(nGlobalActiveTab) + 77;
    var descIdAlertSound = Number(nGlobalActiveTab) + 76;
    var descIdBeginDate = document.getElementById("BeginDateDescId").value;



    var domIdAlertDate = "COL_" + nGlobalActiveTab + "_" + descIdAlertDate + "_" + fileId + "_" + fileId + "_0";
    var domIdAlertTime = "COL_" + nGlobalActiveTab + "_" + descIdAlertTime + "_" + fileId + "_" + fileId + "_0";
    var domIdAlertSound = "COL_" + nGlobalActiveTab + "_" + descIdAlertSound + "_" + fileId + "_" + fileId + "_0";
    var domIdBeginDate = "COL_" + nGlobalActiveTab + "_" + descIdBeginDate + "_" + fileId + "_" + fileId + "_0";

    var strAlertDate = document.getElementById(domIdAlertDate).value;
    var strAlertTime = decode(document.getElementById(domIdAlertTime).value.replace(" ", ""));
    var strAlertSound = document.getElementById(domIdAlertSound).value;
    var strBeginDate = document.getElementById(domIdBeginDate).value;

    var nNew = 1;
    if (fileId > 0)
        nNew = 0;

    var alertUrl = "eAdvAlertParam.aspx";

    modalAlertParams = new eModalDialog(top._res_1014, 0, alertUrl, 500, 180);

    modalAlertParams.ErrorCallBack = function () { setWait(false); }

    modalAlertParams.addParam("new", nNew, "post");
    modalAlertParams.addParam("tab", nGlobalActiveTab, "post");
    modalAlertParams.addParam("alertdate", eDate.ConvertDisplayToBdd(strAlertDate), "post");
    modalAlertParams.addParam("alerttime", strAlertTime, "post");
    modalAlertParams.addParam("alertsound", strAlertSound, "post");
    modalAlertParams.addParam("begindate", eDate.ConvertDisplayToBdd(strBeginDate), "post");
    modalAlertParams.hideMaximizeButton = true;
    modalAlertParams.show();

    modalAlertParams.addButtonFct(top._res_29, alertParamsCancel, "button-gray");
    modalAlertParams.addButtonFct(top._res_28, alertParamsValid, "button-green");


}


function alertParamsCancel() {
    modalAlertParams.hide();
}

function alertParamsValid() {

    // Autosave sur le parametrage de l'alert
    var autoSave = false;
    var mainDiv = document.getElementById("mainDiv");
    if (mainDiv != null && getAttributeValue(mainDiv, "popup") == "1" && getAttributeValue(mainDiv, "autosv") == "1")
        autoSave = true;

    var aParam = modalAlertParams.getIframe().getReturnValue().split(';');

    var strAlertDate = aParam[0];
    var strAlertTime = aParam[1];
    var strAlertSound = aParam[2];

    var fileId = 0;
    var fileDiv = document.getElementById("fileDiv_" + nGlobalActiveTab);
    if (fileDiv != null)
        fileId = fileDiv.getAttribute("fid");

    var descIdAlertDate = Number(nGlobalActiveTab) + 78;
    var descIdAlertTime = Number(nGlobalActiveTab) + 77;
    var descIdAlertSound = Number(nGlobalActiveTab) + 76;

    var domIdAlertDate = "COL_" + nGlobalActiveTab + "_" + descIdAlertDate + "_" + fileId + "_" + fileId + "_0";
    var domIdAlertTime = "COL_" + nGlobalActiveTab + "_" + descIdAlertTime + "_" + fileId + "_" + fileId + "_0";
    var domIdAlertSound = "COL_" + nGlobalActiveTab + "_" + descIdAlertSound + "_" + fileId + "_" + fileId + "_0";

    document.getElementById(domIdAlertDate).value = strAlertDate;
    document.getElementById(domIdAlertTime).value = strAlertTime;
    document.getElementById(domIdAlertSound).value = strAlertSound;

    if (autoSave) {

        UpdateAlertConfig({
            'tab': nGlobalActiveTab,
            'fid': fileId,
            'fields': [descIdAlertDate, descIdAlertTime, descIdAlertSound],
            'onSucess': modalAlertParams.hide
        });

    } else {
        modalAlertParams.hide();
    }
}


/// Met à jour les champs alert sur planning
/// Options :
///   tab : descid de la table planning
///   fid : id de la fiche planning
///   fields : liste des descid de champs à mettre à jour
///   onSucess : callback après le succès d'enregistrement
///
function UpdateAlertConfig(configOptions) {
    if (configOptions.fields.length == 0) {
        configOptions.onSucess();
        return;
    }

    var engine = new eEngine();
    engine.Init();
    engine.AddOrSetParam('tab', configOptions.tab);
    engine.AddOrSetParam('fileId', configOptions.fid);
    engine.AddOrSetParam("engAction", XrmCruAction.UPDATE);
    engine.AddOrSetParam("fieldTrigger", configOptions.fields.join(";"));

    var engineFields = new Array();
    engineFields = getFieldsInfos(configOptions.tab, configOptions.fid, configOptions.fields.join(";"));

    for (var i = 0; i < engineFields.length; i++) {
        engineFields[i].forceUpdate = '1';
        engine.AddOrSetField(engineFields[i]);
    }

    engine.ErrorCallBack = function (oRes) { setWait(false); };
    engine.SuccessCallbackFunction = function (engResult) { setWait(false); configOptions.onSucess(); };

    try { engine.UpdateLaunch(); }
    catch (ex) { setWait(false); }
}

function OnAlertClick(elem) {
    if (elem == null || typeof (elem) == "undefined")
        return;

    var attributeChecked = elem.getAttribute("chk");
    var fldVisu = attributeChecked == "1" ? "block" : "none";
    document.getElementById("AlertParamLnk").style.display = fldVisu;
}


function hideHourPopup() {
    document.getElementById('HoursPopup').style.display = 'none';
    document.onclick = oldDocOnclick;
}

var oldDocOnclick = null;
//GCH - #36012 - Internationnalisation - Planning - ok
function showHourPopup(bEndDate, oTrigger, nTab) {
    if (!nTab)
        nTab = nGlobalActiveTab;

    hideHourPopup();

    var fileId = 0;
    var fileDiv = document.getElementById("fileDiv_" + nTab);
    if (fileDiv != null)
        fileId = fileDiv.getAttribute("fid");

    if (document.getElementById('HoursPopup').style.display == 'block')
        document.getElementById('HoursPopup').style.display = 'none';

    var descIdBeginDate = document.getElementById("BeginDateDescId").value;
    var descIdEndDate = Number(nTab) + endDateDescIdTpl;
    //var descIdScheduleId = Number(nTab) + 82;
    //var descIdTypeId = Number(nTab) + TYPE_PLANNING;

    var beginDateId = "COL_" + nTab + "_" + descIdBeginDate + "_" + fileId + "_" + fileId + "_0";
    var endDateId = "COL_" + nTab + "_" + descIdEndDate + "_" + fileId + "_" + fileId + "_0";
    //var scheduleId = "COL_" + nTab + "_" + descIdScheduleId + "_" + fileId + "_" + fileId + "_0";
    //var endTypeId = "COL_" + nTab + "_" + descIdTypeId + "_" + fileId + "_" + fileId + "_0";

    var hourFieldBegin = "COL_" + nTab + "_" + descIdBeginDate + "_H_" + fileId + "_" + fileId + "_0";
    var hourFieldEnd = "COL_" + nTab + "_" + descIdEndDate + "_H_" + fileId + "_" + fileId + "_0";



    var oDateTimeBeginField = document.getElementById(beginDateId);
    var strFieldValue = eDate.ConvertDisplayToBdd(oDateTimeBeginField.value);

    //if( document.getElementById(strDateEndField+'DIMG').style.visibility =='hidden' )
    //	strFieldValue = '';
    var dDateTimeBegin = eDate.Tools.GetDateFromString(strFieldValue);

    if (document.getElementById(endDateId)) {
        var oDateTimeEndField = eDate.ConvertDisplayToBdd(document.getElementById(endDateId).value);
        var dDateTimeEnd = eDate.Tools.GetDateFromString(oDateTimeEndField);
    }

    var dDateTimeOrig = new Date();
    var dDateTime = new Date();
    if (strFieldValue != '') {
        dDateTime = eDate.Tools.GetDateFromString(strFieldValue);
    }
    else {
        //Pour éviter que le catalogue de choix d'heures soit vierge
        dDateTime = new Date();
    }

    var nWeekDay = dDateTime.getDay(); // Pour détecter le changement de jour
    var strHours = "";

    if (bEndDate) {
        var strDateDescId = endDateId;
        var hourField = document.getElementById(hourFieldEnd);
    }
    else {
        var strDateDescId = beginDateId;
        var hourField = document.getElementById(hourFieldBegin);
    }

    //Pour éviter que le catalogue de choix d'heures soit vierge
    if (!dDateTimeBegin || !dDateTimeEnd) {
        var dDateTimeBegin = new Date();
        var dDateTimeEnd = new Date();
    }


    if (bEndDate && dateDiff("d", dDateTimeBegin, dDateTimeEnd) == 0) {
        // à partir de la date du début rendez-vous
        dDateTimeOrig = dDateTimeEnd;
    }
    else {
        // à partir de 00h00
        dDateTimeOrig = dDateTimeBegin;
        dDateTime.setHours(0);
        dDateTime.setMinutes(0);
        dDateTime.setSeconds(0);
    }

    var nCalendarMinutesInterval = Number(document.getElementById("CalendarInterval").value);

    var strSelectedId = "";

    for (var i = 0; i < ((24 * 60) / nCalendarMinutesInterval); i++) {
        if (i > 0)
            dDateTime = dateAdd("n", nCalendarMinutesInterval, dDateTime); // + 30 minute

        if (nWeekDay != dDateTime.getDay())
            break;

        var strTime = eDate.Tools.GetStringFromDate(dDateTime, false, true);
        if (bEndDate && dateDiff("d", dDateTimeBegin, dDateTimeEnd) == 0)
            var strDuration = " (" + (i * nCalendarMinutesInterval) / 60 + " h)";
        else
            var strDuration = "";

        var strSelected = "";
        var strValueId = "";
        if (bEndDate) {
            strValueId = "HPESV" + strTime.replace(":", ""); // HPESV = "HoursPopupEndSelectValue"
            if (dDateTimeEnd.getTime() == dDateTime.getTime()) {
                strSelected = "class=\"HoursPopupSelected\"";
                strSelectedId = strValueId;
            }
        }
        else {
            strValueId = "HPSSV" + strTime.replace(":", ""); // HPSSV = "HoursPopupStartSelectValue" 
            if (dDateTimeBegin.getTime() == dDateTime.getTime()) {
                strSelected = "class=\"HoursPopupSelected\"";
                strSelectedId = strValueId;
            }
        }

        if (!(i == 0 && bEndDate && dateDiff("d", dDateTimeBegin, dDateTimeEnd) == 0))
            strHours += "<li id=\"" + strValueId + "\" " + strSelected + " innervalue=\"" + strTime + "\">" + strTime + strDuration + "</li>";
    }
    //var oTdField = document.getElementById(strDateDescId + 'HTD');
    if (bEndDate && dateDiff("d", dDateTimeBegin, dDateTimeEnd) == 0) {
        var nWidth = 120;
    }
    else {
        var nWidth = 70;
    }

    document.getElementById("HoursPopup").parentHourField = hourField;
    document.getElementById("HoursPopup").parentHourFieldBtn = oTrigger;

    document.getElementById("HoursPopup").innerHTML = "<ul id='HoursPopupSelect' onclick=\"onChangeTime('" + document.getElementById("HoursPopup").parentHourField.id + "',this," + bEndDate + ", event," + nTab + ");\" class='HoursPopupSelect'>" + strHours + "</ul>";

    //document.getElementById("md_pl-base").setAttribute("onclick", "hideHourPopup()");
    setTimeout(function () { oldDocOnclick = document.onclick; document.onclick = hideHourPopup; }, 300);

    // On provoque le recalcul de la position lors du scroll
    var oScrollableParentElements = new Array();
    oScrollableParentElements.push(document.getElementById('container'));
    // Parcours des éléments sur lesquels rattacher l'évènement
    for (var i = 0; i < oScrollableParentElements.length; i++) {
        if (
            oScrollableParentElements[i] != null &&
            typeof (oScrollableParentElements[i]) != 'undefined' /*&&
                                        (
                                            oScrollableParentElements[i].style.overflow == 'auto' ||
                                            oScrollableParentElements[i].style.overflowX == 'auto' ||
                                            oScrollableParentElements[i].style.overflowY == 'auto'
                                        )
                                        */
        ) {
            setEventListener(oScrollableParentElements[i], 'scroll', onContainerScroll, true);
        }
    }

    // Puis on affiche le contrôle à la bonne position
    setHoursPopupDisplay();

    //KHA demande 63 758: ces lignes provoquent un déplacement intempestif de la ModalDialog de la fiche planning
    //par ailleurs je ne vois pas l'intérêt de faire une scroll alors que la rubrique se trouve toujours en haut de la fiche (cette mise en page étant forcée)
    // On positionne l'ascenseur en fonction de l'éventuelle valeur sélectionnée    
    var oSelectedValue = document.getElementById(strSelectedId);
    var popup = document.getElementById("HoursPopup");
    popup.scrollTop = oSelectedValue.offsetTop;
    //if (oSelectedValue) {
    //    oSelectedValue.scrollIntoView(true);
    //    // Et on empêche ce positionnement d'agir sur le conteneur
    //    document.getElementById('md_pl-base').scrollTop = 0;
    //}

}

function setHoursPopupDisplay() {
    var oObj = document.getElementById("HoursPopup");

    if (!oObj || (!oObj.parentHourField && !oObj.parentHourFieldBtn))
        return;

    var obj_pos = getAbsolutePosition(oObj.parentHourField);
    var btn_pos = getAbsolutePosition(oObj.parentHourFieldBtn);

    oObj.style.position = "absolute";
    oObj.style.display = "block";
    ///AABBA demande #76 429 la popup d'heure est mal positionnée dans l'interface de planning 
    oObj.style.top = (Number(obj_pos.y) - Number(obj_pos.y) + 69) + "px";
    oObj.style.left = Number(obj_pos.x) + "px";

    oObj.style.width = (Number(obj_pos.w) + Number(btn_pos.w) - 7) + "px"; // - 7 car le bouton est positionné légèrement en-dessous du contrôle de saisie de date
}

function onContainerScroll(e) {
    var oScrolledObj = null;
    if (e) {
        if (e.target)
            oScrolledObj = e.target;
        else {
            if (e.srcElement)
                oScrolledObj = e.srcElement;
        }
    }
    else {
        if (window.event)
            oScrolledObj = window.event.srcElement;
    }
    if (oScrolledObj && oScrolledObj.id != "HoursPopup" && oScrolledObj.style.display != 'none') {
        setHoursPopupDisplay();
    }
}
//GCH - #36012 - Internationnalisation - Planning - ok
function onChangeTime(inputId, lst, bEndDate, e, nTab) {
    if (!nTab)
        nTab = Number(nGlobalActiveTab);

    var oSourceObj = e.target || e.srcElement;
    document.getElementById('HoursPopup').style.display = 'none';
    document.getElementById(inputId).value = getAttributeValue(oSourceObj, "innervalue");

    //Mise à jour du champs date

    var fileId = 0;
    var fileDiv = document.getElementById("fileDiv_" + nTab);
    if (fileDiv != null)
        fileId = fileDiv.getAttribute("fid");
    if (bEndDate)
        var descIdDate = Number(nTab) + endDateDescIdTpl;
    else
        var descIdDate = document.getElementById("BeginDateDescId").value;


    var sDateId = "COL_" + nTab + "_" + descIdDate + "_D_" + fileId + "_" + fileId + "_0";
    var sHourId = "COL_" + nTab + "_" + descIdDate + "_H_" + fileId + "_" + fileId + "_0";
    var sHiddenDateId = "COL_" + nTab + "_" + descIdDate + "_" + fileId + "_" + fileId + "_0";
    document.getElementById(sHiddenDateId).value = document.getElementById(sDateId).value + " " + document.getElementById(sHourId).value;
    validDateFields(nTab, null, null, sHourId);

}


function cancelChgAdrType() {
    //On récupère la rubrique système adr92 afin de pouvoir la mettre à jour
    var oAdrTypeField = document.querySelector("input[ename='COL_400_492']");
    if (!oAdrTypeField)
        return;

    var chkPro = document.getElementById("COL_400_492_0");
    chkPro = (chkPro == null || typeof (chkPro) == "undefined" || chkPro.tagName != "INPUT") ? null : chkPro;
    var chkPerso = document.getElementById("COL_400_492_1");
    chkPerso = (chkPerso == null || typeof (chkPerso) == "undefined" || chkPerso.tagName != "INPUT") ? null : chkPerso;
    var chkWithout = document.getElementById("COL_400_492_2");
    chkWithout = (chkWithout == null || typeof (chkWithout) == "undefined" || chkWithout.tagName != "INPUT") ? null : chkWithout;

    // Dans certains cas, chkWithout peut être null
    if (chkPro == null || chkPerso == null)
        return;

    chkPro.checked = false;
    chkPerso.checked = false;
    if (chkWithout != null)
        chkWithout.checked = false;

    var oldVal = getAttributeValue(oAdrTypeField, "value");

    switch (oldVal) {
        case "0":
            chkPro.checked = true;
            break;
        case "1":
            chkPerso.checked = true;
            break;
        case "2":
            if (chkWithout != null)
                chkWithout.checked = true;
            break;
    }
}

// Si on connait pas d'adresse (UNKNOWN) alors on ne peut pas la créer. 
// Cela correspond à la creation de pp sans addresse.
var ADR_TYPE = { PRO: "0", PERSO: "1", WITHOUT: "2" };

/// <summary>
/// Avertir l'utilisateur du vidage des champs
/// </summary>
function warn(sRadioId) {
    //Permet de connaitre si fiche en creation , modification ou consultation 
    var sCurrentView = getCurrentView(document);
    //On récupère la rubrique système adr92 afin de pouvoir la mettre à jour
    var oAdrTypeField = document.querySelector("input[ename='COL_400_492']");
    if (!oAdrTypeField)
        return;

    //On récupère le choix de l'utilisateur pour le type d'adresse
    var oSelectedAdrType = document.getElementById(sRadioId);
    if (typeof (oSelectedAdrType) == "undefined" || oSelectedAdrType == null || oSelectedAdrType.tagName != "INPUT")
        return;

    var oldVal = getAttributeValue(oAdrTypeField, "value");
    var newVal = getAttributeValue(oSelectedAdrType, "value");

    // Si l'utilisateur clique sur le même radion button, on en tient pas compte. 
    if (oldVal == newVal)
        return;

    if (newVal != oldVal && oldVal != ADR_TYPE.WITHOUT && sCurrentView == "FILE_CREATION")
        eConfirm(1, top._res_6740, top._res_6739, top._res_6741, null, null, function () { chgAdrTyp(oAdrTypeField, oSelectedAdrType, sCurrentView); }, cancelChgAdrType);
    else
        chgAdrTyp(oAdrTypeField, oSelectedAdrType, sCurrentView);
}

/// <summary>
/// En fonction de choix de l'utilisateur sur le type d'adresse : pro/perso/sanAdr on met a jour la rubrique système adr92
/// et appliquer les règles/conditions si necessaire
/// </summary>
function chgAdrTyp(oAdrTypeField, oSelectedAdrType, sCurrentView) {

    var oldVal = getAttributeValue(oAdrTypeField, "value");
    var newVal = getAttributeValue(oSelectedAdrType, "value");
    var fileId = this.GetTabFileId(400);
    // Mise à jour du DOM de la nouvelle valeur
    setAttributeValue(oAdrTypeField, "value", newVal);

    // Sans ADR : Dans le cas ou l'utilisateur ne connait pas une adresse, il choisit sans adresse du coup le type d'adresse est inconnu.
    // Mise a jour de la rubrique adr92
    // Ne pas afficher les rubriques de la fiche adresse
    if (newVal == ADR_TYPE.WITHOUT && nGlobalActiveTab == 200) {
        dispalyAdrFileParts(false);
        return;
    }

    // choix Perso/Pro
    // Mise a jour de la rubrique adr92
    // ApplyRuleOnBlank si switch [perso <-> pro]
    if (newVal == ADR_TYPE.PRO || newVal == ADR_TYPE.PERSO) {
        // Dans le cas d'un changement du type d'adresse, on vide les rubriques postales et le telephone
        for (i = 2; i <= 10; i++) {
            if (i == 8)
                continue;

            var eltVal = document.getElementById("COL_400_" + (400 + i) + "_" + fileId + "_" + fileId + "_0");
            if (eltVal && sCurrentView == "FILE_CREATION") {
                eltVal.value = "";
                eltVal.setAttribute("dbv", "");
            }
        }

        // On vide aussi le ratachement éventuel a PM
        var eltPmLnk = document.querySelector("input[ename='COL_400_301']");
        if (eltPmLnk) {
            var nFileId = getAttributeValue(eltPmLnk, "dbv");
            if (nFileId != "") {
                var oLnkId = document.getElementById("lnkid_400");
                if (oLnkId)
                    oLnkId.value = oLnkId.value.replace("300=" + nFileId + ";", "300=0;");

                eltPmLnk.name = eltPmLnk.name.replace("_" + nFileId + "_", "_0_");
                eltPmLnk.id = eltPmLnk.id.replace("_" + nFileId + "_", "_0_");
            }

            eltPmLnk.value = "";
            eltPmLnk.setAttribute("dbv", "");
        }
        //BSE #46194  Valeur par défaut champs de l'adresse postale
        //NHA - #1109 - Tache : Transformer Adresse Perso en adresse Pro (et inversement) 
        //on conserve les champs adresse en cas de modification
        if (sCurrentView == "FILE_MODIFICATION") {
            var arrP = [];
            arrP.push({ name: "fromchgtype", value: "0" });
        }
        else {
            var arrP = [];
            arrP.push({ name: "fromchgtype", value: "1" });
        }
        applyRuleOnBlank(nGlobalActiveTab, undefined, fileId, undefined, "divFilePart1", arrP);
    }
}

/// <summary>
/// Affiche ou cache les parties constituant le mode fiche de Adresse
/// </summary> 
function dispalyAdrFileParts(bDisplay) {
    var ftm = document.getElementById("ftm_" + 400);
    if (ftm)
        ftm.style.display = bDisplay ? "" : "none";

    var fts = document.getElementById("fts_" + 400);
    if (fts)
        fts.style.display = bDisplay ? "" : "none";

    var ftn = document.getElementById("ftn_" + 400);
    if (ftn)
        ftn.style.display = bDisplay ? "" : "none";
}


function isFileTabInBkm() {
    var oInpt = document.getElementById("bftbkm");
    if (!oInpt)
        return false;

    return document.getElementById("bftbkm").value == "1";
}

function isAutoSave(nTab) {
    var bAutoSave = getAttributeValue(document.getElementById("mainDiv"), "autosv") == "1";
    if (bAutoSave)
        return bAutoSave;

    if (nTab != nGlobalActiveTab) {
        if (document.getElementById("fileDiv_" + nGlobalActiveTab)) {
            if (isBkmFile(nTab))
                return true;
        }
    }

    return false;
}


// permet de reloader le cadre des informations parentes en pied ou en tete de fiche
function rldPrtInfo(nTab, nFileId) {
    var oDiv = document.getElementById("divPrt_" + nTab);
    if (typeof (oDiv) == "undefined")
        return;

    var sFormat = getAttributeValue(oDiv, 'fmt');
    if (sFormat == '')
        return;


    var oFileUpdater = getFileUpdater(nTab, nFileId, null);
    oFileUpdater.addParam("pformat", sFormat, "post");

    oFileUpdater.send(function (oRes) { updtPrtInfo(oRes, oDiv, nTab); });

}

function updtPrtInfo(oRes, oDiv, nTab) {
    if (oRes != null) {
        oDiv.innerHTML = oRes;
    }

    initFileTb("ftp_" + nTab);

    var pTab = document.getElementById("ftp_" + nTab);
    var fileDiv = document.getElementById("fileDiv_" + nTab);
    var oLinks = document.getElementById("lnkid_" + nTab);
    var aLinks = oLinks.value.split(";");

    if (!pTab || !fileDiv)
        return;
    var pmid = 0, ppid = 0;
    var inpPM = pTab.querySelector("input[ename=COL_" + nTab + "_301]");
    if (inpPM) {
        pmid = getAttributeValue(inpPM, "lnkid");
    }

    var inpPP = pTab.querySelector("input[ename=COL_" + nTab + "_201]");
    if (inpPP) {
        ppid = getAttributeValue(inpPP, "lnkid");
    }

    for (var i = 0; i < aLinks.length; i++) {
        var alink = aLinks[i].split("=");
        if (alink[0] == "200")
            alink[1] = ppid;
        else if (alink[0] == "300")
            alink[1] = pmid;

        aLinks[i] = alink.join("=");
    }

    oLinks.value = aLinks.join(";");

}

function initBkms(loadFrom) {

    var oDivBkmCtner = document.getElementById("divBkmCtner");
    if (!oDivBkmCtner) {
        eAlert(0, top._res_72, top._res_6573, top._res_6574);
        return;
    }

    /* Dans le cas où on initialise des signets "Liste" (autres que "Détails"), ces signets se trouvent dans un conteneur "divBkmCtner".
    On initialise donc chaque signet enfant */
    if (oDivBkmCtner.children.length > 0) {
        for (var i = 0; i < oDivBkmCtner.children.length; i++) {
            var oDivBkm = oDivBkmCtner.children[i];
            var sDivBkmId = oDivBkm.id;

            if (sDivBkmId == "")
                continue;

            var aId = sDivBkmId.split("_");

            if (aId.length < 2 || aId[0] != "bkm" || isNaN(aId[1]))
                continue;

            var nBkm = getNumber(aId[1]);

            // MAB - Met à jour le signet Notes sélectionné (déclenche le JS permettant d'instancier l'éditeur)
            if (oDivBkm.getAttribute("memobkm") == "1") {
                loadMemoBkm(nBkm);
            }
            else if (isBkmDisc(null, oDivBkm)) {
                var nCommDid = getNumber(getAttributeValue(oDivBkm, "did"));
                loadMemoBkm(nCommDid);
                aBkmMemoEditors[nCommDid].fileId = 0;
                aBkmMemoEditors[nCommDid].disc = true;
                aBkmMemoEditors[nCommDid].dtdid = getNumber(getAttributeValue(oDivBkm, "dtdid"));
                aBkmMemoEditors[nCommDid].usrdid = getNumber(getAttributeValue(oDivBkm, "usrdid"));

            }
            // KHA 2015/07/27 - Signet Mode fiche incrusté
            else if (isBkmFile(nBkm)) {
                updateFile(null, nBkm, getBkmCurrentFileId(nBkm), 3, false, null, null, false, LOADFILEFROM.BKMFILE);
            }
            else {
                listLoaded(nBkm, 1);
                updateComputedCol(nBkm);
                adjustLastCol(nBkm);
            }
        }
    }
    /*
    MAB - 2014-09-16 - #33 288 - Lorsqu'on réinitialise la liste des signets alors que le signet "Détails" est affiché, il faut réinitialiser
    les éditeurs et éléments cliquables (ex : titres séparateurs).
    Pour les signets mode "Liste", cela est fait via listLoaded() ci-dessus, mais dans le cas de "Détails",
    le conteneur "divBkmCtner" ne contient pas de signet "Liste". Il faut donc appeler la méthode fileInitObject() qui réalise les mêmes
    opérations (et qui est appelée, elle, par updateFile() au chargement de la fiche)
    */
    else {
        fileInitObject(nGlobalActiveTab, true, loadFrom);
    }

}
///Tooltip affichant les proprités de la fiche
var oToolTip = (function () {
    return {
        show: function (span, nTab, nFileId) {

            var propDescIds = document.getElementById("fileProp_" + nTab);
            if (propDescIds == null)
                return;

            var fields = getFieldsInfos(nTab, nFileId, propDescIds.value);

            // TODO en attente de la mock-up graphique..., à la place, j'utilise l'attribut title de span
            // que je rafraichit à chaque mouseover, bon bah c'est temporaire...
            var propreties = "";
            var fld = null;
            for (var index = 0; index < fields.length; index++) {
                fld = fields[index];
                propreties += fld.lib + " : " + fld.newLabel + "\n";
            }

            setAttributeValue(span, "title", propreties);


        },
        hide: function () { },
        reset: function () { }
    }
}());


var modPties;

function shPties(nTab, nFileId) {
    var oHidVal = document.getElementById("hv_" + nTab);
    var oTbPties = document.getElementById("pty_" + nTab);

    if (!oTbPties)
        return;

    var bPlanning = false;
    var nWidth = 500;
    var nHeight = 330;

    try {   //Spécificité de dimension pour PLANNING qui contient moins de champs
        var oMainDiv = document.getElementById("fileDiv_" + nTab);
        var sType = oMainDiv.getAttribute("edntype");
        var bCalendar = getAttributeValue(oMainDiv, "ecal") == "1";
        bPlanning = (sType == "1") && bCalendar;
    }
    catch (exx) {
        eAlert(0, top._res_54, top._res_72, top._res_6575 + "<br/>" + top._res_6576 + " : " + exx);
    }

    if (bPlanning) { //1 pour PLANNING
        nWidth = 500;
        nHeight = 250;
    }
    modPties = new eModalDialog(top._res_54, 0, "eBlank.aspx", nWidth, nHeight);

    modPties.ErrorCallBack = function () { eAlert(0, top._res_54, top._res_72, top._res_6575); };

    modPties.setElement(oTbPties);
    modPties.show();

    if (bPlanning) { //1 pour PLANNING
        modPties.addButton(top._res_30, function () { cancelPties(modPties, oHidVal); }, 'button-gray', null, "cancel");
    }
    else {
        modPties.addButton(top._res_29, function () { cancelPties(modPties, oHidVal); }, 'button-gray', null, "cancel");
        modPties.addButton(top._res_28, function () { validPties(nTab, nFileId, oHidVal, modPties); }, 'button-green');
    }

    modPties.onIframeLoadComplete = function () {
        var oDoc = modPties.getIframe().document;

        // Maj de JS
        addScript("eTools", "PTY", null, oDoc);
        addScript("ePopup", "PTY", null, oDoc);
        addScript("eFieldEditor", "PTY", null, oDoc);
        addScript("eFile", "PTY", null, oDoc);
        addScript("eAutoCompletion", "PTY", null, oDoc);
        addScript("eEngine", "PTY", null, oDoc);
        addScript("eModalDialog", "PTY", null, oDoc);
        addScript("eCatalogUser", "PTY", null, oDoc);
        addScript("eUpdater", "PTY", null, oDoc);
        addScript("eContextMenu", "PTY", oDoc);
        addScript("eMain", "PTY", function () { modPties.getIframe().initEditors(); }, oDoc);
        //addScript("eAdminStoreFile", "PTY", oDoc)

        // Maj de CSS
        addCss("eudoFont", "PTY", oDoc);
        addCss("eMain", "PTY", oDoc);
        addCss("eControl", "PTY", oDoc);
        addCss("eFile", "PTY", oDoc);
        addCss("eEditFile", "PTY", oDoc);
        addCss("eCatalog", "PTY", oDoc);
        addCss("eList", "PTY", oDoc);
        addCss("eContextMenu", "PTY", oDoc);
        addCss("theme", "PTY", oDoc);

        //[MOU- 23/08/2013 cf.24569] CAS SPECIAL:
        //On ajoute une div englobant la table qui contient les proprités de la fiche
        //afin d'afficher le catalogue utilisateur directement sans passer par les MRU.
        var container = document.createElement('div');
        container.id = "filePropContainer";
        container.appendChild(modPties.eltToDisp);

        oDoc.body.appendChild(container);
        //        modPties.getIframe().initEditors();

    }


}


function tooltip(nTab, nFileId) {
    var oTbPties = document.getElementById("pty_" + nTab);
}

function cancelPties(modPties, oHidVal) {
    oHidVal.innerHTML += modPties.sOldHTML;
    modPties.hide();
    top.clearHeader("PTY", "CSS");
}

var FLD_OWNER = 99;
var FLD_MULTI_OWNER = 88;
var FLD_TPL_MULTI_OWNER = 92;
var FLD_CONFIDENTIAL = 84;
var FLD_DATE_CREATE = 95;
var FLD_DATE_MODIFY = 96;
var FLD_USER_CREATE = 97;
var FLD_USER_MODIFY = 98;
var FLD_GEOGRAPHY = 74;

function validPties(nTab, nFileId, oHidVal, modPties) {
    var oElt = modPties.eltToDisp;
    var oDoc = modPties.getIframe().document;
    var aDescids = new Array(FLD_OWNER, FLD_MULTI_OWNER, FLD_TPL_MULTI_OWNER, FLD_CONFIDENTIAL, FLD_GEOGRAPHY);
    var aFields = new Array();
    var bApply = false;
    var bConfidChange = false;
    for (var i = 0; i < aDescids.length; i++) {
        var nFieldDescId = parseInt(nTab) + parseInt(aDescids[i]);
        var oField = oDoc.getElementById("COL_" + nTab + "_" + nFieldDescId + "_" + nFileId + "_" + nFileId + "_0");
        var oLabel = oDoc.getElementById("COL_" + nTab + "_" + nFieldDescId);

        if (!oField)
            continue;

        if (getAttributeValue(oLabel, "rul") == "1") {
            bApply = true;
        }
        if (getAttributeValue(oField, "ero") == "1")    //droit de modif manquant
            continue;

        var fldEngine = getFldEngFromElt(oField);
        if (fldEngine != null) {
            aFields.push(fldEngine);
            if (aDescids[i] == FLD_CONFIDENTIAL)
                bConfidChange = true;
        }
    }
    var bAutoSave = isAutoSave(nTab);

    if (isPopup() && !bAutoSave) {
        oHidVal.appendChild(oElt);
        if (bApply)
            applyRuleOnBlank(nTab, null, nFileId)
    }
    else {
        //On envoi la mise à jours seulement si au moins une modification est demandée
        if (aFields.length > 0) {
            if (bConfidChange) {
                validateFile(nTab, nFileId, aFields, false, null, RefreshFile);
            }
            else {
                var afterValidate = function (callBackObj) { refreshPties(callBackObj); }
                validateFile(nTab, nFileId, aFields, false, null, afterValidate);
            }
        }
    }

    modPties.hide();

}

function refreshPties(callBackObj) {

    var eFileUpdater = getFileAsyncUpd(callBackObj.tab, callBackObj.fid, REQ_PART_PROPERTIES);

    eFileUpdater.send(function (oRes) { updatePties(oRes, callBackObj) });



}

function updatePties(oRes, callBackObj) {

    var oHidVal = document.getElementById("hv_" + callBackObj.tab);

    if (!oHidVal) {
        eAlert(0, top._res_72, top._res_6573, top._res_6577);
        return;
    }

    oHidVal.innerHTML += oRes;

    var oDivPties = document.getElementById("divPty_" + callBackObj.tab);

    if (!oDivPties) {
        eAlert(0, top._res_72, top._res_6573, top._res_6577);
        return;
    }
    oDivPties.outerHTML = oDivPties.innerHTML;

    /* #59 797 - Pas de tentative de MAJ du champ Appartient à si celui-ci n'est pas affiché (pas de droits de visu) */
    if (!isPopup() && !document.getElementById("ownerFieldHidden")) {
        var oDisp99 = document.getElementById("file99Value");

        if (!oDisp99) {
            eAlert(0, top._res_72, top._res_6573, top._res_6577);
            return;
        }

        var sInp99ValueId = "COL_" + callBackObj.tab + "_" + (getNumber(callBackObj.tab) + FLD_OWNER) + "_" + callBackObj.fid + "_" + callBackObj.fid + "_0";
        var oInp99Value = document.getElementById(sInp99ValueId);
        if (!oInp99Value) {
            eAlert(0, top._res_72, top._res_6573, top._res_6577);
            return;
        }

        oDisp99.innerHTML = oInp99Value.value;
    }
}



function onDeleteButton(nTab, nFileId, eModFile) {

    if (nTab == TAB_PJ) {
        var refinput = document.getElementById(TAB_PJ + "_LinkedFile");
        var linkedFile = JSON.parse(refinput?.value ?? "");
        if (linkedFile.LinkedTab) {
            top.DeletePJ(linkedFile.LinkedTab, linkedFile.LinkedFileId, nFileId, true, null);
            eModFile.hide();
        }

        return;
    }

    //TODO : Pourquoi appelle t'on deleteTpl (sur ePlanning.js)  qui est dédié a la suppression des template de type planning ?
    if (typeof (top.deleteTpl) == 'function')
        top.deleteTpl(nTab, nFileId, eModFile);
}

function onDeleteCalendarButton(nTab, nFileId, eModFile, openSerie) {
    top.deleteCalendarTpl(nTab, nFileId, eModFile, openSerie);
}

function onPropertiesButton(nTab, nFileId) {
    if (typeof (shPties) == 'function')
        shPties(nTab, nFileId);
}
function onPjButton(strTargetBtnTxtId) {
    if (typeof (showPJFromTpl) == 'function')
        showPJFromTpl('tpl', strTargetBtnTxtId);
}
function onPrintButton() {
    if (typeof (printFile) == 'function')
        printFile();
}
function onSendMailButton(strMailTo, nTab, mailType) {
    if (typeof (sendMailTo) == 'function')
        sendMailTo(strMailTo, nTab, 'mail', mailType);
}

function addBkmToCloneLst(chk) {
    var oHidInp = document.getElementById("bkms");
    if (!oHidInp)
        return;

    oHidInp.value = "";

    var chkSel = document.getElementById("SelAll");
    var chkUnSel = document.getElementById("UnSelAll");

    if (getAttributeValue(chk, 'chk') == "1" && getAttributeValue(chkUnSel, 'chk') == "1")
        chgChk(chkUnSel);

    if (getAttributeValue(chk, 'chk') != "1" && getAttributeValue(chkSel, 'chk') == "1")
        chgChk(chkSel);

    var oTbBkms = document.getElementById("BkmsCloneTb");

    if (!oTbBkms)
        return;
    var nbCk = oTbBkms.rows.length * oTbBkms.rows[0].cells.length;

    if (!nbCk || nbCk == 0) {
        return;
    }

    for (var i = 0; i < nbCk; i++) {
        var oChk = document.getElementById("bkm" + i);
        if (!oChk)
            continue;

        if (getAttributeValue(oChk, "chk") == "1") {
            if (oHidInp.value.length > 0)
                oHidInp.value += ";";

            oHidInp.value += getAttributeValue(oChk, "bkm");
        }
    }
}

function selectAll(obj) {
    if (getAttributeValue(obj, "chk") != "1")
        return;

    var bSel = false;

    if (obj.id == "UnSelAll") {
        //selectAll.setAttribute("chk", (!bSel) == true ? 1 : 0);
        bSel = false;
    }
    /*
    selectAll = document.getElementById("SelAll");
    if (selectAll) {
    selectAll.setAttribute("chk", bSel == true ? 1 : 0);
    }
    */

    if (obj.id == "SelAll") {
        bSel = true;
    }

    var oTbBkms = document.getElementById("BkmsCloneTb");

    if (!oTbBkms)
        return;

    var nbCk = oTbBkms.rows.length * oTbBkms.rows[0].cells.length;

    if (!nbCk || nbCk == 0) {
        return;
    }

    for (var i = 0; i < nbCk; i++) {

        var oChk = document.getElementById("bkm" + i);

        if (!oChk)
            continue;

        if ((bSel && getAttributeValue(oChk, 'chk') != "1") || (!bSel && getAttributeValue(oChk, 'chk') == "1"))
            chgChk(oChk);

        addBkmToCloneLst(oChk);
    }

}

/******************************************************
ZoomBookmark permet de zoomer sur un signet
sDivId : id de l'élement à agrandir
pW : pourcentage de la largeur de l'écran que doit occuper la fenêtre
pH : pourcentage de la longueur de l'écran que doit occuper la fenêtre
******************************************************/
function ZoomBookmark(sDivId, pW, pH, srcElment) {
    var oDivStats = document.getElementById(sDivId);
    var bZoomed = getAttributeValue(oDivStats, 'zoom') == '1';
    var sStyle = "";


    var paramWin = top.getParamWindow();
    var objThm = JSON.parse(paramWin.GetParam("currenttheme").toString());

    if (typeof (pW) == "undefined" || pW == 0)
        pW = 50;

    if (typeof (pH) == "undefined" || pH == 0)
        pH = 75;


    var bDivCampagneChart = false;
    var divChart = oDivStats.querySelectorAll(".SyncFusionChartContainer[syncFusionChart='1']")[0];//oDivStats.querySelectorAll("div[id*='Chart'")[0];//
    if (divChart) {
        bDivCampagneChart = true;

        var statBlock = oDivStats.querySelectorAll("div[class='StatsRendBlock'")[0];
        if (statBlock) {
            var id = statBlock.id;
            var hiddenInputStatBlock = statBlock.querySelectorAll("input[id='" + id + "Hidden'")[0];
            if (hiddenInputStatBlock && !bZoomed) {
                hiddenInputStatBlock.setAttribute('w', statBlock.clientWidth);
                hiddenInputStatBlock.setAttribute('h', statBlock.clientHeight + 100);
            }
        }


    }



    if (bZoomed) {

        //Restauration des styles initiaux
        sStyle = getAttributeValue(oDivStats, 'orgstl');

        var aStyles = sStyle.split('|');
        for (i = 0; i < aStyles.length; i++) {
            var aStyle = aStyles[i].split(':');
            switch (aStyle[0]) {
                case "p":
                    oDivStats.style.position = aStyle[1];
                    break;
                case "t":
                    oDivStats.style.top = aStyle[1];
                    break;
                case "l":
                    oDivStats.style.left = aStyle[1];
                    break;
                case "w":
                    if (aStyle[1] == '' && hiddenInputStatBlock)
                        aStyle[1] = getAttributeValue(hiddenInputStatBlock, 'w') + 'px';
                    oDivStats.style.width = aStyle[1];
                    break;
                case "h":
                    if (aStyle[1] == '' && hiddenInputStatBlock)
                        aStyle[1] = getAttributeValue(hiddenInputStatBlock, 'h') + 'px';

                    oDivStats.style.height = aStyle[1];
                    break;
                case "z":
                    oDivStats.style.zIndex = aStyle[1];
                    break;

                default:

            }
        }

        oDivStats.setAttribute('zoom', '0');

        if (srcElment != "undefined" && srcElment != null)
            switchClass(srcElment, "icon-search-minus", "icon-search-plus");
    }
    else {

        //Sauvegarde des styles initiaux

        sStyle = "p:" + oDivStats.style.position + "|t:" + oDivStats.style.top + "|h:" + oDivStats.style.height + "|l:" + oDivStats.style.left + "|w:" + oDivStats.style.width + "|z:" + oDivStats.style.zIndex;

        var oWinDim = getWindowWH(top);

        var winH = oWinDim[1];
        var winW = oWinDim[0];

        var h = Math.floor(winH * pH / 100);
        var nTop = Math.floor((winH - h) / 2);

        var w = Math.floor(winW * pW / 100)
        var nLeft = Math.floor((winW - w) / 2);

        oDivStats.style.position = 'absolute';
        oDivStats.style.top = nTop + 'px';
        oDivStats.style.height = h + 'px';
        oDivStats.style.left = nLeft + 'px';
        oDivStats.style.width = w + 'px';
        /*ELAIZ - Régression 76477 - changement du z-index afin de passer devant le menu de droite sur eudonet x */

        if (objThm.Version > 1)
            oDivStats.style.zIndex = '11';
        else
            oDivStats.style.zIndex = '5';
        /*ELAIZ - Régression 76477 - changement du z-index afin de passer devant le menu de droite sur eudonet x */

        oDivStats.setAttribute('zoom', '1');
        oDivStats.setAttribute('orgstl', sStyle);

        if (srcElment != "undefined" && srcElment != null)
            switchClass(srcElment, "icon-search-plus", "icon-search-minus");


        if (bDivCampagneChart)
            divChart.setAttribute('syncFusionChart', 1);



    }

    resizeSyncFusionChart(oDivStats, bZoomed);

}

function resizeSyncFusionChart(oBox, bZoomed) {

    try {
        var chart;
        var divChart = oBox.querySelector(".SyncFusionChartContainer[syncFusionChart='1']");

        if (typeof (divChart) != "undefined" && divChart != null) {
            var id = divChart.id;
            if (id) {
                chart = $("#" + id).data('ejChart');
                if (chart) {
                    var h = oBox.clientHeight - 100;
                    var w = oBox.clientWidth - 10;
                    var element = top.document.getElementById(id);
                    if (element) {
                        element.style.minHeight = h + 'px';
                        element.style.minWidth = w + 'px';
                    }

                    chart.size = { height: h.toString(), width: w.toString() };
                    chart.redraw();
                } else {
                    chart = $("#" + id).data("ejCircularGauge");
                    if (chart && typeof eChart == 'function') {
                        divChart.style.height = oBox.clientHeight - (bZoomed ? 0 : 20) + 'px';
                        divChart.style.width = oBox.clientWidth - (bZoomed ? 0 : 20) + 'px';
                        eChart(oBox, id).redraw();
                    }
                }
            }
        }
    } catch (e) {

    }

}
var MAILINGTREATMENT_CANCEL = 4;
/*Campagne mdoe fiche*/
var alertAttentMailing = null;
//CancelDeleyedCampaign : Annuler une campagne
//  nCampaignId : id de la campagne
//  sSender : e-mail expéditeur (#39983)
//  bConfirm : indique si l'annulation a été confirmée par l'utilisateur
function CancelDeleyedCampaign(nCampaignId, sSenderEmail, bConfirm) {
    if (!bConfirm) {
        //Popup êtes-vous sure de vouloir arrêter l'e-mailing ?
        eConfirm(1, top._res_14, top._res_6619, "", 500, 200,
            function () { CancelDeleyedCampaign(nCampaignId, sSenderEmail, true); });
        return;
    }
    else {
        //Affichage de pop up d'attente
        OpenAttentMailing();

        //Début Appel du manager ***************************************************************
        var upd = new eUpdater("mgr/eMailingManager.ashx", 0);
        upd.addParam("fileid", nCampaignId, "post");
        upd.addParam("operation", MAILINGTREATMENT_CANCEL, "post");   //Annuler la campagne
        upd.addParam("sender", sSenderEmail, "post");

        //Créer une methode en cas d'une erreur il l'execute
        upd.ErrorCallBack = CloseAttentMailing;
        upd.send(CallBackCancelDeleyedCampaign);
        //Fin Appel du manager ***************************************************************
    }
}

function CallBackCancelDeleyedCampaign(oDoc) {
    CloseAttentMailing();
    //Rafraichier la fiche
    RefreshFile();
}
//Affichage de pop up d'attente
function OpenAttentMailing() {
    alertAttentMailing = eAlert(3, '', top._res_307, '', null, null, null);
    top.setWait(true);
}
//Fermeture de pop up d'attente
function CloseAttentMailing() {
    alertAttentMailing.hide();
    top.setWait(false);
}


//*************Gestion des signets en mode fiche incrusté***************
function chgBkmViewMode(nTab, nBkm, nMode, nFileId) {
    var updatePref = "tab=" + nTab
        + ";$;viewmode=" + nMode
        + ";$;bkm=" + nBkm;

    var eltHead = document.getElementById("BkmHead_" + nBkm);
    var eltBkmTitle = document.getElementById("bkmTitle_" + nBkm);
    var eltNumPage = document.getElementById("bkmNumPage_" + nBkm);
    var nCurrentPage = 1;

    if (nMode == 1 && !nFileId) {
        nCurrentPage = getNumber(getAttributeValue(eltHead, "pg"));
        nFileId = getNumber(getAttributeValue(eltHead, "fid"));
    }
    else if (eltNumPage) {
        nCurrentPage = getNumber(eltNumPage.value);
    }
    else if (getAttributeValue(eltBkmTitle, "pg")) {
        nCurrentPage = getNumber(getAttributeValue(eltBkmTitle, "pg"));
    }


    updateUserBkmPref(updatePref, function () { loadBkm(nBkm, nCurrentPage, true, false, nFileId); });

    //if (nFileId > 0) {
    //    updateUserBkmPref(updatePref, function () { loadBkm(nBkm, nCurrentPage, true, false, nFileId); });
    //} else {
    //    updateUserBkmPref(updatePref, function () { loadBkm(nBkm, nCurrentPage, true, false, nFileId); });
    //}
}
function getBkmFilesId(nBkm) {
    return getAttributeValue(document.getElementById("bkmTitle_" + nBkm), "lstid").split(";");
}

function getBkmCurrentFileId(nBkm) {
    return getAttributeValue(document.getElementById("fileDiv_" + nBkm), "fid");
}

function setBkmFile(nBkm, nFilePos, aFilesId) {

    if (!aFilesId)
        aFilesId = getBkmFilesId(nBkm);

    if (nFilePos < 0)
        return;

    var nTotalFiles = getNumber(GetText(document.getElementById("bkmTotalNumPage_" + nBkm)));

    if (nFilePos >= nTotalFiles)
        return;


    var fileId = 0;

    if (nFilePos < aFilesId.length)
        fileId = aFilesId[nFilePos];

    if (fileId) {
        loadFile(nBkm, fileId, 3, false, LOADFILEFROM.BKMFILE);
    }
    else {
        //les id de fiches sont récupérés par tranche pour des raison de perf
        //la fiche ne se trouve pas dans la "tranche" et il faut changer de tranche
        // on peut laisser page = 1 car le numéro de page est recalculé coté serveur
        loadBkm(nBkm, 1, true, false, 0, nFilePos);
        return;
    }


    document.getElementById("bkmNumFilePage_" + nBkm).setAttribute("value", nFilePos + 1);

    var bkmTitleBar = document.getElementById("bkmTitle_" + nBkm);
    if (bkmTitleBar) {
        if (nFilePos == nTotalFiles) {
            addClass(bkmTitleBar.querySelector("li.icon-edn-next"), "icnBkmDis");
            addClass(bkmTitleBar.querySelector("li.icon-edn-last"), "icnBkmDis");
        }
        else {
            removeClass(bkmTitleBar.querySelector("li.icon-edn-next"), "icnBkmDis");
            removeClass(bkmTitleBar.querySelector("li.icon-edn-last"), "icnBkmDis");
        }
        if (nFilePos == 0) {
            addClass(bkmTitleBar.querySelector("li.icon-edn-prev"), "icnBkmDis");
            addClass(bkmTitleBar.querySelector("li.icon-edn-first"), "icnBkmDis");
        }
        else {
            removeClass(bkmTitleBar.querySelector("li.icon-edn-prev"), "icnBkmDis");
            removeClass(bkmTitleBar.querySelector("li.icon-edn-first"), "icnBkmDis");
        }
    }

}

function selectBkmFilePage(nBkm, inpBkmPage) {
    setBkmFile(nBkm, inpBkmPage.value - 1);
}

function nextFileBkm(nBkm) {
    //var aFilesId = getBkmFilesId(nBkm);
    //var nCurrentPage = aFilesId.indexOf(getBkmCurrentFileId(nBkm));
    var nCurrentPage = getNumber(document.getElementById("bkmNumFilePage_" + nBkm).value);
    //nCurrentPage++;

    //le numéro de page est en base 1 on doit transmettre la position de la fiche en base 0
    // en théorie on devrait donc faire -1 +1 ce qui revient au même
    setBkmFile(nBkm, nCurrentPage);
}
function lastFileBkm(nBkm) {
    var nTotalFiles = getNumber(GetText(document.getElementById("bkmTotalNumPage_" + nBkm)).replace("/", ""));
    setBkmFile(nBkm, nTotalFiles - 1);
}

function prevFileBkm(nBkm) {
    //var aFilesId = getBkmFilesId(nBkm);
    //var nCurrentPage = aFilesId.indexOf(getBkmCurrentFileId(nBkm));
    //nCurrentPage représente le numero de page en base 0 c'est pourquoi on doit décrémenter de 1 (le numéro de page affiché étant en base 1)
    var nCurrentPage = getNumber(document.getElementById("bkmNumFilePage_" + nBkm).value) - 1;
    nCurrentPage--;

    setBkmFile(nBkm, nCurrentPage);
}

function firstFileBkm(nBkm) {
    setBkmFile(nBkm, 0);
}


function deleteBkmCurrentFile(nBkm) {
    deleteFile(nBkm, getBkmCurrentFileId(nBkm));
}


function openFileInBkm(nBkm, nFileId) {
    var aFilesId = getBkmFilesId(nBkm);
    var nPage = aFilesId.indexOf(nFileId);
    setBkmFile(nBkm, nPage, aFilesId);
}


//************* FIN Gestion des signets en mode fiche incrusté***************
//************* Gestion des signets Discussion***************

function del(obj) {

    var fid = getNumber(getAttributeValue(obj.parentElement, "fid"));
    var tab = getNumber(obj.parentElement.parentElement.parentElement.id.replace("bkm_", ""));
    if (tab > 0 && fid > 0)
        deleteFile(tab, fid);
}

function edit(obj) {
    eMemoEditorObject.onClick(document.getElementById(getAttributeValue(obj, "eacttg")), obj);
}

function refreshComm(nBkm, nFileId) {
    var oFileUpdater = getFileAsyncUpd(nBkm, nFileId, REQ_PART_DISC_COMM);
    oFileUpdater.send(function (oRes) { updateComm(oRes, nBkm, nFileId); });

}

function updateComm(oRes, nBkm, nFileId) {
    var divBkm = eTools.getBookmarkContainer(nBkm);
    var divComm = divBkm.querySelector("div[fid='" + nFileId + "']");
    divComm.outerHTML = oRes;
}

//************* FIN Gestion des signets Discussion***************
var modalAddMailAddr;
function addMailAddr(eltId, fileType) {
    // var fileDiv = document.getElementById("fileDiv_" + nGlobalActiveTab);

    //Infos PP
    try {
        var labPP = document.getElementById("COL_" + nGlobalActiveTab + "_201");
        var interPP = (labPP != null);
        var valPP = document.getElementById(getAttributeValue(labPP, "eltvalid"));
        var ppid = getNumber(getAttributeValue(valPP, "dbv"));
    }
    catch (e) {
    }

    //Infos PM
    try {
        var labPM = document.getElementById("COL_" + nGlobalActiveTab + "_301");
        var interPM = (labPM != null);
        var valPM = document.getElementById(getAttributeValue(labPM, "eltvalid"));
        var pmid = getNumber(getAttributeValue(valPM, "dbv"));
    }
    catch (e) {
    }

    //Infos PM
    try {
        var labADR = document.getElementById("COL_" + nGlobalActiveTab + "_301");
        var interADR = (labADR != null);
        var valADR = document.getElementById(getAttributeValue(labADR, "eltvalid"));
        var adrid = getNumber(getAttributeValue(valADR, "dbv"));
    }
    catch (e) {
    }


    //ouverture de la modale
    modalAddMailAddr = new eModalDialog(top._res_6842, 0, "eAddMailAddr.aspx", 500, 700);
    modalAddMailAddr.addParam("ppid", ppid, "post");
    modalAddMailAddr.addParam("pmid", pmid, "post");
    modalAddMailAddr.addParam("adrid", adrid, "post");
    modalAddMailAddr.addParam("filetype", fileType, "post");
    modalAddMailAddr.show();
    //Fermer
    modalAddMailAddr.addButton(top._res_30, function () { modalAddMailAddr.hide(); }, 'button-gray', null, "cancel");
    //Ajouter
    modalAddMailAddr.addButton(top._res_18, (function (id) { return function () { validMailAddr(id); } })(eltId), 'button-green', null);
    //Appliquer et fermer
    modalAddMailAddr.addButton(top._res_869, (function (id) { return function () { validMailAddr(id, true); } })(eltId), 'button-green', null);

    modalAddMailAddr.onHideFunction = function () { modalAddMailAddr = null; };

}

function validMailAddr(eltId, bClose) {
    if (typeof (modalAddMailAddr.getIframe().getAddr) != "function")
        return;
    var inptDest = document.getElementById(eltId);
    if (inptDest.tagName.toLowerCase() == "td")
        inptDest = inptDest.childNodes[0];

    var value = inptDest.value;
    var aValue = value.split("");
    if (value.length > 0 && aValue[aValue.length - 1] != ";")
        value += ";";

    value += modalAddMailAddr.getIframe().getAddr();
    inptDest.value = value;
    inptDest.setAttribute("dbv", value);

    if (bClose)
        modalAddMailAddr.hide();


}




var nsEfileJS = nsEfileJS || {};


///Met à jour le champ input caché d'un select
nsEfileJS.UpdateSelect = function (sel, oDoc, bAutoSave) {

    if (typeof bAutoSave == "undefined")
        bAutoSave = false;

    if (sel.tagName.toLowerCase() != "select")
        return;

    var sInptId = getAttributeValue(sel, "inptID");
    if (sInptId == "")
        return;

    var sValue = "";
    if (sel.multiple) {
        var result = [];
        for (var i = 0; i < sel.options.length; i++) {
            opt = sel.options[i];
            if (opt.selected) {
                result.push(opt.value || opt.text);
            }
        }
        sValue = result.join(';');
    }
    else
        sValue = sel.options[sel.selectedIndex].value;

    if (!oDoc)
        var oDoc = document;

    var oInpt = oDoc.getElementById(sInptId);
    if (oInpt) {
        oInpt.value = sValue;
        setAttributeValue(oInpt, "dbv", sValue);
    }

    var aINFOS = sInptId.split("_");
    var nDescId = aINFOS[2] * 1;

    nsEfileJS.execComplementaryActions(sel, nDescId);

    if (bAutoSave) {

        var nFileId = aINFOS[aINFOS.length - 2];
        var nTab = getTabDescid(nDescId);
        var eEngineUpdater = new eEngine();

        eEngineUpdater.Init();
        eEngineUpdater.AddOrSetParam('tab', nTab);
        eEngineUpdater.AddOrSetParam('fileId', nFileId);

        var fld = getFldEngFromElt(oInpt)
        eEngineUpdater.AddOrSetField(fld);
        eEngineUpdater.SuccessCallbackFunction = function (a, b) {
        }

        eEngineUpdater.ErrorCustomAlert = function (oObjErr, fctCallBack) {
            eAlert(oObjErr.Type, oObjErr.Title, oObjErr.Msg, oObjErr.DetailMsg);
            if (typeof (fctCallBack) == "function") {
                fctCallBack();
            }
        }

        // erreur du retour de l'engine
        eEngineUpdater.ErrorCallbackFunction = function () { top.setWait(false); };
        eEngineUpdater.AddOrSetParam('onBlurAction', '1');
        eEngineUpdater.UpdateLaunch();
    }


}

nsEfileJS.execComplementaryActions = function (selectElt, nDescId) {

    if (nDescId == 101017) {


        var productHeader = document.getElementById("COL_101000_101034");
        if (productHeader) {

            var display = "";

            if (selectElt.value == "200") {

                // Ajout de l'étoile
                //var input = document.querySelector("[inptid='" + productHeader.getAttribute("eltvalid") + "']");
                //if (input) {
                //    setAttributeValue(input, "obg", "1");
                //}
            }
            else {
                // Si l'utilisateur n'est pas du niveau 200, on cache le champ produit et on dé select la valeur selectionnée
                display = "none";

                productHeader.parentElement.querySelector("select").selectedIndex = 0

            }

            productHeader.style.display = display;
            if (productHeader.nextElementSibling && productHeader.nextElementSibling.tagName == "TD")
                productHeader.nextElementSibling.style.display = display;

        }
    }
}

//SHA : correction bug #71 117
document.addEventListener("keydown", preventAddressBarTabCycle, false);

function preventAddressBarTabCycle(e) {
    //TODO : sélectionner des rubriques spécifiques si besoin
    var focusableEls = document.querySelectorAll("td > [tabindex]");
    var firstFocusableEl = focusableEls[0];
    var lastFocusableEl = focusableEls[focusableEls.length - 1];
    var KEYCODE_TAB = 9;

    var isTabPressed = (e.key === 'Tab' || e.keyCode === KEYCODE_TAB);

    if (!isTabPressed) { return; }

    //si bouton "tab" pressé
    if (document.activeElement === lastFocusableEl) {
        firstFocusableEl.focus();
        e.preventDefault();
    }
}

function resizeAvatarAll() {
    var allAvatars = [].slice.call(document.querySelectorAll(".LNKOPENAVATAR"));
    for (var i = 0; i < allAvatars.length; i++) {
        resizeAvatar(allAvatars[i]);
    }

}

//resize l'image de l'avatar dans la td 
function resizeAvatar(oContainerElt) {

    var dim = Math.min(oContainerElt.offsetHeight, oContainerElt.clientHeight, oContainerElt.offsetWidth, oContainerElt.clientWidth) - 10; //-10 pour les padding
    var img = oContainerElt.querySelector("img");


    /* On vérifie que l'image à bien un src avant de voir si elle est chargée */
    if (img && img.src != null) {
        //on attend que l'image soit chargée pour vérifier sa taille
        img.onload = function () {
            //Si sa taille est inférierure à dim ( valeur min du conteneur) alors pas d'agrandissement
            if (img.offsetHeight < dim)
                dim = Math.min(dim, img.offsetHeight, img.offsetWidth);

            img.style.height = dim + "px";
            img.style.width = dim + "px";

        }

        //Si on a un problème de src sur l'image

        img.onerror = function () {
            img.style.height = dim + "px";
            img.style.width = dim + "px";
            img.classList.add('src-not-found')
        };
    }

}