var _searchTimer = null;
var _searchFilter = null;
var _SearchLimit = 3;
var KeyCombo = new Array();
//Id de la frame contenante
var _parentIframeId;
function FindValues(e, val) {

    var oBtnSrch = document.getElementById("lnkBtnSrch");

    if (oBtnSrch) {
        if (val.length < _SearchLimit && e != null && e.keyCode != 13 && getAttributeValue(oBtnSrch, 'srchState') == 'off')
            return false;
    }
    else {
        if (val.length != 0 && val.length < _SearchLimit && e != null && e.keyCode != 13)
            return false;
    }

    if (oBtnSrch) {
        if (getAttributeValue(oBtnSrch, 'srchState') != 'on' && val != '') {
            oBtnSrch.className = "icon-edn-cross srchFldImg";
            oBtnSrch.setAttribute('srchState', 'on');
        }
        else if (getAttributeValue(oBtnSrch, 'srchState') == 'on' && val == "") {
            oBtnSrch.className = "icon-magnifier srchFldImg";
            oBtnSrch.setAttribute('srchState', 'off');
            setFocus();
        }
    }

    if (e != null) {
        ScanString(e.keyCode);
        if (e.keyCode == 27) {
            // Echap : annuler la fenêtre ?
        }

        if (e.keyCode == 13) {
            window.clearTimeout(_searchTimer);
            _searchFilter = val;
            StartSearch();
            return true;
        }

        if (e.keyCode == 38)   //Haut
        {

        }

        if (e.keyCode == 40)   //bas
        {

        }
    }

    // Pas de recherche si la valeur a rechercher n'a pas changé
    if (val == _searchFilter)
        return false;

    window.clearTimeout(_searchTimer);
    _searchFilter = val;
    _searchTimer = window.setTimeout(StartSearch, 500);
}

///summary
///Parcours la chaine str et la compare au mot de passe paramétré
///pour afficher le son secret
///summary
function ScanString(keyPress) {
    //konamicode : M G S 1 4 0 . 8 5
    var password = [77, 71, 83, 97, 100, 96, 110, 104, 101];
    CHECKSECRET(password, keyPress, PlaySecretF);
}

///summary
///Joue le son secret html5
///summary
function PlaySecretF(bActivate) {
    try {
        var oBody = top.document.body;
        if (oBody) {
            InvertColor(oBody, !IsInvertColor(oBody));
            PlaySecretSound(oBody, "./sounds/secret2.mp3");
        }
    }
    catch (ex) {

    }
}

//Inverse les couleurs à l'écran
function InvertColor(oElem, bInvert) {
    if (oElem) {
        if (bInvert) {
            addClass(oElem, "invert");
        }
        else {
            removeClass(oElem, "invert");
        }
    }
}

//Inverse les couleurs à l'écran
function IsInvertColor(oElem) {
    if (oElem) {
        return (oElem.className.toLowerCase().indexOf("invert") >= 0);
    }
    return false;
}

// Bouton pour lancer ou annuler la recherche
function BtnSrch() {
    var oBtnSrch = document.getElementById("lnkBtnSrch");

    if (oBtnSrch && getAttributeValue(oBtnSrch, 'srchState') == 'on') {
        document.getElementById('eTxtSrch').value = '';
    }
    FindValues(null, document.getElementById('eTxtSrch').value);
};

// Donne le focus à la textbox de recherche
function setFocus() {
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
        if (document.getElementById('eTxtSrch'))
            document.getElementById('eTxtSrch').focus();
    }
}

