import { eSocialNetWorkMixin } from './eSocialNetWorkMixin.js?ver=803000'

export default {
    name: "eSocialNetWorkFile",
    data() {
        return {};
    },
    mounted() { },
    methods: {},
    props: [],
    mixins: [eSocialNetWorkMixin],
    template: `
   <div ref="social" v-on:mouseout="showTooltip(false,'social',icon,IsDisplayReadOnly,dataInput)" v-on:mouseover="showTooltip(true,'social',icon,IsDisplayReadOnly,dataInput)" v-if="!propListe" v-bind:class="[IsDisplayReadOnly?'read-only':'',propHead ? 'headReadOnly' : '', focusIn ? 'focusIn' : '', 'ellips input-group hover-input']"
        :title="!dataInput.ToolTipText ? dataInput.DisplayValue : dataInput.ToolTipText">

        <!-- <span ref="info" v-if="showInformationIco()" class="icon-info-circle info-tooltip"></span> -->

        <!-- Si le champ socialNetWork n'est pas dans le head et est modifiable -->
        <input :field="'field'+dataInput.DescId" v-on:focus="bDisplayPopup = false; RemoveBorderSuccessError();bEmptyDisplayPopup = false; focusIn = true;hideTooltip(true);" 
        v-on:blur="verifSocial($event, that); focusIn = false;hideTooltip(false);" 
        v-if="!IsDisplayReadOnly && this.modif" :IsDetail="propDetail" :IsAssistant="propAssistant" :IsHead="propHead" :IsList="propListe" v-bind:class="[propHead ? 'class_liste_rubrique_socialNetWork_header_' + dataInput.DescId  : '', propDetail ? 'class_liste_rubrique_socialNetWork_detail_' + dataInput.DescId  : '', propAssistant ? 'class_liste_rubrique_socialNetWork_assistant_' + propAssistantNbIndex + '_' + dataInput.DescId  : '', 'form-control input-line fname']" v-bind:style="{ color: dataInput.ValueColor}"  
        :value="dataInput.Value" type="text" :placeholder="dataInput.Watermark">

        <div v-bind:style="[dataInput.ValueColor ? { color: dataInput.ValueColor} : {color: '#337ab7'}]" v-if="!IsDisplayReadOnly && !this.modif" v-on:click="goAction($event)"   type="text" class="divInput ellipsDiv form-control input-line fname">
            <a :field="'field'+dataInput.DescId" v-on:mouseout="icon = false" v-on:mouseover="icon = true" v-bind:href="[dataInput.RootURL != '' ? dataInput.RootURL + dataInput.Value : 'http://' + dataInput.Value]" target="_blank" class="targetIsTrue" v-bind:style="{ color: dataInput.ValueColor}">{{dataInput.Value}}</a>
        </div>

        <!-- Si le champ socialNetWork n'est pas modifiable ou dans le head -->
        <a v-on:mouseout="icon = false" v-on:mouseover="icon = true" v-bind:href="[dataInput.RootURL != '' ? dataInput.RootURL + dataInput.Value : 'http://' + dataInput.Value]" target="_blank" v-if="IsDisplayReadOnly" v-bind:style="{ color: dataInput.ValueColor}" v-on:click="goAction($event)" class="targetIsTrue linkHead readOnly">{{dataInput.Value}}</a>
         
        <!-- Icon -->
        <span v-if="!propHead" v-on:click="goAction($event)" class="input-group-addon"><a href="#!" class="hover-pen"><i :class="[
        (IsDisplayReadOnly && !icon)?'pencil-alt-slash-eudo'
        :(IsDisplayReadOnly && icon)?'fas fa-external-link-alt'
        :(!IsDisplayReadOnly && icon)?'fas fa-external-link-alt'
        :'fas fa-pencil-alt']"></i></a></span>
       
   	    <!-- Message d'erreur après la saisie dans le champs -->
        <eAlertBox  v-if="this.bEmptyDisplayPopup && !(propHead || dataInput.ReadOnly)" >
            <p>{{getRes(2471)}}</p>
        </eAlertBox>

    </div>

`
};
