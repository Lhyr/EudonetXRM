import { eFileComponentsMixin } from '../../../mixins/eFileComponentsMixin.js?ver=803000';

export default {
    name: "eSpeedDial",
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
    <div @action="action">
        <slot name="activator" :on="on" :attrs="attrs"></slot>
        <div>
            <slot></slot>
        </div>
    </div>
`
};