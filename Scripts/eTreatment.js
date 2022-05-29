/// <reference path="eTools.js" />

/* Variables globales nécessaires au traitment */
var calpopup;
var advancedDialog;
var oFieldInfos;


/*Constante*/
var SEPARATOR_LVL1 = "#|#";
var SEPARATOR_LVL2 = "#$#";

var CST_INPT_VISIBLE = "action_update_withnew_value_vis";
var CST_INPT_VISIBLE_REMOVE = "action_update_withnew_value_vis_remove";

var nsTreatment = {};


///Correspondance JS de l'enum CS TraitementOperation de IEudoTreatmentWCF
// A METTRE A JOUR EN PARALLELE
nsTreatment.TraitementOperation = {

    /// <summary>Pas de traitement sélectionné (pas possible)</summary>
    NONE: 0,
    /// <summary>Affecter une nouvelle fiche</summary>
    AFFECT_FILE: 1,
    /// <summary>Mettre à jour la rubrique</summary>
    UPDATE: 2,
    /// <summary>Suppression</summary>
    DELETE: 3,
    /// <summary>Duplication</summary>
    DUPLICATE: 4,
    /// <summary>Vérification de l'état d'avancement du traitement</summary>
    CHECK: 5,
    /// <summary>Création des invitations</summary>
    INVIT_CREA: 6,
    /// <summary>Création des invitations en mode bulk</summary>
    INVIT_CREA_BULK: 7,    /// <summary>Créer temporairement le temps d'avoir une interface de duplication de masse (A suppr quand celle-ci sera plugger)</summary>

    /// <summary>Suppression des invitations en mode bulk</summary>
    INVIT_SUP_BULK: 8,

    /// <summary>Suppression d'invitation requete par requete</summary>
    INVIT_SUP: 9,

    ///
    DUPLICATE_TEMP_VIEW: 99

};

/// <summary>
/// Mise à jour du champ caché de valeur
/// </summary>
///<Author>SPH</Author>
function updateHiddenValue(sValue, nFromRadio) {
    var oRadio = document.getElementById("action_update_" + nFromRadio);
    oRadio.checked = true;
    var inptValueHidden = document.getElementById("action_update_withnew_value");
    inptValueHidden.value = sValue;
}


/************************************************/
/*           DATES                               */
/************************************************/

/// <summary>
/// Appel du catalogue date
/// </summary>
///<Author>SPH</Author>
function selectGADate(date) {
    calpopup = createCalendarPopUp("ValidSelectGADate", "0", "0", top._res_5017, top._res_5003, "SelectDateOk", top._res_29, null, null, oIframe.id, null, date);
}


/// <summary>
/// Action de validation du date picker
/// </summary>
///<Author>SPH</Author>
function ValidSelectGADate(date) {



    var inptVisi = document.getElementById(CST_INPT_VISIBLE);
    updateHiddenValue(eDate.ConvertDisplayToBdd(date), "dateFixed");
    inptVisi.value = date;
    calpopup.hide();

}

//Option décaler une date
function updateDateDecal() {

    var oChk = getCheckedRadio("action_update", "mainDiv");
    if (oChk.value == "withnewdate") {
        var oDecalValue = document.getElementById("action_update_withnew_value_val");
        var oDecalType = document.getElementById("action_update_withnew_value_type");

        if (oDecalValue && oDecalType) {
            if (!isNumeric(oDecalValue.value)) {
                oDecalValue.value = "1";
                eAlert(0, top._res_295, top._res_673);
                oDecalValue.focus();
                return;
            }

            if ((oDecalValue.value) == 0) {
                oDecalValue.value = "1";
                eAlert(0, top._res_295, top._res_673);
                oDecalValue.focus();
                return;
            }

            var inptValueHidden = document.getElementById("action_update_withnew_value");
            updateHiddenValue(oDecalValue.value + SEPARATOR_LVL1 + oDecalType.options[oDecalType.selectedIndex].value, "dateDecal");
        }
    }
    else {
        var oValue = document.getElementById(CST_INPT_VISIBLE);
        var inptValueHidden = document.getElementById("action_update_withnew_value");
        updateHiddenValue(oValue.value, "dateFixed");
    }
}
/*  Fin des fonctions de dates              */
/************************************************/

/************************************************/
/*           Cases à cocher                     */
/************************************************/

function updateBit() {
    var inptValueHidden = document.getElementById("action_update_withnew_value");
    var oBit = document.getElementById("action_update_withnew_value_type");
    updateHiddenValue(oBit.options[oBit.selectedIndex].value, "newvalue");
}

/************************************************/
/*           Fin de Cases à cocher              */
/************************************************/


/************************************************/
/*           Catalogues                         */
/************************************************/
/// <summary>
/// Appel la fenêtre de catalogue
/// </summary>
///<Author>SPH</Author>
function openCat() {
    // récupération des informations de cataloges
    if (!oFieldInfos)
        return;

    //On vérifie quelle radio a été selectionné.

    var allOptions = document.querySelectorAll("input[type='radio'][name='action_update']");
    var myOption = null;
    if (allOptions.length > 0) {
        for (var nI = 0; nI < allOptions.length; nI++) {
            if (allOptions[nI].checked) {
                myOption = allOptions[nI];
                break;
            }
        }
    }

    var bRemove = ((myOption != null) && (myOption.value == "removevalue"));






    var oTarget = document.getElementById(bRemove ? CST_INPT_VISIBLE_REMOVE : CST_INPT_VISIBLE);
    var defValue = document.getElementById("action_update_withnew_value").value;

    showCatGeneric((oFieldInfos.getAttribute("multiple") == "1" && !bRemove), (oFieldInfos.getAttribute("treeview") == "1"), defValue, null
        , oTarget.id, oFieldInfos.getAttribute("popupdescid"), oFieldInfos.getAttribute("popup"), ""
        , "", null, "catalogue", "advancedDialog"
        , false, partOfValidateCatDlg_Trt
        );  //TODORES
}
//Catalogues : Partie de code éxécuté au clique sur valider juste avant la fermeture du catalogue avancé
function partOfValidateCatDlg_Trt(catalogDialog, srcId, trgId, tabSelectedLabels, tabSelectedValues, selectedIDs) {

    var inptVisi = document.getElementById(trgId);

    var nRadio = "newvalue";
    if (trgId == CST_INPT_VISIBLE_REMOVE)
        nRadio = "removevalue";

    updateHiddenValue(joinString(";", tabSelectedValues), nRadio);
    inptVisi.value = joinString(";", tabSelectedLabels);
}
/************************************************/
/*  Fin des fonctions de catalogues             */
/************************************************/

/************************************************/
/*  Champs de liaisons                          */
/************************************************/

