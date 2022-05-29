//Fonctions communes entre les filtres avancée et les filtres dans les eudopart
//Notemment les ouvertures de catalogues et leur validations

//Catalogues
var advancedDialog;
function showCat(tabIndex, lineIndex, prefix) {
    if (typeof prefix == 'undefined' || prefix == null && typeof prefix != 'string')
        prefix = '';

    //Champ value
    var oTarget = document.getElementById(prefix + "value_" + tabIndex + "_" + lineIndex);
    var bMulti = oTarget.getAttribute("multi");
    var catDescId = oTarget.getAttribute("edndescid");
    var catPopupType = oTarget.getAttribute('popup');

    var catBoundDescId = oTarget.getAttribute('ednbounddescid');
    var catBoundPopup = oTarget.getAttribute('ednboundpopup');
    var catBoundValue = getAttributeValue(oTarget, "ednboundvalue");

    if (catBoundValue == "")
        catBoundValue = null;

    var bFromTreat = getAttributeValue(oTarget, "ednfromtreat") == "1";
    if (bFromTreat && catBoundDescId != "" && oDuppiWizard) {
        //recherche la boundvalue
        var s = oDuppiWizard.WizardDocument.querySelector("div[edndescid='" + catBoundDescId + "'][ednidx]");
        if (s) {
            var idx = oDuppiWizard.WizardDocument.getElementById("value_0_" + getAttributeValue(s, "ednidx"));
            if (idx) {
                catBoundValue = getAttributeValue(idx, "ednvalue");
            }
        }
    }

    var catSpec = oTarget.getAttribute("special");
    if (catSpec == "1") {
        showSpecialCat(tabIndex, lineIndex);
        return;
    }

    //Dans duplication en masse
    var popupDescid = oTarget.getAttribute("pud");
    if (popupDescid && popupDescid > 0 && popupDescid != catDescId) {
        catDescId = popupDescid;
    }


    //popupdescid
    var fldSelect = document.getElementById("field_" + tabIndex + "_" + lineIndex);
    if (fldSelect) {
        //fldSelect null en filtre formulaire
        var fldOption = fldSelect.options[fldSelect.selectedIndex];
        var pud = getNumber(getAttributeValue(fldOption, "pud"))
        if (pud > 0)
            catDescId = pud;
    }
    var bAdvanced = oTarget.getAttribute("advanced");
    var defValue = oTarget.getAttribute("ednvalue");
    var treeView = oTarget.getAttribute("treeview");

    /*    p_bMulti, p_btreeView, p_defValue, p_sourceFldId
    , p_targetFldId, p_catDescId, p_catPopupType, p_catBoundDescId
    , p_catBoundPopup, p_catParentValue, p_CatTitle,p_JsVarName
    , p_bMailTemplate, p_partOfAfterValidate
    */
    showCatGeneric((bMulti == "1"), (treeView == "1"), defValue, null
        , oTarget.id, catDescId, catPopupType, catBoundDescId
        , catBoundPopup, catBoundValue, "catalogue", "advancedDialog"
        , false, partOfValidateCatDlg_Flt, null, LOADCATFROM.FILTER
    );  //TODORES
}

