<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ePJIframe.aspx.cs" Inherits="Com.Eudonet.Xrm.ePJIframe" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title><%=PageTitle %></title>
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <meta http-equiv="content-type" content="application/xhtml+xml; charset=UTF-8" />
    <meta name="robots" content="noindex">
    <meta name="googlebot" content="noindex">
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>
</head>
<body style="overflow:hidden">
    <form id="formPJIFrame" runat="server">

        <div id="errorContainer" runat="server">
            <h2 id="errorTitle1" runat="server"> </h2>
            <h1 id="errorTitle2" runat="server"> </h1>
            <p><%=_panelErrorMsg %></p>
            <div id="imgOops"></div>
            <a id="footerCopyright" href="https://www.eudonet.fr">Copyright © Eudonet. All rights reserved</a>
        </div>
    </form>
    <script type="text/javascript">
        window.onload = function AutoFit() {
            var iframe = document.getElementById("pjIframe");
            if (iframe) {
                iframe.height = window.innerHeight - 20;
            }
       

      }
    </script>
</body>
</html>