function openLnkFile() {

    // récupération des informations de cataloges
    if (!oFieldInfos)
        return;

    var oTarget = document.getElementById(CST_INPT_VISIBLE);

    if (!top.eLinkCatFileEditorObject)  //N'est pas initialisé si l'on vient de la HomePage
        top.eLinkCatFileEditorObject = new eFieldEditor('linkCatFileEditor', ePopupObject, 'eLinkCatFileEditorObject', 'eLinkCatFileEditor');
    //    var SEPARATOR_LVL1 = "#|#";
    //    var SEPARATOR_LVL2 = "#$#";
    //    var paramSup = "AllRecord" + SEPARATOR_LVL2 + "1" + SEPARATOR_LVL1 + "EnabledSearchAll" + SEPARATOR_LVL2 + "1";

    top.eLinkCatFileEditorObject.sourceElement = window;
    top.eLinkCatFileEditorObject.openLnkFileDialog(FinderSearchType.Link, getTabDescid(oFieldInfos.getAttribute("popupdescid")), false, onOkLnkFile); //  2 : Associer
}


function onOkLnkFile(iframeId) {
    var catalogObject = top.eTabLinkCatFileEditorObject[iframeId];

    var oFrm = top.document.getElementById(iframeId);
    var oFrmDoc = oFrm.contentDocument;
    var oFrmWin = oFrm.contentWindow;
    var selectedListValues = oFrmWin._selectedListValues;
    if (typeof (selectedListValues) == 'undefined')
        selectedListValues = new Array();

    //REMISE A ZERO
    catalogObject.selectedValues = new Array();
    catalogObject.selectedLabels = new Array();
    if (selectedListValues.length > 0)
        for (var i = 0; i < selectedListValues.length; i++) {
            var oItem = oFrmDoc.getElementById(selectedListValues[i]);

            if (!oItem)
                continue;
            // id  
            var oId = oItem.getAttribute("eid").split('_');
            var nTab = oId[0];
            var nId = oId[oId.length - 1];

            var tabTd = oItem.getElementsByTagName("td");
            var label = "";
            //NameOnly
            label = GetText(tabTd[0]); //Libellé de la première colonne
            catalogObject.selectValue(nId, label, true);
        }
    else
        catalogObject.selectValue("", "", true);


    /*DISPLAY VALUE*/
    var oTarget = catalogObject.sourceElement.document.getElementById(CST_INPT_VISIBLE);
    oTarget.value = catalogObject.selectedLabels[0];
    oTarget.setAttribute("ednvalue", catalogObject.selectedValues[0]);
    /*CLOSE CAT*/
    catalogObject.oModalLnkFile.hide();
    /*DBVALUE*/
    var inptValueHidden = catalogObject.sourceElement.document.getElementById("action_update_withnew_value");
    catalogObject.sourceElement.updateHiddenValue(catalogObject.selectedValues[0], "newvalue");
}

/************************************************/
/*  Fin des fonctions de champs de liaisons     */
/************************************************/
/************************************************/
/*  Catalogue utilisateurs multiple     */
/************************************************/
var modalUserCat;

function openUserCat() {
    modalUserCat = new eModalDialog(top._res_246, 0, "eCatalogDialogUser.aspx", 550, 610);
    top.eTabCatUserModalObject.Add(modalUserCat.iframeId, modalUserCat);
    modalUserCat.addParam("iframeId", modalUserCat.iframeId, "post");
    modalUserCat.ErrorCallBack = function () { setWait(false); }
    modalUserCat.addParam("multi", "1", "post");
    var defValue = document.getElementById("action_update_withnew_value").value;
    modalUserCat.addParam("selected", defValue, "post");
    modalUserCat.addParam("modalvarname", "modalUserCat", "post");
    modalUserCat.show();
    modalUserCat.addButton(top._res_29, onUserCatCancel, "button-gray", null, null, true);
    modalUserCat.addButton(top._res_28, onUserCatOk, "button-green");
}

function onUserCatOk() {


    var strReturned = modalUserCat.getIframe().GetReturnValue();
    modalUserCat.hide();
    var vals = strReturned.split('$|$')[0];
    var libs = strReturned.split('$|$')[1];

    var inptVisi = document.getElementById(CST_INPT_VISIBLE);
    updateHiddenValue(vals, "newvalue");
    inptVisi.value = libs;

}

function onUserCatCancel() {
    modalUserCat.hide();
}

/************************************************/
/*  Fin catalogue utilisateurs multiple     */
/************************************************/

/// <summary>
/// Dispatch l'action sur le click des boutons
//  Nécessaire pour la désactivation des boutons
/// </summary>
///<Author>SPH</Author>
function doAction(obj) {



    if (obj.getAttribute("dis") != null && obj.getAttribute("dis") == 1)
        return;

    var sAction = obj.getAttribute("action");


    if (sAction && sAction.length > 0) {
        switch (sAction) {
            case "LNKCAT":
                openCat();
                break;
            case "LNKFILE":
                openLnkFile();
                break;
            case "LNKUSER":
                if (oFieldInfos.getAttribute("multiple") != "1")
                    GADistribUsr();
                else

                    openUserCat();

                break;
            case "LNKDATE":
                var date = document.getElementById(CST_INPT_VISIBLE).value;
                selectGADate(date);
                break;
            case "LNKNUM":
                //GCH - Internationalisation Numériques - 36869, 37688
                var inptVisi = document.getElementById(CST_INPT_VISIBLE);
                if (inptVisi != null)
                    updateHiddenValue(eNumber.ConvertDisplayToBdd(inptVisi.value), "newvalue");
                break;
            case "LNKMAIL": // CRU : gestion des champs email
            case "LNKTEXT":  // ASY : ajout de la recopie de la valeur pour les champs de type texte
                selectGAText();
                break;
            default:
                return;
                break;
        }
    }
}

// ASY recuperation du champs Texte Saisi
function selectGAText() {

    //var txtValue = document.getElementById("action_update_withnew_value").value;
    var inptVisi = document.getElementById(CST_INPT_VISIBLE);
    if (inptVisi != null)
        updateHiddenValue(inptVisi.value, "newvalue");
}



/************************************************/
//REGION REPARTITION USER

/************************************************/
var oModGADisUsr;

/// <summary>
/// Appel l'écran de sélection des users
/// </summary>
///<Author>KHA</Author>
function GADistribUsr() {


    // Valeur déjà choisies
    var initValue = document.getElementById("action_update_withnew_value").value;
    var oTrt = document.getElementById("trtAttributes");
    var bAllFile = document.getElementById("selection_all").checked;
    var nbFiles = getAttributeValue(oTrt, "enbfiles");
    var lstField = document.getElementById("action_update_value");
    var nDescId = lstField.options[lstField.options.selectedIndex].value;



    oModGADisUsr = new eModalDialog(top._res_932, 0, "eGlobalAffectOwner.aspx", 500, 650, "oModGADisUsr");


    oModGADisUsr.addParam("descid", nDescId, "post"); // fournir le descid // gérer l'option prendre en compte tous les user et gpes
    oModGADisUsr.addParam("initvalues", initValue, "post"); // valeurs initials
    oModGADisUsr.addParam("nbfiles", nbFiles, "post"); // Nombres de fiches à traiter
    oModGADisUsr.addParam("all", bAllFile ? "1" : "0", "post"); // toutes les fiches ou seulement celle sélectionné


    oModGADisUsr.show();

    oModGADisUsr.addButtonFct(top._res_29, function () { oModGADisUsr.hide(); }, 'button-green');
    oModGADisUsr.addButtonFct(top._res_28, function () { validGADistrib(); oModGADisUsr.hide(); }, 'button-gray');


    oModGADisUsr.onIframeLoadComplete = function () {


        var oFrm = top.document.getElementById(oModGADisUsr.iframeId);

        //oFrm.style.height = "130%";
    }
}


