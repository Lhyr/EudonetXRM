var nsAdminHomepages = nsAdminHomepages || {};

var modalUserCat;

nsAdminHomepages.Action = {
    UNDEFINED: 0,
    LIST: 1,
    UPDATEUSERS: 2,
    UPDATETOOLTIP: 3,
    DELETE: 4,
    CLONE: 5,
    ADD: 6,
    EDIT: 7
}

nsAdminHomepages.load = function () {
    var colUsers = document.getElementsByClassName("hpUsers");
    for (var i = 0; i < colUsers.length; i++) {
        setEventListener(colUsers[i], "click", nsAdminHomepages.openUserCatalog);
    }
}

nsAdminHomepages.openUserCatalog = function (event, idElement, userHiden) {

    var input = event.target;
    var module = USROPT_MODULE_ADMIN_HOME_V7_HOMEPAGES;
    var selectedListeUser;
    if (input) {
        input.return = false;
    } else {
        module = USROPT_MODULE_ADMIN_HOME_EXPRESS_MESSAGE
    }


    if (typeof (idElement) != "undefined" && idElement.indexOf("usrs_") != -1) {
        input = document.getElementById(idElement);
        input.return = false;
    } else if (!input && event && event.parentElement && event.parentElement.children[0] && event.parentElement.children[1].children[0].id == idElement) {
        input = event.parentElement.children[1].children[0];
        input.return = true;
    }

    if (typeof (userHiden) != "undefined" && userHiden != null && getAttributeValue(userHiden, "dbv") == "1")
        selectedListeUser = "";
    else {

        var dbv = getAttributeValue(input, "dbv");
        if (!dbv) {
            dbv = getAttributeValue(input.parentNode, "dbv");
        }

        selectedListeUser = dbv;
    }


    modalUserCat = new eModalDialog(top._res_246, 0, "eCatalogDialogUser.aspx", 550, 610);
    modalUserCat.addParam("iframeId", modalUserCat.iframeId, "post");

    modalUserCat.addParam("multi", "1", "post");
    modalUserCat.addParam("usegroup", "1", "post");
    modalUserCat.addParam("selected", selectedListeUser, "post");
    modalUserCat.addParam("modalvarname", "modalUserCat", "post");
    modalUserCat.addParam("showcurrentuser", "1", "post");
    modalUserCat.ErrorCallBack = function () { setWait(false); }
    modalUserCat.show();

    modalUserCat.addButton(top._res_29, function () {
        modalUserCat.hide();
    }, "button-gray", null, null, true);
    modalUserCat.addButton(top._res_28, function () {
        nsAdminHomepages.onUserCatalogOk((typeof (input.return) != "undefined" && input.return) ? input : input.parentNode, idElement, module);
    }, "button-green");
}

nsAdminHomepages.onUserCatalogOk = function (input, idElement, module) {

    var strReturned = modalUserCat.getIframe().GetReturnValue();
    var vals = strReturned.split('$|$')[0];
    var libs = strReturned.split('$|$')[1];

    if (typeof (input.return) != "undefined" && input.return) {
        input.setAttribute("title", libs);
        input.innerHTML = libs
        input.setAttribute("dbv", vals);
        modalUserCat.hide();
        return;
    }


    setAttributeValue(input, "title", libs);
    setAttributeValue(input, "dbv", vals);

    // Mise à jour
    var upd = new eUpdater("eda/Mgr/eAdminHomepagesManager.ashx", 0);
    upd.ErrorCallBack = function () { };
    upd.addParam("action", nsAdminHomepages.Action.UPDATEUSERS, "post");
    upd.addParam("id", getAttributeValue(input, "data-hpId"), "post");
    upd.addParam("module", getUserOptionsModuleHashCode(module), "post");
    //upd.addParam("prop", "UserId", "post");
    upd.addParam("value", getAttributeValue(input, "dbv"), "post");
    upd.send(function () { modalUserCat.hide(); nsAdmin.loadContentModule(module); });
}

// Dupliquer une page d'accueil
nsAdminHomepages.cloneHomepage = function (id) {
    var upd = new eUpdater("eda/Mgr/eAdminHomepagesManager.ashx", 0);
    upd.ErrorCallBack = function () { };
    upd.addParam("action", nsAdminHomepages.Action.CLONE, "post");
    upd.addParam("id", id, "post");
    upd.send(function () { nsAdmin.loadContentModule(USROPT_MODULE_ADMIN_HOME); });
}

