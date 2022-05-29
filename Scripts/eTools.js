var eTools = {}; // namespace 
var rspace = /\s+/;
var rclass = /[\n\t\r]/g;


if (typeof (sTheme) == "undefined") {
    if (typeof (top.sTheme) != "undefined")
        var sTheme = top.sTheme;
    else
        var sTheme = "default";
}

//protype de trim de base pour IE8
if (typeof String.prototype.trim !== 'function') {
    String.prototype.trim = function () {
        return this.replace(/^\s+|\s+$/g, '');
    }
}
if (typeof String.prototype.ltrim !== 'function') {
    String.prototype.ltrim = function () {
        return this.replace(/^\s+/, "");
    }
}
if (typeof String.prototype.rtrim !== 'function') {
    String.prototype.rtrim = function () {
        return this.replace(/\s+$/, "");
    }
}




// Backlog #42
// Déclaration d'un polyfill pour ajouter le support de Object.assign() sur IE - https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Object/assign#Polyfill
if (typeof Object.assign != 'function') {
    // Must be writable: true, enumerable: false, configurable: true
    Object.defineProperty(Object, "assign", {
        value: function assign(target, varArgs) { // .length of function is 2
            'use strict';
            if (target == null) { // TypeError if undefined or null
                throw new TypeError('Cannot convert undefined or null to object');
            }

            var to = Object(target);

            for (var index = 1; index < arguments.length; index++) {
                var nextSource = arguments[index];

                if (nextSource != null) { // Skip over if undefined or null
                    for (var nextKey in nextSource) {
                        // Avoid bugs when hasOwnProperty is shadowed
                        if (Object.prototype.hasOwnProperty.call(nextSource, nextKey)) {
                            to[nextKey] = nextSource[nextKey];
                        }
                    }
                }
            }
            return to;
        },
        writable: true,
        configurable: true
    });
}

// US #1330 - Tâches ##2748, #2750 - Polyfill pour ajouter le support de Array.flat() sur IE caca - https://github.com/jonathantneal/array-flat-polyfill/blob/master/src/polyfill-flat.js
if (!Array.prototype.flat) {
    Object.defineProperty(Array.prototype, 'flat', {
        configurable: true,
        value: function flat() {
            var depth = isNaN(arguments[0]) ? 1 : Number(arguments[0]);

            return depth ? Array.prototype.reduce.call(this, function (acc, cur) {
                if (Array.isArray(cur)) {
                    acc.push.apply(acc, flat.call(cur, depth - 1));
                } else {
                    acc.push(cur);
                }

                return acc;
            }, []) : Array.prototype.slice.call(this);
        },
        writable: true
    });
}

/// <summary>
/// Format de champs
/// </summary>
var FLDTYP =
{
    CHAR: 1,
    /// <summary>Date</summary>
    DATE: 2,
    BIT: 3,
    /// <summary>Compteur auto</summary>
    AUTOINC: 4,
    /// <summary>Numéraire</summary>
    MONEY: 5,
    EMAIL: 6,
    WEB: 7,
    USER: 8,
    MEMO: 9,
    /// <summary>Numérique</summary>
    NUMERIC: 10,
    PHONE: 12,
    TITLE: 15,
    CHART: 17,
    /// <summary>Count</summary>
    COUNT: 18,
    /// <summary>Mode fiche</summary>
    ID: 20

}

// Opérateurs de filtre
var FilterOperators = {
    OP_EQUAL: 0,
    OP_LESS: 1,
    OP_LESS_OR_EQUAL: 2,
    OP_GREATER: 3,
    OP_GREATER_OR_EQUAL: 4,
    OP_DIFFERENT: 5,
    OP_START_WITH: 6,
    OP_END_WITH: 7,
    OP_IN_LIST: 8,
    OP_CONTAIN: 9,
    OP_IS_EMPTY: 10,
    OP_IS_NOT_EMPTY: 17,
    OP_IS_FALSE: 12,
    OP_NOT_START_WITH: 13,
    OP_NOT_END_WITH: 14,
    OP_NOT_IN_LIST: 15,
    OP_NOT_CONTAIN: 16,
    OP_IS_TRUE: 11
}



//Forcer le mode tablette, afin d'utiliser l'interface tablette sur PC (Chrome) ou l'interface PC sur tablettes
//null : détection automatique en fonction de la plateforme
//true : forcer le mode tablettes
//false : forcer le mode PC
var debugTablet = null;
// Modifier ces variables permet d'activer ou désactiver certaines fonctionnalités tablette indépendemment
var debugTabletMenuEnabled = false; 		            // menu escamotable pour tablettes
var debugTabletNavBarEnabled = true; 	            // barre d'onglets (NavBar) avec défilement optimisé pour tablettes
var debugTabletCalendarEnabled = true;              // calendrier avec commandes/gestes pour tablettes
var debugTabletCustomScrollersEnabled = false; 	    // fonctionnalités de défilement ajoutées manuellement sur certains éléments
var debugTabletNativeScrollEnabled = true;          // fonctionnalités de défilement natives du navigateur DONT LE ZOOM
var debugTabletOrientationSupportEnabled = true;    // adapter l'affichage lors d'un changement d'orientation d'écran (rotation à 90°)
var debugTabletTiltSupportEnabled = false;            // autoriser la tablette à effectuer certains traitements lorsqu'on la penche


var oEvent = oEvent || top.oEvent;

//Activer ou désactiver certaines options pour le debug
var debugConfirmUnload = true; // avertir l'utilisateur lorsqu'il quitte l'ensemble de l'application ou rafraîchit la page mère


//Retourne le type d'un objet
eTools.getObjType = function (obj) {

    if (typeof obj === "undefined")
        return "undefined"

    return Object.prototype.toString.call(obj).slice(8, -1)
}

// Stop propagation
function stopEvent(e) {

    var evt = e ? e : window.event;
    e = evt;

    // cancelable event
    e.returnValue = false;
    if (e.preventDefault)
        e.preventDefault();

    // non-cancelable event
    e.cancelBubble = true;
    if (e.stopPropagation)
        e.stopPropagation();
}

// True si l'evenment est en dehors de viewport
function mouseLeaved(e) {
    var evt = e ? e : window.event;
    var from = evt.relatedTarget || evt.toElement;
    if (!from || from.nodeName == "HTML") {
        return true;
    }

    return false;
}

// Déclenche l'évènement nommé sur l'élément indiqué
function fireEvent(element, eventName) {
    if (typeof (element) == "string")
        element = document.getElementById(element);
    if (element != null) {
        if (element.fireEvent) {
            element.fireEvent('on' + eventName);
        }
        else if (document.createEvent) {
            var evObj = document.createEvent('Events');
            evObj.initEvent(eventName, true, false);
            element.dispatchEvent(evObj);
        }
    }
}

// Retourne l'objet document via l'Iframe des params
function getParamWindow() {
    var eParamWindow = top.document.getElementById('eParam');

    if (eParamWindow) {
        if (eParamWindow.contentWindow && typeof (eParamWindow.contentWindow.GetParam) == "function") {
            eParamWindow = eParamWindow.contentWindow;
        }
    }

    return eParamWindow;
}

// Récupère la valeur de l'attribute. Retourne vide si le node et/ou l'attribute n'hesite pas
function getAttributeValue(oNode, attributeName) {
    var bIsOk = oNode != null;
    try {
        bIsOk = bIsOk && oNode.getAttribute;    //Plante sur IE8 si on est sur un node XML, même si la fonction existe.
    }
    catch (e) {
    }
    bIsOk = bIsOk && oNode.getAttribute(attributeName); // SPH: TOCHECK : du coup, si l'attribut a comme valeur 0, ca remonte ''. Est-ce normal ?

    if (bIsOk)
        return oNode.getAttribute(attributeName);
    return '';
}
// Set la valeur de l'attribute.  
function setAttributeValue(oNode, attributeName, value) {

    if (oNode != null && oNode.setAttribute)
        return oNode.setAttribute(attributeName, value);

    return false;

}

// Récupère la valeur de l'attribute en fournissant l'id de l"element  
function getAttributeValueById(NodeId, attrName) {
    if (typeof (NodeId) != 'string')
        return '';

    var oNode = document.getElementById(NodeId);
    return getAttributeValue(oNode, attrName, attrValue);
}

// Set la valeur de l'attribute en fournissant l'id de l'element 
function setAttributeValueById(NodeId, attrName, attrValue) {
    if (typeof (NodeId) != 'string')
        return false;

    var oNode = document.getElementById(NodeId);
    return setAttributeValue(oNode, attrName, attrValue);
}

// Indique si un neoud dans la src exist 
function nodeExist(srcDocument, id) {
    var node = srcDocument.getElementById(id);
    return node != null && typeof (node) != 'undefined';
}

// Indique si un neoud dans l'element exist 
function ElementExist(srcElement, id) {
    //MOU- On recherche sur un element html quelconque
    var nodes = srcElement.childNodes;
    for (i = 0; i < nodes.length; i++) {
        nodeId = getAttributeValue(nodes[i], "id");
        if (nodeId == id)
            return true;
    }
    return false;
}

function GetMainTableDescId(tdId) {
    if (!tdId && tdId.indexOf("COL_") != 0)
        return '';

    if (tdId.indexOf("_") == -1)
        return '';

    // COL_<Alias du field> ou 
    // COL_<Alias du field>_<Id de la fiche principale>_<Id de la fiche du champs>_<Numéro de ligne>
    var infos = tdId.split('_');
    return infos[1];
}

function GetMasterFileId(tdId) {
    if (tdId.indexOf("COL_") != 0)
        return '';

    // COL_<Alias du field>_<Id de la fiche principale>_<Id de la fiche du champs>_<Numéro de ligne>
    var infos = tdId.split('_');
    return infos[infos.length - 3];
}

function GetFieldFileId(tdId) {
    if (tdId.indexOf("COL_") != 0)
        return '';

    // COL_<Alias du field>_<Id de la fiche principale>_<Id de la fiche du champs>_<Numéro de ligne>
    var infos = tdId.split('_');
    return infos[infos.length - 2];
}

// Récupère le numéro de ligne
function GetFieldRowLine(tdId) {
    if (tdId.indexOf("COL_") != 0)
        return '';

    // COL_<Alias du field>_<Id de la fiche principale>_<Id de la fiche du champs>_<Numéro de ligne>
    var infos = tdId.split('_');
    return infos[infos.length - 1];
}

/*Retourne le FIleId de la fiche à partir d'une fiche depuis une table
nTab : Table en cours
*/
function GetTabFileId(nTab) {
    var nFileId = 0;

    var oInputFields = document.getElementById("fieldsId_" + nTab);
    if (oInputFields) {
        var aFieldsDescid = oInputFields.value.split(";");
        for (var nCmptFld = 0; nCmptFld < aFieldsDescid.length; nCmptFld++) {
            var descid = aFieldsDescid[nCmptFld];
            var col = "COL_" + nTab + "_" + descid;
            var oLib = document.getElementById(col);
            if (!oLib)
                continue;
            var idFldInput = getAttributeValue(oLib, "eltvalid");
            if (idFldInput == "")
                continue;
            nFileId = GetFieldFileId(idFldInput);
            break;
        }
    }

    if (nFileId == 0) {

        var nTabShort = 1;

        nTabShort = nTab / 100;

        var sName = "COL_" + nTab + "_" + nTabShort;
        var oElem = document.querySelectorAll('td[id^="' + sName + '"]');

        for (var nCmptFld = 0; nCmptFld < oElem.length; nCmptFld++) {
            var oTD = oElem[nCmptFld];
            var oTDId = oTD.id;

            if (typeof (oTDId) == "string") {
                var aTDID = oTDId.split("_");

                if (aTDID.length == 3 && aTDID[1].length == aTDID[2].length) {
                    var idFldInput = getAttributeValue(oTD, "eltvalid");
                    if (idFldInput == "")
                        continue;

                    nFileId = GetFieldFileId(idFldInput);

                    break;
                }
            }
        }

    }

    return nFileId;
}

/*Retourne le input du champ sur la fiche en cours
nTab : Table en cours
nDescid : descid de l'input du champ
*/
function GetField(nTab, nDescid) {
    var eltHead = document.getElementById("COL_" + nTab + "_" + nDescid);
    if (eltHead)
        return document.getElementById(getAttributeValue(eltHead, "eltvalid"));
    else
        return null;
}



/// Nettoye la chaine d'id passé en paramètre de ses valeurs non numérique  
function CleanListIds(sList, sSep) {



    if (typeof (sList) == "undefined" || sList.length == 0 || typeof (sSep) == "undefined" || sSep.length == 0)
        return sList;

    var sPattern = "[^" + sSep + "]*[^0-9" + sSep + "]{1}[^" + sSep + "]*" + sSep + "?";

    var re = new RegExp(sPattern, "gmi");
    return sList.replace(re, "");



}

/*********************************************************/
/* FONCTIONS MANIPULTION DE CSS      / JS                */
/*********************************************************/

/* Retourne la règle css du selecteur "sCssSelector" de la feuille "sCssName" */
function getCssSelector(sCssName, sCssSelector, doc) {

    var oStyle;
    var sRule;

    doc = doc || document;

    for (var j = 0; j < doc.styleSheets.length; j++) {

        var styleSheetName = doc.styleSheets[j].href != null ? doc.styleSheets[j].href : doc.styleSheets[j].title;

        if (sCssName == "*" || (typeof (styleSheetName) === "string" && styleSheetName.toLowerCase().indexOf(sCssName.toLowerCase()) >= 0)) {
            oStyle = doc.styleSheets[j];
            var myrules = oStyle.cssRules ? oStyle.cssRules : oStyle.rules;
            for (var i = 0; i < myrules.length; i++) {

                var oRule = myrules[i];
                if (oRule.selectorText && oRule.selectorText.toLowerCase() == sCssSelector.toLowerCase()) {
                    return oRule;
                }
            }


            if (sCssName != "*")
                return null;
        }
    }
    return null;
}

//Echange 2 classses 
function switchClass(elem, oldvalue, newvalue) {

    try {
        removeClass(elem, oldvalue);
    }
    catch (e) {

    }

    try {
        addClass(elem, newvalue);
    }
    catch (e) {
    }

}

///Ajoute les classes css <value> (séparateur " ") à l'élément value
function addClass(elem, value) {
    if (elem == null)
        return;
    var classNames, i, l, setClass, c, cl;

    if (value && typeof value === "string") {
        if (value != "") {
            classNames = value.split(rspace);

            if (elem.nodeType === 1) {
                if (!elem.className && classNames.length === 1) {
                    elem.className = value;
                }
                else {
                    setClass = " " + elem.className + " ";
                    for (c = 0, cl = classNames.length; c < cl; c++) {
                        if (! ~setClass.indexOf(" " + classNames[c] + " ")) {
                            setClass += classNames[c] + " ";
                        }
                    }
                    elem.className = eTrim(setClass);
                }
            }
        }
    }
}

///Retire les classes css <value> (séparateur " ") à l'élément value
function removeClass(elem, value) {
    if (elem == null)
        return;

    var classNames, i, l, className, c, cl;

    if ((value && typeof value === "string") || value === undefined) {
        classNames = (value || "").split(rspace);

        if (elem.nodeType === 1 && elem.className) {
            if (value) {
                className = (" " + elem.className + " ").replace(rclass, " ");
                for (c = 0, cl = classNames.length; c < cl; c++) {
                    className = className.replace(" " + classNames[c] + " ", " ");
                }

                elem.className = eTrim(className);

            } else {
                elem.className = "";
            }
        }
    }
}

// Teste si l'élément possède la classe en paramètre
function hasClass(element, className) {
    if (element != null)
        return (' ' + element.className + ' ').indexOf(' ' + className + ' ') > -1;
    return false;
}

// Ajoute/modifie une règle css dans la css 'sIdStyle'
//  ajoute la classe dans le <style> de head ayant le title/id "sIdStyle"
function createCss(sIdStyle, sCssName, sCssClass, bReplace) {

    // Les 3er params sont obligatoires
    if (typeof (sIdStyle) == "undefined" || typeof (sCssName) == "undefined" || typeof (sCssClass) == "undefined")
        return;

    if (typeof (bReplace) == "undefined")
        var bReplace = false;

    sCssName = '.' + sCssName;

    // récupération des déclaration de style
    var oStyle = document.styleSheets[sIdStyle];

    //FF
    if (typeof (oStyle) == "undefined") {
        for (j = 0; j < document.styleSheets.length; j++) {
            if (document.styleSheets[j].title == sIdStyle) {
                oStyle = document.styleSheets[j];
                break;
            }
        }
    }

    if (typeof (oStyle) == "object") {

        //IE/FF selecteur
        var myrules = oStyle.cssRules ? oStyle.cssRules : oStyle.rules;

        //Test existance de la règle
        for (i = 0; i < myrules.length; i++) {

            var myRules = myrules[i];

            //Si règle déjà présente
            if (myRules.selectorText.toLowerCase() == sCssName.toLowerCase()) {
                if (bReplace) {

                    //suppression de la règle
                    if (oStyle.deleteRule)
                        oStyle.deleteRule(i);
                    else
                        oStyle.removeRule(i);

                    break;
                }
                else {
                    return;
                }
            }
        }

        if (sCssClass == "")
            return;

        //Ajout de la règle
        if (oStyle.insertRule) {
            var sRules = sCssName + '{' + sCssClass + '}';
            oStyle.insertRule(sRules, myrules.length);
        } else {
            oStyle.addRule(sCssName, sCssClass);
        }

    }
}



/**
 * Permet d'activer ou de désactiver tous les CSS suivant l'attribut eType
 * @param {any} eType
 * @param {any} bDisabled
 */
function activateCSSIrisFile(eType, bDisabled) {
    document.querySelectorAll('[rel^="stylesheet"][eType='+eType+']').forEach(function (n) { n.disabled = bDisabled });
}


// ajoute une feuille css
// utilisé pour charger à la demande les css des modes listes ou fiche
// Backlog #267 et #304 : code de vérification d'existence de la CSS externalisé dans hasCss()
function addCss(href, stype, oDoc) {
    if (typeof (oDoc) == 'undefined') {
        oDoc = document;
    }


    if (typeof (stype) != "undefined" && stype != "") {

        // vérifie l'existence de la css avant ajout. Si existante, on ne la rajoute pas, mais on active l'existante (paramètre à true de hasCss)
        if (!hasCss(href, oDoc, true)) {
            var sHref = "";

            // #68 13x - Prise en charge de l'éditeur de templates avancé - Inclusion de CSS distantes
            if (href.substring(0, 7) == "http://" || href.substring(0, 8) == "https://" || href.substring(0, 2) == "//")
                sHref = href; // pas de rajout de .css, car certaines URL n'en comportent pas
            // Scripts locaux
            else {
                if (href == "theme")
                    sHref = "themes/" + sTheme + "/css/" + href + ".css";
                else
                    sHref = "themes/default/css/" + href + ".css";

                if (typeof (top._CssVer) != 'undefined' && top._CssVer != "")
                    sHref += '?ver=' + top._CssVer;
            }

            if (oDoc.createStyleSheet) {
                var oCss = oDoc.createStyleSheet(sHref);
                setAttributeValue(oCss, "eType", stype);
            }
            else {
                var oHead = oDoc.getElementsByTagName("head")[0];
                var oCss = oDoc.createElement('link');
                oCss.type = 'text/css';
                oCss.rel = 'stylesheet';
                oCss.href = sHref;
                oCss.setAttribute("eType", stype);
                oHead.appendChild(oCss);
            }
        }
    }
}

// Backlog #267 et #304 - vérifie l'existence d'une feuille css
function hasCss(href, oDoc, enableIfExists) {
    if (typeof (oDoc) == 'undefined') {
        oDoc = document;
    }

    if (oDoc.styleSheets) {

        // vérifie l'existence de la css
        for (var i = 0; i < oDoc.styleSheets.length; i++) {
            var sCssName = href + ".css";

            if (typeof (oDoc.styleSheets[i].href) === "string" && oDoc.styleSheets[i].href.toLowerCase().indexOf(sCssName.toLowerCase()) >= 0) {
                if (enableIfExists) {
                    oDoc.styleSheets[i].disabled = false;
                }
                return true;
            }
        }
    }

    return false;
}

// Backlog #267 et #304 - vérifie l'existence d'un contenu quelconque dans une des feuilles css chargées sur la page
// Pour vérifier notamment si le contenu d'une certaine CSS a été injecté ou non
function hasCssRule(sCssRule, oDoc, enableIfExists) {
    if (typeof (oDoc) == 'undefined') {
        oDoc = document;
    }

    if (oDoc.styleSheets) {

        // vérifie l'existence de la css
        for (var i = 0; i < oDoc.styleSheets.length; i++) {
            for (var j = 0; j < oDoc.styleSheets[i].cssRules.length; j++) {
                if (oDoc.styleSheets[i].cssRules[j].cssText.indexOf(sCssRule) > -1) {
                    if (enableIfExists) {
                        oDoc.styleSheets[i].disabled = false;
                    }
                    return true;
                }
            }
        }
    }

    return false;
}

// Backlog #617 - Charge un contenu CSS dans une iframe fictive, dédoublonne toutes les règles CSS, et renvoie le contenu CSS filtré sans règles en doublon ni commentaires
// Source : https://stackoverflow.com/a/51831549
eTools.cleanCss = function (sCssContents) {
    var dummyFrame = top.document.createElement("iframe");
    var dummyStyle = top.document.createElement("style");
    dummyStyle.innerHTML = sCssContents;
    var dummyFrameAttachment = top.document.body.appendChild(dummyFrame);
    dummyFrame.contentDocument.head.appendChild(dummyStyle);

    var allCSS =
        [].slice.call(dummyFrame.contentDocument.styleSheets)
            .reduce(function (prev, styleSheet) {
                if (styleSheet.cssRules) {
                    return prev +
                        [].slice.call(styleSheet.cssRules)
                            .reduce(function (prev, cssRule) {
                                return prev + cssRule.cssText;
                            }, '');
                } else {
                    return prev;
                }
            }, '');

    var uniqueRules = allCSS
        .split('}')
        .map(function (rule) {
            return rule ? rule.trim() + '}' : '';
        })
        .filter(function (rule, index, self) {
            return self.indexOf(rule) === index;
        })
        .join(' ');

    top.document.body.removeChild(dummyFrame);

    return uniqueRules
        .replace(/(?:{)/g, '{\t\n')
        .replace(/(?:;)/g, ';\t\n')
        .replace(/(?:})/g, '}\n')
        .replace(/(?:\n\n)/g, '\n'); // mise en forme minimale
};



eTools.setCssRuleFromString = function (sCssContents, selector, property, newValue, override) {

    var changeNeed = false;
    var dummyFrame = top.document.createElement("iframe");
    var dummyStyle = top.document.createElement("style");
    dummyStyle.innerHTML = sCssContents;

    var dummyFrameAttachment = top.document.body.appendChild(dummyFrame);
    dummyFrame.contentDocument.head.appendChild(dummyStyle);

    var oStyle = dummyFrame.contentDocument.styleSheets[0];
    var myrules = oStyle.cssRules ? oStyle.cssRules : oStyle.rules;
    var bFound = false;

    var newCss = "";
    for (var i = 0; i < myrules.length; i++) {
        var oRule = myrules[i];
        if (oRule.selectorText && oRule.selectorText.toLowerCase() == selector.toLowerCase()) {
            bFound = true;

            if (oRule.style.getPropertyValue(property) != newValue) {

                if (override) {
                    oRule.style.setProperty(property, newValue)
                    changeNeed = true;
                }
            }
        }

        if (newCss.indexOf(oRule.cssText) === -1)
            newCss += "\r\n" + oRule.cssText;
    }



    if (!bFound) {
        changeNeed = true;
        sCssContents += "\r\n " + selector + " {" + property + ": " + newValue + "}";
    }
    else
        sCssContents = newCss

    top.document.body.removeChild(dummyFrame);

    var res = {
        "hasChanged": changeNeed,
        "value": sCssContents
    }

    return res;

}

// Backlog #652 - Récupère la valeur d'une règle donnée dans un contenu CSS donné, en tenant compte de l'ordre d'application
eTools.getCssRuleFromString = function (sCssContents, selector, property, getLastOnly) {
    // Source : https://stackoverflow.com/a/51831549

    var dummyFrame = top.document.createElement("iframe");
    var dummyStyle = top.document.createElement("style");
    dummyStyle.innerHTML = sCssContents;
    var dummyFrameAttachment = top.document.body.appendChild(dummyFrame);
    dummyFrame.contentDocument.head.appendChild(dummyStyle);

    var selectedRuleValues =
        [].slice.call(dummyFrame.contentDocument.styleSheets)
            .reduce(function (prev, styleSheet) {
                if (styleSheet.cssRules) {
                    return prev +
                        [].slice.call(styleSheet.cssRules)
                            .reduce(function (prev, cssRule) {
                                if (cssRule.selectorText == selector && cssRule.style.getPropertyValue(property)) {
                                    // on concatène les éventuelles valeurs successives correspondant aux critères de recherche avec ,
                                    // permet ainsi de renvoyer "Tahoma, Verdana" si on trouve 2 propriétés font-family: Tahoma et font-family: Verdana pour le même sélecteur
                                    var propertyValue = cssRule.style.getPropertyValue(property);
                                    if (prev != '' && !getLastOnly)
                                        return prev + ", " + propertyValue;
                                    else
                                        return propertyValue;
                                }
                                else
                                    return prev;
                            }, '');
                } else {
                    return prev;
                }
            }, '');

    top.document.body.removeChild(dummyFrame);

    return selectedRuleValues;
};

///Retire la css du doc
function removeCSS(sName, myDoc) {

    if (myDoc == null || typeof myDoc == "undefined")
        myDoc = top.document;

    var oStyle = null;
    if (typeof (myDoc.styleSheets) != 'undefined') {
        for (var i = 0; i < myDoc.styleSheets.length; i++) {

            if (myDoc.styleSheets[i].href != null && myDoc.styleSheets[i].href.indexOf(sName + ".css") > -1) {
                var oStyle = myDoc.styleSheets[i];
                break;
            }
        }

        if (oStyle != null) {
            oStyle.disabled = true;
            oStyle.ownerNode.remove(oStyle);
        }
    }
}



function switchUserThemeToDefaultTheme(callback, errorCallback) {
    var oLink = document.head.querySelectorAll('link[eType="THEME"]')[0];
    if (oLink != null && oLink.hasAttributes("href") && oLink.href.indexOf("themes/" + sTheme + "/css/theme.css") != -1) {
        oLink.setAttribute("userTheme", oLink.href);
        oLink.href = oLink.href.replace("themes/" + sTheme + "/css/theme.css", "themes/default/css/theme.css");
    }

    if (typeof (callback) != "function")
        callback = function () { bDefaultThemeForced = true; } // mise à jour du booléen défini sur eMain, indiquant de restaurer le thème sur le unload() de la page
    if (typeof (errorCallback) != "function")
        errorCallback = function () { }

    // TOCHECK : Le thème par défaut est sauvegardé comme nouveau thème à utiliser parmi les préférences de l'utilisateur,
    // pour que les prochaines fenêtres chargées en tiennent compte. Cependant, si l'utilisateur rafraîchit la page via F5,
    // la fonction switchDefaultThemeToUserTheme() devrait être rechargée au onunload de la page (cf. eMain.js) mais ce phénomène
    // ne se produit pas sur l'admin, ni en mode Debug.
    if (typeof (applyTheme) == "function")
        applyTheme(0, nGlobalCurrentUserid, callback, errorCallback);
}

