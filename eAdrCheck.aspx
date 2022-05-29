<%@ Import Namespace="Com.Eudonet.Xrm" %>
<%@ Import Namespace="Com.Eudonet.Internal" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eAdrCheck.aspx.cs" Inherits="Com.Eudonet.Xrm.eAdrCheck"
    EnableSessionState="true" EnableViewState="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>Eudonet XRM</title>
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>

    <script language="javascript" type="text/javascript">
        function CheckAll(boxChecked) {
            var imgs = getArrayFromTag(document, 'A');

            if (typeof (boxChecked) == 'undefined')
                boxChecked = false;

            for (var idx = 0; idx < imgs.length; idx++) {
                var oImg = imgs[idx];

                if (!oImg.getAttribute('chk') || !oImg.getAttribute('dis'))
                    continue;

                chgChk(oImg, boxChecked);
            }
        }

        function GetReturnValue(cancelBtn) {
            var adrNoUpd = '';
            var adrToUpd = document.getElementById('hiddenAdrtoupd').value;
            
            if (!cancelBtn) {
                var imgs = getArrayFromTag(document, 'A');
                for (var idx = 0; idx < imgs.length; idx++) {
                    var oImg = imgs[idx];

                    if (oImg.getAttribute('adri') == null || typeof (oImg.getAttribute('adri')) == 'undefined')
                        continue;

                    if (oImg.getAttribute('chk') == '1')
                        adrNoUpd += oImg.getAttribute('id') + ';';
                }

                if (adrNoUpd.length > 0) {
                    adrNoUpd = adrNoUpd.substr(0, adrNoUpd.length-1);
                    adrToUpd += ';$|$;' + adrNoUpd;
                }
            }

            return adrToUpd;
        }
    </script>
</head>
<body>
    <input runat="server" type="hidden" id="hiddenDescid" name="hiddenDescid" value="" />
    <input runat="server" type="hidden" id="hiddenAdrtoupd" name="hiddenAdrtoupd" value="" />
    <table cellpadding="0" cellspacing="0" class="AdrCheckTable">
        <thead>
            <tr>
                <td class="td-logo-info"><img class="logo-info" src="ghost.gif" /></td>
                <td runat="server" class="text-alert-info" id="textMsgInfo"></td>
            </tr>
        </thead>
        <tbody>
            <tr class="btnAdrCheck"><td colspan="2">
                <span onclick="CheckAll(true);"><%=eResApp.GetRes(_pref.Lang, 435)%></span> / <span onclick="CheckAll(false);"><%=eResApp.GetRes(_pref.Lang, 436)%></span>
            </td></tr>
            <tr><td colspan="2"><div runat="server" id="adrCheckList" class="adrCheckList"></div></td></tr>
        </tbody>
    </table>
</body>
</html>
