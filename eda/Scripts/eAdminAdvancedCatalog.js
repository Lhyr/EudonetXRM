var nsAdminAdvancedCatalog = nsAdminAdvancedCatalog || {};

nsAdminAdvancedCatalog.load = function (nTabDescid, nFieldDescid) {
    nsAdminAdvancedCatalog.modal = top.window['_md']["modalAdvancedCatalog"];
    nsAdminAdvancedCatalog.tab = nTabDescid;
    nsAdminAdvancedCatalog.field = nFieldDescid;

    nsAdminAdvancedCatalog.InitProperties();
    nsAdminAdvancedCatalog.InitState();

    nsAdminAdvancedCatalog.AddEventListeners();
}

nsAdminAdvancedCatalog.InitProperties = function () {
    var hIsSuperAdmin = document.getElementById("edaIsSuperAdmin");
    if (hIsSuperAdmin != null && hIsSuperAdmin.value == "1")
        nsAdminAdvancedCatalog._isSuperAdmin = true;
    else
        nsAdminAdvancedCatalog._isSuperAdmin = false;

    nsAdminAdvancedCatalog._isDataEnabled = nsAdminAdvancedCatalog.IsDataEnabled();
    nsAdminAdvancedCatalog._isMultiple = nsAdminAdvancedCatalog.IsMultiple();
    nsAdminAdvancedCatalog._isTreeView = nsAdminAdvancedCatalog.IsTreeView();
    nsAdminAdvancedCatalog._isDataAutoEnabled = nsAdminAdvancedCatalog.IsDataAutoEnabled();
    nsAdminAdvancedCatalog._isDataAutoStart = nsAdminAdvancedCatalog.IsDataAutoStart();
    nsAdminAdvancedCatalog._isDataAutoFormula = nsAdminAdvancedCatalog.IsDataAutoFormula();
    nsAdminAdvancedCatalog._isStepMode = nsAdminAdvancedCatalog.IsStepMode();
}

nsAdminAdvancedCatalog.InitState = function () {
    //nsAdminAdvancedCatalog.ChangeStateDataEnabled();
    nsAdminAdvancedCatalog.ChangeStateDisplayMask();
    nsAdminAdvancedCatalog.ChangeStateSortBy();
    //nsAdminAdvancedCatalog.ChangeStateTreeView();
    nsAdminAdvancedCatalog.ChangeStateTreeViewOnlyLastChildren();
    //nsAdminAdvancedCatalog.ChangeStateDataAutoEnabled();
    nsAdminAdvancedCatalog.ChangeStateDataAutoStart();
    nsAdminAdvancedCatalog.ChangeStateTxtDataAutoStart();
    nsAdminAdvancedCatalog.ChangeStateDataAutoFormula();
    nsAdminAdvancedCatalog.ChangeStateTxtDataAutoFormula();
    nsAdminAdvancedCatalog.ChangeStateRblStepMode();
    nsAdminAdvancedCatalog.ChangeStateTxtSelectedValueColor();
    nsAdminAdvancedCatalog.ChangeStateRblSequenceMode();
}

