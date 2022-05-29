import { eModalMixin } from '../../mixins/eModalMixin.js?ver=803000';

export default {
    name: "alertModal",
    data() {
        return {
            animate: false,
            maximized: false,
            detailedInfo: false,
            detailedMessages: new Array(),
        };
    },
    mounted() {
        console.log(this.propOptionsModal)
    },
    components: {},
    props: ['propOptionsModal'],
    mixins: [eModalMixin],
    template: `
    <div :id="propOptionsModal.id">
            <div class="eudonetModal" v-bind:class="{'eudo-maximized': this.maximized,'eudo-out': !this.animate,'eudo-hidden': !this.animate,'eudo-in': this.animate, 'eudo-closable': propOptionsModal.close,'eudo-maximizable': propOptionsModal.maximize,'eudo-zoom': propOptionsModal.type =='zoom'  }">
                <div class="eudo-dimmer"></div>    
                <div  class="eudo-modal" v-bind:class="{ 'alertWarning': propOptionsModal.color == 'warning', 'alertInfo': propOptionsModal.color == 'info','alertDanger': propOptionsModal.color == 'danger', 'alertSuccess': propOptionsModal.color == 'success'}">
                    <div :style="'width:' + propOptionsModal.width + 'px'" class="eudo-dialog">
                        <div class="eudo-commands">
                            <button class="eudo-pin"></button>
                            <button @click="setmaximized" class="eudo-maximize"></button>
                            <button @click="closeModal" class="eudo-close"></button>
                        </div>
                        <div class="eudo-header">
                            <div class="eudo-title">
                                <h3 class="title-h3-list-user"><i class="fa" v-bind:class="{ 'fa-exclamation-circle': propOptionsModal.color == 'warning', 'fa-info-circle': propOptionsModal.color == 'info','fa-exclamation-triangle': propOptionsModal.color == 'danger', 'fa-check-circle': propOptionsModal.color == 'success'}"></i>{{propOptionsModal.title}}</h3>
                            </div>
                        </div>
                        <div class="eudo-body">
                            <div class="eudo-content">
                                <div class="msg-container" @mouseover.ctrl="displayDetailedInfo">
                                    <table>
                                        <tbody>
                                            <tr>
                                                <td class="td-logo">
                                                    <span v-bind:class="{ 'icon-exclamation-triangle logo-warn': propOptionsModal.color == 'warning', 'icon-info-circle logo-info': propOptionsModal.color == 'info','icon-times-circle  logo-error': propOptionsModal.color == 'danger', 'icon-times-circle  logo-success': propOptionsModal.color == 'success'}" class=""></span>
                                                </td>
                                                <td class="text-alert-info">{{propOptionsModal.datas}}</td>
                                            </tr>
                                            <tr v-if="propOptionsModal.msgData">
                                                <td class="text-msg-error">{{detailedMessagesSubtitle}}</td>
                                            </tr>                                            
                                            <tr v-if="propOptionsModal.msgData">
												<td :class="'text-msg-error' + detailedInfoAdditionalClass">
                                                    <textarea class="requestResponseDetail" readonly v-if="propOptionsModal.msgData?.request?.response">{{formatMessage(propOptionsModal.msgData.request.response)}}</textarea>
                                                    <textarea class="alertModalDetail" readonly>{{formatMessage([propOptionsModal.msgData.message, propOptionsModal.msgData.stack])}}</textarea>
												</td>
											</tr>
                                        </tbody>
                                    </table>
                                </div>
                            </div>
                        </div>
                        <div class="eudo-footer">
                            <div class="eudo-buttons" v-bind:class="{ 'eudo-auxiliary': btn.type == 'left','eudo-primary': btn.type == 'right'}" v-for="btn in propOptionsModal.btns">
                                <button @click="closeModal" :class="'eudo-button btn btn-'+ btn.color">{{btn.lib}}</button>
                            </div>
                        </div>
                    </div>
                </div>  
            </div>
        </div>
`,
    methods: {
        /**
         * Maximise la taille de la fenêtre
         */
        setmaximized() {
            if (this.maximized == true) {
                this.maximized = false
            } else {
                this.maximized = true
            }
        },
        /**
         * Ferme la fenêtre
         */
        closeModal() {
            document.body.style.overflow = null
            let that = this
            setTimeout(function() {
                that.animate = false;
            }, 100);

            setTimeout(function() {
                that.$emit('close');
            }, 600);
        },
        /**
         * Affiche le détail de l'erreur (lors du passage de la souris sur la popup avec la touche Ctrl enfoncée)
        */
        displayDetailedInfo() {
            this.detailedInfo = true;
        },
        /**
        * Formate un objet constitué de paires de clés/valeurs de façon à obtenir un descriptif lisible de type "Propriete1: Valeur1\nPropriete2: Valeur2\nPropriete3: Valeur3\n"
        * Utilisé pour formater les infos détaillées de l'erreur
        * @param {object} data Objet à afficher, de la forme { propriete1: valeur1, propriete2: valeur2, propriete3: valeur3 }
        */
        formatMessage(data) {
            let message = "";
            
            if (typeof (data) == "string") {
                try {
                    data = JSON.parse(data);
                }
                catch (e) {
                    message = "Data: " + data;
                }
            }
            if (typeof (data) == "object") {
                for (const [key, value] of Object.entries(data)) {
                    message += "\r\n" + key + ": " + (typeof (value) == "object" ? this.formatMessage(value) : value);
                    if (key.toLowerCase().indexOf("message") > -1 && typeof (value) == "string" && this.detailedMessages.indexOf(value) == -1)
                        this.detailedMessages.push(value);
                }
            }
            
            return message.trim();
        }
    },
    mounted() {
        document.body.style.overflow = "hidden"
        let that = this
        setTimeout(function() {
            that.animate = true;
        }, 100);
    },
    computed: {
        /**
         * Renvoie les classes CSS à appliquer sur l'élément affichant les infos détaillées, selon le contexte
        */
        detailedInfoAdditionalClass() {
            return !this.detailedInfo ? " displayHidden" : "";
        },
        /**
        * Formate la liste des messages "détaillés pour l'utilisateur" récupérés par la fonction formatMessage() pour les afficher en sous-titre de la popup
        */        
        detailedMessagesSubtitle() {
            if (this.detailedMessages.length > 0)
                return this.detailedMessages.join(" - ").trim();
            else
                return "";
        }        
    }
};