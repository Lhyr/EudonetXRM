<%@ Import Namespace="Com.Eudonet.Xrm" %>
<%@ Import Namespace="Com.Eudonet.Internal" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eForgotPassword.aspx.cs" Inherits=" Com.Eudonet.Xrm.eForgotPassword" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title></title>
    <link rel="stylesheet" type="text/css" href="themes/default/Css/eForgotPwd.css?ver=<%=String.Concat(eConst.VERSION,".", eConst.REVISION) %>" />
    <link rel="stylesheet" type="text/css" href="themes/default/Css/eButtons.css?ver=<%=String.Concat(eConst.VERSION,".", eConst.REVISION) %>" />
    <link rel="stylesheet" type="text/css" href="themes/default/Css/eModalDialog.css?ver=<%=String.Concat(eConst.VERSION,".", eConst.REVISION) %>" />
    <script type="text/javascript" src="scripts/eTools.js?ver=<%=String.Concat(eConst.VERSION,".", eConst.REVISION) %>"></script>
    <script type="text/javascript" src="scripts/eUpdater.js?ver=<%=String.Concat(eConst.VERSION,".", eConst.REVISION) %>"></script>
    <script type="text/javascript" src="scripts/eModalDialog.js?ver=<%=String.Concat(eConst.VERSION,".", eConst.REVISION) %>"></script>
    <script type="text/javascript" src="scripts/eForgotPassword.js?ver=<%=String.Concat(eConst.VERSION,".", eConst.REVISION) %>"></script>
    <script type="text/javascript" src="mgr/eResManager.ashx?l=<%= iLang%>&ver=<%=String.Concat(eConst.VERSION,".", eConst.REVISION) %>"></script>
</head>
<body onload="<%=_onLoadBody%>">
    <div class="waiter" id="ImgLoading" style="display: none;">
        <div class="contentWait" id="contentWait">
            <br />
            <img alt="wait" src="themes/default/images/wait.gif" /><br />
            <br />
            <br />
        </div>
    </div>
    <div id="panelForm" runat="server">
        <br />
        <div class="MainDiv" id="MainDiv" edntype="main" runat="server">
            <div class="chapeau" id="chapeau"></div>
            <div class="mail">
                <span id="mail_err_lbl" class="error" style="display: none;"></span>
                <span id="mail_lbl" class="text-mail"></span>
                <input id="txt_mail" class="input-mail" />
            </div>
            <div class="captcha">

                <img id="ImgCapcha" style="display: none;" class="img-captcha" alt="Captcha" src="blank.png" onload="onCaptchaLoad();" />
                <div id="captcha_lbl" class="text-captcha"></div>
                <input id="txt_captcha" class="inpt-captcha" />
            </div>
            <div class="options">
                <div class="up" onclick="reloadCaptcha();">
                    <div class="logo_reload"></div>
                    <span id="text-reload" class="text-reload"></span>
                </div>
                <div id="resdown" class="down">
                    <div class="logo_help"></div>
                    <span class="text-help" id="text-help"></span>
                </div>
            </div>
        </div>
        <form name="value" id="forgotValue" runat="server" style="display: none">
            <input name="dbt" id="dbt" runat="server" />
            <input name="st" id="st" runat="server" />
            <input name="userlogin" id="userlogin" runat="server" />
            <input name="lang" id="lang" runat="server" />
        </form>
    </div>
</body>
</html>
