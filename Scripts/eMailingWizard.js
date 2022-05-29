/// <reference path="eMailing.js" />
/// <reference path="eMailingTpl.js" />
/// <reference path="eModalDialog.js" />
var calpopup;
var oTemplate = new eMailingTemplate();


///Modèle utilisateur
var USER_MAIL_TPL = 0;

//Type de modèle de mail en base (emailing ou mail unitaire)
var MAILTEMPLATETYPE_EMAILING = 0;
var MAILTEMPLATETYPE_EMAIL = 1;
var MAILTEMPLATETYPE_SMSING = 2;

//Type du message
var OperationTplMail =
{
    NONE: 0,   //Aucune action
    ADD: 1, //Sauvegarder un nouveau Modele(Template)
    RENAME: 2,      //Renommer le Modele(Template)
    UPDATE: 3,       //Mettre le Modele(Template) à jour
    CLONE: 4,   //Dupliquer le Modele(Template)
    DELETE: 5,  //Supprimer le Modele(Template)
    DISPLAY: 6,  //Afficher le Modele(Template)
    LOAD: 7,   //Charge un modèle
    DIALOG_SAVEAS: 8, //Affichage de la fenêtre enregistrer sous
    SET_DEFAULT: 9,
    TOOLTIP: 10 //Affiche une infobulle du Modele(Template)
};

var _defaultTemplate = "";

///Initilisation de la liste des modeles de mail, ajustement de la taille des colonnes
function initMailTpl(bNotForEmailing, nTabBkm, nTab, nParentFileId) {
    // Récupération des objets oTemplate/oMailing sur les fenêtres parentes
    var oParentWindow = this;
    if (typeof (top._md['MailingWizard']) != "undefined")
        oParentWindow = top._md['MailingWizard'].getIframe();
    if (typeof (top._md['oModalMailTemplateList']) != "undefined")
        oParentWindow = top._md['oModalMailTemplateList'].getIframe();
    if (typeof (oMailing) == "undefined")
        oMailing = oParentWindow.oMailing;
    if (typeof (oTemplate) == "undefined")
        oTemplate = oParentWindow.oTemplate;

    // Au premier chargement de la fenêtre des modèles de mails unitaires, les objets ne sont pas forcément initialisés.
    // On les initialise alors ici (dans le cas des modèles d'emailing, cette initialisation est faite par eWizard.aspx)
    if (bNotForEmailing && (typeof (oMailing) == "undefined" || typeof (oTemplate) == "undefined")) {
        if (typeof (oMailing) == "undefined") {
            var oMailing = new eMailing(0, 0, TypeMailing.MAILING_UNDEFINED, nTabBkm, nTab, nParentFileId);
            top._md['oModalMailTemplateList'].getIframe().oMailing = oMailing;
            oMailing.SetSubjectEditor(top._md['oModalMailTemplateList']._oSubjectJSVarName);
            oMailing.SetMemoEditor(top._md['oModalMailTemplateList']._oMemoJSVarName);
        }
        if (typeof (oTemplate) == "undefined")
            var oTemplate = new eMailingTemplate();
    }

    if (typeof (oTemplate) != "undefined") {
        oTemplate.SetMailTemplateType(bNotForEmailing ? MAILTEMPLATETYPE_EMAIL : MAILTEMPLATETYPE_EMAILING);

        if (oTemplate.GetType() == USER_MAIL_TPL && (bNotForEmailing || oTemplate.GetTplId() > 0)) {
            var MailingTab = document.getElementById("mt_107000");
            oTrs = MailingTab.getElementsByTagName("tr");

            //On cherche l'element a selectionner
            for (var j = 0; j < oTrs.length; j++) {
                var obj = oTrs[j];
                if (obj.getAttribute("eid") == "107000_" + oTemplate.GetTplId()) {
                    selectMailTpl(obj);
                    break;
                }
            }

            //On ajuste la largeur de la table            
            adjustLastCol(107000);
        }
    }

    if (!ePopupObject)
        ePopupObject = new ePopup('ePopupObject', 220, 250, 0, 0, document.body, false);

    var oHdefault = document.getElementById("HidDefaultTplID")
    if (oHdefault)
        _defaultTemplate = oHdefault.value;

    //if (_defaultTemplate > 0) {
    //    if (oMailing)
    //        oMailing.LoadTemplate(null);
    //}
}


function selectMailingDate() {
    calpopup = createCalendarPopUp("ValidSelectMailingDate", "1", "0", top._res_5017, top._res_5003, "SelectMailingDateOk", top._res_29, "SelectMailingDateCancel", null, wizardIframe.id);
}

var modalDate = null;
var txt = null;
var param = null;

///Fonction appellant le manager de compteur d'emails à envoyer
function callUpdtCmptNbMail() {


    // Appel au compteur, le retour doit être le nombre de mail a envoyer (text brute)
    var oCmptMailsManager = new eUpdater("mgr/eCountOnDemandManager.ashx", 1);

    oCmptMailsManager.ErrorCallBack = function () {
        updtCmptNbMail(-1);
        setWait(false);
    }

    //oMailing._mailingId = 1;

    oCmptMailsManager.addParam("campaignid", oMailing._mailingId, "post");
    oCmptMailsManager.addParam("tab", oMailing._tab, "post");
    oCmptMailsManager.addParam("parenttabid", oMailing._nParentTabId, "post");
    oCmptMailsManager.addParam("parentfileid", oMailing._nParentFileId, "post");



    try {
        // Backlog #617, #619, #648 et #72 087 - Le CSS doit être récupéré de l'éditeur principal (grapesjs ou CKEditor si IE/Edge) comme pour le corps de mail
        // Celui-ci ayant été préalablement mis à jour à partir de l'éditeur secondaire (CKEditor) via majMainEditor()
        // On récupère donc la référence au mainEditor de la même façon que le fait majMainEditor()
        var mainEditor = oMailing._oMemoEditor;
        if (oMailing._oMemoEditor != null && typeof ("".endsWith) == "function" && oMailing._oMemoEditor.name.endsWith("_1")) {
            if (nsMain.getAllMemoEditorIDs().length > 1) {
                mainEditor = nsMain.getMemoEditor(0);
            }
        }
        var sbody = mainEditor.getData();

        var myBody = document.createElement("div");
        myBody.innerHTML = sbody;


        var mergefield = myBody.querySelectorAll("label[ednc='mergefield']");
        var aMergeField = [];
        if (mergefield.length > 0) {
            [].forEach.call(mergefield, function (elem) {

                var nMergeFld = getAttributeValue(elem, "ednd");
                nMergeFld = nMergeFld * 1;

                if (isNaN(nMergeFld))
                    return;

                //  nMergeFld = nMergeFld - nMergeFld % 100;

                if (nMergeFld == ""
                    || aMergeField.filter(function (id) { (id - id % 100) == (nMergeFld - nMergeFld % 100) }).length > 0
                    || (nMergeFld - nMergeFld % 100) == oMailing._tab
                    || (nMergeFld - nMergeFld % 100) == oMailing._nParentTabId
                )
                    return;

                aMergeField.push(nMergeFld);
            });



            if (aMergeField.length > 0) {
                oCmptMailsManager.addParam("mergedtabs", aMergeField.join(";"), "post");
            }
        }
    }
    catch (e) {

    }

    if (oMailing._mailingId == 0)
        oCmptMailsManager.addParam("operation", OperationTplMail.RENAME, "post");  // Compteur campagne en cours de création
    else
        oCmptMailsManager.addParam("operation", OperationTplMail.UPDATE, "post");  // compteur campagne déjà créée

    //Ajout des paramètres de la campagne
    for (var index in oMailing._aMailingParams) {
        if (index != "operation")
            oCmptMailsManager.addParam(index, oMailing._aMailingParams[index], "post");
    }

    //Confirmation
    eConfirm(1, top._res_6620, top._res_6689, '', 500, 200, function () { setWait(true); oCmptMailsManager.send(updtCmptNbMail); });


}

///met à jour le compteur d'email
function updtCmptNbMail(sRes) {

    setWait(false);
    var btn = document.getElementById("CampaingInfoNbMail");
    setAttributeValue(btn, "value", sRes + "");
}



