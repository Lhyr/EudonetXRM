//*****************************************************************************************************//
//*** SPH - 11/2013 - JS spécifique au wizard d'ajout/suppression d'invitations
//*****************************************************************************************************//

var oAffectFile;            // Object fiche pour la création d'invitation
var eCMInvitWizard;         // Menu contextuel (pour le bouton de sélection sur la liste d'invitation (cocher/décocher tous/cocher tous sauf grisé)
var nsInvitWizard = {};     // NameSpace pour les fonctions propre au wizard

//SHA
/// <summary> Types d'invitation de destinataires à partir d'un filtre (++) (ajouté dans Enum.cs) /// </summary>
nsInvitWizard.InvitRecipientMode =
{
    /// <summary>Type d'ajout destinataires non défini</summary>
    INVIT_RECIPIENT_UNDEFINED: -1,
    /// <summary>Ajout immédiat de destinataires à partir d'un filtre (++)</summary>
    INVIT_RECIPIENT_IMMEDIATE: 0,
    /// <summary>Ajout récurrent de destinataires à partir d'un filtre (++)</summary>
    INVIT_RECIPIENT_RECURRENT: 1
}

//SHA
///enum InvitRecipAction
//var InvitRecipAction =
//{
//    /// <summary>pas d'action (ne doit pas se produire)</summary>
//    NONE: { KEY: 0, RES: top._res_6524 }, //"Invalide"
//    /// <summary>Enregistrement de l'ajout de destinataires</summary>
//    INSERT: { KEY: 1, RES: top._res_6381 },//"Enregistrement"
//    /// <summary>Mise à jour de l'ajout de destinataires</summary>
//    UPDATE: { KEY: 2, RES: top._res_961 },//"Mise à jour"
//    /// <summary>Annuler l'ajout de destinataires</summary>
//    CANCEL: { KEY: 4, RES: top._res_6633 }//Annulation
//}

//Action possible pour le manager
nsInvitWizard.Action =
{
    ACTION_LOADINVIT: 0,            // Chargement initial
    ACTION_SELECTINVIT: 1,          // Sélection d'1 invitation
    ACTION_SELECTALLINVIT: 2,       // Sélection de toutes les invitations
    ACTION_UNSELECTINVIT: 3,        // déselection d'1 invitation
    ACTION_UNSELECTALLINVIT: 4,     // déselection de toutes les invitations
    ACTION_SELECTALLINVIT_NODBL: 5,  // Sélection de toutes les invitations non grisée
    ACTION_CONFIRM_AUTO: 6,         //Demande de confirmation pour la génération automatique
    ACTION_RELOAD_CAMPAIGN_TYPE: 7  //Pour Rafraichir la liste des type de campagne
};


nsInvitWizard.chkInvitFileList = function (oChckLst) {
    var oWizardbody = document.getElementById("wizardbody");
    var nBkm = getAttributeValue(oWizardbody, "bkm");

    var oInvitManager = new eUpdater("mgr/eInvitWizardManager.ashx", 0);
    oInvitManager.ErrorCallBack = function () { };
    oInvitManager.addParam("lstInvit", JSON.stringify(oChckLst), "post");
    oInvitManager.addParam("bkm", nBkm, "post");                        //DescId du template invitation
    oInvitManager.addParam("tabfrom", oInvitWizard.TabFrom, "post");    //Descid de l'event de départ
    oInvitManager.addParam("action", 1, "post");

    setWait(true);
    oInvitManager.send(nsInvitWizard.updtInvitCnt, 1);

}

///Ajoute/Retire une fiche invitation
nsInvitWizard.chkInvitFile = function (obj) {

    // remonte jusqu'au TR
    var elem = obj.parentNode || obj.parentElement;
    while (elem.tagName != 'TR') {

        elem = elem.parentNode || elem.parentElement;

        if (elem.tagName == 'TBODY' || elem.tagName == 'BODY')
            return;
    }

    var oWizardbody = document.getElementById("wizardbody");
    var nBkm = getAttributeValue(oWizardbody, "bkm");


    // Id de l'adresse
    var nAdrId = getAttributeValue(elem, "eseladrid");

    // id  du contact
    var nPpId = getAttributeValue(elem, "eselppid");


    // Grise les contact déjà sélectionnés
    nsInvitWizard.greyElements()

    var bAdd = 0;
    var nAction = 3; // décoche
    if (obj.getAttribute("chk") == "1")
        nAction = 1; // coche

    var oInvitManager = new eUpdater("mgr/eInvitWizardManager.ashx", 0);
    oInvitManager.ErrorCallBack = function () { };
    oInvitManager.addParam("ppId", nPpId, "post");
    oInvitManager.addParam("adrId", nAdrId, "post");


    // mode ++ ou XX
    if (oInvitWizard.DeleteMode) {
        var sEid = getAttributeValue(elem, "eid");
        if (sEid.indexOf(oInvitWizard.TabInvit + "_") == 0) {
            var aEid = sEid.split("_");
            oInvitManager.addParam("delete", "1", "post");
            oInvitManager.addParam("tplid", aEid[1], "post");
        }
    }

    oInvitManager.addParam("bkm", nBkm, "post");                        //DescId du template invitation
    oInvitManager.addParam("tabfrom", oInvitWizard.TabFrom, "post");    //Descid de l'event de départ
    oInvitManager.addParam("action", nAction, "post");

    setWait(true);
    oInvitManager.send(nsInvitWizard.updtInvitCnt, nAction);
};

///Lancement automatique
/// Dans le cas de création, la fenêtre de création d'invitation doit être proposé, sinon, le traitement est lancé directement
nsInvitWizard.AutoLaunch = function () {
    if (!oInvitWizard.DeleteMode)
        nsInvitWizard.AddInvitAll();
    else
        nsInvitWizard.RunTreatmentAll();
}

