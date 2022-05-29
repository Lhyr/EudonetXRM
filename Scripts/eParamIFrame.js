//*****************************************************************************************************//
//*****************************************************************************************************//
//*** HLA - 09/2011 - Gestion du chargement des MRU dans la fenêtre de param
//*****************************************************************************************************//
//*****************************************************************************************************//

/*** CHARGEMENT INITIAL DE LA PAGE ***/
function OnLoadParam() {
    var mruValues, elem;
    var oLink;

    // Charge tous les MRU
    top.bIsParamLoaded = "1";

    //Charge MRU
    LoadAllMru();

    //Charge les vue de table
    LoadViewTab();

    // Calcul du nombre de rows par liste en automatique
    var rows = GetNumRows();

    SetParam("Rows", rows);

    if (typeof (top.GetNbFavLnksPerCol) == "function")
        SetParam("nbFavLnkPerCol", top.GetNbFavLnksPerCol());
}

// Récupére la valeur de l'input (le param)
function GetParam(paraName) {
    var elem = document.getElementById(paraName);

    if (!elem)
        return '';

    return elem.value;
}

// Récupére les MRU de la rubrique passée en paramètre
function GetMruParam(descId) {
    var mruMode = document.getElementById('MruMode');
    if (mruMode == null || typeof (mruMode) == 'undefined' || mruMode.value <= 0)
        return '';

    var isTab = (descId % 100 == 0);
    var elem = document.getElementById('MRU_' + descId);

    // Si les MRU du descid n'a pas encore été charge
    if (!elem) {
        var loadFldMru = new eUpdater('eParamIFrame.aspx', 0);
        loadFldMru.asyncFlag = false;

        //BSE #51 494  on vérifi si on veut récuperer les MRU d'une table ou d'une rubrique 
        if (isTab) {
            loadFldMru.addParam("action", "RefreshMRUFile", "get");
            loadFldMru.addParam("newTabs", descId, "post");
        }
        else {
            loadFldMru.addParam("action", "RefreshMRUField", "get");
            loadFldMru.addParam("descId", descId, "post");
        }

        // Ne rien faire, le message d'alerte suffit
        loadFldMru.ErrorCallBack = function () { };
        loadFldMru.send(UpdRefreshMRU);

        elem = document.getElementById('MRU_' + descId);
    }

    if (!elem)
        return '';

    return elem.value;
}

// Récupére les MRU de la rubrique passé en paramètre
function RefreshUserMessage(newUserMsg) {

    var reloadUserMessage = new eUpdater('eParamIFrame.aspx', 0);
    reloadUserMessage.asyncFlag = false;
    reloadUserMessage.addParam("action", "RefreshUserMessage", "get");
    reloadUserMessage.addParam("um", newUserMsg, "post");
    reloadUserMessage.ErrorCallBack = function () { };
    reloadUserMessage.send(function (oRes) {
        // Param Global       
        var oDivAllGlobal = document.getElementById("GLOBAL");
        var oDataNodeGlobal = oRes.getElementsByTagName("Global");
        if (oDivAllGlobal && oDataNodeGlobal.length == 1) {
            for (var i = 0; i < oDataNodeGlobal[0].childNodes.length; i++) {
                var inputId = oDataNodeGlobal[0].childNodes[i].getAttribute("name");
                var inputVal = oDataNodeGlobal[0].childNodes[i].getAttribute("value");
                AddOrUpdInput(oDivAllGlobal, inputId, inputVal);
                break;
            }
        }
    });
}


// Met à jour la valeur de l'input (le param)
function SetParam(paraName, paramValue) {
    var elem = document.getElementById(paraName);

    if (!elem)
        return false;

    elem.value = paramValue;
    return true;
}

// Met à jour la valeur MRU de l'input (le param)
function SetMruParam(descId, paramValue) {
    var elem = document.getElementById('MRU_' + descId);

    if (!elem)
        return false;

    var bDone = false
    if (paramValue.toString().length > 0) {
        try {
            var pattern = /\$\|UPDATE\|\$$/g;

            var elemToAdd = paramValue.split("$|UPDATE|$")[0];
            if (pattern.test(paramValue)) {

                var arr = elem.value.split("$|$");
                var idx = -1;

                var bFound = arr.some(function (val, i) {
                    if (val.toLowerCase() == elemToAdd.toLowerCase()) {
                        idx = i;
                        return true;
                    }
                }
                );


                if (idx == 0)
                    return true; //si déjà à la 1er place, one fait rien
                else if (idx > 0)
                    arr.splice(idx, 1); //si on le trouve, on le retire

                //Ajout de l'élément en 1er pos et retirage du plus ancien
                arr.unshift(elemToAdd);

                //Si plus de 7, on retire le plus vieux
                if (arr.length > 7)
                    arr.pop();

                elem.value = arr.join("$|$");
                return true;
            }
        }
        catch (e) {
            alert(e);
        }
    }
    if (!bDone)
        elem.value = paramValue;

    return true;
}