function onSelectMailingDate(id, parametre) {

    var input = document.getElementById(id);
    var date = getAttributeValue(input, "value"); // TODO : pas la peine de faire une conversion si le catalog date la gère
    /*
    //si date d'envoi on precise l'heure
    if (id == "delayedMail_Date" && date.length == 0) {
        var now = new Date();
        var day = now.getDate();
        if (day < 10) day = '0' + day;
        var month = now.getMonth() + 1; // le mois commence à 0 en JS 
        if (month < 10) month = '0' + month;
        var year = now.getFullYear();
        var strDefaultMessage = day + "/" + month + "/" + year; // TODO: localisation/internationalisation ?
        date = " " + now.toLocaleTimeString();
    }
    */

    txt = id;
    param = parametre;

    modalDate = createCalendarPopUp("onValidSelectgDate", "1", "0", top._res_5017, top._res_5003, "SelectMailingDateOk", top._res_29, null, null, wizardIframe.id, null, date);

}

function onValidSelectgDate(date) {


    var inpt = document.getElementById(txt);

    setAttributeValue(inpt, "value", date);
    modalDate.hide();

    //mise a jour de l'emailing
    if (param != null) {
        oMailing.SetParam(param, eDate.ConvertDisplayToBdd(date));
        if (param == "sendingDate")
            oMailing.SetParam('immediateSending', '0');
    }
}

function SelectMailingDateCancel() {
    calpopup.hide();
    //mise a jour de l'emailing
    oMailing.SetParam('immediateSending', '0');
}

function ValidSelectMailingDate(date) {


    var inpt = document.getElementById("delayedMail_Date");
    inpt.value = date;

    calpopup.hide();

    //mise a jour de l'emailing
    oMailing.SetParam('immediateSending', '0');
    oMailing.SetParam('sendingDate', eDate.ConvertDisplayToBdd(date));


}

var saveClickCount = 0;
function SendEmailing() {
    var strMessages = [
        "L'envoi d'emailing n'est pas encore implémenté. Un peu de patience, le développement est en cours...",
        "Ca va venir, ne vous inquiétez pas...",
        "C'est pour bientôt. Un peu de patience !",
        "Bon, ça suffit, maintenant.",
        "Ca n'ira pas plus vite, vous savez.",
        "C'est fini, oui ?!",
        "Je vais m'énerver.",
        "Stop, maintenant !",
        "Bon, j'arrête. Je me tais."
    ];
    if (saveClickCount >= strMessages.length)
        return false;
    else {
        eAlert(0, "Fonctionnalité non implémentée", strMessages[saveClickCount]);
        saveClickCount++;
    }
}

// ASY : Afficher le catalogue avancé permettant d'associer une catégorie  à un EMailing
var dialogWindow;
function showCategoryCat(tabIndex, lineIndex) {

    //Champ value
    var oTarget = document.getElementById("value_" + tabIndex + "_" + lineIndex);

    var catDescId = oTarget.getAttribute("edndescid");
    var catPopupType = oTarget.getAttribute('popup');

    var catBoundDescId = oTarget.getAttribute('bounddescid');
    var catBoundPopup = oTarget.getAttribute('boundpopup');
    var catParentValue = oTarget.getAttribute('boundvalue');
    var bMulti = oTarget.getAttribute("multi");

    var defValue = oTarget.getAttribute("ednvalue");

    showCatGeneric((bMulti == "1"), false, defValue, null
        , oTarget.id, catDescId, catPopupType, catBoundDescId
        , catBoundPopup, catParentValue, top._res_6479, "dialogWindow", false
        , partOfValidateCatDlg_Mailing
    );
}
//Catalogues : Partie de code exécuté au clic sur valider juste avant la fermeture du catalogue avancé
function partOfValidateCatDlg_Mailing(catalogDialog, srcId, trgId, tabSelectedLabels, tabSelectedValues, selectedIDs) {
    var selectedLabels = "";
    if (tabSelectedLabels.length > 0)
        selectedLabels = tabSelectedLabels[0]
    var selectedValues = "";
    if (tabSelectedValues.length > 0)
        selectedValues = tabSelectedValues[0]

    document.getElementById(trgId).value = selectedLabels;
    document.getElementById(trgId).setAttribute("ednvalue", selectedValues);
    if (selectedLabels != "") {
        setAttributeValue(document.getElementById(trgId), "eAlert", "0");
    }
    //mise a jour parametre emailing
    oMailing.SetParam("category", selectedValues);
}


var dialogWindow;
var idModele;

var SEPARATOR_LVL1 = "#|#";
var SEPARATOR_LVL2 = "#$#";
// #68 13x et Backlog #375 - Editeur de templates HTML avancé (grapesjs) - Uniquement accessible en E2017 - Ajout du paramètre enableHTMLTemplateEditor
// US #1002 - Tâche #1732 - Passage du type de lien de désinscription à afficher (avec ou sans gestion de consentements)
function EmailingTemplateDialog(id, mtType, enableTemplateEditor, useNewUnsubscribeMethod) {

    if (!id) id = 0;
    dialogWindow = new eModalDialog(top._res_748, 0, "eTplMailDialog.aspx", null, null, "EmailingTemplateDialog");
    dialogWindow.addParam("MailTemplateId", id, "post");
    dialogWindow.addParam("EditorType", "mailing", "post");
    dialogWindow.addParam("ToolbarType", (mtType == MAILTEMPLATETYPE_EMAILING ? "mailingtemplate" : "mailtemplate"), "post");
    dialogWindow.addParam("EnableTemplateEditor", (enableTemplateEditor ? "1" : "0"), "post"); // #68 13x - Type de champ Mémo - Editeur de templates HTML avancé (grapesjs) ou CKEditor
    dialogWindow.addParam("UseNewUnsubscribeMethod", (useNewUnsubscribeMethod ? "1" : "0"), "post"); // US #1002 - Tâche #1732 - Passage du type de lien de désinscription à afficher (avec ou sans gestion de consentements)
    dialogWindow.addParam("tabfrom", oMailing._tab, "post");
    //Passages des variables de contextes du mailing
    if (typeof (oCurrentWizard) != "undefined"
        && typeof (oCurrentWizard._nParentTabId) != "undefined" && oCurrentWizard._nParentTabId > 0
        && typeof (oCurrentWizard._nParentFileId) != "undefined" && oCurrentWizard._nParentFileId > 0
        && typeof (oCurrentWizard._tab) != "undefined" && oCurrentWizard._tab > 0)
        dialogWindow.addParam("AddMemoVar",
            "_nParentTabId" + SEPARATOR_LVL2 + oCurrentWizard._nParentTabId
            + SEPARATOR_LVL1 + "_nParentFileId" + SEPARATOR_LVL2 + oCurrentWizard._nParentFileId
            + SEPARATOR_LVL1 + "_tab" + SEPARATOR_LVL2 + oCurrentWizard._tab
            , "post");

    dialogWindow.addParam("Value", "", "post");

    dialogWindow.addParam("mtType", mtType, "post");

    dialogWindow.addParam("iframeScrolling", "yes", "post");

    dialogWindow.show();
    dialogWindow.MaxOrMinModal();

    dialogWindow.addButton(top._res_29, CancelMailingTemplateWizard, "button-gray", null, "cancel_btn");
    dialogWindow.addButtonFct(top._res_28, onUserNewOrUpdtTpl, "button-green", "val_btn");

    dialogWindow.addButtonFct(top._res_25, function () {
        dialogWindow.getIframe().eTplMail.StepClick(1);
    }, "button-gray-leftarrow", "prev_btn"); // précédant


    dialogWindow.addButtonFct(top._res_26, function () {
        dialogWindow.getIframe().eTplMail.StepClick(2);
    }, "button-green-rightarrow", "next_btn");  // suivant





    dialogWindow.hideButtons()

    idModele = id;
}

// #68 13x et Backlog #375 - Editeur de templates HTML avancé (grapesjs) - Uniquement accessible en E2017 - Ajout du paramètre enableTemplateEditor
// US #1002 - Tâche #1732 - Passage du type de lien de désinscription à afficher (avec ou sans gestion de consentements)
function EditMailTemplate(id, mtType, enableTemplateEditor, useNewUnsubscribeMethod) {
    EmailingTemplateDialog(id, mtType, enableTemplateEditor, useNewUnsubscribeMethod);
}

// #68 13x et Backlog #375 - Editeur de templates HTML avancé (grapesjs) - Uniquement accessible en E2017 - Ajout du paramètre enableTemplateEditor
// US #1002 - Tâche #1732 - Passage du type de lien de désinscription à afficher (avec ou sans gestion de consentements)
function AddNewModele(id, mtType, enableTemplateEditor, useNewUnsubscribeMethod) {
    EmailingTemplateDialog(id, mtType, enableTemplateEditor, useNewUnsubscribeMethod);
}


