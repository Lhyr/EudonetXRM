<%@ Import Namespace="Com.Eudonet.Xrm" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eCatalogAdvEdit.aspx.cs"
    Inherits="Com.Eudonet.Xrm.eCatalogAdvEdit" EnableSessionState="true" EnableViewState="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title></title>
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>
</head>
<body class="catDlg">
    <div runat="server" id="divCAtAdvEdit" class="divCAtAdvEdit">
    </div>
    <div runat="server" id="divCatAdvEditF">
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
            // On va mettre le focus sur la 1ere textbox et a la fin de la valeur pour la modification
            var _ctrl = document.getElementById('<%=firstTxtBox %>');
            var _length = _ctrl.value.length;

            function setCaretPosition(ctrl, pos) {
                if (ctrl.setSelectionRange) {
                    ctrl.focus();
                    ctrl.setSelectionRange(pos, pos);
                }
                else if (ctrl.createTextRange) {
                    var range = ctrl.createTextRange();
                    range.collapse(true);
                    range.moveEnd('character', pos);
                    range.moveStart('character', pos);
                    range.select();
                }
            }
            setCaretPosition(_ctrl, _length);
        }
    </script>

</body>
</html>
