import { eCatalogMixin } from './eCatalogMixin.js?ver=803000'

export default {
    name: "eCatalogBtnBase",
    data() {
        return {};
    },
    components: {
    },
    computed: {
        /**
         * Choix de l'icone à afficher suivant qu'on soit en lecture seule ou non.
         * */
        displayIcoBtn: function () {
            return [this.IsDisplayReadOnly ? 'mdi mdi-lock' : 'fas fa-pencil-alt'];
        }
    },
    mounted() { },
    methods: {
        /**
         * revoie l'action au parent qui traitera.
         * @param {any} event
         */
        sendAction: function (event) {
            this.$emit("sendAction", event);
        }

    },
    props: {
        dataInput: Object,
    },
    mixins: [eCatalogMixin],
    template: `
        <span @click="sendAction($event)" class="input-group-addon">
            <a href="#!" class="hover-pen">
                <i :class="displayIcoBtn"></i>
            </a>
        </span>
`
};