import { eMoneyMixin } from './eMoneyMixin.js?ver=803000'

export default {
    name: "eMoneyFile",
    data() {
        return {};
    },
    mounted() { },
    methods: {},
    props: [],
    mixins: [eMoneyMixin],
    template: `
  <div ref="money" v-on:mouseout="showTooltip(false,'money',icon,IsDisplayReadOnly,dataInput)" v-on:mouseover="showTooltip(true,'money',icon,IsDisplayReadOnly,dataInput)" v-if="!propListe" v-bind:class="[IsDisplayReadOnly? 'headReadOnly' : '', focusIn ? 'focusIn' : '', 'ellips input-group hover-input']" 
        :title="!dataInput.ToolTipText ? dataInput.DisplayValue : dataInput.ToolTipText">

        <!-- <span ref="info" v-if="showInformationIco()" class="icon-info-circle info-tooltip"></span> -->
        <!-- Si le champ eNumeric est modifiable -->
        <input :field="'field'+dataInput.DescId" :IsDetail="propDetail" :IsAssistant="propAssistant" :IsHead="propHead" :IsList="propListe" 
            v-bind:class="[propHead ? 'class_liste_rubrique_money_header_' + dataInput.DescId  : '', propDetail ? 'class_liste_rubrique_money_detail_' + dataInput.DescId  : '', propAssistant ? 'class_liste_rubrique_money_assistant_' + propAssistantNbIndex + '_' + dataInput.DescId  : '', 'form-control input-line fname']" 
            v-on:focus="RemoveBorderSuccessError();bEmptyDisplayPopup = false; focusIn = true;verifyValChanged($event)" v-on:blur="verifMoney($event,that); focusIn = false"
            v-bind:style="{ color: dataInput.ValueColor}" v-if="!(propHead || dataInput.ReadOnly)" :value="dataInput.Value" type="text"
            :placeholder="dataInput.Watermark">

        <!-- Si le champ eNumeric n'est pas modifiable -->
        <span v-bind:style="{ color: dataInput.ValueColor}" v-if="dataInput.ReadOnly || propHead" class="readOnly">{{dataInput.Value}}</span>

        <!-- Icon -->
        <span  v-if="!propHead" v-on:click="focusInput('money', {
                    props: this.propAssistant ? PropType.Assistant : this.propDetail ? PropType.Detail : this.propListe ? PropType.Liste : PropType.Defaut,
		            propAssistantNbIndex: this.propAssistantNbIndex,
                    propIndexRow: this.propIndexRow,
                    dataInput: this.dataInput,
                    propSignet: this.propSignet
                })"  class="input-group-addon"><a  href="#!" class="hover-pen"><i :class="[IsDisplayReadOnly?'fas pencil-alt-slash-eudo':'fas fa-pencil-alt']"></i></a></span>
            

        <eAlertBox v-if="this.bDisplayPopup" :b-success="this.bRegExeSuccess">
            <p v-if="!this.bRegExeSuccess">{{messageError}}</p>
        </eAlertBox>

            <!-- Message d'erreur après la saisie dans le champs -->
                <eAlertBox v-if="this.bEmptyDisplayPopup && !(propHead || dataInput.ReadOnly)" >
                    <p>{{getRes(2471)}}</p>
                </eAlertBox>
</div>
`
};
