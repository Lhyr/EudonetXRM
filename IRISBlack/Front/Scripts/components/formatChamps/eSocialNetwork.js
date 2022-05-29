import EventBus from '../../bus/event-bus.js?ver=803000';
import { updateMethod, focusInput, RemoveBorderSuccessError, verifComponent, verifCharacter, showTooltip, hideTooltip, updateListVal, showInformationIco, displayInformationIco } from '../../methods/eComponentsMethods.js?ver=803000';
import { onUpdateCallback } from '../../methods/eFieldEditorMethods.js?ver=803000';
import { PropType } from '../../methods/Enum.min.js?ver=803000';
import { eFileComponentsMixin } from '../../mixins/eFileComponentsMixin.js?ver=803000';


export default {
    name: "eSocialNetWork",
    data() {
        return {
            modif : false,
            //IsDisplayReadOnly: (this.propHead || this.dataInput.ReadOnly),
            bDisplayPopup: false,
            bEmptyDisplayPopup: false,
            that: this,
            icon: false,
            focusIn: false,
            msgHover: '',
            dataInputValue: "",
            rootURLPrefix:"//"
        };
    },
    computed: {
        IsDisplayReadOnly: function () {
            return (this.dataInput.ReadOnly)
        },
        fieldValue() {
            return this.dataInput.Value && this.dataInput.Value != '' ? this.dataInput.Value : this.dataInput.Watermark ? this.dataInput.Watermark : '';
        },
        /** Récupère ou remplace la valeur si on coche le composant et met à jour le back */
        getInputValue:{
            get:function(){
                return this.dataInput.Value;
            },
            set:function(val){
                let finalDtVal = {...this.dataInput,...{Value:val}}
                this.$emit('update:data-input',finalDtVal)
            }
        },
        /** Récupère la valeur du lien */
        getHrefValue(){
            let inputValue = this.getInputValue.includes(this.rootURLPrefix) ? this.getInputValue : this.rootURLPrefix + this.getInputValue;
            return !!this.dataInput.RootURL ? this.dataInput.RootURL + this.getInputValue : inputValue;
        }
    },
    mixins: [eFileComponentsMixin],
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
        showInformationIco,
        displayInformationIco,
        updateListVal,
        hideTooltip,
        showTooltip,
        updateMethod,
        RemoveBorderSuccessError,
        verifCharacter,
        verifComponent,
        focusInput,
        onUpdateCallback,
        // permet de focus le input quand on click sur le crayon

        goAction(evt) {
            if (!evt.target.classList.contains("targetIsTrue")) {
                this.modif = true;
                focusInput("socialNetWork", {
                    props: this.propAssistant ? PropType.Assistant : this.propDetail ? PropType.Detail : this.propListe ? PropType.Liste : this.propHead ? PropType.Head : PropType.Defaut,
                    propAssistantNbIndex: this.propAssistantNbIndex,
                    propIndexRow: this.propIndexRow,
                    dataInput: this.dataInput,
                    propSignet: this.propSignet
                });
            }
        },
        verifSocial(event, that) {
            verifComponent(undefined, event, this.dataInputValue, that, this.dataInput);
        },
        measureTextWidth(event) {
            event.target.parentNode.dataset.value = event.target.value != '' ? event.target.value : this.dataInput.Watermark;
        }
    },
    props: ["dataInput", "propHead", "propListe", "propSignet", "propIndexRow", "propAssistant", "propDetail", "propAssistantNbIndex", "propResumeEdit"],
    template: `
<div class="globalDivComponent">
    <!-- FICHE -->
    <div
        v-if="!propListe"
        ref="social"
        @mouseout="showTooltip(false,'social',icon,IsDisplayReadOnly,dataInput)"
        @mouseover="showTooltip(true,'social',icon,IsDisplayReadOnly,dataInput)"
        v-bind:class="[
            IsDisplayReadOnly? 'headReadOnly read-only' : '',
            focusIn ? 'focusIn' : '',
            'ellips input-group hover-input'
        ]"
    >

        <!-- Si le champ socialNetWork est modifiable  -->
        <input
            v-if="!IsDisplayReadOnly && (this.modif  || this.getInputValue == '') && !propHead"
            :size="getInputValue.length + 30"
            :field="'field'+dataInput.DescId"
            @focus="bDisplayPopup = false; RemoveBorderSuccessError();bEmptyDisplayPopup = false; focusIn = true;hideTooltip(true);"
            @blur="verifSocial($event, that); focusIn = false;hideTooltip(false);"
            :IsDetail="propDetail"
            :IsAssistant="propAssistant"
            :IsHead="propHead"
            :IsList="propListe"
            :class="[
                propDetail ? 'class_liste_rubrique_socialNetWork_detail_' + dataInput.DescId  : '',
                propAssistant ? 'class_liste_rubrique_socialNetWork_assistant_' + propAssistantNbIndex + '_' + dataInput.DescId  : '',
                'form-control input-line fname'
            ]"
            :style="{ color: dataInput.ValueColor}"
            v-model="getInputValue" 
            type="text"
            :placeholder="dataInput.Watermark"
        />

        <label 
            v-if="!IsDisplayReadOnly && this.modif && propHead && dataInput != ''"
            :data-value="fieldValue" 
            :class="[getCaseFormat,'input-sizer']"
        >
            <input
                :IsDetail="propDetail" 
                spellcheck="false" 
                :IsAssistant="propAssistant" 
                :IsHead="propHead" 
                :IsList="propListe" 
                :class="[
                    propDetail ? 'form-control input-line fname class_liste_rubrique_socialNetWork_detail_' + dataInput.DescId  : '', 
                    propAssistant ? 'class_liste_rubrique_socialNetWork_assistant_' + propAssistantNbIndex + '_' + dataInput.DescId  : propHead ? 'class_liste_rubrique_socialNetWork_header_' + dataInput.DescId : '',
                ]"
                :style="{ color: dataInput.ValueColor}"
                @blur="verifSocial($event, that); focusIn = false;hideTooltip(false);"
                @focus="bDisplayPopup = false; RemoveBorderSuccessError();bEmptyDisplayPopup = false; focusIn = true;hideTooltip(true);"
                :field="'field'+dataInput.DescId" 
                type="text" 
                @input="measureTextWidth($event)" 
                size="1" 
                :placeholder="dataInput.Watermark" 
                v-model="getInputValue" 
            />
        </label>

        <span 
            v-if="!IsDisplayReadOnly && propHead && !this.modif && this.getInputValue == ''"
            class="field-placeholder" 
        >{{dataInput.Watermark}}</span>

        <div 
            v-if="!IsDisplayReadOnly && !this.modif && this.getInputValue != ''" 
            :style="[dataInput.ValueColor ? { color: dataInput.ValueColor} : {color: '#337ab7'}]"  
            type="text" 
            class="divInput link-container ellipsDiv form-control input-line fname"
        >
            <a 
                v-show="getInputValue != ''" 
                :field="'field'+dataInput.DescId" 
                @mouseout="icon = false" 
                @mouseover="icon = true" 
                :href="getHrefValue" 
                target="_blank" 
                class="targetIsTrue  text-truncate" 
                :style="{ color: dataInput.ValueColor}"
            >{{ getInputValue }}</a>
            <div 
                @click.self="goAction($event)" 
                class="click-area"
            ></div>
        </div>

        <!-- Si le champ socialNetWork n'est pas modifiable -->
        <a 
            v-if="IsDisplayReadOnly"
            @mouseout="icon = false" 
            @mouseover="icon = true" 
            :href="getHrefValue" 
            target="_blank"  
            :style="{ color: dataInput.ValueColor}" 
            @click="goAction($event)" 
            class="targetIsTrue linkHead readOnly text-truncate"
        >{{ getInputValue }}</a>
         
        <!-- Icon -->
        <span 
            @click="goAction($event)" 
            class="input-group-addon"
        >
            <a href="#!" class="hover-pen">
                <i :class="[
                    (IsDisplayReadOnly && !icon)?'mdi mdi-lock'
                    :(IsDisplayReadOnly && icon)?'fas fa-external-link-alt'
                    :(!IsDisplayReadOnly && icon)?'fas fa-external-link-alt'
                    :'fas fa-pencil-alt']">
                </i>
            </a>
        </span>
       
   	    <!-- Message d'erreur après la saisie dans le champs -->
        <eAlertBox  
            v-if="this.bEmptyDisplayPopup && !(dataInput.ReadOnly)" 
        >
            <p>{{getRes(2471)}}</p>
        </eAlertBox>

    </div>

    <!-- LISTE -->

    <div 
        v-if="propListe"
        ref="social" 
        v-on:mouseout="showTooltip(false,'social',icon,IsDisplayReadOnly,dataInput)" 
        v-on:mouseover="showTooltip(true,'social',icon,IsDisplayReadOnly,dataInput)"  
        v-bind:class="[
            IsDisplayReadOnly ? 'read-only' : '', propListe ? 'listRubriqueRelation' : '', 
            'ellips input-group hover-input',focusIn ? 'focusIn' : ''
        ]"
    >

        <!-- Si le champ socialNetWork est modifiable -->
        <input 
            v-if="!IsDisplayReadOnly && this.modif"
            :field="'field'+dataInput.DescId"  
            :IsDetail="propDetail" 
            :IsAssistant="propAssistant" 
            :IsHead="propHead" 
            :IsList="propListe"  
            :class="'class_liste_rubrique_socialNetWork_' + propSignet.DescId + '_' + propIndexRow + '_' + dataInput.DescId" 
            :disabled="dataInput.ReadOnly" 
            @blur="verifSocial($event, that);focusIn = false" 
            :style="{ color: dataInput.ValueColor}"  
            v-model="getInputValue" 
            type="text" 
            class="ellipsDiv form-control input-line fname"
            :placeholder="dataInput.Watermark"
        />
        
        <div 
            v-if="!IsDisplayReadOnly && !this.modif"
            v-on:click="goAction($event)"   
            type="text" 
            class="ellipsDiv form-control input-line fname"
        >
            <a 
                :field="'field'+dataInput.DescId" 
                :href="getHrefValue" 
                target="_blank" 
                class="targetIsTrue" 
                :style="{ color: dataInput.ValueColor}"
            >{{ getInputValue }}</a>
        </div>

        <!-- Si le champ socialNetWork n'est pas modifiable -->
        <a  
            v-if="IsDisplayReadOnly" 
            :href="getHrefValue" 
            target="_blank" 
            :style="{ color: dataInput.ValueColor}" 
            @click="goAction($event)" 
            class="targetIsTrue linkHead readOnly"
        >{{ getInputValue }}</a>
         

        <!-- Icon -->
        <span 
            v-on:click="!IsDisplayReadOnly ? [goAction($event),focusIn = true] : focusIn = false " 
            class="input-group-addon"
        >
            <a href="#!" class="hover-pen">
                <i :class="[IsDisplayReadOnly?'mdi mdi-lock':'fas fa-pencil-alt']"></i>
            </a>
        </span>
       
       
    </div>

</div>
`
};