//Catalogues : Partie de code éxécuté au clique sur valider juste avant la fermeture du catalogue avancé
function partOfValidateCatDlg_Flt(catalogDialog, srcId, trgId, tabSelectedLabels, tabSelectedValues, selectedIDs) {

    document.getElementById(trgId).value = joinString(";", tabSelectedLabels);
    document.getElementById(trgId).setAttribute("ednvalue", joinString(";", tabSelectedValues));

    var src = document.getElementById(trgId);
    if (src && getAttributeValue(src, "ednfromtreat") == "1") {

        var nDescId = getAttributeValue(src, 'edndescid');

        //recherche des cataloguees ayant ce parent & raz de leur valeur
        var lstChild = oDuppiWizard.WizardDocument.querySelectorAll("input[ednbounddescid='" + nDescId + "']");
        for (var jj = 0; jj < lstChild.length; jj++) {

            var oo = lstChild[jj];
            oo.value = "";
            setAttributeValue(oo, "ednvalue", "");
        }
    }


}
function validateCalFilterDialog(trgId, e, trgId2) {

    var ifrm = advancedDialog.getIframe();
    var _return = ifrm.getReturnValue();
    var valueReturn = _return;


    if (_return != null && typeof (_return) != "undefined") {
        var strMoveTo = '';
        var strMoveTo2 = '';
        var bNoYear = 0;

        if (_return.indexOf('[NOYEAR]', 1) > 0) {
            bNoYear = 1;
            _return = _return.replace('[NOYEAR]', '');
            strMoveTo2 = " (" + top._res_1495 + ") ";
        }

        if (_return.indexOf('+', 1) > 0) {
            var aValue = _return.split('+');
            strDate = aValue[0].replace(' ', '');
            strMoveTo = ' + ' + aValue[1].replace(' ', '');
        }
        else if (_return.indexOf('-', 1) > 0) {
            var aValue = _return.split('-');
            _return = aValue[0].replace(' ', '');
            strMoveTo = ' - ' + aValue[1].replace(' ', '');
        }

        if (_return.indexOf('<DATE>') >= 0)
            var strDateValue = "<" + top._res_367 + ">";
        else if (_return.indexOf('<DATETIME>') >= 0)
            var strDateValue = "<" + top._res_368 + ">";
        else if (_return.indexOf('<MONTH>') >= 0)
            var strDateValue = "<" + top._res_693 + ">";
        else if (_return.indexOf('<WEEK>') >= 0)
            var strDateValue = "<" + top._res_694 + ">";
        else if (_return.indexOf('<YEAR>') >= 0)
            var strDateValue = "<" + top._res_778 + ">";
        else if (_return.indexOf('<DAY>') >= 0)
            var strDateValue = "<" + top._res_1234 + ">";
        else {
            if (bNoYear == 1) { _return = _return.substr(0, 10); }
            //GCH - #36019 - Internationnalisation - Choix de dates
            var strDateValue = eDate.ConvertBddToDisplay(_return);
            valueReturn = eDate.ConvertBddToDisplay(valueReturn);
        }


        document.getElementById(trgId).setAttribute("ednvalue", valueReturn);
        document.getElementById(trgId).value = strDateValue + strMoveTo + strMoveTo2;

        advancedDialog.hide();
    }
    else {
        // En cas d'erreur de saisie sur la fenêtre enfant (retour null ou undefined), on ne la masque pas, afin que l'erreur puisse être corrigée.
        // L'utilisateur fermera éventuellement avec la croix s'il ne souhaite pas effectuer la correction
    }
}

//Calendar - Champs date
function showCal(tabIndex, lineIndex, bFromTreat, prefix) {
    if (typeof prefix == 'undefined' || prefix == null && typeof prefix != 'string')
        prefix = '';
    //Champ value
    var oTarget = document.getElementById(prefix + "value_" + tabIndex + "_" + lineIndex);
    var defValue = eDate.ConvertDisplayToBdd(oTarget.getAttribute("ednvalue"));
    //advancedDialog.addArg(oTarget.id);
    var nWidth = 475;
    var nHeight = 415;

    if (top.eTools.GetFontSize() >= 14) {
        nHeight = 500
        nWidth = 680;
    } else {
        nHeight = 480;
        nWidth = 490;
    }

    if (bFromTreat)
        nWidth = 275;

    advancedDialog = new eModalDialog(top._res_5017, 0, "eFilterWizard.aspx", nWidth, nHeight);

    //CallBack d'erreur
    advancedDialog.ErrorCallBack = launchInContext(advancedDialog, advancedDialog.hide);
    advancedDialog.addParam("action", "calselect", "post");
    advancedDialog.addParam("type", "1", "post");
    advancedDialog.addParam("date", defValue, "post");
    advancedDialog.addParam("modalvarname", "advancedDialog", "post");
    advancedDialog.addParam("fromtreat", bFromTreat ? "1" : "0", "post");

    //ELAIZ - Ajout du type 2 non utilisé pour distinguer les modales date des autres modales
    advancedDialog.show(undefined, undefined, undefined, 2);
    advancedDialog.addButton(top._res_29, null, "button-gray", oTarget.id, "cancel"); // Annuler
    advancedDialog.addButton(top._res_28, validateCalFilterDialog, "button-green", oTarget.id, "ok"); // Valider


    advancedDialog.onIframeLoadComplete = function () { advancedDialog.adjustModalToContent(); };

}

