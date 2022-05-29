
// NameSpace pour les fonctions propre à l'admin
var nsAdminUsers = nsAdminUsers || {};
var oModalGroup;


var USERS_INFO_ACTION = {
    UNDEFINED: 0,
    RENDER_POPUP_COPYPREF: 1,
    UPDATE: 2,
    RENDER_POPUP_CHANGEP_WD: 3,
    RENDER_POPUP_CHANGE_SIG: 4,
    RENDER_POPUP_CHANGE_MEMO: 5,
}

//Init de la popup des users depuis admin
nsAdminUsers.InitDefault = function (oModFile, strTargetGroupItemCode) {
    // Préselection d'un groupe lors de l'ajout d'un utilisateur depuis le clic sur un
    var bInitDefaultGroup = true;

    var sVal = strTargetGroupItemCode;
    if (sVal == null || typeof (sVal) == "undefined")
        sVal = eCU.selectedItem;
    if (sVal == null || typeof (sVal) == "undefined")
        bInitDefaultGroup = false;

    // Affectation de la fonction déclenchée au clic sur la corbeille de la barre d'outils en haut de la fenêtre
    oModFile.getIframe().onDeleteButton = function () { nsAdminUsers.onUserDelete(oModFile); };
    if (bInitDefaultGroup) {
        var myDoc = oModFile.getIframe().document;
        var oSel = myDoc.querySelector("#SEL_COL_101000_101027_0_0_0");
        if (oSel) {
        for (var i = 0, j = oSel.options.length; i < j; ++i) {
            if ('G' + oSel.options[i].value === sVal) {
                oSel.selectedIndex = i;
                break;
            }
        }
        }

        nsEfileJS.UpdateSelect(oSel, myDoc);
    }


}

nsAdminUsers.FilterByGroup = function (oBj, oCu) {

    if (!oBj) {
        return;
    }


    var bchecked = (getAttributeValue(oBj, "chk") == "1");
    var oSelectedObject = oBj.parentNode;

    //on stocke la valeur parmi les valeurs à enregistrer
    oCu.ClickVal(oSelectedObject, bchecked);


    //Maj pref filtre rapide
    var sVar = oCu.GetSelectedIds();
    var sValueExpress = "";
    var aMyElem = sVar.split(";");

    aMyElem.forEach(function (myGrp) {

        var sCurrGrp = myGrp.replace("G", "");
        if (isNumeric(sCurrGrp)) {

            if (sValueExpress.length > 0) {
                sValueExpress = sValueExpress + ";";
            }

            sValueExpress += sCurrGrp;
        }
    });

    if (sValueExpress.length > 0)
        var updatePref = "tab=101000;$;filterExpress=101027;|;8;|;" + sValueExpress;
    else
        var updatePref = "tab=101000;$;filterExpress=101027;|;;|;$cancelthisfilter$";

    top.updateUserPref(
        updatePref,
        function () {
            loadList();
        }
    );
}

//Mode fiche user
nsAdminUsers.goFile = function (obj) {




}

nsAdminUsers.initPagging = function () {
    //Disable/enabled les boutons de pagging
    var currentFile = document.getElementById("fileDiv_101000");
    if (currentFile && currentFile.tagName && currentFile.tagName.toLowerCase() != "div")
        return;

    if (!nsAdminUsers.hasOwnProperty("ListUserId"))
        return;

    //DescId & Id de la ficher en cours
    var nCurrentFileId = currentFile.getAttribute("fid");

    var idx = nsAdminUsers.ListUserId.ListId.indexOf(nCurrentFileId);

    if (idx != -1) {

        var btnFirst = document.getElementById("BrowsingFirst");
        setAttributeValue(btnFirst, "eenabled", idx == 0 ? "0" : "1");

        var btnPrev = document.getElementById("BrowsingPrevious");
        setAttributeValue(btnPrev, "eenabled", idx == 0 ? "0" : "1");

        var btnNext = document.getElementById("BrowsingNext");
        setAttributeValue(btnNext, "eenabled", idx >= (nsAdminUsers.ListUserId.ListId.length - 1) ? "0" : "1");

        var btnLast = document.getElementById("BrowsingLast");
        setAttributeValue(btnLast, "eenabled", idx >= (nsAdminUsers.ListUserId.ListId.length - 1) ? "0" : "1");

    }
}

