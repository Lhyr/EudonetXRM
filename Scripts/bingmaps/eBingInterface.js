
// Interface avec BingMaps v8.0
// Tous les appels vers l'api Bings se font via cet objet
// Il ne faut pas changer le nom de l'objet
var oMaps = (function () {

    var config = null;
    var map = null;
    var infoboxLayer = null;
    var pinLayer = null;
    var polygonLayer = null;
    var viewchangeend = null;

    var clusterLayer = null;
    var zoom = 0;
    var location = null;

    var pinsData = null;
    var drawingManager = null;
    var hoverBox = null;
    var infobox = null;

    // draw
    var polygon = null;
    var pins = null;

    var NORMAL_PUSHPIN_ZOOM = 1;
    var HIGHLIGHT_PUSHPIN_ZOOM = 1.2;

    //Interaction 
    var clusterZoomStack = [];
    var clusterPushpins = null;

    //Mode edition
    var firstPoint = null;
    var editMode = false;
    var loc;

    // Modules chargés au chargement de la carte
    //var wktManager = null;
    var searchManager = null;

    var drawingManager = null;
    var drawingToolsManager = null;

    var imageGenerator;

    // Height : max 116, min 40  
    var infoboxTemplate = '<div id="InfoBoxCtnr" class="customInfoBoxCtnr" onclick="oMaps.infoBoxClick(event);">{pinTemplate}<div class="infobox-pointer"><div class="infobox-pointer-border"></div><div class="infobox-pointer-background"></div></div><div>';
    var infoboxPinTemplate = '<div class="customInfobox" {cHeight}><div class="imageCtr" {height} >{img}<div class="title" onClick="{titleOnClick}" title="{title}">{title}</div><a class="infoboxmarkCbo rChk chk chkAction" href="#"><span fid="{id}" class="infoboxmark {marked}" title="' + top._res_293 + '"></span></a></div><div class="detail" title="{title}">{description}</div></div>';
    // icon-title_sep
    var svg = '<svg xmlns="http://www.w3.org/2000/svg" width="24"  height="40"><g transform="scale({zoom})"><path fill="{bgcolor}" fill-opacity="0.8" fill-rule="nonzero" id="pushpinId" d="M-0.03322,8.51755L-0.03322,8.08277C-0.14192,4.18564 3.55344,0.23913 7.97783,0.23913L7.97783,0.23913C10.10248,0.23913 12.14012,1.0655 13.64249,2.53648C15.14485,4.00744 15.98887,6.00251 15.98887,8.08277L15.98887,8.08277C15.98887,12.41469 9.68437,28.62046 8.0396,32.50764L7.97783,32.56838C6.70365,29.28857 0.18417,13.06687 -0.03322,8.51755zM2.44017,8.29944L2.44017,8.08277C2.33073,11.65703 5.43733,14.0629 8.30614,13.84623C11.17496,13.62956 13.73436,11.5487 13.73436,8.08277C13.73436,4.61684 10.19001,2.42765 8.08726,2.64431L7.86838,2.64432C5.43732,2.64432 2.44017,4.61683 2.44017,8.29944z" /><path fill="{color}"  fill-opacity="{opacity}" fill-rule="nonzero"  d="M2.1461863144342654,8.263995430394615 C2.1461863144342654,5.12344225378068 4.775192814528167,2.5795941807234115 8.020879851681169,2.5795941807234115 C11.266566888834145,2.5795941807234115 13.89557338892807,5.12344225378068 13.89557338892807,8.263995430394615 C13.89557338892807,11.404548607008525 11.266566888834145,13.948396680065812 8.020879851681169,13.948396680065812 C4.775192814528167,13.948396680065812 2.1461863144342654,11.404548607008525 2.1461863144342654,8.263995430394615 z" id="conditionalColor"/></g></svg>';

    function attachEvents() {
        viewchangeend = Microsoft.Maps.Events.addHandler(map, 'viewchangeend', function (evt) { location = map.getCenter(); });

        Microsoft.Maps.Events.addHandler(map, 'rightclick', mapMouseRightClickHandler);
    }
    function mapMouseRightClickHandler(evt) {

        infobox.setOptions({ visible: false });
        if (editMode == false && clusterLayer != null) {

            zoomIntoContainedPushpins(clusterLayer.getPushpins());
            /*

            if (clusterZoomStack.length > 1) {
                var oZoom = clusterZoomStack.pop().pop();
                map.setView({ center: oZoom.center, bounds: oZoom.bounds, padding: 80 });
            }
            else
                zoomIntoContainedPushpins(clusterLayer.getPushpins());
                */
        }
    }
    function loadModules(options) {


        Microsoft.Maps.loadModule('Microsoft.Maps.WellKnownText', function () {
            // wktManager = Microsoft.Maps.WellKnownText;
            if (options.wktModuleCallback && typeof options.wktModuleCallback === "function") {
                options.wktModuleCallback();
            }
        });

        Microsoft.Maps.loadModule('Microsoft.Maps.Search', function () {
            searchManager = new Microsoft.Maps.Search.SearchManager(map);
        });

        // Convert LocationRect to Polygon
        Microsoft.Maps.loadModule('Microsoft.Maps.SpatialMath', function () {
            // spatialMathManager = Microsoft.Maps.SpatialMath;
        });

        Microsoft.Maps.loadModule('Microsoft.Maps.DrawingTools', function () {

            drawingToolsManager = new Microsoft.Maps.DrawingTools(map);

            document.onkeydown = function (e) {
                e = e || window.event;
                if (e.charCode == 27) {
                    drawingToolsManager.finish(shapeDrawn);
                    currentPolygone = null;
                }
            };

        });

        Microsoft.Maps.loadModule("Microsoft.Maps.Clustering", function () {
            displayPushPins();
        });

        //Register and load the module.
        //Microsoft.Maps.registerModule('MapImageGeneratorModule', 'Scripts/bingmaps/modules/MapImageGeneratorModule.js');
        //Microsoft.Maps.loadModule('MapImageGeneratorModule', function () {
        //    imageGenerator = new MapImageGenerator(map);
        //});

    }

    var mouseleaveEventHandler = null;
    var mouseenterEventHandler = null;
    var mouseClickInfoboxEventHandler = null;
    var mouseClickEventHandler = null;
    var currentPolygone = null;

    function startDraw() {

        if (currentPolygone) {
            drawingToolsManager.finish(shapeDrawn);
            currentPolygone = null;
        }

        drawingToolsManager.create(Microsoft.Maps.DrawingTools.ShapeType.polygon, function (newShape) {
            currentPolygone = newShape;

            // couleur hex :#a8d31f          
            currentPolygone.setOptions({
                fillColor: new Microsoft.Maps.Color(0.15, 168, 211, 31),
                strokeColor: new Microsoft.Maps.Color(1, 168, 211, 31),
                strokeThickness: 2
            });
        });

        mouseClickEventHandler = Microsoft.Maps.Events.addHandler(map, 'click', function (evt) { editPolygon(evt); });
    }
    function editPolygon(evt) {

        if (currentPolygone) {

            drawingToolsManager.finish(shapeDrawn);
            var loc = currentPolygone.getLocations();
            loc.unshift(evt.location); // insert le nouveau point 

            if (loc.length > 2)
                loc = Microsoft.Maps.SpatialMath.Geometry.convexHull(loc, false, false).getLocations();


            currentPolygone.setLocations(loc);
            drawingToolsManager.edit(currentPolygone);
        }
    }
    function shapeDrawn(s) {
        currentPolygone = s;
    }
    function clearDrawing() {
        if (currentPolygone) {
            drawingToolsManager.finish(shapeDrawn);
        }
        currentPolygone = null;
        Microsoft.Maps.Events.removeHandler(mouseClickEventHandler);
    }

    function createGeolocPushpin(txtWktId, txtLatId, txtLonId) {

        var pushpin;

        var txtLat = document.getElementById(txtLatId);
        var txtLon = document.getElementById(txtLonId);
        var txtWkt = document.getElementById(txtWktId);

        var txtWkt = document.getElementById(txtWktId);
        if (txtWkt && txtWkt.value) {
            pushpin = Microsoft.Maps.WellKnownText.read(txtWkt.value);
            pushpin.setOptions({ draggable: true });
        }
        else {
            pushpin = new Microsoft.Maps.Pushpin(map.getCenter(), {
                draggable: true
            });
            if (txtWkt) {
                txtWkt.value = Microsoft.Maps.WellKnownText.write(pushpin);
            }
        }
            

        Microsoft.Maps.Events.addHandler(pushpin, 'dragend', function (evt) {
            var location = evt.target.getLocation();

            if (txtLat)
                txtLat.value = location.latitude;
            if (txtLon)
                txtLon.value = location.longitude;
            //#67 479: KJE le 15/01/2020
            //on ajoute un espace entre "POINT" et "("
            if (txtWkt)
                txtWkt.value =  Microsoft.Maps.WellKnownText.write(evt.target).replace('POINT(','POINT (');
        });
        map.entities.push(pushpin);

        var location = pushpin.getLocation();
        if (txtLat)
            txtLat.value = location.latitude;
        if (txtLon)
            txtLon.value = location.longitude;
        map.setView({
            center: location,
            zoom: 15
        });

        return pushpin;
        
    }

    function displayPushPins() {
        if (pinsData.length == 0)
            return;

        pins = new Array();

        var pushpin;
        var pin;
        var data;
        var location;
        for (var i = 0; i < pinsData.length; i++) {

            data = pinsData[i];

            // Pas de pushpin si pas de coordonnées gps
            if (data.geo.value.trim().length == 0)
                continue;

            var bHasRuleColor = data.pushpin.ruleColor.trim() != "";

            // Si le pushpin a été visité, on met la coleur gray           
            var icon = svg.replace("{bgcolor}", data.pushpin.bgColor).replace("{zoom}", NORMAL_PUSHPIN_ZOOM);
            icon = icon.replace("{opacity}", bHasRuleColor ? "0.8" : "0");

            var color = bHasRuleColor ? data.pushpin.ruleColor : "#fff";

            pushpin = new Microsoft.Maps.Pushpin(Microsoft.Maps.WellKnownText.read(data.geo.value).getLocation(), {
                icon: icon,
                anchor: new Microsoft.Maps.Point(8, 30),
                enableClickedStyle: true,
                enableHoverStyle: true,
                color: color,
                zIndex: 20
            });

            pushpin.metadata = {
                fileId: data.id,
                marked: data.marked,
                visited: false,
                color: color,
                bgColor: data.pushpin.bgColor,
                hasRuleColor: bHasRuleColor,
                info: data.infobox
            };

            Microsoft.Maps.Events.addHandler(pushpin, 'mouseover', function (evt) {
                oEvent.fire("pushpin-over", { metadata: evt.target.metadata });

                var icon = svg
                .replace("{bgcolor}", evt.target.metadata.bgColor)
                .replace("{opacity}", evt.target.metadata.hasRuleColor ? "1" : "0")
                .replace("{color}", evt.target.metadata.color)
                .replace("{zoom}", HIGHLIGHT_PUSHPIN_ZOOM);

                evt.target.setOptions({ icon: icon, anchor: new Microsoft.Maps.Point(8 * HIGHLIGHT_PUSHPIN_ZOOM, 30 * HIGHLIGHT_PUSHPIN_ZOOM) });

            });
            Microsoft.Maps.Events.addHandler(pushpin, 'mouseout', function (evt) {
                oEvent.fire("pushpin-out", { metadata: evt.target.metadata });
                var icon = svg
                    .replace("{bgcolor}", evt.target.metadata.bgColor)
                    .replace("{opacity}", evt.target.metadata.hasRuleColor ? "1" : "0")
                    .replace("{color}", evt.target.metadata.color)
                    .replace("{zoom}", NORMAL_PUSHPIN_ZOOM);

                evt.target.setOptions({ icon: icon, anchor: new Microsoft.Maps.Point(8, 30) });

            });

            Microsoft.Maps.Events.addHandler(pushpin, 'click', animate);

            pins.push(pushpin);

        }



        clusterZoomStack = [];
        zoomIntoContainedPushpins(pins);

        if (clusterLayer != null) {
            clusterLayer.clear();
            map.layers.remove(clusterLayer);
        }

        clusterLayer = new Microsoft.Maps.ClusterLayer(pins, {
            clusteredPinCallback: createCustomCluster
        });

        map.layers.insert(clusterLayer);


    }

    function animate(evt) {

        var origin = null;
        var last = null;
        var destination = null;
        var destinationToOrigin = null;
        var current = null;
        var elapsed = 10;
        var lastTime = null;

        function update(evt) {
            current = new Date().getTime();

            if (infobox != null) {
                infobox.setOptions({ visible: false });
            }

            if (destination == null)
                destination = evt.target.getLocation();

            if (origin == null) {
                origin = map.getCenter();
                last = origin;
                destinationToOrigin = Microsoft.Maps.SpatialMath.Geometry.distance(destination, origin);
                lastTime = current;
            }

            elapsed = (current - lastTime) / 1000;

            var lastToOrigin = Microsoft.Maps.SpatialMath.Geometry.distance(last, origin);


            // la distance min pour arreter l'animation en fonction de l'echelle de la carte
            if ((destinationToOrigin - lastToOrigin) <= (map.getMetersPerPixel() / 10)) {

                showInfobox(evt);
                origin = null;
                last = null;
                destination = null;
                destinationToOrigin = null;
                current = null;
                elapsed = null;
                lastTime = null;

            } else {


                last = Microsoft.Maps.SpatialMath.interpolate(last, destination, 0.3);
                last.x = last.x + last.x * elapsed;
                last.y = last.y + last.y * elapsed;

                map.setView({ center: last });
                setTimeout(function (evtObject) {
                    update(evtObject);
                    lastTime = current;
                }, 33, evt);
            }
        }
        update(evt);


    }

    function showInfobox(evt) {

        map.setView({ center: evt.target.getLocation() });

        oEvent.fire("pushpin-click", { metadata: evt.target.metadata });

        var template = infoboxTemplate.replace("{pinTemplate}", getPushpinTemplate(evt.target, false));

        infobox.setOptions({
            htmlContent: template,
            visible: true,
            showCloseButton: false,
            offset: new Microsoft.Maps.Point(-116, 11),
            zIndex: 21
        });
        infobox.setLocation(evt.target.getLocation());

        infobox.target = evt.target;

        mouseenterEventHandler = Microsoft.Maps.Events.addHandler(infobox, 'mouseenter', function (evt) {
            var box = evt.target;
            oEvent.fire("pushpin-over", { metadata: box.target.metadata });
        });
        mouseleaveEventHandler = Microsoft.Maps.Events.addHandler(infobox, 'mouseleave', function (evt) {
            var box = evt.target;
            oEvent.fire("pushpin-out", { metadata: box.target.metadata });
        });

        mouseClickInfoboxEventHandler = Microsoft.Maps.Events.addHandler(map, 'click', outsideInfoboxClicked);

        // pushpin visité on le grise
        if (infobox.target.metadata.visited != true) {

            infobox.target.metadata.bgColor = "#706f6f";
            infobox.target.metadata.visited = true;

            var icon = svg.replace("{bgcolor}", infobox.target.metadata.bgColor)
                           .replace("{opacity}", infobox.target.metadata.hasRuleColor ? "1" : "0")
                           .replace("{color}", infobox.target.metadata.color)
                           .replace("{zoom}", NORMAL_PUSHPIN_ZOOM);
            infobox.target.setOptions({ icon: icon });
        }
    }

    function outsideInfoboxClicked(evt) {

        if (infobox != null) {

            if (infobox.target != null) {
                infobox.target.setOptions({ visible: true });
                oEvent.fire("pushpin-out", { metadata: infobox.target.metadata });
            }

            infobox.setOptions({ visible: false });

            if (mouseleaveEventHandler)
                Microsoft.Maps.Events.removeHandler(mouseClickInfoboxEventHandler);

            if (mouseenterEventHandler)
                Microsoft.Maps.Events.removeHandler(mouseenterEventHandler);

            if (mouseleaveEventHandler)
                Microsoft.Maps.Events.removeHandler(mouseleaveEventHandler);

            mouseleaveEventHandler = null;
            mouseenterEventHandler = null;
            mouseClickInfoboxEventHandler = null;


        }
    }

    function infoboxClicked(evt) {
        var src = evt.srcElement || evt.target;
        if (src && hasClass(src, "infoboxmark"))
            oEvent.fire("cbo-check", { fileId: getAttributeValue(src, "fid") });

    }

    function getPushpinTemplate(pin, isMulti) {

        var data = pin.metadata;

        // Titre sur l'image

        var template = infoboxPinTemplate.replace(/{title}/g, data.info.title.value);
        template = template.replace('{titleOnClick}', data.info.title.onclick);

        // Image et la taille de son container
        if (data.info.image.value.trim() == "") {
            // Vu avec RMA le 25/11/2016 à 16:01, ne rien afficher si pas d'image
            // var color = data.color.length > 0 ? data.color : data.bgColor;
            // template = template.replace('{img}', "<span class='" + data.info.icon.value + "' style='color:" + color + " !important;'></span>");

            template = template.replace('{img}', "")
                .replace("{height}", "style='height:40px;background-color:#f8f8f8;'")
                .replace("{cHeight}", isMulti ? "style='height:115px;'" : "style='height:115px;top: 81px;'");// si multi-infobox pas de top
        } else
            template = template.replace('{img}', "<img src='" + data.info.image.value + "'></img>")
                .replace("{height}", "")
                .replace("{cHeight}", isMulti ? "style='height:215px;'" : "style='height:215px;top:  -19px;'");;

        // Fiche marquée ou pas
        if (data.marked == true)
            template = template.replace("{marked}", "favoris icon-check-square");
        else
            template = template.replace("{marked}", "favoris icon-square-o");

        template = template.replace("{id}", data.fileId);

        // Sous titre
        if (data.info.subTitle.value.length > 0)
            template = template.replace('{description}', '<span class="map-item">' + data.info.subTitle.value + '</span> {description}');

        // Les données mappées au dessous de l'image  
        if (data.info.fields.length > 0) {
            var desc = "";
            for (var i = 0; i < data.info.fields.length; i++) {

                if (data.info.fields[i].value != "") {

                    if (desc.length > 0)
                        desc += " | ";
                    desc += data.info.fields[i].value;

                }
            }

            template = template.replace('{description}', "<p>" + desc + "</p>");
        }
        else
            template = template.replace('{description}', "");

        template = template.replace('{title}',
               data.info.fields.map(
               function (item) {
                   return item.value;
               }).join('\n'));

        return template;
    }

    function showClusterPushpinInfobox(pins) {

        var pinsInfoboxTemplate = "<div class='customInfoBoxTemplatesCtnr'>";
        for (var i = 0; i < pins.length; i++) {
            pinsInfoboxTemplate += getPushpinTemplate(pins[i], true);
        }
        pinsInfoboxTemplate += "</div>";
        var template = infoboxTemplate.replace("customInfoBoxCtnr", "customInfoBoxCtnrMulti")
            .replace("infobox-pointer", "infobox-pointer-multi")
            .replace("{pinTemplate}", pinsInfoboxTemplate);

        infobox.setOptions({
            htmlContent: template,
            visible: true,
            showCloseButton: false,
            offset: new Microsoft.Maps.Point(-116, 11),
            zIndex: 21
        });

        infobox.setLocation(pins[0].getLocation());

        infobox.target = pins[0];

        mouseClickInfoboxEventHandler = Microsoft.Maps.Events.addHandler(map, 'click', outsideInfoboxClicked);

    }

    function createCustomCluster(clusterPushpin) {
        //rayon minimum du cluster
        var minRadius = 12;

        // pour avoir plus d'opacité à l'interieur
        var outlineWidth = 4;

        var clusterSize = clusterPushpin.containedPushpins.length;

        // Calcule le nombre de chiffre
        // de 0 à 9 -> 1 chiffres
        // de 10 à 99 -> 2 chiffres
        // de 100 à 999 -> 3 chiffres
        // On utilise le log 10, Math.log est le logarithme népérien
        var nbNumbers = Math.log(clusterSize) / Math.log(10);

        // 5 pixels/chiffre + le minimum pour que ca soit visible 
        var radius = nbNumbers * 5 + minRadius;

        //dark-rouge > 1000.
        var fillColor = 'rgba(111, 0, 0, 0.7)';
        if (clusterSize < 10)
            //vert < 10.
            fillColor = 'rgba(20, 180, 20, 0.7)';
        else if (clusterSize < 100)
            //jaune entre 10 et 100.
            fillColor = 'rgba(255, 210, 40, 0.7)';
        else if (clusterSize < 1000)
            //rouge entre 100 et 1000.
            fillColor = 'rgba(255, 40, 40, 0.7)';

        //svg icon
        var svg = ['<svg xmlns="http://www.w3.org/2000/svg" width="', (radius * 2), '" height="', (radius * 2), '">',
            '<circle cx="', radius, '" cy="', radius, '" r="', radius, '" fill="', fillColor, '"/>',
            '<circle cx="', radius, '" cy="', radius, '" r="', radius - outlineWidth, '" fill="', fillColor, '"/>',
            '</svg>'];

        // mise a jour des options du clusterPushpin
        clusterPushpin.setOptions({
            icon: svg.join(''),
            anchor: new Microsoft.Maps.Point(radius, radius),
            textOffset: new Microsoft.Maps.Point(0, radius - 8), //font-size = 8 
            enableClickedStyle: true,
            enableHoverStyle: true
        });

        // quand on click sur un clusterPushpin, on zoom pour faire apparaitre que les pushpins le contenant
        Microsoft.Maps.Events.addHandler(clusterPushpin, 'click', clusterPushpinClickHandler);
    }

    function clusterPushpinClickHandler(evt) {
        zoomIntoContainedPushpins(evt.target.containedPushpins);
        // Si on est sur le zoom max
        if (map.getZoom() == 20) {
            showClusterPushpinInfobox(evt.target.containedPushpins);
        }
    }


    // On zoom selon le rectangle contenant tous les pins, en centrant sur le barycentre 
    function zoomIntoContainedPushpins(pins) {
        clusterPushpins = pins;
        var bounds = Microsoft.Maps.SpatialMath.Geometry.bounds(pins);
        var centroid = Microsoft.Maps.SpatialMath.Geometry.centroid(pins);
        map.setView({ center: centroid, bounds: bounds, padding: editMode ? 150 : 80 });
        clusterZoomStack.push({ bounds: bounds, center: centroid });
    }
    // Récupère un polygon avec le format sql server
    function getPolygonMktValue() {
        if (drawingToolsManager != null && Microsoft.Maps.WellKnownText != null) {

            // Si pas de dessin de selection on prend le rectangle du  viewport
            if (currentPolygone == null)
                currentPolygone = Microsoft.Maps.SpatialMath.locationRectToPolygon(map.getBounds());
            //drawingToolsManager.edit(polygon);

            var shape = Microsoft.Maps.SpatialMath.Geometry.makeValid(new Microsoft.Maps.Polygon(currentPolygone.getRings()));
            return Microsoft.Maps.WellKnownText.write(shape);;
        }

        return "";
    }

    function highlight(options) {
        if (pins != null) {
            if (infobox != null)
                tryHighlightPushpin(infobox.target, options);

            for (var i = 0; i < pins.length; i++)
                tryHighlightPushpin(pins[i], options);
        }
    }
    function tryHighlightPushpin(pushpin, options) {

        if (typeof (pushpin) == "undefined" || pushpin == null)
            return;

        if (pushpin.metadata.fileId == options.newId) {

            var icon = svg
                .replace("{bgcolor}", pushpin.metadata.bgColor)
                .replace("{opacity}", pushpin.metadata.hasRuleColor ? "1" : "0")
                .replace("{color}", pushpin.metadata.color)
                .replace("{zoom}", HIGHLIGHT_PUSHPIN_ZOOM);

            pushpin.setOptions({ icon: icon, anchor: new Microsoft.Maps.Point(8 * HIGHLIGHT_PUSHPIN_ZOOM, 30 * HIGHLIGHT_PUSHPIN_ZOOM) });

        } else {
            if (pushpin.metadata.fileId == options.lastId) {
                var icon = svg
                    .replace("{bgcolor}", pushpin.metadata.bgColor)
                    .replace("{opacity}", pushpin.metadata.hasRuleColor ? "1" : "0")
                    .replace("{color}", pushpin.metadata.color)
                    .replace("{zoom}", NORMAL_PUSHPIN_ZOOM);

                pushpin.setOptions({ icon: icon, anchor: new Microsoft.Maps.Point(8, 30) });
            }
        }
    }
    function markInfobox(options) {
        if (pins != null) {

            var pushpin;
            for (var i = 0; i < pins.length; i++) {
                pushpin = pins[i];
                if (pushpin.metadata.fileId == options.fileId)
                    pushpin.metadata.marked = !pushpin.metadata.marked;

            }

            var customInfoBoxCtnr = document.getElementById("InfoBoxCtnr");
            if (customInfoBoxCtnr) {
                var spanCheckbox = customInfoBoxCtnr.querySelector("span[fid='" + options.fileId + "']");
                if (spanCheckbox) {

                    if (spanCheckbox.className.indexOf("icon-square-o") >= 0)
                        switchClass(spanCheckbox, "icon-square-o", "icon-check-square");
                    else if (spanCheckbox.className.indexOf("icon-check-square") >= 0)
                        switchClass(spanCheckbox, "icon-check-square", "icon-square-o");

                }
            }
        }
    }

    function exportMap() {

        var center = map.getCenter();
        var lat = center.latitude;
        var lon = center.longitude;
        var centerPoint = lat + "," + lon;

        var zoom = Math.round(map.getZoom());

        var location, occurence, arrLocations = [];
        for (var i = 0; i < pins.length; i++) {
            arrLocations.push(pins[i].getLocation());
        }

        // Génération de l'URL
        var url = "https://dev.virtualearth.net/REST/v1/Imagery/Map/Road/" + centerPoint + "/" + zoom + "?mapSize=800,800&format=png" + "&key=Aia9V-TFKUb44CNZsVp_oxYGgszFUgksJal8-_IW1SSbodepQ4didGSMVp4UiSwR";

         for (var i = 0; i < arrLocations.length; i++) {
            location = arrLocations[i];
            //occurence = countOccurencesInArray(arrLocations, location);
            //if (occurence == 1)
            //    occurence = "";
            url = url + "&pp=" + location.latitude + "," +location.longitude + ";46;";
        }

        //var xhttp = new XMLHttpRequest();
        //xhttp.open("POST", url, true);
        //xhttp.setRequestHeader("Content-type", "text/plain; charset=utf-8");
        //xhttp.send();

        window.open(url, "_blank");
    }

    // Retourne le nombre d'occurences de la localisation trouvées dans le tableau
    function countOccurencesInArray(arr, obj) {
        var count = 0;
        for (var i = 0; i < arr.length; i++) {
            if (arr[i].latitude == obj.latitude && arr[i].longitude == obj.longitude)
                count++;
        }
        return count;
    }

    function reset() {

        if (polygonLayer != null)
            polygonLayer.clear();

        if (clusterLayer != null)
            clusterLayer.clear();

        zoom = 1;
        pinsData = [];
        currentPolygone = null;
        firstPoint = null;
        editMode = false;
        pins = null;
        clusterPushpins = null;

        clusterZoomStack = [];
    }
    return {
        init: function () {
        },
        reset: function () {
            reset();
        },
        createMap: function (options) {

            var container;
            if (options.containerId)
                container = document.getElementById(options.containerId);
            else
                container = document.body;

            // Création de la map avec les options suivante
            map = new Microsoft.Maps.Map(container, // options.containerId
            {
                credentials: document.getElementById("accessKey").value,
                height: options.height,
                width: options.width,
                mapTypeId: Microsoft.Maps.MapTypeId.road,
                navigationBarMode: (options.navigationBarMode == "default") ? Microsoft.Maps.NavigationBarMode.default : Microsoft.Maps.NavigationBarMode.minified,
                showMapTypeSelector: true,
                showMapLabel: false,
                showBreadcrumb: (options.showCurrentLocation) ? true : false,
                showLocateMeButton: (options.showCurrentLocation) ? true : false,
                showTrafficButton: false,
                disableStreetsideAutoCoverage: false,
                enableCORS: true

            });

            // options.wktModuleCallback pour ajouter un callback au chargement du module WKT

            pinsData = []; //options.pinsData;

            attachEvents();
            loadModules(options);

            polygonLayer = new Microsoft.Maps.Layer();
            map.layers.insert(polygonLayer);


            // Création de l'infobox (cachée)
            infobox = new Microsoft.Maps.Infobox(map.getCenter(), {
                title: 'Map Center',
                description: 'This is the center of the map.',
                visible: false
            });
            infobox.setMap(map);

        },
        createCustomMap: function (mapId, mapOptions) {
            map = new Microsoft.Maps.Map("#" + mapId, mapOptions);
            attachEvents();
        },
        showPushpins: function () {
            if (map != null)
                displayPushPins();
        },
        highlightPushpin: function (options) {
            if (map != null)
                highlight(options);
        },
        markInfobox: function (options) {
            if (map != null)
                markInfobox(options);
        },
        draw: function () {
            if (map != null)
                startDraw();
        },
        getWktPolygon: function () {
            return getPolygonMktValue();
        },
        refreshPushPin: function (data) {
            if (map != null) {
                reset();
                pinsData = data;
                displayPushPins();
            }
        },
        createGeolocPushpin: function(txtWktId, txtLatId, txtLonId) {
            return createGeolocPushpin(txtWktId, txtLatId, txtLonId);
        },
        getWkt: function (lat, lon) {
            if (!lat || !lon)
                return "";
            return "POINT (" + lon + " " + lat + ")";
        },
        setPushpinLocation: function(pushpin, lat, lon) {
            if (!pushpin)
                return;
            var location = new Microsoft.Maps.Location(lat, lon);
            pushpin.setLocation(location);
            map.setView({
                center: location
            });
        },
        erase: function () {
            if (map != null)
                clearDrawing();
        },
        finishDrawing: function () {
            finishPolygon();
        },
        center: function () {
            if (editMode == false && clusterLayer != null && clusterPushpins != null)
                zoomIntoContainedPushpins(clusterPushpins);
        },
        infoBoxClick: function (evt) {
            infoboxClicked(evt);
        },
        markAllFiles: function (options) {

            if (pins != null) {
                var pushpin;
                for (var i = 0; i < pins.length; i++) {
                    pushpin = pins[i];
                    pushpin.metadata.marked = options.marked;
                }

                if (infobox && infobox.target) {

                    var elem = document.querySelector("infoboxmark");
                    if (infobox.target.metadata.marked == true) {
                        switchClass(elem, "icon-square-o", "icon-check-square");

                    } else {
                        if (infobox.target.metadata.marked == false) {
                            switchClass(elem, "icon-check-square", "icon-square-o");
                        }
                    }
                }
            }
        },
        search: function (address) {
            // Recherche et place un pushpin sur la localisation de l'adresse
            Microsoft.Maps.loadModule('Microsoft.Maps.Search', function () {
                var searchManager = new Microsoft.Maps.Search.SearchManager(map);
                var requestOptions = {
                    bounds: map.getBounds(),
                    where: address,
                    callback: function (answer, userData) {
                        map.setView({ bounds: answer.results[0].bestView });
                        map.entities.push(new Microsoft.Maps.Pushpin(answer.results[0].location));
                    }
                };
                searchManager.geocode(requestOptions);
            });
        },
        exportMap: function() {
            exportMap();
        },
        dispose: function () {
            if (map == null)
                return;

            Microsoft.Maps.Events.removeHandler(viewchangeend);
            map.dispose();

            infobox = null;
            config = null;
            map = null;
            viewchangeend = null;
        }
    }
})();

