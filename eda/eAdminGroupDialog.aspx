<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eAdminGroupDialog.aspx.cs" Inherits="Com.Eudonet.Xrm.eda.eAdminGroupDialog" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Groupes</title>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>
</head>
<body class="adminModal" runat="server" id="bodyAdminGroup">
    <form runat="server" id="formAdminGroup" onsubmit="top.nsAdminUsers.onGroupDialogSubmit(event);"></form>
</body>
</html>
