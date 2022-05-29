Number.isInteger = Number.isInteger || function (value) {
    return typeof value === "number" &&
        isFinite(value) &&
        Math.floor(value) === value;
};

Number.isNaN = Number.isNaN || function (value) { return typeof (o) === 'number' && isNaN(o) };

var colors = ['#E94649', '#F6B53F', '#6FAAB0', '#C4C24A', '#FB954F', '#005277', '#E27F2D',
    '#7cb5ec', '#434348', '#90ed7d', '#f7a35c', '#8085e9', '#f15c80', '#e4d354', '#2b908f', '#f45b5b', '#91e8e1',
    '#2f7ed8', '#0d233a', '#8bbc21', '#910000', '#1aadce', '#492970', '#f28f43', '#77a1e5', '#c42525', '#a6c96a',
    '#4572A7', '#AA4643', '#89A54E', '#80699B', '#3D96AE', '#DB843D', '#92A8CD', '#A47D7C', '#B5CA92']

var myChart;
var total = top._res_1278;
var valueLabel = top._res_437;
var combinedvalueLabel = top._res_437;
var percentLabel = top._res_6228;
var combinedpercentLabel = top._res_6228;
var emptyChartRessource = top._res_8211;

var Action = {
    UNDEFINED: 0,
    GLOBALCHART: 1, // syncfusion chart des eudoparts et rubriques
    STATCHART: 2 // syncfusion chart type stats appeler depuis le mode liste sur les filtres des rubriques  
}

var LEGENDPOSITION = {
    TOP: 0,
    BOTTOM: 1,
    RIGHT: 2,
    LEFT: 3
}

var TYPEAGREGATFONCTION = {
    COUNT: 0,
    SUM: 1,
    AVG: 2,
    MAX: 3,
    MIN: 4
}

var TYPEGRAPH = {
    SIMPLE_GRAPHE: 1,
    COMPLEXE_GRAPHE: 2,
    COMBINED_GRAPH: 3
}

var FUSIONCHART = {
    DOUGHNUT_2D: "doughnut2d",
    DOUGHNUT_3D: "doughnut3d",
    SEMI_DOUGHNUT_2D: "semidoughnut2d",
    PIE_2D: "pie2d",
    PIE_3D: "pie3d",
    SEMI_PIE_2D: "semipie2d",
    COLUMN_2D: "column2d",
    MS_COLUMN_2D: "mscolumn2d",
    COLUMN_3D: "column3d",
    MS_COLUMN_3D: "mscolumn3d",
    LINE: "line",
    MS_LINE: "msline",
    SPLINE: "spline",
    FUNNEL: "funnel",
    PYRAMID: "pyramid",
    BAR_2D: "bar2d",
    MS_BAR_2D: "msbar2d",
    MS_BAR_3D: "msbar3d",
    MS_COMBI_3D: "mscombi3d",
    STACKED_BAR_2D: "stackedbar2d",
    STACKED_BAR_3D: "stackedbar3d",
    AREA_2D: "area2d",
    MS_AREA: "msarea",
    STACKED_AREA_2D: "stackedarea2d",
    STACKED_COLUMN_2D: "stackedcolumn2d",
    STACKED_COLUMN_3D: "stackedcolumn3d",
    COMBINED: 'combine',
    CIRCULARGAUGE: 'circulargauge'

}

var SYNCFUSIONCHART = {
    DOUGHNUT: "doughnut",
    PIE: "pie",
    BAR: "bar",
    COLUMN: "column",
    AREA: "area",
    LINE: "line",
    SPLINE: "spline",
    FUNNEL: "funnel",
    PYRAMID: "pyramid",
    STACKING_AREA: "stackingarea",
    STACKING_COLUMN: "stackingcolumn",
    STACKING_BAR: "stackingbar",
    COMBINED: 'combine',
    CIRCULARGAUGE: 'circulargauge'
}

var CIRCULARGAUGEVALUETYPE = {
    FIXEDVALUE: '0',
    DYNAMICVALUE: '1'
}

var CHART_SERIES_TYPE = {
    /// <summary>
    /// Séries simples (Champ de valeur Unique)
    /// </summary>
    SERIES_TYPE_SIMPLE: '0',
    /// <summary>
    /// Serie par regroupement (regroupement d'une même valeurs suivant un autre champ type catalogue)
    /// </summary>
    SERIES_TYPE_GROUP: '1',
    /// <summary>
    ///Serie par valeurs (plusieurs champs de valeurs)
    /// </summary>
    SERIES_TYPE_VALUE: '2'
}

//Appel à la fonction sur navbar
function goNavF(sParam, listMode) {
     
    var oParam = sParam.split("$##$");
    if (oParam.length == 2) {
        var bCombined = oParam[0].split('$**$').length == 2;
        var nReportId = oParam[0];
        var sFilterid = 'filterid';
        var sTab = 'tab';

        if (bCombined) {

            let axes = nReportId[0];
            nReportId = oParam[0].split('$**$')[1];

            if (axes == 'Z') {


                sFilterid = top._CombinedZ.toLowerCase() + 'filterid';
                sTab = top._CombinedZ.toLowerCase() + 'tab';
            }
            else if (axes == 'Y') {

                sFilterid = top._CombinedY.toLowerCase() + 'filterid';
                sTab = 'tab';
            }
        }

        var oCharts = document.querySelectorAll("input[ednreportparam='" + nReportId + "']");

        if (typeof (oCharts) == "undefined")
            oCharts = document.querySelectorAll("div[id=DivChart'" + listMode ? nReportId : '' + "']");
        //if (oCharts.length == 1) {
        var sChartParam = "";
        sChartParam = "tab=" + oCharts[0].getAttribute(sTab) + "&filterid=" + oCharts[0].getAttribute(sFilterid)
            + "&addcurrentfilter=" + (bCombined ? "0" : oCharts[0].getAttribute("addcurrentfilter"));
        sChartParam += "$##$" + oParam[1];
        if (parent.goNav)
            parent.goNav(oParam[0], sChartParam);
        else
            goNav(oParam[0], sChartParam);
        // }
    }
}

function doMaximze(id) {

    if ((window['_md'] == 'undefined') && (window['_md']["myChart_" + id])) {
        //if (window['_md']["myChart_" + id]) {

        var myModal = window['_md']["myChart_" + id];

        if (myModal.isModalDialog) {
            if (myModal.sizeStatus != "max")
                myModal.MaxOrMinModal();
        }
    }
}

///Chargement du chart
function loadChart(nReportId, sync, stat) {
    if (sync) {
        loadSyncFusionChart(nReportId, stat);
        return;
    }
}

function loadSyncFusionChart(nReportId, stat) {
    var lstCharts;
    if (isNumeric(nReportId))
        lstCharts = document.querySelectorAll("input[ednchartparam='" + nReportId + "']");
    else
        lstCharts = document.querySelectorAll("input[ednchartparam]");

    addCss("syncFusion/ej.web.all.min", "HOMEPAGE");
    addCss("syncFusion/ej.responsive", "HOMEPAGE");
    addCss("syncFusion/ejgrid.responsive", "HOMEPAGE");

    addScript("syncFusion/ej.web.all.min", "HOMEPAGE");
    //addScript("syncFusion/ej.responsive", "HOMEPAGE");


    if (lstCharts.length > 0)
        setSyncfusionChart(lstCharts, stat);

}

// Mis a jour le graphique sans le recharger
/// [OBSOLETE] utiliser  eChart(divChart).redraw(); car plusieurs graphiques du même "chartId" peuvent être présents sur le dom
function redrawChart(chartId) {

    var chartDiv = document.getElementById('DivChart' + chartId);
    if (chartDiv) {
        var chart = $(chartDiv).data('ejChart');
        if (chart)
            chart.redraw();
    }
}

// Objet permettant d"encapsuler la logique chargement des graphiques
// qui est présent dans le container
function eChart(container, idChart) {
    var _container = container;
    var _idChart = idChart;
    var _divChart = _container.querySelector("div[id^='" + idChart + "']");
    var redrawCircularChart = function (chart) {
        try {

            if (_divChart) {
                var w = _divChart.clientWidth;
                var h = _divChart.clientHeight;
                var customLabelPosition = { xx: w / 2, yy: (h / 2) };
                var r = w / 2;
                if (w > h)
                    r = h / 2;

                var rangeWidth = (r > 30 ? r / 2 : r);
                Array.prototype.slice.apply(chart.model.scales[0].ranges).forEach(function (arrElem) {
                    arrElem.startWidth = rangeWidth;
                    arrElem.endWidth = rangeWidth;
                });

                chart.model.scales[0].customLabels[0].position.x = customLabelPosition.xx;
                chart.model.scales[0].customLabels[0].position.y = customLabelPosition.yy * 1.7;
                chart.model.scales[0].customLabels[1].position.x = customLabelPosition.xx;
                chart.model.scales[0].customLabels[1].position.y = customLabelPosition.yy - (r * .8);
                chart.setScaleRadius(0, (r < 75 ? r + 20 : r));
                chart.model.height = h;
                chart.model.width = w;
                chart.setPointerLength(0, 0, r - chart.model.scales[0].ranges[0].startWidth);
                chart.setPointerValue(0, 0, chart.model.initialValue);
                chart.resizeCanvas();
                chart.redraw('scale');

            } else
                console.log('Impossible d\'identifier le graphique avec l\'id=> ' + idChart);
        } catch (e) {
            console.log(e);
        }
    };
    return {

        containsData: function () {
            if (_divChart) {
                var chart = $(_divChart).data('ejChart');
                if (chart)
                    return true;
            }

            return false;
        },
        // Recharge complètement le graphique
        reload: function () {

            var bFromWidget = false;
            if (hasClass(_container, "widget-wrapper"))
                bFromWidget = true;
            var _chartParam = _container.querySelectorAll("input[ednchartparam]");
            if (_chartParam && _chartParam.length > 0) {
                setSyncfusionChart(_chartParam, false, bFromWidget);
            }

        },
        // Redessine le graphique
        redraw: function () {
            if (_divChart) {
                var chart = $(_divChart).data('ejChart');
                if (chart == null || typeof chart == 'undefined') {
                    chart = $(_divChart).data("ejCircularGauge");
                    if (chart)
                        redrawCircularChart(chart);
                } else
                    chart.redraw();
            } else
                console.log('Impossible d\'identifier le graphique avec l\'id=> ' + _idChart);
        },
    }
}

// Dans le cas des widgets, le set wait ne sera pas utilisé
function Factory(FromWidget) {

    FromWidget = typeof FromWidget !== "undefined" && FromWidget;

    // Par défaut on prend le setWait normal
    var internalSetWait = function (bWait) { top.setWait(bWait); };
    if (FromWidget)
        internalSetWait = function (bWait) { /* void setwait : ne fait rien*/ };

    return {
        setWait: internalSetWait,
        getUpdater: function (url, type) {

            if (FromWidget)
                return new QueuedUpdater(url, type);

            return new eUpdater(url, type);
        }
    }
}

// Queue unique sur toute l'appli
if (typeof (top.RunnableJobQueue) === "undefined")
    top.RunnableJobQueue = new JobQueue();

/// On garde une liste des updaters qu'on executera à la demande
function QueuedUpdater(url, type) {
    var _url = url;
    var _updater = new eUpdater(url, type);

    return {
        'asyncFlag': true,
        'ErrorCallBack': function () { },
        'addParam': function (key, value, method) { _updater.addParam(key, value, method); },
        'send': function (callback) {
            _updater.asyncFlag = this.asyncFlag;
            _updater.callback = this.ErrorCallBack;
            top.RunnableJobQueue.addJob({
                'execute': function (caller) { _updater.send(function (oRes) { callback(oRes); caller(); }); },
                'url': _url
            });
        }
    }
}

function log(msg) {
    var myDate = new Date();
    console.log("[" + myDate.getHours() + ":" + myDate.getMinutes() + ":" + myDate.getSeconds() + ":" + myDate.getMilliseconds() + "] " + msg);
}

// Queue contenu des jobs à lancer.
// Le lancement se fait en fontion du paramétrage
function JobQueue() {

    // Nombre de lancement à effectuer. 0 pour tous les jobs
    var MAX_PARALLEL_CONNECTION = 3;

    // file d'attente FIFO des jobs à lancer
    var _jobs = [];

    // Lance les jobs
    function execute() {

        var launchers = (MAX_PARALLEL_CONNECTION <= 0 || _jobs.length <= MAX_PARALLEL_CONNECTION) ? _jobs.length : MAX_PARALLEL_CONNECTION;
        for (var i = 0; i < launchers; i++)
            popAndExecute();
    }

    // On prend un job et on le lance puis le callabck fait pareil jusqu'à epuisement des travaux
    function popAndExecute() {

        if (_jobs.length == 0)
            return;

        var job = _jobs.pop();
        if (job) {
            //log("Running job " + job.url);
            job.execute(popAndExecute);
        }
    }

    return {
        'addJob': function (job) {
            //log("Adding job " + job.url);
            _jobs.push(job);
        },
        'run': function () { execute(); }
    }
}

function setSyncfusionChart(lstCharts, stat, bFromWidget) {
    var nReportId = 0;
    var ndisplaygrid = 0;
    var nb = lstCharts.length;
    var action = Action.GLOBALCHART;

    if (stat) {
        action = Action.STATCHART;
        var element = document.querySelectorAll("select[name*='ValuesField']")[0];
        changeSelectValue(element);
    }

    // quand on vient du widget, on n a pas besoin du setwait global
    var setWaitChart = Factory(bFromWidget).setWait;
    var fromModalChart = (typeof top.modalCharts != 'undefined' && top.modalCharts != null && top.modalCharts.IsAvailable());
    for (var i = 0; i < nb; i++) {
        if (stat || fromModalChart)
            setWaitChart(true);
        var syncChart = lstCharts[i];
        ndisplaygrid = getAttributeValue(syncChart, "displaygrid");
        var bDisplayGrid = (ndisplaygrid == "1");
        nReportId = getAttributeValue(syncChart, "ednchartparam");

        if (getAttributeValue(syncChart, 'isLoaded') == '1')
            continue;

        if ((nReportId != "" && nReportId != "0") || stat) {
            //appel updater
            sendToUrl(action, nReportId, syncChart, bDisplayGrid, 0, null, null, bFromWidget)
        }
        else { setWaitChart(false); }
    }

    //Chart en mode popup
    if (top.modalCharts && top.document.getElementById("frm_" + top.modalCharts.UID) && top.modalCharts.IsAvailable()) {
        var globalDiv = document.getElementById("DivGlobal");
        if (globalDiv) {
            var size = top.getWindowSize();
            globalDiv.style.width = (size.w - 50) + 'px';
            globalDiv.style.height = size.h - top.modalCharts.getDivButton().clientHeight - 120 + 'px';
        }
    }
}