// vérifie si la création/supression automatique doit être demandée
nsInvitWizard.checkAutoLaunch = function () {
    if (wizardIframe.contentDocument) {
        var oBaseDocument = wizardIframe.contentDocument;

        var oHV;
        var oMT;
        if (!oInvitWizard.DeleteMode) {
            oHV = oBaseDocument.getElementById("hv_200");
            oMT = oBaseDocument.getElementById("mt_200");
        }
        else {
            oHV = oBaseDocument.getElementById("hv_" + oInvitWizard.TabInvit);
            oMT = oBaseDocument.getElementById("mt_" + oInvitWizard.TabInvit);
        }
        if (!oHV) {
            return;
        }

        var oThreshold = oHV.querySelector("input[name='threshold']");
        var nbFiles = getNumber(getAttributeValue(oMT, "enbtotal"));

        //Proposition de lancement automatique 
        if (getAttributeValue(oThreshold, "autolaunch") == "1") {
            var sAdrLabel = getAttributeValue(oMT, "adrlbl");
            var sEvtLabel = getAttributeValue(oMT, "evtlbl");

            nsInvitWizard.AutoLaunchOptions(oThreshold.value, nbFiles, oInvitWizard.DeleteMode, sAdrLabel, sEvtLabel);
            return;

        }
    }
    nsInvitWizard.updtInvitCnt(null, 0);
    //return false
}


nsInvitWizard.oMdlAutoLaunchOpt = null;
nsInvitWizard.AutoLaunchOptions = function (nTreshold, NbFiles, bDelete, sAdrLabel, sEvtLabel) {

    nsInvitWizard.oMdlAutoLaunchOpt = new eModalDialog(top._res_6622, 1, '', 500, 300);

    nsInvitWizard.oMdlAutoLaunchOpt.hideMaximizeButton = true;

    var msg = top._res_2611 + " " + nTreshold + " " + top._res_300;
    var details;
    var labSelAuto;
    if (!bDelete) {
        details = top._res_2612 + " (" + NbFiles + " " + top._res_300 + ")";
        labSelAuto = top._res_6624;
    }
    else {
        details = top._res_6628 + " (" + NbFiles + " " + top._res_300 + ")";
        labSelAuto = top._res_6629;
    }
    nsInvitWizard.oMdlAutoLaunchOpt.setMessage(msg, details, 1);

    nsInvitWizard.oMdlAutoLaunchOpt.show();


    nsInvitWizard.oMdlAutoLaunchOpt.createControl("radio", "SelType", "SelManu", "selmanu", top._res_6623, "IWSelType", null, true);
    nsInvitWizard.oMdlAutoLaunchOpt.createControl("radio", "SelType", "SelAuto", "selauto", labSelAuto, "IWSelType");

    if (!bDelete) {
        var divAdrToKeep = nsInvitWizard.oMdlAutoLaunchOpt.createDiv("IWAdrToKeepGbl");
        nsInvitWizard.oMdlAutoLaunchOpt.createDiv("IWAdrToKeep", divAdrToKeep, top._res_156.replace("<PREFNAME>", sAdrLabel) + " : ");
        nsInvitWizard.oMdlAutoLaunchOpt.createControl("checkbox", "AdrAct", "AdrAct", "active", top._res_6295, "IWAdrToKeep", divAdrToKeep);
        nsInvitWizard.oMdlAutoLaunchOpt.createControl("checkbox", "AdrPal", "AdrPal", "pal", top._res_6294, "IWAdrToKeep", divAdrToKeep);

        var strNoDbl = top._res_2613.replace("<PREFNAME>", sAdrLabel).replace("<EVENT>", sEvtLabel)
        nsInvitWizard.oMdlAutoLaunchOpt.createControl("checkbox", "NoDblAdr", "NoDblAdr", "nodbladr", strNoDbl, "IWAdrNoDbl", null, true);
    }

    nsInvitWizard.oMdlAutoLaunchOpt.addButtonFct(top._res_866, function () { nsInvitWizard.applyAutoLaunchOptions(); }, "button-green", "cancel");

};

nsInvitWizard.applyAutoLaunchOptions = function () {

    if (!nsInvitWizard.oMdlAutoLaunchOpt) {
        eAlert(0, top._res_72, top._res_6625, top._res_6626)
        return;
    }

    var oMdlDivMain = nsInvitWizard.oMdlAutoLaunchOpt.getDivMain();

    if (!oMdlDivMain) {
        eAlert(0, top._res_72, top._res_6625, top._res_6626)
        return;
    }

    var oSelAuto = oMdlDivMain.querySelector("input[id='SelAuto']");


    var bApply = false;

    if (oSelAuto && oSelAuto.checked) {
        bApply = true;
    }

    var bOnlyActive = getAttributeValue(oMdlDivMain.querySelector("a[id='AdrAct']"), 'chk') == '1';
    var bOnlyPale = getAttributeValue(oMdlDivMain.querySelector("a[id='AdrPal']"), 'chk') == '1';

    //Récupération des options sur les adresse à retenir
    var oAct = document.getElementById("radioAdrAct")
    if (oAct) {
        chgChk(oAct, bOnlyActive);
    }

    var oPrinc = document.getElementById("radioAdrPrinc")
    if (oPrinc) {
        chgChk(oPrinc, bOnlyPale);
    }

    var bDoNotDbl = getAttributeValue(oMdlDivMain.querySelector("a[id='NoDblAdr']"), 'chk') == '1';

    var oDoNotDbl = document.getElementById("chkDoNotDbl");
    if (oDoNotDbl) {
        chgChk(oDoNotDbl, bDoNotDbl);
    }

    if (bApply) {
        nsInvitWizard.AutoLaunch();
    }
    else {
        UpdatePPList(1);
    }
    nsInvitWizard.oMdlAutoLaunchOpt.hide();

}