nsAdminUsers.loadUsersId = function (nPage, fctCallBack) {


    if (!isNumeric(nPage)) {
        nPage = 1;
    }

    if (nPage < 0)
        nPage = 1;

    //On ne reload pas si la page n'a pas changée
    if (nsAdminUsers.hasOwnProperty("ListUserId") && nsAdminUsers.ListUserId.Page == nPage) {

        return;
    }

    var oListIdsUpdater = new eUpdater("mgr/eQueryManager.ashx", 1);
    oListIdsUpdater.ErrorCallBack = function () { };
    oListIdsUpdater.addParam("type", 3, "post");

    var oeParam = getParamWindow();
    var nRows = 0;
    if (typeof (oeParam.GetParam) != "undefined" && oeParam.GetParam('Rows') != '') {
        nRows = oeParam.GetParam('Rows');
    }

    oListIdsUpdater.addParam("rows", nRows, "post");
    oListIdsUpdater.addParam("tab", 101000, "post");
    oListIdsUpdater.addParam("page", nPage, "post");


    oListIdsUpdater.send(
        function (oRes) {
            nsAdminUsers.ListUserId =
                {
                    Page: nPage,
                    ListId: oRes.split(";")
                }

            if (typeof fctCallBack == "function") {
                fctCallBack();
            }


        }

        );
}


//Navigation entre fiche
nsAdminUsers.browseFile = function (obj, bFromReload) {

    if (typeof bFromReload == "undefined")
        bFromReload = false;

    if (getAttributeValue(obj, "eenabled") != 1)
        return false;


    var myObj = { lnkid: -1 };
    myObj.getAttribute = function (sName) {
        if (myObj.hasOwnProperty(sName))
            return myObj[sName];
        else
            return "";
    }

    var args = Array.prototype.slice.call(arguments);
    if (args.length > 1)
        args[2] = true;
    else
        args.push(true);

    var fctRecall = function () {
        nsAdminUsers.browseFile(args);
    }


    var nId = getAttributeValue(myObj, "lnkid");
    var nAction = getAttributeValue(obj, "eaction") + "";
    var nTab = 101000;

    if (!nsAdminUsers.hasOwnProperty("ListUserId")) {
        if (bFromReload)
            return;
        nsAdminUsers.loadUsersId(1, fctRecall);
    }

    //
    if (!Array.isArray(nsAdminUsers.ListUserId.ListId) || nsAdminUsers.ListUserId.ListId.length == 0)
        return;

    var currentFile = document.getElementById("fileDiv_101000");
    if (currentFile && currentFile.tagName && currentFile.tagName.toLowerCase() != "div")
        return;


    //DescId & Id de la ficher en cours
    var nCurrentFileId = currentFile.getAttribute("fid");

    var idx = nsAdminUsers.ListUserId.ListId.indexOf(nCurrentFileId);


    if (idx == -1) {
        myObj.lnkid = nsAdminUsers.ListUserId.ListId[0];
    }
    else {
        switch (nAction) {
            case "0":
                //Next
                if (idx + 1 > nsAdminUsers.ListUserId.ListId.length) {
                    nsAdminUsers.loadUsersId(nsAdminUsers.ListId.Page + 1, fctRecall);
                    return;
                }
                else
                    myObj.lnkid = nsAdminUsers.ListUserId.ListId[idx + 1];



                break;
            case "1":
                //Next
                if (idx - 1 < 0) {
                    nsAdminUsers.loadUsersId(nsAdminUsers.ListId.Page - 1, fctRecall);
                    return;
                }
                else
                    myObj.lnkid = nsAdminUsers.ListUserId.ListId[idx - 1];
                break;
            case "2":
                //First
                myObj.lnkid = nsAdminUsers.ListUserId.ListId[0];
                break;
            case "3":
                //Last
                myObj.lnkid = nsAdminUsers.ListUserId.ListId[nsAdminUsers.ListUserId.ListId.length - 1];
                break;
            default:
                //Next (getAttributeValue remonte "" si l'attribut vaut 0
                myObj.lnkid = nsAdminUsers.ListUserId.ListId[0];
        }
    }

    if (myObj.lnkid > 0)
        nsAdmin.loadAdminModule(USROPT_MODULE_ADMIN_TAB_USER, null, { tab: 101000, userid: myObj.lnkid });
}

