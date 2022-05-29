
///
/// Gestionnaires des grilles sur toutes les tables
///
function eGridLayout(context) {

    var _elm;
    var _widgetCtn;
    var _grid;

    var _context;
    var _shadowBlock = null;
    var _shadowBlockMem = null;
    var _nbMinRows = 5; // val minimu
    var _nbColumns = 12;
    var _widgets;
    var _prototype;
    var _needResize = false;
    var _loaded = false;
    var _handler = null;
    var _handlerResume = null;

    function log(msg, obj) {
        // console.log(msg + JSON.stringify(obj));
    }

    function init() {

        _context = context;


        _elm = document.getElementById("gw-grid-" + context.gridId);

        _widgetCtn = _elm.querySelector("div[id^='widget-grid-container']");

        // affichage de la grille
        setAttributeValue(_elm, "active", "1");


        UpdateContainerPosition();


        // Récupère la liste des widgets corrspondant au context.gridId
        _widgets = getWidgets(_context);

        _grid = _elm.querySelector("table[id^='widget-grid']");
        if (_grid) {

            _nbColumns = _grid.rows[0].cells.length;

            var maxRows = _nbMinRows;
            _widgets.forEach(function (widget) {

                // suppression du dom
                widget.onDelete(handleDelete);

                // Ecoute les evenements des widgets : déplacer, redimmensionner, masquer, supprimer
                widget.dragEvent(handleEvent);

                // Adapter la hauteur de la grille par rapport au widget le plus bas
                var y_plus_h = widget.attr("y") + widget.attr("h") + 1;
                if (maxRows < y_plus_h)
                    maxRows = y_plus_h;
            });


            var rowsDiff = maxRows - _grid.rows.length;
            if (rowsDiff > 0) addRows(rowsDiff);
            else removeRows(rowsDiff);


            updateWidgets();

            autoRefresh();
        }


        _handler = oEvent.on("right-menu-pinned", resizeActive);
    }

    function UpdateContainerPosition() {
        var reference = getAbsolutePosition(_context.ref, false);
        reference.innerHTML = "";

        // la postion par rapport au parent        
        _elm.style.top = reference.y + "px";
        _elm.style.left = reference.x + "px";

    }

    function resizeActive() {

        // Le recalcul de la taille est fait pour l'element actif qui n'est pas une page d'accueil
        if (getAttributeValue(_elm, "active") != "1")
            return;

        var tabMainDivWH = GetMainDivWH();
        var height = tabMainDivWH[1];
        var width = tabMainDivWH[0];
        if (_context.type == "bkm") {
            var bkm = eTools.getBookmarkContainer(_context.tab);
            var height;
            if (bkm) {
                height = (parseInt(bkm.style.height) - 35);
                //width = tabMainDivWH[0] - 15;
                width = bkm.offsetWidth - 15;

            } else {
                bkm = document.getElementById("divBkmPres");
                height = bkm ? (parseInt(bkm.style.height) - 31) : 200;

                if (_context.isAdminMode())
                    height = height - 15;
            }
        } else {
            height = height - 30;
        }

        // Si la taille n'a pas changée pas de resize
        if (_context.height == height && _context.width == width)
            return;

        _context.width = width;
        _context.height = height;

        UpdateContainerPosition();

        _widgetCtn.style.width = _context.width + "px";
        _widgetCtn.style.height = _context.height + "px";

        if (_grid) {
            _grid.style.width = (_context.width - 15) + "px";

            var cellsize = Math.floor(((_context.width - 15) - 8 * _nbColumns) / _nbColumns);


            var newStyle = document.getElementById("grid-cell-" + _context.gridId);
            if (newStyle)
                newStyle.innerHTML = " .grid-cell-" + _context.gridId + " td { width:" + cellsize + "px; height:" + cellsize + "px; }";

            updateWidgets();
        }
    }

    function updateWidgets() {

        var widgetBlock;

        // les widgets prototype ne sont pas affichable dans la grille
        _widgets.filter(function (e) { return e.id > 0; }).forEach(function (widget) {

            eTools.setWidgetWait(widget.id, true);

            try {

                widgetBlock = new Object();
                widgetBlock.startX = widget.attr("x");
                widgetBlock.startY = widget.attr("y");

                // En cas de données incorrectes en base, on positionne les widgets à la fin de la grille
                if (widgetBlock.startY < 0 || widgetBlock.startX < 0 || widgetBlock.startX >= _grid.rows[0].cells.length) {
                    widgetBlock.startX = 0;
                    widgetBlock.startY = _grid.rows.length;
                }

                widgetBlock.endX = widgetBlock.startX + widget.attr("w") - 1;
                widgetBlock.endY = widgetBlock.startY + widget.attr("h") - 1;

                flagGridCells_ON({ "startX": widgetBlock.startX, "startY": widgetBlock.startY, "endX": widgetBlock.endX, "endY": widgetBlock.endY, "wid": widget.id });


                // conversion en pixel
                widgetBlock = gridToPixel(widgetBlock);

                var widgetContainer = widget.getContainer();

                addAnimation(widgetContainer);
                widget.update(widgetBlock);
                removeAnimation(widgetContainer);

            } catch (ex) {
                console.log(ex);
            }

            widget.show();

            eTools.setWidgetWait(widget.id, false);
        });
    }

    // Flag les cellules de la grille : 
    // id de la fiche s'elle est occupée
    // 0 si elle est libre
    function flagGridCells_ON(gridBlock) {

        // Pas assez de coulonne, ne pas continuer
        if (_grid.rows[0].cells.length <= gridBlock.endX || gridBlock.startX < 0 || gridBlock.startY < 0) {
            console.log("Les dimensions du widget '" + gridBlock.wid + "' dépasse la limite de la grille ");
            return;
        }

        // libère les anciennes cellules
        flagGridCells_OFF(gridBlock.wid);

        // Ajout de ligne si pas assez 
        if ((_grid.rows.length - 1) < gridBlock.endY)
            addRows(gridBlock.endY - (_grid.rows.length - 1));


        // On flag les nouvelles cellules
        for (var i = gridBlock.startX; i <= gridBlock.endX; i++)
            for (var j = gridBlock.startY; j <= gridBlock.endY; j++)
                setAttributeValue(_grid.rows[j].cells[i], "wid", gridBlock.wid);
    }

    // Libere les cellules
    function flagGridCells_OFF(wid) {
        // On libère les cellules occuppées avant 
        Array.prototype.slice.call(_grid.querySelectorAll("td[wid='" + wid + "']")).forEach(function (cell) { setAttributeValue(cell, "wid", "0"); });
    }

    // convertit les coordonnées et les dimension pixel en système de grille
    // la conersion se fait sur un point donné
    function pixelToGrid(pixelPoint) {

        var pos = { x: -1, y: -1 };

        // Dans le cas ou il faudra ajouter des lignes on retourne -1
        if ((pixelPoint.x < 0) ||
            (pixelPoint.x >= _grid.offsetWidth) ||
            (pixelPoint.y < 0) ||
            (pixelPoint.y >= _grid.offsetHeight)) {

            // le point est à l'extérieur de la grille
            return pos;
        }

        //On cherche y 
        var firstIndex = 0;
        var lastIndex = _grid.rows.length - 1;
        var row;
        var security = 0;
        while (firstIndex <= lastIndex) {

            row = _grid.rows[Math.floor((firstIndex + lastIndex) / 2)];

            if (pixelPoint.y >= row.offsetTop && pixelPoint.y <= (row.offsetTop + row.offsetHeight + 5)) { pos.y = row.rowIndex; break; }
            else if (firstIndex >= lastIndex) break;
            else if (pixelPoint.y > (row.offsetTop + row.offsetHeight)) firstIndex = row.rowIndex + 1;
            else if (pixelPoint.y < row.offsetTop) lastIndex = row.rowIndex;

            security++;
            if (security > 100) {
                console.log("infinite loop y stopped");
                break;
            }
        }

        // on cherche x si on a trouvé y
        if (pos.y >= 0) {
            firstIndex = 0;
            lastIndex = row.cells.length - 1;
            var cell;
            security = 0;
            while (firstIndex <= lastIndex) {

                cell = row.cells[Math.floor((firstIndex + lastIndex) / 2)];

                if (pixelPoint.x >= cell.offsetLeft && pixelPoint.x <= (cell.offsetLeft + cell.offsetWidth + 5)) { pos.x = cell.cellIndex; break; }
                else if (firstIndex >= lastIndex) break;
                else if (pixelPoint.x > (cell.offsetLeft + cell.offsetWidth)) firstIndex = cell.cellIndex + 1;
                else if (pixelPoint.x < cell.offsetLeft) lastIndex = cell.cellIndex;

                security++;
                if (security > 100) {
                    console.log("infinite loop x stopped");
                    break;
                }
            }
        }

        return pos;
    }

    // convertit les coordonnées et les dimension grille en Pixel
    function gridToPixel(widgetBlock) {

        //Le block doit se trouvé à l'interieur de la grille 
        if (!isInsideGrid(widgetBlock))
            return _shadowBlockMem;

        var topLeftCell = _grid.rows[widgetBlock.startY].cells[widgetBlock.startX];
        var bottomRightCell = _grid.rows[widgetBlock.endY].cells[widgetBlock.endX];

        var startPosition = { x: topLeftCell.offsetLeft, y: topLeftCell.offsetTop };
        var endPosition = { x: bottomRightCell.offsetLeft, y: bottomRightCell.offsetTop, w: bottomRightCell.offsetWidth, h: bottomRightCell.offsetWidth };
        var parentPosition = { x: _grid.offsetLeft, y: _grid.offsetTop };

        var left = startPosition.x - parentPosition.x;
        var top = startPosition.y - parentPosition.y;
        var newWidth = endPosition.x - startPosition.x + endPosition.w - 4;
        var newHeight = endPosition.y - startPosition.y + endPosition.h - 4;

        widgetBlock.left = left;
        widgetBlock.top = top;
        widgetBlock.width = newWidth;
        widgetBlock.height = newHeight;

        return widgetBlock;
    }

    // Le block doit se trouvé à l'interieur de la grille 
    function isInsideGrid(widgetBlock) {

        var bValid = true;
        bvalid = bValid && (widgetBlock.startX >= 0 && widgetBlock.endX >= 0);
        bvalid = bValid && (widgetBlock.startY >= 0 && widgetBlock.endY >= 0);
        bValid = bValid && (widgetBlock.startX <= widgetBlock.endX && widgetBlock.startY <= widgetBlock.endY);
        bValid = bValid && (_grid.rows[0].cells.length > widgetBlock.endX && _grid.rows.length > widgetBlock.endY);
        return bValid;
    }

    // Affiche un block surlignée la place libre qui peut être occupée par le widget
    function highlight(shadowBlock) {

        var shadow = _widgetCtn.querySelector(".widget-shadow");
        if (shadow == null) {
            shadow = document.createElement("div");
            shadow.className = "widget-shadow background-theme";
            _widgetCtn.appendChild(shadow);
        } else {
            shadow.style.display = "block";
        }

        shadow.style.left = shadowBlock.left + "px";
        shadow.style.top = shadowBlock.top + "px";
        shadow.style.width = shadowBlock.width + 'px';
        shadow.style.height = shadowBlock.height + 'px';
    }

    // Fait une rechreche d'un block libre suffisant pour héberger le widget
    function findFreeBlock(widget, startPoint, endPoint) {

        if (_shadowBlockMem == null)
            _shadowBlockMem = widget.widgetBlock();

        if (_shadowBlockMem.startX < 0 || _shadowBlockMem.startY < 0) {
            _shadowBlockMem = findNextFreeBlock(widget.id, widget.attr("w"), widget.attr("h"));
            return _shadowBlockMem;
        }

        // On ajoute une ligne en bas si besoin
        if (endPoint.y >= _grid.rows.length - 1)
            addRows(1);

        // Si on est en dehors on garde le block mémorisé
        if (startPoint.x == -1 || startPoint.y == -1 || endPoint.x == -1 || endPoint.x > _grid.rows[0].cells.length - 1)
            return _shadowBlockMem;

        var widgetBlock = new Object();
        widgetBlock.widgetId = widget.attr("f");
        widgetBlock.startX = startPoint.x;
        widgetBlock.startY = startPoint.y;
        widgetBlock.endX = endPoint.x;
        widgetBlock.endY = endPoint.y;

        var widgetBlock = gridToPixel(widgetBlock);

        // Si y a intersection, on retourne le block libre d"avant
        if (intersect(widgetBlock))
            return _shadowBlockMem;

        _shadowBlockMem = widgetBlock;

        return widgetBlock;
    }

    // A partir des dimensions du widget, on cherche un emplacement libre    
    function findNextFreeBlock(wid, width, height) {

        var block = searchFreeBlock(width, height);
        var widgetBlock = new Object();
        widgetBlock.widgetId = wid;
        widgetBlock.startX = block.startX;
        widgetBlock.startY = block.startY;
        widgetBlock.endX = block.endX;
        widgetBlock.endY = block.endY;
        return gridToPixel(widgetBlock);
    }

    // On sauvegarde les hauteurs dans un histogramme
    function searchFreeBlock(w, h) {

        var histogram = new Array();
        var nb_rows = _grid.rows.length
        var nb_cells = _grid.rows[0].cells.length

        // Init de l'histogramme
        for (var j = 0; j < nb_cells; j++)
            histogram[j] = 0;

        var o = { "exists": false };
        var i = 0;
        while (!o.exists) {
            for (var j = 0; j < nb_cells; j++) {
                var wid = getAttributeValue(_grid.rows[i].cells[j], "wid");
                if (wid == "0" || wid == "")
                    histogram[j] += 1;
                else
                    histogram[j] = 0;
            }

            // Si ca répond à la taille du widgets on retourne la position 
            o = lookup(histogram, w, h, i);

            i++;

            // On ajoute une ligne si necessaire
            if (i >= _grid.rows.length)
                addRows(1);


        }

        var result = new Object();
        result.startX = o.x;
        result.startY = o.y;
        result.endX = o.x + w - 1;
        result.endY = o.y + h - 1;
        return result;
    }

    // On vérifie si les dimensions sont contenues dans l'histogramme
    function lookup(histogram, width, height, currentRow) {
        var o = { x: -1, y: -1, exists: false };
        var w = 0;
        for (var j = 0; j < histogram.length; j++) {
            // si la taille est petite on continue
            if (histogram[j] < height) {
                w = 0;
                o.y = 0;
                continue;
            }

            // debut du rectangle supposé
            o.y = Math.max(o.y, currentRow - histogram[j] + 1);
            w++;

            if (w >= width) {
                o.x = j - w + 1;
                o.exists = true;
                return o;
            }
        }

        return o;
    }

    //Savoir si le widget est en dehors de la grille ou intersecte un autre widget
    function intersect(widgetBlock) {
        // en dehors du container 
        if (widgetBlock.startX < 0 || (widgetBlock.endX - widgetBlock.startX) > _grid.rows[0].cells.length || widgetBlock.startY < 0)
            return true;

        var widgets = Array.prototype.slice.call(_elm.querySelectorAll("div[id^='widget-wrapper-']"))
            // On exclut le widget en cours
            .filter(function (widget) { return getAttributeValue(widget, "f") * 1 != widgetBlock.widgetId; });

        var widget;
        for (var i = 0; i < widgets.length; i++) {
            widget = widgets[i];

            var x = getAttributeValue(widget, "x") * 1;
            var y = getAttributeValue(widget, "y") * 1;
            var w = getAttributeValue(widget, "w") * 1 - 1;
            var h = getAttributeValue(widget, "h") * 1 - 1;

            var sourceX = widgetBlock.startX * 1;
            var sourceY = widgetBlock.startY * 1;
            var sourceW = widgetBlock.endX - widgetBlock.startX;
            var sourceH = widgetBlock.endY - widgetBlock.startY;

            // intersection des rectangle
            var maxX = Math.max(x, sourceX);
            var maxY = Math.max(y, sourceY);

            var minW = Math.min(x + w, sourceX + sourceW);
            var minH = Math.min(y + h, sourceY + sourceH);

            if (minW >= maxX && minH >= maxY)
                return true;
        }

        return false;
    }

    // Ajoute des lignes à la fin de la grille
    function addRows(number) {
        var row;
        for (var i = 0; i < Math.abs(number); i++) {
            row = _grid.insertRow(-1);
            for (var j = 0; j < _nbColumns; j++)
                row.insertCell(0);
        }
    }

    // Enlève les "number" de lignes
    function removeRows(number) {
        for (var i = 0; i < Math.abs(number); i++) {
            _grid.deleteRow(-1);
        }
    }

    // Traitement d'evenements venant des widgets
    // déplacement, redimensionnemnt
    function handleEvent(data) {

        var widget = data.widget;

        var widgetBlock = widget.widgetBlock();

        var startPoint = pixelToGrid({ 'x': widgetBlock.left + 1, 'y': widgetBlock.top + 1 });
        // if (data.isNew)
        //      startPoint.y -= 1; // se base sur la position de souris

        setAttributeValue(_widgetCtn, "drag", "1");
        switch (data.action) {
            case 'resize':

                var endPoint = pixelToGrid({ 'x': widgetBlock.left + widgetBlock.width, 'y': widgetBlock.top + widgetBlock.height });

                // Tenir compte de la taille minimale du widget
                if (Math.abs(endPoint.x - startPoint.x) < widget.minSize.columns)
                    endPoint.x = startPoint.x + widget.minSize.columns - 1;

                if (Math.abs(endPoint.y - startPoint.y) < widget.minSize.rows)
                    endPoint.y = startPoint.y + widget.minSize.rows - 1;

                _shadowBlock = findFreeBlock(widget, startPoint, endPoint);

                highlight(_shadowBlock);

                break;

            case 'move':

                var endPoint = { 'x': startPoint.x + widget.attr("w") - 1, 'y': startPoint.y + widget.attr("h") - 1 };

                _shadowBlock = findFreeBlock(widget, startPoint, endPoint);

                highlight(_shadowBlock);

                break;

            case 'commit':

                addAnimation(widget.getContainer());

                widget.update(_shadowBlock);

                removeAnimation(widget.getContainer());

                ajusteRows();

                widget.save(function (result, w) {

                    flagGridCells_ON({
                        "startX": _shadowBlock.startX,
                        "startY": _shadowBlock.startY,
                        "endX": _shadowBlock.endX,
                        "endY": _shadowBlock.endY,
                        "wid": result.WidgetId
                    });

                    saveReturn(result, w);
                    _shadowBlockMem = null;

                    var shadow = _widgetCtn.querySelector(".widget-shadow");
                    if (shadow != null)
                        setTimeout(function () { shadow.style.display = "none"; }, 500);
                });

                setAttributeValue(_widgetCtn, "drag", "0");

                break;
            default:

                _shadowBlockMem = null;
                setAttributeValue(_widgetCtn, "drag", "0");

                break;

        }
    }

    // Supression du widget dans la liste
    function handleDelete(evt) {

        deleteWidget(evt);
        _widgets.filter(function (w) { return w.id == 0; }).forEach(function (widgetPrototype) { widgetPrototype.refresh(); });
    }

    function resetPrototype() {

        var index = getWidgetIndex(0);
        if (index >= 0)
            _widgets[index].refresh();



    }

    function deleteWidget(evt) {

        flagGridCells_OFF(evt.id);

        var index = getWidgetIndex(evt.id);
        if (index >= 0) {
            var p = evt.src.parentElement;
            if (p)
                p.removeChild(evt.src);

            _widgets.splice(index, 1);

            evt.src = null;
        }
    }

    function getWidgetIndex(wid) {

        for (var i = 0; i < _widgets.length; i++) {
            if (_widgets[i].id == wid)
                return i;
        }

        return -1;
    }

    function saveReturn(result, widget) {

        if (!result.Success)
            return;

        switch (result.ClientAction) {

            case XrmClientWidgetAction.REMOVE_FROM_DOM:
                deleteWidget({ 'id': result.WidgetId, 'src': widget.getContainer() });

                break;
            case XrmClientWidgetAction.RELOAD_WIDGET:
                widget.refresh();
                break;
            case XrmClientWidgetAction.NEW_WIDGET:
                var div = document.createElement("div");
                div.innerHTML = result.Html;
                var widgetDiv = div.querySelector("div[id='widget-wrapper-" + result.WidgetId + "']");
                _widgetCtn.appendChild(widgetDiv);
                var newWidget = new eWidget(widgetDiv, _context);
                _widgets.push(newWidget);

                newWidget.dragEvent(handleEvent);
                newWidget.onDelete(handleDelete);

                var widgetBlock = widget.widgetBlock();
                newWidget.update(widgetBlock);
                newWidget.refresh();

                _widgetCtn.scrollTop = parseInt(widgetDiv.style.top) - 30;

                resetPrototype();
                setAttributeValue(_widgetCtn, "drag", "0");

                break;
        }
    }

    function ajusteRows() {

        var maxRows = _nbMinRows;
        _widgets.forEach(function (widget) {

            // Adapter la hauteur de la grille par rapport au widget le plus bas
            var y_plus_h = widget.attr("y") + widget.attr("h") + 1;
            if (maxRows < y_plus_h)
                maxRows = y_plus_h;
        });


        var rowsDiff = maxRows - _grid.rows.length;
        if (rowsDiff > 0)
            addRows(rowsDiff);
        else removeRows(rowsDiff);

    }

    // Recupères les widgets en fonction du context
    function getWidgets(context) {

        // widgets de la grille
        var elements = _elm.querySelectorAll("div[id^='widget-wrapper-']");
        var widgets = new Array();
        Array.prototype.slice.call(elements)
            .forEach(function (elmnt) {
                widgets.push(new eWidget(elmnt, context));
            });

        //widgets prototype avec id = 0
        _prototype = GridSystem.factory.widget.prototype();
        if (_prototype != null)
            widgets.push(_prototype);

        return widgets;
    }

    // Animation au mouseUp
    function addAnimation(src) {
        try {
            src.style.transition = "0.4s";
            var contentDiv = src.querySelector("div[class='xrm-widget-content']");
            if (contentDiv) {
                contentDiv.style.transition = "0.4s";
                contentDiv.style.zIndex = 5;
            }
        } catch (ex) {/*IE8*/ }
    }

    // pas d'animation au mousemove
    function removeAnimation(src) {
        setTimeout(function () {
            try {
                src.style.transition = "0.0s";
                var contentDiv = src.querySelector("div[class='xrm-widget-content']");
                if (contentDiv) {
                    contentDiv.style.transition = "0.0s";
                    contentDiv.style.zIndex = 0;
                }
            } catch (ex) {/*IE8*/ }
        }, 500);
    }

    function autoRefresh() {

        // les widgets qui se rafraichissent a chaque affichage de la grille
        _widgets.forEach(function (widget) { widget.autoRefreshQueued(); });

        // TODO c'est pas à cette endroit
        if (typeof (top.RunnableJobQueue) !== "undefined")
            top.RunnableJobQueue.run();
    }

    function refreshUpdateDate() {
        var elGridRefreshDate = document.getElementById("xrmGridRefreshDate");
        if (elGridRefreshDate) {
            if (oDicGridRefreshDates && oDicGridRefreshDates[_context.gridId]) {
                elGridRefreshDate.innerHTML = top._res_7871 + ": " + eDate.ConvertBddToDisplay(eDate.Tools.GetStringFromDate(oDicGridRefreshDates[_context.gridId]));
                elGridRefreshDate.style.display = "inline";
            }
            else {
                elGridRefreshDate.style.display = "none";
            }
        }
    }


    // Lance le constructeur de l'objet
    init();

    function dispose() {
        if (_handler != null)
            oEvent.off("right-menu-pinned", _handler);

        if (_handlerResume != null)
            oEvent.off("file-resume-changed", _handlerResume);

        _handlerResume = null;
        _handler = null;
    }

    return {
        'widgets': _widgets,
        'container': _elm,
        'resize': function () {/* resize();*/ },
        'autoRefresh': function () {
            autoRefresh();
        },
        // Affiche la grille et les widgets
        'show': function () {
            // affichage de la grille
            setAttributeValue(_elm, "active", "1");

            resizeActive();
            refreshUpdateDate();
        },
        'dispose': dispose
    }
}


