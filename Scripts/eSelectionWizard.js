var CONST_MAX_PUSHPINS = 500;
var OP_CONTAINS = 9;     
var OP_IS_EMPTY = 10;       
var OP_EQUAL = 0;   
var OP_DISTANCE = 19;
var OP_IN_POLYGON = 20;

var nsSelectionWizard = {};     // NameSpace pour les fonctions propre au wizard
var map, lastCenterLocation, lastZoom, infobox, infoboxLayer, pinsLayer;

//Action possible pour le manager
nsSelectionWizard.action =
{
    ACTION_SELECT: 1,          // Sélection d'1 fiche
    ACTION_SELECTALL: 2,       // Sélection de toutes les fiches
    ACTION_UNSELECTINVIT: 3,        // déselection d'1 fiche
    ACTION_UNSELECTALLINVIT: 4     // déselection de toutes les fiches
};


function eSelectionWizard(nTab, nTabSource) {

    this.Tab = nTab;               
    this.TabSource = nTabSource;

    //this.NbInvit = 0;   // Nombre d'invitations cochées
    //this.NbPP = 0;      // Nombre de pp cochées
    //this.NbAdr = 0;     // Nombre d'adresses cochées

    this.ControlStep = function (nStep) {
        if (nStep == 1) {
        }
        else {

            
        }
        return true;
    }


    ///Type de rapport
    /// Utilisé dans eWizard.js via oCurrentWizard (cf la méthode init dans eWizard.js
    this.GetType = function () {
        return "-1"; //Le type d'un wizard fait référence à un type de report. Dans le cas d'un wizard ++, cela n'a pas de sens, on passe donc -1 (NONE dans l'énum)
    }

    //Surcharge du selectline
    //if (typeof selectLine == "function") {

    //    var origFct = selectLine;  // fonction selecLine d'origine

    //    selectLine = function (obj) {
    //        origFct(obj);
    //        nsInvitWizard.selectLine(obj); // applique le selecline spécifique
    //    }
    //}

}


nsSelectionWizard.SwitchStep = function (step) {
    switch (parseInt(step)) {
        case 1:
            updateSelectionList(1);
            break;
        case 2:

            break;
    }
}

// Effectue une recherche à partir des filtres du haut
nsSelectionWizard.doFilterSearch = function () {

    var filters = document.getElementById("hidFilters");

    var filtersValue = nsSelectionWizard.getCriteriaFiltersValue();

    filters.value = filtersValue;

    updateSelectionList(1, filtersValue);
}

nsSelectionWizard.getCriteriaFiltersValue = function () {

    var element, filterExpression, tagname;
    var filtersValue = "";

    var blockCrit = document.getElementById("blockCriteria");
    var criteria = blockCrit.querySelectorAll("[critDid]");



    for (var i = 0; i < criteria.length; i++) {

        element = criteria[i];
        tagname = element.tagName.toLowerCase();

        if (tagname == "select" || (tagname == "input" && element.className != "numField")) {

            var value = element.value;

            if ((value != "-1" && tagname == "select") || (value != "" && tagname == "input")) {
                var descid = getAttributeValue(element, "critDid");
                var op = OP_CONTAINS; // Contient
                if (tagname == "select") {
                    if (value == "") {
                        op = OP_IS_EMPTY; // OP_IS_EMPTY
                    }
                    else if (getAttributeValue(element, "data-multiple") == "0")
                        op = OP_EQUAL; // OP_EQUAL
                }
                else {
                    if (getAttributeValue(element, "data-geodistance") == "1") {
                        op = OP_DISTANCE; // OP_DISTANCE
                        value = "POINT(2.261318 48.90135929)$D!ST$" + value; // TODO: point à définir
                    }
                }

                filterExpression = descid + ";|;" + op + ";|;" + value;
                filtersValue = filtersValue + "&&" + filterExpression;
            }
        }

    }

    return filtersValue;
}


/// Ouvre le menu contextuel de la case à cocher de sélection des fiches
nsSelectionWizard.getContextMenu = function (chkBox) {
    var obj_pos = getAbsolutePosition(chkBox);
    eCMSelectionWizard = new eContextMenu(null, obj_pos.y - 10, obj_pos.x, "eCMSelectionWizard");

    //Tous
    eCMSelectionWizard.addItemFct(top._res_22, function () { nsSelectionWizard.selectAllInvit(nsSelectionWizard.action.ACTION_SELECTALL); eCMSelectionWizard.hide(); }, 0, 1, "actionItem", top._res_22);

    //Aucun
    eCMSelectionWizard.addItemFct(top._res_436, function () { nsSelectionWizard.selectAllInvit(nsSelectionWizard.action.ACTION_UNSELECTALL); eCMSelectionWizard.hide(); }, 0, 1, "actionItem", top._res_436);

}

