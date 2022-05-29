<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="eEudoCtrlUsrFileSklton.ascx.cs" Inherits="Com.Eudonet.Xrm.UserControls.eEudoCtrlUsrFileSklton" ClientIDMode="Static" %>

<asp:Panel ID="pnWaiterSkltonfile" class="SktOff-file" runat="server" ClientIDMode="Static">
    <asp:Table ID="tblHeader1" CssClass="table_loader table_loader_header" runat="server">
    </asp:Table>

    <asp:Table ID="tblHeader2" CssClass="table_loader table_loader_header" runat="server">
    </asp:Table>

    <table class="table_loader table_loader_summary">
        <tr>
            <td class="col1 circle">
                <span></span>
            </td>
            <td class="col9">
                <span></span>
                <span class="sub-temp"></span>
                <span></span>
                <span class="sub-temp sub-temp-two"></span>
            </td>
        </tr>
    </table>

    <asp:Table ID="tblStepper1" CssClass="table_loader table_loader_assistant" runat="server">
    </asp:Table>
    <asp:Table ID="tblStepper2" CssClass="table_loader table_loader_assistant" runat="server">
    </asp:Table>

    <asp:Table ID="tblFooter" CssClass="table_loader table_loader_header" runat="server">
    </asp:Table>
        
    <asp:Table ID="tblListSklton" CssClass="table_loader table_loader_content" runat="server">
    </asp:Table>
</asp:Panel>
