var listDdlFieldsChar = "ddlHousenumber;ddlStreet;ddlPlace;ddlVillage;ddlTown;ddlCity;ddlPostcode;ddlCitycode";
var listDdlFieldsGeo = "ddlLatitude;ddlLongitude";

function loadListTab() {
    var oUpd = new eUpdater("Mgr/eAdminAutocompleteAddressManager.ashx");
    oUpd.addParam("action", "listtabs", "post");

    oUpd.send(loadListTabResult);
}

function loadListTabResult(oRes)
{
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
                }
            }
        }
    }
}

function loadMapping() {
    var selectTab = document.getElementById("tabid");

    if (selectTab != null && selectTab.value != "") {
        var selectedTabValue = parseInt(selectTab.value);

        if (selectedTabValue == 300) {
            disableRdTriggerPM(false);
        }
        else {
            disableRdTriggerPM(true);
        }

        var oUpd = new eUpdater("Mgr/eAdminAutocompleteAddressManager.ashx");
        oUpd.addParam("action", "loadmapping", "post");
        oUpd.addParam("tab", selectedTabValue, "post");

        oUpd.send(loadMappingResult);
    }
}

function loadMappingResult(oRes) {
    if (!oRes) {
        return;
    }

    var bSuccess = getXmlTextNode(oRes.getElementsByTagName("success")[0]).toLowerCase() == "true" ? true : false;

    if (!bSuccess) {
        var sError = getXmlTextNode(oRes.getElementsByTagName("error")[0]);
        alert(sError);
    }
    else {
        //chargement des valeurs pour les select
        populateDdlField(oRes, "listfieldschar", "fieldid");

        var listDdlFieldsCharArray = listDdlFieldsChar.split(";")
        if (listDdlFieldsCharArray.length > 0) {
            for (var i = 0; i < listDdlFieldsCharArray.length; ++i) {
                clearElement(listDdlFieldsCharArray[i]);
                populateDdlField(oRes, "listfieldschar", listDdlFieldsCharArray[i]);
            }
        }

        var listDdlFieldsGeoArray = listDdlFieldsGeo.split(";")
        if (listDdlFieldsGeoArray.length > 0) {
            for (var i = 0; i < listDdlFieldsGeoArray.length; ++i) {
                clearElement(listDdlFieldsGeoArray[i]);
                populateDdlField(oRes, "listfieldsgeo", listDdlFieldsGeoArray[i]);
            }
        }

        //chargement autocompletefield
        var nAutocompleteField = parseInt(getXmlTextNode(oRes.getElementsByTagName("autocompletefield")[0]));
        var selectField = document.getElementById("fieldid");
        if (selectField != null)
            selectField.value = nAutocompleteField.toString();


        //chargement mapping
        var listMappings = oRes.getElementsByTagName("listmappings")[0].getElementsByTagName("mapping");
        if (listMappings.length > 0) {
            for (var i = 0; i < listMappings.length; ++i) {
                var obj = listMappings[i];

                var nId = parseInt(getXmlTextNode(obj.getElementsByTagName("id")[0]));
                var nSourceDescid = parseInt(getXmlTextNode(obj.getElementsByTagName("sourcedescid")[0]));
                var nSourceType = parseInt(getXmlTextNode(obj.getElementsByTagName("sourcetype")[0]));
                var nDescid = parseInt(getXmlTextNode(obj.getElementsByTagName("descid")[0]));
                var nOrder = parseInt(getXmlTextNode(obj.getElementsByTagName("order")[0]));
                var sSource = getXmlTextNode(obj.getElementsByTagName("source")[0]);
                var sFieldLabel = getXmlTextNode(obj.getElementsByTagName("fieldlabel")[0]);

                var objMapping = {
                    id: nId,
                    sourcedescid: nSourceDescid,
                    sourcetype: nSourceType,
                    descid: nDescid,
                    order: nOrder,
                    source: sSource,
                    fieldlabel: sFieldLabel
                };

                appendMapping(objMapping);
            }
        }
    }
}

function disableRdTriggerPM(disable)
{
    var rdTriggerPMYes = document.getElementById("rdTriggerPMYes");
    var rdTriggerPMNo = document.getElementById("rdTriggerPMNo");

    if (rdTriggerPMYes != null) {
        rdTriggerPMYes.disabled = disable;
        if (disable)
            rdTriggerPMYes.checked = false;
    }

    if (rdTriggerPMNo != null) {
        rdTriggerPMNo.disabled = disable;
        if (disable)
            rdTriggerPMNo.checked = false;
    }
}

function clearElement(elementid) {
    var element = document.getElementById(elementid);
    while (element.firstChild) {
        element.removeChild(element.firstChild);
    }
}

