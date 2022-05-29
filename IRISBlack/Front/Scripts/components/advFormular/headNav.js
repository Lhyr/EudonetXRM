/**
 * Barre de navigation
 * */
export default {
    name: "headNav",
    data() {
        return {};
    },
    computed: {
        formularName() {
            return this.$store.state.formularName
        },
        wizardActiveTab() {
            return this.$store.state.wizardActivTab
        },

        isActiveTab() {
            return function (tabName) { return this.wizardActiveTab == tabName }
        },

        getRes() {
            return function (resid) { return this.$store.getters.getRes(resid) }
        }
    },

    mounted() {
        const input = this.$refs.formularName
        input.focus();
        input.select();
    },
    methods: {

        openTab(tabName) {
            this.$store.commit("setWizardActivTab", tabName);
        },

        updateFormularName(event) { this.$store.commit("setFormularName", event.target.value); },

        closeModal() {
            closeFormularModal();
        },
        saveFormular() {
            this.$store.commit("formularStore/setFormularTS")
            SaveFormular({
                formularName: this.$store.state.formularName,
                url: this.$store.state.url,
                nFormularId: this.$store.state.nFormularId,
                tab: this.$store.state.tab,
                isNewPublication: false
            });
        },

    },
    template: `
    <div id="headNav" ref="headNav">
        <div class="buttonGlobal inputBtn">
            <input id="formularName" ref="formularName" :value="formularName" @input="updateFormularName" ><span><i class="fas fa-pencil-alt"></i></span>
        </div>
        <div class="contentTabHead">
            <div class="tab">
                <button v-bind:class="{'tablinks':true,'active' : isActiveTab('CreatGraphique')}" v-on:click="openTab('CreatGraphique')">{{getRes(2227,'Création Graphique')}}</button>
                <button v-bind:class="{'tablinks':true,'active' : isActiveTab('Param')}" v-on:click="openTab('Param')">{{getRes(181,'Paramètres')}}</button>
                <button v-bind:class="{'tablinks':true,'active' : isActiveTab('Publication')}" v-on:click="openTab('Publication')">{{getRes(2581,'Publication')}}</button>
            </div>
        </div>
        <div class="buttonGlobal">
            <button class="btnHead close-btn" v-on:click="closeModal()">{{getRes(30,'Fermer')}}</button>
        </div> 
        <div class="buttonGlobal save-btn">
            <button class="btnHead btn-success" v-on:click="saveFormular()">{{getRes(286,'Enregistrer')}}</button>
        </div>
    </div>
`,

};