//CATALOGUE de champs de liaison
function showSpecialCat(tabIndex, lineIndex, prefix) {
    if (typeof prefix == 'undefined' || prefix == null && typeof prefix != 'string')
        prefix = '';
    //Champ value
    var oTarget = document.getElementById(prefix + "value_" + tabIndex + "_" + lineIndex);
    var operatorList = document.getElementById(prefix + "op_" + tabIndex + "_" + lineIndex);
    var bMulti = oTarget.getAttribute("multi") == "1";
    var catDescId = oTarget.getAttribute("edndescid");
    var selectedValue = oTarget.getAttribute("ednvalue");
    var targetTab = getNumber(oTarget.getAttribute("popupdescid"));
    var sDisplayValue = oTarget.value;
    if (targetTab > 0)
        targetTab = getTabDescid(targetTab);

    var operator = (operatorList == null ? getAttributeValue(oTarget, "ednop") : operatorList.options[operatorList.selectedIndex].value);
    var bFromTreat = getAttributeValue(oTarget, "ednfromtreat") == "1";


    if (!top.eLinkCatFileEditorObject)  //N'est pas initialisé si l'on vient de la HomePage
    {
        if (typeof (ePopupObject) == "undefined" || ePopupObject == null) {
            ePopupObject = new ePopup('ePopupObject', 220, 250, 0, 0, document.body, false);
            ePopupObject.hide();
        }

        top.eLinkCatFileEditorObject = new eFieldEditor('linkCatFileEditor', ePopupObject, 'eLinkCatFileEditorObject', 'eLinkCatFileEditor');

    }
    top.eLinkCatFileEditorObject.AddNameOnly = true;    //Nom du PP seul en attribbut
    top.eLinkCatFileEditorObject.sourceElement = oTarget;
    top.eLinkCatFileEditorObject.multiple = bMulti;


    var SEPARATOR_LVL1 = "#|#";
    var SEPARATOR_LVL2 = "#$#";
    var paramSup =
        "AllRecord" + SEPARATOR_LVL2 + (bFromTreat ? "0" : "1") + SEPARATOR_LVL1 +
        "EnabledSearchAll" + SEPARATOR_LVL2 + "0" + SEPARATOR_LVL1 +
        "sids" + SEPARATOR_LVL2 + selectedValue;
    if (bMulti)
        paramSup += SEPARATOR_LVL1 + "Multi" + SEPARATOR_LVL2 + "1";
    paramSup += SEPARATOR_LVL1 + "op" + SEPARATOR_LVL2 + operator;

    top.eLinkCatFileEditorObject.openLnkFileDialog((typeof (FinderSearchType) != "undefined" ? FinderSearchType : top.FinderSearchType).AdvFilter, targetTab, false, onOkSpecialCat, paramSup, sDisplayValue);

}
function onOkSpecialCat(iframeId) {
    var catalogObject = top.eTabLinkCatFileEditorObject[iframeId];

    var operateur = catalogObject.oModalLnkFile.getParam("op");

    var oFrm = top.document.getElementById(iframeId);
    var oFrmDoc = oFrm.contentDocument;
    var oFrmWin = oFrm.contentWindow;
    var selectedListValues = oFrmWin._selectedListValues;
    if (typeof (selectedListValues) == 'undefined')
        selectedListValues = new Array();

    var descid = getNumber(getAttributeValue(catalogObject.sourceElement, "edndescid"));
    var bMulti = getAttributeValue(oFrmDoc.getElementById("mainDiv"), "multi") == "1";
    if (bMulti)
        selectedListValues = oFrmWin.getSelectedValues();

    // MCR 40756 filtre avancee, pour une rubrique de type catalogue 'utiliser les valeurs de la rubrique' : 
    //           remplacer la chaine de caractere par l'id de la fiche
    var pop = 0
    if (catalogObject.sourceElement)
        pop = getAttributeValue(catalogObject.sourceElement, "popup");

    //REMISE A ZERO
    catalogObject.selectedValues = new Array();
    catalogObject.selectedLabels = new Array();
    if (!bMulti && selectedListValues.length > 0)
        for (var i = 0; i < selectedListValues.length; i++) {
            var oItem = oFrmDoc.getElementById(selectedListValues[i]);

            if (!oItem)
                continue;

            // id  
            var oId = oItem.getAttribute("eid").split('_');
            var nTab = oId[0];
            var nId = oId[oId.length - 1];

            var tabTd = oItem.getElementsByTagName("td");
            var label = "";
            //NameOnly
            var eNO = getAttributeValue(tabTd[0], "eNO"); //Nom du pp seul
            if (eNO && eNO != "" && descid == 201)
                label = eNO;
            else
                //BSE #43 802 
                //ALISTER #81 166 Problème d'encodage de &
                label = GetTextContent(tabTd[0]);  //Libellé de la première colonne
            catalogObject.selectValue(nId, label, true);
        }
    else if (bMulti && selectedListValues.length > 0) {
        for (var i = 0; i < selectedListValues.length; i++) {
            catalogObject.selectValue(selectedListValues[i].id, selectedListValues[i].label, true);
        }

    }
    else {
        catalogObject.selectValue("", "", true);
    }

    var newValue = "";
    var newLabel = "";
    for (var i = 0; i < catalogObject.selectedValues.length; i++) {
        if (i > 0) {
            newValue += ';';
            newLabel += ';';
        }
        newValue += catalogObject.selectedValues[i];
        newLabel += catalogObject.selectedLabels[i];
    }


    // MCR 40756 filtre avancee, pour une rubrique de type catalogue 'utiliser les valeurs de la rubrique' : 
    //           remplacer la chaine de caractere par l'id de la fiche, pour un catalogue (pop="2")
    // HLA prendre en compte ce param uniquement dans le cas d'operateur 'est égal', 'différent', 'est dans la liste', 'n'est pas dans la liste' 
    catalogObject.sourceElement.value = newLabel;

    if ((pop == "2" || pop == "4") && (operateur == 0 || operateur == 5 || operateur == 8 || operateur == 15))
        catalogObject.sourceElement.setAttribute("ednvalue", newValue);
    else
        catalogObject.sourceElement.setAttribute("ednvalue", newLabel);

    catalogObject.oModalLnkFile.hide();
}
//Catalogue utilisateur
var modalUserCat;
function showUserCat(tabIndex, lineIndex, bFrp, prefix) {

    if (typeof prefix == 'undefined' || prefix == null && typeof prefix != 'string')
        prefix = '';

    //Champ value
    var oTarget = document.getElementById(prefix + "value_" + tabIndex + "_" + lineIndex);
    var sMulti = oTarget.getAttribute("multi");
    var defValue = oTarget.getAttribute("ednvalue");
    var sFormat = oTarget.getAttribute("ednformat");

    var oOP = document.getElementById(prefix + "op_" + tabIndex + "_" + lineIndex);
    var operator = 0;
    if (oOP.tagName == "SELECT")
        operator = oOP.options[oOP.selectedIndex].value;
    else if (oOP.tagName == "INPUT")
        operator = oOP.value;


    modalUserCat = new eModalDialog(top._res_246, 0, "eCatalogDialogUser.aspx", 550, 610);
    top.eTabCatUserModalObject.Add(modalUserCat.iframeId, modalUserCat);
    modalUserCat.addParam("iframeId", modalUserCat.iframeId, "post");
    modalUserCat.ErrorCallBack = function () { setWait(false); }
    modalUserCat.addParam("multi", sMulti, "post");
    modalUserCat.addParam("selected", defValue, "post");
    modalUserCat.addParam("modalvarname", "modalUserCat", "post");
    modalUserCat.addParam("fromtreat", bFrp ? "1" : "0", "post");

    if (sMulti != "1") {
        modalUserCat.addParam("showcurrentgroupfilter", "1", "post"); //Si à 1 => Proposition dans le catalogue : <le groupe de l'utilisateur en cours> pour filtre avancé
        modalUserCat.addParam("showcurrentuserfilter", "1", "post");   //Si à 1 => Proposition dans le catalogue : <utilisateur en cours> pour filtre avancé
        modalUserCat.addParam("usegroup", "1", "post"); //si à 1 => Autorise la sélection de groupe pour le catatalogue simple
    }
    else if (sFormat == "14" && operator == "8") {
        modalUserCat.addParam("showcurrentgroupfilter", "1", "post"); //Si à 1 => Proposition dans le catalogue : <le groupe de l'utilisateur en cours> pour filtre avancé
    }

    modalUserCat.addParam("showvalueempty", "1", "post"); //si à 1 => Affiche <Vide> sur le catalogue simple
    modalUserCat.addParam("showvaluepublicrecord", "0", "post"); //si à 1 => Affiche <Fiche Publique> sur le catalogue simple

    modalUserCat.show();
    modalUserCat.addButton(top._res_29, onUserFilterCatCancel, "button-gray");
    modalUserCat.addButton(top._res_28, onUserFilterCatOk, "button-green", oTarget.id, "ok");
}
//A la validation du Catalogue utilisateur
//trgId : id de l'input dontenant le libellé et sa ednvalue
function onUserFilterCatOk(trgId) {
    var strReturned = modalUserCat.getIframe().GetReturnValue();
    modalUserCat.hide();
    var vals = strReturned.split('$|$')[0];
    var libs = strReturned.split('$|$')[1];
    var oTarget = document.getElementById(trgId);
    oTarget.value = libs;
    oTarget.setAttribute("ednvalue", vals);
}
//A l'annulation du Catalogue utilisateur
function onUserFilterCatCancel() {
    modalUserCat.hide();
}




