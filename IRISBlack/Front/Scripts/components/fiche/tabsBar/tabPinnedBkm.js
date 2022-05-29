import { BKMVIEWMODE, EdnType } from '../../../methods/Enum.min.js?ver=803000';
import { eFileMixin } from '../../../mixins/eFileMixin.js?ver=803000';
import { reloadBkm } from "../../../methods/eActionPinnedFileMethods.js?ver=803000";

export default {
    name: "tabPinnedBkm",
    data() {
        return {
            EdnType,
            DataJson: {},
            PinnedFlag: true
        };
    },
    components: {
        eBkmUnit: () => import(AddUrlTimeStampJS("../bkmUnit.js")),
        eBkmAnnexe: () => import(AddUrlTimeStampJS("../BkmAnnexe.js"))
    },
    computed: {
        /** Si un bkm n'est pas en cours de chargement et qu'on a 0 signet. */
        getDisplayBkm: function () {
            return !this.propBkm;
        },
    },
    watch:{
        /**
         * Recharge la liste afin de récupérer toutes les données à jour.
         */
        propBkm: {
            deep: true,
            handler(signet) {
                this.reloadBkm(signet?.DescId, signet?.id);
            }
        },
    },
    methods: {
        reloadBkm,
        /**
         * Permet de retourner un overflow sur la div parente, si on est en mode fiche.
         * @param {any} signet
         */
        getStyleViewMode: function (signet) {
            return signet.ViewMode == BKMVIEWMODE.FILE ? { 'overflow': 'auto' } : '';
        },
        pinBkm($event) {
            this.$emit('pinnedBkm', $event);
        },
        /** Récupère le bon composant à afficher en fonction du type de signet */
        getBkmComponent(propBkm){
            return propBkm?.TableType != EdnType?.FILE_PJ ? 'eBkmUnit' : 'eBkmAnnexe';
        }
    },
    props: {
        propBkm: Object,
        promBkmData: Promise,
        loadAnotherBkm: Boolean
    },
    mixins: [eFileMixin],
    template: `
<div>
    <component 
        :is="getBkmComponent(propBkm)" 
        @pinnedBkm="pinBkm($event)"
        :key="propBkm.id"
        :signet="propBkm"
        :PinnedFlag="PinnedFlag"
    />
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