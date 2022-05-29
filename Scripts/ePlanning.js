/*necessite : emodaldialog.js, etool.js, emodaldialog.css*/

var SEPARATOR_LVL1 = "#|#";
var SEPARATOR_LVL2 = "#$#";
var VIEW_CAL_LIST = 0;
var VIEW_CAL_DAY = 1;
var VIEW_CAL_TODAY = 2;
var VIEW_CAL_WORK_WEEK = 3;
var VIEW_CAL_TASK = 4;
var VIEW_CAL_DAY_PER_USER = 5;
var VIEW_CAL_MONTH = 6;
var HEADER_HEIGHT = 46; //(voir ePlanning.css)
var HEADER_HOUR_COL_WIDTH = 100; //(voir ePlanning.cs)
var bTabletMode = isTablet();

var _eToolTipModal;    //objet modal tooltip
var _eDateSelectModal; //objet fenêtre modale pour la sélection de date (déplacer vers/dupliquer)
this._fileId = 0;   //id de la fiche cliqué
this._tab = 0;
this._bConfidential = true;

this.debugModePlanning = false; // à mettre à true pour afficher certains messages de diagnostic - NE PAS UTILISER EN PRODUCTION !

// Génère des messages de diagnostic ; uniquement si this.debugModePlanning = true
// ePlanning n'étant pas un objet à proprement parler, on nomme la variable et la fonction avec le suffixe Planning pour les différencier d'éventuelles fonctions et variables
// déjà nommées "debugMode" et "trace" sur d'autres pages/scripts non objet (ex : eList.js)
this.tracePlanning = function (strMessage) {
    if (this.debugModePlanning) {
        try {
            strMessage = 'ePlanning -- ' + strMessage;

            if (typeof (console) != "undefined" && console && typeof (console.log) != "undefined") {
                console.log(strMessage);
            }
            else {
                alert(strMessage); // TODO: adopter une solution plus discrète que alert()
            }
        }
        catch (ex) {

        }
    }
};

/*Permet d'initialiser les infos*/
this.InitRDV = function (oRDV) {
    var retour = true;
    try {
        this._fileId = oRDV.getAttribute("fid");
        if (nGlobalActiveTab)
            this._tab = nGlobalActiveTab;
        this._bConfidential = (oRDV.getAttribute("eCf") == "1");
        retour = retour && (this._fileId != "");
    }
    catch (e) {
        retour = false;
    }
    return retour;
};
//Initialise dans la balise CustomCss les Css dynamique necessaires à l'affichage des RDV.
function initCalCss() {


    var oInput = document.getElementById("CalendarCustomCss");
    if (oInput) {
        var cssList = (oInput.value + "").split(SEPARATOR_LVL1);
        for (var countCss = 0; countCss < cssList.length; countCss++) {
            //cssList[i]
            var sClassName = cssList[countCss].split(SEPARATOR_LVL2)[0];
            var sCss = cssList[countCss].split(SEPARATOR_LVL2)[1];
            createCss('customCss', sClassName, sCss, true);
        }
    }
}
var callBackScrollFct;
this.oRDVtt;   //Dernier id de RDV cliqué
/*Ouvre la ToolTip du planning passé en praramètre au position de l'évennement e de click*/
this.OpenToolTipCalendar = function (e, oRDV) {
    /*event*/
    var targ;
    if (!e)
        e = window.event;

    if (this.InitRDV(oRDV)) {
        var calDiv = document.getElementById("calendarMainHeader");

        /*Fonction ajouté sur le mouvement du scroll du calendrier (MODE JOURS multi USER) pour refermer la tooltip*/
        callBackScrollFct = function () { HideTtModal(); unsetEventListener(calDiv, "scroll", callBackScrollFct); };
        setEventListener(calDiv, "scroll", callBackScrollFct);
        /*******/
        /*Fonction ajouté sur le mouvement du scroll du calendrier (MODE MOIS) pour refermer la tooltip*/
        var CalDivMain = document.getElementById("CalDivMain");
        callBackScrollFct = function () { HideTtModal(); unsetEventListener(CalDivMain, "scroll", callBackScrollFct); };
        setEventListener(CalDivMain, "scroll", callBackScrollFct);
        /*******/

        this.oRDVtt = oRDV;
        try {
            //Clique hors Tooltip 
            setWindowEventListener('click', function (e) { this.onClickHideTtModal(e); });
        }
        catch (Exc) {
        }
        /*Coordonnées de click de la souris sur la page*/
        try {
            var posXY = getClickPositionXY(evt);
        }
        catch (ex) {
            if (this.oicEvtXY) {
                posXY = this.oicEvtXY;
            }
        }
        var left = posXY[0];
        var top = posXY[1];
        top = top - 60; //60 étant la heuteur entre le haut de la tooltip et la pointe de la fheche
        /*******/
        /*Coordonnées du div du RDV sélectionné*/
        //GCH #33929 : il faut en plus ajouter le scroll pour que lorsque l'on est en zoom sur la tablette, il soit compté les dimension de la fenêtre complète
        //CNA #45 771 : ajout de CalDivMain.scrollLeft pour prendre en compte le scroll horizontal en mode jour
        var targX = getAbsolutePositionWithScroll(oRDV).x - CalDivMain.scrollLeft;
        var bLeftOrRight = (getWindowWH()[0] / 2 < left);
        var posXToolTip = targX + ((!bLeftOrRight) ? oRDV.clientWidth : 0);
        /******/
        var strTitre = "";

        var strType = 5;    //ASYNCHRONE       
        var oWidths = {
            [FONTSIZES.xSmall] :320,
            [FONTSIZES.small]:320,
            [FONTSIZES.medium]:320,
            [FONTSIZES.large]:400,
            [FONTSIZES.xLarge]:400,
            [FONTSIZES.xxLarge]:400
        }
        var nUserFontSize = FONTSIZES.medium; // #96 393 : valeur par défaut < 10.803 (correspond à la taille 12 - cf. eMain.js.adjustDivContainer)
        // Il n'y a parfois pas de préférence "Taille de police" sur le profil de l'utilisateur. On utilisera alors la valeur par défaut
        try {
            var paramWindow = window.getParamWindow();
            nUserFontSize = JSON.parse(paramWindow.GetParam("fontsize").toString());
        }
        catch (ex) {
            nUserFontSize = FONTSIZES.medium;
        }
        var nWidth = oWidths[nUserFontSize];
        if (!nWidth)
            nWidth = 320;
        var nHeight = 230;
        var textIcon = '0';
        var strUrl = "mgr/ePlanningManager.ashx";
        if (_eToolTipModal != null) {
            _eToolTipModal.hide();
            _eToolTipModal = null;
        }

        showvcpl(null, 0); //Cacher la vcard

        _eToolTipModal = new eModalDialog(
            strTitre,  // Titre
            strType,   // Type
            strUrl,    // URL
            nWidth,    // Largeur
            nHeight, true);  // Hauteur
        _eToolTipModal.ErrorCallBack = launchInContext(_eToolTipModal, _eToolTipModal.hide);
        _eToolTipModal.noButtons = true;
        _eToolTipModal.addParam("fileid", this._fileId, "post");
        _eToolTipModal.addParam("tab", this._tab, "post");
        _eToolTipModal.addParam("action", "tooltip", "post");
        _eToolTipModal.addParam("divid", oRDV.id, "post");
        _eToolTipModal.addParam("eCf", this._bConfidential ? "1" : "0", "post");
        _eToolTipModal.addParam("iframeScrolling", "no", "post");

        _eToolTipModal.show(posXToolTip, top, bLeftOrRight);
    }
};


/*
Action attachée à l'évènement onclick du document
Si le clic est effectué en dehors de la tooltip, la tooltip est fermée puis l'évènement sur le document est retiré
On vérifie que le clic ne soit pas originaire du RDV ayant déclenché le tooltip (oRDVtt) ou d'un objet enfant de ce RDV (src.parentNode, ex. le pictogramme d'un RDV récurrent)
*/
this.onClickHideTtModal = function (e) {


    if (!e)
        e = window.event;
    try {
        var src = (e.originalTarget || e.srcElement);
        var srcParentNode = src;
        if (src.parentNode) { srcParentNode = src.parentNode; }
        if (e && _eToolTipModal &&
        !((src == this.oRDVtt) || (srcParentNode == this.oRDVtt) || this.SameParentFrameId(src, _eToolTipModal.iframeId))
        ) {
            this.HideTtModal(); //cacher la tooltip
            showvcpl(null, 0); //Cacher la vcard
        }
    }
    catch (Exc) {
        setWindowEventListener('click', null);
    }
}
/*Fermeture visuel de la Modal, déchargement en memoire et suppression de l'evennement sur la page.*/
this.HideTtModal = function () {
    if (_eToolTipModal != null) {
        _eToolTipModal.hide();
        _eToolTipModal = null;
        setWindowEventListener('click', null);
    }
}

/*Indique si un des offset parent de l'objet ne contient l'id passé en paramètre*/
this.SameParentFrameId = function (oElm, id) {
    var origId = id.split("_");
    origId = (origId.length > 1) ? origId[1] : origId[0];
    var idFound = false;
    var current_oElm = oElm;
    while (current_oElm) {
        var currentId = current_oElm.id.split("_");
        currentId = (currentId.length > 1) ? currentId[1] : currentId[0];
        if (origId == currentId) {
            idFound = true;
        }
        current_oElm = (current_oElm.offsetParent || current_oElm.parentElement);
    }
    return idFound;
}

