import { eUserMixin } from './eUserMixin.js?ver=803000'

export default {
    name: "eUserFileMultipleNotEditable",
    data() {
        return {};
    },
    mounted() { },
    methods: {},
    props: [],
    mixins: [eUserMixin],
    template: `
        <span v-if="(propHead || dataInput.ReadOnly) && dataInput.Multiple " :id="'ID_' + dataInput.DescId" class=" noModif multiRenderer form-control input-line fname bold ">
            <ul>
                <li v-bind:style="{ background: dataInput.ValueColor, borderColor:dataInput.ValueColor}" v-for="value in this.valueMultiple" :key="value.id">
                <!-- <span class="multiple_choice_remove" role="presentation">&#215;</span> -->
                    {{value}}
                </li>
            </ul>
        </span>
`
};
