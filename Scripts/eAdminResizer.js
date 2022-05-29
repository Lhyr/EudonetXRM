var nsAdminResizer = nsAdminResizer ||
{

    AdminResizer: function (listColWidth) {

        var constants = {
            CELL_ICON_WIDTH: 25,
            GRIPS_MARGIN: 50
        }

        var el, indexGrip;
        var _arrGrips = []; // Liste des positions des grips
        var _arrColsWidth = []; // Liste des largeurs de colonnes
        var _stopDragCallback;
        var _changeColCallback;
        var _gripPosition;
        var _draggedGrip = null;
        var _debug = false;

        /* FONCTIONS */

        // Définit la fonction callback lancée à la fin d'un drag
        this.setStopDragCallback = function (fct) {
            _stopDragCallback = fct;
        }
        // Définit la fonction callback lancée à la mise à jour d'une largeur de colonne
        this.setChangeColCallback = function (fct) {
            _changeColCallback = fct;
        }

        this.getListColWidth = function () {

            var listColWidth = "";
            for (var i = 0; i < _arrColsWidth.length; i++) {

                if (listColWidth != "")
                    listColWidth += ",";

                if ((i + 1) % 2 == 0) {
                    listColWidth += "A,25"; // Automatique + 25px de l'icône
                }
                else {
                    listColWidth += _arrColsWidth[i];
                }
            }
            return listColWidth;
        }
        
        function init(colWidths) {
            initGrips(colWidths);

            initListeners();
        }

        // Initialisation/création des grips HTML, à partir de la chaîne des largeurs de colonnes
        function initGrips(colWidths) {

            if (!colWidths)
                colWidths = null;

            initGripsPositions(colWidths);

            var element, width;
            var gripsElement = document.getElementById("ruleGrips");
            if (gripsElement) {
                gripsElement.innerHTML = "";
                for (var i = 0; i < _arrGrips.length; i++) {

                    element = document.createElement("div");
                    element.className = "grip icon-title_sep";
                    element.id = "grip" + i;
                    //addGripMouseDownHandler(element, i);
                    element.addEventListener("mousedown", (function (index) {
                        return function (event) {
                            initDragGrip(event, index);
                        }
                    })(i));
                    element.style.left = _arrGrips[i] + "px";

                    gripsElement.appendChild(element);
                }
            }

        }

        // Initialisation des events listeners
        function initListeners() {
            // Au clic sur les largeurs de colonnes
            var listSpan = document.getElementsByClassName("spanColWidth");
            for (var k = 0; k < listSpan.length; k++) {
                listSpan[k].addEventListener("click", function () {
                    addClass(this, "hidden");
                    var colIndex = getAttributeValue(this, "data-col");
                    var txtColWidth = document.getElementById("txtColWidth" + colIndex);
                    removeClass(txtColWidth, "hidden");
                    txtColWidth.focus();
                    txtColWidth.select();
                });
            }
            listSpan = document.getElementsByClassName("spanColAuto");
            for (var k = 0; k < listSpan.length; k++) {
                listSpan[k].addEventListener("click", function () {
                    
                    nsAdmin.loadContentModule(USROPT_MODULE_ADMIN_TAB, null, { tab: nGlobalActiveTab });
                });
            }
            // A la saisie des largeurs...
            var listTxtWidth = document.getElementsByClassName("txtColWidth");
            for (var k = 0; k < listTxtWidth.length; k++) {
                listTxtWidth[k].onchange = function () {
                    colWidthOnChange(this);
                };
                listTxtWidth[k].onkeyup = function (event) {
                    if (event.keyCode == 13)
                        colWidthOnChange(this);
                };

                listTxtWidth[k].addEventListener("focus", function () {
                    this.value = this.value;
                });
            }
        }

        function colWidthOnChange(element) {
            var value = element.value.replace("px", "");
            if (!isNaN(value)) {
                addClass(element, "hidden");

                var colIndex = getAttributeValue(element, "data-col");
                var txtColWidth = document.getElementById("spanColWidth" + colIndex);
                removeClass(txtColWidth, "hidden");

                changeColWidth(colIndex, Number(value));
            }
        }

        // Initialisation du tableau des positions
        function initGripsPositions(colWidths) {
            if (colWidths == null) {
                // Dans le cas où tout est en automatique, on va chercher les largeurs qui ont été générées automatiquement
                var listTr = document.querySelectorAll(".mTabFile .emptyrow");
                if (listTr) {
                    var position;
                    var elTabFile;
                    for (var j = 0; j < listTr.length; j++) {

                        elTabFile = eTools.FindClosestElementWithClass(listTr[j], "mTabFile");
                        if (elTabFile) {
                            if (elTabFile.id.indexOf("ftm") >= 0 || elTabFile.id.indexOf("ftdbkm") >= 0) {
                                var tr = listTr[j];
                                if (tr) {
                                    var tdList = tr.querySelectorAll("td:not(.btn)");

                                    position = 0;

                                    for (var i = 0; i < tdList.length; i++) {

                                        //if (tdList[i].className != "btn") {

                                        _arrColsWidth[i] = tdList[i].offsetWidth;

                                        position = position + _arrColsWidth[i]; // + 1px pour la bordure

                                        if (tdList[i].className == "table_values")
                                            position += constants.CELL_ICON_WIDTH;

                                        _arrGrips[i] = position;

                                        //}
                                    }

                                    break;
                                }
                                break;
                            }
                        }
                    }
                }
            }
            else {
                // TODO: Cas où les largeurs sont définies en admin...
                // Mise à jour des positions des grips à partir du tableau des largeurs
                var position = 0;
                var countCol = 0;
                for (var i = 0; i < _arrColsWidth.length; i++) {
                    if (countCol > 2)
                        countCol = 0;

                    position = position + _arrColsWidth[i]; // + 1px pour la bordure
                    if (countCol == 2)
                        position += constants.CELL_ICON_WIDTH;

                    _arrGrips[i] = position;

                    countCol++;
                }
            }
            //updateTabFileColumns(false);
        }

        // Modifie la largeur d'une colonne, recalcul des positions des grips
        function changeColWidth(index, width) {
            _arrColsWidth[index] = width;

            var span = document.getElementById("spanColWidth" + index);
            span.innerText = width + " px";

            //initGrips(_arrColsWidth);

            //updateWidths();
            //updateTabFileColumns(true);

            if (_changeColCallback && typeof (_changeColCallback) == "function") {
                _changeColCallback();
            }
        }

        // Ajout de l'événement mousedown sur le grip
        //function addGripMouseDownHandler(element, i) {
        //    element.addEventListener("mousedown", function (e) {
        //        initDragGrip(e, i);
        //    });
        //}

        // Ajout des événements sur les grips
        function initDragGrip(e, i) {

            if (e) {
                el = e.target;
                indexGrip = i;

                startDrag(e);
                document.documentElement.addEventListener('mousemove', doDrag, false);
                document.documentElement.addEventListener('mouseup', stopDrag, false);
            }
            
        }

        function isGripElement(element) {
            if (element.tagName == "DIV" && hasClass(element, "grip")) {
                return true;
            }
            return false;
        }

        function startDrag(e) {
            
            if (!_draggedGrip) {
                _draggedGrip = e.target;
                if (_debug) {
                    console.log("Dragged element : " + _draggedGrip);
                }
                eTools.SetClassName(_draggedGrip, "activeGrip");
                _gripPosition = e.pageX;
            }
            
        }

        // Exécution du "drag"
        function doDrag(e) {

            if (_draggedGrip) {
                doDragFunction(_draggedGrip, e.pageX);
            }
            
        }

        function doDragFunction(element, position) {

            // Un grip ne doit pas en dépasser un autre -> d'où positionMin et positionMax

            var prevPosition = (indexGrip > 0) ? _arrGrips[indexGrip - 1] : 0;
            var nextPosition = (indexGrip < _arrGrips.length) ? _arrGrips[indexGrip + 1] : _arrGrips[_arrGrips.length];

            var positionMax = nextPosition - constants.GRIPS_MARGIN; // 10 étant la marge minimum
            var positionMin = prevPosition + constants.GRIPS_MARGIN;

            _gripPosition = position;
            

            if (position > positionMax)
                _gripPosition = positionMax;
            else if (position < positionMin)
                _gripPosition = positionMin;

            if (_debug)
                console.log("Do drag position for grip " + indexGrip + " : " + _gripPosition);

            element.style.left = (_gripPosition) + 'px';
            displayColWidth(indexGrip, _gripPosition - prevPosition);
            displayColWidth(indexGrip + 1, nextPosition - _gripPosition);

            
        }

        // Arrêt du "drag"
        function stopDrag(e) {
            if (_draggedGrip) {
                stopDragFunction(_draggedGrip, _gripPosition);
                _draggedGrip = null;
            }
            
        }

        function stopDragFunction(element, position) {
            eTools.RemoveClassName(element, "activeGrip");
            document.documentElement.removeEventListener('mousemove', doDrag, false);
            document.documentElement.removeEventListener('mouseup', stopDrag, false);

            _arrGrips[indexGrip] = position;
            if (_debug)
                console.log("Stop drag position for grip " + indexGrip + " : " + position);

            updateWidths();

            if (_stopDragCallback && typeof (_stopDragCallback) == "function") {
                _stopDragCallback();
            }
        }

        // Mise à jour du tableau des largeurs suite au changement de position d'un grip
        function updateWidths() {

            var width;
            for (var i = 0; i < _arrGrips.length; i++) {
                width = getWidth(i);
                _arrColsWidth[i] = width;
            }

            // Mise à jour des largeurs des cellules du mode fiche
            var trList = document.querySelectorAll(".mTabFile tr.emptyrow");

            var tdList, tdClass;
            var widthCount, elTabFile;
            for (var j = 0; j < trList.length; j++) {

                elTabFile = eTools.FindClosestElementWithClass(trList[j], "mTabFile");
                if (elTabFile) {
                    //if (bFromRule) {
                    if (elTabFile.id == "adminFieldsContent" || elTabFile.id.indexOf("ftm") >= 0 || elTabFile.id.indexOf("divPrt") >= 0 || elTabFile.id.indexOf("ftdbkm") >= 0) {
                        applyCellsWidth(trList[j]);
                    }
                    //}
                    //else {
                    //    if (elTabFile.id == "adminFieldsContent")
                    //        applyCellsWidth(trList[j]);
                    //}
                }


            }
        }



        // Applique les bonnes largeurs aux cellules d'une "tr"
        function applyCellsWidth(tr) {

            var tdList = tr.querySelectorAll("td:not(.btn)");

            var widthCount = 0;
            for (var i = 0; i < tdList.length; i++) {

                tdClass = tdList[i].className;

                if (tdClass == "table_labels"
                    || tdClass == "table_values") {
                    tdList[i].style.width = _arrColsWidth[widthCount] + "px";
                    widthCount++;
                }
                else if (tdClass == "btn")
                    tdList[i].style.width = "25" + "px";

            }
        }

        // Affichage de la largeur de la colonne en cours de "drag"
        function displayColWidth(index, width) {
            var ruleRow = document.getElementById("ruleRow");
            if (ruleRow) {
                var tdList = ruleRow.querySelectorAll("td:not(.btn)");
                if (index < tdList.length) {

                    var sWidth = width + "px";

                    if (tdList[index].className == "table_values")
                        sWidth = "Automatique"; // TODO RES

                    var spanColWidth = tdList[index].querySelector(".spanColWidth");
                    if (spanColWidth)
                        spanColWidth.innerText = sWidth;
                }
            }
        }

        // Retourne la largeur de la colonne précédant le grip dont l'index est <index>
        function getWidth(index) {

            var width;

            if (_debug) {
                console.log("ARRGRIPS :");
                console.log(_arrGrips);
                console.log("Index " + index + " : " + _arrGrips[index]);
            }
                

            if (index == 0)
                width = _arrGrips[index];
            else
                width = _arrGrips[index] - _arrGrips[index - 1];

            // Si 2eme colonne, on retire la largeur de l'icône
            if ((index + 1) % 2 == 0) {
                width -= constants.CELL_ICON_WIDTH;
            }

            return width;
        }

        

        /* INIT */
        init(listColWidth);
    }
    
}