//Appeler un updater et récuperer un flux XML avec les paramètres du graphique
function sendToUrl(action, nReportId, syncChart, bDisplayGrid, fieldDescid, agregatFonction, expressFilterParam, bFromWidget) {
    try {
        var setWaitChart = Factory(bFromWidget).setWait;
        var sURLChart = "mgr/eChartManager.ashx?sync=1&reportid=" + nReportId;

        if (typeof bFromWidget === "undefined")
            bFromWidget = false;

        if (typeof getCurrentView == "function" && getCurrentView(document) == "FILE_MODIFICATION") {
            var nFileId = document.getElementById("fileDiv_" + nGlobalActiveTab).getAttribute("fid");
            sURLChart += "&fileid=" + nFileId;
        }

        var upd = Factory(bFromWidget).getUpdater(sURLChart, 0);
        upd.addParam("action", action, "post");

        if (action == Action.STATCHART) {
            upd.addParam("tab", syncChart.getAttribute("tab"), "post");
            upd.addParam("tabFrom", syncChart.getAttribute("tabFrom"), "post");
            upd.addParam("idFrom", syncChart.getAttribute("idFrom"), "post");
            upd.addParam("descid", syncChart.getAttribute("descid"), "post");

            if (typeof fieldDescid !== 'undefined' && fieldDescid)
                upd.addParam("field", fieldDescid, "post");

            if (typeof agregatFonction !== 'undefined' && agregatFonction)
                upd.addParam("agregatFonction", agregatFonction, "post");

        } else if (action == Action.GLOBALCHART) {
            if (typeof expressFilterParam != 'undefined' && expressFilterParam)
                upd.addParam("expressFilterParam", expressFilterParam, "post");

            if (bFromWidget && nGlobalActiveTab && typeof (top.GetCurrentFileId) === "function") {
                upd.addParam("tabFrom", nGlobalActiveTab, "post");
                upd.addParam("idFrom", top.GetCurrentFileId(nGlobalActiveTab), "post");
            }
        }
        upd.ErrorCallBack = function (oRes) { console.log(oRes); };
        upd.send(function (oRes) { displaySyncFusionChart(oRes, syncChart, bDisplayGrid, null, null, nReportId, action, bFromWidget); });
    } catch (e) {
        setWaitChart(false);
        eAlert(0, top._res_416, top._res_6474);
        console.log(e);
    }
}