/// <summary>
/// Interprète le retour du manager pour mettre à jour les champs de valeurs
/// </summary>
///<Author>SPH</Author>
function validGADistrib() {
    try {
        var sResult = oModGADisUsr.getIframe().getDistribution();


        var sRealValue = "";
        var sDisplayValue = "";

        if (sResult && sResult.length > 0) {


            var aResult = sResult.split("$|$");

            for (var cmptRes = 0; cmptRes < aResult.length; cmptRes++) {
                var sRes = aResult[cmptRes];
                var oRes = sRes.split(":");
                if (oRes.length == 3) {
                    var nUserId = oRes[0];
                    var sUserName = oRes[1];
                    var nAffect = oRes[2];

                    if (sUserName.length > 0 && isNumeric(nUserId) && isNumeric(nAffect) && nAffect > 0 && nUserId > 0) {

                        if (sRealValue.length > 0)
                            sRealValue += "$|$";
                        sRealValue += nUserId + ':' + nAffect;

                        if (sDisplayValue.length > 0)
                            sDisplayValue += ";"
                        sDisplayValue += sUserName;


                    }

                }


            }

            var inptVisi = document.getElementById(CST_INPT_VISIBLE);

            updateHiddenValue(sRealValue, "newvalue");
            inptVisi.value = sDisplayValue;
        }
    }
    catch (e) {
        //Display error msg
    }

    oModGADisUsr.hide();


}

/************************************************/
//ENDREGION REPARTITION USER

/************************************************/

/*************************************************/
/*      MISE A JOUR DES OPTIONS DE TRAITEMENT DE CHAMPS             */
/*************************************************/

/// <summary>
/// Appel le manager qui retourne le code html d'un tableau contenant un control field
/// </summary>
///<Author>SPH</Author>
/// <param name="nDescId">Descid du champ</param>
/// <param name="bInit">Indique si des fonctions d'initialisation doivent être lancé. A "true" lors du onload</param>
function onChangeUpdatedField(nDescID, bInit) {


    var ulActions = document.getElementById("ulActions");
    var liOptionUpdate = ulActions.getElementsByTagName("li");
    var nbOptions = liOptionUpdate.length;


    //Suppression des options en cours

    while (nbOptions--) {
        if (liOptionUpdate[nbOptions].getAttribute("ednupdateoption") == "1") {
            ulActions.removeChild(liOptionUpdate[nbOptions]);
        }

    }

    var upd = new eUpdater("mgr/eTreatmentFieldOptionManager.ashx", 0);
    upd.addParam("descid", nDescID, "post");
    upd.addParam("action", "getinfos", "post");


    //Gestion d'erreur : Fermeture de la modal
    upd.ErrorCallBack = function () {

    };

    upd.send(updatefieldoption, bInit);


}


/// <summary>
/// Interprete le retour HTML pour générer les contrôles en js
///</summary>
///<Author>SPH</Author>
/// <param name="oRes">Flux xml réponse contenant les information pour générere les contrôles</param>
/// <param name="bLaunchChange">Indique si des fonctions d'initialisation doivent être lancé. A "true" lors du onload</param>
function updatefieldoption(oRes, bLaunchChange) {

    if (typeof (bLaunchChange) == "undefined")
        bLaunchChange = false;

    var inptValueHidden = document.getElementById("action_update_withnew_value");
    inptValueHidden.value = "";

    if (!oRes)
        return;

    var ulActions = document.getElementById("ulActions");
    var liOptionUpdate = ulActions.getElementsByTagName("li");
    var nbOptions = liOptionUpdate.length;



    //Génération des nouvelles options
    var oLstNewOption = oRes.getElementsByTagName("option");
    var liUpdateOption = document.getElementById("liActionsUpdate");

    oFieldInfos = oRes.getElementsByTagName("fieldInfos")[0];
    var idFirstRadioButton = null;

    for (var i = 0; i < oLstNewOption.length; i++) {
        oNewOption = oLstNewOption[i];

        /* création des LI  */

        var newLI = document.createElement("li");
        var nLvl = oNewOption.getAttribute("optionlevel");

        var isRemoveAction = getAttributeValue(oNewOption, "remove") == "1";

        newLI.className = "trt_Opt" + nLvl; // Classe CSS
        newLI.setAttribute("ednupdateoption", "1"); //Indique une option



        if (oNewOption.getAttribute("checkbox") != "1") {
            // Radio button
            var radio = document.createElement("input");
            radio.setAttribute("type", "radio");

            radio.setAttribute("id", "action_update_" + oNewOption.getAttribute("radioName"));
            radio.setAttribute("name", "action_update");
            radio.setAttribute("value", oNewOption.getAttribute("action"));


            if (oNewOption.getAttribute("disabled") == "1") {
                radio.setAttribute("disabled", "1");
                radio.setAttribute("readonly", "1");
            }

            var strOnChange = oNewOption.getAttribute("onchange");
            if (strOnChange != null)
                radio.setAttribute("onchange", strOnChange);

            if (idFirstRadioButton == null && oNewOption.getAttribute("disabled") != "1") {
                idFirstRadioButton = radio.id;
            }
            newLI.appendChild(radio);

            //Label de l'action
            var label = document.createElement("label");
            label.setAttribute("for", "action_update_" + oNewOption.getAttribute("radioName"));
            label.innerHTML = oNewOption.getAttribute("label");
            newLI.appendChild(label);
        }
        else {
            var attributes = new Dictionary();
            attributes.Add("name", "action_update");
            var chk = AddEudoCheckBox(document, newLI, false, "action_update_" + oNewOption.getAttribute("radioName"), oNewOption.getAttribute("label"), attributes);
        }



        var optionControl = oNewOption.getElementsByTagName("optionControl");

        for (var j = 0; j < optionControl.length; j++) {
            var txt = (getXmlTextNode(optionControl[j]));
            newLI.innerHTML += txt;
        }

        //Ajoute le li
        ulActions.insertBefore(newLI, liUpdateOption.nextSibling);

        //Placement du prochain li
        liUpdateOption = newLI;
    }

    if (idFirstRadioButton != null && idFirstRadioButton != "" && document.getElementById(idFirstRadioButton) != null) {


        var firstRadioButton = document.getElementById(idFirstRadioButton)
        firstRadioButton.checked = true;
        if (typeof (firstRadioButton.onchange) == "function")
            firstRadioButton.onchange();
    }


    // Pour le chargement initial, il faut initialiser les disabled
    if (bLaunchChange) {
        var nodeCheckedAction = getCheckedRadio("action", "mainDiv");
        var sAction = nodeCheckedAction.id;
        onChange(sAction);
    }



}

/*************************************************/
/*  Fin des fonctions de mise à jour des options */
/*************************************************/


/*  actions a réalisé au chargement initial */
function initLoad() {

    // Maj du type de field a maj -> action disponible
    var oLstFields = document.getElementById("action_update_value");
    var nDescId = oLstFields.options[oLstFields.options.selectedIndex].value
    onChangeUpdatedField(nDescId, true);


}


