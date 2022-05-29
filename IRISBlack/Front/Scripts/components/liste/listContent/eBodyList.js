import { eListComponentsMixin } from '../../../mixins/eListComponentsMixin.js?ver=803000';
import { dynamicFormatChamps } from '../../../../index.js?ver=803000';
import { showTooltipObj } from '../../../methods/eComponentsMethods.js?ver=803000'
import { AdrField, EdnType, TableType, arrTabForbiddenPinFile, arrDescIdForbiddenPinFile, BKMVIEWMODE } from '../../../methods/Enum.min.js?ver=803000';
import EventBus from "../../../../Scripts/bus/event-bus.js?ver=803000";
import { getTabDescid } from "../../../methods/eMainMethods.js?ver=803000";
import { openPlainFile } from "../../../methods/eActionPinnedFileMethods.js?ver=803000";
import { linkToCall, setViewMode, linkToPost } from "../../../methods/eFileMethods.js?ver=803000";
import { getIrisPurpleActived, openFileGuidedTyping, loadFileDetail, loadPinnedBookmark, loadFileLayout } from '../../../shared/XRMWrapperModules.js?ver=803000'

export default {
    name: "eBodyList",
    data() {
        return {
            constPage: 0,
            numberRows: 10,
            screenDimension: 0.8,
            maxNumRows: 50,
            canMerge: true, // ELAIZ - rajout d'une variable en attendant d'avoir la réponse du back si on a les droits de fusion
            objList: [
                { name: this.getRes(3083), action: this.openFileIncrust, prependIcon: 'fas fa-external-link-square-alt' },
                { name: this.getRes(3070), action: this.openFile, prependIcon: 'fas fa-external-link-alt' },
                { name: this.getRes(3071), action: this.openPinned, prependIcon: 'fas fa-thumbtack' },
                { name: this.getRes(3069), action: this.openFileAssistant, prependIcon: 'fas fa-hat-wizard' },
                { name: this.getRes(8953), action: this.openScenario, prependIcon: 'fas fa-pencil-alt' } /** for the Scenario bookmark */
            ],
            cssClass: { listCtr: '', listElm: '', listVal: '' }
        };
    },
    components: {
        ePopOver: () => import(AddUrlTimeStampJS("../../../components/subComponents/ePopOver.js")),
        eList: () => import(AddUrlTimeStampJS("../../subComponents/eList.js"))
    },
    props: {
        pagingInfo: Object,
        tables: Object,
        propSignet: Object,
        loadNewLine: Boolean,
        nbLineCall: Number,
        forceRefreshTabsBar: Number
    },
    mixins: [eListComponentsMixin],
    methods: {
        getTabDescid,
        getIrisPurpleActived,
        openFileGuidedTyping,
        showTooltipObj,
        dynamicFormatChamps,
        loadFileDetail,
        loadFileLayout,
        linkToCall,
        setViewMode,
        loadPinnedBookmark,
        linkToPost,
        openPlainFile,
        TargetTab: function (dataInput) {
            if (dataInput.TargetTab > 0)
                return dataInput.TargetTab;

            else if (dataInput.IsMainField)
                return getTabDescid(dataInput.DescId);

            return 0;
        },
        /**
          * Ouvrir la saisie guidée en modification
          * @param {any} index de la ligne
          */
        openGuidFile(index) {

            this.openFileGuidedTyping(this.propSignet.DescId, this.tables.Data[index].MainFileId, "", 2);
        },
        /**
          * 
          * @param {any} event
          * @param {any} index de la ligne
          */
        async openFileInPopup(event, index) {

            this.noReloadForce = true;
            //let target = event.target;
            let tablesLen = this.tables.Data.length;
            let nbofTabActive = Math.ceil((this.pagingInfo.NbTotalRows / this.tables.Data.length));
            let actualPage = this.pagingInfo.Page;
            let pageCall = {}
            pageCall.pageNum = actualPage;
            let newPageCall = {
                pageNum: this.pagingInfo.Page,
                nbLine: this.nbLineCall
            }
            //pageCall.pageNum = actualPage - 1;
            //pageCall.nbLine = this.pagingInfo.NbTotalRows - 8;
            //pageCall.nbLine = 10;

            var height = parseInt(window.getComputedStyle(document.body, null).height) * this.screenDimension + "px";
            var width = parseInt(window.getComputedStyle(document.body, null).width) * this.screenDimension + "px"
            //setFldEditor(this.$refs.iconpopup[index], this.$refs.iconpopup[index], "LNKOPENPOPUP", "LCLICK", event);
            //shFileInPopup("2100", "3829", "Destinataires", 1530, 757, 0, "", null, () => { this.callSignet(undefined) }, 0, null, null, null, undefined);

            /*ELAIZ - demande 80083 - on envoie à top le contexte de vue pour pouvoir traiter l'exception dans eEngine lorsque
            l'on modifie des valeur depuis la modale des signets */
            this.setIrisUpdateValFromBkmModal(this);


            if (this.propSignet.TableType == EdnType.FILE_PLANNING) {
                let plan = showTplPlanning(this.propSignet.DescId, this.tables.Data[index].MainFileId, null, this.getRes(151));

                return;
            }

            let nMailStatus = 0;

            if ([EdnType.FILE_MAIL, EdnType.FILE_SMS].some(n => n == this.propSignet.TableType))
                nMailStatus = this.tables.Data[index].MailStatus;


            let nDescId = this.propSignet.ViewMainTab ?? this.propSignet.DescId;
            //let nDescId = this.propSignet.TableType == EdnType.FILE_RELATION ? this.getTab : this.propSignet.DescId;


            shFileInPopup(nDescId, this.tables.Data[index].MainFileId, this.propSignet.Label, width, height, nMailStatus, "", null, () => {
                // permet d'attendre que callsignet ait fin avant de lancer getPage
                new Promise((resolve, reject) => {
                    var options = {
                        id: this.propSignet.id,
                        signet: this.propSignet.DescId,
                        nbLine: this.nbLineCall,
                        pageNum: this.pagingInfo.Page
                    };
                    resolve(EventBus.$emit("reloadSignet_" + this.propSignet.id, options));
                }).then(() => {
                    //Si le nombre de ligne est sup à 10 alors on se base sur l'index de la ligne pour trouver la page sinon on prend la page sur laquelle on est
                    //Cela peut-être utile lorsque l'on modifie une valeur sur la page 1 par ex mais que le scroll nous indique la page 2
                    this.$emit('getPage',
                        (tablesLen > this.numberRows)
                            ? Math.ceil(index / (this.pagingInfo.NbTotalRows / this.pagingInfo.NbPages))
                            : actualPage);

                });
            }, 0, null, null, null, undefined);
        },

        /**
         * Observateur sur la page.
         * previousTop peut servir à savoir si on monte ou on descend, et donc
         * si on scrolle vers le hat ou le bas.
         * */
        setObservePage() {
            const thresholdArray = steps => Array(steps + 1)
                .fill(0)
                .map((_, index) => index / steps || 0)

            let previousTop = 0;
            let previousRatio = 0;

            let observerLoading = new IntersectionObserver(([e]) => {

                const ratio = e.intersectionRatio;
                const boundingRect = e.boundingClientRect;
                const intersectionRect = e.intersectionRect;

                if ((intersectionRect.bottom > (boundingRect.top + boundingRect.height / 2))
                    && e.isIntersecting) {
                    //this.$emit('getMore');
                    observerLoading.unobserve(this.$el);
                }

                previousTop = boundingRect.top;
                previousRatio = ratio;

            }, { root: this.$parent.$parent.$el, threshold: thresholdArray(10) });

            observerLoading.observe(this.$el);
        },
        /**
         * ELAIZ - US 1988 tâche 2894 Méthode qui permet d'appeler la modale de fusion et qui câble 
         * la fonction de fusion à la validation
         * Cette méthode est une copie de la méthode setFldMergeFile qui est présente dans eMain.js
         * @param {any} idRow index la ligne concernée pour avoir accès au MainFileidde la fiche en doublon
         */
        openMergeDialog(idRow) {
            let nParentFileId = this.getFileId;
            let nBkmFileId = this.tables.Data[idRow].MainFileId;

            var eModFileMrg = new eModalDialog(this.getRes(994), 0, "eMergeFiles.aspx", 700, 500);
            eModFileMrg.addParam("tab", this.getTab, "post");
            eModFileMrg.addParam("fromfileid", nParentFileId, "post");
            eModFileMrg.addParam("bkmfileid", nBkmFileId, "post");

            eModFileMrg.ErrorCallBack = function () { eModFileMrg.hide(); };
            eModFileMrg.onIframeLoadComplete = function () { setFldMergeFile_onLoad(eModFileMrg); };
            eModFileMrg.show();

            eModFileMrg.addButton(this.getRes(29), null, "button-gray", null, "cancel"); // Annuler
            eModFileMrg.addButton(this.getRes(28), () => {
                mergeFile(this.getTab, nParentFileId, nBkmFileId, eModFileMrg);
            }, "button-green", null, "ok"); // Valider

            document.getElementById("ImgControlBox_" + eModFileMrg.UID).onclick = function () { eModFileMrg.MaxOrMinModal(); eModFileMrg.getIframe().AdjustScrollDiv(); };

        },
        /**
         * ELAIZ - Méthode intermédiaire pour appeler showTooltip ( en effet elle envoie beaucoup de paramètre ce qui la rend illisible sur le template)
         * J'envoie le contexte du composant avec ctx car il n'est pas possible de binder this à la méthode;
         * @param {any} visible - affiche ou non l'infobulle
         * @param {any} idRow - index de la ligne concernée
         */
        hoveringElm(visible, idRow) {
            showTooltipObj({ visible: visible, elem: 'merginButton', icon: false, readonly: false, merginButton: true, data: this.tables.Data[idRow], label: this.getRes(812), ctx: this, id: idRow });
        },

        /**
         * Ajoute un écouteur qui permet de déclencher le décochage des cases à cocher "Adresse Principale" (412) liées au même fichier principal sur le même
         * signet, lorsqu'on coche une case Adresse Principale, afin de répliquer le comportement effectué côté Back. En effet, un fichier principal
         * (ex : Contacts) ne peut avoir qu'une seule fiche Adresse cochée comme Principale */
        async setRefreshMainAddressListener() {
            EventBus.$on('RefreshMainAddress', (options) => {
                // Récupération des données concernées sur la liste en cours
                let fieldsToUpdate = this.tables.Data
                    .map(x => x.LstDataFields.filter(x => x.DescId == AdrField.PRINCIPALE)) /* on récupère les DataFields dont le DescID est 412 (Adresse Principale) sous forme d'Array */
                    .flat(1) /* on lisse l'Array obtenue de façon à ce que chaque élément ne soit pas un Array[0] mais l'objet directement */
                    .filter(x => x.Value == 1 && x.FileId != options.inputData.FileId); /* puis on récupère uniquement les cases à cocher cochées, SAUF celle que l'on vient de cocher */

                // Puis décochage des cases concernées
                fieldsToUpdate.forEach(fieldToUpdate => fieldToUpdate.Value = 0);
            });
        },
        /** retourne la position de la popover. Les deux derniers sont en top car sinon ils dépassent de ebodylist et sont partiellement masqués */
        getPopOverPos(idx) {
            return idx < this.tables.Data.length - 2 ? 'center' : 'top'
        },
        /** Ouvre la fiche en popup */
        openFileIncrust(idxRow, bodyRow) {
            this.openFileInPopup(undefined, idxRow);
        },
        /** Accède à la fiche */
        openFile(idxRow, bodyRow) {
            let dataInput = bodyRow.LstDataFields.find(a => a.DescId == this.propSignet.DescId + 1);

            if (dataInput) {
                var ntab = this.TargetTab(dataInput);
                this.openPlainFile(ntab, dataInput.FileId);
            }
        },
        /** Ouvre la saisie guidée */
        openFileAssistant(idxRow, bodyRow) {
            this.openGuidFile(idxRow);
        },
        /** Open the existing scenario */
        openScenario(idxRow, bodyRow) {
            addAutomation(null, null, null, bodyRow.MainFileId);
        },
        /**
         * ouvre la fiche épinglée.
         * @param {any} bodyRow
         */
        openPinned: async function (idxRow, bodyRow) {
            let fileId = parseInt(bodyRow.MainFileId)

            // Construit un tableau de position et de fileId en fonction de la page et des lignes afficher
            var aFileId = [];
            this.tables.Data.forEach((row, idx) => {
                aFileId.push({
                    pos: this.pagingInfo.Page != 1 ? ((this.pagingInfo.Page * 10) - 10) + (idx + 1) : idx + 1,
                    fileId: row.MainFileId
                })
            });

            // L'utilisateur doit voir 1 et pas 0, de même que nbRows = 10 et pas 9
            idxRow = this.pagingInfo.Page > 1 ? ((this.pagingInfo.Page * 10) - 10) + (idxRow + 1) : idxRow + 1;

            let oDataDetail = await this.loadFileDetail();
            let oFileLayout = await this.loadFileLayout();

            if (!(bodyRow && this.propSignet?.DescId))
                return;

            await this.setViewMode(BKMVIEWMODE.FILE, this.propSignet.DescId);
            let finalCountDown = this.linkToCall(oDataDetail.url, { ...oDataDetail.params, nTab: parseInt(this.propSignet?.DescId), nFileId: parseInt(bodyRow.MainFileId) })
                .catch(error => top.eAlert(1, this.getRes(412), error.message, error.stack));

            let promFinalLayout = this.linkToCall(oFileLayout.url, { ...oFileLayout.params, nTab: parseInt(this.propSignet?.DescId) })
                .catch(error => top.eAlert(1, this.getRes(412), error.message, error.stack));

            this.$emit("setPinnedBkm", finalCountDown, promFinalLayout, BKMVIEWMODE.FILE, fileId, idxRow, aFileId);
        },
        /** Actions sur la popover des signets */
        setAction(list, idxRow, bodyRow) {
            this.objList.find(lst => lst.name == list.name).action(idxRow, bodyRow)
        },
        setObjListAction(bodyRow) {
            var objAction = [];
            /** for the Scenario bookmark replace the content on the popover*/
            if (this.propSignet.DescId == '119200') {
                objAction.push(
                    {
                        name: this.getRes(8953),
                        action: this.openScenario,
                        prependIcon: 'fas fa-pencil-alt'
                    }
                )
            }
            else {
                objAction.push(
                    {
                        name: this.getRes(3083),
                        action: this.openFileIncrust,
                        prependIcon: 'fas fa-external-link-square-alt'
                    }
                )

                if (!arrTabForbiddenPinFile.includes(this.propSignet.TableType)
                    && !arrDescIdForbiddenPinFile.includes(this.propSignet.DescId)
                    && !this.propSignet?.TableType?.toString().endsWith('87')
                    && !(this.getTabDescid(this.propSignet.DescId) == this.getTab && this.propSignet.DescId != this.getTab)
                    || this.propSignet.DescId == TableType.ADR && this.getTab == TableType.PM

                ) {
                    objAction.push(
                        {
                            name: this.getRes(3071),
                            action: this.openPinned,
                            prependIcon: 'fas fa-thumbtack'
                        }
                    )

                }

                var dataInput = bodyRow.LstDataFields.find(a => a.DescId == this.propSignet.DescId + 1)
                if (dataInput != undefined) {
                    var ntab = this.TargetTab(dataInput);
                    if (ntab > 0) {
                        objAction.push({
                            name: this.getRes(3070),
                            action: this.openFile,
                            prependIcon: 'fas fa-external-link-alt'
                        })
                    }
                }
            }

            return objAction;
        }
    },
    async created() {
        EventBus.$off("RefreshMainAddress");
    },
    computed:{
        /** Classe Css du wrapper de la popover */
        getPropClassPopOver(){
            let browser = new getBrowser();
            return {
                'popover__apple':browser?.isMac || browser?.isIOS
            }
        }
    },
    mounted() {
        this.setObservePage();
        this.setRefreshMainAddressListener();
    },
    template: `
        <tbody  
            :page="tables.Page" 
            :ref="'page_' + tables.Page" 
            id="tbody-infinite-scroll" 
            class="tbody_asso"
        >
            <tr :dataRef="'tr_' + indexRow" v-for="(bodyRow,indexRow) in tables.Data" role="row">
                <td :ename="'HEAD_ICON_COL_' + propSignet.DescId" 
                    :lnkid="bodyRow.MainFileId" efld="1" eaction="LNKOPENPOPUP" :class="getAddPurplFileClass" class="icon-popup-cell">
                    <ePopOver :propClassPopOver="getPropClassPopOver" :elevation="4" :position="getPopOverPos(indexRow)">
                        <template #popTitle>
                            <i :style="{'background':bodyRow.BGColor,'color':bodyRow.Color}" :class="[bodyRow.Icon ? bodyRow.Icon : 'icon-smile-o']"></i>
                        </template>
                        <template #popContent>
                            <eList :idxRow="indexRow" :bodyRow="bodyRow" :cssClass="cssClass" @clickAction='setAction' :prop-list="setObjListAction(bodyRow)" ></eList>
                        </template>
                    </ePopOver>
                </td>
                <td
                    v-if="propSignet.DescId == 2 && canMerge"
                    ref="merginButton"
                >
                    <i
                        @mouseover="hoveringElm(true,indexRow);"
                        @mouseout="hoveringElm(false,indexRow);"
                        @click="openMergeDialog(indexRow)"
                        class="fas fa-times merge-files"
                    ></i>
                </td>
                <td 
                    v-for="(bodyColumn, index) in bodyRow.LstDataFields"
                    :key="index"
                    :tp="bodyColumn.Format"  
                    class="td_3"
                >
                    <div :Did="propSignet.DescId" :FileId="bodyColumn.FileId" :DivDescId="bodyColumn.DescId">
                       <component
                            :key="forceRefreshTabsBar"
                            :prop-index-row="indexRow"
                            :prop-signet="propSignet" 
                            :prop-liste="true" 
                            :data-input="bodyColumn"
                            :MainFileId="bodyRow.MainFileId"
                            :is="dynamicFormatChamps(bodyColumn)"
                        ></component>
                    </div>
                </td>
            </tr>
        </tbody>
`
};