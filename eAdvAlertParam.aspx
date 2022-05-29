<%@ Import Namespace="EudoQuery" %>

<%@ Page ValidateRequest="false" Language="C#" AutoEventWireup="true" CodeBehind="eAdvAlertParam.aspx.cs"
    Inherits="Com.Eudonet.Xrm.eAdvAlertParam" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title></title>
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>
    <style type="text/css">
        #volumeIcon {
            position: relative;
            top: 4px;
            left: 13px;
        }
    </style>
    <script type="text/javascript">

        function getNbrAndDPartFromMin(lMinutes) {
            // Le delai est representé minutes

            lMinutes = CNumeric(lMinutes);

            var nNumber = 0;
            var strDatePart = "";
            var strReturnValue = "";

            var lMinute = 1;
            var lHour = 60 * lMinute;
            var lDay = 24 * lHour;
            var lWeek = 7 * lDay;

            var strMinute = "n";
            var strHour = "h";
            var strDay = "d";
            var strWeek = "w";

            if (lMinutes >= lWeek) {
                strDatePart = strWeek;
                nNumber = lMinutes / lWeek;
            }
            else if (lMinutes < lWeek && lMinutes >= lDay) {
                strDatePart = strDay;
                nNumber = lMinutes / lDay;
            }
            else if (lMinutes < lDay && lMinutes >= lHour) {
                strDatePart = strHour;
                nNumber = lMinutes / lHour;
            }
            else {
                strDatePart = strMinute;
                nNumber = lMinutes;
            }

            strReturnValue = nNumber + '$' + strDatePart;
            return strReturnValue;
        }
        //--
        //-- Renvoi le nombre de minutes
        function getMinFromNbrAndDPart(nNumber, strDatePart) {
            // Le delai est representé minutes

            nNumber = CNumeric(nNumber);

            var lMinutes = 0;
            var strReturnValue = "";

            var lMinute = 1;
            var lHour = 60 * lMinute;
            var lDay = 24 * lHour;
            var lWeek = 7 * lDay;

            var strMinute = "n";
            var strHour = "h";
            var strDay = "d";
            var strWeek = "w";

            if (strDatePart == strWeek) {
                lMinutes = nNumber * lWeek;
            }
            else if (strDatePart == strDay) {
                lMinutes = nNumber * lDay;
            }
            else if (strDatePart == strHour) {
                lMinutes = nNumber * lHour;
            }
            else if (strDatePart == strMinute) {
                lMinutes = nNumber;
            }

            strReturnValue = lMinutes;
            return strReturnValue;
        }
        //--
        //-- Convertie en numeric
        function CNumeric(vValue) {
            if (isNaN(vValue)) {
                vValue = vValue.replace(' ', '');
                vValue = vValue.replace(',', '.');

                // Enleve les caractères non numeriques
                var regEx = new RegExp();
                regEx.source = '[^0-9.-]';
                regEx.ignoreCase = true;
                regEx.global = true;
                vValue = vValue.replace(regEx, '');

                vValue = parseFloat(vValue);
            }
            return vValue;
        }

        // Mis a jour le choix sur l'alerte sonore
        function updateState(volumeIcon)
        {   var sound = document.getElementById('SoundCHK');
            if (sound)
            {
                sound.click();                
                volumeIcon.className = sound.checked ? "icon-volume-high" : "icon-volume-mute2";
            }
        }


    </script>
</head>
<body style="font-size: 8pt; font-family: Verdana;margin-left: 64px; margin-top: 24px;">
    <script language="javascript" type="text/javascript">
        function getReturnValue() {

            //-- Calcul du delai de l'alerte
            var nNumber = document.getElementById('Number').value;
            var strDatePart = document.getElementById('DatePart').value;
            var strTime = getMinFromNbrAndDPart(nNumber, strDatePart) + '';

            //-- Mise à jour des champs
            document.getElementById('<%=nTab + PlanningField.DESCID_ALERT_DATE.GetHashCode()%>').value = '';
            document.getElementById('<%=nTab + PlanningField.DESCID_ALERT_TIME.GetHashCode()%>').value = strTime.replace('.', ',');


           if (document.getElementById('SoundCHK').checked) {
                        document.getElementById('<%=nTab + PlanningField.DESCID_ALERT_SOUND.GetHashCode()%>').value = 'reminder.wav';
            }
            else {
                document.getElementById('<%=nTab + PlanningField.DESCID_ALERT_SOUND.GetHashCode()%>').value = '';
            }

                    //-- Infos de l'Alerte
            var strAlertDate = document.getElementById('<%=nTab + PlanningField.DESCID_ALERT_DATE.GetHashCode()%>').value;
                    var strAlertTime = document.getElementById('<%=nTab + PlanningField.DESCID_ALERT_TIME.GetHashCode()%>').value;
                    var strAlertSound = document.getElementById('<%=nTab + PlanningField.DESCID_ALERT_SOUND.GetHashCode()%>').value;
                    return strAlertDate + ";" + strAlertTime + ";" + strAlertSound;

        }

      

    </script>
    <br />
    <div class="IH">
        <%=GetRes(1530)%>&nbsp;
        <input name="Number" id="Number" style="width: 50px; text-align: right; border: 1px solid #afabac;" value="<%=strNumber%>" class="IZ" />
        <select name="DatePart" id="DatePart" style="width: 100px; text-align: right;" class="IZ">
            <option value="n" <%if (strDatePart == "n")
                         {%> selected <%}%>>
                <%=GetRes(850)%></option>
            <option value="h" <%if (strDatePart == "h")
                         {%> selected <%}%>>
                <%=GetRes(851)%></option>
            <option value="d" <%if (strDatePart == "d")
                         {%> selected <%}%>>
                <%=GetRes(853)%></option>
            <option value="w" <%if (strDatePart == "w")
                         {%> selected <%}%>>
                <%=GetRes(852)%></option>
        </select>&nbsp;
        <%=GetRes(1531)%>.
     

        <input type="checkbox" id="SoundCHK" name="SoundCHK" <%if (!String.IsNullOrEmpty(strAlertSound))
                     {%>  checked <%}%> style="margin-left: 10px; vertical-align: -3px; display:none; "/>      
        <span id="volumeIcon" title="<%= Com.Eudonet.Internal.eResApp.GetRes(_pref,8699) %>"" onclick="updateState(this);" class=<%= string.IsNullOrWhiteSpace(strAlertSound) ? "icon-volume-mute2" : "icon-volume-up" %>></span>
       
        <input type="hidden" name="BeginDate" id="BeginDate" value="<%=strBeginDate%>" />
        <input type="hidden" id="<%=nTab + PlanningField.DESCID_ALERT_DATE.GetHashCode()%>" value="" />
        <input type="hidden" id="<%=nTab + PlanningField.DESCID_ALERT_TIME.GetHashCode()%>" value="<%=nAlertTime%>" />
        <input type="hidden" id="<%=nTab + PlanningField.DESCID_ALERT_SOUND.GetHashCode()%>" value="<%=strAlertSound%>" />
    </div>
</body>
</html>
