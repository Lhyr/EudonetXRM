import { eFileComponentsMixin } from '../../../mixins/eFileComponentsMixin.js?ver=803000'
import { focusInput, RemoveBorderSuccessError, verifComponent, verifCharacter, showTooltip, hideTooltip, updateListVal, showInformationIco, displayInformationIco } from '../../eComponentsMethods.js?ver=803000';
import { onUpdateCallback } from '../../eFieldEditorMethods.js?ver=803000';
import { PropType } from '../../../Enum.js?ver=803000';

/**
 * Mixin commune aux composants eHyperLink.
 * */
export const eHyperLinkMixin = {
    mixins: [eFileComponentsMixin],
    data() {
        return {
            modif: false,
            //IsDisplayReadOnly: (this.propHead || this.dataInput.ReadOnly),
            that: this,
            bEmptyDisplayPopup: false,
            bDisplayPopup: false,
            icon: false,
            focusIn: false,
            msgHover: ''
        };
    },


    computed: {
        IsDisplayReadOnly: function () {
            return (this.propHead || this.dataInput.ReadOnly)
        }
    },
    mounted() {
        this.displayInformationIco();
        Vue.nextTick(() => {
            if (this.dataInput.Required && this.dataInput.Value === "")
                verifCharacter(this.dataInput, this.that)
        });
    },
    components: {
        eAlertBox: () => import(AddUrlTimeStampJS("../../modale/alertBox.js"))
    },
    methods: {
        showInformationIco,
        displayInformationIco,
        showTooltip,
        updateListVal,
        hideTooltip,
        onUpdateCallback,
        verifComponent,
        // permet de focus le input quand on click sur le crayon

        goAction(evt) {
            if (!evt.target.classList.contains("targetIsTrue")) {
                this.modif = true
                focusInput('hyperLink', {
                    props: this.propAssistant ? PropType.Assistant : this.propDetail ? PropType.Detail : this.propListe ? PropType.Liste : PropType.Defaut,
                    propAssistantNbIndex: this.propAssistantNbIndex,
                    propIndexRow: this.propIndexRow,
                    dataInput: this.dataInput,
                    propSignet: this.propSignet
                });
            }
        },
        RemoveBorderSuccessError,

        verifHyperlinks(event, that) {
            verifComponent(undefined, event, this.dataInput.Value, that, this.dataInput);
        }
    },
    props: ["dataInput", "propHead", "propListe", "propSignet", "propIndexRow", "propAssistant", "propDetail", "propAssistantNbIndex"],
}