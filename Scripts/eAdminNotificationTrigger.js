function loadListTab() {
    var oUpd = new eUpdater("Mgr/eAdminNotifTriggerManager.ashx");
    oUpd.addParam("action", "listtabs", "post");

    oUpd.send(loadListTabResult);
}

function loadListTabResult(oRes) {
    if (!oRes) {
        return;
    }

    var bSuccess = getXmlTextNode(oRes.getElementsByTagName("success")[0]).toLowerCase() == "true" ? true : false;

    if (!bSuccess) {
        var sError = getXmlTextNode(oRes.getElementsByTagName("error")[0]);
        alert(sError);
    }
    else {
        var selectTab = document.getElementById("tabid");

        if (selectTab != null) {
            addSelectOption(selectTab, "", "");

            var listTabs = oRes.getElementsByTagName("listtabs")[0].getElementsByTagName("tab");
            if (listTabs.length > 0) {
                for (var i = 0; i < listTabs.length; ++i) {
                    var obj = listTabs[i];

                    var nDescId = parseInt(getXmlTextNode(obj.getElementsByTagName("descid")[0]));
                    var sLibelle = getXmlTextNode(obj.getElementsByTagName("libelle")[0]);

                    addSelectOption(selectTab, nDescId.toString(), sLibelle + " (" + nDescId.toString() + ")");

                    loadListFieldsForTab(nDescId.toString());
                }
            }
        }
    }
}

function loadListFieldsForTab(nTabDescId, sTargetSelectId, sTargetSelectValue, bImage) {
    var oUpd = new eUpdater("Mgr/eAdminNotifTriggerManager.ashx");
    oUpd.addParam("action", "listfields", "post");
    oUpd.addParam("tabId", nTabDescId, "post");
    oUpd.addParam("targetSelectId", sTargetSelectId, "post");
    oUpd.addParam("targetSelectValue", sTargetSelectValue, "post");
    oUpd.addParam("image", bImage ? "1" : "0", "post");

    oUpd.send(loadListFieldsForTabResult);
}

function loadListFieldsForTabResult(oRes) {
    if (!oRes) {
        return;
    }

    var bSuccess = getXmlTextNode(oRes.getElementsByTagName("success")[0]).toLowerCase() == "true" ? true : false;

    if (!bSuccess) {
        var sError = getXmlTextNode(oRes.getElementsByTagName("error")[0]);
        alert(sError);
    }
    else {
        var select = document.getElementById("tabfields_" + getXmlTextNode(oRes.getElementsByTagName("tabId")[0]));
        if (select == null) {
            select = document.createElement("select");
            select.id = "tabfields_" + getXmlTextNode(oRes.getElementsByTagName("tabId")[0]);
            select.style.display = "none";
            document.getElementById("divLoader").appendChild(select);
        }

        if (select != null) {
            addSelectOption(select, "", "");

            var listFields = oRes.getElementsByTagName("listFields")[0].getElementsByTagName("field");
            if (listFields.length > 0) {
                for (var i = 0; i < listFields.length; ++i) {
                    var obj = listFields[i];

                    var nDescId = parseInt(getXmlTextNode(obj.getElementsByTagName("descid")[0]));
                    var sLibelle = getXmlTextNode(obj.getElementsByTagName("label")[0]);

                    addSelectOption(select, nDescId.toString(), sLibelle + " (" + nDescId.toString() + ")");
                    var initialTargetSelect = document.getElementById(getXmlTextNode(oRes.getElementsByTagName("targetSelectId")[0]));
                    if (initialTargetSelect != null) {
                        addSelectOption(initialTargetSelect, nDescId.toString(), sLibelle + " (" + nDescId.toString() + ")");
                        if (getXmlTextNode(oRes.getElementsByTagName("targetSelectValue")[0]) == nDescId.toString())
                            initialTargetSelect.options[initialTargetSelect.options.length - 1].selected = "selected";
                    }
                }
            }
        }
    }
}

