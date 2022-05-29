// Fichier JS utilisé pour l'ajout d'annexes
// 08/2012
// NBA

// cablé sur l'evenement 'onclick' des boutons radio
function ChangeType(strValue) {
    if (document.getElementById('radPJFile') && document.getElementById('radPJLink')) {
        // On désactive le lien si c'est un fichier 
        if (strValue == '0') {
            document.getElementById('radPJFile').checked = true;
            DisableElement('uploadvalue');
            EnableElement('FileToUpload');
        }
        if (strValue == '1') {
            document.getElementById('radPJLink').checked = true;
            EnableElement('uploadvalue');
            DisableElement('FileToUpload');
        }
    }
}



//Désactive ou active un élément grace à son ID
function DisableElement(_idElt) {
    document.getElementById(_idElt).disabled = true;
}
//Active un élément grace à son ID
function EnableElement(_idElt) {
    document.getElementById(_idElt).disabled = false;
}

//Met à jour le style display d'un élément
function UpdateDisplay(_idElt, _style) {
    document.getElementById(_idElt).style.display = _style;
}



/*KHA le 29/04/2014:  on gère également les champs de type fichier*/
var bMultiple = false;
function getSelectedFiles() {

    var divList = document.getElementById("divLstFiles");
    if (!divList)
        return;

    var lstSelectedDiv = divList.querySelectorAll("div[sel='1']");
    var aSelectedFile = new Array();

    for (var i = 0; i < lstSelectedDiv.length; i++) {
        aSelectedFile.push(eTrim(GetText(lstSelectedDiv[i])));
    }

    return aSelectedFile;

}

function getAllFiles() {
    var divList = document.getElementById("divLstFiles");
    if (!divList)
        return;
    var aFiles = new Array();

    for (var i = 0; i < divList.children.length; i++) {
        aFiles.push(eTrim(GetText(divList.children[i])));
    }

    return aFiles;
}


function selFileAdv(divObj) {
    var bDeselect = getAttributeValue(divObj, "sel") == 1;

    if (!bDeselect && !bMultiple) {
        var divList = document.getElementById("divLstFiles");
        if (!divList)
            return;

        var lstSelectedDiv = divList.querySelectorAll("div[sel='1']");

        for (var i = 0; i < lstSelectedDiv.length; i++) {
            selFile(lstSelectedDiv[i]);
        }
    }


    selFile(divObj);

}

function selFile(divObj) {
    var bDeselect = getAttributeValue(divObj, "sel") == 1;
    divObj.setAttribute("sel", bDeselect ? "0" : "1");
    var inptFiles = document.getElementById("files");
    if (inptFiles)
        inptFiles.value = getSelectedFiles().join(';');

    if (getAttributeValue(divObj, "sel") == "1")
        addClass(divObj, "eSel");
    else
        removeClass(divObj, "eSel");
}


function getPhyFileUpdater() {
    var oPhyFileUpdater = new eUpdater("mgr/eFieldFilesManager.ashx", null);
    oPhyFileUpdater.addParam("folder", document.getElementById("folder").value, "post");
    return oPhyFileUpdater;
}

function delFile(idx) {
    var oPhyFileUpdater = getPhyFileUpdater();
    var sFileName;
    var brow = new getBrowser();
    var divList = document.getElementById("divLstFiles");
    if (brow.isIE8) {
        if (!divList)
            return;

        var lstDiv = divList.children;
        for (var i = 0; i < lstDiv.length; i++) {
            if (getAttributeValue(lstDiv[i], "idx") == idx) {
                sFileName = eTrim(GetText(lstDiv[i]));
                break;
            }
        }
    }
    else {
        var divLine = divList.querySelector("div[idx='" + idx + "']");
        if (divLine)
            sFileName = GetText(divLine, true).trim();
    }

    oPhyFileUpdater.addParam("action", "del", "post");
    oPhyFileUpdater.addParam("file", sFileName, "post");

    oPhyFileUpdater.send(delFileReturn);


}

function delFileReturn(oRes) {
    //console.log(oRes);
    var divElementValue = "";
    if (getXmlTextNode(oRes.getElementsByTagName("success")[0]) != "1")
        return;

    var sDeletedFile = eTrim(getXmlTextNode(oRes.getElementsByTagName("deletedfile")[0]));

    var divList = document.getElementById("divLstFiles");
    if (!divList)
        return;

    var lstDiv = divList.children;


    for (var i = 0; i < lstDiv.length; i++) {
        if (lstDiv[i].getAttribute("ref")) {
            lstDiv[i].setAttribute("ref", getXmlTextNode(oRes.getElementsByTagName("refresh")[0]));
            break;
        }
    }


    for (var i = 0; i < lstDiv.length; i++) {

        if (lstDiv[i].firstElementChild != null && typeof (lstDiv[i].firstElementChild) != "undefined")
            divElementValue = eTrim(GetText(lstDiv[i].firstElementChild, true));
        else
            divElementValue = eTrim(GetText(lstDiv[i]));

        if (divElementValue == sDeletedFile) {
            divList.removeChild(lstDiv[i]);
            break;
        }
    }

}

