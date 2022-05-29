import { eUserMixin } from './eUserMixin.js?ver=803000'


export default {
    name: "eUserListSimpleNotEditable",
    data() {
        return {};
    },
    mounted() { },
    methods: {},
    props: [],
    mixins: [eUserMixin],
    template: `
        <div v-bind:style="{ color: dataInput.ValueColor}" v-if="dataInput.ReadOnly && !dataInput.Multiple"  type="text" class="NoModifSimple ellipsDiv form-control input-line fname">
            <div :field="'field'+dataInput.DescId" class="targetIsTrue">{{dataInput.DisplayValue}}</div> 
        </div>
`
};
