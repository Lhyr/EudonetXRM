/// <reference path="eTools.js" />
/// <reference path="eModalDialog.js" />

// <filename : eFilterReportListDialog.js />

//utilisation d'un NS pour éviter les colisition de varname
var nsFilterReportList = {};

var _eReportNameEditor;
var _ePopupVNEditor;
var _eFormularNameEditor;
var _eFilterNameEditor;
var modalWizard;

function selectLine(obj, bDeselectAllowed) {
    if (typeof (bDeselectAllowed) === "undefined")
        bDeselectAllowed = false;

    // Get parent
    var elem = obj;
    while (elem.tagName != 'TR') {

        elem = elem.parentNode || elem.parentElement;

        if (elem.tagName == 'TBODY')
            return;
    }

    if (!bDeselectAllowed) {
        if (_eCurentSelectedFilter != null)
            removeClass(_eCurentSelectedFilter, "eSel");
        if (addClass != null)
            addClass(elem, "eSel");
        _eCurentSelectedFilter = elem;
        var aEid = getAttributeValue(_eCurentSelectedFilter, "eid").split("_");
        if (aEid.length >= 2) {
            _activeFilter = aEid[1];
        }
    }
    else {
        removeClass(_eCurentSelectedFilter, "eSel");
        if (_eCurentSelectedFilter == elem) {
            _eCurentSelectedFilter = null;
            _activeFilter = "";
        }
        else {
            if (addClass != null)
                addClass(elem, "eSel");
            _eCurentSelectedFilter = elem;
            var aEid = getAttributeValue(_eCurentSelectedFilter, "eid").split("_");
            if (aEid.length >= 2) {
                _activeFilter = aEid[1];
            }

        }
    }

    var sFrmName = GetObjectFrameName(elem);
    if (sFrmName != "") {
        // a partir de la frame, on récupère les boutons

        var sBtnDivId = "ButtonModal" + sFrmName.replace("frm", "");
        var oBtb = top.document.getElementById(sBtnDivId);
        if (oBtb) {
            var o = oBtb.querySelector("div[ednmodalbtn='1'][id='scheduleReport']");
            if (o) {

                // si liste par def ou rapport avec confirmation de titre
                var bConfirm = getAttributeValue(_eCurentSelectedFilter, "confirm") + "" == "1";
                if (_activeFilter + "" == "-1" || bConfirm)
                    o.style.display = "none";
                else
                    o.style.display = "";
            }
        }
    }

}

function deselectLine() {
    removeClass(_eCurentSelectedFilter, "eSel");
    _eCurentSelectedFilter = null;
    _activeFilter = "";
}

nsFilterReportList.SelectFilterReport = function (nDescId, nId) {
    var tabList = document.getElementById("mt_" + nDescId);
    var tr = tabList.querySelector("tr[eid='" + nDescId + "_" + nId + "']");
    if (tr)
        selectLine(tr);
};

nsFilterReportList.SelectReport = function (nReportId) {
    nsFilterReportList.SelectFilterReport(105000, nReportId);
};

nsFilterReportList.GetSelectedReport = function () {
    var aEid = getAttributeValue(_eCurentSelectedFilter, "eid").split("_");
    if (aEid.length >= 2) {
        return getNumber(aEid[1]);
    }
    return 0;
};


nsFilterReportList.onPlanifyReport = function (reportId) {
    if (typeof (reportId) == "undefined" || reportId + "" == "0") {
        eAlert(0, top._res_719, top._res_6058);
        return;
    }

    var oModalScheduleParam = wizardIframe.contentWindow.oReport.GetSchedule();

    var d = new Date();
    var yyyy = d.getFullYear().toString();
    var mm = (d.getMonth() + 1).toString(); // getMonth() is zero-based
    var dd = d.getDate().toString();

    var dNow = (dd[1] ? dd : "0" + dd[0]) + "/" + (mm[1] ? mm : "0" + mm[0]) + "/" + yyyy;

    //    var dNow = d.getDate() + "/" + d.getMonth() +1 + "/" + 


    //empty schedule 
    if (oModalScheduleParam == null) {
        oModalScheduleParam = {
            "FrequencyType": "0",
            "Frequency": "1",
            "Day": "0",
            "Order": "0",
            "WeekDays": [],
            "Month": "0",
            "StartDate": dNow,
            //  "EndDate": "",
            "Repeat": "5",
            "Hour": "22:00",
            "ReportParam": {
                "ReportId": 0,
                "TabFrom": 0,
                "TabBkm": 0,
                "FileId": 0
            },
            "Recipients": [],

            "FTPParams": {
                "UseFtp": false,
                "URL": "",
                "Login": "",
                "Password": "",
                "Passive": false,
                "SSL": false,
                "Path": ""
            }
        }

    }

    var sScheduleParam = JSON.stringify(oModalScheduleParam);

    var modalSchedule = new eModalDialog(top._res_1049, 0, "eSchedule.aspx", 500, 400, "mdlReportSchedule");


    modalSchedule.addParam("jsonparam", sScheduleParam, "post");

    modalSchedule.addParam("scheduletype", 1, "post");
    modalSchedule.addParam("New", 1, "post");
    modalSchedule.addParam("iframeScrolling", "yes", "post");
    modalSchedule.addParam("EndDate", 0, "post");
    modalSchedule.addParam("BeginDate", 0, "post");
    modalSchedule.addParam("ScheduleId", 0, "post");
    modalSchedule.addParam("Tab", 0, "post");
    modalSchedule.addParam("Workingday", "", "post");
    modalSchedule.addParam("calleriframeid", 0, "post");
    modalSchedule.addParam("AppType", 0, "post");
    modalSchedule.addParam("FileId", 0, "post");

    modalSchedule.ErrorCallBack = function () { modalSchedule.hide(); };
    modalSchedule.onIframeLoadComplete = function () {
        nsFilterReportList.LoadedSchedule(modalSchedule);
    };

    modalSchedule.show();
    modalSchedule.addButtonFct(top._res_29, function () { modalSchedule.hide(); }, "button-gray", 'cancel');

    return modalSchedule;
}

//permet de planifier un rapport depuis une liste -deprecated, finalement, on ne passe plus par la liste pour planifier mais par le wizard
// il n'est donc plus possible de faire plusieurs planif pour un même rapport.
nsFilterReportList.onPlanifyList = function (oModalFilterList) {


    var Mod = nsFilterReportList.onPlanifyReport(_activeFilter);

    //Création du bouton de validation
    Mod.addButtonFct(top._res_28, function () { nsFilterReportList.ValidScheduleReport(Mod, oModalFilterList); }, "button-green");
}

//permet de planifier un rapport depuis le wizard
nsFilterReportList.onPlanifyWiz = function (id) {


    if (!wizardIframe || !wizardIframe.contentWindow)
        return;

    var oEnabled = wizardIframe.contentWindow.document.getElementById("report_schedule_enabled");

    //Si la planification est activée : 
    //var oEnabled = document.
    if (oEnabled && getAttributeValue(oEnabled, "chk") == "1") {

        var Mod = nsFilterReportList.onPlanifyReport(id);
        if (Mod.isModalDialog) {

            //Ajout du bouton de validation du schedule
            Mod.addButtonFct(top._res_28, function () {

                var s = nsFilterReportList.GetJsonScheduleReport(Mod);
                if (s != null) {
                    wizardIframe.contentWindow.oReport.SetSchedule(s);


                    Mod.hide();
                }

            }, "button-green");
        }


    }
}


///Retourne une chaine de char représentant un JSON de schedule
nsFilterReportList.GetScheduleReport = function (modalSchedule) {

    //Récupération du Json
    var jsonRes = nsFilterReportList.GetJsonScheduleReport(modalSchedule);
    var sJson = "";
    if (jsonRes == null) {
        return;
    }



    //to String
    return JSON.stringify(jsonRes)

}

nsFilterReportList.ValidScheduleReport = function (modalSchedule) {

    sJson = nsFilterReportList.GetScheduleReport(modalSchedule);

    var updSchedule = new eUpdater("mgr/eReportManager.ashx", 0);
    updSchedule.addParam("scheduledata", sJson, "post");
    updSchedule.addParam("operation", 12, "post");

    //Créer une methode en cas d'une erreur il l'execute
    updSchedule.ErrorCallBack = function () {
        eAlert(0, top._res_72, top._res_1806);
        top.setWait(false);
        modalSchedule.hide();
    };

    top.setWait(true);
    updSchedule.send(function (oRes) {

        nsFilterReportList.OnValidScheduleReportOk(oRes, modalSchedule);

    });

}

nsFilterReportList.OnValidScheduleReportOk = function (oRes, oModal) {

    top.setWait(false);

    if (!oModal || !oModal.isModalDialog) {
        eAlert(0, top._res_72, top._res_1806);
        return null;
    }
    var bsuccess = getXmlTextNode(oRes.getElementsByTagName("success")[0]) == "1";
    if (!bsuccess) {
        eAlert(0, top._res_72, top._res_1806);
        return null;
    }

    var dt = getXmlTextNode(oRes.getElementsByTagName("NextAction")[0]);

    eAlert(4, top._res_1711, "Ce rapport a été plannifié. Le premier lancement aura lieu le  : [[BR]]" + dt);

    oModal.hide();
}


