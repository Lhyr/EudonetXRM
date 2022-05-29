/// <reference path="eTools.js" />

var modalPwd, modalPwdChange;
var modalUsr;
var modalConfForgotPwd;
var bRememberMe = false;
var strDbLastConnection = "";
var strUserLastConnection = "";

var browser = new getBrowser();

var V7URL = "";


///sumary
///Fonction de chargement de l'interface, opérant tous les tests de cookies, logins...
///sumary
///bSecondeCall : indique c'est le 2ème appel de onload (par exemple le SSO CAS doit passer une fois pour authentifier sans le process de remplissage des champs
///		puis une seconde pour n'effectuer que le process de préremplissage des champs sans la connexion CAS
function OnLoad(bSecondCall) {

    //Initialisation du SSO de type retour (2ème phase)
    eSSO_CASReturn.Init();
    //Si premier appel de onload et SSO CAS actif avec les bon paramètres on authentifie l'utilisateurs directement
    if (!bSecondCall && eSSO_CASReturn.SSOEnabled()) {
        //SSO CAS : authentification phase 2/2
        eSSO_CASReturn.AuthEudonetWithCASInfo();
        return;
    }



    var bAutSubscriberLaunched = false;

    if (document.getElementById("RememberMe").value == "1") {
        bRememberMe = true;

        if (document.getElementById("AutoConnect"))
            strDbLastConnection = document.getElementById("AutoConnect").value;

        if (strDbLastConnection != null && strDbLastConnection != "-1" && strDbLastConnection != "") {

            //JAS encryption des cookies coté serveur            
            strUserLastConnection = document.getElementById("txtUserLogin").value;

            switchClass(document.getElementById("txtLoginSubscriber"), "inputEdit", "inputFilled");
            switchClass(document.getElementById("txtPasswordSubscriber"), "inputEdit", "inputFilled");
            switchClass(document.getElementById("txtDatabase"), "inputEdit", "inputFilled");

            bAutSubscriberLaunched = true;
            authSubscriber(true);
        }
        else {
            bRememberMe = false;
            document.getElementById("txtUserLogin").value = "";
            //On protège car IE8 détect un bug si l'on donne le focus à un champ invisible ou en lecture seule.
            try {
                if (document.getElementById("txtLoginSubscriber").focus)
                    document.getElementById("txtLoginSubscriber").focus();
            }
            catch (e) {
            }

        }

    }
    else {

        document.getElementById("txtUserLogin").value = "";
        try {
            document.getElementById("txtLoginSubscriber").focus();
        }
        catch (exp) {
        }

    }


    if (!bAutSubscriberLaunched && document.getElementById("IsIntranet").value == "1") {
        authSubscriber(true);
    }

    selectLang();
    ManageLoginScenario();

    try {
        //startSnow();
    }
    catch (e) {
        // Pas de flocons, tant pis
    }

}

///sumary
///Fonction testant l'ensemble des composants de connexion de l'interface
///et affectant les bonnes classes css pour griser les composants s'ils ne sont pas saisis.
///L'objectif étant de "guider" l'utilisateur dans sa saisie. 
///sumary
function ManageLoginScenario() {

    var subLoginInput = document.getElementById("txtLoginSubscriber");
    var subPwdInput = document.getElementById("txtPasswordSubscriber");
    var dblist = document.getElementById("cboBase");
    var dbInput = document.getElementById("txtDatabase");
    var userList = document.getElementById("UserLoginList");
    var userInput = document.getElementById("txtUserLogin");
    var userPwdInput = document.getElementById("txtUserPassword");
    var aLoginItems = new Array(subLoginInput, subPwdInput, dblist, dbInput, userList, userInput, userPwdInput);
    var loginIdex = 0;
    var loginItem = null;
    for (loginIdex = 0; loginIdex < aLoginItems.length; loginIdex++) {
        loginItem = null;
        loginItem = aLoginItems[loginIdex];
        if (loginItem != null)
            if (loginItem.options) {
                if (loginItem.options.length > 0)
                    switchClass(loginItem, "inputEdit", "inputFilled");
                else
                    switchClass(loginItem, "inputFilled", "inputEdit");
            }
            else {
                if (loginItem.value != "")
                    switchClass(loginItem, "inputEdit", "inputFilled");
                else
                    switchClass(loginItem, "inputFilled", "inputEdit");
            }
    }
}

///summary
///fonction appelant la page d'initiation à la connexion via Saml2
///summary
function authSaml2() {
    var cboDb = document.getElementById("cboBase");
    if (cboDb == null || cboDb.length <= 0) {
        eAlert(2, top._res_5004, top._res_6276);
        return;
    }

    var strToken = document.getElementById("SubscriberToken").value;
    var oSelectDB = cboDb.options[cboDb.selectedIndex]
    var strDbToken = oSelectDB.value;

    var lang = '';
    var aParamLang = getUrlParam();
    if ('activeLang' in aParamLang)
        lang = "&lang=" + encode(aParamLang['activeLang']);

    var debug = '';
    if (('d' in aParamLang) && aParamLang['d'] == "1")
        debug = "&dl=0";

    var url = "mgr/saml/eLogin.ashx?h=" + screen.availHeight + "&w=" + screen.availWidth + (lang) + (debug) + "&dbt=" + encode(strDbToken) + "&st=" + encode(strToken);
    window.location = url;
}

