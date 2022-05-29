//*****************************************************************************************************//
//*****************************************************************************************************//
//*** HLA - 05/2012 - framework d'interaction avec le moteur de mise à jour Engine
//*** Nécessite ...
//*** eTools pour les dates et numériques au format international
//*****************************************************************************************************//
//*****************************************************************************************************//

function fldUpdEngine(descid) {
    this.descId = descid;

    this.newValue = '';
    this.newLabel = ''; //pourquoi pas newDisplayValue ?
    this.forceUpdate = '';
    this.readOnly = false;

    this.multiple = false;
    this.popId = '';
    this.popupType = '';

    this.boundDescId = '';
    this.boundPopup = '';
    this.boundValue = '';

    this.treeView = false;
    // HLA - Par defaut, sans attribut chgedVal, la valeur null indique à engine qu'il faut mettre à jour la valeur Sinon, interpréte le 0 ou 1 pour la mise à jour
    this.chgedVal = null;

    /* Propriètés non présente dans la méthode GetSerialize */
    this.isLink = false;

    this.obligat = false;
    this.isInRules = false;
    this.hasMidFormula = false;

    this.oldValue = null;
    this.oldLabel = null;
    this.format = null;
    this.cellId = null;
    this.label = null;
    this.lib = "";

    this.GetSerialize = function () {

        // ATTENTION EN CAS DE MODIFICATION DE LA STRUCTURE 
        // METTRE A JOUR eFileManager.ashx

        var sepValue = '$:$';      // Séparateur entre le nom et ça valeur
        var sepParam = '$|$';      // Séparateur entre les différents param
        var sepValueRepl = '<#$#:#$#>';      // Valeur qui remplace Séparateur entre le nom et ça valeur
        var sepParamRepl = '<#$#|#$#>';      // Valeur qui remplace Séparateur entre les différents param

        var result = 'did' + sepValue + this.descId;

        this.newValue = this.newValue || "";
        this.newLabel = this.newLabel || "";

        result += sepParam + 'newVal' + sepValue + this.newValue.replace(sepParam, sepParamRepl).replace(sepValue, sepValueRepl);
        if (this.newLabel != null && this.newLabel != '')
            result += sepParam + 'newLab' + sepValue + this.newLabel.replace(sepParam, sepParamRepl).replace(sepValue, sepValueRepl);

        if (this.forceUpdate != null && this.forceUpdate != '')
            result += sepParam + 'forceUpd' + sepValue + ((this.forceUpdate || this.forceUpdate == '1') ? '1' : '0');

        if (this.readOnly)
            result += sepParam + 'readonly' + sepValue + '1';

        if (this.multiple != null && this.multiple)
            result += sepParam + 'mult' + sepValue + '1';
        if (this.popId != null && this.popId != '' && this.popId != '0') {
            result += sepParam + 'popid' + sepValue + this.popId;
            result += sepParam + 'poptyp' + sepValue + this.popupType;
        }

        if (this.boundDescId != null && this.boundDescId != '' && this.boundDescId != '0') {
            result += sepParam + 'bndid' + sepValue + this.boundDescId;
            if (this.boundPopup != null)
                result += sepParam + 'bndtyp' + sepValue + this.boundPopup;
            if (this.boundValue != null)
                result += sepParam + 'bndval' + sepValue + this.boundValue;
        }

        if (this.treeView)
            result += sepParam + 'treeview' + sepValue + '1';

        if (this.chgedVal != null)
            result += sepParam + 'chgedval' + sepValue + (this.chgedVal ? '1' : '0');

        if (this.isB64)
            result += sepParam + 'isb64' + sepValue + '1';



        this.prevValue = this.prevValue || "";
        result += sepParam + 'prevVal' + sepValue + this.prevValue.replace(sepParam, sepParamRepl).replace(sepValue, sepValueRepl);




        return result;
    };
}

// Informations en attente du retour de la frame orm display
var engineOrmWait = null;

function OrmConfirmResponse(validResult, urlResult) {
    if (engineOrmWait == null)
        return;

    try {
        engineOrmWait.Engine.validOrmMiddleConfirm(engineOrmWait.Modal, validResult, urlResult);
    } finally {
        engineOrmWait = null;
    }
}

