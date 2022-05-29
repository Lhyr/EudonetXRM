<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eAdminFieldsRGPDListDialog.aspx.cs" Inherits="Com.Eudonet.Xrm.eda.eAdminFieldsRGPDListDialog" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<title>Liste des rubriques RGPD</title>
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
<body class="adminModal" id="bodyAdminFieldsRGPDList" onload="javascript:loadFieldsRGPDListDialog(<%=Tab%>);">
    <form id="formFieldsRGPDList" runat="server" onsubmit="return false;"></form>

    <script type="text/javascript">

        function loadFieldsRGPDListDialog(tab) {
            if (tab != "0" && nsAdminFieldsRGPDList) {
                nsAdminFieldsRGPDList.load(tab);
            }
        }
    </script>

</body>
</html>
