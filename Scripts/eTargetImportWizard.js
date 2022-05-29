///Cette fonction permet de parser les données à importer
///et de créer les colonnes et la première ligne

// nbLines permet de specifier le nombre maximum de lignes à découper Mettre -1 si on veux toute les lignes
function splitCsvContent(sDatas, sSeparator, sQualifier, bHeaderIncols, nbMaxLines) {
    var idx = 0;
    var readChar = '';
    var readNextChar = '';
    var inQuote = false;

    var resultValue = '';
    var resultTabGlobal = new Array();
    var resultTabLine = new Array();
    var resultTabIdxLine = 0;
    var resultTabIdxColumn = 0;

    for (var idx = 0, len = sDatas.length; idx < len; idx++) {
        // Caractère courant et son suivant
        readChar = sDatas[idx];
        readNextChar = '';
        if (idx < sDatas.length - 1)
            readNextChar = sDatas[idx + 1];

        if (readChar == '\n' || (readChar == '\r' && readNextChar == '\n')) {
            // If it's a \r\n combo consume the \n part and throw it away.
            if (readChar == '\r')
                idx++;

            if (inQuote) {
                if (readChar == '\r')
                    resultValue += '\r';
                resultValue += '\n';
            }
            else {
                // Si la ligne n'est pas vide, on passe à la suivante
                if (resultTabIdxColumn > 0 || resultValue.length > 0) {
                    resultTabLine[resultTabIdxColumn] = resultValue;
                    resultTabGlobal[resultTabIdxLine] = resultTabLine;

                    resultTabIdxLine++;
                    resultValue = '';
                    resultTabIdxColumn = 0;
                    resultTabLine = new Array();

                    if (resultTabIdxLine == nbMaxLines)
                        break;
                }
            }
        }
        else if (resultValue.length == 0 && !inQuote) {
            if (readChar == sQualifier)
                inQuote = true;
            else if (readChar == sSeparator) {
                resultTabLine[resultTabIdxColumn] = resultValue;
                resultTabIdxColumn++;
                resultValue = '';
            }
            else if (/\s/.test(readChar)) {
                // TODO - Trouver pour le IF l'equivalent de la fonction char.IsWhiteSpace de .NET en JS
                // Ignore leading whitespace
            }
            else
                resultValue += readChar;
        }
        else if (readChar == sSeparator) {
            if (inQuote)
                resultValue += readChar;
            else {
                resultTabLine[resultTabIdxColumn] = resultValue;
                resultTabIdxColumn++;
                resultValue = '';
            }
        }
        else if (readChar == sQualifier) {
            if (inQuote) {
                if (readNextChar == sQualifier) {
                    idx++;
                    resultValue += readChar;
                }
                else
                    inQuote = false;
            }
            else
                resultValue += readChar;
        }
        else
            resultValue += readChar;
    }

    // On recup la derniere valeur du fichier si elle n'a pas encore été traité
    // Si la ligne n'est pas vide, on passe à la suivante
    if (resultTabIdxColumn > 0 || resultValue.length > 0) {
        resultTabLine[resultTabIdxColumn] = resultValue;
        resultTabGlobal[resultTabIdxLine] = resultTabLine;
    }

    return resultTabGlobal
}