// Lance la recherche
function StartSearch(bFromOption) {
    if (typeof (bFromOption) == "undefined")
        bFromOption = false;

    // En mode recherche, on effectue un appel AJAX pour récupérer les valeurs depuis la base.
    // A la suite de cet appel AJAX, la fonction catalogSearchTreatment() récupère les valeurs (XML), les ajoute à l'objet eCatalog (addValue()) et rappelle renderValues()
    // pour rafraîchir l'affichage avec les valeurs récupérées

    // Vide les nodes enfants et vide le contenu du div
    var oMainDiv = document.getElementById("mainDiv");
    var oPhoneticSearch = document.getElementById("chkValue_PhoneticSearch");
    var oAllRecord = document.getElementById("chkValue_AllRecord");

    try {

        var oTxtSrch = document.getElementById("eTxtSrch");
        var sSearch = oTxtSrch.value;

        if (bFromOption) {
            // Si la recherche est lancé depuis la modification d'une option, on test la taille de la valeur a rechercher avant de lancer la recherche
            if ((sSearch + "").length < 3)
                return;
        }
        setWait(true);
        var nSearchMode = 0; // 0 = Recherche standard, 1 = Recherche étendue, 2 = Recherche étendue sur toutes les rubriques
        try {
            var radios = document.getElementsByName("searchMode");
            for (var i = 0, length = radios.length; i < length; ++i) {
                if (radios[i].checked) {
                    nSearchMode = radios[i].value;
                    break;
                }
            }
        }
        catch (eSpe) { }
        var bPhoneticSearch = false;
        try {
            bPhoneticSearch = (getAttributeValue(oPhoneticSearch, "chk") == "1");
        }
        catch (eSpe) { }
        var bAllRecord = false;
        try {
            bAllRecord = (getAttributeValue(oAllRecord, "chk") == "1");
        }
        catch (eSpe) { }

        var oListUpdater = new eUpdater("mgr/eFinderManager.ashx", 1);

        var nTargetTab = getAttributeValue(oMainDiv, "tab");
        var nTargetFrom = getAttributeValue(oMainDiv, "tabfrom");

        var nDescId = getAttributeValue(oMainDiv, "did");

        var nFileId = getAttributeValue(oMainDiv, "fid");
        var nMulti = getAttributeValue(oMainDiv, "multi");

        var nUserValueIndex = getAttributeValue(oMainDiv, "uvidx");
        var listUserValueField = getAttributeValue(oMainDiv, "uvflst");

        var objHistoFilter = document.getElementById("histoFilter");
        var bHisto = false;
        if (!objHistoFilter || typeof (objHistoFilter) == "undefined")
            bHisto = true
        else
            bHisto = getAttributeValue(objHistoFilter, "ednval") == 1;

        var sListCol = getAttributeValue(oMainDiv, "listCol");
        var sListColSpec = getAttributeValue(oMainDiv, "listColSpec");

        if (sListColSpec != "") {
            if (sListCol != "")
                sListCol = sListColSpec + ";" + sListCol;
            else
                sListCol = sListColSpec;
        }

        var nMode = getAttributeValue(oMainDiv, "eMode");
        var nSearchType = getAttributeValue(oMainDiv, "SearchType");
        var nSearchLimit = getAttributeValue(oMainDiv, "SearchLimit");
        var sMRU = getAttributeValue(oMainDiv, "MRU");

        //Table sur laquelle on recherche
        oListUpdater.addParam("targetTab", nTargetTab, "post");
        //id de la fiche de départ
        oListUpdater.addParam("FileId", nFileId, "post");
        //Multiple?
        oListUpdater.addParam("Multi", nMulti, "post");
        //Champ catalogue sur la fiche de départ
        oListUpdater.addParam("targetfield", nDescId, "post");
        //Table de départ
        oListUpdater.addParam("tabfrom", nTargetFrom, "post");

        //Recherche
        oListUpdater.addParam("Search", encode(sSearch), "post");
        //Recherche étendue
        oListUpdater.addParam("SearchMode", nSearchMode, "post");
        //Recherche phonétique
        oListUpdater.addParam("PhoneticSearch", (bPhoneticSearch) ? "1" : "0", "post");
        //Recherche sur toutes les fiches <TABLE>
        oListUpdater.addParam("AllRecord", (bAllRecord) ? "1" : "0", "post");

        //Index du uservalue
        oListUpdater.addParam("UserValueIndex", nUserValueIndex, "post");
        //Liste des champs concerné par le uservalue
        oListUpdater.addParam("UserValueFieldList", encode(listUserValueField), "post");

        //HISTORIQUE
        oListUpdater.addParam("histo", bHisto ? "1" : "0", "post");

        oListUpdater.addParam("listCol", encode(sListCol), "post");
        oListUpdater.addParam("listColSpec", encode(sListColSpec), "post");

        oListUpdater.addParam("action", "searchdetail", "post");
        //Si a vrai chaque ligne est cliquable et permet de rediriger vers la fiche correspondant à la ligne sélectionnée
        oListUpdater.addParam("eMode", nMode, "post");
        oListUpdater.addParam("SearchType", nSearchType, "post");
        //Nb de caractère tapé minimum
        oListUpdater.addParam("SearchLimit", nSearchLimit, "post");

        oListUpdater.addParam("MRU", sMRU, "post");
        oListUpdater.addParam("NameOnly", getAttributeValue(document.getElementById("mt_" + nTargetTab), "eno"), "post");

        var oModal = null;

        if (top.eTabLinkCatFileEditorObject[_parentIframeId])
            oModal = top.eTabLinkCatFileEditorObject[_parentIframeId];
        if (oModal && oModal.oModalLnkFile) {
            oModal = oModal.oModalLnkFile;
        }
        //taille de la liste
        if (oModal) {
            oListUpdater.addParam("width", Math.round(oModal.absWidth), "post");
            oListUpdater.addParam("height", Math.round(oModal.absHeight), "post");
        }

        oListUpdater.ErrorCallBack = errorList;
        oListUpdater.send(updateFinderList, nTargetFrom);

    } catch (e) {

        alert('eFinder.StartSearch');
        alert(e.description);

    }

};
/*Ajouter une nouvelle fiche depuis popup*/
// bNoLoadFile : ne pas ouvrir la fiche à la validation
function AddFileFromPopup(strToolTipText, nMode, nCallFrom, bNoLoadFile) {
    // Demande #38 106 - Sur tablettes, le clavier virtuel est généralement affiché lorsqu'on clique sur le bouton Ajouter,
    // car en saisisant le critère de recherche de Contact/Société avant ajout, le focus reste généralement sur le champ de saisie.
    // Il faut faire disparaître le clavier virtuel lorsqu'on clique sur le lien, pour que la fenêtre soit correctement dimensionnée.
    // Pour cela, on donne le focus sur l'icône du bouton Ajouter, pour faire disparaître le clavier, et on temporise le
    // déclenchement de l'ouverture de la popup pour que la tablette sache détecter qu'il n'est plus affiché.
    var oSpanAddFileFromPopup = document.getElementById("spanAddFileFromPopup");
    if (isTablet() && oSpanAddFileFromPopup) {
        oSpanAddFileFromPopup.focus();
    }

    // Code à exécuter pour l'ouverture de la popup
    var oAddFileFct = function () {
        var oMainDiv = document.getElementById("mainDiv");
        var nSearchLimit = getAttributeValue(oMainDiv, "SearchLimit");
        var bAutobuildName = (getAttributeValue(oMainDiv, "AutobuildName") == "1");
        var sSearch = document.getElementById('eTxtSrch').value;
        var nTargetTab = getAttributeValue(oMainDiv, "tab");
        if (!nMode)
            nMode = getAttributeValue(oMainDiv, "eMode");
        var sOrigFrameId = getAttributeValue(oMainDiv, "ofid");

        var callBack = function (oparams, aFields) { afterValidateInPopup(oparams, nMode, aFields); };
        AddFile(nTargetTab, sSearch, strToolTipText, nSearchLimit, nMode, bAutobuildName, callBack, sOrigFrameId, nCallFrom, bNoLoadFile);
    }
    // Sur PC, le code est déclenché immédiatement
    // Sur tablettes, il est temporisé
    if (!isTablet())
        oAddFileFct();
    else
        window.setTimeout(oAddFileFct, 1000);
}

