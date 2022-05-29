<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eAdminRelationsDialog.aspx.cs" Inherits="Com.Eudonet.Xrm.eda.eAdminRelationsDialog" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Administrer les relations</title>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>
</head>
<body class="bodyWithScroll"  id="bodyAdminRelations" onload="load()">
    <form runat="server" id="formAdminRelations"></form>

    <script type="text/javascript">
        function load() {
            if (nsAdminRelations) {
                nsAdminRelations.load(<%=_nTab%>);
            }
            if (FieldsSelect) {
                objFieldsSelect = new FieldsSelect();
            }
        }
    </script>
</body>
</html>
