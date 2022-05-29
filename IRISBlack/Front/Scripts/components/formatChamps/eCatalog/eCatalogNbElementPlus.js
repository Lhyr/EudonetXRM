import { eCatalogMixin } from './eCatalogMixin.js?ver=803000'

export default {
    name: "eCatalogNbElementPlus",
    data() {
        return {};
    },
    mounted() { },
    methods: {
    },
    computed: {
        /** Récupère le nombre d'éléments visibles */
        getNbCatHiddenElem(){
            return this.valueMultiple.length - this.nMaxCatalogElm
        }
    },
    props: {
        dataInput: Object,
        nMaxCatalogElm: Number
    },
    mixins: [eCatalogMixin],
    template: `<sup class="multiRenderer fname expCagalogColor">
                    {{getNbCatHiddenElem}}+
               </sup>`
};