function onChange(strEltId) {


    var oSelectedElt = document.getElementById(strEltId);



    if (!oSelectedElt)
        return;

    //Indique si le déclencheur est un changement de type
    var bTypeAction = (oSelectedElt.name == "action");


    /****************************************/
    /*  Partie  "Echantillon aléatoire"      */
    /****************************************/
    // Active/désactive le input du nombre de fiche à affecter
    document.getElementById(inputRandomSampleSelectCount).disabled = (document.getElementById(chkRandomSampleSelect).getAttribute('chk') != '1');


    /****************************************/
    /*  Partie  "Actions                    */
    /****************************************/
    // Option "Ajouter une nouvelle fiche : 
    // Active/Désactive le select du choix de la fiche à créer
    document.getElementById(selectActionsNew).disabled = (document.getElementById(rbActionsNew).checked == false);


    // Option "Mettre à jour la rubrique
    // Active/Désactive le select du choix des rubriques à mettre à jour
    document.getElementById(selectActionsUpdate).disabled = (document.getElementById(rbActionsUpdate).checked == false);


    //Passe en disabled les champs d'options de "mise à jour de champ si
    // le type d'action n'est pas mise à jour
    if (bTypeAction) {

        var sTypeAction = getCheckedRadio("action", "mainDiv").value;
        var oLst = document.querySelectorAll("li[ednupdateoption='1'] *");

        for (var nInpt = 0; nInpt < oLst.length; nInpt++) {
            if (!oLst[nInpt].getAttribute("readonly")) {
                oLst[nInpt].disabled = (sTypeAction != "update");
                if (oLst[nInpt].disabled)
                    oLst[nInpt].checked = false;
            }

            oLst[nInpt].setAttribute("dis", (sTypeAction == "update") ? "0" : "1");
        }
    }


    /* option "supprimer" et "dupliquer"    */
    // désactive la sélection aléatoire
    if ((document.getElementById(rbActionsDuplicate) && document.getElementById(rbActionsDuplicate).checked) || (document.getElementById(rbActionsDelete) && document.getElementById(rbActionsDelete).checked)) {
        setAttributeValue(document.getElementById(chkRandomSampleSelect), 'dis', '1');
        setAttributeValue(document.getElementById(chkRandomSampleSelect), 'chk', '0');
        if ((strEltId == rbActionsDuplicate) || (strEltId == rbActionsDelete))
            onChange(chkRandomSampleSelect);
    }
    else {
        setAttributeValue(document.getElementById(chkRandomSampleSelect), 'dis', '0');
    }
}


//REGION #TREATMENT#

//CONSTANTES
var nTimeOutTreatment = (1000 * 60 * 5); //TimeOut de 5 minutes avant arrêt forcé de l'attente de l'export (variables en millisecondes)
var nCheckTimeOutTreatment = (1000 * 1); //TimeOut de 1 secondes entre chaque appels avant la vérifications de l'état du rapport
//Variables globales
var nServerTreatmentId = -1;   //Identifiant du traitement en cours
var fctTreatmentTimeOut = null;    //timeout de fonction
var fctTreatmentTimeOutCheck = null;    //timeout de fonction
// Alert de demande d'attente ou informe l'utilisatrice que la demande de traitement va démarer
var alertStartTreatment = null;
// Alert de demande d'attente ou informe l'utilisatrice que la demande d'export a été pris en compte
var alertAttentTreatment = null;
var user_address = "";  //Adresse e-mail de l'utilisateur
var nTabFromTreatment = 0;
/*Suppression seulement :****/
var bDeletePp = false;   //si la table ciblée est PP ou PM on a l'option qui demande de supprimer les fichiers liés
var bDeleteAdr = false;   //si la table ciblée est PP ou PM on a l'option qui demande de supprimer les fichiers liés
/*Duplication seulement :****/
var DuplicateFldNewValue = "";
var DuplicateBkmListId = null;
var oModDuplicatFlds = null;
var oAffectFile = null; //pour l affectation de fiche globale.
/****************************/
//Pour executer le traitement le traitement
function CallExecuteInsertTreatment() {
    RunTreatment();
}



