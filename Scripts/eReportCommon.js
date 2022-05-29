//CONSTANTES
var EXPORT_STANDARD = 0;  // Fenêtre attente, pas de mail
var EXPORT_MAIL_ONLY = 1; // Uniquement par mail
var EXPORT_CHOICE = 2; // Mail + Fenêtre attente optionnelle
var REPORT_CHECKMODE = 9;    //Constante de l'action de check du statut du rapport
var REPORTFORMAT_POWERBI = 7;

var nTimeOutReport = (1000 * 60 * 5); //TimeOut de 5 minutes avant arrêt forcé de l'attente de l'export (variables en millisecondes)
var nCheckTimeOutReport = (1000 * 5); //TimeOut de 5 secondes entre chaque appels avant la vérifications de l'état du rapport
//Variables globales
var exportMode = null;  //Type d'affichage d'export paramétré pour l'utilisateur en cours
var nServerReportId = -1;   //Identifiant du rapport en cours
var fctReportTimeOut = null;    //timeout de fonction
var fctReportTimeOutCheck = null;    //timeout de fonction
// Alert de demande d'attente ou informe l'utilisatrice que la demande d'export a été pris en compte
var alertAttentReport = null;


//Function qui nous permet de faire appel à notre function ExecuteInsert(bFile, bBKM, nTabBkm)
function CallExecuteInsert() {

    if (_eCurentSelectedFilter == null)
        return;

    var currentView = top.getCurrentView();

    var eid = getAttributeValue(_eCurentSelectedFilter, "eid").split("_");
    var id = eid[1];    //ReportId sélectionné

    if (id == "-1") {
        var SpanNbElem = top.document.getElementById("SpanNbElem");
        if (SpanNbElem) {
            var nbFiles = getNumber(GetText(SpanNbElem));
            var oeParam = getParamWindow();
            var nExportMaxNbFiles = getNumber(oeParam.GetParam("ExportMaxNbFiles"));

            if (nExportMaxNbFiles > 0 && nbFiles > nExportMaxNbFiles) {
                eAlert(0, top._res_8675, top._res_8676.replace("<NB>", nExportMaxNbFiles), top._res_8677);
                return;
            }
        }
    }


    var bHasFormular = getAttributeValue(_eCurentSelectedFilter, "hasform") != "";
    var bIsSpec = getAttributeValue(_eCurentSelectedFilter, "spec") != "";


    //MOU/SPH 14/11/2014 #34560 : ligne suivante commentée suite a la non-prise en compte du filtre formulaire sur les signets 
    //var bFile = (currentView.indexOf("FILE_") == 0) && getAttributeValue(document.getElementById("mainDiv"), "tabBKM") == "-1";

    if (bHasFormular && !bIsSpec /* && !bFile */) {

        var sFilterForm = getAttributeValue(_eCurentSelectedFilter, "hasform");
        var aFilterForm = sFilterForm.split("|");

        if (aFilterForm.length >= 2) {

            doFormularFilter(aFilterForm[0], aFilterForm[1], 1);
            return;
        }
    }
    else {
        //Confirmer le titre si necessaire
        ConfirmTitle(_eCurentSelectedFilter);
    }

}

function showReportInformation(reportId) {
    var oModalReportInformation = new eModalDialog(top._res_6290, 0, "eReportInformationDialog.aspx", 850, 350);
    oModalReportInformation.addParam("reportid", reportId, "post");
    oModalReportInformation.show();
    oModalReportInformation.addButton(top._res_30, function () { oModalReportInformation.hide(); }, "button-gray", null); // Fermer
}

