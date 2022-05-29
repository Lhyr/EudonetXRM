/****CONSTANTES****/
//Position min depuis le haut
var nTopPage = 2;
//Position min de gauche
var nLeftPage = 2;
//Espace entre les eudopart (hauteur)
var SPACE_BETWEEN_PART_HEIGHT = 2;
//Espace entre les eudopart (largeur)
var SPACE_BETWEEN_PART_WIDTH = 2;
//Taille de l'entête
var TITLE_HEIGHT = 19;
//Bordure du corp d'une eudopart (haut+bas ou gauche+droite)
var BORDERBODY = 2;

// <summary>VIDE</summary>
var TYP_CONTENT_EMPTY = 0;
// <summary>Contenu Html</summary>
var TYP_CONTENT_HTML = 1;
// Contenu Rapport en mode liste
var TYP_CONTENT_REPORT = 2;
// Contenu Rapport Fusion chart
var TYP_CONTENT_CHART = 3;
// Contenu PageWeb depuis une URL
var TYP_CONTENT_WEBPAGE = 4;
// Contenu Flux Rss
var TYP_CONTENT_RSS = 5;
// Contenu Flux Rss
var TYP_CONTENT_MRU = 6;
// Contenu Notes/Post-it
var TYP_CONTENT_NOTE_POSTIT = 7;
//Le maximum d'eudopart est de 4 
var maxParts = 4;
/******************/

function setEudopartSelected(boxId, bSelected, bOthers) {
    /// <summary>setEudopartSelected(boxId, bSelected, bOthers)
    ///   <para>Au clique dans une eudopart : Modifie le style du titre</para>
    /// </summary>
    /// <param name="boxId" type="String">id de l'eudopart</param>
    /// <param name="bSelected" type="String">indique si l'eudopart doit être sélectionnée</param>
    /// <param name="bOthers" type="String">indique si l'on doit déselectionner les autres eudopart</param>
    if (!bOthers)
        bOthers = 0;

    var strBox = boxId.replace("box", "");
    var BoxTitle = document.getElementById("BoxTitle" + strBox);
    var BoxLeftCorner = document.getElementById("BoxLeftCorner" + strBox);
    var BoxRightCorner = document.getElementById("BoxRightCorner" + strBox);
    var ControlBox = document.getElementById("ControlBox" + strBox);
    var BoxType = document.getElementById("BoxType" + strBox);
    var strSel = "";
    if (bSelected == 1) {
        strSel = "Selected";
    }
    BoxTitle.className = "ClEudopartTitle" + strSel;
    BoxLeftCorner.className = "CornerTdLeft" + strSel;
    BoxRightCorner.className = "CornerTdRight" + strSel;
    ControlBox.className = "controlbox" + strSel;
    BoxType.className = "TypePart" + strSel;

    //désélection des autres eudoparts
    if (bOthers == 1) {
        for (var i = 1; i <= 4; i++) {
            if ("box" + i != boxId) {
                if (document.getElementById("box" + i)) {
                    try {
                        setEudopartSelected("box" + i, 0, 0);
                    }
                    catch (e) {
                    }
                }
            }
        }
    }

}

