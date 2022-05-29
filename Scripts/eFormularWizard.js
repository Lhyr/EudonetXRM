
/*********************************************************
 * MOU-01/07/2014
 *
 * Actions sur la liste des formulaire/ lancement de l'assistant
 * Modifiction/Suppression etc.
 *********************************************************/

//Catalog de date
var modalDate = null;
function selectExpireDate(id) {
    modalDate = createCalendarPopUp("OnValidate", 0, 1, top._res_5017, top._res_5003, "OnClick", top._res_29, null, null, "", id, document.getElementById(id).value);
    modalDate.activeInputId = id;
}

function OnValidate(val) {
    var oFld = document.getElementById(modalDate.activeInputId);
    oFld.setAttribute("oldvalue", oFld.value);
    oFld.value = val;
    modalDate.hide();
    checkExpireMsg(oFld);
}
function isValidDate(d) {
    if (Object.prototype.toString.call(d) !== "[object Date]")
        return false;
    return !isNaN(d.getTime());
}

function OnClick(val) {
    alert("SelectDateOk");
}


function checkSubmitMsg(checkbox) {

    if (getAttributeValue(checkbox, "chk") == "1") {
        document.getElementById('li-msg-submit-id').className = 'li-content-step inner-li';
    } else {
        document.getElementById('li-msg-submit-id').className = 'hidden-li';
    }
}

function checkExpireMsg(input) {

    if (input.value != "") {
        document.getElementById('li-msg-expire-id').className = 'li-content-step inner-li';
    } else {
        document.getElementById('li-msg-expire-id').className = 'hidden-li';
    }
}

/*
KHA le 29/04/2015
fonction permettant de retirer certains format de champ de la liste des champ de fusion
*/
function exceptMergeFieldFormat(oFields, nFormat) {
   
    for (var i in oFields) {
        try {
            if (exceptMergeFieldFormat == null)
                continue;

            var aField = oFields[i].split(';');
            if (aField.length != 8)
                continue;

            if (aField[3] == nFormat.toString()) {
                oFields[i] = null;
            }
        }
        catch (e) {
            debugger;
        }
    }
    return oFields;
    
}