///Mise à jour du compteur d'invitations sélectionnés
nsInvitWizard.updtInvitCnt = function (oRes, nAction, bConfirm) {
    try {

        var oWizardbody = document.getElementById("wizardbody");
        var bCheckAll = (nAction == 2);

        if (nAction == 2) {
            setAttributeValue(oWizardbody, "allchecked", "1");
        }
        else if (nAction == 1 || nAction == 4 || nAction == 3) {
            setAttributeValue(oWizardbody, "allchecked", "0");
        }

        if (typeof (bConfirm) == "undefined")
            bConfirm = false;

        //Maj du nombre de fiches cochées
        if (oRes) {
            if (nAction == nsInvitWizard.Action.ACTION_LOADINVIT) {
                //Chargement intital ou tout décocher
                oInvitWizard.NbInvit = 0;
                oInvitWizard.NbAdr = 0;
                oInvitWizard.NbPP = 0;
            }
            else {

                if ((typeof oRes === "object") && oRes.nodeType && oRes.nodeType == 9) {

                    if (oRes.getElementsByTagName("nbinvit").length == 1) {
                        oInvitWizard.NbInvit = getXmlTextNode(oRes.getElementsByTagName("nbinvit")[0]);
                    }

                    if (oRes.getElementsByTagName("nbadr").length == 1) {
                        oInvitWizard.NbAdr = getXmlTextNode(oRes.getElementsByTagName("nbadr")[0]);
                    }

                    if (oRes.getElementsByTagName("nbpp").length == 1) {
                        oInvitWizard.NbPP = getXmlTextNode(oRes.getElementsByTagName("nbpp")[0]);
                    }
                }
            }
        }
        else {
            //Récupération des informations de compteurs depuis le div masqué
            //BSE - #50 667 Mise à jours des compteurs
            if (wizardIframe.contentDocument)
                var oBaseDocument = wizardIframe.contentDocument;

            var oHV;
            if (!oInvitWizard.DeleteMode)
                oHV = oBaseDocument.getElementById("hv_200");
            else
                oHV = oBaseDocument.getElementById("hv_" + oInvitWizard.TabInvit);

            if (oHV) {
                var oCheckedValue = oHV.querySelector("input[id='checked']");
                if (oCheckedValue) {
                    oInvitWizard.NbInvit = getAttributeValue(oCheckedValue, "nb");
                    oInvitWizard.NbAdr = getAttributeValue(oCheckedValue, "nbadr");
                    oInvitWizard.NbPP = getAttributeValue(oCheckedValue, "nbpp");
                }
            }
        }


        var bAllChecked = getAttributeValue(oWizardbody, "allchecked") == "1";

        if (!oInvitWizard.DeleteMode)
            var chkAllChecked = document.getElementById("chkAll_200");
        else
            var chkAllChecked = document.getElementById("chkAll_" + oInvitWizard.TabInvit);


        if (bAllChecked) {
            if (getAttributeValue(chkAllChecked, "chk") != "1") {
                chgChk(chkAllChecked);
            }
        }
        else {
            if (getAttributeValue(chkAllChecked, "chk") == "1") {
                chgChk(chkAllChecked);
            }
        }


        if (nAction == nsInvitWizard.Action.ACTION_SELECTALLINVIT || nAction == nsInvitWizard.Action.ACTION_UNSELECTALLINVIT) {
            //(Dé)coche tout

            var nValCheck = (nAction == 4) ? "1" : "0";
            var allInvit = document.getElementById("PPList").querySelectorAll("a[chk='" + nValCheck + "'][name='chkMF']");

            for (var nI = 0; nI < allInvit.length; nI++) {
                var myChk = allInvit[nI];
                chgChk(myChk);
            }
        }

        //
        if (nAction == nsInvitWizard.Action.ACTION_SELECTALLINVIT_NODBL) {

            var oMainDiv = document.getElementById("PPList");
            var sTRSelector = "table#mt_200 tr[eseladrid]";
            var nTypDbl = "";

            //Type de dédoublonnage (pp ou adr)
            var oDblAct = document.getElementById("radioGreyDbl");
            if (oDblAct && getAttributeValue(oDblAct, "chk") == "1") {
                var oSelect = document.getElementById("invitSelectTypDbl");
                nTypDbl = oSelect.options[oSelect.selectedIndex].value;
            }

            var oAllTR = oMainDiv.querySelectorAll(sTRSelector);

            for (var nI = 0; nI < oAllTR.length; nI++) {


                var myTR = oAllTR[nI];

                // On souhaite en premier lieu cocher la case...
                var nTargetChkState = 1;
                // Sauf s'il s'agit d'une case grisée, auquel cas il faut la décocher
                if (nTypDbl == 400 && getAttributeValue(myTR, "ednadrdbl") == "1")
                    nTargetChkState = 0;

                if (nTypDbl == 200 && getAttributeValue(myTR, "ednppdbl") == "1")
                    nTargetChkState = 0;

                var myChk = myTR.querySelector("a[chk]");

                // Si la case n'est pas déjà dans l'état souhaité, on la coche/décoche
                if (getAttributeValue(myChk, "chk") != nTargetChkState)
                    chgChk(myChk);
            }

        }
    }
    catch (e) {
    }
    finally {
        setWait(false);
    }

    //MaJ des compteurs

    var oPPCmpt = document.getElementById("cmptPPSpan");
    if (oPPCmpt) {
        if (typeof oPPCmpt.innerText !== 'undefined')
            oPPCmpt.innerText = oInvitWizard.NbPP;
        else
            oPPCmpt.textContent = oInvitWizard.NbPP;
    }


    var oAdrCmpt = document.getElementById("cmptAdrSpan");
    if (oAdrCmpt) {
        if (typeof oAdrCmpt.innerText !== 'undefined')
            oAdrCmpt.innerText = oInvitWizard.NbAdr;
        else
            oAdrCmpt.textContent = oInvitWizard.NbAdr;
    }

}