// Pour confirmer le titre avant de lancer le rapport d'impression
function ConfirmTitle(obj) {

    var bConfirm = obj != "undefined" && obj != null && getAttributeValue(obj, "confirm") == "1";
    if (bConfirm) {

        var value = getAttributeValue(obj, "dbv");

        var oConfirmModal = new eModalDialog(top._res_86, '2', null, 460, 200);
        oConfirmModal.hideMaximizeButton = true;
        oConfirmModal.setPrompt(top._res_172, value);
        oConfirmModal.show();

        oConfirmModal.addButton(top._res_29,
            function () {
                oConfirmModal.hide();
            }, "button-gray", null); // Annuler

        oConfirmModal.addButton(top._res_28,
            function () {
                RunReport(oConfirmModal.getPromptValue());
                oConfirmModal.hide();
            }, "button-green"); // Valider

    } else {

        RunReport();
    }
}

//ExecuteInsert : Permet de déclencher un export
//bFile : - Mode Fiche <= true
//        - Mode List <= false
//bBKM :  - Menu droit <= false
//        - Mode signet <= true
//TabBkm : Table du signet si export depuis signet
function RunReport(newTitle) {

    // ASY : #31582 - Ajouter Tous dans l'assistant Rapport -Suite : Si aucun rapport n'est choisi dans la liste ( autre que de type Graphique) et qu'on click sur appliquer : erreur js
    if (_eCurentSelectedFilter == null)
        return;

    var eid = _eCurentSelectedFilter.attributes["eid"].value.split("_");
    var id = eid[1];    //ReportId sélectionné

    // ASY #22215 : [Dev] - Ajouter Tous dans l'assistant Rapport - prise en compte du type tous=99   
    //var lstreportype = document.getElementById("lstreportype");
    //var selectLstReportype = lstreportype.options[lstreportype.options.selectedIndex].value;   //Type de Rapport
    var selectLstReportype = getAttributeValue(_eCurentSelectedFilter, "typ");

    var bIsSpecV7 = getAttributeValue(_eCurentSelectedFilter, "spec") == "1";

    if (selectLstReportype == 4) {
        //Type rapport spécifique (XRM)

        var nSpecId = getAttributeValue(_eCurentSelectedFilter, "specid");
        //recherche de la table des report

        var oMainDiv = document.getElementById("mainDiv");

        var nTab = getAttributeValue(oMainDiv, "tab");
        var nTabBkm = getAttributeValue(oMainDiv, "tabBKM");

        //Appel depuis export signet
        if (nTabBkm != null && nTabBkm != "" && nTabBkm != "-1" && nTabBkm != nTab)
            runSpec(nSpecId, nTabBkm);
        else
            runSpec(nSpecId);

        return;
    }
    else if (selectLstReportype == 6) {
        //Rapport type chart
        displayChart(id);
    }
    else if (bIsSpecV7) {

        var SpecdblCLick = _eCurentSelectedFilter.ondblclick;
        if (typeof SpecdblCLick == "function")
            SpecdblCLick();
        return;
    }
    else {
        if (id != null || id != undefined && eFT != null || eFT != undefined) {
            //Recup Le mode Fiche ou List
            var currentView = top.getCurrentView();
            var bList = currentView == "LIST";
            var bFile = (currentView.indexOf("FILE_") == 0);

            var nTab = document.getElementById("mainDiv").attributes["tab"].value;
            var nTabBkm = document.getElementById("mainDiv").attributes["tabBKM"].value;
            //Signet Mode Fiche
            var bBkmFile = top.isBkmFile(nTabBkm);

            var DynamiqueTitle = "";    //TODO mettre le titre du Rapport!
            //On a la possibilité de renommer le rapport d'impression au moment de le lancer
            if (newTitle != "undefined" && newTitle != null && selectLstReportype == 0)
                DynamiqueTitle = newTitle;

            var fid = "";

            //Signet Mode fiche
            if (bBkmFile) {
                fid = parent.document.getElementById("fileDiv_" + nTabBkm).attributes["fid"].value;
                nTab = nTabBkm;
                nTabBkm = 0;
            }
            else if (bFile || nTabBkm > 0) {
                //Mode Fiche normal
                fid = parent.document.getElementById("fileDiv_" + parent.nGlobalActiveTab).attributes["fid"].value;    //File Id du mode fiche en cours
            }
            var eid = _eCurentSelectedFilter.attributes["eid"].value.split("_");

            var upd = new eUpdater("mgr/eReportManager.ashx", 0);

            upd.addParam("reporttype", selectLstReportype, "post");
            upd.addParam("fid", fid, "post");
            upd.addParam("TabFrom", nTab, "post");    //Table de la liste/fiche en cours
            upd.addParam("TabBkm", nTabBkm, "post"); //Table du signet si export depuis signet
            upd.addParam("DynamiqueTitle", DynamiqueTitle, "post");
            upd.addParam("bFile", bFile, "post");
            upd.addParam("reportid", id, "post");
            upd.addParam("operation", 8, "post");

            //Créer une methode en cas d'une erreur il l'execute
            upd.ErrorCallBack = StopProcessReport;
            upd.send(ReturnRunReport);
            top.setWait(true);
        }
    }
}


