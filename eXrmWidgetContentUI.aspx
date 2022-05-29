<%@ Page Language="C#" AutoEventWireup="true" ViewStateMode="Disabled" EnableViewState="false" CodeBehind="eXrmWidgetContentUI.aspx.cs" Inherits="Com.Eudonet.Xrm.eXrmWidgetContentUI" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>
    
    <script type="text/javascript">
        eTools.setWidgetWait(<%=_wid %>, true);
    </script>
</head>
<body onload="init();" class="widget-iframe-body">
    <form runat="server" id="formWidgetContent">
        <input type="hidden" id="hidWidgetType" runat="server" />
        <input type="hidden" id="hidWid" runat="server" />
        <div runat="server" ID="contentWrapper"></div>
    </form>
     <script type="text/javascript">
        function init() {
            addScript("grid/eWidgetContent", "WIDGET", function () {
                eWidgetContent.init(<%=(int)_wt %>, <%=_wid %>, <%=_tab %>, '<%=_context.Replace(Environment.NewLine, String.Empty) %>');
            });
        }
     </script>
</body>
</html>
