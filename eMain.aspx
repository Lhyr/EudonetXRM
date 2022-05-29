<%@ Import Namespace="Com.Eudonet.Xrm" %>
<%@ Import Namespace="Com.Eudonet.Internal" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eMain.aspx.cs" Inherits="Com.Eudonet.Xrm.eMain"
    EnableSessionState="true" EnableViewState="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 //EN">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title><%=Titre %></title>
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
    <meta http-equiv="content-type" content="application/xhtml+xml; charset=UTF-8" />
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>
    <!--
    Activer cette balise meta pour désactiver le zoom sur tablettes
    <meta name="viewport" content="width=device-width, initial-scale=1.0, user-scalable=no, minimum-scale=1.0, maximum-scale=1.0">
    -->
    <meta name="apple-mobile-web-app-capable" content="yes">
    <meta name="apple-mobile-web-app-status-bar-style" content="black">
    
    <%if (!IsLocal)
      { %>
    <link rel="shortcut icon" type="image/x-icon" href="themes/<%=_pref.ThemePaths.GetImageWebPath("/images/faviconLocal.ico")%>" />
    <%
      }
      else
      { 
    %>
    <link rel="shortcut icon" type="image/x-icon" href="themes/<%=_pref.ThemePaths.GetImageWebPath("/images/favicon.ico")%>" />
    <%} %>
    <link rel="apple-touch-icon" href="themes/<%=_pref.ThemePaths.GetImageWebPath("/images/iPad-icon.png")%>">


    <style type="text/css" id="customCss" title="customCss">
      <% if (!String.IsNullOrEmpty(LogoName))
        {
            //demande 38 142: KHA :  Prise en compte du logo paramétré en V7
        %>
        .hLogo{
            background-image: url("<%=LogoWebPath %>"); 
        }
        <%          
        }
        else
        {
            %>
        .hLogo{
            background-image: url(themes/<%=_pref.ThemePaths.GetImageWebPath("/images/emain_logo.png")%>); 
        }
        <%  
            
        }
      %>


    </style>
    <script type="text/javascript" src="mgr/eResManager.ashx?l=<%=_pref.LangServId %>&ver=<%=String.Concat(eConst.VERSION,".", eConst.REVISION) %>"></script>
