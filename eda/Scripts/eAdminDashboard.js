var nsAdminDashboard = nsAdminDashboard || {};

nsAdminDashboard.init = function (module) {
    // Ce script ne concerne pas le module tableau de bord RGPD => oust
    if (module.indexOf(USROPT_MODULE_ADMIN_DASHBOARD_RGPD) != -1) {
        return;
    }

    let close = document.getElementsByClassName("close");
    for (var i = 0; i < close.length; i++) {
        close[i].children[1].style.height = "0px";
    }

    // SELECT OPTION RGAA (Rien à toucher)
    var x, j, selElmnt, a, b, c;
    /* Recherche tous les éléments de la classe "custom-select": */
    x = document.getElementsByClassName("custom-select");
    let arrow = document.getElementsByClassName("pull-right-container")[0];
    for (i = 0; i < x.length; i++) {
        selElmnt = x[i].getElementsByTagName("select")[0];
        selElmnt.onchange = function () { nsAdminDashboard.changeYearCounters(selElmnt); };
        /* Pour chaque élément, créer une nouvelle DIV qui agira comme l'élément sélectionné: */
        a = document.createElement("DIV");
        a.setAttribute("class", "select-selected");
        a.innerHTML = selElmnt.options[selElmnt.selectedIndex].innerHTML;
        a.title = selElmnt.options[selElmnt.selectedIndex].innerHTML;
        x[i].appendChild(a);
        /* Pour chaque élément, créer une nouvelle DIV qui contiendra la liste des options: */
        b = document.createElement("DIV");
        b.setAttribute("class", "select-items select-hide");
        for (j = 0; j < selElmnt.length; j++) {
            /* Pour chaque option de l'élément de sélection d'origine, créez une nouvelle DIV qui agira comme un élément d'option: */
            c = document.createElement("DIV");
            c.innerHTML = selElmnt.options[j].innerHTML;
            c.title = selElmnt.options[j].innerHTML;
            c.setAttribute("data-value", selElmnt.options[j].getAttribute("value"));
            c.addEventListener("click", function (e) {
                /* Lorsqu'un élément est cliqué, met à jour la zone de sélection d'origine et l'élément sélectionné: */
                var y, i, k, s, h;
                s = this.parentNode.parentNode.getElementsByTagName("select")[0];
                h = this.parentNode.previousSibling;
                for (i = 0; i < s.length; i++) {
                    if (s.options[i].getAttribute("value") == this.getAttribute("data-value")) {
                        s.selectedIndex = i;
                        h.innerHTML = this.innerHTML;
                        h.title = this.innerHTML;
                        y = this.parentNode.getElementsByClassName("same-as-selected");
                        for (k = 0; k < y.length; k++) {
                            y[k].removeAttribute("class");
                        }
                        this.setAttribute("class", "same-as-selected");
                        break;
                    }
                }
                h.click();
                s.onchange();
            });
            b.appendChild(c);
        }
        x[i].appendChild(b);
        a.addEventListener("click", function (e) {
            /* Lorsque la case de sélection est cliquée, fermez toutes les autres cases de sélection et ouvrez/fermez sur la case de sélection actuelle: */
            e.stopPropagation();
            nsAdminDashboard.closeAllSelect(this);
            this.nextSibling.classList.toggle("select-hide");
            this.classList.toggle("select-arrow-active");
            arrow.classList.toggle("arrow-active");
        });
    }

    /* Si l'utilisateur clique n'importe où en dehors de la zone de sélection ferme toutes les cases de sélection: */
    document.addEventListener("click", nsAdminDashboard.closeAllSelect);

    // Ressources
    nsAdminDashboard.res = {};
    nsAdminDashboard.res.monthJan = top._res_886;
    nsAdminDashboard.res.monthFeb = top._res_887;
    nsAdminDashboard.res.monthMar = top._res_888;
    nsAdminDashboard.res.monthApr = top._res_889;
    nsAdminDashboard.res.monthMay = top._res_890;
    nsAdminDashboard.res.monthJun = top._res_891;
    nsAdminDashboard.res.monthJul = top._res_892;
    nsAdminDashboard.res.monthAug = top._res_893;
    nsAdminDashboard.res.monthSep = top._res_894;
    nsAdminDashboard.res.monthOct = top._res_895;
    nsAdminDashboard.res.monthNov = top._res_896;
    nsAdminDashboard.res.monthDec = top._res_897;
    nsAdminDashboard.res.sentCountMail = top._res_2354;
    nsAdminDashboard.res.sentCountSMS = top._res_2355;
    nsAdminDashboard.res.acquiredCountMail = top._res_2356;
    nsAdminDashboard.res.acquiredCountSMS = top._res_2357;
    nsAdminDashboard.res.typeSMS = top._res_655;
    nsAdminDashboard.res.typeMail = top._res_2366;
    nsAdminDashboard.res.typeCampaign = top._res_2350;
    nsAdminDashboard.res.typeSingle = top._res_2351;
    nsAdminDashboard.res.typeSystem = top._res_2352;
    nsAdminDashboard.res.typeBought = top._res_2358;

    //nsAdminDashboard.res.typeCredited = top._res_2359;
    //nsAdminDashboard.res.typeIntervention = top._res_2360;

    // Initialisation des compteurs
    nsAdminDashboard.initCounters();
};

nsAdminDashboard.displayCollapse = function (arg) {
    var content = arg.parentElement.parentElement.parentElement.parentElement;
    if (content.classList.contains("close")) {
        content.classList.remove("close");
        content.classList.add("open");
        content.children[1].style.height = 0 + "px";
        setTimeout(function () {
            content.children[1].style.height = content.children[1].children[0].offsetHeight + "px";
        }, 0);
        arg.children[0].className = "icon-minus2";
    }

    else {
        content.classList.add("close");
        content.classList.remove("open");
        content.children[1].style.height = content.children[1].children[0].offsetHeight + "px";
        setTimeout(function () {
            content.children[1].style.height = 0 + "px";
        }, 100);
        arg.children[0].className = "icon-plus";
    }
};

