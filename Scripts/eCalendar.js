// ASY : Pour rendre CalDate independant
// jsVarName : doit etre unique par instance de calendrier
function eCalendar(jsVarName, modalVarName, strDateInitVal, userDate, nHideHourFieldVal, nHideNoDateVal, divTopVal, divBottomVal, strHour, strMin) {
    that = this;
    this.jsVarName = jsVarName;
    this._modalVarName = modalVarName;


    // ASY : identifie les Elements du calendrier par des Id unique
    this.calendarKey = jsVarName;

    this.oDoc = document;

    // Objets
    this.calDate = new Date();    // date du jour


    this.calControlID = this.calendarKey + "calControl";
    //this.calControlobj = document.getElementById(this.calControlID);

    this.WeekNumberID = this.calendarKey + "weeknumber";
    this.YearID = this.calendarKey + "Year";
    this.MonthID = this.calendarKey + "Month";
    this.DaysID = this.calendarKey + "Day";

    // TODO
    this.HourID = this.calendarKey + "Hour";
    this.MinID = this.calendarKey + "Min";

    // Option pour cacher les heures
    this.nHideHourField = nHideHourFieldVal;
    this.nHideNoDate = nHideNoDateVal;
    this.strDateInit = strDateInitVal;

    this.divTop = divTopVal;
    this.divBottom = divBottomVal;

    /***************************  METHODES DE L OBJETS CALENDAR ****************/


    this.SetInitialDate = function (dDate) {
        var ddmmyyyy = dDate;
        //eval (ddmmyyyy);
        var dd = ddmmyyyy.substring(0, 2);
        var mm = ddmmyyyy.substring(3, 5);
        var yyyy = ddmmyyyy.substring(6, 10);

        var mmddyyyy = mm + '/' + dd + '/' + yyyy;

        this.calDate = new Date(mmddyyyy);
        if (isNaN(this.calDate)) {
            this.calDate = new Date();
        }
        this.calDay = this.calDate.getDate();

    };

    this.SetDateField = function (dDate) {
        this.SetInitialDate(dDate);
        this.calDocTop = this.buildTopCalFrame(userDate);
        this.calDocBottom = this.BuildBottomCalFrame();
    };

    this.GetTimeCode = function () {
        if (this.nHideHourField.toString() != "1") {
            var strReturn = '<div class="calendarhourmin"> ' + top._res_767 + '&#160;';
            strReturn += '<input maxlength=2 id="' + this.HourID + '" class=calendarhour value="' + (strHour == "" ? "HH" : strHour) + '" onfocus="' + this.calendarKey + '.HMfocus(this) ;">&#160;:&#160;<input id="' + this.MinID + '" class=calendarmin value="' + (strMin == "" ? "MM" : strMin) + '" maxlength=2 onfocus="' + this.calendarKey + '.HMfocus(this) ;">';
            strReturn += '</div>';
            return strReturn;
        }
        return "";
    };

    this.BuildBottomCalFrame = function () {
        var calDoc = this.calendarBegin;
        var nMonth = this.calDate.getMonth();
        var nYear = this.calDate.getFullYear();
        var nDay = this.calDay;
        var i = 0;
        var days = this.getDaysInMonth();

        if (nDay > days) {
            nDay = days;
        }
        var firstOfMonth = new Date(nYear, nMonth, 1);
        var startingPos = firstOfMonth.getDay();
        if (startingPos == 0)
            startingPos = 6;
        else
            startingPos--;
        days += startingPos;
        var columnCount = 0;
        for (i = 0; i < startingPos; i++) {
            calDoc += this.blankCell;
            columnCount++;
        }
        var currentDay = 0;
        var nCurrWeek;
        for (i = startingPos; i < days; i++) {
            currentDay = i - startingPos + 1;

            nCurrWeek = DefSemaineNum(nYear, nMonth, currentDay);

            var onDoubleClick = this.calendarKey + ".SelectDate(" + currentDay + "); " + this.calendarKey + ".Valid();";
            var oModal = null;
            var strModal = "";
            //********Gestion du double click
            if (that._modalVarName != "") {
                oModal = window[that._modalVarName];
                strModal = 'window["' + that._modalVarName + '"]';
                if (!oModal || !oModal.CallOnOk) {
                    oModal = parent[that._modalVarName];
                    strModal = 'parent["' + that._modalVarName + '"]';
                }
                if (oModal && oModal.CallOnOk)
                    onDoubleClick = this.calendarKey + '.SelectDate(' + currentDay + ');' + strModal + '.CallOnOk("' + that._modalVarName + '");';
            }
            //*******************************
            if (currentDay == userDate.substring(0, 2) && nMonth + 1 == userDate.substring(3, 5) && nYear == userDate.substring(6, 10)) {
                calDoc += "<td class='day today' originalclassname='day today' onmouseover=\"document.getElementById('" + this.WeekNumberID + "').innerHTML=" + nCurrWeek + ";this.className='day today todayhover';\" onmouseout=\"this.className='day today';\" title=\"" + top._res_143 + "\" onclick='" + this.calendarKey + ".SelectDate(" + currentDay + ");' ondblclick='" + onDoubleClick + "'>" + currentDay + "</td>";
            }
            else if (currentDay == nDay && nMonth + 1 == (this.strDateInit).substring(3, 5) && nYear == (this.strDateInit).substring(6, 10)) {
                calDoc += "<td id='" + this.DaysID + currentDay + "' class='day selectedday' originalclassname='day selectedday' onmouseover=\"document.getElementById('" + this.WeekNumberID + "').innerHTML=" + nCurrWeek + ";if (this.className!='day preselectedday'){ this.className='day selectedday selecteddayhover'};\" onmouseout=\"if (this.className!='day preselectedday'){ this.className='day selectedday';}\" onclick='" + this.calendarKey + ".SelectDate(" + currentDay + ");' ondblclick='" + onDoubleClick + "'>" + currentDay + "</td>";
            }
            else if ((columnCount % 7) == 6 || (columnCount % 7) == 5) {
                calDoc += "<td id='" + this.DaysID + currentDay + "' class='day weekendday' originalclassname='day weekendday' onmouseover=\"document.getElementById('" + this.WeekNumberID + "').innerHTML=" + nCurrWeek + ";if (this.className!='day preselectedday'){ this.className='day weekendday weekenddayhover'};\" onmouseout=\"if (this.className!='day preselectedday'){ this.className='day weekendday';}\" onclick='" + this.calendarKey + ".SelectDate(" + currentDay + ");' ondblclick='" + onDoubleClick + "'>" + currentDay + "</td>";
            }
            else {
                calDoc += "<td id='" + this.DaysID + currentDay + "' class='day weekday' originalclassname='day weekday' onmouseover=\"document.getElementById('" + this.WeekNumberID + "').innerHTML=" + nCurrWeek + ";if (this.className!='day preselectedday'){ this.className='day weekday weekdayhover'};\" onmouseout=\"if (this.className!='day preselectedday'){ this.className='day weekday';}\" onclick='" + this.calendarKey + ".SelectDate(" + currentDay + ");' ondblclick='" + onDoubleClick + "'>" + currentDay + "</td>";
            }


            columnCount++;
            if (columnCount % 7 == 0) {
                calDoc += "</tr><tr>";
            }
        }

        var nLastFullCell = columnCount;
        for (i = days; i < 42; i++) {

            calDoc += this.blankCell;
            columnCount++;
            if (columnCount % 7 == 0) {


                calDoc += "</tr>";
                if (i < 41) {
                    calDoc += "<tr>";
                }
            }
        }

        calDoc += this.calendarEnd + "</table>" + this.GetTimeCode();

        return calDoc;
    };

    var DefSemaineNum = function (aaaa, mm, jj) {
        //initialisation des variables
        //----------------------------
        var MaDate = new Date(aaaa, mm, jj); //date a traiter
        var annee = MaDate.getFullYear(); //année de la date à traiter
        var NumSemaine = 0, //numéro de la semaine

        // calcul du nombre de jours écoulés entre le 1er janvier et la date à traiter.
        // ----------------------------------------------------------------------------
        // initialisation d'un tableau avec le nombre de jours pour chaque mois
        ListeMois = new Array(31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31);
        // si l'année est bissextile alors le mois de février vaut 29 jours
        if (annee % 4 == 0 && annee % 100 != 0 || annee % 400 == 0) { ListeMois[1] = 29 };
        // on parcours tous les mois précédants le mois à traiter 
        // et on calcul le nombre de jour écoulé depuis le 1er janvier dans TotalJour
        var TotalJour = 0;
        for (cpt = 0; cpt < mm; cpt++) { TotalJour += ListeMois[cpt]; }
        TotalJour += jj;

        //Calcul du nombre de jours de la première semaine de l'année à retrancher de TotalJour
        //-------------------------------------------------------------------------------------
        //on initialise dans DebutAn le 1er janvier de l'année à traiter
        DebutAn = new Date(annee, 0, 1);
        //on determine ensuite le jour correspondant au 1er janvier
        //de 1 pour un lundi à 7 pour un dimanche/
        var JourDebutAn;
        JourDebutAn = DebutAn.getDay();
        if (JourDebutAn == 0) { JourDebutAn = 7 };

        //Calcul du numéro de semaine
        //----------------------------------------------------------------------
        //on retire du TotalJour le nombre de jours que dure la première semaine 
        TotalJour -= 8 - JourDebutAn;
        //on comptabilise cette première semaine
        NumSemaine = 1;
        //on ajoute le nombre de semaine compléte (sans tenir compte des jours restants)
        NumSemaine += Math.floor(TotalJour / 7);
        // s'il y a un reste alors le n° de semaine est incrémenté de 1
        if (TotalJour % 7 != 0) { NumSemaine += 1 };


        if (JourDebutAn >= 5) {
            if (NumSemaine > 1)
                NumSemaine = NumSemaine - 1
            else {
                var naaaa = aaaa - 1;
                NumSemaine = DefSemaineNum(naaaa, 11, 31);
            }
        }


        return (NumSemaine);
    };

    this.WriteCalendar = function () {
        this.calDocBottom = this.BuildBottomCalFrame();
        this.divBottom.innerHTML = this.calDocBottom;
        if (document.getElementById("'" + this.WeekNumberID + "'")) {
            document.getElementById("'" + this.WeekNumberID + "'").innerHTML = this.DefSemaineNum(this.calDate.getFullYear(), this.calDate.getMonth(), 1);
        }
    };

    this.getDaysInMonth = function () {
        var nMonth = this.calDate.getMonth() + 1;
        var nYear = this.calDate.getFullYear();
        var nNbreDays = 0;

        if (nMonth == 4 || nMonth == 6 || nMonth == 9 || nMonth == 11)
            nNbreDays = 30;
        else if (nMonth == 2) {
            if (((nYear % 4) == 0) && ((nYear % 100) != 0) || ((nYear % 400) == 0))
                nNbreDays = 29;
            else
                nNbreDays = 28;
        }
        else
            nNbreDays = 31;

        return nNbreDays;
    };

    this.createWeekdayList = function () {
        var aWeekday = top._res_468.split(",");
        var strReturn = "<tr>";
        for (var i = 0; i < aWeekday.length; i++) {
            strReturn += "<td class='weekdayheader'>" + aWeekday[i].replace("'", "").replace("'", "") + "</td>";
        }

        strReturn += "</tr>";
        strReturn = "<tr><td class='weekdayheader' colspan='7' >" + top._res_821 + " <span id='" + this.WeekNumberID + "'></span></td></tr>" + strReturn;
        return strReturn;
    };

    this.buildCalParts = function () {
        this.strWeekDays = this.createWeekdayList();
        this.blankCell = "<td class='day emptyday' onmouseover=\"this.className='day emptyday emptydayhover';\" onmouseout=\"this.className='day emptyday';\">&#160;</td>";
        this.calendarBegin =
        "<html>" +
        "<body id='calendarbody'>" +
        "<table class='calendardays'>" + this.strWeekDays + "<tr>";

        this.calendarEnd = "";

        if (this.nHideNoDate.toString() != "1") {

            this.calendarEnd = "<tr><td colspan=7 class='emptydate' onclick='" + this.calendarKey + ".setNoDate();'><a href=\"#\">" + top._res_314 + "</a></td></tr>"

        }

        this.calendarEnd +=
        "</body>" +
        "</html>";

    };

    this.BuildCalendar = function () {

        this.buildCalParts();

        this.SetDateField(this.strDateInit);
        this.divTop.innerHTML = this.calDocTop;
        this.divBottom.innerHTML = this.calDocBottom;

        //this.divTop.innerHTML = "  <button onclick='alert(eCalendarControlStart.SelectDate)'>pouet</button>";

        if (document.getElementById(this.WeekNumberID)) {
            document.getElementById(this.WeekNumberID).innerHTML = DefSemaineNum(this.calDate.getFullYear(), this.calDate.getMonth(), 1);
        }
    };


    this.buildTopCalFrame = function (userDate) {
        
        var paramWin = top.getParamWindow();

        var inputWidth = 45;

        var cssClass = top.checkAdminMode() ? ' calendar__admin':''

        if (paramWin) {
            var objThm = JSON.parse(paramWin.GetParam("currenttheme").toString());
            //ELAIZ - régression 76662 - changement de la largeur du champs en fonction du thème et du navigateur
            var browser = new getBrowser();
            if (objThm.Version > 1 && browser.isChrome){
                inputWidth = 120
                cssClass += ' chromeBrowser'
            }
            else if (objThm.Version > 1 && !browser.isChrome){
                inputWidth = 100
            }
        }
           

        var cToday = new Date();
        var d = cToday.getDate();
        var m = cToday.getMonth() + 1;
        if (m < 10) {
            m = '0' + m;
        }
        var y = cToday.getFullYear();
        DateduJour = d + '/' + m + '/' + y;

        var calDoc;
        calDoc =
            "<form name='" + this.calControlID + "' onSubmit='return false;' class='" + cssClass + "'" + ">" +
            "<div class='calendartoday onClick='" + this.calendarKey + ".setToday(\"" + userDate + "\");'>" + top._res_143 + "</div>" +
            "<div class=calendarselect><table class=underCal>" +
            "<tr class='calendarMonth' >" +
            "<td class='icon-edn-prev icnListAct' title=\"" + top._res_136 + "\" onClick='" + this.calendarKey + ".setPreviousMonth();'></td>" + //todo séparateur
            "<td class=calMonth>" + this.getMonthSelect() + "</td>" +
            "<td class='icon-edn-next icnListAct' title=\"" + top._res_137 + "\" onClick='" + this.calendarKey + ".setNextMonth();'></td>" +
        "<td colspan=4><input id='" + this.YearID + "' name='" + this.YearID + "' class=calYear style='width:" + inputWidth + "px' value='" + this.calDate.getFullYear() + "' type=text maxlength=4 onKeyUp='" + this.calendarKey + ".setYear()'></input></td>" +
            "</tr>" +
            "</table></div>" +
            "</form>" +
            "</body>" +
            "</html>";

        return calDoc;
    }

    this.getMonthSelect = function () {
        var aMonth = top._res_2391.split(",");
        var nActiveMonth = this.calDate.getMonth();
        var strReturn = "<select id='" + this.MonthID + "' name='month' style='width:65px;' onChange='" + this.calendarKey + ".setCurrentMonth()'>";
        for (i = 0; i < aMonth.length; i++) {
            var sThisMonth = (aMonth[i] + "").replace(/'/g, "");
            strReturn += "<option" + (i == nActiveMonth ? " selected" : "") + ">" + sThisMonth + "</option>\n";   //+"" sur le replace pour bug IE7
        }
        strReturn += "</select>";
        return strReturn;
    }

    this.returnDate = function (inDay) {

        if (inDay == null) {
            this.SetReturnValue('');
        }
        else {

            outDate = this.GetDate();

            this.SetReturnValue(outDate);
        }

    }

    this.GetDate = function () {

        var nHideHourField = this.nHideHourField;

        var outDate = "";
        outDate = eDate.Tools.GetStringFromDate(this.calDate, true, false);

        //SPH : Si pas d'heure, on affiche pas 00:00
        if (document.getElementById(this.HourID) && document.getElementById(this.MinID)) {
            if ((isNumeric(document.getElementById(this.HourID).value) && document.getElementById(this.HourID).value >= 0 && document.getElementById(this.HourID).value <= 23)
            && (isNumeric(document.getElementById(this.MinID).value) && document.getElementById(this.MinID).value >= 0 && document.getElementById(this.MinID).value <= 59)
            )
                nHideHourField = "0";

            else
                nHideHourField = "1";
        }


        if (nHideHourField.toString() != "1") {

            var strHour = eDate.Tools.MakeTwoDigit(trim(document.getElementById(this.HourID).value));
            var strMin = eDate.Tools.MakeTwoDigit(trim(document.getElementById(this.MinID).value));
            if (trim(document.getElementById(this.HourID).value) != "" || trim(document.getElementById(this.MinID).value) != "") {
                nHour = parseInt(strHour, 10);
                nMin = parseInt(strMin, 10);
                if (isNaN(nHour))
                    nHour = -1;
                if (isNaN(nMin))
                    nMin = 0;
                bDateFailed = (nHour < 0 || nHour > 23 || nMin < 0 || nMin > 59);

                if (bDateFailed) {
                    eAlert(0, top._res_6275, top._res_470, '', 500, 200, new function () { document.getElementById(this.HourID).select(); document.getElementById(this.HourID).focus(); });
                    return "";
                }
                else
                    outDate += ' ' + eDate.Tools.MakeTwoDigit(nHour) + ':' + eDate.Tools.MakeTwoDigit(nMin);
            }

        }
        //GCH - #36019 - Internationnalisation - Choix de dates
        return eDate.ConvertBddToDisplay(outDate);
    }


    this.SelectDate = function (inDay, redraw) {

        this.calDate.setDate(inDay);
        this.calDay = this.calDate.getDate();

        var currMonth = document.getElementById(this.MonthID);
        var month = currMonth.selectedIndex;

        this.calDate.setMonth(month);
        var strNewDate = eDate.Tools.GetStringFromDate(this.calDate, true, false);

        this.strDateInit = strNewDate;

        if (redraw) {
            this.calDocTop = this.buildTopCalFrame(this.strDateInit);
            this.WriteCalendar();
        }

        var oTd = document.getElementById(this.DaysID + inDay);
        if (oTd) {
            oTd.className = "day preselectedday";
        }
        for (i = 1; i <= 31; i++) {
            var oTd = document.getElementById(this.DaysID + i);
            if (oTd) {
                if (i != inDay) {
                    oTd.className = oTd.getAttribute("originalclassname");
                }
            }

        }

    }

    this.setToday = function (userDate) {
        var ddmmyyyy = userDate;
        var dd = ddmmyyyy.substring(0, 2);
        var mm = ddmmyyyy.substring(3, 5);
        var yyyy = ddmmyyyy.substring(6, 10);
        var mmddyyyy = mm + '/' + dd + '/' + yyyy;
        this.calDate = new Date(mmddyyyy);

        var currMonth = document.getElementById(this.MonthID);
        currMonth.selectedIndex = this.calDate.getMonth();

        var currYear = document.getElementById(this.YearID);
        currYear.value = this.calDate.getFullYear();

        this.SelectDate(this.calDate.getDate(), true);
    }


    this.setNoDate = function () {
        this.returnDate(null);
    }

    this.setYear = function () {
        var currYear = document.getElementById(this.YearID);
        var nYear = currYear.value;

        if (isNaN(nYear))
            return;
        this.calDate.setFullYear(nYear);
        this.WriteCalendar();
    }

    this.setCurrentMonth = function () {
        var currMonth = document.getElementById(this.MonthID);
        var nMonth = currMonth.selectedIndex;

        this.calDate.setDate(1);
        this.calDate.setMonth(nMonth);
        this.WriteCalendar();
    }

    this.setPreviousYear = function () {
        var currYear = document.getElementById(this.YearID);
        var nYear = currYear.value;
        if (isNaN(nYear))
            return;
        if (nYear < 1000)
            return;
        nYear--;
        this.calDate.setFullYear(nYear);
        currYear.value = nYear;
        this.WriteCalendar();
    }


    this.setPreviousMonth = function () {
        var currYear = document.getElementById(this.YearID);
        var currMonth = document.getElementById(this.MonthID);
        var nYear = currYear.value;
        if (isNaN(nYear))
            return;
        var nMonth = currMonth.selectedIndex;
        if (nMonth == 0) {
            nMonth = 11;
            nYear--;
            this.calDate.setFullYear(nYear);
            currYear.value = nYear;
        }
        else {
            nMonth--;
        }

        this.calDate.setDate(1);
        this.calDate.setMonth(nMonth);
        currMonth.selectedIndex = nMonth;
        this.WriteCalendar();
    }

    this.setNextMonth = function () {
        var currYear = document.getElementById(this.YearID);
        var currMonth = document.getElementById(this.MonthID);
        var nYear = currYear.value;
        if (isNaN(nYear))
            return;
        var nMonth = currMonth.selectedIndex;
        if (nMonth == 11) {
            nMonth = 0;
            nYear++;
            this.calDate.setFullYear(nYear);
            currYear.value = nYear;
        }
        else {
            nMonth++;
        }

        this.calDate.setDate(1);
        this.calDate.setMonth(nMonth);
        currMonth.selectedIndex = nMonth;
        this.WriteCalendar();
    }

    this.setNextYear = function () {
        var currYear = document.getElementById(this.YearID);
        var nYear = currYear.value;
        if (isNaN(nYear))
            return;
        nYear++;
        this.calDate.setFullYear(nYear);
        currYear.value = nYear;
        this.WriteCalendar();
    }

    this.HMfocus = function (input) {
        if ((input.value == "HH") || (input.value == "MM")) {
            input.value = "";

        }
    }

    this.Valid = function () {
        this.returnDate(this.calDate.getDate());
    }

    this.trim = function (strTrim) {
        return strTrim.replace(/(^\s*)|(\s*$)/g, "");
    }

    //retourne la date sélectionnée
    this.SetReturnValue = function (strFinalDate) {


        if (typeof strParentJsValidFunction != "undefined" && strParentJsValidFunction != "") {
            var myFct = parentFrame[strParentJsValidFunction];
            if (typeof (myFct) == 'function') {

                if (typeof frmId == "undefined")
                    var frmId = null;

                myFct(strFinalDate, operator, nodeId, frmId);
                strParentJsValidFunction = "";
            }
        }

    }


}  /*********************** Fin eCalendar *****************/







