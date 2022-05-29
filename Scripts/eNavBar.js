
//mou sauvegarde des dimensions de la fenetre
var oWinSizeMem = {
    _nWidth: -1,
    _nHeight: -1
};


var searchNavBarTimer;

// Charge les MRUs
function LoadMru(nDescId, loadFrom) {
    if (nDescId == null || nDescId == "" || nDescId == "0") {
        return;
    }

    nDescId = Number(nDescId);

    var oLst = top.document.getElementById("ul_mru_" + nDescId);
    var oeParam = top.document.getElementById('eParam').contentWindow;

    if (!oLst || typeof (oeParam) == "undefined" || !oeParam || typeof (oeParam.GetParam) != "function")
        return;

    // RaZ
    ResetLst(nDescId);

    var sMrus = oeParam.GetParam("MRU_" + nDescId);
    if (sMrus != null && sMrus != '') {
        var mruValuesArray = sMrus.split('$|$');

        if (mruValuesArray.length > 0) {


            var nMiniFileTab = (oeParam.GetParam("MiniFileEnabled_" + nDescId) == "1") ? nDescId : 0;

            oLst.parentNode.style.display = "";

            for (var j = 0; j < mruValuesArray.length; j++) {
                // id$;$txt$;$url
                var tmp = mruValuesArray[j].split('$;$');
                if (tmp[2] == "99") {
                    if (tmp.length == 3 && tmp[1].length > 0) {
                        if (!loadFrom)
                            loadFrom = LOADFILEFROM.TAB;

                        fct = (function (Id) {
                            return function () {

                                if (typeof nsAdmin != 'undefined' && loadFrom == LOADFILEFROM.ADMIN)
                                    nsAdmin.loadFile(nDescId, Id);
                                else
                                    top.loadFile(nDescId, Id, 3, false, loadFrom);
                            }
                        })(tmp[0]);
                        oLink = CreateMenuMru(tmp[0], tmp[1], fct, nMiniFileTab);
                        oLst.appendChild(oLink);
                    }
                }
                else {  //Mode Planning
                    fct = (function (dest) {
                        return function () {
                            if (typeof nsAdmin != 'undefined' && loadFrom == LOADFILEFROM.ADMIN)
                                nsAdmin.goTabList(nDescId);

                            top.setCalViewMode(dest, nDescId);
                        }
                    })(tmp[2]);
                    oLink = CreateMenuMru(tmp[0], tmp[1], fct, nMiniFileTab);
                    oLst.appendChild(oLink);
                }
            }
        }
    }

    adjustSbMenuPosition(oLst);
}

function ReLoadMru(nDescId) {
    // Remet le bouton standard
    var oImg = top.document.getElementById("mru_search_btn_" + nDescId);    // bouton recherche
    removeClass(oImg, "icon-edn-cross");
    removeClass(oImg, "srchFldImgCancel");
    removeClass(oImg, "srchFldImgNoResult");
    addClass(oImg, "icon-magnifier");
    addClass(oImg, "srchFldImg");

    // Vide le input
    var oInpt = top.document.getElementById("mru_search_" + nDescId);    // input recherche
    oInpt.value = "";

    // Load le mru
    LoadMru(nDescId);
}

// Charge tous les MRUs
function LoadAllMru() {
    var oeParam = top.document.getElementById('eParam').contentWindow;

    // Charge les listes des MRU sur le menu de chaque onglets
    if (!oeParam || typeof (oeParam) == "undefined" || !oeParam || typeof (oeParam.GetParam) != "function" || !oeParam.GetParam("TabOrder") != "") {
        return false;
    }

    var tabOrder = oeParam.GetParam("TabOrder");
    var arrayTabOrder = tabOrder.split(';');

    // Charge les MRUs
    for (i = 0; i < arrayTabOrder.length; i++) {
        var tabDescId = arrayTabOrder[i];
        LoadMru(tabDescId);
    }

    return true;
}

///Charge les vues d'onglet dans le menu (+)
function LoadViewTab() {

    var oeParam = top.document.getElementById('eParam').contentWindow;

    // Charge les listes des MRU sur le menu de chaque onglets
    if (!oeParam || typeof (oeParam) == "undefined" || !oeParam || typeof (oeParam.GetParam) != "function" || !oeParam.GetParam("ViewTab") != "")
        return false;

    var viewTab = oeParam.GetParam("ViewTab");
    var arrayViewTab = viewTab.split('$|$');

    elem = top.document.getElementById("ul_viewtab");
    if (!elem)
        return;

    // Je met en commentaire le premier séparateur qui ne sert à rien (NBA) 02-03-12 ==> à supprimer
    /*    if (arrayViewTab.length > 0) {
    elem.appendChild(CreateMenuSep());

    }*/

    for (var nCmptI = 0; nCmptI < arrayViewTab.length; nCmptI++) {
        var viewTabInfo = arrayViewTab[nCmptI];
        var arrayInfo = viewTabInfo.split('$;$');

//ELAIZ - changement de la longueur à vérifier car un nouvel élement à été rajouté dans ce param dans eParam.cs
        if (arrayInfo.length == 4) {
            oLink = CreateMenuView(arrayInfo[0], arrayInfo[1], (arrayInfo[2] == "1") ? true : false);
            elem.appendChild(oLink);
        }
    }
}


///Recharge les mru de la nav bar sans reload (depuis les param)
function ReloadNavBarMRU() {
    top.LoadAllMru();
    top.LoadViewTab();
}


// Vide la liste de l'onglet passé en param
function ResetLst(nTab) {

    var oLst = top.document.getElementById("ul_mru_" + nTab);
    if (oLst) {
        //Vide la liste en cours
        while (oLst.firstChild) {
            oLst.removeChild(oLst.firstChild);
        };
        return true;
    }
    return false;
}


function focusSearch(nTab, evt) {

    var objTab = document.getElementById("tab_header_" + nTab); // onglet sélectionné

    showSbMenu(objTab);

    //On ne donne pas le focus sur le champ de saisie en mode tablette car cela provoque l'affichage du clavier virtuel,
    //ce qui décale l'affichage de l'écran et se montre plutôt perturbant
    var bIsTablet = false;
    try {
        if (typeof (isTablet) == 'function')
            bIsTablet = isTablet();
        else if (typeof (top.isTablet) == 'function')
            bIsTablet = top.isTablet();
    }
    catch (e) {

    }

    if (!bIsTablet) {
        //On protège car IE8 détect un bug si l'on donne le focus à un champ invisible ou en lecture seule.
        try {
            // On supprime la surbrillance des éléments sélectionnés au clavier
            oNavKeyManager.ClearSelection(objTab);

            var oInpt = document.getElementById("mru_search_" + nTab); // Input    
            if (!oInpt)
                oInpt = document.getElementById("mru_fakesearch_" + nTab); // Input factice sur les menus sans MRU/recherche
            if (oInpt) {
                focusOrSelect(oInpt, evt);
            }
        }
        catch (e) {
        }
    }
}

// Demande #27 576 - Lorsque la souris ne se trouve plus sur un onglet, on retire explicitement le focus sur le champ de recherche
// afin que le curseur ne reste pas pbloqué dans le champ de recherche sur certains navigateurs (IE notamment)
function unFocusSearch(nTab) {
    var objTab = document.getElementById("tab_header_" + nTab); // onglet sélectionné

    if (!isTablet()) {
        //On protège car IE8 détect un bug si l'on donne le focus à un champ invisible ou en lecture seule.
        try {
            // On supprime la surbrillance des éléments sélectionnés au clavier
            oNavKeyManager.ClearSelection(objTab);
            var oInpt = document.getElementById("mru_search_" + nTab); // Input    
            if (oInpt && oInpt.blur) {
                oInpt.blur();
            }
        }
        catch (e) {
        }
    }
}

// #62 037 (reprise de la #27 576) - Selon le type d'évènement déclencheur, on sélectionne le texte ou on met le focus sur la zone de recherche de la MRU
// - si on a effectué un clic sur la zone de recherche (déclenchant focusSearch) -> on sélectionne l'intégralité du texte (pour permettre la saisie rapide d'une nouvelle valeur)
// - si on a effectué un switch d'onglet rapide, notamment sur tablettes (MoveTab) ou un survol du champ de recherche, on met simplement le focus sans toucher à la sélection existante
// => On évite ainsi de resélectionner arbitrairement tout le texte au survol de plusieurs onglets alors qu'une saisie/recherche était en cours sur un onglet.
// Ce qui permet de poursuivre la recherche lorsqu'on revient sur l'onglet (évitant la problématique décrite sur la demande #62 037) tout en permettant un effacement
// rapide de la recherche saisie si l'utilisateur fait le choix de cliquer dans la zone de recherche (but initial de la sélection de l'intégralité du texte, à priori)
function focusOrSelect(targetElement, evt) {
    if (targetElement) {
        if (evt && evt.type == "doubleclick")
            targetElement.select();
        else
            targetElement.focus();
    }
}


// Création d'un bloque MenuMru pour une valeur
function CreateMenuMru(id, txt, fct, nMiniFileTab) {
    // Création du libellé
    var oTx = top.window.document.createTextNode(txt);

    // Création de l'élément DIV
    var oSpan = top.window.document.createElement("div");
    oSpan.appendChild(oTx);

    // Création de l'élément LI
    var oLi = top.window.document.createElement("li");
    oLi.className = "navLst";
    oLi.setAttribute('id', id);
    oLi.setAttribute('title', txt);

    if (typeof (fct) == "function")
        oLi.onclick = fct;

    // Affichage de la mini-fiche/vCard
    if (typeof nMiniFileTab === "undefined")
        nMiniFileTab = 0;
    if (nMiniFileTab > 0) {
        oLi.setAttribute("vcMiniFileTab", nMiniFileTab);
        oLi.setAttribute("title", ""); // Suppression du tooltip natif si on doit afficher la mini-fiche au survol
        oLi.onmouseenter = function () { top.shvc(this, 1); };
        oLi.onmouseleave = function () { top.shvc(this, 0); };
    }

    oLi.appendChild(oSpan);

    return oLi;
}


