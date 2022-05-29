<%@ Import Namespace="Com.Eudonet.Xrm" %>
<%@ Import Namespace="Com.Eudonet.Internal" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="frm.aspx.cs" Inherits="Com.Eudonet.Xrm.frm"
    EnableSessionState="true" EnableViewState="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 //EN">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title><%=PageTitle%></title>
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <meta http-equiv="content-type" content="application/xhtml+xml; charset=UTF-8" />
    <meta name="robots" content="noindex">
    <meta name="googlebot" content="noindex">
    <asp:PlaceHolder runat="server" ID="MetaSocialNetworksPlaceHolder"></asp:PlaceHolder>
    <asp:PlaceHolder runat="server" ID="MetaPlaceHolder"></asp:PlaceHolder>

    <asp:PlaceHolder runat="server" ID="CustomPlaceHolder"></asp:PlaceHolder>

    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>

    <link rel="shortcut icon" type="image/x-icon" href="themes/<%=DefaultTheme.Folder %>/images/favicon.ico" />
    <%if (!FormAdv2) {%>
    <link rel="stylesheet" type="text/css" href="themes/<%=DefaultTheme.Folder %>/css/eFormular.css?ver=<%=String.Concat(eConst.VERSION,".", eConst.REVISION) %>" />
    <%} %>

    <%if (IsPublic && FormAdv2){%>
    <%--On ajoute google Captcha si le formulaie est publique--%>
    <script  type="text/javascript"  src="https://www.google.com/recaptcha/api.js?hl=<%=CaptchaFormularLanguage%>" ></script>
         <%} %>
    <script type="text/javascript">
        function InitForm() {
            var input = document.getElementById("re");

            if (typeof (LoadRes) == 'function')
                LoadRes();

            //afficher une notification
            var snackbar = document.getElementById("snackbar");
            if (snackbar && snackbar.innerHTML.length > 0) {
                snackbar.classList.add("show");
                setTimeout(function(){ snackbar.classList.remove("show"); }, 6000);
                document.getElementById('messageCloseIcon').onclick = function () {
                    snackbar.classList.remove("show");
                }
            }

            //Dans le cas où c'est un formulaire avancé, on ajoute la gestion du captcha
            var _formularTypeElem = document.getElementById("frmType");
            if (_formularTypeElem && _formularTypeElem.value == "1" && eUserFormAdv) {

                eUserFormAdv.InitFormularAdvCaptcha('<%=CaptchaSiteKey%>', '<%=CaptchaFormularLanguage%>');
                //Si le formulaire est intégré dans un iframe, on envoie la taille du formulaire pour ajuster la taille de l'iframe                
                if (parent && typeof (parent.postMessage) == "function") {

                    //utilise de préférence le document.getElementById("ContainerPanel")
                    var nHeightResize = document.body.clientHeight
                    var p = document.getElementById("ContainerPanel");
                    if (p)
                        nHeightResize = p.clientHeight

                    parent.postMessage({
                        action: "setHeight",
                        value: nHeightResize
                    }, "*");
                }
            }
        }
    </script>
</head>
<%if (FormAdv2) {%>
  <body onload="InitForm();" class="bodyFormularAdv">
<%} else{%>
  <body onload="InitForm();" class="bodyFormular">
<%} %>
    <input type="file" name="FileToUpload" id="FileToUpload" accept="image/jpeg,image/gif,image/png" onchange="return false;" style="position: absolute; left: -2000em;" />
    <div id="ContainerPanel" class="ednGlobalPnl">
        <%
            if (RendType == ExternalPageRendType.ERROR)
            {
        %>
        <div id="ErrPanel" runat="server">
            <%= _panelErrorMsg %>
        </div>
        <%
            }
            else
            {
        %>

          <%if (!FormAdv2){%>
        <form id="userForm" method="post" onsubmit="return eUserForm.SubmitForm();">
         <%} %>

            <div id="waiter" class="waitOff">
            </div>
            <div class="contentWait" id="contentWait" style="display: none;">
                <br />
                <img alt="wait" src="themes/<%= DefaultTheme.Folder %>/images/wait.gif" /><br />
                &nbsp;&nbsp;<br />
                <br />
            </div>
            <div id="FormPanel" runat="server"></div>
            <div id="ContextPanel" style="display: none;" runat="server"></div>

  <%if (!FormAdv2){%>
        </form>
  <%} %>

        <%
            }
        %>
    </div>

    <div id="ScriptContainer" runat="server"></div>
</body>
</html>
