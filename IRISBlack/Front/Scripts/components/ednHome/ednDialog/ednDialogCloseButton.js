import { eDialogMixin } from '../../../mixins/eDialogMixin.js?ver=803000';

export default {
    name: "ednDialogCloseButton",
    data() {
        return {
            sIcon: "mdi-close-circle",
            sColor: "black",
            sClass: "edn--home--dialog--closebutton",
            sId: "ednHomeDialogCloseButton"
        }
    },
    components: {
        ednDialogButton: () => import(AddUrlTimeStampJS("./ednDialogButton.js")),
    },
    computed: {},
    mixins: [eDialogMixin],
    methods: {
        /**
            * On renvoie l'action du bouton au parent.
            * @param {any} event
            */
        action: function (event) {
            this.$emit("action", event)
        }
    },
    props: {
        oProps: Object,
    },
    created() {
        this.sId = this.oProps?.sId || this.sId;
        this.sIcon = this.oProps?.sIcon || this.sIcon;
        this.sColor = this.oProps?.sColor || this.sColor;
        this.sClass = this.oProps?.sClass || this.sClass;
    },
    template: ` <ednDialogButton :oProps="oProps" @action="action($event)">
                    <v-icon :color="sColor" :class="sClass">
                        <slot>{{sIcon}}</slot>
                    </v-icon>
                </ednDialogButton>`,
};