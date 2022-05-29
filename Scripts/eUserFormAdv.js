//*****************************************************************************************************//
//*****************************************************************************************************//
//*** eTools.js
//*****************************************************************************************************//
//*****************************************************************************************************//

var waiterTime;
var nbCompt = 0;
var debugTablet = null;

var rspace = /\s+/;
var rclass = /[\n\t\r]/g;

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
})()

function timeStamp() {

    var date = new Date();

    return date.getTime();
}

function randomID(size) {
    var str = "";
    for (var i = 0; i < size; i++) {
        str += getRandomChar();
    }
    return str;
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

function getRandomChar() {
    var chars = "0123456789abcdefghijklmnopqurstuvwxyzABCDEFGHIJKLMNOPQURSTUVWXYZ";
    return chars.substr(getRandomNumber(62), 1);
}

function getRandomNumber(range) {
    return Math.floor(Math.random() * range);
}

var eDate = (function () {
    /*Début Variables Privées***********************************************/
    var that = this;

    var _isInit = false;    //Paramètres de base déjà initialisés
    var _isValid;

    var _cultureInfoDate = '';  //
    /*Fin Variables Privées*************************************************/

    /*Début Constantes privée***********************************************/
    var FormatLittleEndian = "dd/MM/yyyy";  //format en bdd
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

// Indique si un neoud dans la src exist 
function nodeExist(srcDocument, id) {
    var node = srcDocument.getElementById(id);
    return node != null && typeof (node) != 'undefined';
}

function getTabDescid(descid) {
    return descid - descid % 100;
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

// Retourne l'objet document via l'Iframe des params
function getParamWindow() {
    var eParamWindow = document.getElementById('eParam');

    if (eParamWindow) {
        if (eParamWindow.contentWindow && typeof (eParamWindow.contentWindow.GetParam) == "function") {
            eParamWindow = eParamWindow.contentWindow;
        }
    }

    return eParamWindow;
}

function isInt(n) {
    if (n == null || typeof (n) == "undefined" || (n + '') == '')
        return false;
    return !isNaN(parseInt(n)) && isFinite(n);
}

// Teste si l'élément possède la classe en paramètre
function hasClass(element, className) {
    if (element != null)
        return (' ' + element.className + ' ').indexOf(' ' + className + ' ') > -1;
    return false;
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
// Set la valeur de l'attribute.  
function setAttributeValue(oNode, attributeName, value) {

    if (oNode != null && oNode.setAttribute)
        return oNode.setAttribute(attributeName, value);

    return false;

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
            var strPattern = "^(\\+|\\-|\\_|\\.|\\ |[0-9])*$";
            //regex à améliorer : 
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

//Indique si le format en paramètre est de type numérique (AUTOINC, COUNT, ID, NUMERIC ou MONEY)
function isFormatNumeric(sFormat) {
    return (sFormat == FLDTYP.AUTOINC
        || sFormat == FLDTYP.COUNT
        || sFormat == FLDTYP.ID
        || sFormat == FLDTYP.NUMERIC
        || sFormat == FLDTYP.MONEY);
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

/*addOrDelAttribute : 
// oTarg : objet cible
// sAttribute : attribut cible 
// name : valeur à ajouter dans l'attribut
// bAdd : Ajout/Suppression de la valeur dans l'attribut
// bNoMerging : Si vrai on peux ajouter plusieurs fois la même valeur
*/
function addOrDelAttributeValue(oTarg, sAttribute, name, bAdd, bNoMerging) {
    if (name && name != "") {   //Que si une valeur est demandée
        var sCaller = oTarg.attributes[sAttribute];
        var nPos = (";" + sCaller + ";").indexOf(";" + name + ";");
        if ((bAdd) && (nPos < 0 || bNoMerging)) {   //Ajout de la valeur pour cet attribut : si pas de fusion on vérifit qu'il n'est pas déjà présent
            if (sCaller != "")
                sCaller = sCaller + ";";
            sCaller = sCaller + name;
            oTarg.attributes[sAttribute] = sCaller;
        }
        else if (!bAdd && (nPos > 0)) {   //Suppression d'une des valeurs de l'attribut si présent
            if (sCaller != "") {
                sCaller = (";" + sCaller + ";").replace(";" + name + ";", ";;");
                //Suppression des ; rajouté autour
                sCaller = sCaller.substring(0, 1);
                sCaller = sCaller.substring(sCaller.length - 2, sCaller.length - 1);
                oTarg.attributes[sAttribute] = sCaller;
            }
        }
    }
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

function setWait(bOn, name, upd, fromIris) {
    if (window.DeviceOrientationEvent) {
        window.preventTabletMoveFunctions = bOn;
    }

    var oWaiter = document.getElementById("waiter");
    var maxZIndex = 100;	//zIndex min à 100 pour iso à eModal
    var sClassFinalizer = (fromIris) ? "FrmIris" : "";

    if (oWaiter) {
        addOrDelAttributeValue(oWaiter, "caller", name, bOn, true);

        /** On recherche la classe à remplacer, à l'aide d'un moyen
         * particulièrement ingénieux et novateur, ... une boucle for,
         * et un startwith... */
        var waitClass = oWaiter.className;
        var classToReplace = "";

        if (waitClass.length > 0) {
            var tabClass = waitClass.split(" ")
            for (var cls in tabClass) {
                if (tabClass[cls].toString().toLowerCase().indexOf("waitoff") > -1
                    || tabClass[cls].toString().toLowerCase().indexOf("waiton") > -1) {
                    classToReplace = tabClass[cls].toString();
                }
            }
        }

        if (bOn) {

            clearTimeout(waiterTime);

            if (!isInt(nbCompt) || nbCompt < 0)
                nbCompt = 1;
            else
                nbCompt++;

            switchClass(oWaiter, classToReplace, "waitOn" + sClassFinalizer);
            document.getElementById("contentWait").style.color = (sClassFinalizer != '') ? '#bb1515' : '#ffffff';
            document.getElementById("contentWait").style.display = "block";

            var zIndex = GetMaxZIndex(document, maxZIndex) + 1;
            oWaiter.style.zIndex = zIndex;

            //closeRightMenu();


        }
        else {
            nbCompt--;

            //Ne masque le wait que si tout est loadé
            if (nbCompt <= 0) {
                nbCompt = 0;
                waiterTime = setTimeout(function () {

                    switchClass(oWaiter, classToReplace, "waitOff" + sClassFinalizer);
                    if (document.getElementById("contentWait"))
                        document.getElementById("contentWait").style.display = "none";
                }, 1);

                if (isTablet()) {
                    var moActiveMenu = document.querySelector("ul.sbMenuActive");
                    switchClass(moActiveMenu, "sbMenuActive", "sbMenu");
                    addClass(moActiveMenu, "ul-tab-hidden");
                    var moTabletActiveTabTitle = document.querySelector("div.navTitleTabletFocused");
                    removeClass(moTabletActiveTabTitle, "navTitleTabletFocused");
                }
            }

        }
    }
}

// #58 123 : Calcul de la taille totale du document, scroll compris
// Permet d'afficher le calque sur toute la surface et non pas seulement sur la surface visible avant scroll
// Source : https://stackoverflow.com/questions/1145850/how-to-get-height-of-entire-document-with-javascript
// Auteur : Andrii Verbytskyi - https://stackoverflow.com/users/2768917/andrii-verbytskyi
function getMaxDocWidth(currentWidth, nodesList) {
    if (!currentWidth)
        currentWidth = 0;

    if (!nodesList)
        nodesList = document.documentElement.childNodes;

    for (var i = nodesList.length - 1; i >= 0; i--) {
        if (nodesList[i].scrollWidth && nodesList[i].clientWidth) {
            var elWidth = Math.max(nodesList[i].scrollWidth, nodesList[i].clientWidth);
            currentWidth = Math.max(elWidth, currentWidth);
        }
        if (nodesList[i].childNodes.length)
            currentWidth = getMaxDocWidth(currentWidth, nodesList[i].childNodes);
    }

    return currentWidth;
}

function getMaxDocHeight(currentHeight, nodesList) {

    if (!currentHeight)
        currentHeight = 0;

    if (!nodesList)
        nodesList = document.documentElement.childNodes;

    for (var i = nodesList.length - 1; i >= 0; i--) {

        if (nodesList[i].scrollHeight && nodesList[i].clientHeight) {
            var elHeight = Math.max(nodesList[i].scrollHeight, nodesList[i].clientHeight);
            currentHeight = Math.max(elHeight, currentHeight);
        }
        if (nodesList[i].childNodes.length) {
            currentHeight = getMaxDocHeight(currentHeight, nodesList[i].childNodes);
        }
    }

    return currentHeight;
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


var ModalEudoType =
{
    WIZARD: 0,

    LIST: 1,
    CHART: 2,
    SELECTFIELD: 3,
    SELECTTAB: 4,

    UNSPECIFIED: 99
};
//Drag&Drop fenêtres
var modalId = "";
var dragEnabled = false;
var resizeEnabled = false;
var dashedDiv;
var bgDashedDiv;
var divCtnr;
var divCtnrInitOpacity = null;
var divMove;

//position de la souris pour mémorisation
var xMouseMem = 0;
var yMouseMem = 0;

var winWidth = 0;
var winHeight = 0;
var aWH = [200, 200];
if (typeof (getWindowWH) == "function") {
    aWH = getWindowWH();
}

winWidth = aWH[0];
winHeight = aWH[1];

var MsgType =
{
    MSG_CRITICAL: 0,
    MSG_QUESTION: 1,
    MSG_EXCLAM: 2,
    MSG_INFOS: 3,
    MSG_SUCCESS: 4
};
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

    oModAlert = new eModalDialog(title, 1, '', width, height, null, bDontIgnoreSetWait);
    oModAlert.hideMaximizeButton = true;

    oModAlert.setMessage(message, details, criticity, bHtmlContent);

    oModAlert.show();
    oModAlert.addButtonFct(_res_30 , function () { okFct(); oModAlert.hide(); }, "button-green", "cancel");

    return oModAlert;
}

function eModalDialog(title, type, url, width, height, handle, bDontIgnoreSetWait, bIsExternal, bIsAvancedFormular) {

    this.debugMode = false; // à mettre à true pour afficher certains messages de diagnostic - NE PAS UTILISER EN PRODUCTION !

    // Génère des messages de diagnostic ; uniquement si this.debugMode = true
    this.trace = function (strMessage) {
        if (this.debugMode) {
            try {
                strMessage = 'eModalDialog [' + this.UID + '] -- ' + strMessage;

                if (typeof (console) != "undefined" && console && typeof (console.log) != "undefined") {
                    console.log(strMessage);
                }
                else {
                    alert(strMessage); // TODO: adopter une solution plus discrète que alert()
                }
            }
            catch (ex) {

            }
        }
    };

    this.onHideFunction = null; //fonction exécutée après fermeture de la modaldialog

    //Ajout de le menu contextuel dans le tableau global des modal dialog (_md)
    if (typeof handle != "undefined" && handle != null) {
        if (top) {
            top.window['_md'][handle] = this;
        }
    }

    var that = this; // pointeur vers l'objet eModalDialog lui-même, à utiliser à la place de this dans les évènements onclick (ou this correspond alors à l'objet cliqué)

    this.myOpenerDoc = document;    // Mémorisation du doc de l'ouvrant
    this.myOpenerWin = window;    // Mémorisation du doc de l'ouvrant
    this.openMaximized = false;

    //Type du message
    var ModalType =
    {
        ModalPage: 0,   //Page en popup
        MsgBoxLocal: 1, //Messagebox avec un simple message à afficher
        Prompt: 2,      //Fenêtre pour introduire une valeur texte
        Waiting: 3,       //Attente - TODO
        ProgressBar: 4,   //Barre de progression - TODO
        ToolTip: 5,  //Fenetre ToolTip simplifié (mini fenetre de résumer pour les planning)
        ToolTipSync: 6,  //Fenetre ToolTip simplifié (mini fenetre de résumer pour les planning)
        VCard: 7,   //Fenêtrfe vcard
        ColorPicker: 8, //Selecteur de couleurs
        SelectList: 9, //Liste de sélection (boutons radio) - A Supprimmer, maintenant les boutons sont ajoutable tous le temps.
        DisplayContent: 10, //affiche un élément du DOM
        DisplayContentWithoutTitle: 11 //affiche un élément du DOM sans le titre
    };

    ///Contenu Eudo de la modale (Wizard, Liste, sélection de champ...)
    var oldMouseMove;
    var oldMouseUp;
    var CONST_TOOLTIP_ARROW_WIDTH = 12;

    this.marginDialogMaxSize = 10;

    //Attribut permettant d'identifer un objet comme étant une fenêtre modal
    this.isModalDialog = true;

    //Url de la page à appeler   
    this.url = url;

    this.bIsAvancedFormular = bIsAvancedFormular;

    this.type = type; // type "générique" de fenêtre modale. cf enum ModalType

    this.isSpecif = false;

    this.EudoType = ModalEudoType.UNSPECIFIED.toString();
    this.bBtnAdvanced = false;    //Indique si la popup est avancée est a un espace plus grand à droite au niveau des boutons
    this.Handle = handle;

    this.tabScript = new Array();
    this.tabCss = new Array();

    this.Buttons = new Array();

    this.NoScrollOnMainDiv = false;

    // Sur tous les supports sauf les tablettes, on masque la fenêtre en détruisant l'objet eModalDialog du DOM
    // pour libérer la mémoire
    // On appelle isTablet depuis plusieurs contextes car eTools n'est pas forcément présent sur toutes les modal dialogs (notamment les eConfirm)
    this.bIsTablet = false;
    try {
        if (typeof (isTablet) == 'function')
            this.bIsTablet = isTablet();
        else if (typeof (top.isTablet) == 'function')
            this.bIsTablet = top.isTablet();
    }
    catch (e) {

    }

    // Taille de la fenêtre spécifiée par l'appelant
    // Convertie en String pour les vérifications et calculs automatiques ci-dessous
    if (typeof width == "undefined" || width == null || typeof height == "undefined" || height == null) {
        this.width = (1024 - 40) + '';
        this.height = (768 - 40) + '';
    }
    else {
        this.width = width + '';
        this.height = height + '';
    }
    this.trace("Taille demandée pour la fenêtre : " + this.width + " x " + this.height);


    // Taille de la page
    this.initSize = function () {
        var windowWH = getWindowWH(document);
        this.docWidth = windowWH[0];
        this.docHeight = windowWH[1];

        // #58 123 : Calcul de la taille totale du document, scroll compris
        this.maxDocWidth = getMaxDocWidth();
        this.maxDocHeight = getMaxDocHeight();

        /*
        //GCH #33929 : il faut en plus ajouter le scroll pour que lorsque l'on est en zoom sur la tablette, il soit compté les dimension de la fenêtre complète
        /*
        #58 123 : la valeur de scroll est désormais utilisée pour positionner le coin haut gauche de la fenêtre (cf. pLeft et pTop dans show())
        De plus, à ce jour (10/2017), il semblerait que les navigateurs aient uniformisé ce comportement, que ce soit sur PC et tablettes
        (les valeurs de scroll sont à 0, même si on zoome - testé sur une Galaxy Tab 4 avec Android 5.0.2 et Chrome)
        Ce correctif #33 929 ne semble donc plus nécessaire, y compris dans le contexte décrit sur la demande (affichage des détails d'une fiche Planning lorsqu'on clique dessus en mode graphique)
        Les popups semblent bien positionnées.
        */

        /*
        if (this.bIsTablet) {
        */
        var scrlPos = [0, 0];
        var topScrlPos = [0, 0];
        if (typeof (getScrollXY) == 'function')
            scrlPos = getScrollXY();
        //if (typeof (top.getScrollXY) == 'function')
        //    topScrlPos = top.getScrollXY();
        /*
            this.docWidth = this.docWidth + scrlPos[0];
            this.docHeight = this.docHeight + scrlPos[1];
        }
        */

        // En revanche, on conserve une partie du correctif pour calculer et exposer les valeurs de scroll
        this.scrollWidth = scrlPos[0];
        this.scrollHeight = scrlPos[1];
        this.topScrollWidth = topScrlPos[0];
        this.topScrollHeight = topScrlPos[1];

        this.trace("Largeur et hauteur de document racine visibles (hors scroll) : " + getNumber(this.docWidth) + ", " + getNumber(this.docHeight));
        this.trace("Largeur et hauteur de document racine maximales (scroll compris) : " + getNumber(this.maxDocWidth) + ", " + getNumber(this.maxDocHeight));
        this.trace("Valeurs de scroll du document racine (horizontal, vertical) : " + getNumber(this.topScrollWidth) + ", " + getNumber(this.topScrollHeight));
    };
    this.initSize();

    this.tabScript = new Array();
    this.tabCss = new Array();

    this.Buttons = new Array();

    // Taille de la fenêtre recalculée en valeur absolue si %
    this.initAbsSize = function () {
        if (this.width.indexOf("%") != -1) {
            var widthPercentValue = getNumber(this.width);
            var containerWidth = getNumber(this.docWidth);
            this.trace("Largeur de la fenêtre en pourcentage : " + widthPercentValue + " (conteneur : " + containerWidth + ")");
            if (widthPercentValue > 0 && containerWidth > 0)
                this.absWidth = containerWidth * (widthPercentValue / 100);
            else
                this.absWidth = getNumber(this.width);
        }
        else {
            // On convertit la chaine de caractères en nombre pour que les calculs suivants restent corrects
            this.absWidth = Math.round(getNumber(this.width));
        }
        this.trace("Largeur absolue recalculée : " + this.absWidth);

        if (this.height.indexOf("%") != -1) {
            var heightPercentValue = getNumber(this.height);
            var containerHeight = getNumber(this.docHeight);
            this.trace("Hauteur de la fenêtre en pourcentage : " + heightPercentValue + " (conteneur : " + containerHeight + ")");
            if (heightPercentValue > 0 && containerHeight > 0)
                this.absHeight = containerHeight * (heightPercentValue / 100);
            else
                this.absHeight = getNumber(this.height);
        }
        else {
            // On convertit la chaine de caractères en nombre pour que les calculs suivants restent corrects
            this.absHeight = Math.round(getNumber(this.height));
        }
        this.trace("Hauteur absolue recalculée : " + this.absHeight);

        // Ajustement de la taille de la fenêtre si elle est trop grande par rapport à la taille de l'écran/du document
        if (this.absWidth > getNumber(this.docWidth)) {
            this.trace("Fenêtre trop large par rapport à l'espace disponible à l'écran (demandé : " + this.width + " (" + this.absWidth + "), disponible : " + getNumber(this.docWidth) + ")");
            this.absWidth = getNumber(this.docWidth) - 20;
            this.trace("Nouvelle largeur : " + this.absWidth);
        }
        if (this.absHeight > getNumber(this.docHeight)) {
            this.trace("Fenêtre trop haute par rapport à l'espace disponible à l'écran (demandé : " + this.height + " (" + this.absHeight + "), disponible : " + getNumber(this.docHeight) + ")");
            this.absHeight = getNumber(this.docHeight) - 20;
            this.trace("Nouvelle hauteur : " + this.absHeight);
        }
    };
    this.initAbsSize();

    var containerDiv = null;
    var backgroundDiv = null;       //Div de background
    var mainDiv = null;             //DivPrincipal
    var tdMsgDetails = null;
    var mainDivLibelleToolTip = null    //TOOLTIP DivPrincipal
    var titleDiv = null;            //DivPrincipal
    var buttonsDiv = null;          //DivPrincipal
    var toolbarDivLeft = null;
    var toolbarDivRight = null;
    var uId = randomID(10);

    activeModalId = uId;


    this.UID = uId;

    this.iframeId = "frm_" + uId;   // MAB - pointeur vers l'iframe

    if (top) {
        //top.window['_mdName'][this.iframeId] = this;
    }


    var parameters = new Array(); 	//Paramètres (tableau de updaterParameter)
    var msg = "";                   //Message à afficher
    var msgDetails = "";            //Message détaillé à afficher
    var msgType = 0;                //Type de l'icone à afficher (critical, info....)
    var ednuPopup = null;
    this.arrFct = new Array();
    this.title = title; //  + "  [" + uId + "]";

    this.eltToDisp;
    this.sOldHTML;

    //Pour la génération de case à cocher/bouton radio dynamique :
    //  - Si à true => plusieurs réponses possible donc cases à cocher
    //  - Si à false => réponse unique donc bouton radio
    this.isMultiSelectList = false;

    this.toString = function () {
        return 'eModalDialog';
    }

    var divResize = null;

    var promptLabel = "";
    var promptDefValue = "";

    var tdButtons = null; // right
    var tdButtonsLeft = null;
    var tdButtonsMid = null;
    var ulToolbarLeft = null;
    var ulToolbarRight = null;

    //Avec ou sans le div des boutons
    this.noButtons = false;
    //Avec ou sans le div du title
    this.noTitle = true;
    //Avec ou sans le div de la barre d'outils
    this.noToolbar = true;
    // Masquer le bouton "Agrandir"
    this.hideMaximizeButton = false;
    // Masquer le bouton "Fermer"
    this.hideCloseButton = false;
    // Fermer la fenêtre lors de l'appui sur la touche Echap
    // 0 ou false ou undefined : désactivé
    // 1 : activé, avec message de confirmation avant fermeture
    // 2 : activé, sans message de confirmation avant fermeture
    this.closeOnEscKey = 1;
    // Variable à mettre à true lorsque l'utilisateur a effectué des modifications sur la fenêtre
    this.unsavedChanges = false;
    // cette variable sera mise à true lorsque la popup de confirmation sera affichée, afin de ne pas pouvoir être affichée plusieurs fois lors
    // d'appuis successifs sur la touche Echap
    this.confirmChangesModalDisplayed = false;

    this.onIframeLoadComplete = function () { }; //par défaut, fonction vide

    // Arguments à passer
    this.inputArgs = new Array();

    this.addArg = function (val) {
        this.inputArgs.push(val);
    };

    // Gestion de la taille de la popup (maximiser)
    this.sizeStatus = "";
    this.initialTitleWidth = 0;
    this.initialContainerTop = 0;
    this.initialContainerLeft = 0;
    this.initialContainerWidth = 0;
    this.initialContainerHeight = 0;
    this.initialMainWidth = 0;
    this.initialMainHeight = 0;
    this.initialButtonsWidth = 0;
    this.initialButtonsHeight = 0;
    this.initialContainerMargin = 0;

    this.trace("Calcul du z-index...");
    this.level = 100;// z-index de base = 100
    this.level = GetMaxZIndex(document, this.level, !bDontIgnoreSetWait);
    // On se place à un z-index au-dessus du plus haut élément positionné sur la fenêtre parente
    this.level++;
    this.trace("z-index retenu pour la modal dialog : " + this.level);

    //Classe Css  / Icone de la fenêtre
    this.IconClass = "";
    //Classe Css  : message
    this.textClass = "";

    // Stockage des informations concernant le navigateur sur l'objet
    this.browser = new getBrowser();

    this.setMessage = function (_msg, _msgDetails, _msgType, bAllowHtml) {

        if (!bAllowHtml || typeof (bAllowHtml) == "undefined" || bAllowHtml == null) {
            _msg = encodeHTMLEntities(_msg);
            _msgDetails = encodeHTMLEntities(_msgDetails);
        }

        msg = this.processEudoTag(_msg + "");
        msgDetails = this.processEudoTag(_msgDetails + "");
        msgType = _msgType;


    };



    this.processEudoTag = function (value) {

        value += "";

        //remplace pseudo-tag
        // tag [XURL url='monurl']monlien[/XURL]
        //#42 597 Permettre les  simples cotes dans l'URL
        value = value.replace(/\[XURL url='(.+?)'\](.+?)\[\/XURL\]/g, "<a target=\"_blank\" href=\"$1\">$2</a>");

        //Tag [[BR]]
        value = value.replace(/\[\[BR\]\]/g, "<br/>");

        value = value.replace(/\[\[SPAN onclick='(.+?)' class='(.+?)'\]\]/g, "<span onclick=\"$1\" class=\"$2\">");
        value = value.replace(/\[\[\/SPAN\]\]/g, "</span>");

        value = value.replace(/\[\[UL\]\]/g, "<ul>");
        value = value.replace(/\[\[\/UL\]\]/g, "</ul>");

        value = value.replace(/\[\[LI\]\]/g, "<li>");
        value = value.replace(/\[\[\/LI\]\]/g, "</li>");


        //Tag [[SPACE]]
        value = value.replace(/\[\[SPACE\]\]/g, "&nbsp;");

        // Commentaire html
        value = value.replace(/&lt;!--/gi, "<!--");
        value = value.replace(/--&gt;/gi, "-->");

        //permet de restaurer certains <br>
        return value.replace(/\n/g, "<br/>").replace(/&#10;/g, "<br/>").replace(/&lt;br\s*\/&gt;/gi, "<br/>").replace(/&lt;br\s*&gt;/gi, "<br/>");
    };

    this.setElement = function (oElt) {
        this.eltToDisp = oElt;
        this.sOldHTML = oElt.outerHTML;

    }

    this.TargetObject = null;

    if (this.url != null) {
        //Appel Ajax
        ednuPopup = new eUpdater(this.url, 1);
    };

    this.ErrorCallBack = null;
    this._callOnCancel = '';
    this.CallOnOk = '';
    this._argsOnOk = '';
    /*
    buttonId : si "cancel" execute la fonction définit sur le bouton dès que l'on ferme avec la croix
    si "ok" execute la fonction définit sur le bouton sur la variable this.CallOnOk.
    */

    this.addButton = function (btnLabel, actionFunction, cssName, args, buttonId, position) {

        /*
        <div class="button-green" id="buttonId" >
        <div class="button-green-left"></div>
        <div class="button-green-mid">un libéllé très très très long</div>
        <div class="button-green-right"></div>
        </div>
        */

        if (this.noButtons == true)
            return;

        if (position != 'left') {
            if (position != 'mid') {
                position = 'right';
            }
        }

        var btn = top.document.createElement('div');
        //Ajout d'un flag sur le boutton pour identification
        setAttributeValue(btn, "ednmodalBtn", 1);

        btn.className = cssName;

        if (position == 'left') {
            btn.className += ' button-position-left';
        }
        else if (position == 'mid') {
            btn.className += ' button-position-mid'

        }

        btn.style.align = position;

        if (typeof buttonId != "undefined")
            btn.id = buttonId;


        var btnLeft = top.document.createElement('div');

        btnLeft.className = cssName + "-left";
        btn.appendChild(btnLeft);

        var btnMid = top.document.createElement('div');
        btnMid.className = cssName + "-mid";
        btnMid.innerHTML = btnLabel;

        if (typeof buttonId != "undefined")
            btnMid.id = buttonId + "-mid";

        btn.appendChild(btnMid);

        var btnRight = top.document.createElement('div');
        btnRight.className = cssName + "-right";
        btn.appendChild(btnRight);


        if (typeof (actionFunction) == "function") {
            var functionName = getFunctionName(actionFunction);

            if (typeof (functionName) == 'undefined' || functionName == '') {
                // Anonymous function
                btn.onclick = actionFunction;
            }
            else {
                btn.onclick = new Function(functionName + "(\"" + (args && (typeof args === 'string' || args instanceof String) ? args.replace(/\"/g, '\\\"') : args) + "\",'" + uId + "')");
            }
        }
        else if (actionFunction != null)
            btn.onclick = new Function(actionFunction + "(\"" + (args && (typeof args === 'string' || args instanceof String) ? args.replace(/\"/g, '\\\"') : args) + "\",'" + uId + "')");

        this.Buttons.push(btn);

        if (position == 'left') {
            tdButtonsLeft.appendChild(btn); //En premier pour qu'il apparraisse en dernier
        }
        else if (position == 'mid') {
            tdButtonsMid.appendChild(btn); //button au Millieu 
        }
        else {
            tdButtons.appendChild(btn); //En premier pour qu'il apparraisse en dernier
            //tdButtons.appendChild(emptyBtn);  //GCH pour PNO : retiré car sera géré dans la css des bouton direct
        }
        // NBA 03-12-2012
        // Si le l'id du bouton est "cancel" ou "cancel_btn" dans ce cas à la fermeture sur la croix en haut à droite on lance la fonction passée
        if (buttonId == "cancel" || buttonId == "cancel_btn") {
            if (typeof btn.onclick == "function" && btn.onclick != null) {
                if (!this.hideCloseButton) {
                    this.AddCancelMethode(btn.onclick);
                }
            }
            else {
                btn.onclick = this.hide;
            }
            this._callOnCancel = btn.onclick;
        }
        if (buttonId == "ok") {
            this.CallOnOk = btn.onclick;
            this._argsOnOk = args;
        }

    };

    ///summary
    // #60 193 - Si aucun élément n'a le focus à l'ouverture de la fenêtre, on le place sur le contrôle indiqué, ou sur un contrôle factice créé à la volée
    // Permet de câbler la fonction Echap sur la fenêtre active et non sur la fenêtre parente si la fenêtre active n'a pas de contrôle prenant automatiquement le focus
    // (ex : fenêtre de choix de fichier, eFieldFiles.aspx)
    ///summary
    this.setFocusOnWindowTimer = null;
    this.setFocusOnWindow = function (targetControl) {
        // Si le contenu de la fenêtre n'est pas encore complètement chargé, on diffère l'exécution de la fonction d'une seconde, jusqu'à ce qu'elle ait pu s'exécuter
        if (!this.getIframe() || !this.getIframe().document || !this.getIframe().document.body) {
            this.setFocusOnWindowTimer = setTimeout(function () { that.setFocusOnWindow(targetControl); }, 1000);
        }
        else {
            clearTimeout(this.setFocusOnWindowTimer);
            // Si le contrôle comportant actuellement le focus n'est pas un élement focusable (souvent <body>), on en crée un, ou on utilise celui indiqué en paramètre
            if (!isFocusableElement(this.getIframe().document.activeElement)) {
                // Si un contrôle à focuser a été précisé, on l'utilise
                if (targetControl && isFocusableElement(targetControl))
                    targetControl.focus();
                // Si le faux contrôle de prise de focus a déjà été créé lors d'un précédent appel à la fonction, on le réutilise
                else if (this.getIframe().document.getElementById("fakeFocusElement_" + this.UID))
                    this.getIframe().document.getElementById("fakeFocusElement_" + this.UID).focus();
                // Sinon, on le crée
                else {
                    // Création d'un élément de saisie factice, positionné hors du champ de vision, pour que les raccourcis clavier s'exécutent sur la fenêtre active,
                    // et non la précédente. Il est ajouté dans un conteneur déporté pour ne pas être cliquable
                    // Il ne doit pas être mis en display: none, visibility: hidden ou disabled car les navigateurs modernes refuseront alors de lui donner le focus
                    var fakeFocusElementContainer = this.getIframe().document.createElement("div");
                    var fakeFocusElement = this.getIframe().document.createElement("input");
                    // -------------------
                    // Conteneur
                    fakeFocusElementContainer.id = "fakeFocusElementContainer_" + this.UID;
                    fakeFocusElementContainer.style.position = "absolute"; // pas de dépendance aux autres contrôles de la page
                    // On positionne malgré tout le contrôle au sein de la fenêtre active pour se souvenir qu'il y est rattaché, et ne pas se heurter à une future
                    // éventuelle restriction navigateur sur les éléments positionnés en négatif
                    fakeFocusElementContainer.style.left = "0px";
                    fakeFocusElementContainer.style.top = "0px";
                    // Les attributs ci-dessous le rendent non visible, mais non masqué pour autant. Empêche de cliquer dans le champ de saisie, mais autorise le focus
                    fakeFocusElementContainer.style.width = "0px";
                    fakeFocusElementContainer.style.height = "0px";
                    fakeFocusElementContainer.style.overflow = "hidden";
                    fakeFocusElementContainer.style.opacity = "0";
                    // -------------------
                    // Champ de saisie
                    fakeFocusElement.id = "fakeFocusElement_" + this.UID;
                    fakeFocusElement.attributes["tabindex"] = "0"; // peut être nécessaire pour autoriser le focus
                    // On le rend en readonly pour empêcher toute saisie réelle dedans
                    // Attention : il ne faut pas le mettre en disabled, car le navigateur refuse alors le focus
                    fakeFocusElement.readOnly = true;
                    // le contrôle doit rester dans la zone de son conteneur positionné en absolute pour ne pas avoir d'incidence sur la mise en page
                    fakeFocusElement.style.position = "relative";
                    // On positionne malgré tout le contrôle au sein de la fenêtre active pour se souvenir qu'il y est rattaché, et ne pas se heurter à une future
                    // éventuelle restriction navigateur sur les éléments positionnés en négatif
                    fakeFocusElement.style.left = "0px";
                    fakeFocusElement.style.top = "0px";
                    // La plupart des navigateurs traceront malgré tout un contrôle visible de quelques pixels
                    fakeFocusElement.style.width = "0px";
                    fakeFocusElement.style.height = "0px";
                    // L'attribut opacity rendra toutefois le contrôle non visible à coup sûr, mais non masqué pour autant
                    fakeFocusElement.style.opacity = "0";
                    // -------------------
                    // Rattachement au DOM
                    fakeFocusElementContainer.appendChild(fakeFocusElement);
                    this.getIframe().document.body.appendChild(fakeFocusElementContainer);
                    // -------------------
                    // Et enfin, focus sur l'élément de saisie
                    fakeFocusElement.focus();
                }
            }
        }
    };


    this.switchButtonDisplay = function (handle, bHide) {

        if (typeof bHide == "unedefined")
            bHide = false;

        var myButt = this.Buttons.filter(function (btn) {
            return btn.id == handle
        })

        if (myButt.length > 0) {
            myButt[0].style.display = bHide ? "none" : "";
        }



    }

    ///summary
    ///Cache les boutons sur la modal afin qu'on ne les vois pas au premier chargement et qu'on affiche uniquement le nécessaire par la suite
    /// TODO : Déplacer cette fonction dans eModal.js
    ///summary
    this.hideButtons = function () {
        try {

            var buttonModal = window.parent.document.getElementById("ButtonModal_" + this.UID);
            ///On parcours les div du conteneur des boutons, et pour chaque div ayant un ID de type ******_btn , donc un bouton.
            ///Et on les masque.
            var buttons = buttonModal.getElementsByTagName("div");
            for (iBtn = 0; iBtn < buttons.length; iBtn++) {
                if (buttons[iBtn].id.indexOf("_btn") > 0 && buttons[iBtn].id.indexOf("_btn") + 4 == buttons[iBtn].id.length)
                    buttons[iBtn].style.display = "none";
            }

        } catch (ex) { }
    };


    this.romovebtnClose = function () {
        try {
            var buttonModal = top.document.getElementById("ImgControlBoxClose_" + this.UID);
            buttonModal.style.display = "none";


        } catch (ex) { }
    };


    this.addButtons = function (aBtnsArray) {
        if (typeof (aBtnsArray) != "object")
            return;

        for (var i = 0; i < aBtnsArray.length; i++) {
            var btn = aBtnsArray[i];
            this.addButton(btn.label, btn.fctAction, btn.css);
        }
    }

    ///affiche ou masque les boutons de la modal
    this.switchButtonsDisplay = function (bShow) {

        if (buttonsDiv) {
            var buttons = buttonsDiv.querySelectorAll("[ednmodalbtn='1']");
            for (iBtn = 0; iBtn < buttons.length; iBtn++) {
                if (bShow)
                    buttons[iBtn].style.display = "";
                else
                    buttons[iBtn].style.display = "none";
            }
        }
    };



    //
    this.addScript = function (scriptName) {
        if (this.tabScript.indexOf(scriptName))
            this.tabScript.push(scriptName);
    };

    this.addCss = function (cssName) {
        if (this.tabCss.indexOf(cssName))
            this.tabCss.push(cssName);
    };


    //Permet de définir la méthode à appeler au click sur le bouton de croix (qui est suivit par la fermeture réeel
    this.AddCancelMethode = function (callOnCancel) {
        this._callOnCancel = callOnCancel;
        document.getElementById("ImgControlBoxClose_" + uId).onclick = callOnCancel;
    }

    this.addToolbarButton = function (strBtnId, strBtnLabel, strBtnToolTip, strMainCSSName, strLabelCSSName, strImgCSSName, strLinkCSSName, bCreateLabel, bCreateImg, strPosition, oActionFunction, args) {

        if (this.noToolbar == true)
            return;

        var btn = top.document.createElement('li');
        btn.style.display = "none";
        btn.style.visibility = "hidden";
        btn.className = strMainCSSName;
        btn.id = strBtnId;
        btn.title = strBtnToolTip;
        var onClickFunction;
        if (typeof (oActionFunction) == "function") {
            var functionName = getFunctionName(oActionFunction);
            if (typeof (functionName) == 'undefined' || functionName == '')      // Anonymous function
                onClickFunction = oActionFunction;
            else
                onClickFunction = new Function(getFunctionName(oActionFunction) + "(\"" + args + "\",'" + uId + "')");
        }
        else
            onClickFunction = new Function(oActionFunction + "(\"" + args + "\",'" + uId + "')");

        var oTargetToolbar = ulToolbarRight;
        if (strPosition == 'left')
            oTargetToolbar = ulToolbarLeft;
        oTargetToolbar.appendChild(btn);

        var strToolTip = strBtnToolTip;
        if (strToolTip == null || strToolTip == '')
            strToolTip = strBtnLabel;

        // Si les classes spécifiques des éléments enfants ne sont pas précisées, on leur applique la classe "principale"
        // sauf si les classes spécifiques sont précisées à vide ("")
        if (strLinkCSSName == null)
            strLinkCSSName = strMainCSSName;
        if (strImgCSSName == null)
            strImgCSSName = strMainCSSName;
        if (strLabelCSSName == null)
            strLabelCSSName = strMainCSSName;

        // Si on crée un bouton avec libellé, on crée une structure lien > image > texte pour que l'ensemble réagisse à l'interaction avec la souris (surbrillance, clic...)
        // ou, s'il n'y a pas de lien/fonction a exécuter, simplement un libellé
        var oParentElt = btn;
        if (typeof (oActionFunction) == 'function' && bCreateLabel && strBtnLabel != '') {
            var btnLabelLink = top.document.createElement("a");
            oParentElt = btnLabelLink;
            btnLabelLink.className = strLinkCSSName;
            btnLabelLink.id = strBtnId + "_lbl";
            btnLabelLink.title = strToolTip;
            btnLabelLink.onclick = onClickFunction;
            btn.appendChild(btnLabelLink);
        }
        if (bCreateImg) {
            var btnGhostImage = top.document.createElement("span");
            btnGhostImage.className = strImgCSSName;
            btnGhostImage.id = strBtnId + "_img";
            btnGhostImage.title = strToolTip;
            oParentElt.appendChild(btnGhostImage);
        }
        if (bCreateLabel && strBtnLabel != '') {
            var btnLabelText = top.document.createElement("span");
            btnLabelText.className = strLabelCSSName;
            btnLabelText.id = strBtnId + "_text";
            btnLabelText.title = strToolTip;
            btnLabelText.innerHTML = strBtnLabel;
            oParentElt.appendChild(btnLabelText);
        }
        // Sinon, on affecte juste le clic sur l'élément li ; l'affichage de son image devra alors être géré sur sa classe CSS
        else {
            btn.onclick = onClickFunction;
        }

        return btn;
    };


    //retourne vrai si la modal est disponible.
    // si le top n'est plus dispo, c'est que le contexte à changer par rapport à sa disponibilité
    // notament si elle a été hide ou ouverte sur un autre signet
    // cf 37135
    this.IsAvailable = function () {
        return top != null;
    }



    ///Retourne l'objet Iframe incluant le tag contrairement a getIframe qui retourne le contentwindow
    this.getIframeTag = function () {


        if (top.document.getElementById("frm_" + uId))
            return top.document.getElementById("frm_" + uId);

        return null;
    };

    ///Retourne la window de l'iframe
    this.getIframe = function () {
        if (top.document.getElementById("frm_" + this.UID))
            return top.document.getElementById("frm_" + this.UID).contentWindow;

        return null;
    };

    this.getDivContainer = function () {
        if (top.document.getElementById("ContainerModal_" + uId))
            return top.document.getElementById("ContainerModal_" + uId);

        return null;
    }

    this.getDivButton = function () {
        if (top.document.getElementById("ButtonModal_" + uId))
            return top.document.getElementById("ButtonModal_" + uId);

        return null;
    }

    //  Ajoute un bouton
    //  btnLabel : label du bouton
    //  actionFunction : Objet function sur le onclick
    //  cssName : CSS syr le bouton
    //  buttonId : si "cancel" execute la fonction définit sur le bouton dès que l'on ferme avec la croix
    //      si "ok" execute la fonction définit sur le bouton sur la variable this.CallOnOk.

    this.addButtonFct = function (btnLabel, actionFunction, cssName, buttonId) {
        if (this.noButtons == true)
            return;

        var btn = document.createElement('div');

        //Ajout d'un flag sur le boutton pour identification
        setAttributeValue(btn, "ednmodalBtn", 1);

        btn.className = cssName;
        btn.style.align = "right";



        if (typeof buttonId != "undefined") {
            btn.id = buttonId;
            btn.handle = buttonId;
        }

        var btnLeft = document.createElement('div');
        btnLeft.className = cssName + "-left";
        btn.appendChild(btnLeft);

        var btnMid = document.createElement('div');
        btnMid.className = cssName + "-mid";
        btnMid.innerHTML = btnLabel;
        if (typeof buttonId != "undefined")
            btnMid.id = buttonId + "-mid";
        btn.appendChild(btnMid);
        var btnRight = document.createElement('div');
        btnRight.className = cssName + "-right";
        btn.appendChild(btnRight);

        if (typeof (actionFunction) == "function")
            btn.onclick = actionFunction;

        this.Buttons.push(btn);

        tdButtons.appendChild(btn); //En premier pour qu'il apparraisse en dernier

        // Si le l'id du bouton est "cancel" dans ce cas à la fermeture sur la croix en haut à droite on lance la fonction passée
        if (buttonId == "cancel") {
            this._callOnCancel = btn.onclick;
            if (!this.hideCloseButton) {
                this.AddCancelMethode(this._callOnCancel);
            }
        }
    };

    //Ajoute une fonction à la fenêtre modale
    this.addFunction = function (sFctName, fct) {
        if (typeof (that.arrFct[fct]) == "undefined" && typeof (fct) == "function") {
            that.arrFct[sFctName] = fct;
        }
    }

    // Commenté pour des raisons d'effets indésirables sur la fermeture des pop-up des champs obligatoires.
    //this.hide = function (afterHide) {
    //    containerDiv.className = "containerModal closeTransition";

    //    setTimeout(function () { that.hideMe(afterHide); }, 200);

    //};


    ///Fait progressivement disparaitre la modal en 10 étapes (en nTime ms )
    this.fade = function (nTime) {

        var element = that.getDivContainer();

        var nOp = 1;

        if (nTime == 0)
            that.hide();

        if (nTime < 50)
            nTime = 50;

        var nInter = nTime / 10;

        var step = 0.1;

        var timer = setInterval(function () {
            try {

                if (nOp <= step) {
                    clearInterval(timer);
                    that.hide();
                }

                element.style.opacity = nOp;
                nOp -= nOp * step;
            }
            catch (e) {
                clearInterval(timer);
                that.hide();
            }

        }, nInter);


    };



    this.hide = function (afterHide) {
        globalModalFile = false;
        if (that.debugMode)
            that.trace("Masquage de la fenêtre : " + uId);

        if (!that.bIsTablet && !(that.browser.isIE && that.browser.isIE8)) {
            try {


                that.removeAllChild(backgroundDiv);
            }
            catch (exp) {
                that.trace("ERREUR 1 lors du masquage : " + exp.Description);
            }

            try {
                if (containerDiv)
                    that.removeAllChild(containerDiv);
            }
            catch (exp) {
                that.trace("ERREUR 2 lors du masquage : " + exp.Description);
            }

            try {
                document.body.style.overflow = oldoverflow;
            }
            catch (exp) {
                that.trace("ERREUR 3 lors du masquage : " + exp.Description);
            }
            //Retire la modal du tableau global
            try {
                if (typeof that.Handle != "undefined" && top.window['_md'])
                    delete (top.window['_md'][that.Handle]);

                delete (top.window['_mdName'][that.iframeId]);
            }
            catch (exp) {
                that.trace("ERREUR 4 lors du masquage : Impossible de retirer le handle du tableau global : " + exp.Description);
            }
        }

        // Sur les tablettes, supprimer l'objet du DOM peut entraîner un crash brutal du navigateur
        // On se contente donc juste, sur ces supports, de masquer visuellement la modal dialog sans la détruire en mémoire
        // On altère leurs IDs afin que d'éventuels getElementById() sur les composantes de la fenêtre ne puissent plus pointer vers
        // des fenêtres masquées, mais uniquement vers les dernières fenêtres instanciées (qui auront l'ID initialement souhaité par le
        // développeur, jusqu'à ce que la fenêtre soit masquée à son tour)
        else {
            if (backgroundDiv) {
                backgroundDiv.style.display = 'none';
                backgroundDiv.id = that.iframeId + "_" + backgroundDiv.id;
                var backgroundDivChildren = backgroundDiv.querySelectorAll("*");
                for (var i = 0; i < backgroundDivChildren.length; i++) {
                    backgroundDivChildren[i].id = that.iframeId + "_" + backgroundDivChildren[i].id;
                }
            }
            if (containerDiv) {
                containerDiv.style.display = 'none';
                containerDiv.id = that.iframeId + "_" + containerDiv.id;
                var containerDivChildren = containerDiv.querySelectorAll("*");
                for (var i = 0; i < containerDivChildren.length; i++) {
                    containerDivChildren[i].id = that.iframeId + "_" + containerDivChildren[i].id;
                }
            }
            // On met à jour certaines propriétés pour que les scripts qui les utilisent considèrent que la modal dialog n'existe plus
            that.isModalDialog = false;
            //top.window['_md'][that.Handle] = null;
            if (typeof (that.getIframe) == 'function' && that.getIframe() && that.getIframe().document) {
                that.getIframe().document["_ismodal"] = 0;
                that.getIframe().document.parentModalDialog = null;
            }
        }
        if (typeof (afterHide) == "function")
            afterHide();

        if (typeof (that.onHideFunction) == "function")
            that.onHideFunction();

        if (this.ModalDialogs)
            this.ModalDialogs[that.UID] = null;

        // #60 193 - A la fermeture (via Echap ou autre), on repositionne le focus sur une éventuelle fenêtre parente
        var parentModalDialog = that.getParentModalDialog();
        if (parentModalDialog)
            parentModalDialog.setFocusOnWindow();
    };


    this.removeAllChild = function (obj) {

        if (typeof obj == 'undefined' || obj == null)
            return;

        try {
            if (obj.childNodes) {
                var child = obj.childNodes
                var nb = child.length;
                for (var i = nb; i > 0; i--) {
                    try {
                        this.removeAllChild(child[i - 1]);
                    }
                    catch (ex) {
                        this.trace("ERREUR 6 lors du masquage : Impossible de retirer le handle du tableau global : " + ex.Description);
                    }
                }
            }
        }
        catch (exp) {
            this.trace("ERREUR 7 lors du masquage : Impossible de retirer le handle du tableau global : " + exp.Description);
        }
        try {
            var oParent = obj.parentElement;
            if (oParent && oParent.removeChild)
                oParent.removeChild(obj);
        }
        catch (exp) {
            this.trace("ERREUR 8 lors du masquage : Impossible de retirer le handle du tableau global : " + exp.Description);

        }
    };

    this.displayContent = function () {
        mainDiv.appendChild(this.eltToDisp);
    };
    var mainMsgDiv;
    this.createLocalMsgBox = function () {
        //Nom de la classe CSS
        var cssClass = "";
        var cssFont = "";


        if (msgType == null) {
            msgType = "0";
        }

        switch (msgType.toString()) {

            case MsgType.MSG_CRITICAL.toString():
                cssClass = "error";
                cssFont = "icon-times-circle ";
                break;
            case MsgType.MSG_QUESTION.toString():
                cssClass = "quote";
                cssFont = "icon-question-circle";
                break;
            case MsgType.MSG_EXCLAM.toString():
                cssClass = "warn";
                cssFont = "icon-exclamation-triangle";
                break;
            case MsgType.MSG_INFOS.toString():
                cssClass = "info";
                cssFont = "icon-info-circle";
                break;
            case MsgType.MSG_SUCCESS.toString():
                cssClass = "success";
                cssFont = "icon-check-circle";
                break;
        }
        mainMsgDiv = document.createElement("div");




        mainMsgDiv.className = "msg-container";
        mainDiv.appendChild(mainMsgDiv);
        setAttributeValue(containerDiv, "msgtype", msgType);

        mainMsgDiv.style.height = mainDiv.style.height;
        mainMsgDiv.style.width = containerDiv.style.width;
        var msgTable = document.createElement("table");
        mainMsgDiv.appendChild(msgTable)
        var tBody = document.createElement("tbody");
        msgTable.appendChild(tBody);

        var tr = document.createElement("tr");
        tBody.appendChild(tr);

        //td logo
        var tdLogo = document.createElement("td");
        tr.appendChild(tdLogo);
        tdLogo.className = "td-logo";
        tdLogo.rowSpan = 2;
        /*
        var imgLogo = top.document.createElement("img");
        tdLogo.appendChild(imgLogo);
        imgLogo.className = "logo-" + cssClass;
        imgLogo.src = "ghost.gif";
        */
        var spanLogo = document.createElement("span");
        tdLogo.appendChild(spanLogo);
        spanLogo.className = cssFont + " logo-" + cssClass;

        //td message
        var tdMsg = document.createElement("td");
        tdMsg.id = "msgbox_msg_" + uId;
        tdMsg.innerHTML = msg + "<br>";
        tdMsg.className = "text-alert-" + cssClass + " " + this.textClass;
        tr.appendChild(tdMsg);

        tr = document.createElement("tr");
        tBody.appendChild(tr);

        //td message détaillé
        tdMsgDetails = document.createElement("td");
        tdMsgDetails.id = "msgbox_msgdetails_" + uId;
        tdMsgDetails.className = "text-msg-" + cssClass;
        tdMsgDetails.innerHTML = msgDetails;
        tr.appendChild(tdMsgDetails);
        var oldoverflow = "";
    };

    this.setPrompt = function (msgPrompt, defValue) {
        promptLabel = msgPrompt;
        promptDefValue = defValue;
    };

    this.createPrompt = function () {
        //Nom de la classe CSS

        var block = document.createElement("div");
        block.className = "prompt-container";

        var line = document.createElement("div");
        block.appendChild(line);

        var label = document.createElement("div");
        label.innerHTML = promptLabel;
        label.className = "promptLabel";
        line.appendChild(label);

        var textbox = document.createElement("input");
        textbox.setAttribute("id", "InputPrompt_" + uId);
        textbox.className = "promptText";
        textbox.value = promptDefValue;

        line.appendChild(textbox);

        mainDiv.appendChild(block);


    };


    this.createControl = function (sType, sName, sId, sValue, sLabel, sCSS, oDivGbl, bCkecked) {

        //KHA - pas fait de res car ces messages ne sont pas censés apparaitre à l'utilisateur
        //si les appels sont fait correctement
        if (sType.indexOf("'") > -1 || sType.indexOf("\\") > -1 || sType.indexOf(" ") > -1) {
            eAlert(0, "Controle Invalide", "Paramètre \"type\"");
            return;
        }
        if (sName.indexOf("'") > -1 || sName.indexOf("\\") > -1 || sName.indexOf(" ") > -1) {
            eAlert(0, "Controle Invalide", "Paramètre \"name\"");
            return;
        }
        if (sId.indexOf("'") > -1 || sId.indexOf("\\") > -1 || sId.indexOf(" ") > -1) {
            eAlert(0, "Controle Invalide", "Paramètre \"id\"");
            return;
        }
        if (sCSS.indexOf("'") > -1 || sCSS.indexOf("\\") > -1 || sCSS.indexOf(" ") > -1) {
            eAlert(0, "Controle Invalide", "Paramètre \"classe\"");
            return;
        }


        var div = this.createDiv(sCSS, oDivGbl);

        if (sType == "checkbox") {
            AddEudoCheckBox(top.document, div, bCkecked, sId, sLabel)
        }
        else if (sType == "literal") {
            div.innerHTML += sLabel;
        }
        else {

            var sValue = sValue.replace(/'/g, "\'").replace(/\"/g, "\\\"").replace(/\\/g, "\\\\");

            var strControl = "<input type='" + sType + "' name='" + sName + "' id='" + sId + "' value='" + sValue + "'" + (sType == "radio" && bCkecked ? " checked" : "") + ">";
            var strLabelCtrl = "<label for='" + sId + "'>" + sLabel + "</label>";

            if (sType == "radio")
                div.innerHTML += strControl + strLabelCtrl;
            else
                div.innerHTML += strLabelCtrl + strControl;
        }
    };

    this.createDiv = function (sCSS, oDiv, sLabel) {
        var divContainer = mainDiv;
        if (oDiv)
            divContainer = oDiv;
        else if (mainMsgDiv)
            divContainer = mainMsgDiv;

        var div = top.document.createElement("div");
        divContainer.appendChild(div);
        div.className = sCSS;

        if (sLabel && sLabel != "")
            div.innerHTML = sLabel;

        return div;
    }

    var docEventKey = null; //Document auquel sont attachés les événements

    this.createIframe = function (resp) {
        //Création de l'iframe
        var tmpFrm = top.document.createElement('iframe');
        tmpFrm.id = "frm_" + uId;
        tmpFrm.name = "frm_" + uId;

        tmpFrm.style.left = 0 + "px";
        tmpFrm.style.top = 0 + "px";
        tmpFrm.style.width = "100%";
        tmpFrm.style.height = "98%"; // NBA modif du 27-04-2012 pb affichage d'une scrollbar verticale
        tmpFrm.style.border = "0px";
        tmpFrm.style.margin = "0px";
        // 41590 CRU : Pouvoir définir l'attribut "scrolling" de l'iframe
        // Par défaut à Oui
        var scrolling = "yes";
        if (this.getParam("iframeScrolling") != "" && this.getParam("iframeScrolling") == "no") {
            scrolling = "no";
        }
        tmpFrm.scrolling = scrolling;
        tmpFrm.frameBorder = "0";

        if (mainDivLibelleToolTip != null)
            mainDivLibelleToolTip.appendChild(tmpFrm);
        else
            mainDiv.appendChild(tmpFrm);

        if ((tmpFrm.contentWindow) && (tmpFrm.contentWindow.document)) {

            tmpFrm.contentWindow.document.open();
            tmpFrm.contentWindow.document.write(resp);

            // MCR SPH 39993, faire un set de la variable : _ismodal apres le document.write() sinon l affectation n est pas faite avec IE !!
            tmpFrm.contentWindow.document["_ismodal"] = 1;

            tmpFrm.contentWindow.document.parentModalDialog = that;

            /*On attache la fonctions de touche native à l'evennement d'appuie sur une touche*/
            docEventKey = tmpFrm.contentWindow.document;    //On sauvegarde le document dont l'événements a été setté afin de le supprimmer ensuite
            setEventListener(tmpFrm.contentWindow.document, "keydown", KeyPressNativeFunction, false);
            /*********************************************************************************/
            setEventListener(tmpFrm, "load", that.DoFctLoadBody, false);

            //Attache les fonctions custom à l'objet document
            for (var fct in that.arrFct) {
                if (typeof (tmpFrm.contentWindow.document[fct]) == "undefined" && typeof (that.arrFct[fct]) == "function") {

                    //alert(fct + ' ' +that.arrFct[fct]);
                    tmpFrm.contentWindow.document[fct] = that.arrFct[fct];
                }
            }
            // Compatibilité IE : edge
            setEdgeCompatibility(tmpFrm.contentWindow.document);
            //Ajout des Css et Script
            if (that.tabCss.length > 0) {
                for (var ii = 0; ii < that.tabCss.length; ii++) {
                    addCss(that.tabCss[ii], "MODAL", tmpFrm.contentWindow.document);
                }
            }

            addScripts(that.tabScript, "MODAL", that.DoFctLoadScript, tmpFrm.contentWindow.document);
            tmpFrm.contentWindow.document.close();

        }
    };
    //Permet d'appeler la fonction définit dans onIframeLoadComplete
    //Elle permet de lancer la fonction qu'après le JS ET BODY chargée
    this.bScriptOk = false;
    this.bBodyOk = false;
    this.DoFctLoadBody = function () {
        that.bBodyOk = true;
        try {

            var oFrame = that.getIframeTag();
            if (!that.isSpecif)
                top.eTools.UpdateDocCss(oFrame.contentWindow.document);
        }
        catch (e) {

        }

        that.DoFctLoad();
    };
    this.DoFctLoadScript = function () {
        that.bScriptOk = true;
        that.DoFctLoad();
        /*
                var browser = new getBrowser();
                if (browser.isIE && browser.isIE8) {
                    var aIcon = document.querySelectorAll('[class^="icon-"]');
                    //alert(aIcon.length);
                    for (var nIcmptIds = 0; aIcon < aIcon.length; nIcmptIds++) {
                        var myIcon = aIcon[nIcmptIds];
                        addClass(myIcon, 'ie8fonts');
                    }
                    //alert("icon reloadé");
                }
        */
    };
    this.DoFctLoad = function () {

        // #60 193 - Si aucun élément du <body> ne prend le focus automatiquement à l'ouverture de la fenêtre, on simule le focus sur un élément donné, ou sur un faux élément
        // Permet de câbler la fonction Echap sur la fenêtre active et non sur la fenêtre parente si la fenêtre active n'a pas de contrôle prenant automatiquement le focus
        // (ex : fenêtre de choix de fichier, eFieldFiles.aspx)
        this.setFocusOnWindow();

        if (that.bScriptOk && that.bBodyOk && typeof (that.onIframeLoadComplete) == "function") {

            that.onIframeLoadComplete();

        };

        //top.document.getElementById("ContainerModal_" + uId).className = "ContainerModal";
    };

    //Fonction interne à la modale appelée lorsque l'on presse une touche
    function KeyPressNativeFunction(e) {
        var oFrm = that.getIframe();
        if (!oFrm)
            return;
        if (!e)
            var e = oFrm.event;
        if (!e)
            return;
        //Touche échap : on quitte ferme modale :
        if (e.keyCode == 27) {
            if (that.closeOnEscKey > 0) {
                var oModalClose = function () {
                    if (typeof (docEventKey) !== "undefined")
                        unsetEventListener(docEventKey, "keydown", KeyPressNativeFunction, false);

                    that.hide();

                    return false;
                };

                // Avec confirmation
                if (that.unsavedChanges && that.closeOnEscKey == 1) {
                    if (!that.confirmChangesModalDisplayed) {
                        // On déclenche la sortie de curseur sur les champs de saisie pour éviter les conflits liés au setWait (quitte à enregistrer les modifications effectuées)
                        if (document.activeElement)
                            document.activeElement.blur();
                        // #60 193 - Egalement sur la fenêtre réellement active, et pas uniquement sur le document racine (top)
                        if (oFrm.document.activeElement)
                            oFrm.document.activeElement.blur();

                        that.confirmChangesModalDisplayed = true;
                        return eAdvConfirm({
                            'criticity': 1,
                            'title': top._res_30,
                            'message': top._res_926,
                            'details': '',
                            'width': 500,
                            'height': 200,
                            'okFct': oModalClose,
                            'cancelFct': function () { that.confirmChangesModalDisplayed = false; },
                            'bOkGreen': false,
                            'bHtml': false,
                            'resOk': top._res_30,
                            'resCancel': top._res_29
                        });
                    }
                    else
                        return;
                }
                // Sans confirmation
                else
                    oModalClose(that, docEventKey, KeyPressNativeFunction);
            }
        }
        else {
            // On vérifie si la touche appuyée correspond à un caractère "imprimable" et non à une touche système/de fonction
            if (isWritableCharCode(e.keyCode))
                that.unsavedChanges = true; // on indique que l'utilisateur a saisi des données susceptibles d'être perdues à la fermeture de la fenêtre
            ScanString(e.keyCode);
        }
    }
    ///summary
    ///Parcours la chaine str et la compare au mot de passe paramétré
    ///pour afficher le son secret
    ///summary
    function ScanString(keyPress) {
        try {


        }
        catch (ex) {

        }
    }



    this.loadPage = function () {
        //Envoi de l'id de l'iframe
        ednuPopup.addParam("_parentiframeid", "frm_" + uId, "post");
        //TODO en cas de besoin - Passer le nom de la variable - ednuPopup.addParam("_parentmodalvarname", modalvarname);

        // Mise à jour de la variable éventuellement passée en POST pour que la nouvelle taille soit connue de la page affichée par la Modal Dialog
        this.setParam("width", Math.round(this.absWidth), "post");
        this.setParam("height", Math.round(this.absHeight), "post");
        this.setParam("divMainWidth", this.getDivMainWidth(), "post");
        this.setParam("divMainHeight", this.getDivMainHeight(), "post");

        if (ModalType.ToolTip.toString() == type.toString()) {
            this.InitToolTipDivContener();
        }

        if (typeof (this.ErrorCallBack) == 'function')
            ednuPopup.ErrorCallBack = this.ErrorCallBack;

        if (typeof (this.ErrorCustomAlert) == 'function')
            ednuPopup.ErrorCustomAlert = this.ErrorCustomAlert;

        if (this.url == "blank") {
            this.createIframe("loading");
        }
        else
            ednuPopup.send(this.createIframe, null);
    };

    this.addParam = function (pName, pValue, pMethod) {
        if (ednuPopup)
            ednuPopup.addParam(pName, pValue, pMethod);
    };

    // Recupération d'un paramètre posté
    this.getParam = function (name) {
        if (ednuPopup)
            return ednuPopup.getParam(name);
    };

    // Recupération d'un paramètre posté
    this.setParam = function (name, newValue, requestType) {
        ednuPopup.setParam(name, newValue, requestType);
    };

    this.createBackGround = function () {
        //Div de background
        backgroundDiv = document.createElement('div');
        backgroundDiv.id = "Bg_" + uId;
        backgroundDiv.className = "BackgroundModal";
        backgroundDiv.style.position = 'absolute';
        backgroundDiv.style.left = 0 + "px";
        backgroundDiv.style.top = "-13px";
        // #58 123 - remplacement du 100% par la dimension absolue totale du document (scroll compris)
        // Permet d'afficher le calque sur toute la surface et non pas seulement sur la surface visible avant scroll
        backgroundDiv.style.width = this.maxDocWidth + "px";
        backgroundDiv.style.height = this.maxDocHeight + "px";
        backgroundDiv.style.backgroundColor = "gray";

        backgroundDiv.style.opacity = (30 / 100);
        backgroundDiv.style.MozOpacity = (30 / 100);
        backgroundDiv.style.KhtmlOpacity = (30 / 100);
        backgroundDiv.style.filter = "alpha(opacity=" + 30 + ")";
        backgroundDiv.style.zIndex = this.level;
        document.body.appendChild(backgroundDiv);
        oldoverflow = document.body.style.overflow;
        document.body.style.overflow = "hidden";
    };

    this.createDivContainer = function (docWidth, docHeight, pLeft, pTop, formularType) {
        this.trace("Création du conteneur général, gauche : " + pLeft + ", haut : " + pTop + ", largeur du document : " + docWidth + ", hauteur : " + docHeight);

        //DivContainer
        // var font = document.getElementById("gw-container").classList[0]
        var fsize = 8;
        var oeParam = getParamWindow();

        if (oeParam && typeof (oeParam.GetParam) == 'function')
            fsize = oeParam.GetParam('fontsize');

        fsize = "fs_" + fsize + "pt";
        containerDiv = document.createElement('div');
        containerDiv.id = "ContainerModal_" + uId;
        containerDiv.className = "ContainerModal openTransition " + fsize;
        containerDiv.style.position = 'fixed'; //ALISTER Demande/Request 93 178 => fixed, pour le fixer au milieu de l'écran / fixed, to fix at the center of the screen
        containerDiv.style.left = pLeft + "px";
        containerDiv.style.top = pTop + "px";
        containerDiv.style.width = (formularType == 1) ? "100%" : this.absWidth + "px";
        containerDiv.style.height = (formularType == 1) ? "100%" : this.absHeight + "px";
        containerDiv.style.zIndex = this.level;
        containerDiv.style.left = '390px';
        containerDiv.style.top = '205px';
        containerDiv.style.width = '500px';
        containerDiv.style.height = "200px";
        //containerDiv.style.opacity = 0;
        //containerDiv.style.filter = 'alpha(opacity = 0)';

        document.body.appendChild(containerDiv);
    };

    //TOOLTIP - Permet de créer le div conteneur de la modal
    this.createDivContainerToolTip = function (docWidth, docHeight, pLeft, pTop) {
        this.trace("Création de l'infobulle du conteneur général, gauche : " + pLeft + ", haut : " + pTop + ", largeur du document : " + docWidth + ", hauteur : " + docHeight);

        //DivContainer
        containerDiv = document.createElement('div');
        containerDiv.id = "ContainerModal_" + uId;
        //containerDiv.className = "ContainerModal";
        containerDiv.style.position = 'absolute';
        containerDiv.style.left = pLeft + "px";
        containerDiv.style.top = pTop + "px";
        containerDiv.style.width = this.absWidth + "px";
        containerDiv.style.height = this.absHeight + "px";
        containerDiv.style.zIndex = this.level;
        //containerDiv.style.opacity = 0;
        //containerDiv.style.filter = 'alpha(opacity = 0)';

        top.document.body.appendChild(containerDiv);
    };

    this.createDivTitle = function (docWidth, docHeight, pLeft, pTop) {
        this.trace("Création de la barre de titre, gauche : " + pLeft + ", haut : " + pTop + ", largeur du document : " + docWidth + ", hauteur : " + docHeight);

        if (that.title == null || that.title == "")
            that.title = "&nbsp;";
        else {

            if (typeof (encodeHTMLEntities) == "function")
                that.title = encodeHTMLEntities(that.title);
        }


        this.noTitle = false;

        //DivTitle
        titleDiv = document.createElement('div');
        titleDiv.id = "TitleModal_" + uId;
        //titleDiv.className = "TitleModal";
        titleDiv.style.position = 'absolute';
        titleDiv.style.left = "0px";
        titleDiv.style.top = "-5px";
        titleDiv.style.width = "100%"; // width + "px";
        titleDiv.style.height = "100%";// 26 + "px";
        var strImgControlBoxMaximize = '';
        var strImgControlBoxClose = '';

        if (!this.hideMaximizeButton) {
            strImgControlBoxMaximize = "<div class='icon-maximize' id='ImgControlBox_" + uId + "'></div>";
        }
        if (!this.hideCloseButton) {
            strImgControlBoxClose = "<div class='icon-edn-cross' id='ImgControlBoxClose_" + uId + "'></div>";
        }
        titleDiv.innerHTML = "<table class='tbHtileModal' cellspacing=0 cellpadding=0>"
            + "<tr>"
            + "<td id='td_title_" + uId + "' class='TitleModal' onmousedown='doOnmouseDownModal(event);'>" + that.title + "&nbsp;" + (this.debugMode ? uId : "") + "</td>"
            + "<td id='td_ctrl_" + uId + "' class='ControlBox' align=right>"
            + strImgControlBoxClose
            + strImgControlBoxMaximize
            + "</td>"
            + "</tr>"
            + "</table>";
        titleDiv.style.opacity = 1;
        titleDiv.style.MozOpacity = 1;
        titleDiv.style.KhtmlOpacity = 1;


        containerDiv.appendChild(titleDiv);

        if (this.IconClass != "") {
            var oTitleTd = top.document.getElementById("td_title_" + uId);
            oTitleTd.className = oTitleTd.className + " " + this.IconClass;
        }

        if (this.type == "0" && !this.hideMaximizeButton) {
            document.getElementById("ImgControlBox_" + uId).onclick = this.MaxOrMinModal;
        }

        if (!this.hideCloseButton) {
            document.getElementById("ImgControlBoxClose_" + uId).onclick = this.hide;
        }

    };

    this.getDivMainWidth = function (customWidth) {
        this.trace("Récupération de la largeur du conteneur principal (paramètre : " + customWidth + ")");

        var mainDivWidth = (typeof (customWidth) == 'undefined' || customWidth == null) ? this.absWidth : customWidth;

        this.trace("Largeur renvoyée : " + mainDivWidth);

        return mainDivWidth;
    };

    this.getDivMainHeight = function (customHeight) {
        this.trace("Récupération de la hauteur du conteneur principal (paramètre : " + customHeight + ")");

        var mainDivHeight = (typeof (customHeight) == 'undefined' || customHeight == null) ? this.absHeight : customHeight;

        // Si bouton, le main ne prend pas les 50px
        if (this.noButtons != true)
            mainDivHeight -= 50;        // taille buttonsDiv (50)

        // Si titre, le main ne prend pas les 20px
        if (this.noTitle != true)
            mainDivHeight -= 20;        // taille titleDiv (20)

        // Si barre d'outils, le main ne prend pas les 35px
        if (this.noToolbar != true)
            mainDivHeight -= 35;        // taille toolbarDiv (35

        this.trace("Hauteur renvoyée : " + mainDivHeight);

        return mainDivHeight;
    };


    this.getDivMain = function () {
        return mainDiv;
    };


    this.createDivMain = function (docWidth, docHeight, pLeft, pTop) {
        this.trace("Création du conteneur principal, gauche : " + pLeft + ", haut : " + pTop + ", largeur du document : " + docWidth + ", hauteur : " + docHeight);

        //Div principal
        mainDiv = document.createElement('div');

        mainDiv.id = "MainModal_" + uId;
        mainDiv.className = "MainModal";
        mainDiv.style.position = 'absolute';
        // mainDiv.style.overFlow = 'absolute';
        mainDiv.style.overflowX = 'hidden';
        if (this.NoScrollOnMainDiv)
            mainDiv.style.overflowY = 'hidden';

        var nTopOffset = 0;
        if (this.noTitle != true) { nTopOffset += 20; }
        if (this.noToolbar != true) { nTopOffset += 35; }

        mainDiv.style.left = 0 + "px";
        mainDiv.style.top = nTopOffset + "px"; // offset par rapport à titleDiv
        mainDiv.style.width = "100%"; // width + "px";

        mainDiv.style.height = "100px"; // 70 = taille titleDiv (20) + taille buttonsDiv (50)

        mainDiv.style.opacity = 1;
        mainDiv.style.MozOpacity = 1;
        mainDiv.style.KhtmlOpacity = 1;

        //Creation du contenu
        switch (type.toString()) {
            //Page classique en popup               
            case ModalType.ModalPage.toString():
                this.loadPage();
                break;
            //Messagebox local // avec icone               
            case ModalType.MsgBoxLocal.toString():
                this.createLocalMsgBox();
                break;
            //Messagebox local // avec icone               
            case ModalType.Prompt.toString():
                this.createPrompt();
                break;
            case ModalType.Waiting.toString():
                this.createWaiting();
                break;
            case ModalType.ProgressBar.toString():
                this.createProgressBar();
                break;
            case ModalType.ToolTip.toString():
                this.loadPage();
                break;
            case ModalType.VCard.toString():
                mainDiv.style.top = 0;
                containerDiv.className = "";
                mainDiv.className = "VCardTT openTransition";
                this.loadPage();
                break;
            case ModalType.ToolTipSync.toString():
                //this.loadPage();
                //TODO
                break;
            case ModalType.ColorPicker.toString():
                this.createColorPicker();
                break;
            case ModalType.SelectList.toString():
                this.createSelectList();
                break;
            case ModalType.DisplayContent.toString():
                this.displayContent();
                break;
            case ModalType.DisplayContentWithoutTitle.toString():
                this.displayContent();
                break;
        }

        // Backlog #1659 - Ajout du type de modal en attribut pour le ciblage sur les nouveaux thèmes
        // Au même titre que cette containerDiv contient le msgtype (WARNING, SUCCESS...) pour les modal dialogs de type MsgBoxLocal (2)
        setAttributeValue(containerDiv, "modaltype", type);

        containerDiv.appendChild(mainDiv);

    };


    this.targetPicker = null;
    this.targetPickerText = null;
    this.targetPickerOnChange = null;
    this.srcFrameId = null;
    this.bgColor = null;


    this.createColorPicker = function () {

        var tr, td;

        var colors = "#1785bf;#2d9662;#f1a504;#910101;#553399;#535151;" +
            "#42a7dc;#3fa371;#f9b21a;#bb1515;#800080;#6e6c6c;" +
            "#69c0ee;#69bf69;#f9b931;#dc4646;#954cce;#878585;" +
            "#90d1f3;#8bd48b;#fdce69;#e86c6c;#c48af2;#9a9898;" +
            "#a9ddf8;#a7dfa7;#fbd75b;#f09494;#dbadff;#bbb8b8;" +
            "#c1e8fc;#c4e9c4;#ffec86;#f8c4c4;#e9cdff;#d1d0d0;" +
            "#ceedfd;#daf2da;#fff1b0;#ffe5e5;#f0ddfe;#e7e4e4;" +
            "#e4f8f8;#e8f6e8;#fef6d2;#fceded;#f6ebfe;#efefef";

        var aCol = colors.split(';');

        var tabColors = top.document.createElement("table");
        tabColors.className = "tabColors";
        mainDiv.appendChild(tabColors);

        var nColByLine = 6;

        // Bouton "Couleur par défaut"
        tr = top.document.createElement("tr");
        tabColors.appendChild(tr);
        td = top.document.createElement("td");;
        td.setAttribute("colspan", nColByLine);
        td.className = "btnDefaultColor";
        SetText(td, top._res_7975);
        td.onclick = function () {
            that.targetPicker.style.backgroundColor = "";
            that.targetPicker.setAttribute("value", "");
            if (that.targetPickerText) {
                that.targetPickerText.value = "";
                if (that.targetPickerText.onchange)
                    that.targetPickerText.onchange();
                if (that.targetPickerOnChange && typeof that.targetPickerOnChange === 'function')
                    that.targetPickerOnChange();
            }
            that.hide();
        }

        tr.appendChild(td);

        for (var i = 0; i < aCol.length / nColByLine; i++) {
            tr = top.document.createElement("tr");
            tabColors.appendChild(tr);
            for (var j = 0; j < nColByLine; j++) {

                var sClass = "PersColor";
                var colIdx = i * nColByLine + j;
                if (colIdx > aCol.length)
                    return;
                td = top.document.createElement("td");;
                tr.appendChild(td);

                if (aCol[colIdx] != "") {
                    if (aCol[colIdx] == that.targetPicker.getAttribute("value"))
                        sClass = "selectedColCell";
                    var divCol = top.document.createElement("div");
                    td.appendChild(divCol);
                    divCol.className = sClass;
                    divCol.setAttribute("value", aCol[colIdx]);
                    divCol.setAttribute("title", aCol[colIdx]);
                    divCol.style.backgroundColor = aCol[colIdx];

                    var myFct = (function (a) {
                        return function () {
                            that.targetPicker.style.backgroundColor = aCol[a];
                            that.targetPicker.setAttribute("value", aCol[a]);
                            if (that.targetPickerText) {
                                that.targetPickerText.value = aCol[a];
                                if (that.targetPickerText.onchange)
                                    that.targetPickerText.onchange();
                                if (that.targetPickerOnChange && typeof that.targetPickerOnChange === 'function')
                                    that.targetPickerOnChange();
                            }
                            that.hide();
                        };
                    })(colIdx);

                    divCol.onclick = myFct;
                }
            }

        }
    };

    //Appelcontenu
    this.createDivMainToolTip = function (docWidth, docHeight, pLeft, pTop, pContainerDiv) {

        //Div principal
        mainDiv = top.document.createElement('div');
        mainDiv.id = "MainModal_" + uId;
        mainDiv.className = "MainModal";
        //mainDiv.style.position = 'absolute';
        // mainDiv.style.overFlow = 'absolute';
        mainDiv.style.overflowX = 'hidden';
        mainDiv.style.overflowY = 'auto';


        //        mainDiv.style.left = 0 + "px";
        //        mainDiv.style.top = 20 + "px"; // offset par rapport à titleDiv
        mainDiv.style.width = "100%"; // width + "px";

        mainDiv.style.height = "100px";

        mainDiv.style.opacity = 1;
        mainDiv.style.MozOpacity = 1;
        mainDiv.style.KhtmlOpacity = 1;
        //Creation du contenu
        switch (type.toString()) {
            //Page classique en popup               
            case ModalType.ModalPage.toString():
                this.loadPage();
                break;
            //Messagebox local // avec icone               
            case ModalType.MsgBoxLocal.toString():
                this.createLocalMsgBox();
                break;
            //Messagebox local // avec icone               
            case ModalType.Prompt.toString():
                this.createPrompt();
                break;
            case ModalType.Waiting.toString():
                this.createWaiting();
                break;
            case ModalType.ProgressBar.toString():
                this.createProgressBar();
                break;
            case ModalType.ToolTip.toString():
                this.loadPage();
                break;
            case ModalType.VCard.toString():
                this.loadPage();
                break;
            case ModalType.ToolTip.toString():
                this.loadPage();
                break;
        }
        if (pContainerDiv != null)
            pContainerDiv.appendChild(mainDiv);
        else
            containerDiv.appendChild(mainDiv);

    };

    this.createWaiting = function () {
        var mainMsgDiv = top.document.createElement("div");
        mainDiv.appendChild(mainMsgDiv);
        //Div logo
        var divLogo = top.document.createElement("div");
        divLogo.className = "waiting-image";
        mainMsgDiv.appendChild(divLogo);
        //Div message
        var divMsg = top.document.createElement("div");
        divMsg.id = "waiting-message";
        divMsg.className = "text-alert-info";
        mainMsgDiv.appendChild(divMsg);

    }

    var divProgressContainer = null;
    var spanProgressText = null;
    var divProgress = null;

    this.createProgressBar = function () {
        divMainProgressContainer = top.document.createElement("div");
        mainDiv.appendChild(divMainProgressContainer);
        divMainProgressContainer.className = "divMainProgressContainer";

        divProgressContainer = top.document.createElement("div");
        divMainProgressContainer.appendChild(divProgressContainer);
        divProgressContainer.className = "divProgressContainer";

        divProgress = top.document.createElement("div");
        divProgress.className = "divProgress";
        divProgressContainer.appendChild(divProgress);

        spanProgressText = top.document.createElement("span");
        spanProgressText.className = "spanProgressText";
        spanProgressText.innerHTML = "&nbsp;";
        divProgressContainer.appendChild(spanProgressText);

        divMainProgressContainer.style.marginTop = (getNumber(mainDiv.style.height.replace('px', '')) / 2 - getNumber(top.getCssSelector("eModalDialog.css", ".divProgressContainer").style.height.replace('px', '')) / 2) + 'px';
    };

    this.updateProgressBar = function (val) {
        divProgress.style.width = val + "%";
        spanProgressText.innerHTML = val + "%...";
    };

    this.createDivButtons = function (docWidth, docHeight, pLeft, pTop) {
        //Ajout du div de redimentionnement
        if (this.noButtons == true)
            return;

        //Div Boutons
        buttonsDiv = document.createElement('div');
        buttonsDiv.id = "ButtonModal_" + uId;
        buttonsDiv.className = "ButtonModal";
        buttonsDiv.style.position = 'absolute';
        buttonsDiv.style.left = 0 + "px";
        buttonsDiv.style.top = "120px";//TODO: rendre letop calculé
        buttonsDiv.style.width = "100%";
        buttonsDiv.style.height = 50 + "px";
        /*  buttonsDiv.style.paddingTop = "10px"; */ /* - Pierre - Je me suis permis de l'enlever car elle était de trop lors de mes ajustements après avoir rajouté un border sur tout les modals */

        var table = document.createElement("table");
        table.setAttribute("width", "100%");
        table.setAttribute("cellpadding", "0");
        table.setAttribute("cellspacing", "0");

        var tBody = document.createElement("tbody");

        var tr = document.createElement("tr");

        tdButtonsLeft = document.createElement("td");
        tdButtonsLeft.id = "tdButtonsLeft";
        tr.appendChild(tdButtonsLeft);

        tdButtonsMid = document.createElement("td");
        tdButtonsMid.id = "tdButtonsMid";
        tr.appendChild(tdButtonsMid);

        tdButtons = document.createElement("td");
        tdButtons.id = "tdButtons";
        tr.appendChild(tdButtons);

        var tdGhost = document.createElement("td");
        tdGhost.innerHTML = "&nbsp;";
        if (this.btnSpacerClass) {
            tdGhost.setAttribute("class", this.btnSpacerClass);
        }
        else {
            if (this.bBtnAdvanced)
                tdGhost.setAttribute("class", "actBtnLstAdv");
            else
                tdGhost.setAttribute("class", "actBtnLst");
        }
        tr.appendChild(tdGhost);

        tBody.appendChild(tr)
        table.appendChild(tBody);
        buttonsDiv.appendChild(table);
        containerDiv.appendChild(buttonsDiv);
    };

    this.createDivToolbar = function (docWidth, docHeight, pLeft, pTop) {
        if (this.noToolbar == true)
            return;

        //Div barre d'outils gauche
        toolbarDivLeft = top.document.createElement('div');
        toolbarDivLeft.id = "ToolbarModalLeft_" + uId;
        toolbarDivLeft.className = "ToolbarModal ToolbarModalLeft";
        toolbarDivLeft.style.position = 'absolute';
        toolbarDivLeft.style.left = 0 + "px";
        toolbarDivLeft.style.top = (this.noTitle ? 0 : 20) + "px";
        toolbarDivLeft.style.width = "50%";
        toolbarDivLeft.style.height = 35 + "px";

        //Div barre d'outils droite
        toolbarDivRight = top.document.createElement('div');
        toolbarDivRight.id = "ToolbarModalRight_" + uId;
        toolbarDivRight.className = "ToolbarModal ToolbarModalRight";
        toolbarDivRight.style.position = 'absolute';
        toolbarDivRight.style.right = 0 + "px";
        toolbarDivRight.style.top = (this.noTitle ? 0 : 20) + "px";
        toolbarDivRight.style.width = "50%";
        toolbarDivRight.style.height = 35 + "px";

        var ul = top.document.createElement("ul");
        ul.className = "ToolbarModal";

        ulToolbarLeft = top.document.createElement("ul");
        ulToolbarLeft.className = "ToolbarModal ToolbarModalLeft";
        ulToolbarLeft.id = "ulToolbarButtons";
        toolbarDivLeft.appendChild(ulToolbarLeft);

        ulToolbarRight = top.document.createElement("ul");
        ulToolbarRight.className = "ToolbarModal ToolbarModalRight";
        ulToolbarRight.id = "ulToolbarButtons";
        toolbarDivRight.appendChild(ulToolbarRight);

        containerDiv.appendChild(toolbarDivLeft);
        containerDiv.appendChild(toolbarDivRight);
    };

    //TOOLTIP - Permet de créer le div conteneur de boutons en bas de la modal
    this.createDivButtonsToolTip = function (docWidth, docHeight, pLeft, pTop) {
        if (this.noButtons == true)
            return;

        //Div Boutons
        buttonsDiv = top.document.createElement('div');

        buttonsDiv.id = "ButtonModal_" + uId;
        buttonsDiv.className = "ButtonModal";
        buttonsDiv.style.position = 'absolute';
        buttonsDiv.style.left = 0 + "px";
        buttonsDiv.style.top = (this.absHeight - 30) + "px";
        buttonsDiv.style.width = "100%";
        buttonsDiv.style.height = 30 + "px";

        var table = top.document.createElement("table");
        table.setAttribute("width", "100%");
        table.setAttribute("cellpadding", "0");
        table.setAttribute("cellspacing", "0");

        var tBody = top.document.createElement("tbody");

        var tr = top.document.createElement("tr");

        tdButtons = top.document.createElement("td");
        tdButtons.id = "tdButtons";
        tdButtons.setAttribute("width", "90%");
        tr.appendChild(tdButtons);

        var td2 = top.document.createElement("td");

        tr.appendChild(td2);

        tBody.appendChild(tr)
        table.appendChild(tBody);
        buttonsDiv.appendChild(table);
        containerDiv.appendChild(buttonsDiv);
    };

    var SelectionObjet = function (lbl, value, checked) {
        this.Label = lbl;
        this.Value = value;
        this.Checked = checked;
    };

    this.selList = new Array();
    this.titleSelectOption = "";

    this.addSelectOption = function (lbl, value, checked) {

        // lbl = encodeHTMLEntities(lbl);

        this.selList.push(new SelectionObjet(lbl, value, checked));
    };

    //Retourne la liste des value des valeurs sélectionnées, séparés par des ;
    this.getSelectedValue = function () {
        var ret = "";
        for (var i = 0; i < this.selList.length; i++) {
            var radio = top.document.getElementById("radio_" + i);
            if (this.isMultiSelectList) {
                if (radio.getAttribute("chk") == "1") {
                    if (ret != "")
                        ret += ";";
                    ret += radio.getAttribute("value");
                }
            }
            else {
                if (radio.checked) {
                    ret = radio.value;
                    break;
                }
            }
        }
        return ret;
    };
    //Retourne un tableau de valeurs sélectionné,
    //  chaque entrée du tableau étant un objet composé de :
    //      - lib étant le libellé de la case sélectionné
    //      - val étant la value de la case sélectionné
    this.getSelected = function () {
        var tabRet = new Array();
        for (var i = 0; i < this.selList.length; i++) {
            var radio = top.document.getElementById("radio_" + i);
            var obj = new Object();
            if (this.isMultiSelectList) {
                obj.lib = GetText(radio);
                obj.val = radio.getAttribute("value");
                if (radio.getAttribute("chk") == "1") {
                    tabRet.push(obj);
                }
            }
            else {
                obj.lib = GetText(top.document.getElementById("lib_" + i));
                obj.val = radio.value;
                if (radio.checked) {
                    tabRet.push(obj);
                    break;
                }
            }
        }
        return tabRet;
    };

    this.createSelectList = function () {
        var divTitle = top.document.createElement("div");
        divTitle.setAttribute("class", "divListTitle");
        divTitle.innerHTML = this.titleSelectOption;
        mainDiv.appendChild(divTitle);
        this.createSelectListCheckOpt();
    };

    //Génère le rendu des case à cocher demandée
    this.createSelectListCheckOpt = function () {
        var divList = top.document.createElement("div");
        divList.setAttribute("class", "divListChoice");
        if (type.toString() == ModalType.SelectList.toString() || (tdMsgDetails == null))
            mainDiv.appendChild(divList);
        else
            tdMsgDetails.appendChild(divList);

        for (var i = 0; i < this.selList.length; i++) {
            var divSelect = top.document.createElement("div");
            divSelect.setAttribute("class", "divSelectRadio");

            var value = this.selList[i].Value;
            var label = this.selList[i].Label;
            var checked = this.selList[i].Checked;
            var radio = null;
            if (this.isMultiSelectList) {
                var attributes = new Dictionary();
                attributes.Add("value", value);
                attributes.Add("name", "SelectionList");
                radio = AddEudoCheckBox(top.document, divSelect, checked, "radio_" + i, label, attributes);
            }
            else {
                radio = top.document.createElement("input");
                radio.id = "radio_" + i;
                radio.setAttribute("type", "radio");
                if (checked == true) {
                    radio.setAttribute("checked", "checked");
                }
                radio.setAttribute("value", value);
                radio.setAttribute("name", "SelectionList");
                divSelect.appendChild(radio);

                var span = top.document.createElement("label");
                span.id = "lib_" + i;
                span.setAttribute("for", "radio_" + i);
                span.innerHTML = label;
                divSelect.appendChild(span);
            }

            divList.appendChild(divSelect);
        }
    };

    //Ajuste la Hauteur de la modale au contenu de la div principale.
    //  nBottomMargin : si l'on souhaite avoir une marge en bas de la liste
    this.adjustModalToContent = function (nBottomMargin) {
        if (!nBottomMargin)
            nBottomMargin = 0;
        var oPos = getAbsolutePosition(mainDiv);
        var divHeight = oPos.h;
        var divWidth = oPos.w;
        var divScrollHeight = mainDiv.scrollHeight;
        var divScrollWidth = mainDiv.scrollWidth;
        if (divScrollHeight > divHeight || divScrollWidth > divWidth) {
            // TODO - REVOIR l'utilisation de height et width, on devrai pas s'appuyer sur absHeight et absWidth
            var newHeight = getNumber(this.height) + (divScrollHeight - divHeight) + nBottomMargin;
            if (newHeight > top.document.body.offsetHeight)
                newHeight = top.document.body.offsetHeight - 10;

            var newWidth = getNumber(this.width) + (divScrollWidth - divWidth)
            if (newWidth > top.document.body.offsetWidth)
                newWidth = top.document.body.offsetWidth;

            this.resizeTo(newWidth, newHeight);
        }
    }

    //Ajuste la Hauteur de la modale au contenu de l'iframe.
    // doit être appelé après le chargement de l'iframe
    this.adjustModalToContentIframe = function (nOffset) {
        if (typeof (nOffset) != "number")
            nOffset = 0;

        oFrame = that.getIframeTag();

        var oBody = oFrame.contentWindow.document.body;
        var oHtml = oFrame.contentWindow.document.documentElement;

        var height = Math.max(oBody.scrollHeight, oBody.offsetHeight, oHtml.clientHeight, oHtml.scrollHeight, oHtml.offsetHeight);
        oFrame.style.height = (height + nOffset) + "px";
    };

    // Taille de départ : 90% de la fenêtre
    this.forceWindowMaxSize = function () {
        // On désactive l'agrandissement de la fenêtre car on souhait qu'elle prenne toute la taille de l'écran
        that.hideMaximizeButton = true;

        // Détuit de la taille de la fenetre la marge*2 pour chaque côtés
        that.width = (that.docWidth - that.marginDialogMaxSize * 2) + '';
        that.height = (that.docHeight - that.marginDialogMaxSize * 2) + '';

        that.trace("Force les dimensions de la modal dialog au max - width : " + that.width + ", height : " + that.height + ", margin : " + that.marginDialogMaxSize);

        // On relance les calcules sur les dimensions abs
        that.initAbsSize();
    };

    this.show = function (posLeft, posTop, bLeftOrRight, formularType) {

        if (!this.ModalDialogs)
            this.ModalDialogs = [];

        this.ModalDialogs[this.UID] = this;

        this.trace("Affichage de la modal dialog - posLeft : " + posLeft + ", posTop : " + posTop + ", bLeftOrRight : " + bLeftOrRight);

        this.initSize();

        this.trace("Affichage de la modal dialog - Largeur de la fenêtre : " + this.absWidth + ", hauteur : " + this.absHeight);
        this.trace("Affichage de la modal dialog - Largeur du document : " + this.docWidth + ", hauteur : " + this.docHeight);

        // #58 123 (remplace #33 929) - positionnement de la fenêtre en tenant compte de la position du scroll sur la page racine
        // Permet d'afficher les popups au bon endroit lorsqu'on affiche une page avec scroll (ex : formulaire affiché par la page externe en dehors d'eMain.aspx)
        // Devrait également concerner les tablettes lorsque celles-ci affichent du contenu zoomé (deux doigts), d'où la refactorisation du correctif #33 929 dans initSize()
        var pLeft = ((typeof (posLeft) == "undefined") ? ((this.docWidth - this.absWidth) / 2) + this.topScrollWidth : posLeft);
        var pTop = ((typeof (posTop) == "undefined") ? ((this.docHeight - this.absHeight) / 2) + this.topScrollHeight : posTop);

        this.trace("Affichage de la modal dialog - Position recalculée : gauche " + pLeft + ", haut " + pTop);

        if (pTop < 0)
            pTop = -pTop;

        if (type.toString() == ModalType.VCard.toString()) {
            this.createDivContainer(this.docWidth, this.docHeight, pLeft, pTop);
            this.createDivMain(this.docWidth, this.docHeight, pLeft, pTop);
        }
        else if ((type.toString() != ModalType.ToolTip.toString()) && (type.toString() != ModalType.ToolTipSync.toString())) {
            //Creation des divs
            this.createBackGround();
            this.createDivContainer(this.docWidth, this.docHeight, pLeft, pTop, formularType);
            if (type.toString() != ModalType.DisplayContentWithoutTitle.toString())
                this.createDivTitle(this.docWidth, this.docHeight, pLeft, pTop);
            this.createDivToolbar(this.docWidth, this.docHeight, pLeft, pTop);
            this.createDivMain(this.docWidth, this.docHeight, pLeft, pTop);
            this.createDivButtons(this.docWidth, this.docHeight, pLeft, pTop);
        }
        else if (type.toString() == ModalType.ToolTip.toString()) {  //SI TOOLTIP
            //Position
            if (bLeftOrRight) {
                pLeft = pLeft - this.absWidth;
                pLeft = pLeft - CONST_TOOLTIP_ARROW_WIDTH; //12 étant la largeur de la flèche à gauche de la tooltip
            }
            else
                pLeft = pLeft + CONST_TOOLTIP_ARROW_WIDTH; //12 étant la largeur de la flèche à gauche de la tooltip

            this.createToolTipBorder(this.docWidth, this.docHeight, pLeft, pTop, bLeftOrRight);
        }
        else {
            //Autres types
        }

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
            if (document.getElementById("InputPrompt_" + uId) != null)
                document.getElementById("InputPrompt_" + uId).focus();
        }


        if (this.openMaximized)
            this.MaxOrMinModal();
        //fadeThis(containerDiv.id, top.document);
    };

    //TOOLTIP - Permet de créer le pourtout le la tooltip (fleche à gauche ou droite et cadre)
    //bLeftOrRight correspond à la position de la fleche true = gauche et false = droite
    this.createToolTipBorder = function (docWidth, docHeight, pLeft, pTop, bLeftOrRight) {

        //MOU demande cf.21089 : carte de visite tronquée en bas
        var pTopArrow = pTop;
        if (pTop + this.absHeight > docHeight) {

            pTop = (docHeight - this.absHeight) - 9; //9px pour l espacement en bas 
        }
        pTopArrow = pTopArrow - pTop;


        this.createDivContainerToolTip(docWidth, docHeight, pLeft, pTop);

        var mainTab = top.document.createElement('table');
        mainTab.setAttribute("cellpadding", "0");
        mainTab.setAttribute("cellspacing", "0");
        mainTab.className = "tt_background_bulle";

        var mainTabTR = mainTab.appendChild(document.createElement("tr"));
        var mainTabTD;

        if (!bLeftOrRight) {
            /*FLECHE A DROITE*/
            mainTabTD = mainTabTR.appendChild(document.createElement("td"));
            mainTabTD.className = "tt_arrow_td";
            mainTabTD.style.verticalAlign = "Top";

            var flecheDiv = mainTabTD.appendChild(document.createElement("div"));
            flecheDiv.className = "tt_arrow_right";


            flecheDiv.style.top = pTopArrow + "px"; //MOU demande cf.21089
            flecheDiv.appendChild(document.createTextNode(" "));
            /********/
        }
        mainTabTD = mainTabTR.appendChild(document.createElement("td"));
        mainTabTD.style.verticalAlign = "Top";
        mainTabTD.className = "tt_mid_tab";
        this.createDivMainToolTip(docWidth, docHeight, pLeft, pTop, mainTabTD);

        if (bLeftOrRight) {
            /*FLECHE A GAUCHE*/
            mainTabTD = mainTabTR.appendChild(document.createElement("td"));
            mainTabTD.className = "tt_arrow_td";
            mainTabTD.style.verticalAlign = "Top";
            var flecheDiv = mainTabTD.appendChild(document.createElement("div"));
            flecheDiv.className = "tt_arrow_left";
            flecheDiv.style.top = pTopArrow + "px"; //MOU demande cf.21089
            flecheDiv.appendChild(document.createTextNode(" "));
            /********/
        }

        containerDiv.appendChild(mainTab);
    };

    this.getControl = function (sId) {
        return top.document.getElementById(sId);
    }

    this.getPromptValue = function () {
        var oPrompt = top.document.getElementById('InputPrompt_' + uId);
        if (oPrompt != null) {
            return oPrompt.value;
        }
    };

    this.getPromptIdTextBox = function () {
        var idInput = 'InputPrompt_' + uId;
        return idInput;
    }

    //Fonction d'encodage
    this.encode = function (strValue) {
        var strReturnValue;

        if (strValue == "" || strValue == null)
            return "";

        try {
            var strReturnValue = encodeURIComponent(strValue);
            strReturnValue = strReturnValue.replace(/'/g, "%27");
        }
        catch (e) {
            strReturnValue = escape(strValue);
        }
        return strReturnValue;
    };

    this.decode = function (strValue) {
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
    };
    this.resizeToMaxWidth = function () {
        var nDivMainWidth = that.getDivMainWidth(that.docWidth - (that.marginDialogMaxSize * 2));
        // Title
        if (!that.noTitle) {
            that.initialTitleWidth = titleDiv.style.width;
            titleDiv.style.width = nDivMainWidth + "px";
        }
        containerDiv.style.width = (that.docWidth - (that.marginDialogMaxSize * 2)) + "px";        // -20 pour le margin right et left de 10
        mainDiv.style.width = nDivMainWidth + "px";
        containerDiv.style.left = that.marginDialogMaxSize + "px";


    };
    this.MaxOrMinModal = function () {
        var resizeButton = top.document.getElementById("ImgControlBox_" + uId);
        var maximized = false;

        if (that.sizeStatus != "max")
        //Maximiser la fenêtre
        {
            maximized = true;
            that.initialContainerTop = containerDiv.style.top;
            that.initialContainerLeft = containerDiv.style.left;
            that.initialContainerWidth = containerDiv.style.width;
            that.initialContainerHeight = containerDiv.style.height;
            that.initialContainerMargin = containerDiv.style.margin;

            that.initialMainWidth = mainDiv.style.width;
            that.initialMainHeight = mainDiv.style.height;

            that.initSize();
            var nDivMainWidth = that.getDivMainWidth(that.docWidth - (that.marginDialogMaxSize * 2));
            var nDivMainHeight = that.getDivMainHeight(that.docHeight - (that.marginDialogMaxSize * 2));

            // Title
            if (!that.noTitle) {
                that.initialTitleWidth = titleDiv.style.width;
                titleDiv.style.width = nDivMainWidth + "px";
            }

            var nTitleOffsetTop = (that.noTitle ? 0 : 20);
            var nToolbarOffsetTop = (that.noToolbar ? 0 : 35);

            // Barre d'outils
            if (!that.noToolbar) {
                that.initialToolbarLeftWidth = toolbarDivLeft.style.width;
                that.initialToolbarRightWidth = toolbarDivRight.style.width;
                toolbarDivLeft.style.width = "50%";
                toolbarDivRight.style.width = "50%";
            }

            containerDiv.style.top = "0px";
            containerDiv.style.left = "0px";
            containerDiv.style.width = (that.docWidth - (that.marginDialogMaxSize * 2)) + "px";        // -20 pour le margin right et left de 10
            containerDiv.style.height = (that.docHeight - (that.marginDialogMaxSize * 2)) + "px";        // -20 pour le margin top et bottom de 10
            containerDiv.style.margin = that.marginDialogMaxSize + "px";
            mainDiv.style.width = nDivMainWidth + "px";
            mainDiv.style.height = nDivMainHeight + "px"

            // Btn
            if (!that.noButtons) {
                that.initialButtonsTop = buttonsDiv.style.top;
                that.initialButtonsWidth = buttonsDiv.style.width;
                buttonsDiv.style.width = "100%";
                var nTop = nTitleOffsetTop + nToolbarOffsetTop + nDivMainHeight;
                buttonsDiv.style.top = nTop + "px";
            }

            that.sizeStatus = "max";
            if (resizeButton)
                resizeButton.className = 'icon-restore';
        }
        else
        //Remise à la taille normale
        {
            maximized = false;
            containerDiv.style.top = that.initialContainerTop;
            containerDiv.style.left = that.initialContainerLeft;
            containerDiv.style.width = that.initialContainerWidth;
            containerDiv.style.height = that.initialContainerHeight;
            containerDiv.style.margin = that.initialContainerMargin;

            // Title
            if (!that.noTitle) {
                titleDiv.style.width = that.initialTitleWidth;
            }

            if (!that.noToolbar) {
                toolbarDivLeft.style.width = that.initialToolbarLeftWidth;
                toolbarDivRight.style.width = that.initialToolbarRightWidth;
            }

            mainDiv.style.width = that.initialMainWidth;
            mainDiv.style.height = that.initialMainHeight;

            if (!that.noButtons) {
                buttonsDiv.style.top = that.initialButtonsTop;
                buttonsDiv.style.width = that.initialButtonsWidth;
            }

            that.sizeStatus = "";
            if (resizeButton)
                resizeButton.className = 'icon-maximize';
        }

        //abonnement depuis l'iframe 
        if (top.document.getElementById("frm_" + uId) != null) {
            var frm = top.document.getElementById("frm_" + uId).contentWindow;
            if (frm.onFrameSizeChange != null) {
                var mWidth = mainDiv.offsetWidth;
                var mHeight = mainDiv.offsetHeight;
                frm.onFrameSizeChange(mWidth, mHeight * 0.98); // #48584 : On met la hauteur à 98% de la hauteur de la div pour correspondre aux 98% en CSS de l'iframe
            }
        }
    };

    this.moveTo = function (newLeft, newTop) {
        if (newLeft != null)
            containerDiv.style.left = newLeft + "px";

        if (newTop != null)
            containerDiv.style.top = newTop + "px";
    };

    //fait un decalage de la fenetre de deltaX et deltaY
    this.moveBy = function (deltaX, deltaY) {
        if (deltaX != null)
            containerDiv.style.left = (parseInt(containerDiv.style.left) + deltaX) + "px";

        if (deltaY != null)
            containerDiv.style.top = (parseInt(containerDiv.style.top) + deltaY) + "px";

        return this;
    };

    this.resizeTo = function (newWidth, newHeight) {
        this.initSize();

        if (newWidth != null) {
            var pLeft = (this.docWidth - newWidth) / 2;
            containerDiv.style.left = pLeft + "px";
            containerDiv.style.width = newWidth + "px";
            titleDiv.style.width = newWidth + "px";
            mainDiv.style.width = "100%";
        }

        if (newHeight != null) {
            var pTop = (getDocHeight() - newHeight) / 2;
            containerDiv.style.top = pTop + "px";
            containerDiv.style.height = newHeight + "px";
            mainDiv.style.height = that.getDivMainHeight(newHeight) + "px";
            if (mainMsgDiv)
                mainMsgDiv.style.height = mainDiv.style.height; //On ajuste le contenu de la confirme aussi
            if (buttonsDiv)
                buttonsDiv.style.top = (newHeight - 50) + "px";
        }

        //abonnement depuis l'iframe 
        if (top.document.getElementById("frm_" + uId) != null) {
            var frm = top.document.getElementById("frm_" + uId).contentWindow;
            if (frm.onFrameSizeChange != null) {
                var mWidth = mainDiv.offsetWidth;
                var mHeight = mainDiv.offsetHeight;
                frm.onFrameSizeChange(mWidth, mHeight);
            }
        }
    };

    this.tabLibelleToolTip = null;  //TOOLTIP   :   conteneur de libellés de détail (pour addLibelleToolTip)

    this.InitToolTipDivContener = function () {
        mainDivLibelleToolTip = mainDiv.appendChild(document.createElement('div'));
        //mainDivLibelleToolTip.className = "pl_tt_mid_tab pl_tt_fields";

        if (!this.noButtons) {
            mainDivLibelleToolTip.style.width = (this.absWidth - CONST_TOOLTIP_ARROW_WIDTH) + "px";
        }
        else {
            mainDivLibelleToolTip.style.overflowX = 'hidden';
            mainDivLibelleToolTip.style.overflowY = 'hidden';
            mainDivLibelleToolTip.style.height = "100%"; // 30 = taille titleDiv (30) 
            mainDivLibelleToolTip.style.width = "100%";
        }
    }

    //TOOLTIP - Ajoute une ligne de contenu dans une modal de type ToolTip
    this.addLibelleToolTip = function (key, value) {
        if (this.tabLibelleToolTip == null) {
            this.InitToolTipDivContener();
            this.tabLibelleToolTip = mainDivLibelleToolTip.appendChild(document.createElement('table'));
        }
        if (this.tabLibelleToolTip != null) {
            var mainTabTR = this.tabLibelleToolTip.appendChild(document.createElement("tr"));
            var mainTabTD = mainTabTR.appendChild(document.createElement("td"));
            mainTabTD.appendChild(document.createTextNode(key));
            mainTabTD = mainTabTR.appendChild(document.createElement("td"));
            mainTabTD.innerHTML = value;
        }
    };

    this.buttonTabTr = null;    //TOOLTIP   :   conteneur de boutons (pour addButtonToolTip)
    //TOOLTIP - Ajoute une ligne de contenu dans une modal de type ToolTip
    this.addButtonToolTip = function (toolTipTag, cssClass, jsAction) {
        if (this.buttonTabTr == null) {
            var buttonTab = mainDiv.appendChild(document.createElement('table'));
            buttonTab.setAttribute("cellpadding", "0");
            buttonTab.setAttribute("cellspacing", "0");
            buttonTab.className = "pl_tt_bot_icon";
            this.buttonTabTr = buttonTab.appendChild(document.createElement("tr"));
        }
        if (this.buttonTabTr != null) {
            var mainTabTD = this.buttonTabTr.appendChild(document.createElement("td"));
            mainTabTD.className = cssClass;
            mainTabTD.setAttribute("onclick", jsAction);
            mainTabTD.appendChild(document.createTextNode(" "));
        }
    };

    this.ToolbarButtonType =
    {
        PrintButton: 0,
        DeleteButton: 1,
        PropertiesButton: 2,
        MandatoryButton: 3,
        PjButton: 4,
        SendMailButton: 5,
        DeleteCalendarButton: 6,
        CancelLastValuesButton: 7
        /*TODO - Compléter en cas de besoin*/
    };

    // Crée tous les boutons de barre d'outils susceptibles d'être affichés sur les fichiers de type Template en popup (dont Planning)
    this.addTemplateButtons = function (nTab, fileid, isCalendar, openSerie) {
        // #54537 : Pas de "Propriétés de la fiche" pour la fiche Utilisateur
        if (nTab != 101000)
            this.addToolbarButton("ToolbarButton_" + this.ToolbarButtonType.PropertiesButton + "_" + this.iframeId, top._res_54, top._res_54, "iProp", "iProp", "iProp iconDef_" + nTab, "iProp", true, true, 'left', function () { that.onToolBarClick(that.ToolbarButtonType.PropertiesButton + "_" + this.iframeId, nTab, fileid); });

        this.addToolbarButton("ToolbarButton_" + this.ToolbarButtonType.MandatoryButton + "_" + this.iframeId, "* " + top._res_6304, top._res_6304, "iMnd", "iMnd", "", "iMnd", true, false, 'right', null);

        var btnCancel = this.addToolbarButton("ToolbarButton_" + this.ToolbarButtonType.CancelLastValuesButton + "_" + this.iframeId, " ", top._res_8223, "btnCancelLastModif", "icon-undo", "", "btnCancelLastModif", true, false, 'right', null);
        btnCancel.onmouseover = (function (oModalDialog) {
            return function () {
                var frame = oModalDialog.getIframe();
                if (frame)
                    frame.LastValuesManager.openContextMenu(this, nTab, frame.arrLastValues, new eContextMenu(frame.LastValuesManager.menuWidth, -999, -999, null, null, "lastvalues_contextmenu"), true);
            }

        })(this);

        this.addToolbarButton("ToolbarButton_" + this.ToolbarButtonType.PjButton + "_" + this.iframeId, "(0)", top._res_5042, "", null, "icon-annex", null, true, true, 'right', function () { that.onToolBarClick(that.ToolbarButtonType.PjButton + "_" + this.iframeId, nTab, fileid); });

        //Pas d'impression sur fiche en cours de créa
        if (typeof (fileid) != "undefined" && fileid)
            this.addToolbarButton("ToolbarButton_" + this.ToolbarButtonType.PrintButton + "_" + this.iframeId, top._res_13, top._res_13, "", null, "icon-print2", null, false, true, 'right', function () { that.onToolBarClick(that.ToolbarButtonType.PrintButton + "_" + this.iframeId, nTab, fileid); });

        // TODO #29 959 - Fonctionnalité "Envoi par e-mail" à coder
        //this.addToolbarButton("ToolbarButton_" + this.ToolbarButtonType.SendMailButton+ "_" + this.iframeId, top._res_1390, top._res_1390, "iMl", null, null, null, false, true, 'right', function () { that.onToolBarClick(that.ToolbarButtonType.SendMailButton+ "_" + this.iframeId, nTab, fileid); });

        if (typeof (isCalendar) == 'undefined')
            isCalendar = false;
        if (typeof (openSerie) == 'undefined')
            openSerie = false;

        if (this.CallFrom != CallFromDuplicate && this.fileId != 0) {
            if (isCalendar) {
                this.addToolbarButton("ToolbarButton_" + this.ToolbarButtonType.DeleteCalendarButton + "_" + this.iframeId, top._res_19, top._res_19, "", null, "icon-delete", null, false, true, 'right', function () { that.onToolBarClick(that.ToolbarButtonType.DeleteCalendarButton + "_" + this.iframeId, nTab, fileid, openSerie); });
            }
            else {
                this.addToolbarButton("ToolbarButton_" + this.ToolbarButtonType.DeleteButton + "_" + this.iframeId, top._res_19, top._res_19, "", null, "icon-delete", null, false, true, 'right', function () { that.onToolBarClick(that.ToolbarButtonType.DeleteButton + "_" + this.iframeId, nTab, fileid, false); });
            }
        }
    };

    this.onToolBarClick = function (btn, nTab, fileId, openSerie) {
        var frameModal = this.getIframe();
        var nBtn = parseInt(btn);
        switch (nBtn) {
            case this.ToolbarButtonType.PrintButton:
                if (typeof (frameModal.onPrintButton) == "function")
                    frameModal.onPrintButton(nTab, fileId);
                break;
            case this.ToolbarButtonType.DeleteButton:
                if (typeof (frameModal.onDeleteButton) == "function")
                    frameModal.onDeleteButton(nTab, fileId, this);
                break;
            case this.ToolbarButtonType.DeleteCalendarButton:
                if (typeof (frameModal.onDeleteCalendarButton) == "function")
                    frameModal.onDeleteCalendarButton(nTab, fileId, this, openSerie);
                break;
            case this.ToolbarButtonType.PropertiesButton:
                if (typeof (frameModal.onPropertiesButton) == "function")
                    frameModal.onPropertiesButton(nTab, fileId);
                break;
            case this.ToolbarButtonType.MandatoryButton:
                //Aucun traitement
                break;
            case this.ToolbarButtonType.PjButton:
                if (typeof (frameModal.onPjButton) == "function")
                    frameModal.onPjButton("ToolbarButton_" + this.ToolbarButtonType.PjButton + "_" + this.iframeId + "_text");
                break;
            case this.ToolbarButtonType.SendMailButton:
                if (typeof (frameModal.onSendMailButton) == "function")
                    frameModal.onSendMailButton('', nTab);
                break;


        }
    };

    this.setButtonLabel = function (nBtn, lbl) {
        if (document.getElementById("ToolbarButton_" + nBtn + "_" + this.iframeId + "_text"))
            document.getElementById("ToolbarButton_" + nBtn + "_" + this.iframeId + "_text").innerHTML = lbl;
    };

    this.setToolBarVisible = function (strButtons, bVisible) {
        strButtons = strButtons.toString();
        var aButtons = strButtons.split(';');

        //On affiche les boutons demandés
        for (var i = 0; i < aButtons.length; i++) {
            var strButton = aButtons[i];
            if (strButton && strButton != "") {
                var oToolBarBtn = top.document.getElementById("ToolbarButton_" + strButton + "_" + this.iframeId);
                if (oToolBarBtn) {
                    if (bVisible) {
                        oToolBarBtn.style.display = "block";
                        oToolBarBtn.style.visibility = "visible";
                    } else {
                        oToolBarBtn.style.display = "none";
                        oToolBarBtn.style.visibility = "hidden";
                    }
                }
            }
        }
    };

    ///summary
    ///Retourne la fenêtre modale parente de la fenêtre modale en cours, si existante
    ///On se base sur le tableau global référençant toutes les modal dialogs, pour vérifier si l'objet Window d'une modal dialog correspond, ou non, à celui stocké dans
    ///la propriété myOpenerWin de la modal dialog en cours. Auquel cas, on considère que la modale actuellement examinée est bien la parente de celle en cours
    ///summary
    this.getParentModalDialog = function () {
        try {
            if (!this.ModalDialogs)
                return null;

            for (var modalUID in this.ModalDialogs) {
                var modalDialog = this.ModalDialogs[modalUID];
                if (modalDialog && typeof (modalDialog.getIframe) != "undefined" && modalDialog.getIframe() == this.myOpenerWin) {
                    return modalDialog;
                }
            }

            return null;
        }
        catch (ex) {
            return null;
        }
    };
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

function doOnmouseDownModal(e) {
    return; //TODO:
    oldMouseMove = document.onmousemove;
    oldMouseUp = document.onmouseup;

    document.onmouseup = onmouseUpModal;
    document.onmousemove = onmouseMoveModal;

    if (e && e.target)
        modalId = e.target.id.replace("td_title_", "");
    else
        modalId = window.event.srcElement.id.replace("td_title_", "");

    divCtnr = document.getElementById("ContainerModal_" + modalId);
    // Mémorisation de l'opacité initiale de la fenêtre
    if (divCtnr && divCtnrInitOpacity == null)
        divCtnrInitOpacity = divCtnr.style.opacity;

    if (dashedDiv == null) {
        dashedDiv = document.createElement("div");
        dashedDiv.id = "dashedDiv";
        document.body.appendChild(dashedDiv);
    }

    if (divMove == null) {
        divMove = document.createElement("div");
        divMove.id = "divMove";
        document.body.appendChild(divMove);
    }

    dashedDiv.style.top = divCtnr.style.top;
    dashedDiv.style.left = divCtnr.style.left;
    dashedDiv.style.width = divCtnr.style.width;
    dashedDiv.style.height = divCtnr.style.height;
    dashedDiv.style.position = 'absolute';
    dashedDiv.style.border = '2px dashed gray';
    dashedDiv.style.display = "block";

    dashedDiv.style.zIndex = parseInt(divCtnr.style.zIndex) + 1;
    dashedDiv.style.cursor = "default";


    divMove.style.top = "0px";
    divMove.style.left = "0px";
    divMove.style.width = "100%";
    divMove.style.height = "100%";
    divMove.style.position = 'absolute';

    divMove.style.display = "block";
    divMove.style.zIndex = parseInt(divCtnr.style.zIndex) + 2;




    dragEnabled = true;

    if (!e)
        var e = window.event;
    x = e.clientX;
    y = e.clientY;

    //MOU on initilise la position de la souris
    if (xMouseMem == 0 && yMouseMem == 0) {
        //au premier click on sauvegarde les coordonnées de la souris.
        xMouseMem = x;
        yMouseMem = y;
    }

    // Bonus : Si on appuie sur Shift/Maj tout en déplaçant la fenêtre, celle-ci devient semi-transparente
    // afin de pouvoir lire le contenu qui se trouve sur la fenêtre parente.
    if (e.shiftKey)
        divCtnr.style.opacity = 0.3;
    else
        divCtnr.style.opacity = divCtnrInitOpacity;
}

function onmouseUpModal() {
    try {
        dragEnabled = false;

        top.document.onmouseup = oldMouseUp;
        top.document.onmousemove = oldMouseMove;

        var iTop = parseInt(dashedDiv.style.top);
        var iLeft = parseInt(dashedDiv.style.left);
        /*
        if (iTop < 0)
            divCtnr.style.top = "0px";
        else if (iTop + divCtnr.clientHeight > winHeight)
            divCtnr.style.top = (winHeight - divCtnr.clientHeight) + "px";
        else
        */
        divCtnr.style.top = dashedDiv.style.top;
        /*
        if (iLeft < 0)
            divCtnr.style.left = "0px";
        else if (iLeft + divCtnr.clientWidth > winWidth)
            divCtnr.style.top = (winWidth - divCtnr.clientWidth) + "px";
        else
        */
        divCtnr.style.left = dashedDiv.style.left;


        dashedDiv.style.display = "none";
        divMove.style.display = "none";

        //remise à zéro de la position sauvegardée de la souris (on en a plus besoin)
        xMouseMem = 0;
        yMouseMem = 0;

        // Remise à zéro de l'opacité de la fenêtre
        divCtnr.style.opacity = divCtnrInitOpacity;
    }
    catch (exp) { }
}

function onmouseMoveModal(e) {
    //Calcul des coordonnées
    if (!e)
        var e = window.event;

    var mouse = getClickPositionXY(e);
    x = mouse[0];
    y = mouse[1];





    if (dragEnabled == true) {
        divCtnr = document.getElementById("ContainerModal_" + modalId);


        if (divCtnr != null) {

            // La fenêtre peut être déplacée  en dehors de l'écran, à gauche, à droite et vers le bas (PAS VERS LE HAUT) 
            // avec un coefficient de visibilité
            // 1.0 : la fenêtre ne peut etre masquée
            // 0.5 : au max, la moitié de la fenêtre est masquée 
            // 0.0 : la fenêtre peut être masquée entièrement (pas recommandé)       
            var VISIBLE_WIN_COEF = 0.33;


            //Si on fait un dragndrop sur un element vers l'exterieur de l'ecran, on désactive le move pour eviter que IE agrandit le viewport
            if (y + divCtnr.clientHeight * VISIBLE_WIN_COEF > winHeight || x > winWidth) {

                var fctToRemove = divCtnr.onmousemove;
                divCtnr.removeEventListener("mousemove", fctToRemove);
                onmouseUpModal(e);
                stopEvent(e);
                dragEnabled = false;
                return;
            }

            //MOU On calcule les distances deltaX et deltaY entre l' ancienne et la nouvelle position de la souris 
            var deltaX = xMouseMem - x;
            var deltaY = yMouseMem - y;


            //on déplace le dashedDiv en tenant compte des déplacement deltaX et deltaY
            var dashedTop = parseInt(parseInt(dashedDiv.style.top) - deltaY);
            if (dashedTop >= 0 && (dashedTop + divCtnr.clientHeight * VISIBLE_WIN_COEF < winHeight))
                dashedDiv.style.top = dashedTop + "px";

            var dashedLeft = parseInt(parseInt(dashedDiv.style.left) - deltaX)
            if ((dashedLeft + divCtnr.clientWidth * (1 - VISIBLE_WIN_COEF) > 0) && (dashedLeft + divCtnr.clientWidth * VISIBLE_WIN_COEF < winWidth))
                dashedDiv.style.left = dashedLeft + "px";

            // Pour ne pas déclancher l'évenement de sélection sur certains champs
            stopEvent(e);

            //on mémorise les nouvelles coordonées
            xMouseMem = x;
            yMouseMem = y;

            // Bonus : Si on appuie sur Shift/Maj tout en déplaçant la fenêtre, celle-ci devient semi-transparente
            // afin de pouvoir lire le contenu qui se trouve sur la fenêtre parente.
            if (e.shiftKey)
                divCtnr.style.opacity = 0.3;
            else
                divCtnr.style.opacity = divCtnrInitOpacity;

        } else {
            dragEnabled = false;

        }
    }
}
//*****************************************************************************************************//
//*****************************************************************************************************//
//*** eEngine.js
//*****************************************************************************************************//
//*****************************************************************************************************//

function ErrorUpdateTreatmentReturn(oRes, engineObject) {



    var currentViewDoc = document;
    var currentView = getCurrentView(currentViewDoc);

    var updaterView = currentView;
    // View de la fenêtre initiatrice de la demande de MAJ ou de création. (en fonction de la iframe)
    if (engineObject.ModalDialog != null && engineObject.ModalDialog.modFile != null) {
        currentView = getCurrentView(engineObject.ModalDialog.modFile.document);
    }

    if (currentView != "CALENDAR")
        //top.setWait(false);
    //if (currentView == "KANBAN")
    //    oWidgetKanban.setWait(false);

    if (!engineObject && typeof (engineObject) != 'undefined')
        return;

    if (engineObject.ErrorCallbackFunction != null && typeof (engineObject.ErrorCallbackFunction) == 'function') {
        engineObject.ErrorCallbackFunction();
    }

    if (engineObject.DeleteRecord) {
        // Rien besoin
    }
    else if (engineObject.MergeRecord) {
        // Rien besoin
    }
    else if (engineObject.Formular) {
        // Rien besoin - la fonction de retour d'erreur a été définie à l'appel a engine
    }
    else {
        // En cas d'erreur, on recharge l'écran pour éviter toute donnée affiché erroné
        switch (currentView) {
            case "FILE_CREATION":
                // #41898 : On recharge la fiche, sauf pour le cas où on est en envoi de mail depuis le mode liste
                if (!(updaterView == "LIST" && engineObject.GetParam("bTypeMail") == "1"))
                    LoadFileAfterErrorCreation(oRes, engineObject.ModalDialog, updaterView);
                break;
            case "CALENDAR":
                // RIEN
                break;
            case "CALENDAR_LIST":
            case "LIST":
                // RIEN //ReloadList();
                break;
            case "KANBAN":
                oWidgetKanban.reload();
                break;
            case "FILE_CONSULTATION":
            case "FILE_MODIFICATION":
                engineObject.undoUpd();
                engineObject.FlagEdit(engineObject.GetParam('jsEditorVarName'), true, engineObject.GetParam('parentPopupSrcId'));

                break;

        }
    }

    engineObject.Clear();
}
function UpdateTreatmentReturn(oRes, engineObject, afterUpdate) {
    //    eTools.consoleLogFctCall()

    var currentProp = {}

    if (!engineObject) {

        alert('eEngine.UpdateTreatmentReturn - TODO');
        return;
    }

    //Les retours des formulaires sont traités de façons spécifiques.
    if (engineObject.Formular) {
        if (typeof (engineObject.SuccessCallbackFunction) == "function") {

            engineObject.SuccessCallbackFunction(engineObject, oRes);
        }


        return;
    }

    var editorObjectName = engineObject.GetParam('jsEditorVarName');
    var srcEltId = engineObject.GetParam('parentPopupSrcId');
    var engineConfirm = getXmlTextNode(oRes.getElementsByTagName("confirmmode")[0]);

    var currentViewDoc = (engineObject.GetParam("fromKanban") == "1") ? document : top.document;
    var currentView = getCurrentView(currentViewDoc);

    var isNoteFromParent = engineObject.GetParam('fromparent') == "1";
    var bCallBackGoFile = engineObject.GetParam('callbackgofile') == "1";

    var isMiddleFormula = engineObject.GetParam("engAction") == "4";
    var bReloadAfterCreation = false;

    var bTypeMail = engineObject.GetParam("bTypeMail") == "1";
    // Pas de reload de la fiche après la création
    var bNoLoadFile = engineObject.GetParam("noloadfile") == "1";

    try {


        // Gestion du CHECK ADDRESS
        if (engineConfirm == "1") {
            var adrdescid = getXmlTextNode(oRes.getElementsByTagName("descid")[0]);
            var adrtoupd = getXmlTextNode(oRes.getElementsByTagName("adrtoupd")[0]);
            var adrnoupd = getXmlTextNode(oRes.getElementsByTagName("adrnoupd")[0]);

            engineObject.ShowCheckAdr(adrdescid, adrtoupd, adrnoupd);

            return;
        }
        // Gestion du MIDDLE PROC
        // Gestion de la confirmation de suppression
        // Gestion de la confirmation de fusion
        else if (engineConfirm == "2" || engineConfirm == "3" || engineConfirm == "6") {
            // MessageBox du message ou demande confirmation si l'on veux forcer la mise à jour de la valeur incorrect
            var lstConfirmBox = oRes.getElementsByTagName("confirm");
            if (lstConfirmBox == null || lstConfirmBox.length == 0) return;

            if (engineConfirm == "2")
                engineObject.ShowMiddleConfirm(lstConfirmBox[0]);
            else if (engineConfirm == "3")
                engineObject.ShowCustomConfirm(lstConfirmBox[0],
                    function () { engineObject.validSupConfirm(); },
                    function () { engineObject.cancelSupConfirm(); });
            else if (engineConfirm == "6")
                engineObject.ShowCustomConfirm(lstConfirmBox[0],
                    function () { engineObject.validMergeConfirm(); },
                    function () { engineObject.cancelMergeConfirm(); });

            return;
        }
        // Gestion de la confirmation de suppression avec confirmation de suppression des PP en cascade
        else if (engineConfirm == "4") {
            var fldMainDisplayVal = getXmlTextNode(oRes.getElementsByTagName("fldmaindispval")[0]);
            engineObject.ShowSupPpConfirm(fldMainDisplayVal);

            return;
        }
        // Gestion de la confirmation de suppression avec confirmation de detachement ou suppression du rdv
        else if (engineConfirm == "5") {
            // MessageBox du message ou demande confirmation si l'on veux forcer la mise à jour de la valeur incorrect
            var lstConfirmBox = oRes.getElementsByTagName("confirm");
            if (lstConfirmBox == null || lstConfirmBox.length == 0) return;

            var fldmultiownerdid = getXmlTextNode(oRes.getElementsByTagName("fldmultiownerdid")[0]);
            var multiownernewval = getXmlTextNode(oRes.getElementsByTagName("multiownernewval")[0]);

            engineObject.ShowSupMultiOwnerConfirm(lstConfirmBox[0], engineObject.GetParam("fileId"), fldmultiownerdid, multiownernewval);

            return;
        }
        // Gestion de la confirmation de formule du milieu de l'ORM
        else if (engineConfirm == "7") {
            // Information de la MessageBox de la confirmation de ORM_CONFIRM
            var lstConfirmBox = oRes.getElementsByTagName("confirm");
            if (lstConfirmBox == null || lstConfirmBox.length == 0) return;

            engineObject.ShowOrmMiddleConfirm(lstConfirmBox[0]);

            return;
        }
        // Gestion d'annumlation de l'operation via l'ORM
        else if (engineConfirm == "8") {
            // Information de la MessageBox de la confirmation de ORM_CANCEL
            var lstConfirmBox = oRes.getElementsByTagName("confirm");
            if (lstConfirmBox == null || lstConfirmBox.length == 0) {
                // Si pas de message, on annule la saisie
                engineObject.cancelMiddleConfirm();
            } else {
                // Sinon affichage du message, puis on annule la saisie au clique sur le bouton
                engineObject.ShowCustomConfirm(lstConfirmBox[0],
                    null,
                    function () { engineObject.cancelMiddleConfirm(); });
            }

            return;
        }

        engineObject.Clear();

        //SPH 05/02/2014
        //27488 : dans ce contexte (enregistrement d'une note d'une fiche parente de l'enregistrement courrant ex: note affaire depuis planning),
        //il ne faut pas faire d'autre traitements. Le contexte currentView/updaterView n'est pas en phase avec le retour de l'engine qui suppose 
        // être sur le champ modifié
        if (isNoteFromParent)
            return;

        // View de la fenêtre initiatrice de la demande de MAJ ou de création. (en fonction de la iframe)
        var updaterView = currentView;
        if (engineObject.ModalDialog != null && currentView != "KANBAN")
            updaterView = getCurrentView(engineObject.ModalDialog.modFile.document);

        // Recupe les informations de création de fiche
        var oCreatedRecord = null;
        if (updaterView == "FILE_CREATION") {
            var oCreatedRecord = oRes.getElementsByTagName("createdrecord");
            if (oCreatedRecord != null && oCreatedRecord.length != 0) {
                oCreatedRecord = oCreatedRecord[0];

                // Split car la donnée id représente une liste dans certains cas
                var creaTab = oCreatedRecord.getAttribute('tab');
                var creaFileId = oCreatedRecord.getAttribute('ids');
                if (creaFileId != null && typeof (creaFileId) != "undefined")
                    creaFileId = creaFileId.split(';')[0];

                var oMainFields = oRes.getElementsByTagName("field");
                var sMainLabel = "";
                if (oMainFields != null && oMainFields.length != 0) {
                    var oMainField = oMainFields[0];

                    if (oMainField.querySelector) {
                        oMainField = oMainField.querySelector("record[fid='" + creaFileId + "']");
                    }
                    else {
                        var oRecords = oMainField.getElementsByTagName("record");
                        if (oRecords != null && oRecords.length != 0) {
                            for (var nI = 0; nI < oRecords.length; nI++) {
                                if (oRecords[nI].getAttribute("fid") == creaFileId) {
                                    oMainField = oRecords[nI];
                                    break;
                                }
                            }
                        }
                    }

                    if (oMainField) {
                        sMainLabel = eTools.getInnerHtml(oMainField);
                    }
                }

                if (isInt(creaFileId) && isInt(creaTab)) {

                    oCreatedRecord = { fid: creaFileId, tab: creaTab, lab: sMainLabel };

                    //Pour la création de PP, il y a aussi la creation de adresse
                    if (creaTab == 200)
                        AppendCreatedAdrRecord(oRes, oCreatedRecord);


                } else
                    oCreatedRecord = null;
            }
            else
                oCreatedRecord = null;
        }



        // Recupe les informations de modification de fiche
        var oUpdatedRecord = null;
        if (oCreatedRecord == null) {
            var oUpdatedRecord = oRes.getElementsByTagName("updatedrecord");
            if (oUpdatedRecord != null && oUpdatedRecord.length != 0) {
                oUpdatedRecord = oUpdatedRecord[0];

                var recTab = oUpdatedRecord.getAttribute('tab');
                var recFileId = oUpdatedRecord.getAttribute('id');
                var recAnLnk = oUpdatedRecord.getAttribute('anlnk') == "1";
                var recHistoUpd = oUpdatedRecord.getAttribute('histoupd') == "1";

                if (isInt(recFileId) && isInt(recTab))
                    oUpdatedRecord = { fid: recFileId, tab: recTab, anlnk: recAnLnk, histoupd: recHistoUpd };
                else
                    oUpdatedRecord = null;
            }
            else
                oUpdatedRecord = null;
        }

        // Recupe les informations de suppression de fiche
        var oDeletedRecord = null;
        if (oCreatedRecord == null && oUpdatedRecord == null) {
            var oDeletedRecord = oRes.getElementsByTagName("deletedrecord");
            if (oDeletedRecord != null && oDeletedRecord.length != 0) {
                oDeletedRecord = oDeletedRecord[0];

                var delLstTab = oDeletedRecord.getAttribute('tabs');
                var delMainTab = oDeletedRecord.getAttribute('maintab');

                if (isInt(delMainTab))
                    oDeletedRecord = { lstTab: delLstTab, mainTab: delMainTab };
                else
                    oDeletedRecord = null;
            }
            else
                oDeletedRecord = null;
        }


        // Recupe les informations de fusion de fiche
        var oMergedRecord = null;
        if (oCreatedRecord == null && oUpdatedRecord == null && oDeletedRecord == null) {
            var oMergedRecord = oRes.getElementsByTagName("mergedrecord");
            if (oMergedRecord != null && oMergedRecord.length != 0) {
                oMergedRecord = oMergedRecord[0];

                var mergMasterFileId = oMergedRecord.getAttribute('masterFileId');
                var mergMainTab = oMergedRecord.getAttribute('maintab');

                if (isInt(mergMasterFileId) && isInt(mergMainTab))
                    oMergedRecord = { masterFileId: mergMasterFileId, mainTab: mergMainTab };
                else
                    oMergedRecord = null;
            }
            else
                oMergedRecord = null;
        }

        // Envoi des images non encore uploadées
        var oImages = document.querySelectorAll("img");
        if (engineObject.ModalDialog && engineObject.ModalDialog.modFile)
            oImages = engineObject.ModalDialog.modFile.document.querySelectorAll("img");

        if (oImages) {
            for (var i = 0; i < oImages.length; i++) {
                if (oImages[i].src.indexOf("fid=-1") != -1 || getAttributeValue(oImages[i], "session") == "1") {
                    var url = "mgr/eImageManager.ashx";

                    var oUpdater = new eUpdater(url, null);
                    for (var key in this.parameters) {
                        if (typeof (this.parameters[key]) != 'function')
                            oUpdater.addParam(key, this.parameters[key], "post");
                    }

                    var nbFld = 0;
                    for (var key in this.fields) {
                        if (typeof (this.fields[key]) != 'function') {
                            oUpdater.addParam('fld_' + nbFld, this.fields[key].GetSerialize(), "post");
                            nbFld++;
                        }
                    }

                    var imageFieldFileId = 0;
                    var imageFieldDescId = 0;

                    if (oCreatedRecord != null)
                        imageFieldFileId = oCreatedRecord.fid;
                    else if (oUpdatedRecord != null)
                        imageFieldFileId = oUpdatedRecord.fid;

                    if (document.getElementById(getAttributeValue(oImages[i].parentNode, "ename")))
                        imageFieldDescId = getAttributeValue(document.getElementById(getAttributeValue(oImages[i].parentNode, "ename")), "did");
                    else if (engineObject.ModalDialog.modFile.document.getElementById(getAttributeValue(oImages[i].parentNode, "ename")))
                        imageFieldDescId = getAttributeValue(engineObject.ModalDialog.modFile.document.getElementById(getAttributeValue(oImages[i].parentNode, "ename")), "did");
                    else if (getAttributeValue(oImages[i].parentNode, "did") != "")
                        imageFieldDescId = getAttributeValue(oImages[i].parentNode, "did");

                    var tab = Number(imageFieldDescId) - Number(imageFieldDescId) % 100;

                    //CNA - #48078 - Empêche l'execution de cette partie lors du déclenchement d'une formule du milieu en creátion de fiche
                    if (imageFieldFileId != 0 && imageFieldFileId != "0") {

                        var imageType = "IMAGE_FIELD";
                        if (tab == 101000)
                            imageType = "USER_AVATAR_FIELD";
                        else if (Number(imageFieldDescId) % 100 == 75)
                            imageType = "AVATAR_FIELD";

                        oUpdater.addParam("action", "UPLOAD", "post");
                        oUpdater.addParam("fileId", imageFieldFileId, "post");
                        oUpdater.addParam("fieldDescId", imageFieldDescId, "post");
                        oUpdater.addParam("imageType", imageType, "post");
                        oUpdater.addParam("computeRealThumbnail", "0", "post");
                        oUpdater.addParam("imageWidth", "16", "post");
                        oUpdater.addParam("imageHeight", "16", "post");

                        var oEngine = engineObject;
                        oUpdater.ErrorCallBack = function (oRes) { ErrorImageUploadReturn(oRes, oEngine); };
                        oUpdater.asyncFlag = this.async;
                        oUpdater.send(function (oRes) { ImageUploadReturn(oRes, oEngine); });
                    }
                }
            }
        }

        // Mise à jour de la MRU
        ReloadMru(oRes);



        // Recupe la liste des rubriques impactées par des régles
        var lstDescIdRuleUpdated = getXmlTextNode(oRes.getElementsByTagName("descidrule")[0]);
        if (lstDescIdRuleUpdated.length > 0) {
            lstDescIdRuleUpdated = lstDescIdRuleUpdated.split(';');
        }
        else {
            lstDescIdRuleUpdated = null;
        }

        // Réinitialise les indicateur de refresh
        fieldRefresh.initRefresh();

        var doGlobalRefresh = false;

        // Reloads des fields, bkm, file désactivé
        if (engineObject.ReloadNothing)
            ;
        // On reload la fiche dans le cas d'une fusion
        else if (oMergedRecord != null) {
            // TODO - 33189 - Est-ce le mieux de refresh toute la fiche ?

            //Supprime la fiche en doublon de la lisde des id pour le pagging
            try {

                var eParam = top.document.getElementById('eParam').contentWindow;
                if (eParam) {

                    //Liste des ID de la liste en cours
                    var sIdsListMerged = CleanListIds(eParam.GetParam("List_" + oMergedRecord.mainTab));
                    if (typeof (sIdsListMerged) == "string") {

                        var adListDbl = sIdsListMerged.split(";");

                        if (adListDbl.length > 0 && engineObject.IsMerge && engineObject.MergeInfos) {
                            //Fiche supprimée
                            var dblFile = engineObject.MergeInfos.doublonFileId;

                            var adListDblD = adListDbl.filter(function (elem) { return elem + "" != dblFile + "" });
                            var sNewLst = "";
                            if (adListDblD.length > 0) {
                                sNewLst = adListDblD.join(";");
                                eParam.SetParam("List_" + oMergedRecord.mainTab, sNewLst);
                            }
                            else {
                                //Si plus d'ids, on charge les id suivants
                                var nPage = eParam.GetParam("Page_" + oMergedRecord.mainTab);
                                if (LoadIdsPage(oMergedRecord.mainTab, nPage + 1)) {
                                    eParam.SetParam("Page_" + oMergedRecord.mainTab, nPage + 1);
                                }
                            }
                        }
                    }
                }
            }
            catch (e) {

            }

            RefreshFile();
        }
        // ApplyRuleOnBlank lors du retour des formules du milieu en mode création
        else if (updaterView == "FILE_CREATION" && oCreatedRecord == null) {

            fieldRefresh.refreshFldPopup = true;

            // Fiche pas encore enregistrée
            var modalFrame = null;
            if (engineObject.ModalDialog != null && engineObject.ModalDialog.modFile != null)
                modalFrame = engineObject.ModalDialog.modFile;
            LoadChgFld(oRes, true, modalFrame);

            var oRefreshValue = null;
            var fldEngine = null;
            var isInRule = false;
            var oWin = engineObject.ModalDialog.modFile;
            for (var key in fieldRefresh.dicRecords) {
                if (typeof (fieldRefresh.dicRecords[key]) == 'function')
                    continue;


                oRefreshValue = fieldRefresh.dicRecords[key];
                fldEngine = fieldRefresh.convertRefreshValToUpdEngFld(oRefreshValue);

                if (getAttributeValue(oWin.document.getElementById("COL_" + getTabDescid(fldEngine.descId) + "_" + fldEngine.descId), "rul") == "1"
                    || getAttributeValue(oWin.document.getElementById("COL_" + getTabDescid(fldEngine.descId) + "_" + fldEngine.descId), "mf") == "1"
                ) {
                    isInRule = true;
                    break;
                }
            }

            // HLA - On ajoute également un test sur la rubrique source pour prendre en charge l'appel un eventuel applyruleonblank si une règle est dépendante du champ source - Bug #39 241
            if (!isInRule) {
                var aSrcEltId = srcEltId.split("_");
                var sHeadEltId = aSrcEltId.slice(0, 3).join("_");

                isInRule = getAttributeValue(oWin.document.getElementById(sHeadEltId), "rul") == "1";
            }

            if (isInRule) {
                //var editorObjectName = this.GetParam('jsEditorVarName');
                if (editorObjectName != '') {
                    var editorObject = window[editorObjectName];
                    if (!editorObject) {
                        try {
                            editorObject = eval(editorObjectName);
                        }
                        catch (ex) { }
                    }
                }

                if (editorObject && editorObject.tab == 400 && editorObject.fileId == 0) {
                    if (document.getElementById("fileDiv_" + nGlobalActiveTab) && getAttributeValue(document.getElementById("fileDiv_" + nGlobalActiveTab), "did") == "200" && getAttributeValue(document.getElementById("fileDiv_" + nGlobalActiveTab), "fid") == "0") {
                        oWin.applyRuleOnBlank(200, null, 0, 5, srcEltId);
                    }
                    else
                        oWin.applyRuleOnBlank(editorObject.tab, null, 0, 5, srcEltId);
                }
                else {

                    //#41080 SPH : test de l'existanece de fldEngine + gestion du cas de la création de pp + addresse.
                    // dans ce cas, il faut passer a applyrulonblank 200 puisque c'est la table principale.
                    // TODO : la gestion de ce type de reload est partagé entre efieldeditor.js et eegine.js
                    // du coup, parfois, l'applyruleonblank est appelé 2 fois.
                    // par exemple, en création pp+adr avec un champ sur lequel il existe une règle conditionnel et qui port une formule du milieu qui vient changer une valeur (retour type select 1,'&405=xxxx')
                    // il sera bien de centralisé cela.
                    if (fldEngine != null) {
                        if ((fldEngine.descId - fldEngine.descId % 100 == 400) &&
                            (document.getElementById("fileDiv_" + nGlobalActiveTab) && getAttributeValue(document.getElementById("fileDiv_" + nGlobalActiveTab), "did") == "200" && getAttributeValue(document.getElementById("fileDiv_" + nGlobalActiveTab), "fid") == "0")
                        ) {

                            oWin.applyRuleOnBlank(200, null, 0, 5, srcEltId);
                        }
                        else
                            oWin.applyRuleOnBlank(getTabDescid(fldEngine.descId), null, 0, 5, srcEltId);
                    }
                }
            }
        }
        // Création en mode fiche (non popup) (obsolete)
        else if (updaterView == "FILE_CREATION" && oCreatedRecord != null && engineObject.ModalDialog == null) {

            if (!bReloadAfterCreation)
                LoadFileAfterCreation(oCreatedRecord);

            bReloadAfterCreation = true;
        }
        // Rechargement du rendu list/fiche/bkm/popup lorsque une régle impacte ce rendu
        else if (lstDescIdRuleUpdated != null) {


            // Modif depuis un mode list
            if (updaterView == "LIST" || updaterView == "CALENDAR" || updaterView == "CALENDAR_LIST") {

                // Si la liste n'a pas été raffraichie, on lance tout de même le refresh des fields modifiés
                if (!ReloadList(lstDescIdRuleUpdated)) {
                    doGlobalRefresh = true;
                    // Dans le cas ou la liste n'a pas été raffraichie, on vide la liste pour déclencher le flagedit
                    lstDescIdRuleUpdated = null;
                }
            }
            else if (updaterView == "KANBAN") {
                if (oWidgetKanban) {
                    if (!oWidgetKanban.reloadWithRule(lstDescIdRuleUpdated)) {
                        doGlobalRefresh = true;
                        lstDescIdRuleUpdated = null;
                    }
                }
            }
            // Modif depuis une fiche
            else if (engineObject.ModalDialog == null) {

                //Suppression
                if (oDeletedRecord != null) {
                    if (nGlobalActiveTab == oDeletedRecord.mainTab)
                        top.goTabList(oDeletedRecord.mainTab);
                    else if (eTools.getBookmarkContainer(oDeletedRecord.mainTab))
                        RefreshFile();
                }
                else {
                    //Modif
                    var bGlobalActiveTabRuleUpdated = false;

                    //Parcour des descid impliqué dans une règle
                    // détecte si un champ est conditionné par une règle qui a été recalculé suite à la modification
                    var arrTabRules = []
                    for (var i = 0; i < lstDescIdRuleUpdated.length; ++i) {
                        var nTabRules = lstDescIdRuleUpdated[i] - (lstDescIdRuleUpdated[i] % 100);
                        if ((nTabRules) == nGlobalActiveTab)
                            bGlobalActiveTabRuleUpdated = true;
                        else {
                            if (arrTabRules.indexOf(nTabRules) === -1)
                                arrTabRules.push(nTabRules)
                        }
                    }



                    //Cas fiche signet 
                    if (nGlobalActiveTab != oUpdatedRecord.tab) {

                        var bReloadAllWithRules = false;
                        var bReloadAllBkmWithRules = false;
                        var bReloadHeaderWithRules = false;

                        //récupération du retour des reload spécifique
                        try {
                            //Reload la fiche
                            bReloadAllWithRules = getXmlTextNode(oRes.getElementsByTagName("reloadfileheader")[0]) == "1";
                            //Reload tous les signets
                            bReloadAllBkmWithRules = getXmlTextNode(oRes.getElementsByTagName("reloaddetail")[0]) == "1";
                            //reload l'entête de fiche
                            bReloadHeaderWithRules = getXmlTextNode(oRes.getElementsByTagName("reloadheader")[0]) == "1";
                        }
                        catch (e) {

                        }

                        //Si reload signet+header => reload intégrale de la fiche
                        bReloadAllWithRules = bReloadAllBkmWithRules && bReloadHeaderWithRules

                        //reload intégrale
                        if (bReloadAllWithRules) {
                            RefreshFile(window, srcEltId);

                            fieldRefresh.refreshFld = false;
                            fieldRefresh.refreshFldBkm = false;
                        }
                        else {
                            //  **  reload partiel ***

                            /*  Gestion reload Entête */
                            //Signet mode liste 
                            if (isBkmList(oUpdatedRecord.tab)) {
                                //  reload de l'entête
                                if (bGlobalActiveTabRuleUpdated || bReloadHeaderWithRules) {
                                    RefreshHeader();
                                    fieldRefresh.refreshFld = false;
                                }
                            }
                            //Signet mode incrutsé
                            else if (isBkmFile(oUpdatedRecord.tab)) {

                                //rafraichissemnt en tête systématique
                                RefreshHeader();
                                fieldRefresh.refreshFld = false;

                            }

                            /*  gestion reload signets */

                            //  reload global des signets
                            if (bReloadAllBkmWithRules) {
                                RefreshAllBkm();    // Reload toute l'iframe des bkm
                                fieldRefresh.refreshFldBkm = false;
                            }
                            else {
                                //reload partiel des signets


                                //Refresh uniquement les champs d'entete
                                fieldRefresh.refreshFldHead = true;


                                //Signet encours + signet via reload=bkmXXX
                                var reloadedBkm = ReloadBkm(oRes, oUpdatedRecord) || [];

                                //reload des signets contenant un champ recalculé par une règle qui a été déclenchée
                                if (Array.isArray(reloadedBkm) && Array.isArray(arrTabRules) && arrTabRules.length > 0) {
                                    arrTabRules.forEach(function (elem) {
                                        //si le signet n'a pas été déjà reloadé, on le reload
                                        if (reloadedBkm.indexOf(elem) == -1 && elem != oUpdatedRecord.tab) {
                                            top.RefreshBkm(elem);
                                        }
                                    })
                                }
                            }
                        }
                    }
                    else {
                        //autre (mode fiche "classique") reload globale
                        RefreshFile(window, srcEltId);
                    }
                }
            }
            // Modif depuis popup
            else {
                // Reload la popup si pas de fermeture de la popup
                if (!engineObject.ModalDialog.pupClose && oDeletedRecord == null) {
                    if (updaterView == "FILE_CREATION" && oCreatedRecord != null && getNumber(oCreatedRecord.fid) > 0) {
                        if (!bReloadAfterCreation)
                            LoadFileAfterCreation(oCreatedRecord, engineObject.ModalDialog.modFile);

                        bReloadAfterCreation = true;
                    }
                    else if (isMiddleFormula) {
                        fieldRefresh.refreshFldPopup = true;

                        // Fiche pas encore enregistrée
                        var modalFrame = null;
                        if (engineObject.ModalDialog != null && engineObject.ModalDialog.modFile != null)
                            modalFrame = engineObject.ModalDialog.modFile;
                        LoadChgFld(oRes, true, modalFrame);

                        engineObject.ModalDialog.modFile.applyRuleOnBlank(oUpdatedRecord.tab, null, oUpdatedRecord.fid);
                    }
                    else if (typeof (RefreshFile) == "function") {
                        RefreshFile(engineObject.ModalDialog.modFile);
                    }
                    else if (typeof (engineObject.ModalDialog.modFile.RefreshFile) == "function") {
                        engineObject.ModalDialog.modFile.RefreshFile(engineObject.ModalDialog.modFile);
                    }
                } else {
                    // Dans le cas d'une règle de saisie, on redirige vers la fiche créée
                    if (engineObject.ModalDialog != null && engineObject.ModalDialog.oModFile && engineObject.ModalDialog.oModFile.CallFrom == CallFromNavBar) {
                        if (updaterView == "FILE_CREATION") {
                            if (!bReloadAfterCreation)
                                LoadFileAfterCreation(oCreatedRecord, top);

                            bReloadAfterCreation = true;
                        }
                    }
                }

                // Reload l'arrière plan
                switch (currentView) {
                    case "LIST":
                    case "CALENDAR":
                    case "CALENDAR_LIST":
                        if (oDeletedRecord != null)
                            top.goTabList(oDeletedRecord.mainTab);
                        else {
                            if (oCreatedRecord != null) {
                                if (getAttributeValue(top.document.getElementById("mt_" + nGlobalActiveTab), "edntyp") == "0") {
                                    if (!bReloadAfterCreation)
                                        LoadFileAfterCreation(oCreatedRecord, top);

                                    bReloadAfterCreation = true;
                                }
                                else {



                                    if (currentView == "LIST")
                                        ReloadList();
                                    else

                                        top.loadList();
                                }
                            }
                            else {
                                if (bCallBackGoFile) {
                                    //Mise à jour avec callback pour aller sur une fiche

                                }
                                else {


                                    if (currentView == "LIST")
                                        ReloadList();
                                    else
                                        top.loadList();
                                }
                            }
                        }
                        break;
                    case "KANBAN":
                        oWidgetKanban.reload();
                        break;
                    case "FILE_CONSULTATION":
                    case "FILE_MODIFICATION":
                        if (oCreatedRecord != null) {
                            var sTabId = oCreatedRecord.tab;
                            //NHA bg#74 759 : Bug création fiche contact dans le cas ou il y a une condition d'affichage sur un champ
                            if (top.nGlobalActiveTab == TAB_PM && oCreatedRecord.tab == TAB_PP)
                                sTabId = TAB_ADR
                            if (eTools.getBookmarkContainer(sTabId, top.document)) {
                                if (!engineObject.ModalDialog.pupClose) {
                                    top.loadBkmList(sTabId);
                                    top.RefreshHeader();
                                }
                                else {
                                    top.RefreshFile();
                                }
                            }


                            else if (oCreatedRecord.tab == top.nGlobalActiveTab) {
                                if (!bReloadAfterCreation)
                                    LoadFileAfterCreation(oCreatedRecord, top);
                                bReloadAfterCreation = true;
                            }
                            //Cas où on crée une fiche d'un onglet différent sur lequel on était
                            else if (oCreatedRecord.tab != top.nGlobalActiveTab) {
                                doGlobalRefresh = true;
                            }
                        }
                        else if (oUpdatedRecord != null) {

                            if (bCallBackGoFile) {
                                //Si le callback est un go file, on ne fait pas le reload de fiche / liste
                                // ceux-ci sont inutilent (puisque que une autre fiche va être ouverte) 
                                // et posent de problèmes de synchronicité de load de script
                            }
                            else {
                                if (eTools.getBookmarkContainer(oUpdatedRecord.tab, top.document)) {
                                    if (!engineObject.ModalDialog.pupClose) {
                                        top.loadBkmList(oUpdatedRecord.tab);
                                        top.RefreshHeader();
                                    }
                                    else {
                                        RefreshFile();
                                    }
                                }
                            }
                        } else if (oDeletedRecord != null) {
                            if (eTools.getBookmarkContainer(oDeletedRecord.mainTab, top.document)) {
                                RefreshFile();
                            }
                        }

                        break;
                }
            }
        }
        // Refresh Fields ou Reloads du rendu
        else {
            doGlobalRefresh = true;
        }



        if (doGlobalRefresh) {

            var reloadHeader = getXmlTextNode(oRes.getElementsByTagName("reloadheader")[0]) == "1";
            var reloadDetail = getXmlTextNode(oRes.getElementsByTagName("reloaddetail")[0]) == "1";
            var reloadFileHeader = getXmlTextNode(oRes.getElementsByTagName("reloadfileheader")[0]) == "1";

            //On ne reloade header/detail que si on est bien sur la table que la table courante
            if (top && nGlobalActiveTab != top.nGlobalActiveTab) {
                reloadHeader = false;
                reloadDetail = false;
                reloadFileHeader = false;
            }


            // [38536] MOU3-PC Ex : Force le recharge de la liste
            var reloadList = currentView == "LIST" && (reloadHeader && reloadDetail);

            var reloadKanban = currentView == "KANBAN" && (reloadHeader && reloadDetail);

            // Vérifie que les fonctions de refresh sont disponibles
            reloadHeader = reloadHeader && typeof (RefreshHeader) == 'function';
            reloadDetail = reloadDetail && typeof (RefreshAllBkm) == 'function';
            //reloadFileHeader = reloadFileHeader && typeof (RefreshFileHeader) == 'function';

            //on verifie si la confidentialité a été changée
            var bConfidChange = false;
            var xmlFlds = oRes.getElementsByTagName("field");
            var xmlConfidFld;
            for (var fldIdx = 0; fldIdx < xmlFlds.length; fldIdx++) {
                try {
                    if (getNumber(xmlFlds[fldIdx].getAttribute("descid")) == (getNumber(recTab) + CONFIDENTIAL)) {
                        xmlConfidFld = xmlFlds[fldIdx];
                        break;
                    }
                }
                catch (ex) {
                    break;
                }
            }

            if (xmlConfidFld) {
                var xmlRecs = xmlConfidFld.getElementsByTagName("record");
                var xmlConfidRecFld;
                for (var recIdx = 0; recIdx < xmlRecs.length; recIdx++) {
                    try {
                        if (getNumber(xmlRecs[recIdx].getAttribute("fid")) == recFileId) {
                            xmlConfidRecFld = xmlRecs[recIdx];
                            bConfidChange = xmlConfidRecFld.getAttribute("dbv") == null || xmlConfidRecFld.getAttribute("dbv") == "";
                            break;
                        }
                    }
                    catch (ex) {
                        break;
                    }
                }

            }

            // Modif en mode liste
            if (updaterView == "LIST" || updaterView == "CALENDAR" || updaterView == "CALENDAR_LIST" || updaterView == "KANBAN") {

                // [38536] MOU3-PC Ex :  
                // Une formule de bas sur un champ PM ecoutant le ADR96, se déclanche sur une modification d'un champ ADR quelconque en mode liste de PP.
                // Engine.cs demande a Engine.js de rafraichir la liste d'ou l'ajout de reloadList    
                if (oUpdatedRecord != null && (oUpdatedRecord.anlnk || oUpdatedRecord.histoupd || bConfidChange || reloadList || reloadKanban)) {
                    if (updaterView == "LIST")
                        ReloadList();
                    else if (updaterView == "KANBAN") {
                        if (oWidgetKanban) {
                            oWidgetKanban.reload();
                        }
                    }
                    else
                        top.goTabList(top.nGlobalActiveTab);
                }
                else
                    fieldRefresh.refreshFld = true;
            }
            // Modif en fiche ou popup
            else {
                // Si la popup reste ouverte, on recharge la fiche pour passer du mode crea au mdoe mode modif ou on raffraichi les fields en mode modification
                // MAB #47 701 : Sauf sur une fiche de type E-mail, qui peut être laissée ouverte dans certains cas (ex : envoi de mail de test)
                // afin de permettre de réitérer l'opération - Même type d'exception que pour le correctif #41 898 plus bas
                if (engineObject.ModalDialog != null && !engineObject.ModalDialog.pupClose) {
                    if (updaterView == "FILE_CREATION" && !bTypeMail) {
                        if (!bReloadAfterCreation)
                            LoadFileAfterCreation(oCreatedRecord, engineObject.ModalDialog.modFile);

                        bReloadAfterCreation = true;
                    }
                    else
                        fieldRefresh.refreshFldPopup = true;
                }
                // Si la fiche est fermée en fonction d'ou on vient, on charge la fiche.
                else if (engineObject.ModalDialog != null && engineObject.ModalDialog.oModFile && engineObject.ModalDialog.oModFile.CallFrom == CallFromNavBar) {
                    if (updaterView == "FILE_CREATION") {

                        if (!bReloadAfterCreation)
                            LoadFileAfterCreation(oCreatedRecord, top);

                        bReloadAfterCreation = true;
                    }
                }

                switch (currentView) {
                    case "LIST":
                    case "CALENDAR":
                    case "CALENDAR_LIST":
                    case "KANBAN":

                        // Cas impossible dans le cas du mode fiche
                        if (engineObject.ModalDialog == null)
                            break;

                        if (updaterView == "FILE_CREATION") {
                            // Si on crée une fiche depuis l'accueil(nGlobalActive = 0) on redirige vers la fiche après création [MOU #34495]
                            // Sauf quand c'est un envoi de mail donc création de fiche de type Email [CRU #41898]
                            // et sauf pour la table [RGPDTreatmentsLogs]
                            if (((getAttributeValue(top.document.getElementById("mt_" + nGlobalActiveTab), "edntyp") == "0" || top.nGlobalActiveTab == 0))
                                && !bTypeMail && top.nGlobalActiveTab != 117000 && !bNoLoadFile) {

                                if (!bReloadAfterCreation)
                                    LoadFileAfterCreation(oCreatedRecord, top);

                                bReloadAfterCreation = true;
                            }
                            else {

                                if (!bCallBackGoFile && !bReloadAfterCreation) {

                                    if (currentView == "LIST")
                                        ReloadList();
                                    else
                                        top.loadList();

                                    fieldRefresh.refreshFldPopup = false;
                                }
                            }
                        }
                        else if (oDeletedRecord != null) {
                            top.goTabList(oDeletedRecord.mainTab);
                            fieldRefresh.refreshFldPopup = false;
                        }
                        else {
                            if (reloadFileHeader || (oUpdatedRecord != null && oUpdatedRecord.anlnk)) {

                                if (!bCallBackGoFile) {
                                    if (currentView == "LIST")
                                        ReloadList();
                                    else if (currentView == "KANBAN") {
                                        if (oWidgetKanban) {
                                            oWidgetKanban.reload();
                                        }
                                    }
                                    else
                                        top.loadList();
                                }
                            }
                            else
                                fieldRefresh.refreshFld = true;
                        }
                        break;
                    case "FILE_CONSULTATION":
                    case "FILE_MODIFICATION":
                        fieldRefresh.refreshFld = true;
                        fieldRefresh.refreshFldBkm = true;

                        if (oCreatedRecord != null && oCreatedRecord.tab == nGlobalActiveTab) {
                            if (!bReloadAfterCreation)
                                LoadFileAfterCreation(oCreatedRecord, top);
                            bReloadAfterCreation = true;
                            break;
                        }

                        var bCallFromFinder = engineObject
                            && engineObject.ModalDialog
                            && engineObject.ModalDialog.oModFile
                            && engineObject.ModalDialog.oModFile.CallFrom == CallFromFinder;

                        if (oCreatedRecord && bCallFromFinder) {
                            reloadHeader = false;
                            reloadDetail = false;
                        }

                        if (oUpdatedRecord && bConfidChange) {
                            //Confidentielle modifiée depuis le mode fiche
                            if (top.nGlobalActiveTab == oUpdatedRecord.tab) {
                                reloadHeader = true;
                                reloadDetail = true;
                            }
                            else if (eTools.getBookmarkContainer(oUpdatedRecord.tab, top.document)) {
                                //Confidentielle modifiée depuis un signet.
                                top.loadBkmList(oUpdatedRecord.tab);
                                fieldRefresh.refreshFldBkm = false;
                            }

                            // HLA - Il faut egalement tester si on n'a pas demandé la fermeture de la fenetre, dans ce cas, inutile de refaire une fenetre qui se ferme - #60737
                            if (engineObject.ModalDialog && !engineObject.ModalDialog.pupClose) {
                                var modFileFrm = engineObject.ModalDialog.oModFile.getIframe();
                                if (modFileFrm && modFileFrm.nGlobalActiveTab == oUpdatedRecord.tab) {
                                    RefreshFile(modFileFrm);
                                    fieldRefresh.refreshFld = false;
                                    fieldRefresh.refreshFldBkm = false;
                                    fieldRefresh.refreshFldPopup = false;
                                }
                            }
                        }

                        if (oDeletedRecord != null && engineObject.ModalDialog == null && !isBkmFile(oDeletedRecord.mainTab) && !isBkmDisc(oDeletedRecord.mainTab)) {
                            // Dans le cas de la suppression de la fiche (non popup) en cours de consultation, on revient sur la liste
                            top.goTabList(oDeletedRecord.mainTab);

                            fieldRefresh.refreshFld = false;
                            fieldRefresh.refreshFldBkm = false;
                            fieldRefresh.refreshFldPopup = false;
                        }
                        else if (reloadHeader && reloadDetail) {
                            // Si le reloadHeader et reloadDetail sont demandés, alors on raffraichit la fiche totalement
                            // Inutile de recharger les valeurs des rubriques si les écrans vont être raffraichis

                            /*
                            var refreshTab = null;
                            if (oCreatedRecord != null && oCreatedRecord.tab != null)
                                refreshTab = oCreatedRecord.tab;

                            RefreshFile(null,null, refreshTab );
                                */

                            if ((oCreatedRecord && oCreatedRecord.tab == nGlobalActiveTab)
                                || (oUpdatedRecord && oUpdatedRecord.tab == top.nGlobalActiveTab)) {
                                if (engineObject.ModalDialog)
                                    RefreshFile(engineObject.ModalDialog.modFile);
                                else {
                                    if (!currentProp.isRefreshingFile) {
                                        currentProp.isRefreshingFile = true;
                                        RefreshFile();
                                    }
                                }
                            }

                            //NHA : demande 74 116 automatisme chargement infinis
                            //SPH : evite de lancer plusieurs fois des refresh avec des setwait bloqué
                            if (!currentProp.isRefreshingFile) {
                                currentProp.isRefreshingFile = true;
                                RefreshFile();
                            }
                            fieldRefresh.refreshFld = false;
                            fieldRefresh.refreshFldBkm = false;
                            fieldRefresh.refreshFldPopup = false;
                        }
                        else {
                            // Refresh du header de la fiche
                            if (reloadHeader) {
                                RefreshHeader();

                                fieldRefresh.refreshFld = false;
                            }
                            else if (reloadFileHeader && engineObject.ModalDialog == null) {
                                if (oUpdatedRecord != null && isBkmFile(oUpdatedRecord.tab)) {
                                    rldPrtInfo(oUpdatedRecord.tab, oUpdatedRecord.fid);
                                }
                                else {
                                    RefreshFileHeader();
                                }
                            }

                            // Refresh du detail (signets) de la fiche
                            if (reloadDetail && engineObject.ModalDialog == null) {
                                RefreshAllBkm();    // Reload toute l'iframe des bkm

                                fieldRefresh.refreshFldBkm = false;
                            }
                            else {
                                if (reloadFileHeader && engineObject.ModalDialog != null) {
                                    if (oCreatedRecord != null) {
                                        ReloadBkm(oRes, oCreatedRecord);
                                    }
                                    else {
                                        ReloadBkm(oRes, oUpdatedRecord);
                                    }
                                }
                                else if (oCreatedRecord != null && isBkmFile(oCreatedRecord.tab)) {
                                    ReloadBkm(oRes, oCreatedRecord);
                                }
                                else if (oUpdatedRecord != null && isBkmFile(oUpdatedRecord.tab)) {
                                    //ReloadBkm(oRes, oUpdatedRecord);
                                }
                                else if (oUpdatedRecord != null && isBkmDisc(oUpdatedRecord.tab)) {
                                    refreshComm(oUpdatedRecord.tab, oUpdatedRecord.fid);
                                }
                                else
                                    ReloadBkm(oRes);     // Reload des signets demandés
                            }

                            var tab = 0, fid = 0;
                            if (oCreatedRecord) {
                                tab = oCreatedRecord.tab;
                                fid = oCreatedRecord.fid;
                            }

                            else if (oUpdatedRecord) {
                                tab = oUpdatedRecord.tab;
                                fid = oUpdatedRecord.fid;
                            }
                            else if (oDeletedRecord) {
                                tab = oDeletedRecord.mainTab;
                                fid = oDeletedRecord.fid;
                            }


                            if (document.getElementById("bkmCntFilter_" + tab) && document.getElementById("bkmCntFilter_" + tab).innerHTML != "") {

                                loadBkmBar(nGlobalActiveTab, top.GetCurrentFileId(nGlobalActiveTab), false, { noReload: true, uptCmptTab: tab });
                            }


                        }
                        break;
                }
            }
        }

        // Raffraichies les valeurs des rubriques affichées
        if (fieldRefresh.refreshFld || fieldRefresh.refreshFldBkm || fieldRefresh.refreshFldPopup) {
            var modalFrame = null;
            if (engineObject.ModalDialog != null && engineObject.ModalDialog.modFile != null)
                modalFrame = engineObject.ModalDialog.modFile;
            LoadChgFld(oRes, true, modalFrame);
        }
        else if (fieldRefresh.refreshFldHead) {

            var modalFrame = null;
            if (engineObject.ModalDialog != null && engineObject.ModalDialog.modFile != null)
                modalFrame = engineObject.ModalDialog.modFile;

            LoadChgFldHead(oRes, true, modalFrame, nGlobalActiveTab);

        }
        // Si la fiche n'est pas rafraichie et s'il n'y a pas de règle dépendant de ce champ
        //  =>  Affiche le cadre vert de maj sauf pour les checkbox et reloadList
        if (!reloadList && !reloadHeader && !reloadFileHeader && (!lstDescIdRuleUpdated || lstDescIdRuleUpdated.length <= 0))
            FlagEditAndFocus(engineObject, editorObjectName, srcEltId);

        // Affichage MsgBox
        ShowProcMessage(oRes);

        // LaunchPage
        ShowProcPage(oRes);

        if (engineObject.SuccessCallbackFunction != null && typeof (engineObject.SuccessCallbackFunction) == 'function') {
            var callbackObject;
            if (oCreatedRecord != null)
                callbackObject = Object.assign({}, oCreatedRecord, { isMiddleFormula: isMiddleFormula });
            else if (oUpdatedRecord != null)
                callbackObject = Object.assign({}, oUpdatedRecord, { isMiddleFormula: isMiddleFormula });
            else
                callbackObject = oRes;


            if (engineObject.SuccessCallbackFunction.name
                && engineObject.SuccessCallbackFunction.name == "RefreshFile"
                && currentProp.isRefreshingFile) {
                currentProp.isRefreshingFile = true;

            }
            else
                engineObject.SuccessCallbackFunction(callbackObject);
        }

    }
    catch (ex) {
        engineObject.Clear();
        // eTools.log.inspect({ 'Message': "eEngine::UpdateTreatmentReturn(oRes, engineObject, afterUpdate)", 'Exception': ex });
    }
    finally {
        try {
            if (updaterView != "CALENDAR" && typeof (top.setWait) == 'function') {
                //top.setWait(false);
            }
            //if (updaterView == "KANBAN") {
            //    oWidgetKanban.setWait(false);
            //}
        }
        catch (exp1) {
            // eTools.log.inspect({ 'Message': "eEngine::UpdateTreatmentReturn(oRes, engineObject, afterUpdate)", 'Exception': exp1 });
        }
    }
}

function fldUpdEngine(descid) {
    this.descId = descid;

    this.newValue = '';
    this.newLabel = ''; //pourquoi pas newDisplayValue ?
    this.forceUpdate = '';
    this.readOnly = false;

    this.multiple = false;
    this.popId = '';
    this.popupType = '';

    this.boundDescId = '';
    this.boundPopup = '';
    this.boundValue = '';

    this.treeView = false;
    // HLA - Par defaut, sans attribut chgedVal, la valeur null indique à engine qu'il faut mettre à jour la valeur Sinon, interpréte le 0 ou 1 pour la mise à jour
    this.chgedVal = null;

    /* Propriètés non présente dans la méthode GetSerialize */
    this.isLink = false;

    this.obligat = false;
    this.isInRules = false;
    this.hasMidFormula = false;

    this.oldValue = null;
    this.oldLabel = null;
    this.format = null;
    this.cellId = null;
    this.label = null;
    this.lib = "";

    this.GetSerialize = function () {

        // ATTENTION EN CAS DE MODIFICATION DE LA STRUCTURE 
        // METTRE A JOUR eFileManager.ashx

        var sepValue = '$:$';      // Séparateur entre le nom et ça valeur
        var sepParam = '$|$';      // Séparateur entre les différents param
        var sepValueRepl = '<#$#:#$#>';      // Valeur qui remplace Séparateur entre le nom et ça valeur
        var sepParamRepl = '<#$#|#$#>';      // Valeur qui remplace Séparateur entre les différents param

        var result = 'did' + sepValue + this.descId;

        this.newValue = this.newValue || "";

        this.newLabel = this.newLabel || "";

        result += sepParam + 'newVal' + sepValue + this.newValue.replace(sepParam, sepParamRepl).replace(sepValue, sepValueRepl);
        if (this.newLabel != null && this.newLabel != '')
            result += sepParam + 'newLab' + sepValue + this.newLabel.replace(sepParam, sepParamRepl).replace(sepValue, sepValueRepl);

        if (this.forceUpdate != null && this.forceUpdate != '')
            result += sepParam + 'forceUpd' + sepValue + ((this.forceUpdate || this.forceUpdate == '1') ? '1' : '0');

        if (this.readOnly)
            result += sepParam + 'readonly' + sepValue + '1';

        if (this.multiple != null && this.multiple)
            result += sepParam + 'mult' + sepValue + '1';
        if (this.popId != null && this.popId != '' && this.popId != '0') {
            result += sepParam + 'popid' + sepValue + this.popId;
            result += sepParam + 'poptyp' + sepValue + this.popupType;
        }

        if (this.boundDescId != null && this.boundDescId != '' && this.boundDescId != '0') {
            result += sepParam + 'bndid' + sepValue + this.boundDescId;
            if (this.boundPopup != null)
                result += sepParam + 'bndtyp' + sepValue + this.boundPopup;
            if (this.boundValue != null)
                result += sepParam + 'bndval' + sepValue + this.boundValue;
        }

        if (this.treeView)
            result += sepParam + 'treeview' + sepValue + '1';

        if (this.chgedVal != null)
            result += sepParam + 'chgedval' + sepValue + (this.chgedVal ? '1' : '0');

        if (this.isB64)
            result += sepParam + 'isb64' + sepValue + '1';

        return result;
    };
}

function eEngine() {
    this.parameters = null; 		// Paramètres (tableau de updaterParameter)
    this.fields = null; 		    // Paramètres des fields

    this.SuccessCallbackFunction = null;
    this.ErrorCallbackFunction = null;
    this.ErrorCustomAlert = null;

    // modal dialog représentant la modaldialog dans laquelle est affichée la fiche (si affichage en popup)
    this.ModalDialog = null;
    // bloque la réactualisation de la liste ou du signet en arrière plan suite à la validation et fermeture de template
    this.ReloadNothing = false;
    // indique si on se positionne sur une suppression
    this.DeleteRecord = false;
    // indique si on se positionne sur une fusion
    this.MergeRecord = false;
    // indique si l'appel vient d'un formulaire
    this.Formular = false;

    this.async = true;
    this.Init = function () {
        this.parameters = new Array();
        this.fields = new Array();
    };

    this.Clear = function () {
        this.parameters = null;
        this.fields = null;
    };

    var that = this;

    // Ajout ou modification d'un field à mettre à jour
    this.AddOrSetField = function (fld) {
        if (!fld.descId)
            return;

        this.fields[fld.descId] = fld;
    };

    // Ajout ou modification d'un paramètre
    this.AddOrSetParam = function (name, value) {
        if (typeof (value) == "boolean")
            this.parameters[name] = value ? "1" : "0";
        else
            this.parameters[name] = value;
    };

    // Recuperation de valeur
    this.GetParam = function (name) {
        if (name in this.parameters)
            return this.parameters[name];
        return '';
    };

    this.UpdateLaunch = function () {

        var currentViewDoc = document;
        var currentView = getCurrentView(currentViewDoc);

        // Sauvegarde l'elt déclencheur
        var editorObjectName = this.GetParam('jsEditorVarName');
        if (editorObjectName != '') {
            var editorObject = window[editorObjectName];
            if (!editorObject) {
                try {
                    editorObject = eval(editorObjectName);
                }
                catch (ex) { }
            }
            // HLA - UpdateLaunch peu être rappelé plusieurs fois après une validation (de formule du milieu, de suppression, etc.)
            // Ainsi, pour eviter d'écraser ce param avec un eventuel nouveau SrcElement qui n'a rien avoir avec le SrcElement d'origine.
            // Du coup, j'ajoute le test si la valeur n'existe pas déja dans la collection
            if (editorObject && typeof (editorObject.parentPopup) != 'undefined' && typeof (this.parameters['parentPopupSrcId']) == 'undefined')
                this.AddOrSetParam('parentPopupSrcId', editorObject.GetSourceElement().id);
            //Demande #75 678
            if (editorObject && editorObject.getData && typeof (editorObject.getData) == 'function') {
                var oldValue = editorObject.value;
                var newValue = editorObject.getData();
                if (oldValue != newValue) {
                    var nodeSrcElement = editorObject.getSrcElement();
                    if (nodeSrcElement) {
                        var headerElement = document.getElementById(nodeSrcElement.getAttribute("ename"));
                        //Champ Obligatoire
                        var obligat = getAttributeValue(nodeSrcElement, "obg") == "1"
                        if (obligat && newValue == '') {

                            eAlert(0, top._res_372, top._res_373.replace('<ITEM>', getAttributeValue(headerElement, "lib")));
                            editorObject.setData(oldValue);
                            return;
                        }
                    }
                }
            }
        }


        // Pas d'écran grisé lorsqu'on déplace une fiche Planning et popup sur mode list planning
        //if (currentView != "CALENDAR" && currentView != "KANBAN") {
        //    if (typeof (top.setWait) == 'function')
        //        top.setWait(true);
        //}
        //if (currentView == "KANBAN") {
        //    this.AddOrSetParam("fromKanban", "1");
        //    //oWidgetKanban.setWait(true);
        //}

        try {

            var oEngine = this;

            if (this.DeleteRecord)
                var url = "mgr/eDeleteManager.ashx";
            else if (this.MergeRecord) {
                var url = "mgr/eMergeManager.ashx";

                oEngine.IsMerge = true;
            }
            else if (this.Formular)
                var url = "mgr/eUsrFrmManager.ashx";
            else
                var url = "mgr/eUpdateFieldManager.ashx";

            var oUpdater = new eUpdater(url, null);
            for (var key in this.parameters) {
                if (typeof (this.parameters[key]) != 'function') {
                    oUpdater.addParam(key, this.parameters[key], "post");
                }
            }

            if (oEngine.IsMerge) {
                oEngine.MergeInfos = that.parameters;
            }

            var nbFld = 0;
            for (var key in this.fields) {
                if (typeof (this.fields[key]) != 'function') {
                    oUpdater.addParam('fld_' + nbFld, this.fields[key].GetSerialize(), "post");
                    nbFld++;
                }
            }

            //cf comment commit
            oUpdater.ErrorCallBack = function (oRes) {

                ErrorUpdateTreatmentReturn(oRes, oEngine);

            };

            /*
            //NHA : Correction bug #72930 : refresh le BKM en cas de non envoi de mail sur la page 1 par défaut
            oUpdater.ErrorCallBack = function (oRes) {
                debugger;
                //RefreshFile();
                ReloadBkm(oRes);
            };
            */

            if (typeof (that.ErrorCustomAlert) == "function") {
                oUpdater.ErrorCustomAlert = that.ErrorCustomAlert;
            }

            oUpdater.asyncFlag = this.async;
            oUpdater.send(function (oRes) { UpdateTreatmentReturn(oRes, oEngine); });
        }
        catch (ex) {
            console.log(ex);
        }
    };

    this.ShowOrmMiddleConfirm = function (oConfirm) {
        if (oConfirm == null) {
            this.Clear();
            return;
        }

        var ormId = oConfirm.getAttribute('id');
        var ormUrl = getXmlTextNode(oConfirm.getElementsByTagName("url")[0]);
        var ormUpdates = getXmlTextNode(oConfirm.getElementsByTagName("ormupdates")[0]);

        // On conserve l'ormid et les updates
        this.AddOrSetParam("ormId", ormId);
        this.AddOrSetParam("ormUpdates", ormUpdates);

        var localEngineOrmWait = top.engineOrmWait = new Object();

        var oEngine = this;
        localEngineOrmWait.Engine = oEngine;

        var title = "";		// Demande des architectes, titre de la fenêtre vide
        var modal = new eModalDialog(title, 0, ormUrl, 550, 500);
        localEngineOrmWait.Modal = modal;

        modal.ErrorCallBack = function () {
            try {
                oEngine.cancelMiddleConfirm();
            } finally { modal.hide(); }
        };
        modal.addParam("ormId", ormId, "post");
        modal.hideCloseButton = true;
        modal.show();

        // Pour test :)
        //modal.addButton(top._res_29, function () { oEngine.validOrmMiddleConfirm(modal, false, null); }, "button-gray"); // Annuler
        //modal.addButton(top._res_28, function () { oEngine.validOrmMiddleConfirm(modal, true, 'test'); }, "button-green"); // Valider
    };

    this.validOrmMiddleConfirm = function (modal, validResult, urlResult) {
        try {
            if (validResult) {
                this.AddOrSetParam('ormResponseObj', urlResult);
                this.UpdateLaunch();
            } else {
                this.cancelMiddleConfirm();
            }
        }
        catch (err) {

        }
        finally {
            // On retire la modal
            modal.hide();
        }
    }

    this.ShowCheckAdr = function (descid, adrtoupd, adrnoupd) {
        var oEngine = this;
        var modal = new eModalDialog(top._res_961, 0, "eAdrCheck.aspx", 550, 500);
        modal.ErrorCallBack = function () { modal.hide(); };
        modal.addParam("descid", descid, "post");
        modal.addParam("adrtoupd", adrtoupd, "post");
        modal.addParam("adrnoupd", adrnoupd, "post");
        modal.show();

        modal.addButton(top._res_29, function () { oEngine.actionCheckAdr(modal, true); }, "button-gray"); // Annuler
        modal.addButton(top._res_28, function () { oEngine.actionCheckAdr(modal, false); }, "button-green"); // Valider
    };

    this.actionCheckAdr = function (modal, btnCancel) {
        if (!modal) {
            alert('eEngine.actionCheckAdr - TODO');
            return;
        }

        try {
            var ifrm = modal.getIframe();

            if (!ifrm || !ifrm.GetReturnValue)
                alert('eEngine.validCheckAdr - TODO');

            var adrtoupd = ifrm.GetReturnValue(btnCancel);

            this.AddOrSetParam('engAction', '3');       // EngineAction.CHECK_ADR_OK
            this.AddOrSetParam('adrtoupd', adrtoupd);
            this.UpdateLaunch();
        }
        catch (err) {

        }
        finally {
            modal.hide();
        }
    };

    // Function qui affiche une eConfirm avec 3 bouton Retirer Valider Annuler.
    this.ShowSupMultiOwnerConfirm = function (oConfirm, fileId, fldmultiownerdid, multiownernewval) {
        if (oConfirm == null || fileId == null || typeof fileId == "undefined") {
            this.Clear();
            return;
        }
        var msgType = oConfirm.getAttribute('type');
        var msgTitle = getXmlTextNode(oConfirm.getElementsByTagName("title")[0]);
        var msgDescription = getXmlTextNode(oConfirm.getElementsByTagName("desc")[0]);
        var msgDetail = getXmlTextNode(oConfirm.getElementsByTagName("detail")[0]);

        var oModCfm = new eModalDialog(msgTitle, 1, null, 500, 300);
        oModCfm.textClass = "confirm-msg";
        oModCfm.setMessage(msgDescription, msgDetail, msgType);
        oModCfm.show();
        oModCfm.adjustModalToContent(40);
        var oEngine = this;
        oModCfm.addButtonFct(top._res_29, function () { oEngine.cancelSupConfirm(); oModCfm.hide(); }, 'button-green');
        oModCfm.addButtonFct(top._res_19, function () { oEngine.validSupConfirm(); oModCfm.hide(); }, 'button-gray');
        oModCfm.addButtonFct(top._res_6387, function () { oEngine.validSupMultiOwnerConfirm(fileId, fldmultiownerdid, multiownernewval); oModCfm.hide(); }, 'button-gray');
    };

    this.ShowSupPpConfirm = function (fldMainDisplayVal) {
        var paramWin = top.getParamWindow();
        var objThm = JSON.parse(paramWin.GetParam("currenttheme").toString());
        var oEngine = this;
        var height = 300;
        //ELAIZ - demande 78919 : agrandissement de la  modale de suppression de PM sur eudonet x 
        if (objThm && objThm.Version > 1) {
            height = 500;
        }
        var eDeletePMConfirm = new eModalDialog(top._res_806, 0, "eConfirmDeletePmDialog.aspx", 500, height);
        eDeletePMConfirm.ErrorCallBack = launchInContext(eDeletePMConfirm, eDeletePMConfirm.hide);
        eDeletePMConfirm.addParam("name", fldMainDisplayVal, "post");
        eDeletePMConfirm.addParam("uid", eDeletePMConfirm.UID, "post");
        eDeletePMConfirm.show();
        // Lien vers une fonction situé sur la modal
        var myFunct = (function (obj, myEngine) {
            return function () {
                try {
                    var myModal = obj.getIframe();
                    var chk = myModal.document.getElementById("chk_chkId_" + obj.UID);

                    if (chk && chk.getAttribute("chk") == "1")
                        myEngine.AddOrSetParam('deletePp', '1');

                    var chkAdrDelete = myModal.document.getElementById("chk_chkAdrDelete_" + obj.UID);

                    if (chkAdrDelete && chkAdrDelete.getAttribute("chk") == "1")
                        myEngine.AddOrSetParam('deleteAdr', '1');

                    myEngine.validSupConfirm();
                }
                catch (e) {
                    return;
                }
            }
        })(eDeletePMConfirm, oEngine);

        eDeletePMConfirm.addButtonFct(top._res_29, function () { oEngine.cancelSupConfirm(); eDeletePMConfirm.hide(); }, "button-green");  // Annuler
        eDeletePMConfirm.addButtonFct(top._res_28, function () { myFunct(); eDeletePMConfirm.hide(); }, "button-gray");  // Valider
    };

    this.ShowCustomConfirm = function (oConfirm, okFct, cancelFct) {

        if (oConfirm == null) {
            this.Clear();
            return;
        }

        var msgType = oConfirm.getAttribute('type');
        var msgTitle = getXmlTextNode(oConfirm.getElementsByTagName("title")[0]);
        var msgDescription = getXmlTextNode(oConfirm.getElementsByTagName("desc")[0]);
        var msgDetail = getXmlTextNode(oConfirm.getElementsByTagName("detail")[0]);

        var oEngine = this;
        if (msgType == '1') {
            eConfirm(1, msgTitle, msgDescription, msgDetail, 450, 200, okFct, cancelFct);
        }
        else {
            eAlert(msgType, msgTitle, msgDescription, msgDetail, 450, 300, cancelFct);
        }
    };

    //Methode appelé pour retirer le user en cours d'un rdv de group
    this.validSupMultiOwnerConfirm = function (fileId, fldmultiownerdid, multiownernewval) {
        this.Clear();

        try {
            var eEngineUpdater = new eEngine();
            eEngineUpdater.Init();

            var fldMultiOwner = new fldUpdEngine(fldmultiownerdid);
            fldMultiOwner.newValue = multiownernewval;
            eEngineUpdater.AddOrSetField(fldMultiOwner);

            eEngineUpdater.AddOrSetParam('fileId', fileId);

            eEngineUpdater.ModalDialog = this.ModalDialog;
            var planningFileModal = null;
            if (this.ModalDialog == null)
                planningFileModal = "";     // Pour obliger la fonction à reload le calendar
            else
                planningFileModal = this.ModalDialog.modFile;
            eEngineUpdater.SuccessCallbackFunction = function (engResult) { onPlanningValidateTreatment(engResult, planningFileModal, true, false); };

            eEngineUpdater.UpdateLaunch();
        }
        finally {
            this.Clear();
        }
    };

    this.validSupConfirm = function () {
        this.AddOrSetParam('validDeletion', '1');
        this.UpdateLaunch();
        return;
    };

    this.cancelSupConfirm = function () {
        this.Clear();
    };

    this.validMergeConfirm = function () {
        this.AddOrSetParam('validMerge', '1');
        this.UpdateLaunch();
        return;
    };

    this.cancelMergeConfirm = function () {
        this.Clear();
    };

    this.ShowMiddleConfirm = function (oConfirm) {
        if (oConfirm == null)
            return;

        var msgType = oConfirm.getAttribute('type');
        var msgTitle = getXmlTextNode(oConfirm.getElementsByTagName("title")[0]);
        var msgDescription = getXmlTextNode(oConfirm.getElementsByTagName("desc")[0]);
        var msgDetail = getXmlTextNode(oConfirm.getElementsByTagName("detail")[0]);

        var oEngine = this;
        if (msgType == '1') {
            var oMod = eConfirm(1, msgTitle, msgDescription, msgDetail, 450, 100,
                function () { oEngine.validMiddleConfirm(oConfirm.getAttribute('did'), true); },
                function () { oEngine.cancelMiddleConfirm(); });
            oMod.adjustModalToContent(40);
        }
        else {
            eAlert(msgType, msgTitle, msgDescription, msgDetail, 450, 300,
                function () { oEngine.cancelMiddleConfirm(); });
        }
    };

    this.validMiddleConfirm = function (descId, forceUpd) {
        var doUpdate = false;
        for (var key in this.fields) {
            if (typeof (this.fields[key]) != 'function') {
                var fld = this.fields[key];
                if (fld.descId + '' == descId + '') {
                    fld.forceUpdate = forceUpd;
                    doUpdate = true;
                    break;
                }
            }
        }

        if (doUpdate) {
            this.UpdateLaunch();
            return;
        }

        this.FlagEdit(this.GetParam('jsEditorVarName'), true, this.GetParam('parentPopupSrcId'));
        this.Clear();
    };

    this.cancelMiddleConfirm = function () {
        this.undoUpd();
        this.FlagEdit(this.GetParam('jsEditorVarName'), true, this.GetParam('parentPopupSrcId'));
        this.Clear();
    };

    // Affiche le cadre vert de maj sauf pour les checkbox
    this.FlagEdit = function (editorObjectName, noEdit, srcId) {
        if (editorObjectName != '') {
            var editorObject = window[editorObjectName];
            if (!editorObject) {
                try {
                    editorObject = eval(editorObjectName);
                }
                catch (ex) { }
            }
            if (editorObject && editorObject.flagAsEdited && editorObject.type != 'eCheckBox' && editorObject.type != 'eBitButton')
                editorObject.flagAsEdited(true, noEdit, srcId);
        }
    };

    this.undoUpd = function () {
        try {
            for (var key in this.fields) {
                if (typeof (this.fields[key]) == 'function')
                    continue;

                var fld = this.fields[key];
                if (fld.cellId == null || fld.oldValue == null)
                    continue;

                var oNode = document.getElementById(fld.cellId);
                if (oNode == null || typeof (oNode) == 'undefined')
                    continue;

                // On reprend les anciennes valeurs
                fld.newValue = fld.oldValue;
                fld.newLabel = fld.oldLabel;
                // Pour eviter la mise à jour du BoundValue inutilement
                fld.boundValue = null;

                editInnerField(oNode, fld);
            }
        }
        catch (exp) {
            // Inutile de remonté l'erreur à ce niveau
        }
    };
}

//*****************************************************************************************************//
//*****************************************************************************************************//
//*** eMain.js
//*****************************************************************************************************//
//*****************************************************************************************************//

var nGlobalActiveTab = 0;  /* Table Active - variable global disponible pour tous les JS */

// Methode qui retourne une chaine indiquant dans quel mode nous nous trouvons
// return : 
//        CALENDAR,
//        CALENDAR_LIST,
//        LIST,
//        FILE_CONSULTATION,
//        FILE_MODIFICATION,
//        FILE_CREATION
//      KANBAN
function getCurrentView(doc) {

    if (typeof doc === "undefined" || !doc) {
        doc = top.document;
    }

    if (doc.getElementById("hidWidgetType")) {
        if (doc.getElementById("hidWidgetType").value == "16")
            return "KANBAN";
    }

    var bCalMainDiv = nodeExist(doc, "CalDivMain");
    var oFileDiv = doc.getElementById("fileDiv_" + nGlobalActiveTab);
    if (oFileDiv == null || typeof (oFileDiv) == 'undefined') {
        var oAllFileDiv = doc.querySelectorAll('*[id^="fileDiv_"]');
        if (oAllFileDiv && oAllFileDiv.length > 0)
            oFileDiv = oAllFileDiv[0];
    }
    var bFileDiv = (oFileDiv != null && typeof (oFileDiv) != 'undefined');

    if (bCalMainDiv) {
        var mixteMode = doc.getElementById("CalDivMain").getAttribute("mixtemode");
        if (typeof (mixteMode) == 'undefined' || mixteMode != '1')
            return "CALENDAR";
        else
            return "CALENDAR_LIST";
    }

    if (!bFileDiv) {
        //if (document.getElementById("hidWidgetType"))
        //    return "LIST_WIDGET";
        //else
        return "LIST";
    }
    else {
        var fileType = oFileDiv.getAttribute("ftrdr");

        switch (fileType) {
            case "2":
                return "FILE_CONSULTATION";
                break;
            case "3":
                return "FILE_MODIFICATION";
                break;
            case "5":
                return "FILE_CREATION";
                break;
            case "8":
                return "ADMIN_FILE";
                break;
        }
    }
}

function getFldEngFromElt(oFieldElt) {

    var headerElement = oFieldElt.ownerDocument.getElementById(oFieldElt.getAttribute("eName"));
    var fldEngine = null;
    var bDate = false;	//GCH - #35859 - Internationnalisation Date - Fiche
    var bNumerique = false; //GCH - #36022 - Internationalisation Numerique - Fiche
    var bMemoSpec = false;
    // Information en cellule d'entête
    if (headerElement) {
        fldEngine = new fldUpdEngine(headerElement.getAttribute("did"));

        fldEngine.isInRules = headerElement.getAttribute("rul") == "1";
        fldEngine.hasMidFormula = headerElement.getAttribute("mf") == "1";

        fldEngine.multiple = headerElement.getAttribute("mult") == "1";
        fldEngine.popId = headerElement.getAttribute("popid");
        fldEngine.popupType = headerElement.getAttribute("pop");

        fldEngine.boundDescId = headerElement.getAttribute("bndid");
        fldEngine.boundPopup = headerElement.getAttribute("bndPop");

        fldEngine.treeView = headerElement.getAttribute("tree") == "1";
        fldEngine.label = GetText(headerElement);

        fldEngine.cellId = oFieldElt.id;
        fldEngine.format = getAttributeValue(headerElement, "frm");
        fldEngine.lib = getAttributeValue(headerElement, "lib");

        //GCH - #35859 - Internationnalisation Date - on permet l'identification des champs au format date pour les convertir au format de la Base de données
        bDate = (fldEngine.format == "2");
        //GCH - #36022 - Internationalisation Numerique - Fiche
        bNumerique = isFormatNumeric(fldEngine.format);
        //
        bMemoSpec = getAttributeValue(headerElement, "efrmr") == "1";
    }
    else if (oFieldElt.getAttribute("did")) {
        fldEngine = new fldUpdEngine(oFieldElt.getAttribute("did"));
    }
    else
        return null;

    // #56970 : Si le champ est de type "Graphique", on retourne null pour que le champ ne soit pas mis à jour
    if (fldEngine.format == "17")
        return null;

    // Cas d'une rubrique MEMO
    var bMemo = false;
    if (oFieldElt.tagName == "TD" && oFieldElt.firstChild) {
        bMemo = oFieldElt.firstChild.tagName == "TEXTAREA";
        bMemo = bMemo || (oFieldElt.firstChild.tagName == "DIV" && oFieldElt.firstChild.id.indexOf("eMEG_") == 0);
        bMemo = bMemo || (oFieldElt.firstChild.tagName == "DIV" && oFieldElt.firstChild.className == "editor-row"); /* 68 13x - Détection du champ Mémo si utilisation de l'éditeur de templates HTML avancé (grapesjs) */
    }

    fldEngine.readOnlyBlank = oFieldElt.getAttribute("readonlyonblank") == "1";
    fldEngine.readOnly = (oFieldElt.getAttribute("ero") == "1");
    fldEngine.obligat = oFieldElt.getAttribute("obg") == "1";

    // HLA - Gestion de l'autobuildname en mode création en popup - Dev #33529
    var chgedVal = getAttributeValue(oFieldElt, "chgedval");
    if (chgedVal != '')
        fldEngine.chgedVal = chgedVal == "1";

    var textType = 3;
    if (oFieldElt.tagName == "TD" && oFieldElt.firstChild && oFieldElt.firstChild.nodeType == textType) {
        fldEngine.newLabel = GetText(oFieldElt);
        fldEngine.newValue = getAttributeValue(oFieldElt, "dbv");
    }

    if (oFieldElt.getAttribute("eudofront") == "1" && oFieldElt.getAttribute("edncat") == "1") {//si de type catalogue pour les formulaires avancés
        if (oFieldElt.getAttribute("edncatsimple") == "1")//si c'est un catalogue simple
            fldEngine.newValue = vueJSInstance[fldEngine.cellId];
        else if (oFieldElt.getAttribute("edncatMultiple") == "1")
            if (vueJSInstance[fldEngine.cellId])
                  fldEngine.newValue = vueJSInstance[fldEngine.cellId].join(";");//Ajout de choix multiple dans le rendu de de formulaire avancée
    }
    //TODO Enchainement des if_else_if_else 
    else if (oFieldElt.tagName == "INPUT") {
        if (oFieldElt.getAttribute("eudofront") == "1" || hasClass(oFieldElt.parentElement, "v-text-field__slot")) {
            if (oFieldElt.type === 'checkbox')
                fldEngine.newValue = oFieldElt.checked ? "1" : "0";
            else if (bDate) {
                //Récupérer la valeur de la date pour un composant date 'edn-date'
                //TODO: modifer l'attribut du composant pour vérifier si c'est un input de type eudo-font au lieu d'utiliser la classe 'v-text-field__slot'
                var timeDate = ""; //ALISTER Demande/Request 93 178 => Laisser vide / Let empty
                var timeElement = oFieldElt.ownerDocument.querySelector("input[id='tm" + fldEngine.cellId+ "']");
                if (timeElement)
                    timeDate = timeElement.value;
                fldEngine.newLabel = eDate.ConvertDisplayToBdd(oFieldElt.value + ' ' + timeDate).trim();
                fldEngine.newValue = fldEngine.newLabel.trim();
            }
            else if (bNumerique)
                fldEngine.newValue = eNumber.ConvertDisplayToBdd(oFieldElt.value);
            else
                fldEngine.newValue = oFieldElt.value;
        }
        else {
            fldEngine.boundValue = oFieldElt.getAttribute("pdbv");

            if (oFieldElt.getAttribute("dbv") == null || oFieldElt.getAttribute("dbv") == '') {
                if (oFieldElt.getAttribute("lnkid") == null || oFieldElt.getAttribute("lnkid") == '') {
                    //GCH - #35859 - Internationnalisation - Fiche
                    if (bDate)
                        fldEngine.newValue = eDate.ConvertDisplayToBdd(oFieldElt.value);
                    else if (bNumerique)
                        fldEngine.newValue = eNumber.ConvertDisplayToBdd(oFieldElt.value);
                    else
                        fldEngine.newValue = oFieldElt.value;
                }
                else {
                    fldEngine.newValue = oFieldElt.getAttribute("lnkid");
                    fldEngine.newLabel = oFieldElt.value;
                }
            }
            else {
                fldEngine.newValue = oFieldElt.getAttribute("dbv");
                //GCH - #35859 - Internationnalisation - Fiche
                if (bDate)
                    fldEngine.newLabel = eDate.ConvertDisplayToBdd(oFieldElt.value);
                else if (bNumerique)
                    fldEngine.newLabel = eNumber.ConvertDisplayToBdd(oFieldElt.value);
                else
                    fldEngine.newLabel = oFieldElt.value;
            }
        }

    }
    else if ((oFieldElt.tagName == "TD" || oFieldElt.tagName == "SPAN") && oFieldElt.firstChild && oFieldElt.firstChild.tagName == "A") {
        // cas de case à cocher
        var oChkBx = oFieldElt.firstChild;
        fldEngine.newValue = oChkBx.getAttribute("chk");
        fldEngine.newLabel = fldEngine.newValue == "1" ? top._res_58 : top._res_59; //oui/non
    }
    else if (oFieldElt.tagName == "TD" && oFieldElt.firstChild && oFieldElt.firstChild.tagName == "SELECT") {
        // cas d'une combobox
        var oSelect = oFieldElt.firstChild;
        // On verifie que la combobox n'est pas vide et est bien initialisée. GLA
        if (oSelect.selectedIndex > -1)
            fldEngine.newValue = oSelect.options[oSelect.selectedIndex].value;
    }
    else if (oFieldElt.tagName == "TD" && oFieldElt.firstChild && oFieldElt.firstChild.tagName == "DIV" && getAttributeValue(oFieldElt.firstChild, "eaction") == "LNKSTEPCAT") {
        var oSelected = oFieldElt.firstChild.querySelector("ul li.selectedValue a");
        fldEngine.newValue = getAttributeValue(oSelected, "dbv");
    }
    else if ((oFieldElt.firstChild && oFieldElt.tagName == "TD" && oFieldElt.firstChild.tagName == "IMG")
        || (oFieldElt.tagName == "TD" && isElementFirstChildEmptyPictureArea(oFieldElt))
        || (oFieldElt.firstChild && oFieldElt.tagName == "SPAN" && oFieldElt.firstChild.tagName == "IMG")) {
        // cas d'une rubrique image
        if (oFieldElt.firstChild.tagName == "IMG" && getAttributeValue(oFieldElt.firstChild, "isb64") == "1") {
            fldEngine.newValue = getAttributeValue(oFieldElt.firstChild, "src");
            fldEngine.isB64 = "1";
        }
        else
            fldEngine.newValue = oFieldElt.getAttribute("dbv");
    }
    else if (oFieldElt.tagName == "TD" && oFieldElt.firstChild && oFieldElt.firstChild.tagName == "DIV"
        && oFieldElt.firstChild.className != "eME_globl" && oFieldElt.firstChild.className != "eME_cglobl" && oFieldElt.firstChild.className != "editor-row") {
        // bouton radio 
        var oRadioDiv = oFieldElt.firstChild;
        fldEngine.newValue = oRadioDiv.getAttribute("rval");
    }
    else if ((oFieldElt.tagName == "DIV" && oFieldElt.getAttribute("planningmultiuser") == '1') || (oFieldElt.tagName == "SPAN" && oFieldElt.getAttribute("usrfrm") == '1')) {
        // planning multi-users ou le catalog des formulaire
        fldEngine.newValue = oFieldElt.getAttribute("dbv");
    }
    else if ((oFieldElt.tagName == "SPAN" || oFieldElt.tagName == "TD")
        && oFieldElt.getAttribute("PjIds") != null && oFieldElt.getAttribute("PjIds") != '') {
        // Cas de la rubrique ATTACHEMENT (XX91)
        fldEngine.newValue = getAttributeValue(oFieldElt, "PjIds");
    }
    // Les champs mémo Formulaire
    else if (oFieldElt.tagName == "DIV") {//KJE tâche 2 334
        fldEngine.newValue = oFieldElt.innerHTML;
    }
    // Les champs mémo Formulaire
    else if (oFieldElt.tagName == "TEXTAREA" && (getAttributeValue(oFieldElt, "efrmr") == "1" || bMemoSpec)) {
        fldEngine.newValue = oFieldElt.value;
    }
    else if (bMemo) {        // TYPE MEMO
        if (nsMain.hasMemoEditors() && nsMain.getMemoEditor("edt" + oFieldElt.id)) {
            var memoData = nsMain.getMemoEditor("edt" + oFieldElt.id).getData();
            fldEngine.newValue = memoData;
        }
    }
    //Demande #75 678: pour les champs de type Mémo Htm, on récupére le contenu de l'input
    else if (oFieldElt.tagName == "TD" && oFieldElt.firstChild && oFieldElt.firstChild.tagName == "INPUT"
        && oFieldElt.childNodes.length > 1 && oFieldElt.childNodes[1].tagName == "IFRAME" && oFieldElt.childNodes[1].className == "eME" && oFieldElt.firstChild.value) {
        fldEngine.newValue = oFieldElt.firstChild.value;
    }

    return fldEngine;
}

var eUserFormAdv = (function () {

    //vérification du format coté client
    var _fields, _nTab, _fileId, _formularType;

    var _isInit = false;

    ///Initilisation
    var initParam = function () {
        if (!_isInit) {
            _nTab = document.getElementById("tab").value;
            _fileId = document.getElementById("fileId").value;
            _formularType = document.getElementById("frmType").value;
            eNumber.SetDecimalDelimiter(document.getElementById("NumberDecimalDelimiter").value);
            eNumber.SetSectionDelimiter(document.getElementById("NumberSectionsDelimiter").value);
            eDate.SetCultureInfo(document.getElementById("CultureInfo").value);
            _isInit = true;
        }
    };

    /// Construit un tableau de fldUpdEngine a partir des données d'un mode fiche

    /// <param name="nTab">Table des champs à récupérer</param>
    /// <param name="nFileId">FileId de la fiche</param>
    /// <param name="lstDescId"> lites des descid qu'on veut recuperer leur fldEngine </param>
    /// <returns></returns>
    function getFieldsInfos(nTab, nFileId, lstDescId) {
        var doc = document;
        if ((typeof (lstDescId) == "undefined") || lstDescId == null) {
            lstDescId = "";
        }

        if (typeof (nFileId) == "undefined")
            nFileId = 0;

        var aFields = new Array();

        var oInputFields;
        if (lstDescId.length == 0) {
            oInputFields = doc.getElementById("fieldsId_" + nTab);

            if ((typeof oInputFields == 'undefined' || oInputFields == null) && eModFile != null && typeof eModFile != 'undefined') {

                doc = eModFile.getIframe().document;
                oInputFields = doc.getElementById("fieldsId_" + nTab);
            }

        }


        if (oInputFields || lstDescId.length > 0) {

            var aFieldsDescid = (lstDescId.length > 0 ? lstDescId : oInputFields.value).split(";");
            for (var nCmptFld = 0; nCmptFld < aFieldsDescid.length; nCmptFld++) {
                var descid = aFieldsDescid[nCmptFld];

                if (lstDescId.length > 0 && (";" + lstDescId + ";").indexOf(";" + descid + ";") < 0) {
                    continue;
                }

                var oField = null;
                if (getTabDescid(getNumber(descid)) != nTab && getNumber(descid) % 100 == 1)
                    oField = doc.querySelector("[id^='COL_" + nTab + "_" + descid + "_'][efld='1']");
                else
                    oField = doc.getElementById("COL_" + nTab + "_" + descid + "_" + nFileId + "_" + nFileId + "_0");

                if (oField == null) {
                    oField = doc.getElementById("COL_" + nTab + "_" + descid + "_NAME_" + nFileId + "_" + nFileId + "_0");
                    if (oField == null || oField.querySelector("img[isb64='1']") == null)
                        continue;
                }

                var fldEngine = getFldEngFromElt(oField);

                if (fldEngine != null) {
                    var bFindDescid = false;
                    for (var i = 0; i < aFields.length; i++) {
                        if (aFields[i].descId == fldEngine.descId) {
                            bFindDescid = true;
                            break;
                        }
                    }

                    if (!bFindDescid)
                        aFields.push(fldEngine);
                }
            }
            //else {
            //    var fileDiv = document.getElementById("fileDiv_" + nTab);
            //    var eltFields = fileDiv.querySelectorAll("[id^='COL_" + nTab + "_'][efld='1']");
            //    for (var i = 0; i < eltFields.length; i++) {
            //        var fldEngine = getFldEngFromElt(eltFields[i]);

            //        if (fldEngine != null)
            //            aFields.push(fldEngine);
            //    }
            //}
        }

        //dans le cas d'une création  de contact, on récupère également les informations en provenance d'adresse.
        if (nTab == 200 && nFileId == 0) {

            // Dans le cas ou on ajoute un contact et qu'on a choisi "Sans adresse",
            // il faudrait de ne pas ajouter les champs de la fiche adresse.

            // Par défaut on les ajoute
            var oWithoutAdrRadio = doc.getElementById("COL_400_492_2");
            var withoutAdrRadioChecked = oWithoutAdrRadio != null && typeof (oWithoutAdrRadio) != "undefined" && oWithoutAdrRadio.tagName == "INPUT" && oWithoutAdrRadio.checked;
            if (!withoutAdrRadioChecked) {
                var adrFields = getFieldsInfos(400, 0, lstDescId);
                if (adrFields != null && adrFields.length > 0)
                    aFields = aFields.concat(adrFields);
            }
        }

        // récupération de PPID, PMID ADRID ParentEVTID
        //var oIParentFields = document.getElementById("PrtTabs_" + nTab)
        //if (oIParentFields) {
        // var aParentInputsIds = oIParentFields.value.split(';');
        //Demande #58240 et #60137 - Cas particulier sur ADDRESS : il n'y a pas la section des rubriques parentes, on utilise donc l'ancien système
        var aParentLength = 0;
        var aParentInputsIds = null;
        var aParentFieldsLabels = null;
        if (nTab == 400) {
            var oIParentFields = doc.getElementById("PrtTabs_" + nTab)
            if (oIParentFields) {
                aParentInputsIds = oIParentFields.value.split(';');
                aParentLength = aParentInputsIds.length;
            }
        } else {
            aParentFieldsLabels = doc.querySelectorAll("table[id^='ftp_'] td[prt='1']");
            aParentLength = aParentFieldsLabels.length;
        }
        //var aParentFieldsLabels = document.querySelectorAll("table[id^='ftp_'] td[prt='1']");
        for (var i = 0; i < aParentLength; i++) {
            var d = NaN;
            if (nTab == 400)
                d = getNumber(aParentInputsIds[i]);
            else
                d = getNumber(getAttributeValue(aParentFieldsLabels[i], "did")) + 1;

            if (!isNaN(d)) {
                if (lstDescId.length > 0 && (";" + lstDescId + ";").indexOf(";" + d + ";") < 0) {
                    continue;
                }

                var oField = GetField(nTab, d);
                if (oField == null) {
                    continue;
                }

                var eFld = getFldEngFromElt(oField);
                if (eFld != null) {
                    if (aFields.findIndex) { //IE12/Chrome/Firefox
                        if (aFields.findIndex(function (f) { return f.descId == eFld.descId }) == -1)
                            aFields.push(eFld);
                    }
                    else { //Version IE11
                        var fieldFound = false;
                        for (var fieldFoundIndex = 0; fieldFoundIndex < aFields.length && !fieldFound; ++fieldFoundIndex) {
                            if (aFields[fieldFoundIndex].descId == eFld.descId)
                                fieldFound = true;
                        }
                        if (!fieldFound)
                            aFields.push(eFld);
                    }
                }
            }
        }
        //}

        return aFields;
    }

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
                //regex à améliorer : //ALISTER => Demande/Request 91067, similaire à la demande 85805 / similar to request 85805
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

    var _errorIsDisplayed = false;
    ///vérifie la validité _field[key].newValue
    function checkFormat(oFieldEngine, errCallBack) {

        //si on veut eviter de surcharger le serveur 
        var infoElm = document.getElementById("COL_" + _nTab + "_" + oFieldEngine.descId);
        var frm = getAttributeValue(infoElm, "frm") + "";
        var newValue = oFieldEngine.newValue + "";

        //Inutile de vérifier si la valeur ou le format ne sont pas renseignés,   
        if (newValue.length == 0 || frm.length == 0)
            return true;

        var isValid = false;

        if (isFormatNumeric(frm)) {
            var trueValue = document.getElementById(oFieldEngine.cellId).value;
            eNumber.ConvertDisplayToBdd(trueValue);
            isValid = eNumber.IsValid();
        }
        else if (frm == FLDTYP.DATE) {
            var trueValue = document.getElementById(oFieldEngine.cellId).value;

            isValid = eValidator.isDateJS(trueValue)
            /*
            eDate.ConvertDisplayToBdd(trueValue);
            isValid = eDate.IsValid();
            */
        }
        else
            isValid = eValidator.isValid({ value: newValue, format: frm });

        if (!isValid) {

            //6275, "Format incorrect"
            var title = top._res_6275;
            //2021,"La valeur saisie ""<VALUE>"" ne correspond pas au format et/ou à la longueur de la rubrique <FIELD>"
            var errorMessage = top._res_2021.replace("<VALUE>", oFieldEngine.newValue).replace("<FIELD>", infoElm.innerHTML.replace("*", ""));
            if (!_errorIsDisplayed) {
                _errorIsDisplayed = true;
                eAlert(1, title, errorMessage, null, null, null,
                    function () {
                        eUserFormAdv.ErrorAlertClosed();
                        document.getElementById(oFieldEngine.cellId).focus();
                    }
                );
            }
            return false;
        }

        return true;
    }

    ///Vérifie la validité du formulaire
    function isValidForm() {
        $("#g-recaptcha-error").html("");
        //Captcha google V2 validation
        //TODO: Ajouter la validation du captcha avec VueJS
        var $recaptcha = document.querySelector('#g-recaptcha-response');
        if ($recaptcha && (!$recaptcha.value || $recaptcha.value.length == 0)) {
            $("#g-recaptcha-error").html("<span style=\"color:red;font-size:larger;\">" + _res_2776 +"</span>");
            return false;
        }
        //champs saisie formulaire
        _fields = getFieldsInfos(_nTab, _fileId);

        var obligatFields = "";
        var chckObliNotCoched = false;
        for (var key in _fields) {
            //rubrique obligatoires
            if (_fields[key].obligat == true && _fields[key].newValue == "") {
                var label = document.getElementById("COL_" + _nTab + "_" + _fields[key].descId);
                if (label != null) {

                    var mylab = label.innerHTML.replace("*", "");
                    var div = document.createElement("div");
                    div.innerHTML = mylab;
                    mylab = div.textContent || div.innerText || mylab;

                    obligatFields += mylab + "<br />";
                }
            }

            if (_formularType == "1" && _fields[key].obligat == true && _fields[key].newValue == "0") {
                chckObliNotCoched = true;
            }

            if (!checkFormat(_fields[key]))
                return false;
        }

        //rubriques obligatoires
        if (chckObliNotCoched) {
            eAlert(1, top._res_6377, top._res_2657);
            return false;
        }
        else if (obligatFields.length > 0) {
            eAlert(1, top._res_6377, top._res_6564, obligatFields);
            return false;
        }

        //le formulaire est valide
        return true;
    };

    function disableSubmit(bDisable) {
        var oSubmited = document.getElementById("sub");
        var oBtn = document.querySelector(".eButtonCtrl");

        if (oSubmited) {
            if (bDisable)
                oSubmited.value = "1";
            else
                oSubmited.value = "0";
        }

        if (oBtn) {
            if (bDisable)
                oBtn.style.backgroundColor = "#B5B5B5";
            else
                oBtn.style.backgroundColor = "";
        }
    }

    //Poste les données du formulaire
    function submitFormular(isOnlinePaymentBtn, onlinePaymentParams) {
        if (document.getElementById("sub").value == "1")
            return;

        disableSubmit(true);

        //IsPostBack
        document.getElementById("re").value = "1";
        var form = document.getElementById("userForm");

        //Initialisation
        var eEngineUpdater = new eEngine();
        eEngineUpdater.Init();
        eEngineUpdater.Formular = true;

        //Ajoute les champs
        forEach(getFieldsInfos(_nTab, _fileId), function (fld) {
            eEngineUpdater.AddOrSetField(fld);
        });

        //Ajoute les paramètres spécifiques
        var oD = document.getElementById("ContextPanel");
        if (oD) {
            var lstInput = oD.getElementsByTagName("input");

            var arrlstInput = Array.prototype.slice.call(lstInput);

            forEach(arrlstInput, function (inpt) {
                eEngineUpdater.AddOrSetParam(inpt.name, inpt.value);
            });


            if (_fileId <= 0) {
                var sCaptcha = document.getElementById("txt_captcha");
                if (!sCaptcha)
                    sCaptcha = "";
                else
                    sCaptcha = sCaptcha.value;

                eEngineUpdater.AddOrSetParam("Captcha", sCaptcha);

                
            }

            if (isOnlinePaymentBtn) {
                eEngineUpdater.AddOrSetParam("isOnlinePaymentBtn", isOnlinePaymentBtn ? "1" : "0");
                eEngineUpdater.AddOrSetParam("onlinePaymentParams", JSON.stringify(onlinePaymentParams));
            }

            //on ajoute recaptcha google
            if (_formularType == '1') {
                var sReCaptcha = document.getElementById("g-recaptcha-response");
                if (!sReCaptcha)
                    sReCaptcha = "";
                else
                    sReCaptcha = sReCaptcha.value;

                eEngineUpdater.AddOrSetParam("reCaptcha", sReCaptcha);
            }
        }

        //Méthode de callback de succès
        eEngineUpdater.SuccessCallbackFunction = function (engResult, oRes) {

            disableSubmit(false); //réactive le post
            setWait(false);
            var success = (getXmlTextNode(oRes.getElementsByTagName("success")[0]) == "1");
            if (success) {
                var mode = getXmlTextNode(oRes.getElementsByTagName("mode")[0])
                if (mode == "DIALOG") {
                    if (vueJSInstance) {
                        vueJSInstance.showDialog = true;
                        var dialogType = getXmlTextNode(oRes.getElementsByTagName("dialogType")[0]);
                        if (dialogType == "AMOUNT") {
                            vueJSInstance.dialogText = vueJSInstance.invalidMontantMsg;
                            vueJSInstance.dialogTitle = vueJSInstance.invalidMontantTitle;
                            var redirectionUrl = getXmlTextNode(oRes.getElementsByTagName("redirectionUrl")[0]);
                            vueJSInstance.redirectionUrl = redirectionUrl;
                        }
                    }
                }
                else if (mode == "MSG") {

                    var msg = getXmlTextNode(oRes.getElementsByTagName("msg")[0]);
                    var css = getXmlTextNode(oRes.getElementsByTagName("css")[0]);

                    var oDoc = document.getElementById("ContainerPanel");
                    if (oDoc) {
                        oDoc.innerHTML = msg;
                        //et le css custom si besoin
                        if (css.length > 0)
                            addCSSText(css);
                    }

                }
                else if (mode == "REDIRECT") {
                    var sURL = getXmlTextNode(oRes.getElementsByTagName("url")[0]);
                    top.location = sURL;
                }
            }

        };

        //Message d'alerte spécifique pour les formulaires
        // erreur de l'appel a l'updater
        eEngineUpdater.ErrorCustomAlert = function (oObjErr, fctCallBack) {
            disableSubmit(false);

            window.scrollTo(0, 0); // #53522
            eAlert(oObjErr.Type, oObjErr.Title, oObjErr.Msg, oObjErr.DetailMsg);

            if (typeof (fctCallBack) == "function") {
                fctCallBack();
            }

            setWait(false);
        };

        //Message d'alerte spécifique pour les formulaires
        // erreur du retour de l'engine
        eEngineUpdater.ErrorCallbackFunction = function (engResult) {
            disableSubmit(false);
        };

        eEngineUpdater.AddOrSetParam("action", "UPDTFORM");
        eEngineUpdater.UpdateLaunch();  //AAAA => Il y a un un set wait dedans
    }

    return {
        InitFormularAdvCaptcha: function (site_key, lang) {
            var that = this;
            var captchContainer = document.querySelector('#reCaptchContainer');
            if (captchContainer) {
                grecaptcha.render('reCaptchContainer', {
                    'sitekey': site_key,
                    'callback': that.ReCaptchaCallback,
                    lang: lang,//Pour la langue audio
                    theme: 'light', //light or dark    
                    type: 'image',// image or audio    
                    size: 'normal'//normal or compact    
                });

                //Tâche #3 081: on ajoute l'aspect obligatoire à la validation du formulaire
                grecaptcha.ready(function () {
                    var $recaptcha = document.querySelector('#g-recaptcha-response');
                    if ($recaptcha) {
                        //$recaptcha.setAttribute("required", "required");
                    }
                });
            }
         
        },
        //Appelé par le code pour indiquer que la fenetre d'erreur est fermée
        ErrorAlertClosed: function () {
            _errorIsDisplayed = false;
        },
        ReCaptchaCallback: function (response) {
            if (response !== '') {
                $("#g-recaptcha-error").html("");
                //TODO: ajouter un traitement coté front lorsque l'appel du captcha est réussi
            }
        },


        SubmitForm: function (isOnlinePaymentBtn, onlinePaymentParams) {

            try {
                setWait(true);
               
                initParam();
                // Les champs obligatoires sont pas encore gérés pour  les formulaire
                //on valide le formulaire en vue d'abbord
                if (vueJSInstance.$refs.form.validate() && isValidForm()) {
                    submitFormular(isOnlinePaymentBtn, onlinePaymentParams);
                }
                else
                    setWait(false);

            }
            catch (e) {
                console.log(e);
                setWait(false);
            }
            finally {
                disableSubmit(false);
                return false;
            }
           
        },

    }
})();


//resources
var fr = ({
    ednRequired: 'Merci de remplir ce champ',
    invalidMsg: {
        email: 'Vous devez entrer une adresse mail valide.',
        phone: 'Vous devez entrer un numéro de téléphone valide.',
        url: 'Vous devez entrer une url valide.',
        "switch": 'Merci de bien vouloir activer le switch'
    },
    validateRes: 'Valider',
    pasteContentRes: 'Url copiée',
    placeHolder: {
        ednUrl: 'http://',
        ednCopyPaste: '<script src="https:// >'
    }
});

//resources
var en = ({
    ednRequired: 'Please fill in this field',
    invalidMsg: {
        email: 'You must enter a valid email address.',
        phone: 'You must enter a valid phone number.',
        url: 'You need to enter a valid url.',
        "switch": 'Please activate the switch'
    },
    validateRes: 'Validate',
    pasteContentRes: 'Copied url',
    placeHolder: {
        ednUrl: 'http://',
        ednCopyPaste: '<script src="https:// >'
    }
});