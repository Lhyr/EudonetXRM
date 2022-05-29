import EventBus from '../../bus/event-bus.js?ver=803000';
import { eFileMixin } from '../../mixins/eFileMixin.js?ver=803000';
import { eBkmMixin } from '../../mixins/eBkmMixin.js?ver=803000';
import { UpFilDragLeave, UpFilDrop, UpFilDragOver, callCheckPjExists, checkPjExists, SendFile, activateProgressBar, activateProgressBarv2, cancelNewPjName } from '../../methods/tabMethods.js?ver=803000';
import { showTooltipObj } from '../../methods/eComponentsMethods.js?ver=803000';
import { addNewFile, setBkmObserver } from '../../methods/eFileMethods.js?ver=803000';
import { initIndexedDB, manageIndexedDB, setDataDbAsync, getDataDbAsync, mergeDataDbAsync, filterDataDbAsync, countDataDbAsync, firstDataDbAsync, reloadBkmSkeleton, scrollSignet, setScrollY } from "../../methods/eBkmMethods.js?ver=803000";

export default {
    name: "BkmAnnexe",
    methods: {
        addNewFile,
        showTooltipObj,
        UpFilDragOver,
        UpFilDragLeave,
        UpFilDrop,
        callCheckPjExists,
        checkPjExists,
        SendFile,
        cancelNewPjName,
        activateProgressBar,
        activateProgressBarv2,
        setBkmObserver,
        initIndexedDB,
        manageIndexedDB,
        setDataDbAsync,
        getDataDbAsync,
        mergeDataDbAsync,
        filterDataDbAsync,
        countDataDbAsync,
        firstDataDbAsync,
        reloadBkmSkeleton,
        scrollSignet,
        setScrollY,
        cancelExpressFilter(bkm) {
            var updatePref = "tab=" + this.getTab + ";$;bkm=" + bkm.DescId + ";$;filterExpress=$cancelallexpressfilters$";
            this.setUserBkmPref({ updatePref });
            this.reloadBkm(bkm.id);

        },
        stopDrop() {
            this.blocked = false;
            this.draggedOver = false;
        },
        bGetPagination(bValue) {
            this.paginationActivated = bValue
        },
        getLoadingPhase(bValue) {
            this.loadingActivated = bValue
        },
        emitDragOver(ev) {
            this.UpFilDragOver(this.$refs["dropcontainer"], ev)
        },
        emitDrop(obj) {
            this.UpFilDrop(this.$refs["dropcontainer"], obj.ev);
            this.onLoad = obj.loading;
            this.draggedOver = false;
        },
        emitBlocked(bValue) {
            this.blocked = bValue;
            this.draggedOver = bValue;
        },
        /**
         * ELAIZ - Tâche 3114 - Affiche le tooltip. La méthode est appelée par un intersectionObserver qui observe le moment où la div #mainContentWrap scrolle au niveau du signet Annexe. A ce moment donc l'infobulle est appelée car l'utilisateur l'a dans son champs de vision
         * @param {any} visible
         */
        async hoveringElm(visible) {
            if (this.signet.Actions.Add) { // Demande 83733 Pb Affichage Signet Annexes en lecture seule
                this.showTooltipObj({ visible: visible, elem: 'bkmScroll', icon: false, readonly: false, bkmScroll: true, label: this.getRes(812), ctx: this });
                if (visible) {
                    await this.$nextTick();
                    setTimeout(() => {
                        this.showModalTooltip = false;
                    }, 8000)
                } else {
                    this.showModalTooltip = false;
                }
            }
        },
        getJsonFront(obj) {
            this.jsonFront = obj;
            this.addAttachment = this.jsonFront.find(json => json.index == 0).actions.find(x => x);
        }
    },
    data() {
        return {
            falseDid: 10,
            falseInt: 0,
            expressTitle: this.getRes(6369),
            historiqueTitle: this.getRes(2569),
            reloadBkmTitle: this.getRes(8573),
            tabsignet: [],
            tables: [],
            blocked: false,
            draggedOver: false,
            isInArea: false,
            reloadList: false,
            onLoad: false,
            fileLoad: Object,
            progressBarActivated: false,
            paginationActivated: false,
            loadingActivated: false,
            options: {
                upload: true,
                draggedOver: false,
                blocked: false,
                nbRows: 0,
                title:this.getRes(2670)
            },
            showModalTooltip: false,
            optionsModal: Object,
            tooltipShown: false,
            scrollableElem: null,
            jsonFront: Object,
            dbBkm: null,
            bkmLoaded: false,
            pagination: false,
            scrollLeftSignet: 0
        };
    },
    watch: {
        "signet.nbRows": function (newElem, oldElem) {
            this.options.nbRows = newElem;
        }
    },
    computed: {
        /**
         * Renvoie les classes CSS du conteneur du signet et set la position de la scrollBar
         * */
        getBkmCssClass: function () {
            let cssClass = "";
            // TK #5 854 - Au rechargement du signet après MAJ d'automatisme, le nombre de lignes n'est pas toujours disponible (undefined)
            // Il faut donc positionner le scroll à défaut, en attendant de recevoir le nombre de lignes réel, et ne retirer la classe que s'il est réellement
            // égal à zéro (on pourrait - devrait ? - également tester signet.empty ou signet.error)
            if (typeof (this.signet.nbRows) == "undefined" || this.signet.nbRows > 0) {
                cssClass += " horizontal-scroll"
                this.$nextTick(function () {
                    this.setScrollY()
                })
            }
            if (!this.bkmLoaded) {
                cssClass += " bkm-loading"
            }
            return cssClass;
        },

        isPagination() {
            return (this.paginationActivated) ? 'position:absolute;bottom:20px;' : '';
        },
        dropContainer() {
            return (this.paginationActivated) ? 'drag-drop-container pagination-activated' : (this.signet.nbRows < 1) ? 'drag-drop-container empty-list ' : 'drag-drop-container ';
        },
        isDraggedOver() {
            return (this.draggedOver) ? 'drop-area dragged full-width' : 'drop-area full-width';
        },
        /**
         * Classes CSS du container de la zone de drag n drop
         */
        dropContainerCssClass(){
            return {
                'Onload': !this.bkmLoaded,
                'drop-container':this.blocked,
                'hidden-container':this.progressBarActivated,
                'empty-list-container':this.signet.nbRows < 1,
                'box-body':true
            }
        }
    },
    components: {
        eProgressBar: () => import(AddUrlTimeStampJS("../subComponents/eProgressBar.js")),
        eActionSignet: () => import(AddUrlTimeStampJS("./eActionSignet.js")),
        listContent: () => import(AddUrlTimeStampJS("../liste/listContent.js")),
        eDragAndDrop: () => import(AddUrlTimeStampJS("../subComponents/eDragAndDrop.js")),
        tooltipModale: () => import(AddUrlTimeStampJS("../modale/tooltipModale.js")),
        bkmSkeleton: () => import(AddUrlTimeStampJS("../skeletons/bkmSkeleton.js")),
        bkmHeader: () => import(AddUrlTimeStampJS("./bkmHeader.js")),
    },
    props: ['signet'],
    mixins: [eFileMixin,eBkmMixin],
    template: `
<div :scroll="scrollLeftSignet" :ref="signet.id" :id="signet.id" class="panel box box-primary box-inner box-danger-shadow bkm-wrapper">
    <input v-if="signet.RelationFieldDescId" type="hidden" :id="'BkmHead_' + signet.DescId" :spclnk="signet.RelationFieldDescId" />
    <eActionSignet 
        v-show="bkmLoaded" 
        @pinnedBkm="pinBkm($event)"
        @bkmReload="reloadBkmSkeleton" 
        @hoverOnActionBtn="hoveringElm(false)" 
        @JsonFront="getJsonFront"
        @update-historic="SetHistoric(signet)" 
        :prop-action="signet"></eActionSignet>
    <div @mouseup.self='scrollSignet' :class="getBkmCssClass" ref="bkmScroll" class="bkm-container bkm-attachement">
            <tooltipModale
            v-if="getTooltipObj.visible && getTooltipObj.elem == 'bkmScroll'" 
            @closeTooltipModale="setTooltipObj({visible:false})" 
            :prop-options-modal="getTooltipObj"></tooltipModale>
            <bkmHeader 
                ref="header"
                @update:bPinned="pinBkm($event)" 
                @reloadBkm="reloadBkm($event)" 
                @drop.prevent.self="stopDrop()"  
                @SetHistoric="SetHistoric($event.nBkmTab)" 
                :oTblItem="getBkmHeaderContent" 
                :signet="signet"
                :bPinned.sync="pinnedBkmVal"/>
        <template v-if="false">
            <eProgressBar v-if="onLoad" @callBackfinishLoad="onLoad = false; blocked = false" ref="progressBar" :file="fileLoad"></eProgressBar>
        </template>
        <div v-else :class="{'eProgress':progressBarActivated}" ref="ProgressSpacecraft"></div>
        <div @mouseover.stop ref="dropcontainer" :class="dropContainerCssClass">
            <template v-if="!signet.preventLoad && !(signet.error && signet.empty)">
                <listContent v-show="bkmLoaded"
                v-if="!blocked && !onLoad"
                @listDragDover="emitBlocked(false)" 
                @bkmReload="reloadBkmSkeleton" 
                @loading="getLoadingPhase" 
                @pagination="bGetPagination"  
                :prop-signet="signet" 
                :blocked="blocked"></listContent>
                <template v-if="signet.Actions?.Add">
                    <eDragAndDrop 
                    v-show="bkmLoaded && loadingActivated && (!onLoad || blocked)"
                    @mouseoutOnDropArea="hoveringElm(false)" 
                    @hoverOnDropArea="hoveringElm(true)" 
                    @clickOnDropArea="addNewFile(addAttachment)" 
                    @leavingDrag="emitBlocked" 
                    @upFilDrop="emitDrop" 
                    @UpFilDragOver="emitDragOver" 
                    drag-function="UpFilDragOver" 
                    drop-function="upFilDrop" 
                    :drag-and-drop-expanded.sync="blocked && signet.nbRows > 0" 
                    :drag-and-drop-options="options" 
                    :css-class-drag-area="isDraggedOver" 
                    :css-class-drag-container="dropContainer"></eDragAndDrop>
                </template>
            </template>
            <bkmSkeleton v-show="!bkmLoaded" :bkm-props="{pagination:pagination}" />
        </div>
    </div>
</div>`,
    async mounted() {
        let obs = await this.setBkmObserver(this.$el, this.signet);
        obs.observe(this.$el);

        EventBus.$on('clearExpressFilter', bkm => {
            this.cancelExpressFilter(bkm);
        });
        EventBus.$on('emitnbRows', options => {

            let signet = this.propSignet.signets.find(a => a.DescId == options.signet);
            if (signet)
                Vue.set(signet, 'nbRows', options.pageInfo);

        });

        /*ELAIZ - Permet d'intercepter l'infobulle.Contrairement aux autres cas (champs,mrus etc.), l'infobulle est sur bkmAnnexe pour éviter 
        quelle défile en même temps que le scroll de l'utilisateur, l'idée étant qu'elle reste fixe 5s pour signifier à l'utilisateur qu'il peut
        effetcuer un drag n drop sur les annexes */
        EventBus.$on('tooltipModalInComponent', (options) => {
            this.showModalTooltip = options.visible;
            this.optionsModal = options;
        });

    },
};