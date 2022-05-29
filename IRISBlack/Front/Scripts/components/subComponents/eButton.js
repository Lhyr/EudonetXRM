import { eFileComponentsMixin } from '../../mixins/eFileComponentsMixin.js?ver=803000';

export default {
    name: "eButton",
    data() {
        return {};
    },
    methods: {
        /**
         * l'action a effectuer quand l'élément est sélectionné.
         * @param {any} event
         */
        action: function (event) {
            this.$emit("action", event)
        }
    },
    mixins: [eFileComponentsMixin],
    props: { oTblItm: Object },
    template: `
    <button :class="oTblItm?.CssClass"
        :value="oTblItm?.Value"
        @click.stop="action"
        @mouseover="action"
        @mouseleave="action">
       <slot></slot>
    </button>
`
};