nsSelectionWizard.selectAllInvit = function (action) {
    
    var checks = document.getElementsByClassName("chkAction");
    var chkValue;
    var bCheck;

    //if (action == nsSelectionWizard.action.ACTION_SELECTALL) {
    //    bCheck = true;
    //}
    //else if (action == nsSelectionWizard.action.ACTION_UNSELECTALL) {
    //    bCheck = false;
    //}

    //for (var i = 0; i < checks.length; i++) {
    //    if (bCheck) {
    //        setAttributeValue(checks[i], "chk", "1");
    //    }
    //    else {
    //        setAttributeValue(checks[i], "chk", "0");
    //    }
    //}

    //var oInvitWizardUpdater = new eUpdater("mgr/eSelectionWizardManager.ashx", 0);

    //oInvitWizardUpdater.ErrorCallBack = function () { setWait(false); };
    //oInvitWizardUpdater.addParam("tab", oSelectionWizard.Tab, "post");
    //oInvitWizardUpdater.addParam("tabsource", oSelectionWizard.TabSource, "post");
    //oInvitWizardUpdater.addParam("action", "2", "post");
    //setWait(true);
    //oInvitWizardUpdater.send();
    var filters = document.getElementById("hidFilters");
    updateSelectionList(1, filters.value, action);
}

// Initialisation/chargement de la carte
nsSelectionWizard.initMap = function (divId, key, mapView, width, height, arrCoord) {

    mapDiv = document.getElementById(divId);

    if (mapDiv) {
        //mapDiv.innerHTML = "";
        mapDiv.style.width = width + 'px';
        mapDiv.style.height = height + 'px';
        mapDiv.style.position = "relative";
        

        var arrMapView = mapView.split(',');
        var latitude = arrMapView[0];
        var longitude = arrMapView[1];
        var zoom = Number(arrMapView[2]);

        lastCenterLocation = new Microsoft.Maps.Location(latitude, longitude);
        lastZoomlevel = zoom;

        // Sauvegarde de la vue actuelle de la map
        nsSelectionWizard.updateMapView(lastCenterLocation, lastZoomlevel);

        if (!map) {
            // Création de la map avec ses options
            var mapOptions = {
                credentials: key,
                center: lastCenterLocation,
                mapTypeId: Microsoft.Maps.MapTypeId.Auto,
                showDashboard: true,
                showScalebar: true,
                enableSearchLogo: false,
                enableClickableLogo: false,
                zoom: zoom,
                width: width,
                height: height
            }
            map = new Microsoft.Maps.Map(mapDiv, mapOptions);

            // Infobox Layer
            infoboxLayer = new Microsoft.Maps.EntityCollection({ zIndex: 200 });
            map.entities.push(infoboxLayer);

            // Infobox
            var infoboxOptions = {
                showPointer: true,
                showCloseButton: true,
                offset: new Microsoft.Maps.Point(0, 20),
                visible: false
            }
            infobox = new Microsoft.Maps.Infobox(new Microsoft.Maps.Location(0, 0), infoboxOptions);
            infoboxLayer.push(infobox);

            // Pins Layer
            pinsLayer = new Microsoft.Maps.EntityCollection();
            map.entities.push(pinsLayer);
        }
        

        nsSelectionWizard.displayPins(map, arrCoord);
    }
    
    // Chargement du module de sélection
    Microsoft.Maps.registerModule("WKTModule", "scripts/bingmaps/modules/WKTModule.min.js");
    Microsoft.Maps.loadModule("WKTModule");

    Microsoft.Maps.registerModule("DrawingToolsModule", "scripts/bingmaps/modules/DrawingToolsModule.js");
    Microsoft.Maps.loadModule("DrawingToolsModule", { callback: nsSelectionWizard.drawingToolsModuleLoaded });
}

nsSelectionWizard.updateMapView = function (location, zoom) {
    var hidMapView = document.getElementById("hidMapView");
    if (hidMapView) {
        hidMapView.value = location.latitude + "," + location.longitude + "," + zoom;
    }
}

// Après chargement du module de sélection polygonale...
nsSelectionWizard.drawingToolsModuleLoaded = function() {
    drawingTools = new DrawingTools.DrawingManager(map, {
        
        shapeOptions: {
            strokeThickness: 1,
            strokeColor: new Microsoft.Maps.Color(90, 90, 90, 50),
            fillColor: new Microsoft.Maps.Color(90, 90, 90, 50)
            
        },
        events: {
            drawingEnded: function (s) {
                //showMeasurements(s);
                var wkt = WKTModule.Write(s);
                nsSelectionWizard.doMapSearch(wkt);
                drawingTools.clear();
                
                //DrawingTools.MapMath.haversineDistance: function (loc1, loc2, distanceUnits) {
            },
            drawingStarted: function (s) {
                
            },
            drawingChanging: function (s) {
                nsSelectionWizard.showMeasurements(s);
            }
        //    drawingErased: function (s) {
        //        infoLayer.clear();
        //    }
        }

    });
}

