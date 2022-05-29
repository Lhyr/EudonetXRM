<%@ Import Namespace="Com.Eudonet.Xrm" %>
<%@ Import Namespace="Com.Eudonet.Internal" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eLogin.aspx.cs" Inherits="Com.Eudonet.Xrm.eLogin"
    EnableSessionState="true" EnableViewState="false" ValidateRequest="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" runat="server" id="htmltag">
<head>
    <title><%=eResApp.GetRes(_nLangServ, 61)%> | Eudonet CRM</title>
    <meta name="description" content="<%=eResApp.GetRes(_nLangServ, 2338)%>" />

    <meta http-equiv="Expires" content="0" />
    <meta http-equiv="Cache-Control" content="no-cache" />
    <meta http-equiv="Pragma" content="no-cache" />
    <meta name="viewport" content="width=device-width, initial-scale=0.8, shrink-to-fit=no">
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <%if (IsLocal)
        { %>
    <link rel="shortcut icon" type="image/x-icon" href="themes/default/images/faviconLocal.ico" />
    <%
        }
        else
        {
    %>
    <link rel="shortcut icon" type="image/x-icon" href="themes/default/images/favicon.ico" />
    <%} %>

    <link rel="stylesheet" type="text/css" href="themes/default/css/eLogin.css?ver=<%=String.Concat(eConst.VERSION, ".", eConst.REVISION) %>" />
    <link rel="stylesheet" type="text/css" href="themes/default/css/eModalDialog.css?ver=<%=String.Concat(eConst.VERSION, ".", eConst.REVISION) %>" />
    <link rel="stylesheet" type="text/css" href="themes/default/css/eButtons.css?ver=<%=String.Concat(eConst.VERSION, ".", eConst.REVISION) %>" />
    <link rel="stylesheet" type="text/css" href="themes/default/css/eControl.css?ver=<%=String.Concat(eConst.VERSION, ".", eConst.REVISION) %>" />
    <link rel="stylesheet" type="text/css" href="themes/default/css/eudoFont.css?ver=<%=String.Concat(eConst.VERSION, ".", eConst.REVISION) %>" />
   

    <script type="text/javascript" language="javascript" src="scripts/eEvent.js?ver=<%=String.Concat(eConst.VERSION, ".", eConst.REVISION) %>"></script>
    <script type="text/javascript" language="javascript" src="scripts/eTools.js?ver=<%=String.Concat(eConst.VERSION, ".", eConst.REVISION) %>"></script>
    <script type="text/javascript" language="javascript" src="scripts/eUpdater.js?ver=<%=String.Concat(eConst.VERSION, ".", eConst.REVISION) %>"></script>
    <script type="text/javascript" language="javascript" src="scripts/eUserOptions.js?ver=<%=String.Concat(eConst.VERSION, ".", eConst.REVISION) %>"></script>
    <script type="text/javascript" language="javascript" src="scripts/eLogin.js?ver=<%=String.Concat(eConst.VERSION, ".", eConst.REVISION) %>"></script>
    <script type="text/javascript" language="javascript" src="scripts/eModalDialog.js?ver=<%=String.Concat(eConst.VERSION, ".", eConst.REVISION) %>"></script>
    <script type="text/javascript" language="javascript" src="mgr/eResManager.ashx?l=<%=_nLangServ %>&ver=<%=String.Concat(eConst.VERSION, ".", eConst.REVISION) %>"></script>
