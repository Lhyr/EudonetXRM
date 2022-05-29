import { eMemoMixin } from './eMemoMixin.js?ver=803000'

export default {
    name: "eMemoList",
    data() {
        return {};
    },
    mounted() { },
    methods: {},
    props: [],
    mixins: [eMemoMixin],
    template: `
    <div v-if="propListe" v-bind:style="{ height: 30 + 'px'}" v-bind:class="[propHead ? 'headReadOnly' : '', 'listRubriqueMemo ellips input-group hover-input']"  :title="noHtml">
        <span @click="openMemo()" :readonly="dataInput.ReadOnly" class="textareaRubrique form-control input-line fname">{{noHtml}}</span>
    </div>
`
};