//Traitement spécifique au traitement server
function RunTreatment(bConfirm) {
    var nbFiles = 0;
    var rbAll = document.getElementById("selection_all");
    var rbSelected = document.getElementById("selection_onlyselect");
    if (rbAll.checked)
        nbFiles = getAttributeValue(rbAll, "nbfiles");
    if (rbSelected.checked)
        nbFiles = getAttributeValue(rbSelected, "nbfiles");


    var oeParam = getParamWindow();
    var nExportMaxNbFiles = getNumber(oeParam.GetParam("ExportMaxNbFiles"));

    if (nExportMaxNbFiles > 0 && nbFiles > nExportMaxNbFiles) {
        eAlert(0, top._res_8675, top._res_8690.replace("<NB>", nExportMaxNbFiles), top._res_8691);
        return;
    }


    var nodeCheckedAction = getCheckedRadio("action", "mainDiv");
    if (nodeCheckedAction == null)
        return;

    var sAction = nodeCheckedAction.value;

    var inputTrtAttributes = document.getElementById("trtAttributes");
    var sTabName = getAttributeValue(inputTrtAttributes, "eTabName");
    var sAdrTabName = getAttributeValue(inputTrtAttributes, "eAdrTabName");
    var sPpTabName = getAttributeValue(inputTrtAttributes, "ePpTabName");
    var sPmTabName = getAttributeValue(inputTrtAttributes, "ePmTabName");

    // Tab de provenance :
    //     - pour PP : si adresse sélectionné depuis PP alors sera égal à PP
    //     - pour PM : si adresse sélectionné depuis PM alors sera égal à PM
    //     - pour les autres sera l'événement sélectionné
    var nTabFromId = getAttributeValue(inputTrtAttributes, "eTabFromId");
    nTabFromTreatment = nTabFromId;

    //     - pour PP : si adresse sélectionné depuis PP alors sera égal à adresse)
    //     - pour PM : si adresse sélectionné depuis PM alors sera égal à adresse)
    //     - pour les autres sera l'événement sélectionné -idem à tabfrom
    var nTargetTabId = getAttributeValue(inputTrtAttributes, "eTargetTabId");

    user_address = getAttributeValue(inputTrtAttributes, "eUserAddress");

    var nAction = nsTreatment.TraitementOperation.NONE;
    switch (sAction.toLowerCase()) {
        case "new":
            nAction = nsTreatment.TraitementOperation.AFFECT_FILE;
            if (!bConfirm) {

                var selectedTab = document.getElementById("action_new_value");
                var fileTab = getNumber(selectedTab.options[selectedTab.options.selectedIndex].value);
                var tabName = selectedTab.options[selectedTab.options.selectedIndex].innerHTML;
                var ednType = getNumber(selectedTab.options[selectedTab.options.selectedIndex].getAttribute("edntype"));
                var bIsPlanning = ednType == "1" ? true : false;
                var parentTabName = sTabName;

                oAffectFile = new AffectFile(fileTab, tabName, parentTabName, bIsPlanning);
                oAffectFile.show();

                return;
            }
            break;
        case "update":
            nAction = nsTreatment.TraitementOperation.UPDATE;

            var inptValueHidden = document.getElementById("action_update_withnew_value");
            var nodeCheckedTypeUpd = getCheckedRadio("action_update", "mainDiv");

            if (nodeCheckedTypeUpd == null) return;

            if (!bConfirm) {
                var lstField = document.getElementById("action_update_value");
                var selectFieldName = lstField.options[lstField.selectedIndex].innerHTML;
                var oModCfmSup = eConfirm(1, top._res_68, top._res_305.replace("<ITEM>", selectFieldName) + ".<br\>" + top._res_645, "", 550, 200,
                    function () { RunTreatment(true); });
                //Confirmation : 
                //  68 => Continuer ?
                //  305 => Vous allez mettre à jour la rubrique '<ITEM>' de toutes les fiches sélectionnées
                //  645 => Confirmez-vous la modification ?
                return;
            }
            break;
        case "delete":
            nAction = nsTreatment.TraitementOperation.DELETE;
            if (!bConfirm) {
                var oModCfmSup = eConfirm(1, top._res_68, top._res_832.replace("<ITEM>", sTabName) + ".<br\>" + top._res_833, "", 550, 200,
                    function () { getDeleteParam(oModCfmSup.getSelectedValue()); RunTreatment(true); });
                oModCfmSup.isMultiSelectList = true;
                if (nTargetTabId == 300) {
                    oModCfmSup.addSelectOption(top._res_6447.replace("<PP>", sPpTabName).replace("<PM>", sPmTabName), "deletePp", false);
                    oModCfmSup.addSelectOption(top._res_6447.replace("<PP>", sAdrTabName).replace("<PM>", sPmTabName), "deleteAdr", false);
                }
                oModCfmSup.createSelectListCheckOpt();
                //Confirmation : 
                //  68 => Continuer ?
                //  832 => Vous allez supprimer toutes les fiches '<ITEM>' sélectionnées
                //  833 => Etes vous certain de vouloir supprimer ces enregistrements ?
                return;
            }
            break;
        case "duplicate":
            nAction = nsTreatment.TraitementOperation.DUPLICATE;

            if (!bConfirm) {


                DuplicateFldNewValue = "";
                DuplicateBkmListId = null;


                var modalDupliWizard = new eModalDialog(top._res_534, 0, "eWizard.aspx", 950, 600, "DupliGlobalAffectWizard");

                modalDupliWizard.EudoType = ModalEudoType.WIZARD.toString(); // Type Wizard
                modalDupliWizard.addParam("tab", nTargetTabId, "post");
                modalDupliWizard.addParam("wizardtype", "duplitreat", "post");
                modalDupliWizard.addParam("targettab", nTargetTabId, "post");
                modalDupliWizard.addParam("operation", nsTreatment.TraitementOperation.DUPLICATE_TEMP_VIEW, "post"); //TraitementOperation.DUPLICATE_TEMP_VIEW
                modalDupliWizard.addParam("frmId", modalDupliWizard.iframeId, "post");
                modalDupliWizard.addParam("modalId", modalDupliWizard.UID, "post");

                modalDupliWizard.ErrorCallBack = eTools.GetHideModalFct(modalDupliWizard, true);

                modalDupliWizard.show();


                var fcvalide = function () {

                    var oRes = modalDupliWizard.getIframe().oDuppiWizard.onValidate();

                    oLstInp = oRes.FieldsList;

                    for (var ni = 0; ni < oLstInp.length; ni++) {

                        var nDescid = getAttributeValue(oLstInp[ni], "edndescid");
                        var sValue = getAttributeValue(oLstInp[ni], "ednvalue");

                        if (DuplicateFldNewValue != "")
                            DuplicateFldNewValue = DuplicateFldNewValue + SEPARATOR_LVL1;

                        DuplicateFldNewValue = DuplicateFldNewValue + nDescid + SEPARATOR_LVL2 + sValue;
                    }

                    DuplicateBkmListId = oRes.BkmList;


                    modalDupliWizard.hide();

                    //TODO : relancer le compteur
                    RunTreatment(true);
                }



                // boutton
                modalDupliWizard.addButton(top._res_29, eTools.GetHideModalFct(modalDupliWizard, false), "button-gray", "", "cancel_btn", true); // Annuler
                modalDupliWizard.addButton(top._res_28, fcvalide, "button-green", "", "validate_btn", false); // Valider   
                modalDupliWizard.addButton(top._res_26, function () { modalDupliWizard.getIframe().MoveStep(true, 'duplitreat'); }, "button-green-rightarrow", null, "next_btn");  //Suivant
                modalDupliWizard.addButton(top._res_25, function () { modalDupliWizard.getIframe().MoveStep(false, 'duplitreat'); }, "button-gray-leftarrow", null, "previous_btn");  // Précédent

                modalDupliWizard.switchButtonsDisplay(false);


                return;
            }
            break;
    }

    if (nAction > 0) {
        var cntAll = getAttributeValue(inputTrtAttributes, "eCntAll");
        var cntOnlySelect = getAttributeValue(inputTrtAttributes, "eCntOnlyselect");
        //- Table sur laquel va se faire le traitement
        //  Attention depuis société/contact l'on peut choisir seulement les fiche adresse
        //  nTabId
        // - Toutes les fiches ?
        //   Ou Juste les fiches sélectionnées
        var bAllFile = document.getElementById("selection_all").checked;
        // - Echantillon ?
        //   Attention pas en mode suppression ou duplication
        var bRandom = (document.getElementById("random_select").getAttribute("chk") == 1);
        var nbRandom = eNumber.ConvertDisplayToBdd(document.getElementById("random_select_count").value);

        var upd = new eUpdater("mgr/eTreatmentManager.ashx", 0);

        upd.addParam("operation", nAction, "post");
        upd.addParam("tabfrom", nTabFromId, "post");
        upd.addParam("targettab", nTargetTabId, "post");
        upd.addParam("allfile", bAllFile ? "1" : "0", "post");
        upd.addParam("cnt", bAllFile ? cntAll : cntOnlySelect, "post");

        if (nAction == nsTreatment.TraitementOperation.AFFECT_FILE) {

            // Paramètres pour Mise à jour de rubriques
            upd.addParam("random", bRandom ? "1" : "0", "post");
            upd.addParam("nbRandom", nbRandom, "post");

            upd.addParam("FileAffectTabId", oAffectFile.getTabId(), "post");
            upd.addParam('FldNewValue', oAffectFile.serialize(), "post");

        } else if (nAction == nsTreatment.TraitementOperation.UPDATE) {
            // Paramètres pour Mise à jour de rubriques
            upd.addParam("random", bRandom ? "1" : "0", "post");
            upd.addParam("nbRandom", nbRandom, "post");

            var lstField = document.getElementById("action_update_value");
            upd.addParam("fldDescId", lstField.options[lstField.options.selectedIndex].value, "post");

            var nodeCheckedTypeUpd = getCheckedRadio("action_update", "mainDiv");

            if (nodeCheckedTypeUpd == null) return;
            upd.addParam("typeUpdate", nodeCheckedTypeUpd.value, "post");
            switch (nodeCheckedTypeUpd.value) {
                case "fromexisting":
                    var lstExistingField = document.getElementById("action_update_fromexisting_value");
                    upd.addParam("existingFldDescId", lstExistingField.options[lstExistingField.options.selectedIndex].value, "post");
                    break;
                case "withnew":
                    upd.addParam("updWithnewVal", inptValueHidden.value, "post");
                    break;
                case "withnewdate":
                    upd.addParam("updWithnewVal", inptValueHidden.value, "post");
                    break;
                case "removevalue":
                    upd.addParam("removevalue", inptValueHidden.value, "post");
                    break;
            }
            var oRAZ = document.getElementById("action_update_withnew_erase_existing");
            if (oRAZ) {
                upd.addParam("updWithnewErase", (oRAZ.getAttribute("chk") == "1") ? "1" : "0", "post");
            }
        }
        else if (nAction == nsTreatment.TraitementOperation.DELETE) {
            // Paramètres pour Suppression de fiche
            upd.addParam("deletePp", bDeletePp ? "1" : "0", "post");
            upd.addParam("deleteAdr", bDeleteAdr ? "1" : "0", "post");
        }
        else if (nAction == nsTreatment.TraitementOperation.DUPLICATE) {
            // Paramètres pour la duplication
            //Nouvelles valeurs pour les descId correspondants
            upd.addParam("FldNewValue", DuplicateFldNewValue, "post");
            //Bkm à dupliquer
            if (DuplicateBkmListId != null)
                upd.addParam("BkmListId", DuplicateBkmListId, "post");
        }

        // Créer une methode en cas d'une erreur il l'execute
        upd.ErrorCallBack = function () { StopProcessTreatment(); top.setWait(false); };
        upd.send(ReturnRunTreatment);
        top.setWait(true);
    }
    else {
        //TODO : Merci de sélectionner une action
    }

}




