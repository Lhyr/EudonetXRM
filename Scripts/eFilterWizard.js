/*Necessite eFilterWizardLight.js, eMD5.js*/
//#region manipulation du formulaire et appels ajax
var modalLik;

var nsFilterWizard = nsFilterWizard || {};

//initialisation ...
function doPermParam(permType) {

    //par niveau
    var srcId = "OptLevels_" + permType;
    if (srcId.indexOf("OptLevels_" + permType) >= 0 && document.getElementById(srcId) != null) {
        if (document.getElementById(srcId).checked) {

            document.getElementById("LevelLst_" + permType).disabled = false;
        }
        else {

            document.getElementById("LevelLst_" + permType).disabled = true;
        }
    }
    //par user
    srcId = "OptUsers_" + permType;
    if (srcId.indexOf("OptUsers_" + permType) >= 0 && document.getElementById(srcId) != null) {

        if (document.getElementById(srcId).checked) {

            document.getElementById("TxtUsers_" + permType).style.display = "inline-block";
            document.getElementById("UsersLink_" + permType).style.display = "inline-block";
        }
        else {

            document.getElementById("TxtUsers_" + permType).style.display = "none";
            document.getElementById("UsersLink_" + permType).style.display = "none";
        }
    }
}

function DelFilterTab(tabIdx, bConfirm) {
    if (bConfirm) {
        eConfirm(1, "", top._res_88, '', 600, 200, function () { DelFilterTab(tabIdx, false); }, function () { });
        return;
    }
    if (getTabsNbr() == 1)
        return;
    var oTab = document.getElementById("table_filtres_" + tabIdx);
    var lstTab = document.getElementsByClassName("table_filtres");
    var oOperator;
    if (oTab === lstTab[0]) {
        oOperator = document.getElementById("operateur_principal_" + (tabIdx + 1));
    } else {
        oOperator = document.getElementById("operateur_principal_" + tabIdx);
    }
    if (oTab != null)
        document.getElementById("FilterTabsContainer").removeChild(oTab);

    if (oOperator != null)
        document.getElementById("FilterTabsContainer").removeChild(oOperator);

}

function getTabsNbr() {
    var nbr = 0;
    for (i = 0; i < MAX_NBRE_TABS; i++) {
        if (document.getElementById("table_filtres_" + i) != null)
            nbr++;
    }

    return nbr;
}

function DelFilterLine(tabIdx, lineIdx, bConfirm) {
    if (bConfirm) {
        eConfirm(1, "", top._res_88, '', 600, 200, function () { DelFilterLine(tabIdx, lineIdx, false); }, function () { });
        return;
    }
    //Impossible de supprimer la première ligne du filtre
    if (lineIdx == 0)
        return;
    id = "line_" + tabIdx + "_" + lineIdx;
    oLine = document.getElementById(id);
    if (oLine != null)
        document.getElementById("choix_filtres_" + tabIdx).removeChild(oLine);
}

function getEmptyLine(tabIdx, lineIdx, descId, tab, bEndOperator, oLineOp, treatFunc, fctArg, bFromAdmin) {

    var upd = new eUpdater("mgr/eFilterWizardManager.ashx", 1);


    //Gestion d'erreur : a priori, pas de traitement particulier
    upd.ErrorCallBack = function () {
    };

    upd.addParam("action", "emptyline", "post");
    upd.addParam("filtertype", nfilterType, "post");
    upd.addParam("maintab", nTab, "post");
    upd.addParam("tabindex", tabIdx, "post");
    upd.addParam("lineindex", lineIdx, "post");
    upd.addParam("descid", descId, "post");
    upd.addParam("tab", tab, "post");
    upd.addParam("endoperator", bEndOperator, "post");
    upd.addParam("lineoperator", oLineOp, "post");
    upd.addParam("adminMode", bFromAdmin ? "1" : "0", "post");

    upd.send(treatFunc, fctArg);
}


function showAdvancedfilterOptions() {

    checkFileOnlySaufOp();

    var filtersDiv = document.getElementById("FilterTabsContainer");
    if (document.getElementById("DivOptList").style.display == "none") {
        document.getElementById("DivOptList").style.display = "block";
        document.getElementById("OptBtnDiv").className = "icon-unvelop arrow-opt";
        filtersDiv.className += " filtres-scroll";

    }
    else {
        document.getElementById("DivOptList").style.display = "none";
        document.getElementById("OptBtnDiv").className = "icon-develop arrow-opt";
        filtersDiv.className = "filtres";
    }
}

function onChangeField(lst, bFromAdmin) {

    var aInfos = lst.id.split('_');
    var tabIndex = aInfos[1];
    var lineIndex = aInfos[2];
    var op = lst.options[lst.selectedIndex].value;
    var tabDescid = document.getElementById("file_" + tabIndex).options[document.getElementById("file_" + tabIndex).selectedIndex].value;
    var fieldDescid = lst.options[lst.selectedIndex].value;

    var oLineOp = document.getElementById("and_" + tabIndex + "_" + lineIndex);
    var lineOp = 0;
    if (oLineOp != null)
        lineOp = oLineOp.options[oLineOp.selectedIndex].value;

    if (typeof bFromAdmin === "undefined")
        bFromAdmin = false;

    getEmptyLine(tabIndex, lineIndex, fieldDescid, tabDescid, 0, lineOp, onChangeFieldTreatment, lst.id, bFromAdmin);
}

function onChangeFieldTreatment(oRes, lstId) {
    var aInfos = lstId.split('_');
    var tabIndex = aInfos[1];
    var lineIndex = aInfos[2];
    var oLine = document.getElementById("line_" + tabIndex + "_" + lineIndex);
    oLine.style.opacity = 0;
    oLine.style.filter = 'alpha(opacity = 0)';
    var divTmp = document.createElement("div");
    divTmp.innerHTML = oRes;

    oLine.innerHTML = divTmp.getElementsByTagName("div")[0].innerHTML;
    fadeThis(oLine.id, document);

}