function parseDatasText(sDatas, sSeparator, sQualifier, bHeaderIncols) {

    // on a besoin que de la premiere ligne de données
    var stopLine = bHeaderIncols ? 2 : 1;

    var resultTabGlobal = splitCsvContent(sDatas, sSeparator, sQualifier, bHeaderIncols, stopLine);

    if (resultTabGlobal.length <= 0)
        return;

    var idxLine = 0;
    var aHeaderCol = new Array();
    var divReturn = document.createElement("div");

    for (var idxCol = 0, len = resultTabGlobal[idxLine].length; idxCol < len; idxCol++) {
        if (!bHeaderIncols)
            aHeaderCol[idxCol] = "Col#" + idxCol;
        else if (resultTabGlobal[idxLine][idxCol] == "")
            aHeaderCol[idxCol] = "Col#" + idxCol;
        else
            aHeaderCol[idxCol] = resultTabGlobal[idxLine][idxCol];
    }

    if (bHeaderIncols)
        idxLine++;

    for (var idxCol = 0; idxCol < aHeaderCol.length; idxCol++) {
        var divColGlobal = document.createElement("div");
        divColGlobal.className = "fieldItem";
        var divCol = document.createElement("div");
        divCol.className = "opacityField";
        divColGlobal.appendChild(divCol);

        var divDatas = document.createElement("div");
        divDatas.className = "subFieldHead";
        divDatas.innerHTML = "<span>" + aHeaderCol[idxCol] + "</span>";
        divColGlobal.appendChild(divDatas);

        //Propriétés
        divDatas.setAttribute("onmousedown", "MDN(this.id,event);");
        divDatas.setAttribute("onmouseover", "MOV(this.id,event);");
        divDatas.setAttribute("onmouseout", "MOT(this.id,event);");
        divDatas.setAttribute("ednkey", "0");
        divDatas.setAttribute("ednorig", "Source");
        divDatas.id = "s_" + idxCol;

        //Fin props
        var divBottom = document.createElement("div");
        divBottom.className = "subFieldBottom";
        if (resultTabGlobal[idxLine].length <= idxCol || resultTabGlobal[idxLine][idxCol] == "") {
            divBottom.innerHTML = "&nbsp;";
        }
        else {
            divBottom.innerHTML = removeHTML(resultTabGlobal[idxLine][idxCol]);
        }
        divColGlobal.appendChild(divBottom);

        divReturn.appendChild(divColGlobal);
    }

    return divReturn;
}

//Créer la source de données après le retour de l'import
function createSrcDatasTab(sDatas) {

    var sSeparator = document.getElementById("CustomFieldSeparator").value.replace("<tab>", "\t");
    var sQualifier = document.getElementById("TextIdentifier").value;
    var bHeaderIncols = false;
    if (document.getElementById("ColHeaders").checked)
        bHeaderIncols = true;

    document.getElementById("TabSrc").innerHTML = "";
    document.getElementById("TabSrc").appendChild(parseDatasText(sDatas, sSeparator, sQualifier, bHeaderIncols));

}
///****************************************************
///Déplacement Et mapping
///****************************************************


function SetMapOff(nDescId) {
    var oTds = document.getElementsByTagName('div');

    for (i = 0; i < oTds.length; i++) {
        if (oTds[i].getAttribute('ednDescId') == nDescId) {
            if (oTds[i].getAttribute('ednorig') == "Target") {

                oTds[i].style.backgroundColor = 'DFDFDE';
                removeClass(oTds[i], "ednMappedTgt");

            }
            else {

                oTds[i].setAttribute('ednDescId', 0);
                oTds[i].setAttribute('ednLbl', 0);
                oTds[i].style.backgroundColor = 'DFDFDE';
                removeClass(oTds[i], "ednMappedOrg");
            }
        }
    }
}


var z = 0;


function KeyChange(sId) {

    var oTdKey = document.getElementById(sId);
    var oTdKeyDescId = document.getElementById(sId.replace('tl_', 't_'));

    var nKey = oTdKey.getAttribute('ednKey');
    var nKeyDescId = oTdKeyDescId.getAttribute('ednDescId');

    if (nKey && nKey == '1') {
        oTdKey.setAttribute('ednKey', '0');
        oTdKeyDescId.setAttribute('ednKey', '0');
        oTdKey.className = oTdKey.className.replace("ednKey", "");
        SetKey(nKeyDescId, 0);
    }
    else {
        oTdKey.setAttribute('ednKey', '1');
        oTdKeyDescId.setAttribute('ednKey', '1');
        oTdKey.style.textDecoration = 'underline';
        oTdKey.className += ' ednKey';
        SetKey(nKeyDescId, 1);

    }

}

function docMouseUp() {
    if (document.getElementById("cell").style.display == "block")
        document.getElementById("cell").style.display = "none";
    document.onmousedown = oldMouseDown;
}

function docMouseMove(event) {

    var oCell = document.getElementById("cell");
    if (oCell.style.display == "block") {
        var e = event || window.event;
        var scroll = new Array((document.documentElement && document.documentElement.scrollLeft) || window.pageXOffset || self.pageXOffset || document.body.scrollLeft, (document.documentElement && document.documentElement.scrollTop) || window.pageYOffset || self.pageYOffset || document.body.scrollTop);
        var pos = new Array(e.clientX + scroll[0] - document.body.clientLeft, e.clientY + scroll[1] - document.body.clientTop);


        oCell.style.left = (parseInt(pos[0])) + "px";
        oCell.style.top = (parseInt(pos[1]) - 20) + "px";
    }
}

