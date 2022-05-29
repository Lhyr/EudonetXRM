
// Objet permettant de gérer la creation et la mise a jour des pages d'accueil
// et les widgets. Le seul objet qui permet d'interagir avec le serveur (le menu de droite a part)
var oGridController = (function () {

    var REQ_PART_XRM_HOMEPAGE = 10;
    var REQ_PART_XRM_HOMEPAGE_GRID = 11;
    var MODIF_MODE = 2;

    var _xrmGridTab = 115000;
    var _xrmWidgetTab = 115100;
    var _xrmHomePagTab = 115200;

    var _xrmGridTitleDid = 115001;
    var _xrmGridDispOrderDid = 115003;
    var _xrmGridParentTabDid = 115005;
    var _xrmGridParentFileDid = 115006;
    var _xrmWidgetContentSource = 115113;

    var _fromWebTab = false;

    /*Lance la création de la page*/
    function create(callback) {
        //Creation pour la page d'accueil :  on passe par autocreate manager pour récupérer les valeurs par défaut     
        var oUpdater = new eUpdater("mgr/eAutoCreateManager.ashx");
        oUpdater.ErrorCallBack = function () { /* TODO-Gérer les erreurs */ };
        oUpdater.addParam("tab", _xrmHomePagTab, "post");

        oUpdater.asyncFlag = true;
        oUpdater.send(function (oRes) { createReturn(oRes, callback); });
    }

    // Créer une nouvelle grille avec ratachement à l'onglet parent ou page d'accueil
    function createGrid(parentTab, parentFileId, disporder, callback) {
        //Creation sous une table existante.
        var engine = new eEngine();
        engine.Init();
        //  0 pour création
        engine.AddOrSetParam("fileid", "0");
        engine.AddOrSetParam("tab", _xrmGridTab);
        var updField;
        if (!eValidator.isNumeric(parentTab))
            return;

        updField = new fldUpdEngine(_xrmGridTitleDid);
        updField.newValue = top._res_7977;
        engine.AddOrSetField(updField);

        updField = new fldUpdEngine(_xrmGridParentTabDid);
        updField.newValue = parentTab.toString();
        engine.AddOrSetField(updField);


        if (parentFileId && eValidator.isNumeric(parentFileId) && parentFileId > 0) {
            updField = new fldUpdEngine(_xrmGridParentFileDid);
            updField.newValue = parentFileId.toString();
            engine.AddOrSetField(updField);
        }

        if (disporder) {
            updField = new fldUpdEngine(_xrmGridDispOrderDid);
            updField.newValue = '99'; //on créée la grille en position 99 pour la recaler ensuite
            engine.AddOrSetField(updField);
        }

        engine.SuccessCallbackFunction = function (oRes) { createReturn(oRes, callback, parentTab, disporder); };

        engine.UpdateLaunch();

    }

    // Id de la nouvelle page XRM
    function createReturn(oRes, callback, parentTab, disporder) {

        var oRecord = oRes.getElementsByTagName("createdrecord")[0];

        if (parentTab, disporder) {
            move(parentTab, oRecord.getAttribute('ids'), 99, disporder)
        }

        // TODO-Gérer les erreurs 
        callback(oRecord.getAttribute('ids'));
    }

    function move(tab, gridId, oldPos, newPos) {

        if (oldPos == newPos)
            return;


        //engine ne conviendra pas pour modifier l'order d'apparition des grilles dans les sous onglets, on passe donc par un autre manager
        var upd = new eUpdater("eda/mgr/eWebTabManager.ashx", 1);
        upd.addParam("action", nsAdmin.WebTabManagerAction.MOVEGRID, "post");

        var fileDiv = document.getElementById("fileDiv_" + tab);
        var ednType = getAttributeValue(fileDiv, "ednType");
        if (ednType)
            upd.addParam("ednType", ednType, "post");

        upd.addParam("oldDO", oldPos, "post");
        upd.addParam("newDO", newPos, "post");
        upd.addParam("gridid", gridId, "post");
        upd.addParam("tab", tab, "post");

        if (tab == 115200)
            upd.addParam("pageid", getAttributeValue(fileDiv, "fid"), "post");


        upd.send(

            //Maj du tab
            // oRes contient le html de l'admin des propriété du webtab
            function (oRes) {
                //met à jour la navbar et la barre de propriété
                nsAdmin.updateNavBarSpecifMaj(oRes, 0);
            }

            );
    }

    function editContent(widgetId) {

        var jsVarName = "modalEdit";
        var oWinSize = top.getWindowSize();

        var lWidth = Math.round(oWinSize.w * 0.95);
        var lHeight = Math.round(oWinSize.h * 0.95);

        var editDialog = new eModalDialog(
                        top._res_85, // Titre
                        0,                          // Type
                        "eMemoDialog.aspx",              // URL
                        lWidth,                        // Largeur
                        lHeight);                       // Hauteur

        editDialog.ErrorCallBack = launchInContext(editDialog, editDialog.hide);

        editDialog.addParam("DescId", _xrmWidgetContentSource, "post");
        editDialog.addParam("FileId", widgetId, "post");
        editDialog.addParam("TabId", _xrmWidgetTab, "post");
        editDialog.addParam("Title", top._res_85, "post");

        editDialog.addParam("EditorJsVarName", jsVarName, "post");

        //editDialog.addParam("ParentEditorJsVarName", this.name, "post");

        editDialog.addParam("EditorType", "widget", "post");
        editDialog.addParam("EnableTemplateEditor", "0", "post"); // #68 13x - Pas d'utilisation de l'éditeur de templates HTML avancé (grapesjs) dans ce cas
        editDialog.addParam("width", lWidth - 12, "post"); // 12 : marge intérieure par rapport au conteneur de l'eModalDialog
        editDialog.addParam("height", lHeight - 150, "post"); // 150 : espace réservé à la barre de titre + boutons de l'eModalDialog
        editDialog.addParam("IsHTML", "1", "post");
        editDialog.addParam("ReadOnly", "0", "post");
        // la taille est en pleine ecran, pas de bouton zoom
        editDialog.hideMaximizeButton = true;

        editDialog.show();

        editDialog.addButton(top._res_29, function () { editDialog.hide(); }, "button-gray", jsVarName, "cancel"); // Annuler
        editDialog.addButton(top._res_28, function () {
            var data = editDialog.getIframe().eMemoDialogEditorObject.getData();
            var param = getUpdateParam(_xrmWidgetTab, _xrmWidgetContentSource, widgetId, data, "9", null, null);
            oGridController.config.update(param);
            editDialog.hide();
        }, "button-green", jsVarName, "ok"); // Valider
    }

    function openWidgetInPopup(id, gridId) {

        var oWinSize = top.getWindowSize().scale(0.95);
        var lWidth = Math.round(oWinSize.w);
        var lHeight = Math.round(oWinSize.h);
        var widgetPopup = new eModalDialog(top._res_6594, 0, "eXrmWidgetContentUI.aspx?wid=" + id + "&z=1&w=" + lWidth + "&h=" + lHeight + "&gid=" + gridId, lWidth, lHeight);
        widgetPopup.hideMaximizeButton = true;
        widgetPopup.show();
        widgetPopup.addButton(top._res_30, function () { widgetPopup.hide(); }, "button-gray", null, "cancel"); // Fermer
    }

    function getUpdateParam(tab, descid, fileid, value, format, srcElement, oldValue) {
        //Param principal
        var param = new Object();
        param.src = srcElement;
        param.dbv = oldValue;
        param.format = format;
        param.value = value;
        param.fileId = fileid;
        param.descId = descid;
        param.tab = tab;
        param.callback = function (result) {
            if (param.src)
                setAttributeValue(param.src, "dbv", oldValue);
            if (param.tab == 115100) {
                var widget = document.getElementById("widget-wrapper-" + param.fileId);
                if (widget)
                    nsAdminGrid.reloadWidget(widget);
            };
        }
        return param;

    }

    // Appel au manager pour mettre à jour la RES
    // action : 1 pour update, 2 pour delete
    function updateFileRes(action, param, langId, callback) {
        var oUpdater = new eUpdater("mgr/eFilesResManager.ashx", 1);
        oUpdater.addParam("descid", param.descId, "post");
        oUpdater.addParam("tab", param.tab, "post");
        oUpdater.addParam("fileid", param.fileId, "post");
        oUpdater.addParam("langid", langId, "post");
        oUpdater.addParam("value", param.value, "post");
        oUpdater.addParam("action", action, "post");

        oUpdater.asyncFlag = true;

        oUpdater.send(function (oRes) {
            if (typeof (oRes) == "undefined" || oRes == null || oRes == "")
                return;

            var result = JSON.parse(oRes);
            if (!result.Success) {
                top.eAlert(0, top._res_416, "", result.UserError);
            }
            else {
                if (typeof (callback) == "function") {
                    callback();
                }
            }
        });
    }





    // objet interne non accessible de l'exterieur
    var _widget = {
        // exec
        'process': function (options, addParm) {

            var oUpdater;
            if (options.queued)
                oUpdater = new QueuedUpdater("mgr/eXrmWidgetManager.ashx", 1);
            else
                oUpdater = new eUpdater("mgr/eXrmWidgetManager.ashx", 1);

            //Load Web Tab Properties          
            oUpdater.addParam("action", options.action, "post");
            oUpdater.addParam("widgetid", options.id, "post"); // id liaison              
            oUpdater.addParam("sid", options.sid, "post"); // specif id     
            oUpdater.addParam("gridid", options.gridId || oGridController.grid.getId(), "post"); // id page

            if (top.nGlobalActiveTab && typeof (top.GetCurrentFileId) == 'function') {
                oUpdater.addParam("parenttab", top.nGlobalActiveTab, "post");
                if (typeof top.GetCurrentFileId === "function")
                    oUpdater.addParam("parentfid", top.GetCurrentFileId(nGlobalActiveTab), "post");
            }


            if (typeof (addParm) == "function")
                addParm(oUpdater);

            oUpdater.asyncFlag = true;

            oUpdater.ErrorCallBack = function (oRes) { options.callback(null); };
            oUpdater.send(function (oRes) { _widget.processReturn(oRes, options); });
        },
        // après exec de l'action
        'processReturn': function (oRes, options) {

            // Si sendJson fait appel à un manager qui ne renvoie pas de JSON en retour, on ne fait rien de plus
            if (typeof (oRes) == "undefined" || oRes == null || oRes == "")
                return;

            var result = JSON.parse(oRes);
            if (!result.Success)
                top.eAlert(1, top._res_416, result.ErrorTitle, result.ErrorMsg + "<br />" + result.DebugMsg);



            if (typeof (options.callback) == 'function')
                options.callback(result);

        }
    };
    var _event = {
        'listeners': new Object(),
        'notify': function (evtName, args) {
            for (var id in _event.listeners)
                _event.listeners[id]({
                    "name": evtName,
                    "args": args
                });
        }
    };
    var _engine = {
        'delete': function (param) {
            var eEngineUpdater = new eEngine();
            eEngineUpdater.Init();
            eEngineUpdater.DeleteRecord = true;
            eEngineUpdater.AddOrSetParam('tab', param.tab);
            eEngineUpdater.AddOrSetParam('fileId', param.fileId);
            // on gère nous-même la confirmation, ne pas afficher la confirmation de engine
            // La confirmation est géré par eEngine.cs
            //eEngineUpdater.AddOrSetParam('validDeletion', '1');

            eEngineUpdater.SuccessCallbackFunction = function (result) {
                // Suppression des RES associées
                // déjà géré
                //  updateFileRes(2, param, -1, function () {
                top.setWait(false);
                _engine.return(result, param.callback);
                // });
            };
            eEngineUpdater.UpdateLaunch();
        },
        'update': function (fileId, fieldEngineList, callback) {

            var eEngineUpdater = new eEngine();
            eEngineUpdater.Init();

            eEngineUpdater.AddOrSetParam('fileId', fileId);
            eEngineUpdater.AddOrSetParam("engAction", 2);

            var triggerList = new Array();
            for (var i = 0; i < fieldEngineList.length; i++) {
                triggerList.push(fieldEngineList[i].descId);
                eEngineUpdater.AddOrSetField(fieldEngineList[i]);
            }

            eEngineUpdater.AddOrSetParam("fieldTrigger", triggerList.join(";"));

            eEngineUpdater.SuccessCallbackFunction = function (result) { _engine.return(result, callback); };
            eEngineUpdater.UpdateLaunch();
        },
        'return': function (engResult, callback) {
            callback(engResult);
        }
    };
    var _pageAssign = {

        'modalUserCat': null,
        'openUserCat': function (selectedValues, type, callback) {

            _pageAssign.modalUserCat = new eModalDialog(top._res_246, 0, "eCatalogDialogUser.aspx", 550, 610);

            top.eTabCatUserModalObject.Add(_pageAssign.modalUserCat.iframeId, _pageAssign.modalUserCat);

            _pageAssign.modalUserCat.addParam("iframeId", _pageAssign.modalUserCat.iframeId, "post");
            _pageAssign.modalUserCat.ErrorCallBack = function () { setWait(false); }
            _pageAssign.modalUserCat.addParam("multi", "1", "post");

            if (type == "group")
                _pageAssign.modalUserCat.addParam("showgrouponly", "1", "post");

            if (type == "user")
                _pageAssign.modalUserCat.addParam("showuseronly", "1", "post");

            _pageAssign.modalUserCat.addParam("selected", selectedValues, "post");
            _pageAssign.modalUserCat.addParam("modalvarname", "_catalog.modalUserCat", "post");

            _pageAssign.modalUserCat.show();

            _pageAssign.modalUserCat.addButton(top._res_29, _pageAssign.onUserCatCancel, "button-gray", null, null, true);
            _pageAssign.modalUserCat.addButton(top._res_28, function () { _pageAssign.onUserCatOk(callback, type); }, "button-green");
        },
        'onUserCatOk': function (callback, type) {

            var strReturned = _pageAssign.modalUserCat.getIframe().GetReturnValue();

            var vals = strReturned.split('$|$')[0];
            var libs = strReturned.split('$|$')[1];

            var obj = _pageAssign.cleanUsersOrGroup(vals, libs, type);

            if (typeof (callback) == "function")
                callback({ 'vals': obj.vals, 'libs': obj.libs });

            _pageAssign.onUserCatCancel();

        },
        'onUserCatCancel': function () {
            _pageAssign.modalUserCat.hide();
        },
        'cleanUsersOrGroup': function (vals, libs, type) {

            var valsParts = vals.split(";");
            var libsParts = libs.split(";");

            if (valsParts.length != libsParts.length)
                return { 'vals': vals, 'libs': libs };

            var obj = { 'vals': "", 'libs': "" };

            for (var i = 0; i < valsParts.length; i++) {
                if ((type == "group" && valsParts[i].indexOf("G") == 0) || (type == "user" && eValidator.isNumeric(valsParts[i]))) {

                    obj.vals = obj.vals + valsParts[i];
                    obj.libs = obj.libs + libsParts[i];
                    if (i < valsParts.length - 1) {
                        obj.vals = obj.vals + ";";
                        obj.libs = obj.libs + ";";
                    }
                }
            }

            if (obj.libs == "") obj.libs = "_";

            return obj;
        }
    }
    var _perm = {

        'show': function (tab, pageId, gridId) {

            top.modalRights = new eModalDialog(top._res_6850, 0, "eda/eAdminRightsDialog.aspx", 1000, 800, "modalAdminRights");
            top.modalRights.noButtons = false;
            top.modalRights.addParam("tab", tab, "post");
            top.modalRights.addParam("pageid", pageId, "post");
            top.modalRights.addParam("gridid", gridId, "post");
            top.modalRights.addParam("iframeScrolling", "yes", "post");
            top.modalRights.onIframeLoadComplete = nsAdmin.LoadFunctionsValues;
            top.modalRights.show();
            top.modalRights.addButton(top._res_30, function () { top.modalRights.hide(); }, 'button-gray', null);
            nsAdmin.modalResizeAndMove(top.modalRights);
            top.modalRights.resizeToMaxWidth();
        }

    }

    return {
        'events': {
            'on': function (id, listener) {
                _event.listeners[id] = listener;
            },
            'off': function (id) {
                if (id in _event.listeners)
                    delete _event.listeners[id];
            },
        },
        'grid': {
            'tab': _xrmGridTab,
            // Retourne l'id de la page d'accueil
            // il n'y a qu'une seule page à la fois
            'getId': function () {
                var gid;
                var container = document.getElementById("widget-grid-container");
                if (container) {
                    gid = getAttributeValue(container, "gid");

                    if (gid != "")
                        return gid;
                }

                return 0;
            },
            // Créer une nouvelle page d'accueil xrm puis charger la page
            'new': function (parentTab, disporder) {

                var parentFileId = 0;
                var fileDiv = document.getElementById("fileDiv_" + parentTab);
                if (fileDiv)
                    parentFileId = getAttributeValue(fileDiv, "fid") * 1;

                createGrid(parentTab, parentFileId, disporder,
                    function (id) {
                        if (parentTab == oGridController.page.tab)
                            oGridController.page.load(parentFileId);
                        else
                            nsAdmin.loadAdminFile(parentTab);
                    });
            },


            //déplace la grille dans la barredes onglets web
            'move': function (tab, gridId, oldPos, newPos) {
                move(tab, gridId, oldPos, newPos);
            },

            // Charge la page avec le menu xrm ou en créer une si id=0 
            'load': function (id) {

                // Chargement du menu de droite indépendemment de la page
                // si dans le cas de dependance on remonte cette ligne dans la fonction
                // du callback de loadPage
                oAdminGridMenu.loadMenu(oGridController.grid.tab, id, null, function () { });
                oGridController.grid.loadGrid(id);
            },
            'reloadGrid': function () {
                this.loadGrid(this.getId());
            },
            'loadGrid': function (id) {

                var tabMainDivWH = GetMainDivWH();
                var height = tabMainDivWH[1];
                var width = tabMainDivWH[0];
                var upd = new eUpdater("mgr/eFileAsyncManager.ashx", 1);
                upd.addParam("tab", oGridController.grid.tab, "post");
                upd.addParam("fileid", id, "post");
                upd.addParam("part", REQ_PART_XRM_HOMEPAGE_GRID, "post");
                upd.addParam("type", MODIF_MODE, "post");//
                upd.addParam("height", height, "post");
                upd.addParam("width", width, "post");
                upd.addParam("callby", "oGridController.grid.loadGrid", "post");
                upd.addParam("callbyarguments", "no-args", "post");
                upd.send(function (oRes) {
                    var filePart1 = document.getElementById("divFilePart1");
                    if (filePart1) {

                        var header = filePart1.querySelector('header');

                        //enleve les enfant
                        while (filePart1.firstChild)
                            filePart1.removeChild(filePart1.firstChild);

                        var listHeader = document.createElement("div");
                        listHeader.id = "listheader";
                        listHeader.innerHTML = oRes;

                        if (header != null)
                            filePart1.appendChild(header);

                        if (listHeader != null)
                            filePart1.appendChild(listHeader);

                        // enlève les freres
                        while (filePart1.nextSibling)
                            filePart1.parentNode.removeChild(filePart1.nextSibling);

                    } else {
                        var divId = "listheader";
                        var mainDiv = document.getElementById(divId);

                        if (mainDiv != null)
                            mainDiv.innerHTML = oRes;


                    }

                    var divInfo = document.getElementById("infos");
                    if (divInfo) {
                        // enlève les freres
                        while (divInfo.nextSibling)
                            divInfo.parentNode.removeChild(divInfo.nextSibling);

                        var toolbar = divInfo.querySelector("div[toolbar='0']");
                        if (toolbar)
                            setAttributeValue(toolbar, "toolbar", "1");
                    }


                    // Après le chargement de la page,
                    // on initialise la grille
                    //oXrmHomeGrid.init();

                    // chargement des graphique s'ils existent
                    loadSyncFusionChart();

                    top.setWait(false);

                });

            },
            'config': function () {
                oAdminGridMenu.loadMenu(oGridController.grid.tab, oGridController.grid.getId(), null, function (res) { });
            },
            'delete': function (gridId, callback) {

                var param = new Object();
                param.tab = oGridController.grid.tab;
                param.fileId = gridId;
                param.callback = callback;
                _engine.delete(param);

               
            }

        },
        'page': {
            'tab': _xrmHomePagTab,
            // Créer une nouvelle page d'accueil xrm puis charger la page
            'new': function () {
                create(oGridController.page.load);
            },
            'click': function (fileId, source) {

                if (getAttributeValue(source, "action") == "delete")
                    this.delete(fileId);
                else if (getAttributeValue(source, "action") == "duplicate")
                    this.duplicate(fileId);
                else
                    this.load(fileId);
            },
            // Charge la page avec le menu xrm 
            'load': function (id) {

                // Rechargement du fil d'ariane
                nsAdmin.loadNavBar('ADMIN_HOME_XRM_HOMEPAGE',
                    {
                        'tab': nGlobalActiveTab,
                        'pageId': id,
                        'gridId': 0,
                        'gridLabel': ""
                    });

                if (typeof (oAdminGridMenu) != "undefined")
                    oAdminGridMenu.loadMenu(oGridController.page.tab, id, null, function (res) { });

                var winSize = top.getWindowSize();

                var oFile = getFileUpdater(oGridController.page.tab, id, MODIF_MODE, true);
                oFile.addParam("part", REQ_PART_XRM_HOMEPAGE, "post");
                oFile.addParam("height", winSize.h, "post");
                oFile.send(function (oRes) {

                    var divId = "mainDiv";
                    var mainDiv = document.getElementById(divId);

                    if (mainDiv != null)
                        mainDiv.innerHTML = oRes;

                    // Après le chargement de la page,
                    // on initialise la grille
                    //oXrmHomeGrid.init();

                    nsAdminFile.browser = new getBrowser();
                    nsAdmin.setDDHandlers();

                    top.setWait(false);

                });
            },
            'duplicate': function (fileId) { },
            'delete': function (fileId) {
                var param = new Object();
                param.tab = oGridController.page.tab;
                param.fileId = fileId;
                param.callback = function () {
                    // on retourne  à la liste des grille après suppression
                    nsAdmin.loadAdminModule('ADMIN_HOME_XRM_HOMEPAGES', null, { tab: 115200 });
                }

                var chks = document.querySelectorAll(".chkDefaultHP[chk='1']");
                for (var i = 0; i < chks.length; i++) {
                    if (getAttributeValue(chks[i], "data-hpid") == fileId) {
                        eAlert(1, top._res_69, top._res_2686, top._res_2687);
                        return;
                    }
                }


                _engine.delete(param);
             

            },
            'assign': function (element, fileId, type) {

                var dbv = getAttributeValue(element, "dbv");
                var descId = element.id.split("_")[2];

                _pageAssign.openUserCat(dbv, type,
                    function (obj) {
                        var param = new Object();
                        param.src = element;
                        param.dbv = obj.vals;
                        param.format = getAttributeValue(element, "fmt") * 1;
                        param.value = obj.vals;
                        param.fileId = fileId * 1;
                        param.descId = descId * 1;
                        param.tab = oGridController.page.tab;
                        param.callback = function (result) {


                            SetText(element, obj.libs);
                            setAttributeValue(element, "dbv", obj.vals);
                        }

                        oGridController.config.update(param);

                    });
            },

        },
        'widget': {
            'tab': _xrmWidgetTab,
            // creer une nouvelle widget
            'new': function (options) {
                // on l'ajoute a la structure
                // si Ok on sauvegarde dans la base sinon on le retire de la structure               
                // var hostSuccess = oXrmHomeGrid.host(options);
                // if (hostSuccess) {
                options.id = 0;
                options.action = XrmWidgetAction.CREATE_WIDGET;


                var fct = options.callback;
                top.setWait(true);
                options.callback = function (res) { top.setWait(false); fct(res); };

                _widget.process(options,
                    function (oUpdater) {
                        oUpdater.addParam("type", getAttributeValue(options.src, 't'), "post");
                        oUpdater.addParam("posx", getAttributeValue(options.src, 'x'), "post");
                        oUpdater.addParam("posy", getAttributeValue(options.src, 'y'), "post");
                        oUpdater.addParam("width", getAttributeValue(options.src, 'w'), "post");
                        oUpdater.addParam("height", getAttributeValue(options.src, 'h'), "post");
                    });
                //}
            },
            // supprime le widget et les liaisons
            'delete': function (options) {

                // on demande la confirmation
                eAdvConfirm({
                    'criticity': eAdvConfirm.CRITICITY.MSG_EXCLAM,
                    'title': top._res_8137, // "Suppression du widget",
                    'message': top._res_8138, // "Supprimer le widget",
                    'details': top._res_7637, // "Etes-vous sûr de supprimer le sous-onglet  ?"       
                    'cssOk': 'button-red',
                    'cssCancel': 'button-green',
                    'resOk': top._res_19,
                    'resCancel': top._res_29,
                    'okFct': function () {
                        options.action = XrmWidgetAction.DELETE_WIDGET;
                        _widget.process(options);


                        var fct = options.callback;
                        top.setWait(true);
                        options.callback = function (res) { top.setWait(false); fct(res); };

                        // La suppression d'un widget devrait vider les propriétés dans le menu de droite
                        var navTabs = document.getElementById("navTabs");
                        if (navTabs) {
                            var paramTabPicto1 = navTabs.querySelector("li span#paramTabPicto1");
                            if (paramTabPicto1) paramTabPicto1.click();

                            var paramTab2 = document.getElementById("paramTab2");
                            if (paramTab2) {

                                var h3 = paramTab2.querySelector("h3");
                                if (h3) h3.innerHTML = top._res_8118; //Aucun element séléctionné

                                var p = paramTab2.querySelector("p");
                                if (p) p.innerHTML = top._res_8119;// Veuillez cliqué sur un widget

                                var paramBlockContent = paramTab2.querySelector("div.paramBlockContent");
                                if (paramBlockContent) paramBlockContent.innerHTML = "";
                            }
                        }
                    }
                });

            },
            // Editer le contenu du widget (type éditeur)
            'edit': function (options) {
                editContent(options.id);
            },
            // Ouvre le contenu du widget en popup
            'zoom': function (options) {
                openWidgetInPopup(options.id, oGridController.grid.getId());
            },
            // Ajoute une liaison entre la page et le widget
            'link': function (options) {
                options.action = XrmWidgetAction.LINK_WIDGET;
                _widget.process(options);
            },
            //supprime la liaison
            'unlink': function (options) {
                // on demande la confirmation
                eAdvConfirm({
                    'criticity': eAdvConfirm.CRITICITY.MSG_EXCLAM,
                    'title': top._res_8139, //"Suppression de liaison",
                    'message': top._res_8140, //"Supprimer la liaison",
                    'details': top._res_8141,//"Etes-vous sur de retirer le widget de la grille ?",
                    'cssOk': 'button-red',
                    'cssCancel': 'button-green',
                    'resOk': top._res_19,
                    'resCancel': top._res_29,
                    'okFct': function () {
                        options.action = XrmWidgetAction.UNLINK_WIDGET;
                        _widget.process(options);
                    }
                });
            },
            'reload': function (options) {
                options.action = XrmWidgetAction.REFRESH_WIDGET;
                _widget.process(options);
            }
        },
        'pref': {
            'setVisible': function (options) {
                options.action = XrmWidgetAction.SAVE_VISIBLE_PREF;
                _widget.process(options, function (oUpdater) { oUpdater.addParam("ids", options.ids, "post"); });
            },
            'update': function (options) {
                options.action = XrmWidgetAction.SAVE_WIDGET_PREF;
                _widget.process(options,
                    function (oUpdater) {
                        // pref user
                        oUpdater.addParam("posx", getAttributeValue(options.evt.src, 'x'), "post");
                        oUpdater.addParam("posy", getAttributeValue(options.evt.src, 'y'), "post");
                        oUpdater.addParam("width", getAttributeValue(options.evt.src, 'w'), "post");
                        oUpdater.addParam("height", getAttributeValue(options.evt.src, 'h'), "post");
                    });

            },
            'delete': function (options) {
                // on demande la confirmation
                eAdvConfirm({
                    'criticity': eAdvConfirm.CRITICITY.MSG_QUESTION,
                    'title': top._res_8144,//"Restauration",
                    'message': top._res_8142,// "Restauration la position et la taille d'origine du widget",
                    'details': top._res_8143,//"Etes-vous sur de vouloir continuer  ?",
                    'cssOk': 'button-red',
                    'cssCancel': 'button-green',
                    'resOk': top._res_19,
                    'resCancel': top._res_29,
                    'okFct': function () {
                        options.action = XrmWidgetAction.DELETE_WIDGET_PREF;
                        _widget.process(options);
                    }
                });
            }
        },
        'config': {
            'updateFromWebTab': function (fromWebTab) { _fromWebTab = fromWebTab; },
            'update': function (param) {

                if (!eValidator.isValid(param)) {
                    //6275, "Format incorrect"                    
                    //2021,"La valeur saisie ""<VALUE>"" ne correspond pas au format et/ou à la longueur de la rubrique <FIELD>"
                    var errorMessage = top._res_2021.replace("<VALUE>", param.value).replace("<FIELD>", "\"" + param.lib + "\"");
                    eAlert(1, "Erreur", top._res_6275, errorMessage);

                    // Liseret rouge pendant 1 seconde
                    if (param.src) {
                        param.src.style.border = "2px #ff9393 solid"; // red
                        setTimeout(function () {
                            param.src.style.border = "";
                            param.src.value = param.dbv;
                        }, 1000);
                    }


                    return;
                }

                var fieldEngine = new fldUpdEngine(param.descId);
                fieldEngine.newValue = param.value;
                fieldEngine.forceUpdate = '1';

                var fieldList = new Array();
                fieldList.push(fieldEngine);

                // Liseret vert pendant au moins 0.6 seconde
                if (param.src)
                    param.src.style.border = "1px #86bb31 solid"; //green

                _engine.update(param.fileId, fieldList, function (result) {
                    setTimeout(function () { if (param.src) param.src.style.border = ""; }, 600);

                    if (typeof (param.callback) == "function")
                        param.callback(result);
                });

                // La valeur doit-elle être traduite ?
                if (getAttributeValue(param.src, "lang") != "") {
                    updateFileRes(1, param, getAttributeValue(param.src, "lang"));
                }
            },
            'updateParams': function (options) {
                options.action = XrmWidgetAction.SAVE_WIDGET_PARAM;

                addClass(options.src, "updatedField");

                var oUpdater = new eUpdater("mgr/eXrmWidgetManager.ashx", 1);
                oUpdater.addParam("action", options.action, "post");
                oUpdater.addParam("widgetid", options.id, "post"); // id liaison              
                oUpdater.addParam("gridid", options.gridId || oGridController.grid.getId(), "post"); // id page
                oUpdater.addParam("paramname", options.paramname, "post");
                oUpdater.addParam("paramvalue", options.paramvalue, "post");
                oUpdater.addParam("type", options.type, "post");

                oUpdater.asyncFlag = true;

                oUpdater.ErrorCallBack = function (oRes) { options.callback(null); };
                oUpdater.send(function (oRes) {

                    if (options.src) {
                        removeClass(options.src, "updatedField");
                    }

                    if (options.callback && typeof options.callback === "function") {
                        options.callback();
                    }

                    if (options.tab == 115100) {
                        var widget = document.getElementById("widget-wrapper-" + options.id);
                        if (widget) {
                            eTools.setWidgetWait(options.id, true);
                            nsAdminGrid.reloadWidget(widget);
                        }

                    };


                });
            },
            'updateList': function (fileId, paramList, callback) {

                var fieldEngineList = new Array();
                var param;
                for (var i = 0 ; i < paramList.length; i++) {
                    param = paramList[i];

                    if (!eValidator.isValid(param)) {
                        //6275, "Format incorrect" 2021,"La valeur saisie ""<VALUE>"" ne correspond pas au format et/ou à la longueur de la rubrique <FIELD>"
                        var errorMessage = top._res_2021.replace("<VALUE>", param.value).replace("<FIELD>", "\"" + param.lib + "\"");
                        eAlert(1, "Erreur", top._res_6275, errorMessage);
                        return;
                    }

                    var fieldEngine = new fldUpdEngine(param.descId);
                    fieldEngine.newValue = param.value;
                    fieldEngine.forceUpdate = '1';

                    fieldEngineList.push(fieldEngine);
                }


                _engine.update(fileId, fieldEngineList, callback);
            },
            'rename': function (parent) {

                setAttributeValue(parent, "edit", "1");
                var textEditor = parent.querySelector("input[type='text']");
                if (textEditor)
                    textEditor.focus();

                oAdminGridMenu.showBlock("paramTab3");
            },
            'blur': function (child) {

                var descId = getAttributeValue(child, "did");
                var fileId = getAttributeValue(child, "fid");
                var tab = getAttributeValue(child, "tab");
                var newValue = child.value;

                // Mise à jour du label
                var label = child.previousSibling;
                if (label == null)
                    return;

                // l'input principale du titre
                var titleField = document.getElementById("fld_" + tab + "_" + descId + "_" + fileId);
                if (!titleField) {
                    setAttributeValue(child.parentElement, "edit", "0");
                    child.value = label.value;
                    return;
                }

                titleField.value = newValue;
                SetText(label, newValue);

                oAdminGridMenu.updateField({
                    'id': fileId,
                    'tab': tab,
                    'src': titleField,
                    'callback': function (engineResult) {

                        if (tab == oGridController.grid.tab) {
                            // Container des sous-onglets de type grille
                            var tabsEntries = document.getElementById("ulWebTabs");
                            // mise en surbrillance de l'entrée correspondante
                            if (tabsEntries != null) {
                                var gridLabel = tabsEntries.querySelector("li[fid='" + fileId + "'] > span");
                                if (gridLabel)
                                    SetText(gridLabel, newValue);
                            }
                        }
                    }
                });

                setAttributeValue(child.parentElement, "edit", "0");

            },
            'showViewPerm': function (tab, pageId, gridId) {
                _perm.show(tab, pageId, gridId);
            },
            'updateRes': function (param, langId) {
                updateFileRes(1, param, langId);
            }
        }
    }
}());