nsAdminAdvancedCatalog.AddEventListeners = function () {
    // Evénements au clic sur les titres d'étapes
    titles = document.getElementsByClassName("paramStep");
    for (var i = 0; i < titles.length; i++) {
        titles[i].addEventListener("click", function () {

            // On ferme l'étape déjà ouverte
            var stepActive = document.querySelector(".stepContent[data-active='1']");
            if (stepActive) {
                setAttributeValue(stepActive, "data-active", "0");
                var title = stepActive.parentElement.querySelector(".paramStep");
                eTools.RemoveClassName(title, "active");
            }

            // On ouvre/ferme l'étape correspondante
            var content = this.parentElement.querySelector(".stepContent");
            var active = getAttributeValue(content, "data-active");
            if (active == "1") {
                setAttributeValue(content, "data-active", "0");
                eTools.RemoveClassName(this, "active");
            }
            else {
                setAttributeValue(content, "data-active", "1");
                eTools.SetClassName(this, "active");
            }
        });
    }

    //Radio button Code oui/non
    var radios = document.getElementsByName("rblDataEnabled");
    for (var i = 0; i < radios.length; i++) {
        var radio = radios[i];
        radio.addEventListener("click", function () {
            nsAdminAdvancedCatalog.DataEnabledChanged();
        });
    }

    //Radio button Unitaire/Multiple
    var radios = document.getElementsByName("rblMultiple");
    for (var i = 0; i < radios.length; i++) {
        var radio = radios[i];
        radio.addEventListener("click", function () {
            nsAdminAdvancedCatalog.MultipleChanged();
        });
    }

    //Radio button Liste/Arborescent
    var radios = document.getElementsByName("rblTreeView");
    for (var i = 0; i < radios.length; i++) {
        var radio = radios[i];
        radio.addEventListener("click", function () {
            nsAdminAdvancedCatalog.TreeViewChanged();
        });
    }

    //Radio button Liste/Arborescent
    var radios = document.getElementsByName("rblStepMode");
    for (var i = 0; i < radios.length; i++) {
        var radio = radios[i];
        radio.addEventListener("click", function () {
            nsAdminAdvancedCatalog.StepModeChanged();
        });
    }

    //checkbox AutoStartEnabled
    var chxDataAutoEnabled = document.getElementById("chxDataAutoEnabled");
    if (chxDataAutoEnabled != null) {
        chxDataAutoEnabled.addEventListener("click", function () {
            nsAdminAdvancedCatalog.DataAutoEnabledChanged();
        });
    }

    //Radio button AutoStart/AutoFormula
    var rblDataAutoStart = document.getElementById("rblDataAutoStart");
    if (rblDataAutoStart != null) {
        rblDataAutoStart.addEventListener("click", function () {
            nsAdminAdvancedCatalog.DataAutoStartChanged();
        });
    }
    var rblDataAutoFormula = document.getElementById("rblDataAutoFormula");
    if (rblDataAutoFormula != null) {
        rblDataAutoFormula.addEventListener("click", function () {
            nsAdminAdvancedCatalog.DataAutoFormulaChanged();
        });
    }
}

nsAdminAdvancedCatalog.GetRadiosBoolValue = function (name) {
    var returnValue = false;

    var radios = document.getElementsByName(name);
    for (var i = 0; i < radios.length; i++) {
        var radio = radios[i];

        if (radio.checked && radio.value == "1") {
            returnValue = true;
            break;
        }
    }

    return returnValue;
}

/* region Multiple/Arborescent */

nsAdminAdvancedCatalog.MultipleChanged = function () {
    nsAdminAdvancedCatalog._isMultiple = nsAdminAdvancedCatalog.IsMultiple();

    nsAdminAdvancedCatalog.ChangeStateTreeView();
    nsAdminAdvancedCatalog.ChangeStateTreeViewOnlyLastChildren();
    nsAdminAdvancedCatalog.ChangeStateRblStepMode();
    nsAdminAdvancedCatalog.ChangeStateTxtSelectedValueColor();
    nsAdminAdvancedCatalog.ChangeStateRblSequenceMode();
}

nsAdminAdvancedCatalog.IsMultiple = function () {
    return nsAdminAdvancedCatalog.GetRadiosBoolValue("rblMultiple");
}

nsAdminAdvancedCatalog.TreeViewChanged = function () {
    nsAdminAdvancedCatalog._isTreeView = nsAdminAdvancedCatalog.IsTreeView();

    nsAdminAdvancedCatalog.ChangeStateTreeViewOnlyLastChildren();
}

nsAdminAdvancedCatalog.IsTreeView = function () {
    return nsAdminAdvancedCatalog.GetRadiosBoolValue("rblTreeView");
}

nsAdminAdvancedCatalog.ChangeStateTreeView = function () {
    var radios = document.getElementsByName("rblTreeView");
    for (var i = 0; i < radios.length; i++) {
        var radio = radios[i];

        radio.disabled = !nsAdminAdvancedCatalog._isMultiple;
    }
}

nsAdminAdvancedCatalog.ChangeStateTreeViewOnlyLastChildren = function () {
    var radios = document.getElementsByName("rblTreeViewOnlyLastChildren");
    for (var i = 0; i < radios.length; i++) {
        var radio = radios[i];

        radio.disabled = !nsAdminAdvancedCatalog._isMultiple || !nsAdminAdvancedCatalog._isTreeView;
    }
}

/* fin region Multiple/Arborescent */


/* region Code */

//nsAdminAdvancedCatalog.ChangeStateDataEnabled = function () {
//    var radios = document.getElementsByName("rblDataEnabled");
//    for (var i = 0; i < radios.length; i++) {
//        var radio = radios[i];

