import { eMruComponentsMixin } from '../mixins/eMruComponentsMixin.js?ver=803000';
import { updateMethod, updateListVal, verifComponent, showCatalogGeneric, showTooltip, showTooltipObj, openUserDialog, AddBorderSuccess } from '../methods/eComponentsMethods.js?ver=803000';
import { selectValue, validateCatGenericIris, validateUserDialog, onUpdateCallback } from '../methods/eFieldEditorMethods.js?ver=803000';
import EventBus from '../bus/event-bus.js?ver=803000';
import { FieldType } from '../methods/Enum.min.js?ver=803000';
import { setMruParams } from '../shared/XRMWrapperModules.js?ver=803000';

export const eMruBodyMixin = {
    mixins: [eMruComponentsMixin],
    data() {
        return {
            selectedValues: new Array(),
            selectedLabels: new Array(),
            advancedDialog: null,
            formatField: FieldType
        };
    },
    methods: {
        showTooltip,
        showTooltipObj,
        updateMethod,
        setMruParams,
        validateCatGenericIris,
        validateUserDialog,
        updateListVal,
        verifComponent,
        showCatalogGeneric,
        selectValue,
        onUpdateCallback,
        openUserDialog,
        AddBorderSuccess,

        showCatalogGenericViewIris() {
            this.$parent.$emit('showCatalogGeneric')
        },
        notEmptyMru() {
            this.selectedValues = "";
            try {
                this.updateMethod(this, this.selectedValues, undefined, undefined, this.dataInput);
                this.$parent.$emit('closeMru');
            } catch (e) {
                EventBus.$emit('globalModal', {
                    typeModal: "alert", color: "danger", type: "zoom",
                    close: true, maximize: false, id: 'alert-modal', title: this.getRes(6576), msgData: e, width: 600,
                    btns: [{ lib: this.getRes(30), color: 'default', type: 'left' }], datas: this.getRes(7050)
                })
            }
        },
        selectLabelMru: async function(ev) {
            try {
                this.selectedValues = ev.DbValue != undefined ? ev.DbValue : ev.ItemCode != undefined ? ev.ItemCode : "";
                this.selectedLabels = ev.DisplayLabel != undefined ? ev.DisplayLabel : ev.Label != undefined ? ev.Label : "";
                this.$parent.$emit('newLabel', this.selectedLabels);
                let newV = { NewValue: this.selectedValues, NewDisplay: this.selectedLabels };
                let finalDtVal = {...this.dataInput,...{Value:this.selectedValues, DisplayValue:this.selectedLabels}};
                this.$emit('update:dataInput',finalDtVal);
                this.updateMethod(this, newV, undefined, undefined, this.dataInput);
                this.setMruParams(this.$parent.dataInput.TargetTab, this.selectedValues, this.selectedLabels);
                this.$parent.$emit('closeMru', true); //  // #96 281 - indicateur de MAJ en vert avec les options par défaut - true pour indiquer d'appeler AddBorderSuccess depuis eFileComponentsMixin.closeMru()
            } catch (e) {
                EventBus.$emit('globalModal', {
                    typeModal: "alert", color: "danger", type: "zoom",
                    close: true, maximize: false, id: 'alert-modal', title: this.getRes(6576), msgData: e, width: 600,
                    btns: [{ lib: this.getRes(30), color: 'default', type: 'left' }], datas: this.getRes(7050)
                });
            }
        },
        /** Remonte l'apel du spinner */
        eWaiter: function (oValue) {
            this.$emit("setWaitIris", oValue);
        },
    },
    computed: {
        IsMRUContent: function () {
            return this.DataMru && this.DataMru.length == 0;
        },
        EmptyFieldOptionRes: function () {
            // Dissocier pour les relations, non renseigné pour les autres
            //return [FieldType.Relation, FieldType.AliasRelation].indexOf(this.dataInput.Format) > -1 ? 6333 : 6211;

            //finalement c'est non renseigné pour tout le monde
            return 6211;
        },

    },

    props: ['DataMru', 'dataInput', 'propDataDetail']
}