var SCHEDULE_DAILY = 0;
var SCHEDULE_DAILY_WORKING_DAY = 1;
var SCHEDULE_WEEKLY = 2;
var SCHEDULE_MONTHLY = 3;
var SCHEDULE_YEARLY = 4;
var SCHEDULE_MAX_RANGE_COUNT = 100;
var SCHEDULE_DEFAULT_RANGE_COUNT = 5;
var SCHEDULE_ONCE = 6;
var SCHEDULE_MINUTELY = 99;


var nScheduleId;
var nType;
var nFrequency;
var nDay;
var nMonth;
var nOrder;
var strWeekDay;
var strBeginDateInit;
var strEndDate;
var strRangeCount;
var nTab;
var nCount;
var nDefaultOrder;
var WeekDaystrBeginDate;
var DaystrBeginDate;
var MonthstrBeginDate;
var nCountDefault = 5;
var CalendarWorkingDays;
var bAppointment;
var fileId;
var nMainScheduleType;
var modalUserCat;
var modalDate = null;
var parentDocument = null;
var Hour = "";
//utilisation d'un NS pour éviter les colisition de varname
var nsSchedule = {};

function OnLoad() {
    
    nMainScheduleType = document.getElementById("MainScheduleType");
    nScheduleId = document.getElementById("ScheduleId").value;
    nType = document.getElementById("ScheduleType").value;
    nFrequency = document.getElementById("ScheduleFrequency").value;
    nDay = document.getElementById("ScheduleDay").value;
    nMonth = document.getElementById("ScheduleMonth").value;
    nOrder = document.getElementById("ScheduleOrder").value;


    strWeekDay = document.getElementById("ScheduleWeekDay").value;

    strBeginDateInit = document.getElementById("ScheduleRangeBegin").value;
    strEndDate = document.getElementById("ScheduleRangeEnd").value;
    strRangeCount = document.getElementById("ScheduleRangeCount").value;
    nTab = document.getElementById("Tab").value;
    nCount = document.getElementById("RangeCount").value;
    nDefaultOrder = document.getElementById("DefaultOrder").value;
    WeekDaystrBeginDate = document.getElementById("WeekDaystrBeginDate").value;
    DaystrBeginDate = document.getElementById("DaystrBeginDate").value;
    MonthstrBeginDate = document.getElementById("MonthstrBeginDate").value;

    strWeekDay = document.getElementById("WeekDay").value;

    bAppointment = document.getElementById("bAppointment").value == "1";

    Hour = document.getElementById("Hour").value;

    fileId = document.getElementById("FileId").value;
    if (nScheduleId == 0) {
        SetDefaultValue();

        //KJE
        if (strEndDate == "" && strRangeCount == 1 && strWeekDay == "")
            SetDefaultValue('once');
    }
    else {
        if (nType == SCHEDULE_DAILY) {

            if (strEndDate == "" && strRangeCount == 1 && strWeekDay == "") {
                SetDefaultValue('once');
            } else
                SetDefaultValue();
        }
        if (nType == SCHEDULE_DAILY_WORKING_DAY)
            SetDefaultValue();
        if (nType == SCHEDULE_WEEKLY)
            SetDefaultValue('weekly_DIV');
        if (nType == SCHEDULE_MONTHLY)
            SetDefaultValue('monthly_DIV');
        if (nType == SCHEDULE_YEARLY)
            SetDefaultValue('yearly_DIV');
        else if (nType == SCHEDULE_MINUTELY) {

            SetDefaultValue('minutely');

        }
        var strId = '';

        if (nType == SCHEDULE_DAILY || nType == SCHEDULE_DAILY_WORKING_DAY)
            strId = 'daily';
        else if (nType == SCHEDULE_WEEKLY)
            strId = 'weekly';
        else if (nType == SCHEDULE_MONTHLY)
            strId = 'monthly';
        else if (nType == SCHEDULE_YEARLY)
            strId = 'yearly';

        SetValue(strId);
    }



}

function onCount() {
    // ASY
    document.getElementById('RangeCount').disabled = false;

    document.getElementById('RangeCount').focus();
    if (document.getElementById('RangeCount').value == 0)
        document.getElementById('RangeCount').value = nCountDefault;
    document.getElementById('RangeEndDate').value = ''
    document.getElementById('RangeEndDate').disabled = true;
    document.getElementById('RangeEnd_Cal').style.visibility = 'hidden';
}

function onEndDate() {
    // ASY
    document.getElementById('RangeCount').value = 0
    document.getElementById('RangeCount').disabled = true;

    document.getElementById('RangeEndDate').disabled = false;
    document.getElementById('RangeEndDate').focus();
    document.getElementById('RangeEnd_Cal').style.visibility = 'visible';
}

function SetValue(strId) {
    //document.getElementById(strId).checked = true;
    setScheduleOption();

    document.getElementById('RangeBegin').value = strBeginDateInit;
    document.getElementById('RangeEndDate').value = strEndDate;
    if (document.getElementById('RangeEndDate').value == '') {
        document.getElementById('RangeCountChk').checked = true;
        document.getElementById('RangeCount').value = nCount;
        onCount();
    }
    else {
        document.getElementById('RangeEndDate').value = strEndDate;
        document.getElementById('RangeEndChk').checked = true;
        onEndDate();
    }
    if (strId == 'daily') {

        if (nType == SCHEDULE_DAILY) {
            document.getElementById('dailyEvery').checked = true;
            document.getElementById('daily_day').value = nFrequency;
        }
        else {
            document.getElementById('dailyEveryWorkingDay').checked = true;
        }
    }
    else if (strId == 'weekly') {
        document.getElementById('weekly_weekday').value = nFrequency;

        var aWeekDay = strWeekDay.split(";");
        for (i = 0; i < aWeekDay.length; i++) {
            document.getElementById('WorkWeekDay_' + aWeekDay[i]).checked = true;
        }

    }
    else if (strId == 'monthly') {
        SetDefaultValue('monthly_DIV');
        if (nOrder == 0) {
            document.getElementById('monthlyEvery').checked = true;
            document.getElementById('monthly_day').value = nDay;
            document.getElementById('monthly_month').value = nFrequency;
        }
        else {
            document.getElementById('monthlyOrder').checked = true;
            SelectItem(document.getElementById('monthly_order_1'), nOrder);
            SelectItem(document.getElementById('monthly_weekday_1'), WeekDaystrBeginDate);
            document.getElementById('monthly_month_1').value = nFrequency;
        }
    }
    else if (strId == 'yearly') {
        SetDefaultValue('yearly_DIV');
        if (nOrder == 0) {
            document.getElementById('yearlyEvery').checked = true;
            document.getElementById('yearly_day').value = nDay;
            SelectItem(document.getElementById('yearly_month'), nMonth);
        }
        else {
            document.getElementById('yearlyOrder').checked = true;

            SelectItem(document.getElementById('yearly_order_1'), nOrder);
            SelectItem(document.getElementById('yearly_weekday_1'), strWeekDay);
            SelectItem(document.getElementById('yearly_month_1'), nMonth);
        }
    }

}

