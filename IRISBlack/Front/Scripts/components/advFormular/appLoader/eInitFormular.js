//*****************************************************************************************************//
//*****************************************************************************************************//
//*** KJE - 02/2020 - Afficher le formulaire avancé en VueJS
//** mainAddFormModal: La modale qui doit contenir le formulaire VueJS
//** grapesJsEditor: composant grapesJs
//** mainJS: cette page va initier l'objet Vue
//*****************************************************************************************************//
//*****************************************************************************************************//

var grapesJsEditor = null;
var mainJS = null;
var mainAddFormModal = null;
var vueJSInstance = null;
var FormularEnum = null;

//cette méthode permet de créer le formulaire VueJS en l'initialisant.
function InitializeFormular(modalWizard, tab, formularId, parentFileId) {

	mainAddFormModal = modalWizard;

	(async () => {
		let { LoadJS, LoadCSS } = await import("./LoadIrisScripts.js");
		/** Ici le chargement des 2 fonctions pour le JS et le CSS du nouveau mode fiche. */
		await Promise.all([LoadJS(), LoadCSS()]);
		//on importe le fichier main.js qui initialize vuex et les components
		mainJS = await import("./main.js");
		FormularEnum = await import("../jsonparams/Enum.js");
		//on positionne la modale on top pour prendre 100% de la page
		var modalDiv = document.getElementById('app_assist_form').parentElement;
		modalDiv.className = "ModalOnTop";
		modalDiv.style.height = "100vh";
		//Pour enlever le scroller dans formulaire avancé
		modalDiv.style.overflow = "initial";

		//initiation Store et Vue
		await initializeStore(tab, formularId, parentFileId);

	})();
}

