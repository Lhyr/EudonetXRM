<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eGeolocDialog.aspx.cs" Inherits="Com.Eudonet.Xrm.eGeolocDialog" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>   
    <title></title>
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder> 
    <script type="text/javascript" src="https://www.bing.com/api/maps/mapcontrol?callback=getMap" async defer></script>
    <script language="javascript" type="text/javascript">

        var pushpin;

        function getMap() {

            oMaps.createMap(
                {
                    containerId: "geolocMap",
                    zoom: 5,
                    animation: true,
                    showCurrentLocation: true,
                    navigationBarMode: "default",
                    wktModuleCallback: function () {
                        pushpin = oMaps.createGeolocPushpin("wkt", "txtLat", "txtLon");
                    }
                });
            
        }

        function coordOnChange(element) {
            var wkt = document.getElementById("wkt");
            wkt.value = oMaps.getWkt(document.getElementById("txtLat").value, document.getElementById("txtLon").value);
            oMaps.setPushpinLocation(pushpin, document.getElementById("txtLat").value, document.getElementById("txtLon").value);
        }

    </script>
</head>
<body>

    <form id="formGeoloc" runat="server">
        <input type="hidden" id="accessKey" runat="server" />
        <input type="hidden" id="wkt" runat="server" />
     

        <div id="leftPart">
            <p id="intro" runat="server"></p>
            <div class="field">
                <label>Latitude</label>
                <input type="text" id="txtLat" runat="server" onchange="coordOnChange()" />  
            </div>
            <div class="field">
                <label>Longitude</label>
                <input type="text" id="txtLon" runat="server" onchange="coordOnChange()" />  
            </div>

        </div>

        <div id="geolocMap"></div>
    </form>  
   

</body>
</html>