//Méthode qui permet de charger/Déssiner un graphique
function displaySyncFusionChart(oRes, mychart, bDisplayGrid, idChartRendrer, chartType, reportId, action, bFromWidget) {
    // quand on vient du widget, on a pas besoin du setwait global - on utilise un setWait vide
    var setWaitChart = Factory(bFromWidget).setWait;
    if (oRes != null && typeof oRes.getElementsByTagName("chart")[0] == 'undefined' && oRes.getElementsByTagName("result")[0] != null && typeof oRes.getElementsByTagName("result")[0] != 'undefined') {
        ReturnError(oRes);
        return;
    }

    //stocker temporairement le window.name qui est modifié par eChart
    var windowName = window.name;
    var divChart = 'DivChart';
    var globalDiv = document.getElementById("DivGlobal");
    var divChartDialogue = document.getElementById(divChart);
    var divChartdialogueGlobal = document.getElementById('DivChartBord');
    var fromChartDialog = (divChartDialogue && divChartdialogueGlobal);
    var exportChart = null;
    var bFieldChart = false;

    if (typeof reportId != 'undefined' && reportId.toString().split('_').length == 1
        && top.modalCharts != null && typeof top.modalCharts != 'undefined'
        && top.modalCharts.IsAvailable()
        && top.modalCharts.getIframe() != null
        && (typeof modalCharts == 'undefined' || modalCharts == null)) {
        if (fromChartDialog)
            exportChart = top.modalCharts.getIframe().document.getElementById('exPortChart');
        else
            exportChart = top.modalCharts.getIframe().document.getElementById('exPortChart' + reportId);
    }
    else
        exportChart = document.getElementById('exPortChart' + reportId);

    if (exportChart)
        setAttributeValue(exportChart, 'hideElement', 0);
    //Si aucun retoure , on affiche une alert
    if (!oRes) {
        if (typeof exportChart != 'undefined' && exportChart != null)
            exportChart.parentElement.removeChild(exportChart);
        setWaitChart(false);
        eAlert(0, top._res_72, top._res_6474);
        return;
    }

    var mainChart;
    var typeGraphe = null;
    var complexChart = false;
    var combinedChart = false;
    var sections = [];
    var sectionsCombined = [];
    var dataset = [];
    var globalSelection = [];
    var categorie = [];
    var graphEmpty = (oRes.getElementsByTagName("chart")[0].getAttribute("emptyGraph") == '1');
    var combinedEraphEmpty = (oRes.getElementsByTagName("chart")[0].getAttribute(top._CombinedZ + "emptyGraph") == '1');
    var fldX = oRes.getElementsByTagName("chart")[0].getAttribute("fldX");
    var CombinedZfldX = oRes.getElementsByTagName("chart")[0].getAttribute(top._CombinedZ + "fldX");
    var fldY = oRes.getElementsByTagName("chart")[0].getAttribute("fldY");
    var CombinedZfldY = oRes.getElementsByTagName("chart")[0].getAttribute(top._CombinedZ + "fldY");
    var h = parseInt(getAttributeValue(mychart, "h"));
    var w = parseInt(getAttributeValue(mychart, "w"));
    var bStat = (oRes.getElementsByTagName("chart")[0].getAttribute("fromStat") == '1');
    var chartSerieType = oRes.getElementsByTagName("chart")[0].getAttribute("typeGraph");
    var bCampagneMail = (idChartRendrer != null && typeof idChartRendrer != 'undefined' && idChartRendrer != '' && chartType != null && typeof chartType != 'undefined' && chartType != '');
    var liste = (typeof getCurrentView == "function" && (getCurrentView(document) == "LIST" || getCurrentView(document) == "FILE_MODIFICATION"));
    var idChart = liste ? getAttributeValue(mychart, "ednchartparam") : '';

    if (typeof (reportId) != 'undefined' && reportId != null && reportId.toString().split('_')[0] * 1 > 0) {
        idChart = reportId;
    }

    mainChart = document.getElementById('mainChart_' + idChart);

    //selon le type de la serie on modifie le menu des export //&& 
    if (typeof chartSerieType != 'undefined' && chartSerieType != null && exportChart != null && typeof exportChart != 'undefined' && isNumeric(parseInt(chartSerieType))) {
        if (typeof getCurrentView == 'function' && getCurrentView(document) == 'FILE_MODIFICATION')
            setAttributeValue(exportChart, 'Excel', '1');
        else {
            if (chartSerieType == CHART_SERIES_TYPE.SERIES_TYPE_SIMPLE)
                setAttributeValue(exportChart, 'Excel', '0');
            else
                setAttributeValue(exportChart, 'Excel', '1');
        }
    }

    //Calcule de l'element Chart
    var divChartSelector = "#";
    if (idChartRendrer)
        divChartSelector += idChartRendrer;
    else if (divChartDialogue && divChartdialogueGlobal) {
        divChartSelector += divChart;
        fromChartDialog = true;
    }
    else
        divChartSelector += divChart + idChart;

    var finalDivChart = $(divChartSelector);

    var zeroSize = false

    //Taille du graphique
    if (finalDivChart) {
        if (bCampagneMail) {
            h = 400;
            finalDivChart.height(h);
            finalDivChart.width(finalDivChart.parent().width() - 20);
        }
        else {
            if (finalDivChart.height() == 0) {
                if (globalDiv)
                    h = globalDiv.clientHeight;
                if (bStat)
                    h = h / 1.5;

                //on enlève la hauteur des butons pour ne pas avoir le scrol
                if (top.modalCharts && top.document.getElementById("frm_" + top.modalCharts.UID) && top.modalCharts.IsAvailable())
                    h = h - top.modalCharts.getDivButton().clientHeight - 20;

                //BSE:#67 089
                //on enlève la hauteur des filtres express 
                if (document.getElementsByClassName('divGlobalFiltreExpress').length == 0 && document.getElementById('filterExpress') != null)
                    document.getElementById('filterExpress').style.paddingBottom = '40pt';

                finalDivChart.height(h);
            }

            if (finalDivChart.width() == 0) {
                if (globalDiv)
                    w = globalDiv.clientWidth - 20;
                if (bStat)
                    w = w / 1.5;
                finalDivChart.width(w);
            }
        }

        if (mainChart) {
            finalDivChart.width(mainChart.parentElement.clientWidth);
            //Traitement spécifique pour les rubriques type graphique
            if (typeof mainChart.parentElement != 'undefined' && mainChart.parentElement != null && mainChart.parentElement.tagName == 'TD') {
                bDisplayGrid = false;
                bFieldChart = true;
                if (typeof exportChart != 'undefined' && exportChart != null)
                    exportChart.style.margin = '0px';


                var chartW = mainChart.parentElement.clientWidth - 15;
                var chartH = mainChart.parentElement.clientHeight;


                zeroSize = (chartW <= 0 || chartH <= 0)


                mainChart.style.width = chartW + 'px';
                mainChart.style.height = chartH + 'px'

            }
        }
    }

    if (typeof oRes.getElementsByTagName("chart")[0].length != 0
        && oRes.getElementsByTagName("chart")[0].getElementsByTagName("categories").length == 0
        && oRes.getElementsByTagName("chart")[0].getElementsByTagName("set").length != 0) {
        sections = oRes.getElementsByTagName("chart")[0].getElementsByTagName("set");
        typeGraphe = TYPEGRAPH.SIMPLE_GRAPHE;
    }

    if (typeof oRes.getElementsByTagName("chart")[0].length != 0
        && oRes.getElementsByTagName("chart")[0].getElementsByTagName("categories").length == 0
        && oRes.getElementsByTagName("chart")[0].getElementsByTagName(top._CombinedZ + "set").length != 0) {
        sectionsCombined = oRes.getElementsByTagName("chart")[0].getElementsByTagName(top._CombinedZ + "set");
        typeGraphe = TYPEGRAPH.COMBINED_GRAPH;

    }

    if (sections.length == 0 &&
        typeof oRes.getElementsByTagName("chart")[0] != "undefined" && oRes.getElementsByTagName("chart")[0].getElementsByTagName("categories").length == 1) {
        typeGraphe = TYPEGRAPH.COMPLEXE_GRAPHE;
        categorie = oRes.getElementsByTagName("chart")[0].getElementsByTagName("categories")[0].getElementsByTagName("category");
        dataset = oRes.getElementsByTagName("chart")[0].getElementsByTagName("dataset");
    }

    //si le graph est vide, on remplace par une image et un message
    if ((typeGraphe != TYPEGRAPH.COMBINED_GRAPH && graphEmpty) ||
        (typeGraphe == TYPEGRAPH.COMBINED_GRAPH && graphEmpty && combinedEraphEmpty)) {
        setEmptyGraph(divChartSelector);

        if (typeof exportChart != 'undefined' && exportChart != null)
            setAttributeValue(exportChart, 'hideElement', 1)

        if (top.instanceSyncfusionGrid != null && typeof top.instanceSyncfusionGrid.destroy == 'function') {
            try {
                top.instanceSyncfusionGrid.destroy();
                top.instanceSyncfusionGrid = null;
            } catch (e) {
                setWaitChart(false);
                RemoveSetWait(divChartSelector);
                console.log(e.message);
            }
        }
        setWaitChart(false);
        RemoveSetWait(divChartSelector);
    } else {
        try {
            //on vérifi qu'on a pas dèjas affecté à la div un graphique
            if (typeof (finalDivChart.data("ejChart")) == 'undefined') {
                var chartSeries = [];
                var chartPoints = [];
                var serie = { link: '', text: '', name: '', x: '', y: '', max: 0, min: 0, sum: 0 };
                var rotation = 20;
                var depth = 60;
                var wallsize = 5;
                var tilt = 0;
                var perspectiveAngle = 50;
                var canResize = false;
                var bPercentVal = false;
                var enableNullY = true;
                var enable3D = false;
                var enableRotation = false;
                var explode = false;
                var labelPosition = "outside";
                var seriesOptionsEnableAnimation = true;
                var seriesOptionsTooltipFormat = "#point.x# : #point.y#";
                var seriesOptionsMarkerShape = "circle";
                var chartThemeColor = oRes.getElementsByTagName("chart")[0].getAttribute("xrmThemeColor");
                var useThemeColor = (oRes.getElementsByTagName("chart")[0].getAttribute("useThemeColor") == '1');
                var usePalette = false;

                if (chartThemeColor == null)
                    chartThemeColor = oRes.getElementsByTagName("chart")[0].getAttribute(top._CombinedZ + "xrmThemeColor");
                if (chartThemeColor == '#FFF' || chartThemeColor == '#FFFFFF')
                    chartThemeColor = colors[0];

                var themBlack = (chartThemeColor == '#000');
                var chartDataLabelFont = { color: '#5f5e5f' };
                var chartDataLabel = {
                    visible: true,
                    font: chartDataLabelFont,
                    maximumLabelWidth: 200
                };
                var seriesOptionsTooltipFormatVisible = true;
                var seriesOptionsMarkerShapeVisible = false;
                var seriesOptionsMarkerShapeSize = { height: 5, width: 5 };
                var seriesOptionsBorder = { width: 2 };

                /*Paramètres recupérés du flux xml */
                var nCntWidth = $(divChartSelector).width();
                var titleText = oRes.getElementsByTagName("chart")[0].getAttribute("caption");
                var chartTitle = {
                    position: "top",
                    textAlignment: "center",
                    maximumWidth: nCntWidth > 40 ? nCntWidth - 20 : 20 ,
                    enableTrim: true,
                    font: { fontWeight: "Bold", opacity: 0.8, color: chartThemeColor },
                };

                var cgintervals = oRes.getElementsByTagName("chart")[0].getAttribute("cgintervals");
                var cgvtype = oRes.getElementsByTagName("chart")[0].getAttribute("cgvtype");
                var cgvfixedvalue = oRes.getElementsByTagName("chart")[0].getAttribute("cgvfixedvalue");

                var idTableStat = oRes.getElementsByTagName("chart")[0].getAttribute("idTableStat");
                var CombinedZidTableStat = oRes.getElementsByTagName("chart")[0].getAttribute(top._CombinedZ + "idTableStat");
                var rotate = oRes.getElementsByTagName("chart")[0].getAttribute("Rotate");
                var slantLabels = oRes.getElementsByTagName("chart")[0].getAttribute("slantLabels");
                var showPercentValues = (parseInt(oRes.getElementsByTagName("chart")[0].getAttribute("showPercentValues")) == 1);
                var totalValues = oRes.getElementsByTagName("chart")[0].getAttribute("totalValues");
                var totalCombinedValues = oRes.getElementsByTagName("chart")[0].getAttribute(top._CombinedZ + "totalValues");
                var formatedTotalCombinedValues = oRes.getElementsByTagName("chart")[0].getAttribute(top._CombinedZ + "formatedTotalValues");
                var showValues = parseInt(oRes.getElementsByTagName("chart")[0].getAttribute("showValues"));
                var bShowValues = (showValues == '1');
                var stack100Percent = (parseInt(oRes.getElementsByTagName("chart")[0].getAttribute("stack100Percent")) == 1);
                var decimals = parseInt(oRes.getElementsByTagName("chart")[0].getAttribute("decimals"));
                var CombinedZdecimals = parseInt(oRes.getElementsByTagName("chart")[0].getAttribute(top._CombinedZ + "decimals"));
                var decimalSeparator = oRes.getElementsByTagName("chart")[0].getAttribute("decimalSeparator");
                var thousandSeparator = oRes.getElementsByTagName("chart")[0].getAttribute("thousandSeparator");
                var xAxisName = oRes.getElementsByTagName("chart")[0].getAttribute("xAxisName");
                var CombinedZxAxisName = oRes.getElementsByTagName("chart")[0].getAttribute(top._CombinedZ + "xAxisName");
                var yAxisName = oRes.getElementsByTagName("chart")[0].getAttribute("yAxisName");
                var CombinedZyAxisName = oRes.getElementsByTagName("chart")[0].getAttribute(top._CombinedZ + "yAxisName");
                var tabFldY = oRes.getElementsByTagName("chart")[0].getAttribute("tabfldY");;
                var combinedTabFldY = oRes.getElementsByTagName("chart")[0].getAttribute(top._CombinedZ + "tabfldY");
                var primaryXAxisFont = { fontWeight: "Bold", opacity: 0.8, color: chartThemeColor };
                var primaryYAxisFont = { fontWeight: "Bold", opacity: 0.8, color: chartThemeColor };
                var primaryXisInversed = parseInt(oRes.getElementsByTagName("chart")[0].getAttribute("typeSort"));
                var bPercentGraph = (stack100Percent && showPercentValues);
                var showEtiquette = (parseInt(oRes.getElementsByTagName("chart")[0].getAttribute("showEtiquette")) == 1);
                var showLegend = (parseInt(oRes.getElementsByTagName("chart")[0].getAttribute("showLegend")) == 1);
                var legendPositionFromChart = parseInt(oRes.getElementsByTagName("chart")[0].getAttribute("legendPosition"));
                var sValueOperations = oRes.getElementsByTagName("chart")[0].getAttribute("sValueOperations");
                var CombinedZsValueOperations = oRes.getElementsByTagName("chart")[0].getAttribute(top._CombinedZ + "sValueOperations");
                var displayZaxe = (parseInt(oRes.getElementsByTagName("chart")[0].getAttribute("displayzaxe")) == 1);
                var labelRotation = parseInt(oRes.getElementsByTagName("chart")[0].getAttribute("labelRotation"));
                var bDateFr = (oRes.getElementsByTagName("chart")[0].getAttribute("bDateFr") == '1');
                var bK = (oRes.getElementsByTagName("chart")[0].getAttribute("bk") == '1');
                var combinedBk = (oRes.getElementsByTagName("chart")[0].getAttribute(top._CombinedZ + "bk") == '1');
                var legendDefaultPosition = 'bottom';
                var setOnlyOneLegendColum = false;
                var bAddGraphOpacity = bCampagneMail;
                var chartOpacityValue = 0.7;
                var applayPrimaryXAxisTochart = false;
                var chartAxe = [];
                var series = [];
                var serieMax = 0;
                var serieMin = 0;
                var serieSomme = 0;
                var maxSerieSomme = 0;
                var addDoughnutSize = false;
                var addExplodeIndex = false;
                var link = '', text = '', doubleText = 0, name = '', x = '', y = '', max = 0, min = 0, sum = 0;
                var sideBySideSeriesPlacement = false;
                var bLabelInside = false;
                var limiteNbMax = false;

                if (chartType == null || typeof (chartType) == 'undefined' || chartType == '')
                    chartType = getAttributeValue(mychart, "chart");

                if (chartType == '')
                    chartType = oRes.getElementsByTagName("chart")[0].getAttribute("sSerieName");

                var chartMarker = {
                    dataLabel:
                    {
                        visible: bShowValues,
                        font: chartDataLabelFont,
                        maximumLabelWidth: 200,
                        shape: 'none'
                    }
                };

                var commonSeriesOptions = {
                    enableAnimation: seriesOptionsEnableAnimation,
                    enableSmartLabels: false,
                    tooltip:
                    {
                        visible: seriesOptionsTooltipFormatVisible,
                        format: seriesOptionsTooltipFormat,
                        rx: 5,
                        ry: 5
                    },
                    marker:
                    {
                        shape: seriesOptionsMarkerShape,
                        size: seriesOptionsMarkerShapeSize,
                        visible: seriesOptionsMarkerShapeVisible
                    },
                    border: seriesOptionsBorder,
                };


                if ((!chartType || chartType == '' || chartType == null) && typeGraphe != TYPEGRAPH.COMBINED_GRAPH)
                    chartType = 'pie3d';
                else if (chartType == null && typeGraphe == TYPEGRAPH.COMBINED_GRAPH)
                    chartType = SYNCFUSIONCHART.CIRCULARGAUGE;

                chartType = chartType.toLowerCase();

                if (chartType.indexOf("3d") != -1) {
                    enable3D = true;
                    enableRotation = false;
                }

                if (titleText)
                    chartTitle.text = titleText;

                if (typeGraphe == TYPEGRAPH.COMBINED_GRAPH) {
                    for (var i = 0; i < sections.length; i++) {
                        globalSelection.push(getAttributeValue(sections[i], 'label').split('$#$')[0]);
                    }

                    for (var i = 0; i < sectionsCombined.length; i++) {
                        var key = getAttributeValue(sectionsCombined[i], 'label').split('$#$')[0];
                        if (globalSelection.indexOf(key) === -1)
                            globalSelection.push(key);
                    }

                    //Si la rubrique des ettiquêtes est de type date : tri spécifique pour les jours
                    if (bDateFr)
                        SetDateTimeArraySort(globalSelection);
                    else
                        globalSelection.sort();
                }

                var primaryXAxis = {
                    visible: showEtiquette,
                    labelRotation: isNaN(labelRotation) ? 0 : labelRotation,
                    labelIntersectAction: 'Trim',
                    title: {
                        text: (typeGraphe == TYPEGRAPH.COMBINED_GRAPH ? null : xAxisName),
                        font: primaryXAxisFont,
                        enableTrim: true,
                        //maximumTitleWidth: 200
                    },
                    axisLine: { visible: true, dashArray: "2,3", offset: 5, color: chartThemeColor, width: 1.1 },
                    //isInversed: primaryXisInversed

                };
                var primaryYAxis = {
                    title: {
                        text: yAxisName,
                        font: primaryYAxisFont,
                        enableTrim: true,
                        //maximumTitleWidth: 250
                    },
                    axisLine: { visible: true, dashArray: "2,3", offset: 5, color: chartThemeColor, width: 1.1 },
                    labelFormat: "{value}" + (stack100Percent ? '%' : bK ? 'K' : '')
                };

                /*
                Position de la légende
                */

                switch (legendPositionFromChart) {
                    case LEGENDPOSITION.BOTTOM:
                        legendDefaultPosition = 'bottom';
                        break;
                    case LEGENDPOSITION.TOP:
                        legendDefaultPosition = 'top';
                        break;
                    case LEGENDPOSITION.LEFT:
                        legendDefaultPosition = 'left';
                        setOnlyOneLegendColum = true;
                        break;
                    case LEGENDPOSITION.RIGHT:
                        legendDefaultPosition = 'right';
                        setOnlyOneLegendColum = true;
                        //pour afficher la légende à droite, il faut réduire la largeur du graphique par rapport à la largeur du conteiner pour afficher le scroll
                        if (typeof finalDivChart != 'undefined' && finalDivChart != null && finalDivChart.parent() != null && typeof finalDivChart.parent() != 'undefined')
                            finalDivChart.width(finalDivChart.parent().width() - 10);
                        break;
                    default:
                        legendDefaultPosition = 'bottom';
                        break;
                }

                var chartLegend = {
                    visible: showLegend,
                    position: legendDefaultPosition,
                    shape: 'seriesType',
                    alignment: 'Center',
                    itemPadding: 10,
                    itemStyle: {
                        height: 10,
                        width: 10
                    },
                    textWidth: 100,
                    textOverflow: "Trim"
                };

                // Si on positionne la légénde sur les cotés , on affiche qu'une seule colonne et on lui définie une taille
                if (setOnlyOneLegendColum) {
                    chartLegend.columnCount = 1;
                    chartLegend.size = { width: '10%' };
                }

                //pour campagne mail et chart simple on affiche pas la légende
                chartLegend.visible = (bCampagneMail ? false : showLegend);

                switch (chartType) {
                    case FUSIONCHART.DOUGHNUT_2D:
                    case FUSIONCHART.DOUGHNUT_3D:
                    case FUSIONCHART.SEMI_DOUGHNUT_2D:
                        bPercentVal = true;
                        if (chartType.indexOf("2d") != -1 && !bCampagneMail) {
                            addExplodeIndex = false;
                            commonSeriesOptions.explode = false;
                            explode = true;
                        }

                        labelPosition = 'Outside';
                        primaryXAxis.labelPosition = labelPosition;
                        rotation = 25;
                        depth = 25;
                        tilt = -15;
                        if (chartType.indexOf("semi") != -1) {
                            addExplodeIndex = false;
                            commonSeriesOptions.startAngle = -90;
                            commonSeriesOptions.endAngle = 90;
                        }

                        chartType = SYNCFUSIONCHART.DOUGHNUT;
                        chartLegend.visible = bCampagneMail ? false : showLegend;
                        limiteNbMax = true;
                        break;

                    case FUSIONCHART.PIE_2D:
                    case FUSIONCHART.PIE_3D:
                    case FUSIONCHART.SEMI_PIE_2D:
                        bPercentVal = true;
                        if (chartType.indexOf("2d") != -1 && !bCampagneMail) {
                            addExplodeIndex = false;
                            commonSeriesOptions.explode = false;
                            explode = true;
                        }

                        labelPosition = 'Outside';
                        primaryXAxis.labelPosition = labelPosition;
                        rotation = 25;
                        depth = 25;
                        tilt = -15;

                        if (bStat || bCampagneMail)
                            chartMarker.dataLabel.visible = true;

                        if (chartType.indexOf("semi") != -1) {
                            addExplodeIndex = false;
                            commonSeriesOptions.startAngle = -90;
                            commonSeriesOptions.endAngle = 90;
                        }

                        chartType = SYNCFUSIONCHART.PIE;
                        chartLegend.visible = bCampagneMail ? false : showLegend;
                        limiteNbMax = true;
                        break;
                    case FUSIONCHART.COLUMN_2D:
                    case FUSIONCHART.MS_COLUMN_2D:
                    case FUSIONCHART.COMBINED:
                        usePalette = (chartType == FUSIONCHART.COLUMN_2D);
                        bLabelInside = true;
                        primaryXAxis.title.visible = (typeGraphe != TYPEGRAPH.SIMPLE_GRAPHE);
                        sideBySideSeriesPlacement = true;
                        commonSeriesOptions.columnSpacing = 0.1;

                        if (typeGraphe == TYPEGRAPH.SIMPLE_GRAPHE)
                            commonSeriesOptions.columnWidth = 0.5;
                        else
                            commonSeriesOptions.columnWidth = 0.8;

                        commonSeriesOptions.enableSmartLabels = true && !zeroSize;
                        chartType = SYNCFUSIONCHART.COLUMN;
                        break;

                    case FUSIONCHART.MS_COLUMN_3D:
                    case FUSIONCHART.COLUMN_3D:
                        usePalette = (chartType == FUSIONCHART.COLUMN_3D);
                        chartType = SYNCFUSIONCHART.COLUMN;
                        bLabelInside = true;
                        primaryXAxis.title.visible = (typeGraphe != TYPEGRAPH.SIMPLE_GRAPHE);
                        commonSeriesOptions.cornerRadius = 8;
                        commonSeriesOptions.columnSpacing = 0.1;

                        if (typeGraphe == TYPEGRAPH.SIMPLE_GRAPHE)
                            commonSeriesOptions.columnWidth = 0.5;
                        else
                            commonSeriesOptions.columnWidth = 0.8;
                        sideBySideSeriesPlacement = true;
                        break;

                    case FUSIONCHART.LINE:
                    case FUSIONCHART.MS_LINE:
                        chartType = SYNCFUSIONCHART.LINE;
                        commonSeriesOptions.marker.visible = true;
                        applayPrimaryXAxisTochart = true;
                        chartMarker.dataLabel.verticalTextAlignment = 'center';
                        chartMarker.dataLabel.horizontalTextAlignment = 'center';
                        break;
                    case SYNCFUSIONCHART.SPLINE:
                        commonSeriesOptions.marker.visible = true;
                        applayPrimaryXAxisTochart = true;
                        break;
                    case SYNCFUSIONCHART.FUNNEL:
                    case SYNCFUSIONCHART.PYRAMID:
                        usePalette = true;
                        bPercentVal = true;
                        bLabelInside = false;
                        //Redimentionner le graphique si Funnel ou pyramid
                        if ((globalDiv != null && typeof globalDiv != 'undefined')) {
                            chartMarker.dataLabel.enableContrastColor = true;
                            finalDivChart.width(globalDiv.clientHeight + (globalDiv.clientHeight * (45 / 100)));
                        } else if (bFieldChart)
                            finalDivChart.width(mainChart.parentElement.clientHeight + (mainChart.parentElement.clientHeight * (45 / 100)));
                        finalDivChart.css("margin", "0 auto");
                        break;
                    case FUSIONCHART.BAR_2D:
                    case FUSIONCHART.MS_BAR_2D:
                    case FUSIONCHART.MS_BAR_3D:
                        usePalette = (chartType == FUSIONCHART.BAR_2D);
                        if (chartType != FUSIONCHART.MS_BAR_3D) {
                            primaryXAxis.enableTrim = true;
                            primaryXAxis.maximumLabelWidth = 150;

                        } else {
                            primaryXAxis.labelIntersectAction = '';
                            primaryXAxis.enableTrim = false;
                            primaryXAxis.maximumLabelWidth = '';
                        }

                        bLabelInside = true;
                        sideBySideSeriesPlacement = true;
                        commonSeriesOptions.marker.visible = false;
                        chartMarker.dataLabel.verticalTextAlignment = 'center';
                        commonSeriesOptions.columnSpacing = 0.1;
                        primaryXAxis.title.visible = (typeGraphe != TYPEGRAPH.SIMPLE_GRAPHE);
                        if (typeGraphe == TYPEGRAPH.SIMPLE_GRAPHE)
                            commonSeriesOptions.columnWidth = 0.5;
                        else
                            commonSeriesOptions.columnWidth = 1;

                        primaryXAxis.labelRotation = 0;
                        chartType = SYNCFUSIONCHART.BAR;
                        break;

                    case FUSIONCHART.MS_COMBI_3D:
                        chartType = SYNCFUSIONCHART.LINE;
                        enable3D = false;
                        enableRotation = false;
                        applayPrimaryXAxisTochart = true;
                        break;
                    case FUSIONCHART.STACKED_BAR_2D:
                    case FUSIONCHART.STACKED_BAR_3D:
                        chartType = SYNCFUSIONCHART.STACKING_BAR;
                        bLabelInside = true;
                        commonSeriesOptions.marker.visible = false;
                        chartMarker.dataLabel.verticalTextAlignment = 'center';
                        chartMarker.dataLabel.horizontalTextAlignment = 'center';
                        chartMarker.dataLabel.textPosition = "middle";
                        break;
                    case FUSIONCHART.AREA_2D:
                    case FUSIONCHART.MS_AREA:
                        chartType = SYNCFUSIONCHART.AREA;
                        chartMarker.dataLabel.verticalTextAlignment = 'center';
                        chartMarker.dataLabel.horizontalTextAlignment = 'center';
                        chartMarker.dataLabel.textPosition = "middle";
                        enableNullY = false;
                        bAddGraphOpacity = true;
                        applayPrimaryXAxisTochart = true;
                        break;
                    case FUSIONCHART.STACKED_AREA_2D:
                        chartType = SYNCFUSIONCHART.STACKING_AREA;
                        enableNullY = false;
                        commonSeriesOptions.marker.visible = false;
                        chartMarker.dataLabel.verticalTextAlignment = 'center';
                        chartMarker.dataLabel.horizontalTextAlignment = 'center';
                        chartMarker.dataLabel.textPosition = "middle";
                        bAddGraphOpacity = true;
                        applayPrimaryXAxisTochart = true;
                        break;
                    case FUSIONCHART.STACKED_COLUMN_2D:
                    case FUSIONCHART.STACKED_COLUMN_3D:
                        chartType = SYNCFUSIONCHART.STACKING_COLUMN;
                        bLabelInside = true;
                        commonSeriesOptions.marker.visible = false;
                        chartMarker.dataLabel.verticalTextAlignment = 'center';
                        chartMarker.dataLabel.horizontalTextAlignment = 'center';
                        chartMarker.dataLabel.textPosition = "middle";
                        break;
                    case FUSIONCHART.CIRCULARGAUGE:
                        chartType = SYNCFUSIONCHART.CIRCULARGAUGE;
                        break;

                }
                //Affectation du type au graphique
                commonSeriesOptions.type = chartType;

                //si les valeurs sont en inside , on change la couleur en black
                if (bLabelInside) {
                    labelPosition = "inside";
                    chartDataLabel.font.color = 'black';
                }

                //pour campagne mail on a besoin du total pour calculer le pourcentage
                if (totalValues != null)
                    totalValues = parseFloat(totalValues);

                var SerieLabel = oRes.getElementsByTagName("chart")[0].getAttribute("libelleRubriqueStat");
                var showTotalSummary = true;
                var operationValue = getAgregatFonctionEnum(sValueOperations, action, typeGraphe == TYPEGRAPH.COMBINED_GRAPH, tabFldY);
                var gridData = [];
                var column = [
                    { field: "name", headerText: xAxisName, isPrimaryKey: true, textAlign: ej.TextAlign.Left, clipMode: ej.Grid.ClipMode.EllipsisWithTooltip, width: 75 },
                    { field: "value", headerText: yAxisName, width: 80, textAlign: ej.TextAlign.Center, type: 'numeric', format: "{0:N" + decimals + "}" },
                    { field: "percent", headerText: percentLabel, width: 80, textAlign: ej.TextAlign.Center, type: 'numeric', format: "{0:P2}" }];

                //Si graphique combiné, on ajoute les valeurs/pourcentage du graphique Linéaire
                if (typeGraphe == TYPEGRAPH.COMBINED_GRAPH) {
                    var CombinedZoperationValue = getAgregatFonctionEnum(CombinedZsValueOperations, action, typeGraphe == TYPEGRAPH.COMBINED_GRAPH, combinedTabFldY, true);
                    column.push(
                        { field: "combinedvalue", headerText: CombinedZyAxisName, width: 80, textAlign: ej.TextAlign.Center, type: 'numeric', format: "{0:N" + CombinedZdecimals + "}" }
                    );

                    column.push(
                        { field: "combinedpercent", headerText: combinedpercentLabel, width: 80, textAlign: ej.TextAlign.Center, type: 'numeric', format: "{0:P2}" }
                    );
                }

                var yPourcentage = '';
                var yPourcentageFloat = 0;
                var yFloat = 0;
                var dataLabelVisible = true;
                var nbDataLabelVisible = 0;
                var jColor = 0;

                //paramètres spécifiques pour les stats
                if (bStat) {
                    enable3D = false;
                    explode = true;
                    commonSeriesOptions.explode = true;
                    chartLegend.alignment = 'Center';
                    chartType = SYNCFUSIONCHART.PIE;
                    commonSeriesOptions.type = SYNCFUSIONCHART.PIE;
                    chartMarker.dataLabel = {
                        visible: true,
                        shape: 'none',
                        connectorLine: { type: 'bezier', height: 33 },
                        font: { size: '16px' }
                    };
                    labelPosition = 'OutsideExtended';
                }

                var oSerieParams = {};
                // Paramètres en commun pour les 2 graphiques
                oSerieParams.typeGraphique = typeGraphe;
                oSerieParams.globalSelection = globalSelection;
                oSerieParams.bPercentVal = bPercentVal;
                oSerieParams.bCampagneMail = bCampagneMail;
                oSerieParams.enableNullY = enableNullY;
                oSerieParams.bShowValues = bShowValues;
                oSerieParams.decimals = decimals;
                oSerieParams.bK = bK;
                oSerieParams.useThemeColor = useThemeColor;
                oSerieParams.chartMarker = chartMarker;
                oSerieParams.explode = explode;
                oSerieParams.labelPosition = labelPosition;
                oSerieParams.seriesOptionsEnableAnimation = seriesOptionsEnableAnimation;
                oSerieParams.chartThemeColor = chartThemeColor;
                oSerieParams.bAddGraphOpacity = bAddGraphOpacity;
                oSerieParams.chartOpacityValue = chartOpacityValue;
                oSerieParams.colors = colors;


                oSerieParams.zeroSize = zeroSize;

                switch (typeGraphe) {
                    case TYPEGRAPH.SIMPLE_GRAPHE:
                        if (sections.length > 0) {
                            oSerieParams.sections = sections;
                            oSerieParams.xAxisName = xAxisName;
                            oSerieParams.totalValues = totalValues;
                            oSerieParams.sommeSerie = parseFloat(totalValues).toFixed(20);
                            oSerieParams.chartType = chartType;
                            oSerieParams.usePalette = usePalette;

                            var oResChart = SetChartSerie(oSerieParams, gridData);
                            gridData = oResChart.gridData;

                            if (addDoughnutSize)
                                oResChart.serieItem.doughnutSize = 0.9;

                            if (chartType == SYNCFUSIONCHART.FUNNEL) {
                                oResChart.serieItem.funnelHeight = '20%';
                                oResChart.serieItem.funnelWidth = '15%';
                            }
                            //Ajout la série à la collection
                            series.push(oResChart.serieItem);
                        }
                        break;
                    case TYPEGRAPH.COMPLEXE_GRAPHE:
                        commonSeriesOptions.tooltip.format = '#series.name# : #point.y#';

                        if (dataset.length > 0) {
                            if (!stack100Percent) {
                                for (var j = 0; j < categorie.length; j++) {
                                    y = parseFloat(getAttributeValue(categorie[j], 'total')).toFixed(20);
                                    y = parseInt((y == '') ? 0 : y);
                                    if (max < y)
                                        max = y;

                                    if (min > y)
                                        min = y;
                                }
                            }

                            for (var j = 0; j < dataset.length; j++) {
                                chartPoints = [];

                                sections = dataset[j].getElementsByTagName("set");
                                var nbSections = sections.length;
                                if (nbSections > 0) {

                                    for (var i = 0; i < nbSections; i++) {
                                        dataLabelVisible = true;
                                        serieSomme = 0;
                                        text = '';
                                        y = '';
                                        x = '';
                                        link = '';
                                        //BSE: bug #72 730 // Attente validation PO
                                        //serieSomme = parseInt(parseFloat(getAttributeValue(categorie[i], 'total')).toFixed(decimals));
                                        //y = parseFloat(getAttributeValue(sections[i], 'value').replace(',', '.')).toFixed(decimals);

                                        serieSomme = parseInt(parseFloat(getAttributeValue(categorie[i], 'total')).toFixed(20));
                                        y = parseFloat(getAttributeValue(sections[i], 'value').replace(',', '.')).toFixed(20);
                                        y = parseInt((y == '') ? 0 : y);

                                        if (isNaN(y))
                                            y = 0;

                                        if ((dataset.length * nbSections) > 50 && limiteNbMax && y <= ((max - min) / 50))
                                            dataLabelVisible = false;

                                        if (maxSerieSomme < serieSomme)
                                            maxSerieSomme = serieSomme;

                                        if (stack100Percent) {
                                            if (y != 0) {
                                                if (showPercentValues) {
                                                    y = Math.round(((y / serieSomme) * 100) * 100) / 100;
                                                    text = y + ' %';
                                                } else {
                                                    text = y.toString().replace(/\B(?=(\d{3})+(?!\d))/g, " ");
                                                    y = Math.round(((y / serieSomme) * 100) * 100) / 100;
                                                }
                                            } else
                                                text = ' ';
                                        }
                                        else if (showPercentValues) {
                                            if (y != 0)
                                                text = (Math.round(((y / serieSomme) * 100) * 100) / 100) + ' %';
                                            else
                                                text = ' ';
                                        }
                                        else
                                            text = y;

                                        if (text == '')
                                            text = y;
                                        link = getAttributeValue(sections[i], 'link');
                                        link = (link == '') ? link : link.split('goNavF-')[1];

                                        if (categorie.length > 0 &&
                                            categorie[i] &&
                                            ((getAttributeValue(categorie[i], 'valuelabel') + "_" + getAttributeValue(dataset[j], "seriesId")) == getAttributeValue(sections[i], 'serieAlias') || serieSomme)) {
                                            x = getAttributeValue(categorie[i], 'label').split('$#$')[0];
                                        }

                                        //ALISTER Demande / Request 84244, On retire la limitation de l'affichage de la valeur maximale au lieu de diviser par le %, ce qui fait que les petites
                                        //valeurs seront toujours appliqué dans le graphe, à décommenter si besoin
                                        //We remove the limitation of the max's value display instead of divide by %, so little value can always be displayed in the graph
                                        //uncomment it if needed
                                        /*if (bK && ((stack100Percent && !showPercentValues) || !stack100Percent) && max > 10000) {
                                            text = y.toString().replace(/\B(?=(\d{3})+(?!\d))/g, " ");
                                            y = Math.round((y / 10)) / 100;
                                        }*/

                                        if (y == 0 && enableNullY)
                                            y = null;
                                        else if (y == 0 && !enableNullY)
                                            text = ' ';

                                        if (text == '0' || text == '0%' || text == '0K')
                                            text = ' ';

                                        //Si on a pas coché 'Afficher les valeurs' dans la configuration
                                        if (!bShowValues)
                                            text = ' ';

                                        //Pour les libélés, on ajouté un caractere vide parceque dans le cas ou le libéllé est numéric, l'interval est calculé par syncfusion ce qui peut donnée des interval à décimal 
                                        chartPoints.push({
                                            x: ' ' + x,
                                            y: y,
                                            text: text,
                                            link: link,
                                            marker: { dataLabel: { visible: dataLabelVisible } }
                                        });
                                    }
                                }

                                var serieItem = {
                                    points: chartPoints,
                                    marker: chartMarker,
                                    name: dataset[j].getAttribute("seriesName"),
                                    explode: explode,
                                    enableAnimation: true,
                                    labelPosition: labelPosition,
                                    emptyPointSettings: { visible: false },
                                    opacity: bAddGraphOpacity ? chartOpacityValue : 1,
                                    enableSmartLabels: false
                                };

                                if (addDoughnutSize)
                                    serieItem.doughnutSize = 0.9;
                                //Ajout la série à la collection
                                series.push(serieItem);
                            }
                        }

                        break;
                    case TYPEGRAPH.COMBINED_GRAPH:
                        //Option Ajuster l'echelle pour le graphique combiné
                        if (!displayZaxe)
                            chartAxe = [
                                {
                                    majorGridLines: { visible: false },
                                    orientation: 'Vertical',
                                    opposedPosition: true,
                                    axisLine: { visible: true },
                                    rangePadding: 'normal',
                                    name: 'yAxis',
                                    labelFormat: (combinedBk ? "{value}K" : '{value}'),
                                    title: { text: CombinedZyAxisName, enableTrim: true, font: primaryXAxisFont }
                                }];
                        else {

                            primaryYAxis.title.text += ' VS ' + CombinedZyAxisName;
                            var maxVal = parseFloat(oRes.getElementsByTagName("chart")[0].getAttribute("maxVal"));
                            var minVal = parseFloat(oRes.getElementsByTagName("chart")[0].getAttribute("minVal"));
                            var CombinedZmaxVal = parseFloat(oRes.getElementsByTagName("chart")[0].getAttribute(top._CombinedZ + "maxVal"));
                            var CombinedZminVal = parseFloat(oRes.getElementsByTagName("chart")[0].getAttribute(top._CombinedZ + "minVal"));

                            if (minVal > CombinedZminVal)
                                minVal = (CombinedZminVal < 0 ? CombinedZminVal : 0);
                            else
                                if (minVal > 0)
                                    minVal = 0;

                            if (maxVal < CombinedZmaxVal)
                                maxVal = CombinedZmaxVal;

                            //multiplier la maxval *1.1 pour permettre à syncfusion d'afficher la plus grande valeur 
                            primaryYAxis.range = { min: minVal, max: maxVal * 1.1 };
                        }




                        if (sections.length > 0 || sectionsCombined.length > 0) {
                            // Graphique histogramme
                            if (sections.length > 0) {
                                oSerieParams.sections = sections;
                                oSerieParams.xAxisName = xAxisName;
                                oSerieParams.totalValues = totalValues;
                                oSerieParams.sommeSerie = parseFloat(totalValues).toFixed(20);
                                oSerieParams.chartType = SYNCFUSIONCHART.COLUMN;
                                oSerieParams.fillColor = '#E94649';

                                var oResChart = SetChartSerie(oSerieParams, gridData);
                                gridData = oResChart.gridData;
                                //Ajout la série à la collection
                                series.push(oResChart.serieItem);
                            }

                            // graphique lineaire
                            if (sectionsCombined.length > 0) {
                                oSerieParams.bK = combinedBk;
                                oSerieParams.sections = sectionsCombined;
                                oSerieParams.totalValues = totalCombinedValues;
                                oSerieParams.sommeSerie = parseFloat(totalCombinedValues).toFixed(20);
                                oSerieParams.xAxisName = CombinedZxAxisName;
                                oSerieParams.chartType = SYNCFUSIONCHART.LINE;
                                oSerieParams.fillColor = '#F6B53F';
                                oSerieParams.yAxisName = 'yAxis';
                                oSerieParams.Shape = seriesOptionsMarkerShape;
                                oSerieParams.ShapeSize = seriesOptionsMarkerShapeSize;

                                oResChart = SetChartSerie(oSerieParams, gridData);
                                gridData = oResChart.gridData;
                                //Ajout la série à la collection
                                series.push(oResChart.serieItem);
                            }
                        }
                        break;
                }

                //Si le graphique n'est pas 3D
                if (!enable3D) {
                    rotation = 0;
                    depth = 0;
                    wallsize = 0;
                    tilt = 0;
                    perspectiveAngle = 0;
                }
                else if (typeGraphe == TYPEGRAPH.SIMPLE_GRAPHE)
                    depth = 60;
                //si on a le descid de la table 
                if (parseInt(idTableStat)) {
                    var divIdChart = divChartSelector.replace('#', '');
                    setAttributeValue(document.getElementById(divIdChart), 'ntab', idTableStat);

                    if (bStat)
                        setAttributeValue(document.getElementById(divIdChart), 'bStat', 1);
                }

                if (chartType == SYNCFUSIONCHART.CIRCULARGAUGE) {
                    if (oRes.getElementsByTagName("chart")[0].getAttribute("totalValues") != null && cgvtype == CIRCULARGAUGEVALUETYPE.DYNAMICVALUE)
                        totalValues = parseFloat(oRes.getElementsByTagName("chart")[0].getAttribute("totalValues").replace(',', '.')).toFixed(20) * 1;

                    if (parseInt(CombinedZidTableStat) && getAttributeValue(document.getElementById(divIdChart), 'ntab') == '') {
                        var divIdChart = divChartSelector.replace('#', '');
                        setAttributeValue(document.getElementById(divIdChart), 'ntab', CombinedZidTableStat);
                    }

                    var fontFamily = "Verdana,Geneva,sans-serif";
                    var Max = (totalValues == 0 ? 1 : totalValues);
                    var origMaximumPercent = 100;
                    var dynamicMaximumPercent = origMaximumPercent;
                    //position de l'aiguille => totalCombinedValues
                    //Maximum de l'echelle => totalValues
                    if (cgvtype == CIRCULARGAUGEVALUETYPE.FIXEDVALUE)
                        Max = cgvfixedvalue * 1;
                    if (Max == 0)
                        Max = 1;
                    dynamicMaximumPercent = (parseFloat(totalCombinedValues.replace(',', '.')).toFixed(20) * 100) / Max;

                    origMaximumPercent = Math.ceil((dynamicMaximumPercent > origMaximumPercent) ? dynamicMaximumPercent : origMaximumPercent);
                    var distanceFromScale = 40;
                    var w = $(divChartSelector).width();
                    var h = $(divChartSelector).height();
                    var r = w / 2;
                    var customLabelPosition = { xx: w / 2, yy: (h / 2) + 25 };
                    if (w > h)
                        r = h / 2;

                    var rangeWidth = (r > 30 ? r / 2 : r);
                    var ranges = GetRangeForCircularGauge(cgintervals, Max, rangeWidth, distanceFromScale, origMaximumPercent);
                    if (!bFromWidget) {
                        $(divChartSelector).height(r * 2);
                        $(divChartSelector).width(r * 2);
                        $(divChartSelector).css("margin", "0 auto");
                        customLabelPosition = { xx: r, yy: r };
                    }

                    var locale = (typeof top._resChart != 'undefined' && top._resChart != null && top._resChart != '') ? top._resChart : 'en-US';

                    var circularGaugeCustomLabels = [];
                    if (bShowValues) {
                        var displayVal = formatedTotalCombinedValues;
                        //if (thousandSeparator != ' ' || decimalSeparator != ',') {
                        //    displayVal = format(totalCombinedValues, thousandSeparator);
                        //    if (decimalSeparator != ',')
                        //        displayVal = displayVal.replace(',', decimalSeparator);
                        //}

                        circularGaugeCustomLabels.push(
                            {
                                value: displayVal + (isNaN(dynamicMaximumPercent) ? '' : ' (' + dynamicMaximumPercent.toLocaleString(locale) + '%)'),
                                position: { x: customLabelPosition.xx, y: customLabelPosition.yy * 1.65 },
                                font: { size: "16px", fontFamily: fontFamily },
                                color: chartTitle.font.color
                            });
                    }

                    circularGaugeCustomLabels.push({
                        value: chartTitle.text,
                        color: chartTitle.font.color,
                        font: { size: "16px", fontFamily: fontFamily, fontStyle: chartTitle.font.fontWeight },
                        position: { x: customLabelPosition.xx, y: customLabelPosition.yy - (r * 0.8) }
                        //positionType: ej.datavisualization.CircularGauge.CustomLabelPositionType.Outer
                    });


                    var scales = [{
                        showRanges: true,
                        startAngle: 180,
                        sweepAngle: 180,
                        radius: (r < 75 ? r + 20 : r),
                        showScaleBar: false,
                        size: 1,
                        border: { width: 0.5 },
                        maximum: origMaximumPercent,
                        pointers: [{
                            //value: pointerValue,
                            showBackNeedle: true,
                            //backNeedleLength: r / 8,
                            length: r - distanceFromScale,
                            //length: 20,
                            width: 7,
                            //pointerCap: { radius: r / 8 }
                        }],
                        customLabels: circularGaugeCustomLabels,
                        labels: [{ fontFamily: fontFamily, distanceFromScales: 100, unitText: " %" }],

                        ticks: [{
                            type: "major",
                            distanceFromScale: distanceFromScale,
                            height: 16,
                            width: 1,
                            color: "#8c8c8c"
                        }
                        ],
                        ranges: ranges
                    }];

                    if (origMaximumPercent <= 100)
                        scales[0].majorIntervalValue = 20;
                    else {
                        var int = Math.ceil(dynamicMaximumPercent / 5);
                        scales[0].majorIntervalValue = int;
                    }

                    $(divChartSelector).ejCircularGauge({
                        height: document.getElementById(divIdChart).clientHeight * 2,
                        //width: 400,
                        value: dynamicMaximumPercent,
                        enableResize: true,
                        backgroundColor: "transparent",
                        //gaugePosition: "BottomCenter",
                        frame: { frameType: "halfcircle", halfCircleFrameStartAngle: 180 },
                        isResponsive: false,
                        width: $(divChartSelector).width(),
                        height: $(divChartSelector).height(),
                        outerCustomLabelPosition: "top",
                        tooltip: {
                            showCustomLabelTooltip: true
                        },
                        load: function (args) {
                            setWaitChart(false);
                            RemoveSetWait(divChartSelector);
                        },
                        scales: scales
                    });

                    var chart = $(divChartSelector).data("ejCircularGauge");
                    chart.model.initialValue = dynamicMaximumPercent;

                } else {

                    try {
                        // dans un grille inactive - on désactive smartlabels qui fait planter les navigateurs
                        if (document.querySelector(".gw-grid[active='1']  " + divChartSelector) == null
                            && document.querySelector(".gw-grid[active='0']  " + divChartSelector) != null
                        ) {

                            series.forEach(function (a) {
                                a.enableSmartLabels = false;
                            })

                        }
                        else {
                            var grid = document.querySelector(".gw-grid[active='1']  " + divChartSelector);
                            if (grid.style.width == 0 || grid.style.width == '0px') {
                                series.forEach(function (a) {
                                    a.enableSmartLabels = false;
                                })
                            }

                        }
                    }
                    catch (err) {

                    }


                    $(divChartSelector).ejChart({
                        axes: chartAxe,
                        locale: (typeof top._resChart != 'undefined' && top._resChart != null && top._resChart != '') ? top._resChart : 'en-US',
                        primaryXAxis: primaryXAxis,
                        primaryYAxis: primaryYAxis,
                        series: series,
                        commonSeriesOptions: commonSeriesOptions,
                        pointRegionClick: 'onChartPointClick',
                        load: function (args) {
                            setWaitChart(false);
                            RemoveSetWait(divChartSelector);
                        },
                        isResponsive: false,
                        title: chartTitle,
                        enable3D: enable3D,
                        enableRotation: enableRotation,
                        depth: depth,
                        wallSize: wallsize,
                        tilt: tilt,
                        rotation: rotation,
                        perspectiveAngle: perspectiveAngle,
                        canResize: canResize,
                        legend: chartLegend,
                        sideBySideSeriesPlacement: sideBySideSeriesPlacement,
                        crosshair:
                        {
                            visible: false,
                            type: 'trackball',
                            line:
                            {
                                color: 'transparent'
                            },
                            marker: { size: { height: 10, width: 10 } },
                            trackballTooltipSettings: { mode: 'grouping' }
                        }
                    });
                }


                //Inidque que le graphique pour l'élément séléctionné est déjà chargé
                setAttributeValue(mychart, 'isLoaded', '1');

                if (typeof (finalDivChart.data("ejChart")) != 'undefined' && (bStat || fromChartDialog))
                    top.instanceSyncfusionChart = finalDivChart.ejChart("instance");

                // construction de la grille même si le bDisplayGrid est à false; ça permet d'exporter les valeurs du tableau en Excel et PDF
                if (chartSerieType == CHART_SERIES_TYPE.SERIES_TYPE_SIMPLE) {
                    var oDivChartGrid = document.getElementById("DivChartGrid");
                    var oDivChartGridContent = document.getElementById("chartGrid_" + idChart);
                    var oDivChartContent = document.getElementById("DivChart" + idChart);
                    if (!oDivChartGridContent) {
                        oDivChartGridContent = document.createElement('div');
                        oDivChartGridContent.id = "chartGrid_" + idChart;
                        if (oDivChartGrid)
                            oDivChartGrid.appendChild(oDivChartGridContent);
                        else
                            oDivChartContent.parentNode.appendChild(oDivChartGridContent);

                        if (oDivChartGrid) {
                            if (!oDivChartContent)
                                oDivChartContent = document.getElementById(divChart);
                            if (oDivChartContent) {
                                oDivChartGrid.style.height = oDivChartContent.clientHeight + '';
                                oDivChartGrid.style.width = oDivChartContent.clientWidth + '';
                            }
                        }

                        if (oDivChartGrid) {
                            oDivChartGridContent.style.height = oDivChartGrid.style.height;
                            oDivChartGridContent.style.width = oDivChartGrid.style.width;
                        }
                    }

                    //On cache la grid si bDisplayGrid est false
                    if (!bDisplayGrid && oDivChartGridContent)
                        oDivChartGridContent.style.display = 'none';

                    if (typeof oDivChartGrid != 'undefined' && oDivChartGrid != null) {
                        if (typeof CombinedZfldX != 'undefined' && CombinedZfldX != null && CombinedZfldX != '')
                            setAttributeValue(oDivChartGrid, top._CombinedZ + 'fldX', CombinedZfldX);

                        if (typeof CombinedZfldY != 'undefined' && CombinedZfldY != null && CombinedZfldY != '')
                            setAttributeValue(oDivChartGrid, top._CombinedZ + 'fldY', CombinedZfldY);

                        if (typeof fldX != 'undefined' && fldX != null && fldX != '')
                            setAttributeValue(oDivChartGrid, 'fldX', fldX);

                        if (typeof fldY != 'undefined' && fldY != null && fldY != '')
                            setAttributeValue(oDivChartGrid, 'fldY', fldY);

                        if (typeof sValueOperations != 'undefined' && sValueOperations != null && sValueOperations != '')
                            setAttributeValue(oDivChartGrid, 'sValueOperations', operationValue);
                    }
                    else if (typeof oDivChartGridContent != 'undefined' && oDivChartGridContent != null) {
                        if (typeof CombinedZfldX != 'undefined' && CombinedZfldX != null && CombinedZfldX != '')
                            setAttributeValue(oDivChartGridContent, top._CombinedZ + 'fldX', CombinedZfldX);

                        if (typeof CombinedZfldY != 'undefined' && CombinedZfldY != null && CombinedZfldY != '')
                            setAttributeValue(oDivChartGridContent, top._CombinedZ + 'fldY', CombinedZfldY);

                        if (typeof fldX != 'undefined' && fldX != null && fldX != '')
                            setAttributeValue(oDivChartGridContent, 'fldX', fldX);

                        if (typeof fldY != 'undefined' && fldY != null && fldY != '')
                            setAttributeValue(oDivChartGridContent, 'fldY', fldY);

                        if (typeof sValueOperations != 'undefined' && sValueOperations != null && sValueOperations != '')
                            setAttributeValue(oDivChartGridContent, 'sValueOperations', operationValue);
                    }

                    var finalDivGrid = $("#" + oDivChartGridContent.id);
                    //Création objet ejGrid
                    $(function () {
                        finalDivGrid.ejGrid({
                            dataSource: gridData,
                            locale: (typeof top._resChart != 'undefined' && top._resChart != null && top._resChart != '') ? top._resChart : 'en-US',
                            allowSelection: true,
                            allowTextWrap: true,
                            allowPaging: true,
                            pageSettings: { pageSize: 5 },
                            allowSorting: true,
                            isResponsive: true,
                            allowReordering: false,
                            allowRowDragAndDrop: false,
                            enableCanvasRendering: true,
                            selectionType: "multiple",
                            showSummary: showTotalSummary,
                            summaryRows: [{
                                title: total,
                                summaryColumns: [{
                                    summaryType: ej.Grid.SummaryType.Sum,
                                    format: '{0:N' + decimals + '}',
                                    displayColumn: "value",
                                    dataMember: "value"
                                },
                                {
                                    summaryType: ej.Grid.SummaryType.Sum,
                                    format: '{0:N' + CombinedZdecimals + '}',
                                    displayColumn: "combinedvalue",
                                    dataMember: "combinedvalue"
                                }],
                                showTotalSummary: showTotalSummary
                            }],
                            columns: column
                        });

                        if (bStat || fromChartDialog)
                            top.instanceSyncfusionGrid = finalDivGrid.data("ejGrid");
                        if (bDisplayGrid)
                            finalDivGrid.show();
                        else
                            finalDivGrid.hide();
                    });
                }
                else {
                    //on vide instanceSyncfusionGrid qui est utilisé dans les exports
                    if (bStat || fromChartDialog)
                        top.instanceSyncfusionGrid = null;
                }
            }
            else
                setWaitChart(false);

        } catch (e) {
            console.log(e);
        }
    }
    window.name = windowName;

};