//cette fonction permet d'initialiser l'id du formulaire à zéro ainsi que le nom du formulaire dans le store
function initializeStore(tab, formularId, parentFileId) {
	(async () => {
		//US #1 573: chargement de la liste des blocs pour le formulaire avancé
		const { default: eAxiosHelper } = await import("../../../../../../IRISBlack/Front/Scripts/helpers/eAxiosHelper.js");

		let hlpLien = new eAxiosHelper(mainJS.store.state.url + '/IRISBlack/Front/scripts/components/advFormular/grapesjs/eudonet.json');
		//in récupère les blocs en json
		let grapesJSBlocks = await hlpLien.GetAsync({ responseType: "application/json" });
		//on remplace <eudoRes> pour pouvoir injecter les res dans eMemoEditor
		grapesJSBlocks = JSON.stringify(grapesJSBlocks);
		grapesJSBlocks = grapesJSBlocks.replace(new RegExp('<eudoRes>'.replace(/[-\/\\^$*+?.()|[\]{}]/g, '\\$&'), 'g'), "\" + grapesjs.xrmLang[this.language].blockCustomRes['");
		grapesJSBlocks = grapesJSBlocks.replace(new RegExp('</eudoRes>'.replace(/[-\/\\^$*+?.()|[\]{}]/g, '\\$&'), 'g'), "'] + \"");
		//on le met dans le store
		mainJS.store.state.grapesJSBlocks = grapesJSBlocks;

		//Tâche #2 717: Chargement des champs de fusion
		//TODO: le charement des champs defusion sera dans le chargement des données globales du formulaires        
		await GetFormularGloalParam(tab, eAxiosHelper);


		if (mainJS.store.state.IsWorldLineExtensionIsActivated) {
		hlpLien = new eAxiosHelper(mainJS.store.state.url + '/IRISBlack/Front/scripts/components/advFormular/grapesjs/onlinePaiment.json');
		//in récupère les blocs en json
		let worldlinePaimentBlocs = await hlpLien.GetAsync({ responseType: "application/json" });
		//on remplace <eudoRes> pour pouvoir injecter les res dans eMemoEditor
		worldlinePaimentBlocs = JSON.stringify(worldlinePaimentBlocs);
		worldlinePaimentBlocs = worldlinePaimentBlocs.replace(new RegExp('<eudoRes>'.replace(/[-\/\\^$*+?.()|[\]{}]/g, '\\$&'), 'g'), "\" + grapesjs.xrmLang[this.language].blockCustomRes['");
		worldlinePaimentBlocs = worldlinePaimentBlocs.replace(new RegExp('</eudoRes>'.replace(/[-\/\\^$*+?.()|[\]{}]/g, '\\$&'), 'g'), "'] + \"");
		//on le met dans le store
		mainJS.store.commit("setWorldLinePaimentBlocs", worldlinePaimentBlocs);
        }
		

		mainJS.store.commit("setEvtFileId", parentFileId);
		// Tache #2 676 la chargement de formulaire existant
		if (formularId != 0)
			await GetFormularById(formularId, parentFileId, eAxiosHelper);
		else {
			hlpLien = new eAxiosHelper(mainJS.store.state.url + '/IRISBlack/Front/scripts/components/advFormular/grapesjs/formularModel.json');
			//in récupère les blocs en json
			let formularModel = await hlpLien.GetAsync({ responseType: "application/json" });
			let newFormularBody = formularModel.htmlNewFormularModel.replace("<eudoRes>title</eudoRes>", top._res_2794);
			newFormularBody = newFormularBody.replace("<eudoRes>description</eudoRes>", top._res_2795);
			newFormularBody = newFormularBody.replace("<eudoRes>mailInput</eudoRes>", top._res_2365);
			newFormularBody = newFormularBody.replace("<eudoRes>btnValidate</eudoRes>", top._res_5003);
		 
			mainJS.store.commit("setbodycss", formularModel.cssNewFormularModel);
			mainJS.store.commit("setformularbody", newFormularBody);
		}

		vueJSInstance = mainJS.initVue();
	})();

	mainJS.store.state.tab = tab;
	mainJS.store.state.nFormularId = formularId;
	mainJS.store.state.formularName = top._res_1726;
	mainJS.store.state.body = "";
	mainJS.store.state.css = "";
	mainJS.store.state.LangId = (top && top.hasOwnProperty("_userLangId")) ? top._userLangId : "0";
	mainJS.store.state.Published = false;
	mainJS.store.commit("setAcknowledgmentSelect", FormularEnum.AcknowledgmentSelect.ThankingMessage);
	mainJS.store.commit("setSubmissionBody", "<div style=\"text-align:center; \"><span style=\"font-size: 16pt\">" + top._res_2652 + "</span></div>");
	mainJS.store.state.submissionBodyCss = "";
	mainJS.store.state.submissionRedirectUrl = "";

	mainJS.store.state.ExpireDate = "";
	mainJS.store.state.StartDate = "";
	mainJS.store.state.MsgDateStart = "<div style=\"text-align:center; \"><span style=\"font-size: 16pt\">" + top._res_2738 + "</span></div>";
	mainJS.store.state.MsgDateEnd = "<div style=\"text-align: center; \"><span style=\"font-size: 16pt\">" + top._res_2739 + "</span></div>";

  

}

//cette fonction permet de fermer la modale, supprimer les scripts et racharger la liste des formulaires
async function closeFormularModal() {
	vueJSInstance.$destroy();
	mainAddFormModal.hide();
	top.removeCSS("Res/css/style");
	top.clearHeader("ADVANCEDFORMULAR", "ALL");
	top.window['_md']['oModalFormularList'].getIframe().ReloadList();
}

/**
 * Chargement de la structure de la page.
 * @param {any} eAxiosHelper la classe de chargement.
 * */