///Retourne  un JSON de schedule
//TODO : a unifier avec le valid de schedule.js
nsFilterReportList.GetJsonScheduleReport = function (oModal) {

     if (!oModal || !oModal.isModalDialog) {
        eAlert(0, top._res_72, top._res_1806);
        return null;
    }
    try {

        var oDoc = oModal.getIframe().document;


        var d = new Date();
        var yyyy = d.getFullYear().toString();
        var mm = (d.getMonth() + 1).toString(); // getMonth() is zero-based
        var dd = d.getDate().toString();

        var dNow = (dd[1] ? dd : "0" + dd[0]) + "/" + (mm[1] ? mm : "0" + mm[0]) + "/" + yyyy;


        //Construit le JSON de la classe eScheduleDatas
        var myScheduleDatas = {
            "FrequencyType": "0",
            "Frequency": "1",
            "Day": "0",
            "Order": "0",
            "WeekDays": [],
            "Month": "0",
            "StartDate": dNow,
            //  "EndDate": "",
            "Repeat": "5",
            "Hour": "22:00",
            "ReportParam": {
                "ReportId": 0,
                "TabFrom": 0,
                "TabBkm": 0,
                "FileId": 0
            },
            "Recipients": [],

            "FTPParams": {
                "UseFtp": false,
                "URL": "",
                "Login": "",
                "Password": "",
                "Passive": false,
                "SSL": false,
                "Path": ""
            }

        };


        var currentView = top.getCurrentView();
        var bFile = (currentView.indexOf("FILE_") == 0);


        var myBase = listIframe.contentWindow;

        //NeedPoint de départ -> Modal de la liste de filtre
        var nTab = getNumber(myBase.document.getElementById("mainDiv").attributes["tab"].value);
        var nTabBkm = getNumber(myBase.document.getElementById("mainDiv").attributes["tabBKM"].value);


        //NeedPoint de départ -> fenetre principale
        var fid = 0;
        if (bFile || nTabBkm > 0)
            fid = myBase.parent.document.getElementById("fileDiv_" + parent.nGlobalActiveTab).attributes["fid"].value;    //File Id du mode fiche en cours

        fid = getNumber(fid);

        var oParam = {
            "ReportId": myBase._activeFilter,
            "TabFrom": nTab,
            "FileId": fid,
            "TabBkm": nTabBkm
        };


        myScheduleDatas.ReportParam = oParam;


        /*  Fréquence */
        var oSchedType = oDoc.getElementById("ScheduleTypeList");
        var sFrequency = oSchedType.options[oSchedType.selectedIndex].value;
        var nFrequency = "0";
        switch (sFrequency) {
            case "daily":

                if (oDoc.getElementById("dailyEveryWorkingDay") && oDoc.getElementById("dailyEveryWorkingDay").checked) {
                    myScheduleDatas.FrequencyType = "1";
                    myScheduleDatas.WeekDays = [2, 3, 4, 5, 6];  
                }
                else {
                    myScheduleDatas.FrequencyType = "0";
                    var nF = getNumber(oDoc.getElementById("daily_day").value);
                    if (nF < 1) {
                        eAlert(0, top._res_72, top._res_1806); //LA FREQUENCE DOIT AU MOINS ETRE 1
                        //oModal.hide();
                        return null;
                    }
                    myScheduleDatas.Frequency = nF;

                }
                break
            case "once":
                myScheduleDatas.FrequencyType = "0";
                myScheduleDatas.Frequency = 1
                break
            case "weekly":
                myScheduleDatas.FrequencyType = "2";
                var nF = getNumber(oDoc.getElementById("weekly_weekday").value);
                if (nF < 1) {
                    eAlert(0, top._res_72, top._res_1806); //LA FREQUENCE DOIT AU MOINS ETRE 1
                    return null;
                }

                myScheduleDatas.Frequency = nF;
                var oLst = oDoc.querySelectorAll("input[type='checkbox'][id^='WorkWeekDay']");
                for (nCMpt = 0; nCMpt < oLst.length; nCMpt++) {
                    var chek = oLst[nCMpt];
                    if (chek.checked) {
                        myScheduleDatas.WeekDays.push(getNumber(chek.value));
                    }
                }

                if (myScheduleDatas.WeekDays.length == 0) {
                    strDesc = top._res_1067.replace("<ITEM>", top._res_822);
                    eAlert(0, top._res_72, strDesc); // VOUS DEVEZ CHOISIR AU MOINS UN JOUR
                    return null;
                }
                break;
            case "monthly":
                myScheduleDatas.FrequencyType = "3";

                if (oDoc.getElementById("monthlyEvery").checked) {
                    myScheduleDatas.Day = getNumber(oDoc.getElementById("monthly_day").value);

                    if (myScheduleDatas.Day < 0 || myScheduleDatas.Day > 31) {
                        eAlert(0, top._res_72, top._res_1070); // Jour non valude
                        return null;
                    }

                    myScheduleDatas.Frequency = getNumber(oDoc.getElementById("monthly_month").value);

                    if (myScheduleDatas.Frequency <= 0) {
                        strDesc = top._res_1067.replace("<ITEM>", top._res_405);
                        eAlert(0, top._res_72, strDesc); // VOUS DEVEZ CHOISIR AU MOINS UN MOIS
                        return null;
                    }

                }
                else if (oDoc.getElementById("monthlyOrder").checked) {

                    /* order */
                    var oOrderSel = oDoc.getElementById("monthly_order_1");
                    myScheduleDatas.Order = getNumber(oOrderSel.options[oOrderSel.selectedIndex].value);
                    if (myScheduleDatas.Day < 0 || myScheduleDatas.Day > 5) {
                        eAlert(0, top._res_72, top._res_1070); // Jour non valude
                        return null;
                    }
                    var oWeekDaySel = oDoc.getElementById("monthly_weekday_1");
                    var nDay = getNumber(oWeekDaySel.options[oWeekDaySel.selectedIndex].value) - 1;
                    if (nDay < 0 || nDay > 7) {
                        eAlert(0, top._res_72, top._res_1070); // Jour non valude
                        return null;
                    }

                    myScheduleDatas.Frequency = getNumber(oDoc.getElementById("monthly_month_1").value);
                    myScheduleDatas.WeekDays.push(nDay);

                }
                else {
                    eAlert(0, top._res_72, top._res_1806); // VOUS DEVEZ CHOISIR UNE OPTION DE FREQUENCY
                    return null;
                }

                break;
            case "10minutely":
                myScheduleDatas.FrequencyType = "99";
                myScheduleDatas.Frequency = 10;
                break;
            case "5minutely":
                myScheduleDatas.FrequencyType = "99";
                myScheduleDatas.Frequency = 5;
                break;
            case "minutely":
                myScheduleDatas.FrequencyType = "99";
                myScheduleDatas.Frequency = 1;
                break;
            case "yearly":
                myScheduleDatas.FrequencyType = "4";

                if (oDoc.getElementById("yearlyEvery").checked) {
                    var nDay = getNumber(oDoc.getElementById("yearly_day").value);
                    var oMonth = oDoc.getElementById("yearly_month");
                    var oMonthSel = oMonth.options[oMonth.selectedIndex];
                    var nMonth = getNumber(oMonthSel.value);
                    var nMaxVal = getNumber(getAttributeValue(oMonthSel, "ednday"));
                    if (nDay > nMaxVal) {
                        eAlert(0, top._res_72, top._res_1070); // Jour non valude
                        return null;
                    }
                    myScheduleDatas.Day = nDay;
                    myScheduleDatas.Month = nMonth;
                }
                else if (oDoc.getElementById("yearlyOrder").checked) {

                    var oOrder1 = oDoc.getElementById("yearly_order_1");
                    var oDay1 = oDoc.getElementById("yearly_weekday_1");
                    var oMonth = oDoc.getElementById("yearly_month_1");

                    var nOrder1Sel = getNumber(oOrder1.options[oOrder1.selectedIndex].value);
                    var nDay1Sel = getNumber(oDay1.options[oDay1.selectedIndex].value) - 1;
                    var nMonthSel = getNumber(oMonth.options[oMonth.selectedIndex].value);

                    myScheduleDatas.Order = nOrder1Sel;
                    myScheduleDatas.WeekDays.push(nDay1Sel);
                    myScheduleDatas.Month = nMonthSel;
                }
                else {
                    eAlert(0, top._res_72, top._res_1806); // VOUS DEVEZ CHOISIR UNE OPTION DE FREQUENCY
                    return null;

                }
                break;
        }


        /*  Horaire */
        var oHourSelect = oDoc.getElementById("HourSelect");
        var sHourSelect = oHourSelect.options[oHourSelect.selectedIndex].value;
        myScheduleDatas.Hour = sHourSelect;





        /*  Période */
        var sStartDate = oDoc.getElementById("RangeBegin").value;
        if (!nsFilterReportList.checkValue(oDoc.getElementById("RangeBegin"), true)) {
            return null;
        }
        myScheduleDatas.StartDate = eDate.ConvertDisplayToBdd(sStartDate);

        if (oDoc.getElementById("RangeEndChk").checked) {
            var sEndDate = oDoc.getElementById("RangeEndDate").value;
            if (!nsFilterReportList.checkValue(oDoc.getElementById("RangeEndDate"), true)) {
                return null;
            }
            myScheduleDatas.EndDate = eDate.ConvertDisplayToBdd(sEndDate);
            myScheduleDatas.Repeat = 0;

        } else {


            var sRepeat = oDoc.getElementById("RangeCount").value;
            if (!nsFilterReportList.checkValue(oDoc.getElementById("RangeCount"), true)) {
                return null;
            }


            myScheduleDatas.Repeat = sRepeat;
            myScheduleDatas.EndDate = '01/01/0001';
        }




        /*  Destinataire */
        /*
        if (!nsFilterReportList.checkValue(oDoc.getElementById("RecipientsUser"), true))
            return null;

        if (!nsFilterReportList.checkValue(oDoc.getElementById("RecipientsCustom"), true))
            return null;

        var sEudoUser = getAttributeValue(oDoc.getElementById("RecipientsUser"), "ednvalue");
        var sUserCustom = oDoc.getElementById("RecipientsCustom").value;


        var aEudoUser = [];

        if (sEudoUser.length > 0)
            aEudoUser = sEudoUser.split(";");

        var aUserCustom = [];
        if (sUserCustom.length > 0)
            aUserCustom = sUserCustom.split(";");


        if (aUserCustom.length == 0 && aEudoUser.length == 0) {
            eAlert(0, top._res_72, top._res_577);
            return;
        }
        
        myScheduleDatas.Recipients = aEudoUser.concat(aUserCustom);
        */
        myScheduleDatas.Recipients = [];

        return myScheduleDatas;


    }
    catch (e) {

        eAlert(0, top._res_72, top._res_1806);
        return null;
    }
}