nsInvitWizard.selectAllInvitOnPage = function (obj, nAction) {
    var elem = obj.parentNode || obj.parentElement;
    while (elem.tagName != 'TR') {

        elem = elem.parentNode || elem.parentElement;

        if (elem.tagName == 'TBODY')
            return;
    }


    var oWizardbody = document.getElementById("wizardbody");
    var bOnePage = (oWizardbody.querySelector("[nbpage='1']")) !== null;


    var nValCheck = "0";
    var bNoDbl = (nAction == nsInvitWizard.Action.ACTION_SELECTALLINVIT_NODBL);

    var allInvit = document.getElementById("PPList").querySelectorAll("a[chk='" + nValCheck + "'][name='chkMF']");

    if (allInvit.length > 0) {
        try {
            top.setWait(true);


            var oChckLst = [];
            for (var nI = 0; nI < allInvit.length; nI++) {
                var myChk = allInvit[nI];


                var elem = myChk.parentNode || myChk.parentElement;
                while (elem.tagName.toLowerCase() != 'tr') {

                    elem = elem.parentNode || elem.parentElement;

                    if (elem.tagName.toLowerCase() == 'tbody' || elem.tagName.toLowerCase() == 'body')
                        continue;
                }

                var nAdrId = getAttributeValue(elem, "eseladrid");
                var nPpId = getAttributeValue(elem, "eselppid");





                var IsDBL = bNoDbl && (getAttributeValue(elem, "ednadrdbl") == 1 || getAttributeValue(elem, "ednppdbl") == 1);

                if (!IsDBL) {
                    chgChk(myChk);

                    var myI = { ppid: nPpId, adrid: nAdrId };

                    if (oInvitWizard && oInvitWizard.DeleteMode) {
                        var nTplId = getAttributeValue(elem, "eid").split('_')[1];
                        myI.tplid = nTplId;
                    }

                    oChckLst.push(myI);
                }

            }


            nsInvitWizard.chkInvitFileList(oChckLst)

            top.setWait(false)
        }
        catch (e) {
            top.eAlert(1, top._res_6237, ""); //Une erreur est survenue
            top.setWait(false);
        }
    }

}

//Dé/selectionne toutes les invitations
nsInvitWizard.selectAllInvit = function (obj, nAction) {

    var elem = obj.parentNode || obj.parentElement;
    while (elem.tagName != 'TR') {

        elem = elem.parentNode || elem.parentElement;

        if (elem.tagName == 'TBODY')
            return;
    }

    var oWizardbody = document.getElementById("wizardbody");
    var nBkm = getAttributeValue(oWizardbody, "bkm");

    //Si aucune action n'est explicitement indiqué, test par rapport à l'état de la case à cocher
    if (typeof nAction == "undefined") {
        var nAction = nsInvitWizard.Action.ACTION_UNSELECTALLINVIT; // décoche all
        if (obj.getAttribute("chk") == "1")
            nAction = nsInvitWizard.Action.ACTION_SELECTALLINVIT; // coche all
    }

    // MCR 39377 : correction du bug sur un : Ajout d'un filtre lors de l'ajout en masse ++ d'une cible, 
    // à l'étape "Sélectionner Tous les Destinataires" on a un message erreur : "Aucun Filtre n'a été sélectionné" 
    //_eCurentSelectedFilter est une variable globale déclarée dans eFilterReportList.js qui est vide sur un nouveau filtre.
    // utilisation de la variable globale : _activeFilter qui contient le nouveau filtre en cours

    if (_eCurentSelectedFilter == null) {
        var aEid = ["0", _activeFilter];     // utilisation de aEid[1] dans la suite
    }
    else {
        var aEid = getAttributeValue(_eCurentSelectedFilter, "eid").split("_");
        if (aEid.length < 2) {
            eAlert(0, top._res_719, top._res_430); //Aucun filtre sélectionné
            return;
        }
    }

    var oInvitWizardUpdater = new eUpdater("mgr/eInvitWizardManager.ashx", 0);

    oInvitWizardUpdater.ErrorCallBack = function () { setWait(false); };
    oInvitWizardUpdater.addParam("fid", aEid[1], "post");
    oInvitWizardUpdater.addParam("bkm", nBkm, "post");
    oInvitWizardUpdater.addParam("filefromid", oInvitWizard.FileFromId, "post");    //ID de l'event de départ
    oInvitWizardUpdater.addParam("tabfrom", oInvitWizard.TabFrom, "post");    //Descid de l'event de départ
    oInvitWizardUpdater.addParam("action", nAction, "post");
    oInvitWizardUpdater.addParam("delete", oInvitWizard.DeleteMode ? "1" : "0", "post");

    //Récupération des options sur les adresse à retenir
    var oAct = document.getElementById("radioAdrAct")
    if (oAct && getAttributeValue(oAct, "chk") == "1") {
        oInvitWizardUpdater.addParam("fltact", "1", "post");
    }

    var oPrinc = document.getElementById("radioAdrPrinc")
    if (oPrinc && getAttributeValue(oPrinc, "chk") == "1") {
        oInvitWizardUpdater.addParam("fltprinc", "1", "post");
    }

    //Type de dédoublonnage (pp ou adr)
    var oDblAct = document.getElementById("radioGreyDbl");
    if (oDblAct && getAttributeValue(oDblAct, "chk") == "1") {
        var oSelect = document.getElementById("invitSelectTypDbl");
        var nTypDbl = oSelect.options[oSelect.selectedIndex].value;
        oInvitWizardUpdater.addParam("typdbl", nTypDbl, "post");
    }

    //Récupération des options sur les consentements
    var oCampaignType = document.getElementById("invitSelectCampaignType")
    if (oCampaignType) {
        var oCampaignTypeValue = oCampaignType.options[oCampaignType.selectedIndex].value;
        if (oCampaignTypeValue != "" && oCampaignTypeValue != "0") {
            oInvitWizardUpdater.addParam("fltcampaigntype", oCampaignTypeValue, "post");
        }
    }

    var oTypeConsent = document.getElementById("invitHiddenTypeConsent")
    if (oTypeConsent && oTypeConsent.hasAttribute("value")) {
        var oTypeConsentValue = oTypeConsent.getAttribute("value");
        if (oTypeConsentValue != "" && oTypeConsentValue != "0") {
            oInvitWizardUpdater.addParam("flttypeconsent", oTypeConsentValue, "post");
        }
    }

    var oOptin = document.getElementById("invitChbxOptin")
    if (oOptin && getAttributeValue(oOptin, "chk") == "1" && oOptin.hasAttribute("value")) {
        var fltoptinValue = oOptin.getAttribute("value");
        if (fltoptinValue != "" && fltoptinValue != "0")
            oInvitWizardUpdater.addParam("fltoptin", fltoptinValue, "post");
    }

    var oOptout = document.getElementById("invitChbxOptout")
    if (oOptout && getAttributeValue(oOptout, "chk") == "1" && oOptout.hasAttribute("value")) {
        var fltoptoutValue = oOptout.getAttribute("value");
        if (fltoptoutValue != "" && fltoptoutValue != "0")
            oInvitWizardUpdater.addParam("fltoptout", fltoptoutValue, "post");
    }

    var oNoopt = document.getElementById("invitChbxNoopt")
    if (oNoopt && getAttributeValue(oNoopt, "chk") == "1") {
        oInvitWizardUpdater.addParam("fltnoopt", "1", "post");
    }

    setWait(true);
    oInvitWizardUpdater.send(nsInvitWizard.updtInvitCnt, nAction);
}

