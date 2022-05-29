<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eImportCatalogDialog.aspx.cs" Inherits="Com.Eudonet.Xrm.eImportCatalogDialog" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
        <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>
</head>
<body>
    <form id="form1" runat="server" onSubmit=""  method="post">
        <div>
            <textarea runat="server" id="eTextImportCat" name="eTextImportCat" value="LIB_FRANCAIS;INFOBULLE_FRANCAIS" style="border: 1px solid #b4b4b4; height: 420px; width: 750px; overflow: auto; padding: 12px;" rows="5" cols="5"></textarea>
        </div>
    </form>
</body>
</html>