// Suppression d'une page d'accueil avec confirmation
nsAdminHomepages.deleteHomepage = function (id) {
    var module = USROPT_MODULE_ADMIN_HOME;
    var conf = top.eConfirm(1, top._res_201, top._res_7602, "", null, null,
               function () {
                   var upd = new eUpdater("eda/Mgr/eAdminHomepagesManager.ashx", 0);
                   upd.ErrorCallBack = function () { };
                   upd.addParam("action", nsAdminHomepages.Action.DELETE, "post");
                   upd.addParam("module", getUserOptionsModuleHashCode(module), "post");
                   upd.addParam("id", id, "post");
                   upd.send(function () { nsAdmin.loadContentModule(module); });
               });


}


// Suppression d'une page d'accueil V7 avec confirmation
nsAdminHomepages.deleteHomepageV7 = function (id) {
    var module = USROPT_MODULE_ADMIN_HOME_V7_HOMEPAGES;
    var conf = top.eConfirm(1, top._res_201, top._res_7602, "", null, null,
               function () {
                   var upd = new eUpdater("eda/Mgr/eAdminHomepagesManager.ashx", 0);
                   upd.ErrorCallBack = function () { };
                   upd.addParam("action", nsAdminHomepages.Action.DELETE, "post");
                   upd.addParam("module", getUserOptionsModuleHashCode(module), "post");
                   upd.addParam("id", id, "post");
                   upd.send(function () { nsAdmin.loadContentModule(module); });
               });


}



// Ajouter un message expresse
nsAdminHomepages.addNewExpressMessage = function () {
    var modalNewExpresssMessage = new eModalDialog(top._res_31, 0, "eda/eAdminNewExpressMessageDialog.aspx", 1000, 500, "modalAdminNewExpressMEssage");
    var eMemoDialogEditorObject = null;
    modalNewExpresssMessage.addCss('eAdmin');
    modalNewExpresssMessage.noButtons = false;
    modalNewExpresssMessage.hideMaximizeButton = false;
    modalNewExpresssMessage.addParam("iframeScrolling", "no", "post");
    modalNewExpresssMessage.onIframeLoadComplete = (function (modal) {
        return function () {
            modal.getIframe().document.getElementById("txtNewExpressMessageName").select();
            eMemoDialogEditorObject = modal.getIframe().init(modal);
        }
    })(modalNewExpresssMessage);
    modalNewExpresssMessage.NoScrollOnMainDiv = true;

    modalNewExpresssMessage.show();

    modalNewExpresssMessage.addButton(top._res_30, function () { modalNewExpresssMessage.hide(); }, 'button-gray', null);
    modalNewExpresssMessage.addButton(top._res_28, function () { nsAdminHomepages.onValidAddOrEditNewExpressMessage(modalNewExpresssMessage, eMemoDialogEditorObject); }, 'button-green', null);
}

//Méthode appelée à la confirmation de addNewExpressMessage
nsAdminHomepages.onValidAddOrEditNewExpressMessage = function (modalNewExpresssMessage, eMemoDialogEditorObject) {
    if (!modalNewExpresssMessage)
        return;
    var module = USROPT_MODULE_ADMIN_HOME_EXPRESS_MESSAGE;
    var doc = modalNewExpresssMessage.getIframe().document;
    var newExpressMessageName = doc.getElementById("txtNewExpressMessageName").value;

    var newExpressMessageContent = encode(eMemoDialogEditorObject.getData());
    var newExpressMessageUsers = getAttributeValue(doc.getElementById("fldExpressUser_0"), "dbv");

    //var allUser = getAttributeValue(doc.getElementById("userAllControl"), "dbv");
    var allUser = doc.querySelector("input[name='UserAll']:checked").value;

    if (typeof (allUser) != "undefined" && allUser != null && allUser == "0")
        newExpressMessageUsers = "0";

    var upd = new eUpdater("eda/Mgr/eAdminHomepagesManager.ashx", 1);
    upd.addParam("newExpressMessageName", newExpressMessageName, "post");
    upd.addParam("newExpressMessageContent", newExpressMessageContent, "post");
    upd.addParam("newExpressMessageUsers", newExpressMessageUsers, "post");
    upd.addParam("module", getUserOptionsModuleHashCode(module), "post");
    upd.addParam("allUser", allUser, "post");

    if (modalNewExpresssMessage.getParam('ident')) {
        upd.addParam("ident", modalNewExpresssMessage.getParam('ident'), "post");
        upd.addParam("action", nsAdminHomepages.Action.EDIT, "post");
    }
    else
        upd.addParam("action", nsAdminHomepages.Action.ADD, "post");

    //upd.ErrorCallBack = function () { eAlert(1,"Erreur","L'ajout du message expresse a échoué."); };
    upd.send(function (oRes) { nsAdminHomepages.onAddedNewExpressMEssage(oRes, modalNewExpresssMessage, module); });
}

