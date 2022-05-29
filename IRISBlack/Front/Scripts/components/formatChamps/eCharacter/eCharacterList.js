import { eCharacterMixin } from './eCharacterMixin.js?ver=803000'

export default {
    name: "eCharacterList",
    data() {
        return {};
    },
    mounted() { },
    methods: {},
    props: [],
    mixins: [eCharacterMixin],
    template: `

	<!-- LISTE -->
	<div v-if="propListe" v-bind:class="[propListe ? 'listRubriqueCaractere' : '', 'ellips input-group hover-input']"
		:title="!dataInput.ToolTipText ? dataInput.DisplayValue : dataInput.ToolTipText">

		<!-- Si le champ eCharactere est modifiable -->
		<input spellcheck="false" :ref="'field'+dataInput.DescId" :field="'field'+dataInput.DescId" :IsDetail="propDetail" :IsAssistant="propAssistant" :IsHead="propHead" :IsList="propListe" v-if="!dataInput.ReadOnly" 
			v-on:blur="verifChar($event,that)" :class="'class_liste_rubrique_caractere_' + propSignet.DescId + '_' + propIndexRow + '_' + dataInput.DescId" v-bind:disabled="dataInput.ReadOnly"
			v-on:click="this.bDisplayPopup = false" v-bind:style="{ color: dataInput.ValueColor}"  :value="dataInput.Value" type="text" class="form-control input-line fname"
			:placeholder="dataInput.Watermark">
	   
		<!-- Si le champ eCharactere est pas modifiable --> 
		<div v-bind:style="{ color: dataInput.ValueColor}" v-if="dataInput.ReadOnly" class="NoModifSimple ellipsDiv form-control input-line fname">
			<div class="targetIsTrue">{{dataInput.DisplayValue ? dataInput.DisplayValue : dataInput.Value }}</div> 
		</div>

		<!-- Icon -->
		<span v-on:click="focusInput('caractere', {
		props: propAssistant ? PropType.Assistant : propDetail ? PropType.Detail : propListe ? PropType.Liste : PropType.Defaut,
		propAssistantNbIndex: propAssistantNbIndex,
		propIndexRow: propIndexRow,
		dataInput: dataInput,
		propSignet: propSignet
	})" v-if="!dataInput.ReadOnly" class="input-group-addon"><a  href="#!" class="hover-pen"><i class="fas fa-pencil-alt"></i></a></span>
	</div>
`
};