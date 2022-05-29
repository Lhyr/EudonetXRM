import EventBus from '../../bus/event-bus.js?ver=803000';
import { sizeForm, focusInput, RemoveBorderSuccessError, verifComponent, verifCharacter, showTooltip, hideTooltip, updateListVal, showInformationIco, displayInformationIco } from '../../methods/eComponentsMethods.js?ver=803000';
import { onUpdateCallback } from '../../methods/eFieldEditorMethods.js?ver=803000';
import { PropType } from '../../methods/Enum.min.js?ver=803000';
import { eFileComponentsMixin } from '../../mixins/eFileComponentsMixin.js?ver=803000';

export default {
    name: "eHyperLink",
    data() {
        return {
            modif: false,
            //IsDisplayReadOnly: (this.propHead || this.dataInput.ReadOnly),
            that: this,
            bEmptyDisplayPopup: false,
            bDisplayPopup: false,
            icon: false,
            focusIn: false,
            msgHover:'',
            dataInputValue: ""
        };
    },
    computed: {
        IsDisplayReadOnly: function () {
            return (this.dataInput.ReadOnly)
        },
        /** Récupère ou remplace la valeur si on coche le composant et met à jour le back */
        getInputValue:{
            get:function(){
                return this.dataInputValue;
            },
            set:function(val){
                this.dataInputValue = val;
            }
        }
    },
    created(){
        this.dataInputValue = this.dataInput?.Value;
    },
    mounted() {
        this.setContextMenu();
        this.displayInformationIco();
        Vue.nextTick(() => {
            if (this.dataInput.Required && this.dataInput.Value === "")
                verifCharacter(this.dataInput, this.that)
        });
    },
    components: {
        eAlertBox: () => import(AddUrlTimeStampJS("../modale/alertBox.js"))
    },
    methods: {
        sizeForm,
        showInformationIco,
        displayInformationIco,
        showTooltip,
        updateListVal,
        hideTooltip,
        onUpdateCallback,
        verifComponent,
        // permet de focus le input quand on click sur le crayon

        goAction(evt) {
            if (!evt.target.classList.contains("targetIsTrue")) {
                this.modif = true
                focusInput('hyperLink', {
                    props: this.propAssistant ? PropType.Assistant : this.propDetail ? PropType.Detail : this.propListe ? PropType.Liste : this.propHead ? PropType.Head : PropType.Defaut,
		            propAssistantNbIndex: this.propAssistantNbIndex,
                    propIndexRow: this.propIndexRow,
                    dataInput: this.dataInput,
                    propSignet: this.propSignet
                });
            }
        },
        RemoveBorderSuccessError,
        verifHyperlinks(event, that) {
            verifComponent(undefined, event, this.dataInput?.Value, that, this.dataInput);
        }
    },
    props: ["dataInput", "propHead", "propListe", "propSignet", "propIndexRow", "propAssistant", "propDetail", "propAssistantNbIndex", "propResumeEdit"],
    mixins: [eFileComponentsMixin],
    template: `
<div class="globalDivComponent">
    <!-- FICHE -->
    <div
        v-if="!propListe"
        ref="link"
        v-on:mouseout="showTooltip(false,'link',icon,IsDisplayReadOnly,dataInput)"
        v-on:mouseover="showTooltip(true,'link',icon,IsDisplayReadOnly,dataInput)"
        v-bind:class="[
            IsDisplayReadOnly? 'headReadOnly read-only' : '',
            focusIn ? 'focusIn' : '',
            'ellips input-group hover-input'
        ]"
        :title="!dataInput.ToolTipText ? dataInput.DisplayValue : dataInput.ToolTipText"
    >

        <!-- Si le champ hyperLink et est modifiable -->
        <input 
            v-if="!IsDisplayReadOnly && this.modif"
            :size="sizeForm(getInputValue)" 
            :field="'field'+dataInput.DescId"  
            :IsDetail="propDetail" 
            :IsAssistant="propAssistant" 
            :IsHead="propHead" 
            :IsList="propListe" 
            v-bind:class="[
                propHead ? 'class_liste_rubrique_hyperLink_header_' + dataInput.DescId  : '', 
                propDetail ? 'class_liste_rubrique_hyperLink_detail_' + dataInput.DescId  : '',
                propAssistant ? 'class_liste_rubrique_hyperLink_assistant_' + propAssistantNbIndex + '_' + dataInput.DescId  : '', 
                'form-control input-line fname'
            ]"
            v-on:focus="focusIn = true; bDisplayPopup = false; RemoveBorderSuccessError();bEmptyDisplayPopup = false;hideTooltip(true);" 
            v-on:blur="verifHyperlinks($event, that); focusIn = false; hideTooltip(false)" 
            v-bind:style="{ color: dataInput.ValueColor}"  
            v-model="dataInputValue" 
            type="text"
            :placeholder="dataInput.Watermark"
        />
        <div 
            v-if="!IsDisplayReadOnly && !this.modif" 
            class="divInput link-container ellipsDiv form-control input-line fname"
        >
            <a 
                v-show="getInputValue != ''" :field="'field'+dataInput.DescId" 
                v-on:mouseout="icon = false;" 
                v-on:mouseover="icon = true;" 
                :href="getInputValue" 
                target="_blank" 
                class="targetIsTrue" 
                v-bind:style="{ color: dataInput.ValueColor}"
            >
                {{ getInputValue }}
            </a>
            <div v-on:click.self="goAction($event)" class="click-area"></div>
        </div>

        <!-- Si le champ hyperLink n'est pas modifiable -->
        <a 
            v-if="IsDisplayReadOnly"
            v-on:mouseout="icon = false" 
            v-on:mouseover="icon = true"  
            :href="getInputValue" 
            target="_blank"  
            v-bind:style="{ color: dataInput.ValueColor}" 
            v-on:click="goAction($event)" 
            class="targetIsTrue linkHead readOnly"
        >
            {{ getInputValue }}
        </a>
         
        <!-- Icon -->
        <span
            v-if="!modif"
            v-on:click="goAction($event)"
            :class="[icon?'icons-hidden':'','input-group-addon']"
        >
            <a href="#!" class="hover-pen">
                <i 
                    :class="[
                        (IsDisplayReadOnly && !icon)?'mdi mdi-lock'
                        :(IsDisplayReadOnly && icon)?'fas fa-external-link-alt'
                        :(!IsDisplayReadOnly && icon)?'fas fa-external-link-alt'
                        :'fas fa-pencil-alt'
                    ]"
                />
            </a>
        </span>
       
    	<!-- Message d'erreur si champs obligatoire est vide -->
        <eAlertBox v-if="dataInput.Required && this.bEmptyDisplayPopup && !(dataInput.ReadOnly)" >
            <p>{{getRes(2471)}}</p>
        </eAlertBox>


    </div>

    <!-- LISTE -->
    <div
        v-if="propListe"
        ref="link"
        v-on:mouseout="showTooltip(false,'link',icon,IsDisplayReadOnly,dataInput)"
        v-on:mouseover="showTooltip(true,'link',icon,IsDisplayReadOnly,dataInput)"
        v-bind:class="[
            propListe ? 'listRubriqueRelation' : '',
            'ellips input-group hover-input',
            focusIn ? 'focusIn' : ''
        ]"
    >

        <!-- Si le champ hyperLink et est modifiable -->
        <input 
            v-if="!IsDisplayReadOnly && this.modif"
            :field="'field' + dataInput.DescId"  
            :IsDetail="propDetail" 
            :IsAssistant="propAssistant" 
            :IsHead="propHead" 
            :IsList="propListe"  
            :class="'class_liste_rubrique_hyperLink_' + propSignet.DescId + '_' + propIndexRow + '_' + dataInput.DescId" 
            v-bind:disabled="dataInput.ReadOnly" 
            v-on:focus="bDisplayPopup = false; RemoveBorderSuccessError();bEmptyDisplayPopup = false; "
            v-on:blur="verifHyperlinks($event, that); focusIn = false"
            v-bind:style="{ color: dataInput.ValueColor}"  
            v-model="dataInputValue" 
            type="text" 
            class="ellipsDiv form-control input-line fname"
            :placeholder="dataInput.Watermark"
        >
        
        <div 
            v-if="!IsDisplayReadOnly && !this.modif"
            v-on:click="goAction($event)"  
            class="divInput ellipsDiv form-control input-line fname"
        >
            <a 
                :field="'field'+dataInput.DescId" 
                :href="getInputValue" 
                target="_blank" 
                class="targetIsTrue" 
                v-bind:style="{ color: dataInput.ValueColor}"
            >
                {{ getInputValue }}
            </a>
        </div>

        <!-- Si le champ hyperLink n'est pas modifiable -->
        <a 
            v-if="IsDisplayReadOnly"
            :href="getInputValue" 
            target="_blank"  
            v-bind:style="{ color: dataInput.ValueColor}" 
            v-on:click="goAction($event)" 
            class="targetIsTrue linkHead readOnly"
        >
            {{ getInputValue }}
        </a>
         
        <!-- Icon -->
        <span
            v-if="!IsDisplayReadOnly"
            v-on:click="goAction($event);focusIn = true"
            class="input-group-addon"
        >
            <a
                href="#!"
                class="hover-pen"
            >
                <i
                    class="fas fa-pencil-alt"
                />
            </a>
        </span>
       
    </div>

</div>
`
};