//Ajoute une action clickable
function CreateMenuAction(txt, action, _class) {

    // Création du libellé
    var oTx = document.createTextNode(txt);

    // Création de l'élément DIV
    var oDiv = document.createElement("div");
    oDiv.appendChild(oTx);

    // Création de l'élément LI
    var oLi = document.createElement("li");
    if ((_class) && (_class != ''))
        oLi.className = _class;
    else
        oLi.className = "navAction";

    oLi.setAttribute('title', txt);

    if (typeof (action) == "function")
        oLi.onclick = action;

    oLi.appendChild(oDiv);

    return oLi;

}

// NBA
//Ajoute une action clickable pour débute par :" et "contient :"
function CreateMenuActionFilter(txt1, _bool, txtSearch, action) {

    // Le trigramme de recherche
    var oTxtSearch = document.createTextNode(txtSearch);

    // Libellé débute par et contient   
    var oTxtlibellé = document.createTextNode(txt1);
    var oTxtGuillemet = document.createTextNode("\"");
    //  var oTxtContient = document.createTextNode(" Contient \""); //Todo Getres
    var pitchSpan = document.createElement("span");
    pitchSpan.className = "text-Result";
    pitchSpan.appendChild(oTxtSearch);


    // Création de l'élément DIV
    var oDiv = document.createElement("div");
    oDiv.appendChild(oTxtlibellé);
    oDiv.appendChild(pitchSpan);
    oDiv.appendChild(oTxtGuillemet);

    // Création de l'élément LI
    var oLi = document.createElement("li");

    if (_bool == true)
        oLi.className = "navLstClair borderTop";
    else
        oLi.className = "navLstClair";

    oLi.setAttribute('title', (txt1 + txtSearch + "\""));

    if (typeof (action) == "function")
        oLi.onclick = action;

    oLi.appendChild(oDiv);

    return oLi;

}


function capitatlize(stW) {
    stW += "";
    if (stW.length > 2) {
        return stW.charAt(0).toUpperCase() + stW.substr(1);
    }
    return stW.toUpperCase();
}

function CreateMenuSep() {

    var oLi = document.createElement("li");
    oLi.className = "navSep";
    return oLi;
}



// Création d'un bloque Selection d'onglet pour une valeur
function CreateMenuView(id, txt, selected) {
    // Création du libellé
    var oTx = top.window.document.createTextNode(txt);

    // Création de l'élément SPAN
    var oSpan = top.window.document.createElement("span");
    oSpan.appendChild(oTx);

    // Création de l'élément LI
    var oLi = top.window.document.createElement("li");
    oLi.className = "navLst";
    oLi.setAttribute('id', id);
    oLi.setAttribute('title', txt);

    if (selected)
        oLi.className += ' top-a-sel';

    //oLi.onclick = function () { top.changeView(this.id) };

    // US #1330 - Tâches #2748, #2750 - On reparamètre les variables utilisées par eParamIFrame.eParamOnLoad() pour recharger le TabID, FileID, type d'affichage actuels après rafraîchissement total via loadTabs()
    // Depuis le correctif de l'US #1330, ces variables reprennent le contexte courant (nGlobalActiveTab, getCurrentView()...) UNIQUEMENT si elles sont undefined, et non plus systématiquement comme avant (bug).
    // Donc, appeler la fonction setParamIFrameReloadContext() ci-dessous sans paramètres va les remettre à undefined, ce qui forcera la fonction loadTabs() à les reparamétrer avec le contexte courant (nGlobalActiveTab, getCurrentView()...)
    oLi.setAttribute('onclick', 'top.nsMain.setParamIFrameReloadContext(undefined, undefined, undefined, undefined, undefined, true); top.changeView(' + id + ')');
    oLi.appendChild(oSpan);

    return oLi;
}



//Met à jour la liste de recherche
function updateNavBarList(oRes, nDescId) {



    var oUlLst = document.getElementById("ul_mru_" + nDescId);      // liste des éléments
    var oImg = document.getElementById("mru_search_btn_" + nDescId);    // bouton recherche
    var oInpt = document.getElementById("mru_search_" + nDescId); // Input

    if (oUlLst && oInpt && oRes && oRes.nodeType == 9) {

        //Vide la liste en cours
        ResetLst(nDescId);

        var oRows = oRes.getElementsByTagName("row");

        var sSearch = oInpt.value.toUpperCase();


        if (oRows.length > 0) {

            var oeParam = top.document.getElementById('eParam').contentWindow;
            var nMiniFileTab = (oeParam.GetParam("MiniFileEnabled_" + nDescId) == "1") ? nDescId : 0;

            // Affichage de la valeure recherchée 
            for (var idxNode = 0; idxNode < oRows.length; idxNode++) {

                var oCurrRow = oRows[idxNode];

                //Champ principal
                var oFld = oCurrRow.getElementsByTagName("field")[0];

                var txt = getXmlTextNode(oFld);
                var id = oFld.getAttribute("fileid");
                var nMainDescId = oFld.getAttribute("descid");

                fct = (function (Id) { return function () { top.loadFile(nDescId, Id); } })(id);
                var oLi = CreateMenuMru(id, txt, fct, nMiniFileTab);



                oUlLst.appendChild(oLi);

                //change le bouton en x annuler la recherche avec croix grise
                removeClass(oImg, "icon-magnifier");
                //removeClass(oImg, "srchFldImgNoResult"); // QBO - BUG 95 629
                removeClass(oImg, "srchFldImg");
                addClass(oImg, "icon-edn-cross");
                addClass(oImg, "srchFldImgCancel");
            }

            // On ajoute les "..." si il y a plus de 7 resultats
            if (oRows.length >= 7) {

                var oLi = CreateMenuAction("...", function () {
                    var sValue = "type=20;$;tab=" + nDescId + ";$;value=" + sSearch;
                    updateUserValue(sValue, function () { goTabList(nDescId, true); }, function () { }); // Aucune action



                }, 'navLst');


                oUlLst.appendChild(oLi);
            }

            //   oUlLst.appendChild(CreateMenuSep());

            if (sSearch.length > 0) {

                //débute par
                var oLi = CreateMenuActionFilter(" " + capitatlize(top._res_2006) + " \"", true, sSearch, function () {
                    var updatePref = "tab=" + nDescId + ";$;filterExpress=" + nMainDescId + ";|;6;|;" + sSearch;
                    updateUserPref(updatePref, function () { goTabList(nDescId, true); });
                }

                );
                oUlLst.appendChild(oLi);

                //Contient
                oLi = CreateMenuActionFilter(" " + capitatlize(top._res_2009) + " \"", false, sSearch, function () {
                    var updatePref = "tab=" + nDescId + ";$;filterExpress=" + nMainDescId + ";|;9;|;" + sSearch;
                    updateUserPref(updatePref, function () { goTabList(nDescId, true); });

                }
                );
                oUlLst.appendChild(oLi);
            }

        }
        else {

            //ajoute un node - pas de résultat -
            var oLi = CreateMenuMru(0, top._res_78);
            oLi.className = "navInfo";
            oUlLst.appendChild(oLi);
            if (sSearch.length > 0) {


                oLi = CreateMenuActionFilter(" " + capitatlize(top._res_2009) + " \"", false, sSearch, function () {
                    //TODO : recuperer le mainfield depuis eQueryManager : ajouter sur le retour vide le main field
                    var updatePref = "tab=" + nDescId + ";$;filterExpress=" + (nDescId + 1) + ";|;9;|;" + sSearch;
                    updateUserPref(updatePref, function () { goTabList(nDescId, true); });

                }
                );

                oUlLst.appendChild(oLi);


            }
            //    oUlLst.appendChild(CreateMenuSep());


            //change le bouton en x annuler la recherche avec croix rouge
            removeClass(oImg, "icon-magnifier");
            removeClass(oImg, "srchFldImgCancel");
            removeClass(oImg, "srchFldImg");
            addClass(oImg, "icon-edn-cross");
            addClass(oImg, "srchFldImgNoResult");
        }




        //Ajoute l'action "annuler la recherche"
        oImg.onclick = function () { ReLoadMru(nDescId) };
    }

    // Ajustement de la position du menu s'il se trouve en-dehors de l'écran
    adjustSbMenuPosition(oUlLst);
}



// lance la recherche
function startSearchNavBar(sSearch, nTab, bEnter) {



    if (typeof (sSearch) === "string" && (sSearch.length > 2 || (bEnter && sSearch.length > 0))) {

        var oeParam = top.document.getElementById('eParam').contentWindow;
        var nMiniFileTab = (oeParam.GetParam("MiniFileEnabled_" + nTab) == "1") ? nTab : 0;

        // Affiche recherche en cours
        // TODO RESSOURCES // res  642 
        ResetLst(nTab);
        var oLi = CreateMenuMru(0, "Recherche en cours...", null, nMiniFileTab);
        adjustSbMenuPosition(document.getElementById("ul_mru_" + nTab));
        oLi.className = "navInfo";
        var oUlLst = document.getElementById("ul_mru_" + nTab);
        oUlLst.appendChild(oLi);

        // oUlLst.appendChild(CreateMenuSep());

        var oListUpdater = new eUpdater("mgr/eQueryManager.ashx", 0);
        oListUpdater.ErrorCallBack = function () { };
        oListUpdater.addParam("tab", nTab, "post");
        oListUpdater.addParam("rows", 7, "post"); //Nombre maximum de résultats de recherche dans la navbar 
        oListUpdater.addParam("type", 1, "post");
        oListUpdater.addParam("search", sSearch, "post");
        oListUpdater.addParam("multiword", "1", "post");
        oListUpdater.send(updateNavBarList, nTab);

    }
}



