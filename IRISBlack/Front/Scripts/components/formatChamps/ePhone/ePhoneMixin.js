import { eFileComponentsMixin } from '../../../mixins/eFileComponentsMixin.js?ver=803000'
import { focusInput, RemoveBorderSuccessError, verifComponent, verifCharacter, showTooltip, hideTooltip, updateMethod, updateListVal, showInformationIco, displayInformationIco } from '../../eComponentsMethods.js?ver=803000';
import { onUpdateCallback } from '../../eFieldEditorMethods.js?ver=803000';
import { PropType } from '../../../Enum.js?ver=803000';

/**
 * Mixin commune aux composants ePhone.
 * */
export const ePhoneMixin = {
    mixins: [eFileComponentsMixin],
    data() {
        return {
            PropType,
            messageError: this.getRes(2246),
            messageSuccess: "",
            modif: false,
            bRegExeSuccess: true,
            bDisplayPopup: false,
            //IsDisplayReadOnly: (this.propHead || this.dataInput.ReadOnly),
            bEmptyDisplayPopup: false,
            that: this,
            patternVerif: /^[\-.+()# 0-9]*$/i,
            icon: false,
            focusIn: false,
            msgHover: '',
            that: this
        };
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
    computed: {
        IsDisplayReadOnly: function () {
            return (this.propHead || this.dataInput.ReadOnly)
        },
        //msgHover: function () {
        //    if (this.icon && this.dataInput.DisplaySmsBtn)
        //        return 'Envoyer un SMS'
        //    else if (this.icon && !this.dataInput.DisplaySmsBtn)
        //        return 'Appeler un numéro'
        //    else if (!this.icon && this.IsDisplayReadOnly)
        //        return this.getRes(2477)
        //    else
        //        return this.getRes(7393)
        //}
    },
    methods: {
        displayInformationIco,
        showInformationIco,
        //onBlur(event,dis) {
        //    verifCharacter(event, this,dis);
        //},
        updateListVal,
        updateMethod,
        hideTooltip,
        showTooltip,
        focusInput,
        verifCharacter,
        onUpdateCallback,
        verifComponent,
        // permet de focus le input quand on click sur le crayon

        goAction(evt) {
            if (!evt.target.classList.contains("targetIsTrue")) {
                this.modif = true;
                focusInput("phone", {
                    props: this.propAssistant ? PropType.Assistant : this.propDetail ? PropType.Detail : this.propListe ? PropType.Liste : PropType.Defaut,
                    propAssistantNbIndex: this.propAssistantNbIndex,
                    propIndexRow: this.propIndexRow,
                    dataInput: this.dataInput,
                    propSignet: this.propSignet
                });
            } else {
                var objParentInfo = { parentTab: this.getTab, parentFileId: this.getFileId }
                top.selectFileMail(getParamWindow().document.getElementById("SMSFiles"), this.dataInput.Value, objParentInfo, TypeMailing.SMS_MAILING_UNDEFINED);
            }
        },
        verifPhone(event, that) {
            verifComponent(this.patternVerif, event, this.dataInput.Value, that, this.dataInput);
        },
        RemoveBorderSuccessError
    },
    props: ["dataInput", "propHead", "propListe", "propSignet", "propIndexRow", "propAssistant", "propDetail", "propAssistantNbIndex"],
}