//Supprimer le loader si on pas de données ou aprés le chargement du graphique
function RemoveSetWait(div) {
    var c = document.getElementById(div.replace("#", ""));
    if (c) {
        var w = c.querySelector(".xrm-widget-waiter");
        if (w) w.parentElement.removeChild(w);
    }
}

function onChartPointClick(sender) {

    // on utilise un boolean pour empécher la proppagation de l'event, le stopEvent(e) ne fonctionne pas 
    if (typeof sender.model.event.originalEvent.isSecondEvent != 'undefined' && sender.model.event.originalEvent.isSecondEvent)
        return;

    sender.model.event.originalEvent.isSecondEvent = true;
    var serieIndex = sender.data.region.SeriesIndex;

    if (!checkAdminMode()) {
        var listMode = (typeof getCurrentView == "function" && (getCurrentView(document) == "LIST" || getCurrentView(document) == 'FILE_MODIFICATION'));
        var pointIndex = sender.data.region.Region.PointIndex;
        var link = sender.model._visibleSeries[serieIndex]._visiblePoints[pointIndex].link;
        // querySelectorAll retourne une liste de node, on doit donc le transformer en array via le slice
        Array.prototype.slice.apply(top.document.querySelectorAll("div[class^='ejTooltip']")).forEach(
            function (arrElem) {
                arrElem.parentNode.removeChild(arrElem);
            });
        goNavF(link, listMode);
    }
}

