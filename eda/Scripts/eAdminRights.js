var nsAdminRights = nsAdminRights || {};

// Affichage de la popup de traitement des droits de traitement
nsAdminRights.showTreatmentsRights = function () {
    // Récup de la liste des droits en cours
    var tidList = "";
    var modalContent = top.modalRights.getIframe().document;
    var tab = modalContent.getElementById("tableFilters");
    var trs = tab.querySelectorAll("tbody tr");
    var hidField = modalContent.getElementById("hidTidList");
    if (hidField)
        tidList = hidField.value;

    var modalTreatmentRights = new top.eModalDialog(top._res_295, 0, "eda/eAdminRightsTreatmentDialog.aspx", 526, 303, "modalTreatmentRights");
    modalTreatmentRights.addScript("eTools");
    modalTreatmentRights.noButtons = false;
    modalTreatmentRights.show();

    modalTreatmentRights.addButton(top._res_29, function () { modalTreatmentRights.hide(); }, 'button-gray', null);
    modalTreatmentRights.addButton(top._res_28, function () { nsAdminRights.updateTreatmentsList(trs); }, 'button-green', null);
}

//nsAdminRights.exportRights = function () {

//    top.setWait(true);

//    var listTabs = document.getElementById("ddlListTabs");
//    if (!listTabs)
//        return;
//    var tab = listTabs.value;

//    if (!tab || tab == "0")
//    {
//        eAlert(2, top._res_7538, top._res_8540);
//        return;
//    }

//    var upd = new top.eUpdater("eda/Mgr/eAdminExportRightsManager.ashx", 1);
//    upd.addParam("tab", tab, "post");
//    upd.ErrorCallBack = function () {
//        top.setWait(false);
//    };
//    upd.send(function () {
//        top.setWait(false);
//    });
//}

// Affichage d'un catalogue utilisateur au clic sur la colonne
// TODO: à fusionner avec showUsersCat et showUsersCatInIP
nsAdminRights.showUsersCatInTreatment = function (element) {
    if (element.getAttribute("active") == "1") {
        var modalUserCatTreatment = new top.eModalDialog(top._res_246, 0, "eCatalogDialogUser.aspx", 550, 610, "modalUserCatTreatment");
        modalUserCatTreatment.addParam("iframeId", modalUserCatTreatment.iframeId, "post");
        modalUserCatTreatment.ErrorCallBack = function () { setWait(false); }
        modalUserCatTreatment.addParam("multi", "1", "post");
        modalUserCatTreatment.addParam("showallusersoption", "1", "post");

        var modalTreatmentRights = top.eTools.GetModal("modalTreatmentRights");
        var topModalContent = modalTreatmentRights.getIframe().document;
        var oTarget = topModalContent.getElementById("lbTraitUsers");
        modalUserCatTreatment.addParam("selected", oTarget.getAttribute("ednvalue"), "post");

        modalUserCatTreatment.addParam("modalvarname", "modalUserCatTreatment", "post");
        modalUserCatTreatment.show();

        modalUserCatTreatment.addButton(top._res_29, function () {
            modalUserCatTreatment.hide();
        }, "button-gray", null, null, true);
        modalUserCatTreatment.addButton(top._res_28, nsAdminRights.onUsersCatTreatmentOk, "button-green");
    }
}

nsAdminRights.onUsersCatTreatmentOk = function () {
    var modal = top.eTools.GetModal("modalUserCatTreatment");
    var strReturned = modal.getIframe().GetReturnValue();
    var vals = strReturned.split('$|$')[0];
    var libs = strReturned.split('$|$')[1];

    var modalTreatmentRights = eTools.GetModal("modalTreatmentRights");

    var topModalContent = modalTreatmentRights.getIframe().document;
    var oTarget = topModalContent.getElementById("lbTraitUsers");

    if (libs == "")
        libs = top._res_141;
    oTarget.value = libs;
    oTarget.setAttribute("title", libs);
    oTarget.setAttribute("ednvalue", vals);
    modal.hide();
}


nsAdminRights.activeTraitField = function (element) {
    var bCheck = true;

    var chk = element.querySelector(".rChk");
    if (getAttributeValue(chk, "chk") == "0")
        bCheck = false;

    var nextElement = element.nextSibling;
    if (nextElement.className == "selectionField") {
        if (nextElement.tagName.toLowerCase() == "label" || nextElement.tagName.toLowerCase() == "input") {
            if (!bCheck)
                nextElement.setAttribute("active", "0");
            else
                nextElement.setAttribute("active", "1");
        }
        else {
            if (!bCheck)
                nextElement.disabled = true;
            else
                nextElement.disabled = false;
        }
    }
}