///Variables utilisées lors du eConfirm de changement du fichier depuis la liste déroulante
///Nécessaire car depuis onValidateChangeFileTabTreatment et onCancelChangeFileTabTreatment
///la modal "modalik" était n,on instanciée encore (undefined)

///Descid du fichier sélectionné dans la liste
var _lstTab;
///Index du fichier sélectionné dans la liste
var _lstTabIndex;
///Nom du fichier sélectionnée dans la liste
var _lstTabName;
///Liste déroulante des fichiers
var _lstTabList;

///summary
///Assure le procédé de changement de fichier depuis la liste déroulante des fichiers sur lequel porte l'onglet.
///<param name="lst">Liste déroulante de sélection des fichiers</param>
///summary
function onChangeFileTab(lst) {

    var tabIdx = lst.id.replace("file_", "");
    var tab = lst.options[lst.selectedIndex].value;
    var tabName = lst.options[lst.selectedIndex].text;

    var oldIndex = getNumber(getAttributeValue(lst, "ednindex"));
    var oldTab = lst.options[oldIndex].value;
    var oldTabName = lst.options[oldIndex].innerHTML;   // pour que les chevrons (<>) soient encodés, on utilise la propriété inner html 


    if (tab == "0")//Lier un nouveau fichier
    {
        modalLik = new eModalDialog(top._res_982, 0, "eFilterWizard.aspx", 400, 175);
        modalLik.addParam("tab", nTab, "post");
        modalLik.addParam("action", "getlinkedfile", "post");

        //CallBack d'erreur : on masque la modal
        modalLik.ErrorCallBack = launchInContext(modalLik, modalLik.hide);

        modalLik.show();

        var mydiv = modalLik.getDivContainer();
        setAttributeValue(mydiv.children[2], "lierFichier", "true");
        setAttributeValue(mydiv.children[1], "lierFichierIframe", "true");

        console.log(mydiv.children[1]);

        modalLik.addArg(lst);
        modalLik.addButton(top._res_29, cancelLinkFile, "button-gray", ""); // Annuler
        modalLik.addButton(top._res_28, validLinkFile, "button-green", ""); // Valider
    }
    else {
        _lstTab = tab;
        _lstTabIndex = tabIdx;
        _lstTabName = tabName;
        _lstTabList = lst;
        eConfirm(1, "", top._res_6334.replace('<TAB>', oldTabName), "", 450, 150, onValidateChangeFileTabTreatment, onCancelChangeFileTabTreatment);
    }
}

///summary
///
///summary
function onValidateChangeFileTabTreatment() {
    _lstTabList.setAttribute("ednindex", _lstTabList.selectedIndex);
    getEmptyLine(_lstTabIndex, 0, 0, _lstTab, 1, 0, onChangeFileTabTreatment, _lstTabIndex);
    return;
}

function onCancelChangeFileTabTreatment() {
    _lstTabList.selectedIndex = _lstTabList.getAttribute("ednindex");
    return;
}

function cancelLinkFile(varName) {
    var lst = modalLik.inputArgs[0];
    lst.selectedIndex = lst.getAttribute("ednindex");
    modalLik.hide();
}


function validLinkFile() {
    var _ifrm = modalLik.getIframe();
    var strReturned = _ifrm.getReturnValueLinked();
    var aReturn = strReturned.split(";|;");
    var descId = aReturn[0];
    var libelle = aReturn[1];
    //affectation dans la liste
    var lst = modalLik.inputArgs[0];
    lst.options[lst.length] = new Option(libelle, descId);
    lst.selectedIndex = lst.length - 1;

    //fermer le popup
    modalLik.hide();
    onChangeFileTab(lst);
}

function onChangeFileTabTreatment(oRes, tabIdx) {
    var oTab = document.getElementById("choix_filtres_" + tabIdx);

    //oTab.style.opacity = 0;
    //oTab.style.filter = 'alpha(opacity = 0)';
    oTab.innerHTML = oRes;
    //fadeThis(oTab.id, document);


}


function onChangeFilterLineOp(lst) {
    if (lst.id.indexOf("end_operator_line_") == 0) //Operateur de fin de lignes
    {
        addNewFilterLine(lst);
    }
    else {
        //Si fin ==> supprimer toutes les autres lignes
        if (lst.options[lst.selectedIndex].value == 0) {
            var aInfos = lst.id.split('_');
            var tabIdx = aInfos[1];
            var lineIdx = aInfos[2];
            for (i = lineIdx; i < MAX_NBRE_LINES; i++) {
                DelFilterLine(tabIdx, i);
            }
        }
    }
}


// L'opérateur sauf est incompatible avec l'option "ne retenir que"
function checkFileOnlySaufOp() {
    try {

        // vérifie si un des opérateur est "sauf"
        var oSauf = [].slice.call(document.getElementById("FilterTabsContainer").querySelectorAll("select[id^='link'] option:checked[value='3']"));

        var chkFileOnly = document.getElementById("chk_fileonly");
        if (!chkFileOnly)
            return;

        if (oSauf.length > 0) {
            //décoche si nécessaire
            if (getAttributeValue(chkFileOnly, "chk") == "1")
                chkFileOnly.click();

            //désactive la case
            disChk(chkFileOnly, true)
        }
        else
            disChk(chkFileOnly, false)
    }
    catch (e) {

    }

}

function onChangeFilterTabOp(lst) {

    if (lst.id.indexOf("end_operator_tab_") == 0) //Operateur de fin de tables
    {
        addNewFilterTab(lst);
    }
    else {
        //Si fin ==> supprimer toutes les autres tabs
        if (lst.options[lst.selectedIndex].value == 0) {
            var aInfos = lst.id.split('_');
            var tabIdx = aInfos[1];

            for (i = tabIdx; i < MAX_NBRE_TABS; i++) {
                DelFilterTab(i);
            }
        }

    }

    checkFileOnlySaufOp();



}


