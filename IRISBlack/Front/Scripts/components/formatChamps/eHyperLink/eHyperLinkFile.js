import { eHyperLinkMixin } from './eHyperLinkMixin.js?ver=803000'

export default {
    name: "eHyperLinkFile",
    data() {
        return {};
    },
    mounted() { },
    methods: {},
    props: [],
    mixins: [eHyperLinkMixin],
    template: `
 <div ref="link" v-on:mouseout="showTooltip(false,'link',icon,IsDisplayReadOnly,dataInput)" v-on:mouseover="showTooltip(true,'link',icon,IsDisplayReadOnly,dataInput)" v-if="!propListe" v-bind:class="[IsDisplayReadOnly?'read-only':'', propHead ? 'headReadOnly' : '', focusIn ? 'focusIn' : '', 'ellips input-group hover-input']"
        :title="!dataInput.ToolTipText ? dataInput.DisplayValue : dataInput.ToolTipText">

        <!-- <span ref="info" v-if="showInformationIco()" class="icon-info-circle info-tooltip"></span> -->

        <!-- Si le champ hyperLink n'est pas dans le head et est modifiable -->
        <input :field="'field'+dataInput.DescId" v-if="!IsDisplayReadOnly && this.modif" :IsDetail="propDetail" :IsAssistant="propAssistant" :IsHead="propHead" 
            :IsList="propListe" v-bind:class="[propHead ? 'class_liste_rubrique_hyperLink_header_' + dataInput.DescId  : '', propDetail ? 'class_liste_rubrique_hyperLink_detail_' + dataInput.DescId  : '', propAssistant ? 'class_liste_rubrique_hyperLink_assistant_' + propAssistantNbIndex + '_' + dataInput.DescId  : '', 'form-control input-line fname']"
            v-on:focus="focusIn = true; bDisplayPopup = false; RemoveBorderSuccessError();bEmptyDisplayPopup = false;hideTooltip(true);" 
            v-on:blur="verifHyperlinks($event, that); focusIn = false; hideTooltip(false)" 
            v-bind:style="{ color: dataInput.ValueColor}"  :value="dataInput.Value" type="text"
            :placeholder="dataInput.Watermark">
        <div v-if="!IsDisplayReadOnly && !this.modif" v-on:click="goAction($event)" class="divInput ellipsDiv form-control input-line fname">
            <a :field="'field'+dataInput.DescId" v-on:mouseout="icon = false;" v-on:mouseover="icon = true;" :href="dataInput.Value" target="_blank" class="targetIsTrue" v-bind:style="{ color: dataInput.ValueColor}">{{dataInput.Value}}
            <!-- <span class="modification">{{msgHover}}</span> -->
            </a>
        </div>

        <!-- Si le champ hyperLink n'est pas modifiable ou dans le head -->
        <a v-on:mouseout="icon = false" v-on:mouseover="icon = true"  :href="dataInput.Value" target="_blank" v-if="IsDisplayReadOnly" v-bind:style="{ color: dataInput.ValueColor}" v-on:click="goAction($event)" class="targetIsTrue linkHead readOnly">{{dataInput.Value}}</a>
         
        <!-- Icon -->
        <span v-if="!propHead" v-on:click="goAction($event)" :class="[icon?'icons-hidden':'','input-group-addon']">
            <a href="#!" class="hover-pen">
                <i :class="[
        (IsDisplayReadOnly && !icon)?'pencil-alt-slash-eudo'
        :(IsDisplayReadOnly && icon)?'fas fa-external-link-alt'
        :(!IsDisplayReadOnly && icon)?'fas fa-external-link-alt'
        :'fas fa-pencil-alt']"></i>
            </a>
        </span>
       
    	<!-- Message d'erreur si champs obligatoire est vide -->
        <eAlertBox v-if="dataInput.Required && this.bEmptyDisplayPopup && !(propHead || dataInput.ReadOnly)" >
            <p>{{getRes(2471)}}</p>
        </eAlertBox>
    </div>
`
};
