<%@ Import Namespace="Com.Eudonet.Xrm" %>
<%@ Import Namespace="Com.Eudonet.Internal" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CTI.aspx.cs" Inherits="Com.Eudonet.Xrm.test.CTI"
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

    <link rel="shortcut icon" type="image/x-icon" href="<%=eLibTools.GetAppUrl(Request) %>/themes/default/images/favicon.ico" />

    <link rel="stylesheet" type="text/css" href="../themes/default/css/eMain.css?ver=<%=String.Concat(eConst.VERSION,".", eConst.REVISION) %>" />

    <script type="text/javascript" language="javascript" src="../scripts/eTools.js?ver=<%=String.Concat(eConst.VERSION,".", eConst.REVISION) %>"></script>
    <script type="text/javascript" language="javascript" src="../scripts/eMain.js?ver=<%=String.Concat(eConst.VERSION,".", eConst.REVISION) %>"></script>
    <script type="text/javascript" language="javascript" src="../scripts/eModalDialog.js?ver=<%=String.Concat(eConst.VERSION,".", eConst.REVISION) %>"></script>

    <script type="text/javascript" language="javascript">
        /* Principe de cette page d'atterrissage CTI : paramétrer le système de CTI pour qu'il l'ouvre, avec, en QueryString ("pn"), le numéro de téléphone
        appelé. Cette page va alors stocker le numéro dans l'espace de stockage local du navigateur, et un cookie indiquant qu'elle a été initialisée.
        Puis, si l'application Eudonet est ouverte, eMain.js va intercepter cette donnée, et effectuer une action en fonction.
        */

        var phoneNumber = '' + '<%=PhoneNumber%>';
        setCookie('initCTI', '1', false);

        // Inutile de vider le workingStorage si on ferme la fenêtre (source de problèmes sous IE)
        // cf. demandes #38 964, #39 400 et #55 715 pour plus de détails
        //sessionStorage.clear();
        //localStorage.clear();

        localStorage.setItem('pn', phoneNumber);

        /* #39 400, #56 495
        Fermeture automatique de la fenêtre, sous conditions et sous réserve de tolérance du navigateur
        Fonctionne sous toutes les versions d'IE antérieures à Edge (07/07/2017)
        A été progressivement interdit sous les autres navigateurs pour raisons de sécurité ;
        Un JavaScript malicieux pouvait en effet exploiter cette faille pour fermer toutes les fenêtres du navigateur ouvertes par l'utilisateur, même si celles-ci
        étaient sans aucun lien avec le JavaScript exploitant la faille.
        Mi-2015, tous les "hacks" introduits ci-dessous (notamment "open("", '_self').close();" ci-dessous avec le paramètre noclose) fonctionnaient encore avec d'autres
        navigateurs qu'IE (testé par MCR lors du traitement de la demande #39 400, et par SPH en septembre et octobre 2015).
        Sources :
        http://forums.mozillazine.org/viewtopic.php?f=38&t=2881589
        https://bugs.chromium.org/p/chromium/issues/detail?id=407507
        https://stackoverflow.com/questions/25937212/window-close-doesnt-work-scripts-may-close-only-the-windows-that-were-opene/25937289
        */

        // Pour debug
        //console.log("Numéro de téléphone stocké dans l'espace local : " + localStorage.getItem('pn'));
        //console.log("URL de la page : " + location);
        //console.log("Position de noclose dans l'URL : " + location.indexOf("noclose=1"));

        var myLoc = location;

        try {
            if (myLoc.href && myLoc.href.indexOf("noclose=1") > 0) {
                //open(location, '_self').close(); // Cette variante fonctionnait en septembre 2015, puis les navigateurs ont été patchés.
                open("", '_self').close(); // Cette variante fonctionne toujours sur IE au 07/07/2017
            } else {
                // Cette variante, bien qu'elle laisse le navigateur demander confirmatiion à l'utilisateur avant fermeture, ne fonctionne plus non plus
                // sur les navigateurs patchés (au 07/07/2017)
                window.close();
            }
        }
        catch (ex) {
            // Cette exception ne devrait jamais être levée, car à ce jour (07/07/2017), open("", '_self').close(); émet un warning, et non une exception, sous
            // Firefox et Chrome : "Scripts may close only the windows that were opened by it."
            // On laisse toutefois un try/catch pour afficher éventuellement le message si les navigateurs décident un jour de lever une exception à la place.
        }
    </script>
</head>
<body id="bodyCTI">
    <div id="errorContainer" runat="server"></div>
    <div id="ScriptContainer" runat="server"></div>

    <script language="javascript" type="text/javascript">
                var userLang = navigator.language || navigator.userLanguage;
                var messageTitle = (userLang == "fr" || userLang.indexOf("fr-") == 0) ? '<%= eResApp.GetRes(0, 6733).Replace("'", "\\'") %>' : '<%= eResApp.GetRes(1, 6733).Replace("'", "\\'") %>'; // Information
                var messageDescription = (userLang == "fr" || userLang.indexOf("fr-") == 0) ? '<%= eResApp.GetRes(0, 8226).Replace("'", "\\'") %>' : '<%= eResApp.GetRes(1, 8226).Replace("'", "\\'") %>'; // Cette fenêtre ne peut pas être fermée automatiquement...

                document.getElementById("errorContainer").innerHTML =
                    "<div class='errorTitle'><span class='icon-ban'></span><span>" + messageTitle + "</span></div>" +
                    "<span>" + messageDescription + "</span>";
        </script>
</body>
</html>