function openCloseTab(tabidx, img) {
    var tab = document.getElementById("choix_filtres_" + tabidx);
    if (tab.style.display == "none") {
        tab.style.display = "block";
        img.className = "OpenDiv";
    }
    else {
        tab.style.display = "none";
        img.className = "CloseDiv";
    }


}
function addNewFilterLine(lst) {
    var aInfos = lst.id.split('_');
    var tabIndex = aInfos[3];
    //Calcul de lineIndex
    var i = 0;
    for (i = 0; i < MAX_NBRE_LINES; i++) {
        var line = document.getElementById("line_" + tabIndex + "_" + i);
        if (line == null) {
            break;
        }
    }
    if (i == MAX_NBRE_LINES) {
        eAlert(3, top._res_182, top._res_6277.replace('<NBR>', MAX_NBRE_LINES), '', null, null, function () { lst.selectedIndex = 2; });
        return;
    }

    var tabDescid = document.getElementById("file_" + tabIndex).options[document.getElementById("file_" + tabIndex).selectedIndex].value;
    var lineOp = lst.options[lst.selectedIndex].value;
    getEmptyLine(tabIndex, i, 0, tabDescid, 0, lineOp, addNewFilterLineTreatment, "line_" + tabIndex + "_" + i);
    lst.selectedIndex = 2;
}
function addNewFilterTab(lst) {
    //Calcul du tabIndex
    var tabIndex = 0;

    for (i = 1; i <= MAX_NBRE_TABS; i++) {
        var tab = document.getElementById("file_" + i);
        if (tab == null) {
            tabIndex = i;
            break;
        }
    }
    if (i == MAX_NBRE_TABS) {
        eAlert(3, top._res_182, top._res_6277.replace('<NBR>', MAX_NBRE_TABS), '', null, null, function () { lst.selectedIndex = 3; });
        return;
    }

    var tabOperator = lst.options[lst.selectedIndex].value;
    getEmptyFilterTab(tabIndex, tabOperator, addNewFilterTabTreatments, tabIndex);
    lst.selectedIndex = 3;
}

function getEmptyFilterTab(tabIdx, tabOperator, funct, fctArgs) {

    var onlyFilesOpt = "0";
    if (document.getElementById("OnlyFilesOpt").checked)
        onlyFilesOpt = "1";

    var upd = new eUpdater("mgr/eFilterWizardManager.ashx", 1);
    //Gestion d'erreur : a priori, pas de traitement particulier
    upd.ErrorCallBack = function () {
    };
    upd.addParam("action", "emptytab", "post");
    upd.addParam("filtertype", nfilterType, "post");
    upd.addParam("tabindex", tabIdx, "post");
    upd.addParam("onlyfilesopt", onlyFilesOpt, "post");
    upd.addParam("maintab", nTab, "post");
    upd.addParam("endoperator", 0, "post");
    upd.addParam("taboperator", tabOperator, "post");
    upd.send(funct, fctArgs);
}

function addNewFilterTabTreatments(oRes, tabIdx) {
    var endOp = document.getElementById("end_operator_tab_");
    var oNewTabContainer = document.createElement("div");
    oNewTabContainer.innerHTML = oRes;
    var oNewOp = oNewTabContainer.getElementsByTagName("div")[0];
    var oNewTab = oNewTabContainer.getElementsByTagName("div")[1];
    document.getElementById("FilterTabsContainer").insertBefore(oNewOp, endOp);
    document.getElementById("FilterTabsContainer").insertBefore(oNewTab, endOp);
}
function addNewFilterLineTreatment(oRes, lineId) {
    var aInfos = lineId.split("_");
    var tabIndex = aInfos[1];
    var lineIndex = aInfos[2];
    var otmpLine = document.createElement("div");
    otmpLine.innerHTML = oRes;
    otmpLine.innerHTML = otmpLine.getElementsByTagName("div")[0].innerHTML;
    var oNewLine = document.createElement("div");
    oNewLine.id = lineId;
    oNewLine.className = "ligne";
    oNewLine.innerHTML = otmpLine.innerHTML;
    document.getElementById("choix_filtres_" + tabIndex).insertBefore(oNewLine, document.getElementById("EndLineOperatorContainer_" + tabIndex));

}
//#endregion

//#region Action du champs value

//#region parse du form et construction de la chaine param
function getFilterParams() {

    //parcours des tabs
    var _filterParams = "";
    var idxTab = 0;
    var i = 0;
    for (i = 0; i < MAX_NBRE_TABS; i++) {
        if (document.getElementById("file_" + i) != null) {
            var paramTab = getTabParams(i, idxTab);
            _filterParams += "&" + paramTab;
            idxTab++;
        }
    }

    var fileonly = "0";
    var negation = "0";
    var raz = "0";
    var random = "0";

    if (document.getElementById("fileonly") && document.getElementById("fileonly").checked) {
        fileonly = "1";
    }
    if (document.getElementById("negation") && document.getElementById("negation").checked) {
        negation = "1";
    }
    if (document.getElementById("raz") && document.getElementById("raz").checked) {
        raz = "1";
    }
    if (document.getElementById("random") && document.getElementById("random").checked) {
        random = document.getElementById("TxtRandom").value;
    }



    _filterParams += getGlobalTabsOptions(fileonly, negation, raz, random);


    // option filtre par défaut
    if (document.getElementById("applyDefaultFilter")) {

        var applyed = document.querySelector("input[name='applyDefaultFilter']:checked");
        var always = document.querySelector("input[name='alwaysActiveDefaultFilter']:checked");
        if (applyed && always) {
            _filterParams += '&defaultfilter=' + applyed.value;
            _filterParams += '&activedefaultfilter=' + always.value;

        }


    }
    return _filterParams;
}
//#endregion

