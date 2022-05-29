import { eCatalogMixin } from './eCatalogMixin.js?ver=803000'

export default {
    name: "eCatalogList",
    data() {
        return {};
    },
    components: {
        eCatalogListSimpleEditable: () => import(AddUrlTimeStampJS("./eCatalogListSimpleEditable.js")),
        eCatalogListSimpleNotEditable: () => import(AddUrlTimeStampJS("./eCatalogListSimpleNotEditable.js")),

    },
    mounted() { },
    methods: {},
    props: [],
    mixins: [eCatalogMixin],
    template: `
 <div v-if="propListe" v-bind:class="[propListe ? 'listRubriqueRelation' : '', 'ellips input-group hover-input']"  
        :title="!dataInput.ToolTipText ? dataInput.DisplayValue : dataInput.ToolTipText">
    <!--<div v-if="propListe" class="ellips input-group hover-input" :title="!dataInput.ToolTipText ? dataInput.DisplayValue : dataInput.ToolTipText">-->

        <!-- Si le champ simple et modifiable -->
        <eCatalogListSimpleEditable />

        <!-- Si le champ simple et pas modifiable -->
        <eCatalogListSimpleNotEditable />

        <!-- Icon -->
        <span @click="showCatalogGenericViewIris" v-if="!dataInput.ReadOnly" class="input-group-addon"><a  href="#!" class="hover-pen"><i class="fas fa-pencil-alt"></i></a></span>
    </div>
`
};