function populateDdlField(oRes, oResTagName, selectid)
{
    var selectField = document.getElementById(selectid);

    if (selectField != null) {
        addSelectOption(selectField, "", "");

        var listTabs = oRes.getElementsByTagName(oResTagName)[0].getElementsByTagName("field");
        if (listTabs.length > 0) {
            for (var i = 0; i < listTabs.length; ++i) {
                var obj = listTabs[i];

                var nDescId = parseInt(getXmlTextNode(obj.getElementsByTagName("descid")[0]));
                var sLibelle = getXmlTextNode(obj.getElementsByTagName("libelle")[0]);

                addSelectOption(selectField, nDescId.toString(), sLibelle + " (" + nDescId.toString() + ")");
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
/*
function populateSelectFields(elDdl)
{
    var option = document.createElement("option");
    option.setAttribute("value", "");
    option.innerHTML = "";
    elDdl.appendChild(option);
}
*/

function appendMapping(obj)
{
    var ddl;
    var hdn;

    switch (obj.source.toLowerCase()) {
        case "streetname":
            ddl = document.getElementById("ddlStreet");
            hdn = document.getElementById("hdnStreet");
            break;
        case "city":
            ddl = document.getElementById("ddlCity");
            hdn = document.getElementById("hdnCity");
            break;
        case "postalcode":
            ddl = document.getElementById("ddlPostcode");
            hdn = document.getElementById("hdnPostcode");
            break;
        default:
            return;
    }

    if (ddl != null) {
        ddl.value = obj.descid;
    }

    if (hdn != null) {
        hdn.value = obj.id;
    }
}


function setAutocompleteField() {
    var selectTab = document.getElementById("tabid");

    if (selectTab != null && selectTab.value != "") {
        var selectedTabValue = parseInt(selectTab.value);

        var selectField = document.getElementById("fieldid");
        var selectFieldValue = 0;
        if (selectField != null && selectField.value != "")
            selectFieldValue = parseInt(selectField.value);

        var oUpd = new eUpdater("Mgr/eAdminAutocompleteAddressManager.ashx");
        oUpd.addParam("action", "setautocompletefield", "post");
        oUpd.addParam("tab", selectedTabValue, "post");
        oUpd.addParam("autocompletefield", selectFieldValue, "post");

        oUpd.send(genericUpdResult);
    }
}

function setMapping() {
    var selectTab = document.getElementById("tabid");

    if (selectTab != null && selectTab.value != "") {
        var selectedTabValue = parseInt(selectTab.value);

        var rdTriggerValue = false;
        var rdTriggerPMValue = false;

        var rdTrigger = document.querySelector('input[name="rdTrigger"]:checked');
        var rdTriggerPM = document.querySelector('input[name="rdTriggerPM"]:checked');

        if (rdTrigger != null && rdTrigger.value == "yes")
            var rdTriggerValue = true;

        if (rdTriggerPMValue != null && rdTriggerPMValue.value == "yes")
            var rdTriggerPMValue = true;

        var objMappings = {};
        objMappings.Housenumber = getMappingValue("Housenumber");
        objMappings.Street = getMappingValue("Street");
        objMappings.Place = getMappingValue("Place");
        objMappings.Village = getMappingValue("Village");
        objMappings.Town = getMappingValue("Town");
        objMappings.City = getMappingValue("City");
        objMappings.Postcode = getMappingValue("Postcode");
        objMappings.Citycode = getMappingValue("Citycode");
        objMappings.Latitude = getMappingValue("Geography");

        var objMappingsJSON = JSON.stringify(objMappings);

        var oUpd = new eUpdater("Mgr/eAdminAutocompleteAddressManager.ashx");
        oUpd.addParam("action", "savemapping", "post");
        oUpd.addParam("tab", selectedTabValue, "post");
        oUpd.addParam("mappingsJSON", objMappingsJSON, "post");

        oUpd.send(genericUpdResult);
    }
}

function getMappingValue(name)
{
    var ddl = document.getElementById("ddl" + name);
    var hdn = document.getElementById("hdn" + name);

    var objMapping = {};

    if (ddl != null && ddl.value != "" && !isNaN(parseInt(ddl.value)))
        objMapping.value = parseInt(ddl.value);
    else
        objMapping.value = 0;

    if (hdn != null && hdn.value != "" && !isNaN(parseInt(hdn.value)))
        objMapping.id = parseInt(hdn.value);
    else
        objMapping.id = 0;

    return objMapping;
}

function genericUpdResult(oRes) {
    if (!oRes) {
        return;
    }

    var bSuccess = getXmlTextNode(oRes.getElementsByTagName("success")[0]).toLowerCase() == "true" ? true : false;

    if (!bSuccess) {
        var sError = getXmlTextNode(oRes.getElementsByTagName("error")[0]);
        alert(sError);
    }
}