async function SaveFormular(formularParams, callBack) {
	//contrôle sur le nom du formulaire (s'il est renseigné ou pas)
	if (formularParams.formularName.length == 0 || formularParams.formularName.trim().length == 0) {
		eAlert(0, top._res_5080, top._res_2771, top._res_1722);
		if (callBack)
			callBack(null);
		return;
	}

	//on instancie AxiosHelper pour faire les appels ajax
	const { default: eAxiosHelper } = await import("../../../../../../IRISBlack/Front/Scripts/helpers/eAxiosHelper.js");
	let helper = new eAxiosHelper(formularParams.url + '/api/FormularManager');
	//construction du Json
   
		var updateData = getFormularFields(formularParams.formularName, formularParams.nFormularId, formularParams.tab);

	try {
		//Appeler la méthode Post dans le controller et on récupére la réponse
		if (updateData.Status == FormularEnum.FormularState.Published) {
			//Initialisation de l'objet Erreur
			var frmValidation = new eErrorValidation();
			if (grapesJsEditor.validateGrapesJSComponents(frmValidation) &&
				validateFormularComponents(frmValidation)
				&& !frmValidation.isValid) {
				if (formularParams.isNewPublication)
					eAlert(0, top._res_3080, top._res_2650, frmValidation.toString(), 450, 300, null, true, true);
				else
					eAlert(0, top._res_3080, top._res_3081, frmValidation.toString(), 450, 300, null, true, true);
				if (callBack)
					callBack();
				return;
			}
		}
		
		let imagedata='';
		if (updateData.ImageFormular instanceof File && mainJS.store.state.ImageHasChanged)
			imagedata =updateData.ImageFormular;
		else if (updateData.ImageFormular instanceof File && !mainJS.store.state.ImageHasChanged)
		{
			imagedata = '';
		}
		else if( !(updateData.ImageFormular instanceof File) && mainJS.store.state.ImageHasChanged)
		{
			updateData.MetaImgURL = '';
		}	
		
		updateData.ImageFormular=null;

		let formdata = new FormData();
		formdata.append("formulardata",JSON.stringify(updateData));
		if(imagedata)
			formdata.append("imageFile", imagedata);
		else
			 imagedata= '';
  

		const requestconfig = {
			headers: {
				"Content-Type": "multipart/form-data"
			}
		};
				
		var responseJson = JSON.parse(await helper.PostAsync(
			formdata,
			requestconfig
		));

		//on affiche les alertes de succes ou d'échec selon le résultat
		if (responseJson) {
			if (responseJson.Success == true) {
				mainJS.store.state.nFormularId = responseJson.FormularId;
				mainJS.store.commit("setFormularLink", responseJson.RewrittenURL);
				mainJS.store.commit("setFormularIntegrationScript", responseJson.ScriptIntegration);
				mainJS.store.commit("setViewPermId", responseJson.ViewPerm.PermId);
				//mainJS.store.commit("setUpdatePermId", responseJson.RewrittenURL);
				if (formularParams.isNewPublication)
					eAlert(4, top._res_5080, top._res_8838, top._res_2775, 450, 300, null); //Publication : Formulaire en ligne
				else
					eAlert(4, top._res_5080, responseJson.Message, responseJson.Detail, 450, 300, null);
			}
			else {
				if (responseJson.DisableIfPublished)//On désactive la publication
					eAlert(0, top._res_8797, responseJson.Message, responseJson.Detail, 450, 300, null);
				else
					eAlert(0, top._res_5080, responseJson.Message, responseJson.Detail, 450, 300, null);
			}
			//permet de lancer un traitement après un retour (callback)
			if (callBack)
				callBack(responseJson);
		}
	}
	catch (e) {
		throw e;
	}
}

