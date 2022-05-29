import EventBus from '../../bus/event-bus.js?ver=803000';
import { focusInput, RemoveBorderSuccessError, verifComponent, verifCharacter, showTooltip, hideTooltip, showInformationIco, displayInformationIco } from '../../methods/eComponentsMethods.js?ver=803000';
import { onUpdateCallback } from '../../methods/eFieldEditorMethods.js?ver=803000';
import { PropType } from '../../methods/Enum.min.js?ver=803000';
import { eFileComponentsMixin } from '../../mixins/eFileComponentsMixin.js?ver=803000';

export default {
    name: "eMailAdress",
    data() {
        return {
            messageError: this.getRes(2453),
            messageSuccess: this.getRes(2462),
            modif: false,
            //IsDisplayReadOnly: (this.propHead || this.dataInput.ReadOnly),
            bRegExeSuccess: true,
            bDisplayPopup: false,
            bEmptyDisplayPopup: false,
            patternVerif: /^[a-zA-Z0-9.!#$%&'*+/=?^_\`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)+$/i,
            that: this,
            icon: false,
            focusIn: false,
            iconEmailVerification: [
                {
                    id: 0,
                    name: "UNCHECKED",
                    iconClass: 'fas fa-exclamation fa-stack-1x fa-inverse',
                    description: this.getRes(2828),
                    statuseudo: this.getRes(2821),
                    statuseudoTech: '',
                    statuseudoTechSub: ''
                },
                {
                    id: 1,
                    name: "VALID",
                    iconClass: 'fas fa-check fa-stack-1x fa-inverse',
                    description: this.getRes(2825),
                    statuseudo: this.getRes(2819),
                    statuseudoTech: '',
                    statuseudoTechSub: ''
                },
                {
                    id: 2,
                    name: "INVALID",
                    iconClass: 'fas fa-times-circle',
                    description: this.getRes(2826),
                    statuseudo: this.getRes(2820),
                    statuseudoTech: '',
                    statuseudoTechSub: ''
                },
                {
                    id: 3,
                    name: "UNKNOWN",
                    iconClass: 'fas fa-question-circle',
                    description: this.getRes(2827),
                    statuseudo: this.getRes(2822),
                    statuseudoTech: '',
                    statuseudoTechSub: ''
                },
                {
                    id: 4,
                    name: "VERIFICATION_IN_PROGRESS",
                    iconClass: 'fas fa-clock',
                    description: this.getRes(3017),
                    statuseudo: this.getRes(3016),
                    statuseudoTech: '',
                    statuseudoTechSub: ''
                }
            ],
            statusTechSubList: [
                {
                    id: 1,
                    name: this.getRes(2823)
                },
                {
                    id: 2,
                    name: this.getRes(2824)
                },
                {
                    id: 3,
                    name: this.getRes(2833)
                },
                {
                    id: 4,
                    name: this.getRes(2851)
                },
                {
                    id: 5,
                    name: this.getRes(2852)
                },
                {
                    id: 6,
                    name: this.getRes(2853)
                },
                {
                    id: 7,
                    name: this.getRes(2854)
                },
                {
                    id: 8,
                    name: this.getRes(2855)
                },
                {
                    id: 9,
                    name: this.getRes(2856)
                },
                {
                    id: 10,
                    name: this.getRes(2857)
                },
                {
                    id: 11,
                    name: this.getRes(2858)
                },
                {
                    id: 12,
                    name: this.getRes(2859)
                },
                {
                    id: 13,
                    name: this.getRes(2860)
                },
                {
                    id: 14,
                    name: this.getRes(2861)
                },
                {
                    id: 15,
                    name: this.getRes(2862)
                },
                {
                    id: 16,
                    name: this.getRes(2863)
                },
                {
                    id: 17,
                    name: this.getRes(2864)
                },
                {
                    id: 18,
                    name: this.getRes(2865)
                },
                {
                    id: 19,
                    name: this.getRes(2866)
                },
                {
                    id: 20,
                    name: this.getRes(2867)
                },
                {
                    id: 21,
                    name: this.getRes(2868)
                },
                {
                    id: 22,
                    name: this.getRes(2869)
                },
                {
                    id: 23,
                    name: this.getRes(2870)
                },
                {
                    id: 24,
                    name: this.getRes(2871)
                },
                {
                    id: 25,
                    name: this.getRes(2872)
                },
                {
                    id: 26,
                    name: this.getRes(2873)
                },
                {
                    id: 27,
                    name: this.getRes(2874)
                },
                {
                    id: 28,
                    name: this.getRes(2875)
                },
                {
                    id: 29,
                    name: this.getRes(2876)
                },
                {
                    id: 30,
                    name: this.getRes(2877)
                },
                {
                    id: 32,
                    name: this.getRes(2926)
                },
                {
                    id: 33,
                    name: this.getRes(2856)
                },
                {
                    id: 34,
                    name: this.getRes(2854)
                },
                {
                    id: 35,
                    name: this.getRes(2927)
                },
                {
                    id: 36,
                    name: this.getRes(2928)
                },
                {
                    id: 37,
                    name: this.getRes(2929)
                },
                {
                    id: 38,
                    name: this.getRes(2930)
                },
                {
                    id: 39,
                    name: this.getRes(2931)
                },
                {
                    id: 40,
                    name: this.getRes(2932)
                },
                {
                    id: 41,
                    name: this.getRes(2933)
                },
                {
                    id: 42,
                    name: this.getRes(2934)
                }
            ],
            statusVerificationItem: '',
            showOrHideTooltip: false,
            cssClass: ['unchecked-email-icon', 'checked-email-icon', 'invalid-email-icon', 'unknown-email-icon', 'verification-email-icon'],
            dataInputValue: ""
        };
    },
    components: {
        eAlertBox: () => import(AddUrlTimeStampJS("../modale/alertBox.js"))
    },
    mounted() {
        this.setContextMenu();
        this.displayInformationIco();
        Vue.nextTick(() => {
            if (this.dataInput.Required && this.dataInput.Value === "")
                verifCharacter(this.dataInput, this.that)
        });
    },
    computed: {
        IsDisplayReadOnly: function () {
            return (this.dataInput.ReadOnly)
        },

        msgHover: function () {
            if (this.icon)
                return this.getRes(6184)
            else if (!this.icon && this.IsDisplayReadOnly)
                return this.getRes(2477)
            else
                //return "Modifier l'adresse email"
                return this.getRes(7393)
        },
        getverifStatus() {
            this.statusVerificationItem = this.iconEmailVerification.find(status => status.id == this.dataInput.MailStatusEudo);

            if (!this.statusVerificationItem)
                return;

            this.statusVerificationItem.statuseudoTechSub = this.statusTechSubList.find(statusSubTech => statusSubTech.id == this.dataInput.MailStatusSubTech);
            return this.statusVerificationItem;
        },
        isNotValid() {
            return (this.getverifStatus?.name != "VALID" && this.getverifStatus?.name != "UNCHECKED");
        },
        /** Renvoie la classe de l'icône pour la vérification de l'email, pour gérer la couleur ou la taille de police */
        getStatusIconCssClass() {
            return this.cssClass[this.getverifStatus?.id];
        },
        isMouseOver() {
            return this.getverifStatus?.id == 0 || this.getverifStatus?.id == 1;
        },
        /** Récupère ou remplace la valeur si on coche le composant et met à jour le back */
        getInputValue:{
            get:function(){
                // this.dataInputValue = this.dataInput.Value;
                return this.dataInput.Value;
            },
            set:function(val){
                // this.dataInputValue = val;
                this.$emit('update:dataInput',{...this.dataInput,...{Value:val}})
            }
        }
    },
    created(){
        this.dataInputValue = this.dataInput?.Value;
    },
    mixins: [eFileComponentsMixin],
    methods: {
        displayInformationIco,
        showInformationIco,
        hideTooltip,
        showTooltip,
        RemoveBorderSuccessError,
        verifCharacter,
        onUpdateCallback,
        verifComponent,
        /**
         * Fonction qui va chainer les vérif en commencant par la pattern,
         * et en terminant par vide, obligatoire ou non. G.L 
         * @param  {any} event l'événement
         * @param  {any} that le contexte appelant.
         * */
        verifEmail(event, that) {
            verifComponent(this.patternVerif, event, this.dataInputValue, that, that.dataInput);
        },
        tooltipHover(visible) {

            this.showTooltip(visible, this.getverifStatus?.name, this.icon, this.IsDisplayReadOnly, null, null, this.getverifStatus);

        },
        tooltipClick() {
            let animation = {
                animationDelay: '0ms',
                animationDuration: '100ms'
            }
            if (!this.isMouseOver) {
                this.showOrHideTooltip = !this.showOrHideTooltip;
                this.showTooltip(this.showOrHideTooltip, this.getverifStatus?.name, this.icon, this.IsDisplayReadOnly, null, null, this.getverifStatus, animation);
            }
        }
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
<div class="globalDivComponent d-flex full-width">
    <!-- FICHE -->
    <div
        v-if="!propListe"
        ref="email" 
        v-on:mouseout="showTooltip(false,'email',icon,IsDisplayReadOnly,dataInput);showOrHideTooltip = false" 
        v-on:mouseover="showTooltip(true,'email',icon,IsDisplayReadOnly,dataInput)"  
        v-bind:class="[
            IsDisplayReadOnly? 'headReadOnly read-only' : '',
            focusIn ? 'focusIn' : '',
            'ellips input-group hover-input full-width'
        ]" 
    >

        <!-- Si le champ mailAdress est modifiable -->
        <input 
            v-if="!IsDisplayReadOnly && this.modif" 
            :IsDetail="propDetail" :IsAssistant="propAssistant"             
            :IsHead="propHead" 
            :IsList="propListe" 
            v-bind:class="[
                propHead ? 'class_liste_rubrique_mailAdress_header_' + dataInput.DescId  : '', 
                propDetail ? 'class_liste_rubrique_mailAdress_detail_' + dataInput.DescId  : '', 
                propAssistant ? 'class_liste_rubrique_mailAdress_assistant_' + propAssistantNbIndex + '_' + dataInput.DescId  : '', 
                'form-control input-line fname'
            ]"
            @blur="verifEmail($event,that); focusIn = false;hideTooltip(false)"
            @focus="bDisplayPopup = false; RemoveBorderSuccessError();bEmptyDisplayPopup = false; focusIn = true;hideTooltip(true)"
            :style="{ color: dataInput.ValueColor}"  
            v-model="getInputValue" 
            type="email"
            :placeholder="dataInput.Watermark"
        />


        <div 
            v-if="!IsDisplayReadOnly && !this.modif"
            v-bind:style="[
                dataInput.ValueColor ? { color: dataInput.ValueColor} : {color: '#337ab7'}
            ]"   
            class="divInput ellipsDiv form-control input-line fname link-container"
        >
            <a 
                v-show="getInputValue != ''" 
                @click.self="goEmailPopup($event)" 
                v-on:mouseout="icon = false" 
                v-on:mouseover="icon = true" 
                href="#!" 
                class="targetIsTrue" 
                v-bind:style="{ color: dataInput.ValueColor}"
            >{{ getInputValue }}</a>
            <div @click.self="goEmailPopup($event)" class="click-area"></div>
        </div>
        <!-- Si le champ mailAdress n'est pas modifiable -->
        <a 
            v-if="IsDisplayReadOnly" 
            v-on:mouseout="icon = false" 
            v-on:mouseover="icon = true" 
            href="#!"
            :style="{ color: dataInput.ValueColor}"
            @click="goEmailPopup($event)" 
            class="targetIsTrue linkHead readOnly"
        >{{ getInputValue }}</a>


        <!-- Icon -->
        <span 
            v-on:click="goEmailPopup($event)" 
            :class="[
                IsDisplayReadOnly?'read-only':'',
                'input-group-addon'
            ]"
        >
            <a href="#!" class="hover-pen">
                <i :class="[
                        (IsDisplayReadOnly && !icon)?'mdi mdi-lock'
                        :(IsDisplayReadOnly && icon)?'fas fa-envelope'
                        :(!IsDisplayReadOnly && icon)?'fas fa-envelope'
                        :'fas fa-pencil-alt'
                    ]"
                />
            </a>
        </span>

        <eAlertBox v-if="this.bDisplayPopup" :b-success="this.bRegExeSuccess">
            <p v-if="!this.bRegExeSuccess">{{this.messageError}}</p>
        </eAlertBox>

        <!-- Message d'erreur si champs obligatoire est vide -->
        <eAlertBox v-if="dataInput.Required && this.bEmptyDisplayPopup && !(dataInput.ReadOnly)" >
             <p>{{getRes(2471)}}</p>
        </eAlertBox>
     </div>    

    <!-- LISTE -->
    <div 
        v-if="propListe"
        ref="email" 
        v-on:mouseout="showTooltip(false,'email',icon,IsDisplayReadOnly,dataInput)" 
        v-on:mouseover="showTooltip(true,'email',icon,IsDisplayReadOnly,dataInput)"  
        v-bind:class="[
            IsDisplayReadOnly ? 'read-only' : '', 
            propListe ? 'listRubriqueRelation' : '', 
            'ellips input-group hover-input',
            focusIn ? 'focusIn' : ''
        ]"
    >

        <!-- Si le champ mailAdress et est modifiable -->
        <input 
            v-if="!IsDisplayReadOnly && this.modif"
            @blur="focusIn = false"  
            :IsDetail="propDetail" 
            :IsAssistant="propAssistant" 
            :IsHead="propHead" 
            :IsList="propListe"  
            :class="'class_liste_rubrique_mailAdress_' + propSignet.DescId + '_' + propIndexRow + '_' + dataInput.DescId" 
            v-bind:disabled="dataInput.ReadOnly" 
            v-on:blur="verifEmail($event,that);" 
            v-bind:style="{ color: dataInput.ValueColor}"  
            v-model="getInputValue"
            type="email"
            :placeholder="dataInput.Watermark"
        />
        
        <div 
            v-if="!IsDisplayReadOnly && !this.modif"
            v-on:click="goEmailPopup($event)"  
            class="divInput ellipsDiv form-control input-line fname"
        >
            <a 
                href="#!" 
                class="targetIsTrue" 
                v-bind:style="{ color: dataInput.ValueColor}"
            >{{ getInputValue }}</a>
        </div>

        <!-- Si le champ mailAdress n'est pas modifiable -->
        <a 
            v-if="IsDisplayReadOnly"
            href="#!"  
            v-bind:style="{ color: dataInput.ValueColor}" 
            v-on:click="goEmailPopup($event)" 
            class="targetIsTrue linkHead readOnly"
        >{{ getInputValue }}</a>
         
        <!-- Icon -->
        <span 
            v-on:click="!IsDisplayReadOnly ? [goEmailPopup($event),focusIn = true] : focusIn = false " 
            class="input-group-addon"
        >
            <a href="#!" class="hover-pen">
                <i :class="[IsDisplayReadOnly?'mdi mdi-lock':'fas fa-pencil-alt']"></i>
            </a>
        </span>

    </div>
    
     <!-- icône pour la vérification d'email -->
     <template 
        v-if="getInputValue != '' && getverifStatus" 
        class="emailverifdiv"
    >                
        <div 
            :class="{'cursor-pointer':!isMouseOver}" 
            class="divVerifEmail d-flex align-center justify-center" 
            :ref='getverifStatus.name'
            v-on:mouseout="tooltipHover(false)" 
            v-on:mouseover="tooltipHover(true)"
        >
            <i 
                v-if="isNotValid" 
                :class="[getverifStatus.iconClass,getStatusIconCssClass,'email-icons']"
            />
            <span 
                v-else 
                class="fa-stack email-verification fa-1x"
            >
                <i 
                    class="fas fa-certificate fa-stack-2x" 
                    :class="[getStatusIconCssClass,'email-icons']" 
                    te=""
                ></i>
                <i :class="getverifStatus.iconClass"></i>
            </span>
        </div>                  
     </template>   
</div>
`
};