/****VARIABLES GLOBALES****/
//Si un mouvement est en court, cette variable est mise à false pendant le mouvement
var bMoveAllowed = true;
//Si un mouvement est en court, cette variable est mise à vrai pendant le mouvement
var bDragApproved = false;
var agt = navigator.userAgent.toLowerCase();
//Navigateur est internet explorer
var is_ie = ((agt.indexOf("msie") != -1) && (agt.indexOf("opera") == -1));
/*
z : element en mouvement
x : position x de l'élément en mouvement
y : position y de l'élément en mouvement
*/
var z, x, y;
var maxleft, maxtop, maxright, maxbottom;   //TODO GCH : à part bottom les autres semblent inutilisée, à vérifier
/*Position X et Y de la div source du déplacement*/
var currX;
var currY;
//Div apparraissant lors du déplacement d'une eudopart à la place de l'eudopart déplacée
var DashedBox;
function InitDashedBox() {
    DashedBox = document.getElementById("DashedBox");
}
/**************************/
function drags(e) {
    /// <summary>drags(e)
    ///   <para>Au Clique dans le titre d'une eudopart : pour l'effet de drag/drop de déplacement</para>
    /// </summary>
    /// <param name="e" type="String">evennement appelant</param>
    if (!(is_ie) && !(!is_ie))
        return;
    if (!bMoveAllowed)
        return;

    var firedobj = (!is_ie) ? e.target : event.srcElement;
    var topelement = (!is_ie) ? "HTML" : "BODY";
    if (firedobj.className != "ClEudopartTitle" && firedobj.className != "ClEudopartTitleSelected")
        return;
    while (firedobj.tagName != topelement && firedobj.className != "box" && firedobj.tagName != 'SELECT' && firedobj.tagName != 'TEXTAREA' && firedobj.tagName != 'INPUT' && firedobj.tagName != 'IMG') {
        //you can add the elements that cannot be used for drag here. using their class name or id or tag names
        firedobj = (!is_ie) ? firedobj.parentNode : firedobj.parentElement;
    }

    if (firedobj.className == "box") {

        z = firedobj;
        var tmpheight = z.style.height.split("px");
        maxbottom = (tmpheight[0]) ? document.body.clientHeight - tmpheight[0] : document.body.clientHeight - 20;

        temp1 = parseInt(z.style.left + 0);
        temp2 = parseInt(z.style.top + 0);

        x = (!is_ie) ? e.clientX : event.clientX;
        y = (!is_ie) ? e.clientY : event.clientY;
        if (!bDragApproved) {
            currX = temp1;
            currY = temp2;
            DashedBox.style.left = currX + "px";
            DashedBox.style.top = currY + "px";
            DashedBox.style.display = 'block';
            DashedBox.style.visibility = 'visible';
            DashedBox.style.width = z.style.width;
            DashedBox.style.height = z.style.height;
        }
        bDragApproved = true;
        z.style.zIndex = 3;
        //z.style.opacity = "0.25";
        document.onmousemove = move;
        return false;
    }
}


