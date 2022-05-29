import { eMoneyMixin } from './eMoneyMixin.js?ver=803000'

export default {
    name: "eMoneyList",
    data() {
        return {};
    },
    mounted() { },
    methods: {},
    props: [],
    mixins: [eMoneyMixin],
    template: `
<div v-if="propListe" v-bind:class="[propListe ? 'listRubriqueCaractere' : '', 'ellips input-group hover-input']"
        :title="!dataInput.ToolTipText ? dataInput.DisplayValue : dataInput.ToolTipText">

        <!-- Si le champ numeric est modifiable -->
        <input :field="'field'+dataInput.DescId" v-on:focus="RemoveBorderSuccessError();bEmptyDisplayPopup = false;;verifyValChanged($event) " v-on:blur="verifMoney($event,that); focusIn = false" :IsDetail="propDetail" :IsAssistant="propAssistant" 
            :IsHead="propHead" :IsList="propListe" v-if="!dataInput.ReadOnly" :class="'class_liste_rubrique_money_' + propSignet.DescId + '_' + propIndexRow + '_' + dataInput.DescId" 
            v-bind:disabled="dataInput.ReadOnly" v-bind:style="{ color: dataInput.ValueColor}"  :value="dataInput.Value" type="text" class="form-control input-line fname"
            :placeholder="dataInput.Watermark">
       
        <!-- Si le champ numeric n'est pas modifiable -->        
        <div v-bind:style="{ color: dataInput.ValueColor}" v-if="dataInput.ReadOnly"  type="text" class="ellipsDiv form-control input-line fname">
            {{dataInput.Value}}
        </div>

        <!-- Icon -->
        <span v-on:click="focusInput('money',{
                    props: propAssistant ? PropType.Assistant : propDetail ? PropType.Detail : propListe ? PropType.Liste : PropType.Defaut,
		            propAssistantNbIndex: propAssistantNbIndex,
                    propIndexRow: propIndexRow,
                    dataInput: dataInput,
                    propSignet: propSignet
                })" v-if="!dataInput.ReadOnly" class="input-group-addon"><a  href="#!" class="hover-pen"><i class="fas fa-pencil-alt"></i></a></span>
    </div>
`
};