function getTabParams(tabIndex, idxTab) {
    var _tabParams = "";
    if (document.getElementById("link_" + tabIndex) != null) {
        var tabOperator = document.getElementById("link_" + tabIndex).options[document.getElementById("link_" + tabIndex).selectedIndex].value;
        _tabParams = getTabOperator(tabOperator, idxTab);
    }
    var file = document.getElementById("file_" + tabIndex).options[document.getElementById("file_" + tabIndex).selectedIndex].value;
    if (_tabParams != "")
        _tabParams += "&";
    _tabParams += getTabFile(file, idxTab);

    //Liste des lines
    var idxLine = 0;
    var i = 0;
    for (i = 0; i < MAX_NBRE_LINES; i++) {
        if (document.getElementById("field_" + tabIndex + "_" + i) != null) {
            var paramLine = getLineParams(tabIndex, i, idxTab, idxLine);
            _tabParams += "&" + paramLine;
            idxLine++;
        }
    }

    // Options de Group By
    if (document.getElementById("importance_" + tabIndex).checked) {
        var importance = 1;
        var top = document.getElementById("top_" + tabIndex).options[document.getElementById("top_" + tabIndex).selectedIndex].value;
        var groupby = document.getElementById("groupby_" + tabIndex).options[document.getElementById("groupby_" + tabIndex).selectedIndex].value;
        _tabParams += getTabGroupByOption(importance, top, groupby, idxTab);
    }

    return _tabParams;
}

function getLineParams(tabIndex, lineIndex, idxTab, idxLine) {
    var logicOpRet = "";
    if (document.getElementById("and_" + tabIndex + "_" + lineIndex) != null) {
        var logicOp = document.getElementById("and_" + tabIndex + "_" + lineIndex).options[document.getElementById("and_" + tabIndex + "_" + lineIndex).selectedIndex].value;
        logicOpRet = getLogicLineParam(logicOp, idxTab, idxLine);
    }
    var field = document.getElementById("field_" + tabIndex + "_" + lineIndex).options[document.getElementById("field_" + tabIndex + "_" + lineIndex).selectedIndex].value;
    var op = document.getElementById("op_" + tabIndex + "_" + lineIndex).options[document.getElementById("op_" + tabIndex + "_" + lineIndex).selectedIndex].value;
    var value = "";
    var inpt = document.getElementById("value_" + tabIndex + "_" + lineIndex);
    //inpt.setAttribute("ednValue", inpt.value);


    if (nfilterType == 1 || nfilterType == 4) {
        if (inpt != null && inpt.selectedIndex !== undefined && inpt.selectedIndex != null)
            value = inpt.options[inpt.selectedIndex].value;
    }
    else {

        value = inpt.getAttribute("ednValue");
    }



    // si le format est de type date, on fait la conversion
    var DATE_FORMAT = "2";
    var isDate = getAttributeValue(inpt, "format") == DATE_FORMAT;


    value = isDate ? eDate.ConvertDisplayToBdd(value) : value;

    var retValue = logicOpRet + getLineValueParam(field, op, value, idxTab, idxLine);

    return retValue;
}


//Retourne les filterparams pour les champs vides
function getEmptyFilterParams() {
    var sParams = "";
    for (var i = 0; i < MAX_NBRE_TABS; i++) {
        for (var j = 0; j < MAX_NBRE_LINES; j++) {
            if (document.getElementById("field_" + i + "_" + j) != null) {
                var paramLine = getLineParams(i, j, i, j);
                sParams += "&" + paramLine;
            }
        }
    }
    return sParams;
}
//#endregion

//#region Lier un fichier

function onChangeLinkFileTab(lst) {
    if (lst.id != "LinkedFromList")
        return;
    var tab = lst.options[lst.selectedIndex].value;
    var upd = new eUpdater("eFilterWizard.aspx", 1);

    //CallBack d'erreur
    //  Le message d'erreur est suffisant, pas d'autre action
    upd.ErrorCallBack = function () { };

    upd.addParam("tab", tab, "post");
    upd.addParam("action", "reloadlinkedfile", "post");
    upd.send(onChangeLinkFileTabTreatment, null);
}

function onChangeLinkFileTabTreatment(oRes) {
    document.getElementById("FileListDiv").innerHTML = oRes;
}
function getReturnValueLinked() {
    var lstLinkedFrom = document.getElementById("LinkedFromList");
    var lstLinked = document.getElementById("LinkedList");
    var descId =
        lstLinked.options[lstLinked.selectedIndex].value +
        "," +
        lstLinkedFrom.options[lstLinkedFrom.selectedIndex].value;


    var libelle = "<" + lstLinked.options[lstLinked.selectedIndex].text
        + " " + top._res_535 + " "
        + lstLinkedFrom.options[lstLinkedFrom.selectedIndex].text + ">";

    return descId + ";|;" + libelle;
}

//#endregion

//#region validation de la fenêtre

function saveFilterAs() {
    var filterParams = getFilterParams();
    beforeSaveFilter(true);

}

function saveFilter() {
    var filterParams = getFilterParams();
    var isNewFilter = document.getElementById("TxtFilterId").value == "0";
    beforeSaveFilter(isNewFilter);
}

var bSaved = false;

