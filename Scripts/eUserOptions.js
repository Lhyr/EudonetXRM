//cette variable permet d'empêcher les appels au serveur dans le cas où il y a un appel serveur pour changer le mot de passe est encore en cours
var action = { pending: false, type: "" }
///Recalcule la taille du contenu de l'option Rapport d'exports
function resizeUserOptExportContent() {

    // Ajuste le contenu du menu admin au resize du window
    if (typeof (adjustDivContainer) == "function")
        adjustDivContainer();

    var versionchoice = document.getElementById('version-choice');
    if (versionchoice) //pour faire apparaitre la scrollbar si pas assez d'espace
        versionchoice.style.height = (getWindowSize().h - 500) + "px";
}

///Function de mise a jour des options utilisateur
/// Ex: new OptionUpdater({ "optiontype": 1, "option": 2, "optionvalue": 1 })
function OptionUpdater(options) {
    var oUpdater = new eUpdater("mgr/eUserOptionsManager.ashx", null);

    for (var key in options)
        oUpdater.addParam(key, options[key], "post");

    return {
        send: function (onSuccess, onError) {
            oUpdater.ErrorCallBack = function () {
                if (typeof (onError) == "function")
                    onError();
            }

            oUpdater.send(onSuccess);
        },
        addParam: function (key, value) {
            oUpdater.addParam(key, value, "post");
        }
    }
}

function setLng() {

    var oSelect = document.getElementById("lguser");
    if (!oSelect)
        return;

    var lng = oSelect.options[oSelect.selectedIndex].value;

    var optUpdater = new OptionUpdater({ "optiontype": 5, "option": 1, "optionvalue": lng });
    optUpdater.send(
        function (oDoc) {
            afterSet({ "oRes": oDoc, "bReload": true, "title": top._res_445 });
        });
}

function setFontSize() {

    var oSelect = document.getElementById("ftsize");
    if (!oSelect)
        return;

    var fontSize = oSelect.options[oSelect.selectedIndex].value;

    var optUpdater = new OptionUpdater({ "optiontype": 3, "option": 6, "optionvalue": fontSize });
    optUpdater.send(
        function (oDoc) {
            // US #3 105 - Tâches #6 597, #6 636 et #6 662 - On applique directement la nouvelle taille de police sans rafraîchissement ni confirmation, en mettant directement à jour eParamIFrame et en l'injectant dans le DOM
            getParamWindow().SetParam("fontsize", fontSize);
            eTools.UpdateDocCss(document, fontSize, true);
        });
}

//Met à jour le profil utilisateur de préférence
//Update user's profile preference
function setUserProfilePref() {
    //On récupère les informations nécessaires
    //We retrieve necessary informations
    var myDoc = document;
    if (typeof myMod != "undefined" && myMod & myMod.oConditionListModal) {
        myDoc = myMod.getIframe().document;
    }

    //On récupère l'utilisateur source
    //We get the source user
    var oValSrc = getAttributeValue(myDoc.getElementById('EDA_CPREF_SRC'), "ednvalue");

    //Si l'utilisateur source n'a pas de valeur, ou est égale à 0 alors on renvoit une erreur
    //If the source user doesn't have value or equals 0, we send an error
    if (typeof oValSrc !== "string" || oValSrc.length === 0 || oValSrc === parseInt(oValSrc, 10)) {
        eAlert(0, top._res_372, top._res_373.replace('<ITEM>', top._res_7373));
        return;
    }

    var conf = top.eConfirm(1, "", top._res_8944, "", null, null,
        function () {
            top.setWait(true);
            updateUserProfilePref(oValSrc, function () {
                
                var optUpdater = new OptionUpdater({ "optiontype": 5, "option": 8, "optionvalue": "" });
                optUpdater.send(
                    function (oDoc) {
                        setWait(false);
                        afterSet({ "oRes": oDoc, "bReload": true, "title": top._res_7908 });
                    });

                //top.eAlert(4, top._res_7908, top._res_1761);

            }, function () { });
        },
        null,
        false,
        true);
    
}

//Met à jour la signature 
function setSign() {

    //cfg, signature, 1
    var optUpdater = new OptionUpdater({ "optiontype": 1, "option": 2, "optionvalue": 1 });

    var checkbox = document.getElementById("auto-sign");
    if (!checkbox)
        return;

    var bAutoAddSign = getAttributeValue(checkbox, "chk");
    if (bAutoAddSign == 0 || bAutoAddSign == 1)
        optUpdater.addParam("autoaddsign", bAutoAddSign);

    var oMemoEditor = nsMain.getMemoEditor('edtBodySignMemoId');
    if (oMemoEditor)
        optUpdater.addParam("bodysign", oMemoEditor.getData());

    optUpdater.send(function (oDoc) {
        afterSet({ "oRes": oDoc, "bReload": false, "title": top._res_445 });
    });
}


