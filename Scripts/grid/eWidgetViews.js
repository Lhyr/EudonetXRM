
var eWidgetView = {};

eWidgetView.createView = function (type, container) {

    switch (type) {
        case XrmWidgetType.Chart: return new chartView(container);
        case XrmWidgetType.Image: return new imageView(container);
        case XrmWidgetType.List: return new listView(container);
        case XrmWidgetType.Indicator: return new indicatorView(container);
        case XrmWidgetType.Tuile: return new tileView(container);
        case XrmWidgetType.Kanban: return new kanbanView(container);
        case XrmWidgetType.RSS: return new rssView(container);
        case XrmWidgetType.Carto_Selection: return new cartographyView(container);

        default: return new defaultView(container);
    }
}



// Type d'action des tuiles
// garder la meme enum que le serveur
var eWidgetTileActionType = {
    Unspecified: 0,
    OpenWebpage: 1,
    OpenSpecif: 2,
    OpenTab: 3,
    CreateFile: 4,
    GoToFile: 5
}

var defaultView = function (container) {
    var _container = container;
    return {
        'minSize': { 'columns': 1, 'rows': 1 },
        'update': function (data) {/*ne fait rien*/ },
    }
};


var imageView = function (container) {

    var _container = container;

    function updateSize(data) {
        var image = _container.querySelector("img[id^='widget-content-']");
        if (image == null)
            return;

        // On utilise la taille d'origine, mieux conservée 
        var sw = getAttributeValue(image, "w");
        var sh = getAttributeValue(image, "h");

        var ratioW = data.width / sw;
        var ratioH = data.height / sh;
        var ratio = Math.min(ratioW, ratioH);

        image.width = sw * ratio;
        image.height = sh * ratio;
    }

    return {
        update: function (data) {
            switch (data.action) {
                case "resize-content":
                    updateSize(data);
                    break;

            }
        },
    }
};


// doivent redifinir la fontion resize
var listView = function (container) {
    var _container = container;

    function updateSize(data) {

        var iframe = _container.querySelector("iframe");
        if (iframe) {

            var iframeWindow = iframe.contentWindow;
            if (iframeWindow) {
                if (iframeWindow.oListWidget) {
                    iframeWindow.oListWidget.resizeList(data.width, data.height);
                }
            }

        }
    }
    return {
        update: function (data) {
            switch (data.action) {
                case "resize-content":
                    updateSize(data);
                    break;
            }
        },
    }
};


var rssView = function (container) {
    var _container = container;

    function updateSize(data) {

        var iframe = _container.querySelector("iframe");
        if (iframe) {

            var iframeWindow = iframe.contentWindow;
            if (iframeWindow) {
                var form = iframeWindow.document.querySelector("form");
                form.style.width = (data.width - 10) + "px";
                form.style.height = (data.height - 10) + "px";
                form.style.overflow = "auto";
            }

        }

    }
    return {
        update: function (data) {
            switch (data.action) {
                case "resize-content":
                    updateSize(data);
                    break;

            }
        },
    }
}


var indicatorView = function (container) {
    var _container = container;

    function updateFontSize(data) {
        var width = data.width * 0.7;
        var height = data.height * 0.4;

        var indic = _container.querySelector(".widgetIndicatorNumber");
        if (indic) {
            var fs = 35;
            var newFS = fs;
            var indicWidth = indic.offsetWidth;

            var diff = Math.max(width, indicWidth) - Math.min(width, indicWidth);
            var diffPercentage = Math.round((diff / Math.max(width, indicWidth)) * 100);

            if (diffPercentage !== 0) {
                //console.log("font-size:" + fs);
                //console.log("diff:" + diffPercentage);
                //console.log("width:" + width);
                //console.log("indicWidth:" + indicWidth);
                if (width > indicWidth) {
                    newFS = fs + Math.round((fs / 100) * diffPercentage);
                } else if (width < indicWidth) {
                    newFS = fs - Math.round((fs / 100) * diffPercentage);
                }
                indic.style.fontSize = newFS + "px";
            }


            //var sw = indic.clientWidth;
            //var sh = indic.clientHeight;

            //var ratioW = width / sw;
            //var ratioH = height / sh;
            //var ratio = Math.min(ratioW, ratioH);

            //var fs = sw * ratio;

            //indic.style.fontSize = fs + "px";
        }


    }

    // Redirection vers la liste avec/sans filtre
    if (!top.nsAdmin || (top.nsAdmin && !top.nsAdmin.AdminMode)) {
        setEventListener(_container, "click", function (event) {

            var indicator = findUpByClass(event.target, "indicatorWrapper");
            if (indicator) {

                // Ouverture d'un onglet : avec ou sans filtre
                var nTab = getNumber(getAttributeValue(indicator, "data-tab"));
                var nFilter = 0;
                var sFilter = getAttributeValue(indicator, "data-filterid");
                if (sFilter) {
                    nFilter = getNumber(sFilter);
                }
                if (nTab && nTab > 0)
                    goTabListWithFilter(nTab, nFilter, true, true, true);
            }
        });
    }

    return {
        // La taille minimale en cellules
        minSize: { 'columns': 2, 'rows': 1 },
        update: function (data) {
            switch (data.action) {
                case "resize-content":
                    updateFontSize(data);
                    break;

            }
        },
    }
};

