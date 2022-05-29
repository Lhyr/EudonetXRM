///summary
///Constructeur de la classe eReport
///<param name="reportType">Type de rapport (Export, publipostage, impression, graphique...)</param>
///<param name="reportName">Nom du rapport</param>
///<param name="userId">Utilisateur propriétaire</param>
///<param name="tab">onglet sur lequel le rapport est actif</param>
///<param name="reportId">Identifiant du rapport (dans le cas de l'édition d'un rapport existant)</param>
///<param name="end Procedure">Procédure à exécuter après le rapport</param>
///summary

var modalPowerBIUserCat;
var modalPowerBIExpirationDate;

function eReport(reportType, reportName, userId, tab, reportId, endProcedure) {

    ///Propriétés
    var that = this;
    var _iReportType = reportType;
    var _strReportName = reportName;
    var _iUserId = userId;
    var _aReportParams = new Array();
    var _iTab = tab;
    var _iParamLength = 0;
    var _strViewRule = "";
    var _iReportId = reportId;
    var _strStartProcedure = "";
    var _strEndProcedure = endProcedure == null || endProcedure == "undefined" ? "" : endProcedure;
    ///permId, Mode, Level, User
    var _aViewPerm = { "id": "0", "mode": "-1", "level": "0", "user": "" };
    var _aUpdatePerm = { "id": "0", "mode": "-1", "level": "0", "user": "" };

    var _sScheduleOrig = "";
    var _sSchedule = "";


    ///Méthodes  Accesseurs
    this.IsViewPermActive = function () {
        return (this._aViewPerm["mode"] != "-1") && (
         parseInt(this._aViewPerm["level"]) > 0
        || this._aViewPerm["user"] != "") ? "1" : "0";
    }

    this.IsUpdatePermActive = function () {
        return (this._aUpdatePerm["mode"] != "-1") && (
         parseInt(this._aUpdatePerm["level"]) > 0
        || this._aUpdatePerm["user"] != "") ? "1" : "0";
    }
    this.GetViewPerm = function () {
        return this._aViewPerm;
    }

    this.GetUpdatePerm = function () {
        return this._aUpdatePerm;
    }
    this.SetViewPerm = function (value) {
        this._aViewPerm = value;
        this.UpdateMode("view");
    }


    this.SetScheduleOrig = function (value) {
        this._sScheduleOrig = value;
    }


    this.GetScheduleOrig = function () {

        return this._sScheduleOrig;
    }


    var _bIsScheduled = 0;

    this.SetIsScheduled = function (value, bOpen) {

        this._bIsScheduled = value == 1 ? 1 : 0;

        if (this.GetIsScheduled() && bOpen) {
            nsFilterReportList.onPlanifyWiz(this.GetId())
        }

    }

    this.GetIsScheduled = function (value) {
        return this._bIsScheduled == 1;
    }


    this.SetSchedule = function (value) {
        this._sSchedule = value;
    }




    this.GetSchedule = function () {

        return this._sSchedule;
    }


    this.SetUpdatePerm = function (value) {
        this._aUpdatePerm = value;
        this.UpdateMode("update");
    }

    this.SetViewPermParam = function (param, value) {
        this._aViewPerm[param] = value;
        this.UpdateMode("view");
    }

    this.SetUpdatePermParam = function (param, value) {
        this._aUpdatePerm[param] = value;
        this.UpdateMode("update");
    }

    ///Procédure à exécuter avant le rapport
    this.GetStartProcedure = function () {
        return _strStartProcedure;
    }

    ///Procédure à exécuter avant le rapport
    this.SetStartProcedure = function (value) {
        if (value == null)
            return;
        _strStartProcedure = value;
    }

    ///Procédure à exécuter après le rapport
    this.SetEndProcedure = function (value) {
        if (value == null)
            return;
        _strEndProcedure = value;
    }

    ///Procédure à exécuter après le rapport
    this.GetEndProcedure = function () {
        return _strEndProcedure;
    }



    ///Onglet
    this.GetTab = function () {
        return _iTab;
    }

    ///Règle d'envoi conditionnel
    this.GetViewRule = function () {
        return _strViewRule;
    }

    ///Type de rapport
    this.GetType = function () {
        return _iReportType;
    }

    ///Utilisateur propriétaire
    this.GetUserId = function () {
        return _iUserId;
    }

    ///Libellé du rapport
    this.GetName = function () {
        return _strReportName;
    }
    this.SetName = function (value) {
        _strReportName = value;
    }

    ///Droit de modification
    this.GetUpdatePermId = function () {
        return this._aUpdatePerm["id"];
    }

    ///Droit de visualisation
    this.GetViewPermId = function () {
        return this._aViewPerm["id"];
    }

    ///Identifiant du rapport
    this.GetId = function () {
        return _iReportId;
    }
    this.SetId = function (value) {
        _iReportId = value;
    }

    ///summary
    ///Vérifie dans le dictionnaire des paramètres du rapport si la clé passée en paramètre existe
    ///<param name="key">clé de paramètre de rapport</param>
    ///summary
    this.KeyExists = function (key) {
        return (key in this.aReportParams);
    }
    ///Summary
    /// Retourne la valeur associée à la clé
    ///<param name="key">clé de paramètre de rapport</param>
    ///summary
    this.GetParam = function (key) {
        if (key == null)
            return null;
        if (this.KeyExists(key))
            return this.aReportParams[key];
        else
            return null;
    }
    ///summary
    ///Charge le dictionnaire de paramètres internes à partir du paramètre transmit
    ///<param name="aParams">Dictionnaire de paramètres de rapport</param>
    ///summary
    this.LoadParam = function (aParams) {
        this.aReportParams = aParams
        iParamLength = this.ParamLength();
        this.bCircularGauge = (aParams.typechart == '4|2');
    }

    ///summary
    ///Retourne la taille du dictionnaire interne de paramètres
    ///summary
    this.ParamLength = function () {
        var length = 0, key;
        for (key in this.aReportParams) {
            if (this.aReportParams.hasOwnProperty(key)) length++;
        }
        return length;
    };

    ///summary
    ///Affecte un paramètre simple (valeur unique associée à la clé) au rapport
    ///Le paramètre doit avoir été ajouté au préalable par eReportWizardRenderer.BuildJavascriptReportParams()
    ///Le tableau est construit par ce renderer à partir des valeurs renvoyées par eReportWizardRenderer.GetReportParamList()
    ///<param name="key">clé de paramètre de rapport</param>
    ///<param name="val">valeur</param>
    ///summary
    this.SetParam = function (key, val) {
        if (this.KeyExists(key))
            this.aReportParams[key] = val;
    }

    ///summary
    ///Affecte un paramètre complexe (valeurs multiples associées à la clé) au rapport
    ///Le paramètre doit avoir été ajouté au préalable par eReportWizardRenderer.BuildJavascriptReportParams()
    ///Le tableau est construit par ce renderer à partir des valeurs renvoyées par eReportWizardRenderer.GetReportParamList()
    ///<param name="key">clé de paramètre de rapport</param>
    ///<param name="val">valeur</param>
    ///summary
    this.SetComplexParam = function (key, val, bdelete) {
        if (this.KeyExists(key))
            this.aReportParams[key] = this.SetValue(this.aReportParams[key], val, bdelete);
    }
    ///summary
    ///Affecte la valeur dans le paramètre associé en vérifiant s'il existe ainsi que ses paramètres avancés
    ///<param name="paramString">valeur de paramètre du rapport</param>
    ///<param name="val">nouvelle valeur</param>
    ///summary
    this.SetValue = function (paramString, val, bdelete) {
        var descId = val.split(",")[0];
        var valueArray = paramString.split(";");
        var bFound = false;
        if (paramString == "") {
            return val;
        }

        for (valIdx = 0; valIdx < valueArray.length; valIdx++) {
            if (valueArray[valIdx] == "")
                continue;

            if (valueArray[valIdx].split(",")[0] == descId) {
                valueArray[valIdx] = bdelete ? "" : val;
                bFound = true;
                break;
            }
        }

        if (!bFound)
            valueArray.push(val);

        var res = valueArray.join(";").trim().replace(";;", ";");
        if (res.indexOf(";") == 0)
            res = res.substring(1);
        if (res.length > 0 && res.lastIndexOf(";") + 1 == res.length)
            res = res.substring(0, res.length - 1);

        return res;
    }
    this.GetSQLParamString = function () {
        var key = "";
        var SQLString = new Array();
        for (key in this.aReportParams) {
            SQLString.push(key + "=" + (this.aReportParams[key].indexOf('&') != -1 ? this.aReportParams[key].replace(/&/gi, '#AMP#') : this.aReportParams[key]));
        }

        return encode(SQLString.join("&"));
    }

    this.UpdateMode = function (permCode) {
        if (permCode == "view") {
            if (parseInt(this._aViewPerm["level"]) > 0
                && this._aViewPerm["user"] != "") {
                this._aViewPerm["mode"] = "2";
            }
            else if (parseInt(this._aViewPerm["level"]) > 0 && this._aViewPerm["user"] == "") {
                this._aViewPerm["mode"] = "0";
            }
            else if ((parseInt(this._aViewPerm["level"]) <= 0 || isNaN(parseInt(this._aViewPerm["level"]))) && this._aViewPerm["user"] != "") {
                this._aViewPerm["mode"] = "1"
            }
            else {
                this._aViewPerm["mode"] = "-1";
            }
        }
        else if (permCode == "update") {
            if (parseInt(this._aUpdatePerm["level"]) > 0
                && this._aUpdatePerm["user"] != "") {
                this._aUpdatePerm["mode"] = "2";
            }
            else if (parseInt(this._aUpdatePerm["level"]) > 0 && this._aUpdatePerm["user"] == "") {
                this._aUpdatePerm["mode"] = "0";
            }
            else if ((parseInt(this._aUpdatePerm["level"]) <= 0 || isNaN(parseInt(this._aUpdatePerm["level"]))) && this._aUpdatePerm["user"] != "") {
                this._aUpdatePerm["mode"] = "1"
            }
            else {
                this._aUpdatePerm["mode"] = "-1";
            }
        }
    }

    //#region Power BI
    this.SelectPowerBIUrl = function (control) {
        if (control) {
            var returnValue = control.value;
            copyControlTextToClipboard(control);
        }
    };

    this.SelectPowerBIUser = function (control) {
        if (control) {
            modalPowerBIUserCat = new eModalDialog(top._res_246, 0, "eCatalogDialogUser.aspx", 550, 610);
            top.eTabCatUserModalObject.Add(modalPowerBIUserCat.iframeId, modalPowerBIUserCat);
            modalPowerBIUserCat.addParam("iframeId", modalPowerBIUserCat.iframeId, "post");
            modalPowerBIUserCat.ErrorCallBack = function () { setWait(false); }
            modalPowerBIUserCat.addParam("multi", "0", "post");
            modalPowerBIUserCat.addParam("selected", control.getAttribute("ednvalue"), "post");
            modalPowerBIUserCat.addParam("modalvarname", "modalUserCat", "post");
            modalPowerBIUserCat.show();
            // Les 2 fonctions onSelectPowerBIUser* doivent être hors namespace eReport pour que leur interaction avec eModalDialog et eCatalogUser.SetSelDblClick
            // fonctionnent correctement
            // Ainsi que pour gérer le double-clic sur la liste des utilisateurs via CallOnOk ci-dessous
            modalPowerBIUserCat.CallOnOk = function () { onSelectPowerBIUserValidate(control.id); };
            modalPowerBIUserCat.addButton(top._res_29, onSelectPowerBIUserCancel, "button-gray", control.id, null, true);
            modalPowerBIUserCat.addButton(top._res_28, onSelectPowerBIUserValidate, "button-green", control.id);
        }
    };

    this.SetPowerBIUser = function (value) {
        oReport.SetParam("powerbiuser", value);
    };

    this.SelectPowerBIExpirationDate = function (control) {
        if (control) {
            var label = document.querySelector(".editor_powerbi_date_label").firstChild.innerText;
            var sDate = getAttributeValue(control, "dbv");

            modalPowerBIExpirationDate = createCalendarPopUp("onSelectPowerBIExpirationDateValidate", 0, 1, label, top._res_5003, "onSelectPowerBIExpirationDateOk", top._res_29, null, null, wizardIframe.id, control.id, control.value);
            modalPowerBIExpirationDate.activeInputId = control.id;
        };
    };

    this.SetPowerBIExpirationDate = function (value) {
        oReport.SetParam("powerbidate", value);
    };

    this.SetPowerBIExecutionMode = function (control) {
        if (control)
            oReport.SetParam("powerbimode", control.value);

        var calendarWorkHourBeginCtrl = document.getElementById("editor_powerbi_calendarworkhourbegin");
        if (calendarWorkHourBeginCtrl)
            oReport.SetParam("WorkHourBegin", calendarWorkHourBeginCtrl.value);

        var calendarWorkHourEndCtrl = document.getElementById("editor_powerbi_calendarworkhourend");
        if (calendarWorkHourEndCtrl)
            oReport.SetParam("WorkHourEnd", calendarWorkHourEndCtrl.value);
    };

    this.SetPowerBIFormat = function (control) {
        if (control) {
            oReport.SetParam("powerbiformat", control.value);

            var columnSeparatorCntr = document.getElementById("editor_powerbi_columnseparator_container");
            var columnSeparatorCustomValueCtrl = document.getElementById("editor_powerbi_columnseparatorcustomvalue_input");

            if (control.value == "csv") {
                columnSeparatorCntr.style.display = "block";
                columnSeparatorCustomValueCtrl.style.display = (columnSeparatorCntr.options.selectedIndex == 6 ? "block" : "none");
            }
            else {
                columnSeparatorCntr.style.display = "none";
            }
        }
    };

    this.SetPowerBIColumnSeparator = function (control) {
        if (control) {
            oReport.SetParam("columnseparator", control.value);
            var columnSeparatorCustomValueCtrl = document.getElementById("editor_powerbi_columnseparatorcustomvalue_input");
            if (control.value == "custom")
                columnSeparatorCustomValueCtrl.style.display = "block";
            else
                columnSeparatorCustomValueCtrl.style.display = "none";
        }
    };

    this.SetPowerBIColumnSeparatorCustomValue = function (control) {
        if (control) {
            // Si l'utilisateur a saisi au moins un caractère spécial comme séparateur de colonnes personnalisé, on annule sa saisie et on la remplace par le
            // séparateur de colonnes le plus proche. Ceci, pour ne pas stocker une valeur incorrecte dans ReportParam pouvant mettre en péril le découpage de
            // la chaîne de paramètres
            var columnSeparatorControl = document.getElementById("editor_powerbi_columnseparator_select");
            var correctedColumnSeparatorControlValue = "custom";
            if (control.value.indexOf(",") != -1)
                correctedColumnSeparatorControlValue = "comma";
            else if (control.value.indexOf(";") != -1)
                correctedColumnSeparatorControlValue = "semicolon";
            else if (control.value.indexOf(":") != -1)
                correctedColumnSeparatorControlValue = "colon";
            else if (control.value.indexOf("=") != -1)
                correctedColumnSeparatorControlValue = "equals";
            else if (control.value.indexOf(" ") != -1)
                correctedColumnSeparatorControlValue = "space";
            else if (control.value.indexOf("\t") != -1)
                correctedColumnSeparatorControlValue = "tab";

            for (var i = 0; i < columnSeparatorControl.options.length; i++) {
                if (columnSeparatorControl.options[i].value == correctedColumnSeparatorControlValue) {
                    columnSeparatorControl.options.selectedIndex = i;
                    // Mise à jour de l'affichage (masquage/affichage du champ de saisie de caractère personnalisé)
                    this.SetPowerBIColumnSeparator(columnSeparatorControl);
                }
            }
            if (correctedColumnSeparatorControlValue != "custom")
                oReport.SetParam("columnseparatorcustom", "");
            else {
                oReport.SetParam("columnseparatorcustom", control.value);

            }
        }
    };
};

