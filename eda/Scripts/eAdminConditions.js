
// NameSpace pour les fonctions propre à l'admin
var nsConditions = nsConditions || {};




///retourne la modal de la fenêtre de condition
nsConditions.modalWindow = function () {

    var myModDialog = null;
    if (typeof (top.window['_md']['modalConditions']) != 'undefined') {
        myModDialog = top.window['_md']['modalConditions'];
        return myModDialog;
    }
    else {


        return null;
    }
}

//Masque/Affiche les filtres non sélectionnées dans les blocs de rules
nsConditions.showHideSelectedFilter = function () {
    var oLstInp = document.querySelectorAll("a[id^='CHK_ID_'][chk='0']");
    var arrAllEntry = Array.prototype.slice.call(oLstInp);

    var sStyle = "none";
    if (arrAllEntry[0].parentElement.style.display == "none")
        sStyle = "";


    arrAllEntry.forEach(
       function (myFilter) {
           var myDiv = myFilter.parentElement;
           myDiv.style.display = sStyle;
       });
}

nsConditions.refreshConditionsLaunch = function (nDescId ) {

    //Modal de la liste des fiche
    var myConditionModal = top.eTools.GetModal('oConditionListModal');
    if (!(myConditionModal && myConditionModal.isModalDialog))
        return;

    var myDoc = myConditionModal.getIframe().document;

    var mySelectTab = myDoc.getElementById("ddlListTabs");
    var mySelectFilter = myDoc.getElementById("ddlListTypes");

    if (mySelectFilter && mySelectTab) {


        mySelectedTab = mySelectTab.options[mySelectTab.selectedIndex].value;
        if (typeof (nDescId) == "undefined" || nDescId == null)
            nDescId = mySelectTab.options[mySelectTab.selectedIndex].value;


        var mySelectedFilter = mySelectFilter.options[mySelectFilter.selectedIndex].value;

        var upd = new eUpdater("eda/Mgr/eAdminConditionListManager.ashx", 1);
        upd.addParam("action", 1, "post");
        upd.addParam("descid", nDescId, "post");
        upd.addParam("parenttab", mySelectedTab, "post");
        upd.addParam("typefilter", mySelectedFilter, "post");
        upd.addParam("_parentiframeid", "frm_" + myConditionModal.UID, "post");

        upd.send(

            //Maj du tab
            // oRes contient le html de l'admin des propriété du webtab
            function (oRes) {
                try {
                    var myRes = JSON.parse(oRes);

                    if (myRes.Success) {
                        var oDivparts = document.createElement('div');
                        oDivparts.innerHTML = myRes.Html;

                        var replaced = myDoc.getElementById("tableWrapper");
                        replaced.innerHTML = oDivparts.innerHTML;
                    }
                    else {

                        top.eAlert(1, top._res_416, myRes.ErrorMsg);

                    }
                }
                catch (e) {
                    top.eAlert(1, top._res_416, "Impossible de lire la réponse du serveur");

                }

            }

            );
    }
}



/************     Manipulation Bloc des RULES/CONDITIONS  ***********/
//fait apparaitre/disparaitre le select d'opérateur inter filtre
nsConditions.optionOnClick = function (element) {

    var myDoc = document;

    var bChecked = getAttributeValue(element, "chk") == "1";
    var p = element.parentElement;

    var nBlockId = element.id.split("_")[2]; //Format CHK_ID_<idblock>_<idfilter>
    var nIdFilter = element.id.split("_")[3]; //Format CHK_ID_<idblock>_<idfilter>

    var select = p.querySelector("select[id='SEL_OP_" + nBlockId + "_" + nIdFilter + "']");


    if (bChecked) {
        select.style.visibility = "";
    }
    else {
        select.style.visibility = "hidden";
    }


    //On parcours tous les filtres coché du bloc pour maj le display du select : le 1er doit ^tre masqué
    var oLstInp = myDoc.querySelectorAll("a[id^='CHK_ID_" + nBlockId + "_'][chk='1']");
    var arrAllEntry = Array.prototype.slice.call(oLstInp);

    var bFirst = true;
    arrAllEntry.forEach(
        function (myFilter) {

            var nId = myFilter.id.split("_")[3]; //Format CHK_ID_<idblock>_<idfilter>
            var select = myDoc.querySelector("select[id='SEL_OP_" + nBlockId + "_" + nId + "']");
            if (select) {
                if (bFirst)
                    select.style.visibility = "hidden";
                else
                    select.style.visibility = "";

                bFirst = false;
            }

        });
}

