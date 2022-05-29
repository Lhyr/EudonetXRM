<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ePrintFile.aspx.cs" Inherits="Com.Eudonet.Xrm.ePrintFile"
    EnableSessionState="true" EnableViewState="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>Eudonet XRM - Impression</title>
    <meta http-equiv='X-UA-Compatible' content='IE=edge' />
    <meta http-equiv='content-type' content='text/html; charset=UTF-8' />
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>
    <script><%=_sJs%></script>
</head>
<body>
    <div id="divHeadPage">
        <div id="btnPrint" onclick="window.print();">
            <span><%=Com.Eudonet.Internal.eResApp.GetRes(_pref, 13) %></span>
        </div>
    </div>
    <form id="form1" runat="server">
        <div id="PrintContent" runat="server">
        </div>
    </form>
    
</body>
</html>