//TODO : cette fonction qui sert a récupérer des valeurs d'un catalogue utilisateur
// existe a pas mal d'endroit de l'appli. Si un jour on a le temps (lol), ce serait bien d'unifier/centraliser tout ça.
nsSchedule.SetUsers = function (id) {
    modalUserCat = new eModalDialog(top._res_246, 0, "eCatalogDialogUser.aspx", 550, 610);


    modalUserCat.addParam("iframeId", modalUserCat.iframeId, "post");
    modalUserCat.ErrorCallBack = function () { setWait(false); }
    modalUserCat.addParam("multi", "1", "post");
    modalUserCat.addParam("selected", document.getElementById(id).getAttribute("ednvalue"), "post");
    modalUserCat.addParam("modalvarname", "modalUserCat", "post");
    modalUserCat.show();
    modalUserCat.addButton(top._res_29, function () { modalUserCat.hide(); }, "button-gray", id, null, true);
    modalUserCat.addButton(top._res_28, function () { nsSchedule.SetUserValid(modalUserCat, id); }, "button-green", id);
}


//
nsSchedule.SetUserValid = function (objModal, trgId) {

    var strReturned = objModal.getIframe().GetReturnValue();
    objModal.hide();
    var vals = strReturned.split('$|$')[0];
    var libs = strReturned.split('$|$')[1];
    var oTarget = document.getElementById(trgId);
    oTarget.value = libs;
    oTarget.setAttribute("value", libs);
    oTarget.setAttribute("ednvalue", vals);

}


function SetDefaultValue(strId) {

    var selIndex = 0;
    if (!strId) {
        document.getElementById('ScheduleTypeList').selectedIndex = 0;
        document.getElementById('dailyEvery').checked = true;
        document.getElementById('daily_day').value = 1;
        document.getElementById('RangeBegin').value = strBeginDateInit;
        document.getElementById('RangeCountChk').checked = true;
        document.getElementById('RangeCount').value = nCount;
        document.getElementById('RangeEndDate').value = '';
        selIndex = 0;
        onCount();
    }
    else if (strId == 'weekly_DIV') {
        document.getElementById('weekly_weekday').value = 1;
        //document.getElementById('WorkWeekDay_WeekDay(now)').checked = true;


        if (nScheduleId != -1) {

            //dans le cadre de planification par jour, cette case a pour effet de forcer le jour du début de planification
            // comme faisant partie des jours de récurence. Par exemple, si la planif commence le 17/05/2016 et que ce jour est un mardi,
            // alors la récurence aura lieu les mardi même si ce jour n'est pas coché initialement.
            // cela peut être déstabilisant car cela n'est visible  que si le user réouvre les paramètres
            // je laisse tel quel pour les planing mais je désactive pour les rapports             
            document.getElementById("WorkWeekDay_" + WeekDaystrBeginDate).checked = true;
        }
        selIndex = 1;
    }
    else if (strId == 'monthly_DIV') {
        document.getElementById('monthlyEvery').checked = true;
        document.getElementById('monthly_day').value = DaystrBeginDate;
        document.getElementById('monthly_month').value = 1;

        SelectItem(document.getElementById('monthly_order_1'), nDefaultOrder);
        SelectItem(document.getElementById('monthly_weekday_1'), Number(WeekDaystrBeginDate));
        document.getElementById('monthly_month_1').value = 1;
        selIndex = 2;
    }
    else if (strId == 'yearly_DIV') {
        document.getElementById('yearlyEvery').checked = true;
        document.getElementById('yearly_day').value = DaystrBeginDate;
        SelectItem(document.getElementById('yearly_month'), MonthstrBeginDate);

        SelectItem(document.getElementById('yearly_order_1'), nDefaultOrder);
        SelectItem(document.getElementById('yearly_weekday_1'), Number(WeekDaystrBeginDate));
        SelectItem(document.getElementById('yearly_month_1'), MonthstrBeginDate);
        selIndex = 3;
    }
    else if (strId == 'once') {
        document.getElementById('dailyEvery').checked = true;
        document.getElementById('daily_day').value = 1;
        document.getElementById('RangeBegin').value = strBeginDateInit;
        document.getElementById('RangeCountChk').checked = true;
        document.getElementById('RangeCount').value = nCount;
        document.getElementById('RangeEndDate').value = '';


        var elHourSelect = document.getElementById('HourSelect');
        var optp = elHourSelect.querySelector('option[value="' + Hour + '"]')
        if (optp)
            document.getElementById("HourSelect").selectedIndex = optp.index;

        selIndex = 4;
    }
    else if (strId == 'minutely') {

        var nFrequency = document.getElementById("ScheduleFrequency").value;
        var arrOptionsTp = Array.prototype.slice.call(document.getElementById("ScheduleTypeList").options);
        var sKey = (nFrequency != 1 ? nFrequency : "") + "minutely";
        selIndex = arrOptionsTp.findIndex(op => op.value === sKey)
        if (selIndex === -1)
            selIndex = 7;

    }

    document.getElementById("ScheduleTypeList").selectedIndex = selIndex;
    setScheduleOption();
}