//fonction utilisée pour lancer un rapport dans xrm en dehors de tout autre contexte
function runReportFromGlobal(reportid, reporttype, nTab, fid, nTabBkm, bFile) {
    if (!bFile)
        bFile = false;

    var upd = new eUpdater("mgr/eReportManager.ashx", 0);

    upd.addParam("reporttype", reporttype, "post");
    upd.addParam("fid", fid, "post");
    upd.addParam("TabFrom", nTab, "post");    //Table de la liste/fiche en cours
    upd.addParam("TabBkm", nTabBkm, "post"); //Table du signet si export depuis signet
    upd.addParam("bFile", bFile, "post");
    upd.addParam("reportid", reportid, "post");
    upd.addParam("operation", 8, "post");

    //Créer une methode en cas d'une erreur il l'execute
    upd.ErrorCallBack = StopProcessReport;
    upd.send(ReturnRunReport);
    top.setWait(true);
}

function printCurrentList() {

    var currentView = top.getCurrentView();
    var bList = currentView == "LIST";
    var bCalendar = (currentView == "CALENDAR");





    var nTab = nGlobalActiveTab



    var upd = new eUpdater("mgr/eReportManager.ashx", 0);

    upd.addParam("reporttype", 0, "post");
    upd.addParam("TabFrom", nTab, "post");    //Table de la liste/fiche en cours    
    upd.addParam("reportid", -1, "post");
    upd.addParam("operation", 8, "post");

    //Créer une methode en cas d'une erreur il l'execute
    upd.ErrorCallBack = StopProcessReport;
    upd.send(ReturnRunReport);
    top.setWait(true);

}



//Appelé au retour de RunReport qui récupère le mode d'export choisi et en fonction de ce qui a été choisi,
//  Va redirigé vers le fonctionnement correspondant :
//  Mode Rapport+Mail => On affiche une fenêtre demandant à l'utilisateur s'il souhaite attendre
//  Mode Standard => Affichage de la fenêtre d'attente et affichage du résultat
//  Mode Envoi de Mail seul => Affichage d'un message avertissement qu'un mail contenant le liens vers le rapport sera envoyé
function ReturnRunReport(oDoc) {

    alertAttentReport = null;
    exportMode = null;
    nServerReportId = -1;


    var bSuccess = (getXmlTextNode(oDoc ? oDoc.getElementsByTagName("success")[0] : "") == "1");
    // dans le cas ou on pas les droits 
    if (!bSuccess) {
        CloseWaitReportStatus();
        return;
    }

    top.setWait(false);

    exportMode = getXmlTextNode(oDoc.getElementsByTagName("exportMode")[0]);
    nServerReportId = getXmlTextNode(oDoc.getElementsByTagName("serverreportid")[0]);

    if (exportMode == EXPORT_CHOICE) {
        eConfirm(1, top._res_86, top._res_6263 + ".<br\>" + top._res_6264 + ".<br\>" + top._res_6265, "", 500, 200,
            function () { GetReportStatut(); });
    }
    else if (exportMode == EXPORT_STANDARD) {
        GetReportStatut();
    }
    else if (exportMode == EXPORT_MAIL_ONLY) {
        // Au bouton fermer, fermer la liste des rapport
        alertAttentReport = eAlert(3, '', top._res_6263 + ".<br\>" + top._res_6264, '', null, null, function () { CloseAlertMailOnly(); });
    }
}

