import EventBus from '../../bus/event-bus.js?ver=803000';
import { eFileMixin } from '../../mixins/eFileMixin.js?ver=803000';
import { eBkmMixin } from '../../mixins/eBkmMixin.js?ver=803000';
import { setBkmObserver } from "../../methods/eFileMethods.js?ver=803000";
import { initIndexedDB, manageIndexedDB, setDataDbAsync, getDataDbAsync, mergeDataDbAsync, filterDataDbAsync, countDataDbAsync, firstDataDbAsync, reloadBkmSkeleton, setScrollY, scrollSignet } from "../../methods/eBkmMethods.js?ver=803000";
import { BKMVIEWMODE } from '../../methods/Enum.min.js?ver=803000';

export default {
    name: "BkmUnit",
    methods: {
        reloadBkmSkeleton,
        setBkmObserver,
        initIndexedDB,
        manageIndexedDB,
        setDataDbAsync,
        getDataDbAsync,
        mergeDataDbAsync,
        filterDataDbAsync,
        countDataDbAsync,
        firstDataDbAsync,
        scrollSignet,
        setScrollY,
        cancelExpressFilter(bkm) {
            var updatePref = "tab=" + this.getTab + ";$;bkm=" + bkm.DescId + ";$;filterExpress=$cancelallexpressfilters$";
            this.setUserBkmPref({ updatePref });
            this.reloadBkm(bkm);
        },
        /**
         * ouvre la fiche épinglée.
         * @param {any} finalCountDown
         * @param {any} oFinalLayout
         * @param {any} bkmViewMode
         * @param {any} fileId
         */
        setPinnedBkm: function (finalCountDown, oFinalLayout, bkmViewMode, fileId, idxRow, aFileId) {
            this.pinBkm(true, finalCountDown, oFinalLayout, bkmViewMode, fileId, idxRow, aFileId);
        },

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
                
            //return this.signet.nbRows > 0 ? 'horizontal-scroll' : ''

            return cssClass;
        },
    },
    created(){
        this.pinnedBkmVal = this.signet.IsPinned;
    },
    data() {
        return {
            falseDid: 10,
            falseInt: 0,
            tabsignet: [],
            reloadBkmTitle: this.getRes(8573),
            expressTitle: this.getRes(6369),
            tables: [],
            dbBkm: null,
            bkmLoaded: false,
            pagination: false,
            scrollLeftSignet: 0
        };
    },
    components: {
        eActionSignet: () => import(AddUrlTimeStampJS("./eActionSignet.js")),
        listContent: () => import(AddUrlTimeStampJS("../liste/listContent.js")),
        eEncrustedFile: () => import(AddUrlTimeStampJS("../encrustedFile/eEncrustedFile.js")),
        bkmSkeleton: () => import(AddUrlTimeStampJS("../skeletons/bkmSkeleton.js")),
        bkmHeader: () => import(AddUrlTimeStampJS("./bkmHeader.js")),
    },
    props: {
        signet: {
            type: Object,
            default: {}
        },
        PinnedFlag: {
            type: Boolean,
            default: false
        },
        forceRefreshTabsBar:{
            type:Number,
            default:0
        }
    },
    mixins: [eFileMixin,eBkmMixin],
    template: `
    <div :scroll="scrollLeftSignet" :ref="signet.id" :id="signet.id" class="panel box box-primary box-inner box-danger-shadow bkm-wrapper">
        <input
            v-if="signet.RelationFieldDescId"
            type="hidden"
            :id="'BkmHead_' + signet.DescId"
            :spclnk="signet.RelationFieldDescId"
        />
        <eActionSignet
            v-show="bkmLoaded"
            @pinnedBkm="pinBkm($event)"
            @bkmReload="reloadBkmSkeleton($event)"
            @update-historic="SetHistoric(signet)"
            :prop-action="signet"
            :PinnedFlag="PinnedFlag"
        />
        <div
            ref="bkmScroll"
            @mouseup.self='scrollSignet'
            :class="getBkmCssClass"
        >
            <bkmHeader
                @update:bPinned="pinBkm($event)"
                @reloadBkm="reloadBkm($event)"
                @SetHistoric="SetHistoric($event.nBkmTab)"
                :oTblItem="getBkmHeaderContent"
                :signet="signet"
                :bPinned.sync="pinnedBkmVal"
                :PinnedFlag="PinnedFlag"
            />
            <div
                v-if="(!signet.preventLoad && !(signet.error && signet.empty)) || PinnedFlag"
                :class="{'Onload': !bkmLoaded}"
                class="box-body"
            >
                <div v-show="bkmLoaded">
                    <listContent
                        :forceRefreshTabsBar="forceRefreshTabsBar"
                        @pinFile="pinFile($event)"
                        @setPinnedBkm="setPinnedBkm"
                        @pagination="pagination = $event"
                        @bkmReload="reloadBkmSkeleton($event)"
                        :prop-signet="signet"
                    />
                </div>
                <bkmSkeleton
                    v-show="!bkmLoaded"
                    :bkm-props="{pagination:pagination}"
                />
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
            if (signet) {
                Vue.set(signet, 'nbRows', options.pageInfo);
            }
        });
    }
};