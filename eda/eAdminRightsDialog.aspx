<%-- SHA : demande #75 330 --%>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eAdminRightsDialog.aspx.cs" Inherits="Com.Eudonet.Xrm.eda.eAdminRightsDialog"  EnableEventValidation="false" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Administrer les droits</title>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>  
</head>
<body class="adminModal bodyWithScroll" id="bodyAdminRights" onload="load();">
    <form runat="server" id="formAdminRights">
    </form>
    <script type="text/javascript">

        function load() {
            nsAdminRights.loadRightsSliders();
        }
    </script>
</body>
</html>
