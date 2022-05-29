<%@ Import Namespace="Com.Eudonet.Xrm" %>
<%@ Import Namespace="Com.Eudonet.Internal" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eMapsIFrame.aspx.cs" Inherits="Com.Eudonet.Xrm.eMapsIFrame"
    EnableSessionState="true" EnableViewState="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>   
    <title></title>
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder> 
    <script type="text/javascript" src="https://www.bing.com/api/maps/mapcontrol?branch=release"></script>
    <script language="javascript" type="text/javascript">

        function mapsLoaded(evt)
        {
            oEvent.fire("maps-iframe-loaded", { oMapsInterface : oMaps, oDoc : document });
        }

    </script>
</head>
<body onload="mapsLoaded(event);">  
     <input type="hidden" id="accessKey" runat="server" />     
</body>
</html>