///retourne la représentation JS des filtres sélectionnés pour la/les conditions
nsConditions.getCurrentConditions = function (myModalCondition) {


    var oRules = [];
    var myDoc = myModalCondition.getIframe().document;


    //TAb
    var nTab = myDoc.getElementById("RULES_TAB").value;

    //Type
    var nType = myDoc.getElementById("RULES_TYPE").value;

    // Parcours des bloc
    var allRulesBloc = myDoc.querySelectorAll("div[id^='RULES_CHOICE_'][data-active='1']");

    var allArrRulesBloc = Array.prototype.slice.call(allRulesBloc);
    allArrRulesBloc.forEach(

        function (myBloc) {

            var nIdBloc = myBloc.id.replace("RULES_CHOICE_", "");

            if (nIdBloc == "BASE_FILTER")
                return;

            // filters
            var oLstInp = myBloc.querySelectorAll("a[id^='CHK_ID_" + nIdBloc + "_'][chk='1']");
            var arrAllEntry = Array.prototype.slice.call(oLstInp);

            var sRulesBloc = "";

            arrAllEntry.forEach(

                function (myFilter) {


                    var nId = myFilter.id.split("_")[3]; //Format CHK_ID_<idblock>_<idfilter>

                    if (sRulesBloc.length != 0) {
                        var p = myFilter.parentElement;
                        var select = p.querySelector("select[id='SEL_OP_" + nIdBloc + "_" + nId + "']");
                        var op = select.options[select.selectedIndex].value;

                        var sOP = "AND"
                        if (op + "" == "1")
                            sOP = " AND "
                        else
                            sOP = " OR "

                        sRulesBloc += sOP;
                    }
                    sRulesBloc += '$' + nId + '$'
                }
                );

            var oPicto = myDoc.querySelector("span[id='selectedPicto_" + nIdBloc + "']");

            var sRuleName = "";
            var sColor = "";
            var sBackground = "";
            var sIcon = "";

            if (oPicto) {
                sColor = getAttributeValue(oPicto, "picto-color");// oPicto.style.color;
                sBackground = oPicto.style.backgroundColor;
                sIcon = getAttributeValue(oPicto, "picto-key");
            }

            oRules.push(
                {
                    "RuleId": nIdBloc,
                    "RuleDef": sRulesBloc,
                    "RuleName": sRuleName,
                    "Color": sColor,
                    "Background": sBackground,
                    "Icon": sIcon

                }
                );


        });

    return (oRules);
}