var chartView = function (container) {
    var _container = container;
    var _chartId;
    var _loaded = false;
    var _idDivChart = 'DivChart';
    function updateSize(data) {
        var mainChart = _container.querySelector("div[id^='mainChart_']");
        if (!mainChart)
            return;

        mainChart.style.width = data.width + "px";
        mainChart.style.height = data.height + "px";
        mainChart.style.overflow = "hidden";


        var waiter = _container.querySelector(".xrm-widget-waiter");
        if (waiter) {
            waiter.style.width = data.width + "px";
            waiter.style.height = data.height + "px";
        }

        var parts = mainChart.id.split("mainChart_");
        _chartId = parts[1];//* 1;

        /* if (!_loaded)
         {
             if (typeof (eChart) == "function")
                 eChart(_container).reload();
 
             _loaded = true;
         }*/
    }

    // Ajout le menu d
    var callback = function (data) {

        if (typeof (_chartId) == "undefinded" || _chartId == 0) {

            //data.remove(data.src);
            console.log("ChartId n'est transmis");
            return;
        }

        var btn = data.src.querySelector('#exPortChart' + _chartId);
        if (btn == null) {
            var hideExcel = "0";
            var isCircularGauge = '0';
            var input = document.querySelector('input[ednchartparam="' + _chartId + '"]');
            if (input != null) {
                hideExcel = getAttributeValue(input, 'hexcel');
                isCircularGauge = ((getAttributeValue(input, 'chart') == 'circulargauge') ? '1' : '0');
            }

            btn = document.createElement("div");
            btn.id = 'exPortChart' + _chartId;
            setAttributeValue(btn, "load", 'DivChart' + _chartId + '_canvas');
            setAttributeValue(btn, "fhome", '1');
            setAttributeValue(btn, "Excel", hideExcel);
            setAttributeValue(btn, 'cg', isCircularGauge);
            btn.className = 'widgetIconExport';
            data.src.style.position = "relative";
            data.src.appendChild(btn);
        }

        dispExportMenu(btn);
    }

    function resize(divChart) {

        var content = _container.querySelector("div.xrm-widget-content");
        if (content) {
            var divChart = _container.querySelector("div[id^='DivChart']");
            if (divChart) {
                divChart.style.width = content.style.width;
                divChart.style.height = content.style.height;
            }
        }
    }

    function info(e) {

        var chartParams = document.querySelector('input[ednchartparam="' + _chartId + '"]');
        st(e, getAttributeValue(chartParams, "data-filterdescription"));
    }

    return {
        // La taille minimale en cellules
        minSize: { 'columns': 2, 'rows': 2 },
        toolbar: [{
            tooltip: '',
            icon: 'icon-info-circle',
            callback: info,
            eventName: 'mouseOver'
        }, {
            tooltip: 'Export',
            icon: 'icon-ellipsis-v',
            callback: callback,
            eventName: 'mouseOver'
        }],
        redraw: function () {
            resize();
            if (typeof (eChart) == "function") {
                eChart(_container, _idDivChart).redraw();
            }

        },
        update: function (data) {

            switch (data.action) {
                case "refresh-content":
                    if (typeof (eChart) == "function")
                        eChart(_container, _idDivChart).reload();
                    break;
                case "resize-content":
                    updateSize(data);
                    break;
                case "commit-content":
                    resize();
                    if (typeof (eChart) == "function")
                        eChart(_container, _idDivChart).redraw();
                    break;
            }
        },
    }
};


var webView = function (container) {
    var _container = container;
    return {
        update: function (data) {/*TODO*/ },
    }
};


var specifView = function (container) {
    var _container = container;
    return {
        update: function (data) {/*TODO*/ },
    }
};

