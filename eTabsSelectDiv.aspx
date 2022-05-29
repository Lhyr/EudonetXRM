<%@ Import Namespace="Com.Eudonet.Xrm" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eTabsSelectDiv.aspx.cs"
    Inherits="Com.Eudonet.Xrm.eTabsSelectDiv" EnableSessionState="True" EnableViewState="false"
    EnableViewStateMac="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title></title>
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>


</head>
<body onload="initDragOpt();" style="overflow: hidden;">
    <form id="FrmDefault" runat="server">
        <table class="MainTab" cellpadding="0" cellspacing="0">
            <tr>
                <td></td>
                <td colspan="4">
                    <br />
                    <%=GetRes(6212) %> :
                    <br />
                    <br />
                </td>
            </tr>
            <tr>
                <td style="width: 2%"></td>
                <td style="width: 45%" class="tdHeader">
                    <%=GetRes(6231)%>
                </td>
                <td style="width: 4%">&nbsp;
                </td>
                <td style="width: 45%" class="tdHeader">
                    <%=GetRes(6232)%>
                </td>
                <td style="width: 4%">&nbsp;
                </td>
            </tr>
            <tr>
                <td></td>
                <td class="ItemListTd" id="TdSourceList" runat="server"></td>
                <td class="ItemListSep">
                    <center>
                        <div class="icon-item_add" id="DivButtonSelectUnit" onclick="SelectItem('AllTabList','TabSelectedList');">
                        </div>
                        <br />
                        <div class="icon-item_rem" id="DivButtonUnselectUnit" onclick="SelectItem('TabSelectedList','AllTabList');">
                        </div>
                    </center>
                </td>
                <td class="ItemListTd" id="TdTargetList" runat="server"></td>
                <td class="ItemListSep">
                    <center>
                        <div class="icon-item_up" id="DivButtonUpDown" onclick="MoveComboItem(true);">
                        </div>
                        <br />
                        <div class="icon-item_down" id="Div1" onclick="MoveComboItem(false);">
                        </div>
                    </center>
                </td>
            </tr>
            <tr>
                <td></td>
                <td>
                    <br />
                    <center>
                        <div class="btnSelectUnselectAll" id="DivButtonSelectAll" onclick="MoveAllItems('AllTabList','TabSelectedList');">
                            <div class="btnSelectUnselectAllIcon icon-select_all" id="DivButtonSelectAllIcon"></div>
                            <div class="btnSelectUnselectAllText" id="DivButtonSelectAllText"><%=GetRes(431)%></div>
                        </div>
                    </center>
                </td>
                <td></td>
                <td>
                    <br />
                    <center>
                        <div class="btnSelectUnselectAll" id="DivButtonUnselectAll" onclick="MoveAllItems('TabSelectedList','AllTabList');">
                            <div class="btnSelectUnselectAllIcon icon-remove_all" id="DivButtonUnselectAllIcon"></div>
                            <div class="btnSelectUnselectAllText" id="DivButtonUnselectAllText"><%=GetRes(432)%></div>
                        </div>
                    </center>
                </td>
                <td></td>
            </tr>
            <tr>
                <td colspan="5">
                    <br />
                </td>
            </tr>
            <tr>
                <td></td>
                <td colspan="4">
                    <div id="AdvancedTitle" class="DivClosed" closed="1" onclick="ShowHideAdvanced(this,parent.<%=_modalName %>);">
                        <span class="icon-add"></span><%=GetRes(597)%></span>
                    <br />
                        <div class="SeparatorAdvanced" />
                    </div>
                </td>
            </tr>
            <tr>
                <td></td>
                <td colspan="3">
                    <br />
                    <div id="DivSelectionList" style="display: none;">
                        <%=GetRes(6296)%> :<br />
                        <div class="DivOpen" onclick="AddView()"><span class="icon-add" title="<%=GetRes(18) %>"></span><span><%=GetRes(18)%></span></div>

                        <div class="tdHeader">
                            <%=GetRes(6197)%>
                        </div>
                        <div id="DivSelectionListResult" runat="server" class="DivSelectionListResult">
                        </div>
                    </div>
                </td>
                <td></td>
            </tr>
        </table>
    </form>
    <div id="Drag_divTmp" class="divDragTmp" style="position: absolute; display: none">
    </div>
    <input id="srch" onkeyup="onSrchType(this)" style="position: fixed; top:-500px;" />

</body>
</html>