///Enregistre les Conditions
///Tab : table de la condition
///sFrom : point d'appel de la fenêtre de condition : soit MENU pour un appel depuis le menu droit, soit LIST pour un appel depuis le mode liste
nsConditions.validateConditions = function (nDescId, sFrom) {


    //Fenêtre de configuration de la conditions
    var myModalCondition = top.eTools.GetModal('modalConditions');
    var myDoc = myModalCondition.getIframe().document;
    var fctCallBack = function () { };



    //TAb
    // var nTab = myDoc.getElementById("RULES_TAB").value;

    //capsule
    var caps = new Capsule(nDescId);


    //Categoeir
    var nCat = myDoc.getElementById("RULES_CAT").value;

    //TreatmentType
    var nTreat = myDoc.getElementById("RULES_TREAT").value;


    //Type
    var nType = myDoc.getElementById("RULES_TYPE").value;

    var oRules = nsConditions.getCurrentConditions(myModalCondition);

    var sRules = ""
    if (nCat != 13) {
        if (oRules.length > 0)
            sRules = oRules[0].RuleDef;
    }
    else {
        sRules = JSON.stringify(oRules);

        //Icon par 
        var spanDesf;


        spanDesf = myDoc.querySelector("div[id^='RULES_ALL_DEF_'][data-active='1'] span[id^='selectedPicto']");

        if (spanDesf) {

            fctCallBack = function () {
                //Maj de l'icone (seulement si modif depuis la table de l'icone (cf mode liste qui permet de modifier les règles d'autres tables)
                if ((nDescId - nDescId % 100) == top.nsAdmin.tab) {
                    var pico = {};
                    pico.color = getAttributeValue(spanDesf, "picto-color");
                    pico.key = getAttributeValue(spanDesf, "picto-key");
                    top.nsAdmin.updatePicto(pico);
                }

            }
        }
    }

    if (nTreat == 17) // saisie obligatoire
    {

        if (sRules.length == 0) {
            //Si pas de règle de modif, on vérifie le flag "obligat"
            var oObligat = myDoc.querySelector("select[id='SEL_OBL_DEF_NORULES']");
            var nSelValueObl = oObligat.options[oObligat.selectedIndex].value;
            caps.AddProperty(nCat, 10, nSelValueObl == "1" ? "1" : "0");
        }
    }


    if (nTreat == 18) // lecture seule
    {
        if (sRules.length == 0) {
            //Si pas de règle de modif, on vérifie le flag "readlonly"
            var oRO = myDoc.querySelector("select[id='SEL_UPDT_DEF_NORULES']");
            var nSelValue = oRO.options[oRO.selectedIndex].value;
            caps.AddProperty(nCat, 43, nSelValue == "1" ? "1" : "0");
        }

    }

    if (nTreat == 19) // Duplication
    {
        if (sRules.length == 0) {
            //Si pas de règle de modif, on vérifie le flag "nodefaultclone"
            var oRO = myDoc.querySelector("select[id='SEL_CLN_DEF_NORULES']");
            var nSelValue = oRO.options[oRO.selectedIndex].value;
            sRules = nSelValue;
        }
    }

    if (nTreat == 20) // Import
    {
        if (sRules.length == 0) {
            //Si pas de règle de modif, on vérifie le flag "nodefaultclone"
            var oRO = myDoc.querySelector("select[id='SEL_IMPRT_DEF_NORULES']");
            var nSelValue = oRO.options[oRO.selectedIndex].value;
            sRules = nSelValue;
        }
    }


    caps.ParentTab = top.nsAdmin.tab;
    caps.AddProperty(nCat, nType, sRules);


    var upd = new eUpdater("eda/Mgr/eAdminDescManager.ashx", 1);
    var json = JSON.stringify(caps);
    upd.json = json;
    upd.ErrorCallBack = function () { };





    upd.send(function (oRes) {

        var res = JSON.parse(oRes);
        if (!res.Success) {
            top.eAlert(1, top._res_416, res.UserErrorMessage);

            myModalCondition.hide();
        }
        else {


            //Si on modif depuis le menu, on maj le menu ou s'il s'agit d'une condition sur la liste en cours
            if (sFrom == "MENU" || top.nsAdmin.tab == (nDescId - nDescId % 100)) {

                if (res.Result && Object.prototype.toString.call(res.Result) === '[object Array]') {
                    res.Result.forEach(function (myRes) {
                        if (myRes.hasOwnProperty('ResultType') && myRes.ResultType == 1) {

                            if (nTreat == 17 || nTreat == 18) {
                                
                                top.nsAdmin.loadContentModule(USROPT_MODULE_ADMIN_TAB, null, { tab: top.nsAdmin.tab });
                            }

                            var myTypeRules = myRes.TypeRules;
                            var myNbRules = myRes.NbRules;
                            var myRulesDesc = myRes.RulesDescription;
                            var mySpan = top.document.getElementById("RULES_INFO_" + myTypeRules);

                            if (mySpan) {
                                setAttributeValue(mySpan, "ednnb", myNbRules);
                                setAttributeValue(mySpan, "title", myRulesDesc);
                                mySpan.innerHTML = " (" + myNbRules + ")";
                            }
                        }
                    });
                }
            }

            //Si modif depuis la liste, on refresh la liste
            if (sFrom == "LIST") {

                var myConditionModal = top.eTools.GetModal('oConditionListModal');
                if (!(myConditionModal && myConditionModal.isModalDialog))
                    return;

                var myDoc = myConditionModal.getIframe();

                if ((nDescId - nDescId % 100) != nDescId)
                    myDoc.nsConditions.refreshConditionsLaunch(nDescId);
                else
                    myDoc.nsConditions.refreshConditionsLaunch();
            }

            
            if (typeof fctCallBack == "function")
                fctCallBack();

            //myModalCondition.hide();
            myModalCondition.fade(100);
            

        }

    });


}

