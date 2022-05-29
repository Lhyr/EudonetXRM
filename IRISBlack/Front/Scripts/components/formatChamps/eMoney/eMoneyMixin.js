import { eFileComponentsMixin } from '../../../mixins/eFileComponentsMixin.js?ver=803000'
import { updateMethod, RemoveBorderSuccessError, verifCharacter, focusInput, verifComponent, showTooltip, hideTooltip, updateListVal, verifyValChanged, showInformationIco, displayInformationIco } from '../../eComponentsMethods.js?ver=803000';
import { onUpdateCallback } from '../../eFieldEditorMethods.js?ver=803000';
import { PropType } from '../../../Enum.js?ver=803000';

/**
 * Mixin commune aux composants eMoney.
 * */
export const eMoneyMixin = {
    mixins: [eFileComponentsMixin],
    data() {
        return {
            bEmptyDisplayPopup: false,
            that: this,
            modif: false,
            //IsDisplayReadOnly: (this.propHead || this.dataInput.ReadOnly),
            focusIn: false,
            icon: false,
            patternVerif: /^(\d|\ )+(\.|\,)?\d*$/i,
            bRegExeSuccess: true,
            messageError: this.getRes(2469),
            PropType
        };
    },
    components: {
        eAlertBox: () => import(AddUrlTimeStampJS("../../modale/alertBox.js"))
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
        focusInput,
        verifyValChanged,
        hideTooltip,
        updateListVal,
        showTooltip,
        onUpdateCallback,
        RemoveBorderSuccessError,
        updateMethod,
        verifComponent,
        verifMoney(event, that) {
            verifComponent(this.patternVerif, event, this.dataInput.Value, that, this.dataInput);
        }
    },
    computed: {
        IsDisplayReadOnly: function () {
            return (this.propHead || this.dataInput.ReadOnly)
        }
    },
    props: ["dataInput", "propHead", "propListe", "propSignet", "propIndexRow", "propAssistant", "propDetail", "propAssistantNbIndex"],
}