function doMouseUp() {
    /// <summary>doMouseUp()
    ///   <para>Au laché d'eudopart depuis clique gauche suite à un drag</para>
    /// </summary>
    if (bDragApproved) {
        if (z.className == "box") {
            z.style.zIndex = 1;
            //z.style.opacity = "1";
            z.style.top = DashedBox.style.top;
            z.style.left = DashedBox.style.left;
            DashedBox.style.display = "none";

            DashedBox.style.visibility = "hidden";

            resizeBodies();
            setNewOrder();


        }

        bDragApproved = false;
    }
}
function move(e) {
    /// <summary>move(e)
    ///   <para>Effet visuel de déplacement de l'eudopart</para>
    /// </summary>
    /// <param name="e" type="String">evennement appelant</param>
    var tmpXpos = (!is_ie) ? temp1 + e.clientX - x : temp1 + event.clientX - x;
    var tmpYpos = (!is_ie) ? temp2 + e.clientY - y : temp2 + event.clientY - y;
    if (bDragApproved) {

        z.style.left = tmpXpos + "px";
        z.style.top = tmpYpos + "px";


        if (tmpXpos < maxleft) z.style.left = maxleft + "px";
        if (tmpXpos > maxright) z.style.left = maxright + "px";

        //if(tmpYpos < maxtop)z.style.top = maxtop;
        //if(tmpYpos > maxbottom)z.style.top = maxbottom;
        var vTmpx = (!is_ie) ? e.clientX : event.clientX;
        var vTmpy = (!is_ie) ? e.clientY : event.clientY;
        if (z.className == "box")
            moveOthersDivs(z.id, vTmpx, vTmpy);

        return false
    }
}
function moveOthersDivs(strBoxId, vx, vy) {
    /// <summary>moveOthersDivs(strBoxId, vx, vy)
    ///   <para>Enregistrement Visuel des nouvelles positions des eudoparts</para>
    /// </summary>
    /// <param name="strBoxId" type="String">eudopart déplacé</param>
    /// <param name="vx" type="String">nouvelle position (left) de l'eudopart modifiée</param>
    /// <param name="vy" type="String">nouvelle position (top) de l'eudopart modifiée</param>
    var CurrentBox = document.getElementById(strBoxId);
    var OtherBox;
    var x1OtherBox;
    var y1OtherBox;
    var x2OtherBox;
    var y2OtherBox;
    var i;

    var OtherBoxes = getElementsByAttribute(document, "div", "ednDivType", "HomePart");
    var i;

    for (i = 0; i < OtherBoxes.length; i++) {

        OtherBox = OtherBoxes[i];

        y1OtherBox = parseInt(OtherBox.style.top);
        x1OtherBox = parseInt(OtherBox.style.left);
        var nWidth = parseInt(OtherBox.style.width);
        var nHeight = parseInt(OtherBox.style.height);

        y2OtherBox = y1OtherBox + nHeight;
        x2OtherBox = x1OtherBox + nWidth;

        if ((vx > x1OtherBox) && (vx < x2OtherBox) && (vy > y1OtherBox) && (vy < y2OtherBox) && (CurrentBox != OtherBox)) {

            var wTmp = CurrentBox.style.width;
            var hTmp = CurrentBox.style.height;
            var xTmp = OtherBox.style.left;
            var yTmp = OtherBox.style.top;

            //échanger les tailles
            CurrentBox.style.width = OtherBox.style.width;
            CurrentBox.style.height = OtherBox.style.height;

            OtherBox.style.width = wTmp;
            OtherBox.style.height = hTmp;

            //échanger le ednOrder
            var nCurrTmpOrder = CurrentBox.getAttribute("ednOrder");
            var nOtherTmpOrder = OtherBox.getAttribute("ednOrder");
            CurrentBox.setAttribute("ednOrder", nOtherTmpOrder);
            OtherBox.setAttribute("ednOrder", nCurrTmpOrder);

            //Positionne le div dashed
            DashedBox.style.left = OtherBox.style.left;
            DashedBox.style.top = OtherBox.style.top;
            DashedBox.style.width = CurrentBox.style.width;
            DashedBox.style.height = CurrentBox.style.height;
            DashedBox.style.display = 'block';
            DashedBox.style.visibility = 'visible';
            //positionne le div cible
            OtherBox.style.left = currX + "px";
            OtherBox.style.top = currY + "px";
            resizeBodies();
            currX = parseInt(xTmp);
            currY = parseInt(yTmp);
        }
    }
}
function resizeBodies() {
    /// <summary>resizeBodies()
    ///   <para>Redimensionne le contenu de l'eudopart en prenant en compte le titre</para>
    /// </summary>
    var BodyBoxes = getElementsByAttribute(document, "div", "ednDivType", "BoxBodyDiv");
    for (i = 0; i < BodyBoxes.length; i++) {
        BodyBoxes[i].style.height = (parseInt(document.getElementById("box" + parseInt(i + 1)).style.height) - TITLE_HEIGHT - BORDERBODY) + 'px';
        var nPartId = BodyBoxes[i].getAttribute("ednPartId");
        var nPartType = BodyBoxes[i].getAttribute("ednPartType");
        //Resizes spécifiques
        if (nPartType == TYP_CONTENT_MRU) { //RESIZE pour les mru pour ne pas avoir d'ascensseur en trop.
            var ednPartTab = BodyBoxes[i].getAttribute("ednPartTab");
            document.getElementById("div" + ednPartTab).style.height = parseInt(BodyBoxes[i].style.height) + 'px';
        }
    }
}

function resizeSyncFusionChart(oBox) {
    var chartSelector = document.querySelectorAll("div[edndivtype='BoxBodyDiv']");

    if (typeof (chartSelector) == "undefined" || chartSelector == null || chartSelector.length == 0)
        return;

    for (var index in chartSelector) {
        if (chartSelector.hasOwnProperty(index)) {
            var oBox = chartSelector[index];
            var divChart = oBox.querySelectorAll("div[syncFusionChart='1']");
            if (typeof (divChart) != "undefined" && divChart.length > 0) {
                var chart = $("#" + divChart[0].id);
                if (typeof chart.data().ejChart != 'undefined') {
                    chart = chart.ejChart("instance");
                    var h = oBox.clientHeight;
                    var w = oBox.clientWidth;
                    chart.size = { height: '100%', width: '100%' };
                    chart.redraw();
                }
            }
        }

    }



}

