<%@ Import Namespace="Com.Eudonet.Xrm" %>
<%@ Import Namespace="EudoQuery" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eFilterWizard.aspx.cs"
    Inherits="Com.Eudonet.Xrm.eFilterWizard" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title></title>
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>

   
     
    <script type="text/javascript" language="javascript">
        var MAX_NBRE_TABS= <%=EudoQuery.MAX_NBRE_PAGES%>;
        var MAX_NBRE_LINES = <%=EudoQuery.MAX_NBRE_LINE%>;
        var nTab = <%=_tabDid%>;
        var nfilterType = <%=_filterType.GetHashCode()%>;
        var parentIframe = "<%=_parentIframe%>";
        
    </script>
    <%
        if (sAction == "calselect")
        { 
    %>
    <script language="javascript" type="text/javascript">
        var currentIframeId = '<%=_currentIframeId%>';
        var eCalendarControl = null;
        var calDate = new Date();
        var strDate = '<%=strDateValue%>';
        var userDate = '<%=userDate%>';
        var strHour = '<%=strHour%>';
        var strMin = '<%=strMin%>';
        var strParentJsValidFunction = "";

        function onLoadBody() {
            var divTop = document.getElementById("topCalDiv");
            var divBottom = document.getElementById("bottomCalDiv");
            eCalendarControl = new eCalendar("eCalendarControl", "<%=_modalvarname %>", strDate, userDate, '0', '1', divTop, divBottom, strHour, strMin);
            eCalendarControl.BuildCalendar();
        }
        function onCalOptClick() {

            var radioObj = document.forms['radioOptForm'].elements['date'];

            for (var i = 0; i < radioObj.length; i++) {
                radioObj[i].checked = false;
                if (radioObj[i].value == "3") {
                    radioObj[i].checked = true;
                }
            }
            onSelectRadio(3);

        }
        
    </script>
    <%
        }
        else
        {
    %>
    <script language="javascript" type="text/javascript">
        
        var currentIframeId = '<%=_currentIframeId%>';
        function onLoadBody() {

            //initialisation des blocks de visu et de modif
            doPermParam("View");
            doPermParam("Update");            
            
            resizeFilterMainDiv();
         }
    </script>
    <%
        }
    %>
</head>
<body onload="onLoadBody();">
    <%
        if (sAction == "calselect")
        { 
    %>
    <table class="tabOptions">
        <tr>
            <td colspan="3">
                <%=Com.Eudonet.Internal.eResApp.GetRes(_pref, 6281)%>
                :
            </td>
        </tr>
        <tr>
            <td class="TdOpt" onclick="onCalOptClick()">
                <div runat="server" id="Calendar" class="CalContainer">
                    <div id="topCalDiv">
                    </div>
                    <div id="bottomCalDiv" class="calendarbody">
                    </div>
                </div>
            </td>
            <td class="TdOpt" runat="server" id="tdWizSep">
                <div runat="server" id="vSep" class="VSep">
                </div>
            </td>
            <td class="TdOpt" runat="server" id="tdWizDecal">
                <div runat="server" id="DivDateSelect" class="DateOpt">
                </div>
            </td>
        </tr>
    </table>
    <%
        }
        else
        {
            if (sAction == "getlinkedfile")
            {
    %>
    <div runat="server" id="LinkFileResult" class="LinkFileResult">
    </div>
    <%
        }
            else
            { 
    %>
    <form method="post" action="">
    <div class="MainWizard" runat="server" id="MainWizard">
    </div>
    <script type="text/javascript" language="javascript">
        var bIsTablet = false;
        try {
            if (typeof (isTablet) == 'function')
                bIsTablet = isTablet();
            else if (typeof (top.isTablet) == 'function')
                bIsTablet = top.isTablet();
        }
        catch (e) {

        }

        if (!bIsTablet) {
            if (document.getElementById("FilterName") != null)
                document.getElementById("FilterName").focus();
        }
    </script>
    </form>
    <%
            }
        }
    %>
    <script type="text/javascript" language="javascript">
        try{
            var isSafari = /^((?!chrome|android).)*safari/i.test(navigator.userAgent);
            if(isSafari)
            {
                setTimeout( resizeFilterMainDiv,50);
                
            }
        }
        catch(e)
        {

        }
    </script>
</body>
</html>
