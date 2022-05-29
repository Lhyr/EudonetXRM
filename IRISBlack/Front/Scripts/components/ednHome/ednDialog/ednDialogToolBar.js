import { eDialogMixin } from '../../../mixins/eDialogMixin.js?ver=803000';

export default {
    name: "ednDialogToolBar",
    data: function () {
        return {
            sColor: "",
            sClass: ""
        }
    },
    components: {},
    mixins: [eDialogMixin],
    props: {
        oProps: Object,
    },
    created() {
        this.sColor = this.oProps?.sColor || this.sColor;
        this.sClass = this.oProps?.sClass || this.sClass;
    },
    template: `
            <v-toolbar :color="sColor" :class="sClass">
                <slot></slot>
            </v-toolbar>
`,
};