/*Ajouter une nouvelle fiche depuis MRU*/
function AddFileFromMRU(strToolTipText, jsVarName) {
    var catalogObject = window[jsVarName];

    var nSearchLimit = catalogObject.SearchLimit;
    var bAutobuildName = catalogObject.bAutobuildName;
    var sSearch = document.getElementById("eCatalogEditorSearch").value;
    var nMode = "2";
    var nTargetTab = catalogObject.tab;


    var objParent = {};
    try {
        var ename = getAttributeValue(catalogObject.sourceElement, "ename")

        var tab = 0;
        if (ename == "") {
            tab = getAttributeValue(catalogObject.sourceElement, "fldlnk")
        }
        else {
            var cols = ename.split("_");
            if (cols.length > 1)
                tab = cols[1];
        }

        var lnkid = catalogObject.parentPopup.relativeTo.querySelectorAll("[id=lnkid_" + tab + "]")[0].value
        objParent.lnkid = lnkid;
        objParent.currentTab = tab;
    }
    catch (err) {

    }
    //TO-DO ReloadList a enlever apres
    var callBack = function (oparams, aFields) { afterValidate(oparams, nMode, catalogObject, aFields); };
    AddFile(nTargetTab,
        sSearch,
        strToolTipText,
        nSearchLimit,
        nMode,
        bAutobuildName,
        callBack, "", 0, false, objParent);

    catalogObject.parentPopup.hide();

}




/*Ajouter une nouvelle fiche*/
function AddFile(nTargetTab,
    sSearch,
    strToolTipText,
    nSearchLimit,
    nMode,
    bAutobuildName,
    callBack,
    sOrigFrameId,
    nCallFrom,
    bNoLoadFile,
    options) {


    //Test si nombre minimum de caractère entré pour l'ajout sauf pour champ principal construit automatiquement (bAutoBuildName)
    if ((sSearch.length < nSearchLimit) && !bAutobuildName) {
        top.eAlert(2, top._res_225, top._res_1499.replace("<CAR_MIN>", nSearchLimit));
        return;
    }

    if (typeof nCallFrom === "undefined" || nCallFrom == 0) {
        nCallFrom = 5; //mode finder

        if (nMode == "1")
            nCallFrom = 1;	//depuis page d'accueil
    }



    var oOptions = Object.assign({}, options)
    if (bNoLoadFile) {
        oOptions = oOptions.noLoadFile = true;
    }

    var sMainFieldValue = (bAutobuildName) ? "" : sSearch

    if ([CallFromNavBarToPurple, CallFromMenuToPurple, CallFromBkmToPurple].some(i => i == nCallFrom)) {
        openPurpleFile(nTargetTab, 0, sMainFieldValue, nCallFrom, null);
    }
    else {
        shFileInPopup(nTargetTab, 0, strToolTipText, null, null, 0, sMainFieldValue,
            true, callBack, nCallFrom, sOrigFrameId, null, null, oOptions);
    }
}

