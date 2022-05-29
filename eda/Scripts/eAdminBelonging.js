var nsAdminBelonging = nsAdminBelonging || {};

// Affecter valeurs par défaut
nsAdminBelonging.setDefaultValues = function () {
    var userid, username;
    var inputUser;
    var lines = document.querySelectorAll("#tableBelongings tr");
    for (var i = 0; i < lines.length; i++) {
        userid = getAttributeValue(lines[i], "data-userid");
        username = getAttributeValue(lines[i], "data-userdisplayname");
        inputUser = document.getElementById("txtOwner" + userid);
        if (inputUser) {
            setAttributeValue(inputUser, "dbv", userid);
            inputUser.value = username;
        }
        
    }
}

// Affecter fiches publiques à tous les utilisateurs
nsAdminBelonging.setAllPublic = function () {
    var inputs = document.getElementsByClassName("txtOwner");
    for (var i = 0; i < inputs.length; i++) {
        setAttributeValue(inputs[i], "dbv", "0");
        inputs[i].value = "<" + top._res_53 + ">";
    }
}

// Crée une capsule avec toutes les données à mettre à jour
nsAdminBelonging.getUpdCapsule = function (tab) {
    var capsule = new Capsule(tab);

    var hid = document.getElementById("hidPref");
    var dsc = getAttributeValue(hid, "dsc");
    var aDsc = dsc.split("|");
    var inputs = document.getElementsByClassName("txtOwner");
    for (var i = 0; i < inputs.length; i++) {
        capsule.AddProperty(aDsc[0], aDsc[1], getAttributeValue(inputs[i], "dbv"), null, true, getAttributeValue(inputs[i], "data-userid"), true);
    }

    return capsule;
}