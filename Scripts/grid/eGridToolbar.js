///
/// Objet GridToolbar représente les intéraction entre les grilles, les boutons options
///
var oGridToolbar = (function () {

    function internalInit() { }

    // Clique sur les boutons de la navbar de l agrille
    function click(event) {

        var evt = event || window.event;
        var src = evt.srcElement || evt.target;

        if (!src) return;

        var action = getAttributeValue(src, "action");
        switch (action) {
            case "refresh":
                refresh();
                break;
            case "options":
                showHiddenWidget();
                break;
        }
    }

    // Actualise la grille en rafraichissant tous les widgets associés
    function refresh() {
        var GRID_TYPE = 1;
        var XRM_HOMAPAGE_TAB = 115200;
        var container = document.getElementById("SubTabMenuCtnr");
        var nTab = getAttributeValue(container, "tab");
        var nParentTab = nGlobalActiveTab;

        var selected = container.querySelector("span a[selected='1']");
        if (selected) {
            var gridId = getAttributeValue(selected, "gid") * 1;
            var itemtype = getAttributeValue(selected, "itemtype") * 1;
            if (gridId > 0 && itemtype == 1) {
                nTab = nTab <= 0 ? XRM_HOMAPAGE_TAB : nTab;

                var grid = document.getElementById("gw-grid-" + gridId);
                if (grid != null)
                    grid.parentElement.removeChild(grid);

                var containerId = getAttributeValue(SubTabMenuCtnr, "ctn");
                if (containerId == "") containerId = "mainDiv";

                // var ctn = document.getElementById(containerId);               
                // var h = parseInt(ctn.style.height) - 4;
                // var w = parseInt(ctn.style.width);

                var oContext = new eGridContext();
                oContext.tab = nTab;
                oContext.fileId = getFileId(oContext.tab);
                oContext.gridId = gridId;
                oContext.ref = document.getElementById("listheader");
                oContext.height = 0;
                oContext.width = 0;
                oContext.parentTab = nParentTab;
                if (nParentTab > 0 && typeof (top.GetCurrentFileId) == "function") {
					if (typeof top.GetCurrentFileId === "function")
						oContext.parentFileId = top.GetCurrentFileId(nParentTab);
                    if (oContext.parentFileId) {
                        oContext.type = "bkm";
                        oContext.gridLocation = eGridLocation.BKM;
                    }                   
                }

                manager = oGridManager.resetGrid(gridId, oContext);
                oGridManager.refreshVisibility(nTab);
                manager.load();
                // On sauvegarde dans eParamIframe a valider avec MJO
                setGridParamWindowKey('FirstSubTab_' + nTab, gridId);
            }
        }
    }

    // Affiche de nouvelles widgets ratachés à la grille qui sont masqués
    function showHiddenWidget() {

        var container = document.getElementById("SubTabMenuCtnr");
        var selected = container.querySelector("span a[selected='1']");

        var gridId = getAttributeValue(selected, "gid") * 1;
        var oMultiSelect = new eMultiSelect(eMultiSelect.eType.Widget, {
            'title': top._res_8168, //'Sélection des widgets',
            'autoClose': true,
            'size': {
                'width': 800,
                'height': 530
            }
        });

        oMultiSelect.setParam('gridId', gridId);
        oMultiSelect.onValidate(
            function updateSelectedValue(result) {
                if (result.success == '1')
                    oGridController.pref.setVisible({
                        'ids': result.data.ids,
                        'gridId': gridId,
                        'callback': function (args) {
                            refresh();
                        }
                    });
            });

        oMultiSelect.onCancel(function (result) { });
        oMultiSelect.show();
    }

    // On charge la grille depuis le serveur
    function load(event) {

        var evt = event || window.event;
        var src = evt.srcElement || evt.target;

        if (!src) return;

        resetAllItems();
        setAttributeValue(src, "selected", "1");
        addClass(src, "selected");


        var list = getAttributeValue(src, "list") * 1;
        var gridId = getAttributeValue(src, "gid") * 1;
        var type = getAttributeValue(src, "itemtype") * 1;

        loadById(gridId, type, list, event);


    }

    function loadById(nGridId, nItemType, nList, event) {

        if (typeof nList === "undefined")
            nList = 0;

        var SubTabMenuCtnr = document.getElementById('SubTabMenuCtnr');
        var nTab = getAttributeValue(SubTabMenuCtnr, "tab");
        var nParentTab = nGlobalActiveTab;

        if (nList != 1)
            oEvent.fire("mode-grid", { 'tab': nTab, 'gridId': nGridId, 'type': nItemType })

        var oeParam = getParamWindow();
        oeParam.SetParam('FirstSubTab_' + nTab, nGridId);

        var GRID_TYPE = 1;
        var XRM_HOMAPAGE_TAB = 115200;


        var listheader = document.getElementById('listheader');
        if (listheader)
            listheader.innerHTML = '';

        var listFilters = document.getElementById("listfiltres");
        if (listFilters)
            listFilters.parentElement.removeChild(listFilters);


        var tabInfos = document.getElementById("tabInfos");
        if (tabInfos)
            tabInfos.parentElement.removeChild(tabInfos);

        var infos = document.getElementById("infos");
        var toolbar = infos.querySelector(".grid-toolbar-ctn");

        // Dans le cas de la grille on regarde si elle est déjà sur le DOM
        if (nItemType == GRID_TYPE) {

            var containerId = getAttributeValue(SubTabMenuCtnr, "ctn");
            if (containerId == "") containerId = "mainDiv";

            var ctn = document.getElementById(containerId);
            var h = parseInt(ctn.style.height) - 4;
            var w = parseInt(ctn.style.width);

            if (toolbar)
                setAttributeValue(toolbar, "toolbar", "1");


            var oContext = new eGridContext();
            oContext.tab = nTab <= 0 ? XRM_HOMAPAGE_TAB : nTab;
            oContext.fileId = getFileId(oContext.tab);
            oContext.gridId = nGridId;
            oContext.ref = ctn.querySelector("#listheader");
            oContext.height = h;
            oContext.width = w;
            oContext.parentTab = nParentTab;
            if (nParentTab) {
                if (typeof top.GetCurrentFileId === "function")
                    oContext.parentFileId = top.GetCurrentFileId(nParentTab);
                if (oContext.parentFileId)
                    oContext.gridLocation = eGridLocation.BKM;               
            }


            oGridManager.refreshVisibility(oContext.tab);

            var grid = oGridManager.getGrid(nGridId, oContext);
            grid.load();

            // On sauvegarde dans eParamIframe a valider avec MJO
            setGridParamWindowKey('FirstSubTab_' + oContext.tab, nGridId);

        } else {


            if (toolbar)
                setAttributeValue(toolbar, "toolbar", "0");

            oWebMgr.loadSpec(nGridId, nItemType, event);

        }
    }

    function resetAllItems() {

        var SubTabMenuCtnr = document.getElementById("SubTabMenuCtnr");
        var items = SubTabMenuCtnr.querySelectorAll(".subTab");
        Array.prototype.slice.call(items).forEach(function (item) {

            removeClass(item, "selected");
            setAttributeValue(item, "selected", "0");
        });
    }

    // Retourne l'id de la fiche pages d'accueil
    function getFileId(nTab) {
        var master = document.getElementById("gw-container");
        if (!master)
            return 0;

        var grid = master.querySelector("div[id^='gw-tab-" + nTab + "-']");
        if (grid) {
            var parts = grid.id.split("-");
            if (parts.length == 4)
                return parts[3] * 1;
        }

        return 0;
    }

    function setSelected(gridId) {
        var SubTabMenuCtnr = document.getElementById("SubTabMenuCtnr");
        var item = SubTabMenuCtnr.querySelector("span a[gid='" + gridId + "']");
        if (!item)
            return;

        addClass(item, "selected");
        setAttributeValue(item, "selected", "1");

    }

    internalInit();

    return {
        reset: function () {
            resetAllItems();
        },
        setSelected: setSelected,
        load: function (event) { load(event); },
        refresh: function () { refresh(); },
        click: function (event) { click(event); },
        loadById: function (nGridId, nItemType, nList, event) {
            loadById(nGridId, nItemType, nList, event);
        }
    }
})();

