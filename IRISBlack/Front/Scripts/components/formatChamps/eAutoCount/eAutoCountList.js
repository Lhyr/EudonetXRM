import { eAutoCountMixin } from './eAutoCountMixin.js?ver=803000'

export default {
    name: "eAutoCountList",
    data() {
        return {};
    },
    mounted() { },
    methods: {},
    props: [],
    mixins: [eAutoCountMixin],
    template: `
    <div v-on:mouseover="inputHovered = true" v-on:blur="inputHovered = false" v-if="!propListe" v-bind:class="[propHead ? 'headReadOnly' : '', 'ellips input-group hover-input']"
        :title="!dataInput.ToolTipText ? dataInput.DisplayValue : dataInput.ToolTipText">
        <!-- <span ref="info" v-if="showInformationIco()" class="icon-info-circle info-tooltip"></span> -->

        <!-- Si le champ autCount n'est pas modifiable -->
        <span v-bind:style="{ color: dataInput.ValueColor}" class="readOnly" >{{dataInput.Value}}</span>

        <!-- Icon -->
        <span v-if="!propHead" :class="[inputHovered ? 'editing-mode':'','input-group-addon']"><a  href="#!" class="hover-pen"><i :class="[IsDisplayReadOnly?'fas pencil-alt-slash-eudo':'fas fa-pencil-alt']"></i></a></span>

    </div>
`
};