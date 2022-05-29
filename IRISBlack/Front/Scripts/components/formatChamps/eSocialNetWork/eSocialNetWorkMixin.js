import { eFileComponentsMixin } from '../../../mixins/eFileComponentsMixin.js?ver=803000'
import { updateMethod, focusInput, RemoveBorderSuccessError, verifComponent, verifCharacter, showTooltip, hideTooltip, updateListVal, showInformationIco, displayInformationIco } from '../../eComponentsMethods.js?ver=803000';
import { onUpdateCallback } from '../../eFieldEditorMethods.js?ver=803000';
import { PropType } from '../../../Enum.js?ver=803000';

/**
 * Mixin commune aux composants eSocialNetwork.
 * */
export const eSocialNetworkMixin = {
    mixins: [eFileComponentsMixin],
    data() {
        return {
            modif: false,
            bDisplayPopup: false,
            bEmptyDisplayPopup: false,
            that: this,
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
        updateListVal,
        hideTooltip,
        showTooltip,
        updateMethod,
        RemoveBorderSuccessError,
        verifCharacter,
        verifComponent,
        focusInput,
        onUpdateCallback,
        // permet de focus le input quand on click sur le crayon

        goAction(evt) {
            if (!evt.target.classList.contains("targetIsTrue")) {
                this.modif = true;
                focusInput("socialNetWork", {
                    props: this.propAssistant ? PropType.Assistant : this.propDetail ? PropType.Detail : this.propListe ? PropType.Liste : PropType.Defaut,
                    propAssistantNbIndex: this.propAssistantNbIndex,
                    propIndexRow: this.propIndexRow,
                    dataInput: this.dataInput,
                    propSignet: this.propSignet
                });
            }
        },
        verifSocial(event, that) {
            verifComponent(undefined, event, this.dataInput.Value, that, this.dataInput);
        }
    },
    props: ["dataInput", "propHead", "propListe", "propSignet", "propIndexRow", "propAssistant", "propDetail", "propAssistantNbIndex"],
}