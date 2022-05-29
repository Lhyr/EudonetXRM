var oModal;
///Poste la demande de mot de passe oublié
function DoForgotPassword() {

    document.getElementById("mail_err_lbl").style.display = "none";
    document.getElementById("mail_lbl").style.display = "";
    document.getElementById("txt_mail").className = document.getElementById("txt_mail").className.replace(" invalid", "");


    //ELAIZ - backlog #1819 - retrait d'une classe CSS invalid sur le conteneur du champ email
    if (!document.querySelector("#txt_mail.invalid"))
        document.querySelector(".mail").classList.remove("invalid");

    document.getElementById("txt_captcha").className = document.getElementById("txt_captcha").className.replace(" invalid", "");
    document.getElementById("captcha_lbl").className = document.getElementById("captcha_lbl").className.replace(" InvalidTxtCaptcha ", "");

    var email = document.getElementById("txt_mail").value;

    if (email == '' || !/^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,3})+$/.test(email)) {
        document.getElementById("txt_mail").classList.add("invalid");
        return;
    }

    var captcha = document.getElementById("txt_captcha").value;

    if (captcha == '') {
        document.getElementById("txt_captcha").classList.add("invalid");
        return;
    }
    top.setWait(true);
    var login = document.getElementById("userlogin").value;

    //var ednu = new eUpdater("eForgotPassword.aspx", 0);
    var ednu = new eUpdater("mgr/eLoginMgr.ashx", 0);

    var sDBT = document.getElementById("dbt").value;
    var sST = document.getElementById("st").value;
    var lang = document.getElementById("lang").value;

    ednu.addParam("UserLogin", login, "post");
    ednu.addParam("UserEmail", email, "post");
    ednu.addParam("Captcha", captcha, "post");
    ednu.addParam("dbt", sDBT, "post"); // Token Bdd
    ednu.addParam("st", sST, "post");   // Token Abonné
    ednu.addParam("action", "forgotpassword", "get");
    ednu.ErrorCallBack = function () {
        top.setWait(false);
    };

    ednu.send(DoForgotPasswordTreatment, null);
    return false;
}

function DoForgotPasswordTreatment(oRes) {
    top.setWait(false);

    var sSuccess = getXmlTextNode(oRes, "result");
    var sMsg = getXmlTextNode(oRes, "msg");
    var sErrSrc = getXmlTextNode(oRes, "src");

    if (sSuccess == 'SUCCESS') {


        parent.modalConfForgotPwd = new top.eModalDialog(_res_6096, 1, null, 500, 145, "modalConfForgotPwd");
        parent.modalConfForgotPwd.setMessage(_res_6181, sMsg, '3');
        parent.modalConfForgotPwd.show();
        parent.modalConfForgotPwd.adjustModalToContent(40);
        parent.modalConfForgotPwd.addButton(_res_30, parent.onConfForgotPwdOk, 'button-gray', null);
    }
    else if (sErrSrc == 'mail') {


        parent.modalConfForgotPwd = eAlertAdvParam({
            criticity: top.eMsgBoxCriticity.MSG_CRITICAL,
            title: top._res_72,
            message: sMsg,
            width: 500,
            height: 135,
            okFct: parent.onConfForgotPwdOk,

        });



    }
    else {
        document.getElementById(sErrSrc + "_lbl").className += " InvalidTxtCaptcha ";
        document.getElementById("txt_captcha").classList.add("invalid");
    }

}

function onCaptchaLoad() {
    document.getElementById("ImgCapcha").style.display = "block";
    document.getElementById("ImgLoading").style.display = "none";
}


function reloadCaptcha() {
    document.getElementById("ImgCapcha").style.display = "none";
    document.getElementById("ImgLoading").style.display = "block";
    document.getElementById("ImgCapcha").src = "ecaptchaget.aspx?date=" + ((new Date()).getTime());
}

function onloadLib() {
    setCurrentTxt(document.getElementById("chapeau"), GetAppRes(6222));
    setCurrentTxt(document.getElementById("mail_lbl"), GetAppRes(656));
    setCurrentTxt(document.getElementById("captcha_lbl"), GetAppRes(6224) + " :");
    setCurrentTxt(document.getElementById("text-reload"), GetAppRes(6225));
    document.getElementById("resdown").title = GetAppRes(6226);
    document.getElementById("text-help").title = GetAppRes(6187);
    document.getElementById("ImgCapcha").src = "ecaptchaget.aspx?date=" + ((new Date()).getTime());

    setCurrentTxt(document.getElementById("lnkCancel"), GetAppRes(29));
    setCurrentTxt(document.getElementById("lnkDo"), GetAppRes(5003));
}

function setCurrentTxt(obj, sText) {
    if (obj != null)
        SetText(obj, sText);
}
