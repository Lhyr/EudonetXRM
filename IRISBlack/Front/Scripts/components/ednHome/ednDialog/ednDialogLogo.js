import { eDialogMixin } from '../../../mixins/eDialogMixin.js?ver=803000';

export default {
    name: "ednDialogLogo",
    components: {},
    mixins: [eDialogMixin],
    props: {
        sImage: {
            type: String,
            default: "./IRISBlack/Front/Assets/Imgs/Eudonet.svg"
        }
    },
    template: `<v-toolbar-title><v-img :src="sImage" class="edn--home--dialog--logo"></v-img></v-toolbar-title>`,
};