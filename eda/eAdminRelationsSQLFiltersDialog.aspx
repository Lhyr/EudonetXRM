<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eAdminRelationsSQLFiltersDialog.aspx.cs" Inherits="Com.Eudonet.Xrm.eda.eAdminRelationsSQLFiltersDialog" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Programmer un filtre sur la liste des fiches dans le signet</title>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>
</head>
<body class="bodyWithScroll"  id="bodyAdminRelations" onload="load()">
    <form runat="server" id="formAdminRelationsSQLFilters"></form>

    <script type="text/javascript">
        function load() {
            if (nsAdminRelationsSQLFilters) {
                nsAdminRelationsSQLFilters.load(<%=_nTab%>);
            }
        }
    </script>
</body>
</html>
