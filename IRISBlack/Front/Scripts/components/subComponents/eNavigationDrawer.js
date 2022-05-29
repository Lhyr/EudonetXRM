import { eFileComponentsMixin } from '../../mixins/eFileComponentsMixin.js?ver=803000';

export default {
    name: "eNavigationDrawer",
    data() {
        return {
            BackgroundColor: "",
            ForeColor: "white",
            CssClass: "",
        };
    },
    computed: {
        /** Récupére le style du composant parent, via une props.
         * Sinon, c'est le style par défaut. */
        getStyleFromAbove() {
            return {
                //'background-color': this.oTblItm?.BackgroundColor || this.BackgroundColor,
                'color': this.oTblItm?.ForeColor || this.ForeColor,
            }
        }
    },
    methods: {
        /**
         * l'action a effectuer quand l'élément est sélectionné.
         * @param {any} event
         */
        action: function (event) {
            this.$emit("action", event);
        },
    },
    mixins: [eFileComponentsMixin],
    props: { oTblItm: Object},
    template: `    
    <div :style="getStyleFromAbove" :class="oTblItm.CssClass || CssClass">
        <slot name="activator"></slot>
        <slot name="default"></slot>
    </div>
`
};