///retire une condition
nsConditions.removeCondition = function (obj) {

    var myModDialog = nsConditions.modalWindow();
    var myDoc = myModDialog.getIframe().document;

    var confirmfct = function () {

        var nIdBloc = obj.id.replace("DEL_CHOICE_", "");
        var oBlocToHide = myDoc.querySelector("div[id='RULES_CHOICE_" + nIdBloc + "']");
        setAttributeValue(oBlocToHide, "data-active", "0");

        var nCat = myDoc.getElementById("RULES_CAT").value;
        if (nCat == 13) {
            var rul = myDoc.querySelectorAll("div[id^='RULES_CHOICE_'][data-active='1']");
            if (rul.length == 0) {

                var aaaa = myDoc.querySelector("div[id='RULES_ALL_DEF_ACTION']");
                setAttributeValue(aaaa, "data-active", 0);

                var all = document.querySelector("div[id^='RULES_ALL_DEF_NORULES']");
                setAttributeValue(all, "data-active", 1);
            }
        }
        else {

            var btn = document.getElementById("SHOW_CHOICE");
            setAttributeValue(btn, "data-active", "1");

            var all = document.querySelector("div[id^='RULES_ALL_DEF_NORULES']");
            setAttributeValue(all, "data-active", 1);
            btn.style.display = "";
        }
    }

    var nCat = myDoc.getElementById("RULES_CAT").value;
    var sMsg = "Confirmation de suppression de condition";
    var sDetailMsg;

    if (nCat != 13)
        sDetailMsg = "Confirmez-vous la suppression de cette condition et le retour à un comportement par défaut ?";
    else
        sDetailMsg = "Confirmez-vous la suppression de cette condition ?";


    eAdvConfirm({
        'criticity': eAdvConfirm.CRITICITY.MSG_EXCLAM,
        'title': top._res_806,
        'message': sMsg,
        'details': sDetailMsg,
        'bOkGreen': false,
        'resOk': top._res_28,
        'resCancel': top._res_29,
        'okFct': confirmfct
    }
     );



}

//Affiche le bloc condition si aucune n'existe
nsConditions.showConditions = function () {


    nsConditions.addCondition();

    var btn = document.getElementById("SHOW_CHOICE");
    var all = document.querySelector("div[id^='RULES_ALL_']");


    var a = getAttributeValue(all, "data-active");
    if (a + "" == "1") {
        setAttributeValue(all, "data-active", 0);
        btn.style.display = "none";
    }


}