// Affecte a chaque inpput une fct de vérification la validité des valeurs
nsFilterReportList.LoadedSchedule = function (oModal) {

    if (!oModal || !oModal.isModalDialog) {
        eAlert(0, top._res_72, top._res_1806);
        return;
    }

    var oDoc = oModal.getIframe().document;
    //Checker input
    var allInpt = oDoc.querySelectorAll("input[endcheck='1']");

    forEach(allInpt, function (curInpt) {
        curInpt.onchange = function () {
            nsFilterReportList.checkValue(curInpt, true);
        }
    });
}


nsFilterReportList.checkRange = function (inptChk, bDisplayMsg) {

    var nValue = getNumber(inptChk.value);
    var bValid = true;
    if (inptChk.hasAttribute) {
        if (inptChk.hasAttribute("ednrngmin")) {
            if (nValue < getNumber(getAttributeValue(inptChk, "ednrngmin"))) {
                bIsValid = false;
                sDesc = top._res_2001 + ' ' + getAttributeValue(inptChk, "ednrngmin");
            }
        }

        if (inptChk.hasAttribute("ednrngmax")) {
            if (nValue > getNumber(getAttributeValue(inptChk, "ednrngmax"))) {
                bIsValid = false;
                sDesc = top._res_2003 + ' ' + getAttributeValue(inptChk, "ednrngmax");
            }

        }
    }

    if (bDisplayMsg && !bValid)
        eAlert(0, top._res_72, sDesc);

    return bValid;
}

///Vérifie la valeur d'une saisie
nsFilterReportList.checkValue = function (inptChk, bDisplayMsg) {

    var sType = getAttributeValue(inptChk, "edntype");



    var sValue = getAttributeValue(inptChk, "ednvalue") + "";
    if (sValue.length == "")
        sValue = inptChk.value;

    var sTitle = top._res_72;
    var sDesc = "";
    var sDetail = "";

    var bIsValid = true;
    var sDefVal = "";

    var bMulti = getAttributeValue(inptChk, "ednmulti") + "" == "1";

    switch (sType) {
        case "num":

            sDefVal = 1;
            if (bMulti) {
                sCleanVal = CleanListIds(sValue, ";");
                if (sCleanVal.split(";").length != sValue.split(";").length) {
                    bIsValid = false;
                    sDesc = top._res_673; // Vous devez saisir une valeur numérique
                    break;
                }
            }
            else {
                var strNumCheck = eNumber.ConvertDisplayToBdd(sValue);


                if (!eValidator.isNumeric(strNumCheck)) {
                    bIsValid = false;
                    sDesc = top._res_673; // Vous devez saisir une valeur numérique
                    break;
                }

                if (!nsFilterReportList.checkRange(inptChk, bDisplayMsg))
                    return;
            }


            break;
        case "date":

            var strDateCheck = eDate.ConvertDisplayToBdd(sValue);

            if (!eValidator.isDate(strDateCheck)) {
                bIsValid = false;
                sDesc = top._res_846; // date invalide
            }
            break;

        case "mail":

            var mailValid = true;
            var mails = sValue.split(";");
            var invalidMails = "";
            var sCleanVal = "";

            if (!bMulti && mails.length > 1) {
                bIsValid = false;
                sDesc = top._res_1909; // Une seule valeur supportée
            } else {

                for (var i = 0; i < mails.length; i++) {
                    if (mails[i].length > 0 && !eValidator.isEmail(mails[i]))
                        invalidMails = invalidMails + mails[i] + "<br />";
                    else if (mails[i].length > 0) {
                        if (sCleanVal.length > 0)
                            sCleanVal += ";"

                        sCleanVal += mails[i];
                    }
                }
            }

            if (invalidMails.length > 0) {
                bIsValid = false;
                sDesc = top._res_1147;
                sTitle = top._res_6275;
                sDetail = invalidMails;
                sDefVal = sCleanVal;
            }

            break;
    }



    if (!bIsValid) {
        if (bDisplayMsg) {
            eAlert(0, sTitle, sDesc, sDetail, null, null, function () {
                inptChk.value = sDefVal;
                inptChk.focus();
            });
        }
        else {
            inptChk.value = sDefVal;
            inptChk.focus();
        }
    }

    return bIsValid;
}

//REGION #FILTER#
/// <summary>
/// Initie le renommage d'un filtre via appel à un eFieldEditor
///   fonction de efieldeditor : renameFilter via onClick
/// </summary>
/// <param name="baseName">Id de la cellule</param>
/// <param name="elem">Bouton cliqué (déclencheur de l'action de renommage)</param>
function renFilter(baseName, elem) {

    var libElem = document.getElementById(baseName);

    if (libElem)
        _eFilterNameEditor.onClick(libElem, elem);
}

/// <summary>
/// Boite de confirmation de suppression de filtre
///   fonction de efieldeditor : renameFilter via onClick
/// </summary>
function delFilter(nFilterId, nTab) {
    eConfirm(1, top._res_806, top._res_266, '', 500, 200, function () { onDeleteFilter(nFilterId, nTab) });
}

/// <summary>
/// Suppression du filtre & reload de la page
/// </summary>
function onDeleteFilter(nFilterId, nTab) {
    var url = "mgr/eFilterWizardManager.ashx";
    var ednu = new eUpdater(url, 0);
    ednu.addParam("action", "delete", "post");
    ednu.addParam("maintab", nTab, "post");
    ednu.addParam("filterid", nFilterId, "post");
    ednu.ErrorCallBack = function () { };
    ednu.send(onDeleteFilterTrait);
}

/// <summary>
/// Retour de suppression
///   -> maj la liste ou affiche un msg d'erreur/warningx
/// </summary>
/// <param name="oDoc">Flux XML de retour</param>
function onDeleteFilterTrait(oDoc) {
    if (oDoc && oDoc.nodeType == 9) {

        var bSuccess = (getXmlTextNode(oDoc.getElementsByTagName("success")[0]) == "1");
        var nId = getXmlTextNode(oDoc.getElementsByTagName("filterid")[0]);


        if (!bSuccess) {

            var nErrCode = getXmlTextNode(oDoc.getElementsByTagName("ErrorCode")[0]);
            var sErrDesc = getXmlTextNode(oDoc.getElementsByTagName("ErrorDescription")[0]);

            var errorTitle = top._res_6530 + " : ";
            if (nErrCode == "2")
                errorTitle = top._res_69;

            showWarning("", errorTitle, sErrDesc);

        }
        else {

            // Succes de la MaJ
            // déselectione la ligne
            // reload la liste
            deselectLine();
            if (oIframe) {

                var oTable = oIframe.contentDocument.getElementById("mt_104000");
                var nNbRow = oTable.rows.length;

                for (var nCmpt = 0; nCmpt < nNbRow; nCmpt++) {
                    var oTR = oTable.rows[nCmpt];
                    if (oTR.getAttribute("eid") == "104000_" + nId) {
                        oTable.deleteRow(nCmpt);
                        nCmpt--;
                        nNbRow--;
                    }
                }

                var oLstTR = oTable.querySelectorAll("tbody tr[eid]");

                for (var nCmpt = 0; nCmpt < oLstTR.length; nCmpt++) {

                    removeClass(oLstTR[nCmpt], "list_odd");
                    removeClass(oLstTR[nCmpt], "list_even");

                    var sClassName = nCmpt % 2 ? "list_odd" : "list_even";

                    addClass(oLstTR[nCmpt], sClassName);
                }


            }
        }
    }
}

