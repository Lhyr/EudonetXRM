import { eDialogMixin } from '../../../mixins/eDialogMixin.js?ver=803000';

export default {
    name: "ednDialogLink",
    data() {
        return {
            sId: "",
            sLabel: "",
            sColor: "",
            bText: false,
        }
    },
    props: {
        oProps: Object,
    },
    components: {
        ednDialogButton: () => import(AddUrlTimeStampJS("./ednDialogButton.js")),
    },
    computed: {
        /** Retourne la props pour le bouton. */
        getPropsButton: function () {
            return {
                bText: this.bText,
                sColor: this.sColor,
            }
        }
    },
    mixins: [eDialogMixin],
    methods: {
    },
    created() {
        this.sId = this.oProps?.sId || this.sId;
        this.sLabel = this.oProps?.sLabel || this.sLabel;
        this.sColor = this.oProps?.sColor || this.sColor;
        this.bText = this.oProps?.bText
    },

    template: `<ednDialogButton :oProps="getPropsButton" :id="sId" @action="action">{{sLabel}}</ednDialogButton>`,
};