//*****************************************************************/
//***** FONCTIONS COMMUNES SELECTION DES ONGLETS ET RUBRIQUES *****/
//*****************************************************************/

function SelectItem(oFromId, oToId) {



    var oTo = document.getElementById(oToId).getElementsByTagName("div");
    var oFrom = document.getElementById(oFromId).getElementsByTagName("div");
    if (oFrom.length <= 0)
        return;

    var Current = getSelectedIndex(document.getElementById(oFromId));

    // ASY :(22124) choix des rubriques postale
    //  SPH -> si on vient d'un wizard, la propriéte descid n'est pas rempli, le descid est alors dans la propriété value
    //  le sélectionneur de champ attend dans certaines partie, notament, la sélection automatique des rubriques postale
    // des propriétés et des éléments ne se trouvant que dans le choix des rubriques des modes listes. Pour les rubriques des wizard,
    // cette gestion des adresses postales est faite différment. (cf eReportWizard.js)
    nDescId = getAttributeValue(oFrom[Current], "DescId");

    if (Current != -1) {
        if (document.getElementById(oToId) != null) {

            var currTargetIdx = getSelectedIndex(document.getElementById(oToId));
            var oElt = document.getElementById(oFromId).removeChild(oFrom[Current]);

            adjustLabel(oElt, oToId);

            if (currTargetIdx < 0 || currTargetIdx >= oTo.length)
                document.getElementById(oToId).appendChild(oElt);
            else {
                var aDivs = document.createElement("div");

                var idxD = 0;
                while (oTo.length > 0) {
                    aDivs.appendChild(oTo[0]);

                    if (idxD == currTargetIdx) {
                        aDivs.appendChild(oElt);
                    }

                    idxD++;

                }

                var aDivsList = aDivs.getElementsByTagName("div");
                var idxD = 0;
                while (aDivsList.length > 0) {
                    document.getElementById(oToId).appendChild(aDivsList[0]);
                    idxD++;
                }

            }

        }

        // ASY :(22124) choix des rubriques postales
        if (oToId == "ItemsUsed")
            AdrSelect(oFrom, nDescId);

        setCssList(document.getElementById(oToId), "cell", "cell2");
        setCssList(document.getElementById(oFromId), "cell", "cell2");

        // setSelectedIndex(document.getElementById(oFromId), 0);

        setSelectedIndex(document.getElementById(oToId), currTargetIdx + 1);



    }
}

// ASY :(22124) choix des rubriques postales
function onAddPostalFields(oFrom, nDescId) {

    var nNextDescId = 0;
    if (nDescId == 402 || nDescId == 404 || nDescId == 407 || nDescId == 409 || nDescId == 410 || nDescId == 302 || nDescId == 304 || nDescId == 307 || nDescId == 309 || nDescId == 310) {
        nNextDescId = nDescId;
        bStop = false;

        while (!bStop) {
            if ((nNextDescId == 303 || nNextDescId == 403))
                bStop = true;

            for (i = 0; i <= oFrom.length - 1; i = i + 1) {

                tmpDescID = getAttributeValue(oFrom[i], "DescID");

                if (tmpDescID == nNextDescId) {

                    oFrom.selectedIndex = i;
                    setElementSelected(oFrom[i]);
                    AddItem(oFrom, nNextDescId);
                    break;

                }
            }
            nNextDescId = GetNextDescId(nNextDescId, oFrom);

        }

        setCssList(oFrom = document.getElementById(oFrom.id), "cell", "cell2");

    }
}

function AdrSelect(oFrom, nDescId) {

    if ((nDescId == 402 || nDescId == 302)) {
        eConfirm(1, '', top._res_6584, '', 500, 200, function () { onAddPostalFields(oFrom, nDescId) }, function () { });
    }



}

