<%@ Page Language="C#" EnableViewState="false" ViewStateMode="Disabled" AutoEventWireup="true" CodeBehind="eAdminPictoDialog.aspx.cs" Inherits="Com.Eudonet.Xrm.eda.eAdminPictoDialog" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Administrer les pictogrammes</title>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>
</head>
<body class="adminModal bodyWithScroll" runat="server" id="bodyAdminPicto">
    <form runat="server" id="formAdminPicto" class="picto-container" onsubmit="return false;"></form>
</body>
</html>
