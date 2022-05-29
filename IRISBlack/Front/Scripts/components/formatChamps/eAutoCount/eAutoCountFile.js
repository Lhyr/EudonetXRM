import { eAutoCountMixin } from './eAutoCountMixin.js?ver=803000'

export default {
    name: "eAutoCountFile",
    data() {
        return { };
    },
    mounted() { },
    methods: { },
    props: [],
    mixins: [eAutoCountMixin],
    template: `
    <div v-if="propListe" v-bind:class="[propListe ? 'listRubriqueCaractere' : '', 'ellips input-group hover-input']"
        :title="!dataInput.ToolTipText ? dataInput.DisplayValue : dataInput.ToolTipText">

        <!-- Si le champ autCount n'est modifiable -->        
        <div v-bind:class="[dataInput.ReadOnly ? 'readOnlyList' : '', 'ellipsDiv form-control input-line fname']" v-bind:style="{ color: dataInput.ValueColor}">
            <div class="targetIsTrue">{{dataInput.Value}}</div>
        </div>

    </div>
`
};