import { eUserMixin } from './eUserMixin.js?ver=803000'

export default {
    name: "eUserListMultipleEditable",
    data() {
        return {};
    },
    mounted() { },
    methods: {},
    props: [],
    mixins: [eUserMixin],
    template: `
        <span v-on:click="openDial" v-if="dataInput.Multiple && !dataInput.ReadOnly" class="Modif multiRenderer form-control input-line fname bold">
            <ul>
                <li v-bind:style="{ background: dataInput.ValueColor, borderColor:dataInput.ValueColor}" v-for="value in this.valueMultiple" :key="value.id">
                <!-- <span class="multiple_choice_remove" role="presentation">&#215;</span> -->
                    {{value}}
                </li>
            </ul>
        </span>
`
};
