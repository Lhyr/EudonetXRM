import { eDialogMixin } from '../../../mixins/eDialogMixin.js?ver=803000';

export default {
    name: "ednDialogLinkContainer",
    data: function () {
        return {
            sColor: "",
            aLinks: [],
            bText: true,
        }
    },
    components: {
        ednDialogLink: () => import(AddUrlTimeStampJS("./ednDialogLink.js")),
        ednDialogToolBarItem: () => import(AddUrlTimeStampJS("./ednDialogToolBarItem.js")),
    },
    methods: {
        /**
         * Rervoie la props pour les liens.
         * @param {any} itm
         */
        getPropsLink: function (itm) {
            return {
                sId: itm.id,
                sLabel: itm.label,
                sColor: this.sColor,
                bText: this.bText,
            }
        },
        openLink: function (itm) {
            window.open(itm.url);
        },
    },
    props: {
        oProps: Object,
    },
    mixins: [eDialogMixin],
    created() {
        this.sColor = this.oProps?.sColor || this.sColor;
        this.aLinks = this.oProps?.aLinks || this.aLinks;
        this.bText = this.oProps?.bText;
    },
    template: `<ednDialogToolBarItem>
                <template v-for="item in aLinks">
                    <ednDialogLink :oProps="getPropsLink(item)" @action="openLink(item)"></ednDialogLink>
                </template>
               </ednDialogToolBarItem>`,
};