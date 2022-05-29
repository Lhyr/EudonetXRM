/// <reference path="eTools.js" />
var oModalCalendarExpressFilter;
//*****************************************************************************************************//
//*****************************************************************************************************//
//*** JBE - 12/2011 - Affichage des menu contextuels
//*** Nécessite eTools.js
//*** Nécessite eContextMenu.js
//*** Params:
//*** debugMode : mettre à true pour empêcher la disparition du menu si la souris n'est
//*** 			  plus dessus et permettre le debug de son contenu sous Firebug ou autre
//*** nDescId : descid de la rubrique
//*** nTab : Table qui affiche la rubrique (mode liste ou signet)
//*** nTabFrom : Table affichée
//*** pop : type de catalogue
//*****************************************************************************************************//
//*****************************************************************************************************//

function eExpressFilter(jsVarName, nDescId, nFieldType, nTab, nTabType, nTabFrom, nTop, left, pop, bMultiple, bSpecial, parentFileId) {


    debugger

    var that = this; // pointeur vers l'objet eFieldEditor lui-même, à utiliser à la place de this dans les évènements onclick (ou this correspond alors à l'objet cliqué)
    //keyCode saisie, utilisé dans le onKeyUpSearch car sous IE8 le paramètre e (event) n'était pas correctement transmis dans la sous fonction du SetTimeOut.
    this.KeyEvent = 0;
    this.debugMode = false;
    this.bMultiple = bMultiple;
    this.nTab = nTab;
    this.nDescId = nDescId;
    this.nTabFrom = nTabFrom;
    //Champs text de recherche
    this.SearchField = null;
    //Type du champs
    this.nFieldType = nFieldType;
    //Type de la table
    this.nTableType = nTabType;
    //Id de la fiche parent <IdForm>
    this.nParentFileId = parentFileId;
    this.tree = false;
    this.popid = 0;
    this.lib = "";
    this.userid = "";
    this.userDisplay = "";
    this.bFromWidget = false; // Filtre express contenu dans un widget
    this.oOpenedModal = null; // Modale ouverte par un filtre express date (calendrier)
    this.thCallerId = null; //Entête du tableau th depuis laquelle on a appelé le filtre 


    var TYP_DATE = 2;       //	'Date
    var TYP_BIT = 3;        //	'Logique
    var TYP_USER = 8;       //   'Utilisateur
    var TYP_GROUP = 14;
    var TYP_NUMERIC = 10;   //  'Numérique
    var TYP_AUTOINC = 4; 	//  'Compteur auto
    var TYP_MONEY = 5; 		//  'Numéraire
    var TYP_COUNT = 18;     //  'Count
    var TYP_GEOGRAPHY = 24;     //  'Geographique
    var TYP_BITBUTTON = 25; // Bouton logique

    var FLD_MAIL_STATUS = 85;
    var FLD_MAIL_SENDTYPE = 86;

    // Historique fields
    var FLD_HISTO_FIELD = 100004;
    var FLD_TRANS_FIELD = 119506;
    var FLD_HISTO_TYPE = 100003;
    var FLD_HISTO_EXPORT_TAB = 100010;

    // Utilisateurs fields
    var FLD_USER_LEVEL = 101017;
    var FLD_USER_PRODUCT = 101034;
    var FLD_USER_PASSWORD_ALGO = 101035;

    //champ de PJ
    var FLD_PJ_TYPE = 102009;
    var FLD_PJ_FILE = 102011;
    

    // Filter fields
    var FLD_LIBELLE = 104001;
    var FLD_PARAM = 104002;
    var FLD_TYPE = 104003;
    var FLD_VIEW_PERMID = 104004;
    var FLD_UPDATE_PERMID = 104005;
    var FLD_DATE_LAST_MODIFIED = 104006;
    var FLD_USERID = 104007;
    var FLD_TAB = 104008;
    var FLD_ID = 104009;

    // Campagnes mail fields
    var FLD_CAMPAIGN_MAIL_ADR_DESCID = 106014;
    var FLD_CAMPAIGN_STATUS = 106016;
    var FLD_CAMPAIGN_SENDTYPE = 106017;

    //WorkflowScenario
    let FLD_WORKFLOWSCENARIO_EVENTTAB = 119204;
    let FLD_WORKFLOWSCENARIO_TARGETTAB = 119205;

    // Campagne stats fields
    var FLD_CAMPAIGNSTAT_CATEGORY = 111001;

    // Modèle de mail fields
    var FLD_MAILTEMPLATE_OWNER = 107099;

    // Formulaire XRM fields
    var FLD_FORMULAR_OWNER = 113099;
    var FLD_FORMULAR_STATUS = 113025;

    // Journal des traitements fields
    var FLD_RGPD_TYPE = 117007;
    var FLD_RGPD_PERSONNAL_DATA_CATEGORY = 117025;
    var FLD_RGPD_SENSIBLE_DATA_CATEGORY = 117026;
    var FLD_RGPD_STATUS = 117034;

    // IMPORTTEMPLATE fields
    var FLD_IMPORTTEMPLATE_OWNER = 119005;

    this.bQuickUserFilter = (jsVarName == "quickUserFilter");

    this.contextMenu = new eContextMenu(null, nTop, left);

    this.contextMenu.middleMenuUl.style.height = "138px";
    this.contextMenu.middleMenuUl.style.overflowY = "auto";
    this.contextMenu.middleMenuUl.style.overflowX = "hidden";


    var obj = this.contextMenu;

    this.onClickHide = function (e) {

        if (!e)
            e = window.event;

        // Objet source
        var oSourceObj = e.target || e.srcElement;
        //var oSourceObjOrig = oSourceObj;
        //var topelement = "BODY"; 
        //var FHeader = "COL_" + this.nTab + "_" + this.nDescId;  //Filtre express en liste
        var FRapide = "SPAN_" + this.nDescId;   //Filtre rapide en liste

        var continueLoop = true;
        do {
            if (oSourceObj != null && oSourceObj.id != null) {
                var aId = oSourceObj.id.split("_");

                if (!(
                    (oSourceObj.id == this.contextMenu.parentPopup.div.id)
                    || (aId[aId.length - 1] == this.nDescId)
                    || FRapide == oSourceObj.id
                    || (this.oOpenedModal != null && eTools.indexOf(aId, this.oOpenedModal.UID) != -1)
                )
                ) {
                    oSourceObj = oSourceObj.parentNode || oSourceObj.parentElement;
                }
                else {
                    continueLoop = false;
                }
            }
            else {
                that.contextMenu.hide();
                expressFilter = null;
                quickUserFilter = null;
                removeWindowEventListener('click', hideExpressFilter);

                continueLoop = false;
            }
        } while (continueLoop);
    };

    if (!this.debugMode) {
        var oActionMenuMouseOver = function () {

            var actionOut = setTimeout(
                function () {
                    obj.hide();
                }
                , 200);

            //Annule la disparition
            setEventListener(obj.mainDiv, "mouseover", function () { clearTimeout(actionOut) });
        };

        //Faire disparaitre le menu
        try {
            //Clique hors Tooltip fait disparaitre le menu
            //setWindowEventListener('click', function (o) { that.onClickHide(o); });
            setWindowEventListener('click', hideExpressFilter);
        }
        catch (Exc) {
        }

        this.contextMenu.firstSeparator = 1;
        this.contextMenu.secondSeparator = 1;
    };

    //
    this.hide = function () {
        try {
            this.contextMenu.hide();
        }
        catch (exp) { }
    };

    this.addTextBox = function () {
        var sOnKeyUp;
        var sOnClick;
        if (this.bQuickUserFilter) {
            sOnKeyUp = 'doQckUsrFltSearch(event,this, ' + jsVarName + ');';
            sOnClick = 'doQckUsrFltSearch(event, document.getElementById(\'srchFromExpressFilter\'), ' + jsVarName + ');';
        }
        else {

            sOnKeyUp = 'doOnKeyUpSearch(event,this,' + jsVarName + ',false);';
            sOnClick = 'doOnKeyUpSearch(event, document.getElementById(\'srchFromExpressFilter\'), ' + jsVarName + ',true);';
        }

        var divSrch = document.createElement("div");
        divSrch.innerHTML = '<input class="eCMSearch" id="srchFromExpressFilter" onkeyup="' + sOnKeyUp + '">'
            + '<div class="eCMSearch"><DIV id="eBtnSrch" class="icon-magnifier srchFldImg" title="' + top._res_1040 + '" alt="' + top._res_1040 + '" oldonclick="' + sOnClick + '" onclick="' + sOnClick + '"></DIV></DIV>';
        this.contextMenu.addHtmlElementSearch(divSrch, 0);
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
            document.getElementById('srchFromExpressFilter').focus();
        }


        if (nFieldType.toString() == TYP_NUMERIC.toString()
            || nFieldType.toString() == TYP_AUTOINC.toString()
            || nFieldType.toString() == TYP_MONEY.toString()
            || nFieldType.toString() == TYP_COUNT.toString()) {
            document.getElementById('srchFromExpressFilter').style.textAlign = "right";
        }

    };


    var OP_EQUAL = 0;      //	'=
    var OP_LESS = 1;       //	'<
    var OP_GREATER = 3;    //	'>
    var OP_BETWEEN = 97;   // ASY : entre 2 dates

    this.addCalendarBox = function () {
        this.contextMenu.addItem(top._res_5017, "doCalendarFilter(" + jsVarName + "," + OP_EQUAL + ");", 0, 0, "actionItem specialItem", "", "ednNoValidate=1");  //Choisir une date
        this.contextMenu.addItem(top._res_5015, "doCalendarFilter(" + jsVarName + "," + OP_LESS + ");", 0, 0, "actionItem specialItem", ""); //Avant le
        this.contextMenu.addItem(top._res_5016, "doCalendarFilter(" + jsVarName + "," + OP_GREATER + ");", 0, 0, "actionItem specialItem", ""); //Après le
        // ASY : choix entre 2 dates
        this.contextMenu.addItem(top._res_6675, "doCalendarBetwenFilter(" + jsVarName + "," + OP_BETWEEN + ");", 0, 0, "actionItem specialItem", ""); //Between

        this.addSeparator(0);
    };

    this.addLogicBox = function (isButton) {
        var isTrueLabel = (isButton) ? top._res_1842 : top._res_2011;
        var isFalseLabel = (isButton) ? top._res_1843 : top._res_2012;

        this.contextMenu.addItem(isTrueLabel, "doLogicFilter(" + jsVarName + ",1);", 0, 0, "actionItem", ""); //côché
        this.contextMenu.addItem(isFalseLabel, "doLogicFilter(" + jsVarName + ",0);", 0, 0, "actionItem", ""); //décôchée
    };

    this.addStats = function () {
        this.contextMenu.addItem(top._res_1395, "doStats(" + jsVarName + ");", 2, 1.5, "actionItem icon-stats", ""); //Statistiques
    };

    this.addLoadAllvalues = function () {
        this.contextMenu.addItem(top._res_1126, "doLoadAllValues(" + jsVarName + ");", 2, 0, "actionItem loadAllVal", ""); //Charger toutes les valeurs
    };

    this.addItem = function (html, jsAction, to, level, css, tooltip) {
        this.contextMenu.addItem(html, jsAction, to, level, css, tooltip);
    };

    this.addSeparator = function (to) {
        this.contextMenu.addSeparator(to);
    };

    this.addAdvancedSelection = function () {
        //Sélection avancée
        this.contextMenu.addItem(top._res_6221, jsVarName + ".openCatalog();", 2, 1.5, "actionItem icon-magnifier", "");
    };

    this.getFilterValue = function () {

        var defValue = "";

        var nTab = this.nTab;

        //vérirication table relation depuis signet
        var mt = document.getElementById("mt_" + nTab)
        if (mt) {
            if (getAttributeValue(mt, "ednrel") != "") {
                nTab = getAttributeValue(mt, "ednrel")
            }
        }

        var tHead = document.getElementById("HEAD_" + nTab);
        if (tHead) {

            var tdHead = tHead.querySelector("TH#" + this.thCallerId);


            var aExpressFilter = getAttributeValue(tdHead, "aef").split("$%$");
            if (aExpressFilter.length >= 2) {
                var op = aExpressFilter[0];
                var aValue = aExpressFilter[1].split("#$|#$");
                if (op == 8 || op == 0)
                    defValue = aValue.length == 1 ? aValue[0] : aValue[1];
            }
        }
        return defValue;

    };

    this.openCatalog = function () {
        var defValue = this.getFilterValue();
        //eExpressFilter(jsVarName, nDescId, nFieldType, nTab, nTabType, nTabFrom, nTop, left, pop, bMultiple, bSpecial, parentFileId)
        showCatGeneric(
            true,                                //p_bMulti,
            this.tree,                           //p_btreeView,        
            defValue,                            //p_defValue,
            this.thCallerId,                     //p_sourceFldId,
            null,                                //p_targetFldId,
            this.popid,                          //p_catDescId,
            pop,                                 //p_catPopupType,
            0,                                   //p_catBoundDescId,   //TODO
            0,                                   //p_catBoundPopup,    //TODO
            0,                                   //p_catParentValue,   //TODO
            this.lib,                            //p_CatTitle,
            jsVarName,                           //p_JsVarName,
            false,                               //p_bMailTemplate,
            this.validateCatalog,                //p_partOfAfterValidate,     
            null,                                //p_partOfAfterCancel,
            LOADCATFROM.EXPRESSFILTER
            //true,                                //p_fromFilter,
            //false,                               //p_fromTreat,
            //false                                //p_fromAdmin
        );
    };

    this.validateCatalog = function (catalogDialog, srcId, trgId, tabSelectedLabels, tabSelectedValues, selectedIDs) {
        //OP_IN_LIST = 8
        searchOperator(8, joinString(";", tabSelectedValues), that);
    };

    this.addAdvancedUsersSelection = function () {
        //Sélection avancée
        this.contextMenu.addItem(top._res_6221, jsVarName + ".openUserCatalog();", 2, 1.5, "actionItem icon-magnifier", "");
    };

    this.modalUserCat = null;
    this.openUserCatalog = function () {

        var defValue = this.getFilterValue();

        this.modalUserCat = new eModalDialog(top._res_246, 0, "eCatalogDialogUser.aspx", 550, 610);
        top.eTabCatUserModalObject.Add(this.modalUserCat.iframeId, this.modalUserCat);
        this.modalUserCat.addParam("iframeId", this.modalUserCat.iframeId, "post");
        this.modalUserCat.ErrorCallBack = function () { setWait(false); }
        this.modalUserCat.addParam("multi", 1, "post");
        this.modalUserCat.addParam("selected", defValue, "post");
        this.modalUserCat.addParam("modalvarname", jsVarName + ".modalUserCat", "post");
        this.modalUserCat.addParam("fromtreat", "0", "post");

        if (nFieldType == TYP_GROUP) {
            this.modalUserCat.addParam("fulluserlist", "1", "post");
            this.modalUserCat.addParam("descid", this.nDescId, "post");
        }


        //if (sMulti != "1") {
        //    modalUserCat.addParam("showcurrentgroupfilter", "1", "post"); //Si à 1 => Proposition dans le catalogue : <le groupe de l'utilisateur en cours> pour filtre avancé
        //    modalUserCat.addParam("showcurrentuserfilter", "1", "post");   //Si à 1 => Proposition dans le catalogue : <utilisateur en cours> pour filtre avancé
        //    modalUserCat.addParam("usegroup", "1", "post"); //si à 1 => Autorise la sélection de groupe pour le catatalogue simple
        //}

        this.modalUserCat.addParam("showvalueempty", "1", "post"); //si à 1 => Affiche <Vide> sur le catalogue simple
        this.modalUserCat.addParam("showvaluepublicrecord", "0", "post"); //si à 1 => Affiche <Fiche Publique> sur le catalogue simple

        this.modalUserCat.show();
        this.modalUserCat.addButton(top._res_29, function () { that.modalUserCat.hide(); }, "button-gray");
        this.modalUserCat.addButton(top._res_28, this.validateUserCatalog, "button-green", null, "ok");
    }

    this.validateUserCatalog = function () {
        var strReturned = that.modalUserCat.getIframe().GetReturnValue();
        that.modalUserCat.hide();
        var vals = strReturned.split('$|$')[0];
        var libs = strReturned.split('$|$')[1];
        searchOperator(8, vals, that);
    };

    this.addEmptyNotEmpty = function () {
        this.addSeparator(2);
        this.contextMenu.addItem(top._res_141, "doFilterIsEmpty(" + jsVarName + ",1);", 2, 0, "actionItem specialItem", ""); //vide
        this.contextMenu.addItem(top._res_1203, "doFilterIsEmpty(" + jsVarName + ",0);", 2, 0, "actionItem specialItem", ""); //non vide
    };

    this.clearList = function (to) {
        this.contextMenu.clearList(to);
    };

    // Les 2 cas particuliers, tout 2 font chercher en base les valeurs contenu dans le sfiches, sinon trop de valeurs sont possibles
    if (this.nDescId == FLD_HISTO_EXPORT_TAB
        || this.nDescId == FLD_CAMPAIGN_MAIL_ADR_DESCID
        || nDescId == FLD_PJ_FILE
        || nDescId == FLD_PJ_TYPE
        || nDescId == FLD_WORKFLOWSCENARIO_EVENTTAB
        || nDescId == FLD_WORKFLOWSCENARIO_TARGETTAB
    ) {
        this.addSeparator(2);
        this.addLoadAllvalues();
        this.addEmptyNotEmpty();
    } else {
        switch (nFieldType.toString()) {

            case TYP_DATE.toString():
                this.addCalendarBox();
                this.addEmptyNotEmpty();
                break;
            case TYP_NUMERIC.toString():
            case TYP_AUTOINC.toString():
            case TYP_MONEY.toString():
            case TYP_COUNT.toString():
                // Traitement particulier pour les champs Rubriques et Types de la table Historique : Afficher toutes les valeurs
                if (nDescId != FLD_HISTO_FIELD
                    && nDescId != FLD_TRANS_FIELD
                    && nDescId != FLD_HISTO_TYPE
                    && nDescId != FLD_TAB
                    && nDescId != FLD_CAMPAIGN_STATUS
                    && nDescId != FLD_CAMPAIGN_SENDTYPE
                    && nDescId != FLD_CAMPAIGNSTAT_CATEGORY
                    && nDescId != FLD_PJ_FILE
                    && nDescId != FLD_PJ_TYPE
                    && nDescId % 100 != TYPE_PLANNING
                    && nDescId % 100 != FLD_MAIL_STATUS
                    && nDescId % 100 != FLD_MAIL_SENDTYPE
                    && nDescId != FLD_RGPD_TYPE
                    && nDescId != FLD_RGPD_STATUS) {
                    this.addTextBox();
                    this.addEmptyNotEmpty();
                }
                else {
                    this.addSeparator(2);
                    this.addLoadAllvalues();
                    this.addEmptyNotEmpty();
                }
                break;
            case TYP_USER.toString():
            case TYP_GROUP.toString():
                if (this.bQuickUserFilter) {
                    this.addTextBox();
                }
                else {
                    this.addSeparator(2);
                    // ASY (24 236) Modif suite a :  [Mode liste] - Filtre express sur Catalogue Utilisateurs
                    this.addTextBox();
                    this.addLoadAllvalues();
                    this.addSeparator(2);
                    this.addAdvancedUsersSelection();
                    // #29289 : GMA 20140311
                    // Sur la rubrique Appartient à, affichage de 'FILTRE PUBLIC' au lieu de 'VIDE' et suppression de 'NON VIDE'.
                    // GCH : idem pour le propriétaire des modèles
                    if (this.nDescId == FLD_USERID.toString() || this.nDescId == FLD_MAILTEMPLATE_OWNER) {
                        this.addSeparator(2);
                        this.contextMenu.addItem(top._res_681, "doFilterIsEmpty(" + jsVarName + ",1);", 2, 0, "actionItem specialItem", ""); //vide
                    }
                    else if (this.nDescId == FLD_FORMULAR_OWNER) {
                        this.addSeparator(2);
                        this.contextMenu.addItem(top._res_2371, "doFilterIsEmpty(" + jsVarName + ",1);", 2, 0, "actionItem specialItem", ""); //vide
                    }
                    else if (this.nDescId == FLD_IMPORTTEMPLATE_OWNER.toString()) {
                        this.addSeparator(2);
                        this.contextMenu.addItem(top._res_8420, "doFilterIsEmpty(" + jsVarName + ",1);", 2, 0, "actionItem specialItem", "");
                    } else {
                        this.addEmptyNotEmpty();
                    }
                }
                break;
            case TYP_BIT.toString():
                this.addLogicBox(false);
                break;
            case TYP_BITBUTTON.toString():
                this.addLogicBox(true);
                break;
            case TYP_GEOGRAPHY.toString():
                this.addEmptyNotEmpty();
                break;
            default:
                if (nDescId == FLD_USER_PRODUCT
                    || nDescId == FLD_USER_LEVEL
                    || nDescId == FLD_USER_PASSWORD_ALGO
                    || nDescId == FLD_FORMULAR_STATUS
                ) {
                    this.addSeparator(2);
                    this.addLoadAllvalues();
                } else {
                    this.addTextBox();
                    if (pop > 0 && !bSpecial && pop != 5) {
                        this.addSeparator(2);
                        this.addLoadAllvalues();
                        this.addSeparator(2);
                        this.addAdvancedSelection();
                    }
                }
                this.addEmptyNotEmpty();
                break;
        }
    }

    //Aucun filtre
    this.addSeparator(2);

    if (this.bQuickUserFilter) {

        var curView = getCurrentView(document);
        var mainTableList = document.getElementById("mt_" + this.nTab);
        var planningEnabled = mainTableList != null && getAttributeValue(mainTableList, "eCalendarEnabled") == "1";
        //var bCalendarView = curView == "CALENDAR" || curView == "CALENDAR_LIST" || getAttributeValue(mainTableList, "edntyp") == "1"; // Si on est sur planning

        // fiche publique uniquement si appartient à
        //NHA US999 : en mode liste ou graphique sur un planning on affiche l'option "Fiche publique"
        var nDescIdNum = this.nDescId % 100;
        if (nDescIdNum == 99 ||
            (this.nTableType == 0 && (nDescIdNum == 88)) ||
            (this.nTableType == 1 && this.nFieldType.toString() == TYP_USER.toString() && (nDescIdNum == 92)) ||
            (this.nTableType != 0 && this.nTableType != 1 && this.nTableType != 5 && nDescIdNum == 92)) {
            this.contextMenu.addItem(top._res_53, "saveQckUsrFltFromList(" + nTab + ", '0','" + top._res_53.replace(/'/g, "\\'") + "', " + this.nDescId + ");", 2, 1.5, "actionItem publicFilter", "");
            this.addSeparator(2);
        }
        // pas d'option "aucun filtre" lorsque l'on est en mode planning graphique



        // MCR 39667 : en mode liste sur un planning ne pas afficher la valeur : aucun filtre   
        //NHA US#999 : en mode liste ou graphique sur un planning on affiche "aucun filtre"
        //bCalendarList = curView == "LIST" && this.nTableType == 1 && this.nDescId % 100 == 92 && this.nFieldType.toString() == TYP_USER.toString();

        //if (curView != "CALENDAR" && curView != "CALENDAR_LIST" && planningEnabled != true) {
            this.contextMenu.addItem(top._res_183, "saveQckUsrFltFromList(" + this.nTab + ", -1, '', " + this.nDescId + ");", 2, 1.5, "actionItem icon-rem_filter", ""); //aucun filtre
            this.addSeparator(2);
        //}
        this.contextMenu.addItem(top._res_6221, "doQckUsrFltSrch(document.getElementById('QckUsrFltTxt_" + this.nDescId + "'));", 2, 1.5, "actionItem icon-magnifier", ""); //selection avancee
    }
    else {
        this.contextMenu.addItem(top._res_183, "cancelThisFilter(" + this.nTab + "," + this.nDescId + ", expressFilter);", 2, 1.5, "actionItem icon-rem_filter", ""); //aucun filtre
    }
    pop = getNumber(pop);
    // ajout de test sur bSpecial pour ne pas avoir les Stat sur la colonne FICHE de l onglet HISTORIQUE
    if ((pop > 0 || nFieldType == TYP_USER || nFieldType == TYP_GROUP || nFieldType == TYP_BIT || nFieldType == TYP_BITBUTTON) && !bSpecial) {
        this.addSeparator(2);
        this.addStats();
    }
}


function doLogicFilter(oExpressFilter, value) {
    var descId = oExpressFilter.nDescId;
    var tabFrom = oExpressFilter.nTabFrom;
    var tab = oExpressFilter.nTab;

    saveExpressfilterFromList(tab, descId, 0, value, oExpressFilter);
    oExpressFilter.hide();

}

var fltExprTimeOut;
function doOnKeyUpSearch(e, inpt, oExpressFilter, bValid) {
    clearTimeout(fltExprTimeOut);
    //keyCode saisie, utilisé dans le onKeyUpSearch car sous IE8 le paramètre "e" (event) n'était pas correctement transmis dans la sous fonction du SetTimeOut.
    oExpressFilter.KeyEvent = e.keyCode;

    fltExprTimeOut = setTimeout(
        function () {
            var q = inpt.value;
            var btnSearch = document.getElementById("eBtnSrch");

            // 42365 CRU : Correctif problème affichage loupe
            if (q != "") {
                switchClass(btnSearch, "icon-magnifier", "icon-edn-cross");
                //document.getElementById("eBtnSrch").className = "eBtnSrchCross";
                //document.getElementById("eBtnSrch").setAttribute("onclick", 'document.getElementById(\'srchFromExpressFilter\').value=\'\';this.className = \'eBtnSrch\'');
                btnSearch.onclick = function () {
                    inpt.value = "";
                    oExpressFilter.clearList(1);
                    switchClass(btnSearch, "icon-edn-cross", "icon-magnifier");
                    btnSearch.onclick = function () { return; };
                };
            }
            else {
                switchClass(btnSearch, "icon-edn-cross", "icon-magnifier");
                //document.getElementById("eBtnSrch").className = "eBtnSrch";
                //document.getElementById("eBtnSrch").setAttribute("onclick", document.getElementById('eBtnSrch').getAttribute('oldonclick'));
                if (typeof (doQckUsrFlt) == "function") {
                    oExpressFilter.clearList(1);
                }
                return;
            }

            if (oExpressFilter.KeyEvent != 13 && bValid != true) {
                if (q.length < 3) {
                    return;
                }
            }
            else {
                if (q.length == 0) {
                    return;
                }
            }
            //GCH - SPRINT 2015.04 - #36869, #37690 - [Internationalisation] - Affichage des numériques - Rendu/Maj - Filtre express (MRU)
            if (isFormatNumeric(oExpressFilter.nFieldType))
                q = eNumber.ConvertDisplayToBdd(q, true);
            var oExpressFilterMagr = new eUpdater("mgr/eExpressFilterManager.ashx", 0);
            oExpressFilterMagr.ErrorCallBack = function () { }
            oExpressFilterMagr.addParam("tab", oExpressFilter.nTab, "post");
            oExpressFilterMagr.addParam("tabfrom", oExpressFilter.nTabFrom, "post");
            oExpressFilterMagr.addParam("parentFileId", oExpressFilter.nParentFileId, "post");
            oExpressFilterMagr.addParam("descid", oExpressFilter.nDescId, "post");
            oExpressFilterMagr.addParam("q", q, "post");
            oExpressFilterMagr.addParam("multiple", oExpressFilter.bMultiple ? "1" : "0", "post");
            //AAB tâche 1 882
            if (eTools.GetModal("oModalFormularList") != null) {
                var formularType = eTools.GetModal("oModalFormularList").getParam("formularType");
                oExpressFilterMagr.addParam("formularType", formularType, "post");
            }

            oExpressFilterMagr.send(updateExpressFilterList, oExpressFilter, q);
        }
        , 200);


}


function updateExpressFilterList(oRes, oExpressFilter, q) {
    oExpressFilter.clearList(1);

    var bMultiple = (getXmlTextNode(oRes.getElementsByTagName("multiple")[0]) == "1") ? true : false;
    var bNumeric = (getXmlTextNode(oRes.getElementsByTagName("isnumeric")[0]) == "1") ? true : false;
    /* débute par et contient  */

    var op = bMultiple ? 9 : 0;    // si catalogue multiple alors l'opérateur est contient sinon égal à 

    var oElmList = oRes.getElementsByTagName("element");

    if (oElmList.length > 0) {

        for (var ielm = 0; ielm < oElmList.length; ielm++) {

            var elm = oElmList[ielm];
            var strElmValue = getXmlTextNode(elm.childNodes[0]);
            var type = elm.getAttribute("type");
            var value = elm.getAttribute("value");


            var jsAction = "";

            if (type == "operator") {
                var aValue = value.split(";|;");

                oExpressFilter.addItem(strElmValue, "searchOperator('" + aValue[0].replace(/'/g, "\\'") + "','" + aValue[1].replace(/'/g, "\\'") + "', expressFilter);", 1, 0, "actionItem", "");
            }
            else {
                if (type == "separator") {
                    oExpressFilter.addSeparator(1);
                }
                else {
                    var cssClass = "actionItem";
                    if (bNumeric) {
                        cssClass += " numericItem";
                    }

                    if (oExpressFilter.bFromWidget) {
                        value = strElmValue + "#$|#$" + value;
                    }

                    oExpressFilter.addItem(strElmValue, "searchOperator(" + op + ",'" + value.replace(/'/g, "\\'").replace(/\n/g, "") + "', expressFilter);", 1, 0, cssClass, "");
                }
            }
        }
    }
}

// recherche sur les utilisateurs dans un filtre rapide
function doQckUsrFltSearch(e, inpt, oQuickFilter) {
    clearTimeout(fltExprTimeOut);

    fltExprTimeOut = setTimeout(
        function () {
            var q = inpt.value;
            if (e.keyCode != 13 && q.length < 3)
                return;
            var descId = oQuickFilter.nDescId;
            var tabFrom = oQuickFilter.nTabFrom;
            var tab = oQuickFilter.nTab;
            var btnSrch = document.getElementById("eBtnSrch")
            if (q.length == 0) {    //Si pas de valeurs recherchée on remet les valeurs par défaut
                switchClass(btnSrch, "icon-edn-cross", "icon-magnifier");
                if (typeof (doQckUsrFlt) == "function") {
                    oQuickFilter.clearList(1);
                    SetParamQuickUserFilter(oQuickFilter, tab);
                }
                return;
            }
            else {
                switchClass(btnSrch, "icon-magnifier", "icon-edn-cross");
                btnSrch.onclick = function () {
                    inpt.value = "";
                    oQuickFilter.clearList(1);
                    SetParamQuickUserFilter(oQuickFilter, tab);
                    switchClass(btnSrch, "icon-edn-cross", "icon-magnifier");
                    btnSrch.onclick = function () { return; };
                };
            }

            var oExpressFilterMagr = new eUpdater("mgr/eExpressFilterManager.ashx", 0);
            oExpressFilterMagr.ErrorCallBack = function () { }
            oExpressFilterMagr.addParam("tab", tab, "post");
            oExpressFilterMagr.addParam("tabfrom", tabFrom, "post");
            oExpressFilterMagr.addParam("descid", descId, "post");
            oExpressFilterMagr.addParam("q", q, "post");
            //oExpressFilterMagr.addParam("multiple", bMultiple ? "1" : "0", "post");
            oExpressFilterMagr.addParam("action", "quickuserfilter", "post");
            oExpressFilterMagr.send(updateQuickFilterList, oQuickFilter);
        }
        , 200);
}

/*Affichage des résultat de recherche du filtre rapide*/

function updateQuickFilterList(oRes, quickUserFilter) {

    quickUserFilter.clearList(1);

    var oElmList = oRes.getElementsByTagName("element");

    if (oElmList.length > 0) {

        for (var ielm = 0; ielm < oElmList.length; ielm++) {
            var elm = oElmList[ielm];
            var sDisplay = getXmlTextNode(elm.childNodes[0]);
            var sValue = elm.getAttribute("value");
            var sUsrGrp = elm.getAttribute("usrgrp");
            var bDisabled = elm.getAttribute("disabled") == "1";
            var jsAction = "";
            var sCssAdd = "";
            if (bDisabled)
                sCssAdd = " disabledItem";
            if (sUsrGrp == "usr") {
                quickUserFilter.addItem(sDisplay, "saveQckUsrFltFromList(" + quickUserFilter.nTab + ", " + sValue.replace(/'/g, "\\'") + ",'" + sDisplay.replace(/'/g, "\\'") + "', " + quickUserFilter.nDescId + ");", 1, 0, "actionItem" + sCssAdd, "");
            }
            else if (sUsrGrp == "grp") {
                quickUserFilter.addItem(sDisplay, "saveQckUsrFltFromList(" + quickUserFilter.nTab + ", '" + sValue.replace(/'/g, "\\'") + "','" + sDisplay.replace(/'/g, "\\'") + "', " + quickUserFilter.nDescId + ");", 1, 0, "actionItem qckGroupFlt" + sCssAdd, "");
            }
        }
    }
}

function searchOperator(op, value, oExpressFilter) {

    if (!oExpressFilter)
        oExpressFilter = expressFilter; // objet instancié par eList.js

    if (oExpressFilter != null) {
        saveExpressfilterFromList(oExpressFilter.nTab, oExpressFilter.nDescId, op, value, oExpressFilter);
        oExpressFilter.hide();
    }
}

//function cancelThisFilter(oExpressFilter) {
function cancelThisFilter(tab, descId, oExpressFilter) {

    saveExpressfilterFromList(tab, descId, "", "$cancelthisfilter$", oExpressFilter);

    if (!oExpressFilter)
        oExpressFilter = expressFilter; // objet instancié par eList.js

    if (oExpressFilter != null)
        oExpressFilter.hide();
}

function cancelAllExpressFilters(oExpressFilter) {
    if (!oExpressFilter)
        oExpressFilter = expressFilter; // objet instancié par eList.js

    /* Modif by KHA pour les perfs
    var oFiltersNode = document.getElementById(strFiltersNodeId);
    if (oFiltersNode) {
    var oFilterElts = oFiltersNode.getElementsByTagName('div');
    if (oFilterElts) {
    if (oExpressFilter != null)
    oExpressFilter.hide();
    for (var i = 0; i < oFilterElts.length; i++) {
    var oLi = oFilterElts[i];
    var sfValue = oLi.getAttribute("val");
    var sfTab = oLi.getAttribute("tab");
    var sfType = oLi.getAttribute("typ");
    // 3 = filtre express
    if (sfType == 3) {
    saveExpressfilterFromList(sfTab, sfValue, "", "$cancelthisfilter$");
    }
    }
    }
    }
    */

    if (oExpressFilter.nTab == oExpressFilter.nTabFrom || oExpressFilter.nTab == 104000 || oExpressFilter.nTab == 105000) {

        ///appel l'updater pour les filtres express en mode Liste Principale
        var updatePref = "tab=" + tab + ";$;filterExpress=$cancelallexpressfilters$";
        updateUserPref(updatePref, function () {
            loadList();
        });
    }
    else {
        ///appel l'updater pour les filtres express en mode signet
        var updatePref = "tab=" + oExpressFilter.nTabFrom + ";$;bkm=" + oExpressFilter.nTab + ";$;filterExpress=$cancelallexpressfilters$";

        updateUserBkmPref(updatePref, function () { loadBkm(oExpressFilter.nTab); });
    }



}

function doFilterIsEmpty(oExpressFilter, nEmpty) {
    var descId = oExpressFilter.nDescId;
    var tabFrom = oExpressFilter.nTabFrom;
    var tab = oExpressFilter.nTab;

    if (nEmpty == 0) {
        saveExpressfilterFromList(tab, descId, 0, "<>", oExpressFilter);
    }
    else {
        saveExpressfilterFromList(tab, descId, 0, "NULL", oExpressFilter);
    }
    oExpressFilter.hide();
}

function saveQckUsrFltFromList(tab, value, displayValue, quickFilterDescid) {

    var oInputElement = document.getElementById('QckUsrFltTxt_' + quickFilterDescid);

    if (quickFilterDescid % 100 == 92 || quickFilterDescid % 100 == 88) {
        var updatePref = "tab=" + tab + ";$;menuuserid=" + value;
        updateUserPref(updatePref, function () {
            loadList();
        });
    }
    else {
        var updUsrVal = "type=0;$;tab=" + tab + ";$;descid=" + quickFilterDescid + ";$;index=" + oInputElement.getAttribute("ednIdx") + ";$;value=" + value;
        updateUserValue(updUsrVal, function () {
            loadList();
        });
    }

    //quickFilter.hide();
    /* //GCH : commenté suite à demande #20355 : Mode Liste - Au clic sur un lien vers une fiche l’input des filtres rapides saute
    if (value == "-1") {
    document.getElementById("QckUserFltTag_" + quickFilter.nDescId).setAttribute("class", "nopuce");
    oInputElement.value = top._res_435.toUpperCase();
    }
    else {
    document.getElementById("QckUserFltTag_" + quickFilter.nDescId).setAttribute("class", "puce");
    oInputElement.value = displayValue;
    }
    */

}

///
function saveExpressfilterFromList(tab, col, op, value, oExpressFilter) {
    if (!oExpressFilter)
        oExpressFilter = expressFilter; // objet instancié par eList.js

    if (oExpressFilter != null) {
        var bFromInvit = document.getElementById("invitWizardListDiv") != null;
        var bFromSelection = document.getElementById("blockSelectionList") != null;

        var mainDiv = document.getElementById("mainDiv");
        var bFromFinder = getAttributeValue(mainDiv, "edntype") == "lnkfile"
            && getAttributeValue(mainDiv, "tab") != ""
            && getAttributeValue(mainDiv, "tabfrom") != "";

        if (bFromFinder) {
            // appel l'updater pour les filtres express en mode Liste Principale
            var updatePref = "tab=" + tab + ";$;filterExpress=" + col + ";|;" + op + ";|;" + value;
            updateUserFinderPref(updatePref, function () { StartSearch(); });
        }
        else if (oExpressFilter.bFromWidget) {
            if (oListWidget) {
                oListWidget.filter(col, op, value);
            }
        }
        else if (oExpressFilter.nTab == oExpressFilter.nTabFrom || oExpressFilter.nTab == 104000 || oExpressFilter.nTab == 105000 || oExpressFilter.nTab == 107000 || oExpressFilter.nTab == TAB_IMPORTTEMPLATE || oExpressFilter.nTab == 119500) {
            // appel l'updater pour les filtres express en mode Liste Principale
            var updatePref = "tab=" + tab + ";$;filterExpress=" + col + ";|;" + op + ";|;" + value;
            updateUserPref(updatePref, function () {
                loadList();
            });
        }
        else if ((bFromInvit || bFromSelection) && oExpressFilter.nTab != 104000 && oExpressFilter.nTabFrom == 104000) {
            ///appel l'updater pour les filtres express en mode "Ajouter depuis un Filtre"
            if (bFromInvit) {
                setWait(true);
                var oWizardHead = document.getElementById("wizardheader");
                var nTab = getNumber(getAttributeValue(oWizardHead, "bkm"));
            }
            else {
                var nTab = getNumber(document.getElementById("hidTabSource").value);
            }

            var updatePref = "tab=" + nTab + ";$;filterExpress=" + col + ";|;" + op + ";|;" + value + ";|;addfromfilter";
            if (bFromInvit) {


                if (oInvitWizard)
                    updatePref += ";$;deletemode=" + (oInvitWizard.DeleteMode ? "1" : "0") + ";$;targetmode=" + oInvitWizard.Target;

                updateColsPref(updatePref, function () { UpdatePPList(1); });
            }
            else
                updateUserPref(updatePref, function () { updateSelectionList(1); });
        }
        else if (oExpressFilter.nTabFrom > 0) {
            // appel l'updater pour les filtres express en mode signet
            var updatePref = "tab=" + oExpressFilter.nTabFrom + ";$;bkm=" + oExpressFilter.nTab + ";$;filterExpress=" + col + ";|;" + op + ";|;" + value;
            updateUserBkmPref(updatePref, function () { loadBkm(oExpressFilter.nTab); });
        }
        else
            alert("erreur saveExpressfilterFromList : cas non géré");
    }
    else {
        var updatePref = "tab=" + tab + ";$;filterExpress=" + col + ";|;" + op + ";|;" + value;
        setWait(true);
        updateUserPref(updatePref, function () {
            loadList();
            setWait(false);
        });
    }
}

function onCalendarOkFunction() {

}

function CancelExpressFilterDate() {
    var oExpressFilter = expressFilter; // objet instancié par eList.js
    oExpressFilter.oOpenedModal.hide(); //TODO - Envoi à eCalendar.aspx pour fermeture au double click sur le calendrier
    setWindowEventListener('click', hideExpressFilter);
}

function ValidExpressFilterDate(strDate, op, nodeId, frmId) {
    // 36 008 - Internationalisation
    strDate = eDate.ConvertDisplayToBdd(strDate);

    var oExpressFilter = expressFilter; // objet instancié par eList.js

    /*cas de la popup calendar appelée depuis les filtres express de la liste des filtres
    on récupère en plus l'id de la popup pour conserver le contexte d'exécution
    */
    if (frmId != null && frmId != "undefined") {
        oExpressFilter = document.getElementById(frmId).contentWindow["expressFilter"];
        oModalCalendarExpressFilter = document.getElementById(frmId).contentWindow["oModalCalendarExpressFilter"];

        document.getElementById(frmId).contentWindow.saveExpressfilterFromList(oExpressFilter.nTab, oExpressFilter.nDescId, op, strDate);
    }
    else
        saveExpressfilterFromList(oExpressFilter.nTab, oExpressFilter.nDescId, op, strDate, oExpressFilter);


    oExpressFilter.hide();
    oModalCalendarExpressFilter.hide(); //TODO - Envoi à eCalendar.aspx pour fermeture au double click sur le calendrier
}

function doCalendarFilter(oExpressFilter, op) {

    var descId = oExpressFilter.nDescId;
    var tabFrom = oExpressFilter.nTabFrom;
    var tab = oExpressFilter.nTab;
    var wfrmid = "";
    
    if (oExpressFilter.bFromWidget) {
        if (oListWidget && typeof oListWidget.getFrameId == "function") {
            wfrmid = "widgetIframe_" + oListWidget.getFrameId()
        }        
    }

    oModalCalendarExpressFilter = createCalendarPopUp("ValidExpressFilterDate", 1, 1, top._res_5017, top._res_5003, "onCalendarOkFunction", top._res_29, "CancelExpressFilterDate", op, wfrmid);

    oExpressFilter.oOpenedModal = oModalCalendarExpressFilter;
    //Demande #59615 - on enlève cet évènement pour éviter que le drag'n'drop de la modal ferme le filtre express
    removeWindowEventListener('click', hideExpressFilter);
}

// ASY : choix entre 2 dates
var modalBetweenDate;
function doCalendarBetwenFilter(oExpressFilter, op) {

    var descId = oExpressFilter.nDescId;
    var tabFrom = oExpressFilter.nTabFrom;
    var tab = oExpressFilter.nTab;

    var nWith = 430;
    var nHeight = 380;

    if (top.eTools.GetFontSize() >= 14) {
        nWith = 630;
        nHeight = 420;
    }

    modalBetweenDate = new eModalDialog(top._res_5017, 0, "eDateSelect.aspx", nWith, nHeight, "modalBetweenDate");

    modalBetweenDate.addParam("HideNoDate", 1, "post");
    modalBetweenDate.addParam("HideHourField", 1, "post");

    modalBetweenDate.noButtons = false;
    modalBetweenDate.ErrorCallBack = function () { modalBetweenDate.hide(); };

    modalBetweenDate.show();

    modalBetweenDate.addButton(top._res_29, function () { onCalendarBetwenCancelFunction(oExpressFilter) }, 'button-gray', null, "cancel");
    modalBetweenDate.addButton(top._res_28, function () { onCalendarBetweenOKFunction(modalBetweenDate, oExpressFilter) }, 'button-green', null);

    oExpressFilter.oOpenedModal = modalBetweenDate;
    //Demande #59615 - on enlève cet évènement pour éviter que le drag'n'drop de la modal ferme le filtre express
    removeWindowEventListener('click', hideExpressFilter);
}

// ASY 
function onCalendarBetwenCancelFunction(oExpressFilter) {
    oExpressFilter.oOpenedModal.hide();
    //Demande #59615 - on remet l'évènement
    setWindowEventListener('click', hideExpressFilter);
}

// ASY
var OP_BETWEEN = 97;   // entre 2 dates
function onCalendarBetweenOKFunction(myModal, oExpressFilter) {


    if ((oExpressFilter != null) && (myModal != null)) {
        var descId = oExpressFilter.nDescId;

        //var oDoc = myModal.getIframe().document;
        if (myModal.getIframe().eCalendarControlStart && myModal.getIframe().eCalendarControlEnd) {
            //if ((oDoc != null) && (myModal.getIframe()) != null) {

            var strDateStart = eDate.ConvertDisplayToBdd(myModal.getIframe().eCalendarControlStart.GetDate());   //oDoc.getElementById('inputStart_' + descId).value;
            var strDateEnd = eDate.ConvertDisplayToBdd(myModal.getIframe().eCalendarControlEnd.GetDate());   //oDoc.getElementById('inputEnd_' + descId).value;
            var strDates = strDateStart + "$B#W$" + strDateEnd;

            // Check si la date de debut est inferieur a la date de fin
            var dtStart = eDate.Tools.GetDateFromString(strDateStart);
            var dtEnd = eDate.Tools.GetDateFromString(strDateEnd);
            if (dtStart > dtEnd) {
                eAlert(2, top._res_804, top._res_804, '', 500, 200);
            }
            else {
                saveExpressfilterFromList(oExpressFilter.nTab, oExpressFilter.nDescId, OP_BETWEEN, strDates, oExpressFilter);
                oExpressFilter.hide();
                oExpressFilter.oOpenedModal.hide();
            }
        }


    }
}

//var modalStats;
function doStats(oExpressFilter) {
    var myBrowser = new getBrowser();
    var size = top.getWindowSize();
    //modalStats = new eModalDialog(top._res_1395, 0, "eStatisticsOld.aspx", 800, 800, "modalStats");
    top.modalCharts = new eModalDialog(top._res_1395, 0, "eStatistics.aspx", size.w, size.h, "modalStats");

    top.modalCharts.addParam("tab", oExpressFilter.nTab, "post");
    top.modalCharts.addParam("tabfrom", oExpressFilter.nTabFrom, "post");
    if (oExpressFilter.nTab != oExpressFilter.nTabFrom && typeof (GetCurrentFileId) == "function")
        top.modalCharts.addParam("idfrom", GetCurrentFileId(oExpressFilter.nTabFrom), "post");
    top.modalCharts.hideMaximizeButton = true;
    top.modalCharts.addParam("descid", oExpressFilter.nDescId, "post");
    //modalStats.noButtons = true;
    top.modalCharts.ErrorCallBack = function () { top.modalCharts.hide(); };

    top.modalCharts.show();

    //top.modalCharts.addButton(top._res_13, function () {
    //if (myBrowser != null && myBrowser.isIE)
    //        printGrid(top.modalCharts.getIframe());

    // else
    //     printChart(top.modalCharts.getIframe());
    //}, 'button-green', null);
    //top.modalCharts.addButton(top._res_13, function () { printGrid(top.modalCharts.getIframe()); }, 'button-green', null);
    top.modalCharts.addButton(top._res_30, function () { top.modalCharts.hide(); }, 'button-gray', null);

    top.modalCharts.MaxOrMinModal();

    oExpressFilter.hide();
}

function buildTag(tag, innerHtml, styles, attrs) {
    /// <summary>Helper to build jQuery element</summary>
    /// <param name="tag" type="String">tagName#id.cssClass</param>
    /// <param name="innerHtml" type="String"></param>
    /// <param name="styles" type="Object">A set of key/value pairs that configure styles</param>
    /// <param name="attrs" type="Object">A set of key/value pairs that configure attributes</param>
    /// <returns type="jQuery"></returns>
    var tagName = /^[a-z]*[0-9a-z]+/ig.exec(tag)[0];

    var id = /#([_a-z]+[-_0-9a-z]+)/ig.exec(tag);
    id = id ? id[id.length - 1] : undefined;

    var className = /\.([a-z]+[-_0-9a-z ]+)/ig.exec(tag);
    className = className ? className[className.length - 1] : undefined;

    return $(document.createElement(tagName))
        .attr(id ? { "id": id } : {})
        .addClass(className || "")
        .css(styles || {})
        .attr(attrs || {})
        .html(innerHtml || "");
}

function infoBrowser() {
    var browser = {}, clientInfo = [],
        browserClients = {
            opera: /(opera|opr)(?:.*version|)[ \/]([\w.]+)/i, edge: /(edge)(?:.*version|)[ \/]([\w.]+)/i, webkit: /(chrome)[ \/]([\w.]+)/i, safari: /(webkit)[ \/]([\w.]+)/i, msie: /(msie|trident) ([\w.]+)/i, mozilla: /(mozilla)(?:.*? rv:([\w.]+)|)/i
        };
    for (var client in browserClients) {
        if (browserClients.hasOwnProperty(client)) {
            clientInfo = navigator.userAgent.match(browserClients[client]);
            if (clientInfo) {
                browser.name = clientInfo[1].toLowerCase() == "opr" ? "opera" : clientInfo[1].toLowerCase();
                browser.version = clientInfo[2];
                browser.culture = {};
                browser.culture.name = browser.culture.language = navigator.language || navigator.userLanguage;
                if (typeof (ej.globalize) != 'undefined') {
                    var oldCulture = ej.preferredCulture().name;
                    var culture = (navigator.language || navigator.userLanguage) ? ej.preferredCulture(navigator.language || navigator.userLanguage) : ej.preferredCulture("en-US");
                    for (var i = 0; (navigator.languages) && i < navigator.languages.length; i++) {
                        culture = ej.preferredCulture(navigator.languages[i]);
                        if (culture.language == navigator.languages[i])
                            break;
                    }
                    ej.preferredCulture(oldCulture);
                    $.extend(true, browser.culture, culture);
                }
                if (!!navigator.userAgent.match(/Trident\/7\./)) {
                    browser.name = "msie";
                }
                break;
            }
        }
    }
    browser.isMSPointerEnabled = (browser.name == 'msie') && browser.version > 9 && window.navigator.msPointerEnabled;
    browser.pointerEnabled = window.navigator.pointerEnabled;
    return browser;
};

function doLoadAllValues(oExpressFilter) {

    
    var descId = oExpressFilter.nDescId;
    var tabFrom = oExpressFilter.nTabFrom;
    var tab = oExpressFilter.nTab;
   
    var oExpressFilterMagr = new eUpdater("mgr/eExpressFilterManager.ashx", 0);
    oExpressFilterMagr.ErrorCallBack = function () { }
    oExpressFilterMagr.addParam("tab", tab, "post");
    oExpressFilterMagr.addParam("tabfrom", tabFrom, "post");
    oExpressFilterMagr.addParam("enumtype", oExpressFilter.catenum, "post");
    oExpressFilterMagr.addParam("datadesct", oExpressFilter.datadesct, "post");
    
    oExpressFilterMagr.addParam("descid", descId, "post");
    oExpressFilterMagr.addParam("action", "loadallvalues", "post");

    oExpressFilterMagr.send(updateExpressFilterList, oExpressFilter);


    //todo changer le libellé "charger ttes les valeurs" avec "acualiser" res 1127
}

function hideExpressFilter(e) {
    if (expressFilter)
        expressFilter.onClickHide(e);

    if (quickUserFilter)
        quickUserFilter.onClickHide(e);
}