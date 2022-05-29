import { ePhoneMixin } from './ePhoneMixin.js?ver=803000'

export default {
    name: "ePhoneFile",
    data() {
        return {};
    },
    methods: {},
    props: [],
    mixins: [ePhoneMixin],
    template: `
 <div ref="phone" v-on:mouseout="showTooltip(false,'phone',icon,IsDisplayReadOnly,dataInput)" v-on:mouseover="showTooltip(true,'phone',icon,IsDisplayReadOnly,dataInput)" v-if="!propListe" v-bind:class="[focusIn ? 'focusIn' : '', IsDisplayReadOnly ? 'headReadOnly' : '', 'ellips input-group hover-input']" 
        :title="!dataInput.ToolTipText ? dataInput.DisplayValue : dataInput.ToolTipText">

            <!-- Si l'extension phone est activé -->
            <!-- Si le champ phone n'est pas dans le head et est modifiable -->

            <!-- <span ref="info" v-if="showInformationIco()" class="icon-info-circle info-tooltip"></span> -->

            <input :field="'field'+dataInput.DescId" v-on:focus="focusIn = true; bDisplayPopup = false; RemoveBorderSuccessError();bEmptyDisplayPopup = false;hideTooltip(true);" 
                v-on:blur="verifPhone($event, that); focusIn = false;hideTooltip(false);" 
                v-if="!IsDisplayReadOnly && this.modif && dataInput.DisplaySmsBtn" 
                :IsDetail="propDetail" :IsAssistant="propAssistant" :IsHead="propHead" :IsList="propListe" 
                v-bind:class="[propHead ? 'class_liste_rubrique_phone_header_' + dataInput.DescId  : '', propDetail ? 'class_liste_rubrique_phone_detail_' + dataInput.DescId  : '', propAssistant ? 'class_liste_rubrique_phone_assistant_' + propAssistantNbIndex + '_' + dataInput.DescId  : '', 'form-control input-line fname']" v-bind:style="{ color: dataInput.ValueColor}"  
                :value="dataInput.Value" type="tel">


            <div v-if="!IsDisplayReadOnly && !this.modif && dataInput.DisplaySmsBtn" v-on:click="goAction($event)" class="divInput ellipsDiv form-control input-line fname">
                <a :field="'field'+dataInput.DescId" v-on:mouseout="icon = false" v-on:mouseover="icon = true" href="#!" class="targetIsTrue" v-bind:style="{ color: dataInput.ValueColor}">{{dataInput.Value}}</a>
            </div>

            <!-- Si le champ phone n'est pas modifiable ou dans le head -->
            <a v-on:mouseout="icon = false" v-on:mouseover="icon = true" :href="['tel:'+ dataInput.Value]" v-if="IsDisplayReadOnly && dataInput.DisplaySmsBtn" v-bind:style="{ color: dataInput.ValueColor}" v-on:click="goAction($event)" class="targetIsTrue linkHead readOnly">{{dataInput.Value}}</a>
         
            <!-- Icon -->
            <span v-if="!propHead && dataInput.DisplaySmsBtn" v-on:click="goAction($event)" class="input-group-addon"><a href="#!" class="hover-pen"><i :class="[
        (IsDisplayReadOnly && !icon)?'pencil-alt-slash-eudo'
        :(IsDisplayReadOnly && icon)?'fas fa-phone'
        :(!IsDisplayReadOnly && icon)?'fas fa-phone'
        :'fas fa-pencil-alt']"></i></a></span>

            <eAlertBox v-show="this.bDisplayPopup" :b-success="this.bRegExeSuccess">
                <p v-if="!this.bRegExeSuccess">{{messageError}}</p>
            </eAlertBox>

	        <!-- Message d'erreur après la saisie dans le champs -->
            <eAlertBox v-if="this.bEmptyDisplayPopup && !(propHead || dataInput.ReadOnly)" >
                <p>{{getRes(2471)}}</p>
            </eAlertBox>
            

            <!-- Si l'extension phone est desactivé -->
            <!-- Si le champ phone est modifiable -->

            <input v-on:focus="focusIn = true; bDisplayPopup = false; RemoveBorderSuccessError();bEmptyDisplayPopup = false;" 
                v-on:blur="focusIn = false;verifPhone($event, that)" v-if="!(propHead || dataInput.ReadOnly) && !dataInput.DisplaySmsBtn" 
                :IsDetail="propDetail" :IsAssistant="propAssistant" :IsHead="propHead" :IsList="propListe" 
                v-bind:class="[propHead ? 'class_liste_rubrique_phone_header_' + dataInput.DescId  : '', propDetail ? 'class_liste_rubrique_phone_detail_' + dataInput.DescId  : '', propAssistant ? 'class_liste_rubrique_phone_assistant_' + propAssistantNbIndex + '_' + dataInput.DescId  : '', 'form-control input-line fname']" v-bind:style="{ color: dataInput.ValueColor}"  
                :value="dataInput.Value" type="tel">

             <!-- Si le champ phone n'est pas modifiable -->
            <span v-if="IsDisplayReadOnly && !dataInput.DisplaySmsBtn" v-bind:style="{ color: dataInput.ValueColor}" class="readOnly">{{dataInput.Value}}</span>

            <!-- Icon -->
            <span v-on:click="focusInput('phone', {
            props: propAssistant ? PropType.Assistant : propDetail ? PropType.Detail : propListe ? PropType.Liste : PropType.Defaut,
        propAssistantNbIndex: propAssistantNbIndex,
        propIndexRow: propIndexRow,
        dataInput: dataInput,
        propSignet: propSignet
                })" v-if="!(propHead || dataInput.ReadOnly) && !dataInput.DisplaySmsBtn" class="input-group-addon"><a  href="#!" class="hover-pen"><i class="fas fa-pencil-alt"></i></a></span>
        
    </div>
`
};