function onChangeLineOp(lst, prefix) {

    if (typeof prefix == 'undefined' || prefix == null && typeof prefix != 'string')
        prefix = '';

    var aIdx = lst.id.split("_");
    var tabIndex = aIdx[1];
    var lineIndex = aIdx[2];
    var descIdList = document.getElementById(prefix + "field_" + tabIndex + "_" + lineIndex);
    var operatorList = document.getElementById(prefix + "op_" + tabIndex + "_" + lineIndex);
    var oHdr = document.getElementById("header_" + tabIndex + "_" + lineIndex); //Cas des eudoparts essentiellement
    var descId = null;
    if (descIdList)
        descId = descIdList.options[descIdList.selectedIndex].value;
    else
        descId = getAttributeValue(oHdr, "edndescid");

    var operator = operatorList.options[operatorList.selectedIndex].value;

    var upd = new eUpdater("mgr/eFilterWizardManager.ashx", 1);
    //Gestion d'erreur : a priori, pas de traitement particulier
    upd.ErrorCallBack = function () { };

    upd.addParam("action", "reloadvalues", "post");
    if (typeof (nfilterType) != "undefined")
        upd.addParam("filtertype", nfilterType, "post");
    else
        upd.addParam("filtertype", getAttributeValue(oHdr, "ednfiltertype"), "post");
    upd.addParam("tabindex", tabIndex, "post");
    upd.addParam("lineindex", lineIndex, "post");
    if (typeof (nTab) != "undefined")
        upd.addParam("maintab", nTab, "post"); //Table de provenance
    else
        upd.addParam("maintab", getAttributeValue(oHdr, "edntab"), "post");
    upd.addParam("descid", descId, "post");
    upd.addParam("operator", operator, "post");

    //Ajout du prefix pour le filtre express Combiné
    if (typeof prefix != 'undefined' && prefix != null && prefix != '')
        upd.addParam("prefixFilter", prefix, "post");

    upd.send(onChangeLineOpTreatment, lst.id, prefix);


}