function onUserTplCancel() {
    dialogWindow.hide();
}

function CancelMailingTemplateWizard() {
    if (dialogWindow) {
        var pjIds = dialogWindow.getIframe().document.getElementById("btnPJ").getAttribute("pjids");

        if (idModele == 0 && (pjIds != null && pjIds != ""))
            DeletePJ(107000, 0, pjIds, false, null);

        onUserTplCancel();
    }
}

function onUserNewOrUpdtTpl() {
    var label = dialogWindow.getIframe().document.getElementById("lbl").value;

    if (label.replace(/\s/g, "").length == 0) {
        eAlert(0, top._res_203, top._res_373.replace("<ITEM>", top._res_6485), '', 510, 170);
    }
    else {
        var objet = null;
        var objEl = dialogWindow.getIframe().document.getElementById("obj");

        //ALISTER #81112 Concerne le Modèle d'email unitaire, devant récupéré la valeur directement dans le champ texte
        if (objEl.tagName === "INPUT")
            objet = dialogWindow.getIframe().document.getElementById("obj").value;
        else
            objet = dialogWindow.getIframe().document.getElementById("obj").innerHTML;

        ///AAbbA tache #1 940
        var preheader = dialogWindow.getIframe().document.getElementById("preheader");

        if (dialogWindow.getIframe().eTplMail && dialogWindow.getIframe().eTplMail.currentEditor == 2) {
            var oContent = dialogWindow.getIframe().eTplMailDialogEditorObjectCKe.getData();
            var oCssContent = dialogWindow.getIframe().eTplMailDialogEditorObjectCKe.getCss();


            dialogWindow.getIframe().eTplMailDialogEditorObject.setData(oContent);
            dialogWindow.getIframe().eTplMailDialogEditorObject.setCss(oCssContent);
        }


        var body = dialogWindow.getIframe().eTplMailDialogEditorObject.getData();
        var bodyCss = dialogWindow.getIframe().eTplMailDialogEditorObject.getCss();
        var mtType = dialogWindow.getIframe().document.getElementById("mtType").value;
        var pjIds = dialogWindow.getIframe().document.getElementById("btnPJ").getAttribute("pjids");

        //oTemplate.GetPerm().SetPublic(false); //#43518 MOU : il a été decdidé de le mettre en non public par défaut, #41134 CRU : Lorsque le modèle est ajouté depuis "Vos modèles", on le met en public pour l'instant

        // Ajout des parametres de PERMISSION   

        var bPublic = getAttributeValue(dialogWindow.getIframe().document.getElementById("chk_OptPublicFilter"), "chk") == "1";

        oTemplate.GetPerm().SetPublic(bPublic);

        UpdatePermission("View");
        UpdatePermission("Update");
        // AABBA tache #1 940 ajout de preheader dans le paramètres
        //SHA : tâche #1 939
        if (!preheader || typeof preheader === 'undefined') {
            preheader = document.getElementById("COL_106000_106047_0_0_0");
            if (preheader)
                preheader = preheader.value;
        }
        else
            preheader = preheader.value;

        AddOrUpdateMailTpl(idModele, label, objet, preheader, body, bodyCss, mtType, pjIds);
    }
}


/// <summary>
/// Initie le renommage d'un modèle d'emailing via appel à un eFieldEditor
/// </summary>
/// <param name="baseName">Id de la cellule</param>
/// <param name="elem">Bouton cliqué (déclencheur de l'action de renommage)</param>
function renMailTpl(baseName, elem) {

    _ePopupVNEditor = new ePopup('_ePopupVNEditor', 220, 250, 0, 0, document.body, false);
    _eMailTplNameEditor = new eFieldEditor('inlineEditor', _ePopupVNEditor, "_eMailTplNameEditor");
    _eMailTplNameEditor.action = 'renameMailTpl';

    var libElem = document.getElementById(baseName);

    if (libElem)
        _eMailTplNameEditor.onClick(libElem, elem);
}

/// <summary>
/// Gére le retour xml aprés renommage d'un modèle d'emailing: 
/// </summary>
/// <param name="oDoc">Document xml renvoyé</param>
function onMailTplRenameTrait(oDoc) {
    if (oDoc && oDoc.nodeType == 9) {

        var bSuccess = (getXmlTextNode(oDoc.getElementsByTagName("success")[0]) == "1");
        if (!bSuccess) {
            DisplayErrorInfo(oDoc);
            return;
        }

        // Succes de la MaJ
        var sNewName = getXmlTextNode(oDoc.getElementsByTagName("mailTplname")[0]);
        var nId = getXmlTextNode(oDoc.getElementsByTagName("iMailTemplateId")[0]);

        var mailTplTab = document.getElementById("mt_107000");
        divs = mailTplTab.getElementsByTagName("div");

        //Maj la liste     
        for (var i = 0; i < divs.length; i++) {

            var div = divs[i];
            if (div.id.indexOf("COL_107000_107001_" + nId + "_" + nId + "_") == 0) {
                //Gestion bug IE7 sur innerHTML
                if (div.innerText != null)
                    div.innerText = sNewName;
                else {
                    if (div.textContent != null)
                        div.textContent = sNewName;
                    else
                        div.innerHTML = sNewName;
                }

                if (oTemplate.GetTplId() == nId && oTemplate.GetType() == USER_MAIL_TPL)
                    oTemplate.SetName(sNewName);

                break;
            }
        }

        //CSS de validation contour de champ
        if (typeof (_eMailTplNameEditor) == "object") {
            _eMailTplNameEditor.flagAsEdited(true);
        }

    }
}

function delMailTpl(MainFileid) {
    eConfirm(1, top._res_806, top._res_6480, '', 500, 200, function () { OnDeleteTemplate(MainFileid) });
}

//function addPJToMailTpl(element, id) {
//    var nTab = nGlobalActiveTab;
//    var sourceTpl = "mailtemplate";
//    var pjSpan = null;
//    var idsOfPj = getAttributeValue(findUp(element, "TR"), "pjlist");

//    var oModalPJAdd = showPJDialog(nTab, id, sourceTpl, pjSpan, false, idsOfPj);
//}

/// <summary>
/// Sélectionne un modèle de mail 
/// </summary>
/// <param name="obj">Td sélectionné</param>
function selectMailTpl(obj) {

    //Gére la sélection des modèles de mails utilisateurs
    oTemplate.selectUserTpl(obj);

    //On déselectionne l'ancien modèle
    var elem = obj;

    if (typeof (_eCurrentSelectedMailTpl) != 'undefined' && _eCurrentSelectedMailTpl != null)
        removeClass(_eCurrentSelectedMailTpl, "eSel");

    if (addClass != null)
        addClass(elem, "eSel");

    _eCurrentSelectedMailTpl = elem;

    updateDefaultTplCheckbox(elem)
}

function updateDefaultTplCheckbox(element) {
    var mtid = getAttributeValue(element, "mtid");

    var defaultTpl = document.getElementById("DefaultTemplate");
    setAttributeValue(defaultTpl, "mtid", mtid);

    chgChk(defaultTpl, (mtid == _defaultTemplate));


}

/// Fonction de callback gérant le retour serveur
///<param name="oDoc">Document xml de retour serveur</param>
function ManageFeedback(oDoc) {

    var oWizard;

    if (typeof oMailing != "undefined")
        oWizard = oMailing
    else if (typeof oSmsing != "undefined")
        oWizard = oSmsing

    var bSuccess = (getXmlTextNode(oDoc.getElementsByTagName("success")[0]) == "1");
    if (!bSuccess) {
        DisplayErrorInfo(oDoc);
        return;
    }

    //Si on a crée un nouveau modèle, on mis a jour oTemplate et on présélection le modèle dans la liste
    oTemplate.UpdateUserTpl(oDoc);


    //Met à jour les infos sur le wizard
    var currWizard = oWizard || (eTools.GetModal("MailingWizard") != null && eTools.GetModal("MailingWizard").isModalDialog && eTools.GetModal("MailingWizard").getIframe().oMailing) || false


    if (currWizard && currWizard.TypeWizard === "mailing") {

        let localStep;

        if (typeof iCurrentStep != "undefined") {
            localStep = iCurrentStep;
        }
        else
            localStep = currWizard._currentStep

        if (typeof localStep != "undefined") {

            if (currWizard.GetStepName(localStep) == "mail" || currWizard.GetStepName(localStep) == "mailck") {
                currWizard.SetParam("templateId", oTemplate.id);
                currWizard.SetParam("templateType", oTemplate.type);
            }

            //On ferme la fenetre et on recharge la liste des modèle (Pas de modèle préselection dans la liste quand on appelle loadlist)
            if (dialogWindow)
                dialogWindow.hide();

            loadList();
        }
    }
    else if (currWizard && currWizard.TypeWizard === "smsing") {

        currWizard.SetTemplateId(oTemplate.id)

        try {
            var sNameTpl = getXmlTextNode(oDoc.getElementsByTagName("mailTplname")[0])

            var currDocContext = document;
            if (top.modalWizard != null)
                currDocContext = top.modalWizard.getIframe().document;

            var label = currDocContext.getElementById("smsingShowTplLstBtn")
            if (label)
                label.innerHTML = top._res_464 + " : " + sNameTpl;
        }
        catch (err) { }

        if (dialogWindow)
            dialogWindow.hide();
    }


}

