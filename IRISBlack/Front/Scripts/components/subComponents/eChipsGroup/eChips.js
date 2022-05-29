import { eFileComponentsMixin } from '../../../mixins/eFileComponentsMixin.js?ver=803000';

export default {
    name: "eChips",
    data() {
        return {};
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
    <div>
       <slot></slot>
    </div>
`
};