function SelectItem(cboTarget, strValue) {
    if (!cboTarget)
        return;

    for (var j = 0; j < cboTarget.length; j++) {
        if (cboTarget.options.item(j).value == strValue) {
            cboTarget.selectedIndex = j;
            break;
        }
    }
    if (j > (cboTarget.length - 1)) {
        cboTarget.selectedIndex = -1;
    }
}

function Valid(callBackFunction) {



    var bDaily = document.getElementById('ScheduleTypeList')[document.getElementById('ScheduleTypeList').selectedIndex].value == "daily";
    var bWeekly = document.getElementById('ScheduleTypeList')[document.getElementById('ScheduleTypeList').selectedIndex].value == "weekly";
    var bMonthly = document.getElementById('ScheduleTypeList')[document.getElementById('ScheduleTypeList').selectedIndex].value == "monthly";
    var bYearly = document.getElementById('ScheduleTypeList')[document.getElementById('ScheduleTypeList').selectedIndex].value == "yearly";
    var bOnce = document.getElementById('ScheduleTypeList')[document.getElementById('ScheduleTypeList').selectedIndex].value == "once";

    var bMinutely = document.getElementById('ScheduleTypeList')[document.getElementById('ScheduleTypeList').selectedIndex].value == "minutely";
    var b5Minutely = document.getElementById('ScheduleTypeList')[document.getElementById('ScheduleTypeList').selectedIndex].value == "5minutely";
    var b10Minutely = document.getElementById('ScheduleTypeList')[document.getElementById('ScheduleTypeList').selectedIndex].value == "10minutely";


    var bDisplayMsg = false;
    var strTitle = '';
    var strDesc = '';
    var strMsg = top._res_1065;
    var strResNumericValue = top._res_673;


    var strBddBeginDate = eDate.ConvertDisplayToBdd(document.getElementById('RangeBegin').value);
    var oBddBeginDate = eDate.Tools.GetDateFromString(strBddBeginDate); //avec la conversion

    //Champs de la table
    var oType = document.getElementById('ScheduleType');
    var oFrequency = document.getElementById('ScheduleFrequency');
    var oDay = document.getElementById('ScheduleDay');
    var oMonth = document.getElementById('ScheduleMonth');
    var oOrder = document.getElementById('ScheduleOrder');
    var oWeekDay = document.getElementById('ScheduleWeekDay');
    var oRangeBegin = document.getElementById('ScheduleRangeBegin');
    var oRangeEnd = document.getElementById('ScheduleRangeEnd');
    var oRangeCount = document.getElementById('ScheduleRangeCount');
    var oHour = document.getElementById('Hour');

    //Initialisation des champs
    oType.value = 0;
    oFrequency.value = 0;
    oDay.value = 0;
    oMonth.value = 0;
    oOrder.value = 0;
    oWeekDay.value = '';
    oRangeBegin.value = '';
    oRangeEnd.value = '';
    oRangeCount.value = '';
    oHour.value = '';

    if (bDaily) {
        if (isNaN(document.getElementById('daily_day').value)) {
            bDisplayMsg = true;
            strDesc = strResNumericValue;
        }

        if (document.getElementById('daily_day').value == '')
            document.getElementById('daily_day').value = 0;
        if (document.getElementById('dailyEvery').checked && parseInt(document.getElementById('daily_day').value) <= 0) {
            bDisplayMsg = true;
            strDesc = top._res_1067.replace("<ITEM>", top._res_822);
        }

        if (document.getElementById('dailyEvery').checked) {
            oType.value = SCHEDULE_DAILY;
            oFrequency.value = document.getElementById('daily_day').value;
        }
        else if (document.getElementById('dailyEveryWorkingDay').checked) {
            oType.value = SCHEDULE_DAILY_WORKING_DAY;
            oWeekDay.value = document.getElementById("strWorkingDay").value;
        }
    }
    else if (bOnce) {
        oType.value = SCHEDULE_DAILY;


        document.getElementById('RangeEndDate').value = "";
        document.getElementById('RangeCount').value = 1;
        document.getElementById('daily_day').value = 1;

        document.getElementById('RangeCountChk').checked = true;
        document.getElementById('dailyEvery').checked = true;

        oFrequency.value = 1;

    }
    else if (bWeekly) {
        var bNoWeekday = true;
        for (var i = 1; i <= 7; i++) {
            if (document.getElementById('WorkWeekDay_' + i).checked) {
                bNoWeekday = false;
                if (oWeekDay.value == '')
                    oWeekDay.value = i;
                else
                    oWeekDay.value = oWeekDay.value + ';' + i;
            }
        }

        if (isNaN(document.getElementById('weekly_weekday').value)) {
            bDisplayMsg = true;
            strDesc = strResNumericValue;
        }

        if (document.getElementById('weekly_weekday').value == '')
            document.getElementById('weekly_weekday').value = 0;

        if (bNoWeekday) {
            bDisplayMsg = true;
            strDesc = strDesc = top._res_1067.replace("<ITEM>", top._res_822);
        }
        if (parseInt(document.getElementById('weekly_weekday').value) <= 0) {
            bDisplayMsg = true;
            strDesc = strDesc = top._res_1069.replace("<ITEM>", top._res_821);
        }

        //if( !document.getElementById('WorkWeekDay_WeekDay(strBeginDate)').checked && !bDisplayMsg )
        if (!bDisplayMsg) {
            if (strWeekDay != '') {
                strWeekDay = oWeekDay.value;
                var aWeekDay = strWeekDay.split(";");
                //Premier jour choisi
                var nFirstWeekDaySelect = aWeekDay[0];
                var nWeekDay = WeekDaystrBeginDate;
                var nDiff = nFirstWeekDaySelect - nWeekDay;

                if (nDiff != 0) {
                    var beginDate = eDate.Tools.GetStringFromDate(dateAdd('d', nDiff, eDate.Tools.GetDateFromString(eDate.ConvertDisplayToBdd(strBeginDateInit))));

                    //Modification de la date de début
                    onChangeParentDate(beginDate);
                }
            }
        }
        oType.value = SCHEDULE_WEEKLY;
        oFrequency.value = document.getElementById('weekly_weekday').value;
    }
    else if (bMonthly) {

        if (document.getElementById('monthlyEvery').checked) {
            if (isNaN(document.getElementById('monthly_day').value)) {
                bDisplayMsg = true;
                strDesc = strResNumericValue;
            }

            if (isNaN(document.getElementById('monthly_month').value)) {
                bDisplayMsg = true;
                strDesc = strResNumericValue;
            }

            if (document.getElementById('monthly_day').value == '')
                document.getElementById('monthly_day').value = 0

            if (document.getElementById('monthly_month').value == '')
                document.getElementById('monthly_month').value = 0;
        }
        else {
            if (isNaN(document.getElementById('monthly_month_1').value)) {
                bDisplayMsg = true;
                strDesc = strResNumericValue;
            }

            if (document.getElementById('monthly_month_1').value == '')
                document.getElementById('monthly_month_1').value = 0;
        }

        if (document.getElementById('monthlyEvery').checked && (parseInt(document.getElementById('monthly_day').value) <= 0 || parseInt(document.getElementById('monthly_day').value) > 31)) {
            bDisplayMsg = true;
            strDesc = top._res_1070;
        }
        if (document.getElementById('monthlyEvery').checked && parseInt(document.getElementById('monthly_month').value) <= 0) {
            bDisplayMsg = true;
            strDesc = top._res_1067.replace("<ITEM>", top._res_405);
        }
        if (document.getElementById('monthlyOrder').checked && parseInt(document.getElementById('monthly_month_1').value) <= 0) {
            bDisplayMsg = true;
            strDesc = top._res_1067.replace("<ITEM>", top._res_405);

        }

        if (document.getElementById('monthlyEvery').checked && !bDisplayMsg) {
            var strMonth = oBddBeginDate.getMonth() + 1;
            var strYear = oBddBeginDate.getFullYear();
            var nNbDay = parseInt(document.getElementById('monthly_day').value);

            if (nNbDay > getNbDayOfMonth(strMonth, strYear) || nNbDay == 29 || nNbDay == 30 || nNbDay == 31) {
                var strMsg = top._res_1071;
                strMsg = strMsg.replace("<DAY>", nNbDay);
                if (!confirm(strMsg)) {
                    return;
                }
            }
        }

        if (!bDisplayMsg) {
            oType.value = SCHEDULE_MONTHLY;
            if (document.getElementById('monthlyEvery').checked) {
                var strDayBegin = strBddBeginDate.substring(0, 2);
                strDayBegin = eDate.Tools.MakeTwoDigit(strDayBegin);
                //Jour choisi
                var strDayBeginChoice = (document.getElementById('monthly_day').value).substring(0, 2);
                strDayBeginChoice = eDate.Tools.MakeTwoDigit(strDayBeginChoice);
                if (strDayBegin != strDayBeginChoice) {
                    var beginDate = strDayBeginChoice + strBddBeginDate.substring(2, strBddBeginDate.length);

                    //Modification de la date de début
                    onChangeParentDate(beginDate);
                }
                oDay.value = document.getElementById('monthly_day').value;
                oFrequency.value = document.getElementById('monthly_month').value;
            }
            else {
                oOrder.value = document.getElementById('monthly_order_1').value;
                var oLst = document.getElementById('monthly_weekday_1');
                oWeekDay.value = oLst.options[oLst.selectedIndex].value;
                oFrequency.value = document.getElementById('monthly_month_1').value;

                var nOrder = oOrder.value;
                var nWeekDay = oWeekDay.value;

                //Date du début
                var strDayBeginChoice = strBddBeginDate.substring(0, 2);

                //
                var strMonth = oBddBeginDate.getMonth() + 1;
                // HLA - pour firefox
                if (oBddBeginDate.getFullYear)
                    var strYear = oBddBeginDate.getFullYear();
                else
                    var strYear = oBddBeginDate.getYear();

                strMonth = eDate.Tools.MakeTwoDigit(strMonth);
                var dBeginMonth = eDate.Tools.GetDateFromString('01/' + strMonth + '/' + strYear);
                var dTemp = dBeginMonth;
                var bNoFind = true;

                while (bNoFind) {
                    if (dTemp.getDay() == nWeekDay - 1) {
                        bNoFind = false;

                        if (nOrder >= 2 && nOrder <= 4)
                            dTemp = dateAdd('d', (nOrder - 1) * 7, dTemp);
                        else if (nOrder == 5) {
                            //Dernier jour
                            if (dateAdd('d', 4 * 7, dTemp).getMonth() + 1 == parseInt(strMonth))
                                dTemp = dateAdd('d', 4 * 7, dTemp);
                            else
                                dTemp = dateAdd('d', 3 * 7, dTemp);
                        }
                    }
                    if (bNoFind) {
                        dTemp = dateAdd('d', 1, dTemp);
                    }
                }

                var strTime = eDate.Tools.GetStringFromDate(eDate.Tools.GetDateFromString(eDate.ConvertDisplayToBdd(strBeginDateInit)), false, true)

                var strTemp = eDate.Tools.GetStringFromDate(dTemp);
                var beginDate = strTemp.replace('00:00:00', strTime).replace(' 00:00', ' ' + strTime);

                //Modification de la date de début
                onChangeParentDate(beginDate);
            }
        }
    }
    else if (bYearly) {
        oType.value = SCHEDULE_YEARLY;
        oFrequency.value = 1;
        if (document.getElementById('yearlyEvery').checked) {
            if (isNaN(document.getElementById('yearly_day').value)) {
                bDisplayMsg = true;
                strDesc = strResNumericValue;
            }

            if (document.getElementById('yearly_day').value == '')
                document.getElementById('yearly_day').value = 0;

            var oLst = document.getElementById('yearly_month');
            var nbDayOfMonth = oLst.options[oLst.selectedIndex].getAttribute("ednDay");
            var nDay = parseInt(document.getElementById('yearly_day').value);

            if (nDay <= 0 || nDay > nbDayOfMonth) {
                bDisplayMsg = true;
                strDesc = strDesc = top._res_1070;
            }

            oDay.value = document.getElementById('yearly_day').value;
            oMonth.value = oLst.options[oLst.selectedIndex].value;

            if (oDay.value == 29 && parseInt(oMonth.value) == 2) {
                strMsg = top._res_1072;
                if (!confirm(strMsg)) {
                    return;
                }
            }

            if (!bDisplayMsg) {
                //Jour et Mois du début
                var strDayBeginChoice = strBddBeginDate.substring(0, 2);
                var strMonthBeginChoice = strBddBeginDate.substring(3, 5);

                if (strDayBeginChoice + '/' + strMonthBeginChoice != eDate.Tools.MakeTwoDigit(oDay.value) + '/' + eDate.Tools.MakeTwoDigit(oMonth.value)) {
                    var strYear = eDate.ConvertDisplayToBdd(strBeginDateInit).substring(6, 10);
                    var strTime = eDate.Tools.GetStringFromDate(eDate.Tools.GetDateFromString(eDate.ConvertDisplayToBdd(strBeginDateInit)), false, true)

                    var beginDate = eDate.Tools.MakeTwoDigit(oDay.value) + '/' + eDate.Tools.MakeTwoDigit(oMonth.value) + '/' + strYear + ' ' + strTime

                    //Modification de la date de début
                    onChangeParentDate(beginDate);
                }
            }
        }
        else {
            var oLst = document.getElementById('yearly_order_1');
            oOrder.value = oLst.options[oLst.selectedIndex].value;
            var oLst = document.getElementById('yearly_weekday_1');
            oWeekDay.value = oLst.options[oLst.selectedIndex].value;
            var oLst = document.getElementById('yearly_month_1');
            oMonth.value = oLst.options[oLst.selectedIndex].value;

            var nOrder = oOrder.value;
            var nWeekDay = oWeekDay.value;

            //Date du début
            var strDayBeginChoice = strBddBeginDate.substring(0, 2);

            var strYear = oBddBeginDate.getFullYear();

            strMonth = eDate.Tools.MakeTwoDigit(oMonth.value);
            var dBeginMonth = eDate.Tools.GetDateFromString('01/' + strMonth + '/' + strYear);
            var dTemp = dBeginMonth;
            var bNoFind = true;

            while (bNoFind) {
                if (dTemp.getDay() == nWeekDay - 1) {
                    bNoFind = false;

                    if (nOrder >= 2 && nOrder <= 4)
                        dTemp = dateAdd('d', (nOrder - 1) * 7, dTemp);
                    else if (nOrder == 5) {
                        //Dernier jour
                        if (dateAdd('d', 4 * 7, dTemp).getMonth() + 1 == parseInt(strMonth))
                            dTemp = dateAdd('d', 4 * 7, dTemp);
                        else
                            dTemp = dateAdd('d', 3 * 7, dTemp);
                    }
                }
                if (bNoFind) {
                    dTemp = dateAdd('d', 1, dTemp);
                }
            }

            var strTime = eDate.Tools.GetStringFromDate(eDate.Tools.GetDateFromString(strBeginDateInit), false, true)

            var strTemp = eDate.Tools.GetStringFromDate(dTemp);

            //le format des heures retourné par eDate.Tools.GetStringFromDate et ' 00:00', on inclut l'espace pour faire le replace
            var beginDate = strTemp.replace('00:00:00', strTime).replace(' 00:00', ' ' + strTime);

            //Modification de la date de début
            onChangeParentDate(beginDate);
        }
    }
    else if (b10Minutely) {
        oType.value = 99;
        oFrequency.value = 10;
    }
    else if (b5Minutely) {
        oType.value = 99;
        oFrequency.value = 5;
    }
    else if (bMinutely) {
        oType.value = 99;
        oFrequency.value = 1;
    }


    if (document.getElementById('RangeBegin').value == '')
        bDisplayMsg = true;

    if (document.getElementById('RangeEndDate').value == '' && document.getElementById('RangeEndChk').checked) {
        bDisplayMsg = true;
        strDesc = top._res_330;
    }

    if (document.getElementById('RangeCount').value == '' && document.getElementById('RangeCountChk').checked) {
        bDisplayMsg = true;
        strDesc = strResNumericValue;
    }


    if (eDate.Tools.GetDateFromString(eDate.ConvertDisplayToBdd(document.getElementById('RangeBegin').value)) >= eDate.Tools.GetDateFromString(eDate.ConvertDisplayToBdd(document.getElementById('RangeEndDate').value))) {
        bDisplayMsg = true;
        strDesc = top._res_804;
    }

    oRangeBegin.value = document.getElementById('RangeBegin').value;

    if (document.getElementById('RangeEndDate').value != "") {
        if (!IsDate("RangeEndDate")) {
            bDisplayMsg = true;
            strDesc = top._res_846;
        }
    }
    oRangeEnd.value = document.getElementById('RangeEndDate').value;

    if (document.getElementById('RangeCountChk').checked && isNaN(document.getElementById('RangeCount').value)) {
        bDisplayMsg = true;
        strDesc = strResNumericValue;
    }

    if (document.getElementById('RangeCountChk').checked && parseInt(document.getElementById('RangeCount').value) <= 0) {
        bDisplayMsg = true;
        strTitle = top._res_1054;
        strDesc = top._res_2999.replace('<X>', '0').replace('<Y>', SCHEDULE_MAX_RANGE_COUNT);
    }

    if (document.getElementById('RangeCountChk').checked && parseInt(document.getElementById('RangeCount').value) > SCHEDULE_MAX_RANGE_COUNT) {
        bDisplayMsg = true;
        strTitle = top._res_1054;
        strDesc = top._res_1068 + ' (' + SCHEDULE_MAX_RANGE_COUNT + ')';
    }

    if (oRangeEnd.value == '')
        oRangeCount.value = document.getElementById('RangeCount').value;

    if (oRangeEnd.value != '') {
        var nYearDiff = eDate.Tools.GetDateFromString(eDate.ConvertDisplayToBdd(oRangeEnd.value)).getYear() - eDate.Tools.GetDateFromString(eDate.ConvertDisplayToBdd(oRangeBegin.value)).getYear();

        if (nYearDiff >= 10) {
            bDisplayMsg = true;
            strDesc = top._res_1068;
        }
    }

    var elHourSelect = document.getElementById('HourSelect');
    if (elHourSelect != null && elHourSelect.selectedIndex > -1)
        oHour.value = elHourSelect[elHourSelect.selectedIndex].value;

    if (bDisplayMsg) {
        if (strTitle == '')
            strTitle = strDesc;
        //MsgBox( strMsg, 'MSG_EXCLAM', 'Replace( strTitle, "'", "\'" )',strDesc,'');
        eAlert(0, strTitle, strDesc, '', 500, 200);
        return;
    }

    //Envoi du formulaire

    var nScheduleId = document.getElementById("ScheduleId").value;
    var nType = document.getElementById("ScheduleType").value;
    var nFrequency = document.getElementById("ScheduleFrequency").value;
    var nDay = document.getElementById("ScheduleDay").value;
    var nMonth = document.getElementById("ScheduleMonth").value;
    var nOrder = document.getElementById("ScheduleOrder").value;
    var strWeekDay = document.getElementById("ScheduleWeekDay").value;
    var strBeginDate = document.getElementById("ScheduleRangeBegin").value;
    var strEndDate = document.getElementById("ScheduleRangeEnd").value;
    var nCount = document.getElementById("ScheduleRangeCount").value;
    var Hour = document.getElementById("Hour").value;
    var nTab = document.getElementById("Tab").value;



    var oUpd = new eUpdater("mgr/eScheduleManager.ashx", 0);

    oUpd.addParam("ScheduleId", nScheduleId, "post");
    oUpd.addParam("ScheduleType", nType, "post");
    oUpd.addParam("ScheduleFrequency", nFrequency, "post");
    oUpd.addParam("ScheduleDay", nDay, "post");
    oUpd.addParam("ScheduleMonth", nMonth, "post");
    oUpd.addParam("ScheduleOrder", nOrder, "post");
    oUpd.addParam("ScheduleWeekDay", strWeekDay, "post");
    //GCH - #36012 - Internationalisation - Planning
    oUpd.addParam("ScheduleRangeBegin", eDate.ConvertDisplayToBdd(strBeginDate), "post");
    oUpd.addParam("ScheduleRangeEnd", eDate.ConvertDisplayToBdd(strEndDate), "post");
    oUpd.addParam("ScheduleRangeCount", nCount, "post");
    oUpd.addParam("ScheduleHour", Hour, "post");
    oUpd.addParam("Tab", nTab, "post");
    oUpd.addParam("action", "update", "post");

    //S'il y a une erreur, le message via eUpdater Suffit
    oUpd.ErrorCallBack = function () { };

    if (callBackFunction == null)
        callBackFunction = validScheduleTreatment;

    if (arguments.length < 2)
        oUpd.send(callBackFunction);
    else {
        //Si plus d'un argument, on les repasse pour être utilisé par le callback au callback (qui est le 1er argument)
        var args = Array.prototype.slice.call(arguments);
        oUpd.send.apply(oUpd, args)
    }


}

