import { JSONTryParse } from "./eMainMethods.js?ver=803000"
import { linkToCall, linkToPost/*, linkToPostWHeader*/ } from "./eFileMethods.js?ver=803000"
import { loadFileUploadFile } from "../shared/XRMWrapperModules.js?ver=803000";
import EventBus from "../bus/event-bus.js?ver=803000";


/**
 * Affiche la progressbar.
 * @param {any} dvRoot l'élément racine qui va servir à accrocher la progressbar.
 */
async function activateProgressBar(dvRoot, myFiles) {

    let { default: eProgressBar } = await import(AddUrlTimeStampJS("../components/subComponents/eProgressBar.js"));
    let progress = Vue.extend(eProgressBar);

    let instance = new progress({
        propsData: { "file": myFiles },
        store: this.$store,
    });

    instance.$on('callBackfinishLoad', () => {
        this.onLoad = false;
        this.blocked = false;

        dvRoot.removeChild(instance.$el);
        this.progressBarActivated = false;
    });

    instance.$mount();
    dvRoot.appendChild(instance.$el);
    this.progressBarActivated = true;
}


/**
 * Affiche la progressbar.
 * @param {any} dvRoot l'élément racine qui va servir à accrocher la progressbar.
 */
async function activateProgressBarv2(dvRoot, data) {

    let { default: eProgressBar } = await import(AddUrlTimeStampJS("../components/subComponents/eProgressBarv2.js"));
    let progress = Vue.extend(eProgressBar);

    let instance = new progress({
        propsData: data,
        store: this.$store,
    });

    instance.$on('callBackfinishLoad', () => {
        this.onLoad = false;
        this.blocked = false;

        dvRoot.removeChild(instance.$el);
    });

    instance.$mount();
    dvRoot.appendChild(instance.$el);
}

/**
 * Si... euh... un ... truc ?...
 * @param {any} element
 */
function isElementFirstChildEmptyPictureArea(element) {
    return element.firstChild
        && element.firstChild.tagName == "DIV"
        && element.firstChild.hasAttribute("data-eEmptyPictureArea")
        && element.firstChild.getAttribute("data-eEmptyPictureArea") == "1";
}

/**
 * Est-ce qu'un des éléments qu'on cherche à déposer serait par hasard
 * du type fichier ?
 * @param {any} event
 */
function containsFiles(event) {
    return event.dataTransfer.types
        .some(typ => typ == "Files");
}

function cancelNewPjName(modChgPjName) {
    this.onLoad = false;
    this.blocked = false;

    modChgPjName.hide();
}

/**
 * Après avoir interrogé le back, on effectue le traitement, 
 * si une pièce jointe existe ou non.
 * @param {any} oRes
 * @param {any} onSuccessFct
 * @param {any} onValidNewNameFct
 * @param {any} oFilesInfos
 */
async function checkPjExists(oRes, onSuccessFct, onValidNewNameFct, oFilesInfos) {

    // Si les PJ n'existent pas, on lance la fonction onSuccessFct
    if (oRes != null
        && oRes["Success"] == true) {
        if (typeof (onSuccessFct) == "function") {
            await onSuccessFct(oFilesInfos);
        } else
            launchAddPj();
    }

    // Si au moins une des PJ existe, on affiche la modale

    let oResUnsucess = [...oRes["CheckFile"]]
        .filter(jsF => jsF["Successfull"] == false);

    if (oResUnsucess.length > 0) {
        let oJson = oResUnsucess.map(jsF => {
            return {
                filename: jsF.RealName,
                saveAs: jsF.SuggestedName,
                action: 0
            }
        });

        let sTitle = oResUnsucess.map(n => n["WindowsTitle"]).find(n => n) || this.getRes(8693);
        let sDescription = oResUnsucess.map(n => n["WindowsDescription"]).find(n => n) || this.getRes(8693);

        var modalPJChecker = new eModalDialog(sTitle, 0, "mgr/eCheckPJExists.ashx", 600, 400, "modalPJChecker");
        modalPJChecker.addParam("action", "1", "post");
        modalPJChecker.addParam("files", JSON.stringify(oJson), "post");
        modalPJChecker.addParam("tab", this.getTab, "post");
        modalPJChecker.addParam("fileid", this.getFileId, "post");
        modalPJChecker.addParam("description", sDescription, "post");

        modalPJChecker.show();

        modalPJChecker.addButton(this.getRes(29), () => this.cancelNewPjName(modalPJChecker), "button-gray");
        modalPJChecker.addButton(this.getRes(28), () => validNewPjName(modalPJChecker, oFilesInfos, onSuccessFct, onValidNewNameFct), "button-green");

    }
}


