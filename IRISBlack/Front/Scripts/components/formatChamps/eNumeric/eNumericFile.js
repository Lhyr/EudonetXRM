import { eNumericMixin } from './eNumericMixin.js?ver=803000'

export default {
    name: "eNumericFile",
    data() {
        return {};
    },
    mounted() { },
    methods: {},
    props: [],
    mixins: [eNumericMixin],
    template: `
 <div ref="numeric" v-on:mouseout="showTooltip(false,'numeric',icon,IsDisplayReadOnly,dataInput)" v-on:mouseover="showTooltip(true,'numeric',icon,IsDisplayReadOnly,dataInput)"  v-if="!propListe" v-bind:class="[focusIn ? 'focusIn': '', IsDisplayReadOnly?'read-only':'', propHead ? 'headReadOnly' : '', 'ellips input-group hover-input']"
        :title="!dataInput.ToolTipText ? dataInput.DisplayValue : dataInput.ToolTipText">

        <!-- <span ref="info" v-if="showInformationIco()" class="icon-info-circle info-tooltip"></span> -->

        <!-- Si le champ eNumeric est modifiable -->
        <input :IsDetail="propDetail" :IsAssistant="propAssistant" :IsHead="propHead" :IsList="propListe" 
            v-bind:class="[propHead ? 'class_liste_rubrique_numeric_header_' + dataInput.DescId  : '', propDetail ? 'class_liste_rubrique_numeric_detail_' + dataInput.DescId  : '', propAssistant ? 'class_liste_rubrique_numeric_assistant_' + propAssistantNbIndex + '_' + dataInput.DescId  : '', 'form-control input-line fname']"
            v-on:focus="focusIn = true; bDisplayPopup = false; RemoveBorderSuccessError();bEmptyDisplayPopup = false;inputHovered = true "
            v-on:blur="verifNumeric($event,that);inputHovered = false; focusIn = false" 
            v-bind:style="{ color: dataInput.ValueColor}" v-if="!(propHead || dataInput.ReadOnly)" :value="dataInput.DisplayValue" type="text"
            :placeholder="dataInput.Watermark">

        <!-- Si le champ eNumeric n'est pas modifiable -->
        <span v-bind:style="{ color: dataInput.ValueColor}" v-if="dataInput.ReadOnly || propHead" class="readOnly">{{dataInput.DisplayValue}}</span>

        <!-- Icon -->
        <span v-if="!propHead" v-on:click="focusInput('numeric', {
                    props: this.propAssistant ? PropType.Assistant : this.propDetail ? this.PropType.Detail : this.propListe ? this.PropType.Liste : this.PropType.Defaut,
		            propAssistantNbIndex: this.propAssistantNbIndex,
                    propIndexRow: this.propIndexRow,
                    dataInput: this.dataInput,
                    propSignet: this.propSignet
                })"  
                :class="[inputHovered ? 'editing-mode':'','input-group-addon']"><a  href="#!" class="hover-pen"><i :class="[IsDisplayReadOnly?'fas pencil-alt-slash-eudo':'fas fa-pencil-alt']"></i></a></span>

        <eAlertBox v-if="this.bDisplayPopup" :b-success="this.bRegExeSuccess">
            <p v-if="!this.bRegExeSuccess">{{messageError}}</p>
        </eAlertBox>

    	<!-- Message d'erreur si champs obligatoire est vide -->
        <eAlertBox v-if="dataInput.Required && this.bEmptyDisplayPopup && !(propHead || dataInput.ReadOnly)" >
            <p>{{getRes(2471)}}</p>
        </eAlertBox>
    </div>
`
};
