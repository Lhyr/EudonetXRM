import { eGeolocalisationMixin } from './eGeolocalisationMixin.js?ver=803000'

export default {
    name: "eGeolocalisationList",
    data() {
        return {};
    },
    mounted() { },
    methods: {},
    props: [],
    mixins: [eGeolocalisationMixin],
    template: `
    <div v-if="propListe" v-bind:class="[propListe ? 'listRubriqueRelation' : '', 'ellips input-group hover-input']"
        :title="!dataInput.ToolTipText ? dataInput.DisplayValue : dataInput.ToolTipText">

        <!-- Si le champ geolocation n'est pas dans le head et est modifiable -->
        <div v-on:click="goAction($event)" v-if="!IsDisplayReadOnly"  type="text" class="ellipsDiv form-control input-line fname">
            <a class="targetIsTrue" href="#!" v-bind:style="{ color: dataInput.ValueColor}">{{dataInput.Value}}</a>
        </div>

        <!-- Si le champ geolocation n'est pas modifiable ou dans le head -->
        <a v-if="IsDisplayReadOnly" v-bind:style="{ color: dataInput.ValueColor}" v-on:click="goAction($event)" class="targetIsTrue linkHead readOnly">{{dataInput.Value}}</a>
         
        <!-- Icon -->
        <span v-on:click="goAction($event)" v-if="!IsDisplayReadOnly" class="input-group-addon"><a href="#!" class="hover-pen"><i class="fas fa-pencil-alt"></i></a></span>

    </div>
`
};
