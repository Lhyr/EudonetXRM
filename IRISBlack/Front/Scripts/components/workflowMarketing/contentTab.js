const blocksPanelComponent = () => import("./flowyComponent/blocksPanel.js");
const configurationPanelComponent = () => import("./flowyComponent/configurationPanel.js");

export default {
    name: "contentTab",
    data() {
        return {
        };
    },
    components: {
        blocksPanelComponent,
        configurationPanelComponent
    },
    computed: {
        showConfigurationPanel: {
            get() {
                return this.$store.state.ShowConfigurationPanel
            },
            set(hideConfigurationPanel) {
                this.$store.commit("setConfigurationPanel", hideConfigurationPanel);
            }
        },
        getConfigurationPanel: {
            get() {
                return this.$store.state.ShowConfigurationPanel
            }
        }
    },
    mounted() {
        var that = this;
        var tempblock2;
        flowy(document.getElementById("canvas"), drag, release, snapping, null, null, null, that.$store);

        function addEventListenerMulti(type, listener, capture, selector) {
            var nodes = document.querySelectorAll(selector);
            for (var i = 0; i < nodes.length; i++) {
                nodes[i].addEventListener(type, listener, capture);
            }
        }

        function snapping(drag, first) {
            var grab = drag.querySelector(".grabme");
            if (grab && grab.parentNode)
                grab.parentNode.removeChild(grab);
            
            var blockin = drag.querySelector(".blockin");

            if (!blockin || !blockin.parentNode)
                return true;
            blockin.parentNode.removeChild(blockin);

            //trigger when a recipient is added
            if (drag.querySelector(".blockelemtype").value == "1") {
                drag.innerHTML += "<div class='block_type trigger'>" + top._res_8799 + "</div> <div class='blockyleft trigger'><i class='fas fa-user-plus'></i><p class='blockyname'>" + top._res_8793 + "</p></div><div class='blockydiv'></div><div class='blockyinfo'>" + top._res_8798 + "</div>";
            }
            //trigger repetitive action
            else if (drag.querySelector(".blockelemtype").value == "3") {
                drag.innerHTML += "<div class='block_type trigger'>" + top._res_8799 + "</div> <div class='blockyleft trigger'><i class='fas fa-calendar'></i><p class='blockyname'>" + top._res_8899 + "</p></div><div class='blockydiv'></div><div class='blockyinfo'>" + top._res_8901 + "</div>";
            }
            //action send an email
            else if (drag.querySelector(".blockelemtype").value == "2") {
                drag.innerHTML += "<div class='block_type action'>" + top._res_8803 + "</div> <div class='blockyleft action'><i class='far fa-envelope'></i><p class='blockyname'>" + top._res_8807 + "</p></div><div class='blockydiv'></div><div class='blockyinfo actioninfo'>" + top._res_8808 + "</div>";
            }
            //action Add a delay
            else if (drag.querySelector(".blockelemtype").value == "4") {
                drag.innerHTML += "<div class='block_type action'>" + top._res_8803 + "</div> <div class='blockyleft action'><i class='fas fa-hourglass-half'></i><p class='blockyname'>" + top._res_8938 + "</p></div><div class='blockydiv'></div><div class='blockyinfo actioninfo'>" + top._res_8939 + "</div>";
            }

            let divCanvas = document.querySelector("#canvas");
            divCanvas.setAttribute('data-value', "");
            var styleElem = document.head.appendChild(document.createElement("style"));
            styleElem.innerHTML = "#canvas:before {content: '';}";
            return true;
        }

        function drag(block) {
            block.classList.add("blockdisabled");
            tempblock2 = block;
        }
        function release() {
            if (tempblock2) {
                tempblock2.classList.remove("blockdisabled");
            }
        }

        var noinfo = false;
        var beginTouch = function (event) {
            noinfo = false;
            if (event.target.closest(".create-flowy")) {
                noinfo = true;
            }
        }
       

        var tempblock;

        var doneTouch = function (event) {
            if (event.type === "mouseup" && !noinfo) {
                if (event.target.closest(".block") && !event.target.closest(".block").classList.contains("dragging")) {
                    if (flowy.removeSelectionFromAllBlocs)
                        flowy.removeSelectionFromAllBlocs();
                    tempblock = event.target.closest(".block");
                    if (document.getElementById("properties"))
                        document.getElementById("properties").classList.add("expanded");
                    tempblock.classList.add("selectedblock");
                    tempblock.classList.remove("blockActionError");
                    var blockIdElem = tempblock.querySelector(".blockid").value;
                    if (blockIdElem)
                        that.$store.commit("setCurrentBlockId", blockIdElem);
                    var blockElemtype = tempblock.querySelector(".blockelemtype").value;
                    if (blockElemtype)
                        that.$store.commit("setCurrentElementType", blockElemtype);
                    that.$store.commit("setConfigurationPanel", true);

                    flowy.switchRightPanel(blockElemtype, blockIdElem);

                    if (that.$store.state.ShowAlertScenario) {
                        if (tempblock.classList.contains("selectedblock"))
                            tempblock.classList.remove("blockActionError");
                    }
                }
                else if (event.target.id == "canvas") {
                    that.$store.commit("setConfigurationPanel", false);
                    flowy.removeSelectionFromAllBlocs();
                }
            }

            let elements = document.getElementsByClassName("block");            
            for (var i = 0; i < elements.length; i++) {
                elements[i].onclick = function () {
                    // remove class from sibling
                    var el = elements[0];
                    while (el) {
                        if (el.tagName === "DIV") {                            
                            //remove class
                            el.classList.remove("selectedblock");
                           
                        }                       
                       if (that.$store.state.ShowAlertScenario) {
                            var blockError = el.getElementsByClassName("blockyinfo actioninfo")[0];
                            if (blockError) {
                                if (blockError.childElementCount == 0)
                                    el.classList.add("blockActionError");
                                var blockErrorWarning = blockError.getElementsByTagName("DIV")[1];
                                if (blockErrorWarning && blockErrorWarning.classList.contains('styleWarning'))
                                    el.classList.add("blockActionError");
                            }
                        }

                        // pass to the new sibling
                        el = el.nextSibling;
                    }
                    this.classList.add("selectedblock");

                    var mContent = document.querySelector(".v-menu__content")
                    var vSelect = document.querySelector(".v-select")
                    if (mContent && mContent.classList.contains('menuable__content__active')) {
                        mContent.classList.remove("menuable__content__active");
                        mContent.style.display = 'none';
                        vSelect.classList.add("removeSelect");
                        vSelect.classList.remove("v-select--is-menu-active");
                        document.querySelector('div.removeSelect div[role="button"]').setAttribute('aria-expanded', 'false');
                    }

                    if (that.$store.state.ShowAlertScenario) {
                        let actionInfo = this.getElementsByClassName("blockyinfo actioninfo")[0];
                        if (actionInfo) {
                            if (actionInfo.childElementCount == 0)
                                this.classList.remove("blockActionError");
                            var blockErrorWarning = actionInfo.getElementsByTagName("DIV")[1];
                            if (blockErrorWarning && blockErrorWarning.classList.contains('styleWarning'))
                                this.classList.remove("blockActionError");
                        }
                    }
                };
            }

            if ((event.type === "mouseup" || event.type === "click") && (event.target.closest(".close") || event.target.id == "canvas")) {
                document.getElementById("properties").classList.remove("expanded");
                if (tempblock) {
                    tempblock.classList.remove("selectedblock");
                    if (that.$store.state.ShowAlertScenario) {
                        let blockyInfo = tempblock.getElementsByClassName("blockyinfo actioninfo")[0];
                        if (blockyInfo) {
                            if (tempblock.getElementsByClassName("blockyinfo actioninfo")[0].childElementCount == 0)
                                tempblock.classList.add("blockActionError");
                            var blockErrorWarning = tempblock.getElementsByClassName("blockyinfo actioninfo")[0].getElementsByTagName("DIV")[1];
                            if (blockErrorWarning && blockErrorWarning.classList.contains('styleWarning'))
                                tempblock.classList.add("blockActionError");
                        }
                    }
                }
                that.$store.commit("setConfigurationPanel", false);
            }
        }

        var hidePanel = function (event) {
            var blocks = document.getElementsByClassName("blockelem noselect block selectedblock");
            if (event.type === "click" && event.target.closest(".block")) {
                document.getElementById("properties").classList.add("expanded");
            }
            if (event.type === "click" && event.target.id == "canvas") {
                document.getElementById("properties").classList.remove("expanded");
                while (blocks.length > 0) {
                    blocks[0].classList.remove("selectedblock");
                }
            }

        }

        addEventListener("click", hidePanel);
        addEventListener("click", doneTouch);
        addEventListener("mousedown", beginTouch, false);
        addEventListener("mouseup", doneTouch, false);
        addEventListenerMulti("touchstart", beginTouch, false, ".block");

        if (this.$store.state.Datas != null && this.$store.state.Datas.html != '') {
            setTimeout(function () {
                flowy.import(that.$store.state.Datas);
                flowy.removeSelectionFromAllBlocs();
                let divCanvas = document.querySelector("#canvas");
                divCanvas.setAttribute('data-value', '');
                var styleElem = document.head.appendChild(document.createElement("style"));
                styleElem.innerHTML = "#canvas:before {content: '';}";

                if (that.$store.state.ActivateScenario) {
                    that.$store.commit("setShowAlertScenario", true);
                }

                //chargemenet des res
                if (that.$store.state.TriggerType == 3) {

                    let triggerTag = document.querySelectorAll('.block_type.trigger');
                    if (triggerTag.length > 0) {
                        triggerTag = triggerTag[0];
                        triggerTag.innerHTML = that.$store.getters.getRes(8799, '');
                    }

                    let triggerTitle = document.querySelectorAll('.blockyleft.trigger');
                    if (triggerTitle.length > 0) {
                        triggerTitle = triggerTitle[0].querySelector("p");
                        triggerTitle.innerHTML = that.$store.getters.getRes(8899, '');
                    }

                    let blocInfo = document.querySelector('.trigger.blockelem.block');
                    var blockDesctiprion = blocInfo.querySelector(".blockyinfo");
                    if (!that.$store.state.TriggerFilterLabel || that.$store.state.TriggerFilterLabel == '') {
                        if (that.$store.state.TriggerScheduleInfo && that.$store.state.TriggerScheduleInfo != '')
                            blockDesctiprion.innerHTML = "<div class='triggerPlanificationLabel'>" + that.$store.state.TriggerScheduleInfo + "</div >" + " " + that.$store.getters.getRes(8913, '');
                        else
                            blockDesctiprion.innerHTML = that.$store.getters.getRes(8901, '');
                    }
                    else if (that.$store.state.TriggerFilterLabel && that.$store.state.TriggerFilterLabel != '') {
                        if (that.$store.state.TriggerScheduleInfo && that.$store.state.TriggerScheduleInfo != '')
                            blockDesctiprion.innerHTML = "<div class='triggerPlanificationLabel'>" + that.$store.state.TriggerScheduleInfo + "</div >" + " " + that.$store.getters.getRes(8915, '') + " " + "<div class='triggerFilterLabel'>" + that.$store.state.TriggerFilterLabel + "</div >";
                        else
                            blockDesctiprion.innerHTML = that.$store.getters.getRes(8901, '');
                    }
                }
                else {
                    let triggerTag = document.querySelectorAll('.block_type.trigger');
                    if (triggerTag.length > 0) {
                        triggerTag = triggerTag[0];
                        triggerTag.innerHTML = that.$store.getters.getRes(8799, '');
                    }

                    let triggerTitle = document.querySelectorAll('.blockyleft.trigger');
                    if (triggerTitle.length > 0) {
                        triggerTitle = triggerTitle[0].querySelector("p");
                        triggerTitle.innerHTML = that.$store.getters.getRes(8793, ''); 
                    }

                    let blocInfo = document.querySelectorAll('.trigger.block');
                    if (blocInfo.length > 0) {
                        let triggerFilterLabel = blocInfo[0].querySelector(".triggerFilterLabel");
                        blocInfo = blocInfo[0].querySelector(".blockyinfo");
                        blocInfo.innerHTML = top._res_8798;
                        if (triggerFilterLabel && triggerFilterLabel.innerHTML.length > 0)
                            blocInfo.innerHTML += " " + that.$store.getters.getRes(8805, '') + " " + triggerFilterLabel.outerHTML;
                    }
                }
                
                //modify res for actions
                var actions = document.querySelectorAll('.action.block');
                actions = Array.from(actions).filter(a => a.children[1].tagName && a.children[1].tagName.toLowerCase() == "input");
                for (var i = 0; i < actions.length; i++) {
                    //Tag
                    let actionTag = actions[i].querySelectorAll('.block_type');
                    if (actionTag.length > 0) {
                        actionTag = actionTag[0];
                        actionTag.innerHTML = that.$store.getters.getRes(8803, '');
                    }

                    let blockid, actionInfo;
                    blockid = actions[i].querySelector(".blockid");
                    if (blockid)
                        blockid = blockid.value;
                    if (actionInfos && actionInfos[blockid]) {
                        actionInfo = actionInfos[blockid];
                    }

                    //Header
                    let actionTitle = actions[i].querySelectorAll('.blockyname');
                    if (actionTitle.length > 0) {
                        actionTitle = actionTitle[0];
                        if (actionInfo && actionInfo.actionType == 1)
                            actionTitle.innerHTML = that.$store.getters.getRes(8807, '');
                        else if (actionInfo && actionInfo.actionType == 2)
                                actionTitle.innerHTML = that.$store.getters.getRes(8938, '');
                    }

                    //description
                    var blockDescription = actions[i].querySelector(".blockyinfo");
                    if (actionInfo && actionInfo.actionType == 1) {//Send Mail Action
                        if (actionInfo.campaignDescr && actionInfo.campaignDescr != '') {
                            if (actionInfo.campaignState == '9') {
                                blockDescription.innerHTML = that.$store.getters.getRes(8808, '') + " " + "<div class='actionFilterLabel'>" + actionInfo.campaignDescr + "</div><div onmouseover='mOver(event)' class='styleSuccess'><i class='fas fa-check iconI'></i></div>";
                            }
                            else { 
                                blockDescription.innerHTML = that.$store.getters.getRes(8808, '') + " " + "<div class='actionFilterLabel'>" + actionInfo.campaignDescr + "</div><div onmouseover='mOver(event)' class='styleWarning'><i class='fas fa-exclamation-triangle iconI'></i></div>";
                            }
                        }
                        else {
                            blockDescription.innerHTML = that.$store.getters.getRes(8808, '');
                        }
                    }
                    else if (actionInfo && actionInfo.actionType == 2) {//Delay Action
                        let lDelayType = that.$store.state.DelayItems.find(x => x.id == actionInfo.delaySelect);
                        if (lDelayType)
                            lDelayType = lDelayType.label;
                        else
                            lDelayType = '';
                        if (actionInfo.delayInput && actionInfo.delayInput != "" && (!lDelayType || lDelayType == ""))
                            blockDescription.innerHTML = that.$store.getters.getRes(8942, '') + " " + "<div class='actionDelayLabel'>" + actionInfo.delayInput + "</div >" + " " + that.$store.getters.getRes(8943, '');
                        else if ((!actionInfo.delayInput || actionInfo.delayInput == "") && lDelayType != "")
                            blockDescription.innerHTML = that.$store.getters.getRes(8942, '') + " " + "<div class='actionDelayLabel'>" + lDelayType + "</div >" + " " + that.$store.getters.getRes(8943, '');
                        else if (actionInfo.delayInput != "" && lDelayType)
                            blockDescription.innerHTML = that.$store.getters.getRes(8942, '') + " " + "<div class='actionDelayLabel'>" + actionInfo.delayInput + " " + lDelayType + "</div >" + " " + that.$store.getters.getRes(8943, '');
                        else
                            blockDescription.innerHTML = mainJS.store.getters.getRes(8939, '');
                    }

                    

                    if (!that.$store.state.ShowAlertScenario)
                        actions[i].classList.remove("blockActionError");
                }

                that.$store.commit("setWorkflowDatas", flowy.output());

            }, 100);
        }
        else {
            flowy.deleteBlocks();
            let divCanvas = document.querySelector("#canvas");
            divCanvas.innerHTML = "<div class='indicator invisible'></div>"
            divCanvas.setAttribute('data-value', that.$store.getters.getRes(8837, ''));
            var styleElem = document.head.appendChild(document.createElement("style"));
            styleElem.innerHTML = "#canvas:before {content: url(./IRISBlack/Front/Assets/CSS/flowy/assets/arrow.png);}";
        }

    },
    methods: {
    },
    template: `
    <div class="contentElem" ref="contentTabs">
     
        <blocksPanelComponent></blocksPanelComponent>
        <div id="canvas">
            
        </div>
            <v-navigation-drawer  style='top:50px' right v-model="showConfigurationPanel" class="flowyRightPanel" absolute>
               <configurationPanelComponent></configurationPanelComponent>
            </v-navigation-drawer>
    </div>
`,

};