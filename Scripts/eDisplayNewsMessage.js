nsNewsMsg = window.nsNewsMsg || {};

if (!window.nsNewsMsg)
    window.nsNewsMsg = nsNewsMsg;

nsNewsMsg.called = false;
nsNewsMsg.displayMsg = function () {
    
    var num = getCookie("ednstopnews");

    if (num != "")
        num = getNumber(num);

    if (isNaN(num))
        num = 0;

    if (isNaN(top._newsLetterNum))
        return;

    if (num >= getNumber(top._newsLetterNum))
        return;

    nsNewsMsg.called = true;

    // US #2 244 - Demande #80 848 - Nouvelle popup newsletter pour les navigateurs modernes
    var browser = new getBrowser();
    // Pour IE : ancienne popup
    if (browser.isIE) {
        // L'url ne dépend pas du numéro de version
        // US #2 244 - Demande #80 848 - Sur IE, l'URL est désormais en dur, et ne varie qu'en fonction de la langue
        var newsletterLang = (top && top._newsLetterLang == "LANG_00") ? "fr" : "uk";
        var sBaseURL = "https://" + newsletterLang + ".eudonet.com/news-admin/News-Admin.html";
        /*
        if (top && top._newsLetterUrl)
            sBaseURL = top._newsLetterUrl + "";
        */

        var oModalMSG = new eModalDialog("Newsletter", 0, "blank", 850, 650, null, true);
        oModalMSG.hideMaximizeButton = true;

        //CNA - Demande #49015
        //le gif d'animation du setWait a son z-index à 999, on place donc la modal encore plus haut
        oModalMSG.level = getComputedStyle(document.getElementById("contentWait"), null).zIndex + 1;

        oModalMSG.show();

        oModalMSG.addButtonFct(top._res_2379, function () { oModalMSG.hide() }, 'button-green', null, "");   //Lire plus tard
        oModalMSG.addButtonFct(top._res_2380, function () { nsNewsMsg.StopDisplay(oModalMSG); }, 'button-gray', null, "");   //Ok, Marquer comme lu

        oModalMSG.getIframeTag().src = sBaseURL;
    }
    // Pour les autres, appel à la fonction LoadIrisHome() de eInitNewErgo
    else {
        var sBaseURL = "https://" + newsletterLang + ".eudonet.com/news-admin/News-Admin.html";
        var aNewsletterLinks = [];
        var sNewsLetterButtonText = top._res_2768;
        if (top) {
            if (top._newsLetterUrl)
                sBaseURL = top._newsLetterUrl + "";
            if (top._newsLetterLinks)
                aNewsLetterLinks = top._newsLetterLinks;
            if (top._newsLetterButtonText)
                sNewsLetterButtonText = top._newsLetterButtonText;
        }
        addScript("../IRISBlack/Front/Scripts/eInitNewErgo", "HOME", function () {
            LoadIrisHome({ sUrl: sBaseURL, aLinks: aNewsletterLinks, sButtonText: sNewsLetterButtonText });
        });
    }
}

nsNewsMsg.StopDisplay = function (mod) {
    // Mise à jour du cookie côté client
    setCookie("ednstopnews", getNumber(top._newsLetterNum), true);
    // Mise à jour de l'information en base ([DISPLAYVERMSG] dans [CONFIG])
    var updt = new eUpdater("mgr/eUserOptionsManager.ashx", 1);
    updt.addParam("optiontype", "1", "post");
    updt.addParam("option", "5", "post");
    updt.addParam("optionvalue", "1", "post");
    updt.send();
    // Fermeture de la popup (ancienne version, IE uniquement)
    if (mod)
        mod.hide();
}






nsNewsMsg.CallDomBackLoaded = function () {

    if (document.addEventListener) {
        document.addEventListener("DOMContentLoaded", nsNewsMsg.displayMsg, false);
    }
    else if (document.attachEvent) {
        document.attachEvent("onreadystatechange", function () {
            if (document.readyState == "complete") {
                nsNewsMsg.displayMsg();
            }
        });
    }
}


nsNewsMsg.CallDomBackLoaded();