function onRenErrTreatment(oDoc) {

}


/// <summary>
/// Retour du renommage
///   -> maj la liste ou affiche un msg d'erreur/warning
/// </summary>
/// <param name="oDoc">Flux XML de retour</param>
function onRenOkTreatment(oDoc) {

    if (oDoc && oDoc.nodeType == 9) {

        var bSuccess = (getXmlTextNode(oDoc.getElementsByTagName("success")[0]) == "1");
        var nId = getXmlTextNode(oDoc.getElementsByTagName("filterid")[0]);
        var sNewName = getXmlTextNode(oDoc.getElementsByTagName("filtername")[0]);

        if (!bSuccess) {

            var nErrCode = getXmlTextNode(oDoc.getElementsByTagName("ErrorCode")[0]);
            var sErrDesc = getXmlTextNode(oDoc.getElementsByTagName("ErrorDescription")[0]);

            if (nErrCode == "1") {
                //Erreur : un filre avec le même nom existe déjà
                sErrDesc = top._res_782.replace("<ITEM>", sNewName);
                showWarning(top._res_1713, top._res_719 + " :", sErrDesc);
            }
            else
                showWarning("", " Erreur non gérée :", sErrDesc);
        }
        else {

            // Succes de la MaJ
            var filterTab = document.getElementById("mt_104000");
            var oDivs = filterTab.getElementsByTagName("div");

            //Maj la liste     
            var bFound = false;
            for (var i = 0; i < oDivs.length; i++) {

                var div = oDivs[i];
                if (div.id.indexOf("COL_104000_104001_" + nId + "_" + nId + "_") == 0) {
                    bFound = true;
                    //Gestion bug IE7 sur innerHTML
                    if (div.innerText != null)
                        div.innerText = sNewName;
                    else {
                        if (div.textContent != null)
                            div.textContent = sNewName;
                        else
                            div.innerHTML = sNewName;
                    }
                }
            }



            //CSS de validation contour de champ
            if (typeof (_eFilterNameEditor) == "object") {
                _eFilterNameEditor.flagAsEdited(true);
                //window.setTimeout(function () { _eFilterNameEditor.flagAsEdited(false) }, 500);
            }
        }
    }
}






function ApplySelectedFilter() {

    if (_eCurentSelectedFilter != null && _eCurentSelectedFilter.getAttribute("eid")) {

        var oId = _eCurentSelectedFilter.getAttribute("eid").split('_');
        var nTab = _eCurentSelectedFilter.getAttribute("eft");
        var nId = oId[oId.length - 1];
        top.checkIfFormular(nTab, nId);
        /*var updatePref = "tab=" + nTab + ";$;listfilterid=" + nId  ;
        top.updateUserPref(updatePref, top.loadList);
        top.oModalFilterList.hide();*/

        return

    }
    else {
        showWarning(top._res_719, top._res_430, "");
    }
}


function EditSelectedFilter() {

    if (_eCurentSelectedFilter != null && _eCurentSelectedFilter.getAttribute("eid")) {
        var oId = _eCurentSelectedFilter.getAttribute("eid").split('_');
        var nTab = _eCurentSelectedFilter.getAttribute("eft");
        var nId = oId[oId.length - 1];
        editFilter(nId, nTab);
    }
    else {
        showWarning(top._res_719, top._res_430, "");
    }
}

function AddNewFormular(tab, parentFileId, formularType) {
    editForm(0, tab, parentFileId, formularType);
}

///Ouvre l'assistant de formulaire 
///<param name="formularId">id du formulaire</param>
///<param name="tab">++ ou cible etendu</param>
///<param name="parentFileId">++ ou cible etendu</param>
function editForm(formularId, tab, parentFileId, formularType) {
    var sWidth, sHeight;

    if (formularId == "undefined")
        formularId = 0;
    if (formularType == "undefined" || formularType === undefined)
        formularType = 0;
    else
        formularType = Number(formularType);

    if (parentFileId == 0)
        parentFileId = Number(getAttributeValue(document.getElementById('mainDiv'), "pfid"));

    //Document principal
    var winSize = top.getWindowSize();

    //KJE: tâche 2075 si c'est un formulaire avancé, on affiche le formulaire VueJS sinon on affiche un formulaire classique
    switch (formularType) {
        case null:
        case undefined:
        case 0:
            sWidth = '90%';
            sHeight = '90%';

            modalWizard = new eModalDialog(top._res_1908, 0, "eWizard.aspx", sWidth, sHeight, "FormularWizard"); // Assistant Formulaire

            modalWizard.EudoType = ModalEudoType.WIZARD.toString(); // Type Wizard

            modalWizard.addParam("width", sWidth, "post");
            modalWizard.addParam("height", sHeight, "post");
            modalWizard.addParam("docwidth", winSize.w, "post");
            modalWizard.addParam("docheight", winSize.h, "post");
            modalWizard.addParam("wizardtype", "formular", "post");
            modalWizard.addParam("tab", tab, "post");
            modalWizard.addParam("parentFileId", parentFileId, "post");
            modalWizard.addParam("formularid", formularId, "post");

            modalWizard.addParam("frmId", modalWizard.iframeId, "post");
            modalWizard.addParam("modalId", modalWizard.UID, "post");

            modalWizard.ErrorCallBack = launchInContext(modalWizard, modalWizard.hide);
            modalWizard.show();

            modalWizard.addButton(top._res_345, "modalWizard.getIframe().oFormular.Preview();", "button-gray", null, "preview_btn", 'left');
            modalWizard.addButton(top._res_29, "modalWizard.getIframe().oFormular.Cancel();", "button-gray", null, "cancel_btn", true);
            modalWizard.addButton(top._res_286, "modalWizard.getIframe().oFormular.Save(); ", "button-green", null, "save_btn"); // Enregistrer
            modalWizard.addButton(top._res_869, "modalWizard.getIframe().oFormular.SaveAndExit(); ", "button-green", null, "savenexit_btn"); // Appliquer et Fermer
            modalWizard.addButton(top._res_26, "modalWizard.getIframe().MoveStep(true, 'formular');", "button-green-rightarrow", null, "next_btn"); // Suivant
            modalWizard.addButton(top._res_25, "modalWizard.getIframe().MoveStep(false, 'formular');", "button-gray-leftarrow", null, "previous_btn"); // Précédent

            HideModalButtons();
            break;
        case 1:
            var browser = new getBrowser();
            if (!browser.isIE && !browser.isEdge) {
                sWidth = '100%';
                sHeight = '100%';
                modalWizard = new eModalDialog(top._res_1908, 11, null, sWidth, sHeight); // Assistant Formulaire avancé
                modalWizard.noButtons = true;

                var divContent = document.createElement("div");
                divContent.id = "app_assist_form";
                modalWizard.setElement(divContent);
                modalWizard.show(undefined, undefined, undefined, formularType);

                //KJE, tâche 2 460
                //Créer l'assistant formulaire VueJs en faisant appel au script initFormular
                addScript("../IRISBlack/Front/scripts/components/advFormular/appLoader/eInitFormular", "ADVANCEDFORMULAR", function () {
                    window.top.InitializeFormular(modalWizard, tab, formularId, parentFileId);
                }, window.top.document);
            }
            else eAlert(3, top._res_719, top._res_2814, top._res_2815);

            break;
    }

}


function renFormular(baseName, elem) {
    _ePopupVNEditor = new ePopup('_ePopupVNEditor', 220, 250, 0, 0, document.body, false);
    _eFormularNameEditor = new eFieldEditor('inlineEditor', _ePopupVNEditor, "_eFormularNameEditor ");
    _eFormularNameEditor.action = 'renameFormular';

    var libElem = document.getElementById(baseName);

    if (libElem)
        _eFormularNameEditor.onClick(libElem, elem);
}



function onRenameFormularTrait(oDoc) {
    top.setWait(false);
    if (oDoc && oDoc.nodeType == 9) {

        var bSuccess = (getXmlTextNode(oDoc.getElementsByTagName("success")[0]) == "1");
        var nId = getXmlTextNode(oDoc.getElementsByTagName("formularid")[0]);
        var sNewName = getXmlTextNode(oDoc.getElementsByTagName("label")[0]);

        if (!bSuccess) {

            var nErrCode = getXmlTextNode(oDoc.getElementsByTagName("ErrorCode")[0]);
            var sErrUser = getXmlTextNode(oDoc.getElementsByTagName("message")[0]);
            var sErrDesc = getXmlTextNode(oDoc.getElementsByTagName("ErrorDescription")[0]);

            if (nErrCode == "1") {
                //Erreur : un filre avec le même nom existe déjà
                sErrDesc = top._res_782.replace("<ITEM>", sNewName);
                showWarning(top._res_1713, top._res_719 + " :", sErrDesc);
            }
            else
                if (sErrUser.length > 0)
                    showWarning(top._res_72, sErrUser);
                else
                    showWarning("", " Erreur non gérée :", sErrDesc);
        }
        else {

            // Succes de la MaJ


            var filterTab = document.getElementById("div113000");

            var oDivs = filterTab.getElementsByTagName("div");

            //Maj la liste     
            var bFound = false;

            for (var i = 0; i < oDivs.length; i++) {

                var div = oDivs[i];
                if (div.id.indexOf("COL_113000_113001_" + nId + "_" + nId + "_") == 0) {
                    bFound = true;
                    SetText(div, sNewName);
                }
            }
            //CSS de validation contour de champ
            if (typeof (_eFormularNameEditor) == "object") {
                _eFormularNameEditor.flagAsEdited(true);
            }


        }
    }

}