//Lance la recherche
function launchNavBarSearch(sSearch, e, nDescId) {

    if (e == null)
        return;

    if (!isTablet() && !e.keyCode)
        return;

    var bEnter = e.keyCode == 13;

    // Touches considérées comme touches de navigation, ne devant pas provoquer le déclenchement de la recherche
    // On pourrait, ici, insérer toutes les touches qui n'ont pas vocation à modifier le contenu du champ de recherche, et dont l'appui déclenche malgré tout la
    // recherche. Ex : Ctrl, Maj...
    var bIsNavHotkey =
        e.keyCode == 33 || /* Page précédente (PgUp) */
        e.keyCode == 34 || /* Page suivante (PgDn) */
        e.keyCode == 35 || /* Fin (End) */
        e.keyCode == 36 || /* Origine/Accueil (Home) */
        e.keyCode == 37 || /* Gauche */
        e.keyCode == 38 || /* Haut */
        e.keyCode == 39 || /* Droite */
        e.keyCode == 40;   /* Bas */
    // Appui sur une touche de navigation susmentionnée : on ignore. L'évènement devra avoir été géré par un appel à onNavBarKeyDown depuis la div de la navbar
    // Appui sur Entrée dans le champ de recherche, ou saisie d'au moins 2 caractères : on déclenche la recherche

    if (!bIsNavHotkey && typeof (sSearch) === "string" && (sSearch.length > 2 || (bEnter && sSearch.length > 0))) {
        //JAS Si le champ de recherche du mode liste est reseigné, on annule la recherche
        var mainlistSearchinput = document.getElementById("eFSInput");

        if (mainlistSearchinput && mainlistSearchinput.value != "")
            resetFilter();
        clearTimeout(searchNavBarTimer);
        searchNavBarTimer = window.setTimeout(function () { startSearchNavBar(sSearch, nDescId, bEnter) }, 300);
    }
}

// Gestion de la navigation clavier
function onNavBarKeyDown(e, oSourceObj) {
    if (!e || !e.keyCode)
        return;

    if (!oSourceObj)
        oSourceObj = e.target || e.srcElement;

    oNavKeyManager.sourceObj = oSourceObj;

    switch (e.keyCode) {
        case 13: // Entrée
            oNavKeyManager.TriggerActiveElement();
            break;

        case 37: // Flèche Gauche
            oNavKeyManager.MoveTab(0);
            break;

        case 38: // Flèche Haut
            oNavKeyManager.MoveSubMenu(0);
            break;

        case 39: // Flèche Droite
            oNavKeyManager.MoveTab(1);
            break;

        case 40: // Flèche Bas
            oNavKeyManager.MoveSubMenu(1);
            break;

        case 27: // Echap
            oNavKeyManager.CloseMenu();
            break;

        default:
            // On empêche toute saisie de caractère dans le champ de recherche factice servant à donner le focus sur les menus sans MRU
            if (oNavKeyManager.sourceObj.id.lastIndexOf("mru_fakesearch_", 0) === 0)
                stopEvent(e);
            break;
    }
}



///Change l'onglet actif
function switchActiveTab(nTab, type) {


    // On créait un object pour renvoyer au setWait ou es ce qu'on veux aller
    var oParamGoTabList = {
        to: type,
        nTab: nTab,
    }

    // Le setWait du switchActiveTab poser problème, il est suffisaement appeler comme ça dans toutes les autres fonction ! demande (92 249)
    // setWait(true, undefined, undefined, isIris(top.getTabFrom()));

    if (typeof (bReloadFileMenu) == "undefined")
        var bReloadFileMenu = true;

    // Restaure l'état original de l'onglet actuellement actif
    var objActiveTab = document.getElementById("tab_header_" + nGlobalActiveTab);
    if (objActiveTab) {

        switchClass(objActiveTab, "navTitleActive", "navTitle")
        if (objActiveTab.parentNode.getAttribute("ednTabHidden") == "1") {
            addClass(objActiveTab.parentNode, "tab-hidden");
        }
        // Puis on agit sur les menus
        for (var x = 0; objActiveTab.parentNode.childNodes[x]; x++) {
            if (objActiveTab.parentNode.childNodes[x].tagName == 'UL') {
                switchClass(objActiveTab.parentNode.childNodes[x], "sbMenuActive", "sbMenu");
            }
        }
    }

    // Puis passe l'onglet sélectionné en onglet actif
    nGlobalActiveTab = nTab;
    objActiveTab = document.getElementById("tab_header_" + nGlobalActiveTab);

    if (objActiveTab) {

        var nTabPage = Number(objActiveTab.parentNode.getAttribute("ednTabPage"));

        if (nTabPage != nActivePageTabs)
            switchActivePageTab(nTabPage);

        switchClass(objActiveTab, "navTitle", "navTitleActive")
        if (objActiveTab.parentNode.getAttribute("ednTabHidden") == "1") {
            removeClass(objActiveTab.parentNode, "tab-hidden");
        }
        // Puis on agit sur les menus
        for (var x = 0; objActiveTab.parentNode.childNodes[x]; x++) {
            if (objActiveTab.parentNode.childNodes[x].tagName == 'UL') {
                //objActiveTab.parentNode.childNodes[x].className += (objActiveTab.parentNode.childNodes[x].className ? ' ' : '') + 'ul-tab-hidden';
                addClass(objActiveTab.parentNode.childNodes[x], "ul-tab-hidden");
                switchClass(objActiveTab.parentNode.childNodes[x], "sbMenu", "sbMenuActive");
            }
        }

    }
    // Le setWait du switchActiveTab poser problème, il est suffisaement appeler comme ça dans toutes les autres fonction ! demande (92 249)
    // setWait(false, undefined, undefined, isIris(top.getTabFrom()), oParamGoTabList);

}
function showSbMenu(objTab) {
    // Suppression de la classe additionnelle utilisée sur tablettes pour matérialiser la sélection/surbrillance
    if (isTablet()) {
        var oMenuNavBar = document.getElementById('menuNavBar');
        if (oMenuNavBar && oMenuNavBar.childNodes) {
            for (var i = 0; i < oMenuNavBar.childNodes.length; i++) {
                for (var j = 0; j < oMenuNavBar.childNodes[i].childNodes.length; j++) {
                    if (oMenuNavBar.childNodes[i].childNodes[j].tagName == "DIV") {
                        if (oMenuNavBar.childNodes[i].childNodes[j] != objTab)
                            removeClass(oMenuNavBar.childNodes[i].childNodes[j], "navTitleTabletFocused");
                        else
                            addClass(oMenuNavBar.childNodes[i].childNodes[j], "navTitleTabletFocused");
                    }
                }
            }
        }
    }
    for (var x = 0; objTab.parentNode.childNodes[x]; x++) {
        if (objTab.parentNode.childNodes[x].tagName == 'UL') {
            //objTab.parentNode.childNodes[x].className += (objTab.parentNode.childNodes[x].className ? ' ' : '') + 'ul-tab-hidden';
            removeClass(objTab.parentNode.childNodes[x], "ul-tab-hidden");
        }
    }

    // Ajustement de la position du menu s'il se trouve en-dehors de l'écran
    adjustSbMenuPosition(objTab.parentNode);
}

// Repositionne le menu avec alignement à droite s'il est affiché hors écran, et vice-versa
function adjustSbMenuPosition(oUlOrTab) {
    if (!oUlOrTab)
        return;

    try {
        // Récupération de l'objet DOM correspondant au sous-menu
        var oMenu = null;
        // Cas 1 : l'objet passé en paramètre est le menu lui-même
        if (oUlOrTab.className.indexOf("sbMenu") > -1)
            oMenu = oUlOrTab;
        // Cas 2 : l'objet passé en paramètre est un sous-élément de menu non actif (ex : une valeur de MRU)
        if (!oMenu)
            oMenu = eTools.FindClosestElementWithClass(oUlOrTab, "sbMenu");
        // Cas 3 : l'objet passé en paramètre est un sous-élément de menu actif (ex : une valeur de MRU)
        if (!oMenu)
            oMenu = eTools.FindClosestElementWithClass(oUlOrTab, "sbMenuActive");
        // Cas 4 : l'objet passé en paramètre est un parent du menu
        if (!oMenu)
            oMenu = oUlOrTab.querySelector(".sbMenu");

        // Puis, si identifié, on détecte si son bord droit se situe en-dehors de la zone visible, auquel cas, on ajoute le style approprié
        // après l'avoir d'abord retiré pour réinitialiser la vérification (cas où le menu aurait été précédemment repositionné à droite à cause d'une liste de
        // résultats trop longue qui n'est plus d'actualité)
        // Le calcul de la zone visible se fait en tenant compte de la taille du menu droit, sauf sur l'accueil où il est toujours escamotable
        if (oMenu) {
            oMenu.style.left = '0px';
            oMenu.style.width = 'auto';
            removeClass(oMenu, "sbMenuD");
            var docWidth = getDocWidth();
            if (nGlobalActiveTab > 0)
                docWidth -= GetRightMenuWidth();
            var parentTabLiWidth = oMenu.parentNode.getBoundingClientRect().width;
            var correctedLeft = 0;
            if (oMenu.getBoundingClientRect().right > docWidth) {
                addClass(oMenu, "sbMenuD");
                correctedLeft = oMenu.getBoundingClientRect().width - parentTabLiWidth;
                oMenu.style.left = '-' + correctedLeft + 'px';
            }
            // Si, après correction, le menu dépasse de nouveau (à gauche, cette fois), on le retaille, quitte à tronquer son contenu, avec une marge pour ne 
            // pas le coller à l'écran
            var newLeft = oMenu.getBoundingClientRect().left;
            var newWidth = oMenu.getBoundingClientRect().width;
            var margin = 30;
            if (newLeft < 0) {
                var leftOffset = Math.abs(newLeft);
                oMenu.style.width = Math.floor(newWidth - leftOffset - margin) + 'px';
                oMenu.style.left = '-' + (oMenu.getBoundingClientRect().width - parentTabLiWidth) + 'px';
            }
        }
    }
    catch (ex) { }
}

