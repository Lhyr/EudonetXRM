var nsAdminFieldsRGPDList = nsAdminFieldsRGPDList || {};

nsAdminFieldsRGPDList.load = function (tab) {
    nsAdminFieldsRGPDList.tab = tab;
    nsAdminFieldsRGPDList.setEventListeners();
}

// Evénements
nsAdminFieldsRGPDList.setEventListeners = function () {
    //
}


// Appel manager au changement d'onglet
nsAdminFieldsRGPDList.changeTab = function (element) {
    var upd = new eUpdater("eda/Mgr/eAdminFieldsRGPDListManager.ashx", 1);
    upd.addParam("tab", element.value, "post");

    upd.ErrorCallBack = function () { eAlert("Chargement échoué"); };

    top.setWait(true);
    upd.send(function (oRes) {
        nsAdminFieldsRGPDList.refreshTable(oRes);
        nsAdminFieldsRGPDList.load(element.value);
    });
}

// Rafraîchissement du tableau à partir des résultats
nsAdminFieldsRGPDList.refreshTable = function (oRes) {
    var container = document.getElementById("fieldsListContainer");
    container.innerHTML = oRes;
    top.setWait(false);
}