// Faire la mise à jour en base avec l'objet ePref avant cette appel
// Chargement des MRU de table choisi
// TabsDescId : DescId des tables à charger séparé par des ;
// Utile ? TypeChange : Changement d'onglet de type : (1) Modification de la selection (Ajouter/Masquer un onglet), (2) Ajout de selection, (3) Selection d'un autre selection
// fct : fonction a exécuter au retour de RefreshMru
function RefreshMRU(tabsDescId, fct) {
    if (tabsDescId == null || typeof (tabsDescId) == 'undefined')
        return;

    tabsDescId = tabsDescId.toString();

    var aTabDescId = tabsDescId.split(";");

    //Ne mettre à jour que les tables principales
    var sNewTabsDescId = "";
    if (aTabDescId.length > 0) {
        for (var nCMptI = 0; nCMptI < aTabDescId.length; nCMptI++) {

            var oTabType = parent.document.getElementById("nvb" + aTabDescId[nCMptI]);

            if (oTabType && oTabType.tagName.toLowerCase() == "li" && oTabType.getAttribute("edntype") == "0") {
                if (sNewTabsDescId.length > 0)
                    sNewTabsDescId += ";";

                sNewTabsDescId += aTabDescId[nCMptI];
            }
        }
    }

    //
    if (sNewTabsDescId.length == 0)
        return;
    else
        tabsDescId = sNewTabsDescId;

    var ednu = new eUpdater("eParamIFrame.aspx", null);
    ednu.asyncFlag = true;
    ednu.addParam("NewTabs", tabsDescId, "post");
    ednu.addParam("action", "RefreshMRUFile", "get");
    //Ne rien faire, le message d'alerte suffit
    ednu.ErrorCallBack = function () { };
    ednu.send(UpdRefreshMRU, fct);
}

// Mise à jour du DOM après avoir tout calculé en .NET
// fct : fonction a exécuter au retour de RefreshMru
function UpdRefreshMRU(oRes, fct) {
    var i, j;
    var tab;
    var name, value;
    var oInput;

    // Param Global
    var oDivAllGlobal = document.getElementById("GLOBAL");
    var oDataNodeGlobal = oRes.getElementsByTagName("Global");
    if (oDivAllGlobal && oDataNodeGlobal.length == 1) {

        for (var i = 0; i < oDataNodeGlobal[0].childNodes.length; i++) {
            var inputId = oDataNodeGlobal[0].childNodes[i].getAttribute("name");
            var inputVal = oDataNodeGlobal[0].childNodes[i].getAttribute("value")

            AddOrUpdInput(oDivAllGlobal, inputId, inputVal);
        }
    }

    // Param Mru Table
    var oDivAllTab = document.getElementById("TABS");
    var oDataNodeMruTab = oRes.getElementsByTagName("MruTab");
    if (oDivAllTab && oDataNodeMruTab.length == 1) {

        for (var i = 0; i < oDataNodeMruTab[0].childNodes.length; i++) {
            oTabNode = oDataNodeMruTab[0].childNodes[i];
            if (oTabNode.length <= 0)
                continue;

            var divTabId = oTabNode.getAttribute("id");
            var oDivTab = document.getElementById(divTabId);
            if (!oDivTab) {
                oDivTab = document.createElement("div");
                oDivTab.setAttribute('id', divTabId);
                oDivAllTab.appendChild(oDivTab);
            }

            var tabDescId = divTabId.replace("TAB_MRU_", "");
            for (var j = 0; j < oTabNode.childNodes.length; j++) {
                var inputId = oTabNode.childNodes[j].getAttribute("name");
                var inputVal = oTabNode.childNodes[j].getAttribute("value");

                AddOrUpdInput(oDivTab, inputId, inputVal);
            }

            LoadMru(tabDescId);
        }
    }

    // Param Mru Field
    var oDivAllFields = document.getElementById("FIELDS");
    var oDataNodeMruField = oRes.getElementsByTagName("MruField");
    if (oDivAllFields && oDataNodeMruField.length == 1) {

        for (var i = 0; i < oDataNodeMruField[0].childNodes.length; i++) {
            var inputId = 'MRU_' + oDataNodeMruField[0].childNodes[i].getAttribute("descId");
            var inputVal = oDataNodeMruField[0].childNodes[i].getAttribute("value");

            AddOrUpdInput(oDivAllFields, inputId, inputVal);
        }
    }

    if (fct != null && typeof (fct) == "function")
        fct();
}

function AddOrUpdInput(conteneur, inputId, inputVal) {
    var oInput = document.getElementById(inputId);

    if (!oInput) {
        // Création de l'élément INPUT
        oInput = document.createElement("input");

        // Intégration dans le DOM
        conteneur.appendChild(oInput);

        setAttributeValue(oInput, "name", inputId);
        setAttributeValue(oInput, "type", 'text');
        setAttributeValue(oInput, "id", inputId);
    }

    // Ajout/Modif des attributs
    if (inputVal != null)
        oInput.value = inputVal;
}

// Ajout d'une nouvelle valeur pour les MenuUserListAdv
function AddMenuUserAdv(vals, libs) {
    var values = new Array;

    // Ajout de la nouvelle valeur dans le tableau
    values[0] = new Array;
    values[0][0] = vals;
    values[0][1] = libs;

    // Reprend les valeurs dans un tableau pour une meilleure manipulation
    for (var i = 0; i < nNbrMaxAdvancedUsr; i++) {
        var oVal = document.getElementById('MenuUserListAdvVal_' + i);
        var oLib = document.getElementById('MenuUserListAdvLbl_' + i);

        if (!oVal || !oLib || oVal.value == '' || oLib.value == '')
            continue;

        // On évite ajouter deux fois la même valeur
        if (oVal.value != vals) {
            var idxNewVal = values.length;
            if (idxNewVal >= nNbrMaxAdvancedUsr)
                break;

            values[idxNewVal] = new Array;
            values[idxNewVal][0] = oVal.value;
            values[idxNewVal][1] = oLib.value;
        }
    }

    // Réaffecte les valeurs dans param
    for (var i = 0; i < values.length; i++) {
        document.getElementById('MenuUserListAdvVal_' + i).value = values[i][0];
        document.getElementById('MenuUserListAdvLbl_' + i).value = values[i][1];
    }
}
