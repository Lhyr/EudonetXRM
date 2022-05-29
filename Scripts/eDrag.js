var dragOpt = {
    debugMode: false, // à mettre à true pour afficher certains messages de diagnostic - NE PAS UTILISER EN PRODUCTION !
    debugLevel: 1, // indique le niveau de traces ajoutées en mode debug. Plus le nombre est élevé = plus précises sont les traces. N'a aucun effet si debugMode = false

    bCssCustom: false,
    // VARIABLES PARAMETRABLES
    // How far should the mouse move before it's considered a drag, not a click?
    // Distance parcouru par le curseur nécessaire au déclenchement du drag
    dragRadius: 100, 	// valeur par defaut 10 pixels
    setMinDragDistance: function (x) {
        dragOpt.dragRadius = x * x;
    },

    // Largueur de la ligne de séparation de colonne au moment du retaille de la colonne
    colLineWidth: 1, 	// valeur par defaut 1 pixel
    setColLineWidth: function (x) {
        dragOpt.colLineWidth = x;
    },


    // Global object to hold drag information.
    eventsObj: new Object(),

    allowedListOffset: 35,
    SrcPos: null,
    SrcList: null,  //objet du dom source
    TrgtPos: null,
    TrgtList: null,  //objet du dom destination
    FldSel: false,
    DestList: null,  //Pour le custom, destination au moment du laché de la souris
    // Detect all draggable tables and attach handlers to their headers.
    init: function () {
        // Don't initialize twice
        if (arguments.callee.done) return;
        arguments.callee.done = true;
        if (!document.createElement || !document.getElementsByTagName) return;

        dragOpt.eventsObj.zIndex = 0;
        dragOpt.browser = new getBrowser();

        //ATTENTION : A définir avant l'appel de INIT
        if (!dragOpt.SrcList || !dragOpt.TrgtList)
            return;

        addClass(dragOpt.SrcList, 'nonSelectable');
        addClass(dragOpt.TrgtList, 'nonSelectable');

        dragOpt.SrcPos = getAbsolutePosition(dragOpt.SrcList);
        dragOpt.TrgtPos = getAbsolutePosition(dragOpt.TrgtList);

    },

    // Position du curseur
    eventPosition: function (event) {
        var x, y;
        if (dragOpt.browser.isIE) {
            x = window.event.clientX + document.documentElement.scrollLeft + document.body.scrollLeft;
            y = window.event.clientY + document.documentElement.scrollTop + document.body.scrollTop;
            return { x: x, y: y };
        }
        return { x: event.pageX, y: event.pageY };
    },

    // DEBUT EVENT DE DRAG
    dragStart: function (event) {

        var brow = dragOpt.browser;
        var evtObj = dragOpt.eventsObj;

        if (evtObj.origElt != null)
            return;

        if (brow.isIE)
            this.origElt = window.event.srcElement;
        else
            this.origElt = event.target;
        this.origElt = dragOpt.customSourceElement(this.origElt);
        // Get cursor position with respect to the page.
        var pos = dragOpt.eventPosition(event);

        evtObj.origElt = this.origElt;

        evtObj.origList = this.origElt.parentNode;

        // Si la liste source fait partie des listes sur lesquelles le drag & drop est implémenté,
        // on récupère l'ID du guide à afficher dans la liste adverse
        evtObj.elemntGuid = null;
        dragOpt.gotGuideIdWithPartialId = false;
        var origListObj = dragOpt.customSourceElement(evtObj.origList);
        if (origListObj)
            evtObj.elemntGuid = dragOpt.getTargetListGuide(origListObj.id);
        if (typeof (evtObj.elemntGuid) == "undefined")
            evtObj.elemntGuid = null;

        // Création du nouveau élément HTML
        var new_elt = dragOpt.fullCopy(this.origElt, true);
        new_elt.style.margin = '0';

        var obj_pos = getAbsolutePosition(this.origElt);
        new_elt.style.position = "absolute";
        new_elt.style.left = obj_pos.x + "px";
        new_elt.style.top = obj_pos.y + "px";

        // On soustrait 12px de haut et de large pour pallier au rajout des marges supplémentaires. Sauf dans le cas du mode custom ou cette marge supplémentaire es ten gérer en css
        if (this.bCssCustom) {
            new_elt.style.width = obj_pos.w + "px";
            new_elt.style.height = obj_pos.h + "px";
        }
        else {
            new_elt.style.width = (obj_pos.w - 12) + "px";
            new_elt.style.height = (obj_pos.h - 12) + "px";
        }
        new_elt.style.opacity = 0.7;

        // Indicateur si notre nouvel élément a été ajouté au conteneur de la table
        evtObj.addedNode = false;

        // Conteneur de la table, emplacement ou l'on va insérer notre nouveau élément HTML
        evtObj.tableContainer = document.body;

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
        setEventListener(document, 'mousemove', dragOpt.dragMove, true);
        setEventListener(document, 'mouseup', dragOpt.dragEnd, true);

        dragOpt.stopEvent(event);
    },

    dragMove: function (event) {
        var evtObj = dragOpt.eventsObj;

        // Get cursor position with respect to the page.
        var pos = dragOpt.eventPosition(event);

        // Différences entre les coordonnées de départ et d'arrivé du curseur
        var dx = evtObj.cursorStartX - pos.x;
        var dy = evtObj.cursorStartY - pos.y;

        // Ajout notre nouvel élément
        if (!evtObj.addedNode && dx * dx + dy * dy > dragOpt.dragRadius) {
            evtObj.tableContainer.appendChild(evtObj.elNode);
            evtObj.addedNode = true;
        }

        // Move drag element by the same amount the cursor has moved.
        var style = evtObj.elNode.style;
        style.left = (evtObj.elStartLeft + pos.x - evtObj.cursorStartX) + "px";
        style.top = (evtObj.elStartTop + pos.y - evtObj.cursorStartY) + "px";

        // Determine au dessus de quelle liste se trouve le curseur
        evtObj.destList = null;

        if (dragOpt.comparePos(pos, dragOpt.SrcPos)) {
            evtObj.destList = dragOpt.SrcList;
        }
        else if (dragOpt.comparePos(pos, dragOpt.TrgtPos)) {
            evtObj.destList = dragOpt.TrgtList;
        }

        // On stop le drag si la souris est à l'exterieur de la zone
        if (evtObj.destList == null) {
            dragOpt.trace('Vérification de la position du curseur...');
            if (dragOpt.SrcList && dragOpt.TrgtList) {
                var srcListContainer = dragOpt.findUpWithClass(document.getElementById(dragOpt.SrcList.id), 'ItemList');
                var trgtListContainer = dragOpt.findUpWithClass(document.getElementById(dragOpt.TrgtList.id), 'ItemList');
                var srcList_pos = getAbsolutePosition(srcListContainer);
                var trgtList_pos = getAbsolutePosition(trgtListContainer);

                if (srcList_pos != null && trgtList_pos != null &&
                    (pos.x < srcList_pos.x || pos.x > trgtList_pos.x + trgtList_pos.w
                    || pos.y < srcList_pos.y || pos.y > srcList_pos.y + srcList_pos.h + srcListContainer.scrollTop + dragOpt.allowedListOffset)) {
                    dragOpt.trace('Déplacement refusé : le curseur (' + pos.x + ', ' + pos.y + ') est situé hors de la liste source (' + srcList_pos.x + ', ' + srcList_pos.y + ') ou destination (' + trgtList_pos.x + ', ' + trgtList_pos.y + ')');
                    dragOpt.dragEnd(event);
                }
            }
            else {
                dragOpt.trace("ATTENTION, vérification ignorée : liste source (" + dragOpt.SrcList + ") ou destination (" + dragOpt.TrgtList + ") non définie(s)");
            }
        }
        else {
            // Pas de déplacement interne dans la source de données
            if (evtObj.elemntGuid != null) {
                if (dragOpt.customSourceElement(evtObj.destList) == dragOpt.customSourceElement(evtObj.origList)) {
                    if (dragOpt.logLevel > 1)
                        dragOpt.trace('Déplacement non matérialisé : le déplacement se fait au sein de la même liste');
                    evtObj.elemntGuid.style.display = "none";
                }
                else {
                    if (dragOpt.logLevel > 1)
                        dragOpt.trace("Déplacement autorisé - recherche de l'élément positionné sous le curseur...");
                    var targetOption = dragOpt.findOption(dragOpt.customSourceElement(evtObj.destList), pos.y);
                    var obj_pos = null;
                    // Si le curseur n'a pas été positionné sur un élément existant de la liste, il faut positionner l'indicateur sur l'élément le plus proche
                    if (targetOption == null) {
                        // On le positionne sur le dernier objet de la liste
                        var lastListObj = dragOpt.customSourceElement(evtObj.destList).children[dragOpt.customSourceElement(evtObj.destList).children.length - 1];
                        // Si le dernier objet de la liste est l'indicateur, on se repositionne sur l'élément de la liste situé immédiatement au-dessus,
                        // si celui-ci existe
                        if (lastListObj.getAttribute("syst") != null && dragOpt.customSourceElement(evtObj.destList).children.length > 1) {
                            lastListObj = dragOpt.customSourceElement(evtObj.destList).children[dragOpt.customSourceElement(evtObj.destList).children.length - 2];
                        }
                        // Et dans tous les cas, on positionne l'indicateur uniquement si l'objet que l'on cible est un élément de la liste, et non
                        // l'indicateur lui-même
                        if (lastListObj.getAttribute("syst") == null) {
                            obj_pos = getAbsolutePosition(lastListObj);
                            obj_pos.y = getAbsolutePosition(lastListObj).y + getAbsolutePosition(lastListObj).h;
                        }
                        if (dragOpt.debugLevel > 1) {
                            if (obj_pos)
                                dragOpt.trace("Curseur positionné hors d'un élément existant. Position retenue : " + obj_pos.x + ", " + obj_pos.y);
                            else
                                dragOpt.trace("Curseur positionné hors d'un élément existant.");
                        }
                    }
                    else {
                        obj_pos = getAbsolutePosition(targetOption);
                        if (dragOpt.debugLevel > 2)
                            dragOpt.trace("Position de l'élément sous le curseur : " + obj_pos.x + ", " + obj_pos.y);
                    }
                    evtObj.elemntGuid.style.display = "block";
                    evtObj.elemntGuid.style.position = "absolute";
                    if (obj_pos == null || (obj_pos.x == 0 && obj_pos.y == 0 && obj_pos.w == 0)) {
                        obj_pos = getAbsolutePosition(evtObj.destList);
                    }
                    evtObj.elemntGuid.style.left = obj_pos.x + "px";
                    evtObj.elemntGuid.style.top = obj_pos.y + "px";
                    evtObj.elemntGuid.style.width = obj_pos.w + "px";
                }
            }
            else {
                if (dragOpt.logLevel > 1)
                    dragOpt.trace("Pas de guide à matérialiser dans cette liste");
            }
        }

        dragOpt.stopEvent(event);
    },

    dragEnd: function (event) {
        if (dragOpt.logLevel > 1)
            dragOpt.trace("Arrêt du drag et traitement du drop...");
        var brow = dragOpt.browser;

        // Stop capturing mousemove and mouseup events.
        unsetEventListener(document, 'mousemove', dragOpt.dragMove, true);
        unsetEventListener(document, 'mouseup', dragOpt.dragEnd, true);

        //    dragOpt.headActiv(false);

        var evtObj = dragOpt.eventsObj;

        // Récupération du guide de déplacement
        if (evtObj.elemntGuid != null) {
            evtObj.elemntGuid.style.display = "none";
            evtObj.elemntGuid = null;
        }

        // Get cursor position with respect to the page.
        var pos = dragOpt.eventPosition(event);

        // Si notre élément n'a pas été ajouté au conteneur de la table, on ne fait rien
        if (!evtObj.addedNode) {
            dragOpt.endAction();
            return;
        }

        // Suppression de notre élément
        evtObj.tableContainer.removeChild(evtObj.elNode);

        // Determine au dessus de quelle liste se trouve le curseur
        evtObj.destList = null;
        if (dragOpt.comparePos(pos, dragOpt.SrcPos)) {
            evtObj.destList = dragOpt.SrcList;
        }
        else if (dragOpt.comparePos(pos, dragOpt.TrgtPos)) {
            evtObj.destList = dragOpt.TrgtList;
        }
        dragOpt.DestList = evtObj.destList;

        if (evtObj.destList == null) {
            dragOpt.endAction();
            return;
        }

        //pas de déplacement interne dans la source de données
        if (evtObj.destList == dragOpt.customSourceElement(evtObj.origList) && dragOpt.customSourceElement(evtObj.origList) == dragOpt.SrcList) {
            dragOpt.trace("Pas de déplacement interne !");
            dragOpt.endAction();
            return;
        }
        if (!dragOpt.customDestMouseUp(event)) {
            dragOpt.endAction();
            return;
        }

        if (dragOpt.logLevel > 1)
            dragOpt.trace("Recherche de l'élément à la position " + pos.y);
        var targetOption = dragOpt.findOption(dragOpt.customSourceElement(evtObj.destList), pos.y);
        targetOption = (targetOption);
        if (targetOption != evtObj.origElt) {
            if (dragOpt.logLevel > 1)
                dragOpt.trace("Repositionnement de l'élément");
            dragOpt.moveOption(dragOpt.customSourceElement(evtObj.origList), evtObj.origElt, dragOpt.customSourceElement(evtObj.destList), targetOption);
        }

        //HDJ function pour initialiser la div des Choix
        if (typeof (SetSelectedDescIds) == 'function') {
            SetSelectedDescIds();
        }

        dragOpt.endAction();
    },
    // FIN EVENT DE DRAG

    moveOption: function (cont1, obj1, cont2, obj2) {
        var tmpObj = cont1.removeChild(obj1);

        dragOpt.refreshList(cont1);

        // dans le cas ou l'on retire une rubrique, on verifie que la source sélectionnée correspond bien à la table à laquelle appartient cette rubrique.
        if (dragOpt.FldSel && cont2 == dragOpt.SrcList && cont2.id != "FieldList_" + getTabDescid(getAttributeValue(tmpObj, "value")))
            return;


        if (typeof (adjustLabel) == "function")
            adjustLabel(tmpObj, cont2.id);

        if (obj2 != null)
            cont2.insertBefore(tmpObj, obj2);
        else
            cont2.appendChild(tmpObj);

        dragOpt.refreshList(cont2);

    },

    findOption: function (divList, y) {
        var evtObj = dragOpt.eventsObj;
        var optionElts = divList.children;

        for (var i = 0; i < optionElts.length; i++) {
            var optPos = getAbsolutePosition(optionElts[i]);
            if (optPos.y <= y && y <= optPos.y + optPos.h) {
                return optionElts[i];
            }
        }
        return null;
    },

    refreshList: function (divList) {
        dragOpt.trace("Rafraîchissement de la liste " + divList.id);
        var optionElts = divList.children;
        for (var i = 0; i < optionElts.length; i++) {
            if (optionElts[i].getAttribute("syst") != null || optionElts[i].getAttribute("field_list") != null)
                continue;

            if (i % 2 == 1)
                optionElts[i].className = "cell";
            else
                optionElts[i].className = "cell2";
        }
    },

    // OUTILS
    // Renvoie l'élément parent de l'élément passé en paramètre, correspondant à un ID donné
    findUp: function (elt, id) {
        var maxIterations = 1000;
        var levelCount = 0;
        if (elt == null)
            return null;

        do {
            if (elt.id && elt.id == id)
                return elt;
            else
                levelCount++;
        } while (elt = elt.parentElement && levelCount < maxIterations);

        return null;
    },

    // Renvoie l'élément parent de l'élément passé en paramètre, dont la classe CSS correspond en totalité ou partie à la classe passée en paramètre
    findUpWithClass: function (elt, eltClass) {
        if (elt == null)
            return null;

        do {
            if (elt.className && elt.className.indexOf(eltClass) > -1)
                return elt;
        } while (elt = elt.parentElement);

        return null;
    },

    // Clone an element, copying its style and class.
    fullCopy: function (elt, deep) {
        if (elt == null)
            return;
        var new_elt = elt.cloneNode(deep);
        new_elt.className = elt.className;
        forEach(elt.style,
            function (value, key, object) {
                if (value == null || !value) return;    //!value => ie8 retour autre chose que null !
                if (typeof (value) == "string" && value.length == 0) return;

                new_elt.style[key] = elt.style[key];
            });

        new_elt.style.backgroundColor = "#bcdcfb";
        new_elt.innerHTML = elt.innerHTML;
        return new_elt;
    },

    endAction: function () {
        var evtObj = dragOpt.eventsObj;

        if (!this.bCssCustom) {
            // Rafraîchissement des couleurs des lignes
            setCssList(evtObj.origList, "cell", "cell2");
            setCssList(evtObj.destList, "cell", "cell2");
        }
        evtObj.origElt = null;
        evtObj.origList = null;
        evtObj.destList = null;
    },

    // Stop propagation
    stopEvent: function (event) {
        if (dragOpt.browser.isIE) {
            if (window.event) {
                window.event.cancelBubble = true;
                window.event.returnValue = false;
            }
        } else {
            if (typeof (event) != 'undefined' && event)
                event.preventDefault();
        }
    },


    // Teste si la pos se trouve dans pos2
    comparePos: function (pos, pos2) {
        if (pos && pos2) {
            return (
                pos.y > pos2.y &&
                pos.y < pos2.y + pos2.h + dragOpt.allowedListOffset &&
                pos.x > pos2.x &&
                pos.x < pos2.x + pos2.w);
        }
        else
            return false;
    },
    // GCH/MAB : fonction pour une gestion spécifique sur les fenêtres qui nécessitent d'accéder, non pas à l'élément source directement, mais à un
    // sous-élément intermédiaire qui contient les éléments de la liste (ex : fenêtres de catalogues ou liste des rubriques sur assistant
    // Reporting : div > div avec attribut field_list > div au lieu de div > div)
    // Par défaut, cette fonction retourne donc l'élément lui-même
    // Pour les fenêtres spécifiques susmentionnées, il faudra donc réimplémenter la fonction pour faire pointer sur le bon élément
    customSourceElement: function (oElement) {
        return oElement;
    },
    // Même chose ici : fonction par défaut à réimplémenter selon le contexte
    customDestMouseUp: function (event) {
        return true;
    },

    // Renvoie l'ID du guide de la liste cible vers laquelle effectuer le drag & drop, par rapport à l'ID d'une liste source
    // Premier sens testé : SrcList ("tous les éléments) ==> TrgtList (éléments sélectionnés)
    // Second sens testé : SrcList ("tous les éléments) <== TrgtList (éléments sélectionnés)
    getTargetListGuide: function (srcListId) {
        var targetListGuide = null;
        var trgtList = null;
        if (srcListId + "" == "")   //Dans le cas ou pas de guideline on renvoit null
            return null;
        // On commence par déterminer quelle est la liste "adverse" (cible)
        // On recherche d'abord l'ID passé en paramètre parmi les ancêtres de la liste de gauche
        var parentList = dragOpt.findUpWithClass(document.getElementById(srcListId), 'ItemList');
        if (parentList == dragOpt.SrcList || parentList == dragOpt.SrcList.parentElement)
            trgtList = dragOpt.TrgtList;
            // Si non trouvé, on le recherche parmi les ancêtres de la liste de droite
        else {
            if (parentList == dragOpt.TrgtList || parentList == dragOpt.TrgtList.parentElement)
                trgtList = dragOpt.SrcList;
        }

        // On recherche l'ID de l'élément de type "guide" (classe "dragGuideTab) parmi les enfants de la liste adverse
        var targetGuide = null;
        var targetGuides = null;
        if (trgtList != null)
            targetGuides = trgtList.querySelectorAll('div.dragGuideTab');
        // S'il n'y a qu'un seul élément guide parmi les enfants, c'est celui-ci que l'on cible
        if (targetGuides) {
            if (targetGuides.length == 1)
                targetGuide = targetGuides[0];
                // S'il y a plusieurs éléments guide, on recherche celui rattaché à la liste qui est actuellement visible sur la fenêtre
            else {
                for (var i = 0; i < targetGuides.length; i++) {
                    if (targetGuides[i].style.display != 'none' || targetGuides[i].parentElement.style.display != 'none') {
                        targetGuide = targetGuides[i];
                        i = targetGuides.length; // sortie
                    }
                }
            }
        }
        // Si on a pu cibler un élément guide, on le renvoie
        if (targetGuide)
            targetListGuide = targetGuide;

        return targetListGuide;
    },

    // Génère des messages de diagnostic ; uniquement si this.debugMode = true
    trace: function (strMessage) {
        if (dragOpt.debugMode) {
            try {
                strMessage = 'eDrag -- ' + strMessage;

                if (typeof (console) != "undefined" && console && typeof (console.log) != "undefined") {
                    console.log(strMessage);
                }
                else {
                    alert(strMessage); // TODO: adopter une solution plus discrète que alert()
                }
            }
            catch (ex) {

            }
        }
    }
}