nsAdminUsers.updateContent = function (oRes, nFileId) {




    top.updateFile(oRes, 101000, nFileId, 3);


    //document.getElementById("mainDiv").innerHTML = oRes;

}

nsAdminUsers.FilterGroupCatalog = function (event, sVal) {



    window.clearTimeout(nsAdminUsers._searchTimer);
    nsAdminUsers._searchTimer = window.setTimeout(function () { nsAdminUsers.FilterGroupCatalogShowHide(sVal) }, 500);

}

nsAdminUsers.FilterGroupCatalogBtn = function (event, sVal) {

    var oBtnSrch = document.getElementById("eBtnSrch");
    if (getAttributeValue(oBtnSrch, "srchstate") == "on") {
        var oTxt = document.getElementById("eTxtSrch");
        oTxt.value = "";
        nsAdminUsers.FilterGroupCatalog(event, "");
    }
    else
        nsAdminUsers.FilterGroupCatalog(event, sVal);
}

nsAdminUsers.FilterGroupCatalogShowHide = function (sV) {
    sV = sV + "";
    var oRoot = document.getElementById("ResultDiv");
    var alls = oRoot.querySelectorAll("li[id^='eTVB_']");

    var oAllArr = [].slice.call(alls);

    sV = sV.toLowerCase();


    var oBtnSrch = document.getElementById("eBtnSrch");
    if (sV.length > 0) {

        setAttributeValue(oBtnSrch, "srchstate", "on");
        switchClass(oBtnSrch, "icon-magnifier", "icon-edn-cross");

    }
    else {
        setAttributeValue(oBtnSrch, "srchstate", "off");
        switchClass(oBtnSrch, "icon-edn-cross", "icon-magnifier");
    }

    //
    oAllArr.forEach(function (myLi) {
        var sSpaniId = myLi.id.replace("eTVB_", "eTVBLVT_");
        var myVal = myLi.querySelector("span[id='" + sSpaniId + "']");
        var sVal = myVal.innerHTML.toLowerCase();
        var search = getAttributeValue(myLi, "ednsearch");
        if (sV.length > 0 && sVal.indexOf(sV) > -1)
            setAttributeValue(myLi, "edndisplay", "1");
        else
            setAttributeValue(myLi, "edndisplay", "0");


    });


    //
    oAllArr.forEach(function (myLi) {
        var search = getAttributeValue(myLi, "edndisplay");
        if (search == "0" && sV.length > 0) {

            if (myLi.querySelectorAll("li[id^='eTVB_'][edndisplay='1']").length == 0)
                myLi.style.display = "none";
            else
                myLi.style.display = "";

        }
        else
            myLi.style.display = "";

    });


}

nsAdminUsers.getGroupDialog = function (nAction, strTargetItemCode) {
    var strType = "0";
    var strUrl = "eda/eAdminGroupDialog.aspx";
    var nWidth = 525;
    var nHeight = 250;

    if (!strTargetItemCode || strTargetItemCode == '') {
        strTargetItemCode = eCU.selectedItem;
    }

    var nSelectedGroupId = 0;
    var strSelectedGroupName = top._res_7576; // <Racine>
    if (strTargetItemCode && strTargetItemCode != '' && strTargetItemCode != "ROOT") {
        nSelectedGroupId = Number(strTargetItemCode.replace("G", ""));
        var oSelectedGroupLabel = document.getElementById("eTVBLVT_" + strTargetItemCode);
        if (oSelectedGroupLabel.textContent)
            strSelectedGroupName = oSelectedGroupLabel.textContent;
        else if (oSelectedGroupLabel.innerText)
            strSelectedGroupName = oSelectedGroupLabel.innerText;
    }

    if (nAction != 0 && (!strTargetItemCode || strTargetItemCode == '')) {
        eAlert(1, top._res_6536, top._res_7580); // "Veuillez sélectionner un groupe à modifier ou supprimer."
        return;
    }
    if (nAction != 0 && (strTargetItemCode == 'ROOT')) {
        eAlert(1, top._res_6536, top._res_7581); // "Vous ne pouvez pas modifier ou supprimer ce groupe."
        return;
    }

    if (nAction < 2) {
        var strTitle = top._res_7582; // Paramètres du groupe
        oModalGroup = new eModalDialog(strTitle, strType, strUrl, nWidth, nHeight);

        oModalGroup.addParam("action", nAction, "post");
        oModalGroup.addParam("groupId", nSelectedGroupId, "post");

        oModalGroup.addScript("eTools");
        oModalGroup.noButtons = false;

        oModalGroup.show();

        oModalGroup.addButton(top._res_28, nsAdminUsers.onGroupValid, "button-green", null);
        oModalGroup.addButton(top._res_29, nsAdminUsers.onGroupCancel, "button-gray", null);
    }
    else {
        oModalGroup = eConfirm(1, top._res_6536, top._res_7583.replace("<ITEM>", strSelectedGroupName), "", 300, 200,
        function () { nsAdminUsers.onGroupDelete(nSelectedGroupId); }); // Supprimer le groupe <ITEM> ?
    }
}