//Tableau de valeurs sélectionnée
var _selectedListValues = new Array();
/* Fonction selValue : met en surbrillance la ligne cliquée et ajoute la valeurs sélectionnée à la coche */
function selValue(oSelectedObjectId, bChecked) {
    oSelectedObject = document.getElementById(oSelectedObjectId);
    if (!oSelectedObject)
        return;

    var bMulti = getAttributeValue(document.getElementById("mainDiv"), "multi") == "1";

    // id  
    var sEid = getAttributeValue(oSelectedObject, "eid");
    var oId = sEid.split('_');
    var nTab = oId[0];
    var nId = oId[oId.length - 1];
    if (oId != '') {
        var selectedClassName = "eSel";
        if ((bChecked == 'undefined') || (typeof (bChecked) == 'undefined'))
            bChecked = (oSelectedObject.className.indexOf(selectedClassName) < 0);

        if (bChecked) {

            //déselection des autres valeurs déja sélectionées :
            for (var i = 0; i < _selectedListValues.length; i++) {
                currentDiv = document.getElementById(_selectedListValues[i]);
                if (currentDiv) {
                    removeClass(currentDiv, selectedClassName);
                }
            }
            if (!bMulti) {
                //sélection de la nouvelle valeur
                _selectedListValues = new Array();
                addClass(oSelectedObject, selectedClassName);
                _selectedListValues.push(oSelectedObjectId);
            }
            else {
                var selTable = document.getElementById("mt_Sel" + nTab);
                if (!isSelected(sEid, selTable)) {
                    var newTR = selTable.insertRow(-1);
                    try {
                        newTR.innerHTML = oSelectedObject.innerHTML;
                    }
                    catch (e) {

                        //IE8 IE9 innerHTML est readonly pour les tablerow.
                        for (var i = 0; i < oSelectedObject.cells.length; i++) {
                            var finderCell = oSelectedObject.cells[i];
                            var cell = newTR.insertCell(-1);
                            for (var j = 0; j < finderCell.attributes.length; j++) {
                                var att = finderCell.attributes[j];
                                cell.setAttribute(att.name, att.value);
                            }
                            cell.innerHTML = finderCell.innerHTML;
                        }

                    }
                    newTR.setAttribute("eid", sEid);
                    var ename = "COL_" + nTab + "_" + (getNumber(nTab) + 1);
                    var tabTd = newTR.querySelector("td[ename='" + ename + "']");
                    newTR.setAttribute("label", GetText(tabTd));
                    tabTd.innerHTML = "<div class=\"logo_modifs\" style=\"width:32px;\"><div title=\"Supprimer\" class=\"icon-delete\" onclick=\"removeFile('" + sEid + "');\"></div></div>" + tabTd.innerHTML;
                    var checkbox = newTR.querySelector("a[sf='1']");
                    if (checkbox)
                        addClass(checkbox, "hidden");

                    if (selTable.rows.length % 2 == 0)
                        addClass(newTR, "list_odd");
                    else
                        addClass(newTR, "list_even");
                }

            }

        }
        else {
            //désélection de la valeur
            if (!bMulti) {
                removeClass(oSelectedObject, selectedClassName);
                var iPos = _selectedListValues.indexOf(oSelectedObjectId);
                if (iPos >= 0)
                    _selectedListValues.splice(iPos, 1);
            }
            else {
                removeFile(sEid);
            }

        }
    }

}

//vérifie si la valeur est déjà sélectionnée ou non (multiple uniquement)
function isSelected(eid, selTab) {
    var aEid = eid.split("_");
    if (!selTab)
        selTab = document.getElementById("mt_Sel" + aEid[0]);

    var selRow = selTab.querySelector("tr[eid='" + eid + "']");
    if (selRow)
        return true;
    else
        return false;

}

//sélectionner toutes les lignes du finder
function selAllValues(chkAll) {
    var table = findUp(chkAll, "TABLE");
    if (!table)
        return;
    var tRows = table.rows;
    var bChecked = getAttributeValue(chkAll, "chk") == "1";
    for (var i = 1; i < tRows.length; i++) {
        var checkbox = tRows[i].querySelector("a[sf='1']");
        if (bChecked != (getAttributeValue(checkbox, "chk") == "1")) {
            chgChk(checkbox);
        }

        selValue(tRows[i].id, bChecked);
    }

}