///<summary>
///Objet qui traite l'affectation de la nouvelle fiche pour le traitement de masse.
///<param name="nTab">La tab de la nouvelle fiche</param>
///<param name="sTabName">le nom de la tab</param>
///<param name="sParentTabName">Le nom de la tab parente</param>
///<summar>
function AffectFile(nTab, sTabName, sParentTabName, isPlanning) {

    var that = this;
    this.tab = nTab; //la tab de la nouvelle fiche
    this.tabName = sTabName;
    this.parentTabName = sParentTabName;
    this.fields = new Array(); //les rubriques de la fiches
    this.listId = new Array(); //liste des ids sélectionnée
    this.bIsPlanning = isPlanning;
    this.oWind = null;
    this.bIsInvit = false;


    this.RunTreatmentFct = RunTreatment;


    ///<summary>
    ///Retourne le tab de la fiche  
    ///<summary>
    this.getTabId = function () {
        return this.tab;
    }

    ///<summary>
    ///Affiche la popup en mode creation
    ///<param name="eModFile">La popup de la nouvelle fiche</param>
    ///<summary
    this.show = function () {

        //Pour chaque fiche <PARENTTAB>, affecte la nouvelle fiche <PARENTTAB>
        var title = top._res_6430
                    .replace("<TAB>", this.tabName)
                    .replace("<PARENTTAB>", this.parentTabName);

        //Fenetre en mode création. 
        //7 pour global affect
        //width = height = 0, shFileInPopup calcule la taille par defaut de la fenetre   
        top.shFileInPopup(this.tab, 0, title, 0, 0, 0, null, true, function (eModFile) { that.valid(top.eModFile); }, 7);
    }




    this.hide = function () {
        eModFile.hide();
    }

    ///<summary>
    ///Valide tous les champs de la nouvelle fiche et lance le traitement  de l'affectation globale coté serveur 
    ///<param name="eModFile">La popup de création de nouvelle fiche</param>
    ///<summary
    this.valid = function (eModFile) {

        this.oWin = eModFile.getIframe();
        this.fields = this.oWin.getFieldsInfos(this.tab, 0);

        //si pas de champs obligatoires restants, on lance le traitement
        if (!this.oWin.chkExistsObligatFlds(this.fields, null, null, this.oWin)) {
            this.RunTreatmentFct(true);
        }
    }

    this.CheckConflictInfos = function (eModFile) {

        this.oWin = eModFile.getIframe();
        alert("Not implemented...");

    }

    ///<summary>
    ///Concatène les champs de la nouvelle fiche 
    ///<exemple>DescId#$#Value#|#DescId2#$#Value<exemple>
    ///<summary>
    this.serialize = function () {

        var param = "";

        for (var key in this.fields) {
            if (param != "")
                param = param + SEPARATOR_LVL1;

            // HLA - Gestion de l'autobuildname en mode création en popup - Dev #33529
            if (this.fields[key].chgedVal == null || this.fields[key].chgedVal)
                param = param + this.fields[key].descId + SEPARATOR_LVL2 + this.fields[key].newValue;
        }

        return param;
    }

}




function getDeleteParam(param) {
    if (param != "" && param.indexOf("deletePp") >= 0)
        bDeletePp = true;
    else
        bDeletePp = false;

    if (param != "" && param.indexOf("deleteAdr") >= 0)
        bDeleteAdr = true;
    else
        bDeleteAdr = false;

}
//Appelé au retour de RunTreatment qui récupère le mode d'export choisi et en fonction de ce qui a été choisi,
function ReturnRunTreatment(oDoc) {

    alertAttentTreatment = null;
    nServerTreatmentId = -1;

    var bSuccess = (getXmlTextNode(oDoc.getElementsByTagName("success")[0]) == "1");

    if (!bSuccess) {
        // Cas non possible
        return;
    }

    top.setWait(false);

    var bRecurentMode = (getXmlTextNode(oDoc.getElementsByTagName("recurrentmode")[0]) == "1");

    // En cas de recurence, on averti seulement l'utilisateur que nous avons bien pris en compte ca demande
    if (bRecurentMode) {
        var title = top._res_5080;      //Informations
        var message = top._res_306;     //Traitement en cours...
        var detail = top._res_6263;     //Votre demande a été prise en compte
        var msgType = MsgType.MSG_INFOS.toString();
        top.eAlert(msgType, top._res_5080, message, detail, 450, 180);

        // On recharge la fiche
        var nType = getCurrentView(top.document) == "FILE_MODIFICATION" ? 3 : 2;
        top.loadFile(top.nGlobalActiveTab, oInvitWizard.FileFromId, nType);

        // Fermeture du wizard
        //#33088 sph : on doit fermer la fenêtre en dernier car c'est sur celle-ci que se trouve les JS
        top.modalWizard.hide();
    } else {
        alertStartTreatment = showWaitDialog(top._res_307, top._res_1811);
        alertStartTreatment.onHideFunction = function () { alertStartTreatment = null; };

        nServerTreatmentId = getXmlTextNode(oDoc.getElementsByTagName("servertreatmentid")[0]);

        GetTreatmentStatut();
        //checkStatut(nServerTreatmentId);
    }
}

function checkStatut(treatId) {

    oTreatProgress.Init(treatId);
    oTreatProgress.ShowProgress();
    oTreatProgress.OnPercentCompleted[50] = function (percent) { alert(percent); };
    oTreatProgress.OnPercentCompleted[25] = function (percent) { alert(percent); };
    oTreatProgress.OnSucess = function (oDoc) { oTreatProgress.Close(); };
    oTreatProgress.OnError = function (oDoc) { GetDisplayMsg(oDoc); };
};