//Fermeture de la avertissant que le rapport arrivera par mail et de la liste des rapport
function CloseAlertMailOnly() {
    if (alertAttentReport)
        alertAttentReport.hide();

    if (typeof top.onCloseReportList == "function")
        top.onCloseReportList();
}
//Enclenche la récupération du statut du rapport
function GetReportStatut() {
    if (alertAttentReport == null) {  //Premier lancement on affiche la fenêtre d'attente et on déclare un timeout d'attente de temps maximum
        top.setWait(true);
        alertAttentReport = eAlert(3, '', top._res_307, '', null, null, function () { top.setWait(false); });
        fctReportTimeOut = window.setTimeout(function () { CloseAfterTimeOut(); }, nTimeOutReport);
    }
    var upd = new eUpdater("mgr/eReportManager.ashx", 0);
    upd.addParam("reportid", nServerReportId, "post");
    upd.addParam("operation", REPORT_CHECKMODE, "post");   //CHECKMODE

    //Créer une methode en cas d'une erreur il l'execute
    upd.ErrorCallBack = StopProcessReport;
    upd.send(RetourReportStatut);
}

//Au retour de GetReportStatut 
//  Vérifie le statut du rapport qui a été checké
//  => Si en état WAIT ou RUNNING
//      On appel de nouveau la méthode GetReportStatut avec un timeout avant execution
//  => Si en état SUCCESS 
//      On affiche le résultat
//  => Si en état MAIL_ERROR
//      On affiche qu'une erreur de mail s'est produite
//  => Sinon c'est qu'on est en erreur alors on affiche un message d'erreur
function RetourReportStatut(oDoc) {
    if (!oDoc || typeof oDoc == "undefined")
        return;

    var bSuccess = (getXmlTextNode(oDoc.getElementsByTagName("success")[0]) == "1");

    var statut = getXmlTextNode(oDoc.getElementsByTagName("Statut")[0]);

    //Satut == En attente ou En cours => On retente tant que l'on dépasse pas le nombre d'appel maximum
    if (statut == "WAIT" || statut == "RUNNING") {
        fctReportTimeOutCheck = window.setTimeout(GetReportStatut, nCheckTimeOutReport);
        return;
    }
    else if (statut == "TIME_OUT") {
        var ErrorDescription = getXmlTextNode(oDoc.getElementsByTagName("ErrorDescription")[0]);
        if (typeof (ErrorDescription) != "undefined" && ErrorDescription != null) {
            var errorUser = ErrorDescription.split("$$|$$")[1];
            if (errorUser != null && typeof (errorUser) != "undefined" && errorUser.length > 0) {
                top.showInfoDialog(top._res_2229, "", errorUser);
            }
            else {
                top.showWarning("", top._res_6162, "");
            }
        }
        else {
            top.showWarning("", top._res_6162, "");
        }

        CloseWaitReportStatus();
    }
    else if (statut == "SUCCESS") {
        //Recupération de l'url du Rapport generé par le service WCF
        var WebPathRapport = getXmlTextNode(oDoc.getElementsByTagName("WebPath")[0]);
        var MsgRapport = getXmlTextNode(oDoc.getElementsByTagName("MsgDetail")[0]);
        var MsgTitle = getXmlTextNode(oDoc.getElementsByTagName("MsgTitle")[0]);

        // TOCHECK POWER BI
        if (WebPathRapport.indexOf(".html") > 0 || WebPathRapport.indexOf(".txt") > 0 || WebPathRapport.indexOf(".pdf") > 0 ||
            WebPathRapport.indexOf(".csv") > 0 || WebPathRapport.indexOf(".xml") > 0) {
            if (WebPathRapport.indexOf(".pdf") > 0 && top.getCurrentView().indexOf("FILE_") == 0) {
                top.RefreshBkm(top.nGlobalActiveTab + top.ATTACHMENT);
            }

            window.open(WebPathRapport);
            CloseWaitReportStatus();

        }
        else {

            top.open(WebPathRapport);
            //document.location.href = WebPathRapport;

            if (fctReportTimeOut)
                window.clearTimeout(fctReportTimeOut);
            fctReportTimeOut = null;

            top.setWait(false);

            Myfct = function () {

                alertAttentReport.hide();

                CloseWaitReportStatus();

            }


            if (alertAttentReport) {

                alertAttentReport.hide();

                alertAttentReport = top.eAlert(4, '', MsgTitle, MsgRapport);
                //alertAttentReport.hide();
            }
        }
    }
    else if (statut == "MAIL_ERROR") {  //MAIL_ERROR : Adresse mail ou smtp non valide...
        var ErrorDescription = getXmlTextNode(oDoc.getElementsByTagName("ErrorDescription")[0]);

        top.showWarning("", top._res_1023, "");
        CloseWaitReportStatus();
    }
    else {  //ERROR
        var ErrorDescription = getXmlTextNode(oDoc.getElementsByTagName("ErrorDescription")[0]);
        if (typeof (ErrorDescription) != "undefined" && ErrorDescription != null) {
            var errorUser = ErrorDescription.split("$$|$$")[1];
            if (errorUser != null && typeof (errorUser) != "undefined" && errorUser.length > 0) {
                top.showWarning("", top._res_72, ErrorDescription.split("$$|$$")[1]);
            }
            else {
                top.showWarning("", top._res_6162, "");
            }
        }
        else {
            top.showWarning("", top._res_6162, "");
        }

        CloseWaitReportStatus();
    }

}
//Fermeture de la fenêtre d'attente, de la liste des rapport et du fond grisé
function CloseWaitReportStatus() {

    if (fctReportTimeOut)
        window.clearTimeout(fctReportTimeOut);

    fctReportTimeOut = null;
    top.setWait(false);
    if (alertAttentReport)
        alertAttentReport.hide();
    if (typeof top.onCloseReportList == "function")
        top.onCloseReportList();
}


