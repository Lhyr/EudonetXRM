<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eStatistics.aspx.cs" Inherits="Com.Eudonet.Xrm.eStatistics"
    EnableViewState="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>

    <%--<title><%= FieldName%></title>--%>

    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>


</head>
<body onload="loadSyncFusionChart(null,true);">
    <form action="/" method="post" runat="server">
        <div id="divContainer">

            <div id="DivGlobal" class="Global" runat="server" style="margin: 0 auto; position: relative;">
                <%--<canvas id="DivGlobal_canvas" height="1200" width="1200" style="width: 1200px; height: 1200px; position: absolute; left: 0; top: 0; z-index: 0;"></canvas>--%>
                <div runat="server" id="StatsRendDtl" class="data">
                </div>

            </div>


        </div>
    </form>
</body>
</html>
