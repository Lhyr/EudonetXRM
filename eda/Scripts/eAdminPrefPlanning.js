var nsAdminPrefPlanning = nsAdminPrefPlanning || {};

nsAdminPrefPlanning.load = function (descid) {
    nsAdminPrefPlanning.updCaps = new Capsule(descid);
    nsAdminPrefPlanning.addEventListeners();
}
nsAdminPrefPlanning.addEventListeners = function () {
    var days = document.querySelectorAll("#listWeekDays li");
    for (var i = 0; i < days.length; i++) {
        days[i].onclick = nsAdminPrefPlanning.updateDdlWeekFirstDay;
    }

    var firstDay = document.getElementById("ddlFirstDay");
    //if (firstDay)
    //    firstDay.onchange = nsAdminPrefPlanning.updateSelectedWeekFirstDay;

    // Couleurs
    var listColors = document.getElementsByClassName("rdvColor");
    for (var i = 0; i < listColors.length; i++) {
        listColors[i].onclick = function (event) {
            nsAdmin.openColorPicker(event.target);
        }
    }

    // Décalage des horaires entre heure de début et fin
    var beginHour = document.getElementById("ddlBeginHour");
    var endHour = document.getElementById("ddlEndHour");
    beginHour.onchange = function () {
        nsAdminPrefPlanning.controlHours(beginHour, endHour);
    }
    endHour.onchange = function () {
        nsAdminPrefPlanning.controlHours(beginHour, endHour);
    }
}
nsAdminPrefPlanning.setCapsule = function () {
    // Jours de travail
    var workingDays = "";
    var dayNum = "";
    var listSelectedDays = document.querySelectorAll("#listWeekDays li.selected");
    var firstWorkingDay = document.querySelector("#ddlFirstDay option:checked").value;
    //document.getElementById('ddlFirstDay').value = getAttributeValue(days[0], "data-daynum");
    var indexFirstWorkingDay = 0; //Par defaut on commence avec le premier jour coche
    //On recupere l index du jour selectionne dans la ddl Premier jour de la semaine
    for (var i = 0; i < listSelectedDays.length; i++) {
        dayNum = getAttributeValue(listSelectedDays[i], "data-dayNum");
        if (dayNum != "" && dayNum == firstWorkingDay) {
            indexFirstWorkingDay = i;
        }
    }
    //On insert dans la liste tout les jours suivants
    for (var i = indexFirstWorkingDay; i < listSelectedDays.length; i++) {
        dayNum = getAttributeValue(listSelectedDays[i], "data-dayNum");
        if (dayNum != "") {
            if (workingDays != "")
                workingDays += ";";
            workingDays += dayNum;
        }
    }
    // On reboucle pour avoir les potentiels jour précédents selectionnes
    for (var i = 0; i < indexFirstWorkingDay; i++) {
        dayNum = getAttributeValue(listSelectedDays[i], "data-dayNum");
        if (dayNum != "") {
            if (workingDays != "")
                workingDays += ";";
            workingDays += dayNum;
        }
    }

    var desc = getAttributeValue(document.getElementById("listWeekDays"), "dsc");
    var aDesc = desc.split("|");
    nsAdminPrefPlanning.updCaps.AddProperty(aDesc[0], aDesc[1], workingDays); // Jours de travail

    //Mode semaine
    // Heure de début
    var element = document.getElementById("ddlViewBeginHour");
    nsAdmin.addPropertyToCapsule(element, nsAdminPrefPlanning.updCaps);
    // Heure de fin
    element = document.getElementById("ddlViewEndHour");
    nsAdmin.addPropertyToCapsule(element, nsAdminPrefPlanning.updCaps);

    // Options semaine de travail

    // Heure de début
    var element = document.getElementById("ddlBeginHour");
    nsAdmin.addPropertyToCapsule(element, nsAdminPrefPlanning.updCaps);
    // Heure de fin
    element = document.getElementById("ddlEndHour");
    nsAdmin.addPropertyToCapsule(element, nsAdminPrefPlanning.updCaps);
    // Durée des intervalles
    element = document.getElementById("ddlRangesDuration");
    nsAdmin.addPropertyToCapsule(element, nsAdminPrefPlanning.updCaps);
    // Durée par défaut d'un rendez-vous
    element = document.getElementById("ddlDefaultRange");
    nsAdmin.addPropertyToCapsule(element, nsAdminPrefPlanning.updCaps);
    // Nb max de chevauchements
    element = document.getElementById("ddlMaxCh");
    nsAdmin.addPropertyToCapsule(element, nsAdminPrefPlanning.updCaps);

    // Tâches affichées en mode jour
    nsAdmin.addRbPropertyToCapsule("rbDayOption", nsAdminPrefPlanning.updCaps);

    // Activer les conflits
    nsAdmin.addCbPropertyToCapsule(document.getElementById("chkConflictEnabled"), nsAdminPrefPlanning.updCaps);
    // Planning sur la date du jour
    nsAdmin.addCbPropertyToCapsule(document.getElementById("chkTodayOnLogin"), nsAdminPrefPlanning.updCaps);
    // Afficher uniquement fiches non archivées
    nsAdmin.addCbPropertyToCapsule(document.getElementById("chkHistoByPassEnabled"), nsAdminPrefPlanning.updCaps);

    // Couleurs
    nsAdmin.addPropertyToCapsule(document.getElementById("gripuserownercolor"), nsAdminPrefPlanning.updCaps);
    nsAdmin.addPropertyToCapsule(document.getElementById("gripconfidentialcolor"), nsAdminPrefPlanning.updCaps);
    nsAdmin.addPropertyToCapsule(document.getElementById("gripmultiownercolor"), nsAdminPrefPlanning.updCaps);
    nsAdmin.addPropertyToCapsule(document.getElementById("grippubliccolor"), nsAdminPrefPlanning.updCaps);
    nsAdmin.addPropertyToCapsule(document.getElementById("gripotherconfidentialcolor"), nsAdminPrefPlanning.updCaps);

};
// Met à jour le premier jour de la semaine
nsAdminPrefPlanning.updateDdlWeekFirstDay = function (event) {
    var element = event.target;
    if (element.classList.contains("selected"))
        removeClass(element, "selected");
    else
        addClass(element, "selected");

    //var days = document.querySelectorAll("#listWeekDays li.selected");
    //document.getElementById('ddlFirstDay').value = getAttributeValue(days[0], "data-daynum");
}
// Sélectionne le premier jour de la semaine
nsAdminPrefPlanning.updateSelectedWeekFirstDay = function (event) {
    var element = event.target;
    var dayNum = element.value;
    var day = document.querySelector("#listWeekDays li[data-daynum='" + dayNum + "']");
    if (!day.classList.contains(dayNum))
    {
        addClass(day, "selected"); 
    }
    var tempDaynum;
    var list = document.getElementById("listWeekDays").children;

    for (var i = 0; i < list.length; i++) {
        tempDaynum = Number(getAttributeValue(list[i], "data-daynum"));
        if (tempDaynum < Number(dayNum)) {
            removeClass(list[i], "selected");
        }
        else
            break;
    }
}
// Contrôle heure de début/heure de fin : on rajoute une demi-heure à l'heure de fin lorsque les 2 sont identiques
nsAdminPrefPlanning.controlHours = function (beginHourElt, endHourElt) {
    if (beginHourElt.value == endHourElt.value) {
        eAlert(1, top._res_1264, top._res_913);
        endHourElt.selectedIndex = endHourElt.selectedIndex + 1;
    }
}

