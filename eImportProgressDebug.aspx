<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eImportProgressDebug.aspx.cs" Inherits="Com.Eudonet.Xrm.eImportProgressDebug"
    EnableSessionState="true" EnableViewState="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 //EN">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>
</head>

<body>
    <script>
        <% if (bDebug)
        { %>
        var handle = setInterval(function () {
            try {

                var oUpd = new eUpdater("mgr/eImportDebugManager.ashx", 1);

                //S'il y a une erreur, le message via eUpdater Suffit
                oUpd.ErrorCallBack = function () { };
                oUpd.send(function (html) {
                    var doc = document.getElementById("debugContainer");
                    if (doc)
                        doc.innerHTML = html;
                });

            } catch (e) {

                clearInterval(handle);
            }
        }, 1000);
        <% } %>
    </script>

    <div id="debugContainer" style="border: 1px solid #b4b4b4; height: 671px; width: 98%; overflow: auto; padding: 12px;">
    </div>
</body>
</html>

