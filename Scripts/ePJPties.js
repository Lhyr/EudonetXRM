function serializePjPties() {
    try{
        var lstFlds = document.getElementById("flds").value;
        if (lstFlds == "")
        {
            eAlert(0, top._res_72, top._res_6649);
            return;
        }

        var aFlds = lstFlds.split("|");
        var sReturn = "";
        for (var i = 0; i < aFlds.length; i++) {
            if (sReturn.length > 0)
                sReturn += "$|$";
            sReturn += aFlds[i] + "$=$" + document.getElementById(aFlds[i]).value;
        }
        return sReturn;
    }
    catch (e) {
        eAlert(0, top._res_72, top._res_6649);
        return;
    }
}

//Verifie que le format de la date saisie soit vide ou correct
function validateDate(idDateToValidate) {
    var inptDate = document.getElementById(idDateToValidate);
    if (inptDate) {
        var dLimitDate = eDate.Tools.GetDateFromString(inptDate.value);
        
        if (inptDate.value != '' && !isValidDate(dLimitDate)) {
            eAlert(0, top._res_846, top._res_846);
            inptDate.value = ''; //On vide la date
            return;
        }
    }
    
}