/// Affiche un message approprié en fonction du code d'erreur
function DisplayErrorInfo(oDoc) {

    var sNewName = getXmlTextNode(oDoc.getElementsByTagName("mailTplname")[0]);

    var nErrCode = getNumber(getXmlTextNode(oDoc.getElementsByTagName("ErrorCode")[0]));
    var sErrDesc = getXmlTextNode(oDoc.getElementsByTagName("ErrorDescription")[0]);

    switch (nErrCode) {
        case 1:
            //le nom existe déjà
            sErrDesc = top._res_6604.replace("<NAME>", sNewName);
            break;
        case 2:
            //le nom ne devrait pas etre vide (on devrait pas tomber sur ce cas..sauf si modif en cours d'acheminement de la requete)
            sErrDesc = top._res_373.replace("<ITEM>", top._res_6485);
            break;
        default:
            break;
    }

    eAlert(0, top._res_203, sErrDesc, '', 450, 170);
}

///summary
/// Fonction de callback gérant la suppression d'un modèle
///<param name="mailTplId">Id du modèle</param>
///summary
function OnDeleteTemplate(templateId) {

    var url = "mgr/eMailingTemplateManager.ashx";
    var ednu = new eUpdater(url, 0);
    ednu.addParam("operation", OperationTplMail.DELETE, "post");
    ednu.addParam("MailTemplateId", templateId, "post");
    //alert("delMailTpl(" + templateId + ")");
    ednu.ErrorCallBack = function () { };
    ednu.send(OnDeleteMailingTemplate);
}

function OnDeleteMailingTemplate(oDoc) {

    if (oDoc && oDoc.nodeType == 9) {

        var bSuccess = (getXmlTextNode(oDoc.getElementsByTagName("success")[0]) == "1");
        var nId = getXmlTextNode(oDoc.getElementsByTagName("iMailTemplateId")[0]);

        if (!bSuccess) {
            var nErrCode = getXmlTextNode(oDoc.getElementsByTagName("ErrorCode")[0]);
            var sErrDesc = getXmlTextNode(oDoc.getElementsByTagName("ErrorDescription")[0]);
            showWarning("", " Erreur non gérée :", sErrDesc);//TODORES
        }
        else {
            // Succes
            // reload la liste
            // var oTable = document.getElementById("mt_107000");
            var oTable = document.getElementById("mt_107000");
            var nNbRow = oTable.rows.length;

            for (var nCmpt = 0; nCmpt < nNbRow; nCmpt++) {
                var oTR = oTable.rows[nCmpt];
                if (oTR.getAttribute("eid") == "107000_" + nId) {
                    oTable.deleteRow(nCmpt);
                    // HLA - Inutile de continuer, l'emailing a été supprimé de la grille
                    break;
                    //nCmpt--;
                    //nNbRow--;
                }
            }

            var oLstTR = oTable.querySelectorAll("tbody tr[eid]");

            for (var nCmpt = 0; nCmpt < oLstTR.length; nCmpt++) {

                removeClass(oLstTR[nCmpt], "list_odd");
                removeClass(oLstTR[nCmpt], "list_even");

                var sClassName = nCmpt % 2 ? "list_odd" : "list_even";

                addClass(oLstTR[nCmpt], sClassName);
            }

            loadList();

            //si le modèle est supprimé, on le retire de l'objet oTemplate
            if (nId == oTemplate.GetTplId())
                oTemplate.id = 0;
        }
    }
}


function onUserSaveTplCancel() {
    oModal.hide();
}
function onUserSaveTpl() {
    // Backlog #617, #619, #648 et #72 087 - Le CSS doit être récupéré de l'éditeur principal (grapesjs ou CKEditor si IE/Edge) comme pour le corps de mail
    // Celui-ci ayant été préalablement mis à jour à partir de l'éditeur secondaire (CKEditor) via majMainEditor()
    // On récupère donc la référence au mainEditor de la même façon que le fait majMainEditor()
    var mainEditor = oMailing._oMemoEditor;
    if (oMailing._oMemoEditor != null && typeof ("".endsWith) == "function" && oMailing._oMemoEditor.name.endsWith("_1")) {
        if (nsMain.getAllMemoEditorIDs().length > 1) {
            mainEditor = nsMain.getMemoEditor(0);
        }
    }

    var lbl = document.querySelectorAll("td[lib='Objet']");
    var _obj = document.querySelectorAll("input[ename=" + lbl[0].id + "]");
    var obj = document.getElementById(_obj[0].id).value;
    var body = mainEditor.getData();
    var bodyCss = mainEditor.getCss();

    OnAddTemplate(oModal.getPromptValue(), obj, body, bodyCss);

    oModal.hide();
    loadList();
}


//************************************************
/// param oMod : modal du modèle
function beforeSaveTemplate(oMod) {

    dialogWindow = new eModalDialog(top._res_6543, 0, "mgr/eMailingTemplateManager.ashx", 450, 400);

    //CallBack d'erreur : on masque la modal
    //dialogWindow.ErrorCallBack = launchInContext(dialogWindow, dialogWindow.hide);

    //Ajout des parametres de PERMISSION   
    oTemplate.GetPerm().AppendParams(dialogWindow);

    dialogWindow.addParam("operation", OperationTplMail.DIALOG_SAVEAS, "post");
    dialogWindow.addParam("MailTemplateId", oTemplate.GetTplId(), "post");
    dialogWindow.addParam("lbl", oTemplate.GetName(), "post");
    dialogWindow.addParam("tplType", oTemplate.GetType(), "post");
    dialogWindow.addParam("tplTypeDb", oTemplate.GetMailTemplateType(), "post");

    dialogWindow.onIframeLoadComplete = (function (iframeId) { return function () { onLnkTplMailLoad(iframeId); } })(dialogWindow.iframeId);
    dialogWindow.show();

    dialogWindow.addButton(top._res_29, function () { beforeSaveTplCancel() }, "button-gray", ""); // Annuler
    dialogWindow.addButton(top._res_28, function () { beforeSaveTplValid(dialogWindow, oMod) }, "button-green"); // Enregistrer

}


function onLnkTplMailLoad(iFrameId) {
    try {

        var oFrm = dialogWindow.getIframe();
        var oFrmDoc = oFrm.document;
        var oFrmWin = oFrm.window;

        onDialogPermLoad(iFrameId);

        doPermParam("View", oFrmWin);
        doPermParam("Update", oFrmWin);


        setWait(false);


    }
    catch (exp) {
    }
}
function beforeSaveTplCancel() {
    dialogWindow.hide();
}