//bApply : si = 1, on active le filtre
//saveas : libellé sous leques est enregistré le filtre
//bSaveAsLastFilter : en mettant cette variable à true le filtre est enregistré comme dernier filtre non sauvegardé.
//returnFct : fonction a exécuter après avoir enregistré le filtre.
function saveDb(bApply, saveas, bSaveAsLastFilter, returnFct) {

    // dans le cas des règles on n'autorise pas les valeurs vides
    if (nfilterType == 2) {
        var lstValuesInputElts = document.querySelectorAll("input[id^='value_']");

        for (var iptVal of lstValuesInputElts) {
            if (!(iptVal?.value?.length > 0)
                && !(iptVal.style.display == "none")
                && !(iptVal.style.visibility == "hidden")
            ) {
                eAlert(0, top._res_3147, top._res_3148);
                return;
            }
        }
    }

    if (saveas == null)
        saveas = 0;

    if (bApply == null || bApply == undefined) {
        bApply = 0;
    }

    //Pour savoir si le filtre a été sauvegardé ou pas
    bSaved = bApply == 0 || saveas == 1 || bSaved;

    if (bSaveAsLastFilter)
        bSaved = false;

    //Si le filtre sans nom on force l'enregistrement sous ou le filtre des notifications
    if (!bSaveAsLastFilter && !bApply && trim(document.getElementById("TxtFilterName").value) == "" && nfilterType != 1 && nfilterType != 3 && nfilterType != 4 && nfilterType != 9) {
        saveFilterAs();
        return;
    }

    //On compare les nouveaux parametres du filtre à ceux d'origines pour détecter s'il y a eu des modifs
    //La comparaison se fait avec leurs clés md5.
    var filterParams = getFilterParams();
    var bModified = eMD5.EncryptMd5(filterParams) != document.getElementById("TxtFilterParams").value;
    var sAction = document.getElementById("TxtFilterAction").value;

    // filtres avancé => modifier le filtre en cours => faire des modifs puis appliquer sans enregistrer 
    if (sAction == "edit" && bModified && !bSaved) {
        document.getElementById("TxtFilterName").value = "";
        document.getElementById("TxtFilterId").value = "0";
    }
    // filtres avancé => modifier le filtre en cours => Appliquer sans rien modifier
    if (sAction == "edit" && !bModified && !bSaveAsLastFilter && bApply) {
        top.checkIfFormular(nTab, document.getElementById("TxtFilterId").value);
        return;
    }

    var bViewPerm = 0;
    if (document.getElementById("ViewPerm").value == "1")
        bViewPerm = 1;
    var bUpdatePerm = 0;
    if (document.getElementById("UpdatePerm").value == "1")
        bUpdatePerm = 1;

    var upd = new eUpdater("mgr/eFilterWizardManager.ashx", 0);
    upd.addParam("maintab", nTab, "post");
    upd.addParam("filterid", document.getElementById("TxtFilterId").value, "post");
    upd.addParam("filterparams", filterParams, "post");
    upd.addParam("filtertype", document.getElementById("TxtFilterType").value, "post");
    upd.addParam("filtername", document.getElementById("TxtFilterName").value, "post");
    upd.addParam("userid", document.getElementById("TxtFilterUserId").value, "post");
    upd.addParam("viewperm", bViewPerm, "post");
    upd.addParam("updateperm", bUpdatePerm, "post");
    upd.addParam("saveas", saveas, "post");

    //Gestion d'erreur : a priori, pas de traitement particulier

    upd.ErrorCallBack = function () { top.setWait(false) };

    if (bViewPerm == 1) {
        upd.addParam("viewpermid", document.getElementById("TxtFilterViewPermId").value, "post");
        upd.addParam("viewpermmode", document.getElementById("TxtFilterViewPermMode").value, "post");
        upd.addParam("viewpermusersid", document.getElementById("TxtFilterViewPermUsersId").value, "post");
        upd.addParam("viewpermlevel", document.getElementById("TxtFilterViewPermLevel").value, "post");
    }
    if (bUpdatePerm == 1) {
        upd.addParam("updatepermid", document.getElementById("TxtFilterUpdatePermId").value, "post");
        upd.addParam("updatepermmode", document.getElementById("TxtFilterUpdatePermMode").value, "post");
        upd.addParam("updatepermusersid", document.getElementById("TxtFilterUpdatePermUsersId").value, "post");
        upd.addParam("updatepermlevel", document.getElementById("TxtFilterUpdatePermLevel").value, "post");
    }
    upd.addParam("applyfilter", bApply, "post");
    upd.addParam("action", "validfilter", "post");

    top.setWait(true);




    if (typeof (returnFct) == "function")
        upd.send(function (oRes) { top.setWait(false); returnFct(oRes); });
    else
        upd.send(saveFilterTreatment, bApply);
}

function beforeSaveFilter(bSaveAs) {

    var bViewPerm = 0;
    if (document.getElementById("ViewPerm").value == "1")
        bViewPerm = 1;
    var bUpdatePerm = 0;
    if (document.getElementById("UpdatePerm").value == "1")
        bUpdatePerm = 1;

    var filterName = document.getElementById("TxtFilterName").value;
    if (!filterName)
        bSaveAs = true;

    advancedDialog = new eModalDialog(top._res_6292, 0, "eFilterWizard.aspx", 400, 400);

    //CallBack d'erreur : on masque la modal
    advancedDialog.ErrorCallBack = launchInContext(advancedDialog, advancedDialog.hide);

    advancedDialog.addParam("filtertype", nfilterType, "post");
    advancedDialog.addParam("filtername", document.getElementById("TxtFilterName").value, "post");
    advancedDialog.addParam("type", document.getElementById("TxtFilterType").value, "post");
    advancedDialog.addParam("filterNameIsReadOnly", bSaveAs ? "0" : "1", "post");

    advancedDialog.addParam("viewperm", bViewPerm, "post");
    advancedDialog.addParam("updateperm", bUpdatePerm, "post");
    advancedDialog.addParam("userid", document.getElementById("TxtFilterUserId").value, "post");

    advancedDialog.addParam("viewpermid", document.getElementById("TxtFilterViewPermId").value, "post");
    advancedDialog.addParam("viewpermmode", document.getElementById("TxtFilterViewPermMode").value, "post");
    advancedDialog.addParam("viewpermusersid", document.getElementById("TxtFilterViewPermUsersId").value, "post");
    advancedDialog.addParam("viewpermlevel", document.getElementById("TxtFilterViewPermLevel").value, "post");

    advancedDialog.addParam("updatepermid", document.getElementById("TxtFilterUpdatePermId").value, "post");
    advancedDialog.addParam("updatepermmode", document.getElementById("TxtFilterUpdatePermMode").value, "post");
    advancedDialog.addParam("updatepermusersid", document.getElementById("TxtFilterUpdatePermUsersId").value, "post");
    advancedDialog.addParam("updatepermlevel", document.getElementById("TxtFilterUpdatePermLevel").value, "post");

    advancedDialog.addParam("action", "filtername", "post");

    advancedDialog.onIframeLoadComplete = (function (iframeId) { return function () { onDialogPermLoad(iframeId); } })(advancedDialog.iframeId);

    advancedDialog.show();

    advancedDialog.addButton(top._res_29, beforeSaveFilterCancel, "button-gray", ""); // Annuler  B

    if (nfilterType == 8)
        advancedDialog.addButton(top._res_28, beforeSaveFilterNotifValid, "button-green", bSaveAs ? "1" : "0"); // Valider
    else
        advancedDialog.addButton(top._res_28, beforeSaveFilterValid, "button-green", bSaveAs ? "1" : "0"); // Valider

}

