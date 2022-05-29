<%@ Page Language="C#" AutoEventWireup="true" ViewStateMode="Disabled" EnableViewState="false" CodeBehind="eXrmHomePageList.aspx.cs" Inherits="Com.Eudonet.Xrm.eXrmHomePageList" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Administrer les droits</title>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>
</head>
<body>
    <div id="HomePageListContainer" class="list-container">
         <div id="listContent" class="tabeul" runat="server"></div>
    </div>
    <script type="text/javascript">

        var _activeFilter = '0';
        var _nSelectedFilter = '0';
        var _eCurentSelectedFilter = null;
        nGlobalActiveTab = 115200;
        try {
            initHeadEvents();
        }
        catch (exp) { }

        // EVENEMENT LORS DU REDIMENSIONNEMENT DE LA MODALDIALOG
        function onFrameSizeChange(nNewWidth, nNewHeight) {

            var nTab = 115200;
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