function addSelectOption(elDdl, value, libelle) {
    var option = document.createElement("option");
    option.setAttribute("value", value);
    option.innerHTML = libelle;
    elDdl.appendChild(option);
}

function clearSelectOptions(elDdl) {
    if (elDdl != null)
        while (elDdl.options.length > 0)
            elDdl.options.remove(0);
}
function clearDiv(divId) {
    var div = document.getElementById(divId);
    if (div != null) {
        while (div.firstChild) {
            div.removeChild(div.firstChild);
        }
    }
}

function appendTriggerToTable(obj, table, index, even) {
    if (table != null) {
        tblRow = document.createElement("tr");
        tblRow.id = "row_trigger_" + index;
        if (even)
            tblRow.setAttribute("class", "even");
        else
            tblRow.setAttribute("class", "odd");

        table.appendChild(tblRow);

        //Supprimer
        tblCol = document.createElement("td");
        btnDelete = document.createElement("input");
        btnDelete.setAttribute("type", "button");
        btnDelete.setAttribute("value", "Supprimer");
        btnDelete.setAttribute("onclick", "deleteTrigger(" + index + ");");
        tblCol.appendChild(btnDelete);
        tblRow.appendChild(tblCol);

        // ID
        tblCol = document.createElement("td");
        tblCol.innerHTML = obj.id.toString();
        hddnId = document.createElement("input");
        hddnId.setAttribute("type", "hidden");
        hddnId.setAttribute("id", "hddnId_" + index);
        hddnId.setAttribute("value", obj.id);
        tblCol.appendChild(hddnId);
        tblRow.appendChild(tblCol);

        // Autres champs à afficher en liste
        tblRow.appendChild(getTriggerFieldForListTable("Label", index, obj.label, false)); // Libellé
        tblRow.appendChild(getTriggerFieldForListTable("TriggerTabField", index, obj.triggerTabField, false)); // Onglet et rubrique déclencheur
        tblRow.appendChild(getTriggerFieldForListTable("TriggerEvt", index, obj.triggerEvt, false)); // Evènement déclencheur
        tblRow.appendChild(getTriggerFieldForListTable("TriggerCondition", index, obj.triggerCondition, false)); // Evènement déclencheur
        tblRow.appendChild(getTriggerFieldForListTable("NotificationType", index, obj.notificationType, false)); // Type de notification
        tblRow.appendChild(getTriggerFieldForListTable("BroadcastType", index, obj.broadcastType, false)); // Mode de diffusion
        tblRow.appendChild(getTriggerFieldForListTable("Subscribers", index, obj.subscribers, false)); // Destinataires fixes
        tblRow.appendChild(getTriggerFieldForListTable("SubscribersUserField", index, obj.subscribersUserField, false)); // Rubrique de fiche contenant les destinataires de la notification

        // Editer
        tblCol = document.createElement("td");
        btnUpdate = document.createElement("input");
        btnUpdate.setAttribute("type", "button");
        btnUpdate.setAttribute("value", "Editer");
        btnUpdate.setAttribute("onclick", "loadFile(" + obj.id + ");");
        tblRow.setAttribute("onclick", "loadFile(" + obj.id + ");");
        tblCol.appendChild(btnUpdate);
        tblRow.appendChild(tblCol);
    }
}

function getTriggerFieldForListTable(fieldName, index, value, editable) {
    var tblCol = document.createElement("td");
    if (!editable) {
        if (typeof (value) != "undefined" && value != null && value != "")
            tblCol.innerHTML = value;
    }
    else {
        var txt = document.createElement("input");
        txt.setAttribute("type", "text");
        txt.setAttribute("id", "txt" + fieldName + "_" + index);
        if (typeof (value) != "undefined" && value != null && value != "")
            txt.setAttribute("value", value);
        tblCol.appendChild(txt);
    }
    return tblCol;
}

