
/************************************
 *MOU 26/03/2014
 *Gestion des droits de visus/modifs
 *
 **************************************/
/// Objet qui traite les permissions (modif + visu)
function ePermission() {

    var self = this;

    ///permId, Mode, Level, User
    this._aViewPerm = { "id": "0", "mode": "-1", "level": "0", "user": "" };
    this._aUpdatePerm = { "id": "0", "mode": "-1", "level": "0", "user": "" };
    this._bPublic = 0;

    ///Méthodes  Accesseurs
    this.SetPublic = function (bPublic) {
        self._bPublic = bPublic ? 1 : 0;
    }

    this.IsPublic = function () {
        return self._bPublic == 1;
    }
    this.IsViewPermActive = function () {
        return (self._aViewPerm["mode"] != "-1") && (parseInt(self._aViewPerm["level"]) > 0 || self._aViewPerm["user"] != "")
               ? "1"
               : "0";
    }
    this.IsUpdatePermActive = function () {
        return (self._aUpdatePerm["mode"] != "-1") && (parseInt(self._aUpdatePerm["level"]) > 0 || self._aUpdatePerm["user"] != "")
                ? "1"
                : "0";
    }

    this.GetViewPerm = function () {
        return self._aViewPerm;
    }
    this.GetUpdatePerm = function () {
        return self._aUpdatePerm;
    }

    this.SetViewPerm = function (value) {
        self._aViewPerm = value;
        self.UpdateMode("view");
    }
    this.SetUpdatePerm = function (value) {
        self._aUpdatePerm = value;
        self.UpdateMode("update");
    }

    ///Mis a jour des permission en fonction de type (view ou update)
    this.SetPermParam = function (permType, param, value) {
        if (permType.toLowerCase() == "view")
            self.SetViewPermParam(param, value);
        else if (permType.toLowerCase() == "update")
            self.SetUpdatePermParam(param, value);
    }
    this.SetViewPermParam = function (param, value) {
        this._aViewPerm[param] = value;
        self.UpdateMode("view");
    }
    this.SetUpdatePermParam = function (param, value) {
        self._aUpdatePerm[param] = value;
        self.UpdateMode("update");
    }

    ///Retourne la valeur de la clé dans le dictionnaire droits de visus 
    this.GetViewPermByKey = function (key) {
        if (self.KeyExists(key))
            return self._aViewPerm[key];
    }

    ///Retourne la valeur de la clé dans le dictionnaire droits de modif
    this.GetUpdatePermByKey = function (key) {
        if (self.KeyExists(key))
            return self._aUpdatePerm[key];
    }

    ///Mise à jour du mode
    this.UpdateMode = function (permCode) {

        if (permCode == "view") {
            var nlevel = parseInt(self._aViewPerm["level"]);
            if (isNaN(nlevel))
                nlevel = 0;
            if (nlevel > 0
                && self._aViewPerm["user"] != "") {
                self._aViewPerm["mode"] = "2";
            }
            else if (nlevel > 0 && self._aViewPerm["user"] == "") {
                self._aViewPerm["mode"] = "0";
            }
            else if (nlevel == 0 && self._aViewPerm["user"] != "") {
                self._aViewPerm["mode"] = "1"
            }
            else {
                self._aViewPerm["mode"] = "-1";
            }
        }
        else if (permCode == "update") {
            var nlevel = parseInt(self._aUpdatePerm["level"]);
            if (isNaN(nlevel))
                nlevel = 0;
            if (nlevel > 0
                && self._aUpdatePerm["user"] != "") {
                self._aUpdatePerm["mode"] = "2";
            }
            else if (nlevel > 0 && self._aUpdatePerm["user"] == "") {
                self._aUpdatePerm["mode"] = "0";
            }
            else if (nlevel == 0 && self._aUpdatePerm["user"] != "") {
                self._aUpdatePerm["mode"] = "1"
            }
            else {
                self._aUpdatePerm["mode"] = "-1";
            }
        }
    }

    ///Ajout des parametres de permission à l'objet oSender qui implemente la methode addParam (Utilisation avec l'updater, eModalDialog ... )
    this.AppendParams = function (oSender) {
        //Public
        oSender.addParam("public", self._bPublic, "post");

        //Actives
        oSender.addParam("viewperm", self.IsViewPermActive(), "post");
        oSender.addParam("updateperm", self.IsUpdatePermActive(), "post");

        //View
        oSender.addParam("viewpermid", self.GetViewPermByKey("id"), "post");
        oSender.addParam("viewpermmode", self.GetViewPermByKey("mode"), "post");
        oSender.addParam("viewpermusersid", self.GetViewPermByKey("user"), "post");
        oSender.addParam("viewpermlevel", self.GetViewPermByKey("level"), "post");

        //Update
        oSender.addParam("updatepermid", self.GetUpdatePermByKey("id"), "post");
        oSender.addParam("updatepermmode", self.GetUpdatePermByKey("mode"), "post");
        oSender.addParam("updatepermusersid", self.GetUpdatePermByKey("user"), "post");
        oSender.addParam("updatepermlevel", self.GetUpdatePermByKey("level"), "post");
    }

    ///Clé existe 
    this.KeyExists = function (key) {
        return (key in self._aViewPerm) && (key in self._aUpdatePerm);
    }
}

