var nsAdminRelations = nsAdminRelations || {};
var nsAdminRelationsSQLFilters = nsAdminRelationsSQLFilters || {};



nsAdminRelations.load = function (nTabDescid) {
    nsAdminRelations.modal = top.window['_md']["modalAdminRelations"];
    nsAdminRelations.updCaps = new Capsule(nTabDescid);
    nsAdminRelations.updCaps.SetCapsuleConfirmed(false);
    nsAdminRelations.tab = nTabDescid;

    nsAdminRelations.addEventListeners();
}

nsAdminRelationsSQLFilters.load = function (nTabDescid) {
    nsAdminRelationsSQLFilters.modal = top.window['_md']["modalAdminRelationsSQLFilters"];

    nsAdminRelationsSQLFilters.updCaps = new Capsule(nTabDescid);
}

nsAdminRelations.addEventListeners = function () {
    // Evenements au clic sur les titres de tables liées
    var titles = document.getElementsByClassName("adminTableRelationTitle");
    for (var i = 0; i < titles.length; i++) {
        titles[i].addEventListener("click", function () {
            var content = this.parentElement.querySelector(".adminTableRelationContent");
            var icon = this.querySelector(".openerButtonIcon");
            var active = getAttributeValue(content, "data-active");
            if (active == "1") {
                setAttributeValue(content, "data-active", "0");
                eTools.SetClassName(icon, "icon-chevron-down");
                eTools.RemoveClassName(icon, "icon-chevron-up");
            }
            else {
                setAttributeValue(content, "data-active", "1");
                eTools.SetClassName(icon, "icon-chevron-up");
                eTools.RemoveClassName(icon, "icon-chevron-down");
            }
        });
    }

    // Evénements au clic sur les titres d'étapes
    titles = document.getElementsByClassName("paramStep");
    for (var i = 0; i < titles.length; i++) {
        titles[i].addEventListener("click", function () {

            // On ferme l'étape déjà ouverte
            var stepActive = document.querySelector(".stepContent[data-active='1']");
            if (stepActive) {
                setAttributeValue(stepActive, "data-active", "0");
                var title = stepActive.parentElement.querySelector(".paramStep");
                eTools.RemoveClassName(title, "active");
            }

            // On ouvre/ferme l'étape correspondante
            var content = this.parentElement.querySelector(".stepContent");
            var active = getAttributeValue(content, "data-active");
            if (active == "1") {
                setAttributeValue(content, "data-active", "0");
                eTools.RemoveClassName(this, "active");
            }
            else {
                setAttributeValue(content, "data-active", "1");
                eTools.SetClassName(this, "active");
            }

            if (this.id == "stepTitle2") {
                top.nsAdmin.updateRelations(true, false, false, nsAdminRelations.updateStep2Content);
                // Nettoyage des rubriques sélectionnées
                //var chkInterEVT = getAttributeValue(document.getElementById("chkInterEVT"), "chk");
                //var chkInterPP = getAttributeValue(document.getElementById("chkInterPP"), "chk");
                //var chkInterPM = getAttributeValue(document.getElementById("chkInterPM"), "chk");
                //var ddlInterEVTNum = document.getElementById("ddlInterTables");

                //var selectedFields = document.querySelectorAll("#selectedFields ul li");
                //for (var i = 0; i < selectedFields.length; i++) {
                //    if (chkInterPP == "0" && getAttributeValue(selectedFields[i], "tab") == "200") {
                //        objFieldsSelect.transferField(selectedFields[i], false);
                //    }
                //    if (chkInterPM == "0" && getAttributeValue(selectedFields[i], "tab") == "300") {
                //        objFieldsSelect.transferField(selectedFields[i], false);
                //    }
                //}
            }
        });
    }

    //// Affichage ou non de l'option AutoSelectValue
    //var rbButtons = document.getElementsByName("autoselectenabled");
    //for (var i = 0; i < rbButtons.length; i++) {
    //    rbButtons[i].addEventListener("onclick", function () {
    //        setAttributeValue(document.getElementById("blockAutoSelectValue"), "data-active", this.value);
    //    })
    //}


}
// Affichage d'une option complémentaire pour [adresse] lorsqu'un radio bouton est à 1
nsAdminRelations.checkAutoSelectEnabled = function (element) {
    setAttributeValue(document.getElementById("blockAutoSelectValue"), "data-active", element.value);
}

nsAdminRelations.updateStep2Content = function () {
    var updater = new eUpdater("eda/mgr/eAdminRelationsFieldsManager.ashx", 1);
    updater.addParam("tab", nsAdminRelations.tab, "post");
    updater.send(function (oRes) {

        var stepContent = document.getElementById("stepContent2");
        stepContent.innerHTML = oRes;

        if (FieldsSelect) {
            objFieldsSelect = new FieldsSelect();
        }

    });
}



