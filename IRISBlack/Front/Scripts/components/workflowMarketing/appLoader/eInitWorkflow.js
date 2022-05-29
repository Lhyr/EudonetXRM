//*****************************************************************************************************//
//*****************************************************************************************************//
//** Display the workflow for Marketing Automation in VueJS
//** mainAddWorkflowModal: The modal that must contain the VueJS workflow
//** mainJS: this page will initiate the Vue object
//*****************************************************************************************************//
//*****************************************************************************************************//

var mainJS = null;
var mainAddWorkflowModal = null;
var vueJSInstance = null;
var WorkflowEnum = null;
var actionInfos = [];
//this method creates the VueJS workflow by initializing it
function InitializeWorkflow(modalWizard, tab, parentTab, parentFileId, scenarioId) {

	mainAddWorkflowModal = modalWizard;

	(async () => {
		let { LoadJS, LoadCSS } = await import("./LoadIrisScripts.js");
		//Here is the loading of the 2 functions for the JS and CSS of the new sheet mode. 
		await Promise.all([LoadJS(), LoadCSS()]);
		//import the main.js file that initializes vuex and the components
		mainJS = await import("./main.js");
		WorkflowEnum = await import("./Enum.js");
		//position the modal on top to take 100% of the page
		var modalDiv = document.getElementById('app_assist_workflow').parentElement;
		modalDiv.className = "ModalOnTop";
		modalDiv.style.height = "100vh";

		//initiation Vue
		await initializeVue(tab, parentTab, parentFileId, scenarioId);
		
	})();
}

function initializeVue(tab, parentTab, parentFileId, scenarioId) {
	(async () => {
		const { default: eAxiosHelper } = await import("../../../../../../IRISBlack/Front/Scripts/helpers/eAxiosHelper.js");
		let workflowInfos;

		if (scenarioId) {
			workflowInfos = await GetScenarioInfo(scenarioId, eAxiosHelper);
		}
		else {
			workflowInfos = await GetWorkflowInfo(parentTab, parentFileId, tab, eAxiosHelper);
        }
		
		let hlpLien = new eAxiosHelper(mainJS.store.state.url + '/IRISBlack/Front/Scripts/components/workflowMarketing/flowy/flowyJson/blockItems.json');
		if (!eTools.isObject(workflowInfos) && typeof (workflowInfos) === 'string')
			workflowInfos = JSON.parse(workflowInfos);
		//in récupère les blocs en json
		let workflowJSBlocks = await hlpLien.GetAsync({ responseType: "application/json" });
		//workflowjsblocks = json.stringify(workflowjsblocks);
		//workflowjsblocks = workflowjsblocks.replace("<eudores>triggertitlewhenrecipientisadded</eudores>", top._res_8793);
		//on le met dans le store
		mainJS.store.commit("setWorkflowBlocks", workflowJSBlocks);

		if (scenarioId) {
			mainJS.store.commit("setTab", workflowInfos.Tab);
			mainJS.store.commit("setParentTab", workflowInfos.EvtTabId);
			mainJS.store.commit("setParentFileId", workflowInfos.EvtFileId);
		}
		else {
			mainJS.store.commit("setTab", tab);
			mainJS.store.commit("setParentTab", parentTab);
			mainJS.store.commit("setParentFileId", parentFileId);
        }
		
		mainJS.store.commit("setCurrentWorkflowId", workflowInfos.ScenarioId);
		mainJS.store.commit("setCurrentWorkflowTriggerId", workflowInfos.WorkflowTriggerId);
		mainJS.store.commit("setCurrentWorkflowTriggerStepId", workflowInfos.WorkflowTriggerStepId);
		mainJS.store.commit("setWorkflowLabel", workflowInfos.Label);
		mainJS.store.commit("setWorkflowTmpLabel", workflowInfos.Label);
		mainJS.store.commit("setTriggerFilterId", workflowInfos.WorkflowTriggerFilterId);
		mainJS.store.commit("setTriggerFilterLabel", workflowInfos.WorkflowTriggerFilterLabel);
		mainJS.store.commit("setTriggerBlockId", workflowInfos.WorkflowTriggerId > 0 ? 0 : -1);
		mainJS.store.commit("setTriggerType", workflowInfos.TriggerType);
		mainJS.store.commit("setActivateScenario", workflowInfos.IsActivated);
		mainJS.store.commit("setSchedulerJobId", workflowInfos.SchedulerJobId);
		mainJS.store.commit("setTriggerScheduleId", workflowInfos.ScheduleId);
		mainJS.store.commit("setTriggerScheduleInfo", workflowInfos.ScheduleDescription);
		mainJS.store.commit("setDelayFrequency", workflowInfos.DelayFrequency);

		//chargement des données flowy
		if (workflowInfos.Datas != null && workflowInfos.Datas != '') {
			let datas = JSON.parse(workflowInfos.Datas);
			let jsonDatas = JSON.parse(datas);
			mainJS.store.commit("setWorkflowDatas", jsonDatas);

			//chargement des infos des campagnes
			workflowInfos.ActionInfos.forEach(actCmpn => {
				jsonDatas.blocks.forEach(b => {
					for (var i = 0; i < b.attr.length; i++) {
						if (b.attr[i].actnid == actCmpn.ActionId) {
							actionInfos[b.data[1].value] = {
								'actionId': actCmpn.ActionId, 'actionType': actCmpn.ActionType, 'delayInput': actCmpn.ActionDelayCount, 'delaySelect': actCmpn.ActionDelayType,
								'campaignId': actCmpn.ActionCampaignId, 'campaignDescr': actCmpn.ActionCampaignDescription, 'campaignState': actCmpn.ActionCampaignStatus,
							};
						}
					}
				});
			});
			//actionInfos[mainJS.store.state.CurrentBlockId] = { 'campaignId': idCampaign, 'campaignDescr': actionSendEmailDescr, 'campaignState': campagnState };
		}
		else
			mainJS.store.commit("setWorkflowDatas", {
				html: '',
				blockarr: [],
				blocks: []
			});

		vueJSInstance = mainJS.initVue();
		mainJS.store.commit("setLoading", false);
	})();
}