//cette fonction permet de créer l'objet Json qui contient les infors des formulaires
function getFormularFields(formularName, nFormularId, tab) {
	let viewPermMode, viewPermLevel, viewPermUser;
	let updatePermMode, updatePermLevel, updatePermUser;

	//On met à jour les permUser et les permLevelavant le sauvegarde
	if (!mainJS.store.state.ShowViewPerm) {
		viewPermMode = -1;
		viewPermLevel = "";
		viewPermUser = "";
	}
	else {
		viewPermMode = mainJS.store.state.ViewPermMode;
		viewPermLevel = mainJS.store.state.ViewPermLevel;
		viewPermUser = mainJS.store.state.ViewPermUser;
		if (viewPermUser == "") {
			switch (viewPermMode) {
				case 1: viewPermMode = -1; break;
				case 3: viewPermMode = 0; break;
				default: break;
			}
		}

		if (viewPermLevel == "") {
			switch (viewPermMode) {
				case 0: viewPermMode = -1; break;
				case 3: viewPermMode = 1; break;
				default: break;
			}
		}
	}

	if (!mainJS.store.state.ShowUpdatePerm) {
		updatePermMode = -1;
		updatePermLevel = "";
		updatePermUser = "";
	}
	else {
		updatePermMode = mainJS.store.state.UpdatePermMode;
		updatePermLevel = mainJS.store.state.UpdatePermLevel;
		updatePermUser = mainJS.store.state.UpdatePermUser;
		if (updatePermUser == "") {
			switch (updatePermMode) {
				case 1: updatePermMode = -1; break;
				case 3: updatePermMode = 0; break;
				default: break;
			}
		}

		if (updatePermLevel == "") {
			switch (updatePermMode) {
				case 0: updatePermMode = -1; break;
				case 3: updatePermMode = 1; break;
				default: break;
			}
		}
	}

	return {
		'Tab': tab,
		'MailDisplayName': '',
		'FormularName': formularName,
		'Operation': 1, // type de formulaire : Avancé 
		'FormularType': '1',
		'Body': grapesJsEditor.htmlTemplateEditor.getHtml(),
		'BodyCss': grapesJsEditor.getCss("GRAPESJS", true),
		'FormularId': nFormularId,
		'FormularLang': mainJS.store.state.LangId,
		'FormularStatus': mainJS.store.state.Published ? '1' : '0',
		'Status': mainJS.store.state.Published ? FormularEnum.FormularState.Published : FormularEnum.FormularState.NotPublished,
		'BodySubmission': mainJS.store.state.submissionBody,
		'BodySubmissionCss': mainJS.store.state.submissionBodyCss,
		'SubmissionRedirectUrl': mainJS.store.state.isValidRedirectUrl ? mainJS.store.state.submissionRedirectUrl : "",
		'FormularExtendedParam': {
			AcknowledgmentSelect: mainJS.store.state.AcknowledgmentSelect,
			AccentuationColor: mainJS.store.state.AccentuationColor,
			ButtonBackgroundColor: mainJS.store.state.ButtonBackgroundColor,
			LinkColor: mainJS.store.state.LinkColor,
			PoliceColor: mainJS.store.state.PoliceColor,
			ButtonPoliceColor: mainJS.store.state.ButtonPoliceColor,
			FontSize: mainJS.store.state.FontSize,
			FontName: mainJS.store.state.FontName
		},
		'EvtFileId': mainJS.store.state.EvtFileId,
		'StartDate': mainJS.store.state.StartDate,
		'ExpireDate': mainJS.store.state.ExpireDate,
		'MsgDateStart': mainJS.store.state.MsgDateStart,
		'MsgDateEnd': mainJS.store.state.MsgDateEnd,
		'IsPublic': mainJS.store.state.PublicFormular,
		'viewPermId': mainJS.store.state.ViewPermId,
		'PermMode': viewPermMode,
		'PermLevel': viewPermLevel,
		'PermUser': viewPermUser,
		'UpdatePermId': mainJS.store.state.UpdatePermId,
		'UpdatePermMode': updatePermMode,
		'UpdatePermUser': updatePermUser,
		'UpdatePermLevel': updatePermLevel,
		'MetaTitle': mainJS.store.state.MetaTitle,
		'MetaDescription': mainJS.store.state.MetaDescription,
		'MetaImgURL': mainJS.store.state.MetaImgURL,
		'ImageFormular': mainJS.store.state.FileImage
	}
}

