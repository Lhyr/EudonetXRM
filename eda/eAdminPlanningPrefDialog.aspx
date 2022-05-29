<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eAdminPlanningPrefDialog.aspx.cs" Inherits="Com.Eudonet.Xrm.eda.eAdminPlanningPrefDialog" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Administrer les préférences par défaut du mode graphique</title>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>
</head>
<body class="adminModal bodyWithScroll" id="bodyAdminPlanningPref" onload="javascript:loadPlanningPrefDialog(<%=Tab%>);">
    <form runat="server" id="formAdminPlanningPref"></form>

    <script type="text/javascript">
        function loadPlanningPrefDialog(tab) {
            if (tab != "0" && nsAdminPrefPlanning) {
                nsAdminPrefPlanning.load(tab);
            }
        }
    </script>
</body>
</html>
