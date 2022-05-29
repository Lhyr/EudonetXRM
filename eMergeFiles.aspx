<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eMergeFiles.aspx.cs" Inherits="Com.Eudonet.Xrm.eMergeFiles" EnableViewState="false" EnableSessionState="true" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title></title>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>


    <script type="text/javascript">
        function GetReturnValue() {
            var resObj = {
                fieldChange: '',
                fieldConcat: ''
            };
            // Rubrique (à changer)
            var concatRadio = document.querySelectorAll("input[doublon][type=radio][did]"); //GCH querySelector :checked incompatible avec IE8 => test dans la boucle directement
            forEach(concatRadio, function (oRadio) {
                if (!oRadio.checked)
                    return;
                if (resObj.fieldChange.length > 0)
                    resObj.fieldChange += ";";
                resObj.fieldChange += getAttributeValue(oRadio, "did");
            });

            // Rubrique multiple (à changer ou à concatener)
            var concatChk = document.querySelectorAll("a[doublon][chk='1'][did]");
            forEach(concatChk, function (oChk) {
                // Recup du master correspondant
                var oMasterChk = document.getElementById(oChk.id.replace("_DOUBLON_", "_MASTER_"));

                if (oMasterChk != null && typeof (oMasterChk) != 'undefined') {

                    if (getAttributeValue(oMasterChk, "chk") == "1") {
                        // On concat si le master est egalement coché
                        if (resObj.fieldConcat.length > 0)
                            resObj.fieldConcat += ";";
                        resObj.fieldConcat += getAttributeValue(oChk, "did");
                    }

                    else {
                        // Sinon on change la valeur
                        if (resObj.fieldChange.length > 0)
                            resObj.fieldChange += ";";
                        resObj.fieldChange += getAttributeValue(oChk, "did");
                    }
                }
            });

            // Options
            var chk = document.getElementById("KeepAllAdr");
            resObj.keepAllAdr = chk && chk.getAttribute("chk") == "1";
            chk = document.getElementById("OverwriteAdrInfos");
            resObj.overwriteAdrInfos = chk && chk.getAttribute("chk") == "1";

            return resObj;
        }
    </script>
</head>
<body onload="AdjustScrollDiv()">
    <div runat="server" id="divCompFiles" class="divCompFiles">
    </div>
</body>
</html>