nsAdminUsers.addGroup = function (strTargetItemCode) {
    nsAdminUsers.getGroupDialog(0, strTargetItemCode);
}

nsAdminUsers.editGroup = function (strTargetItemCode) {
    nsAdminUsers.getGroupDialog(1, strTargetItemCode);
}

nsAdminUsers.delGroup = function (strTargetItemCode) {
    nsAdminUsers.getGroupDialog(2, strTargetItemCode);
}

nsAdminUsers.addUserInGroup = function (strTargetItemCode) {
    shFileInPopup(101000, 0, top._res_31.replace("'", "\'"), null, null, 0, null, false, null, 2, null, null, function (oModFile) { nsAdminUsers.InitDefault(oModFile, strTargetItemCode); });
}

nsAdminUsers.onGroupDialogSubmit = function (event) {



    nsAdminUsers.onGroupValid();

    if (event.preventDefault) {
        event.preventDefault();
    } else {
        event.returnValue = false; // IE
    }

    return false;
}

nsAdminUsers.onGroupValid = function () {



    var modalDocument = oModalGroup.getIframe().document;

    var nGroupId = modalDocument.getElementById("groupId").value;
    var nAction = modalDocument.getElementById("action").value;
    var strGroupName = modalDocument.getElementById("inputGroupName").value;
    var bGroupPublic = modalDocument.getElementById("chkGroupPublic").getAttribute("chk") == "1";
    var nParentGroupId = modalDocument.getElementById("ddlParentGroup").value;
    var nCurrentParentGroupId = modalDocument.getElementById("currentParentGroupId").value;




    var oUpdateFct = function () {
        var upd = new eUpdater("eda/Mgr/eAdminGroupManager.ashx", 1);

        upd.addParam("action", nAction, "post");
        upd.addParam("groupId", nGroupId, "post");
        upd.addParam("groupName", strGroupName, "post");
        upd.addParam("groupPublic", bGroupPublic ? "1" : "0", "post");
        upd.addParam("parentGroupId", nParentGroupId, "post");

        upd.send(
            // Résultat
            function (oRes) {
                var res = JSON.parse(oRes);
                if (!res.Success) {
                    eAlert(1, top._res_6237, res.Error); // Une erreur est survenue lors de la mise à jour
                }
                else {
                    //
                }

                oModalGroup.hide();

                nsAdmin.loadAdminModule(USROPT_MODULE_ADMIN_ACCESS_USERGROUPS, null, { tab: 101000 });
            }
        );

        oModalGroup.hide();
    }

    if (nAction == 1 && nCurrentParentGroupId != nParentGroupId) {
        var oModalConfirmParentGroupChange = eConfirm(1, top._res_6536,
            top._res_7584,
            "", 400, 200, oUpdateFct
        ); // La modification de la hiérarchie des groupes peut entrainer un temps de traitement long, notamment si certains onglets utilisent les fonctions de confidentialité. Cette opération risque de dégrader temporairement les performances de votre base.
    }
    else
        oUpdateFct();
}

nsAdminUsers.onGroupCancel = function () {
    oModalGroup.hide();
}

