import { eFileComponentsMixin } from '../../../mixins/eFileComponentsMixin.js?ver=803000';

export default {
    name: "ednReadOnlyIcon",
    data() {
    },
    components: {
        ednReadOnlyLabel: () => import(AddUrlTimeStampJS("./ROSubComponents/ednReadOnlyLabel.js"))
    },
    computed: {
        /** fonction récupérée dans Vuetify pour récupérer une icone depuis un slot. */
        getIcon: function () {
            let iconName = '';

            if (this.$slots.default && this.$slots.default.length > 0)
                iconName = this.$slots.default[0].text?.trim();

            return iconName;
        },
    },
    methods: {
        /**
         * l'action a effectuer quand l'élément est sélectionné.
         * @param {any} event
         */
        action: function (event) {
            this.emit("action", event)
        }
    },
    mixins: [eFileComponentsMixin],
    props: { oTblItm: Object },
    template: `
    <span class="getIcon">
    </span>
    <ednReadOnlyLabel>
        <slot name="label"></slot>
    </ednReadOnlyLabel>
`
};