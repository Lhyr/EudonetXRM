<%@ Page Language="C#" AutoEventWireup="true" ViewStateMode="Disabled" EnableViewState="false" CodeBehind="eAdminAutomationListDialog.aspx.cs" Inherits="Com.Eudonet.Xrm.eda.eAdminAutomationListDialog" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Administrer les droits</title>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>
</head>
<body class="adminModal bodyWithScroll">
    <div id="adminContainer" class="admin-container">
         <div id="listContent" runat="server"></div>
    </div>
    <script type="text/javascript">

        var _activeFilter = '0';
        var _nSelectedFilter = '0';
        var _eCurentSelectedFilter = null;
        nGlobalActiveTab = 114200;
        try {
            initHeadEvents();
        }
        catch (exp) { }

        // EVENEMENT LORS DU REDIMENSIONNEMENT DE LA MODALDIALOG
        function onFrameSizeChange(nNewWidth, nNewHeight) {

            var nTab = 114200;
            var oDivMain = document.getElementById("mainDiv");

            var listContent = document.getElementById("listContent");
            if (!listContent)
                return;

            listContent.style.height = (nNewHeight - 70 - 50) + "px"; //70 = hauteur des eléments au dessus de la liste, 50 = hauteur des boutons

            adjustLastCol(nTab, oDivMain, true);
        }

    </script>
</body>
</html>