///summary
///fonction appelant le manager de connexion pour authentifier l'utilisateur
///<param name="bSilent">Booléen déterminant si l'erreur est affichée à l'utilisateur </param>
///summary
function authUser(bSilent) {
    var cboDb = document.getElementById("cboBase");
    if (cboDb.length <= 0) {
        eAlert(2, top._res_5004, top._res_6276);
        return;
    }

    var oSelectDB = cboDb.options[cboDb.selectedIndex]

    //Base v7 seulement pour les navigateur compatible

    if (oSelectDB.attributes["v7"].value == "1" && !browser.isV7Compatible) {

        //Message niveau "MSG_INFOS"   
        eAlert(3, GetAppRes(1264), GetAppRes(6693));

        return;
    }


    var strToken = oSelectDB.value;
    var strDbToken = document.getElementById("SubscriberToken").value;
    //SSO CAS : authentification phase 1/2 si active
    if (eSSO_CAS.SSOEnabled()) {
        eSSO_CAS.AuthCAS(strDbToken, strToken);
        return;
    }
    //fonctionnement habituelle
    var chkRememberMe = document.getElementById("chkRememberMe");
    chkRememberMe.focus();

    var strUserLogin = document.getElementById("txtUserLogin").value;
    var strUserPassword = document.getElementById("txtUserPassword").value;

    var strSubscriberLogin = document.getElementById("txtLoginSubscriber").value;
    var strSubscriberPassword = document.getElementById("txtPasswordSubscriber").value;



    var strUserListEnabled = oSelectDB.attributes["LogUserListEnabled"].value;

    if (strUserLogin == "") {
        if (!bSilent)
            eAlert(2, GetAppRes(5004), GetAppRes(5));

        return;
    }

    setWait(true);

    var url = "mgr/eLoginMgr.ashx";

    //Appel Ajax
    var ednu = new eUpdater(url, null);

    ednu.addParam("action", "authuser", "get");
    ednu.addParam("UserLogin", strUserLogin, "post");
    ednu.addParam("UserPassword", strUserPassword, "post");
    ednu.addParam("dbt", strToken, "post");
    ednu.addParam("rememberme", document.getElementById("RememberMe").value, "post");
    ednu.addParam("st", strDbToken, "post");
    ednu.addParam("LogUserListEnabled", strUserListEnabled, "post");
    ednu.addParam("Height", screen.availHeight, "post");
    ednu.addParam("Width", screen.availWidth, "post");

    var aParamLang = getUrlParam("activeLang");

    if (aParamLang && aParamLang.constructor("activeLang")) {
        ednu.addParam("forceactivelang", "1", "post");
    }


    ednu.ErrorCallBack = function () { setWait(false); };
    ednu.send(loginUserTreatment, bSilent);

}

function getUrlParam() {

    var urlParams;


    var match,
        pl = /\+/g,  // Regex for replacing addition symbol with a space
        search = /([^&=]+)=?([^&]*)/g,
        decode = function (s) { return decodeURIComponent(s.replace(pl, " ")); },
        query = window.location.search.substring(1);

    urlParams = {};
    while (match = search.exec(query))
        urlParams[decode(match[1])] = decode(match[2]);


    return urlParams;
}


function authUserToken(db, user, stL, stP) {

    var strToken = document.getElementById("DBTokenFR").value;
    var strDbToken = document.getElementById("SubscriberTokenFR").value;
    var strUserToken = document.getElementById("UserTokenFR").value;
    var userlogin = document.getElementById("UserLoginList");
    if (typeof (document.getElementById("txtUserPassword")) != "undefined")
        document.getElementById("txtUserPassword").value = "";

    if (typeof (document.getElementById("txtLoginSubscriber")) != "undefined")
        document.getElementById("txtLoginSubscriber").value = stL;

    if (typeof (document.getElementById("txtPasswordSubscriber")) != "undefined")
        document.getElementById("txtPasswordSubscriber").value = stP;

    if (typeof (document.getElementById("chkRememberMe")) != "undefined") {
        var rmm = document.getElementById("chkRememberMe");
        rmm.setAttribute("chk", 1);
    }


    var option = document.createElement("option");
    option.text = user;
    option.selected = true;
    userlogin.add(option);

    var cboDb = document.getElementById("cboBase");
    var option = document.createElement("option");
    option.text = db;
    option.selected = true;
    option.setAttribute("LogUserListEnabled", "1");
    cboDb.add(option);
    bRememberMe = true;
    activateUserList();

    var url = "mgr/eLoginMgr.ashx";

    var ednu = new eUpdater(url, null);

    ednu.addParam("action", "authuser", "get");

    ednu.addParam("ut", strUserToken, "post");
    ednu.addParam("dbt", strToken, "post");
    ednu.addParam("rememberme", document.getElementById("RememberMe").value, "post");
    ednu.addParam("st", strDbToken, "post");
    ednu.addParam("LogUserListEnabled", 0, "post");
    ednu.addParam("Height", screen.availHeight, "post");
    ednu.addParam("Width", screen.availWidth, "post");



    ednu.ErrorCallBack = function () { setWait(false); };
    ednu.send(loginUserTreatment, false);

}

///summary
///fonction appelant le manager de connexion pour authentifier l'abonné
///<param name="bSilent">Booléen déterminant si l'erreur est affichée à l'utilisateur </param>
///summary
function authSubscriber(bSilent) {
    //HDJ Vider le message d'erreur en cas de presence!
    document.getElementById("errUser").style.display = "none";
    removeClass(document.getElementById("txtUserPassword"), "errorPwd");
    removeClass(document.getElementById("UserLoginList"), "errorPwd");
    removeClass(document.getElementById("txtUserLogin"), "errorPwd");

    //Vidage de la liste des bases
    document.getElementById("cboBase").options.length = 0;

    var strSubscriberLogin = document.getElementById("txtLoginSubscriber").value;
    var strSubscriberPassword = document.getElementById("txtPasswordSubscriber").value;

    if (strSubscriberLogin == "" || strSubscriberPassword == "") {
        return;
    }



    document.getElementById("cboBase").style.display = "none";
    setWait(true)

    //Appel Ajax
    url = "mgr/eLoginMgr.ashx";
    var ednu = new eUpdater(url, null);
    ednu.addParam("SubscriberLogin", strSubscriberLogin, "post");
    ednu.addParam("SubscriberPassword", strSubscriberPassword, "post");
    ednu.addParam("rememberme", document.getElementById("RememberMe").value, "post");
    ednu.addParam("action", "authsubscriber", "get");
    ednu.ErrorCallBack = function () { setWait(false); };
    ednu.send(loginSubscriberTreatment, bSilent);
}

///summary
///fonction affichant le div avec la roue crantée utilisée pour les delai d'attente
///<param name="bOn">Booléen déterminant si l'le divWait est affiché ou non </param>
///summary
function setWait(bOn) {
    if (bOn)
        document.getElementById("ImgLoading").style.display = "block";
    else
        document.getElementById("ImgLoading").style.display = "none";
}