///Action lancé sur le changement d'étape
nsInvitWizard.SwitchStep = function (step) {
    switch (parseInt(step)) {
        case 1:
            break;
        case 2:
            if (getAttributeValue(_eCurentSelectedFilter, "iq") == "1") {
                var nTab = getAttributeValue(_eCurentSelectedFilter, "eft");
                var filterId = getAttributeValue(_eCurentSelectedFilter, "eid").split('_')[1];
                doFormularFilter(nTab, filterId, FROM_INVIT);
            }
            else {
                if (!oInvitWizard.RecurrentMode)
                    UpdatePPList();
            }
            break;
    }
}

///Change l'étape du wizard - traitement de fin d'appel
nsInvitWizard.PostSwitch = function (step) {
    switch (parseInt(step)) {
        case 1:
            // Vider le contenu du div de liste
            var oDiv = document.getElementById("PPList");
            if (oDiv) {
                oDiv.innerHTML = "";
            }
            break;
        case 2:
            if (!oInvitWizard.RecurrentMode)
                UpdatePPList();
            break;
    }
}

///Grise les contacts déjà sélectionné
nsInvitWizard.greyElements = function () {
    var oGreyDisabled = document.getElementById("radioGreyDbl");
    var oSelect = document.getElementById("invitSelectTypDbl");
    var oBaseDocument = wizardIframe.contentDocument;

    if (oSelect && oBaseDocument) {
        var oMT = oBaseDocument.getElementById("invitWizardListDiv");

        var nTypDbl = oSelect.options[oSelect.selectedIndex].value;

        if (getAttributeValue(oGreyDisabled, "chk") != "1")
            setAttributeValue(oMT, "edninvitdbl", 0);
        else
            setAttributeValue(oMT, "edninvitdbl", nTypDbl);
    }
    else {

    }

}

///Ouvre le menu contextuel de la case à cocher d'affectation des invitations
nsInvitWizard.getContextMenu = function (chkBox) {
    var obj_pos = getAbsolutePosition(chkBox);
    eCMInvitWizard = new eContextMenu(null, obj_pos.y - 10, obj_pos.x, "eCMInvitWizard");

    //Tous
    eCMInvitWizard.addItemFct(top._res_22, function () { nsInvitWizard.selectAllInvit(chkBox, nsInvitWizard.Action.ACTION_SELECTALLINVIT); eCMInvitWizard.hide(); }, 0, 1, "actionItem", top._res_22);

    //Aucun
    eCMInvitWizard.addItemFct(top._res_436, function () { nsInvitWizard.selectAllInvit(chkBox, nsInvitWizard.Action.ACTION_UNSELECTALLINVIT); eCMInvitWizard.hide(); }, 0, 1, "actionItem", top._res_436);

    //Tous sauf les fiches grisées
    eCMInvitWizard.addItemFct(top._res_1013, function () { nsInvitWizard.selectAllInvit(chkBox, nsInvitWizard.Action.ACTION_SELECTALLINVIT_NODBL); eCMInvitWizard.hide(); }, 0, 1, "actionItem", top._res_1013);

}

//Action suplémentaire sur click line - Surcharge sur init de eInvitWizard
nsInvitWizard.selectLine = function (obj) {

    if (!_activeFilter) {
        var aEid = getAttributeValue(_eCurentSelectedFilter, "eid").split("_");
        if (aEid.length < 2) {
            return;
        }
    }

    //Active étape 2
    var stepDiv = document.getElementById("step_2");
    removeClass(stepDiv, "state_grp");
    addClass(stepDiv, "state_grp-validated");
}

///Retourne un eUpdater pour la création des fiches invitations comportant les paramètres communs au mode bulk et manuelle
nsInvitWizard.GetSelectUpdater = function () {
    var upd = new eUpdater("mgr/eTreatmentManager.ashx", 0);

    //Contexte de création des invitations
    upd.addParam("tabfrom", oInvitWizard.TabFrom, "post");        // event de base
    upd.addParam("filefromid", oInvitWizard.FileFromId, "post");    //ID de l'event de départ

    /* TODO : la différence d'utilisation entre ces 2 variables est a checker */
    upd.addParam("targettab", oInvitWizard.TabInvit, "post"); // descid de la table d'invitation
    upd.addParam("FileAffectTabId", oInvitWizard.TabInvit, "post"); // descid de la table d'invitation

    //HLA & SHA
    upd.addParam("recurrentMode", oInvitWizard.RecurrentMode ? "1" : "0", "post");
    upd.addParam("scheduleId", oInvitWizard.ScheduleId, "post");

    //Parametre de la fiche invitation
    if (!oInvitWizard.DeleteMode && !oInvitWizard.RecurrentMode) {
        //Valeur des champs
        upd.addParam('FldNewValue', oAffectFile.serialize(), "post");
    }

    return upd;
}

///Lancement du traitement "sélection manuelle"
nsInvitWizard.RunTreatment = function () {
    var upd = nsInvitWizard.GetSelectUpdater();

    if (!oInvitWizard.DeleteMode) {
        upd.addParam("operation", nsTreatment.TraitementOperation.INVIT_CREA, "post");

        //Ferme la fenêtre  de création
        oAffectFile.hide();
    }
    else
        upd.addParam("operation", nsTreatment.TraitementOperation.INVIT_SUP, "post");

    // Créer une methode en cas d'une erreur il l'execute
    upd.ErrorCallBack = function () { StopProcessTreatment(); top.setWait(false); };
    upd.send(ReturnRunTreatment);
    top.setWait(true);
}