function eEngine() {
    this.parameters = null; 		// Paramètres (tableau de updaterParameter)
    this.fields = null; 		    // Paramètres des fields

    this.SuccessCallbackFunction = null;
    this.ErrorCallbackFunction = null;
    this.ErrorCustomAlert = null;

    // modal dialog représentant la modaldialog dans laquelle est affichée la fiche (si affichage en popup)
    this.ModalDialog = null;
    // bloque la réactualisation de la liste ou du signet en arrière plan suite à la validation et fermeture de template
    this.ReloadNothing = false;
    // indique si on se positionne sur une suppression
    this.DeleteRecord = false;
    // indique si on se positionne sur une fusion
    this.MergeRecord = false;
    // indique si l'appel vient d'un formulaire
    this.Formular = false;

    this.async = true;
    this.Init = function () {
        this.parameters = new Array();
        this.fields = new Array();
    };

    this.Clear = function () {
        this.parameters = null;
        this.fields = null;
    };

    var that = this;

    // Ajout ou modification d'un field à mettre à jour
    this.AddOrSetField = function (fld) {
        if (!fld.descId)
            return;

        this.fields[fld.descId] = fld;
    };

    // Ajout ou modification d'un paramètre
    this.AddOrSetParam = function (name, value) {
        if (typeof (value) == "boolean")
            this.parameters[name] = value ? "1" : "0";
        else
            this.parameters[name] = value;
    };

    // Recuperation de valeur
    this.GetParam = function (name) {
        if (name in this.parameters)
            return this.parameters[name];
        return '';
    };

    this.UpdateLaunch = function () {

        var currentViewDoc = (this.GetParam("fromKanban") == "1" || (this.ModalDialog && this.ModalDialog.bPlanning)) ? document : top.document;
        var currentView = getCurrentView(currentViewDoc);

        var paramWin = top.getParamWindow();
        var objThm;

        try {
            objThm = (paramWin) ? JSON.parse(paramWin.GetParam("currenttheme").toString()) : { "Id": 11, "Version": 1 };
        }
        catch {
            objThm = { "Id": 11, "Version": 1 }
        }




        // Sauvegarde l'elt déclencheur
        var editorObjectName = this.GetParam('jsEditorVarName');
        if (editorObjectName != '') {
            var editorObject = window[editorObjectName];
            if (!editorObject) {
                try {
                    editorObject = eval(editorObjectName);
                }
                catch (ex) { }
            }
            // HLA - UpdateLaunch peu être rappelé plusieurs fois après une validation (de formule du milieu, de suppression, etc.)
            // Ainsi, pour eviter d'écraser ce param avec un eventuel nouveau SrcElement qui n'a rien avoir avec le SrcElement d'origine.
            // Du coup, j'ajoute le test si la valeur n'existe pas déja dans la collection
            if (editorObject && typeof (editorObject.parentPopup) != 'undefined' && typeof (this.parameters['parentPopupSrcId']) == 'undefined')
                this.AddOrSetParam('parentPopupSrcId', editorObject.GetSourceElement().id);
            //Demande #75 678
            if (editorObject && editorObject.getData && typeof (editorObject.getData) == 'function') {
                var oldValue = editorObject.value;
                var newValue = editorObject.getData();
                if (oldValue != newValue) {
                    var nodeSrcElement = editorObject.getSrcElement();
                    if (nodeSrcElement) {
                        var headerElement = document.getElementById(nodeSrcElement.getAttribute("ename"));
                        //Champ Obligatoire
                        var obligat = getAttributeValue(nodeSrcElement, "obg") == "1"
                        if (obligat && newValue == '') {

                            eAlert(0, top._res_372, top._res_373.replace('<ITEM>', getAttributeValue(headerElement, "lib")));
                            editorObject.setData(oldValue);
                            return;
                        }
                    }
                }
            }
        }

        var oParamGoTabList = {
            to: 3,
            nTab: top.getTabFrom()
        }

        // Pas d'écran grisé lorsqu'on déplace une fiche Planning et popup sur mode list planning
        let bIsEudonetXModificationView = (objThm && objThm.Version > 1 && currentView == "FILE_MODIFICATION");
        if (currentView != "CALENDAR" && currentView != "KANBAN") {
            if (bIsEudonetXModificationView)
                top.setWait(true, undefined, undefined, isIris(top.getTabFrom()));
            else
                top.setWait(true);
        }
        //if (currentView == "KANBAN") {
        //    this.AddOrSetParam("fromKanban", "1");
        //    //oWidgetKanban.setWait(true);
        //}

        try {

            var oEngine = this;

            if (this.DeleteRecord)
                var url = "mgr/eDeleteManager.ashx";
            else if (this.MergeRecord) {
                var url = "mgr/eMergeManager.ashx";

                oEngine.IsMerge = true;
            }
            else if (this.Formular)
                var url = "mgr/eUsrFrmManager.ashx";
            else
                var url = "mgr/eUpdateFieldManager.ashx";

            var oUpdater = new eUpdater(url, null);
            for (var key in this.parameters) {
                if (typeof (this.parameters[key]) != 'function') {
                    oUpdater.addParam(key, this.parameters[key], "post");
                }
            }

            if (oEngine.IsMerge) {
                oEngine.MergeInfos = that.parameters;
            }

            var nbFld = 0;
            for (var key in this.fields) {
                if (typeof (this.fields[key]) != 'function') {
                    oUpdater.addParam('fld_' + nbFld, this.fields[key].GetSerialize(), "post");
                    nbFld++;
                }
            }

            //cf comment commit
            oUpdater.ErrorCallBack = function (oRes) {

                ErrorUpdateTreatmentReturn(oRes, oEngine);

            };

            /*
            //NHA : Correction bug #72930 : refresh le BKM en cas de non envoi de mail sur la page 1 par défaut
            oUpdater.ErrorCallBack = function (oRes) {
                debugger;
                //RefreshFile();
                ReloadBkm(oRes);
            };
            */

            if (typeof (that.ErrorCustomAlert) == "function") {
                oUpdater.ErrorCustomAlert = that.ErrorCustomAlert;
            }

            oUpdater.asyncFlag = this.async;
            oUpdater.send(function (oRes) { UpdateTreatmentReturn(oRes, oEngine); });
        }
        catch (ex) {
            if (currentView != "CALENDAR" && (this.ModalDialog == null || !this.ModalDialog.bPlanning)) {
                top.setWait(false);
            }
        }
    };

    this.ShowOrmMiddleConfirm = function (oConfirm) {
        if (oConfirm == null) {
            this.Clear();
            return;
        }

        var ormId = oConfirm.getAttribute('id');
        var ormUrl = getXmlTextNode(oConfirm.getElementsByTagName("url")[0]);
        var ormUpdates = getXmlTextNode(oConfirm.getElementsByTagName("ormupdates")[0]);

        // On conserve l'ormid et les updates
        this.AddOrSetParam("ormId", ormId);
        this.AddOrSetParam("ormUpdates", ormUpdates);

        var localEngineOrmWait = top.engineOrmWait = new Object();

        var oEngine = this;
        localEngineOrmWait.Engine = oEngine;

        var title = "";		// Demande des architectes, titre de la fenêtre vide
        var modal = new eModalDialog(title, 0, ormUrl, 550, 500);
        localEngineOrmWait.Modal = modal;

        modal.ErrorCallBack = function () {
            try {
                oEngine.cancelMiddleConfirm();
            } finally { modal.hide(); }
        };
        modal.addParam("ormId", ormId, "post");
        modal.hideCloseButton = true;
        modal.show();

        // Pour test :)
        //modal.addButton(top._res_29, function () { oEngine.validOrmMiddleConfirm(modal, false, null); }, "button-gray"); // Annuler
        //modal.addButton(top._res_28, function () { oEngine.validOrmMiddleConfirm(modal, true, 'test'); }, "button-green"); // Valider
    };

    this.validOrmMiddleConfirm = function (modal, validResult, urlResult) {
        try {
            if (validResult) {
                this.AddOrSetParam('ormResponseObj', urlResult);
                this.UpdateLaunch();
            } else {
                this.cancelMiddleConfirm();
            }
        }
        catch (err) {

        }
        finally {
            // On retire la modal
            modal.hide();
        }
    }

    /**
     * Fonction qui ouvre la popup de redescente des données d'Adresse
     * @param {any} descid DescID de la fiche source
     * @param {any} adrtoupd DescID de la fiche Adresse à mettre à jour
     * @param {any} adrnoupd Indique si la mise à jour doit être faite, ou non (selon le choix de l'utilisateur)
     * @param {any} callbackFct NOUVEAU MODE FICHE UNIQUEMENT : fonction à rappeler après la réception de la réponse de l'utilisateur depuis la popup
     */
    this.ShowCheckAdr = function (descid, adrtoupd, adrnoupd, callbackFct) {
        var oEngine = this;
        var modal = new eModalDialog(top._res_961, 0, "eAdrCheck.aspx", 550, 500);
        modal.ErrorCallBack = function () { modal.hide(); };
        modal.addParam("descid", descid, "post");
        modal.addParam("adrtoupd", adrtoupd, "post");
        modal.addParam("adrnoupd", adrnoupd, "post");
        modal.show();

        modal.addButtonFct(top._res_29, function () { oEngine.actionCheckAdr(modal, true, callbackFct); }, "button-gray"); // Annuler
        modal.addButtonFct(top._res_28, function () { oEngine.actionCheckAdr(modal, false, callbackFct); }, "button-green"); // Valider
    };

    /**
     * Fonction exécutée lors de la validation ou l'annulation de la popup de confirmation de redescente des données d'Adresse
     * E17 et Nouveau mode fiche
     * @param {any} modal Objet eModalDialog correspondant à la popup
     * @param {any} btnCancel true si la fonction est déclenchée par le bouton Annuler de la popup, false sinin
     * @param {any} callbackFct NOUVEAU MODE FICHE UNIQUEMENT : à la fin du traitement, appeler cette fonction plutôt que les fonctions E17 par défaut
     */
    this.actionCheckAdr = function (modal, btnCancel, callbackFct) {
        if (!modal) {
            alert('eEngine.actionCheckAdr - TODO');
            return;
        }

        try {
            var ifrm = modal.getIframe();

            if (!ifrm || !ifrm.GetReturnValue)
                alert('eEngine.validCheckAdr - TODO');

            var adrtoupd = ifrm.GetReturnValue(btnCancel);

            this.AddOrSetParam('engAction', '3');       // EngineAction.CHECK_ADR_OK
            this.AddOrSetParam('adrtoupd', adrtoupd);
            if (typeof (callbackFct) != "function")
                this.UpdateLaunch();
            else
                callbackFct(this);
        }
        catch (err) {

        }
        finally {
            modal.hide();
        }
    };

    // Function qui affiche une eConfirm avec 3 bouton Retirer Valider Annuler.
    this.ShowSupMultiOwnerConfirm = function (oConfirm, fileId, fldmultiownerdid, multiownernewval) {
        if (oConfirm == null || fileId == null || typeof fileId == "undefined") {
            this.Clear();
            return;
        }
        var msgType = oConfirm.getAttribute('type');
        var msgTitle = getXmlTextNode(oConfirm.getElementsByTagName("title")[0]);
        var msgDescription = getXmlTextNode(oConfirm.getElementsByTagName("desc")[0]);
        var msgDetail = getXmlTextNode(oConfirm.getElementsByTagName("detail")[0]);

        var oModCfm = new eModalDialog(msgTitle, 1, null, 500, 300);
        oModCfm.textClass = "confirm-msg";
        oModCfm.setMessage(msgDescription, msgDetail, msgType);
        oModCfm.show();
        oModCfm.adjustModalToContent(40);
        var oEngine = this;
        oModCfm.addButtonFct(top._res_29, function () { oEngine.cancelSupConfirm(); oModCfm.hide(); }, 'button-green');
        oModCfm.addButtonFct(top._res_19, function () { oEngine.validSupConfirm(); oModCfm.hide(); }, 'button-gray');
        oModCfm.addButtonFct(top._res_6387, function () { oEngine.validSupMultiOwnerConfirm(fileId, fldmultiownerdid, multiownernewval); oModCfm.hide(); }, 'button-gray');
    };

    this.ShowSupPpConfirm = function (fldMainDisplayVal) {
        var paramWin = top.getParamWindow();
        var objThm;

        try {
            objThm = (paramWin) ? JSON.parse(paramWin.GetParam("currenttheme").toString()) : { "Id": 11, "Version": 1 };
        }
        catch {
            objThm = { "Id": 11, "Version": 1 }
        }
        var oEngine = this;
        var height = 300;
        //ELAIZ - demande 78919 : agrandissement de la  modale de suppression de PM sur eudonet x 
        if (objThm && objThm.Version > 1) {
            height = 500;
        }
        var eDeletePMConfirm = new eModalDialog(top._res_806, 0, "eConfirmDeletePmDialog.aspx", 500, height);
        eDeletePMConfirm.ErrorCallBack = launchInContext(eDeletePMConfirm, eDeletePMConfirm.hide);
        eDeletePMConfirm.addParam("name", fldMainDisplayVal, "post");
        eDeletePMConfirm.addParam("uid", eDeletePMConfirm.UID, "post");
        eDeletePMConfirm.show();
        // Lien vers une fonction situé sur la modal
        var myFunct = (function (obj, myEngine) {
            return function () {
                try {
                    var myModal = obj.getIframe();
                    var chk = myModal.document.getElementById("chk_chkId_" + obj.UID);

                    if (chk && chk.getAttribute("chk") == "1")
                        myEngine.AddOrSetParam('deletePp', '1');

                    var chkAdrDelete = myModal.document.getElementById("chk_chkAdrDelete_" + obj.UID);

                    if (chkAdrDelete && chkAdrDelete.getAttribute("chk") == "1")
                        myEngine.AddOrSetParam('deleteAdr', '1');

                    myEngine.validSupConfirm();
                }
                catch (e) {
                    return;
                }
            }
        })(eDeletePMConfirm, oEngine);

        eDeletePMConfirm.addButtonFct(top._res_29, function () { oEngine.cancelSupConfirm(); eDeletePMConfirm.hide(); }, "button-green");  // Annuler
        eDeletePMConfirm.addButtonFct(top._res_28, function () { myFunct(); eDeletePMConfirm.hide(); }, "button-gray");  // Valider
    };

    this.ShowCustomConfirm = function (oConfirm, okFct, cancelFct) {

        if (oConfirm == null) {
            this.Clear();
            return;
        }

        var msgType = oConfirm.getAttribute('type');
        var msgTitle = getXmlTextNode(oConfirm.getElementsByTagName("title")[0]);
        var msgDescription = getXmlTextNode(oConfirm.getElementsByTagName("desc")[0]);
        var msgDetail = getXmlTextNode(oConfirm.getElementsByTagName("detail")[0]);

        var oEngine = this;
        if (msgType == '1') {
            eConfirm(1, msgTitle, msgDescription, msgDetail, 450, 200, okFct, cancelFct);
        }
        else {
            eAlert(msgType, msgTitle, msgDescription, msgDetail, 450, 300, cancelFct);
        }
    };

    //Methode appelé pour retirer le user en cours d'un rdv de group
    this.validSupMultiOwnerConfirm = function (fileId, fldmultiownerdid, multiownernewval) {
        this.Clear();

        try {
            var eEngineUpdater = new eEngine();
            eEngineUpdater.Init();

            var fldMultiOwner = new fldUpdEngine(fldmultiownerdid);
            fldMultiOwner.newValue = multiownernewval;
            eEngineUpdater.AddOrSetField(fldMultiOwner);

            eEngineUpdater.AddOrSetParam('fileId', fileId);

            eEngineUpdater.ModalDialog = this.ModalDialog;
            var planningFileModal = null;
            if (this.ModalDialog == null)
                planningFileModal = "";     // Pour obliger la fonction à reload le calendar
            else
                planningFileModal = this.ModalDialog.modFile;
            eEngineUpdater.SuccessCallbackFunction = function (engResult) { onPlanningValidateTreatment(engResult, planningFileModal, true, false); };

            eEngineUpdater.UpdateLaunch();
        }
        finally {
            this.Clear();
        }
    };

    this.validSupConfirm = function () {
        this.AddOrSetParam('validDeletion', '1');
        this.UpdateLaunch();
        return;
    };

    this.cancelSupConfirm = function () {
        this.Clear();
    };

    this.validMergeConfirm = function () {
        this.AddOrSetParam('validMerge', '1');
        this.UpdateLaunch();
        return;
    };

    this.cancelMergeConfirm = function () {
        this.Clear();
    };

    this.ShowMiddleConfirm = function (oConfirm) {
        if (oConfirm == null)
            return;

        var msgType = oConfirm.getAttribute('type');
        var msgTitle = getXmlTextNode(oConfirm.getElementsByTagName("title")[0]);
        var msgDescription = getXmlTextNode(oConfirm.getElementsByTagName("desc")[0]);
        var msgDetail = getXmlTextNode(oConfirm.getElementsByTagName("detail")[0]);

        var oEngine = this;
        if (msgType == '1') {
            var oMod = eConfirm(1, msgTitle, msgDescription, msgDetail, 450, 100,
                function () { oEngine.validMiddleConfirm(oConfirm.getAttribute('did'), true); },
                function () { oEngine.cancelMiddleConfirm(); });
            oMod.adjustModalToContent(40);
        }
        else {
            eAlert(msgType, msgTitle, msgDescription, msgDetail, 450, 300,
                function () { oEngine.cancelMiddleConfirm(); });
        }
    };

    this.validMiddleConfirm = function (descId, forceUpd) {
        var doUpdate = false;
        for (var key in this.fields) {
            if (typeof (this.fields[key]) != 'function') {
                var fld = this.fields[key];
                if (fld.descId + '' == descId + '') {
                    fld.forceUpdate = forceUpd;
                    doUpdate = true;
                    break;
                }
            }
        }

        if (doUpdate) {
            this.UpdateLaunch();
            return;
        }

        this.FlagEdit(this.GetParam('jsEditorVarName'), true, this.GetParam('parentPopupSrcId'));
        this.Clear();
    };

    this.cancelMiddleConfirm = function () {
        this.undoUpd();
        this.FlagEdit(this.GetParam('jsEditorVarName'), true, this.GetParam('parentPopupSrcId'));
        this.Clear();
    };

    // Affiche le cadre vert de maj sauf pour les checkbox
    this.FlagEdit = function (editorObjectName, noEdit, srcId) {
        if (editorObjectName != '') {
            var editorObject = window[editorObjectName];
            if (!editorObject) {
                try {
                    editorObject = eval(editorObjectName);
                }
                catch (ex) { }
            }
            if (editorObject && editorObject.flagAsEdited && editorObject.type != 'eCheckBox' && editorObject.type != 'eBitButton')
                editorObject.flagAsEdited(true, noEdit, srcId);
        }
    };

    this.undoUpd = function () {
        try {
            for (var key in this.fields) {
                if (typeof (this.fields[key]) == 'function')
                    continue;

                var fld = this.fields[key];
                if (fld.cellId == null || fld.oldValue == null)
                    continue;

                var oNode = document.getElementById(fld.cellId);
                if (oNode == null || typeof (oNode) == 'undefined')
                    continue;

                // On reprend les anciennes valeurs
                fld.newValue = fld.oldValue;
                fld.newLabel = fld.oldLabel;
                // Pour eviter la mise à jour du BoundValue inutilement
                fld.boundValue = null;

                editInnerField(oNode, fld);
            }
        }
        catch (exp) {
            // Inutile de remonté l'erreur à ce niveau
        }
    };
}

function ErrorUpdateTreatmentReturn(oRes, engineObject) {



    var currentViewDoc = (engineObject.GetParam("fromKanban") == "1") ? document : top.document;
    var currentView = getCurrentView(currentViewDoc);

    var updaterView = currentView;
    // View de la fenêtre initiatrice de la demande de MAJ ou de création. (en fonction de la iframe)
    if (engineObject.ModalDialog != null && engineObject.ModalDialog.modFile != null) {
        currentView = getCurrentView(engineObject.ModalDialog.modFile.document);
    }

    if (currentView != "CALENDAR")
        top.setWait(false);
    //if (currentView == "KANBAN")
    //    oWidgetKanban.setWait(false);

    if (!engineObject && typeof (engineObject) != 'undefined')
        return;

    if (engineObject.ErrorCallbackFunction != null && typeof (engineObject.ErrorCallbackFunction) == 'function') {
        engineObject.ErrorCallbackFunction();
    }

    if (engineObject.DeleteRecord) {
        // Rien besoin
    }
    else if (engineObject.MergeRecord) {
        // Rien besoin
    }
    else if (engineObject.Formular) {
        // Rien besoin - la fonction de retour d'erreur a été définie à l'appel a engine
    }
    else {
        // En cas d'erreur, on recharge l'écran pour éviter toute donnée affiché erroné
        switch (currentView) {
            case "FILE_CREATION":
                // #41898 : On recharge la fiche, sauf pour le cas où on est en envoi de mail depuis le mode liste
                if (!(updaterView == "LIST" && engineObject.GetParam("bTypeMail") == "1"))
                    LoadFileAfterErrorCreation(oRes, engineObject.ModalDialog, updaterView);
                break;
            case "CALENDAR":
                // RIEN
                break;
            case "CALENDAR_LIST":
            case "LIST":
                // RIEN //ReloadList();
                break;
            case "KANBAN":
                oWidgetKanban.reload();
                break;
            case "FILE_CONSULTATION":
            case "FILE_MODIFICATION":
                engineObject.undoUpd();
                engineObject.FlagEdit(engineObject.GetParam('jsEditorVarName'), true, engineObject.GetParam('parentPopupSrcId'));

                break;

        }
    }

    engineObject.Clear();
}

function getResParam(oRes, sParamName, bSafeMode, sDefaultValue) {

    try {
        return getXmlTextNode(oRes.getElementsByTagName("reloadlist")[0]);
    }
    catch (e) {
        if (bSafeMode)
            return sDefaultValue;

        throw e;
    }

}

