import { eDialogMixin } from '../../../mixins/eDialogMixin.js?ver=803000';

export default {
    name: "ednDialogToolBarItem",
    data: function () {
        return {
        }
    },
    components: {},
    mixins: [eDialogMixin],
    props: {
        oProps: Object,
    },
    created() {
    },
    template: `
            <v-toolbar-items>
                <slot></slot>
            </v-toolbar-items>
`,
};