function fontSizeInit() {

    var oeParam = top.getParamWindow();
    var objThm = JSON.parse(oeParam.GetParam('currenttheme').toString());
    var curtheme = objThm.Id;

    var myselect = document.getElementById("ftsize");
    var fontSizeWarning = document.getElementById("adminFontSizeWarning");
    if (myselect) {
        var themeMaxFont = eTools.GetMaxFontSize(curtheme);
        var disabledOptions = 0;

        [].forEach.call(myselect.options, function (opt) {

			// US #2 925 - Libellés pour chaque valeur, avec une taille correspondante à la police sélectionnée
			// Ne semble pas fonctionner en CSS avec option[value=X], d'où le style inline
			opt.style.fontSize = opt.value + 'pt';
			opt.title = opt.innerHTML + ' - ' + opt.value + 'pt';

            if (themeMaxFont <= 0) {
                opt.style.display = "";
                opt.disabled = false;
                fontSizeWarning.style.display = "none";
            }
            else if (themeMaxFont < (opt.value * 1)) {
                opt.style.display = "none";
                opt.disabled = true;
                opt.selected = false;
                fontSizeWarning.style.display = "";
                disabledOptions++;
            }

            // US #2 925 - S'il n'y a plus d'options de taille de police disponibles, on désactive la liste et on remplace le libellé sélectionné par "Standard"
            if (myselect.options.length - disabledOptions < 2 && myselect.selectedIndex < myselect.options.length) {
                myselect.disabled = true;
                myselect.title = GetText(fontSizeWarning); // Eudonet x propose une taille de police standard...
                myselect.options[myselect.selectedIndex].style.fontSize = "";
                myselect.options[myselect.selectedIndex].title = myselect.title; // Eudonet x propose une taille de police standard...
                myselect.options[myselect.selectedIndex].text = top._res_6256; // Standard
            }
        });


    }

}

function themeInit() {
    fontSizeInit();


}


//Met à jour de message utilisateur
function setMemo() {
    var oMemoEditor = nsMain.getMemoEditor('edtBodySignMemoId');
    if (oMemoEditor) {
        var newMessage = oMemoEditor.getData();
        var optUpdater = new OptionUpdater({ "optiontype": 5, "option": 3, "optionvalue": newMessage });
        optUpdater.send(function (oDoc) {
            afterSet({ "oRes": oDoc, "bReload": false });
            getParamWindow().RefreshUserMessage(newMessage);
        });
    }
}

// Met à jour le MruMode : activer les MRU
function setMruMode() {
    var oSelect = document.getElementById("ddlMruMode");
    if (!oSelect)
        return;

    var mruMode = oSelect.options[oSelect.selectedIndex].value;

    var optUpdater = new OptionUpdater({ "optiontype": 1, "option": 7, "optionvalue": mruMode });
    optUpdater.send(
        function (oDoc) {
            afterSet({ "oRes": oDoc, "bReload": true, "title": top._res_445 });
        });
}

//function appelée après mise à jour ou pas
function afterSet(args) {
    var success = (getXmlTextNode(args.oRes.getElementsByTagName("success")[0]) == "1");
    if (success) {
        // on utilise cette fonction pour reloader;
        eAlert(4, args.title, top._res_1761, '', args.width, args.height,
            function () {
                if (args.bReload)
                    top.window.location.reload(true);
            });
    }
    else {
        eAlert(0, args.title, top._res_72, getXmlTextNode(args.oRes.getElementsByTagName("message")[0]));
    }
}
function viewPassword(e) {
    var passwordInput = e;
    var passStatus = document.getElementById('pass-status' + "_" + e.id);
    if (passwordInput.type == 'password') {
        passwordInput.type = 'text';
        passStatus.className = 'icon-eye-blocked';
    }
    else {
        passwordInput.type = 'password';
        passStatus.className = 'icon-edn-eye';
    }
}