function UpdateTreatmentReturn(oRes, engineObject, afterUpdate) {
    //    eTools.consoleLogFctCall()
    ;
    var currentProp = {}

    var paramWin = top.getParamWindow();

    var objThm;

    try {
        objThm = (paramWin) ? JSON.parse(paramWin.GetParam("currenttheme").toString()) : { "Id": 11, "Version": 1 };
    }
    catch {
        objThm = { "Id": 11, "Version": 1 }
    }

    if (!engineObject) {

        alert('eEngine.UpdateTreatmentReturn - TODO');
        return;
    }

    //Les retours des formulaires sont traités de façons spécifiques.
    if (engineObject.Formular) {
        if (typeof (engineObject.SuccessCallbackFunction) == "function") {

            engineObject.SuccessCallbackFunction(engineObject, oRes);
        }


        return;
    }

    var editorObjectName = engineObject.GetParam('jsEditorVarName');
    var srcEltId = engineObject.GetParam('parentPopupSrcId');
    var engineConfirm = getXmlTextNode(oRes.getElementsByTagName("confirmmode")[0]);

    var currentViewDoc = (engineObject.GetParam("fromKanban") == "1") ? document : top.document;
    var currentView = getCurrentView(currentViewDoc);

    var isNoteFromParent = engineObject.GetParam('fromparent') == "1";
    var bCallBackGoFile = engineObject.GetParam('callbackgofile') == "1";

    var isMiddleFormula = engineObject.GetParam("engAction") == "4";
    var bReloadAfterCreation = false;

    var bTypeMail = engineObject.GetParam("bTypeMail") == "1";
    // Pas de reload de la fiche après la création
    var bNoLoadFile = engineObject.GetParam("noloadfile") == "1";



    var reloadList = getResParam(oRes, "reloadlist", true, "0") == "1";

    try {


        // Gestion du CHECK ADDRESS
        if (engineConfirm == "1") {
            var adrdescid = getXmlTextNode(oRes.getElementsByTagName("descid")[0]);
            var adrtoupd = getXmlTextNode(oRes.getElementsByTagName("adrtoupd")[0]);
            var adrnoupd = getXmlTextNode(oRes.getElementsByTagName("adrnoupd")[0]);

            engineObject.ShowCheckAdr(adrdescid, adrtoupd, adrnoupd);

            return;
        }
        // Gestion du MIDDLE PROC
        // Gestion de la confirmation de suppression
        // Gestion de la confirmation de fusion
        else if (engineConfirm == "2" || engineConfirm == "3" || engineConfirm == "6") {
            // MessageBox du message ou demande confirmation si l'on veux forcer la mise à jour de la valeur incorrect
            var lstConfirmBox = oRes.getElementsByTagName("confirm");
            if (lstConfirmBox == null || lstConfirmBox.length == 0) return;

            if (engineConfirm == "2")
                engineObject.ShowMiddleConfirm(lstConfirmBox[0]);
            else if (engineConfirm == "3")
                engineObject.ShowCustomConfirm(lstConfirmBox[0],
                    function () { engineObject.validSupConfirm(); },
                    function () { engineObject.cancelSupConfirm(); });
            else if (engineConfirm == "6")
                engineObject.ShowCustomConfirm(lstConfirmBox[0],
                    function () { engineObject.validMergeConfirm(); },
                    function () { engineObject.cancelMergeConfirm(); });

            return;
        }
        // Gestion de la confirmation de suppression avec confirmation de suppression des PP en cascade
        else if (engineConfirm == "4") {
            var fldMainDisplayVal = getXmlTextNode(oRes.getElementsByTagName("fldmaindispval")[0]);
            engineObject.ShowSupPpConfirm(fldMainDisplayVal);

            return;
        }
        // Gestion de la confirmation de suppression avec confirmation de detachement ou suppression du rdv
        else if (engineConfirm == "5") {
            // MessageBox du message ou demande confirmation si l'on veux forcer la mise à jour de la valeur incorrect
            var lstConfirmBox = oRes.getElementsByTagName("confirm");
            if (lstConfirmBox == null || lstConfirmBox.length == 0) return;

            var fldmultiownerdid = getXmlTextNode(oRes.getElementsByTagName("fldmultiownerdid")[0]);
            var multiownernewval = getXmlTextNode(oRes.getElementsByTagName("multiownernewval")[0]);

            engineObject.ShowSupMultiOwnerConfirm(lstConfirmBox[0], engineObject.GetParam("fileId"), fldmultiownerdid, multiownernewval);

            return;
        }
        // Gestion de la confirmation de formule du milieu de l'ORM
        else if (engineConfirm == "7") {
            // Information de la MessageBox de la confirmation de ORM_CONFIRM
            var lstConfirmBox = oRes.getElementsByTagName("confirm");
            if (lstConfirmBox == null || lstConfirmBox.length == 0) return;

            engineObject.ShowOrmMiddleConfirm(lstConfirmBox[0]);

            return;
        }
        // Gestion d'annumlation de l'operation via l'ORM
        else if (engineConfirm == "8") {
            // Information de la MessageBox de la confirmation de ORM_CANCEL
            var lstConfirmBox = oRes.getElementsByTagName("confirm");
            if (lstConfirmBox == null || lstConfirmBox.length == 0) {
                // Si pas de message, on annule la saisie
                engineObject.cancelMiddleConfirm();
            } else {
                // Sinon affichage du message, puis on annule la saisie au clique sur le bouton
                engineObject.ShowCustomConfirm(lstConfirmBox[0],
                    null,
                    function () { engineObject.cancelMiddleConfirm(); });
            }

            return;
        }

        engineObject.Clear();

        //SPH 05/02/2014
        //27488 : dans ce contexte (enregistrement d'une note d'une fiche parente de l'enregistrement courrant ex: note affaire depuis planning),
        //il ne faut pas faire d'autre traitements. Le contexte currentView/updaterView n'est pas en phase avec le retour de l'engine qui suppose 
        // être sur le champ modifié
        if (isNoteFromParent)
            return;

        // View de la fenêtre initiatrice de la demande de MAJ ou de création. (en fonction de la iframe)
        var updaterView = currentView;
        if (engineObject.ModalDialog != null && currentView != "KANBAN")
            updaterView = getCurrentView(engineObject.ModalDialog.modFile.document);

        // Recupe les informations de création de fiche
        var oCreatedRecord = null;
        if (updaterView == "FILE_CREATION") {
            var oCreatedRecord = oRes.getElementsByTagName("createdrecord");
            if (oCreatedRecord != null && oCreatedRecord.length != 0) {
                oCreatedRecord = oCreatedRecord[0];

                // Split car la donnée id représente une liste dans certains cas
                var creaTab = oCreatedRecord.getAttribute('tab');
                var creaFileId = oCreatedRecord.getAttribute('ids');
                if (creaFileId != null && typeof (creaFileId) != "undefined")
                    creaFileId = creaFileId.split(';')[0];

                var oMainFields = oRes.getElementsByTagName("field");
                var sMainLabel = "";
                if (oMainFields != null && oMainFields.length != 0) {
                    var oMainField = oMainFields[0];

                    if (oMainField.querySelector) {
                        oMainField = oMainField.querySelector("record[fid='" + creaFileId + "']");
                    }
                    else {
                        var oRecords = oMainField.getElementsByTagName("record");
                        if (oRecords != null && oRecords.length != 0) {
                            for (var nI = 0; nI < oRecords.length; nI++) {
                                if (oRecords[nI].getAttribute("fid") == creaFileId) {
                                    oMainField = oRecords[nI];
                                    break;
                                }
                            }
                        }
                    }

                    if (oMainField) {
                        sMainLabel = eTools.getInnerHtml(oMainField);
                    }
                }

                if (isInt(creaFileId) && isInt(creaTab)) {

                    oCreatedRecord = { fid: creaFileId, tab: creaTab, lab: sMainLabel };

                    //Pour la création de PP, il y a aussi la creation de adresse
                    if (creaTab == 200)
                        AppendCreatedAdrRecord(oRes, oCreatedRecord);


                } else
                    oCreatedRecord = null;
            }
            else
                oCreatedRecord = null;
        }



        // Recupe les informations de modification de fiche
        var oUpdatedRecord = null;
        if (oCreatedRecord == null) {
            var oUpdatedRecord = oRes.getElementsByTagName("updatedrecord");
            if (oUpdatedRecord != null && oUpdatedRecord.length != 0) {
                oUpdatedRecord = oUpdatedRecord[0];

                var recTab = oUpdatedRecord.getAttribute('tab');
                var recFileId = oUpdatedRecord.getAttribute('id');
                var recAnLnk = oUpdatedRecord.getAttribute('anlnk') == "1";
                var recHistoUpd = oUpdatedRecord.getAttribute('histoupd') == "1";

                if (isInt(recFileId) && isInt(recTab))
                    oUpdatedRecord = { fid: recFileId, tab: recTab, anlnk: recAnLnk, histoupd: recHistoUpd };
                else
                    oUpdatedRecord = null;
            }
            else
                oUpdatedRecord = null;
        }

        // Recupe les informations de suppression de fiche
        var oDeletedRecord = null;
        if (oCreatedRecord == null && oUpdatedRecord == null) {
            var oDeletedRecord = oRes.getElementsByTagName("deletedrecord");
            if (oDeletedRecord != null && oDeletedRecord.length != 0) {
                oDeletedRecord = oDeletedRecord[0];

                var delLstTab = oDeletedRecord.getAttribute('tabs');
                var delMainTab = oDeletedRecord.getAttribute('maintab');

                if (isInt(delMainTab))
                    oDeletedRecord = { lstTab: delLstTab, mainTab: delMainTab };
                else
                    oDeletedRecord = null;
            }
            else
                oDeletedRecord = null;
        }


        // Recupe les informations de fusion de fiche
        var oMergedRecord = null;
        if (oCreatedRecord == null && oUpdatedRecord == null && oDeletedRecord == null) {
            var oMergedRecord = oRes.getElementsByTagName("mergedrecord");
            if (oMergedRecord != null && oMergedRecord.length != 0) {
                oMergedRecord = oMergedRecord[0];

                var mergMasterFileId = oMergedRecord.getAttribute('masterFileId');
                var mergMainTab = oMergedRecord.getAttribute('maintab');

                if (isInt(mergMasterFileId) && isInt(mergMainTab))
                    oMergedRecord = { masterFileId: mergMasterFileId, mainTab: mergMainTab };
                else
                    oMergedRecord = null;
            }
            else
                oMergedRecord = null;
        }

        // Envoi des images non encore uploadées
        var oImages = document.querySelectorAll("img");
        if (engineObject.ModalDialog && engineObject.ModalDialog.modFile)
            oImages = engineObject.ModalDialog.modFile.document.querySelectorAll("img");

        if (oImages) {
            for (var i = 0; i < oImages.length; i++) {
                if (oImages[i].src.indexOf("fid=-1") != -1 || getAttributeValue(oImages[i], "session") == "1") {
                    var url = "mgr/eImageManager.ashx";

                    var oUpdater = new eUpdater(url, null);
                    for (var key in this.parameters) {
                        if (typeof (this.parameters[key]) != 'function')
                            oUpdater.addParam(key, this.parameters[key], "post");
                    }

                    var nbFld = 0;
                    for (var key in this.fields) {
                        if (typeof (this.fields[key]) != 'function') {
                            oUpdater.addParam('fld_' + nbFld, this.fields[key].GetSerialize(), "post");
                            nbFld++;
                        }
                    }

                    var imageFieldFileId = 0;
                    var imageFieldDescId = 0;

                    if (oCreatedRecord != null)
                        imageFieldFileId = oCreatedRecord.fid;
                    else if (oUpdatedRecord != null)
                        imageFieldFileId = oUpdatedRecord.fid;

                    if (document.getElementById(getAttributeValue(oImages[i].parentNode, "ename")))
                        imageFieldDescId = getAttributeValue(document.getElementById(getAttributeValue(oImages[i].parentNode, "ename")), "did");
                    else if (engineObject.ModalDialog.modFile.document.getElementById(getAttributeValue(oImages[i].parentNode, "ename")))
                        imageFieldDescId = getAttributeValue(engineObject.ModalDialog.modFile.document.getElementById(getAttributeValue(oImages[i].parentNode, "ename")), "did");
                    else if (getAttributeValue(oImages[i].parentNode, "did") != "")
                        imageFieldDescId = getAttributeValue(oImages[i].parentNode, "did");

                    var tab = Number(imageFieldDescId) - Number(imageFieldDescId) % 100;

                    //CNA - #48078 - Empêche l'execution de cette partie lors du déclenchement d'une formule du milieu en creátion de fiche
                    if (imageFieldFileId != 0 && imageFieldFileId != "0") {

                        var imageType = "IMAGE_FIELD";
                        if (tab == 101000)
                            imageType = "USER_AVATAR_FIELD";
                        else if (Number(imageFieldDescId) % 100 == 75)
                            imageType = "AVATAR_FIELD";

                        oUpdater.addParam("action", "UPLOAD", "post");
                        oUpdater.addParam("fileId", imageFieldFileId, "post");
                        oUpdater.addParam("fieldDescId", imageFieldDescId, "post");
                        oUpdater.addParam("imageType", imageType, "post");
                        oUpdater.addParam("computeRealThumbnail", "0", "post");
                        oUpdater.addParam("imageWidth", "16", "post");
                        oUpdater.addParam("imageHeight", "16", "post");

                        var oEngine = engineObject;
                        oUpdater.ErrorCallBack = function (oRes) { ErrorImageUploadReturn(oRes, oEngine); };
                        oUpdater.asyncFlag = this.async;
                        oUpdater.send(function (oRes) { ImageUploadReturn(oRes, oEngine); });
                    }
                }
            }
        }

        // Mise à jour de la MRU
        ReloadMru(oRes);



        // Recupe la liste des rubriques impactées par des régles
        var lstDescIdRuleUpdated = getXmlTextNode(oRes.getElementsByTagName("descidrule")[0]);
        if (lstDescIdRuleUpdated.length > 0) {
            lstDescIdRuleUpdated = lstDescIdRuleUpdated.split(';');
        }
        else {
            lstDescIdRuleUpdated = null;
        }

        // Réinitialise les indicateur de refresh
        fieldRefresh.initRefresh();

        var doGlobalRefresh = false;

        // Reloads des fields, bkm, file désactivé
        if (engineObject.ReloadNothing)
            ;
        // On reload la fiche dans le cas d'une fusion
        else if (oMergedRecord != null) {
            // TODO - 33189 - Est-ce le mieux de refresh toute la fiche ?

            //Supprime la fiche en doublon de la lisde des id pour le pagging
            try {

                var eParam = top.document.getElementById('eParam').contentWindow;
                if (eParam) {

                    //Liste des ID de la liste en cours
                    var sIdsListMerged = CleanListIds(eParam.GetParam("List_" + oMergedRecord.mainTab));
                    if (typeof (sIdsListMerged) == "string") {

                        var adListDbl = sIdsListMerged.split(";");

                        if (adListDbl.length > 0 && engineObject.IsMerge && engineObject.MergeInfos) {
                            //Fiche supprimée
                            var dblFile = engineObject.MergeInfos.doublonFileId;

                            var adListDblD = adListDbl.filter(function (elem) { return elem + "" != dblFile + "" });
                            var sNewLst = "";
                            if (adListDblD.length > 0) {
                                sNewLst = adListDblD.join(";");
                                eParam.SetParam("List_" + oMergedRecord.mainTab, sNewLst);
                            }
                            else {
                                //Si plus d'ids, on charge les id suivants
                                var nPage = eParam.GetParam("Page_" + oMergedRecord.mainTab);
                                if (LoadIdsPage(oMergedRecord.mainTab, nPage + 1)) {
                                    eParam.SetParam("Page_" + oMergedRecord.mainTab, nPage + 1);
                                }
                            }
                        }
                    }
                }
            }
            catch (e) {

            }

            RefreshFile();
        }
        // ApplyRuleOnBlank lors du retour des formules du milieu en mode création
        else if (updaterView == "FILE_CREATION" && oCreatedRecord == null) {

            fieldRefresh.refreshFldPopup = true;

            // Fiche pas encore enregistrée
            var modalFrame = null;
            if (engineObject.ModalDialog != null && engineObject.ModalDialog.modFile != null)
                modalFrame = engineObject.ModalDialog.modFile;
            LoadChgFld(oRes, true, modalFrame);

            var oRefreshValue = null;
            var fldEngine = null;
            var isInRule = false;
            var oWin = engineObject.ModalDialog.modFile;
            for (var key in fieldRefresh.dicRecords) {
                if (typeof (fieldRefresh.dicRecords[key]) == 'function')
                    continue;


                oRefreshValue = fieldRefresh.dicRecords[key];
                fldEngine = fieldRefresh.convertRefreshValToUpdEngFld(oRefreshValue);

                if (getAttributeValue(oWin.document.getElementById("COL_" + getTabDescid(fldEngine.descId) + "_" + fldEngine.descId), "rul") == "1"
                    || getAttributeValue(oWin.document.getElementById("COL_" + getTabDescid(fldEngine.descId) + "_" + fldEngine.descId), "mf") == "1"
                ) {
                    isInRule = true;
                    break;
                }
            }

            // HLA - On ajoute également un test sur la rubrique source pour prendre en charge l'appel un eventuel applyruleonblank si une règle est dépendante du champ source - Bug #39 241
            if (!isInRule) {
                var aSrcEltId = srcEltId.split("_");
                var sHeadEltId = aSrcEltId.slice(0, 3).join("_");

                isInRule = getAttributeValue(oWin.document.getElementById(sHeadEltId), "rul") == "1";
            }

            if (isInRule) {
                //var editorObjectName = this.GetParam('jsEditorVarName');
                if (editorObjectName != '') {
                    var editorObject = window[editorObjectName];
                    if (!editorObject) {
                        try {
                            editorObject = eval(editorObjectName);
                        }
                        catch (ex) { }
                    }
                }

                if (editorObject && editorObject.tab == 400 && editorObject.fileId == 0) {
                    if (document.getElementById("fileDiv_" + nGlobalActiveTab) && getAttributeValue(document.getElementById("fileDiv_" + nGlobalActiveTab), "did") == "200" && getAttributeValue(document.getElementById("fileDiv_" + nGlobalActiveTab), "fid") == "0") {
                        oWin.applyRuleOnBlank(200, null, 0, 5, srcEltId);
                    }
                    else
                        oWin.applyRuleOnBlank(editorObject.tab, null, 0, 5, srcEltId);
                }
                else {

                    //#41080 SPH : test de l'existanece de fldEngine + gestion du cas de la création de pp + addresse.
                    // dans ce cas, il faut passer a applyrulonblank 200 puisque c'est la table principale.
                    // TODO : la gestion de ce type de reload est partagé entre efieldeditor.js et eegine.js
                    // du coup, parfois, l'applyruleonblank est appelé 2 fois.
                    // par exemple, en création pp+adr avec un champ sur lequel il existe une règle conditionnel et qui port une formule du milieu qui vient changer une valeur (retour type select 1,'&405=xxxx')
                    // il sera bien de centralisé cela.
                    if (fldEngine != null) {
                        if ((fldEngine.descId - fldEngine.descId % 100 == 400) &&
                            (document.getElementById("fileDiv_" + nGlobalActiveTab) && getAttributeValue(document.getElementById("fileDiv_" + nGlobalActiveTab), "did") == "200" && getAttributeValue(document.getElementById("fileDiv_" + nGlobalActiveTab), "fid") == "0")
                        ) {

                            oWin.applyRuleOnBlank(200, null, 0, 5, srcEltId);
                        }
                        else
                            oWin.applyRuleOnBlank(getTabDescid(fldEngine.descId), null, 0, 5, srcEltId);
                    }
                }
            }
        }
        // Création en mode fiche (non popup) (obsolete)
        else if (updaterView == "FILE_CREATION" && oCreatedRecord != null && engineObject.ModalDialog == null) {

            if (!bReloadAfterCreation)
                LoadFileAfterCreation(oCreatedRecord);

            bReloadAfterCreation = true;
        }
        // Rechargement du rendu list/fiche/bkm/popup lorsque une régle impacte ce rendu
        else if (lstDescIdRuleUpdated != null) {


            // Modif depuis un mode list
            if (updaterView == "LIST" || updaterView == "CALENDAR" || updaterView == "CALENDAR_LIST") {

                // Si la liste n'a pas été raffraichie, on lance tout de même le refresh des fields modifiés
                if (!ReloadList(lstDescIdRuleUpdated)) {
                    doGlobalRefresh = true;
                    // Dans le cas ou la liste n'a pas été raffraichie, on vide la liste pour déclencher le flagedit
                    lstDescIdRuleUpdated = null;
                }
            }
            else if (updaterView == "KANBAN") {
                if (oWidgetKanban) {
                    if (!oWidgetKanban.reloadWithRule(lstDescIdRuleUpdated)) {
                        doGlobalRefresh = true;
                        lstDescIdRuleUpdated = null;
                    }
                }
            }
            // Modif depuis une fiche
            else if (engineObject.ModalDialog == null) {

                //Suppression
                if (oDeletedRecord != null) {
                    if (nGlobalActiveTab == oDeletedRecord.mainTab)
                        top.goTabList(oDeletedRecord.mainTab);
                    else if (eTools.getBookmarkContainer(oDeletedRecord.mainTab))
                        RefreshFile();
                }
                else {
                    //Modif
                    var bGlobalActiveTabRuleUpdated = false;

                    //Parcour des descid impliqué dans une règle
                    // détecte si un champ est conditionné par une règle qui a été recalculé suite à la modification
                    var arrTabRules = []
                    for (var i = 0; i < lstDescIdRuleUpdated.length; ++i) {
                        var nTabRules = lstDescIdRuleUpdated[i] - (lstDescIdRuleUpdated[i] % 100);
                        if ((nTabRules) == nGlobalActiveTab)
                            bGlobalActiveTabRuleUpdated = true;
                        else {
                            if (arrTabRules.indexOf(nTabRules) === -1)
                                arrTabRules.push(nTabRules)
                        }
                    }



                    //Cas fiche signet 
                    if (nGlobalActiveTab != oUpdatedRecord.tab) {

                        var bReloadAllWithRules = false;
                        var bReloadAllBkmWithRules = false;
                        var bReloadHeaderWithRules = false;
                        var bReloadListWithRules = false;

                        //récupération du retour des reload spécifique
                        try {
                            //Reload la fiche
                            bReloadAllWithRules = getXmlTextNode(oRes.getElementsByTagName("reloadfileheader")[0]) == "1";
                            //Reload tous les signets
                            bReloadAllBkmWithRules = getXmlTextNode(oRes.getElementsByTagName("reloaddetail")[0]) == "1";
                            //reload l'entête de fiche
                            bReloadHeaderWithRules = getXmlTextNode(oRes.getElementsByTagName("reloadheader")[0]) == "1";

                            bReloadListWithRules = getResParam(oRes, "reloadlist", true, "0") == "1";
                        }
                        catch (e) {

                        }

                        //Si reload signet+header => reload intégrale de la fiche
                        bReloadAllWithRules = bReloadAllBkmWithRules && bReloadHeaderWithRules

                        //reload intégrale
                        if (bReloadAllWithRules) {
                            RefreshFile(window, srcEltId);

                            fieldRefresh.refreshFld = false;
                            fieldRefresh.refreshFldBkm = false;
                        }
                        else {
                            //  **  reload partiel ***

                            /*  Gestion reload Entête */
                            //Signet mode liste 
                            if (isBkmList(oUpdatedRecord.tab)) {
                                //  reload de l'entête
                                if (bGlobalActiveTabRuleUpdated || bReloadHeaderWithRules) {
                                    RefreshHeader();
                                    fieldRefresh.refreshFld = false;
                                }
                            }
                            //Signet mode incrutsé
                            else if (isBkmFile(oUpdatedRecord.tab)) {

                                //rafraichissemnt en tête systématique
                                RefreshHeader();
                                fieldRefresh.refreshFld = false;

                            }

                            /*  gestion reload signets */

                            //  reload global des signets
                            if (bReloadAllBkmWithRules) {
                                RefreshAllBkm();    // Reload toute l'iframe des bkm
                                fieldRefresh.refreshFldBkm = false;
                            }
                            else {
                                //reload partiel des signets


                                //Refresh uniquement les champs d'entete
                                fieldRefresh.refreshFldHead = true;


                                //Signet encours + signet via reload=bkmXXX
                                var reloadedBkm = ReloadBkm(oRes, oUpdatedRecord) || [];

                                //reload des signets contenant un champ recalculé par une règle qui a été déclenchée
                                if (Array.isArray(reloadedBkm) && Array.isArray(arrTabRules) && arrTabRules.length > 0) {
                                    arrTabRules.forEach(function (elem) {
                                        //si le signet n'a pas été déjà reloadé, on le reload
                                        if (reloadedBkm.indexOf(elem) == -1 && elem != oUpdatedRecord.tab) {
                                            top.RefreshBkm(elem);
                                        }
                                    })
                                }
                            }
                        }
                    }
                    else {
                        //autre (mode fiche "classique") reload globale
                        RefreshFile(window, srcEltId);
                    }
                }
            }
            // Modif depuis popup
            else {
                // Reload la popup si pas de fermeture de la popup
                if (!engineObject.ModalDialog.pupClose && oDeletedRecord == null) {
                    if (updaterView == "FILE_CREATION" && oCreatedRecord != null && getNumber(oCreatedRecord.fid) > 0) {
                        if (!bReloadAfterCreation)
                            LoadFileAfterCreation(oCreatedRecord, engineObject.ModalDialog.modFile);

                        bReloadAfterCreation = true;
                    }
                    else if (isMiddleFormula) {
                        fieldRefresh.refreshFldPopup = true;

                        // Fiche pas encore enregistrée
                        var modalFrame = null;
                        if (engineObject.ModalDialog != null && engineObject.ModalDialog.modFile != null)
                            modalFrame = engineObject.ModalDialog.modFile;
                        LoadChgFld(oRes, true, modalFrame);

                        engineObject.ModalDialog.modFile.applyRuleOnBlank(oUpdatedRecord.tab, null, oUpdatedRecord.fid);
                    }
                    else if (typeof (RefreshFile) == "function") {
                        RefreshFile(engineObject.ModalDialog.modFile);
                    }
                    else if (typeof (engineObject.ModalDialog.modFile.RefreshFile) == "function") {
                        engineObject.ModalDialog.modFile.RefreshFile(engineObject.ModalDialog.modFile);
                    }
                } else {
                    // Dans le cas d'une règle de saisie, on redirige vers la fiche créée
                    if (engineObject.ModalDialog != null && engineObject.ModalDialog.oModFile && engineObject.ModalDialog.oModFile.CallFrom == CallFromNavBar) {
                        if (updaterView == "FILE_CREATION") {
                            if (!bReloadAfterCreation)
                                LoadFileAfterCreation(oCreatedRecord, top);

                            bReloadAfterCreation = true;
                        }
                    }
                }

                // Reload l'arrière plan
                switch (currentView) {
                    case "LIST":
                    case "CALENDAR":
                    case "CALENDAR_LIST":
                        if (oDeletedRecord != null)
                            top.goTabList(oDeletedRecord.mainTab);
                        else {
                            if (oCreatedRecord != null) {
                                if (getAttributeValue(top.document.getElementById("mt_" + nGlobalActiveTab), "edntyp") == "0") {
                                    if (!bReloadAfterCreation)
                                        LoadFileAfterCreation(oCreatedRecord, top);

                                    bReloadAfterCreation = true;
                                }
                                else {



                                    if (currentView == "LIST")
                                        ReloadList();
                                    else

                                        top.loadList();
                                }
                            }
                            else {
                                if (bCallBackGoFile) {
                                    //Mise à jour avec callback pour aller sur une fiche

                                }
                                else {


                                    if (currentView == "LIST")
                                        ReloadList();
                                    else
                                        top.loadList();
                                }
                            }
                        }
                        break;
                    case "KANBAN":
                        oWidgetKanban.reload();
                        break;
                    case "FILE_CONSULTATION":
                    case "FILE_MODIFICATION":
                        if (oCreatedRecord != null) {
                            var sTabId = oCreatedRecord.tab;
                            //NHA bg#74 759 : Bug création fiche contact dans le cas ou il y a une condition d'affichage sur un champ
                            if (top.nGlobalActiveTab == TAB_PM && oCreatedRecord.tab == TAB_PP)
                                sTabId = TAB_ADR
                            if (eTools.getBookmarkContainer(sTabId, top.document)) {
                                if (!engineObject.ModalDialog.pupClose) {
                                    top.loadBkmList(sTabId);
                                    top.RefreshHeader();
                                }
                                else {
                                    top.RefreshFile();
                                }
                            }


                            else if (oCreatedRecord.tab == top.nGlobalActiveTab) {
                                if (!bReloadAfterCreation)
                                    LoadFileAfterCreation(oCreatedRecord, top);
                                bReloadAfterCreation = true;
                            }
                            //Cas où on crée une fiche d'un onglet différent sur lequel on était
                            else if (oCreatedRecord.tab != top.nGlobalActiveTab) {
                                doGlobalRefresh = true;
                            }
                        }
                        else if (oUpdatedRecord != null) {

                            if (bCallBackGoFile) {
                                //Si le callback est un go file, on ne fait pas le reload de fiche / liste
                                // ceux-ci sont inutilent (puisque que une autre fiche va être ouverte) 
                                // et posent de problèmes de synchronicité de load de script
                            }
                            else {
                                if (eTools.getBookmarkContainer(oUpdatedRecord.tab, top.document)) {
                                    if (!engineObject.ModalDialog.pupClose) {
                                        top.loadBkmList(oUpdatedRecord.tab);
                                        top.RefreshHeader();
                                    }
                                    else {
                                        RefreshFile();
                                    }
                                }
                            }
                        } else if (oDeletedRecord != null) {
                            if (eTools.getBookmarkContainer(oDeletedRecord.mainTab, top.document)) {
                                RefreshFile();
                            }
                        }

                        break;
                }
            }
        }
        // Refresh Fields ou Reloads du rendu
        else {
            doGlobalRefresh = true;
        }



        if (doGlobalRefresh) {

            var reloadHeader = getXmlTextNode(oRes.getElementsByTagName("reloadheader")[0]) == "1";
            var reloadDetail = getXmlTextNode(oRes.getElementsByTagName("reloaddetail")[0]) == "1";
            var reloadFileHeader = getXmlTextNode(oRes.getElementsByTagName("reloadfileheader")[0]) == "1";
            var reloadListFromEng = getResParam(oRes, "reloadlist", true, "0") == "1";

            //On ne reloade header/detail que si on est bien sur la table que la table courante
            if (top && nGlobalActiveTab != top.nGlobalActiveTab) {
                reloadHeader = false;
                reloadDetail = false;
                reloadFileHeader = false;
            }


            // [38536] MOU3-PC Ex : Force le recharge de la liste
            var reloadList = currentView == "LIST" && ((reloadHeader && reloadDetail) || reloadListFromEng);

            var reloadKanban = currentView == "KANBAN" && (reloadHeader && reloadDetail);

            // Vérifie que les fonctions de refresh sont disponibles
            reloadHeader = reloadHeader && typeof (RefreshHeader) == 'function';
            reloadDetail = reloadDetail && typeof (RefreshAllBkm) == 'function';
            //reloadFileHeader = reloadFileHeader && typeof (RefreshFileHeader) == 'function';

            //on verifie si la confidentialité a été changée
            var bConfidChange = false;
            var xmlFlds = oRes.getElementsByTagName("field");
            var xmlConfidFld;
            for (var fldIdx = 0; fldIdx < xmlFlds.length; fldIdx++) {
                try {
                    if (getNumber(xmlFlds[fldIdx].getAttribute("descid")) == (getNumber(recTab) + CONFIDENTIAL)) {
                        xmlConfidFld = xmlFlds[fldIdx];
                        break;
                    }
                }
                catch (ex) {
                    break;
                }
            }

            if (xmlConfidFld) {
                var xmlRecs = xmlConfidFld.getElementsByTagName("record");
                var xmlConfidRecFld;
                for (var recIdx = 0; recIdx < xmlRecs.length; recIdx++) {
                    try {
                        if (getNumber(xmlRecs[recIdx].getAttribute("fid")) == recFileId) {
                            xmlConfidRecFld = xmlRecs[recIdx];
                            bConfidChange = xmlConfidRecFld.getAttribute("dbv") == null || xmlConfidRecFld.getAttribute("dbv") == "";
                            break;
                        }
                    }
                    catch (ex) {
                        break;
                    }
                }

            }

            // Modif en mode liste
            //ALISTER #75979 J'ai ajouté ADMIN_FILE parce que l'on doit recharger la liste quand on la met à jour en admin /
            //I add ADMIN_FILE, because it should reload when we update the list on admin
            if (updaterView == "ADMIN_FILE" || updaterView == "LIST" || updaterView == "CALENDAR" || updaterView == "CALENDAR_LIST" || updaterView == "KANBAN") {

                // [38536] MOU3-PC Ex :  
                // Une formule de bas sur un champ PM ecoutant le ADR96, se déclanche sur une modification d'un champ ADR quelconque en mode liste de PP.
                // Engine.cs demande a Engine.js de rafraichir la liste d'ou l'ajout de reloadList    
                if (oUpdatedRecord != null && (oUpdatedRecord.anlnk || oUpdatedRecord.histoupd || bConfidChange || reloadList || reloadKanban)) {
                    if (updaterView == "LIST")
                        ReloadList();
                    else if (updaterView == "KANBAN") {
                        if (oWidgetKanban) {
                            oWidgetKanban.reload();
                        }
                    }
                    else
                        top.goTabList(top.nGlobalActiveTab);
                }
                else
                    fieldRefresh.refreshFld = true;
            }
            // Modif en fiche ou popup
            else {
                // Si la popup reste ouverte, on recharge la fiche pour passer du mode crea au mdoe mode modif ou on raffraichi les fields en mode modification
                // MAB #47 701 : Sauf sur une fiche de type E-mail, qui peut être laissée ouverte dans certains cas (ex : envoi de mail de test)
                // afin de permettre de réitérer l'opération - Même type d'exception que pour le correctif #41 898 plus bas
                if (engineObject.ModalDialog != null && !engineObject.ModalDialog.pupClose) {
                    if (updaterView == "FILE_CREATION" && !bTypeMail) {
                        if (!bReloadAfterCreation)
                            LoadFileAfterCreation(oCreatedRecord, engineObject.ModalDialog.modFile);

                        bReloadAfterCreation = true;
                    }
                    else
                        fieldRefresh.refreshFldPopup = true;
                }
                // Si la fiche est fermée en fonction d'ou on vient, on charge la fiche.
                else if (engineObject.ModalDialog != null && engineObject.ModalDialog.oModFile && engineObject.ModalDialog.oModFile.CallFrom == CallFromNavBar) {
                    if (updaterView == "FILE_CREATION") {

                        if (!bReloadAfterCreation)
                            LoadFileAfterCreation(oCreatedRecord, top);

                        bReloadAfterCreation = true;
                    }
                }

                switch (currentView) {
                    case "LIST":
                    case "CALENDAR":
                    case "CALENDAR_LIST":
                    case "KANBAN":

                        // Cas impossible dans le cas du mode fiche
                        if (engineObject.ModalDialog == null)
                            break;

                        if (updaterView == "FILE_CREATION") {
                            // Si on crée une fiche depuis l'accueil(nGlobalActive = 0) on redirige vers la fiche après création [MOU #34495]
                            // Sauf quand c'est un envoi de mail donc création de fiche de type Email [CRU #41898]
                            // et sauf pour la table [RGPDTreatmentsLogs]
                            if (((getAttributeValue(top.document.getElementById("mt_" + nGlobalActiveTab), "edntyp") == "0" || top.nGlobalActiveTab == 0))
                                && !bTypeMail && top.nGlobalActiveTab != 117000 && !bNoLoadFile) {

                                if (!bReloadAfterCreation)
                                    LoadFileAfterCreation(oCreatedRecord, top);

                                bReloadAfterCreation = true;
                            }
                            else {

                                if (!bCallBackGoFile && !bReloadAfterCreation) {

                                    if (currentView == "LIST")
                                        ReloadList();
                                    else
                                        top.loadList();

                                    fieldRefresh.refreshFldPopup = false;
                                }
                            }
                        }
                        else if (oDeletedRecord != null) {
                            top.goTabList(oDeletedRecord.mainTab);
                            fieldRefresh.refreshFldPopup = false;
                        }
                        else {
                            if (reloadFileHeader || (oUpdatedRecord != null && oUpdatedRecord.anlnk)) {

                                if (!bCallBackGoFile) {
                                    if (currentView == "LIST")
                                        ReloadList();
                                    else if (currentView == "KANBAN") {
                                        if (oWidgetKanban) {
                                            oWidgetKanban.reload();
                                        }
                                    }
                                    else
                                        top.loadList();
                                }
                            }
                            else
                                fieldRefresh.refreshFld = true;
                        }
                        break;
                    case "FILE_CONSULTATION":
                    case "FILE_MODIFICATION":
                        fieldRefresh.refreshFld = true;
                        fieldRefresh.refreshFldBkm = true;

                        if (oCreatedRecord != null && oCreatedRecord.tab == nGlobalActiveTab) {
                            if (!bReloadAfterCreation)
                                LoadFileAfterCreation(oCreatedRecord, top);
                            bReloadAfterCreation = true;
                            break;
                        }

                        var bCallFromFinder = engineObject
                            && engineObject.ModalDialog
                            && engineObject.ModalDialog.oModFile
                            && engineObject.ModalDialog.oModFile.CallFrom == CallFromFinder;

                        if (oCreatedRecord && bCallFromFinder) {
                            reloadHeader = false;
                            reloadDetail = false;
                        }

                        if (oUpdatedRecord && bConfidChange) {
                            //Confidentielle modifiée depuis le mode fiche
                            if (top.nGlobalActiveTab == oUpdatedRecord.tab) {
                                reloadHeader = true;
                                reloadDetail = true;
                            }
                            else if (eTools.getBookmarkContainer(oUpdatedRecord.tab, top.document)) {
                                //Confidentielle modifiée depuis un signet.
                                top.loadBkmList(oUpdatedRecord.tab);
                                fieldRefresh.refreshFldBkm = false;
                            }

                            // HLA - Il faut egalement tester si on n'a pas demandé la fermeture de la fenetre, dans ce cas, inutile de refaire une fenetre qui se ferme - #60737
                            if (engineObject.ModalDialog && !engineObject.ModalDialog.pupClose) {
                                var modFileFrm = engineObject.ModalDialog.oModFile.getIframe();
                                if (modFileFrm && modFileFrm.nGlobalActiveTab == oUpdatedRecord.tab) {
                                    RefreshFile(modFileFrm);
                                    fieldRefresh.refreshFld = false;
                                    fieldRefresh.refreshFldBkm = false;
                                    fieldRefresh.refreshFldPopup = false;
                                }
                            }
                        }

                        if (oDeletedRecord != null && engineObject.ModalDialog == null && !isBkmFile(oDeletedRecord.mainTab) && !isBkmDisc(oDeletedRecord.mainTab)) {
                            // Dans le cas de la suppression de la fiche (non popup) en cours de consultation, on revient sur la liste
                            top.goTabList(oDeletedRecord.mainTab);

                            fieldRefresh.refreshFld = false;
                            fieldRefresh.refreshFldBkm = false;
                            fieldRefresh.refreshFldPopup = false;
                        }
                        else if (reloadHeader && reloadDetail) {
                            // Si le reloadHeader et reloadDetail sont demandés, alors on raffraichit la fiche totalement
                            // Inutile de recharger les valeurs des rubriques si les écrans vont être raffraichis

                            /*
                            var refreshTab = null;
                            if (oCreatedRecord != null && oCreatedRecord.tab != null)
                                refreshTab = oCreatedRecord.tab;

                            RefreshFile(null,null, refreshTab );
                                */

                            if ((oCreatedRecord && oCreatedRecord.tab == nGlobalActiveTab)
                                || (oUpdatedRecord && oUpdatedRecord.tab == top.nGlobalActiveTab)) {
                                if (engineObject.ModalDialog)
                                    RefreshFile(engineObject.ModalDialog.modFile);
                                else {
                                    if (!currentProp.isRefreshingFile) {
                                        currentProp.isRefreshingFile = true;
                                        RefreshFile();
                                    }
                                }
                            }

                            //NHA : demande 74 116 automatisme chargement infinis
                            //SPH : evite de lancer plusieurs fois des refresh avec des setwait bloqué
                            if (!currentProp.isRefreshingFile) {
                                currentProp.isRefreshingFile = true;
                                RefreshFile();
                            }
                            fieldRefresh.refreshFld = false;
                            fieldRefresh.refreshFldBkm = false;
                            fieldRefresh.refreshFldPopup = false;
                        }
                        else {
                            // Refresh du header de la fiche
                            if (reloadHeader) {
                                RefreshHeader();

                                fieldRefresh.refreshFld = false;
                            }
                            else if (reloadFileHeader && engineObject.ModalDialog == null) {
                                if (oUpdatedRecord != null && isBkmFile(oUpdatedRecord.tab)) {
                                    rldPrtInfo(oUpdatedRecord.tab, oUpdatedRecord.fid);
                                }
                                else {
                                    RefreshFileHeader();
                                }
                            }

                            // Refresh du detail (signets) de la fiche
                            if (reloadDetail && engineObject.ModalDialog == null) {
                                RefreshAllBkm();    // Reload toute l'iframe des bkm

                                fieldRefresh.refreshFldBkm = false;
                            }
                            else {
                                if (reloadFileHeader && engineObject.ModalDialog != null) {
                                    if (oCreatedRecord != null) {
                                        ReloadBkm(oRes, oCreatedRecord);
                                    }
                                    else {
                                        ReloadBkm(oRes, oUpdatedRecord);
                                    }
                                }
                                else if (oCreatedRecord != null && isBkmFile(oCreatedRecord.tab)) {
                                    ReloadBkm(oRes, oCreatedRecord);
                                }
                                else if (oUpdatedRecord != null && isBkmFile(oUpdatedRecord.tab)) {
                                    //ReloadBkm(oRes, oUpdatedRecord);
                                }
                                else if (oUpdatedRecord != null && isBkmDisc(oUpdatedRecord.tab)) {
                                    refreshComm(oUpdatedRecord.tab, oUpdatedRecord.fid);
                                }
                                else
                                    ReloadBkm(oRes);     // Reload des signets demandés
                            }

                            var tab = 0, fid = 0;
                            if (oCreatedRecord) {
                                tab = oCreatedRecord.tab;
                                fid = oCreatedRecord.fid;
                            }

                            else if (oUpdatedRecord) {
                                tab = oUpdatedRecord.tab;
                                fid = oUpdatedRecord.fid;
                            }
                            else if (oDeletedRecord) {
                                tab = oDeletedRecord.mainTab;
                                fid = oDeletedRecord.fid;
                            }


                            if (document.getElementById("bkmCntFilter_" + tab) && document.getElementById("bkmCntFilter_" + tab).innerHTML != "") {

                                loadBkmBar(nGlobalActiveTab, top.GetCurrentFileId(nGlobalActiveTab), false, { noReload: true, uptCmptTab: tab });
                            }


                        }
                        break;
                }
            }
        }

        // Raffraichies les valeurs des rubriques affichées
        if (fieldRefresh.refreshFld || fieldRefresh.refreshFldBkm || fieldRefresh.refreshFldPopup) {
            var modalFrame = null;
            if (engineObject.ModalDialog != null && engineObject.ModalDialog.modFile != null)
                modalFrame = engineObject.ModalDialog.modFile;
            LoadChgFld(oRes, true, modalFrame);
        }
        else if (fieldRefresh.refreshFldHead) {

            var modalFrame = null;
            if (engineObject.ModalDialog != null && engineObject.ModalDialog.modFile != null)
                modalFrame = engineObject.ModalDialog.modFile;

            LoadChgFldHead(oRes, true, modalFrame, nGlobalActiveTab);

        }
        // Si la fiche n'est pas rafraichie et s'il n'y a pas de règle dépendant de ce champ
        //  =>  Affiche le cadre vert de maj sauf pour les checkbox et reloadList
        if (!reloadList && !reloadHeader && !reloadFileHeader && (!lstDescIdRuleUpdated || lstDescIdRuleUpdated.length <= 0))
            FlagEditAndFocus(engineObject, editorObjectName, srcEltId);

        // Affichage MsgBox
        ShowProcMessage(oRes);

        // LaunchPage
        ShowProcPage(oRes);

        if (engineObject.SuccessCallbackFunction != null && typeof (engineObject.SuccessCallbackFunction) == 'function') {
            var callbackObject;
            if (oCreatedRecord != null)
                callbackObject = Object.assign({}, oCreatedRecord, { isMiddleFormula: isMiddleFormula });
            else if (oUpdatedRecord != null)
                callbackObject = Object.assign({}, oUpdatedRecord, { isMiddleFormula: isMiddleFormula });
            else
                callbackObject = oRes;


            if (engineObject.SuccessCallbackFunction.name
                && engineObject.SuccessCallbackFunction.name == "RefreshFile"
                && currentProp.isRefreshingFile) {
                currentProp.isRefreshingFile = true;

            }
            else
                engineObject.SuccessCallbackFunction(callbackObject);
        }

    }
    catch (ex) {
        engineObject.Clear();
        // eTools.log.inspect({ 'Message': "eEngine::UpdateTreatmentReturn(oRes, engineObject, afterUpdate)", 'Exception': ex });
    }
    finally {
        try {
            // On tente de fermer tout waiter ouvert
            // Attention : Si le waiter dépend d'une fenêtre modale qui a été fermée précédemment, le setWait() ne pourra pas être exécuté, d'où la vérification sur sa disponibilité.
            // cf. cas levé par la demande #88 822, et les correctifs effectués dans eMain.validateFile() en ce sens 
            //let bIsEudonetXModificationView = (objThm && objThm.Version > 1 && updaterView == "FILE_CREATION");
            //TODO: gérer le setwait avec bIsEudonetXModificationView
            if (updaterView != "CALENDAR" && typeof (top.setWait) == 'function') {
                //if(!bIsEudonetXModificationView)
                top.setWait(false);
            }
            //if (updaterView == "KANBAN") {
            //    oWidgetKanban.setWait(false);
            //}
        }
        catch (exp1) {
            // eTools.log.inspect({ 'Message': "eEngine::UpdateTreatmentReturn(oRes, engineObject, afterUpdate)", 'Exception': exp1 });
        }
    }
}