//Ajoute un bloc condition
nsConditions.addCondition = function () {

    // recherche les block nouvelles conditions
    var all = document.querySelectorAll("div[id^='RULES_CHOICE_NEW']");
    var nIdNew = 0;

    if (all.length > 0) {

        var arrAllEntry = Array.prototype.slice.call(all);
        arrAllEntry.sort(
                //fonction de tri du tableau
                function sort(elem1, elem2) {

                    //on va trier sur l'attribut ednRulesId
                    var sortTestValue1 = elem1.id.replace("RULES_CHOICE_NEW", "") * 1;
                    var sortTestValue2 = elem2.id.replace("RULES_CHOICE_NEW", "") * 1;

                    if (sortTestValue1 < sortTestValue2)
                        return 1;
                    else if (sortTestValue1 > sortTestValue2)
                        return -1;
                    else
                        return 0;
                }
     );

        nIdNew = arrAllEntry[0].id.replace("RULES_CHOICE_NEW", "");
        nIdNew = nIdNew * 1 + 1;
    }


    var sNewBaseID = "_NEW" + nIdNew;

    //On prend le bloc de base
    var divBlockChoice = document.getElementById("RULES_CHOICE_BASE_FILTER");


    //On le duplique
    var divNewBlock = divBlockChoice.cloneNode(true);
    divNewBlock.id = divNewBlock.id.replace("_BASE_FILTER", sNewBaseID);

    //On l'append à la fin des autres conditions, juste avant le "par défaut"
    var parentBloc = divBlockChoice.parentElement;
    parentBloc.insertBefore(divNewBlock, document.getElementById("RULES_ALL_DEF_ACTION"));
    var myNewBlock = document.getElementById("RULES_CHOICE" + sNewBaseID);


    //On renomme les ID avec le nouvel ID nIdNew
    var divChoice = myNewBlock.querySelector("span[id='SHOW_CHOICE_BASE_FILTER']")
    divChoice.id = divChoice.id.replace("_BASE_FILTER", sNewBaseID);

    var divDelChoice = myNewBlock.querySelector("span[id='DEL_CHOICE_BASE_FILTER']")
    divDelChoice.id = divDelChoice.id.replace("_BASE_FILTER", sNewBaseID);

    var allFilter = myNewBlock.querySelectorAll("div.stepRulesConditionalFilterBloc");
    var arrAllEntryFilter = Array.prototype.slice.call(allFilter);
    arrAllEntryFilter.forEach(
        function (bl) {

            //On force l'affichage
            bl.style.display = "";

            //On renomme les id
            var a = bl.querySelector("a[id^='CHK_ID_BASE_FILTER_']");
            a.id = a.id.replace("_BASE_FILTER", sNewBaseID);

            var sel = bl.querySelector("select[id^='SEL_OP_BASE_FILTER_']");
            sel.id = sel.id.replace("_BASE_FILTER", sNewBaseID);
        }
        );


    var divBtnPicto = myNewBlock.querySelector("div[id='btnSelectPicto_BASE_FILTER']");
    var spanBtnPicto = myNewBlock.querySelector("span[id='selectedPicto_BASE_FILTER']");
    if (divBtnPicto) {
        divBtnPicto.id = divBtnPicto.id.replace("_BASE_FILTER", sNewBaseID);
        spanBtnPicto.id = spanBtnPicto.id.replace("_BASE_FILTER", sNewBaseID);
    }



    //On clone un spacer
    var divBlockSpace = parentBloc.querySelector("div.divAdminRulesSpace").cloneNode(true);
    parentBloc.insertBefore(divBlockSpace, document.getElementById("RULES_ALL_DEF_ACTION"));

    //Affiche la nouvelle condition
    myNewBlock.style.display = "";
    setAttributeValue(myNewBlock, "data-active", 1);

    var myModDialog = nsConditions.modalWindow();

    var myDoc = myModDialog.getIframe().document;
    // si cat == 13 (couleur) pass de "sinon" à "dans tous les cas" en fonction du nombre de condition active
    var nCat = myDoc.getElementById("RULES_CAT").value;
    if (nCat == 13) {
        var allRules = myDoc.querySelectorAll("div[id^='RULES_CHOICE_'][data-active='1']");
        if (allRules.length > 0) {
            var aShow = myDoc.querySelector("div[id='RULES_ALL_DEF_ACTION']");
            var aHide = myDoc.querySelector("div[id='RULES_ALL_DEF_NORULES']");
        }
        else {

            var aShow = myDoc.querySelector("div[id='RULES_ALL_DEF_NORULES']");
            var aHide = myDoc.querySelector("div[id='RULES_ALL_DEF_ACTION']");
        }

        setAttributeValue(aShow, "data-active", "1");
        setAttributeValue(aHide, "data-active", "0");

    }




}