function setNewOrder() {
    /// <summary>setNewOrder()
    ///   <para>Modifit lordre visuel des EUDOPART suite au drag</para>
    /// </summary>
    var i;
    var strOrder = "";
    var aDivs;
    var nPartIdTmp;

    for (i = 0; i < maxParts; i++) {
        aDivs = getElementsByAttribute(document, "div", "ednOrder", i + 1);
        if (aDivs.length > 0) {
            nPartIdTmp = aDivs[0].getAttribute("ednPartId");
            if (strOrder != "") {
                strOrder = strOrder + ";"
            }
            strOrder = strOrder + nPartIdTmp;
        }
    }

    document.getElementById("CustomEudoPartsId").value = strOrder;
    SetPref(strOrder, "EudoPartCustom");

}

function SetPref(strValue, strPrefField) {
    /// <summary>SetPref(strValue, strPrefField) 
    ///   <para>Sauvegarde de pref (limité dans le manager appelé à eudopartcustom)</para>
    /// </summary>
    /// <param name="strValue" type="String">valeurs à insérer dans la pref</param>
    /// <param name="strPrefField" type="String">pref à modifier</param>
    try {
        var oHpgUpdater = new eUpdater("mgr/eHomepageManager.ashx", 1);
        oHpgUpdater.addParam("action", "savepref", "post");
        oHpgUpdater.addParam(strPrefField, strValue, "post");
        oHpgUpdater.ErrorCallBack = function () { };
        oHpgUpdater.send(function () { }, null);
    }
    catch (e)
    { }
}



var vExpandTop;
var vExpandLeft;
var vExpandWidth;
var vExpandHeight;
var bMoveAllowed = true;
//
function expandPart(strPartId, strBodyId) {
    /// <summary>expandPart(strPartId, strBodyId)
    ///   <para>Permet d'agrandir l'eudopart</para>
    /// </summary>
    /// <param name="strPartId" type="String">id de l'eudopart</param>
    /// <param name="strBodyId" type="String">id du contenu de l'eudopart</param>

    var BoxPart = GetBoxContainerWH();
    height = BoxPart[1];
    width = BoxPart[0];
    width = width - SPACE_BETWEEN_PART_WIDTH - nLeftPage;
    height = height - SPACE_BETWEEN_PART_HEIGHT - nTopPage;
    var oBox = document.getElementById(strPartId);
    var oBodyPart = document.getElementById(strBodyId);
    var bIsExpanded = (getAttributeValue(oBox, "endIsExpanded") == "1");
    if (bIsExpanded) {
        oBox.style.top = vExpandTop;
        oBox.style.left = vExpandLeft;
        oBox.style.width = vExpandWidth;
        oBox.style.height = vExpandHeight;
        oBox.setAttribute("endIsExpanded", "0");
        oBox.style.zIndex = 1;
        bMoveAllowed = true;
        ShowAllBoxes();
    }
    else {
        vExpandTop = oBox.style.top;
        vExpandLeft = oBox.style.left;
        vExpandWidth = oBox.style.width;
        vExpandHeight = oBox.style.height;
        HideOtherBoxes(strPartId);


        oBox.style.top = nTopPage + 'px';
        oBox.style.left = nLeftPage + 'px';
        oBox.style.width = width + 'px';
        oBox.style.height = height + 'px';
        oBox.setAttribute("endIsExpanded", "1");
        oBox.style.zIndex = 3;
        bMoveAllowed = false;


    }
    resizeBodies();
    resizeSyncFusionChart(oBox);
}
function GetBoxContainerWH() {

    //La div innerContainer prend toute la largeur et la hauteur du contenu de la page d'accueil 
    var innerContainer = document.getElementById("innerContainer");
    if (innerContainer && innerContainer.clientWidth && innerContainer.clientWidth > 0 && innerContainer.clientHeight > 0)
        return [innerContainer.clientWidth, innerContainer.clientHeight];

    return GetMainDivWH();
}