///summary
///fonction de callback de l'authentification abonné
///<param name="oRes">document XML de résultat de l'appel AJAX</param>
///<param name="bSilent">Booléen déterminant si l'erreur est affichée à l'utilisateur </param>
///summary
function loginSubscriberTreatment(oRes, bSilent) {

    document.getElementById("cboBase").options.length = 0;

    var strLoginSuccess = getXmlTextNode(oRes, "result");
    var subscriberToken = getXmlTextNode(oRes, "subscribertoken");
    var bApplicationSSO = getXmlTextNode(oRes, "applicationsso") == "1";


    document.getElementById("SubscriberToken").value = subscriberToken;



    if (strLoginSuccess != "SUCCESS") {
        if (!bSilent) {



            var strErrDesc = getXmlTextNode(oRes, "errordescription");
            if (strErrDesc == null || strErrDesc == "") {
                strErrDesc = top._res_6210;
            }
            document.getElementById("txtLogingSubscriberErr").innerHTML = strErrDesc;
            document.getElementById("errSub").style.display = "block";
            addClass(document.getElementById("txtLoginSubscriber"), "errorPwd");
            addClass(document.getElementById("txtPasswordSubscriber"), "errorPwd");


            document.getElementById("txtPasswordSubscriber").value = "";
            document.getElementById("txtUserLogin").value = "";

            document.getElementById("txtUserPassword").value = "";
            var errCode = getXmlTextNode(oRes, "errcode");
            if (errCode == "NBR_MAX_CONN")
                setWindowEnabled();
        }

        //Vidage de la liste des utilisateurs
        var cboUsr = document.getElementById("UserLoginList");
        if (cboUsr != null) {
            while (cboUsr.options.length > 0) {
                cboUsr.remove(0);
            }
        }
        document.getElementById("cboBase").style.display = "block";
        setWait(false);
        document.getElementById("txtDatabase").style.display = "none";
        ManageLoginScenario();
        return;
    }

    removeClass(document.getElementById("txtLoginSubscriber"), "errorPwd");
    removeClass(document.getElementById("txtPasswordSubscriber"), "errorPwd");
    document.getElementById("errSub").style.display = "none";

    V7URL = getXmlTextNode(oRes.getElementsByTagName("V7URL")[0]);

    var oDbList = oRes.getElementsByTagName("db");

    if (oDbList.length > 0) {
        //JAS switchClass(oDbList, "inputEdit", "inputFilled");
        for (var idb = 0; idb < oDbList.length; idb++) {
            var oDb = oDbList[idb];

            var strDbInfos = getXmlTextNode(oDb.childNodes[0]);
            var strDbLongName = getXmlTextNode(oDb.childNodes[1]);
            var strUserListEnabled = getXmlTextNode(oDb.childNodes[2]);
            var bSSOEnabled = (getXmlTextNode(oDb.childNodes[4]) == "1");
            var bSSOSASEnabled = getXmlTextNode(oDb, "SSOSASEnabled") == "1";   //SSO en SAS actif
            var AuthnMode = getXmlTextNode(oDb, "AuthnMode");

            var bV7 = getXmlTextNode(oDb, "v7") == "1";
            var bIsSel = getXmlTextNode(oDb, "isselected") == "1";

            var cboDb = document.getElementById("cboBase");


            //Création de l'élément sur le DOM      
            var dbOptNew = document.createElement("option");

            // Libelle/Value
            dbOptNew.text = strDbLongName;
            dbOptNew.value = strDbInfos;



            dbOptNew.setAttribute("LogUserListEnabled", strUserListEnabled);
            dbOptNew.setAttribute("SSOEnabled", (bSSOEnabled && bApplicationSSO ? "1" : "0"));
            //Ajout du paramètre dans la liste des bases de données pour indiquer que le SSO en mode SAS est actif
            dbOptNew.setAttribute("SSOSASEnabled", (!bSSOEnabled && bSSOSASEnabled ? "1" : "0"));
            dbOptNew.setAttribute("AuthnMode", AuthnMode);
            dbOptNew.setAttribute("v7", bV7 ? "1" : "0");

            if (bIsSel) {
                dbOptNew.setAttribute("selected", "1");
                var authnBlock = document.getElementById("globalAuthnBlock");
                if (authnBlock)
                    setAttributeValue(authnBlock, "mode", AuthnMode);
            }

            if (bV7) {
                if (!browser.isV7Compatible) {
                    //    dbOptNew.disabled = true;
                    dbOptNew.className += " disabled";
                }
            }

            try {

                cboDb.add(dbOptNew, cboDb.options.length);
            }
            catch (ex) {
                cboDb.add(dbOptNew, null);
            }

        }


        if (strDbLastConnection == "1" && bRememberMe)
            setDb(strDbLastConnection);
    }
    else {
        eAlert(2, top._res_5004, top._res_6276);
    }

    document.getElementById("cboBase").style.display = "block";
    document.getElementById("cboBase").focus();
    setWait(false);
    //document.getElementById("txtUserLogin").focus();
    //Si SSO et base unique alors connexion de l'utilisateur
    if (bApplicationSSO && document.getElementById("cboBase").length == 1) {
        if (document.getElementById("cboBase").options[0].attributes["SSOEnabled"].value == "1") {
            // HLA - On simule la saisie du userlogin dans le cas d'un sso pour continuer le process de connexion malgrès l'absence d'userlogin - Appui Tech Dev #42610
            document.getElementById("txtUserLogin").value = "sso";
            authUser(false);
        }
    }

    if (bRememberMe) {

        document.getElementById("txtUserLogin").style.display = "block";
        document.getElementById("UserLoginList").style.display = "none";
        document.getElementById("txtDatabase").style.display = "block";
        document.getElementById("txtDatabase").style.visibility = "";
        document.getElementById("cboBase").style.display = "none";
        document.getElementById("txtUserPassword").focus();
    }
    else {
        onChangeDbList(false);
    }

    ManageLoginScenario();
}

///summary
///fonction Activant la liste Déroulante des bases
///summary
function activateDbList() {
    if (bRememberMe) {
        document.getElementById("txtDatabase").style.display = "none";
        document.getElementById("txtDatabase").style.visibility = "hidden";
        document.getElementById("cboBase").style.display = "block";
        document.getElementById("cboBase").focus();
    }
}

///summary
///fonction Activant la liste Déroulante des utilisateurs
///summary
function activateUserList() {

    var cboDb = document.getElementById("cboBase");
    if (cboDb.selectedIndex < 0)
        return;
    var strUserListEnabled = cboDb.options[cboDb.selectedIndex].attributes["LogUserListEnabled"].value;

    if (bRememberMe && strUserListEnabled == "1") {
        document.getElementById("txtUserLogin").style.display = "none";
        document.getElementById("UserLoginList").style.display = "block";
    }

}