function validScheduleTreatment(oRes) {
    var scheduleId = getXmlTextNode(oRes.getElementsByTagName("scheduleid")[0]);
    top.document.getElementById(calleriframeid).contentWindow.setScheduleId(scheduleId);
}

function KeyPress(e) {
    var key;
    var ie = window.event;

    if (ie) {
        e = window.event;
        key = e.keyCode
    } else {
        if (e.which == 0 && e.keyCode != 0) {
            key = e.keyCode;
        } else {
            key = e.which;
        }
    }

    if (key == KEY_CANCEL) {
        if (ie) {
            e.returnValue = false;
        } else {
            e.preventDefault();
        }
        self.close();
    }

    if (key == KEY_ENTER) {
        if (ie) {
            e.returnValue = false;
        } else {
            e.preventDefault();
        }
        Valid();
    }
}

function trim(strTrim) {
    return strTrim.replace(/(^\s*)|(\s*$)/g, "");
}

function parseDate(strDate) {
    var strDate = trim(strDate);

    var aFullDate = strDate.split(" ");

    var strFullDate = aFullDate[0];

    var strFullDate = strFullDate.replace(/\./g, "/");
    var strFullDate = strFullDate.replace(/\-/g, "/");


    if ((strFullDate.length == 4 || strFullDate.length == 8 || strFullDate.length == 6) && strFullDate.indexOf('/') == -1)
        strFullDate = strFullDate.substring(0, 2) + '/' + strFullDate.substring(2, 4) + '/' + strFullDate.substring(4, strFullDate.length);


    var aDate = strFullDate.split("/");

    var strDay = '';
    var strMonth = '';
    var strYear = '';

    if (aDate.length >= 1)
        strDay = aDate[0];
    if (aDate.length >= 2)
        strMonth = aDate[1];
    if (aDate.length >= 3) {
        strYear = aDate[2];
        if (strYear.length > 4)
            strYear = strYear.substring(0, 4);
    }

    if (parseInt(strDay, 10) < 10 && strDay.length == 1)
        strDay = '0' + parseInt(strDay, 10);


    if (parseInt(strMonth, 10) < 10 && strMonth.length == 1)
        strMonth = '0' + parseInt(strMonth, 10);


    if (isNaN(strDay) || isNaN(strMonth) || (strDay == '' || strMonth == '' || parseInt(strDay, 10) > 31 || parseInt(strMonth, 10) < 1 || parseInt(strMonth, 10) > 12) || strYear.length == 3) {
        strDate = '';
    }


    else if (strYear == '' || !strYear) {
        var dCurrentDate = new Date();
        strDate = strDay + '/' + strMonth + '/' + dCurrentDate.getFullYear();
    }
    else if (parseInt(strYear, 10) < 100) {
        if (strYear.length == 1)
            strYear = '0' + strYear;

        if (parseInt(strYear, 10) > 50)
            var strCurrentYear = '19';
        else
            var strCurrentYear = '20';
        strDate = strDay + '/' + strMonth + '/' + strCurrentYear + strYear;
    }
    else if ((parseInt(strYear, 10) < 1753 || parseInt(strYear, 10) > 9999) || isNaN(strYear)) {
        strDate = '';
    }
    else {
        strDate = strDay + '/' + strMonth + '/' + strYear;
    }

    if (aFullDate.length == 2)
        strDate = strDate + ' ' + aFullDate[1];
    return (strDate);
}