//        radio.disabled = !nsAdminAdvancedCatalog._isSuperAdmin && nsAdminAdvancedCatalog._isDataAutoFormula && nsAdminAdvancedCatalog.GetTxtDataAutoFormula() != "";
//    }
//}

nsAdminAdvancedCatalog.StepModeChanged = function () {
    nsAdminAdvancedCatalog._isStepMode = nsAdminAdvancedCatalog.IsStepMode();

    nsAdminAdvancedCatalog.ChangeStateRblSequenceMode();
    nsAdminAdvancedCatalog.ChangeStateTxtSelectedValueColor();
}

nsAdminAdvancedCatalog.DataEnabledChanged = function () {
    nsAdminAdvancedCatalog._isDataEnabled = nsAdminAdvancedCatalog.IsDataEnabled();

    nsAdminAdvancedCatalog.ChangeStateDisplayMask();
    nsAdminAdvancedCatalog.ChangeStateSortBy();
    nsAdminAdvancedCatalog.ChangeStateDataAutoEnabled();
    nsAdminAdvancedCatalog.ChangeStateDataAutoStart();
    nsAdminAdvancedCatalog.ChangeStateDataAutoFormula();
    nsAdminAdvancedCatalog.ChangeStateTxtDataAutoStart();
    nsAdminAdvancedCatalog.ChangeStateTxtDataAutoFormula();
}

nsAdminAdvancedCatalog.IsDataEnabled = function () {
    return nsAdminAdvancedCatalog.GetRadiosBoolValue("rblDataEnabled");
}

nsAdminAdvancedCatalog.ChangeStateDisplayMask = function () {
    var element = document.getElementById("ddlDisplayMask");
    if (element != null)
        element.disabled = !nsAdminAdvancedCatalog._isDataEnabled;    
}

nsAdminAdvancedCatalog.ChangeStateSortBy = function () {
    var element = document.getElementById("ddlSortBy");
    if (element != null)
        element.disabled = !nsAdminAdvancedCatalog._isDataEnabled;
}

nsAdminAdvancedCatalog.DataAutoEnabledChanged = function () {
    nsAdminAdvancedCatalog._isDataAutoEnabled = nsAdminAdvancedCatalog.IsDataAutoEnabled();

    nsAdminAdvancedCatalog.ChangeStateDataAutoStart();
    nsAdminAdvancedCatalog.ChangeStateDataAutoFormula();
    nsAdminAdvancedCatalog.ChangeStateTxtDataAutoStart();
    nsAdminAdvancedCatalog.ChangeStateTxtDataAutoFormula();
}

nsAdminAdvancedCatalog.IsDataAutoEnabled = function () {
    var element = document.getElementById("chxDataAutoEnabled");
    if (element != null && element.hasAttribute("chk") && element.getAttribute("chk") == "1")
        return true;
    else
        return false;
}

nsAdminAdvancedCatalog.ChangeStateDataAutoEnabled = function () {
    var element = document.getElementById("chxDataAutoEnabled");
    if (element != null)
        disChk(element, !nsAdminAdvancedCatalog._isDataEnabled || (!nsAdminAdvancedCatalog._isSuperAdmin && nsAdminAdvancedCatalog._isDataAutoFormula && nsAdminAdvancedCatalog.GetTxtDataAutoFormula() != ""));
}

nsAdminAdvancedCatalog.DataAutoStartChanged = function () {
    if (nsAdminAdvancedCatalog.IsDataAutoStart());
    {
        var element = document.getElementById("rblDataAutoFormula");
        if (element != null) {
            element.checked = false;
        }

        nsAdminAdvancedCatalog._isDataAutoStart = true;
        nsAdminAdvancedCatalog._isDataAutoFormula = nsAdminAdvancedCatalog.IsDataAutoFormula();

        nsAdminAdvancedCatalog.SetTxtDataAutoFormula("");
    }

    nsAdminAdvancedCatalog.ChangeStateTxtDataAutoStart();
    nsAdminAdvancedCatalog.ChangeStateTxtDataAutoFormula();
}

nsAdminAdvancedCatalog.IsDataAutoStart = function () {
    var element = document.getElementById("rblDataAutoStart");
    if (element != null && element.checked)
        return true;
    else
        return false;
}