///summary
///fonction de callBack de l'authentification utilisateur lorsque l'authentification est réussie
///summary
function onPwdOk(strUrlRedirection) {
    modalPwd.hide();
    modalPwdChange = new eModalDialog(top._res_6725, 0, strUrlRedirection, 500, 180);
    modalPwdChange.addParam("context", "LOGIN_EXPIRED", "post");
    // Masquer le bouton "Agrandir"
    modalPwdChange.hideMaximizeButton = true;
    // Masquer le bouton "Fermer"
    modalPwdChange.hideCloseButton = true;
    modalPwdChange.show();
    modalPwdChange.addButton(top._res_28, function () { onPwdValid(modalPwdChange) }, "button-green", strUrlRedirection);
    modalPwdChange.addButton(top._res_29, onPwdAbort, "button-gray", null, true);
}

///summary
///Fonction de callBack de l'authentification utilisateur lorsque l'authentification à échoué
///summary
function onPwdAbort() {
    window.location = "eLogin.aspx";
}

///summary
///Fonction de callback de l'authentification Utilisateur
///summary
function loginUserTreatment(oRes, bSilent) {


    var strLoginSuccess = getXmlTextNode(oRes.getElementsByTagName("result")[0]);
    var strUrlRedirection = getXmlTextNode(oRes.getElementsByTagName("url")[0]);

    if (strLoginSuccess == "REDIRECT") {


        window.location = strUrlRedirection;
        return;


    }
    else if (strLoginSuccess != "SUCCESS") {
        setWait(false);
        //Si l'on vient de l'authentification SSO : les informations des cookies n'ont pas été préchargée donc il faut les précharger en cas d'erreur avant d'afficher l'erreur
        if (eSSO_CASReturn.SSOEnabled()) {
            OnLoad(true);
        }
        var strErrDesc = getXmlTextNode(oRes.getElementsByTagName("errordescription")[0]);
        if (!bSilent) {

            document.getElementById("errUser").style.display = "";
            if (strErrDesc == null || strErrDesc == "") {
                strErrDesc = top._res_5;
            }
            document.getElementById("labelUserEr").innerHTML = strErrDesc;
            document.getElementById("txtUserPassword").value = "";


            addClass(document.getElementById("txtUserPassword"), "errorPwd");
            addClass(document.getElementById("UserLoginList"), "errorPwd");
            addClass(document.getElementById("txtUserLogin"), "errorPwd");

            var errCode = getXmlTextNode(oRes.getElementsByTagName("errcode")[0]);
            if (errCode == "NBR_MAX_CONN")
                setWindowEnabled();

        }


        ManageLoginScenario();
        return;
    }


    ManageLoginScenario();
    document.getElementById("errUser").style.display = "none";
    removeClass(document.getElementById("txtUserPassword"), "errorPwd");
    removeClass(document.getElementById("UserLoginList"), "errorPwd");
    removeClass(document.getElementById("txtUserLogin"), "errorPwd");

    /* Récupération de l'url de redirection
    Dans eMain.aspx ainsi que le reste des pages de l'appli
    on vérifie à chaque fois la session de l'utilisateur en cours
    si jamais la page eMain.aspx est appelée directement */


    if (strUrlRedirection.toLowerCase() == "euserpassword.aspx") {
        setWait(false);
        modalPwd = new eModalDialog(top._res_2, 1, null, 500, 180);
        modalPwd.setMessage(top._res_6238, "", 1);
        // Masquer le bouton "Agrandir"
        modalPwd.hideMaximizeButton = true;
        // Masquer le bouton "Fermer"
        modalPwd.hideCloseButton = true;
        //TOCHECK: FromLogin passé à 1, puis commenté à cet endroit. Non repris

        modalPwd.show();
        modalPwd.addButton(top._res_28, onPwdOk, "button-green", strUrlRedirection);
        modalPwd.addButton(top._res_29, onPwdAbort, "button-gray", null, null, true);

    }
    else {
        window.location = strUrlRedirection;
    }

}

function onUsrOk(strRes) {
    modalUsr.hide();
}


function viewPassword(e) {
    if (e.id == "NewPassword" || e.id == "ConfirmNewPassword") {
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
    } else {

        var ie = window.event;
        var firedField = (!ie) ? e.target : event.srcElement
        var value = document.getElementById("txtUserPassword").value;
        var passwordInput = document.getElementById('txtUserPassword');
        var passStatus = document.getElementById('pass-status');


        if (passwordInput.type == 'password') {
            passwordInput.type = "text";
            passStatus.className = 'icon-eye-blocked';

            document.getElementById("txtUserPassword").style.display = "none";
            document.getElementById("txtUserPasswordBlock").value = value;
            document.getElementById("txtUserPasswordBlock").style.display = "block";
        }
        else {
            passwordInput.type = 'password';
            passStatus.className = 'icon-edn-eye';
            document.getElementById("txtUserPassword").style.display = "block";
            document.getElementById("txtUserPasswordBlock").style.display = "none";
        }
    }
}

///summary
///Permet d'affecter le mot de passe à la rubrique type mot de passe
///<param name="e">Evenement déclencheur (normalement KeyPress)</param>
///summary
function setPassWord(e) {
    var ie = window.event;
    var firedField = (!ie) ? e.target : event.srcElement;
    if (firedField.id == "txtUserPasswordBlock")
        document.getElementById("txtUserPassword").value = firedField.value;

}

///summary
///Fonction assurant la gestion de la saisie dans les champs texte de l'interface de connexion
///<param name="e">Evenement déclencheur (normalement KeyPress)</param>
///summary
function KeyPress(e) {


    var key;
    var ie = window.event;
    var targetObjet = e.target;
    var firedField = (!ie) ? e.target : event.srcElement

    if (ie) {
        e = window.event;
        key = e.keyCode
    } else {
        if (e.which == 0 && e.keyCode != 0) {
            key = e.keyCode;
        } else {
            key = e.which;
        }
    }

    if (key == 13) {
        if (ie) {
            e.returnValue = false;
        } else {
            e.preventDefault();
        }

        if (firedField.id == "txtUserPassword" || firedField.id == "txtUserPasswordBlock" || firedField.id == "txtUserLogin") {
            authUser(false);

        }
        if (firedField.id == "txtPasswordSubscriber") {
            authSubscriber(false);
        }
        if (firedField.id == "txtLoginSubscriber") {
            document.getElementById("txtPasswordSubscriber").focus();
        }
    }
}

