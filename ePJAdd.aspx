<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ePJAdd.aspx.cs" Inherits="Com.Eudonet.Xrm.ePJAdd"
    EnableSessionState="true" EnableViewState="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">

<head>
    <title runat="server">Annexes</title>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>
</head>

<body>
    <%
        if (_sAction == "init")
        { 
    %>
    <form enctype="Multipart/form-Data" accept-charset="utf-8" id="frmUpload" name="frmUpload"
        action="ePJAdd.aspx" method="post" runat="server">
        <div class="divPJGlobal">
            <ul class="createAnn">
                <li class="pJDlMsg">
                    <input id="radPJFile" name="radioPJ" type="radio" onclick="ChangeType(0);" />
                    <%=string.Concat(Com.Eudonet.Internal.eResApp.GetRes(_pref, 6316), " ", Com.Eudonet.Internal.eResApp.GetRes(_pref, 6311))%> :</li>
                <li>
                    <asp:FileUpload ID="FileToUpload" runat="server" onchange="ChangeType(0);" />
                    <input id="SaveAs" name="SaveAs" type="hidden" />
                </li>
                <li class="pJsep"></li>
                <li>
                    <input id="radPJLink" name="radioPJ" type="radio" onclick="ChangeType(1);" />
                    <%=Com.Eudonet.Internal.eResApp.GetRes(_pref, 6315)%>
                (<span><%=Com.Eudonet.Internal.eResApp.GetRes(_pref, 6313)%></span>)</li>
                <li><%=Com.Eudonet.Internal.eResApp.GetRes(_pref, 1500)%> :</li>
                <li>
                    <input type="text" id="uploadvalue" name="uploadvalue" onclick="ChangeType(1);" />
                </li>
                <li>
                    <%=Com.Eudonet.Internal.eResApp.GetRes(_pref, 119)%>
                (<%=Com.Eudonet.Internal.eResApp.GetRes(_pref, 6314)%>) :</li>
                <li>
                    <input type="text" id="txtToolTip" name="txtToolTip" />
                </li>

                <li class="pJsep"></li>
                <li>
                    <%=Com.Eudonet.Internal.eResApp.GetRes(_pref, 121)%>
                :</li>
                <li>
                    <input type="text" id="txtDesc" name="txtDesc" value="<%=Com.Eudonet.Internal.eResApp.GetRes(_pref, 113) %>&nbsp;:&nbsp;<%=Com.Eudonet.Internal.eDate.ConvertBddToDisplay(_pref.CultureInfo, DateTime.Now) %>" />
                </li>
            </ul>
        </div>
        <div id="divHidden" runat="server">
            <input type="hidden" id="action" name="action" />
            <input type="hidden" id="lstTypeLink" name="lstTypeLink" />
            <input type="hidden" id="UploadLink" name="UploadLink" />
            <input type="hidden" id="txtToolTipLink" name="txtToolTipLink" />
            <input type="hidden" id="txtDescLink" name="txtDescLink" />
            <input type="hidden" id="herrorCallBack" name="herrorCallBack" runat="server" />
            <input type="hidden" id="hfMemoId" name="hfMemoId" runat="server" />
            <input type="hidden" id="hUploadMode" name="hUploadMode" runat="server" />
        </div>
    </form>
    <%
        }
    %>
</body>
</html>