</head>
<body ondrop="return false;" onresize="resizeNavBar();" ondragover="return false;" ondragleave="return false;" onunload="doUnload();">

    <script type="text/javascript">
        
        // MEMORISATION D'INFORMATIONS DE CONTEXTE POUVANT ETRE MODIFIEES EN JAVASCRIPT

        // Booléens déclarés dans eTools.js si non déclarés auparavant
        var sTheme = '<%=_pref.ThemeXRM.Folder%>'; // Thème actuellement utilisé

        // Booléens déclarés en haut d'eMain.js, d'où l'absence de var
        nThemeId = getNumber(<%=_pref.ThemeXRM.Id%>); // Identifiant du thème actuellement utilisé
        bDefaultThemeForced = false; // Indique si le thème par défaut est appliqué temporairement, sans tenir compte du paramétrage utilisateur (ex : passage en Administration)
        nGlobalCurrentUserid = getNumber('<%=_pref.UserId.ToString()%>'); // Identifiant de l'utilisateur actuellement connecté
        
        //todo : gérer la CONST en c#
        var nNbrMaxAdvancedUsr = 3;  // En mode liste, nombre de filtre users avancé sauvegardés
               

        //La gestion de la session se fait en partie en JS   
        oSession.Init({'timeout' : <%= Session.Timeout %> , 'debug':'<%= DEBUG %>' });  //timeout en minutes

       
   
         


        //Affichage des informations de versions
        function About() {
            <%=sJsAbout%>
        }
        //Pour la modal de version, permet de masquer/afficher les valeurs
        function DisplayAndHide(oToHide) {
            var oToDisplay = document.getElementById(oToHide.getAttribute("eDisp"));
            oToHide.style.display = "none";
            oToDisplay.style.display = "inline";
        }
        
        var ctiSpecifId = 0;

        <%if (IsSpecifCtiEnabled)
          {
              //demande 41250: MCR/RMA :  pour la CCI77 set d un listener pour lancer une specif CTI
        %>
        ctiSpecifId = <%=CtiSpecifId%>   ;
        <%          
          }
    %>

        //localStorage.clear();
        localStorage.setItem('pn', 'eudonet');
        setWindowEventListener("storage", handlerCTI);
        
        

    </script>


    <!-- EN-TETE -->
    <div id="imStillHere"></div>
    <div id="en-tete" class="header">
        <div class="hLogo" onclick="About();">
        </div>

 
        <!-- BOUTONS ACTION RAPIDE  -->
        <div class="hQuickActions">
            <ul class="hQuickActionsMenu">
                <li><a class="sprite-societes" href="#" title="<%=eResApp.GetRes(_pref, 6182) %>"></a></li>
                <li><a class="sprite-contact" href="#" title="<%=eResApp.GetRes(_pref, 6183) %>"></a></li>
                <li><a class="sprite-email" href="#" title="<%=eResApp.GetRes(_pref, 6184) %>"></a></li>
                <li><a class="sprite-planning" href="#" title="<%=eResApp.GetRes(_pref, 6185) %>"></a></li>
                <li><a class="sprite-taches" href="#" title="<%=eResApp.GetRes(_pref, 6186) %>"></a></li>
                <li><a class="sprite-notes" href="#" title="ACTION A DEFINIR 1"></a></li>
                <li><a class="sprite-chat" href="#" title="ACTION A DEFINIR 2"></a></li>
            </ul>
        </div>
        <!-- FIN ACTION RAPIDE -->
    </div>
    <!-- FIN ENTETE -->
    <div id="waiter" class="waitOn">
    </div>
    <!-- DEBUT NAVBAR -->
    <div id="globalNav" class="globalNav" runat="server">
    </div>
    <!-- FIN NAVBAR -->
    <div id="container" class="contentMaster"  runat="server">

        <div class="contentWait" id="contentWait">
            <br />
            <img alt="wait" src="themes/default/images/wait.gif" /><br />
            &nbsp;&nbsp;<br />
            <br />
        </div>
        
        <!-- CONTENU -->
        <asp:Panel ID="mainDiv" CssClass="mainDiv mainDivWidth" runat="server">
       
            <!-- START CONTENTEUR PRINCIPALE DES FICHES/LISTES -->
            Chargement...
            <!-- END CONTENTEUR PRINCIPALE DES FICHES/LISTES -->
        </asp:Panel>
        <!-- END CONTENU -->
        <!-- MENU DROITE -->
        <!-- CE MENU DEVRA ETRE GENERE DYNAMIQUEMENT EN FONCTION DU MODE D'AFFICHAGE (LISTE/FICHE/ACCUEIL -->
      
        <div id="rightMenu"  class="rightMenu"> </div>
        <eEudoCtrlUsr:ListSklton runat="server" />
        <eEudoCtrlUsr:FileSklton runat="server" />

        <!-- NOTIFICATIONS TOASTERS -->
      
        <div id="divNotifToastersWrapper">
        </div>
        <script>
            //lance les notifications
            notifToastCallLauncher();
        </script>
       
        <!-- FIN NOTIFICATIONS TOASTERS -->
        <!-- NOTIFICATIONS LIST --> 
        <div id="notifListWrapper" class="notifListHide notifListAnimHide">
            <div id="notifArrow1"></div>
            <div id="notifArrow2"></div>
            <div id="notifWrapper">
                <div id="notifHeader">
                    <span>
                        <a id="notifLinkShowUnread" onclick="notifShowUnread();"><%=eResApp.GetRes(_pref, 6874) %></a>
                    </span>
                    <span>
                        <a id="notifLinkShow" onclick="notifShow();"><%=eResApp.GetRes(_pref, 6875) %></a>
                    </span>
                    <span class="notifHeaderLinkTagAll">
                        <a id="notifLinkTagAllRead" onclick="notifTagAllRead();"><%=eResApp.GetRes(_pref, 6876) %></a>
                    </span>
                    <span class="icon-edn-cross notifHeaderClose" onclick="notifListToggle(event);" title="<%=eResApp.GetRes(_pref, 30) %>"></span>
                </div>
                <div id="notifContents" onscroll="notifScroll();">
                </div>
            </div>
        </div>       
        <!-- FIN NOTIFICATIONS LIST -->

    </div>
    <!-- SCRIPT DE LOADING -->
    <script lang="javascript" type="text/javascript">
        <%if (AppLoadingLog.Length > 0)
        {%>
        <%=AppLoadingLog%>
        <%}%>

        // Lancement des scripts de chargement
        loadNavBar();
        //Lance menu accueil (1er chargement de la page)
        //goTabList(<%= DefaultTab %>);
        // Chargement des scripts pour tablettes
        if (typeof (setTabletScripts) == 'function') {
            setTabletScripts();
        }

    </script>
    <!-- FIN SCRIPT LOADING -->
    <!-- IFRAME DES PARAM -->
    <script type="text/javascript" lang="javascript">
        /*GCH : commenté suite à demande RMA/CMO 31012013*/
        /*MOU : décommenté suite à demande RMA/CMO  cf. 23835 */
        setWait(true);

    </script>
    <iframe id="eParam" src="eParamIframe.aspx" width="1000" height="400" style="visibility: hidden; display: none;" onload="eParamOnLoad()"></iframe>

    <!-- IFRAME DES PARAM -->

    <script type="text/javascript">
            <%=_jsFileRedir %>
    </script>

   <% if(CanRunCartography())
       { %>
    <!--  CARTOGRAPHY   -->
    <div id="carto-container" class="carto-container" style="display: none;">
        <div id="maps-container" class="maps-container" onclick="oCartography.onClick(event);">
            <div id="maps-open" class="maps-open" state="close">
                <div id="first-maps-btn" title="<%=eResApp.GetRes(_pref, 7487) %>" class="icon-map-marker background-theme"></div>
                <div id="second-maps-btn" title="<%=eResApp.GetRes(_pref, 7488) %>" class="icon-times background-theme" style="display: none;"></div>
                <div id="refresh-maps-btn" title="<%=eResApp.GetRes(_pref, 7489) %>" class="icon-refresh background-theme"></div>
                <div id="edit-maps-btn" title="<%=eResApp.GetRes(_pref, 7490) %>" mode="0" class="icon-crop background-theme"></div>
                <div id="adv-maps-btn" active="0" class="icon-filter2 background-theme">
                    <ul id="filter-maps-menu" class="filter-maps-menu">
                        <li id="maps-item-page" class="background-hover-theme"><%=eResApp.GetRes(_pref, 7098) %></li>
                        <li id="maps-item-xxx" class="background-hover-theme"><%=eResApp.GetRes(_pref, 7099).Replace("<NB_MAX>", eConst.MAP_NB_MAX_FILES.ToString()) %></li>
                      <!-- <li id="maps-item-all" class="background-hover-theme"><%=eResApp.GetRes(_pref, 7100) %></li> -->
                      <!--   <li id="maps-item-search" class="background-theme"><%=eResApp.GetRes(_pref, 7101) %></li> -->
                    </ul>
                </div>
                <div id="export-maps-btn" class="icon-export background-theme"></div>
            </div>
            <div id="maps-provider" class="maps-provider default-cursor"></div>
        </div>       
    </div>        
    <!--  FIN CARTOGRAPHY   -->
    <% } %>

    <% if (CanRunBingAutoSuggest())
        { %>
        <script type='text/javascript' src='https://www.bing.com/api/maps/mapcontrol' async defer></script>
        <input type="hidden" id="accessKey" runat="server" />
    <% } %>
   <script type="text/javascript">
       // On câble le chargement des scripts/CSS IRIS à la fin du chargement du système de grilles, mais uniquement sur les autres navigateurs que IE
       // Ainsi que le chargement des polices
       var brow = new getBrowser();
       if (!brow.isIE) {
           var onReadyFct = function () {
               // Chargement des scripts/CSS IRIS
               var loc = window.location.pathname;
               var dir = loc.substring(0, loc.lastIndexOf('/'));
               addScript("../IRISBlack/Front/Scripts/eInitNewErgo", "FILE", function () {
                   LoadIrisJSCSS(dir + "/IRISBlack/Front/Scripts/");
               });
           };
           GridSystem.onReady(onReadyFct);
       }

       // Démarrage du système de grilles
       GridSystem.start();
       <%

       if (bThemeIncompat)
       {
       %>
       eAlert(3, top._res_6733, "<%=sMsgErrorCourtNav%>", "<%=sbMgErrorNav.ToString()%>");
       <%
        }
       %>
   </script>    
</body>
</html>

 
