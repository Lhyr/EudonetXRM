import { eFileComponentsMixin } from '../../../mixins/eFileComponentsMixin.js?ver=803000'
import EventBus from '../../../bus/event-bus.js?ver=803000';
import { updateMethod, verifRelation, showTooltip, hideTooltip, updateListVal, showInformationIco, displayInformationIco } from '../../eComponentsMethods.js?ver=803000';
import { selectValue, onUpdateCallback } from '../../eFieldEditorMethods.js?ver=803000';

/**
 * Mixin commune aux composants eRelation.
 * */
export const eRelationMixin = {
    mixins: [eFileComponentsMixin],
    data() {
        return {

            tabLnk: null,
            selectedValues: null,
            selectedLabels: null,
            oModalLnkFile: null,
            //IsDisplayReadOnly: (this.propHead || this.dataInput.ReadOnly),
            that: this,
            evtItem: '',
            oldValue: '',
            bEmptyDisplayPopup: false,
            modified: false,
            modif: false,
            icon: false,
        };
    },
    mounted() {
        this.displayInformationIco();
        if (this.modified === false)
            verifRelation(this.that);
    },
    components: {
        eAlertBox: () => import(AddUrlTimeStampJS("../../modale/alertBox.js"))
    },
    computed: {
        IsDisplayReadOnly: function () {
            return (this.propHead || this.dataInput.ReadOnly)
        },
        msgHover: function () {
            if (this.icon)
                //return 'Ouvrir la relation'
                return this.getRes(1122)
            else if (!this.icon && this.IsDisplayReadOnly)
                return this.getRes(2477)
            else
                return this.getRes(7393)
        }
    },
    methods: {
        displayInformationIco,
        showInformationIco,
        hideTooltip,
        showTooltip,
        selectValue,
        onUpdateCallback,
        updateListVal,
        onLnkFileLoad(iFrameId, nTab) {
            try {
                //var catalogObject = top.eTabLinkCatFileEditorObject[this.oModalLnkFile.iframeId];

                var oFrm = this.oModalLnkFile.getIframe();
                var oFrmDoc = oFrm.document;
                var oFrmWin = oFrm.window;

                // Donne le focus à la textbox de recherche
                if (oFrmDoc.getElementById("eTxtSrch")) {

                    var oInpt = oFrmDoc.getElementById("eTxtSrch");

                    var bIsTablet = false;
                    try {
                        bIsTablet = this.getIsTablet;
                        //if (typeof (isTablet) == 'function')
                        //    bIsTablet = isTablet();
                        //else if (typeof (top.isTablet) == 'function')
                        //    bIsTablet = top.isTablet();
                    }
                    catch (e) {

                    }

                    if (!bIsTablet) {
                        oInpt.focus();
                    }

                    //SPH : conserver ka recherche MRU et mettre la focus dedans (car le controle n'est pas affiché lorsque l'on fait le focus())
                    var val = oInpt.value;
                    oInpt.value = '';
                    oInpt.value = val;

                }
                var oMainDiv = oFrmDoc.getElementById("mainDiv");

                oFrmWin.thtabevt.init();
                oFrmWin.adjustLastCol(nTab, oMainDiv);
                oFrmWin.initHeadEvents();
                oFrmWin.afterListLoaded();
                oFrmWin._parentIframeId = iFrameId;
                /*Gestion d'erreur*/
                var tbError = oFrmWin.document.getElementById("tbError");
                if (tbError) {
                    oFrmWin.showErr(tbError.value);
                }
            }
            finally {
                top.setWait(false);
            }
        },
        async validateLnkFile() {
            //var catalogObject = top.eTabLinkCatFileEditorObject[this.oModalLnkFile.iframeId];

            var oFrm = top.document.getElementById(this.oModalLnkFile.iframeId);

            if (!oFrm)
                return;

            var oFrmDoc = oFrm.contentDocument;
            var oFrmWin = oFrm.contentWindow;
            var selectedListValues = oFrmWin._selectedListValues;
            if (typeof (selectedListValues) == 'undefined')
                selectedListValues = new Array();


            //REMISE A ZERO
            //console.log(catalogObject);
            //console.log(selectedListValues);


            this.selectedValues = new Array();
            this.selectedLabels = new Array();

            if (selectedListValues.length > 0) {
                var oMainDiv = oFrmDoc.getElementById("mainDiv");
                var nTabFrom = getNumber(getAttributeValue(oMainDiv, "tabFrom"));
                var nTargetTab = getNumber(getAttributeValue(oMainDiv, "tab"));
                var nSourceDescId = getNumber(getAttributeValue(oMainDiv, "did"));
                //Table parente PP
                var bPpInFile = this.tabLnk && this.tabLnk.indexOf("200") && nSourceDescId == "200";
                //Table parente PM
                var bPmInFile = this.tabLnk && this.tabLnk.indexOf("300") && nSourceDescId == "300";
                //La table ciblée est une liaison haute de la table parente mas n'est pas PP ou PM
                var bEventInFile = (this.tabLnk && this.tabLnk.indexOf(this.dataInput.TargetTab) >= 0 && nSourceDescId == this.dataInput.TargetTab && this.dataInput.TargetTab != "200" && this.dataInput.TargetTab != "300");

                for (var i = 0; i < selectedListValues.length; i++) {
                    var oItem = oFrmDoc.getElementById(selectedListValues[i]);

                    if (!oItem)
                        continue;


                    // id  
                    var oId = oItem.getAttribute("eid").split('_');
                    var nTab = oId[0];
                    var nId = oId[oId.length - 1];

                    //kha : depuis un future catalogue à choix multiple, cela ne fonctionnera plus à cause des cases à cocher
                    // on pourra alors tester ceci : 
                    var tabTd = oItem.getElementsByTagName("td");
                    //var ename = "COL_" + nTab + "_" + (nTab+1);
                    //var tabTd = oItem.querySelector("td[ename='"+ename+"']");

                    //Libellé de la première colonne
                    var label = GetText(tabTd[0]);
                    /*Depuis Contact/Société Récupération des informations sur l'adresse et la Société/Contact sélectionnée*/
                    var nAdrId = -1;
                    var sAdr01 = "";
                    var nPmId = -1;
                    var sPm01 = "";
                    var nPpId = -1;
                    var sPp01 = "";
                    if (bPpInFile || bPmInFile || bEventInFile) {
                        for (var j = 0; j < tabTd.length; j++) {
                            //GCH : on ne peut affecter que les fiches dont on a les droits de visu.
                            if (getAttributeValue(tabTd[j], "eNotV") != "1") {
                                //Recherche 400
                                if (nAdrId < 0 && tabTd[j].id && getAttributeValue(tabTd[j], "ename").search("^COL_[2-3]00_4[0-9]{2}$") == 0) { //ADDRESS depuis PP ou PM
                                    nAdrId = GetFieldFileId(tabTd[j].id);
                                    sAdr01 = GetText(tabTd[j]);
                                }
                                //Recherche 300
                                else if (nPmId < 0 && tabTd[j].id
                                    && (
                                        (getAttributeValue(tabTd[j], "ename").search("^COL_[2-3]{1}00_400_3[0-9]{2}$") == 0)    //PM depuis PP
                                        ||
                                        (bEventInFile && getAttributeValue(tabTd[j], "ename").search("^COL_" + nTargetTab + "_301$") == 0)    //PM depuis EVENT

                                    )
                                ) {
                                    nPmId = GetFieldFileId(tabTd[j].id);
                                    sPm01 = GetText(tabTd[j]);
                                }
                                //Recherche 200
                                else if (nPpId < 0 && tabTd[j].id
                                    && (
                                        (getAttributeValue(tabTd[j], "ename").search("^COL_[2-3]{1}00_400_2[0-9]{2}$") == 0)    //PP depuis PM
                                        ||
                                        (bEventInFile && getAttributeValue(tabTd[j], "ename").search("^COL_" + nTargetTab + "_201$") == 0)  //PP depuis EVENT
                                    )

                                ) {
                                    nPpId = GetFieldFileId(tabTd[j].id);
                                    sPp01 = GetText(tabTd[j]);
                                }
                            }

                        }
                    }

                    //// Si on choisit PM à partir d'ADDRESS, on n'inclut pas le ppid
                    //if (!(nTargetTab == 300 && nTabFrom == 400))
                    //nId = nId + ";|;" + nPpId + '$|$' + sPp01;
                    //nId = nId + ";|;" + nPmId + '$|$' + sPm01;
                    //nId = nId + ";|;" + nAdrId + '$|$' + sAdr01;

                    this.selectValue(nId, label, true);
                }
            }
            else
                this.selectValue("", "", true);


            var TabDescId;
            if (this.propSignet == undefined) {
                TabDescId = this.getTab
            } else {
                TabDescId = this.propSignet.DescId
            }

            //let updateData;

            //if (this.selectedValues[0] != '' && this.dataInput.Required || !this.dataInput.Required) {

            //    updateData = {
            //        'Fields': [
            //            {
            //                'Descid': this.dataInput.DescId,
            //                'NewValue': this.selectedValues.join(';')
            //            }
            //        ],
            //        'TabDescId': TabDescId,
            //        'FileId': this.getFileId
            //    };
            //}

            //const { default: eAxiosHelper } = await import("../../helpers/eAxiosHelper.js");
            //let helper = new eAxiosHelper(this.$store.state.url + '/api/createupdate');

            //try {
            //    //var updateReturn = await helper.PutAsync(updateData);
            //    await helper.PutAsync(updateData);
            //    this.onUpdateCallback(true, { Values: this.selectedValues.join(";"), Labels: this.selectedLabels.join(";") });
            //    this.oModalLnkFile.hide();
            //} catch (err) {
            //    if (err.response && (err.response.status < 200 || err.response.status > 299)) {
            //        this.onUpdateCallback(false, { Values: this.selectedValues.join(";"), Labels: this.selectedLabels.join(";") }, err);
            //        this.oModalLnkFile.hide();
            //        return;
            //    }
            //    this.onUpdateCallback(false, { Values: this.selectedValues.join(";"), Labels: this.selectedLabels.join(";") }, err);
            //    this.oModalLnkFile.hide();
            //}

            updateMethod(this, this.selectedValues.join(';'), undefined, undefined, this.dataInput);

            this.oModalLnkFile.hide();

            // demande 36826 : MCR :  Pour gérer les appels entrants des CTIs
            //top.nModalLnkFileLoaded = top.nModalLnkFileLoaded - 1;

        },
        cancelLnkFile() {
            this.oModalLnkFile.hide();
        },
        openLnkFileDialog() {
            var that = this;
            var nSearchType = 2, targetTab, bBkm, onCustomOk, paramSup, sSearchValue, nCallFrom, bNoLoadFileAfterValid;
            top.setWait(true);

            //#33286
            sSearchValue = this.dataInput.DisplayValue;

            if (typeof sSearchValue == "undefined")
                sSearchValue = "";

            var descId = this.dataInput.DescId;
            targetTab = this.dataInput.TargetTab;
            var fileId = this.getFileId;


            /*Uservalue*/
            //Récup des info des champs affichés actuellement sur la fiche en cours

            var nFieldTab = this.getTab;

            //var aUvFldValue = top.getFieldsInfos(nFieldTab, fileId);
            /*Fin Uservalue*/

            var nMode = "1";    //Chercher
            var strTitle = this.getRes(10); //Chercher
            if (nSearchType == 1) {
                strTitle = this.getRes(18); //Ajouter
                if (bBkm)
                    var nMode = "3";    //Ajouté depuis bkm
            }
            else if (nSearchType == 2 || nSearchType == 3 || nSearchType == 4) {
                strTitle = this.getRes(73); //Associer
                nMode = "0";
            }
            else if (nSearchType == 6) {
                strTitle = this.getRes(8747); // Sélectionner
                nMode = "4";
            }
            else if (nSearchType == 5) {
                //-------------- demande 36826 : MCR/RMA :  Pour gérer les appels entrants des CTIs, barre de titre modale        
                if (top.nModalLnkFileLoaded > 0) // si des modales sont deja ouvertes alors affichage message alert !!
                {
                    setWait(false);
                    // MCR 39400 : ajout de libelles dans res
                    eAlert(0, this.getRes(6771), this.getRes(6761), '<br>');
                    return;
                }

                strTitle = strTitle + " : " + sSearchValue;

            }


            var oTabWH = top.getWindowWH(top);
            var maxWidth = 1000; //Taille max à l'écran (largeur)
            var maxHeight = 550; //Taille max à l'écran (hauteur)
            var width = oTabWH[0];
            var height = oTabWH[1];

            //si largeur de l'écran plus grande que largeur max alors il faut utiliser la largeur max
            width = width > maxWidth ? maxWidth : width - 10;
            //si hauteur de l'écran plus grande que largeur max alors il faut utiliser la largeur max
            height = height > maxHeight ? maxHeight : height - 10;


            if (this.parentPopup)
                this.parentPopup.hide();

            if (this.oModalLnkFile != null) {
                try {
                    this.oModalLnkFile.hide();
                    if (!(this.oModalLnkFile.bScriptOk && this.oModalLnkFile.bBodyOk))
                        top.setWait(false);

                }
                catch (e) {
                    debugger;
                }
            }

            this.oModalLnkFile = new eModalDialog(strTitle, 0, 'mgr/eFinderManager.ashx', width, height, "modalFinder");
            this.oModalLnkFile.NoGlobalCLose = true;
            this.oModalLnkFile.addScript("eTools");
            this.oModalLnkFile.addScript("eUpdater");
            this.oModalLnkFile.addScript("eGrapesJSEditor");
            this.oModalLnkFile.addScript("eMemoEditor");
            this.oModalLnkFile.addScript("eExpressFilter");
            this.oModalLnkFile.addScript("eContextMenu");
            this.oModalLnkFile.addScript("eModalDialog");
            this.oModalLnkFile.addScript("eEngine");
            this.oModalLnkFile.addScript("eMain");
            this.oModalLnkFile.addScript("eList");
            this.oModalLnkFile.addScript("eFinder");
            this.oModalLnkFile.addScript("ePopup");

            this.oModalLnkFile.addCss("eMain");
            this.oModalLnkFile.addCss("eIcon");
            this.oModalLnkFile.addCss("eControl");
            this.oModalLnkFile.addCss("eTitle");
            this.oModalLnkFile.addCss("eContextMenu");
            this.oModalLnkFile.addCss("eModalDialog");
            this.oModalLnkFile.addCss("eMemoEditor");
            this.oModalLnkFile.addCss("eList");
            //if (this.multiple)
            //    this.oModalLnkFile.addCss("eActionList");
            this.oModalLnkFile.addCss("eFinder");
            this.oModalLnkFile.addCss("eudoFont");
            this.oModalLnkFile.addCss("theme");

            var myFunct = (function (obj) {
                return function () {
                    obj.hide();
                    top.setWait(false);
                }
            })(this.oModalLnkFile);

            this.oModalLnkFile.ErrorCallBack = myFunct;

            //On ajoute le finder à la liste des finder OUVERT
            //top.eTabLinkCatFileEditorObject.Add(this.oModalLnkFile.iframeId, this);

            /*Début - Uservalue - Envoi des informations de d'uservalue à la modale*/
            var uvflstDetail = "";
            //Liste des séparateurs utilisé pour construire des liste, des listes de tableaux...
            var SEPARATOR_LVL1 = "#|#";
            var SEPARATOR_LVL2 = "#$#";

            //ne pas transférer les champs note pour les uservalue

            /*
            var oLstMemoField = document.getElementById("memoIds_" + nFieldTab)
            var aLstMemoField = new Array();
            if (oLstMemoField != null && oLstMemoField.value)
                aLstMemoField = oLstMemoField.value.split(";");
                */
            //var bCheckMemo = (Array.prototype.indexOf && typeof (Array.prototype.indexOf) == "function" && aLstMemoField.length > 0);

            //Nom du param = (IsFound$|$Parameter$|$Value$|$Label)

            /*

            for (var i = 0; i < aUvFldValue.length; i++) {
                if (!aUvFldValue[i])
                    continue;

                //ne pas transférer les champs note pour les uservalue
                if (bCheckMemo && aLstMemoField.indexOf(aUvFldValue[i].cellId) != -1)
                    continue;

                if (uvflstDetail != "")
                    uvflstDetail = uvflstDetail + SEPARATOR_LVL1;
                try {
                    uvflstDetail = uvflstDetail + "1" + SEPARATOR_LVL2 + aUvFldValue[i].descId + SEPARATOR_LVL2 + aUvFldValue[i].newValue + SEPARATOR_LVL2 + aUvFldValue[i].newLabel;
                }
                catch (e) {
                    continue;
                }
            }
            
            try {
                //Ecran de duplication en masse.
                if (aUvFldValue.length == 0) {
                    var divDupli = this.sourceElement.ownerDocument.getElementById("dupliTreatSelectFields");
                    if (divDupli) {
                        var oTrs = divDupli.querySelectorAll("TR");
                        for (var i = 0; i < oTrs.length; i++) {
                            var tr = oTrs[i];
                            var cb = tr.querySelector("a.chk[id^='chk_chkDup'][chk='1']");
                            if (!cb) {
                                continue;
                            }
                            var edndescid = getNumber(getAttributeValue(cb.parentElement, "edndescid"));
                            if (getTabDescid(edndescid) != getNumber(getAttributeValue(divDupli, "tab")))
                                edndescid = getTabDescid(edndescid);
                            var input = tr.querySelector("INPUT[ednvalue]");
                            var newValue = getAttributeValue(input, "ednvalue");
                            var newLabel = input.value;
                            if (uvflstDetail != "")
                                uvflstDetail = uvflstDetail + SEPARATOR_LVL1;
                            try {
                                uvflstDetail = uvflstDetail + "1" + SEPARATOR_LVL2 + edndescid + SEPARATOR_LVL2 + newValue + SEPARATOR_LVL2 + newLabel;
                            }
                            catch (e) {
                                continue;
                            }

                        }
                    }
                }
            }
            catch (ex) {
            }
            

            // #47175 : On ajoute la liaison haute si elle existe
            
            if (top.nGlobalActiveTab) {
                if (typeof (top.GetCurrentFileId) !== 'undefined') {
                    if (top.GetCurrentFileId(top.nGlobalActiveTab) != "") {
                        if (uvflstDetail != "")
                            uvflstDetail = uvflstDetail + SEPARATOR_LVL1;
                        uvflstDetail = uvflstDetail + "1" + SEPARATOR_LVL2 + top.nGlobalActiveTab + SEPARATOR_LVL2 + top.GetCurrentFileId(top.nGlobalActiveTab) + SEPARATOR_LVL2 + "";
                    }
                }

            }
            


            //Liste des champs concerné par le uservalue
            this.oModalLnkFile.addParam('UserValueFieldList', encode(uvflstDetail), "post");
            //Fin - Uservalue - Envoi des informations de d'uservalue à la modale

            if (paramSup && paramSup != "") {
                var tabParamSup = paramSup.split(SEPARATOR_LVL1);
                for (cpt = 0; cpt < tabParamSup.length; cpt++) {
                    var currentValues = tabParamSup[cpt];
                    var tabCurrentParamSup = currentValues.split(SEPARATOR_LVL2);
                    if (tabCurrentParamSup[0] != "" && tabCurrentParamSup.length > 1) {
                        this.oModalLnkFile.addParam(tabCurrentParamSup[0], tabCurrentParamSup[1], "post");
                    }
                }
            }

           

            //on rajoute ici un objet contenant les valeurs à insérer dans une nouvelle fiche.
            var assDescId = descId.toString() == "400" ? "200" : descId.toString()
            var arrAssFields = document.querySelectorAll("[assfld^='[" + assDescId + "]_']");

            var defValues = new Object();
            for (var i = 0; i < arrAssFields.length; i++) {
                var assFldElt = arrAssFields[i];
                try {
                    var assFldDid = getAttributeValue(assFldElt, "assfld").split('_')[1].replace("[", "").replace("]", "");
                }
                catch (e) {
                    continue;
                }

                //Formaté pour être desérialisé en Dictionaire C# dans eFileDisplayer.
                var value = getAttributeValue(assFldElt, "dbv");
                if (getAttributeValue(assFldElt, "eaction") == "LNKCHECK") {
                    value = assFldElt.querySelector("a[chk]").getAttribute("chk");
                }
                defValues[assFldDid] = value ? value : assFldElt.value;

                if (assFldDid == getNumber(descId) + 1 && sSearchValue == "") {
                    sSearchValue = defValues[assFldDid];
                }
            }

            this.oModalLnkFile.addParam("defvalues", JSON.stringify(defValues), "post");

            */


            //Table sur laquelle on recherche
            this.oModalLnkFile.addParam("targetTab", targetTab, "post");
            //id de la fiche de départ
            this.oModalLnkFile.addParam("FileId", fileId, "post");
            //Champ catalogue sur la fiche de départ
            this.oModalLnkFile.addParam("targetfield", descId, "post");

            //Table de départ
            this.oModalLnkFile.addParam("tabfrom", this.getTab, "post");
            //Table de départ
            this.oModalLnkFile.addParam("NameOnly", false ? "1" : "0", "post");

            // valeur recherchée 		#33286
            this.oModalLnkFile.addParam("Search", sSearchValue, "post");


            //-------------- demande 36826 : MCR/RMA :  Pour gérer les appels entrants des CTIs
            if (nSearchType == 5) {
                this.oModalLnkFile.addParam("action", "cti", "post");   // CAS du 'CTI'
                this.oModalLnkFile.addParam("pn", sSearchValue, "post");   // ajout du phone number : pn comme parametre du post
            }
            else
                this.oModalLnkFile.addParam("action", "dialog", "post");

            //Si a 1 chaque ligne est cliquable et permet de rediriger vers la fiche correspondant à la ligne sélectionnée
            this.oModalLnkFile.addParam("eMode", nMode, "post");
            //Type de recherche demandée
            this.oModalLnkFile.addParam("SearchType", nSearchType, "post");

            // si le champ se trouve dans une popup, on transmet l'id de la frame

            /*
            if (this.sourceElement && this.sourceElement.ownerDocument != top.document) {

                for (var i = 0; i < top.frames.length; i++) {
                    // kha le 27 juin 2016 je rajoute un try catch car dans le cas où  une iframe contient une page exterieure 
                    // cela provoque une erreur cross-origin
                    try {
                        if (top.frames[i].document == this.sourceElement.ownerDocument) {
                            this.oModalLnkFile.addParam("origframeid", top.frames[i]._parentIframeId, "post");
                            break;
                        }
                    }
                    catch (exc) {
                    }
                }
            }
            */
            /*
            //Récup des MRU si vide (cas de la recherche étendue et de nouveau)
            if (nMode == "1") {
                var oeParam = getParamWindow();
                if (oeParam.GetMruParam)
                    this.mruParamValue = oeParam.GetMruParam(descId);
            }
            
            //MRU :
            this.oModalLnkFile.addParam("MRU", encode(this.mruParamValue), "post");
            */

            this.oModalLnkFile.addParam("callfrom", nCallFrom, "post");
            this.oModalLnkFile.addParam("noloadfile", bNoLoadFileAfterValid ? "1" : "0", "post");

            this.oModalLnkFile.onIframeLoadComplete = (function (iframeId, nTab) { return function () { that.onLnkFileLoad(iframeId, nTab); } })(this.oModalLnkFile.iframeId, targetTab);

            // demande 36826 : MCR :  Pour gérer les appels entrants des CTIs
            //top.nModalLnkFileLoaded = top.nModalLnkFileLoaded + 1;

            this.oModalLnkFile.show();

            this.tabLnk = top.getTabFileLnkId(this.getTab);
            if (typeof (onCustomOk) == "undefined" || onCustomOk == "")
                onCustomOk = this.validateLnkFile;

            if ((nMode == "1") || (nMode == "3")) {
                //Recherche ou ajouter pas de sélection de valeurs donc pas de bouton valider
                this.oModalLnkFile.addButton(this.getRes(30), this.cancelLnkFile, 'button-gray', this.oModalLnkFile.iframeId, "cancel");   //Fermer
            }
            else {
                //Champ de liaison
                this.oModalLnkFile.addButton(this.getRes(29), this.cancelLnkFile, 'button-gray', this.oModalLnkFile.iframeId, "cancel");   //Annuler
                this.oModalLnkFile.addButton(this.getRes(28), onCustomOk, "button-green", this.oModalLnkFile.iframeId, "ok"); // Valider

                // Si catalogue champ de liaison et valeurs déjà sélectionnée on affiche Dissocier
                if (this.dataInput.Value.length > 0)
                    this.oModalLnkFile.addButton(this.getRes(6333),
                        (function (frmId, okFct) { return function () { dissociateLnkFile(frmId, okFct); } })(this.oModalLnkFile.iframeId, onCustomOk),
                        "button-red", null, "dissociate"); // Dissocier
            }
        },

        emitMethod() {
            let options = {
                typeModal: "alert",
                color: "info",
                type: "zoom",
                close: true,
                maximize: true,
                id: 'alert-modal',
                title: this.getRes(7905),
                width: 600,
                btns: [{
                    lib: this.getRes(30),
                    color: 'default',
                    type: 'left'

                }],
                datas: this.getRes(6368)
            }
            EventBus.$emit('globalModal', options);
        },
        goAction(evt) {
            this.modified = false;
            this.oldValue = this.propListe ? this.$refs.relationlist.text : this.$refs.relationfile.text;
            if (evt.target.classList.contains("targetIsTrue")) {
                var ntab = this.dataInput.TargetTab;
                var tabOrder = document.getElementById('eParam').contentWindow.document.getElementById('TabOrder')
                var tab = tabOrder.value.split(';');
                var ntabFound = tab.find(function (element) {
                    return element == ntab;
                });

                if (ntabFound == undefined) {
                    this.emitMethod()
                } else {
                    top.loadFile(ntabFound, this.dataInput.Value, 3)
                }

            } else {
                this.openLnkFileDialog()
            }

        },
        showVCardOrMiniFile(event, show) {
            if (this.dataInput.IsMiniFileEnabled)
                shvc(event.target, show);
        },
        verifRelation
    },
    props: ["dataInput", "propHead", "propListe", "propSignet", "propIndexRow", "propAssistant", "propDetail", "propAssistantNbIndex"],
}