<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eDateSelect.aspx.cs" Inherits="Com.Eudonet.Xrm.eDateSelect"
    EnableSessionState="true" EnableViewState="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 //EN">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>

    <script lang="javascript" type="text/javascript">
        var eCalendarControlStart = null;
        var eCalendarControlEnd = null;

                 <%=jsStringVars %>

        function onLoadBody() {

            var divTopStart = document.getElementById("topCalDivStart");
            var divBottomStart = document.getElementById("bottomCalDivStart");
            eCalendarControlStart = new eCalendar("eCalendarControlStart", "<%=_modalVarNameStart %>", strDateStart, userDateStart, nHideHourField, nHideNoDate, divTopStart, divBottomStart, strHour, strMin);
                    eCalendarControlStart.BuildCalendar();

                    var divTopEnd = document.getElementById("topCalDivEnd");
                    var divBottomEnd = document.getElementById("bottomCalDivEnd");
                    eCalendarControlEnd = new eCalendar("eCalendarControlEnd", "<%=_modalVarNameEnd %>", strDateEnd, userDateEnd, nHideHourField, nHideNoDate, divTopEnd, divBottomEnd, strHour, strMin);
                eCalendarControlEnd.BuildCalendar();

            }

    </script>

</head>
<body id="calendarbody" onload="onLoadBody();">

    <div class="dbCalendar">
        <div style="display: inline-block;">
            <div id="topCalDivStart">
            </div>
            <div id="bottomCalDivStart" class="calendarbody">
            </div>
        </div>
        <div style="display: inline-block;">
            <div id="topCalDivEnd">
            </div>
            <div id="bottomCalDivEnd" class="calendarbody">
            </div>
        </div>

    </div>
</body>
</html>