function SetKey(nDescId, nValue) {
    var oTds = document.getElementsByTagName('div');

    for (i = 0; i <= oTds.length - 1; i++) {
        if (oTds[i].getAttribute('ednDescId') == nDescId)
            oTds[i].setAttribute('ednKey', nValue);
    }
}

var oldMouseDown;
function MDN(sId, e) {
    oldMouseDown = document.onmousedown;
    document.onmousedown = function () { return false }
    document.getElementById("sIdFrom").value = sId;
    var oFrom = document.getElementById(document.getElementById("sIdFrom").value);
    var nDescId = oFrom.getAttribute('ednDescId');

    if (nDescId == 0 || nDescId == null) {
        var oCell = document.getElementById('cell');

        oCell.innerHTML = oFrom.innerHTML;
        oCell.style.width = oFrom.style.width;
        oCell.style.height = oFrom.style.height;

        if (!e)
            var e = window.event;
        var scroll = new Array((document.documentElement && document.documentElement.scrollLeft) || window.pageXOffset || self.pageXOffset || document.body.scrollLeft, (document.documentElement && document.documentElement.scrollTop) || window.pageYOffset || self.pageYOffset || document.body.scrollTop);
        var pos = new Array(e.clientX + scroll[0] - document.body.clientLeft, e.clientY + scroll[1] - document.body.clientTop);

        oCell.style.left = (parseInt(pos[0])) + "px";
        oCell.style.top = (parseInt(pos[1]) - 20) + "px";

        oCell.style.display = "block";
        oCell.style.backgroundColor = '#BCDCFB';



        z = 1;
        document.onmousemove = docMouseMove;
        document.onmouseup = docMouseUp;
        document.onselectionstart = "return false;";
    }
    else {
        if (document.getElementById("sIdFrom").value.indexOf('s_') > -1) {
            top.eAlert(3, top._res_1672, top._res_1672);
        }
        document.getElementById("sIdFrom").value = '';
    }
}

function MOV(sId) {
    document.getElementById("sIdTo").value = sId;
    var oTo = document.getElementById(document.getElementById("sIdTo").value);
    if (sId.indexOf('s_') > -1 || document.getElementById("sIdFrom").value != '') {
        document.body.style.cursor = 'move';
        if (document.getElementById("sIdFrom").value != '' && oTo.innerHTML.replace('&nbsp;', '') == '&nbsp;')
            oTo.style.backgroundColor = 'DFDFDE';
    }
    else {
        if (sId.indexOf('t_') > -1 && document.getElementById("sIdFrom").value == '' && oTo.innerHTML.replace('&nbsp;', '') != '&nbsp;') {
            document.body.style.cursor = 'hand';
        }
    }
}

function MOT(sId) {
    document.body.style.cursor = '';
    /*var oEl = document.getElementById(sId);
    oEl.style.border = '1px solid #BB1515';
    if (oEl.innerHTML.replace('&nbsp;', '') == '&nbsp;')
    oEl.style.backgroundColor = 'FFFFFF';*/
}

function MUP(id) {
    if (document.getElementById("sIdFrom").value == "" || document.getElementById("sIdTo").value == "")
        return;
    if (document.getElementById("sIdFrom").value != document.getElementById("sIdTo").value) {

        var oFrom = document.getElementById(document.getElementById("sIdFrom").value);
        var oTo = document.getElementById(document.getElementById("sIdTo").value);

        sFromOrig = oFrom.getAttribute('ednOrig');
        sToOrig = oTo.getAttribute('ednOrig');

        nDescId = oTo.getAttribute('ednDescId');

        var nKey = oTo.getAttribute('ednKey');
        var sLbl = document.getElementById(oTo.id.replace("t_", "tl_")).innerText || document.getElementById(oTo.id.replace("t_", "tl_")).textContent;

        if (sFromOrig != sToOrig || (sFromOrig == sToOrig && sToOrig == 'Target')) {
            var sToHTML = oTo.innerHTML;
            var sFromHTML = oFrom.innerHTML;

            oTo.innerHTML = sFromHTML;
            oFrom.style.backgroundColor = '82F888';

            oFrom.setAttribute('ednDescId', nDescId);
            oFrom.setAttribute('ednLbl', sLbl);
            oFrom.setAttribute('ednKey', nKey);
            addClass(oFrom, "ednMappedOrg");
            addClass(oTo, "ednMappedTgt");

            document.getElementById("sIdTo").value = '';
            document.getElementById("sIdFrom").value = '';

            var oCell = document.getElementById('cell');
            oCell.style.display = 'none';
            document.body.style.cursor = '';

            z = 0;
        }
        else {
            top.eAlert(2, top._res_1674, top._res_1674);
        }
    }
    document.getElementById("sIdTo").value = '';
    document.getElementById("sIdFrom").value = '';
}