nsAdminRights.refreshRightsLaunch = function (obj) {
    top.setWait(true);
    var upd = new eUpdater("eda/mgr/eAdminRightDialogMgr.ashx", 1);

    var modalDoc = (top.modalRights) ? top.modalRights.getIframe().document : document;

    // Affichage des permissions pour :
    var levelSelect = modalDoc.getElementById("ddlLevels");
    var userSelect = modalDoc.getElementById("ddlGroupsAndUsers");

    // onglet selectionné
    var tabSelect = modalDoc.getElementById("ddlListTabs");
    var tab = tabSelect.options[tabSelect.selectedIndex].value;

    // Cas particulier pour les page d'accueil, on ajoute son identifiant
    if (tab.indexOf("_") > 0) {
        var parts = tab.split("_");
        tab = getNumber(parts[0]);
        if (parts.length == 2)
            upd.addParam("pageid", getNumber(parts[1]), "post");

    } else
        tab = getNumber(tab);


    upd.addParam("tab", tab, "post");



    //Depuis
    var fromSelect = modalDoc.getElementById("ddlListFrom");
    var from = fromSelect.options[fromSelect.selectedIndex].value;
    from = getNumber(from);

    //rubrique
    var fieldSelect = modalDoc.getElementById("ddlListFields");
    var field = fieldSelect.options[fieldSelect.selectedIndex].value;
    field = getNumber(field);

    // Type
    var typeSelect = modalDoc.getElementById("ddlListTypes");
    var type = typeSelect.options[typeSelect.selectedIndex].value;

    // Fonction
    var fctSelect = modalDoc.getElementById("ddlListFct");
    var fct = fctSelect.options[fctSelect.selectedIndex].value;

    if (obj != fctSelect) {
        fctSelect.selectedIndex = 0;
        fct = "";
    }

    if (obj == tabSelect) {
        fromSelect.selectedIndex = 0;
        from = 0;

        if (tab == 0 && type != "9") // si on affiche tous les onglets, on affiche aucune rubrique (performances)
        {
            fieldSelect.selectedIndex = 1;
            field = -1;
        }
        else {
            fieldSelect.selectedIndex = 0;
            field = 0;
        }
    }
    else if (obj == fromSelect && from > 0) { // si on filtre sur "depuis" on affiche pas d'info de rubrique
        fieldSelect.selectedIndex = 1;
        field = -1;
    }
    else if (obj == fieldSelect && field > -1) { // Si on filtre sur rubrique, on desactive les filtres sur depuis
        fromSelect.selectedIndex = 1;
        from = -1;
    }
    else if (obj == levelSelect) {
        // Si on sélectionne le niveau, le filtre utilisateur revient sur "Tous"
        userSelect.value = "";
    }
    else if (obj == userSelect) {
        // Si on sélectionne l'utilisateur, le filtre niveau revient sur "Tous"
        levelSelect.value = "";
    }
    else if (obj == typeSelect) {
        if (tab == 0 && type != "9") {
            fieldSelect.selectedIndex = 1;
            field = -1;
        }
        else {
            fieldSelect.selectedIndex = 0;
            field = 0;
        }
    }

    //Si on affiche tous les onglets, le filtre sur les rubrique est masqué
    var fltField = modalDoc.getElementById("fltField");
    if (tab == 0) {
        fltField.style.display = "none";
    }
    else {
        fltField.style.display = "";
    }


    upd.addParam("from", from, "post");
    upd.addParam("field", field, "post");
    upd.addParam("fct", fct, "post");

    // et les types de droits qu'on veut afficher
    var typesSelect = modalDoc.getElementById("ddlListTypes");
    var type = typesSelect.options[typesSelect.selectedIndex].value;
    upd.addParam("types", type, "post");

    // permet de placer un indicateur pour savoir si le niveau en question a accès au droit concerné par la ligne
    var levelSelect = modalDoc.getElementById("ddlLevels");
    var level = levelSelect.options[levelSelect.selectedIndex].value;
    upd.addParam("level", level, "post");

    // permet de placer un indicateur pour savoir si l'utilsateur en question a accès au droit concerné par la ligne
    var userSelect = modalDoc.getElementById("ddlGroupsAndUsers");
    var users = userSelect.options[userSelect.selectedIndex].value;
    upd.addParam("users", users, "post");

    upd.send(function (oRes) {
        document.getElementById("hidTab").value = tab;
        document.getElementById("hidFct").value = fct;
        nsAdminRights.refreshRights(oRes);
    });

    if (obj == tabSelect) {
        var updFlt = new eUpdater("eda/mgr/eAdminRightDialogMgr.ashx", 1);
        updFlt.addParam("action", 1, "post");
        updFlt.addParam("tab", tab, "post");
        updFlt.send(nsAdminRights.refreshRightsFilter);
    }
};

