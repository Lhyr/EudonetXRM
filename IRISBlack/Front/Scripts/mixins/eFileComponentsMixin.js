import { eMotherClassMixin } from './eMotherClassMixin.js?ver=803000';
import { PropType, caseFormat } from '../methods/Enum.min.js?ver=803000';
import { focusInput, AddBorderSuccess } from '../methods/eComponentsMethods.js?ver=803000';
import containerModal from '../components/modale/containerModal.js?ver=803000';
import { store } from '../store/store.js?ver=803000';

/**
 * Mixin commune aux composants des fiches (mais pas aux listes).
 * */
export const eFileComponentsMixin = {
    data() {
        return {
            modif: false,
            bRegExeSuccess: true,
            caseFormat
        }
    },
    mixins: [eMotherClassMixin],
    components: {
    },
    created() {
    },
    computed: {
        /**
         * Récupère la couleur du libellé définie en admin
         * */
        getCustomForeColor() {
            return this.dataInput.StyleForeColor
        },
        /**
        * Récupère les formats d'affichage du libellé définis en admin pour les bordures ( séparateur )
        * */
        getCssClass() {
            return {
                'italicLabel': this.dataInput.Italic,
                'boldLabel': this.dataInput.Bold,
                'underLineLabel': this.dataInput.Underline,
                'labelHidden': this.dataInput.LabelHidden,
                'text-truncate':true
            }
        },
        /**
        * Récupère le titre à afficher 
        * */
        getTitle() {
            return !this.dataInput.ToolTipText ? this.dataInput.Label : this.dataInput.ToolTipText
        },
        /** met en forme la case en fonction du format retourné par le back */
        getCaseFormat() {
            return this.caseFormat[this.dataInput.DisplayFormat];
        },
    },
    methods: {
        AddBorderSuccess,
        
        /** Remonte l'apel du spinner */
        eWaiter: function (oValue) {
            this.$emit("setWaitIris", oValue);
        },

        /* Câble la fonction déclenchée au clic droit sur le champ
         * Dans notre cas, la fonction affichant la fenêtre "Infos Debug" si la touche Ctrl est maintenue appuyée.
         */
        setContextMenu() {
            this.$el.oncontextmenu = (function (oEvent) {
                return function (oEvent) {
                    showttid(oEvent.currentTarget, event);
                }
            })(this.$el);
        },
        askForAnOption(data = this.dataInput) {
            //Si rubrique vide on rentre en edit
            if (data?.Value == "") {
                this.modif = true;
                focusInput("phone", {
                    props: this.propAssistant ? PropType.Assistant : this.propDetail ? PropType.Detail : this.propListe ? PropType.Liste : PropType.Defaut,
                    propAssistantNbIndex: this.propAssistantNbIndex,
                    propIndexRow: this.propIndexRow,
                    dataInput: data,
                    propSignet: this.propSignet
                });
                return;
            }
               
            //Si l'option SMS n'est pas dispo on lance l'appel directement
            if (!data?.DisplaySmsBtn) {
                this.launchCall(data);
                return;
            }
            
            //autrement il faut proposer le choix à l'utilisateur
            this.emitPhoneActionModale(data);

        },
        launchCall(data = this.dataInput) {
            let phoneNum = data?.Value || this.dataInput?.Value;
            window.open("tel:" + phoneNum);
        },
        emitPhoneActionModale(data = this.dataInput) {
            let modalTitle = this.getRes(928); //Que voulez-vous faire?

            let options = {
                id: "PhoneActionModal",
                class: "modal-motherofAll",
                style: {
                    width: document.querySelector('#MainWrapper') ? document.querySelector('#MainWrapper').offsetWidth + "px" : ""
                },
                actions: [],
                header: {
                    text: `${this.getRes(657)} : ${data?.Label}`,
                    class: "modal-header-motherofAll modal-header-motherofAll-Max relation-modal",
                    btn: [
                        {
                            name: 'close', 
                            class: "icon-edn-cross titleButtonsAlignement", 
                            action: this.closePhoneActionModale
                        }
                    ]
                },
                main: {
                    class: "detailContent modal-content-motherofAll modal-content-motherofAll-Max relation-modal",
                    componentsClass: "grid-container form-group relation-container",
                    title: modalTitle,
                    icon:"phone-options-icon",
                    links: [
                        /*Appeler*/
                        { 
                            label: this.getRes(2651),
                            action: this.launchCall
                        }, 
                        /*envoyer un sms*/
                        { 
                            label: this.getRes(1893),
                            action: this.openSmsForm
                        }

                    ],
                    alert: [
                        {
                            value: "",
                            class: "fas fa-exclamation-circle"
                        },
                        {
                            value: "",
                            class: "alert-content"
                        }
                    ]
                },
                footer: {
                    class: "modal-footer-motherofAll modal-footer-motherofAll-Max relation-modal",
                    btn: [
                        {
                            title: this.getRes(29), class: "btncancel eudo-button btn btn-default"
                            , action: this.closePhoneActionModale
                        }
                    ]
                },
            };
            this.modalOptions = options;

            var ComponentClass = Vue.extend(containerModal);
            this.instance = new ComponentClass({
                propsData: { 'propOptionsModal': options },
                store: store
            })
            this.instance.$mount();
            this.$root.$children.find(x => x.$options.name == "App").$el.appendChild(this.instance.$el);
        },
        closePhoneActionModale() {
            if (!this.instance)
                return;
            this.instance.$destroy();
            [...this.$root.$children.find(x => x.$options.name == "App").$el.children].find(x => x.className == "containerModal") ? this.$root.$children.find(x => x.$options.name == "App").$el.removeChild(this.instance.$el) : '';
        },
        openSmsForm(data = this.dataInput) {
            let phoneNum = data?.Value || this.dataInput?.Value;
            var objParentInfo = { parentTab: this.getTab, parentFileId: this.getFileId }
            top.selectFileMail(getParamWindow().document.getElementById("SMSFiles"), phoneNum, objParentInfo, TypeMailing.SMS_MAILING_UNDEFINED);
            this.closePhoneActionModale();
        },
        /**
         * permet de focus le input quand on click sur le crayon
         * @param {any} evt l'événement
         */
        goEmailPopup(evt) {

            if (!evt.target.classList.contains("targetIsTrue") || this.dataInput.Value == "") {
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
        },
        /**
         * Ferme le menu MRU d'un composant catalogue et le marque comme mis à jour avec les options par défaut, si demandé
         * @param {boolean} Si true, ajoute le liséré/indicateur/bordure vert(e) pour indiquer qu'une MAJ en base s'est faite
         */
        closeMru(bFlagAsSuccess) {
            this.showMru = false;
            if (bFlagAsSuccess) {
                this.AddBorderSuccess();
            }
        },
    }
}