//Ajout de règle : Met à jour les blocs de condition avec le filtre en paramètre
nsConditions.updateConditions = function (nFilterIdFocus, oDoc) {


    if (oDoc && oDoc.nodeType == 9) {

        var bSuccess = (getXmlTextNode(oDoc.getElementsByTagName("success")[0]) == "1");

        if (!bSuccess) {
            var nErrCode = getXmlTextNode(oDoc.getElementsByTagName("ErrorCode")[0]);
            var sErrDesc = getXmlTextNode(oDoc.getElementsByTagName("ErrorDescription")[0]);
            showWarning(top._res_1713, top._res_719 + " : ", "(" + nErrCode + ")  " + sErrDesc);
            // masque le tool tip
            return;
        }
        else {

            var sDesc = getXmlTextNode(oDoc.getElementsByTagName("filterdescription")[0]);
            var nId = getXmlTextNode(oDoc.getElementsByTagName("filterid")[0]);
        }
    }
    else {

        alert('Erreur');
        return;
    }

    var oRules = {
    };
    var myModDialog = null;
    if (typeof (top.window['_md']['modalConditions']) != 'undefined') {
        myModDialog = top.window['_md']['modalConditions'];
        oRules = nsConditions.getCurrentConditions(myModDialog);
    }
    else {

        alert("erreur");
        return;
    }

    var myDoc = myModDialog.getIframe().document;


    var allChoiceBlock = myDoc.querySelectorAll("div[id^='RULES_CHOICE_']");

    var arrAllEntry = Array.prototype.slice.call(allChoiceBlock);

    arrAllEntry.forEach(
    function (allElem) {

        var nIdBlock = allElem.id.replace("RULES_CHOICE_", "");

        var blockFilter = myDoc.createElement("div");
        blockFilter.className = "stepRulesConditionalFilterBloc";

        var chk = myDoc.createElement("a");
        blockFilter.appendChild(chk);

        chk.id = "CHK_ID_" + nIdBlock + "_" + nFilterIdFocus;
        chk.className = "rChk chk";
        setAttributeValue(chk, "dis", 0);
        setAttributeValue(chk, "chk", 0)
        setAttributeValue(chk, "align", 0)
        setAttributeValue(chk, "onclick", " chgChk(this); nsConditions.optionOnClick(this ); return false; ");
        setAttributeValue(chk, "href", "#")

        var spanChk = myDoc.createElement("span");
        chk.appendChild(spanChk);
        spanChk.className = "icon-square-o";
        //spanChk.innerHTML = "::before";

        var select = myDoc.createElement("select");
        blockFilter.appendChild(select);
        select.id = "SEL_OP_" + nIdBlock + "_" + nFilterIdFocus;
        setAttributeValue(select, "style", "width:40px;visibility:hidden;");

        var option1 = myDoc.createElement("option");
        select.appendChild(option1);
        option1.value = "1";
        option1.innerText = top._res_269; // ET

        var option2 = myDoc.createElement("option");
        select.appendChild(option2);
        option2.value = "2";
        option2.innerText = top._res_270; // OU


        var divLibelle = myDoc.createElement("div");
        blockFilter.appendChild(divLibelle);
        divLibelle.className = "rulesDescription";
        divLibelle.innerHTML = sDesc;


        var elem;
        var oLstInp = allElem.querySelectorAll(".stepRulesConditionalFilterBloc");
        var bFound = false;
        allElem.querySelectorAll(".stepRulesConditionalBloc");
        elem = allElem.querySelectorAll(".stepRulesConditionalBloc")[1];

        if (oLstInp.length > 0) {



            for (var cmptU = 0; cmptU < oLstInp.length; cmptU++) {

                var myElemFilter = oLstInp[cmptU];
                var myElemmFilterDesc = myElemFilter.querySelector("div.rulesDescription").innerHTML;
                if (sDesc > myElemmFilterDesc) {
                    elem = myElemFilter;
                    bFound = true;
                }
            };

            if (bFound)
                elem = elem.nextSibling;


        }


        allElem.insertBefore(blockFilter, elem);

    });
}



