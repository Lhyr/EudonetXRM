import { FieldType, expressFilterEnum } from '../../methods/Enum.min.js?ver=803000';
import expressFilterDate from "./expressFilterDate.js?ver=803000";
import expressFilterCaractere from "./expressFilterCaractere.js?ver=803000";
import expressFilterLogique from "./expressFilterLogique.js?ver=803000";
import expressFilterNumerique from "./expressFilterNumerique.js?ver=803000";



export default {
    name: "expressFilter",
    data() {
        return {
            expressFilterEnumLocal: expressFilterEnum
        };
    },
    props: ['propOptionsExpressFilter','filterPropTab'],
    template: `
        <component @closeFilterModal="closeModal()"
            :filter-tab="filterPropTab"
            :prop-data="propOptionsExpressFilter"
            :is="componentFormat(propOptionsExpressFilter.datas.Format)"
            :css-class="{'popup':true, 'control_contextmenu':true, 'fs_8pt':true}" 
            @click=""
            role="menu"></component>
`,

    methods: {
        handleOutsideClick(e) {
            e.stopPropagation();

            if (this.$el && !this.$el.contains(e.target)) {
                this.closeModal();
                document.removeEventListener('mousedown', this.handleOutsideClick);
            }
        },

        closeModal() {
            this.$emit('closeFilter');
        },

        componentFormat: function (eType) {
            if (!FieldType)
                return;
            switch (eType) {
                case FieldType.Undefined:
                case FieldType.Character:
                case FieldType.Catalog:
                case FieldType.MailAddress:
                case FieldType.Phone:
                case FieldType.Relation:
                case FieldType.SocialNetwork:
                case FieldType.AliasRelation:
                case FieldType.Geolocation: 
                case FieldType.HyperLink: 
                case FieldType.User: 
                case FieldType.Memo: return expressFilterCaractere;
                case FieldType.AutoCount:
                case FieldType.Numeric: 
                case FieldType.Money: return expressFilterNumerique;
                case FieldType.Button: 
                case FieldType.Logic: return expressFilterLogique;
                case FieldType.Date: return expressFilterDate;
                default: return expressFilterCaractere;
            }
        },
    },
    beforeDestroy() {
        var uxDataTable = document.getElementsByClassName("ux-data-table");
        for (var i = 0; i < uxDataTable.length; i++) {
            uxDataTable[i].removeEventListener('scroll', this.closeModal, false);
        }

        var MainWrapper = this.$parent.$parent.$refs.mainContentWrap;

        MainWrapper.removeEventListener('scroll', this.closeModal, false);

    },
    mounted() {
        document.addEventListener('mousedown', this.handleOutsideClick);

        var MainWrapper = this.$parent.$parent.$refs.mainContentWrap;
        MainWrapper.addEventListener('scroll', this.closeModal, false);

        var uxDataTable = document.getElementsByClassName("ux-data-table");
        for (var i = 0; i < uxDataTable.length; i++) {
            uxDataTable[i].addEventListener('scroll', this.closeModal, false);
        }
    },
    beforeDestroy() {
        document.removeEventListener('mousedown', this.handleOutsideClick);
    }
};