///Lancement du traitement pour l'ajout "bulk"
nsInvitWizard.RunTreatmentAll = function () {

    var upd = nsInvitWizard.GetSelectUpdater();

    if (oInvitWizard.DeleteMode) {
        upd.addParam("operation", nsTreatment.TraitementOperation.INVIT_SUP_BULK, "post");
    } else {
        upd.addParam("operation", nsTreatment.TraitementOperation.INVIT_CREA_BULK, "post");

        //Récupération des options sur les adresses à retenir
        var oAct = document.getElementById("radioAdrAct");
        if (oAct && getAttributeValue(oAct, "chk") == "1")
            upd.addParam("fltact", "1", "post");

        var oPrinc = document.getElementById("radioAdrPrinc");
        if (oPrinc && getAttributeValue(oPrinc, "chk") == "1")
            upd.addParam("fltprinc", "1", "post");

        var oDoNotDbl = document.getElementById("chkDoNotDbl");
        if (oDoNotDbl && getAttributeValue(oDoNotDbl, "chk") == "1")
            upd.addParam("donotdbladr", "1", "post");

        if (!oInvitWizard.RecurrentMode) {
            //Ferme la fenêtre  de création
            oAffectFile.hide();
        } else {
            var oCampaignType = document.getElementById("invitSelectCampaignType")
            if (oCampaignType) {
                var oCampaignTypeValue = oCampaignType.options[oCampaignType.selectedIndex].value;

                if (oCampaignTypeValue != "" && oCampaignTypeValue != "0") {
                    upd.addParam("fltcampaigntype", oCampaignTypeValue, "post");
                }
            }

            var oTypeConsent = document.getElementById("invitHiddenTypeConsent")
            if (oTypeConsent && oTypeConsent.hasAttribute("value")) {
                var oTypeConsentValue = oTypeConsent.getAttribute("value");
                if (oTypeConsentValue != "" && oTypeConsentValue != "0") {
                    upd.addParam("flttypeconsent", oTypeConsentValue, "post");
                }
            }

            var oOptin = document.getElementById("invitChbxOptin")
            if (oOptin && getAttributeValue(oOptin, "chk") == "1" && oOptin.hasAttribute("value")) {
                var fltoptinValue = oOptin.getAttribute("value");
                if (fltoptinValue != "" && fltoptinValue != "0")
                    upd.addParam("fltoptin", fltoptinValue, "post");
            }

            var oOptout = document.getElementById("invitChbxOptout")
            if (oOptout && getAttributeValue(oOptout, "chk") == "1" && oOptout.hasAttribute("value")) {
                var fltoptoutValue = oOptout.getAttribute("value");
                if (fltoptoutValue != "" && fltoptoutValue != "0")
                    upd.addParam("fltoptout", fltoptoutValue, "post");
            }

            var oNoopt = document.getElementById("invitChbxNoopt")
            if (oNoopt && getAttributeValue(oNoopt, "chk") == "1") {
                upd.addParam("fltnoopt", "1", "post");
            }
        }
    }

    ///Id du filtre
    if (_eCurentSelectedFilter == null) {
        var aEid = ["0", _activeFilter];     // utilisation de aEid[1] dans la suite
    }
    else
        var aEid = getAttributeValue(_eCurentSelectedFilter, "eid").split("_");

    if (aEid.length < 2) {
        eAlert(0, top._res_719, top._res_430); //Aucun filtre sélectionné
        return;
    }

    upd.addParam('fid', aEid[1], "post");
    upd.addParam('all', "1", "post");

    // Créer une méthode en cas d'une erreur il l'execute
    upd.ErrorCallBack = function () { StopProcessTreatment(); top.setWait(false); };
    upd.send(ReturnRunTreatment);
    top.setWait(true);
}

///Fonction valider pour l'ajout des invitations
nsInvitWizard.AddInvit = function (fileTab, tabName, parentTabName, bIsPlanning) {
    var chkboxImmediat = document.getElementById("rbAddImmediateRecipFilter");
    var chkboxRecurrent = document.getElementById("rbAddRecurrentRecipFilter");

    if (chkboxImmediat && chkboxRecurrent) {
        if (chkboxImmediat.checked)
            nsInvitWizard.AddInvitImmediate(fileTab, tabName, parentTabName, bIsPlanning);
        else if (chkboxRecurrent.checked)
            nsInvitWizard.AddInvitRecurrent(fileTab, tabName, parentTabName, bIsPlanning);
    } else {
        nsInvitWizard.AddInvitImmediate(fileTab, tabName, parentTabName, bIsPlanning);
    }
}

///Fonction valider pour l'ajout des invitations
nsInvitWizard.AddInvitImmediate = function (fileTab, tabName, parentTabName, bIsPlanning) {
    if (oInvitWizard.NbInvit == 0) {
        //alert(top._res_1069);
        alert('Vous devez choisir au moins une adresse.'); // TODO RES : 1069 - remplacer "<ITEM>" par le nom du fichier adresse 
        return;
    }

    oAffectFile = new AffectFile(fileTab, tabName, parentTabName, bIsPlanning);
    oAffectFile.show = nsInvitWizard.show;
    oAffectFile.bIsInvit = true;
    oAffectFile.RunTreatmentFct = nsInvitWizard.RunTreatment;
    oAffectFile.show();
}

//Fonction valider pour l'ajout récurrent de destinataires répondant au filtre sélectionné
nsInvitWizard.AddInvitRecurrent = function (fileTab, tabName, parentTabName, bIsPlanning) {
    if (!oInvitWizard.ControlStep(2)) {
        return;
    }

    nsInvitWizard.RunTreatmentAll();
}

