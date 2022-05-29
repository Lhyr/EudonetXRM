import { eUserMixin } from './eUserMixin.js?ver=803000'


export default {
    name: "eUserListSimpleEditable",
    data() {
        return {};
    },
    mounted() { },
    methods: {},
    props: [],
    mixins: [eUserMixin],
    template: `
        <div v-on:click="openDial" v-if="!dataInput.ReadOnly && !dataInput.Multiple" v-bind:style="{ color: dataInput.ValueColor}" type="text" class="modifSimple ellipsDiv form-control input-line fname">
            <div :field="'field'+dataInput.DescId" class="targetIsTrue">{{dataInput.DisplayValue}}</div> 
        </div>
`
};