function appendTriggerFieldToFileTable(obj, table, index, even) {
    if (table != null) {
        tblRow = document.createElement("tr");
        tblRow.id = "file_row_trigger_" + index + "_" + obj.id;
        if (even)
            tblRow.setAttribute("class", "even");
        else
            tblRow.setAttribute("class", "odd");

        table.appendChild(tblRow);

        // Champ
        tblCol = document.createElement("td");
        tblCol.innerHTML = obj.label;
        tblRow.appendChild(tblCol);

        // Valeur
        tblCol = document.createElement("td");
        if (!obj.editable)
            tblCol.innerHTML = obj.value;
        else {
            // TODO: format
            // Catalogues : TYP_USER ou TYP_NUMERIC
            if (obj.isCatalog) {
                selectValue = document.createElement("select");
                selectValue.id = "selFieldValue_" + obj.id;
                if (obj.multiple)
                    selectValue.setAttribute("multiple", "multiple");
                switch (obj.format) {
                    case "8": // TYP_USER
                        // TODO: options
                        var selectValueOption = new Option(obj.value, obj.dbValue, false, true);
                        selectValue.appendChild(selectValueOption);
                        break;
                    case "10": // TYP_NUMERIC
                        switch (obj.catalogType) {
                            case "TAB":
                                var rootTabList = document.getElementById("tabid");
                                if (rootTabList != null) {
                                    for (var i = 0; i < rootTabList.options.length; i++) {
                                        selectValue.appendChild(rootTabList.options[i].cloneNode(true));
                                        if (rootTabList.options[i].value == obj.value) {
                                            selectValue.options[selectValue.options.length - 1].selected = "selected";
                                            var selectedTabId = document.getElementById("selectedTabId");
                                            selectedTabId.value = obj.value; // mémorisation de la valeur sélectionnée dans le select parent dans un élément caché de la page
                                        }
                                    }
                                    selectValue.onchange = function () {
                                        var selectedTabId = document.getElementById("selectedTabId");
                                        selectedTabId.value = this.value; // mémorisation de la valeur sélectionnée dans le select parent dans un élément caché de la page
                                        var childSelectElementsIds = [114207, 114211, 114224]; // TODO : remplacer DescIDs en dur
                                        for (var i = 0; i < childSelectElementsIds.length; i++) {
                                            var childSelectId = "selFieldValue_" + childSelectElementsIds[i];
                                            var childSelectValue = "";
                                            if (document.getElementById(childSelectId) != null) {
                                                childSelectValue = document.getElementById(childSelectId).value;
                                                clearSelectOptions(document.getElementById(childSelectId));
                                            }
                                            loadListFieldsForTab(selectedTabId.value, childSelectId, childSelectValue);
                                        }
                                    }
                                }
                                break;
                            case "FIELD":
                                var selectedTabId = document.getElementById("selectedTabId").value;
                                var tabFieldsList = document.getElementById("tabfields_" + selectedTabId);
                                if (tabFieldsList != null) {
                                    for (var i = 0; i < tabFieldsList.options.length; i++) {
                                        selectValue.appendChild(tabFieldsList.options[i].cloneNode(true));
                                        if (tabFieldsList.options[i].value == obj.value)
                                            selectValue.options[selectValue.options.length - 1].selected = "selected";
                                    }
                                }
                                else {
                                    loadListFieldsForTab(selectedTabId.value, selectValue.id, obj.value, obj.image);
                                }
                                break;
                        }
                }
                tblCol.appendChild(selectValue);
            }
            else {
                var inputFormat = "text";
                var value = obj.value;
                switch (obj.format) {
                    /*
                    TODO: date - contraintes régionales trop complexes à gérer
                    Ici : une date en 18/07/2016 renvoie invalid date, une date en 05/03/2016 (5 mars) est interprétée comme 8 mai
                    On laisse donc l'utilisateur saisir la date au bon format, et on renverra l'erreur Engine s'il y a
                    case "2":
                        inputFormat = "datetime-local";
                        try {
                            var valueDate = new Date(obj.value);
                            value =
                                valueDate.getFullYear() + "-" +
                                new String("00" + (valueDate.getMonth() + 1)).slice(-2) + "-" +
                                new String("00" + valueDate.getDate()).slice(-2) + "T" +
                                new String("00" + valueDate.getHours()).slice(-2) + ":" +
                                new String("00" + valueDate.getMinutes()).slice(-2) + ":" +
                                new String("00" + valueDate.getSeconds()).slice(-2);
                        }
                        catch (ex) {
                            value = obj.value;
                        }
                        break;
                        */
                    case "10":
                        inputFormat = "number";
                        break;
                }

                txtValue = document.createElement("input");
                txtValue.setAttribute("type", inputFormat);
                txtValue.setAttribute("id", "txtFieldValue_" + obj.id);
                if (value != null && value != "")
                    txtValue.setAttribute("value", value);
                tblCol.appendChild(txtValue);
            }
        }
        tblRow.appendChild(tblCol);

        // Description
        tblCol = document.createElement("td");
        tblCol.innerHTML = obj.description;
        tblRow.appendChild(tblCol);
    }
}