function switchDefaultThemeToUserTheme(callback, errorCallback) {


    /** ICI, si on est sur le nouveau thème Eudonet X, on charge un fichier CSS supplémentaire
     * qui sert de base aux CSS des thèmes Eudonet X. G.L */
    var paramWin = top.getParamWindow();
    var objThm = JSON.parse(paramWin.GetParam("currenttheme").toString());

    if (objThm.Version > 1)
        addCss("../../Theme2019/css/theme", "THEMEBASE");

    var oLink = document.head.querySelectorAll('link[eType="THEME"]')[0];
    if (oLink.getAttribute("userTheme") != null && oLink.getAttribute("userTheme") != '') {
        oLink.href = oLink.getAttribute("userTheme");
        oLink.removeAttribute("userTheme");
    }

    if (typeof (callback) != "function")
        callback = function () { bDefaultThemeForced = false; } // remise à zéro du booléen défini sur eMain, indiquant de restaurer le thème sur le unload() de la page
    if (typeof (errorCallback) != "function")
        errorCallback = function () { }

    if (typeof (applyTheme) == "function")
        applyTheme(nThemeId, nGlobalCurrentUserid, callback, errorCallback);
}

/// retire les link css/js
// du mode spécifié (LIST ou FICHE ou ALL)
//  et du type (JS/CSS/ALL)
function clearHeader(sMode, sType) {
    if (typeof (sType) == "undefined")
        var sType = "ALL";

    var bHasType = sMode || 0;
    if (bHasType) {


        //Clear CSS
        if (sType == "ALL" || sType == "CSS") {
            var oLink = document.getElementsByTagName("link");
            for (i = oLink.length - 1; i >= 0; i--) {

                if (sMode == "ALL" || oLink[i].getAttribute("eType") == sMode) {
                    oLink[i].parentNode.removeChild(oLink[i]);

                }
            }
        }

        //Clear JS
        if (sType == "ALL" || sType == "JS") {
            var oJS = document.getElementsByTagName("script");
            for (i = oJS.length - 1; i >= 0; i--) {
                if (sMode == "ALL" || oJS[i].getAttribute("eType") == sMode) {
                    oJS[i].parentNode.removeChild(oJS[i]);
                }
            }
        }
    }
}

//Ajoute une liste de fichier javascript les uns à la suite des autres et appel la fonction de callback après le chargement du dernier fichier
//  arrayHref : (Array) liste de JS à charger
//  stype : nom affiché (balise eType)
//  callback : fonction de callback appelée après le chargement du dernier fichier (null si pas utilisé)
//  oDoc : document cible de l'ajout (si null prendra le document courant)
/// addScriptFct : fonction à utiliser pour l'ajout de script (par défaut, addScript)
function addScripts(arrayHref, stype, callback, oDoc, addScriptFct) {
    if (!addScriptFct)
        addScriptFct = addScript;

    if (arrayHref && arrayHref.length > 0) {
        var href = arrayHref[0];
        var arrayHrefNew = arrayHref;
        arrayHrefNew.splice(0, 1);
        //tant qu'il y a des Script à charger, on load à la fin du chargement le JS suivant
        if (arrayHrefNew.length > 0)
            addScriptFct(href, stype,
                (function (paramarrayHref, paramstype, paramcallback, paramoDoc, paramAddScriptFct) {
                    return function () {
                        if (typeof addScripts === "function") {
                            addScripts(paramarrayHref, paramstype, paramcallback, paramoDoc, paramAddScriptFct);
                        }

                    };
                })(arrayHrefNew, stype, callback, oDoc, addScriptFct)
                , oDoc);
        else
            addScriptFct(href, stype, callback, oDoc); //s'il s'agit du dernier JS à charger, on appel la méthode de callback principale
    }
    else
        if (callback && typeof callback == "function")
            callback();
}


///AJoute un js 
function addGenericScript(href, stype, sRoot, callback, oDoc) {
    if (!oDoc || typeof (oDoc) == 'undefined') {
        oDoc = document;
    }


    if (typeof (stype) != "undefined" && stype != "") {
        var oHead = oDoc.getElementsByTagName("head")[0];
        var oScript = oDoc.createElement('script');

        oScript.type = 'text/javascript';

        // #68 13x - Prise en charge de l'éditeur de templates avancé - Inclusion de scripts distants
        if (href.substring(0, 7) == "http://" || href.substring(0, 8) == "https://" || href.substring(0, 2) == "//") {
            oScript.src = href; // pas de rajout de .js, car certaines URL n'en comportent pas
        }
        // Scripts locaux
        else {

            if (typeof sRoot != "string")
                sRoot = "scripts"

            sRoot += "/";

            oScript.src = sRoot + href + ".js";
            if (typeof (top._jsVer) != 'undefined' && top._jsVer != "")
                oScript.src += '?ver=' + top._jsVer;
        }

        oScript.setAttribute("eType", stype);

        if (typeof (callback) == "function") {
            // mza avec rma on a decemmentés cette ligne suite a une erreur quand on veux ajouter un nouveau contact 
            oScript.async = 'true';

            //Le code suivant fonctionne avec IE8 et supérieur :
            //  onload => Ne fonctionne pas avec IE8
            //  onreadystatechange => Fonctionne avec IE8 et autre            
            oScript.onload = oScript.onreadystatechange = function () {

                var rs = this.readyState;
                if (rs && rs != 'complete' && rs != 'loaded')
                    return;

                oScript.onload = oScript.onreadystatechange = null; //Pas 2 fois le même script

                try {
                    callback();
                }
                catch (e) {
                    alert('addScript Error - ' + callback);
                    alert('Exception: ' + e);
                    alert('Description: ' + e.message);
                }
            };
        }
        oHead.appendChild(oScript);

        // Demande #71 789 - Paramétrage de la propriété timestamp de CKEditor, afin de lui affecter le numéro de version actuel de l'application
        // Cette variable est ajoutée en queryString des plugin.js par CKEditor (?t=<TIMESTAMP>) afin de court-circuiter le cache navigateur lorsque le plugin est mis à jour
        // au même titre que notre variable ver= ajoutée sur les JS locaux ci-dessus
        // Il faut paramétrer cette variable ici en amont car CKEditor commencera à charger certains fichiers dès que possible, dès ajout de ses scripts dans le DOM, et
        // avant qu'on puisse le faire via eMemoEditor (cf. eMemoEditor.updateHTMLEditorTimestamp)
        if (typeof (CKEDITOR) != "undefined" && typeof (top._jsVer) != 'undefined' && top._jsVer != "" && CKEDITOR.timestamp != top._jsVer)
            CKEDITOR.timestamp = top._jsVer;
    }
}

/// Ajoute un link vers un JS
// lance éventuellement une fonction au chargement du fichier js
//
function addScript(href, stype, callback, oDoc) {
    addGenericScript(href, stype, "scripts", callback, oDoc)
}
/***********************************************************/


///ajout un script JS "Brut"
function addScriptText(sScript, oDoc) {
    if (!oDoc || typeof (oDoc) == 'undefined') {
        oDoc = document;
    }

    var oScript = document.createElement('script');
    oScript.type = 'text/javascript';
    oScript.text = sScript;
    oDoc.getElementsByTagName('body')[0].appendChild(oScript);

}




///Ajout du css "brut"
// Demande #72 138/#72 207 - Possibilité de préciser le document cible (iframe, ...) et d'ajouter à la fin/remplacer l'existant
function addCSSText(sStyle, targetDoc, cssId, appendIfExists) {
    if (!targetDoc)
        targetDoc = document;

    var isNewStyle = false;
    var myStyle = null;
    if (cssId && targetDoc.getElementById(cssId))
        myStyle = targetDoc.getElementById(cssId);
    else {
        myStyle = targetDoc.createElement('style');
        myStyle.id = cssId;
        isNewStyle = true;
    }

    myStyle.type = 'text/css';
    if (appendIfExists)
        myStyle.innerHTML += sStyle;
    else
        myStyle.innerHTML = sStyle;

    if (isNewStyle || appendIfExists)
        targetDoc.getElementsByTagName('head')[0].appendChild(myStyle);

}

// Ajout meta compatibilité Edge, s'il n'existe pas de meta http-equiv = "X-UA-Compatible"
function setEdgeCompatibility(targetDocument) {

    try {
        var metas = targetDocument.getElementsByTagName('meta');
        var found = false;
        for (var i = 0; i < metas.length; ++i) {
            var meta = metas[i];
            if (meta.getAttribute('http-equiv') === "X-UA-Compatible") {
                found = true;
                break;
            }
        }

        if (!found) {
            var newMeta = targetDocument.createElement("meta");
            newMeta.httpEquiv = "X-UA-Compatible";
            newMeta.content = "IE=edge";
            targetDocument.getElementsByTagName('head')[0].appendChild(newMeta);
        }
    }
    catch (e) {
    }

}

//Fonction de trim
function eTrim(myString) {
    myString += '';
    return myString.replace(/^\s+/g, '').replace(/\s+$/g, '');
}

function getXmlTextNode(oNode, sNodeName) {
    if (oNode == null || oNode == undefined)
        return "";

    if (sNodeName == null || sNodeName == undefined || typeof (sNodeName) != "string")
        sNodeName = "";
    else {
        var oNodeList = oNode.getElementsByTagName(sNodeName);
        if (oNodeList.length >= 1)
            oNode = oNodeList[0]
        else
            return "";
    }

    try {
        var sValue = "";
        if (oNode.text != null)
            sValue = oNode.text;
        else
            sValue = oNode.textContent;

        return sValue;
    }
    catch (e) {
        return "";
    }
}



/*
Cette fonction convertit un code clavier en description de touche
Utile, notamment, pour les touches système
Source : www.cambiaresearch.com/articles/15/javascript-char-codes-key-codes
*/
function getKeyDescFromCharCode(nCharCode) {
    if (nCharCode == 8) return "backspace"; //  backspace
    if (nCharCode == 9) return "tab"; //  tab
    if (nCharCode == 13) return "enter"; //  enter
    if (nCharCode == 16) return "shift"; //  shift
    if (nCharCode == 17) return "ctrl"; //  ctrl
    if (nCharCode == 18) return "alt"; //  alt
    if (nCharCode == 19) return "pause/break"; //  pause/break
    if (nCharCode == 20) return "caps lock"; //  caps lock
    if (nCharCode == 27) return "escape"; //  escape
    if (nCharCode == 33) return "page up"; // page up, to avoid displaying alternate character and confusing people	         
    if (nCharCode == 34) return "page down"; // page down
    if (nCharCode == 35) return "end"; // end
    if (nCharCode == 36) return "home"; // home
    if (nCharCode == 37) return "left arrow"; // left arrow
    if (nCharCode == 38) return "up arrow"; // up arrow
    if (nCharCode == 39) return "right arrow"; // right arrow
    if (nCharCode == 40) return "down arrow"; // down arrow
    if (nCharCode == 45) return "insert"; // insert
    if (nCharCode == 46) return "delete"; // delete
    if (nCharCode == 91) return "left window"; // left window
    if (nCharCode == 92) return "right window"; // right window
    if (nCharCode == 93) return "select key"; // select key
    if (nCharCode == 96) return "numpad 0"; // numpad 0
    if (nCharCode == 97) return "numpad 1"; // numpad 1
    if (nCharCode == 98) return "numpad 2"; // numpad 2
    if (nCharCode == 99) return "numpad 3"; // numpad 3
    if (nCharCode == 100) return "numpad 4"; // numpad 4
    if (nCharCode == 101) return "numpad 5"; // numpad 5
    if (nCharCode == 102) return "numpad 6"; // numpad 6
    if (nCharCode == 103) return "numpad 7"; // numpad 7
    if (nCharCode == 104) return "numpad 8"; // numpad 8
    if (nCharCode == 105) return "numpad 9"; // numpad 9
    if (nCharCode == 106) return "multiply"; // multiply
    if (nCharCode == 107) return "add"; // add
    if (nCharCode == 109) return "subtract"; // subtract
    if (nCharCode == 110) return "decimal point"; // decimal point
    if (nCharCode == 111) return "divide"; // divide
    if (nCharCode == 112) return "F1"; // F1
    if (nCharCode == 113) return "F2"; // F2
    if (nCharCode == 114) return "F3"; // F3
    if (nCharCode == 115) return "F4"; // F4
    if (nCharCode == 116) return "F5"; // F5
    if (nCharCode == 117) return "F6"; // F6
    if (nCharCode == 118) return "F7"; // F7
    if (nCharCode == 119) return "F8"; // F8
    if (nCharCode == 120) return "F9"; // F9
    if (nCharCode == 121) return "F10"; // F10
    if (nCharCode == 122) return "F11"; // F11
    if (nCharCode == 123) return "F12"; // F12
    if (nCharCode == 144) return "num lock"; // num lock
    if (nCharCode == 145) return "scroll lock"; // scroll lock
    if (nCharCode == 186) return ";"; // semi-colon
    if (nCharCode == 187) return "="; // equal-sign
    if (nCharCode == 188) return ","; // comma
    if (nCharCode == 189) return "-"; // dash
    if (nCharCode == 190) return "."; // period
    if (nCharCode == 191) return "/"; // forward slash
    if (nCharCode == 192) return "`"; // grave accent
    if (nCharCode == 219) return "["; // open bracket
    if (nCharCode == 220) return "\\"; // back slash
    if (nCharCode == 221) return "]"; // close bracket
    if (nCharCode == 222) return "'"; // single quote
    else return String.fromCharCode(nCharCode); // autres cas
}

/*
Cette fonction vérifie si un code clavier correspond à un caractère imprimable/affichable
Permet de distinguer les touches système des touches d'imprimerie
Source : cf. fonction getKeyDescFromCharCode
A compléter si des cas sont manquants
*/
function isWritableCharCode(nCharCode) {
    if (nCharCode == 96) return true; // numpad 0
    if (nCharCode == 97) return true; // numpad 1
    if (nCharCode == 98) return true; // numpad 2
    if (nCharCode == 99) return true; // numpad 3
    if (nCharCode == 100) return true; // numpad 4
    if (nCharCode == 101) return true; // numpad 5
    if (nCharCode == 102) return true; // numpad 6
    if (nCharCode == 103) return true; // numpad 7
    if (nCharCode == 104) return true; // numpad 8
    if (nCharCode == 105) return true; // numpad 9
    if (nCharCode == 106) return true; // multiply
    if (nCharCode == 107) return true; // add
    if (nCharCode == 109) return true; // subtract
    if (nCharCode == 110) return true; // decimal point
    if (nCharCode == 111) return true; // divide

    if (nCharCode == 186) return true; // ; semi-colon
    if (nCharCode == 187) return true; // = equal-sign
    if (nCharCode == 188) return true; // , comma
    if (nCharCode == 189) return true; // - dash
    if (nCharCode == 190) return true; // . period
    if (nCharCode == 191) return true; // / forward slash
    if (nCharCode == 192) return true; // ` grave accent
    if (nCharCode == 219) return true; // [ open bracket
    if (nCharCode == 220) return true; // \ back slash
    if (nCharCode == 221) return true; // ] close bracket
    if (nCharCode == 222) return true; // ' single quote

    // Autres cas
    return getKeyDescFromCharCode(nCharCode) === String.fromCharCode(nCharCode);
}

function isInt(n) {
    if (n == null || typeof (n) == "undefined" || (n + '') == '')
        return false;
    return !isNaN(parseInt(n)) && isFinite(n);
}

/*Permet d'attacher à un évennement de la page, une fonction.*/
setWindowEventListener = function (evt, callFct) {
    try {
        if (typeof window.addEventListener != 'undefined') {
            window.addEventListener(evt, callFct, false);
        }
        else if (typeof document.addEventListener != 'undefined') {
            document.addEventListener(evt, callFct, false);
        }
        else if (typeof document.attachEvent != 'undefined') {
            document.attachEvent("on" + evt, callFct);
        }
        else if (typeof window.attachEvent != 'undefined') {
            window.attachEvent("on" + evt, callFct);
        }
    } catch (Exc) {

    }

}

/*Permet d'attacher à un évennement de la page, une fonction.*/
removeWindowEventListener = function (evt, callFct) {
    try {
        if (typeof window.removeEventListener != 'undefined') {
            window.removeEventListener(evt, callFct, false);
        }
        else if (typeof document.removeEventListener != 'undefined') {
            document.removeEventListener(evt, callFct, false);
        }
        else if (typeof document.detachEvent != 'undefined') {
            document.detachEvent("on" + evt, callFct);
        }
        else if (typeof window.detachEvent != 'undefined') {
            window.detachEvent("on" + evt, callFct);
        }
    } catch (Exc) {

    }

}

///retire tous les event ajouté via setEventListener
removeAllListener = function (obj, evt) {
    try {
        if (Array.isArray(obj.listListener)) {
            obj.listListener.forEach(
                function (myelem) {
                    if (typeof (myelem.action) === "string" && typeof (evt) === "string") {
                        if (myelem.action === evt) {
                            unsetEventListener(obj, evt, myelem.fct);
                        }
                    }
                }
            );
        }

        return true;
    }
    catch (e) {
        return false;
    }
}

/*
Permet d'attacher un evennement à un objet qui execute la fonction passée en paramètre
obj : objet auquel on ajoute l'evennement
evt : nom de l'evennement (Click, move, dblclick...)
callFct : fonction à attacher
use Capture : si défini à true, useCapture indique que l'utilisateur désire initier la capture. Après avoir initié la capture, tous les évènements du type spécifié seront dispatchés vers l'EventListener avant d'être envoyés à toute cible (EventTarget) plus bas dans l'arbre. Les évènements qui se propagent vers le haut dans l'arbre (bubbling) ne déclencheront pas un EventListener désigné pour utiliser la capture. Consultez DOM Level 3 Events pour une explication détaillée.
*/
setEventListener = function (obj, evt, callFct, useCapture) {
    if (typeof (useCapture) == 'undefined' || useCapture == null)
        useCapture = false;

    if (typeof obj == 'undefined' || obj == null)
        return false;

    try {
        obj.listListener = obj.listListener || [];

        if (typeof obj.addEventListener != 'undefined') {
            obj.addEventListener(evt, callFct, useCapture);

            obj.listListener.push({ action: evt, fct: callFct })

            return true;
        }
        else if (typeof obj.attachEvent != 'undefined') {
            obj.listListener.push({ action: evt, fct: callFct })
            return true;
        }
        else {
            return false;
        }
    } catch (Exc) {
        return false;
    }
}


/*
Permet de détacher un evennement à l'objet la fonction passée en paramètre
obj : objet contenant l'evennement
evt : nom de l'evennement (Click, move, dblclick...)
callFct : fonction à détacher
*/
unsetEventListener = function (obj, evt, callFct, useCapture) {
    if (typeof (useCapture) == 'undefined' || useCapture == null)
        useCapture = false;

    try {
        if (typeof obj.removeEventListener != 'undefined') {
            obj.removeEventListener(evt, callFct, useCapture);
            return true;
        }
        else if (typeof obj.attachEvent != 'undefined') {
            obj.detachEvent("on" + evt, callFct);
            return true;
        }
        else
            return false;
    } catch (Exc) {
        return false;
    }
}

/*********************************************************/
/** GESTION DU TOOLTIP (TITLE)                          **/
/*********************************************************/

// fct : showTitleByElementId
function ste(e, elemId, customCssClass) {
    var oNode = document.getElementById(elemId);

    if (!oNode)
        return;

    if (oNode.tagName.toLowerCase() == "input") {
        if (!oNode.value || oNode.value == "")
            return;

        st(e, oNode.value, customCssClass);
    }
    else {
        if (!oNode.innerHTML || oNode.innerHTML == "")
            return;

        st(e, oNode.innerHTML, customCssClass);
    }
}

// fct : showTitle
/*
* e : event
* divInnerHtml : innerHtml du tooltip
* customCssClass : classe CSS spécifique à définir pour le tooltip
* doc : document des dimensions limite du tooltip - todo
*/
function st(e, divInnerHtml, customCssClass, doc) {
    var oDiv;
    var oDoc = document;

    try {
        if (divInnerHtml.trimLeft().trimRight() === "")
            return;
    }
    catch (e) { }

    var defaultClass = 'divShowTitle';
    if (typeof doc != 'undefined' && doc != null)
        oDoc = doc;

    if (customCssClass == null || customCssClass == '' || typeof (customCssClass) == 'undefined')
        customCssClass = defaultClass;
    else
        customCssClass = defaultClass + " " + customCssClass;

    if (oDoc.getElementById("idDivShowTitle")) {
        oDiv = oDoc.getElementById("idDivShowTitle");
        oDiv.className = customCssClass;
        oDiv.style.visibility = 'visible';
        oDiv.style.display = 'block';
    }
    else {
        oDiv = oDoc.createElement("div");
        oDiv.id = "idDivShowTitle";
        oDiv.className = customCssClass;
        oDiv.style.position = "absolute";
        setEventListener(oDiv, "click", function () { ht(); });
        oDoc.body.appendChild(oDiv);
    }


    // Encapsule le contenu pour redimensionner le div par rapport à la ballise HTML insérée
    oDiv.innerHTML = "<LABEL id=\"labGlobal\">" + divInnerHtml + "</LABEL>";

    var winSize = getWindowSize();
    var mouseSize = 22;

    // Position de la souris
    var tip = GetTip(e);


    var divPosX = tip.x;
    var divPosY = tip.y + mouseSize;      // Ajout de 22 pour faire apparaitre le tooltip sous la souris
    // Dimension de la div
    var divWidth = oDiv.offsetWidth;
    var divHeight = oDiv.offsetHeight;

    if (divPosX + divWidth > winSize.w) {
        // - 20 pour palier au problème du clientWidth
        divPosX = winSize.w - divWidth - mouseSize;
    }
    // Avec application d'une marge empêchant d'afficher la popup sur le curseur de la souris
    if (divPosY + divHeight > winSize.h) {
        //divPosY = winClientHeight - divHeight - (mouseSize * 2); // 22 : marge appliquée sur les coordonnées d'origine par rapport au curseur de la souris
        divPosY = divPosY - divHeight - (mouseSize * 2);
    }

    // Pas de valeur negative (ni undefined sous IE 8)
    if (divPosX < 0 || !divPosX || typeof (divPosX) == "undefined")
        divPosX = 0;
    if (divPosY < 0 || !divPosY || typeof (divPosY) == "undefined")
        divPosY = 0;

    oDiv.style.left = divPosX + 'px';
    oDiv.style.top = divPosY + 'px';
    // HLA - ne pas définir de taille pour eviter le mauvais calcule lors des affichages suivant de showTitle
    /*oDiv.style.width = divWidth + 'px';
    oDiv.style.height = divHeight + 'px';*/
}

// fct : hideTitle
function ht(doc) {
    var oDoc = document;
    if (typeof doc != 'undefined' && doc != null)
        oDoc = doc;

    if (!oDoc.getElementById("idDivShowTitle"))
        return;

    var oDiv = oDoc.getElementById("idDivShowTitle");
    oDiv.style.visibility = 'hidden';
    oDiv.style.display = 'none';
}

// Structure de donnée qui permet d'encapsuler la largeur et la hauteur de la fenetre
// permet de faire de manipulation geometrique sur la dimension
function eWinSize(options) {
    var that = this;

    options = options || { w: 0, h: 0 };

    this.w = options.w || 0;
    this.dw = options.w || 0;
    this.h = options.h || 0;
    this.isTablet = isTablet();

    // ajout ou retire des quantiés à la largeur et la hauteur 
    this.add = function (options) {

        if (!options)
            return that;

        options.dx = options.dx || 0;
        options.dy = options.dy || 0;

        that.w += options.dx;
        that.h += options.dy;

        return that;
    };

    // personalise la taille
    this.set = function (options) {
        if (!options)
            return that;

        that.w = options.w || that.w;
        that.h = options.h || that.h;

        return that;
    };

    // transformation d'échèlles
    this.scale = function (value) {
        return that.scaleHW({ sw: value, sh: value });
    };

    // transformation d'échèlles
    this.scaleHW = function (options) {

        if (!options)
            return that;

        options.sw = options.sw || 1;
        options.sh = options.sh || 1;

        // value doit etre positif   
        if (options.sw > 0) {
            that.w *= options.sw;
        }

        // value doit etre positif   
        if (options.sh > 0) {
            that.h *= options.sh;
        }

        return that;
    };

    // La taille minimale pour afficher correctement la fenetre
    this.min = function (options) {

        if (!options || that.isTablet)
            return that;

        options.w = options.w || that.w;
        options.h = options.h || that.h;
        if (that.w <= options.w) that.w = options.w;
        if (that.h <= options.h) that.h = options.h;

        return that;
    };

    // La taille max pour afficher correctement la fenetre
    this.max = function (options) {

        if (!options || that.isTablet)
            return that;

        options.w = options.w || that.w;
        options.h = options.h || that.h;
        if (that.w >= options.w) that.w = options.w;
        if (that.h > options.h) that.h = options.h;

        return that;
    };

    // Défault si la taille petite ou tablette
    this.tablet = function (options) {
        if (that.isTablet && options) {
            options.w = options.w || that.w;
            options.h = options.h || that.h;
            if (that.w <= options.w) that.w = options.w;
            if (that.h <= options.h) that.h = options.h;
        }

        return that;
    };
}

function getWindowSize() {
    var winClientWidth = 0;
    var winClientHeight = 0;

    // POUR IE < 9  
    if (typeof (window.innerWidth) != 'undefined') {
        winClientWidth = parseInt(window.innerWidth);

        winClientHeight = parseInt(window.innerHeight);
    }

    else //pour IE8
        if (document.documentElement) {
            if (document.documentElement.clientWidth)
                winClientWidth = parseInt(document.documentElement.clientWidth);
            else if (document.documentElement.offsetWidth)
                winClientWidth = parseInt(document.documentElement.offsetWidth);

            if (document.documentElement.clientHeight)
                winClientHeight = parseInt(document.documentElement.clientHeight);
            else if (document.documentElement.offsetHeight)
                winClientHeight = parseInt(document.documentElement.offsetHeight);
        }
        else
            if (document.body) {
                if (document.body.clientWidth)
                    winClientWidth = parseInt(document.body.clientWidth);
                else if (document.body.offsetWidth)
                    winClientWidth = parseInt(document.body.offsetWidth);

                if (document.body.clientHeight)
                    winClientHeight = parseInt(document.body.clientHeight);
                else if (document.body.offsetHeight)
                    winClientHeight = parseInt(document.body.offsetHeight);
            }
            else
                if (window.screen && typeof (window.screen) == "object") {
                    if (window.screen.width)
                        winClientWidth = parseInt(window.screen.width);
                    if (window.screen.height)
                        winClientHeight = parseInt(window.screen.height);
                }

    if (winClientWidth <= 0 || winClientHeight <= 0)
        throw "getWindowSize invalid";


    var rightMenuWidth = GetRightMenuWidth();

    var size = new eWinSize();

    //largeur de la fenetre principale
    size.w = winClientWidth;

    //largeur sans le menu escamotable
    size.dw = size.w - rightMenuWidth;

    //hauteur de la fenetre principale
    size.h = winClientHeight;

    // pour les tablettes
    if (size.isTablet)
        return AdapteToViewportSize(size, rightMenuWidth);

    return size;
}


// Rretourne la largeur du menu escamotable
// Pour info : la largeur de la div RightMenu ne prend pas en compte la largeur de la div enfant : menuBar qui est en position absolute.
function GetRightMenuWidth() {
    var rightMenu = document.getElementById("rightMenu");
    if (rightMenu) {
        var menuBar = document.getElementById("menuBar");
        var pinned = getAttributeValue(menuBar, "pinned") == "1";
        if (pinned === true) {
            if (rightMenu.clientWidth)
                return rightMenu.clientWidth + menuBar.clientWidth;
            else if (rightMenu.offsetWidth)
                return rightMenu.offsetWidth + menuBar.offsetWidth;
        }
    }

    return 0;
}

