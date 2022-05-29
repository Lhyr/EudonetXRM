<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eGlobalAffectOwner.aspx.cs"
    Inherits="Com.Eudonet.Xrm.eGlobalAffectOwner" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title></title>
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>
    

    <script type="text/javascript" language="javascript">
        var nTotalFlNb = <%=TotalFilesNumber.ToString() %>;
    </script>
</head>
<body onload="onLoad();">
    <div id="GAUserList" class="GAUserList" runat="server">
    </div>
    <div id="DivBottom" class="usr_btm">
        <div id="btm1">
            <div id="dispHidUsr" class="dispHidUsr" runat="server">
            </div>
            <div id="autoset" class="autoset" onclick="autoSet();getDistribution();">
                <div id="aslogo" class="aslogo">
                </div>
                <div id="asLbl" class="asLbl">
                    <%=Res[6437]%>
                </div>
            </div>
        </div>
        <div id="dispOptUsr" class="usrbt-radio" runat="server">
        </div>
        <div class="sumup">
            <span id="nbUsr">0</span><span>&nbsp;<%=Res[6424] %></span><br />
            <span id="nbSetRec">0</span><span>&nbsp;<%=Res[6425] %></span> <span>&nbsp;/&nbsp;<%=TotalFilesNumber.ToString() %>
                <%=Res[950] %></span>
            <br />
            <span id="nbRem">
                <%=TotalFilesNumber.ToString() %></span>&nbsp;<span id="lblRem"><%=Res[6426] %> </span>
        </div>
    </div>
</body>
</html>