function GetNextDescId(nDescId, oFrom) {

    var bOk = false;
    var nNextDescId = 0;
    var nNextDescId = nDescId;

    if (nDescId == 302) nNextDescId = 304;
    if (nDescId == 304) nNextDescId = 307;
    if (nDescId == 307) nNextDescId = 309;
    if (nDescId == 309) nNextDescId = 310;
    if (nDescId == 310) nNextDescId = 303;

    if (nDescId == 402) nNextDescId = 404;
    if (nDescId == 404) nNextDescId = 407;
    if (nDescId == 407) nNextDescId = 409;
    if (nDescId == 409) nNextDescId = 410;
    if (nDescId == 410) nNextDescId = 403;

    if (nNextDescId == 0)
        return;

    for (i = 0; i <= oFrom.length - 1; i = i + 1) {
        tmpDescID = getAttributeValue(oFrom[i], "DescID")
        if (tmpDescID == nNextDescId) {
            bOk = true;
            break;
        }
    }

    if (bOk == true || nNextDescId == 303 || nNextDescId == 403) {
        return nNextDescId;
    }
    else {
        return GetNextDescId(nNextDescId, oFrom);
    }
}


function getSelectedIndex(oList) {
    var selIdx = -1;

    if (oList.getElementsByTagName("div").length > 0) {
        selIdx = oList.getAttribute("SelectedIndex");

        // Par défaut, si aucun index n'a été défini, on considère que le premier élément de la liste est celui sélectionné
        if (selIdx == null || typeof (selIdx) == "undefined")
            selIdx = 0;

        // Si l'index sélectionné correspond au "guide", il faut tenter de sélectionner la cellule située après, ou avant
        if (oList.getElementsByTagName("div")[selIdx] != null && oList.getElementsByTagName("div")[selIdx].getAttribute("syst") != null) {
            if (oList.getElementsByTagName("div")[selIdx + 1] != null) {
                selIdx++;
            }
            else {
                if (oList.getElementsByTagName("div")[selIdx - 1] != null)
                    selIdx--;
                else
                    selIdx = -1; // il ne reste plus que le guide dans la liste = plus aucune sélection possible
            }
        }
            // Si l'index sélectionné ne correspond à aucune cellule (index supérieur au nombre d'éléments de la liste)
            // on positionne la sélection sur le dernier élément de la liste
        else {
            if (oList.getElementsByTagName("div")[selIdx] == null) {
                selIdx = oList.getElementsByTagName("div").length - 1;
            }
            // Si le tout dernier élément de la liste est le guide, on se positionne sur l'élément antérieur
            if (oList.getElementsByTagName("div")[selIdx] != null && oList.getElementsByTagName("div")[selIdx].getAttribute("syst") != null)
                selIdx--;

            // Si l'index ne correspond toujours pas à un élément, on considère qu'il n'y a plus d'élément sélectionnable
            if (oList.getElementsByTagName("div")[selIdx] == null ||
                (oList.getElementsByTagName("div")[selIdx] != null && oList.getElementsByTagName("div")[selIdx].getAttribute("syst") != null)
            )
                selIdx = -1;
        }
    }


    return parseInt(selIdx);
}

function getSelectedElement(oList) {
    var selIdx = getSelectedIndex(oList);
    return oList.getElementsByTagName("div")[selIdx];
}

function getSelectedDescId() {
    var strReturn = "";
    // Sélection d'onglets...
    var oRootElement = document.getElementById('TabSelectedList');
    var strValueAttribute = "DescId";
    // ...ou sélection des rubriques
    if (!oRootElement) {
        oRootElement = document.getElementById('ItemsUsed');
        strValueAttribute = "value";
    }
    if (!oRootElement)
        return;
    var oUsed = oRootElement.getElementsByTagName("div");
    if (!oUsed)
        return;
    for (var i = 0; i < oUsed.length; i++) {
        if (!isNaN(getNumber(oUsed[i].getAttribute(strValueAttribute)))) {
            if (strReturn != "")
                strReturn += ";";
            strReturn += oUsed[i].getAttribute(strValueAttribute);
        }
    }
    return strReturn;
}

