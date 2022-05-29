<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eAdminAdvSearchDialog.aspx.cs" Inherits="Com.Eudonet.Xrm.eda.eAdminAdvSearchDialog" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Recherche avancée</title>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>
</head>
<body class="bodyWithScroll"  id="bodyAdminAdvSearch" onload="load()">
    <form runat="server" id="formAdminAdvSearch"></form>

    <script type="text/javascript">
        function load() {
<%--            if (nsAdminRelations) {
                nsAdminRelations.load(<%=_nTab%>);
            }--%>
            if (FieldsSelect) {
                objFieldsSelect = new FieldsSelect();
            }
        }
    </script>
</body>
</html>

