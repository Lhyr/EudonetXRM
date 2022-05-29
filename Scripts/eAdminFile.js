/// <summary>Nombre de colonnes sur les en-têtes pour PP</summary>
var NB_LINE_HEAD_PP = 2;
/// <summary>Nombre de colonnes sur les en-têtes pour PM</summary>
var NB_LINE_HEAD_PM = 5;

var nsAdminFile = {};
nsAdminFile.debug = false;
nsAdminFile.isInit = false;


function initAdminFile(event) {

    event = event || window.event;

    //pare le pb provoqué par l'opération similaire effectuée dans eHomePage.js
    document.onmousedown = null;
    document.onmouseup = null;
    document.onclick = null;
    nsAdminFile.browser = new getBrowser();

    var aCellClasses = ["table_labels", "table_values", "btn", "bkmHead"];
    for (var j = 0; j < aCellClasses.length; j++) {
        var cells = document.querySelectorAll("td." + aCellClasses[j]);
        for (var i = 0; i < cells.length; i++) {
            cells[i].addEventListener('mouseover', function () { selectCell(this, true); });
            cells[i].addEventListener('mouseout', function () { selectCell(this, false); });
        }
    }

    // Au clic sur le libellé
    var labels = document.querySelectorAll(".mTabFile .table_labels");
    for (var k = 0; k < labels.length; k++) {
        labels[k].addEventListener("click", function (event) {

            if (event.target.id != "resumeBreakLine") {
                var td = findUp(this, "TD");
                var sDid = getAttributeValue(td, "did");

                if (sDid != "" && this.tagName != "INPUT")
                    clickLabel(sDid, this);
            }

        });
    }

    var bkmDtls = document.querySelector(".bkmDtls");
    if (bkmDtls)
        bkmDtls.addEventListener("mousedown", function (event) { nsAdmin.loadAdminFile(nGlobalActiveTab); });

    // Au clic sur l'icône "configuration"
    var configOp = document.querySelectorAll(".mTabFile .configOption, .bkmHead .configOption, .bkmHead > a, #tabAdminFileProp .configOption");
    for (var k = 0; k < configOp.length; k++) {
        configOp[k].addEventListener("mousedown", function (event) {

            var nEdnType, sDid, iDid;

            var ulFieldOptions = (this.tagName == "A") ? this.nextSibling : this.parentElement;

            var tdElement = ulFieldOptions.parentElement;

            deselectCells();
            selectCell(tdElement, true, "active");

            nEdnType = getAttributeValue(ulFieldOptions, "edntype") + "";
            sDid = getAttributeValue(ulFieldOptions, "did");
            iDid = getNumber(sDid.replace("bkm", ""));

            nsAdminGrid.removeBkmGrid(nEdnType);

            if (nEdnType == "16") {
                nsAdminBkmWeb.editBkmWebProperties(iDid, event);
            }
            else if (nEdnType == "24") {
                nsAdminGrid.editBkm(iDid);
            }
            else if (sDid.match("^bkm") && !sDid.match("94$") && !sDid.match("89$")) {
                nsAdminBkm.editBkmProperties(iDid);
            }
            else {
                nsAdminField.editFieldProperties(iDid, this);
            }

            event.stopPropagation();
        });
    }

    // Au clic sur l'icône "supprimer"
    var delOp = document.querySelectorAll(".mTabFile .deleteOption");
    for (var k = 0; k < delOp.length; k++) {
        delOp[k].addEventListener("mousedown", function () {
            var activeTd = document.querySelectorAll(".mTabFile .active");
            for (var i = 0; i < activeTd.length; i++) {
                selectCell(activeTd[i], false, "active");
            }

            var td = findUp(this, "TD");
            selectCell(td, true, "active");
            var descid = getNumber(getAttributeValue(this.parentElement, "did"));
            var disporder = getNumber(getAttributeValue(td, "edo"));
            if (descid)
                nsAdminField.dropField(this, descid);
            else if (td.className.indexOf("free") > -1)
                nsAdminField.dropSpace(disporder);
            event.stopPropagation();
        });
    }



    nsAdminFile.initDefaultValueHandler();
    nsAdminFile.initBkmBarMove();

    initMemoFields(nGlobalActiveTab, 3);

    /// Abonnement à l'evenement onKeyDown
    document.addEventListener('keydown', nsAdmin.onKeyDown);
    nsAdminFile.isInit = true;

    // Initialisation des événements sur les dropzones de la resume breakline
    oResume.init();
}

function deselectCells() {

    var activeTd = document.querySelectorAll(".mTabFile .active, #tabAdminFileProp .active, #ulWebTabs .selectedTab, .bkmHead.active");
    var activeElement;
    for (var i = 0; i < activeTd.length; i++) {
        activeElement = activeTd[i];

        if (activeElement.className.indexOf("selectedTab") <= -1)
            selectCell(activeElement, false, "active");
        else
            // pour désactiver la selection au sous-onglet web
            removeClass(activeElement, "selectedTab");
    }
}

function selectCell(element, bMouseOver, className) {
    if (!element)
        return;

    if (!nsAdminFile.isInit) {
        initAdminFile();
    }

    var cellpos = "";
    var parent;

    if (!className)
        className = "selected";

    parent = element.parentElement;
    cellpos = element.getAttribute("cellpos");

    //var eltTable = findUp(parent, "table");
    //if (getAttributeValue(eltTable, "fmt") == "head") {

    //}

    if (!cellpos)
        return;

    var cellsToSelect = parent.querySelectorAll("[cellpos='" + cellpos + "']");
    for (var j = 0; j < cellsToSelect.length; j++) {
        if (bMouseOver)
            addClass(cellsToSelect[j], className);
        else
            removeClass(cellsToSelect[j], className);

        if (cellsToSelect[j].classList.contains("table_values") || cellsToSelect[j].classList.contains("table_labels") || cellsToSelect[j].classList.contains("bkmHead")) {
            var optionsBlock = cellsToSelect[j].querySelector(".fieldOptions");
            if (optionsBlock != null) {
                if (bMouseOver)
                    addClass(optionsBlock, className);
                else
                    removeClass(optionsBlock, className);

                // On vérifie si, après affichage de la barre d'outils par application de la classe CSS fieldOptions, si la barre dispose de suffisamment d'espace pour
                // s'afficher correctement. Si ce n'est pas le cas (overflow détecté par isOverflowed), on rajoute une classe pour faire passer la barre d'outils sur une
                // seconde ligne. Il faut passer à la fonction une marge de tolérance (en pixels) pour tenir compte des disparités de calcul entre navigateurs (IE, notamment)
                // ici, la marge correspond plus ou moins aux différences d'interprétation constatées entre IE 11 et Chrome, soit 2 pixels.
                // Mais on ne l'utilise qu'avec IE et Edge (sic), qui semblent être les seuls à présenter des disparités entre client*/offset*/scroll* lorsqu'on applique la classe fieldOptions
                // On pourrait aussi appliquer une marge équivalente à la taille d'un bouton de la barre d'outils, soit 24 pixels environ.
                var overflowedClassName = 'fieldOptionsOverflowed';
                //#67127 - Passe la marge de 2 à 4 pour IE et Edge
                var overflowMargin = nsAdminFile.browser.isIE || nsAdminFile.browser.isEdge ? 4 : 0;
                if (eTools.isOverflowed(cellsToSelect[j], overflowMargin))
                    addClass(optionsBlock, overflowedClassName);
                else
                    removeClass(optionsBlock, overflowedClassName);
            }
        }
    }

}

function clickLabel(descid, element) {
    nsAdminField.selectField(element);
    nsAdminField.editFieldProperties(descid, element, null, true);
}

//******************************Déplacement de signet/onglet web *****************************************
var nsAdminMoveWebTab = {};
nsAdminMoveWebTab.debug = false;

//******************************Déplacement de champ *****************************************
//raccourci
function fmd(e) {
    var hdElt = e.target || e.srcElement;
    nsAdminMoveField.onFieldMouseDown(e, hdElt, false);
}

//Space DragStart
function sds(e, hdElt) {
    if (nsAdminFile.browser.isFirefox)
        e.dataTransfer.setData("text", hdElt);
    nsAdminMoveField.onFieldMouseDown(e, hdElt, true);
    if (getAttributeValue(hdElt, "wsr") == "1")
        nsAdminMoveField.WholeSpaceRow = true;
}

//New Field
function cds(e, hdElt) {
    if (nsAdminFile.browser.isFirefox)
        e.dataTransfer.setData("text", hdElt);
    nsAdminMoveField.Create = true;
    if (getAttributeValue(hdElt, "ust") == "1")
        nsAdminMoveField.UnspecifiedType = true;
    nsAdminMoveField.onFieldMouseDown(e, hdElt, true);
}

function ofm(e) {
    nsAdminMoveField.onFieldMove(e);
}

function ofmu(e) {
    nsAdminMoveField.onFieldMoveEnd(e);
}

var nsAdminMoveField = {};
nsAdminMoveField.debug = false;
nsAdminMoveField.Create = false;
nsAdminMoveField.UnspecifiedType = false;
nsAdminMoveField.WholeSpaceRow = false;
nsAdminMoveField.DropSpaceRow = false;
nsAdminMoveField.SpecField = null;

nsAdminMoveField.onFieldMouseDown = function (e, hdElt, bNew) {

    if (nsAdminMoveField.debug) {
        console.log("---------------------Begin nsAdminMoveField.onFieldMouseDown-----------------------");
        console.log("e : ");
        console.log(e);
        console.log("hdElt : ");
        console.log(hdElt);
        console.log("bNew : ");
        console.log(bNew);
    }

    /// initialisation
    if (!hdElt)
        return;

    if (getAttributeValue(hdElt, "action") == "dropRow")
        return;


    // On court-circuite le déplacement de champ si on clique sur un élément comportant un évènement onclick
    // et on déclenche la fonction câblée au clic à la place
    // Exemple : petites étoiles/astérisques vertes ouvrant la fenêtre des automatismes avancés pour les super-administrateurs
    var hoveredElements = eTools.getElementsUnderMouseCursor(e);
    for (var i = 0; i < hoveredElements.length; i++) {
        // Remarque : on effectue pour l'instant ce traitement dans le seul cas particulier de l'astérisque verte
        // indiquant la présence d'une formule sur un champ, mais on pourrait tout aussi bien faire le choix de
        // déclencher le clic de n'importe quel autre élément, si le clic est toujours prioritaire sur le
        // déplacement de champ
        if (hoveredElements[i].className == "formulaAst" && typeof (hoveredElements[i].onclick) == "function") {
            hoveredElements[i].click();
            return;
        }
        if (hoveredElements[i].id == "resumeBreakline") {
            return;
        }
    }

    nsAdmin.initDragDrop(this);

    nsAdminMoveField.initPositions();
    nsAdminMoveField.New = bNew;

    if (nsAdminMoveField.debug) { console.log("new : " + nsAdminMoveField.New); }
    if (bNew) {
        nsAdminMoveField.origHeaderElt = hdElt;
        nsAdminMoveField.origDisporder = 90;
        nsAdminMoveField.origDescid = nGlobalActiveTab;
    }
    else {
        nsAdminMoveField.origHeaderElt = findUp(hdElt, "TD");
        //nsAdminMoveField.origHeaderElt = hdElt;
        nsAdminMoveField.origCells = nsAdminMoveField.origHeaderElt.parentElement.querySelectorAll("[cellpos='" + getAttributeValue(nsAdminMoveField.origHeaderElt, "cellpos") + "']");
        nsAdminMoveField.origDisporder = getNumber(getAttributeValue(nsAdminMoveField.origHeaderElt, "edo"));
        if (hasClass(nsAdminMoveField.origHeaderElt, "free"))
            nsAdminMoveField.origDescid = nGlobalActiveTab;
        else
            nsAdminMoveField.origDescid = getNumber(getAttributeValue(nsAdminMoveField.origHeaderElt, "did"));
        nsAdminMoveField.origCellPos = getAttributeValue(nsAdminMoveField.origHeaderElt, "cellpos");
        // Cas où on est dans l'entête
        if (getAttributeValue(nsAdminMoveField.origHeaderElt, "colindex") != "")
            nsAdminMoveField.isHeaderField = true;

        if (!hasClass(nsAdminMoveField.origHeaderElt, "free")) {
            // Focus du champ libellé
            clickLabel(nsAdminMoveField.origDescid, nsAdminMoveField.origHeaderElt);
        }
        else {
            // Cas d'un champ libre : on affiche le 1er onglet du menu
            nsAdminField.selectField(nsAdminMoveField.origHeaderElt);
            nsAdmin.showBlock('paramTab1');
            var title = document.querySelector("#partFileFields header");
            if (title) {
                nsAdmin.showHidePart(title, true);
            }
        }
    }

    if (nsAdminMoveField.debug) { console.log(nsAdminMoveField.origDisporder + " - nsAdminMoveField.origDescid : " + nsAdminMoveField.origDescid + " - nsAdminMoveField.isHeaderField : " + nsAdminMoveField.isHeaderField); }
    if ((!nsAdminMoveField.origDisporder || !nsAdminMoveField.origDescid) && !nsAdminMoveField.isHeaderField) {
        if (nsAdminMoveField.debug) { console.log("Information manquante : nsAdminMoveField.origDisporder : " + nsAdminMoveField.origDisporder + " - nsAdminMoveField.origDescid : " + nsAdminMoveField.origDescid + " - nsAdminMoveField.isHeaderField : " + nsAdminMoveField.isHeaderField); }
        nsAdminMoveField.reset();
        return;
    }

    //si ne marche pas sur le dernier IE check dragOpt.eventPosition dans eDrag.js
    var pos = nsAdminFile.getPos(e);

    nsAdminMoveField.cursorStartX = pos.x;
    nsAdminMoveField.cursorStartY = pos.y;

    if (!bNew) { //CAD si on n'est pas en train de déplacer une rubrique depuis le menui de droite auquel cas tout est géré par drag & drop
        nsAdminMoveField.createShadow();
        setEventListener(document, 'mousemove', nsAdminMoveField.onFieldMove, true);
        setEventListener(document, 'mouseup', nsAdminMoveField.onFieldMoveEnd, true);
    }
    else if (nsAdminFile.browser.isFirefox) {
        //FF n'expose pas clientX et clientY lors du drag d'un element : https://bugzilla.mozilla.org/show_bug.cgi?id=505521
        //On passe donc par une fonction intermédiaire pour récupérer les coordonées
        setEventListener(document, 'dragover', nsAdminFile.getDocPos, true);
    }

};


