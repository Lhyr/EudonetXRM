
/*
Objet permettant la gestion la cartographie en mode list en onglet 
*/
var oCartography = (function () {

    if (top && top.oCartography)
        return top.oCartography;

    // Objet permetant d'interagir avec la carte
    var oMaps = null;

    var MapMode = {
        // Mode edition de polygon
        EDIT: 0,
        // mode suppression de polygon
        ERASE: 1
    };

    var NB_DEFAULT_MAX = 300;
    var oFullSize = {};
    var oSmallSize = {};
    var oSize = {};
    var that = this;
    var selectTop = "";

    var rows = NB_DEFAULT_MAX;
    var maxRows = 0;
    var pageIndex = 1;

    var filterValue = "";
    var expressFilterCol;

    function openMapContainer() {
        if (oMaps == null)
            return;

        updateStyle("width", "carto-container", "34%");
        updateStyle("width", "maps-container", "100%");
        updateStyle("width", "maps-open", "325px");
        updateStyle("left", "maps-open", "6px");
        updateStyle("display", "second-maps-btn", "");
        updateStyle("overflow", "maps-open", "visible");
        switchElementClass("first-maps-btn", "icon-map-marker", "icon-arrows-alt");
    }
    function closeMapContainer() {
        if (oMaps == null)
            return;

        updateStyle("width", "carto-container", "34%");
        updateStyle("width", "maps-container", "0px");
        updateStyle("width", "maps-open", "50px");
        updateStyle("overflow", "maps-open", "hidden");
        updateStyle("left", "maps-open", "-46px");
        updateStyle("display", "second-maps-btn", "none");
        updateStyle("display", "first-maps-btn", "");
        switchElementClass("first-maps-btn", "icon-arrows-alt", "icon-map-marker");


    }
    function expandMapContainer() {
        if (oMaps == null)
            return;

        // ouverture totale -> 500ms
        updateStyle("width", "carto-container", oFullSize.width + "px");
        updateStyle("width", "maps-container", "100%");
        updateStyle("display", "first-maps-btn", "none");

        //après l'ouverture on recentre la carte
        setTimeout(oMaps.center, 600);
    }
    function switchElementClass(elementId, oldClass, newClass) {
        var element = document.getElementById(elementId);
        switchClass(element, oldClass, newClass);
    }
    function updateStyle(name, elementId, value) {
        var element = document.getElementById(elementId);
        element.style[name] = value;
    }

    function createMap() {

        if (oMaps == null)
            return;

        initIndexes();
        oMaps.init();

        // loadData(function (data) {
        oMaps.createMap(
        {
            mapContainer: document.getElementById("maps-provider"),
            width: oSize.width,
            height: oSize.height,
            zoom: 5,
            location:
            {
                latitude: '46.641998291015625',
                longitude: '2.3380000591278076'
            },
            animation: true
            //,
            // pinsData: data
        });
        //});
    }
    function initIndexes() {
        maxRows = 0;
        var oeParam = getParamWindow();
        rows = oeParam.GetNumRows();
        filterValue = "";

        pageIndex = 1;
        var inputElement = document.getElementById("inputNumpage");
        if (inputElement)
            pageIndex = inputElement.value;

    }
    function loadData(callback) {

        if (filterValue.length > 0) {

            if (expressFilterCol && expressFilterCol > 0) {
                var updatePref = "tab=" + nGlobalActiveTab + ";$;filterExpress=" + expressFilterCol + ";|;20;|;" + filterValue + ";|;";
                updateUserPref(updatePref, top.loadList);
                filterValue = "";
            }

        } else {
            var upd = new eUpdater("mgr/eCartographyManager.ashx", 0);

            upd.ErrorCallBack = function () {
                //setWait(false)
            };

            upd.addParam("tab", nGlobalActiveTab, "post");
            upd.addParam("maxRows", maxRows, "post");
            upd.addParam("rows", rows, "post");
            upd.addParam("page", pageIndex, "post");
            upd.addParam("geofilter", filterValue, "post");

            // setWait(true);
            upd.send(function (oRes) {

                if (oRes == null)
                    return;

                var success = getXmlTextNode(oRes.getElementsByTagName("success")[0]) == "1";
                if (success) {

                    var mapsRecords = new Array();

                    var content = oRes.getElementsByTagName("records")[0];

                    expressFilterCol = getAttributeValue(content, "filter");
                    var tabIcon = getAttributeValue(content, "icon");
                    var records = oRes.getElementsByTagName("record");

                    var record = {};
                    var infobox = {};
                    var pushpin = {};

                    for (var i = 0; i < records.length; i++) {

                        record = records[i];
                        var infobox = new Object();

                        infobox.icon = GetMappedObject(record.getElementsByTagName("ruleIcon")[0]);
                        if (infobox.icon.value == "")
                            infobox.icon.value = tabIcon;

                        infobox.image = GetMappedObject(record.getElementsByTagName("image")[0]);
                        infobox.title = GetMappedObject(record.getElementsByTagName("title")[0]);
                        infobox.subTitle = GetMappedObject(record.getElementsByTagName("subtitle")[0]);

                        infobox.fields = new Array();
                        infobox.fields.push(GetMappedObject(record.getElementsByTagName("field00")[0]));
                        infobox.fields.push(GetMappedObject(record.getElementsByTagName("field01")[0]));
                        infobox.fields.push(GetMappedObject(record.getElementsByTagName("field02")[0]));
                        infobox.fields.push(GetMappedObject(record.getElementsByTagName("field03")[0]));
                        infobox.fields.push(GetMappedObject(record.getElementsByTagName("field04")[0]));

                        var pushpin = new Object();
                        pushpin.bgColor = getXmlTextNode(record.getElementsByTagName("bgColor")[0]) + "";
                        pushpin.ruleColor = getXmlTextNode(record.getElementsByTagName("ruleColor")[0]) + "";

                        data = new Object();
                        data.id = getXmlTextNode(record.getElementsByTagName("fileId")[0]);
                        data.geo = GetMappedObject(record.getElementsByTagName("geography")[0]);
                        data.marked = getXmlTextNode(record.getElementsByTagName("marked")[0]) == "1";

                        data.pushpin = pushpin;
                        data.infobox = infobox;

                        mapsRecords.push(data);
                    }

                    callback(mapsRecords);


                } else {
                    var info = getXmlTextNode(oRes.getElementsByTagName("info")[0]);
                    alert(info);
                }
            }

            );
        }



        // recupère le descid et la valeur
        function GetMappedObject(element) {
            var obj = new Object();
            obj.descid = getAttributeValue(element, "descid");
            obj.value = getXmlTextNode(element);
            obj.onclick = getAttributeValue(element, "onclick");
            return obj;
        }
    }

    function disposeMap() {
        if (oMaps == null)
            return;

        setTimeout(oMaps.dispose, 800);
    }

    function manageMapContainer(src) {

        // L'iframe n'est pas chargée
        if (oMaps == null)
            return;

        var state = getAttributeValue(src.parentElement, "state");

        switch (state) {
            case "close":
                if (src.id == "first-maps-btn") {
                    oSize = oSmallSize;
                    openMapContainer();
                    reloadData();
                    state = "open";
                }

                break;

            case "open":
                if (src.id == "first-maps-btn") {
                    oSize = oFullSize;
                    expandMapContainer();
                    state = "expand";

                } else {
                    closeMapContainer();
                    state = "close";
                }

                break;

            case "expand":
                closeMapContainer();
                state = "close";

                break;
        }

        setAttributeValue(src.parentElement, "state", state);
    }

    function manageMapToolBar(src) {

        switch (src.id) {
            case 'refresh-maps-btn':
                initIndexes();
                filterValue = oMaps.getWktPolygon();
                //  oMaps.erase();
                if (filterValue.length > 0)
                    reloadData();

                break;
            case 'adv-maps-btn':
                break;
            case 'edit-maps-btn':

                var mapContainer = document.getElementById("maps-provider");
                var mode = getAttributeValue(src, "mode");
                if (mode == MapMode.EDIT) {
                    switchClass(src, "background-theme", "background-hover-theme");
                    switchClass(mapContainer, "default-cursor", "edit-cursor");
                    setAttributeValue(src, "mode", MapMode.ERASE);
                    setAttributeValue(src, "title", top._res_7491);
                    oMaps.draw();
                } else {
                    // MapMode.ERASE
                    switchClass(src, "background-hover-theme", "background-theme");
                    switchClass(mapContainer, "edit-cursor", "default-cursor");
                    setAttributeValue(src, "mode", MapMode.EDIT);
                    setAttributeValue(src, "title", top._res_7490);
                    oMaps.erase();
                }

                break;
            case "export-maps-btn":
                oMaps.exportMap();
                break;
            case 'maps-item-page':
                // var filter = document.getElementById("adv-maps-btn");
                //  setAttributeValue(filter, "active", "0");
                //  setAttributeValue(filter, "title", "Liste en cours");
                initIndexes();
                reloadData();
                break;
            case 'maps-item-xxx':
                // var filter = document.getElementById("adv-maps-btn");
                // setAttributeValue(filter, "active", "1");
                // setAttributeValue(filter, "title", NB_DEFAULT_MAX + " premières fiche");
                rows = NB_DEFAULT_MAX;
                maxRows = 0;
                pageIndex = 1;
                reloadData();
                break;
            case 'maps-item-all':
                // var filter = document.getElementById("adv-maps-btn");
                // setAttributeValue(filter, "active", "1");
                // setAttributeValue(filter, "title", "Toutes les fiches");
                maxRows = 1;
                pageIndex = 1;
                reloadData();
                break;
            case 'maps-item-search':
                break;
        
            default:
                break;
        }
    }

    function reloadData() {

        if (oMaps == null)
            return;

        loadData(function (data) {
            oMaps.refreshPushPin(data);
        });

    }

    var memClass = null;
    function highlightLine(evt) {

        if (evt && evt.data && evt.data.metadata) {

            var tr_eid = nGlobalActiveTab + '_';
            var trs = document.querySelectorAll("tr[eid^='" + tr_eid + "']");
            var tr, id;
            if (trs) {
                for (var i = 0; i < trs.length; i++) {
                    tr = trs[i];
                    if (tr.className.indexOf('highlight') >= 0)
                        switchClass(tr, tr.className, tr.className.replace(' highlight', ''));

                    id = (getAttributeValue(tr, "eid") + "").split("_")[1];
                    if (tr && evt.name == "pushpin-over" && id == evt.data.metadata.fileId)
                        switchClass(tr, tr.className, tr.className + ' highlight');
                }
            }
        }
    }

    function markLine(evt) {
        //
        var tr_eid = nGlobalActiveTab + '_' + evt.data.fileId;
        var tr = document.querySelector("tr[eid='" + tr_eid + "']");

        if (tr && tr.children.length > 0 && tr.children[0].children.length > 0) {
            var cbo = tr.children[0].children[0];
            chgChk(cbo);
            applyChkUpdate(tr, cbo);
        }
    }


    function fileAllMarked(evt) {
        if (oMaps != null)
            oMaps.markAllFiles({ marked: evt.data.marked });
    }

    function listMouseOver(evt) {
        var target = evt.data.mouseevent.srcElement || evt.data.mouseevent.target;
        if (target) {

            if (target.tagName == "BODY") {
                inspect(null);
                return;
            }

            while (target.tagName != "BODY") {
                if (inspect(target))
                    break;

                target = target.parentElement;
            }
        }
    }

    function listMouseOut(evt) {
        highlightPushpin(0);
    }

    // On cherche si le mouse over est sur une TR en mode liste
    function inspect(element) {

        if (element == null) {
            // on désactive l'ancien
            highlightPushpin(0);
            return true;
        }

        if (element && element.tagName == "TR") {
            var eid = getAttributeValue(element, "eid");
            if (eid && eid.length > 0) {
                var tab = eid.split("_");
                if (tab && tab.length == 2 && tab[0] == nGlobalActiveTab) {
                    highlightPushpin(tab[1]);
                    return true;
                }
            }
        }

        return false;
    }

    var lastId = 0;
    function highlightPushpin(newId) {
        if ((newId == 0 && lastId == 0) || oMaps == null)
            return;

        oMaps.highlightPushpin({
            lastId: lastId, newId: newId
        });
        lastId = newId;
    }

    function fileMarked(evt) {
        if (oMaps != null)
            oMaps.markInfobox({ fileId: evt.data.fileId });
    }

    function setViewPort(evt) {

        if (!canDisplay()) {
            return;
        }

        var viewPort = evt.data;
        var container = document.getElementById("carto-container");
        if (container) {
            container.style.display = "block";
            container.style.top = (viewPort.offsetTop + 23) + "px";// 23 hauteur du header des filtres express

            var width = isTablet() ? 0 : GetRightMenuWidth();
            container.style.right = (width + 6) + "px";
        }

        oFullSize.width = viewPort.width;
        oFullSize.height = viewPort.height;

        oSmallSize.width = (viewPort.width / 3) + 12;
        oSmallSize.height = viewPort.height;

        var mapsContainer = document.getElementById("maps-container");
        if (mapsContainer)
            mapsContainer.style.height = (viewPort.height - 23) + "px";// 23 hauteur du footer des filtres lettre



        if (viewPort.bInitSizeList == true) {

            if (oMaps != null)
                oMaps.reset();

            initIndexes();

            // On recharge les données que si la map est ouverte
            var mapState = document.getElementById("maps-open");
            var state = mapState ? getAttributeValue(mapState, "state") : "close";
            if (state != "close")
                reloadData();
        }
    }

    function updateCartoDisplay(evt) {

        // 0 : accueil // 1 : liste // 2 : fiche // 3 : Modification // 4 : vcard // 5 : creation // 6 : Impression // 7 : Administration // 8 : onglet web
        if (evt.data.type != 1 || !canDisplay()) {

            if (oMaps == null)
                return;

            closeMapContainer();
            setAttributeValue(document.getElementById('maps-open'), "state", "close");
            updateStyle("display", "carto-container", "none");
        } else {

            if (oMaps == null)
                loadMapsIframe();
        }
    }

    function canDisplay() {

        var element = document.getElementById('mapDisplay');
        return element && element.value == "1";
    }

    var mapsIFrameId = "eMapsIFrame";
    function loadMapsIframe() {
        unloadMapsIframe();
        var iframe = document.createElement("iframe");

        setAttributeValue(iframe, "id", mapsIFrameId);
        setAttributeValue(iframe, "src", "eMapsIFrame.aspx");
        setAttributeValue(iframe, "class", "maps-iframe");

        var provider = document.getElementById("maps-provider");
        provider.appendChild(iframe);

    }

    function unloadMapsIframe() {

        if (oMaps != null)
            oMaps.dispose();

        var provider = document.getElementById("maps-provider");
        provider.innerHTML = "";
        oMaps = null;
    }

    function mapsLoaded(evt) {
        oMaps = evt.data.oMapsInterface;
        createMap();
    }

    function hide(evt) {
       // unloadMapsIframe();
        closeMapContainer();
        setAttributeValue(document.getElementById('maps-open'), "state", "close");
        updateStyle("display", "carto-container", "none");

    }

    return {
        init: function () {
            // On s'abonne à ces evenements,
            oEvent.on("list-bounds-changed", setViewPort);
            oEvent.on("right-menu-loaded", updateCartoDisplay);
            //oEvent.on("pushpin-click", highlightLine);
            oEvent.on("pushpin-over", highlightLine);
            oEvent.on("pushpin-out", highlightLine);
            oEvent.on("list-mouseover", listMouseOver);
            oEvent.on("list-mouseout", listMouseOut);
            oEvent.on("file-marked", fileMarked);
            oEvent.on("file-all-marked", fileAllMarked);
            oEvent.on("cbo-check", markLine);
            oEvent.on("maps-iframe-loaded", mapsLoaded);
            oEvent.on("load-admin", hide);
            oEvent.on("mode-file", hide); 
            oEvent.on("mode-grid", hide); 
        },
        onClick: function (evt) {
            var src = evt.srcElement || evt.target;
            switch (src.id) {
                case 'first-maps-btn':
                case 'second-maps-btn':
                    manageMapContainer(src);
                    break;
                default:
                    manageMapToolBar(src);

                    break;
            }
            stopEvent(evt);
        },
        openMap: function () {
            var src = document.getElementById("first-maps-btn");
            manageMapContainer(src);
        }
    }
})();

oCartography.init();