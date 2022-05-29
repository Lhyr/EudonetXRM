//Redimension de l'espace disponible pour le choix des valeurs de champs à conserver.
function AdjustScrollDiv() {
    var divCompFile = document.getElementById("divCompFiles");
    var divCompValues = document.getElementById("divCompValues");

    if (!divCompFile || !divCompValues)
        return;
    var brow = new getBrowser();
    if (brow.isIE && brow.isIE8)
        divCompValues.style.display = 'block';
    else
        divCompValues.style.display = 'none';
    var oSize = getAbsolutePosition(divCompFile);
    var oWin = getWindowWH();
    // HLA - 30px supplémentaire pour la marge
    divCompValues.style.height = (oWin[1] - oSize.h - 30) + 'px';
    divCompValues.style.display = 'block';

}

