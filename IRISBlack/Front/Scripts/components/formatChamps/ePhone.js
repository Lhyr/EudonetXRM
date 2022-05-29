import EventBus from '../../bus/event-bus.js?ver=803000';
import { focusInput, RemoveBorderSuccessError, verifComponent, verifCharacter, showTooltip, hideTooltip, updateMethod, updateListVal, showInformationIco, displayInformationIco } from '../../methods/eComponentsMethods.js?ver=803000';
import { onUpdateCallback } from '../../methods/eFieldEditorMethods.js?ver=803000';
import { PropType } from '../../methods/Enum.min.js?ver=803000';
import { eFileComponentsMixin } from '../../mixins/eFileComponentsMixin.js?ver=803000';
import containerModal from '../modale/containerModal.js?ver=803000';
import { store } from '../../../Scripts/store/store.js?ver=803000';

export default {
    name: "ePhone",
    data() {
        return {
            PropType,
            messageError: this.getRes(2246),
            messageSuccess: "",
            modif: false,
            bRegExeSuccess: true,
            bDisplayPopup: false,
            //IsDisplayReadOnly: (this.propHead || this.dataInput.ReadOnly),
            bEmptyDisplayPopup: false,
            that: this,
            patternVerif: /^[\-.+()# 0-9]*$/i,
            icon: false,
            focusIn: false,
            msgHover: '',
            instance: null,
            that: this,
            sOldValue:null
        };
    },
    created(){
        this.sOldValue = this.dataInput.Value
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
    computed: {
        IsDisplayReadOnly: function () {
            return (this.dataInput.ReadOnly)
        },
        iconCssClass() {
            let icon;

            if (this.dataInput.ReadOnly && !this.icon)
                icon = 'mdi mdi-lock'
            else if (this.dataInput.ReadOnly && this.icon)
                icon = 'fas fa-phone'
            else if (!this.dataInput.ReadOnly && this.icon)
                icon = 'fas fa-phone'
            else
                icon = 'fas fa-pencil-alt'
            return icon;
        },
        getInputValue:{
            get:function(){
                return this.dataInput.Value;
            },
            set:function(val){
                let finalDtVal = {...this.dataInput,...{Value:val}}
                this.$emit('update:data-input',finalDtVal)
            }
        }
    },
    mixins: [eFileComponentsMixin],
    methods: {
        displayInformationIco,
        showInformationIco,
        updateListVal,
        updateMethod,
        hideTooltip,
        showTooltip,
        focusInput,
        verifCharacter,
        onUpdateCallback,
        verifComponent,
        // permet de focus le input quand on click sur le crayon
        goAction(evt) {
            if (!evt.target.classList.contains("targetIsTrue") || this.dataInput.Value == "") {
                this.modif = true;
                focusInput("phone", {
                    props: this.propAssistant ? PropType.Assistant : this.propDetail ? PropType.Detail : this.propListe ? PropType.Liste : this.propHead ? PropType.Head : PropType.Defaut,
                    propAssistantNbIndex: this.propAssistantNbIndex,
                    propIndexRow: this.propIndexRow,
                    dataInput: this.dataInput,
                    propSignet: this.propSignet
                });
            } else {
                this.openSmsForm();
            }
        },
        verifPhone(event, that) {
            verifComponent(this.patternVerif, event, this.sOldValue, that, this.dataInput);
        },
        RemoveBorderSuccessError
    },
    props: [
        "dataInput",
        "propHead",
        "propListe",
        "propSignet",
        "propIndexRow",
        "propAssistant",
        "propDetail",
        "propAssistantNbIndex",
        "propResumeEdit"
    ],
    template: `
<div class="globalDivComponent">
    <!-- FICHE -->
    <div ref="phone" @mouseout="showTooltip(false,'phone',icon,IsDisplayReadOnly,dataInput)" @mouseover="showTooltip(true,'phone',icon,IsDisplayReadOnly,dataInput)" v-if="!propListe" :class="[focusIn ? 'focusIn' : '', IsDisplayReadOnly ? 'headReadOnly read-only' : '', 'ellips input-group hover-input']">

            <!-- Si l'extension phone est activé -->
            <!-- Si le champ phone et est modifiable -->

            <!-- <span ref="info" v-if="showInformationIco()" class="icon-info-circle info-tooltip"></span> -->

            <input :size="dataInput.Value.length + 30" :field="'field'+dataInput.DescId"
                @focus="focusIn = true; bDisplayPopup = false; RemoveBorderSuccessError();bEmptyDisplayPopup = false;hideTooltip(true);" 
                @blur="verifPhone($event, that); focusIn = false;hideTooltip(false);" 
                v-if="!IsDisplayReadOnly && (this.modif  || this.dataInput.Value == '') && !propHead" 
                :IsDetail="propDetail" 
                :IsAssistant="propAssistant" 
                :IsHead="propHead" 
                :IsList="propListe" 
                :class="[propHead ? 'class_liste_rubrique_phone_header_' + dataInput.DescId  : '', propDetail ? 'class_liste_rubrique_phone_detail_' + dataInput.DescId  : '', propAssistant ? 'class_liste_rubrique_phone_assistant_' + propAssistantNbIndex + '_' + dataInput.DescId  : '', 'form-control input-line fname']" :style="{ color: dataInput.ValueColor}"  
                v-model="getInputValue" type="tel">


            <label :data-value="dataInput.Value" v-if="!IsDisplayReadOnly && this.modif && propHead" :class="[getCaseFormat,'input-sizer']">
             <input :field="'field'+dataInput.DescId"
                @focus="focusIn = true; bDisplayPopup = false; RemoveBorderSuccessError();bEmptyDisplayPopup = false;hideTooltip(true);" 
                @blur="verifPhone($event, that); focusIn = false;hideTooltip(false);" 
                :IsDetail="propDetail" 
                :IsAssistant="propAssistant" 
                :IsHead="propHead" 
                :IsList="propListe" 
                :class="[propHead ? 'class_liste_rubrique_phone_header_' + dataInput.DescId  : '', propDetail ? 'class_liste_rubrique_phone_detail_' + dataInput.DescId  : '', propAssistant ? 'class_liste_rubrique_phone_assistant_' + propAssistantNbIndex + '_' + dataInput.DescId  : '', 'form-control input-line fname']" :style="{ color: dataInput.ValueColor}"  
                type="text" :onInput="'this.parentNode.dataset.value = this.value'" size="1" :placeholder="dataInput.Watermark" v-model="getInputValue">
            </label>


            <div v-if="!IsDisplayReadOnly && !this.modif" class="divInput ellipsDiv form-control input-line fname link-container">
                <a v-show="dataInput.Value != ''" @click="askForAnOption()" :field="'field'+dataInput.DescId" @mouseout="icon = false" @mouseover="icon = true" href="#!" class="targetIsTrue" :style="{ color: dataInput.ValueColor}">{{dataInput.Value}}</a>
                <div @click="goAction($event)" class="click-area"></div>
            </div>

            <!-- Si le champ phone n'est pas modifiable -->
            <a @mouseout="icon = false" @mouseover="icon = true" @click="askForAnOption()" v-if="IsDisplayReadOnly" :style="{ color: dataInput.ValueColor}" class="targetIsTrue linkHead readOnly">{{dataInput.Value}}</a>
         
            <!-- Icon -->
            <span v-if="dataInput.DisplaySmsBtn" @click="goAction($event)" class="input-group-addon"><a href="#!" class="hover-pen"><i :class="iconCssClass"></i></a></span>

            <eAlertBox v-show="this.bDisplayPopup" :b-success="this.bRegExeSuccess">
                <p v-if="!this.bRegExeSuccess">{{messageError}}</p>
            </eAlertBox>

	        <!-- Message d'erreur après la saisie dans le champs -->
            <eAlertBox v-if="this.bEmptyDisplayPopup && !(dataInput.ReadOnly)" >
                <p>{{getRes(2471)}}</p>
            </eAlertBox>
            

            <!-- Si l'extension phone est desactivé -->
            <!-- Si le champ phone est modifiable -->

            <!-- Icon -->
            <span @click="focusInput('phone', {
            props: propAssistant ? PropType.Assistant : propDetail ? PropType.Detail : propListe ? PropType.Liste : propHead ? PropType.Head : PropType.Defaut,
        propAssistantNbIndex: propAssistantNbIndex,
        propIndexRow: propIndexRow,
        dataInput: dataInput,
        propSignet: propSignet
                }), goAction($event)" v-if="!(dataInput.ReadOnly) && !dataInput.DisplaySmsBtn" class="input-group-addon"><a  href="#!" class="hover-pen"><i class="fas fa-pencil-alt"></i></a></span>
        
    </div>

    <!-- LISTE -->
    <div ref="phone" @mouseout="showTooltip(false,'phone',icon,IsDisplayReadOnly,dataInput)" @mouseover="showTooltip(true,'phone',icon,IsDisplayReadOnly,dataInput)" v-if="propListe" :class="[IsDisplayReadOnly ? 'read-only' : '', propListe ? 'listRubriqueRelation' : '', 'ellips input-group hover-input',focusIn ? 'focusIn' : '']">

        <!-- Si l'extension phone est activé -->
        <!-- Si le champ phone et est modifiable -->
        <input @blur="focusIn = false; verifPhone($event, that);" :field="'field'+dataInput.DescId" v-if="!IsDisplayReadOnly && this.modif && dataInput.DisplaySmsBtn" :IsDetail="propDetail" :IsAssistant="propAssistant" 
            :IsHead="propHead" :IsList="propListe"  :class="'class_liste_rubrique_phone_' + propSignet.DescId + '_' + propIndexRow + '_' + dataInput.DescId" :disabled="dataInput.ReadOnly" 
            :style="{ color: dataInput.ValueColor}"  :value="dataInput.Value" type="tel" class="ellipsDiv form-control input-line fname"
            :placeholder="dataInput.Watermark">
        
        <div @click="askForAnOption($event)" v-if="!IsDisplayReadOnly && !this.modif && dataInput.DisplaySmsBtn" class="divInput ellipsDiv form-control input-line fname">
            <a :field="'field'+dataInput.DescId" href="#!" class="targetIsTrue" :style="{ color: dataInput.ValueColor}">{{dataInput.Value}}</a>
        </div>

        <!-- Si le champ phone n'est pas modifiable -->
        <a href="#!" v-if="IsDisplayReadOnly && dataInput.DisplaySmsBtn" :style="{ color: dataInput.ValueColor}" @click="goAction($event)" class="targetIsTrue linkHead readOnly">{{dataInput.Value}}</a>
         
        <!-- Icon -->
        <span @click="goAction($event);focusIn = true" v-if="dataInput.DisplaySmsBtn" class="input-group-addon"><a href="#!" class="hover-pen"><i :class="iconCssClass"></i></a></span>
       

        <!-- Si l'extension phone est desactivé -->
        <!-- Si le champ phone est modifiable -->
        <input v-if="!dataInput.ReadOnly && !dataInput.DisplaySmsBtn" :IsDetail="propDetail" :IsAssistant="propAssistant" :IsHead="propHead" :IsList="propListe"  
            :class="'class_liste_rubrique_phone_' + propSignet.DescId + '_' + propIndexRow + '_' + dataInput.DescId" :disabled="dataInput.ReadOnly" 
            @blur="verifPhone($event, that);" :style="{ color: dataInput.ValueColor}"  
            :value="dataInput.Value" type="tel" 
            :placeholder="dataInput.Watermark" class="form-control input-line fname">
       
        <!-- Si le champ phone est modifiable -->        
        <div :style="{ color: dataInput.ValueColor}" v-if="dataInput.ReadOnly && !dataInput.DisplaySmsBtn"  type="text" class="ellipsDiv form-control input-line fname">
            {{dataInput.Value}}
        </div>

        <!-- Icon -->
        <span @click="focusInput('phone', {
                    props: propAssistant ? PropType.Assistant : propDetail ? PropType.Detail : propListe ? PropType.Liste : PropType.Defaut,
		            propAssistantNbIndex: propAssistantNbIndex,
                    propIndexRow: propIndexRow,
                    dataInput: dataInput,
                    propSignet: propSignet
                });focusIn = true" v-if="!dataInput.DisplaySmsBtn" class="input-group-addon"><a  href="#!" class="hover-pen"><i :class="iconCssClass"></i></a></span>

    </div>
</div>
`
};