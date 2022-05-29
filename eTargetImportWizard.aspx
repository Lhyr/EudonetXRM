<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eTargetImportWizard.aspx.cs"
    Inherits="Com.Eudonet.Xrm.eTargetImportWizard" %>

<%@ Import Namespace="Com.Eudonet.Internal" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title></title>
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>


</head>
<body onload="StepClick(1)" id="importWizard">
    <div class="MainDivWizard" is="MainDivWizard">
        <!--VOLET ETAPES ASSISTANT-->
        <center>
            <div class="states_placement">
                <div onclick="StepClick(1);" class="state_grp-current" id="step_1">
                    <div id="txtnum_1">
                        1
                    </div>
                    <span><%= eResApp.GetRes(_pref.Lang,6713)%></span>
                </div>
                <div class="state_sep">
                </div>
                <div onclick="StepClick(2);" class="state_grp" id="step_2">
                    <div id="txtnum_2">
                        2
                    </div>
                    <span><%= eResApp.GetRes(_pref.Lang,6343)%></span>
                </div>
                <div class="state_sep">
                </div>
                <div onclick="StepClick(3);" class="state_grp" id="step_3">
                    <div id="txtnum_3">
                        3
                    </div>
                    <span><%= eResApp.GetRes(_pref.Lang,6344)%></span>
                </div>
            </div>
        </center>
        <!--FIN VOLET ETAPES ASSISTANT-->
        <!--VOLET ETAPE 1-->
        <div id="step1" class="import-step">
            <div class="fieldWrapper">
                <input onclick="setDatasSrc();" type="radio" checked="checked" name="DataSource" value="0" id="srcFile" />
                <label for="srcFile">
                    <%=eResApp.GetRes(_pref.Lang,1114) %>
                    (.csv, .txt)</label>
            </div>

            <div id="FromFile" class="import-step">
                <form target="iframeupload" action="mgr/eTargetProcessManager.ashx" name="frmupload"
                    id="frmupload" enctype="multipart/form-data" method="post">
                    <input type="hidden" id="action" name="action" value="2" />

                    <!--ELAIZ/GL - demande 76 825 : condition pour afficher
                         le champs custom uniquement sur eudonet x--> 
                    <% if (_pref.ThemeXRM.Version < 2) { %>
                        <input type="file" id="filesrc" name="filesrc" onchange="doUpload();" />
                    <% }
                    else { %>
                        <!--ELAIZ : Ajout du champs fichier customisé eudonet x--> 
                        <div class="label-container">        
                            <input type="file" id="filesrc" <% if (_pref.ThemeXRM.Version > 1) { Response.Write("style=\"display:none\""); } %> name="filesrc" onchange="doUpload();" />
                            <span onclick="document.querySelector('#filesrc').click()" id="avatarName"><%= eResApp.GetRes(_pref, 1114) %></span>
                            <label for="filesrc"><%= eResApp.GetRes(_pref, 6498) %></label>  
                        </div>
                     <%} %>
                    <!--<input type="submit" value="Envoyer" />-->
                </form>
                <iframe id="iframeupload" name="iframeupload" style="display: none" onload="setWait(false)"></iframe>
            </div>

            <div class="fieldWrapper">
                <input onchange="setDatasSrc();" type="radio" name="DataSource" value="1" id="srcClipboard" />
                <label for="srcClipboard"><%=eResApp.GetRes(_pref.Lang, 6345)%></label>
            </div>

            <div id="FromClipboar">
                 <!--ELAIZ : on masque le textarea en inline pour empêcher les transitions CSS de fonctionner à l'ouverture--> 
                <textarea <% if (_pref.ThemeXRM.Version > 1) { Response.Write("style=\"display:none\""); } %> rows="10" id="DatasClipboard" cols="150"></textarea>
            </div>

            <div class="fieldWrapper">
                <label class="textLabel"><%=eResApp.GetRes(_pref.Lang, 6723)%> :</label>
                <select id="FieldSeparator" onchange="setSeparator();">
                    <option value=";" selected="selected">;</option>
                    <option value="<tab>">Tab</option>
                    <option value=",">,</option>
                    <option value="*"><%=eResApp.GetRes(_pref.Lang, 6722)%></option>
                </select>
                <div id="CustomSeparatorDiv" style="display: none">
                    <input type="text" id="CustomFieldSeparator" value=";" />
                </div>
            </div>
            <div class="fieldWrapper">
                <label class="textLabel"><%=eResApp.GetRes(_pref.Lang, 1665)%> :</label>
                <input type="text" id="TextIdentifier" />
            </div>

                    <% if (_pref.ThemeXRM.Version < 2) { %>
                   <div class="fieldWrapper">
                       <input type="checkbox" checked="checked" id="ColHeaders" />
                    <% }
                        else { %>
                    <!--ELAIZ : Ajout de la checkbox eudonet x--> 
                    <div class="fieldWrapper" id="option-table-checkbox">
                        <input style="opacity:0;position:absolute" type="checkbox" checked="checked" id="ColHeaders" />
                        <span onclick="document.querySelector('#ColHeaders').click()" class="icon-square-o"></span>
                    <%} %>
                        <label for="ColHeader"><%=eResApp.GetRes(_pref.Lang, 1666)%></label>
                    </div>
        </div>
        <!--FIN VOLET ETAPE 1-->
        <!--VOLET ETAPE 2-->
         <!--ELAIZ : Ajout de la classe import step pour eudonet x --> 
        <div id="step2" class="import-step">
            <p><%=Com.Eudonet.Internal.eResApp.GetRes(_pref.Lang, 1668)%></p>
            <div id="TabSrc" runat="server" class="field_container">
            </div>
            <p class="ednKey"><%=Com.Eudonet.Internal.eResApp.GetRes(_pref.Lang, 1669)%></p>
            <div id="TabTrg" runat="server" class="field_container">
            </div>
        </div>
        <div id="step3">
            <ul class="eImport">
                <li>
                    <ul class="spritLine">
                        <li class="nbFiles"></li>
                        <li class="sizeFile"></li>
                        <li class="createFile"></li>
                        <li class="noFile"></li>
                    </ul>
                </li>
                <li>
                    <ul class="textLine">
                        <li id="trgNbFiles"></li>
                        <li id="trgSize"></li>
                        <li id="trgCreated"></li>
                        <li id="trgNotCreated"></li>
                    </ul>
                </li>
                <li class="infoBloc" id="infoError" style="display: none;">
                    <ul class="blocLine">
                        <span class="infoImg"><%=Com.Eudonet.Internal.eResApp.GetRes(_pref.Lang, 6831)%></span>
                        <li class="textInfo" id="trgMsg"></li>
                    </ul>
                </li>
            </ul>

        </div>
        <div class="opacityFieldDash" style="display: none;" id="cell"></div>
        <!--FIN VOLET ETAPE 2-->
        <!--CHAMPS CACHES-->
        <input type="hidden" id="sIdFrom" />
        <input type="hidden" id="sIdTo" />
        <input type="hidden" id="tab" value="<%=nTab%>" />
        <input type="hidden" id="tabfrom" value="<%=nTabFrom%>" />
        <input type="hidden" id="evtid" value="<%=nEvtId%>" />
    </div>
</body>
</html>