var oFileNameEditor;
var oPopupObj;
function initFileNameEditor() {
    oPopupObj = new ePopup('oPopupObj', 220, 250, 0, 0, document.body, false);
    oFileNameEditor = new eFieldEditor('inlineEditor', oPopupObj, 'oFileNameEditor');
    oFileNameEditor.action = 'renameFile';
}

function renameFile(objEditor) {

    var inptEditor = document.getElementById('eInlineEditor');
    if (!inptEditor)
        return;

    var sFileName = eTrim(objEditor.value);
    var sNewFileName = eTrim(inptEditor.value);

    if (sFileName == "" || sNewFileName == "")
        return;


    var inptMult = document.getElementById("mult");
    var sMult = "0";
    if (inptMult)
        sMult = inptMult.value;

    var oPhyFileUpdater = getPhyFileUpdater();
    oPhyFileUpdater.addParam("action", "ren", "post");
    oPhyFileUpdater.addParam("file", sFileName, "post");
    oPhyFileUpdater.addParam("newname", sNewFileName, "post");
    oPhyFileUpdater.addParam("files", getSelectedFiles().join(";"), "post");
    oPhyFileUpdater.addParam("idx", getAttributeValue(objEditor.sourceElement, "idx"), "post");
    oPhyFileUpdater.addParam("mult", sMult, "post");

    oPhyFileUpdater.send(renameFileReturn);
}

function renameFileReturn(oRes) {
    //console.log(oRes);
    if (getXmlTextNode(oRes.getElementsByTagName("success")[0]) != "1")
        return;

    var idx = getAttributeValue(oFileNameEditor.sourceElement, "idx");
    oFileNameEditor.parentPopup.hide();

    var divList = document.getElementById("divLstFiles");
    if (!divList)
        return;

    var sNewFileName = eTrim(GetText(oRes.getElementsByTagName("renamedfile")[0].childNodes[0]));
    SetText(oFileNameEditor.sourceElement, sNewFileName);

    addClass(divList.children[idx], "eFieldEditorEdited");
    setTimeout(function () { removeClass(divList.children[idx], "eFieldEditorEdited"); }, 500);
}

function openFile(oContent) {
    var sFolder = document.getElementById("folder").value;
    var sFileName = eTrim(GetText(oContent));

    window.open("ePjDisplay.aspx?folder=" + sFolder + "&file=" + sFileName + "&dispFrom=fieldfiles");

}

function selPj(obj) {
    updateSelectedPJ(getSelectedPj());
}


function selectAllPj(obj) {
    var tablePJ = document.getElementById("mt_102000");
    var lstSelectedObj = tablePJ.querySelectorAll("a[chk]");
    var sNewValue = getAttributeValue(obj, "chk") == "1" ? "1" : "0";
    for (var i = 0; i < lstSelectedObj.length; i++) {
        var chk = lstSelectedObj[i];
        if (chk == obj)
            continue;
        if (getAttributeValue(chk, "chk") != sNewValue)
            chgChk(chk);
    }

    updateSelectedPJ(getSelectedPj());


}

function getSelectedPj() {

    var tablePJ = document.getElementById("mt_102000");
    var lstSelectedObj;
    var aSelectedFile = new Array();
    try {
        lstSelectedObj = tablePJ.querySelectorAll("a[chk='1']");
        for (var i = 0; i < lstSelectedObj.length; i++) {
            if (lstSelectedObj[i].id == "chkAll_102000")
                continue;

            var tr = lstSelectedObj[i].parentElement.parentElement;
            var eid = getAttributeValue(tr, "eid");
            var pjid = getNumber(eid.split("_")[1]);
            aSelectedFile.push(pjid);
        }


        var nChkBox = tablePJ.querySelectorAll("a[chk]").length - 1; // on ne compte pas la check box d'entete
        var nSelChk = lstSelectedObj.length;
        var oChkGlobal = document.getElementById("chkAll_102000");
        if (getAttributeValue(oChkGlobal, "chk") == "1")
            nSelChk--;

        if (nChkBox == nSelChk && nSelChk > 0) {
            if (getAttributeValue(oChkGlobal, "chk") == "0")
                chgChk(oChkGlobal);
        }
        else if (nChkBox != nSelChk || nSelChk == 0) {
            if (getAttributeValue(oChkGlobal, "chk") == "1")
                chgChk(oChkGlobal);

        }
    }
    catch (e) {

    }


    return aSelectedFile;

}

function updateSelectedPJ(aSelectedFile) {
    var inptFiles = document.getElementById("idspj");

    if (inptFiles)
        inptFiles.value = aSelectedFile.join(';');

    var nbPj = document.getElementById("nbpj");

    if (nbPj)
        nbPj.value = aSelectedFile.length;

}

function changePjView(sParam) {
    var inpt = document.getElementById("viewtype");
    var form = document.getElementById("frmUpload");
    if (!inpt || !form)
        return;

    inpt.value = sParam;

    top.setWait(true);

    form.submit();


}