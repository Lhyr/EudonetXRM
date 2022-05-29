<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="eEudoCtrlUsrListSklton.ascx.cs" Inherits="Com.Eudonet.Xrm.UserControls.eEudoCtrlUsrListSklton" ClientIDMode="Static" %>

<asp:Panel ID="pnWaiterSkltonlist" class="SktOff-list" runat="server">
    <table class="table_loader table_loader_header">
        <tr>
            <td class="col3">
                <span></span>
            </td>
            <td class="white">
                <span></span>
            </td>
            <td class="col2">
                <span></span>
            </td>
        </tr>
    </table>

    <asp:Table ID="tblListSklton" CssClass="table_loader table_loader_content" runat="server">
    </asp:Table>
</asp:Panel>
