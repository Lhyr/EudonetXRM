/// <reference path="D:\DEV\eudonetXRM\XRM\Scripts/eTools.js" />

// NameSpace pour les fonctions propre à l'admin des automatismes
var nsAdminAutomation = nsAdminAutomation || {};
// la table automatisme

nsAdminAutomation.create = function (tab, descid) {
    var selectTab = document.getElementById("ddlListTabs");
    if (selectTab && selectTab.value != tab)
        tab = selectTab.value;

    var selectFlds = document.getElementById("ddlListFields");
    if (selectFlds && selectFlds.value > 0)
        descid = selectFlds.value;

    nsAdminAutomation.edit(0, tab, descid, 0);
}
/// fileId : id de l'automatisme
/// tab :table sur laquelle est activé l'automatisme
/// field : rubrique qui déclenche l'automatisme
/// type : type de l'automatisme : notif, rappel ...
nsAdminAutomation.edit = function (fileId, tab, field, type) {

    // scale : mise à l'echelle de 0.80 % de la taille totale de l'ecran (top)
    // min : taille minimale pour la fenetre, pour que le rendu soit optimisé sur les pc 
    // tablet : taille minimale pour les tablettes
    var size = top.getWindowSize().scale(0.8).min({ w: 1024, h: 600 }).tablet({ w: 750 });

    var oAutomationFileModal = new eModalDialog(top._res_7482, 0, "eda/eAdminAutomationFileDialog.aspx", size.w, size.h, "modalAautomationFile");
    oAutomationFileModal.noButtons = false;
    oAutomationFileModal.addParam("tab", tab, "post");
    oAutomationFileModal.addParam("field", field, "post");
    oAutomationFileModal.addParam("id", fileId, "post");
    oAutomationFileModal.addParam("type", type, "post");
    oAutomationFileModal.onIframeLoadComplete = function () { oAutomationFileModal.getIframe().oAutomation.init(); top.setWait(false); }

    top.setWait(true);
    oAutomationFileModal.show();
    oAutomationFileModal.addButton(top._res_29, function () { oAutomationFileModal.hide(); }, 'button-gray', null); // annuler
    oAutomationFileModal.addButton(top._res_28, function () { nsAdminAutomation.validate(oAutomationFileModal); }, 'button-green', null); // valider
}
nsAdminAutomation.validate = function (modal) {
    modal.getIframe().oAutomation.validate(
        function () {
            loadList(1, false);
            modal.hide();
        });
}
nsAdminAutomation.delete = function (fileId) {
    eAdvConfirm({
        'criticity': eAdvConfirm.CRITICITY.MSG_EXCLAM,
        'title': top._res_806,
        'message': top._res_1136,
        //  'details': top._res_1136 ,   
        'bOkGreen': false,
        'resOk': top._res_28,
        'resCancel': top._res_29,
        'okFct': function () {
            var size = top.getWindowSize();

            size.w = size.w * 0.65;
            size.h = size.h * 0.95;

            //var bTabletMode = true;
            if (isTablet() && Number(size.w) < 1024) {
                size.w = 750;
                size.h = 600;
            }

            var mainDiv = document.getElementById("mainDiv");
            var tab = getAttributeValue(mainDiv, "target");

            var ednd = new eUpdater("eda/mgr/eAdminAutomationManager.ashx", 0);
            ednd.addParam("action", "delete", "post");
            ednd.addParam("tab", tab, "post");
            ednd.addParam("fileid", fileId, "post");
            ednd.ErrorCallBack = function () { };
            ednd.send(nsAdminAutomation.deleteReturn);
        }
    });

}
nsAdminAutomation.deleteReturn = function (oRes) {

    var bSuccess = getXmlTextNode(oRes.getElementsByTagName("success")[0]) == "1";
    if (bSuccess) {
        loadList(1, false);
    } else {
        var error = getXmlTextNode(oRes.getElementsByTagName("error")[0]);
        eAlert(0, top._res_416, error);
    }
}
nsAdminAutomation.updateRecordNotifCount = function () {

    var mainDiv = document.getElementById("mainDiv");
    var targetTab = getAttributeValue(mainDiv, "tab");

    // Le compteur est mis a jour uniquement pour la table en cours d'administration
    // si l'automatisme est fait pour une autre table, on zappe
    if (targetTab == top.nGlobalActiveTab) {

        var descid = getAttributeValue(mainDiv, "field");

        // Soit on mis à jour le compteur des automatismes actifs de la rubrique ou ceux de la table
        var notifNumber;
        if (descid != null && descid > 0)
            notifNumber = top.document.getElementById("btnListAutomation_" + descid);
        else if (descid == 0)
                notifNumber = top.document.getElementById("btnListAutomation_" + targetTab);

        if (notifNumber)
            notifNumber.innerHTML = (top._res_7485 + "").replace("<COUNT>", getAttributeValue(mainDiv, "actif-record-count"));

    }
}
nsAdminAutomation.onTabChanged = function (element) {
    var targetTab = element.value
    var mainDiv = document.getElementById("mainDiv");

    setAttributeValue(mainDiv, "tab", targetTab);

    // Quand c'est la table qui change, on réinitialise le select des champ à la valeur par défaut
    setAttributeValue(mainDiv, "field", "0");
    loadList(1, false);
}
nsAdminAutomation.onFldChanged = function (element) {
    var targetFld = element.value
    var mainDiv = document.getElementById("mainDiv");
    setAttributeValue(mainDiv, "field", targetFld);
    loadList(1, false);
}
nsAdminAutomation.refreshListReturn = function (oRes) {

}






