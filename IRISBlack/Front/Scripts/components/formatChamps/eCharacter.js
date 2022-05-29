import EventBus from '../../bus/event-bus.js?ver=803000';
import { updateMethod, verifComponent, focusInput, RemoveBorderSuccessError, verifCharacter, showTooltip, hideTooltip, updateListVal, showInformationIco, displayInformationIco } from '../../methods/eComponentsMethods.js?ver=803000';
import { PropType, FieldType } from '../../methods/Enum.min.js?ver=803000';
import eAxiosHelper from "../../helpers/eAxiosHelper.js?ver=803000";
import { eMotherClassMixin } from '../../mixins/eMotherClassMixin.js?ver=803000';
import { eFileComponentsMixin } from '../../mixins/eFileComponentsMixin.js?ver=803000';

export default {
    name: "eCharacter",
    data() {
        return {
            catalogDialog: null,
            PropType,
            FieldType,
            bEmptyDisplayPopup: false,
            focusIn: false,
            oldValue: null,
            modified: false,
            icon: false,
            dataInputValue: ''
        };
    },
    created(){
        this.dataInputValue = this.dataInput?.Value;
    },
    mounted() {
        this.setContextMenu();

        this.displayInformationIco();

        this.oldValue = this.dataInput.Value;

        Vue.nextTick(() => {
            if (this.dataInput.Required && this.dataInput.Value === "")
                verifCharacter(this.dataInput, this)
        });
    },
    updated() {
        if (this.modified) {
            this.oldValue = this.dataInput.Value;
            this.modified = false;
        }
    },
    components: {
        eAlertBox: () => import(AddUrlTimeStampJS("../modale/alertBox.js"))
    },
    mixins: [eMotherClassMixin, eFileComponentsMixin],
    methods: {
        showInformationIco,
        displayInformationIco,
        updateMethod,        
        eAxiosHelper,
        showTooltip,
        hideTooltip,
        focusInput,
        RemoveBorderSuccessError,
        updateListVal,
        verifComponent,
        verifCharacter,
        onUpdateCallback() {
            let options = {
                reloadSignet: false,
                reloadHead: false,
                reloadAssistant: false,
                reloadAll: true
            }
            if (this.dataInput.IsInRules)
                EventBus.$emit('emitLoadAll', options);
        },
        verifChar(event) {        	   	
            verifComponent(undefined, event, this.oldValue, this, this.dataInput);
        },
        adjustInputSize() {
            event.target.parentNode.dataset.value = event.target.value;

        }
    },
    computed: {
        IsDisplayReadOnly: function () {
            return (this.dataInput.ReadOnly)
        },
        characterFieldCssClass() {
            return this.propHead ? 'class_liste_rubrique_caractere_header_' + this.dataInput.DescId : this.propDetail ? 'class_liste_rubrique_caractere_detail_' + this.dataInput.DescId : this.propAssistant ? 'class_liste_rubrique_caractere_assistant_' + this.propAssistantNbIndex + '_' + this.dataInput.DescId : '';
        },
        /** Récupère ou remplace la valeur si on coche le composant et met à jour le back */
        getInputValue:{
            get:function(){
                return this.dataInput.Value;
            },
            set:function(val){
                let finalDtVal = { ...this.dataInput, ...{ Value: val } }
                this.$emit('update:data-input', finalDtVal)
            }
        }
    },
    props: ["dataInput", "propHead", "propListe", "propSignet", "propIndexRow", "propAssistant", "propDetail", "propAssistantNbIndex"],
    template: `


<div class="globalDivComponent">

	<!-- FICHE -->
	<div ref="character" 
        v-on:mouseout="showTooltip(false,'character',icon,IsDisplayReadOnly,dataInput)" 
        v-on:mouseover="showTooltip(true,'character',icon,IsDisplayReadOnly,dataInput)" 
        v-if="!propListe" 
        v-bind:class="[IsDisplayReadOnly?'read-only':'', focusIn ? 'focusIn' : '' , 'ellips input-group hover-input', bEmptyDisplayPopup ? 'display-alert' : '']">
   
        <!-- <span ref="info" v-if="showInformationIco()" class="icon-info-circle info-tooltip"></span>  -->

        <!-- Si le champ eCharacter est modifiable et dans la zone résumé -->
        <label :data-value="dataInput.Value" v-on:blur="focusIn = false" v-if="propHead && !dataInput.ReadOnly" :class="[getCaseFormat,'input-sizer']">
            <input spellcheck="false" 
                type="text" 
                @input="adjustInputSize()" 
                size="4" 
                v-model="getInputValue"
                :ref="'field'+dataInput.DescId" 
                :field="'field'+dataInput.DescId" 
		        v-on:blur="verifChar($event);" 
                :IsDetail="propDetail" 
                :IsAssistant="propAssistant" 
                :IsHead="propHead"
                :IsList="propListe"
                :class="[characterFieldCssClass, getCaseFormat,'form-control input-line fname']" 
                :style="{ color: dataInput.ValueColor}" 
			    :placeholder="dataInput.Watermark"
			 >
        </label>

		<!-- Si le champ eCharactere est modifiable -->
		<input 
            v-if="!(dataInput.ReadOnly) && !propHead" 
		    spellcheck="false" 
            :ref="'field'+dataInput.DescId" 
            :field="'field'+dataInput.DescId" 
		    v-on:blur="verifChar($event); focusIn = false" 
            :IsDetail="propDetail" 
            :IsAssistant="propAssistant" 
            :IsHead="propHead"
		    :IsList="propListe" 
            v-bind:class="[
				propHead ? 'class_liste_rubrique_caractere_header_' + dataInput.DescId  : '', 
				propDetail ? 'class_liste_rubrique_caractere_detail_' + dataInput.DescId  : '', 
				propAssistant ? 'class_liste_rubrique_caractere_assistant_' + propAssistantNbIndex + '_' + dataInput.DescId  : '', 
				getCaseFormat,  
				'form-control input-line fname'
            ]" 
            v-bind:style="{ color: dataInput.ValueColor}" 
		    type="text" 
            :placeholder="dataInput.Watermark"
            v-model="getInputValue" 
         />

		<!-- Si le champ eCharactere n'est pas modifiable -->
		<span v-bind:style="{ color: dataInput.ValueColor}" v-if="dataInput.ReadOnly" class="readOnly">{{dataInput.Value}}</span>

		<!-- Icon -->
		<span 
			v-on:click="focusInput(
			'caractere', { 
				props: propAssistant ? PropType.Assistant : propDetail ? PropType.Detail : propListe ? PropType.Liste : propHead ? PropType.Head : PropType.Defaut,
				propAssistantNbIndex: propAssistantNbIndex,
				propIndexRow: propIndexRow,
				dataInput: dataInput,
				propSignet: propSignet
			});"   
			class="input-group-addon"
	    >
	        <a  href="#!" class="hover-pen"><i :class="[IsDisplayReadOnly?'mdi mdi-lock':'fas fa-pencil-alt']"></i></a>
	    </span>

		
		<!-- Message d'erreur après la saisie dans le champs -->
		<eAlertBox v-if="this.bEmptyDisplayPopup && !(dataInput.ReadOnly)" >
			<p>{{getRes(2471)}}</p>
		</eAlertBox>
	</div>

	<!-- LISTE -->
	<div ref="character" v-on:mouseout="showTooltip(false,'character',icon,IsDisplayReadOnly,dataInput)" v-on:mouseover="showTooltip(true,'character',icon,IsDisplayReadOnly,dataInput)" v-if="propListe" v-bind:class="[IsDisplayReadOnly ? 'read-only' : '',  propListe ? 'listRubriqueCaractere' : '', 'ellips input-group hover-input',focusIn ? 'focusIn' : '']">

		<!-- Si le champ eCharactere est modifiable -->
		<input 
		    v-if="!dataInput.ReadOnly" 
		    spellcheck="false" 
            :ref="'field'+dataInput.DescId" 
            v-model="getInputValue" 
            :field="'field'+dataInput.DescId" 
            :IsDetail="propDetail" 
            :IsAssistant="propAssistant" 
            :IsHead="propHead" 
            :IsList="propListe" 
			v-on:blur="verifChar($event);focusIn = false" 
            @focus="focusIn = true" 
            :class="['class_liste_rubrique_caractere_' + propSignet.DescId + '_' + propIndexRow + '_' + dataInput.DescId, getCaseFormat]" 
            v-bind:disabled="dataInput.ReadOnly"
            v-bind:style="{ color: dataInput.ValueColor}" 
            type="text" 
            class="form-control input-line fname"
			:placeholder="dataInput.Watermark"
	    />
	   
		<!-- Si le champ eCharactere est pas modifiable --> 
		<div v-bind:style="{ color: dataInput.ValueColor}" v-if="dataInput.ReadOnly" class="NoModifSimple ellipsDiv form-control input-line fname">
			<div class="targetIsTrue">{{dataInput.DisplayValue ? dataInput.DisplayValue : dataInput.Value }}</div> 
		</div>


<!-- Icon -->
		<span 
			v-on:click="!IsDisplayReadOnly ? focusIn = true : focusIn = false , !IsDisplayReadOnly ? focusInput('caractere', {
				props: propAssistant ? PropType.Assistant : propDetail ? PropType.Detail : propListe ? PropType.Liste : PropType.Defaut,
				propAssistantNbIndex: propAssistantNbIndex,
				propIndexRow: propIndexRow,
				dataInput: dataInput,
				propSignet: propSignet
			}) : '' " 
	        class="input-group-addon"
	    >
	        <a  href="#!" class="hover-pen"><i :class="[IsDisplayReadOnly?'mdi mdi-lock':'fas fa-pencil-alt']"></i></a>
	    </span>

	</div>
</div>
`
};