/// <summary>
/// Boite de confirmation de suppression de formulaire
/// </summary>
function delFormular(nFormularId) {
    eConfirm(1, top._res_806, top._res_6760, '', 500, 200, function () { onDeleteFormular(nFormularId) });
}
/// <summary>
/// Suppression du formulaire & reload de la page
/// </summary>
function onDeleteFormular(nFormularId) {
    top.setWait(true);
    var url = "mgr/eFormularManager.ashx";
    var ednu = new eUpdater(url, 0);
    ednu.addParam("operation", "delete", "post");
    ednu.addParam("id", nFormularId, "post");
    ednu.ErrorCallBack = function () {
        top.setWait(false);
    };
    ednu.send(onDeleteFormularTrait);
}

/// <summary>
/// Retour de suppression
///   -> maj la liste ou affiche un msg d'erreur/warningx
/// </summary>
/// <param name="oDoc">Flux XML de retour</param>
function onDeleteFormularTrait(oDoc) {
    top.setWait(false);
    if (oDoc && oDoc.nodeType == 9) {

        var bSuccess = (getXmlTextNode(oDoc.getElementsByTagName("success")[0]) == "1");
        var nId = getXmlTextNode(oDoc.getElementsByTagName("formularid")[0]);


        if (!bSuccess) {

            var nErrCode = getXmlTextNode(oDoc.getElementsByTagName("ErrorCode")[0]);
            var sErrDesc = getXmlTextNode(oDoc.getElementsByTagName("ErrorDescription")[0]);
            showWarning("", " Erreur non gérée :", sErrDesc);

        }
        else {

            // Succes de la MaJ
            // reload la liste
            if (oIframe) {

                var oTable = oIframe.contentDocument.getElementById("mt_113000");
                var nNbRow = oTable.rows.length;

                for (var nCmpt = 0; nCmpt < nNbRow; nCmpt++) {
                    var oTR = oTable.rows[nCmpt];
                    if (oTR.getAttribute("eid") == "113000_" + nId) {
                        oTable.deleteRow(nCmpt);
                        nCmpt--;
                        nNbRow--;
                    }
                }

                var oLstTR = oTable.querySelectorAll("tbody tr[eid]");

                for (var nCmpt = 0; nCmpt < oLstTR.length; nCmpt++) {

                    removeClass(oLstTR[nCmpt], "list_odd");
                    removeClass(oLstTR[nCmpt], "list_even");

                    var sClassName = nCmpt % 2 ? "list_odd" : "list_even";

                    addClass(oLstTR[nCmpt], sClassName);
                }


            }
        }
    }
}

/// <summary>
/// Boite de confirmation de duplication de formulaire
/// </summary>
function duplicateFormular(nFormularId, nParentFileId) {

    if (typeof nParentFileId != "number" )
        nParentFileId = 0

    if (nParentFileId == 0)
        nParentFileId = Number(getAttributeValue(document.getElementById('mainDiv'), "pfid"));

    eConfirm(1, top._res_1799, top._res_2792, '', 500, 200, function () { onDuplicateFormular(nFormularId, nParentFileId) });
}
/// <summary>
/// Suppression du formulaire & reload de la page
/// </summary>
function onDuplicateFormular(nFormularId, nParentFileId) {
    top.setWait(true);
    var url = "mgr/eFormularManager.ashx";
    var ednu = new eUpdater(url, 0);
    ednu.addParam("operation", "clone", "post");
    ednu.addParam("id", nFormularId, "post");
    ednu.addParam("parentfileid", nParentFileId, "post");
    ednu.ErrorCallBack = function () {
        top.setWait(false);
    };
    ednu.send(onDuplicateFormularTrait);
}

/// <summary>
/// Retour de la duplication
///   -> maj la liste ou affiche un msg d'erreur/warningx
/// </summary>
/// <param name="oDoc">Flux XML de retour</param>
function onDuplicateFormularTrait(oDoc) {
    top.setWait(false);
    if (oDoc && oDoc.nodeType == 9) {

        var bSuccess = (getXmlTextNode(oDoc.getElementsByTagName("success")[0]) == "1");
        var nId = getXmlTextNode(oDoc.getElementsByTagName("formularid")[0]);

        if (!bSuccess) {

            var nErrCode = getXmlTextNode(oDoc.getElementsByTagName("ErrorCode")[0]);
            var sErrDesc = getXmlTextNode(oDoc.getElementsByTagName("ErrorDescription")[0]);
            showWarning("", " Erreur non gérée :", sErrDesc);

        }
        else {

            // Succes de la MaJ
            // reload la liste
            var nId = getXmlTextNode(oDoc.getElementsByTagName("formularid")[0]);
            var nTab = getXmlTextNode(oDoc.getElementsByTagName("formularTab")[0]);
            var nParentFileId = getXmlTextNode(oDoc.getElementsByTagName("formularParentFileId")[0]);
            var vIsAdv = getXmlTextNode(oDoc.getElementsByTagName("formularType")[0]);
            editForm(nId, nTab, nParentFileId, vIsAdv);
        }
    }
}



///Function d'initialisation de la liste
/// doit être appelé chaque fois que la liste est rechargée (en ajax)
function initFilterList() {

    if (_activeFilter != 0) {
        var filterTab = document.getElementById("mt_104000");
        oTrs = filterTab.getElementsByTagName("tr");
        for (var j = 0; j < oTrs.length; j++) {
            if (oTrs[j].getAttribute("eid") == "104000_" + _activeFilter) {
                selectLine(oTrs[j]);
                break;
            }
        }
    }




    //kha : necessaire pour les filtres colonnes.
    if (!ePopupObject)
        ePopupObject = new ePopup('ePopupObject', 220, 250, 0, 0, document.body, false);

    _ePopupVNEditor = new ePopup('_ePopupVNEditor', 220, 250, 0, 0, document.body, false);
    _eFilterNameEditor = new eFieldEditor('inlineEditor', _ePopupVNEditor, "_eFilterNameEditor");
    _eFilterNameEditor.action = 'renameFilter';

    //Bind l'appel à la Fenêtre info debug
    try {
        if (document.getElementById('mt_104000') && typeof (fldInfoOnContext) == "function")
            document.getElementById('mt_104000').oncontextmenu = fldInfoOnContext;
    }
    catch (e) {

    }
    // HLA - Bug #24495- Cela fait des appels à eSelectionManager.ashx pour chaque colonnes -> 
    //      update sur PREF -> super performance -> pas de conservation des resize sur les colonnes -> perte pref des users !
    // TODO - le lancer uniquement pour la liste des filtre sur l'assistant ++, peut-être ?
    /*
    var oDivMain = document.getElementById("mainDiv");
    if (!oDivMain)
    oDivMain = document.getElementById("content");
    autoResizeColumns("104000", oDivMain);
    */
}
//ENDREGION #FILTER#

//REGION #FORMULAR#
function EditSelectedForm() {

    if (_eCurentSelectedFilter != null && _eCurentSelectedFilter.getAttribute("eid")) {
        //#2 752: on annule la modif si on n'a pas les droits de modifs
        if (_eCurentSelectedFilter && (!getAttributeValue(_eCurentSelectedFilter, "showbtnmodify")) || getAttributeValue(_eCurentSelectedFilter, "showbtnmodify") == "0") {
            return;
        }
        var oId = _eCurentSelectedFilter.getAttribute("eid").split('_');
        var nTab = _eCurentSelectedFilter.getAttribute("eft");
        var nParentFileId = Number(getAttributeValue(document.getElementById('mainDiv'), "pfid"));
        var nId = oId[oId.length - 1];
        var advf = _eCurentSelectedFilter.getAttribute("advf");
        editForm(nId, nTab, nParentFileId, advf);
    }
    else {
        showWarning(top._res_719, top._res_87, "");
    }
}
function InjectMailingSelectedForm(oMemo) {
    if (_eCurentSelectedFilter != null && _eCurentSelectedFilter.getAttribute("eid")) {
        var oId = _eCurentSelectedFilter.getAttribute("eid").split('_');
        var nTab = _eCurentSelectedFilter.getAttribute("eft");
        var nId = oId[oId.length - 1];
        var sFormularName = top._res_1142;
        if (_eCurentSelectedFilter.children.length > 1)
            sFormularName = eTrim(GetText(_eCurentSelectedFilter.children[1]));
        oMemo.insertFormularField(nId, sFormularName);
        return true;
    }
    else {
        showWarning(top._res_719, top._res_87, "");
        return false;
    }
}