///oModSav : modal de sauvegarde du modèle
/// oModMailTpl : modal d'édition du modèle. (normalement, correspond à la modal du wizard de la campage)
function beforeSaveTplValid(oModSave, oModMailTpl) {
    // Récupération de la fenêtre modal du modèle
    var oDocMailTpl;
    if (oModMailTpl && oModMailTpl.isModalDialog)
        oDocMailTpl = oModMailTpl.getIframe().document;
    else
        oDocMailTpl = document;

    var nCampagnId = 0;

    var fileDiv = oDocMailTpl.querySelector("div[id='fileDiv_106000']");
    if (fileDiv) {
        nCampagnId = getAttributeValue(fileDiv, "fid");
    }

    var oIfrm = oModSave.getIframe();
    var newName = oIfrm.document.getElementById("PermName").value;


    if (newName.replace(/\s/g, "").length == 0) {
        var rubrique = oIfrm.document.getElementById("DivLabel").innerHTML;
        eAlert(0, top._res_203, top._res_373.replace("<ITEM>", rubrique), '', 500, 170);
    }
    else {
        var bPublic = getAttributeValue(oIfrm.document.getElementById("chk_OptPublicFilter"), "chk") == "1";

        var objInpt = oDocMailTpl.querySelector("input[ename='COL_106000_106005']");
        var obj = "";

        if (objInpt)
            obj = objInpt.value;
        else {//KJE tâche 2 334
            var objDiv = oDocMailTpl.querySelector("DIV[ename='COL_106000_106005']");
            if (objDiv)
                obj = objDiv.innerHTML;
        }


        var saver = function () {

            // Backlog #617, #619, #648 et #72 087 - Le CSS doit être récupéré de l'éditeur principal (grapesjs ou CKEditor si IE/Edge) comme pour le corps de mail
            // Celui-ci ayant été préalablement mis à jour à partir de l'éditeur secondaire (CKEditor) via majMainEditor()
            // On récupère donc la référence au mainEditor de la même façon que le fait majMainEditor()
            var mainEditor;

            var mtType = MAILTEMPLATETYPE_EMAILING;


            var body = "";
            var bodyCss = "";
            var bgColor = "";
            var preheader = "";
            var sPjIds = "";


            if (typeof oMailing !== "undefined") {
                mainEditor = oMailing._oMemoEditor;
                if (oMailing._oMemoEditor != null && typeof ("".endsWith) == "function" && oMailing._oMemoEditor.name.endsWith("_1")) {
                    if (nsMain.getAllMemoEditorIDs().length > 1) {
                        mainEditor = nsMain.getMemoEditor(0);
                    }
                }

                body = mainEditor.getData();
                bodyCss = mainEditor.getCss();
                bgColor = mainEditor.getColor()

                if (bgColor) {
                    var res = eTools.setCssRuleFromString(bodyCss, "body", "background-color", bgColor, true);
                    if (res.hasChanged) {
                        mainEditor.setCss(res.value)
                        bodyCss = res.value;
                    }
                }

                //#62907 : CNA - Quand c'est un modèle personnalisable on ne passe pas l'id, sinon en back on charge les infos d'un modèle utilisateurs et on récupère des attributs erronés
                // AABBA tache #1 940 ajout de preheader dans le paramètres
                //SHA : tâche #1 939
                preheader = document.getElementById("COL_106000_106047_0_0_0");

                //SHA : tâche #1 939
                if (preheader && typeof preheader !== 'undefined')
                    preheader = preheader.value;

                sPjIds = oMailing.GetPjIds()
            }
            else if (typeof oSmsing !== "undefined") {
                mainEditor = oSmsing.GetMemoEditor();
                body = mainEditor.getData();

                mtType = MAILTEMPLATETYPE_SMSING;
            }
            else {
                return;
            }
            //permissions
            oTemplate.GetPerm().SetPublic(bPublic);
            oTemplate.SetFromCampaign(true);

            UpdatePermission("View");
            UpdatePermission("Update");

            AddOrUpdateMailTpl(oTemplate.GetType() == "1" ? 0 : oTemplate.GetTplId(), newName, obj, preheader, body, bodyCss, mtType, sPjIds, true, nCampagnId);


        }

        //On met à jour l'éditeur principal
        if (oCurrentWizard && oCurrentWizard.TypeWizard !== "smsing" && oCurrentWizard.GetStepName(oCurrentWizard._currentStep) == "mailck") {
            //le corps du mail étant pris de l'éditeur principal, on met à jour celui-ci
            oMailing.majMainEditor(saver);
        }
        else
            saver();
    }

}
// AABBA tache #1 940 ajout de preheader dans le paramètres
function AddOrUpdateMailTpl(id, newName, subject, preheader, body, bodyCss, mtType, pjIds, clonePjCampaignIds, nCampagnId) {

    var url = "mgr/eMailingTemplateManager.ashx";
    var ednu = new eUpdater(url, 0);

    // update
    if (id > 0) {
        ednu.addParam("operation", OperationTplMail.UPDATE, "post");
        ednu.addParam("MailTemplateId", oTemplate.GetTplId(), "post");
    }
    else //insert
    {
        ednu.addParam("operation", OperationTplMail.ADD, "post");
        ednu.addParam("MailTemplateId", 0, "post");
    }

    var tab = 0;
    if (typeof oMailing != "undefined")
        tab = oMailing._tab
    else if (typeof oSmsing != "undefined")
        tab = oSmsing.GetBkmTab()

    ednu.addParam("tab", tab, "post");
    ednu.addParam("lbl", newName, "post");
    ednu.addParam("obj", subject, "post");
    // AABBA tache #1 940
    ednu.addParam("preheader", preheader, "post");
    ednu.addParam("body", body, "post");
    ednu.addParam("bodyCss", bodyCss, "post");

    ednu.addParam("tplTypeDb", mtType, "post");

    if (typeof (pjIds) !== 'undefined')
        ednu.addParam("pjids", pjIds, "post");

    // Si on sauvegarde la campagne mail comme modèle, le serveur doit savoir s'il duplique les entrées dans pj
    ednu.addParam("clonePjIds", (typeof (clonePjCampaignIds) !== 'undefined' && clonePjCampaignIds) ? "1" : "0", "post");

    // identifiant de la campagne mail
    ednu.addParam("campaignId", (typeof (nCampagnId) !== 'undefined') ? nCampagnId : "0", "post");

    //Ajout des parametres de PERMISSION   
    oTemplate.GetPerm().AppendParams(ednu);

    ednu.send(ManageFeedback);
}


//initialisation ...
function doPermParam(permType, oFrmWin) {

    if (oFrmWin == null || typeof (oFrmWin) == "undefined")
        oFrmWin = window;

    //par niveau
    var srcId = "OptLevels_" + permType;
    if (srcId.indexOf("OptLevels_" + permType) >= 0 && oFrmWin.document.getElementById(srcId) != null) {
        if (oFrmWin.document.getElementById(srcId).checked) {

            oFrmWin.document.getElementById("LevelLst_" + permType).disabled = false;
        }
        else {

            oFrmWin.document.getElementById("LevelLst_" + permType).disabled = true;
        }
    }
    //par user
    srcId = "OptUsers_" + permType;
    if (srcId.indexOf("OptUsers_" + permType) >= 0 && oFrmWin.document.getElementById(srcId) != null) {

        if (oFrmWin.document.getElementById(srcId).checked) {

            oFrmWin.document.getElementById("TxtUsers_" + permType).style.display = "inline-block";
            oFrmWin.document.getElementById("UsersLink_" + permType).style.display = "inline-block";
        }
        else {

            oFrmWin.document.getElementById("TxtUsers_" + permType).style.display = "none";
            oFrmWin.document.getElementById("UsersLink_" + permType).style.display = "none";
        }
    }

    //mise a jour du modèle actuel,
    //mise a jour des inputs Hidden
    var permId = oFrmWin.document.getElementById(permType + "PermId").value;
    var levels = oFrmWin.document.getElementById(permType + "PermLevel").value;
    var users = oFrmWin.document.getElementById(permType + "PermUsersId").value;
    var perMode = oFrmWin.document.getElementById(permType + "PermMode").value;
    var oPerm = oTemplate.GetPerm();
    //Application des permissions sur le modèle actuel
    oPerm.SetPermParam(permType, "id", permId);
    oPerm.SetPermParam(permType, "level", levels);
    oPerm.SetPermParam(permType, "user", users);
    oPerm.SetPermParam(permType, "mode", perMode);

}

//**************************

/// Mis a jour
function UpdatePermission(permTyp) {

    var childwindow = dialogWindow.getIframe();

    var objRetValue = getPermReturnValue(permTyp, childwindow);

    //Application des permissions sur le modèle actuel
    var oPerm = oTemplate.GetPerm();
    oPerm.SetPermParam(permTyp, "level", objRetValue.levels);
    oPerm.SetPermParam(permTyp, "user", objRetValue.users);
    oPerm.SetPermParam(permTyp, "mode", objRetValue.perMode);

}

function onDefaultTemplateCheck(element, typeEmail) {
    var mtid = getAttributeValue(element, "mtid");
    var bChecked = getAttributeValue(element, "chk");


    if (bChecked == "0") {
        mtid = "";
    }


    updateDefaultTemplate(mtid, typeEmail);

    // Coloration de la ligne
    var trDefaultMailTemplate = document.querySelector("tr.defaultTemplate");
    var trMailTemplate = document.querySelector("tr[mtid='" + getAttributeValue(element, "mtid") + "']");

    if (bChecked == "0") {
        mtid = "";

        eTools.RemoveClassName(trMailTemplate, "defaultTemplate");
    }
    else {
        eTools.RemoveClassName(trDefaultMailTemplate, "defaultTemplate");

        eTools.SetClassName(trMailTemplate, "defaultTemplate");
    }
}