function IsDate(id) {

    var oItem = document.getElementById(id);
    var strDate = oItem.value;

    // CRU #45801 : isDate -> isDateJS
    return eValidator.isDateJS(strDate);

    //return strDate != '';
}

// On cherche à modifier la rubrique date de debut de la fiche planning
function onChangeParentDate(strBeginDate) {
    // Si pas d'élément "calleriframeid", on ne viens pas d'un fiche planning
    var oFrame = top.document.getElementById(calleriframeid);
    if (!oFrame || oFrame == "undefined")
        return;

    // Fiche planning
    var oParentDoc = oFrame.contentWindow;

    var descIdBeginDate = Number(nTab) + 2;
    var beginDateIdDom = "COL_" + nTab + "_" + descIdBeginDate + "_" + fileId + "_" + fileId + "_0";
    var beginDateId = "COL_" + nTab + "_" + descIdBeginDate + "_D_" + fileId + "_" + fileId + "_0";
    var beginHourId = "COL_" + nTab + "_" + descIdBeginDate + "_H_" + fileId + "_" + fileId + "_0";

    var descIdEndDate = Number(nTab) + 89;
    var endDateIdDom = "COL_" + nTab + "_" + descIdEndDate + "_" + fileId + "_" + fileId + "_0";
    var endDateId = "COL_" + nTab + "_" + descIdEndDate + "_D_" + fileId + "_" + fileId + "_0";
    var endHourId = "COL_" + nTab + "_" + descIdEndDate + "_H_" + fileId + "_" + fileId + "_0";


    var oParentBeginDate = oParentDoc.document.getElementById(beginDateIdDom);
    var oParentEndDate = oParentDoc.document.getElementById(endDateIdDom);

    var oParentBeginDateD = oParentDoc.document.getElementById(beginDateId);
    var oParentEndDateD = oParentDoc.document.getElementById(endDateId);

    var oParentBeginDateH = oParentDoc.document.getElementById(beginHourId);
    var oParentEndDateH = oParentDoc.document.getElementById(endHourId);

    //Durée du rendez-vous
    var nMinuteDiff = 0;
    if (bAppointment)
        nMinuteDiff = dateDiff('n', eDate.Tools.GetDateFromString(oParentBeginDate.value), eDate.Tools.GetDateFromString(oParentEndDate.value));

    if (strBeginDate.substring(0, 6) == "29/02/") {
        //	
        //JavaScript NE GERE PAS LES ANNEES BISEXTILES ---> 29/02 = 01/03
        //

        var strYear = eDate.Tools.GetDateFromString(strBeginDate).getFullYear();

        var strEndDate = "29/02/" + strYear;

        var nHour = nMinuteDiff / 60;
        var nMinuteDiff = nMinuteDiff - (nHour * 60);

        var strBeginHour = (oParentBeginDateH.value).substring(0, 2);
        if (strBeginHour.substring(0, 1) == 0)
            strBeginHour = strBeginHour.substring(1, 2);
        var nHour = parseInt(strBeginHour) + nHour;

        var strBeginMinute = (oParentBeginDateH.value).substring(3, 5);
        if (strBeginMinute.substring(0, 1) == 0)
            strBeginMinute = strBeginMinute.substring(1, 2);
        var nMinute = parseInt(strBeginMinute) + nMinuteDiff;

        //Seulement Jour
        oParentBeginDateD.value = strEndDate;
        if (bAppointment) oParentEndDateD.value = strEndDate;

        //Date de fin
        strEndDate = strEndDate + ' ' + eDate.Tools.MakeTwoDigit(nHour) + ':' + eDate.Tools.MakeTwoDigit(nMinute) + ':00';
    }
    else {
        //Date de fin
        if (bAppointment)
            var strEndDate = eDate.Tools.GetStringFromDate(dateAdd('n', nMinuteDiff, eDate.Tools.GetDateFromString(strBeginDate)));

        //Les jours sans les heures
        var StartDateD = eDate.Tools.GetStringFromDate(eDate.Tools.GetDateFromString(strBeginDate), true, false);
        oParentBeginDateD.value = eDate.ConvertBddToDisplay(StartDateD);
        if (bAppointment) {
            var endDate = eDate.Tools.GetStringFromDate(eDate.Tools.GetDateFromString(strEndDate), true, false);
            oParentEndDateD.value = eDate.ConvertBddToDisplay(endDate);
        }
    }

    //Les dates   
    oParentBeginDate.value = eDate.ConvertBddToDisplay(strBeginDate);
    if (bAppointment) {
        oParentEndDate.value = eDate.ConvertBddToDisplay(strEndDate);
    }


    //Les heures
    //oParentBeginDateH.value = eDate.Tools.GetStringFromDate( eDate.Tools.GetDateFromString(strBeginDate), false, true ).substring(0,5);
    //oParentEndDateH.value = eDate.Tools.GetStringFromDate( eDate.Tools.GetDateFromString(strEndDate), false, true ).substring(0,5);
    document.getElementById('RangeBegin').value = eDate.ConvertBddToDisplay(strBeginDate);
}

