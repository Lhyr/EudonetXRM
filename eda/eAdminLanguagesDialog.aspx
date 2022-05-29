<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eAdminLanguagesDialog.aspx.cs" Inherits="Com.Eudonet.Xrm.eda.eAdminLanguagesDialog" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <title>Administrer les langues</title> 
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>
</head>
<body class="adminModal bodyWithScroll" id="bodyAdminLanguages" onload="javascript:loadLanguagesDialog();">
    <form id="formAdminLanguages" runat="server"></form>
    <script type="text/javascript">
        function loadLanguagesDialog() {
            if (nsAdminLang) {
                nsAdminLang.load();
            }
        }
    </script>
</body>
</html>
