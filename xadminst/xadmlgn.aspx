<%@ Import Namespace="Com.Eudonet.Xrm" %>
<%@ Import Namespace="Com.Eudonet.Internal" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="xadmlgn.aspx.cs" Inherits="Com.Eudonet.Xrm.xadminst.xadmlgn"
    EnableSessionState="true" EnableViewState="false" %>

<div class="adminModal">
    <div class="adminModalTitle">
        <div class="icon-mes_prefs"></div>
        <div><%=eResApp.GetRes(_pref, 445)%></div>
    </div>
    <ul class="adminBreadCrumbs">
        <li class="onClick"><%=eResApp.GetRes(_pref, 21)%></li>
        <li>></li>
        <li class="onClick"><%=eResApp.GetRes(_pref, 445)%></li>
        <li>></li>
        <li>Mon profil</li>
    </ul>
    <div class="adminModalMiddleTitle"><%=eResApp.GetRes(_pref, 977)%></div>
    <div class="adminCntnt">
        <div class="adminCntntTtl"><%=eResApp.GetRes(_pref, 317)%> :</div>
        <select id="lguser" runat="server">
            <option>Français</option>
        </select><div class="adminBtnPart">
            <input type="button" value="Valider" onclick="setLng()" />
        </div>
    </div>
</div>

<div class="adminMenu">
    <div class="adminBlock">
        <div class="icon-cogs2"></div>
        <div>Administrateur</div>
    </div>
    <ul class="adminSndBlock">
        <li class="firstElmentMenu">
            <div class="icon-mes_prefs"></div>
            <div><%=eResApp.GetRes(_pref, 445)%></div>
        </li>

        <li>
            <div class="icon-edn-next"></div>
            <div>Profil</div>
        </li>
        <!--
        <li>
            <div class="icon-edn-next"></div>
            <div>Affichage</div>
        </li>
        <li>
            <div class="icon-edn-next"></div>
            <div>Onglets</div>
        </li>
        <li>
            <div class="icon-edn-next"></div>
            <div>Modifier mon mot de passe</div>
        </li>
        <li>
            <div class="icon-edn-next"></div>
            <div>Ajouter/modifier ma signature</div>
        </li>
        <li>
            <div class="icon-edn-next"></div>
            <div>Ajouter/modifier un mémo</div>
        </li>
        -->
    </ul>
    <!--
    <ul class="adminThrdBlock">
        <li class="firstElmentMenu">
            <div class="icon-mes_options"></div>
            <div>Mes options avancées</div>
        </li>
        <li>
            <div class="icon-edn-next"></div>
            <div>Messagerie</div>
        </li>
        <li>
            <div class="icon-edn-next"></div>
            <div>Planning</div>
        </li>
        <li>
            <div class="icon-edn-next"></div>
            <div>Rapport d'exports</div>
        </li>
    </ul>
    -->
</div>