/* fonction loadValue : Charge la fiche au premier plan de la ligne cliquée puis ferme la fenetre de recherche */
function loadValue(oSelectedObjectId) {
    oSelectedObject = document.getElementById(oSelectedObjectId);
    if (!oSelectedObject)
        return;
    // id  
    var oId = getAttributeValue(oSelectedObject, "eid").split('_');
    var nTab = oId[0];
    var nId = oId[oId.length - 1];
    if (oId != '') {
        top.loadFile(nTab, nId);

        var catalogObject = top.eTabLinkCatFileEditorObject[_parentIframeId];

        catalogObject.oModalLnkFile.hide();
    }
}
/*  Mise à jour du conteneur du tableau de la liste */
function updateFinderList(oList, mainTab) {

    var oContent = document.getElementById("listContent");
    setWait(false);

    oContent.innerHTML = oList;

    var myChildNodes = oContent.childNodes;
    if (myChildNodes.length > 0) {
        myChildNodes = myChildNodes[0];
        oContent.innerHTML = myChildNodes.innerHTML
    }
    /*Gestion d'erreur*/


    var tbError = document.getElementById("tbErrorUpdate");
    if (tbError) {
        showErr(tbError.value);
    }
    else
        afterListLoaded();
    /****************/

}
/* fonction afterValidateInPopup : Est appelée après clique sur btn valider du champ de liaison
, permet de mettre à jours la valeurs du champs source
, appel engine pour enregistrer en base
et ferme la fenêtre pour éviter les courant d'air
oRecord : Retour xml d'erreur ou pas
nMode : Indique si l'on doit afficher la fiche après création
aFields : liste des valeurs des champs de la fiche appelante
*/
function afterValidateInPopup(oRecord, nMode, aFields) {
    var catalogObject = top.eTabLinkCatFileEditorObject[_parentIframeId];


    var browser = new getBrowser();
    //Dans le cas du finder, sur IE9, la rediredction vers la fiche créée peut échouée si elle a déjà été lancé depuis eEngine.
    //  TODO : il semble que eEngine gère tout seul la rediction et donc que celle effectué par afterValidate est redondante
    // a voir donc si on ne peut pas la retirer complètement

    afterValidate(oRecord, nMode, catalogObject, aFields, !browser.isIE9)
}

/* fonction afterValidate : Est appelée après clique sur btn valider du champ de liaison
, permet de mettre à jorus la valeurs du champs source
, appel engine pour enregistrer en base
et ferme la fenêtre pour éviter les courant d'air
oRecord : Retour xml d'erreur ou pas
nMode : Indique si l'on doit afficher la fiche après création
catalogObject : objet catalog appelant
aFields : liste des valeurs des champs de la fiche appelante
*/
function afterValidate(oRecord, nMode, catalogObject, aFields, bDoRedirect) {


    if (typeof (oRecord.fid) == "undefined")
        return;
    if (!catalogObject)
        return;

    if (typeof (bDoRedirect) == "undefined") {
        var bDoRedirect = true;
    }

    catalogObject.selectedValues = new Array();
    catalogObject.selectedLabels = new Array();
    var label = "";
    //Si nMode on redirige vers la fiche créée
    //Sinon on affecte la fiche créée au champ appelant
    if (nMode == "1") { //RECHERCHE AVANCéE

        top.setWait(false);

        //canceled KHA 30/01/2017 provoque un reload en doublon du signet actif
        //var fctLoafFile = top.loadFile; //On stock la fct de redirection de la fiche avant de perdre le contexte du top      //      ??? KHA pas compris l'intérêt à part rajouter des lignes dans la pile d'appel...
        //var setWaitOff = function () { top.setWait(false); }; //      ??? KHA pas compris l'intérêt à part rajouter des lignes dans la pile d'appel...
        //setWaitOff();
        //if (bDoRedirect)
        //    fctLoafFile(oRecord.tab, oRecord.fid);    //On redirige vers la fiche créées

        catalogObject.oModalLnkFile.hide(); //On ferme le finder  
    }
    else if (nMode == "2") {    //MRU
        var nFileId = getNewValueFromFieldsLink(oRecord.fid, oRecord.tab, aFields, oRecord.adr);
        var nTabId = oRecord.tab;
        catalogObject.selectValue(nFileId, oRecord.lab ? oRecord.lab : nTabId, true);
        catalogObject.validate();   // Enregistrement des valeurs sélectionnées en base et de son rafraichissement
    }
    else if (nMode == "3") { //ajout depuis BKM => AutoCreate
        var nFileId = oRecord.fid;
        var nTabId = oRecord.tab;
        addFromBkm(nFileId, nTabId, false);
    }
    else {
        var nFileId = getNewValueFromFieldsLink(oRecord.fid, oRecord.tab, aFields, oRecord.adr);
        var nTabId = oRecord.tab;
        catalogObject.selectValue(nFileId, oRecord.lab ? oRecord.lab : nTabId, true);
        catalogObject.validate();   // Enregistrement des valeurs sélectionnées en base et de son rafraichissement

        top.setWait(false);
        catalogObject.oModalLnkFile.hide(); //Fermeture du champ de liaison de recherche
    }
}
//GCH #34808 : Création PP à partir de Planning
//Retourne le FileId avec les liaisons du haut en plus si cela est necessaires (id;|;Adrid$|$Adr01;|;PmId$|$Pm01)
function getNewValueFromFieldsLink(nFileId, nTab, aFields, adr) {
    if (nTab == 200 && aFields) {
        var SEP1 = ";|;";
        var SEP2 = "$|$";
        var nPmId = "";
        var sPm01 = "";

        if (typeof (adr) != "undefined")//cas de creation de pp et adr en popup en meme temps     
            return nFileId + SEP1 + adr.adrId + SEP2 + adr.adr01 + SEP1 + adr.pmId + SEP2 + adr.pm01;


        //GCH : on décrémente car le champ de liason pm01 est ajouté en dernier donc ce sera plus rapide.
        for (i = aFields.length - 1; i >= 0; i--) {
            if (aFields[i].descId == 300) {
                nPmId = aFields[i].newValue;
                sPm01 = aFields[i].newLabel;
                break;
            }
        }

        if (nPmId > 0) {

            //id;|;Adrid$|$Adr01;|;PmId$|$Pm01                 
            nFileId = nFileId + SEP1 + adrId + SEP2 + adr01 + SEP1 + nPmId + SEP2 + sPm01
        }
    }

    return nFileId;
}

