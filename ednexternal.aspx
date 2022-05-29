<%@ Import Namespace="Com.Eudonet.Xrm" %>
<%@ Import Namespace="Com.Eudonet.Internal" %>


<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ednexternal.aspx.cs" Inherits="Com.Eudonet.Xrm.ednexternal" %>


<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 //EN">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title><%=PageTitle%></title>
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <meta http-equiv="content-type" content="application/xhtml+xml; charset=UTF-8" />
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>

    <link rel="shortcut icon" type="image/x-icon" href="<%=eLibTools.GetAppUrl(Request) %>/themes/<%=DefaultTheme.Folder %>/images/favicon.ico" />    
    <link rel="stylesheet" type="text/css" href="<%=eLibTools.GetAppUrl(Request) %>/themes/<%=DefaultTheme.Folder %>/css/eTrack.css?ver=<%=String.Concat(eConst.VERSION,".", eConst.REVISION) %>" />

    <script type="text/javascript" lang="javascript">
        function UnsubMail() {
            var myForm = document.getElementById("unsubForm");
            myForm.submit();
        }
    </script>

    <style>
        <%--=_css--%>
    </style>
</head>
<body>
    <div id="globalPnl" class="ednGlobalPnl">
        <%
            if (RendType == ExternalPageRendType.TRACK_VISU)
            {
        %>
        <div id="visuPnl" class="ednVisuPnl" runat="server"></div>
        <%
            }
            else if (RendType == ExternalPageRendType.TRACK_UNSUB_CHOICE || RendType == ExternalPageRendType.TRACK_UNSUB_VALID)
            {
        %>
        <div id="unsubPnl" class="ednUnsubPnl">
            <p id="unsubTitle" class="ednUnsubTitle">Confirmation de désinscription.</p>
            <%
                if (RendType == ExternalPageRendType.TRACK_UNSUB_CHOICE)
                {
            %>
            <form id="unsubForm" method="post">
                <div id="unsubMsg" class="ednUnsubMsg" runat="server"></div>
                <div id="unsubChoice" class="ednUnsubChoice" runat="server"></div>
                <div class="button-green-edn" onclick="javascript:UnsubMail();">
                    <div class="button-green-edn-left"></div>
                    <div class="button-green-edn-mid"><%=eResApp.GetRes(0, 1787)%></div>
                    <div class="button-green-edn-right"></div>
                </div>
            </form>
            <%--<p class="dotHr"></p>
            <div class="ednUnsubClient">
                La base de contacts utilisée est gérée sous la responsabilité de l'émetteur du message :
                <div>
                    <p>{CLIENT NAME}</p>
                    <p>{CLIENT TEL}</p>
                    <p>{CLIENT FAX}</p>
                </div>
            </div>
            <div class="btmText">
            <p>
                Ce message a été routé par l'application <a href="http://www.eudonet.com">Eudonet</a>, 
                prestataire technique qui met à la disposition de ses clients et annonceurs une plate-forme 
                leur permettant de gérer leurs campagnes de communication auprès de leurs propres bases.
            </p>
            <p>
                Toutes les bases utilisées par la plate-forme sont de la responsabilité de nos clients/annonceurs.
            </p>
            <p>
                Si vous pensez que l'un de nos clients/annonceurs ne respecte pas l'ensemble des obligations légales et réglementaires applicables, 
            ou si vous pensez que ce message vous a été envoyé de façon abusive ou illégale merci de nous le faire savoir : 
            écrivez à <a href="mailto:abuse@eudonet.com">abuse@eudonet.com</a> en transférant l'intégralité du message original que vous avez reçu.
            </p>
            <p>
                Nous vous invitons également à consulter notre <a href="http://www.eudoweb.com/antispam-mode-demploi.html">charte de déontologie et anti-spam</a>.
            </p>--%>
        </div>
        <%
                }
                else
                {
        %>
        <div id="unsubValid" class="ednUnsubValid" runat="server"></div>
        <%
                }
        %>
    </div>
    <%
            }
            else if (RendType == ExternalPageRendType.ERROR)
            {
    %>
    <div id="errPnl" class="ednErrPnl" runat="server">
        <%= _panelErrorMsg %>
    </div>
    <%
            }
    %>
    </div>
</body>
</html>
