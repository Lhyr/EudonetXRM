import { eNumericMixin } from './eNumericMixin.js?ver=803000'

export default {
    name: "eNumericList",
    data() {
        return {};
    },
    mounted() { },
    methods: {},
    props: [],
    mixins: [eNumericMixin],
    template: `
    <div v-if="propListe" v-bind:class="[propListe ? 'listRubriqueCaractere' : '', 'ellips input-group hover-input']"
        :title="!dataInput.ToolTipText ? dataInput.DisplayValue : dataInput.ToolTipText">

        <!-- Si le champ numeric est modifiable -->
        <input :IsDetail="propDetail" :IsAssistant="propAssistant" :IsHead="propHead" :IsList="propListe" v-if="!dataInput.ReadOnly" 
            :class="'class_liste_rubrique_numeric_' + propSignet.DescId + '_' + propIndexRow + '_' + dataInput.DescId" 
            v-bind:disabled="dataInput.ReadOnly" 
            v-on:blur="verifNumeric($event,that);" 
            v-bind:style="{ color: dataInput.ValueColor}"  
            :value="dataInput.DisplayValue" type="text" class="form-control input-line fname"
            :placeholder="dataInput.Watermark">
       
        <!-- Si le champ numeric est pas modifiable -->        
        <div v-bind:style="{ color: dataInput.ValueColor}" v-if="dataInput.ReadOnly"  type="text" class="ellipsDiv form-control input-line fname">
            {{dataInput.DisplayValue}}
        </div>

        <!-- Icon -->
        <span v-on:click="focusInput('numeric', {
                    props: propAssistant ? PropType.Assistant : propDetail ? PropType.Detail : propListe ? PropType.Liste : PropType.Defaut,
		            propAssistantNbIndex: propAssistantNbIndex,
                    propIndexRow: propIndexRow,
                    dataInput: dataInput,
                    propSignet: propSignet
                })" v-if="!dataInput.ReadOnly" class="input-group-addon"><a  href="#!" class="hover-pen"><i class="fas fa-pencil-alt"></i></a></span>
    </div>
`
};
