import { eRelationMixin } from './eRelationMixin.js?ver=803000'

export default {
    name: "eRelationList",
    data() {
        return {};
    },
    mounted() { },
    methods: {},
    props: [],
    mixins: [eRelationMixin],
    template: `
   <div v-if="propListe" v-bind:class="[propListe ? 'listRubriqueRelation' : '', 'ellips input-group hover-input']" 
        :title="!dataInput.ToolTipText ? dataInput.DisplayValue : dataInput.ToolTipText">

        <!-- Si le champ relation n'est pas dans le head et est modifiable -->
        <div :field="'field'+dataInput.DescId" v-on:click="goAction($event)" v-if="!IsDisplayReadOnly"  type="text" class="ellipsDiv form-control input-line fname">
            <a ref="relationlist" v-on:mouseover="showVCardOrMiniFile($event, true);" v-on:mouseout="showVCardOrMiniFile($event, false);" class="targetIsTrue" href="#!" v-bind:style="{ color: dataInput.ValueColor}">{{dataInput.DisplayValue}}</a>
        </div>

        <div v-on:click="goAction($event)" v-if="IsDisplayReadOnly"  type="text" class="NoModifSimple ellipsDiv form-control input-line fname">
            <a ref="relationlist" v-bind:style="{ color: dataInput.ValueColor}"  class="targetIsTrue linkHead readOnly" v-on:mouseover="showVCardOrMiniFile($event, true);" v-on:mouseout="showVCardOrMiniFile($event, false);" :dbv="dataInput.Value" :vcMiniFileTab="dataInput.TargetTab">{{dataInput.DisplayValue}}</a>
        </div>

        <!-- Si le champ relation n'est pas modifiable ou dans le head -->
        
         
        <!-- Icon -->
        <span  v-on:click="goAction($event)" v-if="!IsDisplayReadOnly" class="input-group-addon"><a href="#!" class="hover-pen"><i class="fas fa-pencil-alt"></i></a></span>
       
    </div>
`
};
