<%@ Import Namespace="Com.Eudonet.Xrm" %>
<%@ Import Namespace="Com.Eudonet.Internal" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eTreatmentDialog.aspx.cs"
    EnableSessionState="true" EnableViewState="false" Inherits="Com.Eudonet.Xrm.eTreatmentDialog" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title></title>
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>
    
 


</head>
<body onload="initLoad();">
    <div class="window_iframe" id="mainDiv" runat="server">
        <p id="message" runat="server"></p>
    </div>
    <script language="javascript" type="text/javascript">
        <%=_generatedJavaScript.ToString()%>
    </script>
</body>
</html>
