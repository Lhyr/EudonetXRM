import { eFileComponentsMixin } from '../../../mixins/eFileComponentsMixin.js?ver=803000';

export default {
    name: "ednReadOnlyButton",
    components: {
        ednReadOnlyLabel: () => import(AddUrlTimeStampJS("./ROSubComponents/ednReadOnlyLabel.js"))
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
    <ednReadOnlyLabel>
        <slot name="label"></slot>
    </ednReadOnlyLabel>
    <button>
        <slot></slot>
    </button>
`
};