function getSelectedDescIdObject() {
    var strReturn = "";
    var strReturnLib = "";
    // Sélection d'onglets...
    var oRootElement = document.getElementById('TabSelectedList');
    var strValueAttribute = "DescId";
    // ...ou sélection des rubriques
    if (!oRootElement) {
        oRootElement = document.getElementById('ItemsUsed');
        strValueAttribute = "value";
    }
    if (!oRootElement)
        return;
    var oUsed = oRootElement.getElementsByTagName("div");
    if (!oUsed)
        return;
    for (var i = 0; i < oUsed.length; i++) {
        if (!isNaN(getNumber(oUsed[i].getAttribute(strValueAttribute)))) {
            if (strReturn != "") {
                strReturn += ";";
                strReturnLib += ";";
            }

            strReturn += getAttributeValue(oUsed[i], strValueAttribute); 
            strReturnLib += getAttributeValue(oUsed[i], "shlb");
        }
    }

    return {
        dbv: strReturn,
        lib: strReturnLib
    };
}


function setElementSelected(element) {

    if (element.className == "SelectedItem")
        return;

    var oList = element.parentNode;
    var aList = oList.getElementsByTagName("div")

    for (i = 0; i < aList.length; i++) {
        if (element == aList[i]) {
            aList[i].setAttribute("oldCss", aList[i].className);
            aList[i].className = "SelectedItem";
            oList.setAttribute("SelectedIndex", i)
        }
        else {
            // Pas d'action si l'élément actuellement visé est l'indicateur de drag & drop
            if (aList[i].getAttribute("syst") == null)
                aList[i].className = aList[i].getAttribute("oldCss");
        }
    }

}


function setSelectedIndex(oList, idx) {
    var aList = oList.getElementsByTagName("div")
    if (aList.length >= idx) {
        for (i = 0; i < aList.length; i++) {
            if (aList[i].getAttribute("syst") == null) {
                if (i == idx) {
                    if (aList[i].className == "SelectedItem")
                        return;

                    if (aList[i].getAttribute("oldCss") != aList[i].className)
                        aList[i].setAttribute("oldCss", aList[i].className);

                    aList[i].className = "SelectedItem";
                    oList.setAttribute("SelectedIndex", idx)
                }
                else {
                    if (aList[i].getAttribute("oldCss") != aList[i].className)
                        aList[i].className = aList[i].getAttribute("oldCss");
                }
            }
        }
    }
    else {
        oList.setAttribute("SelectedIndex", idx)
    }
}

