
///
/// 
///
function eWidget(element, context) {

    var _id;
    var _context = context;
    var _elm = element;
    var _view;

    var _toolbar = null;
    var _dragEvent;
    var _onDelete = function () { };
    var _widgetBlockMem;
    var _stateMem = null;

    function init() {
        _id = getAttributeValue(_elm, "f") * 1;
        _dragEvent = function () { };

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

        attachEvents();
    }


    function attachEvents() {

        _toolbar = GridSystem.factory.createWidgetToolbar(_elm);

        _toolbar.onClick(onClick);
        _toolbar.onDragStart(dragStart);
        _toolbar.onDragMove(dragMove);
        _toolbar.onDragEnd(dragEnd);

        var type = getAttributeValue(_elm, "t") * 1;
        _view = eWidgetView.createView(type, _elm);

        _view.minSize = _view.minSize || { 'columns': 1, 'rows': 1 };
        _view.minSize.columns = _view.minSize.columns > 0 ? _view.minSize.columns : 1;
        _view.minSize.rows = _view.minSize.rows > 0 ? _view.minSize.rows : 1;

        if (typeof (_view.update) == 'undefined')
            _view.update = function () { };

        if (typeof (_view.toolbar) != 'undefined')
            _toolbar.setCustomItems(_view.toolbar);
    }


    function onClick(evt) {

        switch (evt.action) {
            case "zoom":
                zoom();
                break;
            case "reload":
                reload(evt.src);
                break;
            case "openlink":
                openlink();
                break;
            case "remove":
                evt.id = _id;
                evt.src = _elm;
                _onDelete(evt);
                break;
            default:
                break;
        }
    }

    function reload(triggerElt) {

        // Demande #71 615 - On masque le bouton Rafraîchir lors du clic, pour ne pas qu'il apparaîsse gris sur fond blanc dans le cas où il est opaque
        if (triggerElt)
            triggerElt.style.visibility = 'hidden';

        setWidgetWait();

        oGridController.widget.reload({
            'id': getAttributeValue(_elm, "f"),
            'srcId': _elm.id,
            'callback': function (evt) {
                reloadReturn(evt);

                // TODO c'est pas à cette endroit
                if (typeof (top.RunnableJobQueue) !== "undefined")
                    top.RunnableJobQueue.run();
            },
            'queued': false

        });
    }

    function reloadQueued() {

        //  setWidgetWait();

        oGridController.widget.reload({
            'id': getAttributeValue(_elm, "f"),
            'srcId': _elm.id,
            'callback': reloadReturn,
            'queued': true
        });
    }

    function setWidgetWait() {

        // Chargement en cours ...
        var waiter = document.createElement("div");

        var div = document.createElement("div");
        div.className = "xrm-widget-waiter";
        waiter.appendChild(div);

        var img = document.createElement("img");
        setAttributeValue(img, "alt", "wait");
        setAttributeValue(img, "src", "themes/default/images/wait.gif");
        div.appendChild(img);


        if (_elm) {
            var content = _elm.querySelector(".xrm-widget-content");
            if (content)
                content.innerHTML = waiter.innerHTML;
        }
    }

    function reloadReturn(result) {
        if (!result)
            return;

        saveState();

        var parent = _elm.parentElement;
        if (parent)
            parent.removeChild(_elm);

        var div = document.createElement("div");
        div.innerHTML = result.Html;

        _elm = div.querySelector("div[id='widget-wrapper-" + result.WidgetId + "']");
        if (parent)
            parent.appendChild(_elm);

        _elm.style.display = "block";

        restoreState();

        attachEvents();


        var size = { w: _elm.offsetWidth, h: _elm.offsetHeight };

        if (size.w == 0 || size.h == 0) {
            size.w = parseInt(_elm.style.width);
            size.h = parseInt(_elm.style.height);
        }

        updateContent(size.w, size.h);

        updateView({ 'action': 'refresh-content' });
    }

    function zoom() {
        oGridController.widget.zoom({
            'id': getAttributeValue(_elm, "f") * 1,
            'srcId': _elm.id
        });
    }

    function openlink() {
        // Ouverture de la page web dans un nouvel onglet
        var iframe = _elm.querySelector(".xrm-widget-content iframe");
        if (iframe) {
            window.open(iframe.src, "_blank");
        }
    }

    function dragStart(data) {

        saveState();
        _dragEvent(data);

        setAttributeValue(_elm, "active", "1");
        setAttributeValue(_elm, "action", data.action);
    }

    function dragMove(data) {
        updateState(data);
        _dragEvent(data);
    }

    function dragEnd(data) {


        updateState(data);
        data.action = 'commit';


        _dragEvent(data);

        updateView({ 'action': 'commit-content' });

        setTimeout(function () {
            setAttributeValue(_elm, "active", "0");
            setAttributeValue(_elm, "action", "idle");
        }, 600);
    }

    function updateState(data) {

        if (data.action == 'move') {
            _widgetBlockMem.left += data.deltaX;
            _widgetBlockMem.top += data.deltaY;
        }
        else
            if (data.action == 'resize') {
                _widgetBlockMem.width += data.deltaX;
                _widgetBlockMem.height += data.deltaY;

                updateContent(_widgetBlockMem.width, _widgetBlockMem.height);
            }

        _elm.style.top = _widgetBlockMem.top + "px";
        _elm.style.left = _widgetBlockMem.left + "px";
        _elm.style.width = _widgetBlockMem.width + "px";
        _elm.style.height = _widgetBlockMem.height + "px";
    }

    // Sauvegarde l'etat actuel
    function saveState() {
        _stateMem = new Object();
        _stateMem.x = getAttributeValue(_elm, 'x') * 1;
        _stateMem.y = getAttributeValue(_elm, 'y') * 1;
        _stateMem.w = getAttributeValue(_elm, 'w') * 1;
        _stateMem.h = getAttributeValue(_elm, 'h') * 1;

        _stateMem.top = _elm.style.top;
        _stateMem.left = _elm.style.left;
        _stateMem.width = _elm.style.width;
        _stateMem.height = _elm.style.height;
    }

    // Revenir à l'etat d'avant
    function restoreState() {
        if (_stateMem != null) {
            setAttributeValue(_elm, 'x', _stateMem.x);
            setAttributeValue(_elm, 'y', _stateMem.y);
            setAttributeValue(_elm, 'w', _stateMem.w);
            setAttributeValue(_elm, 'h', _stateMem.h);

            _elm.style.top = _stateMem.top;
            _elm.style.left = _stateMem.left;
            _elm.style.width = _stateMem.width;
            _elm.style.height = _stateMem.height;
        }
    }

    // Mis a jour la taille du contenu du wrappé en fonction du block
    function updateContent(width, height) {
        var contentDiv = _elm.querySelector("div[class='xrm-widget-content']");
        if (contentDiv) {
            var toolbar = _elm.querySelector("div[class='xrm-widget-titlebar']");
            if (toolbar) {
                var toolbarHeight = 38;
                height = height - toolbarHeight;
                toolbar.style.height = toolbarHeight + "px";
            }

            contentDiv.style.height = (height - 1) + "px";
            contentDiv.style.width = (width - 1) + "px";
        }

        updateView({ 'action': 'resize-content', 'width': width - 1, 'height': height - 1 });

    }

    // On encapsule la mise à jour du contenu dans un try-catch
    function updateView(data) {

        try {
            _view.update(data);

        } catch (ex) {

            // le widget contenu à fonctionner si les donnée à l'interieur pose souci
            oEvent.fire("log-error", { 'message': ex.message, 'name': ex.name, 'stack': ex.stack });

        }
    }

    function reset() {
        _widgetBlockMem = null;
        _stateMem = null;

    }

    // Lance le constructeur de l'objet
    init();

    return {
        'id': _id,
        'minSize': _view.minSize,
        'getContainer': function () { return _elm; },
        'widgetBlock': function () { return _widgetBlockMem; },
        'attr': function (attrName) {
            return getAttributeValue(_elm, attrName) * 1;
        },
        'show': function () {
            _elm.style.display = "block";

        },
        'redraw': function () {
            if (typeof (_view.redraw) == "function")
                _view.redraw();
        },
        'restoreState': function () {
            restoreState();
        },
        'save': function (callback) {
            var w = this;
            oGridController.pref.update({
                'evt': { src: _elm },
                'id': _id,
                'gridId': _context.gridId,
                'callback': function (result) { callback(result, w); }
            });
        },
        'cancel': function () {
            _toolbar.cancel();
        },
        'dragEvent': function (callback) {
            var that = this;
            _dragEvent = function (data) {
                data.widget = that;
                try {
                    callback(data);
                } catch (ex) {
                    console.log(ex);
                    _toolbar.cancel();
                }
            };
        },
        'update': function (widgetBlock) {

            reset();

            _widgetBlockMem = widgetBlock;

            _elm.style.left = widgetBlock.left + "px";
            _elm.style.top = widgetBlock.top + "px";
            _elm.style.width = widgetBlock.width + 'px';
            _elm.style.height = widgetBlock.height + 'px';

            setAttributeValue(_elm, 'x', widgetBlock.startX);
            setAttributeValue(_elm, 'y', widgetBlock.startY);
            setAttributeValue(_elm, 'w', widgetBlock.endX - widgetBlock.startX + 1);
            setAttributeValue(_elm, 'h', widgetBlock.endY - widgetBlock.startY + 1);

            updateContent(widgetBlock.width, widgetBlock.height);

            saveState();
        },
        'refresh': function () {
            reload();
        },
        'autoRefresh': function () {
            // mr => manuel refresh
            if (this.attr("mr") == 0 || this.attr("async") == 1)
                this.refresh();

        },
        'autoRefreshQueued': function () {
            // mr => manuel refresh
            if (this.attr("mr") == 0 || this.attr("async") == 1)
                reloadQueued();

        },
        'refreshSize': function () {
            this.update(_widgetBlockMem);
        },
        'onDelete': function (onDelete) { _onDelete = onDelete; }
    }
};
