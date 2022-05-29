<%@ Import Namespace="Com.Eudonet.Xrm" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eFileDisplayer.aspx.cs"
    Inherits="Com.Eudonet.Xrm.eFileDisplayer"
    EnableSessionState="True"
    EnableViewState="false"
    EnableViewStateMac="false"
    EnableEventValidation="false"
    ViewStateMode="Disabled" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title></title>
    <meta http-equiv="content-type" content="application/xhtml+xml; charset=UTF-8" />
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>

    <style type="text/css" id="customCss" title="customCss"></style>



    <script type="text/javascript">
        // Chargement des scripts pour tablettes
        if (typeof (setTabletScripts) == 'function') {
            setTabletScripts();
        }

        // EVENEMENT LORS DU REDIMENSIONNEMENT DE LA MODALDIALOG
        function onFrameSizeChange(nNewWidth, nNewHeight) {
            var container = document.getElementById("container");
            if (container)
                container.setAttribute("style", "height:" + (nNewHeight) + "px");

            var mainDiv = document.getElementById("mainDiv");
            if (mainDiv)
                mainDiv.setAttribute("style", "height:" + (nNewHeight - 2) + "px");

            if (document.getElementById("md_pl-base")) {
                document.getElementById("md_pl-base").style.height = (nNewHeight - 10) + "px";
            }

            if (typeof (setHoursPopupDisplay) == "function")
                setHoursPopupDisplay();
        }
    </script>

</head>
<body onload="<%=onLoad%>" class="bodyWithScroll">
    <div runat="server" id="container" class="GlbPopDiv">

        <div runat="server" id="mainDiv" class="mainDivPopup" popup="1">
        </div>
    </div>

    <% if (CanRunBingAutoSuggest())
    { %>
        <script type='text/javascript' src='https://www.bing.com/api/maps/mapcontrol' async defer></script>
    <% } %>
</body>
</html>
