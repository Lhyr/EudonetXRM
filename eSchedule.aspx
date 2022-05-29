<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eSchedule.aspx.cs" Inherits="Com.Eudonet.Xrm.eSchedule"
    EnableSessionState="true" EnableViewState="false" %>

<%@ Import Namespace="Com.Eudonet.Xrm" %>
<%@ Import Namespace="Com.Eudonet.Internal" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title></title>
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>

    <script type="text/javascript">
        var calleriframeid = "<%=Request.Form["calleriframeid"].ToString()%>";
        var _parentIframeId = "<%=Request.Form["_parentiframeid"].ToString()%>";
    </script>
</head>
<body onload="OnLoad()">
    <div runat="server" id="divFreq" class="divFreq">
        <h3 class="selFreq"><%=GetResHtml(1063)%></h3>
        <ul>
            <li>
                <div class="line">
                    <select id="ScheduleTypeList" onchange="setScheduleOption(this);"  runat="server">                        
      
                    </select>
                </div>
            </li>
            <li>
                <div runat="server" id="SchType">
                
                    <!--DAILY-->
                    <div id="daily_DIV" runat="server">
                        <div class="line">
                            <input name="ScheduleChoiceAdvDaily" type="radio" onclick="document.getElementById('daily_day').focus();"
                                id="dailyEvery" /><%=GetResHtml(1055)%>&nbsp;
                            <input onfocus="document.getElementById('dailyEvery').checked=true;" onclick="document.getElementById('dailyEvery').checked = true;"
                                type="text" style="width: 35px;" id="daily_day" value="1" endcheck="1" edntype="num" ednrngmin="1" />
                            &nbsp;<%=GetResHtml(853).ToLower()%>
                        </div>
                        <div class="line">
                            <input name="ScheduleChoiceAdvDaily" type="radio" id="dailyEveryWorkingDay" /><%=GetResHtml(1057)%>
                        </div>
                    </div>
                    <!--WEEKLY--> 
                    <div id="weekly_DIV" runat="server">
                        <%=GetResHtml(1056)%>&nbsp;<input type="text" style="width: 35px;" id="weekly_weekday" value="1" endcheck="1" edntype="num" ednrngmin="1" />&nbsp;<%=string.Concat( GetResHtml(852) , " " , GetResHtml(1058) )%>
                        <%
                            for (int i = 1; i <= 7; i++)
                            {
                        %>
                        <div class="line">
                            <input type="checkbox"  id="WorkWeekDay_<%=i%>" value="<%=i%>" />
                            <label for="WorkWeekDay_<%=i%>"><%=( GetResHtml(43 + i) )%></label>
                        </div>
                        <%
                            }
                        %>
                    </div>
                    <!--MONTHLY-->
                    <div id="monthly_DIV">
                        <div class="line">
                            <input type="radio" name="ScheduleChoiceAdvMonthly" onclick="document.getElementById('monthly_day').focus();"
                                id="monthlyEvery" /><%=GetResHtml(1058)%>&nbsp;
                            <input type="text" onclick="document.getElementById('monthlyEvery').checked = true;"
                                onfocus="document.getElementById('monthlyEvery').checked=true;" style="width: 35px;"
                                id="monthly_day" value="1" endcheck="1" edntype="num" ednrngmin="1" />&nbsp;<%= GetResHtml(1055 )%>&nbsp;
                            <input type="text" onclick="document.getElementById('monthlyEvery').checked = true;"
                                onfocus="document.getElementById('monthlyEvery').checked=true;" style="width: 35px;"
                                id="monthly_month" value="1"
                                endcheck="1" edntype="num" ednrngmin="1" />
                            &nbsp;<%= GetResHtml(854 )%>
                        </div>

                        <div class="line">
                            <input type="radio" name="ScheduleChoiceAdvMonthly" id="monthlyOrder" /><%=GetResHtml(1058)%>&nbsp;
                            <select onclick="document.getElementById('monthlyOrder').checked=true;" style="width: 85px"
                                size="1" id="monthly_order_1" name="monthly_order_1">
                                <option value="1">
                                    <%=GetResHtml(24)%></option>
                                <option value="2">
                                    <%= GetResHtml(1059 )%></option>
                                <option value="3">
                                    <%=GetResHtml(1060 )%></option>
                                <option value="4">
                                    <%=GetResHtml(1061 )%></option>
                                <option value="5">
                                    <%=GetResHtml(27 )%></option>
                            </select>
                            <select onclick="document.getElementById('monthlyOrder').checked=true;" style="width: 85px"
                                size="1" id="monthly_weekday_1" name="monthly_weekday_1">
                                <%
                                    for (int i = 1; i <= 7; i++)
                                    {
                                %>
                                <option value="<%=i%>">
                                    <%=GetResHtml(43 + i)%></option>
                                <%
                                    }
                                %>
                            </select>&nbsp;<%= GetResHtml(1055 )%>&nbsp;
                            <input type="text" onclick="document.getElementById('monthlyOrder').checked = true;"
                                onfocus="document.getElementById('monthlyOrder').checked=true;" style="width: 35px;"
                                id="monthly_month_1" value="1" endcheck="1" edntype="num" ednrngmin="1" />
                            &nbsp;<%= GetResHtml(854) %>
                        </div>
                    </div>
                    <!--YEARLY-->
                    <div id="yearly_DIV">
                        <div class="line">
                            <input type="radio" onclick="document.getElementById('yearly_day').focus();" name="ScheduleChoiceAdvYearly"
                                id="yearlyEvery" /><%=GetResHtml(1062)%>&nbsp;
                            <input onfocus="document.getElementById('yearlyEvery').checked=true;" onclick="document.getElementById('yearlyEvery').checked = true;"
                                type="text" style="width: 35px;" id="yearly_day" value="1" endcheck="1" edntype="num" ednrngmin="1" />
                            <select onclick="document.getElementById('yearlyEvery').checked=true;" style="width: 85px"
                                size="1" id="yearly_month" name="yearly_month">
                                <%
                                    for (int i = 1; i <= 12; i++)
                                    { 
                                %>
                                <option value="<%=i%>" ednday="<%=GetDaysOfMont(DateTime.Parse(strBeginDate),i)%>">
                                    <%=GetResHtml(31 + i)%></option>
                                <%
                                    }
                                %>
                            </select>
                        </div>
                        <div class="line">
                            <input type="radio" name="ScheduleChoiceAdvYearly" id="yearlyOrder" /><%=GetResHtml(1058)%>&nbsp;
                            <select onclick="document.getElementById('yearlyOrder').checked=true;" style="width: 85px"
                                size="1" id="yearly_order_1" name="yearly_order_1">
                                <option value="1">
                                    <%= GetResHtml(24) %></option>
                                <option value="2">
                                    <%=GetResHtml(1059) %></option>
                                <option value="3">
                                    <%=GetResHtml(1060) %></option>
                                <option value="4">
                                    <%=GetResHtml(1061) %></option>
                                <option value="5">
                                    <%=GetResHtml(27) %></option>
                            </select>
                            <select onclick="document.getElementById('yearlyOrder').checked=true;" style="width: 85px"
                                size="1" id="yearly_weekday_1" name="yearly_weekday_1">
                                <%
                                    for (int j = 1; j <= 7; j++)
                                    {
                                %>
                                <option value="<%=j%>">
                                    <%=GetResHtml(43 + j)%></option>
                                <%
                                    }
                                %>
                            </select>
                            &nbsp;<%=GetResHtml(554)%>&nbsp;
                            <select onclick="document.getElementById('yearlyOrder').checked=true;" style="width: 85px"
                                size="1" id="yearly_month_1" name="yearly_month_1">
                                <%
                                    for (int i = 1; i <= 12; i++)
                                    {
                                %>
                                <option value="<%=i%>" ednday="<%=GetDaysOfMont(DateTime.Parse(strBeginDate),i)%>">
                                    <%=GetResHtml(31+i)%></option>
                                <%
                                    }
                                %>
                            </select>
                        </div>

                    </div>
                   <!--UNE FOIS-->
                    <div id="once_DIV" runat="server"></div>
                </div>
            </li>
        </ul>
    </div>

    <!-- Horaires -->
    <h3 runat="server" id="hourTitle" class="selHour">Horaire</h3>
    <div runat="server" id="hourDiv">
        <table border="0" cellpadding="0" width="100%">
            <tr>
                <td class="scheduleEmptyLine" colspan="2"></td>
            </tr>
            <tr>
                <td class="scheduleLabel" nowrap><%=GetResHtml(469)%></td>
                <td class="scheduleDate" runat="server" id="tdHourLaunch"></td>
            </tr>
        </table>
    </div>


    <h3 class="selFreq"><%=GetResHtml(402)%></h3>
    <div id="periodeDiv">
        <table border="0" cellpadding="0" width="100%">
            <tr>
                <td class="scheduleEmptyLine" colspan="4"></td>
            </tr>
            <tr>
                <td></td>
                <td class="scheduleLabel" nowrap>
                    <%=GetResHtml(1091)%>
                </td>
                <td class="scheduleDate">
                    <input name="RangeBegin" id="RangeBegin" endcheck="1" edntype="date" /></td>
                <td class="lnkdate" id="RangeBegin_Cal" ><span class="icon-agenda" onclick="selectDateBegin('RangeBegin');"></span></td>
            </tr>
            <tr id="trRangeEnd">
                <td class="scheduleRadio">
                    <input type="radio" onclick="onEndDate();" name="RangeEnd" id="RangeEndChk" />
                </td>
                <td class="scheduleLabel" nowrap>
                    <%=GetResHtml(1090)%>
                </td>
                <td class="scheduleDate" width="50px">
                    <input name="RangeEndDate" id="RangeEndDate" endcheck="1" edntype="date" />
                </td>
                <td class="lnkdate" id="RangeEnd_Cal"><span class="icon-agenda"  onclick="selectDateEnd('RangeEndDate');"></span></td>
            </tr>
            <tr id="trRangeCount">
                <td class="scheduleRadio">
                    <input type="radio" onclick="onCount();" name="RangeEnd" id="RangeCountChk" />
                </td>
                <td class="scheduleLabel" nowrap>
                    <%=GetResHtml(1054)%>
                </td>
                <td class="scheduleDate" colspan="2">
                    <input onfocus="document.getElementById('RangeCountChk').checked=true;" name="RangeCount"
                        id="RangeCount" value="<%=nCount%>" endcheck="1" edntype="num" ednrngmin="1" ednrngmax="<%=MAX_REPEAT%>" />&nbsp;(1 - <%=MAX_REPEAT %>)
                </td>
            </tr>
        </table>
    </div>

 
    <!--
    <h3 runat="server" id="recipientsTitle" class="selHour"><%=GetResHtml(389)%> :</h3>
    <div runat="server" id="recipientsDiv">
        <table border="0" cellpadding="0" width="100%">
            <tr>
                <td class="scheduleEmptyLine" colspan="2"></td>
            </tr>
            <tr>
                <td class="scheduleLabel" nowrap><%=GetResHtml(6400)%> :</td>
                <td class="scheduleDate" runat="server" id="td1">
                    <input ednvalue="" type="text" id="RecipientsUser" name="RecipientsUser" readonly="readonly" />
                    <span id="UsersLinkRecipients" style="display: inline-block;" onclick="nsSchedule.SetUsers('RecipientsUser')" class="icon-catalog"></span>

                </td>
            </tr>
            <tr>
                <td class="scheduleLabel" nowrap>Autres destinataires :</td>
                <td class="scheduleDate" runat="server">
                    <input type="text" id="RecipientsCustom" name="RecipientsCustom" endcheck="1" edntype="mail" ednmulti="1" />
                </td>
            </tr>

        </table>
    </div>
    -->

 
    <h3 runat="server" id="ftpTitle" class="selHour">FTP :</h3>
    <div runat="server" id="ftpDiv">
        <table border="0" cellpadding="0" width="100%">
            <tr>
                <td class="scheduleEmptyLine" colspan="2"></td>
            </tr>
        </table>
    </div> 

    <!--CHAMPS PARAMS-->
    <input type="hidden" id="MainScheduleType" runat="server" />
    <input type="hidden" id="ScheduleId" value="<%=nScheduleId%>" />
    <input type="hidden" id="ScheduleType" value="<%=nType%>" />
    <input type="hidden" id="ScheduleFrequency" value="<%=nFrequency%>" />
    <input type="hidden" id="ScheduleDay" value="<%=nDay%>" />
    <input type="hidden" id="ScheduleMonth" value="<%=nMonth%>" />
    <input type="hidden" id="ScheduleOrder" value="<%=nOrder%>" />
    <input type="hidden" id="ScheduleWeekDay" value="<%= strWeekDay %>" />
    <input type="hidden" id="ScheduleRangeBegin" value="<%=eDate.ConvertBddToDisplay(_pref.CultureInfo, strBeginDate )%>" />
    <input type="hidden" id="ScheduleRangeEnd" value="<%= strEndDate.Length > 0 ? eDate.ConvertBddToDisplay(_pref.CultureInfo, strEndDate ) : ""%>" />
    <input type="hidden" id="ScheduleRangeCount" value="<%=nCount %>" />
    <input type="hidden" id="Tab" value="<%=nTab%>" />
    <input type="hidden" id="DefaultOrder" value="<%=nDefaultOrder%>" />
    <input type="hidden" id="WeekDaystrBeginDate" value="<%=WeekDaystrBeginDate%>" />
    <input type="hidden" id="DaystrBeginDate" value="<%=DaystrBeginDate%>" />
    <input type="hidden" id="MonthstrBeginDate" value="<%=MonthstrBeginDate%>" />
    <input type="hidden" id="WeekDay" value="<%=strWeekDay%>" />
    <input type="hidden" id="strWorkingDay" value="<%=strWorkingDay%>" />
    <input type="hidden" id="bAppointment" value="<%=bAppointment?"1":"0"%>" />
    <input type="hidden" id="FileId" value="<%=nFileId%>" />

    <input type="hidden" id="Hour" value="<%=_sHour%>" />
    <input type="hidden" id="Occ" value="<%=_nNbOcc%>" />
</body>
</html>
