function LastValuesManager() {
}



LastValuesManager.nbLimitValues = 5;
LastValuesManager.menuWidth = 400;
LastValuesManager.debugMode = false;

/* Méthodes statiques */

// Ajout d'une nouvelle valeur dans la liste des dernières valeurs saisies
LastValuesManager.addValue = function (srcElementId, tab, descid, fieldLabel, value, valueLabel, isPlanningDate) {

    

    // arrLastValues est déclarée dans eFile.js
    if (typeof arrLastValues !== 'undefined' && arrLastValues instanceof Array) {

        if (LastValuesManager.debugMode) {
            console.log("LastValuesManager.addValue");
        }

        if (typeof isPlanningDate === "undefined")
            isPlanningDate = false;

        value = value || "";
        valueLabel = valueLabel || "";

        if (!tab)
            tab = getTabDescid(descid);

        // Suppression de la ligne stockée si déjà existante pour le descid
        for (var i = 0; i < arrLastValues.length; i++) {

            if (arrLastValues[i].descid == descid && arrLastValues[i].tab == tab) {
                arrLastValues.splice(i, 1);
            }
        }

        // Ajout du nouvel objet en première position
        arrLastValues.splice(0, 0,
            {
                'srcElementId': srcElementId,
                'label': fieldLabel,
                'descid': descid,
                'tab': tab,
                'dbv': value,
                'value': valueLabel,
                'isPlanningDate': isPlanningDate
            }
        );
    }
}


// Affichage du menu contextuel contenant les dernières valeurs saisies
// btn : Elément survolé
// tab : Descid de la table
LastValuesManager.openContextMenu = function (btn, tab, arrayLastValues, oLastValuesContextMenu, bPopup) {

    if (typeof oLastValuesContextMenu === 'undefined')
        oLastValuesContextMenu = new eContextMenu(LastValuesManager.menuWidth, -999, -999, null, null, "lastvalues_contextmenu");
    
    if (typeof arrayLastValues !== "undefined" && arrayLastValues instanceof Array) {

        if (LastValuesManager.debugMode) {
            console.log("LastValuesManager.openContextMenu");
        }

        var itemLabel, itemElement, item, srcElement;
        var countAddedItems = 0;

        if (arrayLastValues.length > 0) {
            for (var i = 0; i < arrayLastValues.length; i++) {

                item = arrayLastValues[i];

                // On n'affiche pas plus de X items
                if (countAddedItems >= LastValuesManager.nbLimitValues)
                    break;

                // Si le champ est réversible, on rajoute la valeur dans l'historique
                if (LastValuesManager.isValueReversible(item, tab)) {

                    itemLabel = top._res_8225.replace("{0}", item.label).replace("{1}", item.value);
                    itemElement = oLastValuesContextMenu.addItemFct(itemLabel, function () { }, 1, 0, "actionItem", itemLabel, "icon-repeat");
                    // On déclare le "onclick" séparément pour que ce soit plus simple
                    itemElement.onclick = (function (obj, tabDescid) {
                        return function () {
                            LastValuesManager.restoreLastValue(obj, tabDescid);
                        }
                    })(item, tab);

                    countAddedItems++;
                }
            }

            if (countAddedItems > 0) {
                oLastValuesContextMenu.addSeparator(1);
                oLastValuesContextMenu.addItemFct(top._res_8224, function () { LastValuesManager.restoreAllValues(tab); }, 2, 0, "actionItem", top._res_8224, "icon-repeat");
            }
        }

        // Aucune valeur
        if (countAddedItems == 0) {
            oLastValuesContextMenu.addItemFct(top._res_8166, function () { }, 1, 0, "");
        }
        
    }

    oLastValuesContextMenu.alignElement(btn, "UNDER", null, null, bPopup);


    // Evénement mouseout pour cacher le menu contextuel
    if (!mainDebugMode) {
        var oMenuMouseOver = function () {

            var fctOut = setTimeout(
                function () {
                    //Masque le oLastValuesMenu
                    if (oLastValuesContextMenu) {
                        oLastValuesContextMenu.hide();
                        unsetEventListener(oLastValuesContextMenu.mainDiv, "mouseout", oMenuMouseOver);
                    }
                }
            , 200);

            //Annule la disparition
            if (oLastValuesContextMenu) {
                setEventListener(oLastValuesContextMenu.mainDiv, "mouseover", function () { clearTimeout(fctOut) });
            }
            if (btn) {
                setEventListener(btn, "mouseover", function () { clearTimeout(fctOut); });
            }
        };

        //si on sort de la div de bouton ou de oLastValuesContextMenu, on a 200ms pour se rattraper
        setEventListener(btn, "mouseout", oMenuMouseOver);
        if (oLastValuesContextMenu) {
            setEventListener(oLastValuesContextMenu.mainDiv, "mouseout", oMenuMouseOver);
        }
    }
}