function closeAndRefresh() {
    var modalDialog = null;
    var modalFilterList = null;
    if (top && top.window) {
        if (typeof (top.window['_md']['oModalFilterWizard']) != 'undefined') {
            modalDialog = top.window['_md']['oModalFilterWizard'];
            modalDialog.hide();
        }
    }
}



function beforeSaveFilterValid(saveas) {
    if (saveas == "1")
        beforeSaveFilterValidWithCheck(true);
    else
        beforeSaveFilterValidWithoutCheck(false);
}

function beforeSaveFilterNotifValid(saveas) {
    beforeSaveFilterValidWithoutCheck(saveas == "1" ? true : false);

}


function beforeSaveFilterValidWithoutCheck(saveas) {
    var returnValue = advancedDialog.getIframe().getRenameFilterValues(saveas);
    returnValue = returnValue + ";|;" + (saveas ? "1" : "0");

    beforeSaveFilterValidTreatment(returnValue);
}

function beforeSaveFilterValidWithCheck(saveas) {
    var returnValue = advancedDialog.getIframe().getRenameFilterValues(saveas);

    var aFilterInfos = returnValue.split(";|;");

    var filterName = aFilterInfos[0];
    var bPublicFilter = aFilterInfos[1];

    if (trim(filterName) == "") {
        eAlert(0, top._res_274, top._res_274, '', 700, 200);
        return;
    }

    //teste si le nom existe

    var upd = new eUpdater("mgr/eFilterWizardManager.ashx", 0);
    upd.addParam("filtername", filterName, "post");
    upd.addParam("saveas", (saveas ? "1" : "0"), "post");
    upd.addParam("filterid", document.getElementById("TxtFilterId").value, "post");
    upd.addParam("action", "checkfiltername", "post");
    //Gestion d'erreur : a priori, pas de traitement particulier
    upd.ErrorCallBack = function () { };

    upd.send(beforeSaveFilterValidWithCheckTreatment, returnValue + ";|;" + (saveas ? "1" : "0"));
}

function beforeSaveFilterValidWithCheckTreatment(oRes, returnValue) {
    var cnt = parseInt(getXmlTextNode(oRes.getElementsByTagName("count")[0]));
    var aFilterInfos = returnValue.split(";|;");

    var filterName = aFilterInfos[0];

    if (cnt > 0) {
        eAlert(0, top._res_203, top._res_440.replace("<NAME>", filterName), '', 600, 170);
        return;
    }

    beforeSaveFilterValidTreatment(returnValue);
}

function beforeSaveFilterValidTreatment(returnValue) {

    var aFilterInfos = returnValue.split(";|;");
    var filterName = aFilterInfos[0];

    // Appartenance
    var bPublicFilter = aFilterInfos[1];
    var userid = document.getElementById("TxtFilterUserId").value;
    // Affecter la valeur du userid selon l'appartenance du filtre versus public
    if (bPublicFilter == "1")
        userid = "0";       // UserId = 0 : Filtre public
    else if (userid == "0")
        userid = "-1";      // UserId = -1 : Le cs reprendra le userid courant (modification d'un filtre public vers un filtre non public)
    document.getElementById("TxtFilterUserId").value = userid;

    // View
    var bViewPerm = aFilterInfos[2];
    document.getElementById("ViewPerm").value = bViewPerm;
    var viewPermUsersId = aFilterInfos[3];
    document.getElementById("TxtFilterViewPermUsersId").value = viewPermUsersId;
    var viewPermLevel = aFilterInfos[4];
    document.getElementById("TxtFilterViewPermLevel").value = viewPermLevel;
    var viewPermMode = aFilterInfos[5];
    document.getElementById("TxtFilterViewPermMode").value = viewPermMode;

    // Update
    var bUpdatePerm = aFilterInfos[6];
    document.getElementById("UpdatePerm").value = bUpdatePerm;
    var updatePermUsersId = aFilterInfos[7];
    document.getElementById("TxtFilterUpdatePermUsersId").value = updatePermUsersId;
    var updatePermLevel = aFilterInfos[8];
    document.getElementById("TxtFilterUpdatePermLevel").value = updatePermLevel;
    var updatePermMode = aFilterInfos[9];
    document.getElementById("TxtFilterUpdatePermMode").value = updatePermMode;

    //Nom du filtre
    document.getElementById("TxtFilterName").value = filterName;

    var saveas = aFilterInfos[10];

    saveDb(0, saveas);

}

