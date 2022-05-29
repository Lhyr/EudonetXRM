
///
/// Drag n drop depuis la Bibliothèque des Widgets prototypes
///
function eAdminWidgetPrototype(element) {

    var _dragEvent;
    var _elm = element; // tables contenu les info de prototypage
    var _sourceElement;
    var _offset = { left: 0, top: 0, deltaX: 0, deltaY: 0 };
    var _ghost = null;
    var _xMem, _yMem;

    var _widgetBlockMem;
    var _stateMem = null;

    // On attache l'element a l'evenment mouse down
    function internalInit() {       
        setEventListener(_elm, "mousedown", _drag.start);
        _widgetBlockMem = {
            "top": 0,
            "left": 0,
            "width": 0,
            "height": 0,
            "startX": -1,
            "startY": -1,
            "endX": -1,            
            "endY": -1
        };
    }

    var _drag = {

        // Le drag and drop est lancé
        start: function (mouseEvent) {

            mouseEvent = mouseEvent || window.event;

            // sauf bouton gauche de la souris est autorisé
            if (mouseEvent.which != 1)
                return;

            // retourne la td comportant les infos 
            _sourceElement = getPrototypeSource(mouseEvent);
            if (_sourceElement == null)
                return;

            setAttributeValue(_sourceElement, "f", "0"); // prototype

            saveState();
            setAttributeValue(_sourceElement, "active", "1");

            _xMem = mouseEvent.clientX;
            _yMem = mouseEvent.clientY;

            // on affiche la div _ghost uniquement pour le déplacement
            display_ghost(mouseEvent.clientX, mouseEvent.clientY);

            var src = mouseEvent.srcElement || mouseEvent.target;
            _dragEvent({
                'isNew': true,
                'action': 'move',
                'mouseX': mouseEvent.clientX,
                'mouseY': mouseEvent.clientY,
                'deltaX': 0,
                'deltaY': 0
            });

            setEventListener(document, "mousemove", _drag.move);
            setEventListener(document, "mouseup", _drag.stop);

        },

        // Le drag and drop en cours
        move: function (mouseEvent) {

            if (_ghost == null)
                return;

            mouseEvent = mouseEvent || window.event;

            // DragNdrop d'un nouveau widget   

            refresh_ghostPosition(mouseEvent.clientX, mouseEvent.clientY);

            var delta = new Object();
            delta.x = mouseEvent.clientX - _xMem;
            delta.y = mouseEvent.clientY - _yMem;

            if (Math.sqrt(delta.x * delta.x + delta.y * delta.y) < 1) {
                stopEvent(mouseEvent);
                return;
            }
            var src = mouseEvent.srcElement || mouseEvent.target;
            var data = {
                'isNew': true,
                'action': 'move',
                'mouseX': mouseEvent.clientX,
                'mouseY': mouseEvent.clientY,
                'deltaX': delta.x,
                'deltaY': delta.y
            };

            updateState(data);
            _dragEvent(data);


            _xMem = mouseEvent.clientX;
            _yMem = mouseEvent.clientY;
        },

        //le drag and drop est terminé
        stop: function (mouseEvent) {

            mouseEvent = mouseEvent || window.event;

            var delta = new Object();
            delta.x = mouseEvent.clientX - _xMem;
            delta.y = mouseEvent.clientY - _yMem;
            var src = mouseEvent.srcElement || mouseEvent.target;
            var data = {
                'isNew': true,
                'action': 'commit',
                'mouseX': mouseEvent.clientX,
                'mouseY': mouseEvent.clientY,
                'deltaX': delta.x,
                'deltaY': delta.y

            };

            updateState(data);
            _dragEvent(data);
            setAttributeValue(_sourceElement, "active", "0");
         

            unsetEventListener(document, "mousemove", _drag.move);
            unsetEventListener(document, "mouseup", _drag.stop);

            reset();

        }
    }

    /// On récupère la td qui contient les infos sur le nouveau widget
    function getPrototypeSource(mouseEvent) {
         var src = mouseEvent.srcElement || mouseEvent.target;
         while (src.id != 'menu-widget-ctn' && src.id != _elm.id) {
             if (src.tagName == "DIV" && hasClass(src, "widgetTypeContainer") && getAttributeValue(src, "c") == "1")
                 return src;
             src = src.parentElement;
         }

        return null;
    }

    //Affiche une div fontôme
    function display_ghost(x, y) {

        _offset = get_sourceElement_offset(x, y);

        _ghost = document.createElement("div");
        _ghost.className = "widget-ghost-container";

        for (var i = 0; i < _sourceElement.childNodes.length; i++)
            _ghost.appendChild(_sourceElement.childNodes[i].cloneNode(true));

        document.body.appendChild(_ghost);

        _ghost.style.left = (_offset.left) + "px";
        _ghost.style.top = (_offset.top) + "px";

        _ghost.style.width = _sourceElement.style.width;
        _ghost.style.height = _sourceElement.style.height;

        document.body.style.cursor = "move";
    }

    // Positionne la div fontome sur la nouvelle position de la souris
    function refresh_ghostPosition(x, y) {

        _ghost.style.left = (x + 1 - _offset.deltaX) + "px";
        _ghost.style.top = (y + 1 - _offset.deltaY) + "px";
    }


    // récupère la position de l'element par rapport au document en cours
    // pour les td
    function get_sourceElement_offset(x, y) {
        var obj = getAbsolutePosition(_sourceElement);

        var _offset = new Object();
        _offset.left = obj.x;
        _offset.top = obj.y;
        _offset.deltaX = x - _offset.left;
        _offset.deltaY = y - _offset.top;

        return _offset;
    }

    // réinitialise l'objet
    function reset() {


        if (_ghost != null)
            _ghost.parentElement.removeChild(_ghost);

        _ghost = null;
        _offset = { left: 0, top: 0, deltaX: 0, deltaY: 0 };
        _xMem = 0, _yMem = 0;
        _stateMem = null;
        document.body.style.cursor = "default";
    }


    function updateState(data) {

        _widgetBlockMem.left += data.deltaX;
        _widgetBlockMem.top += data.deltaY;
    }

    // Sauvegarde l'etat actuel
    function saveState() {
        _stateMem = new Object();
        _stateMem.top = _sourceElement.style.top;
        _stateMem.left = _sourceElement.style.left;

        _stateMem = new Object();
        _stateMem.x = getAttributeValue(_sourceElement, 'x') * 1;
        _stateMem.y = getAttributeValue(_sourceElement, 'y') * 1;
        _stateMem.w = getAttributeValue(_sourceElement, 'w') * 1;
        _stateMem.h = getAttributeValue(_sourceElement, 'h') * 1;

        _stateMem.top = _sourceElement.style.top;
        _stateMem.left = _sourceElement.style.left;
        _stateMem.width = _sourceElement.style.width;
        _stateMem.height = _sourceElement.style.height;

    }

    // Revenir à l'etat d'avant
    function restoreState() {
        if (_stateMem != null && _ghost != null) {

            _ghost.style.top = _stateMem.top;
            _ghost.style.left = _stateMem.left;
        }
    }
    
    // Initialisation du widget avec les events js
    internalInit();

    return {
        'id': 0, // widget n'existe pas
        'minSize': { 'columns': 1, 'rows': 1 },
        'getContainer': function () { return _ghost; },
        'widgetBlock': function () { return _widgetBlockMem; },
        'attr': function (attrName) { return getAttributeValue(_sourceElement, attrName) * 1; },
        'show': function () { /* pas d'affichage*/ },
        'restoreState': function () { restoreState(); },
        'redraw': function () {},
        'save': function (callback) {           

            var w = this;
            oGridController.widget.new(
                {
                    'id': 0,
                    'src': _ghost,
                    'callback': function (result) { callback(result, w); }
                });
           
        },
        'cancel': function () { reset(); },
        'dragEvent': function (callback) {
            var that = this;
            _dragEvent = function (data) {
                data.widget = that;
                try {

                    _widgetBlockMem.left = data.mouseX;
                    _widgetBlockMem.top = data.mouseY;

                    callback(data);
                } catch (ex) {
                    console.log(ex);
                }
            };
        },
        'update': function (widgetBlock) {                       

            _widgetBlockMem = widgetBlock;

            if (_ghost != null) {
                _ghost.style.left = widgetBlock.left + "px";
                _ghost.style.top = widgetBlock.top + "px";
                _ghost.style.width = widgetBlock.width + 'px';
                _ghost.style.height = widgetBlock.height + 'px';

                setAttributeValue(_ghost, 't', getAttributeValue(_sourceElement, "t"));
                setAttributeValue(_ghost, 'x', widgetBlock.startX);
                setAttributeValue(_ghost, 'y', widgetBlock.startY);
                setAttributeValue(_ghost, 'w', widgetBlock.endX - widgetBlock.startX + 1);
                setAttributeValue(_ghost, 'h', widgetBlock.endY - widgetBlock.startY + 1);
            }

            saveState();

        },
        'refresh': function () {
            reset();
            _elm = document.getElementById("menu-widget-ctn");
            internalInit();
        },
        'autoRefresh': function () { },
        'autoRefreshQueued': function () { },
        'refreshSize': function () { },
        'onDelete': function (onDelete) { }
    }
};
