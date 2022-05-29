import { Operator } from '../../methods/Enum.min.js?ver=803000';
import { eMotherClassMixin } from '../../mixins/eMotherClassMixin.js?ver=803000';

export default {
    name: "expressFilterFooter",
    data() {
        return {
        };
    },
    components: {},
    mixins: [eMotherClassMixin],
    template: `
    <li class="data-button-footer">
        <ul class="xprss-filter__list">
            <!--vide-->
            <li class="xprss-filter__li" :title="getRes(141)" @click="emptyFilter">{{getRes(141)}}</li>
            <!--Non vide-->
            <li class="xprss-filter__li" :title="getRes(1203)" @click="notEmptyFilter">{{getRes(1203)}}</li>
            <!--Aucun filtre-->
            <li class="xprss-filter__li" @click="noFilters" ><span><i class="icon-rem_filter" :title="getRes(183)"></i></span>{{getRes(183)}}</li>
        </ul>
    </li>
`,
    methods: {
        emptyFilter: function () {
            this.$parent.updatePref(Operator.OP_IS_EMPTY + ";|;NULL");
        },
        notEmptyFilter: function () {
            this.$parent.updatePref(Operator.OP_IS_NOT_EMPTY + ";|;<>");
        },
        noFilters: function () {
            this.$parent.updatePref(Operator.OP_IS_NOT_EMPTY + ";|;$cancelthisfilter$")
        },
    },
    mounted() {
    }
};