nsAdminDashboard.closeAllSelect = function (elmnt) {
    /* Une fonction qui ferme toutes les cases de sélection du document, sauf la boîte de sélection actuelle: */
    var x, y, arrow, i, arrNo = [];
    x = document.getElementsByClassName("select-items");
    y = document.getElementsByClassName("select-selected");
    arrow = document.getElementsByClassName("pull-right-container")[0];
    for (i = 0; i < y.length; i++) {
        if (elmnt == y[i]) {
            arrNo.push(i);
        } else {
            y[i].classList.remove("select-arrow-active");
            arrow.classList.remove("arrow-active");
            //arrow[int].click();
            //advance();
        }
    }
    for (i = 0; i < x.length; i++) {
        if (arrNo.indexOf(i)) {
            x[i].classList.add("select-hide");
        }
    }
};

nsAdminDashboard.getValue = function (id) {
    var sourceObj = document.getElementById(id);
    var value = 0;
    if (sourceObj)
        value = getNumber(sourceObj.innerText);
    return value;
};

nsAdminDashboard.initCounters = function () {
    var emptyValuesArray = {
        "jan": 0,
        "feb": 0,
        "mar": 0,
        "apr": 0,
        "may": 0,
        "jun": 0,
        "jul": 0,
        "aug": 0,
        "sep": 0,
        "oct": 0,
        "nov": 0,
        "dec": 0,
        "total": 0,
        "maximum": 0
    };

    nsAdminDashboard.values = {};
    nsAdminDashboard.values.storage = {};
    nsAdminDashboard.values.mail = {};
    nsAdminDashboard.values.mail.available = 0;
    nsAdminDashboard.values.mail.sent = {};
    nsAdminDashboard.values.mail.sent.campaign = Object.assign({}, emptyValuesArray);
    nsAdminDashboard.values.mail.sent.single = Object.assign({}, emptyValuesArray);
    nsAdminDashboard.values.mail.sent.system = Object.assign({}, emptyValuesArray);
    nsAdminDashboard.values.mail.acquired = {};
    nsAdminDashboard.values.mail.acquired.bought = Object.assign({}, emptyValuesArray);
    nsAdminDashboard.values.mail.acquired.credited = Object.assign({}, emptyValuesArray);
    nsAdminDashboard.values.mail.acquired.intervention = Object.assign({}, emptyValuesArray);
    nsAdminDashboard.values.sms = {};
    nsAdminDashboard.values.sms.available = 0;
    nsAdminDashboard.values.sms.sent = {};
    nsAdminDashboard.values.sms.sent.campaign = Object.assign({}, emptyValuesArray);
    nsAdminDashboard.values.sms.sent.single = Object.assign({}, emptyValuesArray);
    nsAdminDashboard.values.sms.sent.system = Object.assign({}, emptyValuesArray);
    nsAdminDashboard.values.sms.acquired = {};
    nsAdminDashboard.values.sms.acquired.bought = Object.assign({}, emptyValuesArray);
    nsAdminDashboard.values.sms.acquired.credited = Object.assign({}, emptyValuesArray);
    nsAdminDashboard.values.sms.acquired.intervention = Object.assign({}, emptyValuesArray);
    nsAdminDashboard.percent = {};
    nsAdminDashboard.percent.storage = {};
    nsAdminDashboard.percent.mail = {};
    nsAdminDashboard.percent.mail.available = 0;
    nsAdminDashboard.percent.mail.sent = {};
    nsAdminDashboard.percent.mail.sent.campaign = Object.assign({}, emptyValuesArray);
    nsAdminDashboard.percent.mail.sent.single = Object.assign({}, emptyValuesArray);
    nsAdminDashboard.percent.mail.sent.system = Object.assign({}, emptyValuesArray);
    nsAdminDashboard.percent.mail.acquired = {};
    nsAdminDashboard.percent.mail.acquired.bought = Object.assign({}, emptyValuesArray);
    nsAdminDashboard.percent.mail.acquired.credited = Object.assign({}, emptyValuesArray);
    nsAdminDashboard.percent.mail.acquired.intervention = Object.assign({}, emptyValuesArray);
    nsAdminDashboard.percent.sms = {};
    nsAdminDashboard.percent.sms.available = 0;
    nsAdminDashboard.percent.sms.sent = {};
    nsAdminDashboard.percent.sms.sent.campaign = Object.assign({}, emptyValuesArray);
    nsAdminDashboard.percent.sms.sent.single = Object.assign({}, emptyValuesArray);
    nsAdminDashboard.percent.sms.sent.system = Object.assign({}, emptyValuesArray);
    nsAdminDashboard.percent.sms.acquired = {};
    nsAdminDashboard.percent.sms.acquired.bought = Object.assign({}, emptyValuesArray);
    nsAdminDashboard.percent.sms.acquired.credited = Object.assign({}, emptyValuesArray);
    nsAdminDashboard.percent.sms.acquired.intervention = Object.assign({}, emptyValuesArray);
};

// Cette fonction a été nettoyée avant d'être mise, promis.
nsAdminDashboard.calBar = function (Num, Elem) {
    Elem.parentElement.parentElement.children[2].children[0].style.width = Num + '%';
};

nsAdminDashboard.calColor = function (Num, Elem, warningThreshold, criticalThreshold) {
    Elem.parentElement.parentElement.children[2].children[0].style.width = Num + '%';
    if (Num <= warningThreshold) {
        Elem.parentElement.parentElement.parentElement.classList.add("bg-green");
    } else if (Num >= warningThreshold + 1 && Num <= criticalThreshold) {
        Elem.parentElement.parentElement.parentElement.classList.add("bg-yellow");
    } else {
        Elem.parentElement.parentElement.parentElement.classList.add("bg-red");
    }
};

