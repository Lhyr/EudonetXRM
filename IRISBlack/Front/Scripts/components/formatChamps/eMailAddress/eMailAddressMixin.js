import { eFileComponentsMixin } from '../../../mixins/eFileComponentsMixin.js?ver=803000'
import { focusInput, RemoveBorderSuccessError, verifComponent, verifCharacter, showTooltip, hideTooltip, showInformationIco, displayInformationIco } from '../../eComponentsMethods.js?ver=803000';
import { onUpdateCallback } from '../../eFieldEditorMethods.js?ver=803000';
import { PropType } from '../../../Enum.js?ver=803000';

/**
 * Mixin commune aux composants eMailAddress.
 * */
export const eMailAddressMixin = {
    mixins: [eFileComponentsMixin],
    data() {
        return {
            messageError: this.getRes(2453),
            messageSuccess: this.getRes(2462),
            modif: false,
            //IsDisplayReadOnly: (this.propHead || this.dataInput.ReadOnly),
            bRegExeSuccess: true,
            bDisplayPopup: false,
            bEmptyDisplayPopup: false,
            patternVerif: /^[a-zA-Z0-9.!#$%&'*+/=?^_\`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)+$/i,
            that: this,
            icon: false,
            focusIn: false
        };
    },
    mounted() {
        this.displayInformationIco();
        Vue.nextTick(() => {
            if (this.dataInput.Required && this.dataInput.Value === "")
                verifCharacter(this.dataInput, this.that)
        });
    },
    computed: {
        IsDisplayReadOnly: function () {
            return (this.propHead || this.dataInput.ReadOnly)
        },

        msgHover: function () {
            if (this.icon)
                return this.getRes(6184)
            else if (!this.icon && this.IsDisplayReadOnly)
                return this.getRes(2477)
            else
                //return "Modifier l'adresse email"
                return this.getRes(7393)
        }
    },
    components: {
        eAlertBox: () => import(AddUrlTimeStampJS("../../modale/alertBox.js"))
    },
    methods: {
        displayInformationIco,
        showInformationIco,
        hideTooltip,
        showTooltip,
        RemoveBorderSuccessError,
        verifCharacter,
        onUpdateCallback,
        verifComponent,
        /**
         * Fonction qui va chainer les vérif en commencant par la pattern,
         * et en terminant par vide, obligatoire ou non. G.L 
         * @param  {any} event l'événement
         * @param  {any} that le contexte appelant.
         * */
        verifEmail(event, that) {
            verifComponent(this.patternVerif, event, this.dataInput.Value, that, that.dataInput);
        },

        /**
         * permet de focus le input quand on click sur le crayon
         * @param {any} evt l'événement
         */
        goAction(evt) {

            if (!evt.target.classList.contains("targetIsTrue")) {
                this.modif = true;
                this.bRegExeSuccess = false;
                focusInput("mailAdress", {
                    props: this.propAssistant ? PropType.Assistant : this.propDetail ? PropType.Detail : this.propListe ? PropType.Liste : PropType.Defaut,
                    propAssistantNbIndex: this.propAssistantNbIndex,
                    propIndexRow: this.propIndexRow,
                    dataInput: this.dataInput,
                    propSignet: this.propSignet
                });
            } else {
                var objParentInfo = { parentTab: this.getTab, parentFileId: this.getFileId }
                top.selectFileMail(getParamWindow().document.getElementById("MLFiles"), this.dataInput.Value, objParentInfo, TypeMailing.MAILING_UNDEFINED);
            }
        }
    },
    props: ["dataInput", "propHead", "propListe", "propSignet", "propIndexRow", "propAssistant", "propDetail", "propAssistantNbIndex"],
}