// Retourne les dimensions du viewport
function AdapteToViewportSize(winSize, rightMenuWidth) {
    var winClientWidth = 0;
    var winClientHeight = 0;
    if (document.documentElement) {
        if (document.documentElement.clientWidth)
            winClientWidth = parseInt(document.documentElement.clientWidth);
        else if (document.documentElement.offsetWidth)
            winClientWidth = parseInt(document.documentElement.offsetWidth);

        if (document.documentElement.clientHeight)
            winClientHeight = parseInt(document.documentElement.clientHeight);
        else if (document.documentElement.offsetHeight)
            winClientHeight = parseInt(document.documentElement.offsetHeight);
    }
    else
        if (window.screen && typeof (window.screen) == "object") {
            if (window.screen.width) {
                winClientWidth = window.screen.width;
                winClientHeight = window.screen.height;
            }
        }

    if (winClientHeight > 0) {
        winSize.h = winClientHeight;
    }
    if (winClientWidth > 0) {
        winSize.w = winClientWidth;
        winSize.dw = winSize.w - rightMenuWidth
    }
    return winSize;
}

function GetTip(e) {
    var px = 0, py = 0;

    if (window.event && window.event.clientX)
        px = window.event.clientX;
    else
        px = e.pageX;

    if (window.event && window.event.clientY)
        py = window.event.clientY;
    else
        py = e.pageY;

    var obj = new Object();
    obj.x = px;
    obj.y = py;

    return obj;
}

/*********************************************************/
/** FIN DE GESTION DU TOOLTIP (TITLE)                   **/
/*********************************************************/


function changeBitField(elem, forceValue) {

    if (!elem)
        return;


    var bForceValue = forceValue != null && typeof (forceValue) != 'undefined';

    if (!bForceValue && elem.getAttribute("dis") == "1")
        return;

    var sSetChk = '';

    if ((!bForceValue && elem.getAttribute("chk") == "1") || (bForceValue && !forceValue)) {
        sSetChk = '0';
    }
    else {
        sSetChk = '1';
    }
    elem.setAttribute("chk", sSetChk);
}

function disableBitField(elem, forceValue) {

    if (!elem)
        return;


    var bForceValue = forceValue != null && typeof (forceValue) != 'undefined';

    var sSetDis = '';

    if ((!bForceValue && elem.getAttribute("dis") == "1") || (bForceValue && !forceValue)) {
        sSetDis = '0';
    }
    else {
        sSetDis = '1';
    }
    elem.setAttribute("dis", sSetDis);
}

// Changement de la CSS de la checkbox clickée pour la cocher/décocher
// forceValue :
//  - si indéfini : inverse l'état de la checkbox, sauf si elle est désactivée
//  - si false : décoche la case
//  - si true : coche la case
function chgChk(elem, forceValue) {
    if (elem == null || typeof (elem) == "undefined")
        return;

    changeBitField(elem, forceValue);

    var sSetChk = getAttributeValue(elem, "chk");

    var item;

    if (getAttributeValue(elem, "swap") == "1" && elem.children && elem.children.length > 1)
        item = elem.children[1];
    else if (elem && elem.firstChild)
        item = elem.firstChild;

    var sCheckCss = getCheckIconClass(elem.getAttribute("dis"), sSetChk);

    //retirer l'ancienne classe spécifique checkbox
    removeClass(item, "icon-square-o icon-check-square-o icon-check-square icon-square");

    item.className = sCheckCss + " " + item.className;



}

// Changement de la CSS de la checkbox indiquée pour l'activer/désactiver
// forceValue :
//  - si indéfini : inverse l'état de la checkbox
//  - si false : active la case
//  - si true : désactive la case
function disChk(elem, forceValue) {

    disableBitField(elem, forceValue);

    var sSetDis = getAttributeValue(elem, "dis");

    if (elem.firstChild)
        elem.firstChild.className = getCheckIconClass(sSetDis, elem.getAttribute("chk"));
}

function getCheckIconClass(bDisabled, bChecked) {
    var iconClass = "icon-square-o";


    if (bChecked == "1") {
        iconClass = (bDisabled == "1") ? "icon-check-square-o" : "icon-check-square";
    }
    else if (bDisabled == "1") {
        iconClass = "icon-square";
    }
    return iconClass;
}

/* BOUTONS LOGIQUES */

function changeBtnBit(elem, forceValue) {
    changeBitField(elem, forceValue);
}

function disableBtnBit(elem, forceValue) {

    disableBitField(elem, forceValue);
}

/* CATALOGUES ETAPES */

function selectStep(elem) {
    if (!elem)
        return;
    var dbv = getAttributeValue(elem, "dbv");

    var td = findUp(elem, "TD");
    selectStepDbv(dbv, td);
}

function selectStepDbv(dbv, td, bForce) {

    if (getAttributeValue(td, "ero") == "1")
        return;

    var selectedLi = null;

    // Forcer la sélection
    if (typeof (bForce) === "undefined")
        bForce = false;

    var ul = td.querySelector("ul");
    var listLi = ul.children;
    var pTooltip = td.querySelector("p.catalogTooltip");
    var tooltip = "";
    var bSequenceMode = getAttributeValue(ul, "seq") == "1";
    var bBeforeValue = true;

    for (var i = 0; i < listLi.length; i++) {
        if (dbv == getAttributeValue(listLi[i].children[0], "dbv")) {
            selectedLi = listLi[i];
            if (hasClass(listLi[i], "selectedValue") && !bForce) {
                removeClass(listLi[i], "selectedValue");
            }
            else {
                addClass(listLi[i], "selectedValue");
                tooltip = getAttributeValue(listLi[i], "title");
                bBeforeValue = false;
            }
        }
        else {
            if (hasClass(listLi[i], "selectedValue")) {
                removeClass(listLi[i], "selectedValue");
            }

            if (bSequenceMode && bBeforeValue)
                addClass(listLi[i], "beforeSelectedValue");
            else
                removeClass(listLi[i], "beforeSelectedValue");
        }
    }
    // Si aucune valeur sélectionnée, on retire la classe "beforeSelectedValue" sur toutes les valeurs
    if (bBeforeValue) {
        for (var j = 0; j < listLi.length; j++) {
            removeClass(listLi[j], "beforeSelectedValue");
        }
    }
    // Mise à jour de l'infobulle (texte court)
    SetText(pTooltip, tooltip);

    return selectedLi;
}

//Déclenche une recherche de liste côté client en filtrant les données d'un tableau HTML
function updateTableFromSearch(sValue, oTargetTable, myFunct, myFunctError) {
    try {
        if (oTargetTable != null && oTargetTable.rows != null) {
            for (var i = 1, row; row = oTargetTable.rows[i]; i++) { /* i = 1 pour ignorer la première ligne (entête) */
                matchingRow = (sValue == ''); // si le champ de recherche est vide, on affiche la ligne, donc on effectue aucune recherche
                if (!matchingRow) {
                    for (var j = 0, col; col = row.cells[j]; j++) {
                        if (GetText(col).toLowerCase().indexOf(sValue.toLowerCase()) > -1) {
                            matchingRow = true;
                            j = row.cells.length;
                        }
                        else {
                            var input = findElementByTagName(col.children, "INPUT");
                            if (input != null) {
                                if (input.value.toLowerCase().indexOf(sValue.toLowerCase()) > -1) {
                                    matchingRow = true;
                                    j = row.cells.length;
                                }
                            }

                        }
                    }
                }
                row.style.display = (matchingRow ? "" : "none");
            }
        }

        if (typeof (myFunct) == "function")
            myFunct();
    }
    catch (ex) {
        if (typeof (myFunctError) == "function")
            myFunctError(ex);
    }
}

//Met à jour les uservalues
function updateUserValue(sValue, myFunct, myFunctError) {
    updateUserPrefGbl('mgr/eUserValueManager.ashx', sValue, myFunct, myFunctError);
}

//Met à jour les prefs
function updateUserPref(prefFldVal, myFunct) {
    updateUserPrefGbl('mgr/ePrefManager.ashx', 'typ=USER_PREF;$;' + prefFldVal, myFunct, function () { });
}


//Met à jour les prefs
function updateColsPref(prefFldVal, myFunct) {
    updateUserPrefGbl('mgr/ePrefManager.ashx', 'typ=USER_COLSPREF;$;' + prefFldVal, myFunct, function () { });
}

//Met à jour les Bkm prefs
function updateUserBkmPref(prefFldVal, myFunct) {
    updateUserPrefGbl('mgr/ePrefManager.ashx', 'typ=USER_BKM_PREF;$;' + prefFldVal, myFunct, function () { });
}

//Met à jour les prefs de l'ecran de recherche
function updateUserFinderPref(prefFldVal, myFunct) {
    updateUserPrefGbl('mgr/ePrefManager.ashx', 'typ=USER_FINDER_PREF;$;' + prefFldVal, myFunct, function () { });
}

//Met à jour la selection
function updateUserSelection(prefFldVal, myFunct, myFunctError) {
    updateUserPrefGbl('mgr/ePrefManager.ashx', 'typ=USER_SELECTION;$;' + prefFldVal, myFunct, myFunctError);
}

//Met à jour la selection d'onglet
function updateUserTabs(prefFldVal, myFunct, myFunctError) {
    updateUserPrefGbl('mgr/eTabsManager.ashx', prefFldVal, myFunct, myFunctError);
}

//Met à jour les prefs Adv
function updateUserPrefAdv(prefFldVal, myFunct, myFunctError) {
    updateUserPrefGbl('mgr/ePrefAdvConfigAdvManager.ashx', prefFldVal, myFunct, myFunctError);
}

// Met à jour les prefs des widgets de type liste
function updateListWidgetPref(prefFldVal, myFunct) {
    updateUserPrefGbl('mgr/ePrefManager.ashx', 'typ=USER_LISTWIDGET_PREF;$;' + prefFldVal, myFunct, function () { });
}

// Met à jour les prefs des profils utilisateurs
//Update user's profile prefs
function updateUserProfilePref(prefFldVal, myFunct) {
    updateUserPrefGbl('mgr/ePrefManager.ashx', 'typ=USER_PROFILE_PREF;$;' + 'src=' + prefFldVal, myFunct, function () { });
}


//pref : champs=valeurs (séparés par ;$;)
//myFunct : fonction javascript lancé après la mise à jour en base pour le reload de la page
//myErrorCallBack : Fonction js lancé en cas d'erreur
function updateUserPrefGbl(file, prefFldVal, myFunct, myErrorCallBack) {
    var prefFld, prefValue;
    var ednu = new eUpdater(file, 0);

    //boucle sur tous les params
    var aFields = prefFldVal.split(";$;");
    for (var i = 0; i < aFields.length; i++) {

        //ALISTER Demande / Request 81142 Remplace le 1er "=" trouvé / Replace 1st occurence of "=" char found
        aPref = aFields[i].replace(/\=/, "&=").split("&=")

        prefFld = aPref[0];
        prefValue = "";
        if (aPref.length == 2)
            prefValue = aPref[1];

        ednu.addParam(prefFld, prefValue, "post");
    }

    if (typeof (myErrorCallBack) == "function") {
        ednu.ErrorCallBack = myErrorCallBack;
    }

    //Envoi de l'update vers la serveur
    ednu.send(doUpdateUserTreatment, myFunct);
}

function userProfilePrefCat(objBtn, idEditField, bMulti, bProfil, bProfilOnly, bIsMyEudonet) {
    var oTarget = document.getElementById(idEditField);
    modalUserCat = new eModalDialog(top._res_246, 0, "eCatalogDialogUser.aspx", 550, 610);
    modalUserCat.ErrorCallBack = function () { setWait(false); }


    if (typeof bProfil == "undefined")
        var bProfil = 0;

    if (typeof bMulti == "undefined")
        var bMulti = 0;

    if (typeof bProfilOnly == "undefined")
        var bProfilOnly = 0;



    modalUserCat.addParam("iframeId", modalUserCat.iframeId, "post");
    modalUserCat.addParam("showdefaultvalue", idEditField === "EDA_CNX_AS" ? "0" : "1", "post");
    modalUserCat.addParam("multi", bMulti ? "1" : "0", "post");
    modalUserCat.addParam("profil", bProfil ? "1" : "0", "post");
    modalUserCat.addParam("onlyprofil", bProfilOnly ? "1" : "0", "post");
    modalUserCat.addParam("ismyeudonet", bIsMyEudonet ? "1" : "0", "post");
    modalUserCat.addParam("selected", getAttributeValue(oTarget, "ednvalue"), "post");
    modalUserCat.addParam("modalvarname", "modalUserCat", "post");
    modalUserCat.onIframeLoadComplete = function () {

        //Désactive les entrées sélectionnées dans l'autres combox (interdiction de recopier des pref sur soit même)
        var oValSrc = "";
        if (idEditField === "EDA_CPREF_DST")
            oValSrc = getAttributeValue(document.getElementById('EDA_CPREF_SRC'), "ednvalue");
        else if (idEditField === "EDA_CNX_AS")
            oValSrc = getAttributeValue(document.getElementById('EDA_CNX_AS'), "ednvalue");
        else
            oValSrc = getAttributeValue(document.getElementById('EDA_CPREF_DST'), "ednvalue");

        var content = modalUserCat.getIframe().document;
        if (typeof oValSrc === "string") {
            var arroValSrc = oValSrc.split(";");
            arroValSrc.forEach(
                function (val) {
                    var liSrc = content.getElementById("eTVB_" + val);
                    if (liSrc)
                        liSrc.style.display = "none";
                });
        }
    }

    modalUserCat.show();

    top.eTabCatUserModalObject.Add(modalUserCat.iframeId, modalUserCat);

    modalUserCat.addButton(top._res_29, function () {
        modalUserCat.hide();
    }, "button-gray", null, null, true);


    modalUserCat.CallOnOk = function () { onUserProfilePrefCatOk(oTarget) };
    modalUserCat.addButton(top._res_28, modalUserCat.CallOnOk, "button-green");
}

function onUserProfilePrefCatOk(oRes) {

    var strReturned = modalUserCat.getIframe().GetReturnValue();
    var vals = strReturned.split('$|$')[0];
    var libs = strReturned.split('$|$')[1];

    setAttributeValue(oRes, "ednvalue", vals);
    setAttributeValue(oRes, "value", libs);

    modalUserCat.hide();
}

function doUpdateUserTreatment(oRes, myFunct) {
    if (!getXmlTextNode)
        return;
    var strSuccess = getXmlTextNode(oRes.getElementsByTagName("result")[0]);
    var strCallBack = getXmlTextNode(oRes.getElementsByTagName("callback")[0]);

    if (strSuccess != "SUCCESS") {
        // EN ERREUR
        var strErrDesc = getXmlTextNode(oRes.getElementsByTagName("errordescription")[0]);
        alert('ERREUR : ' + strErrDesc);        // DEBUG
        return;
    }
    else {
        // DEBUG
        /*
        if (oRes.getElementsByTagName("updSelection")[0]) {
        var sUpdSel = "";
        for (var nI = 0; nI < oRes.getElementsByTagName("updSelection")[0].attributes.length; nI++)
        sUpdSel += ";" + oRes.getElementsByTagName("updSelection")[0].attributes[nI].name + '=' + oRes.getElementsByTagName("updSelection")[0].attributes[nI].value;
        alert(sUpdSel);
        }
        */

        if (typeof myFunct == "function") {

            myFunct();
        }
        else {
            // Aucune action

        }
    }
}

