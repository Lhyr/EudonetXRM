import { eMemoMixin } from './eMemoMixin.js?ver=803000'

export default {
    name: "eMemoFile",
    data() {
        return {};
    },
    components: {
        eMemoFilePlainText: () => import(AddUrlTimeStampJS("./eMemo/eMemoFilePlainText.js")),
    },
    mounted() { },
    methods: {},
    props: [],
    mixins: [eMemoMixin],
    template: `
    <div v-if="!propListe" v-bind:style="{ height: !cptFromModal ? (dataInput.Rowspan*60) - (36 + 16 + 10) + 'px' : ''}" v-bind:class="[propHead ? 'headReadOnly' : '' , dataInput.ReadOnly ? 'readOnlyTxt' : '', 'rubriqueEmemo ellips input-group hover-input']"
        :title="!dataInput.ToolTipText ? dataInput.DisplayValue : dataInput.ToolTipText">

        <!-- <span ref="info" v-if="showInformationIco()" class="icon-info-circle info-tooltip"></span> -->

        <textarea @focus="TextAreaFocus()" @blur="TextAreaBlur($event)" :IsDetail="propDetail" v-if="dataInput.IsHtml" :id="GetComponentId" :readonly="dataInput.ReadOnly" class="form-control input-line fname">{{ValueToDisplay}}</textarea>
        <eMemoFilePlainText />
    </div>
    <eAlertBox v-if="this.bEmptyDisplayPopup && !(propHead || dataInput.ReadOnly)" >
        <p>{{getRes(2471)}}</p>
    </eAlertBox>
`
};
