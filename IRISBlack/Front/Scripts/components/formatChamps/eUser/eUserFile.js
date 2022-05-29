import { eUserMixin } from './eUserMixin.js?ver=803000'

export default {
    name: "eUserFile",
    data() {
        return {};
    },
    components: {
        eUserFileMultipleEditable: () => import(AddUrlTimeStampJS("./eUserFileMultipleEditable.js")),
        eUserFileMultipleNotEditable: () => import(AddUrlTimeStampJS("./eUserFileMultipleNotEditable.js")),
        eUserFileSimpleEditable: () => import(AddUrlTimeStampJS("./eUserFileSimpleEditable.js")),
        eUserFileSimpleNotEditable: () => import(AddUrlTimeStampJS("./eUserFileSimpleNotEditable.js")),
    },
    methods: {},
    props: [],
    mixins: [eUserMixin],
    template: `
    <div ref="user" v-on:mouseout="showTooltip(false,'user',false,IsDisplayReadOnly,dataInput)" v-on:mouseover="showTooltip(true,'user',false,IsDisplayReadOnly,dataInput)" v-if="!propListe" v-bind:class="[propHead || dataInput.ReadOnly ? 'headReadOnly' : '', 'ellips input-group hover-input']" 
        :title="!dataInput.ToolTipText ? dataInput.DisplayValue : dataInput.ToolTipText">


        <!-- Si le champ multiple et modifiable -->
        <eUserFileMultipleEditable />

        <!-- Si le champ multiple et pas modifiable -->
        <eUserFileMultipleNotEditable />

        <!-- Si le champ simple et modifiable -->
        <eUserFileSimpleEditable />

        <!-- Si le champ simple et pas modifiable -->
        <eUserFileSimpleNotEditable />

        <!-- Icon -->
        <span v-on:click="openDial" class="input-group-addon"><a href="#!" class="hover-pen"><i :class="[IsDisplayReadOnly?'fas pencil-alt-slash-eudo':'fas fa-pencil-alt']"></i></a></span>
    </div>
`
};
