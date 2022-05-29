var nsAdminMiniFile = nsAdminMiniFile || {};

nsAdminMiniFile.load = function (nType) {
    if (nType == MiniFileType.Kanban) {

    }
}

//nsAdminMiniFile.refreshTemplate = function () {
//    var upd = new eUpdater("eda/mgr/eAdminKanbanMgr.ashx");
//    upd.addParam("action", "3", "post");

//    upd.send(nsAdminMiniFile.refreshTemplateReturn);
//}

//nsAdminMiniFile.refreshTemplateReturn = function (oRes) {
//    var container = document.getElementById("edamfImgContainer");
//    if (container) {
//        container.innerHTML = oRes;
//    }
//}

nsAdminMiniFile.updateMiniFile = function (tab, type) {

    var obj = {};
    var rubrique = {};

    //Rubrique Titre
    rubrique = { value: 0, id: 0, label: false };

    var elTitle = document.getElementById("ddl" + "title");
    if (elTitle != null) {
        rubrique.value = elTitle.value;

        if (elTitle.hasAttribute("edamfmpid")) {
            rubrique.id = elTitle.getAttribute("edamfmpid");
        }
    }

    var elTitleLib = document.getElementById("chkbxLib" + "title");
    if (elTitleLib != null && elTitleLib.hasAttribute("chk")) {
        rubrique.label = elTitleLib.getAttribute("chk") == "1" ? true : false;
    }
    obj.title = rubrique;

    //Rubrique Image
    rubrique = { value: 0, id: 0, label: false };

    var elImage = document.getElementById("ddl" + "image");
    if (elImage != null) {
        rubrique.value = elImage.value;

        if (elImage.hasAttribute("edamfmpid")) {
            rubrique.id = elImage.getAttribute("edamfmpid");
        }
    }
    obj.image = rubrique;

    //Rubriques onglet
    for (var i = 1; i <= 6; ++i) {
        rubrique = { value: 0, id: 0, label: false };

        var elRub = document.getElementById("ddl" + "rub" + i);
        if (elRub != null) {
            rubrique.value = elRub.value;

            if (elRub.hasAttribute("edamfmpid")) {
                rubrique.id = elRub.getAttribute("edamfmpid");
            }
        }

        var elRubLib = document.getElementById("chkbxLib" + "rub" + i);
        if (elRubLib != null && elRubLib.hasAttribute("chk")) {
            rubrique.label = elRubLib.getAttribute("chk") == "1" ? true : false;
        }

        obj["rub" + i] = rubrique;
    }

    var sections = ["pm", "pp", "prt"];
    sections.forEach(function (value, index, array) {
        var section = value;

        rubrique = { value: false, id: 0, sepid: 0, label: false };

        var elSection = document.getElementById("chkbx" + section + "rub");
        if (elSection != null) {
            if (elSection.hasAttribute("chk")) {
                rubrique.value = elSection.getAttribute("chk") == "1" ? true : false;
            }

            if (elSection.hasAttribute("edamfmpspid")) {
                rubrique.sepid = elSection.getAttribute("edamfmpspid");
            }
        }

        var elSectionTitleRub = document.getElementById("ddl" + section + "rub" + "title");
        if (elSectionTitleRub != null && elSectionTitleRub.hasAttribute("edamfmpid")) {
            rubrique.id = elSectionTitleRub.getAttribute("edamfmpid");
        }
        var elSectionTitleLib = document.getElementById("chkbxLib" + section + "rub" + "title");
        if (elSectionTitleLib != null && elSectionTitleLib.hasAttribute("chk")) {
            rubrique.label = elSectionTitleLib.getAttribute("chk") == "1" ? true : false;
        }
        obj[section + "rub"] = rubrique;

        for (var i = 1; i <= 4; ++i) {
            rubrique = { value: 0, id: 0, label: false };

            var elRub = document.getElementById("ddl" + section + "rub" + i);
            if (elRub != null) {
                rubrique.value = elRub.value;

                if (elRub.hasAttribute("edamfmpid")) {
                    rubrique.id = elRub.getAttribute("edamfmpid");
                }
            }

            var elRubLib = document.getElementById("chkbxLib" + section + "rub" + i);
            if (elRubLib != null && elRubLib.hasAttribute("chk")) {
                rubrique.label = elRubLib.getAttribute("chk") == "1" ? true : false;
            }

            obj[section + "rub"]["rub" + i] = rubrique;
        }
    });

    var upd = new eUpdater("eda/Mgr/eAdminMiniFileDialogManager.ashx", 1);
    upd.addParam("tab", tab, "post");
    upd.addParam("type", type, "post");
    upd.addParam("mappingsJson", JSON.stringify(obj), "post");

    upd.send(
        function (oRes) {
            var res = JSON.parse(oRes);
            if (!res.Success) {
                top.eAlert(1, "Mise à jour mapping MiniFiche", res.Error);
            }
            else {
                var modal = eTools.GetModal("modalAdminMiniFile");
                if (modal) {
                    modal.hide();
                }
            }
        }
    );
}

nsAdminMiniFile.adminMinifileToggleSection = function (el, id) {
    if (el != null && el.hasAttribute("chk") && id != null) {
        var checked = el.getAttribute("chk") == "1" ? true : false;

        for (i = 0; i <= 4 ; ++i) {
            var field = null;

            if (i == 0)
                field = document.getElementById("field" + id + "title");
            else
                field = document.getElementById("field" + id + i);

            if (field != null) {
                if (checked)
                    field.style.display = "block";
                else
                    field.style.display = "none";
            }
        }
    }
}

nsAdminMiniFile.adminMinifileToggleField = function (elToggleField) {
    var newValue = elToggleField.value;

    // Si on retire le mapping (valeur "Non renseigné"), on décoche également la case "Libellé"
    if (newValue == "0") {
        var container = findUpByClass(elToggleField, "field");
        var chkLabel = container.querySelector(".edamflibelle .chk");
        if (chkLabel) {
            chgChk(chkLabel, false);
        }
    }

    var oldValue = 0;
    if (elToggleField.hasAttribute("edamfOldvalue")) {
        oldValue = elToggleField.getAttribute("edamfOldvalue");
    }

    var fields = document.getElementsByTagName("select");

    for (i = 0; i < fields.length; ++i) {
        var elField = fields[i];

        if (elField != null && elField != elToggleField) {
            var ops = elField.getElementsByTagName("option");
            for (var j = 0; j < ops.length; ++j) {
                var op = ops[j];
                if (newValue != "0" && op.value == newValue)
                    op.disabled = true;

                if (oldValue != "0" && op.value == oldValue)
                    op.disabled = false;
            }
        }
    }

    elToggleField.setAttribute("edamfOldvalue", newValue);
}

nsAdminMiniFile.adminMinifileHoverField = function (hoveredElement, divId, display) {
    var div = document.getElementById(divId);

    if (div != null) {
        var mfType = getAttributeValue(hoveredElement, "data-mftype");
        if (mfType == "1") {
            if (display)
                addClass(div, "selectedZone");
            else
                removeClass(div, "selectedZone");
        }
        else {
            if (display == true)
                div.style.display = "block";
            else
                div.style.display = "none";
        }

    }
}