nsAdminMoveField.createShadow = function () {
    //debugger;
    var shadowWidth = 3;
    var shadowHeight = nsAdminMoveField.origHeaderElt.offsetHeight;
    for (var i = 0; i < nsAdminMoveField.origCells.length; i++) {
        var cell = nsAdminMoveField.origCells[i];
        shadowWidth += cell.offsetWidth;
    }

    nsAdminMoveField.shadow = document.createElement("div");
    addClass(nsAdminMoveField.shadow, "shadow");
    document.documentElement.appendChild(nsAdminMoveField.shadow);

    var obj_pos = getAbsolutePosition(nsAdminMoveField.origHeaderElt);
    nsAdminMoveField.shadow.style.position = "absolute";
    nsAdminMoveField.shadow.style.left = obj_pos.x + "px";
    nsAdminMoveField.shadow.style.top = obj_pos.y + "px";
    nsAdminMoveField.shadow.style.width = shadowWidth + "px";
    nsAdminMoveField.shadow.style.height = shadowHeight + "px";

    nsAdminMoveField.elStartLeft = parseInt(obj_pos.x, 10);
    nsAdminMoveField.elStartTop = parseInt(obj_pos.y, 10);
};


nsAdminMoveField.onFieldMove = function (event) {

    if (nsAdminMoveField.debug) { console.log("------------------Begin onFieldMove----------------------"); }

    //si ne marche pas sur le dernier IE check dragOpt.eventPosition dans eDrag.js
    var pos = nsAdminFile.getPos(event);

    if (nsAdminMoveField.debug) {
        console.log(event);
        console.log(pos);
        console.log('nsAdminMoveField.posTab1'); console.log(nsAdminMoveField.posTab1);
        console.log('nsAdminMoveField.posTab2'); console.log(nsAdminMoveField.posTab2);
        console.log('nsAdminMoveField.posTabHeader'); console.log(nsAdminMoveField.posTabHeader);
    }

    if (nsAdminMoveField.shadow) {
        // Move drag element by the same amount the cursor has moved.
        var style = nsAdminMoveField.shadow.style;
        style.left = (nsAdminMoveField.elStartLeft + pos.x - nsAdminMoveField.cursorStartX) + "px";
        style.top = (nsAdminMoveField.elStartTop + pos.y - nsAdminMoveField.cursorStartY) + "px";
    }

    //pour déplacer un champ plus facilement sur de grands intervalles on scrolle automatiquement
    if (nsAdminMoveField.tab2) {
        try {
            if (nsAdminMoveField.debug) { console.log(pos.y + " " + nsAdminMoveField.DivBkmPres.offsetTop + " " + nsAdminMoveField.DivBkmPres.offsetTop); }
            if (pos.y >= nsAdminMoveField.DivBkmPres.offsetTop - 15
                && pos.y <= nsAdminMoveField.DivBkmPres.offsetTop + 15) {
                if (nsAdminMoveField.debug) { console.log("on scroll vers le haut"); }
                nsAdminMoveField.DivBkmPres.scrollTop += -5;
            }
            else if (pos.y >= nsAdminMoveField.DivBkmPres.offsetTop + nsAdminMoveField.DivBkmPres.offsetHeight - 10
                && pos.y <= nsAdminMoveField.DivBkmPres.offsetTop + nsAdminMoveField.offsetHeight + 10) {
                if (nsAdminMoveField.debug) { console.log("on scroll vers le bas"); }
                nsAdminMoveField.DivBkmPres.scrollTop += 5;
            }
        }
        catch (e) {
            console.log("Oups! le scroll auto a planté : " + e.message);
        }

    }

    var bTab1 = isPointInArea(pos, nsAdminMoveField.posTab1);
    var bTab2 = isPointInArea(pos, nsAdminMoveField.posTab2);
    var bTabHeader = isPointInArea(pos, nsAdminMoveField.posTabHeader);

    if (nsAdminMoveField.debug) { console.log(bTab1 + " - " + bTab2); }

    if (!bTab1 && !bTab2 && !bTabHeader) {
        selectCell(nsAdminMoveField.destCell, false);
        nsAdminMoveField.resetDestination();
        if (nsAdminMoveField.debug) {
            console.log("--Ceci n'est pas une cellule");
            console.log("------------------End onFieldMove----------------------");
        }
        return;
    }

    var oTab;
    var cells;
    if (bTab1) {
        oTab = nsAdminMoveField.tab1;
        cells = nsAdminMoveField.cells1;
    }
    else if (bTab2) {
        oTab = nsAdminMoveField.tab2;
        cells = nsAdminMoveField.cells2;
    }
    else if (bTabHeader) {
        oTab = nsAdminMoveField.tabHeader;
        cells = nsAdminMoveField.cellsHeader;
    }

    var oCell;
    for (var i = 0; i < cells.length; i++) {
        var posCell = getAbsolutePosition(cells[i]);

        if (nsAdminMoveField.debug) { console.log(posCell); }

        if (isPointInArea(pos, posCell)) {
            oCell = cells[i];
            break;
        }
    }
    if (nsAdminMoveField.debug) { console.log("--CELLULE DESTINATAIRE--"); console.log(oCell); }
    if (!oCell) {
        selectCell(nsAdminMoveField.destCell, false);
        nsAdminMoveField.resetDestination();
        if (nsAdminMoveField.debug) {
            console.log("--Ceci n'est pas une cellule");
            console.log("------------------End onFieldMove----------------------");
        }
        return;
    }

    if (oCell != nsAdminMoveField.destCell) {

        selectCell(nsAdminMoveField.destCell, false);
        nsAdminMoveField.destCell = oCell;
        selectCell(nsAdminMoveField.destCell, true);
    }

    if (nsAdminMoveField.debug) { console.log("------------------End onFieldMove----------------------"); }

};

nsAdminMoveField.onFieldMoveEnd = function (e) {
    if (nsAdminMoveField.debug) { console.log("------------------Begin onFieldMoveEnd----------------------"); }

    if (nsAdminMoveField.shadow)
        document.documentElement.removeChild(nsAdminMoveField.shadow);
    // cancel mousemove and mouseup events 
    if (!nsAdminMoveField.New) {
        unsetEventListener(document, 'mousemove', nsAdminMoveField.onFieldMove, true);
        unsetEventListener(document, 'mouseup', nsAdminMoveField.onFieldMoveEnd, true);
    }
    else if (nsAdminFile.browser.isFirefox) {
        //FF n'expose pas clientX et clientY lors du drag d'un element : https://bugzilla.mozilla.org/show_bug.cgi?id=505521
        //On passe donc par une fonction intermédiaire pour récupérer les coordonées
        unsetEventListener(document, 'dragover', nsAdminFile.getDocPos, true);
    }


    if (!nsAdminMoveField.destCell || getAttributeValue(nsAdminMoveField.destCell, "cellpos") == "") {
        if (nsAdminMoveField.debug) { console.log("--La destination n'est pas une cellule"); }
        nsAdminMoveField.reset();
        if (nsAdminMoveField.debug) { console.log("------------------End onFieldMoveEnd----------------------"); }
        return;
    }

    nsAdminMoveField.destHeaderElt = nsAdminMoveField.destCell.parentElement.querySelector("[cellpos='" + getAttributeValue(nsAdminMoveField.destCell, "cellpos") + "'][edo]");
    nsAdminMoveField.destDisporder = getAttributeValue(nsAdminMoveField.destHeaderElt, "edo");
    nsAdminMoveField.destCellPos = getAttributeValue(nsAdminMoveField.destHeaderElt, "cellpos");
    aCellPos = nsAdminMoveField.destCellPos.split(';');
    if (aCellPos.length == 2) {
        nsAdminMoveField.destCellPosX = aCellPos[0];
        nsAdminMoveField.destCellPosY = aCellPos[1];
    }
    if (nsAdminMoveField.origHeaderElt == nsAdminMoveField.destHeaderElt) {
        if (nsAdminMoveField.debug) { console.log("--Les positions de départ et de destination sont identiques"); }
        nsAdminMoveField.reset();
        if (nsAdminMoveField.debug) { console.log("------------------End onFieldMoveEnd----------------------"); }
        return;
    }
    if (nsAdminMoveField.debug) { console.log("--Appel du manager"); console.log(nsAdminMoveField); }

    if (nsAdminMoveField.isHeaderField) {

        if (nsAdminMoveField.debug) {
            console.log("--ORIG CELLPOS : " + nsAdminMoveField.origCellPos);
            console.log("--DEST CELLPOS : " + nsAdminMoveField.destCellPos);
        }

        if (nsAdminMoveField.origCellPos == nsAdminMoveField.destCellPos) {
            if (nsAdminMoveField.debug) { console.log("--Reset"); }
            nsAdminMoveField.reset();
            if (nsAdminMoveField.debug) { console.log("------------------End onFieldMoveEnd----------------------"); }
            return;
        }

        var updMoveHead = new eUpdater("eda/mgr/eMoveHeaderFieldManager.ashx", 0);
        updMoveHead.addParam("tab", nGlobalActiveTab, "post");
        updMoveHead.addParam("origdescid", nsAdminMoveField.origDescid, "post");
        updMoveHead.addParam("origcellpos", nsAdminMoveField.origCellPos, "post");
        updMoveHead.addParam("destcellpos", nsAdminMoveField.destCellPos, "post");
        updMoveHead.send(function (oRes) {
            nsAdmin.loadContentModule(USROPT_MODULE_ADMIN_TAB, null, { tab: nGlobalActiveTab });
        });
    }
    else if (nsAdminMoveField.Create) {
        if (nsAdminMoveField.UnspecifiedType) {
            nsAdminMoveField.addNewField();
            return;
        }
        else {
            nsAdminMoveField.LaunchCreation();
        }
    }
    else {
        var updDispOrder = new eUpdater("eda/mgr/eMoveFieldManager.ashx", 0);
        updDispOrder.addParam("origdescid", nsAdminMoveField.origDescid, "post");
        updDispOrder.addParam("origdisporder", nsAdminMoveField.origDisporder, "post");
        updDispOrder.addParam("destdisporder", nsAdminMoveField.destDisporder, "post");
        updDispOrder.addParam("destx", nsAdminMoveField.destCellPosX, "post");
        updDispOrder.addParam("desty", nsAdminMoveField.destCellPosY, "post");

        if (e && e.ctrlKey)
            updDispOrder.addParam("modeswap", 1, "post");

        if (nsAdminMoveField.WholeSpaceRow)
            updDispOrder.addParam("wsr", 1, "post");

        updDispOrder.ErrorCallBack = nsAdminMoveField.reset;
        updDispOrder.send(function (oRes) {
            nsAdmin.loadContentModule(USROPT_MODULE_ADMIN_TAB, null, { tab: getTabDescid(nsAdminMoveField.origDescid) });
        });
    }

    nsAdminMoveField.ScrollTop = nsAdminMoveField.DivBkmPres.scrollTop;
    if (nsAdminMoveField.debug) { console.log("nsAdminMoveField.ScrollTop : " + nsAdminMoveField.ScrollTop); }

    if (nsAdminMoveField.debug) { console.log("--Reset"); }
    nsAdminMoveField.reset();
    if (nsAdminMoveField.debug) { console.log("------------------End onFieldMoveEnd----------------------"); }
};

