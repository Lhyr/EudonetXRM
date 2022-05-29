///
/// 
///
function eWidgetToolbar(element) {

    var _elm = element;

    // Elements de la toolbar
    var _elmDrag;
    var _elmOptions;
    var _elmResize;

    // Etat de l'action de la toolbar
    var _isMoving = false;
    var _isResizing = false;
    var _actionName = "";

    // Variable de sauvegarde
    var _stateMem = null;
    var _mouseMem = null;

    // Abonnements exterieurs
    var _dragStart;
    var _dragMove;
    var _dragEnd;
    var _optionClick;

    function init() {

        _dragStart = function () { };
        _dragMove = function () { };
        _dragEnd = function () { };
        _optionClick = function () { };

        setEvents();
    }

    // ajout d'event sur la bar d'outils 
    function setEvents() {

        _elmDrag = _elm.querySelector(".widgetDragBar");
        _elmOptions = _elm.querySelector(".widgetOptions");
        _elmResize = _elm.querySelector(".xrm-widget-resize");


        setEventListener(_elmOptions, "click", onClick);
        setEventListener(_elmDrag, "mousedown", dragStart);
        setEventListener(_elmResize, "mousedown", dragStart);
        setEventListener(_elmOptions, "mouseout", onMouseOut);
    }

    function onClick(mouseEvent) {

        if (!mouseEvent) mouseEvent = window.event;
        src = mouseEvent.srcElement || mouseEvent.target;
        if (!src) return;

        var actionName = '';
        // séelction l'action souhaité
        switch (src.id) {
            case "xrm-widget-edit":
                actionName = 'edit';
                break;
            case "xrm-widget-delete":
                actionName = 'delete';
                break;
            case "xrm-widget-unlink":
                actionName = 'unlink';
                break;
            case "xrm-widget-openlink":
                actionName = 'openlink';
                break;
            case "xrm-widget-zoom":
                actionName = 'zoom';
                break;
            case "xrm-widget-config":
                actionName = 'config';
                break;
            case "xrm-widget-reload":
                actionName = 'reload';
                break;
            default:
                actionName = '';
                break;
        }

        if (actionName == '')
            return;

        _optionClick({ 'action': actionName, 'src': src });

        stopEvent(mouseEvent);
    }

    function dragStart(mouseEvent) {

        if (!mouseEvent) mouseEvent = window.event;
        var src = mouseEvent.srcElement || mouseEvent.target;

        if (src.className == 'widgetDragBar') {
            _isMoving = true;
            _isResizing = false;
            _actionName = "move";

            // pour que le mouseover ne sera pas capturer dans l'iframe
            src.style.height = "200px";

        } else if (src.className == 'xrm-widget-resize') {
            _isMoving = false;
            _isResizing = true;
            _actionName = "resize";
        }

        if (!_isMoving && !_isResizing)
            return;

        _mouseMem = { 'x': mouseEvent.clientX, 'y': mouseEvent.clientY };

        _dragStart({
            'action': _actionName,
            'mouseX': mouseEvent.clientX,
            'mouseY': mouseEvent.clientY,
            'deltaX': 0,
            'deltaY': 0
        });

        stopEvent(mouseEvent);
        setEventListener(document, "mousemove", dragMove);
        setEventListener(document, "mouseup", dragEnd);
        //setEventListener(document, "mouseout", dragEnd);
        setEventListener(document, "keyup", dragKeyUp);
    }

    function dragMove(mouseEvent) {

        if (!mouseEvent) mouseEvent = window.event;
        if (!_isMoving && !_isResizing) {
            reset();
            return;
        }

        var delta = new Object();
        delta.x = mouseEvent.clientX - _mouseMem.x;
        delta.y = mouseEvent.clientY - _mouseMem.y;


        if (Math.sqrt(delta.x * delta.x + delta.y * delta.y) < 1) {
            stopEvent(mouseEvent);
            return;
        }

        _dragMove({
            'action': _actionName,
            'mouseX': mouseEvent.clientX,
            'mouseY': mouseEvent.clientY,
            'deltaX': delta.x,
            'deltaY': delta.y
        });

        _mouseMem.x = mouseEvent.clientX;
        _mouseMem.y = mouseEvent.clientY;

        stopEvent(mouseEvent);
    }

    function dragKeyUp(event) {

        if (event && event.which == eTools.keyCode.ESCAPE) {
            _dragEnd({
                'action': _actionName,
                'mouseX': _mouseMem.x,
                'mouseY': _mouseMem.y,
                'deltaX': 0,
                'deltaY': 0
            });

            stopEvent(mouseEvent);
            cancelEvents();
            reset();
        }
    }

    function dragEnd(mouseEvent) {

        if (!mouseEvent) mouseEvent = window.event;
        var src = mouseEvent.srcElement || mouseEvent.target;

        if (!_isMoving && !_isResizing) {
            reset();
            return;
        }

        // pour que le mouseover ne sera pas capturer dans l'iframe
        if (_isMoving && src.className == "widgetDragBar")
            src.style.height = "";

        var delta = new Object();
        delta.x = mouseEvent.clientX - _mouseMem.x;
        delta.y = mouseEvent.clientY - _mouseMem.y;

        _dragEnd({
            'action': _actionName,
            'mouseX': mouseEvent.clientX,
            'mouseY': mouseEvent.clientY,
            'deltaX': delta.x,
            'deltaY': delta.y
        });

        stopEvent(mouseEvent);
        cancelEvents();
        reset();
    }

    function onMouseOut(mouseEvent) {
        ht();
    }

    function cancelEvents() {

        unsetEventListener(document, "mousemove", dragMove);
        unsetEventListener(document, "mouseup", dragEnd);
       // unsetEventListener(document, "mouseout", dragEnd);
        unsetEventListener(document, "keyup", dragKeyUp);
    }

    // ré initilise les valeur de sauvegarde
    function reset() {
        _mouseMem = null;
        _stateMem = null;
        _isMoving = false;
        _isResizing = false;
    };


    function addCustemItems(items) {

        removeCustomItems();

        var itemContainer = _elm.querySelector("ul.widgetOptions");
        if (itemContainer) {
            var i = 0;
            items.forEach(function (item) {

                if (typeof (item.icon) == "undefined" || typeof (item.eventName) == "undefined" || typeof (item.callback) == "undefined")
                    return;

                var li = document.createElement('li');
                li.id = "custom_" + i;

                setAttributeValue(li, "custom", "1");

                if (typeof (item.tooltip) == "undefined")
                    return item.tooltip = "";

                setAttributeValue(li, "title", item.tooltip);

                li.className = item.icon;
                setCustomAction(li, item.eventName, item.callback);
                itemContainer.appendChild(li);
                i++;
            });
        }
    }

    function removeCustomItems() {

        var items = Array.prototype.slice(_elm.querySelectorAll("li[custom='1']"));
        if (items)
            items.forEach(function (item) { item.parentElement.removeChild(item); });
    }

    function setCustomAction(item, eventName, callback) {
        switch (eventName.toLowerCase()) {
            case "mouseover":
                setEventListener(item, "mouseover", function (mouseEvent) {
                    try {
                        callback({ src: item, remove: function (i) { i.parentElement.removeChild(i); } });
                       // stopEvent(mouseEvent);

                    } catch (ex) { console.log(ex); }
                });
                break
            case "click":
            default:
                setEventListener(item, "click", function (mouseEvent) {
                    try {
                        callback({ src: item });
                        stopEvent(mouseEvent);
                    } catch (ex) { console.log(ex); }
                });
                break;
        }
    }

   

    // Lance le constructeur de l'objet
    init();

    return {
        cancel: function () { cancelEvents(); },
        onClick: function (onClick) { _optionClick = onClick; },
        onDragStart: function (dragStart) { _dragStart = dragStart; },
        onDragMove: function (dragMove) { _dragMove = dragMove },
        onDragEnd: function (dragEnd) { _dragEnd = dragEnd },
        setCustomItems: function (items) { addCustemItems(items); }
    }
};
