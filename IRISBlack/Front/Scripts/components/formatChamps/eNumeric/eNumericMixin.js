import { eFileComponentsMixin } from '../../../mixins/eFileComponentsMixin.js?ver=803000'
import { RemoveBorderSuccessError, verifCharacter, verifComponent, focusInput, showTooltip, hideTooltip, showInformationIco, displayInformationIco } from '../../eComponentsMethods.js?ver=803000';
import { onUpdateCallback } from '../../eFieldEditorMethods.js?ver=803000';
import { PropType } from '../../../Enum.js?ver=803000';

/**
 * Mixin commune aux composants eNumeric.
 * */
export const eNumericMixin = {
    mixins: [eFileComponentsMixin],
    data() {
        return {
            messageError: top._res_2469,
            messageSuccess: "",
            modif: false,
            bRegExeSuccess: true,
            bDisplayPopup: false,
            bEmptyDisplayPopup: false,
            that: this,
            patternVerif: /^(\d|\ )+(\.|\,)?\d*$/i,
            inputHovered: false,
            //IsDisplayReadOnly: (this.propHead || this.dataInput.ReadOnly),
            focusIn: false,
            icon: false,
            PropType
        };
    },

    computed: {
        IsDisplayReadOnly: function () {
            return (this.propHead || this.dataInput.ReadOnly)
        }
    },
    components: {
        eAlertBox: () => import("../../modale/alertBox.js")
    },
    mounted() {
        this.displayInformationIco();
        Vue.nextTick(() => {
            if (this.dataInput.Required && this.dataInput.Value === "")
                verifCharacter(this.dataInput, this.that)
        });
    },
    methods: {
        displayInformationIco,
        showInformationIco,
        hideTooltip,
        showTooltip,
        focusInput,
        RemoveBorderSuccessError,
        verifComponent,

        onUpdateCallback,
        verifNumeric(event, that) {
            verifComponent(this.patternVerif, event, this.dataInput.Value, that, this.dataInput);
        }
    },
    props: ["dataInput", "propHead", "propListe", "propSignet", "propIndexRow", "propAssistant", "propDetail", "propAssistantNbIndex"],
}