//Charge les éléments de style et classes post chargement de la liste
function afterListLoaded() {
    var oMainDiv = document.getElementById("mainDiv");
    var nTargetTab = getAttributeValue(oMainDiv, "tab");
    var oTabList = document.getElementById("mt_" + nTargetTab);
    _SearchLimit = getAttributeValue(oMainDiv, "SearchLimit");

    setFocus();

    adjustLastCol(nTargetTab);

    var bMulti = getAttributeValue(oMainDiv, "multi") == "1";
    if (bMulti) {
        _selectedListValues = getSelectedValues();
        for (var i = 0; i < _selectedListValues.length; i++) {
            var sEid = _selectedListValues[i].eid;
            var oTr = oTabList.querySelector("tr[eid='" + sEid + "']");
            if (!oTr) {
                continue;
            }

            var chk = oTr.querySelector("a[sf='1']");
            if (getAttributeValue(chk, "chk") != "1")
                chgChk(chk);
        }

    }

}

var modalListCol;
/* fonction selCol : Permet d'ouvrir le catalogue de choix de rubrique
nTab : table affichée depuis laquelle on cherche à avoir les rubriques sélectionnable
nParentTab : table parente de la table de recherche
*/
function selCol(nTab, nParentTab) {
    var oMainDiv = document.getElementById("mainDiv");
    if ((!oMainDiv) || (typeof (oMainDiv) == "undefined"))
        return;
    var sListCol = getAttributeValue(oMainDiv, "listCol");
    var sListColSpec = getAttributeValue(oMainDiv, "listColSpec");

    modalListCol = new eModalDialog(top._res_96, 0, "eFieldsSelect.aspx", 850, 550);
    modalListCol.ErrorCallBack = function () { setWait(false); }

    modalListCol.addParam("tab", nTab, "post");
    modalListCol.addParam("parentTab", nParentTab, "post");
    modalListCol.addParam("listCol", sListCol, "post");
    modalListCol.addParam("listColNotDisplay", sListColSpec, "post");
    modalListCol.addParam("action", "initlnkfile", "post");

    modalListCol.bBtnAdvanced = true;
    modalListCol.show();
    modalListCol.addButton(top._res_29, onSelColAbort, "button-gray", nTab);
    modalListCol.addButton(top._res_28, onSelColOk, "button-green", nTab);
}
/* fonction onSelColAbort : Permet de fermer la fenêtre de choix de rubrique*/
function onSelColAbort() {
    modalListCol.hide();
}

/* fonction onSelColOk : Permet de fermer la fenêtre de choix de rubrique et de valider les rubriques sélectionnées puis de rafraichir la liste*/
function onSelColOk(nTab, popupId) {
    var _frm = top.window.frames["frm_" + popupId];
    var strListCol = _frm.getSelectedDescId();

    var oMainDiv = document.getElementById("mainDiv");
    oMainDiv.setAttribute("listCol", strListCol);

    var updatePref = "tab=" + nTab + ";$;listcol=" + strListCol;
    updateUserFinderPref(updatePref, function () { StartSearch(); }, function () { });

    modalListCol.hide();
}

