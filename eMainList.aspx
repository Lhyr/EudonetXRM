<%@ Import Namespace="Com.Eudonet.Xrm" %>
<%@ Import Namespace="Com.Eudonet.Internal" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eMainList.aspx.cs" Inherits="Com.Eudonet.Xrm.eMainList"
    EnableSessionState="true" EnableViewState="false" %>

<div id="mainListContent">
    <div id="infos">
        <table class="infos" cellpadding="0" cellspacing="0">
            <tr>

                <td class="icone">
                    <div id="iconeFilter" class="icon-list_filter" runat="server" onmouseover="stfilter(event);">
                    </div>
                </td>
                <td class="lib" width="30%">
                    <span runat="server" id='SpanNbElem'></span>&nbsp;<span runat="server" id='SpanLibElem'></span>
                    <!-- </div> -->
                </td>
                <%
                    if (!isCalendarEnabled)
                    {
                %>
                <td>
                    <div id="SubTabMenuCtnr" runat="server" class="subTabDiv">
                    </div>
                </td>
                <%
                    }
                %>
                <td width="10%">
                    <div runat="server" id="PlanningAction">
                    </div>
                </td>
                <td class="barreoutils" width="30%">
                    <div id="barre-outils" class="barre-outils">
                        <ul class="outils">                            
                            <%--<%
                                if (!isCalendarGraphEnabled || isMixedMode)
                                {
                            %>
                            <li><a class="icon-a_moins" onclick="resizeFont(-2);" rel="nofollow" title="<%=eResApp.GetRes(_pref, 6188)%>"></a></li>
                            <li><a class="icon-a_plus" onclick="resizeFont(2);" rel="nofollow" title="<%=eResApp.GetRes(_pref, 6189)%>"></a></li>
                            <%
                                }
                            %>--%>
                        </ul>
                    </div>
                </td>
            </tr>
        </table>
    </div>
    <div class="listfiltres" id="listfiltres">
        <table class="listfiltrestab">
            <tr>
                <td nowrap="nowrap">
                    <table runat="server" id="listQuickFilters" class="listQuickFilters">
                    </table>
                </td>

                <td width="280" align="right" nowrap="nowrap">

                    <% if (!isCalendarGraphEnabled || isMixedMode)
                        { %>

                    <div id="histoFilter" runat="server"></div>

                    <%} %>
                    <div class="advFltMenu" id="AdvFltMenu" runat="server" onmouseover="dispFltMenu(this,false);"
                        onclick="dispFltMenu(this,false);" onmouseout="hideFltMenu(this);">
                        <span><%=eResApp.GetRes(_pref, 6191)%></span>
                    </div>
                </td>

            </tr>
        </table>
    </div>