// Vérifie que la valeur est réversible
// Si la rubrique n'est pas affichée ou si elle est en lecture seule, elle n'est pas réversible
LastValuesManager.isValueReversible = function (objLastValue, tab) {

    // Si on n'est pas dans le même contexte de table, ce n'est pas possible
    if (tab != objLastValue.tab)
        return false;

    var srcElement = LastValuesManager.getSourceElement(objLastValue);

    if (srcElement) {
        if (getAttributeValue(srcElement, "ero") != "1")
            return true;
    }

    return false;
}


// Retourne l'élément source de la valeur en historique
LastValuesManager.getSourceElement = function (objLastValue) {
    var srcElement = document.getElementById(objLastValue.srcElementId);
    if (!srcElement) {
        if (objLastValue.descid % 100 == 0)
            objLastValue.descid++;
        srcElement = document.querySelector("[ename='COL_" + objLastValue.tab + "_" + objLastValue.descid + "']");
    }
    return srcElement;
}


// Au clic, rétablissement de la valeur précédemment saisie
LastValuesManager.restoreLastValue = function (objLastValue, tab, bFromRestoreAll, fCallback) {

    // Si la valeur n'est pas réversible, on ne fait rien
    if (!LastValuesManager.isValueReversible(objLastValue, tab))
        return;

    if (typeof (bFromRestoreAll) == 'undefined')
        bFromRestoreAll = false;

    var bFromRestore = true;

    // Suppression de la valeur cliquée dans le tableau
    if (!bFromRestoreAll) {
        for (var i = 0; i < arrLastValues.length; i++) {
            if (arrLastValues[i].descid == objLastValue.descid && arrLastValues[i].dbv == objLastValue.dbv) {
                arrLastValues.splice(i, 1);
            }
        }
    }
   

    // Enregistrement de la valeur suivant le type de rubrique
    var srcElt = LastValuesManager.getSourceElement(objLastValue);

    if (srcElt) {

        var eaction = getAttributeValue(srcElt, "eaction");

        // Pour LNKGOFILE, on récupère l'action du bouton associé
        if (eaction == "LNKGOFILE") {
            var btn = srcElt.parentElement.querySelector("[eacttg='" + srcElt.id + "']");
            if (btn) {
                eaction = getAttributeValue(btn, "eaction");
            }
        }
        // Ceci permet de ne pas ouvrir le catalogue en popup
        if (eaction == "LNKADVCAT" || eaction == "LNKCAT" || eaction == "LNKFREECAT" || eaction == "LNKCATFILE" || eaction == "LNKGOFILE" || eaction == "LNKCATUSER" || eaction == "LNKCATDESC" || eaction == "LNKCATENUM") {
            setFldEditor(srcElt, srcElt, ((eaction == "LNKGOFILE") ? "LNKCATFILE" : eaction), "LASTVALUE_CLICK", null);
        }
        else {
            srcElt.click();
        }
        switch (eaction) {
            case 'LNKPHONE': // Champ téléphone
            case 'LNKWEBSIT':   // Catalogue Site Web        
            case 'LNKSOCNET':   // Catalogue Reseau Social        
            case 'LNKFREETEXT': // Champ saisie libre
            case 'LNKNUM': // Edition champ numérique
            case 'LNKMAIL': // Rubrique e-mail
            case 'LNKGEO': // Champ géo


                if (srcElt.tagName == "INPUT")
                    srcElt.value = objLastValue.value;

                if (eaction != "LNKMAIL") {
                    eInlineEditorObject.validate(false, bFromRestore);
                }
                else {
                    var sCurrentView = getCurrentView(document);
                    if ((sCurrentView == 'FILE_CREATION' || sCurrentView == 'FILE_MODIFICATION') && srcElt.tagName == 'INPUT' && !srcElt.readOnly) {
                        eMailEditorObject.validate(false, bFromRestore);
                    }
                    else {
                        eInlineEditorObject.validate(false, bFromRestore);
                    }
                }

                break;

            case 'LNKCATUSER':
                //eCatalogUserEditorObject.advancedDialog.hide();
                eCatalogUserEditorObject.selectedValues = [];
                eCatalogUserEditorObject.selectedLabels = [];
                eCatalogUserEditorObject.selectValue(objLastValue.dbv, objLastValue.value, true);
                eCatalogUserEditorObject.validate(false, bFromRestore);
                top.setWait(false);
                break;

            case 'LNKCATDESC':
            case 'LNKCATENUM':
            case 'LNKADVCAT':
            case 'LNKCAT':
            case 'LNKFREECAT':

                if (eCatalogEditorObject.multiple) {

                    eCatalogEditorObject.selectedValues = [];
                    eCatalogEditorObject.selectedLabels = [];

                    if (objLastValue.dbv != "") {
                        eCatalogEditorObject.setValue(objLastValue.dbv, objLastValue.value, false, bFromRestore);
                    }

                    eCatalogEditorObject.validate(false, bFromRestore);
                }
                else {
                    eCatalogEditorObject.setValue(objLastValue.dbv, objLastValue.value, false, bFromRestore);
                }

                break;

            case 'LNKGOFILE':
            case 'LNKCATFILE':

                eLinkCatFileEditorObject.selectedValues = [];
                eLinkCatFileEditorObject.selectedLabels = [];

                if (objLastValue.dbv != "")
                    eLinkCatFileEditorObject.selectValue(objLastValue.dbv, objLastValue.value, true);
                    
                eLinkCatFileEditorObject.validate(false, bFromRestore);
                break;

            case 'LNKCHECK':
                var chk = srcElt.querySelector("a.rChk");
                chgChk(chk, (objLastValue.dbv == "1") ? true : false);
                eCheckBoxObject.validate(false, bFromRestore);
                break;

            case 'LNKBITBUTTON':
                var chk = srcElt.querySelector("a.btnBitField");
                changeBtnBit(chk, (objLastValue.dbv == "1") ? true : false);
                eBitButtonObject.validate(false, bFromRestore);
                break;
            
            case 'LNKSTEPCAT':

                var elt = null;
                if (objLastValue.dbv != "") {
                    elt = srcElt.querySelector("li a.stepValue[dbv='" + objLastValue.dbv + "']");
                } 
                else {
                    elt = srcElt.querySelector("li.selectedValue a.stepValue");
                }

                if (elt) {
                    selectStep(elt);
                    // Pour ce type de champ, la fonction "setFldEditor" fait aussi le "validate()"
                    setFldEditor(srcElt, elt, eaction, "LASTVALUE_CLICK", null);
                }
                
                break;

            default:

                /* Champs mémo */
                if (hasClass(srcElt, "inlineMemoEditor") || hasClass(srcElt, "eMemoEditor")) {
                    var oMemoEditor = nsMain.getMemoEditor('edt' + srcElt.id);
                    if (oMemoEditor)
                        //oMemoEditor.validate(objLastValue.dbv);
                        oMemoEditor.setData(objLastValue.dbv, function () {
                            oMemoEditor.validate(objLastValue.dbv)
                        });
                }
                else if (objLastValue.isPlanningDate) {
                    /* Date planning */
                    var arrValue = objLastValue.value.split(" ");
                    if (arrValue.length == 2) {
                        var eltDate = document.getElementById("COL_" + objLastValue.tab + "_" + objLastValue.descid + "_D_0_0_0");
                        var eltHour = document.getElementById("COL_" + objLastValue.tab + "_" + objLastValue.descid + "_H_0_0_0");
                        var eltCompleteDate = document.getElementById("COL_" + objLastValue.tab + "_" + objLastValue.descid + "_0_0_0");

                        setAttributeValue(eltDate, "oldvalue", eltDate.value);
                        setAttributeValue(eltHour, "oldvalue", eltHour.value);
                        setAttributeValue(eltCompleteDate, "oldvalue", eltCompleteDate.value);

                        eltDate.value = arrValue[0];
                        eltHour.value = arrValue[1];
                        eltCompleteDate.value = objLastValue.value;

                        validDateFields(objLastValue.tab, null, true);
                    }
                }
                break;
        }
    }

    // Callback 
    if (typeof (fCallback) == 'function')
        fCallback();

}


// Rétablissement de toutes les anciennes valeurs
LastValuesManager.restoreAllValues = function (tab) {

    if (arrLastValues.length > 0) {
        var item = arrLastValues[0];
        arrLastValues.splice(0, 1);

        LastValuesManager.restoreLastValue(item, tab, true, function () {
            LastValuesManager.restoreAllValues(tab);
        });
    }

}