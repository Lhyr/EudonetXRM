<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eFieldFiles.aspx.cs" Inherits="Com.Eudonet.Xrm.eFieldFiles" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title></title>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>


</head>
<body ondrop="return false;" ondragover="return false;" ondragleave="return false;" onload="initFileNameEditor();setWait(false);" class="bodyFieldFiles">
    <script type="text/javascript">
        bMultiple = "<%=_bMultiple?"1":"0"%>" == "1";
    </script>
    <div class="window_iframe" id="mainDiv" runat="server">
        <form id="form1" runat="server">
            <input id="folder" name="folder" runat="server" type="hidden" />
            <input id="descid" name="descid" runat="server" type="hidden" />
            <input id="mult" name="mult" runat="server" type="hidden" />
            <input id="files" name="files" runat="server" type="hidden" />
            <input id="filetype" name="filetype" runat="server" type="hidden" />

            <asp:Label ID="lbl_erreur" runat="server" Text="" CssClass="lblErreur" ForeColor="Red"></asp:Label>

            <ul class="anxFilUp">
                <li style="margin-bottom: 15px;" runat="server" id="li1"></li>
                <li>
                    <asp:FileUpload ID="FileToUpload" runat="server" CssClass="fileUpload" />
                    <asp:LinkButton CssClass="anxFile_a buttonAdd" runat="server" OnClientClick="javascript:setWait(true);"
                        ID="lnkBtnAddFile" mode="" OnClick="lnkBtnAddFile_Click">
                        <asp:Label CssClass="icon-add" id="iconPlus" runat="server" Text=""></asp:Label>
                        <asp:Label ID="lblAdd" CssClass="addLbl" Text="" runat="server"></asp:Label>
                    </asp:LinkButton>
                </li>

            </ul>
            <div id="divLstFiles" class="divlstPJ" runat="server"></div>
        </form>
    </div>
</body>
</html>
