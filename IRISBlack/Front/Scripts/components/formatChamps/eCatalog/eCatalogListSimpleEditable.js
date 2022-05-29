import { eCatalogMixin } from './eCatalogMixin.js?ver=803000'

export default {
    name: "eCatalogListSimpleEditable",
    data() {
        return {};
    },
    mounted() { },
    methods: {},
    props: [],
    mixins: [eCatalogMixin],
    template: `
        <div @click="showCatalogGenericViewIris" v-if="!dataInput.ReadOnly && !dataInput.Multiple" v-bind:style="{ color: dataInput.ValueColor}" type="text" class="modifSimple ellipsDiv form-control input-line fname">
            <div :ref="'field'+dataInput.DescId" :field="'field'+dataInput.DescId" class="targetIsTrue">{{dataInput.DisplayValue}}</div> 
        </div>
`
};