/**cette fonction permet de fermer la modale, supprimer les scripts et racharger la liste des formulaires
 * */
async function closeWorkflowModal() {
	vueJSInstance.$destroy();
	mainAddWorkflowModal.hide();

	top.clearHeader("WORKFLOWMARKETING", "ALL");
	top.removeCSS("Res/css/style");

	flowy.removeEventListeners();
}

/**
 * Mettre à jour les ids des actions sauvegardées
 * @param {any} actions liste des actions
 */
async function SaveWorkflowActionResult(actions, url, nWorflowId, tab) {
	for (var i = 0; i < actions.length; i++) {
		var action = actions[i];
		var elem = document.querySelector('input[value="' + action.ActionFlowyId + '"][name="blockid"]');
		if (elem) {
			elem.parentElement.setAttribute('stpid', action.ActionStepDatabaseId);
			elem.parentElement.setAttribute('actnid', action.ActionDatabaseId);
        }
	}

	//on met à jours les données coté back
	const { default: eAxiosHelper } = await import("../../../../../../IRISBlack/Front/Scripts/helpers/eAxiosHelper.js");
	let helper = new eAxiosHelper(url + '/api/MarketingAutomation');
	//construction du Json
	var updateData = getScenarioFields(nWorflowId, tab, 2);
	try {
		//Appeler la méthode Post dans le controller et on récupére la réponse
		var updateReturn = helper.PostAsync.bind(helper);
		var responseJson = JSON.parse(await updateReturn(updateData, function () { }));
	}
	catch (e) {
		mainJS.store.commit("setShowAlert", true);
		mainJS.store.commit("setAlertMessage", top._res_8823);
		mainJS.store.commit("setAlertType", "error");
		mainJS.store.commit("setLoading", false);
	}
}

/**
 * Mettre à jour le store et les blocs flowy
 * @param {any} responseJson
 */
async function updateWorkflowInfos(responseJson, workflowParams) {
	mainJS.store.commit("setCurrentWorkflowId", responseJson.ScenarioId);
	mainJS.store.commit("setCurrentWorkflowTriggerId", responseJson.WorkflowTriggerId);
	mainJS.store.commit("setCurrentWorkflowTriggerStepId", responseJson.WorkflowTriggerStepId);
	mainJS.store.commit("setWorkflowTmpLabel", mainJS.store.state.Label);
	if (responseJson.ActionResult)
		await SaveWorkflowActionResult(responseJson.ActionResult, workflowParams.url, responseJson.ScenarioId, workflowParams.tab);
	mainJS.store.commit("setLoading", false);
	mainJS.store.commit("setWorkflowDatas", flowy.output());
	mainJS.store.commit("setSchedulerJobId", responseJson.SchedulerJobId);
}

