import { eUserMixin } from './eUserMixin.js?ver=803000'

export default {
    name: "eUserFileSimpleNotEditable",
    data() {
        return {};
    },
    mounted() { },
    methods: {},
    props: [],
    mixins: [eUserMixin],
    template: `
        <div v-bind:style="{ color: dataInput.ValueColor}" v-if="(propHead || dataInput.ReadOnly) && !dataInput.Multiple"  type="text" class="ellipsDiv form-control input-line fname">
            {{dataInput.DisplayValue}}
        </div>
`
};