//Change la page des onglets
function switchActivePageTab(nTabPage) {
    // Le bouton Accueil est sur la page "0", il ne faut donc pas switcher dessus, sinon, tous les autres onglets se masquent
    // Ce bouton Accueil est désormais toujours visible, il n'y a donc plus lieu de switcher pour le rendre accessible
    if (nTabPage == 0) {
        if (isTablet() && isTabletNavBarEnabled())
            nTabPage = 1;
        else
            return;
    }

    nActivePageTabs = nTabPage;

    var oNavPage = document.getElementById("menuNavBar");
    var oNavTabPage = document.getElementById("navigTab");
    var oListFile = oNavPage.getElementsByTagName("LI");
    var oListImg = oNavTabPage.getElementsByTagName("SPAN");

    for (var iIg = 0; iIg < oListImg.length; iIg++) {
        var oElem = oListImg[iIg];

        if (oElem.id == ("switch" + nTabPage))
            oElem.className = 'icon-circle imgAct';
        else
            oElem.className = 'icon-circle-thin imgInact';
    }

    var bSwitched = false;
    for (var iLi = 0; iLi < oListFile.length; iLi++) {

        var oElem = oListFile[iLi];
        var nPage = Number(oElem.getAttribute("ednTabPage"));

        var bTabletNavBarEnabled = isTablet() && isTabletNavBarEnabled();

        if (nPage == 0 && bTabletNavBarEnabled)
            nPage = 1;

        if (nPage > 0) {
            if (bTabletNavBarEnabled) {
                // On affiche le + "natif" le temps de la transition
                // Il sera masqué par la fonction tabletAfterNavBarTransition
                var oExistingPlusDiv = document.getElementById('navPlusLi');
                if (oExistingPlusDiv)
                    oExistingPlusDiv.style.visibility = 'visible';
                // On masque le + "spécial tablettes" le temps de la transition
                // Il sera réaffiché par la fonction tabletAfterNavBarTransition
                var oPlusDiv = document.getElementById('navPlus');
                if (oPlusDiv)
                    oPlusDiv.style.visibility = 'hidden';
                oElem.style.visibility = 'visible';
                oNavPage.addEventListener('webkitTransitionEnd', tabletAfterNavBarTransition);
                var oHomeWidth = getNumber(document.getElementById('navHomeLi').clientWidth);
                if (!bSwitched && nPage == nTabPage) {
                    if (nPage > 1) {
                        var currentMarginLeft = getNumber(oNavPage.style.marginLeft);
                        if (isNaN(currentMarginLeft))
                            currentMarginLeft = oHomeWidth;
                        oNavPage.style.marginLeft = '' + getNumber(currentMarginLeft - getNumber(oElem.offsetLeft) + oHomeWidth) + 'px';
                        bSwitched = true;
                    }
                    else {
                        oNavPage.style.marginLeft = oHomeWidth + 'px';
                        bSwitched = true;
                    }
                }
            }
            if (nPage != nTabPage)
                addClass(oElem, "tab-hidden");
            else
                removeClass(oElem, "tab-hidden");
        }
    }
}

function tabletAfterNavBarTransition() {
    if (isTablet() && isTabletNavBarEnabled()) {
        var oNavPage = document.getElementById('menuNavBar');
        var oListFile = oNavPage.getElementsByTagName("LI");

        var oNewHomeLi = document.getElementById('navHomeLi');
        var oExistingHomeLi = document.getElementById('navOldHomeLi');
        if (oExistingHomeLi) {
            oNewHomeLi.innerHTML = oExistingHomeLi.innerHTML;
            oNavPage.removeChild(oExistingHomeLi);
        }

        var oNewPlusDiv = document.getElementById('navPlus');
        if (oNewPlusDiv) {
            oExistingPlusDiv = document.getElementById('navPlusLi');
            if (oExistingPlusDiv.offsetLeft + oExistingPlusDiv.clientWidth < oNewPlusDiv.offsetLeft) {
                oExistingPlusDiv.style.visibility = 'visible';
                oNewPlusDiv.style.visibility = 'hidden';
            }
            else {
                document.getElementById('navSecPlusLi').innerHTML = document.getElementById('navPlusLi').innerHTML;
                oExistingPlusDiv.style.visibility = 'hidden';
                oNewPlusDiv.style.visibility = 'visible';
            }
        }

        // Sur tablettes, si la navbar optimisée est activée, on redimensionne le "+" de façon à ce qu'il se cale au menu droit
        refreshTabletNavBarWidth();

        // On masque le dernier onglet avant le "+" supplémentaires car il y a de fortes chances que son affichage soit coupé
        /*
        for (var iLi = 0; iLi < oListFile.length; iLi++) {

            var oElem = oListFile[iLi];
            var nPage = oElem.getAttribute("ednTabPage");

            if (nPage > 0) {
                var oPlusDiv = document.getElementById('navPlus');
                var nNbPages = Number(oNavPage.getAttribute("nbtab"));
                if (oPlusDiv && oElem.offsetLeft + oElem.clientWidth > oPlusDiv.offsetLeft) {
                    oElem.style.visibility = 'hidden';
                    iLi = oListFile.length; // sortie de la boucle
                }
            }
        }
        */
    }
}

//var tabWidgetCallStack = top.tabWidgetCallStack || { tabs: [0] }


// Fonction appelée lors du clic ou de l'appui (tablette) sur le titre d'un onglet
// Sur PC, déclenche goTabList (affichage de la liste de l'onglet sélectionné)
// Sur tablettes, ne fait rien (laisse le navigateur gérer pour faire apparaître le menu)
// Il est nécessaire, même sur tablettes, de laisser un véritable évènement onClick afin que le navigateur
// sache que l'élément est cliquable/focusable et simule le "onMouseOver" pour, dans notre cas, afficher le menu
function onTabSelect(nTab) {

 

    if (top.oGridManager) {
        top.oGridManager.addTabToStack(nTab);
        top.oGridManager.hideGrids();
    }

    // #36771 Si on a recement consulté une fiche , on regarde dans l'historique de navigation -- pas pour les tablettes
    // si on clique sur l'onglet de la fiche ouverte on passe en mode liste
    if (oNavManager.HasRecent('File', nTab) && oNavManager.IsTabSwitching(nTab) && !isTablet()) {
        top.loadFile(nTab, oNavManager.GetRecentId('File', nTab), 2);
        return;
    }

    if (nGlobalActiveTab != nTab && (nTab == 0 || nGlobalActiveTab == 0)) {
        top.bResizeNavBar = 1;

    }

    //Mou 24/06/2013 demande xrm cf.23614
    var oCurrentWinSize = getWindowSize();

    var bIsFirstTime = (oWinSizeMem._nWidth == -1 && oWinSizeMem._nHeight == -1);
    if (bIsFirstTime) {
        //chargement de la page pour la premiere fois
        oWinSizeMem._nWidth = oCurrentWinSize.w;
        oWinSizeMem._nHeight = oCurrentWinSize.h;


    }
    else {

        var bIsWinSizeChanged = oCurrentWinSize.w != oWinSizeMem._nWidth || oCurrentWinSize.h != oWinSizeMem._nHeight;
        if (bIsWinSizeChanged) {

            //rafraichissement de la page entiere ...
            nGlobalActiveTab = nTab;
            // loadNavBar();

            // US #1330 - Tâches #2748, #2750 - On reparamètre les variables utilisées par eParamIFrame.eParamOnLoad() pour recharger le TabID, FileID, type d'affichage actuels après rafraîchissement total ci-dessous
            // Depuis le correctif de l'US #1330, ces variables reprennent le contexte courant (nGlobalActiveTab, getCurrentView()...) UNIQUEMENT si elles sont undefined, et non plus systématiquement comme avant (bug).
            // Donc, appeler la fonction setParamIFrameReloadContext() ci-dessous sans paramètres va les remettre à undefined, ce qui forcera le code ci-dessous à les reparamétrer avec le contexte qui lui convient
            top.nsMain.setParamIFrameReloadContext();

            // TODO - NE PAS RECHARGER LA FRAME PARAM
            // Si FraParam est reload, il faut mettre bIsParamLoaded & bIsIFrameLoaded à 0 pour lancer le LoadParam
            top.bIsParamLoaded = 0;
            top.bIsIFrameLoaded = 0;

            // #23 614, #39 338, #59 724 - Mémorisation de l'onglet utilisé avant rechargement d'eParamIFrame, qui provoque un rechargement de la page
            // Code corrigé et repassé en revue pour la US #1330 et ses tâches liées (#2748, #2750)
            // Ces variables peuvent être mises à jour via un appel à nsMain.setParamIFrameReloadContext(), soit sans paramètres (auquel cas tout est mis à undefined, et donc réajusté ci-dessous), soit avec paramètres
            if (typeof (top.tabToLoadAfterParamIFrame) == "undefined" && typeof (top.nGlobalActiveTab) != "undefined")
                top.tabToLoadAfterParamIFrame = top.nGlobalActiveTab;
            // Cette fonction onTabSelect() est appelée uniquement pour afficher le mode Liste (clic sur un onglet). On ignore donc la vue et le fichier actuellement affichés, d'où l'écrasement systématique
            // des variables ci-dessous, et la mise en commentaire du test. A réajuster si un autre besoin se fait ressentir
            //if (typeof (top.viewToLoadAfterParamIFrame) == "undefined")
                top.viewToLoadAfterParamIFrame = "LIST";
            //if (typeof (top.fileToLoadAfterParamIFrame) == "undefined")
                top.fileToLoadAfterParamIFrame = "0";
            // Ces variables sont mises à zéro dans tous les cas, car on ne recharge pas le contexte éventuellement utilisé à l'ouverture de session si on est venu d'eGotoFile.aspx
            //if (typeof (top.isTplMailToLoadAfterParamIFrame) == "undefined")
                top.isTplMailToLoadAfterParamIFrame = "0";
            //if (typeof (top.loadFileInPopupAfterParamIFrame) == "undefined")
                top.loadFileInPopupAfterParamIFrame = "0";
            top.document.getElementById('eParam').contentWindow.location.reload(true);

            //On sauvegarde les nouvelles dimensions de la fenetre
            oWinSizeMem._nWidth = oCurrentWinSize.w;
            oWinSizeMem._nHeight = oCurrentWinSize.h;

            /*

            */

        }
    }
    if (isTablet()) {
        if (nTab == 0) {
            goTabList(0);
        }
        else {
            // oActionMenuMouseOver();
            return;
        }
    }
    else {
        goTabList(nTab);
    }
    // La navbar tient compte du menu escamotable, affiché ou pas, du coup
    // loadNavBar() est executé une fois la liste/fiche est completement charge
}