function onChange(strEltId) {
    var oSelectedElt = document.getElementById(strEltId);

    if (!oSelectedElt)
        return;

    if (getAttributeValue(oSelectedElt, 'chk') == '0')
        setAttributeValue(oSelectedElt, 'chk', '1');
    else
        setAttributeValue(oSelectedElt, 'chk', '0');
}

// Les 2 fonctions onSelectPowerBI* doivent être hors namespace eReport pour que leur interaction avec eModalDialog et ses variables globales fonctionnent correctement
function onSelectPowerBIUserValidate(trgId) {
    var strReturned = modalPowerBIUserCat.getIframe().GetReturnValue();
    modalPowerBIUserCat.hide();
    var vals = strReturned.split('$|$')[0];
    var libs = strReturned.split('$|$')[1];
    var oTarget = document.getElementById(trgId);
    oTarget.value = libs;
    oTarget.setAttribute("title", libs);
    oTarget.setAttribute("ednvalue", vals);
    // La fonction onChange du champ de saisie ne se déclenche pas automatiquement au remplissage de .value
    oReport.SetPowerBIUser(vals);
};

function onSelectPowerBIUserCancel() {
    modalPowerBIUserCat.hide();
};

// La fonction Ok passée à createCalendarPopUp, câblée sur le double clic du calendrier, doit apparemment être différente de la fonction du bouton Valider
// sinon, les 2 fonctions s'appellent récursivement en boucle infinie
function onSelectPowerBIExpirationDateOk() {

}

function onSelectPowerBIExpirationDateValidate(date) {
    var dbv = eDate.ConvertDisplayToBdd(date);
    var eltInput = document.getElementById("editor_powerbi_date_input");
    eltInput.setAttribute("dbv", dbv);
    eltInput.value = date;
    modalPowerBIExpirationDate.hide();
    // La fonction onChange du champ de saisie ne se déclenche pas automatiquement au remplissage de .value
    oReport.SetPowerBIExpirationDate(dbv);
}
