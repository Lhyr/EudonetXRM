var nsPickADate = {};


nsPickADate.loadCalendar = function () {
    var strDate = document.getElementById("date").value;
    var userDate = document.getElementById("userdate").value;
    var strHour = document.getElementById("hour").value;
    var strMin = document.getElementById("min").value;
    var divTop = document.getElementById("topCalDiv");
    var divBottom = document.getElementById("bottomCalDiv");
    nsPickADate.Calendar = new eCalendar("nsPickADate.Calendar", null, strDate, userDate, '0', '1', divTop, divBottom, strHour, strMin);
    nsPickADate.Calendar.BuildCalendar();
};


nsPickADate.getReturnValue = function () {

    var strDbv = "";
    var radioObj = document.forms['radioOptForm'].elements['date'];
    for (var i = 0; i < radioObj.length; i++) {
        if (radioObj[i].checked) {
            strDbv = radioObj[i].value;
            break;
        }
    }


    if (strDbv == "<NONE>") {
        return { dbv: "", disp: "" };
    }
    else if (strDbv == "<CALENDAR>") {

        //Calendrier
        //GCH - #36019 - Internationnalisation - Choix de dates
        outDate = eDate.Tools.GetStringFromDate(nsPickADate.Calendar.calDate, true, false);
        var oHour = document.getElementById(nsPickADate.Calendar.HourID);
        var oMin = document.getElementById(nsPickADate.Calendar.MinID);
        var nHour;
        var nMin;

        var bNoHour = false;

        if (!(oMin && oHour))
            bNoHour = true;


        if (!bNoHour) {
            nHour = getNumber(oHour.value);
            nMin = getNumber(oMin.value);

            if (isNaN(nHour) || isNaN(nMin))
                bNoHour = true;
        }

        if (!bNoHour) {

            bDateFailed = (nHour < 0 || nHour > 23 || nMin < 0 || nMin > 59);

            if (bDateFailed) {
                eAlert(0, top._res_6275, top._res_470, '', 500, 200, new function () { document.getElementById(oCalendar.HourID).select(); document.getElementById(oCalendar.HourID).focus(); });
                return;
            }
            else {
                outDate += ' ' + eDate.Tools.MakeTwoDigit(nHour) + ':' + eDate.Tools.MakeTwoDigit(nMin);
            }
        }
        // document.getElementById('FixedDate').value = outDate;
        strDbv = outDate;

    }

    /*else if( document.getElementById('7').checked )
    var strDbv = '<DAY>';*/

    if (document.getElementById('lstMove').value != 0) {
        strDbv = strDbv + ' ' + document.getElementById('lstMove').value;
    }
    var ChkNoYear = document.getElementById('ChkNoYear');
    if (ChkNoYear && ChkNoYear.checked) {
        strDbv = strDbv + '[NOYEAR]';
    }

    var strDisp = nsPickADate.getDisplay(strDbv);
    return { dbv: strDbv, disp: strDisp };

}