nsAdminMoveField.addNewField = function () {
    var modNewField = new eModalDialog(top._res_6929, 0, "eda/mgr/eAdminNewFieldDialog.ashx", 500, 200, "modNewField");
    var fileDiv = document.getElementById("fileDiv_" + nGlobalActiveTab);

    if (!nsAdminMoveField.destDisporder) {
        var oAvailableSlotsContainer = document/*.getElementById("ftdbkm_" + nsAdmin.tab)*/;
        if (oAvailableSlotsContainer) {
            var oAvailableSlots = oAvailableSlotsContainer.querySelectorAll("td[class^='free']");
            if (oAvailableSlots && oAvailableSlots.length > 0)
                nsAdminMoveField.destDisporder = getAttributeValue(oAvailableSlots[0], "edo");
        }
    }

    modNewField.addParam("tab", nGlobalActiveTab, "post");
    modNewField.addParam("tt", getAttributeValue(fileDiv, "tt"), "post");

    modNewField.noButtons = false;
    modNewField.hideMaximizeButton = true;
    modNewField.addParam("iframeScrolling", "no", "post");
    modNewField.NoScrollOnMainDiv = true;
    modNewField.show();
    modNewField.addButton(top._res_30, function () { modNewField.hide(); nsAdminMoveField.reset(); }, 'button-gray', null);
    modNewField.addButton(top._res_28, function () { nsAdminMoveField.LaunchSpecifiedCreation(modNewField); modNewField.hide(); nsAdminMoveField.reset(); }, 'button-green', null);
}

nsAdminMoveField.LaunchSpecifiedCreation = function (modNewField) {
    if (!modNewField)
        return;

    nsAdminMoveField.SpecField = modNewField.getIframe().nsAdminNewFieldDialog.getSpecField();
    nsAdminMoveField.LaunchCreation();

};

nsAdminMoveField.LaunchCreation = function () {

    top.setWait(true);

    var updDispOrder = new eUpdater("eda/mgr/eAdminFieldPropertiesManager.ashx", 1);
    updDispOrder.addParam("action", nsAdminField.FieldManagerAction.CREATEFIELD, "post");
    updDispOrder.addParam("disporder", nsAdminMoveField.destDisporder, "post");
    updDispOrder.addParam("col", getAttributeValue(nsAdminMoveField.destCell, "cellpos").split(";")[0], "post");

    updDispOrder.addParam("tab", nGlobalActiveTab, "post");

    if (nsAdminMoveField.SpecField) {
        updDispOrder.addParam("label", nsAdminMoveField.SpecField.label, "post");
        updDispOrder.addParam("descid", nsAdminMoveField.SpecField.did, "post");
        var caps = nsAdminMoveField.SpecField.fieldType;
        for (var i = 0; i < caps.ListProperties.length; i++) {
            var pty = caps.ListProperties[i];
            var name = pty.Category + "_" + pty.Property;
            var value = pty.Value;
            updDispOrder.addParam(name, value, "post");
        }
    }
    else {
        var re = new RegExp('^[0-9]+_[0-9]+$');
        for (var i = 0; i < nsAdminMoveField.origHeaderElt.attributes.length; i++) {
            var name = nsAdminMoveField.origHeaderElt.attributes[i].name;
            var value = nsAdminMoveField.origHeaderElt.attributes[i].value;
            if (name.match(re)) {
                updDispOrder.addParam(name, value, "post");
            }
        }
    }
    updDispOrder.send(function (oRes) { nsAdminMoveField.afterCreate(oRes, updDispOrder); });

};

nsAdminMoveField.afterCreate = function (oRes) {
    var result = JSON.parse(oRes);


    if (!result.Success) {
        if (result.Criticity == 2)
            top.eAlert(result.Criticity, top._res_7358, result.UserErrorMessage);
        else
            top.eAlert(1, top._res_416, result.UserErrorMessage);

        return;
    }

    nsAdminMoveField.ScrollTop = nsAdminMoveField.DivBkmPres.scrollTop;
    nsAdmin.loadContentModule(USROPT_MODULE_ADMIN_TAB, null, { tab: getTabDescid(result.Descid) });

    //dans le cas du dépot d'un rubrique de type aliasrelation, on recharge la partie relation du menu de droite de paramétrage de l'onglet
    if (getAttributeValue(nsAdminMoveField.origHeaderElt, "0_3") == "29")
        nsAdmin.loadAdminFile(nsAdmin.tab, "RelationsPart");

    // TODO: Transmettre la position du nouveau champ si possible
    nsAdminField.editFieldProperties(result.Descid, null, function (oRes) { nsAdminField.selectLabel(); });

    top.setWait(false);

};

nsAdminMoveField.reset = function () {
    if (!nsAdminMoveField.New) {
        unsetEventListener(document, 'mousemove', nsAdminMoveField.onFieldMove, true);
        unsetEventListener(document, 'mouseup', nsAdminMoveField.onFieldMoveEnd, true);
    }
    else if (nsAdminFile.browser.isFirefox) {
        //FF n'expose pas clientX et clientY lors du drag d'un element : https://bugzilla.mozilla.org/show_bug.cgi?id=505521
        //On passe donc par une fonction intermédiaire pour récupérer les coordonées
        unsetEventListener(document, 'dragover', nsAdminFile.getDocPos, true);
    }

    nsAdminMoveField.origDescId = null;
    nsAdminMoveField.origDisporder = null;
    nsAdminMoveField.origCells = null;
    nsAdminMoveField.resetDestination();
    nsAdminMoveField.shadow = null;
    nsAdminMoveField.Create = false;
    nsAdminMoveField.UnspecifiedType = false;
    nsAdminMoveField.SpecField = null;
    nsAdminMoveField.isHeaderField = false;
    nsAdminMoveField.WholeSpaceRow = false;

    nsAdmin.resetDragDrop();
};

nsAdminMoveField.resetDestination = function () {
    nsAdminMoveField.destHeaderElt = null;
    nsAdminMoveField.destDisporder = null;
    nsAdminMoveField.destCell = null;

    nsAdmin.resetDragDrop();
};


nsAdminMoveField.init = function () {

    if (!nsAdminFile.canMoveFields()) {
        if (nsAdminMoveField.debug) { console.log("!nsAdminFile.canMoveFields() --> end of nsAdminMoveField.init()"); }
        // Pas de déplacement des champs sur la fiche
        // On se contente d'initialiser les positions
        nsAdminMoveField.initPositions();
        return;
    }

    nsAdminMoveField.tab1 = document.getElementById("ftm_" + nGlobalActiveTab);
    if (nsAdminMoveField.debug) { console.log("nsAdminMoveField.tab1 : "); console.log(nsAdminMoveField.tab1); }
    if (nsAdminMoveField.tab1) {
        nsAdminMoveField.cells1 = nsAdminMoveField.tab1.querySelectorAll("td");
        var cells_Lab = nsAdminMoveField.tab1.querySelectorAll("td[edo]");
        for (var i = 0; i < cells_Lab.length; i++) {
            setEventListener(cells_Lab[i], 'mousedown', fmd);
        }
    }
    if (nsAdminMoveField.debug) { console.log("nsAdminMoveField.tab2 : "); console.log(nsAdminMoveField.tab2); }
    nsAdminMoveField.tab2 = document.getElementById("ftdbkm_" + nGlobalActiveTab);
    if (nsAdminMoveField.tab2) {
        nsAdminMoveField.cells2 = nsAdminMoveField.tab2.querySelectorAll("td");
        var cells_Lab = nsAdminMoveField.tab2.querySelectorAll("td[edo]");
        for (var i = 0; i < cells_Lab.length; i++) {
            setEventListener(cells_Lab[i], 'mousedown', fmd);
        }
    }

    nsAdminMoveField.tabHeader = document.getElementById("ftp_" + nGlobalActiveTab);
    if (nsAdminMoveField.debug) { console.log("nsAdminMoveField.tabHeader : "); console.log(nsAdminMoveField.tabHeader); }
    if (nsAdminMoveField.tabHeader) {
        nsAdminMoveField.cellsHeader = nsAdminMoveField.tabHeader.querySelectorAll("td");
        var cells_Lab = nsAdminMoveField.cellsHeader;
        //var cells_Lab = nsAdminMoveField.tabHeader.querySelectorAll("td.table_labels");
        for (var i = 0; i < cells_Lab.length; i++) {
            setEventListener(cells_Lab[i], 'mousedown', fmd);
        }
    }

    nsAdminMoveField.DivBkmPres = document.getElementById("divBkmPres");
    if (nsAdminMoveField.debug) { console.log("nsAdminMoveField.DivBkmPres : "); console.log(nsAdminMoveField.DivBkmPres); }

    nsAdminMoveField.initPositions();

};



nsAdminMoveField.initPositions = function () {
    nsAdminMoveField.posTab1 = getAbsolutePosition(nsAdminMoveField.tab1);
    nsAdminMoveField.posTab2 = getAbsolutePosition(nsAdminMoveField.tab2);
    nsAdminMoveField.posTabHeader = getAbsolutePosition(nsAdminMoveField.tabHeader);
};

//***************************** Déplacement de champ - FIN *************************************

//******************Déplacement de la barre des signet (BreakLine) *****************************

var nsAdminMoveBkmBar = {};
nsAdminMoveBkmBar.debug = false;

nsAdminMoveBkmBar.init = function () {
    var hdDtls = document.getElementById("divBkmPaging");
    //setEventListener(hdDtls, 'mousedown', nsAdminMoveBkmBar.onMouseDown, true);
    nsAdminMoveBkmBar.origHeaderElt = hdDtls;
    nsAdminMoveBkmBar.ble = getAttributeValue(nsAdminMoveBkmBar.origHeaderElt, "ble");

    if (nsAdminMoveField.tab1) {
        nsAdminMoveBkmBar.trs1 = nsAdminMoveField.tab1.querySelectorAll("tr");
        nsAdminMoveBkmBar.glWidth = nsAdminMoveField.tab1.offsetWidth;
        nsAdminMoveBkmBar.glX = nsAdminMoveField.posTab1.x;
        nsAdminMoveBkmBar.nbCellsPerRows = getNumber(getAttributeValue(nsAdminMoveField.tab1, "cpr"));
    }
    else if (nsAdminMoveField.tab2) {
        nsAdminMoveBkmBar.glWidth = nsAdminMoveField.tab2.offsetWidth;
        nsAdminMoveBkmBar.glX = nsAdminMoveField.posTab2.x;
        nsAdminMoveBkmBar.nbCellsPerRows = getNumber(getAttributeValue(nsAdminMoveField.tab2, "cpr"));
    }

    if (nsAdminMoveField.tab2) {
        nsAdminMoveBkmBar.trs2 = nsAdminMoveField.tab2.querySelectorAll("tr");
    }
}

