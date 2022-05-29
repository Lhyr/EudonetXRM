<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eReportUserList.aspx.cs" Inherits="Com.Eudonet.Xrm.eReportUserListDialog"
    EnableSessionState="true" EnableViewState="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title></title>
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>
     


    <script type="text/javascript">

        var _activeFilter = 0;
        var _nSelectedFilter = 0;
        var _eCurentSelectedFilter = null;

        // Init du mode liste
        function init(){
            try { initHeadEvents(); } catch (exp) { }
        }

        // EVENEMENT LORS DU REDIMENSIONNEMENT DE LA MODALDIALOG
        function onFrameSizeChange(nNewWidth, nNewHeight) {

            var nTab = 102000;
            var oDivMain = document.getElementById("mainDiv");

            var listContent = document.getElementById("listContent");
            if (!listContent)
                return;

            listContent.style.height = (nNewHeight - 70 - 50) + "px"; //70 = hauteur des eléments au dessus de la liste, 50 = hauteur des boutons

            adjustLastCol(nTab, oDivMain, true);

        }

        function archiveReport(id) {

            var url = "mgr/eReportManager.ashx";
            var ednu = new eUpdater(url, 0);
            ednu.ErrorCallBack = function () { };
            ednu.addParam("operation", 10, "post");
            ednu.addParam("reportid", id, "post");
            ednu.send();
        }

        function reloadReportList(bArchived) {
            var url = "mgr/eReportManager.ashx";
            var ednu = new eUpdater(url, 1);
            ednu.ErrorCallBack = function () { };
            ednu.addParam("operation", 11, "post");
            ednu.addParam("archiveonly", bArchived ? 0 : 1, "post");
            ednu.send(updateReportList);
        }

        function updateReportList(oList) {
            var oContent = document.getElementById("DivGlobal");
            oContent.innerHTML = oList;
        }
    </script>
</head>
<body onload="init();">
    <div id="DivGlobal" class="Global" runat="server">
    </div>
</body>
</html>