nsAdminUsers.onGroupDelete = function (nGroupId) {
    var upd = new eUpdater("eda/Mgr/eAdminGroupManager.ashx", 1);
    upd.addParam("action", "2", "post");
    upd.addParam("groupId", nGroupId, "post");
    upd.ErrorCallBack = function (oRes) { nsAdminUsers.afterGroupDelete(oRes, true); }

    upd.send(
        // Résultat
        function (oRes) {
            nsAdminUsers.afterGroupDelete(oRes, false);
        }
    );
}


//Demande de suppression d'un utilisateur
nsAdminUsers.userDelete = function (nFileId) {

    var oModFile = {};
    oModFile.fileId = nFileId;
    oModFile.hide = function () { };
    oModFile.getIframe = function () { return top };

    nsAdminUsers.onUserDelete(oModFile);
}


//Demande de remplacement des pref d'un user
nsAdminUsers.replacePref = function (nFileId) {

    var oReplacePrefDlg = new eModalDialog(top._res_7908, 0, "mgr/eUserInfosManager.ashx", 800, 310, "oReplacePrefDlg");
    oReplacePrefDlg.addParam("fileid", nFileId, "post");
    oReplacePrefDlg.addParam("action", USERS_INFO_ACTION.RENDER_POPUP_COPYPREF, "post"); //
    oReplacePrefDlg.show();



    oReplacePrefDlg.addButton(top._res_29, null, "button-green", null, "cancel"); // Annuler



        oReplacePrefDlg.addButton(top._res_28, function () {
            oReplacePrefDlg.getIframe().nsAdminPref.copyPref(oReplacePrefDlg)
        }, "button-red", null, "ok"); // Valider
    
}



//Demande de changement de password
nsAdminUsers.chgPwd = function (nFileId) {
    var eMemoDialogEditorObject = null;
    var modalPwdChange = new eModalDialog(top._res_7959, 0, "mgr/eUserInfosManager.ashx", 550, 250, "modalPwdChange");

    modalPwdChange.addParam("fileid", nFileId, "post");
    modalPwdChange.addParam("action", USERS_INFO_ACTION.RENDER_POPUP_CHANGEP_WD, "post"); //

    modalPwdChange.noRedirect = true;
    modalPwdChange.userid = nFileId;
    modalPwdChange.show();

    modalPwdChange.addButton(top._res_29, null, "button-gray", null, "cancel"); // Annuler
    modalPwdChange.addButton(top._res_28, function () { modalPwdChange.getIframe().onPwdValid(modalPwdChange) }, "button-green", null, "ok"); // Valider


}

//Demande de changement de mémo
nsAdminUsers.chgMemo = function (nFileId) {


    var oChgMemo = new eModalDialog(top._res_7962, 0, "mgr/eUserInfosManager.ashx", 1000, 350, "oChgMemo");
    oChgMemo.addParam("iframeScrolling", "no", "post");
    oChgMemo.addParam("fileid", nFileId, "post");
    oChgMemo.addParam("action", USERS_INFO_ACTION.RENDER_POPUP_CHANGE_MEMO, "post"); //

    oChgMemo.onIframeLoadComplete = (function (modal) {
        return function () {
            eMemoDialogEditorObject = modal.getIframe().InitMemoUsrOpt(function () { }, 250);
        }
    })(oChgMemo);

    oChgMemo.show();
    oChgMemo.addButton(top._res_30, function () { oChgMemo.hide(); }, 'button-gray', null);
    oChgMemo.addButton(top._res_28, function () { oChgMemo.getIframe().nsAdminUsers.UpdateMemoPref(oChgMemo, nFileId, 1); }, 'button-green', null);

}


//Demande de changement de Signature
nsAdminUsers.chgSig = function (nFileId) {
    var oChgSig = new eModalDialog(top._res_7963, 0, "mgr/eUserInfosManager.ashx", 1000, 475, "oChgSig");
    oChgSig.addParam("iframeScrolling", "no", "post");
    oChgSig.addParam("fileid", nFileId, "post");
    oChgSig.addParam("action", USERS_INFO_ACTION.RENDER_POPUP_CHANGE_SIG, "post"); //


    var eMemoDialogEditorObject = null;
    oChgSig.onIframeLoadComplete = (function (modal) {
        return function () {

            eMemoDialogEditorObject = modal.getIframe().InitMemoUsrOpt(function () { }, 350);
        }
    })(oChgSig);


    oChgSig.show();
    oChgSig.addButton(top._res_30, function () { oChgSig.hide(); }, 'button-gray', null);  // annuler
    oChgSig.addButton(top._res_28, function () { oChgSig.getIframe().nsAdminUsers.UpdateMemoPref(oChgSig, nFileId, 2); }, 'button-green', null);  // valider


}


