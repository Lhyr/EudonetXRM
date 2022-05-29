import { eFileMixin } from '../../mixins/eFileMixin.js?ver=803000';

export default {
    name: "eIcon",
    data() {
        return {};
    },
    computed: {
        /** Permet de récupérer les informations du slot par défaut et de les réinjecter ailleurs. */
        getIconFromSlot: function () {
            return ((this.$slots["default"]
                && this.$slots["default"][0]?.text.trim())
                || "");
        },
    },
    mixins: [eFileMixin],
    props: { oTblItm: Object },
    template: `<i :class="getIconFromSlot"></i>`,
};