const configComponent = () => import("./configComponent.js");
import { eWorkflowMixin } from '../mixin/eWorkflowMixin.js?ver=803000';

export default {
    name: "configurationPanel",
    data: function () {
        return {
            blockId: null,
            items: this.$store.state.DelayItems
        }
    },
    mixins: [eWorkflowMixin],
    components: {
        configComponent,
        getTab: {
            get() {
                return this.$store.state.Tab
            },
        }
    },
    computed: {
        elementType: {
            get() {
                return this.$store.state.CurrentElementType;
            }
        },
        campaignInfo: {
            get() {
                return this.$store.state.ActionSendEmailDescr;
            }
        },    
        displayTriggerConfig: {
            get() {
                if (this.$store.state.CurrentElementType == '1')
                    return true;
                else return false;
            }
        },
        displaySendEmailConfig: {
            get() {
                if (this.$store.state.CurrentElementType == '2') {
                    return true;
                }    
                else return false;
            }
        },
        displayRepetitiveAction: {
            get() {
                if (this.$store.state.CurrentElementType == '3')
                    return true;
                else return false;
            }
        },
        displayAddDelay: {
            get() {
                if (this.$store.state.CurrentElementType == '4')
                    return true;
                else
                    return false;
            }
        },
        displayCampaignInfo: {
            get() {
                if (!this.$store.state.ActionSendEmailDescr && this.$store.state.ActionSendEmailDescr == '')
                    return false;
                else
                    return true;
            }
        },
        campaignStatus: {
            get() {
                if (this.$store.state.ActionCampaignState == '9')
                    return true;
                else
                    return false;
            }
        },
        blocType: {
            get() {
                switch (this.$store.state.CurrentElementType) {
                    case '1':
                    case '3':
                        return this.$store.getters.getRes(8799, ''); // Trigger
                        break;
                    case '2':
                    case '4' :
                        return this.$store.getters.getRes(8803, ''); //Action
                        break;
                }
            }
        },
        title: {
            get() {
                switch (this.$store.state.CurrentElementType) {
                    case '1':
                        return this.$store.getters.getRes(8793, ''); 
                        break;
                    case '2': 
                        return this.$store.getters.getRes(8807, '');
                        break;
                    case '3':
                        return this.$store.getters.getRes(8899, '');
                        break
                    case '4':
                        return this.$store.getters.getRes(8938, '');
                        break;
                }
            }
        },
        description: {
            get() {
                switch (this.$store.state.CurrentElementType) {
                    case '1':
                        return this.$store.getters.getRes(8804, ''); 
                        break;
                    case '2':
                        return this.$store.getters.getRes(8810, '');
                        break;
                    case '3':
                        return this.$store.getters.getRes(8900, '');
                        break;
                    case '4':
                        return this.$store.getters.getRes(8939, '');
                        break;
                }
            }
        },
        triggerFilterLabel: {
            get() {
                if (this.$store.state.TriggerFilterLabel && this.$store.state.TriggerFilterLabel != '') 
                    return this.$store.state.TriggerFilterLabel;
                else 
                    return this.$store.getters.getRes(8918, '');
            }
        },
        actionCampaignDescription: {
            get() {
                switch (this.$store.state.ActionCampaignState) {
                    case '8':  //campaign saved
                    case '10': // test email
                        return this.$store.getters.getRes(8867, '');
                        break;
                    case '9': //campagne validée
                        return this.$store.getters.getRes(8866, '');
                        break;
                }
            }
        },
        actionMailingButton: {
            get() {
                if (this.$store.state.ActivateScenario)//Open the mail campaign
                    return this.$store.getters.getRes(8903, '');
                else if (!this.$store.state.ActionMailingButton)
                    return this.$store.getters.getRes(8811, ''); //design the email to be sent
                else
                    return this.$store.getters.getRes(8864, ''); //edit email to send
            }
        },
        triggerPlanificationDescr: {
            get() {
                if (this.$store.state.TriggerScheduleInfo && this.$store.state.TriggerScheduleInfo != '')
                    return this.$store.state.TriggerScheduleInfo;
                else
                    return this.$store.getters.getRes(8905, '');  
            }
        },
        triggerRepetitiveFilter: {
            get() {
                if (this.$store.state.TriggerFilterLabel && this.$store.state.TriggerFilterLabel != '')
                    return this.$store.state.TriggerFilterLabel;
                else
                    return this.$store.getters.getRes(8911, '');
            }
        },
        triggerPlanificationButton: {
            get() {
                if (this.$store.state.TriggerScheduleInfo && this.$store.state.TriggerScheduleInfo != '')
                    return this.$store.getters.getRes(8906, '');
                else
                    return this.$store.getters.getRes(8916, '');
            }
        },
        triggerFilterButton: {
            get() {
                if (this.$store.state.TriggerFilterLabel && this.$store.state.TriggerFilterLabel != '')
                    return this.$store.getters.getRes(8912, '');
                else
                    return this.$store.getters.getRes(8917, '');
            }
        },
        actionDelayInformation: {
            get() {
                var delayValue = 60;
                delayValue = this.$store.state.DelayFrequency;
                var delayInfos = this.$store.getters.getRes(8941, '');
                return delayInfos.replace('{execution_frequency_parameter}', delayValue);
            }
        },
        saveActionDelayInput: {
            get() {
                return this.$store.state.DelayInputValue;
            },
            set(newValue) {
                this.$store.commit("setDelayInputValue", newValue);
            }
        },
        saveActionDelaySelect: {
            get() {
                return this.$store.state.DelaySelectValue;
            },
            set(newValue) {
                this.$store.commit("setDelaySelectValue", newValue);
            }
        },
        getRes() {
            return function (resid) { return this.$store.getters.getRes(resid) }
        },
    },
    mounted() {  
    },
    methods: {
        addMailing() {
            if (actionInfos) {
                let cmapignId = null;
                let campaignInfos = actionInfos[this.$store.state.CurrentBlockId];
                if (campaignInfos) {
                    cmapignId = campaignInfos.campaignId;
                    if (this.$store.state.ActivateScenario)
                        this.closeModal(cmapignId);
                    else
                        AddMailing(this.$store.state.Tab, 5, cmapignId);
                }
                else
                    AddMailing(this.$store.state.Tab, 5);
            }
            else
                AddMailing(this.$store.state.Tab, 5);
        },
        openRecipientsFilterModal() {
            var that = this;
            var applyRecipientsFilterModal = function (modal) {
                var currentFilterId = "0";
                var currentFilterLib = "";
                var currentFilter = modal.getIframe()._eCurentSelectedFilter;
                if (currentFilter) {
                    // Recup de l'id du filtre
                    var oId = currentFilter.getAttribute("eid").split('_');
                    currentFilterId = oId[oId.length - 1];

                    // Recupe du libelle du filtre
                    var tabLibFilter = currentFilter.querySelectorAll("div[ename='COL_104000_104001']");
                    if (tabLibFilter.length > 0) {
                        currentFilterLib = tabLibFilter[0].innerHTML.trim();
                    }
                }

                //on met � jour le store
                that.$store.commit("setTriggerFilterId", currentFilterId);
                that.$store.commit("setTriggerFilterLabel", currentFilterLib);

                //to update the description which depending if exist or not a selected condition 
                var blockDesctiprion = document.querySelector(".blockyinfo");

                //trigger block "When a recipient is added"
                if (that.$store.state.CurrentElementType == '1') { 
                    if (that.$store.state.TriggerFilterLabel != '')
                        blockDesctiprion.innerHTML = that.$store.getters.getRes(8798, '') + " " + that.$store.getters.getRes(8805, '') + " " + "<div class='triggerFilterLabel'>" + currentFilterLib + "</div >";
                     else
                        blockDesctiprion.innerHTML = that.$store.getters.getRes(8798, '');
                }

                //trigger block "Repetitive action"
                if (that.$store.state.CurrentElementType == '3') {
                    if (that.$store.state.TriggerScheduleInfo == '' || that.$store.state.TriggerScheduleId == 0) {
                        if (that.$store.state.TriggerFilterLabel != '')
                            blockDesctiprion.innerHTML = that.$store.getters.getRes(8914, '') + " " + "<div class='triggerFilterLabel'>" + currentFilterLib + "</div >";
                        else
                            blockDesctiprion.innerHTML = that.$store.getters.getRes(8901, '');
                    }
                    else if (that.$store.state.TriggerScheduleInfo != '') {
                        if (that.$store.state.TriggerFilterLabel != '')
                            blockDesctiprion.innerHTML = "<div class='triggerPlanificationLabel'>" + that.$store.state.TriggerScheduleInfo + "</div >" + " " + that.$store.getters.getRes(8915, '') + " " + "<div class='triggerFilterLabel'>" + currentFilterLib + "</div >";
                        else
                            blockDesctiprion.innerHTML = "<div class='triggerPlanificationLabel'>" + that.$store.state.TriggerScheduleInfo + "</div >" + " " + that.$store.getters.getRes(8913, '');
                    }

                }
               
                modal.hide();

            }
            var options = {
                tab: this.$store.state.Tab,
                onApply: function (modal) {
                    applyRecipientsFilterModal(modal);
                },
                value: this.$store.state.TriggerFilterId,
                deselectAllowed: true,
                selectFilterMode: true,
            }
            top.filterListObjet(0, options);
            
        },
        openScheduleParameterModal() {
            var nNew = 0;
            var that = this;
            // On choisi le prochain creaneau d'heure
            var d = new Date();
            while (d.getMinutes() % 30 != 0) {
                d.setMinutes(d.getMinutes() + 1);
            }
            var hours = ("0" + d.getHours()).slice(-2);
            var minutes = ("0" + d.getMinutes()).slice(-2);

            that._modalSchedule = new eModalDialog(top._res_1049, 0, "eSchedule.aspx", 450, 500);

            that._modalSchedule.addParam("scheduletype", 5, "post");
            that._modalSchedule.addParam("New", nNew, "post");
            that._modalSchedule.addParam("iframeScrolling", "yes", "post");
            that._modalSchedule.addParam("EndDate", 0, "post");
            that._modalSchedule.addParam("BeginDate", 0, "post");
            that._modalSchedule.addParam("ScheduleId", that.$store.state.TriggerScheduleId, "post");
            that._modalSchedule.addParam("Tab", 0, "post");
            that._modalSchedule.addParam("Workingday", "TODO", "post");
            that._modalSchedule.addParam("calleriframeid", 0, "post");
            that._modalSchedule.addParam("hour", hours + ":" + minutes, "post");
            that._modalSchedule.addParam("AppType", 0, "post");
            that._modalSchedule.addParam("FileId", 0, "post");

            that._modalSchedule.ErrorCallBack = that._modalSchedule.hide();
            
            that._modalSchedule.show();
            that._modalSchedule.addButtonFct(top._res_29, function () { that._modalSchedule.hide(); }, "button-gray", 'cancel');
            that._modalSchedule.addButtonFct(top._res_28, function () { openScheduleParameterValidReturn(that._modalSchedule); }, "button-green");

        },
        showDelay() {
            var blocid = this.$store.state.CurrentBlockId;
            var delaySelect = this.$store.state.DelaySelectValue;
            var delayInput = this.$store.state.DelayInputValue;
            //store in a table for every action block the delay configuration
            actionInfos[blocid] = { 'delayInput': delayInput, 'delaySelect': delaySelect };

            var elem = document.querySelector('input[value="' + blocid + '"][name="blockid"]');

            if (elem) {
                var blockDescription = elem.parentElement.querySelector(".blockyinfo");
                if (blockDescription != '') {
                    let lDelayType = this.items.find(x => x.id === this.$store.state.DelaySelectValue);
                    if (lDelayType)
                        lDelayType = lDelayType.label;
                    if (this.$store.state.DelayInputValue != "" && this.$store.state.DelaySelectValue == "")
                        blockDescription.innerHTML = this.$store.getters.getRes(8942, '') + " " + "<div class='actionDelayLabel'>" + this.$store.state.DelayInputValue + "</div >" + " " + this.$store.getters.getRes(8943, '');
                    else if (this.$store.state.DelayInputValue == "" && this.$store.state.DelaySelectValue != "")
                        blockDescription.innerHTML = this.$store.getters.getRes(8942, '') + " " + "<div class='actionDelayLabel'>" + lDelayType + "</div >" + " " + this.$store.getters.getRes(8943, '');
                    else if (this.$store.state.DelayInputValue != "" && this.$store.state.DelaySelectValue != "")
                        blockDescription.innerHTML = this.$store.getters.getRes(8942, '') + " " + "<div class='actionDelayLabel'>" + this.$store.state.DelayInputValue + " " + lDelayType + "</div >" + " " + this.$store.getters.getRes(8943, '');
                    else
                        blockDescription.innerHTML = mainJS.store.getters.getRes(8939, '');
                }

                flowy.rearrangeMe();
                elem.parentElement.setAttribute('ndly', this.$store.state.DelayInputValue);
                elem.parentElement.setAttribute('tdly', this.$store.state.DelaySelectValue);
            }
        },
        deleteBloc() {
            if (this.$store.state.CurrentElementType == '1' || this.$store.state.CurrentElementType == '3') { //trigger block 
                this.$store.commit("setTriggerScheduleId", 0);
                this.$store.commit("setTriggerScheduleInfo", "");
                this.$store.commit("setTriggerScheduleUpdated", "");
                this.$store.commit("setCurrentWorkflowTriggerId", 0);
                this.$store.commit("setTriggerFilterLabel", '');
                this.$store.commit("setTriggerRepetitiveFilter", "");
                this.$store.commit("setTriggerBlockId", -1);
                this.$store.commit("setTriggerType", -1);
                this.$store.commit("setSchedulerJobId", -1);

                //action Send an email
                this.$store.commit("setActionSendEmailDescr", "");
                this.$store.commit("setActionMailingButton", false);
                this.$store.commit("setActionCampaignState", "-1");
                
                //action Add a Delay
                this.$store.commit("setDelayInputValue", "");
                this.$store.commit("setDelaySelectValue", "");
                actionInfos = [];
            }
            
            if (this.$store.state.CurrentElementType == '2') { //action block "Send an email"
                this.$store.commit("setActionSendEmailDescr", "");
                this.$store.commit("setActionMailingButton", false);
                this.$store.commit("setActionCampaignState", "-1");
                delete actionInfos[this.$store.state.CurrentBlockId];
            }

            if (this.$store.state.CurrentElementType == '4') { //action block "Add a delay"
                this.$store.commit("setDelayInputValue", "");
                this.$store.commit("setDelaySelectValue", "");
                delete actionInfos[this.$store.state.CurrentBlockId];
            }
            
            document.getElementById("properties").classList.remove("expanded");
            this.$store.commit("setConfigurationPanel", false);
            flowy.deleteBlock(this.$store.state.CurrentBlockId, false);

            if (!flowy.output() || (flowy.output() && flowy.output().blocks && flowy.output().blocks.length == 0)) {
                let divCanvas = document.querySelector("#canvas");
                divCanvas.setAttribute('data-value', this.$store.getters.getRes(8837, ''));
                var styleElem = document.head.appendChild(document.createElement("style"));
                styleElem.innerHTML = "#canvas:before {content: url(./IRISBlack/Front/Assets/CSS/flowy/assets/arrow.png);}";
            }
            
        }
    },
    template: `
        <div id="properties">
            <div><configComponent :typeBlock="blocType" :titleBlock="title" :descriptionBlock="description"/></div>
            <div class="propsContent">
                <div v-show="this.displayTriggerConfig">
                    <div class="titleProps"><span>{{ getRes(8809,'') }}</span></div>
                    <div class="planificationTitle"> {{ triggerFilterLabel }} </div> 
                    <v-btn outlined justify="center" color="primary" @click="openRecipientsFilterModal"> {{ triggerFilterButton }} </v-btn>
                </div>
                <div v-show="this.displayRepetitiveAction">  
                    <div class="titleProps"><span>{{ getRes(8904,'') }}</span></div>
                    <div class="planificationTitle"> {{ triggerPlanificationDescr }} </div>                   
                    <v-btn outlined justify="center" color="primary" @click="openScheduleParameterModal"> {{ triggerPlanificationButton }} </v-btn>
                    <div class="titleProps addPadding"><span>{{ getRes(8910,'') }}</span></div>
                    <div class="planificationTitle"> {{ triggerRepetitiveFilter }} </div>                   
                    <v-btn outlined justify="center" color="primary"  @click="openRecipientsFilterModal"> {{ triggerFilterButton }} </v-btn>
                </div>             
                <div v-show="this.displaySendEmailConfig">
                    <div class="actionProps" v-show="this.displayCampaignInfo">
                        <div v-bind:class="[ !campaignStatus ? 'warningStatus' : 'successStatus']"><i v-bind:class="[ !campaignStatus ? 'fas fa-exclamation-triangle' : 'fas fa-check']"></i></div>
                        <div class="campaignDescription"><span> {{ getRes(8865, '') }} <span style="font-weight: 700;"> {{ campaignInfo }} </span> {{ actionCampaignDescription }} </span></div>
                    </div>
                    <v-btn outlined justify="center" color="primary" @click="addMailing"> {{ actionMailingButton }} </v-btn>
                </div>
                <div v-show="this.displayAddDelay">
                    <div class="actionProps">
                        <v-row>
                            <v-col cols="3" class="delayCol"><span class="delayPadding">{{ getRes(8940,'') }}</span></v-col>
                            <v-col cols="3" class="delayCol"><v-text-field outlined class="delayInput" v-model="saveActionDelayInput" type="number" onfocus="this.previousValue = this.value" onkeydown="this.previousValue = this.value" oninput="validity.valid || (value = this.previousValue)" @change="showDelay"></v-text-field></v-col>
                            <v-col cols="6" class="delayCol">
                                <v-select :items="items" item-text="label" item-value="id" outlined dense v-model="saveActionDelaySelect" @change="showDelay"></v-select>
                            </v-col>
                        </v-row>
                    </div>
                    <div class="titleProps">
                        <span>{{ actionDelayInformation }}</span>
                    </div>
                </div>
            </div>
            <div id="removeblock"><v-btn block large color="primary" dark @click="deleteBloc"> {{getRes(8806,'Remove the block')}} </v-btn></div>
        </div>
`,

};