///Fonction valider pour la suppression des invitation version selection manuelle
nsInvitWizard.SuppInvit = function (fileTab, tabName, parentTabName, bIsPlanning) {

    var title = oInvitWizard.TabName;
    var sMsg = "";
    if (oInvitWizard.NbInvit == 0) {
        sMsg = top._res_1069.replace("<ITEM>", title);
        eAlert(MsgType.MSG_CRITICAL.toString(), top._res_719, sMsg); //L'ajout récurrent n'autorise pas la selection d'un filtre formulaire
        return;
    }
    else {

        
         sMsg = top._res_832.replace("\'<ITEM>\'", oInvitWizard.NbInvit) + ".<br\>" + top._res_833;
        if (oInvitWizard.NbInvit > 20000 && oInvitWizard.HasORM ) 
            sMsg = top._res_2718.replace("\"<TPL_NAME>\"", title);
        

        var oModCfmSup = eConfirm(1, top._res_68, sMsg, "", 550, 200,
            function () { nsInvitWizard.RunTreatment(); });
    }
}

///Fonction valider pour l'ajout des invitations version bulk
nsInvitWizard.AddInvitAll = function () {
    oAffectFile = new AffectFile(oInvitWizard.TabInvit);
    oAffectFile.show = nsInvitWizard.show;
    oAffectFile.bIsInvit = true;
    oAffectFile.RunTreatmentFct = nsInvitWizard.RunTreatmentAll;
    oAffectFile.show();
}

//Surcharge de la fonction "show" de affectfile
nsInvitWizard.show = function () {

    var title = oAffectFile.tabName ? oAffectFile.tabName : "Invitations"; // TODO : RES - CHOISIR LA RES

    let nb = parseInt(oInvitWizard.NbAdr);

    var fct = function () {
        shFileInPopup(oAffectFile.tab, 0, title, 0, 0, 0, null, true, function (eModFile) {
            oAffectFile.valid(eModFile);
        }, 8); //Type 8 : invitation
    }

    if (!isNaN(nb) && nb > 20000 && oInvitWizard.HasORM) {
        eConfirm(1, top._res_68, top._res_2717.replace("\"<TPL_NAME>\"", title), "", 550, 200,
            fct);
    }
    else
        fct();
}

nsInvitWizard.ChangeMediaTypeSelect = function (sender) {
    nsInvitWizard.RefreshUnsubscribeCheckboxes();

    var selectedMediaType = sender.options[sender.selectedIndex].value;

    var oWizardbody = document.getElementById("wizardbody");
    var nBkm = getAttributeValue(oWizardbody, "bkm");

    var oInvitManager = new eUpdater("mgr/eInvitWizardManager.ashx", 0);
    oInvitManager.ErrorCallBack = function () { };
    oInvitManager.addParam("bkm", nBkm, "post");
    oInvitManager.addParam("action", nsInvitWizard.Action.ACTION_RELOAD_CAMPAIGN_TYPE, "post");
    oInvitManager.addParam("mediaType", selectedMediaType, "post");

    setWait(true);
    oInvitManager.send(nsInvitWizard.ReloadCampaignTypeSelect);
}

nsInvitWizard.ReloadCampaignTypeSelect = function (oRes) {
    var oldSelectedValue = "0";

    try {
        var invitSelectCampaignType = document.getElementById("invitSelectCampaignType");
        oldSelectedValue = invitSelectCampaignType.options[invitSelectCampaignType.selectedIndex].value

        if (invitSelectCampaignType != null) {
            for (var i = 0; i < invitSelectCampaignType.options.length;) {
                if (invitSelectCampaignType.options[i].value != "0")
                    invitSelectCampaignType.remove(i);
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
                    invitSelectCampaignType.add(option);
                }
            }

        }
    }
    catch (e) {
    }
    finally {
        setWait(false);
    }

    if (oldSelectedValue != "0")
        nsInvitWizard.ChangeCampaignTypeSelect();
}

nsInvitWizard.ChangeFilters = function (sender) {
    if (!oInvitWizard.RecurrentMode)
        UpdatePPList(1);
}

nsInvitWizard.ChangeCampaignTypeSelect = function (sender) {
    nsInvitWizard.RefreshUnsubscribeCheckboxes();

    nsInvitWizard.ChangeFilters();
}

nsInvitWizard.RefreshUnsubscribeCheckboxes = function () {
    var selectedMediaType = "0";
    var selectedCampaignType = "0";

    var invitSelectMediaType = document.getElementById("invitSelectMediaType");
    var invitSelectCampaignType = document.getElementById("invitSelectCampaignType");

    if (invitSelectMediaType != null)
        selectedMediaType = invitSelectMediaType.options[invitSelectMediaType.selectedIndex].value;

    if (invitSelectCampaignType != null)
        selectedCampaignType = invitSelectCampaignType.options[invitSelectCampaignType.selectedIndex].value;

    var spanOptIn = document.getElementById("invitChbxOptinContainer");
    var spanOptOut = document.getElementById("invitChbxOptoutContainer");
    var spanNoOpt = document.getElementById("invitChbxNooptContainer");
    var spansOpt = [spanOptIn, spanOptOut, spanNoOpt];

    for (var i = 0; i < spansOpt.length; ++i) {
        var spanOpt = spansOpt[i];
        if (spanOpt != null) {
            if (selectedMediaType != "0" && selectedCampaignType != "0") {
                removeClass(spanOpt, "hidden");
            }
            else {
                addClass(spanOpt, "hidden");
                var chckbx = spanOpt.querySelector("input[type='checkbox']");
                if (chckbx != null)
                    chckbx.checked = false;
            }
        }
    }
}