function updateDefaultTemplate(mtid, typeEmail) {
    _defaultTemplate = mtid;

    var url = "mgr/eMailingTemplateManager.ashx";
    var ednu = new eUpdater(url, 0);

    ednu.addParam("operation", OperationTplMail.SET_DEFAULT, "post");
    ednu.addParam("MailTemplateId", mtid, "post");
    ednu.addParam("tab", oMailing._tab, "post");
    ednu.addParam("tplTypeDb", typeEmail, "post");
    //ednu.addParam("bSetDefault", bChecked, "post");

    ednu.send();
}

//Gère l'affichage de l'infobulle des Modeles (Templates)
function shTplDescId(templateId) {

    var url = "mgr/eMailingTemplateManager.ashx";
    var ednu = new eUpdater(url, 0);
    ednu.addParam("operation", OperationTplMail.TOOLTIP, "post");
    ednu.addParam("MailTemplateId", templateId, "post");

    ednu.ErrorCallBack = function () { };
    ednu.send(OnDisplayTooltipTemplate);
}


function OnDisplayTooltipTemplate(oDoc) {

    if (oDoc && oDoc.nodeType == 9) {

        var bSuccess = (getXmlTextNode(oDoc.getElementsByTagName("success")[0]) == "1");
        var textBody = getXmlTextNode(oDoc.getElementsByTagName("body")[0]);

        if (!bSuccess) {
            var sErrDesc = getXmlTextNode(oDoc.getElementsByTagName("ErrorDescription")[0]);
            showWarning("", " Erreur non gérée :", sErrDesc);//TODORES
        }
        else {
            st(event, textBody, "filterToolTip");
        }

    }
}

function showTemplatePJList(pjTabDescid, templateID, sourceType) {
    var sourceTpl;
    if (typeof (sourceType) == "undefined")
        sourceTpl = "mailtemplate";
    else
        sourceTpl = sourceType;

    var pjSpan = null;
    var idsOfPj = "";

    var oModalPJAdd = showPJDialog(pjTabDescid, templateID, sourceTpl, pjSpan, false, idsOfPj);
}


function RefreshSenderAlias(isExternalEmailing) {
    if (isExternalEmailing == true) {

        var slctSender = document.getElementById("sender-opt");
        var slctDomain = document.getElementById("domain-opt");
        var txtbxAlias = document.getElementById("mailing_SenderAlias");

        if (slctSender != null && slctDomain != null && txtbxAlias != null) {
            txtbxAlias.value = ReplaceOriginMailDomainWithAlias(slctSender.value, slctDomain.value)
        }
    }
}

function ReplaceOriginMailDomainWithAlias(mail, domainAlias) {
    if (domainAlias == null || domainAlias == '')
        return mail;

    var accountName = mail.split('@')[0];
    return accountName + "@" + domainAlias;
}

var eMemoDialogEditorObject = null;
function onCreateObjCKEditor() {
    var objContainer = document.getElementById('objCKEditor');
    var objValue = document.getElementById('obj');
    eMemoDialogEditorObject = new eMemoEditor("eMemoDialogEditor", true
        , objContainer
        , null
        , objValue.innerHTML
        , true, 'eMemoDialogEditorObject');
    eMemoDialogEditorObject.inlineMode = true;
    eMemoDialogEditorObject.setSkin('eudonet');
    eMemoDialogEditorObject.config.width = "50%";
    eMemoDialogEditorObject.config.height = "20px";
    eMemoDialogEditorObject.enterMode = CKEDITOR.ENTER_DIV;
    eMemoDialogEditorObject.show();
}

//KJE, tâche #2 431: cette méthode permet d'afficher les champs de fusion pour l'objet du mail
function openMemo_TplObj(elementId) {

    var lWidth = 900;
    var lHeight = 600;
    var oAutomationModal = new eModalDialog(top._res_2495, 0, "eMemoDialog.aspx", lWidth, lHeight);

    var target = document.getElementById(elementId);
    var html = "";
    if (target)
        html = encode(target.innerHTML);

    // Instanciation d'un objet eMemoEditor "interne"
    var oAutomationMemo = new eMemoEditor("oAutoMemo_TplObj", true, null, null, "", true, "oAutomationMemo");
    oAutomationMemo.showOnlyMerged = "1";
    oAutomationMemo.childDialog = oAutomationModal;
    oAutomationMemo.mergeFields = mailObjectMergeFields;   // liste des champs de fusion 
    oAutomationMemo.toolbarType = "mailSubject";
    oAutomationMemo.automationEnabled = true;
    oAutomationModal.addParam("ParentFrameId", oAutomationModal.iframeId, "post");
    oAutomationModal.addParam("ParentEditorJsVarName", "oAutoMemo_TplObj", "post");
    oAutomationModal.addParam("divMainWidth", lWidth, "post");
    oAutomationModal.addParam("divMainHeight", lHeight, "post");
    oAutomationModal.addParam("IsHTML", "1", "post");
    oAutomationModal.addParam("Value", html, "post");
    oAutomationModal.addParam("toolbarType", oAutomationMemo.toolbarType, "post");
    oAutomationModal.addParam("showOnlyMerged", oAutomationMemo.showOnlyMerged, "post");

    oAutomationModal.ErrorCallBack = launchInContext(oAutomationModal, oAutomationModal.hide);

    oAutomationModal.onIframeLoadComplete = function () { oAutomationModal.getIframe().onFrameSizeChange(lWidth, lHeight - 40); };
    oAutomationModal.show();

    oAutomationModal.addButton(top._res_29, oAutomationModal.hide, "button-gray", "oAutomationMemo"); // Annuler
    oAutomationModal.addButton(top._res_28,
        function () {
            target.innerHTML = oAutomationModal.getIframe().eMemoDialogEditorObject.getData().replace(/<br \/>/g, '').replace(/\n|\r|(\n\r)/g, ""); oAutomationModal.hide();
        }, "button-green", "oAutomationMemo"); // Valider

}

//Charger les types de compagne
function ChangeMediaTypeSelect(sender) {

    var selectedMediaType = sender.options[sender.selectedIndex].value;

    var oInvitManager = new eUpdater("mgr/eInvitWizardManager.ashx", 0);
    oInvitManager.ErrorCallBack = function () { };
    oInvitManager.addParam("bkm", 0, "post");
    oInvitManager.addParam("action", 7, "post"); //Pour Rafraichir la liste des type de campagne
    oInvitManager.addParam("mediaType", selectedMediaType, "post");

    setWait(true);
    oInvitManager.send(ReloadCampaignTypeSelect);
    if (selectedMediaType == 0) {
        var divcalcul = document.getElementById('dvCampaignConsentStatus');
        if (divcalcul)
            divcalcul.style.display = 'none';
        let score = 0;
        var divcampaignscore = document.getElementById('campaignTypeScoreValue');
        let classToRemove = divcampaignscore.parentElement.classList[1];
        if (classToRemove) {
            divcampaignscore.parentElement.classList.remove(classToRemove);
            divcampaignscore.innerHTML = score;
        }
    }

}

//remplir la liste déroulante type de compagne
function ReloadCampaignTypeSelect(oRes) {

    try {
        var selectCampaignType = document.getElementById("selectCampaignType");

        if (selectCampaignType != null) {
            for (var i = 0; i < selectCampaignType.options.length;) {
                if (selectCampaignType.options[i].value != "0")
                    selectCampaignType.remove(i);
                else
                    ++i;
            }

            var campaignTypesNode = oRes.getElementsByTagName("campaignTypes")[0];
            if (campaignTypesNode != null) {
                var campaignTypeNodes = campaignTypesNode.getElementsByTagName("campaignType");

                for (var i = 0; i < campaignTypeNodes.length; ++i) {
                    var campaignTypeNode = campaignTypeNodes[i];

                    var option = document.createElement("option");
                    option.text = getXmlTextNode(campaignTypeNode.getElementsByTagName("label")[0]);
                    option.value = getXmlTextNode(campaignTypeNode.getElementsByTagName("value")[0]);
                    selectCampaignType.add(option);
                }
            }

        }
    }
    catch (e) {
    }
    finally {
        setWait(false);
    }

}