/**
 * Demande de vérification que les PJ n'existent pas déjà
 * @param {any} oFilesInfos
 * @param {any} onSuccessFct
 * @param {any} onValidNewNameFct
 */
async function callCheckPjExists(oFilesInfos, onSuccessFct, onValidNewNameFct) {
    let oDataUploadFile = loadFileUploadFile();

    if (!(oDataUploadFile))
        return;

    let param = {
        nAction: 0,
        sFiles: JSON.stringify(oFilesInfos),
    };

    this.sendFilesJson = JSONTryParse(
        await linkToCall(
            oDataUploadFile.url,
            { ...oDataUploadFile.params, ...param }
        )
    );

    await this.checkPjExists(this.sendFilesJson, onSuccessFct, onValidNewNameFct, oFilesInfos);
}


/**
 * Si on passe sur la zone de drag'n'drop.
 * @param {any} oCall
 * @param {any} e
 */
function UpFilDragOver(oCall, e) {
    if (!e)
        return;
    if (!containsFiles(e))
        return;

    if (isElementFirstChildEmptyPictureArea(oCall))
        addClass(oCall.firstChild, 'PjHover');
    else
        addClass(oCall, 'PjHover');
}

/**
 * Si on quitte la zone de drag'n'drop.
 * @param {any} oCall
 */
function UpFilDragLeave(oCall) {

    if (isElementFirstChildEmptyPictureArea(oCall))
        removeClass(oCall.firstChild, 'PjHover');
    else
        removeClass(oCall, 'PjHover');
}

/**
 * Si on relache la bouton de la souris, avec des fichiers, et 
 * qu'on est sur la zone de drag'n'drop
 * @param {any} oCall
 * @param {any} e
 * @param {any} filesList
 * @param {any} oFilesInfo
 * @param {any} isImage
 */
async function UpFilDrop(oCall, e, filesList, oFilesInfo) {
    try {
        if (isElementFirstChildEmptyPictureArea(oCall))
            removeClass(oCall.firstChild, 'PjHover');
        else
            removeClass(oCall, 'PjHover');

        let mesfichiers;

        if (e && e.dataTransfer && e.dataTransfer.files)
            mesfichiers = e.dataTransfer.files;

        if (filesList)
            mesfichiers = filesList;

        if (!oFilesInfo)
            oFilesInfo = [...mesfichiers].map(fic => {
                return { filename: fic.name, saveas: fic.name }
            });


        let sendFileFct = (oFilesInfo) => this.SendFile(mesfichiers, oFilesInfo);
        let validFct = (oFilesInfo) => this.UpFilDrop(oCall, e, mesfichiers, oFilesInfo);

        await this.callCheckPjExists(oFilesInfo, sendFileFct, validFct);
    }
    catch (e) {
        // Si l'exception renvoyée comporte une requête avec des informations plus précises dans le corps de réponse, on les utilise, plutôt que de renvoyer une erreur 500 générique de xhr.js
        let newEx = e;
        try {
            // On vérifie si on a un corps de réponse exploitable, soit dans response, soit dans request
            if (typeof (e?.response?.data) == "object")
                newEx = e?.response?.data;
            else if (e?.request?.response != "")
                newEx = JSON.parse(e?.request?.response);
            // Conversion des propriétés d'une exception .NET (.ExceptionMessage ou .Message et .StackTrace) en propriétés d'exception JavaScript (e.message et e.stack)
            if (newEx.ExceptionMessage)
                newEx = {
                    message: newEx.ExceptionMessage,
                    stack: newEx.StackTrace
                };
            else
                newEx = {
                    message: newEx.Message,
                    stack: newEx.StackTrace
                };
        }
        // Si on a pas réussi à utiliser l'exception plus précise, on conserve celle d'origine
        catch (e2) {
            newEx = e;
        }

        EventBus.$emit("globalModal", {
            typeModal: "alert",
            color: "danger",
            type: "zoom",
            close: true,
            maximize: false,
            id: "alert-modal",
            title: this.getRes(72),
            msgData: newEx,
            width: 600,
            btns: [{ lib: this.getRes(30), color: "default", type: "left" }],
            datas: newEx.message,
        });
    }
}


/**
 * On envoie des fichiers.
 * @param {any} myFiles
 * @param {any} filesInfos
 */
