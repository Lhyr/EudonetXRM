var nsExtranetAdmin = nsExtranetAdmin || {};

nsExtranetAdmin.updateParam = function (elem, id, confirm) {
    
    confirm = Object.assign({}, { needConfirm: false, msgTitle: "", msgBody: "" }, confirm)


    if (confirm.needConfirm) {
        eConfirm(0, confirm.msgTitle, confirm.msgBody, '', 500, 200, function () { nsExtranetAdmin.updateParam(elem, id) }, function () { });
        return;
    }


    var dsc = getAttributeValue(elem, "dsc");
    var val = elem.value;
    var extid = 0;

    var caps = dsc.split('|')

    switch (caps[1 + ""]) {
        case "1":
            //Nom de l'extranet
            break;
        case "2":
            //Activé
            val = getAttributeValue(elem, "chk");
            break;
        case "3":
            //TOM Extranet
            break;
        case "4":
            // Nb User
            break;
        case "5":
            // Regen key
            break;

        case "6":

            //création extranet, 5 max
            extid = getAttributeValue(elem, "stid");

            break;
    }

    var upd = new eUpdater("eda/Mgr/eAdminExtranetParamManager.ashx", 1);

    upd.ErrorCallBack = function () {
        nsAdmin.revertLastValue(elem);
        setWait(false);
    };

    var obj = {
        p: caps[1],
        i: id,
        v: val,
        extid: extid
    }

    upd.json = JSON.stringify(obj);
    setWait(true);

    upd.send(function (oRes) {
        setWait(false);

        var res = JSON.parse(oRes);

        if (!res.Success) {

            var sT = nsExtranetAdmin.ReplaceRes(res.ErrorTitle);
            var sE = nsExtranetAdmin.ReplaceRes(res.ErrorMsg);
            var sD = nsExtranetAdmin.ReplaceRes(res.ErrorDetailMsg);
            top.eAlert(1, sT, sE, sD);

        }
        else {

            var ext = JSON.parse(res.ExtranetValue);
            var t = document.querySelector("[ednid='ADMIN_EXTENSIONS_EXTRANET_" + ext.i + "_token_" + ext.i + "'] > input");
            if (caps[1] == 6 && !t) {



                var oDiv = document.createElement("div");
                oDiv.innerHTML = ext.content;

                var newBloc = oDiv.querySelector("div[ednextid='" + ext.i + "']");

                var st = document.getElementById("settings");

                st.insertBefore(newBloc, st.lastElementChild)


                nsAdmin.addStepTitleEventListeners()


                newBloc.scrollIntoView();

            }


            if (t) {


                t.value = ext.k;
                if (ext.v == 1) {
                    var tInvalid = document.querySelector("[ednin='ADMIN_EXTENSIONS_EXTRANET_" + ext.i + "_invalid_" + ext.i + "'] > input");
                    if (tInvalid)
                        tInvalid.remove()

                }

                var ttitle = document.querySelector("#section_ADMIN_EXTENSIONS_EXTRANET_" + ext.i + "  > span.stepTitle");
                if (ttitle) {
                    if (ext.n)
                        ttitle.innerHTML = top._res_2956.replace("##NAME##", ext.n)
                    else
                        ttitle.innerHTML = top._res_2956.replace("##NAME##", ext.i)
                }
            }
        }



    });
}



nsExtranetAdmin.deleteExt = function (id, name, confirm) {

    confirm = Object.assign({}, { needConfirm: true, msgTitle: "Confirmation", msgBody: top["_res_2957"].replace('##NAME##', id) }, confirm)


    if (confirm.needConfirm) {

        eConfirm(0, confirm.msgTitle, confirm.msgBody, '', 500, 200, function () { nsExtranetAdmin.deleteExt(id, name, { needConfirm: false }) }, function () { });

        return;
    }

    var upd = new eUpdater("eda/Mgr/eAdminExtranetParamManager.ashx", 1);

    upd.ErrorCallBack = function () { setWait(false); };

    var obj = { p: 7, i: id, }

    upd.json = JSON.stringify(obj);
    setWait(true);

    upd.send(function (oRes) {
        setWait(false);

        var res = JSON.parse(oRes);

        if (!res.Success) {

            var sT = nsExtranetAdmin.ReplaceRes(res.ErrorTitle);
            var sE = nsExtranetAdmin.ReplaceRes(res.ErrorMsg);
            var sD = nsExtranetAdmin.ReplaceRes(res.ErrorDetailMsg);


            top.eAlert(1,
                sT, sE, sD);
        }
        else {

            // top.eAlert(0, "Mise à jour réussi", "Mise à jour réussi", "");
            var t = document.querySelector("div[ednextid='" + id + "']");
            if (t)
                t.remove();
        }



    });
}

nsExtranetAdmin.ReplaceRes = function (str) {
    if (str == null || typeof str != "string" || str == "")
        return ""

    var rg = /({{(\d+)}})/;

    var nMax = 0
    var bContinue = true

    while (nMax < 5 && bContinue) {
        lst = str.match(rg)

        nMax++;
        if (lst != null && lst.length == 3) {
            str = str.replace(lst[1], top["_res_" + lst[2]]);


        }
        else
            bContinue = false;
    }


    return str;

}