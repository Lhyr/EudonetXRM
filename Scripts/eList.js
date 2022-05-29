var debugModeList = false; // mettre à true pour empêcher la disparition de certains éléments (menus, VCard) si la souris n'est plus dessus et permettre le debug de leur contenu sous Firebug ou autre

var modalListCol;
var modalListTab;
var expressFilter;
var contextMenu;
var modalUserCat;

// Utile pour le retour de l'Engine
var modeList = false;
var targetCol = null;
var TYP_BIT = 3;        //	'Logique
var TYP_BITBUTTON = 25; // Bouton logique


///Function d'initialisation de la liste des PJ
/// doit être appelé chaque fois que la liste est rechargée (en ajax)
function initPjList() {
    adjustLastCol(TAB_PJ);
}

var eMFEObject = null;


function initMarkedFiles() {
    try {
        eMFEObject = new eMarkedFileEditor('eMFEObject');
    }
    catch (e) {
        alert('Erreur : initEditors - eMFEObject (eMarkedFileEditor)');
    }
}

// Modal dialog
var oModInfos;
// Objet eMemoEditor tampon pour effectuer la sauvegarde du contenu après fermeture de la boîte de dialogue
var oMemoInfos;
// Dernier objet appelant qui a ouvert la popup Informations
var oInfosCaller;
/*
Affichage de la popup Informations (champ Mémo)
obj : objet appelant qui a ouvert la popup
nFileId : id de la fiche appelante
*/
function shi(obj, nFileId) {
    oInfosCaller = obj;

    if (typeof (nFileId) == 'undefined' || nFileId <= 0)
        nFileId = obj.getAttribute("lnkid");
    if (typeof (nFileId) == 'undefined' || nFileId <= 0)
        nFileId = obj.getAttribute("dbv");
    if (typeof (nFileId) == 'undefined' || nFileId <= 0)
        return;

    var lWidth = document.body.scrollWidth - 150;
    var lHeight = document.body.scrollHeight - 150;

    var nDescId = getAttributeValue(obj, "did");
    var title = getAttributeValue(obj, "title");

    oModInfos = new eModalDialog(
        title,            // Titre
        0,                          // Type
        "eMemoDialog.aspx",         // URL
        lWidth,                     // Largeur
        lHeight);                   // Hauteur

    // Ajustement de la taille du champ en fonction de sa nature Texte brut/HTML
    var bIsHTML = (getAttributeValue(obj, "html") == "1");
    if (!bIsHTML) {
        lWidth = lWidth - 12;
        lHeight = lHeight + 55;
    }

    // Instanciation d'un objet eMemoEditor "interne", "virtuel" (sans affichage de champ Mémo sur la page HTML) pour la sauvegarde de la valeur en base
    oMemoInfos = new eMemoEditor(
        "eMEInfos_" + nDescId, bIsHTML, null, null, "", true, "oMemoInfos"
    );

    oMemoInfos.descId = nDescId;
    oMemoInfos.fileId = nFileId;

    // On lie le champ Mémo à la boîte de dialogue pour qu'on puisse déclencher sa fermeture avec les fonctions cancelMemoDialog/validateMemoDialog
    oMemoInfos.childDialog = oModInfos;
    // On indique que la mise à jour en base est autorisée à partir de la valeur qui sera saisie dans la fenêtre
    oMemoInfos.uaoz = true;

    oModInfos.ErrorCallBack = launchInContext(oModInfos, oModInfos.hide);
    oModInfos.addParam("DescId", nDescId, "post");
    oModInfos.addParam("TabId", nGlobalActiveTab, "post");
    oModInfos.addParam("FileId", nFileId, "post");
    oModInfos.addParam("Title", getAttributeValue(obj, "name"), "post");
    oModInfos.addParam("ParentFrameId", "", "post");
    oModInfos.addParam("EditorJsVarName", "oMemoInfos", "post");
    // la valeur initiale du champ Mémo sera récupérée par eMemoDialog.aspx.cs via GetFieldManager
    oModInfos.addParam("width", lWidth - 12, "post"); // 12 : marge intérieure par rapport au conteneur de l'eModalDialog
    oModInfos.addParam("height", lHeight - 150, "post"); // 150 : espace réservé à la barre de titre + boutons de l'eModalDialog
    oModInfos.addParam("IsHTML", (bIsHTML ? "1" : "0"), "post");

    oModInfos.show();

    oModInfos.addButton(top._res_29, cancelInfosDialog, "button-gray", "oMemoInfos"); // Annuler
    oModInfos.addButton(top._res_28, validateInfosDialog, "button-green", "oMemoInfos"); // Valider
}

function cancelInfosDialog(jsVarName) {
    if (typeof (cancelMemoDialog) == "function")
        cancelMemoDialog(jsVarName)
    else
        oModInfos.hide();
}

function validateInfosDialog(jsVarName) {
    if (typeof (validateMemoDialog) == "function")
        validateMemoDialog(jsVarName)
    else
        oModInfos.hide();
}

/*********************************************************/
/** GESTION RESIZE ET MOVE DES COLONNES                 **/
/*********************************************************/

// Initialise l'event click sur les cellules
function initHeadEvents() {
    thtabevt.init();
}


// Here's the notice from Mike Hall's draggable script:
//*****************************************************************************
//dragtable v1.0
//June 26, 2008
//Dan Vanderkam, http://danvk.org/dragtable/
//http://code.google.com/p/dragtable/

//This is code was based on:
//- Stuart Langridge's SortTable (kryogenix.org/code/browser/sorttable)
//- Mike Hall's draggable class (http://www.brainjar.com/dhtml/drag/)
//- A discussion of permuting table columns on comp.lang.javascript

//Licensed under the MIT license.

// Copyright 2001 by Mike Hall.
// See http://www.brainjar.com for terms of use.
//*****************************************************************************