///Charge le menu/navabar d'amin
function loadEdA(nTab) {


}

//Charge les web tab, si grille defini en premier on charge la grille sinon chargement de la liste

// Commenté suite au double rafraichissement et désynchronisation au niveau de chargement de de menu et la barre de navigation
/*
function loadTab(nTab) {
    var upd = new eUpdater("eda/mgr/eWebTabManager.ashx", 1);
    upd.addParam("action", 8, "post"); //GETSUBTAB
    upd.addParam("tab", nTab, "post");
    var ednType = getAttributeValue(document.getElementById("fileDiv_" + nTab), "ednType")
    if (ednType)
        upd.addParam("ednType", ednType, "post");
    var myFunct = (function (nTab) {
        return function (oRes) {
            var callBack = function () {
                if (oRes) { //Si retour on charge les sous onglets et on charge la grille ou la liste en fonction de la selection.
                    updateContent(oRes, nTab);
                    var itemSelected = document.getElementById('firstSubTabItem');
                    if (itemSelected) {
                        var isList = getAttributeValue(itemSelected, "list");
                        if (isList && isList == 1) {
                            onTabSelect(nTab);
                        } else {
                            var gridId = getAttributeValue(itemSelected, "gid");
                            oWebMgr.loadSpec(nTab, 1); //1 = GRID
                        }
                    } else {
                        onTabSelect(nTab);
                    }
                } else { //Si pas de retour => Pas de sous onglet, on charge la liste
                    onTabSelect(nTab);
                }


            }
            //scritp JS
            addScript("eButtons", "LIST");
            addScript("ePopup", "LIST",
            function () {
                addScript("eFieldEditor", "LIST");
                addScript("eGrapesJSEditor", "LIST");
                addScript("eMemoEditor", "LIST");
                addScript("eMarkedFile", "LIST", callBack);
            });
        }
    })(nTab);

    upd.send(myFunct);

    clearHeader("LIST");
    addCss("eActions", "LIST");



}

*/


/* #53413 : indique si on doit rediriger vers le précédent mode Liste affiché lors de l'ouverture d'une fiche, même si elle est issue d'un autre
onglet, ou s'il faut rediriger vers le mode Liste correspondant à la fiche en question
Exemple 1 : je suis sur la liste Contacts, j’affiche un sous-onglet/grille comportant un mode Liste Sociétés, et je clique sur une fiche Sociétés.
Exemple 2 : je suis sur la liste Contacts, je déroule le menu de Sociétés, et je clique sur une fiche Société
Dans ces 2 cas, si ce mode est activé, cliquer sur "Mode Liste" à droite depuis la fiche Sociétés redirigera vers le dernier mode Liste affiché (Contacts)
et non vers Sociétés.
Le retour de cette fonction est passé par eFileMenuManager à la fonction goTabList() ci-dessous pour savoir si elle doit rediriger vers un mode Liste situé sur un
autre onglet ou non.
Ce comportement pouvant être déroutant, il a été décidé de ne pas l'activer pour l'instant.
L'utilisation d'une fonction pourra permettre, à terme, de l'activer sous conditions (pour certains onglets ou certains cas) si souhaité.
*/
function goTabListFromDifferentTab(nTab) {
    return false;
}


/// Passe en mode liste
function goTabList(nTab, bReload, callbackFct, bUseHistory, bForceList) {

    // On crée un object pour indiquer au setWait où on souhaite aller
    // US #4261 - TK #6962 - Skeleton nécessaire pour le passage Admin > Utilisation mode Liste
    // Mais non désiré si on affiche la même liste qu'actuellement
    var oParamGoTabList = null;
    /** ELAIZ - Demande 95 034  - Dans le cas où l'on arrive la 1ère fois sur la page d'acceuil 
     * nTab et nGlobalActiveTab sont à 0 alors qu'on ne recharge pas la même liste, 
     * du coup dans ce cas on remplit oParamGoTabList  */
    var bReloadsSameList = (nTab == nGlobalActiveTab && nTab != top.TAB_HOME);
    if (!bReloadsSameList) {
        oParamGoTabList = {
            to: 1,
            nTab: nTab,
            context: "eNavBar.goTabList"
        }
    }

    if (top.oNewFileMainVueJSApp)
        top.oNewFileMainVueJSApp.destroyVue();

    /****** La visu de l'admi *****/
    if (top.irisHomeAdminVue)
        top.irisHomeAdminVue.$destroy();

    var oMainDiv = document.getElementById("app-menu");

    if (!oMainDiv) {
        oMainDiv = document.createElement("div");
        oMainDiv.id = "app-menu";
    }

    oMainDiv.innerHTML = "";

    /** ELAIZ - Demande 95 034  - Si oParamGoTabList  existe, on vérifie qu'il va 
     * sur la page d'accueil et si la popup marketing est active pour empêcher de faire le clearHeader
     * du css de la popup Marketing dans ce cas (EDNHOME) */
    var blsHomepage = !!oParamGoTabList && oParamGoTabList?.nTab == top.TAB_HOME && !getCookie('ednstopnews')

    if(!blsHomepage)
        clearHeader("EDNHOME", "CSS");
    /****** La visu de l'admin *****/

    setWait(true, undefined, undefined, isIris(top.getTabFrom()), oParamGoTabList);

    // #56965 - Permet de remettre le thème du user dans le cas où cette fonction est appelée depuis l'admin
    if (top.nsAdmin && top.nsAdmin.AdminMode) {
        top.nsAdmin.goTabList(nTab);
        return;
    }

    // #53413 (similaire à #36771) - Lors de l'appel à la fonction depuis le bouton "Mode Liste" du bandeau droit (eFileMenuManager),
    // Si on a récemment consulté une liste, on regarde dans l'historique de navigation
    if (bUseHistory && oNavManager.HasRecent('List', nTab)) {
        var nRecentTab = oNavManager.GetRecentId('List', nTab);
        // Passage de bReload à true, car on ne sait pas depuis quel contexte doit être rechargé le TabId cible. On ne peut pas s'assurer à 100%
        // que le rechargement se fera toujours depuis un mode Liste déjà existant sur la page.
        // Passage de bUseHistory à false : on ne fait qu'une seule utilisation de l'historique pour éviter les boucles infinies.
        // On pourrait aussi faire un appel à oNavManager.ResetMode('List', nRecentTab) pour effacer l'historique de l'onglet concerné
        goTabList(nRecentTab, true, callbackFct, false, bForceList);
        return;
    }


    if (Number(nTab) == -1) {
        //Charge le contenu de l'admin (page principale)
        nsAdmin.loadAdminModule(USROPT_MODULE_ADMIN);
        return;
    }

    if (nGlobalActiveTab != nTab && (nTab == 0 || nGlobalActiveTab == 0)) {
        top.bResizeNavBar = 1;
    }

    if (isTablet()) {
        switchActiveTab(nTab);
        // objActiveTab = document.getElementById('nvb' + nGlobalActiveTab);
        var objActiveTab = document.getElementById("tab_header_" + nGlobalActiveTab);
        if (objActiveTab) {
            var moActiveMenu = document.querySelector("ul.sbMenuActive");
            switchClass(moActiveMenu, "sbMenuActive", "navBar");

        }

    }


    //////TEST SI ONGLET Affiché avant redirection
    if (nTab > 0) {
        //#36771 On remet à zéro la navigation vers le mode fiche
        oNavManager.ResetMode('File', nTab);

        if (!isTabDisplayed(nTab)) {
            setWait(false, undefined, undefined, isIris(top.getTabFrom()));

            // 01/09/15 : Demande 40 869
            var eParamWindow = getParamWindow();
            if (eParamWindow != null && eParamWindow != undefined) {

                var tabName = eParamWindow.document.getElementById("TAB_MRU_" + nTab);
                if (!tabName) {
                    nsMain.switchToHiddenTabUsingView(nTab, 0, "LIST");
                    return false;
                }
            }
        }
    }


    oEvent.fire("mode-list", { tab: nTab });

    ////////////////////////////////
    var oMainDiv = document.getElementById("mainDiv");
    if (oMainDiv != null) {
        var tabMainDivWH = GetMainDivWH();
        var height = tabMainDivWH[1];
        var width = tabMainDivWH[0];
        if (height > 0) {
            oMainDiv.style.height = height + "px";  //On force le dimensionnement de la div principale
            oMainDiv.style.width = width + "px";  //On force le dimensionnement de la div principale
        }
    }


    //Change l'onglet actif
    // nTab : Onglet
    // Type de page (1 liste)
    switchActiveTab(nTab,1);



    //Mode liste
    if (Number(nTab) > 0) {

        //Charge le menu fichier de droite
        loadFileMenu(nTab, 1, 0);
        activateCSSIrisFile("FICHEIRIS", true);
        clearHeader("FICHEIRIS", "CSS");

        // On récupère l'id de la grille en première position sauf si on force le mode liste, dans ce cas nGrid = 0 
        var nGrid = bForceList ? 0 : getParamWindowKey('FirstSubTab_' + nTab, 0) * 1;

        // Si la grille est en première position on charge la grille 
        if (nGrid > 0) {
            oGridManager.loadGrid(nTab, nGrid);
        } else {

            //Nouveau mode liste
            if (isIris(nTab, "dvIrisCrimsonInput")) {
                setWait(false);

                var loc = window.location.pathname;
                var dir = loc.substring(0, loc.lastIndexOf('/'));

                addScript("../IRISBlack/Front/Scripts/eInitNewErgo", "LIST", function () {
                    LoadIrisList(nTab, dir + "/IRISBlack/Front/Scripts/");
                });

                return;
            }

            // Chargement du mode liste     
            var nPage = getParamWindowKey('Page_' + nTab, 1);
            loadList(nPage, bReload);
        }
    }
    // Mode Accueil ou Admin
    else {
        var CallBack = null;
        var nType = 0;
        if (Number(nTab) == -2) {
            //Charge le contenu des options utilisateur (page principale)
            CallBack = function () { loadUserOption(USROPT_MODULE_MAIN); };
            nType = 7; // ADMIN
        }
        else if (Number(nTab) == -1) {
            //Charge le contenu de l'admin (page principale)
            CallBack = function () { nsAdmin.loadAdminModule(USROPT_MODULE_ADMIN); };
            nType = 7; // ADMIN
        }
        else {
            //Charge le contenu de la homepage
            CallBack = function () {
                try {

                    loadHomePageXrm();

                }
                catch (ex) {
                    // L'accès n'est pas bloqué
                    setWait(false, undefined, undefined, isIris(top.getTabFrom()));

                    oEvent.fire("log-error", { 'message': "Impossible de charger la page d'accueil", 'exception': ex });
                }

            };
        }

        //Charge le menu fichier de droite
        loadFileMenu(nTab, nType, 0, CallBack);
    }

    //HDJ Faire disparaitre la div control_contextmenu du coller Planning
    deleteDivPaste();

    if (callbackFct && typeof (callbackFct) == "function") {
        callbackFct();
    }

    setWait(false, undefined, undefined, isIris(top.getTabFrom()), oParamGoTabList);
}