function HideOtherBoxes(strPartId) {
    /// <summary>HideOtherBoxes(strPartId)
    ///   <para>Cache les Eudopart non agrandit</para>
    /// </summary>
    /// <param name="strPartId" type="String">id de l'eudopart</param>
    var OtherBoxes = getElementsByAttribute(document, "div", "ednDivType", "HomePart");
    var i;
    for (i = 0; i < OtherBoxes.length; i++) {
        OtherBox = OtherBoxes[i];
        if (OtherBox.id != strPartId) {
            OtherBox.style.visibility = 'hidden';
            OtherBox.style.display = 'none';
        }
    }
}
function ShowAllBoxes() {
    /// <summary>ShowAllBoxes()
    ///   <para>Affiche toutes les Eudopart non agrandit</para>
    /// </summary>
    var OtherBoxes = getElementsByAttribute(document, "div", "ednDivType", "HomePart");
    var i;
    for (i = 0; i < OtherBoxes.length; i++) {
        OtherBox = OtherBoxes[i];
        OtherBox.style.visibility = 'visible';
        OtherBox.style.display = 'block';
    }
}
function showEudoPart(nPartId) {
    /// <summary>showEudoPart(nPartId)
    ///   <para>Affiche l'eudopart dans une pop up TODO GCH</para>
    /// </summary>
    /// <param name="nPartId" type="String">id de l'eudopart</param>
    if (nPartId == 0)
        return;
    //TODO GCH
    window.open('HomePageAdvancedRender.asp?PartId=' + nPartId, "", "menubar=no, status=no, scrollbars=auto, menubar=no, width=700, height=600");     //TODO GCH
}

function afterHomepageLoaded() {
    /// <summary>afterHomepageLoaded()
    ///   <para>Valeurs dynamique à effectuer après chargement des eudopart (exemple : chargement des evennement de déplacement des eudopart)</para>
    /// </summary>
    var oMainBox = document.getElementById("HpContainer");
    var bLocked = (getAttributeValue(oMainBox, "ednIsLocked") == "1");
    var bReadonly = (getAttributeValue(oMainBox, "ednIsReadonly") == "1");

    if (!(bLocked || bReadonly)) {
        document.onmousedown = drags;     //Fonction appelée au clique sur l'une des eudoparts
        document.onmouseup = doMouseUp;   //Fonction appelée au laché du clique
    }

}

function goMruFile(e) {
    if (!e)
        e = window.event;
    var eid = "";
    var src = (e.originalTarget || e.srcElement);
    while (src && eid == "") {
        eid = getAttributeValue(src, "eid");
        if (src.parentNode) {
            src = src.parentNode;
        }
        else
            break;
    }
    if (eid == "")
        return;

    loadFileWithEid(eid);   //Redirection vers la fiche cliquée
}

// Ouvre une fiche à partir de l'eid
function loadFileWithEid(eid) {
    var oId = eid.split('_');
    var nTab = oId[0];
    var nId = oId[oId.length - 1];
    if (oId != '') {
        top.loadFile(nTab, nId);
    }
}

/* fonction loadValue : Charge la fiche au premier plan de la ligne cliquée */
function loadMruValue(oSelectedObjectId) {
    var oSelectedObject = document.getElementById(oSelectedObjectId);
    if (!oSelectedObject)
        return;
    // id  
    loadFileWithEid(getAttributeValue(oSelectedObject, "eid"));
}

function initEudopartList() {
    // HLA - Bug #24495- Cela fait des appels à eSelectionManager.ashx pour chaque colonnes -> 
    //      update sur PREF -> super performance -> pas de conservation des resize sur les colonnes -> perte pref des users !
    // Dans ce cas, il faudrai fixer la colonne à la dim du EudoPart, non ?
    /*
    var OtherBoxes = getElementsByAttribute(document, "div", "ednPartType", "6");
    for (var cpt = 0; cpt < OtherBoxes.length; cpt++) {
        var oBox = OtherBoxes[cpt];
        var nTab = getNumber(getAttributeValue(OtherBoxes[cpt], "ednPartTab"));
        if (!isNaN(nTab) && nTab > 0) {
            autoResizeColumns(nTab, oBox);
        }
    }
    */
}


/*Filtre de recherche : Necessite eFilterWizardLight.js*/