///summary
///Fonction sélectionnant la base de donnée de la liste déroulante correspondant à la valeur saisie en paramètre.
///<param name="value">nom de la base de données</param>
///summary
function setDb(value) {
    /* Le selected de la combo box est activé quand celui-ci est créé
    var cboDb = document.getElementById("cboBase");
    for (var idb = 0; idb < cboDb.options.length; idb++) {
        if (cboDb.options[idb].text.toLowerCase() == value.toLowerCase()) {
            cboDb.options[idb].selected = true;
            break;
        }
    }*/
    onChangeDbList(false);
}

function setLang(langImg) {
    var strLang = langImg.id.replace("DIV_", "");
    window.location = "eLogin.aspx?activeLang=" + strLang;
}

function selectLang() {
    var lang = document.getElementById("langue").value;
    var oDiv = document.getElementById("DIV_" + lang.toUpperCase());
    if (oDiv)
        oDiv.className = "bt_flag_active";
    else {
        oDiv = document.getElementById("DIV_LANG_06");
        oDiv.className = "bt_flag_active";
    }
}

function onChangeDbList(fromClick) {
    if (fromClick) {
        bRememberMe = false;
    }

    var cboDb = document.getElementById("cboBase");
    if (cboDb.selectedIndex < 0)
        return;

    if (bRememberMe && fromClick) {
        document.getElementById("txtDatabase").style.display = "none";
    }
    var itmDb = cboDb.options[cboDb.selectedIndex];

    document.getElementById("txtDatabase").value = itmDb.text;

    var bIsV7 = getAttributeValue(itmDb, "v7") == "1";

    if (bIsV7) {
        var errlabel = document.getElementById("labelErrGlobal");
        errlabel.innerHTML = top._res_2398;
        var eltLink = errlabel.querySelector("#RedirectV7Link");

        if (eltLink) {
            if (!browser.isV7Compatible) {
                eltLink.addEventListener("click", function () { eAlert(3, GetAppRes(1264), GetAppRes(6693)); });
                eltLink.setAttribute("href", "#");

            }
            else {
                eltLink.setAttribute("href", V7URL);
            }
        }

        document.getElementById("errGlobal").style.display = "";
        document.getElementById("txtLogingSubscriberErr").innerHTML = "";
        document.getElementById("userAuthnBlock").style.display = "none";
        //eAlert(3, "", top._res_2398.replace("##V7URL##", V7URL));
        return;
    }
    else {
        document.getElementById("errGlobal").style.display = "";
        document.getElementById("labelErrGlobal").innerHTML = "";
        document.getElementById("txtLogingSubscriberErr").innerHTML = "";
        document.getElementById("userAuthnBlock").style.display = "";
    }

    ManageLoginScenario();

    //Mode SSO pour la BDD vérifier si la base de donnée est configurée pour du SSO : 
    // Si oui ne pas afficher la liste des utilisateurs même si loguserlistenabled est activé
    // Si non garder le fonctionnement sans SSO

    //vidage de la liste des utilisateurs
    var cboUsr = document.getElementById("UserLoginList");
    if (cboUsr != null) {
        while (cboUsr.options.length > 0) {
            cboUsr.remove(0);
        }
    }

    /****************************************************************/
    /* réinitialise les champs (msg d'erreur,...)                   */
    /****************************************************************/
    document.getElementById("UserLoginList").style.display = "none";
    document.getElementById("txtUserLogin").style.display = "block";

    removeClass(document.getElementById("txtUserLogin"), "errBase");
    removeClass(document.getElementById("txtUserPassword"), "errBase");

    // On réactive les champs login/MDP utilisateur
    document.getElementById("txtUserLogin").removeAttribute("disabled");
    document.getElementById("txtUserPassword").removeAttribute("disabled");

    // On efface l'éventuel message d'erreur précédent si une base non-XRM avait été sélectionnée
    document.getElementById("txtLogingSubscriberErr").innerHTML = "";
    document.getElementById("errSub").style.display = "none";


    /****************************************************************/


    var subscriberToken = document.getElementById("SubscriberToken").value;

    if (subscriberToken == "")
        return;
    //Préremplissage des utilisateurs
    //Si SSO SAS Inactif : process habituelle de connexion à EUDO (affichage de user/mdp ou liste des utilisateur)
    //			Si Actif : process de SSO CAS (pas de saisi de login/MDP)
    var bSsoSas = itmDb.getAttribute("SSOSASEnabled") == "1";   //Si le SSO de type SAS est actif on vérifit qu'il est correctement paramétré sur la base paramétrée
    var bSso = itmDb.getAttribute("SSOEnabled") == "1";
    if (!bSsoSas) {
        //-------------
        //Pour parer au bug qui peut se produire si'lon sélectionne une base SSO puis une autre non SSO on force le réaffichage des infos utilisateurs
        //Bug #39246 : Seulement si pas de SSO même natif sur la base sélectionnée.
        if (!bSso)
            ShowUserLogin();



        // mode de cxn
        manageCxnMode(itmDb);

        //-------------
        loadUserList(subscriberToken, itmDb, fromClick);
    }
    else {
        checkSSOSAS(subscriberToken, itmDb, fromClick); //vérifie que le SSO est correctement paramétré sur la base paramétrée est récupère les paramètres propre au SSO
    }


}

function manageCxnMode(itmDb) {
    var aParamLang = getUrlParam();
    var mode;
    if ('m' in aParamLang && aParamLang['m'] == "form")
        mode = aParamLang['m'];
    else
        mode = getAttributeValue(itmDb, "AuthnMode");

    var authnBlock = document.getElementById("globalAuthnBlock");
    if (authnBlock)
        setAttributeValue(authnBlock, "mode", mode);
}

//SSO SAS : vérifie que le SSO est correctement paramétré sur la base paramétrée est récupère les paramètres propre au SSO
function checkSSOSAS(subscriberToken, itmDb, fromClick) {

    setWait(true);
    var strDbToken = itmDb.value;

    var url = "mgr/eLoginMgr.ashx";
    var ednu = new eUpdater(url, null);

    ednu.addParam("rememberme", document.getElementById("RememberMe").value, "post");
    ednu.addParam("st", subscriberToken, "post");
    ednu.addParam("db", strDbToken, "post");
    ednu.addParam("action", "SSOSASPARAM", "get");
    //ednu.addParam("RedirectURL", redirectURL, "post");
    ednu.ErrorCallBack = function () { setWait(false); };
    ednu.send(checkSSOSASTreatment, subscriberToken, itmDb, fromClick);
}

