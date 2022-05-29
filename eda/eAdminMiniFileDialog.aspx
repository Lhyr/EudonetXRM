<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eAdminMiniFileDialog.aspx.cs" Inherits="Com.Eudonet.Xrm.eda.eAdminMiniFileDialog" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">  
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <title>Administrer les MiniFiches</title>
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>
</head>
<body class="adminModal bodyWithScroll" id="bodyAdminMiniFile" onload="load()">
    <form id="formAdminMiniFile" runat="server"></form>
    <script type="text/javascript">
        function load() {
            if (nsAdminMiniFile) {
                nsAdminMiniFile.load(<%=(int)this.MiniFileType %>);
            }
        }
    </script>
</body>
</html>