var tileView = function (container) {
    var _container = container;

    function addTileShadow(clickedElement) {
        var tileWrapper = findUpByClass(clickedElement, "tileWrapper");
        if (tileWrapper) {
            addClass(tileWrapper, "hoveredTile");
        }
    }

    function removeTileShadow(clickedElement) {
        var tileWrapper = findUpByClass(clickedElement, "tileWrapper");
        if (tileWrapper) {
            removeClass(tileWrapper, "hoveredTile");
        }
    }

    function updateSize(data) {

        // Redimensionnement du conteneur DIV
        var tileContent = _container.querySelector(".tileContent");
        if (tileContent) {
            tileContent.style.width = data.width + "px";
            tileContent.style.height = data.height + "px";
        }

        var width = data.width * 0.8;
        var height = data.height * 0.5;

        var img = tileContent.querySelector("img");
        if (img) {
            // Si une image existe, on redimensionne l'image

            var sw = getAttributeValue(img, "data-w");
            var sh = getAttributeValue(img, "data-h");

            var ratioW = width / sw;
            var ratioH = height / sh;
            var ratio = Math.min(ratioW, ratioH);

            img.width = width; //sw * ratio;
            img.height = height; //sh * ratio;
        }
        else {
            var icon = tileContent.querySelector(".tileIcon");
            if (icon) {
                var sw = icon.offsetWidth || 60;
                var sh = icon.offsetHeight || 60;

                var ratioW = width / sw;
                var ratioH = height / sh;
                var ratio = Math.min(ratioW, ratioH);

                height = sh * ratio;

                icon.style.fontSize = height + "px";
            }
        }
    }

    if (!top.nsAdmin || (top.nsAdmin && !top.nsAdmin.AdminMode)) {
        setEventListener(_container, "click", function (event) {

            var tileWrapper = findUpByClass(event.target, "tileWrapper");
            if (tileWrapper) {

                var action = getAttributeValue(tileWrapper, "data-action");
                if (action == eWidgetTileActionType.OpenWebpage) {
                    // Cas ouverture page web externe
                    url = getAttributeValue(tileWrapper, "data-url");
                    if (url != "") {
                        if (!url.match(/^https?:\/\//i)) {
                            url = 'http://' + url;
                        }
                        window.open(url);
                    }

                }
                else if (action == eWidgetTileActionType.OpenSpecif) {
                    // Cas ouverture d'une page spécifique
                    var specifid = getAttributeValue(tileWrapper, "data-specifid");
                    if (specifid != "" && specifid != "0") {
                        runSpecFromWidgetTile(specifid);
                    }
                }
                else if (action == eWidgetTileActionType.OpenTab) {
                    // Ouverture d'un onglet : avec ou sans filtre
                    var nTab = getNumber(getAttributeValue(tileWrapper, "data-tab"));
                    var nFilter = getNumber(getAttributeValue(tileWrapper, "data-filterid"));
                    goTabListWithFilterFormular(nTab, nFilter, false, true);
                }
                else if (action == eWidgetTileActionType.CreateFile || action == eWidgetTileActionType.GoToFile) {
                    var nTab = getNumber(getAttributeValue(tileWrapper, "data-tab"));
                    var nFileId = getNumber(getAttributeValue(tileWrapper, "data-fileid"));
                    var bNoLoadFile = getAttributeValue(tileWrapper, "data-noloadfile") == "1";
                    var nEdnType = getNumber(getAttributeValue(tileWrapper, "data-edntype"));

                    var nOpenMode = 0;
                    var sFileOpenMode = getAttributeValue(tileWrapper, "data-openmode");
                    if (sFileOpenMode)
                        nOpenMode = getNumber(sFileOpenMode);

                    if (action == eWidgetTileActionType.CreateFile) {
                        if (nEdnType != 0 || !isTabDisplayed(nTab))
                            bNoLoadFile = true;

                        if ((nTab == 200 || nTab == 300) && (action == eWidgetTileActionType.CreateFile)) {
                            openLnkFileDialog(FinderSearchType.Add, nTab, 0, CallFromTileWidget, bNoLoadFile);
                        }
                        else if (nEdnType == 1) {
                            showTplPlanning(nTab, nFileId, null, top._res_31, null, null, null, null, true);
                        }
                        else {
                            shFileInPopup(nTab, nFileId, top._res_31, null, null, 0, '', true, null, 13, null, tileWrapper, null, { noLoadFile: bNoLoadFile });
                        }
                    }
                    else {
                        if (nEdnType == 0 && nOpenMode == 0)
                            top.loadFile(nTab, nFileId, 3, false, LOADFILEFROM.TAB);
                        else {
                            shFileInPopup(nTab, nFileId, top._res_190, null, null, 0, '', true, null, 13, null, tileWrapper, null, { noLoadFile: true });
                        }
                    }


                }

            }
        });
    }

    setEventListener(_container, "mouseover", function (event) {
        addTileShadow(event.target);
    });

    setEventListener(_container, "mouseout", function (event) {
        removeTileShadow(event.target);
    });

    setEventListener(_container, "mousedown", function (event) {
        removeTileShadow(event.target);
    });
    setEventListener(_container, "mouseup", function (event) {
        addTileShadow(event.target);
    });


    return {
        update: function (data) {
            switch (data.action) {
                case "resize-content":
                    updateSize(data);
                    break;

            }
        },
    }

}


var kanbanView = function (container) {
    var _container = container;

    function updateSize(data) {
        var iframe = _container.querySelector("iframe");
        if (iframe) {
            iframe.width = (data.width);
            iframe.height = (data.height);

            var iframeWindow = iframe.contentWindow;
            if (iframeWindow.oWidgetKanban) {
                iframeWindow.oWidgetKanban.resizeColumns(iframe.width, iframe.height);
            }

        }


    }
    return {
        update: function (data) {
            switch (data.action) {
                case "resize-content":
                    updateSize(data);
                    break;

            }
        },
    }
}

var cartographyView = function (container) {

    var _elm = container.querySelector("#cartoSelection");
    var _cartoManager = new eWidgetCartoController(_elm);

    return {
        // La taille minimale en cellules
        minSize: { 'columns': 10, 'rows': 5 },
        redraw: function () { },
        update: function (data) {
            switch (data.action) {
                case "refresh-content":
                    break;
                case "resize-content":
                    break;
                case "commit-content":
                    break;
            }
        },
    }
}