function onDialogPermLoad(iFrameId) {
    try {
        var oFrm = top.window[iFrameId];
        var oFrmDoc = oFrm.document;
        var oFrmWin = oFrm.window;

        // Donne le focus à la textbox de recherche
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
            if (oFrmDoc.getElementById('PermName')) {
                oFrmDoc.getElementById('PermName').focus();
            }
        }


        addCss("efilterwizard", "Perm", oFrmDoc);
        addCss("eControl", "Perm", oFrmDoc);
        addCss("eModalDialog", "Perm", oFrmDoc);
        addCss("ePerm", "Perm", oFrmDoc);
        addCss("theme", "Perm", oFrmDoc);
        addCss("eudoFont", "Perm", oFrmDoc);


        addScript("eTools", "Perm", null, oFrmDoc);
        addScript("eMd5", "Perm", null, oFrmDoc);
        addScript("eUpdater", "Perm", null, oFrmDoc);
        addScript("eFieldEditor", "Perm", null, oFrmDoc);
        addScript("eFilterWizardLight", "Perm", null, oFrmDoc);
        addScript("eModalDialog", "Perm", null, oFrmDoc);
        addScript("eCalendar", "Perm", null, oFrmDoc);
        addScript("eFilterWizard", "Perm", null, oFrmDoc);
        addScript("eCatalog", "Perm", null, oFrmDoc);
        addScript("ePerm", "Perm", null, oFrmDoc);


    }
    catch (exp) {
    }
}

function onCheckOption(srcId) {
    if (srcId.indexOf("importance_") == 0) {
        var idDiv = srcId.replace("importance", "divGrp");
        var div = document.getElementById(idDiv);
        if (div.style.display == "none")
            div.style.display = "block";
        else
            div.style.display = "none";
    }
    if (document.getElementById(srcId).checked) {

        document.getElementById(srcId).checked = false;
    } else {
        document.getElementById(srcId).checked = true;
    }
    if (srcId.indexOf("OnlyFilesOpt") >= 0) {

        if (document.getElementById(srcId).checked) {
            for (i = 0; i < MAX_NBRE_TABS; i++) {
                if (document.getElementById("OptBlock_" + i) != null) {
                    document.getElementById("OptBlock_" + i).style.display = "block";
                }
            }
        }
        else {
            //cacher les blocks
            for (i = 0; i < MAX_NBRE_TABS; i++) {
                if (document.getElementById("OptBlock_" + i) != null) {
                    document.getElementById("OptBlock_" + i).style.display = "none";
                }
            }
        }

    }


    if (srcId.indexOf("OptViewFilter") >= 0 || srcId.indexOf("OptUpdateFilter") >= 0) {

        if (document.getElementById(srcId).checked) {
            document.getElementById(srcId + "Link").style.display = "block";
        }
        else {
            document.getElementById(srcId + "Link").style.display = "none";
        }
    }
    //par niveau
    if (srcId.indexOf("OptLevels_View") >= 0 || srcId.indexOf("OptLevels_Update") >= 0) {


        if (document.getElementById(srcId).checked) {
            document.getElementById("LevelLst_" + srcId.split("_")[1]).disabled = false;
        }
        else {

            document.getElementById("LevelLst_" + srcId.split("_")[1]).disabled = true;
        }
    }

    //par user
    if (srcId.indexOf("OptUsers_View") >= 0 || srcId.indexOf("OptUsers_Update") >= 0) {

        if (document.getElementById(srcId).checked) {

            document.getElementById("TxtUsers_" + srcId.split("_")[1]).style.display = "inline-block";
            document.getElementById("UsersLink_" + srcId.split("_")[1]).style.display = "inline-block";
        }
        else {

            document.getElementById("TxtUsers_" + srcId.split("_")[1]).style.display = "none";
            document.getElementById("UsersLink_" + srcId.split("_")[1]).style.display = "none";
        }
    }

}

