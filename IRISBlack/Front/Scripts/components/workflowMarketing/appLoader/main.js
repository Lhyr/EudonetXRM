
var getters = {
    nFileId: state => state.nFileId,
    getRes: function (state) {
        return function (id) {

            if (typeof id === "number") {

                if (top && top.hasOwnProperty("_res_" + id))
                    return top["_res_" + id];
                else
                    return "##[INVALID_RES_" + id + "]##";
            }
            else if (typeof id === "string") {
                return "(TODO RES) " + id
            }

            return "";
        }
    },
    getUrl: function () {
        let domaine = window.location.href.split("/");
        domaine.pop();
        return domaine.join("/");
    },
    /** Récupère l'id de la langue de l'utilisateur. */
    getUserLangID: function () {
        return (top && top.hasOwnProperty("_userLangId")) ? top._userLangId : 0
    },
    
   

}

var domaine = window.location.href.split("/");
domaine.pop();
var urlStruc = domaine.join("/");

Vue.use(Vuex);

const getDefaultState = () => {
    return {
        nFileId: null,
        url: urlStruc,
        Tab: 0,
        Label:"",
        wizardActivTab: "Design",
        LangId: 0,
        AllAvailableLng: [
            { "0": "Français" },
        ],
        UserInfos: {
            LangId: 0,
            Login: "",

        },
        ParentFileId : 0,
        AllAvailableLevels: {},
        ShowAlert: false,
        AlertMessage: "",
        ShowConfigurationPanel: false,
        TriggerBlockId: -1,
        TriggerFilterId: 0,
        TriggerFilterLabel: "",
        WorkflowJSBlocks: null, 
        CurrentBlockId: null,
        CurrentElementType: null,
        ShowLoading: true,
        AlertType: "",
        ShowDialog: false,
        ShowDialogAlert: false,
        ShowDialogText : "",
        ActionMailingButton: false,
        ActionSendEmailDescr: "",
        ActionCampaignState: "",
        ShowAlertScenario: false,
        AlertTopScenario: "",
        ActivateScenario: false,
        TriggerScheduleInfo: "",
        TriggerScheduleId: 0,
        TriggerScheduleUpdated: "",
        TriggerRepetitiveFilter: "",
        TriggerType: 0,
        TriggerSchedulerJobId: 0,
        DelayInputValue: "",
        DelaySelectValue: "",
        DelayItems: [
            {
                id: 1, label: eTools.getRes(851)
            }, //hour(s)
            {
                id: 2, label: eTools.getRes(853)
            }, //day(s)
            {
                id: 3, label: eTools.getRes(852)
            }, //week(s)
            {
                id: 4, label: eTools.getRes(854)
            } //month(s)
            , //week(s)
            {
                id: 5, label: eTools.getRes(850)
            } //
        ],
        DelayFrequency:10//fréquence du delai dans eudoprocess
    }
}

const defaultState = getDefaultState();

