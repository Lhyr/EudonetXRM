import { eFileComponentsMixin } from '../../../mixins/eFileComponentsMixin.js?ver=803000'
import { selectValue, onUpdateCallback, cancelAdvancedDialog, validateUserDialog } from '../../eFieldEditorMethods.js?ver=803000';
import { openUserDialog, updateMethod, showTooltip, hideTooltip, updateListVal, showInformationIco, displayInformationIco } from '../../eComponentsMethods.js?ver=803000';

/**
 * Mixin commune aux composants eUser.
 * */
export const eUserMixin = {
    mixins: [eFileComponentsMixin],
    data() {
        return {
            selectedValues: null,
            selectedLabels: null,
            advancedDialog: null,
            modif: false,
        };
    },
    computed: {
        IsDisplayReadOnly: function () {
            return (this.propHead || this.dataInput.ReadOnly)
        },

        valueMultiple: function () {

            //if (this.dataInput.DisplayValue == "")
            //    return "";

            //return this.dataInput.Multiple ? / *; */g[Symbol.split](this.dataInput.DisplayValue) : this.dataInput.DisplayValue;

            if (this.dataInput.DisplayValue == "")
                return [];

            if (!this.dataInput.Multiple)
                return { id: this.dataInput.Value, value: this.dataInput.DisplayValue };

            let val = / *; */g[Symbol.split](this.dataInput.Value);
            let dispVal = / *; */g[Symbol.split](this.dataInput.DisplayValue);

            let res = val.map(function (e, i) {
                return { id: e, value: dispVal[i] };
            });

            return res;

        }
    },
    mounted() {
        this.displayInformationIco();
    },
    beforeDestroy() { },
    methods: {
        displayInformationIco,
        showInformationIco,
        hideTooltip,
        showTooltip,
        selectValue,
        onUpdateCallback,
        openUserDialog,
        cancelAdvancedDialog,
        validateUserDialog,
        updateMethod,
        updateListVal,
        /**
         * Va rechercher la fonction d'ouverture des dialogues
         * des utilisateurs.
         * */
        openDial: async function () {
            var that = this;
            this.openUserDialog(
                this.dataInput.DescId,
                this.dataInput.Label,
                this.dataInput.FullUserList,
                this.dataInput.Multiple,
                this.cancelAdvancedDialog,
                () => {
                    this.validateUserDialog(() => updateMethod(this, this.selectedValues.join(';'), undefined, undefined, this.dataInput));

                    //document.querySelectorAll(`div[fileid='${this.dataInput.FileId}'][divdescid='${this.dataInput.DescId}']`).forEach(element => {
                    //    element.querySelector('.targetIsTrue').innerText = this.selectedLabels;
                    //    element.classList.add('border-success');
                    //    window.setTimeout(() => {
                    //        element.classList.remove('border-success');
                    //    },1000)
                    //});
                    //updateListVal(that, '.targetIsTrue');
                },
                this.dataInput.Value,
                this.dataInput.DisplayValue
            );
            //var options = {
            //    signet: this.propSignet.DescId,
            //};
            //EventBus.$emit('reloadSignet', options);
        }

    },
    props: ["dataInput", "propHead", "propListe", "propSignet", "propIndexRow", "propAssistant", "propDetail", "propAssistantNbIndex"],
}