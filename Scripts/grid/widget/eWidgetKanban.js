var eWidgetKanban = function (nWid, nTab, sContext) {

    var _nWid = nWid;
    var _nTab = nTab;
    var _context = sContext;
    var _slDescid = 0;
    var _slIsGroup = false;
    var _oDragDropMgr = null;

    // Mémorisation de la position du scroll vertical (pas besoin pour l'horizontal car il appartient à <form> qui n'est pas rechargé)
    var _scrollTop = 0;

    // Tag booléen pour indiquer que le kanban a été reloadé
    var _reloaded = false;

    var _columMinWidth = 300;
    var _widgetMargins = 20;
    var _debugMode = false;

    // Initialisation
    function init() {

        // Redimensionnement de l'iframe
        //console.log("Redimensionnement de l'iframe");
        var widgetWrapper = top.document.getElementById("widget-wrapper-" + _nWid);
        resize(parseInt(widgetWrapper.style.width), parseInt(widgetWrapper.style.height));


        // Ouverture/fermeture des lignes de couloir
        //console.log("Ouverture/fermeture des lignes de couloir");
        var listSL = document.getElementsByClassName("kbSL");
        for (var i = 0; i < listSL.length; i++) {

            listSL[i].addEventListener("click", function () {

                var slID = getAttributeValue(this, "data-slid");

                var block = this.parentElement.querySelector(".kbColumnsContainer[data-slid='" + slID + "']");

                if (getAttributeValue(this, "data-active") == "1") {
                    this.children[0].className = "icon-caret-right";
                    setAttributeValue(this, "data-active", "0");
                    setAttributeValue(block, "data-active", "0");
                }
                else {
                    this.children[0].className = "icon-caret-down";
                    setAttributeValue(this, "data-active", "1");
                    setAttributeValue(block, "data-active", "1");
                }
            });

        }

        // Clic sur les titres des cartes
        //console.log("Clic sur les titres des cartes");
        var cardTitles = document.getElementsByClassName("cardTitle");
        for (var i = 0; i < cardTitles.length; i++) {

            cardTitles[i].addEventListener("click", function () {

                var fid = getAttributeValue(this, "data-fid");
                var tab = getAttributeValue(this, "data-tab");
                var bOpenPopup = getAttributeValue(this, "data-openpopup") == "1";

                if (fid != "" && fid != "0") {
                    if (bOpenPopup) {
                        shFileInPopup(tab, fid, getAttributeValue(this, "data-tablabel"), 0, 0, 0, "", true, function () {
                            // After validate
                            eTools.setWidgetWait(_nWid, false);
                            //refresh();
                        }, CallFromKanban, "widget-wrapper-" + _nWid, this);
                    }
                    else
                        top.loadFile(tab, fid, 3);
                }
                
            });

        }

        // Clic sur une rubrique de ligne de couloir
        //console.log("Clic sur une rubrique de ligne de couloir");
        var slOptions = document.querySelectorAll("#kbListSL li");
        for (var i = 0; i < slOptions.length; i++) {
            slOptions[i].addEventListener("click", function () {
                selectSL(
                    getAttributeValue(this, "data-sldid"),
                    (getAttributeValue(this, "data-usergroup") == "1")
               );
            });
        }
        var btnSL = document.getElementById("slLabel");
        if (btnSL) {
            btnSL.addEventListener("click", function () {
                selectSL(
                    getAttributeValue(this, "data-sldid"),
                    (getAttributeValue(this, "data-usergroup") == "1")
               );
            }, true);
        }
        

        //console.log("Drag&Drop Manager");
        // Drag&Drop

        // Si cela a été initié avant, il faut unbind les events
        if (_oDragDropMgr)
            _oDragDropMgr.clearEvents();

        var ddOptions = {
            draggedClass: "kanbanCard",
            dropzoneClass: "kbCol",
            shadowClass: "kbCardShadow",
            dropCallbackFct: function (draggedElt, origDropZone, dropZone) {

                // Suppression des lignes de couloir survolés
                var lanes = document.querySelectorAll(".kbSL.hovered");
                for (var i = 0; i < lanes.length; i++) {
                    removeClass(lanes[i], "hovered");
                }

                updateCard(draggedElt, origDropZone, dropZone);
            },
            checkDropAllowedFct: function (draggedElt, origDropZone, dropZone) {
                //return true;
                return checkDropAllowed(draggedElt, origDropZone, dropZone, true);
            },
            checkDragEnterAllowedFct: function (draggedElt, origDropZone, dropZone) {
                //return true;
                return checkDropAllowed(draggedElt, origDropZone, dropZone, false);
            }
        };

        _oDragDropMgr = new eDragDropManager(ddOptions);
        _oDragDropMgr.init();
        _oDragDropMgr.setKanbanSpecificCallbacks();

        
    }

    // Vérification que le drop est possible
    function checkDropAllowed(draggedElt, origDropZone, dropZone, bShowAlert) {

        // Test si la rubrique définie en colonne est obligatoire
        var bMandatory = getAttributeValue(draggedElt, "data-colrequired") == "1";
        var bReadOnly = getAttributeValue(draggedElt, "data-colro") == "1";

        if (bMandatory) {

            var colId = getAttributeValue(dropZone, "data-col");
            if (colId == "0") {
                if (bShowAlert)
                    eAlert(2, top._res_92, top._res_7548, top._res_8187);
                return false;
            }

        }

        // Test si on peut modifier la valeur de la rubrique en colonne
        if (bReadOnly) {
            var oldColId = getAttributeValue(origDropZone, "data-col") || "0";
            var colId = getAttributeValue(dropZone, "data-col") || "0";

            if (oldColId != colId) {
                if (bShowAlert)
                    eAlert(2, top._res_92, top._res_8187, top._res_8405);
                return false;
            }
        }

        // Test si la rubrique définie en ligne de couloir est obligatoire
        var bMandatory = getAttributeValue(draggedElt, "data-slrequired") == "1";
        var bReadOnly = getAttributeValue(draggedElt, "data-slro") == "1";

        if (bMandatory) {

            var slId = getAttributeValue(dropZone, "data-sl");
            if (slId == "0") {
                if (bShowAlert)
                    eAlert(2, top._res_92, top._res_7548, top._res_8187);
                return false;
            }
        }

        // Test si on peut modifier la valeur de la ligne de couloir
        if (bReadOnly) {
            var oldSlId = getAttributeValue(origDropZone, "data-sl") || "0";
            var slId = getAttributeValue(dropZone, "data-sl") || "0";

            if (oldSlId != slId) {
                if (bShowAlert)
                    eAlert(2, top._res_92, top._res_8187, top._res_8405);
                return false;
            }
        }


        // Recherche des champs obligatoires de la carte à déplacer
        var fieldValue;
        var arrRequiredFields = new Array();

        var colDid = "";
        var attrSetCol = getAttributeValue(dropZone, "data-setcol");
        if (attrSetCol.split('|').length == 2)
            colDid = attrSetCol.split('|')[0];

        var requiredFields = draggedElt.querySelectorAll(".cardField[data-required='1']");
        for (var i = 0; i < requiredFields.length; i++) {
            fieldValue = requiredFields[i].querySelector(".fieldValue");
            if (getAttributeValue(fieldValue, "data-did") == colDid) {
                continue;
            }
            if (GetText(fieldValue).trim() == "") {
                arrRequiredFields.push(getAttributeValue(fieldValue, "data-label"));
            }
        }
        if (arrRequiredFields.length > 0) {
            var strRequiredFields = '';
            for (var i = 0; i < arrRequiredFields.length; i++) {
                strRequiredFields += arrRequiredFields[i].replace(/\n/g, "") + '<br />';
            }
            if (bShowAlert) {
                eAlert(0, top._res_372, top._res_1268, strRequiredFields);
                return false;
            }
                
        }

        return true;
    }

    // Mise à jour d'une carte
    function updateCard(draggedElt, eltOrig, eltDest) {

        _reloaded = false;

        var fileId = getAttributeValue(draggedElt, "data-fid");

        var oEngine = new eEngine();

        oEngine.Init();

        oEngine.AddOrSetParam('tab', _nTab);
        oEngine.AddOrSetParam('fileId', fileId);
        oEngine.AddOrSetParam("engAction", "1"); // UPDATE
        oEngine.AddOrSetParam("fromKanban", "1"); 
        
        // Nouvelle colonne
        var colId;
        var origCol = getAttributeValue(eltOrig, "data-col") || "0";
        var colAttr = getAttributeValue(eltDest, "data-setcol");
        var arrColAttr = colAttr.split('|');
        if (arrColAttr.length == 2) {

            colId = arrColAttr[1] || "0";
            if (origCol != colId) {
                var fld = new fldUpdEngine(arrColAttr[0]);
                fld.newValue = colId;
                oEngine.AddOrSetField(fld);
            }

        }
        // Nouvelle ligne de couloir
        var origSl = getAttributeValue(eltOrig, "data-sl") || "0";
        var slAttr = getAttributeValue(eltDest, "data-setsl");
        var arrSlAttr = slAttr.split('|');
        if (arrSlAttr.length == 2 && arrSlAttr[1] != "-1") {

            var newVal = arrSlAttr[1] || "0";
            if (origSl != newVal) {
                var fld = new fldUpdEngine(arrSlAttr[0]);
                fld.newValue = arrSlAttr[1];
                oEngine.AddOrSetField(fld);
            }
            
        }

        //eTools.setWidgetWait(_nWid, true);

        oEngine.ErrorCallbackFunction = function (oRes) {
            // TODO: Il faudrait idéalement remettre la fiche où elle était :(
            // Refresh pour l'instant (voir eEngine.js - ErrorUpdateTreatmentReturn)
            // Tag la carte mise à jour
            addClass(draggedElt, "error");

        };

        oEngine.SuccessCallbackFunction = function (oRes) {
            // Tag la carte mise à jour
            addClass(draggedElt, "updated");
            var timer = setTimeout(function () {
                removeClass(draggedElt, "updated");
            }, 1000);
         

            // Mise à jour des aggrégats si pas de refresh général
            if (colId && !_reloaded)
                updateAggregates(colId, origCol);
            
        }

        try {
            oEngine.UpdateLaunch();
        }
        catch (ex) {
            console.log(ex);
        }
        

    }

    // Mise à jour des aggrégats pour une colonne
    function updateAggregates(colId, origColId) {

        var oUpdater = new eUpdater("mgr/widget/eKanbanWidgetManager.ashx", 1);

        oUpdater.addParam("action", 1, "post");
        oUpdater.addParam("wid", _nWid, "post");
        oUpdater.addParam("colid", colId, "post");
        oUpdater.addParam("origcolid", origColId, "post");
        oUpdater.addParam("context", _context, "post");

        oUpdater.ErrorCallBack = function (oRes) { };
        oUpdater.asyncFlag = true;

        oUpdater.send(function (oRes) {

            var id, sHtml;
            var objList = JSON.parse(oRes);
            for (var i = 0; i < objList.length; i++) {
                id = objList[i].Id;
                sHtml = "";
                var aggr = document.querySelector(".kbHeaderCol[data-id='" + id + "'] .kbAggr");
                if (aggr) {
                    for (var j = 0; j < objList[i].AggrData.length; j++) {
                        sHtml += "<li>" + objList[i].AggrData[j].Result + "</li>";
                    }
                    aggr.innerHTML = sHtml;
                }
            }
            
            
        });
    }

    // Redimensionnement des colonnes suivant la taille du widget
    function resize(windowWidth, windowHeight) {

        var kbCols = document.querySelectorAll(".kbHeaderCol");

        var totalWidth = windowWidth - _widgetMargins; // Marges diverses


        // Largeur de colonne
        var nbCols = kbCols.length || 1;
        var colWidth = totalWidth / nbCols - 20; // px de sécurité

        if (_debugMode) {
            console.log("windowWidth : " + windowWidth);
            console.log("windowHeight : " + windowHeight);
            console.log("totalWidth : " + totalWidth);
            console.log("colWidth : " + colWidth);
        }


        if (colWidth < _columMinWidth) {
            colWidth = _columMinWidth;
            totalWidth = _widgetMargins + nbCols * (colWidth) + 50;
        }

        var contentWrapper = document.getElementById("contentWrapper");
        if (contentWrapper)
            contentWrapper.style.width = (totalWidth) + "px";

        // Redimensionnement des colonnes => Pas besoin : fait en CSS3 avec flex
        //kbCols = document.querySelectorAll(".kbHeaderCol, .kbCol");
        //for (var i = 0; i < kbCols.length; i++) {
        //    kbCols[i].style.width = (colWidth) + "px"; 
        //}

        // Redimensionnement hauteur
        var kbContent = document.getElementById("kbContent");
        if (kbContent) {
            var headerHeight = 70; // 70 pour l'entête
            var colHeader = document.getElementById("kbColHeader");
            if (colHeader) {
                headerHeight = headerHeight + colHeader.offsetHeight;
                kbContent.style.height = (windowHeight - 40 - headerHeight) + "px"; // 40 pour le padding de kbContent
            }
            else {
                // Cas de l'affichage de l'erreur
                kbContent.style.height = (windowHeight - 50) + "px"; 
            }


            
        }
            
    }

    // Rafraîchissement du Kanban
    function refresh() {

        _reloaded = true;

        _scrollTop = document.getElementById("kbContent").scrollTop;

        var oUpdater = new eUpdater("mgr/widget/eKanbanWidgetManager.ashx", 1);

        oUpdater.addParam("action", 0, "post");
        oUpdater.addParam("wid", _nWid, "post");
        oUpdater.addParam("sldid", _slDescid, "post");
        oUpdater.addParam("slisgroup", (_slIsGroup ? "1" : "0"), "post");
        oUpdater.addParam("context", _context, "post");

        oUpdater.ErrorCallBack = function (oRes) { };
        oUpdater.asyncFlag = true;

        eTools.setWidgetWait(_nWid, true);
        oUpdater.send(function (oRes) {
            document.getElementById("contentWrapper").innerHTML = oRes;

            init();

            // Scroll
            // SetTimeout car sinon c'est trop rapide et le document n'a pas le temps d'être chargé
            setTimeout(function() {
                kbContent.scrollTop = _scrollTop;
            }, 100);

            eTools.setWidgetWait(_nWid, false);

            
        });
    }

    // Sélection d'une ligne de couloir
    function selectSL(descid, bGroup) {
        _slDescid = descid;
        _slIsGroup = bGroup;
        refresh();
    }

    // Edition d'une rubrique : modification de la valeur
    function editField(fld) {

        if (fld) {
            var element = document.querySelector(".kanbanCard[data-fid='" + fld.fid + "'] .fieldValue[data-did='" + fld.descId + "']");
            if (element) {
                var label = fld.newLabel;

                // Pour les cases à cocher : Oui/Non
                if (fld.format == "3" || fld.format == "25")
                    label = (fld.newLabel == "1") ? top._res_58 : top._res_59;

                element.innerHTML = label;


                // Si la valeur est vide, on cache la ligne
                var field = findUp(element, "DIV");
                if (field) {
                    field.style.display = (!fld.newLabel) ? "none" : "";
                }

                
            }
        }
        
    }

    // Check si on doit recharger le kanban à cause des règles
    function reloadWithRule(updatedDescidRuleList) {

        var bReload = false;

        if (updatedDescidRuleList && updatedDescidRuleList.length > 0) {
            // On doit recharger le kanban si les descid en paramètres sont affichés
            // Similaire à ReloadList(lstDescIdRuleUpdated) dans eList.js

            var card = document.querySelector(".kanbanCard");
            if (card) {
                var fields = card.querySelectorAll(".cardField[data-did]");
                for (var i = 0; i < fields.length; i++) {
                    if (updatedDescidRuleList.indexOf(getAttributeValue(fields[i], "data-did")) > -1) {
                        bReload = true;
                        break;
                    }
                }
            }

        }

        if (!bReload)
            return false;

        refresh();
        bReload = true;
        return bReload;
    }


    return {
        "init": function () {
            init();
        },
        "update": function (element, fileId) {
            updateCard(element, fileId);
        },
        "resizeColumns": function (windowWidth, windowHeight) {
            resize(windowWidth, windowHeight);
        },
        "selectSwimlame": function (descid) {
            selectSL(descid);
        },
        "reload": function () {
            refresh();
        },
        "reloadWithRule": function (updatedDescidRuleList) {
            return reloadWithRule(updatedDescidRuleList);
        },
        "editField": function (fldEngine) {
            editField(fldEngine);
        },
        "setWait": function (bSet) {
            eTools.setWidgetWait(_nWid, bSet);
        }
    }
}