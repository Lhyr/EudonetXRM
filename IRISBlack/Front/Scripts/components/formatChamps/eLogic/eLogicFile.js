import { eLogicMixin } from './eLogicMixin.js?ver=803000'

export default {
    name: "eLogicFile",
    data() {
        return {};
    },
    mounted() { },
    methods: {},
    props: [],
    mixins: [eLogicMixin],
    template: `
    <div v-if="!propListe" v-bind:class="[propHead ? 'headReadOnly' : '', 'ellips input-group hover-input']"
        :title="!dataInput.ToolTipText ? dataInput.DisplayValue : dataInput.ToolTipText">

        <!-- <span ref="info" v-if="showInformationIco()" class="icon-info-circle info-tooltip"></span> -->

        <input :value="this.dataInput.Value" class="inp-cbx checkboxComponent" :id="this.ComponentId" style="display: none" type="checkbox" :disabled="IsDisplayReadOnly" :checked="this.dataInput.Value == 1" v-on:click="verifLogic($event, that)"
            :placeholder="dataInput.Watermark">
        <label v-bind:class="{'readonly':IsDisplayReadOnly}" class="checkBoxRubrique cbx" :for="this.ComponentId" v-on:click="verifLogic($event, that)">
            <span class="checkboxComponentSpan">
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 12 10" width="12px" height="10px">
                    <polyline points="1.5,6 4.5,9 10.5,1" />
                </svg>
            </span>
        </label>
    </div>
`
};