function getRenameFilterValues(saveas) {

    var strReturn = "";
    var filterName = saveas ? document.getElementById("PermName").value : GetText(document.getElementById("PermName"));
    var bPublicFilter = "0";
    var bViewPerm = 0;
    var viewPermUsersId = "";
    var viewPermLevel = "";
    var viewPermMode = "";
    var bUpdatePerm = 0;
    var updatePermUsersId = "";
    var updatePermLevel = "";
    var updatePermMode = "";

    if (document.getElementById("DivOptBlock")) {
        bPublicFilter = document.getElementById("OptPublicFilter").checked ? "1" : "0";
        var oPermView = getPermReturnValue("View");
        bViewPerm = oPermView.Opt ? 1 : 0;
        viewPermUsersId = oPermView.users;
        viewPermLevel = oPermView.levels;
        viewPermMode = oPermView.perMode;

        var oPermUpd = getPermReturnValue("Update");
        bUpdatePerm = oPermUpd.Opt ? 1 : 0;
        updatePermUsersId = oPermUpd.users;
        updatePermLevel = oPermUpd.levels;
        updatePermMode = oPermUpd.perMode;
    }
    return filterName + ";|;" + bPublicFilter + ";|;" + bViewPerm + ";|;" + viewPermUsersId + ";|;" + viewPermLevel + ";|;" + viewPermMode + ";|;" +
        bUpdatePerm + ";|;" + updatePermUsersId + ";|;" + updatePermLevel + ";|;" + updatePermMode;
}

function beforeSaveFilterCancel() {
    advancedDialog.hide();
}

function applyFilter() {
    saveDb(1);
}

function saveFilterTreatment(oRes, bApply) {
    top.setWait(false);

    var filterid = getXmlTextNode(oRes.getElementsByTagName("filterid")[0]);
    document.getElementById("TxtFilterId").value = filterid;
    if (bApply + "" == "1") {
        top.checkIfFormular(nTab, filterid);
    }
    else {
        //Mise à jour du nom du filtre si l'appel provient depuis "enregisrter sous"
        var filterName = document.getElementById("TxtFilterName").value;
        if (document.getElementById("FilterTitleDiv").innerText != null)
            document.getElementById("FilterTitleDiv").innerText = filterName;
        else
            document.getElementById("FilterTitleDiv").textContent = filterName;
        try {
            if (advancedDialog != null)
                advancedDialog.hide();
        } catch (exp) { }
    }



    if (typeof (top.window['_md']['oModalFilterWizard']) != 'undefined') {
        var myModDialog = top.window['_md']['oModalFilterWizard'];
        if (typeof (myModDialog.getParam("CalllBackSuccess")) == "function") {
            var fct = myModDialog.getParam("CalllBackSuccess");
            fct(oRes);
        }
    }

    if (nfilterType == 2) {
        closeAndRefresh();
    }
}


//#endregion

//#region Calendar Select
function onSelectRadio(radioId) {
    var _lst = document.getElementById('lstMove');
    var _isVisible = (radioId == 1 || radioId == 4 || radioId == 5 || radioId == 6 || radioId == 7);
    if (!_isVisible) {
        //_lst.style.display = 'none';
        _lst.value = 0;
        document.getElementById('DivMove').style.display = 'none';
    }
    else {
        var strValue = '';
        if (radioId == 1 || radioId == 7) {

            strValue = top._res_853;
        }
        else if (radioId == 5) {
            strValue = top._res_852;
        }
        else if (radioId == 4) {
            strValue = top._res_854;
        }
        else if (radioId == 6) {
            strValue = top._res_855;
        }
        document.getElementById('DivMove').style.display = "block";

        if (document.getElementById('LabelMove').innerText) {
            document.getElementById('LabelMove').innerText = strValue;
        } else {
            document.getElementById('LabelMove').textContent = strValue;
        }
        //_lst.style.display = 'block';
    }
    if (radioId == 3) {
        document.getElementById('dateValue').focus();
    }
    if (radioId == 1 || radioId == 4 || radioId == 3) {
        document.getElementById('DivNoYear').style.display = 'block';
    }
    else {
        document.getElementById('ChkNoYear').checked = 0;
        document.getElementById('chk_ChkNoYear').className = document.getElementById('chk_ChkNoYear').className;
        document.getElementById('chk_ChkNoYear').setAttribute("chk", "0");
        document.getElementById('DivNoYear').style.diaplay = 'none';
    }
}
function getReturnValue() {
    var strReturn = "";

    var radioObj = document.forms['radioOptForm'].elements['date'];

    if (radioObj[0].checked)
        strReturn = '';
    else if (radioObj[1].checked)
        strReturn = '<DATE>';
    else if (radioObj[2].checked)
        strReturn = '<DATETIME>';
    else if (radioObj[4].checked)
        strReturn = '<MONTH>';
    else if (radioObj[5].checked)
        strReturn = '<WEEK>';
    else if (radioObj[6].checked)
        strReturn = '<YEAR>';
    else {
        var oCalendar = eCalendarControl;
        //Calendrier
        //GCH - #36019 - Internationnalisation - Choix de dates
        outDate = eDate.Tools.GetStringFromDate(oCalendar.calDate, true, false);
        var oHour = document.getElementById(oCalendar.HourID);
        var oMin = document.getElementById(oCalendar.MinID);
        var nHour;
        var nMin;

        var bNoHour = false;

        if (!(oMin && oHour))
            bNoHour = true;


        if (!bNoHour) {
            nHour = getNumber(oHour.value);
            nMin = getNumber(oMin.value);

            if (isNaN(nHour) || isNaN(nMin))
                bNoHour = true;
        }

        if (!bNoHour) {

            bDateFailed = (nHour < 0 || nHour > 23 || nMin < 0 || nMin > 59);

            if (bDateFailed) {
                eAlert(0, top._res_6275, top._res_470, '', 500, 200, new function () { document.getElementById(oCalendar.HourID).select(); document.getElementById(oCalendar.HourID).focus(); });
                return;
            }
            else {
                outDate += ' ' + eDate.Tools.MakeTwoDigit(nHour) + ':' + eDate.Tools.MakeTwoDigit(nMin);
            }
        }
        document.getElementById('dateValue').value = outDate;
        strReturn = document.getElementById('dateValue').value;

    }

    /*else if( document.getElementById('7').checked )
    var strReturn = '<DAY>';*/

    if (document.getElementById('lstMove').value != 0) {
        strReturn = strReturn + ' ' + document.getElementById('lstMove').value;
    }

    if (document.getElementById('ChkNoYear').checked == true) {
        strReturn = strReturn + '[NOYEAR]';
    }

    return strReturn;

}
function trim(strTrim) {
    return strTrim.replace(/(^\s*)|(\s*$)/g, "");
}
function IsDate() {
    var strDate = document.getElementById("dateValue").value;
    if (strDate != '') {
        strDate = parseDate(strDate);
        if (strDate == '') {
            eAlert(0, top._res_231, top._res_959, '', null, null, function () { document.getElementById("dateValue").focus(); });
        }
        else {
            document.getElementById("dateValue").value = strDate;
        }
    }
}


