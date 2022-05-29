import { dynamicFormatChamps } from '../../../index.js?ver=803000';
import { eModalMixin } from '../../mixins/eModalMixin.js?ver=803000';

export default {
    name: "MotherOfAllModals",
    data: function () {
        return {
            ctx: this,
            modalAlert:false
        }
    },
    components: {
        eDropdown: () => import(AddUrlTimeStampJS("../subComponents/eDropdown.js")),
        eAlertBox: () => import(AddUrlTimeStampJS("./alertBox.js")),
    },
    mounted() {
        if (this.$refs.relationtitle) {
            this.$refs.relationtitle.focus();
        }
    },   
    methods: {
        dynamicFormatChamps,
        /**
         * permet de rendre la popup transparente.
         * @param {any} bTransparent oui/non transparence.
         */
        setTransparent: function (bTransparent) {
            this.$el.style.opacity = bTransparent ? "0.1" : "1";
        }
    },
    computed: {
        /**
         * Retourne une grille avec le nombre de colonnes 
         * pour les boutons du footer.
         * @return {string} le grid template avec les colonnes en 1fr.
         * */
        gridFooterStyle() {
            return "grid-template-columns: repeat(" + this.propOptionsModal.footer.btn.length + ", 1fr)";
        },
        /**
         * Retourne une grille avec le nombre de colonnes 
         * pour les boutons du header.
         * @return {string} le grid template avec les colonnes en 1fr.
         * */
        gridHeaderStyle() {
            return "grid-template-columns: repeat(" + this.propOptionsModal.header.btn.length + ", 1fr)";
        },

        /**
         * Retourne une grille avec le nombre de colonnes
         * pour les composants du main.
         * @return {string} le grid template avec les colonnes en 1fr.
         * */
        gridMainComponentsStyle() {
            return "grid-template-columns: repeat(" + this.propOptionsModal.main.lstComponents.length + ", 1fr)";
        }
    },
    props: {
        /** un modèle de la props qu'on doit recevoir... */
        propOptionsModal: {
            id: "MotherOfAllModals",
            style: { height: "", width: "" },
            class: "",
            actions: [],
            header: {
                text: "MotherOfAllModals",
                class: "",
                btn: [
                    { class: "", action: () => { } },
                    { class: "", action: () => { } }
                ]
            },
            main: {
                class: "",
                componentsClass: "",
                lstComponents: [
                    { input: undefined, class:""},
                ],
                title: undefined,
                dropdown:{ elem: undefined, class: "", opt: undefined, optionSelected: undefined}
            },
            footer: {
                class: "",
                btn: [
                    { title: "", class: "", action: () => { } },
                    { title: "", class: "", action: () => { } }
                ]
            }
        },
    },
    mixins: [eModalMixin],
    template: `
    <section ref="mothermodal" v-if="propOptionsModal" 
            :id="propOptionsModal.id" 
            :style="{'width':propOptionsModal.style.width}" 
            :class="propOptionsModal.class">
        <header v-if="propOptionsModal.header" 
            class="grid-container" 
            :class="propOptionsModal.header.class">
            <div class="titleDisplay">
                {{propOptionsModal.header.text}}
            </div>
            <div v-if="propOptionsModal.header.btn && propOptionsModal.header.btn.length > 0" class="titleButtons" :style="gridHeaderStyle">
                <button @click="btn.action" :class="btn.class" v-for="btn in propOptionsModal.header.btn">{{btn.title}}</button>                    
            </div>
        </header>
        <main v-if="propOptionsModal.main" :class="propOptionsModal.main.class">
            <slot name="header">

            </slot>
            <div v-if="propOptionsModal.main.lstComponents && propOptionsModal.main.lstComponents.length > 0"
                 v-for="cpt in propOptionsModal.main.lstComponents"
                 :class="propOptionsModal.main.componentsClass"
                 :style="gridMainComponentsStyle">
                <component ref="fields" :prop-detail="true" :data-input="cpt.input" :cpt-from-modal="true" :is="dynamicFormatChamps(cpt.input)" :class="cpt.class"></component>
            </div>
            <div v-if="false">
              <!-- <div v-if="propOptionsModal.main.dropdown" :class="propOptionsModal.main.componentsClass">
                    <h3 tabindex="0" ref="relationtitle" class="relation-title">{{propOptionsModal.main.title}}</h3>
                    <eDropdown v-if="propOptionsModal.main.dropdown && propOptionsModal.main.dropdown.opt.length > 1" :prop-dropdown="propOptionsModal.main.dropdown" :handle-outside-click="true" >
                    </eDropdown>
		            <eAlertBox :multiple="propOptionsModal.main.alert" :warning="true" v-if="modalAlert" >
		            </eAlertBox>
               </div>-->
            </div>
            <slot name="footer">
                
            </slot>
        </main>
        <footer v-if="propOptionsModal.footer"  class="grid-container" :class="propOptionsModal.footer.class" :style="gridFooterStyle">
            <button tabindex="0" v-if="propOptionsModal.footer.btn && propOptionsModal.footer.btn.length > 0" 
                @click="btn.action(ctx)" :class="btn.class"
                v-for="btn in propOptionsModal.footer.btn">{{btn.title}}</button>
        </footer>
    </section>
`
};