function FlagEditAndFocus(engineObject, editorObjectName, srcEltId) {
    engineObject.FlagEdit(editorObjectName, false, srcEltId);
    var oSrcEltId = document.getElementById(srcEltId);
    // SPH : Ajout de la restriction oSrcEltId.tagName !="INPUT" - cf 21648
    if (oSrcEltId && oSrcEltId.focus && oSrcEltId.tagName != "INPUT")
        oSrcEltId.focus();
}

/// Pour les nouveau pp créer depuis une laisons, on a besoin de mettre a jour l'adresse de liaison
function AppendCreatedAdrRecord(oRes, oCreatedRecord) {
    var adrRecord = { adrId: 0, adrLab: "", pmId: 0, pmLab: "" }, nDescId = "0";
    var adrId = 0, pmId = 0, field;
    try {

        var fields = oRes.getElementsByTagName("field");
        for (var index = 0; index < fields.length; index++) {

            try {
                field = fields[index];
                nDescId = field.getAttribute("descid");
            } catch (ex) {
                continue;
            }

            // On cherche l adrId
            if (adrRecord.adrId == 0 && getTabDescid(nDescId) == 400) {
                var newRecords = field.getElementsByTagName("record");
                if (newRecords != null && newRecords.length != 0) {
                    adrRecord.adrId = newRecords[0].getAttribute("fid");
                }
            }

            // Adresse perso 
            if (nDescId == "401") {
                var newRecords = field.getElementsByTagName("record");
                if (newRecords != null && newRecords.length != 0) {
                    adrRecord.adrLab = eTools.getInnerHtml(newRecords[0]);
                    if (adrRecord.pmId > 0 || (adrRecord.adrId > 0 && adrRecord.adrLab.length > 0))
                        break;
                }
            }
            // Adresse pro 
            if (nDescId == "301") {
                var newRecords = field.getElementsByTagName("record");
                if (newRecords != null && newRecords.length != 0) {
                    adrRecord.pmId = newRecords[0].getAttribute("fid");
                    adrRecord.pmLab = eTools.getInnerHtml(newRecords[0]);
                    if (adrRecord.adrId > 0)
                        break;
                }
            }
        }

        if (typeof (adrRecord) != 'undefined' && adrRecord != null)
            oCreatedRecord.adr = {
                'adrId': adrRecord.adrId,
                'adr01': adrRecord.adrLab,
                'pmId': adrRecord.pmId,
                'pm01': adrRecord.pmLab
            };

    } catch (exp) {
        //eTools.log.inspect({'function' : 'AppendCreatedAdrRecord', "Exception": exp });
    }
}