//Filtrer les templates Eudonet et la dropdown "Thématique" selon le "Secteur d'activités" sélectionné
function FilterThemeOnActivitySelect(select) {
    //Réinitialiser la visibilité des options de la liste
    var theme = document.getElementById('selectTheme');
    var themesOpts = theme.options;
    for (var y = 1; y < themesOpts.length; y++) {
        themesOpts[y].setAttribute('style', 'display : none');
    }

    var activitySectorValue = select.options[select.selectedIndex].value;
    var themeValue = theme.options[theme.selectedIndex].value;

    var themesToShowArray = [];

    var tpls = document.querySelectorAll("#tblTemplates div");
    for (var i = 0; i < tpls.length; i++) {
        if (tpls[i].className.indexOf('tplElement') < 0)
            continue;
        var activitySectrAttr = getAttributeValue(tpls[i], "activity");
        var activitySectrArray = activitySectrAttr ? activitySectrAttr.split(";") : [];
        var themeAttr = getAttributeValue(tpls[i], "theme");
        var themeArray = themeAttr ? themeAttr.split(";") : [];

        if (activitySectorValue == "0" && themeValue == "0")
            tpls[i].style.display = 'block';
        else if (activitySectorValue != "0" && themeValue != "0") {
            if (activitySectrArray.indexOf(activitySectorValue) > -1 && themeArray.indexOf(themeValue) > -1)
                tpls[i].style.display = 'block';
            else
                tpls[i].style.display = 'none';
        }
        else if (activitySectorValue != "0" && themeValue == "0") {
            if (activitySectrArray.indexOf(activitySectorValue) > -1)
                tpls[i].style.display = 'block';
            else
                tpls[i].style.display = 'none';
        }
        else if (activitySectorValue == "0" && themeValue != "0") {
            if (themeArray.indexOf(themeValue) > -1)
                tpls[i].style.display = 'block';
            else
                tpls[i].style.display = 'none';
        }

        if (tpls[i].style.display == 'block')
            Array.prototype.push.apply(themesToShowArray, themeArray);
    }

    //Remove duplicates
    themesToShowArray = themesToShowArray.filter(function (item, index) {
        return themesToShowArray.indexOf(item) === index;
    });


    if (themesToShowArray.length > 0)
        for (var j = 0; j < themesToShowArray.length; j++) {
            var optionToShow = theme.querySelector('[value="' + themesToShowArray[j] + '"]');
            if (optionToShow)
                optionToShow.setAttribute('style', 'display : block');
        }

    //Désélectionner un modèle s'il ne figure pas dans les résultats de filtres "Secteur d'activité" et "Thématique"
    var selectedTemplate = document.getElementsByClassName("graphCadreSel")[0];
    if (selectedTemplate && selectedTemplate.style.display == 'none') {
        selectedTemplate.classList.remove("graphCadreSel");
        oTemplate.id = 0;
        oTemplate.type = 0;
    }
}

//Retirer les options du "Secteur d'activité" pour lesquelles il n'y a aucun résultat
function RemoveNoModelsThemeOptions() {
    var theme = document.getElementById('selectTheme');
    var tpls = document.querySelectorAll("#tblTemplates div");
    let optionsToShow = [];
    let optionsToHide = [];
    for (var i = 0; i < tpls.length; i++) {
        if (tpls[i].className.indexOf('tplElement') < 0)
            continue;
        var themeAttr = getAttributeValue(tpls[i], "theme");
        var themeArray = themeAttr ? themeAttr.split(";") : [];

        for (var j = 1; j < theme.length; j++) {
            var option = theme.options[j];
            var optValue = option.value;
            if (themeArray.includes(optValue))
                optionsToShow.push(option);
            else
                optionsToHide.push(option);
        }
    }

    //Remove duplicates
    optionsToHide = optionsToHide.filter(function (item, index) {
        return optionsToHide.indexOf(item) === index;
    });

    optionsToShow = optionsToShow.filter(function (item, index) {
        return optionsToShow.indexOf(item) === index;
    });


    optionsToHide.forEach(el => el.style.display = 'none');
    optionsToShow.forEach(el => el.style.display = 'block');
}

//Filtrer les templates Eudonet et la dropdown "Secteur d'activités" sur la "Thématique" sélectionnée
function FilterActivityOnThemeSelect(select) {
    //Réinitialiser la visibilité des options de la liste
    var activitySector = document.getElementById('selectActivitySector');
    var activitySectorOpts = activitySector.options;
    for (var y = 1; y < activitySectorOpts.length; y++) {
        activitySectorOpts[y].setAttribute('style', 'display : none');
    }

    var themeValue = select.options[select.selectedIndex].value;
    var activitySectorValue = activitySector.options[activitySector.selectedIndex].value;

    var activitySectorsToShowArray = [];

    var tpls = document.querySelectorAll("#tblTemplates div");
    for (var i = 0; i < tpls.length; i++) {
        if (tpls[i].className.indexOf('tplElement') < 0)
            continue;
        var themeAttr = getAttributeValue(tpls[i], "theme");
        var themeArray = themeAttr ? themeAttr.split(";") : [];
        var activitySectrAttr = getAttributeValue(tpls[i], "activity");
        var activitySectrArray = activitySectrAttr ? activitySectrAttr.split(";") : [];

        if (activitySectorValue == "0" && themeValue == "0")
            tpls[i].style.display = 'block';
        else if (activitySectorValue != "0" && themeValue != "0") {
            if (activitySectrArray.indexOf(activitySectorValue) > -1 && themeArray.indexOf(themeValue) > -1)
                tpls[i].style.display = 'block';
            else
                tpls[i].style.display = 'none';
        }
        else if (activitySectorValue != "0" && themeValue == "0") {
            if (activitySectrArray.indexOf(activitySectorValue) > -1)
                tpls[i].style.display = 'block';
            else
                tpls[i].style.display = 'none';
        }
        else if (activitySectorValue == "0" && themeValue != "0") {
            if (themeArray.indexOf(themeValue) > -1)
                tpls[i].style.display = 'block';
            else
                tpls[i].style.display = 'none';
        }

        if (tpls[i].style.display == 'block')
            Array.prototype.push.apply(activitySectorsToShowArray, activitySectrArray);
    }

    //Remove duplicates
    activitySectorsToShowArray = activitySectorsToShowArray.filter(function (item, index) {
        return activitySectorsToShowArray.indexOf(item) === index;
    });

    //Secteur d'activité
    if (activitySectorsToShowArray.length > 0)
        for (var j = 0; j < activitySectorsToShowArray.length; j++) {
            var optionToShow = activitySector.querySelector('[value="' + activitySectorsToShowArray[j] + '"]');
            if (optionToShow)
                optionToShow.setAttribute('style', 'display : block');
        }

    //Désélectionner un modèle s'il ne figure pas dans les résultats de filtres "Secteur d'activité" et "Thématique"
    var selectedTemplate = document.getElementsByClassName("graphCadreSel")[0];
    if (selectedTemplate && selectedTemplate.style.display == 'none') {
        selectedTemplate.classList.remove("graphCadreSel");
        oTemplate.id = 0;
        oTemplate.type = 0;
    }
}

//Retirer les options du "Thématique" pour lesquelles il n'y a aucun résultat
function RemoveNoModelsActivityOptions() {
    var activitySector = document.getElementById('selectActivitySector');
    var tpls = document.querySelectorAll("#tblTemplates div");
    let optionsToShow = [];
    let optionsToHide = [];
    for (var i = 0; i < tpls.length; i++) {
        if (tpls[i].className.indexOf('tplElement') < 0)
            continue;
        var activitySectrAttr = getAttributeValue(tpls[i], "activity");
        var activitySectrArray = activitySectrAttr ? activitySectrAttr.split(";") : [];

        for (var j = 1; j < activitySector.length; j++) {
            var option = activitySector.options[j];
            var optValue = option.value;
            if (activitySectrArray.includes(optValue))
                optionsToShow.push(option);
            else
                optionsToHide.push(option);
        }
    }

    //Remove duplicates
    optionsToHide = optionsToHide.filter(function (item, index) {
        return optionsToHide.indexOf(item) === index;
    });

    optionsToShow = optionsToShow.filter(function (item, index) {
        return optionsToShow.indexOf(item) === index;
    });


    optionsToHide.forEach(el => el.style.display = 'none');
    optionsToShow.forEach(el => el.style.display = 'block');
}

