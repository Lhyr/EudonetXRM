import { eModalComponentsMixin } from '../../../Scripts/mixins/eModalComponentsMixin.js?ver=803000';

export default {
    name: "eAccordionPane",
    mixins: [eModalComponentsMixin],
    data() {
        return {

        }
    },
    computed: {
    },
    methods: {
        /**
         * événement qui se produit quand on bascule
         * du détail ouvert au détail fermé.
         * @param {any} event
         */
        toggle: function (event) {
            this.$emit("toggle", event);
        },
        /**
         * Oblige le detail à s'ouvrir ou se fermer selon la props.
         * */
        setDetailsOpen(bOpen) {
            this.$emit('setDetailsOpen', bOpen);
        },

        /** Détermine si le details doit être ouvert ou fermé */
        setInitPaneOpenClose() {
            this.$refs["detailsPane"].open = this.isDetailOpen;
        }
    },
    props: {
        cssClassElt: String,
        cssClassFtr: String,
        cssClassPane: String,
        isDetailOpen: Boolean,
        readOnly:Boolean
    },
    created() {
    },
    updated() {
        this.setInitPaneOpenClose();
    },
    mounted() {
        this.setInitPaneOpenClose();
    },
    template: `
        <details ref="detailsPane" 
            @toggle="toggle" 
            @mouseover="setDetailsOpen(true)"
            @mouseout="setDetailsOpen(false)"
            :class="cssClassPane">
            <summary>
                <slot name="header"></slot>                  
            </summary>
            <div :class="cssClassElt">
                <slot name="main"></slot>
            </div>
            <div v-if="!readOnly" :class="cssClassFtr">
                <slot name="footer"></slot>
            </div>
        </details>
`
}