var LIST = 0;
var LIST_BKM = 1;
var LIST_SEL = 2;
var LIST_SEL_FILTERED = 3;
var LIST_FINDER = 4;
var LIST_WIDGET = 5; // widget mode liste
thtabevt = {
    // VARIABLES PARAMETRABLES
    // How far should the mouse move before it's considered a drag, not a click?
    // Distance parcouru par le curseur nécessaire au déclenchement du drag
    dragRadius: 100, 	// valeur par defaut 10 pixels
    setMinDragDistance: function (x) {
        thtabevt.dragRadius = x * x;
    },

    // Largueur de la ligne de séparation de colonne au moment du retaille de la colonne
    colLineWidth: 1, 	// valeur par defaut 1 pixel
    setColLineWidth: function (x) {
        thtabevt.colLineWidth = x;
    },

    // Detect all draggable tables and attach handlers to their headers.
    init: function () {
        // Don't initialize twice
        if (arguments.callee.done) return;
        arguments.callee.done = true;
        if (!document.createElement || !document.getElementsByTagName) return;

        thtabevt.eventsObj.zIndex = 0;
        thtabevt.browser = new getBrowser();
    },



    // Indicateur de resize pour ne pas declencher le drag
    resizeOn: false,
    // Indicateur si il y a-++ eu un move sur le resize pour ne pas le declencher lors du resizeAuto
    resizeMoveOn: false,

    // Global object to hold drag information.
    eventsObj: new Object(),
    // Objet contenant les informations sur le browser
    browser: null,

    // DEBUT EVENT DE DRAG
    dragStart: function (event) {

        if (thtabevt.resizeOn)
            return;

        var brow = thtabevt.browser;
        var evtObj = thtabevt.eventsObj;

        if (brow != null && brow.isIE)
            var origNode = window.event.srcElement;
        else
            var origNode = event.target;

        // Get cursor position with respect to the page.
        var pos = thtabevt.eventPosition(event);
        // Recherche le TH (colonne d'en-tête)
        var col = thtabevt.findUp(origNode, "TH");
        evtObj.origNodeTh = col;
        // Recherche la table globale
        var table = thtabevt.findUp(col, "TABLE");
        evtObj.origNodeTable = table;

        if (col == null || table == null)
            return;

        // Recherche l'index du TH (colonne d'en-tête)
        evtObj.origNodeThIdx = thtabevt.findColumn(table, pos.x);
        if (evtObj.origNodeThIdx == -1) return;

        thtabevt.headActiv(true);

        // Récupération du guide de déplacement de colonne
        evtObj.elementGuid = document.getElementById("colGuide");
        evtObj.elementGuid.style.display = "none";

        // Safari doesn't support table.tHead, sigh
        if (table.tHead == null)
            table.tHead = table.getElementsByTagName('thead')[0];

        ////////////////////////////////////////////////////////////////
        // Création du nouveau élément HTML
        var new_elt = thtabevt.fullCopy(table, false);
        new_elt.id += "clone";
        new_elt.style.margin = '0';
        ////////////////////////////////////////////////////////////////

        // First the heading
        if (table.tHead) {
            new_elt.appendChild(thtabevt.copySectionColumn(table.tHead, evtObj.origNodeThIdx));
        }
        forEach(table.tBodies, function (tb) {
            new_elt.appendChild(thtabevt.copySectionColumn(tb, evtObj.origNodeThIdx));
        });
        if (table.tFoot) {
            new_elt.appendChild(thtabevt.copySectionColumn(table.tFoot, evtObj.origNodeThIdx));
        }
        ////////////////////////////////////////////////////////////////

        var obj_pos = thtabevt.absolutePosition(col);
        new_elt.style.position = "fixed";
        new_elt.style.left = obj_pos.x + "px";
        new_elt.style.top = obj_pos.y + "px";
        new_elt.style.width = obj_pos.w + "px";
        new_elt.style.height = obj_pos.h + "px";
        new_elt.style.opacity = 0.7;

        // Indicateur si notre nouvelle élément a été ajouté au conteneur de la table
        evtObj.addedNode = false;
        // Conteneur de la table, emplacement ou l'on va insérer notre nouveau élément HTML
        evtObj.tableContainer = table.parentNode || document.body;
        // Enregistre notre nouveau élément en mémoire de notre objet tabevt
        evtObj.elNode = new_elt;
        // Enregistre les positions de départ du curseur en mémoire de notre objet tabevt
        evtObj.cursorStartX = pos.x;
        evtObj.cursorStartY = pos.y;
        evtObj.elStartLeft = parseInt(evtObj.elNode.style.left, 10);
        evtObj.elStartTop = parseInt(evtObj.elNode.style.top, 10);

        if (isNaN(evtObj.elStartLeft)) evtObj.elStartLeft = 0;
        if (isNaN(evtObj.elStartTop)) evtObj.elStartTop = 0;

        // Passe notre nouveau élément HTML devant
        evtObj.elNode.style.zIndex = ++evtObj.zIndex;

        // Capture mousemove and mouseup events on the (TODO : page or table ?)
        setEventListener(document, 'mousemove', thtabevt.dragMove, true);
        setEventListener(document, 'mouseup', thtabevt.dragEnd, true);

        thtabevt.stopEvent(event);
    },

    dragMove: function (event) {
        var evtObj = thtabevt.eventsObj;

        // Get cursor position with respect to the page.
        var pos = thtabevt.eventPosition(event);

        // Différences entre les coordonnées de départ et d'arrivé du curseur
        var dx = evtObj.cursorStartX - pos.x;
        var dy = evtObj.cursorStartY - pos.y;

        // Ajout notre nouveau élément
        if (!evtObj.addedNode && dx * dx + dy * dy > thtabevt.dragRadius) {
            evtObj.tableContainer.insertBefore(evtObj.elNode, evtObj.origNodeTable);
            evtObj.addedNode = true;
        }

        // On déplace le guide en fonction de l'élément ciblé.
        if (evtObj.elementGuid != null) {
            var drawGuid = true;

            // Determine whether the drag ended over the table, and over which column.
            var table_pos = thtabevt.absolutePosition(evtObj.origNodeTable);
            if (pos.y < table_pos.y || pos.y > table_pos.y + table_pos.h)
                drawGuid = false;
            if (pos.x < table_pos.x || pos.x > table_pos.x + table_pos.w)
                drawGuid = false;

            if (!drawGuid) {
                evtObj.elementGuid.style.display = "none";
            } else {
                var targetColIdx = thtabevt.findColumn(evtObj.origNodeTable, pos.x);

                if (targetColIdx != -1) {
                    // On exclu les colonnes d'actions, d'icones et la première colonne non déplaçable
                    var targetCol = evtObj.origNodeTable.tHead.rows[0].cells[targetColIdx];

                    var targetColPos = thtabevt.absolutePosition(targetCol);
                    if (targetColPos != null && targetCol.getAttribute("nomove") != "1") {
                        evtObj.elementGuid.style.display = "block";

                        if (targetColIdx <= evtObj.origNodeThIdx)
                            evtObj.elementGuid.style.left = targetColPos.x + "px";
                        else
                            evtObj.elementGuid.style.left = (targetColPos.x + targetColPos.w) + "px";
                    } else {
                        evtObj.elementGuid.style.display = "none";
                    }
                }
            }
        }

        // Move drag element by the same amount the cursor has moved.
        var style = evtObj.elNode.style;
        style.left = (evtObj.elStartLeft + pos.x - evtObj.cursorStartX) + "px";
        style.top = (evtObj.elStartTop + pos.y - evtObj.cursorStartY) + "px";

        thtabevt.stopEvent(event);
    },

    dragEnd: function (event) {
        var brow = thtabevt.browser;

        // Stop capturing mousemove and mouseup events.
        unsetEventListener(document, 'mousemove', thtabevt.dragMove, true);
        unsetEventListener(document, 'mouseup', thtabevt.dragEnd, true);

        thtabevt.headActiv(false);

        // Récupération de l'élément concerné par le drag & drop
        var evtObj = thtabevt.eventsObj;

        // Masquage de la flèche matérialisant le déplacement
        evtObj.elementGuid.style.display = "none";
        evtObj.elementGuid = null;

        // Si notre élément n'a pas été ajouté au conteneur de la table, on ne fait rien
        if (!evtObj.addedNode) {
            return;
        }

        // Suppression de notre élément
        evtObj.tableContainer.removeChild(evtObj.elNode);

        // Get cursor position with respect to the page.
        var pos = thtabevt.eventPosition(event);

        // Determine whether the drag ended over the table, and over which column.
        var table_pos = thtabevt.absolutePosition(evtObj.origNodeTable);
        if (pos.y < table_pos.y || pos.y > table_pos.y + table_pos.h) {
            return;
        }
        if (pos.x < table_pos.x || pos.x > table_pos.x + table_pos.w) {
            return;
        }

        var targetColIdx = thtabevt.findColumn(evtObj.origNodeTable, pos.x);
        if (targetColIdx != -1 && targetColIdx != evtObj.origNodeThIdx) {
            // On exclu les colonnes d'actions, d'icones et la première colonne non déplaçable
            var targetCol = evtObj.origNodeTable.tHead.rows[0].cells[targetColIdx];
            if (targetCol.getAttribute("nomove") == "1")
                return;

            thtabevt.moveColumn(evtObj.origNodeTable, evtObj.origNodeThIdx, targetColIdx);
        }

        thtabevt.endAction();
    },
    // FIN EVENT DE DRAG

    // DEBUT EVENT DE RESIZE
    resizeStart: function (event) {
        thtabevt.resizeOn = true;

        var brow = thtabevt.browser;
        var evtObj = thtabevt.eventsObj;
        if (!brow)
            return;
        if (brow.isIE)
            evtObj.origNodeResize = window.event.srcElement;
        else
            evtObj.origNodeResize = event.target;

        // Get cursor position with respect to the page.
        var pos = thtabevt.eventPosition(event);

        // Recherche le TH (colonne d'en-tête)
        var col = thtabevt.findUp(evtObj.origNodeResize, "TH");
        evtObj.origNodeTh = col;
        // Recherche la table globale
        var table = thtabevt.findUp(col, "TABLE");
        evtObj.origNodeTable = table;

        if (col == null || table == null)
            return;

        // Conteneur de la table, emplacement ou l'on va insérer notre nouveau élément HTML
        evtObj.tableContainer = table.parentNode || document.body;

        thtabevt.headActiv(true);

        // Safari doesn't support table.tHead, sigh
        if (table.tHead == null)
            table.tHead = table.getElementsByTagName('thead')[0];

        // Enregistre les positions de départ du curseur en mémoire de notre objet tabevt
        evtObj.cursorStartX = pos.x;
        evtObj.cursorStartY = pos.y;

        // Affiche la ligne de séparation
        if (table.tBodies.length > 0) {
            if (evtObj.lineNode == null) {
                var oDiv = document.createElement("div");
                evtObj.lineNode = oDiv;

                //thtabevt.setColLineWidth(10); // Pour rendre la colonne plus large

                var tableFirstBody = table.tBodies[0]
                var ndTabBody_pos = thtabevt.absolutePosition(tableFirstBody, true);

                // Données fixe de la ligne de séparation
                oDiv.id = "idDivColLine";
                oDiv.style.position = "absolute";
                oDiv.style.left = -1;       // Par défaut
                oDiv.style.width = thtabevt.colLineWidth + "px";
                oDiv.style.top = ndTabBody_pos.y + "px";
                oDiv.style.height = ndTabBody_pos.h + "px";
                oDiv.style.backgroundColor = '#DFDFDE';
                oDiv.style.opacity = 0.7;

                evtObj.tableContainer.appendChild(oDiv);
            }

            // Position de la ligne en fonction de la colonne
            var ndResize_pos = thtabevt.absolutePosition(evtObj.origNodeResize, true);
            // Position en X du TD de resize de colonne + le width de celle-ci - la taille de la futur ligne de séparation
            evtObj.lineNode.style.left = (ndResize_pos.x + parseInt(ndResize_pos.w) - thtabevt.colLineWidth) + "px";

            // Fait apparaitre la ligne
            evtObj.lineNode.style.visibility = 'visible';
            evtObj.lineNode.style.display = 'block';
        }

        // Capture mousemove and mouseup events on the (TODO : page or table ?)
        setEventListener(document, 'mousemove', thtabevt.resizeMove, true);
        setEventListener(document, 'mouseup', thtabevt.resizeEnd, true);

        thtabevt.stopEvent(event);
    },

    resizeMove: function (event) {
        thtabevt.resizeMoveOn = true;

        var evtObj = thtabevt.eventsObj;

        // Get cursor position with respect to the page.
        var pos = thtabevt.eventPosition(event);

        // Différences à ajouter au width de notre colonne
        var dx = pos.x - evtObj.cursorStartX;
        var newWidth = parseInt(evtObj.origNodeTh.width) + parseInt(dx);

        var mustEndResize = false;
        // -----------------------------------
        // Calcul de la taille minimum requise
        var fldAlias = evtObj.origNodeTh.id;
        var colHeadMaxSize = document.getElementById('LENH_' + fldAlias);

        // Recup de la nouvelle taille
        var minWidth = parseInt(colHeadMaxSize.offsetWidth);
        if (newWidth < minWidth) {
            mustEndResize = true;
            newWidth = minWidth;
        }

        // Taille mini defini par l'appli - compare avec la constante defini dans eConst
        var nMinWidthDef = getNumber(document.getElementById("minColWidth").value);
        if (newWidth < nMinWidthDef && minWidth > nMinWidthDef) {
            mustEndResize = true;
            newWidth = nMinWidthDef;
        }

        //ASY [BUG # 29409] - Taille max des colonnes - compare avec la constante defini dans eConst
        var nMaxWidthDef = getNumber(document.getElementById("maxColWidth").value);
        if (newWidth > nMaxWidthDef) {
            mustEndResize = true;
            newWidth = nMaxWidthDef;
        }
        // -----------------------------------

        // Resize column
        evtObj.origNodeTh.width = newWidth + "px";
        evtObj.origNodeTh.style.width = newWidth + "px";

        // Deplace la ligne de séparation
        if (evtObj.lineNode != null) {
            var ndResize_pos = thtabevt.absolutePosition(evtObj.origNodeResize, true);
            // Position en X du TD de resize de colonne + le width de celle-ci - la taille de la futur ligne de séparation
            evtObj.lineNode.style.left = (ndResize_pos.x + parseInt(ndResize_pos.w) - thtabevt.colLineWidth) + "px";
        }

        // Enregistre les positions de départ du curseur en mémoire de notre objet tabevt
        evtObj.cursorStartX = pos.x;

        // Affiche la taille choisi
        st(event, newWidth + "px", "divShowColResize");

        var descid = evtObj.origNodeTh.getAttribute("did");
        //Ajustement des colonnes dont les champs contiennent des boutons d'actions et dont le libellé est contenu dans une div intermédiaire
        if (getNumber(descid) % 100 != 1) {
            adjustBtnFieldWidth(evtObj.origNodeTh);
        }

        if (mustEndResize) {
            thtabevt.resizeEnd(event);
        }

        thtabevt.stopEvent(event);
    },

    resizeEnd: function (event) {
        if (!thtabevt.resizeOn)
            return;
        thtabevt.resizeOn = false;

        var brow = thtabevt.browser;
        var evtObj = thtabevt.eventsObj;

        // Stop capturing mousemove and mouseup events.
        unsetEventListener(document, 'mousemove', thtabevt.resizeMove, true);
        unsetEventListener(document, 'mouseup', thtabevt.resizeEnd, true);

        // Fait disparaitre la ligne de separation
        if (evtObj.lineNode != null) {
            evtObj.lineNode.style.visibility = 'hidden';
            evtObj.lineNode.style.display = 'none';
        }

        thtabevt.headActiv(false);
        ht(document);

        if (!thtabevt.resizeMoveOn)
            return;
        thtabevt.resizeMoveOn = false;

        // MISE A JOUR EN BDD
        var oCol = evtObj.origNodeTh;
        var descid = oCol.getAttribute("did");
        var nNewWidth = parseInt(oCol.width);

        var listMode = LIST;

        if (evtObj.origNodeTable.getAttribute("ednmode") == "bkm")
            listMode = LIST_BKM;

        if (evtObj.origNodeTable.getAttribute("ednmode") == "sel")
            listMode = LIST_SEL;

        if (evtObj.origNodeTable.getAttribute("ednmode") == "finderlist")
            listMode = LIST_FINDER;

        if (evtObj.origNodeTable.getAttribute("ednmode") == "listwidget")
            listMode = LIST_WIDGET;

        if (!oCol.getAttribute("nomoveupdt"))
            thtabevt.updBase("listwidth=" + descid + ';' + nNewWidth, listMode);

        thtabevt.endAction();
    },
    // FIN EVENT RESIZE

    resizeLastMax: function (event, targetElement, oDivMain) {

        var paramWin = top.getParamWindow();
        var objThm = JSON.parse(paramWin.GetParam("currenttheme").toString());

        var brow = thtabevt.browser;
        var evtObj = thtabevt.eventsObj;
        if (!oDivMain || typeof (oDivMain) == "undefined")
            oDivMain = document.getElementById("mainDiv");
        if (!oDivMain || typeof (oDivMain) == "undefined")
            oDivMain = document.getElementById("content");
        var origNode = null;

        if (targetElement) {
            origNode = targetElement;
        }
        else if (brow.isIE && window.event && window.event.srcElement) {
            origNode = window.event.srcElement;
        }
        else {
            if (typeof (event) != 'undefined' && event && event.target)
                origNode = event.target;
        }

        if (!origNode) {
            thtabevt.stopEvent(event);
            return;
        }

        // Recherche le TH (colonne d'en-tête)
        var col = thtabevt.findUp(origNode, "TH");
        evtObj.origNodeTh = col;
        // Recherche la table globale
        var table = thtabevt.findUp(col, "TABLE");
        evtObj.origNodeTable = table;
        if (!table)
            return;
        var idTable = table.id.split('_')[1];
        var maxSizeElem = oDivMain.offsetWidth;
        if (col == null || table == null)
            return;
        var fldAlias = col.id.replace("COL_", "");

        var posLastCol = getAbsolutePosition(col, true);

        var posDivMainCol = getAbsolutePosition(oDivMain, true);

        if (posLastCol.x + posLastCol.w > posDivMainCol.x + maxSizeElem)
            return;

        // Cas des Bookmarks
        var oDivBkms = document.getElementById("divBkmPres");
        var fileDiv = document.querySelector(".fileDiv");
        var adjust = 20;
        /* ELAIZ - demande 76124 - Ajout d'une marge plus importante (41px) pour éviter 
        l'ascenseur horizontal dans le nouveau thème */
        if (fileDiv && objThm.Version == 2)
            adjust = 45;
        if (oDivBkms) {
            if (oDivBkms.scrollHeight > oDivBkms.clientHeight) {
                //Il y a un scroll vertical, il faut ajuster la taille de la colonne pour ne pas
                // provoquer un scroll horizontal sur les signets.s
                adjust += 20;
            }
            /* ELAIZ - demande 76124 - Ajout d'une marge plus importante (41px) pour éviter
            l'ascenseur horizontal dans le nouveau thème */
            if (fileDiv && fileDiv.scrollHeight > fileDiv.clientHeight && objThm.Version == 2)
                adjust += 20;
        }
        nNewWidth = ((posDivMainCol) ? posDivMainCol.x : 0) + maxSizeElem - posLastCol.x - adjust;  // 10px correspond à l'espace nécessaire pour le pas avoir d'ascenseur vertical
        var nMinWidth = getNumber(document.getElementById("minColWidth").value);      // Taille mini defini par l'appli
        if (nNewWidth < nMinWidth)
            nNewWidth = nMinWidth;

        // Retaille de la colonne
        col.width = nNewWidth;
        col.style.width = nNewWidth + 'px';
        thtabevt.stopEvent(event);
    },

    // DEBUT EVENT DOUBLE CLICK
    resizeAuto: function (event, targetElement) {

        var brow = thtabevt.browser;
        var evtObj = thtabevt.eventsObj;

        var origNode = null;
        if (brow.isIE) {
            if (window.event && window.event.srcElement)
                origNode = window.event.srcElement;
            else
                origNode = targetElement;
        }
        else {
            if (typeof (event) != 'undefined' && event && event.target)
                origNode = event.target;
            else
                origNode = targetElement;
        }

        // Recherche le TH (colonne d'en-tête)
        var col = thtabevt.findUp(origNode, "TH");
        evtObj.origNodeTh = col;
        // Recherche la table globale
        var table = thtabevt.findUp(col, "TABLE");
        evtObj.origNodeTable = table;

        if (col == null || table == null)
            return;

        var fldAlias = col.id;

        // Recup de la nouvelle taille
        var nNewWidth = thtabevt.getColMaxSize(fldAlias);
        if (nNewWidth == 0)
            return;

        nNewWidth += 10;       // 10 espace libre supplémentaire

        //Si on est dans une colonne de type email on rajoute la largeur du bouton
        //#59 789 : valable pour PHONE également si SMS
        var oRuleLine = getCssSelector("customCss", ".divct_" + fldAlias);
        var oRuleBtn = getCssSelector("eList.css", ".btn");

        if (oRuleLine && oRuleBtn) {
            nNewWidth += getNumber(oRuleBtn.style.width.replace("px", ""));
        }


        var nMinWidth = getNumber(document.getElementById("minColWidth").value);      // Taille mini defini par l'appli
        if (nNewWidth < nMinWidth)
            nNewWidth = nMinWidth;

        // Retaille de la colonne
        col.width = nNewWidth;
        col.style.width = nNewWidth + "px";


        // MISE A JOUR EN BDD
        var descid = col.getAttribute("did");
        nNewWidth = parseInt(col.width);

        var listMode = LIST;

        if (evtObj.origNodeTable.getAttribute("ednmode") == "bkm")
            listMode = LIST_BKM;

        if (evtObj.origNodeTable.getAttribute("ednmode") == "sel")
            listMode = LIST_SEL;

        if (evtObj.origNodeTable.getAttribute("ednmode") == "selection")
            listMode = LIST_SEL_FILTERED;

        if (evtObj.origNodeTable.getAttribute("ednmode") == "finderlist")
            listMode = LIST_FINDER;

        if (evtObj.origNodeTable.getAttribute("ednmode") == "listwidget")
            listMode = LIST_WIDGET;

        // NBA et HLA 29-11-2012
        // Modifications apportées pour ne pas sauvegarder en base pour la liste de pj en mode TPL
        if (!col.getAttribute("nomoveupdt"))
            thtabevt.updBase("listwidth=" + descid + ';' + nNewWidth, listMode);

        //Ajustement des colonnes dont les champs contiennent des boutons d'actions et dont le libellé est contenu dans une div intermédiaire        
        if (getNumber(descid) % 100 != 1) {
            adjustBtnFieldWidth(col);
        }

        thtabevt.stopEvent(event);
    },
    // FIN EVENT DOUBLE CLICK

    // DEBUT EVENT SORT
    sortList: function (event, forceOrder) {
        var brow = thtabevt.browser;
        var evtObj = thtabevt.eventsObj;

        if (brow.isIE)
            var origNode = window.event.srcElement;
        else
            var origNode = event.target;

        // Recherche le TH (colonne d'en-tête)
        var sortCol = thtabevt.findUp(origNode, "TH");
        evtObj.origNodeTh = sortCol;
        // Recherche la table globale
        var table = thtabevt.findUp(sortCol, "TABLE");
        evtObj.origNodeTable = table;

        if (sortCol == null || table == null)
            return;

        var fldAlias = sortCol.id.replace("COL_", "");
        var imgSortDesc = document.getElementById("IMG_SORT_DESC_" + fldAlias);
        var imgSortAsc = document.getElementById("IMG_SORT_ASC_" + fldAlias);

        if (!imgSortDesc || !imgSortAsc)
            return;

        var sortOrder = 0;
        if (forceOrder != null)
            sortOrder = forceOrder;
        else {
            // Tri Asc activé sur la rubrique
            if (imgSortAsc.getAttribute("actif") == "1") {
                sortOrder = 1;
            }
            // Tri Desc activé sur la rubrique
            else if (imgSortDesc.getAttribute("actif") == "1") {
                sortOrder = 0;
            }
            // Sinon Aucun tri activé sur la rubrique ==> on tri Desc
        }

        // MISE A JOUR EN BDD
        var descid = sortCol.getAttribute("did");
        var listMode = LIST;

        if (evtObj.origNodeTable.getAttribute("ednmode") == "bkm")
            listMode = LIST_BKM;

        if (evtObj.origNodeTable.getAttribute("ednmode") == "sel")
            listMode = LIST_SEL;

        if (evtObj.origNodeTable.getAttribute("ednmode") == "selection")
            listMode = LIST_SEL_FILTERED;

        if (evtObj.origNodeTable.getAttribute("ednmode") == "finderlist")
            listMode = LIST_FINDER;

        if (evtObj.origNodeTable.getAttribute("ednmode") == "listwidget")
            listMode = LIST_WIDGET;


        thtabevt.updBase("listsort=" + descid + ";$;listorder=" + sortOrder, listMode, true);
        thtabevt.stopEvent(event);
    },
    // FIN EVENT SORT

    // MODIF RENDU
    headActiv: function (bActivCell) {
        var evtObj = thtabevt.eventsObj;
        chgHeadActiv(evtObj.origNodeTh, evtObj.origNodeResize, bActivCell);
    },

    // MODIF BDD
    updBase: function (addPref, listMode, bReload) {
        if (!addPref)
            return;

        if (!listMode)
            listMode = LIST;

        if (!bReload)
            bReload = false;

        var evtObj = thtabevt.eventsObj;
        var oTable = evtObj.origNodeTable;

        var mainTab = oTable.id.replace("mt_", "");
        var mainField = document.getElementById("mainFld_" + mainTab);
        if (mainField)
            mainField = mainField.value;
        else
            mainField = 0;

        if (mainTab != null) {
            switch (listMode) {
                case LIST:
                    var updatePref = "tab=" + mainTab
                        + ";$;mainFld=" + mainField
                        + ";$;" + addPref;

                    // Callback d'erreur : on reste sur la page
                    updateUserSelection(updatePref, function () { if (bReload) { loadList(); } }, function () { });
                    break;
                case LIST_BKM:
                    var updatePref = "tab=" + nGlobalActiveTab
                        + ";$;bkm=" + mainTab
                        + ";$;mainFld=" + mainField
                        + ";$;" + addPref;

                    updateUserBkmPref(updatePref, function () { if (bReload) { loadBkm(mainTab); } });
                    break;
                case LIST_SEL:
                    addPref = addPref.replace(/list/g, "listsel");
                    var ivtTab = getAttributeValue(oTable, "ivttab");

                    var updatePref = "tab=" + ivtTab
                        + ";$;mainFld=" + mainField
                        + ";$;" + addPref;

                    if (oInvitWizard)
                        updatePref += ";$;deletemode=" + (oInvitWizard.DeleteMode ? "1" : "0") + ";$;targetmode=" + oInvitWizard.Target;

                    // Callback d'erreur : on reste sur la page
                    updateColsPref(updatePref, function () { if (bReload) { UpdatePPList(1); } }, function () { });
                    break;
                case LIST_SEL_FILTERED:
                    addPref = addPref.replace(/list/g, "listsel");
                    var updatePref = "tab=" + mainTab
                        + ";$;mainFld=" + mainField
                        + ";$;" + addPref;

                    if (oInvitWizard)
                        updatePref += ";$;deletemode=" + (oInvitWizard.DeleteMode ? "1" : "0") + ";$;targetmode=" + oInvitWizard.Target;

                    updateColsPref(updatePref, function () { if (bReload) { updateSelectionList(1); } }, function () { });
                    break;
                case LIST_FINDER:
                    var updatePref = "tab=" + mainTab
                        + ";$;" + addPref;

                    updateUserFinderPref(updatePref, function () { if (bReload) { StartSearch(); } }, function () { });
                    break;
                case LIST_WIDGET:
                    var widgetID = "";

                    var oWidget = findUpByClass(oTable, "widget-wrapper");
                    var hidWid = document.getElementById("hidWid");
                    if (hidWid)
                        widgetID = hidWid.value;

                    var updatePref = "tab=" + mainTab
                        + ";$;mainFld=" + mainField
                        + ";$;wid=" + widgetID
                        + ";$;" + addPref;

                    updateListWidgetPref(updatePref, function () {
                        if (bReload) {
                            //if (oWidget && typeof (oAdminWidget) !== 'undefined') {
                            //    oAdminWidget.reload(oWidget);
                            //}
                            if (oListWidget)
                                oListWidget.refresh();
                        }
                    }, function () { });

                    break;
            }
        }
    },

    // OUTILS
    getColMaxSize: function (colFldAlias) {
        var eleMaxSize = document.getElementById('LEN_' + colFldAlias);
        var eleHeadMaxSize = document.getElementById('LENH_' + colFldAlias);

        var maxSizeA = eleMaxSize == null ? 0 : parseInt(eleMaxSize.offsetWidth);
        var maxSizeB = eleHeadMaxSize == null ? 0 : parseInt(eleHeadMaxSize.offsetWidth);

        return maxSizeA > maxSizeB ? maxSizeA : maxSizeB;
    },

    // Climb up the DOM until there's a tag that matches.
    findUp: function (elt, tag) {
        if (elt == null)
            return null;

        do {
            if (elt.nodeName && elt.nodeName.search(tag) != -1)
                return elt;
        } while (elt = elt.parentNode);

        return null;
    },

    // Position du curseur
    eventPosition: function (event) {
        var x, y;
        if (thtabevt.browser && thtabevt.browser.isIE) {
            x = window.event.clientX + document.documentElement.scrollLeft + document.body.scrollLeft;
            y = window.event.clientY + document.documentElement.scrollTop + document.body.scrollTop;
            return { x: x, y: y };
        }
        return { x: event.pageX, y: event.pageY };
    },

    // Determine the position of this element on the page. Many thanks to Magnus
    // Kristiansen for help making this work with "position: fixed" elements.
    // SPH - Pour pouvoir utiliser absolutePosition hors du mode fiche, déplacement de la function dans eTools.js
    absolutePosition: function (elt, stopAtRelative) {
        return getAbsolutePosition(elt, stopAtRelative);
    },

    // Clone an element, copying its style and class.
    fullCopy: function (elt, deep) {
        if (elt == null)
            return;
        var new_elt = elt.cloneNode(deep);
        new_elt.className = elt.className;
        forEach(elt.style,
            function (value, key, object) {
                if (value == null) return;
                if (typeof (value) == "string" && value.length == 0) return;

                new_elt.style[key] = elt.style[key];
            });
        return new_elt;
    },

    // Copy the entire column
    copySectionColumn: function (sec, col) {
        // Copie de la balise HTML
        var new_sec = thtabevt.fullCopy(sec, false);
        forEach(sec.rows, function (row) {
            var cell = row.cells[col];
            if (cell == undefined)
                return;
            // Copie du TR
            var new_tr = thtabevt.fullCopy(row, false);
            if (row.offsetHeight) new_tr.style.height = row.offsetHeight + "px";
            // Copie du TD et ces enfants
            var new_td = thtabevt.fullCopy(cell, true);
            if (cell.offsetWidth) new_td.style.width = cell.offsetWidth + "px";
            new_tr.appendChild(new_td);
            new_sec.appendChild(new_tr);
        });
        return new_sec;
    },

    // Recherche l'index de colonne sous le curseur
    // Which column does the x value fall inside of? x should include scrollLeft.
    findColumn: function (table, x) {
        var headCells = table.tHead.rows[0].cells;
        for (var i = 0; i < headCells.length; i++) {
            var pos = thtabevt.absolutePosition(headCells[i]);
            if (pos.x <= x && x <= pos.x + pos.w) {
                return i;
            }
        }
        return -1;
    },

    // Move a column of table from start index to finish index.
    // Based on the "Swapping table columns" discussion on comp.lang.javascript.
    // Assumes there are columns at sIdx and fIdx
    moveColumn: function (table, sIdx, fIdx) {
        var row;

        // Modification des données en BDD
        var headCells = table.tHead.rows[0].cells;
        var descIdFrom = headCells[sIdx].getAttribute("did");
        var descIdTo = headCells[fIdx].getAttribute("did");

        var listMode = LIST;

        if (table.getAttribute("ednmode") == "bkm")
            listMode = LIST_BKM;

        if (table.getAttribute("ednmode") == "sel")
            listMode = LIST_SEL;

        if (table.getAttribute("ednmode") == "selection")
            listMode = LIST_FILTERED_SEL;


        // NBA et HLA 29-11-2012
        // Modifications apportées pour ne pas sauvegarder en base pour la liste de pj en mode TPL
        if (!headCells[sIdx].getAttribute("nomoveupdt"))
            thtabevt.updBase("listmove=" + descIdFrom + ';' + descIdTo, listMode);

        // Modification graphique
        var i = table.rows.length;
        while (i--) {
            row = table.rows[i];
            if (sIdx < row.cells.length) {
                var x = row.removeChild(row.cells[sIdx]);
                if (fIdx < row.cells.length) {
                    row.insertBefore(x, row.cells[fIdx]);
                } else {
                    row.appendChild(x);
                }
            }
        }
    },

    endAction: function () {
        var evtObj = thtabevt.eventsObj;

        evtObj.origNodeTable = null;
        evtObj.origNodeTh = null;
        evtObj.origNodeResize = null;
    },

    // Stop propagation
    stopEvent: function (event) {
        if (thtabevt.browser && thtabevt.browser.isIE) {
            if (window.event) {
                window.event.cancelBubble = true;
                window.event.returnValue = false;
            }
        } else {
            if (typeof (event) != 'undefined' && event)
                event.preventDefault();
        }
    }
}