//Fermeture en cas de délai d'attente dépassé.
function CloseAfterTimeOut() {
    eAlert(3, top._res_6266, top._res_6263 + ".<br\>" + top._res_6268, '', null, null, null);
    if (fctReportTimeOutCheck)
        window.clearTimeout(fctReportTimeOutCheck);
    fctReportTimeOutCheck = null;
    CloseWaitReportStatus();
}


function StopProcessReport(oDoc) {
    top.setWait(false);

    if (alertAttentReport)
        alertAttentReport.hide();
}

var widgetNS = {};

widgetNS.GanttAdmin = function (nType) {

    eModFile = new eModalDialog("Admin Gantt", 0, "widget/gantt/gantt_admin.aspx", 1024, 1024, "AdminGantt");
    eModFile.onHideFunction = function () { eModFile = null; };




    eModFile.addParam("type", 1, "post");
    eModFile.tab = parent.nGlobalActiveTab;
    eModFile.addParam("tab", parent.nGlobalActiveTab, "post");


    eModFile.ErrorCallBack = function () { top.setWait(false); eModFile.hide(); eModFile = null; };
    eModFile.onIframeLoadComplete = function () { top.setWait(false); };

    eModFile.show();

    eModFile.addButton(top._res_30, function () { eModFile.hide(); }, "button-green");      // Fermer
}