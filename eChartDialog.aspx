<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eChartDialog.aspx.cs" Inherits="Com.Eudonet.Xrm.eChartDialog" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" style="height: 90%;">
<head>
    <title></title>
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>


</head>
<body id="BodyChart" style="height: 100%;" runat="server">

    <div id="DivGlobal" class="Global" runat="server">
        <div runat="server" id="charttitle" class="title_chart"></div>
        <div id="DivChartBord" style="height: 100%;">
            <div class="exportChartMenu  " style="height: 100%;">
                <div id="filterExpress" class="filterExpress" runat="server"></div>
                <div id="exPortChart" fhome="0" load="DivChart_canvas" class="icon-ellipsis-v advExportChartMenu  " onmouseover="top.dispExportMenu(this);" style="display: inline-block" runat="server"></div>
                <div id="btnShowFilter" class="icon-info-circle" runat="server"></div>
                
                <div id="DivHidden" style="visibility: hidden; display: none;" runat="server"></div>
                <div id="DivChart" class="SyncFusionChartContainer" runat="server">
                </div>

                <div id="DivChartGrid" class="Chart" runat="server">
                </div>
            </div>
        </div>
    </div>
</body>
</html>