function onChangeLineOpTreatment(oRes, lstId, prefix) {
    if (typeof prefix == 'undefined' || prefix == null && typeof prefix != 'string')
        prefix = '';

    var aIdx = lstId.split("_");
    var tabIndex = aIdx[1];
    var lineIndex = aIdx[2];
    var targetValueDiv = document.getElementById(prefix + "DivValues_" + tabIndex + "_" + lineIndex);


    var oNewDiv = document.createElement("div");
    oNewDiv.innerHTML = oRes;

    targetValueDiv.style.opacity = 0;
    targetValueDiv.style.filter = 'alpha(opacity = 0)';
    targetValueDiv.innerHTML = oNewDiv.firstChild.innerHTML;
    fadeThis(targetValueDiv.id, document);
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
            document.getElementById(prefix + "value_" + tabIndex + "_" + lineIndex).focus();
        }
    }
    catch (exp1) { }


}

function doOnchangeText(txt) {
    var sOldValue = getAttributeValue(txt, "ednvalue");

    if (txt.getAttribute("ednfreetext") == "1") {
        txt.setAttribute("ednvalue", txt.value);
    }



    var nFormat = getNumber(getAttributeValue(txt, "ednformat"));

    switch (nFormat) {
        case 4:
        case 5:
        case 10:


            var nBddVal = eNumber.ConvertNumToBdd(txt.value);

            if (eNumber.IsValid()) {


                txt.value = eNumber.ConvertBddToDisplayFull(nBddVal);
                setAttributeValue(txt, "ednvalue", nBddVal);
            }
            else {

                // msg d'erreur
                eAlert(0, top._res_295, top._res_673);


                //remplace par l'ancienne valeur si elle est valide, par vide sinon
                var sDisplayOldValue = eNumber.ConvertBddToDisplayFull(sOldValue);
                var bOldValid = eNumber.IsValid();


                if (bOldValid) {
                    txt.value = sDisplayOldValue;
                    setAttributeValue(txt, "ednvalue", sOldValue);
                }
                else {
                    txt.value = "";
                    setAttributeValue(txt, "ednvalue", "");
                }
            }
            break;

        default:

    }




}