function FilterTargetCampaign() {

    //Type de campagne
    var oCampaignType = document.getElementById('selectCampaignType');
    var oCampaignTypeValue = oCampaignType.options[oCampaignType.selectedIndex].value;


    if (oCampaignTypeValue != 0) {

        var divcalcul = document.getElementById('dvCampaignConsentStatus');
        divcalcul.style.display = 'inline';
    }
    else {
        var divcalcul = document.getElementById('dvCampaignConsentStatus');
        divcalcul.style.display = 'none';
    }


    var upd = new eUpdater("mgr/eMailingWizardManager.ashx", 0);

    upd.ErrorCallBack = function () { top.setWait(false) };

    //Type de média
    var oMediaType = document.getElementById('campaginSelectMediaType');
    if (oMediaType != null) {
        var oMediaTypeValue = oMediaType.options[oMediaType.selectedIndex].value;
        if (oMediaTypeValue != "" && oMediaTypeValue != "0") {
            upd.addParam("fltmediatype", oMediaTypeValue, "post");
        }
    }

    if (oCampaignType != null) {
        //var oCampaignTypeValue = oCampaignType.options[oCampaignType.selectedIndex].value;
        if (oCampaignTypeValue != "" && oCampaignTypeValue != "0") {
            upd.addParam("fltcampaigntype", oCampaignTypeValue, "post");
            oMailing._campaignTypeScoreCalculated = true;
        }
    }

    upd.addParam("campaignid", oMailing._mailingId, "post");
    upd.addParam("tab", oMailing._tab, "post");
    upd.addParam("parenttabid", oMailing._nParentTabId, "post");
    upd.addParam("parentfileid", oMailing._nParentFileId, "post");

    upd.addParam("removedoubles", 0, "post");

    //Ajout des filtre   de la campagne
    var filters = {
        //Dans le cas où le consentement est désactivé
        optin: !document.getElementById("swcampaignSwOptin") || document.getElementById("swcampaignSwOptin").checked ? 1 : 0,
        optout: !document.getElementById("swcampaignSwOptout") || document.getElementById("swcampaignSwOptout").checked ? 1 : 0,
        noconsent: !document.getElementById("swcampaignSwNoopt") || document.getElementById("swcampaignSwNoopt").checked ? 1 : 0,


        valid: document.getElementById("swqualityAdressEmailSwValide").checked ? 1 : 0,
        notverified: document.getElementById("swqualityAdressEmailSwNotVerified").checked ? 1 : 0,
        invalid: document.getElementById("swqualityAdressEmailSwInvalide").checked ? 1 : 0,
        removeDoubles: document.getElementById("swcampaignSwDedoublonnage").checked ? 1 : 0,
    }

    upd.addParam("mailfilters", JSON.stringify(filters), "post");


    //Ajout des paramètres de la campagne
    for (var index in oMailing._aMailingParams) {
        if (
            index != "operation"
            && index != "removedoubles"
            && index != "parentfileid"
            && index != "parenttabid"
            && index != "campaignid"
            && index != "body"
            && index != "boddyCss"
        )
            upd.addParam(index, oMailing._aMailingParams[index], "post");
    }

    top.setWait(true)
    upd.send(function (oRes) {
        top.setWait(false);
        updReturnPPList(oRes);

    });
}

var firstTime = false;

function updReturnPPList(oRes) {

    var failure = getXmlTextNode(oRes.getElementsByTagName("success")[0]) != "1";
    if (!failure) {

        //Type de campagne
        var oCampaignType = document.getElementById('selectCampaignType');
        var oCampaignTypeValue = oCampaignType.options[oCampaignType.selectedIndex].value;

        var optin = getXmlTextNode(oRes.getElementsByTagName("nOptin")[0]);
        var optout = getXmlTextNode(oRes.getElementsByTagName("nOptout")[0]);
        var Noconsent = getXmlTextNode(oRes.getElementsByTagName("nNoconsent")[0]);

        if (document.getElementById("spnNbrDestcampaignSwOptin"))
            document.getElementById("spnNbrDestcampaignSwOptin").innerHTML = optin;
        if (document.getElementById("spnNbrDestcampaignSwNoopt"))
            document.getElementById("spnNbrDestcampaignSwNoopt").innerHTML = Noconsent;
        if (document.getElementById("spnNbrDestcampaignSwOptout"))
            document.getElementById("spnNbrDestcampaignSwOptout").innerHTML = optout;


        var invalid = getXmlTextNode(oRes.getElementsByTagName("invalid")[0]);
        var valid = getXmlTextNode(oRes.getElementsByTagName("valid")[0]);
        var notverifierd = getXmlTextNode(oRes.getElementsByTagName("notchecked")[0]);


        document.getElementById("spnNbrDestqualityAdressEmailSwValide").innerHTML = valid;
        document.getElementById("spnNbrDestqualityAdressEmailSwNotVerified").innerHTML = notverifierd;
        document.getElementById("spnNbrDestqualityAdressEmailSwInvalide").innerHTML = invalid;

        var total = getXmlTextNode(oRes.getElementsByTagName("totalRecepient")[0]);
        document.getElementById("spnToTalRecepientValue").innerHTML = total;
        var totalWithoutDouble = getXmlTextNode(oRes.getElementsByTagName("totalRecepientWithoutDoubles")[0]);
        document.getElementById("spnTotalRecipientAfterRemoveDoublontValue").innerHTML = totalWithoutDouble;

        //Status des switchs
        var swtchInputOptin = document.getElementById("swcampaignSwOptin");
        var swtchInputOptout = document.getElementById("swcampaignSwOptout");
        var swtchInputNoopt = document.getElementById("swcampaignSwNoopt");

        var swtchInputValid = document.getElementById("swqualityAdressEmailSwValide");
        var swtchInputInvalid = document.getElementById("swqualityAdressEmailSwInvalide");
        var swtchInputNotChecked = document.getElementById("swqualityAdressEmailSwNotVerified");

        var swtchInputRemoveDoubles = document.getElementById("swcampaignSwDedoublonnage");

        let scoreCampaignType = 0;
        let scoreEmaiilQuality = 0;
        let scoreRemoveDoubles = 0;


        if (swtchInputInvalid.checked) {
            //invalid score )
            scoreEmaiilQuality = 0;
        }
        else {


            if (swtchInputValid.checked && !swtchInputNotChecked.checked) {
                // seulement valide
                scoreEmaiilQuality = 25;
            } else if (swtchInputNotChecked.checked && !swtchInputValid.checked) {
                //seulement non vérifié
                scoreEmaiilQuality = 10;
            } else if (swtchInputValid.checked && swtchInputNotChecked.checked) {

                if ((valid + notverifierd) == 0) {
                    scoreEmaiilQuality = 10;
                }
                else
                    scoreEmaiilQuality = Math.round(((valid * 1 / (valid * 1 + notverifierd * 1)) * 25))

                if (scoreEmaiilQuality < 10)
                    scoreEmaiilQuality = 10
            }
        }

        if (!swtchInputOptout && oCampaignTypeValue == 0)
            scoreCampaignType = 0;
        else if (!swtchInputOptout && oCampaignTypeValue != 0)
            scoreCampaignType = 25;
        else if (swtchInputOptout.checked || oCampaignTypeValue == 0)
            scoreCampaignType = 0;
        else {
            if (swtchInputOptin.checked && swtchInputNoopt.checked) {
                if (Noconsent != 0 && optin != 0)
                    scoreCampaignType = Math.round(((optin * 1 / (optin * 1 + Noconsent * 1)) * 25))
                else if (Noconsent == 0 && optin == 0)
                    scoreCampaignType = 0;
                else if ((Noconsent == 0 && optin != 0))
                    scoreCampaignType = 25;

                if (scoreCampaignType < 10)
                    scoreCampaignType = 10
            }
            else if (swtchInputOptin.checked || !swtchInputNoopt.checked) {
                scoreCampaignType = 25;
            } else if (!swtchInputOptin.checked || swtchInputNoopt.checked) {
                scoreCampaignType = 10;
            }
        }

        if (swtchInputRemoveDoubles.checked)
            scoreRemoveDoubles = 25;
        else
            scoreRemoveDoubles = 0;
        //chargement de score
        oMailing.SetScore(
            {
                qualityEmailAdresses: scoreEmaiilQuality,
                campaignType: scoreCampaignType,
                recepientCount: scoreRemoveDoubles
            }
        )
    }
}

document.addEventListener("DOMContentLoaded", function (event) {
    RemoveNoModelsThemeOptions();
    RemoveNoModelsActivityOptions();
});