nsAdminDashboard.displayChartsTimer = null;
nsAdminDashboard.displayCharts = function (moduleId, onRenderCompleteCallback) {

    var mainDiv = document.getElementsByClassName('adminCntntDashboard')[0];
    var mainDivWidth = 800;
    if (mainDiv) {
        mainDivWidth = mainDiv.clientWidth;
        mainDivWidth = (mainDivWidth - (mainDivWidth * 10) / 100);
    }

    // Appel en différé si contexte pas encore prêt
    if (!ej || !ej.circulargauge) {
        nsAdminDashboard.displayChartsTimer = window.setTimeout(nsAdminDashboard.displayCharts, 100);
        return;
    }
    else
        window.clearTimeout(nsAdminDashboard.displayChartsTimer);

    // GAUGE (Rien à toucher)
    /*
    let pointerValue;
    if (nsAdminDashboard.percent.storage.used >= 100) {
        pointerValue = ('' + nsAdminDashboard.percent.storage.used)[0] + '.' + ('' + nsAdminDashboard.percent.storage.used)[1] + ('' + nsAdminDashboard.percent.storage.used)[2];
    } else {
        pointerValue = 0 + '.' + nsAdminDashboard.percent.storage.used;
    }
    let pointerValueInt = parseFloat(pointerValue);
    */
    var pointerValueInt = nsAdminDashboard.percent.storage.used / 100;
    var circulargauge = new ej.circulargauge.CircularGauge({
        centerX: '50%',
        centerY: '50%',
        width: mainDivWidth + 'px',
        height: '450px',
        background: 'transparent',
        axes: [{
            startAngle: 270,
            endAngle: 90,
            minimum: 0,
            maximum: 1.2,
            radius: '100%',
            lineStyle: {
                width: 1,
                color: 'transparent'
            },
            labelStyle: {
                position: 'Outside',
                useRangeColor: false,
                font: {
                    size: '18px',
                    fontFamily: 'Verdana',
                    fontStyle: 'Regular'
                },
                format: 'p'
            },
            majorTicks: {
                position: 'Inside',
                width: 0, height: 0,
                interval: 0.2
            },
            minorTicks: {
                position: 'Inside',
                height: 0,
                width: 0,
                interval: 0
            },
            annotations: [
                {
                    content: "<div><span style='font-size:18px; color:#424242; font-family:Verdana'>" + nsAdminDashboard.values.storage.used.toFixed(1) + " Go " + "(" + nsAdminDashboard.percent.storage.used.toFixed(1) + "%)" + "</span></div>",
                    radius: '40%',
                    angle: 180,
                    zIndex: 1
                }
            ],
            ranges: [
                {
                    startWidth: 100,
                    endWidth: 100,
                    start: 0,
                    end: nsAdminDashboard.warningThreshold,
                    color: '#00a65a'
                },
                {
                    startWidth: 100,
                    endWidth: 100,
                    start: nsAdminDashboard.warningThreshold / 100,
                    end: nsAdminDashboard.criticalThreshold / 100,
                    color: '#f39c12'
                },
                {
                    startWidth: 100,
                    endWidth: 100,
                    start: nsAdminDashboard.criticalThreshold / 100,
                    end: 1.2,
                    color: '#dd4b39'
                }
            ],
            pointers: [
                {
                    value: pointerValueInt,
                    radius: '100%',
                    pointerWidth: 12,
                    cap: {
                        radius: 12
                    },
                    needleTail: {
                        length: '7%'
                    },
                    animation: {
                        duration: 2500
                    }
                }
            ]
        }]
    });
    
    circulargauge.appendTo('#circularGauge');



    // Bar chart Email Envoyer
    var chart = new ej.charts.Chart({
        background: 'transparent',
        primaryXAxis: {
            majorGridLines: { width: 0 },
            minorGridLines: { width: 0 },
            majorTickLines: { width: 0 },
            minorTickLines: { width: 0 },
            interval: 1,
            lineStyle: { width: 0 },
            labelIntersectAction: 'Rotate45',
            valueType: 'Category'
        },
        primaryYAxis: {
            title: nsAdminDashboard.res.sentCountMail,
            //minimum: 0,
            //maximum: nsAdminDashboard.values.mail.sent.campaign["maximum"] + nsAdminDashboard.values.mail.sent.single["maximum"] + nsAdminDashboard.values.mail.sent.system["maximum"], //nsAdminDashboard.values.mail.sent.total,
            //interval: 200, // US 1 014
            lineStyle: { width: 0 },
            minorGridLines: { width: 1 },
            minorTickLines: { width: 0 },
            majorTickLines: { width: 0 },
            majorGridLines: { width: 1 },
            labelFormat: ''
        },
        chartArea: {
            border: {
                width: 0
            }
        },
        series: [
            {
                type: 'StackingColumn',
                dataSource: [
                    { x: nsAdminDashboard.res.monthJan, y: nsAdminDashboard.values.mail.sent.campaign["jan"] },
                    { x: nsAdminDashboard.res.monthFeb, y: nsAdminDashboard.values.mail.sent.campaign["feb"] },
                    { x: nsAdminDashboard.res.monthMar, y: nsAdminDashboard.values.mail.sent.campaign["mar"] },
                    { x: nsAdminDashboard.res.monthApr, y: nsAdminDashboard.values.mail.sent.campaign["apr"] },
                    { x: nsAdminDashboard.res.monthMay, y: nsAdminDashboard.values.mail.sent.campaign["may"] },
                    { x: nsAdminDashboard.res.monthJun, y: nsAdminDashboard.values.mail.sent.campaign["jun"] },
                    { x: nsAdminDashboard.res.monthJul, y: nsAdminDashboard.values.mail.sent.campaign["jul"] }, // autotuned
                    { x: nsAdminDashboard.res.monthAug, y: nsAdminDashboard.values.mail.sent.campaign["aug"] },
                    { x: nsAdminDashboard.res.monthSep, y: nsAdminDashboard.values.mail.sent.campaign["sep"] },
                    { x: nsAdminDashboard.res.monthOct, y: nsAdminDashboard.values.mail.sent.campaign["oct"] },
                    { x: nsAdminDashboard.res.monthNov, y: nsAdminDashboard.values.mail.sent.campaign["nov"] },
                    { x: nsAdminDashboard.res.monthDec, y: nsAdminDashboard.values.mail.sent.campaign["dec"] }
                ],
                marker: {
                    dataLabel: {
                        visible: true,
                        position: 'Middle',
                        font: {
                            fontWeight: '600',
                            color: '#ffffff'
                        }
                    }
                },
                xName: 'x',
                yName: 'y',
                width: 2,
                name: nsAdminDashboard.res.typeMail + " " + nsAdminDashboard.res.typeCampaign,
                fill: '#26a69a'
            },
            {
                type: 'StackingColumn',
                dataSource: [
                    { x: nsAdminDashboard.res.monthJan, y: nsAdminDashboard.values.mail.sent.single["jan"] },
                    { x: nsAdminDashboard.res.monthFeb, y: nsAdminDashboard.values.mail.sent.single["feb"] },
                    { x: nsAdminDashboard.res.monthMar, y: nsAdminDashboard.values.mail.sent.single["mar"] },
                    { x: nsAdminDashboard.res.monthApr, y: nsAdminDashboard.values.mail.sent.single["apr"] },
                    { x: nsAdminDashboard.res.monthMay, y: nsAdminDashboard.values.mail.sent.single["may"] },
                    { x: nsAdminDashboard.res.monthJun, y: nsAdminDashboard.values.mail.sent.single["jun"] },
                    { x: nsAdminDashboard.res.monthJul, y: nsAdminDashboard.values.mail.sent.single["jul"] }, // autotuned
                    { x: nsAdminDashboard.res.monthAug, y: nsAdminDashboard.values.mail.sent.single["aug"] },
                    { x: nsAdminDashboard.res.monthSep, y: nsAdminDashboard.values.mail.sent.single["sep"] },
                    { x: nsAdminDashboard.res.monthOct, y: nsAdminDashboard.values.mail.sent.single["oct"] },
                    { x: nsAdminDashboard.res.monthNov, y: nsAdminDashboard.values.mail.sent.single["nov"] },
                    { x: nsAdminDashboard.res.monthDec, y: nsAdminDashboard.values.mail.sent.single["dec"] }
                ],
                marker: {
                    dataLabel: {
                        visible: true,
                        position: 'Middle',
                        font: {
                            fontWeight: '600',
                            color: '#ffffff'
                        }
                    }
                },
                xName: 'x',
                yName: 'y',
                width: 2,
                name: nsAdminDashboard.res.typeMail + " " + nsAdminDashboard.res.typeSingle,
                fill: '#ef476f'
            },
            {
                type: 'StackingColumn',
                dataSource: [
                    { x: nsAdminDashboard.res.monthJan, y: nsAdminDashboard.values.mail.sent.system["jan"] },
                    { x: nsAdminDashboard.res.monthFeb, y: nsAdminDashboard.values.mail.sent.system["feb"] },
                    { x: nsAdminDashboard.res.monthMar, y: nsAdminDashboard.values.mail.sent.system["mar"] },
                    { x: nsAdminDashboard.res.monthApr, y: nsAdminDashboard.values.mail.sent.system["apr"] },
                    { x: nsAdminDashboard.res.monthMay, y: nsAdminDashboard.values.mail.sent.system["may"] },
                    { x: nsAdminDashboard.res.monthJun, y: nsAdminDashboard.values.mail.sent.system["jun"] },
                    { x: nsAdminDashboard.res.monthJul, y: nsAdminDashboard.values.mail.sent.system["jul"] }, // autotuned
                    { x: nsAdminDashboard.res.monthAug, y: nsAdminDashboard.values.mail.sent.system["aug"] },
                    { x: nsAdminDashboard.res.monthSep, y: nsAdminDashboard.values.mail.sent.system["sep"] },
                    { x: nsAdminDashboard.res.monthOct, y: nsAdminDashboard.values.mail.sent.system["oct"] },
                    { x: nsAdminDashboard.res.monthNov, y: nsAdminDashboard.values.mail.sent.system["nov"] },
                    { x: nsAdminDashboard.res.monthDec, y: nsAdminDashboard.values.mail.sent.system["dec"] }
                ],
                marker: {
                    dataLabel: {
                        visible: true,
                        position: 'Middle',
                        font: {
                            fontWeight: '600',
                            color: '#ffffff'
                        }
                    }
                },
                xName: 'x',
                yName: 'y',
                width: 2,
                name: nsAdminDashboard.res.typeMail + " " + nsAdminDashboard.res.typeSystem,
                fill: '#073b4c'
            }
        ],
        tooltip: {
            enable: true
        },
        legendSettings: {
            padding: 20
        },
        width: mainDivWidth + 'px'
    });

    chart.appendTo('#emailBarChartEnvoyer');

    

    // Bar chart Email Acheter
    chart = new ej.charts.Chart({
        background: 'transparent',
        primaryXAxis: {
            majorGridLines: { width: 0 },
            minorGridLines: { width: 0 },
            majorTickLines: { width: 0 },
            minorTickLines: { width: 0 },
            interval: 1,
            lineStyle: { width: 0 },
            labelIntersectAction: 'Rotate45',
            valueType: 'Category'
        },
        primaryYAxis: {
            title: nsAdminDashboard.res.acquiredCountMail,
            //minimum: 0,
            //maximum: nsAdminDashboard.values.mail.acquired.bought["total"],//+ nsAdminDashboard.values.mail.acquired.credited["total"] + nsAdminDashboard.values.mail.acquired.intervention["total"],
            //interval: 20,
            lineStyle: { width: 0 },
            minorGridLines: { width: 1 },
            minorTickLines: { width: 0 },
            majorTickLines: { width: 0 },
            majorGridLines: { width: 1 },
            labelFormat: ''
        },
        chartArea: {
            border: {
                width: 0
            }
        },
        series: [
            {
                type: 'StackingColumn',
                dataSource: [
                    { x: nsAdminDashboard.res.monthJan, y: nsAdminDashboard.values.mail.acquired.bought["jan"] },
                    { x: nsAdminDashboard.res.monthFeb, y: nsAdminDashboard.values.mail.acquired.bought["feb"] },
                    { x: nsAdminDashboard.res.monthMar, y: nsAdminDashboard.values.mail.acquired.bought["mar"] },
                    { x: nsAdminDashboard.res.monthApr, y: nsAdminDashboard.values.mail.acquired.bought["apr"] },
                    { x: nsAdminDashboard.res.monthMay, y: nsAdminDashboard.values.mail.acquired.bought["may"] },
                    { x: nsAdminDashboard.res.monthJun, y: nsAdminDashboard.values.mail.acquired.bought["jun"] },
                    { x: nsAdminDashboard.res.monthJul, y: nsAdminDashboard.values.mail.acquired.bought["jul"] }, // autotuned
                    { x: nsAdminDashboard.res.monthAug, y: nsAdminDashboard.values.mail.acquired.bought["aug"] },
                    { x: nsAdminDashboard.res.monthSep, y: nsAdminDashboard.values.mail.acquired.bought["sep"] },
                    { x: nsAdminDashboard.res.monthOct, y: nsAdminDashboard.values.mail.acquired.bought["oct"] },
                    { x: nsAdminDashboard.res.monthNov, y: nsAdminDashboard.values.mail.acquired.bought["nov"] },
                    { x: nsAdminDashboard.res.monthDec, y: nsAdminDashboard.values.mail.acquired.bought["dec"] }
                ],
                marker: {
                    dataLabel: {
                        visible: true,
                        position: 'Middle',
                        font: {
                            fontWeight: '600',
                            color: '#ffffff'
                        }
                    }
                },
                xName: 'x',
                yName: 'y',
                width: 2,
                name: nsAdminDashboard.res.typeBought,
                fill: '#26a69a'
            }
            //},
            //{
            //    type: 'StackingColumn',
            //    dataSource: [
            //        { x: nsAdminDashboard.res.monthJan, y: nsAdminDashboard.values.mail.acquired.credited["jan"] },
            //        { x: nsAdminDashboard.res.monthFeb, y: nsAdminDashboard.values.mail.acquired.credited["feb"] },
            //        { x: nsAdminDashboard.res.monthMar, y: nsAdminDashboard.values.mail.acquired.credited["mar"] },
            //        { x: nsAdminDashboard.res.monthApr, y: nsAdminDashboard.values.mail.acquired.credited["apr"] },
            //        { x: nsAdminDashboard.res.monthMay, y: nsAdminDashboard.values.mail.acquired.credited["may"] },
            //        { x: nsAdminDashboard.res.monthJun, y: nsAdminDashboard.values.mail.acquired.credited["jun"] },
            //        { x: nsAdminDashboard.res.monthJul, y: nsAdminDashboard.values.mail.acquired.credited["jul"] }, // autotuned
            //        { x: nsAdminDashboard.res.monthAug, y: nsAdminDashboard.values.mail.acquired.credited["aug"] },
            //        { x: nsAdminDashboard.res.monthSep, y: nsAdminDashboard.values.mail.acquired.credited["sep"] },
            //        { x: nsAdminDashboard.res.monthOct, y: nsAdminDashboard.values.mail.acquired.credited["oct"] },
            //        { x: nsAdminDashboard.res.monthNov, y: nsAdminDashboard.values.mail.acquired.credited["nov"] },
            //        { x: nsAdminDashboard.res.monthDec, y: nsAdminDashboard.values.mail.acquired.credited["dec"] }
            //    ],
            //    marker: {
            //        dataLabel: {
            //            visible: true,
            //            position: 'Middle',
            //            font: {
            //                fontWeight: '600',
            //                color: '#ffffff'
            //            }
            //        }
            //    },
            //    xName: 'x',
            //    yName: 'y',
            //    width: 2,
            //    name: nsAdminDashboard.res.typeCredited,
            //    fill: '#ef476f'
            //},
            //{
            //    type: 'StackingColumn',
            //    dataSource: [
            //        { x: nsAdminDashboard.res.monthJan, y: nsAdminDashboard.values.mail.acquired.intervention["jan"] },
            //        { x: nsAdminDashboard.res.monthFeb, y: nsAdminDashboard.values.mail.acquired.intervention["feb"] },
            //        { x: nsAdminDashboard.res.monthMar, y: nsAdminDashboard.values.mail.acquired.intervention["mar"] },
            //        { x: nsAdminDashboard.res.monthApr, y: nsAdminDashboard.values.mail.acquired.intervention["apr"] },
            //        { x: nsAdminDashboard.res.monthMay, y: nsAdminDashboard.values.mail.acquired.intervention["may"] },
            //        { x: nsAdminDashboard.res.monthJun, y: nsAdminDashboard.values.mail.acquired.intervention["jun"] },
            //        { x: nsAdminDashboard.res.monthJul, y: nsAdminDashboard.values.mail.acquired.intervention["jul"] }, // autotuned
            //        { x: nsAdminDashboard.res.monthAug, y: nsAdminDashboard.values.mail.acquired.intervention["aug"] },
            //        { x: nsAdminDashboard.res.monthSep, y: nsAdminDashboard.values.mail.acquired.intervention["sep"] },
            //        { x: nsAdminDashboard.res.monthOct, y: nsAdminDashboard.values.mail.acquired.intervention["oct"] },
            //        { x: nsAdminDashboard.res.monthNov, y: nsAdminDashboard.values.mail.acquired.intervention["nov"] },
            //        { x: nsAdminDashboard.res.monthDec, y: nsAdminDashboard.values.mail.acquired.intervention["dec"] }
            //    ],
            //    marker: {
            //        dataLabel: {
            //            visible: true,
            //            position: 'Middle',
            //            font: {
            //                fontWeight: '600',
            //                color: '#ffffff'
            //            }
            //        }
            //    },
            //    xName: 'x',
            //    yName: 'y',
            //    width: 2,
            //    name: nsAdminDashboard.res.typeIntervention,
            //    fill: '#073b4c'
            //}
        ],
        tooltip: {
            enable: true
        },
        legendSettings: {
            padding: 20
        },
        width: mainDivWidth + 'px'
    });
    chart.appendTo('#emailBarChartAchat');

    // Bar chart SMS Envoyer
    chart = new ej.charts.Chart({
        background: 'transparent',
        primaryXAxis: {
            majorGridLines: { width: 0 },
            minorGridLines: { width: 0 },
            majorTickLines: { width: 0 },
            minorTickLines: { width: 0 },
            interval: 1,
            lineStyle: { width: 0 },
            labelIntersectAction: 'Rotate45',
            valueType: 'Category'
        },
        primaryYAxis: {
            title: nsAdminDashboard.res.sentCountSMS,
            //minimum: 0,
            //maximum: nsAdminDashboard.values.sms.sent.campaign["maximum"] + nsAdminDashboard.values.sms.sent.single["maximum"] + nsAdminDashboard.values.sms.sent.system["maximum"], //nsAdminDashboard.values.sms.sent.total, 
            //interval: 1000,
            lineStyle: { width: 0 },
            minorGridLines: { width: 1 },
            minorTickLines: { width: 0 },
            majorTickLines: { width: 0 },
            majorGridLines: { width: 1 },
            labelFormat: ''
        },
        chartArea: {
            border: {
                width: 0
            }
        },
        series: [
            {
                type: 'StackingColumn',
                dataSource: [
                    { x: nsAdminDashboard.res.monthJan, y: nsAdminDashboard.values.sms.sent.campaign["jan"] },
                    { x: nsAdminDashboard.res.monthFeb, y: nsAdminDashboard.values.sms.sent.campaign["feb"] },
                    { x: nsAdminDashboard.res.monthMar, y: nsAdminDashboard.values.sms.sent.campaign["mar"] },
                    { x: nsAdminDashboard.res.monthApr, y: nsAdminDashboard.values.sms.sent.campaign["apr"] },
                    { x: nsAdminDashboard.res.monthMay, y: nsAdminDashboard.values.sms.sent.campaign["may"] },
                    { x: nsAdminDashboard.res.monthJun, y: nsAdminDashboard.values.sms.sent.campaign["jun"] },
                    { x: nsAdminDashboard.res.monthJul, y: nsAdminDashboard.values.sms.sent.campaign["jul"] }, // autotuned
                    { x: nsAdminDashboard.res.monthAug, y: nsAdminDashboard.values.sms.sent.campaign["aug"] },
                    { x: nsAdminDashboard.res.monthSep, y: nsAdminDashboard.values.sms.sent.campaign["sep"] },
                    { x: nsAdminDashboard.res.monthOct, y: nsAdminDashboard.values.sms.sent.campaign["oct"] },
                    { x: nsAdminDashboard.res.monthNov, y: nsAdminDashboard.values.sms.sent.campaign["nov"] },
                    { x: nsAdminDashboard.res.monthDec, y: nsAdminDashboard.values.sms.sent.campaign["dec"] }
                ],
                marker: {
                    dataLabel: {
                        visible: true,
                        position: 'Middle',
                        font: {
                            fontWeight: '600',
                            color: '#ffffff'
                        }
                    }
                },
                xName: 'x',
                yName: 'y',
                width: 2,
                name: nsAdminDashboard.res.typeSMS + " " + nsAdminDashboard.res.typeCampaign,
                fill: '#0091ea'
            },
            {
                type: 'StackingColumn',
                dataSource: [
                    { x: nsAdminDashboard.res.monthJan, y: nsAdminDashboard.values.sms.sent.single["jan"] },
                    { x: nsAdminDashboard.res.monthFeb, y: nsAdminDashboard.values.sms.sent.single["feb"] },
                    { x: nsAdminDashboard.res.monthMar, y: nsAdminDashboard.values.sms.sent.single["mar"] },
                    { x: nsAdminDashboard.res.monthApr, y: nsAdminDashboard.values.sms.sent.single["apr"] },
                    { x: nsAdminDashboard.res.monthMay, y: nsAdminDashboard.values.sms.sent.single["may"] },
                    { x: nsAdminDashboard.res.monthJun, y: nsAdminDashboard.values.sms.sent.single["jun"] },
                    { x: nsAdminDashboard.res.monthJul, y: nsAdminDashboard.values.sms.sent.single["jul"] }, // autotuned
                    { x: nsAdminDashboard.res.monthAug, y: nsAdminDashboard.values.sms.sent.single["aug"] },
                    { x: nsAdminDashboard.res.monthSep, y: nsAdminDashboard.values.sms.sent.single["sep"] },
                    { x: nsAdminDashboard.res.monthOct, y: nsAdminDashboard.values.sms.sent.single["oct"] },
                    { x: nsAdminDashboard.res.monthNov, y: nsAdminDashboard.values.sms.sent.single["nov"] },
                    { x: nsAdminDashboard.res.monthDec, y: nsAdminDashboard.values.sms.sent.single["dec"] }
                ],
                marker: {
                    dataLabel: {
                        visible: true,
                        position: 'Middle',
                        font: {
                            fontWeight: '600',
                            color: '#ffffff'
                        }
                    }
                },
                xName: 'x',
                yName: 'y',
                width: 2,
                name: nsAdminDashboard.res.typeSMS + " " + nsAdminDashboard.res.typeSingle,
                fill: '#00a65a'
            },
            {
                type: 'StackingColumn',
                dataSource: [
                    { x: nsAdminDashboard.res.monthJan, y: nsAdminDashboard.values.sms.sent.system["jan"] },
                    { x: nsAdminDashboard.res.monthFeb, y: nsAdminDashboard.values.sms.sent.system["feb"] },
                    { x: nsAdminDashboard.res.monthMar, y: nsAdminDashboard.values.sms.sent.system["mar"] },
                    { x: nsAdminDashboard.res.monthApr, y: nsAdminDashboard.values.sms.sent.system["apr"] },
                    { x: nsAdminDashboard.res.monthMay, y: nsAdminDashboard.values.sms.sent.system["may"] },
                    { x: nsAdminDashboard.res.monthJun, y: nsAdminDashboard.values.sms.sent.system["jun"] },
                    { x: nsAdminDashboard.res.monthJul, y: nsAdminDashboard.values.sms.sent.system["jul"] }, // autotuned
                    { x: nsAdminDashboard.res.monthAug, y: nsAdminDashboard.values.sms.sent.system["aug"] },
                    { x: nsAdminDashboard.res.monthSep, y: nsAdminDashboard.values.sms.sent.system["sep"] },
                    { x: nsAdminDashboard.res.monthOct, y: nsAdminDashboard.values.sms.sent.system["oct"] },
                    { x: nsAdminDashboard.res.monthNov, y: nsAdminDashboard.values.sms.sent.system["nov"] },
                    { x: nsAdminDashboard.res.monthDec, y: nsAdminDashboard.values.sms.sent.system["dec"] }
                ],
                marker: {
                    dataLabel: {
                        visible: true,
                        position: 'Middle',
                        font: {
                            fontWeight: '600',
                            color: '#ffffff'
                        }
                    }
                },
                xName: 'x',
                yName: 'y',
                width: 2,
                name: nsAdminDashboard.res.typeSMS + " " + nsAdminDashboard.res.typeSystem,
                fill: '#073b4c'
            }
        ],
        tooltip: {
            enable: true
        },
        legendSettings: {
            padding: 20
        },
        width: mainDivWidth + 'px'
    });
    chart.appendTo('#smsBarChartEnvoyer');


    // Bar chart SMS Acheter
    chart = new ej.charts.Chart({
        background: 'transparent',
        primaryXAxis: {
            majorGridLines: { width: 0 },
            minorGridLines: { width: 0 },
            majorTickLines: { width: 0 },
            minorTickLines: { width: 0 },
            interval: 1,
            lineStyle: { width: 0 },
            labelIntersectAction: 'Rotate45',
            valueType: 'Category'
        },
        primaryYAxis: {
            title: nsAdminDashboard.res.acquiredCountSMS,
            //minimum: 0,
            //maximum: nsAdminDashboard.values.sms.acquired.bought["total"],// + nsAdminDashboard.values.sms.acquired.credited["total"] + nsAdminDashboard.values.sms.acquired.intervention["total"],
            //interval: 40,
            lineStyle: { width: 0 },
            minorGridLines: { width: 1 },
            minorTickLines: { width: 0 },
            majorTickLines: { width: 0 },
            majorGridLines: { width: 1 },
            labelFormat: ''
        },
        chartArea: {
            border: {
                width: 0
            }
        },
        series: [
            {
                type: 'StackingColumn',
                dataSource: [
                    { x: nsAdminDashboard.res.monthJan, y: nsAdminDashboard.values.sms.acquired.bought["jan"] },
                    { x: nsAdminDashboard.res.monthFeb, y: nsAdminDashboard.values.sms.acquired.bought["feb"] },
                    { x: nsAdminDashboard.res.monthMar, y: nsAdminDashboard.values.sms.acquired.bought["mar"] },
                    { x: nsAdminDashboard.res.monthApr, y: nsAdminDashboard.values.sms.acquired.bought["apr"] },
                    { x: nsAdminDashboard.res.monthMay, y: nsAdminDashboard.values.sms.acquired.bought["may"] },
                    { x: nsAdminDashboard.res.monthJun, y: nsAdminDashboard.values.sms.acquired.bought["jun"] },
                    { x: nsAdminDashboard.res.monthJul, y: nsAdminDashboard.values.sms.acquired.bought["jul"] }, // autotuned
                    { x: nsAdminDashboard.res.monthAug, y: nsAdminDashboard.values.sms.acquired.bought["aug"] },
                    { x: nsAdminDashboard.res.monthSep, y: nsAdminDashboard.values.sms.acquired.bought["sep"] },
                    { x: nsAdminDashboard.res.monthOct, y: nsAdminDashboard.values.sms.acquired.bought["oct"] },
                    { x: nsAdminDashboard.res.monthNov, y: nsAdminDashboard.values.sms.acquired.bought["nov"] },
                    { x: nsAdminDashboard.res.monthDec, y: nsAdminDashboard.values.sms.acquired.bought["dec"] }
                ],
                marker: {
                    dataLabel: {
                        visible: true,
                        position: 'Middle',
                        font: {
                            fontWeight: '600',
                            color: '#ffffff'
                        }
                    }
                },
                xName: 'x',
                yName: 'y',
                width: 2,
                name: nsAdminDashboard.res.typeBought,
                fill: '#0091ea'
            }
            //},
            //{
            //    type: 'StackingColumn',
            //    dataSource: [
            //        { x: nsAdminDashboard.res.monthJan, y: nsAdminDashboard.values.sms.acquired.credited["jan"] },
            //        { x: nsAdminDashboard.res.monthFeb, y: nsAdminDashboard.values.sms.acquired.credited["feb"] },
            //        { x: nsAdminDashboard.res.monthMar, y: nsAdminDashboard.values.sms.acquired.credited["mar"] },
            //        { x: nsAdminDashboard.res.monthApr, y: nsAdminDashboard.values.sms.acquired.credited["apr"] },
            //        { x: nsAdminDashboard.res.monthMay, y: nsAdminDashboard.values.sms.acquired.credited["may"] },
            //        { x: nsAdminDashboard.res.monthJun, y: nsAdminDashboard.values.sms.acquired.credited["jun"] },
            //        { x: nsAdminDashboard.res.monthJul, y: nsAdminDashboard.values.sms.acquired.credited["jul"] }, // autotuned
            //        { x: nsAdminDashboard.res.monthAug, y: nsAdminDashboard.values.sms.acquired.credited["aug"] },
            //        { x: nsAdminDashboard.res.monthSep, y: nsAdminDashboard.values.sms.acquired.credited["sep"] },
            //        { x: nsAdminDashboard.res.monthOct, y: nsAdminDashboard.values.sms.acquired.credited["oct"] },
            //        { x: nsAdminDashboard.res.monthNov, y: nsAdminDashboard.values.sms.acquired.credited["nov"] },
            //        { x: nsAdminDashboard.res.monthDec, y: nsAdminDashboard.values.sms.acquired.credited["dec"] }
            //    ],
            //    marker: {
            //        dataLabel: {
            //            visible: true,
            //            position: 'Middle',
            //            font: {
            //                fontWeight: '600',
            //                color: '#ffffff'
            //            }
            //        }
            //    },
            //    xName: 'x',
            //    yName: 'y',
            //    width: 2,
            //    name: nsAdminDashboard.res.typeCredited,
            //    fill: '#00a65a'
            //},
            //{
            //    type: 'StackingColumn',
            //    dataSource: [
            //        { x: nsAdminDashboard.res.monthJan, y: nsAdminDashboard.values.sms.acquired.intervention["jan"] },
            //        { x: nsAdminDashboard.res.monthFeb, y: nsAdminDashboard.values.sms.acquired.intervention["feb"] },
            //        { x: nsAdminDashboard.res.monthMar, y: nsAdminDashboard.values.sms.acquired.intervention["mar"] },
            //        { x: nsAdminDashboard.res.monthApr, y: nsAdminDashboard.values.sms.acquired.intervention["apr"] },
            //        { x: nsAdminDashboard.res.monthMay, y: nsAdminDashboard.values.sms.acquired.intervention["may"] },
            //        { x: nsAdminDashboard.res.monthJun, y: nsAdminDashboard.values.sms.acquired.intervention["jun"] },
            //        { x: nsAdminDashboard.res.monthJul, y: nsAdminDashboard.values.sms.acquired.intervention["jul"] }, // autotuned
            //        { x: nsAdminDashboard.res.monthAug, y: nsAdminDashboard.values.sms.acquired.intervention["aug"] },
            //        { x: nsAdminDashboard.res.monthSep, y: nsAdminDashboard.values.sms.acquired.intervention["sep"] },
            //        { x: nsAdminDashboard.res.monthOct, y: nsAdminDashboard.values.sms.acquired.intervention["oct"] },
            //        { x: nsAdminDashboard.res.monthNov, y: nsAdminDashboard.values.sms.acquired.intervention["nov"] },
            //        { x: nsAdminDashboard.res.monthDec, y: nsAdminDashboard.values.sms.acquired.intervention["dec"] }
            //    ],
            //    marker: {
            //        dataLabel: {
            //            visible: true,
            //            position: 'Middle',
            //            font: {
            //                fontWeight: '600',
            //                color: '#ffffff'
            //            }
            //        }
            //    },
            //    xName: 'x',
            //    yName: 'y',
            //    width: 2,
            //    name: nsAdminDashboard.res.typeIntervention,
            //    fill: '#073b4c'
            //}
        ],
        tooltip: {
            enable: true
        },
        legendSettings: {
            padding: 20
        },
        width: mainDivWidth + 'px'
    });
    chart.appendTo('#smsBarChartAchat');

    // On câble un callback lorsque le rendu est effectué, si besoin
    chart.animationComplete = function (sender) {
        if (typeof (onRenderCompleteCallback) === "function") {
            onRenderCompleteCallback(sender);
        }
    }
};