nsAdminUsers.UpdateMemoPref = function (oMod, nFileId, nType) {

    var myDoc = oMod.getIframe().document;

    var oMemoEditor = nsMain.getMemoEditor('edtBodySignMemoId', myDoc);
    var sContent = oMemoEditor.getData();


    if (nType === 2) {

        var optUpdater = new OptionUpdater({ "optiontype": 1, "option": 2, "optionvalue": 1 });

        //AutoSign        
        var bAutotoSign = getAttributeValue(myDoc.getElementById("auto-sign"), "chk")


        if (bAutotoSign == 0 || bAutotoSign == 1)
            optUpdater.addParam("autoaddsign", bAutotoSign);

        optUpdater.addParam("bodysign", sContent);
    }
    else {
        var optUpdater = new OptionUpdater({ "optiontype": 5, "option": 3, "optionvalue": sContent });
    }

    optUpdater.addParam("userid", nFileId);
    optUpdater.send(function (oRes) {

        var success = (getXmlTextNode(oRes.getElementsByTagName("success")[0]) == "1");
        if (success) {
            eAlert(4, '', top._res_1761, '', null, null,
                function () {
                    if (nType == 1) {
                        //Met à jour le message si id = user connext
                        var nCurrentUser = 0;
                        var oA = top.document.getElementById("UserAvatar");
                        if (oA) {
                            nCurrentUser = getAttributeValue(oA, "fid") + "";
                            if (nFileId == nCurrentUser + "") {
                                getParamWindow().RefreshUserMessage(sContent);
                            }
                        }
                    }
                    oMod.hide();
                });
        }
        else {
            eAlert(0, '', top._res_72, getXmlTextNode(oRes.getElementsByTagName("message")[0]));
        }


    });




}


//Demande de suppression de user
nsAdminUsers.onUserDelete = function (oModFile) {
    if (!oModFile)
        return;

    var upd = new eUpdater("eda/Mgr/eAdminUserManager.ashx", 1);
    upd.addParam("action", "0", "post"); // 0 = récupération d'informations
    upd.addParam("userId", oModFile.fileId, "post");
    upd.ErrorCallBack = function () { top.setWait(false); }

    top.setWait(true); // l'opération de vérification des fiches impactées peut prendre un peu de temps sur certaines bases

    upd.send(
        // Résultat
        function (oRes) {
            top.setWait(false);

            var res = JSON.parse(oRes);
            if (!res.Success) {
                eAlert(1, top._res_6237, res.Error); // Une erreur est survenue lors de la mise à jour
            }
            else {

                var sUserName = "";
                var eltUserName = oModFile.getIframe().document.querySelector("input[ename='COL_101000_101001']");
                if (eltUserName != null)
                    sUserName = eltUserName.value;
                var sDelMsg = top._res_7676; // Attention, cette opération est irréversible.<br>Les fiches affectées à cet utilisateur
                if (res.DelWarning != "")
                    sDelMsg += top._res_7677; // ", et listées ci-dessous,";
                sDelMsg += " " + top._res_7678; // " seront désormais publiques.";

                var nModalHeight = 150;
                nModalHeight += res.DelWarning.split("\n").length * 15;
                oModalDelUser = eAdvConfirm({
                    'criticity': 1,
                    'title': top._res_6536,
                    'message': sDelMsg,
                    'details': res.DelWarning,
                    'width': 600,
                    'height': nModalHeight,
                    'okFct': function () {
                        var updDel = new eUpdater("eda/Mgr/eAdminUserManager.ashx", 1);
                        updDel.addParam("action", "1", "post"); // 1 = suppression
                        updDel.addParam("userId", oModFile.fileId, "post");
                        updDel.addParam("userName", sUserName, "post");
                        //updDel.addParam("replacementUserId", 0, "post");  // TODO: interface pour choisir le UserID de remplacement
                        updDel.ErrorCallBack = function (oRes) { nsAdminUsers.afterUserDelete(oRes, oModalDelUser, oModFile, true); }

                        updDel.send(
                            // Résultat
                            function (oRes) {
                                nsAdminUsers.afterUserDelete(oRes, oModalDelUser, oModFile, false);
                            }
                        ); // updDel.send
                    },
                    'cancelFct': null,
                    'bOkGreen': false,
                    'bHtml': false,
                    'resOk': top._res_19,
                    'resCancel': top._res_30,
                    'cssOk': 'button-red'
                }); // eConfirm
            }
        }
    ); // upd.send
}