async function SendFile(myFiles, filesInfos) {

    let uploadFileModel = new FormData();

    let nFileId = this.getFileId;
    let nTabId = this.getTab;


    if (nFileId < 0 || !nTabId || nTabId <= 0 || myFiles.length <= 0)  //tabid ne peut pas être vide ou <= à 0 et dileid n peut être < à 0
        return false;

    uploadFileModel.append("FileId", nFileId);
    uploadFileModel.append("Tab", nTabId);

    if (filesInfos) {
        var filesNameList = filesInfos
            .map(fic => fic.filename + " : " + fic.saveas)
            .join("|");

        uploadFileModel.append("SaveAs", filesNameList);
        filesInfos.forEach(function (fi, idx) {
            uploadFileModel.append(`UploadInfo[${idx}]`, JSON.stringify(fi));
        });
    }

    [...myFiles].forEach((fic, idx) => uploadFileModel.append(`fileCollection[${idx}]`, fic, fic.saveas));

    let oDataUploadFile = loadFileUploadFile();

    this.activateProgressBar(this.$refs["ProgressSpacecraft"], [...myFiles]);

    /** Si un jour on utilise le html5, on pourra utiliser cette fonction.
     * conjointement avec eProgressBarv2 (qui utilise porgress et output).
     * Tout est prévu. G.L */
    //this.activateProgressBarv2(this.$refs["ProgressSpacecraft"], {
    //    min: 0,
    //    max: 100,
    //    value: 0,
    //    label: this.getRes(6545),
    //});
    //let cptProgress = this.$refs["ProgressSpacecraft"]["pgEudo"]
    //let cptOutProgress = this.$refs["ProgressSpacecraft"]["pgStrongUploadValue"]
    //let config = {
    //    onUploadProgress: function (progressEvent) {
    //        let percentCompleted = Math.round((progressEvent.loaded * 100) / progressEvent.total);

    //        if (cptProgress)
    //          cptProgress.value = percentCompleted;
    //
    //        if(cptOutProgress)
    //           cptOutProgress.innerText = percentCompleted;
    //    }
    //}
    //try {
    //    this.sendFilesJson = JSONTryParse(await linkToPostWHeader(oDataUploadFile.url, uploadFileModel, config));
    //}
    //catch (e) {
    //    throw e;
    //}
    /*******************************************************************************************/

    try {
        // Envoi de la demande au contrôleur
        this.sendFilesJson = JSONTryParse(await linkToPost(oDataUploadFile.url, uploadFileModel));

        // Interception des retours d'erreur
        // Les erreurs du contrôleur pouvant être renvoyés en HTTP 200 (OK), on vérifie le retour pour s'assurer qu'il n'y ait pas eu d'erreur
        // Le contrôleur renvoie "" si l'ajout s'est bien passé. Sinon, il renverra un objet issu de EudoException avec plusieurs propriétés *Message
        if (typeof (this.sendFilesJson) == "object" && this.sendFilesJson.UserMessage != "") {
            // On renvoie de base le message utilisateur
            let exceptionMessage = this.sendFilesJson.UserMessage;
            // Puis on ajoute tous les messages supplémentaires renvoyés par le contrôleur.
            // Celui-ci ne renvoyant pas DebugMessage et StackTrace si le contexte ne le souhaite pas (= machine distante ou non Eudo), on les ajoute sans vérification (déjà faite côté Back)
            if (exceptionMessage.indexOf(this.sendFilesJson.Message) === -1)
                exceptionMessage += " - " + this.sendFilesJson.Message;
            if (exceptionMessage.indexOf(this.sendFilesJson.DebugMessage) === -1)
                exceptionMessage += " - " + this.sendFilesJson.DebugMessage;
            // Puis on renvoie (format attendu par globalModal.msgData)
            throw {
                message: exceptionMessage, stack: this.sendFilesJson.StackTrace
            };
        }
    }
    catch (e) {
        throw e;
    }
}

/**
 * Une popup de progression
 * */
function createProgress() {
    modalProgress = null;
    prog = 0;
    modalProgress = new eModalDialog(this.getRes(6545), 4, this.getRes(6546), 550, 160, "modalProgress");
    modalProgress.noButtons = true;
    modalProgress.show();
    return modalProgress;
}


export { UpFilDragOver, UpFilDragLeave, UpFilDrop, callCheckPjExists, checkPjExists, SendFile, activateProgressBar, activateProgressBarv2, cancelNewPjName };