function ErrorImageUploadReturn(oRes, engineObject) {
    var currentView = getCurrentView();
    var updaterView = currentView;
    // View de la fenêtre initiatrice de la demande de MAJ ou de création. (en fonction de la iframe)
    if (engineObject.ModalDialog != null) {
        currentView = getCurrentView(engineObject.ModalDialog.modFile.document);
    }

    if (currentView != "CALENDAR")
        top.setWait(false);

    if (!engineObject && typeof (engineObject) != 'undefined')
        return;

    if (engineObject.ErrorCallbackFunction != null && typeof (engineObject.ErrorCallbackFunction) == 'function') {
        engineObject.ErrorCallbackFunction();
    }

    engineObject.Clear();
}

function ImageUploadReturn(oRes, engineObject, afterUpload) {
    if (!engineObject) {
        alert('eEngine.ImageUploadReturn - Echec');
        return;
    }

    if (afterUpload != null && typeof (afterUpload) == 'function') {
        afterUpload();
    }

    engineObject.Clear();
}

function LoadFileAfterCreation(record, owner) {
    // FILE_MODIFICATION : 3
    if (record.tab == 101000) {
        nsAdmin.loadAdminModule(USROPT_MODULE_ADMIN_TAB_USER, null, { tab: 101000, userid: record.fid });
    } else if (typeof (owner) != "undefined" && owner != null) {
        owner.loadFile(record.tab, record.fid, 3);
        // owner.hide();
    }

    else
        loadFile(record.tab, record.fid, 3);
}
//
function LoadFileAfterErrorCreation(record, ownerModal, parentView) {


    var nTab = record.getElementsByTagName("tab")[0];
    if (nTab == null) return;
    nTab = getXmlTextNode(nTab);
    var nFileId = record.getElementsByTagName("fid")[0];
    if (nFileId == null) return;
    nFileId = getXmlTextNode(nFileId);
    bMaintTab = getAttributeValue(document.getElementById("mt_" + nGlobalActiveTab), "edntyp") == "0";
    //SI depuis liste de FICHIER PRINCIPAL : on load la fiche en pleine page.
    if (getAttributeValue(document.getElementById("mt_" + nGlobalActiveTab), "edntyp") == "0") {
        loadFile(nTab, nFileId, 3);
    }
    else if (((parentView == "FILE_MODIFICATION") || (parentView == "FILE_CONSULTATION")) && nGlobalActiveTab == nTab) {
        //Si depuis fiche une autre diche du même onglet
        loadFile(nTab, nFileId, 3);

    }
    else {
        if (ownerModal) {
            //sinon (template) on ferme la fiche si cela a été demandé
            if (ownerModal.pupClose && ownerModal.oModFile && ownerModal.oModFile.hide) {
                ownerModal.oModFile.hide();
                if (ownerModal.bPlanning && ownerModal.docTop)
                    ownerModal.docTop.setWait(false); //En mode planning un setWait en trop ?
            }
            else {
                //si la fermeture de la fiche créée n'a pas été demandée on la met reload.
                ownerModal.modFile.loadFile(nTab, nFileId, 3);
            }
        }
        //Selon d'ou l'on vient on rafraichit de manière différente               
        switch (parentView) {
            case "LIST":
            case "CALENDAR_LIST":
                if (ownerModal.bPlanning) {
                    if (ownerModal.docTop)
                        ownerModal.docTop.ReloadList();
                }
                else {
                    //rechargement de la liste (seulement si fichier principal)
                    if (getAttributeValue(top.document.getElementById("mt_" + nGlobalActiveTab), "edntyp") != 0) {
                        ReloadList();
                    }
                }
                break;
            case "CALENDAR":
                if (ownerModal.docTop)
                    ownerModal.docTop.goTabList(nTab, 1);
                break;
            case "FILE_CONSULTATION":
            case "FILE_MODIFICATION":
                //rechargement de la fiche et des signets sources   => en cas d'erreur à la création nous n'avons pas le détail des champs et bkm à mettre à jours donc on refresh tout.
                top.RefreshFile();  //Reload la fiche
                top.RefreshBkm(nTab);    // rafraichissement de la bkm source
                break;
        }
    }

}

