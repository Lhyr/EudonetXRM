﻿import { eHyperLinkMixin } from './eHyperLinkMixin.js?ver=803000'

export default {
    name: "eHyperLinkList",
    data() {
        return {};
    },
    mounted() { },
    methods: {},
    props: [],
    mixins: [eHyperLinkMixin],
    template: `
    <div v-if="propListe" v-bind:class="[propListe ? 'listRubriqueRelation' : '', 'ellips input-group hover-input']" 
        :title="!dataInput.ToolTipText ? dataInput.DisplayValue : dataInput.ToolTipText">

        <!-- Si le champ hyperLink n'est pas dans le head et est modifiable -->
        <input :field="'field'+dataInput.DescId" v-if="!IsDisplayReadOnly && this.modif" :IsDetail="propDetail" :IsAssistant="propAssistant" :IsHead="propHead" :IsList="propListe"  
            :class="'class_liste_rubrique_hyperLink_' + propSignet.DescId + '_' + propIndexRow + '_' + dataInput.DescId" v-bind:disabled="dataInput.ReadOnly" 
            v-on:focus="bDisplayPopup = false; RemoveBorderSuccessError();bEmptyDisplayPopup = false; " v-on:blur="verifHyperlinks($event, that)"
            v-bind:style="{ color: dataInput.ValueColor}"  :value="dataInput.Value" type="text" class="ellipsDiv form-control input-line fname"
            :placeholder="dataInput.Watermark">
        
        <div v-on:click="goAction($event)" v-if="!IsDisplayReadOnly && !this.modif" class="divInput ellipsDiv form-control input-line fname">
            <a :field="'field'+dataInput.DescId" :href="dataInput.Value" target="_blank" class="targetIsTrue" v-bind:style="{ color: dataInput.ValueColor}">{{dataInput.Value}}</a>
        </div>

        <!-- Si le champ hyperLink n'est pas modifiable ou dans le head -->
        <a :href="dataInput.Value" target="_blank" v-if="IsDisplayReadOnly" v-bind:style="{ color: dataInput.ValueColor}" v-on:click="goAction($event)" class="targetIsTrue linkHead readOnly">{{dataInput.Value}}</a>
         
        <!-- Icon -->
        <span v-on:click="goAction($event)" v-if="!IsDisplayReadOnly" class="input-group-addon"><a href="#!" class="hover-pen"><i class="fas fa-pencil-alt"></i></a></span>
       
    </div>
`
};