function MapOff(sId) {
    var oTo = document.getElementById(sId);
    oTo.innerHTML = '&nbsp;';
    var nDescId = oTo.getAttribute('ednDescId');
    SetMapOff(nDescId);
    document.body.style.cursor = '';
    document.getElementById("sIdTo").value = '';
    document.getElementById("sIdFrom").value = '';
}


///****************************************************
///End Mapping
///****************************************************


//Gestion des étapes
var nCurrentStep = 1;
var oUpdater = null;

function validImportOptions() {
    var oTabSrc = document.getElementById('TabSrc');
    var oColumns = oTabSrc.querySelectorAll("div[ednDescId]");

    var strId = '';
    var strDescId = '';
    var strLabels = '';
    var strKey = '';

    for (i = 0; i <= oColumns.length - 1; i++) {
        if (oColumns[i].getAttribute('ednDescId').value == "")
            continue;

        var nDescId = oColumns[i].getAttribute('ednDescId');
        var sFileLabel = oColumns[i].getAttribute('ednLbl');
        var nId = oColumns[i].getAttribute('id');
        var nKey = oColumns[i].getAttribute('ednKey');
        if (nKey == null)
            nKey = "";

        if (nDescId && nDescId != '') {
            if (strId == '') {
                strId = nId.replace('s_', '');
                strDescId = nDescId;
                strKey = nKey;
                strLabels = sFileLabel;
            }
            else {
                strId = strId + ';' + nId.replace('s_', '');
                strDescId = strDescId + ';' + nDescId;
                strKey = strKey + ';' + nKey;
                strLabels = strLabels + ';|;' + sFileLabel;
            }
        }
    }

    if (strId == undefined || strId == null || strId == '') {
        top.eAlert(2, top._res_1673, top._res_1673);
        return false;
    }

    //Post du formulaire
    var url = "mgr/eTargetProcessManager.ashx";
    oUpdater = new eUpdater(url, 0);

    oUpdater.addParam("tab", document.getElementById("tab").value, "post");
    oUpdater.addParam("tabfrom", document.getElementById("tabfrom").value, "post");
    oUpdater.addParam("evtid", document.getElementById("evtid").value, "post");
    oUpdater.addParam("filecontent", document.getElementById("DatasClipboard").value, "post");
    oUpdater.addParam("separator", document.getElementById("CustomFieldSeparator").value, "post");
    oUpdater.addParam("identificator", document.getElementById("TextIdentifier").value, "post");
    oUpdater.addParam("colheader", document.getElementById("ColHeaders").checked ? "1" : "0", "post");
    oUpdater.addParam("descidlist", strDescId, "post");
    oUpdater.addParam("labellist", strLabels, "post");
    oUpdater.addParam("idlist", strId, "post");
    oUpdater.addParam("keylist", strKey, "post");
    oUpdater.addParam("action", "1", "post");

    //CallBack d'erreur : setwait a false (passé a true lors du send de l'eupdater)
    oUpdater.ErrorCallBack = (function (obj) { return function () { processImportResultError(obj); } })(oUpdater);

    //Source fichier ou texte
    var radios = document.getElementsByName('DataSource');
    var valChk = 0;
    for (var i = 0, length = radios.length; i < length; i++) {
        if (radios[i].checked) {
            valChk = radios[i].value;
        }
    }

    //Depuis un fichier
    if (valChk == "0")
        oUpdater.addParam("fromfile", "1", "post");

    // TODO - Pkoi en commentaire ?
    //setWait(true, oUpdater);
    oUpdater.send(processImportResult);

    //todo progress bar :
    oModalDialog = showWaitDialog(top._res_307, top._res_1675, 550, 160);
   
    return true;
}

