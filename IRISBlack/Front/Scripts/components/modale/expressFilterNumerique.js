import { eExpressFilterMixin } from '../../mixins/eExpressFilterMixin.js?ver=803000';
import { Operator } from '../../methods/Enum.min.js?ver=803000';

export default {
    name: "expressFilterNumerique",
    data() {
        return {
        };
    },
    mixins: [eExpressFilterMixin],
    template: `
    <ul :format="this.propData.datas.Format" class="filterContent" v-bind:style="{ top: this.top, left: this.propData.posLeft + 'px' }">
        <expressFilterSearch />
        <li v-if="SearchValue == ''" class="data-mru">
            <ul>
                <li class="actionItem" 
                    :title="dtMru.DisplayValue" 
                    v-for="dtMru in this.DataMRU"
                    @click="searchFromValue(dtMru.Value)">{{dtMru.DisplayValue}}</li>
            </ul>
        </li>
        <li v-else class="data-mru">
            <ul>
                <!-- 2000 -   égal à -->
                <li class="actionItem" :title="GetResValue(getRes(2000))">
                    <span @click="equalFilter">{{ GetResValue(getRes(2000)) }}</span></li>
                <!-- 2001 -   inférieur à -->
                <li class="actionItem" :title="GetResValue(getRes(2001))">
                    <span @click="lessThanFilter">{{ GetResValue(getRes(2001)) }}</span></li>
                <!-- 2003 -   supérieur à -->
                <li class="actionItem" :title="GetResValue(getRes(2003))">
                    <span @click="greaterThanFilter">{{ GetResValue(getRes(2003)) }}</span></li>
            </ul>
        </li>
        <expressFilterFooter />
    </ul>
`,
    methods: {
        /**
         * Retourne une res concaténée de la valeur recherchée.
         * @param {string} numRes la res que l'on souhaite.
         * @returns {string} la valeur concaténée
         */
        GetResValue: function(numRes) {
            return numRes.concat(' ', this.SearchValue);
        },

        /**
         * Filtre égal à
         * */
        equalFilter: function () {
            this.updatePref(Operator.OP_EQUAL + ";|;" + this.SearchValue);
        },
        /**
         * Filtre plus petit que
         * */
        lessThanFilter: function () {
            this.updatePref(Operator.OP_LESS + ";|;" + this.SearchValue);
        },

        /**
         * Filtre plus grand que
         * */
        greaterThanFilter: function () {
            this.updatePref(Operator.OP_GREATER + ";|;" + this.SearchValue);
        },
    },
    mounted() {
    },
};