function showErr(strMessage) {
    top.eAlert(0, top._res_225, top._res_6235, top._res_6236 + " <!--" + strMessage + "-->");  //TODOGCH

}
/*Action au clique sur une ligne d'une liste de catalogue de champ de liaison.
e : evennement appelant
bDblClick : Si double click on force la sélection de la ligne
nMode : Si LoadFile on redirige vers la fiche sélectionnée sinon sélectionne juste la fiche
*/
function ocf(e, bDblClick, nMode, jsVarName) {
    if (!e)
        e = window.event;
    var idRow = null;
    var oRow = null;
    var src = (e.originalTarget || e.srcElement);
    var bIsSrcCheckBox = false;
    var bMulti = getAttributeValue(document.getElementById("mainDiv"), "multi") == "1";

    //--- SPH RMA MCR : 39400 ajout debut cas du CTI, sur le clic d une cellule, redirection vers la fiche selectionnee (PM) ou (PP) nMode=1 & eaction="LNKFILE"
    //                  par SetFldGoFile 
    var oModal = null;
    if (top.eTabLinkCatFileEditorObject[_parentIframeId])
        oModal = top.eTabLinkCatFileEditorObject[_parentIframeId];
    if (oModal && oModal.oModalLnkFile) {
        oModal = oModal.oModalLnkFile;
    }


    //Cas du cti - on se base sur le catalogue de la cellule, pas du lien
    if (oModal != null && src != null && getAttributeValue(src, "eaction") == "LNKGOFILE" && nMode == 1 && (oModal.getParam("SearchType") + "") == "5") {
        SetFldGoFile(src);
        // MCR 39936 : fermeture de la fenetre modale, apres clic sur le champ 'Nom' de type PP ou le champ 'societe ' de type PM
        // faire un hide() sur la fenetre modale : oModal
        // oModal.oModalLnkFile.hide();
        oModal.hide();
        return;
    }
    //--- SPH RMA MCR : 39400 fin ajout code 

    while (src && idRow == null) {
        if (bMulti && src.tagName == "A" && getAttributeValue(src, "sf") == "1") {
            bIsSrcCheckBox = true;
        }
        if (src.id && src.id.indexOf("row") == 0) {
            idRow = src.id;
            oRow = src;
        }
        if (src.parentNode) {
            src = src.parentNode;
        }
        else
            break;
    }
    if (idRow == null)
        return;
    if (nMode == "1")
        loadValue(idRow);   //Redirection vers la fiche cliquée
    else
        if (nMode == "2") { //MRU sélection de la valeurs
            selValue(idRow, true);    //Sélection de la fiche cliquée
            validateLnkMRU(jsVarName);  //Enregistrement
        }
        else if (nMode == "3") { //ajout depuis BKM => AutoCreate
            addFromBkmFinder(idRow);
        }
        //else if (nMode == "4") {
        //    // Sélection d'une fiche

        //}
        else if (bDblClick) {
            selValue(idRow, true);    //Sélection de la fiche cliquée
            var catalogObject = top.eTabLinkCatFileEditorObject[_parentIframeId];
            if (catalogObject.oModalLnkFile && catalogObject.oModalLnkFile.CallOnOk)
                catalogObject.oModalLnkFile.CallOnOk(_parentIframeId);
            else
                parent.validateLnkFile(_parentIframeId);
        }
        else {
            if (bMulti) {
                var checkbox = oRow.querySelector("a[sf='1']");
                if (!bIsSrcCheckBox) {
                    chgChk(checkbox);
                }
                selValue(idRow, getAttributeValue(checkbox, "chk") == "1");       //Sélection de la fiche cliquée
            }
            else {
                selValue(idRow);       //Sélection de la fiche cliquée
            }
        }
}

// EVENEMENT LORS DU RESIZE DE LA MODALDIALOG DU CHAMP DE LIAISON
function onFrameSizeChange(w, h) {
    var oDivGlobal = document.getElementById("mainDiv");
    var oDivCatVal = document.getElementById("listContent"); // DIV contenant les valeurs du catalogue
    var oIFrameAddFile = document.querySelector(".window_iframe.add-file"); // si la classe css add-file existe alors on ignore le traitement de la hauteur, qui est traité en css

    if (oIFrameAddFile)
        return false;


    if (oDivGlobal)
        oDivGlobal.style.height = (parseInt(h)) + "px";
    if (oDivCatVal) {
        var nHeight = parseInt(h) - 180;
        if (getAttributeValue(oDivGlobal, "multi") == "1") {
            nHeight -= 20; //espace entre les deux blocs
            oDivCatVal.style.height = (nHeight / 2) + "px";
            var oDivSel = document.getElementById("selContent");
            if (oDivSel)
                oDivSel.style.height = (nHeight / 2) + "px";
        }
        else {
            oDivCatVal.style.height = (parseInt(h) - 180) + "px";
        }
    }
}


