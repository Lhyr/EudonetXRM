import { eExpressFilterMixin } from '../../mixins/eExpressFilterMixin.js?ver=803000';
import { FieldType, Operator } from '../../methods/Enum.min.js?ver=803000';

export default {
    name: "expressFilterLogique",
    data() {
        return {
            checked: true,
            FieldType: FieldType,
            top: 0
        };
    },
    mixins: [eExpressFilterMixin],
    template: `
<div :format="this.propData.datas.Format" class="filterContent" v-bind:style="{ top: this.top, left: this.propData.posLeft + 'px' }">
    <ul class="filter-list">
        <!-- <li v-on:click="logicFilter('1','change')"> -->
        <li v-on:click="searchFromValue(1)">
            <span :title="getRes(183)" class="fas fa-check"></span>
            <span >{{propData.datas.Format == FieldType.Button ? getRes(7845) : getRes(2011)}}</span>
        </li>
        <!--<li v-on:click="logicFilter('0','change')">-->
        <li v-on:click="searchFromValue(0)">
            <span :title="getRes(183)" class="fas fa-times"></span>
            <span >{{propData.datas.Format == FieldType.Button ? getRes(2244) : getRes(2012)}}</span>
        </li>
    </ul>
    <ul class="filter-actions" >
        <!-- <li v-on:click="logicFilter(null,'cancel')">-->
        <li v-on:click="updatePref(';|;$cancelthisfilter$')">
            <span class="actionItem icon-rem_filter" :title="getRes(183)"></span>
            <span>{{getRes(183)}}</span>
        </li>
        <li v-on:click="doLogicChart()"><span class="actionItem icon-stats" :title="getRes(1395)"></span><span>{{getRes(1395)}}</span></li>
    </ul>
</div>
`,
    methods: {
        //doLogicChart() {
        //    let nTabType = this.propData.datas.Format == FieldType.Button ? '25' : '3';
        //    var expressFilter = new eExpressFilter("expressFilter", this.propData.datas.DescId, nTabType,this.filterTab[0].signets[0].DescId, this.filterTab[0].signets[0].TableType, this.getTab, "","", null);

        //    doStats(expressFilter);
        //    //modalDialog.addButton(this.getRes(30), function () { modalDialog.hide(); }, 'button-gray', null);

        //    //modalDialog.MaxOrMinModal();

        //    //this.res.eExpressFilter.hide();

        //},
        logicFilter(val,action) {
            if (action != 'cancel') {
                this.updatePref(`${Operator.OP_EQUAL};|;${val}`);
            } else {
                this.updatePref(`;|;$cancelthisfilter$`);
            }
        }
    },
    async mounted() {
    },
};