//Fonction d'encodage
function encode(strValue) {
    var strReturnValue;

    if (strValue == "" || strValue == null)
        return "";

    try {
        strReturnValue = encodeURIComponent(strValue);
        strReturnValue = strReturnValue.replace(/'/g, "%27");
    }
    catch (e) {
        strReturnValue = escape(strValue);
    }
    return strReturnValue;
}

function decode(strValue) {
    var strReturnValue;

    if (strValue == "" || strValue == null)
        return "";

    //strValue = strValue.replace( /\%27/g, "'" );
    try {
        strReturnValue = decodeURIComponent(strValue);
    }
    catch (e) {
        strReturnValue = unescape(strValue);
    }
    return strReturnValue;
}

// Renvoie une version de la chaîne passée en paramètre sans balises HTML
// TODO: a améliorer, ex : "Le bilan est < à 70 mais > 60" --> "Le bilan est  60"
function removeHTML(strHTML) {

    if (strHTML == "" || strHTML == null)
        return "";

    var strText = strHTML;
    var re = new RegExp("<br[^>]*>", "gmi");
    strText = strText.replace(re, "\n");
    var re = new RegExp("<[^>]*>", "gmi");
    strText = strText.replace(re, "");
    var re = new RegExp("&nbsp;", "gmi");
    strText = strText.replace(re, " ");
    return strText;
}

function decodeHTMLEntities(strHTML) {
    var str, temp = document.createElement('p');
    temp.innerHTML = strHTML;
    str = temp.textContent || temp.innerText;
    temp = null;
    return str;
}


function encodeHTMLEntities(strHTML) {
    strHTML += "";

    // insuffisant, n'encode pas tout
    //return document.createElement('div').appendChild(document.createTextNode(strHTML)).parentNode.innerHTML;


    return strHTML.
        replace(/&/g, '&amp;').
        replace(/[\uD800-\uDBFF][\uDC00-\uDFFF]/g, function (strHTML) {
            var hi = strHTML.charCodeAt(0);
            var low = strHTML.charCodeAt(1);
            return '&#' + (((hi - 0xD800) * 0x400) + (low - 0xDC00) + 0x10000) + ';';
        }).
        replace(/([^\#-~| |!])/g, function (strHTML) {
            return '&#' + strHTML.charCodeAt(0) + ';';
        }).
        replace(/</g, '&lt;').
        replace(/>/g, '&gt;');

}



function convertStringToRewrittenUrl(label, nMaxLength) {
    if (typeof (nMaxLength) == "undefined" || isNaN(nMaxLength))
        nMaxLength = 50;
    var str = label;
    str = str.toLowerCase()
    var accent = [/[\340-\346]/g, // a
        /[\350-\353]/g, // e
        /[\354-\357]/g, // i
        /[\362-\370]/g, // o
        /[\371-\374]/g, // u
        /[\361]/g, // n
        /[\347]/g // c
    ];
    var noaccent = ['a', 'e', 'i', 'o', 'u', 'n', 'c'];
    for (var i = 0; i < accent.length; i++) {
        str = str.replace(accent[i], noaccent[i]);
    }
    str = str.substring(0, str.Length <= nMaxLength ? str.length : nMaxLength);



    return str
        .toLowerCase()
        .replace(/ /g, '-')
        .replace(/[^\w-]+/g, '')
        ;
}

/// Convertit une chaîne de type String en int
/// A utiliser, par exemple, pour convertir "122px", "100%", "15pt" en 122, 100 et 15
function getNumber(value) {
    try {
        value += ''; // conversion en String
        value = value.replace(/ /g, "");
        var numValue = parseInt(value, 10);
        if (isNaN(numValue)) {
            numValue = parseFloat(value);
        }
        return numValue;
    }
    catch (e) {
        return NaN; // permet de faire des tests avec isNaN()
    }
}

// Get the NodeList and transform it into an array
function getArrayFromTag(oNode, tagname) {
    // Array.prototype.slice.call ne fonctionne pas sur <= IE8
    //return Array.prototype.slice.call(oNode.getElementsByTagName(tagname));

    var oNodes = oNode.getElementsByTagName(tagname);
    var results = new Array();
    for (var i = 0; i < oNodes.length; i++)
        results.push(oNodes[i]);
    return results;
}


//////////////////////////////////////////
//// Méthodes d'extension Javascript /////
//////////////////////////////////////////
if (!Array.prototype.indexOf) {
    Array.prototype.indexOf = function (searchElement /*, fromIndex */) {
        "use strict";

        if (this === void 0 || this === null)
            throw new TypeError();

        var t = Object(this);
        var len = t.length >>> 0;
        if (len === 0)
            return -1;

        var n = 0;
        if (arguments.length > 0) {
            n = Number(arguments[1]);
            if (n !== n) // shortcut for verifying if it's NaN
                n = 0;
            else if (n !== 0 && n !== (1 / 0) && n !== -(1 / 0))
                n = (n > 0 || -1) * Math.floor(Math.abs(n));
        }

        if (n >= len)
            return -1;

        var k = n >= 0
            ? n
            : Math.max(len - Math.abs(n), 0);

        for (; k < len; k++) {
            if (k in t && t[k] === searchElement)
                return k;
        }

        return -1;
    };
}


// Dean's forEach: http://dean.edwards.name/base/forEach.js
/*
forEach, version 1.0
Copyright 2006, Dean Edwards
License: http://www.opensource.org/licenses/mit-license.php
*/
// Array-like enumeration
// MAB - #60 518 et #63 849 - 2018-03-02 - Attention, forEach ne fonctionne pas sur les objets de type NodeList sur les anciens navigateurs et IE
// Source: https://stackoverflow.com/questions/13433799/why-doesnt-nodelist-have-foreach
// Utiliser, dans ce cas, Array.prototype.slice.call(nodelist).forEach(...)

if (!Array.forEach) { // mozilla already supports this
    Array.forEach = function (array, block, context) {
        for (var i = 0; i < array.length; i++) {
            block.call(context, array[i], i, array);
        }
    };
}
// On pourrait utiliser la copie (mutation) de prototype, comme ceci, mais ça n'est pas recommandé pour IE (encore lui) :
// NodeList.prototype.forEach = Array.prototype.forEach;

// Generic enumeration
Function.prototype.forEach = function (object, block, context) {
    for (var key in object) {
        if (typeof this.prototype[key] == "undefined") {
            block.call(context, object[key], key, object);
        }
    }
};

// Character enumeration
String.forEach = function (string, block, context) {
    Array.forEach(string.split(""), function (chr, index) {
        block.call(context, chr, index, string);
    });
};

// Globally resolve forEach enumeration
var forEach = function (object, block, context) {
    if (object) {
        var resolve = Object; // default
        if (object instanceof Function) {
            // functions have a "length" property
            resolve = Function;
        } else if (object.forEach instanceof Function) {
            // the object implements a custom forEach method so use that
            object.forEach(block, context);
            return;
        } else if (typeof object == "string") {
            // the object is a string
            resolve = String;
        } else if (typeof object.length == "number") {
            // the object is array-like
            resolve = Array;
        }

        resolve.forEach(object, block, context);
    }
};

if (!Array.prototype.removeValue) {
    Array.prototype.removeValue = function () {
        var what, a = arguments, L = a.length, ax;
        while (L && this.length) {
            what = a[--L];
            while ((ax = this.indexOf(what)) !== -1) {
                this.splice(ax, 1);
            }
        }
        return this;
    };
}

//////////////////////////////////////////
// Fin Méthodes d'extension Javascript ///
//////////////////////////////////////////

//////////////////////////////////////////////////////////////////////
// Méthode de gestion des touches de validations (exemple : ENTER) ///
// onkeypress="if(isValidationKey(event))MAFONCTION();" 
//////////////////////////////////////////////////////////////////////
function isValidationKey(event) {
    var retour = (event.keyCode == 13);
    return retour;
}

//////////////////////////////////////////////////////////////////////////
// Méthode qui retourne si la chaine en parametre est un entier ou non ///
//////////////////////////////////////////////////////////////////////////
function isNumeric(n) {
    if (typeof (n) == 'undefined' || !n)
        return false;
    var retour = (n.toString().search(/^-?[0-9]+$/)) > -1;
    return retour;
}



function getIdxArray(myArray, myValue) {




    if (myArray instanceof Array) {

        //Fonction navtive IE9+
        if (myArray.indexOf)
            return myArray.indexOf(myValue);





        for (var nCmpt = 0; nCmpt < myArray.length; nCmpt++) {

            if (myArray[nCmpt] === myValue) {
                return nCmpt;
            }
        }
    }

    return -1;
}



function getCookie(name) {
    var start = document.cookie.indexOf(name + "=");
    var len = start + name.length + 1;
    if ((!start) && (name != document.cookie.substring(0, name.length))) return null;
    if (start == -1) return null;
    var end = document.cookie.indexOf(";", len);
    if (end == -1) end = document.cookie.length;
    return document.cookie.substring(len, end);
}

function setCookie(cookieName, cookieValue, bReset) {
    var expDate = new Date();
    expDate.setTime(expDate.getTime() + (360 * 24 * 3650000));
    if (bReset == true) document.cookie = '';
    document.cookie = cookieName + "=" + cookieValue + ";path=/; expires=" + expDate.toGMTString();
}

function getRandomChar() {
    var chars = "0123456789abcdefghijklmnopqurstuvwxyzABCDEFGHIJKLMNOPQURSTUVWXYZ";
    return chars.substr(getRandomNumber(62), 1);
}

function getRandomNumber(range) {
    return Math.floor(Math.random() * range);
}

function randomID(size) {
    var str = "";
    for (var i = 0; i < size; i++) {
        str += getRandomChar();
    }
    return str;
}

function getFunctionName(func) {
    if (typeof func == "function" || typeof func == "object")
        var fName = ("" + func).match(
            /function\s*([\w\$]*)\s*\(/
        ); if (fName !== null) return fName[1];
}

//*********************************************************************************************************************
//***Fonction principale de création du calendrier
//*********************************************************************************************************************
//   op : uniquement dans le cas des filtre express // mettre null dans les autre cas
//   strJsValidAction : nom de la fonction de validation// sera appelée avec paramètre la date choisie
//   nHideNoDate : 0/1 cahcer le bouton "aucune date"
//   nHideHourField : 0/1 cacher le champs heure/minute
//   title : titre de la fenêtre
//   lblBtnOk : label du bouton OK
//   onCalendarOkFunction : fonction onClickOk 
//   lblBtnCancel : label du bouton cancel
//   onCalendarCancelFunction  : fonction onClickCancel (attention: il s'agit d'une fonction et non pas d'une chaine)
//*********************************************************************************************************************
function createCalendarPopUp(strJsValidAction, nHideNoDate, nHideHourField, title, lblBtnOk, onCalendarOkFunction, lblBtnCancel, onCalendarCancelFunction, op, frmId, nodeId, date) {
    var strType = '0';
    var strUrl = 'eCalendar.aspx';
    var nWidth = 275; /* CRU : Augmentation de la largeur pour que les boutons rentrent sur la popup */


    var nHeight = 385;

    if (top.eTools.GetFontSize() >= 14) {
        nWidth = 345;
        nHeight = 425;
    }

    if (nHideHourField == 0)
        nHeight += 90;
    if (nHideNoDate == 0)
        nHeight += 15;

    var modalVar = new eModalDialog(title, strType, strUrl, nWidth, nHeight);
    var windowName = (window.name == 'SeriesType') ? '' : window.name;
    modalVar.ErrorCallBack = function () { setWait(false); }

    modalVar.addParam("HideNoDate", nHideNoDate, "post");
    modalVar.addParam("HideHourField", nHideHourField, "post");


    modalVar.addParam("JsValidAction", strJsValidAction, "post");

    modalVar.addParam("JsOkButtonAction", onCalendarOkFunction, "post");


    modalVar.addParam("operator", op, "post");
    modalVar.addParam("frmId", windowName, "post");
    if (frmId != "") {
        modalVar.addParam("parentframeid", frmId, "post");
    }


    // Element de départ de la modification de valeur
    if (typeof (nodeId) != 'undefined' && nodeId != "")
        modalVar.addParam("nodeId", nodeId, "post");
    // Valeur d'origine de la rubrique en cours de modification
    //GCH - #36019 - Internationnalisation - Choix de dates
    if (typeof (date) != 'undefined' && date != "") {
        modalVar.addParam("date", eDate.ConvertDisplayToBdd(date), "post");
    }

    // Classe spécifique pour l'espacement des boutons à droite
    modalVar.btnSpacerClass = "actBtnLstCal";

    modalVar.show();

    modalVar.addButton(lblBtnCancel, onCalendarCancelFunction, 'button-gray', null, "cancel");

    if (typeof onCalendarOkFunction == 'function')
        modalVar.addButtonFct(lblBtnOk, onCalendarOkFunction, 'button-green', null);
    else
        modalVar.addButton(lblBtnOk, onCalendarOkFunction, 'button-green', null);

    return modalVar;
}

/*
var example = new eDateTimePicker({
    "title": "Titre de la fenêtre",
    "okLabel": "Valider",
    "cancelLabel": "Annuler",
    "hideEmptyDate": true,
    "hideHoursAndMinutes": true,
    "value": "",
    "onValidate": onValidate, // pour valider la valeur   
    "onCancel": onCancel // pour annuler
});
*/

function eDateTimePicker(data) {

    var __modal = null;
    if (!data.value)
        data.value = (new Date()).toString();


    // function de vérification
    top.__validateDate__ = function validateDate(date) {

        top.__validateDate__ = "";
        if (typeof (data.onValidate) == 'function') {
            data.onValidate(date);
        }

        if (__modal)
            __modal.hide();
    };

    // function d'annulation
    var __onCancel__ = function () {


        if (typeof (data.onCancel) == 'function')
            data.onCancel();

        if (__modal)
            __modal.hide();
    };

    if (!data.okLabel)
        data.okLabel = eTools.getRes(420);// "Ok";

    if (!data.cancelLabel)
        data.cancelLabel = eTools.getRes(29); // "Annuler";

    if (!data.title)
        data.title = eTools.getRes(5017);//"Choisir une date";


    var nHideNoDate = data.hideEmptyDate ? 1 : 0;
    var nHideHourField = data.hideHoursAndMinutes ? 1 : 0;

    __modal = createCalendarPopUp("__validateDate__", nHideNoDate, nHideHourField, data.title, data.okLabel, "__onOK__", data.cancelLabel, __onCancel__, null, "", null, data.value);

}



///Objet définissant le niveau d'importance de message affiché avec un Alert ou une eConfirm
var eMsgBoxCriticity =
{
    /// Niveau critique : 0
    MSG_CRITICAL: 0,
    /// Question à l'itilisateur : 1
    MSG_QUESTION: 1,
    /// Demande d'attention à l'utilisateur : 2
    MSG_EXCLAM: 2,
    /// A titre informatif : 3
    MSG_INFOS: 3,
    /// Operation déroulée avec succès : 4
    MSG_SUCCESS: 4
};

// Methode personnalisée de XRM destinée à remplacer la méthode native confirm() en utilisant eModalDialog
// criticity : niveau d'importance du message:
//        MSG_CRITICAL: 0,
//        MSG_QUESTION: 1,
//        MSG_EXCLAM: 2,
//        MSG_INFOS: 3,
//        MSG_SUCCESS: 4
// title: apparait dans la barre des titres
// message : apparait en gras à la suite du logo représentant le niveau de criticité
// details : apparait à la suite du message en non gras
// width et height : dimensions de la popup
// okFct : fonction à exécuter à la validation
// cancelFct : fonction à exécuter à l'annulation
// bOkGreen : Si à vrai le bouton contenant la fonction ok sera en vert et à l'inverse celui du cancel sera en gris
function eConfirm(criticity, title, message, details, width, height, okFct, cancelFct, bOkGreen, bHtml) {
    return eAdvConfirm({
        'criticity': criticity,
        'title': title,
        'message': message,
        'details': details,
        'width': width,
        'height': height,
        'okFct': okFct,
        'cancelFct': cancelFct,
        'bOkGreen': bOkGreen,
        'bHtml': bHtml,
        'resOk': top._res_28,
        'resCancel': top._res_29
    });
}

//Permet de personaliser les libellé des boutons, etc
function eAdvConfirm(config) {

    if (typeof (config) == 'undefined' || config == null)
        return;

    var oModCfm;
    if (typeof (config.details) == 'undefined' || config.details == null)
        config.details = '';
    if (typeof (config.width) == 'undefined' || config.width == null)
        config.width = 500;
    if (typeof (config.height) == 'undefined' || config.height == null)
        config.height = 200;

    if (typeof (config.resOk) == 'undefined' || config.resOk == null || config.resOk == '')
        config.resOk = top._res_28;

    if (typeof (config.resCancel) == 'undefined' || config.resCancel == null || config.resCancel == '')
        config.resCancel = top._res_29;

    if (typeof (config.bHtml) == 'undefined' || config.bHtml == null)
        config.bHtml = false;

    // Fonction "do nothing"
    if (typeof (config.okFct) != 'function')
        config.okFct = function () { };


    if (typeof (config.cancelFct) != 'function')
        config.cancelFct = function () { };

    if (config.bOkGreen) {
        strCssOk = 'button-green';
        strCssCancel = 'button-gray';
    }
    else {
        strCssOk = 'button-gray';
        strCssCancel = 'button-green';
    }

    // #50 353 - surcharge possible de cssOk et cssCancel pour afficher des boutons d'autres couleurs
    // Surcharge donc le paramètre bOkGreen
    // ELAIZ - retour homol #3047 - ajout classe css fenêtre doublon
    if (typeof (config.cssOk) != 'undefined' && config.cssOk != null)
        strCssOk = config.cssOk;

    if (typeof (config.cssCancel) != 'undefined' && config.cssCancel != null)
        strCssCancel = config.cssCancel;

    oModCfm = new eModalDialog(config.title, 1, null, config.width, config.height);
    oModCfm.hideMaximizeButton = true;
    oModCfm.textClass = (oModCfm.title == top._res_814) ? "duplicate-msg confirm-msg" : "confirm-msg";
    oModCfm.setMessage(config.message, config.details, config.criticity, config.bHtml);
    oModCfm.show();

    oModCfm.addButtonFct(config.resCancel, function () { config.cancelFct(); oModCfm.hide(); }, strCssCancel);
    oModCfm.addButtonFct(config.resOk, function () { config.okFct(); oModCfm.hide(); }, strCssOk);

    return oModCfm;
}

// Enumération de la criticité
eAdvConfirm.CRITICITY =
{
    MSG_CRITICAL: 0,
    MSG_QUESTION: 1,
    MSG_EXCLAM: 2,
    MSG_INFOS: 3,
    MSG_SUCCESS: 4
}


function eAlertAdvParam(param) {

    param = Object.assign({}, {
        criticity: eAdvConfirm.CRITICITY.MSG_INFOS,
        title: "",
        message: "",
        details: "",
        width: 500,
        height: 200,
        okFct: function () { },
        DontIgnoreSetWait: false,
        HtmlContent: false

    }, param)


    oModAlert = new eModalDialog(param.title, 1, '', param.width, param.height, null, param.DontIgnoreSetWait);
    oModAlert.hideMaximizeButton = true;
    oModAlert.setMessage(param.message, param.details, param.criticity, param.HtmlContent);
    oModAlert.show();

    //btn fermer
    oModAlert.addButtonFct(top._res_30, function () { param.okFct(); oModAlert.hide(); }, "button-green", "cancel");


    return oModAlert;
}

// Methode personnalisée de XRM destinée à remplacer la méthode native alert() en utilisant eModalDialog
// criticity : niveau d'importance du message
//        MSG_CRITICAL: 0,
//        MSG_QUESTION: 1,
//        MSG_EXCLAM: 2,
//        MSG_INFOS: 3,
//        MSG_SUCCESS: 4
// title: apparait dans la barre des titres
// message : apparait en gras à la suite du logo représentant le niveau de criticité
// details : apparait à la suite du message en non gras
// width et height : dimensions de la popup
// okFct : fonction à exécuter à la validation
// bDontIgnoreSetWait : Permet de forcer l'affichage au dessus des set wait

function eAlert(criticity, title, message, details, width, height, okFct, bDontIgnoreSetWait, bHtmlContent) {



    if (!bHtmlContent || typeof (bHtmlContent) == "undefined" || bHtmlContent == null)
        var bHtmlContent = false;


    var oModAlert;

    this.Modal = oModAlert;

    if (details == null)
        details = '';

    if (width == null || width == 0)
        width = 500;
    if (height == null || height == 0)
        height = 200;

    // Fonction "do nothing"
    if (typeof (okFct) != 'function')
        okFct = function () { };


    var p = {
        criticity: criticity,
        title: title,
        message: message,
        details: details,
        width: width,
        height: height,
        okFct: okFct,
        DontIgnoreSetWait: bDontIgnoreSetWait,
        HtmlContent: bHtmlContent

    }

    //eAlertAdvParam(p)


    oModAlert = new eModalDialog(title, 1, '', width, height, null, bDontIgnoreSetWait);
    oModAlert.hideMaximizeButton = true;
    oModAlert.setMessage(message, details, criticity, bHtmlContent);
    oModAlert.show();
    oModAlert.addButtonFct(top._res_30, function () { okFct(); oModAlert.hide(); }, "button-green", "cancel");
    return oModAlert;
}

/* #81 722 - Crée une fenêtre modale de type Question, avec, par défaut, un bouton Fermer, et la possibilité de sélectionner le texte par défaut inscrit dans le champ de saisie */
function ePrompt(title, label, value, width, height, bSelectPromptValue, bNoCloseButton) {
    var strType = "2"; // ModalType.Prompt dans eModalDialog

    var oModalPrompt = new eModalDialog(title, strType, null, width, height);
    oModalPrompt.setPrompt(label, value);
    oModalPrompt.show();
    if (bSelectPromptValue) {
        var oModalPromptInput = document.getElementById(oModalPrompt.getPromptIdTextBox());
        if (oModalPromptInput) {
            oModalPromptInput.select();
            oModalPromptInput.focus();
        }
    }

    if (!bNoCloseButton)
        oModalPrompt.addButton(top._res_30, function () { oModalPrompt.hide(); }, "button-green"); // Fermer

    return oModalPrompt;
}

var opFadeIn = 0;
var oFadeDoc;
function fadeThis(eid, oDoc) {
    oFadeDoc = oDoc;
    opFadeIn = 0;
    setTimeout("doFade('" + eid + "')", 10);
}


function doFade(eid) {
    if (opFadeIn == 100)
        return;

    var element = oFadeDoc.getElementById(eid);
    if (element == null)
        return;
    opFadeIn = opFadeIn + 10;
    element.style.opacity = opFadeIn / 100;
    element.style.filter = 'alpha(opacity = ' + (opFadeIn) + ')';

    setTimeout("doFade('" + eid + "')", 10);
}

// Retourne la sélection effectuée sur l'élément HTML désigné
function getElementSelection(elt) {
    if (typeof (elt.getSelection) == "function")
        return elt.getSelection(); // Tous les navigateurs normaux
    else {
        if (typeof (elt.selection) != "undefined")
            return elt.selection; // IE
        else
            return null;
    }
}

// Retourne la sélection effectuée à la position indiquée sur l'élément HTML désigné
function getElementSelectionRangeAt(elt, rangeIndex) {
    var oSel = getElementSelection(elt);
    if (typeof (oSel) != "undefined" && typeof (oSel.getRangeAt) == "function")
        return oSel.getRangeAt(rangeIndex);
    else
        return null;
}

// Retourne le texte actuellement sélectionné et l'élément dans lequel il se trouve
// Source : Tim Down @ StackOverflow
// https://stackoverflow.com/questions/4636919/how-can-i-get-the-element-in-which-highlighted-text-is-in
// http://jsfiddle.net/timdown/Q9VZT/
function getSelectionTextAndContainerElement() {
    var text = "", containerElement = null;
    if (typeof window.getSelection != "undefined") {
        var sel = window.getSelection();
        if (sel.rangeCount) {
            var node = sel.getRangeAt(0).commonAncestorContainer;
            containerElement = node.nodeType == 1 ? node : node.parentNode;
            text = sel.toString();
        }
    } else if (typeof document.selection != "undefined" &&
        document.selection.type != "Control") {
        var textRange = document.selection.createRange();
        containerElement = textRange.parentElement();
        text = textRange.text;
    }
    return {
        text: text,
        containerElement: containerElement
    };
}

// Retourne le conteneur de l'élément actuellement sélectionné
// Source : Tim Down @ StackOverflow
// https://stackoverflow.com/questions/1335252/how-can-i-get-the-dom-element-which-contains-the-current-selection
// http://jsfiddle.net/pmrotule/dmjsnghw
function getSelectionBoundaryElement(isStart) {
    var range, sel, container;
    if (document.selection) {
        range = document.selection.createRange();
        range.collapse(isStart);
        return range.parentElement();
    } else {
        sel = window.getSelection();
        if (sel.getRangeAt) {
            if (sel.rangeCount > 0) {
                range = sel.getRangeAt(0);
            }
        } else {
            // Old WebKit
            range = document.createRange();
            range.setStart(sel.anchorNode, sel.anchorOffset);
            range.setEnd(sel.focusNode, sel.focusOffset);

            // Handle the case when the selection was selected backwards (from the end to the start in the document)
            if (range.collapsed !== sel.isCollapsed) {
                range.setStart(sel.focusNode, sel.focusOffset);
                range.setEnd(sel.anchorNode, sel.anchorOffset);
            }
        }

        if (range) {
            container = range[isStart ? "startContainer" : "endContainer"];

            // Check if the container is a text node and return its parent if so
            return container.nodeType === 3 ? container.parentNode : container;
        }
    }
}

///retourne un objet contenant la position et la dimension de l'objet elt en NE tenant PAS compte de la position du scroll
//  stopAtRelative : indique si la position est relative au parent
function getAbsolutePosition(elt, stopAtRelative) {

    if (elt == null)
        return null;
    if (typeof (stopAtRelative) == 'undefined' || stopAtRelative == null)
        stopAtRelative = false;

    var obj = getAbsolutePositionWithScroll(elt, stopAtRelative);

    var scrollPos = getScrollPosition(elt);
    // A supprimer car scrollDivId est obsolete
    if (scrollPos != null) {
        obj.x = obj.x - scrollPos.x;
        obj.y = obj.y - scrollPos.y;
    }

    return obj;
}

///retourne un objet contenant la position et la dimension de l'objet elt en tenant compte de la position du scroll
//  stopAtRelative : indique si la position est relative au parent
function getAbsolutePositionWithScroll(elt, stopAtRelative) {
    var ex = 0, ey = 0;

    if (elt == null)
        return null;

    if (typeof (stopAtRelative) == 'undefined' || stopAtRelative == null)
        stopAtRelative = false;

    var brow = new getBrowser();

    var eOffsetWidth = elt.offsetWidth, eOffsetHeight = elt.offsetHeight;

    do {
        var curStyle = brow.isIE ? elt.currentStyle : window.getComputedStyle(elt, '');
        var supportFixed = !(brow.isIE && brow.version < 7);
        // HLA - Je désactive le mode "supportFixed" car il n'est pas géré dans le cas du "scrollDivId"
        supportFixed = false;

        if (stopAtRelative && curStyle.position == 'relative') {
            break;
        } else if (supportFixed && curStyle.position == 'fixed') {
            // Get the fixed el's offset
            ex += parseInt(curStyle.left, 10);
            ey += parseInt(curStyle.top, 10);
            // Compensate for scrolling
            ex += document.body.scrollLeft;
            ey += document.body.scrollTop;
            // End the loop
            break;
        } else {
            ex += elt.offsetLeft;
            ey += elt.offsetTop;
        }
    } while (elt = elt.offsetParent);

    var obj = new Object();
    obj.x = ex;
    obj.y = ey;
    obj.w = eOffsetWidth;
    obj.h = eOffsetHeight;

    return obj;
}

function isPointInArea(point, area) {
    if (!area || !point)
        return false;

    var bX = point.x >= area.x && point.x <= area.x + area.w;
    var bY = point.y >= area.y && point.y <= area.y + area.h;

    return bX && bY;
}

/*
Permet de scroller de façon à ce que l'élément soit affiché (en position absolue) à une hauteur y

eltToScroll : Element sur lequel porte la scrollBar
oElt : Element qui doit arriver à la hauteur y
y : hauteur à laquelle oElt doit arriver
*/
function scrollEltToY(eltToScroll, oElt, y) {
    if (!eltToScroll || !oElt || !y)
        return;

    eltToScroll.scrollTop += getAbsolutePosition(oElt).y - y;

}

function getScrollPosition(elt) {
    if (elt == null)
        return null;

    var pos = { x: 0, y: 0 };

    do {
        if (elt.nodeName && elt.nodeName.search('HTML') != -1)
            break;

        pos.x += elt.scrollLeft;
        pos.y += elt.scrollTop;
    } while (elt = elt.parentNode);

    return pos;
}

// Determine browser and version.
function getBrowser() {
    this.version = null;

    var ua = navigator.userAgent.toLowerCase();




    this.isIE = ((navigator.appName == 'Microsoft Internet Explorer') || ((navigator.appName == 'Netscape') && (new RegExp("Trident/.*rv:([0-9]{1,}[\.0-9]{0,})").exec(navigator.userAgent) != null)));


    this.isIE8 = (navigator.userAgent.indexOf("Trident/4") > -1);
    this.isIE9 = (navigator.userAgent.indexOf("Trident/5") > -1);
    this.isIE10 = (navigator.userAgent.indexOf("Trident/6") > -1);
    this.isIE11 = (navigator.userAgent.indexOf("Trident/7") > -1);

    this.isEdge = (navigator.userAgent.indexOf("Edge/") > -1);

    this.isChrome = /Chrome/.test(navigator.userAgent) && /Google Inc/.test(navigator.vendor);

    this.isOpera = (window && !!window.opera && window.opera.version);
    this.isWebKit = (ua.indexOf(' applewebkit/') > -1);
    this.isGecko = (navigator.product && navigator.product.toLowerCase() == 'gecko' && !this.isWebKit && !this.isOpera);

    this.isFirefox = navigator.userAgent.toLowerCase().indexOf('firefox') > -1;

    this.isMac = (ua.indexOf('macintosh') > -1);

    this.isMobile = (ua.indexOf('mobile') > -1);
    this.isIOS = /(ipad|iphone|ipod)/.test(ua);
    this.isAndroid = /(android)/.test(ua);

    if (this.isIE) {
        if (ua.match(/msie (\d+(.\d+)?)/))
            this.version = parseFloat(ua.match(/msie (\d+(.\d+)?)/)[1]);
        else {
            var re = new RegExp("trident/.*rv:([0-9]{1,}[\.0-9]{0,})");
            if (re.exec(ua) != null)
                this.version = parseFloat(RegExp.$1);


        }
    }
    else if (this.isEdge) {
        var re = new RegExp("edge/([0-9]{1,}[\.0-9]{0,})");
        if (re.exec(ua) != null)
            this.version = parseFloat(RegExp.$1);
    }
    else if (this.isGecko) {
        var geckoRelease = ua.match(/rv:([\d\.]+)/);

        if (geckoRelease) {
            geckoRelease = geckoRelease[1].split('.');
            this.version = geckoRelease[0] * 10000 + (geckoRelease[1] || 0) * 100 + (geckoRelease[2] || 0) * 1;
        }
    }
    else if (this.isOpera)
        this.version = parseFloat(opera.version());
    else if (this.isWebKit)
        this.version = parseFloat(ua.match(/ applewebkit\/(\d+(.\d+)?)/)[1]);

    this.isV7Compatible = this.isIE || this.isFirefox;

    //Normalement, le mode de compatibilité doit être désactivé
    this.CompatibilityMode = this.isIE && this.version == 7 && (this.isIE8 || this.isIE9 || this.isIE10 || this.isIE11);
}

// Indique si l'on consulte l'application avec une tablette Android
function isTabletAndroid() {
    if (debugTablet == null) {
        var browser = new getBrowser();
        return browser.isAndroid;
    }
    else {
        return debugTablet;
    }

}

// Indique si l'on consulte l'application avec une tablette
function isTablet() {

    if (debugTablet == null) {
        var browser = new getBrowser();
        return browser.isMobile || browser.isIOS || browser.isAndroid;
    }
    else {
        return debugTablet;
    }

}

function isTabletMenuEnabled() {
    return debugTabletMenuEnabled;
}

function isTabletNavBarEnabled() {
    return debugTabletNavBarEnabled;
}

function isTabletCalendarEnabled() {
    return debugTabletCalendarEnabled;
}

function areTabletCustomScrollersEnabled() {
    return debugTabletCustomScrollersEnabled;
}

function isTabletNativeScrollEnabled() {
    return debugTabletNativeScrollEnabled;
}

function isTabletOrientationSupportEnabled() {
    return debugTabletOrientationSupportEnabled;
}

function isTabletTiltSupportEnabled() {
    return debugTabletTiltSupportEnabled;
}

// Permet d'ouvrir le menu escamotable - Fonction alias vers eMain.ocMenu(true)
function openRightMenu(e) {
    if (typeof (ocMenu) == "function")
        ocMenu(true, e);
}
// Permet de fermer le menu escamotable - Fonction alias vers eMain.ocMenu(false)
function closeRightMenu(e) {
    if (typeof (ocMenu) == "function")
        ocMenu(false, e);
}

function canConfirmUnload() {
    return debugConfirmUnload;
}

///retourne un input de formulaire
function getInput(sName, sType, sValue, sId, oWin) {

    if (typeof (sType) == "undefined")
        var sType = "text";

    if (typeof (sValue) == "undefined")
        var sValue = "";

    if (typeof (sId) == "undefined")
        var sId = sName

    if (typeof (oWin) == "undefined" || oWin == null || !oWin.document)
        oWin = top;

    try {
        var input = oWin.document.createElement('<input>');
    }
    catch (e) {
        var input = oWin.document.createElement('input');
    }

    input.setAttribute("NAME", sName);
    input.id = sId;
    input.type = sType;
    input.value = sValue;

    return input;
}

/*************PLANNING TOOLTIP**************/
/*Retourne la taille du scroll en X et Y sous forme d'un tableau de deux valeurs*/
function getScrollXY() {
    var scrOfX = 0, scrOfY = 0;
    if (typeof (window.pageYOffset) == 'number') {
        //Netscape compliant
        scrOfY = window.pageYOffset;
        scrOfX = window.pageXOffset;
    } else if (document.body && (document.body.scrollLeft || document.body.scrollTop)) {
        //DOM compliant
        scrOfY = document.body.scrollTop;
        scrOfX = document.body.scrollLeft;
    } else if (document.documentElement && (document.documentElement.scrollLeft || document.documentElement.scrollTop)) {
        //IE6 standards compliant mode
        scrOfY = document.documentElement.scrollTop;
        scrOfX = document.documentElement.scrollLeft;
    }
    return [scrOfX, scrOfY];
}

/*Retourne la position X et Y d'un élément en tenant compte du scroll sous forme d'un tableau de deux valeurs*/
function getClickPositionXY(e) {
    var posx = 0;
    var posy = 0;
    var scrollXY = getScrollXY();
    if (!e) var e = window.event;
    // 39222 CRU : sur tablette, la récupération de la position cliquée doit provenir d'un événement "touch"
    if (e.type == "touchend") {
        if (e.changedTouches) {
            posx = e.changedTouches[0].clientX + scrollXY[0];
            posy = e.changedTouches[0].clientY + scrollXY[1];
        }
    }
    else {
        if (e.pageX || e.pageY) {
            posx = e.pageX;
            posy = e.pageY;
        }
        else if (e.clientX || e.clientY) {
            posx = e.clientX + scrollXY[0];
            posy = e.clientY + scrollXY[1];
        }
        else if (e.offsetLeft || e.offsetTop) {
            posx = e.offsetLeft + scrollXY[0];
            posy = e.offsetTop + scrollXY[1];
        }
    }


    return [posx, posy];
}

function getWindowWH(oDoc) {
    if (!oDoc)
        oDoc = window;
    var width = 0;
    var height = 0;

    var obj = null;
    try {
        obj = oDoc.getWindowSize();
        width = obj.w;
        height = obj.h;

    }
    catch (eeee) {
        width = 0;
        height = 0;
    }
    return [width, height]
}

/*******************************************/

// Utilisé pour les navigateur ne prenant pas en charge outerHTML
function altOuterHTML(oElement, newContent) {
    var oNewElement = document.createElement("span");
    oNewElement.innerHTML = newContent;
    oElement.parentNode.replaceChild(oNewElement.childNodes[0], oElement);
}

var modalColorPicker = null;
function pickColor(colorPicker, colorPickerText, onHideFct) {
    //CNA - Demande #57670 - Simule un click pour le colorPicker des planning en mode fiche incrustée en signet
    if (colorPickerText != null && colorPickerText.hasAttribute("eaction") && colorPickerText.getAttribute("eaction") == "LNKFREETEXT")
        colorPickerText.click();

    var pickerOptions = {
        picker: colorPicker,
        pickerTxt: colorPickerText,
        color: getAttributeValue(colorPicker, "value"),
        onHide: onHideFct
    };
    var oPicker = new eColorPicker(pickerOptions);
    oPicker.show();
}


/****************************************************/


///Permet de lancer une méthode en gardant son contexte
///utile lorsque l'on doit passer la méthode d'un objet à une autre fonction pour être appelé plus tard
// par exemple pour définir les actions sur un bouton dans les eModalDialog
function launchInContext(monObj, maFonct) {
    return function () {
        return maFonct.apply(monObj, arguments);
    }
}

/****************************************************/
/* Ajoute un trigger sur une méthode                */
/*  addTrigger(monObj, "methode", trigger1, monObj);  */
/****************************************************/
function addTrigger(myObj, myMethName, myTrigger) {

    // Sauvegarde la méthode d'origine
    var objOrigMeth = myObj[myMethName];

    // sauvegade la méthode originale sous un autre nom
    // utilisé pour retiter le/les triggers
    if (typeof myObj["_orig" + myMethName] != "function")
        myObj["_orig" + myMethName] = objOrigMeth;

    // redéfini la méthode en ajoutant le trigger
    myObj[myMethName] = function () {

        //Appel la méthode originale
        objOrigMeth.apply(myObj, arguments);

        //Appel le trigger - fourni l'objet et le nom de la méthode originale
        try { myTrigger(myObj, myMethName); } catch (ex) { };

    }
}

/**********************************************************/
/*  retire tous les trigger sur la methode d'un objet     */
/*  removeTrigger(origObj, name);                         */
/**********************************************************/
function removeTrigger(myObj, myMethName) {
    if (typeof myObj["_orig" + myMethName] == "function") {
        myObj[myMethName] = myObj["_orig" + myMethName];
        myObj["_orig" + myMethName] = null;
        return true
    }
    else
        return false;
}



function getTabDescid(descid) {
    return descid - descid % 100;
}

function getDescid(descid) {
    return descid % 100;
}


function timeStamp() {

    var date = new Date();

    return date.getTime();
}


//Retourne le contenu texte de l'objet s'il y a lieu, en tenant compte du navigateur (textcontent)
function GetText(obj, html) {
    if (obj.text != null)
        return obj.text;
    else if (obj.innerHTML != null && html === true)   //réuperer toutes les espaces à l'interrieur de la chaine qui sont absents dans l'innerText
        return obj.innerHTML;
    else if (obj.innerText != null)   //on préfère innertext à textcontent car sur chrome le text content renvoi des espaces
        return obj.innerText;
    else if (obj.textContent != null)   //sur IE8 il faut utiliser textContent
        return obj.textContent;
    else
        throw "impossible de récupérer le contenu.";
}
//Affecte le contenu texte de l'objet s'il y a lieu, en tenant compte du navigateur (textcontent)
function SetText(obj, value) {


    if (obj.tagName && typeof (obj.tagName) == "string" && obj.tagName.toLowerCase() == 'textarea') {
        //#46286 
        // Il ne faut pas utiliser innerText(prio dans les if), cela pause des pb sur firefox : les sauts de lignes sont remplacer par des br
        // En théorie il faut utiliser value pour les textarea.
        // Mais pour certaines fonctionnalités, c'est textContent qui est utilisé. Du coup, on utilise que textContent
        // on fait un return plutot qu'un elseif pour le cas ie8
        if (obj.value != null) {
            obj.value = value;
            return;
        }
        if (obj.textContent != null) {
            obj.textContent = value;
            return;
        }
    }

    if (obj.text != null)
        obj.text = value;
    else if (obj.innerText != null)
        obj.innerText = value;
    else if (obj.textContent != null)
        obj.textContent = value;
    else
        obj.data = value;   // pour IE8




}
//Retourne le contenu texte de l'objet s'il y a lieu, en tenant compte du navigateur (textcontent)
function GetTextContent(obj) {
    if (obj.textContent != null)
        return obj.textContent;
    else if (obj.innerText != null)
        return obj.innerText;
    else
        return "";
}

//Suppression des espaces à droite et à gauche de la chaine
function trim(myString) {
    return myString.replace(/^\s+/g, '').replace(/\s+$/g, '');
}

//===========================================================================
// Provides a Dictionary object for client-side java scripts
//===========================================================================
/* EXEMPLE D'UTILISATION DU DICTIONNAIRE JS*/
/*
//On remplit
var userDict = new Dictionary();
userDict.Add("smith", "admin");
userDict.Add("jones", "user");
userDict.Delete("jones");
//On exploite :
var keys = userDict.Keys;
for (var i = 0; i < keys.length; i++) {
alert((keys[i] + ": " + userDict.Lookup(keys[i])));
} 
*/
/*EXEMPLE D'UTILISATION BI-DIMENSIONNEL */
/*
//On remplit
var tabDict = new Dictionary();

tabDict.Add(1, new Dictionary());
var myVal = tabDict.Lookup(1)
myVal.Add(1, "Dimension 1>1");
myVal.Add(2, "Dimension 1>2");

tabDict.Add(2, new Dictionary());
myVal = tabDict.Lookup(2)
myVal.Add(1, "Dimension 2>1");

tabDict.Add(3, new Dictionary());
myVal = tabDict.Lookup(3)
myVal.Add(1, "Dimension 3>1");
myVal.Add(2, "Dimension 3>2");
myVal.Add(3, "Dimension 3>3");

//On exploite :
var keys = tabDict.Keys;
for (var i = 0; i < keys.length; i++) {
var tabDict2 = tabDict.Lookup(keys[i]);
alert((keys[i] + ": " + tabDict2));

var keys2 = tabDict2.Keys;
for (var j = 0; j < keys2.length; j++) {
var myVal = tabDict2.Lookup(keys2[j]);
if (myVal != "" && typeof (myVal) != "undefined")
alert((keys2[j] + ": " + myVal));
}
}
*/
function Dictionary() {
    this.Add = function () {
        for (c = 0; c < this.Add.arguments.length; c += 2) {
            // Add the property
            this[this.Add.arguments[c]] = this.Add.arguments[c + 1];
            // And add it to the keys array
            this.Keys[this.Keys.length] = this.Add.arguments[c];
        }
    };
    this.Lookup = function (key) {
        return (this[key]);
    };
    this.Delete = function () {
        for (c = 0; c < this.Delete.arguments.length; c++) {
            this[this.Delete.arguments[c]] = null;
        }
        // Adjust the keys (not terribly efficient)
        var keys = new Array()
        for (var i = 0; i < this.Keys.length; i++) {
            if (this[this.Keys[i]] != null)
                keys[keys.length] = this.Keys[i];
        }
        this.Keys = keys;
    };
    this.Keys = new Array();
}
//===========================================================================

function GetMainDivWH() {
    var oMainDiv = document.getElementById("mainDiv");
    var height = ((top.innerHeight) ? top.innerHeight : getWindowWH()[1]) - oMainDiv.offsetTop - 10;
    var width = 0;
    if (oMainDiv.clientWidth && oMainDiv.clientWidth > 0)
        width = oMainDiv.clientWidth;
    return [width, height];
}

//Retourne la taille disponible de la fiche
function GetFileWidth() {
    //pop up : Largeur de la fenêtre
    //pas pop up : Largeur de la fiche    

    var nInnerWidth = 0;
    if (parent.window.innerWidth)
        nInnerWidth = parent.window.innerWidth;
    else
        nInnerWidth = parent.document.documentElement.clientWidth;

    nInnerWidth = Math.round((8 / 10) * nInnerWidth);


    return nInnerWidth;

    // return Math.round((8 / 10) * getWindowSize().w);
}

//Retourne un tableau contenant la liste des TabId des tables Parentes pour le mode fiche
function getTabFileLnkId(nTab) {
    var oLnkIds = document.getElementById("lnkid_" + nTab);
    if (!oLnkIds)
        return null;

    var sLnkId = oLnkIds.value;
    var tabLnkIdFileId = sLnkId.split(";");
    var tabLnkId = new Array();
    for (var cpt = 0; cpt < tabLnkIdFileId.length; cpt++) {
        var sCurrentValue = tabLnkIdFileId[cpt];
        if (sCurrentValue == "" || !sCurrentValue)
            continue;
        var nCurrentTab = getNumber(sCurrentValue.split("=")[0]);
        if (nCurrentTab > 0)
            tabLnkId.push(nCurrentTab);
    }
    return tabLnkId;
}


function isValidMail(sMail) {
    var strPattern = "^[a-z0-9!#$%&'*+\/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+\/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?$";
    var regMail = new RegExp(strPattern, "i");
    return regMail.test(sMail + "");
}


function isValidNumber(sNumber) {


    sNumber = sNumber + "";

    //sNumber = sNumber.replace(/\s/g, '');    
    sNumber = sNumber.replace(/[\s\xA0]/gi, '');


    var regNumber = new RegExp("^[+-]?((\\d+((\\.|\\,)\\d*)?)|((\\.|\\,)\\d+))$", "i");
    return regNumber.test(sNumber);


}

function getElementsByAttribute(oElm, strTagName, arrayAttributeName, arrayAttributeValue) {
    if (typeof oElm == "undefined" || typeof strTagName == "undefined" || typeof arrayAttributeName == "undefined")
        return null;

    var selector = strTagName;

    var aValues = null;
    var aAttributes = arrayAttributeName.split(";");
    if (typeof arrayAttributeValue != "undefined")
        aValues = (arrayAttributeValue + "").split(";");

    for (i = 0; i < aAttributes.length; i++) {
        if (aValues != null && aValues.length > i)
            selector = selector + "[" + aAttributes[i] + "='" + aValues[i] + "']";
        else
            selector = selector + "[" + aAttributes[i] + "]";
    }

    return oElm.querySelectorAll(selector);
}
//Retourne le radio bouton sélectionné d'une liste d'un groupe de radio boutons
function getCheckedRadio(name, parentElem) {

    var brow = new getBrowser();

    var lstChecked;

    if (brow.isIE8) {

        if (typeof (parentElem) == "undefined" || parentElem == null)
            lstChecked = document.querySelectorAll("input[type='radio'][name='" + name + "']");
        else
            lstChecked = document.getElementById(parentElem).querySelectorAll("input[type='radio'][name='" + name + "']");


        for (var i = 0; i < lstChecked.length; i++) {
            if (lstChecked[i].checked) {
                return lstChecked[i];
            }
        }


    }
    else {


        if (typeof (parentElem) == "undefined" || parentElem == null)
            return document.querySelector("input[type='radio'][name='" + name + "']:checked");
        else
            return document.getElementById(parentElem).querySelector("input[type='radio'][name='" + name + "']:checked");
    }
}

/// <summary>
/// Affiche un message d'avertissement en cas
/// de problème de traitement sur le filtre
/// </summary>
/// <param name="sTitle">Titre de la fenêtre</param>
/// <param name="sMsg">Message principale (en gras) </param>
/// <param name="sDetailMsg">Detail du message</param>
function showWarning(sTitle, sMsg, sDetailMsg) {

    return eAlert(2, sTitle, sMsg, sDetailMsg, 500, 200, null);
}

/// <summary>
/// Affiche un message d'infos
/// de problème de traitement sur le filtre
/// </summary>
/// <param name="sTitle">Titre de la fenêtre</param>
/// <param name="sMsg">Message principale (en gras) </param>
/// <param name="sDetailMsg">Detail du message</param>
function showInfoDialog(sTitle, sMsg, sDetailMsg) {

    return eAlert(3, sTitle, sMsg, sDetailMsg, 600, 250, null);
}


var KeyComboSecret = new Dictionary();
function CHECKSECRET(password, keyPress, onSuccess) {
    try {
        if (!KeyComboSecret.Lookup(password)) {
            KeyComboSecret.Add(password, new Array());
        }
        KeyComboSecret.Lookup(password).push(keyPress);
        var pwdIdx = 0;
        var success = true;
        for (pwdIdx = 0; pwdIdx < KeyComboSecret.Lookup(password).length; pwdIdx++) {
            if (KeyComboSecret.Lookup(password)[pwdIdx] != password[pwdIdx]) {
                KeyComboSecret.Delete(password);
                success = false;
                break;
            }
        }
        if (KeyComboSecret[password] != null && password.length == KeyComboSecret[password].length && success) {
            onSuccess();
            KeyComboSecret.Delete(password);
        }
    }
    catch (ex) {

    }
}


///summary
///Joue le son secret html5
///summary
function PlaySecretSound(oObj, sound) {
    var oSS = document.getElementById("secretsound");
    if (!oSS) {
        oSS = oObj.appendChild(document.createElement("audio"));
        oSS.style.display = "none";
        oSS.setAttribute("autobuffer", "autobuffer");
        oSS.id = "secretsound";
    }
    if (oSS) {
        oSS.setAttribute("src", sound);
        oSS.play();
    }

}

/// Objet permettant de jouer un son
var Sound = (function () {

    // Un seul objet pour toute l'appli suffit
    if (typeof (top.Sound) != 'undefined')
        return top.Sound;

    var _element = null;

    function internalInit(url) {
        _element = top.document.getElementById("soundobject");
        if (_element == null) {
            _element = top.document.createElement("audio");
            _element.style.display = "none";
            _element.id = "soundobject";

            setAttributeValue(_element, "autobuffer", "autobuffer");
        }

        if (_element)
            setAttributeValue(_element, "src", url);
    }

    function internalPlay() {
        if (_element != null) {
            try {
                _element.play();
            } catch (e) {
                console.log("Impossible de lire le fichier audio : " + e);
            }
        }
    }

    function internalPause() {
        if (_element != null) {
            try {
                _element.pause();
            } catch (e) {
                console.log("Impossible de mettre en pause la lecture : " + e);
            }
        }
    }

    return {
        play: function (sound) {
            internalInit(sound);
            internalPlay();
            return this;
        },
        pause: function () {
            internalPause();
            return this;
        }
    }
}());


/*Permet de créer la case à cocher EUDO*/
//oDoc : page créant le controle (type document)
//oElement : objet parent de la checkbox à créer
//bChecked : cocher la checkbox ou pas ?
//id : identifiants de la balise lien représentant la case à cocher
//label : texte à associer à la case à cocher
//attributesDict : liste des attributs à ajouter à la balise lien représentant la case à cocher (type Dictionary déclaré un peu plus haut)
function AddEudoCheckBox(oDoc, oElement, bChecked, id, label, attributesDict) {
    var radio = oDoc.createElement("a");
    radio.id = id;
    oElement.appendChild(radio);
    radio.setAttribute("href", "#");
    radio.setAttribute("onclick", "chgChk(this);return false;");
    radio.setAttribute("class", "rChk chk");
    radio.setAttribute("chk", bChecked ? "1" : "0");

    if (attributesDict != null && attributesDict.Keys) {
        var keys = attributesDict.Keys;
        for (var i = 0; i < keys.length; i++) {
            radio.setAttribute(keys[i], attributesDict.Lookup(keys[i]));
        }
    }
    var img = oDoc.createElement("img");
    img.setAttribute("src", "ghost.gif");
    img.setAttribute("style", "border-width:0px;");
    radio.appendChild(img);
    if (label != null && label != "") {
        var txt = oDoc.createTextNode(label);
        radio.appendChild(txt);
    }
    return radio;
}

///summary
///Affiche une fenetre d'attente
///summary
function showWaitDialog(title, message) {

    //eModalDialog(title, type, url, width, height, handle)
    var oWaitDialog = new eModalDialog(title, 3, null, 450, 150, "oWaitDialog");

    // Masquer le bouton "Agrandir"
    oWaitDialog.hideMaximizeButton = true;

    // Masquer le bouton "Fermer"
    oWaitDialog.hideCloseButton = true;
    oWaitDialog.noButtons = true;
    oWaitDialog.show();

    //dans eModalDialog type=3 (wait)
    var parentDiv = top.document.getElementById("waiting-message");
    //exemple de déclaration : this.createDiv = function (sCSS, oDiv, sLabel)

    var innerDiv = oWaitDialog.createDiv("waiting-label", parentDiv, message);

    return oWaitDialog;
}


// Calcul du nombre de rows par liste en automatique
function GetNumRows(nHeight) {



    var rows = 0;
    var heightRemaining = 0;
    if (nHeight) {
        heightRemaining = nHeight
    }
    else {
        // Taille de la fenêtre
        if (typeof (top.window.innerHeight) != 'undefined')
            heightRemaining = top.window.innerHeight;
        else if (typeof (top.document.documentElement.clientHeight) != 'undefined')
            heightRemaining = top.document.documentElement.clientHeight;
        else {
            alert('ANOMALIE');
            return;
        }
        // Hauteur utilisé par l'en-tête de l'application
        if (top.document.getElementById("mainDiv"))
            heightRemaining -= parseInt(top.document.getElementById("mainDiv").offsetTop);
        // Hauteur de la barre des infos de l'en-tête de la liste
        heightRemaining -= 34;

        // Hauteur + bordure de la barre des filtres de l'en-tête de la liste
        heightRemaining -= 30 + 2;
        // Hauteur de la barre des actions de l'en-tête de la liste
        heightRemaining -= 30;
        // Hauteur de la barre des actions du pied de page de la liste
        //heightRemaining -= 30; // ? N'existe plus ?
        // Hauteur de la barre du charindex du pied de page de la liste
        heightRemaining -= 30;
        // Hauteur du thead de la liste
        heightRemaining -= 22;
        // Bordures haute et basse du div de la liste
        heightRemaining -= 2;
        // Marge necessaire pour les fluctutations de la taille des fenêtres
        heightRemaining -= 20;
    }
    // Hauteur + bordure basse de ligne voir les classes CSS "cell" et "cell2" dans eList.css
    var cellHeight = 26; //TODO - constante



    var rows = Math.floor(heightRemaining / (cellHeight + 1));

    //SPH
    // TODO : refaire le calcul
    //rows = rows - 1; // ? encore utile ?

    if (rows <= 0) {
        var winParam = top.getParamWindow();
        if (winParam)
            rows = winParam.nMaxRows;
    }
    return rows;
}

///summary
//Retourne la ressource d'application si elle existe
///summaryeu
function GetAppRes(nResId) {

    if (isNumeric(nResId)) {
        try {
            var sResname = "_res_" + nResId;
            if (window[sResname])
                return window[sResname];
        }
        catch (e) {
        }
    }

    return "res #" + nResId + " invalide.";

}

//Retourne le niveau max de zindex utilisé sur la page
//oDoc (facultatif) : indique le document on l'on doit récupérer le zindex max
//nBaseLevel (facultatif) : indique le zindex minimum souhaité
//bIgnoreSetWait : Si a true, le z-index du setwait ne sera pas compté
function GetMaxZIndex(oDoc, nBaseLevel, bIgnoreSetWait) {
    if (!nBaseLevel)
        nBaseLevel = 1;
    if (!oDoc)
        oDoc = window.document;
    var allParentDocElements = oDoc.getElementsByTagName("*");
    for (var i = 0; i < allParentDocElements.length; i++) {
        var currentObj = allParentDocElements[i];
        if (getNumber(currentObj.style.zIndex) > nBaseLevel) {

            if (bIgnoreSetWait) {
                if (currentObj.id != "waiter")  //Si on ne doit pas tenir compte du setwait est selectionné on ignore l'index du setwait
                    nBaseLevel = getNumber(currentObj.style.zIndex);
            }
            else
                nBaseLevel = getNumber(currentObj.style.zIndex);
        }
    }
    return nBaseLevel;

}




// Recherche l'élement parent du tag voulu
// elt : element à analyser ; tag : tag de l'élement recherché
function findUp(elt, tag) {
    if (elt == null)
        return null;

    do {
        if (elt.nodeName && elt.nodeName.search(tag) != -1)
            return elt;
    } while (elt = elt.parentNode);

    return null;
};

// Recherche l'élement parent du tag voulu
// elt : element à analyser ; tag : tag de l'élement recherché
function findUpByClass(elt, className) {
    if (elt == null)
        return null;

    do {
        if (elt.className && elt.className.search(className) != -1)
            return elt;
    } while (elt = elt.parentElement);

    return null;
};


// Recherche l'élement parent du tag voulu
// elt : element à analyser ; tag : tag de l'élement recherché
function isParentElementOf(childElement, expectedParentElement) {
    if (childElement == null || expectedParentElement == null)
        return null;

    do {
        if (childElement == expectedParentElement)
            return true;
    } while (childElement = childElement.parentNode);

    return false;
};

/// MOU 22-10-2014 #33153
/// Recherche l'element dont l'id est elementId dans le container (ex : div, table, ...)
/// Globalement, la fonction simule "container.getElementById(elementId)"
/// S'il y a plusieurs element avec le meme id alors elle retourne le premier trouvé 
/// ATTENTION : (Algo DFS : le parcours se fait en profondeur)
function findElementById(container, elementId) {

    if (container.id == elementId)
        return container;

    var element = null;
    for (var key in container.children) {
        element = findElementById(container.children[key], elementId);
        if (element != null)
            break;
    }

    return element;
};

/// Recherche l'element en connaissant le tag name 
function findElementByTagName(elements, tagName) {

    if (elements == null && elements == "undefined")
        return null;

    for (var key in elements) {
        var child = elements[key];
        if (child != "undefined" && child.tagName == tagName)
            return child;

    }
    var elm = null;
    for (var index in elements) {
        elm = findElementByTagName(elements[index].children, tagName);
        if (elm != null)
            return elm;
    }

    return null;
};


function getTabFrom(oWin) {
    var tabFrom = 0;
    if (!oWin)
        oWin = window;

    var oParentDoc = oWin.document;

    if (oParentDoc == null || typeof (oParentDoc) == 'undefined')
        return tabFrom;

    var bCalMainDiv = nodeExist(oParentDoc, 'CalDivMain');
    var bFileDiv = nodeExist(oParentDoc, "fileDiv_" + oWin.nGlobalActiveTab);
    var bFinderDiv;
    var brow = new getBrowser();
    if (brow.isIE8) {
        var mainDiv = oParentDoc.getElementById("mainDiv");
        bFinderDiv = getAttributeValue(mainDiv, "edntype") == "lnkfile"
            && getAttributeValue(mainDiv, "tab") != ""
            && getAttributeValue(mainDiv, "tabfrom") != "";
    }
    else {
        bFinderDiv = oParentDoc.querySelector("#mainDiv[edntype=lnkfile][tab][tabfrom]") != null;
    }

    if (bCalMainDiv) {
        var mixteMode = getAttributeValue(oParentDoc.getElementById('CalDivMain'), 'mixtemode');
        if (mixteMode != '1')
            tabFrom = 0;       // CALENDAR
        else
            tabFrom = 0;       // CALENDAR_LIST
    }

    if (bFinderDiv) {
        tabFrom = getAttributeValue(oParentDoc.getElementById('mainDiv'), 'tabfrom');
    }
    else if (!bFileDiv)
        tabFrom = 0;
    else {
        tabFrom = getAttributeValue(oParentDoc.getElementById("fileDiv_" + oWin.nGlobalActiveTab), 'did');
    }

    return tabFrom;
}
//Transforme le tableau tabArray en chaine séparé par la chaine separator
function joinString(separator, tabArray) {
    var stringJoined = "";

    var cntAr = 0;
    for (cntAr = 0; cntAr < tabArray.length; cntAr++) {
        if (stringJoined != "")
            stringJoined = stringJoined + separator;
        stringJoined = stringJoined + tabArray[cntAr];
    }
    return stringJoined;
}


/*Gestion de l'ouverture et des actions standards des catalogues*/
/*CATALOGUES*/
/*
paramètres : 
    p_bMulti : (boolean) affichage en mode multiple
    p_btreeView : (boolean) affichage en mode arborescent
    p_defValue : valeur(s) à présélectionner (séparer par des ;)	
    p_sourceFldId : (facultatif : null si non renseigné)
    p_targetFldId :
    p_catDescId
    p_catPopupType
    p_catBoundDescId
    p_catBoundPopup
    p_catParentValue
    p_CatTitle
    p_JsVarName
    p_bMailTemplate	
    p_partOfAfterValidate (function) : appelée au clique sur valider juste avant la fermeture de la fenêtre
        possède en paramètres :
                - catalogDialog :  objet modal de la fenêtre créée
                - srcId
                - trgId
                - tabSelectedLabels	: (Array) Valeur affiché
                - tabSelectedValues	: (Array) Valeur en BDD
                - selectedIDs   : Identifiant de la valeur (même si catalogue simple)
    p_partOfAfterCancel (function) :  appelée au clique sur valider juste avant la fermeture de la fenêtre
        possède en paramètres :
                - catalogDialog : objet modal de la fenêtre créée
                - trgId
*/

var catalogDialog;
var partOfAfterValidate = null;
var partOfAfterCancel = null;



//liste non exhaustive à compléter
var LOADCATFROM = { UNDEFINED: 0, FILTER: 1, TREAT: 2, ADMIN: 3, EXPRESSFILTER: 4 };

function showCatGeneric(p_bMulti, p_btreeView, p_defValue, p_sourceFldId, p_targetFldId, p_catDescId, p_catPopupType, p_catBoundDescId, p_catBoundPopup,
    p_catParentValue, p_CatTitle, p_JsVarName, p_bMailTemplate, p_partOfAfterValidate, p_partOfAfterCancel, p_from/*, p_fromFilter, p_fromTreat, p_fromAdmin*/) {
    top.setWait(true);
    partOfAfterValidate = p_partOfAfterValidate;
    partOfAfterCancel = p_partOfAfterCancel;

    if (!p_from)
        p_from = LOADCATFROM.UNDEFINED;


    //KHA : refacto avec une enum à la place des 3 bool
    //var bFromFilter = false;
    //if (typeof (p_fromFilter) != "undefined") {
    //    // HLA - il faut egalement tester le 'true' car la string "true" != de la string "1" - #61219
    //    bFromFilter = p_fromFilter === true || p_fromFilter + "" == "1";
    //}

    //var bFromTreat = false;
    //if (typeof (p_fromTreat) != "undefined") {

    //    // HLA - il faut egalement tester le 'true' car la string "true" != de la string "1" - #61219
    //    bFromTreat = p_fromTreat === true || p_fromTreat + "" == "1";
    //}

    //var bFromAdmin = false;
    //if (typeof (p_fromAdmin) != "undefined") {
    //    // HLA - il faut egalement tester le 'true' car la string "true" != de la string "1" - #61219
    //    bFromAdmin = p_fromAdmin === true || p_fromAdmin + "" == "1";
    //}

    var catWidth = 800;
    var catHeight = 550;
    if (p_bMulti) {
        catMultiple = "1";
        catWidth = 850;
        catHeight = 530;
    }
    if (p_btreeView) {
        catWidth = 850;
        catHeight = 614;
    }

    // Freecode CRU 14122015 : Ajout du libellé de catalogue dans le titre de la modale
    var modalTitle = top._res_225;
    try {
        if (p_CatTitle) {
            modalTitle = modalTitle + ' : ' + decode(p_CatTitle);
        }
    }
    catch (e) { }
    catalogDialog = new eModalDialog(modalTitle, 0, "eCatalogDialog.aspx", catWidth, catHeight);
    catalogDialog.ErrorCallBack = function () { top.setWait(false); };
    catalogDialog.onIframeLoadComplete = function () { top.setWait(false); };

    catalogDialog.addParam("CatDescId", p_catDescId, "post");
    if (p_catParentValue != null)
        catalogDialog.addParam("CatParentValue", p_catParentValue, "post");
    catalogDialog.addParam("CatBoundPopup", p_catBoundPopup, "post");
    catalogDialog.addParam("CatBoundDescId", p_catBoundDescId, "post");

    catalogDialog.addParam("CatAction", "ShowDialog", "post");
    catalogDialog.addParam("CatPopupType", p_catPopupType, "post");
    catalogDialog.addParam("CatMultiple", p_bMulti ? "1" : "0", "post");
    catalogDialog.addParam("CatSearch", "", "post");
    catalogDialog.addParam("CatInitialValues", p_defValue, "post");
    catalogDialog.addParam("CatEditorJsVarName", p_JsVarName, "post");
    catalogDialog.addParam("CatTitle", p_CatTitle, "post");
    catalogDialog.addParam("treeview", p_btreeView ? "true" : "false", "post");
    catalogDialog.addParam("FrameId", catalogDialog.iframeId, "post");

    catalogDialog.addParam("From", p_from, "post");
    //catalogDialog.addParam("FromFilter", p_from == LOADCATFROM.FILTER ? "1" : "0", "post");
    //catalogDialog.addParam("FromTreat", p_from == LOADCATFROM.TREAT ? "1" : "0", "post");
    //catalogDialog.addParam("FromAdmin", p_from == LOADCATFROM.ADMIN ? "1" : "0", "post");

    if (p_bMailTemplate)
        catalogDialog.addParam("MailTemplate", p_bMailTemplate ? "true" : "false", "post");

    if (p_sourceFldId || p_targetFldId) {
        var sourceElement = document.getElementById(p_sourceFldId || p_targetFldId);
        var sourceAction = getAttributeValue(sourceElement, "eaction");
        if (sourceAction == "LNKCATDESC") {
            catalogDialog.addParam("CatSource", "1", "post");
            catalogDialog.addParam("DescType", getAttributeValue(sourceElement, "data-desct"), "post");
            catalogDialog.addParam("DescTypeIds", getAttributeValue(sourceElement, "data-desctids"), "post");

        }
        else if (sourceAction == "LNKCATENUM") {
            catalogDialog.addParam("CatSource", "2", "post");
            catalogDialog.addParam("EnumType", getAttributeValue(sourceElement, "data-enumt"), "post");
        }
    }


    top.eTabCatModalObject.Add(catalogDialog.iframeId, catalogDialog);

    catalogDialog.show();
    catalogDialog.addButton(top._res_29, cancelCatGeneric, "button-gray", p_targetFldId, "cancel"); // Annuler
    catalogDialog.addButton(top._res_28, validateCatGeneric, "button-green", p_sourceFldId + '|' + p_targetFldId, "ok"); // Valider

    return catalogDialog;
}

function cancelCatGeneric(trgId) {
    if (partOfAfterCancel && typeof partOfAfterCancel == "function") {
        partOfAfterCancel(catalogDialog, trgId);
    }
    catalogDialog.hide();
}

function validateCatGeneric(srcTrgId) {
    var oIDs = srcTrgId.split('|');
    var srcId = oIDs[0];
    var trgId = oIDs[1];
    var oFrm = catalogDialog.getIframe();

    var selectedListValues = oFrm.selectedListValues;
    if (typeof (selectedListValues) == 'undefined' && typeof (oFrm.eC) != 'undefined')
        selectedListValues = oFrm.eC.getSelectedListId();

    var tabSelectedValues = new Array();
    var tabSelectedLabels = new Array();

    var selectedIDs = "";

    if (oFrm.eC && oFrm.eC.treeview && oFrm.eC.selectedListValuesDico) {
        //Cas particulier pour treeview


        var keys = oFrm.eC.selectedListValuesDico.Keys;
        for (var i = 0; i < keys.length; i++) {

            var nId = keys[i];
            var sVal = oFrm.eC.selectedListValuesDico[nId];

            tabSelectedValues.push(nId);
            tabSelectedLabels.push(sVal);
            if (selectedIDs != "") {
                selectedIDs = selectedIDs + ";";
            }
            selectedIDs += nId;
        }
    }
    else {

        for (var i = 0; i < selectedListValues.length; i++) {
            if (selectedListValues[i] == "")    //le getElementById vide plante sous IE8
                continue;
            var oItem = oFrm.document.getElementById(selectedListValues[i]);

            if (!oItem)
                continue;
            var label = '';
            if (oItem.getAttribute('ednval') == "") {
                label = '';
            }
            else {
                var id_Lbl = selectedListValues[i].replace('val', 'lbl');
                id_Lbl = id_Lbl.replace('_sel', '');
                label = oFrm.document.getElementById(id_Lbl).innerText || oFrm.document.getElementById(id_Lbl).textContent; // #42895 CRU : On affiche la valeur texte et non innerHtml
                tabSelectedValues.push(oItem.getAttribute('ednval'));
                tabSelectedLabels.push(label);
            }

            if (oItem.getAttribute('ednid') != "") {
                if (selectedIDs != "") {
                    selectedIDs = selectedIDs + ";";
                }
                selectedIDs = selectedIDs + oItem.getAttribute('ednid');
            }
        }
    }

    if (partOfAfterValidate && typeof partOfAfterValidate == "function") {
        partOfAfterValidate(catalogDialog, srcId, trgId, tabSelectedLabels, tabSelectedValues, selectedIDs);
    }

    catalogDialog.hide();
}




// A partir des options tranmises, on gènère un catalog
eTools.showCatalogGenericView = function (data) {

    function setDefaultIfUndefined(key, defaultValue) {
        var op = data[key];
        if (typeof op == "undefined")
            data[key] = defaultValue;
    }

    setDefaultIfUndefined("bMulti", false);
    setDefaultIfUndefined("btreeView", "0");
    setDefaultIfUndefined("defValue", "");
    setDefaultIfUndefined("sourceFldId", null);
    setDefaultIfUndefined("targetFldId", null);
    setDefaultIfUndefined("catDescId", 0);
    setDefaultIfUndefined("catPopupType", 0);
    setDefaultIfUndefined("catBoundDescId", 0);
    setDefaultIfUndefined("catParentValue", false);
    setDefaultIfUndefined("CatTitle", null);
    setDefaultIfUndefined("JsVarName", "");
    setDefaultIfUndefined("bMailTemplate", false);
    setDefaultIfUndefined("partOfAfterValidate", null);
    setDefaultIfUndefined("partOfAfterCancel", null);
    setDefaultIfUndefined("from", LOADCATFROM.UNDEFINED);
    //setDefaultIfUndefined("fromFilter", false);
    //setDefaultIfUndefined("fromTreat", false);
    //setDefaultIfUndefined("fromAdmin", false);

    showCatGeneric(
        data.bMulti,
        data.btreeView,
        data.defValue,
        data.sourceFldId,
        data.targetFldId,
        data.catDescId,
        data.catPopupType,
        data.catBoundDescId,
        data.catBoundPopup,
        data.catParentValue,
        data.CatTitle,
        data.JsVarName,
        data.bMailTemplate,
        data.partOfAfterValidate,
        data.partOfAfterCancel,
        data.from);
}

// Avec certaines options du catalog en utilisant l'element header
eTools.showCatalogView = function (target, viewOptions) {
    var ename = getAttributeValue(target, "ename");
    if (!ename)
        return;

    var header = document.getElementById(ename);
    if (!header)
        return;

    eTools.showCatalogGenericView({
        "bMulti": getAttributeValue(header, "mult") == "1" ? true : false,
        "btreeView": getAttributeValue(header, "tree") == "1" ? true : false,
        "defValue": viewOptions.value,
        "sourceFldId": null,
        "targetFldId": null,
        "catDescId": getAttributeValue(header, "popid"),
        "catPopupType": getAttributeValue(header, "pop"),
        "catBoundDescId": getAttributeValue(header, "bndid"),
        "catBoundPopup": 0,
        "catParentValue": null,
        "CatTitle": viewOptions.title,
        "partOfAfterValidate": viewOptions.onValidate
    });
}


/*CATALOGUES UTILISATEURS*/

//TODO
/****************************************************************/



/// Created MOU 26/03/2014
/// Updated MOU 12/08/2014

///Objet qui permet de vérifier le format de données 
var eValidator = (function () {

    /* private */
    //Applique l'expression régulière definit pour ce type de format    
    function applyRegEx(object) {

        //si null ou undefined on mets "" pour executer match
        object.data = object.data + "";
        var regExp = new RegExp(object.pattern, object.modifiers);
        object.result = object.data.match(regExp);

        //une seule corespondance
        return object.result != null && object.result.length >= 1;
    };

    return {
        /* public */
        isValid: function (object) {

            if (!object && !object.format && object.value)
                throw "Invalid data object argment!";

            var nFormat = object.format ? object.format : getAttributeValue(object, "format") * 1;

            return this.isValueValid(nFormat, object.value);
        },

        isValueValid: function (nFormat, value) {

            if (!nFormat && !value)
                throw "Invalid data object argment!";

            if (nFormat === this.format.EMAIL)
                return this.isEmail(value);

            else if (nFormat === this.format.DATE)
                return this.isDate(value);

            else if (nFormat === this.format.NUMERIC)
                return this.isNumeric(value);

            //SHA : correction bug de la user story "SMS Net message > Envoi" à propos de l'alerte envoyée quand téléphone formaté (espace ou . ou - ou rien)
            else if (nFormat === this.format.PHONE)
                return this.isPhone(value);

            else if (nFormat === this.format.CURRENCY)
                return this.isCurrency(value);

            else if (nFormat === this.format.BIT)
                return this.isBit(value);

            else if (nFormat === this.format.GEO)
                return this.isGeo(value);

            else if (nFormat === this.format.WEB)
                return this.isUrl(value)
            else
                return true;    //Cas non gérés
        },

        isUrl: function (strUrl) {

            var regexp = /(ftp|http|https):\/\/(\w+:{0,1}\w*@)?(\S+)(:[0-9]+)?(\/|\/([\w#!:.?+=&%@!\-\/]))?/
            return regexp.test(strUrl);

        },

        isEmail: function (strMail) {

            var parts = strMail.split("@");
            if (parts.length != 2)
                return false;
            var secondParts = parts[1].split(".");
            if (secondParts.length < 2) //GCH : doit avoir minimum 1 point mais peut être > 2 si adresse avec plusieurs . dans la 2ème partie, ce qui est valide
                return false;

            // HLA - Gestion du label
            array = strMail.match(/(.*)[\[<](.*)[\]>]/i);
            if (array != null && array.length == 3)
                strMail = array[2];

            var strPattern = "^[a-z0-9!#$%&'*+\/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+\/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?$";
            return applyRegEx({
                data: strMail,
                pattern: strPattern,
                modifiers: "gi"  //(g) global match et (i) case insensitive
            });
        },

        isDate: function (strDate) {
            ///Format de date  JJ/MM/AAAA hh:mm(:ss)
            //var strPattern = "^([0-9]{1,2})+\/([0-9]{1,2})+\/([0-9]{4})+\s([0-9]{1,2})+:[0-9]{1,2}(:[0-9]{1,2})*$";
            //GCH : date+heure ou date
            var oRegExp = /^((\d{2})+\/(\d{2})+\/(\d{4})(\s(\d{2}):(\d{2})(:\d{2})*)*)$/g;  // entourer par / pour créer un objet regexp et ne pas avoir de pb d'échappement le g étant l'option global match
            return oRegExp.test(strDate);   //GCH : semble mieux fonctionner que le applyRegEx
        },

        isDateJS: function (strDate) {
            try {


                var aDate = strDate.split(" ");
                var aDatePart = aDate[0].split("/");

                var year = getNumber(eDate.GetDatePartMask("yyyy", strDate));

                if (year < 1753 || year > 9999) {

                    //date hors de plage
                    return false;
                }

                var month = getNumber(eDate.GetDatePartMask("MM", strDate));
                var day = getNumber(eDate.GetDatePartMask("dd", strDate));

                var hours = 0;
                var minutes = 0;
                var seconds = 0;

                if (aDate.length == 2) {
                    var aHourPart = aDate[1].split(":");

                    hours = getNumber(aHourPart[0]);
                    minutes = getNumber(aHourPart[1]);

                }


                var madate = new Date(year, month - 1, day, hours, minutes, 0, 0)

                return (
                    madate.getFullYear() == year
                    && madate.getMonth() + 1 == month
                    && madate.getDate() == day

                    && madate.getMinutes() == minutes
                    && madate.getHours() == hours

                );

            }
            catch (e) {
                var err = e.message;
                return false;
            }

        },

        isNumeric: function (strNumber) {
            var strPattern = "^(-|\\+)?\\d+((\\.|,)\\d+)?$";
            return applyRegEx({
                data: strNumber,
                pattern: strPattern,
                modifiers: "g"
            });
        },

        //SHA : correction bug de la user story "SMS Net message > Envoi" à propos de l'alerte envoyée quand téléphone formaté (espace ou . ou - ou rien)
        isPhone: function (strPhone) {
            //regex à améliorer : //ALISTER => Demande/Request 85805
            var strPattern = /^(?:(?:\(?(?:00|\+)([1-4]\d\d|[1-9]\d?)\)?)?[\s\-\.\ \\\/]?)?((?:\(?\d{1,}\)?[\s\-\.\ \\\/]?){0,})(?:[\s\-\.\ \\\/]?(?:#|ext\.?|extension|x)[\s\-\.\ \\\/]?(\d+))?$/;
            //var strPattern = "^(\\+|\\-|\\_|\\.|\\ |[0-9])*$";
            //var strPattern = "^((((\\+|00)[1-9]{1}[0-9]{1,2}|0[1-9]{1}))(([0-9]{2}){4}|(\\s+[0-9]{2}){4}|(-[0-9]{2}){4}|(\\.[0-9]{2}){4})){1}$";
            return applyRegEx({
                data: strPhone,
                pattern: strPattern,
                modifiers: "g"
            });
        },

        isCurrency: function (strNumber) {
            var strPattern = "^[0-9]+(,[0-9]{2})?$";
            return applyRegEx({
                data: strNumber,
                pattern: strPattern,
                modifiers: "g"
            });
        },

        isBit: function (strBit) {
            var strPattern = "^(0|1|true|false)+$";
            return applyRegEx({
                data: strBit,
                pattern: strPattern,
                modifiers: "gi"
            });
        },

        isGeo: function (sGeo) {
            sPatternCoord = "-?[0-9]+[\\.[0-9]+]? +-?[0-9]+[\\.[0-9]+]?";
            if (sGeo & "" == "")
                return true;

            var obj = {
                data: sGeo,
                pattern: "^POINT *\\( *(" + sPatternCoord + ") *\\)$",
                modifiers: ""
            }
            var bPoint = applyRegEx(obj);
            if (bPoint && obj.result.length != 2)
                return false;

            var bPolygon = false;
            if (!bPoint) {
                //verif polygone
                obj = {
                    data: sGeo,
                    pattern: "^POLYGON *\\(\\( *(" + sPatternCoord + ")( *, *" + sPatternCoord + "){3,} *\\)\\)$",
                    modifiers: ""
                }

                bPolygon = applyRegEx(obj);

            }

            if (!bPoint && !bPolygon)
                return false;


            //on récupère la liste des points fournis
            obj = {
                data: sGeo,
                pattern: sPatternCoord,
                modifiers: "g"
            }

            applyRegEx(obj);

            //Prévalidation des coordonnées
            var aFirstPoint = new Object();
            var aLastPoint = new Object();
            for (var i = 0; i < obj.result.length; i++) {
                var aCoord = obj.result[i].split(" ");
                var long = parseFloat(aCoord[0].trim());
                var lat = parseFloat(aCoord[1].trim());

                if (isNaN(long) || isNaN(lat))
                    return false;

                //La latitude doit se trouver entre 90 et -90 degrés
                if (Math.abs(lat) > 90)
                    return false;

                if (i == 0) {
                    aFirstPoint.lat = lat;
                    aFirstPoint.long = long;
                }
                else if (i == obj.result.length - 1) {
                    aLastPoint.lat = lat;
                    aLastPoint.long = long;
                }
            }

            if (bPolygon) {
                //vérifier que les premiers et derniers points ont les mêmes coordonnées
                if (aFirstPoint.lat != aLastPoint.lat || aFirstPoint.long != aLastPoint.long)
                    return false;
            }

            return true;
        },

        format: {
            // Mêmes format que desc
            HIDDEN: 0,
            CHAR: 1,
            DATE: 2,
            BIT: 3,
            AUTOINC: 4,
            MONEY: 5,
            EMAIL: 6,
            WEB: 7,
            USER: 8,
            MEMO: 9,
            NUMERIC: 10,
            FILE: 11,
            PHONE: 12,
            IMAGE: 13,
            GROUP: 14,
            TITLE: 15,
            IFRAME: 16,
            CHART: 17,
            COUNT: 18,
            RULE: 19,
            ID: 20,
            BINARY: 21,
            GEOGRAPHY: 24,
            CURRENCY: 5
        }

        // Types des valeurs possibles en admin
        //adminFormat: {
        //    ADM_TYPE_BIT: 0,
        //    ADM_TYPE_CHAR: 1,
        //    ADM_TYPE_NUM: 2,
        //    ADM_TYPE_MEMO: 3,
        //    ADM_TYPE_PICTO: 4,
        //    ADM_TYPE_HIDDEN: 5,
        //    ADM_TYPE_FIELDTYPE: 6,
        //    ADM_TYPE_RADIO: 7
        //}
    };
})();








function replaceRes(oError) {
    try {

        //regexp de capture
        var reg = "{{(\d+)}}";

        //fonction de remplacement
        var replaceRes = function (cor, gp) {
            return top["_res_" + gp];
        }

        oError.Title = oError.Title.replace(/{{(\d+)}}/g, replaceRes);
        oError.Msg = oError.Msg.replace(/{{(\d+)}}/g, replaceRes);

        oError.DetailMsg = oError.DetailMsg.replace(/{{(\d+)}}/g, replaceRes);
        oError.DetailDev = oError.DetailDev.replace(/{{(\d+)}}/g, replaceRes);
    }
    catch (e) {
        return oError;
    }

    return oError;
}

///Affiche un message d'alert passant au dessus-d'un eventuel setWait
function eAlertError(oError, fctCallBack, width, height) {

    if (typeof (eAlert) == "undefined")
        return;
    if (typeof (width) == "undefined")
        width = null;
    if (typeof (height) == "undefined")
        height = null;

    oError = replaceRes(oError);

    var myAlertError = eAlert(oError.Type, oError.Title, oError.Msg, oError.DetailMsg + '\n<!--' + encodeHTMLEntities(oError.DetailDev) + '-->', width, height, fctCallBack, true, true);

    //KHA le 08/03/2018 Attention cette fonction doit rester générique. 
    //la fermeture de oModalPJAdd est géré dans la fonction de retour de ePjAdd.aspx via la méthode LaunchErrorHTML
    // Référence appelé pour fermer la fenetre
    //if (top.oModalPJAdd)
    //    top.oModalPJAdd.hide();

    // Pour pouvoir la fermer de l'exterieur
    //top.oModalPJAdd = myAlertError;

    if (myAlertError && myAlertError.getDivContainer) {
        myAlertDiv = myAlertError.getDivContainer();
    }

    return myAlertError;
}

/*DATE INTERNATIONNALISATION*************************************/
//<className>eDate</className>
//<summary>objet de gestion de l'affichage des dates dans XRM (GCH #36022)</summary>
//<purpose></purpose>
//<authors>GCH</authors>
//<date>2015-01-15</date>
//<sample>
//exemple de valeurs retournées en mode Big endian :
//    eDate.ConvertBddToDisplay("24/12/2014 10:00")
//        "2014/12/24 10:00"
//    eDate.ConvertBddToDisplay("24/12/2014")
//        "2014/12/24"
//    eDate.ConvertBddToDisplay("24/12/2014 10:00:23")
//        "2014/12/24 10:00:23"
//    eDate.ConvertDisplayToBdd("2014/12/21 10:00:23")
//        "21/12/2014 10:00:23"
//
// exemples avec un objet Date en paramètre :
//    eDate.ConvertDtToDisplay(new Date("12/22/2014 10:00:02"))
//    "2014/12/22 10:00:02"
//    eDate.ConvertDtToDisplay(new Date("12/22/2014 10:00"))
//    "2014/12/22 10:00"
//    eDate.ConvertDtToDisplay(new Date("12/22/2014"))
//    "2014/12/22"
//    eDate.ConvertDtToBdd(new Date("10/22/2014 10:00"))
//    "22/10/2014 10:00"
//    eDate.ConvertDtToBdd(new Date("10/22/2014 10:00:02"))
//    "22/10/2014 10:00:02"
//</sample>
var eDate = (function () {
    /*Début Variables Privées***********************************************/
    var that = this;

    var _isInit = false;    //Paramètres de base déjà initialisés
    var _isValid;

    var _cultureInfoDate = '';  //
    /*Fin Variables Privées*************************************************/

    /*Début Constantes privée***********************************************/
    var FormatLittleEndian = "dd/MM/yyyy";  //format en bdd
    var FormatRFC3339 = "yyyy-MM-dd";  //format en bdd
    var FormatYear = "yyyy";    //format Année
    var FormatMonth = "MM"; //format Mois
    var FormatDay = "dd";   //format Jours
    var FormatHour = "HH";  //format Heure
    var FormatMinute = "mm";    //format Minute
    var FormatSecond = "ss";    //format Seconde
    var FormatList = [FormatYear, FormatMonth, FormatDay, FormatHour, FormatMinute, FormatSecond];
    /*Fin Constantes privée*************************************************/

    /*Début Méthodes Privées***********************************************/
    //initialisation des paramètres
    var initParam = function () {
        _isValid = true;
        //Initialisation qu'une seule fois
        if (!_isInit) {
            var paramFct = getParamWindow ? getParamWindow : top.getParamWindow;
            if (!paramFct) return false;
            var oeParam = paramFct();
            if (!oeParam) return false;

            _cultureInfoDate = oeParam.GetParam("CultureInfoDate");

            _isInit = true;  //ok
        }
    };

    //Retourne le masque des heures/minutes/secondes en fonction de la date passée en paramètre
    var getTimeMask = function (sValue) {
        var sTimeMask = "";
        if (sValue.length >= 11) {
            sTimeMask = FormatHour + ":" + FormatMinute;
            if (sValue.length >= 17)
                sTimeMask = sTimeMask + ":" + FormatSecond;
        }
        return sTimeMask;
    };

    //  Convertit une date en lui indiquant le masque de date actuelle et le masque souhaité (sans indiquer les heures car elles sont déduites de la date passée en paramètre)
    //sDate : Date à convertir
    //sMaskParamDate : Masque actuel de la date
    //sMaskExpected : Masque à appliquer
    //Retourne la date convertie
    var convertDateMask = function (sDate, sMaskParamDate, sMaskExpected) {
        if (sMaskParamDate == sMaskExpected)
            return sDate;

        var sTimeMask = getTimeMask(sDate);
        if (sTimeMask.length > 0)
            sTimeMask = " " + sTimeMask;
        var sMaskOrig = sMaskParamDate + sTimeMask;
        var sDateFound = sMaskExpected + sTimeMask;

        for (i = 0; i < FormatList.length; i++) {
            sDateFound = sDateFound.replace(FormatList[i]
                , getValueFromMask(sMaskOrig, FormatList[i], sDate)
            );
        }
        if (sDateFound.length != sDate.length)
            return sDate;
        return sDateFound;
    };

    //  Retourne la partie de chaine se trouvant au même emplacement que le morceau de masque
    //exemple :
    //  getValueFromMask("tititoto", "toto", "20142015")
    //  retourne 2015
    //sMask : masque complet
    //sPartOfMask : morceaux de masque à trouver
    //sValueToParse : Valeur à filtrer avec le masque
    var getValueFromMask = function (sMask, sPartOfMask, sValueToParse) {
        var nCharBegin = sMask.indexOf(sPartOfMask);
        if (nCharBegin < 0)
            return "";
        var nCharEnd = sPartOfMask.length;
        return sValueToParse.substring(nCharBegin, nCharBegin + nCharEnd);
    };

    var convertDateMaskDt = function (dtDate, sMaskParamDate, sMaskExpected) {
        var sDate = getStringFromDate(dtDate, (dtDate.getHours() <= 0 && dtDate.getMinutes() <= 0 && dtDate.getSeconds() <= 0), false);
        return convertDateMask(sDate, sMaskParamDate, sMaskExpected);
    };

    //  Retourne le format francais à partir d'un objet date
    var getStringFromDate = function (dDate, bOnlyDate, bOnlyTime) {
        if (isNaN(dDate))
            return;

        var strDate = "";

        if (!bOnlyTime) {
            strDate = makeTwoDigit(dDate.getDate()) + "/" + makeTwoDigit(dDate.getMonth() + 1) + "/" + makeTwoDigit(dDate.getFullYear());
        }
        if (!bOnlyDate) {
            if (strDate != "")
                strDate += " ";
            strDate += makeTwoDigit(dDate.getHours()) + ":" + makeTwoDigit(dDate.getMinutes());

            if (dDate.getSeconds() > 0)
                strDate += ":" + makeTwoDigit(dDate.getSeconds());
        }
        return (strDate);
    };


    //  Retourne l'objet date à partir d'une chaine au format francais
    var getDateFromString = function (s) {
        if (!s || s == "")
            return;
        s = s.replace(/\//g, "-").replace(/ /g, "-").replace(/:/g, "-");

        var dReturn = new Date();

        var aDate = s.split('-');

        var day = aDate[0];
        var month = aDate[1];
        var year = aDate[2];

        var hour = 0;
        var mn = 0;
        var sc = 0;

        if (aDate.length >= 4)
            hour = aDate[3];
        if (aDate.length >= 5)
            mn = aDate[4];
        if (aDate.length >= 6)
            sc = aDate[5];

        dReturn = new Date(year, month - 1, day, hour, mn, sc, 0);
        return dReturn;
    };



    //  Convertit un nombre en string en ajoutant un 0 à gauche si < 10
    var makeTwoDigit = function (nValue) {
        var nValue = parseInt(nValue, 10);
        if (isNaN(nValue))
            nValue = 0;
        if (nValue < 10)
            return ("0" + nValue);
        else
            return (nValue);
    };
    /*Fin Méthodes Privées*************************************************/
    return {
        /*Début Méthodes publics***********************************************/
        //Permet de définir la culture info autrement qu'avec la manière native de l'application XRM
        //  sCultureInfo : format de date à utiliser
        SetCultureInfo: function (sCultureInfo) {
            _cultureInfoDate = sCultureInfo;
            _isInit = true;
        },


        GetDatePartMask: function (sPartOfMask, sValueToParse) {

            initParam();
            return getValueFromMask(_cultureInfoDate, sPartOfMask, sValueToParse);

        },


        //Retourne la date local
        //sFrDate : la date en francais 
        ConvertBddToDisplay: function (sBddDate) {
            initParam();
            //test sBddDate
            if (sBddDate == "")
                return sBddDate;
            if (!eValidator.isDate(sBddDate)) {
                _isValid = false;
                return sBddDate;
            }

            var sDisplayDate = convertDateMask(sBddDate, FormatLittleEndian, _cultureInfoDate);

            return sDisplayDate;
        },

        //Retourne la culture info en cours
        CultureInfoDate:
            function () {
                initParam();
                return _cultureInfoDate;
            },

        //Retourne la date à utiliser en BDD
        //sBddDate : la date affichée
        ConvertDisplayToBdd:
            function (sDisplayDate) {
                initParam();
                if (sDisplayDate == "")
                    return sDisplayDate;

                var sBddDate = convertDateMask(sDisplayDate, _cultureInfoDate, FormatLittleEndian);
                //test sBddDate 
                if (!eValidator.isDate(sBddDate)) {
                    _isValid = false;
                    return sDisplayDate;
                }
                return sBddDate;
            },
        //Retourne la date à utiliser en BDD
        //sBddDate : la date affichée
        ConvertDisplayToFormatRFC3339:
            function (sDisplayDate) {

                initParam();
                if (sDisplayDate == "")
                    return sDisplayDate;



                var sBddDate = convertDateMask(sDisplayDate, _cultureInfoDate, FormatLittleEndian);
                //test sBddDate 
                if (!eValidator.isDate(sBddDate)) {
                    _isValid = false;
                    return sDisplayDate;
                }

                sBddDate = convertDateMask(sDisplayDate, _cultureInfoDate, FormatRFC3339);
                return sBddDate;
            },

        //Retourne la date affichée en fonction du format local d'affichage depuis un objet date
        ConvertDtToDisplay: function (dtDate) {
            initParam();
            return convertDateMask(convertDateMaskDt(dtDate), FormatLittleEndian, _cultureInfoDate);
        },
        //Retourne la date à utiliser en BDD depuis un objet date
        ConvertDtToBdd: function (dtDate) {
            initParam();
            return convertDateMask(convertDateMaskDt(dtDate), FormatLittleEndian, FormatLittleEndian);
        },
        IsValid: function () { return _isValid },
        //Début Outils génériques de dates*******
        Tools: {
            GetStringFromDate: function (dDate, bOnlyDate, bOnlyTime) {
                return getStringFromDate(dDate, bOnlyDate, bOnlyTime);
            },
            GetStringFromDateWithTiret: function (dDate, bOnlyDate, bOnlyTime) {
                return getStringFromDate(dDate, bOnlyDate, bOnlyTime).replace(/\//g, "-").replace(/ /g, "-").replace(/:/g, "-");
            },
            GetDateFromString: function (strDate) {
                return getDateFromString(strDate);
            },
            MakeTwoDigit: function (nValue) {
                return makeTwoDigit(nValue);
            }
        }
        //Fin Outils génériques de dates*******

        /*Fin Méthodes publics*************************************************/
    }
})();


/*NUMERIQUE INTERNATIONNALISATION*************************************/
//<className>eNumber</className>
//<summary>objet de gestion de l'affichage des numeriques dans XRM (GCH #37681, #36869)</summary>
//<purpose></purpose>
//<authors>GCH</authors>
//<date>2015-03-20</date>
//<sample>
//exemple pour séparateurs du client : millier - "," decimale "."
//    eNumber.ConvertBddToDisplay("10 000 000 001,55")
//    "10,000,000,001.55"
//    eNumber.ConvertDisplayToBdd("10,000,000,001.55")
//    "10 000 000 001,55"
//    eNumber.ConvertBddToDisplay("10000000001,55")
//    "10000000001.55"
//    eNumber.ConvertDisplayToBdd("10000000001.55")
//    "10000000001,55"
//    eNumber.ConvertNumToBdd(700000.5)
//    "700000,5"
//    eNumber.ConvertNumToDisplay(700000.5)
//    "700000.5"
//    eNumber.ConvertNumToDisplayFull(700000.5)
//    "700,000.5"
//</sample>
var eNumber = (function () {
    /*Début Variables Privées***********************************************/
    var that = this;

    var _isInit = false;    //Paramètres de base déjà initialisés

    var _isValid;
    var _initParamCustom = null;

    var _numberDecimalDelimiter = '';  //Séparateur de decimale du client
    var _numberSectionsDelimiter = '';  //Séparateur de millier du client
    /*Fin Variables Privées*************************************************/

    /*Début Constantes privée***********************************************/
    var FRDELIMITERDECIMAL = ",";   //Séparateur de decimale de la BDD
    var FRDELIMITERSECTION = " ";   //Séparateur de millier de la BDD
    /*Fin Constantes privée*************************************************/

    /*Début Méthodes Privées***********************************************/
    //initialisation des paramètres
    var initParam = function () {
        _isValid = true;
        //Initialisation qu'une seule fois
        if (!_isInit) {
            _numberDecimalDelimiter = FRDELIMITERDECIMAL;  //Séparateur de decimale du client par défaut
            _numberSectionsDelimiter = FRDELIMITERSECTION;  //Séparateur de millier du client par défaut

            var paramFct = getParamWindow ? getParamWindow : top.getParamWindow;
            if (!paramFct) return false;
            var oeParam = paramFct();
            if (!oeParam) return false;

            _numberDecimalDelimiter = oeParam.GetParam("NumberDecimalDelimiter");
            _numberSectionsDelimiter = oeParam.GetParam("NumberSectionsDelimiter");
            _isInit = true;  //ok
        }
    };
    /*Permet d'indiquer si le numérique est valide
    strNumber : chaine numerique au format de la BDD    
    */
    var validation = function (strNumber) {
        var oRegExp = /^-?[0-9]+(,[0-9]+)?$/g;  // entourer par / pour créer un objet regexp et ne pas avoir de pb d'échappement le g étant l'option global match

        return oRegExp.test(strNumber.replace(new RegExp("[" + FRDELIMITERSECTION + "]", "g"), ""));   //GCH : semble mieux fonctionner que le applyRegEx
    };

    /*Permet de convertir un numérique au format de la BDD sans les séparateurs de millier*/
    var convertNumToFrDate = function (num) {

        var sNumOrig = num + "";
        var sNum = sNumOrig;

        initParam();


        //retire le séparateur de millier  et remplace le séparateur de décimal "utilisateur" par celui bdd
        sNum = sNum.replace(new RegExp("[" + _numberSectionsDelimiter + "]", "g"), "").replace(new RegExp("[" + _numberDecimalDelimiter + "]", "g"), FRDELIMITERDECIMAL);
        _isValid = validation(sNum);


        if (!_isValid && _numberSectionsDelimiter != FRDELIMITERDECIMAL && _numberDecimalDelimiter == "," && sNumOrig.split(".").length == 2) {
            sNum = num + "";
            sNum = sNum.replace(new RegExp("[" + _numberSectionsDelimiter + "]", "g"), "").replace(new RegExp("[.]", "g"), FRDELIMITERDECIMAL);
            _isValid = validation(sNum);

        }

        return sNum;
    }

    /*
    Permet de remplacer les delimiter de decimale et de millier par d'autres dans une chaine
    sDecimalDelimiterSrc : sigle decimale du nombre passé en paramètre
    sSectionsDelimiterSrc : sigle millier du nombre passé en paramètre
    sDecimalDelimiterDest : sigle decimale du nombre à retourner
    sSectionsDelimiterDest : sigle millier du nombre à retourner
    */
    var swhitchNumberFormberDisplayed = function (sNum, sDecimalDelimiterSrc, sSectionsDelimiterSrc, sDecimalDelimiterDest, sSectionsDelimiterDest) {
        var tabNum = sNum.split(sDecimalDelimiterSrc);
        var sNumDest = "";
        for (var i = 0; i < tabNum.length; i++) {
            if (sNumDest != "")
                sNumDest = sNumDest + sDecimalDelimiterDest;
            sNumDest = sNumDest + tabNum[i].replace(new RegExp("[" + sSectionsDelimiterSrc + "]", "g"), sSectionsDelimiterDest);
        }
        return sNumDest;

    }

    /*Fin Méthodes Privées*************************************************/
    return {
        /*Début Méthodes publics***********************************************/
        SetDecimalDelimiter: function (sDelimiter) {
            _numberDecimalDelimiter = sDelimiter;
            if (_numberSectionsDelimiter == _numberDecimalDelimiter)
                _numberSectionsDelimiter = "";
            _isInit = true;
        },
        SetSectionDelimiter: function (sDelimiter) {
            _numberSectionsDelimiter = sDelimiter;
            if (_numberSectionsDelimiter == _numberDecimalDelimiter)
                _numberDecimalDelimiter = "$undefinedDec$";
            _isInit = true;
        },
        //Retourne le numérique au format du client
        //sBddNum : la chaine du numérique de la BDD 
        ConvertBddToDisplay: function (sBddNum) {
            initParam();
            if (sBddNum == "")
                return sBddNum;
            if (!validation(sBddNum)) {
                _isValid = false;
                return sBddNum;
            }


            var sNumReturn = swhitchNumberFormberDisplayed(sBddNum, FRDELIMITERDECIMAL, FRDELIMITERSECTION, _numberDecimalDelimiter, _numberSectionsDelimiter);

            return sNumReturn;
        },
        //Retourne le numérique à utiliser en BDD
        //sDisplayNum : la chaine du numérique au format du client 
        //bRemoveSectionDelimiter : (non obligatoire) si à true, force le retrait des séparaterus de millier
        ConvertDisplayToBdd: function (sDisplayNum, bRemoveSectionDelimiter) {
            initParam();
            if (sDisplayNum == "")
                return sDisplayNum;
            var sNumReturn = swhitchNumberFormberDisplayed(sDisplayNum, _numberDecimalDelimiter, _numberSectionsDelimiter, FRDELIMITERDECIMAL
                , (bRemoveSectionDelimiter) ? "" : FRDELIMITERSECTION);
            if (!validation(sNumReturn)) {
                _isValid = false;
                return sDisplayNum; //on retourne le nombre en paramètre
            }
            return sNumReturn;
        },

        //Convertit un nombre au format bdd pour l'afficher au format du client sans délimiteur de millier
        ConvertNumToDisplay: function (num) {

            return this.ConvertBddToDisplay(convertNumToFrDate(num));
        },

        //Convertit un nombre du format bdd au format client avec séparateur de décimal
        ConvertBddToDisplayFull: function (num) {


            initParam();
            num = num + "";
            if (!validation(num)) {
                _isValid = false;
                return num; //on retourne le nombre en paramètre
            }

            var aNumb = num.split(FRDELIMITERDECIMAL); // séparation partie entier/décimal
            var sPartInt = aNumb[0]; // partie entière
            var sPartDec = aNumb.length > 1 ? _numberDecimalDelimiter + aNumb[1] : ''; //partie décimal

            var myRegExp = /(\d+)(\d{3})/g; // on recherche des chiffres suivi directement de 3 chiffres


            if (_numberSectionsDelimiter !== "" && sPartInt > 3) {
                while (myRegExp.test(sPartInt)) {
                    sPartInt = sPartInt.replace(myRegExp, '$1' + _numberSectionsDelimiter + '$2');
                }
            }

            return sPartInt + sPartDec;
        },

        //Convertit un nombre pour l'afficher au format de la BDD (FR) sans séparateur de millier
        ConvertNumToBdd: function (num) {
            initParam();
            return convertNumToFrDate(num);
        },


        //Indique si le numérique covnertit est valide
        IsValid: function () { return _isValid; }
        /*Fin Méthodes publics*************************************************/
    }
})();



/*EUDO - Récup d'un paramètre d'une query string*/
/*urlParam : non obligatoire, si non renseigné prend les param en entête*/
function URL_getParameterByName(name, urlParam) {
    if (!urlParam)
        urlParam = location.search;
    name = name.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
    var regex = new RegExp("[\\?&]" + name + "=([^&#]*)"),
        results = regex.exec(urlParam);
    return results == null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
}

//Indique si le format en paramètre est de type numérique (AUTOINC, COUNT, ID, NUMERIC ou MONEY)
function isFormatNumeric(sFormat) {
    return (sFormat == FLDTYP.AUTOINC
        || sFormat == FLDTYP.COUNT
        || sFormat == FLDTYP.ID
        || sFormat == FLDTYP.NUMERIC
        || sFormat == FLDTYP.MONEY);
}


function evalInContext(js, context) {
    return function () { return eval(js); }.call(context);
}

/// Trace les erreurs a la demande. Faut faire : eTools.log.activate() dans la console de debug
eTools.log = (function () {

    //On a besoin que d'une seule instance
    if (top.eTools && top.eTools.log)
        return top.eTools.log;


    activate(true);

    var tabulation = " ", maxDepth;

    function space() {
        return tabulation;
    };
    function trace(msg) {
        if (console.log)
            console.log(msg);
    }
    function initAndSpy(obj) {
        maxDepth = 3;
        return spy("{", obj, "}");
    };
    function spy(open, obj, close) {
        var str = open, type = "", marginLeft = space();

        if (maxDepth == -1)
            return open + "..." + close;


        for (var key in obj) {

            if (!obj.hasOwnProperty(key))
                continue;

            type = typeof (obj[key]);

            if (type && type.toLowerCase() == 'object') {
                str += marginLeft + key + " : " + spy("{", obj[key], "}") + ", ";
                continue;
            }

            if (type && type.toLowerCase() == 'function') {
                str += marginLeft + key + " : function, ";
                continue;
            }

            if (type && type.toLowerCase() == 'array') {
                str += marginLeft + key + " :  " + spy("[", obj[key], "]") + ", ";
                continue;
            }

            if (type != 'undefined')
                str += marginLeft + key + " : " + obj[key] + ", ";
            else
                str += marginLeft + key + " : undefined, ";
        }

        maxDepth--;
        return str.substring(0, str.length - 2) + " " + close;
    };

    function sp(nb) {
        var str = "";
        for (var i = 0; i < nb; i++)
            str += " ";

        return str;
    };

    function clear() {
        console.clear();
    };
    function log(line, css) {
        console.log("%c" + line, css);
    }
    function getRandomInt(min, max) {
        return Math.floor(Math.random() * (max - min)) + min;
    }

    //Par defaut est désactivé
    function activate(bActivate) {
        if (top) {
            if (bActivate) {
                top.traceEnabled = true;
                return 'log activated';
            } else {
                top.traceEnabled = false;
                return 'log désactivé';
            }
        }
        else
            return 'Can\'t activate log, Top element is undefined';
    }

    return {

        activate: function (bActivate) {
            activate(bActivate);
        },
        write: function (msg) {
            if (top && top.traceEnabled)
                trace(msg);
        },
        inspect: function (obj) {
            if (top && top.traceEnabled && obj)
                trace(initAndSpy(obj));
        }
    }
}());

//Function qui récupère le innerHTML
eTools.getInnerHtml = function (element) {

    if (typeof (element.innerHTML) !== 'undefined')
        return element.innerHTML;

    if (element.firstChild && typeof (element.firstChild.innerHTML) !== 'undefined')
        return element.firstChild.innerHTML;

    if (element.firstChild && typeof (element.firstChild.nodeValue) !== 'undefined')
        return element.firstChild.nodeValue;

    return GetText(element);
};

// Renvoie la taille de police actuellement choisie par l'utilisateur
eTools.GetFontSize = function () {
    var oeParam = top.getParamWindow();
    var fsize = oeParam.GetParam('fontsize');
    if (fsize == "")
        fsize = "8";

    return getNumber(fsize);

}

/**
 * Renvoie la classe CSS correspondant à la taille de police souhaitée
 * @param {any} nTargetFontSize Taille de police désirée (si non précisée, récupère la taille de police actuellement choisie par l'utilisateur)
 */
eTools.GetFontSizeClassName = function (nTargetFontSize) {
    var nTargetFontSize = nTargetFontSize || top.eTools.GetFontSize();
    var sFontCss = "fs_" + nTargetFontSize + "pt";
    return sFontCss;

}

// Indique si le thème ciblé impose une taille de police maximale ou non (si rien n'est précisé, renvoie 0 pour les anciens thèmes ou 8 pour les nouveaux thèmes)
eTools.GetMaxFontSize = function (nNewThemeId) {
    var maxFontSize = 0;
    var targetTheme = document.querySelector("a[thid='" + nNewThemeId + "']");
    if (targetTheme) {
        var mxfontAttr = getAttributeValue(targetTheme, "mxfont");
        if ((mxfontAttr + "").length > 0) {
            maxFontSize = mxfontAttr.split(";").map(function (a) { return a * 1 }).reduce(function (a, b) { return Math.max(a, b) })
        }
    }
    // Fallback : si l'information n'est pas indiquée sur la pastille de sélection du thème, on considère pour l'instant que tout thème 2019 = 8 pt. maximum
    //else {
    //    if (nNewThemeId == THEMES.ROUGE2019.Id || nNewThemeId == THEMES.BLEU2019.Id || nNewThemeId == THEMES.VERT2019.Id)
    //        maxFontSize = 8;
    //}
    return maxFontSize;
};

/**
 * Ajoute la CSS gérant la taille de police souhaitée, au document indiqué, si elle n'est pas déjà présente
 * @param {any} doc Document cible
 * @param {any} nTargetFontSize Taille de police désirée (si non précisée, récupère la taille de police actuellement choisie par l'utilisateur)
 * @param {Boolean} bReplaceExisting Si true, supprime toutes les CSS eFontSize_ existantes (sauf celle souhaitée) avant d'ajouter celle souhaitée
 */
eTools.UpdateDocCss = function (doc, nTargetFontSize, bReplaceExisting) {
    doc = doc || document;
    nTargetFontSize = nTargetFontSize || top.eTools.GetFontSize(); // utilisation de la taille passée en paramètre, ou, à défaut, celle d'eParamIFrame (en partant donc du principe qu'elle aura été changée via un SetParam())
    let style = doc.getElementsByTagName("link");

    // Partie 1 : remplacement/ajout du fichier CSS correspondant à la nouvelle taille
    if (bReplaceExisting) {
        // On ne supprime que les CSS "eFontSize" différentes de celle que l'on veut ajouter
        let existingFontCSSNames =
            [...style]
                .filter(stl => stl.rel == "stylesheet" && stl.href.includes("eFontSize_"))
                .map(stlFiltered => stlFiltered.href.slice(stlFiltered.href.indexOf("eFontSize_"), stlFiltered.href.toLowerCase().indexOf(".css")));
        existingFontCSSNames.filter(stlName => stlName != "eFontSize_" + nTargetFontSize)?.forEach(stlNameFiltered => removeCSS(stlNameFiltered, doc));
    }
    // Ajout si elle n'existe pas déjà (optimisation)
    let bTargetCSSExists = [...style].some(stl => stl.rel == "stylesheet" && stl.href.includes("eFontSize_"));
    if (!bTargetCSSExists)
        addCss("eFontSize_" + nTargetFontSize, "FONT", doc);
    
    // Partie 2 : remplacement/ajout de la classe CSS fs_* sur les éléments du DOM
    var sTargetFontSizeCssName = top.eTools.GetFontSizeClassName(nTargetFontSize);    
    // #6711 - On remplace toute trace de classe CSS correspondant à une tailel de police, par celle correspondant à la nouvelle
    let oAllElementsWithFSClass = doc.querySelectorAll('[class^=fs_][class$=pt]').forEach(
        domElement => {
            Array.from(domElement.classList).filter(className => className.startsWith("fs_")).forEach(
                filteredClassName => switchClass(domElement, filteredClassName, sTargetFontSizeCssName));
        }
    );  
    // Et dans tous les cas, on s'assure que la nouvelle soit au moins sur le body, qu'elle ait été présente auparavant ou non
    var mybody = doc.querySelector("body");
    if (!mybody.classList.contains(sTargetFontSizeCssName))
        addClass(mybody, sTargetFontSizeCssName);
}

///Met à jour le innerHTML d'un element
eTools.setInnerHtml = function (element, html) {

    if (typeof (element.innerHTML) !== 'undefined')
        return element.innerHTML = html;

    if (element.firstChild && typeof (element.firstChild.innerHTML) !== 'undefined')
        return element.firstChild.innerHTML = html;

    if (element.firstChild && typeof (element.firstChild.nodeValue) !== 'undefined')
        return element.firstChild.nodeValue = html;

    return SetText(element, html);
}

///Creation d'un objet de merge field inséré dans ckeditor
eTools.eMergeFieldData = function (sText, sData, sActiveDescId) {

    var text = sText;
    var data = sData;
    var activeDescid = sActiveDescId;


    // Ordre des valeurs séparées par ;
    // cf. eLibTools, GetMergeFieldsData
    // 0 : DescId
    // 1 : FieldType
    // 2 : FieldName
    // 3 : FieldFormat
    // 4 : FieldMultiple
    // 5 : FieldPopup
    // 6 : FieldPopupDescId
    // 7 : FieldBoundDescId
    // 8 : FieldReadOnly
    // 9 : FieldRequired

    var aField = data.split("\;");
    // le descid du champ
    var strFieldDescId = aField[0];
    // type du champ (formule, user...)
    var strFieldType = aField[1];
    // nom du champ (sum, avg, count...)
    var strFieldName = aField[2];
    // format du champ (caractère, numérique, mémo...à
    var strFieldFormat = aField[3];
    // champ à choix multiples ?
    var strFieldMultiple = aField[4];
    // liaisons
    var strFieldPopup = aField[5];
    var strFieldPopupDescId = aField[6];
    var strFieldBoundDescId = aField[7];
    var strFieldReadOnly = aField[8];
    var strFieldRequired = aField[9];

    function isCatalog() { return Number(strFieldPopup) > 0; };
    var Catalog = {
        // utilise les valeurs d'une rubrique - Liaisons de tables
        IsLink: function () {
            var nFieldDescId = Number(strFieldDescId);
            var nFieldPopupDescId = Number(strFieldPopupDescId);
            return nFieldDescId != nFieldPopupDescId && nFieldPopupDescId % 100 == 1 && strFieldPopup == "4"; // popuptype == SPECIAL
        },
        // lier le catalogue aux valeurs de la rubrique - Catalogue lié
        IsBound: function () { return Number(strFieldBoundDescId) > 0; }

    };

    return {

        // getters
        Text: text,
        DescId: strFieldDescId,
        Type: strFieldType,
        Name: strFieldName,
        Format: strFieldFormat,
        Multiple: strFieldMultiple,
        Popup: strFieldPopup,
        PopupDescId: strFieldPopupDescId,
        BoundDescId: strFieldBoundDescId,
        // le merge field est il editable ou pas
        IsEditable: function () {

            //la rubrique apartient a la table active : nGlobalActiveTab donc editable
            var bEditable = (Number(strFieldDescId) - Number(strFieldDescId) % 100) == getTabDescid(activeDescid);
            var readOnly = Number(strFieldReadOnly) == 1;

            //Pas editable on zappe
            //Les images ne sont pas editables 
            if (!bEditable || strFieldFormat == 13 || readOnly)
                return false;

            //L'implémentation des catalogues n'est pas encore terminée !
            if (!isCatalog())
                return true;

            //On exclut les catalogues liés et les catalogues de type liaison
            return !Catalog.IsLink() && !Catalog.IsBound();
        },
        ///Correspondance JS de l'enum CS FieldFormat de Enum.cs
        // A METTRE A JOUR EN PARALLELE
        FieldFormat: eTools.FieldFormat,
        IsRequired: function () {
            return Number(strFieldRequired) == 1;
        }
    }
}

/// retourne la fct de hide la modal fourni en param
eTools.GetHideModalFct = function (oMod, bCancelSetWait) {
    return function () {

        //Setwait a false
        if (bCancelSetWait && typeof (top.setWait) == "function")
            top.setWait(false);

        //Hide la modal
        if (oMod && oMod.isModalDialog) {
            oMod.hide();
        }
    }
}


eTools.GetModal = function (sHandle) {

    if (typeof (sHandle) == "string" && top.window["_md"] != null) {

        var oM = top.window["_md"][sHandle];
        if (oM && oM.isModalDialog)
            return oM;


    }
    return null;
}

eTools.GetModalFromId = function (sId) {


    sId += "";

    //Prend aussi le format id de la frame
    if (sId.indexOf("frm_") == 0 && sId.length > "frm_".length)
        sId = sId.substr("frm_".length);

    if (top.window['_md']) {
        for (var i in top.window['_md']) {

            var modal = top.window['_md'][i];

            if (modal.isModalDialog && modal.UID.toLowerCase() == sId.toLowerCase()) {
                return modal;
            }
        }
    }
    return null;
}



eTools.AlertNotImplementedFunction = function () {

    eAlert(3, "", top._res_8470); //Cette fonction n'a pas encore été développée

}




eTools.Sleep = function (milliseconds) {
    var start = new Date().getTime();
    for (var i = 0; i < 1e7; i++) {
        if ((new Date().getTime() - start) > milliseconds) {
            break;
        }
    }
}


eTools.GetFilesInfos = function (filesCollection) {
    var oReturn = new Array();
    for (var i = 0; i < filesCollection.length; i++) {
        oReturn.push({ filename: filesCollection[i].name, saveas: filesCollection[i].name });
    }

    return oReturn;
}

eTools.GetRenamedFilesList = function (oRes) {
    var sReturn = new Array();
    var xmlFiles = oRes.getElementsByTagName("file");
    for (var i = 0; i < xmlFiles.length; i++) {
        var filename = getAttributeValue(xmlFiles[i], "id");
        var sSuggestedName = getXmlTextNode(xmlFiles[i].getElementsByTagName("suggestedname")[0]);
        sReturn.push(filename + ":" + sSuggestedName);
    }
    return sReturn.join("|");
}


//retourne vrai si les 2 tableaux contiennent les mêmes valeurs
eTools.ArrayEqual = function (arr1, arr2) {

    // pour les vieux navigateurs... (IE8)
    if (!('every' in Array.prototype)) {
        Array.prototype.every = function (tester, that /*opt*/) {
            for (var i = 0, n = this.length; i < n; i++)
                if (i in this && !tester.call(that, this[i], i, this))
                    return false;
            return true;
        };
    }
    var bEqual = false;

    bEqual = (arr1.length == arr2.length) && arr1.every(function (element, index) {
        return element === arr2[index];
    });

    return bEqual;
}

eTools.FindClosestElementWithClass = function (el, className) {
    while (el.parentNode) {
        el = el.parentNode;
        if (el && el.classList && el.classList.contains(className))
            return el;
    }
    return null;
}

eTools.SetClassName = function (oNode, className) {
    if (oNode != null) {
        if (oNode.className != "") {
            if (oNode.className.indexOf(className) < 0)
                oNode.className += " " + className;
        }
        else
            oNode.className = className;
        return true;
    }
    return false;
}

eTools.RemoveClassName = function (oNode, className) {
    if (oNode != null) {
        if (oNode.classList) {
            oNode.classList.remove(className);
        }
        else {
            var reg = new RegExp('(\\s|^)' + className + '(\\s|$)');
            oNode.className.replace(reg, '');
        }
        return true;
    }
    return false;
}

eTools.indexOf = function (collection, item) {
    var i = -1;

    for (var j = 0; j < collection.length; j++) {
        if (collection[j] == item) {
            i = j;
            break;
        }
    }

    return i;
}


eTools.caller = function (args) {
    return args.callee.caller != null && args.callee.caller.name != "" ? args.callee.caller.name : arguments;
}



// les codes clavier
eTools.keyCode = {
    /// Retour arrière
    BACKSPACE: 8,
    /// Tabulation
    TAB: 9,
    /// Entrée
    ENTER: 13,
    /// Echape
    ESCAPE: 27,
    /// Page haut
    PAGE_UP: 33,
    /// Page bas
    PAGE_DOWN: 34,
    /// Flèche haut
    UP_ARROW: 38,
    /// Flèche bas
    DOWN_ARROW: 40,
    /// Suppression
    DELETE: 46
    // Ajouter des entrées si nécessaire
}


//retourne la valeur sélectionée d'un select 
eTools.getSelectedValueFromID = function (sId) {

    return eTools.getSelectedValue(document.getElementById(sId));
};


//retourne la valeur sélectionée d'un select 
eTools.getSelectedValue = function (obj) {

    var item = eTools.getSelectedItem(obj);
    if (!item)
        return "";

    return item.value;
};

//retourne l'item sélectioné d'un select 
eTools.getSelectedItem = function (obj) {
    if (!obj)
        return null;

    if (obj.tagName != "SELECT")
        return null;

    if (obj.selectedIndex == -1)
        return null;

    return obj.options[obj.selectedIndex];
};

//désactive les item correspondant à une valeur
eTools.disableItemValue = function (obj, value) {
    if (!obj)
        return null;

    if (obj.tagName != "SELECT")
        return null;

    for (var i = 0; i < obj.options.length; i++) {
        if (obj.options[i].value == value)
            obj.options[i].disabled = true;
    }

};


// Génére une chaine de longueur specifié pour utilisation dans les ids
eTools.generateClientId = function () {

    var CLIENT_ID_LENGTH = 7;

    var clientId = "";
    var alphaNumeric = "abcdefghijklmnopqrstuvwxyz0123456789";
    for (var i = 0; i < CLIENT_ID_LENGTH; i++)
        // on prends une position alétoire dans l'alphaNumeric
        clientId += alphaNumeric.charAt(Math.floor(Math.random() * alphaNumeric.length));

    return clientId;
}

// Affiche ou masque un élément HTML
eTools.toggleElement = function (id) {
    var element = document.getElementById(id);
    if (element) {
        if (element.style.display == "none") {
            element.style.display = "block";
        }
        else {
            element.style.display = "none";
        }
    }
}


eTools.switchOfBetaTheme = function () {


    var oModalMSG = new eModalDialog(top._res_2422, 10, "eBlank.aspx", 900, 350); // hauteur avant à 475px
    oModalMSG.textClass = "confirm-msg";
    oModalMSG.hideCloseButton = false;
    oModalMSG.hideMaximizeButton = true;


    oModalMSG.revert = true;

    oModalMSG.onHideFunction = function () {
        if (oModalMSG.revert) {
            var chk = document.getElementById("chckNwThm");
            chk.checked = true;
            //SHA : backlog #1 647
            var spnNwItm = document.querySelector("#chckNwThm ~ span");
            var imgNwItm = document.querySelector("#chckNwThm ~ img");
            //imgNwItm.innerHTML = top._res_2378;
            imgNwItm.setAttribute("title", top._res_2378);
            imgNwItm.setAttribute("alt", top._res_2378);
            imgNwItm.style.visibility = 'visible';
            imgNwItm.style.display = 'block';

            spnNwItm.style.visibility = 'hidden';
            spnNwItm.style.display = 'none';
        }
    }

    var divContent = document.createElement("div");
    divContent.id = "betaswitchoff";

    divContent.className = "comms-new-theme-container";
    var title = document.createElement("h2");
    divContent.appendChild(title);

    title.innerHTML =
        top._res_2388; //+ "<br/>" +   //Message  prevenant qu'il va retourner sur le thème blanc/rouge [ATTENTE MSG ALEB]
    //top._res_2389 + "<br/>" +   //Message remerciant d'avoir essayer le nouveau thème [ATTENTE MESSAGE ALEB]
    //top._res_2381;              //"Avant de repasser sur un thème classique, laissez-nous votre impression du nouveau thème"

    // rajout d'une balise p pour le contenu texte suivant

    var desc = document.createElement("p");
    divContent.appendChild(desc);

    desc.innerHTML = top._res_2389;
    /* Message remerciant d'avoir essayer le nouveau thème [ATTENTE MESSAGE ALEB] + 
    "Avant de repasser sur un thème classique, laissez-nous votre impression du nouveau thème" */


    var comment = document.createElement("textarea");
    setAttributeValue(comment, "rows", "10");
    setAttributeValue(comment, "cols", "25");
    setAttributeValue(comment, "placeholder", top._res_2382);
    //setAttributeValue(comment, "style", "margin: 0px; width: 848px; height: 315px;"); // Rajouté dans le CSS
    setAttributeValue(comment, "autofocus", "");
    comment.id = "betathemecomment";
    divContent.appendChild(comment);



    oModalMSG.setElement(divContent);
    oModalMSG.show();
    oModalMSG.adjustModalToContent();

    oModalMSG.addButtonFct(top._res_2383, function () { eTools.switchOfBetaThemeProcess(oModalMSG, true); }, 'button-green', null, "");   // Envoyer commentaire
    oModalMSG.addButtonFct(top._res_2384, function () { eTools.switchOfBetaThemeProcess(oModalMSG, false); }, 'button-gray', null, "");   // Plus tard
}

/* Fonction de callback appelée au changement de thème nouveau <=> ancien, effectuant un reload sous conditions */
eTools.switchBetaThemeCallback = function (nNewThemeId, switchingOn) {
    var isThemePickerDisplayed = document.querySelector("div#colorPick.colorPick") != null;
    var isFontSizePickerDisplayed = document.querySelector("select#ftsize.ftsize") != null;
    var currentFont = top.eTools.GetFontSize();
    var themeMaxFont = top.eTools.GetMaxFontSize(nNewThemeId);

    // On doit recharger complètement la page si :
    // - les pictogrammes de sélection de thème sont affichés (Mon Eudonet)
    // - le sélecteur de taille de police est affiché (Mon Eudonet)
    // - le nouveau thème choisi impose une taille de police maximale ET que celle actuellement choisie par l'utilisateur est plus importante
    var needsReload = isThemePickerDisplayed || isFontSizePickerDisplayed || (themeMaxFont > 0 && themeMaxFont > currentFont);
    if (!needsReload) {
        applyThemeWithoutReload();
    }
    else {
        applyThemeDefaultCallback();
    }
};

eTools.switchOnBetaTheme = function () {


    var oModalMSG = new eModalDialog(top._res_2392, 0, "blank", 900, 425, null, true);
    oModalMSG.hideCloseButton = true;
    // oModalMSG.hideMaximizeButton = true;

    oModalMSG.show();
    oModalMSG.getIframeTag().src = top._res_2435;

    oModalMSG.addButtonFct(top._res_2385, function () {
        var nNewThemeId = THEMES.ROUGE2019.Id;
        oModalMSG.hide();
        addCss("../../Theme2019/css/theme", "THEMEBASE");
        applyTheme(nNewThemeId, nGlobalCurrentUserid, function () { eTools.switchBetaThemeCallback(nNewThemeId, true); });
    }, 'button-green', null, "");   //Commenter
}

eTools.switchOfBetaThemeProcess = function (oModal, bSendComment) {

    oModal.revert = false;
    var mainDiv = oModal.getDivContainer();

    var txt = mainDiv.querySelector("#betathemecomment").value;

    var fct = function () {
        clearHeader("THEMEBASE", "CSS");
        var nNewThemeId = THEMES.BLANCROUGE.Id;
        applyTheme(nNewThemeId, nGlobalCurrentUserid, function () { eTools.switchBetaThemeCallback(nNewThemeId, false); });
        oModal.hide();
    }

    if (bSendComment) {
        if (txt.trim() == "") {
            eAlert(0, top._res_416, top._res_2363.replace("{RUB}", top._res_1501));
            return
        }

        top.eTools.sendFeedBack(1, txt, fct);
    }
    else {
        fct();
    }

}


eTools.sendFeedBack = function (type, content, callback) {



    if ((content + "").trim == "")
        return;

    top.setWait(true)
    var upd = new eUpdater("mgr/eSendFeedback.ashx", 1);
    // upd.addParam("type", 1, "POST")
    // upd.addParam("content", content, "POST")
    var json = {
        type: type,
        content: content
    }


    var fctCallBack = function (sRes) {

        top.setWait(false)

        if (typeof (callback) == "function") {
            callback();
        }

        try {
            var res = JSON.parse(sRes);
            if (!res.Success) {
                eAlert(2, res.ErrorTitle, res.ErrorMsg, res.ErrorDetailMsg);
            }
        }
        catch (e) {

        }
    }

    upd.json = JSON.stringify(json);
    upd.ErrorCallBack = fctCallBack;

    upd.send(fctCallBack);
}

// Renvoie un tableau des éléments HTML/DOM situés sous le curseur de la souris, en tenant compte des différents calques
// Il faut passer, en paramètre de la fonction, l'objet évènement de type MouseEvent déclenchant l'action (onMouseDown, onClick..., généralement "event")
// Compatibilité optimale : Chrome 43+, Firefox 46+
eTools.getElementsUnderMouseCursor = function (e) {
    try {
        // La fonction elementsFromPoint de document permet de retourner tous les éléments situés sous le curseur de
        // la souris, quel que soit leur z-index, contrairement à querySelectorAll qui renvoie uniquement les éléments
        // situés les plus au-dessus sur certains navigateurs. Mais son support peut varier d'un navigateur à l'autre :
        // http://stackoverflow.com/questions/8813051/determine-which-element-the-mouse-pointer-is-on-top-of-in-javascript
        // On utilisera donc la première méthode si elle est implémentée sur le navigateur (ex : Chrome, Firefox),
        // et la seconde dans le cas contraire (ex : IE 11, Edge)
        var hoveredElements = new Array();
        if (typeof (document.elementsFromPoint) == "function") {
            // Pour déterminer la position du curseur : e.x/y sont théoriquement des alias de e.clientX/Y 
            // Mais les 2 variables peuvent parfois retourner des données différentes
            // https://developer.mozilla.org/en-US/docs/Web/API/MouseEvent/x
            // On utilisera donc plutôt clientX/Y en premier lieu
            var mousePositionX = e.clientX ? e.clientX : e.x;
            var mousePositionY = e.clientY ? e.clientY : e.y;
            hoveredElements = document.elementsFromPoint(mousePositionX, mousePositionY);
        }
        else
            hoveredElements = document.querySelectorAll(":hover");
    }
    catch (ex) {
        hoveredElements = new Array();
    }

    return hoveredElements;
}

/// Indique si un élément du DOM subit un overflow (= s'il déborde de son conteneur) en largeur OU en hauteur, en tenant compte d'une marge d'erreur an pixels si spécifiée en paramètre
eTools.isOverflowed = function (element, margin) {
    return eTools.isOverflowedWidth(element, margin) || eTools.isOverflowedHeight(element, margin);
}

/// Indique si un élément du DOM subit un overflow (= s'il déborde de son conteneur) en largeur, en tenant compte d'une marge d'erreur an pixels si spécifiée en paramètre
eTools.isOverflowedWidth = function (element, margin) {
    if (!margin)
        margin = 0;

    if (element && element.scrollWidth && element.clientWidth)
        return Math.abs(element.scrollWidth - element.clientWidth) > Math.abs(margin);
    else
        return null; // TOCHECK: éventuellement retourner false ?
}

/// Indique si un élément du DOM subit un overflow (= s'il déborde de son conteneur) en hauteur, en tenant compte d'une marge d'erreur an pixels si spécifiée en paramètre
eTools.isOverflowedHeight = function (element, margin) {
    if (!margin)
        margin = 0;

    if (element && element.scrollHeight && element.clientHeight)
        return Math.abs(element.scrollHeight - element.clientHeight) > Math.abs(margin);
    else
        return null; // TOCHECK: éventuellement retourner false ?
}




//Retourne le ModalDialog d'un fieldeditor
eTools.getModalFromField = function (fldEditor) {

    if (fldEditor.headerElement && fldEditor.headerElement.ownerDocument) {

        var winName = fldEditor.headerElement.ownerDocument.defaultView.name;
        var myTab = fldEditor.tab;

        return eTools.getModalFromWindowName(winName, myTab);
    }

    return null;
}

eTools.getModalFromWindowName = function (windowName, tab) {

    if (top["_mdName"] && top["_mdName"][windowName]) {
        var myMod = top["_mdName"][windowName];

        if (myMod.isModalDialog && myMod.tab == tab)
            return myMod;
    }

    return null;
}

eTools.escapeJS = function (jsValue) {
    return jsValue.replace(/\\n/g, "\\n")
        .replace(/\\'/g, "\\'")
        .replace(/\\"/g, '\\"')
        .replace(/\\&/g, "\\&")
        .replace(/\\r/g, "\\r")
        .replace(/\\t/g, "\\t")
        .replace(/\\b/g, "\\b")
        .replace(/\\f/g, "\\f");
}

// Affichage d'un waiter dans le widget
eTools.setWidgetWait = function (wid, bSet) {

    var widgetContent = top.document.querySelector("#widget-wrapper-" + wid + " .xrm-widget-content");
    if (widgetContent) {
        if (bSet) {

            if (!document.getElementById("widgetWaiter")) {
                var waiter = top.document.createElement("div");
                waiter.id = "widgetWaiter";

                var img = top.document.createElement("img");
                setAttributeValue(img, "alt", "wait");
                setAttributeValue(img, "src", "themes/default/images/wait.gif");
                waiter.appendChild(img);

                widgetContent.appendChild(waiter);
            }


        }
        else {
            var waiter = widgetContent.querySelector("#widgetWaiter");
            if (waiter) {
                waiter.parentElement.removeChild(waiter);
            }
        }
    }
}

/// dédoublone un tableau d'objet suivant une proprité de ces objetq
eTools.GetUnique = function (arr, prop) {

    if (Array.isArray(arr) && arr.length > 0 && arr[0].hasOwnProperty(prop)) {

        //Retourne un tableau des clés des objets 
        var mapKey = arr.map(function (obj) { return obj[prop] });

        //Filtre le tableau : retourne 
        var filteredArr = arr.filter(function (obj, idx) {

            return mapKey.indexOf(obj[prop]) == idx;
        });

        return filteredArr;
    }

    return null;
}

// Recherche d'un object dans une array à partir d'une clé/valeur
function findObjectByKey(array, key, value) {
    for (var i = 0; i < array.length; i++) {
        if (array[i][key] === value) {
            return array[i];
        }
    }
    return null;
}

// Copie le contenu d'un champ de saisie (input) dans le presse-papiers
function copyControlTextToClipboard(inputControl, sourceDocument) {
    if (!inputControl)
        return;

    if (!sourceDocument)
        sourceDocument = document;

    inputControl.select();
    sourceDocument.execCommand("Copy");
}

// L'onglet est-il affiché dans la liste des onglets ?
function isTabDisplayed(nTab) {
    if (nTab > 0) {
        var oeParam = top.document.getElementById('eParam').contentWindow;

        // Liste des tables affichée
        if (typeof (oeParam) == "undefined" || !oeParam || typeof (oeParam.GetParam) != "function" || !oeParam.GetParam("TabOrder") != "")
            return false;

        var sTabOrder = oeParam.GetParam("TabOrder");
        if ((";" + sTabOrder + ";").indexOf(";" + nTab + ";") < 0) {
            return false;
        }
        return true;
    }
    return false;
}

///summary
///L'élément indiqué peut-il recevoir un focus ?
///https://stackoverflow.com/questions/1599660/which-html-elements-can-receive-focus/1600194#1600194
///summary
function isFocusableElement(domElement) {
    var isFocusableElement = false;
    if (domElement) {
        if (
            domElement.tagName.toLowerCase() == 'a' ||
            domElement.tagName.toLowerCase() == 'iframe' ||
            (domElement.tagName.toLowerCase() == 'input' && domElement.attributes["disabled"] != "disabled") ||
            !isNaN(Number(domElement.attributes["tabindex"]))
        )
            isFocusableElement = true;
    }

    return isFocusableElement;
}

///summary
///Renvoie une fonction permettant de comparer 2 éléments, en fonction de la nature de la donnée à comparer (passée en paramètre)
///summary
function getContentsComparerFunction(sampleContentsToCompare) {
    var doNumericComparison = isNumeric(sampleContentsToCompare.replace(/ /g, ''));
    if (doNumericComparison)
        return function (a, b) { return parseInt(a.replace(/ /g, ''), 10) - parseInt(b.replace(/ /g, ''), 10); }
    else
        return function (a, b) { return a.localeCompare(b); }
}


var Cartography = {};
Cartography.switchActive = function (source) {
    if (!source || !source.parentElement)
        return;

    var parent = source.parentElement;

    var oldValue = getAttributeValue(parent, 'data-active');
    var newValue = oldValue == "0" ? "1" : "0";

    setAttributeValue(parent, 'data-active', newValue);
};

// Si la res n'existe pas, on affiche ##[INVALID_RES_XXXX]##
// au lieu d'une erreur JS
eTools.getRes = function (resId) {
    if (top && top.hasOwnProperty("_res_" + resId))
        return top["_res_" + resId];
    else
        return "##[INVALID_RES_" + resId + "]##";
}

// Renvoie un document type DOM à partir d'une chaîne de caractères représentant du code HTML
// Permet d'effectuer des manipulations DOM (getElementById, querySelector*) sur le code HTML représenté par une chaîne en vue de chercher/remplacer/insérer des éléments sans faire de replace()
// Compatibilité : IE 10 et >
// https://stackoverflow.com/questions/30040319/how-to-execute-document-queryselectorall-on-a-text-string-without-inserting-it-i
eTools.stringToHTMLDocument = function (htmlString) {
    var parser = new DOMParser();
    var doc = parser.parseFromString(htmlString, "text/html");
    return doc;
};


/// permet de logger les appels de fonction avec indentation en suivant lors position dans la call stack
// options: {  
//      fullstack : toutes la stack en cours
//      comments : commentaire a logger
//      asObj : retourne le résultat comme un obj dans le log, plutot qu'en string
//}

// ex : eTools.consoleLogFctCall({fullstack:true, comments:'test' })
eTools.consoleLogFctCall = function (options) {
    try {

        //assign plutôt que spread, on a un polyfill pour assign sur IE, pas pour spread
        options = Object.assign({}, { fullstack: false, comments: "", asObj: false }, options)

        var err = new Error()
        var st = err.stack;
        var aSt = st.split(/\n/)

        //log le commentaire
        if (options.comments != "")
            console.log("comment : ", options.comments)


        var idxSelf = Number.MAX_SAFE_INTEGER;

        //parcour de la pile d'appels
        var arrStack = aSt.reverse().map(function (elem, idx) {
            if (idx < idxSelf && elem != "") {
                try {
                    elem = elem.trim();
                    try {
                        //ff
                        var fctName = elem.split("@")[0];
                        var file = elem.split("@")[1].split("://")[1].split('?')[0].split(':')[0];
                    }
                    catch (ee) {
                        //chrome
                        var fctName = elem.trim().split(" ")[1];
                        try {
                            var file = elem.split(" ")[2].split("://")[1].split('?')[0].split(':')[0];
                        }
                        catch (zz) {
                            file = elem.split("://")[1].split('?')[0].split(':')[0]
                        }
                    }
                }
                catch (oo) {
                    file = elem
                }

                var o = { functionName: fctName + "", filename: file + "", From: { file: "", line: "" }, elem: elem }

                //ne retourne pas l'appel a lui meme
                if (o.functionName.indexOf("consoleLogFctCall") > - 1) {
                    idxSelf = idx
                    return null;
                }

                //retourne le fichier et la ligne appelante
                if (idx > 0) {
                    if (aSt[idx - 1] != "") {
                        try {

                            var fileFrom = aSt[idx - 1].split('://')[1].split('?')[0].split(':')[0];
                            var arr = aSt[idx - 1].split(':');
                            var lineFrom = aSt[idx - 1].split(':')[arr.length - 2];
                            o.From = { file: fileFrom, line: lineFrom }
                        }
                        catch (zz) {
                            o.From = { elem: aSt[idx - 1] }
                        }
                    }
                }
                return o;
            }
            return null
        }
        );

        arrStack.filter(function (elem) { return elem != null }).forEach(function (o, idx) {
            if (options.asObj)
                console.log("--> ".repeat(idx), (o));
            else
                console.log("--> ".repeat(idx), " fct : [" + o.functionName + "] - fichier : [" + o.filename + "]" + ((o.From.file != "") ? ("( depuis : " + o.From.file + " - ligne " + o.From.line + ")") : ""))
        }
        );
    }
    catch (e) {
        console.log(e)
    }


}



// Modifie le theme du document en paramètre si le lien Thème est présent
// Cela permet d'appeler cette fonction dans des iframes
eTools.changeDocumentTheme = function (oldThemeLink, newThemeLink, themeColor) {
    var oLink = document.head.querySelectorAll('link[eType="THEME"]')[0];
    if (oLink != null && oLink.hasAttributes("href") && oLink.href.indexOf(oldThemeLink) != -1) {
        oLink.href = oLink.href.replace(oldThemeLink, newThemeLink);
    }

    // Pour chaque iframe, on appelle la même fonction dans son contexte
    var iframes = document.querySelectorAll("iframe");
    if (iframes && iframes.length > 0) {
        Array.prototype.slice.call(iframes).forEach(function (iframe) {
            if (!iframe || iframe.id == "eParam" || !iframe.contentWindow) // pas la peine
                return;

            //#74 106
            //CNA: S'il y a des iframe pointant sur un autre domaine, une erreur de cross-origin frame sera levée
            //on catch ces erreurs pour éviter de bloquer le traitement
            try {
                if (iframe.contentWindow.eTools && typeof (iframe.contentWindow.eTools.changeDocumentTheme) == "function")
                    iframe.contentWindow.eTools.changeDocumentTheme(oldThemeLink, newThemeLink);

                // S'il y a previent l'iframe du changement du thème
                if (typeof (iframe.contentWindow.themeChanged) == "function")
                    iframe.contentWindow.themeChanged(oldThemeLink, newThemeLink, themeColor);
            }
            catch (err) {
                if (err.code == 18 || err.name == "SecurityError")
                    console.log(err);
                else
                    throw err;
            }
        });
    }
}

// Backlog #451 - Détecte s'il existe des caractères non latins (Unicode) dans une chaîne
// Source : https://stackoverflow.com/questions/147824/how-to-find-whether-a-particular-string-has-unicode-characters-esp-double-byte/1697749#1697749
eTools.containsNonLatinCodepoints = function (strData) {
    return /[^\u0000-\u00ff]/.test(strData);
}

// Backlog #451 - Supprime les caractères Unicode non visibles d'une chaîne
eTools.removeHiddenSpecialChars = function (strData, bForceReplace) {
    // Par défaut, le traitement n'est pas effectué si la chaîne ne comporte pas de caractères concernés, sauf si le paramètre bForceReplace est passé à true
    var bCanReplace = bForceReplace || eTools.containsNonLatinCodepoints(strData);

    if (bCanReplace) {
        //console.log("Des caractères Unicode ont été détectés dans la chaîne source ! Les caractères non visibles vont être remplacés.");

        // Backlog #451 - Suppression des caractères Zero-width Space (Unicode) et autres caractères spéciaux invisibles parfois insérés en JavaScript,
        // notamment avec des champs de fusion, donnant des ??? à l'interprétation
        // cf. correctif précédemment effectué sur eMemoEditor.getUserMessage pour la même raison : #31 571
        // Les insertions de ce genre de caractères sont souvent provoquées par les méthodes de manipulation de Sélections/Ranges en JavaScript
        // Source : https://stackoverflow.com/questions/11305797/remove-zero-width-space-characters-from-a-javascript-string
        strData = strData.replace(/\u200A/g, ""); //\u200A (8202 in hex) - HAIR SPACE SPACE - https://www.fileformat.info/info/unicode/char/200a/index.htm
        strData = strData.replace(/\u200b/g, ""); //\u200B (8203 in hex) - ZERO WIDTH SPACE - https://www.fileformat.info/info/unicode/char/200b/index.htm
        strData = strData.replace(/\u200B/g, ""); //\u200B (8203 in hex) - ZERO WIDTH SPACE - https://www.fileformat.info/info/unicode/char/200b/index.htm
        strData = strData.replace(/\u200C/g, ""); //\u200C (8204 in hex) - ZERO WIDTH NON-JOINER - https://www.fileformat.info/info/unicode/char/200c/index.htm
        strData = strData.replace(/\u200D/g, ""); //\u200D (8205 in hex) - ZERO WIDTH JOINER - https://www.fileformat.info/info/unicode/char/200d/index.htm
        strData = strData.replace(/\u200E/g, ""); //\u200E (8206 in hex) - LEFT-TO-RIGHT MARK - https://www.fileformat.info/info/unicode/char/200e/index.htm
        strData = strData.replace(/\u200F/g, ""); //\u200F (8207 in hex) - RIGHT-TO-LEFT MARK - https://www.fileformat.info/info/unicode/char/200f/index.htm
        strData = strData.replace(/\uFEFF/g, ""); //\uFEFF (65279 in hex) - ZERO WIDTH NO-BREAK SPACE - https://www.fileformat.info/info/unicode/char/feff/index.htm

        //console.log("Les caractères Unicode non visibles et connus ont été remplacés dans la chaîne source.");
    }

    return strData;
}

//cette fonction permet de position la position du curseur à un emplacement donné dans un content éditable
eTools.setCurrentCursorPosition = function (currentwindow, ctrl, chars) {
    if (chars >= 0) {
        if (!currentwindow)
            currentwindow = window;
        var selection = currentwindow.getSelection();

        range = eTools.createRange(currentwindow, ctrl, { count: chars });

        if (range) {
            range.collapse(false);
            selection.removeAllRanges();
            selection.addRange(range);
        }
    }
};

eTools.createRange = function (currentwindow, node, chars, range) {
    if (!range) {
        range = currentwindow.document.createRange()
        range.selectNode(node);
        range.setStart(node, 0);
    }

    if (chars.count === 0) {
        range.setEnd(node, chars.count);
    } else if (node && chars.count > 0) {
        if (node.nodeType === Node.TEXT_NODE) {
            if (node.textContent.length < chars.count) {
                chars.count -= node.textContent.length;
            } else {
                range.setEnd(node, chars.count);
                chars.count = 0;
            }
        } else {
            for (var lp = 0; lp < node.childNodes.length; lp++) {
                range = createRange(currentwindow, node.childNodes[lp], chars, range);

                if (chars.count === 0) {
                    break;
                }
            }
        }
    }

    return range;
};

eTools.addAttributesToMemoData = function (memoData) {
    var oExtendedLabelDoc = eTools.stringToHTMLDocument(memoData);
    var sources = oExtendedLabelDoc.querySelectorAll("label");
    for (var j = 0; j < sources.length; j++) {
        var source = sources[j].getAttribute("ednc");
        if (source && source === 'mergefield') {
            sources[j].setAttribute("contenteditable", "false");
            sources[j].setAttribute("data-gjs-type", "mergefield");
            sources[j].parentElement.setAttribute("data-gjs-type", "text");
        }
        else if (source && source.hasClass === 'eudonet-extended-label') {
            sources[j].setAttribute("contenteditable", "true");
            sources[j].parentElement.setAttribute("data-gjs-type", "label");
        }
    }
    memoData = oExtendedLabelDoc.body.innerHTML;
    return memoData;
};

//Cette méthode permet de récupérer le champ fusion (texte) à partir du descid
eTools.getTextMergeFieldFromDescId = function (mergedFields, descId) {
    for (var i in mergedFields) {
        var aField = mergedFields[i].split("\;");
        // le descid du champ
        var strFieldDescId = aField[0];
        if (strFieldDescId == descId)
            return i;
    }
    return '';
};

/* #81 722 - Sélection du texte d'un élément HTML
/* Attention : ne fonctionne que si la sélection de texte par l'utilisateur n'a pas été court-circuitée sur document.body, ex. en CSS via user-select: none
 */
eTools.selectText = function (node) {
    // Ciblage
    if (typeof (node) === "string")
        node = document.getElementById(node);

    if (!node)
        return;

    // Sélection
    var range = null;
    if (document.body.createTextRange) {
        range = document.body.createTextRange();
        range.moveToElementText(node);
        range.select();
    } else if (window.getSelection) {
        var selection = window.getSelection();
        range = document.createRange();
        range.selectNodeContents(node);
        selection.removeAllRanges();
        selection.addRange(range);
    }
}

eTools.FieldFormat = {
    TYP_HIDDEN: 0,
    TYP_CHAR: 1,
    TYP_DATE: 2,
    TYP_BIT: 3,
    TYP_AUTOINC: 4,
    TYP_MONEY: 5,
    TYP_EMAIL: 6,
    TYP_WEB: 7,
    TYP_USER: 8,
    TYP_MEMO: 9,
    TYP_NUMERIC: 10,
    TYP_FILE: 11,
    TYP_PHONE: 12,
    TYP_IMAGE: 13,
    TYP_GROUP: 14,
    TYP_TITLE: 15,
    TYP_IFRAME: 16,
    TYP_CHART: 17,
    TYP_COUNT: 18,
    TYP_RULE: 19,
    TYP_ID: 20,
    TYP_BINARY: 21,
    TYP_GEOGRAPHY_OLD: 24,
    TYP_BITBUTTON: 25,
    TYP_ALIAS: 26,
    TYP_SOCIALNETWORK: 27,
    TYP_GEOGRAPHY_V2: 28,
    TYP_ALIASRELATION: 29,
    TYP_PJ: 30,
    TYP_DESC: 31,
    TYP_PASSWORD: 32
}

eTools.DescIdEudoModel = {
    TBL_PP: 200,
    TBL_PM: 300,
    TBL_Adress: 400,
}

eTools.DescIdEudoPlanningField = {
    Alerts_Sound: 76,
    Alerts_Hour: 77,
    Alerts: 79,
    Calendar_Color: 80,
    Periodicity: 82,
    Type: 83,
    Informatios: 93,
    Notes: 94,
    CreatedOn: 95,
    ModifiedOn: 96
}

//Codes erreurs sur le contrôle du formulaire
eTools.FormularValidationErrorCode = {
    InputTextError: 2601,
    InputMailError: 2365,
    InputPhoneError: 5138,
    InputNumError: 236,
    InputCheckboxError: 2204,
    URLRedirectionError: 2649,
    MessageRedirectionError: 2648,
    InputDateError: 231,
    InputMemoError: 2688,
    CatalogError: 2806,
    MultipleChoiceError: 247,
    WorldLinePaymentBtn: 8771
}

//Permet de gérer les erreurs liées au contrôle du formulaire 
var eErrorValidation = function () {
    this.errorList = {};
    this.isValid = true;

    this.InitNewError = function (codeRes) {
        this.errorList[codeRes] = [];
        if (this.isValid)
            this.isValid = false;
    }

    this.ErrorIsAlreadyAdded = function (codeRes) {
        this.errorList.hasOwnProperty(codeRes)
    }

    this.addNewError = function (codeRes, resError) {
        this.InitNewError(codeRes);
        this.errorList[codeRes].push(resError);
    }

    this.toString = function () {
        var errorMessage = "<ul>";
        for (var e in this.errorList) {
            if (this.errorList[e].length && this.errorList[e].length > 0) {
                errorMessage += "<li>";
                errorMessage += this.errorList[e];
                errorMessage += "</li>";
            }
        }
        errorMessage += "</ul>";
        return errorMessage;
    }
};

/**
 * Renvoie l'élément conteneur du signet dont le TabID est passé en paramètre - #84 907
 * @param {any} TabID (nTab) du signet à cibler
 * @param {any} Objet Document sur lequel cibler la recherche. Si null ou undefined, la vérification portera sur window.document
 */
eTools.getBookmarkContainer = function (nTab, oDoc) {
    if (!oDoc)
        oDoc = document;

    if (oDoc) {
        /* E17 : "bkm_200" */
        var elt = oDoc.getElementById("bkm_" + nTab);
        if (!(elt == null || typeof (elt) == 'undefined'))
            return elt;

        /* IRIS/Nouveau mode Fiche/Eudonet x : débute par id_xxx : "id_200_Contacts" */
        elt = oDoc.querySelector("[id^='id_" + nTab + "_']");
        if (elt)
            return elt;
    }

    return null;
};

/**
* Indique si l'élément est visible dans la surface affichée du navigateur (viewport)
* @source https://www.javascripttutorial.net/dom/css/check-if-an-element-is-visible-in-the-viewport/ et grapesjs
* @param  {HTMLElement} Element à tester
* @param  {Boolean} true si on doit également tester la taille réelle de l'élément (si renvoyée par le navigateur)
* @return {Boolean} true si l'élément est dans le viewport, false sinon
*/
eTools.isInViewport = function (oElement, bCheckSize) {
    var rect = oElement.getBoundingClientRect();
    bCheckSize = bCheckSize && typeof (rect.height) != "undefined" && typeof (rect.width) != "undefined";
    return (
        rect.top >= 0 &&
        rect.left >= 0 &&
        rect.bottom <= (window.innerHeight || document.documentElement.clientHeight) &&
        rect.right <= (window.innerWidth || document.documentElement.clientWidth) &&
        (bCheckSize ? rect.height > 0 && rect.width > 0 : true)
    );
};

/**
 * Indique si l'élément est un objet
 * @param {any} obj
 */
eTools.isObject = function (obj) {
    return obj !== undefined && obj !== null && obj.constructor == Object;
}

// Merci d'utiliser le namespace eTools comme l'exemple ci-dessus
// Merci de garder ce commentaire en bas de cette page
// eTools = {} est déjà défini au début de la page.