//SSO SAS : si le SSO SAS est correctement paramétré on affiche un rendu propre au SSO SAS
function checkSSOSASTreatment(oRes, subscriberToken, itmDb, fromClick) {
    var strListSuccess = getXmlTextNode(oRes.getElementsByTagName("result")[0]);

    // Liste des utilisateurs non récupérée = AppUrl invalide (URL v7, serveur injoignable)
    if (strListSuccess != "SUCCESS") {
        ShowErrLogin(getXmlTextNode(oRes.getElementsByTagName("errordescription")[0]));
        setWait(false);
        return;
    }
    eSSO_CAS.Init(
        getXmlTextNode(oRes.getElementsByTagName("CAS")[0]) == "1"
        , getXmlTextNode(oRes.getElementsByTagName("CAS_URL")[0])
        , getXmlTextNode(oRes.getElementsByTagName("XRM_RETURNURL")[0])
    );
    HideErrLogin();

    setWait(false);
    if (eSSO_CAS.SSOEnabled()) {
        //SSO paramétré.
        //Ne pas afficher les login et mot de passes
        ShowUserLogin(true);
    }
    else {
        ShowUserLogin();
        //SSO non paramétré
        loadUserList(subscriberToken, itmDb, fromClick);
    }

}
//Affiche ou cache les champs userlogin/MDP
function ShowUserLogin(bHide) {
    document.getElementById("UserLoginList").parentElement.style.display = bHide ? "none" : "block";
    document.getElementById("txtUserLogin").parentElement.style.display = bHide ? "none" : "block";
    document.getElementById("txtUserPassword").parentElement.style.display = bHide ? "none" : "block";
}


//SSO CAS 1ère phase :
//	Objet de récupération des paramètres du SSO en mode CAS
var eSSO_CAS = (function () {
    "use strict";
    //Indique que le SSO CAS est actif
    var _bSSOenabled = false;
    //Retourne l'url de login au SSO
    var _sSSOurl = "";
    //url XRM ou le SSO doit nous rediriger
    var _sXRM_ReturnUrl = "";
    //Initialisation de l'objet à l'aide des paramètres saisie :
    // bSSOenabled : SSO en mode SAS activé
    // sSSOurl : URL du SSO CAS
    // sXRM_ReturnUrl :  url XRM ou le SSO doit nous rediriger
    var init = function (bSSOenabled, sSSOurl, sXRM_ReturnUrl) {
        _bSSOenabled = bSSOenabled;
        _sSSOurl = sSSOurl;
        _sXRM_ReturnUrl = sXRM_ReturnUrl;
        if (!_sSSOurl || _sSSOurl.trim().length <= 0)
            _bSSOenabled = false;
    };

    //Effectue l'action d'authentification vers le SSO de type CAS	
    // strDbToken : token d'abonné
    // strToken : token de la base de donnée
    var authCAS = function (strDbToken, strToken) {
        //url XRM ou le SSO doit nous rediriger
        var strReturnUrl = eSSO_CAS.XRM_ReturnUrl() + "?st=" + encodeURIComponent(strDbToken) + "&dbt=" + encodeURIComponent(strToken) + "&rm=" + document.getElementById("RememberMe").value;

        var strUrl = eSSO_CAS.SSOurl() + "?service=" + encodeURIComponent(strReturnUrl);

        window.location = strUrl;
    }

    return {
        //Indique que le SSO CAS est actif
        SSOEnabled: function () { return _bSSOenabled; },
        //Retourne l'url de login au SSO
        SSOurl: function () { return _sSSOurl; },
        //Retourne l'url XRM ou le SSO doit nous rediriger
        XRM_ReturnUrl: function () { return _sXRM_ReturnUrl; },
        //Initialisation de l'objet à l'aide des paramètres saisie :
        // bSSOenabled : SSO en mode SAS activé
        // sSSOurl : URL du SSO CAS
        // sXRM_ReturnUrl :  url XRM ou le SSO doit nous rediriger
        Init: init,
        //Effectue l'action d'authentification vers le SSO de type CAS	
        // strDbToken : token d'abonné
        // strToken : token de la base de donnée
        AuthCAS: authCAS
    };
})();
var eSSO_CASReturn = (function () {
    "use strict";
    //Indique que le SSO CAS est actif
    var _bSSOenabled = false;
    //ticket d'authentification retourné par le SSO (attention il ne peut être validé qu'une fois)
    var _sTicket = "";
    //token d'abonné
    var _strDbToken = "";
    //token de la base de donnée
    var _strToken = "";
    //Indique si l'utilisateur souhaite mémoriser ses identifiants 
    var _bRememberMe = false;
    //Récupère les informations retournées en query string par le CAS
    var init = function () {
        _sTicket = URL_getParameterByName("ticket");
        _strDbToken = decodeURIComponent(URL_getParameterByName("st"));
        _strToken = decodeURIComponent(URL_getParameterByName("dbt"));
        _bRememberMe = URL_getParameterByName("rm") == "1";

        if (_sTicket.length > 0 && _strDbToken.length > 0 && _strToken.length > 0)
            _bSSOenabled = true;
    };
    //Fonction permettant l'identification eudonet au travers des informations retournées par la page de connexion CAS.
    var authEudonetWithCASInfo = function () {
        setWait(true);
        var url = "mgr/eLoginMgr.ashx";

        //Appel Ajax
        var ednu = new eUpdater(url, null);

        ednu.addParam("action", "authcas", "get");

        ednu.addParam("dbt", _strToken, "post");
        ednu.addParam("rememberme", _bRememberMe ? "1" : "0", "post");
        ednu.addParam("st", _strDbToken, "post");
        ednu.addParam("casticket", _sTicket, "post");
        ednu.addParam("Height", screen.availHeight, "post");
        ednu.addParam("Width", screen.availWidth, "post");



        ednu.ErrorCallBack = function () { setWait(false); };
        ednu.send(loginUserTreatment, false);


    };

    return {
        //Indique que le SSO CAS est actif
        SSOEnabled: function () { return _bSSOenabled; },
        //Récupère les informations retournées en query string par le CAS
        Init: init,
        //Fonction permettant l'identification eudonet au travers des informations retournées par la page de connexion CAS.
        AuthEudonetWithCASInfo: authEudonetWithCASInfo
    };
})();


