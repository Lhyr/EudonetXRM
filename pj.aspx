<%@ Import Namespace="Com.Eudonet.Xrm" %>
<%@ Import Namespace="Com.Eudonet.Internal" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="pj.aspx.cs" Inherits="Com.Eudonet.Xrm.pj"
    EnableSessionState="true" EnableViewState="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 //EN">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title><%=PageTitle %></title>
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <meta http-equiv="content-type" content="application/xhtml+xml; charset=UTF-8" />
    <meta name="robots" content="noindex">
    <meta name="googlebot" content="noindex">
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>

    <link rel="shortcut icon" type="image/x-icon" href="<%=eLibTools.GetAppUrl(Request) %>/themes/<%=DefaultTheme.Folder %>/images/favicon.ico" />
    <link rel="stylesheet" type="text/css" href="<%=eLibTools.GetAppUrl(Request) %>/themes/<%=DefaultTheme.Folder %>/css/ePJ.css?ver=<%=String.Concat(eConst.VERSION,".", eConst.REVISION) %>" />
</head>
<body id="bodyPJ">
    <%
        if (RendType == ExternalPageRendType.ERROR)
        {
    %>
    <div id="errorContainer" runat="server">
        <h2><%=eResApp.GetRes(UserLangId, 416) %></h2>
        <h1><%=eResApp.GetRes(UserLangId, 7175) %></h1>
        <p><%=_panelErrorMsg %></p>
        <div id="imgOops"></div>
        <a id="footerCopyright" href="https://www.eudonet.fr">Copyright © Eudonet. All rights reserved</a>
    </div>
    <%
        }
    %>
</body>
</html>