// resizeDoubleClick (resizeAuto)
function rdc(e) {
    thtabevt.resizeAuto(e);
}

// resizeDown (resizeStart)
function rd(e) {
    thtabevt.resizeStart(e);
}

// moveColDown (dragStart)
function mcd(e) {
    thtabevt.dragStart(e);
}

function chgHeadActiv(oCol, oColResize, bActivCell) {
    if (oCol) {
        if (bActivCell)
            addClass(oCol, 'headActif');
        else
            removeClass(oCol, 'headActif');
    }

    if (oColResize) {
        if (bActivCell)
            addClass(oColResize, 'hdResizeActif');
        else
            removeClass(oColResize, 'hdResizeActif');
    }
}

/*********************************************************/
/** FIN GESTION RESIZE ET MOVE DES COLONNES             **/
/*********************************************************/

/****************************************************************/
/** GESTION AFFICHAGE DES ICON D'ENTETE (tri, filtre, calcul)  **/
/****************************************************************/


function mouseWheelEvent(evt) {

    try {
        var sourceObj = evt.srcElement || evt.target;
        var delta = evt.wheelDelta ? evt.wheelDelta : -evt.detail;

        var nTab = sourceObj.id.split("_")[1];


        var myDiv = document.getElementById("div" + nTab)

        if (delta < 0 && oListScrollManager.IsScrollBottom(sourceObj) && oListScrollManager.IsScrollBottom(myDiv)) {
            nextpage(nTab, false, true)
        }

    }
    catch (e) {

    }

}