/**
 * Chargement de la structure de la page.
 * @param {any} eAxiosHelper la classe de chargement.
 * */
async function SaveWorkflow(workflowParams) {
	//chargement
	mainJS.store.commit("setLoading", true);
	if (!checkWorkflowName()) {
		mainJS.store.commit("setShowAlert", true);
		mainJS.store.commit("setAlertMessage", top._res_8950);
		mainJS.store.commit("setAlertType", "error");
		mainJS.store.commit("setLoading", false);
		return;
    }
		
	//on instancie AxiosHelper pour faire les appels ajax
	const { default: eAxiosHelper } = await import("../../../../../../IRISBlack/Front/Scripts/helpers/eAxiosHelper.js");
	let helper = new eAxiosHelper(workflowParams.url + '/api/MarketingAutomation');
	//construction du Json
	var updateData = getScenarioFields(workflowParams.nWorflowId, workflowParams.tab, 1);
	try {
		//Appeler la méthode Post dans le controller et on récupére la réponse
		var updateReturn = helper.PostAsync.bind(helper);
		var responseJson = JSON.parse(await updateReturn(updateData, function () { }));

		//on affiche les alertes de succes ou d'échec selon le résultat
		if (responseJson) {
			mainJS.store.commit("setLoading", false);
			if (responseJson.Success == true) {
				mainJS.store.commit("setShowAlert", true);
				mainJS.store.commit("setAlertMessage", top._res_8822);
				mainJS.store.commit("setAlertType", "success");
				await updateWorkflowInfos(responseJson, workflowParams);
			}
			else {
				mainJS.store.commit("setShowAlert", true);
				if (responseJson.Message && responseJson.Message != "")
					mainJS.store.commit("setAlertMessage", top._res_8823 + ": " + responseJson.Message);
				else
					mainJS.store.commit("setAlertMessage", top._res_8823);
				mainJS.store.commit("setAlertType", "error");
				mainJS.store.commit("setLoading", false);
			}
		}
		mainJS.store.commit("setLoading", false);
	}
	catch (e) {
		mainJS.store.commit("setShowAlert", true);
		mainJS.store.commit("setAlertMessage", top._res_8823);
		mainJS.store.commit("setAlertType", "error");
		mainJS.store.commit("setLoading", false);
	}
}

/**
 * cette fonction permet de créer l'objet Json qui contient les infors des formulaires
 * @param {any} nWorflowId
 * @param {any} tab
 */
function getScenarioFields(nWorflowId, tab, operation) {
	return {
		'Tab': tab,
		'Label': mainJS.store.state.Label,
		'TmpLabel': mainJS.store.state.TmpLabel,
		'WorkflowId': nWorflowId,
		'Operation': operation, // type d'operation
		'WorkflowTriggerId': mainJS.store.state.WorkflowTriggerId,
		'TriggerBlockId': mainJS.store.state.TriggerBlockId,
		'TriggerType': mainJS.store.state.TriggerType,
		'TriggerFilterId': mainJS.store.state.TriggerFilterId,
		'WorkflowTriggerStepId': mainJS.store.state.WorkflowTriggerStepId,
		'Datas': JSON.stringify(flowy.output()),
		'EvtFileId': mainJS.store.state.ParentFileId,
		'EvtDescId': mainJS.store.state.ParentTab,
		'ScheduleId': mainJS.store.state.TriggerScheduleId,
		'SchedulerJobId': mainJS.store.state.TriggerSchedulerJobId
	}
}

/**
 * Récuperer les info du workflow
 * @param {any} parentTab
 * @param {any} parentFileId
 * @param {any} tab
 * @param {any} eAxiosHelper
 */
async function GetWorkflowInfo(parentTab, parentFileId, tab, eAxiosHelper) {
	let helper = new eAxiosHelper(mainJS.store.state.url + '/api/MarketingAutomation');
	try {
		return await helper.GetAsync({
			params: {
				nParentTab: parentTab,
				nParentFileId: parentFileId,
				nTab: tab
			},
			responseType: 'json'
		});
	} catch (e) {
		throw e;
	}
}

async function GetScenarioInfo(scenarioId, eAxiosHelper) {
	let helper = new eAxiosHelper(mainJS.store.state.url + '/api/MarketingAutomation');
	try {
		return await helper.GetAsync({
			params: {
				nScenarioId: scenarioId
			},
			responseType: 'json'
		});
	} catch (e) {
		throw e;
	}
}