//ENDREGION #FORMULAR#

//REGION #REPORT#
function initReportList() {

    adjustLastCol(105000);

    //kha : necessaire pour les filtres colonnes.
    if (!ePopupObject)
        ePopupObject = new ePopup('ePopupObject', 220, 250, 0, 0, document.body, false);

    _ePopupVNEditor = new ePopup('_ePopupVNEditor', 220, 250, 0, 0, document.body, false);
    _eReportNameEditor = new eFieldEditor('inlineEditor', _ePopupVNEditor, "_eReportNameEditor");
    _eReportNameEditor.action = 'renameReport';

    if (oIframe && oIframe.contentDocument.getElementById("mt_113000")) {
        var elem = oIframe.contentDocument.getElementById("mt_113000").querySelector('[updatable]');

        //#77 224: KJE, Si on n'a pas les droits de modifs, on désative le bouton de modification
        var modBtn = top.document.getElementById("editReport");
        if (modBtn == null)
            modBtn = top.document.getElementById("fltLstModBtn");
        if (modBtn != null && typeof (modBtn) != "undefined") {
            if (elem && getAttributeValue(elem, "showbtnmodify") == "1") {
                setAttributeValue(modBtn, "showbtnmodify", "1");
                modBtn.style.display = "block";
            }
            else {
                setAttributeValue(modBtn, "showbtnmodify", "0");
                modBtn.style.display = "none";
            }
        }
    }
}



///eModalDialog de l'assistant d'export
//var modalWizard;

///summary
///Ouvre l'assistant reporting en mode Edition
///<param name="nReportId">id du rapport</param>
///<param name="reportType">Type de rapport</param>
///summary
function editReport(nReportId, reportType) {
    // Ouverture du rapport sélectionné si l'ID n'est pas renseigné
    if (!nReportId && _eCurentSelectedFilter) {
        if (getAttributeValue(_eCurentSelectedFilter, "updatable") == "0") {
            eAlert(3, "", top._res_6762);
            return;
        }
        var eid = _eCurentSelectedFilter.attributes["eid"].value.split("_");

        nReportId = eid[1];

        var lstreportype = document.getElementById("lstreportype");
        reportType = lstreportype.options[lstreportype.options.selectedIndex].value;   //Type de Rapport
    }
    // Si l'ID n'est toujours pas renseigné (ex : clic sur Modifier sans avoir effectué de sélection), on ignore
    if (isNaN(Number(nReportId)))
        return;

    //#77 224: KJE, on ne charge pas la fenêtre Edition des rapport si on n'a pas les droits de modifs
    if (_eCurentSelectedFilter && (!getAttributeValue(_eCurentSelectedFilter, "showbtnmodify")) || getAttributeValue(_eCurentSelectedFilter, "showbtnmodify") == "0") {
        return;
    }

    modalWizard = new eModalDialog(top._res_6390, 0, "eWizard.aspx", 950, 600, "ReportWizard"); // Assistant Reporting
    modalWizard.EudoType = ModalEudoType.WIZARD.toString(); // Type Wizard
    modalWizard.addParam("wizardtype", "report", "post");
    modalWizard.addParam("operation", 3, "post");
    modalWizard.addParam("reportid", nReportId, "post");
    modalWizard.addParam("rtype", reportType, "post");
    modalWizard.addParam("frmId", oIframe.id, "post");
    modalWizard.addParam("modalId", modalWizard.iframeId, "post");

    if (reportType == 6)
        modalWizard.hideMaximizeButton = true;
    modalWizard.ErrorCallBack = launchInContext(modalWizard, modalWizard.hide);
    modalWizard.show();
    if (reportType == 6) {
        modalWizard.MaxOrMinModal();
        top.setWait(true);

        modalWizard.onIframeLoadComplete = function () {
            top.setWait(false);
        };
    }

    modalWizard.addButton(top._res_29, null, "button-gray", null, "cancel_btn", true);  //Annuler
    modalWizard.addButton(top._res_28, "modalWizard.getIframe().SaveReport();", "button-green", null, "save_btn"); // Valider
    modalWizard.addButton(top._res_869, "modalWizard.getIframe().SaveReportAndExit(); ", "button-green", null, "savenexit_btn"); // Appliquer et Fermer
    modalWizard.addButton(top._res_26, "modalWizard.getIframe().MoveStep(true, 'report');", "button-green-rightarrow", null, "next_btn"); // Suivant
    modalWizard.addButton(top._res_25, "modalWizard.getIframe().MoveStep(false, 'report');", "button-gray-leftarrow", null, "previous_btn"); // Précédent

    HideModalButtons();
}

///summary
///Ouvre l'assistant reporting en mode création
///<param name="tab">Onglet en cours</param>
///<param name="reportType">Type de rapport</param>
///summary
function AddReport(tab, reportType) {

    modalWizard = new eModalDialog(top._res_6390, 0, "eWizard.aspx", 950, 600, "ReportWizard"); // Assistant Reporting
    modalWizard.EudoType = ModalEudoType.WIZARD.toString(); // Type Wizard
    modalWizard.addParam("wizardtype", "report", "post");
    modalWizard.addParam("tab", tab, "post");
    modalWizard.addParam("rtype", reportType, "post");
    modalWizard.addParam("operation", 1, "post");
    modalWizard.addParam("frmId", oIframe.id, "post");
    modalWizard.addParam("modalId", modalWizard.iframeId, "post");
    if (reportType == 6)
        modalWizard.hideMaximizeButton = true;
    modalWizard.ErrorCallBack = launchInContext(modalWizard, modalWizard.hide);
    modalWizard.show();
    if (reportType == 6) {
        modalWizard.MaxOrMinModal();
        top.setWait(true);
        modalWizard.onIframeLoadComplete = function () {
            top.setWait(false);
        };
    }

    modalWizard.addButton(top._res_29, null, "button-gray", null, "cancel_btn", true);  //Annuler
    modalWizard.addButton(top._res_286, "top.window['_md']['ReportWizard'].getIframe().SaveReport();", "button-green", null, "save_btn"); // Enregistrer
    modalWizard.addButton(top._res_1910, "top.window['_md']['ReportWizard'].getIframe().SaveReportAndExit();", "button-green", null, "savenexit_btn"); // Enregistrer et Fermer
    modalWizard.addButton(top._res_26, "modalWizard.getIframe().MoveStep(true, 'report');", "button-green-rightarrow", null, "next_btn"); // Suivant
    modalWizard.addButton(top._res_25, "modalWizard.getIframe().MoveStep(false, 'report');", "button-gray-leftarrow", null, "previous_btn"); // Précédent

    HideModalButtons();
}

///summary
///Cache les boutons sur la modal afin qu'on ne les vois pas au premier chargement et qu'on affiche uniquement le nécessaire par la suite
///summary
function HideModalButtons() {
    var buttonModal = window.parent.document.getElementById("ButtonModal" + modalWizard.iframeId.replace("frm", ""));
    ///On parcours les div du conteneur des boutons, et pour chaque div ayant un ID de type ******_btn , donc un bouton.
    ///Et on les masque.
    var buttons = buttonModal.getElementsByTagName("div");
    for (iBtn = 0; iBtn < buttons.length; iBtn++) {
        if (buttons[iBtn].id.indexOf("_btn") > 0 && buttons[iBtn].id.indexOf("_btn") + 4 == buttons[iBtn].id.length)
            buttons[iBtn].style.display = "none";

    }
}

function SaveReportAndExit() {
    return SaveReport(true);
}

