<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eAddMailAddr.aspx.cs" Inherits="Com.Eudonet.Xrm.eAddMailAddr" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title></title>
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>

</head>
<body class="catDlg addMailToList">
    <form id="form1" runat="server">
        <input type="hidden" runat="server" id="pmid" name="pmid" />
        <input type="hidden" runat="server" id="adrid" name="adrid" />
        <input type="hidden" runat="server" id="ppid" name="ppid" />
        <input type="hidden" runat="server" id="fileType" name="fileType" />
        <div id="divCatDialog" class="catDlg">
            <div id="catDivSrch" class="catDivSrch">
                <table cellpadding="0" cellspacing="0" border="0">
                    <tbody>
                        <tr>
                            <td class="txtsrch">Recherche : </td>
                            <td>
                                <input runat="server" name="search" type="text" id="search" class="eTxtSrch" onkeyup="searchAddr(this, event);" /></td>
                            <td>
                                <div class="catloupe">
                                        <span id="eBtnSrch" title="Lancer la recherche" srchstate="off" class="logo-search icon-magnifier" onclick="javascript:searchAddr(document.getElementById('search'));">
                                        </span>
    
                                </div>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
            <div id="catDivHead" class="catDivHead">
            </div>
            <div id="eCEDValues" class="catEditVal" onclick="setAddr(event,true);">
                <ul runat="server" id="ulListAddr" class="catEditVal" cellpadding="0" cellspacing="0">
                </ul>
            </div>
        </div>
    </form>
</body>
</html>
