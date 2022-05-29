<%@ Import Namespace="Com.Eudonet.Xrm" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eFieldsSelect.aspx.cs"
    Inherits="Com.Eudonet.Xrm.eFieldsSelect" EnableSessionState="True" EnableViewState="false"
    EnableViewStateMac="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title></title>
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>
     
    <script type="text/javascript" language="javascript">
        // EVENEMENT LORS DU RESIZE DE LA MODALDIALOG DE CHOIX DES RUBRIQUES
        function onFrameSizeChange(w, h) {
            var oDivGlobal = document.getElementById("tabDialog");
            var oDivCatVal = document.getElementById("DivSourceList"); // DIV contenant les valeurs du catalogue
            var oDivCatSelMul = document.getElementById("DivTargetList"); // DIV contenant les valeurs sélectionnées
            if (oDivCatSelMul)
                oDivCatSelMul.style.height = (parseInt(h) - 170) + "px";
            if (oDivCatVal)
                oDivCatVal.style.height = (parseInt(h) - 170) + "px";
            if (oDivGlobal)
                oDivGlobal.style.height = (parseInt(h)) + "px";
        }    
    </script>
</head>
<body onload="initDragOpt();" style="overflow: hidden;">
    <form id="FrmDefault" runat="server">
    <table class="MainTab" id="tabDialog" cellpadding="0" cellspacing="0">
        <tr>
            <td style="width: 2%">
            </td>
            <td colspan="4">
                <br />
                <div class="eTitle" id="DivTitle" runat="server">
                    <%=GetRes(6712)%>
                    :
                </div>
                <div id="DivViewMode" runat="server">
                </div>
                <br />
            </td>
        </tr>
        <tr>
            <td style="">
            </td>
            <td style="width: 45%">
                <div class="eMainFileList" id="DivMainFileList" runat="server">
                </div>
                <br />
            </td>
            <td style="">
            </td>
            <td style="width: 45%">
            </td>
            <td style="">
            </td>
        </tr>
        <tr>
            <td>
            </td>
            <td class="tdHeader">
                <%=GetRes(6229)%>
            </td>
            <td>
                &nbsp;
            </td>
            <td class="tdHeader">
                <%=GetRes(6230)%>
            </td>
            <td>
                &nbsp;
            </td>
        </tr>
        <tr>
            <td>
            </td>
            <td class="ItemListTd">
                <div class="ItemList" id="DivSourceList" runat="server">
                </div>
            </td>
            <td class="ItemListSep">
                <center>
                    <div class="icon-item_add" id="DivButtonSelectUnit" onclick="AddItem();">
                    </div>
                    <br />
                    <div class="icon-item_rem" id="DivButtonUnselectUnit" onclick="DelItem();">
                    </div>
                </center>
            </td>
            <td class="ItemListTd">
                <div class="ItemList" id="DivTargetList" runat="server">
                </div>
            </td>
            <td class="ItemListSep">
                <center>
                    <div class="icon-item_up" id="DivButtonUp" onclick="MoveComboItem(true);">
                    </div>
                    <br />
                    <div class="icon-item_down" id="DivButtonDown" onclick="MoveComboItem(false);">
                    </div>
                </center>
            </td>
        </tr>
        <tr>
            <td>
            </td>
            <td align="center">
                <br />
                <div class="btnSelectUnselectAll" id="DivButtonSelectAll" onclick="AddAll();">
                    <div class="btnSelectUnselectAllIcon icon-select_all" id="DivButtonSelectAllIcon"></div>
                    <div class="btnSelectUnselectAllText" id="DivButtonSelectAllText"><%=GetRes(431)%></div>
                </div>
            </td>
            <td>
            </td>
            <td align="center">
                <br />
                <div class="btnSelectUnselectAll" id="DivButtonUnselectAll" onclick="DelAll();">
                    <div class="btnSelectUnselectAllIcon icon-remove_all" id="DivButtonUnselectAllIcon"></div>
                    <div class="btnSelectUnselectAllText" id="DivButtonUnselectAllText"><%=GetRes(432)%></div>
                </div>
            </td>
            <td>
            </td>
        </tr>
        <tr>
            <td colspan="5">
                <br />
                <br />
            </td>
        </tr>
        <tr style="display: none;">
            <td>
            </td>
            <td colspan="4">
                <div id="AdvancedTitle" class="DivClosed" closed="1" onclick="ShowHideAdvanced(this);">
                    <%=GetRes(597)%>
                </div>
                <div class="eSelectionList" id="DivSelectionList" style="display: none;" runat="server">
                    Vues disponibles dans la base :
                </div>
            </td>
        </tr>
    </table>
        <input id="srch" onkeyup="doSearch(this);" style="position:fixed;top:-500px;"/>
    </form>
</body>
</html>