nsAdminHomepages.onAddedNewExpressMEssage = function (oRes, modalNewExpresssMessage, module) {
    var result = JSON.parse(oRes);
    if (!result.Success) {
        if (result.Criticity == 2)
            top.eAlert(result.Criticity, top._res_92, result.ErrorMsg);
        else
            top.eAlert(1, top._res_416, result.ErrorMsg);
        return;
    }

    nsAdmin.loadContentModule(module);

    if (modalNewExpresssMessage)
        modalNewExpresssMessage.hide();
};


// Suppression d'un message expresse
nsAdminHomepages.deleteExpressMessage = function (event, id) {

    var module = USROPT_MODULE_ADMIN_HOME_EXPRESS_MESSAGE;
    var conf = top.eConfirm(1, top._res_201, top._res_7603, "", null, null,
               function () {
                   var upd = new eUpdater("eda/Mgr/eAdminHomepagesManager.ashx", 0);
                   upd.ErrorCallBack = function () { };
                   upd.addParam("action", nsAdminHomepages.Action.DELETE, "post");
                   upd.addParam("module", getUserOptionsModuleHashCode(module), "post");
                   upd.addParam("id", id, "post");
                   upd.send(function () { nsAdmin.loadContentModule(module); });
               }
               , true
               , true
               );
    stopEvent(event);

}

// Editer un message expresse
nsAdminHomepages.editExpressMessage = function (id) {
    var modalNewExpresssMessage = new eModalDialog(top._res_151, 0, "eda/eAdminNewExpressMessageDialog.aspx", 1000, 500, "modalAdminNewExpressMEssage");
    var eMemoDialogEditorObject = null;
    modalNewExpresssMessage.addCss('eAdmin');

    modalNewExpresssMessage.noButtons = false;
    modalNewExpresssMessage.hideMaximizeButton = true;
    modalNewExpresssMessage.addParam("iframeScrolling", "no", "post");
    modalNewExpresssMessage.addParam("ident", id, "post");
    modalNewExpresssMessage.onIframeLoadComplete = (function (modal) {
        return function () {
            modal.getIframe().document.getElementById("txtNewExpressMessageName").select();
            eMemoDialogEditorObject = modal.getIframe().init(modal);
        }
    })(modalNewExpresssMessage);
    modalNewExpresssMessage.NoScrollOnMainDiv = true;

    modalNewExpresssMessage.show();

    modalNewExpresssMessage.addButton(top._res_30, function () { modalNewExpresssMessage.hide(); }, 'button-gray', null);
    modalNewExpresssMessage.addButton(top._res_28, function () { nsAdminHomepages.onValidAddOrEditNewExpressMessage(modalNewExpresssMessage, eMemoDialogEditorObject); }, 'button-green', null);
}


nsAdminHomepages.showHideUser = function (obj, catUser, userHiden, idListeUser) {

    if (typeof (obj) != "undefined" && obj.id.indexOf("UserAll") != -1) {
        if (typeof (catUser) != "undefined" && catUser != null) {
            catUser.setAttribute("data-active", obj.value);
            if (typeof (userHiden) != "undefined" && userHiden != null)
                userHiden.setAttribute("dbv", obj.value);
            if (obj.value == "0") {
                idListeUser.innerHTML = "";
                idListeUser.innerText = "";
                idListeUser.value = "";
            }

        }
    }
}