function addTrigger() {
    var oNotifTriggerUpd = new eUpdater("Mgr/eAdminNotifTriggerManager.ashx");
    oNotifTriggerUpd.addParam("action", "add", "post");

    oNotifTriggerUpd.send(addTriggerResult);
}

function addTriggerResult(oRes) {
    genericTriggerResult(oRes);
}

function updateTrigger(descId) {
    if (descId != null && !isNaN(descId) && descId != 0) {
        var oNotifTriggerUpd = new eUpdater("Mgr/eAdminNotifTriggerManager.ashx");
        oNotifTriggerUpd.addParam("action", "update", "post");
        oNotifTriggerUpd.addParam("triggerid", descId.toString(), "post");

        var label;
        var triggerFields = document.getElementById("tblTriggerFile").getElementsByTagName("input");
        if (triggerFields != null && triggerFields.length > 0) {
            for (var i = 0; i < triggerFields.length; i++) {
                if ((triggerFields[i].id.substring(0, 14) == "txtFieldValue_"))
                    oNotifTriggerUpd.addParam(triggerFields[i].id.substring(3), triggerFields[i].value, "post");
            }
        }
        var triggerFields = document.getElementById("tblTriggerFile").getElementsByTagName("select");
        if (triggerFields != null && triggerFields.length > 0) {
            for (var i = 0; i < triggerFields.length; i++) {
                if ((triggerFields[i].id.substring(0, 14) == "selFieldValue_"))
                    oNotifTriggerUpd.addParam(triggerFields[i].id.substring(3), triggerFields[i].value, "post");
            }
        }

        oNotifTriggerUpd.send(updateTriggerResult);
    }
}


function updateTriggerResult(oRes) {
    genericTriggerResult(oRes);
}

function loadFile(descId) {
    if (descId != null && !isNaN(descId) && descId != 0) {
        var oNotifTriggerUpd = new eUpdater("Mgr/eAdminNotifTriggerManager.ashx");
        oNotifTriggerUpd.addParam("action", "loadFile", "post");
        oNotifTriggerUpd.addParam("triggerid", descId.toString(), "post");
        oNotifTriggerUpd.send(loadFileResult);
    }
}

