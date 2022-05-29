
// Objet permettant la gestion du menu de droite de la page d'accueil en paramètrage
var oAdminGridMenu = (function () {
    var oModalWidgetList = null;
    var modalAdvSearch = null;
    // Type de chargement
    // garder la meme enum que le serveur
    var MENU_PART = {
        'LOAD_ALL': 0,
        'LOAD_CONTENT': 1,
        'LOAD_WIDGET_CONFIG': 2,
        'LOAD_PAGE_CONFIG': 3,
        //'LOAD_WIDGET_PROPERTIES': 4
    };

    // Type d'action des tuiles
    // garder la meme enum que le serveur
    var XrmWidgetTileAction = {
        Unspecified: 0,
        OpenWebpage: 1,
        OpenSpecif: 2,
        OpenTab: 3,
        CreateFile: 4
    }

    var oModalWidgetList = null;
    var TAB = 115000; // table des pages d'accueil xrm  

    // récupère la fiche si 0 en crée une nouvelle
    function loadMenu(tab, id, context, callback) {


        if (top.oGridManager && context)
            top.oGridManager.addTabToStack(context.tab);

        //Resize du menu
        var winSize = top.getWindowSize();

        // ajour de la feuille css pour le menu
        addCss("eAdminMenu", "ADMIN");

        var oFileMenuParam = new eUpdater("mgr/eXrmHomeMenuParamManager.ashx", 1);
        oFileMenuParam.asyncFlag = false;
        oFileMenuParam.addParam("tab", tab, "post");
        oFileMenuParam.addParam("part", MENU_PART.LOAD_ALL, "post");
        oFileMenuParam.addParam("fileid", id, "post");
        oFileMenuParam.addParam("height", winSize.h, "post");
        oFileMenuParam.addParam("width", winSize.dw, "post");
        if (nGlobalActiveTab) {
            oFileMenuParam.addParam("parenttab", nGlobalActiveTab, "post");
            oFileMenuParam.addParam("parentfid", top.GetCurrentFileId(nGlobalActiveTab), "post");
            if (context)
                oFileMenuParam.addParam("gridlocation", context.gridLocation, "post");
        }

        oFileMenuParam.send(function (oRes) { loadMenuReturn(oRes, tab, id, callback); });
    }

    // Charge le menu du paramétrage
    function loadMenuReturn(oRes, tab, id, callback) {
        var menu = document.getElementById("rightMenu");

        menu.innerHTML = oRes;

        // initialise le menu avec le events js
        initEvents(menu);

        initParamTabEvents({ tab: tab, id: id }, 1);
        initParamTabEvents({ tab: tab, id: id }, 3);

        // callback client
        callback();

    }

    // Charge une partie de menu de droite
    function loadMenuPart(options, callback) {

        //Resize du menu
        var winSize = top.getWindowSize();
        var oFileMenuParam = new eUpdater("mgr/eXrmHomeMenuParamManager.ashx", 1);
        oFileMenuParam.asyncFlag = false;
        oFileMenuParam.addParam("tab", options.tab, "post");
        oFileMenuParam.addParam("gid", options.gid, "post");
        oFileMenuParam.addParam("fileid", options.id, "post");
        oFileMenuParam.addParam("part", options.part, "post");
        oFileMenuParam.addParam("height", winSize.h, "post");
        oFileMenuParam.addParam("width", winSize.dw, "post");
        if (nGlobalActiveTab) {
            oFileMenuParam.addParam("parenttab", nGlobalActiveTab, "post");
            oFileMenuParam.addParam("parentfid", top.GetCurrentFileId(nGlobalActiveTab), "post");
            if (options.fromBkm)
                oFileMenuParam.addParam("gridlocation", eGridLocation.BKM, "post");
        }
        oFileMenuParam.send(function (oRes) {
            loadMenuPartReturn(oRes, options.part, callback);
        });
    }

    // retourn serveur
    function loadMenuPartReturn(oRes, partIndex, callback) {

        var paramTab = document.getElementById("paramTab" + partIndex);
        if (paramTab == null)
            return;

        // ergonomie : on garde les index des sous-menus ouverts
        var indexes = getOpenedSubMenus(paramTab);

        // conteneur tmp
        var container = document.createElement("div");
        container.innerHTML = oRes;

        var newParamTab = container.querySelector("div[id='paramTab" + partIndex + "']");
        paramTab.innerHTML = newParamTab.innerHTML;


        // Après le innerHTML, on ouvre les même sous-menu le nouveau menu
        openSubMenus(paramTab, indexes);

        // evenment js
        initEvents(paramTab);

        // callback client
        if (callback && typeof callback === "function")
            callback(oRes);
    }

    // Retourne les index des sous-menu ouverts
    function getOpenedSubMenus(paramTab) {
        var submenus = new Array();
        var paramPartContents = paramTab.querySelectorAll(".paramPartContent");
        for (var i = 0; i < paramPartContents.length; i++) {
            if (getAttributeValue(paramPartContents[i], "data-active") == "1")
                submenus.push(getAttributeValue(paramPartContents[i], "index"));
        }

        return submenus;
    }

    // ouvre les sous menu avec des index dans la liste en param
    function openSubMenus(paramTab, indexes) {
        for (var i = 0; i < indexes.length; i++) {
            var paramPart = paramTab.querySelector("div[id='paramPart_" + indexes[i] + "']");
            if (paramPart) {
                var paramPartContent = paramPart.querySelector(".paramPartContent");
                if (getAttributeValue(paramPartContent, "data-active") == "0") {
                    var header = paramPart.querySelector(".paramPart header");
                    if (header)
                        oAdminGridMenu.showHidePart(header, paramPartContent);
                }
            }
        }
    }

    // valide l'ajout d'un widget et demande à la page de l'ajouter
    function validateWidgetList() {
        if (oModalWidgetList == null)
            return;

        var doc = oModalWidgetList.getIframe().document;

        if (doc) {
            var line = doc.querySelector("tr[select='1']");
            if (line) {


                var eid = getAttributeValue(line, "eid");
                var id = (eid.split('_')[1]) * 1;

                oGridController.widget.link({
                    id: id,
                    callback: function (result) {

                        cancelWidgetList();

                        //     var w = oXrmHomeGrid.add(id);
                        //    oAdminWidget.reload(w);
                    }
                })

            }
        }


    }

    // annule l'ajout d'un widget depuis la liste
    function cancelWidgetList() {
        if (oModalWidgetList != null)
            oModalWidgetList.hide();
    }

    // Initialise le menu de droite avec les evenements js
    function initEvents(target) {

        var headers = target.querySelectorAll(".paramPart header");
        for (var i = 0; i < headers.length; i++) {
            headers[i].addEventListener("click", function () {
                var elements = this.parentElement.getElementsByClassName("paramPartContent");
                if (elements)
                    oAdminGridMenu.showHidePart(this, elements[0]);
            });
        }

        oAdminGridMenu.resizeBlockContent("paramTab1");
        oAdminGridMenu.resizeBlockContent("paramTab2");

        // Ouverture des catalogues
        var catalogs = target.querySelectorAll("input[opencatdid]");
        for (var i = 0; i < catalogs.length; i++) {
            catalogs[i].addEventListener("click", function () {

                var fctValidate = function (catalogDialog, srcId, trgId, tabSelectedLabels, tabSelectedValues, selectedIDs) {
                    var input = document.getElementById(trgId);
                    if (input) {
                        input.setAttribute("paramvalue", selectedIDs);
                        // Mise à jour de l'affichage même si on rafraîchit juste après, pour l'impression de fluidité
                        var displayValue = tabSelectedLabels.join(';');
                        input.value = displayValue;

                        updateWidgetParam(input, null, true);
                    }
                };
                var dialog = showCatGeneric(true, false, getAttributeValue(this, "paramvalue"), null, this.id, getAttributeValue(this, "opencatdid"), "3", null, null, null, encode(top._res_8439), "advCatalog", false, fctValidate, function () { }, "0", "0", "1");
            });

        }
        // Ouverture des catalogues au clic de l'icône
        var btnCatalogs = target.querySelectorAll("[opencatdid][ebtnparam='1']");
        for (var i = 0; i < btnCatalogs.length; i++) {
            btnCatalogs[i].addEventListener("click", function () {
                var id = getAttributeValue(this, "ename");
                var input = document.getElementById(id);
                if (input)
                    input.click();
            });

        }

        var btnSelectFile = target.querySelectorAll("span.btnSelectFile");
        for (var i = 0; i < btnSelectFile.length; i++) {
            btnSelectFile[i].addEventListener("click", function () {

                var input = this.parentElement.querySelector("input.inputSelectFile");

                openLnkFileDialog(FinderSearchType.SelectFile, getAttributeValue(this, "data-tab"), false, CallFromTileWidget, true, function (iframeId) {

                    var nFileId = 0;
                    var sFileLabel = "";

                    var oFrmWin = eTools.GetModal("modalFinder").getIframe();

                    var oFrmDoc = oFrmWin.document;

                    var selectedListValues = oFrmWin._selectedListValues;
                    if (typeof (selectedListValues) === 'undefined')
                        selectedListValues = new Array();

                    var selectedValue;
                    if (selectedListValues.length > 0) {
                        selectedValue = selectedListValues[0];
                        var oItem = oFrmDoc.getElementById(selectedValue);
                        if (!oItem)
                            return;

                        // id  
                        var oId = oItem.getAttribute("eid").split('_');
                        var nTab = oId[0];
                        nFileId = oId[oId.length - 1];

                        var tabTd = oItem.getElementsByTagName("td");
                        sFileLabel = GetText(tabTd[0]); //Libellé de la première colonne
                    }
                    /*DISPLAY VALUE*/
                    input.value = sFileLabel;
                    input.setAttribute("paramvalue", nFileId);
                    input.onchange();
                    /*CLOSE CAT*/
                    var modalFinder = eTools.GetModal("modalFinder");
                    if (modalFinder)
                        modalFinder.hide();

                });
            });
        }
        var inputSelectFile = target.querySelectorAll("input.inputSelectFile");
        for (var i = 0; i < inputSelectFile.length; i++) {
            inputSelectFile[i].addEventListener("click", function () {
                var btn = this.parentElement.querySelector(".btnSelectFile");
                if (btn) {
                    btn.click();
                }
            });
        }

    }

    // ajout d'event js aux rubriques
    function initParamTabEvents(options, index) {
        var widgetParamTab = document.getElementById("paramTab" + index);
        if (widgetParamTab) {
            // On recupère les element html avec leur id, ex id^=115100_115
            Array.prototype.slice.call(widgetParamTab.querySelectorAll("input[id^='fld_" + options.tab + "_']"))
                .forEach(function (target) {

                    //Dans le cas des images on appel manuellement l upload
                    if (target.tagName != "INPUT" || target.getAttribute("class") == "widgetImg")
                        return;

                    setEventListener(target, "change", function (mouseEvent) {

                        var src = mouseEvent.srcElement || mouseEvent.target;

                        var paramName = getAttributeValue(this, "paramname");

                        if (paramName && paramName != "specifUrl") {
                            if (this.value != getAttributeValue(this, "paramvalue"))
                                oAdminGridMenu.updateParam(this);
                        }
                        else {
                            oAdminGridMenu.updateField({
                                'id': options.id,
                                'tab': options.tab,
                                'src': src,
                                'callback': function () {
                                    if (getAttributeValue(this, "data-reloadgrids") == "1") {
                                        // TODO
                                    }
                                }
                            });
                        }


                    });
                });
        }
    }

    function manageDragInfos(evt) {

        // pas d'element source
        if (evt.src == null) return;

        switch (evt.name) {
            case "start":
                break;
            case "move":
                break;
            case "commit":
                break;
            case "after-commit":
                break;
        }
    }

    function updatePicto(src, pictoData, handler) {

        if (src.id != "btnSelectPicto")
            return;

        var list = new Array();
        var iconHeader = document.getElementById(getAttributeValue(src, "icon-header"));
        if (iconHeader) {
            var parts = iconHeader.id.split("_");
            if (parts.length != 4)
                return;

            var paramIcon = new Object();
            paramIcon.src = iconHeader;
            paramIcon.dbv = getAttributeValue(iconHeader, "dbv");
            paramIcon.format = getAttributeValue(iconHeader, "fmt") * 1;
            paramIcon.value = pictoData.key;
            paramIcon.descId = parts[2] * 1;
            paramIcon.tab = parts[1] * 1;
            paramIcon.fileId = parts[3] * 1;

            list.push(paramIcon);
        }

        var colorHeader = document.getElementById(getAttributeValue(src, "color-header"));
        if (colorHeader) {
            var parts = colorHeader.id.split("_");
            if (parts.length != 4)
                return;

            var paramColor = new Object();
            paramColor.src = colorHeader;
            paramColor.dbv = getAttributeValue(colorHeader, "dbv");
            paramColor.format = getAttributeValue(colorHeader, "fmt") * 1;
            paramColor.value = pictoData.color;
            paramColor.descId = parts[2] * 1;
            paramColor.tab = parts[1] * 1;
            paramColor.fileId = parts[3] * 1;

            list.push(paramColor);
        }

        if (list.length > 0) {

            var tab = list[0].tab;
            var id = list[0].fileId;

            oGridController.config.updateList(id, list,
                function (result) {

                    setAttributeValue(iconHeader, "dbv", pictoData.key);
                    setAttributeValue(colorHeader, "dbv", pictoData.color);


                    if (tab == 115100) {
                        var widget = document.getElementById("widget-wrapper-" + id);
                        if (widget)
                            nsAdminGrid.reloadWidget(widget);
                    }

                    if (handler)
                        handler.hide();
                });
        }
    }

    function updateImage(element, dbv, width, height) {
        var parts = element.id.split("_");
        if (parts.length != 4)
            return;

        var tab = parts[1] * 1;
        var fileId = parts[3] * 1;
        var descId = parts[2] * 1;
        var paramList = new Array();

        //Param contentSource
        var param = new Object();
        param.src = element;
        param.dbv = dbv;
        param.format = getAttributeValue(element, "fmt") * 1;
        param.value = dbv;
        param.descId = descId;
        param.tab = tab;
        param.fileId = fileId;
        paramList.push(param);


        //Methode callback
        var callback = function (result) {
            setAttributeValue(element, "dbv", dbv);
            if (param.tab == 115100) {
                var widget = document.getElementById("widget-wrapper-" + param.fileId);
                if (widget)
                    nsAdminGrid.reloadWidget(widget);
            };
        }

        oGridController.config.updateList(fileId, paramList, callback);
    }

    function showGraphics(tab, id, callback) {
        _modal.showReportOrFilterList({
            'tab': tab,
            'filterOrReportTab': 105000,
            'id': id,
            'title': top._res_7696.replace("<REPORTTYPE>", top._res_1005),
            'reportType': 10,
            'listType': 13,
            'validate': callback,
            'handlerId': 'ReportList'
        });
    }

    function showSelectCols(nTab, listCol, id, callback) {


        modalAdvSearch = new eModalDialog(top._res_7316, 0, "eFieldsSelect.aspx", 985, 550);
        modalAdvSearch.ErrorCallBack = function () { setWait(false); }
        modalAdvSearch.addParam("tab", nTab, "post");
        modalAdvSearch.addParam("listCol", listCol, "post");
        modalAdvSearch.addParam("forcelst", "0", "post");
        modalAdvSearch.addParam("bFromGrid", "1", "post");

        modalAdvSearch.bBtnAdvanced = true;

        modalAdvSearch.show();
        modalAdvSearch.addButton(top._res_30, function () { modalAdvSearch.hide(); }, "button-gray", nTab);
        modalAdvSearch.addButton(top._res_28, callback, "button-green", nTab);
    }

    function updateListParam(element, bRefreshPart) {

        updateWidgetParam(element, null, bRefreshPart);

    }

    function updateSpecifParam(widgetID) {



        var specifID = document.getElementById("hidSpecifID");
        var specifURL = document.getElementById("specifURL");
        var specifURLParam = document.getElementById("specifURLParam");
        var specifLabel = document.getElementById("fld_115100_115101_" + widgetID);

        // #65655 : On autorise les querystrings
        //if (specifURL.value.indexOf("?") != -1) {
        //    eAlert(0, "", top._res_8054, specifURL.value);
        //    return false;
        //}


        var caps = new Capsule(0);
        caps.SpecifId = specifID.value;
        caps.AddProperty("15", "1", "4", null); // Specif type : Eudopart
        caps.AddProperty("15", "2", "3", null); // OpenMode : iframe
        caps.AddProperty("15", "5", specifLabel.value, null);
        caps.AddProperty("15", "6", specifURL.value, null);
        caps.AddProperty("15", "7", specifURLParam.value, null);


        var sources = document.querySelectorAll("input[name='rbSpecifSource'][dsc='15|3']:checked")
        if (sources.length > 0) {
            var mySource = getAttributeValue(sources[0], 'value');
            caps.AddProperty("15", "3", mySource, null);

        }
        else
            caps.AddProperty("15", "3", 2, null);

        var statics = document.querySelectorAll("input[name='rbSpecifStatic'][dsc='15|11']:checked")
        if (statics.length > 0) {
            var mySource = getAttributeValue(statics[0], 'value');
            caps.AddProperty("15", "11", mySource == "1" ? "1" : "0", null);
        }
        else
            caps.AddProperty("15", "11", "0", null);



        var json = JSON.stringify(caps);
        var upd;

        upd = new eUpdater("eda/Mgr/eAdminSpecifManager.ashx", 1);
        upd.json = json;
        upd.send(function (oRes) {
            var oResult = JSON.parse(oRes);
            if (oResult.Success) {
                var idSpecif = oResult.SpecifId;
                var contentSource = document.getElementById("fld_115100_115113_" + widgetID);
                var oSpecId = document.getElementById("hidSpecifID")
                if (oSpecId)
                    oSpecId.value = idSpecif;

                contentSource.value = idSpecif;
                updateValueAndReload(contentSource, contentSource.value);

            }
            else {
                top.eAlert(0, top._res_92, oResult.ErrorMessage, "");
            }
        });
    }

    function updateTileSpecifParam(element, event) {
        var elHidSpecifID = null;
        var elTileSpecifIdContainer = document.getElementById("tileSpecifIdContainer");
        if (elTileSpecifIdContainer != null)
            elHidSpecifID = elTileSpecifIdContainer.querySelector("input[paramname='specifid']");

        if (elHidSpecifID == null) {
            top.eAlert(0, top._res_92, top._res_92, "");
            return false;
        }

        var paramName = "";

        if (element.hasAttribute("paramname")) {
            paramName = element.getAttribute("paramname");
        }

        var specifParamId = ""
        switch (paramName) {
            case "specifUrl":
                specifParamId = "6";
                break;
            case "specifURLParam":
                specifParamId = "7";
                break;
            case "specifURLAdmin":
                specifParamId = "10";
                break;
            case "specifOpenMode":
                specifParamId = "2";
                break;
            default:
                top.eAlert(0, top._res_92, oResult.ErrorMessage, "");
                return false;
                break;
        }

        var paramValue = element.value;

        // #65655 : On autorise les querystrings
        //if (paramName == "specifUrl" && paramValue.indexOf("?") != -1) {
        //    eAlert(0, "", top._res_8054, paramValue);
        //    return false;
        //}

        var caps = new Capsule(0);
        caps.SpecifId = elHidSpecifID.value;
        caps.AddProperty("15", specifParamId, paramValue, null);

        //si la specif n'existe pas, on ajoute un OpenMode : Modal et un type : widget tuile
        if (elHidSpecifID.value == "0") {
            caps.AddProperty("15", "1", "15", null); // Specif type : widget tuile
            if (specifParamId != "specifOpenMode")
                caps.AddProperty("15", "2", "1", null); // OpenMode : Modal
        }

        var json = JSON.stringify(caps);
        var upd;

        upd = new eUpdater("eda/Mgr/eAdminSpecifManager.ashx", 1);
        upd.json = json;
        upd.send(updateTileSpecifParamReturnProcess, elHidSpecifID, element);
    }

    function updateTileSpecifParamReturnProcess(oRes, elTarget, elSource) {
        var oResult = JSON.parse(oRes);
        if (oResult.Success) {
            if (elTarget != null && elTarget.value != oResult.SpecifId) {
                elTarget.value = oResult.SpecifId;

                oAdminGridMenu.updateParam(elTarget);

            }
            addClass(elSource, "updatedField");

            setTimeout(function () {
                removeClass(elSource, "updatedField");
            }, 1000);
        }
        else {
            top.eAlert(0, top._res_92, oResult.ErrorMessage, "");
        }
    }

    function showFilters() { }


    function updateValueAndReload(element, newVal) {

        var dbv = getAttributeValue(element, "dbv");
        var parts = element.id.split("_");
        if (parts.length != 4 && parts.length != 5)
            return;

        //pas de modif on ignore le clique
        if (dbv == newVal)
            return;

        //Param principal
        var param = new Object();
        param.src = element;
        param.dbv = dbv;
        //param.format = format;
        param.value = newVal;
        param.fileId = parts[3] * 1;
        param.descId = parts[2] * 1;
        param.tab = parts[1] * 1;
        param.callback = function (result) {
            if (element)
                setAttributeValue(element, "dbv", param.value);
            if (param.tab == 115100) {
                var widget = document.getElementById("widget-wrapper-" + param.fileId);
                if (widget)
                    nsAdminGrid.reloadWidget(widget);
            };
        }
        oGridController.config.update(param);
    }

    function updateWidgetParam(element, value, bRefreshPart) {
        //var parts = element.id.split("_");
        //if (parts.length != 4 && parts.length != 5)
        //    return;
        updateWidgetParamValue(element, getAttributeValue(element, "wid"), getAttributeValue(element, "paramname"), value, bRefreshPart);
    }

    function updateWidgetParamValue(element, nWid, paramname, paramvalue, bRefreshPart) {

        if (getAttributeValue(element, "data-hasrcode") == "1") {

            var code = getAttributeValue(element, "data-rcode");

            if (code == "") {
                // Création de nouveau RESCODE
                nsAdminResCode.createNewResCode(paramvalue, getAttributeValue(element, "data-rloc"), function (oRes) {
                    top.setWait(false);
                    var oResult = JSON.parse(oRes);
                    if (oResult.Success) {
                        setAttributeValue(element, "data-rcode", oResult.ResCode);
                        paramvalue = oResult.ResAlias;
                        setAttributeValue(element, "paramvalue", paramvalue);

                        launchParamUpdate(element, nWid, paramname, paramvalue, bRefreshPart);
                    }
                });
            }
            else {
                // Mise à jour RESCODE
                nsAdminResCode.updateResCode(code, paramvalue, function (oRes) {
                    top.setWait(false);
                    var oResult = JSON.parse(oRes);
                    if (oResult.Success) {
                        paramvalue = oResult.ResAlias;
                        launchParamUpdate(element, nWid, paramname, paramvalue, bRefreshPart);
                    }
                });
            }
        }
        else {
            launchParamUpdate(element, nWid, paramname, paramvalue, bRefreshPart);
        }

    }

    function launchParamUpdate(element, nWid, paramname, paramvalue, bRefreshPart) {

        if (typeof bRefreshPart === "undefined")
            bRefreshPart = false;

        var options = new Object();
        options.tab = 115100;
        options.id = nWid;
        options.src = element;
        options.paramname = paramname;
        options.type = getAttributeValue(document.getElementById("specificParamsContainer"), "widgettype");

        if (typeof paramvalue !== "undefined" && paramvalue != null)
            options.paramvalue = paramvalue;
        else
            options.paramvalue = getAttributeValue(element, "paramvalue");

        var fromBkm = false;
        var tabGrid = findUpByClass(document.getElementById("widget-wrapper-" + nWid), "gw-tab");
        if (tabGrid) {
            var ptab = getAttributeValue(tabGrid, "parent-tab") * 1;
            fromBkm = ptab > 0 && ptab != 115200;
        }
        options.fromBkm = fromBkm;

        if (bRefreshPart) {
            options.callback = function () {

                options.part = MENU_PART.LOAD_WIDGET_CONFIG;
                loadMenuPart(options);
            }
        }

        oGridController.config.updateParams(options);
    }

    // Suppression du filtre
    function deleteFilter(nFilterId, nTab, fctCallback) {
        var url = "mgr/eFilterWizardManager.ashx";
        var upd = new eUpdater(url, 0);
        upd.addParam("action", "delete", "post");
        upd.addParam("maintab", nTab, "post");
        upd.addParam("filterid", nFilterId, "post");
        upd.ErrorCallBack = function () { };
        upd.send(fctCallback);
    }

    function sendWidgetImage(element, file) {
        if (!element || !file)
            return;

        //On upload l image
        sendImage(element, file, function () {
            //On met a jour la valeur en base et dans le dom
            var parts = element.id.split("_");
            var img = top.document.querySelector("#widget-wrapper-" + parts[3] + " .xrm-widget-content img");

            if (img) {
                updateImage(element, getAttributeValue(img, "dbv"), img.width, img.height);

                var hidWidth = document.querySelector("[paramname='width']");
                if (hidWidth) {
                    setAttributeValue(hidWidth, "paramvalue", img.width);
                    updateWidgetParam(hidWidth);
                }

                var hidHeight = document.querySelector("[paramname='height']");
                if (hidHeight) {
                    setAttributeValue(hidHeight, "paramvalue", img.height);
                    updateWidgetParam(hidHeight);
                }

            }
        });

    }

    var _modal = {
        'handler': null,
        'showReportOrFilterList': function (param) {

            if (typeof (param.cancel) != 'function')
                param.cancel = _modal.close;

            if (typeof (param.validate) != 'function')
                param.validate = _modal.close

            // scale : mise à l'echelle de 0.80 % de la taille totale de l'ecran (top)
            // require : taille minimale pour la fenetre, pour que le rendu soit optimisé
            // tablet : taille minimale pour les tablettes
            var size = top.getWindowSize().scale(0.85).min({ w: 1024, h: 600 }).tablet({ w: 750 });

            _modal.handler = new eModalDialog(param.title, 0, 'eFilterReportListDialog.aspx', size.w, size.h, param.handlerId);
            _modal.handler.ErrorCallBack = launchInContext(_modal.handler, _modal.handler.hide);
            _modal.handler.addParam("type", param.reportType, "post");
            _modal.handler.addParam("width", size.w, "post");
            _modal.handler.addParam("height", size.h, "post");
            _modal.handler.addParam("tab", param.tab, "post");
            _modal.handler.addParam("frmId", _modal.handler.iframeId, "post");
            _modal.handler.addParam("lstType", param.listType, "post");
            _modal.handler.addParam("fid", param.id, "post");

            if (param.id > 0)
                _modal.handler.onIframeLoadComplete = function () {
                    _modal.handler.getIframe().nsFilterReportList.SelectFilterReport(param.filterOrReportTab, param.id);
                };

            _modal.handler.show();
            _modal.handler.addButtonFct(top._res_29, function () { _modal.close(param.cancel); }, 'button-gray');
            _modal.handler.addButtonFct(top._res_28, function () { _modal.validate(param.validate); }, 'button-green');
        },
        'close': function (cancelCallback) {

            if (typeof (cancelCallback) == 'function')
                cancelCallback();

            if (_modal.handler != null)
                _modal.handler.hide();

        },
        'validate': function (validateCallback) {
            if (typeof (validateCallback) == 'function')
                validateCallback({
                    'modal': _modal.handler,
                    'selectedId': _modal.handler.getIframe().nsFilterReportList.GetSelectedReport()
                });

            _modal.close();
        }
    }
    return {
        // charge le menu de droite avec l'id : optionel  
        'loadMenu': function (tab, id, context, callback) {
            loadMenu(tab, id, context, callback);
        },
        'updateField': function (options) {
            var src = options.src;
            var parts = src.id.split("_");
            if (!src || parts.length != 4)
                return;

            var param = new Object();
            param.src = src;
            param.dbv = getAttributeValue(src, "dbv");
            param.format = getAttributeValue(src, "fmt") * 1;
            param.descId = parts[2] * 1;
            param.tab = options.tab * 1;
            param.fileId = options.id * 1;
            param.attribute = getAttributeValue(src, "attr");

            if (param.format == "3")
                param.value = getAttributeValue(src, "paramvalue");
            else
                param.value = src.value;

            //???????
            //if (src.previousSibling && src.previousSibling.tagName.toLowerCase() == "p")
            //    param.lib = GetText(src.previousSibling);

            if (param.dbv == param.value)
                return;

            param.callback = function (engineResult) {

                setAttributeValue(src, "dbv", param.value);

                //TODO c'est moche
                if (param.tab == 115100) {
                    var widget = document.getElementById("widget-wrapper-" + param.fileId);
                    if (widget) {
                        nsAdminGrid.reloadWidget(widget);
                    }

                } else {
                    var field = document.getElementById("grid_" + param.tab + "_" + param.descId + "_" + param.fileId);
                    if (field)
                        SetText(field, param.value);
                }

                options.callback(engineResult);
            };

            oGridController.config.update(param);


        },
        // affichage de la liste des widget en mode fentere standard 
        'showWidgetList': function () {

            oModalWidgetList = new eModalDialog(top._res_8145 /*"Liste des widgets prédéfinis"*/, 0, 'eXrmWidgetList.aspx', 900, 500, "eXrmWidgetList");
            oModalWidgetList.ErrorCallBack = launchInContext(oModalWidgetList, oModalWidgetList.hide);
            oModalWidgetList.addParam("rows", 1, "post");
            oModalWidgetList.addParam("page", 1, "post");
            oModalWidgetList.addParam("tab", "115100", "post");
            oModalWidgetList.addParam("fileid", oGridController.grid.getId(), "post");
            oModalWidgetList.addParam("frmId", oModalWidgetList.iframeId, "post");

            oModalWidgetList.show();

            oModalWidgetList.addButtonFct(top._res_28, validateWidgetList, 'button-green', 'ok');
            oModalWidgetList.addButtonFct(top._res_29, cancelWidgetList, 'button-gray');
        },
        'selectFromList': function (table, evt) {

            Array.prototype.slice.call(table.querySelectorAll("tr[select='1']"))
                .forEach(function (cell) { cell.removeAttribute("select"); });

            if (!evt)
                evt = window.event;

            var src = evt.srcElement || evt.target;


            if (src.tagName == "TD" && src.parentElement != null)
                src = src.parentElement;

            if (src.tagName == "TR")
                setAttributeValue(src, "select", "1");

        },
        // Affiche/masque une partie du menu
        'showHidePart': function (header, element) {
            var newIcon = header.querySelector("div [class^='icon']");
            if (element.getAttribute("data-active") == "1") {
                element.setAttribute("data-active", "0");
                element.setAttribute("eactive", "0");
                newIcon.className = "icon-caret-right";
            }
            else {
                element.setAttribute("data-active", "1");
                element.setAttribute("eactive", "1");
                newIcon.className = "icon-caret-down";
            }
        },
        // Affichage des blocs suivant l'icône
        'showBlock': function (id) {

            var blocks = document.getElementsByClassName("paramBlock");
            for (var i = 0; i < blocks.length; i++) {
                blocks[i].style.display = "none";
            }

            var destPosition = "5px";
            var position = id[id.length - 1];
            var startPosition = 27;
            var nSize = eTools.GetFontSize();

            if (nSize >= 14) {

                switch (position) {

                    case "1": destPosition = "44px"; break;
                    case "2": destPosition = "160px"; break;
                    case "3": destPosition = "277px"; break;
                }
            }
            else {
                switch (position) {

                    case "1": destPosition = "27px"; break;
                    case "2": destPosition = "110px"; break;
                    case "3": destPosition = "193px"; break;
                }
            }

            oAdminGridMenu.animateLeft(document.getElementById("slidingArrow"), destPosition);

            var block = document.getElementById(id);
            block.style.display = "block";

            oAdminGridMenu.resizeBlockContent(id);
        },
        // Redimensionnement du bloc par rapport à la résolution
        'resizeBlockContent': function (blockId) {
            var blockHeight = window.innerHeight - 180; // on enlève la partie user + navigation + titre + bouton déconnexion

            var blockContent = document.querySelector("#" + blockId + " .paramBlockContent");
            if (blockContent) {
                blockContent.style.display = "block";
                blockContent.style.height = blockHeight + "px";
            }
        },
        // Animation gauche (pour la flèche)
        'animateLeft': function (obj, leftPosition) {
            obj.style.transition = "0.5s";
            obj.style.left = leftPosition;
        },
        // affiche le paramètrage du widget dans le menu de droite
        'part': {
            'loadContent': function (options) { },
            'loadConfigWidget': function (options) {

                top.setWait(true);

                if (typeof (options.id) == 'undefined')
                    return;

                options.tab = 115100;
                options.part = MENU_PART.LOAD_WIDGET_CONFIG;
                loadMenuPart(options, function (oRes) {
                    initParamTabEvents(options, 2);
                    oAdminGridMenu.showBlock("paramTab2");
                    if (typeof (options.callback) == 'function')
                        options.callback(oRes, options);
                    top.setWait(false);
                });
            },
            'loadConfigPage': function (options) { },
        },
        'setBoolValue': function (element, evt) {

            if (!evt) evt = window.event;
            var src = evt.srcElement || evt.target;
            if (src.tagName.toLowerCase() == "label")
                src = document.getElementById(getAttributeValue(src, "for"));

            // on ignore le clique
            if (!src || src.tagName.toLowerCase() != "input")
                return;

            if (element.hasAttribute("paramname")) {
                if (src.getAttribute("type").toLowerCase() == "radio" && src.hasAttribute("value")) //cas d'un radio button
                    updateWidgetParam(element, src.getAttribute("value"));
                else //Autres cas
                    updateWidgetParam(element);
            }
            else {
                var ids = element.getAttribute("id").split("_");

                if (src.getAttribute("type").toLowerCase() == "radio" && src.hasAttribute("value")) //cas d'un radio button
                    element.setAttribute("paramvalue", src.getAttribute("value"))

                oAdminGridMenu.updateField({
                    'id': ids[3] * 1,
                    'tab': ids[1] * 1,
                    'src': element,
                    'callback': function () { }
                });
            }
        },
        'selectChanged': function (element, bDeleteFilter) {

            if (element == null || (element && element.value == "-1"))
                return;

            if (typeof bDeleteFilter === "undefined")
                bDeleteFilter = false;

            //Dans le cas du widget list on vide le paramValue du bouton Rubrique affiches et filtre associe au changement d'onglet
            var parts = element.id.split("_");
            var fileId = parts[3] * 1;
            var btnRubrique = document.querySelector('[paramname="listcol"]');
            var btnFiltre = document.querySelector('[paramname="filterid"]');
            if (btnRubrique) {
                setAttributeValue(btnRubrique, "paramValue", ""); //btn rubrique
                updateListParam(btnRubrique);
            }
            if (btnFiltre) {
                var oldFilterId = getAttributeValue(btnFiltre, "paramValue");

                setAttributeValue(btnFiltre, "paramValue", ""); //btn filtre
                updateListParam(btnFiltre);

                if (bDeleteFilter) {
                    deleteFilter(oldFilterId, element.value, function () {
                        btnFiltre.innerText = btnFiltre.innerText.replace("(1)", "");
                    });
                }
            }

            updateValueAndReload(element, element.value);
        },
        'uploadFile': function (element) {
        },
        'updateParam': function (element, bRefresh) {
            var value = element.value;
            if (hasClass(element, "chk"))
                value = getAttributeValue(element, "chk");

            setAttributeValue(element, "paramvalue", value);
            updateListParam(element, bRefresh);
        },
        'updateParamValue': function (element, nWidgetId, paramValue, bRefresh) {

            var paramName = getAttributeValue(element, "paramname");

            if (!paramValue) {
                var paramValue = element.value;
                if (hasClass(element, "chk"))
                    paramValue = getAttributeValue(element, "chk");

                setAttributeValue(element, "paramvalue", paramValue);
            }

            updateWidgetParamValue(element, nWidgetId, paramName, paramValue, bRefresh);
        },
        'updateListParam': function (element, bRefresh) {
            updateListParam(element, bRefresh);
        },
        'updateSpecifParam': function (widgetID) {
            updateSpecifParam(widgetID);
        },
        'openPicto': function (element) {

            var target = element.querySelector("span[id='selectedPicto'");
            if (!target)
                return;

            var oPicto = new ePictogramme(target.id, {
                tab: 0, // pas de picto
                title: "Administrer le pictogramme",
                color: getAttributeValue(target, "picto-color"),
                iconKey: getAttributeValue(target, "picto-key"),
                width: 750,
                height: 675,
                callback: function (pictoData) { updatePicto(element, pictoData, oPicto); }
            });

            oPicto.show();
        },
        'onChangeIndicator': function (element, evt) {
            if (!evt) evt = window.event;
            var src = evt.srcElement || evt.target;
            if (src.tagName.toLowerCase() == "label")
                src = document.getElementById(getAttributeValue(src, "for"));

            // on ignore le clique
            if (!src || src.tagName.toLowerCase() != "input")
                return;

            updateValueAndReload(element, src.value);

            var ddlOngletNumLbl = document.querySelector('[paramname="tabNumId"]').previousElementSibling;
            var ddlOperatorNumLbl = document.querySelector('[paramname="operatorNum"]').previousElementSibling;
            var ddlFieldNumLbl = document.querySelector('[paramname="fieldNumId"]').previousElementSibling;
            var btnFilterNumLbl = document.querySelector('[paramname="filterNumId"]');
            var btnFilterNumLbl = document.querySelector('[paramname="filterNumId"]');
            var fieldPercent = document.getElementById("wrapperUnitInPercent");

            //Ratio
            if (src.value == "1") {
                document.getElementById("containerRatio").style = "display:inline;";
                ddlOngletNumLbl.innerHTML = top._res_8031;
                ddlOperatorNumLbl.innerHTML = top._res_8032;
                ddlFieldNumLbl.innerHTML = top._res_8033;
                btnFilterNumLbl.innerHTML = top._res_8034;

                fieldPercent.style.display = "block";
            } else {
                document.getElementById("containerRatio").style = "display:none;";
                ddlOngletNumLbl.innerHTML = top._res_264;
                ddlOperatorNumLbl.innerHTML = top._res_8040;
                ddlFieldNumLbl.innerHTML = top._res_222;
                btnFilterNumLbl.innerHTML = top._res_8016;

                fieldPercent.style.display = "none";
            }

            this.updateListParam(element);
        },
        'onTabIndicatorChange': function (element) {
            setAttributeValue(element, "paramValue", element.value);
            var paramName = getAttributeValue(element, "paramname");
            var tabId = getAttributeValue(element, "paramvalue");
            var ddlList;
            var ddl;
            var selectedDDL;
            var operator;
            if (paramName == "tabNumId") {
                ddlList = document.querySelectorAll("*[id^=ddlFieldsNum]");
                selectedDDL = document.getElementById("ddlFieldsNum_" + tabId);
                operator = document.querySelector('[paramname="operatorNum"]');
            } else if (paramName == "tabDenId") {
                ddlList = document.querySelectorAll("*[id^=ddlFieldsDen]");
                selectedDDL = document.getElementById("ddlFieldsDen_" + tabId);
                operator = document.querySelector('[paramname="operatorDen"]');
            }
            for (var i = 0; i < ddlList.length; i++) {
                ddl = ddlList[i];
                ddl.style.display = "none";
                ddl.children[1].style.display = "none";
            }

            // Si l'opérateur n'est pas "Nombre de fiches", on affiche la rubrique 
            if (getAttributeValue(operator, "paramvalue") != "NB") {
                selectedDDL.style.display = "inline";
                selectedDDL.children[1].style.display = "inline";
            }

            this.updateListParam(element);


        },
        'onOpIndicatorChange': function (element) {
            setAttributeValue(element, "paramValue", element.value);
            var isNum = getAttributeValue(element, "paramname").indexOf("Num") >= 0;
            var paramName = "tabNumId";
            var ddlName = "ddlFieldsNum_";
            if (!isNum) {
                paramName = "tabDenId";
                ddlName = "ddlFieldsDen_";
            }
            var ddlTab = document.querySelector('[paramname="' + paramName + '"]');
            var tabId = getAttributeValue(ddlTab, "paramvalue");
            selectedDDL = document.getElementById(ddlName + tabId);

            if (element.value == "NB") {
                selectedDDL.style.display = "none";
                selectedDDL.children[1].style.display = "none";
            } else {
                selectedDDL.style.display = "inline";
                selectedDDL.children[1].style.display = "inline";
            }

            this.updateListParam(element);
        },
        'onPercentOptionChange': function (element, evt) {

            if (!evt) evt = window.event;
            var src = evt.srcElement || evt.target;
            if (src.tagName.toLowerCase() == "label")
                src = document.getElementById(getAttributeValue(src, "for"));

            // on ignore le clique
            if (!src || src.tagName.toLowerCase() != "input")
                return;

            setAttributeValue(element, "paramvalue", src.value);

            //var unit = document.querySelector('[paramname="unit"]');
            //if (unit) {
            //    unit.readOnly = (src.value == "1");
            //    unit.value = (src.value == "1") ? "%" : "";
            //}

            this.updateListParam(element);
        },
        'onVisuTypeChange': function (element, evt) {

            if (!evt) evt = window.event;
            var src = evt.srcElement || evt.target;
            if (src.tagName.toLowerCase() == "label")
                src = document.getElementById(getAttributeValue(src, "for"));

            // on ignore le clique
            if (!src || src.tagName.toLowerCase() != "input")
                return;

            setAttributeValue(element, "paramvalue", src.value);

            this.updateListParam(element);

            var elPictureArea = document.getElementById("tilePictureParamArea");
            if (elPictureArea != null) {
                if (src.value == "1") //Image
                    elPictureArea.style.display = "block";
                else  //Pictogramme
                    elPictureArea.style.display = "none";
            }
        },
        'onTypeActionChange': function (element) {
            setAttributeValue(element, "paramValue", element.value);
            this.updateListParam(element, true);

            var elTileUrlContainer = document.getElementById("tileUrlContainer");
            var elTileSpecifUrlContainer = document.getElementById("tileSpecifUrlContainer");
            var elTileSpecifUrlParamContainer = document.getElementById("tileSpecifUrlParamContainer");
            var elTileSpecifUrlAdmContainer = document.getElementById("tileSpecifUrlAdmContainer");
            var elTileSpecifOpenModeContainer = document.getElementById("tileSpecifOpenModeContainer");
            var elTileTabContainer = document.getElementById("tileTabContainer");
            var elTileFilterIdContainer = document.getElementById("tileFilterIdContainer");

            var elementsArray = [
                elTileUrlContainer,
                elTileSpecifUrlContainer,
                elTileSpecifUrlParamContainer,
                elTileSpecifUrlAdmContainer,
                elTileSpecifOpenModeContainer,
                elTileTabContainer,
                elTileFilterIdContainer
            ];

            for (var i = 0; i < elementsArray.length; ++i) {
                if (elementsArray[i] != null)
                    elementsArray[i].style.display = "none";
            }

            if (element.value == XrmWidgetTileAction.OpenWebpage) {
                if (elTileUrlContainer != null) {
                    elTileUrlContainer.style.display = "block";
                }
            }
            else if (element.value == XrmWidgetTileAction.OpenSpecif) {
                if (elTileSpecifUrlContainer != null)
                    elTileSpecifUrlContainer.style.display = "block";
                if (elTileSpecifUrlParamContainer != null)
                    elTileSpecifUrlParamContainer.style.display = "block";
                if (elTileSpecifUrlAdmContainer != null)
                    elTileSpecifUrlAdmContainer.style.display = "block";
                if (elTileSpecifOpenModeContainer != null)
                    elTileSpecifOpenModeContainer.style.display = "block";
            }
            else if (element.value == XrmWidgetTileAction.OpenTab) {
                if (elTileTabContainer != null)
                    elTileTabContainer.style.display = "block";
                if (elTileFilterIdContainer != null)
                    elTileFilterIdContainer.style.display = "block";
            }
        },
        'onOpenModeChange': function (element) {
            setAttributeValue(element, "paramValue", element.value);
            this.updateListParam(element);
        },
        'updateTileSpecifParam': function (element, event) {
            updateTileSpecifParam(element, event);
        },
        'saveImage': function (element) {

            var file;
            if (typeof (element.files) != "undefined" && element.files.length == 1) {
                file = element.files[0];
            }
            sendWidgetImage(element, file);
        },
        'dropImage': function (event) {
            event.preventDefault();

            UpFilDragLeave(document.querySelector(".dashed"));

            var file;
            if (event.dataTransfer.items) {
                var item = event.dataTransfer.items[0];
                if (item.kind === "file")
                    file = item.getAsFile();
            }

            if (!file) {
                if (event.dataTransfer.files.length > 0)
                    file = event.dataTransfer.files[0];
            }

            // Cleanup
            if (event.dataTransfer.items) {
                // Use DataTransferItemList interface to remove the drag data
                event.dataTransfer.items.clear();
            } else {
                // Use DataTransfer interface to remove the drag data
                event.dataTransfer.clearData();
            }

            var element = document.querySelector("input.widgetImg");
            sendWidgetImage(element, file);
        },
        'showGraphics': function (element) {
            var dbv = getAttributeValue(element, "dbv");
            if (dbv == "")
                dbv = 0;
            //On recupere la DDL onglet
            var ddlTab = document.getElementById("fld_115100_115114_" + element.id.split("_")[3] + "_tab");
            if (ddlTab) {
                selectedTab = ddlTab.value;
            } else {
                selectedTab = oGridController.widget.tab;
            }
            if (selectedTab && selectedTab != "") {
                showGraphics(selectedTab, dbv, function (args) {
                    updateValueAndReload(element, (args.selectedId + ""));
                });
            }
            else {
                eAlert(2, "", top._res_8660); //Veuillez préalablement sélectionner un onglet pour le graphique
            }
        },
        'showSelectCols': function (element) {
            var dbv = getAttributeValue(element, "dbv");
            if (dbv == "")
                dbv = 0;
            //Recuperation infos widget
            var part = element.id.split("_");
            //On recupere la DDL onglet
            var ddlTab = document.getElementById("fld_115100_115113_" + part[3]);
            if (ddlTab) {
                selectedTab = ddlTab.value;
            } else {
                selectedTab = oGridController.widget.tab;
            }

            if (selectedTab != -1) {
                //Recuperation listCol selectionné et affichage popup
                var listCol = getAttributeValue(element, "paramValue");
                showSelectCols(selectedTab, listCol, dbv, function (args) {
                    if (window.frames[modalAdvSearch.iframeId]) {
                        var list = window.frames[modalAdvSearch.iframeId].getSelectedDescId()
                        setAttributeValue(element, "paramValue", list);
                        updateListParam(element);
                        modalAdvSearch.hide();
                    }
                });
            }

        },
        'showSelectFilter': function (element, srcDllId) {
            var dbv = getAttributeValue(element, "dbv");
            var btnName = getAttributeValue(element, "paramName");
            if (dbv == "")
                dbv = 0;
            //Recuperation infos widget
            var part = element.id.split("_");
            //On recupere la DDL onglet
            var ddlTab;
            if (btnName == "filterDenId") { //Cas des indicateurs
                ddlTab = document.querySelector('[paramName="tabDenId"]');
            } else if (btnName == "filterNumId") { //Cas des indicateurs
                ddlTab = document.querySelector('[paramName="tabNumId"]')
            } else { //Cas classique
                ddlTab = document.getElementById("fld_115100_115113_" + part[3]);
            }

            if (ddlTab) {
                selectedTab = ddlTab.value;
            } else {
                selectedTab = oGridController.widget.tab;
                if (typeof (srcDllId) != 'undefined' && srcDllId != null) {
                    var rightMenu = document.getElementById("rightMenu");
                    if (rightMenu) {
                        var dll = rightMenu.querySelector("select[paramname='" + srcDllId + "']");
                        if (dll) {
                            selectedTab = dll.value;
                        }
                    }
                }
            }

            if (selectedTab != -1) {
                //Recuperation du filtre actuel
                var filter = getAttributeValue(element, "paramValue");
                filterListObjet(0, {
                    tab: selectedTab,
                    onApply: function (modal) {
                        var currentFilterId = "0";
                        var currentFilter = modal.getIframe()._eCurentSelectedFilter;
                        if (currentFilter) {
                            var oId = currentFilter.getAttribute("eid").split('_');
                            currentFilterId = oId[oId.length - 1];
                        }
                        element.innerText = top._res_8016 + ((currentFilterId != "0") ? " (1)" : "");
                        setAttributeValue(element, "paramValue", currentFilterId);
                        updateListParam(element);
                        modal.hide();

                    },
                    value: filter,
                    deselectAllowed: true,
                    adminMode: true
                });
            }
        },
        'showFilterEditor': function (element) {
            var widgetId = getAttributeValue(element, "wid");
            var ddlTab = document.querySelector('[paramName="tab"]');
            //var txtTitle = document.getElementById("fld_115100_115101_" + widgetId);
            var filterId = getAttributeValue(element, "paramvalue");
            var tab = ddlTab.value;

            var widgetType = getAttributeValue(document.getElementById("specificParamsContainer"), "widgettype");

            // Fonction d'enregistrement du filtre
            var fctSave = function () {
                var myFilterFrame = oModalFilterWizard.getIframe();
                myFilterFrame.saveDb(0, 0, false, function (oRes) {

                    if (oRes) {
                        // Le manager ne renvoie pas de "Success"
                        //if (getXmlTextNode(oRes.getElementsByTagName("success")[0]) == "1") {
                        var filterid = getXmlTextNode(oRes.getElementsByTagName("filterid")[0]);
                        setAttributeValue(element, "paramvalue", filterid);
                        updateListParam(element);
                        element.innerText = top._res_8266 + " (1)";
                        //}
                        //else {
                        //    eAlert(0, top.top._res_1760, top._res_72, getXmlTextNode(oRes.getElementsByTagName("ErrorDescription")[0]));
                        //}

                    }
                    oModalFilterWizard.hide();
                });
            }
            // Fonction de suppression du filtre
            var fctRemove = function () {
                var myFilterFrame = oModalFilterWizard.getIframe();
                var nFilterId = getNumber(myFilterFrame.document.getElementById("TxtFilterId").value);
                myFilterFrame.deleteFilter(nFilterId, getNumber(tab), function (oRes) {
                    if (oRes) {
                        if (getXmlTextNode(oRes.getElementsByTagName("success")[0]) == "1") {
                            setAttributeValue(element, "paramvalue", "");
                            updateListParam(element);
                            element.innerText = top._res_8266;
                            oModalFilterWizard.hide();
                        }
                        else {
                            eAlert(0, top._res_69, top._res_69, getXmlTextNode(oRes.getElementsByTagName("ErrorDescription")[0]));
                        }

                    }


                });
            }

            // Boutons
            var arrButtons = [];
            arrButtons.push({ label: top._res_29, fctAction: cancelFilter, css: "button-gray" });
            if (filterId != "" && filterId != "0")
                arrButtons.push({ label: top._res_6333, fctAction: fctRemove, css: "button-red" });
            arrButtons.push({ label: top._res_5003, fctAction: fctSave, css: "button-green" });

            // Ouverture de la popup
            var nTypeFilter = 9;
            if (filterId == "" || filterId == "0")
                AddNewFilter(tab, arrButtons, true, nTypeFilter, widgetId, widgetType); // Filtre widget
            else
                editFilter(filterId, tab, arrButtons, nTypeFilter, true, widgetType);

        },
        'kanbanFieldOnChange': function (element) {
            var widgetId = getAttributeValue(element, "wid");
            var selectedOption = element.options[element.selectedIndex];
            var obj = new Object();
            if (selectedOption) {
                obj.DescID = selectedOption.value;
                obj.PopupDescID = getAttributeValue(selectedOption, "data-popid");
                obj.Mask = getAttributeValue(selectedOption, "data-mask");
                obj.Label = selectedOption.text;
            }
            if (obj)
                var paramValue = JSON.stringify(obj);
            else
                var paramValue = "";

            var paramName = getAttributeValue(element, "paramname");

            updateWidgetParamValue(element, widgetId, paramName, paramValue, true);
        }
    }
})();