/**
 * Mettre à jour les infos de la campagne
 * @param {any} idCampaign
 * @param {any} title
 * @param {any} description
 * @param {any} savingMode
 */
function updateCampaignInfo(idCampaign, title, description, campagnState) {
	let actionSendEmailDescr = description && description != "" ? description : title;
	if (campagnState == 9) {//campagne validée
		mainJS.store.commit("setShowAlert", true);
		let alertMsg = description && description != "" ? top._res_3118.replace('{desc}', description) : top._res_3118.replace('{desc}', title);
		mainJS.store.commit("setAlertMessage", alertMsg);
		mainJS.store.commit("setAlertType", "success");
		mainJS.store.commit("setActionSendEmailDescr", actionSendEmailDescr); // the campaign description/title
		mainJS.store.commit("setActionMailingButton", true); // true value - will change the content for the button which open the Mailing wizard
		mainJS.store.commit("setActionCampaignState", "" + campagnState); //the campaign state, in this case it will be 1
	}
	else
		if (campagnState == 8 || campagnState == 10) { //campaign saved || test email 
			mainJS.store.commit("setActionSendEmailDescr", actionSendEmailDescr); // the campaign description/title
			mainJS.store.commit("setActionMailingButton", true); // true value - will change the content for the button which open the Mailing wizard
			mainJS.store.commit("setActionCampaignState", "" + campagnState); //the campaign state, in this case it will be 0
		}
	//store in a table for every action block the associated campaign
	actionInfos[mainJS.store.state.CurrentBlockId] = { 'campaignId': idCampaign, 'campaignDescr': actionSendEmailDescr, 'campaignState': campagnState };
	//mettre à jour les infos du bloc action
	updateActionBloc(mainJS.store.state.CurrentBlockId, mainJS.store.state.CurrentElementType, idCampaign, actionSendEmailDescr, campagnState);
}

//content of the tooltip when you hover over the icon of the dropped block
function mOver(event) {
	var tooltipCampagne = document.querySelector('.tooltipCampagne');
	var blocid = event.target.parentElement.parentElement.parentElement.querySelector('.blockid');	
	if (blocid)
		blocid = blocid.value;
	let campaign = actionInfos[blocid];
	if (!campaign)
		return;

	if (actionInfos && campaign) {
		if (campaign.campaignState == '9') {
			tooltipCampagne.innerHTML = "<span id='tooltip1' class='v-tooltip__content'>" + mainJS.store.getters.getRes(8865, '') + " " + campaign.campaignDescr + " " + mainJS.store.getters.getRes(8866, '') + "</span>";
		}
		else {
			tooltipCampagne.innerHTML = "<span id='tooltip1' class='v-tooltip__content'>" + mainJS.store.getters.getRes(8865, '') + " " + campaign.campaignDescr + " " + mainJS.store.getters.getRes(8867, '') + "</span>";
        }
	}
	var $tooltipContainer = $("#tooltip1"),
		posSrollY = 10;	
	$(document).ready(function () {
		$(".iconI").on("mouseover", function () {
			var $this = $(this),
				strongText = $this.text();
			$tooltipContainer.show();
			$tooltipContainer.append('<span>' + strongText + '</span>');
			//condition here if tooltip container is outside the screen
			/*
			 * change the posScrollX and/or posSrollY 
			 * to make the position of the container visible on the screen
			 */
		}).on("mousemove", function (mousePos) {
			var overlap = mousePos.pageY + posSrollY + $tooltipContainer.height() - $(window).height() - $(window).scrollTop();
			$tooltipContainer.css({
				left: mousePos.pageX - '336' +'px',
				top: mousePos.pageY + posSrollY - (overlap > 0 && overlap) - '65' + 'px'
			});
		}).on("mouseleave", function () {
			$tooltipContainer.hide();
			$tooltipContainer.find("span").remove();
		});
	});
		
}

 //to update the description for the action block which depending if exist or not a campaign