nsAdminRights.refreshRights = function (oRes) {
    oRes = oRes.replace(/^<div id="rightsAdminModalContent" class="adminModalContent">/i, "").replace(/<\/div>$/i, "");

    var modalDoc = (top.modalRights) ? top.modalRights.getIframe().document : document;

    var tableTreats = modalDoc.getElementById("tableWrapper");
    tableTreats.outerHTML = oRes;

    nsAdminRights.loadRightsSliders();

    nsAdmin.LoadFunctionsValues();

    top.setWait(false);

};


nsAdminRights.loadRightsSliders = function () {

    top.setWait(true);

    var sliders = document.getElementsByClassName('nouislider');

    nsAdmin.loadSliders(noUiSlider, sliders, function (slider) {
        // Lorsque l'on est sur "Ne pas tenir compte du niveau", la barre devient grise
        nsAdminRights.updateTreatment(slider.target);

        var max = getNumber(slider.target.getAttribute("data-max"));

        if (max == getNumber(slider.get())) {
            slider.target.querySelector(".noUi-connect").style.backgroundColor = "gray";
        }
        else {
            slider.target.querySelector(".noUi-connect").style.backgroundColor = "";
        }

    }, function (slider) {
        nsAdminRights.updateLevelLabel(slider);

        var max = getNumber(slider.target.getAttribute("data-max"));
        var value = getNumber(slider.target.getAttribute("data-value"))
        // Quand la valeur est sur max, on exécute la méthode "set" pour exécuter la fonction associée pour griser la barre
        if (max == value) {
            if (slider.noUiSlider)
                slider.noUiSlider.set(value);
        }
    });

    top.setWait(false);

}

nsAdminRights.refreshRightsFilter = function (oRes) {
    var oResult = JSON.parse(oRes);
    var modalDoc = (top.modalRights) ? top.modalRights.getIframe().document : document;

    var fromSelect = modalDoc.getElementById("ddlListFrom");
    var fieldSelect = modalDoc.getElementById("ddlListFields");

    var nbFirstOptionsToKeep = 3;
    // on retire toutes les options des ddl à l'exception des 2 premières Tous et Aucun
    while (fromSelect.length > nbFirstOptionsToKeep) {
        fromSelect.remove(nbFirstOptionsToKeep);
    }
    while (fieldSelect.length > nbFirstOptionsToKeep) {
        fieldSelect.remove(nbFirstOptionsToKeep);
    }

    fromSelect.selectedindex = 0;
    fieldSelect.selectedindex = 0;

    for (var i = 0; i < oResult.Fields.length; i++) {
        var fld = oResult.Fields[i];
        var option = modalDoc.createElement("option");
        option.text = fld.Text;
        option.value = fld.Id;
        fieldSelect.add(option)
    }


    for (var i = 0; i < oResult.FromTabs.length; i++) {
        var fld = oResult.FromTabs[i];
        var option = document.createElement("option");
        option.text = fld.Text;
        option.value = fld.Id;
        fromSelect.add(option)
    }



};



nsAdminRights.getTreatRightCapsule = function (tr) {
    return {
        tl: getAttributeValue(tr, "tl"),
        pid: getAttributeValue(tr, "pid"),
        tid: getAttributeValue(tr, "tid"),
        did: getAttributeValue(tr, "did")
    };
};



nsAdminRights.updateLevelLabel = function (slider) {
    var value = Number(slider.get());
    var outputValue = slider.target.parentElement.querySelector("output");
    outputValue.innerHTML = nsAdmin.getLevelOutputText(value.toString());
}

// Mise à jour d'un droit de traitement
nsAdminRights.updateTreatment = function (obj) {
    if (nsAdminRights.timerSlider)
        window.clearTimeout(nsAdminRights.timerSlider);

    var tr = findUp(obj, "TR");
    var Container = {
        //action: "update",
        capsules: [nsAdminRights.getTreatRightCapsule(tr)],
        level: 0,
        user: ""
    };

    //var levelValue = frameContent.getElementById("levelTrait" + traitId);
    //var usersValue = frameContent.getElementById("usersTrait" + traitId);
    var levelValue = tr.querySelector("[perm='level']");
    var usersValue = tr.querySelector("[perm='user']");

    var levelSlider = levelValue.noUiSlider;
    var lValue = Number(levelSlider.get()).toString();

    var uValue = getAttributeValue(usersValue, "ednvalue");

    if (getAttributeValue(levelValue, "lastvalid") != lValue || getAttributeValue(usersValue, "lastvalid") != uValue) {

        // Libellé "Aucun utilisateur" ou "Tous les utilisateurs"
        if (uValue.length == 0)
            usersValue.text = nsAdmin.getNullUserLabel(uValue);

        Container.user = uValue;
        Container.level = lValue;

        var fctMaj = function () {
            var upd = new eUpdater("eda/Mgr/eAdminTreatmentRightManager.ashx", 0);
            var json = JSON.stringify(Container);
            upd.json = json;

            //upd.ErrorCallBack = function () { alert('Erreur'); };
            //upd.addParam("action", "update", "post");
            //upd.addParam("tid", traitId, "post");
            //upd.addParam("pid", traitId, "post");
            //upd.addParam("lvalue", levelValue.value, "post");
            //upd.addParam("uvalue", uValue, "post");
            upd.send(
                function () {
                    setAttributeValue(levelValue, "lastvalid", lValue);
                    setAttributeValue(usersValue, "lastvalid", uValue);
                }
            );
        }


        nsAdminRights.timerSlider = window.setTimeout(fctMaj, 500);
    }
};