function LoadChgFld(xmlDoc, chgRend, modalFrame) {
    if (chgRend == null || typeof (chgRend) == 'undefined')
        chgRend = true;

    if (typeof (modalFrame) == 'undefined')
        modalFrame = null;
    fieldRefresh.Tab = 0
    fieldRefresh.xmlDoc = xmlDoc;
    fieldRefresh.refreshFields();
    if (chgRend)
        fieldRefresh.chgRend(modalFrame);
}

function LoadChgFldHead(xmlDoc, chgRend, modalFrame, nTab) {

    if (chgRend == null || typeof (chgRend) == 'undefined')
        chgRend = true;

    if (typeof (modalFrame) == 'undefined')
        modalFrame = null;

    fieldRefresh.xmlDoc = xmlDoc;
    fieldRefresh.Tab = nTab
    fieldRefresh.refreshFields();
    if (chgRend)
        fieldRefresh.chgRend(modalFrame);
}

function ReloadMru(xmlDoc) {
    // Pour l'actualisation des mru par onglet - suppression
    var oMruTab = xmlDoc.getElementsByTagName("mrutab")[0];
    if (oMruTab != null && oMruTab.length != 0) {
        var oeParam = getParamWindow();
        if (oeParam.RefreshMRU)
            oeParam.RefreshMRU(getXmlTextNode(oMruTab));
    }

    // Pour l'actualisation des mru par rubrique - modif, crea
    var oMrus = xmlDoc.getElementsByTagName("mrus")[0];
    if (oMrus == null || oMrus.length == 0) return;

    var lstMru = oMrus.getElementsByTagName("mru");
    if (lstMru == null || lstMru.length == 0) return;

    var oeParam = getParamWindow();
    if (oeParam.SetMruParam) {
        forEach(lstMru, function (mru) {
            var descId = mru.getAttribute('descid');
            if (descId == null || typeof (descId) == 'undefined')
                return;

            var mruValue = getXmlTextNode(mru);
            oeParam.SetMruParam(descId, mruValue);

            //Refresh des MRU pour le cas des tables
            if (descId && descId.length > 2 && descId.substring(descId.length - 2, descId.length) == "00")
                top.LoadMru(descId);
        });
    }
}

