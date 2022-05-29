import EventBus from '../../bus/event-bus.js?ver=803000';
import { updateMethod, focusInput, verifComponent, verifRelation, showTooltip, hideTooltip, updateListVal, showInformationIco, displayInformationIco, showCatalogGeneric, showVCardOrMiniFile } from '../../methods/eComponentsMethods.js?ver=803000';
import { selectValue, onUpdateCallback, validateCatGenericIris, cancelCatGenericIris } from '../../methods/eFieldEditorMethods.js?ver=803000';
import { getTabDescid } from "../../methods/eMainMethods.js?ver=803000";
import { PropType } from '../../methods/Enum.min.js?ver=803000';
import { PJField } from '../../methods/Enum.js?ver=803000';
import { eFileComponentsMixin } from '../../mixins/eFileComponentsMixin.js?ver=803000';
import containerModal from '../modale/containerModal.js?ver=803000';
import { store } from '../../../Scripts/store/store.js?ver=803000';
import { setMruParams } from '../../shared/XRMWrapperModules.js?ver=803000';



export default {
    name: "eRelation",
    data() {
        return {
            tabLnk: null,
            selectedValues: null,
            selectedLabels: null,
            oModalLnkFile: null,
            that: this,
            evtItem: '',
            oldValue: '',
            bEmptyDisplayPopup: false,
            modified: false,
            modif: false,
            icon: false,
            instance: "",
            showMru: false,
            tabsMru: Object,
            dataInputValue: ""
        };
    },
    created(){
        this.dataInputValue = this.LnkId == "0" ? "" : (this.dataInput.DisplayValue || this.dataInput.Value);
    },
    mounted() {
        this.setContextMenu();
        this.displayInformationIco();
        if (this.modified === false)
            verifRelation(this.that);
    },
    components: {
        eAlertBox: () => import(AddUrlTimeStampJS("../modale/alertBox.js")),
        eMru: () => import(AddUrlTimeStampJS("../eMru/eMru.js")),
        containerModal: () => import(AddUrlTimeStampJS("../modale/containerModal;js"))
    },
    watch: {
        dataInput: {
            handler: function (val) {
                this.dataInputValue = this.LnkId == "0" ? "" : (val.DisplayValue || val.Value)
            },
            deep: true
        }, 
    },
    computed: {
        IsDisplayReadOnly: function () {
            return (this.dataInput.ReadOnly)
        },
        blankField() {
            if (this.IsDisplayReadOnly && this.dataInput.DescId == 301) {
                return "blank-field"
            }
        },
        msgHover: function () {
            if (this.icon)
                //return 'Ouvrir la relation'
                return this.getRes(1122)
            else if (!this.icon && this.IsDisplayReadOnly)
                return this.getRes(2477)
            else
                return this.getRes(7393)
        },
        classMru: function () {
            return (this.showMru) ? 'mru-opened' : 'multiRenderer form-control';
        },
        mruMode: function () {
            return (this.showMru) ? 'mru-mode' : '';
        },
        LnkId: function () {
            // MAB - US #1586 - Tâche #3265/3268 - Demande #75 895 - Minifiche sur les champs Alias
            // Equivalent côté Front de eLibTools.GetLnkId() côté Back
            // Dans le cas du JavaScript, on peut utiliser Value systématiquement, car le travail est fait sur AliasRelationDataFieldModel côté back
            return this.dataInput.IsMainField ? this.dataInput.FileId : this.dataInput.Value;
        },
        TargetTab: function () {
            if (this.dataInput.DescId == PJField.FILEID)
                return this.dataInput.PJTargetTab;

            if (this.dataInput.TargetTab > 0)
                return this.dataInput.TargetTab;

            return getTabDescid(this.dataInput.DescId);
        },
        fieldDisplayValue: function () {
            return this.dataInputValue;
        },
        getInput:{
            get: function(){
                return this.dataInput;
            },
            set:function(input){
                let dataInput = {...this.dataInput,...input}
                this.$emit('update:data-input',dataInput)
            }
        }
    },
    methods: {
        displayInformationIco,
        showInformationIco,
        hideTooltip,
        showTooltip,
        selectValue,
        updateMethod,
        onUpdateCallback,
        updateListVal,
        showVCardOrMiniFile,
        getTabDescid,
        setMruParams,
        showMruClick() {
            LoadMruRelation();
        },
        async LoadMruRelation(event) {

            if (!this.$refs.MRU) {
                this.openLnkFileDialog();

                return false;
            }
            try {
                await this.$refs.MRU.LoadMru(this.TargetTab);
            } catch (e) {
                EventBus.$emit('globalModal', {
                    typeModal: "alert", color: "danger", type: "zoom",
                    close: true, maximize: false, id: 'alert-modal', title: this.getRes(6576), msgData: e, width: 600,
                    btns: [{ lib: this.getRes(30), color: 'default', type: 'left' }], datas: this.getRes(7050)
                });
                return;
            }
            if (this.$refs.MRU?.DataMru && this.$refs.MRU?.DataMru.length > 0) {
                this.showMru = true;
            } else {
                this.goAction(event);
            }
        },
        /**
         * Cette méthode ouvre une modale qui permet de présenter les vues qui ont l'onglet où l'on désire naviguer
         * @param {any} obj vues qui contiennent l'onglet sur lequel on souhaite aller
         * @param {any} viewElm vue choisie
         */
        emitViewModale(obj, viewElm, targetTab, value) {
            if (obj.length < 2) {
                viewElm = obj[0].view;
            }
            let modalTitle;
            if (obj.length > 1) {
                //Cet onglet n'est pas affiché dans votre barre d'onglet, choississez la vue vers laquelle naviguer pour afficher l'onglet :
                modalTitle = this.getRes(2591);
            } else {
                //Cet onglet n'est pas affiché dans votre barre d'onglet, souhaitez-vous naviguer vers la vue XXX pour afficher l'onglet ?;
                modalTitle = this.getRes(2592).replace("{0}", obj[0].name);
            }

            let options = {
                id: "MotherOfAllModals",
                class: "modal-motherofAll",
                style: {
                    //heigth: document.querySelector('#MainWrapper') ? document.querySelector('#MainWrapper').offsetHeight + "px" : "",
                    width: document.querySelector('#MainWrapper') ? document.querySelector('#MainWrapper').offsetWidth + "px" : ""
                },
                actions: [],
                header: {
                    text: this.dataInput.TargetTabLabel ? this.dataInput.TargetTabLabel : this.dataInput.Label ? this.dataInput.Label : this.getRes(805),
                    class: "modal-header-motherofAll modal-header-motherofAll-Max relation-modal",
                    btn: [
                        {
                            name: 'close', class: "icon-edn-cross titleButtonsAlignement", action: () => {

                                [...this.$root.$children.find(x => x.$options.name == "App").$el.children].find(x => x.className == "containerModal") ? this.$root.$children.find(x => x.$options.name == "App").$el.removeChild(this.instance.$el) : '';
                            }
                        }
                    ]
                },
                main: {
                    class: "detailContent modal-content-motherofAll modal-content-motherofAll-Max relation-modal",
                    componentsClass: "grid-container form-group relation-container",
                    title: modalTitle,
                    dropdown: {
                        //class: {
                        //    select: "select-relation",
                        //    option: "select-option",
                        //},
                        optionSelected: this.getRes(2596),
                        opt: obj,
                        // A chaque changement d'option dans le dropdown par l'utilisateur, on met à jour la la vue sélectionnée
                        action: (options, view) => {
                            view.propDropdown.optionSelected = options.name;
                            viewElm = options.view;
                            view.$parent.$options.name == "MotherOfAllModals" ? view.$parent.modalAlert = false : "";
                        }
                    },
                    alert: [
                        {
                            value: "",
                            class: "fas fa-exclamation-circle"
                        },
                        {
                            value: this.getRes(2603),
                            class: "alert-content"
                        }
                    ]
                },
                footer: {
                    class: "modal-footer-motherofAll modal-footer-motherofAll-Max relation-modal",
                    btn: [
                        {
                            title: this.getRes(28), class: "btnvalid eudo-button btn btn-success", action: (target) => {
                                //On récupère l'id de la vue et on appelle changeView()
                                if (!viewElm) {
                                    target.$parent.modalAlert = true;
                                    //target.modalAlert = true;
                                    return;
                                }

                                // US #1330 - Tâches #2748, #2750 - On paramètre les variables utilisées par eParamIFrame.eParamOnLoad() pour recharger le TabID, FileID, type d'affichage actuels après rafraîchissement total via loadTabs()
                                // qui est appelée par changeView()
                                nsMain.setParamIFrameReloadContext(targetTab, value, "FILE_MODIFICATION", undefined, undefined, true);
                                // Et on déclenche la chaîne de fonctions : changeView => loadTabs => eParamOnLoad() => ...
                                top.changeView(viewElm);
                            },
                            disabled: 'true'
                        },
                        {
                            title: this.getRes(29), class: "btncancel eudo-button btn btn-default", action: (ctx) => {

                                this.instance.$destroy();
                                [...this.$root.$children.find(x => x.$options.name == "App").$el.children].find(x => x.className == "containerModal") ? this.$root.$children.find(x => x.$options.name == "App").$el.removeChild(this.instance.$el) : '';
                            }
                        }
                    ]
                },
            };
            this.modalOptions = options;
            // if (this.dataInput.IsHtml) {
            //EventBus.$emit("MotherOfAllModals", options);

            var ComponentClass = Vue.extend(containerModal);
            //var ComponentClass = this.$root.extend(containerModal);
            this.instance = new ComponentClass({
                propsData: { 'propOptionsModal': options },
                store: store
            })
            this.instance.$mount();
            this.$root.$children.find(x => x.$options.name == "App").$el.appendChild(this.instance.$el);
        },
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
                var bEventInFile = (this.tabLnk && this.tabLnk.indexOf(this.TargetTab) >= 0 && nSourceDescId == this.TargetTab && this.TargetTab != "200" && this.TargetTab != "300");

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

            updateMethod(this, this.selectedValues.join(';'), undefined, undefined, this.dataInput);

            this.setMruParams(this.TargetTab, this.selectedValues.join(';'),this.selectedLabels.join(';'))

            this.oModalLnkFile.hide();

        },
        cancelLnkFile() {
            this.oModalLnkFile.hide();
        },
        openLnkFileDialog() {
            // #85 993 - A l'ouverture d'une boîte de dialogue, fermeture des MRU
            this.closeMru();

            var that = this;
            var nSearchType = 2, targetTab, bBkm, onCustomOk, paramSup, sSearchValue, nCallFrom, bNoLoadFileAfterValid;
            top.setWait(true);

            //#33286
            sSearchValue = this.dataInput.DisplayValue;

            if (typeof sSearchValue == "undefined")
                sSearchValue = "";

            var descId = this.dataInput.DescId;
            targetTab = this.TargetTab || this.getTabDescid(this.dataInput.DescId);
            var fileId = this.getFileId;


            /*Uservalue*/
            //Récup des info des champs affichés actuellement sur la fiche en cours

            var nFieldTab = this.getTab;

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
            top.eTabLinkCatFileEditorObject.Add(this.oModalLnkFile.iframeId, this);

            /*Début - Uservalue - Envoi des informations de d'uservalue à la modale*/
            var uvflstDetail = "";
            //Liste des séparateurs utilisé pour construire des liste, des listes de tableaux...
            var SEPARATOR_LVL1 = "#|#";
            var SEPARATOR_LVL2 = "#$#";

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

            this.oModalLnkFile.addParam("callfrom", nCallFrom, "post");
            this.oModalLnkFile.addParam("noloadfile", bNoLoadFileAfterValid ? "1" : "0", "post");

            this.oModalLnkFile.onIframeLoadComplete = (function (iframeId, nTab) { return function () { that.onLnkFileLoad(iframeId, nTab); } })(this.oModalLnkFile.iframeId, targetTab);

            // demande 36826 : MCR :  Pour gérer les appels entrants des CTIs
            //top.nModalLnkFileLoaded = top.nModalLnkFileLoaded + 1;

            this.oModalLnkFile.show();

            this.tabLnk = top.getTabFileLnkId(this.getTab);
            if (typeof (onCustomOk) == "undefined" || onCustomOk == "") {
                onCustomOk = (() => this.validateLnkFile)().bind(this);
            }

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
        /*
         * Ouvre la PJ sélectionnée, ou renvoie son URL selon paramètre
         * Portage de eMain.SetFldPj()
         */
        openPJ(pjInfo, action, bReturnUrl) {
            if (!pjInfo)
                return;

            /*#region Modication d'une PJ*/
            var sAction = "";
            var sActionUrl = "";

            var url = (pjInfo.PJSrcUrl + "").trim() || this.dataInput.Value;
            var idPj = this.dataInput.Value;
            if (!Number(idPj))
                idPj = pjInfo.PJFileID;

            //Sauvegarde la pj selectionnée.
            SelectedPjId = idPj;

            // On détermine le type de lien à construire en fonction du type d'annexe
            var sLnkType = '';
            switch (pjInfo.PJType) {
                case "0":                 // Fichier
                case "6":                 // Fichier         
                case "7":                 // Fichier
                    sLnkType = 'pjDisplay';
                    break;
                case "1":                 //Fichier local : KO           
                case "5":                 //Repertoire ne marche que sur des répertoires partagés réseaux
                    sLnkType = 'file';
                    break;
                case "2":                 //Mail: ok
                    sLnkType = 'mailto';
                    break;
                case "3":                 //site web : ok
                    sLnkType = 'http';
                    break;
                case "4":                 //FTP : OK
                    sLnkType = 'ftp';
                    break;
                default:
                    sLnkType = 'pjDisplay';
                    break;
            }

            // Si on souhaite utilise un lien direct alors que l'URL n'est pas renseignée (cas du clic sur le bouton de la visionneuse : on ne peut
            // pas récupérer l'URL via innerHTML), on se repose sur pjDisplay
            if (sLnkType != 'pjDisplay' && url == '')
                sLnkType = 'pjDisplay';

            var nTab = this.getTab;
            var fileId = this.getFileId;
            var sDispFrom = 'pj';

            // Puis on construit le lien adapté
            switch (sLnkType) {

                case "pjDisplay":
                    if (nGlobalActiveTab == TAB_PJ) {
                        sDispFrom = 'pjtab';
                        if (fileId == "" || nTab == "") {
                            // sur la table des pj, on doit chercher la table sur la ligne de la pj
                            nTab = pjInfo.PJTabDescID;
                            fileId = pjInfo.PJFileID;
                        }
                    }


                    // On refuse l'affichage de la PJ si les ID passés ne sont pas des entiers.
                    // On accepte toutefois qu'ils soient à 0 (ce qui équivaut à Number(nTab) == false) pour autoriser l'affichage de PJ de
                    // fiches en cours de création.
                    // C'est ePjDisplay, appelée par l'URL renvoyée ci-dessous, qui se chargera de refuser l'affichage si elle détermine que
                    // le FileID est passé à 0 alors qu'il existe bien une fiche rattachée à l'annexe.
                    if (fileId == null || nTab == null || typeof (fileId) == 'undefined' || typeof (nTab) == 'undefined')
                        return;

                    if (Number(fileId) == 'NaN' || Number(nTab) == 'NaN')
                        return;

                    sAction = "windowOpen";
                    if (action == 'LNKVIEWPJ')
                        sActionUrl = "ePjDisplay.aspx?pj=" + idPj + "&descId=" + nTab + "&fileId=" + fileId + "&pjtype=" + sType + "&dispFrom=" + sDispFrom;
                    else
                        sActionUrl = url;
                    break;

                case "file":
                    while (url.toLowerCase().indexOf(String.fromCharCode(92)) != -1)
                        url = url.replace(String.fromCharCode(92), '/');

                    if (url.toLowerCase().indexOf("file") != 0)
                        url = "file:///" + url;
                    sAction = "windowOpen";
                    sActionUrl = url;
                    break;

                case "mailto":                 //Mail: ok
                    // todo : remplacer mailto par l'ouverture d'une fiche de type email
                    sAction = "replaceUrl";
                    sActionUrl = "mailto:" + url;
                    break;

                case "http":                 /*site web et FTP : ok*/
                case "ftp":
                    /*
                    #40 252 : un lien http commençant par *ftp*:// est considéré comme valide et laissé tel quel.
                    Idem pour un lien ftp commençant par http*://
                    Ceci, pour éviter la construction de liens invalides de type http://ftp://eudonet.com
                    Si aucun préfixe ne convient, on corrige simplement le lien en fonction du protocole sélectionné
                    */

                    if (
                        url.toLowerCase().indexOf("http://") != 0 &&
                        url.toLowerCase().indexOf("https://") != 0 &&
                        url.toLowerCase().indexOf("ftp://") != 0 &&
                        url.toLowerCase().indexOf("ftps://") != 0 &&
                        url.toLowerCase().indexOf("sftp://") != 0
                    )
                        url = sLnkType + "://" + url;


                    sAction = "windowOpen";
                    sActionUrl = url;
                    break;
            }

            // Action à effectuer en fonction des paramètres
            if (bReturnUrl)
                return sActionUrl;
            else {
                switch (sAction) {
                    case "windowOpen":
                        window.open(sActionUrl);
                        break;
                    case "replaceUrl":
                        location.href = sActionUrl;
                        break;
                }
            }
            /*#endregion*/
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
            // #85 993 - A l'ouverture d'une boîte de dialogue, fermeture des MRU
            this.closeMru();

            //ELAIZ - demande 80208 - on empêche une action sur la Raison sociale si l'adresse personnelle est active
            if (this.dataInput.DescId == 301 && this.dataInput.ReadOnly == true) {
                return false;
            }
            // Demande #79 950 - Cas du clic sur le champ 01 de PJ (Annexes) : on redirige vers la fonction ouvrant l'annexe en popup
            if (this.dataInput.DescId == TAB_PJ + 1) {
                let pjInfo = null;
                if (this.$parent) {
                    // Demande #81 453 - DataJson peut être sur le grand-parent (tradition familiale)
                    let dataJson = this.$parent.DataJson || (this.$parent.$parent ? this.$parent.$parent.DataJson : null);
                    if (dataJson && dataJson.Data && dataJson.Data.length > this.propIndexRow)
                        pjInfo = dataJson.Data[this.propIndexRow].PJInfo;
                    this.openPJ(pjInfo, "LNKOPENPJ", false);
                    return;
                }
            }


            let viewElem;
            let target = this.TargetTab;
            let viewObj = nsMain.getViewsContainingTab(target);

            //Objet qui récupère les vues dont l'utilisateur dispose et qui dispose de l'onglet où l'utilisateur souhaite naviguer

            this.modified = false;
            this.oldValue = this.propListe ? this.$refs.relationlist.text : this.$refs.relationfile.text;

            if (evt && evt.target.classList.contains("targetIsTrue") && this.dataInput.Value != "" && this.dataInput.Value != 0) {
                var ntab = this.TargetTab;
                var tabOrder = document.getElementById('eParam').contentWindow.document.getElementById('TabOrder')
                var tab = tabOrder.value.split(';');
                var ntabFound = tab.find(function (element) {
                    return element == ntab;
                });

                if (ntabFound == undefined) {

                    //ElAIZ - On vérifie que l'on a l'onglet recherche dans une autre vue sinon on met une modale d'alerte

                    if (viewObj.length < 1) {
                        this.emitMethod();
                    } else {
                        this.emitViewModale(viewObj, viewElem, target, this.LnkId);
                    }


                    //this.emitMethod()

                }
                //KJE: chargement d'un scénario existant
                else if (ntabFound == 119200){
                    top.addAutomation(null, null, null, this.LnkId)
                }
                else {
                    top.loadFile(ntabFound, this.LnkId, 3)
                }

            } else {
                this.openLnkFileDialog()
            }

        },
        AddFileFromMRU(event) {

            ePopupObject = new ePopup('ePopupObject', 220, 250, 0, 0, document.body, false);
            ePopupObject.hide();
            eCatalogEditorObject = new eFieldEditor('catalogEditor', ePopupObject, 'eCatalogEditorObject', 'eCatalogEditor');
            eLinkCatFileEditorObject = new eFieldEditor('linkCatFileEditor', ePopupObject, 'eLinkCatFileEditorObject', 'eLinkCatFileEditor');
            eCatalogUserEditorObject = new eFieldEditor('catalogUserEditor', ePopupObject, 'eCatalogUserEditorObject', 'eCatalogUserEditor');
            var catalogObject = window["eLinkCatFileEditorObject"];
            var nMode = "2";
            let that = this;
            var callBack = function (oparams, aFields) {
                that.updateMethod(that, oparams.fid, undefined, undefined, that.dataInput);
            };


            //TO-DO ReloadList a enlever apres 
            catalogObject.parentPopup.hide();
            let nTargetTab = this.TargetTab ;
            let strToolTipText = "Ajouter";
            let bAutobuildName = catalogObject.bAutobuildName;
            var sSearch = this.dataInput.DisplayValue;
            let nCallFrom = 0;
            let sOrigFrameId = "";
            let oOptions = {
                currentTab: this.dataInput.DescId,
                lnkid: "200=0;300=0;400=0;0=0"
            }

            if (this.dataInput.ParentLinks.length > 0) {
                oOptions.lnkid = this.dataInput.ParentLinks
                    .map(l => this.getTabDescid(l.DescId) + "=" + l.FileId)
                    .join(';');
            }

            // AssociateFields = les rubriques associées dépendantes du champs de liaison (datafield)
            // sur chacun de ces champ AssociateField représente le paramétrage de la rubrique cible dans la fiche à créer
            if (this.dataInput.AssociateFields.length > 0) {
                oOptions.defaultValues = new Object();
                this.dataInput.AssociateFields.forEach(f => {
                    oOptions.defaultValues[f.AssociateFieldDescId] = f.Value;
                }
                );
            }

            shFileInPopup(nTargetTab, 0, strToolTipText, null, null, 0, ((bAutobuildName) ? "" : sSearch),
                true, callBack, nCallFrom, sOrigFrameId, null, null, oOptions);
        },
        updateDataInput (newValues) {
            this.dataInputValue = newValues;
        }
    },
    props: [
        "dataInput",
        "propHead",
        "propListe",
        "propSignet",
        "propIndexRow",
        "propAssistant",
        "propDetail",
        "propAssistantNbIndex",
        "propDataDetail",
        "propResumeEdit",
        "MainFileId"
    ],
    mixins: [eFileComponentsMixin],
    template: `
<div
    ref="relationContainer"
    class="globalDivComponent"
    id="globalDivComponent"
    v-click-outside-iris="showMru"
>
    <!-- FICHE -->

    <div
        v-if="!propListe"
        v-bind:class="['ellips input-group hover-input']"
        ref="relation"
        @mouseout="showTooltip(false,'relation',icon,IsDisplayReadOnly,dataInput)" 
        @mouseover="showTooltip(true,'relation',icon,IsDisplayReadOnly,dataInput)"
        @click.self="!IsDisplayReadOnly ? LoadMruRelation($event) : ''"
        :title="dataInput.ToolTipText || fieldDisplayValue"
    >

        <!-- Si le champ relation et est modifiable -->

        <div 
            v-if="!(dataInput.ReadOnly)"
            id="drop-recherche"
            class="ellipsDiv input-line fname"           
            :class="classMru" 
            :style="{ color: dataInput.ValueColor}"
            @mouseover.stop=""
        >
            <eMru v-show="showMru"
                :focusSearch="showMru"
                ref="MRU"
                :dataInput.sync="getInput"
                @openSpecificDialog="openLnkFileDialog"
                @closeMru="closeMru"
                @addMethod="AddFileFromMRU"
                @newLabel="updateDataInput"

            />
            <div
                v-show="!showMru"
                :field="'field'+dataInput.DescId" 
                :id="'COL_' + getTab + '_' + dataInput.DescId + '_1_1_0'" 
                type="text" 
                class="ellipsDiv form-control input-line fname  link-container"
            >
                <a
                    v-show="dataInput.Value != ''"
                    ref="relationfile" 
                    @click.stop="goAction($event)" 
                    @mouseover="showVCardOrMiniFile($event, true);icon = true;" 
                    @mouseout="showVCardOrMiniFile($event, false);icon = false;" 
                    :dbv="dataInput.Value" 
                    :lnkid="LnkId"
                    :vcMiniFileTab="TargetTab" 
                    :style="{ color: dataInput.ValueColor}"
                    class="targetIsTrue mru-link" href="#!"
                >
                    {{ fieldDisplayValue }}
                </a>
                <div @click.self="!IsDisplayReadOnly ? LoadMruRelation($event) : ''" class="click-area"></div>
            </div>
        </div>         

        <!-- Si le champ relation n'est pas modifiable -->
        <a
            v-if="IsDisplayReadOnly"
            ref="relationfile"
            v-bind:style="{ color: dataInput.ValueColor}"
            v-on:click="goAction($event)"
            class="targetIsTrue linkHead readOnly"
            v-on:mouseover="showVCardOrMiniFile($event, true);icon = true"
            v-on:mouseout="showVCardOrMiniFile($event, false);icon = false"
            :dbv="dataInput.Value"
            :lnkid="LnkId"
            :vcMiniFileTab="TargetTab"
        >{{ fieldDisplayValue }}</a>
         
        <!-- Icon -->
        <span
            :id="'COL_' + getTab + '_' + this.dataInput.DescId"
            v-on:click="!IsDisplayReadOnly ? LoadMruRelation($event) : ''"
            class="input-group-addon"
        >
            <a
                href="#!"
                class="hover-pen"
            >
                <i
                    :class="[
                        (IsDisplayReadOnly && !icon)?'mdi mdi-lock'
                        :(IsDisplayReadOnly && icon)?'fas fa-link'
                        :(!IsDisplayReadOnly && icon)?'fas fa-link'
                        :'fas fa-pencil-alt'
                    ]"
                ></i>
            </a>
        </span>
       
	    <!-- Message d'erreur si champs obligatoire est vide -->
        <eAlertBox v-if="this.dataInput.Required && this.bEmptyDisplayPopup" >
            <p>{{getRes(2471)}}</p>
        </eAlertBox>

    </div>

    <!-- LISTE -->

    <div
        v-if="propListe"
        ref="relation"
        @mouseout="showTooltip(false,'relation',icon,IsDisplayReadOnly,dataInput)" 
        @mouseover="showTooltip(true,'relation',icon,IsDisplayReadOnly,dataInput)"
        v-bind:class="[
            propListe ? 'listRubriqueRelation' : '', 
            'ellips input-group hover-input',
            blankField, 
            IsDisplayReadOnly ? 'read-only' : ''
        ]" 
    >

        <!-- Si le champ relation est modifiable -->
        <a
            v-if="!IsDisplayReadOnly"
            :field="'field'+dataInput.DescId" 
            v-on:click="goAction($event)"   
            ref="relationlist" 
            v-on:mouseover="showVCardOrMiniFile($event, true);" 
            v-on:mouseout="showVCardOrMiniFile($event, false);" 
            class="targetIsTrue linkHead readOnly" 
            href="#!" 
            v-bind:style="{ color: dataInput.ValueColor}"
        >{{ fieldDisplayValue }}</a>
        

        <a 
            v-if="IsDisplayReadOnly"
            v-on:click="goAction($event)"  
            ref="relationlist" 
            v-bind:style="{ color: dataInput.ValueColor}"  
            class="targetIsTrue linkHead readOnly" 
            v-on:mouseover="showVCardOrMiniFile($event, true);" 
            v-on:mouseout="showVCardOrMiniFile($event, false);" 
            :dbv="dataInput.Value" 
            :lnkid="LnkId" 
            :vcMiniFileTab="TargetTab"
        >{{ fieldDisplayValue }}</a>
        

        <!-- Si le champ relation n'est pas modifiable -->
        
        <!-- Icon -->
        <span 
            v-if="!dataInput.IsMainField" 
            :id="'COL_' + getTab + '_' + this.dataInput.DescId" 
            v-on:click="!IsDisplayReadOnly ? goAction($event) : '' " 
            class="input-group-addon"
        >
            <a 
                href="#!" class="hover-pen"
            >
                <i
                    :class="[
                        (IsDisplayReadOnly && !icon) ? 'mdi mdi-lock' : (IsDisplayReadOnly && icon) ? 'fas fa-link' : (!IsDisplayReadOnly && icon) ? 'fas fa-link' : 'fas fa-pencil-alt'
                    ]"
                />
            </a>
        </span>
       
    </div>

</div>
`
};