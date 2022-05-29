import { eFileMixin } from '../../mixins/eFileMixin.js?ver=803000';
import { setClosePinnedBkm } from "../../methods/eFileMethods.js?ver=803000";

export default {
    name: "BkmHeader",
    methods: {
        setClosePinnedBkm,
        cancelExpressFilter(bkm) {
            var updatePref = "tab=" + this.getTab + ";$;bkm=" + bkm.DescId + ";$;filterExpress=$cancelallexpressfilters$";
            this.setUserBkmPref({ updatePref });
            this.$emit('reloadBkm', bkm)
        },
        SetHistoric(nBkmTab, callBack) {
            this.$emit('SetHistoric', {
                nBkmTab: nBkmTab,
                callBack: callBack
            })
        },
        /** Annule l'épinglage  */
        cancelPinnedBkm() {
            // this.setClosePinnedBkm();
            this.$emit('update:bPinned', false);
        }
    },
    computed: {},
    components: {},
    props: {
        signet: {
            type: Object,
            default: {}
        },
        oTblItem: {
            type: Object,
            default: {}
        },
        bPinned: {
            type: Boolean,
            default: false
        },
        PinnedFlag: {
            type: Boolean,
            default: false
        }
    },
    mixins: [
        eFileMixin
    ],
    template: `
        <div ref="header" class="box-header bkm-header with-border">
            <h3 class="box-title">
                {{signet.nbRows ? signet.nbRows : 0}} {{signet.Label}}
            </h3>
            <a
                v-if="bPinned && !PinnedFlag"
                class="pinned-bkm"
                @click="cancelPinnedBkm"
                :title="oTblItem.pinnedBkmTitle"
            >
                <i class="fas fa-thumbtack"></i>
            </a>
            <a class="eFilterExcpress"
                v-if=" signet.ExpressFilterActived "
                :title="oTblItem.expressTitle"
                @click="cancelExpressFilter(signet)"
            >
                <i class="FilterActived i-table icon-rem_filter"></i>
            </a>
            <a class="eFilterExcpress  eHistorique" v-if="signet.HistoricActived" :title="oTblItem.historiqueTitle" @click="SetHistoric(signet)" > 
                <i class="FilterActived i-table fa fa-history"></i>
            </a>            
        </div>`
}