function setScheduleOption() {
    var sel = document.getElementById("ScheduleTypeList");
    if (sel.selectedIndex < 0)
        sel.selectedIndex = 0;

    var selValue = sel.options[sel.selectedIndex].value;

    for (var i = 0; i < 4; i++) {
        if (sel.options[i].value == selValue)
            document.getElementById(sel.options[i].value + "_DIV").style.display = "block";
        else
            document.getElementById(sel.options[i].value + "_DIV").style.display = "none";
    }

    //Si mode une fois, on passe le nombre d'occurence à 1  et on masque date de fin
    if (selValue == "once") {

        if (!document.getElementById('RangeCountChk').checked)
            document.getElementById('RangeCountChk').click();

        document.getElementById("RangeCount").value = 1;

        document.getElementById("RangeEndDate").value = "";

        //Masque les ligne des dates
        document.getElementById("trRangeEnd").style.display = "none";
        document.getElementById("trRangeCount").style.display = "none";


        document.getElementById('daily_day').value = 1;



    }
    else {

        document.getElementById("trRangeEnd").style.display = "";
        document.getElementById("trRangeCount").style.display = "";
    }

}

function getNbDayOfMonth(nMonth, nCurrentYear) {
    var nDayOfMonth;
    if (nMonth == 2) {
        //Gestion des annees bisextiles
        if (nCurrentYear % 4 == 0) {
            nDayOfMonth = 29;
        }
        else {
            nDayOfMonth = 28;
        }
    }
    else {
        nDayOfMonth = dateAdd('d', -1, eDate.Tools.GetDateFromString('01/' + eDate.Tools.MakeTwoDigit(nMonth + 1) + '/' + nCurrentYear)).getDate();
    }
    return nDayOfMonth;
}