function ReloadBkm(xmlDoc, oRecord) {
    var oBookmarks = xmlDoc.getElementsByTagName("bookmarks")[0];

    if (oBookmarks == null && typeof (oRecord) == "undefined") return;

    var lstBkm = getXmlTextNode(oBookmarks);
    if (typeof (oRecord) != "undefined") {
        if (isBkmFile(oRecord.tab) && !isIris(top.getTabFrom())) {
            top.loadBkm(oRecord.tab, 1, false, false, oRecord.fid);
            if ((";" + lstBkm + ";").indexOf(";" + oRecord.tab + ";") >= 0) {
                lstBkm = (";" + lstBkm + ";").replace(";" + oRecord.tab + ";", ";");
                if (lstBkm == ";")
                    lstBkm = "";
                else
                    lstBkm = lstBkm.substring(1, lstBkm.length - 1);
            }
        }
        else if ((";" + lstBkm + ";").indexOf(";" + oRecord.tab + ";") == -1) {
            if (lstBkm.length != 0)
                lstBkm += ";";
            lstBkm += oRecord.tab;
        }
    }

    if (lstBkm.length == 0) return;

    lstBkm = lstBkm.split(';');



    if (typeof (top.RefreshBkm) != 'function')
        return;

    forEach(lstBkm, function (bkm) {
        top.RefreshBkm(bkm);
    });

    return lstBkm
}

function ShowProcMessage(xmlDoc) {

    var oMsgs = xmlDoc.getElementsByTagName("procmessages")[0];
    if (oMsgs == null || oMsgs.length == 0) return;

    var lstMsg = oMsgs.getElementsByTagName("msg");
    if (lstMsg == null || lstMsg.length == 0) return;

    forEach(lstMsg, function (msg) {
        var msgTitle = getXmlTextNode(oMsgs.getElementsByTagName("title")[0]);
        var msgDescription = getXmlTextNode(oMsgs.getElementsByTagName("desc")[0]);
        var msgDetail = getXmlTextNode(oMsgs.getElementsByTagName("detail")[0]);

        top.eAlert(3, msgTitle, msgDescription, msgDetail);
    });
}



///Lancement des spécifs
function ShowProcPage(xmlDoc) {

    //Specif V7
    var oPages = xmlDoc.getElementsByTagName("procpages")[0];
    if (oPages == null || oPages.length == 0) return;

    var lstUrl = oPages.getElementsByTagName("url");
    if (!(lstUrl == null || lstUrl.length == 0)) {

        forEach(lstUrl, function (url) {
            exportToLinkToV7(getXmlTextNode(url), null, 3);
        });
    }

    //Specif XRM
    var lstUrlXRM = oPages.getElementsByTagName("urlxrm");
    if (!(lstUrlXRM == null || lstUrlXRM.length == 0)) {

        forEach(lstUrlXRM, function (url) {

            var nSpecId;
            var nTab;
            var nFileId;
            var nFieldDescId;


            var sURL = getXmlTextNode(url)
            var aURL = sURL.split('$|$');

            if (aURL.length == 5) {

                nSpecId = aURL[0];
                nTab = aURL[1];
                nFileId = aURL[2];
                nFieldDescId = aURL[3];

                top.runSpec(nSpecId, nTab, nFileId, nFieldDescId);
            }

        });
    }


}

fieldRefresh = {
    // Retour XML à interprêter
    xmlDoc: null,
    // Dictionnaire des records
    dicRecords: new Array(),
    // Information si l'on a chargé des nouvelles valeurs de rubriques depuis le flux XML
    dicRecordHasValues: false,
    // Noeud HTML de la VCard
    oNodeVCard: null,

    Tab: 0,

    // Raffraichi les fields de la fiche / liste
    refreshFld: false,
    // Raffraichi les fields des signets
    refreshFldBkm: false,
    // Raffraichi les fields de la popup
    refreshFldPopup: false,

    //Raffraichi les champs d'entête
    refreshFldHead: false,

    initRefresh: function () {
        this.refreshFld = false;
        this.refreshFldBkm = false;
        this.refreshFldPopup = false;
        this.refreshFldHead = false;
    },

    // Moteur du raffraichissement du rendu par les nouvelles valeurs.
    // Etape 1 : contruction du dictionnaire avec l'association entre le DESCID_FILEID et la nouvelle valeur
    refreshFields: function () {
        var oRefreshValue = null;

        this.dicRecords = new Array();
        this.dicRecordHasValues = false;

        // VCard de la fiche contact
        this.oNodeVCard = document.getElementById("vCardUl");
        if (this.oNodeVCard != null && this.oNodeVCard.tagName.toLowerCase() != "ul")
            this.oNodeVCard = null;

        var oFields = this.xmlDoc.getElementsByTagName("fields")[0];
        if (oFields == null || oFields.length == 0) return;

        var lstField = oFields.getElementsByTagName("field");
        if (lstField == null || lstField.length == 0) return;

        // Alimente le tableau associatif avec les informations du XML

        Array.prototype.slice.apply(lstField).forEach(function (oField) {
            if (this.Tab > 0) {
                var tabfield = getAttributeValue(oField, "descid") - getAttributeValue(oField, "descid") % 100
                if (tabfield == this.Tab)
                    fieldRefresh.refreshField(oField);
            }
            else
                fieldRefresh.refreshField(oField);
        }, this);
    },

    // Etape 2 : mise à jour du rendu + la vCard sur une fiche PP
    chgRend: function (modalFrame) {
        if (!this.dicRecordHasValues) return;

        // Modification de la valeur en rendu
        this.chgValue(modalFrame);
        // Modification des valeurs de la VCard de PP
        // Obsolete, il n'existe plus de VCARD en mode fiche consultation de PP
        //this.chgVCard();
    },

    // Modification des différentes valeurs modifiées sur le rendu
    chgValue: function (modalFrame) {

        /* 
         * ELAIZ - demande 80083 - Modification fiche signet
         * On vérifie que la modification des valeurs a été faite sur le nouveau mode fiche
           On récupère ainsi dans top le contexte de Vue et on on met à jour les data avec les données 
           dans dicRecords uniquement si la rubrique est présente dans l'onglet actuel en vérifiant le 
           descid et le fileId
           On vide par la suite top._IrisUpdateValFromBkmModal pour éviter de retomber dans ce cas à d'autre moment
         */

        if (top._IrisUpdateValFromBkmModal) {
            Object.keys(this.dicRecords).forEach(function (key) {
                var ids = key.split('_');

                var fileDataIris = top._IrisUpdateValFromBkmModal.$root
                    ?.$children?.find(n => n.$options.name == "App")
                    ?.$children?.find(n => n.$options.name == "v-app")
                    ?.$children?.find(n => n.$options.name == "v-main")
                    ?.$children?.find(n => n.$options.name)?.DataStruct.Structure.LstStructFields.find(function (x) {
                    return x.DescId == ids[0] && x.FileId == ids[1];
                });

                if (fileDataIris != undefined) fileDataIris.Value = this.dicRecords[key].innerHTML;
            }.bind(this));
            top._IrisUpdateValFromBkmModal = null;
        }


        if (getCurrentView(document) == "KANBAN") {

            if (oWidgetKanban) {

                if (this.refreshFld || this.refreshFldHead || this.refreshFldBkm) {

                    for (var arrKey in this.dicRecords) {
                        oWidgetKanban.editField(this.convertRefreshValToUpdEngFld(this.dicRecords[arrKey]));
                    }

                }
            }
        }
        else {
            // version 2
            var cells;
            var results = new Array();
            if (this.refreshFld || this.refreshFldHead || this.refreshFldBkm) {
                // Ajoute du DIV pour les rubriques mail

                // Modification pour widget liste : pourquoi "top" ? A re-modifier si régression
                // SPH : Il y a une regression : #63929 . Cette partie sert également a mettre à jour la liste 
                // en arriètre plan depuis une modale ce qui nécessite untop.
                cells = top.document.querySelectorAll("td[ename],input[ename],div[ename]");

                //ALISTER => Demande / Request #75979
                //Lorsque l'on modifie dans un widget de type liste, la valeur ne se modifie pas, mais
                //à l'aide de cette condition, la modification s'effectue à nouveau car je vérifie si il n'y a pas de nom,
                //ou de valeur. /
                //When we modify a type list widget, the value doesn't modify, but with this condition
                //the modification occurs when we check by length and name if we havn't name or value.
                if (cells.length <= 0)
                    cells = document.querySelectorAll("td[ename],input[ename],div[ename]");

                for (var i = 0; i < cells.length; i++)
                    results.push(cells[i]);
            }

            if (this.refreshFldPopup && modalFrame != null) {
                // Ajoute du DIV pour les rubriques mail
                cells = modalFrame.document.querySelectorAll("td[ename],input[ename],div[ename]");
                for (var i = 0; i < cells.length; i++)
                    results.push(cells[i]);
            }

            cells = results;
            // Parcours des TD
            for (var cellIdx = 0; cellIdx < cells.length; cellIdx++) {
                var oTd = cells[cellIdx];

                // Recup du DIV depuis une cellule d'édition de mail depuis une liste
                if (oTd.id.indexOf("COL_") != 0) {
                    var oChildren = oTd.children;
                    if (oChildren.length != 0) {
                        if (oChildren[0].tagName == 'DIV' && oChildren[0].id.indexOf("COL_") == 0)
                            oTd = oChildren[0];
                    }
                }

                // Refresh du champ notes (94), mode signet #36236
                if (oTd.id.indexOf("eBkmMemoEditorContainer_") == 0) {
                    var name = getAttributeValue(oTd, 'ename');
                    var fileId = getAttributeValue(oTd, 'fid');
                    var descid = name.split("_")[1];
                    var key = descid + "_" + fileId;
                    if (key in this.dicRecords)
                        editInnerField(oTd, this.convertRefreshValToUpdEngFld(this.dicRecords[key]));

                    continue;
                }

                if (oTd.id.indexOf("COL_") != 0)
                    continue;

                var cellName = oTd.getAttribute('ename');
                var cellFileId = GetFieldFileId(oTd.id);

                if (cellName == null || typeof (cellName) == 'undefined' || cellFileId == null || typeof (cellFileId) == 'undefined' || cellName.indexOf("_") == -1)
                    continue;

                var arrayTmp = cellName.split('_');
                var cellFieldDescid = arrayTmp[arrayTmp.length - 1];
                var arrKey = cellFieldDescid + "_" + cellFileId;

                if (arrKey in this.dicRecords) {
                    oRefreshValue = this.dicRecords[arrKey];
                    editInnerField(oTd, this.convertRefreshValToUpdEngFld(oRefreshValue));

                    //Si le champ a une colonnes calculée, relance le calcul
                    var colName = cellName.replace("COL_", "IMG_SUM_COLS_");
                    var oSumCol = document.getElementById(colName);
                    if (oSumCol && oSumCol.getAttribute("actif") == "1") {
                        docc(oSumCol, true);
                    }
                }
            }
        }


    },

    // Modification des différentes valeurs modifiées sur le rendu vCard de PP
    // Obsolete, il n'existe plus de VCARD en mode fiche consultation de PP
    chgVCard: function () {
        var oNode = this.oNodeVCard;

        if (oNode == null)
            return;

        var oSpan = null;
        var oRefreshValue = null;
        var oLstSpan = getArrayFromTag(oNode, 'SPAN');

        for (var idx = 0; idx < oLstSpan.length; idx++) {
            oSpan = oLstSpan[idx];

            var valDescid = oSpan.getAttribute('did');
            var valFid = GetFieldFileId(oSpan.id);
            if (valDescid == null || typeof (valDescid) == 'undefined' || valFid == null || typeof (valFid) == 'undefined')
                continue;

            var arrKey = valDescid + "_" + valFid;
            if (arrKey in this.dicRecords) {
                oRefreshValue = this.dicRecords[arrKey];
                editInnerField(oSpan, this.convertRefreshValToUpdEngFld(oRefreshValue));
            }
        }
    },

    // Fonction outil pour reprendre les valeurs d'un refreshvalue dans un fieldUpdateEngine
    convertRefreshValToUpdEngFld: function (oRefreshValue) {
        var oRefreshField = this.findUp(oRefreshValue, "field");

        if (oRefreshField) {
            var fld = new fldUpdEngine(oRefreshField.getAttribute('descid'));
            fld.format = oRefreshField.getAttribute('format');
            fld.newLabel = getXmlTextNode(oRefreshValue);
            fld.newValue = oRefreshValue.getAttribute("dbv");
            fld.boundValue = oRefreshValue.getAttribute("pdbv");
            fld.fid = oRefreshValue.getAttribute("fid");

            fld.isLink = (getAttributeValue(oRefreshValue.parentNode.parentNode, "isLink") === "1");

            return fld;
        }

        return null;
    },

    // Parcours des fields du XML pour l'alimentation du dictionnaire
    refreshField: function (oField) {

        var records = oField.getElementsByTagName("record");

        forEach(records, function (oRecord) {
            fieldRefresh.refreshRecord(oField, oRecord);
        });
    },

    // Parcours des records du XML pour l'alimentation du dictionnaire
    refreshRecord: function (oField, oRecord) {
        var fieldDescId = oField.getAttribute('descid');
        var recordFileId = oRecord.getAttribute('fid');

        if (typeof (fieldDescId) == 'undefined' || typeof (recordFileId) == 'undefined')
            return;

        this.addRecord(fieldDescId + '_' + recordFileId, oRecord);
    },

    addRecord: function (key, oValue) {
        this.dicRecordHasValues = true;
        this.dicRecords[key] = oValue;
    },

    findUp: function (elt, tag) {
        if (elt == null)
            return null;

        do {
            if (elt.nodeName && elt.nodeName.search(tag) != -1)
                return elt;
        } while (elt = elt.parentNode);

        return null;
    }
}



