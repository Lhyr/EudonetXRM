<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eCalendar.aspx.cs" Inherits="Com.Eudonet.Xrm.eCalendar"
    EnableSessionState="true" EnableViewState="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html>
<head>
    <title></title>
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>
    

    <script language="javascript" type="text/javascript">
    var eCalendarControl = null;
    var calDate = new Date();
    <%=jsStringVars %>

    function onLoadBody() {
        var divTop = document.getElementById("topCalDiv");
        var divBottom = document.getElementById("bottomCalDiv");
        eCalendarControl = new eCalendar("eCalendarControl","<%=_modalVarName %>",strDate, userDate,nHideHourField,nHideNoDate, divTop, divBottom, strHour ,strMin);
        eCalendarControl.BuildCalendar();
    }
    </script>
</head>
<body id="calendarbody" onload="onLoadBody();">
    <center>
        <div id="topCalDiv">
        </div>
        <div id="bottomCalDiv" class="calendarbody">
        </div>
    </center>
</body>
</html>
