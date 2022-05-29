/**
 *  Application principale
 * 
 * 
 */
const headNav = () =>   import ("./headNav.js");
const contentTab = () =>   import ("./contentTab.js")

export default {
    name: "App_assist_workflow",
    data() {
        return {
            ActiveTab: "",
            timeout: 10000
        };
    },
    components: {
        headNav,
        contentTab
    },
    methods: {
        closeWithoutSaving() {
            this.showDialog = false;
            let cmapignId = null;
            let campaignInfos = actionInfos[this.$store.state.CurrentBlockId];
            if (campaignInfos) {
                cmapignId = campaignInfos.campaignId;
                if (this.$store.state.ActivateScenario)
                    top.loadFile(106000, cmapignId);
            }
            closeWorkflowModal();
        },
        close() {
            this.showDialog = false;
        },
        closeDialog() {
            this.$store.commit("setActivateScenario", true);
            this.$store.commit("setShowAlertScenario", true);
            this.$store.commit("setShowDialogAlert", false);
        },
        async confirmDeactivate() {
            await DeleteDeactiveScenario({
                url: this.$store.state.url,
                nWorflowId: this.$store.state.WorkflowId,
                tab: this.$store.state.Tab
            },
                (bActivated, bShowAlert, error) => {

                    this.$store.commit("setActivateScenario", bActivated);
                    this.$store.commit("setShowAlertScenario", bShowAlert);
                    if (bActivated && bShowAlert && error) {
                        this.$store.commit("setShowAlert", true);
                        this.$store.commit("setAlertMessage", error);    
                    }
                    else {
                        this.$store.commit("setShowDialogAlert", false);
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
    },
    computed: {
        getRes() {
            return function (resid) { return this.$store.getters.getRes(resid) }
        },
        showAlert: {
            get() {
                return this.$store.state.ShowAlert
            },
            set(hideAlert) {
                this.$store.commit("setShowAlert", hideAlert);
            }
        },
        showDialogAlert: {
            get() {
                return this.$store.state.ShowDialogAlert
            },
            set(hideDialog) {
                this.$store.commit("setShowDialogAlert", hideDialog);
            }
        },
        showDialogText: {
            get() {
                return this.$store.state.ShowDialogText
            }
        },
        showDialog: {
            get() {
                return this.$store.state.ShowDialog
            },
            set(hideDialog) {
                this.$store.commit("setShowDialog", hideDialog);
            }
        },
        showLoader: {
            get() {
                return this.$store.state.ShowLoading;
            },
            set(hideLoader) {
                this.$store.commit("setLoading", hideLoader);
            }
        },
        alertType: {
            get() {
                return this.$store.state.AlertType;
            }
        },
        getAlertMessage: {
            get() {
                return this.$store.state.AlertMessage;
            }
        },
        showAlertScenario: {
            get() {
                return this.$store.state.ShowAlertScenario;
            },
            set(hideAlertScenario) {
                this.$store.commit("setShowAlertScenario", hideAlertScenario);
            }
        },
        showAlertTop: {
            get() {
                return this.$store.state.AlertTopScenario;
            }
        }
    },
    template: `

    <v-app ref="wrapper" class="wrapper workflowWrapper">

            <v-dialog v-model='showDialog' persistent max-width='500' >
                <v-card>
                    <v-card-title class='warningOrange white--text'> {{ getRes(8830) }} </v-card-title>
                    <v-card-text class='paddingModal'> {{ getRes(8824) }} </v-card-text>
                    <v-divider></v-divider>
                    <v-card-actions>
                        <v-spacer></v-spacer>
                        <v-btn class='warningOrange white--text' text  @click='closeWithoutSaving()'> {{ getRes(8831) }} </v-btn> 
                        <v-btn color='warningOrange' text  @click='close()'> {{getRes(29)}} </v-btn>
                    </v-card-actions>
                </v-card>
            </v-dialog>

            <v-dialog v-model='showDialogAlert' persistent max-width='500' >
                <v-card>
                    <v-card-title class='warningOrange white--text'> {{getRes(8947)}}</v-card-title>
                    <v-card-text v-html="showDialogText" class='paddingModal'></v-card-text>
                    <v-divider></v-divider>
                    <v-card-actions>
                        <v-spacer></v-spacer>
                        <v-btn class='warningOrange white--text' text @click='confirmDeactivate()'> {{getRes(8948)}} </v-btn> 
                        <v-btn color='warningOrange' text @click='closeDialog()'> {{getRes(29)}} </v-btn>
                    </v-card-actions>
                </v-card>
            </v-dialog>

        <template>
            <v-alert outlined type="warning" prominent border="left" id="scenarioAlert" v-model="showAlertScenario"> {{getRes(8872)}} </v-alert>
        </template>

        <template v-if="alertType === 'error'">
            <v-snackbar :class="showAlertTop" color="#bb1515" style='z-index:999999999;margin-top:50px' v-model="showAlert" top :timeout="timeout"><v-icon>mdi-alert</v-icon>
                <template v-slot:action="{ attrs }">                 
                    <v-btn icon style='float:right' @click="showAlert = false"><v-icon>mdi-close</v-icon></v-btn>
                </template>
                <div><span v-for="(text, index) in getAlertMessage.split('##')" v-bind:class="(index > 0 )?'alertDetail':''" :key="index" >{{ text }}</span></div>
            </v-snackbar>
        </template>
        <template v-else-if="alertType === 'warning'">
             <v-snackbar :class="showAlertTop" color="#ff8a00" style='z-index:999999999;margin-top:50px' v-model="showAlert" top :timeout="timeout"><v-icon class="icon-exclamation-circle"></v-icon>
                <template v-slot:action="{ attrs }">
                    <v-btn icon style='float:right' @click="showAlert = false"><v-icon>mdi-close</v-icon></v-btn>
                </template>
                <span>{{getAlertMessage}}</span>
             </v-snackbar>
        </template>
        <template v-else-if="alertType === 'info'">
            <v-snackbar color="#3c8dbc" style='z-index:999999999;margin-top:50px' v-model="showAlert" top :timeout="timeout"><v-icon>mdi-information</v-icon>
                 <template v-slot:action="{ attrs }">
                    <v-btn icon style='float:right' @click="showAlert = false"><v-icon>mdi-close</v-icon></v-btn>
                 </template>
                 <span>{{getAlertMessage}}</span>
            </v-snackbar>
        </template>
        <template v-else-if="alertType === 'success'">
            <v-snackbar :class="showAlertTop" color="#58a04a" style='z-index:999999999;margin-top:50px' v-model="showAlert" top :timeout="timeout"><v-icon>mdi-check-circle</v-icon>
                <template v-slot:action="{ attrs }">
                    <v-btn icon style='float:right' @click="showAlert = false"><v-icon>mdi-close</v-icon></v-btn>
                </template>
                <span>{{getAlertMessage}}</span>
            </v-snackbar>
        </template>

        <div class="content-wrapper">   
            <section class="content">
                <v-dialog v-model="showLoader" persistent fullscreen content-class="loading-dialog">
                    <v-container fill-height>
                        <v-layout justify-center align-center >
                            <v-progress-circular indeterminate :size="70" :width="7" ></v-progress-circular>
                        </v-layout>
                    </v-container>
                </v-dialog>
                <div class="row">
                    <headNav></headNav>
                </div>
                <div class="row">                    
                    <contentTab></contentTab>
                </div>
            </section>
        </div>
    </v-app>
`,
};