eListScrollManager = function () {

    that = this;

    this.storedScrolls = new Array(); //dico  
    this.saveScroll = function (evt) {
        var sourceObj = evt.srcElement || evt.target;
        var parts = sourceObj.id.split("_");
        if (parts.length >= 2) {
            var tab = parts[parts.length - 2];
            this.saveScrollPosition(tab);
        }
    };


    this.saveScrollPosition = function (nTab) {

        var containerId = "div" + nTab;
        var cnt = document.getElementById(containerId);
        if (!cnt) {
            containerId = "div" + nGlobalActiveTab;
            cnt = document.getElementById(containerId);
        }
        if (cnt)
            this.storedScrolls[nTab] = {
                id: containerId,
                left: cnt.scrollLeft,
                top: cnt.scrollTop
            };
    };

    that.IsScrollBottom = function scrolled(o) {

        if (o.offsetHeight + o.scrollTop >= (o.scrollHeight)) {
            return true;
        }
        return false;
    };

    this.scroll = function (nTab) {

        var savedScroll = this.storedScrolls[nTab];
        if (savedScroll) {
            var cnt = document.getElementById(savedScroll.id);
            if (cnt) {
                cnt.scrollLeft = savedScroll.left;
                // cnt.scrollTop = savedScroll.top;
            }
        }
    };
    this.clear = function (evt) {

        var sourceObj = evt.srcElement || evt.target;



        var scrollKey = sourceObj.id.replace("div", "");

        if (scrollKey in this.storedScrolls)
            delete this.storedScrolls[scrollKey];
    }
};

oListScrollManager = new eListScrollManager();

function ScrollIntoLastPosition(mainTab) {
    if (oListScrollManager != null && typeof (oListScrollManager) == "object")
        oListScrollManager.scroll(mainTab);
}

function sl(e) {

    thtabevt.sortList(e, null);
    oListScrollManager.saveScroll(e);
}

function sld(e) {

    thtabevt.sortList(e, 1);
    oListScrollManager.saveScroll(e);
}

function sla(e) {

    thtabevt.sortList(e, 0);
    oListScrollManager.saveScroll(e);
}


///Récupère la liste des descid
/// des champs sur lesquels la somme des colonnes est activée pour les mode liste
function updateComputedColList() {


    var oMainTab = document.getElementById("mt_" + nGlobalActiveTab);
    if (!oMainTab)
        return "";


    var oLstCol = oMainTab.querySelectorAll("td.sumColRow[enabled='1']");

    var sLst = "";

    if (oLstCol.length >= 1) {
        for (var nTD = 0; nTD < oLstCol.length; nTD++) {
            var oTD = oLstCol[nTD];
            var sID = oTD.id;

            var aID = sID.split("_");
            var nDescId = aID[aID.length - 1];

            if (sLst != "")
                sLst += ";";

            sLst += nDescId;

        }
    }

    return sLst;

}

//Met à jour les colonnes calculé du signet spécifié (-1 pour tous)
// pour les bookmark
function updateComputedCol(nBkm) {

    //Block fiche
    // s'il n'est pas présent, on n'est pas dans un mode permettant cette fonctionnalité
    var oMainFileDiv = document.getElementById("fileDiv_" + nGlobalActiveTab);
    if (!oMainFileDiv)
        return;

    //Info fiche
    var nParentFileId = oMainFileDiv.getAttribute("fid");
    var nParentTab = nGlobalActiveTab;

    var sBkm;
    //Tous les signet
    if (nBkm == -1) {
        var oDivBkm = document.getElementById("divBkmPres");
        var oBkmList = oDivBkm.querySelectorAll("div.bkmdiv");
        if (oBkmList.length == 0)
            return;


        for (var nCmtBkm = 0; nCmtBkm < oBkmList.length; nCmtBkm++) {
            sBkm = oBkmList.id.replace("bkm_", "");
            var oHV = document.getElementById("CMPFLD_" + sBkm);
            if (oHV && oHV.tagName == "INPUT") {
                var sLst = oHV.value;
                if (sLst.length > 0) {



                    //Appel a l'updater                    
                    updtComputedCol(nBkm, sLst, nParentTab, nParentFileId);
                }
            }
        }
    }
    else {
        sBkm = nBkm;
        var oHV = document.getElementById("CMPFLD_" + sBkm);
        if (oHV && oHV.tagName == "INPUT") {
            var sLst = oHV.value;
            if (sLst.length > 0) {

                //Appel a l'updater
                updtComputedCol(nBkm, sLst, nParentTab, nParentFileId);
            }
        }
    }



}

//Lance le calcul de la somme des colonnes
function docc(o, bReload, widgetFilterId) {
    if (typeof (bReload) == "undefined")
        bReload = false;

    var fldAlias = o.id.replace("IMG_SUM_COLS_", "");
    var oTd = document.getElementById("SUM_VAL_" + fldAlias);

    if (!oTd || oTd.tagName != "TD")
        return;

    //Test si un appel a déjà été fait sur ce champ
    var pending = oTd.getAttribute("pending") == "1";
    if (pending)
        return;

    var aDescId = fldAlias.split("_");
    var descId = aDescId[aDescId.length - 1];
    var nTab = aDescId[0];
    var nParentFileId = 0;
    var nParentTab = 0;
    var nWidgetFilterId = 0;

    if (!(typeof (widgetFilterId) == "undefined"))
        nWidgetFilterId = widgetFilterId;

    //Test si la liste existe
    var oCurTab = document.getElementById("mt_" + nTab);
    if (!oCurTab || oCurTab.tagName != "TABLE")
        return;

    //Cas des bookmarks
    var bFromBkm = oCurTab.getAttribute("ednmode") == "bkm";
    if (bFromBkm) {
        var oMainFileDiv = document.getElementById("fileDiv_" + nGlobalActiveTab);
        nParentFileId = oMainFileDiv.getAttribute("fid");
        nParentTab = nGlobalActiveTab;
    }

    var enabled = oTd.getAttribute("enabled") == "1";

    //SI activé, on désactive
    if (enabled && !bReload) {
        oTd.setAttribute("enabled", "0");
        oTd.setAttribute("pending", "0");

        oTd.innerText = "";
        oTd.textContent = "";
        o.style.visibility = "hidden";
        o.setAttribute("actif", "0");

        updtComputedColManager(null, nTab, nParentTab);
        return;
    }
    else {
        //Sinon on passe le statut a enabled
        oTd.setAttribute("enabled", "1");
        oTd.setAttribute("pending", "1");

        o.style.visibility = "";
        o.setAttribute("actif", "1");
    }

    //Appel a l'updater
    updtComputedCol(nTab, descId, nParentTab, nParentFileId, nWidgetFilterId);
}

function updtComputedCol(nTab, sListCol, nParentTab, nParentFileId, nWidgetFilterId) {



    var currentView = getCurrentView(document);

    //Caclul des sommes des colonnes uniquement en mode liste et fiche
    if (currentView != "LIST" && currentView != "FILE_MODIFICATION") {
        return;
    }

    var oComputedColManager = new eUpdater("mgr/eComputedValue.ashx", 0);
    oComputedColManager.ErrorCallBack = function () { };
    oComputedColManager.addParam("ListCol", sListCol, "post");
    oComputedColManager.addParam("Tab", nTab, "post");
    oComputedColManager.addParam("ParentTab", nParentTab, "post");
    oComputedColManager.addParam("ParentFileId", nParentFileId, "post");
    oComputedColManager.addParam("widgetFilterId", nWidgetFilterId, "post");

    if (currentView == "FILE_MODIFICATION") {
        var fileDiv = document.getElementById("fileDiv_" + nParentTab);
        var nParentPMID = getAttributeValue(fileDiv, "pmid");
        oComputedColManager.addParam("ParentPmFileId", nParentPMID, "post");
    }


    oComputedColManager.send(
        function (oRes) {
            updtComputedColManager(oRes, nTab, nParentTab);
        }
    );
}

function updtComputedColManager(oXml, nTab, nParentTab) {
    var bFromBkm = (nTab != nParentTab && nParentTab != 0)
    var sTabAlias;

    if (bFromBkm)
        sTabAlias = "mt_" + nTab;
    else
        sTabAlias = "mt_" + nTab;

    var oTab = document.getElementById(sTabAlias);
    if (oTab && oTab.tagName == "TABLE")
        var oTrComputed = oTab.querySelectorAll("tr.sumColRow");
    else
        return;

    if (oTrComputed.length == 0)
        return;

    //Si retour de updater
    if (oXml && oXml.nodeType == 9) {

        //Passe les lignes en visible
        for (var nCmptTR = 0; nCmptTR < oTrComputed.length; nCmptTR++) {
            oTrComputed[nCmptTR].style.display = "";
        }

        //Met à jour la valeur
        var oFields = oXml.getElementsByTagName("field");
        forEach(oFields, function (field) {
            var sAlias = field.getAttribute("alias").replace("COL_", "");
            var sValue = field.getAttribute("value");

            var oElem = document.getElementById('SUM_VAL_' + sAlias);
            var oImg = document.getElementById('IMG_SUM_COLS_' + sAlias);

            if (oElem && oImg) {
                oElem.innerText = sValue;
                oElem.textContent = sValue;
                oElem.setAttribute("enabled", "1");
                oElem.setAttribute("pending", "0");

                oImg.style.visibility = "";
                oImg.setAttribute("actif", "1");

            }
        });
    }
    else {
        //recherche si des colonnes calculées sont encore affichées
        var oTdComputed = oTab.querySelectorAll("td.sumColRow");
        for (var nCmptTD = 0; nCmptTD < oTdComputed.length; nCmptTD++) {
            if (oTdComputed[nCmptTD].getAttribute("enabled") == "1")
                return;
        }

        //Passe les lignes en invisible
        for (var nCmptTR = 0; nCmptTR < oTrComputed.length; nCmptTR++) {
            oTrComputed[nCmptTR].style.display = "none";
            sAlias = oTrComputed[nCmptTR]

            var oElem = document.getElementById('SUM_VAL_' + sAlias);
            var oImg = document.getElementById('IMG_SUM_COLS_' + sAlias);

            if (oElem && oImg) {
                oElem.innerText = sValue;
                oElem.setAttribute("enabled", "0");
                oElem.setAttribute("pending", "0");
                oImg.style.visibility = "hidden";
                oImg.setAttribute("actif", "0");
            }
        }
    }
}

// doFilter
function dof(o, bFromWidget) {

    var ednTyp = o.getAttribute("ednTyp");
    var fldAlias = o.id.replace("IMG_FILTER_", "");
    // Rechercher le parent hdName
    o = thtabevt.findUp(o, "TH");

    if (o == null) {
        return;
    }

    var nTab = GetMainTableDescId(o.id);
    var descId = o.getAttribute("did");

    oListScrollManager.saveScrollPosition(nTab);

    //Position
    var obj_pos = getAbsolutePosition(o);
    var myX = obj_pos.x - 6;

    // Type catalogue
    var pop = o.getAttribute("pop");
    var bMultiple = o.getAttribute("mult") == "1" ? true : false;
    var bSpecial = (o.getAttribute("special") == "1");
    var tabTyp = o.getAttribute("tabtyp");

    var parentFileId = '';
    var currentView = getCurrentView(document);
    if (currentView == "FILE_CONSULTATION" || currentView == "FILE_MODIFICATION")
        parentFileId = document.getElementById("fileDiv_" + nGlobalActiveTab).getAttribute("fid");

    if (bFromWidget == null || typeof bFromWidget === "undefined")
        bFromWidget = false;

    expressFilter = new eExpressFilter("expressFilter", descId, ednTyp, nTab, tabTyp, nGlobalActiveTab, obj_pos.y + 18, obj_pos.x, pop, bMultiple, bSpecial, parentFileId);
    expressFilter.tree = o.getAttribute("tree") == "1"
    expressFilter.popid = getNumber(o.getAttribute("popid"));
    expressFilter.catenum = getNumber(getAttributeValue(o, "data-enumt"));
    expressFilter.datadesct = getNumber(getAttributeValue(o, "data-desct"));

    expressFilter.lib = o.getAttribute("lib");
    expressFilter.bFromWidget = bFromWidget;
    expressFilter.thCallerId = o.getAttribute("id");
    //MCR  40756 : ajout de la variable : pop=2 si c'est un catalogue
    loadInitialValues(expressFilter, fldAlias, ednTyp, pop);
}