function onChartDbClick(sender) {
    if (sender.model.enable3D) {
        return;
    } else {
        if (!checkAdminMode()) {
            var listMode = (typeof getCurrentView == "function" && (getCurrentView(document) == "LIST" || getCurrentView(document) == 'FILE_MODIFICATION'));
            var tab = sender.data.id.split("_");
            var serie = tab[2].replace('Series', '');
            var point = tab[3].replace('Point', '');
            var link = sender.model._visibleSeries[serie]._visiblePoints[point].link;
            Array.prototype.slice.apply(top.document.querySelectorAll("div[class^='ejTooltip']")).forEach(
                function (arrElem) {
                    arrElem.parentNode.removeChild(arrElem);
                });
            goNavF(link, listMode);
        }
    }
}

function UpdateGraph(field, value) {

    top.setWait(true);
    var descidField;
    var operation;
    var finalDivChart = $("#DivChart");
    if (top.instanceSyncfusionChart) {
        if (typeof (finalDivChart.data("ejChart")) == 'undefined')
            finalDivChart.html('');
        if (finalDivChart.hasClass('emtyDataChart'))
            finalDivChart.removeClass('emtyDataChart');
        else {
            top.instanceSyncfusionChart.destroy();

            if (top.instanceSyncfusionGrid) {
                top.instanceSyncfusionGrid.destroy();
            }
        }
    }

    if (field == 'ValuesOperation') {
        operation = value;
        descidField = document.querySelectorAll("select[name*='ValuesField']")[0].value;
    }

    if (field == 'valuesfield') {
        descidField = value;
        operation = document.getElementById("ValuesOperation").value;
    }

    operation = getAgregatFonctionEnum(operation);
    var divChart = document.querySelectorAll("input[ednchartparam]")[0];
    if (divChart)
        sendToUrl(Action.STATCHART, 0, divChart, divChart.getAttribute("displaygrid") == '1', descidField, operation, true)
    else
        top.setWait(false);
}

