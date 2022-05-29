<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eGoToFile.aspx.cs" Inherits="Com.Eudonet.Xrm.eGoToFile" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>
</head>
<body>
    <form id="formRedirect" runat="server">
        <input type="hidden" id="redirTabID" runat="server" />
        <input type="hidden" id="redirFileID" runat="server" />
        <input type="hidden" id="fileInPopup" runat="server" />
        <input type="hidden" id="redirTPLMail" runat="server" />
        <input type="hidden" id="redirBkmTabID" runat="server" />
        <input type="hidden" id="redirBkmFileID" runat="server" />
    </form>

    <script type="text/javascript">
        document.getElementById('formRedirect').submit();
        <%=_jsToExecute %>
    </script>
</body>
</html>