nsAdminUsers.afterGroupDelete = function (oRes, bFromErrorCallBack) {
    if (!bFromErrorCallBack) {
        var res = JSON.parse(oRes);
        if (!res.Success) {
            eAlert(1, top._res_6237, "", res.Error); // Une erreur est survenue lors de la mise à jour
        }
        else {
            //
        }
    }
    // Pas de retour JSON, mais un retour XML envoyé par l'ErrorCallBack d'eUpdater : le message d'erreur aura déjà été affiché par eUpdater
    // On ne l'affiche donc pas de nouveau ici

    oModalGroup.hide();

    nsAdmin.loadAdminModule(USROPT_MODULE_ADMIN_ACCESS_USERGROUPS, null, { tab: 101000 });
}


//Traitement de retour de la suppression de user
nsAdminUsers.afterUserDelete = function (oRes, oModalDelUser, oModalUser, bFromErrorCallBack) {
    if (!bFromErrorCallBack) {
        var res = JSON.parse(oRes);
        if (!res.Success) {
            var nModalHeight = 150;
            nModalHeight += res.Error.split("\n").length * 15;
            eAlert(1, top._res_6237, top._res_7674, res.Error, nModalHeight); // "Une erreur est survenue lors de la mise à jour :" - "La suppression des informations concernant cet utilisateur s'est terminée avec des erreurs :"
        }
        else {
            var nModalHeight = 150;
            nModalHeight += res.DelWarning.split("\n").length * 15;
            eAlert(4, top._res_1676, top._res_7675, res.DelWarning, 600, nModalHeight); // "Traitement terminé" - "Les informations concernant cet utilisateur ont été correctement supprimées."
        }
    }
    // Pas de retour JSON, mais un retour XML envoyé par l'ErrorCallBack d'eUpdater : le message d'erreur aura déjà été affiché par eUpdater
    // On ne l'affiche donc pas de nouveau ici

    oModalDelUser.hide();
    oModalUser.hide();

    nsAdmin.loadAdminModule(USROPT_MODULE_ADMIN_ACCESS_USERGROUPS, null, { tab: 101000 });
}



nsAdminUsers.eMemoDialogEditorObject = null;

nsAdminUsers.InitMemo = function init(modal) {

    var bHTML = true;
    var bCompactMode = false;
    eMemoDialogEditorObject = new eMemoEditor(
                                           "eMemoEditorValue",
                                            bHTML,
                                            document.getElementById('eExpressMessageMemoEditorContainer'),
                                            null,
                                             document.getElementById('eMemoEditorValue').value,
                                            bCompactMode,
                                            'eMemoDialogEditorObject'
                                       );
    eMemoDialogEditorObject.childDialog = modal;
    eMemoDialogEditorObject.title = top._res_383;

    //eMemoDialogEditorObject.descId = '4438';
    //eMemoDialogEditorObject.fileId = '45641';
    eMemoDialogEditorObject.inlineMode = false;
    eMemoDialogEditorObject.isFullScreen = false;
    eMemoDialogEditorObject.focusOnShow = false;
    //eMemoDialogEditorObject.preventCompactMode = true;
    eMemoDialogEditorObject.updateOnBlur = false;
    eMemoDialogEditorObject.readOnly = false;
    eMemoDialogEditorObject.editorType = '';
    eMemoDialogEditorObject.toolbarType = '';
    if (bHTML) {
        eMemoDialogEditorObject.setSkin('eudonet');
    }
    eMemoDialogEditorObject.config.width = '99%';
    eMemoDialogEditorObject.config.height = '100px';
    eMemoDialogEditorObject.show();

    eMemoDialogEditorObject.setToolBarDisplay(true, true);

    return eMemoDialogEditorObject;
}