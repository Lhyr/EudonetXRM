import EventBus from '../../../bus/event-bus.js?ver=803000';
import { eListComponentsMixin } from '../../../mixins/eListComponentsMixin.js?ver=803000';
import { typeAlert } from "../../../methods/Enum.min.js?ver=803000"

export default {
    name: "eEmptyErrorList",
    data() {
        return {
            typeCssAlert: typeAlert.Standard,
        };
    },
    components: {
        eAlertBox: () => import(AddUrlTimeStampJS("../../modale/alertBox.js"))

    },
    props: {
        dataJson: [Object, Array],
        propSignet: Object,
        error: Boolean,
    },
    mixins: [eListComponentsMixin],
    computed: {
        /** Message d'erreur. */
        errorBkm() {
            return this.error;
        },
        /** le signet ne possède aucune information. */
        emptyBkm() {
            return (!this.dataJson || this.dataJson == null || this.dataJson?.PagingInfo?.NbTotalRows == 0)
                && !this.propSignet?.ExpressFilterActived;
        },
        /** Le filtre activé ne retrourne aucune information. */
        emptyBkmCauseFilter() {
            return this.propSignet?.ExpressFilterActived;
        },
        /**
         * retourne le titre du message, suivant si c'est une erreur,
         * un signet vide ou un signet 
         * */
        titleMessage() {
            let strTitle = this.getRes(7164);

            if (this.errorBkm) {
                strTitle = this.getRes(72);
            }
            else if (this.emptyBkm) {
                strTitle = this.getRes(83);
            }
            else if (this.emptyBkmCauseFilter) {
                strTitle = this.getRes(809);
            }

            return strTitle;
        }
    },
    methods: {
        /**
         * Permet de désactiver les filtres express
        **/
        clearExpressFilter(bkm) {
            EventBus.$emit('clearExpressFilter', bkm);
        },
    },
    template: `
		<eAlertBox css-class="NoDataTxtAssistant" css-style="padding: 30px 0 0 0;"  :type-css-alert="typeCssAlert">
            <h3>{{ titleMessage }}</h3>
            <template v-if="emptyBkm || errorBkm">

                <!-- US 1491 (amélioration de messages) -> tache 3133 ([Nouveau mode fiche] supprimer le sous titre) -->
                <!-- <p>{{ (emptyBkm) ? getRes(2496) : getRes(2640) }}</p> -->
                <p>{{ (emptyBkm) ? '' : getRes(2640) }}</p>

            </template>
            <template v-else-if="emptyBkmCauseFilter">
                <a @click="clearExpressFilter(propSignet)" href="#!">
                    {{getRes(6369)}}
                </a>
            </template>            
            <template v-else>
                <p></p>
            </template>
		</eAlertBox>
`
};