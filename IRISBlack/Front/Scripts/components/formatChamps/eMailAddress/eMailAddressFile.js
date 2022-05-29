import { eMailAddressMixin } from './eMailAddressMixin.js?ver=803000';

export default {
    name: "eMailAddressFile",
    data() {
        return {};
    },
    mounted() { },
    methods: {},
    props: [],
    mixins: [eMailAddressMixin],
    template: `
<div ref="email" v-on:mouseout="showTooltip(false,'email',icon,IsDisplayReadOnly,dataInput)" v-on:mouseover="showTooltip(true,'email',icon,IsDisplayReadOnly,dataInput)" v-if="!propListe" v-bind:class="[IsDisplayReadOnly?'read-only':'', propHead ? 'headReadOnly' : '', focusIn ? 'focusIn' : '', 'ellips input-group hover-input']" 
        :title="!dataInput.ToolTipText ? dataInput.DisplayValue : dataInput.ToolTipText">

        <!-- <span ref="info" v-if="showInformationIco()" class="icon-info-circle info-tooltip"></span> -->
        <!-- Si le champ mailAdress n'est pas dans le head et est modifiable -->
        <input
                v-if="!IsDisplayReadOnly && this.modif"
                :IsDetail="propDetail"
                :IsAssistant="propAssistant"
                :IsHead="propHead" :IsList="propListe"
                v-bind:class="[propHead ? 'class_liste_rubrique_mailAdress_header_' + dataInput.DescId  : '', propDetail ? 'class_liste_rubrique_mailAdress_detail_' + dataInput.DescId  : '', propAssistant ? 'class_liste_rubrique_mailAdress_assistant_' + propAssistantNbIndex + '_' + dataInput.DescId  : '', 'form-control input-line fname']"
                @blur="verifEmail($event,that); focusIn = false;hideTooltip(false)"
                @focus="bDisplayPopup = false; RemoveBorderSuccessError();bEmptyDisplayPopup = false; focusIn = true;hideTooltip(true)"
                :style="{ color: dataInput.ValueColor}"
                :value="dataInput.Value" type="email"
                :title="!dataInput.ToolTipText ? dataInput.Value : dataInput.ToolTipText"
                :placeholder="dataInput.Watermark"
        >


        <div v-bind:style="[dataInput.ValueColor ? { color: dataInput.ValueColor} : {color: '#337ab7'}]" v-if="!IsDisplayReadOnly && !this.modif" v-on:click="goAction($event)" class="divInput ellipsDiv form-control input-line fname">
            <a v-on:mouseout="icon = false" v-on:mouseover="icon = true" href="#!" class="targetIsTrue" v-bind:style="{ color: dataInput.ValueColor}">{{dataInput.Value}}</a>
        </div>
        <!-- Si le champ mailAdress n'est pas modifiable ou dans le head -->
        <a v-on:mouseout="icon = false" v-on:mouseover="icon = true" href="#!" v-if="IsDisplayReadOnly" :style="{ color: dataInput.ValueColor}" @click="goAction($event)" class="targetIsTrue linkHead readOnly">{{dataInput.Value}}</a>


        <!-- Icon -->
        <span v-if="!propHead" v-on:click="goAction($event)" :class="[IsDisplayReadOnly?'read-only':'','input-group-addon']"><a href="#!" class="hover-pen"><i :class="[
        (IsDisplayReadOnly && !icon)?'pencil-alt-slash-eudo'
        :(IsDisplayReadOnly && icon)?'fas fa-envelope'
        :(!IsDisplayReadOnly && icon)?'fas fa-envelope'
        :'fas fa-pencil-alt']"></i></a></span>

        <eAlertBox v-if="this.bDisplayPopup" :b-success="this.bRegExeSuccess">
            <p v-if="!this.bRegExeSuccess">{{this.messageError}}</p>
        </eAlertBox>

	    <!-- Message d'erreur si champs obligatoire est vide -->
        <eAlertBox v-if="dataInput.Required && this.bEmptyDisplayPopup && !(propHead || dataInput.ReadOnly)" >
             <p>{{getRes(2471)}}</p>
        </eAlertBox>
     </div>
`
};