function loadFileResult(oRes) {
    if (!oRes) {
        return;
    }

    var bSuccess = getXmlTextNode(oRes.getElementsByTagName("success")[0]).toLowerCase() == "true" ? true : false;

    if (!bSuccess) {
        var sError = getXmlTextNode(oRes.getElementsByTagName("error")[0]);
        alert(sError);
    }
    else {
        clearDiv("divResultFile");

        var divResultFile = document.getElementById("divResultFile");

        var pDescId = document.createElement("p");
        pDescId.innerText = "Edition de la notification n°" + oRes.getElementsByTagName("triggerFile")[0].getAttribute("descId");
        divResultFile.appendChild(pDescId);

        //Autres triggers
        tblTriggerFile = document.createElement("table");
        tblTriggerFile.id = "tblTriggerFile";
        divResultFile.appendChild(tblTriggerFile);

        tblRowheaderFile = document.createElement("tr");
        tblRowheaderFile.setAttribute("class", "header");
        tblTriggerFile.appendChild(tblRowheaderFile);

        tblColFile = document.createElement("th");
        tblColFile.innerHTML = "Champ";
        tblRowheaderFile.appendChild(tblColFile);

        tblColFile = document.createElement("th");
        tblColFile.innerHTML = "Valeur";
        tblRowheaderFile.appendChild(tblColFile);

        tblColFile = document.createElement("th");
        tblColFile.innerHTML = "Description";
        tblRowheaderFile.appendChild(tblColFile);

        var listTriggerFields = oRes.getElementsByTagName("triggerFile")[0].getElementsByTagName("triggerField");
        if (listTriggerFields.length > 0) {
            for (var i = 0, even = false; i < listTriggerFields.length; ++i) {
                var obj = listTriggerFields[i];

                var nId = parseInt(obj.getAttribute("descId"));
                var sValue = obj.innerHTML;
                var sDescription = obj.getAttribute("description");
                var sLabel = obj.getAttribute("friendlyName");
                var sFormat = obj.getAttribute("format");
                var bEditable = obj.getAttribute("editable") == "1";
                var bMultiple = obj.getAttribute("multiple") == "1";
                var sDbValue = obj.getAttribute("dbValue");
                var bIsCatalog = obj.getAttribute("isCatalog") == "1";
                var sCatalogType = obj.getAttribute("catalogType");
                var bIsImage = obj.getAttribute("image") == "1";

                var objTrigger = {
                    id: nId,
                    label: sLabel,
                    value: sValue,
                    description: sDescription,
                    dbValue: sDbValue,
                    editable: bEditable,
                    multiple: bMultiple,
                    format: sFormat,
                    catalogType: sCatalogType,
                    isCatalog: bIsCatalog,
                    image: bIsImage
                };

                even = !even;
                appendTriggerFieldToFileTable(objTrigger, tblTriggerFile, parseInt(oRes.getElementsByTagName("triggerFile")[0].getAttribute("descId")), even);
            }
        }
    }

    tblRowFooterFile = document.createElement("tr");
    tblRowFooterFile.setAttribute("class", "footer");
    tblTriggerFile.appendChild(tblRowFooterFile);

    tblColFile = document.createElement("td");
    tblRowFooterFile.appendChild(tblColFile);
    btnUpdate = document.createElement("input");
    btnUpdate.setAttribute("type", "button");
    btnUpdate.setAttribute("value", "Modifier");
    btnUpdate.setAttribute("onclick", "updateTrigger(" + parseInt(oRes.getElementsByTagName("triggerFile")[0].getAttribute("descId")) + ");");
    tblColFile.appendChild(btnUpdate);
}


function loadList() {
    var oNotifTriggerUpd = new eUpdater("Mgr/eAdminNotifTriggerManager.ashx");
    oNotifTriggerUpd.addParam("action", "listTriggers", "post");

    oNotifTriggerUpd.send(loadListResult);
}