/* fonction addFromBkmFinder : Action affectuée lorsque l'on sélectionne une ligne dans le finder depuis un bkm :
- oSelectedObjectId : id de ligne cliquée
*/
function addFromBkmFinder(oSelectedObjectId) {
    oSelectedObject = document.getElementById(oSelectedObjectId);
    if (!oSelectedObject)
        return;
    // id  
    var oId = getAttributeValue(oSelectedObject, "eid").split('_');
    var nTab = oId[0];
    var nId = oId[oId.length - 1];
    if (oId != '') {
        addFromBkm(nId, nTab, true)
    }
}
/* fonction addFromBkmFinder : Action affectuée lorsque l'on sélectionne une ligne dans le finder depuis un bkm :
- nFileId : Id de bkm ayant été créée/sélectionnée
- nTabId : Id de la table ayant créée/sélectionnée
- bAdd : définit si on doit ajouter l'adresse entre le contact et la société
*/
function addFromBkm(nFileId, nTabId, bAdd) {

    var parentTab = top.nGlobalActiveTab;
    var parentFileId = top.GetCurrentFileId(top.nGlobalActiveTab);
    var currentTab = nTabId;


    if ((currentTab == 200) && (parentTab == 300)) {    //Cas particulier de l'adresse depuis société qui fait un autocreate à la sélection
        currentTab = 400;
        if (bAdd) {
            top.setWait(true);
            var fctCloseAndReload = function (oRes) {
                try {
                    var catalogObject = top.eTabLinkCatFileEditorObject[_parentIframeId];
                    if (catalogObject && catalogObject.oModalLnkFile && typeof catalogObject.oModalLnkFile.specialAction == "function") {
                        catalogObject.oModalLnkFile.specialAction(oRes);
                    }
                    else {
                        top.loadBkmList(currentTab);
                        var reloadFileHeader = getXmlTextNode(oRes.getElementsByTagName("reloadfileheader")[0]) == "1";
                        if (reloadFileHeader)
                            top.RefreshHeader();

                        top.setWait(false);

                        catalogObject.oModalLnkFile.hide();
                    }
                }
                catch (e) {
                    top.setWait(false);
                }
            }

            top.autoCreate(currentTab, new Array({ tab: nTabId, fid: nFileId }, { tab: parentTab, fid: parentFileId }), true, fctCloseAndReload);
        } else {



            var catalogObject = top.eTabLinkCatFileEditorObject[_parentIframeId];
            
            if (catalogObject && catalogObject.oModalLnkFile && typeof catalogObject.oModalLnkFile.specialAction == "function") {

                catalogObject.oModalLnkFile.specialAction();
                top.setWait(false)
            }
            else
            catalogObject.oModalLnkFile.hide();
        }
    }
    else {
        //Rafraichir le bkm source

        top.loadBkmList(currentTab);


        var catalogObject = top.eTabLinkCatFileEditorObject[_parentIframeId];
        catalogObject.oModalLnkFile.hide();
    }
}

//Catalogue multiple : recense les valeurs sélectionnées
function getSelectedValues() {
    _selectedListValues = new Array();
    var nTab = getAttributeValue(document.getElementById("mainDiv"), "tab");
    var selTable = document.getElementById("mt_Sel" + nTab);
    for (var i = 1; i < selTable.rows.length; i++) {
        var sEid = getAttributeValue(selTable.rows[i], "eid");
        var oId = sEid.split('_');
        var nId = oId[oId.length - 1];
        var sLabel = getAttributeValue(selTable.rows[i], "label");

        _selectedListValues.push({ id: nId, label: sLabel, eid: sEid });
    }
    return _selectedListValues;
}

function removeFile(sEid) {
    var nTab = sEid.split('_')[0];

    // suppression dans la table de sélection
    var selTab = document.getElementById("mt_Sel" + nTab);
    var selTr = selTab.querySelector("tr[eid='" + sEid + "']");
    for (var i = 0; i < selTab.rows.length; i++) {
        if (getAttributeValue(selTab.rows[i], "eid") == sEid) {
            selTab.deleteRow(i);
            break;
        }
    }

    //décochage de la case à cocher
    var mainTab = document.getElementById("mt_" + nTab);
    var mainTr = mainTab.querySelector("tr[eid='" + sEid + "']");
    if (mainTr) {
        var chk = mainTr.querySelector("a[sf='1']");
        if (getAttributeValue(chk, "chk") == "1")
            chgChk(chk);

        var chkAll = document.getElementById("chkAll_" + nTab);
        if (getAttributeValue(chkAll, "chk") == "1")
            chgChk(chkAll);
    }
}