///Objet Wizard pour les invitations
/// Utilisé dans eWizard.js - Initialisé par le eInvitWizardRenderer
/// Contient des propriétés/méthodes génériques au wizard ainsi que des propriétés spécifiques
function eInvitWizard(nTab, nTabFrom, nFileId) {
    this.TabInvit = nTab;               // Table des invitations  //SHA : TargetTab
    this.TabFrom = nTabFrom;            // Table de l'évènement
    this.FileFromId = nFileId;          // Id de l'évènement //SHA : ParentEvtId
    this.DeleteMode = false;            // Mode suppression
    this.RecurrentMode = false;         // Mode recurrent

    this.NbInvit = 0;   // Nombre d'invitation cochées
    this.NbPP = 0;      // Nombre de pp cochées
    this.NbAdr = 0;     // Nombre d'adresses cochées

    // HLA & SHA
    this.ScheduleModal = null;     // Modal pour la selection de la recurrence
    this.ScheduleId = 0;            // Id du Schedule en BDD

    var me = this;

    this.ControlStep = function (step) {

        switch (parseInt(step)) {
            case 1: //Choix du filtre
                // On n'autorise pas les filtre formulaire pour la recurrence
                if (oInvitWizard.RecurrentMode && getAttributeValue(_eCurentSelectedFilter, "iq") == "1") {
                    eAlert(MsgType.MSG_CRITICAL.toString(), top._res_719, top._res_2065); //L'ajout récurrent n'autorise pas la selection d'un filtre formulaire
                    return false;
                }

                if (_activeFilter)
                    return true;

                var aEid = getAttributeValue(_eCurentSelectedFilter, "eid").split("_");
                if (aEid.length < 2) {
                    eAlert(MsgType.MSG_CRITICAL.toString(), top._res_719, top._res_430); //Aucun filtre sélectionné
                    return false;
                }

                _activeFilter = aEid[1];
                return true;
                break;

            case 2: //Choix des destinataires

                if (!oInvitWizard.RecurrentMode)
                    return true;

                // Vérification des champs obligatoire dans l'étape 2 pour le cas de la recurrence
                var obligatFields = "";

                //2040,"Planification"
                if (me.ScheduleId <= 0)
                    obligatFields += top._res_2040 + "<br />";

                if (obligatFields != "") {
                    eAlert(MsgType.MSG_CRITICAL.toString(), top._res_372, top._res_6564, obligatFields);
                    return false;
                }

                return true;
                break;
        }
    }

    ///Type de rapport
    /// Utilisé dans eWizard.js via oCurrentWizard (cf la méthode init dans eWizard.js
    this.GetType = function () {
        return "-1"; //Le type d'un wizard fait référence à un type de report. Dans le cas d'un wizard ++, cela n'a pas de sens, on passe donc -1 (NONE dans l'énum)
    }

    //Surcharge du selectline
    if (typeof selectLine == "function") {

        var origFct = selectLine;  // fonction selecLine d'origine

        selectLine = function (obj) {
            origFct(obj);
            nsInvitWizard.selectLine(obj); // applique le selecline spécifique
        }
    }

    //SHA
    this.openScheduleParameterValidReturn = function (oModal) {
        me.ScheduleModal.getIframe().Valid(me.ValidInvitRecipScheduleTreatment);
    }

    //SHA
    this.openScheduleParameterCancelReturn = function () {
        me.ScheduleModal.hide();
    }

    //SHA
    this.ValidInvitRecipScheduleTreatment = function (oRes) {
        me.ScheduleId = getXmlTextNode(oRes.getElementsByTagName("scheduleid")[0]);
        me.ScheduleModal.hide();

        var scheduleInfo = document.getElementById("lnkScheduleInfo");
        if (scheduleInfo) {
            scheduleInfo.style.display = "";
            SetText(scheduleInfo, " : \"" + getXmlTextNode(oRes.getElementsByTagName("scheduleinfo")[0]) + "\"");
        }
    }

    //SHA
    this.openScheduleParameter = function () {

        // On choisi le prochain créneau d'heure
        var d = new Date();
        while (d.getMinutes() % 30 != 0) {
            d.setMinutes(d.getMinutes() + 1);
        }
        var hours = ("0" + d.getHours()).slice(-2);
        var minutes = ("0" + d.getMinutes()).slice(-2);

        me.ScheduleModal = new eModalDialog(top._res_1049, 0, "eSchedule.aspx", 450, 500);

        var modal = me.ScheduleModal;
        modal.addParam("scheduletype", 3, "post");
        modal.addParam("iframeScrolling", "yes", "post");
        modal.addParam("EndDate", 0, "post");
        modal.addParam("BeginDate", 0, "post");
        modal.addParam("ScheduleId", me.ScheduleId, "post");
        modal.addParam("Tab", 0, "post");
        modal.addParam("Workingday", "TODO", "post");
        modal.addParam("calleriframeid", 0, "post");
        modal.addParam("hour", hours + ":" + minutes, "post");
        modal.addParam("AppType", 0, "post");

        modal.ErrorCallBack = me.openScheduleParameterCancelReturn();

        modal.show();
        modal.addButtonFct(top._res_29, function () { me.openScheduleParameterCancelReturn(); }, "button-gray", 'cancel');
        modal.addButtonFct(top._res_28, function () { me.openScheduleParameterValidReturn(); }, "button-green");
    };
}

//SHA
///Afficher ou cacher la PPList de sélection de destinataires PP selon le choix de l'ajout dans la 1ère étape
nsInvitWizard.OnSelectAddFilter = function (recurrentMode) {
    var oPPList = document.getElementById("PPList");
    var oFrequencyPlanif = document.getElementById("InvitListFilterFrequency");
    var oFilterGreyDbl = document.getElementById("FilterGreyDbl");
    var oFilterNotDbl = document.getElementById("FilterNotDbl");
    var oFilterCmpt = document.getElementById("FilterCmpt");

    function DisplayPPList(bAddMode) {
        if (oPPList)
            oPPList.style.display = (bAddMode) ? "block" : "none";
        oFilterGreyDbl.style.display = (bAddMode) ? "block" : "none";
        oFilterCmpt.style.display = (bAddMode) ? "block" : "none";
    };

    function DisplayFrequency(bAddMode) {
        oInvitWizard.RecurrentMode = bAddMode;

        if (oFrequencyPlanif)
            oFrequencyPlanif.style.display = (bAddMode) ? "block" : "none";
        oFilterNotDbl.style.display = (bAddMode) ? "block" : "none";
    };

    if (recurrentMode) {
        //Mode récurrent
        DisplayPPList(false);
        DisplayFrequency(true);
    } else {
        //Mode normal
        DisplayPPList(true);
        DisplayFrequency(false);
    }
}