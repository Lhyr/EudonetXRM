var nsAdminPref = nsAdminPref || {};

nsAdminPref.showUserCat = function (objBtn, idEditField, bMulti, bProfil, bProfilOnly, bIsMyEudonet) {
    //accède aux catalogues utilisateur / access to user's catalog
    userProfilePrefCat(objBtn, idEditField, bMulti, bProfil, bProfilOnly, bIsMyEudonet);
}


//Se connecter en tant que
nsAdminPref.cnxAs = function () {



    var myDoc = document;
    if (typeof myMod != "undefined" && myMod & myMod.oConditionListModal) {
        myDoc = myMod.getIframe().document;

    }

    var oValCnxAs = getAttributeValue(myDoc.getElementById('EDA_CNX_AS'), "ednvalue");
    var oValCnxAsAlias = getAttributeValue(myDoc.getElementById('EDA_CNX_AS'), "value");


    /// Obligatoire
    if (typeof oValCnxAs !== "string" || oValCnxAs.length === 0 || oValCnxAs === parseInt(oValCnxAs, 10)) {
        eAlert(0, top._res_372, top._res_373.replace('<ITEM>', top._res_7373));
        return;
    }

    /// Obtention du token de connexion
    var conf = top.eConfirm(1, "", "Vous allez être déconnecté de votre session d'administration et être reconnecté en tant que " + oValCnxAsAlias + ". Êtes-vous sur ?", "", null, null,
         function () {
             var upd = new eUpdater("eda/mgr/eAdminTokenLogin.ashx", 1);
             upd.addParam("uid", oValCnxAs, "post");
             upd.ErrorCallBack = function () { top.setWait(false) };

             top.setWait(true);
             upd.send(
                 function (oRes) {

 

                     // Si sendJson fait appel à un manager qui ne renvoie pas de JSON en retour, on ne fait rien de plus
                     if (typeof (oRes) == "undefined" || oRes == null || oRes == "")
                         return;

                     var res = JSON.parse(oRes);
                     if (res.Success) {
                         top.window.location.reload(true);
                         top.setWait(false);
                     }
                     else
                         top.eAlert(0, res.ErrorTitle, res.ErrorMsg);
                 }
             );
         },
         null,
         false,
         true);
}

nsAdminPref.copyPref = function (myMod) {

    var myDoc = document;
    if (typeof myMod != "undefined" && myMod & myMod.oConditionListModal) {
        myDoc = myMod.getIframe().document;


    }
    if (myDoc.getElementById('EDA_CPREF_SRC') != null) {
        var oValSrc = getAttributeValue(myDoc.getElementById('EDA_CPREF_SRC'), "ednvalue");
        var oValDst = getAttributeValue(myDoc.getElementById('EDA_CPREF_DST'), "ednvalue");

        if (typeof oValSrc !== "string" || oValSrc.length === 0 || oValSrc === parseInt(oValSrc, 10)) {

            eAlert(0, top._res_372, top._res_373.replace('<ITEM>', top._res_7373));

            return;
        }


        if (typeof oValDst !== "string" || oValDst.length === 0) {
            eAlert(0, top._res_372, top._res_373.replace('<ITEM>', top._res_7879));
            return;
        }

        var arrValDst = oValDst.split(';');
        if (arrValDst.findIndex && arrValDst.findIndex(function (val) { return val === oValSrc }) !== -1) {

            eAlert(0, top._res_372, top._res_6384);

            return;
        }


        var conf = top.eConfirm(1, "", top._res_7907, "", null, null,
            function () {
                var upd = new eUpdater("eda/mgr/eAdminAccessPrefManager.ashx", 1);
                upd.addParam("action", "copy", "post");
                upd.ErrorCallBack = function () { top.setWait(false) };
                upd.addParam("src", oValSrc, "post");
                upd.addParam("dst", arrValDst.filter(function (num) { return num == parseInt(num, 10) || /^G\d+$/.test(num) }).filter(function (zz, idx, self) { return self.indexOf(zz) === idx }).reduce(function (a, b) { return a + ";" + b; }, ""), "post");
                top.setWait(true);
                upd.send(
                    function (oRes) {

                        setWait(false);

                        // Si sendJson fait appel à un manager qui ne renvoie pas de JSON en retour, on ne fait rien de plus
                        if (typeof (oRes) == "undefined" || oRes == null || oRes == "")
                            return;

                        var res = JSON.parse(oRes);
                        if (res.Success)
                            top.eAlert(4, top._res_7908, top._res_1761);
                        else
                            top.eAlert(0, res.ErrorTitle, res.ErrorMsg);
                    }
                );
            },
            null,
            false,
            true);
    }
    


}

// Demande de confirmation pour réinitialiser les préférences
nsAdminPref.resetPref = function () {

    var caps = {};
    caps.action = "reset";
    var conf = top.eConfirm(1, top._res_7713, top._res_7714, "", null, null,
             function () {
                 var upd = new eUpdater("eda/mgr/eAdminAccessPrefManager.ashx", 1);
                 upd.addParam("action", "reset", "post");
                 setWait(true);
                 upd.send(
                     function (oRes) {

                         setWait(false);

                         // Si sendJson fait appel à un manager qui ne renvoie pas de JSON en retour, on ne fait rien de plus
                         if (typeof (oRes) == "undefined" || oRes == null || oRes == "")
                             return;

                         var res = JSON.parse(oRes);
                         if (res.Success)
                             top.eAlert(4, top._res_7713, top._res_1761);
                         else
                             top.eAlert(1, "", res.ErrorMessage);
                     }
                 );
             },
             null,
             false,
             true);
}