nsAdminMoveBkmBar.onDragStart = function (e) {

    if (nsAdminFile.browser.isFirefox) {
        e.dataTransfer.setData("text", e.target);
        //FF n'expose pas clientX et clientY lors du drag d'un element : https://bugzilla.mozilla.org/show_bug.cgi?id=505521
        //On passe donc par une fonction intermédiaire pour récupérer les coordonées
        setEventListener(document, 'dragover', nsAdminFile.getDocPos, true);
    }

    nsAdmin.initDragDrop(this);

    // on se sert des variables calculées dans nsAdminMoveField.initPositions() pour ne pas les calculer deux fois
    nsAdminMoveField.initPositions();


    //debugger;
    var shadowHeight = 3;
    var shadowWidth = nsAdminMoveBkmBar.glWidth;

    nsAdminMoveBkmBar.shadow = document.createElement("div");
    addClass(nsAdminMoveBkmBar.shadow, "shadow");
    document.documentElement.appendChild(nsAdminMoveBkmBar.shadow);

    var obj_pos = getAbsolutePosition(nsAdminMoveBkmBar.origHeaderElt);
    if (nsAdminMoveBkmBar.debug) { console.log(obj_pos); }
    nsAdminMoveBkmBar.shadow.style.position = "absolute";
    nsAdminMoveBkmBar.shadow.style.left = nsAdminMoveBkmBar.glX + "px";
    nsAdminMoveBkmBar.shadow.style.top = obj_pos.y + "px";
    nsAdminMoveBkmBar.shadow.style.width = shadowWidth + "px";
    nsAdminMoveBkmBar.shadow.style.height = shadowHeight + "px";

    //si ne marche pas sur le dernier IE check dragOpt.eventPosition dans eDrag.js
    var pos = nsAdminFile.getPos(e);

    nsAdminMoveBkmBar.cursorStartX = pos.x;
    nsAdminMoveBkmBar.cursorStartY = pos.y;
    nsAdminMoveBkmBar.elStartLeft = parseInt(obj_pos.x, 10);
    nsAdminMoveBkmBar.elStartTop = parseInt(obj_pos.y, 10);

};

nsAdminMoveBkmBar.onDrag = function (e) {

    var pos = nsAdminFile.getPos(e);

    // le pointeur est toujours sur la même ligne : inutile de relancer des calculs couteux
    if (isPointInArea(pos, nsAdminMoveBkmBar.eltTr))
        return;

    // Move drag element by the same amount the cursor has moved.
    var style = nsAdminMoveBkmBar.shadow.style;
    style.top = (nsAdminMoveBkmBar.elStartTop + pos.y - nsAdminMoveBkmBar.cursorStartY) + "px";


    // on se sert des variables calculées dans nsAdminMoveField.init() pour ne pas les calculer deux fois
    var bTab1 = isPointInArea(pos, nsAdminMoveField.posTab1);
    var bTab2 = isPointInArea(pos, nsAdminMoveField.posTab2);
    var bTabHeader = isPointInArea(pos, nsAdminMoveField.posTabHeader);

    if (!bTab1 && !bTab2 && !bTabHeader)
        return;

    var oTab;
    var trs;
    if (bTab1) {
        //if (nsAdminMoveBkmBar.debug) { console.log("--TAB1--"); }
        oTab = nsAdminMoveField.tab1;
        trs = nsAdminMoveBkmBar.trs1;
    }
    else if (bTab2) {
        //if (nsAdminMoveBkmBar.debug) { console.log("--TAB2--"); }
        oTab = nsAdminMoveField.tab2;
        trs = nsAdminMoveBkmBar.trs2;
    }
    else if (bTabHeader) {
        oTab = nsAdminMoveField.tabHeader;
        trs = nsAdminMoveBkmBar.trs2;
    }

    for (var i = 0; i < trs.length; i++) {
        var posTr = getAbsolutePosition(trs[i]);
        // if (nsAdminMoveBkmBar.debug) { console.log(pos); console.log(posTr); }

        if (isPointInArea(pos, posTr)) {
            if (!getAttributeValue(trs[i], "y")) {
                //if (nsAdminMoveBkmBar.debug) { console.log("--Pas d'attribut y sur cette ligne--"); }
                return;
            }
            if (!nsAdminMoveBkmBar.isLineComplete(trs[i])) {
                //if (nsAdminMoveBkmBar.debug) { console.log("--Cette ligne est incomplète--"); }
                return;
            }
            nsAdminMoveBkmBar.eltTr = trs[i];
            nsAdminMoveBkmBar.y = getNumber(getAttributeValue(trs[i], "y"));
            nsAdminMoveBkmBar.shadow.style.top = posTr.y + "px";
            addClass(nsAdminMoveBkmBar.shadow, "ok");
            break;
        }
        else {
            nsAdminMoveBkmBar.reset();
        }
    }

};

nsAdminMoveBkmBar.onDragEnd = function (e) {

    // Mise à jour de la PREF pour forcer la désactivation du mode résumé
    var updUsrVal = "type=22;$;tab=" + nGlobalActiveTab + ";$;value=0";
    updateUserValue(updUsrVal);


    document.documentElement.removeChild(nsAdminMoveBkmBar.shadow);

    if (!nsAdminMoveBkmBar.eltTr)
        return;


    if (!(nsAdminMoveBkmBar.y >= 0))
        return;

    nsAdminMoveBkmBar.y = getNumber(nsAdminMoveBkmBar.y);

    if (nsAdminMoveBkmBar.debug) { console.log("y : " + nsAdminMoveBkmBar.y); }

    var tab = getNumber(getAttributeValue(document.getElementById("navTabs"), "did"));
    if (!tab)
        return;

    //l'attribut y est en base 0 la breakline en base est en base 1
    nsAdminMoveBkmBar.y++;

    // Ne s'applique plus
    //switch (tab) {
    //    case TAB_PP:
    //        nsAdminMoveBkmBar.y += NB_LINE_HEAD_PP;
    //        break;
    //    case TAB_PM:
    //    case TAB_ADR:
    //        nsAdminMoveBkmBar.y += NB_LINE_HEAD_PM;
    //        break;
    //    default:

    //}

    var caps = new Capsule(tab);
    caps.AddProperty("0", nsAdminMoveBkmBar.ble, nsAdminMoveBkmBar.y);

    var json = JSON.stringify(caps);
    var upd = new eUpdater("eda/Mgr/eAdminDescManager.ashx", 1);

    upd.json = json;
    upd.ErrorCallBack = function () { alert('La mise à jour du positionnement de la barre des signets a échoué'); };

    upd.send(function (oRes) { nsAdminMoveBkmBar.onUpdate(oRes, tab); });


    nsAdminMoveBkmBar.reset();

    if (nsAdminFile.browser.isFirefox) {
        //FF n'expose pas clientX et clientY lors du drag d'un element : https://bugzilla.mozilla.org/show_bug.cgi?id=505521
        //On passe donc par une fonction intermédiaire pour récupérer les coordonées
        unsetEventListener(document, 'dragover', nsAdminFile.getDocPos, true);
    }

};

nsAdminMoveBkmBar.reset = function () {
    removeClass(nsAdminMoveBkmBar.shadow, "ok");
    nsAdminMoveBkmBar.eltTr = null;
    nsAdminMoveBkmBar.y = null;

    nsAdmin.resetDragDrop();
};

nsAdminMoveBkmBar.isLineComplete = function (eltTr) {
    if (eltTr.cells.length >= nsAdminMoveBkmBar.nbCellsPerRows)
        return true;

    var cnt = 0;
    for (var i = 0; i < eltTr.cells.length; i++) {
        //if (nsAdminMoveBkmBar.debug) { console.log("colSpan = " + eltTr.cells[i].colSpan); }
        if (eltTr.cells[i].colSpan)
            cnt += eltTr.cells[i].colSpan;
        else
            cnt++;
    }

    return (cnt >= nsAdminMoveBkmBar.nbCellsPerRows);

};

nsAdminMoveBkmBar.onUpdate = function (oRes, tab) {
    var result = JSON.parse(oRes);

    if (!result.Success)
        alert(result.Error);

    var pty = nsAdmin.findProperty(result.Capsule.ListProperties, 0, 119)
    document.getElementById("admBreakLine").value = pty.Value;

    nsAdmin.loadContentModule(USROPT_MODULE_ADMIN_TAB, null, { tab: tab });
}



//******************Déplacement de la barre des signet (BreakLine)- FIN ************************
var nsAdminAddBkmWeb = {};
nsAdminAddBkmWeb.debug = false;
nsAdminAddBkmWeb.onDragStart = function (e) {
    e.dataTransfer.dropEffect = "copy";
    if (nsAdminFile.browser.isFirefox) {
        e.dataTransfer.setData("text", e.target);
        //FF n'expose pas clientX et clientY lors du drag d'un element : https://bugzilla.mozilla.org/show_bug.cgi?id=505521
        //On passe donc par une fonction intermédiaire pour récupérer les coordonées
        setEventListener(document, 'dragover', nsAdminFile.getDocPos, true);
    }
    nsAdmin.initDragDrop(e.currentTarget);
    addClass(document.getElementById("bkmHeadClean"), "ondragbkm");

    nsAdminAddBkmWeb.BkmBar = document.getElementById("bkmTab_" + nGlobalActiveTab);
    nsAdminAddBkmWeb.BkmBarTabPos = getAbsolutePosition(nsAdminAddBkmWeb.BkmBar);
    nsAdminAddBkmWeb.OK = false;
    if (nsAdminAddBkmWeb.debug) { console.log("-->BkmBarTab : "); console.log(sAdminAddBkmWeb.BkmBarTab); console.log("-->BkmBarTabPos : "); console.log(sAdminAddBkmWeb.BkmBarTabPos); }
}


nsAdminAddBkmWeb.onDrag = function (e) {
    var pos = nsAdminFile.getPos(e);
    if (nsAdminAddBkmWeb.debug) { console.log("---->pos : "); console.log(pos); }

    if (isPointInArea(pos, nsAdminAddBkmWeb.BkmBarTabPos)) {
        addClass(nsAdminAddBkmWeb.BkmBar, "hover");
        nsAdminAddBkmWeb.OK = true;
        if (nsAdminAddBkmWeb.debug) { console.log("------>ON"); }
    }
    else {
        removeClass(nsAdminAddBkmWeb.BkmBar, "hover");
        nsAdminAddBkmWeb.OK = false;
        if (nsAdminAddBkmWeb.debug) { console.log("------>OFF"); }
    }
}

nsAdminAddBkmWeb.onDragEnd = function (e) {
    try {
        removeClass(document.getElementById("bkmHeadClean"), "ondragbkm");

        if (nsAdminAddBkmWeb.OK) {
            //alert("et là on lance le manager pour créer le signet");
            var upd = new eUpdater("eda/mgr/eAdminCreateBkmWeb.ashx", 1);
            upd.addParam("parenttab", nGlobalActiveTab, "post");

            var myElem = nsAdmin.dragSrcElt;
            var nSubType = getNumber(getAttributeValue(myElem, "esubtype"));
            upd.addParam("subtype", nSubType, "post");


            upd.send(function (oRes) { nsAdminAddBkmWeb.afterCreate(oRes); })
        }
    }
    catch (e) {
        if (nsAdminAddBkmWeb.debug) { console.log(e); }
    }
    finally {
        nsAdminAddBkmWeb.OK = false;
        removeClass(nsAdminAddBkmWeb.BkmBar, "hover");

        if (nsAdminFile.browser.isFirefox) {
            //FF n'expose pas clientX et clientY lors du drag d'un element : https://bugzilla.mozilla.org/show_bug.cgi?id=505521
            //On passe donc par une fonction intermédiaire pour récupérer les coordonées
            unsetEventListener(document, 'dragover', nsAdminFile.getDocPos, true);
        }
    }

    nsAdmin.resetDragDrop();
}

nsAdminAddBkmWeb.afterCreate = function (oRes) {
    var result = JSON.parse(oRes);
    if (nsAdminAddBkmWeb.debug) { console.log(result); }
    if (result.Success) {

        nsAdmin.loadContentModule(USROPT_MODULE_ADMIN_TAB, null, { tab: nGlobalActiveTab });

        nsAdminBkmWeb.editBkmWebProperties(result.BkmDescid);
    }
    else {
        eAlert(0, top._res_72, result.Error);
    }
}

///Edition des propriétes d'un signet
var nsAdminBkm = {};

nsAdminBkm.BkmManagerAction = {
    UNDEFINED: 0,
    GETINFOS: 1
}