nsSelectionWizard.showMeasurements = function (shape) {
    pinsLayer.clear();

    if (shape.ShapeInfo && shape.ShapeInfo.type != DrawingTools.DrawingMode.pushpin) {
        var pin, loc;

        var unit = "m";
        var distanceValue;

        if (shape.ShapeInfo.type == DrawingTools.DrawingMode.circle) {

            //For circles show the radius distance at the center of the circle.
            distanceValue = Math.round(shape.ShapeInfo.radius);
            if (distanceValue > 1000) {
                distanceValue = DrawingTools.MapMath.convertDistance(distanceValue, DrawingTools.DistanceUnit.meters, DrawingTools.DistanceUnit.km);
                unit = "km";
            }
            pin = new Microsoft.Maps.Pushpin(shape.ShapeInfo.center, {
                htmlContent: '<span class="labelDistance">R : ' + Math.round(distanceValue * 10) / 10 + ' ' + unit + '</span>',
                width: 65,
                height: 20
            });
            pinsLayer.push(pin);
        } else {
            var locs = shape.getLocations();
            var segmentLenth, totalLength = 0;

            //Show the length of each line segment of a shape.
            for (var i = 0; i < locs.length - 1; i++) {
                segmentLenth = DrawingTools.MapMath.haversineDistance(locs[i], locs[i + 1], DrawingTools.DistanceUnit.meters);
                //totalLength += segmentLenth;

                if (segmentLenth > 1000) {
                    segmentLenth = DrawingTools.MapMath.convertDistance(segmentLenth, DrawingTools.DistanceUnit.meters, DrawingTools.DistanceUnit.km);
                    unit = "km";
                }

                loc = drawingTools.calculateVisualMidpoint(locs[i], locs[i + 1]);

                pin = new Microsoft.Maps.Pushpin(loc, {
                    htmlContent: '<span class="labelDistance">' + Math.round(segmentLenth * 10) / 10 + ' '+unit+'</span>',
                    width: 65,
                    height: 20
                });
                pinsLayer.push(pin);
            }

            //Show the total perimeter distance.
            //pin = new Microsoft.Maps.Pushpin(locs[locs.length - 1], {
            //    htmlContent: '<span class="labelDistance">' + Math.round(totalLength) + ' km</span>',
            //    width: 65,
            //    height: 20
            //});
            //pinsLayer.push(pin);
        }
    }
}

// Recherche cartographique (sélections polygonales)
nsSelectionWizard.doMapSearch = function (polygonValue) {
    var geoField = document.getElementById("hidGeoField");
    var descid = geoField.value;
    var filterExpression = descid + ";|;" + OP_IN_POLYGON + ";|;" + polygonValue;

    var filters = nsSelectionWizard.getCriteriaFiltersValue();
    var hidFilters = document.getElementById("hidFilters");
    hidFilters.value = filters + "&&" + filterExpression;

    updateSelectionList(1, hidFilters.value);
}

// Affichage des Pushpins sur la carte, à partir des coordonnées
nsSelectionWizard.displayPins = function (map, arrCoord) {
    // Affichage des pins s'il y en a moins de CONST_MAX_PUSHPINS
    var arrLocations = [];

    var tabSource = document.getElementById("hidTabSource").value;

    pinsLayer.clear();

    var arrLoc, lat, lon, pin, location;
    if (arrCoord && arrCoord.length <= CONST_MAX_PUSHPINS) {
        for (var i = 0; i < arrCoord.length; i++) {
            if (arrCoord[i] != "") {
                arrLoc = arrCoord[i].split('$,$');
                if (arrLoc.length == 5) {
                    location = new Microsoft.Maps.Location(arrLoc[0], arrLoc[1]);
                    arrLocations.push(location);
                    pin = new Microsoft.Maps.Pushpin(location, { zIndex: 20 });
                    pin.id = tabSource + "_" + arrLoc[2];
                    pin.Title = arrLoc[3];
                    pin.Description = arrLoc[4];
                    Microsoft.Maps.Events.addHandler(pin, 'click', nsSelectionWizard.displayInfobox);

                    pinsLayer.push(pin);
                }
            }
        }
    }
    else {
        var message = document.getElementById("mapMessage");
        message.style.display = "block";
    }

    // Recentrage de la carte par rapport aux Pushpins
    if (arrLocations.length > 0) {
        var bestView = Microsoft.Maps.LocationRect.fromLocations(arrLocations);

        setTimeout((function () {
            map.setView({ bounds: bestView });
        }).bind(this), 1000);

        lastCenterLocation = map.getCenter();
        lastZoom = map.getZoom();

        // Sauvegarde de la vue actuelle de la map
        nsSelectionWizard.updateMapView(lastCenterLocation, lastZoomlevel);
    }
}

// Affichage de l'infobulle d'un Pushpin
nsSelectionWizard.displayInfobox = function (e) {
    if (e.targetType == 'pushpin') {

        var location = e.target.getLocation();

        infobox.setLocation(location);
        infobox.setOptions({
            visible: true,
            title: e.target.Title,
            description: e.target.Description,
            height: 125
        });

        map.setView({ center: location });

        lastCenterLocation = location;
        lastZoom = map.getZoom();

        // Sauvegarde de la vue actuelle de la map
        nsSelectionWizard.updateMapView(lastCenterLocation, lastZoomlevel);

        //var line = document.querySelector("tr[eid='" + e.target.id + "']");
        //if (line != null) {
        //    line.style.backgroundColor = "green";
        //}
    }

    
}