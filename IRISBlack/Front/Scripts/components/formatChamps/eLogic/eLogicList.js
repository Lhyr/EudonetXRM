import { eLogicMixin } from './eLogicMixin.js?ver=803000'

export default {
    name: "eLogicList",
    data() {
        return {};
    },
    mounted() { },
    methods: {},
    props: [],
    mixins: [eLogicMixin],
    template: `
     <div v-if="propListe" v-bind:class="[propListe ? 'listRubriqueLogic' : '', 'ellips input-group hover-input']"
        :title="!dataInput.ToolTipText ? dataInput.DisplayValue : dataInput.ToolTipText">
        <input v-on:click="verifLogic($event, that)" class="inp-cbx checkboxComponent" :id="'id_liste_rubrique_checkbox' + propSignet.DescId + '_' + propIndexRow + '_' + dataInput.DescId" style="display: none" type="checkbox"
            :disabled="IsDisplayReadOnly" :checked="this.dataInput.Value == 1"
            :placeholder="dataInput.Watermark">
        <label v-on:click="verifLogic($event, that)" v-bind:class="{'readonly':IsDisplayReadOnly}" class="checkBoxRubrique cbx" :for="'id_liste_rubrique_checkbox' + propSignet.DescId + '_' + propIndexRow + '_' + dataInput.DescId">
            <span class="checkboxComponentSpan">
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 12 10" width="12px" height="10px">
                    <polyline points="1.5,6 4.5,9 10.5,1" />
                </svg>
            </span>
        </label>
    </div>
`
};