function doInitSearch(oList, e) {
    var eltSearch = document.getElementById("srch");
    if (!eltSearch) {
        return;
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

    if (bIsTablet)
        return;

    eltSearch.focus();
    eltSearch.value = "";

    eltSearch.setAttribute("lst", oList.id);
}

var toRstSrch;

function resetSearch() {
    document.getElementById("srch").value = "";
}
function onSrchType(srch) {
    clearTimeout(toRstSrch);
    toRstSrch = setTimeout(function () { doSearch(srch); }, 300);
}

function doSearch(srch) {
    if (srch.value == "")
        return;


    var oList = document.getElementById(getAttributeValue(srch, "lst"));
    if (!oList)
        return;

    var aList = oList.getElementsByTagName("div")
    var selElt;
    var selIdx = getSelectedIndex(oList);
    for (var j = 0; j < aList.length; j++) {
        var i = (j + selIdx + 1) % aList.length;
        if (getAttributeValue(aList[i], "shlb").toLowerCase().indexOf(srch.value.toLowerCase()) == 0
            || (getAttributeValue(aList[i], "shlb") == "" && GetText(aList[i]).toLowerCase().indexOf(srch.value.toLowerCase()) == 0)) {
            selElt = aList[i];
            setElementSelected(selElt);

            var oDivScroll = oList.parentElement;
            if (getAttributeValue(aList[i], "shlb") == "")
                oDivScroll = oList;
            var posDivScroll = getAbsolutePositionWithScroll(oDivScroll);


            var posElt = getAbsolutePosition(selElt);
            //si l'élément sélectionné se trouve en dessous de la zone visible 
            if (posElt.y + posElt.h > posDivScroll.y + posDivScroll.h) {
                newY = posDivScroll.y + posDivScroll.h - posElt.h;
                scrollEltToY(oDivScroll, selElt, newY);
            }
                //si l'élément sélectionné se trouve au dessus de la zone visible 
            else if (posElt.y < posDivScroll.y) {
                scrollEltToY(oDivScroll, selElt, posDivScroll.y);
            }

            break;
        }
    }
    document.getElementById("srch").value = "";
}

function setCssList(oList, css1, css2) {
    if (oList == null)
        return;
    var css = css1;
    var oElements = oList.getElementsByTagName("div");
    for (var i = 0; i < oElements.length; i++) {
        if (oElements[i].getAttribute("syst") == null) {
            if (css == css1)
                css = css2;
            else
                css = css1;

            oElements[i].className = css;
            oElements[i].setAttribute("oldCss", css);
        }
    }
}

function MoveAllItems(oFromId, oToId) {
    var oTo = document.getElementById(oToId).getElementsByTagName("div");
    var oFrom = document.getElementById(oFromId).getElementsByTagName("div");
    var len = oFrom.length;
    var allElementsRemoved = false;
    while (oFrom.length > 0 && !allElementsRemoved) {
        var elementToRemove = oFrom[0];
        if (elementToRemove.getAttribute("syst") != null) {
            elementToRemove = oFrom[1];
        }
        if (elementToRemove != null) {
            var oElement = document.getElementById(oFromId).removeChild(elementToRemove);
            adjustLabel(oElement, oToId);
            document.getElementById(oToId).appendChild(oElement);
        }
        if (oFrom.length == 1 && oFrom[0].getAttribute("syst") != null)
            allElementsRemoved = true;
    }

    if (document.getElementById(oToId).getAttribute("syst") == null)
        setCssList(document.getElementById(oToId), "cell", "cell2");
    else
        document.getElementById(oToId).className = "dragGuideTab";

    if (document.getElementById(oFromId).getAttribute("syst") == null)
        setCssList(document.getElementById(oFromId), "cell", "cell2");
    else
        document.getElementById(oFromId).className = "dragGuideTab";
}

function MoveComboItem(bUp) {
    var cbo = document.getElementById("TabSelectedList");
    if (!cbo)
        cbo = document.getElementById("ItemsUsed");
    if (!cbo)
        return;
    var divList = cbo.getElementsByTagName("div");
    var selectedIndex = getSelectedIndex(cbo);
    if (selectedIndex == -1) {
        if (divList.length > 0)
            setSelectedIndex(cbo, 0);
        return;
    }
    if (bUp && selectedIndex <= 0)
        return;
    if (!bUp && selectedIndex < 0 && selectedIndex >= divList.length - 1)
        return;


    var nIndex; // Nouvel index pour deplacement

    if (bUp)
        nIndex = selectedIndex - 1;
    else
        nIndex = selectedIndex + 1;

    if (!divList[nIndex])
        return;
    var srcDiv = divList[selectedIndex];
    var trgDiv = divList[nIndex];

    var divTmp = document.createElement("div");
    divTmp.style.display = 'none';
    document.body.appendChild(divTmp);

    var nbrElm = divList.length;

    for (i = 0; i < nbrElm; i++) {
        if (i == nIndex) {
            divTmp.appendChild(srcDiv);
        }
        else
            if (i == selectedIndex) {
                divTmp.appendChild(trgDiv);
            }
            else {
                divTmp.appendChild(divList[0]);
            }
    }
    var newDivList = divTmp.getElementsByTagName("div");
    for (i = 0; i < nbrElm; i++) {
        cbo.appendChild(newDivList[0]);
    }

    // DEMANDE 80 168 - Ergonomie sélection des rubriques avec barre de défilement qui ne suit pas les rubriques SEM - QBO
    var correction = 0;
    var eudonetX = parent.document.getElementsByClassName('switch-new-theme-wrap')[0].classList.contains('activated');
    if (eudonetX) {
        correction = 70
    }
    cbo.scrollTop = srcDiv.offsetTop - correction;

    setCssList(cbo, "cell", "cell2");
    setSelectedIndex(cbo, nIndex);
}



//*********************************************************/
//***** FONCTIONS SPECIFIQUES SELECTION DES RUBRIQUES *****/
//*********************************************************/

function setTabFields() {
    var oSel = document.getElementById("MainFileList");
    var descId = oSel.options[oSel.selectedIndex].value;
    var mainTab = oSel.getAttribute("MainTab");
    url = "eFieldsSelect.aspx";

    var forceListMode = false;
    if (typeof (document.getElementById("forcelistmode")) != 'undefined' && document.getElementById("forcelistmode") != null)
        forceListMode = document.getElementById("forcelistmode").value == "1";

    //Appel Ajax
    var ednu = new eUpdater(url, 1);
    ednu.ErrorCallBack = function () { setWait(false); }
    ednu.addParam("action", "loadtab", "post");
    ednu.addParam("tabtoload", descId, "post");
    ednu.addParam("forcelst", forceListMode ? "1" : "0", "post");
    ednu.addParam("itemused", getSelectedDescId(), "post");
    ednu.addParam("Tab", mainTab, "post");
    ednu.send(setTabFieldsTreatment);
}

function setTabFieldsTreatment(oDoc) {
    var divSource = document.getElementById("DivSourceList");
    divSource.innerHTML = oDoc;
    initDragOpt();  //On réinitialise le drag&drop afin qu'il se câble sur les nouveaux tableaux créés (demande #20930 et #32501)
}

function AddItem() {
    var oTo = document.getElementById('ItemsUsed');
    var tabDescId = document.getElementById("MainFileList").options[document.getElementById("MainFileList").selectedIndex].value;
    var oFrom = document.getElementById("FieldList_" + tabDescId);
    SelectItem(oFrom.id, oTo.id);
}

function AddAll() {
    var oTo = document.getElementById('ItemsUsed');
    var tabDescId = document.getElementById("MainFileList").options[document.getElementById("MainFileList").selectedIndex].value;
    var oFrom = document.getElementById("FieldList_" + tabDescId);
    MoveAllItems(oFrom.id, oTo.id);
}

function DelItem() {
    var oFrom = document.getElementById('ItemsUsed');
    var tabDescId = document.getElementById("MainFileList").options[document.getElementById("MainFileList").selectedIndex].value;
    var oElt = getSelectedElement(oFrom);

    // Il ne faut pas supprimer d'élément s'il ne correspond à rien, ou s'il s'agit du "guide"
    if (typeof (oElt) != "undefined" && oElt && oElt.getAttribute("field_list") == null && oElt.getAttribute("syst") == null) {
        //KHA le 18/01/13 : ex : si l'onglet Société est sélectionné et que l'on retire de la liste des rubriques selectionnées une rubrique appartenant à Adresse 
        // on ne replace pas cette rubrique dans la liste des rubriques de société
        if (tabDescId != getAttributeValue(oElt, "edntab")) {
            oFrom.removeChild(oElt);
            setCssList(oFrom, "cell", "cell2");
            setSelectedIndex(oFrom, 0);
            return;
        }
    }


    var oTo = document.getElementById("FieldList_" + tabDescId);
    SelectItem(oFrom.id, oTo.id);
}

function DelAll() {
    var oTo = document.getElementById('ItemsUsed');
    var tabDescId = document.getElementById("MainFileList").options[document.getElementById("MainFileList").selectedIndex].value;
    var oFrom = document.getElementById("FieldList_" + tabDescId);
    MoveAllItems(oTo.id, oFrom.id);
}

//*******************************************************/
//***** FONCTIONS SPECIFIQUES SELECTION DES ONGLETS *****/
//*******************************************************/

var oModalAdd = null;
var selectionToRenameId = "";

function sortList(oList) {
    var tmpAry = new Array();
    for (var i = 0; i < oList.options.length; i++) {
        tmpAry[i] = new Array();
        tmpAry[i][0] = oList.options[i].text;
        tmpAry[i][1] = oList.options[i].value;
    }
    tmpAry.sort();
    while (oList.options.length > 0) {
        oList.options[0] = null;
    }
    for (var i = 0; i < tmpAry.length; i++) {
        var op = new Option(tmpAry[i][0], tmpAry[i][1]);
        oList.options[i] = op;
    }

    setCssList(oList, "cell", "cell2");
    return;
}

function setSelection(oList) {

}

function setSelectedTreatment(oDoc) {

}

function ShowHideAdvanced(divTitle, topModal) {

    var divLeftResize = document.getElementById("AllTabList");
    var divRightResize = document.getElementById("TabSelectedList");

    var divSelection = document.getElementById("DivSelectionList");

    if (divTitle.getAttribute("closed") == "1") {

        // J'ai enlevé cette valeur car elle n'a plus lieu d'être, étant donné que la taille de la fenêtre ne change plus.
        //topModal.resizeTo(null, 750);

        divLeftResize.style.height = "155px";
        divRightResize.style.height = "155px";

        divSelection.style.display = "block";
        divTitle.className = "DivOpen";
        divTitle.setAttribute("closed", "0");

    }
    else {
        topModal.resizeTo(null, 550);
        divSelection.style.display = "none";
        divTitle.className = "DivClosed";
        divTitle.setAttribute("closed", "1");
        divLeftResize.style.height = "300px";
        divRightResize.style.height = "300px";

    }
}

//*******************
//** drag & drop
//*******************

var _Drag_srcElement = null;
var _Drag_trgElement = null;
var _Drag_dragApproved = false;
var Drag_divTmp = null;
var parentFrom = null;


Drag_divTmp = document.getElementById("Drag_divTmp");
//document.onselectstart = new Function("return false");
document.onmousemove = "doOnMouseMove(event);"

function doOnMouseDown(element) {
    return;
    /*
    window.status = element.id + "***" + element.innerHTML;
    _Drag_srcElement = element;
    document.getElementById("Drag_divTmp").innerHTML = element.innerHTML;
    _Drag_dragApproved = true;
    */
}

function doOnMouseMove(e) {
    return;
    /*
    if (_Drag_dragApproved == true) {
    document.getElementById("Drag_divTmp").style.display = "block";

    //Coordonnées de la souris
    var x = (e) ? e.clientX : event.clientX;
    var y = (e) ? e.clientY : event.clientY;

    document.getElementById("Drag_divTmp").style.top = parseInt(y + 5) + "px";
    document.getElementById("Drag_divTmp").style.left = parseInt(x + 5) + "px";

    }
    else {
    document.getElementById("Drag_divTmp").style.display = "none";
    _Drag_dragApproved = false;
    }
    */
}

function doOnMouseOver(element) {
    return;
    /*
    _Drag_trgElement = element;
    parentFrom = element.parentNode;
    document.getElementById("Drag_divTmp").innerHTML = parentFrom.id;
    //_Drag_trgElement.parentNode.scrollTop = _Drag_trgElement.parentNode.scrollTop + 1;
    */
}

function doOnMouseUp() {
    return;


}

//document.onselectstart = function () { return false; };

function doOnSelectionclick(selId) {
    url = "eTabsSelectDiv.aspx";

    //Chargement de la liste pour la table selectionnée
    var ednu = new eUpdater(url, 1);
    ednu.addParam("action", "loadselection", "post");
    ednu.addParam("selid", selId, "post");
    ednu.ErrorCallBack = function () { };
    ednu.send(setTabsTreatment, selId);
}

function setTabsTreatment(oDoc, selId) {
    var tdSourceList = document.getElementById("TdSourceList");
    var tdTargetList = document.getElementById("TdTargetList");


    var aContent = oDoc.split("$$!!$$");

    tdSourceList.innerHTML = aContent[0];
    tdTargetList.innerHTML = aContent[1];
    initDragOpt();  //On réinitialise le drag&drop afin qu'il se câble sur les nouveaux tableaux créés (demande #20930 et #32501)

    document.getElementById("DivSelectionListResultList").setAttribute("SelectedSel", selId);

    //Classe CSS

    var element = document.getElementById("tr_sel_" + selId);

    //if (element.className == "SelectedItem")
    //    return;
    var oList = element.parentNode;
    var aList = oList.getElementsByTagName("tr")
    var cssLine = "cell";
    for (i = 0; i < aList.length; i++) {
        if (cssLine == "cell")
            cssLine = "cell2";
        else
            cssLine = "cell";

        aList[i].className = cssLine;

        if (element == aList[i]) {
            //aList[i].setAttribute("oldCss", aList[i].className);
            aList[i].className = "SelectedItem";
            oList.setAttribute("SelectedIndex", i)
        }

    }

    //On doit réappliquer l'ouverture des options avancé
    var oAdv = document.getElementById("AdvancedTitle");
    if (oAdv && oAdv.getAttribute("closed") == "0") {
        oAdv.setAttribute("closed", 1);
        oAdv.click();
    }


}

function AddView() {
    var strTitre = top._res_5054;
    var strType = '2';
    var nWidth = 550;
    var nHeight = 200;
    var PromptLabel = top._res_6140 + ' : ';
    var PromptValue = top._res_5054;

    oModalAdd = new eModalDialog(strTitre, strType, null, nWidth, nHeight);
    oModalAdd.setPrompt(PromptLabel, PromptValue);
    oModalAdd.show();
    oModalAdd.addButton(top._res_29, onAddCancel, "button-gray"); // Annuler
    oModalAdd.addButton(top._res_28, onAddOk, "button-green"); // Valider
}


function onAddCancel(val) {
    oModalAdd.hide();
}

function onAddOk(val) {
    url = "eTabsSelectDiv.aspx";
    var selectionname = oModalAdd.getPromptValue();

    var ednu = new eUpdater(url, 0);
    ednu.addParam("action", "createempty", "post");
    ednu.addParam("selectionname", selectionname, "post");
    ednu.ErrorCallBack = function () { }; //En cas d'erreur on laisse la fenetre ouverte. L'utilsateur peut noter sa selection/essayer d'autre valeur
    ednu.send(onAddOkTreatment);
}

function onAddOkTreatment(oDoc) {
    var success = getXmlTextNode(oDoc.getElementsByTagName("success")[0]);

    if (success != "1") {
        var errDesc = getXmlTextNode(oDoc.getElementsByTagName("error")[0]);
        eAlert(3, "", errDesc, '', null, null, null);
        //eAlert(errDesc);
    }
    else {
        var newId = getXmlTextNode(oDoc.getElementsByTagName("newid")[0]);
        var newName = getXmlTextNode(oDoc.getElementsByTagName("newname")[0]);
        var newSelectionList = getXmlTextNode(oDoc.getElementsByTagName("newselectionlist")[0]);
        document.getElementById("DivSelectionListResult").innerHTML = newSelectionList;
        doOnSelectionclick(newId);
    }
    oModalAdd.hide();
}

var ePopupVNEditor;
var ViewNameEditor;
function RenView(selId, openerElem) {
    ePopupVNEditor = new ePopup('ePopupVNEditor', 220, 250, 0, 0, document.body, false);
    ViewNameEditor = new eFieldEditor('inlineEditor', ePopupVNEditor, 'ViewNameEditor');
    ViewNameEditor.action = 'renameView';

    //openerElem.parentNode.parentNode.parentNode.parentNode correspond à la cellule de droite qui contient les boutons
    //document.getElementById('td_sel_' + selId correspond à la cellule de gauche qui contient le libellé
    ViewNameEditor.onClick(document.getElementById('td_sel_' + selId), openerElem);
}

function onRenOkTreatment(oDoc) {
    var success = getXmlTextNode(oDoc.getElementsByTagName("success")[0]);


    if (success != "1") {
        var errDesc = getXmlTextNode(oDoc.getElementsByTagName("error")[0]);
        alert(errDesc);
    }
    else {
        var newId = getXmlTextNode(oDoc.getElementsByTagName("selid")[0]);
        var newName = getXmlTextNode(oDoc.getElementsByTagName("newselname")[0]);

        document.getElementById("td_sel_" + newId).innerHTML = newName;
        doOnSelectionclick(newId);
    }


}

//KHA - cette fonction est appelée pour utilisé un econfirm (message de confirmation customisé
// puis d'appeler la fonction DelView si la boite est validée
// la fonction n'est pas utilisée directement sur onclick pour ne pas alourdir la page
function cfmDelView(selId, label) {
    eConfirm(1, top._res_806, top._res_5050, '', 500, 200, function () { DelView(selId, label); }, function () { });
}

function DelView(selId, label) {
    var url = "eTabsSelectDiv.aspx";

    var ednu = new eUpdater(url, 0);
    ednu.addParam("action", "delete", "post");
    ednu.addParam("selid", selId, "post");
    ednu.ErrorCallBack = function () { }; // L'utilisateur reste sur la fene^tre
    ednu.send(onDelOkTreatment);
}

function onDelOkTreatment(oDoc) {
    var success = getXmlTextNode(oDoc.getElementsByTagName("success")[0]);


    if (success != "1") {
        var errDesc = getXmlTextNode(oDoc.getElementsByTagName("error")[0]);
        alert(errDesc);
    }
    else {
        var selId = getXmlTextNode(oDoc.getElementsByTagName("selid")[0]);
        document.getElementById("tr_sel_" + selId).style.display = "none";

        var newSelId = getXmlTextNode(oDoc.getElementsByTagName("newselid")[0]);

        document.getElementById("DivSelectionListResultList").setAttribute("SelectedSel", newSelId);

        var nbrcount = getXmlTextNode(oDoc.getElementsByTagName("nbr")[0]);


        doOnSelectionclick(newSelId);
        doOnDeleteSelection(newSelId, nbrcount);
    }
}

function doOnDeleteSelection(SelId, nbrcount) {
    if (nbrcount <= 1) {
        var oDelView = document.getElementById("DelView_" + SelId);
        if (!oDelView) return;
        oDelView.style.display = "none";

    }

}

function initDragOpt() {
    // choix des onglets
    dragOpt.SrcList = document.getElementById("AllTabList");
    dragOpt.TrgtList = document.getElementById("TabSelectedList");
    dragOpt.FldSel = false;

    // choix des rubriques
    if (!dragOpt.SrcList || !dragOpt.TrgtList) {
        try {
            dragOpt.SrcList = document.getElementById("DivSourceList").children[0];
            dragOpt.TrgtList = document.getElementById("DivTargetList").children[0];
            dragOpt.FldSel = true;
        }
        catch (e) {
            dragOpt.trace("Une erreur est survenue : aucune liste prenant en charge le drag and drop n'a été détectée. Le drag & drop ne sera pas disponible. " + e);
        }
    }

    dragOpt.init();
}

function strtDrag(e) {
    dragOpt.dragStart(e);
}

// en fonction de la liste dans laquelle la rubrique est placée, 
// son libellé peut être sous la forme <Table>.<rubrique> ou <rubrique>
function adjustLabel(obj, oToId) {

    var sShLab = getAttributeValue(obj, "shlb");
    var sLgLab = getAttributeValue(obj, "lglb");

    if (sLgLab != "" && sShLab != "") {
        if (oToId == "ItemsUsed") {
            obj.innerHTML = encodeHTMLEntities(sLgLab);
        }
        else {
            obj.innerHTML = encodeHTMLEntities(sShLab);
        }
    }
}