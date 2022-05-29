<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eExportToV7.aspx.cs" Inherits="Com.Eudonet.Xrm.eExportToV7page" EnableSessionState="true" EnableViewState="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title></title>
</head>
<body onload="document.getElementById('exporttov7').submit()">
    <form method="post" name="exporttov7" id="exporttov7" runat="server">
        <div style="visibility: hidden">
            <textarea runat="server" id="token" name="token" rows="1" cols="1"></textarea>
            <textarea runat="server" id="baseurl" name="baseurl" rows="1" cols="1"></textarea>
            <input runat="server" id="typespecif" name="typespecif" />
        </div>
    </form>
</body>
</html>