export const store = new Vuex.Store({
    state: defaultState,

    getters: getters,

    mutations: {
        
        setWizardActivTab(state, wizTab) {
            state.wizardActivTab = wizTab;
        },
        
        setLangdid(state, langid) {
            state.LangId = langid
        },

        setUserInfos(state, user) {
            state.UserInfos = { ...state.UserInfos, user }
        },

        resetState(state) {
            Object.assign(state, getDefaultState())
        },
        setParentFileId(state, evtFileId) {
            state.ParentFileId = evtFileId;
        },
        setTab(state, tab) {
            state.Tab = tab;
        },
        setParentTab(state, parentTab) {
            state.ParentTab = parentTab;
        },
        setWorkflowLabel(state, label) {
            state.Label = label;
        },
        setWorkflowTmpLabel(state, tmplabel) {
            state.TmpLabel = tmplabel;
        },
        setShowAlert(state, showAlert) {
            state.ShowAlert = showAlert;
        },
        setShowDialog(state, showDialog) {
            state.ShowDialog = showDialog;
        }, 
        setShowDialogAlert(state, showDialogAlert) {
            state.ShowDialogAlert = showDialogAlert;
        },
        setShowDialogText(state, showDialogText) {
            state.ShowDialogText = showDialogText;
        },
        setAlertMessage(state, message) {
            state.AlertMessage = message;
        },
        setConfigurationPanel(state, ConfigurationPanel) {
            state.ShowConfigurationPanel = ConfigurationPanel;
        },
        setTriggerBlockId(state, triggerBlockId) {
            state.TriggerBlockId = triggerBlockId;
        },
        setTriggerFilterId(state, triggerFilterId) {
            state.TriggerFilterId = triggerFilterId;
        },
        setTriggerFilterLabel(state, triggerFilterLabel) {
            state.TriggerFilterLabel = triggerFilterLabel;
        },
        setWorkflowBlocks(state, workflowJSBlocks) {
            state.WorkflowJSBlocks = workflowJSBlocks;
        },
        setCurrentBlockId(state, currentBlockId) {
            state.CurrentBlockId = currentBlockId;
        },
        setCurrentElementType(state, currentElementType) {
            state.CurrentElementType = currentElementType;
        },
        setCurrentWorkflowId(state, workflowId) {
            state.WorkflowId = workflowId;
        },
        setCurrentWorkflowTriggerId(state, workflowTriggerId) {
            state.WorkflowTriggerId = workflowTriggerId;
        },
        setCurrentWorkflowTriggerStepId(state, workflowTriggerStepId) {
            state.WorkflowTriggerStepId = workflowTriggerStepId;
        },
        setWorkflowDatas(state, datas) {
            state.Datas = datas;
        },
        setLoading(state, loading) {
            state.ShowLoading = loading;
        },
        setAlertType(state, atype) {
            state.AlertType = atype;
        },
        setActionMailingButton(state, actionMailingButton) {
            state.ActionMailingButton = actionMailingButton;
        },
        setActionSendEmailDescr(state, actionSendEmailDescr) {
            state.ActionSendEmailDescr = actionSendEmailDescr;
        },
        setActionCampaignState(state, actionCampaignState) {
            state.ActionCampaignState = actionCampaignState;
        },
        setShowAlertScenario(state, showAlertScenario) {
            state.ShowAlertScenario = showAlertScenario;
        }, 
        setAlertTopScenario(state, alertTopScenario) {
            state.AlertTopScenario = alertTopScenario;
        },
        setActivateScenario(state, activateScenario) {
            state.ActivateScenario = activateScenario;
        },
        setTriggerScheduleInfo(state, triggerScheduleInfo) {
            state.TriggerScheduleInfo = triggerScheduleInfo;
        },
        setTriggerScheduleId(state, triggerScheduleId) {
            state.TriggerScheduleId = triggerScheduleId;
        },
        setTriggerScheduleUpdated(state, triggerScheduleUpdated) {
            state.TriggerScheduleUpdated = triggerScheduleUpdated;
        },
        setTriggerRepetitiveFilter(state, triggerRepetitiveFilter) {
            state.TriggerRepetitiveFilter = triggerRepetitiveFilter;
        },
        setTriggerType(state, triggerType) {
            state.TriggerType = triggerType;
        },
        setSchedulerJobId(state, schedulerJobId) {
            state.TriggerSchedulerJobId = schedulerJobId;
        },
        setDelayInputValue(state, delayInputValue) {
            state.DelayInputValue = delayInputValue;
        },
        setDelaySelectValue(state, delaySelectValue) {
            state.DelaySelectValue = delaySelectValue;
        },
        setDelayFrequency(state, delayFrequency) {
            state.DelayFrequency = delayFrequency;
        }
    }
});

export function initVue() {

    Vue.use(eudoFront.default);
    Vue.use(Vuetify);

    const opts = {
        icons: {
        },
        theme: {
        },
    };
    const vuetify = new Vuetify(opts);

    const App_assist_workflow = () => import("../workflowMainApp.js");
    return new Vue({
        render: h => h(App_assist_workflow),
        store,
        vuetify,
        destroyed() {
            //on reset le store
            store.commit('resetState')
        }
    }).$mount('#app_assist_workflow');
}