</div>
<div class="listheader" id="listheader" runat="server">
    <!-- ACTION LIST -->
    <div class="listactions" id="actions">
        <table class="listactionstab" cellspacing="0" cellpadding="0" border="0">
            <tbody>
                <tr>
                    <%


                        if (isCalendarGraphEnabled || isDayMode)
                        {
                    %>
                    <td>
                        <div class="calheader" id="calendarMainHeader">
                            <div class="pl_ht" <%=sCalHeadNavCss%>>

                                <table border="0" cellspacing="0" cellpadding="0">
                                    <tbody>
                                        <tr>
                                            <td align="right">
                                                <span onclick="setCalendarDate('<%=_nTab %>','<%=DateTime.Now %>');" class="head-today">
                                                    <%=eResApp.GetRes(_pref, 143)%></span>
                                            </td>
                                            <td>
                                                <div id="DIV1" class="icon-edn-prev fLeft icnListAct" onclick="setPrevCalDate();">
                                                </div>
                                            </td>
                                            <td align="center" nowrap>
                                                <span class="head_title-text" id="WeekLabel" runat="server"></span>
                                            </td>
                                            <td>
                                                <div id="DIV2" class="icon-edn-next fRight icnListAct" onclick="setNextCalDate();">
                                                </div>
                                            </td>
                                            <td>&nbsp;
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>

                            </div>
                        </div>
                    </td>
                    <%
                        }

                        if (!isCalendarGraphEnabled)
                        {
                    %>
                    <td style="width: 20%">
                        <%if (!bHideActionSelection)
                            {%>
                        <div class="actions" eudoaction="1" id="actionsH">
                            <div id="btnActionsTop">
                                <div id="actionLeft">
                                    <div id="aGH" class="icon-bt-actions-left aGH"></div>
                                </div>
                                <div id="actionMiddle">
                                    <div id="aMH" class="aM" onmouseover="displayActions('H');" onmouseout="omouA('H');hideActions('H');" onclick="displayActions('H')"><span><%=eResApp.GetRes(_pref, 296)%></span></div>
                                </div>
                                <div id="actionRight">
                                    <div id="aDH" class="icon-bt-actions-right aDH" onmouseover="displayActions('H');" onmouseout="omouA('H');hideActions('H');" onclick="displayActions('H')"></div>
                                </div>
                            </div>

                        </div>
                        <%} %>
                    </td>
                    <td width="22%">
                        <%if (!bHideActionSelection)
                            {%>
                        <i>
                            <%=eResApp.GetRes(_pref, 187)%>
                            <span id="nbCheckedHead"><%= nbMarkedFile%>  </span></i>
                        <%} %>
                    </td>
                    <%
                        }
                        if (!isCalendarGraphEnabled || isMixedMode)
                        {

                    %>
                    <td width="20px">
                        <div id="idFirst" class="icon-edn-first" runat="server">
                        </div>
                    </td>
                    <td width="15px">
                        <div id="idPrev" class="icon-edn-prev fLeft icnListAct" runat="server">
                        </div>
                    </td>
                    <td class="numpage" align="center" width="8%">
                        <table width="100%" cellpadding="0" cellspacing="0" border="0">
                            <tr>
                                <td style="text-align: right; width: 50%;">
                                    <span>
                                        <input class="pagInput" id="inputNumpage" name="inputNumpage" runat="server" size="1"
                                            style="display: none;" />
                                    </span>
                                </td>
                                <td id="tdNumpage" style="width: 50%; text-align: left;">
                                    <div id="divNumpage" runat="server">
                                    </div>
                                </td>
                            </tr>
                        </table>
                    </td>
                    <td width="15px">
                        <div id="idNext" class="icon-right fRight icnListAct" runat="server">
                        </div>
                    </td>
                    <td width="20px">
                        <div id="idLast" class="pagLast" runat="server">
                        </div>
                    </td>
                    <%
                        }
                    %>
                    <td class="eFSTD" width="42%">
                        <!-- CHAMPS DE RECHERCHE POUR LES FICHIERS PRINCIPAUX -->
                        <%
                            if (!isPlanning)
                            {
                        %>
                        <div id="eFS" runat="server" class="eFSContainer">
                            <input onfocus=" searchFocus();" onblur="searchBlur()" title="" type="text" name="q"
                                id="eFSInput" class="eFSInput" onkeyup="launchSearch(this.value, event);" />
                            <div class="" id="eFSStatus">
                            </div>
                        </div>
                        <%
                            }

                        %>
                        &nbsp;
                    </td>
                </tr>
            </tbody>
        </table>
    </div>
    <!-- FIN ACTION LIST -->
    <!-- FIN ACTION LIST -->
    <!-- LIST -->
    <div id="listContent" runat="server" width="100%" onmouseover="onListMouseOver(event); return false;">      

    </div>
    <div class="listactions" id="actions2" style="display: none">
        <table class="listactionstab" cellspacing="0" cellpadding="0" border="0" width="99%">
            <tbody>
                <tr>
                    <td style="width: 20%">
                        <div class="actions" eudoaction="1" id="actionsB">
                            <table style="border-collapse: collapse">
                                <tr>
                                    <td id="aGB" class="action aGB"></td>
                                    <td id="aMB" eactif="0" class="aM" onmouseover="omovA('B');displayActions('B');"
                                        onmouseout="omouA('B');hideActions('B');" onclick="displayActions('B')">
                                        <%=eResApp.GetRes(_pref.Lang, 296)%>
                                    </td>
                                    <td id="aDB" class="action aDB" onmouseover="omovA('B');displayActions('B');" onmouseout="omouA('B');hideActions('B');"
                                        onclick="displayActions('B')"></td>
                                </tr>
                            </table>
                        </div>
                    </td>
                    <td width="22%">
                        <i>
                            <%=eResApp.GetRes(_pref.Lang, 187)%>
                            <span id="nbCheckedFoot">0</span></i>
                    </td>
                    <td width="20px">
                        <div id="clsFirstB" class="pagFirst" runat="server">
                        </div>
                    </td>
                    <td width="15px">
                        <div id="clsPrevB" class="icon-edn-prev fLeft icnListAct " runat="server">
                        </div>
                    </td>
                    <td class="numpage" align="center" width="8%">
                        <table width="100%" height="100%" cellpadding="0" cellspacing="0">
                            <tr>
                                <td style="text-align: right; width: 50%;">
                                    <input class="pagInput" id="inputNumpageB" name="inputNumpageB" runat="server" size="1"
                                        style="display: none;" />
                                </td>
                                <td id="tdNumpageB" style="width: 50%; text-align: left;">
                                    <div id="divNumpageB" runat="server">
                                    </div>
                                </td>
                            </tr>
                        </table>
                    </td>
                    <td width="15px">
                        <div id="clsNextB" class="icon-edn-next fRight icnListAct" runat="server">
                        </div>
                    </td>
                    <td width="20px">
                        <div id="clsLastB" class="pagLast" runat="server">
                        </div>
                    </td>
                    <td width="42%" style="text-align: right; padding-right: 5px;"></td>
                </tr>
            </tbody>
        </table>
    </div>


    <div runat="server" class="idxFilter" id="fltindex" style="visibility: hidden;">
    </div>

</div>