var progressBar = null;
function ShowProgress() {
    progressBar = new eModalDialog(top._res_307, 4, top._res_1675, 550, 160);  
    progressBar.hideMaximizeButton = true;   
    progressBar.hideCloseButton = true;
    progressBar.show();
    progressBar.onHideFunction = function () { progressBar = null; };

    UpdateProgress();
}
function UpdateProgress() {    
    var url = "mgr/eTargetProgressManager.ashx";
    oUpdater = new eUpdater(url, 0);
    oUpdater.addParam("action", "3", "post");
    oUpdater.send(function (oRes) {

        var percent = getXmlTextNode(oRes.getElementsByTagName("Percent")[0]) * 1;
        var TotalLine = getXmlTextNode(oRes.getElementsByTagName("TotalLine")[0]) * 1;
        var TotalSuccessLine = getXmlTextNode(oRes.getElementsByTagName("TotalSuccessLine")[0]) * 1;
              
        console.log("TotalSuccessLine/TotalLine :" + TotalSuccessLine + "/" + TotalLine);      

        if (percent >= 0 && percent < 100 && progressBar != null)
            progressBar.updateProgressBar(percent);

        setTimeout(UpdateProgress, 5000);
    });
}

var oModalDialog = null;

function processImportResultError(oUpdater) {
    try {

        // TODO - Hide ?

    }
    finally {
        setWait(false, oUpdater);
    }
}

///Traite le retour de l'import
//Et crée le rapport
function processImportResult(oRes) {

    try {
        //fin de la progress bar
        if (oModalDialog != null) {
            oModalDialog.hide();
            oModalDialog = null;
        }
        var divRsult = document.getElementById("step3");

        var liNbFiles = document.getElementById("trgNbFiles");
        var liSize = document.getElementById("trgSize");
        var liNbCreated = document.getElementById("trgCreated");
        var liNotCreated = document.getElementById("trgNotCreated");

        var liMsg = document.getElementById("trgMsg");

        liNbFiles.innerHTML = top._res_437 + " : " + getXmlTextNode(oRes.getElementsByTagName("GlobalCount")[0]);
        liSize.innerHTML = top._res_639 + " : " + getXmlTextNode(oRes.getElementsByTagName("GlobalSize")[0]);
        liNbCreated.innerHTML = top._res_874.replace("<ITEM>", oRes.getElementsByTagName("ValidLines")[0].getAttribute("CountLinks"));

        var aErrLines = oRes.getElementsByTagName("ErrorLines")[0];
        var infoError = document.getElementById("infoError");
        if (aErrLines != null && aErrLines.getAttribute("CountLinks") != "" && aErrLines.getAttribute("CountLinks") != "0") {
            infoError.style.display = "";
            liNotCreated.innerHTML = top._res_875.replace("<ITEM>", aErrLines.getAttribute("CountLinks"));

            try {
                var text = GetText(aErrLines);
                liMsg.innerHTML = text.replace(/\n/g, "<br />").replace(/\t/g, "&nbsp;");
            }
            catch (ex) { }
        }
        else {
            infoError.style.display = "none";
            liNotCreated.innerHTML = top._res_875.replace("<ITEM>", 0);
            document.getElementById("infoError").style.display = "none";
            addClass(trgNotCreated, '4emeb');
        }

        //Retrouver les boutons suivant - précedent
        var btnNext = parent.top.document.getElementById("ImportTargetsNext");
        var btnPrev = parent.top.document.getElementById("ImportTargetsPrev");

        if (nCurrentStep == 3) {
            // #39516 CRU : Affichage correct du bouton "Terminer"
            try {
                // Transformation du bouton "Suivant" en "Terminer"
                btnNext.className = "button-green";
                var btnNextDiv = btnNext.getElementsByTagName("div");
                for (var i = 0; i < btnNextDiv.length; i++) {
                    if (i == 1)
                        btnNextDiv[i].innerHTML = top._res_170;
                    btnNextDiv[i].className = btnNextDiv[i].className.replace("rightarrow-", "");
                }

                btnPrev.style.display = "none";
            }
            catch (e) {
                alert("Buttons not displayed properly");
            }
        }

        oUpdater = null;
    }
    finally {
        setWait(false);
    }
}