nsAdminBkm.editBkmProperties = function (bkm, fctAfter) {

    // Rafraîchissement en asynchrone de la navbar à l'édition d'un signet
    nsAdmin.loadNavBar(USROPT_MODULE_ADMIN_TABS, { tab: nsAdmin.tab, bkm: bkm });

    // Puis chargement du bandeau droit
    var upd = new eUpdater("eda/mgr/eAdminBkmPropertiesManager.ashx", 1);
    upd.addParam("action", nsAdminBkm.BkmManagerAction.GETINFOS, "post");
    upd.addParam("tab", nsAdmin.tab, "post");
    upd.addParam("bkm", bkm, "post");

    upd.send(

        function (oRes) {
            nsAdmin.refreshTab(2, oRes);
            nsAdmin.addTitleClickEvent("paramTab2");
            if (typeof (fctAfter) == "function")
                fctAfter(oRes);
        }

    );
}


///Edition des propriétes d'un signet web

var nsAdminBkmWeb = {};
nsAdminBkmWeb.Action = {
    UNDEFINED: 0,
    GETINFOS: 1, // Recupère le html de la tab d'édition de l'onglet web
    TOSPECIF: 2, //Utilisation d'une Specif dans le signet Web 
    TOEXTERN: 3, // Utilisation d'une URL Externe
    DELETE: 4 // Utilisation d'une URL Externe

}

nsAdminBkmWeb.editBkmWebProperties = function (bkm, event) {

    if (event)
        top.stopEvent(event);

    //Load Web Tab Properties
    var upd = new eUpdater("eda/mgr/eAdminBkmWebManager.ashx", 1);
    upd.addParam("action", nsAdminBkmWeb.Action.GETINFOS, "post");
    upd.addParam("tab", nsAdmin.tab, "post");
    upd.addParam("bkm", bkm, "post");

    upd.send(

        //Maj du tab
        // oRes contient le html de l'admin des propriété du webtab
        function (oRes) {
            nsAdmin.refreshTab(2, oRes);
            nsAdmin.addTitleClickEvent("paramTab2");

            //Affecte/réaffecte les handler de drag&drop
            nsAdmin.setDDHandlers();

            //nsAdmin.highLightSpecif(specifId);
        }

    );
};


//Suppression d'un signet web
nsAdminBkmWeb.deleteBkm = function (bkm, e) {

    //Cancel bubling
    stopEvent(e);


    //Fonction de suppression
    var fctDelete = function () {

        var upd = new eUpdater("eda/mgr/eAdminBkmWebManager.ashx", 1);
        upd.addParam("tab", nsAdmin.tab, "post");
        upd.addParam("bkm", bkm, "post");
        upd.addParam("action", nsAdminBkmWeb.Action.DELETE, "post");
        upd.send(
            function () {
                //On recharge la fenêtre

                nsAdmin.loadContentModule(USROPT_MODULE_ADMIN_TAB, null, { tab: nsAdmin.tab });

                //Et on va sur les paramètres de l'onglet
                nsAdmin.showBlock('paramTab1');
                nsAdmin.loadNavBar(null, { tab: nsAdmin.tab, bkm: 0 });
            }
        );
    };

    var bkmLabel = document.getElementById("BkmHead_" + bkm);
    bkmLabel = bkmLabel.childNodes[0];
    if (bkmLabel)
        bkmLabel = GetText(bkmLabel);


    //Fenêtre de confirmation
    var conf = eAdvConfirm({
        'criticity': eAdvConfirm.CRITICITY.MSG_EXCLAM,
        'title': top._res_806,
        'message': (top._res_7806 + "").replace("<BKM>", bkmLabel), //"Suppression du signet "<BKM>""
        'details': top._res_7637, // "Etes-vous supprimer le sous-onglet  ?"       
        'cssOk': 'button-red',
        'cssCancel': 'button-green',
        'resOk': top._res_19,
        'resCancel': top._res_29,
        'okFct': fctDelete
    });






}

nsAdminBkmWeb.switchIsSpecif = function (obj) {
    bkm = getNumber(getAttributeValue(obj, "bkm"));

    if (getAttributeValue(obj, "chk") == "1") {
        action = nsAdminBkmWeb.Action.TOSPECIF;
    }
    else {
        action = nsAdminBkmWeb.Action.TOEXTERN; // non implémenté pour l'instant on a décidé que ce n'était pas possible et que le cas où on revient en arrière est rare
    }

    var upd = new eUpdater("eda/mgr/eAdminBkmWebManager.ashx", 1);
    //upd.addParam("action", nsAdmin.WebTabManagerAction.GETINFOS, "post");
    upd.addParam("tab", nGlobalActiveTab, "post");
    upd.addParam("bkm", bkm, "post");
    upd.addParam("action", action, "post");

    upd.send(
        function () {
            nsAdminBkmWeb.editBkmWebProperties(bkm);
        }

    );
};

var nsAdminORM = {};
nsAdminORM.DeleteToken = function (id) {


    var fctConfirm = function () {



        var upd = new eUpdater("eda/Mgr/eAdminOrmTokenManager.ashx", 1);

        upd.ErrorCallBack = function () {
            nsAdmin.revertLastValue(elem);
            setWait(false);
        };

        var obj = {
            a: 1, //Suppression
            i: id
        }

        upd.json = JSON.stringify(obj);
        setWait(true);

        upd.send(function (oRes) {
            setWait(false);

            var res = JSON.parse(oRes);

            if (!res.Success) {
                var sT = res.ErrorTitle;
                var sE = res.ErrorMsg;
                var sD = res.ErrorDetailMsg;
                top.eAlert(0, sT, sE, sD);
            }
            else {
                var w = document.querySelector("[data-tokdid='" + id + "']");
                w.remove()
                //  top.eAlert(0, "Suppression réussie", "Suppression réussie", "");
            }
        });
    }

    var conf = eAdvConfirm({
        'criticity': eAdvConfirm.CRITICITY.MSG_EXCLAM,
        'title': top._res_719,
        'message': top._res_2800,
        'details': '',
        'cssOk': 'button-red',
        'cssCancel': 'button-green',
        'resOk': top._res_19,
        'resCancel': top._res_29,
        'okFct': fctConfirm,
        'cancelFct': function () { conf.hide(); }
    });

}

nsAdminORM.Disable = function (id, disable) {

    var fctConfirm = function () {


        var upd = new eUpdater("eda/Mgr/eAdminOrmTokenManager.ashx", 1);

        upd.ErrorCallBack = function () {
            nsAdmin.revertLastValue(elem);
            setWait(false);
        };

        var obj = {
            a: 3, //Suppression
            i: id,
            d: disable
        }

        upd.json = JSON.stringify(obj);
        setWait(true);

        upd.send(function (oRes) {
            setWait(false);

            var res = JSON.parse(oRes);

            if (!res.Success) {
                var sT = res.ErrorTitle;
                var sE = res.ErrorMsg;
                var sD = res.ErrorDetailMsg;
                top.eAlert(0, sT, sE, sD);
            }
            else {
                top.eAlert(1, top._res_1761, top._res_1761, "", 0, 0, function () { nsAdmin.loadAdminModule('ADMIN_ORM', 'ADMIN_GENERAL_CONFIGADV') });
            }
        });
    }

    if (disable) {
        var conf = eAdvConfirm({
            'criticity': eAdvConfirm.CRITICITY.MSG_EXCLAM,
            'title': top._res_719,
            'message': top._res_2805,
            'details': '',
            'cssOk': 'button-red',
            'cssCancel': 'button-green',
            'resOk': disable ? top._res_6745 : top._res_6744,
            'resCancel': top._res_29,
            'okFct': fctConfirm,
            'cancelFct': function () { conf.hide(); }
        });
    }
    else {
        fctConfirm();
    }
}

var modalDate = null
nsAdminORM.onSelectExiprationDate = function (id) {


    var datePicker = new eDateTimePicker({
        "title": "Titre de la fenêtre",
        "okLabel": "Valider",
        "cancelLabel": "Annuler",
        "hideEmptyDate": true,
        "hideHoursAndMinutes": true,
        "value": "",
        "onValidate": nsAdminORM.ValidateExpirationDate, // pour valider la valeur   

    });

}


nsAdminORM.ValidateExpirationDate = function (date) {
    document.getElementById('INPUT_ORMTOKEN_DATEEXP').value = date

}


nsAdminORM.NewToken = function () {
    var appName = document.getElementById("INPUT_ORMTOKEN_APPNAME")
    var userId = document.getElementById("INPUT_ORMTOKEN_USERID")
    var dateExpiration = document.getElementById("INPUT_ORMTOKEN_DATEEXP")
    var rights = document.getElementById("INPUT_ORMTOKEN_APPRIGHTS");
    var typeToken = document.getElementById("INPUT_ORMTOKEN_TYPE");


    

    var selectedRights = Array.prototype.slice.call(rights.options).filter(function (elem) { return elem.selected }).map(function (elemen) { return elemen.value })

    var upd = new eUpdater("eda/Mgr/eAdminOrmTokenManager.ashx", 1);

    upd.ErrorCallBack = function () {
        setWait(false);
    };

    var obj = {
        a: 2, //Creation
        n: appName.value,
        id: userId.options[userId.selectedIndex].value,
        d: eDate.ConvertDisplayToFormatRFC3339(dateExpiration.value),
        r: selectedRights,
        t: typeToken.options[typeToken.selectedIndex].value
    }


    if (typeof obj.n != "string" || obj.n.length <= 0 || obj.n.l > 50) {
        var sMsg =  top._res_2021.replace('<VALUE>', obj.n.toString()).replace('<FIELD>', '[Application]');
        top.eAlert(0, top._res_6524, top._res_6524, sMsg);
        return;
    }

    upd.json = JSON.stringify(obj);
    setWait(true);

    upd.send(function (oRes) {
        setWait(false);

        var res = JSON.parse(oRes);

        if (!res.Success) {
            var sT = res.ErrorTitle;
            var sE = res.ErrorMsg;
            var sD = res.ErrorDetailMsg;
            top.eAlert(0,
                sT, sE, sD);
        }
        else {
            top.eAlert(1, top._res_1761, top._res_1761, "", 0, 0, function () { nsAdmin.loadAdminModule('ADMIN_ORM', 'ADMIN_GENERAL_CONFIGADV') });


        }
    });
}

/*************************************************************************
                Administration des champs
**************************************************************************/
var nsAdminField = {};


nsAdminField.CopyValueToClipBoard = function (event) {
    var inpt = event.target;
    inpt.select();
    document.execCommand('copy');

}

nsAdminField.CopyTextToClipBoard = function (myval) {
    var inpt = document.createElement('input');
    inpt.value = myval;
    inpt.style.position = 'absolute';
    inpt.style.left = '-9999px';
    document.body.appendChild(inpt);
    inpt.select();
    document.execCommand('copy');
    document.body.removeChild(inpt);
};

nsAdminField.FieldManagerAction = {
    UNDEFINED: 0,
    GETINFOS: 1,
    CREATEFIELD: 2,
    DROPFIELD: 3,
    RESOLVECONFLICT: 4

}

nsAdminField.selectField = function (td) {
    var activeTd = document.querySelectorAll(".mTabFile .active");
    for (var i = 0; i < activeTd.length; i++) {
        selectCell(activeTd[i], false, "active");
        selectCell(activeTd[i], false, "selected");
    }
    selectCell(td, true, "active");
};

// Reset des paramètres de la rubrique - Aucune rubrique sélectionnée
nsAdminField.cancelPropertiesEdition = function () {

    var upd = new eUpdater("eda/mgr/eAdminFieldPropertiesManager.ashx", 1);
    upd.addParam("action", nsAdminField.FieldManagerAction.GETINFOS, "post");
    upd.addParam("descid", 0, "post");

    upd.send(function (oRes) {
        nsAdmin.refreshTab(2, oRes);
        nsAdmin.addTitleClickEvent("paramTab2");
    });
}