// Fonction outil pour la modif du inner d'un noeud HTML en fonction de la nouvelle valeur du field
function editInnerField(oNode, oField) {
    var nodeAction = getAttributeValue(oNode, 'eaction');

    //BSE: #52 292 si une rubrique de type link, on ajoute les attributs pour que le lien soit cliquable
    //HLA: #52 292 j'ai repris et adapté le code de BSE venant de la fonction chgValue pour la reprendre ici, méthode plus globale
    if (oField.isLink) {
        if (nodeAction == "" && getAttributeValue(oRefreshValue, "dbv") != "") {
            oNode.setAttribute("efld", 1);
            // Si la rubrique n'est pas editable alors le eaction d'une islink est vide
            oNode.setAttribute("eaction", "LNKGOFILE");

            nodeAction = getAttributeValue(oNode, 'eaction');
        }
        else if (nodeAction == "LNKGOFILE" && getAttributeValue(oRefreshValue, "dbv") == "") {

            oNode.setAttribute("efld", 1);
            // Si la rubrique n'est pas editable alors le eaction d'une islink est vide
            oNode.setAttribute("eaction", "LNKCATFILE");

            removeClass(oNode, "gofile");
            removeClass(oNode, "LNKGOFILE");
            addClass(oNode, "LNKCATFILE");
            addClass(oNode, "edit")
            addClass(oNode, "readonly")

            nodeAction = getAttributeValue(oNode, 'eaction');
        }
    }


    if ((oField.format == "6" || oField.format == "7" || oField.format == "12" || oField.format == "27")) {

        var btn = document.querySelector("[eacttg='" + oNode.id + "']")

        if (btn) {
            var IsOff = getAttributeValue(oRefreshValue, "dbv") == "";
            var sAction = "";
            var sIconButton = "";
            switch (oField.format) {
                //Mail
                case "6":
                    sAction = "LNKSENDMAIL";
                    sIconButton = "icon-email icnFileBtn";
                    break;
                //Web
                case "7":
                    sAction = "LNKOPENWEB";
                    sIconButton = "icon-site_web icnFileBtn";
                    break;
                //Phone, ALISTER => Demande 82 928
                case "12":
                    sAction = "LNKSENDPHONE";
                    sIconButton = "icon-sms icnFileBtn";
                    break;
                //Social NetWork
                case "27":
                    sAction = "LNKOPENSOCNET";
                    sIconButton = "icon-site_web icnFileBtn";
                    break;
            }

            //ELAIZ - demande 78 183 - remplacement de display none par opacity car sinon il y a un décalage entre les rubriques en mode fiche
            btn.style.opacity = IsOff ? 0 : 1;
            setAttributeValue(btn, "eaction", IsOff ? "" : sAction)
            //btn.className = IsOff ? "" : sIconButton;

        }

    }

    // Détermine si c'est un type bit, il faut cocher la case
    if (oField.format == "3" || nodeAction == "LNKCHECK") {
        if (oNode.children.length > 0) {
            var oImg = oNode.children[0];
            if (oImg != null && oImg.tagName == "A")
                chgChk(oImg, oField.newValue == "1");
        }

        return;
    }
    // Détermine si c'est un type bouton, il faut cocher la case
    else if (oField.format == "25" || nodeAction == "LNKBITBUTTON") {
        if (oNode.children.length > 0) {
            var oBtnLink = oNode.children[0];
            if (oBtnLink != null && oBtnLink.tagName == "A")
                changeBtnBit(oBtnLink, oField.newValue == "1");
        }

        return;
    }
    else if (nodeAction == "LNKSTEPCAT") {
        selectStepDbv(oField.newValue, oNode, true);
    }

    //GCH - #35859 - Internationnalisation Date - Fiche
    var bDate = (oField.format == FLDTYP.DATE);
    //GCH - #36022 - Internationalisation Numerique - Fiche
    var bNumerique = isFormatNumeric(oField.format);
    // Cas particulier - mise à jour de l'en-tête de la fiche
    var nodeName = oNode.getAttribute('ename');
    if (nodeName != null && typeof (nodeName) != 'undefined') {
        var nodeHeaderFile = document.getElementById('fileName_' + nodeName.replace('COL_', ''));

        if (nodeHeaderFile != null && typeof (nodeHeaderFile) != 'undefined')
            editInnerField(nodeHeaderFile, oField);
    }

    // Cas particulier pour le PP01 en mode fiche consultation
    var cellInnerHtml = oField.newLabel;
    if (oField.descId == "201" && oNode.getAttribute('nameonly') != null)     // Rubrique PP.nom
        cellInnerHtml = oField.newValue;

    // #40 182 : Cas particulier de la date : il faut prendre la valeur issue de la base (dd/MM/yyyy) et non la valeur d'affichage,
    // pour être sûr du format utilisé en vue, justement, de convertir la date pour l'affichage.
    // La valeur en base pouvant contenir l'heure en plus de la date, on récupère une partie de la valeur correspondant à la longueur
    // de la valeur d'affichage. Exemple :
    // - valeur d'affichage en MM/DD/YYYY = 08/12/2015 (12 août 2015)
    // - valeur en base : 12/08/2015 00:00:00
    // => valeur retenue : substring de "12/08/2015 00:00:00" de 0 jusqu'à "08/12/2015".length caractères = "12/08/2015"
    // Valeur qui sera ensuite reconvertie au format d'affichage de la base par eDate.ConvertBddToDisplay() via editInnerField() plus bas
    // 16/09/2015 ALE/CRU/SPH
    // la conversion est déjà faite dans le retour de l'eupdater, cette valeur va donc etre utilisé directement plutot qu'être reconverti
    // la valeur de oField.newLabel est déjà convertie suivant l'internationalisation, d'ou une double conversion.
    if (bDate) {
        //cellInnerHtml = oField.newValue.substring(0, oField.newLabel.length);
        cellInnerHtml = oField.newLabel;
    }
    else if (bNumerique && oField.descId % 100 != 83) {
        //La valeur numérique est reconvertie en js ci-dessous depuis la valeur en bdd
        // la valeur de oField.newLabel est déjà convertie suivant l'internationalisation, d'ou une double conversion.
        cellInnerHtml = oField.newLabel;
    }

    //SHA : demande #77 629
    if (oField.descId % 100 == 83 && oField.format == 10) // Type agenda => dans ce cas, le input est un champ caché dont "value", et pas dbv contient la vrai valeur. TODO : refaire le planning pour uniformiser
        oNode.tagName == 'INPUT' ? cellInnerHtml = oField.newValue : cellInnerHtml = oField.newLabel;

    // Cas particulier pour la rubrique de type Mémo en mode HTML (iFrame incrustée dans la cellule de tableau)
    var bUpdateIFrame = false;
    var bUpdateInnerHTML = true;
    if (oField.format == "9" || nodeAction == "LNKOPENMEMO") {

        // Cas d'une rubrique MEMO - Voir méthode getFldEngFromElt de eMain.js
        var bMemo = false;
        if (oNode.firstChild) {
            bMemo = oNode.firstChild.tagName == "TEXTAREA";
            bMemo = bMemo || (oNode.firstChild.tagName == "DIV" && oNode.firstChild.id.indexOf("eMEG_") == 0);
        }

        // Cas du mode Fiche : on ne remplace pas à jour le contenu intérieur de la cellule, si elle contient le champ Mémo, par la valeur mise à jour.
        // Par contre, s'il n'y a que du texte (cas du mode Liste), on laissera la mise à jour se faire ci-dessous
        if (bMemo) {
            bUpdateInnerHTML = false;

            var oTextArea = findElementByTagName(oNode.children, "TEXTAREA");
            if (oTextArea != null) {
                // Il faut recupérer la dbv pour conserver toute la valeur et non les 1000 premiers caractères
                cellInnerHtml = oField.newValue;

                // CRU - 03/09/2015 - Bug 40793
                if (cellInnerHtml == null) cellInnerHtml = "";

                // Bug #36236
                // Dans le cas d'un memo HTML ou non HTML, nous avons le textarea ou/et le div à mettre à jour
                // le div est mis à jour via le mémoEditor  
                try {
                    // Mise à jour du champ mémo
                    var memo = null;
                    if (oField.descId in aBkmMemoEditors) {
                        // MODE SIGNET : Récupère le champ note du signet à mettre à jour 
                        memo = aBkmMemoEditors[oField.descId];
                    } else {
                        // MODE FICHE : Récupère le champ mémo de la fiche  à mettre à jour
                        var key = "edt" + oNode.id;
                        if (key in top["_medt"]) {
                            memo = top["_medt"][key];
                        }
                        // Set du textarea
                        setInnerNode(oTextArea, cellInnerHtml);
                    }

                    if (memo != null)
                        memo.setData(cellInnerHtml, function () {/*TODO : log par exemple*/ });

                } catch (ex) {/*log*/ }
            }
        }
    }
    else if (oField.format == "13") {
        //gestion champ image
        bUpdateInnerHTML = false;

        if (oField.newLabel == null || oField.newLabel == '') {
            //Construction empty div
            var divEmptyImg = document.createElement("div");
            while (oNode.firstChild) {
                oNode.removeChild(oNode.lastChild);
            }
            oNode.appendChild(divEmptyImg);

        }
        else {
            var divImg = document.createElement("img");
            divImg.src = oField.newLabel;



            divImg.style = 'border-width:0px;max-height:100%;max-width:100%;'

            setAttributeValue(divImg, "fid", oField.fid);
            setAttributeValue(divImg, "tab", oField.descId - oField.descId % 100);
            setAttributeValue(divImg, "onerror", "onErrorImg(this);");


            while (oNode.firstChild) {
                oNode.removeChild(oNode.lastChild);
            }
            oNode.appendChild(divImg);


        }
    }
    else if (nodeAction == "LNKSTEPCAT") {
        bUpdateInnerHTML = false;
    }

    if (!bUpdateIFrame) {
        if (oNode.tagName == 'INPUT') {
            // Mise à jour des champs de saisie simples type INPUT
            if (bDate) {
                oNode.value = (cellInnerHtml);
                //   oNode.value = eDate.ConvertBddToDisplay(cellInnerHtml);
            }
            else if (bNumerique) {
                // bug internationalisation des dates
                //cf plus haut. La valeur initailee de cellInnerHtml était oField.newLabel qui était déjà transformé pour l'internationalisation
                // il faut soit utiliser la oField.newValue et convertir, soit utiliser newLabel et ne pas reconvertir.


                oNode.value = (cellInnerHtml);
                // oNode.value = eNumber.ConvertBddToDisplayFull(oField.newValue);
            }
            else
                oNode.value = cellInnerHtml;

            // CRU - 03/09/2015 - Bug 40793
            if (cellInnerHtml == null) cellInnerHtml = "";

            oNode.setAttribute('value', cellInnerHtml);
            //kha le 06/06/2014 : si le champ est obligatoire et rempli, on retire le liseré rouge.
            if (cellInnerHtml != "")
                removeClass(oNode.parentElement, "mandatory");

        }
        else if (bUpdateInnerHTML) {
            setInnerNode(oNode, cellInnerHtml);
        }
    }

    //ALISTER => Demande 82 928
    if (oField.format == "12" || nodeAction == "LNKSENDPHONE" || nodeAction == "LNKPHONE") {
        oNode.setAttribute("title", cellInnerHtml);
    }

    if (nodeAction == "LNKGOFILE" || nodeAction == "LNKCATFILE") {
        if (oField.newValue != "" && oField.newValue != "-1") {
            oNode.setAttribute("eaction", "LNKGOFILE");
            switchClass(oNode, "LNKCATFILE", "LNKGOFILE gofile");
        }

        oNode.setAttribute("title", cellInnerHtml);
        oNode.setAttribute("dbv", oField.newValue);

        if (oField.descId % 100 == 1) {

            if (typeof oField.fid != "undefined")
                oNode.setAttribute("lnkid", oField.fid);
            else
                oNode.setAttribute("lnkid", "");
        }
        else {
            oNode.setAttribute("lnkid", oField.newValue);
        }
    }
    else if (oField.newValue != null && oNode.getAttribute("dbv") != null) {
        oNode.setAttribute("dbv", oField.newValue);
    }


    if (oField.boundValue != null && oNode.getAttribute("pdbv") != null) {
        oNode.setAttribute("pdbv", oField.boundValue);
    }
    if (!oField.readOnly) {
        var oInputFields = document.getElementById("fieldsId_" + getTabDescid(oField.descId));
        if (oInputFields != null && (';' + oInputFields.value + ';').indexOf(';' + oField.descId + ';') == -1)
            oInputFields.value += ";" + oField.descId;
    }
}

function setInnerNode(oNode, fldNewValue) {
    // Mise à jour de la cellule source du mode Liste (dont champs Mémo) ou de l'élément s'il ne s'agit pas d'un INPUT
    var isHtml = false;
    if (oNode.getAttribute("html")) {
        isHtml = (oNode.getAttribute("html") == "1") ? true : false;
    }
    if (isHtml)
        oNode.innerHTML = fldNewValue;
    else
        oNode.textContent = fldNewValue;
}