// Récupère la valeur de la clé, si elle n'existe pas retourne la valeur par defaut
function getParamWindowKey(keyName, defaultValue) {
    var value = defaultValue;
    var oeParam = document.getElementById('eParam').contentWindow;
    if (typeof (oeParam) != "undefined" && oeParam && typeof (oeParam.GetParam) == "function" && oeParam.GetParam(keyName) != '')
        value = oeParam.GetParam(keyName);
    return value;
}
// Mise à jour la valeur de la clé de grille
function setGridParamWindowKey(keyName, value) {
    var oeParam = document.getElementById('eParam').contentWindow;
    var gridContainer = oeParam.document.getElementById("GRIDS");
    if (typeof (oeParam) != "undefined" && oeParam && typeof (oeParam.AddOrUpdInput) == "function")
        oeParam.AddOrUpdInput(gridContainer, keyName, value);
}


// S'il existe une page d'accueil xrm définit pour cette utilisateur
// on la charge sinon on charge la page v7
function loadHomePageXrm() {
    addScript("syncFusion/ej.web.all.min", "HOMEPAGE");
    //addScript("syncFusion/ej.responsive", "HOMEPAGE");

    // Page XRM
    if (oGridManager.pageExists()) {

        oGridManager.loadPage();
    } else {
        // Page V7
        loadHomePage();
    }
}

//Page d'accueil V7
function loadHomePage() {

    setWait(true);

    var oHpgUpdater = new eUpdater("mgr/eHomepageManager.ashx", 1);
    // Vide les nodes enfants et vide le contenu du div
    var oMainDiv = document.getElementById("mainDiv");
    var tabMainDivWH = GetMainDivWH();
    var height = tabMainDivWH[1];
    var width = tabMainDivWH[0];
    if (height > 0)
        oMainDiv.style.height = height + "px";  //On force le dimensionnement de la div principale

    oHpgUpdater.ErrorCallBack = function () { setWait(false); };
    oHpgUpdater.addParam("action", "homepage", "post"); //Action demandée = affichage de la page principale
    oHpgUpdater.addParam("divH", height, "post");
    oHpgUpdater.addParam("divW", width, "post");
    oHpgUpdater.addParam("readonly", 0, "post");    //Lecture seule désactivée, elle concerne l'affichage depuis l'admin
    oHpgUpdater.send(setHomePage, null);
}
//Initialisation des contraintes des eudoparts
function InitHomePage() {

    initEudopartList();
    InitDashedBox();
    afterHomepageLoaded();
}

function initHomepageCssScripts() {
    clearHeader("EDITFILE", "CSS"); //Conflit avec les btns de catalogues
    clearHeader("HOMEPAGE", "JS");

    addCss("eFilterWizard", "HOMEPAGE");    //Eudopart : Recherche
    addCss("eList", "HOMEPAGE"); //Eudopart : MRU  et Necessaire pour le Finder aussi

    //BSE :utilisation de syncfusion chart  
    addScript("eCharts", "HOMEPAGE", function () {
        loadSyncFusionChart();
    });
}

function setHomePage(oHtml) {


    var mainDiv = document.getElementById("mainDiv");
    // Initialise les scripts pour les tablettes tactiles
    if (isTablet() && areTabletCustomScrollersEnabled()) {
        mainDiv.innerHTML = '<div id="scroller">' + oHtml + "</div>";
        loadTabletScrollers();
    }
    else {
        mainDiv.innerHTML = oHtml;
        // mainDiv.setAttribute("edntype", "xrmhomepage");
    }

    //Pour tester
    //if (typeof (oXrmHomeGrid) != "undefined") {
    //    oXrmHomeGrid.init();
    //}

    initHomepageCssScripts();
    initHeadEvents();

    // Si on n'est pas dans la nouvelle version des pages d'accueil, on charge 
    var container = document.querySelector(".gw-tab#gw-tab-115200-0");
    if (container == null) {
        InitHomePage();

    }

    setWait(false);
}