nsAdminField.editFieldProperties = function (descid, element, fctAfter, bFocusLabel) {

    FIELD_DID = descid;
    descid = descid + "";

    if (typeof (bFocusLabel) === "undefined")
        bFocusLabel = false;

    // Est-ce qu'on essaie de modifier un champ système ?
    var sys = "0";
    try {
        if (typeof (element) !== "undefined" && element != null) {
            var ename = "";
            var valueCell = findUp(element, "TD");

            if (hasClass(valueCell, "LNKCHECK"))
                ename = getAttributeValue(valueCell, "ename");
            else
                ename = getAttributeValue(valueCell.children[0], "ename");

            var labelCell = document.getElementById(ename);
            sys = getAttributeValue(labelCell, "sys");
        }
    }
    catch (e) {
        // Tant pis, on saura pas que c'est un champ système !
    }


    var upd = new eUpdater("eda/mgr/eAdminFieldPropertiesManager.ashx", 1);
    upd.addParam("action", nsAdminField.FieldManagerAction.GETINFOS, "post");
    upd.addParam("descid", descid, "post");
    upd.addParam("tab", nGlobalActiveTab, "post");
    upd.addParam("sys", sys, "post");

    upd.send(

        function (oRes) {
            nsAdmin.refreshTab(2, oRes);
            nsAdmin.addTitleClickEvent("paramTab2");
            if (typeof (fctAfter) == "function")
                fctAfter(oRes);
            if (bFocusLabel) {
                var txt = document.getElementById("txtFieldName");
                if (txt) {
                    txt.select();
                }
            }
        }

    );
}

nsAdminField.refreshFieldPropertiesPart = function (descid, idpart) {
    FIELD_DID = descid;

    var upd = new eUpdater("eda/mgr/eAdminFieldPropertiesManager.ashx", 1);
    upd.addParam("action", nsAdminField.FieldManagerAction.GETINFOS, "post");
    upd.addParam("descid", descid, "post");
    upd.addParam("idpart", idpart, "post");

    upd.send(

        function (oRes) {

            // Re-création de la partie à rafraîchir

            var paramPartContent = document.querySelector("#" + idpart + " .paramPartContent");

            var newElement = document.createElement("div");
            newElement.innerHTML = oRes;

            if (newElement.children.length > 0) {
                // Ouvrir le bloc
                setAttributeValue(newElement.firstChild, "data-active", "1");

                paramPartContent.parentElement.replaceChild(newElement.firstChild, paramPartContent);
            }

            // Ré-affecte les événements sur les éléments
            nsAdmin.setAction();
            nsAdmin.addTitleClickEvent(idpart);

        }

    );
}

nsAdminField.dropField = function (element, descid, bconfirm) {


    if (!bconfirm)
        bconfirm = 0;

    var fieldLabel = "";
    var ename = "COL_" + (descid - descid % 100) + "_" + descid;
    if (ename) {
        fieldLabel = getAttributeValue(document.getElementById(ename), "lib");
    }
    var upd = new eUpdater("eda/mgr/eAdminFieldPropertiesManager.ashx", 1);
    upd.addParam("action", nsAdminField.FieldManagerAction.DROPFIELD, "post");
    upd.addParam("descid", descid, "post");
    upd.addParam("confirm", bconfirm, "post");

    top.setWait(true);

    upd.send(
        function (oRes) {
            top.setWait(false);
            var res = JSON.parse(oRes);

            if (res.NeedConfirm) {

                var conf = eAdvConfirm({
                    'criticity': eAdvConfirm.CRITICITY.MSG_EXCLAM,
                    'title': top._res_8724.replace("{0}", fieldLabel),
                    'message': res.UserErrorMessage,
                    'details': '',
                    'cssOk': 'button-red',
                    'cssCancel': 'button-green',
                    'resOk': top._res_19,
                    'resCancel': top._res_29,
                    'okFct': function () { nsAdminField.dropField(element, descid, "1"); },
                    'cancelFct': function () { conf.hide(); }
                });
            }
            else if (!res.Success) {
                top.eAlert(res.Criticity, top._res_90, res.UserErrorMessage);
            }
            else {
                nsAdminMoveField.ScrollTop = nsAdminMoveField.DivBkmPres.scrollTop;
                nsAdmin.loadContentModule(USROPT_MODULE_ADMIN_TAB, null, { tab: nGlobalActiveTab });
                var HdElt = document.querySelector("TD[did='" + descid + "']");
                if (getAttributeValue(HdElt, "prt") == "1")
                    nsAdmin.loadAdminFile(nGlobalActiveTab, "RelationsPart");
                nsAdminField.cancelPropertiesEdition();
            }


        }
    );
};

nsAdminField.dropSpace = function (disporder) {
    var updDispOrder = new eUpdater("eda/mgr/eMoveFieldManager.ashx", 0);
    updDispOrder.addParam("origdescid", nGlobalActiveTab, "post");
    updDispOrder.addParam("origdisporder", disporder, "post");
    updDispOrder.addParam("destdisporder", 200, "post");
    updDispOrder.send(function (oRes) {
        nsAdminMoveField.ScrollTop = nsAdminMoveField.DivBkmPres.scrollTop;
        nsAdmin.loadContentModule(USROPT_MODULE_ADMIN_TAB, null, { tab: nGlobalActiveTab });
    });


};

nsAdminField.setDefaultValue = function (eltInput, bUpdate) {

    if (typeof (bUpdate) === 'undefined')
        bUpdate = true;

    if (!eltInput)
        return;

    //s'il n'y est pas on rajoute le code pour mettre à jour Desc|Default
    if (!eltInput.hasAttribute("dsc") && document.getElementById("edfvc") != null)
        eltInput.setAttribute("dsc", document.getElementById("edfvc").value);

    if (!nsAdminField.checkDefaultValue(eltInput)) {
        //6275, "Format incorrect"
        var title = top._res_6275;

        //2021,"La valeur saisie ""<VALUE>"" ne correspond pas au format et/ou à la longueur de la rubrique <FIELD>"
        errorMessage = top._res_2021.replace("<VALUE>", eltInput.value).replace("<FIELD>", "");

        //reprise valeur précédente
        eltInput.value = nsAdminField.getDisplayFormat(eltInput, getAttributeValue(eltInput, "oldval"));
        eAlert(1, title, errorMessage);

        return;
    }

    var td = findUp(eltInput, "TD");
    if (!nsAdminField.isChange(eltInput) && getAttributeValue(td, "eaction") != "LNKSTEPCAT") {
        eltInput.value = nsAdminField.getDisplayFormat(eltInput);
        return;
    }

    if (bUpdate)
        nsAdmin.sendJson(eltInput, false, true);
};

//le champ memo n'est pas représenté directement par une balise HTML
// il faut donc créer une balise HTML factice pour que nsAdmin.SendJSON puisse soumettre les infos
nsAdminField.setMemoDefaultValue = function (memoEditorObj) {

    var eltInput = document.createElement('INPUT');
    eltInput.setAttribute("dsc", document.getElementById("edfvc").value);
    eltInput.setAttribute("did", memoEditorObj.descId);
    eltInput.value = memoEditorObj.getData();

    nsAdmin.sendJson(eltInput);
};

nsAdminField.isChange = function (eltInput) {
    return (getAttributeValue(eltInput, "oldval") != nsAdminField.newValue);
};

nsAdminField.checkDefaultValue = function (eltInput) {
    nsAdminField.isValueValid = true;
    nsAdminField.newDisplay = eltInput.value;
    nsAdminField.newValue = eltInput.value;
    if (getAttributeValue(eltInput, "dbv")) {
        nsAdminField.newValue = getAttributeValue(eltInput, "dbv");
    }

    switch (getAttributeValue(eltInput, "eAction")) {
        case "LNKNUM":
            nsAdminField.newValue = eNumber.ConvertDisplayToBdd(eltInput.value, true);
            nsAdminField.isValueValid = eNumber.IsValid();
            break;
        case "LNKDATE":
            nsAdminField.newValue = eDate.ConvertDisplayToBdd(eltInput.value);
            nsAdminField.isValueValid = eDate.IsValid();
            break;
        default:
    }

    return nsAdminField.isValueValid;
};


nsAdminField.getBDDFormat = function (eltInput) {
    var newValue = eltInput.value;

    if (getAttributeValue(eltInput, "dbv")) {
        newValue = getAttributeValue(eltInput, "dbv");
    }
    else {
        switch (getAttributeValue(eltInput, "eAction")) {
            case "LNKNUM":
                newValue = eNumber.ConvertDisplayToBdd(eltInput.value, true);
                break;
            case "LNKDATE":
                newValue = eDate.ConvertDisplayToBdd(eltInput.value);
                break;
            default:
        }
    }
    return newValue;
}


nsAdminField.setOldVal = function (eltInput) {
    eltInput.setAttribute("oldval", nsAdminField.getBDDFormat(eltInput));

}


nsAdminField.openCat = function (btn) {

    var input = btn.parentElement.querySelector("#" + getAttributeValue(btn, "eacttg"));
    var label = btn.parentElement.querySelector("#" + getAttributeValue(input, "ename"));
    var bMultiple = getAttributeValue(label, "mult") == "1";

    nsAdminField.setOldVal(input);

    switch (getAttributeValue(btn, "eaction")) {
        case "LNKCAT":
        case "LNKADVCAT":
            var bTreeview = getAttributeValue(label, "tree") == "1";
            var sDefault = input.value;
            if (getAttributeValue(input, "dbv")) {
                sDefault = getAttributeValue(input, "dbv");
            }

            catDescId = label.getAttribute('popid');
            catPopupType = label.getAttribute('pop');
            catBoundDescId = label.getAttribute('bndId');
            catBoundPopup = label.getAttribute('bndPop');
            catBoundValue = input.getAttribute('pdbv');

            showCatGeneric(bMultiple, bTreeview, sDefault, null, input.id, catDescId, catPopupType, catBoundDescId, catBoundPopup, catBoundValue, label.getAttribute("lib"), "adminCatalogValue", false,
                nsAdminField.validateCatalogValue,
                function () {
                    if (nsAdminMoveField && nsAdminMoveField.DivBkmPres)
                        nsAdminMoveField.ScrollTop = nsAdminMoveField.DivBkmPres.scrollTop;
                    nsAdmin.loadContentModule(USROPT_MODULE_ADMIN_TAB, null, { tab: nsAdmin.tab })
                }, LOADCATFROM.ADMIN);

            break
        case "LNKCATUSER":
            var nDescid = Number(getAttributeValue(label, "did"));
            if (nDescid % 100 == 99) {
                // Si rubrique "Appartient à", on ouvre la popup de délégation d'appartenance
                nsAdmin.showBelongingPopup();
            }
            else {
                nsAdminField.openUserCat(btn, input, label);
            }

            break;
        case "LNKPICKCOLOR":
            nsAdmin.openColorPicker(input, input, function () { nsAdminField.setDefaultValue(input); });
            break;
        default:

    }
};
//Catalogues : Partie de code éxécuté au clique sur valider juste avant la fermeture du catalogue avancé
nsAdminField.validateCatalogValue = function (catalogDialog, srcId, trgId, tabSelectedLabels, tabSelectedValues, selectedIDs) {

    var trgInput = document.getElementById(trgId);
    if (trgInput.tagName == "INPUT") {
        trgInput.value = joinString(";", tabSelectedLabels);
        trgInput.setAttribute("dbv", joinString(";", tabSelectedValues));
        nsAdminField.setDefaultValue(trgInput);
    }
    else if (getAttributeValue(trgInput, "eaction") == "LNKSTEPCAT") {
        var element = null;
        if (tabSelectedValues.length == 1) {
            element = selectStepDbv(tabSelectedValues[0], trgInput, true);
            nsAdminField.setDefaultValue(element.children[0]);
        }
        else {
            element = selectStepDbv("", trgInput);
            nsAdminField.setDefaultValue(trgInput);
        }
    }
}