function UpdateDynamicGraph(nReportId) {
    try {
        top.setWait(true);
        var finalDivChart = $("#DivChart");
        var block = document.getElementById('block');
        if (top.instanceSyncfusionChart != null && top.instanceSyncfusionChart != 'undefined' && top.instanceSyncfusionChart.model != null && (typeof block == 'undefined' || block == null))
            top.instanceSyncfusionChart.destroy();
        else {
            if (finalDivChart.hasClass('emtyDataChart'))
                finalDivChart.removeClass('emtyDataChart');
            if (typeof block != 'undefined' && block != null) {
                if (block.parentElement != null)
                    block.parentElement.removeChild(block);
            }
        }

        if (typeof nReportId != 'undefined' && nReportId != null && typeof (finalDivChart) != 'undefined' && finalDivChart != null) {
            var lstCharts = document.querySelectorAll("input[ednchartparam='" + nReportId + "']");
            for (var i = 0; i < lstCharts.length; i++) {
                var divConfigChart = lstCharts[i];
                var displaygrid = (divConfigChart.getAttribute('displaygrid') == '1');
                //Appel eUpdater
                sendToUrl(Action.GLOBALCHART, nReportId, finalDivChart, displaygrid, undefined, undefined, getExpressFilterValuesParam());
            }

        }
    } catch (e) {
        top.setWait(false);
        eAlert(0, top._res_72, top._res_6474);
        console.log(e);
    }
}

function getExpressFilterValuesParam() {
    var val = '';
    for (var i = 0; i < 3; i++) {
        val += 'value_0_' + i + '|' + getAttributeValue(document.getElementById('value_0_' + i), 'ednvalue') + (i < 2 ? '$|$' : '');
    }
    return encode(val);
}

function getAgregatFonctionEnum(operation, action, bcombined, tab, bCombinedZ) {
    var bstat = false;
    var val = null;
    if (action == Action.STATCHART) {
        bstat = true;
        var element = document.querySelectorAll("select[name*='ValuesField']")[0];
        if (element != null && typeof element != 'undefined' && element.options.length > 0)
            val = element.options[element.selectedIndex].innerText;
    }

    switch (operation) {
        case 'COUNT':
            if (bcombined) {
                if (bCombinedZ) {
                    combinedvalueLabel = top._res_437 + ' (' + tab + ')';
                    combinedpercentLabel = top._res_6228 + ' (' + tab + ')';
                }
                else {
                    percentLabel = top._res_6228 + ' (' + tab + ')';
                    valueLabel = top._res_437 + ' (' + tab + ')';
                }
            }
            else
                valueLabel = top._res_437;

            return TYPEAGREGATFONCTION.COUNT;
            break;
        case 'SUM':
            if (bcombined) {
                if (bCombinedZ) {
                    combinedvalueLabel = top._res_633 + ' (' + tab + ')';
                    combinedpercentLabel = top._res_6228 + ' (' + tab + ')';
                }
                else {
                    percentLabel = top._res_6228 + ' (' + tab + ')';
                    valueLabel = top._res_633 + ' (' + tab + ')';
                }
            }
            else
                valueLabel = top._res_633 + (bstat ? ' (' + val + ')' : '');
            return TYPEAGREGATFONCTION.SUM;
            break;
        case 'AVG':
            if (bcombined) {
                if (bCombinedZ) {
                    combinedvalueLabel = top._res_634 + ' (' + tab + ')';
                    combinedpercentLabel = top._res_6228 + ' (' + tab + ')';
                }
                else {
                    percentLabel = top._res_6228 + ' (' + tab + ')';
                    valueLabel = top._res_634 + ' (' + tab + ')';
                }
            }
            else
                valueLabel = top._res_634 + (bstat ? ' (' + val + ')' : '');
            return TYPEAGREGATFONCTION.AVG;
            break;
        case 'MAX':
            if (bcombined) {
                if (bCombinedZ) {
                    combinedvalueLabel = top._res_636 + ' (' + tab + ')';
                    combinedpercentLabel = top._res_6228 + ' (' + tab + ')';
                }
                else {
                    percentLabel = top._res_6228 + ' (' + tab + ')';
                    valueLabel = top._res_636 + ' (' + tab + ')';
                }
            }
            else
                valueLabel = top._res_636 + (bstat ? ' (' + val + ')' : '');
            return TYPEAGREGATFONCTION.MAX;
            break;
        case 'MIN':
            if (bcombined) {
                if (bCombinedZ) {
                    combinedvalueLabel = top._res_635 + ' (' + tab + ')';
                    combinedpercentLabel = top._res_6228 + ' (' + tab + ')';
                }

                else {
                    percentLabel = top._res_6228 + ' (' + tab + ')';
                    valueLabel = top._res_635 + ' (' + tab + ')';
                }
            }
            else
                valueLabel = top._res_635 + (bstat ? ' (' + val + ')' : '');
            return TYPEAGREGATFONCTION.MIN;
            break;
        default:
            return TYPEAGREGATFONCTION.COUNT;
            break;
    }
}

function changeSelectValue(element) {
    if (element == null || typeof (element) == 'undefined')
        return;

    var valuesOperation = document.getElementById("ValuesOperation");
    var length = valuesOperation.options.length;
    var typeElement = element.options[element.selectedIndex].getAttribute('fmt');
    var action;

    if (typeElement == '3') {
        valuesOperation.selectedIndex = TYPEAGREGATFONCTION.COUNT;
        action = 'none';
    }
    else
        action = 'block';

    for (i = 0; i < length; i++) {
        switch (i) {
            case TYPEAGREGATFONCTION.AVG:
            case TYPEAGREGATFONCTION.MAX:
            case TYPEAGREGATFONCTION.MIN:
                valuesOperation.options[i].style.display = action;
            default:
        }
    }
}

function checkAdminMode() {
    var adminMode = '';
    var divMenuBar = top.document.getElementById('menuBar');
    if (divMenuBar) {
        adminMode = getAttributeValue(divMenuBar, "adminmode");
    }
    if (adminMode == '1')
        return true;
    else
        return false;
}

function setEmptyGraph(divChartSelector) {
    var element = divChartSelector.replace('#', '');
    var div = document.getElementById(element);
    if (!hasClass(div, "emtyDataChart")) {
        setAttributeValue(div, "syncFusionChart", '0');
        div.className += " emtyDataChart";
        div.style.textAlign = "center";
        div.style.height = "75%";
        div.style.width = "100%";
        var iDiv = document.createElement('div');
        iDiv.id = 'block';
        iDiv.className = 'block';
        iDiv.textContent = emptyChartRessource;
        div.appendChild(iDiv);
    }
}

function ReturnError(oRes) {
    top.setWait(false);
    eAlert(0, top._res_72, top._res_422, top._res_544, null, null, function () {
        if (top.modalCharts != null && typeof top.modalCharts != 'undefined')
            top.modalCharts.hide();
    });
}

//Construit un serieItem qu'on va passer à la collection des séries
function SetChartSerie(object, gridData) {

    var chartPoints = [];
    var max = 0, min = 0, y = 0, yFloat = 0, x = '', link = '', yPourcentageFloat = '', text = '', doubleText = 0, yPourcentage = 0, iColor = 0;
    var fillColor;
    //Afficher le datalabel (valeur du point)
    var dataLabelVisible = true;

    for (var i = 0; i < object.globalSelection.length; i++)
        chartPoints.push(object.globalSelection[i]);

    for (var i = 0; i < object.sections.length; i++) {

        link = getAttributeValue(object.sections[i], 'link');
        link = (link == '') ? link : link.split('goNavF-')[1];
        x = getAttributeValue(object.sections[i], 'label');
        y = getAttributeValue(object.sections[i], 'value').replace(',', '.');
        yPourcentage = parseFloat(getAttributeValue(object.sections[i], 'percent').replace(',', '.')).toFixed(20);
        yFloat = parseFloat((y == '') ? 0 : y);
        y = parseInt(yFloat);

        switch (object.typeGraphique) {
            case TYPEGRAPH.COMBINED_GRAPH:
                if (gridData.length > 0) {
                    var exist = false;
                    var lineIndex;

                    gridData.forEach(function (col, index) {
                        if (col.name == x) {
                            exist = true;
                            lineIndex = index;
                            col.combinedvalue = yFloat;
                            col.combinedpercent = Math.abs(yPourcentage);
                        }
                    });

                    if (!exist) {
                        if (typeof object.yAxisName != 'undefined')
                            gridData.push({ name: x, value: 0, percent: '0.00', combinedvalue: yFloat, combinedpercent: Math.abs(yPourcentage) });
                        else gridData.push({ name: x, value: yFloat, percent: Math.abs(yPourcentage), combinedvalue: 0, combinedpercent: '0.00' });
                    }

                } else {
                    if (typeof object.yAxisName != 'undefined')
                        gridData.push({ name: x, value: 0, percent: '0.00', combinedvalue: yFloat, combinedpercent: Math.abs(yPourcentage) });
                    else
                        gridData.push({ name: x, value: yFloat, percent: Math.abs(yPourcentage), combinedvalue: 0, combinedpercent: '0.00' });
                }
                break;
            case TYPEGRAPH.SIMPLE_GRAPHE:
                gridData.push({ name: x, value: yFloat, percent: Math.abs(yPourcentage) });
                break;
            default:
        }

        if (object.bPercentVal)
            dataLabelVisible = (getAttributeValue(object.sections[i], 'labelValueVisible') == "1");

        if (object.bCampagneMail)
            dataLabelVisible = true;

        //Néttoyage de x
        x = x.split('$#$')[0];

        //Si le graphique simple peut afficher des valeurs en pourcentage et qu'on a cocher l'option dans l'assistant
        // cette option s'applique sur les graphiques : DOUGHNUT_2D,DOUGHNUT_3D,SEMI_DOUGHNUT_2D ,FPIE_2D,PIE_3D,SEMI_PIE_2D:
        text = getAttributeValue(object.sections[i], 'text');
        if (text != '')
            text = '' + (object.bCampagneMail ? x + ': ' : '') + text;
        else
            text = ' ';

        if (y == 0 && object.enableNullY)
            y = null;

        if (parseFloat(yPourcentageFloat * 100) > 1 && yFloat < 1 && yFloat > -1)
            y = yFloat;

        if (!object.bShowValues)
            text = ' ';

        //Pour les libélés, on ajouté un caractere vide parceque dans le cas ou le libéllé est numéric, l'interval est calculé par syncfusion ce qui peut donnée des interval à décimal 
        var sPoint = { x: ' ' + x, y: object.bK ? y / 1000 : y, text: text, link: link, marker: { dataLabel: { visible: dataLabelVisible } } };
        if (object.typeGraphique == TYPEGRAPH.COMBINED_GRAPH) {
            var index = chartPoints.indexOf(x);
            chartPoints[index] = sPoint;
        }
        else {
            if (object.usePalette) {
                if (!object.useThemeColor) {

                    //Dans le cas d'un graphique simple , on gère dynamiquement la couleur de la série
                    if (object.typeGraphique == TYPEGRAPH.SIMPLE_GRAPHE && y != null && y > 0) {
                        if (iColor > object.colors.length)
                            iColor = 0;
                        sPoint.fill = object.colors[iColor];
                        iColor++;
                    }
                }

            }
            chartPoints.push(sPoint);
        }


    }

    var serieItem = {
        points: chartPoints,
        marker: object.chartMarker,
        explode: object.explode,
        name: object.xAxisName,
        type: object.chartType,
        enableAnimation: object.seriesOptionsEnableAnimation,
        labelPosition: object.labelPosition,
        fill: (!object.useThemeColor ? object.fillColor : object.chartThemeColor),
        emptyPointSettings: {
            visible: false,
        }, opacity: object.bAddGraphOpacity ? object.chartOpacityValue : 1,
        enableSmartLabels: !object.zeroSize,
        tooltip: {
            visible: true,
            format: "#series.name# :#point.x# " + (object.bPercentVal ? "(ej.format(#point.y#,n" + object.decimals + "))" : "")
        }
    };
    //#65 426
    if (TYPEGRAPH.SIMPLE_GRAPHE && object.bCampagneMail)
        serieItem.tooltip.format = "#point.x#: #point.y#";

    if (object.typeGraphique == TYPEGRAPH.COMBINED_GRAPH) {
        for (var i = 0; i < chartPoints.length; i++) {
            if (typeof chartPoints[i] != 'object')
                chartPoints[i] = { x: ' ' + chartPoints[i].split('$#$')[0], y: 0, text: ' ', dataLabel: { visible: false } };
        }

        if (typeof object.yAxisName != 'undefined' && object.chartType == SYNCFUSIONCHART.LINE) {
            serieItem.yAxisName = object.yAxisName;
            serieItem.marker =
            {
                shape: object.Shape,
                size: object.ShapeSize,
                visible: true
            };
        }
    }

    //Nétoyage valeur du tableau
    gridData.forEach(function (element, index) {
        if (element.name.indexOf('$#$') > 0) {
            element.name = element.name.split('$#$')[0];
        }
    });

    var oRes = {};
    oRes.serieItem = serieItem;
    oRes.gridData = gridData;
    return oRes;
}