function getPermReturnValue(id, oWin) {
    if (!oWin)
        oWin = window;

    var OptLevels = 0;
    var OptUsers = 0;
    var levels = "";
    var users = "";
    var perMode = "0";
    var Opt = 0;
    if (oWin.document.getElementById("Opt" + id + "Filter").checked)
        Opt = 1;
    if (Opt == 1) {
        if (oWin.document.getElementById("OptLevels_" + id).checked)
            OptLevels = 1;
        if (oWin.document.getElementById("OptUsers_" + id).checked)
            OptUsers = 1;

        if (OptLevels == 1)
            levels = oWin.document.getElementById("LevelLst_" + id).options[oWin.document.getElementById("LevelLst_" + id).selectedIndex].value;

        if (OptUsers == 1) {
            var oTarget = oWin.document.getElementById("TxtUsers_" + id);
            users = oTarget.getAttribute("ednvalue");
        }
        var perMode;

        if (OptLevels == "1" && OptUsers == "1") {
            perMode = "2";
        }
        else if (OptLevels == "1") {
            perMode = "0";
            users = "";
        }
        else if (OptUsers == "1") {
            perMode = "1";
            levels = "";
        }
        else {
            perMode = "-1";
            levels = "";
            users = "";
        }
    }
    //mise a jour des inputs Hidden
    //OptLevels_View
    oWin.document.getElementById("OptLevels_" + id).value = levels;
    oWin.document.getElementById("OptUsers_" + id).value = users;
    oWin.document.getElementById("Opt" + id + "Filter").value = perMode;

    var obj = new Object();
    obj.Opt = Opt;
    obj.levels = levels;
    obj.users = users;
    obj.perMode = perMode;

    return obj;
}

//#region Permission
var modalUserCat;

function SetUsersPerm(id) {
    modalUserCat = new eModalDialog(top._res_246, 0, "eCatalogDialogUser.aspx", 550, 610);
    top.eTabCatUserModalObject.Add(modalUserCat.iframeId, modalUserCat);
    modalUserCat.addParam("iframeId", modalUserCat.iframeId, "post");
    modalUserCat.ErrorCallBack = function () { setWait(false); }
    modalUserCat.addParam("multi", "1", "post");
    modalUserCat.addParam("selected", document.getElementById(id).getAttribute("ednvalue"), "post");
    modalUserCat.addParam("modalvarname", "modalUserCat", "post");
    modalUserCat.show();
    modalUserCat.addButton(top._res_29, onUserCatCancel, "button-gray", id, null, true);
    modalUserCat.addButton(top._res_28, onUserCatOk, "button-green", id);
}

function onUserCatOk(trgId) {


    var strReturned = modalUserCat.getIframe().GetReturnValue();
    modalUserCat.hide();
    var vals = strReturned.split('$|$')[0];
    var libs = strReturned.split('$|$')[1];
    var oTarget = document.getElementById(trgId);
    oTarget.value = libs;
    oTarget.setAttribute("title", libs);
    oTarget.setAttribute("ednvalue", vals);
}

function onUserCatCancel() {
    modalUserCat.hide();
}
//#endregion