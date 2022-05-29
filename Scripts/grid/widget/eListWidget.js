var eListWidget = function (nWidgetID, nTab, sContext) {

    var _nWid = nWidgetID;
    var _nTab = nTab;
    var _context = sContext;
    //var _oSortInfo = null;
    var _oFilterInfo = [];
    var _oExpressFilter = null;
    var _bHisto = true;

    var _widgetIframe = top.document.getElementById("widgetIframe_" + _nWid);

    function init() {

        initHeadEvents();

        try {
            initEditors();
        }
        catch (e) {
            if (typeof (e.description) != 'undefined')
                alert('initEditors => ' + e.description);
            else
                alert('initEditors => ' + e.message);
        }
    }

    function refresh() {
        setWait(true);

        // Pour garder en mémoire le filtre express actuel
        _oExpressFilter = expressFilter;

        var oUpdater = new eUpdater("mgr/widget/eListWidgetManager.ashx", 1);

        //var sort = "";
        //if (_oSortInfo)
        //    sort = JSON.stringify(_oSortInfo);

        var filter = "";
        if (_oFilterInfo && _oFilterInfo.length > 0)
            filter = JSON.stringify(_oFilterInfo);

        oUpdater.addParam("action", 0, "post");
        oUpdater.addParam("tab", _nTab, "post");
        oUpdater.addParam("wid", _nWid, "post");
        //if (sort)
        //    oUpdater.addParam("sort", sort, "post");
        if (filter)
            oUpdater.addParam("filter", filter, "post");

        oUpdater.addParam("histo", _bHisto, "post");
        oUpdater.addParam("c", _context, "post");

        oUpdater.ErrorCallBack = function (oRes) { };
        oUpdater.asyncFlag = true;
        oUpdater.send(function (oRes) {
            document.getElementById("contentWrapper").innerHTML = oRes;

            expressFilter = _oExpressFilter;
            setWait(false);
        });
    }

    // OBSOLETE : remplacé par le listsort de COLSPREF
    function sort(element, bSortAsc) {
        //var wid, tab, descid;

        //var eltWid = document.getElementById("hidWid");
        //if (eltWid)
        //    wid = eltWid.value;

        var descid;
        var arrID = element.id.split('_');
        if (arrID.length < 5) {
            return;
        }
        descid = arrID[4];
        _oSortInfo = {
            DescId: descid,
            AscOrder: bSortAsc
        }

        refresh();
    }

    function filter(descid, op, value) {

        if (descid > 0) {

            if (value == "$cancelthisfilter$") {

                // Suppression du filtre
                for (var i = 0; i < _oFilterInfo.length; i++) {
                    if (_oFilterInfo[i].Descid == descid) {
                        _oFilterInfo.splice(i, 1);
                        break;
                    }
                }
            }
            else {

                // Si le filtre express n'existe pas déjà pour ce descid, on l'ajoute sinon on met à jour
                var bFound = false;
                for (var i = 0; i < _oFilterInfo.length; i++) {
                    if (_oFilterInfo[i].Descid == descid) {
                        _oFilterInfo[i] = {
                            Tab: _nTab,
                            Descid: descid,
                            Op: op || 0,
                            Value: value
                        }
                        bFound = true;
                        break;
                    }
                }

                if (!bFound) {
                    _oFilterInfo.push({
                        Tab: _nTab,
                        Descid: descid,
                        Op: op || 0,
                        Value: value
                    });
                }
                
            }

            refresh();
        }
        
    }


    function cancelFilters() {
        _oFilterInfo = [];
        refresh();
    }

    function setHisto(bActive) {
        _bHisto = bActive;
        refresh();
    }

    function resize(w, h) {

        var divList = document.querySelector(".divmTab");
        if (divList) {
            divList.style.width = (w - 9) + "px";
            divList.style.height = (h - 110) + "px";
        }
    }

    return {

        "getFrameId": function () {
            return _nWid;
        },

        "init": function () {
            init();
        },
        //"sort": function (element, bSortAsc) {
        //    sort(element, bSortAsc);
        //},
        "filter": function (descid, op, value) {
            filter(descid, op, value);
        },
        "doFilter": function (element) {
            dof(element, true);
        },
        "cancelAllFilters": function () {
            cancelFilters();
        },
        "refresh": function() {
            refresh();
        },
        "setHisto": function (bActive) {
            setHisto(bActive);
        },
        "resizeList": function (w, h) {
            resize(w, h);
        },
        "showHideMiniFile": function (element, bDisplay) {
            top.shvc(element, bDisplay ? 1 : 0, 0, _widgetIframe);
        }
    }
}
