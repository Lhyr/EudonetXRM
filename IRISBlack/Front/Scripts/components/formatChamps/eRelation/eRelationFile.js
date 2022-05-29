import { eRelationMixin } from './eRelationMixin.js?ver=803000'

export default {
    name: "eRelationFile",
    data() {
        return {};
    },
    mounted() { },
    methods: {},
    props: [],
    mixins: [eRelationMixin],
    template: `
    <div ref="relation" 
        @mouseout="showTooltip(false,'relation',icon,IsDisplayReadOnly,dataInput)" 
        @mouseover="showTooltip(true,'relation',icon,IsDisplayReadOnly,dataInput)"
        v-if="!propListe" v-bind:class="[propHead ? 'headReadOnly' : '', 'ellips input-group hover-input']" 
        :title="!dataInput.ToolTipText ? dataInput.DisplayValue : dataInput.ToolTipText">

        <!-- <span ref="info" v-if="showInformationIco()" class="icon-info-circle info-tooltip"></span> -->

        <!-- Si le champ relation n'est pas dans le head et est modifiable -->
        <div :field="'field'+dataInput.DescId" :id="'COL_' + getTab + '_' + this.dataInput.DescId + '_1_1_0'" v-on:click="goAction($event)" v-if="!IsDisplayReadOnly"  type="text" class="ellipsDiv form-control input-line fname">
            <a ref="relationfile" v-on:mouseover="showVCardOrMiniFile($event, true);icon = true;" 
            v-on:mouseout="showVCardOrMiniFile($event, false);icon = false;" :dbv="dataInput.Value" :vcMiniFileTab="dataInput.TargetTab" 
            class="targetIsTrue" href="#!" 
            v-bind:style="{ color: dataInput.ValueColor}">{{dataInput.DisplayValue}}</a>
        </div>

        <!-- Si le champ relation n'est pas modifiable ou dans le head -->
        <a ref="relationfile" v-if="IsDisplayReadOnly" v-bind:style="{ color: dataInput.ValueColor}" v-on:click="goAction($event)" class="targetIsTrue linkHead readOnly" v-on:mouseover="showVCardOrMiniFile($event, true);icon = true" v-on:mouseout="showVCardOrMiniFile($event, false);icon = false" :dbv="dataInput.Value" :vcMiniFileTab="dataInput.TargetTab">{{dataInput.DisplayValue}}</a>
         
        <!-- Icon -->
        <span v-if="!propHead" :id="'COL_' + getTab + '_' + this.dataInput.DescId" v-on:click="goAction($event)" class="input-group-addon"><a href="#!" class="hover-pen"><i :class="[
        (IsDisplayReadOnly && !icon)?'pencil-alt-slash-eudo'
        :(IsDisplayReadOnly && icon)?'fas fa-link'
        :(!IsDisplayReadOnly && icon)?'fas fa-link'
        :'fas fa-pencil-alt']"></i></a></span>
       
	    <!-- Message d'erreur si champs obligatoire est vide -->
        <eAlertBox v-if="this.dataInput.Required && this.bEmptyDisplayPopup" >
            <p>{{getRes(2471)}}</p>
        </eAlertBox>

    </div>
`
};