//Retourne le numero de la semaine
function week(d) {
    w = d.getDay()
    return Math.floor((yearday(d) - 1 - w) / 7) + 2
}

//Retourne le jour de l'annee (1 à 366)
function yearday(d) {
    var d1 = new Date(d);
    d1.setMonth(0); d1.setDate(1)
    return Math.round((d.getTime() - d1.getTime()) / (24 * 3600000)) + 1
}

// Compare deux dates et retourne le nombre de jours
function dateDiff(strInterval, dDateBegin, dDateEnd) {
    var d = (dDateEnd.getTime() - dDateBegin.getTime()) / 1000
    switch (strInterval.toLowerCase()) {
        case "yyyy": d /= 12
        case "m": d *= 12 * 7 / 365.25
        case "ww": d /= 7
        case "d": d /= 24
        case "h": d /= 60
        case "n": d /= 60
    }
    //return Math.round( d );
    return parseInt(d, 10);
}

function selectDate(id) {

    modalDate = createCalendarPopUp("onCalendarValid", 1, 1, top._res_5017, top._res_5003, "onCalendarOk", top._res_29, null, null, _parentIframeId, id, document.getElementById(id).value);
    modalDate.activeInputId = id;
}

function selectDateBegin(id) {

    modalDate = createCalendarPopUp("onCalendarValidBegin", 1, 1, top._res_5017, top._res_5003, "onCalendarOk", top._res_29, null, null, _parentIframeId, id, document.getElementById(id).value);
    modalDate.activeInputId = id;
}

