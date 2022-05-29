<%@ Import Namespace="Com.Eudonet.Xrm" %>
<%@ Import Namespace="Com.Eudonet.Internal" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eFilterReportListDialog.aspx.cs"
    Inherits=" Com.Eudonet.Xrm.eFilterReportListDialog" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title></title>
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>

    <style type="text/css" id="customCss" title="customCss"></style>


</head>
<body onload="nGlobalActiveTab = <%=  _nTab %>;initHeadEvents();<%=_BodyLoadJavascript %>">
    <div class="waiter" id="ImgLoading" style="display: none;">
        <div class="contentWait" id="contentWait">
            <img alt="wait" src="themes/default/images/wait.gif" />
        </div>
    </div>
    <div id="waiter" class="waitOff">
    </div>

    <div class="window_iframe" id="mainDiv" runat="server">
        <%=_strReportSwitch %>
        <div id="catDivHeadAdv" class="catDivHeadAdv" runat="server">
            <ul id="catToolAdd" class="catToolAdd" runat="server">
                <li class="catToolAddLib" runat="server" id="btnAdd"></li>
            </ul>
            <div id="GetLstSelPagging" class="paggingFilterPopupList" runat="server"></div>
            <ul class="catTool" runat="server" id="btnPrint" style="display: none">
                <li class="icon-print2" title="Imprimer" onclick="return false;"></li>
            </ul>

        </div>
        <div class="filterList" id="filterListGlobal" runat="server">
            <div class="tabeul" id="listContent" runat="server">
            </div>
        </div>
        <div id="listOptions" runat="server">

        </div>
        <%=StrMailTemplatesOptions %>
    </div>

    <script language="javascript" type="text/javascript">
    <%=_sGeneratedJavaScript %>

        var _activeFilter = '<%=_activeFilter %>';
        var _nSelectedFilter = '<%= _activeFilter %>';
        var _eCurentSelectedFilter = null;



        try {
            initHeadEvents();
        }
        catch (exp) { }

        // EVENEMENT LORS DU REDIMENSIONNEMENT DE LA MODALDIALOG
        function onFrameSizeChange(nNewWidth, nNewHeight) {

            var nTab = <%=TabList.ToString()%>;
            var oDivMain = document.getElementById("mainDiv");

            var listContent = document.getElementById("listContent");
            if (!listContent)
                return;

            listContent.style.height = (nNewHeight - 70 - 50) + "px"; //70 = hauteur des eléments au dessus de la liste, 50 = hauteur des boutons

            adjustLastCol(nTab, oDivMain, true);

        }
    </script>

</body>
</html>
