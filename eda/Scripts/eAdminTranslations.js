nsAdminTranslations = {};
nsAdminTranslations.sorts = null;
nsAdminTranslations.refreshList = function (elt) {
    top.setWait(true);
    var descid = document.getElementById("inptDescid").value;
    var resid = document.getElementById("inptResid").value; // différent de descid pour les catalogues et les spécifs
    var fileid = document.getElementById("inptFileid").value;

    var ddlNature = document.getElementById("ddlNature");
    var nature = ddlNature[ddlNature.selectedIndex].value;

    var ddlLang = document.getElementById("ddlLang");
    var lang = ddlLang[ddlLang.selectedIndex].value;

    var inptSearch = document.getElementById("eFSInput");
    var search = inptSearch.value;


    var updTranslations = new eUpdater("eda/mgr/eAdminTranslationsMgr.ashx", 1);
    updTranslations.addParam("action", 1, "post");
    updTranslations.addParam("descid", descid, "post");
    updTranslations.addParam("resid", resid, "post");
    updTranslations.addParam("nature", nature, "post");
    updTranslations.addParam("lang", lang, "post");
    updTranslations.addParam("search", search, "post");
    updTranslations.addParam("fileid", fileid, "post");
    updTranslations.addParam("sorts", JSON.stringify(nsAdminTranslations.sorts), "post");
    updTranslations.send(function (oRes) { nsAdminTranslations.updateContent(oRes, elt); });

};


nsAdminTranslations.updateContent = function (oRes, elt) {
    var div = document.getElementById("TranslationsList");
    div.outerHTML = oRes;
    nsAdminTranslations.initUpdateClick();
    top.setWait(false);
    ///kha : provoque le rafraichissement de la page dès qu'on essaye de sortir de l'input
    ///et on ne peut plus alors que faire echap pour clore la modaldialog
    //if (elt.tagName == "INPUT") {
    //    elt.focus();
    //    var i = elt.value.length;
    //    elt.setSelectionRange(i, i);
    //}
    div = document.getElementById("TranslationsList");
    nsAdminTranslations.sorts = JSON.parse(getAttributeValue(div, "sorts"));

};

nsAdminTranslations.translateInput = document.createElement("INPUT");


nsAdminTranslations.initUpdateClick = function () {
    nsAdminTranslations.updCells = new Array();
    var tab = document.getElementById("tabTranslations");
    var aUpdTd = tab.querySelectorAll("td[upd]");
    for (var i = 0; i < aUpdTd.length; i++) {
        setEventListener(aUpdTd[i], "click", nsAdminTranslations.openEditor, true);
        nsAdminTranslations.updCells.push(aUpdTd[i]);
    }

    setEventListener(nsAdminTranslations.translateInput, "keydown", nsAdminTranslations.onKey, true);
    setEventListener(nsAdminTranslations.translateInput, "click", stopEvent, true);

    var div = document.getElementById("TranslationsList");
    nsAdminTranslations.sorts = JSON.parse(getAttributeValue(div, "sorts"));

};


nsAdminTranslations.openEditor = function (evt, obj) {

    var td = obj || evt.target || evt.srcElement;

    if (td.tagName != "TD")
        return;

    nsAdminTranslations.validTranslate();

    var sOldTranslation = GetText(td);

    nsAdminTranslations.translateInput.value = sOldTranslation;
    nsAdminTranslations.translateInput.className = "translate";

    td.appendChild(nsAdminTranslations.translateInput);

    nsAdminTranslations.translateInput.focus();
    nsAdminTranslations.translateInput.select();

};

nsAdminTranslations.onKey = function (evt) {
    var td = nsAdminTranslations.translateInput.parentElement;

    //td = document.getElementById("");
    if (evt.keyCode == 13) {
        nsAdminTranslations.validTranslate();
    }
    else if (evt.keyCode == 27) {
        td.removeChild(nsAdminTranslations.translateInput);
        //pour éviter la fermeture de la modale.
        stopEvent(evt);
    }
    else if (evt.keyCode == 38 || evt.keyCode == 40) { //arrowup & arrowdown
        var i = nsAdminTranslations.updCells.indexOf(td);
        if (evt.keyCode == 38) { //arrowup
            if (i == 0)
                return;
            i--;
        }
        else if (evt.keyCode == 40) { //arrowdown
            if (i == nsAdminTranslations.updCells.length - 1)
                return;
            i++;
        }
        var nextTd = nsAdminTranslations.updCells[i];
        nsAdminTranslations.openEditor(null, nextTd);
        stopEvent(evt);

    }


};

nsAdminTranslations.validTranslate = function () {
    var td = nsAdminTranslations.translateInput.parentElement;

    if (!td)
        return;

    //si la valeur n'a pas changé, on ne fait rien
    if (GetText(td) == nsAdminTranslations.translateInput.value) {
        td.removeChild(nsAdminTranslations.translateInput);
        return;
    }

    //    SetText(td, nsAdminTranslations.translateInput.value);

    var updObj = JSON.parse(getAttributeValue(td, "upd"));
    updObj.tl = nsAdminTranslations.translateInput.value;

    var updTranslations = new eUpdater("eda/mgr/eAdminTranslationsMgr.ashx", 1);
    updTranslations.json = JSON.stringify(updObj);
    updTranslations.send(function (oRes) { nsAdminTranslations.afterValidTranslate(oRes, td); });
    //updTranslations.send(nsAdminTranslations.afterValidTranslate);

};

nsAdminTranslations.afterValidTranslate = function (oRes, td) {
    var result = JSON.parse(oRes);

    if (result.Success) {
        SetText(td, result.Result[0].tl);
        nsAdminTranslations.green(td);
    }
    else {
        top.eAlert(result.Criticity, top._res_72, result.UserErrorMessage, result.DebugErrorMessage);
    }
};

nsAdminTranslations.green = function (td) {
    var div = document.createElement("DIV");
    div.className = "eFieldEditorEdited";
    SetText(div, GetText(td));
    SetText(td, "");
    td.appendChild(div);

    window.setTimeout(function () { nsAdminTranslations.degreen(td); }, 300);
};

nsAdminTranslations.degreen = function (td) {
    var div = td.querySelector("div");
    if (!div)
        return;

    SetText(td, GetText(div));
};

function srt(obj, col, sort) {
    for (var i = 0; i < nsAdminTranslations.sorts.length; i++) {
        if (nsAdminTranslations.sorts[i].Col == col) {
            nsAdminTranslations.sorts.splice(i, 1);;
            break;
        }
    }
    nsAdminTranslations.sorts.unshift({ Col: col, Sort: sort });
    nsAdminTranslations.refreshList(obj);
}