import { eCatalogMixin } from './eCatalogMixin.js?ver=803000'

export default {
    name: "eCatalogFileSimpleNotEditable",
    data() {
        return {};
    },
    mounted() { },
    methods: {},
    props: [],
    mixins: [eCatalogMixin],
    template: `
        <div v-bind:style="{ color: dataInput.ValueColor}" v-if="(propHead || dataInput.ReadOnly) && !dataInput.Multiple"  type="text" class="ellipsDiv form-control input-line fname">
            {{dataInput.DisplayValue}}
                <span class="multiple_choice_remove" role="presentation"></span>
        </div>
`
};