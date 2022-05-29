import EventBus from "../../bus/event-bus.js?ver=803000";
import { eFileMixin } from '../../mixins/eFileMixin.js?ver=803000';
import containerModal from '../modale/containerModal.js?ver=803000';
import { LoadStructBkm, linkToPost, setViewMode } from "../../methods/eFileMethods.js?ver=803000";
import { store } from '../../../Scripts/store/store.js?ver=803000';
import eAxiosHelper from "../../helpers/eAxiosHelper.js?ver=803000";
import { loadPinnedBookmark } from "../../shared/XRMWrapperModules.js?ver=803000";
import { reloadBkm } from "../../methods/eActionPinnedFileMethods.js?ver=803000"
import { BKMVIEWMODE } from "../../methods/Enum.min.js?ver=803000";

export default {
    name: "eActionSignet",
    data() {
        return {
            currentHoldStatus: this.propAction.IsMarkettingStepHold,
            IframeBkmCol: null,
            jsonFront: [
                {
                    "index": 0,
                    "name": this.getRes(18),
                    "idGrp": "add",
                    actions: []
                },
                {
                    "index": 1,
                    "name": this.getRes(397),
                    "idGrp": "filter",
                    actions: []
                },
                {
                    "index": 2,
                    "name": this.getRes(2067),
                    "idGrp": "display",
                    actions: []
                },
                {
                    "index": 3,
                    "name": this.getRes(296),
                    "idGrp": "action",
                    actions: []
                },
                {
                    "index": 4,
                    "name": this.getRes(6606),
                    "idGrp": "analyzes",
                    actions: []
                }
            ]
        };
    },
    props: {
        propAction: {
            type: Object,
            default: {}
        },
        PinnedFlag: {
            type: Boolean,
            default: false
        }
    },
    mixins: [eFileMixin],
    template: `
<div @mouseover.stop="$emit('hoverOnActionBtn')" id="right_global_button" class="right">
    <div v-for="tpAction in jsonFront" 
        :class="['btn-group','btn-' + tpAction.idGrp]" 
        :key="tpAction.id"
        v-on="tpAction.actionMainBtn ? { ...tpAction.actionMainBtn } : {}"
        >
        <div v-if="tpAction.actions.length || tpAction.actionMainBtn" class="btn-group dropdown drop-100">
            <li class="dropdown drop-100">
                <button type="button" v-bind:class="['btn btn-default drop-100']" data-toggle="dropdown" aria-expanded="false">
                    <span class="action-title">{{tpAction.name}}
                        <span v-if="tpAction.actions.length" class="fas fa-chevron-down"></span>
                    </span>
                </button>
                <div :id="'fitres_nav_' + tpAction.name" class="dropdown-content-action" role="menu">
                    <template v-for="lnkAction in tpAction.actions">
                        <a :class="[lnkAction.type == 4 && propAction.HistoricActived ? 'actived' : '']" v-on:click="getAction(lnkAction)" href="#!">
                            <i :class="['marg-right ' + lnkAction.icon]"></i>
                            {{lnkAction.name}}
                        </a>
                    </template>
                </div>
            </li>
        </div>        
    </div>
</div>`,

    methods: {
        LoadStructBkm,
        linkToPost,
        loadPinnedBookmark,
        reloadBkm,
        setViewMode,
        /**
         * On annule l'affichage de la popup rubrique.
         * */
        onSetBkmColAbort() {
            modalBkmCol.hide();
        },
        onSetBkmColOk() {
            var _frm = window.frames[this.IframeBkmCol.iframeId];
            var strBkmCol = _frm.getSelectedDescId();
            var updatePref = "tab=" + this.getTab + ";$;bkmcol=" + strBkmCol + ";$;bkm=" + this.propAction.DescId;
            this.setUserBkmPref({ updatePref });
            modalBkmCol.hide();
            var options = {
                id: this.propAction.id,
                signet: this.propAction.DescId,
                nbLine: 9,
                pageNum: 1

            };

            EventBus.$emit('reloadSignet_' + this.propAction.id, options);
        },
        onSetAddNewColOk() {
            var _frm = window.frames[this.IframeBkmCol.iframeId];
            var strBkmCol = _frm.getSelectedDescId();
            var updatePref = "tab=" + this.getTab + ";$;bkmcol=" + strBkmCol + ";$;bkm=" + this.propAction.DescId;
            this.setUserBkmPref({ updatePref });
            eModFile.hide();
            var options = {
                id: this.propAction.id,
                signet: this.propAction.DescId,
                nbLine: 9,
                pageNum: 1

            };
            EventBus.$emit('reloadSignet_' + this.propAction.id, options);
        },
        setBkmCol(nBkmTab) {
            modalBkmCol = new eModalDialog(this.getRes(96), 0, "eFieldsSelect.aspx", 850, 550);
            modalBkmCol.ErrorCallBack = function () { console.log("Error") }

            modalBkmCol.addParam("tab", nBkmTab.DescIdSignet, "post");
            modalBkmCol.addParam("parenttab", this.getTab, "post");
            modalBkmCol.addParam("action", "initbkm", "post");

            modalBkmCol.bBtnAdvanced = true;
            modalBkmCol.show();
            modalBkmCol.addButton(this.getRes(29), () => this.onSetBkmColAbort(), "button-gray", nBkmTab);
            modalBkmCol.addButton(this.getRes(28), () => this.onSetBkmColOk(), "button-green", nBkmTab);
            this.IframeBkmCol = modalBkmCol

        },
        addNewFile(nBkmTab) {
            //Regression #83 768 :taille des popup disproportionnée depuis les signets
            //var percent = 90;
            //var size = getWindowSize();
            //size.h = size.h * percent / 100;
            //size.w = size.w * percent / 100;
            var oTabWH = getWindowWH(top);
            var maxWidth = 1000; //Taille max à l'écran (largeur)
            var maxHeight = 550; //Taille max à l'écran (hauteur)
            var width = oTabWH[0];
            var height = oTabWH[1];
            if (width > maxWidth)   //si largeur de l'écran plus grande que largeur max alors il faut utiliser la largeur max
                width = maxWidth;
            else
                width = width - 10;   //marge de "sécurité"

            if (height > maxHeight)   //si hauteur de l'écran plus grande que largeur max alors il faut utiliser la largeur max
                height = maxHeight;
            else
                height = height - 10;   //marge de "sécurité"
            let descIdSignet = nBkmTab.DescIdSignet;

            var afterValidate = function () {
                var options = {
                    id: nBkmTab.id,
                    signet: descIdSignet,
                    nbLine: 9,
                    pageNum: 1
                };
                EventBus.$emit('reloadSignet_' + nBkmTab.id, options);
            };

            switch (this.$options.propsData.propAction.TableType) {
                case EDNTYPE_MAIL:
                    shFileInPopup(descIdSignet, 0, nBkmTab.name, width, height, 1, null, false, afterValidate, CallFromSendMail);
                    break;
                case EDNTYPE_PJ:
                    setPreventLoadBkmList(true);
                    showAddPJ(null, () => {
                        afterValidate();
                        setPreventLoadBkmList(false);
                    },{ width:650 , height:550 });
                    break;
                case EDNTYPE_PLANNING:
                    showTplPlanning(descIdSignet, 0, null, this.getRes(151));
                    break;
                default:
                    let viewObj;
                    if (descIdSignet == eTools.DescIdEudoModel.TBL_Adress
                        && this.getTab == eTools.DescIdEudoModel.TBL_PM) {
                        var modFinder = openLnkFileDialog(1, eTools.DescIdEudoModel.TBL_PP, true, 4); //4 (CallFromBkm) => indique qu'on est sur de l'ajout depuis un signet
                        if (typeof (specialAction) != "undefined") {
                            modFinder.specialAction = function () {
                                top.setWait(false)
                                afterValidate()
                                modFinder.hide();
                            }
                        }
                        //shFileInPopup(200, 0, nBkmTab.name, size.w, size.h, false, null, true, afterValidate, CallFromBkm);
                    }
                    else if (descIdSignet == eTools.DescIdEudoModel.TBL_PM || descIdSignet == eTools.DescIdEudoModel.TBL_PP) {
                        var modFinder = openLnkFileDialog(1, descIdSignet, true, 4); //4 (CallFromBkm) => indique qu'on est sur de l'ajout depuis un signet
                        if (typeof (specialAction) != "undefined") {
                            modFinder.specialAction = function () {
                                top.setWait(false)
                                afterValidate()
                                modFinder.hide();
                            }
                        }
                    }
                    else {
                        let oCtxInfos = {
                            values: [
                                { descid: this.getTab, value: this.getFileId, displayvalue: this.getFileValue, spclnk: this.propAction.RelationFieldDescId },
                            ]
                        };

                        if (this.getEvtid?.descid != this.getTab && this.getEvtid?.descid > 0)
                            oCtxInfos.values.push(this.getEvtid);

                        if (this.getTab != 200)
                            oCtxInfos.values.push(this.getPPid);

                        if (this.getTab != 300)
                            oCtxInfos.values.push(this.getPMid);

                        shFileInPopup(descIdSignet, 0, nBkmTab.name, width, height, false, null, true, afterValidate, CallFromBkm, undefined, oCtxInfos);
                    }
                    break;
            }

            if (eModFile)
                eModFile.ErrorCallBack = function () { console.log("Error survenue lors de la creation d'une nouvelle fiche dans le signet") }

        },
        //nouveau mode fiche téléguidé
        addNewPurpleFile(nBkmTab) {
            let descIdSignet = nBkmTab.DescIdSignet;

            let oCtxInfos = {
                values: [
                    { descid: this.getTab, Value: this.getFileId, DisplayValue: this.getFileValue, spclnk: this.propAction.RelationFieldDescId },
                ]
            };

            if (this.getEvtid?.descid != this.getTab && this.getEvtid?.descid > 0)
                oCtxInfos.values.push(this.getEvtid);

            if (this.getTab != 200)
                oCtxInfos.values.push(this.getPPid);

            if (this.getTab != 300)
                oCtxInfos.values.push(this.getPMid);

            openPurpleFile(descIdSignet, 0, "", CallFromBkmToPurple, oCtxInfos);
        },
        importFile(nBkmTab) {
            var parentTab = this.getTab;
            var paranteFileId = this.getFileId;
            var importTab = nBkmTab.DescIdSignet;
            oImportWizard.ShowBkmWizard(parentTab, paranteFileId, importTab);
            oImportWizard.ErrorCallBack = function () { console.log("Error survenue lors de l'import depuis le signet") }
        },
        importTargetFile(nBkmTab) {
            //BSE: Bug #79 427: Ouvrir la fenètre d'import signet ++
            let parentTab = this.getTab;
            let paranteFileId = this.getFileId;
            let importTab = nBkmTab;

  
            importTargets(parentTab, paranteFileId, nBkmTab.DescIdSignet, function () {
                let options = {
                    id: importTab.id,
                    signet: importTab.DescIdSignet,
                    nbLine: 9,
                    pageNum: 1
                };
                EventBus.$emit('reloadSignet_' + importTab.id, options);
            });
        },

        HoldMarkettingStep(nBkmTab) {


            let nParentTabId = this.getTab;
            let nParentFileId = this.getFileId;
            let context = this;
            let opt = {
                headtitle: context.getRes(645),
                maintitle: context.currentHoldStatus ? context.getRes(2763, "alert reprise").replace("'<FILENAME>'", "") : context.getRes(2764, "alerte mise en pause").replace("'<FILENAME>'", ""),
                actionOk: async function (target) {
                    //call wcf                   
                    let helper = new eAxiosHelper(context.getUrl + "/mgr/eOnBreakEventStepMgr.ashx");
                    let responseHold = await helper.PostAsync({
                        Status: context.currentHoldStatus ? 0 : 1,
                        ParentTabId: nParentTabId,
                        ParentFileId: nParentFileId,
                    });

                    //affiche message résultat
                    if (responseHold.Success) {
                        context.AlertModal({ 
                          headtitle: context.getRes(2712), 
                          maintitle: !context.currentHoldStatus ? context.getRes(2716, "Les étapes ont été mis en pause") : context.getRes(2765, "Réactivation effectuée") 
                        });
                    }
                    else {
                        context.AlertModal({ 
                          headtitle: context.getRes(2712), 
                          maintitle: responseHold.ErrorMsg 
                        })
                    };

                    // Maj le status - idéalement, il faudrait le faire remonter sur la pile de composant. Pour l'instant, on le stock dans un data local
                    context.currentHoldStatus = (responseHold.Status == 1);

                    //reload signet
                    context.reloadBkm(nBkmTab.DescIdSignet, context.propAction.id);

                    //Modifie le libellé de l'action - le menu n'est pas basé directement sur un data/props ou via computed, on doit le manipuler directement
                    let entryAction = context.jsonFront.find(elem => elem.idGrp === "pause");

                    if (entryAction && entryAction.actionMainBtn) {
                        entryAction.name = !context.currentHoldStatus ? context.getRes(2689, "mettre en pause") : context.getRes(2690, "reprendre");
                        let actionHolld, prop = "type";

                        if (entryAction.actionMainBtn.hasOwnProperty(prop))
                            if (entryAction.actionMainBtn[prop] === 18)
                                actionHolld = entryAction.actionMainBtn;

                        if (actionHolld) {
                            //changement du libellé
                            actionHolld.name = !context.currentHoldStatus ? context.getRes(2689, "mettre en pause") : context.getRes(2690, "reprendre");
                            //context.jsonFront.$set(context.jsonFront.findIndex( elem => elem.idGrp === "action" ), entryAction)
                        }
                    }
                }
            }

            this.ConfirmModal(opt);
        },
        exportFile(nBkmTab) {
            var importTab = nBkmTab.DescIdSignet;
            reportList(2, importTab); ////nTabBkm =2 : si 0 > l'export est lancé depuis le tabid du signet indiqué
            oModalReportList.ErrorCallBack = function () { console.log("Erreur survenue lors de l'export depuis le signet") }
        },
        publiPostage(nBkmTab) {
            var importTab = nBkmTab.DescIdSignet;
            reportList(3, importTab); ////nTabBkm =3 : si 0 > l'export est lancé depuis le tabid du signet indiqué
            oModalReportList.ErrorCallBack = function () { console.log("Erreur survenue lors de publipostage depuis le signet") }
        },
        AddSmsMailing(nBkmTab) {
            var nCalledTab = nBkmTab.DescIdSignet;
            AddSmsMailing(nCalledTab, 3);
            modalWizard.ErrorCallBack = function () { console.log("Erreur survenue lors de creation de sms depuis le signet") }
        },
        AddMailing(nBkmTab) {
            var tab = nBkmTab.DescIdSignet;
            AddMailing(tab, 1);
            modalWizard.ErrorCallBack = function () { console.log("Erreur survenue lors de creation de mail depuis le signet") }
        },
        AddFromFilter(nBkmTab) {
            var tab = nBkmTab.DescIdSignet;
            var sLibelle = this.propAction.Label;
            let descIdSignet = nBkmTab.DescIdSignet;
            ActionFromFilter(tab, sLibelle, 0);
            modalWizard.ErrorCallBack = function () { console.log("Erreur survenue lors de l'ajout de filtre depuis le signet") }
        },
        DeleteFromFilter(nBkmTab) {
            var tab = nBkmTab.DescIdSignet;
            var sLibelle = this.propAction.Label;
            ActionFromFilter(tab, sLibelle, 1);
            modalWizard.ErrorCallBack = function () { console.log("Erreur survenue lors de la suppression du filtre depuis le signet") }
        },
        ShowFormularList(nBkmTab, formularType) {
            var tabBKM = nBkmTab.DescIdSignet;
            var parentFileId = this.getFileId;
            let fileRoot = this.getFileRoot;
            fileRoot.bDisplayed = false;
            ShowFormularList(tabBKM, parentFileId, 0, formularType,fileRoot);
            oModalFormularList.ErrorCallBack = function () { console.log("Erreur survenue lors de l'ouverture du formulaire " + formularType == 0 ? "" : "avancé" + " depuis le signet") }
        },
        SetHistoric(nBkmTab) {
            this.$emit('update-historic', nBkmTab);
        },
        MarketingAutomation(nBkmTab) {
            var tabBKM = nBkmTab.DescIdSignet;
            var parentFileId = this.getFileId;
            addAutomation(tabBKM, this.getTab, parentFileId);
            modalWizard.ErrorCallBack = function () { console.log("Erreur survenue lors de creation de Marketing Automation depuis le signet") }
        },
        async PinBookmark(nBkmTab) {
            // wait the responce from back
            let pinnedBkm = true;
            this.$emit('pinnedBkm', {
                val: pinnedBkm,
                type:'pinned-bkm'
            });
        },
        /**
         * On switch du mode liste au mode fiche et vice et versa. Et vice et versaaaaa!
         * @param {any} nBkmTab
         */
        SwitchViewMode: async function (nBkmTab) {
            let vwMode = (!this.propAction.ViewMode || this.propAction.ViewMode == BKMVIEWMODE.LIST) ? BKMVIEWMODE.FILE : BKMVIEWMODE.LIST;

            try {
                await this.setViewMode(vwMode, nBkmTab?.DescIdSignet);

                let options = {
                    reloadSignet: true,
                    reloadHead: false,
                    reloadAssistant: false,
                    reloadAll: false
                }

                await this.LoadStructBkm(true);
                EventBus.$emit('emitLoadAll', options);
            } catch (e) {
                console.log(e);
            }

        },
        getAction: function (tpAction) {

            switch (tpAction.type) {
                case 1: return this.addNewFile(tpAction); // Add
                case 2: return this.AddFromFilter(tpAction); // AddFromFilter
                case 3: return this.DeleteFromFilter(tpAction); // DeleteFromFilter
                case 4: return this.SetHistoric(tpAction.DescIdSignet); // Historic
                case 5: return this.setBkmCol(tpAction); // Choix des rubrique
                case 6: return console.log(tpAction); // Print
                case 7: return this.AddMailing(tpAction); // Mailing
                case 8: return this.AddSmsMailing(tpAction); // SMS
                case 9: return this.publiPostage(tpAction); // publiPostage
                case 10: return this.importFile(tpAction); // Import
                case 11: return this.importTargetFile(tpAction); // ImportTarget
                case 12: return this.ShowFormularList(tpAction, 0); // Formular
                case 13: return this.exportFile(tpAction); // Export
                case 14: return console.log(tpAction); // Chart
                case 15: return this.reloadBkm(tpAction.DescIdSignet, this.propAction.id); // Actualiser
                //tâche #3 095 : bouton Formulaire Avancé
                case 16: return this.ShowFormularList(tpAction, 1); // Formulaire Avancé
                case 17: return this.SwitchViewMode(tpAction);
                case 18: return this.HoldMarkettingStep(tpAction);
                case 19: return this.addNewPurpleFile(tpAction); //nouveau mode fiche téléguidé
                case 20: return this.MarketingAutomation(tpAction); // Marketing Automation
                case 21: return this.PinBookmark(tpAction); // Epingler un signet
            }

        },
        rewriteJson: function () {
            for (var i = 0; i < this.jsonFront.length; i++) {
                if (this.jsonFront[i].idGrp == 'add') {
                    if (this.propAction?.Actions?.Add) {
                        this.jsonFront[i].actions.push({
                            "DescIdSignet": this.propAction.DescId,
                            "type": 1,
                            "name": this.getRes(31),
                            "icon": "fa fa-plus-square",
                            "id":this.propAction?.id
                        })
                    }
                    if (this.propAction?.Actions?.AddPurpleFile) {
                        this.jsonFront[i].actions.push({
                            "DescIdSignet": this.propAction.DescId,
                            "type": 19,
                            "name": this.getRes(3005), //Assistant création
                            "icon": "fa fa-plus-square"
                        })
                    }
                    if (this.propAction?.Actions?.AddFromFilter) {
                        this.jsonFront[i].actions.push({
                            "DescIdSignet": this.propAction.DescId,
                            "type": 2,
                            "name": this.getRes(428),
                            "icon": "far fa-plus-square"
                        })
                    }
                    //BSE: BUG #79 426 Déplacer l'import dans Ajouter
                    if (this.propAction?.Actions?.Import && this.propAction?.Actions?.ImportTarget) {
                        this.jsonFront[i].actions.push({
                            "DescIdSignet": this.propAction.DescId,
                            "type": 11,
                            "name": this.getRes(6340),
                            "icon": "fas fa-download"
                        })
                    }
                    if (this.propAction?.Actions?.EventTargetForScenario) {
                        this.propAction?.ListTargetScenario.forEach((item) => {
                            this.jsonFront[i].actions.push({
                                "DescIdSignet": item.Key,
                                "type": 20,
                                "name": this.getRes(8954) + item.Value,
                                "icon": "far fa-plus-square"
                            })
                        })

                    }
                }
                else if (this.jsonFront[i]?.idGrp == 'filter') {


                }
                else if (this.jsonFront[i]?.idGrp == 'display') {
                    this.jsonFront[i].actions.push({
                        "DescIdSignet": this.propAction.DescId,
                        "type": 5,
                        "name": this.getRes(96),
                        "icon": "fa fa-table"
                    })

                    // action pin bookmark
                    if (this.propAction?.type != "pinned-bkm" && !this.PinnedFlag) {
                        this.jsonFront[i].actions.push({
                            "DescIdSignet": this.propAction.DescId,
                            "type": 21,
                            "name": this.getRes(8922),
                            "icon": "fas fa-thumbtack"
                        })
                    }
                    this.jsonFront[i].actions.push({
                        "DescIdSignet": this.propAction.DescId,
                        "type": 15,
                        "name": this.getRes(1127),
                        "icon": "fa icon-refresh"
                    })
                    if (this.propAction?.Actions?.Historic) {
                        this.jsonFront[i].actions.push({
                            "DescIdSignet": this.propAction.DescId,
                            "type": 4,
                            "name": this.propAction.HistoricActived ? this.getRes(6216) : this.getRes(6217),
                            "icon": "fa fa-history "
                        })
                    }

                    /* Invalid action, we can not switch from list to file without fileId
                    if (this.propAction?.Actions?.SwitchViewFile && this.propAction?.type == "pinned-bkm") {
                        let vwMode = (!this.propAction.ViewMode || this.propAction.ViewMode == 0)
                            ? [this.getRes(6283), "fa-sticky-note"]
                            : [this.getRes(23), "fa-list"];

                        //switch mode fiche mode liste
                        this.jsonFront[i].actions.push({
                            "DescIdSignet": this.propAction.DescId,
                            "type": 17,
                            "name": vwMode[0],
                            "icon": "fa " + vwMode[1]
                        })
                    }
                    */
                }
                else if (this.jsonFront[i]?.idGrp == 'action') {

                    if (this.propAction?.Actions?.Export) {
                        this.jsonFront[i].actions.push({
                            "DescIdSignet": this.propAction.DescId,
                            "type": 13,
                            "name": this.getRes(7538),
                            "icon": "fas fa-upload"
                        })
                    }
                    if (this.propAction?.Actions?.Mailing) {
                        this.jsonFront[i].actions.push({
                            "DescIdSignet": this.propAction.DescId,
                            "type": 7,
                            "name": this.getRes(14),
                            "icon": "fa fa-envelope"
                        })
                    }
                    if (this.propAction?.Actions?.SMS) {
                        this.jsonFront[i].actions.push({
                            "DescIdSignet": this.propAction.DescId,
                            "type": 8,
                            "name": this.getRes(655),
                            "icon": "fas fa-sms"
                        })
                    }
                    if (this.propAction?.Actions?.MarketingAutomation) {
                        this.jsonFront[i].actions.push({
                            "DescIdSignet": this.propAction.DescId,
                            "type": 20,
                            "name": this.getRes(8772),
                            "icon": "fas fa-robot"
                        })
                    }
                    if (this.propAction?.Actions?.Merge) {
                        this.jsonFront[i].actions.push({
                            "DescIdSignet": this.propAction.DescId,
                            "type": 9,
                            "name": this.getRes(7540),
                            "icon": "far fa-file-word"
                        })
                    }
                    if (this.propAction?.Actions?.Formular) {
                        this.jsonFront[i].actions.push({
                            "DescIdSignet": this.propAction.DescId,
                            "type": 12,
                            "name": this.getRes(6610),
                            "icon": "fab fa-wpforms"
                        })
                    }
                    //tâche #3 095 : bouton Formulaire Avancé
                    if (this.propAction?.Actions?.AdvFormular) {
                        this.jsonFront[i].actions.push({
                            "DescIdSignet": this.propAction.DescId,
                            "type": 16,
                            "name": this.getRes(2660),
                            "icon": "nmf icon-list-alt"
                        })
                    }
                    if (this.propAction?.Actions?.DeleteFromFilter) {
                        this.jsonFront[i].actions.push({
                            "DescIdSignet": this.propAction.DescId,
                            "type": 3,
                            "name": this.getRes(529),
                            "icon": "far fa-minus-square"
                        })
                    }

                }
                else if (this.jsonFront[i]?.idGrp == 'analyzes') {
                    if (this.propAction?.Actions?.Chart) {
                        this.jsonFront[i].actions.push({
                            "DescIdSignet": this.propAction.DescId,
                            "type": 14,
                            "name": this.getRes(1005),
                            "icon": "fas fa-chart-pie"
                        })
                    }
                }
            }

            if (this.propAction?.Actions?.HoldMarkettingStep) {
                this.jsonFront.unshift(
                    {
                        "index": 5,
                        "name": !this.currentHoldStatus ? this.getRes(2689, "mettre en pause") : this.getRes(2690, "reprendre"),
                        "idGrp": "pause",
                        actions: [],
                        actionMainBtn:
                        {
                            click: () => this.HoldMarkettingStep(this.propAction),
                            type: 18

                        }

                    }
                )
            }

        },
        //dynamicFormatChamps,
        /**
         * Ouvre une popup de confirmation
         * @param {any} opt option de la popup
         */
        ConfirmModal(opt = {
            headtitle: '',
            maintitle: ''
        }) {
            let myModalContext = this;

            let closeModal = function () {
                if (!myModalContext.instance)
                    return;
                myModalContext.instance.$destroy();
                [...myModalContext.$root.$children.find(x => x.$options.name == "App").$el.children].find(x => x.className == "containerModal") ? myModalContext.$root.$children.find(x => x.$options.name == "App").$el.removeChild(myModalContext.instance.$el) : '';
            }

            let options = {
                id: "ActionModal",
                class: "modal-motherofAll",
                style: { width: document.querySelector('#MainWrapper') ? document.querySelector('#MainWrapper').offsetWidth + "px" : "" },
                actions: [],
                header: {

                    text: opt.headtitle,
                    class: "modal-header-motherofAll modal-header-motherofAll-Max relation-modal",
                    btn: [
                        {
                            name: 'close',
                            class: "icon-edn-cross titleButtonsAlignement",
                            action: closeModal
                        }
                    ]
                },

                main: {
                    class: "detailContent modal-content-motherofAll modal-content-motherofAll-Max relation-modal",
                    componentsClass: "grid-container form-group relation-container",
                    title: opt.maintitle,
                },

                //footer - btn
                footer: {
                    class: "modal-footer-motherofAll modal-footer-motherofAll-Max relation-modal",
                    btn: [
                        //confirmer
                        {
                            title: this.getRes(28, "Valider"), class: "btncancel eudo-button btn btn-default",
                            action: function () {
                                closeModal();
                                if (typeof opt.actionOk === "function")
                                    opt.actionOk(myModalContext)
                            }
                        },
                        //annuller
                        { title: this.getRes(29, "Annuler"), class: "btncancel eudo-button btn btn-default", action: closeModal },
                    ]
                },
            };

            this.modalOptions = options;
            var ComponentClass = Vue.extend(containerModal);

            this.instance = new ComponentClass({
                propsData: { 'propOptionsModal': options },
                store: store
            })
            this.instance.$mount();
            this.$root.$children.find(x => x.$options.name == "App").$el.appendChild(this.instance.$el);
        },
        /**
        * Ouvre une popup d'alerte
        * @param {any} opt option de la popup
        */
        AlertModal(opt = { headtitle: '', maintitle: '' }) {
            let myModalContext = this;


            let closeAlertModal = function () {
                if (!myModalContext.instanceAlert)
                    return;

                myModalContext.instanceAlert.$destroy();
                [...myModalContext.$root.$children.find(x => x.$options.name == "App").$el.children].find(x => x.className == "containerModal") ? myModalContext.$root.$children.find(x => x.$options.name == "App").$el.removeChild(myModalContext.instanceAlert.$el) : '';
            }

            let options = {
                id: "ActionModal",
                class: "modal-motherofAll",
                style: { width: document.querySelector('#MainWrapper') ? document.querySelector('#MainWrapper').offsetWidth + "px" : "" },
                actions: [],
                header: {
                    text: opt.headtitle,
                    class: "modal-header-motherofAll modal-header-motherofAll-Max relation-modal",
                },

                main: {
                    class: "detailContent modal-content-motherofAll modal-content-motherofAll-Max relation-modal",
                    componentsClass: "grid-container form-group relation-container",
                    title: opt.maintitle,
                },

                //footer - btn femer
                footer: {
                    class: "modal-footer-motherofAll modal-footer-motherofAll-Max relation-modal",
                    btn: [{ title: this.getRes(30, "Fermer"), class: "btncancel eudo-button btn btn-default", action: closeAlertModal }]
                },
            };

            this.modalOptions = options;

            var ComponentClass = Vue.extend(containerModal);

            this.instanceAlert = new ComponentClass({
                propsData: { 'propOptionsModal': options },
                store: store
            })

            this.instanceAlert.$mount();
            this.$root.$children.find(x => x.$options.name == "App").$el.appendChild(this.instanceAlert.$el);
        },

    },
    computed: {},
    watch: {
        "propAction.HistoricActived": function (oldVal, newVal) {

            let lstActions = this.jsonFront.find(b => b.index === 2);

            if (!lstActions)
                return false;

            lstActions = lstActions.actions.find(f => f.type === 4);

            if (!lstActions)
                return false;

            this.jsonFront.find(b => b.index === 2).actions.find(f => f.type === 4).name = !newVal ? this.getRes(6216) : this.getRes(6217);
        }
    },
    mounted() {
        this.rewriteJson();
        this.$emit('JsonFront', this.jsonFront);
    }
};