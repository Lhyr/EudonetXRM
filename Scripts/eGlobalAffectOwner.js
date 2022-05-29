
//Position des informations dans chaque ligne du tableau de sélection
var IDX_CHK = 0;
var IDX_USER = 1;
var IDX_NB = 2

var MainTab;
var repNbEditor;
var bTooMuchRec = false;

function onLoad() {
    MainTab = document.getElementById("UsrLstTab");

    //intialisation d'un eFieldEditor pour répartir les fiches
    var repNbPopup = new ePopup("repNbPopup", "50px", "15px", 0, 0, document.body, false);
    repNbEditor = new eFieldEditor("inlineEditor", repNbPopup, "repNbEditor", "");
    repNbEditor.action = "GARepNb";

    sumUp();
    resizeGAUserList();
}

function testMainTab() {
    if (!MainTab)
        MainTab = document.getElementById("UsrLstTab");

    if (!MainTab || MainTab.tagName != "TABLE" || MainTab.rows.length == 0)
        return false;

    return true;
}

function selAllUsr(obj) {
    if (!testMainTab())
        return;

    var nRows = MainTab.rows.length;

    for (var i = 1; i < nRows; i++) {
        var oChkBox = document.getElementById("usr" + i);

        if (!oChkBox)
            continue;

        // on vérifie que l'état de la case à cocher en tete de colonne est différent de celle permettant de sélectionner un user
        if ((getAttributeValue(oChkBox, "chk") == "1") != (getAttributeValue(obj, "chk") == "1"))
            chgChk(oChkBox);

    }

    autoSet();

}

function activUsr(obj) {

    if (!testMainTab())
        return;

    var oThisRow = document.getElementById(obj.id.replace("usr", "line"));

    var oNbRem = document.getElementById("nbRem");
    var oNbSetRec = document.getElementById("nbSetRec");

    sumUp();
    autoSet();

    if (getAttributeValue(obj, "chk") == "1") {
    //Modif par KHA le 17/10/2013 : lorsqu'on sélectionne un user cela redéclanche le calcul automatique sur tous les autres user
    /*
        if (!bTooMuchRec)
            SetText(oThisRow.cells[IDX_NB], GetText(oNbRem));
        else
            SetText(oThisRow.cells[IDX_NB], 0);
*/
       // SetText(oNbSetRec, nTotalFlNb);
        updRem(0);
    }
    else {
        SetText(oThisRow.cells[IDX_NB], "");
    }

}

// donne les information d'ensemble en bas de page
function sumUp() {

    if (!testMainTab())
        return;

    var nRows = MainTab.rows.length;


    var oNbUsr = document.getElementById("nbUsr");
    var oNbSetRec = document.getElementById("nbSetRec");

    var nbUsr = 0;
    var nbRem = 0;
    var nbSetRec = 0;

    for (var i = 1; i < nRows; i++) {
        var oChkBox = document.getElementById("usr" + i);

        if (!oChkBox || getAttributeValue(oChkBox, "chk") != "1")
            continue;

        nbUsr++;

        var oRow = document.getElementById(oChkBox.id.replace("usr", "line"));
        var nb = GetText(oRow.cells[IDX_NB]);

        if (!isNumeric(nb))
            continue;

        nbSetRec += parseInt(nb);

    }

    // nTotalFlNb : nombre total des fiches à répartir - déclarée dans eGlobalAffectOwner.aspx
    nbRem = nTotalFlNb - nbSetRec

    SetText(oNbUsr, nbUsr);
    SetText(oNbSetRec, nbSetRec);
    updRem(nbRem);


}

// répartion automatique
function autoSet() {

    if (!testMainTab())
        return;

    var nRows = MainTab.rows.length;

    var nbUsr = 0;
    var nbPerUsr = 0;
    var nbRem = 0;

    var oNbUsr = document.getElementById("nbUsr");
    var oNbSetRec = document.getElementById("nbSetRec");


    // MCR: 26816 , traitement champ utilisateur, repartition automatique, 
    // la checkbox, utilisateur masquee est cochee alors set du boolean : bChkUnmsk a true
    var bChkUnmsk = false;
    var oChkUnmsk = document.getElementById("chkUnmsk");
    if (oChkUnmsk) {
        if (getAttributeValue(oChkUnmsk, "chk") == "1") {
            // la check box 'afficher les utilisateurs masques' est cochee 
            bChkUnmsk = true;

        }
    }


    var aChkBox = new Array();
    for (var i = 1; i < nRows; i++) {

        var oChkBox = document.getElementById("usr" + i);
        if (!oChkBox)
            continue;

        var oRow = document.getElementById(oChkBox.id.replace("usr", "line"));
        if (!oRow)
            continue;

        if (getAttributeValue(oChkBox, "chk") != "1") {
            SetText(oRow.cells[IDX_NB], "");
            continue;
        }

        // MCR: 26816 , traitement champ utilisateur, repartition automatique
        // si le boolean : bChkUnmsk est a true (afficher les utilisateurs masques), alors afficher les utilisateurs masques également
        // si l'attribut h=1 (pour hidden) 
        bUserHidden = false;
        if (getAttributeValue(oRow, "h") == "1") {
            bUserHidden = true;
        }

        // MCR 26816 traitement champ utilisateur, repartition automatique
        // si la checkbox 'afficher les users masques' n est pas coche et si l utilisateur courant est de type 'hidden' 
        // alors ne pas le selectionner dans la repartition automatique
        if (!bChkUnmsk && bUserHidden) {
            //
        }
        else 
            aChkBox.push(oChkBox);

    }

    nbUsr = aChkBox.length;

    if (nbUsr == 0) {
        SetText(oNbUsr, nbUsr);
        SetText(oNbSetRec, 0);
        updRem(nTotalFlNb);         
         return;
    }

    // nTotalFlNb : nombre total des fiches à répartir - déclarée dans eGlobalAffectOwner.aspx
    // on fait la division entière de ce chiffre pour obtenir un première répartition des fiches
    nbPerUsr = Math.floor(nTotalFlNb / nbUsr);

    // on calcul le restant à répartir
    nbRem = nTotalFlNb % nbUsr;

    for (var i = 0; i < nbUsr; i++) {
        var nb = nbPerUsr;

        //affectation du reste
        if (i < nbRem)
            nb++;

        var oRow = document.getElementById(aChkBox[i].id.replace("usr", "line"));
        SetText(oRow.cells[IDX_NB], nb);
    }



    SetText(oNbUsr, nbUsr);
    SetText(oNbSetRec, nTotalFlNb);
    updRem(0);

}