async function GetFormularGloalParam(nTab, eAxiosHelper) {
	let helper = new eAxiosHelper(mainJS.store.state.url + '/api/FormularParam');
	try {
		//on charge la liste des params globaux
		let globalParams = await helper.GetAsync({
			params: {
				nTab: nTab
			},
			responseType: 'json'
		});

		if (!eTools.isObject(globalParams) && typeof (globalParams) === 'string')
			globalParams = JSON.parse(globalParams);

		mainJS.store.state.mergeFields = globalParams.MergeFields;//Liste des champs de fusion
		mainJS.store.state.AllAvailableLng = globalParams.AvailableLanguages;//a liste des langues
		mainJS.store.state.UserInfos = globalParams.UserInfos;//informations utilisateur

		mainJS.store.state.IsWorldLineExtensionIsActivated = globalParams.IsWorldLineExtensionIsActivated;//a liste des langues

		mainJS.store.commit("setMergeFieldsWithoutExtended", globalParams.MergeFieldsWithoutExtendedFields);
		mainJS.store.commit("setHyperLinksMergeFields", globalParams.HyperLinkMergeFields);

	} catch (e) {
		throw e;
	}
}

//cette méthode permet de récupérer les données d'un formulaire xrm
async function GetFormularById(formularId, parentFileId, eAxiosHelper) {
	let helper = new eAxiosHelper(mainJS.store.state.url + '/api/FormularManager');
	try {
		//appel de la méthode coté serveur
		var responseformular = await helper.GetAsync({
			params: {
				formularId: formularId,
				parentFileId: parentFileId
			},
			responseType: 'json'
		});

		if (!eTools.isObject(responseformular) && typeof (responseformular) === 'string')
			responseformular = JSON.parse(responseformular);
		//on injecte dans le store
		mainJS.store.state.formularId = responseformular.FormularId;
		mainJS.store.state.formularName = responseformular.FormularName;
		mainJS.store.state.body = responseformular.Body;
		mainJS.store.state.bodyCss = responseformular.Css;
		mainJS.store.state.LangId = responseformular.FormularLang;
		mainJS.store.commit("setPublished", responseformular.FormularStatus == '1');
		if (responseformular.FormularExtendedParam)
			mainJS.store.state.AcknowledgmentSelect = responseformular.FormularExtendedParam.AcknowledgmentSelect;
		else
			mainJS.store.state.AcknowledgmentSelect = (responseformular.SubmissionBody || !responseformular.SubmissionRedirectUrl) ? FormularEnum.AcknowledgmentSelect.ThankingMessage : FormularEnum.AcknowledgmentSelect.URLRedirection;
		mainJS.store.state.submissionBody = responseformular.SubmissionBody;
		mainJS.store.state.submissionBodyCss = responseformular.SubmissionBodyCss;
		mainJS.store.state.submissionRedirectUrl = responseformular.SubmissionRedirectUrl;
		mainJS.store.commit("setFormularLink", responseformular.RewrittenURL);
		mainJS.store.commit("setFormularIntegrationScript", responseformular.ScriptIntegration);

		mainJS.store.state.ExpireDate = responseformular.ExpireDate;
		mainJS.store.state.StartDate = responseformular.StartDate;
		mainJS.store.state.MsgDateStart = responseformular.MsgDateStart;
		mainJS.store.state.MsgDateEnd = responseformular.MsgDateEnd;
		mainJS.store.commit("setPublicFormular", responseformular.IsPublic);
		mainJS.store.commit("setViewPermId", responseformular.ViewPerm.PermId);
		if (responseformular.ViewPerm.PermMode == 0 && responseformular.ViewPerm.PermLevel == 0 && responseformular.ViewPerm.PermUser == "")
			mainJS.store.commit("setViewPermMode", -1);
		else
			mainJS.store.commit("setViewPermMode", responseformular.ViewPerm.PermMode);
		mainJS.store.commit("setViewPermLevel", responseformular.ViewPerm.PermLevel);
		mainJS.store.commit("setViewPermUser", responseformular.ViewPerm.PermUser);
		mainJS.store.commit("setViewPermUserDisplay", responseformular.ViewPerm.PermUserDisplay);

		mainJS.store.commit("setUpdatePermId", responseformular.UpdatePerm.PermId);
		if (responseformular.UpdatePerm.PermMode == 0 && responseformular.UpdatePerm.PermLevel == 0 && responseformular.UpdatePerm.PermUser == "")
			mainJS.store.commit("setUpdatePermMode", -1);
		else
			mainJS.store.commit("setUpdatePermMode", responseformular.UpdatePerm.PermMode);
		mainJS.store.commit("setUpdatePermLevel", responseformular.UpdatePerm.PermLevel);
		mainJS.store.commit("setUpdatePermUser", responseformular.UpdatePerm.PermUser);
		mainJS.store.commit("setUpdatePermUserDisplay", responseformular.UpdatePerm.PermUserDisplay);
		//Meta
		mainJS.store.commit("setMetaTitle", responseformular.MetaTitle);
		mainJS.store.commit("setMetaDescription", responseformular.MetaDescription);
		//Personnalisation des couleurs et boutons et police
		mainJS.store.commit("setAccentuationColor", responseformular.FormularExtendedParam.AccentuationColor);
		mainJS.store.commit("setButtonBackgroundColor", responseformular.FormularExtendedParam.ButtonBackgroundColor);
		mainJS.store.commit("setButtonPoliceColor", responseformular.FormularExtendedParam.ButtonPoliceColor);
		mainJS.store.commit("setLinkColor", responseformular.FormularExtendedParam.LinkColor);
		mainJS.store.commit("setPoliceColor", responseformular.FormularExtendedParam.PoliceColor);
		mainJS.store.commit("setFontSize", responseformular.FormularExtendedParam.FontSize);
		mainJS.store.commit("setFontName", responseformular.FormularExtendedParam.FontName);
		mainJS.store.state.MetaImgURL = responseformular.MetaImgURL;
		if(mainJS.store.state.MetaImgURL)
		{            
			mainJS.store.state.FileImage = await createFile();
		}
		
		mainJS.store.commit("formularStore/setFormularTS")

	} catch (e) {
		throw e;
	}
}