function selectDateEnd(id) {

    modalDate = createCalendarPopUp("onCalendarValidEnd", 1, 1, top._res_5017, top._res_5003, "onCalendarOk", top._res_29, null, null, _parentIframeId, id, document.getElementById(id).value);
    modalDate.activeInputId = id;
}
function onCalendarOk() {
    //  alert("ici");
}

function onCalendarValidBegin(date) {
    onCalendarValid(date, true);
}

function onCalendarValidEnd(date) {
    onCalendarValid(date, false);
}

function onCalendarValid(date, isBegin) {

    // ASY : #31560 [bug] - Rdv Récurrent - Ecran de paramètres - Choix d'une date de fin - Heure de fin
    // MOU : #38072 [bug] - Réinitialisation des dates et heures à la création d'un RDV récurrent

    if (nMainScheduleType && nMainScheduleType.value == "0") {
        //Seulement pour les schedule planing
        var descIdHour;
        if (isBegin != undefined && isBegin == true) {
            descIdHour = Number(nTab) + 02;
        } else {
            descIdHour = Number(nTab) + 89;
        }
        var endHourId = "COL_" + nTab + "_" + descIdHour + "_H_" + fileId + "_" + fileId + "_0";
        var oParentDoc = top.document.getElementById(calleriframeid).contentWindow;
        var oParentEndDateH = oParentDoc.document.getElementById(endHourId);

        date = date + " " + oParentEndDateH.value;
    }

    document.getElementById(modalDate.activeInputId).value = date;
    modalDate.hide();
}
/* Ajoute un interval a une date et retourne la date
d Day 
h Hour 
n Minute 
s Second 
*/
function dateAdd(strInterval, lNumber, dDate) {


    var dReturnDate = new Date(dDate);



    switch (strInterval.toLowerCase()) {
        case "h":
            dReturnDate.setHours(dDate.getHours() + lNumber);
            break;
        case "n":
            dReturnDate.setMinutes(dDate.getMinutes() + lNumber);
            break;
        case "s":
            dReturnDate.setSeconds(dDate.getSeconds() + lNumber);
            break;
        default:
            dReturnDate.setDate(dDate.getDate() + lNumber);
    }

    return dReturnDate;
}


