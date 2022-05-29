<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eAdminFieldsListDialog.aspx.cs" Inherits="Com.Eudonet.Xrm.eda.eAdminFieldsListDialog" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Liste des rubriques</title>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>
    <style type="text/css">
        @media print { 
            @page {
                size: landscape;
                
            }
        }
    </style>
</head>
<body class="adminModal" id="bodyAdminFieldsList" onload="javascript:loadFieldsListDialog(<%=Tab%>);">
    <form id="formFieldsList" runat="server"></form>

    <script type="text/javascript">

        function loadFieldsListDialog(tab) {
            if (tab != "0" && nsAdminFieldsList) {
                nsAdminFieldsList.load(tab);

                resizeFieldsListContainer(window.frameElement.offsetHeight)
            }
        }

        

        // EVENEMENT LORS DU REDIMENSIONNEMENT DE LA MODALDIALOG
        function onFrameSizeChange(nNewWidth, nNewHeight) {
            resizeFieldsListContainer(nNewHeight);
        }

        function resizeFieldsListContainer(nNewHeight)
        {
            var listContainer = document.getElementById("fieldsListContainer");
            if (!listContainer)
                return;

            listContainer.style.height = (nNewHeight - 55) + "px"; //55 = hauteur des eléments au dessus de la liste
        }
    </script>

</body>
</html>