async function createFile ()
		{
			let file;
			
				if(mainJS.store.state.MetaImgURL)
				{
					let response = await fetch(mainJS.store.state.url  + '/api/FormularManager?Name='+mainJS.store.state.MetaImgURL);
					let data = await response.blob();
					let metadata = {
						type: 'image/*'
					};
					 file = new File([data],mainJS.store.state.MetaImgURL, metadata);
				}
				   
			
			return file;
		}

//Contrôle sur l'URL de redirection ou le message de remerciement
function validateFormularComponents(errorValidation) {
	switch (mainJS.store.state.AcknowledgmentSelect) {
		case FormularEnum.AcknowledgmentSelect.URLRedirection:
			if (!mainJS.store.state.isValidRedirectUrl || mainJS.store.state.submissionRedirectUrl == null || mainJS.store.state.submissionRedirectUrl == '')
				errorValidation.addNewError(eTools.FormularValidationErrorCode.URLRedirectionError, eTools.getRes(eTools.FormularValidationErrorCode.URLRedirectionError) + '.');
			break;
		case FormularEnum.AcknowledgmentSelect.ThankingMessage:
			if (mainJS.store.state.submissionBody == null || mainJS.store.state.submissionBody == '')
				errorValidation.addNewError(eTools.FormularValidationErrorCode.MessageRedirectionError, eTools.getRes(eTools.FormularValidationErrorCode.MessageRedirectionError) + '.');
			break;
		default:
			break;
	}
	return true;
}

/**
 * Chargement de la structure de la page.
 * @param {any} eAxiosHelper la classe de chargement.
 * */
async function UpdateFormularRes(nLangId, nRes) {
	//on instancie AxiosHelper pour faire les appels ajax
	const { default: eAxiosHelper } = await import("../../../../../../IRISBlack/Front/Scripts/helpers/eAxiosHelper.js");
	let helper = new eAxiosHelper(mainJS.store.state.url + '/api/FormularParam');
	try {
		//on charge la liste des params globaux
		let globalRes = await helper.GetAsync({
			params: {
				nLangId: parseInt(nLangId),
				nRes: nRes
			},
			responseType: 'json'
		});
		grapesJsEditor.UpdateRes(globalRes);
	}
	catch (e) {
	}
}