function hideAllModal() {
    if (typeof (alertStartTreatment) != "undefined" && alertStartTreatment != null)
        alertStartTreatment.hide();

    if (typeof (alertAttentTreatment) != "undefined" && alertAttentTreatment != null)
        alertAttentTreatment.hide();
}

function progessTreatment(prog) {
    // Pas de progression car traitement en bulk
    if (prog == -2) {
        if (alertAttentTreatment == null || alertAttentTreatment.type != 3) {
            hideAllModal();

            alertAttentTreatment = showWaitDialog(top._res_307, top._res_306);
            alertAttentTreatment.onHideFunction = function () { alertAttentTreatment = null; };
        }
    }
    else if (prog >= 0 && prog <= 100) {
        // Premier lancement on affiche la fenêtre d'attente et on déclare un timeout d'attente de temps maximum
        if (alertAttentTreatment == null || alertAttentTreatment.type != 4) {
            hideAllModal();

            alertAttentTreatment = new eModalDialog(top._res_307, 4, top._res_307, 550, 160);
            // Masquer le bouton "Agrandir"
            alertAttentTreatment.hideMaximizeButton = true;
            // Masquer le bouton "Fermer"
            alertAttentTreatment.hideCloseButton = true;
            alertAttentTreatment.show();

            alertAttentTreatment.onHideFunction = function () { alertAttentTreatment = null; };
        }

        alertAttentTreatment.updateProgressBar(prog);
    }
}


//Enclenche la récupération du statut du traitement
function GetTreatmentStatut() {
    var upd = new eUpdater("mgr/eTreatmentManager.ashx", 0);
    upd.addParam("traitementid", nServerTreatmentId, "post");
    upd.addParam("operation", nsTreatment.TraitementOperation.CHECK, "post");   //CHECKMODE

    //Créer une methode en cas d'une erreur il l'execute
    upd.ErrorCallBack = StopProcessTreatment;
    upd.send(RetourTreatmentStatut);
}

///Si on fait un traitement de masse depuis le menu action
///On initialise la nTabFromTreatment
function SetTabFromTreatement(TabFrom) {
    nTabFromTreatment = TabFrom;
}

//Au retour de GetTreatmentStatut 
//  Vérifie le statut du rapport qui a été checké
//  => Si en état WAIT ou RUNNING
//      On appel de nouveau la méthode RetourTreatmentStatut avec un timeout avant execution
//  => Si en état SUCCESS 
//      On affiche une confirmation de bon fonctionnement
//  => Si en état MAIL_ERROR
//      On affiche qu'une erreur de mail s'est produite
//  => Sinon c'est qu'on est en erreur alors on affiche un message d'erreur
function RetourTreatmentStatut(oDoc) {

    if (!oDoc || typeof oDoc == "undefined")
        return;

    var bSuccess = (getXmlTextNode(oDoc.getElementsByTagName("success")[0]) == "1");
    var statut = getXmlTextNode(oDoc.getElementsByTagName("Statut")[0]);
    var nAction = getXmlTextNode(oDoc.getElementsByTagName("action")[0]);
    var nPercentProgress = getXmlTextNode(oDoc.getElementsByTagName("PercentProgress")[0]);

    var bInvit = (
        nAction == nsTreatment.TraitementOperation.INVIT_CREA
        || nAction == nsTreatment.TraitementOperation.INVIT_CREA_BULK
        || nAction == nsTreatment.TraitementOperation.INVIT_SUP_BULK
        || nAction == nsTreatment.TraitementOperation.INVIT_SUP
        );

    var bTraitement = (
        nAction == nsTreatment.TraitementOperation.AFFECT_FILE
		|| nAction == nsTreatment.TraitementOperation.UPDATE
		|| nAction == nsTreatment.TraitementOperation.DELETE
		|| nAction == nsTreatment.TraitementOperation.DUPLICATE
        );

    var myTopWin = top;

    //Satut == En attente ou En cours => On retente tant que l'on dépasse pas le nombre d'appel maximum
    if (statut == "WAIT" || statut == "RUNNING") {
        progessTreatment(nPercentProgress);
        fctTreatmentTimeOutCheck = window.setTimeout(GetTreatmentStatut, nCheckTimeOutTreatment);
        return;
    }
    else if (statut == "SUCCESS") {

        if (!GetDisplayMsg(oDoc))
            myTopWin.eAlert(3, '', myTopWin._res_1676, '', null, null, null); //Si pas de message de définit par le traitement, on affiche le message par défaut "traitement terminé"

        if (bInvit) {
            CloseWaitTreatmentStatus();

            var nType = getCurrentView(myTopWin.document) == "FILE_MODIFICATION" ? 3 : 2;
            myTopWin.loadFile(myTopWin.nGlobalActiveTab, oInvitWizard.FileFromId, nType);

            //#33088 sph : on doit fermer la fenêtre en dernier car c'est sur celle-ci que se trouve les JS
            myTopWin.modalWizard.hide();
        }
        else {
            var nTabFrom = nTabFromTreatment;

            // HLA - Fiche toujours présente dans les dernières fiches consultées (survol onglet) après suppression - #55168
            // Recharge les mru
            var oeParam = getParamWindow();
            if (oeParam.RefreshMRU)
                oeParam.RefreshMRU(nTabFrom);

            CloseWaitTreatmentStatus();

            // Recharge la liste
            myTopWin.goTabList(nTabFrom);
        }
    }
    else if (statut == "MAIL_ERROR") {  //MAIL_ERROR : Adresse mail ou smtp non valide...
        var errorDescription = getXmlTextNode(oDoc.getElementsByTagName("errorDescription")[0]);
        GetDisplayMsg(oDoc);
        myTopWin.showWarning(myTopWin._res_72 + " (mail)", myTopWin._res_1023, myTopWin._res_422 + "<br>" + myTopWin._res_544);    //Affichage du message d'erreur standard
        CloseWaitTreatmentStatus();

        if (bInvit) {
            var nType = getCurrentView(myTopWin.document) == "FILE_MODIFICATION" ? 3 : 2;
            myTopWin.loadFile(myTopWin.nGlobalActiveTab, oInvitWizard.FileFromId, nType);

            //#33088 sph : on doit fermer la fenêtre en dernier car c'est sur celle-ci que se trouve les JS
            myTopWin.modalWizard.hide();
        } else if (bTraitement) {
            myTopWin.ReloadList();
        }
    }
    else if (statut == "ERROR_USER") {
        var errorDescription = getXmlTextNode(oDoc.getElementsByTagName("errorDescription")[0]);
        GetDisplayMsg(oDoc);
        CloseWaitTreatmentStatus();

        if (bInvit) {
            var nType = getCurrentView(myTopWin.document) == "FILE_MODIFICATION" ? 3 : 2;
            myTopWin.loadFile(myTopWin.nGlobalActiveTab, oInvitWizard.FileFromId, nType);

            //#33088 sph : on doit fermer la fenêtre en dernier car c'est sur celle-ci que se trouve les JS
            myTopWin.modalWizard.hide();
        } else if (bTraitement) {
            myTopWin.ReloadList();
        }
    }
    else {  //ERROR
        var errorDescription = getXmlTextNode(oDoc.getElementsByTagName("errorDescription")[0]);
        GetDisplayMsg(oDoc);    //Affichage du résumé
        myTopWin.showWarning(myTopWin._res_72, myTopWin._res_72, myTopWin._res_422 + "<br>" + myTopWin._res_544);    //Affichage du message d'erreur standard
        CloseWaitTreatmentStatus();

        if (bTraitement)
            myTopWin.ReloadList();
    }
}