//
// CALENDAR FUNCTIONS
//
//Retourne le menu déroulant des mois
function getMonthSelect(calDate) {
    var aMonth = top._res_2391.split(",");
    var nActiveMonth = calDate.getMonth();
    var nWidth = (getWindowWH()[0] <= 1024) ? 45 : 65;
    var strReturn = "<select id='CalMenuMonth_" + nGlobalActiveTab + "' name='month' style='width:" + nWidth + "px;' onChange='setCurrentMonth(this.selectedIndex)'>";
    for (i = 0; i < aMonth.length; i++) {
        var sThisMonth = (aMonth[i] + "").replace(/'/g, "");
        
        strReturn += "<option" + (i == nActiveMonth ? " selected" : "") + ">" + sThisMonth + "</option>\n";   //+"" sur le replace pour bug IE7
    }
    strReturn += "</select>";
    return strReturn;
}

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

function getCalendar(strCalId, nMonth, nFullYear, dDateInit, nViewMode, strWorkingDays, bHighLight) {
    var strCellInnerHtml = "";
    var bHiddenDay = false;
    var bSelectedDay = false;
    var calDate = new Date(nFullYear, nMonth - 1, 1);
    var d = new Date();
    var nTodayYear = d.getFullYear();
    var nTodayDay = d.getDate();
    var nTodayMonth = d.getMonth()

    var aWorkingDays = strWorkingDays.split(";");

    var nNbrDayInWeek = aWorkingDays.length;

    var dToday = new Date(nTodayYear, nTodayMonth, nTodayDay);

    strWeekDays = createWeekdayList(aWorkingDays);
    var blankCell = "<td class='day emptyday'> </td>";

    // Mois suivant
    if (nMonth >= 12) {
        var strNextMonth = 1;
        var strNextYear = (nFullYear + 1);
    }
    else {
        var strNextMonth = (nMonth + 1);
        var strNextYear = nFullYear;
    }
    // Mois précédent
    if (nMonth <= 1) {
        var strPrevMonth = 12;
        var strPrevYear = (nFullYear - 1);
    }
    else {
        var strPrevMonth = (nMonth - 1);
        var strPrevYear = nFullYear;
    }
    var aMonthName = top._res_829.split(",");
    calendarBegin = "<table id='MenuCalendar_" + nGlobalActiveTab + "' viewmode='" + nViewMode + "' workingdays='" + strWorkingDays + "' cellpadding=0 cellspacing=0>" +
        "<tr>" +
        "<td height='1px' valign=top colspan=" + nNbrDayInWeek + ">" +
            "<center>" +
             "<div class=calendarselect style='margin-bottom: 0px'>" +
                "<table class=underCal>" +
                    "<tr>" +
                        "<td class='icon-edn-prev icnListAct' title=\"" + top._res_136 + "\" onClick='setPreviousMonth();'></td>" + //todo séparateur
                        "<td class='calMonth'>" + getMonthSelect(dDateInit) + "</td>" +
                        "<td class='icon-edn-next icnListAct' title=\"" + top._res_137 + "\" onClick='setNextMonth();'></td>" +
                        "<td><input id='CalMenuYear_" + nGlobalActiveTab + "' name='year' class=calYear style='width:32px;text-align:right' value='" + dDateInit.getFullYear() + "' type=text maxlength=4 onKeyUp=\"window.setTimeout('setCalendarMenuYear()',2000)\"></input></td>" +
                    "</tr>" +
                "</table>" +
            "</div>" +
            "<div class='calendarbody'>" +
                "<div class='labelWeek'>" + top._res_821 + " <span id='weeknumber'>" + DefSemaineNum(calDate.getFullYear(), calDate.getMonth(), dDateInit.getDate()) + "</span></div>" +
                "<table class='calendardays'>" +
                    strWeekDays + "<tr>";

    calendarEnd = "</tr></table></div></center></td></tr></table>";

    var calDoc = calendarBegin;
    var nMonth = calDate.getMonth();
    var nYear = calDate.getFullYear();
    var nDay = dDateInit.getDate();
    var dCurDate = new Date();

    var i = 0, j = 0;
    var days = getDaysInMonth(calDate);
    if (nDay > days) {
        nDay = days;
    }
    var firstOfMonth = new Date(nYear, nMonth, 1);
    var startingPos = 0;

    startingPos = getStartingPos(firstOfMonth, aWorkingDays);
    days += startingPos;

    var columnCount = 0;
    for (i = 0; i < startingPos; i++) {
        calDoc += blankCell;
        columnCount++;
    }
    var currentDay = 0;

    for (i = startingPos; i < days; i++) {
        var strClassName = "day";

        currentDay = i - startingPos + 1;
        dCurDate = new Date(nYear, nMonth, currentDay);

        strCellInnerHtml = "";
        bSelectedDay = false;

        // Jours non affichable
        bHiddenDay = true;
        for (j in aWorkingDays) {
            if (aWorkingDays[j] == dCurDate.getDay() + 1) {
                bHiddenDay = false;
                break;
            }
        }
        // Date du jour
        if (dCurDate.getTime() == dToday.getTime()) {
            strClassName += " today";
            strCellInnerHtml = " title=\"" + top._res_143 + "\" ";
        }
        // Dates sélectionnées
        if (bHighLight) {
            if (nViewMode == VIEW_CAL_WORK_WEEK) {
                var firstDateInWk = dateAdd("d", -(dDateInit.getDay() - 1), dDateInit);

                if ((dCurDate.getTime() >= firstDateInWk.getTime()) && (dCurDate.getTime() <= dateAdd("d", 6, firstDateInWk).getTime())) {
                    strClassName += " selectedday";
                    bSelectedDay = true;
                }
            }
            else if (nViewMode == VIEW_CAL_MONTH) {
                strClassName += " selectedday";
                bSelectedDay = true;
            }
            else {
                if (dCurDate.getTime() == dDateInit.getTime()) {
                    strClassName += " selectedday";
                    bSelectedDay = true;
                }
            }
        }
        var nCurrWeek = DefSemaineNum(nYear, nMonth, currentDay);

        if (!bHiddenDay) {
            if (!bSelectedDay) {
                strCellInnerHtml += " weeknumber=" + nCurrWeek + " onmouseout=\"Calendar_onMouseOut( '" + strCalId + "', this );\" onmouseover=\"Calendar_onMouseOver( '" + strCalId + "', this );\" ";
            }
            if (nNbrDayInWeek > 6)
                var nFontSize = 7;
            else
                var nFontSize = 8;

            calDoc += "<TD align=center " + strCellInnerHtml + " class='" + strClassName + "' style='font-size:" + nFontSize + "pt' id='" + currentDay + "_" + (nMonth + 1) + "_" + nFullYear + "' onclick=\"setCalendarDate('" + nGlobalActiveTab + "','" + currentDay + "/" + (nMonth + 1) + "/" + nFullYear + "');\">" + currentDay + "</TD>";
        }

        columnCount++;
        if (dCurDate.getDay() + 1 == aWorkingDays[aWorkingDays.length - 1]) {
            calDoc += "</TR><TR>";
        }
    }
    var ret = calDoc + calendarEnd;
    return ret;
}

function getStartingPos(dDate, aWorkingDays) {
    var i = 0;
    for (i in aWorkingDays) {
        if (aWorkingDays[i] == dDate.getDay() + 1) {
            if (i > 6)
                i = 0;
            return (parseInt(i, 10));
        }
    }
    return (getStartingPos(dateAdd("d", 1, dDate), aWorkingDays));
}

var validActiveInterval = null;

function Calendar_onMouseOut(strCalId, oTd) {

    oTd.className = strLastClassName;
}

function Calendar_onMouseOver(strCalId, oTd) {
    document.getElementById("weeknumber").innerHTML = oTd.getAttribute("weeknumber");
    strLastClassName = oTd.className;
    oTd.className += " todayhover";
}

function getDaysInMonth(dDate) {
    var nMonth = dDate.getMonth() + 1;
    var nYear = dDate.getFullYear();
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
}

function createWeekdayList(aWorkingDays) {
    var strReturn = "<tr>";
    var aWeekday = top._res_885.split(",");
    for (i = 0; i < aWeekday.length; i++) {
        aWeekday[i] = aWeekday[i].substr(1, aWeekday[i].length - 2);
    }
    var nNbrDayInWeek = aWorkingDays.length;
    if (nNbrDayInWeek > 6)
        var nFontSize = 7;
    else
        var nFontSize = 8;

    for (i = 0; i < aWeekday.length; i++) {
        if (aWorkingDays.length > i && aWeekday.length > aWorkingDays[i] - 1)
            strReturn += "<td style='text-align:center;font-size:" + nFontSize + "pt'>" + (aWeekday[aWorkingDays[i] - 1]).substr(0, 2) + "</td>";
    }
    strReturn += "</tr>";
    return strReturn;
}

function getFirstDateOfWeek(dDate, nViewMode) {
    if (nViewMode != VIEW_CAL_WORK_WEEK)
        return dDate;

    while (nFirstDayOfWeek != dDate.getDay() + 1) {
        dDate = dateAdd("d", -1, dDate);
    }
    return dDate;
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

function setPrevCalDate() {
    setCalendarDate(nGlobalActiveTab, document.getElementById('CalDivMain').getAttribute('prevdate'));
    //document.getElementById("WeekLabel").innerHTML = document.getElementById('CalDivMain').getAttribute('prevlabel');
}

function setNextCalDate() {
    setCalendarDate(nGlobalActiveTab, document.getElementById('CalDivMain').getAttribute('nextdate'));
    //document.getElementById("WeekLabel").innerHTML = document.getElementById('CalDivMain').getAttribute('nextlabel');
}

function setPrevMonthCalDate() {
    setCalendarDate(nGlobalActiveTab, document.getElementById('CalDivMain').getAttribute('prevmonth'));
}

function setNextMonthCalDate() {
    setCalendarDate(nGlobalActiveTab, document.getElementById('CalDivMain').getAttribute('nextmonth'));
}

function setCurrentMonth(idx) {

    var divCal = document.getElementById("Calendar_" + nGlobalActiveTab);
    if (divCal == null)
        return;

    var oTab = document.getElementById("MenuCalendar_" + nGlobalActiveTab);

    var nViewMode = oTab.getAttribute("viewmode");
    var sWorkingDays = oTab.getAttribute("workingdays");;
    var calMonth = idx;
    var calYear = document.getElementById("CalMenuYear_" + nGlobalActiveTab).value;
    var calmode = divCal.getAttribute("calmode");
    var bHighLight = true;

    if (isNaN(calYear))
        return;

    var myDate = new Date(calYear, getNumber(calMonth), 1);

    divCal.innerHTML = getCalendar("Calendar_" + nGlobalActiveTab, calMonth + 1, myDate.getFullYear(), myDate, calmode, sWorkingDays, bHighLight);



}

function setPreviousMonth() {
    var divCal = document.getElementById("Calendar_" + nGlobalActiveTab);
    if (divCal == null)
        return;

    var oTab = document.getElementById("MenuCalendar_" + nGlobalActiveTab);

    var nViewMode = oTab.getAttribute("viewmode");
    var sWorkingDays = oTab.getAttribute("workingdays");;
    var calMonth = document.getElementById("CalMenuMonth_" + nGlobalActiveTab).selectedIndex;
    var calYear = document.getElementById("CalMenuYear_" + nGlobalActiveTab).value;
    var calmode = divCal.getAttribute("calmode");
    var bHighLight = false;
    if (isNaN(calYear))
        return;

    if (calMonth > 0)
        calMonth--;
    else {
        calMonth = 11;
        calYear--;
    }

    var myDate = new Date(calYear, getNumber(calMonth), 1);

    divCal.innerHTML = getCalendar("Calendar_" + nGlobalActiveTab, calMonth + 1, myDate.getFullYear(), myDate, calmode, sWorkingDays, bHighLight);
}

function setNextMonth() {

    var divCal = document.getElementById("Calendar_" + nGlobalActiveTab);
    if (divCal == null)
        return;

    var oTab = document.getElementById("MenuCalendar_" + nGlobalActiveTab);

    var nViewMode = oTab.getAttribute("viewmode");
    var sWorkingDays = oTab.getAttribute("workingdays");;
    var calMonth = document.getElementById("CalMenuMonth_" + nGlobalActiveTab).selectedIndex;
    var calYear = document.getElementById("CalMenuYear_" + nGlobalActiveTab).value;
    var calmode = divCal.getAttribute("calmode");
    var bHighLight = false;


    if (isNaN(calYear))
        return;

    if (calMonth < 11)
        calMonth++;
    else {
        calMonth = 0;
        calYear++;
    }



    var myDate = new Date(calYear, getNumber(calMonth), 1);

    divCal.innerHTML = getCalendar("Calendar_" + nGlobalActiveTab, calMonth + 1, myDate.getFullYear(), myDate, calmode, sWorkingDays, bHighLight);

}

function setCalendarMenuYear() {

    var divCal = document.getElementById("Calendar_" + nGlobalActiveTab);
    if (divCal == null)
        return;

    var oTab = document.getElementById("MenuCalendar_" + nGlobalActiveTab);

    var nViewMode = oTab.getAttribute("viewmode");
    var sWorkingDays = oTab.getAttribute("workingdays");;
    var calMonth = document.getElementById("CalMenuMonth_" + nGlobalActiveTab).selectedIndex;
    var calYear = document.getElementById("CalMenuYear_" + nGlobalActiveTab).value;
    var calmode = divCal.getAttribute("calmode");
    var bHighLight = true;

    if (isNaN(calYear)) {
        return;
    }


    var myDate = new Date(calYear, getNumber(calMonth), 1);

    divCal.innerHTML = getCalendar("Calendar_" + nGlobalActiveTab, calMonth + 1, myDate.getFullYear(), myDate, calmode, sWorkingDays, bHighLight);
}


//**************************************
//*** GESTION DES EVT SOURIS      ******
//**************************************

var selectedItemId = null;  //Id de la div de RDV sélectionnée
var userNbDay = null;

function OIC(evt, obj) {
    OnItemClicked(evt, obj, true);
}
var timerClick = null;
function OnItemClicked(evt, obj, withToolTip) {
    setSelected(obj);
    if (withToolTip == true) {
        if (timerClick)
            clearTimeout(timerClick);
        try {
            this.oicEvtXY = getClickPositionXY(evt);
        }
        catch (ex) {
        }
        timerClick = setTimeout(function () { OpenToolTipCalendar(evt, obj); }, 350);
    }
}

function setSelected(obj) {



    var nFileId = obj.getAttribute("fid");
    if (obj.getAttribute("sel") == "1")
        return;

    var oElmList = getElementsByAttribute(document.getElementById('CalDivMain'), "div", "fid", nFileId);
    var idSel = "";
    userNbDay = "";

    for (var i = 0; i < oElmList.length; i++) {
        id = oElmList[i].id;
        if (idSel != "")
            idSel += ";";
        idSel += id;

        if (userNbDay != "") {
            userNbDay += ";";
        }
        userNbDay += oElmList[i].getAttribute('pint').value;
    }
    selectItem(selectedItemId, false);
    selectItem(idSel, true);
    selectedItemId = idSel;
}


//met en surbrillance le RDV et ajoute les div de redimensionnement en haut et en bas
//eId : 
//bValue : true : Affiche les divGrip et false : cache les divGrip

function selectItem(eId, bValue) {



    if (eId == null || eId == "")
        return;
    var aIds = null;
    if (eId.indexOf(";") < 0) {
        aIds = new Array(eId);
    }
    else {
        aIds = eId.split(";");
    }

    for (i = 0; i < aIds.length; i++) {
        eId = aIds[i];
        var div = getCalElt(eId);
        var divGripLeft = document.getElementById("gl_" + eId);
        if (divGripLeft == null)
            return;
        var divGripTop = document.getElementById("gt_" + eId);
        var divGripBottom = document.getElementById("gb_" + eId);
        var display;
        if (bValue)
            display = "visible";
        else
            display = "hidden";
        if (divGripTop != null)
            divGripTop.style.visibility = display;
        if (divGripBottom != null)
            divGripBottom.style.visibility = display;
        if (!bValue)
            div.style.backgroundColor = div.getAttribute("clr");
        else
            div.style.backgroundColor = "#AECAEF";
    }

}




function showvcpl(obj, nOn, nFileId) {
    var planningObj = null;

    if (nOn == 1 && obj != null)
        planningObj = document.getElementById(_eToolTipModal.iframeId);

    shvc(obj, nOn, nFileId, planningObj);
}

function cleanCutCopyItem() {
    var oeParam = getParamWindow();

    var oldCutItm = document.getElementById(oeParam.GetParam("CuttedItem"));
    if (oldCutItm != null) {
        var oElmList = getElementsByAttribute(document.getElementById('CalDivMain'), "div", "fid", oldCutItm.getAttribute("fid"));
        for (var i = 0; i < oElmList.length; i++) {
            removeClass(oElmList[i], "calElmCutted");
        }

        oeParam.SetParam("CuttedItem", "");
    }

    var oldCopyItm = document.getElementById(oeParam.GetParam("CopiedItem"));
    if (oldCopyItm != null) {
        var oElmList = getElementsByAttribute(document.getElementById('CalDivMain'), "div", "fid", oldCopyItm.getAttribute("fid"));
        for (var i = 0; i < oElmList.length; i++) {
            removeClass(oElmList[i], "calElmCopied");
        }

        oeParam.SetParam("CopiedItem", "");
    }
    else {
        if (oeParam.GetParam("CopiedItem") != null) {
            oeParam.SetParam("CopiedItem", "");
        }
    }

    oeParam.oCalendarBuffer = new Array();
}

//Déplacement/Redimensionnement du planning
var sourceIntervalBegin = null;
var sourceIntervalEnd = null;
var oldMouseMove = null;
var oldMouseDown = null;
var oldMouseUp = null;
var sourceDay = null;
var activeDay = null;
var activeInterval = null;
var activeItem = null;
var dashedDivCal = null;
var activeIntervalResizeBottom = null;
var deltaInterval = 0;

function findPosX(obj) {
    var curleft = 0;
    if (obj.offsetParent) {
        while (obj.offsetParent) {
            curleft += obj.offsetLeft
            obj = obj.offsetParent;
        }
    }
    else if (obj.x)
        curleft += obj.x;
    return curleft;
}

function findPosY(obj) {
    var curtop = 0;
    if (obj.offsetParent) {
        while (obj.offsetParent) {
            curtop += obj.offsetTop
            obj = obj.offsetParent;
        }
    }
    else if (obj.y)
        curtop += obj.y;
    return curtop;
}

function doc_onMouseMoveMove(e) {



    if (_eToolTipModal != null) {
        _eToolTipModal.hide();
        _eToolTipModal = null;
    }

    var objCal = document.getElementById("cal_mt_" + nGlobalActiveTab);
    if (objCal == null)
        return;

    if (window.event) {
        e = window.event;
    }

    if (
    (activeInterval != null && sourceIntervalBegin.id.indexOf(activeDay.id) >= 0 && parseInt(document.getElementById("CalDivMain").getAttribute("calmode")) == 5)
 ||
           (activeInterval != null && parseInt(document.getElementById("CalDivMain").getAttribute("calmode")) != 5)

    ) {
        if (dashedDivCal == null) {
            dashedDivCal = document.createElement("div");
            dashedDivCal.className = "dashedDiv";
        }

        dashedDivCal.style.width = activeDay.getElementsByTagName("div")[0].style.width;
        dashedDivCal.style.height = (activeItem.offsetHeight - 5) + "px";

        var itmHeight = parseInt(dashedDivCal.style.height);
        var nCellHeight = parseInt(document.getElementById("CalDivMain").getAttribute("cellheight"));

        var nbrIntervals = 1 + parseInt(itmHeight / (nCellHeight + 1));
        //var activeRange = parseInt(activeInterval.id.split("_")[2]);

        var aActiveRangeId = activeInterval.id.split("_");
        var diff = parseInt(aActiveRangeId[2]) - deltaInterval
        aActiveRangeId[2] = diff > 0 ? diff : 0;
        var ctnerInterval = getIntervalElt(aActiveRangeId.join('_'));

        if (aActiveRangeId[2] + nbrIntervals <= getMaxIntervals()) {
            dashedDivCal.style.display = "block";
            ctnerInterval.appendChild(dashedDivCal);
            validActiveInterval = ctnerInterval.id;
        }
    }
}

function getMaxIntervals() {
    var objCal = document.getElementById("cal_mt_" + nGlobalActiveTab);
    if (objCal == null)
        return;
    var nMax = parseInt(objCal.getAttribute("nbi"));

    return nMax;

}

function IOMD(obj, e) {
    item_onMouseDown(obj, e);
}

function IOR(obj, e) {
    item_onResize(obj, e);
}

function item_onResize(obj, e) {
    sourceIntervalBegin = null;
    oldMouseMove = null;
    oldMouseUp = null;
    activeDay = null;
    sourceDay = null;
    activeInterval = null;
    activeItem = null;
    dashedDivCal = null;

    if (obj.id.indexOf("gb_") == 0)
        var firedobj = getCalElt(obj.id.replace("gb_", ""));
    else
        var firedobj = getCalElt(obj.id.replace("gt_", ""));

    activeItem = firedobj;      // Item du rdv
    sourceIntervalBegin = activeItem.parentNode;      // Interval de debut
    sourceDay = thtabevt.findUp(activeItem, "TD");        // Jour

    var intrvRange = parseInt(sourceIntervalBegin.id.split("_")[2]);
    var intrv = parseInt(activeItem.getAttribute("int"));
    var lrg = intrvRange + intrv;
    sourceIntervalEnd = getIntervalElt(sourceIntervalBegin.id.split("_")[0] + "_" + sourceIntervalBegin.id.split("_")[1] + "_" + parseInt(lrg));

    oldMouseMove = document.onmousemove;
    oldMouseUp = document.onmouseup;
    oldMouseDown = document.onmousedown;
    if (obj.id.indexOf("gb_") == 0)
        document.onmousemove = doc_onMouseMoveResizeBottom;
    else
        document.onmousemove = doc_onMouseMoveResizeTop;
    if (obj.id.indexOf("gb_") == 0)
        document.onmouseup = doc_onMouseUpResizeBottom;
    else
        document.onmouseup = doc_onMouseUpResizeTop
}

function getIntervalDate(interv) {
    var nRange = Number(interv.id.split("_")[2]);
    var interval = Number(document.getElementById("CalDivMain").getAttribute("interval"));
    var parentDay = thtabevt.findUp(interv, "TD");        // Jour

    var dayDate = eDate.Tools.GetDateFromString(parentDay.getAttribute("date"));

    var intervalDate = DateAdd("n", nRange * interval, dayDate);

    return intervalDate;
}

function doc_onMouseUpResizeTop() {
    document.onmousemove = oldMouseMove;
    document.onmouseup = oldMouseUp;
    document.onmousedown = oldMouseDown;

    if (activeInterval != null) {
        var intervalDate = getIntervalDate(activeInterval);

        var newDateBegin = new Date(intervalDate);

        var itm = new ePlanningItem(activeItem);



        checkConflictGraph(itm, newDateBegin, itm.DateEnd, false, true, true);


        // Supression du z-index imposé par le resize.
        activeItem.style.zIndex = "";

        if (_eToolTipModal != null) {
            _eToolTipModal.hide();
            _eToolTipModal = null;
        }
    }
}

function doc_onMouseUpResizeBottom() {
    document.onmousemove = oldMouseMove;
    document.onmouseup = oldMouseUp;
    document.onmousedown = oldMouseDown;

    if (activeIntervalResizeBottom != null) {
        var nRange = Number(activeIntervalResizeBottom.id.split("_")[2]);
        var interval = Number(document.getElementById("CalDivMain").getAttribute("interval"));

        var dayDate = eDate.Tools.GetDateFromString(activeDay.getAttribute("date"));
        var intervalDate = DateAdd("n", (nRange + 1) * interval, dayDate);
        var newDateEnd = new Date(intervalDate);

        activeIntervalResizeBottom = null;

        var itm = new ePlanningItem(activeItem);

        // Supression du z-index imposé par le resize.
        activeItem.style.zIndex = "";

        checkConflictGraph(itm, itm.DateBegin, newDateEnd, false, true, true);

        /*
        itm.DateEnd = newDateEnd;
        setWait(true);
        itm.save();
        */

        if (_eToolTipModal != null) {
            _eToolTipModal.hide();
            _eToolTipModal = null;
        }
    }
}

function doc_onMouseMoveResizeBottom(e) {
    if (_eToolTipModal != null) {
        _eToolTipModal.hide();
        _eToolTipModal = null;
    }

    var objCal = document.getElementById("cal_mt_" + nGlobalActiveTab);
    if (objCal == null)
        return;

    // HLA - Remplacer par activeIntervalResizeBottom = activeInterval; plus bas
    /*if (activeIntervalResizeBottom == null || Number(activeInterval.id.split("_")[2]) >= Number(sourceIntervalBegin.id.split("_")[2])) {
    activeIntervalResizeBottom = activeInterval;
    }*/

    if (activeInterval != null) {
        var nCellHeight = parseInt(document.getElementById("CalDivMain").getAttribute("cellheight"));
        var srcIntervalRange = parseInt(sourceIntervalBegin.id.split("_")[2]);
        var activeIntervalRange = parseInt(activeInterval.id.split("_")[2]);

        if (activeIntervalRange < srcIntervalRange)
            return;

        //pour ne pas resizer en dehors de jour
        activeInterval.id.split("_")[1] = sourceIntervalBegin.id.split("_")[1];


        activeIntervalResizeBottom = activeInterval;

        var newHeight = ((nCellHeight + 1) * (activeIntervalRange - srcIntervalRange + 1)) - 1;
        if (newHeight < 0)
            newHeight = -newHeight;

        var aSelectedItem = selectedItemId.split(";");

        for (var i = 0; i < aSelectedItem.length; i++) {
            getCalElt(aSelectedItem[i]).style.height = newHeight + "px";
            document.getElementById("gb_" + aSelectedItem[i]).style.top = (newHeight - 5) + "px";
            getCalElt(aSelectedItem[i]).style.zIndex = "100";
        }

    }
}

function doc_onMouseMoveResizeTop(e) {
    if (_eToolTipModal != null) {
        _eToolTipModal.hide();
        _eToolTipModal = null;
    }

    var objCal = document.getElementById("cal_mt_" + nGlobalActiveTab);
    if (objCal == null)
        return;

    if (window.event) {
        e = window.event;
    }

    if (activeInterval != null && sourceIntervalEnd != null) {
        var nCellHeight = parseInt(document.getElementById("CalDivMain").getAttribute("cellheight"));
        var trgIntervalRange = parseInt(sourceIntervalEnd.id.split("_")[2]);
        var activeIntervalRange = parseInt(activeInterval.id.split("_")[2]);

        var newHeight = ((nCellHeight + 1) * (trgIntervalRange - activeIntervalRange)) - 1;

        if (newHeight < 0) {
            var prevIntervalId = sourceIntervalEnd.id.split("_")[0] + "_" + sourceIntervalEnd.id.split("_")[1] + "_" + (trgIntervalRange - 1);
            activeInterval = getIntervalElt(prevIntervalId);
            return;
        }

        var aSelectedItem = selectedItemId.split(";");
        for (var i = 0; i < aSelectedItem.length; i++) {
            if (parseInt(document.getElementById("CalDivMain").getAttribute("calmode")) == 5) //Mode jour uniquement
            {
                var parentInterval = getIntervalElt(aSelectedItem[i]).offsetParent;
                var id = activeInterval.id.replace(activeInterval.id.split("_")[0] + "_" + activeInterval.id.split("_")[1] + "_", parentInterval.id.split("_")[0] + "_" + parentInterval.id.split("_")[1] + "_");
                var itemActiveInterval = getIntervalElt(id);
            }
            else
                var itemActiveInterval = activeInterval;

            itemActiveInterval.appendChild(getCalElt(aSelectedItem[i]));
            getCalElt(aSelectedItem[i]).style.height = newHeight + "px";
            getCalElt(aSelectedItem[i]).style.zIndex = "100";
            document.getElementById("gb_" + aSelectedItem[i]).style.top = (newHeight - 5) + "px";

        }
    }
}

function item_onMouseDown(obj, e) {



    var srcActiveInterval = activeInterval;

    sourceIntervalBegin = null;
    oldMouseMove = null;
    oldMouseUp = null;
    activeDay = null;
    sourceDay = null;
    activeInterval = null;
    activeItem = null;
    dashedDivCal = null;
    validActiveInterval = null;

    firedobj = getCalElt(obj.id.replace("gl_", ""));
    OnItemClicked(e, firedobj, false);

    activeItem = firedobj;     // Item du rdv




    sourceIntervalBegin = activeItem.parentNode;      // Interval de debut
    sourceDay = thtabevt.findUp(activeItem, "TD");        // Jour

    if (firedobj.getAttribute("mov") != "1")
        return;

    if (sourceIntervalBegin == null || srcActiveInterval == null)
        return;

    var nIntervalBegin = getNumber(sourceIntervalBegin.id.split('_')[2])
    var nIntervalActive = getNumber(srcActiveInterval.id.split('_')[2])

    if (nIntervalBegin < nIntervalActive)
        deltaInterval = nIntervalActive - nIntervalBegin;


    oldMouseMove = document.onmousemove;
    oldMouseUp = document.onmouseup;
    oldMouseDown = document.onmousedown;
    document.onmousemove = doc_onMouseMoveMove;
    document.onmousedown = function () { return false; };
    document.onmouseup = doc_onMouseUpMove;
}

function doc_onMouseUpMove(e) {
    document.onmousemove = oldMouseMove;
    document.onmouseup = oldMouseUp;
    document.onmousedown = oldMouseDown;
    if (dashedDivCal == null) {
        dashedDivCal = document.createElement("div");
    }
    dashedDivCal.style.display = "none";
    activeInterval = getIntervalElt(validActiveInterval);

    if (activeInterval != null && activeInterval != sourceIntervalBegin) {
        //calcul des intervalles dans le cas d'affichage multiple (mode jour)
        var aSelectedItem = selectedItemId.split(";");
        for (var i = 0; i < aSelectedItem.length; i++) {
            if (parseInt(document.getElementById("CalDivMain").getAttribute("calmode")) == 5) //Mode jour uniquement
            {
                var parentInterval = getIntervalElt(aSelectedItem[i]).offsetParent;
                var id = activeInterval.id.replace(activeInterval.id.split("_")[0] + "_" + activeInterval.id.split("_")[1] + "_", parentInterval.id.split("_")[0] + "_" + parentInterval.id.split("_")[1] + "_");
                var itemActiveInterval = getIntervalElt(id);
                itemActiveInterval.appendChild(getCalElt(aSelectedItem[i]));
            }
            else
                activeInterval.appendChild(getCalElt(aSelectedItem[i]));
        }

        var intervalDate = getIntervalDate(activeInterval);

        var itm = new ePlanningItem(activeItem);

        var hour = intervalDate.getHours();
        var mn = intervalDate.getMinutes();
        var sc = intervalDate.getSeconds();

        var parentDay = activeInterval.parentNode || activeInterval.parentElement;
        var dayDate = eDate.Tools.GetDateFromString(parentDay.getAttribute("date"));

        var newDateBegin = new Date(dayDate.getFullYear(), dayDate.getMonth(), dayDate.getDate(), hour, mn, sc);

        var itemDelay = (itm.DateEnd - itm.DateBegin) / (1000 * 60);
        var newDateEnd = DateAdd("n", itemDelay, newDateBegin);

        trgCopyDay = activeInterval.parentNode.id; // jour ou est positioné la souris

        checkConflictGraph(itm, newDateBegin, newDateEnd);

        if (_eToolTipModal != null) {
            _eToolTipModal.hide();
            _eToolTipModal = null;
        }
    }
}

var createApproved = false;
var createSrcInterval = null;
var createTrgInterval = null;


function onTabMouseMove(e) {
    if (e == null)
        e = window.event;

    var topMouse = parseInt(e.clientY);
    //Calcul de l'intervalle actif
    if (activeDay == null)
        return;
    var firstInterval = getIntervalElt(activeDay.id + "_0");
    var beginTop = FindY(firstInterval);
    var nTop = topMouse - beginTop;
    var nCellHeight = document.getElementById("CalDivMain").getAttribute("cellheight");
    var nbTotalIntervals = nTop / (parseInt(nCellHeight) + 1);
    var intervalId = activeDay.id + "_" + parseInt(nbTotalIntervals);
    if (getIntervalElt(intervalId) != null) {
        activeInterval = getIntervalElt(intervalId);
    }

    if (createApproved == true) {
        var activeParent = activeInterval.parentNode || activeInterval.parentElement;
        var srcParent = createSrcInterval.parentNode || createSrcInterval.parentElement;
        createTrgInterval = activeInterval;
        if (createTrgInterval != null) {
            setRangeSelected(createSrcInterval.id, createTrgInterval.id, true);
        }
    }
}

function setRangeSelected(fromId, toId, bOn) {
    if (fromId == null || toId == null)
        return;


    if (fromId == toId)
        return;

    //teste si le from est avant le to
    var dayFrom = Number(fromId.split("_")[1]);
    var intervalFrom = Number(fromId.split("_")[2]);

    var dayTo = Number(toId.split("_")[1]);
    var intervalTo = Number(toId.split("_")[2]);

    var bReverse = false;
    if (dayFrom < dayTo)
        bReverse = false;
    else {
        if (dayFrom > dayTo)
            bReverse = true;
        else
            if (intervalFrom > intervalTo)
                bReverse = true;
    }

    if (bReverse) {
        var nDayBegin = dayTo;
        var nIntervalBegin = intervalTo;

        var nDayEnd = dayFrom;
        var nIntervalEnd = intervalFrom;
    }
    else {
        var nDayBegin = dayFrom;
        var nIntervalBegin = intervalFrom;

        var nDayEnd = dayTo;
        var nIntervalEnd = intervalTo;
    }
    for (var i = 0; i <= 7; i++) {
        for (var j = 0; j <= getMaxIntervals() ; j++) {
            var interv = getIntervalElt(fromId.split("_")[0] + "_" + i + "_" + j);
            if (interv == null)
                continue;
            if (i < nDayBegin || i > nDayEnd) {
                removeClass(interv, "rangeSelected");
                continue;
            }
            if (nDayBegin == nDayEnd && i == nDayBegin) {
                if (j >= nIntervalBegin && j <= nIntervalEnd) {
                    if (bOn)
                        addClass(interv, "rangeSelected");
                    else
                        removeClass(interv, "rangeSelected");
                }
                else
                    removeClass(interv, "rangeSelected");
            }
            else {
                if (i == nDayBegin) {
                    if (j >= nIntervalBegin) {
                        if (bOn)
                            addClass(interv, "rangeSelected");
                        else
                            removeClass(interv, "rangeSelected");
                    }
                    else
                        removeClass(interv, "rangeSelected");
                }
                else {
                    if (i == nDayEnd) {
                        if (j <= nIntervalEnd) {
                            if (bOn)
                                addClass(interv, "rangeSelected");
                            else
                                removeClass(interv, "rangeSelected");
                        }
                        else
                            removeClass(interv, "rangeSelected");
                    }
                    else {
                        if (bOn)
                            addClass(interv, "rangeSelected");
                        else
                            removeClass(interv, "rangeSelected");
                    }
                }
            }
        }
    }
}

function onTabMouseDown(e) {
    if (createApproved == false && activeInterval != null) {
        //for (var i = 0; i < lstIntervals.length; i++) {
        //    removeClass(lstIntervals[i], "rangeSelected");
        //}
        //lstIntervals = new Array();
        createApproved = true;
        createSrcInterval = activeInterval;
    }
}

function onTabMouseUp(e) {

    // Gestion pour l'ipdad de la création sur click d'un planning
    if (bTabletMode) {

        // Si on est sur un élément, on ne pas changer le comportement standard

        var is_ie = ((agt.indexOf("msie") != -1) && (agt.indexOf("opera") == -1)); // a priori, les tablettes surfaces ne sont pas supportés
        var firedobj = (!is_ie) ? e.target : event.srcElement;

        // vérifie que l'objet sur lequel on est est un intervalle (et pas un rdv)
        if (firedobj.getAttribute("isinter") == "1") {

            //
            if (createSrcInterval != null && (createTrgInterval == null)) {

                // récupération de l'interval suivant
                if (createSrcInterval.id.split("_").length == 3) {


                    //recherche du nombre d'interval pris par un nouveau rdv
                    var oMainTab = document.getElementById("cal_mt_" + nGlobalActiveTab);
                    var nbDefInter = oMainTab.getAttribute("nbibyrdv");
                    if (!isNumeric(nbDefInter))
                        nbDefInter = 1;
                    else
                        nbDefInter = Number(nbDefInter) - 1;

                    if (nbDefInter < 1)
                        nbDefInter = 1;

                    var aFromId = createSrcInterval.id.split("_");
                    var nextId = Number(aFromId[0]) + "_" + Number(aFromId[1]) + "_" + (Number(aFromId[2]) + nbDefInter);


                    createTrgInterval = getIntervalElt(nextId);

                    if (createTrgInterval == null)
                        createTrgInterval = createSrcInterval;
                }
            }
        }
    }


    if (createApproved == true && createSrcInterval != null && createTrgInterval != null) {



        if (createSrcInterval != createTrgInterval || bTabletMode) {


            //inverser les intervalles si besoin

            //teste si le from est avant le to
            var dayFrom = Number(createSrcInterval.id.split("_")[1]);
            var intervalFrom = Number(createSrcInterval.id.split("_")[2]);

            var dayTo = Number(createTrgInterval.id.split("_")[1]);
            var intervalTo = Number(createTrgInterval.id.split("_")[2]);

            var bReverse = false;
            if (dayFrom < dayTo)
                bReverse = false;
            else {
                if (dayFrom > dayTo)
                    bReverse = true;
                else
                    if (intervalFrom > intervalTo)
                        bReverse = true;
            }

            var fromId = createSrcInterval.id;
            var toId = createTrgInterval.id;
            if (bReverse) {
                fromId = createTrgInterval.id;
                toId = createSrcInterval.id;
            }

            removeClass(selectedCell, "cellSelected");
            selectedCell = getIntervalElt(fromId);
            addClass(selectedCell, "cellSelected");
            //for (var i = 0; i < lstIntervals.length; i++) {
            //    removeClass(lstIntervals[i], "rangeSelected");
            //}

            var nConcernedUserId = 0;
            if (selectedCell)
                nConcernedUserId = Number(selectedCell.id.split("_")[0]);

            showTplPlanning(nGlobalActiveTab, 0, fromId, _res_31, nConcernedUserId, true, toId);

            //lstIntervals = new Array();
        }
    }
    createSrcInterval = null;
    createTrgInterval = null;

    createApproved = false;
}

function FindY(obj) {
    var y = 0;
    while (obj != null) {
        y += obj.offsetTop - obj.scrollTop;
        obj = obj.offsetParent;
    }
    return parseInt(y);
}


function OCMM(obj) {
    activeDay = obj;
}

function OCMO(obj) {
    col_onMouseOut(obj);
}

function col_onMouseOut(obj) {
}

//*****************Mise à jour suite aux déplacemlent et redimensionnement

var ePlanningItem = function (itmDiv, isForMonth) {
    this.Div = itmDiv;
    this.innerHTMLDiv = itmDiv.innerHTML;

    if (isForMonth != null && isForMonth == true) {
        this.FileId = Number(itmDiv.getAttribute("fid"));

        this.DateBegin = eDate.Tools.GetDateFromString(itmDiv.getAttribute("_db"));
        this.DateEnd = eDate.Tools.GetDateFromString(itmDiv.getAttribute("_de"));
        this.Owner = itmDiv.getAttribute("owner");
        this.MultiOwner = itmDiv.getAttribute("multiowner");
    } else {
        this.DateBegin = eDate.Tools.GetDateFromString(itmDiv.getAttribute("_db"));
        this.DateEnd = eDate.Tools.GetDateFromString(itmDiv.getAttribute("_de"));
        this.Left = 0;
        this.MaxLeft = 0;
        // ASY : stock la taille car pb lorsqu on change de semaine apres un copier  la taille de la classe css est celle utilisée
        this.DivHeight = itmDiv.clientHeight;

        this.FileId = Number(itmDiv.getAttribute("fid"));

        this.Intervals = Number(itmDiv.getAttribute("int"));
        this.IsAllDay = getAttributeValue(itmDiv, "ad") == "1"
        this.Owner = itmDiv.getAttribute("owner") == null ? itmDiv.id.split("_")[0] : itmDiv.getAttribute("owner");
        this.MultiOwner = itmDiv.getAttribute("multiowner");
    }

    ///Permet de vérifier si l'élement est en conflict avec le RDV passé en paramètre
    this.isOverLapWith = function (itm) {
        var bReturn = (
        (this.DateBegin >= itm.DateBegin && this.DateBegin < itm.DateEnd)
        || (this.DateEnd > itm.DateBegin && this.DateEnd <= itm.DateEnd)
        || (this.DateBegin < itm.DateEnd && this.DateEnd > itm.DateBegin));
        return bReturn;
    };

    this.save = function (bForceRefresh, callbackFct) {

        var dDateBegin = new Date(this.DateBegin);
        var sDateBegin = eDate.Tools.GetStringFromDate(dDateBegin);

        var dDateEnd = new Date(this.DateEnd);
        var sDateEnd = eDate.Tools.GetStringFromDate(dDateEnd);

        var eEngineUpdater = new eEngine('eEngineUpdater');
        eEngineUpdater.Init();
        var descIdBegin = document.getElementById('CalDivMain').getAttribute('datedescid');
        var descIdEnd = Number(nGlobalActiveTab) + 89;

        var fldDateBegin = new fldUpdEngine(descIdBegin);
        fldDateBegin.newValue = sDateBegin;
        eEngineUpdater.AddOrSetField(fldDateBegin);

        var fldDateEnd = new fldUpdEngine(descIdEnd);
        fldDateEnd.newValue = sDateEnd;
        eEngineUpdater.AddOrSetField(fldDateEnd);

        // HLA - On averti qu'on est en validation de fiche - Dev #45363
        eEngineUpdater.AddOrSetParam('onValideFileAction', '1');

        eEngineUpdater.AddOrSetParam('fileId', this.FileId);
 
        eEngineUpdater.ErrorCallbackFunction = function () {

            if (!bForceRefresh) {
                initItemPos(false, dDateBegin, dDateEnd);
            }
            
        };
        eEngineUpdater.SuccessCallbackFunction = function () {

            if (!bForceRefresh) {
                initItemPos(true, dDateBegin, dDateEnd);
            }
            
            if (typeof callbackFct === "function")
                callbackFct();
            
        };

        eEngineUpdater.UpdateLaunch();
    };
};

function initItemPos(success, dBegin, dEnd) {
    try {
        if (success == false) {
            sourceIntervalBegin.appendChild(activeItem);
        }
        else {
            var strDay = '';
            var nCellHeight = parseInt(document.getElementById("CalDivMain").getAttribute("cellheight"));



            var aSelectedItem = selectedItemId.split(";");
            for (var i = 0; i < aSelectedItem.length; i++) {
                var oSelectedItem = getCalElt(aSelectedItem[i]);


                oSelectedItem.setAttribute("_db", eDate.Tools.GetStringFromDateWithTiret(dBegin));
                oSelectedItem.setAttribute("_de", eDate.Tools.GetStringFromDateWithTiret(dEnd));

                var intervals = parseInt(oSelectedItem.offsetHeight) / (nCellHeight + 1);
                oSelectedItem.setAttribute("int", intervals);

                var objDay = thtabevt.findUp(oSelectedItem, "TD");        // Jour
                if (strDay.indexOf(';' + objDay.id + ';') == -1)
                    strDay = strDay + objDay.id + ';';
            }



            var aDay = strDay.split(";");
            for (var i = 0; i < aDay.length; i++) {
                if (aDay[i] == '')
                    continue;

                var calItem = getCalElt(aDay[i]);


                if (typeof calItem != "undefined")
                    setItemsPosition(calItem);
            }
        }

        // HLA - Modifier par un parcours des jour impactés
        /*setItemsPosition(activeDay);
        if (sourceDay.id != activeDay.id)
        setItemsPosition(sourceDay);*/
    }
    finally {
        setWait(false);
    }
}

function DateAdd(p_Interval, p_Number, p_Date) {
    p_Number = new Number(p_Number);
    var dt = new Date(p_Date);

    switch (p_Interval.toLowerCase()) {
        case "yyyy":
            {
                dt.setFullYear(dt.getFullYear() + p_Number);
                break;
            }
        case "q":
            {
                dt.setMonth(dt.getMonth() + (p_Number * 3));
                break;
            }
        case "m":
            {
                dt.setMonth(dt.getMonth() + p_Number);
                break;
            }
        case "y": 		// day of year
        case "d": 		// day
        case "w":
            {		// weekday
                dt.setDate(dt.getDate() + p_Number);
                break;
            }
        case "ww":
            {	// week of year
                dt.setDate(dt.getDate() + (p_Number * 7));
                break;
            }
        case "h":
            {
                dt.setHours(dt.getHours() + p_Number);
                break;
            }
        case "n":
            {		// minute
                dt.setMinutes(dt.getMinutes() + p_Number);
                break;
            }
        case "s":
            {
                dt.setSeconds(dt.getSeconds() + p_Number);
                break;
            }
        case "ms":
            {	// JS extension
                dt.setMilliseconds(dt.getMilliseconds() + p_Number);
                break;
            }
        default:
            {
                return "invalid interval: '" + p_Interval + "'";
            }
    }

    return dt;
}

function setItemsPosition(objDay) {
    if (objDay == null)
        return;

    var nCurIdx, nI, nIndex, isViewable;
    var nPos, nCntPosBusy, nCurMaxPos, aPosBusy, aItemMatch;
    var nOwnerIndex, sCellBegin, oCellBegin, nLeft, nWidth;
    var dayItems = new Array();

    var MAX_ITEM_OVERLAP = parseInt(document.getElementById("CalDivMain").getAttribute("maxovl"));

    var lstDivs = getElementsByAttribute(objDay, "div", "_db;_de");
    for (i = 0; i < lstDivs.length; i++) {
        var itm = new ePlanningItem(lstDivs[i]);
        dayItems.push(itm);
    }

    // Vide les positions et les positions max de chaque item
    for (var i = 0; i < dayItems.length; i++) {
        var itm = dayItems[i];
        itm.Left = 0;
        itm.MaxLeft = 0;
    }

    aPosBusy = new Array();

    // Recherche les positions pour chaque item
    for (nCurIdx = 0; nCurIdx < dayItems.length; nCurIdx++) {
        var itm = dayItems[nCurIdx];

        //if (!itm.isVisible)
        //    continue;

        nCntPosBusy = 0;
        nCurMaxPos = 0;

        // Tableau des item matcher par l'item en cours
        aItemMatch = new Array();

        for (nPos = 1; nPos <= MAX_ITEM_OVERLAP; nPos++)
            aPosBusy[nPos] = 0;

        // Recherche les positions déjà utilisé pour l'item en cours
        for (nI = 0; nI < dayItems.length; nI++) {
            var itmBis = dayItems[nI];
            // Sauf l'Item en cours et si l'item a déjà été positionné
            if (nCurIdx != nI && itmBis.Left > 0) {
                if (itm.isOverLapWith(itmBis)) {
                    // Tableau des item matcher
                    aItemMatch.push(itmBis);
                    // Compte le nb de position occupé
                    nCntPosBusy++;
                    // Enregistre la position occupé
                    aPosBusy[itmBis.Left] = 1;
                    // Recupère la position max
                    if (nCurMaxPos < itmBis.Left)
                        nCurMaxPos = itmBis.Left;
                }
            }

            if (nCntPosBusy >= MAX_ITEM_OVERLAP)
                break;
        }

        // Indique pour l'item en cours la position libre pour lui même (-1 : aucunes disponibles de position)
        if (nCntPosBusy >= MAX_ITEM_OVERLAP) {
            itm.Left = -1;
        }
        else {
            for (nPos = 1; nPos <= MAX_ITEM_OVERLAP; nPos++) {
                if (aPosBusy[nPos] == 0) {
                    itm.Left = nPos;

                    // Reprendre le position en cours si c'est la position max
                    if (nCurMaxPos < nPos)
                        nCurMaxPos = nPos;
                    // Ajout l'item en cours
                    aItemMatch.push(itm);
                    // Réinitialise les pos max des items matcher et de l'item en cours
                    for (nI = 0; nI < aItemMatch.length; nI++) {
                        var itmBis2 = aItemMatch[nI];
                        if (itmBis2.MaxLeft < nCurMaxPos)
                            itmBis2.MaxLeft = nCurMaxPos;
                    }
                    break;
                }
            }
        }
    }

    // Recherche les positions maximal pour chaque item
    for (nCurIdx = 0; nCurIdx < dayItems.length; nCurIdx++) {
        var itm = dayItems[nCurIdx];
        nPosMax = 0;

        for (nI = 0; nI < dayItems.length; nI++) {
            var itmBis3 = dayItems[nI];
            // Si l'item en cours ou Sauf l'Item en cours et match avec l'item de parcouru
            if (nCurIdx == nI || (nCurIdx != nI && itm.isOverLapWith(itmBis3))) {
                if (nPosMax < itmBis3.MaxLeft)
                    nPosMax = itmBis3.MaxLeft;

                if (nPosMax >= MAX_ITEM_OVERLAP)
                    break;
            }
        }

        itm.MaxLeft = nPosMax;
    }

    for (var i = 0; i < dayItems.length; i++) {
        var thisItm = dayItems[i];
        if (thisItm.Left < 0)
            thisItm.Div.style.visibility = "hidden";
        else {
            var cellWidth = parseInt(activeDay.getElementsByTagName("div")[0].style.width);
            var nWidth = (cellWidth - 10) / thisItm.MaxLeft;
            var nDayLeft = 0; // _parentCelndar.CellWidth* dayIdx;
            var nLeft = nDayLeft + (thisItm.Left - 1) * nWidth;
            thisItm.Div.style.visibility = "visible";
            thisItm.Div.style.width = (nWidth - 10) + "px";
            thisItm.Div.style.left = nLeft + "px";
        }
    }

}

/// Initialise l'event click sur les cellules
function cal_initFldClick(mainTab) {
    if (document.getElementById("cal_mt_" + mainTab) == undefined)
        return;
    if (document.getElementById("cal_mt_" + mainTab) == null)
        return;
    document.getElementById("cal_mt_" + mainTab).onclick = cal_fldLClick;
}

/// Action sur click gauche
function cal_fldLClick(e, topelement) {
    //Evenement
    if (!e)
        var e = window.event;

    // Objet source
    var oSourceObj = e.target || e.srcElement;
    try {
        while (
            oSourceObj.tagName != topelement
            && !(oSourceObj.tagName == 'TD' && oSourceObj.getAttribute("fid") == "1")
         ) {
            oSourceObj = oSourceObj.parentNode || oSourceObj.parentElement;
        }
    }
    catch (ee) {

        return;
    }

    if ((oSourceObj.getAttribute("fid") != "") && (oSourceObj.getAttribute("fid") != null)) {
        OIC(e, oSourceObj);
        stopEvent(e);
    }
}

//var tplFileModal; // on remplace par eModFile défini dans eMain.js
var srcCopyDay;
var trgCopyDay;

/**************************************************************************************/
/**   fonctions de mise à jour des elements planning                              ****/

/*  SPH : TODO : Vérifier les appels a cette fonction. Elle est en fait appelée     */
/*  pour d'autres suppression que les planning                                      */
/**************************************************************************************/
function deleteTpl(nTab, nFileId) {

    deleteFile(nTab, nFileId, eModFile);


}

function deleteCalendarTpl(nTab, nFileId, eModFile, openSerie) {

    var callBack = function (engResult) { onPlanningValidateTreatment(engResult, eModFile, true, false); };
    deleteFile(nTab, nFileId, eModFile, openSerie, callBack);

}



function deleteCalByToolTip(nTab, nFileId, divId) {

    var callBack = function () {


        //Fermeture de la tooltip si elle est déjà ouverte.
        if (_eToolTipModal != null) {
            _eToolTipModal.hide();
            _eToolTipModal = null;
        }

        //CNA - Si divId est vide (cas du mode mois), on recharge le calendrier au complet
        if (divId != null && divId != "") {
            var strDay = ';';

            // Selection de tous les elem commun pour les remove
            var itm = getCalElt(divId);
            var oElmList = getElementsByAttribute(document.getElementById('CalDivMain'), "div", "fid", itm.getAttribute("fid"));
            for (var i = 0; i < oElmList.length; i++) {
                var elem = oElmList[i];     // Item du rdv
                var objInterval = elem.parentNode;      // Interval de debut
                var objDay = thtabevt.findUp(objInterval, "TD");        // Jour
                if (typeof objInterval != "undefined" && typeof objDay != "undefined") {
                    if (strDay.indexOf(';' + objDay.id + ';') == -1)
                        strDay = strDay + objDay.id + ';';

                    objInterval.removeChild(elem);
                }
            }

            var aDay = strDay.split(";");
            for (var i = 0; i < aDay.length; i++) {
                if (aDay[i] == '')
                    continue;

                var calItem = getCalElt(aDay[i]);
                if (typeof calItem != "undefined")
                    setItemsPosition(calItem);
            }
        }
        else {
            top.loadList(1, true);
        }
    };
    deleteFile(nTab, nFileId, null, false, callBack);
}

function printTpl(ntab, fileId) {
    alert("print : " + ntab + "-->" + fileId);
}

function selectOpenSeries(nTab, fileId, lbl, masterid) {


    
    //Fermeture de la tooltip si elle est déjà ouverte.
    if (_eToolTipModal != null) {
        _eToolTipModal.hide();
        _eToolTipModal = null;
    }

    if (timerClick)
        clearTimeout(timerClick);

    var modChoice = eConfirm(1, top._res_1073, top._res_1074, "", 600, 200, function () { onValidOpenSeries(modChoice, nTab, fileId, lbl, masterid); }, null, true);
    //Indique que l'on souhaite que les option soient des RadioBoutons
    modChoice.isMultiSelectList = false;
    //RadioBouton 1 : ouvrir cette occurence
    modChoice.addSelectOption(top._res_1075, "0", true);
    //RadioBouton 2 : ouvrir la série
    modChoice.addSelectOption(top._res_1076, "1", false);
    //Force l'ajout des radioboutons
    modChoice.createSelectListCheckOpt();
}
//Méthode appelée à la confirmation de selectOpenSeries
function onValidOpenSeries(modChoice, nTab, fileId, lbl, masterid) {
    if (!modChoice)
        throw "onValidOpenSeries - modal non trouvée.";
    //Récupère les valeurs sélectionnées ici "0" ou "1"
    let bopenSerie = modChoice.getSelectedValue() == "1";


    showTplPlanning(nTab, fileId, null, lbl, null, null, null, bopenSerie ? true : false, false, masterid);
}

function showTpl(fid) {
    showTplPlanning(nGlobalActiveTab, fid, null, top._res_151);
}

function showTplPlanning(nTab, fileId, parentIntervalId, lbl, userId, fromDblClick, targetIntervalId, openSeries, bNoLoadFile, masterid) {


    if (timerClick)
        clearTimeout(timerClick);

    if (parentIntervalId == null || parentIntervalId == undefined) {
        if (selectedCell != null)
            parentIntervalId = selectedCell.id;
    }


    if (!openSeries)
        openSeries = 0;
    if (!userId)
        userId = 0;

    if (!masterid)
        masterid = 0;


    var nInnerWidth;
    if (window.innerWidth)
        nInnerWidth = window.innerWidth;
    else
        nInnerWidth = document.documentElement.clientWidth;

    var nInnerHeight;
    if (window.innerHeight)
        nInnerHeight = window.innerHeight;
    else
        nInnerHeight = document.documentElement.clientHeight;

    var width = Math.round((8 / 10) * nInnerWidth);
    var height = Math.round((9 / 10) * nInnerHeight);

    if (width < 910) { //Force l'affichage de la fenêtre à sa taille minimum.
        width = 1536;
        height = 843;
    }
    /*Récupération du mode visu de planning*/
    var planningViewMode = getAttributeValue(document.getElementById("planningModes"), "planning_cvm");

    // Récupération de l'heure de début de la journée de travail (pour préremplissage de l'heure si le champ Date de début est défini à "<Date du jour>")
    var workHourBegin = getAttributeValue(document.getElementById("planningModes"), "planning_whb");

    eModFile = new eModalDialog(lbl, 0, "eFileDisplayer.aspx", width, height, "eModFile");

    globalModalFile = true;

    eModFile.ErrorCallBack = launchInContext(eModFile, function () { top.setWait(false); eModFile.hide(); eModFile = null });

    eModFile.onHideFunction = function () { eModFile = null; };

    eModFile.fileId = fileId;
    eModFile.masterId = masterid;
    eModFile.tab = nTab;

    eModFile.addParam("istablet", bTabletMode ? "1" : "0", "post");

    eModFile.addParam("tab", nTab, "post");
    eModFile.addParam("fileid", openSeries ? masterid : fileId, "post");
    eModFile.addParam("masterid", masterid, "post");
    eModFile.addParam("origid", fileId, "post");
    eModFile.addParam("type", 0, "post");
    eModFile.addParam("popup", 1, "post");
    eModFile.addParam("planningviewmode", planningViewMode, "post");
    eModFile.addParam("workhourbegin", workHourBegin, "post");
    // Passage de la taille de la fenêtre en Request.Form pour adapter le contenu en fonction dans eFileDisplayer (template planning)
    eModFile.addParam("width", width, "post");
    eModFile.addParam("height", height, "post");

    if (parentIntervalId != null && document.getElementById("CalDivMain") != null && fromDblClick == true) {
        var beginDate = getIntervalDate(selectedCell);

        eModFile.addParam("date", eDate.Tools.GetStringFromDateWithTiret(beginDate), "post");
        eModFile.addParam("intervalid", parentIntervalId, "post");
        eModFile.addParam("concerneduser", userId, "post");
    }
    eModFile.addParam("openseries", openSeries ? "1" : "0", "post");


    if (targetIntervalId != null && fromDblClick == true) {
        var endDate = getIntervalDate(getIntervalElt(targetIntervalId));
        eModFile.addParam("enddate", eDate.Tools.GetStringFromDateWithTiret(endDate), "post");
    }

    setRangeSelected(parentIntervalId, targetIntervalId, false);

    // SPH  : a priori, le onPlanningFileLoadComplete est déjà appellé dans updatefile via le onload de eFileDisplayer.aspx
    //eModFile.onIframeLoadComplete = onPlanningFileLoadComplete;


    var currentView = top.getCurrentView();
    //Pour une création depuis un signet 
    if (fileId == 0 && typeof (top.GetCurrentFileId) == "function" && (currentView == "FILE_MODIFICATION" || currentView == "FILE_CREATION")) {
        eModFile.addParam("parenttab", top.nGlobalActiveTab, "post");
        eModFile.addParam("parentfileid", top.GetCurrentFileId(top.nGlobalActiveTab), "post");

        //pour les liaisons PP/PM ou EVT  MOU cf. 27801
        eModFile.addParam("lnkid", getAttributeValue(document.getElementById("lnkid_" + top.nGlobalActiveTab), "value"), "post");

        // pour les liaisons spéciales
        var oDivBkm = top.document.getElementById("BkmHead_" + nTab);
        if (oDivBkm && oDivBkm.getAttribute("spclnk")) {
            eModFile.addParam("spclnk", oDivBkm.getAttribute("spclnk"), "post");
        }
    }

    //Histo et Creer pour les planning
    if (fileId == 0 && oLinksInfo.hasValue()) {
      
        //pour les liaisons PP/PM ou EVT depuis le planning en mode historiser et créer, MOU cf. 38369
        eModFile.addParam("lnkid", oLinksInfo.getValue(), "post");
        eModFile.addParam("ptype", oLinksInfo.type, "post");
    }

    if (typeof bNoLoadFile === "undefined")
        bNoLoadFile = false;
    eModFile.addParam("noloadfile", bNoLoadFile ? "1" : "0", "post");

    //On reset après usage les informations
    if (oLinksInfo && typeof (oLinksInfo.reset) == "function")
        oLinksInfo.reset();

    eModFile.noToolbar = false;
    top.setWait(true);
    eModFile.show();

    //En mode série la fiche est en lecture seule
    if (openSeries != "1") {
        eModFile.addButton(top._res_29, onCancelTplFiche, 'button-gray', '', 'cancel');
        eModFile.addButton(top._res_869, onSaveAndCloseTplFiche, 'button-green', null, "savenclose"); //Appliquer et fermer
        eModFile.addButton(top._res_362, onHistoAndCreate, 'button-gray', null, 'histocreate'); //Historiser et créer
        eModFile.addButton(top._res_286, onSaveTplFiche, 'button-gray', null, "save"); //enregistrer
    }
    else {
        eModFile.addButton(top._res_30, onCancelTplFiche, 'button-gray', '', 'cancel');
    }

    //kha le 18/11/14
    //remplacé par la propriété commune tab (cf plus haut dans la meme fonction)
    //eModFile.shownFileTab = nTab;


    // Barre d'outils
    //eModFile.addTemplateButtons(nTab, fileId, (currentView == "CALENDAR" || currentView == "CALENDAR_LIST"), openSeries);
    eModFile.addTemplateButtons(nTab, fileId, true, openSeries);
   
     
    return eModFile;
}

function copyTpl(divId) {
    return cutCopyTpl(divId, "copy");
}

var oContextMenuUpt = null;

function cutTpl(divId) {
    return cutCopyTpl(divId, "cut");
}

function cutCopyTpl(divId, action) {
    // Définition des objets à mettre à jour selon l'action demandée
    var strParamName = "CopiedItem";
    var strClassName = "calElmCopied";
    var strAction = "copy";
    if (action == "cut") {
        strParamName = "CuttedItem";
        strClassName = "calElmCutted";
        strAction = "cut";
    }

    // Remise à zéro du presse-papiers
    cleanCutCopyItem();

    // Mémorisation de l'opération côté client
    var oeParam = getParamWindow();
    oeParam.SetParam(strParamName, divId);

    // Sélection du "fragment" de RDV cliqué
    var itm = getCalElt(divId);
    // Sélection de tous les "fragments" composant le RDV (cas des RDV sur plusieurs jours = plusieurs div)
    var oElmList = getElementsByAttribute(document.getElementById('CalDivMain'), "div", "fid", itm.getAttribute("fid"));
    for (var i = 0; i < oElmList.length; i++) {
        addClass(oElmList[i], strClassName); // on matérialise visuellement le statut copié/collé (transparence, pointillés...)
        oeParam.oCalendarBuffer.push(new ePlanningItem(oElmList[i])); // ajout du fragment parmi ceux à coller
    }

    // Masquage de la bulle de propriétés du RDV
    if (_eToolTipModal != null) {
        _eToolTipModal.hide();
        _eToolTipModal = null;
    }

    // Mémorisation de l'opération côté serveur
    var oUpd = new eUpdater("mgr/ePlanningManager.ashx", 1);
    oUpd.ErrorCallBack = function () { };
    oUpd.addParam("tab", nGlobalActiveTab, "post");
    oUpd.addParam("fileid", itm.getAttribute("fid"), "post");
    oUpd.addParam("action", strAction, "post");
    oUpd.send(function (oRes) { });

    // Mémorisation du jour cible pour la vérification des conflits
    var objDay = thtabevt.findUp(itm, "TD");
    srcCopyDay = objDay.id;
}

function moveDuplTpl(typCopy, divId, isForMonth) {
    var oeParam = getParamWindow();

    // Vidage du "presse-papiers" consacré à la duplication/copie de RDV
    oeParam.oMoveCalendarBuffer = new Array();

    if (isForMonth != null && isForMonth == true) {
        // Sélection de tous les "fragments" composant le RDV (les inputs sur les differentes plages horaires)
        var oElmList = getElementsByAttribute(document.getElementById('CalDivMain'), "input", "fid", divId);
        for (var i = 0; i < oElmList.length; i++) {
            oeParam.oMoveCalendarBuffer.push(new ePlanningItem(oElmList[i], true)); // ajout du fragment parmi ceux à coller
        }
    }
    else {
        // Sélection du "fragment" de RDV cliqué
        var itm = getCalElt(divId);
        // Sélection de tous les "fragments" composant le RDV (cas des RDV sur plusieurs jours = plusieurs div)
        var oElmList = getElementsByAttribute(document.getElementById('CalDivMain'), "div", "fid", itm.getAttribute("fid"));
        for (var i = 0; i < oElmList.length; i++) {
            oeParam.oMoveCalendarBuffer.push(new ePlanningItem(oElmList[i])); // ajout du fragment parmi ceux à coller
        }
    }

    var targetFunctionName = "ValidMoveTpl";
    var strWindowTitle = top._res_6796; // Déplacer vers...
    if (typCopy) {
        targetFunctionName = "ValidDuplTpl";
        strWindowTitle = top._res_6795; // Copier (dupliquer) vers...
    }

    _eDateSelectModal = createCalendarPopUp(targetFunctionName, 1, 0, strWindowTitle, top._res_5003, "onCalendarMoveDuplOkFunction", top._res_29, "onCalendarMoveDuplCancelFunction", null, "", null, eDate.ConvertBddToDisplay(eDate.Tools.GetStringFromDate(oeParam.oMoveCalendarBuffer[0].DateBegin)));

    // Masquage de la bulle de propriétés du RDV
    if (_eToolTipModal != null) {
        _eToolTipModal.hide();
        _eToolTipModal = null;
    }
}

/*
function moveDuplTplMonth(typCopy, fid) {
    var oeParam = getParamWindow();

    // Vidage du "presse-papiers" consacré à la duplication/copie de RDV
    oeParam.oMoveCalendarBuffer = new Array();
   
    // Sélection de tous les "fragments" composant le RDV (cas des RDV sur plusieurs jours = plusieurs div)
    var oElmList = getElementsByAttribute(document.getElementById('CalDivMain'), "input", "fid", fid);
    for (var i = 0; i < oElmList.length; i++) {
        oeParam.oMoveCalendarBuffer.push(new ePlanningItemTest(oElmList[i], true)); // ajout du fragment parmi ceux à coller
    }
    
    var targetFunctionName = "ValidMoveTpl";
    var strWindowTitle = top._res_6796; // Déplacer vers...
    if (typCopy) {
        targetFunctionName = "ValidDuplTpl";
        strWindowTitle = top._res_6795; // Copier (dupliquer) vers...
    }

    _eDateSelectModal = createCalendarPopUp(targetFunctionName, 1, 0, strWindowTitle, top._res_5003, "onCalendarMoveDuplOkFunction", top._res_29, "onCalendarMoveDuplCancelFunction", null, "", null, eDate.ConvertBddToDisplay(eDate.Tools.GetStringFromDate(oeParam.oMoveCalendarBuffer[0].DateBegin)));

    // Masquage de la bulle de propriétés du RDV
    if (_eToolTipModal != null) {
        _eToolTipModal.hide();
        _eToolTipModal = null;
    }
    
}
*/

function closeDateSelectModal() {
    if (_eDateSelectModal != null) {
        _eDateSelectModal.hide();
        _eDateSelectModal = null;
    }
}

function onCalendarMoveDuplOkFunction(date) {
    ValidMoveTpl(date);
}
function onCalendarMoveDuplCancelFunction(date) {
    closeDateSelectModal();
}

function ValidMoveTpl(date) {
    pasteMoveCalendarItem(false, date);
}

function ValidDuplTpl(date) {
    pasteMoveCalendarItem(true, date);
}

//Colle un calendrier coupé/collé, le déplace ou le duplique
//CAS 1 : Si typCopy = true et moveToDate non renseigné : collage d'un RDV copié
//CAS 2 : Si typCopy = false et moveToDate non renseigné : collage d'un RDV coupé
//CAS 3 : Si typCopy = true et moveToDate renseigné : duplication d'un RDV à la date moveToDate
//CAS 4 : Si typCopy = false et moveToDate renseigné : déplacement d'un RDV à la date moveToDate
function pasteMoveCalendarItem(typCopy, moveToDate) {
    var oeParam = getParamWindow();

    // Masquage de la fenêtre de sélection de date
    closeDateSelectModal();

    // On détermine quelle opération effectuer
    var itm = oeParam.oCalendarBuffer;
    var bPasteOperation = true; // CAS 1 ou 2 : collage d'un RDV sur une zone cible (paramètre moveToDate non précisé)
    if (typeof (moveToDate) != "undefined" && moveToDate != null && typeof (oeParam.oMoveCalendarBuffer) != "undefined") {
        itm = oeParam.oMoveCalendarBuffer;
        bPasteOperation = false; // CAS 3 ou 4 : déplacement ou duplication d'un RDV à la date moveToDate
    }
    var bCopy = (typCopy != null && typCopy != 'undefined' && typCopy); // CAS 1 ou 3 : copie ou duplication d'un RDV

    if (itm == null || itm.length == 0)
        return;

    // Calcul des dates de début/fin du RDV
    // CAS 1 ou 2 : collage d'un RDV sur une zone cible (paramètre moveToDate non précisé)
    if (bPasteOperation) {
        // On détermine sur quelle cellule positionner le rendez-vous collé
        if (itm[0].IsAllDay) {
            var newSelectedCell = selectedCell.parentElement.querySelector("div[isinter='1']");
            if (newSelectedCell) {
                removeClass(selectedCell, "cellSelected");
                selectedCell = newSelectedCell;
                addClass(selectedCell, "cellSelected");
                activeInterval = selectedCell;
            }
        }

        // Calcul des dates de début / fin en fonction de la cellule sélectionnée, ou de la date de début passée en paramètre
        var intervalDate = getIntervalDate(selectedCell);
        var hour = intervalDate.getHours();
        var mn = intervalDate.getMinutes();
        var sc = intervalDate.getSeconds();
        var parentDay = selectedCell.parentNode || selectedCell.parentElement;
        var dayDate = eDate.Tools.GetDateFromString(parentDay.getAttribute("date"));
        var newDateBegin;
        if (itm[0].IsAllDay) {
            newDateBegin = new Date(dayDate.getFullYear(), dayDate.getMonth(), dayDate.getDate());
        }
        else {
            newDateBegin = new Date(dayDate.getFullYear(), dayDate.getMonth(), dayDate.getDate(), hour, mn, sc);
        }
    }
        // CAS 3 ou 4 : déplacement ou duplication d'un RDV à la date moveToDate
    else
        newDateBegin = eDate.Tools.GetDateFromString(eDate.ConvertDisplayToBdd(moveToDate));

    // Calcul de la durée totale de la fiche Planning
    // Pour les RDV sur plusieurs jours : on prend la date de fin sur le dernier fragment, et la date de début sur le premier
    var itemDelay = (itm[itm.length - 1].DateEnd - itm[0].DateBegin) / (1000 * 60);
    var newDateEnd = DateAdd("n", itemDelay, newDateBegin);

    // Déplacement visuel (JS) des éléments affichés à l'écran
    // Si le nouveau rendez-vous est trop complexe à redessiner en JavaScript (cas des éléments redimensionnés, à cheval sur plusieurs
    // jours, trop grands pour être positionnés en fin de journée...), on demande au moteur de rafraîchir complètement la page après
    // collage. Pour les cas les plus simples (grande majorité des cas), on positionne simplement l'élément div sur sa nouvelle date de
    // début, ce qui évite un rafraîchissement complet.
    var bForceRefresh = itm.length != 1;
    if (!bCopy) {
        // CAS 2 : Collage d'un RDV coupé (déplacement)
        if (oeParam.GetParam("CuttedItem") != "") {
            // Repositionnement en JS pour les cas les plus simples
            if (!bForceRefresh) {
                if (getCalElt(itm[0].Div.id) != null)
                    selectedCell.appendChild(getCalElt(itm[0].Div.id));
                else {
                    itm[0].Div.innerHTML = itm[0].innerHTMLDiv;
                    selectedCell.appendChild(itm[0].Div);
                }
            }
        }
            // CAS 4 : Déplacement à la date donnée
        else {
            bForceRefresh = true;
        }
    }
    else {
        // CAS 1 : Collage d'un RDV copié (copie)
        if (oeParam.GetParam("CopiedItem") != "") {
            // Repositionnement en JS pour les cas les plus simples
            if (!bForceRefresh) {
                if (itm[0].Div.innerHTML == null || itm[0].Div.innerHTML == "") {
                    itm[0].Div.innerHTML = itm[0].innerHTMLDiv;
                }
            }
        }
            // CAS 3 : Duplication à la date donnée
        else {
            bForceRefresh = true;
        }
    }
    // Vérification des conflits et/ou rafraîchissement de l'affichage
    checkConflictGraph(itm[0], newDateBegin, newDateEnd, bCopy, bForceRefresh);
}



/// <summary>
/// Valide la position d'un calendrier après déplacement
//  Appellé après la vérification des conflits
/// </summary>
/// <param name="itm">Object "item" calendrier</param>
/// <param name="newDateBegin">Nouvelle date de début</param>
/// <param name="newDateEnd">Nouvelle date de fin</param>
/// <param name="copy">Flag copier/coller</param>
/// <param name="bForceRefresh">Indique s'il faut forcer le rafraîchissement complet du planning (cas complexes)</param>
/// <param name="bFromResize">Vient d'un redimensionnement</param>
function validCalendarItemPos(itm, newDateBegin, newDateEnd, copy, bForceRefresh, bFromResize) {
    if (itm.Div.parentNode != null)
        itm.Div.setAttribute("pint", itm.Div.parentNode.id);

    var oeParam = getParamWindow();

    var bCopy = (
        (copy != null && copy != 'undefined' && copy) ||
        (typCopy != null && typCopy != 'undefined' && typCopy)
    );

    if (bCopy) {
        setWait(true);


        var eEngineUpdater = new eEngine();
        var nFileId = itm.Div.getAttribute('fid');
        var descIdBegin = document.getElementById('CalDivMain').getAttribute('datedescid');

        eEngineUpdater.Init();
        eEngineUpdater.AddOrSetParam('tab', nGlobalActiveTab);
        eEngineUpdater.AddOrSetParam('oldFileid', nFileId);
        eEngineUpdater.AddOrSetParam('newDateBegin', eDate.Tools.GetStringFromDateWithTiret(newDateBegin));
        eEngineUpdater.AddOrSetParam('newDateEnd', eDate.Tools.GetStringFromDateWithTiret(newDateEnd));

        // HLA - On averti qu'on est en validation de fiche - Dev #45363
        eEngineUpdater.AddOrSetParam('onValideFileAction', '1');

        eEngineUpdater.ErrorCallbackFunction = function () { setWait(false); };

        eEngineUpdater.SuccessCallbackFunction = function (oRes) {
            updateClonePlanning(oRes, eDate.Tools.GetStringFromDateWithTiret(newDateBegin), eDate.Tools.GetStringFromDateWithTiret(newDateEnd), bForceRefresh);
        }

        eEngineUpdater.UpdateLaunch();
    }
    else {
        setWait(true);

        itm.DateBegin = newDateBegin;
        itm.DateEnd = newDateEnd;
        itm.save(bForceRefresh, function () {

            setWait(false);

            itm.Div.setAttribute("_db", eDate.Tools.GetStringFromDateWithTiret(newDateBegin));
            itm.Div.setAttribute("_de", eDate.Tools.GetStringFromDateWithTiret(newDateEnd));


            if (bForceRefresh) {
                // #36 302 - MAB : application pour d'autres cas complexes (RDV à cheval sur plusieurs jours)
                // il est parfois compliqué de replacer un planning à sa place initiale, les différents éléments et
                // routines de placement du planning étant gérés séparément (grip haut, grip bas, div du planning, infos système liées).
                // Dans ces cas, on recharge entièrement le planning plutôt que d'effectuer des déplacements hasardeux via JS
                // On remet à zéro le presse-papiers AVANT d'envoyer la demande de rafraîchissement, même si les 2 traitements, asynchrones,
                // peuvent potentiellement se dérouler dans le désordre (ce qui provoquerait l'apparition de fiches Planning transparentes après
                // rafraîchissement...)
                if (!bCopy) {
                    clearClipBoard(function () {
                        // #64654 : [planning graphique] - Problème de raffraichissement
                        // On ne recharge pas la liste s'il s'agit d'un redimensionnement
                        //if (!bFromResize)
                            top.loadList(1, true);
                    });
                }
            }
            else {
                var objSrcCopyDay = getCalElt(srcCopyDay);
                if (objSrcCopyDay != null && typeof (objSrcCopyDay) != "undefined")
                    setItemsPosition(objSrcCopyDay);

                if (trgCopyDay != srcCopyDay) {
                    var objTrgCopyDay = getCalElt(trgCopyDay);
                    if (objTrgCopyDay != null && typeof (objTrgCopyDay) != "undefined")
                        setItemsPosition(objTrgCopyDay);
                }
                // On remet à zéro le presse-papiers
                if (!bCopy) {
                    clearClipBoard();
                }
            }
        });

        
    }

}

//Methode pour actuliser / rafraîchir le planning
/// <param name="bForceRefresh">Indique s'il faut forcer le rafraîchissement complet du planning (cas complexes)</param>
function updateClonePlanning(oRes, newDateBegin, newDateEnd, bForceRefresh) {


    setWait(false);

    var oeParam = getParamWindow();
    var itm = oeParam.oCalendarBuffer;

    if (itm.length > 1)
        bForceRefresh = true;

    var OldId = oeParam;
    var dtBegin = newDateBegin;
    var dtEnd = newDateEnd;
    var createdRecord = oRes.getElementsByTagName("createdrecord")[0].getAttribute("ids");

    if (dtBegin == '' || dtEnd == '' || createdRecord == null || typeof (createdRecord) == "undefined" || itm == null) {
        return;
    }

    // Cas complexes nécessitant un rafraîchissement complet du planning (ex : déplacement via le grip haut/bas d'un RDV, RDV
    // à cheval sur plusieurs jours...)
    if (bForceRefresh) {
        // SPH : implémentation initiale
        // #36 302 - MAB : application pour d'autres cas complexes (RDV à cheval sur plusieurs jours)
        // il est parfois compliqué de replacer un planning à sa place initiale, les différents éléments et
        // routines de placement du planning étant gérés séparément (grip haut, grip bas, div du planning, infos système liées).
        // Dans ces cas, on recharge entièrement le planning plutôt que d'effectuer des déplacements hasardeux via JS
        top.loadList(1, true);
        return;
    }

    //Dans le cas où on traite un élément en une seule partie : cloner le rdv au niveau DOM et puis remplacer les val en dur : Ids
    if (itm.length == 1)
        itm = itm[0];



    if (itm && itm.Div && itm.Div.id != null) {


        try {

            var divDupliquer = itm.Div.cloneNode(true);

            //TODO Gerer le troisieme param de l'id en cas d'un rdv sur 2 jours ou +
            divDupliquer.id = divDupliquer.id.replace(itm.Div.id.split("_")[1], createdRecord);

            // ASY (26 162, et 25533) : Les valeurs des attributs n etaient pas correctement affectées (remplacer l affectation des attribut par SetAttribut )
            setAttributeValue(divDupliquer, "fid", createdRecord);
            setAttributeValue(divDupliquer, "ondblclick", "showTpl(" + createdRecord + ");");
            setAttributeValue(divDupliquer, "_db", getStringFromDateEngine(dtBegin));
            setAttributeValue(divDupliquer, "_de", getStringFromDateEngine(dtEnd));
            removeClass(divDupliquer, 'calElmCopied');
            // ASY : récupère la taille originale (car pb lorsqu on change de semaine apres un copier  la taille de la classe css est celle utilisée)

            divDupliquer.style.height = getPastItemHeight(itm.DivHeight) + "px";

            //TODO - A REVOIR
            if (Number(divDupliquer.children[0].getAttribute('length')) > 1 && divDupliquer.children[0].attributes[1].value == "logo-recur") {



                divDupliquer.children[0].outerHTML = "";
                divDupliquer.children[0].title = "";
                divDupliquer.children[0].className = "";
            }

            // ASY : La valeur de l'attribut n'etait pas correctement affectée (remplacer l affectation par SetAttribut )
            if (itm.Div.parentNode != null) {

                setAttributeValue(divDupliquer, "pint", selectedCell.id);
            }

            var childNode;
            for (var i = 0; i < divDupliquer.children.length; i++) {

                //En cas d'un récurrent pourvoir deplacer l'element coller
                if (divDupliquer.children[i].className == "gripleft" && !divDupliquer.children[i].onmousedown) {
                    divDupliquer.children[i].setAttribute("onmousedown", "IOMD(this,event);");
                    divDupliquer.children[i].style.value = "background-color: blue;";
                    // ASY : La valeur de l'attribut n'etait pas correctement affectée (remplacer l affectation par SetAttribut )
                    setAttributeValue(divDupliquer, "mov", "1");
                }
                if (divDupliquer.children[i].id != null && divDupliquer.children[i].id != "") {
                    divDupliquer.children[i].id = divDupliquer.children[i].id.split("_")[0] + "_" + divDupliquer.children[i].id.split("_")[1] + "_" + createdRecord + "_" + divDupliquer.children[i].id.split("_")[3];
                }
            }

            selectedCell.appendChild(divDupliquer);

            /* Mode jour plusieurs users */
            //        var reg = new RegExp(itm.Div.id.split("_")[1], "g");
            //        selectedItemId = selectedItemId.replace(reg, divDupliquer.id.split("_")[1]);
            //        userNbDay
            //        Deja dupliqué
            selectedItemId = selectedItemId.replace(itm.Div.id + ";", "").replace(";" + itm.Div.id, "");
            //Mettre la bonne occurence horizontal
        }
        catch (e) {
            alert(e);
        }




        if (parseInt(document.getElementById("CalDivMain").getAttribute("calmode")) == 5) //Mode jour uniquement et plsieurs users
        {



            selectedItemId = selectedItemId.split(";");
            userNbDay = userNbDay.split(";");
            //Parcourir la listes des ids des autres items
            for (var i = 0; i < selectedItemId.length; i++) {
                //Parcourir les id des td du jour ID_Colonne_Ligne
                for (var l = 0; l < userNbDay.length; l++) {
                    //Test s'il ont le meme user id
                    if (userNbDay[l].split("_")[0] == selectedItemId[i].split("_")[0]) {
                        var cellId = userNbDay[l].split("_");
                        cellId = cellId[0] + "_" + cellId[1] + "_" + selectedCell.id.split("_")[2];
                        var cellClone = getIntervalElt(cellId);
                        if (cellClone != null && cellClone != 'undefined') {
                            userNbDay[l] = userNbDay[l].replace(cellClone.id.split(";")[2], selectedCell.id.split("_")[2]);
                            var divCloneAutrUser = divDupliquer.cloneNode(true);
                            //Mettre le bon id user dans id de la div
                            divCloneAutrUser.id = divDupliquer.id.replace(itm.Div.id.split("_")[0], cellClone.id.split("_")[0]);
                            var idChild = null;
                            for (var z = 0; z < divCloneAutrUser.children.length; z++) {
                                //La div où il ya du text le premiere na pas d'id
                                if (divDupliquer.children[z].id != null && divDupliquer.children[z].id != "") {
                                    idChild = divDupliquer.children[z].id.split("_");
                                    divCloneAutrUser.children[z].id = idChild[0] + "_" + cellClone.id.split("_")[0] + "_" + idChild[2] + "_" + idChild[3];
                                }
                            }
                            // ASY : La valeur de l'attribut n'etait pas correctement affectée (remplacer l affectation par SetAttribut )
                            setAttributeValue(divCloneAutrUser, "pint", cellClone.id);

                            cellClone.appendChild(divCloneAutrUser);
                        }
                    }
                }
            }
        }



        //userNbDay = userNbDay.replace(selectedCell.id + ";", "").replace(";" + selectedCell.id, "");

        //        var aSelectedItem = selectedItemId.split(";");
        //        if (parseInt(document.getElementById("CalDivMain").getAttribute("calmode")) == 5) //Mode jour uniquement et plsieurs users
        //        {
        //            for (var i = 0; i < aSelectedItem.length; i++) {
        //                var parentInterval = document.getElementById(aSelectedItem[i].replace(aSelectedItem[i].split("_")[1], divDupliquer.id.split("_")[1])).offsetParent;
        //                var id = activeInterval.id.replace(activeInterval.id.split("_")[0] + "_" + activeInterval.id.split("_")[1] + "_", parentInterval.id.split("_")[0] + "_" + parentInterval.id.split("_")[1] + "_");
        //                var itemActiveInterval = document.getElementById(id);
        //            }
        //        }
        /* Fin mode jour */

        setSelected(divDupliquer);

        var objTrgCopyDay = getCalElt(trgCopyDay);
        if (objTrgCopyDay != null && typeof (objTrgCopyDay) != "undefined")
            setItemsPosition(objTrgCopyDay);

    }

}

// our calculer la hauteur de l item copier : la hauteur du source ou si pas assez de place la hauteur jusqu'au dernier intervale
function getPastItemHeight(pastHeight) {
    var itmHeight = parseInt(pastHeight);
    var nCellHeight = parseInt(document.getElementById("CalDivMain").getAttribute("cellheight"));

    var nbrIntervals = 1 + parseInt(itmHeight / (nCellHeight + 1));
    var activeRange = (activeInterval) ? parseInt(activeInterval.id.split("_")[2]) : 0; // 39215 CRU : activeInterval non défini dans certains cas
    var maxInterv = getMaxIntervals();
    if (activeRange + nbrIntervals > maxInterv) {
        itmHeight = (maxInterv - activeRange) * nCellHeight;
    }

    return itmHeight;
}

function getStringFromDateEngine(date) {
    if (date != null && date != 'undefined') {
        return date.replace(/\//g, "-").replace(/ /g, "-").replace(/:/g, "-");
    }
    return '';
}

function clearClipBoard(afterClear) {
    var oUpd = new eUpdater("mgr/ePlanningManager.ashx", 1);
    oUpd.ErrorCallBack = function () { setWait(false); };
    oUpd.addParam("tab", nGlobalActiveTab, "post");
    oUpd.addParam("action", "clear", "post");

    oUpd.send(function (oRes) {
        if (typeof (afterClear) == "function") {
            afterClear();
        }
        cleanCutCopyItem();
    });

}

function onReloadTplFiche() {
    eModFile.getIframe().RefreshFile();
}

//Objet contenant les infos de liaisons PP, PM, EVT
var oLinksInfo =
    {
        getValue: function () { return "200=" + this.ppid + ";300=" + this.pmid + ";" + this.evttab + "=" + this.evtid +';400=' + this.adrid ; },
        hasValue: function () { return this.ppid > 0 || this.pmid > 0 || this.evtid > 0 || this.adrid > 0; },
        reset: function () { this.ppid = 0; this.pmid = 0; this.evtid = 0; this.evttab = 0; this.type = null , this.adrid=0},
        ppid: 0,
        pmid: 0,
        adrid: 0,
        evtid: 0,
        evttab: 0,
        type: null
    };

function onHistoAndCreate(afterSave) {

    var nTab = eModFile.tab;
    var nFileId = getNumber(getAttributeValue(eModFile.getIframe().document.getElementById("fileDiv_" + nTab), "fid"));
    SaveLinksInfo(nTab);
    validatePlanningFile(nTab, nFileId, eModFile, true, true, afterSave);
}

function SaveLinksInfo(nTab) {
    
    oLinksInfo.reset();
    var value = getAttributeValue(eModFile.getIframe().document.getElementById("lnkid_" + nTab), "value");
    //ex: value : 200=173260;300=49624;100=73347
    var data = value.split(";");
    if (data != null && data.length >= 3) {
      
        data.forEach(
            function (elem) {
                var arrVal = elem.split('=')
                if (arrVal.length == 2) {

                    var nTab = getNumber(arrVal[0]);
                    var nVal = arrVal[1];

                    if (nTab == 200)
                        oLinksInfo.ppid = nVal;
                    else if (nTab == 300)
                        oLinksInfo.pmid = nVal;
                    else if (nTab == 400)
                        oLinksInfo.adrid = nVal;
                    else if (nTab != 400) {
                        oLinksInfo.evttab = nTab;
                        oLinksInfo.evtid = nVal;
                    }

                }
            }

            );

    }
}


function onSaveAndCloseTplFiche(afterSave) {

    var oTplFileModal = eTools.GetModal("eModFile");

    var nTab = oTplFileModal.tab;
    var nFileId = getNumber(getAttributeValue(oTplFileModal.getIframe().document.getElementById("fileDiv_" + nTab), "fid"));

    validatePlanningFile(nTab, nFileId, oTplFileModal, true, false, afterSave);
}

function onSaveTplFiche(afterSave) {
    var oTplFileModal = eModFile;
    if (typeof (oTplFileModal) == 'undefined')
        oTplFileModal = top.eModFile;

    var nTab = oTplFileModal.tab;
    var nFileId = getNumber(getAttributeValue(oTplFileModal.getIframe().document.getElementById("fileDiv_" + nTab), "fid"));

    validatePlanningFile(nTab, nFileId, oTplFileModal, false, false, afterSave);
}


// fermeture de la modale du planning
// Modif le 13/11/2012 NBA pour gestion PJ
function onCancelTplFiche() {



    var iframeTpl = eModFile.getIframe();

    if (iframeTpl && iframeTpl.document && iframeTpl.document.isTplLoading) {
        this.tracePlanning("Des éléments sont toujours en cours de chargement. La fenêtre ne peut pas être fermée avant la fin des opérations");
        return;
    }

    cancelAndDeletePJ(iframeTpl);

    cancelAndDeleteImages(iframeTpl);

    if (eModFile && eModFile.hide)
        eModFile.hide();

}


function onPlanningFileLoadComplete(sFrom) {
    try {



        //A ne faire que pour les planning graphique (type 1)
        if (!eModFile && top.window && top.window['_md'])
            eModFile = top.window['_md']["eModFile"];


        if (!(eModFile && eModFile.isModalDialog)) {
            return;
        }

        var nType = -1;
        if (eModFile.getIframe()) {
            var curFile = eModFile.getIframe().document.getElementById("fileDiv_" + eModFile.tab);
            nType = Number(getAttributeValue(curFile, "edntype"));
        }

        if (nType != 1)
            return;


        top.setWait(true);
        setTimeout(function () {


            if (eModFile && eModFile.getIframe() != null && eModFile.getIframe().setConflictInfos) {  //test car sinon plante si l'on ouvre et ferme direct une nouvelle fiche planning
                if (eModFile.getIframe().setConflictInfos != null && typeof (eModFile.getIframe().setConflictInfos) == "function") {


                    eModFile.getIframe().setConflictInfos(function () { });
                }
            }

            if (eModFile) {
                var nFileId = getNumber(getAttributeValue(eModFile.getIframe().document.getElementById("fileDiv_" + eModFile.tab), "fid"));
                eModFile.setToolBarVisible(
                    //eModFile.ToolbarButtonType.CancelLastValuesButton + ";" +
                    eModFile.ToolbarButtonType.PropertiesButton + ";" +
                    eModFile.ToolbarButtonType.MandatoryButton + ";" +
                    eModFile.ToolbarButtonType.PjButton + ";" +
                    eModFile.ToolbarButtonType.PrintButton + ";" +
                    eModFile.ToolbarButtonType.SendMailButton + ";" +
                    (nFileId != 0 ? eModFile.ToolbarButtonType.DeleteCalendarButton : '')
                , true
                );


                // la fiche template est complètement chargée
                eModFile.getIframe().document.isTplLoading = false;

                top.setWait(false);


            }
        }, 500);
    }
    catch (exp) {
        eAlert(0, "Fiche planning", "Une erreur est survenue", "Une erreur est survenue durant l'affichage des propriétés de la fiche <br/>Erreur JS : " + exxexp);
    }


    top.setWait(false);


}


var selectedCell;

var calContextMenu;

function onCalDblClick(e, bFromTabletLongTap) {
    var agt = navigator.userAgent.toLowerCase();
    var is_ie = ((agt.indexOf("msie") != -1) && (agt.indexOf("opera") == -1));
    var firedobj = (!is_ie) ? e.target : event.srcElement;
    var topelement = (!is_ie) ? "HTML" : "BODY";

    if (isTablet() && !bFromTabletLongTap)
        return;

    if (
            firedobj.className != "i-D pgc"
        && firedobj.className != "i-D_b pgc"
        && firedobj.className != "i-D_b pgc cellSelected"
        && firedobj.className != "i-D pgc cellSelected"
        && firedobj.className != "i-D pgc rangeSelected"
        ) {
        return;
    }
    else {
        if (selectedCell != null)
            selectedCell.className = selectedCell.className.replace(" cellSelected", "");

        selectedCell = firedobj;

        trgCopyDay = selectedCell.id.split("_")[0] + "_" + selectedCell.id.split("_")[1];
        var concernedUser = selectedCell.id.split("_")[0];

        if (selectedCell.className.indexOf("cellSelected") < 0)
            selectedCell.className = selectedCell.className + " cellSelected";

        showTplPlanning(nGlobalActiveTab, 0, selectedCell.id, top._res_31, concernedUser, true);

    }


}

function onCalClick(e) {

    e = e || window.event; // window.event si e n'est pas défini (pour IE)

    var agt = navigator.userAgent.toLowerCase();
    var is_ie = ((agt.indexOf("msie") != -1) && (agt.indexOf("opera") == -1));
    var firedobj = (!is_ie) ? e.target : event.srcElement;
    var topelement = (!is_ie) ? "HTML" : "BODY";

    //if (getAttributeValue(firedobj, "isinter") != "1")
    //    return;

    // Objet source
    var oSourceObj = firedobj;
    var oSourceObjOrig = oSourceObj;
    try {
        while (
            oSourceObj.tagName != topelement
            && !(oSourceObj.tagName == 'DIV' && oSourceObj.id == "CalDivMain")
         ) {
            oSourceObj = oSourceObj.parentNode || oSourceObj.parentElement;
        }
    }
    catch (ee) {
        return;
    }

    if (oSourceObj.id != "CalDivMain")
        return;

    var targetElement = e.target || e.srcElement; // srcElement for IE

    // Au clic sur les intervalles (vides)
    if (getAttributeValue(firedobj, "isinter") == "1") {


        if (getNumber(getAttributeValue(oSourceObj, "fid")) > 0)
            return;

        //while (firedobj.tagName != topelement && firedobj.className != "PostIt" && firedobj.className != "box" && firedobj.tagName != 'SELECT' && firedobj.tagName != 'TEXTAREA' && firedobj.tagName != 'INPUT' && firedobj.tagName != 'IMG') {
        //you can add the elements that cannot be used for drag here. using their class name or id or tag names
        //firedobj = (!is_ie) ? firedobj.parentNode : firedobj.parentElement;
        if (selectedCell != null)
            selectedCell.className = selectedCell.className.replace(" cellSelected", "");

        selectedCell = firedobj;

        trgCopyDay = selectedCell.id.split("_")[0] + "_" + selectedCell.id.split("_")[1];
        var concernedUser = selectedCell.id.split("_")[0];

        if (selectedCell.className.indexOf("cellSelected") < 0)
            selectedCell.className = selectedCell.className + " cellSelected";

        var oeParam = getParamWindow();
        var itm = oeParam.oCalendarBuffer[0];

        //Coller ou Copier
        if (oeParam.GetParam("CuttedItem") != "" || oeParam.GetParam("CopiedItem") != "") {
            //        if (itm.Owner == Number(concernedUser)) {
            var obj_pos = getAbsolutePosition(selectedCell);
            calContextMenu = new eContextMenu(null, obj_pos.y, obj_pos.x);

            var oActionMenuMouseOver = function () {
                var actionOut = setTimeout(
                    function () {
                        calContextMenu.hide();
                    }
                    , 200);

                // Annule la disparition
                setEventListener(calContextMenu.mainDiv, "mouseover", function () { clearTimeout(actionOut) });
            };

            if (oeParam.GetParam("CopiedItem") != "") {
                calContextMenu.addItem(top._res_1388, "pasteMoveCalendarItem(true); calContextMenu.hide();", 0, 1, "actionItem", top._res_1388);
            }
            else {
                calContextMenu.addItem(top._res_1388, "pasteMoveCalendarItem(false); calContextMenu.hide();", 0, 1, "actionItem", top._res_1388);
            }
            calContextMenu.addItem(top._res_1701, " clearClipBoard(); calContextMenu.hide();", 0, 1, "actionItem", top._res_1701);
            //        }
            //        else if (calContextMenu != null && typeof calContextMenu != "undefined") {
            //            calContextMenu.hide();
            //            calContextMenu = null;
            //        }
        }
    }
    else if (targetElement.className.indexOf("calElm") > -1 || targetElement.className.indexOf("elmCnt") > -1) {
        // Cas d'une tâche Agenda
        var targetElt;
        if (targetElement.className.indexOf("calElm") > -1) {
            targetElt = targetElement;
        }
        else {
            targetElt = targetElement.parentElement;
        }

        OIC(e, targetElt);

        if (e.type == "touchend") {
            e.preventDefault();
            return false;
        }
    }
    else {
        // Cas par défaut où on prend l'événement click
        if (targetElement.click) {
            targetElement.click();
        }
    }

}

//Ouverture de l'éditeur du champ note
function PlShMemo(oSourceObj) {

    if (oSourceObj == null || typeof (oSourceObj) == 'undefined')
        return;

    eMemoEditorObject.onClick(oSourceObj);

    // l'appel à la function HideTtModal rend inaccessible oSourceObj (Permission refusée) car elle fait l'affectation du container : _eToolTipModal = null   #39111
    // Le planning est rafraichis au retour ajax, du coup, la ligne suivante est commenté 
    // parent.HideTtModal();

}
//Redirection vers une fiche tout en fermant la VCARD si active et la tooltip
function PlLoadFile(bVCARD, nDescId, nFileId) {
    parent.loadFile(nDescId, nFileId, 2);
    if (bVCARD)
        showvcpl(null, 0);
    parent.HideTtModal();
}


// ASY : Interception des evenements clavier pour les suppressions, impression, copier,  couper, modifier ( demande JBE )
document.onkeydown = Calendar_onKeyDown;
document.onkeyup = Calendar_onKeyUp;
document.onkeypressed = Calendar_onKeyPressed;

KEY_DEL = 46;
KEY_C = 67;
KEY_M = 77;
KEY_P = 80;
KEY_X = 88;
KEY_W = 87;



function Calendar_onKeyDown(e) {

    // s assure qu on est bien dans le menu calendar
    var oTab = document.getElementById("MenuCalendar_" + nGlobalActiveTab);
    if (oTab) {

        var winObj = checkEventObj(e);

        if (winObj != null) {

            // Gestion Effacement
            if (winObj.keyCode == KEY_DEL) {
                DeleteByKeyboardCalendarItem();
            }

        }
    }




}

function Calendar_onKeyUp(e) {
    // s assure qu on est bien dans le menu calendar
    var oTab = document.getElementById("MenuCalendar_" + nGlobalActiveTab);
    var winObj = checkEventObj(e);
    if (oTab) {



        if (winObj != null) {

            if (winObj.ctrlKey)
                switch (winObj.keyCode) {
                    case KEY_C:
                        CopyByKeyboardCalendarItem();
                        break;
                    case KEY_X:
                        CutByKeyboardCalendarItem();
                        break;
                    case KEY_P:
                        PrintByKeyboardCalendarItem();
                        break;
                    case KEY_M:
                        ModifyByKeyboardCalendarItem();
                        break;
                }
        }
    }

    try {
        //retire le setwait
        if (winObj.altKey) {
            if (winObj.keyCode == KEY_W)
                top.setWait(false);
            else if (winObj.keyCode == KEY_X)
                top.setWait(true);
        }
    }
    catch (e) {
    }

}


function Calendar_onKeyPressed(e) {
    // s assure qu on est bien dans le menu calendar
    var oTab = document.getElementById("MenuCalendar_" + nGlobalActiveTab);
    if (oTab) {

        var winObj = checkEventObj(e);

        if ((winObj != null) && (winObj.keyCode == KEY_DEL)) {
            // alert('Calendar_onKeyPressed - KEY_DEL = 46 ');
        }
    }
}

function checkEventObj(_event_) {
    // --- IE explorer
    if (window.event)
        return window.event;
        // --- Netscape and other explorers
    else
        return _event_;
}

// fonction qui prend en charge la suppression par le clavier en mode Semaine , si la fiche n est pas confidentielle
function DeleteByKeyboardCalendarItem() {

    var oTab = document.getElementById("MenuCalendar_" + nGlobalActiveTab);
    if (oTab) {
        var nViewMode = oTab.getAttribute("viewmode");

        // On ne peut pas supprimer pour le mode mois, ou lorsque la fiche est confidentielle
        if ((nViewMode != VIEW_CAL_MONTH) && (this._bConfidential == false)) {

            var oTabCal = document.getElementById("cal_mt_" + nGlobalActiveTab);
            var oDivCalToSup = oTabCal.querySelector("div[fid='" + this._fileId + "']");
            if (oDivCalToSup && oDivCalToSup.id)
                deleteCalByToolTip(nGlobalActiveTab, this._fileId, oDivCalToSup.id);

        }
    }

}

// par le clavier : Impresssion Ctr + P
function PrintByKeyboardCalendarItem() {
    if ((this.oRDVtt) && (this._bConfidential == false))
        printTpl(this.oRDVtt.id);
}

// par le clavier : Gestion Copier Ctr + C
function CopyByKeyboardCalendarItem() {
    if ((this.oRDVtt) && (this._bConfidential == false))
        copyTpl(this.oRDVtt.id);
}

// par le clavier : Gestion Couper Ctr + X
function CutByKeyboardCalendarItem() {
    if ((this.oRDVtt) && (this._bConfidential == false))
        cutTpl(this.oRDVtt.id);
}


// par le clavier : Gestion Modifier Ctr + M
function ModifyByKeyboardCalendarItem() {
    /*
    if (ScheduleId > 0)
    selectOpenSeries(nGlobalActiveTab, this._fileId, eResApp.GetRes(pref.Lang, 151) );
    else
    //showTplPlanning(nGlobalActiveTab, this._fileId, null, eResApp.GetRes(pref.Lang, 151));  // dans le cs
    showTplPlanning(nGlobalActiveTab, this._fileId, selectedCell.id, top._res_31, concernedUser, true);
    // function
    //showTplPlanning(nTab, fileId, parentIntervalId, lbl, userId, fromDblClick, targetIntervalId, openSeries) // prototype de la function
    */
}

function getCalElt(id) {
    var obj = document.querySelector("div[id='" + id + "'].calElm");
    if (!obj)
        obj = document.getElementById(id);

    return obj;
}

function getIntervalElt(id) {
    var obj = document.querySelector("div[id='" + id + "'].pgc");
    if (!obj)
        obj = document.getElementById(id);

    return obj;
}