/*************  Gestion des filtres  *****************/
//Ajout de filtre : Ouverture de la modal de création
nsConditions.addNewRules = function (nTab) {


    var arrBtn = new Array();

    var btnSave = {
    };
    btnSave.label = top._res_286;
    btnSave.fctAction = saveFilter;

    var fctSave = function () {

        var myFilterFrame = oModalFilterWizard.getIframe();

        myFilterFrame.saveDb(0, null, false, function () {
            oModalFilterWizard.hide();
        });

        // oModalFilterWizard.hide();
    }

    btnSave.fctAction = saveFilter;
    btnSave.css = "button-green";

    var btnCancel = {
    };
    btnCancel.label = top._res_29;
    btnCancel.fctAction = cancelFilter;
    btnCancel.css = "button-gray";

    arrBtn.push(btnSave);
    arrBtn.push(btnCancel);



    AddNewFilter(nTab, arrBtn, true);
    oModalFilterWizard.addParam("CalllBackSuccess", nsConditions.callBackAddNewRules);


}


//Ajout de règle : CallBack suite à la création de la règle
//  Appel eFilterWizardManager pour obtenir les informations sur le filtre créé
nsConditions.callBackAddNewRules = function (oRes) {

    var filterid = getXmlTextNode(oRes.getElementsByTagName("filterid")[0]);
    oModalFilterWizard.hide();


    var url = "mgr/eFilterWizardManager.ashx";
    var ednu = new eUpdater(url, 0);
    ednu.addParam("action", "getdesc", "post");
    ednu.addParam("filterid", filterid, "post");

    ednu.send(
    function (oRe) {
        nsConditions.updateConditions(filterid, oRe)
    }
    );

}



/*****************  Picto       ***************************/
///Ouvre le picker de picto
nsConditions.pickPicto = function (obj) {

    var oSpan = obj.querySelector("span[id='" + obj.id.replace("btnSelectPicto_", "selectedPicto_") + "']");


    var oPicto = new top.ePictogramme(obj.id, {

        tab: 0,
        title: "Administrer le pictogramme",
        width: 750,
        height: 675,
        color: getAttributeValue(oSpan, "picto-color"),
        iconKey: getAttributeValue(oSpan, "picto-key"),
        callback: function (picto) {

            nsConditions.updatePicto(picto, obj)
        }
    });

    //debugger;
    oPicto.show();
}


///Met à jour le picto sur les couleurs conditionnel
nsConditions.updatePicto = function (picto, obj) {
    var ico = obj.querySelector("span[id='" + obj.id.replace("btnSelectPicto_", "selectedPicto_") + "']");
    if (ico) {
        ico.className = picto.className;
        ico.style.color = picto.color;
        setAttributeValue(ico, "picto-color", picto.color);
        setAttributeValue(ico, "picto-key", picto.key);
        setAttributeValue(ico, "picto-class", picto.className);
    }
}