///summary
///Fonction valide le nouveau mot de passe
///summary
function onPwdValid(oMod) {

    if (action.pending && action.type === "PWDVALID")
        return;


    var pwdChangeDoc = null;

    if (typeof (oMod) != "undefined")
        pwdChangeDoc = oMod.getIframe().document;
    else
        pwdChangeDoc = document;

    var oldPass = null;
    if (pwdChangeDoc.getElementById("OldPassword") != null)
        oldPass = pwdChangeDoc.getElementById("OldPassword").value;

    var newPass = pwdChangeDoc.getElementById("NewPassword").value;
    var confirmNewPass = pwdChangeDoc.getElementById("ConfirmNewPassword").value;

    if (newPass != confirmNewPass) {
        //on affiche la fenêtre indiquant que les MDP ne correspondent pas
        eAlert(0, top._res_2, top._res_6726, top._res_202.replace(/\n/g, "<br />"), 410, 200);

        return;
    }

    //Appel Ajax
    url = "mgr/eLoginMgr.ashx";
    var ednu = new eUpdater(url, null);
    if (oldPass != null)
        ednu.addParam("txtOldPassword", oldPass, "post");
    ednu.addParam("txtNewPassword", newPass, "post");
    ednu.addParam("action", "changepassword", "get");

    if (oMod && oMod.isModalDialog && oMod.userid) {

        ednu.addParam("userid", oMod.userid, "post");
        var bChkNotExpire = getAttributeValue(pwdChangeDoc.getElementById("ChkNotExpire"), "chk");
        var bChkMustChangePwd = getAttributeValue(pwdChangeDoc.getElementById("ChkMustChangePwd"), "chk");

        ednu.addParam("noexpire", bChkNotExpire, "post");
        ednu.addParam("changepwd", bChkMustChangePwd, "post");

    }

    action.pending = true;
    action.type = "PWDVALID";

    var stopPending = (function (act) {
        return function () { act.pending = false }
    })(action)


    ednu.ErrorCallBack = function () { top.setWait(false); stopPending() };

    top.setWait(true);
    ednu.send(function (oRes) { top.setWait(false); stopPending(); pwdChangeTreatment(oRes, oMod) });

}

///summary
/// Lorsqu'on clique sur le bouton Annuler du module Mot de passe en admin : on revient à Mes Préférences
///summary
function onPwdCancel() {
    loadUserOption(USROPT_MODULE_PREFERENCES);
}

///summary
/// Traitement du retour serveur success/failure
///summary
function pwdChangeTreatment(oRes, oMod) {
    var strPwdChangedSuccess = getXmlTextNode(oRes.getElementsByTagName("result")[0]);
    var strUrlRedirection = getXmlTextNode(oRes.getElementsByTagName("url")[0]);
    var strTitle = getXmlTextNode(oRes.getElementsByTagName("title")[0]);
    var strMessage = getXmlTextNode(oRes.getElementsByTagName("msg")[0]);

    var okFct = null;
    var criticity = 0;
    if (strPwdChangedSuccess == "SUCCESS") {

        criticity = 4;
        okFct = function () {


            setWait(false);
            if (oMod && oMod.isModalDialog && oMod.noRedirect) {
                oMod.hide();
            }
            else {


                if (typeof (oMod) != "undefined")
                    oMod.hide();

                window.location = strUrlRedirection;


            }
        };
    }

    //on affiche la fenetre de confirmation
    eAlert(criticity, top._res_2, strTitle, strMessage, 410, 200, okFct);
}

///summary
/// Fonction activant le bouton Valider du module de changement de mot de passe si le formulaire est correctement rempli
///summary
function togglePwdValid() {
    var oConfirmPwdChangeChk = document.getElementById("ChkConfirmPwdChange");
    var oValidBtn = document.getElementById("btnPwdValid");

    if (oValidBtn) {
        if (oConfirmPwdChangeChk && oConfirmPwdChangeChk.getAttribute("chk") == "1")
            oValidBtn.style.visibility = "visible";
        else
            oValidBtn.style.visibility = "hidden";
    }
}

///summary
/// Fonction sauvegardant la version d office choisi
///summary
function setOfficeVer(src, officeId) {
    var container = document.getElementById("version-choice");
    var oldOfficeId = getAttributeValue(container, "curr-office");

    //On clique sur la version déjà en selection
    if (oldOfficeId == officeId)
        return;

    //on désactive l'encadré de l'ancienne version:
    var old = document.getElementById("version-" + oldOfficeId);
    removeClass(old, "actived");

    //on active la version choisie
    addClass(src, "actived");

    //on sauvegarde la nouvelle valeur
    setAttributeValue(container, "curr-office", officeId);
}

///summary
/// Fonction sauvegardant la version d office et le mode d'export dans la base choisi
///summary
function setExportProp() {
    var container = document.getElementById("version-choice");
    var officeId = getAttributeValue(container, "curr-office");

    var select = document.getElementById("export-mode");
    var value = select.selectedIndex;

    //cfg, 
    var optUpdater = new OptionUpdater({ "optiontype": 1, "option": 4, "optionvalue": 1 });

    optUpdater.addParam("officerelease", officeId);
    optUpdater.addParam("exportmode", value);

    optUpdater.send(function (oDoc) {
        afterSet({ "oRes": oDoc, "bReload": false, "title": top._res_6774 });
    });
}