nsAdminRelations.optionOnClick = function (element) {
    // Afficher la partie correspondante
    var bChecked = getAttributeValue(element, "chk") == "1";
    var p = element.parentElement;
    var div = p.nextSibling;
    if (div && div.className == "relationsFeatures") {

        var ddlInterTables = document.getElementById("ddlInterTables");
        var ddlInterPMADR = document.getElementById("ddlInterPMADR");

        if (bChecked) {


            if (element.id == "chkInterEVT") {
                if (ddlInterTables.value == "")
                    return;
            }
            else if (element.id == "chkInterPM") {
                if (ddlInterPMADR.value == "")
                    return;
            }

            setAttributeValue(div, "data-active", "1");

        }
        else {
            setAttributeValue(div, "data-active", "0");

            if (element.id == "chkInterEVT") {

                ddlInterTables.value = "";

            }
            else if (element.id == "chkInterPM") {

                if (ddlInterPMADR.options.length > 1)
                    ddlInterPMADR.value = "";
            }
        }


    }
}

// Mise à jour des libellés au changement de l'onglet parent
nsAdminRelations.updateParentTab = function (element) {

    var selectedTabName = element.options[element.selectedIndex].innerHTML;
    var selectedTabDescid = element.value;

    if (element.id == "ddlInterPMADR") {
        // Lorsqu'on modifie l'onglet parent, l'option se coche automatiquement
        var chkLink = document.getElementById("chkInterPM");
        if (chkLink) {

            chgChk(chkLink, (selectedTabDescid != ""));
            nsAdminRelations.optionOnClick(chkLink);
        }

        // Mise à jour des libellés
        var elements = document.getElementsByClassName("pmaddressTabName");
        for (var i = 0; i < elements.length; i++) {
            elements[i].innerHTML = selectedTabName;
        }

        var block = document.getElementById("blockPM");

        elements = block.querySelectorAll(".field");
        for (var i = 0; i < elements.length; i++) {
            if (getAttributeValue(elements[i], "data-optfor") == "pm") {
                if (selectedTabDescid == "300") {
                    setAttributeValue(elements[i], "data-active", "1");
                }
                else if (selectedTabDescid == "400") {
                    setAttributeValue(elements[i], "data-active", "0");
                }
            }
            else if (getAttributeValue(elements[i], "data-optfor") == "adr") {
                if (selectedTabDescid == "400") {
                    setAttributeValue(elements[i], "data-active", "1");
                }
                else if (selectedTabDescid == "300") {
                    setAttributeValue(elements[i], "data-active", "0");
                }
            }

        }
    }
    else {
        // Lorsqu'on modifie l'onglet parent, l'option se coche automatiquement
        var chkLink = document.getElementById("chkInterEVT");
        if (chkLink) {

            chgChk(chkLink, (selectedTabDescid != ""));
            nsAdminRelations.optionOnClick(chkLink);
        }

        // Mise à jour des libellés
        var elements = document.getElementsByClassName("parentTabName");
        for (var i = 0; i < elements.length; i++) {
            elements[i].innerHTML = selectedTabName;
        }
    }

}

