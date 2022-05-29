import { eFileMixin } from './eFileMixin.js?ver=803000';
import { getTabDescid } from "../../methods/eMainMethods.js?ver=803000";

export default {
    name: "eFileList",
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
    <div v-if="propListe" v-bind:class="[propListe ? 'listRubriqueRelation' : '', 'ellips input-group hover-input']"
        :title="!dataInput.ToolTipText ? dataInput.DisplayValue : dataInput.ToolTipText">

        <!-- Si le champ file n'est pas dans le head et est modifiable -->

        <div v-on:click="goAction($event)" v-if="!IsDisplayReadOnly"  type="text" class="ellipsDiv form-control input-line fname">
            <a :field="'field'+dataInput.DescId" :href="getBaseUrl + 'folders/[' + getTabDescid(this.dataInput.DescId) + ']/' + this.dataInput.Value" target="_blank" class="targetIsTrue" v-bind:style="{ color: dataInput.ValueColor}">{{dataInput.Value}}</a>
        </div>

        <!-- Si le champ file n'est pas modifiable ou dans le head -->
        <a :href="getBaseUrl + 'folders/[' + getTabDescid(this.dataInput.DescId) + ']/' + this.dataInput.Value" target="_blank" v-if="IsDisplayReadOnly" v-bind:style="{ color: dataInput.ValueColor}" v-on:click="goAction($event)" class="targetIsTrue linkHead readOnly">{{dataInput.Value}}</a>
         
        <!-- Icon -->
        <span v-on:click="goAction($event)" v-if="!IsDisplayReadOnly" class="input-group-addon"><a href="#!" class="hover-pen"><i class="fas fa-pencil-alt"></i></a></span>
       
    </div>

`
};
