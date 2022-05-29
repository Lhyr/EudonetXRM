//*****************************************************************************************************//
//*****************************************************************************************************//
//*** JBE & SPH - 08/2011 - framework AJAX eudonet XRM
//*** Nécessite eTools.js
//*****************************************************************************************************//
//*****************************************************************************************************//

function updaterParameter(name, value, method) {
    this.name = name;
    this.value = value;
    this.method = method;
}

var bSessionLost = false;
var uptStack = null;

try {
    if (top && !top.window['_updatestack'])
        top.window['_updatestack'] = [];
    uptStack = top.window['_updatestack'];
}
catch (e) {

}

/// <summary>
/// Permet d'effectuer des appels AJAX
/// </summary>
/// <param name="url">Url de l'appel AJAX</param>
/// <param name="type">type de retour la page appelée  0:XML, 1:Text, 2:json</param>
function eUpdater(url, type) {

    this.debugMode = true; // à mettre à true pour afficher certains messages de diagnostic - NE PAS UTILISER EN PRODUCTION !

    //Pointer vers l'objet
    that = this;

    // On fait l'appel, et au retour serveur on met a jour la session
    this.bKeepSessionAlive = true;

    this.sid = timeStamp() + '_' + randomID(20);

    //Type de retour 0: XML (XmlResponse) ou 1: HTML (ResponseText)
    this.type = type;
    this.ErrorCallBack = null;              // Fonction callback d'erreur
    this.ErrorCustomAlert = null;           // Fonction custom pour un affichage spécifique de l'alert d'erreur
    this.url = url; 						// Url de la page à appeler
    this.asyncFlag = true;                  // Par defaut le mode asynchrone en actif

    this.Error = false;                     // Indique si la demande est en erreur
    this.Result = null;                     // objet retourné par l'appel

    this.json = "";                         // flux json, si renseigné, parameters n'est pas pris en compte

    // Génère des messages de diagnostic ; uniquement si this.debugMode = true
    this.trace = function (strMessage) {
        if (this.debugMode) {
            try {
                strMessage = 'eUpdater [URL demandée : ' + this.url + '] (statut : ' + (this.Error ? 'en ERREUR' : 'OK') + ') -- ' + strMessage;

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

    try {
        var request = internalGetHttpRequest(); 		// Objet XMLHTTP
    }
    catch (ex) {
        that.trace("Exception de internalGetHttpRequest : " + ex.message);
    }
    if (request == null) {
        that.trace("ERREUR : internalGetHttpRequest() a retourné null");
    }

    var parameters = new Array(); 		    // Paramètres (tableau de updaterParameter)
    this.uId = window['_uidupdater'];

    var bError = false;

    // la fonction est placé dans le scope de l'eupdater pour que le request créé le soit aussi et que des appels concurent n'écrase pas
    // celui d'un autre eUpdater
    function internalGetHttpRequest() {
        var xmlhttp = false;

        if (window.XMLHttpRequest) { // Mozilla, Safari, ...
            xmlhttp = new XMLHttpRequest();
        } else if (window.ActiveXObject) { // IE 8 and older
            xmlhttp = new ActiveXObject("Microsoft.XMLHTTP");
        }

        return xmlhttp;
    }

    if (!request) {
        return false;
    }

    //Ajout d'un paramètre à poster
    this.addParam = function (name, value, requestType) {
        parameters.push(new updaterParameter(name, value, requestType));
    };

    //Récupération d'un paramètre posté
    this.getParam = function (name) {
        for (var ii = 0; ii < parameters.length; ii++) {
            if (parameters[ii].name == name)
                return parameters[ii].value;
        }

        /*NE FCT PAS SUR CHROME 32*/
        /*
        if (name in parameters)
            return parameters[name];*/
        return '';
    };

    // Mise à jour d'un paramètre passé
    this.setParam = function (name, newValue, requestType) {

        for (var ii = 0; ii < parameters.length; ii++) {
            if (parameters[ii].name == name) {
                parameters[ii] = new updaterParameter(name, newValue, requestType);
                return newValue;
            }
        }
        /*NE FCT PAS SUR CHROME 32*/
        /*if (name in parameters)
            parameters[name] = newValue;
        else
        */
        this.addParam(name, newValue, requestType);
        return newValue;
    }

    this.progressStatus = 0;
    this.progressMessage = "";

    //post
    this.send = function (myFunct) {
        // Init
        var args = arguments;
        var querystringGet = "";
        var querystringPost = "";
        var internalErrorCallBack;
        var internalAlertErrorCallBack;

        //La fonction de callback d'erreur doit être passée à la fonction "send" interne de l'objet eUpdater, les propriétés de cet objet n'étant plus
        // accessible  dans le onreadystatechange
        if (typeof (this.ErrorCallBack) == "function") {
            internalErrorCallBack = this.ErrorCallBack;
        }

        if (typeof (this.ErrorCustomAlert) == "function") {
            internalAlertErrorCallBack = this.ErrorCustomAlert;
        }

        //Ajout de l'id
        this.addParam("_processid", this.uId, "post");

        //Préparation des params
        if (this.json == "") {
            for (i = 0; i < parameters.length; i++) {
                var p = parameters[i];

                if (p.method == "post") {
                    if (querystringPost != "") {
                        querystringPost += "&"
                    }


                    querystringPost += (p.name + "=" + this.encode(p.value + ''));

                }
                else //Get
                {
                    if (querystringGet != "") {
                        querystringGet += "&"
                    }
                    querystringGet += (p.name + "=" + this.encode(p.value + ''));
                }
            }
        }
        else {
            var ob = this.json;

            if (typeof ob == "string")
                ob = JSON.parse(this.json);

            ob._pid = this.uId;
            querystringPost = JSON.stringify(ob);
        }
        //ajout des param en Get dans l'URL
        if (querystringGet != "") {
            querystringGet = "?" + querystringGet
        }

        //Gestion de l'appel
        /*******   DEBUT DE LA FONCTION DE PROCCESS  *****/
        /**  le paramètre  updater représente le eUpdater **/
        request.onreadystatechange = (function (updter) {
            return function () {

                if (request != null && request.readyState == 4) {
                    var objReturn;
                    var objXmlReturn;
                    var oErrorObj;

                    if (request.status == 200) {
                        //
                        var bError = request.getResponseHeader("X-EDN-ERRCODE") == "1";
                        var bSessionLost = request.getResponseHeader("X-EDN-SESSION") == "1";

                        updter.Error = bError;

                        objXmlReturn = request.responseXML;
                        if (type == 1) {
                            objReturn = request.responseText;
                        }
                        else {
                            objReturn = request.responseXML;
                        }

                        //Gestion d'erreur
                        if (bError || bSessionLost) {
                            if (type == 1) {
                                // si type de retour txt, transformation du flux txt en xml
                                oErrorObj = errorXML2Obj(createXmlDoc(objReturn), bSessionLost);
                            }
                            else
                                oErrorObj = errorXML2Obj(objReturn, bSessionLost);
                           
                            if (bSessionLost) {
                                //Message d'avertissement - Valeur "par défaut"                         
                                oErrorObj.Type = (oErrorObj.Type == "") ? "1" : oErrorObj.Type;
                                oErrorObj.Title = (oErrorObj.Title == "") ? top._res_503 : oErrorObj.Title; // votre session a expiré...
                                oErrorObj.Msg = (oErrorObj.Msg == "") ? top._res_6068 : oErrorObj.Msg; // votre session a expiré...détail
                                oErrorObj.DetailMsg = oErrorObj.DetailMsg;
                                oErrorObj.DetailDev = oErrorObj.DetailDev;

                                // remplace le callback intial par un retour à l'accueil
                                internalErrorCallBack = function () {
                                    if (updter.bKeepSessionAlive && typeof (oSession) != "undefined")
                                        oSession.Expire();

                                    top.document.location = "elogin.aspx";
                                }
                            }
                        }
                        else {

                            if (updter.bKeepSessionAlive && typeof (oSession) != "undefined")
                                oSession.KeepAlive();
                        }
                    }
                    // N'affiche pas de message d'erreur si la requête est vidée/interrompue (ex : fenêtre fermée)
                    else if (request.status > 0) {

                        // Gestion des erreurs status != 200
                        bError = true;
                        updter.Error = bError;

                        oErrorObj = new Object();
                        oErrorObj.Type = "1";
                        oErrorObj.Title = top._res_416; // Erreur
                        oErrorObj.Msg = top._res_72; // Une erreur est survenue
                        oErrorObj.DetailMsg = top._res_544; // cette erreur a été transmise à notre équipe technique.
                        oErrorObj.DetailDev = "Code erreur " + request.status + " dans eUpdater.onreadystatechange\n" + request.responseText;

                        objReturn = "";

                        that.trace(oErrorObj.DetailDev);
                    }

                    // debug
                    if (document.getElementById("DebugTextarea") != null) {
                        document.getElementById("DebugTextarea").value = document.getElementById("DebugTextarea").value + "\n\r-----------\n\r" + request.responseText;
                    }

                    //Libération des ressources
                    delete request;
                    request = null;

                    if (bError || bSessionLost) {

                        var okFct;
                        if (typeof (internalErrorCallBack) == "function") {
                            //Error Callback
                            okFct = (function (oParam) {
                                return function () {
                                    internalErrorCallBack(oParam);
                                };
                            })(objReturn);

                        }
                        else {
                            // Callback par défaut
                            okFct = (function (oParam) {
                                return function () {
                                    that.trace("Aucun callback d'erreur défini - setWait(false) utilisé - Type de paramètre : " + typeof (oParam));
                                    try {
                                        setWait(false);
                                    }
                                    catch (ex) {
                                        that.trace("setWait non utilisé : " + ex.message);
                                    }
                                };
                            })(objReturn);
                        }

                        // Affichage de l'erreur
                        if (internalAlertErrorCallBack == null || typeof (internalAlertErrorCallBack) != "function")
                            eAlertError(oErrorObj, okFct);
                        else
                            internalAlertErrorCallBack(oErrorObj, okFct);


                        return;
                    }
                    else if (typeof (myFunct) == "function") {
                        //Passage d'un nombre d'arguments dynamique en passant en premier l'objet xml retourné par la page appelée
                        var tabArgs = new Array();
                        tabArgs.push(objReturn);
                        if (args.length > 1) {
                            for (var i = 1; i < args.length; i++) {
                                tabArgs.push(args[i]);
                            }
                        }

                        //call-back   
                        myFunct.apply(that, tabArgs);


                    }

                    updter.Result = objReturn;


                }

            }
        })(that);
        /*******   FIN DE LA FONCTION DE PROCCESS  *****/

        //Préparation de l'appel
        request.open("POST", this.url + querystringGet, this.asyncFlag);
        //Ajout des params en POST sous forme de datas
        if (this.json == "")
            request.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
        else
            request.setRequestHeader('Content-Type', 'application/json');

        request.setRequestHeader('X-FROM-EUPDATER', '1');
        request.setRequestHeader('X-EUPDATER-SID', this.uId);
        // Appel AJAX



        request.send(querystringPost);
    };

    //Fonction d'encodage
    this.encode = function (strValue) {
        if (strValue == "" || strValue == null)
            return "";

        var strReturnValue = strValue;




        try {
            // HLA - pour éviter l'erreur ci-dessous lors d'un post en .NET, on transforme les '<' et '>' [3] [4], puis les apostrophes après coup (qui échappent à l'encodage) [6]
            // MAB - il faut également gérer le cas du &# (entité HTML numérique, ex : &#39; = ') qui provoque cette erreur
            // Pour cela, on convertit toutes les entités numériques en caractères clairs [1], qui seront réencodés ensuite par encodeURIComponent [5]
            // Cependant, si l'entité n'est pas correcte (ex : &#39 au lieu de &#39;), elle n'est pas encodée ; dans ce cas (qui devrait être marginal),
            // on considère qu'il s'agit d'une erreur/d'une malveillance et on ajoute un espace pour supprimer tout risque [2]
            // Erreur à gérer : [HttpRequestValidationException] A potentially dangerous request.form has been detected (une requête request.form potientiellement dangereuse a été détectée...)
            strReturnValue = this.decodeCharacterReferences(strReturnValue); // [1]
            strReturnValue = strReturnValue.replace(/&#/g, "& #"); // [2]

            // #36751 CRU/SPH : Encodages différents pour &lt;/&gt; et les caractères < et > afin de différencier les balises et les signes
            // car EudoDAL fait un Decode HTML au moment de l'update

            // #42740 - SPH/HLA : utilisation d'une regexp pour le cas des label de champ de fusion contenu dans un lien : dans ce cas
            // ckeditor encode les chevron des balises (ainsi que les '& ' mais les lecteurs de mails semble le gérer) ce qui cause un double encodage
            // et empêche donc la fusion. Pour éviter cela, on modifie le temps de ces traitments les balises label des fusions déjà encodé par ckeditor
            var regLabel = /&lt;(\s*label.*)&gt;(.*)&lt;(\/\s*label\s*)&gt;/g;
            var sOpenTag = "##OPEN##";
            var sCloseTag = "##CLOSE##";

            if (strReturnValue.match(regLabel)) {
                strReturnValue = strReturnValue.replace(regLabel, sOpenTag + "$1" + sCloseTag + "$2" + sOpenTag + "$3" + sCloseTag);
            }

            strReturnValue = strReturnValue.replace(/&lt;/g, "&amp;amp;lt;");
            strReturnValue = strReturnValue.replace(/&gt;/g, "&amp;amp;gt;");

            //SPH  45806
            strReturnValue = strReturnValue.replace(/&quot;/g, "&amp;amp;quot;");
            strReturnValue = strReturnValue.replace(/&nbsp;/g, "&amp;amp;nbsp;");

            strReturnValue = strReturnValue.replace(new RegExp(sOpenTag, "g"), "&lt;");
            strReturnValue = strReturnValue.replace(new RegExp(sCloseTag, "g"), "&gt;");

            strReturnValue = strReturnValue.replace(/</g, "&lt;"); // [3]
            strReturnValue = strReturnValue.replace(/>/g, "&gt;"); // [4] 


            strReturnValue = encodeURIComponent(strReturnValue); // [5]



            strReturnValue = strReturnValue.replace(/'/g, "%27"); // [6]
        }
        catch (e) {
            strReturnValue = escape(strReturnValue);
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

    // Décodage de chaînes en UTF-16, utile pour décoder les entités numériques HTML telles que &#39;
    // Source : http://stackoverflow.com/questions/4292320/use-javascript-regex-to-replace-numerical-html-entities-with-their-actual-charac
    String.fromCharCodePoint = function (/* codepoints */) {
        var codeunits = [];
        for (var i = 0; i < arguments.length; i++) {
            var c = arguments[i];
            if (arguments[i] < 0x10000) {
                codeunits.push(arguments[i]);
            } else if (arguments[i] < 0x110000) {
                c -= 0x10000;
                codeunits.push((c >> 10 & 0x3FF) + 0xD800);
                codeunits.push((c & 0x3FF) + 0xDC00);
            }
        }
        return String.fromCharCode.apply(String, codeunits);
    };

    this.decodeCharacterReferences = function (s) {
        return s.replace(/&#(\d+);/g, function (_, n) {
            ;
            return String.fromCharCodePoint(parseInt(n, 10));
        }).replace(/&#x([0-9a-f]+);/gi, function (_, n) {
            return String.fromCharCodePoint(parseInt(n, 16));
        });
    };

    this.getErrorCode = function (oRes) {
        var strReturnValue = getXmlTextNode(oRes.getElementsByTagName("ErrorCode")[0]);
        return strReturnValue;
    };

    this.getErrorMessage = function (oRes) {
        var strReturnValue = getXmlTextNode(oRes.getElementsByTagName("ErrorDescription")[0]);
        return strReturnValue;
    };

    this.getContent = function (oRes) {
        var strReturnValue = getXmlTextNode(oRes.getElementsByTagName("Content")[0]);
        return strReturnValue;
    };
}

//Crée et retourne l'équivalent d'un XmlDocument
function createXmlDoc(strDoc, bSilent) {
    try {
        if (window.DOMParser) {
            parser = new DOMParser();
            var xmlDoc = parser.parseFromString(strDoc, "text/xml");
        }
        else // Internet Explorer
        {
            var xmlDoc = new ActiveXObject("Microsoft.XMLDOM");
            xmlDoc.async = "false";
            xmlDoc.loadXML(strDoc);
        }
        return xmlDoc;
    }
    catch (exp) {
        if (!bSilent)
            alert(strDoc);
    }
}

///Transforme un flux XML en un objet JS
// oRes : Objet XML
// bEmptyIfFail : si la transformation échoue, fourni un objet "vide"
function errorXML2Obj(oRes, bEmptyIfFail) {


    var oError = new Object();

    try {

        var engineSuccess = getXmlTextNode(oRes.getElementsByTagName("success")[0]);
        oError.Success = engineSuccess;

        if (oError.Success == "1")
            return oError;

        var oErrNodeTitle = oRes.getElementsByTagName("title")[0];
        oError.Title = getXmlTextNode(oErrNodeTitle);

        var oErrNode = oRes.getElementsByTagName("msg")[0];
        oError.Msg = getXmlTextNode(oErrNode);

        var oErrNodeDetail = oRes.getElementsByTagName("detail")[0];
        oError.DetailMsg = getXmlTextNode(oErrNodeDetail);

        var oErrNodeDetailDev = oRes.getElementsByTagName("detaildev")[0];
        oError.DetailDev = getXmlTextNode(oErrNodeDetailDev);

        var errorTyp = 0;
        if (oErrNode != null && typeof (oErrNode) != 'undefined' && oErrNode.getAttribute('errTyp') != 'undefined')
            oError.Type = oErrNode.getAttribute('errTyp');

        return oError;
    }
    catch (e) {
        oError.Success = "0";
        oError.Type = bEmptyIfFail ? "" : "1";
        oError.Title = bEmptyIfFail ? "" : "Erreur sur la page";
        oError.Msg = bEmptyIfFail ? "" : "Erreur imprévue.";
        oError.DetailMsg = bEmptyIfFail ? "" : "";
        oError.DetailDev = bEmptyIfFail ? "" : e.Description;
        return oError;
    }
}