function parseDate(strDate) {
    var strDate = trim(strDate);

    var aFullDate = strDate.split(" ");

    var strFullDate = aFullDate[0];

    var strFullDate = strFullDate.replace(/\./g, "/");
    var strFullDate = strFullDate.replace(/\-/g, "/");


    if ((strFullDate.length == 4 || strFullDate.length == 8 || strFullDate.length == 6) && strFullDate.indexOf('/') == -1)
        strFullDate = strFullDate.substring(0, 2) + '/' + strFullDate.substring(2, 4) + '/' + strFullDate.substring(4, strFullDate.length);


    var aDate = strFullDate.split("/");

    var strDay = '';
    var strMonth = '';
    var strYear = '';

    if (aDate.length >= 1)
        strDay = aDate[0];
    if (aDate.length >= 2)
        strMonth = aDate[1];
    if (aDate.length >= 3) {
        strYear = aDate[2];
        if (strYear.length > 4)
            strYear = strYear.substring(0, 4);
    }

    if (parseInt(strDay, 10) < 10 && strDay.length == 1)
        strDay = '0' + parseInt(strDay, 10);


    if (parseInt(strMonth, 10) < 10 && strMonth.length == 1)
        strMonth = '0' + parseInt(strMonth, 10);


    if (isNaN(strDay) || isNaN(strMonth) || (strDay == '' || strMonth == '' || parseInt(strDay, 10) > 31 || parseInt(strMonth, 10) < 1 || parseInt(strMonth, 10) > 12) || strYear.length == 3) {
        strDate = '';
    }


    else if (strYear == '' || !strYear) {
        var dCurrentDate = new Date();
        strDate = strDay + '/' + strMonth + '/' + dCurrentDate.getFullYear();
    }
    else if (parseInt(strYear, 10) < 100) {
        if (strYear.length == 1)
            strYear = '0' + strYear;

        if (parseInt(strYear, 10) > 50)
            var strCurrentYear = '19';
        else
            var strCurrentYear = '20';
        strDate = strDay + '/' + strMonth + '/' + strCurrentYear + strYear;
    }
    else if ((parseInt(strYear, 10) < 1753 || parseInt(strYear, 10) > 9999) || isNaN(strYear)) {
        strDate = '';
    }
    else {
        strDate = strDay + '/' + strMonth + '/' + strYear;
    }

    if (aFullDate.length == 2)
        strDate = strDate + ' ' + aFullDate[1];
    return (strDate);
}
//#endregion




function resizeFilterMainDiv(height) {


    if (Number(height) > 0 && document.getElementById('MainFilterDiv')) {
        document.getElementById('MainFilterDiv').style.height = Number(height) + 'px';
    }
    else {
        var modalDialog = null;

        if (top && top.window) {
            if (typeof (top.window['_md']['oModalFilterWizard']) != 'undefined')
                modalDialog = top.window['_md']['oModalFilterWizard'];
            else if (typeof (top.window['_md']['oModalFilterForm']) != 'undefined')
                modalDialog = top.window['_md']['oModalFilterForm'];
            else if (typeof (top.window['_md']['advancedDialog']) != 'undefined')
                modalDialog = top.window['_md']['advancedDialog'];
        }
        if (!modalDialog && parent && parent.window) {
            if (typeof (parent.window['_md']['oModalFilterWizard']) != 'undefined')
                modalDialog = parent.window['_md']['oModalFilterWizard'];
            else if (typeof (parent.window['_md']['oModalFilterForm']) != 'undefined')
                modalDialog = parent.window['_md']['oModalFilterForm'];
            else if (typeof (parent.window['_md']['advancedDialog']) != 'undefined')
                modalDialog = parent.window['_md']['advancedDialog'];
        }

        if (document.getElementById('MainFilterDiv') && modalDialog != null) {
            var ifram = modalDialog.getIframe();

            // ASY/HLA - Compatibilité IE8
            if (typeof (ifram.innerHeight) == "undefined")
                document.getElementById('MainFilterDiv').style.height = modalDialog.getIframe().document.documentElement.clientHeight - 2 + 'px';
            else
                document.getElementById('MainFilterDiv').style.height = modalDialog.getIframe().innerHeight - 15 + 'px';
        }
    }
}


function onFrameSizeChange(w, h) {
    resizeFilterMainDiv(h);
}

// Suppression du filtre
function deleteFilter(nFilterId, nTab, fctCallback) {
    var url = "mgr/eFilterWizardManager.ashx";
    var upd = new eUpdater(url, 0);
    upd.addParam("action", "delete", "post");
    upd.addParam("maintab", nTab, "post");
    upd.addParam("filterid", nFilterId, "post");
    upd.ErrorCallBack = function () { };
    upd.send(fctCallback);
}



nsFilterWizard.selectOptionDefaultFilter = function (obj) {

    if (obj.name === "applyDefaultFilter") {

        //désactive le disablable si pas apply
        var applyed = document.querySelector('input[name=' + obj.name + ']:checked')
        var doc = document.getElementById("disableDefaultFilter");
        if (applyed.value == "1") {
            doc.style.display = "";

        }
        else {

            doc.style.display = "none";
        }
    }



}