function updateActionBloc(blocid, blocType, campaignId, blocDescription, campaignState) {
	var elem = document.querySelector('input[value="' + blocid + '"][name="blockid"]');
	var canvas_div = document.getElementById("canvas");
	var tol = document.createElement("DIV");
	tol.classList.add('tooltipCampagne');
	canvas_div.appendChild(tol);
	if (elem) {
		elem.parentElement.setAttribute('cmpnid', campaignId);
		var blockDescription = elem.parentElement.querySelector(".blockyinfo");
		if (blocDescription != '') {
			if (campaignState == '9') {
				blockDescription.innerHTML = mainJS.store.getters.getRes(8808, '') + " " + "<div class='actionFilterLabel'>" + blocDescription + "</div><div class='styleSuccess'><i onmouseover='mOver(event)' class='fas fa-check iconI'></i></div>";
			}
			else { 
				blockDescription.innerHTML = mainJS.store.getters.getRes(8808, '') + " " + "<div class='actionFilterLabel'>" + blocDescription + "</div><div class='styleWarning'><i onmouseover='mOver(event)' class='fas fa-exclamation-triangle iconI'></i></div>";
			}
		}
		else {
			blockDescription.innerHTML = mainJS.store.getters.getRes(8808, '');
		}

		flowy.rearrangeMe();
	}
}

async function ActiveScenario(workflowParams, callBack) {
	//chargement
	mainJS.store.commit("setLoading", true);
	if (!checkWorkflowName()) {
		mainJS.store.commit("setShowAlert", true);
		mainJS.store.commit("setAlertMessage", top._res_8950);
		mainJS.store.commit("setAlertType", "error");
		mainJS.store.commit("setLoading", false);
		setTimeout(function () { mainJS.store.commit("setActivateScenario", false); }, 200);
		return;
	}
	//on instancie AxiosHelper pour faire les appels ajax
	const { default: eAxiosHelper } = await import("../../../../../../IRISBlack/Front/Scripts/helpers/eAxiosHelper.js");
	let helper = new eAxiosHelper(workflowParams.url + '/api/MarketingAutomation');
	//construction du Json
	//construction du Json
	var updateData = getScenarioFields(workflowParams.nWorflowId, workflowParams.tab, 3);

	try {
		//Appeler la méthode Post dans le controller et on récupére la réponse
		var updateReturn = helper.PostAsync.bind(helper);
		var responseJson = JSON.parse(await updateReturn(updateData, function () { }));
		await updateWorkflowInfos(responseJson, workflowParams);
		if (responseJson == null || !responseJson.Success) {
			//on annule l'action du switch
			if (callBack) {
				callBack(false, false, responseJson.ListErrorCheckValidScenario);
			}
		}
		else {
			//mettre à jour les infos
			if (callBack) {
				callBack(true, true);
			}
			mainJS.store.commit("setShowAlert", true);
			mainJS.store.commit("setAlertMessage", top._res_8909);
			mainJS.store.commit("setAlertType", "success");
			mainJS.store.commit("setAlertTopScenario", "alertTop");
		}
		mainJS.store.commit("setLoading", false);
	}
	catch (e) {
		if (callBack) {
			callBack(false, false);
		}
		mainJS.store.commit("setLoading", false);
	}
}

function openScheduleParameterValidReturn(oModal) {
	oModal.getIframe().Valid(ValidMailingScheduleTreatment, oModal);
}

function ValidMailingScheduleTreatment(oRes, oModal) {
	var TriggerScheduleId = getXmlTextNode(oRes.getElementsByTagName("scheduleid")[0]);
	//SetParam("scheduleId", scheduleId);
	//SetParam("scheduleUpdated", "1");
	mainJS.store.commit("setTriggerScheduleId", TriggerScheduleId);
	mainJS.store.commit("setTriggerScheduleUpdated", "1");
	oModal.hide();
	//var scheduleInfo = document.getElementById("lnkScheduleInfo");
	//if (scheduleInfo) {
	//    scheduleInfo.style.display = "";
	//    SetText(scheduleInfo, " : \"" + getXmlTextNode(oRes.getElementsByTagName("scheduleinfo")[0]) + "\"");
	//}
	var TriggerScheduleInfo = getXmlTextNode(oRes.getElementsByTagName("scheduleinfo")[0]);
	mainJS.store.commit("setTriggerScheduleInfo", TriggerScheduleInfo);
	//to update the description which depending if exist or not a planification 
	var blockDesctiprion = document.querySelector(".blockyinfo");
	if (mainJS.store.state.TriggerFilterLabel == '') {
		if (mainJS.store.state.TriggerScheduleInfo != '')
			blockDesctiprion.innerHTML = "<div class='triggerPlanificationLabel'>" + TriggerScheduleInfo + "</div >" + " " + mainJS.store.getters.getRes(8913, '');
		else
			blockDesctiprion.innerHTML = mainJS.store.getters.getRes(8901, '');
	}
	else if (mainJS.store.state.TriggerFilterLabel != '') {
		if (mainJS.store.state.TriggerScheduleInfo != '')
			blockDesctiprion.innerHTML = "<div class='triggerPlanificationLabel'>" + TriggerScheduleInfo + "</div >" + " " + mainJS.store.getters.getRes(8915, '') + " " + "<div class='triggerFilterLabel'>" + mainJS.store.state.TriggerFilterLabel + "</div >";
		else
			blockDesctiprion.innerHTML = "<div class='triggerPlanificationLabel'>" + TriggerScheduleInfo + "</div >" + " " + mainJS.store.getters.getRes(8913, '');
	}	

}

