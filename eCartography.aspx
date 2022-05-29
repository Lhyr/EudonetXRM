<%@ Import Namespace="Com.Eudonet.Xrm" %>
<%@ Import Namespace="Com.Eudonet.Internal" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eCartography.aspx.cs" Inherits="Com.Eudonet.Xrm.eCartography"
    EnableSessionState="true" EnableViewState="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title></title>
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>
    <script type='text/javascript' src='https://www.bing.com/api/maps/mapcontrol?key=<%= eLibConst.BING_MAPS_KEY %>&branch=release' async defer></script>

   <script type="text/javascript">

       // Fonction appelée par le parent à chaque changement du thème,voir eTools.changeDocumentTheme
       function themeChanged(oldLink, newLink, newColorTheme)
       {
           var backColor = document.getElementById("backColor");
           if (backColor)
               backColor.value = newColorTheme;

           if (BingAPI) 
               BingAPI.ThemeChanged(newColorTheme);
       }

   </script>
</head>
<body>
    <input type="hidden" id="backColor" value="<%=_pref.ThemeXRM.Color %>" />
    <div id="CartoSelection"></div>
    <div id="DrawingModes">
        <span item-type="polygon" title="<%=GetRes(7490) %>" class="icon-polygon background-theme"></span>
        <span item-type="refresh" title="<%=GetRes(2060) %>" class="icon-refresh background-theme"></span>
        <span item-type="delete" title="<%=GetRes(2177) %>" class="icon-delete background-theme"></span>
        <span item-type="zoom-out" title="<%=GetRes(2176) %>" class="icon-zoom-out background-theme"></span>
        <span item-type="zoom-in" title="<%=GetRes(2175) %>" class="icon-zoom-in background-theme"></span>
        <span item-type="center" title="<%=GetRes(2174) %>" class="icon-dot-circle-o background-theme"></span>
        <span item-type="bullseye" title="<%=GetRes(2173) %>" class="icon-target background-theme"></span>
        <div class="background-theme">
            <input placeholder="Rechercher" title="<%=GetRes(2172) %>" type="text" id="searchTxt"></input>
            <span item-type="search" id="searchLoupe" class="icon-search"></span>
        </div>
    </div>   
</body>
</html>
