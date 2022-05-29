import { eCatalogMixin } from './eCatalogMixin.js?ver=803000'

export default {
    name: "eCatalogListSimpleNotEditable",
    data() {
        return {};
    },
    mounted() { },
    methods: {},
    props: [],
    mixins: [eCatalogMixin],
    template: `
        <div v-bind:style="{ color: dataInput.ValueColor}" v-if="dataInput.ReadOnly && !dataInput.Multiple"  type="text" class="NoModifSimple ellipsDiv form-control input-line fname">
            <div class="targetIsTrue">{{dataInput.DisplayValue}}</div> 
        </div>
`
};