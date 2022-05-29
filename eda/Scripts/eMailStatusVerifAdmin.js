var nsMailStatusVerifAdmin = nsMailStatusVerifAdmin || {};

nsMailStatusVerifAdmin.CountMailToCheckInProgress = false;
nsMailStatusVerifAdmin.LaunchVerificationMailAdress = function () {
    var that = this;
    if (that.CountMailToCheckInProgress)
        return;

    that.CountMailToCheckInProgress = true;
    var modalCalulWait = top.eAlert(3, top._res_2980, top._res_2981, null, null, null, function () { that.CountMailToCheckInProgress = false; }, function () { that.CountMailToCheckInProgress = false; });

    var upd = new eUpdater("eda/Mgr/eAdminMailStatusVerifManager.ashx", 1);
    upd.ErrorCallBack = function () { setWait(false); };
    var bOld = getAttributeValue(document.getElementById("chkIncludeOldMailAdress"), "chk");
    var obj = { OperationType: 1, Old: bOld == "1" ? "1" : "0" }

    upd.json = JSON.stringify(obj);
    setWait(true);

    upd.send(function (oRes) {
        setWait(false);
        modalCalulWait.hide();
        var res = JSON.parse(oRes);
        if (!res.Success) {
            
            var oErrorObj = {
                Type : "0",
                Title : top._res_416, // Erreur
                Msg: top._res_72, // Une erreur est survenue
                DetailMsg: "",
                ErrorDebugMsg : ""

            }

 
            if (res.ErrorMsg != "")
                oErrorObj.Msg = res.ErrorMsg;

            if (res.ErrorDetailMsg != "")
                oErrorObj.DetailMsg = res.ErrorDetailMsg;

            if (res.ErrorDebugMsg !="")
                oErrorObj.DetailDev = res.ErrorDebugMsg; 

            that.CountMailToCheckInProgress = false;
            eAlertError(oErrorObj)


            
        }
        else if (res.MailToCheckCount == 0) {//Si aucune adresse email à vérifier n'a été trouvée 
            top.eAlert(3, top._res_2984, top._res_2985, null, null, null, function () { that.CountMailToCheckInProgress = false; }, function () { that.CountMailToCheckInProgress = false; });
        }
        else {
            eConfirm(1, top._res_201, res.MessageFromService, null, 500, 200, function () { nsMailStatusVerifAdmin.CallJobVerifMailAdress() }, function () { that.CountMailToCheckInProgress = false; });
        }
    });
}

nsMailStatusVerifAdmin.CallJobVerifMailAdress = function () {
    var that = this;
    var upd = new eUpdater("eda/Mgr/eAdminMailStatusVerifManager.ashx", 1);
    upd.ErrorCallBack = function () { setWait(false); };
    var bOld = getAttributeValue(document.getElementById("chkIncludeOldMailAdress"), "chk");
    var obj = { OperationType: 2, Old: bOld ? "1" : "0" }

    upd.json = JSON.stringify(obj);
    setWait(true);

    upd.send(function (oRes) {
        setWait(false);
        var res = JSON.parse(oRes);
        if (!res.Success) {
            var oErrorObj = {
                Type: "0",
                Title: top._res_416, // Erreur
                Msg: top._res_72, // Une erreur est survenue
                DetailMsg: "",
                ErrorDebugMsg: ""
            }
 

            if (res.ErrorMsg != "")
                oErrorObj.Msg = res.ErrorMsg;

            if (res.ErrorDetailMsg != "")
                oErrorObj.DetailMsg = res.ErrorDetailMsg;

            if (res.ErrorCode > 0)
                oErrorObj.DetailDev = res.ErrorDebugMsg; 


            eAlertError(oErrorObj)
            
            that.CountMailToCheckInProgress = false;
        }
        else {
            //Après l'ajout du job de vérif//
            //recharger l'extension après l'ajou du job (pour avoir le nouveau status)
            var old = document.getElementById("chkIncludeOldMailAdress");
            if (old) {
                old.setAttribute("dis", "1");
                old.click();
            }

            var btnLaunchVerif = document.getElementById("btnLaunchVerificationMailAdress");
            if (btnLaunchVerif)
                btnLaunchVerif.setAttribute("data-ro", "1");

            var dvMailVerifInfoText = document.getElementById("dvMailVerifInfoText");
            if (dvMailVerifInfoText) {
                dvMailVerifInfoText.style.display = '';
                dvMailVerifInfoText.innerHTML = res.MessageFromService;
            }
        }
    });
}