//Afficher tous les utilisateurs ou seulement ceux qui sont sélectionnés
// bool a true si on affiche que les users selectionnés
// bool à false si on affiche tous
function dispOnlySelUsr(bool) {
    if (!testMainTab())
        return;

    var nRows = MainTab.rows.length;
    var aChkBox = new Array();


    // MCR: 26816 , traitement champ utilisateur, repartition automatique, 
    // la checkbox, utilisateur masquee est cochee alors set du boolean : bChkUnmsk a true
    var bChkUnmsk = false;
    var oChkUnmsk = document.getElementById("chkUnmsk");
    if (oChkUnmsk) {
        if (getAttributeValue(oChkUnmsk, "chk") == "1") {
            // la check box 'afficher les utilisateurs masques' est cochee 
            bChkUnmsk = true;

        }
    }


    for (var i = 1; i < nRows; i++) {

        var oRow = MainTab.rows[i];
        var oGpRow;

        if (!oRow)
            continue;

        var oChkBox = document.getElementById("usr" + i);

        // MCR: 26816 , traitement champ utilisateur, repartition automatique
        // si le boolean : bChkUnmsk est a true (afficher les utilisateurs masques), alors afficher les utilisateurs masques également
        // si l'attribut h=1 (pour hidden) 
        bUserHidden = false;
        if (getAttributeValue(oRow, "h") == "1") {
            bUserHidden = true;
        }


        // on est ici dans le cas d'un groupe : on le cache et on le réaffiche si on a séelectionné un user correspondant
        if (!oChkBox) {
            oGpRow = oRow;
            if (bool)
                oGpRow.style.display = "none";
            else
                            
                oGpRow.style.display = "";
            continue;
        }


        if (getAttributeValue(oChkBox, "chk") != "1" && bool) {
            SetText(oRow.cells[IDX_NB], "");
            oRow.style.display = "none";
            continue;
        }

        // MCR 26816 traitement champ utilisateur, repartition automatique
        // si la checkbox 'afficher les users masques' n est pas coche et si l utilisateur courant est de type 'hidden' 
        // alors ne pas l afficher, si le radio bouton afficher tous les utilisateurs est selectionne (bool=false)
        if (!bChkUnmsk && bUserHidden) {
            oRow.style.display = "none";
        }
        else {
            oRow.style.display = "";
            if (oGpRow)
                oGpRow.style.display = "";
        }


    }



}

function updNb(cellNb) {
    var nb = "";

    if (document.getElementById('eInlineEditor'))
        nb = document.getElementById('eInlineEditor').value;

    if (isNumeric(nb))
        SetText(cellNb, nb);

    var oRow = cellNb.parentElement;

    var oChkBox = document.getElementById(oRow.id.replace("line", "usr"));
    chgChk(oChkBox, true);

    sumUp();


}

function updRem(nbRem) {
    var oNbRem = document.getElementById("nbRem");
    var oLblRem = document.getElementById("lblRem");

    if (nbRem < 0) {
        SetText(oLblRem, top._res_6427);
        bTooMuchRec = true;
    }
    else {
        SetText(oLblRem, top._res_6426);
        bTooMuchRec = false;
    }

    SetText(oNbRem, Math.abs(nbRem));

}


function getDistribution() {
    if (!testMainTab()) {
        return;
    }

    var sReturn = "";
    var nRows = MainTab.rows.length;

    for (var i = 1; i < nRows; i++) {
        var oRow = MainTab.rows[i];
        var oChkBox = document.getElementById("usr" + i);
        if (getAttributeValue(oChkBox, "chk") != "1")
            continue;

        var nNb = getNumber(GetText(oRow.cells[IDX_NB]));
        var userid = getAttributeValue(oRow, "u");
        var username = GetText(oRow.cells[IDX_USER]);
        if (sReturn.length > 0)
            sReturn += "$|$";

        sReturn += userid + ":" + username + ":" + nNb;
    }


    return sReturn;
}

function dispUsrMasked(obj) {
    var bDisp = getAttributeValue(obj, "chk") == "1";
    var oTab = document.getElementById("UsrLstTab");

    for (var i = 0; i < oTab.rows.length; i++) {
        var oRow = oTab.rows[i];
        if (getAttributeValue(oRow, "h") != "1")
            continue;

        if (bDisp)
            oRow.style.display = "";
        else
            oRow.style.display = "none";
    }

}

function resizeGAUserList() {
    
    try {
        var height = window.innerHeight;
        height -= 20; // pour la marge
        /* kha --> ne marche pas... probablement a cause des float sur l'ensemble des childrenElement...
        var oDivBottom = document.getElementById("DivBottom");
        if (oDivBottom)
            height -= oDivBottom.offsetHeight;
            */
        height -= 100;  // pour le divBottom
        height -= 20; //margintop

        document.getElementById("GAUserList").style.height = height + "px";
    }
    catch(ex)
    {
    }
}