nsAdminRights.timerSlider = null;


nsAdminRights.updateTreatmentsList = function (trs) {

    var modalTreatmentRights = eTools.GetModal("modalTreatmentRights");

    var modalDocument = modalTreatmentRights.getIframe().document;
    var bLevel = getAttributeValue(modalDocument.getElementById("chkLevel"), "chk") == "1";
    var bUsers = getAttributeValue(modalDocument.getElementById("chkGroupsAndUsers"), "chk") == "1";
    var lValue = bLevel ? modalDocument.getElementById("ddlTraitLevel").value : -2;
    var uValue = bUsers ? getAttributeValue(modalDocument.getElementById("lbTraitUsers"), "ednvalue") : -2;

    if (lValue == "0" && uValue == "0") {
        // à remplacer par un eConfirm
        alert("Aucun choix sélectionné");
    }
    else {

         var Container = {
            capsules: null,
            isLevel: bLevel,
            isUser: bUsers,
            level: lValue,
            user: uValue
         };
         Container.capsules = new Array();
         for (var i = 0; i < trs.length; i++) {
            Container.capsules.push(nsAdminRights.getTreatRightCapsule(trs[i]));
         }

         var upd = new eUpdater("eda/Mgr/eAdminTreatmentRightManager.ashx", 0);
         var json = JSON.stringify(Container);
         upd.json = json;
         upd.ErrorCallBack = function () { };
         //upd.addParam("action", "updateTreatment", "post");
         //upd.addParam("tidlist", listTid, "post");
         //upd.addParam("updatelevel", bLevel, "post");
         //upd.addParam("updateusers", bUsers, "post");
         //upd.addParam("lvalue", lValue, "post");
         //upd.addParam("uvalue", uValue, "post");
         upd.send(function () {
            modalTreatmentRights.hide();
            nsAdminRights.refreshRightsLaunch();
            //nsAdmin.loadRangeSliders(top.document, true);
         });
    }
};


// Affichage d'un catalogue utilisateur au clic sur la colonne
nsAdminRights.showUsersCat = function (obj) {
    modalUserCat = new eModalDialog(top._res_246, 0, "eCatalogDialogUser.aspx", 550, 610);
    modalUserCat.addParam("iframeId", modalUserCat.iframeId, "post");
    modalUserCat.ErrorCallBack = function () { setWait(false); }
    modalUserCat.addParam("multi", "1", "post");
    modalUserCat.addParam("showallusersoption", "1", "post");
    var tr = findUp(obj, "TR");
    var oTarget = tr.querySelector("[perm='user']");
    var tid = getAttributeValue(tr, "tid")
    if (tid == "101018") {
        modalUserCat.addParam("onlyadmin", "1", "post");
    }

    modalUserCat.addParam("selected", getAttributeValue(oTarget, "ednvalue"), "post");

    modalUserCat.addParam("modalvarname", "modalUserCat", "post");
    modalUserCat.show();

    modalUserCat.addButton(top._res_29, function () {
        modalUserCat.hide();
    }, "button-gray", null, null, true);
    modalUserCat.addButton(top._res_28, function () { nsAdminRights.onUsersCatOk(obj); }, "button-green");
}

nsAdminRights.onUsersCatOk = function(obj) {

    var strReturned = modalUserCat.getIframe().GetReturnValue();
    var bAllUserSelected = modalUserCat.getIframe().IsAllUsersOptionSelected();
    var vals = strReturned.split('$|$')[0];
    var libs = strReturned.split('$|$')[1];

    var tr = findUp(obj, "TR");
    var oTarget = tr.querySelector("[perm='user']");
    var level = tr.querySelector("[perm='level']");
    if (bAllUserSelected) {
        libs = top._res_6869;
        vals = "0";
    }
    else if (vals.length == 0) {
        libs = nsAdmin.getNullUserLabel(vals);
    }

    if (oTarget.innerText)
        oTarget.innerText = libs;
    else
        oTarget.textContent = libs;

    oTarget.setAttribute("title", libs);
    oTarget.setAttribute("value", libs);
    oTarget.setAttribute("ednvalue", vals);
    modalUserCat.hide();

    nsAdminRights.updateTreatment(oTarget);
}
