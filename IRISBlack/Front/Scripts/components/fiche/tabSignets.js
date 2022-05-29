import { EdnType } from '../../methods/Enum.min.js?ver=803000';
import { eFileMixin } from '../../mixins/eFileMixin.js?ver=803000';

export default {
    name: "tabSignets",
    data() {
        return {
            EdnType,
        };
    },
    components: {
        eBkmUnit: () => import(AddUrlTimeStampJS("./bkmUnit.js")),
        eBkmAnnexe: () => import(AddUrlTimeStampJS("./BkmAnnexe.js")),
    },
    computed: {
        /** Si un bkm n'est pas en cours de chargement et qu'on a 0 signet. */
        getDisplayBkm: function () {
            return this.propSignet.signets <= 0;
        },
    },
    methods: {
        /**
         * Permet de retourner un overflow sur la div parente, si on est en mode fiche.
         * @param {any} signet
         */
        getStyleViewMode: function(signet) {
            return signet.ViewMode == 1 ? { 'overflow': 'auto' } : '';
        },
        pinBkm($event) {
            this.$emit('pinnedBkm', $event);
        }
    },
    props: {
        propSignet: Object,
        propDataDetail: [Object, Array],
        loadAnotherBkm: Boolean,
        forceRefreshTabsBar: Number
    },
    mixins: [eFileMixin],
    template: `
<div>
    <template v-for="(signet, index) in propSignet.signets"  >
        <eBkmAnnexe :forceRefreshTabsBar="forceRefreshTabsBar" v-if="signet.TableType == EdnType.FILE_PJ" :bPinned.sync="signet.IsPinned" @pinnedBkm="$emit('pinnedBkm',$event)" :key="signet.id" :signet="signet" />
        <eBkmUnit :forceRefreshTabsBar="forceRefreshTabsBar" v-else :bPinned.sync="signet.IsPinned" @pinnedBkm="pinBkm($event)" :key="signet.id" :signet="signet" />
    </template>
    <div v-if="getDisplayBkm">
        <div style="height: 210px;">
            <div class="NoDataTxtAssistant">
                <h3>{{ getRes(2643) }}</h3>
                <p>{{ getRes(2644) }}.</p>
            </div>
        </div>
    </div>
</div>`
};