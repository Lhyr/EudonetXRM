import { eGeolocalisationMixin } from './eGeolocalisationMixin.js?ver=803000'

export default {
    name: "eGeolocalisationFile",
    data() {
        return {};
    },
    mounted() { },
    methods: {},
    props: [],
    mixins: [eGeolocalisationMixin],
    template: `
    <div  ref="geolocation" v-on:mouseout="showTooltip(false,'geolocation',false,IsDisplayReadOnly,dataInput)" v-on:mouseover="showTooltip(true,'geolocation',false,IsDisplayReadOnly,dataInput)" v-if="!propListe" v-bind:class="[propHead || dataInput.ReadOnly ? 'headReadOnly' : '', 'ellips input-group hover-input']"
        :title="!dataInput.ToolTipText ? dataInput.DisplayValue : dataInput.ToolTipText">

        <!-- <span ref="info" v-if="showInformationIco()" class="icon-info-circle info-tooltip"></span> -->

        <!-- Si le champ catalogue est modifiable et pas multiple et pas dans le head -->
        <!-- Si le champ catalogue n'est pas multiple et dans le head ou si il est pas multiple et pas modifiable -->
        <div v-on:click="[!dataInput.ReadOnly ?  goAction() : '']" v-bind:style="{ color: dataInput.ValueColor}"  type="text" class="ellipsDiv form-control input-line fname">
            {{dataInput.Value}}
        </div>

        <!-- Icon -->
        <span v-if="!propHead" v-on:click="goAction" class="input-group-addon"><a href="#!" class="hover-pen"><i :class="[IsDisplayReadOnly?'fas pencil-alt-slash-eudo':'fas fa-pencil-alt']"></i></a></span>
    </div>
`
};