</head>
<body onload="OnLoad();">
    <div class="waiter" id="ImgLoading" style="display: none;">
        <div class="contentWait" id="contentWait">
            <br />
            <img alt="wait" src="themes/default/images/wait.gif" /><br />
            <br />
            <br />
        </div>
    </div>
    <!-- #container -->
    <div id="container">
        <!-- #container-info -->
        <div id="container-info">
            <!-- #header -->
            <div id="header" class="header">
                <div class="hLogo"></div>
                <div class="espabo">
                    <span id="txtWelcomeTo" runat="server"></span>
                    <br />
                    <span id="txtLoginSpace" runat="server"></span>
                </div>
                <!--<div class="espaversion"><span>La version Eudonet XRM 10.211.000 sera disponible à partir du xxxx xx xxxx xxxx. </span></div>-->
            </div>
            <!-- header -->

            <div id="container-connect">
                <div id="divLang" class="divLang" runat="server"></div>

                <!-- #info -->
                <div id="info">
                    <div id="titleLogin">
                        <span class="icon-lock2"></span><span class="title-box" id="spConnexion" runat="server"></span>
                    </div>
                    <div class="formContainer">
                        <div id="errGlobal" runat="server" style="display: none">
                            <label runat="server" class="errLogin" id="labelErrGlobal">
                            </label>
                        </div>
                        <div id="errSub" style="display: none">
                            <label class="errLogin" id="txtLogingSubscriberErr">
                                <b>
                                    <%=eResApp.GetRes(_nLangServ, 6210)%></b></label>
                        </div> 
                        <div class="fieldRow" <%=_visIntranetStyle%>>
                            <label for="txtLoginSubscriber">
                                <%=eResApp.GetRes(_nLangServ, 315)%></label>
                            <input oldcss="" class="inputText inputEdit" type="text" name="txtLoginSubscriber"
                                size="45" value="<%=_defSubscriberLogin%>" onchange="authSubscriber(true);" id="txtLoginSubscriber"
                                onkeypress="KeyPress(event);" tabindex="1" />
                        </div>
                        <!-- nom abonné  -->
                        <div class="fieldRow" <%=_visIntranetStyle%>>
                            <label for="txtPasswordSubscriber">
                                <%=eResApp.GetRes(_nLangServ, 316)%></label>
                            <input oldcss="" size="45" class="inputText  inputEdit" type="password" value="<%=_defSubscriberPassword%>"
                                onblur="authSubscriber(false);" name="txtPasswordSubscriber" id="txtPasswordSubscriber"
                                onkeypress="KeyPress(event);" tabindex="2" />
                        </div>
                        <!-- mot de passe abonné  -->
                        <div class="fieldRow">
                            <label for="cboBase">
                                <%=eResApp.GetRes(_nLangServ, 320)%></label>
                            <select oldcss="" onchange="onChangeDbList(true);" name="cboBase" id="cboBase" class="inputText  inputEdit"
                                tabindex="3">
                            </select>
                            <input oldcss="" onfocus="activateDbList();" class="inputText  inputEdit" type="text"
                                name="txtDatabase" id="txtDatabase" style="display: none;" tabindex="4" />
                        </div>
                        <!-- base  -->
                        <div class="sep">
                        </div>
                        <div id="globalAuthnBlock" mode="form">
                            <div id="userAuthnBlock" >
                                <div id="errUser" style="display: none">
                                    <label class="errLogin" id="labelUserEr">
                                        <%=eResApp.GetRes(_nLangServ, 5)%></label>
                                </div>
                                <div class="fieldRow" <%=_visuUser %>>
                                    <label for="txtUserLogin">
                                        <%=eResApp.GetRes(_nLangServ, 691)%></label>
                                    <input oldcss="" class="inputText  inputEdit" type="text" onkeypress="KeyPress(event);"
                                        name="txtUserLogin" onfocus="activateUserList();" id="txtUserLogin" value="<%=_defUserLogin %>"
                                        tabindex="5" />
                                    <select oldcss="" onchange="onChangeUserList();" id="UserLoginList" name="UserLoginList"
                                        class="inputText  inputEdit" style="display: none;" tabindex="6">
                                    </select>
                                </div>
                                <!-- utilisateur  -->
                                <div class="fieldRow passwordField" <%=_visuUser %>>
                                    <label for="txtUserPassword">
                                        <%=eResApp.GetRes(_nLangServ, 316)%></label>
                                    <input oldcss="" class="inputText  inputEdit" type="password" name="txtUserPassword"
                                        id="txtUserPassword" onkeypress="KeyPress(event);" tabindex="7" />
                                    <input oldcss="" class="inputText  inputEdit" type="text" name="txtUserPassword"
                                        id="txtUserPasswordBlock" onkeypress="KeyPress(event);" onchange="setPassWord(event)" tabindex="7" style="display:none"/> 
                                    <i id="pass-status" class="icon-edn-eye" onclick="viewPassword(event)"></i>
                                    
                                </div>
                                <div id="btnForgotPassword">
                                    <a class="fieldAction" onclick="forgotPassword();" tabindex="9">
                                        <%=eResApp.GetRes(_nLangServ, 6096)%></a>
                                </div>
                                <div runat="server" id="divRememberMe" tabindex="10"/>                                
                                <div class="greenBtn" id="btnConnect">
                                    <div onclick="authUser();" tabindex="8">
                                        <%=eResApp.GetRes(_nLangServ, 5004)%>
                                    </div>

                                </div>
                            </div>
                            <div id="samlAuthnBlock" class="greenBtn" title="Authentification SAML2">
                                <div onclick="authSaml2();" tabindex="8"> <%=eResApp.GetRes(_nLangServ, 5004)%></div>
                            </div>
                        </div>
                        <!-- bouton se connecter -->
                    </div>
                    <!-- formContainer -->
                </div>
                <!-- info -->

                <div id="footerAskDemo" runat="server" style="display: none">
                </div>


            </div>

            <!-- #footer -->
            <div id="footerDiv" class="footerDiv" style="display: none">

                <p class="eudonet-links">
                    <%=eResApp.GetRes(_nLangServ, 6209)%> : <a href="http://www.eudonet.fr" target="_blank">www.eudonet.fr</a><br>
                    <%=eResApp.GetRes(_nLangServ, 6208)%> : <a href="http://www.eudonet.ca" target="_blank">www.eudonet.ca</a><br>
                    <%=eResApp.GetRes(_nLangServ, 1825)%> : <a href="http://www.eudonet.co.uk" target="_blank">www.eudonet.co.uk</a><br>
                    <%=eResApp.GetRes(_nLangServ, 6207).Replace("<YEAR_MIN>", "2000").Replace("<YEAR_MAX>", DateTime.Now.Year.ToString())%>
                </p>
            </div>
            <!-- footer -->

            <div id="alertMessage">
                <span id="messageIcon" class="icon-exclamation-circle"></span>
 

            </div>
        </div>
        <!-- container-info -->

        <!-- #pub -->
        <div id="pubDiv" class="pubDiv" runat="server">
        </div>

        <!-- <div id="slogan" runat="server"></div> -->
    </div>
    <!-- container -->
    <input type="hidden" id="langue" name="langue" value="<%=_langueServ %>" />
    <input type="hidden" id="RememberMe" value="<%=_rememberMe %>" />
    <input type="hidden" id="IsIntranet" value="<%=_isIntranetValue %>" />
    <input type="hidden" id="SubscriberToken" runat="server" />
    <input type="hidden" id="AutoConnect" runat="server" />
    <input type="hidden" id="SubscriberTokenFR" runat="server" />
    <input type="hidden" id="UserTokenFR" runat="server" />
    <input type="hidden" id="DBTokenFR" runat="server" />
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>
</body>
</html>