function loadInitialValues(oExpressFilter, fldAlias, ednTyp, pop) {
    var strValuesList = "$|$";
    var bMulti = (document.getElementById("COL_" + fldAlias).getAttribute("mult") == "1");

    var htmlTable = document.getElementById("mt_" + fldAlias.split("_")[0]);

    // HLA - pour distinguer les valeurs selectionnées et l'operateur contient du filtre express, on laisse l'opérateur égal à - #38838
    var op = 0;

    if (ednTyp == TYP_BIT || ednTyp == TYP_BITBUTTON)   //Case à cocher, on ne reprend pas le contenu de la liste affichée.
        return;

    var aAlias = fldAlias.split('_');
    var nTab = getNumber(aAlias[0]);
    var nDescid = getNumber(aAlias[1]);

    var bFromWidget = oExpressFilter.bFromWidget;
    var sValue = "";

    for (var i = 0; i < htmlTable.getElementsByTagName('td').length; i++) {
        var oCell = htmlTable.getElementsByTagName('td')[i];
        if (oCell.getAttribute('ename') != "COL_" + fldAlias) {
            continue;
        }

        var strDBValue = (oCell.getAttribute('dbv') != null && oCell.getAttribute('dbv') != "") ? (oCell.getAttribute('dbv')) : decodeHTMLEntities(oCell.innerHTML);

        if (oCell.getAttribute('lastdataid') != null && oCell.getAttribute('lastdataid') != "") {
            strDBValue = oCell.getAttribute('lastdataid');
        }

        // HLA - On retire la valeur de non visu de la valeur
        if (strDBValue == null || strDBValue == "" || strDBValue == "&nbsp;" || strDBValue == top._res_1695 || (typeof (strDBValue) == "string" && strDBValue.trim() == ""))
            continue;

        var aDbValues;
        var aDisplayedLabels;

        if (bMulti) {
            aDbValues = strDBValue.split(';');
            aDisplayedLabels = GetText(oCell).split(';');
        }
        else {
            aDbValues = new Array();
            aDbValues[0] = strDBValue;

            aDisplayedLabels = new Array();
            if (nTab == TAB_PP && nDescid == TAB_PP + 1)
                aDisplayedLabels[0] = encodeHTMLEntities(strDBValue);
            else {
                aDisplayedLabels[0] = GetText(oCell);          // le libelle a afficher //BSE #45 013
                // MCR 40756 Filtre express catalogue utiliser les valeurs de la rubrique : 
                //           remplacer la chaine de caractere par l'id de la fiche pour une recherche exacte
                //           si la rubrique est un catalogue,  pop == "2"
                // 41 409 CRU : On n'affecte les valeurs que si la valeur "lnkid" n'est pas vide sinon cela écrase les valeurs affectées précédemment
                if (oCell.getAttribute('eaction') == "LNKGOFILE" && (pop == "2" || pop == "4") && oCell.getAttribute('lnkid')) {
                    aDbValues[0] = oCell.getAttribute('lnkid');
                    strDBValue = oCell.getAttribute('lnkid');
                }
            }
        }

        for (var j = 0; j < aDbValues.length; j++) {
            strDBValue = aDbValues[j];
            if (strValuesList.indexOf('$|$' + strDBValue + '$|$') == -1) {
                var cssClass = "actionItem";
                var bNumeric = isFormatNumeric(ednTyp);
                if (bNumeric) // var TYP_NUMERIC = 10; // 'Numérique var TYP_AUTOINC = 4; // 'Compteur auto var TYP_MONEY = 5; // 'Numéraire var TYP_COUNT = 18; // 'Count
                    cssClass += " numericItem";

                strValuesList += strDBValue + '$|$';

                // 36 008 - Internationalisation des champs de type Date
                if (ednTyp == FLDTYP.DATE)
                    strDBValue = eDate.ConvertDisplayToBdd(strDBValue);
                else if (bNumeric)	//GCH - SPRINT 2015.04 - #36869, #37690 - [Internationalisation] - Affichage des numériques - Rendu/Maj - Filtre express (MRU)
                    strDBValue = eNumber.ConvertDisplayToBdd(strDBValue, true);

                if (bFromWidget) {
                    strDBValue = aDisplayedLabels[j] + "#$|#$" + strDBValue;
                }

                oExpressFilter.addItem(aDisplayedLabels[j], "searchOperator(" + op + ",'" + strDBValue.replace(/'/g, "\\'") + "', expressFilter);", 1, 0, cssClass, "");
            }
        }

    }
}

var quickUserFilter;
// Filtre Rapide Utilisateurs avec recherche activée
function doQckUsrFlt(oId, descId, ednTyp, nTab, ednTabTyp, bMultiple, userid, userDisplay) {
    var aOtherUsrid, aOtherUsrName; //params suppr

    var o = document.getElementById(oId);

    //Position
    var obj_pos = getAbsolutePosition(o);
    var myX = obj_pos.x - 6;

    quickUserFilter = new eExpressFilter("quickUserFilter", descId, ednTyp, nTab, ednTabTyp, nTab, obj_pos.y, obj_pos.x, 0, bMultiple);
    quickUserFilter.userid = userid;
    quickUserFilter.userDisplay = userDisplay;
    SetParamQuickUserFilter(quickUserFilter, nTab);
    /******************************************************/
}

//Initialisation aux valeurs par défaut (celle de eParam) du filtre rapide
function SetParamQuickUserFilter(quickUserFilter, nTab) {
    quickUserFilter.addItem(quickUserFilter.userDisplay, "saveQckUsrFltFromList(" + nTab + ", " + quickUserFilter.userid + ",'" + quickUserFilter.userDisplay.replace(/'/g, "\\'") + "', " + quickUserFilter.nDescId + ");", 1, 0, "actionItem", "");

    var oeParam = getParamWindow();
    if (!oeParam)
        return;
    var SEPARATOR_MRU_LVL1 = "$|$";
    var SEPARATOR_MRU_LVL2 = "$;$";
    /*Init de la liste d'utilisateur du groupe en cours*****/
    var oParamQckUsrLst = oeParam.document.getElementById('QckUsrLstInput');
    if (!oParamQckUsrLst)
        return;
    var sQckUsrLst = oParamQckUsrLst.value;
    if (!sQckUsrLst)
        return;
    var tabQckUsrLst = sQckUsrLst.split(SEPARATOR_MRU_LVL1);
    if (!tabQckUsrLst)
        return;
    for (var i = 0; i < tabQckUsrLst.length; i++) {
        var currentUsr = tabQckUsrLst[i].split(SEPARATOR_MRU_LVL2);
        if (currentUsr.length > 1) {
            var sId = currentUsr[0];
            var sLib = currentUsr[1];
            if (sId != quickUserFilter.userid) {    //L'utilisateur en cours ne doit pas apparraitre 2 fois car il est implicitement ajouté plus haut.
                if (sId.indexOf("G") >= 0)  //GROUPE pas les même classes
                    quickUserFilter.addItem(sLib, "saveQckUsrFltFromList(" + nTab + ", '" + sId + "','" + sLib.replace(/'/g, "\\'") + "', " + quickUserFilter.nDescId + ");", 1, 0, "actionItem qckGroupFlt", "");
                else
                    quickUserFilter.addItem(sLib, "saveQckUsrFltFromList(" + nTab + ", " + sId + ",'" + sLib.replace(/'/g, "\\'") + "', " + quickUserFilter.nDescId + ");", 1, 0, "actionItem", "");
            }
        }
    }
}

// TODO - modif le nom de la fonction pour un nom plus court
// a priori, jamais utilisé
/*
function doGlobalFilter(mainTab, filterId) {
    var updatePref = "tab=" + mainTab + ";$;listfilterid=" + filterId;
    updateUserPref(updatePref, function () { firstpage(mainTab); });
}
*/
// colHeadOver
function chov(o) {
    if (!o)
        return;

    var fldAlias = o.id.replace("COL_", "");

    showMenuImg('IMG_SORT_ASC_' + fldAlias, true);
    showMenuImg('IMG_SORT_DESC_' + fldAlias, true);
    showMenuImg('IMG_FILTER_' + fldAlias, true);
    showMenuImg('IMG_SUM_COLS_' + fldAlias, true);


}

// colHeadOut
function chou(o) {
    if (!o)
        return;

    var fldAlias = o.id.replace("COL_", "");

    showMenuImg('IMG_SORT_ASC_' + fldAlias, false);
    showMenuImg('IMG_SORT_DESC_' + fldAlias, false);
    showMenuImg('IMG_FILTER_' + fldAlias, false);
    showMenuImg('IMG_SUM_COLS_' + fldAlias, false);
}

//
function showMenuImg(imgId, bVisible) {
    var bActif;

    if (!document.getElementById(imgId))
        return;

    var oElem = document.getElementById(imgId);

    if (bVisible)
        oElem.style.visibility = 'visible';
    else {
        try {
            // Gestion du trie déjà activé
            bActif = (oElem.getAttribute("actif") == "1");
        }
        catch (e) {
            bActif = false;
        }

        if (!bActif)
            oElem.style.visibility = 'hidden';
    }
}

/***********************************************************************/
/** FIN DE GESTION AFFICHAGE DES ICON D'ENTETE (tri, filtre, calcul)  **/
/***********************************************************************/
function colMenu(fldAlias, e) {
    if (!e)
        e = window.event;

    stopEvent(e);
}

/// Initialise l'event click sur les cellules
function initFldClick(mainTab) {

    if (document.getElementById("mt_" + mainTab) == undefined)
        return;
    if (document.getElementById("mt_" + mainTab) == null)
        return;

    var mt = document.getElementById("mt_" + mainTab);

    mt.onclick = fldLClick;
    mt.onkeyup = fldKeyUp;
    mt.onselect = fldSelect;
    mt.oncontextmenu = fldInfoOnContext;


    //pour mode liste, scroll sur mousescroll
    if (false && mt && getAttributeValue(mt, "ednmode") == "list") {
        // For Chrome
        mt.addEventListener('mousewheel', mouseWheelEvent);

        // For Firefox
        mt.addEventListener('DOMMouseScroll', mouseWheelEvent);
    }

    if (document.getElementById("tblIDX_" + mainTab))
        document.getElementById("tblIDX_" + mainTab).onclick = fltIdx;
}

function refreshCharIndex(oTableNode) {
    var mode = oTableNode.getAttribute("eVisuMode");

    forEach(oTableNode.tBodies[0].rows[0].cells, function (td) {
        if (td.id == 'fidx_alp' || td.id == 'fidx_num') {
            if (td.id.replace('fidx_', '') != mode)
                removeClass(td, 'fIHide');
            else
                addClass(td, 'fIHide');
        }
        else if (td.getAttribute('typ')) {
            if (td.getAttribute('typ') == mode)
                removeClass(td, 'fIHide');
            else
                addClass(td, 'fIHide');
        }
    });
}

//
function fltIdx(e) {
    //Evenement
    if (!e)
        var e = window.event;

    // Objet source
    var oSourceObj = e.target || e.srcElement;
    var topelement = "TABLE";
    try {
        while (
            oSourceObj.tagName != topelement
            && !(oSourceObj.tagName == 'TD' && oSourceObj.id.indexOf("fidx_") == 0)
        ) {
            oSourceObj = oSourceObj.parentNode || oSourceObj.parentElement;
        }
    }
    catch (ee) {
        return;
    }

    if (oSourceObj.id.indexOf("fidx_") == 0) {
        var oParentTable = oSourceObj.parentNode || oSourceObj.parentElement;
        while (oParentTable.tagName != "TABLE")
            oParentTable = oParentTable.parentNode || oParentTable.parentElement;

        if (oSourceObj.id == "fidx_num" || oSourceObj.id == "fidx_alp") {
            // On inverse le mode
            var mode = oParentTable.getAttribute("eVisuMode");

            if (mode == 'alp')
                oParentTable.setAttribute("eVisuMode", "num");
            else
                oParentTable.setAttribute("eVisuMode", "alp");

            refreshCharIndex(oParentTable);
            return;
        }

        //Table principale
        var mainTab = oParentTable.id.replace("tblIDX_", "");

        //Dernier index
        var sLastIdx = oParentTable.getAttribute("eLastIdx");

        if (sLastIdx == "")
            sLastIdx = "*";

        var oLastIdx = document.getElementById("fidx_" + sLastIdx);

        //Index Courrant
        var oCurrentIdx = oParentTable.getAttribute("eCurrIdx");
        var sIdx = oSourceObj.id.split("_")[1];

        var sOrigIdx = sIdx;
        if (sIdx == "*")
            sIdx = "";

        // Maj Pref
        var updatePref = "tab=" + mainTab + ";$;charindex=" + sIdx;
        updateUserPref(updatePref, function () { firstpage(mainTab); });

        // Switch index
        oParentTable.setAttribute("eLastIdx", sOrigIdx);

        var itemAll = false;
        itemAll = oLastIdx.id.replace("fidx_", "") == "*";
        if (!itemAll)
            switchClass(oLastIdx, "fIOn", "fIOff");

        itemAll = oSourceObj.id.replace("fidx_", "") == "*";
        if (!itemAll)
            switchClass(oSourceObj, "fIOff", "fIOn");
    }
}

/************************/
/*  BOUTONS ACTIONS     */
/************************/
function omovA(sBtn) {
    if (sBtn != "H")
        sBtn = "B";

    var btnG = document.getElementById("aG" + sBtn);
    var btnM = document.getElementById("aM" + sBtn);
    var btnD = document.getElementById("aD" + sBtn);

    switchClass(btnG, "aG" + sBtn, "aG" + sBtn + "H");
    switchClass(btnD, "aD" + sBtn, "aD" + sBtn + "H");
    switchClass(btnM, "aM", "aMH");
}

function omouA(sBtn) {
    if (sBtn != "H")
        sBtn = "B";

    var btnG = document.getElementById("aG" + sBtn);
    var btnM = document.getElementById("aM" + sBtn);
    var btnD = document.getElementById("aD" + sBtn);

    if (btnM.getAttribute("eactif") == "1")
        return;

    switchClass(btnG, "aG" + sBtn + "H", "aG" + sBtn);

    removeClass(btnD, "aD" + sBtn + "A");
    switchClass(btnD, "aD" + sBtn + "H", "aD" + sBtn);

    switchClass(btnM, "aMH", "aM");
}

/****  Paging   *** **/
//1er page
function firstpage(nTab, bReload) {
    if (!bReload)
        var bReload = false;

    var oTable = document.getElementById("mt_" + nTab);
    var nbPage = 1;
    if (oTable != null)
        nbPage = Number(oTable.getAttribute("nbPage"));

    var oeParam = getParamWindow();
    oeParam.SetParam('Page_' + nTab, 1);
    loadList(1, bReload);
}

//Dernières Pages
function lastpage(nTab, bReload) {
    if (!bReload)
        var bReload = false;

    var oTable = document.getElementById("mt_" + nTab);
    var nbPage = Number(oTable.getAttribute("nbPage"));
    var eof = Number(oTable.getAttribute("eof"));

    if (eof == "1")
        return;

    var oeParam = getParamWindow();
    oeParam.SetParam('Page_' + nTab, nbPage);
    loadList(nbPage, bReload);
}

///Page Suivante
function nextpage(nTab, bReload, addtotab) {
    if (!bReload)
        var bReload = false;

    var oTable = document.getElementById("mt_" + nTab);
    var cPage = Number(oTable.getAttribute("cpage"));
    var eof = Number(oTable.getAttribute("eof"));

    if (eof != "1")
        cPage++
    else
        return;

    var oeParam = getParamWindow();
    oeParam.SetParam('Page_' + nTab, cPage);

    loadList(cPage, bReload, addtotab);

}

///Page précédente
function prevpage(nTab, bReload) {
    if (!bReload)
        var bReload = false;

    var oTable = document.getElementById("mt_" + nTab);
    var cPage = Number(oTable.getAttribute("cPage"));

    if (cPage > 1)
        cPage--;
    else
        return;

    var oeParam = getParamWindow();
    oeParam.SetParam('Page_' + nTab, cPage);
    loadList(cPage, bReload);
}

///Page selection
function selectpage(nTab, oNewPage, bReload) {
    if (!bReload)
        var bReload = false;

    var oTable = document.getElementById("mt_" + nTab);
    var newPage = Number(oNewPage.value);
    var cPage = Number(oTable.getAttribute("cPage"));
    var nbpage = Number(oTable.getAttribute("nbpage"));

    //changement de page seulement si la page demandée n'est pas la page actuellement affichée, page demandée > 1 et page demandée < au nombre de page de la liste en cours 
    if ((newPage == cPage) || (newPage < 1) || (newPage > nbpage) || !isNumeric(newPage)) {
        oNewPage.value = cPage;
        return;
    }

    var oeParam = getParamWindow();
    oeParam.SetParam('Page_' + nTab, newPage);

    loadList(newPage, bReload);
}

///Met à jour les informations de paging
function setPaging(nTab) {
    // Récupération information de paging
    var oTable = document.getElementById("mt_" + nTab);
    if (oTable == null || oTable == undefined)
        return;

    var cnton = Number(oTable.getAttribute("cnton"));
    var cPage = Number(oTable.getAttribute("cPage"));
    var nbPage = Number(oTable.getAttribute("nbPage"));

    if (nbPage == 0)
        nbPage = 1;

    // Paging non activé
    if (cPage <= 1)
        cPage = 1;

    if (cPage >= nbPage && cnton == "1")
        cPage = nbPage;

    //Boite de numéro de page
    var oPag = document.getElementById("divNumpage");
    var oPagInput = document.getElementById("inputNumpage");
    var oPagTd = document.getElementById("tdNumpage");



    if (cnton == "1") {
        if (nbPage != "0") {
            var sInputPage = "";
            var sTextPaging = "&nbsp;/&nbsp;" + nbPage;
            if (nbPage > 1) {
                sInputPage = cPage;
            }
            else {
                sTextPaging = cPage + sTextPaging;
            }

            if (oPagInput && oPagTd) {
                if (sInputPage + "" != "") {

                    oPagInput.value = sInputPage;
                    oPagInput.style.display = "block";
                    oPagInput.visible = true;
                    oPagInput.size = nbPage.toString().length;
                    oPagInput.maxlength = nbPage.toString().length;


                    oPagTd.style.width = "51%";

                }
                else {
                    oPagInput.value = "";
                    oPagInput.style.display = "none";
                    oPagInput.visible = false;


                    oPagTd.style.width = "70%";

                }
            }

            oPag.innerHTML = sTextPaging;

        }
    }
    else {
        oPag.innerHTML = cPage;

    }

    //
    var bof = Number(oTable.getAttribute("bof"));
    var eof = Number(oTable.getAttribute("eof"));


    var oFirst = document.getElementById("idFirst");
    var oLast = document.getElementById("idLast");

    var oPrev = document.getElementById("idPrev");
    var oNext = document.getElementById("idNext");

    //désactive/active précédent & première page
    oFirst.className = "";
    oPrev.className = "";

    if (bof != "1") {
        if (cnton == "1") {
            oFirst.className = "icon-edn-first icnListAct";
        }

        oPrev.className = "icon-edn-prev fLeft icnListAct";
    }
    else {
        if (cnton == "1") {
            oFirst.className = "icon-edn-first disable icnListAct";
        }

        oPrev.className = "icon-edn-prev fLeft disable icnListAct";
    }


    oLast.className = ""
    oNext.className = ""

    if (eof != "1") {

        if (cnton == "1") {
            oLast.className = "icon-edn-last icnListAct"
        }

        oNext.className = "icon-edn-next fRight icnListAct"
    }
    else {

        if (cnton == "1") {
            ///  oLastB.className = "pagLastDis"
            oLast.className = "icon-edn-last disable icnListAct"
        }

        //oNextB.className = "icon-edn-next disable"
        oNext.className = "icon-edn-next fRight disable icnListAct"
    }
}


///Count on demand
function cod(nTab) {
    var oCoDManager = new eUpdater("mgr/eCountOnDemandManager.ashx", 1);
    oCoDManager.ErrorCallBack = function () { };
    oCoDManager.addParam("operation", 1, "post");
    oCoDManager.send(updatePagging, nTab);
}

//Mise à jour du count on demand
function updatePagging(sParam, nTab) {
    if (sParam.length <= 0)
        return;

    var aResult = sParam.split(";");

    if (aResult < 3)
        return;

    var nNbResult = (aResult[0]);
    var nNbPage = (aResult[1]);
    var nNbTotalResult = (aResult[2]);

    //Nombre d'éléments
    var oSpan = document.getElementById("SpanNbElem");
    var oSpanLib = document.getElementById("SpanLibElem");
    if (oSpan) {
        oSpan.innerHTML = nNbResult;
        // sur total
        if (nNbTotalResult != "-1")
            oSpan.innerHTML = oSpan.innerHTML + "&nbsp;/&nbsp;" + nNbTotalResult;
    }

    //Pagging
    var oTab = document.getElementById("mt_" + nTab);
    setAttributeValue(oTab, "eNbCnt", nNbResult);
    setAttributeValue(oTab, "eNbTotal", nNbTotalResult);
    setAttributeValue(oTab, "nbpage", nNbPage);
    setAttributeValue(oTab, "eHasCount", "1");
    // réactivation voir #62537 - Attente de validation 
    setAttributeValue(oTab, "cnton", "1");

    setPaging(nTab);
}

//Marked Line
function chkMarkedFile(obj) {
    // 
    var elem = obj.parentNode || obj.parentElement;

    //surbillance de la ligne..
    highlightLineIfFromList(elem);

    while (elem.tagName != 'TR') {

        elem = elem.parentNode || elem.parentElement;

        if (elem.tagName == 'TBODY')
            return;
    }

    applyChkUpdate(elem, obj);

}

// Appel au manager pour mise à jour
function applyChkUpdate(trElement, cbo) {
    // id  
    var oId = trElement.getAttribute("eid").split('_');
    var nTab = oId[0];
    var nFileId = oId[oId.length - 1];

    //Cas d'un contact avec plusieurs @ check tous les users
    checkElemts(trElement, nFileId);

    var bAdd = 0;
    if (cbo.getAttribute("chk") == "1")
        bAdd = 1;

    var oMarkedFileManager = new eUpdater("mgr/eMarkedFilesManager.ashx", 0);
    oMarkedFileManager.ErrorCallBack = function () { };
    oMarkedFileManager.addParam("fileid", nFileId, "post");
    oMarkedFileManager.addParam("type", 3, "post");
    oMarkedFileManager.addParam("add", bAdd, "post");
    oMarkedFileManager.addParam("tab", nTab, "post");

    oMarkedFileManager.send(function (oXml) { updtMarkedCnt(oXml, nFileId); });

}

//Function qui slectionne les contact ou societés avec plusieurs @
function checkElemts(elem, nId) {
    var elemL = elem.parentNode;
    var elemC = null;
    var oIdL = null;
    var nIdL = 0;

    for (var i = 0; i < elemL.children.length; i++) {

        if (elemL.children[i].getAttribute("eid") != null) {
            oIdL = elemL.children[i].getAttribute("eid").split('_');
            nIdL = oIdL[oIdL.length - 1];

            if (nId == nIdL && elemL.children[i] != elem) {
                elemC = elemL.children[i].children[0].children[0];
                chgChk(elemC);
            }
        }
    }
}

// décoche/coche la page en cours
function selectAllList(obj) {
    var bAdd = 0;
    if (obj.getAttribute("chk") == "1")
        bAdd = 1;


    if (obj.id.indexOf("chkAll_") != 0) {
        return;
    }

    var aTab = obj.id.split('_');
    var nTab = aTab[1];

    var oMarkedFileManager = new eUpdater("mgr/eMarkedFilesManager.ashx", 0);
    oMarkedFileManager.ErrorCallBack = function () { };
    oMarkedFileManager.addParam("type", 4, "post");
    oMarkedFileManager.addParam("add", bAdd, "post");
    oMarkedFileManager.send(updtMarkedCnt);
}

//Met à jour le compteur de fiches marquées
function updtMarkedCnt(oXml, nFileId) {
    if (oXml && oXml.nodeType == 9) {

        // Test erreur
        if (getXmlTextNode(oXml.getElementsByTagName("success")[0]) == '1') {

            var nType = getXmlTextNode(oXml.getElementsByTagName("actiontype")[0]);

            //Savoir si Afficher "Uniquement les liste cochées" est activé
            var bSelectionEnabled = (getXmlTextNode(oXml.getElementsByTagName("bSelectionEnabled")[0]) == "1");

            //Informations sur la sélections de fiches marquées en cours
            var bhasSelect = (getXmlTextNode(oXml.getElementsByTagName("hasSelect")[0]) == "1");
            if (bhasSelect) {

                var nTab = getXmlTextNode(oXml.getElementsByTagName("tab")[0]);
                var sLabel = getXmlTextNode(oXml.getElementsByTagName("markedfilename")[0]);
                var nId = getXmlTextNode(oXml.getElementsByTagName("markedfileid")[0]);
                var nbMarkedFiles = getXmlTextNode(oXml.getElementsByTagName("nbmakedfiles")[0]);

                var oMFId = document.getElementById("markedFileId_" + nTab);
                var oMFLabel = document.getElementById("markedLabel_" + nTab)
                var oMFNb = document.getElementById("markedNb_" + nTab)
                if (oMFId) {

                    oMFId.value = nId;
                    oMFLabel.value = sLabel;
                    oMFNb.value = nbMarkedFiles;
                }

                var oNbCheckedFoot = document.getElementById("nbCheckedFoot");
                var oNbCheckedHead = document.getElementById("nbCheckedHead");

                //Met à jour les compteurs
                if ((oNbCheckedFoot != null) && (typeof (oNbCheckedFoot) != "undefined")) {
                    oNbCheckedFoot.innerHTML = nbMarkedFiles;
                }

                if (typeof (oNbCheckedHead) != "undefined") {
                    oNbCheckedHead.innerHTML = nbMarkedFiles;
                }

                var fileId = getNumber(nFileId);
                if (!isNaN(fileId))
                    oEvent.fire("file-marked", { fileId: fileId + "" });
            }

            //decoche/coche toutes les cases
            if (oXml.getElementsByTagName("checkall").length == 1 && getXmlTextNode(oXml.getElementsByTagName("checkall")[0]) == "1") {

                var nTab = getXmlTextNode(oXml.getElementsByTagName("tab")[0]);
                var oLstChk = document.getElementsByName("chkMF");
                var obj = document.getElementById("chkAll_" + nTab);
                var oChekAllStatus = obj.getAttribute("chk");

                for (var i = 0; i < oLstChk.length; i++) {

                    var oChk = oLstChk[i];

                    if (oChekAllStatus != oChk.getAttribute("chk"))
                        chgChk(oChk);
                }

                oEvent.fire("file-all-marked", { 'marked': oChekAllStatus });
            }

            //MOU 29/04/2014 si l'affichage uniqument des lignes cochées est activé, 
            //on recharge la liste principale de la page courante et on mis a jour le compteur
            if (bSelectionEnabled) {
                var oTable = document.getElementById("mt_" + top.nGlobalActiveTab);
                var currentPage = Number(oTable.getAttribute("cPage"));
                loadList(currentPage, true);
            }
        }
        // 
    }
}


function setPlgCol(nTab) {

    var oCalDiv = document.getElementById("CalDivMain");

    if (!oCalDiv) {
        eAlert(0, top._res_6380, top._res_6655);
        return;
    }

    var bMixMode = getAttributeValue(oCalDiv, "mixtemode") == "1";

    if (!bMixMode) {
        setCol(nTab);
        return;
    }

    var mdlChoiceCalViewFlds = eConfirm(1, top._res_96, top._res_837, "", 450, 100, function () { onSetPlgCol(mdlChoiceCalViewFlds, nTab); }, null, true);
    //Indique que l'on souhaite que les options soient des RadioBoutons (choix unique)
    mdlChoiceCalViewFlds.isMultiSelectList = false;

    mdlChoiceCalViewFlds.addSelectOption(top._res_849, "lst", true);    //Liste
    mdlChoiceCalViewFlds.addSelectOption(top._res_838, "cal", false);   //Calendrier

    //Force l'ajout des radioboutons
    mdlChoiceCalViewFlds.createSelectListCheckOpt();

    //Indique que la taille de la fenêtre doit s'ajuster au contenu
    mdlChoiceCalViewFlds.adjustModalToContent(40);
}
function onSetPlgCol(mdlChoiceCalViewFlds, nTab) {
    if (!mdlChoiceCalViewFlds) {
        eAlert(0, top._res_72, top._res_6658, '');
        throw "onSetPlgCol - modal non trouvée.";
        return;
    }
    //Récupère les valeurs sélectionnées ici "lst" ou "cal"
    var result = mdlChoiceCalViewFlds.getSelected();
    if (result && result.length > 0 && result[0].val) {
        var bLst = (result[0].val == "lst");
        setCol(nTab, bLst);
    }
    else {
        eAlert(0, top._res_72, top._res_6658, '');
        throw "onSetPlgCol - pas de résultat !";
        return;
    }
}

function setCol(nTab, forcelst) {
    modalListCol = new eModalDialog(top._res_96, 0, "eFieldsSelect.aspx", 850, 550);
    modalListCol.ErrorCallBack = function () { setWait(false); }
    modalListCol.addParam("tab", nTab, "post");

    if (forcelst)
        modalListCol.addParam("forcelst", forcelst ? "1" : "0", "post");

    modalListCol.bBtnAdvanced = true;
    closeRightMenu();

    modalListCol.show();
    modalListCol.addButton(top._res_29, onSetColAbort, "button-gray", nTab);
    modalListCol.addButton(top._res_28, onSetColOk, "button-green", nTab);
}

function onSetColOk(nTab, popupId) {
    var _frm = window.frames["frm_" + popupId];
    var strListCol = _frm.getSelectedDescId();

    //Récupération du strListCol
    _frm = document.getElementById("frm_" + popupId);
    var _oDoc = _frm.contentWindow.document || _frm.contentDocument;
    var cbo = _oDoc.getElementById("AllSelections");
    var selId = cbo.options[cbo.selectedIndex].value;
    var calViewMode = _oDoc.getElementById("calviewmode").value == "1";

    var fct;
    if (getAttributeValue(document.getElementById("CalDivMain"), "mixtemode") == "1")
        fct = function () { goTabList(nTab); };
    else
        fct = function () { loadList(); };

    if (calViewMode) {
        var updatePref = "tab=" + nTab + ";$;calendarcol=" + strListCol;
        updateUserPref(updatePref, fct);
    }
    else {
        var mainField = document.getElementById("mainFld_" + nTab);
        if (mainField)
            mainField = mainField.value;
        else
            mainField = 0;
        var updatePref = "tab=" + nTab + ";$;mainFld=" + mainField + ";$;listcol=" + selId + ';|;' + strListCol;
        // Callback d'erreur : on reste sur la page
        updateUserSelection(updatePref, fct, function () { });
    }

    modalListCol.hide();
}

function onSetColAbort(v1, popupId) {
    modalListCol.hide();
}

/* Boite de recherche */
function searchFocus() {
    var eFSContainer = document.getElementById("eFS");
    addClass(eFSContainer, "eFSContainerSearch");
}

function searchBlur() {
    var eFSContainer = document.getElementById("eFS");
    removeClass(eFSContainer, "eFSContainerSearch");
}

var searchTimerMainF;
var previousSearch; //Dernière recherche
//Lance la recherche
//Si oTargetTable est défini, on effectue la recherche en JS côté client en filtrant les cellules du tableau correspondant à cette variable
function launchSearch(sSearch, e, oTargetTable) {




    var bEnter = (e != null && e.keyCode && e.keyCode == 13);


    clearTimeout(searchTimerMainF);
    if (typeof (sSearch) === "string") {

        if ((oTargetTable && sSearch.length > 2) || (bEnter && sSearch.length > 0)) { // CNA - Demande #52864 - La recherche loupe ne doit se déclencher que sur entrer
            var fct = function () {

                var eFSInput = document.getElementById("eFSInput");
                var eFSStatus = document.getElementById("eFSStatus");

                //Affiche le wait champ filtre
                eFSStatus.className = "eFSWait";

                // sort du focus
                eFSInput.blur();

                // affiche le wait global
                setWait(true);

                // Selon le contexte, déclenche la recherche côté serveur (MAJ de USERVALUE) ou côté client (masquage/affichage des lignes du tableau cible)
                if (oTargetTable) {
                    updateTableFromSearch(sSearch, oTargetTable, function () { endSearch(oTargetTable); }, function () { endSearch(oTargetTable) });
                }
                // Pas de table HTML précisée en paramètre : déclenchement de la recherche côté serveur par MAJ de USERVALUE
                else {
                    // Construction de la USERVALUE
                    var sValue = "type=20;$;tab=" + nGlobalActiveTab + ";$;value=" + sSearch;
                    // Action le end search
                    updateUserValue(sValue, function () { endSearch(); firstpage(nGlobalActiveTab); }, function () { endSearch() });
                }
            };

            searchTimerMainF = window.setTimeout(function () { fct() }, 500);
        }
        else if ((sSearch.length == 0) && (sSearch != previousSearch)) {
            resetFilter(oTargetTable);
        }
    }
    previousSearch = sSearch;
}

// ajoute la croix de recherche
function setReset(oTargetTable) {

    var eFSStatus = document.getElementById("eFSStatus");
    eFSStatus.className = "eFSReset";

    // Passage du champ de recherche en vert lorsqu'il est rempli
    var eFSContainer = document.getElementById("eFS");
    switchClass(eFSContainer, "eFSContainerSearch", "eFSContainerSearchActive");

    // click pour reset le filtre
    eFSStatus.onclick = function () {
        //Retire le filtre
        resetFilter(oTargetTable);
    }
}

// Reset le filtre de recherche
function resetFilter(oTargetTable) {
    // retire l'event on click et le bouton
    var eFSStatus = document.getElementById("eFSStatus");
    if (eFSStatus != null) {
        eFSStatus.onclick = "";
        eFSStatus.className = "";
    }

    // Remise à zéro du style
    var eFSContainer = document.getElementById("eFS");
    switchClass(eFSContainer, "eFSContainerSearchActive", "eFSContainerSearch");

    //vide le champ 
    var eFSInput = document.getElementById("eFSInput");
    eFSInput.value = "";

    //Place le wait
    setWait(true);

    // Selon le contexte, déclenche la recherche côté serveur (MAJ de USERVALUE) ou côté client (réaffichage des lignes du tableau cible)
    if (oTargetTable) {
        updateTableFromSearch('', oTargetTable, function () { endSearch(oTargetTable); }, function () { endSearch(oTargetTable) });
    }
    // Pas de table HTML précisée en paramètre : déclenchement de la recherche côté serveur par MAJ de USERVALUE
    else {
        //Retire le filtre
        var sValue = "type=20;$;tab=" + nGlobalActiveTab + ";$;value=";
        //Action le end search
        updateUserValue(sValue, function () { endSearch(); firstpage(nGlobalActiveTab, true); }, function () { endSearch(); });
    }
}

function endSearch(oTargetTable) {
    searchIn = 0;
    var eFSInput = document.getElementById("eFSInput");
    var sSearchValue = eFSInput.value;

    // Bouton reset si valeur de recherche
    if (sSearchValue.length > 0)
        setReset(oTargetTable);

    // On réajuste le style des lignes
    afterSortOrSearch(getTabLinesAsArray(oTargetTable, "line"));

    setWait(false);
}

///summary
/// Après un tri ou une recherche, on réinsère les éléments dans le bon ordre (tri uniquement), et/ou on réajuste les CSS des lignes pour réalterner les couleurs après avoir
/// modifié leur emplacement (tri) ou en avoir masqué (recherche)
function afterSortOrSearch(arrAllEntry) {
    if (!arrAllEntry)
        arrAllEntry = getTabLinesAsArray(oTargetTable);

    arrAllEntry.forEach(function (myElt) {
        // On réajuste les styles CSS pour rétablir correctement l'alternance de couleurs entre les lignes, en tenant compte des lignes masquées
        // (la précédente ligne comparée doit être la précédente visible, qui n'est pas forcément la précédente dans le DOM si une recherche l'a masquée)
        readjustTableLineStyle(myElt, "line1", "line2");
    });
}

///summary
///Récupère la liste des noeuds DOM correspondant aux lignes du tableau de la liste des onglets, et la convertit en Array
///summary
function getTabLinesAsArray(oTargetTable, lineSelector) {
    if (typeof (oTargetTable) != "object" && oTargetTable && oTargetTable != "")
        oTargetTable = document.getElementById(oTargetTable);

    if (!oTargetTable)
        return [];

    // Récupération des éléments à trier : lignes dont la classe CSS débute par "line"
    var allEntry = oTargetTable.querySelectorAll("tr[class^='" + lineSelector + "']");

    // On va devoir utiliser 2 fonctions disponibles uniquement sur le type Array (sort && forEach) sur le résultat du querySelectorAll (qui est de type NodeList).
    // On fait donc appel à "slice" pour convertir de NodeList à Array et disposer de sort et forEach (à défaut d'implémenter sur NodeList le même prototype que Array, ce
    // qui mettrait forEach & sort à disposition de tout objet NodeList)
    return Array.prototype.slice.call(allEntry);
}

///summary
/// Ajuste la classe CSS d'une ligne de tableau par rapport à sa voisine immédiatement visible, par ex. pour rétablir l'alternance de couleurs après un tri ou un masquage
/// des lignes
///summary
function readjustTableLineStyle(currentLine, lineStyle1, lineStyle2) {
    // On réajuste les styles CSS pour rétablir correctement l'alternance de couleurs entre les lignes, en tenant compte des lignes masquées
    // (la précédente ligne comparée doit être la précédente visible, qui n'est pas forcément la précédente dans le DOM si une recherche l'a masquée)
    if (!currentLine.style || currentLine.style.display != "none") {
        var previousVisibleSibling = currentLine.previousSibling;
        while (
            previousVisibleSibling != null && (
                previousVisibleSibling.nodeType != 1 || /* on évite les noeuds de type #text (3) pour cibler uniquement les DOM Element */
                (previousVisibleSibling.style && previousVisibleSibling.style.display == "none") /* on évite les DOM Element dont display est défini à none, pour ne cibler que les lignes visibles */
            )
        ) {
            previousVisibleSibling = previousVisibleSibling.previousSibling;
        }
        if (previousVisibleSibling != null && previousVisibleSibling.className == currentLine.className)
            switchClass(currentLine, currentLine.className, currentLine.className == lineStyle1 ? lineStyle2 : lineStyle1);
    }
}

/*********************************************************/
/*  Fonctions de ToolTip                                */
/*********************************************************/
function setFilterTip(mainTab) {
    var oDiv = document.getElementById("iconeFilter");
    var filterTipHtml = document.getElementById("filterTip_" + mainTab);

    if (!filterTipHtml) {
        if (oDiv != null) {
            removeClass(oDiv, "icon-list_filter");
        }
        return;
    }

    if (oDiv)
        oDiv.className = "icon-list_filter";

    forEach(filterTipHtml.childNodes, function (fTipDiv) {
        if (fTipDiv.getAttribute("fValue") == "|SPEC|") {
            var nDescid = fTipDiv.id.replace("fTipSpec", "");
            var elem = document.getElementById("QuickF_" + nDescid);

            if (elem && elem.selectedIndex >= 0) {
                fTipDiv.innerHTML = elem.options[elem.selectedIndex].text;
            }
        }
    });

    ////demande 84 750
    //if (oDiv) {
    //    var SpanNbElem = oDiv.nextSibling.querySelector("#SpanNbElem");
    //    var SpanLibElem = oDiv.nextSibling.querySelector("#SpanLibElem");
    //    var tabInfos = oDiv.parentNode;
    //    if (SpanNbElem && SpanLibElem && tabInfos.id === "tabInfos")
    //        tabInfos.className = "tabInfosFilter";
    //}
}

function stfilter(e, nTab) {
    if (!nTab)
        nTab = nGlobalActiveTab;

    var oNode = document.getElementById('filterTip_' + nTab);

    if (!oNode || oNode.innerHTML == '')
        return;

    if (!oNode.innerHTML || oNode.innerHTML == "")
        return;

    // Position
    var o = document.getElementById("iconeFilter");
    var obj_pos = getAbsolutePosition(o);

    contextMenu = new eContextMenu(null, obj_pos.y, obj_pos.x);

    if (!debugModeList) {
        var oActionMenuMouseOver = function () {

            var actionOut = setTimeout(
                function () {
                    contextMenu.hide();
                }
                , 200);

            // Annule la disparition
            setEventListener(contextMenu.mainDiv, "mouseover", function () { clearTimeout(actionOut) });
        };

        // Faire disparaitre le menu
        setEventListener(contextMenu.mainDiv, "mouseout", oActionMenuMouseOver);
    }

    var oElems = oNode.getElementsByTagName('div');
    if (oElems && oElems.length > 0) {
        var bHasExpressFilters = false;
        var bFromWidget = getAttributeValue(o, "data-wfilter") == "1";

        for (var i = 0; i < oElems.length; i++) {
            var oLi = oElems[i];
            var sfType = oLi.getAttribute("typ");
            var sfLevel = oLi.getAttribute("lvl");
            var sfValue = oLi.getAttribute("val");
            var sfTab = oLi.getAttribute("tab");
            var sfNoAct = (oLi.getAttribute("act") == "1");
            var sfBhtml = (oLi.getAttribute("html") == "1");

            if (sfLevel == 0)
                contextMenu.addLabel(oLi.innerHTML, 0, 0, "actionLabel");
            else {
                // 3 = filtre express
                if (sfType == 3) {
                    bHasExpressFilters = true;
                }
                //Infobulle sur le filtre en cours
                var strToolTip = (top._res_1179 + ":" + (sfBhtml ? oLi.innerText : removeHTML(oLi.innerText)));

                if (sfNoAct) {
                    contextMenu.addItem(oLi.innerHTML, "cancelFilterFromTip('" + sfType + "','" + sfValue + "','" + sfTab + "', " + ((bFromWidget) ? "true" : "false") + "); contextMenu.hide();", 0, getNumber(sfLevel), "actionItem icon-list_filter", strToolTip);
                }
                else
                    contextMenu.addItem(oLi.innerHTML, "", 0, getNumber(sfLevel), "actionItem fltMenuNoAct", strToolTip);
            }
        }
        if (bHasExpressFilters) {
            contextMenu.addSeparator(0);
            contextMenu.addItem(top._res_183, "CancelAllFiltersFromTip();contextMenu.hide();", 0, 1.5, "actionItem icon-rem_filter", "");
        }
    }
}

// Données à partir de l'enum FilterTipType du fichier eListMain.cs
var FILTER_CHARINDEX = 1;
var FILTER_HISTO = 2;
var FILTER_EXPRESS = 3;
var FILTER_QUICK = 4;
var FILTER_DEFAULT = 5;
var FILTER_MARKEDFILE = 6;
var FILTER_RANDOM = 7;
var FILTER_ADV = 8;

function CancelAllFiltersFromTip() {

    if (expressFilter && expressFilter.bFromWidget) {

        if (oListWidget) {
            oListWidget.cancelAllFilters();
        }
    }
    else {
        var updatePref = "tab=" + nGlobalActiveTab + ";$;charindex=;$;histo=0;$;canceladvfilter=1;$;filterexpress=$cancelallexpressfilters$";
        updateUserPref(updatePref, top.loadList);
    }

}

function cancelFilterFromTip(fTyp, fValue, fTab, bFromWidget) {

    var nTyp = getNumber(fTyp);

    switch (nTyp) {
        case FILTER_CHARINDEX:
            // Maj Pref
            var updatePref = "tab=" + fTab + ";$;charindex=";
            setWait(true);
            updateUserPref(updatePref, function () { firstpage(fTab); setWait(false); });
            var oTabParet = document.getElementById("tblIDX_" + fTab);

            // Dernier index
            var sLastIdx = oTabParet.getAttribute("eLastIdx");
            var oLastIdx = document.getElementById("fidx_" + sLastIdx);
            var oAllIdx = document.getElementById("fidx_*");

            // Switch index
            switchClass(oLastIdx, "fIOn", "fIOff");
            switchClass(oAllIdx, "fIOff", "fIOn");
            break;
        case FILTER_HISTO:
            doHistoFilter(bFromWidget);
            break;
        case FILTER_EXPRESS:
            cancelThisFilter(fTab, fValue, expressFilter);
            break;
        case FILTER_QUICK:
            // Plus d'actualité
            break;
        case FILTER_DEFAULT:
            // Pas possible
            break;
        case FILTER_MARKEDFILE:
            eMFEObject.markedFile(0, null);
            break;
        case FILTER_RANDOM:
            // TODO - Pas possible ?
            break;
        case FILTER_ADV:
            cancelAdvFlt(fTab);
            break;
    }
}
/****************************************************************/
/** Fin Fonctions de ToolTip                                   **/
/****************************************************************/

/*  Filtre Histo */
function doHistoFilter(bFromWidget) {

    var objHistoFilter = document.getElementById("histoFilter");
    if (!objHistoFilter)
        return;

    var nHistoStatus = objHistoFilter.getAttribute("ednval");
    var objDisplyText = findElementById(objHistoFilter, "histoFilterTxt"); //eTools

    // Inverser click
    if (nHistoStatus == "1") {
        objDisplyText.innerHTML = objHistoFilter.getAttribute("ednLibShow");
        objHistoFilter.setAttribute("ednval", 0);
        objHistoFilter.className = "histoFilter";
        nHistoStatus = 0;
    }
    else {
        objDisplyText.innerHTML = objHistoFilter.getAttribute("ednLibHide");
        objHistoFilter.setAttribute("ednval", 1);
        objHistoFilter.className = "histoFilter histoActive";
        nHistoStatus = 1;
    }

    if (bFromWidget) {

        if (oListWidget) {
            oListWidget.setHisto((nHistoStatus == 1));
        }
        return;
    }

    // par defaut c'est mainlist
    var type = getAttributeValue(objHistoFilter, "type");
    switch (type) {
        case "finder":
            StartSearch();
            break;
        case "mainlist":
        default:
            var updatePref = "tab=" + nGlobalActiveTab + ";$;histo=" + nHistoStatus;
            updateUserPref(updatePref, function () { loadList(); });
            break;
    }
}


/****************************************************************/
/** Debut Fonctions de Filtre rapide                           **/
/****************************************************************/
function doQuickFilter(oNode) {
    var nDescId = oNode.id.replace("QuickF_", "");
    var nIndex = oNode.getAttribute("ednIdx");

    if (oNode.selectedIndex < 0)
        return;

    var sValue = oNode.options[oNode.selectedIndex].value;

    //selection de Tous
    if (sValue == '-1')
        removeClass(oNode, 'activeQF');
    else
        addClass(oNode, 'activeQF');

    // Maj Value
    sValue = "type=0;$;tab=" + nGlobalActiveTab + ";$;descid=" + nDescId + ";$;index=" + nIndex + ";$;value=" + sValue;
    // Aucune action
    updateUserValue(sValue, function () { loadList(); }, function () { });
}

function onSetQuickUserAbort(jsVarName) {
    var catalogObject = window[jsVarName];
    var modalUserCat = catalogObject.advancedDialog;
    var obj = modalUserCat.TargetObject;
    modalUserCat.hide();
    quickUserSelectItem(obj);
}

function onSetQuickUserOk(jsVarName) {
    var modalObject = window[jsVarName];
    var vals;
    var libs;

    if (modalObject) {
        var oCatEditor = modalObject.advancedDialog;
        var strReturned = oCatEditor.getIframe().GetReturnValue();
        oCatEditor.hide();

        vals = strReturned.split('$|$')[0];
        libs = strReturned.split('$|$')[1];
    }

    try {
        var oeParam = document.getElementById('eParam').contentWindow;
        oeParam.AddMenuUserAdv(vals, libs);
    }
    catch (e) { }

    // Met à jour en base MenuUserId et raffraichi la liste
    quickUserSave(vals, obj);
    // Ajoute les anciennes valeurs adv ajoutées
    quickUserRefreshCombo();
}

// KHA - Filtre rapide user avec recherche activée
function doQckUsrFltSrch(oNode) {
    //doUserCatalogAdv(oNode, onSetQckUsrSrchOk);
    eCatalogUserEditorObject.OpenUserDialog(oNode, onSetQckUsrSrchOk);
}

function onSetQckUsrSrchOk(jsVarName) {
    var vals;
    var libs;
    var modalObject = window[jsVarName];
    try {
        var oCatEditor = modalObject.advancedDialog;
        var strReturned = oCatEditor.getIframe().GetReturnValue();
        oCatEditor.hide();

        var vals = strReturned.split('$|$')[0];
        var libs = strReturned.split('$|$')[1];
        var nQckFltDescId = getNumber(modalObject.GetSourceElement().id.split("_")[1]);
        // Met à jour en base MenuUserId et raffraichi la liste
        saveQckUsrFltFromList(nGlobalActiveTab, vals, libs, nQckFltDescId);
    }
    catch (e) {
        eAlert(0, top._res_727, top._res_5179);
    }
}

function quickUserSave(selValues, oSelNode) {
    // Met à jour l'attribut dbv du select
    oSelNode.setAttribute('dbv', selValues);

    // Maj Value
    sValue = "tab=" + nGlobalActiveTab + ";$;menuuserid=" + selValues;
    updateUserPref(sValue, function () { firstpage(nGlobalActiveTab); });
}

function quickUserSelectItem(oComboQuickUser) {
    // Parcours les option pour selectionner la valeur
    for (var i = 0; i < oComboQuickUser.length; i++) {
        var oOpt = oComboQuickUser[i];

        if (oOpt.value == oComboQuickUser.getAttribute("dbv")) {
            oComboQuickUser.selectedIndex = i;
            break;
        }
    }
}

function quickUserRefreshCombo() {
    var oeParam = document.getElementById('eParam').contentWindow;
    var oLstQuickFilter = document.getElementById('listQuickFilters');

    if (!oeParam || !oLstQuickFilter)
        return;

    var oeParamDoc = oeParam.document;
    var oFirstVal = oeParamDoc.getElementById('MenuUserListAdvVal_0');

    // Si pas de valeur en param, on ne fait rien
    if (!oFirstVal || oFirstVal.value == '')
        return;

    try {
        // Recherche la combo du QuickUserFilter
        var oComboQuickUser = null;
        var oLstCombo = oLstQuickFilter.getElementsByTagName('select');
        forEach(oLstCombo, function (oSource) {
            if (oSource.id.indexOf("QuickF_") == 0 && oSource.getAttribute("dbv"))
                oComboQuickUser = oSource;
        });

        // Supprime toutes les anciennes valeurs
        for (var i = oComboQuickUser.length - 1; i >= 0; i--) {
            if (oComboQuickUser[i].getAttribute("ednAdv") == "1") {
                oComboQuickUser.options[i] = null;
            }
        }

        // Ajoute les valeurs
        var idxOptionAdv = 2;       // En dure égal à 2 ?
        for (var i = 0; i < nNbrMaxAdvancedUsr; i++) {
            var oVal = oeParamDoc.getElementById('MenuUserListAdvVal_' + i);
            var oLib = oeParamDoc.getElementById('MenuUserListAdvLbl_' + i);

            if (oVal.value == '')
                continue;

            var oOption = document.createElement('option');
            oOption.text = '<...> ' + oLib.value;
            oOption.setAttribute('title', oLib.value);
            oOption.setAttribute('ednAdv', '1');
            oOption.value = oVal.value;

            var oOptBefore = oComboQuickUser.options[idxOptionAdv + i];
            try {
                oComboQuickUser.add(oOption, oOptBefore);          // Standards compliant; doesn't work in IE
            }
            catch (ex) {
                oComboQuickUser.add(oOption, idxOptionAdv + i);       // IE only
            }
        }

        quickUserSelectItem(oComboQuickUser);
    }
    catch (e) { }
}
/****************************************************************/
/** Fin Fonctions de Filtre rapide                           **/
/****************************************************************/
// si "lstDescIdRuleUpdated" est indéfini, la liste est rechargée
function ReloadList(lstDescIdRuleUpdated) {
    var cellDescId, cellTableDid;
    var reloadList = false;

    var oTable = document.getElementById("mt_" + nGlobalActiveTab);

    //Pas de liste affichée (cas de nouveau ou recherche depuis HOMEPAGE par exemple)
    if (oTable == null)
        return;

    if (lstDescIdRuleUpdated != null && typeof (lstDescIdRuleUpdated) != 'undefined') {

        // Safari doesn't support table.tHead, sigh
        if (oTable.tHead == null)
            oTable.tHead = oTable.getElementsByTagName('thead')[0];

        if (oTable.tHead.rows.length == 0)
            return;

        var row = oTable.tHead.rows[0];
        for (i = 0; i < row.cells.length; i++) {
            var oTh = row.cells[i];

            if (oTh.id.indexOf("COL_") != 0)
                continue;

            cellDescId = parseInt(getAttributeValue(oTh, "did"));
            if (isNaN(cellDescId))
                continue;

            cellTableDid = cellDescId - cellDescId % 100;

            if (lstDescIdRuleUpdated.indexOf(cellDescId + '') != -1 || lstDescIdRuleUpdated.indexOf(cellTableDid + '') != -1) {
                reloadList = true;
                break;
            }
        }

        if (!reloadList)
            return false;
    }

    var cPage = Number(oTable.getAttribute("cPage"));
    loadList(cPage, true);
    return true;
}

//Principe: chaque div contenant un bouton "dans" le champ est dotée de la classe css divct_ + FieldAlias 
//cette classe est modifiée par javascript en fonction de la largeur de la colonne et de la largeur du bouton.
function adjustBtnFieldWidth(objTh) {
    //KHA redimensionnement des champs contenant un bouton
    var oRuleLine = getCssSelector("customCss", ".divct_" + objTh.id.replace("COL_", ""));
    var oRuleBtn = getCssSelector("eList.css", ".btn");

    if (oRuleLine && oRuleBtn) {
        oRuleLine.style.width = (objTh.width.replace("px", "") - oRuleBtn.style.width.replace("px", "") - 10) + "px";
        // le -10 est du aux margin right et left sur la cellule
    }
}





/**************************************************************************/
//sens de lecture de la discussion
/**************************************************************************/
function sdo(obj) {
    setDiscOrder(obj)
}

function setDiscOrder(obj) {
    var ids = obj.id.split('_');
    if (ids.length < 3)
        return;
    var bkm = ids[ids.length - 2];
    var descid = ids[ids.length - 1];
    var sort = ids[ids.length - 3] == "DESC" ? "1" : "0";
    var updatePref = "tab=" + nGlobalActiveTab
        + ";$;bkm=" + bkm
        + ";$;listsort=" + descid + ";$;listorder=" + sort;
    updateUserBkmPref(updatePref, function () { loadBkm(bkm); });
}