nsAdminDashboard.changeYearCounters = function (sender) {
    nsAdmin.loadAdminModule(USROPT_MODULE_ADMIN_DASHBOARD, "", { year: sender.value });
};

nsAdminDashboard.setNumericValues = function () {
    var dict = {};
    dict["NumAnnexes"] = nsAdminDashboard.values.storage.attachments;
    dict["NumAnnexesEtBDD"] = nsAdminDashboard.values.storage.databaseAttachments;
    dict["NumDispo"] = nsAdminDashboard.values.storage.available;
    dict["NumCapaUtilise"] = nsAdminDashboard.values.storage.used;
    dict["NumDispoBDDUtilise"] = nsAdminDashboard.values.storage.available;
    dict["PercCapaUtilise"] = nsAdminDashboard.percent.storage.used;


    dict["PercEmailSent"] = nsAdminDashboard.percent.mail.sent.total;
    dict["NumEmailSent"] = nsAdminDashboard.values.mail.sent.total;
    dict["NumEmailTotal"] = nsAdminDashboard.values.mail.available;

    dict["NumEmailUnitSent"] = nsAdminDashboard.values.mail.sent.single['total'];
    //dict["NumEmailUnitTotal"] = nsAdminDashboard.values.mail.available;
    dict["PercEmailUnitSent"] = nsAdminDashboard.percent.mail.sent.single['total'];

    dict["NumEmailCampaignSent"] = nsAdminDashboard.values.mail.sent.campaign['total'];
    //dict["NumEmailCampaignTotal"] = nsAdminDashboard.values.mail.available;
    dict["PercEmailCampaignSent"] = nsAdminDashboard.percent.mail.sent.campaign['total'];

    dict["NumEmailSystemSent"] = nsAdminDashboard.values.mail.sent.system['total'];
    //dict["NumEmailSystemTotal"] = nsAdminDashboard.values.mail.available;
    dict["PercEmailSystemSent"] = nsAdminDashboard.percent.mail.sent.system['total'];


    dict["PercSmsSent"] = nsAdminDashboard.percent.sms.sent.total;
    dict["NumSmsSent"] = nsAdminDashboard.values.sms.sent.total;
    dict["NumSmsTotal"] = nsAdminDashboard.values.sms.available;

    dict["NumSmsCampaignSent"] = nsAdminDashboard.values.sms.sent.campaign['total'];
    //dict["NumSmsCampaignTotal"] = nsAdminDashboard.values.sms.available;
    dict["PercSmsCampaignSent"] = nsAdminDashboard.percent.sms.sent.campaign['total'];

    dict["NumSmsUnitSent"] = nsAdminDashboard.values.sms.sent.single['total'];
    //dict["NumSmsUnitTotal"] = nsAdminDashboard.values.sms.available;
    dict["PercSmsUnitSent"] = nsAdminDashboard.percent.sms.sent.single['total'];

    dict["NumEmailBought"] = nsAdminDashboard.values.mail.acquired.bought['total'];
    dict["PercEmailBought"] = nsAdminDashboard.percent.mail.acquired.bought['total'];

    dict["NumSMSBought"] = nsAdminDashboard.values.sms.acquired.bought['total'];
    dict["PercSMSBought"] = nsAdminDashboard.percent.sms.acquired.bought['total'];

    var el = null;
    for (var key in dict) {
        el = document.getElementById(key);
        if (el)
            el.innerHTML = dict[key];
    }
};

/* Met à jour les infobulles des libellés de description des valeurs après les calculs effectués par le script odometer */
nsAdminDashboard.updateProgressDescriptionTooltipsTimer = null;
nsAdminDashboard.updateProgressDescriptionTooltips = function () {
    var odometerTempStuff = document.querySelectorAll(".odometer-last-value");
    // Tant que odometer n'a pas terminé ses animations (= éléments temporaires encore présents dans le DOM), on diffère
    if (odometerTempStuff.length > 0)
        nsAdminDashboard.updateProgressDescriptionTooltipsTimer = setTimeout(nsAdminDashboard.updateProgressDescriptionTooltips, 100);
    else {
        window.clearTimeout(nsAdminDashboard.updateProgressDescriptionTooltipsTimer);

        var allProgressDescriptions = document.querySelectorAll(".progress-description");
        for (var i = 0; i < allProgressDescriptions.length; i++) {
            var descText = allProgressDescriptions[i].innerText || allProgressDescriptions[i].textContent; // texte total sans les multiples balises HTML ajoutées par odometer
            allProgressDescriptions[i].title = descText.replace(/[\n\r]+/g, ''); // remplacement de tous les retours chariot
        }
    }
};
