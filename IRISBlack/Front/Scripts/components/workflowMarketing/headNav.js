import { eWorkflowMixin } from './mixin/eWorkflowMixin.js?ver=803000'
/**
 * Barre de navigation
 * */
export default {
    name: "headNav",
    data: function () {
        return {
           
        }
    },
    mixins: [eWorkflowMixin],
    watch: {
        switchActiveScenario: async function (newActivate, oldActivate) {
            if (newActivate && !oldActivate) {
                await ActiveScenario({
                    url: this.$store.state.url,
                    nWorflowId: this.$store.state.WorkflowId,
                    tab: this.$store.state.Tab
                },
                    (bActivated, bShowAlert, errorList) => {

                        this.$store.commit("setActivateScenario", bActivated);
                        this.$store.commit("setShowAlertScenario", bShowAlert);
                        let errorDescription = '';
                        if (!bActivated && !bShowAlert && errorList) {
                            for (var error in errorList) {
                                errorDescription += '##' + errorList[error] ;
                                var elem = document.querySelector('input[value="' + error + '"][name="blockid"]');
                                if (elem)
                                    elem.parentElement.classList.add("blockActionError");
                            }

                            this.$store.commit("setShowAlert", true);
                            this.$store.commit("setAlertMessage", top._res_8888 + " " + errorDescription);
                            this.$store.commit("setAlertType", "error");
                            this.$store.commit("setAlertTopScenario", "alertTop");
                        }
                        return;
                    });
            }
            
           
        }     

    },
    computed: {
        wizardActiveTab() {
            return this.$store.state.wizardActivTab
        },

        getRes() {
            return function (resid) { return this.$store.getters.getRes(resid) }
        },
        getWorkflowLabel() {
             return this.$store.state.Label
        },
        isActive() {
            return this.$store.state.ActivateScenario;
        },
        switchActiveScenario: {
            get() {
                return this.$store.state.ActivateScenario;
            },
            set(newValue) {               
                this.$store.commit("setActivateScenario", newValue);
               
               
            }
        }
    },
    mounted() {
        const input = this.$refs.workflowName
        input.select();
    },
    methods: {
        saveWorkflow() {
            SaveWorkflow({
                url: this.$store.state.url,
                nWorflowId: this.$store.state.WorkflowId,
                tab: this.$store.state.Tab
            }, this);
        },
        updateWorkflowLabel(event) { this.$store.commit("setWorkflowLabel", event.target.value); },

        async SwitchDesactivate(switchActiveScenario) {           
            if (switchActiveScenario) { 
                await DeactiveScenario({
                    url: this.$store.state.url,
                    nWorflowId: this.$store.state.WorkflowId,
                    tab: this.$store.state.Tab
                },
                    (bActivated, bShowAlert,type, error) => {

                    
                        if (bActivated && bShowAlert && error) {
                            if (type == 'Warning') {
                                this.$store.commit("setShowDialogText", error);
                                this.$store.commit("setShowDialogAlert", true);
                            }
                            else if (bActivated && bShowAlert && error) {
                                this.$store.commit("setActivateScenario", bActivated);
                                this.$store.commit("setShowAlertScenario", bShowAlert);
                                this.$store.commit("setShowAlert", true);
                                this.$store.commit("setAlertMessage" + error);
                                this.$store.commit("setAlertType", "error");
                            }
                            
                            
                        }
                        else {
                            this.$store.commit("setShowAlert", true);
                            this.$store.commit("setAlertMessage", top._res_8908);
                            this.$store.commit("setAlertType", "success");
                            if (document.querySelector(".action").classList.contains("blockdisabled"))
                                document.querySelector(".action").classList.remove("blockdisabled");
                        }
                        return;

                    });                
                this.$store.commit("setShowAlertScenario", false);
                

            }
        }


    },
    template: `
    <div id="headNav" ref="headNav" style='max-height:50px'>
        <div class="buttonGlobal inputBtn">
             <input id="workflowName" ref="workflowName":value="getWorkflowLabel" @input="updateWorkflowLabel"><span><i class="fas fa-pencil-alt"></i></span>
        </div>
        <div class="contentTabHead">            
            <div id="tabWorkflow" class="tab">
                <button v-bind:class="{'tablinks':true,'active' : true}" >{{getRes(8777,'Design')}}</button>
            </div>
        </div>
        <div id="scnearioSwitch">
            <label id="scnearioSwitchLabel">{{getRes(8835,'Design')}}</label>
             <v-switch v-model="switchActiveScenario" inset color="success" :value="isActive" v-on:change="SwitchDesactivate(!switchActiveScenario)"></v-switch>
        </div>
        <div class="buttonGlobal save-btn">
            <button :disabled="isActive" class="btnHead btn-success" v-on:click="saveWorkflow()" v-bind:class="[ isActive ? 'disabled' : '']">{{getRes(286,'Enregistrer')}}</button>
        </div>
        <div class="closeButton" id="closeWorkflow">
            <button class="icon-edn-cross" v-on:click="closeModal()"></button>
        </div> 
    </div>
`,

};