// Récupère la capsule permettant de mettre à jour les données
nsAdminRelations.setCapsule = function (bUpdateFields) {

    var lstProps = document.querySelectorAll("[dsc]:not([dsc='']");
    var arrAllEntry = Array.prototype.slice.call(lstProps);
    arrAllEntry.forEach(function (myProperty) {

        var sDSC = getAttributeValue(myProperty, "dsc");
        var aDSC = sDSC.split("|");
        if (aDSC.length > 1) {

            var sCat = aDSC[0];
            var sProp = aDSC[1];
            var sVal = "";

            // Type check box
            if (myProperty.hasAttribute("chk")) {
            }
            else if (getAttributeValue(myProperty, "type") == "radio") {
            }
            else if (myProperty.tagName.toLowerCase() == "select") {
            }

            // nsAdminRelations.updCaps.AddProperty(sCat, sProp , sVal);

        }
    });

    var chkInterEVT = document.getElementById("chkInterEVT");
    var aChkDesc = getAttributeValue(chkInterEVT, "dsc").split("|");


    var chkInterPP = document.getElementById("chkInterPP");
    var aChkPPDesc = getAttributeValue(chkInterPP, "dsc").split("|");


    var chkInterPM = document.getElementById("chkInterPM");


    var chkAutolinkEnabled = document.getElementById("chkAutolinkEnabled");

    var hidInterPM = document.getElementById("hidInterPM");
    var hidAdrJoin = document.getElementById("hidAdrJoin");


    var ddlInterEVTNum = document.getElementById("ddlInterTables");
    var ddlInterPMADR = document.getElementById("ddlInterPMADR");

    var aTabDesc = getAttributeValue(ddlInterEVTNum, "dsc").split("|");



    var aChkPMDesc = getAttributeValue(hidInterPM, "dsc").split("|");
    var aChkAdrJoinDesc = getAttributeValue(hidAdrJoin, "dsc").split("|");

    var aChkAutolinkEnabledDesc = getAttributeValue(chkAutolinkEnabled, "dsc").split("|");

    var selectedTabDescid = ddlInterEVTNum ? ddlInterEVTNum.value : "";
    var interPMADR = ddlInterPMADR.value; // 300 ou 400

    if (ddlInterEVTNum) {
        // Mise à jour capsule
        // InterEVT
        nsAdminRelations.updCaps.AddProperty(aTabDesc[0], aTabDesc[1], selectedTabDescid); // InterEVTNum
        nsAdminRelations.updCaps.AddProperty(aChkDesc[0], aChkDesc[1], getAttributeValue(chkInterEVT, "chk"), false); // InterEVT
        nsAdminRelations.addRbPropertyToCapsule("intereventneeded");
        nsAdminRelations.addRbPropertyToCapsule("intereventhidden");
    }

    // PP
    nsAdminRelations.updCaps.AddProperty(aChkPPDesc[0], aChkPPDesc[1], getAttributeValue(chkInterPP, "chk"), false); // InterPP
    nsAdminRelations.addRbPropertyToCapsule("interppneeded");

    // PM
    if (getAttributeValue(chkInterPM, "chk") == "1") {
        if (interPMADR == "300") {
            nsAdminRelations.updCaps.AddProperty(aChkPMDesc[0], aChkPMDesc[1], "1"); // InterPM
            nsAdminRelations.updCaps.AddProperty(aChkAdrJoinDesc[0], aChkAdrJoinDesc[1], "0"); // AdrJoin
        }
        else if (interPMADR == "400") {
            nsAdminRelations.updCaps.AddProperty(aChkPMDesc[0], aChkPMDesc[1], "0"); // InterPM
            nsAdminRelations.updCaps.AddProperty(aChkAdrJoinDesc[0], aChkAdrJoinDesc[1], "1"); // AdrJoin
        }
    }
    else {
        nsAdminRelations.updCaps.AddProperty(aChkPMDesc[0], aChkPMDesc[1], "0", false); // InterPM
        nsAdminRelations.updCaps.AddProperty(aChkAdrJoinDesc[0], aChkAdrJoinDesc[1], "0", false); // AdrJoin
    }

    nsAdminRelations.addRbPropertyToCapsule("interpmneeded");
    nsAdminRelations.addRbPropertyToCapsule("nodefaultlink100");
    nsAdminRelations.addRbPropertyToCapsule("nodefaultlink200");
    nsAdminRelations.addRbPropertyToCapsule("nodefaultlink300");
    nsAdminRelations.addRbPropertyToCapsule("defaultlink100");
    nsAdminRelations.addRbPropertyToCapsule("defaultlink200");
    nsAdminRelations.addRbPropertyToCapsule("defaultlink300");
    nsAdminRelations.addRbPropertyToCapsule("nocascadepmpp");
    nsAdminRelations.addRbPropertyToCapsule("nocascadepppm");

    nsAdminRelations.addRbPropertyToCapsule("rbSearchAll_EVT");
    nsAdminRelations.addRbPropertyToCapsule("rbSearchAllBlocked_EVT");
    nsAdminRelations.addRbPropertyToCapsule("rbSearchAll_PP");
    nsAdminRelations.addRbPropertyToCapsule("rbSearchAllBlocked_PP");
    nsAdminRelations.addRbPropertyToCapsule("rbSearchAll_PMADR");
    nsAdminRelations.addRbPropertyToCapsule("rbSearchAllBlocked_PMADR");

    // AutolinkEnabled
    nsAdminRelations.updCaps.AddProperty(aChkAutolinkEnabledDesc[0], aChkAutolinkEnabledDesc[1], getAttributeValue(chkAutolinkEnabled, "chk")); // AutolinkEnabled

    // AutolinkCreation
    nsAdminRelations.addRbPropertyToCapsule("autolinkcreation");

    // Autolinkfile
    var autolinkfile = document.getElementById("autolinkfile");
    if (autolinkfile != null) {
        aAutolinkfileDsc = getAttributeValue(autolinkfile, "dsc").split("|");
        var autolinkfile100 = nsAdminRelations.getRbValue("autolinkfile100");
        var autolinkfile200 = nsAdminRelations.getRbValue("autolinkfile200");
        var autolinkfile300 = nsAdminRelations.getRbValue("autolinkfile300");
        var autolinkfilevalue = "";
        if (autolinkfile100 != "") {
            if (selectedTabDescid == "0")
                autolinkfile100 = "100";
            else
                autolinkfile100 = (Number(selectedTabDescid) + 10) * 100;
            autolinkfilevalue += ((autolinkfilevalue != "") ? ";" : "") + autolinkfile100;
        }
        if (autolinkfile200 != "")
            autolinkfilevalue += ((autolinkfilevalue != "") ? ";" : "") + autolinkfile200;
        if (autolinkfile300 != "")
            autolinkfilevalue += ((autolinkfilevalue != "") ? ";" : "") + autolinkfile300;

        nsAdminRelations.updCaps.AddProperty(aAutolinkfileDsc[0], aAutolinkfileDsc[1], autolinkfilevalue); // Autolinkfile
    }

    // Tpl_100, Tpl_200, Tpl_300
    nsAdminRelations.addRbPropertyToCapsule("tpl100");
    nsAdminRelations.addRbPropertyToCapsule("tpl200");
    nsAdminRelations.addRbPropertyToCapsule("tpl300");

    // Adresses
    nsAdminRelations.addRbPropertyToCapsule("autoselectenabled");
    var autoSelectValue = document.getElementById("txtAutoSelectValue");
    if (autoSelectValue) {
        var dsc = getAttributeValue(autoSelectValue, "dsc");
        var arrDsc = dsc.split('|');
        nsAdminRelations.updCaps.AddProperty(arrDsc[0], arrDsc[1], autoSelectValue.value);
    }


    // HEADER
    if (objFieldsSelect && bUpdateFields) {
        var list = objFieldsSelect.getSelectedFieldsList();

        if (objFieldsSelect.getOrigSelectedFieldsList() != list) {
            // On met à jour l'entête gauche
            var hidHeader300 = document.getElementById("hidHeader300");
            var dsc = getAttributeValue(hidHeader300, "dsc");
            var arrDsc = dsc.split('|');
            nsAdminRelations.updCaps.AddProperty(arrDsc[0], arrDsc[1], list);
            // On vide l'entête droite
            var hidHeader200 = document.getElementById("hidHeader200");
            dsc = getAttributeValue(hidHeader200, "dsc");
            arrDsc = dsc.split('|');
            nsAdminRelations.updCaps.AddProperty(arrDsc[0], arrDsc[1], "");
        }

    }

    //nsAdminRelations.updCaps.SetCapsuleConfirmed(false);
}

