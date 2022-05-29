import { eButtonMixin } from './eButtonMixin.js?ver=803000'

export default {
    name: "eButtonFile",
    data() {
        return {};
    },
    mounted() { },
    methods: {},
    props: [],
    mixins: [eButtonMixin],
    template: `
    <div v-if="!propListe" v-bind:class="[propHead ? 'headReadOnly' : '', 'ellips input-group hover-input btn_eudo_content']" :title="!dataInput.ToolTipText ? dataInput.Label : dataInput.ToolTipText">

        <!-- <span ref="info" v-if="showInformationIco()" class="icon-info-circle info-tooltip"></span> -->
        <!-- modification -->
        <button v-on:click="verifButton($event, that)" v-if="!(propHead || dataInput.ReadOnly)" v-bind:style="{ width: '100%!important', color: dataInput.StyleForeColor+'!important', background: dataInput.BackgroundColor+'!important', borderColor: dataInput.BackroundColor+'!important'}" :value="dataInput.Value" type="button" class="btn_eudo btn btn-default nouveau-contact">{{dataInput.Label}}</button>
        
        <!-- non modification -->
        <button v-if="propHead || dataInput.ReadOnly" disabled v-bind:style="{ width: '100%!important', color: dataInput.StyleForeColor+'!important', background: dataInput.BackgroundColor+'!important', borderColor: dataInput.BackroundColor+'!important'}"  :value="dataInput.Label" type="button" class="btn_eudo btn btn-default nouveau-contact">{{dataInput.Label}}</button>
        
       
    </div>
`
};