///summary
///<param name="close">True s'il faut fermer la fenêtre après ajout/modification, False sinon</param>
///summary
function SaveReport(bClose) {

    // #64 326 - Avec l'apparition d'un bouton Enregistrer sans fermeture de la fenêtre, il faut que la fenêtre passe en mode Mise à jour une fois
    // que le rapport a été sauvegardé la première fois, et que l'ID a été renseigné. Sinon, cliquer plusieurs fois sur Enregistrer (qui est câblé comme
    // un bouton d'ajout si on ouvre la fenêtre en mode Création/Nouveau rapport) provoquerait l'ajout multiple (duplication) du même rapport dans la base
    // On considère donc désormais que la fenêtre est en mode Nouveau rapport si ID = 0, et en mode Mise à jour sinon
    var add = oCurrentWizard.GetId() < 1;

    // Si la notion de fermeture de la fenêtre après traitement n'est pas définie par paramètre :
    // - cette fonction fermera la fenêtre si le bouton "Appliquer et fermer" est masqué
    if (typeof (bClose) == "undefined" || bClose == null) {
        try {
            if (typeof modalWizard == "undefined")
                modalWizard = top.window['_md']['ReportWizard'];
            var buttonModalDiv = modalWizard.getIframe().parent.document.getElementById("ButtonModal" + modalWizard.iframeId.replace("frm", ""));
            var btnSaveNExit = buttonModalDiv.ownerDocument.getElementById("savenexit_btn");
            bClose = btnSaveNExit.style.display == "none";
        }
        catch (ex) {
            bClose = true;
        }
    }

    if (oCurrentWizard.GetType() == 6) {
        setExpressFilterValuesParams();
        setEtiquettesTriValuesParams();
    }


    var isReportValid = ManageReportValidation();
    if (isReportValid) {
        UpdatePermission();
        var url = "mgr/eReportManager.ashx";
        var ednu = new eUpdater(url, 0);
        ednu.addParam("operation", add == true ? 1 : 3, "post");
        ednu.addParam("reportid", oCurrentWizard.GetId(), "post");
        ednu.addParam("reporttype", oCurrentWizard.GetType(), "post");
        ednu.addParam("reportname", oCurrentWizard.GetName(), "post");
        ednu.addParam("userid", oCurrentWizard.GetUserId(), "post");
        ednu.addParam("tab", oCurrentWizard.GetTab(), "post");
        ednu.addParam("viewpermchecked", oCurrentWizard.IsViewPermActive(), "post");
        ednu.addParam("viewpermid", oCurrentWizard.GetViewPermId(), "post");
        var tabViewPerm = oCurrentWizard.GetViewPerm();
        ednu.addParam("viewpermmode", tabViewPerm["mode"], "post");
        ednu.addParam("viewpermlevel", tabViewPerm["level"], "post");
        ednu.addParam("viewpermuser", tabViewPerm["user"], "post");
        ednu.addParam("updatepermchecked", oCurrentWizard.IsUpdatePermActive(), "post");
        ednu.addParam("updatepermid", oCurrentWizard.GetUpdatePermId(), "post");
        var tabUpdPerm = oCurrentWizard.GetUpdatePerm();
        ednu.addParam("updatepermmode", tabUpdPerm["mode"], "post");
        ednu.addParam("updatepermlevel", tabUpdPerm["level"], "post");
        ednu.addParam("updatepermuser", tabUpdPerm["user"], "post");
        ednu.addParam("viewruleid", oCurrentWizard.GetViewRule(), "post");
        ednu.addParam("endproc", oCurrentWizard.GetEndProcedure(), "post");
        ednu.addParam("startproc", "", "post");
        ednu.addParam("close", bClose ? "1" : "0", "post");
        ednu.addParam("params", oCurrentWizard.GetSQLParamString(), "post");

        //schedule - si le schedule a changer, on doit le metre à jour.
        var oOrigSchedule = oCurrentWizard.GetScheduleOrig();
        var oCurrSchedule = oCurrentWizard.GetSchedule();

        var bIsScheduled = oCurrentWizard.GetIsScheduled();
        var bMajSchedule = false;


        //Suppression du schedule
        if (!bIsScheduled && oOrigSchedule != null)
            bMajSchedule = true; //

        if (bIsScheduled) {

            if (oOrigSchedule == null)
                bMajSchedule = true; // création d'un schedule
            else {

                //comparaison de oOrigSchedule & oCurrSchedule pour vérifier si le schedule a été modifié
                // la comparaison ne se fait plus sur les criètres de recipients/reportparam/ftp, ceux ci n'étant finalement plus utlisés
                // ils sont cependant conservé si on revient sur cette décision fonctionnelle
                if (
                    oOrigSchedule.FrequencyType == oCurrSchedule.FrequencyType
                    && oOrigSchedule.Frequency == oCurrSchedule.Frequency
                    && oOrigSchedule.Day == oCurrSchedule.Day
                    && oOrigSchedule.Order == oCurrSchedule.Order
                    && eTools.ArrayEqual(oOrigSchedule.WeekDays, oCurrSchedule.WeekDays)
                    && oOrigSchedule.Month == oCurrSchedule.Month
                    && oOrigSchedule.StartDate == oCurrSchedule.StartDate
                    && oOrigSchedule.EndDate == oCurrSchedule.EndDate
                    && oOrigSchedule.Repeat == oCurrSchedule.Repeat
                    && oOrigSchedule.Hour == oCurrSchedule.Hour
                )
                    bMajSchedule = false;
                else {

                    bMajSchedule = true;
                }

            }
        }

        ednu.addParam("majschedule", bMajSchedule ? "1" : "0", "post");

        if (bMajSchedule) {
            if (bIsScheduled) {
                ednu.addParam("scheduledata", JSON.stringify(oCurrSchedule), "post");
            }
        }

        if (typeof modalWizard == "undefined")
            modalWizard = top.window['_md']['ReportWizard'];

        ednu.ErrorCallBack = function (bClose) {


            top.setWait(false);
            if (bClose)
                modalWizard.hide();

        };

        top.setWait(true);
        ednu.send(

            function (oRes, bClose) {
                top.setWait(false);
                AfterReportTrait(oRes, bClose);

            }
        );
    }
}
function UpdatePermission() {
    if (typeof modalWizard == "undefined")
        modalWizard = top.window['_md']['ReportWizard'];
    var childwindow = modalWizard.getIframe();

    var objRetValue = getPermReturnValue("View", childwindow);
    oCurrentWizard.SetViewPermParam("level", objRetValue.levels);
    oCurrentWizard.SetViewPermParam("user", objRetValue.users);
    oCurrentWizard.SetViewPermParam("mode", objRetValue.perMode);
    var objRetValue = getPermReturnValue("Update", childwindow);
    oCurrentWizard.SetUpdatePermParam("level", objRetValue.levels);
    oCurrentWizard.SetUpdatePermParam("user", objRetValue.users);
    oCurrentWizard.SetUpdatePermParam("mode", objRetValue.perMode);
}

///summary
///Gère l'affichage de l'interface après l'insertion ou la mise à jour d'un rapport depuis l'assistant.
///<param name="oDoc" > Document xml retourné par l'appel AJAX de eUpdater</param>
///<param name="close"> Indique s'il faut fermer la fenêtre ou non</param>
///summary
function AfterReportTrait(oDoc) {
    if (oDoc && oDoc.nodeType == 9) {
        var bSuccess = (getXmlTextNode(oDoc.getElementsByTagName("success")[0]) == "1");
        var nId = getXmlTextNode(oDoc.getElementsByTagName("reportid")[0]);
        var sName = getXmlTextNode(oDoc.getElementsByTagName("reportname")[0]);
        var bClose = (getXmlTextNode(oDoc.getElementsByTagName("close")[0]) == "1");

        if (!bSuccess) {
            var nErrCode = getXmlTextNode(oDoc.getElementsByTagName("ErrorCode")[0]);
            var sErrDesc = getXmlTextNode(oDoc.getElementsByTagName("ErrorDescription")[0]);
            showWarning("", " Erreur non gérée :", sErrDesc);
        }
        else {

            // Succes

            // #64 326 - Si l'URL Power BI est retournée, et si le contrôle existe sur l'assistant, on la renseigne
            var powerBIURL = oDoc.getElementsByTagName("powerbiurl");
            if (powerBIURL.length > 0) {
                var powerBIURLInput = document.getElementById("editor_powerbi_url_input");
                if (powerBIURLInput)
                    powerBIURLInput.value = getXmlTextNode(powerBIURL[0]);
            }

            // reload la Liste
            var reportListModal = top.window['_md']['ReportList'];
            reportListModal.getIframe().loadList();

            //Fermeture de la fenêtre d'assistant
            if (bClose) {
                if (!modalWizard)
                    modalWizard = top.window['_md']['ReportWizard'];
                modalWizard.hide();
            }
            // #64 326 - Sinon, on renseigne l'ID et le nom du rapport sauvegardé en base, pour que les prochains clics sur Enregistrer fassent une MAJ du rapport et non
            // un ajout (ce qui provoquerait une duplication en masse si on clique sur le bouton Enregistrer plusieurs fois)
            // (les 2 critères sont testés par eReportManager pour savoir s'il faut faire une MAJ ou un Enregistrer sous : si Id = 0 et UPDATE = erreur, et si
            // nom différent de celui retrouvé en base ou vide = Enregistrer sous = nouveau rapport)
            else if (oCurrentWizard) {
                oCurrentWizard.SetId(nId);
                oCurrentWizard.SetName(sName);
                // Et on change également les libellés des boutons "Enregistrer" et "Enregistrer et Fermer", qui deviennent "Valider" et "Appliquer et Fermer"
                try {
                    if (typeof modalWizard == "undefined")
                        modalWizard = top.window['_md']['ReportWizard'];
                    var buttonModalDiv = modalWizard.getIframe().parent.document.getElementById("ButtonModal" + modalWizard.iframeId.replace("frm", ""));
                    var btnSaveLabel = buttonModalDiv.ownerDocument.getElementById("save_btn-mid");
                    var btnSaveNExitLabel = buttonModalDiv.ownerDocument.getElementById("savenexit_btn-mid");
                    if (btnSaveLabel)
                        btnSaveLabel.innerHTML = top._res_28; // Valider
                    if (btnSaveNExitLabel)
                        btnSaveNExitLabel.innerHTML = top._res_869; // Appliquer et Fermer
                }
                catch (ex) {

                }
            }
        }
    }
}

/// <summary>
/// Duplique un rapport
/// </summary>
/// <param name="baseName">Id de la cellule</param>
/// <param name="elem">Bouton cliqué (déclencheur de l'action de renommage)</param>
function duplicateReport(baseName, elem) {
    alert("Duplicate Report");
}