// Récupère la capsule permettant de mettre à jour les données
nsAdminRelationsSQLFilters.setCapsule = function () {
    var updateCategory = document.getElementById("updateCategory"); // 8 = eAdminUpdateProperty.CATEGORY.BKMPREF
    var updateFieldTab = document.getElementById("updateFieldTab"); // 0 = eConst.PREF_BKMPREF.TAB
    var updateFieldBkm = document.getElementById("updateFieldBkm"); // 1 = eConst.PREF_BKMPREF.BKM
    var updateFieldAddedBkmWhere = document.getElementById("updateFieldAddedBkmWhere"); // 10 = eConst.PREF_BKMPREF.ADDEDBKMWHERE

    var valueTab = document.getElementById("valueTab");
    var valueBkm = document.getElementById("valueBkm");
    var valueAddedBkmWhere = document.getElementById("valueAddedBkmWhere");

    nsAdminRelationsSQLFilters.updCaps.AddProperty(updateCategory.value, updateFieldTab.value, valueTab.value); // 8 = BKMPREF, 0 = eConst.PREF_BKMPREF.TAB
    nsAdminRelationsSQLFilters.updCaps.AddProperty(updateCategory.value, updateFieldBkm.value, valueBkm.value); // 8 = BKMPREF, 1 = eConst.PREF_BKMPREF.BKM
    nsAdminRelationsSQLFilters.updCaps.AddProperty(updateCategory.value, updateFieldAddedBkmWhere.value, valueAddedBkmWhere.value); // 8 = BKMPREF, 10 = eConst.PREF_BKMPREF.ADDEDBKMWHERE
}

// Récupère la valeur du radio bouton et met à jour la capsule
nsAdminRelations.addRbPropertyToCapsule = function (name, bNeedConfirm) {
    var value = "";
    var dsc = "";
    var radios = document.getElementsByName(name);
    for (var i = 0, length = radios.length; i < length; i++) {
        if (radios[i].checked) {
            value = radios[i].value;
            dsc = getAttributeValue(radios[i], "dsc");
            break;
        }
    }

    if (dsc != "") {
        var aDesc = dsc.split("|");
        nsAdminRelations.updCaps.AddProperty(aDesc[0], aDesc[1], value, bNeedConfirm);
    }

}


nsAdminRelations.getRbValue = function (name) {
    var value = "";
    var radios = document.getElementsByName(name);
    for (var i = 0, length = radios.length; i < length; i++) {
        if (radios[i].checked) {
            value = radios[i].value;
            break;
        }
    }
    return value;
}