//Affichage du retour du traitement détaillé
//  Envoi faux si pas de contenu détaillé à afficher
function GetDisplayMsg(oDoc) {

    paramWin = top.getParamWindow();
    objThm = JSON.parse(paramWin.GetParam("currenttheme").toString());

    var bMsgDisplayed = false;
    var oDisp = oDoc.getElementsByTagName("DisplayMsg");
    if (oDisp && oDisp.length > 0) {
        var Title = getXmlTextNode(oDisp[0].getElementsByTagName("Title")[0]);
        var Msg = getXmlTextNode(oDisp[0].getElementsByTagName("Msg")[0]);
        var Detail = getXmlTextNode(oDisp[0].getElementsByTagName("Detail")[0]);
        var Criticity = getXmlTextNode(oDisp[0].getElementsByTagName("TypeCriticity")[0]);

        if (Msg != "") {
            /*AJOUT de SAUT DE LIGNES*/
            Msg = Msg.replace(/(\r\n|\n|\r)/gm, "[[BR]]");
            Detail = Detail.replace(/(\r\n|\n|\r)/gm, "[[BR]]");
            Detail = Detail.replace(/(\t)/gm, "[[SPACE]][[SPACE]][[SPACE]][[SPACE]]");
            /*************************/

            //ELAIZ - tâche 2601 -  si eudonet x alors taille plus sur les modales d'alert
            if (objThm.Version > 1) {
                var height = 200;
            } else {
                var height = 100;
            }

            var modAlert = top.eAlert(Criticity, Title, Msg, Detail, 600, height);

            bMsgDisplayed = true;
            //Indique que la taille de la fenêtre doit s'ajuster au contenu
            modAlert.adjustModalToContent(40);
        }
    }
    return bMsgDisplayed;
}

//Fermeture de la fenêtre d'attente, de la liste des rapport et du fond grisé
function CloseWaitTreatmentStatus() {
    window.clearTimeout(fctTreatmentTimeOut);
    fctTreatmentTimeOut = null;

    hideAllModal();

    if (typeof top.onCloseTreatment == "function")
        top.onCloseTreatment();
}

//Fermeture en cas de délai d'attente dépassé.
function CloseAfterTimeOutTreatment() {
    eAlert(3, top._res_6266, top._res_6263 + ".<br\>" + top._res_6077.replace("<USER_ADDRESS>", user_address), '', null, null, null);

    CloseWaitTreatmentStatus();
}

function StopProcessTreatment(oDoc) {
    hideAllModal();
}
//ENDREGION #TREATMENT#





oTreatProgress = (function () {
    var me;
    var treatId = 0;
    var oModalDialog;

    var checkRate = 1000; // appels serveur toutes les secondes
    var refreshRate = 200; // rafraichissement de l'ecran tous les 100 ms

    var oldPercent = 0;
    var currentPercent = 0;

    var refreshRateHandler = null;
    var checkRateHandler = null;


    function setCheckRateInterval() {
        checkRateHandler = window.setInterval(checkStatut, checkRate);
    };

    function init() {
        oModalDialog = new eModalDialog(top._res_307, 4, top._res_307, 550, 160);
        // Masquer le bouton "Agrandir"
        oModalDialog.hideMaximizeButton = true;
        // Masquer le bouton "Fermer"
        oModalDialog.hideCloseButton = true;
    };

    //Interpolation
    function lerp(startValue, endValue, speedRate) {
        return (parseInt(endValue) - parseInt(startValue)) * parseInt(speedRate) + parseInt(startValue);
    };

    //Enclenche la récupération du statut du traitement
    function checkStatut() {

        var upd = new eUpdater("mgr/eTreatmentManager.ashx", 0);
        upd.addParam("traitementid", treatId, "post");
        upd.addParam("operation", nsTreatment.TraitementOperation.CHECK, "post");   //CHECKMODE

        //Créer une methode en cas d'une erreur il l'execute
        upd.ErrorCallBack = stop;
        upd.send(newStatut);
    }

    function stop(oDoc) {
        me.OnError(oDoc);
        me.Close();
    };

    function newStatut(oDoc) {

        if (!oDoc || typeof oDoc == "undefined") {
            // stop(oDoc);
            return;
        }
        var bSuccess = (getXmlTextNode(oDoc.getElementsByTagName("success")[0]) == "1");
        var statut = getXmlTextNode(oDoc.getElementsByTagName("Statut")[0]);
        var nAction = getXmlTextNode(oDoc.getElementsByTagName("action")[0]);
        var nPercentProgress = getXmlTextNode(oDoc.getElementsByTagName("PercentProgress")[0]);

        //Satut == En attente ou En cours => On retente tant que l'on dépasse pas le nombre d'appel maximum
        if (statut == "WAIT" || statut == "RUNNING") {

            oldPercent = currentPercent;
            currentPercent = parseInt(nPercentProgress);

            updateCheckRate();
            updateRefreshRate();

            me.OnCheck(currentPercent);

            if (refreshRateHandler != null)
                clearInterval(refreshRateHandler);

            refreshRateHandler = window.setInterval(repaint, refreshRate);
        }
        else if (statut == "SUCCESS") {
            me.OnSucess(oDoc);
            me.Close();
        } else if (statut == "ERROR_USER") {
            stop(oDoc);
        }
    }

    function repaint() {

        var newPercent = lerp(oldPercent, currentPercent, refreshRate);

        me.OnRefresh(newPercent);

        oldPercent = currentPercent;
        currentPercent = newPercent;

        oModalDialog.updateProgressBar(currentPercent);

        if (currentPercent in me.OnPercentCompleted && typeof (me.OnPercentCompleted[currentPercent]) == 'function') {
            me.OnPercentCompleted[currentPercent](currentPercent);
        }
    }

    function updateCheckRate() {

    };
    function updateRefreshRate() {

    };
    function clearIntervals() {
        if (checkRateHandler != null)
            clearInterval(checkRateHandler);

        if (refreshRateHandler != null)
            clearInterval(refreshRateHandler);
    };


    return {
        Init: function (treatmentId) {
            treatId = treatmentId;
            init();
            me = this;
        },
        ShowProgress: function () {
            setCheckRateInterval();
            oModalDialog.show();
        },
        Close: function () {
            clearIntervals();
            if (oModalDialog == null)
                oModalDialog.hide();
        },
        OnRefresh: function (percent) { /* S'abonner */ },
        OnCheck: function (percent) { /* S'abonner */ },
        OnSucess: function (oDoc) { /* S'abonner */ },
        OnError: function (oDoc) { /* S'abonner */ },
        OnPercentCompleted: [] /* S'abonner */
    }
}());