var SEPARATOR_LVL1 = "#|#";
//Action à la validation d'une recherche d'eudopart
function onOkEudoPartFilter(nPartId) {
    var oMax_nbre_line = document.getElementById("countfields_" + nPartId);
    if (!oMax_nbre_line)
        return;
    var max_nbre_line = getNumber(oMax_nbre_line.value);
    var i = 0;
    var nfilterType = 0; //TypeFilter.USER
    //DIctionnaire bi-dimensionnel : 1ère dimension égal toutes les tables
    // 2ème dimension égal liste des descid de champ et leur valeur 
    // on cré le dico seulement si valeur de saisie !
    var tabDict = new Dictionary();
    for (i = 0; i < max_nbre_line; i++) {
        var oLine = document.getElementById("value_" + nPartId + "_" + i);
        if (oLine) {
            var oOp = document.getElementById("op_" + nPartId + "_" + i);
            var lineOp = oOp.options[oOp.selectedIndex].value;

            var ntab = 0;
            var ndescid = getNumber(oLine.getAttribute("edndescid"));
            if (ndescid > 0) {
                var myValue = "";
                var ednFreeText = oLine.getAttribute("ednfreetext");
                if (ednFreeText == "1")
                    myValue = oLine.value;
                else
                    myValue = oLine.getAttribute("ednvalue");
                var myDisplayValue = oLine.value;
                if (myValue != "" && typeof (myValue) != "undefined") {
                    ntab = getTabDescid(ndescid);
                    var tabDescidDict = tabDict.Lookup(ntab);
                    if (!tabDescidDict) {
                        tabDict.Add(ntab, new Dictionary());
                        tabDescidDict = tabDict.Lookup(ntab);
                    }
                    tabDescidDict.Add(ndescid, lineOp + SEPARATOR_LVL1 + myValue);
                }
            }
        }
    }
    var filterparam = "";
    var nTab = 0;
    /*Parcours de chaque tables********************/
    var keys = tabDict.Keys;
    for (var i = 0; i < keys.length; i++) {
        var tabDict2 = tabDict.Lookup(keys[i]);
        var tabId = keys[i];
        var tabop = "1";   //Toujours opérateur AND (1) pour les filtres d'eudopart

        _tabParams = "";
        if (i > 0)
            _tabParams += "&" + getTabOperator(tabop, i) + "&";
        else
            nTab = tabId;
        _tabParams += getTabFile(tabId, i);

        //--Parcours de chaques champ de table
        var keys2 = tabDict2.Keys;
        for (var j = 0; j < keys2.length; j++) {
            var descid = keys2[j];
            var logicLineOp = "1";   //Toujours opérateur AND (1) pour les filtres d'eudopart
            var myVal = tabDict2.Lookup(keys2[j]);
            var tabVal = myVal.split(SEPARATOR_LVL1);
            var lineOp = tabVal[0];
            var sValue = tabVal[1];
            var paramLine = "";
            if (j > 0)
                paramLine += getLogicLineParam(logicLineOp, i, j);
            paramLine += getLineValueParam(descid, lineOp, sValue, i, j);

            _tabParams += "&" + paramLine;
        }
        //-----------------------------------
        filterparam += _tabParams;
    }
    if (filterparam != "")
        filterparam += getGlobalTabsOptions(0, 0, 0, 0);
    /**********************************************/

    if (filterparam != "") {
        /////////////////////////////////////////////////////////
        var bViewPerm = 0;
        var bUpdatePerm = 0;
        var bsaveas = 0;
        var bApply = 1;
        var upd = new eUpdater("mgr/eFilterWizardManager.ashx", 0);
        upd.addParam("maintab", nTab, "post");
        upd.addParam("filterid", 0, "post");
        upd.addParam("filterparams", filterparam, "post");
        upd.addParam("filtertype", nfilterType, "post");
        upd.addParam("filtername", "", "post");
        upd.addParam("viewperm", bViewPerm, "post");
        upd.addParam("updateperm", bUpdatePerm, "post");
        upd.addParam("saveas", bsaveas, "post");

        //Gestion d'erreur : a priori, pas de traitement particulier
        upd.ErrorCallBack = function () { };

        upd.addParam("applyfilter", bApply, "post");
        upd.addParam("action", "validfilter", "post");
        upd.send(goTabAfterFilter, nTab);
    }
}
//près enregistrement du filtre en pref, on affiche la liste correspondante
function goTabAfterFilter(oRes, nTab) {
    var filterid = getXmlTextNode(oRes.getElementsByTagName("filterid")[0]);
    if (filterid > 0)
        goTabList(nTab);
}