<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eCartoMulti.aspx.cs" Inherits="Com.Eudonet.Xrm.eCartoMulti" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>Carte EudoBingMaps</title>
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>
    <script type="text/javascript" src="https://www.bing.com/api/maps/mapcontrol?callback=GetMap&branch=experimental" async defer></script>
    <script type="text/javascript">

        var map = null;
        var pinInfoBox;  //the pop up info box
        var infoboxLayer;
        var pinLayer;
        var arrayPins = new Array();

        // Chargement des marqueurs sur la carte
        function LoadAllPushPin() {

            var titre = '';
            var latitude = '';
            var longitude = '';
            var arrayLocations = new Array();

            var pinInfobox;

            <% if (dico.Count > 0)
               {

                   for (int i = 0; i < dico.Count; i++)
                   {%>
            titre = "<%= Server.HtmlDecode(dico[i].NameOfLbl)%>";
            latitude = '<%= dico[i].Latitude %>';
            longitude = ' <%= dico[i].Longitude %>';

            var pinlocation = new Microsoft.Maps.Location(latitude, longitude);
            arrayLocations.push(pinlocation);

            var pin = new Microsoft.Maps.Pushpin(pinlocation,
                                                {
                                                    anchor: new Microsoft.Maps.Point(0, 50)
                                                });

            pin.Title = titre;
            pin.Description = "<%= Server.HtmlDecode(dico[i].AdresseHTML)%>";
            Microsoft.Maps.Events.addHandler(pin, 'click', displayInfobox);
            arrayPins.push(pin);
            //map.entities.push(pin);

            
            

          <%  }
            }
               else
               { %>
            alert('Aucune adresse géolocalisée n\'est présente dans votre selection');
            <% } %>

            

        }

        // Centre la carte par rapport aux pushpins
        function centerMap(pins) {
            var bounds = Microsoft.Maps.SpatialMath.Geometry.bounds(pins);
            var centroid = Microsoft.Maps.SpatialMath.Geometry.centroid(pins);
            map.setView({ center: centroid, bounds: bounds });
        }


        // Fonction d'initialisation de la carte
        function GetMap() {

            // Recuperation latitude et longitude
            var p_lat = '';
            var p_long = '';
            var p_titre = '';

            //Key
            var bingMapKey = document.getElementById('h_key').value;

            // Define the pushpin location
            var location = new Microsoft.Maps.Location('46.641998291015625', '2.3380000591278076');

            // Set the map and view options, setting the map style to Road and
            var mapOptions = {
                credentials: bingMapKey,
                mapTypeId: Microsoft.Maps.MapTypeId.road,
                center: location,
                liteMode: true
            };

            map = new Microsoft.Maps.Map('#mapDiv', mapOptions);

            Microsoft.Maps.Events.addHandler(map, 'viewchange', hideInfobox);

            // Create the info box for the pushpin
            pinInfobox = new Microsoft.Maps.Infobox(map.getCenter(), {
                title: 'Map Center',
                description: 'This is the center of the map.',
                visible: false
            });
            pinInfobox.setMap(map);


            LoadAllPushPin();

            loadModules();

        }

        function loadModules() {
            Microsoft.Maps.loadModule('Microsoft.Maps.SpatialMath', function () {
                // On centre la carte par rapport à la collection de points
                if (arrayPins.length > 0) {
                    centerMap(arrayPins);
                }
            });

            Microsoft.Maps.loadModule('Microsoft.Maps.Clustering', function () {
                // On centre la carte par rapport à la collection de points
                var clusterLayer = new Microsoft.Maps.ClusterLayer(arrayPins, {
                    gridSize: 100,
                    clusteredPinCallback: customClusteredPin
                });
                map.layers.insert(clusterLayer);
            });
        }

        function customClusteredPin(cluster) {
            Microsoft.Maps.Events.addHandler(cluster, 'click', clusterClicked);
        }

        function clusterClicked(e) {
            if (e.target.containedPushpins) {
                var locs = [];
                for (var i = 0, len = e.target.containedPushpins.length; i < len; i++) {
                    //Get the location of each pushpin.
                    Microsoft.Maps.Events.addHandler(e.target.containedPushpins[i], 'click', displayInfobox);
                    locs.push(e.target.containedPushpins[i].getLocation());
                }

                //Create a bounding box for the pushpins.
                var bounds = Microsoft.Maps.LocationRect.fromLocations(locs);

                //Zoom into the bounding box of the cluster. 
                //Add a padding to compensate for the pixel area of the pushpins.
                map.setView({ bounds: bounds, padding: 100 });
            }
            
        }

        // Rend visile l'infoculle
        function displayInfobox(e) {
            pinInfobox.setOptions({
                title: e.target.Title,
                description: e.target.Description,
                visible: true,
                offset: new Microsoft.Maps.Point(10, 15)
            });
            pinInfobox.setLocation(e.target.getLocation());
        }

        // Masque l'infobulle
        function hideInfobox(e) {
            pinInfobox.setOptions({ visible: false });
        }


    </script>

</head>
<body>
    <%-- <div id="divWait" runat="server">
   <img src="img/wait.gif" alt="Veuillez Patientez..." /></div>--%>
    <div class="divgModBig">
        <div class="header">
            <img src="themes/default/images/BingMaps/bing-map-logo.png" alt="EudoBingMaps" />
        </div>
        <div id="mapDiv" class="mapDiv">
        </div>
        <div id='itineraryDiv' style="position: relative; width: 400px;">
        </div>
    </div>
    <div id="divHidden" runat="server">
    </div>
    <input id="h_key" type="hidden" runat="server" />
    <script type="text/javascript">
        //go();
    </script>
</body>
</html>
