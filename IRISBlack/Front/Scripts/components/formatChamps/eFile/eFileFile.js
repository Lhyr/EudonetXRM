import { eFileMixin } from './eFileMixin.js?ver=803000';
import { getTabDescid } from "../../methods/eMainMethods.js?ver=803000";

export default {
    name: "eFileFile",
    data() {
        return {};
    },
    mounted() { },
    methods: {
        getTabDescid,
    },
    props: [],
    mixins: [eFileMixin],
    template: `
    <div ref="file" v-on:mouseout="showTooltip(false,'file',icon,IsDisplayReadOnly,dataInput)" v-on:mouseover="showTooltip(true,'file',icon,IsDisplayReadOnly,dataInput)" v-if="!propListe" v-bind:class="[IsDisplayReadOnly?'read-only':'', propHead ? 'headReadOnly' : '', 'ellips input-group hover-input']"
        :title="!dataInput.ToolTipText ? dataInput.DisplayValue : dataInput.ToolTipText">

        <!-- <span ref="info" v-if="showInformationIco()" class="icon-info-circle info-tooltip"></span> -->

        <!-- Si le champ file n'est pas dans le head et est modifiable -->
        
        <div v-bind:style="[dataInput.ValueColor ? { color: dataInput.ValueColor} : {color: '#337ab7'}]" v-on:click="goAction($event)" v-if="!IsDisplayReadOnly"  type="text" class="ellipsDiv form-control input-line fname">
            <a  :field="'field'+dataInput.DescId" v-on:mouseout="icon = false" v-on:mouseover="icon = true" class="targetIsTrue" :href="getBaseUrl + 'folders/[' + getTabDescid(this.dataInput.DescId) + ']/' + this.dataInput.Value" target="_blank" v-bind:style="{ color: dataInput.ValueColor}">{{dataInput.Value}}</a>
        </div>

        <!-- Si le champ file n'est pas modifiable ou dans le head -->
        <a v-on:mouseout="icon = false" v-on:mouseover="icon = true" :href="getBaseUrl + 'folders/[' + getTabDescid(this.dataInput.DescId) + ']/' + this.dataInput.Value" target="_blank" v-if="IsDisplayReadOnly" v-bind:style="{ color: dataInput.ValueColor}" v-on:click="goAction($event)" class="targetIsTrue linkHead readOnly">{{dataInput.Value}}</a>
         
        
        <!-- Icon -->
        <span v-if="!propHead" v-on:click="goAction($event)" class="input-group-addon"><a href="#!" class="hover-pen"><i :class="[
        (IsDisplayReadOnly && !icon)?'pencil-alt-slash-eudo'
        :(IsDisplayReadOnly && icon)?'fas fa-external-link-alt'
        :(!IsDisplayReadOnly && icon)?'fas fa-external-link-alt'
        :'fas fa-pencil-alt']"></i></a></span>
    </div>
`
};
