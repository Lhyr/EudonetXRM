import { eMruComponentsMixin } from '../mixins/eMruComponentsMixin.js?ver=803000';
import { showTooltip, showTooltipObj } from '../methods/eComponentsMethods.js?ver=803000';
import { FieldType } from '../methods/Enum.min.js?ver=803000';
import { GetMruSearch } from '../methods/eMRUMethods.js?ver=803000'

export const eMruFooterMixin = {
    mixins: [eMruComponentsMixin],
    data() {
        return {
            bDisplayLoadAllOption: false,
            bDisplayAddOption: false,
            displayAllOptionFieldTypes: [FieldType.Catalog, FieldType.User],
            displayAddOptionFieldTypes: [FieldType.Relation, FieldType.AliasRelation],
        };
    },
    methods: {
        GetMruSearch,
        showTooltipObj,
        openSpecificDialog: function (event) {
            this.$emit('openSpecificDialog', event);
        },
        addMethod: function (event) {
            this.$emit('addMethod', event);
        },
        loadAllValues: async function () {
            let mru = await this.GetMruSearch(this.dataInput, "");
            this.$emit('UpdateMruData', mru);
        },
        hoveringElem: function (event) {
            showTooltipObj({ visible: true, elem: 'allMru', icon: false, readonly: false, data: this.dataInput, allMru: true, label: this.getRes(6211), event: event, ctx: this });
        }
    },
    created() {
        this.bDisplayLoadAllOption = this.displayAllOptionFieldTypes.indexOf(this.dataInput.Format) > -1;
        this.bDisplayAddOption = this.displayAddOptionFieldTypes.indexOf(this.dataInput.Format) > -1;
    },
    props: ['dataInput']
}