
// IIFE pour garder le scope
var BingAPI = (function () {
    var map;
    var tools;
    var toolsTmp;
    var polygonLayer;
    var circleLayer
    var searchManager;
    var spiderManager;
    var centerLayer;
    var currentShape;
    var currentMode;
    var events = [];
    var pinEvents = [];

    var fontOption;
    var fontOptionVisited;
    var fontOptionSelected;

    var contextMenu;
    var infobox;

    var pinCircleRadius = 7;
    var pinCircleStrokeWidth = 2;

    var mapClickInfobox;
    var mapClickPolygone;
    var drawingChanged;
    var drawingModeChanged;
    var drawingChanging;

    var eventManager = {
        "map-loaded": function () { /* filtre sur les données*/ },
        "map-filtered": function (pins) { /* filtre sur les données*/ },
        "pushpin-hovered": function (pin) {/*clic sur le pushpin*/ },
        "pushpin-clicked": function (pin) {/*clic sur le pushpin*/ },
        "map-reset": function (pin) {/*clic sur le pushpin*/ },
        "data-refresh": function (pin) {/*clic sur le pushpin*/ }
    }
    var EventNames = {
        MAP_LOADED: "map-loaded",
        MAP_FILTERERD: "map-filtered",
        PUSHPIN_HOVERED: "pushpin-hovered",
        PUSHPIN_CLICKED: "pushpin-clicked",
        MAP_RESET: "map-reset",
        DATA_REFRESH: "data-refresh"
    };
    var style = {
        color: 'green',
        fillColor: 'rgba(0,255,0,0.15)',
        themeColor: 'rgba(60, 60,60, 1)',
        strokeColor: 'rgba(130, 162, 25, 1)',
        strokeThickness: 1
    };

    var mouseText;

    // Seules ces functions suivantes sont accessibles depuis l'exterieur et permettent d'interagir avec BingMaps
    var publicAPI = {
        "Init": internalInit,
        "Display": internalDisplayAll,
        "SelectPin": updatePinState,
        "LocateFromName": internalSearch,
        "GetMapBounds": getMapBounds,
        "GetMapCenter": getMapCenter,
        "AutoMapZoom": zoomIntoContainedPushpins,
        "Reset": internalReset,
        "ThemeChanged": changePinsColor,
        'Events': {
            'On': subscribe,
            'Off': unsubscribe
        },
        'Infobox': {
            'Select': selectInfobox,
            'Show': showInfobox,
            'Hide': hideInfobox
        }
    };
    function changePinsColor(newColor) {
        style.themeColor = newColor;
        fontOption = getPushpinFont('\uf041', 'eudoFonts', 30, style.themeColor);

        var shapes = centerLayer.getPrimitives();
        if (shapes && shapes.length > 0)
            shapes[0].setOptions(getPushpinFont('\uf192', 'eudoFonts', 28, style.themeColor));

        var pins = [];
        if (spiderManager) {
            pins = spiderManager.getClusterLayer().getPushpins();
            spiderManager.setOptions({ visible: false });
        }
        for (var i = 0; i < pins.length; i++) {
            if (!pins[i].metadata || pins[i].metadata.selected || pins[i].metadata.visited)
                continue;

            pins[i].setOptions(fontOption);
        }

        if (spiderManager)
            spiderManager.setOptions({ visible: true });
    }
    function internalSearch(locationName) {
        var menu = document.getElementById("DrawingModes");
        if (!menu)
            return;

        var searchTxt = menu.querySelector("#searchTxt");
        if (!searchTxt)
            return;

        var oldValue = searchTxt.value;
        searchTxt.value = locationName;
        //map
        if (locationName && locationName.length > 0 && oldValue != locationName)
            search(locationName);

    }
    function getMapBounds() {
        // Récupère le rectangle de la carte
        if (Microsoft.Maps.WellKnownText != null) {
            var currentPolygone = Microsoft.Maps.SpatialMath.locationRectToPolygon(map.getBounds());
            var shape = Microsoft.Maps.SpatialMath.Geometry.makeValid(new Microsoft.Maps.Polygon(currentPolygone.getRings()));
            return Microsoft.Maps.WellKnownText.write(shape);;
        }
        return null;
    }
    function getMapCenter() {
        if (Microsoft.Maps.WellKnownText != null) {
            var shapes = centerLayer.getPrimitives();
            var currentCenter;
            if (shapes == null || shapes.length == 0)
                currentCenter = map.getCenter();
            else
                currentCenter = shapes[0];

            var shape = Microsoft.Maps.SpatialMath.Geometry.makeValid(currentCenter);
            return Microsoft.Maps.WellKnownText.write(shape);
        }
        return null;
    }
    function setMap(options) {
        map = new Microsoft.Maps.Map('#CartoSelection', {
            showBreadcrumb: false,
            showDashboard: true,
            showLocateMeButton: false,
            showMapTypeSelector: true,
            showScalebar: true,
            showZoomButtons: false,
            showNavigationBar: false
        });

        Microsoft.Maps.registerModule('SpiderClusterManager', 'Scripts/bingmaps/modules/SpiderClusterManager.js');
        Microsoft.Maps.loadModule([
            'Microsoft.Maps.DrawingTools',
            'Microsoft.Maps.SpatialMath',
            'Microsoft.Maps.SpatialDataService',
            'Microsoft.Maps.WellKnownText',
            'Microsoft.Maps.Search',
            'SpiderClusterManager'], function () {

                centerLayer = new Microsoft.Maps.Layer();
                map.layers.insert(centerLayer);

                polygonLayer = new Microsoft.Maps.Layer();
                map.layers.insert(polygonLayer);

                circleLayer = new Microsoft.Maps.Layer();
                circleLayer.setZIndex(20000);
                map.layers.insert(circleLayer);

                tools = new Microsoft.Maps.DrawingTools(map);
                toolsTmp = new Microsoft.Maps.DrawingTools(map);

                searchManager = new Microsoft.Maps.Search.SearchManager(map);

                style.themeColor = document.getElementById("backColor").value;

                fontOption = getPushpinFont('\uf041', 'eudoFonts', 30, style.themeColor);
                fontOptionVisited = getPushpinFont('\uf041', 'eudoFonts', 30, "rgba(188, 176, 11, 1)");// "rgba(251, 188, 5,1)");
                fontOptionSelected = getPushpinFont('\uf041', 'eudoFonts', 30, "rgba(109, 170, 11, 1)");// "rgba(52, 168, 83, 1)");


                setCenterPin(map.getCenter());
                fireEvent(EventNames.MAP_LOADED);

            });
    }
    function selectInfobox(data) {

        updatePinState({ "id": data.id, "selected": data.selected });

        // infobulle pas ouverte
        if (!infobox)
            return;

        // pas la meme fiche
        if (infobox.metadata.id != data.id)
            return;

        var __customInfobox__ = document.getElementById("__customInfobox__");
        var cbo = __customInfobox__.querySelector("input[type='checkbox']");
        if (cbo) {
            cbo.checked = data.selected == true;
        }
    }
    function updatePinState(pinState) {

        var pin = getPin(pinState.id);
        if (pin) {

            if (spiderManager)
                spiderManager.hideSpiderCluster();

            if (typeof (pinState.selected) != "undefined")
                pin.metadata.selected = pinState.selected == true;

            if (typeof (pinState.visited) != "undefined")
                pin.metadata.visited = pinState.visited == true;

            if (pin.metadata.selected)
                pin.setOptions(fontOptionSelected);
            else if (pin.metadata.visited)
                pin.setOptions(fontOptionVisited);
            else
                pin.setOptions(fontOption);
        }
    }
    function getPin(id) {
        var pins = [];
        if (spiderManager)
            pins = spiderManager.getClusterLayer().getPushpins();

        if (pins.length == 0)
            return null;
        for (var i = 0; i < pins.length; i++) {
            if (!pins[i].metadata)
                continue;

            if (pins[i].metadata.id == id)
                return pins[i];
        }
    }
    function showInfobox(data) {

        var template = "<div id='__customInfobox__' class='customInfobox'>" + data.html + "</div>";
        if (!infobox) {
            infobox = new Microsoft.Maps.Infobox(data.location, {
                htmlContent: template,
                visible: true
            });

            infobox.setMap(map);
        } else {
            infobox.setLocation(data.location);
            infobox.setOptions({
                htmlContent: template,
                visible: true
            });
        }
        if (typeof (infobox.metadata) == "undefined")
            infobox.metadata = {};

        infobox.metadata.id = data.id;

        // updatePinState({ "id": data.id, "visited": true });

        // On passe le conteneur au client pour ajouter des evenments
        var __customInfobox__ = document.getElementById("__customInfobox__");
        if (typeof (data.setEvents) == "function")
            data.setEvents(__customInfobox__);


        mapClickInfobox = Microsoft.Maps.Events.addHandler(map, 'click', hideInfobox);
    }
    function hideInfobox() {
        if (infobox)
            infobox.setOptions({ visible: false });

        if (mapClickInfobox)
            Microsoft.Maps.Events.removeHandler(mapClickInfobox);
    }
    function subscribe(name, handler) {
        if (!eventManager.hasOwnProperty(name) || typeof handler != "function")
            return;

        eventManager[name] = handler;
    }
    function unsubscribe(name) {
        if (!eventManager.hasOwnProperty(name))
            return;
        eventManager[name] = function (data) { };
    }
    function fireEvent(name, args) {
        if (!eventManager.hasOwnProperty(name)) {
            console.log("Pas d'event " + name);
            return;
        }
        eventManager[name](args);
    }
    function internalInit(options) {
        setMap(options);
        setEvents();
    }
    function internalDisplay(data) {
        if (Microsoft.Maps.WellKnownText == null)
            return;

        var shape = Microsoft.Maps.WellKnownText.read(data.geo);
        var location = shape.getLocation();
        var pushpin = new Microsoft.Maps.Pushpin(shape.getLocation(), {});
        pushpin.setOptions(fontOption);
        pushpin.metadata = data.metadata;

        return pushpin;
    }
    function internalDisplayAll(sources) {
        if (typeof (sources) != "object")
            return;

        if (!Array.isArray(sources))
            sources = [sources];

        var layer = [];
        for (var i = 0; i < sources.length; i++) {
            layer.push(internalDisplay(sources[i]));
        }

        if (spiderManager) {
            spiderManager.getClusterLayer().clear();
            spiderManager.dispose();
        }
        spiderManager = new SpiderClusterManager(map, layer, {
            clusteredPinCallback: function (clusterPushpin) {
                createCustomCluster(clusterPushpin);
            },
            pinSelected: function (pin, cluster) {
                if (pin && pin.metadata) {
                    pin.metadata.visited = true;
                    if (!pin.selected)
                        pin.setOptions(fontOptionVisited);

                    fireEvent(EventNames.PUSHPIN_CLICKED, {
                        'metadata': pin.metadata,
                        'location': pin.getLocation()
                    });
                }
            },
            pinUnselected: function () {
                hideInfobox();
            },
            gridSize: 80
        });
    }
    function createCustomCluster(clusterPushpin) {
        //rayon minimum du cluster
        var minRadius = 12;

        var clusterSize = clusterPushpin.containedPushpins.length;

        // Calcule le nombre de chiffre
        // de 0 à 9 -> 1 chiffres
        // de 10 à 99 -> 2 chiffres
        // de 100 à 999 -> 3 chiffres
        // On utilise le log 10, Math.log est le logarithme népérien
        var nbNumbers = Math.log(clusterSize) / Math.log(10);

        // 5 pixels/chiffre + le minimum pour que ca soit visible 
        var radius = nbNumbers * 7 + minRadius;

        //svg icon
        var svg = ['<svg xmlns="http://www.w3.org/2000/svg" width="', (radius * 2), '" height="', (radius * 2), '">',
            '<circle cx="', radius, '" cy="', radius, '" r="', (radius - radius / 6), '" fill="none"  stroke="', style.themeColor, '"/>',
            '<circle cx="', radius, '" cy="', radius, '" r="', (radius - radius / 3), '"  fill="', style.themeColor, '"/>',
           '</svg>'];

        // mise a jour des options du clusterPushpin
        clusterPushpin.setOptions({
            icon: svg.join(''),
            anchor: new Microsoft.Maps.Point(radius, radius),
            textOffset: new Microsoft.Maps.Point(0, radius - 7),
            enableClickedStyle: true,
            enableHoverStyle: true
        });
    }
    function zoomIntoContainedPushpins() {
        if (!spiderManager)
            return;

        var shapes = spiderManager.getClusterLayer().getPrimitives();
        var bounds = Microsoft.Maps.SpatialMath.Geometry.bounds(shapes);
        map.setView({ center: bounds.center, bounds: bounds, padding: 30 });
    }
    function setCenterPin(centerPin) {
        centerLayer.clear();
        var center = new Microsoft.Maps.Pushpin(centerPin, { draggable: true });
        center.setOptions(getPushpinFont('\uf192', 'eudoFonts', 28, style.themeColor)); //"rgba(20, 20, 20, 1)"))
        centerLayer.add(center);
    }
    function getPushpinFont(text, fontName, fontSizePx, color) {
        var c = document.createElement('canvas');
        var ctx = c.getContext('2d');
        //Define font style
        var font = fontSizePx + 'px ' + fontName;
        ctx.font = font;
        //Resize canvas based on sie of text.
        var size = ctx.measureText(text);
        c.width = size.width;
        c.height = fontSizePx;
        //Reset font as it will be cleared by the resize.
        ctx.font = font;
        ctx.textBaseline = 'top';
        ctx.fillStyle = color;
        ctx.antialias = true;
        ctx.fillText(text, 0, 0);
        return {
            icon: c.toDataURL(),
            anchor: new Microsoft.Maps.Point(c.width / 2, c.height / 2)
        };
    }
    function findIntersecting(shapeArea) {
        var selectedPins = findIntersectingData(shapeArea);
        if (selectedPins == null)
            selectedPins = [];

        fireEvent(EventNames.MAP_FILTERERD, selectedPins);
    }
    function internalReset() {

        tools.finish();
        toolsTmp.finish();

        hideInfobox();
        resetDrawingModes();
        resetDrawingState();      

        pinEvents.push(mapClickPolygone);
        pinEvents.push(mapClickInfobox);
        pinEvents.push(drawingChanged);
        pinEvents.push(drawingChanging);
        pinEvents.push(drawingModeChanged);

        for (var i = 1; i < pinEvents.length; i++) {
            Microsoft.Maps.Events.removeHandler(pinEvents[i]);
        }

        pinEvents = [];

        if (spiderManager) {
            spiderManager.getClusterLayer().clear();
            spiderManager.dispose();
            spiderManager = null;
        }

      
        fireEvent(EventNames.MAP_RESET);
    }
    function setEvents() {
        var menu = document.getElementById("DrawingModes");
        if (!menu)
            return;

        var items = menu.querySelectorAll("span[class^='icon-'");
        Array.prototype.slice.call(items).forEach(function (menuItem) {
            setEventListener(menuItem, "click", function (evt) {
                var src = evt.srcElement || evt.target;
                if (!src)
                    return;

                var type = getAttributeValue(src, "item-type");
                while (type == null || type == "") {
                    src = src.parentElement;
                    if (src.id == "DrawingModes")
                        return;

                    type = getAttributeValue(src, "item-type");
                }

                switch (type) {
                    case "zoom-in":
                    case "zoom-out":
                        setZoom(type);
                        break;
                    case "center":
                        setCenterPin(map.getCenter());
                        break;
                    case "bullseye":
                        setUserLocation();
                        break;
                    case "delete":
                        internalReset();
                        break;
                    case "refresh":
                        internalReset();
                        fireEvent(EventNames.DATA_REFRESH);
                        break;
                    case "search":
                        break;
                    default:
                        setDrawingMode(type, src);
                        break;
                }
            });
        });

        // Recherche sur Bing
        var searchTxt = menu.querySelector("#searchTxt");
        if (searchTxt) {
            setEventListener(searchTxt, "change", function (evt) {
                searchValue(evt);
            });
            setEventListener(searchTxt, "keydown", function (evt) {
                if (event.key != "Enter")
                    return;
                searchValue(evt);
            });
        };

        // 'esc', supprime le polygone.
        var mapCtn = document.getElementById("CartoSelection");
        setEventListener(mapCtn, "keypress", function (evt) {
            if (evt.charCode === 27) {
                setMode(null);
            }
        });

        var div = document.createElement("div");
        Array.prototype.slice.call(items).forEach(function (item) {

            var itemClone = item.cloneNode(true);
            itemClone.style.display = "inline-block";
            itemClone.innerText = getAttributeValue(item, "title");
            div.appendChild(itemClone);
        });

        var ct = ["<div id='cntnr'>",
                    "<ul id='items'>",
                      "<li class='icon-polygon'><span>Déssiner une sélection</span></li>",
                      "<li class='icon-refresh'><span>Relancer la recherche</span></li>",
                    "</ul>",
                    "<hr />",
                    "<ul id='items'>",
                        "<li class='icon-delete'><span>Réinitialiser</span></li>",
                   "</ul>",
                   " <hr />",
                   "<ul id='items'>",
                      "<li class='icon-zoom-out'><span>Agrandir la zone</span></li>",
                      "<li class='icon-zoom-in'><span>Réduire la zone</span></li>  ",
                    "</ul>",
                    " <hr />",
                    "<ul id='items'>",
                      "<li class='icon-dot-circle-o'><span>Déplacer le repère</span></li>",
                      "<li class='icon-target'><span>Votre position</span></li>",
                    "</ul>",
                  "</div>"];

        // contextMenu = new Microsoft.Maps.Infobox(map.getCenter(), {
        //     htmlContent: '<div id="__contextMenuDiv__">' + ct.join('') + '</div>',
        //     visible: false
        // });


        //// contextMenu.setMap(map);



        // Microsoft.Maps.Events.addHandler(map, 'rightclick', function (e) {
        //     var __contextMenuDiv__ = document.getElementById("__contextMenuDiv__");

        //     contextMenu.setOptions({
        //         location: e.location,
        //         visible: true,
        //         offset: new Microsoft.Maps.Point(0, -__contextMenuDiv__.offsetHeight),
        //     });
        // });        
        // document.body.onmousedown = function () {
        //     closeContextMenu();
        // };
    }
    function closeContextMenu() {
        contextMenu.setOptions({ visible: false });
    }
    function setUserLocation() {
        navigator.geolocation.getCurrentPosition(function (position) {
            var location = new Microsoft.Maps.Location(position.coords.latitude, position.coords.longitude);
            map.setView({ center: location, zoom: 15 });
            setCenterPin(location);
        });
    }
    function setZoom(type) {

        var scale = type == "zoom-in" ? 1 : -1;

        var zoom = map.getZoom();
        zoom = zoom + scale;

        if (zoom <= 0) {
            zoom = 1;
        } else if (zoom >= 21) {
            zoom = 21;
        }

        map.setView({ zoom: zoom });
    }
    function setDrawingMode(mode, elm) {
        resetDrawingModes();
        setAttributeValue(elm, 'active', '1');
        switch (mode) {
            case 'polygon':
                drawPolygon(elm);
                break;
            default:
                break;
        }
    }
    function drawPolygon(elm) {
        if (setMode('polygon')) {
            var path = elm.querySelector("#plygonePath");
            if (path) {
                path.style.stroke = "#79BE0B";
                path.style.fill = "#82a2194f";

            }
            var circles = elm.querySelector("#circles");
            if (circles) {
                circles.style.stroke = "#79BE0B";
                circles.style.fill = "#79BE0B";
            }

            toolsTmp.create(Microsoft.Maps.DrawingTools.ShapeType.polyline, function (polyline) {
                polyline.setOptions(style);
                currentShape = polyline;
            });

            if (drawingChanging)
                Microsoft.Maps.Events.removeHandler(drawingChanging);

            drawingChanging = Microsoft.Maps.Events.addHandler(toolsTmp, 'drawingChanging', function (currentPolyline) {
                updatePolygon(currentPolyline);
            });


            if (drawingModeChanged)
                Microsoft.Maps.Events.removeHandler(drawingModeChanged);

            drawingModeChanged = Microsoft.Maps.Events.addHandler(toolsTmp, 'drawingModeChanged', function (event) {
                if (event.mode == Microsoft.Maps.DrawingTools.DrawingMode.edit)
                    finishPolygon(true);
            });
        }
    }
    function updatePolygon(polyline) {

        drawPoly(polyline);
        var loc = polyline.getLocations();
        if (loc.length < 3)
            return;

        var firstLocation = loc[0];
        var lastLocation = loc[loc.length - 1];

        var scale = map.getMetersPerPixel(); // metre/Pixel
        var distance = Microsoft.Maps.SpatialMath.Geometry.distance(firstLocation, lastLocation);//metre
        var deltaPixel = distance / scale;

        var circles = circleLayer.getPrimitives();
        if (deltaPixel > pinCircleRadius) {
            if (circles && circles.length > 0)
                circles[0].setOptions({ visible: false });
            return;
        }

        if (circles && circles.length > 0)
            circles[0].setOptions({ visible: true });

        lastLocation = loc[loc.length - 2];
        scale = map.getMetersPerPixel(); // metre/Pixel
        distance = Microsoft.Maps.SpatialMath.Geometry.distance(firstLocation, lastLocation);//metre
        deltaPixel = distance / scale;
        if (deltaPixel <= pinCircleRadius) {
            finishPolygon(false);
        }
    }
    function drawPoly(polyline) {
        var loc = polyline.getLocations();
        var s = {
            color: 'green',
            fillColor: 'rgba(0,255,0,0.15)',
            themeColor: 'rgba(60, 60,60, 1)',
            strokeColor: 'rgba(130, 162, 25, 1)',
            strokeThickness: 0
        };

        var polygons = polygonLayer.getPrimitives();
        if (polygons && polygons.length > 0) {
            polygons[0].setLocations(loc);
        } else {
            polygonLayer.clear();
            polygonLayer.add(new Microsoft.Maps.Polygon(loc, s));
        }

        var polygons = circleLayer.getPrimitives();
        if (!polygons || polygons.length == 0) {
            circleLayer.clear();
            circleLayer.add(createCirclePushpin(loc[0], pinCircleRadius, "#fff", s.strokeColor, pinCircleStrokeWidth));
        }

        tools.finish();
        tools.edit(polyline);
    }
    function finishPolygon(fromChangeMode) {
        polygonLayer.clear();
        circleLayer.clear();
        toolsTmp.finish(function (polyline) {
            var loc = polyline.getLocations();
            if (!fromChangeMode)
                loc.splice(-1, 1);
            else {
                var firstLocation = loc[0];
                var lastLocation = loc[loc.length - 1];

                var scale = map.getMetersPerPixel(); // metre/Pixel
                var distance = Microsoft.Maps.SpatialMath.Geometry.distance(firstLocation, lastLocation);//metre
                var deltaPixel = distance / scale;

                if (deltaPixel < pinCircleRadius)
                    loc.splice(-1, 1);
            }

            currentShape = new Microsoft.Maps.Polygon(loc, style);
            tools.finish();


            tools.edit(currentShape);
            findIntersecting(currentShape);

            if (drawingChanged)
                Microsoft.Maps.Events.removeHandler(drawingChanged);

            drawingChanged = Microsoft.Maps.Events.addHandler(tools, 'drawingChanged', function (currentPolyline) {
                findIntersecting(currentPolyline);
            });
        });
    }
    function createCirclePushpin(location, radius, fillColor, strokeColor, strokeWidth) {
        strokeWidth = strokeWidth || 0;
        var svg = ['<svg xmlns="http://www.w3.org/2000/svg" width="', (radius * 2),
            '" height="', (radius * 2), '"><circle cx="', radius, '" cy="', radius, '" r="',
            (radius - strokeWidth), '" stroke="', strokeColor, '" stroke-width="', strokeWidth, '" fill="', fillColor, '"/></svg>'];
        return new Microsoft.Maps.Pushpin(location, {
            icon: svg.join(''),
            anchor: new Microsoft.Maps.Point(radius, radius),
            visible: false,
            zIndex: 1
        });
    }
    function setMode(mode) {
        for (var i = 0; i < events.length; i++) {
            Microsoft.Maps.Events.removeHandler(events[i]);
        }
        events = [];
        var state = false;
        if (currentMode === mode || mode == null) {
            currentMode = null;
            resetDrawingModes();
            resetDrawingState();
        } else {
            currentMode = mode;
            state = true;
        }

        return state;
    }
    function resetDrawingState() {
        if (currentShape) {
            tools.finish();
            currentShape = null;
        }
    }
    function clearSelection(parentId, className) {
        var parent = document.getElementById(parentId);
        for (var i = 0; i < parent.children.length; i++) {
            parent.children[i].className = className;
        }
    }
    function resetDrawingModes() {
        var parent = document.getElementById("DrawingModes");
        for (var i = 0; i < parent.children.length; i++) {
            setAttributeValue(parent.children[i], 'active', '0');
        }

        var path = parent.querySelector("#plygonePath");
        if (path) {
            path.style.stroke = "#ffffff";
            path.style.fill = "none";
        }
        var circles = parent.querySelector("#circles");
        if (circles) {
            circles.style.stroke = "#ffffff";
            circles.style.fill = "#ffffff";

        }
    }
    function findIntersectingData(searchArea) {
        //Find all pushpins on the map that intersect with the drawn search area.
        //Ensure that the search area is a valid polygon, should have 4 Locations in it's ring as it automatically closes.
        if (searchArea && searchArea.getLocations().length >= 4) {
            //Get all the pushpins from the drawingLayer.
            var pins = [];
            if (spiderManager)
                pins = spiderManager.getClusterLayer().getPushpins();

            if (pins.length == 0)
                return null;

            //Using spatial math find all pushpins that intersect with the drawn search area.
            //The returned data is a copy of the intersecting data and not a reference to the original shapes, 
            //so making edits to them will not cause any updates on the map.
            var intersectingPins = Microsoft.Maps.SpatialMath.Geometry.intersection(pins, searchArea);
            //The data returned by the intersection function can be null, a single shape, or an array of shapes. 
            if (intersectingPins) {
                //For ease of usem wrap individudal shapes in an array.
                if (intersectingPins && !(intersectingPins instanceof Array)) {
                    intersectingPins = [intersectingPins];
                }

                var selectedPins = [];
                //Loop through and map the intersecting pushpins back to their original pushpins by comparing their coordinates.
                for (var j = 0; j < intersectingPins.length; j++) {
                    for (var i = 0; i < pins.length; i++) {
                        if (!pins[i].metadata)
                            continue;

                        if (Microsoft.Maps.Location.areEqual(pins[i].getLocation(), intersectingPins[j].getLocation())) {

                            if (pins[i].metadata) {
                                pins[i].setOptions(fontOptionVisited);
                                pins[i].metadata.visited = true;
                                selectedPins.push({ metadata: pins[i].metadata });
                            } else {
                                console.log(pins[i]);
                            }
                            //  break;
                        }
                    }
                }
                //Return the pushpins that were selected.
                return selectedPins;
            }
        }
        return null;
    }
    function resetPinsFont(pins) {

        //Clear any previously selected data.
        for (var i = 0; i < pins.length; i++) {
            // if (!pins[i].metadata.selected)
            pins[i].setOptions(fontOption);
        }
    }
    function searchValue(evt) {
        var src = evt.srcElement || evt.target;
        if (!src || src.value == "")
            return;

        var oldValue = getAttributeValue(src, "oldValue");
        if (src.value == oldValue)
            return;

        setAttributeValue(src, "oldValue", src.value);
        search(src.value);
    }
    function search(value) {
        searchManager.geocode({
            where: value,
            callback: setSearchResult,
            errorCallback: function (e) { console.log("Pas de résultat " + e); }
        });
    }
    function setSearchResult(data) {
        if (data && data.results && data.results.length > 0) {
            map.setView({ bounds: data.results[0].bestView });
            setCenterPin(data.results[0].bestView.center);
        }
    }

    return publicAPI;
}());