function createHtmlElement(name, id, css) {

    var oElm = document.createElement(name);
    oElm.id = id;
    oElm.className = css;
    return oElm;
}


function StepClick(nStep, nTab) {

    //Affiche le div en questioon
    switch (nStep) {
        case 1:
            setDatasSrc();
            break;
        case 2:
            //disableSelection(document.getElementById("step2"));
            if (nCurrentStep == 1) {

                var radios = document.getElementsByName('DataSource');
                var valChk = 0;
                for (var i = 0, length = radios.length; i < length; i++) {
                    if (radios[i].checked) {
                        valChk = radios[i].value;
                    }
                }

                if (valChk == "0") {
                    var frm = document.getElementById("iframeupload");
                    var doc = (frm.contentWindow || frm.contentDocument);
                    var sDatas = "";
                    if (doc.document.getElementById("result") != null)
                        sDatas = doc.document.getElementById("result").innerHTML;
                }
                else {
                    var sDatas = document.getElementById("DatasClipboard").value
                }

                while (sDatas.indexOf("<br>") >= 0) {
                    sDatas = sDatas.replace("<br>", "\n");
                }
                if (eTrim(sDatas) == "")
                    return;

                // MCR 38463 caractere & (ampersand ) est encode en html en : &amp;  et pose probleme pour le rendu en cible etendu
                // http://www.degraeve.com/reference/specialcharacters.php

                while (sDatas.indexOf("&amp;") >= 0) {
                    sDatas = sDatas.replace("&amp;", "&");
                }
                createSrcDatasTab(sDatas);
            }
            else
                return;


            break;
        case 3:
            if (nCurrentStep == 1)
                return;
            if (nCurrentStep == 2) {
                if (!validImportOptions())
                    return;
            }
            break;
        case 4:
            top.loadBkm(nTab);
            top.modalBkmCol.hide();
            break;
        default:
            break;
    }

    for (var i = 1; i <= 3; i++) {
        if (i == nStep) {

            document.getElementById("step_" + i).className = "state_grp-current";
            document.getElementById("step" + i).style.display = "block";
        }
        else {
            document.getElementById("step_" + i).className = "state_grp";
            document.getElementById("step" + i).style.display = "none";
        }
    }

    nCurrentStep = nStep;
}


function disableSelection(target) {
    if (typeof target.onselectstart != "undefined") //IE route
        target.onselectstart = function () { return false }
    else if (typeof target.style.MozUserSelect != "undefined") //Firefox route
        target.style.MozUserSelect = "none"
    else
        target.onmousedown = function () { return false }
    target.style.cursor = "default"
}

function setDatasSrc() {
    var radios = document.getElementsByName('DataSource');
    var valChk = 0;
    for (var i = 0, length = radios.length; i < length; i++) {
        if (radios[i].checked) {
            valChk = radios[i].value;
        }
    }

    if (parseInt(valChk) == 0)
        document.getElementById("DatasClipboard").style.display = "none";
    else
        document.getElementById("DatasClipboard").style.display = "block";
}


function doUpload() {
    setWait(true);
    var frm = document.getElementById("frmupload");
    frm.submit();
    //ELAIZ - demande 76 825 - ajout du nom fichier chargé sur eudonet x
    var paramWin = top.getParamWindow();
    var objThm = JSON.parse(paramWin.GetParam("currenttheme").toString());
    if (objThm.Version > 1 && document.querySelector('#filesrc').files.length > 0)
        document.querySelector('#avatarName').innerText = document.querySelector('#filesrc').files[0].name
}

function setSeparator() {

    var selValue = document.getElementById("FieldSeparator").options[document.getElementById("FieldSeparator").selectedIndex].value;
    if (selValue == "*")
        document.getElementById("CustomSeparatorDiv").style.display = "inline-block";
    else
        document.getElementById("CustomSeparatorDiv").style.display = "none";
    document.getElementById("CustomFieldSeparator").value = selValue;

}
//Fin gestion des étapes

function doPrevious(nTab) {
    if (nCurrentStep != 1)
        StepClick(Number(nCurrentStep) - 1, nTab);
}


function doNext(nTab) {
    if (nCurrentStep != 4)
        StepClick(Number(nCurrentStep) + 1, nTab);

}
