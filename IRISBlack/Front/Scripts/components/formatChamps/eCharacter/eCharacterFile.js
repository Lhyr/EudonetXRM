import { eCharacterMixin } from './eCharacterMixin.js?ver=803000'

export default {
    name: "eCharacterFile",
    data() {
        return {};
    },
    mounted() { },
    methods: {},
    props: [],
    mixins: [eCharacterMixin],
    template: `

	<div ref="character" v-on:mouseout="showTooltip(false,'character',icon,IsDisplayReadOnly,dataInput)" v-on:mouseover="showTooltip(true,'character',icon,IsDisplayReadOnly,dataInput)" v-if="!propListe" v-bind:class="[IsDisplayReadOnly?'read-only':'', propHead ? 'headReadOnly' : '', focusIn ? 'focusIn' : '' , 'ellips input-group hover-input']">
   
        <!-- <span ref="info" v-if="showInformationIco()" class="icon-info-circle info-tooltip"></span>  -->

		<!-- Si le champ eCharactere est modifiable -->
		<input spellcheck="false" autocomplete="off" :ref="" v-on:keyup="dataInput.AutoComplete && ((triggerSireneMapping != null && triggerSireneMapping.indexOf(dataInput.DescId + ';') != -1) || (triggerPredictiveAddressMapping ==  dataInput.DescId)) ? getDataAutocompletion($event) : ''" :ref="'field'+dataInput.DescId" :field="'field'+dataInput.DescId" 
        v-on:focus="openAutoCompletion = true; dataInput.AutoComplete && ((triggerSireneMapping != null && triggerSireneMapping.indexOf(dataInput.DescId + ';') != -1) || (triggerPredictiveAddressMapping ==  dataInput.DescId)) ? getDataAutocompletion($event) : ''; bDisplayPopup = false; RemoveBorderSuccessError();bEmptyDisplayPopup = false; focusIn = true;verifyValChanged($event)" 
		v-on:blur="verifChar($event,that); focusIn = false" :IsDetail="propDetail" :IsAssistant="propAssistant" :IsHead="propHead"
        v-on:mouseover="verifyValChanged($event)"
			:IsList="propListe" v-bind:class="[propHead ? 'class_liste_rubrique_caractere_header_' + dataInput.DescId  : '', propDetail ? 'class_liste_rubrique_caractere_detail_' + dataInput.DescId  : '', propAssistant ? 'class_liste_rubrique_caractere_assistant_' + propAssistantNbIndex + '_' + dataInput.DescId  : '', 'form-control input-line fname']" v-bind:style="{ color: dataInput.ValueColor}" v-if="!(propHead || dataInput.ReadOnly)" 
			 type="text" :placeholder="dataInput.Watermark">

		<!-- Si le champ eCharactere n'est pas modifiable -->
		<span v-bind:style="{ color: dataInput.ValueColor}" v-if="dataInput.ReadOnly || propHead" class="readOnly">{{dataInput.Value}}</span>

		<!-- Icon -->
		<span v-if="!propHead" v-on:click="focusInput('caractere', {props: propAssistant ? PropType.Assistant : propDetail ? PropType.Detail : propListe ? PropType.Liste : PropType.Defaut,
		propAssistantNbIndex: propAssistantNbIndex,
		propIndexRow: propIndexRow,
		dataInput: dataInput,
		propSignet: propSignet
	})" class="input-group-addon"><a  href="#!" class="hover-pen"><i :class="[IsDisplayReadOnly?'fas pencil-alt-slash-eudo':'fas fa-pencil-alt']"></i></a></span>

		
		<!-- Message d'erreur après la saisie dans le champs -->
		<eAlertBox v-if="this.bEmptyDisplayPopup && !(propHead || dataInput.ReadOnly)" >
			<p>{{getRes(2471)}}</p>
		</eAlertBox>
	</div>
`
};