nsPickADate.getDisplay = function (sDbv) {

    var tmpDbv = sDbv;
    var sDateDisp = "";
    var strMoveTo = '';
    var strMoveTo2 = '';
    var bNoYear = 0;

    if (tmpDbv.indexOf('[NOYEAR]', 1) > 0) {
        bNoYear = 1;
        tmpDbv = tmpDbv.replace('[NOYEAR]', '');
        strMoveTo2 = " (" + top._res_1495 + ") ";
    }

    if (tmpDbv.indexOf('+', 1) > 0) {
        var aValue = tmpDbv.split('+');
        tmpDbv = aValue[0].replace(' ', '');
        strMoveTo = ' + ' + aValue[1].replace(' ', '');
    }
    else if (tmpDbv.indexOf('-', 1) > 0) {
        var aValue = tmpDbv.split('-');
        tmpDbv = aValue[0].replace(' ', '');
        strMoveTo = ' - ' + aValue[1].replace(' ', '');
    }

    if (tmpDbv.indexOf('<DATE>') >= 0)
        var sDateDisp = "<" + top._res_367 + ">";
    else if (tmpDbv.indexOf('<DATETIME>') >= 0)
        var sDateDisp = "<" + top._res_368 + ">";
    else if (tmpDbv.indexOf('<MONTH>') >= 0)
        var sDateDisp = "<" + top._res_693 + ">";
    else if (tmpDbv.indexOf('<WEEK>') >= 0)
        var sDateDisp = "<" + top._res_694 + ">";
    else if (tmpDbv.indexOf('<YEAR>') >= 0)
        var sDateDisp = "<" + top._res_778 + ">";
    else if (tmpDbv.indexOf('<DAY>') >= 0)
        var sDateDisp = "<" + top._res_1234 + ">";
    else {
        if (bNoYear == 1) { tmpDbv = tmpDbv.substr(0, 10); }
        //GCH - #36019 - Internationnalisation - Choix de dates
        var sDateDisp = eDate.ConvertBddToDisplay(tmpDbv);
    }

    return sDateDisp + strMoveTo + strMoveTo2;
}

nsPickADate.onSelectRadio = function (elt) {
    var _lst = document.getElementById('lstMove');
    var radioId = elt.id;
    var _isMoveOptVisible = false;// = (radioId == 1 || radioId == 4 || radioId == 5 || radioId == 6 || radioId == 7);
    var labelMove = document.getElementById('LabelMove');

    switch (elt.value) {
        case "<DATE>":
            _isMoveOptVisible = true;
            SetText(labelMove, top._res_853);
            break;
        case "<MONTH>":
            _isMoveOptVisible = true;
            SetText(labelMove, top._res_854);
            break;
        case "<WEEK>":
            _isMoveOptVisible = true;
            SetText(labelMove, top._res_852);
            break;
        case "<YEAR>":
            _isMoveOptVisible = true;
            SetText(labelMove, top._res_855);
            break;
        default:
            _isMoveOptVisible = false;
    }

    //on teste s'il s'agit d'un eudotag 
    if (elt.value != "<CALENDAR>") {
        var tdSelectedDay = document.getElementById("bottomCalDiv").querySelector("table.calendardays td.day.preselectedday");
        if (tdSelectedDay)
            tdSelectedDay.className = tdSelectedDay.getAttribute("originalclassname");

        tdSelectedDay = document.getElementById("bottomCalDiv").querySelector("table.calendardays td.day.selectedday");
        if (tdSelectedDay)
            removeClass(tdSelectedDay, "selectedday");

        //document.getElementById("FixedDate").value = "";
    }

    if (!_isMoveOptVisible) {
        //_lst.style.display = 'none';
        _lst.value = 0;
        document.getElementById('DivMove').style.display = 'none';
    }
    else {
        document.getElementById('DivMove').style.display = "block";
        //_lst.style.display = 'block';
    }

    var DivNoYear = document.getElementById('DivNoYear');

    if (DivNoYear) {
        switch (elt.value) {
            case "<DATE>":
            case "<MONTH>":
            case "<WEEK>":
                DivNoYear.style.display = 'block';
                break;
            default:
                document.getElementById('ChkNoYear').checked = 0;
                document.getElementById('chk_ChkNoYear').className = document.getElementById('chk_ChkNoYear').className;
                document.getElementById('chk_ChkNoYear').setAttribute("chk", "0");
                DivNoYear.style.display = 'none';
                break;
        }
    }
}

nsPickADate.onCalOptClick = function () {

    var radioObj = document.querySelectorAll("input[type='radio'][name='date']");

    for (var i = 0; i < radioObj.length; i++) {
        radioObj[i].checked = false;
        radioObj[i].removeAttribute("checked");
    }
    var radioFixedDate = document.getElementById("FixedDate");
    radioFixedDate.checked = true;
    nsPickADate.onSelectRadio(radioFixedDate);


}
