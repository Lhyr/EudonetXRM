
/// <summary>
/// Gestionnaire de l'ensemble des grilles de la session
/// </summary>
var oGridManager = (function () {

    /// <summary>
    /// Le gestionnaire est initialisé
    /// </summary>
    var _initialized = false;

    /// <summary>
    /// Conteneur de l'ensemble des grilles
    /// </summary>
    var _masterContainer;

    /// <summary>
    /// la div principale pour le mode liste 
    /// </summary>
    var _mainDiv;

    /// <summary>
    /// liste des grilles chargées sur le DOM 
    /// </summary>
    var _gridCollection = new Array();

    // Dernière fiche consultée (pour un signet grille)
    var _lastParentFileId = 0;

    var tabWidgetCallStack = { tabs: [0] };

    /// <summary>
    /// Initialisation du gestionnaire
    /// Chargemlent du js/css supplémentaires
    /// Abonnements aux différents évènments de l'application
    /// </summary>
    function internalInit() {

        _mainDiv = document.getElementById("mainDiv");

        // container des tables
        _masterContainer = document.getElementById("gw-container");
        if (_masterContainer == null) {
            _masterContainer = document.createElement("div");
            _masterContainer.id = "gw-container";
            document.body.appendChild(_masterContainer);
        }

        if (!_initialized) {
            _mainDiv.innerHTML = "";

            oEvent.on("mode-list", update);
            oEvent.on("mode-file", update);
            oEvent.on("load-admin", update);
            oEvent.on("bkm-loaded", update);
            oEvent.on("file-detail-display", update);

            initHomepageCssScripts();
            initHeadEvents();

            _initialized = true;
        }


        oEvent.on("file-resume-changed", removeGrid);
        oEvent.fire("log-info", { 'message': "GridSystem est chargé" });
    }

    function removeGrid(evt) {
        resetBkmGrids(evt.data.tab);
    }

    /**
     * On vérifie que la grille n'ait pas fait n'importe quoi, ce qui arrive.
     * Elle se met mais sans ID, si c'est le cas, on supprime tout, et on recrée un container.
     * Désactive les grilles.
     * tricks pour les cas où la grille reste.
     */
    function deactivateGrid() {

        var oGlobalWidget = document.querySelector("#gw-container");
        var oGridLeukemia = document.querySelector("div[class^='fs_'][class$='pt']:not([id])");

        if (oGridLeukemia && !oGlobalWidget) {
            oGridLeukemia.id = "gw-container";
            oGlobalWidget = oGridLeukemia;
        }
        else if (!oGlobalWidget) {
            oGlobalWidget = document.createElement("div");
            oGlobalWidget.id = "gw-container";
            oGlobalWidget.className = "fs_8pt";
            document.querySelector('body').appendChild(oGlobalWidget);
        }

        if (oGlobalWidget) {
            let gwTab = document.querySelectorAll("[id^='gw-tab-'], [id^='gw-bkm-']");

            if (gwTab && gwTab.length > 0) {
                for (i = 0; i < gwTab.length; i++) {
                    try {
                        gwTab[i].setAttribute("active", 0);
                    } catch (e) {
                        gwTab[i].remove();
                        continue;
                    }

                    if (gwTab[i].hasChildNodes())
                        for (nd = 0; nd < gwTab[i].childNodes.length; nd++) {
                            try {
                                gwTab[i].childNodes[nd].setAttribute("active", 0);
                            } catch (e) {
                                gwTab[i].childNodes[nd].remove();
                                continue;
                            }
                        }
                    else
                        gwTab[i].remove();
                }
            }
        }
    }
    /// <summary>
    /// En fonction de type d'evenment de l'application, on affiche/masque la grille correspondante 
    /// </summary>
    function update(evt) {

        switch (evt.name) {
            case "mode-list":
                deactivateGrid();
                if (evt.data.tab == 0)
                    refreshVisibility(115200);
                else
                    refreshVisibility(evt.data.tab);
                break;
            case "mode-file":
                deactivateGrid();
                break;
            // En cas de changement de fiche, on reset les signets grilles
            // if (evt.data.fileId != _lastParentFileId) {
            //     resetBkmGrids(evt.data.tab);
            //     _lastParentFileId = evt.data.fileId;
            // }

            case "mode-admin":
                deactivateGrid();
                break;
            //  case "file-detail-display":
            default:
                refreshVisibility(0);
                break;
        }
    }



    // Chargement du signet grille
    function loadBkmGrid(nBkm) {
        nBkm = nBkm * 1;
        oGridManager.loadBkmGrid(nBkm);
    }

    /// <summary>
    /// Mise à jour la visibilité de la grile en fonction de la table activée
    /// </summary>
    function refreshVisibility(tab) {

        var isBkm = false;
        var bOutOfOrdeCall = false;
        if (tabWidgetCallStack
            && tabWidgetCallStack.tabs.length > 0

        ) {
            var nLastCalledTab = tabWidgetCallStack.tabs[tabWidgetCallStack.tabs.length - 1];

            if (
                (nLastCalledTab == tab)
                || (nLastCalledTab == 0 && tab == 115200)
                || (nLastCalledTab == 115200 && tab == 0)) {
                //
            }
            else {
                // console.log("ASYNC ANOMALY :  ", tabWidgetCallStack.tabs[tabWidgetCallStack.tabs.length - 1], "/", tab);
                bOutOfOrdeCall = true;
            }

            if (tabWidgetCallStack.tabs.length > 2) {
                tabWidgetCallStack.tabs.shift();
            }
        }

        var master = document.getElementById("gw-container");
        if (!master)
            return;

        var tabs = master.querySelectorAll("div[id^='gw-tab-']");
        Array.prototype.slice.call(tabs).forEach(function (tabCtn) {
            setAttributeValue(tabCtn, "active", "0");
            var visibleGrids = tabCtn.querySelectorAll(".gw-grid[active='1']");
            Array.prototype.slice.call(visibleGrids).forEach(function (current) {
                setAttributeValue(current, "active", "0");
            });
        });

        var bkms = master.querySelectorAll("div[id^='gw-bkm-']");
        Array.prototype.slice.call(bkms).forEach(function (tabCtn) {
            if (tabCtn.id === 'gw-bkm-' + tab + '-0')
                isBkm = true;
            setAttributeValue(tabCtn, "active", "0");
            var visibleGrids = tabCtn.querySelectorAll(".gw-grid[active='1']");
            Array.prototype.slice.call(visibleGrids).forEach(function (current) {
                setAttributeValue(current, "active", "0");
            });


        });


        if (tab > 0 && (!bOutOfOrdeCall || isBkm === true)) {
            var selected = master.querySelector("div[id^='gw-tab-" + tab + "-']");
            if (selected) {
                setAttributeValue(selected, "active", "1");
            } else {
                selected = master.querySelector("div[id^='gw-bkm-" + tab + "-']");
                if (selected)
                    setAttributeValue(selected, "active", "1");
            }
        }
    }

    function hideGrids() {

        var master = document.getElementById("gw-container");
        if (!master)
            return;

        var tabs = master.querySelectorAll("div[id^='gw-tab-']");
        Array.prototype.slice.call(tabs).forEach(function (tabCtn) {
            setAttributeValue(tabCtn, "active", "0");
            var visibleGrids = tabCtn.querySelectorAll(".gw-grid[active='1']");
            Array.prototype.slice.call(visibleGrids).forEach(function (current) {
                setAttributeValue(current, "active", "0");
            });
        });
    }

    // Vide les signets grilles de la table parente
    function resetBkmGrids(parentTab) {

        var master = document.getElementById("gw-container");
        if (!master)
            return;

        var bkmTabs = master.querySelectorAll(".gw-tab[data-bkm='1'][data-parenttab='" + parentTab + "']");

        for (var i = 0; i < bkmTabs.length; i++) {

            var arrBkmTabId = bkmTabs[i].id.split('-');
            if (arrBkmTabId.length < 3)
                continue;

            var bkm = arrBkmTabId[2];

            var grids = bkmTabs[i].querySelectorAll(".gw-grid");
            for (var j = 0; j < grids.length; j++) {
                var arrGridId = grids[j].id.split('-');
                if (arrGridId.length < 3)
                    continue;

                deleteGrid(arrGridId[2]);
            }
        }
    }

    /// <summary>
    /// Si le sous-menu des grille existe on le charge
    /// </summary>
    function loadGridSubMenu(nTab, nGrid, callback) {
        var mainDiv = document.getElementById("mainDiv");
        if (!mainDiv)
            return;

        var oUpdater = new eUpdater("mgr/eSubMenuManager.ashx", 1);
        oUpdater.ErrorCallBack = function () { setWait(false); };
        oUpdater.addParam("nTab", nTab, "post");
        oUpdater.addParam("nGrid", nGrid, "post");
        oUpdater.send(function (oRes) {

            // On vire le contenu 
            mainDiv.innerHTML = "";
            var mainListContent = document.createElement("div");
            mainListContent.id = "mainListContent";

            var listHeader = document.createElement("div");
            listHeader.id = "listheader";

            mainDiv.appendChild(mainListContent);
            mainDiv.appendChild(listHeader);

            mainListContent.innerHTML = oRes;


            callback();

            setWait(false);
        });
    }

    /// <summary>
    /// Chargement du contenu de la grille
    /// </summary>
    function loadGridContent(nTab, nGrid) {

        var container = document.getElementById("SubTabMenuCtnr");
        if (!container)
            return;

        setAttributeValue(container, "ctn", "mainDiv");

        var selected = container.querySelector("span a[selected='1']");

        if (!selected) {
            var all = container.querySelectorAll("span a");
            if (all != null && all.length > 0) {
                selected = all[0];
                setAttributeValue(selected, "selected", "1");
            } else {
                console.log("Pas de grille pour le signet grille");
                return;
            }
        }

        if (selected) {
            selected.className = "subTab selected";
            var itemtype = getAttributeValue(selected, "itemtype") * 1;
            if (nGrid > 0 && itemtype == 1) {

                var manager = getGrid(nGrid, {
                    'type': 'tab',
                    'tab': nTab,
                    'fileId': 0,
                    'gridId': nGrid,
                    'ref': document.getElementById("listheader")
                });

                refreshVisibility(nTab);
                manager.load();
                setGridParamWindowKey('FirstSubTab_' + nTab, nGrid);

            } else {
                selected.click();
            }
        }
    }

    /// <summary>
    /// Si le sous-menu des grille en signet existe on le charge
    /// </summary>
    function loadBkmGridSubMenu(nBkm, nGrid, callback) {
        var mainDiv = document.getElementById("divBkmCtner");
        if (!mainDiv)
            return;
        var oUpdater = new eUpdater("mgr/eSubMenuManager.ashx", 1);
        oUpdater.ErrorCallBack = function () { setWait(false); };
        oUpdater.addParam("nTab", nBkm, "post");
        oUpdater.addParam("nGrid", nGrid, "post");
        oUpdater.send(function (oRes) {
            // On vire le contenu 
            mainDiv.innerHTML = "";
            var bkmDiv = document.createElement("div");
            bkmDiv.id = "bkm_" + nBkm;
            bkmDiv.className = "bkmdiv BkmWeb";
            mainDiv.appendChild(bkmDiv);

            //  var tabMainDivWH = GetMainDivWH();
            var divBkmPres = document.getElementById("divBkmPres");
            bkmDiv.style.height = (parseInt(divBkmPres.style.height) - 40) + "px";
            // bkmDiv.style.width = (tabMainDivWH[0] - 20) + "px";

            var mainListContent = document.createElement("div");
            mainListContent.id = "mainListContent";
            mainListContent.className = "bkmTitle";

            var listHeader = document.createElement("div");
            listHeader.id = "listheader";

            bkmDiv.appendChild(mainListContent);
            bkmDiv.appendChild(listHeader);

            mainListContent.innerHTML = oRes;
            callback();

            setWait(false);
        });
    }

    /// <summary>
    /// Chargement du contenu de la grille du signet
    /// </summary>
    function loadBkmGridContent(nBkm, nGrid) {

        var mainDiv = document.getElementById("divBkmCtner");
        if (!mainDiv)
            return;

        var container = mainDiv.querySelector("#SubTabMenuCtnr");
        if (!container)
            return;

        setAttributeValue(container, "ctn", "bkm_" + nBkm);

        var selected = container.querySelector("span a[selected='1']");
        if (!selected) {
            var all = container.querySelectorAll("span a");
            if (all != null && all.length > 0) {
                selected = all[0];
                setAttributeValue(selected, "selected", "1");

            } else {
                console.log("Pas de grille pour le signet grille");
                return;
            }
        }

        selected.className = "subTab selected";

        var ref = mainDiv.querySelector("#listheader");
        if (ref == null) {
            ref = document.createElement("DIV");
            ref.id = "listheader";
            ref.className = "listheader";
            ref.appendChild(GetDefaultMessage());
            mainDiv.appendChild(ref);
        }



        var itemtype = getAttributeValue(selected, "itemtype") * 1;
        nGrid = nGrid || (getAttributeValue(selected, "gid") * 1);
        if (nGrid > 0 && itemtype == 1) {

            var bAutoRefresh = getAttributeValue(document.getElementById("emptyGridPanel"), "are") == "1";
            if (bAutoRefresh) {
                refreshVisibility(0);
                oGridToolbar.refresh();
                return;
            }

            if (!_gridCollection.hasOwnProperty(nGrid)) {
                return;
            }

            // ref.innerHTML = "";

            var divBkmPres = eTools.getBookmarkContainer(nBkm);
            var h = (parseInt(divBkmPres.style.height) - 4);
            var w = parseInt(divBkmPres.style.width);
            var pTab = 0;
            var pFileId = 0;
            if (nGlobalActiveTab != nBkm && typeof (top.GetCurrentFileId) == "function") {
                pTab = nGlobalActiveTab;
                pFileId = top.GetCurrentFileId(nGlobalActiveTab) * 1;
            }


            var manager = getGrid(nGrid, {
                'type': 'bkm',
                'tab': nBkm,
                'fileId': 0,
                'gridId': nGrid,
                'parentTab': pTab,
                'parentFileId': pFileId,
                'ref': ref,
                'height': h,
                'width': w
            });

            refreshVisibility(0);
            manager.display();
            setGridParamWindowKey('FirstSubTab_' + nBkm, nGrid);
        }
    }


    /// Affiche un message incitant les users a cliquer pour rafraichir
    function GetRefreshMessageContent() {
        var panel = document.createElement("DIV");
        panel.id = "emptyGridPanel";
        var text = document.createElement("DIV");
        text.id = "info";
        text.innerHTML = top._res_8573 + "<span class='icon-forward' id='infoIcon'></span>";
        panel.appendChild(text);
        return panel;
    }


    function deleteGrid(gridId) {
        var grid = document.getElementById("gw-grid-" + gridId);
        grid.parentElement.removeChild(grid);

        if (_gridCollection.hasOwnProperty(gridId))
            delete _gridCollection[gridId];

        oEvent.fire("log-info", { 'message': "Grille supprimée" });


    }

    /// <summary>
    /// Remise à zéro des paramètres de la grille 
    /// </summary>
    function resetGrid(gridId, context) {
        if (_gridCollection.hasOwnProperty(gridId)) {
            if (_gridCollection[gridId])
                _gridCollection[gridId].dispose();

            _gridCollection[gridId] = null;
            delete _gridCollection[gridId];

            oEvent.fire("log-info", { 'message': "Gestionnaire du grille supprimé", 'context': context });
        }


        return getGrid(gridId, context);
    }

    /// <summary>
    /// Remise à zéro des grilles 
    /// </summary>
    function resetGridAll() {
        _gridCollection = new Array();
        _masterContainer.innerHTML = "";
    }

    /// <summary>
    /// retourne un objet "eGrid" définit par son identifiant, sinon on en crée un
    /// </summary>
    function getGrid(gridId, context) {
        if (_gridCollection.hasOwnProperty(gridId)) {
            var m = _gridCollection[gridId];
            if (typeof (context) != 'undefined' && context != null) {
                m.setContext(context);
            }
            return m;
        }

        return createNewGrid(context);
    }

    /// <summary>
    /// Creation d'un objet eGrid avec le context en paramètre
    /// </summary>
    function createNewGrid(context) {
        if (typeof (context) == 'undefined' || context == null)
            return null;

        var grid;
        // if (context.type == "bkm")
        //     grid = new eGridBkm(context);
        // else
        grid = new eGrid(context);

        _gridCollection[context.gridId] = grid;

        oEvent.fire("log-info", { 'message': "Nouveau gestionnaire du grille est crée", 'context': context });

        return grid;
    }

    /// ********************
    /// Methodes publiques
    /// ********************
    return {

        /// <summary>
        /// Collection de grilles disponibles sur le DOM
        /// </summary>
        '_': _gridCollection,

        /// <summary>
        /// Initialisation depuis l'extérieur
        /// </summary>
        'init': internalInit,

        /// <summary>
        /// On réinitialise le DOM des grilles/widgets
        /// </summary>
        'reset': function () {
            resetGridAll();
            tabWidgetCallStack = { tabs: [0] };
        },

        'resetStack': function () {
            tabWidgetCallStack = { tabs: [0] };
        },

        'addTabToStack': function (tab) {
            tabWidgetCallStack.tabs.push(tab);
        },

        /// <summary>
        /// Mise à jour de la visibilitie de la grille associée à la table
        /// </summary>
        'refreshVisibility': function (tab) {
            refreshVisibility(tab);
        },

        hideGrids: hideGrids,

        /// <summary>
        /// Savoir si la page d'accueil existe pour l'utilisateur en cours
        /// </summary>
        'pageExists': function () {
            var oParam = getParamWindow();
            return oParam.GetParam("XrmHomePageId") * 1 > 0;
        },

        /// <summary>
        /// Chargement de la première grille de la page d'accueil 
        /// </summary>
        'loadPage': function () {
            var gridId = getParamWindowKey('FirstSubTab_115200', 0);
            loadGridSubMenu(115200, gridId, function () {
                switchActiveTab(0);
                loadGridContent(115200, gridId);
            });
        },

        /// <summary>
        /// Chargement de la première grille de l'onglet actif
        /// </summary>
        'loadTabGrid': function (nTab) {
            var gridId = getParamWindowKey('FirstSubTab_' + nTab, 0);
            loadFileMenu(nTab, 8, 0);
            this.loadGrid(nTab, gridId);
        },

        /// <summary>
        /// Chargement de la première grille du signet actif
        /// </summary>
        'loadBkmGrid': function (nBkm) {

            //setWait(true);
            loadBkmGridSubMenu(nBkm, 0, function () {

                // Changment du visuel
                switchBkmSelection(getActiveBkm(nGlobalActiveTab), nBkm);

                // Changement des prefs
                setActiveBkm(nGlobalActiveTab, nBkm);

                swFileBkm(1);

                loadBkmGridContent(nBkm, 0);
            });
        },

        /// <summary>
        /// Si la grille est dans le cache on l'affiche sinon on laisse l'utilisateur cliquer sur le bouton pour rafraichir
        /// </summary>
        'showBkmGrid': function (nBkm) {
            loadBkmGridContent(nBkm, 0); // 0 pour charger la première dans le menu
        },

        /// <summary>
        /// retourne la grille identifiée
        /// </summary>
        'getGrid': function (gridId, context) {
            return getGrid(gridId, context);
        },

        /// <summary>
        /// remis-à-zéro la grille avec le nouveau context
        /// </summary>
        'resetGrid': function (gridId, context) {
            return resetGrid(gridId, context);
        },

        /// <summary>
        /// Chargement de la grille identifiée
        /// </summary>
        'loadGrid': function (nTab, nGrid) {
            loadGridSubMenu(nTab, nGrid, function () {
                switchActiveTab(nTab);
                loadGridContent(nTab, nGrid);
            });
        },

        /// <summary>
        /// Chargement de la grille identifiée
        /// </summary>
        'dispose': function () {
            //TODO netoies
        }
    }
})();


