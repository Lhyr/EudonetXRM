var aStatsCharts = new Array();

function loadStatsCharts() {
    var oDivDataToChart = document.getElementById("DataToChart");

    if (!oDivDataToChart || !oDivDataToChart.children.length || !oDivDataToChart.children.length > 0)
        return;

    for (var i = 0; i < oDivDataToChart.children.length; i++) {
        var oEntry = oDivDataToChart.children[i];
        var chartType = getAttributeValue(oEntry, "ChartFmt");
        var targetEltId = getAttributeValue(oEntry, "DivChartId");
        var suffix = getAttributeValue(oEntry, "Suffix");
        var xmlData = oEntry.value;
        loadStatsChart(chartType, suffix, xmlData, targetEltId);
        displayDetails(suffix);

    }

}

function loadStatsChart(chartType, suffix, xmlData, targetEltId) {

    if (typeof (displaySyncFusionChart) == 'function') {
        var parser = new DOMParser();
        var xmlDoc = parser.parseFromString(xmlData, "application/xml"); //important to use "text/xml"
        displaySyncFusionChart(xmlDoc, null, false, targetEltId, chartType);
        return;
    }
}

function displayDetails(suffix) {
    if (!suffix)
        suffix = "";
    var oBtn = document.getElementById("StatsRend" + suffix + "dispDtl");
    var oDivDetail = document.getElementById("StatsRend" + suffix + "Dtl");


    if (!oBtn || !oDivDetail)
        return;

    var icon = oBtn.firstChild;
    var labelSep = oBtn.querySelector(".sepTitle");

    if (!icon || !labelSep)
        return;

    var bDisplayed = (oBtn.getAttribute("ednisdisplayed") == "1");
    if (bDisplayed) {
        oDivDetail.style.display = "none";
        SetElmText(labelSep, top._res_6271);
        oBtn.setAttribute("ednisdisplayed", "0");
        icon.className = "icon-develop";

    }
    else {
        oDivDetail.style.display = "block";
        SetElmText(labelSep, top._res_6272);
        oBtn.setAttribute("ednisdisplayed", "1");
        icon.className = "icon-unvelop";
    }


}
function SetElmText(oElm, sContent) {
    if (oElm.innerText)
        oElm.innerText = sContent;
    if (oElm.textContent)
        oElm.textContent = sContent;
    else
        oElm.innerHTML = sContent;
}
function doSort(sort) {

    if (document.getElementById("countHead").getAttribute("ednSort") == sort)
        return;


    var oTbDetails = document.getElementById("TbDetails");
    var oTbTmp = document.createElement("Table");

    //deplacement des lignes  dans un tableau temporaire en inversant le sens
    while (oTbDetails.rows.length > 2) {
        var i = 1;
        var newRow = oTbTmp.insertRow(-1);
        newRow.className = oTbDetails.rows[i].className;
        newRow.ondblclick = oTbDetails.rows[i].ondblclick;

        for (j = 0; j < oTbDetails.rows[i].cells.length; j++) {
            var newCell = newRow.insertCell(j);
            newCell.className = oTbDetails.rows[i].cells[j].className;
            newCell.innerHTML = oTbDetails.rows[i].cells[j].innerHTML;
        }
        oTbDetails.deleteRow(i);
        i++;
    }

    // replacement des lignes dans le tableau d'origine tableau
    for (i = 0; i < oTbTmp.rows.length; i++) {
        var newRow = oTbDetails.insertRow(1);
        newRow.className = oTbTmp.rows[i].className;
        newRow.ondblclick = oTbTmp.rows[i].ondblclick;


        for (j = 0; j < oTbTmp.rows[i].cells.length; j++) {
            var newCell = newRow.insertCell(j);
            newCell.className = oTbTmp.rows[i].cells[j].className;
            newCell.innerHTML = oTbTmp.rows[i].cells[j].innerHTML;
        }
    }

    if (document.getElementById("countHead").getAttribute("ednSort") == '1') {
        document.getElementById("countHead").setAttribute("ednSort", "0");
        document.getElementById("IMG_ASC").style.visibility = "visible";
        document.getElementById("IMG_DESC").style.visibility = "hidden";
    }
    else {
        document.getElementById("countHead").setAttribute("ednSort", "1");
        document.getElementById("IMG_ASC").style.visibility = "hidden";
        document.getElementById("IMG_DESC").style.visibility = "visible";
    }
}


function chovstats() {
    document.getElementById("IMG_ASC").style.visibility = "visible";
    document.getElementById("IMG_DESC").style.visibility = "visible";
}


function choustats() {
    if (document.getElementById("countHead").getAttribute("ednSort") == '1')
        document.getElementById("IMG_ASC").style.visibility = "hidden";
    else
        document.getElementById("IMG_DESC").style.visibility = "hidden";

}

function doFilter(descid, value) {
    alert(descid);
    alert(value);
}

var cptresize = 0;
function doOnResize(bMaxCpt) {
    cptresize++;
    if (cptresize > 10) {    //GCH/SPH : Bug sur IOS 7 : #25401 - iPad - IOS 7 - Statistiques : Le OnResize boucle indéfiniement à partir de getAbsolutePosition
        cptresize = 0;
        return;
    }
    var nHeight = document.innerHeight;
    if (typeof (nHeight) == "undefined")
        nHeight = document.documentElement.clientHeight;

    var oDivGlobal = document.getElementById("DivGlobal");
    oDivGlobal.style.height = (nHeight - 10) + "px";

    var oDivDetails = document.getElementById("StatsRendDtl");
    var oDivChartBord = document.getElementById("DivChartBord");

    var objPos = getAbsolutePosition(oDivChartBord);

    //oDivDetails.style.height = (nHeight - 10) - (20) + "px";
    // #44530 : Tableau statistiques incomplet à l'affichage
    oDivDetails.style.height = (nHeight - 375) + "px"; // 375px correspond aux marges + titre + graphique + titre du tableau
}

function UpdateFirstRubrique() {

    var ddlValuesField = document.querySelectorAll("select[id*='ValuesField_']")[0];
    var ddlValuesOperation = document.getElementById("ValuesOperation");
    if (ddlValuesField) {
        var typeElement = ddlValuesField.options[ddlValuesField.selectedIndex].getAttribute('fmt');
        ddlValuesField.disabled = false;
        if ("count" == ddlValuesOperation.value.toLowerCase() && typeElement != 3) {
            ddlValuesField.disabled = true;
        }
    }


}