//Permet l'affichage d'une erreur de connexion utilisateur
//sError : erreur à afficher
function ShowErrLogin(sError) {
    // On affiche un message d'erreur et on empêche l'accès au login/MDP utilisateur
    document.getElementById("txtLogingSubscriberErr").innerHTML = sError;
    document.getElementById("errSub").style.display = "block";
    document.getElementById("txtUserLogin").setAttribute("disabled", "disabled");
    document.getElementById("txtUserPassword").setAttribute("disabled", "disabled");
    document.getElementById("txtUserLogin").style.display = "block";
    document.getElementById("UserLoginList").style.display = "none";

}
//Cache l'erreur de conenxion utilisateur
function HideErrLogin() {
    // On ajoute un attribut sur l'option correspondant à cette base pour indiquer qu'elle pointe sur une URL XRM valide
    if (typeof (cboDb) != "undefined" && cboDb) {
        cboDb.options[cboDb.selectedIndex].setAttribute("xrm", "0");
        removeClass(cboDb, "errBase");
    }
    HideErr();
}


//Cache l'erreur de conenxion utilisateur
function HideErr() {

    // On rétablit l'affichage des contrôles (TODO: à vérifier dans ManageLoginScenario)
    removeClass(document.getElementById("txtDatabase"), "errBase");
    document.getElementById("txtLogingSubscriberErr").innerHTML = "";
    document.getElementById("errSub").style.display = "none";
    document.getElementById("txtUserLogin").removeAttribute("disabled");
    document.getElementById("txtUserPassword").removeAttribute("disabled");

}



//Charge les informations de la liste des utilisateurs si necessaire
function loadUserList(subscriberToken, itmDb, fromClick) {
    var strDbToken = itmDb.value;
    var strUserListEnabled = itmDb.attributes["LogUserListEnabled"].value;

    if (strUserListEnabled != "1") {
        setWait(false);
        if (fromClick)
            document.getElementById("txtUserLogin").focus();
        return;
    }


    document.getElementById("txtUserLogin").style.display = "none";


    if (fromClick) {
        document.getElementById("UserLoginList").style.display = "block";
        document.getElementById("UserLoginList").focus();
    }

    setWait(true);

    /*
    MAB - 2013-08-22 - Demande #24 613
    Comme on appelle ici une URL qui peut ne pas se trouver sur le même serveur, on ne doit pas effectuer directement d'appel AJAX
    au risque de se retrouver avec une erreur XMLHTTPRequest relative à la sécurité (appels cross-domain) si le fichier eLoginMgr.ashx
    n'existe pas (ce qui est le cas si l'AppUrl de la base pointe sur Eudonet v7) : "Origin is not allowed by Access-Control-Allow-Origin"
    Or, lorsque cette erreur cross-domain survient, l'objet XMLHTTPRequest est brutalement interrompu et aucune information concernant
    la requête ne peut être remontée (request.status à 0, request.responseText et corps de requête vide), ce qui prête à confusion car
    cela provoque l'affichage du message d'erreur générique d'eUpdater qui n'est pas vraiment explicite.
    Pour traiter ce cas, on fait un appel serveur à eLoginMgr.ashx sur le même serveur, en lui demandant d'effectuer lui-même la requête
    vers le fichier eLoginMgr.ashx de cette URL externe (ex : http://serveur2/xrm/mgr/eLoginMgr.ashx).
    - si l'appel échoue, on renvoie un message d'erreur explicite en générant un flux XML d'erreur  ;
    - si l'appel réussit, eLoginMgr.ashx local (appelé par le JS) récupère le résultat de eLoginMgr.ashx distant et l'incorpore dans
      son corps de réponse pour que le JavaScript en cours puisse l'intercepter et permettre le remplissage de la liste des utilisateurs
    */




    var url = "mgr/eLoginMgr.ashx";
    var ednu = new eUpdater(url, null);

    ednu.addParam("rememberme", document.getElementById("RememberMe").value, "post");
    ednu.addParam("st", subscriberToken, "post");
    ednu.addParam("db", strDbToken, "post");
    ednu.addParam("action", "getuserlist", "get");
    //ednu.addParam("RedirectURL", redirectURL, "post");
    ednu.ErrorCallBack = function () { setWait(false); };
    ednu.send(dbListChangeTreatment);
}