async function DeactiveScenario(workflowParams, callBack) {
	//chargement
	mainJS.store.commit("setLoading", true);
	if (!checkWorkflowName()) {
		mainJS.store.commit("setShowAlert", true);
		mainJS.store.commit("setAlertMessage", top._res_8950);
		mainJS.store.commit("setAlertType", "error");
		mainJS.store.commit("setLoading", false);
	}
	//on instancie AxiosHelper pour faire les appels ajax
	const { default: eAxiosHelper } = await import("../../../../../../IRISBlack/Front/Scripts/helpers/eAxiosHelper.js");
	let helper = new eAxiosHelper(workflowParams.url + '/api/MarketingAutomation');
	//construction du Json
	//construction du Json
	var updateData = getScenarioFields(workflowParams.nWorflowId, workflowParams.tab, 4);

	try {
		//Appeler la méthode Post dans le controller et on récupére la réponse
		var updateReturn = helper.PostAsync.bind(helper);
		var responseJson = JSON.parse(await updateReturn(updateData, function () { }));
		await updateWorkflowInfos(responseJson, workflowParams);
		if (responseJson == null || !responseJson.Success) {
			//on annule l'action du switch
			if (callBack) {
				if (responseJson.Message == '') {
					callBack(true, true,'Warning', responseJson.Warning);
                }					
				else
					callBack(true, true,'Erreur', responseJson.Message);
			}
		}
		else {
			////mettre à jour les infos
			if (callBack) {
				callBack(false, false);
			}
		}
		mainJS.store.commit("setLoading", false);
	}
	catch (e) {
		if (callBack) {
			callBack(true, true);
		}
		mainJS.store.commit("setLoading", false);
	}
}

async function DeleteDeactiveScenario(workflowParams, callBack) {
	//chargement
	mainJS.store.commit("setLoading", true);
	if (!checkWorkflowName()) {
		mainJS.store.commit("setShowAlert", true);
		mainJS.store.commit("setAlertMessage", top._res_8950);
		mainJS.store.commit("setAlertType", "error");
		mainJS.store.commit("setLoading", false);
		return;
	}
	//on instancie AxiosHelper pour faire les appels ajax
	const { default: eAxiosHelper } = await import("../../../../../../IRISBlack/Front/Scripts/helpers/eAxiosHelper.js");
	let helper = new eAxiosHelper(workflowParams.url + '/api/MarketingAutomation');
	//construction du Json
	//construction du Json
	var updateData = getScenarioFields(workflowParams.nWorflowId, workflowParams.tab, 5);

	try {
		//Appeler la méthode Post dans le controller et on récupére la réponse
		var updateReturn = helper.PostAsync.bind(helper);
		var responseJson = JSON.parse(await updateReturn(updateData, function () { }));
		await updateWorkflowInfos(responseJson, workflowParams);
		if (responseJson == null || !responseJson.Success) {
			//on annule l'action du switch
			if (callBack) {
				callBack(true, true, responseJson.Message);
			}
		}
		else {
			////mettre à jour les infos
			if (callBack) {
				callBack(false, false);
			}
		}
		mainJS.store.commit("setLoading", false);
	}
	catch (e) {
		if (callBack) {
			callBack(true, true);
		}
		mainJS.store.commit("setLoading", false);
	}
}

/** Vérifier les params du workflow
 * */
function checkWorkflowName() {
	if (!mainJS.store.state.Label || mainJS.store.state.Label.trim().length == 0) {
		return false;
	}
	else
		return true;
}