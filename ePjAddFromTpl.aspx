<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ePjAddFromTpl.aspx.cs"
    Inherits="Com.Eudonet.Xrm.ePjAddFromTpl" EnableSessionState="true" EnableViewState="true" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>Annexes</title>
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>

    <style type="text/css" id="customCss" title="customCss"></style>

    <script type="text/javascript" language="javascript">
        // Liste des noms d'annexe pour le TPL Mail
        <%= _outputJs %>
    </script>
</head>
<body onload="initHeadEvents();initFldClick('102000');initPjList();loadCustomCss(102000);" ondrop="return false;" ondragover="return false;" ondragleave="return false;">
    <div class="window_iframe" id="mainDiv" runat="server">
        <form id="frmUpload" runat="server">

            <div id="blockUpload">
                <% if (IsAddAllowed)
                    { %>
                <div id="blockAddPj" class="blockAddPj <%=IsAddLinkAllowed ? " borderRight" : "" %>">

                    <% if (IsAddLinkAllowed)
                        { %>
                    <input id="radPJFile" name="radioPJ" type="radio" value="0" onclick="ChangeType(0);" />
                    <% } %>
                    <%=String.Concat(Com.Eudonet.Internal.eResApp.GetRes(_pref, 6316), " ", Com.Eudonet.Internal.eResApp.GetRes(_pref, 6311), " :")%>
                    <asp:Label ID="lbl_erreur" runat="server" Text="" CssClass="lblErreur" ForeColor="Red"></asp:Label>

                    <!-- left : -2000 pour que le bouton file n'apparait pas à l'ecran -->
                    <asp:FileUpload ID="FileToUpload" runat="server" Style="position: absolute; left: -2000em;" onchange="document.getElementById('SaveAs').value = ''; AddPj();" />
                    <!-- Puisque le clique n'est pas autorisé sur le bouton file (access denied sur IE8),
                                        on passe par le label qui a l'attribut "for" attaché au bouton file, le label a les autorisations -->
                    <label class="button-green fileUpload" for="FileToUpload" onclick="ChangeType(0);"><%=Com.Eudonet.Internal.eResApp.GetRes(_pref, 1114)%> </label>
                    <input id="SaveAs" name="SaveAs" type="hidden" />
                </div>
                <% }
                    if (IsAddLinkAllowed)
                    { %>
                <div id="blockAddLink">


                    <input id="radPJLink" name="radioPJ" type="radio" value="1" onclick="ChangeType(1);" />
                    <%=Com.Eudonet.Internal.eResApp.GetRes(_pref, 6315)%>
                                (<span><%=Com.Eudonet.Internal.eResApp.GetRes(_pref, 6313)%></span>)

                                <%=Com.Eudonet.Internal.eResApp.GetRes(_pref, 1500)%> :
                                <asp:Label ID="lbl_erreur_link" runat="server" Text="" CssClass="lblErreur" ForeColor="Red"></asp:Label>

                    <input type="text" id="uploadvalue" name="uploadvalue" class="pjAddSplitterLink" onclick="ChangeType(1);" />

                    <label class="button-green linkAdd" for="uploadvalue" onclick="ChangeType(1); AddPj();"><%=Com.Eudonet.Internal.eResApp.GetRes(_pref, 18)%></label>

                </div>

                <% } %>
            </div>

            <ul class="pjAddSplitter" style="display: none;">
                <li>
                    <%=Com.Eudonet.Internal.eResApp.GetRes(_pref, 119)%>
                        (<%=Com.Eudonet.Internal.eResApp.GetRes(_pref, 6314)%>) :</li>
                <li>
                    <input type="text" id="txtToolTip" name="txtToolTip" style="display: none;" />
                </li>

                <li class="pJsep"></li>
                <li>
                    <%=Com.Eudonet.Internal.eResApp.GetRes(_pref, 121)%>
                        :</li>
                <li>
                    <input type="text" id="txtDesc" name="txtDesc" style="display: none;" value="<%=Com.Eudonet.Internal.eResApp.GetRes(_pref, 113) %>&nbsp;:&nbsp;<%=Com.Eudonet.Internal.eDate.ConvertBddToDisplay(_pref.CultureInfo, DateTime.Now) %>" />
                </li>
            </ul>

            <div id="divlstPJ" runat="server" class="divlstPJ" ondragover="UpFilDragOver(this, event);return false;"
                ondragleave="UpFilDragLeave(this);return false;" ondrop="UpFilDrop(this,event);return false;">
            </div>
            <div id="divHidden" runat="server" style="clear: both; display: none;">
                <input type="hidden" id="action" name="action" />
                <input type="hidden" id="_sourceIframeId" name="_sourceIframeId" />
                <input type="hidden" id="_parentiframeid" name="_parentiframeid" />
                <input type="hidden" id="nbpj" name="nbpj" runat="server" />
                <input type="hidden" id="idspj" name="idspj" runat="server" />
                <input type="hidden" id="nFileID" name="nFileID" runat="server" />
                <input type="hidden" id="nTab" name="nTab" runat="server" />
                <input type="hidden" id="parentEvtFileId" name="parentEvtFileId" runat="server" />
                <input type="hidden" id="parentEvtTabId" name="parentEvtTabId" runat="server" />
                <input type="hidden" id="fromtpl" name="fromtpl" runat="server" />
                <input type="hidden" id="ppid" name="ppid" runat="server" />
                <input type="hidden" id="mailForward" name="mailForward" runat="server" />
                <input type="hidden" id="pmid" name="pmid" runat="server" />
                <input type="hidden" id="viewtype" name="all" runat="server" />
                <input type="hidden" id="pjType" name="pjType" runat="server" />
                <input type="hidden" id="lstTypeLink" name="lstTypeLink" runat="server" />
                <input type="hidden" id="UploadLink" name="UploadLink" runat="server" />
                <input type="hidden" id="txtToolTipLink" name="txtToolTipLink" runat="server" />
                <input type="hidden" id="txtDescLink" name="txtDescLink" runat="server" />
                <input type="hidden" id="herrorCallBack" name="herrorCallBack" runat="server" />
                <input type="hidden" id="hUploadMode" name="hUploadMode" runat="server" value="0" />

                <asp:Button ID="btnrefreshLst" runat="server" Text="" CssClass="btnRefresh" OnClick="RefreshListPj" />
            </div>
            <ul id="setViewType" runat="server">
                <li id="setViewType1" runat="server">
                    <input type="radio" id="rbViewType1" name="rbViewType" runat="server" /><label runat="server" id="labelViewType1" for="rbViewType1"></label></li>
                <li id="setViewType2" runat="server">
                    <input type="radio" id="rbViewType2" name="rbViewType" runat="server" /><label runat="server" id="labelViewType2" for="rbViewType2"></label></li>
            </ul>

            <script type="text/javascript">
                <%
                // #58059 On sélectionne la ligne qui vient d'être ajoutée pour les template mail et les campaigne mail
                if (_nTab == (int)EudoQuery.TableType.MAIL_TEMPLATE || _nTab == (int)EudoQuery.TableType.CAMPAIGN)
                {  %>
                    selectPjLine(<%=(int)EudoQuery.TableType.PJ%>, <%=_pjId%>);
                <%}%>

                <%=_outputJsSetWait%>
            </script>
        </form>
    </div>
</body>
</html>