///summary
///<param name="reportId">Supprime le rapport</param>
///summary
function delReport(reportId) {
    eConfirm(1, top._res_806, top._res_175, '', 500, 200, function () { OnDeleteReport(reportId) });
}

///summary
/// Fonction de callback gérant la suppression d'un rapport
///<param name="reportId">Id du Rapport</param>
///summary
function OnDeleteReport(reportId) {
    var url = "mgr/eReportManager.ashx";
    var ednu = new eUpdater(url, 0);
    ednu.addParam("operation", 5, "post");
    ednu.addParam("reportid", reportId, "post");
    ednu.ErrorCallBack = function () { };
    ednu.send(OnDeleteReportTrait);
}

function OnDeleteReportTrait(oDoc) {

    if (oDoc && oDoc.nodeType == 9) {

        var bSuccess = (getXmlTextNode(oDoc.getElementsByTagName("success")[0]) == "1");
        var nId = getXmlTextNode(oDoc.getElementsByTagName("reportid")[0]);

        if (!bSuccess) {
            var nErrCode = getXmlTextNode(oDoc.getElementsByTagName("ErrorCode")[0]);
            var sErrDesc = getXmlTextNode(oDoc.getElementsByTagName("ErrorDescription")[0]);

            var errorTitle = top._res_6530 + " : ";

            showWarning("", errorTitle, sErrDesc);
        }
        else {
            // Succes
            // reload la liste
            var oTable = oIframe.contentDocument.getElementById("mt_105000");
            var nNbRow = oTable.rows.length;

            for (var nCmpt = 0; nCmpt < nNbRow; nCmpt++) {
                var oTR = oTable.rows[nCmpt];
                if (oTR.getAttribute("eid") == "105000_" + nId) {
                    oTable.deleteRow(nCmpt);
                    // HLA - Inutile de continuer, le rapport a été supprimé de la grille
                    break;
                    //nCmpt--;
                    //nNbRow--;
                }
            }

            _eCurentSelectedFilter = null;

            ///todo : refaire l'alternance de ligne ...                     
        }
    }
}



/// <summary>
/// Initie le renommage d'un rapport via appel à un eFieldEditor
///   fonction de efieldeditor : renameReportr via onClick
/// </summary>
/// <param name="baseName">Id de la cellule</param>
/// <param name="elem">Bouton cliqué (déclencheur de l'action de renommage)</param>
function renReport(baseName, elem) {
    var libElem = document.getElementById(baseName);

    if (libElem)
        _eReportNameEditor.onClick(libElem, elem);
}

function onReportRenameTrait(oDoc) {

    if (oDoc && oDoc.nodeType == 9) {

        var bSuccess = (getXmlTextNode(oDoc.getElementsByTagName("success")[0]) == "1");
        var nId = getXmlTextNode(oDoc.getElementsByTagName("reportid")[0]);
        var sNewName = getXmlTextNode(oDoc.getElementsByTagName("reportname")[0]);

        if (!bSuccess) {

            var nErrCode = getXmlTextNode(oDoc.getElementsByTagName("ErrorCode")[0]);
            var sErrDesc = getXmlTextNode(oDoc.getElementsByTagName("ErrorDescription")[0]);

            if (nErrCode == "1") {
                //Erreur : un rapport avec le même nom existe déjà               
                showWarning(top._res_1713, top._res_719 + " :", sErrDesc);
            }
            else
                showWarning("", " Erreur non gérée :", sErrDesc);
        }
        else {

            // Succes de la MaJ
            var ReportTab = document.getElementById("mt_105000");
            oDivs = ReportTab.getElementsByTagName("div");

            //Maj la liste     
            for (var i = 0; i < oDivs.length; i++) {

                var div = oDivs[i];
                if (div.id.indexOf("COL_105000_105001_" + nId + "_" + nId + "_") == 0) {

                    try {
                        div.innerHTML = sNewName;
                    }
                    catch (e) {
                        if (div.innerText != null)
                            div.innerText = sNewName;
                        else {
                            if (div.textContent != null)
                                div.textContent = sNewName;
                            else
                                div.innerHTML = sNewName;
                        }
                    }

                    break;
                }
            }

            //CSS de validation contour de champ
            if (typeof (_eReportNameEditor) == "object") {
                _eReportNameEditor.flagAsEdited(true);
            }
        }
    }
}


/// <summary>
/// Affiche la description du filtre
/// </summary>
/// <param name="elem">élément clicker</param>
var reportEventSave;
function shReportDesc(e, obj) {

    // clearTimeout(hideTT);

    reportEventSave = e;

    //récupère la ligne du filtre
    var elem = obj.parentNode || obj.parentElement;
    while (elem.tagName != 'TR') {

        elem = elem.parentNode || elem.parentElement;

        if (elem.tagName == 'TBODY')
            return;
    }


    // ASY : #31582 - Ajouter Tous dans l'assistant Rapport -Suite : Ajouter le type dans le tooltip lorsqu'on selectionne Tous - Recuperer la selection du tous
    var lstreportype = document.getElementById("lstreportype");
    var selectLstReportype = lstreportype.options[lstreportype.options.selectedIndex].value;   //Type des Rapports : dans la combobox

    // id  et table du filtre
    var oId = elem.getAttribute("eid").split('_');
    var nTab = oId[0];
    var nReportId = oId[oId.length - 1];

    var url = "mgr/eReportManager.ashx";
    var ednu = new eUpdater(url, 0);
    ednu.addParam("operation", 6, "post");
    ednu.addParam("reportid", nReportId, "post");

    // ASY : #31582 - Ajouter Tous dans l'assistant Rapport -Suite : Ajouter le type dans le tooltip lorsqu'on selectionne Tous 
    ednu.addParam("AllWiz", selectLstReportype, "post");

    ednu.ErrorCallBack = ht;
    toTT = setTimeout(function () { ednu.send(onShowReportDescription); }, 100);
}

/// <summary>
/// Gestion de l'affichage de la description du rapport
///   -> Show Tooltip
/// </summary>
/// <param name="oDoc">Flux XML de retour</param>
var hideTT;
var toTT;
function onShowReportDescription(oDoc) {
    if (oDoc && oDoc.nodeType == 9) {

        var bSuccess = (getXmlTextNode(oDoc.getElementsByTagName("success")[0]) == "1");

        if (!bSuccess) {
            var nErrCode = getXmlTextNode(oDoc.getElementsByTagName("ErrorCode")[0]);
            var sErrDesc = getXmlTextNode(oDoc.getElementsByTagName("ErrorDescription")[0]);
            showWarning(top._res_1713, top._res_719 + " : ", "(" + nErrCode + ")  " + sErrDesc);
            // masque le tool tip
            ht();
        }
        else {

            var sDesc = getXmlTextNode(oDoc.getElementsByTagName("reportdescription")[0]);
            var nId = getXmlTextNode(oDoc.getElementsByTagName("reportid")[0]);

            // Pas de description, on affiche rien
            if (sDesc.length == 0) {
                ht();
                return;
            }

            st(reportEventSave, sDesc, "filterToolTip");

            if (window.event)
                var origNode = reportEventSave.srcElement;
            else
                var origNode = reportEventSave.target;
            //if (origNode != null)
            //    origNode.onmouseout = function () {
            //        hideTT = setTimeout(ht, 250);
            //    };
        }
    }
    else {
        //retour invalide, on masque le tooltip
        ht();
    }
}

function shFilterDesc(e, obj) {

    //récupère la ligne du filtre
    var elem = obj.parentNode || obj.parentElement;
    while (elem.tagName != 'TR') {

        elem = elem.parentNode || elem.parentElement;

        if (elem.tagName == 'TBODY')
            return;
    }

    // id  et table du filtre
    var oId = elem.getAttribute("eid").split('_');
    var nTab = oId[0];
    var nFilterId = oId[oId.length - 1];

    shFilterDescriptionById(e, nFilterId);
}

function ReloadReportList(oList) {
    var reportType = oList.options[oList.selectedIndex].value;
    var nTabBkm = document.getElementById("mainDiv").attributes["tabBKM"].value;
    top.reportList(reportType, nTabBkm);
}

//REGION #Modèle import#


function initImportTpl() {

    if (_activeImportTemplate != 0) {
        var importTemplate = document.querySelector("tr[mtid='" + _activeImportTemplate + "']");
        if (importTemplate)
            addClass(importTemplate, 'eSel');
    }

    adjustLastCol(TAB_IMPORTTEMPLATE);
    //kha : necessaire pour les filtres colonnes.
    if (!ePopupObject)
        ePopupObject = new ePopup('ePopupObject', 220, 250, 0, 0, document.body, false);

    _ePopupVNEditor = new ePopup('_ePopupVNEditor', 220, 250, 0, 0, document.body, false);
    _eFilterNameEditor = new eFieldEditor('inlineEditor', _ePopupVNEditor, "_eFilterNameEditor");
    _eFilterNameEditor.action = 'renameImportTemplate';

}




//ENDREGION #REPORT#