//drilldown 
function goNav(idReport, sOtherParam) {

    var oChartParam = document.querySelector("input[ednreportparam='" + idReport + "']");
    if (top.window["_md"]) {
        var oChartModal = top.window["_md"]["myChart_" + idReport];

        if (oChartModal && oChartModal.isModalDialog) {
            if (typeof oChartParam == 'undefined' || oChartParam == null)
                oChartParam = oChartModal.getIframe().document.querySelector("input[ednreportparam='" + idReport + "']");

            oChartModal.hide();
        }

    }

    if (oModalReportList && oModalReportList.hide)
        oModalReportList.hide();
    else if (top && top.oModalReportList && top.oModalReportList.hide)
        top.oModalReportList.hide()



    if (sOtherParam == "") {
        //drill down sur le chart complet
        if (oChartParam) {
            sOtherParam = "tab=" + getAttributeValue(oChartParam, "tab")
                + "&addcurrentfilter=" + (getAttributeValue(oChartParam, "addcurrentfilter") == "1" ? "1" : "0")
                + "&filterid=" + getAttributeValue(oChartParam, "filterid") + "$##$";
        }
    }

    //
    var oParam = sOtherParam.split("$##$");
    if (oParam.length != 2)
        return;

    var sParamChart = oParam[0];
    var sParamExpressFilter = oParam[1];

    // Il y a au moins 3
    var oParamChart = sParamChart.split("&");
    if (oParamChart.length != 3)
        return;

    var nTab;
    var nFilterId;
    var bAddCurrentFilter;
    //Paramètre du chart
    for (var i = 0; i < oParamChart.length; i++) {
        var oParamValue = oParamChart[i].split("=");
        if (oParamValue.length != 2)
            return;

        if (oParamValue[0] == "tab")
            nTab = oParamValue[1];
        else if (oParamValue[0] == "addcurrentfilter")
            bAddCurrentFilter = (oParamValue[1] == "1");
        else if (oParamValue[0] == "filterid")
            nFilterId = oParamValue[1];
    }

    //Filtre express
    var aParamExpressFilter = sParamExpressFilter.split("$||$");
    var sFld = "";
    var sOp = "";
    var sValue = "";
    for (var j = 0; j < aParamExpressFilter.length; j++) {
        var operator = '0';
        var oFieldFilter = aParamExpressFilter[j].split("$#$");

        if (oFieldFilter.length != 2)
            continue;

        if (oFieldFilter[1].length == 0)
            continue;

        var sFieldAlias = oFieldFilter[0];
        var aFieldAlias = sFieldAlias.split("_");
        var sFieldName = aFieldAlias[aFieldAlias.length - 1];
        var sFieldValue = oFieldFilter[1].replace(sFieldAlias + "_", "");

        var aFieldValue = sFieldValue.split("$|D|$");
        if (aFieldValue.length > 1)
            sFieldValue = aFieldValue[1];
        else {
            sFieldValue = aFieldValue[0].split("$|#|$")[0];
            if (typeof aFieldValue[0].split("$|#|$")[1] != 'undefined')
                operator = aFieldValue[0].split("$|#|$")[1];
        }



        if (sFld.length > 0) {
            sFld += ";#;";
            sOp += ";#;";
            sValue += ";#;";
        }

        sFld += sFieldName;
        if (sFieldValue.length == 0 && operator == '0')
            sOp += "10"; //VIDE
        else
            sOp += operator;
        sValue += sFieldValue;

    }

    var sLstExpress = ""
    if (sFld.length > 0)
        sLstExpress = "filterexpress=" + sFld + ";|;" + sOp + ";|;" + sValue;




    //Table
    var updatePref = "tab=" + nTab;

    //Filtre User
    if (isNumeric(nFilterId) && nFilterId > 0)
        updatePref += ";$;listfilterid=" + nFilterId;


    //Ajoute les filtre express
    if (sLstExpress.length > 0)
        updatePref += ";$;" + sLstExpress;


    // On récupère les paramètres de la page que l'on va stocker dans une variable
    var oeParam = getParamWindow();

    // Après l'avoir stocké on lui applique le numéro de la page.
    oeParam.SetParam('Page_' + nTab, 1);

    if (!bAddCurrentFilter) {

        //Annule tous les filtres liste

        var sCancellAll = "clearalltabfilter";
        var parameters = "tab=" + nTab + ";$;" + sCancellAll;

        updateUserPref(parameters, function () {
            updateUserPref(updatePref, function () {
                //BSE:#63 477 => Après un drillDown, forcer l'affichage du mode liste
                var oeParam = getParamWindow();
                oeParam.SetParam('FirstSubTab_' + nTab, 0);
                goTabList(nTab, true, undefined, undefined, true);
            });
        });

    }
    else {
        updateUserPref(updatePref, function () {
            //BSE:#63 477 => Après un drillDown, forcer l'affichage du mode liste
            var oeParam = getParamWindow();
            oeParam.SetParam('FirstSubTab_' + nTab, 0);
            goTabList(nTab, true, undefined, undefined, true);
        });
    }
}

// Accéder à l'onglet avec le filtre demandé
function goTabListWithFilterFormular(nTab, nFilterId, bClearAllFilters, bForceList, bHisto) {
    if (isNumeric(nFilterId) && nFilterId > 0) {
        var obj = { ClearAllFilters: bClearAllFilters, ForceList: bForceList, Histo: bHisto };

        checkIfFormular(nTab, nFilterId, FROM_WIDGET, JSON.stringify(obj));
    }
    else
        goTabListWithFilter(nTab, nFilterId, bClearAllFilters, bForceList, bHisto);
}

function goTabListWithFilter(nTab, nFilterId, bClearAllFilters, bForceList, bHisto) {
    if (typeof bClearAllFilters === "undefined")
        bClearAllFilters = true;

    var updatePref = "tab=" + nTab;

    if (nFilterId > 0) {
        if (isNumeric(nFilterId) && nFilterId > 0)
            updatePref += ";$;listfilterid=" + nFilterId;
    }

    // ʕ´•ᴥ•`ʔ Si bHisto est défini, on force la pref historique
    if (typeof (bHisto) !== "undefined") {
        updatePref += ";$;histo=" + (bHisto ? "1" : "0");
    }

    if (bClearAllFilters) {
        updateUserPref("tab=" + nTab + ";$;" + "clearalltabfilter", function () {
            updateUserPref(updatePref, function () { goTabList(nTab, true, undefined, undefined, bForceList); });
        });
    }
    else {
        updateUserPref(updatePref, function () { goTabList(nTab, true, undefined, undefined, bForceList); });
    }

    //if (updatePref)

    //else
    //    goTabList(nTab, true, undefined, undefined, bForceList);
}

//#36771 Historique de navigation sur les onglets
var oNavManager = (function () {
    return {
        //Sauvegarde le mode indiqué
        SaveMode: function (mode, currentTab, newValue) {
            var oParam = getParamWindow();
            oParam.SetParam(mode + 'Id_' + currentTab, newValue);
            oParam.SetParam('TabMem', currentTab);
            if (mode == "List") {
                // Si newValue est renseigné, on indique quel sera le sous-onglet à afficher pour le mode Liste en cours
                // On utilise newValue ici et non currentTab, car ces valeurs seront utilisées par loadPrevWebTab() qui sera appelée par le mode Liste réellement
                // affiché (celui correspondant à newValue) une fois le chargement terminé (listLoaded)
                if (newValue != '') {
                    var webTabId = oWebMgr.getSelectedId();
                    var webTabItemType = oWebMgr.getSelectedItemType();
                    var webTabLabel = webTabId > 0 ? oWebMgr.getSelectedLabel() : ''; // si on se situe sur le sous-onglet "Liste" : pas de libellé personnalisé
                    oParam.SetParam('WebTabId_' + newValue, webTabId);
                    oParam.SetParam('WebTabItemType_' + newValue, webTabItemType);
                    oParam.SetParam('WebTabLabel_' + newValue, webTabLabel);
                }
                // Si newValue est à '' = Reset
                // On remet alors à zéro les infos mémorisées concernant le dernier sous-onglet affiché pour le TabId en cours
                else {
                    oParam.SetParam('WebTabId_' + currentTab, '');
                    oParam.SetParam('WebTabItemType_' + currentTab, '');
                    oParam.SetParam('WebTabLabel_' + currentTab, '');
                }
            }
        },
        //Remet à zéro le mode indiqué
        ResetMode: function (mode, currentTab) {
            return this.SaveMode(mode, currentTab, "");
        },
        //Retourne l'id (FileId ou TabId) du dernier mode consulté (Fiche ou Liste)
        GetRecentId: function (mode, currentTab) {
            var oParam = getParamWindow();
            if (typeof (oParam) == "undefined" || !oParam || typeof (oParam.GetParam) != "function")
                return 0;
            var recentId = oParam.GetParam(mode + 'Id_' + currentTab);
            if (recentId != "undefined" && recentId != null && recentId != '' && parseInt(recentId) > 0) {
                return parseInt(recentId);
            }

            return 0;
        },
        //On passe de mode fiche en mode liste sauf si on clique sur l'onglet de la fiche qu'on vient de consulter
        IsTabSwitching: function (tab) {
            var oParam = getParamWindow();
            if (typeof (oParam) == "undefined" || !oParam || typeof (oParam.GetParam) != "function")
                return false;
            var oldTab = oParam.GetParam('TabMem');
            if (oldTab == null || oldTab.length == 0)
                return true;

            return (parseInt(oldTab) != parseInt(tab));
        },

        //Savoir si pour cette table, il y a une fiche ou liste précédemment consultée
        HasRecent: function (mode, tab) {
            return this.GetRecentId(mode, tab) > 0;
        }
    }
})();

