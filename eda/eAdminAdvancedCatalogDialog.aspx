<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eAdminAdvancedCatalogDialog.aspx.cs" Inherits="Com.Eudonet.Xrm.eda.eAdminAdvancedCatalogDialog" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <title>Administrer l’affichage des valeurs, les droits et options du catalogue</title>
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>
</head>
<body class="adminModal bodyWithScroll" id="bodyAdminAdvancedCatalog" onload="load()">
    <form id="formAdminAdvancedCatalog" runat="server"></form>

    <script type="text/javascript">
        function load() {
            if (nsAdminAdvancedCatalog) {
                nsAdminAdvancedCatalog.load(<%=_nTab%>, <%=_nField%>);
            }
        }
    </script>
</body>
</html>