//Fonction d execution du filtre

/////////////////
/////POUR 1 TABLE
//0 : operateur entre table (pas pour première table)
function getTabOperator(tabOperator, idxTab) {
    return "link_" + idxTab + "=" + tabOperator;
}
//1 : Déclaration d'une table
function getTabFile(file, idxTab) {
    return "file_" + idxTab + "=" + file;
}


//2 : Options de regoupement d'une table (A placer après la récupération des infos de lignes et si option de regroupement souhaité)
function getTabGroupByOption(importance, top, groupby, idxTab) {
    return "&importance_" + idxTab + "=" + importance + "&lsttop_" + idxTab + "=" + top + "&lstgroupby_" + idxTab + "=" + groupby;
}

//////POUR 1 LIGNE
//0 : Opérateur de logique entre chaque ligne (attention pas pour la première)
function getLogicLineParam(logicOp, idxTab, idxLine) {
    return "and_" + idxTab + "_" + idxLine + "=" + logicOp + "&";

}
//1 : contenu pour une ligne
function getLineValueParam(descid, op, value, idxTab, idxLine) {

    value = trim(value);
    var retValue = "field_" + idxTab + "_" + idxLine + "=" + descid + "&op_" + idxTab + "_" + idxLine + "=" + op + "&value_" + idxTab + "_" + idxLine + "=" + encode(value);

    if (value == "" && op != 96 && op != 10 && op != 12 && op != 17 && op != 11 && op != 96) {
        retValue += "&question_" + idxTab + "_" + idxLine + "=1";
    }
    else {
        retValue += "&question_" + idxTab + "_" + idxLine + "=0";
    }

    return retValue;
}

///////////////
/////GLOBAL
//Options de filtrage, tri, échantillonage... du filtre
//paramètre en 1 ou 0 pour vrai ou faux
function getGlobalTabsOptions(fileonly, negation, raz, random) {
    return "&fileonly=" + fileonly + "&negation=" + negation + "&raz=" + raz + "&random=" + random;
}
///////////////