var oNavKeyManager = (function () {
    return {
        debugEnabled: false, // Mettre à true pour afficher des messages de diagnostic - A NE PAS UTILISER EN PRODUCTION

        // ID utilisés
        navBarId: "menuNavBar",

        // Classes CSS utilisées
        navBarElementActive: "navActiveElt",
        navBarEntry: "navEntry",
        navBarEntryActive: "navEntryActive",
        navBarEntryHidden: "tab-hidden",
        menuClass: "sbMenu",
        menuClassActive: "sbMenuActive",
        menuClassHidden: "ul-tab-hidden",
        menuClassVisible: "ul-tab-visible",
        menuEntryClass: "navLst",
        menuAltEntryClass: "navLstClair",
        menuActionEntryClass: "navAction",
        menuBottomEntryClass: "sbmBottom",
        menuSearchInputClass: "navSearchInpt",

        // Références aux objets ciblés
        navBar: null,
        sourceObj: null,

        Debug: function (message) {
            if (this.debugEnabled) {
                try { console.log(message); }
                catch (ex) { }
            }
        },

        InitSelection: function () {
            if (!this.navBar) {
                this.Debug("Objet non initialisé. Initialisation à partir de " + this.navBarId);
                this.navBar = document.getElementById(this.navBarId);
            }
            else
                this.Debug("Objet déjà initialisé");
        },

        ClearSelection: function (oCurrentTab) {
            this.navBar = null;
            this.sourceObj = null;
            var oNavSelectedElements = document.querySelectorAll("." + this.navBarElementActive + ", ." + this.navBarEntryActive + ", ." + this.menuClassVisible);
            for (var i = 0; i < oNavSelectedElements.length; i++) {
                var oCurrentElementParentTab = eTools.FindClosestElementWithClass(oNavSelectedElements[i], this.navBarEntry);
                if (oCurrentElementParentTab != oCurrentTab) {
                    removeClass(oNavSelectedElements[i], this.navBarElementActive);
                    removeClass(oNavSelectedElements[i], this.menuClassVisible);
                }
                if (oNavSelectedElements[i] != oCurrentTab) {
                    removeClass(oNavSelectedElements[i], this.navBarEntryActive);
                }
            }
        },

        MoveSubMenu: function (nDirection) {
            if (!this.navBar)
                this.InitSelection();

            var oCurrentTab = this.navBar.querySelector("." + this.navBarEntryActive);
            if (!oCurrentTab && this.sourceObj) {
                this.Debug("Pas d'onglet sélectionné détecté - Recherche de l'onglet parent à partir de l'élément " + this.sourceObj.id);
                oCurrentTab = eTools.FindClosestElementWithClass(this.sourceObj, this.navBarEntry);
            }

            if (oCurrentTab) {
                this.Debug("Onglet sélectionné détecté : " + oCurrentTab.id);
                var oCurrentSubMenu = oCurrentTab.querySelector("." + this.menuClass) || oCurrentTab.querySelector("." + this.menuClassActive);
                if (oCurrentSubMenu) {
                    this.Debug("Sous-menu détecté : " + oCurrentSubMenu.id);
                    // Eléments sélectionnables : uniquement les li comportant les classes listées ci-dessous (navLst/navLstClair = entrées des MRU, navAction : actions fixes sous les MRU, sbmBottom : bouton Administrer)
                    var oCurrentEntries = oCurrentSubMenu.querySelectorAll(
                        "li." + this.menuEntryClass +
                        ", li." + this.menuAltEntryClass +
                        ", li." + this.menuActionEntryClass +
                        ", li." + this.menuBottomEntryClass);
                    var oCurrentSelectedEntry = oCurrentSubMenu.querySelector("li." + this.navBarElementActive);
                    var oEntryToSelect = null;
                    if (oCurrentSelectedEntry) {
                        this.Debug("Elément actuellement sélectionné : " + oCurrentSelectedEntry.id + ". Recherche de l'élément suivant ou précédent...");
                        for (var i = 0; i < oCurrentEntries.length; i++) {
                            if (oCurrentEntries[i] == oCurrentSelectedEntry) {
                                this.Debug("Elément sélectionné ciblé, index " + i + ". Recherche du prochain à sélectionner...");
                                // Vers le haut
                                if (nDirection == 0) {
                                    if (i > 0) {
                                        this.Debug("Sélection de l'élément " + (i - 1) + " (navigation vers le haut)");
                                        oEntryToSelect = oCurrentEntries[i - 1];
                                    }
                                    else {
                                        this.Debug("L'élément sélectionné est le premier de la liste, retour au dernier élément de la liste à l'index " + (oCurrentEntries.length - 1) + " (navigation vers le haut)");
                                        oEntryToSelect = oCurrentEntries[oCurrentEntries.length - 1];
                                    }
                                }
                                // Vers le bas
                                else {
                                    if (i >= oCurrentEntries.length - 1) {
                                        this.Debug("L'élément sélectionné est le dernier de la liste, retour au premier élément de la liste à l'index 0 (navigation vers le bas)");
                                        oEntryToSelect = oCurrentEntries[0];
                                    }
                                    else {
                                        this.Debug("Sélection de l'élément " + (i + 1) + " (navigation vers le bas)");
                                        oEntryToSelect = oCurrentEntries[i + 1];
                                    }
                                }
                            }
                            removeClass(oCurrentEntries[i], this.navBarElementActive);
                        }
                    }
                    if (!oEntryToSelect && oCurrentEntries.length > 0) {
                        this.Debug("Aucun élement à sélectionner n'a pu être trouvé, sélection de l'élément 0");
                        oEntryToSelect = oCurrentEntries[0];
                    }
                    if (oEntryToSelect) {
                        this.Debug("Sélection de l'élément " + oEntryToSelect.id);
                        addClass(oEntryToSelect, this.navBarElementActive);
                    }
                }
            }
            else {
                this.MoveTab(0);
            }
        },

        MoveTab: function (nDirection) {
            if (!this.navBar)
                this.InitSelection();

            // Sélection de tous les onglets à l'exception d'Accueil (nvb0) et des onglets masqués sur d'autres pages (comportant la classe tab-hidden)
            var oAllTabs = this.navBar.querySelectorAll("." + this.navBarEntry + ":not(." + this.navBarEntryHidden + "):not(#nvb0)");
            var oTabToSelect = null;

            var oCurrentTab = this.navBar.querySelector("." + this.navBarEntryActive);
            if (!oCurrentTab && this.sourceObj) {
                this.Debug("Pas d'onglet sélectionné détecté - Recherche de l'onglet parent à partir de l'élément " + this.sourceObj.id);
                oCurrentTab = eTools.FindClosestElementWithClass(this.sourceObj, this.navBarEntry);
            }

            if (oCurrentTab) {
                var oSearchInput = oCurrentTab.querySelector("li." + this.menuSearchInputClass + " input[type='text']");
                if (oSearchInput && oSearchInput.id.lastIndexOf("mru_search_", 0) === 0 && oSearchInput.value != '') {
                    this.Debug("La navigation clavier a été annulée, priorité au champ de recherche où ont été saisis plusieurs caractères.");
                    return;
                }

                this.Debug("Onglet sélectionné détecté : " + oCurrentTab.id + ". Recherche de l'onglet suivant ou précédent...");
                for (var i = 0; i < oAllTabs.length; i++) {
                    if (oAllTabs[i] == oCurrentTab) {
                        this.Debug("Onglet sélectionné ciblé, index " + i + ". Recherche du prochain à sélectionner...");
                        // Vers la gauche
                        if (nDirection == 0) {
                            if (i > 0) {
                                this.Debug("Sélection de l'onglet " + (i - 1) + " (navigation vers la gauche)");
                                oTabToSelect = oAllTabs[i - 1];
                            }
                            else {
                                this.Debug("L'onglet sélectionné est le premier de la liste, retour au dernier onglet de la liste à l'index " + (oAllTabs.length - 1) + " (navigation vers la gauche)");
                                oTabToSelect = oAllTabs[oAllTabs.length - 1];
                            }
                        }
                        // Vers la droite
                        else {
                            if (i >= oAllTabs.length - 1) {
                                this.Debug("L'onglet sélectionné est le dernier de la liste, retour au premier onglet de la liste à l'index 0 (navigation vers la droite)");
                                oTabToSelect = oAllTabs[0];
                            }
                            else {
                                this.Debug("Sélection de l'onglet " + (i + 1) + " (navigation vers la droite)");
                                oTabToSelect = oAllTabs[i + 1];
                            }
                        }
                    }
                    removeClass(oAllTabs[i], this.navBarEntryActive);
                    var oCurrentSubMenu = oAllTabs[i].querySelector("." + this.menuClass) || oAllTabs[i].querySelector("." + this.menuClassActive);

                    if (oCurrentSubMenu) {
                        addClass(oCurrentSubMenu, this.menuClassHidden);
                        removeClass(oCurrentSubMenu, this.menuClassVisible);
                        switchClass(oCurrentSubMenu, this.menuClass, this.menuClassActive);
                    }
                }
            }
            if (!oTabToSelect && oAllTabs.length > 0) {
                this.Debug("Aucun onglet à sélectionner n'a pu être trouvé, sélection de l'élément 0");
                oTabToSelect = oAllTabs[0];
            }
            if (oTabToSelect) {
                this.Debug("Sélection de l'onglet " + oTabToSelect.id);
                addClass(oTabToSelect, this.navBarEntryActive);
                var oCurrentSubMenu = oTabToSelect.querySelector("." + this.menuClass) || oTabToSelect.querySelector("." + this.menuClassActive);
                if (oCurrentSubMenu) {
                    addClass(oCurrentSubMenu, this.menuClassVisible);
                    removeClass(oCurrentSubMenu, this.menuClassHidden);
                    switchClass(oCurrentSubMenu, this.menuClass, this.menuClassActive);
                    var oSearchInput = oCurrentSubMenu.querySelector("li." + this.menuSearchInputClass + " input[type='text']");
                    if (oSearchInput)
                        focusOrSelect(oSearchInput, null);
                }
            }
        },

        CloseMenu: function () {

        },

        TriggerActiveElement: function () {
            if (!this.navBar)
                this.InitSelection();

            var oCurrentTab = this.navBar.querySelector("." + this.navBarEntryActive);
            if (!oCurrentTab && this.sourceObj) {
                this.Debug("Pas d'onglet sélectionné détecté - Recherche de l'onglet parent à partir de l'élément " + this.sourceObj.id);
                oCurrentTab = eTools.FindClosestElementWithClass(this.sourceObj, this.navBarEntry);
            }
            if (oCurrentTab) {
                this.Debug("Onglet sélectionné détecté : " + oCurrentTab.id);
                var oCurrentSubMenu = oCurrentTab.querySelector("." + this.menuClass) || oCurrentTab.querySelector("." + this.menuClassActive);
                if (oCurrentSubMenu) {
                    this.Debug("Sous-menu détecté : " + oCurrentSubMenu.id);
                    var oCurrentSelectedEntry = oCurrentSubMenu.querySelector("li." + this.navBarElementActive);
                    if (oCurrentSelectedEntry) {
                        this.Debug("Elément actuellement sélectionné : " + oCurrentSelectedEntry.id);
                        if (typeof (oCurrentSelectedEntry.click) == "function") {
                            this.Debug("Fonction click trouvée - Déclenchement");
                            oCurrentSelectedEntry.click();
                        }
                    }
                }
            }
        }
    }
})();