function loadListResult(oRes) {
    if (!oRes) {
        return;
    }

    var bSuccess = getXmlTextNode(oRes.getElementsByTagName("success")[0]).toLowerCase() == "true" ? true : false;

    if (!bSuccess) {
        var sError = getXmlTextNode(oRes.getElementsByTagName("error")[0]);
        alert(sError);
    }
    else {
        clearDiv("divResult");
        clearDiv("divResultFile");

        var divResult = document.getElementById("divResult");

        //Autres triggers
        tblTrigger = document.createElement("table");
        tblTrigger.id = "tblTrigger";
        divResult.appendChild(tblTrigger);

        tblRowheader = document.createElement("tr");
        tblRowheader.setAttribute("class", "header");
        tblTrigger.appendChild(tblRowheader);

        tblCol = document.createElement("th");
        tblCol.innerHTML = "Supprimer";
        tblRowheader.appendChild(tblCol);

        tblCol = document.createElement("th");
        tblCol.innerHTML = "ID";
        tblRowheader.appendChild(tblCol);

        tblCol = document.createElement("th");
        tblCol.innerHTML = "Libellé";
        tblRowheader.appendChild(tblCol);

        tblCol = document.createElement("th");
        tblCol.innerHTML = "Onglet et rubrique déclencheur";
        tblRowheader.appendChild(tblCol);

        tblCol = document.createElement("th");
        tblCol.innerHTML = "Evènement déclencheur";
        tblRowheader.appendChild(tblCol);

        tblCol = document.createElement("th");
        tblCol.innerHTML = "Condition de déclenchement";
        tblRowheader.appendChild(tblCol);

        tblCol = document.createElement("th");
        tblCol.innerHTML = "Type de notification";
        tblRowheader.appendChild(tblCol);

        tblCol = document.createElement("th");
        tblCol.innerHTML = "Mode de diffusion";
        tblRowheader.appendChild(tblCol);

        tblCol = document.createElement("th");
        tblCol.innerHTML = "Destinataires fixes";
        tblRowheader.appendChild(tblCol);

        tblCol = document.createElement("th");
        tblCol.innerHTML = "Rubrique de fiche contenant les destinataires de la notification";
        tblRowheader.appendChild(tblCol);

        tblCol = document.createElement("th");
        tblCol.innerHTML = "Editer";
        tblRowheader.appendChild(tblCol);

        var listTriggers = oRes.getElementsByTagName("listTriggers")[0].getElementsByTagName("trigger");
        if (listTriggers.length > 0) {
            for (var i = 0, even = false; i < listTriggers.length; ++i) {
                var obj = listTriggers[i];

                var nId = parseInt(getXmlTextNode(obj.getElementsByTagName("id")[0]));
                var slabel = getXmlTextNode(obj.getElementsByTagName("label")[0]);

                var objTrigger = {
                    id: nId,
                    label: slabel
                };

                even = !even;
                appendTriggerToTable(objTrigger, tblTrigger, i, even);
            }
        }
    }

    tblRowFooter = document.createElement("tr");
    tblRowFooter.setAttribute("class", "footer");
    tblTrigger.appendChild(tblRowFooter);

    tblCol = document.createElement("td");
    tblRowFooter.appendChild(tblCol);
    btnAdd = document.createElement("input");
    btnAdd.setAttribute("type", "button");
    btnAdd.setAttribute("value", "Ajouter");
    btnAdd.setAttribute("onclick", "addTrigger();");
    tblCol.appendChild(btnAdd);

    for (var i = 0; i < 10; ++i) {
        tblCol = document.createElement("td");
        tblRowFooter.appendChild(tblCol);
    }
}

function getTriggerId(index) {
    var id;
    var hddnId = document.getElementById("hddnId_" + index);
    if (hddnId != null)
        id = parseInt(hddnId.value);

    return id;
}

function deleteTrigger(index) {

    if (index != null && !isNaN(index)) {
        var id = getTriggerId(index);

        if (id != null && !isNaN(id) && id != 0) {
            var oNotifTriggerUpd = new eUpdater("Mgr/eAdminNotifTriggerManager.ashx");
            oNotifTriggerUpd.addParam("action", "delete", "post");
            oNotifTriggerUpd.addParam("triggerid", id.toString(), "post");

            oNotifTriggerUpd.send(deleteTriggerResult);
        }
    }

}

function deleteTriggerResult(oRes) {
    genericTriggerResult(oRes);
}

function genericTriggerResult(oRes) {
    if (!oRes) {
        return;
    }

    var bSuccess = getXmlTextNode(oRes.getElementsByTagName("success")[0]).toLowerCase() == "true" ? true : false;

    if (!bSuccess) {
        var sError = getXmlTextNode(oRes.getElementsByTagName("error")[0]);
        alert(sError);
    }
    else {
        nTriggerId = 0;
        try {
            nTriggerId = parseInt(getXmlTextNode(oRes.getElementsByTagName("triggerId")[0]));
        }
        catch (ex) {
            getXmlTextNode(oRes.getElementsByTagName("error")[0]);
        }

        loadList();
        if (nTriggerId > 0) {
            loadFile(nTriggerId);
        }
    }
}