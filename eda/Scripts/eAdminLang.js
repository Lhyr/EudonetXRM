var nsAdminLang = nsAdminLang || {};

nsAdminLang.load = function (descid) {
    nsAdminLang.addEventListeners();
}
nsAdminLang.addEventListeners = function () {
    var f;
    var fields = document.querySelectorAll("[dsc]");
    for (var i = 0; i < fields.length; i++) {
        f = fields[i];
        f.addEventListener("change", function (e) { nsAdminLang.updateLang(e.target); });
    }

    //fields = document.querySelectorAll(".icon-delete");
    //for (var i = 0; i < fields.length; i++) {
    //    f = fields[i];
    //    f.addEventListener("click", nsAdminLang.deleteLang);
    //}
}

nsAdminLang.updateLang = function (element) {

    var tr = findUp(element, "TR");
    var langId = getAttributeValue(tr, "langId");
    var value = getAttributeValue(element, "chk") == "" ? element.value : getAttributeValue(element, "chk");

    var upd = new eUpdater("eda/Mgr/eAdminLangManager.ashx", 1);
    upd.addParam("action", 1, "post");
    upd.addParam("langId", langId, "post");
    upd.addParam("updateCol", getAttributeValue(element, "dsc"), "post");
    upd.addParam("value", value, "post");

    upd.ErrorCallBack = function () { setWait(false); };
    upd.send(function (oRes) {});
}

nsAdminLang.updateDefault = function (element) {
    //On recupere l element selectionné, on change sa valeur et on met a jour en BDD
    //[id!="' + element.id + '"]
    var listSelected = document.querySelectorAll('*[id^="chkLangDefault"][chk="1"]');
    var selected;
    for (var i = 0; i < listSelected.length; i++) {
        if (listSelected[i].id != element.id)
            selected = listSelected[i];
    }
    if (selected) {
        chgChk(selected);
        nsAdminLang.updateLang(selected);

        nsAdminLang.updateLang(element);
    }
    
}



//nsAdminLang.deleteLang = function (e) {
//    var element = e.target;

//    var tr = findUp(element, "TR");
//    var langId = getAttributeValue(tr, "langId");

//    var conf = top.eConfirm(1, "", top._res_1136, "", null, null,
//        function () {
//            var upd = new eUpdater("eda/Mgr/eAdminLangManager.ashx", 1);
//            upd.addParam("action", 2, "post");
//            upd.addParam("langId", langId, "post");

//            upd.ErrorCallBack = function () { setWait(false); };
//            upd.send(function (oRes) {

//            });
//        }, null, true, true);

    
//}