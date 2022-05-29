// DRAG&DROP Manager : permet de déplacer des éléments vers des blocs
// Argument "options " :
// options.draggedClass : classe CSS des éléments pouvant être déplacés
// options.dropzoneClass : classe CSS des zones pouvant accueillir les éléments déplacés
// options.shadowClass : classe CSS de l'ombre de la zone de destination
// options.origShadowClass : classe CSS de l'ombre laissée par l'élément en cours de déplacement
// options.dropCallbackFct : fonction appelée au "drop" d'un élément. Paramètres : draggedElt, origDropZone, destDropZone
// options.dragEnterCallbackFct : fonction appelée à l'entrée d'un élément. Paramètres : e
// options.dragOverCallbackFct : fonction appelée au survol d'un élément. Paramètres : e
// options.checkDropAllowedFct : fonction de vérification avant drop. Paramètres : draggedElt, origDropZone, destDropZone
// options.checkDragEnterAllowedFct : fonction de vérification au DragEnter. Paramètres : draggedElt, origDropZone, destDropZone
var eDragDropManager = function (options) {

    var _debug = false;

    var _dragObjClassname = options.draggedClass || "ddDraggedElement";
    var _dropzoneClassname = options.dropzoneClass || "ddDropzone";
    var _shadowClassname = options.shadowClass || "ddDropShadow";
    var _draggedShadowClassname = options.origShadowClass || "ddElementShadow";
    var _hoveredDropzoneClassName = "hovered";

    var _draggedElement = null;
    var _dropZone = null;
    var _draggedWidth, _draggedHeight;

    var _dropCallback = null;
    if (typeof options.dropCallbackFct === "function")
        _dropCallback = options.dropCallbackFct;

    var _dragEnterCallback = null;
    if (typeof options.dragEnterCallbackFct === "function")
        _dragEnterCallback = options.dragEnterCallbackFct;

    var _dragOverCallback = null;
    if (typeof options.dragOverCallbackFct === "function")
        _dragOverCallback = options.dragOverCallbackFct;

    var _dragLeaveCallback = null;

    var _checkDropAllowedFct = null;
    if (typeof options.checkDropAllowedFct === "function")
        _checkDropAllowedFct = options.checkDropAllowedFct;

    var _checkDragEnterAllowedFct = null;
    if (typeof options.checkDragEnterAllowedFct === "function")
        _checkDragEnterAllowedFct = options.checkDragEnterAllowedFct;

    // Spécifique pour Kanban
    var _isKanban = false;
    var _hoveredSL = null;
    var _timer, _scrollTimer;


    // Initialisation des events
    function initEvents() {

        document.addEventListener("dragstart", dragStart, false);
        document.addEventListener("dragover", dragOver, false);
        document.addEventListener("dragenter", dragEnter, false);
        document.addEventListener("dragleave", dragLeave, false);
        document.addEventListener("drop", drop, false);
        document.addEventListener("drag", drag, false);
        document.addEventListener("dragend", dragEnd, false);

    }

    function clearEvents() {
        document.removeEventListener("dragstart", dragStart, false);
        document.removeEventListener("dragover", dragOver, false);
        document.removeEventListener("dragenter", dragEnter, false);
        document.removeEventListener("dragleave", dragLeave, false);
        document.removeEventListener("drop", drop, false);
        document.removeEventListener("drag", drag, false);
        document.removeEventListener("dragend", dragEnd, false);
    }

    function drag(e) {

        if (_isKanban) {

            // Gestion du scroll

            var x = e.x || e.clientX;
            var y = e.y || e.clientY;

            var kbContent = document.getElementById("kbContent");
            var form = document.getElementById("formWidgetContent");
            var rect = kbContent.getBoundingClientRect();

            if (y > window.innerHeight - 50) {
                // Scroll down
                kbContent.scrollTop += 50;
            }
            else if (y <= window.innerHeight - 50 && y > window.innerHeight - 100) {
                kbContent.scrollTop += 25;
            }
            else {
                // Scroll top
                if (y < rect.top + 50) {
                    kbContent.scrollTop -= 50;
                }
                else if (y >= rect.top + 50 && y < rect.top + 100) {
                    kbContent.scrollTop -= 25;
                }
            }

            rect = form.getBoundingClientRect();
            if (_debug) {
                console.log("x : " + x);
                console.log("window width : " + window.innerWidth);
                console.log("rect left : " + rect.left);
            }
            if (x > window.innerWidth - 50) {
                // Scroll right
                form.scrollLeft += 50;
            }
            else if (x <= window.innerWidth - 50 && x > window.innerWidth - 100) {
                form.scrollLeft += 25;
            }
            else {
                // Scroll left
                if (x < rect.left + 50) {
                    form.scrollLeft -= 50;
                }
                else if (x >= rect.left + 50 && x < rect.left + 100) {
                    form.scrollLeft -= 25;
                }
            }
        }
        
    }

    // DragStart : lorsqu'on commence à drag un objet
    function dragStart(e) {

        e.dataTransfer.clearData();
        e.dataTransfer.effectAllowed = "all";
        _draggedElement = findUpByClass(e.target, _dragObjClassname);
        // Il faut mémoriser la taille de l'élément car lorsqu'une ligne de couloir est fermée, nous ne pouvons récupérer la taille de la carte à déplacer
        _draggedWidth = _draggedElement.offsetWidth;
        _draggedHeight = _draggedElement.offsetHeight;

        // Histoire d'envoyer quelque chose... :
        e.dataTransfer.setData("text/plain", _draggedElement.id);

        addClass(_draggedElement, _draggedShadowClassname);
    }


    function dragOver(e) {
        
        e.preventDefault();
        e.stopPropagation();

        e.dataTransfer.dropEffect = "move";

        if (_isKanban) {
            if (hasClass(e.target, "kbSL")) {
                return true;
            }
        }

        // Vérif qu'on est bien autorisé à dropper dans la zone
        var origDropZone = findUpByClass(_draggedElement, _dropzoneClassname);
        var dropzone = findUpByClass(e.target, _dropzoneClassname);

        var bAllowed = true;

        if (!dropzone)
            bAllowed = false;
        else if (typeof _checkDragEnterAllowedFct === "function")
            if (!_checkDragEnterAllowedFct(_draggedElement, origDropZone, dropzone)) {
                bAllowed = false;
            }

        if (!bAllowed) {
            e.dataTransfer.dropEffect = "none";
            return false;
        }

        //if (_isKanban) {
        //    if (hasClass(e.target, "scrollZone") || hasClass(e.target, "scrollZoneH")) {
                
        //        if (!_scrolling) {
        //            _scrolling = true;
        //            addClass(e.target, "hovered");
        //            console.log("scrolling");
        //            _scrollTimer = setTimeout(function () {

        //                var scrollId = e.target.id;

        //                if (scrollId == "scrollDown")
        //                    document.getElementById("kbContent").scrollTop += 25;
        //                else if (scrollId == "scrollTop")
        //                    document.getElementById("kbContent").scrollTop -= 40;
        //                else if (scrollId == "scrollLeft")
        //                    document.getElementById("kbContent").scrollLeft -= 40;
        //                else if (scrollId == "scrollRight")
        //                    document.getElementById("kbContent").scrollLeft += 40;

        //                _scrolling = false;
        //            }, 10);
        //        }
                
        //    }
        //}

        if (typeof _dragOverCallback === "function")
            _dragOverCallback(e);

        return false;
    }

    // DragEnter : lorsqu'on entre dans la zone de drop
    function dragEnter(e) {

        e.preventDefault();
        e.stopPropagation();

        var target = e.target;

        if (_isKanban) {

            // Gestion de l'ouverture des lignes de couloir au survol
            var listSL = document.querySelectorAll(".kbSL.hovered");
            for (var i = 0; i < listSL.length; i++) {
                removeClass(listSL[i], "hovered");
            }

            if (hasClass(target, "kbSL")) {
                addClass(target, "hovered");

                _hoveredSL = target;
                
                _timer = setTimeout(function () {
                    if (_debug)
                        console.log("click on SL : " + getAttributeValue(_hoveredSL, "data-slid"));
                    _hoveredSL.click();
                }, 1000);

                return false;
            }
            //else if (hasClass(target, "scrollZone") || hasClass(target, "scrollZoneH")) {
            //    addClass(target, "hovered");
            //}
        }


        if (target != _draggedElement && !hasClass(target, _shadowClassname)) {

            var origDropZone = findUpByClass(_draggedElement, _dropzoneClassname);
            var dropzone = findUpByClass(e.target, _dropzoneClassname);

            // Vérif qu'on est bien autorisé à dropper dans la zone
            if (typeof _checkDragEnterAllowedFct === "function")
                if (!_checkDragEnterAllowedFct(_draggedElement, origDropZone, dropzone)) {
                    removeAllShadows();
                    return;
                }

            // Si on n'est pas dans la même dropzone qu'actuellement, on crée le shadow
            if (origDropZone != dropzone) {

                var x = e.x || e.clientX;
                var y = e.y || e.clientY;
                var rect;
                var bAddedShadow = false;

                if (hasClass(target, _dropzoneClassname)) {
                    
                    // Si aucune carte n'est survolée...
                    rect = target.getBoundingClientRect();

                    // Si on rentre dans la zone sur le côté entre 2 cartes, il y a des marges x)
                    card = findUpByClass(document.elementFromPoint(x - 20, y - 20), _dragObjClassname);
                    if (!card)
                        card = findUpByClass(document.elementFromPoint(x + 20, y - 20), _dragObjClassname);

                    if (card) {
                        var nextSibling = findNextSibling(card);
                        if (nextSibling) {
                            insertShadow(nextSibling, target);
                            bAddedShadow = true;
                        }
                    }

                    if (!bAddedShadow)
                        appendShadow(target);

                    // Si on est dans la partie marge haute, on insère le shadow
                    //if (y <= rect.top + 20 && y >= rect.top && target.children.length > 0) {
                    //    var nextSibling = findNextSibling(target.children[0]);
                    //    if (nextSibling) {
                    //        insertShadow(nextSibling, target);
                    //    }
                    //}
                    //else {
                    //    // Sinon on place le shadow à la fin 
                    //    appendShadow(target);
                    //}
                }
                else {
                    
                    var target = findUpByClass(e.target, _dragObjClassname);
                    dropzone = findUpByClass(target, _dropzoneClassname);

                    // Cas où on place le shadow entre 2 éléments
                    if (target && dropzone) {
                        rect = target.getBoundingClientRect();

                        // Si survol de la partie inférieure de la carte
                        if (y >= rect.bottom - rect.height / 2 && y <= rect.bottom + 20) {
                            // Recherche du prochain élément qui n'est pas un shadow, pour l'insérer
                            var nextSibling = findNextSibling(target);
                            if (nextSibling) {
                                insertShadow(nextSibling, dropzone);
                            }
                        }
                        else {
                            // Si survol inférieure...
                            insertShadow(target, dropzone);
                        }
                    }
                 
                }
            }
            else {
                removeAllShadows();
            }

        }


        if (typeof _dragEnterCallback === "function")
            _dragEnterCallback(e);
    }

    // DragLeave : lorsqu'on quitte la zone de drop
    function dragLeave(e) {

        if (_debug) {
            console.log("dragLeave");
        }

        var target = e.target;

        if (_isKanban) {

            // Le timer doit être "cleared" au survol d'une autre ligne de couloir
            if (hasClass(target, "kbSL")) {
                if (_debug)
                    console.log("cleartimeout");
                clearTimeout(_timer);
            }
            //else if (hasClass(target, "scrollZone") || hasClass(target, "scrollZoneH")) {
            //    _scrolling = false;
            //    clearTimeout(_scrollTimer);
            //    removeClass(target, "hovered");
            //}
        }

        if (typeof _dragLeaveCallback === "function")
            _dragLeaveCallback(e);
    }

    // Fin du drag sans drop
    function dragEnd(e) {
        removeClass(_dropZone, _shadowClassname);
        removeClass(_draggedElement, _draggedShadowClassname);
        clear(e);
        removeAllShadows();
    }

    // Drop
    function drop(e) {

        e.preventDefault();


        removeClass(_dropZone, _shadowClassname);
        removeClass(_draggedElement, _draggedShadowClassname);


        if (_dropZone && _draggedElement) {

            var origDropZone = findUpByClass(_draggedElement, _dropzoneClassname);
            var destDropZone = findUpByClass(_dropZone, _dropzoneClassname);

            // Vérification avant drop
            if (typeof _checkDropAllowedFct === "function") {
                if (!_checkDropAllowedFct(_draggedElement, origDropZone, destDropZone)) {
                    _dropZone.parentElement.removeChild(_dropZone);
                    clear(e);
                    return;
                }
            }


            if (destDropZone) {
                
                //_dropZone.style.opacity = 0;
                destDropZone.replaceChild(_draggedElement, _dropZone);

                // Pour l'animation fadein
            //    var steps = 0;
            //    var timer = setInterval(function () {
            //        steps++;
            //        test.style.opacity = 0.05 * steps;
            //        if (steps >= 20) {
            //            clearInterval(timer);
            //            timer = undefined;
            //        }
            //    }, 300);
            }
                

            if (typeof _dropCallback === "function")
                _dropCallback(_draggedElement, origDropZone, destDropZone);
        }


        clear(e);

        removeAllShadows();

    }

    function clear(e) {
        _draggedElement = null;
        _dropZone = null;
        _draggedWidth = 0;
        _draggedHeight = 0;
        
    }

    // Ajoute le shadow au conteneur
    function appendShadow(container) {

        removeAllShadows();

        var shadow = createShadow();
        container.appendChild(shadow);

        addClass(container, _hoveredDropzoneClassName);

        _dropZone = shadow;

    }

    // Insère le shadow
    function insertShadow(nextElement, container) {

        try {
            removeAllShadows();
            var shadow = createShadow();
            if (shadow)
                container.insertBefore(shadow, nextElement);

            addClass(container, _hoveredDropzoneClassName);

            _dropZone = shadow;
        }
        catch (err) {
            if (_debug) {
                console.log(err);
                console.log(container);
                console.log(nextElement);
            }
        }
        
    }

    // Création du shadow
    function createShadow() {

        if (_debug)
            console.log("createShadow");

        var shadow = document.createElement("div");
        shadow.className = _shadowClassname + " " + _dragObjClassname;
        shadow.style.width = _draggedWidth + "px";
        shadow.style.height = _draggedHeight + "px";
        shadow.addEventListener("drop", drop); // Ajout de l'événement car sinon on ne peut pas déposer sur le shadow directement

        return shadow;
    }

    function removeAllShadows() {
        _dropZone = null;
        // Si des shadows existent deja, on supprime tout
        var shadows = document.getElementsByClassName(_shadowClassname);
        for (var i = 0; i < shadows.length; i++) {
            shadows[i].parentElement.removeChild(shadows[i]);
        }

        // Suppression de la classe sur les dropzones survolées
        var hoveredDropzones = document.getElementsByClassName(_hoveredDropzoneClassName);
        for (var i = 0; i < hoveredDropzones.length; i++) {
            removeClass(hoveredDropzones[i], _hoveredDropzoneClassName);
        }
    }

    // Suppression du shadow
    function removeShadow() {
        if (_debug) {
            console.log("removeShadow");
        }

        if (_dropZone)
            _dropZone.parentElement.removeChild(_dropZone);
        _dropZone = null;

    }

    // Recherche du prochain élément qui n'est pas un shadow
    function findNextSibling(target) {
        if (target == null)
            return null;

        while (target = target.nextSibling) {
            if (target && !hasClass(target, _shadowClassname))
                return target;
        }

        return null;
    }


    return {
        "init": function () {

            initEvents();
        },
        "setKanbanSpecificCallbacks": function () {
            // Pas le choix T_T
            _isKanban = true;
        },
        "clearEvents": function () {
            clearEvents();
        }
    }
}