nsAdminField.openUserCat = function (btn, eltInput, label, bUpdate) {
    var fullUserList = label.getAttribute("fulluserlist");
    var showEmptyGroup = label.getAttribute("showemptygroup");
    var multi = label.getAttribute("mult");
    var selected = eltInput.getAttribute("dbv");
    var libelle = label.innerHTML;
    var descId = getAttributeValue(label, "did");

    var showUserOnly = label.getAttribute("showuseronly");
    var showCurrentGroupFilter = label.getAttribute("showcurrentgroupfilter");
    var showCurrentUserFilter = label.getAttribute("showcurrentuserfilter");
    var useGroup = label.getAttribute("usegroup");
    var bShowCurrentUser = true;    //Vrai => Propose dans le catalogue : l'utilisateur en cours

    if (fullUserList == null) fullUserList = "";
    if (showEmptyGroup == null) showEmptyGroup = "";
    if (showUserOnly == null) showUserOnly = "";
    if (showCurrentGroupFilter == null) showCurrentGroupFilter = "";
    if (showCurrentUserFilter == null) showCurrentUserFilter = "";
    if (useGroup == null) useGroup = "";
    if (multi == null) multi = "";
    if (selected == null) selected = "";
    if (libelle == null) libelle = "";

    if (typeof (bUpdate) === 'undefined')
        bUpdate = true;

    var maxWidth = 550; //Taille max à l'écran (largeur)
    var maxHeight = (multi == 1) ? 640 : 590; //Taille max à l'écran (hauteur)
    var oTabWH = getWindowWH(top);
    var nWidth = oTabWH[0];
    var nHeight = oTabWH[1];
    if (nWidth > maxWidth)   //si largeur de l'écran plus grande que largeur max alors il faut utiliser la largeur max
        nWidth = maxWidth;
    else
        nWidth = nWidth - 10;   //marge de "sécurité"
    if (nHeight > maxHeight)   //si hauteur de l'écran plus grande que largeur max alors il faut utiliser la largeur max
        nHeight = maxHeight;
    else
        nHeight = nHeight - 10;   //marge de "sécurité"

    nsAdminField.userDialog = new eModalDialog(label.getAttribute("lib"), 0, "eCatalogDialogUser.aspx", nWidth, nHeight);

    nsAdminField.userDialog.ErrorCallBack = function () { setWait(false); }
    nsAdminField.userDialog.TargetObject = eltInput;

    nsAdminField.userDialog.addParam("multi", multi, "post");
    nsAdminField.userDialog.addParam("selected", selected, "post");
    nsAdminField.userDialog.addParam("descid", descId, "post");

    nsAdminField.userDialog.addParam("showemptygroup", showEmptyGroup, "post");
    nsAdminField.userDialog.addParam("showuseronly", showUserOnly, "post"); //si à 1 => la liste sera toujours sans groupes d'affichés
    nsAdminField.userDialog.addParam("fulluserlist", fullUserList, "post");
    nsAdminField.userDialog.addParam("modalvarname", "nsAdminField.userDialog", "post");
    //On ajoute le finder à la liste des catalogues utilisateurs OUVERT
    top.eTabCatUserModalObject.Add(nsAdminField.userDialog.iframeId, nsAdminField.userDialog);
    nsAdminField.userDialog.addParam("iframeId", nsAdminField.userDialog.iframeId, "post");


    nsAdminField.userDialog.addParam("showcurrentuser", (bShowCurrentUser ? "1" : "0"), "post");
    nsAdminField.userDialog.addParam("showcurrentgroupfilter", showCurrentGroupFilter, "post"); //Si à 1 => Proposition dans le catalogue : <le groupe de l'utilisateur en cours> pour filtre avancé
    nsAdminField.userDialog.addParam("showcurrentuserfilter", showCurrentUserFilter, "post");   //Si à 1 => Proposition dans le catalogue : <utilisateur en cours> pour filtre avancé
    nsAdminField.userDialog.addParam("usegroup", useGroup, "post"); //si à 1 => Autorise la sélection de groupe pour le catatalogue simple

    nsAdminField.userDialog.addParam("showvalueempty", "0", "post"); //si à 1 => Proposition dans le catalogue : <Vide> sur le catalogue simple
    nsAdminField.userDialog.addParam("showvaluepublicrecord", "1", "post"); //si à 1 => Proposition dans le catalogue : <Fiche Publique> sur le catalogue simple

    nsAdminField.userDialog.onIframeLoadComplete = function () { top.setWait(false); };
    nsAdminField.userDialog.ErrorCallBack = function () { top.setWait(false); };

    nsAdminField.userDialog.show();

    nsAdminField.userDialog.addButton(top._res_29, function () { nsAdminField.userDialog.hide(); }, "button-gray");
    nsAdminField.userDialog.addButton(top._res_5003, (function (elt) { return function () { nsAdminField.validUser(elt, bUpdate) }; })(eltInput), "button-green", null, "ok");
};


nsAdminField.validUser = function (eltInput, bUpdate) {
    var strReturned = nsAdminField.userDialog.getIframe().GetReturnValue();
    nsAdminField.userDialog.hide();
    var vals = strReturned.split('$|$')[0];
    var libs = strReturned.split('$|$')[1];
    eltInput.value = libs;
    eltInput.setAttribute("dbv", vals);

    nsAdminField.setDefaultValue(eltInput, bUpdate);

}

nsAdminField.pickADate = function (btn) {
    var input = btn.parentElement.querySelector("#" + getAttributeValue(btn, "eacttg"));
    var label = btn.parentElement.querySelector("#" + getAttributeValue(input, "ename"));
    var sDate = getAttributeValue(input, "dbv");

    nsAdminField.setOldVal(input);


    nsAdminField.pickADateDialog = new eModalDialog(label.getAttribute("lib"), 0, "mgr/ePickADateManager.ashx", 475, 415);
    nsAdminField.pickADateDialog.addParam("date", sDate, "post");
    nsAdminField.pickADateDialog.addParam("from", "0", "post");
    nsAdminField.pickADateDialog.show();
    nsAdminField.pickADateDialog.addButton(top._res_29, null, "button-gray", null, "cancel"); // Annuler
    nsAdminField.pickADateDialog.addButton(top._res_28, (function (elt) { return function () { nsAdminField.validDate(elt) }; })(input), "button-green", null, "ok"); // Valider
};

nsAdminField.validDate = function (eltInput) {
    var objResult = nsAdminField.pickADateDialog.getIframe().nsPickADate.getReturnValue();
    eltInput.setAttribute("dbv", objResult.dbv);
    eltInput.value = objResult.disp;
    nsAdminField.pickADateDialog.hide();

    nsAdminField.setDefaultValue(eltInput);

};

nsAdminField.openMemo = function (btn) {
    //var input = btn.parentElement.querySelector("#" + getAttributeValue(btn, "eacttg"));
    //var label = btn.parentElement.querySelector("#" + getAttributeValue(input, "ename"));

    if (nsMain.hasMemoEditors()) {
        if (nsMain.getMemoEditor('edt' + getAttributeValue(btn, "ctrl"))) {
            nsMain.getMemoEditor('edt' + getAttributeValue(btn, "ctrl")).isBeingZoomed = true;
            nsMain.getMemoEditor('edt' + getAttributeValue(btn, "ctrl")).switchFullScreen(true);
        }
        else if (nsMain.getMemoEditor('edt' + btn.id)) {
            nsMain.getMemoEditor('edt' + btn.id).switchFullScreen(true);
        }
    }
};


nsAdminField.getDisplayFormat = function (eltInput, bddValue) {
    if (!bddValue)
        bddValue = eltInput.value;
    var newDisplay = bddValue;
    switch (getAttributeValue(eltInput, "eAction")) {
        case "LNKNUM":
            newDisplay = eNumber.ConvertBddToDisplay(bddValue);
            break;
        case "LNKDATE":
            newDisplay = eDate.ConvertBddToDisplay(bddValue);
            break;
        default:
    }
    return newDisplay;
};

nsAdminField.selectLabel = function () {
    document.getElementById("txtFieldName").select();
};

nsAdminField.showChartsList = function (nChartId) {
    reportList(10, 0, nChartId);
};

nsAdminField.GetSelectedChart = function (modal) {
    var nChartId = modal.getIframe().nsFilterReportList.GetSelectedReport();
    nChartId = getNumber(nChartId);
    if (nChartId > 0) {
        var inptChartId = document.getElementById("ChartId");
        inptChartId.value = nChartId;
        nsAdminField.SetChartParameters();
    }
    modal.hide();
};

nsAdminField.SetChartParameters = function () {
    var inptChartParam = document.getElementById("ChartParameters");
    var inptChartId = document.getElementById("ChartId");
    var inptChartWidth = document.getElementById("ChartWidth");
    var inptChartHeight = document.getElementById("ChartHeight");

    if (!(nsAdmin.validParam(inptChartId) && nsAdmin.validParam(inptChartWidth) && nsAdmin.validParam(inptChartHeight)))
        return;

    inptChartParam.value = "reportid=" + inptChartId.value + "&w=" + inptChartWidth.value + "&h=" + inptChartHeight.value;

    nsAdmin.sendJson(inptChartParam);

};

nsAdminField.refreshFieldAliasSources = function () {
    var ddlFiles = document.getElementById("ddlAliasLinkedFiles");
    var ddlFields = document.getElementById("ddlAliasLinkedFields");

    // on vide la ddl des champs:
    while (ddlFields.options.length > 1) {
        ddlFields.options.remove(1);
    }
    var sSelectedFile = eTools.getSelectedValue(ddlFiles);
    if (sSelectedFile == "")
        return;

    var sTab = sSelectedFile.split('_')[1];

    var upd = new eUpdater("eda/mgr/eAdminFieldAliasRelationMgr.ashx", 1);
    upd.addParam("tab", sTab, "post");
    upd.addParam("ctxt", 0, "post");

    upd.send(nsAdminField.loadFieldAliasSources);
};

nsAdminField.loadFieldAliasSources = function (oRes) {

    var ddlFiles = document.getElementById("ddlAliasLinkedFiles");
    var ddlFields = document.getElementById("ddlAliasLinkedFields");
    var sSelectedFile = eTools.getSelectedValue(ddlFiles);
    if (sSelectedFile == "")
        return;

    var aSelectedFile = sSelectedFile.split('_');
    var sTab = aSelectedFile[1];
    var sLinkFieldDid = aSelectedFile[0];

    var aFields = JSON.parse(oRes);

    for (var i = 0; i < aFields.length; i++) {
        var field = aFields[i];
        var option = new Option(field.Value, "[" + sLinkFieldDid + "]_[" + field.Key + "]");
        ddlFields.options.add(option);
    }

};

nsAdminField.refreshAssociateFields = function () {
    var ddlFiles = document.getElementById("ddlAssociateFiles");
    var ddlFields = document.getElementById("ddlAssociateFields");

    // on vide la ddl des champs:
    while (ddlFields.options.length > 1) {
        ddlFields.options.remove(1);
    }

    var sSelectedFile = eTools.getSelectedValue(ddlFiles);
    if (sSelectedFile == "")
        return;

    var sTab = sSelectedFile.split('_')[1];

    var upd = new eUpdater("eda/mgr/eAdminFieldAliasRelationMgr.ashx", 1);
    upd.addParam("tab", sTab, "post");
    upd.addParam("did", getAttributeValue(ddlFields, "did"), "post");
    upd.addParam("ctxt", 1, "post");
    upd.send(nsAdminField.loadAssociateFields);
};

nsAdminField.loadAssociateFields = function (oRes) {

    var ddlFiles = document.getElementById("ddlAssociateFiles");
    var ddlFields = document.getElementById("ddlAssociateFields");

    var sSelectedFile = eTools.getSelectedValue(ddlFiles);
    if (sSelectedFile == "")
        return;

    var aSelectedFile = sSelectedFile.split('_');
    var sTab = aSelectedFile[1];
    var sLinkFieldDid = aSelectedFile[0];


    var aFields = JSON.parse(oRes);

    for (var i = 0; i < aFields.length; i++) {
        var field = aFields[i];
        var option = new Option(field.Value, "[" + sLinkFieldDid + "]_[" + field.Key + "]");
        ddlFields.options.add(option);
    }

};
/* Déplacement des champs d'entête */
//var nsAdminMoveHeaderField = {};

//nsAdminMoveHeaderField.


/*************************************************************************
                Administration des champs - Fin
**************************************************************************/

/*************************************************************************
                FONCTIONS COMMUNES
**************************************************************************/


nsAdminFile.docX = 0;
nsAdminFile.docY = 0;

nsAdminFile.getPos = function (event) {
    var x, y;
    if (event.pageX) {
        return { x: event.pageX, y: event.pageY };
    }
    else if (nsAdminFile.browser.isIE) {
        x = window.event.clientX + document.documentElement.scrollLeft + document.body.scrollLeft;
        y = window.event.clientY + document.documentElement.scrollTop + document.body.scrollTop;
        return { x: x, y: y };
    }
    else if (nsAdminFile.browser.isFirefox) {
        return { x: nsAdminFile.docX, y: nsAdminFile.docY };
    }

};


nsAdminFile.getDocPos = function (event) {
    nsAdminFile.docX = event.clientX;
    nsAdminFile.docY = event.clientY;
};