//Tri de l'array de date de la plus ancienne à la plus récente
function SetDateTimeArraySort(ar) {
    ar.sort(function (a, b) {
        var pA = a.split("/");
        if (pA.length < 3)
            pA.push('01');
        var dateA = new Date(Number(pA[1]), Number(pA[0]) - 1, Number(pA[2]));
        var pB = b.split("/");
        if (pB.length < 3)
            pB.push('01');

        var dateB = new Date(Number(pB[1]), Number(pB[0]) - 1, Number(pB[2]));

        if (dateA.getTime() > dateB.getTime()) return 1;
        if (dateA.getTime() < dateB.getTime()) return -1;
        return 0;
    });
}

// #BEGIN REGION Traitement CHART#
//Affiche le menu des exports sur les graphiques
function dispExportMenu(oExtBtnDiv, modalCharts) {
    var fromModal = false;
    var fromHomePAge = (getAttributeValue(oExtBtnDiv, 'fHome') == '1');
    var circularGauge = (getAttributeValue(oExtBtnDiv, 'cg') == '1');
    var pos = (fromHomePAge ? "UNDER" : "BEFORE");
    var offsets = oExtBtnDiv.getBoundingClientRect();
    var offsetsTop = offsets.top;
    var offsetsLeft = offsets.left;
    var element = getAttributeValue(oExtBtnDiv, 'load');
    var idDivChart = 0;
    var hideExcel = getAttributeValue(oExtBtnDiv, 'Excel');
    var myTopAdjust = 0;
    var myLeftAdjust = 5;

    if (!fromHomePAge) {
        myTopAdjust = 30;
        if (top.modalCharts != null
            && typeof top.modalCharts != 'undefined'
            && top.modalCharts.IsAvailable()
            && (typeof modalCharts == 'undefined' || modalCharts == null))
            modalCharts = top.modalCharts.getIframe();
    }

    if (typeof modalCharts != 'undefined' && modalCharts != null) {
        idDivChart = getAttributeValue(modalCharts.document.querySelector('[ednchartparam]'), 'ednchartparam');
        fromModal = true;
    }
    else
        idDivChart = (element != null && typeof element != 'undefined') ? element.replace('_canvas', '').split('DivChart')[1] : '0';

    //Position initiale hors écran
    oExportMenu = new eContextMenu(null, -999, -999, null, null, "contextChartMenu");
    addClass(oExtBtnDiv, " advFltPressed ");

    if ((typeof ($('#DivChart' + idDivChart).data("ejChart")) != 'undefined' || fromModal) && !circularGauge) {
        oExportMenu.addItemFct(top._res_8231, function () { downloadChart(element, CHART_EXPORT_TYPE.PDF, modalCharts); }, 1, 0, "advFltItem NewFlt", null, "icnFlt icon-file-pdf-o");
        oExportMenu.addSeparator(1);
        oExportMenu.addItemFct(top._res_8232, function () { downloadChart(element, CHART_EXPORT_TYPE.PNG, modalCharts); }, 1, 0, "advFltItem FltList", null, "icnFlt icon-file-image-o");
        if (hideExcel != '1') {
            oExportMenu.addSeparator(1);
            oExportMenu.addItemFct(top._res_8233, function () { downloadChart(element, CHART_EXPORT_TYPE.XLS, modalCharts); }, 1, 0, "advFltItem FltList canBeHiden", null, "icnFlt icon-file-excel-o");
        }

        // drill down => affichage liste complète
        if (idDivChart != null && typeof idDivChart != 'undefined' && idDivChart != '0' && !isNaN(parseInt(idDivChart))) {
            oExportMenu.addSeparator(1);
            oExportMenu.addItemFct(top._res_8417, function () { goNav(parseInt(idDivChart), ''); }, 1, 0, "advFltItem FltList", null, "icnFlt icon-edn-list");
        }
    }
    else {
        if (circularGauge || typeof ($('#DivChart' + idDivChart).data("ejCircularGauge")) != 'undefined') {
            oExportMenu.addItemFct(top._res_8231, function () {
                var canvas;
                var MIME_TYPE = "image/png";
                if (modalCharts != null && typeof modalCharts != 'undefined')
                    canvas = modalCharts.document.getElementsByTagName("canvas")[0];
                else
                    canvas = document.getElementById('DivChart' + idDivChart).getElementsByTagName("canvas")[0];

                downloadCircularGauge(element, canvas, CHART_EXPORT_TYPE.PDF);

            }, 1, 0, "advFltItem NewFlt", null, "icnFlt icon-file-pdf-o");
            oExportMenu.addSeparator(1);

            oExportMenu.addItemFct(top._res_8232, function () {
                var canvas;
                var MIME_TYPE = "image/png";
                if (modalCharts != null && typeof modalCharts != 'undefined')
                    canvas = modalCharts.document.getElementsByTagName("canvas")[0];
                else
                    canvas = document.getElementById('DivChart' + idDivChart).getElementsByTagName("canvas")[0];
                downloadCircularGauge(element, canvas, CHART_EXPORT_TYPE.PNG);

            }, 1, 0, "advFltItem FltList", null, "icnFlt icon-file-excel-o");
            oExportMenu.addSeparator(1);
        }

        // drill down => affichage liste complète
        if (idDivChart != null && typeof idDivChart != 'undefined' && idDivChart != '0' && !isNaN(parseInt(idDivChart)))
            oExportMenu.addItemFct(top._res_8417, function () { goNav(parseInt(idDivChart), ''); }, 1, 0, "advFltItem FltList", null, "icnFlt icon-edn-list");

    }

    oExportMenu.alignElement(oExtBtnDiv, pos, myTopAdjust + "|" + myLeftAdjust, offsetsLeft); // Le menu apparait devant le bouton
    var exportOut;
    var oExportMenuMouseOver = function () {
        exportOut = setTimeout(
            function () {
                //Masque le menu
                if (oExportMenu) {
                    if (oExtBtnDiv.className.indexOf(" advFltActiv ") > -1)
                        oExtBtnDiv.className = oExtBtnDiv.className.replace(" advFltActivPressed ", "");
                    else
                        oExtBtnDiv.className = oExtBtnDiv.className.replace(" advFltPressed ", "");
                    oExportMenu.hide();
                    unsetEventListener(oExportMenu.mainDiv, "mouseout", oExportMenuMouseOver);
                }
            }
            , 200);
    };
    //Annule la disparition
    if (oExportMenu) {
        setEventListener(oExportMenu.mainDiv, "mouseover", function () { clearTimeout(exportOut) });
    }
    if (oExtBtnDiv) {
        setEventListener(oExtBtnDiv, "mouseover", function () { clearTimeout(exportOut); });
    }
    //si on sort de la div de bouton ou de menu, on a 200ms pour se rattraper
    setEventListener(oExtBtnDiv, "mouseout", oExportMenuMouseOver);
    if (oExportMenu) {
        setEventListener(oExportMenu.mainDiv, "mouseout", oExportMenuMouseOver);
    }
}