nsAdminAdvancedCatalog.ChangeStateDataAutoStart = function () {
    var element = document.getElementById("rblDataAutoStart");
    if (element != null)
        element.disabled = !nsAdminAdvancedCatalog._isDataEnabled
            || !nsAdminAdvancedCatalog._isDataAutoEnabled
            || (!nsAdminAdvancedCatalog._isSuperAdmin && nsAdminAdvancedCatalog.GetTxtDataAutoFormula() != "");
}

nsAdminAdvancedCatalog.DataAutoFormulaChanged = function () {
    if (nsAdminAdvancedCatalog.IsDataAutoFormula());
    {
        var element = document.getElementById("rblDataAutoStart");
        if (element != null) {
            element.checked = false;
        }

        nsAdminAdvancedCatalog._isDataAutoStart = nsAdminAdvancedCatalog.IsDataAutoStart();
        nsAdminAdvancedCatalog._isDataAutoFormula = true;

        nsAdminAdvancedCatalog.SetTxtDataAutoStart("");
    }

    nsAdminAdvancedCatalog.ChangeStateTxtDataAutoStart();
    nsAdminAdvancedCatalog.ChangeStateTxtDataAutoFormula();
}

nsAdminAdvancedCatalog.IsDataAutoFormula = function () {
    var element = document.getElementById("rblDataAutoFormula");
    if (element != null && element.checked)
        return true;
    else
        return false;
}

nsAdminAdvancedCatalog.IsStepMode = function () {
    var element = document.getElementById("rblStepMode_1");
    if (element != null && element.checked)
        return true;
    else
        return false;
}


nsAdminAdvancedCatalog.ChangeStateDataAutoFormula = function () {
    var element = document.getElementById("rblDataAutoFormula");
    if (element != null)
        element.disabled = !nsAdminAdvancedCatalog._isDataEnabled
            || !nsAdminAdvancedCatalog._isDataAutoEnabled
            || !nsAdminAdvancedCatalog._isSuperAdmin;
}

nsAdminAdvancedCatalog.GetTxtDataAutoStart = function () {
    var element = document.getElementById("txtDataAutoStart");
    if (element != null)
        return element.value;
    else
        return "";
}

nsAdminAdvancedCatalog.SetTxtDataAutoStart = function (value) {
    var element = document.getElementById("txtDataAutoStart");
    if (element != null)
        element.value = value;
}

nsAdminAdvancedCatalog.ChangeStateTxtDataAutoStart = function () {
    var element = document.getElementById("txtDataAutoStart");
    if (element != null)
        element.disabled = !nsAdminAdvancedCatalog._isDataEnabled
            || !nsAdminAdvancedCatalog._isDataAutoEnabled
            || (!nsAdminAdvancedCatalog._isSuperAdmin && nsAdminAdvancedCatalog.GetTxtDataAutoFormula() != "")
            || !nsAdminAdvancedCatalog._isDataAutoStart;
}

nsAdminAdvancedCatalog.GetTxtDataAutoFormula = function () {
    var element = document.getElementById("txtDataAutoFormula");
    if (element != null)
        return element.value;
    else
        return "";
}

nsAdminAdvancedCatalog.SetTxtDataAutoFormula = function (value) {
    var element = document.getElementById("txtDataAutoFormula");
    if (element != null)
        element.value = value;
}

nsAdminAdvancedCatalog.ChangeStateTxtDataAutoFormula = function () {
    var element = document.getElementById("txtDataAutoFormula");
    if (element != null)
        element.disabled = !nsAdminAdvancedCatalog._isDataEnabled
            || !nsAdminAdvancedCatalog._isDataAutoEnabled
            || !nsAdminAdvancedCatalog._isDataAutoFormula
            || !nsAdminAdvancedCatalog._isSuperAdmin;
}

nsAdminAdvancedCatalog.ChangeStateRblStepMode = function () {
    var elements = document.getElementsByName("rblStepMode");
    for (var i = 0; i < elements.length; i++) {
        elements[i].disabled = nsAdminAdvancedCatalog._isMultiple;
    }
}

nsAdminAdvancedCatalog.ChangeStateTxtSelectedValueColor = function () {
    var element = document.getElementById("txtSelectedValueColor");
    if (element != null)
        element.disabled = !nsAdminAdvancedCatalog._isStepMode;
}

nsAdminAdvancedCatalog.ChangeStateRblSequenceMode = function () {
    var elements = document.getElementsByName("rblSequenceMode");
    for (var i = 0; i < elements.length; i++) {
        elements[i].disabled = !nsAdminAdvancedCatalog._isStepMode;
    }
}

/* fin region Code */