///summary
///Fonction de callback assurant la gestion du rechargement des éléments d'interface au changement de la base de données cible
///<param name="oRes">Document XML résultant de l'appel AJAX récupérant les paramètres de connexion utilisateur de la base</param>
///summary
function dbListChangeTreatment(oRes) {
    document.getElementById("txtUserLogin").value = "";
    var cboDb = document.getElementById("cboBase");

    var strListSuccess = getXmlTextNode(oRes.getElementsByTagName("result")[0]);

    // Liste des utilisateurs non récupérée = AppUrl invalide (URL v7, serveur injoignable)
    if (strListSuccess != "SUCCESS") {
        // On ajoute un attribut sur l'option correspondant à cette base pour indiquer qu'elle ne pointe pas sur une URL XRM valide
        if (typeof (cboDb) != "undefined" && cboDb) {
            cboDb.options[cboDb.selectedIndex].setAttribute("xrm", "0");
            addClass(cboDb, "errBase");
            addClass(cboDb.options[cboDb.selectedIndex], "errBase");
        }
        addClass(document.getElementById("txtDatabase"), "errBase");

        // On affiche un message d'erreur et on empêche l'accès au login/MDP utilisateur
        document.getElementById("txtLogingSubscriberErr").innerHTML = getXmlTextNode(oRes.getElementsByTagName("errordescription")[0]);
        document.getElementById("errSub").style.display = "block";
        document.getElementById("txtUserLogin").setAttribute("disabled", "disabled");
        document.getElementById("txtUserPassword").setAttribute("disabled", "disabled");
        document.getElementById("txtUserLogin").style.display = "block";


        setWait(false);
        document.getElementById("UserLoginList").style.display = "none";
        return;
    }
    // Liste des utilisateurs récupérée : AppUrl valide
    else {
        // On ajoute un attribut sur l'option correspondant à cette base pour indiquer qu'elle pointe sur une URL XRM valide
        if (typeof (cboDb) != "undefined" && cboDb) {
            cboDb.options[cboDb.selectedIndex].setAttribute("xrm", "0");
            removeClass(cboDb, "errBase");
        }
        // On rétablit l'affichage des contrôles (TODO: à vérifier dans ManageLoginScenario)
        removeClass(document.getElementById("txtDatabase"), "errBase");
        document.getElementById("txtLogingSubscriberErr").innerHTML = "";
        document.getElementById("errSub").style.display = "none";
        document.getElementById("txtUserLogin").removeAttribute("disabled");
        document.getElementById("txtUserPassword").removeAttribute("disabled");
    }

    var oUsrList = oRes.getElementsByTagName("user");
    var cboUsr = document.getElementById("UserLoginList");

    for (var iusr = 0; iusr < oUsrList.length; iusr++) {
        var oUsr = oUsrList[iusr];


        var strUserId = getXmlTextNode(oUsr.childNodes[0]);
        var strUserLogin = getXmlTextNode(oUsr.childNodes[1]);
        var strUserName = getXmlTextNode(oUsr.childNodes[2]);

        //Création de l'élément sur le DOM      
        var usrOptNew = document.createElement("option");

        // Libelle/Value
        usrOptNew.text = strUserLogin;
        usrOptNew.value = strUserLogin;
        usrOptNew.setAttribute("UserId", strUserId);

        try {
            cboUsr.add(usrOptNew, cboUsr.options.length);
        }
        catch (ex) {
            cboUsr.add(usrOptNew, null);
        }
    }

    document.getElementById("UserLoginList").style.display = "block"; // à vérifier si userlistenabled est à false


    setWait(false);
    if (strUserLastConnection != null && bRememberMe) {
        setUser(strUserLastConnection);
    }
    else {
        onChangeUserList();
    }


    if (bRememberMe) {
        document.getElementById("txtUserLogin").style.display = "block";

        setWait(false);
        document.getElementById("UserLoginList").style.display = "none";
    }

    ManageLoginScenario();
}

///summary
///Fonction affectant à l'input de login utilisateur la valeur sélectionné dans la liste déroulante des utilisateurs
///summary
function onChangeUserList() {

    var cboUsr = document.getElementById("UserLoginList");
    var strUserLogin = cboUsr.options[cboUsr.selectedIndex].value;

    document.getElementById("txtUserLogin").value = strUserLogin;
    ManageLoginScenario();
}

function setUser(value) {
    var cboUsr = document.getElementById("UserLoginList");
    for (var iusr = 0; iusr < cboUsr.options.length; iusr++) {
        if (cboUsr.options[iusr].value == value) {
            cboUsr.options[iusr].selected = true;
            break;
        }
    }
    onChangeUserList();
    try {
        document.getElementById("txtUserPassword").focus();
    }
    catch (e) { }

}



///Appel La fene^re de mot de passe oublié
function forgotPassword(bSilent) {


    var strUserLogin = document.getElementById("txtUserLogin").value;

    if (typeof (strUserLogin) != "string" || strUserLogin.length == 0) {
        eAlert(2, GetAppRes(719), GetAppRes(373).replace("<ITEM>", GetAppRes(691)));
        return;
    }

    /* Paramètres */
    var sSubscriberToken = document.getElementById("SubscriberToken").value;

    var cboDb = document.getElementById("cboBase");
    var sDBToken = cboDb.options[cboDb.selectedIndex].value;
    var lang = document.getElementById("langue").value;




    /* Appel */
    modalPwd = new eModalDialog(top._res_6096, 0, "eForgotPassword.aspx", 620, 430);
    modalPwd.addParam("lang", lang, "post");
    modalPwd.addParam("UserLogin", strUserLogin, "post");
    modalPwd.addParam("dbt", sDBToken, "post");
    modalPwd.addParam("st", sSubscriberToken, "post");
    modalPwd.addParam("action", "forgotpasswordform", "post");

    modalPwd.ErrorCallBack = launchInContext(modalPwd, modalPwd.hide);


    modalPwd.show();
    modalPwd.addButton(top._res_29, null, "button-gray", this.jsVarName, "cancel"); // Annuler
    modalPwd.addButton(top._res_28, onForgetPwdOk, "button-green", this.jsVarName, "ok"); // Valider
}

function onForgetPwdOk() {
    modalPwd.getIframe().DoForgotPassword();
}

function onConfForgotPwdOk() {
    modalConfForgotPwd.hide();
    modalPwd.hide();
}



function AddToFav() {

    bookmarkurl = window.location;
    bookmarktitle = "eudonet XRM";
    if (document.all)
        window.external.AddFavorite(bookmarkurl, bookmarktitle);
    else if (window.sidebar) // firefox
        window.sidebar.addPanel(bookmarktitle, bookmarkurl, "");
}

function SetAsHpg() {
    document.getElementById("hp").style.behavior = 'url(#default#homepage)';
    document.getElementById("hp").setHomePage(window.location);
}

function GoEdx() {
    window.open("http://www.eudonet.fr");
}

function onChkRememberMe() {
    var elem = document.getElementById("chkRememberMe");

    document.getElementById("RememberMe").value = elem.getAttribute("chk");
}


function onInputLostFocus(oInput) {

    oInput.className = oInput.attributes["oldCss"].value;

}

function onInputFocus(oInput) {
    oInput.setAttribute("oldCss", oInput.className);
    oInput.className = "focusa";
}


function setWindowEnabled() {
    //Champs
    var oField = document.getElementsByTagName("INPUT");
    for (var i = 0; i < oField.length; i++) {
        oField[i].disabled = true;
    }

    //listes
    var oField = document.getElementsByTagName("select");
    for (var i = 0; i < oField.length; i++) {
        oField[i].disabled = true;
    }

    //Notes
    var oField = document.getElementsByTagName("TEXTAREA");
    for (var i = 0; i < oField.length; i++) {
        oField[i].readOnly = true;
    }
    //Lien hypertext

    var oField = document.getElementsByTagName("a");
    for (var i = 0; i < oField.length; i++) {
        oField[i].setAttribute("href", "javascript:eAlert(0,'" + top._res_5004 + "','" + top._res_692 + "');");
    }

    //images

    var oField = document.getElementsByTagName("img");
    for (var i = 0; i < oField.length; i++) {
        oField[i].setAttribute("onclick", "javascript:void('" + top._res_692 + "')");
    }
}





