<%@ Import Namespace="Com.Eudonet.Xrm" %>
<%@ Import Namespace="Com.Eudonet.Internal" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eParamIFrame.aspx.cs" Inherits="Com.Eudonet.Xrm.eParamIFrame"
    EnableSessionState="readonly" EnableViewState="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>
    <title></title>



    <script language="javascript" type="text/javascript">
        var nMaxRows = <%=eLibConst.MAX_ROWS%>;
        var nNbrMaxAdvancedUsr = <%=eConst.NBR_MAX_ADVANCED_USR%>;  // En mode liste, nombre de filtre users avancé sauvegardés
        var oCalendarBuffer = new Array();     //Presse Papier Mode semaine
        var oMoveCalendarBuffer = new Array(); //Presse Papier Mode semaine pour déplacement/duplication vers une date donnée
        var oMonthCalendarBuffer = null;    //Presse papier Mode Mois MultiUser
        var oMonthCalendarBufferOne = null;     //Presse papier Mode Mois OneUser
        var oMonthCalendarCut = false;
    </script>
</head>
<body>
    <!-- Param globaux -->
    <div id="GLOBAL" runat="server" />
    <!-- Param grilles -->
    <div id="GRIDS" runat="server" />
    <!-- Mru Table -->
    <div id="TABS" runat="server" />
    <!-- Mru Field -->
    <div id="FIELDS" runat="server" />
    <!-- Email Files -->
    <div id="MLFiles" runat="server" />
    <!-- SMS Files -->
    <div id="SMSFiles" runat="server" />
    <!-- Quick user list -->
    <div id="QckUsrLst" runat="server" />
    <!-- Nouvelle Ergonomie Iris Black -->
    <div id="dvIrisBlack" runat="server" />
    <!-- Nouvelle Ergonomie sur les listes Iris Crimson -->
    <div id="dvIrisCrimson" runat="server" />
    <!-- Nouvelle Ergonomie sur le mode téléguidé -->
    <div id="dvIrisPurple" runat="server" />

    <!--<textarea id="DebugTextarea" name="DebugTextarea" cols="0" rows="0" style="visibility: hidden;
        display: none;"></textarea>-->

    <script type="text/javascript" language="javascript">
        top.bIsIFrameLoaded = "1";
        //Gestion du chargement asymétrique entre la navbar et la frame de param
        if (top.bIsNavBarLoaded == "1" && top.bIsParamLoaded != "1")
            OnLoadParam();
            
        /*GCH : commenté suite à demande RMA/CMO 31012013*/
        /*MOU : décommenté suite à demande RMA/CMO  cf. 23835 */
        window.parent.setWait(false);
    </script>
</body>
</html>