nsAdminFile.initDefaultValueHandler = function () {
    //gestion des valeurs par défaut sur les inputs basiques
    var aFieldsInputs = document.querySelectorAll("input[efld='1']");
    var defDsc = "";
    if (document.getElementById("edfvc"))
        defDsc = document.getElementById("edfvc").value;

    for (var k = 0; k < aFieldsInputs.length; k++) {
        var input = aFieldsInputs[k];
        input.setAttribute("dsc", defDsc);
        var mytd = findUp(input, "TD");
        var descid = getNumber(getAttributeValue(input, "did"));
        setEventListener(input, 'blur', (function (elt) { return function () { nsAdminField.setDefaultValue(elt); }; })(input), true);
        setEventListener(input, 'click', (function (elt, td, did) {
            return function () {
                nsAdminField.setOldVal(elt);
                nsAdminField.selectField(td);
                nsAdminField.editFieldProperties(did);
            };
        })(input, mytd, descid), true);
    }

    //gestion des valeurs par défaut sur les cases à cocher
    var aCheckBoxes = document.querySelectorAll("td[efld='1'][eaction='LNKCHECK']");
    for (var k = 0; k < aCheckBoxes.length; k++) {
        var link = aCheckBoxes[k].firstElementChild;

        link.setAttribute("dsc", defDsc);
        link.setAttribute("did", getAttributeValue(aCheckBoxes[k], "did"));
        setEventListener(link, 'click', (function (elt) {
            return function () {
                nsAdminField.setDefaultValue(elt);
                nsAdminField.selectField(elt);
            };
        })(link), false);
    }

    var aBitButtons = document.querySelectorAll("td[efld='1'][eaction='LNKBITBUTTON']");
    for (var k = 0; k < aBitButtons.length; k++) {
        var link = aBitButtons[k].firstElementChild;

        link.setAttribute("dsc", defDsc);
        link.setAttribute("did", getAttributeValue(aBitButtons[k], "did"));
        setEventListener(link, 'click', (function (elt) {
            return function () {
                nsAdminField.setDefaultValue(elt);
                nsAdminField.selectField(elt);
            };
        })(link), false);
    }

    //gestion des valeurs par défaut sur les catalogues et champs de type user + couleurs
    var aCatBtns = document.querySelectorAll("td.icon-catalog, [eaction='LNKPICKCOLOR']");
    for (var k = 0; k < aCatBtns.length; k++) {
        var btn = aCatBtns[k];
        setEventListener(btn, 'click', (function (elt) { return function () { nsAdminField.openCat(elt); nsAdminField.selectField(elt); }; })(btn), false);
    }
    //gestion des valeurs par défaut sur les dates
    var aDateBtns = document.querySelectorAll("td.icon-agenda");
    for (var k = 0; k < aDateBtns.length; k++) {
        var btn = aDateBtns[k];
        setEventListener(btn, 'click', (function (elt) { return function () { nsAdminField.pickADate(elt); nsAdminField.selectField(elt); }; })(btn), false);
    }

    //gestion des valeurs par défaut sur les memos
    var aMemoBtns = document.querySelectorAll("td.icnMemo");
    for (var k = 0; k < aMemoBtns.length; k++) {
        var btn = aMemoBtns[k];
        setEventListener(btn, 'click', (function (elt) { return function () { nsAdminField.openMemo(elt); nsAdminField.selectField(elt); }; })(btn), false);
    }

    // Valeurs par défaut sur les valeurs des catalogues type Etape
    var aValues = document.querySelectorAll("td[eaction='LNKSTEPCAT'] ul li");
    for (var k = 0; k < aValues.length; k++) {
        var btn = aValues[k];
        var a = btn.children[0];
        setEventListener(btn, 'click', (function (elt) {
            return function () {
                nsAdminField.setDefaultValue(elt);
                nsAdminField.selectField(elt);
            };
        })(a), false);
    }

};



nsAdminFile.initBkmBarMove = function () {
    var bkmdtls = document.getElementById("bkmDtls");
    var bkmBar = document.getElementById("bkmTab_" + nGlobalActiveTab);

    setEventListener(bkmdtls, 'mouseover', function () { addClass(bkmBar, "hover"); }, true);
    setEventListener(bkmdtls, 'mouseout', function () { removeClass(bkmBar, "hover"); }, true);

    //51718 : ajout du "highlight" sur la barre complète
    var odivBkmPaging = document.getElementById("divBkmPaging");
    if (odivBkmPaging) {
        setEventListener(odivBkmPaging, 'mouseover', function () { addClass(bkmBar, "hover"); }, true);
        setEventListener(odivBkmPaging, 'mouseout', function () { removeClass(bkmBar, "hover"); }, true);

    }

};


nsAdminFile.openSpecFilter = function (nType) {

    var aBtns = new Array();

    aBtns.push(new eModalButton(top._res_30, cancelFilter, "button-gray"));

    var fctSave = function () {
        var myFilterFrame = oModalFilterWizard.getIframe();
        myFilterFrame.saveDb(0, null, false, function () {

            oModalFilterWizard.hide();
        });

    }

    aBtns.push(new eModalButton(top._res_28, fctSave, "button-green"));

    top.editFilter(-1, nsAdmin.tab, aBtns, nType);
}




nsAdminFile.repairFieldsOrder = function (nTab, link) {
    var updDispOrder = new eUpdater("eda/mgr/eAdminFieldPropertiesManager.ashx", 1);
    updDispOrder.addParam("action", nsAdminField.FieldManagerAction.RESOLVECONFLICT, "post");
    updDispOrder.addParam("tab", nTab, "post");

    updDispOrder.send(function () {
        nsAdmin.loadContentModule(USROPT_MODULE_ADMIN_TAB, null, { tab: nTab });
    });

    var divMainModal = findUpByClass(link, "MainModal");
    var modalUID = divMainModal.id.split("_")[1];
    var oModal = top.ModalDialogs[modalUID];
    oModal.hide();

    top.setWait(false);
};

nsAdminMoveField.dropRow = function (e, y) {
    if (nsAdminMoveField.debug) {
        console.log("----------Begin nsAdminMoveField.dropRow -----------");
        console.log("e : ");
        console.log(e);
        console.log("y:" + y);
    }

    var updDispOrder = new eUpdater("eda/mgr/eMoveFieldManager.ashx", 0);
    updDispOrder.addParam("origdescid", nGlobalActiveTab, "post");
    updDispOrder.addParam("desty", y, "post");

    updDispOrder.addParam("wsr", 1, "post");
    updDispOrder.addParam("dsr", 1, "post");
    updDispOrder.send(function (oRes) {
        nsAdmin.loadContentModule(USROPT_MODULE_ADMIN_TAB, null, { tab: nGlobalActiveTab });
    });

};


nsAdminFile.updateHistoDescId = function (elt) {
    var divFiltersSearchesDuplicatesPart = document.getElementById("FiltersSearchesDuplicatesPart");
    var caps1 = nsAdmin.getCapsule(divFiltersSearchesDuplicatesPart.querySelector("select[dsc='1|27'][did='" + nGlobalActiveTab + "']"));
    var radios = divFiltersSearchesDuplicatesPart.querySelectorAll("input[type='radio'][dsc='1|27'][did='" + nGlobalActiveTab + "']");
    var radio = radios[1].checked ? radios[1] : radios[0];
    if (caps1.ListProperties[0].Value != "0")
        caps1.ListProperties[0].Value = radio.value + caps1.ListProperties[0].Value;

    var upd = new eUpdater("eda/Mgr/eAdminDescManager.ashx", 1);
    upd.json = JSON.stringify(caps1);

    upd.ErrorCallBack = function () {
        //alert('Erreur');
        setWait(false);
    };
    setWait(true);
    upd.send(function (oRes) {
        setWait(false);

        // Si sendJson fait appel à un manager qui ne renvoie pas de JSON en retour, on ne fait rien de plus
        if (typeof (oRes) == "undefined" || oRes == null || oRes == "")
            return;

        var res = JSON.parse(oRes);

        if (!res.Success) {
            if (res.Criticity == 2)
                top.eAlert(res.Criticity, top._res_92, res.UserErrorMessage);
            else
                top.eAlert(1, top._res_416, res.UserErrorMessage);
            nsAdmin.revertLastValue(obj);
        }
    });

};

// type de fichiers
nsAdminFile.EDN_TYPE = { MAIL: "3" };

// savoir si on peut déplacer les champs sur la table actuelle
nsAdminFile.canMoveFields = function () {

    var fileDiv = document.getElementById("fileDiv_" + nGlobalActiveTab);
    if (fileDiv) {

        var edtType = getAttributeValue(fileDiv, "edntype");

        switch (edtType) {
            // #51628 : onglet sys pas de déplacement de champs       
            case nsAdminFile.EDN_TYPE.MAIL:
                return false;

            default:
                return true;
        }
    }

    // Par défaut on déplace
    return true;
}

/* RESUME BREAKLINE */
/* Drag&Drop Resume BreakLine */
var oResume = (function () {
    var _debug = false;

    function initDropZones() {
        var fileDiv = document.getElementById("fileDiv_" + nGlobalActiveTab);
        if (getAttributeValue(fileDiv, "fid") == "")
            return;

        var bl = document.getElementById("resumeBreakline");

        var tables = document.querySelectorAll("#fileDiv_" + nGlobalActiveTab + " table.mTabFile");
        for (var j = 0; j < tables.length; j++) {

            if (tables[j].id.indexOf("ftdbkm") >= 0 || tables[j].id.indexOf("fth") >= 0)
                continue;

            tables[j].addEventListener("dragenter", function (event) {
                // Création d'un shadow pour représenter le marqueur
                var tdDest = event.target;
                var tr = tdDest.parentElement;
                if (tdDest.tagName == "TD" && tr.children[0] == tdDest) {
                    createShadow(tdDest);
                    if (_debug)
                        console.log("DRAGENTER: " + tdDest.id);
                }

            });
            tables[j].addEventListener("dragleave", function (event) {
                // Suppression du shadow
                var tdDest = event.target;
                var tr = tdDest.parentElement;
                if (tdDest.tagName == "TD" && tr.children[0] == tdDest) {
                    removeShadow(tdDest);
                    if (_debug)
                        console.log("DRAGLEAVE: " + tdDest.id);
                }
            });
            tables[j].addEventListener("drop", function (event) {

                var tdDest = event.target;
                var tr = tdDest.parentElement;
                if (tdDest.tagName == "TD") {

                    // Si la td de destination est la première de la ligne, on recrée la breakline à l'endroit souhaité
                    if (tr.children[0] == tdDest) {

                        if (_debug)
                            console.log("DROP: " + tdDest.id);

                        var bl = document.getElementById("resumeBreakline");

                        removeShadow(tdDest);

                        if (bl) {
                            var num = Number(getAttributeValue(tr, "data-nbline"));
                            setAttributeValue(bl, "y", num);

                            tdDest.appendChild(bl);

                            updateResumeLine(getAttributeValue(bl, "dsc"), num.toString());
                        }

                    }

                }

            });

        }
    }

    function updateResumeLine(attrDsc, value) {
        if (attrDsc.length == 0)
            return;

        var arrDsc = attrDsc.split('|');

        if (arrDsc.length != 2)
            return;

        var caps = new Capsule(nGlobalActiveTab);
        caps.AddProperty(arrDsc[0], arrDsc[1], value);
        var json = JSON.stringify(caps);

        var upd = new eUpdater("eda/Mgr/eAdminDescManager.ashx", 1);
        upd.json = json;
        upd.send(function (oRes) {
            var res = JSON.parse(oRes);

            if (!res.Success) {
                top.eAlert(1, top._res_1760, res.UserErrorMessage);
            }
        });
    }
    function createShadow(td) {
        var elt = document.createElement("div");
        elt.className = "resumeBreakline resumeBreaklineShadow icon-caret-right";
        td.appendChild(elt);
    }

    function removeShadow(td) {
        var bl = td.querySelector(".resumeBreakline.resumeBreaklineShadow");
        if (bl) {
            td.removeChild(bl);
        }
    }
    return {
        init: function () {
            initDropZones();
        },
        onDragStart: function (e) {
            e.dataTransfer.setData("text/plain", e.target.id);
            e.target.style.opacity = "0.4";
        },
        onDragEnd: function (e) {
            e.target.style.opacity = "1";
        }
    }


})();