// Export Graphique
function downloadChart(chartElement, formatExport, modalCharts) {
    try {
        var bStat = 0;
        var nTab = 0;
        var nField = 0;
        var nCombinedField = 0;
        var nOperationType = 0;
        var gridContainer = null;
        var container = null;
        var gridDataSource = null;
        var gridColumns = null;
        var gridParamSorting = null;
        var chartType = null;
        var chartTitle = null;
        var primarySerieAxis = null;
        var primaryValueAxis = null;
        var chartLegendFontSize = "12px";
        var chartTitleFontSize = "16px";
        var chartPrimaryXAxis = "12px";
        var chartPrimaryYAxis = "12px";
        var chartLegendTextWidth = 100;
        var chartSeriesFontSize = "12px";
        var chartPrimaryYAxisTitleFontSize = "12px";
        var chartPrimaryXAxisTitleFontSize = "12px";
        var chartLegendItemStyleHeight = 10;
        var chartLegendItemStyleWidth = 10;
        var chartLegendItemPadding = 10;
        var chartCombinedTitleXAxisFontSize = "12px";
        var chartCombinedFontSize = "12px";
        var idDivChart = chartElement.replace('_canvas', '');
        var idDivGrid = idDivChart.split('DivChart')[1];
        var object = (typeof modalCharts == 'undefined' || modalCharts == null) ? (top.eModFile == 'undefined' || top.eModFile == null ? top : top.eModFile.getIframe()) : modalCharts;
        var bRedraw = (formatExport != CHART_EXPORT_TYPE.XLS);
        var expresFilter = object.document.getElementById('filterExpress');
        var expressFilterLine = new Array();
        var graphObject = {
            object: object,
            enable3D: false,
            chartLegendFontSize: chartLegendFontSize,
            chartTitleFontSize: chartTitleFontSize,
            chartPrimaryYAxis: chartPrimaryYAxis,
            chartPrimaryXAxis: chartPrimaryXAxis,
            chartPrimaryYAxisTitleFontSize: chartPrimaryYAxisTitleFontSize,
            chartPrimaryXAxisTitleFontSize: chartPrimaryXAxisTitleFontSize,
            chartLegendItemStyleHeight: chartLegendItemStyleHeight,
            chartLegendItemStyleWidth: chartLegendItemStyleWidth,
            chartLegendItemPadding: chartLegendItemPadding,
            size: {},
            chartSeriesFontSize: chartSeriesFontSize,
            chartCombinedTitleXAxisFontSize: chartCombinedTitleXAxisFontSize,
            chartCombinedFontSize: chartCombinedFontSize
        };

        gridContainer = object.document.getElementById('DivChartGrid');
        container = object.document.getElementById('DivChart');
        if (typeof container != 'undefined' || container != null) {
            container = object.document.getElementById(idDivChart);
            if (typeof container != 'undefined' && container != null) {
                nTab = getAttributeValue(container, 'ntab');
                bStat = getAttributeValue(container, 'bStat');
            }
        }

        if (bStat == '1' || typeof gridContainer == 'undefined' || gridContainer == null)
            gridContainer = object.document.getElementById('chartGrid_' + (idDivGrid != null && typeof idDivGrid != 'undefined' && idDivGrid.trim() != '' ? idDivGrid : ''));

        if (typeof gridContainer != 'undefined' && gridContainer != null) {

            nField = getAttributeValue(gridContainer, 'fldY');
            nCombinedField = getAttributeValue(gridContainer, top._CombinedZ + 'fldY');
            nOperationType = getAttributeValue(gridContainer, 'sValueOperations');

            if (nTab != '' && !Number.isNaN(parseInt(nTab)) && nField != '' && !Number.isNaN(parseInt(nField))) {
                if ((nField - (nField % 100)) != nTab) {
                    nField = getAttributeValue(gridContainer, 'fldX');

                    if (nCombinedField != '')
                        nCombinedField = getAttributeValue(gridContainer, top._CombinedZ + 'fldX');
                }
            }
        }

        var chartObject = $('#' + idDivChart).ejChart("instance");
        var gridObject = $("#chartGrid_" + idDivGrid).data("ejGrid");

        if (typeof gridObject == 'undefined' || gridObject.model == 'undefined')
            gridObject = top.instanceSyncfusionGrid;

        if (typeof gridObject != 'undefined' && gridObject != null && gridObject.model != null) {
            gridDataSource = gridObject.model.dataSource;
            gridColumns = gridObject.model.columns;
            gridParamSorting = gridObject.model.sortSettings.sortedColumns;
        }

        if (typeof chartObject.model == 'undefined')
            chartObject = top.instanceSyncfusionChart;

        if (typeof expresFilter != 'undefined' && expresFilter != null) {
            var divExpressFilter = expresFilter.querySelectorAll('div[class="divExpressFilter"]');
            for (var i = 0; i < divExpressFilter.length; i++)
                expressFilterLine.push({ txt: divExpressFilter[i].firstChild.innerText, value: divExpressFilter[i].lastChild.firstChild.value, op: getAttributeValue(divExpressFilter[i], 'op') });
        }

        if (chartObject != null && typeof chartObject != 'undefined' && chartObject.model != null) {
            var newPngChartSize = { width: 1920, height: 1080 };
            var newPdfChartSize = { width: 1080, height: 680 };
            var svgHeight = chartObject.model.svgHeight;
            var svgWidth = chartObject.model.svgWidth;
            chartLegendFontSize = chartObject.model.legend.font.size;
            chartTitleFontSize = chartObject.model.title.font.size;
            chartLegendTextWidth = chartObject.model.legend.textWidth;
            chartPrimaryYAxis = chartObject.model.primaryYAxis.font.size;
            chartPrimaryXAxis = chartObject.model.primaryYAxis.font.size;
            chartPrimaryYAxisTitleFontSize = chartObject.model.primaryYAxis.title.font.size;
            chartPrimaryXAxisTitleFontSize = chartObject.model.primaryYAxis.title.font.size;
            chartLegendItemStyleHeight = chartObject.model.legend.itemStyle.height;
            chartLegendItemStyleWidth = chartObject.model.legend.itemStyle.width;
            chartLegendItemPadding = chartObject.model.legend.itemPadding;
            chartSeriesFontSize = chartObject.model.series[0].marker.dataLabel.font.size;
            chartTitle = chartObject.model.title.text;
            graphObject.enable3D = chartObject.model.enable3D;
            chartType = chartObject.model.series[0].type;
            chartObject.model.enableCanvasRendering = true;
            chartObject.model.enable3D = false;

            if (chartObject.model.axes[0] != null && typeof chartObject.model.axes[0] != 'undefined') {
                chartCombinedTitleXAxisFontSize = chartObject.model.axes[0].title.font.size;
                chartCombinedFontSize = chartObject.model.axes[0].font.size;
            }

            if (formatExport == CHART_EXPORT_TYPE.PDF)
                chartObject.model.size = newPdfChartSize;
            else {
                chartObject.model.size = newPngChartSize;
                chartObject.model.size = newPngChartSize;
                chartObject.model.legend.font.size = "18px";
                chartObject.model.legend.textWidth = 300;
                chartObject.model.legend.itemStyle.height = 20;
                chartObject.model.legend.itemStyle.width = 20;
                chartObject.model.legend.itemPadding = 20;
                for (var i = 0; i < chartObject.model.series.length; i++) {
                    chartObject.model.series[i].marker.dataLabel.font.size = "15px"
                }
            }
            chartObject.redraw();
            var imageChart, imageLegend = null;
            var canvasChart = object.document.getElementById(chartElement);
            var canvasLegend = object.document.getElementById('legend_' + chartElement);
            var baseurl = '/' + window.location.pathname.split('/')[1] + '/mgr/eChartManagerExport.ashx';
            var form = CreateElement('form', ["method", "action", "style", "target"], ["post", baseurl, "display:hidden;", "_blank"]);
            var format = CreateElement("input", ["type", "name", "value"], ["hidden", "format", formatExport]);
            form.appendChild(format);
            if (typeof canvasChart == 'undefined' || canvasChart == null)
                return;
            else {
                imageChart = canvasChart.toDataURL("image/png");
                var chartContent = CreateElement("input", ["type", "name", "value"], ["hidden", "dataChart", imageChart]);
                form.appendChild(chartContent);
                if (chartType && chartType != null) {
                    var chartTypeContent = CreateElement("input", ["type", "name", "value"], ["hidden", "chartType", chartType]);
                    form.appendChild(chartTypeContent);
                }
            }

            if (typeof canvasLegend != 'undefined' && canvasLegend != null) {
                imageLegend = canvasLegend.toDataURL("image/png");
                var legendContent = CreateElement("input", ["type", "name", "value"], ["hidden", "dataLegend", imageLegend]);
                form.appendChild(legendContent);
            }

            if (typeof gridObject != 'undefined' && gridObject != null && gridObject.model != null && gridDataSource != null) {
                var gridHeader = CreateElement("input", ["type", "name", "value"], ["hidden", "gridDataColumns", JSON.stringify(gridColumns)]);
                var gridContent = CreateElement("input", ["type", "name", "value"], ["hidden", "gridDataSource", JSON.stringify(gridDataSource)]);
                form.appendChild(gridHeader);
                form.appendChild(gridContent);

                if (gridParamSorting.length > 0) {
                    var gridSorting = CreateElement("input", ["type", "name", "value"], ["hidden", "gridDataSorting", JSON.stringify(gridParamSorting)]);
                    form.appendChild(gridSorting);
                }
            }

            // pour les stats on a besoin de la table
            if (!Number.isNaN(parseInt(nTab)) && parseInt(nTab) > 0) {
                var divNtab = CreateElement("input", ["type", "name", "value"], ["hidden", "nTab", nTab]);
                form.appendChild(divNtab);
            }

            if (!Number.isNaN(parseInt(nField)) && parseInt(nField) > 0) {
                var divNfield = CreateElement("input", ["type", "name", "value"], ["hidden", "nField", parseInt(nField)]);
                form.appendChild(divNfield);
            }

            if (!Number.isNaN(parseInt(nCombinedField)) && parseInt(nCombinedField) > 0) {
                var divNfield = CreateElement("input", ["type", "name", "value"], ["hidden", "nCombinedField", parseInt(nCombinedField)]);
                form.appendChild(divNfield);
            }

            if (!Number.isNaN(parseInt(nOperationType)) && parseInt(nOperationType) >= 0) {
                var divNoperationType = CreateElement("input", ["type", "name", "value"], ["hidden", "nOperationType", nOperationType]);
                form.appendChild(divNoperationType);
            }

            // en cas de statistiques
            if (!Number.isNaN(parseInt(bStat)) && parseInt(bStat) > 0) {
                var bStatElement = CreateElement("input", ["type", "name", "value"], ["hidden", "bStat", bStat]);
                form.appendChild(bStatElement);
            }

            if (formatExport == CHART_EXPORT_TYPE.XLS) {
                primarySerieAxis = (typeof chartObject.model.yAxisTitleRegion != 'undefined' && chartObject.model.yAxisTitleRegion != null && chartObject.model.yAxisTitleRegion.length > 0) ? chartObject.model.yAxisTitleRegion[0].labelText : '';
                primaryValueAxis = (typeof chartObject.model.xAxisTitleRegion != 'undefined' && chartObject.model.xAxisTitleRegion != null && chartObject.model.xAxisTitleRegion.length > 0) ? chartObject.model.xAxisTitleRegion[0].labelText : chartObject.model.series[0].name;

                var chartTitleElement = CreateElement("input", ["type", "name", "value"], ["hidden", "chartTitle", chartTitle]);
                form.appendChild(chartTitleElement);

                var chartPrimarySerieAxis = CreateElement("input", ["type", "name", "value"], ["hidden", "primarySerieAxis", primarySerieAxis]);
                form.appendChild(chartPrimarySerieAxis);

                var chartPrimaryValueAxis = CreateElement("input", ["type", "name", "value"], ["hidden", "primaryValueAxis", primaryValueAxis]);
                form.appendChild(chartPrimaryValueAxis);

                var chart3D = CreateElement("input", ["type", "name", "value"], ["hidden", "chart3D", (graphObject.enable3D ? '1' : '0')]);
                form.appendChild(chart3D);
            }

            if (expressFilterLine.length > 0) {
                var expressFilterElements = CreateElement("input", ["type", "name", "value"], ["hidden", "expressFilterElements", JSON.stringify(expressFilterLine)]);
                form.appendChild(expressFilterElements);
            }
            object.document.body.appendChild(form);
            form.submit();
            object.document.body.removeChild(form);
            actionAfterExport(graphObject, chartObject.model);
            chartObject.redraw();
        }

    } catch (e) {
        actionAfterExport(graphObject, chartObject.model)
        chartObject.redraw();
        eAlert(0, top._res_416, top._res_6474);
        console.log(e);
    }
}


function downloadCircularGauge(chartElement, canvas, formatExport) {

    if (canvas) {
        var idDivChart = chartElement.replace('_canvas', '');
        var container = null;
        var nTab = 0;
        var object = (typeof modalCharts == 'undefined' || modalCharts == null) ? (top.eModFile == 'undefined' || top.eModFile == null ? top : top.eModFile.getIframe()) : (modalCharts.IsAvailable()) ? modalCharts.getIframe() : top;

        container = object.document.getElementById('DivChart');
        if (typeof container == 'undefined' || container == null)
            container = object.document.getElementById(idDivChart);

        if (typeof container != 'undefined' && container != null)
            nTab = getAttributeValue(container, 'ntab');

        if (isNaN(parseInt(nTab)) || parseInt(nTab) <= 0) {
            eAlert(0, top._res_416, top._res_6162);
        } else {
            var MIME_TYPE = "image/png";
            var img = canvas.toDataURL(MIME_TYPE);
            if (formatExport == CHART_EXPORT_TYPE.PNG) {
                var dlLink = document.createElement('a');
                dlLink.download = 'ExportCircularGauge';
                dlLink.href = img;
                dlLink.dataset.downloadurl = [MIME_TYPE, dlLink.download, dlLink.href].join(':');
                document.body.appendChild(dlLink);
                dlLink.click();
                document.body.removeChild(dlLink);
            } else if (formatExport == CHART_EXPORT_TYPE.PDF) {
                var title = container.querySelector('div[class="' + idDivChart + 'outercustomlbl"]')
                var expressFilterLine = new Array();
                var baseurl = '/' + window.location.pathname.split('/')[1] + '/mgr/eChartManagerExport.ashx';
                var form = CreateElement('form', ["method", "action", "style", "target"], ["post", baseurl, "display:hidden;", "_blank"]);
                var chartContent = CreateElement("input", ["type", "name", "value"], ["hidden", "dataChart", img]);
                form.appendChild(chartContent);

                var chartType = CreateElement("input", ["type", "name", "value"], ["hidden", "chartType", "0"]);
                form.appendChild(chartType);
                var format = CreateElement("input", ["type", "name", "value"], ["hidden", "format", formatExport]);
                form.appendChild(format);

                var divNtab = CreateElement("input", ["type", "name", "value"], ["hidden", "nTab", nTab]);
                form.appendChild(divNtab);
                if (title) {
                    var divTitle = CreateElement("input", ["type", "name", "value"], ["hidden", "title", title.innerText]);
                    form.appendChild(divTitle);
                }


                var expresFilter = object.document.getElementById('filterExpress');
                if (typeof expresFilter != 'undefined' && expresFilter != null) {
                    var divExpressFilter = expresFilter.querySelectorAll('div[class="divExpressFilter"]');
                    for (var i = 0; i < divExpressFilter.length; i++) {
                        var value = divExpressFilter[i].lastChild.firstChild.value;
                        var txtFilter = divExpressFilter[i].firstChild.innerText;
                        var operator = getAttributeValue(divExpressFilter[i], 'op');
                        var obj = GetEncodeValueForChartExport(value);

                        expressFilterLine.push({ FilterTxt: txtFilter, FilterValue: obj.value, FilterOperator: operator, ValueEncoded: obj.encode });
                    }

                }
                if (expressFilterLine.length > 0) {
                    var expressFilterElements = CreateElement("input", ["type", "name", "value"], ["hidden", "expressFilterElements", JSON.stringify(expressFilterLine)]);
                    form.appendChild(expressFilterElements);
                }

                document.body.appendChild(form);
                form.submit();
                document.body.removeChild(form);
            }
        }

    } else {
        eAlert(0, top._res_416, top._res_6162);
    }
}

//Action à exécuter apès l'export
function actionAfterExport(graphObject, model) {
    model.enableCanvasRendering = false;
    model.enable3D = graphObject.enable3D;
    model.legend.font.size = graphObject.chartLegendFontSize;
    model.title.font.size = graphObject.chartTitleFontSize;
    model.primaryYAxis.font.size = graphObject.chartPrimaryYAxis;
    model.primaryXAxis.font.size = graphObject.chartPrimaryXAxis;
    model.primaryYAxis.title.font.size = graphObject.chartPrimaryYAxisTitleFontSize
    model.primaryXAxis.title.font.size = graphObject.chartPrimaryXAxisTitleFontSize
    model.legend.itemStyle.height = graphObject.chartLegendItemStyleHeight;
    model.legend.itemStyle.width = graphObject.chartLegendItemStyleWidth;
    model.legend.itemPadding = graphObject.chartLegendItemPadding;
    model.size = graphObject.size;

    for (var i = 0; i < model.series.length; i++) {
        model.series[i].marker.dataLabel.font.size = graphObject.chartSeriesFontSize;
    }

    if (model.axes[0] != null && typeof model.axes[0] != 'undefined') {
        model.axes[0].title.font.size = graphObject.chartCombinedTitleXAxisFontSize;
        model.axes[0].font.size = graphObject.chartCombinedFontSize;
    }
}

function CreateElement(element, atr, value) {
    var ele = document.createElement(element);
    for (var i = 0; i < atr.length; i++) {
        ele.setAttribute(atr[i], value[i]);
    }
    return ele;
}

// #END REGION EXPORT CHART#


function GetRangeForCircularGauge(cgintervals, cgvfixedvalue, rangeWidth, distanceFromScale, percentMax) {
    var ranges = [];
    var cgintervalsTab = cgintervals.split(';');
    var intCgvfixedvalue = parseInt(cgvfixedvalue);
    var defaultColor = colors[0];
    var intervalColor = defaultColor;

    if (cgintervalsTab.length > 0 && !isNaN(intCgvfixedvalue)) {
        var minVal = 0;
        var maxVal = percentMax;

        for (var i = 0; i < cgintervalsTab.length; i++) {
            if (minVal >= percentMax)
                continue;
            var intervalsParam = cgintervalsTab[i].split(',');

            if (intervalsParam.length == 2 && minVal < percentMax) {
                var valMax = (isNaN(parseInt(intervalsParam[0])) ? maxVal : parseInt(intervalsParam[0]));

                var valMin = minVal;
                intervalColor = intervalsParam[1];
                if (valMin < valMax) {
                    ranges.push({

                        startWidth: rangeWidth,
                        endWidth: rangeWidth,
                        distanceFromScale: distanceFromScale,
                        startValue: valMin,
                        endValue: valMax,
                        backgroundColor: intervalColor,
                        border: { color: intervalColor }

                    });
                    minVal = valMax;
                }
            }
        }

        if (percentMax > minVal)
            ranges.push({
                startWidth: rangeWidth,
                endWidth: rangeWidth,
                distanceFromScale: distanceFromScale,
                startValue: minVal,
                endValue: percentMax,
                backgroundColor: intervalColor,
                border: { color: intervalColor }
            })

    } else ranges = [{
        startWidth: rangeWidth,
        endWidth: rangeWidth,
        distanceFromScale: distanceFromScale,
        startValue: 0,
        endValue: 100,
        backgroundColor: defaultColor,
        border: { color: defaultColor }
    }];

    return ranges;
}


function GetEncodeValueForChartExport(FilterValue) {
    var obj = { value: FilterValue, encode: false };
    if (FilterValue != '' && (FilterValue.indexOf('<') != -1 || FilterValue.indexOf('>') != -1)) {
        obj.encode = true;
        obj.value = encode(FilterValue);
    }
    return obj;
}

//Ne fonctionne pas sur IE
//function format(num, thousandSeparator) {
//    const n = String(num),
//          p = n.indexOf('.')
//    return n.replace(
//        /\d(?=(?:\d{3})+(?:\.|$))/g,
//        (m, i) => p < 0 || i < p ? `${m}` + thousandSeparator : m
//    )
//}



