import { PropType, EngineConfirmModes, MsgType, FieldType, XrmCruAction, AdrField } from './Enum.js?ver=803000';
import { setRunSpec, loadFileCreateUpdate } from "../shared/XRMWrapperModules.js?ver=803000";
import { linkToPost, linkToPut } from "./eFileMethods.js?ver=803000"
import { getTabDescid } from "./eMainMethods.js?ver=803000"
import EventBus from "../bus/event-bus.js?ver=803000";

/**
 * Suivant le format du champ, inclus dans le datainput,
 * On retourne soit le descid, soit autre chose.
 * @param {any} ctx le contexte appelant.
 * @returns {any} le bon descid
 */
function getRightDescIDFromType(ctx) {
    switch (ctx.dataInput.Format) {
        case FieldType.AliasRelation: return ctx.dataInput.TargetTab;
        default: return ctx.dataInput.DescId;
    }
}

/**
 * Suivant le format du champ ou d'autres critères, inclus dans le datainput,
 * On retourne le type d'éditeur au sens E17 (eFieldEditor.type) qui serait utilisé pour éditer le champ en question.
 * Cette propriété est requise pour certains traitements transverses et communs entre E17 et IRIS (ex : la MAJ des MRU)
 * Source : eMain.initEditors(). TOUS les eFieldEditor de l'application E17 sont initialisés dans cette méthode.
 * Les autres appels à new eFieldEditor() étant, soit des appels avec le type "inlineEditor" pour initialiser ponctuellement un éditeur simple pour un besoin spécifique (ex : champ pour renommer
 * un élément dans une liste spécifique type Liste des filtres, rapports, graphiques...), soit un appel pour initialiser un éditeur global qui n'aurait pas été initialisé par eMain.initEditors()
 * suite à un contexte particulier (ex : besoin d'utiliser un éditeur depuis un widget de page d'accueil)
 * @param {any} ctx le contexte appelant.
 * @returns {any} le type d'éditeur E17 utilisé pour modifier le champ en question ("new eFieldEditor(type)")
 */
function getLegacyFieldEditorType(ctx) {
    // Types pouvant être déterminés à partir du format
    switch (ctx.dataInput.Format) {
        case FieldType.Date:
            return "dateEditor";
        case FieldType.Relation:
        case FieldType.AliasRelation:
            return "linkCatFileEditor";
        case FieldType.User:
            return "catalogUserEditor";
        case FieldType.Button:
            return "eBitButton";
        case FieldType.Logic:
            return "eCheckBox";
        case FieldType.File:
            return "fileEditor";
        case FieldType.Geolocation:
            return "geolocEditor";
        case FieldType.MailAddress:
            return "mailEditor";
        case FieldType.Memo:
            return "memoEditor";
    }
    // Types catalogue devant être déterminés autrement
    // Catalogue avancé
    if (ctx.dataInput.PopupType == POPUPTYPE.DATA) {
        // Catalogue "Etape"
        if (ctx.dataInput.PopupDataRend == POPUPDATARENDER.STEP)
            return "stepCatalogEditor";
        // Autres types de catalogue (hors Utilisateur, déterminé plus haut)
        else
            return "catalogEditor";
    }
    // Type d'éditeur de base, par défaut : champ <input> simple
    else
        return "inlineEditor";
}

/**
 * Affiche ou masque loader invitant l'utilisateur à patienter le temps du traitement
 * @param {any} bOn true s'il doit être affiché, false s'il doit être masqué
 */
export function setWaitIris(bOn) {
    let loadBlockAll = document.getElementById("waiter");
    let contentWait = document.getElementById("contentWait");

    if (bOn) {
        loadBlockAll.className = "waitOn waitOnIris";
        contentWait.style.display = "block";
    }
    else {
        Vue.nextTick(function () {
            contentWait.style.display = "none";
            loadBlockAll.className = "waitOff";
        });
    }
}
/**
 * Renvoie le conteneur de l'élément en cours (dataInput) en fonction du contexte (mode Liste, mode Fiche, zone Assistant, zone Résumé...)
 * @param {object} ctx le contexte d'appel (this, that...)
 * @returns {object} du rien
 * */
export function getContainerElement(ctx) {
    let container = null;       
    let elem = ctx?.$parent?.$el;

    if (!ctx || !ctx.dataInput || !elem?.tagName)
        return null;

    if (ctx.propDetail)
        container = elem?.querySelector('div[divdescid="' + ctx.dataInput.DescId + '"]');
    else if (ctx.propAssistant)
        container = elem?.querySelector('div[divdescidassitzone="' + ctx.dataInput.DescId + '_' + ctx.propAssistantNbIndex + '"]');
    else if (ctx.propListe)
        container = elem?.querySelector('#' + 'id_input_liste_' + ctx.propIndexRow + '_' + ctx.dataInput.DescId);
    else if (ctx.propHead || ctx.propResumeEdit)
        container = elem?.querySelector('div[divdescid="' + ctx.dataInput.DescId + '"]')

    if (container == null)
        container = ctx.$el;

    return container;
}

/**
 *  permet de mettre a jours le front
 * @param {object} ctx le contexte d'appel (this)
 * @param {object} newValue l'élement qui envoie la demande. Soit sous forme de string (valeur seule) soit sous forme d'objet (NewValue + NewDisplay) pour les catalogues
 * @param {object} oldValue l'ancienne valeur saisie sur le champ, pour restauration en cas d'erreur. Soit sous forme de string (valeur seule) soit sous forme d'objet (OldValue + OldDisplay) pour les catalogues
 * @param {object} additionalUpdateData paramètres de mise à jour additionnels à ajouter à ceux transmis de base par cette fonction vers Engine
 * @param {object} inputData l'ensemble des données de la fiche.
 * @returns {object} du rien
 * */
export async function updateMethod(ctx, newValue, oldValue, additionalUpdateData, inputData) {

    ctx.eWaiter({ bOn: true, nOpacity: 0.3 });
 
    // Appel au contrôleur de MAJ
    let updateReturn;
    let responseJson;
    let oDataCreateUpdate = loadFileCreateUpdate();
    if (!(oDataCreateUpdate))
        return;

    // Données transmises au contrôleur
    let bParentsLinks = ctx.dataInput?.ParentLinks?.some(pl => pl.DescId == ctx.dataInput.DescId);
    var nTab = bParentsLinks ? ctx.getTab : getTabDescid(ctx.dataInput.DescId); //ctx.getTab;
    var fileId = bParentsLinks ? ctx.getFileId : ctx.dataInput.FileId; //ctx.getFileId;

    //modification d'une relation système parente depuis un signet (par ex fiche event => signet destinataire=> Rubrique Contacts.Nom)
    if (
        ctx.dataInput.Format == FieldType.AliasRelation
        && ctx.dataInput.DescId % 100 == 1
        && ctx.propSignet?.DescId
        && ctx.propSignet?.DescId != nTab
        && ctx.MainFileId > 0) {

        nTab = ctx.propSignet.DescId;
        fileId = ctx.MainFileId;
    }

    // Analyse des paramètres newValue/oldValue
    let sNewValue = newValue;
    let sNewDisplay = newValue;
    let sOldValue = oldValue;
    let sOldDisplay = oldValue;
    if (newValue && newValue.NewValue && newValue.NewDisplay) {
        sNewValue = newValue.NewValue;
        sNewDisplay = newValue.NewDisplay;
    }
    if (oldValue && oldValue.OldValue && oldValue.OldDisplay) {
        sOldValue = oldValue.OldValue;
        sOldDisplay = oldValue.OldDisplay;
    }  
    ctx.$emit('saveScrollPosition');

    var updateData = {
        'Fields': [
            {
                /*
                'AddValueInCatalog': false,
                */
                'BoundDescid': (inputData ? inputData.BoundDescId : 0),
                /*
                'BoundPopup': null,
                'BoundValue': '',
                'Case': null,
                'ChangedValue': null,
                */
                'Descid': getRightDescIDFromType(ctx),
                /*
                'ForceUpdate': false,
                'Format': null, // ATTENTION : Format côté IRIS correspond à l'Enum FieldType et non pas à l'Enum EudoQuery.FieldFormat. Il ne faut donc pas utiliser dataInput.Format (FieldType) ici pour renseigner 'Format' (FieldFormat)
                'IsB64': false,
                'IsCatDesc': false,
                'IsCatEnum': false,
                'IsStepMode': false,
                */
                'Multiple': (inputData ? inputData.Multiple : false),
                'NewDisplay': sNewDisplay,
                'NewValue': sNewValue,
                'OldValue': sOldValue,
                'Popup': (inputData ? inputData.PopupType : null),
                //'PopupDataRend': null,
                'PopupDescId': (inputData ? inputData.PopupDescId : 0),
                'ReadOnly': (inputData ? inputData.ReadOnly : false),
                /*
                'RealName': '',
                'Table': null,
                'Unicode': null
                */

            }
        ],
        'TabDescId': nTab,
        'FileId': fileId,
        'FieldTrigger': ctx.dataInput.DescId,
        'FieldEditorType': getLegacyFieldEditorType(ctx),
        'TriggerOnBlurAction': true//TODO à modifier quand on fera la validation d'une fiche complète (création, template en popup)

    };

    updateData = Object.assign(updateData, additionalUpdateData);

    let updateAreas = {};

    let root = ctx.$root.$children.find(child => child.$options.name == 'App').$children.find(child => child.$options.name == 'fiche');
    
    // Permet de recharger les zones où le champs est situé dans la fiche.
    updateAreas = {
        // updateBkmArea: false,
        // updateSummaryArea: root?.JsonSummary?.inputs?.find(input => input?.DescId == inputData?.DescId),
        // updateWizardArea: root?.JsonWizardBar?.FieldsById?.filter(fld => fld?.DisplayedFields?.find(fld => fld?.DescId == inputData?.DescId)).length > 0,
        // updateDetailsArea: (ctx?.propDetail || root?.tabAllDetail?.filter(fld => fld?.DescId == inputData?.DescId)) && !additionalUpdateData?.stepBarUpdateTriggered,
        updateAllAreas: inputData?.IsInRules,
        // On recharge  uniquement si il y a une formule ORM, du mileu, un comportement conditionnel ou un changement d'étape
        needToUpdate: inputData?.IsInRules || inputData?.HasMidFormula || inputData?.HasORMFormula,
    }

    // Envoi de la MAJ au contrôleur
    try {
        let updateReturn = null;
        let updateCallbackFct = function () {
            ctx.validatedData = { Values: sNewValue, Labels: ctx.selectedLabels ? [ctx.selectedLabels].join(";") : "" }; // Labels est utilisée pour passer la DisplayValue. Pas besoin de passer newValue (object) à Values, uniquement la valeur seule (string)
            ctx.updateCallbackData = ctx.onUpdateCallback(true, ctx.validatedData, ctx, updateAreas);
        };

        // Modification d'un fichier existant : envoi d'une requête PUT
        updateReturn = await linkToPost(oDataCreateUpdate.url, updateData, updateCallbackFct);
        // Comme on souhaite gérer les retours d'erreur explicitement, on ne passe pas par JSONTryParse ici
        responseJson = JSON.parse(updateReturn);
    }
    catch (e) {
        if (!responseJson)
            responseJson = { 'Messages': undefined };

        if (!responseJson.ErrorMessages)
            responseJson.ErrorMessages = new Array();
        responseJson.ErrorMessages.push({ Title: top._res_72, Description: e, Detail: e });

        ctx.updateCallbackData = ctx.onUpdateCallback(false, null, e, inputData);
    }

    let bRefreshMainAddress = ctx.dataInput.DescId == AdrField.PRINCIPALE; // Si mise à jour du champ Adresse.Principale (412), traitement spécifique

    // Initialisation d'un objet eEngine E17 pour appeler les fonctions historiques en attendant qu'elles soient redéveloppées pour le nouveau mode Fiche
    var legacyEngineObject = new eEngine();
    legacyEngineObject.Init();

    // #US 1170 - Tâche 2051 - Gestion des retours de formules avec popup
    switch (responseJson.ConfirmBoxMode) {
        // Gestion du CHECK ADDRESS
        case EngineConfirmModes.ADDRESS_CHECK: // 1
            if (responseJson && responseJson.OtherParameters) {
                var adrdescid = responseJson.OtherParameters.descid;
                var adrtoupd = responseJson.OtherParameters.adrtoupd;
                var adrnoupd = responseJson.OtherParameters.adrnoupd;
                var callbackFct = function (updatedEngineObject) {
                    // US #1495/#3561 - Tâche #2514/#5292 - On indique à Engine de procéder à la MAJ en fonction du retour de la popup
                    if (!additionalUpdateData)
                        additionalUpdateData = new Object();
                    additionalUpdateData.EngineAction = updatedEngineObject.GetParam("engAction")
                    additionalUpdateData.AddressToUpdate = updatedEngineObject.GetParam("adrtoupd");
                    updateMethod(ctx, newValue, oldValue, additionalUpdateData, inputData);
                }
                legacyEngineObject.ShowCheckAdr(adrdescid, adrtoupd, adrnoupd, callbackFct);
            }

            ctx.eWaiter({ bOn: false, nOpacity: 0 });

            return;
        // Gestion du MIDDLE PROC
        // Gestion de la confirmation de suppression
        // Gestion de la confirmation de fusion
        case EngineConfirmModes.MIDDLE_PROC: // "2"
        case EngineConfirmModes.DELETE: // "3"
        case EngineConfirmModes.MERGE: // "6"
            // MessageBox du message ou demande confirmation si l'on veux forcer la mise à jour de la valeur incorrect
            if (responseJson) {
                if (responseJson.ConfirmBoxMode == EngineConfirmModes.MIDDLE_PROC) {
                    showMiddleConfirm(ctx, newValue, oldValue, responseJson);
                }
                else if (responseJson.ConfirmBoxMode == EngineConfirmModes.DELETE) {
                    showCustomConfirm(
                        responseJson,
                        function () { /*legacyEngineObject.validSupConfirm();*/ },
                        function () { /*legacyEngineObject.cancelSupConfirm();*/ },
                        false
                    );
                }
            }
            else if (engineConfirm == EngineConfirmModes.MERGE) {
                /*
                legacyEngineObject.ShowCustomConfirm(lstConfirmBox[0],
                    function () { legacyEngineObject.validMergeConfirm(); },
                    function () { legacyEngineObject.cancelMergeConfirm(); });
                */
            }

            ctx.eWaiter({ bOn: false, nOpacity: 0 });

            return;
        // Gestion de la confirmation de suppression avec confirmation de suppression des PP en cascade
        case EngineConfirmModes.DELETE_PM_PP: // "4"
            /*
            legacyEngineObject.ShowSupPpConfirm(responseJson.ConfirmBoxInfoElements.fldmaindispval);
            */

            ctx.eWaiter({ bOn: false, nOpacity: 0 });

            return;
        // Gestion de la confirmation de suppression avec confirmation de detachement ou suppression du rdv
        case EngineConfirmModes.DELETE_PLANNING_MULTI_OWNER: // "5"
            // MessageBox du message ou demande confirmation si l'on veux forcer la mise à jour de la valeur incorrect
            var fldmultiownerdid = responseJson.ConfirmBoxInfoElements.fldmultiownerdid;
            var multiownernewval = responseJson.ConfirmBoxInfoElements.multiownernewval;
            /*
            legacyEngineObject.ShowSupMultiOwnerConfirm(responseJson, legacyEngineObject.GetParam("fileId"), fldmultiownerdid, multiownernewval);
            */

            ctx.eWaiter({ bOn: false, nOpacity: 0 });

            return;
        // Gestion de la confirmation de formule du milieu de l'ORM
        case EngineConfirmModes.ORM_CONFIRM: // "7"
            if (responseJson)
                showOrmMiddleConfirm(ctx, newValue, oldValue, responseJson, inputData);

            ctx.eWaiter({ bOn: false, nOpacity: 0 });

            return;
        // Gestion d'annumlation de l'operation via l'ORM
        case EngineConfirmModes.ORM_CANCEL: // "8"
            // Information de la MessageBox de la confirmation de ORM_CANCEL
            if (!responseJson) {
                // Si pas de message, on annule la saisie
                cancelMiddleConfirm(ctx, responseJson, oldValue);
            }
            else {
                // Sinon affichage du message, puis on annule la saisie au clic sur le bouton
                showCustomConfirm(
                    responseJson,
                    null,
                    function () {
                        cancelMiddleConfirm(ctx, responseJson, oldValue);
                    },
                    false
                );
            }

            ctx.eWaiter({ bOn: false, nOpacity: 0 });

            return;
    }

    // Affichage des messages d'erreur critiques (interrompent la MAJ)
    if (responseJson.ErrorMessages && responseJson.ErrorMessages.length > 0) {
        // Cas de plusieurs messages à afficher : eEngine.js > ShowProcMessage les affichait tous simultanément, sans attendre
        for (var errMsgIndex = 0; errMsgIndex < responseJson.ErrorMessages.length; errMsgIndex++) {
            var errorMsg = responseJson.ErrorMessages[errMsgIndex];
            showConfirmOrAlert(errorMsg.Criticity, errorMsg.Title, errorMsg.Description, errorMsg.Detail, function () { }, function () { }, false);
        }
        // Annulation de la MAJ dans tous les cas
        cancelUpdate(ctx, responseJson, oldValue, additionalUpdateData);

        ctx.eWaiter({ bOn: false, nOpacity: 0 });

        return false;
    }
    else {
        // envoyer sNewDisplay si elle existe, pour eRelation
        if (sNewDisplay) {
            responseJson['sNewDisplay'] = sNewDisplay;
        }
        updateListValue(ctx, `[field='field${ctx.dataInput.DescId}']`, sNewValue, sOldValue, responseJson);


    }

    // Affichage des messages d'erreur fonctionnels (sans interruption de la MAJ - cf. eEngine.js > ShowProcMessage les affichait tous simultanément, sans attendre)
    if (responseJson.Messages && responseJson.Messages.length > 0) {
        // Cas de plusieurs messages à afficher : eEngine.js > ShowProcMessage les affichait tous simultanément, sans attendre
        for (var msgIndex = 0; msgIndex < responseJson.Messages.length; msgIndex++) {
            var msg = responseJson.Messages[msgIndex];
            showConfirmOrAlert(msg.Criticity, msg.Title, msg.Description, msg.Detail, function () { }, function () { }, false);
        }
    }

    if (responseJson.URLs && responseJson.URLs.length > 0) {
        forEach(responseJson.URLs, function (url) {

            var nSpecId;
            var nTab;
            var nFileId;
            var nFieldDescId;


            var aURL = url.split('$|$');

            // Remarque : on teste qu'il y ait 5 composantes car Engine.SpecifXrmInfo.GetUrlParams() en renvoie 5
            // Id_de_la_specif$|$Table_de_la_rubrique_déclencheur$|$Id_de_la_fiche_à_l'orgine_de_l'automatisme$|$Rubrique_déclencheur$|$Rubrique_déclencheur_initiale
            // Même si, dans le cas présent, on n'en utilise que 4, ça permet de s'assurer que le retour d'Engine
            // n'ait pas été altéré
            // cf. eEngine.js, ShowProcPage()
            if (aURL.length == 5) {

                nSpecId = aURL[0];
                nTab = aURL[1];
                nFileId = aURL[2];
                nFieldDescId = aURL[3];

                setRunSpec(nSpecId, nTab, nFileId, nFieldDescId);
            }

        });

    }


    // Si un reload global est demandé, on l'effectue a ce niveau
    // #96 179/#96 219 - Sauf pour les champs Mémo HTML, ça semble perturber leur encodage
    if (responseJson.ReloadInfos) {
        if (!(ctx.dataInput.Format == FieldType.Memo && ctx.dataInput.IsHtml) && (responseJson.ReloadInfos.ReloadFileHeader ||  responseJson.ReloadInfos.ReloadHeader || responseJson.ReloadInfos.ReloadDetail)) {
            ctx.eWaiter({ bOn: false, nOpacity: 0 });
            await EventBus.$emit("emitLoadAll", {
                reloadAll: true,
            })
            return;
        }
    }


    // On dresse la liste des colonnes dont la somme est activée, et devant être actualisée
    let aComputedValuesToRefresh = new Array();

    // On met à jour les valeurs dans le champ de saisie (précédemment effectué sur onUpdateCallback())
    // ctx.dataInput.Value = ctx.validatedData.Values;
    if (FieldType.Date == ctx.dataInput.Format && additionalUpdateData) {


 

        // On reformat la date au bon format
        let tabDateFormat = ["DD/MM/YYYY HH:mm", "DD-MM-YYYY HH:mm", "YYYY-MM-DD HH:mm", "YYYY/MM/DD HH:mm", "MM/DD/YYYY HH:mm", "dddd D MMMM YYYY HH:mm", "ddd DD/MM/YYYY HH:mm", "ddd D MMM YYYY HH:mm"]

        let dateDefautFormat = 'DD/MM/YYYY';
        let timeDefautFormat = 'HH:mm:ss';

        let newFormatStart = moment(additionalUpdateData.Fields[1].NewValue, tabDateFormat, 'fr');
        let newFormatEnd = moment(additionalUpdateData.Fields[0].NewValue, tabDateFormat, 'fr');

        let returnDateRangeStart = newFormatStart.format(dateDefautFormat + ' ' + timeDefautFormat);
        let returnDateRangeEnd = newFormatEnd.format(dateDefautFormat + ' ' + timeDefautFormat);

        //ELAIZ - On vérifie que la valeur est différente de "" pour éviter de renvoyer un format invalid au moment de parser 

        let newDateStartValue = additionalUpdateData.Fields[1].NewValue
        let newDateEndValue = additionalUpdateData.Fields[0].NewValue

        ctx.dataInput.DateStartValue = newDateStartValue != undefined ? returnDateRangeStart : "";
        ctx.dataInput.DateEndValue = newDateEndValue != undefined ? returnDateRangeEnd : "";

        let dateRangePicker = {
            FileId : fileId,
            DateStart : {
                DescId : additionalUpdateData.Fields[1].Descid,
                Value :  ctx.dataInput.DateStartValue,

            },
            DateEnd : {
                DescId : additionalUpdateData.Fields[0].Descid,
                Value :  ctx.dataInput.DateEndValue,
            }
        }

        EventBus.$emit('valueEditedDataPicker', dateRangePicker);

        //ctx.dataInput.DateEndValue = additionalUpdateData.Fields[0].NewValue;
        //ctx.dataInput.DateStartValue = additionalUpdateData.Fields[1].NewValue;

    }
    if ([FieldType.AliasRelation, FieldType.Relation, FieldType.Alias, FieldType.User, FieldType.Catalog, FieldType.Catalog].indexOf(ctx.dataInput.Format) > -1) {
        if (ctx.dataInput.DisplayValue != ctx.validatedData.Labels) {
            ctx.dataInput.DisplayValue = ctx.validatedData.Labels;
        }
    } else {
        // if (ctx.dataInput.DisplayValue != ctx.validatedData.Values) {
        //     ctx.dataInput.DisplayValue = ctx.validatedData.Values;
        // }
    }

    if (ctx.dataInput.IsComputable) {
        let nTabId = ctx.propSignet?.DescId;
        let oTabComputedValues = aComputedValuesToRefresh.find(element => element.Tab == nTabId);
        if (!oTabComputedValues) {
            oTabComputedValues = { Tab: nTabId, ComputedValues: new Array() };
            aComputedValuesToRefresh.push(oTabComputedValues);
        }
        if (oTabComputedValues.ComputedValues.indexOf(ctx.dataInput.DescId) == -1)
            oTabComputedValues.ComputedValues.push(ctx.dataInput.DescId);
    }

    //Chargement des valeurs modifiées dans l'interface
    let bkmIptWthFormula = ctx.propListe && (ctx.dataInput.Formula || ctx.dataInput.HasMidFormula || ctx.dataInput.HasORMFormula);
    if(!bkmIptWthFormula)
        refreshNewValues(ctx, responseJson, aComputedValuesToRefresh);
    else
        reloadBkm(ctx)

    //rafraichissement des règles
    await refreshRulesExecution(ctx, responseJson);



    // #81 758 - Rafraîchissement de l'affichage de la StepBar, si cette méthode updateMethod() n'a pas déjà été appelée par la méthode de MAJ de la stepBar elle-même
    if (!additionalUpdateData || !additionalUpdateData.stepBarUpdateTriggered) {
        let options = {
            newValue: newValue,
            inputData: inputData,
            alreadyUpdated: true, /* indique à la fonction déclenchée par l'EventBus, de ne pas redéclencher d'appel à updateMethod(), puisqu'on vient tout juste de faire le traitement en question */
        };
        EventBus.$emit('checkStepBarField', options);
    }

    // #82 113 - Rafraîchissement des cases à cocher Adresse Principale liées à un même FileID
    if (responseJson.RefreshFieldNewValues.find(f => f.DescId == AdrField.PRINCIPALE))
        bRefreshMainAddress = true;

    // #82 113 - Rafraîchissement des cases à cocher Adresse Principale liées à un même FileID
    if (bRefreshMainAddress) {
        let options = {
            ctx: ctx,
            additionalUpdateData: additionalUpdateData,
            inputData: inputData,
        };
        EventBus.$emit('RefreshMainAddress', options);
    }

    // #88 984 - Rafraîchissement des sommes des colonnes sur chaque signet cible (un écouteur par signet)
    aComputedValuesToRefresh.forEach(element => {
        let options = {
            descIds: element.ComputedValues
        };
        EventBus.$emit('RefreshComputedValues_' + element.Tab, options);
    });

    setTimeout(function () { ctx.modif = false; }, 250);

    let oParent;

    if (ctx.propListe
        && ctx.$parent)
        oParent = ctx.$parent;


    if (inputData) {
        let descTab = { descid: inputData.TargetTab, value: inputData.Value, displayvalue: inputData.DisplayValue };

        if (ctx.getEvtid?.descid == inputData.TargetTab)
            ctx.setnEvtid(descTab);

        if (inputData.TargetTab == 200)
            ctx.setnPPid(descTab);

        if (inputData.TargetTab == 300)
            ctx.setnPMid(descTab);
    }

    ctx.eWaiter({ bOn: false, nOpacity: 0 });

    await EventBus.$emit("emitLoadAll", {
         reloadSignet: true
    });
};

export function refreshNewValues(ctx, responseJson, aComputedValuesToRefresh) {

    if (responseJson.RefreshFieldNewValues) {
        var cacheSignet = []; // contient les DescId du signet en cours        
        for (var i = 0; i < responseJson.RefreshFieldNewValues.length; i++) {
            var papa = null; // conteneur du champ à mettre à jour
            var targ = new Array(); // champ(s) à mettre à jour (peuvent être en plusieurs exemplaires dans la fiche)
            let currentSignet = null;

            // Fonction à appeler pour retirer l'indicateur de MAJ après MAJ
            var refreshNewValuesRemoveBorderFct = function (targetElt) {
                RemoveBorderSuccessError(targetElt);
                ctx.bDisplayPopup = false;
            };

            // Sélection du résultat d'Engine correspondant au champ à mettre à jour
            let tabResult = (responseJson.RefreshFieldNewValues) ?
                responseJson.RefreshFieldNewValues.filter(fld => fld.Field && fld.Field.Descid == responseJson.RefreshFieldNewValues[i].Field.Descid).flatMap(nwVal => nwVal.List)
                : null;

            // S'il s'agit du champ modifié, on met à jour ctx.dataInput
            // #82 587 : Et on indique de rappeler updateListVal() avec la valeur corrigée pour que la valeur mise à jour par Engine ne soit pas écrasée par le premier appel de updateListVal() plus haut
            // (ex : remise à zéro d'une case à cocher utilisée comme déclencheur d'automatisme)
            if (responseJson.RefreshFieldNewValues[i].Field && responseJson.RefreshFieldNewValues[i].Field.Descid == ctx.dataInput.DescId) {
                targ.push({ input: ctx.dataInput, updateListVal: true });
            }
            // S'il s'agit d'un champ tiers modifié par un automatisme, on le cible
            //else {
            let nTargetFileId = responseJson.RefreshFieldNewValues[i].List?.length > 0 ? responseJson.RefreshFieldNewValues[i].List[0].FileId : 0;
            // Soit en le retrouvant dans les données JSON du composant parent ou grand-parent
            if (ctx?.$parent?.$parent?.DataJson?.Data && nTargetFileId) {
                let oNewTargetFromParents = [...ctx.$parent.$parent.DataJson.Data]
                    .flatMap(data => data?.LstDataFields)
                    .find(n => n.FileId == responseJson.RefreshFieldNewValues[i].List[0].FileId && n.DescId == responseJson.RefreshFieldNewValues[i].Field.Descid);
                if (oNewTargetFromParents)
                    targ.push({ input: oNewTargetFromParents, updateListVal: false });
            }
            // Puis, on poursuit la recherche d'autres champs (ex : champ situé dans l'entête alors que la modification a lieu depuis un signet),
            // en ciblant depuis la racine de l'application, tous les endroits susceptibles de contenir le champ
            let oRootFile = ctx?.$root?.$children;             
                                
            let app = oRootFile?.find(f => f.$options.name == 'App')?.$children;

            if (app)
                oRootFile = app;

            let vap = oRootFile?.find(vapp => vapp.$options.name == "v-app")?.$children;

            if (vap)
                oRootFile = vap;

            let vmain = oRootFile?.find(vapp => vapp.$options.name == "v-main")?.$children;

            if (vmain)
                oRootFile = vmain;

            let fiche = oRootFile?.find(vapp => vapp.$options.name == "fiche");

            if (fiche)
                oRootFile = fiche;

            let oNewTargetFromHeader;
            let oNewTargetFromAside;
            let oNewTargetFromDetail;
            
            if (oRootFile) {
                oNewTargetFromHeader = oRootFile.tabHeader?.inputs?.find(a => a.DescId == responseJson.RefreshFieldNewValues[i].Field.Descid);
                oNewTargetFromAside = oRootFile.tabAside?.find(a => a.DescId == responseJson.RefreshFieldNewValues[i].Field.Descid);
                oNewTargetFromDetail = oRootFile.tabAllDetail?.find(a => a.DescId == responseJson.RefreshFieldNewValues[i].Field.Descid);
                if (oNewTargetFromHeader) { targ.push({ input: oNewTargetFromHeader, updateListVal: false }); } // zone Résumé
                if (oNewTargetFromAside) { targ.push({ input: oNewTargetFromAside, updateListVal: false }); } // zone latérale Notes/Description
                if (oNewTargetFromDetail) { targ.push({ input: oNewTargetFromDetail, updateListVal: false }); } // zone Détails
            }
            //}

            // Après ciblage, mise à jour de la valeur à partir de tabResult
            targ?.forEach(oTargetToUpdate => {
                if (!oTargetToUpdate.input)
                    return;

                let oldValueBeforeUpdate = oTargetToUpdate.input.Value;
                // MAJ de la valeur
                oTargetToUpdate.input.Value = (tabResult != null) ? tabResult.map(dbVal => dbVal.DbValue).join(';') : sNewValue;
                // #79 441 - Il ne faut faire cette mise à jour que sur les champs où DisplayValue a été explicitement créée et déclarée dans eFileMethods.js (cf. // Set value sous titre header fiche)
                // Sinon, on créerait alors un objet Vue dont les propriétés ne seraient pas synchrones avec ce qu'on utilise réellement, et la mise à jour des champs identiques sur la page ne se ferait
                // alors plus. (MAB/QBO)
                // Il faut donc tester l'existence exacte de la propriété (targ.DisplayValue !== null et typeof (targ.DisplayValue) !== "undefined") et non pas uniquement :
                // - le fait qu'elle soit définie à vide (targ.DisplayValue == "") car la mise à jour ne se ferait que si la propriété a bien été créée, mais que la valeur du champ est vide en BDD (= impossible de modifier un champ valorisé).
                // - le fait qu'elle soit true ou équivalente à true (if (targ.DisplayValue)) car dans le cas où la valeur serait vide en base (targ.DisplayValue == ""), un if (targ.DisplayValue) renvoie false
                if (typeof (oTargetToUpdate.input.DisplayValue) !== "undefined" && oTargetToUpdate.input.DisplayValue !== null) {
                    oTargetToUpdate.input.DisplayValue = (tabResult != null) ? tabResult.map(dbVal => dbVal.DisplayValue).join(';') : sNewDisplay;
                }
                // Indicateur visuel de MAJ
                AddBorderSuccess(
                    document.querySelectorAll('div[divdescid="' + oTargetToUpdate.input.DescId + '"][fileid="' + oTargetToUpdate.input.FileId + '"]'),
                    refreshNewValuesRemoveBorderFct,
                    1500
                );
                
                // #82 587 : Rappel de updateListVal() avec la valeur corrigée si on traite le champ déclencheur
                // Ceci, pour que la valeur mise à jour par Engine ne soit pas écrasée par le premier appel de updateListVal() plus haut
                // (ex : remise à zéro d'une case à cocher utilisée comme déclencheur d'automatisme)
                if (oTargetToUpdate.updateListVal) {
                    updateListValue(ctx, `[field='field${oTargetToUpdate.input.DescId}']`, oTargetToUpdate.input.Value, oldValueBeforeUpdate, responseJson);
                }

                // #88 984 : Si le champ mis à jour est une colonne sur laquelle la somme des colonnes est active, on réactualise la somme
                if (oTargetToUpdate.input.IsComputable && typeof (aComputedValuesToRefresh) != 'undefined') {
                    let nTabId = ctx.propSignet?.DescId;
                    let oTabComputedValues = aComputedValuesToRefresh.find(element => element.Tab == nTabId);
                    if (!oTabComputedValues) {
                        oTabComputedValues = { Tab: nTabId, ComputedValues: new Array() };
                        aComputedValuesToRefresh.push(oTabComputedValues);
                    }
                    if (oTabComputedValues.ComputedValues.indexOf(oTargetToUpdate.input.DescId) == -1)
                        oTabComputedValues.ComputedValues.push(oTargetToUpdate.input.DescId);
                }
            });

            if (oNewTargetFromHeader) {
                EventBus.$emit("emitLoadAll", {
                    reloadHead: true
                });
            }

            //#83 528 -Depuis la zone activité on mets à jour le signet concerné puis on fait un reload sur le signet en question

            // #96187 - [Regression][803] Lors d'un automatisme les signets sont pas raffraichis | QBO
            currentSignet = ctx.$root.$children?.find(f => f.$options.name == 'App').$refs.file.$refs.tabsBar.getSignet.find(a => a.DescId == responseJson.ReloadBkms[i])

            if (currentSignet != null || typeof currentSignet !== 'undefined') {
                if (cacheSignet.indexOf(currentSignet.DescId) == -1) {
                    // #83 877 - Repositionnement du scroll à l'endroit précédent avant MAJ
                    //let currentList = document.getElementById(currentSignet.id)?.querySelector("#listContent");
                    // #US 3 108 retour 5 146  - Problème d'enregistrement du scroll
                    let currentList = document.getElementById(currentSignet.id)?.querySelector(".horizontal-scroll");

                    let id = `id_${currentSignet.DescId}_${currentSignet.Label}`;

                    var options = {
                        id: id,
                        signet: currentSignet.DescId,
                        nbLine: 9,
                        pageNum: 1,
                        scrollLeft: currentList?.scrollLeft,
                        bkmLoaded: false
                    };

                    EventBus.$emit('reloadSignet_' + id, options);
                    cacheSignet.push(currentSignet.DescId);
                }
            }

        }
    }

}

export async function refreshRulesExecution(ctx, responseJson) {
    if (responseJson.DescidRuleUpdated?.length > 0) {
        let tabsBar = null;
        try {
            tabsBar = ctx.$root.$children?.find(f => f.$options.name == 'App')
            .$children?.find(fiche => fiche.$options.name == 'v-app')
            .$children?.find(fiche => fiche.$options.name == 'v-main')
            .$children?.find(fiche => fiche.$options.name == 'fiche')
            .$children?.find(f => f.$options.name == 'tabsBar')

            if (!tabsBar)
                throw "composant tabsBar introuvable"

            await tabsBar.selfReloadBkm();

        }
        catch (e) {
            console.log("erreur lors du rechargement de la barre des signets");
            console.log(e)
        }

    }

}

/**
 * affiche une boîte de dialogue de message fonctionnel (type ORM) à partir du flux JSON renvoyé par le contrôleur via Engine
 * @param {object} responseJson le retour JSON d'Engine contenant toutes les infos permettant l'affichage
 * @param {object} okFct fonction à exécuter si l'utilisateur valide la popup
 * @param {object} cancelFct fonction à exécuter si l'utilisateur annule la popup
 * @param {boolean} adjustToContent indique si on doit ajuster la taille de la fenêtre à son contenu
 */
export function showCustomConfirm(responseJson, okFct, cancelFct, adjustToContent) {
    if (!responseJson)
        return;

    var msgType = responseJson.ConfirmBoxInfoAttributes.type;
    var msgTitle = responseJson.ConfirmBoxInfoElements.title;
    var msgDescription = responseJson.ConfirmBoxInfoElements.desc;
    var msgDetail = responseJson.ConfirmBoxInfoElements.detail;

    // Si on a pas encore affiché la popup, on l'affiche
    if (responseJson.ConfirmBoxInfoAttributes && responseJson.ConfirmBoxInfoElements) {
        // Cas 1 : ResultConfirmBox.BoxInfo est de type ResultBoxInfo
        if (responseJson.ConfirmBoxInfoAttributes.type) {
            /*
            var typeModal = "alert";
            var color = "danger";
            var type = "zoom";
            var close = true;
            var maximize = false;
            var id = "alert-modal";
            var btns =
                responseJson.ConfirmBoxMode == EngineConfirmModes.ORM_CONFIRM
                    ? [{ lib: top._res_58, color: 'success', type: 'left' }, { lib: top._res_59, color: 'default', type: 'left' }] // Oui / Non (TOCHECK)
                    : [{ lib: top._res_30, color: 'default', type: 'left' }]
                ;
            switch (responseJson.ConfirmBoxInfoAttributes.type) {
                case MsgType.INFOS:
                    typeModal = "alert";
                    color = "info";
                    break;
                // TODO / TOCHECK: il n'existe pour l'instant pas de alertQuestion en CSS (cf. ficheGrid.css)
                case MsgType.QUESTION:
                    typeModal = "alert";
                    color = "info";
                    break;
                case MsgType.SUCCESS:
                    typeModal = "alert";
                    color = "success";
                    break;
                case MsgType.EXCLAMATION:
                    typeModal = "alert";
                    color = "warning";
                    break;
                case MsgType.CRITICAL:
                    typeModal = "alert";
                    color = "danger";
                    break;
            }

            // MAB - A vérifier en revue de code - ctx.$emit sans effet ?
            // Utilisation de eModalDialog, il semble pourtant l'instant impossible de câbler une action sur les boutons
            EventBus.$emit('globalModal', {
                //ctx.$emit('globalModal', {
                typeModal: typeModal,
                color: color,
                type: type,
                close: close,
                maximize: maximize,
                id: id,
                title: responseJson.ConfirmBoxInfoElements.title,
                msgData: responseJson.ConfirmBoxInfoElements.detail,
                width: 600,
                btns: btns,
                datas: responseJson.ConfirmBoxInfoElements.desc // Renvoie ResultConfirmBox.BoxInfo.Msg
            });
            */

            var oMod = showConfirmOrAlert(msgType, msgTitle, msgDescription, msgDetail, okFct, cancelFct, adjustToContent);
        }
    }
};

export function showConfirmOrAlert(msgType, msgTitle, msgDescription, msgDetail, okFct, cancelFct, adjustToContent) {
    var oMod = null;
    if (msgType == MsgType.QUESTION) {
        oMod = eConfirm(msgType, msgTitle, msgDescription, msgDetail, 450, 200, okFct, cancelFct);
        if (adjustToContent)
            oMod.adjustModalToContent(40);
    }
    else {
        oMod = eAlert(msgType, msgTitle, msgDescription, msgDetail, 450, 300, cancelFct);
    }

    return oMod;
};

/**
 * annule la saisie en cours après erreur, en fonction du contexte
 * @param {object} ctx Contexte d'exécution (généralement this = objet VueJS)
 * @param {object} responseJsonOrErrorMsg Réponse reçue du serveur, en format JSON (objet JS), ou message d'erreur à remonter (string)
 * @param {object} oldValue l'ancienne valeur saisie sur le champ avant tentative de MAJ. Soit sous forme de string (valeur seule) soit sous forme d'objet (OldValue + OldDisplay) pour les catalogues
 * @param {object} additionalUpdateData données de MAJ transmises, pouvant éventuellement contenir plusieurs champs et leurs anciennes valeurs respectives
 * @returns {void}
 */
export function cancelUpdate(ctx, responseJsonOrErrorMsg, oldValue, additionalUpdateData) {
    if (!ctx || !responseJsonOrErrorMsg)
        return false;

    if (responseJsonOrErrorMsg.Messages && responseJsonOrErrorMsg.Messages.length > 0)
        ctx.messageError = responseJsonOrErrorMsg.Messages[0].Description;
    else
        ctx.messageError = responseJsonOrErrorMsg;

    // Analyse des paramètres newValue/oldValue
    let sOldValue = oldValue;
    let sOldDisplay = oldValue;
    if (oldValue && oldValue.OldValue && oldValue.OldDisplay) {
        sOldValue = oldValue.OldValue;
        sOldDisplay = oldValue.OldDisplay;
    }

    // Annulation de la saisie sur le champ principal/déclencheur de la modification
    var targ = ctx.dataInput;
    if (targ) {
        if (responseJsonOrErrorMsg.Messages && responseJsonOrErrorMsg.Messages.length > 0)
            targ.ErrorMessage = responseJsonOrErrorMsg.Messages[0].Description;
        else
            targ.ErrorMessage = responseJsonOrErrorMsg;

        var papa = ctx.$parent.$el.querySelector('div[divdescid="' + targ.DescId + '"]');
        if (papa) {
            ctx.bDisplayPopup = true;
            // On n'écrase pas la valeur saisie/présente dans le champ si on a pas de quoi remettre l'ancienne valeur
            if (typeof (sOldValue) != 'undefined' && sOldValue != null) {
                targ.Value = sOldValue;
            }
            if (typeof (sOldDisplay) != 'undefined' && sOldDisplay != null) {
                targ.DisplayValue = sOldDisplay;
            }
            // Ajout de la bordure rouge sur le champ
            papa.classList.add("border-error");
            setTimeout(RemoveBorderSuccessError, 1500, papa);
        }
    }

    // Annulation de la saisie sur les champs annexes (impactés par la mise à jour)
    if (additionalUpdateData && additionalUpdateData.Fields) {
        additionalUpdateData.Fields.forEach(elem => {

            let oRootFile = ctx?.$root?.$children;

            let app = oRootFile?.find(f => f.$options.name == 'App')?.$children;

            if (app)
                oRootFile = app;

            let vap = oRootFile?.find(vapp => vapp.$options.name == "v-app")?.$children;

            if (vap)
                oRootFile = vap;

            let vmain = oRootFile?.find(vapp => vapp.$options.name == "v-main")?.$children;

            if (vmain)
                oRootFile = vmain;

            let fiche = oRootFile?.find(vapp => vapp.$options.name == "fiche");

            if (fiche)
                oRootFile = fiche;

            targ = oRootFile.tabAllDetail.find(a => a.DescId == elem.Descid);

            if (targ) {
                papa = ctx.$parent.$el.querySelector('div[divdescid="' + targ.DescId + '"]');
                if (papa) {
                    ctx.bDisplayPopup = true;
                    // On n'écrase pas la valeur saisie/présente dans le champ si on a pas de quoi remettre l'ancienne valeur
                    if (typeof (elem.OldValue) != 'undefined' && elem.OldValue != null)
                        targ.Value = elem.OldValue;
                    if (typeof (elem.OldDisplay) != 'undefined' && elem.OldDisplay != null)
                        targ.DisplayValue = elem.OldDisplay;
                    // Ajout de la bordure rouge sur le champ
                    papa.classList.add("border-error");
                    setTimeout(RemoveBorderSuccessError, 1500, papa);
                }
            }
        }
        );
    }
    ctx.modified = true;
};


/**
 * affiche la fenêtre popup résultant de l'exécution d'une formule du milieu classique (non ORM)
 * @param {object} ctx contexte d'exécution
 * @param {string} newValue valeur saisie à l'origine par l'utilisateur dans le champ ayant déclenché l'automatisme
 * @param {string} oldValue ancienne valeur saisie dans le champ ayant déclenché l'automatisme
 * @param {object} responseJson objet contenant les données de retour d'Engine au complet
 */
export function showMiddleConfirm(ctx, newValue, oldValue, responseJson) {
    if (!responseJson || !responseJson.ConfirmBoxInfoAttributes)
        return;
    else
        return showCustomConfirm(
            responseJson,
            function () {
                validMiddleConfirm(ctx, newValue, oldValue, responseJson.ConfirmBoxInfoAttributes.did, true);
            },
            function () {
                cancelMiddleConfirm(ctx, responseJson, oldValue);
            },
            true
        );
};

/**
 * fonction exécutée à la validation de la popup de la formule du milieu non ORM
 * @param {object} ctx contexte d'exécution
 * @param {string} newValue valeur saisie à l'origine par l'utilisateur dans le champ ayant déclenché l'automatisme. Soit sous forme de string (valeur seule) soit sous forme d'objet (NewValue + NewDisplay) pour les catalogues
 * @param {object} oldValue l'ancienne valeur saisie sur le champ, pour restauration en cas d'erreur. Soit sous forme de string (valeur seule) soit sous forme d'objet (OldValue + OldDisplay) pour les catalogues
 * @param {int} descId descID du champ ciblé sur lequel forcer la MAJ
 * @param {bool} forceUpd indique si on doit forcer la MAJ du champ ou non
 */
export function validMiddleConfirm(ctx, newValue, oldValue, descId, forceUpd) {
    // Analyse des paramètres newValue/oldValue
    let sNewValue = newValue;
    let sNewDisplay = newValue;
    if (newValue && newValue.NewValue && newValue.NewDisplay) {
        sNewValue = newValue.NewValue;
        sNewDisplay = newValue.NewDisplay;
    }

    // A l'application de la formule du milieu, on indique à la fonction de MAJ qu'il faut forcer la MAJ sur le champ ciblé
    // La fonction updateMethod() fusionnera le JSON qu'on lui passe avec celui qu'elle construit de base
    var updateData = {
        'Fields': [
            {
                'Descid': descId,
                'ForceUpdate': forceUpd,
                'NewValue': sNewValue,
                'NewDisplay': sNewDisplay
            }
        ],
    };
    updateMethod(ctx, newValue, oldValue, updateData);
};

/**
 * affiche la fenêtre popup résultant de l'exécution d'une formule du milieu ORM
 * @param {object} ctx contexte d'exécution
 * @param {string} newValue valeur saisie à l'origine par l'utilisateur dans le champ ayant déclenché l'automatisme. Soit sous forme de string (valeur seule) soit sous forme d'objet (NewValue + NewDisplay) pour les catalogues
 * @param {object} oldValue l'ancienne valeur saisie sur le champ, pour restauration en cas d'erreur. Soit sous forme de string (valeur seule) soit sous forme d'objet (OldValue + OldDisplay) pour les catalogues
 * @param {object} responseJson objet contenant les données de retour d'Engine au complet, contenant notamment la boîte de dialogue avec retour ORM (EngineResult.BoxInfo)
 * @param {object} inputData champ déclencheur à l'origine de l'opération
 */
export function showOrmMiddleConfirm(ctx, newValue, oldValue, responseJson, inputData) {

    /*
    var ormConfirmDialog = new eModalDialog("ORM", 0, responseJson.ConfirmBoxInfoElements.url);
    ormConfirmDialog.setParam("OrmId", responseJson.ConfirmBoxInfoAttributes.id);
    ormConfirmDialog.setParam("OrmUpdates", responseJson.ConfirmBoxInfoElements.ormupdates);
    ormConfirmDialog.addButton(top._res_58, validMiddleConfirm, "", null, "ok");
    ormConfirmDialog.addButton(top._res_59, validMiddleConfirm, "", null, "cancel");
    ormConfirmDialog.show();
    */

    if (!ctx || !responseJson)
        return;
    var oConfirmBoxInfoAttributes = responseJson.ConfirmBoxInfoAttributes;
    var oConfirmBoxInfoElements = responseJson.ConfirmBoxInfoElements;
    if (!oConfirmBoxInfoAttributes || !oConfirmBoxInfoElements)
        return;

    var title = "";		// Demande des architectes, titre de la fenêtre vide
    // Tâche #2348 - Si la fenêtre a déjà été ouverte par le même évènement asynchrone, on ferme l'existante pour la remplacer par celle du dernier automatisme déclenché
    // TOCHECK: s'il y a réellement besoin d'afficher plusieurs modales/gérer plusieurs affichages de popup en simultané, il faudra peut-être les référencer dans un tableau
    if (ctx.ormConfirmModal) {
        ctx.ormConfirmModal.hide();
    }
    ctx.ormConfirmModal = new eModalDialog(title, 0, oConfirmBoxInfoElements.url, 550, 500);

    // Contexte d'exécution en attente du retour de la popup ORM - Stocké dans le contexte global "top" (cf. ci-dessous)
    top.irisBlackOrmUpdateContext = { ctx: ctx, newValue: newValue, oldValue: oldValue, inputData: inputData, responseJson: responseJson, ConfirmBoxInfoAttributes: oConfirmBoxInfoAttributes, ConfirmBoxInfoElements: oConfirmBoxInfoElements };
    // La fonction ci-dessus est appelée par tout automatisme ORM affiché en popup (formule du milieu) lorsque l'utilisateur clique sur le bouton de validation
    // le code des automatismes ORM est câblé pour appeler top.OrmConfirmResponse en lui passant les paramètres ci-dessous.
    // On la câble donc ainsi pour des raisons de compatibilité ascendante
    /**

     * @param {boolean} validResult true si l'utilisateur a validé la fenêtre, false sinon
     * @param {string} urlResult URL à appeler pour effectuer la validation (transmise par l'automatisme ORM appelé)
     */
    top.OrmConfirmResponse = function (validResult, urlResult) {
        if (!isIris(nGlobalActiveTab)) {
            if (engineOrmWait == null)
                return;

            try {
                engineOrmWait.Engine.validOrmMiddleConfirm(engineOrmWait.Modal, validResult, urlResult);
            } finally {
                engineOrmWait = null;
            }
        }
        else {
            if (top.irisBlackOrmUpdateContext == null)
                return;

            try {
                validOrmMiddleConfirm(top.irisBlackOrmUpdateContext, validResult, urlResult);
            }
            finally {
                // on ferme la modale si non fait
                if (top.irisBlackOrmUpdateContext.ctx.ormConfirmModal)
                    top.irisBlackOrmUpdateContext.ctx.ormConfirmModal.hide();
                top.irisBlackOrmUpdateContext = null;
            }
        }
    };

    ctx.ormConfirmModal.ErrorCallBack = function () {
        try {
            cancelMiddleConfirm(ctx, responseJson, oldValue);
        }
        finally {
            top.irisBlackOrmUpdateContext = null;
            ctx.ormConfirmModal.hide();
        }
    };
    ctx.ormConfirmModal.addParam("OrmId", oConfirmBoxInfoAttributes.id, "post");
    ctx.ormConfirmModal.addParam("OrmUpdates", oConfirmBoxInfoElements.ormupdates, "post");
    ctx.ormConfirmModal.hideCloseButton = true;
    ctx.ormConfirmModal.show();

    // Pour test :)
    //ctx.ormConfirmModal.addButton(top._res_29, function () { validOrmMiddleConfirm(ctx, false, null); }, "button-gray"); // Annuler
    //ctx.ormConfirmModal.addButton(top._res_28, function () { validOrmMiddleConfirm(ctx, true, 'test'); }, "button-green"); // Valider
};

/**
 * fonction exécutée au clic sur les boutons de la fenêtre popup de formule du milieu ORM
 * @param {object} ormCtx contexte complet d'exécution de updateMethod au moment de l'appel ORM, soit un objet contenant ctx + newValue + oldValue
 * @param {boolean} validResult true si l'utilisateur a cliqué sur OK, false sinon
 * @param {string} urlResult URL de confirmation ORM à envoyer à Engine pour la mise à jour
 */
export function validOrmMiddleConfirm(ormCtx, validResult, urlResult) {
    try {
        if (validResult) {
            // Remarque : la casse des paramètres ci-dessous doit correspondre à celle de l'objet sérialisé/désérialisé côté .NET (donc UpdateFieldsModel) et non à celle
            // attendue par les paramètres d'Engine (qui seront, eux, transmis avec la casse adaptée par UpdateFieldsModel). Si la casse ne correspond pas, CreateUpdateController
            // recevra null en tant que valeur, et renverra donc une InternalServerError (500)
            // Même chose pour OrmResponseObj qui attend un String et non un tableau comme reçu via OnOrmResponse (comprenant généralement un seul paramètre à l'index 0)
            var convertedUrlResult = urlResult;
            if (convertedUrlResult && typeof (convertedUrlResult) != "string") {
                if (typeof (convertedUrlResult) == "object" && convertedUrlResult.length > 0)
                    convertedUrlResult = convertedUrlResult[0];
            }

            var updateData = {
                'OrmId': ormCtx.ConfirmBoxInfoAttributes.id, // ResultBoxInfo.XmlAttributes
                'OrmUpdates': ormCtx.ConfirmBoxInfoElements.ormupdates, // ResultBoxInfo.XmlElements
                'OrmResponseObj': convertedUrlResult
            };
            updateMethod(ormCtx.ctx, ormCtx.newValue, ormCtx.oldValue, updateData, ormCtx.inputData);
        } else {
            cancelMiddleConfirm(ormCtx.ctx, ormCtx.responseJson, ormCtx.oldValue);
        }
    }
    catch (err) {
        cancelMiddleConfirm(ormCtx.ctx, err, ormCtx.oldValue);
    }
    finally {
        // On retire la modale
        ormCtx.ctx.ormConfirmModal.hide();
    }
};

export function cancelMiddleConfirm(ctx, responseJsonOrErrorMsg, oldValue) {
    cancelUpdate(ctx, responseJsonOrErrorMsg, oldValue);
};

/**
 * fonction pour le retour du focus des composants (?)
 * @param {string} champ le type de champs à mettre à jour.
 * @param {object} obj objet contenant les éléments de la classe à appeler..
 */
export function focusInput(champ, obj) {

    if (obj.dataInput.ReadOnly)
        return false;

    let sClasse = "";

    if (obj.props == PropType.Assistant) {
        sClasse = 'class_liste_rubrique_' + champ + '_assistant_' + obj.propAssistantNbIndex + '_' + obj.dataInput.DescId;
    } else if (obj.props == PropType.Detail) {
        sClasse = 'class_liste_rubrique_' + champ + '_detail_' + obj.dataInput.DescId;
    } else if (obj.props == PropType.Liste) {
        sClasse = 'class_liste_rubrique_' + champ + '_' + obj.propSignet.DescId + '_' + obj.propIndexRow + '_' + obj.dataInput.DescId;
    } else if (obj.props == PropType.Head) {
        sClasse = 'class_liste_rubrique_' + champ + '_header_' + obj.dataInput.DescId;
    }

    Vue.nextTick(function () {
        let input = document.getElementsByClassName(sClasse);
        if (input.length > 0)
            input[0].focus();
    });
}

/**
 * Va executer toutes les vérifications idoines, séquentiellement.
 * @param {any} pattern la pattern de vérification.
 * @param {any} event l'évent qui est provoqué par l'utilisateur.
 * @param {any} oldValue l'ancienne valeur du composant.
 * @param {any} ctx le contexte appelant.
 * @param {object} inputData l'ensemble des données de la fiche.
 * @param {object} additionalUpdateData paramètres de mise à jour additionnels à passer à la fonction updateMethod, qui les fusionnera avec ceux qu'elle construit à la base
 */
export async function verifComponent(pattern, event, oldValue, ctx, inputData, additionalUpdateData, origin) {
    let TimeOutModif = 250;
    let arTypeURL = [FieldType.SocialNetwork, FieldType.HyperLink]; // #85 889
    let evtTargetVal;

    /*
    ATTENTION, la série de if/else ci-dessous pour affecter evtTargetVal est CRITIQUE et doit être modifiée avec précaution, notamment lorsqu'il s'agit de corriger une problématique liée aux rubriques Mémo.
    Elle a été modifiée pour TOUS les cas suivants. Il faudra donc les retester à CHAQUE modification :
    #85 889 - ELAIZ - Rubriques Réseau Social : encodage URL visible sur la valeur - décode la valeur si elle contient des caractères spéciaux (URL)
    #90 164 - ELAIZ - Texte saisi disparu à l'entrée dans un champ Mémo - dans le cas où l'on ouvre le mémo en pleine page la cible n'est pas la même
    #94 108 - MABBE - Des ??? apparaîssent à la validation du contenu des champs Mémo HTML - Si on doit cibler explicitement un champ Mémo HTML, il faut utiliser l'API de CKEditor afin de récupérer le contenu sans caractères parasites type ZWS - cf. https://dev.ckeditor.com/ticket/10031
    #94 921 - WXIE - Saisie dans le signet Notes non sauvegardée - Dû à un mauvais ciblage résultant des correctifs précéents - Désormais rattaché au cas général
    #96 179 et #96 219 - MABBE - Problème d'encodage des rubriques Mémo à la sortie du curseur
    #96 278 - MABBE - Problème d'insertion d'images
    */

    // Cas d'un champ Mémo CKEditor - #90 164, #94 108, #94 921, #96 179/96 219, #96 278
    if (inputData.Format == FieldType.Memo && inputData.IsHtml && ctx.CKEditorInstance)
        evtTargetVal = ctx.CKEditorInstance.getData();
    // Cas d'un champ Mémo géré par EudoFront/Quill
    else if (origin == "eudofront")
        evtTargetVal = ctx.bValue ? "1" : "0";
    // Cas d'un champ de type Réseau social ou Lien - #85 889
    else if (arTypeURL.includes(inputData.Format))
        evtTargetVal = decodeURIComponent(event.target?.value);
    // Autres cas différents de ci-dessus
    else
        evtTargetVal = event.target?.value;

    if (event?.target?.parentElement?.tagName == "LI" || (evtTargetVal == "" && ctx.dataInput.Required == true)) {
        verifCharacter(event, ctx, oldValue);
        return;
    }

    if (evtTargetVal == oldValue && evtTargetVal != "") {
        if (pattern) {
            verifRegexOnBlank(pattern, event, ctx, oldValue, inputData);
            if (ctx.bRegExeSuccess)
                setTimeout(function () { ctx.modif = false; }, TimeOutModif);
            return;
        }

        ctx.bDisplayPopup = false;
        setTimeout(function () { ctx.modif = false; }, TimeOutModif);
        return;
    }
    else if (evtTargetVal == oldValue && evtTargetVal === "") {
        ctx.bDisplayPopup = false;
        setTimeout(function () { ctx.modif = false; }, TimeOutModif);
        return;
    }

    if (evtTargetVal === "")
        verifCharacter(event, ctx, oldValue, inputData, additionalUpdateData);
    else if (pattern)
        verifRegex(pattern, event, ctx, oldValue, inputData);
    else {
        // ElAIZ - Request 90 164 - Promesse afin d'avoir le retour d'updateMethod dans le cas où verifComponent est lui-même une promesse
        await updateMethod(ctx, evtTargetVal, oldValue, additionalUpdateData, inputData);
        return;
    }

}

/**
 * Ajoute la bordure/le flag vert indiquant la réussite d'une MAJ de composant en base
 * @param {any} oParent le ou les élements sur lequels doivent être appliquée la classe. Si non précisé, on visera tous les éléments correspondant au DescId du champ en cours
 * @param {any} oCallbackFct Si précisée, cette fonction sera appelée au bout de nCallbackFctTimeout après l'ajout ou le retrait des classes sur chaque élément
 * @param {int} nCallbackFctTimeout Si précisée, la fonction oCallbackFct sera appelée au bout de ce nombre de millisecondes après l'ajout ou le retrait des classes sur chaque élément  
 */
export function AddBorderSuccess(oParent, oCallbackFct, nCallbackFctTimeout) {
    // Pour cette fonction, on ajoute un callback par défaut pour supprimer la bordure au bout de X millisecondes
    if (!oCallbackFct)
        oCallbackFct = function (targetElt) {
            RemoveBorderSuccess(targetElt);
        };
    return AddOrRemoveBorderUpdateClassesOnComponent(["border-success"], oParent, true, oCallbackFct, nCallbackFctTimeout, this);
}

/**
 * Ajoute la bordure/le flag rouge indiquant l'échec d'une MAJ de composant en base
 * @param {any} oParent le ou les élements sur lequels doivent être appliquée la classe. Si non précisé, on visera tous les éléments correspondant au DescId du champ en cours
 * @param {any} oCallbackFct Si précisée, cette fonction sera appelée au bout de nCallbackFctTimeout après l'ajout ou le retrait des classes sur chaque élément
 * @param {int} nCallbackFctTimeout Si précisée, la fonction oCallbackFct sera appelée au bout de ce nombre de millisecondes après l'ajout ou le retrait des classes sur chaque élément  
 */
export function AddBorderError(oParent, oCallbackFct, nCallbackFctTimeout) {
    // Pour cette fonction, on ajoute un callback par défaut pour supprimer la bordure au bout de X millisecondes
    if (!oCallbackFct)
        oCallbackFct = function (targetElt) {
            RemoveBorderError(targetElt);
        };
    return AddOrRemoveBorderUpdateClassesOnComponent(["border-error"], oParent, true, oCallbackFct, nCallbackFctTimeout, this);
}

/**
 * Efface les bordures/flags CSS de succès ajoutés pour matérialiser visuellement la MAJ (ou non) d'un composant en base
 * @param {any} oParent le ou les élements sur lequels doivent être appliquée la classe. Si non précisé, on visera tous les éléments correspondant au DescId du champ en cours
 * @param {any} oCallbackFct Si précisée, cette fonction sera appelée au bout de nCallbackFctTimeout après l'ajout ou le retrait des classes sur chaque élément
 * @param {int} nCallbackFctTimeout Si précisée, la fonction oCallbackFct sera appelée au bout de ce nombre de millisecondes après l'ajout ou le retrait des classes sur chaque élément 
 */
export function RemoveBorderSuccess(oParent, oCallbackFct, nCallbackFctTimeout) {
    return AddOrRemoveBorderUpdateClassesOnComponent(["border-success"], oParent, false, oCallbackFct, nCallbackFctTimeout, this);
}

/**
 * Efface les bordures/flags CSS d'erreur ajoutés pour matérialiser visuellement la MAJ (ou non) d'un composant en base
 * @param {any} oParent le ou les élements sur lequels doivent être appliquée la classe. Si non précisé, on visera tous les éléments correspondant au DescId du champ en cours
 * @param {any} oCallbackFct Si précisée, cette fonction sera appelée au bout de nCallbackFctTimeout après l'ajout ou le retrait des classes sur chaque élément
 * @param {int} nCallbackFctTimeout Si précisée, la fonction oCallbackFct sera appelée au bout de ce nombre de millisecondes après l'ajout ou le retrait des classes sur chaque élément 
 */
export function RemoveBorderError(oParent, oCallbackFct, nCallbackFctTimeout) {
    return AddOrRemoveBorderUpdateClassesOnComponent(["border-error"], oParent, false, oCallbackFct, nCallbackFctTimeout, this);
}

/**
 * Efface les bordures/flags CSS de succès et d'erreur ajoutés pour matérialiser visuellement la MAJ (ou non) d'un composant en base
 * @param {any} oParent le ou les élements sur lequels doivent être appliquée la classe. Si non précisé, on visera tous les éléments correspondant au DescId du champ en cours
 * @param {any} oCallbackFct Si précisée, cette fonction sera appelée au bout de nCallbackFctTimeout après l'ajout ou le retrait des classes sur chaque élément
 * @param {int} nCallbackFctTimeout Si précisée, la fonction oCallbackFct sera appelée au bout de ce nombre de millisecondes après l'ajout ou le retrait des classes sur chaque élément 
 */
export function RemoveBorderSuccessError(oParent, oCallbackFct, nCallbackFctTimeout) {
    return AddOrRemoveBorderUpdateClassesOnComponent(["border-error", "border-success"], oParent, false, oCallbackFct, nCallbackFctTimeout, this);
}

/**
 * Ajoute ou enlève la ou les bordures/flags CSS indiqués pour matérialiser visuellement la MAJ (ou non) d'un ou plusieurs composants en base
 * @param {array} aTargetClasses les classes CSS à cibler, sous forme de tableau
 * @param {any} oParent le ou les élements sur lequels doivent être appliquée la classe. Si non précisé, on visera tous les éléments correspondant au DescId du champ en cours
 * @param {boolean} bAdd true pour ajouter les classes, false pour les enlever
 * @param {any} oCallbackFct Si précisée, cette fonction sera appelée au bout de nCallbackFctTimeout après l'ajout ou le retrait des classes sur chaque élément
 * @param {int} nCallbackFctTimeout Si précisée, la fonction oCallbackFct sera appelée au bout de ce nombre de millisecondes après l'ajout ou le retrait des classes sur chaque élément 
 * @param {any} ctx L'objet this, si besoin d'être passé. Permet de passer le contexte d'un objet VueJS de la fonction parente type AddBorder* ou RemoveBorder*, si appelée via un this.AddBorder* ou this.RemoveBorder*
 */
export function AddOrRemoveBorderUpdateClassesOnComponent(aTargetClasses, oParent, bAdd, oCallbackFct, nCallbackFctTimeout, ctx) {
    // Si aucune classe CSS n'est précisée, dehors
    if (!aTargetClasses || aTargetClasses.length == 0)
        return; 
    // Si le contexte n'est pas passée, on considère qu'il s'agit de this, même undefined (sera vérifié plus bas)
    if (!ctx)
        ctx = this;
    // Récupération d'une liste de cibles par défaut si on a un dataInput : on vise tous les champs ayant le DescId cible sur TOUTE la page (zone Résumé, zone Assistant, Détails...)
    if (!oParent && ctx && ctx.dataInput)
        oParent = ctx.$root.$el.querySelectorAll('div[divdescid="' + ctx.dataInput.DescId + '"]');
    // Convsersion du résultat d'un querySelectorAll
    if (oParent instanceof NodeList)
        oParent = Array.from(oParent);
    // Conversion du passage d'un seul objet en tant que oParent pour avoir une Array énumérable
    if (oParent && !Array.isArray(oParent))
        oParent = [oParent];

    if (oParent && oParent.length > 0) {
        oParent.forEach(targetPrt => {
            aTargetClasses.forEach(elem => {
                if (typeof targetPrt.classList != "undefined") {
                    // Ajout/retrait de CSS
                    if (bAdd && !targetPrt.classList.contains(elem))
                        targetPrt.classList.add(elem);
                    else if (!bAdd && targetPrt.classList.contains(elem))
                        targetPrt.classList.remove(elem);
                    // Appel de la fonction de callback sur chaque élément
                    if (oCallbackFct) {
                        if (!nCallbackFctTimeout)
                            nCallbackFctTimeout = 1500;
                        setTimeout(oCallbackFct, nCallbackFctTimeout, targetPrt);
                    }
                }
            });
        });
    }
}

/**
 * Paramètre le message d'erreur relatif au composant en cours
 * @param {any} message Message d'erreur. Si vide, celui-ci est remis à zéro */
export function SetErrorMessage(dataInput, message) {
    if (!dataInput)
        return null;

    dataInput.ErrorMessage = message;
}

/**
 * Permet de vérifier, en sortant du champs que la valeur est bien correcte.
 * Et éventuellement d'enregistrer l'adresse.
 * La regex vient de Monsieur W3C.
 * @param {string} regex l'expression régulière de vérification.
 * @param {object} elem un objet représetant l'objet courant modifié.
 * @param {object} that context de l'appelant.
 * @param {object} oldValue l'ancienne valeur saisie sur le champ, pour restauration en cas d'erreur
 */
export function verifRegex(regex, elem, that, oldValue, inputData) {
    verifRegexOnBlank(regex, elem, that, inputData);

    //ELAIZ - décode la valeur si elle contient des caractères spéciaux ( url )
    let evtTargetVal = decodeURIComponent(elem.target?.value)

    if (!that.bRegExeSuccess)
        return;

    try {
        updateMethod(that, evtTargetVal, oldValue, undefined, inputData);
    } catch (e) {
        papa.classList.add("border-error");
        that.bDisplayPopup = true;
    }
}

/**
 * Permet de valider une adresse mail, juste ça.
 * @param {string} regex l'expression régulière de vérification.
 * @param {any} elem  un objet représetant l'objet courant.
 * @param {object} that context de l'appelant.
 * @param {object} oldValue l'ancienne valeur saisie sur le champ, pour restauration en cas d'erreur
 */
export function verifRegexOnBlank(regex, elem, that, oldValue) {

    if (elem.target.value == "") {
        that.bDisplayPopup = false;
        return;
    }

    var papa = getContainerElement(that);
    RemoveBorderSuccessError(papa);
    papa.classList.remove("emptyBase");
    SetErrorMessage(that.dataInput, '');

    that.bRegExeSuccess = regex.test(elem.target.value);
    // that.dataInput.Value = elem.target.value;

    if (!that.bRegExeSuccess) {
        that.bDisplayPopup = true;
        papa.classList.add("border-error");
        SetErrorMessage(that.dataInput, that.messageError); // #80 517 - ajout du message d'erreur par défaut du composant (utilisé en cas d'incohérence de saisie) sur dataInput pour affichage en infobulle
        if (that.propListe)
            papa.classList.add("emptyBase"); // remplace l'icône "Retour" par l'icône par défaut pour le mode Liste
    }
}

/**
 * Empêcher de vider les rubriques obligatoires (Tache 1907 - US 1142)
 * @param {any} elem l'élement qui envoie la demande.
 * @param {any} that le contexte d'appel (this)
 * @param {object} oldValue l'ancienne valeur saisie sur le champ, pour restauration en cas d'erreur
 */
export function verifCharacter(elem, that, oldValue, inputData, additionalUpdateData) {

    let val;

    //ELAIZ - décode la valeur si elle contient des caractères spéciaux ( url )
    let evtTargetVal = decodeURIComponent(elem.target?.value)

    if (elem.target)
        val = evtTargetVal;
    else if (elem.target && that.dataInput.Format == 2)
        val = elem.target.previousSibling.textContent;
    else
        val = elem.Value;

    var papa = getContainerElement(that);

    if (!that.dataInput.Required && that.dataInput.Format != 2)
        updateMethod(that, val, oldValue, additionalUpdateData, inputData);
    else if (!that.dataInput.Required && that.dataInput.Format == 2) {
        updateMethod(that, that.selectedValues.join(';'), undefined, undefined, that.dataInput);
    }


    if (papa != null && that.dataInput.Required) {
        if (val != "" && that.dataInput.Format != 2) {
            //if (elem != "" && elem.target.value) {
            that.bEmptyDisplayPopup = false;
            papa.classList.remove("border-error");
            papa.classList.remove("emptyBase");
            SetErrorMessage(that.dataInput, '');
            //that.dataInput.Value = val;
            // that.dataInput.Value = elem;
            updateMethod(that, val, oldValue, inputData);
        } else if (elem.target && elem.target.parentElement.tagName == "LI" && that.dataInput.Format == 2) {
            that.bEmptyDisplayPopup = true;
            papa.classList.add("border-error");
            //that.$parent.$children.filter(x => x.dataInput.DescId == that.dataInput.DescId)
            //    .map(x => x.$el.parentElement.classList.add("border-error"));
            //elem.target.previousSibling.textContent = that.dataInput.DisplayValue;
            // US #1854 - Demande #80 517 et Tâche #2257 - On matérialise les champs avec le même DescID sur la même page en erreur, mais uniquement en mode Fiche
            if (!that.propListe)
                [].slice.call(that.$parent.$el.querySelectorAll(`[divdescid="${that.dataInput.DescId}"]`)).map(x => x.classList.add("border-error"));
            // Sinon, en mode Liste, on remonte le message d'erreur pour affichage au survol (via showTooltip)
            else {
                SetErrorMessage(that.dataInput, that.getRes(2471)); // Cette rubrique est obligatoire //that.messageError
                papa.classList.add("emptyBase"); // remplace l'icône "Retour" par l'icône par défaut pour le mode Liste
            }
            that.bEmptyDisplayPopup = true;
        } else {
            that.bEmptyDisplayPopup = true;
            papa.classList.add("border-error");
            //that.$parent.$children.filter(x => x.dataInput.DescId == that.dataInput.DescId)
            //    .map(x => x.$el.parentElement.classList.add("border-error"));

            // US #1854 - Demande #80 517 et Tâche #2257 - On matérialise les champs avec le même DescID sur la même page en erreur, mais uniquement en mode Fiche
            if (!that.propListe)
                [].slice.call(that.$parent.$el.querySelectorAll(`[divdescid="${that.dataInput.DescId}"]`)).map(x => x.classList.add("border-error"));
            // Sinon, en mode Liste, on remonte le message d'erreur pour affichage au survol (via showTooltip)
            else {
                SetErrorMessage(that.dataInput, that.getRes(2471)); // Cette rubrique est obligatoire //that.messageError
                papa.classList.add("emptyBase"); // remplace l'icône "Retour" par l'icône par défaut pour le mode Liste
            }

            // elem.target ? elem.target.value = that.dataInput.Value : elem.Value = that.dataInput.Value;
        }
        return;
    }

}

/**
 * Empêcher de vider les rubriques obligatoires relation (Tache 1907 - US 1142)
 * @param {any} that le contexte d'appel (this)
 */

export function verifRelation(that) {
    var papa = getContainerElement(that);

    let val;
    if (that.selectedValues)
        val = that.selectedValues[0];
    else
        val = that.dataInput.Value

    if (papa != null && that.dataInput.Required) {
        if (val != '') {
            that.bEmptyDisplayPopup = false;
            papa.classList.remove("border-error");
        } else {
            that.bEmptyDisplayPopup = true;
            papa.classList.add("border-error");
            // that.dataInput.Value = that.oldValue;
            that.dataInput.DisplayValue = that.oldValue;
            val = that.oldValue;
        }
        that.modified = true;
    }

    return;
}


/**
 * A partir des options tranmises, on gènère un catalog
 * @param {any} sourceAction le type de cataloguqe
 * @param {any} p_DescType pour les catalogues en desc
 * @param {any} p_EnumType pour les catalogues en enum
 * @param {any} p_bMulti si c'est de type simple ou multicritere
 * @param {any} p_btreeView si c'est un cataloguqe arborescent
 * @param {any} p_defValue la valeur par défaut
 * @param {any} p_sourceFldId le champs source
 * @param {any} p_targetFldId le champs cible.
 * @param {any} p_catDescId le descid
 * @param {any} p_catPopupType le type de popup
 * @param {any} p_catBoundDescId le descid de la liaison
 * @param {any} p_catBoundPopup la popup liée
 * @param {any} p_catParentValue la valeur parent
 * @param {any} p_CatTitle le titre de la popup
 * @param {any} p_JsVarName le nom de la modale
 * @param {any} p_bMailTemplate le template du mail (non, je n'ai pas compris non plus... G.L)
 * @param {any} p_partOfAfterValidate ce qu'on fait en validant
 * @param {any} p_partOfAfterCancel ce qu'on fait en annulant
 * @param {any} p_fromFilter depuis un filtre
 * @param {any} p_fromTreat depuis ...
 * @param {any} p_fromAdmin depuis l'admin
 */
export function showCatalogGeneric(
    sourceAction,
    p_DescType,
    p_EnumType,
    p_bMulti,
    p_btreeView,
    p_defValue,
    p_sourceFldId,
    p_targetFldId,
    p_catDescId,
    p_catPopupType,
    p_catBoundDescId,
    p_catBoundPopup,
    p_catParentValue,
    p_CatTitle,
    p_JsVarName,
    p_bMailTemplate,
    p_partOfAfterValidate,
    p_partOfAfterCancel,
    p_from
    //p_fromFilter,
    //p_fromTreat,
    //p_fromAdmin
) {

    top.setWait(true);

    //var bFromFilter = false;
    //if (typeof (p_fromFilter) != "undefined") {
    //    // HLA - il faut egalement tester le 'true' car la string "true" != de la string "1" - #61219
    //    bFromFilter = p_fromFilter === true || p_fromFilter + "" == "1";
    //}

    //var bFromTreat = false;
    //if (typeof (p_fromTreat) != "undefined") {
    //    // HLA - il faut egalement tester le 'true' car la string "true" != de la string "1" - #61219
    //    bFromTreat = p_fromTreat === true || p_fromTreat + "" == "1";
    //}

    //var bFromAdmin = false;
    //if (typeof (p_fromAdmin) != "undefined") {
    //    // HLA - il faut egalement tester le 'true' car la string "true" != de la string "1" - #61219
    //    bFromAdmin = p_fromAdmin === true || p_fromAdmin + "" == "1";
    //}

    if (!p_from)
        p_from = LOADCATFROM.UNDEFINED;

    var catWidth = (p_bMulti || p_btreeView) ? 850 : 800;
    var catHeight = 550;

    if (p_bMulti)
        catHeight = 530;

    if (p_btreeView)
        catHeight = 614;

    // Freecode CRU 14122015 : Ajout du libellé de catalogue dans le titre de la modale
    var modalTitle = top._res_225 + ((p_CatTitle) ? ' : ' + decode(p_CatTitle) : "");

    this.catalogDialog = new eModalDialog(modalTitle, 0, "eCatalogDialog.aspx", catWidth, catHeight);
    this.catalogDialog.ErrorCallBack = () => top.setWait(false);
    this.catalogDialog.onIframeLoadComplete = () => {
        if (!p_btreeView) {
            new Promise((resolve, reject) => {
                try {
                    resolve(this.adjustColsWidth());
                } catch (e) {
                    reject(e);
                }
            });
        }
        top.setWait(false);
    };

    this.catalogDialog.addParam("CatDescId", p_catDescId, "post");

    if (p_catParentValue != null)
        this.catalogDialog.addParam("CatParentValue", p_catParentValue, "post");

    this.catalogDialog.addParam("CatBoundPopup", p_catBoundPopup, "post");
    this.catalogDialog.addParam("CatBoundDescId", p_catBoundDescId, "post");

    this.catalogDialog.addParam("CatAction", "ShowDialog", "post");
    this.catalogDialog.addParam("CatPopupType", p_catPopupType, "post");
    this.catalogDialog.addParam("CatMultiple", p_bMulti ? "1" : "0", "post");
    this.catalogDialog.addParam("CatSearch", "", "post");
    this.catalogDialog.addParam("CatInitialValues", p_defValue, "post");
    this.catalogDialog.addParam("CatEditorJsVarName", p_JsVarName, "post");
    this.catalogDialog.addParam("CatTitle", p_CatTitle, "post");
    this.catalogDialog.addParam("treeview", p_btreeView ? "true" : "false", "post");
    this.catalogDialog.addParam("FrameId", this.catalogDialog.iframeId, "post");

    this.catalogDialog.addParam("From", p_from, "post");
    //this.catalogDialog.addParam("FromFilter", bFromFilter ? "1" : "0", "post");
    //this.catalogDialog.addParam("FromTreat", bFromTreat ? "1" : "0", "post");
    //this.catalogDialog.addParam("FromAdmin", bFromAdmin ? "1" : "0", "post");

    if (p_bMailTemplate)
        this.catalogDialog.addParam("MailTemplate", p_bMailTemplate ? "true" : "false", "post");

    if (sourceAction == "LNKCATDESC") {
        this.catalogDialog.addParam("CatSource", "1", "post");
        this.catalogDialog.addParam("DescType", p_DescType, "post");

    }
    else if (sourceAction == "LNKCATENUM") {
        this.catalogDialog.addParam("CatSource", "2", "post");
        this.catalogDialog.addParam("EnumType", p_EnumType, "post");
    }

    top.eTabCatModalObject.Add(this.catalogDialog.iframeId, this.catalogDialog);

    this.catalogDialog.show();

    this.catalogDialog.addButton(top._res_29, p_partOfAfterCancel, "button-gray", p_targetFldId, "cancel"); // Annuler
    this.catalogDialog.addButton(top._res_28, p_partOfAfterValidate, "button-green", p_sourceFldId + '|' + p_targetFldId, "ok"); // Valider

    //return this.catalogDialog;
}


/**
 * Enl�ve une valeur de la liste.
 * @param {any} event pour arr�ter l'evenement parent.
 * @param {any} Value la valeur � retirer.
 */
export function removeOneValue(event, Value) {

    if (this.dataInput.IsTree)
        return false;

    event.stopPropagation();

    this.selectedValues = / *; */g[Symbol.split](this.dataInput.Value);
    var oldVal = this.selectedValues;
    var array = this.selectedValues.filter(x => x !== Value.id);


    //if (array.length < 1) {
    //    this.selectedValues = this.selectedValues.filter(x => x != this.selectedValues);
    //    this.selectedLabels = this.selectedLabels.filter(x => x != this.selectedLabels);
    //    verifComponent(undefined, event, this.dataInput.Value, this.that, this.dataInput);
    //    return;
    //}

    //this.selectedValues = this.selectedValues.filter(x => x != Value.id);
    //this.selectedLabels = this.dataInput.DisplayValue.split(";").filter(x => x != Value.value);

    try {
        this.updateMethod(this, array.join(';'), oldVal, undefined, this.dataInput);
    } catch (e) {
        EventBus.$emit('globalModal', {
            typeModal: "alert", color: "danger", type: "zoom",
            close: true, maximize: false, id: 'alert-modal', title: this.getRes(6576), msgData: e, width: 600,
            btns: [{ lib: this.getRes(30), color: 'default', type: 'left' }], datas: this.getRes(7050)
        });

        return;
    }
}

/**
 *
 * @param {any} val bool qui permet de savoir si on doit ou non afficher le tooltip
 * @param {any} elem cible qui permet de positionner le tooltip
 * @param {any} icon savoir si nous sommes en édition ou ouverture (lien)
 * @param {any} readonly savoir si l'élément est en lecture seule ou non
 * @param {any} data dataInput afin de vérifier si l'élément à loption SMS ou son format
 * @param {any} dateFormat dataInput afin de vérifier si l'élément à loption SMS ou son format
 */
export async function showTooltip(val, elem, icon, readonly, data, dateFormat, emailVerif, animation) {

    //n'affiche pas le tooltip si nous sommes sur les relations et que la minifiche est active ( double infobulle sinon)
    /*ELAIZ- demande 80028 - pas de tooltip si on passe la souris sur le champs Raison social pour une
    société*/
    if ((data) &&
        ((data?.IsMiniFileEnabled && (data?.Format == FieldType.AliasRelation || data?.Format == FieldType.Alias || data?.Format == FieldType.Relation))
            || (data?.Format == FieldType.Relation && data?.Value != "" && data?.Value != "0")
            || (data?.Format == FieldType.AliasRelation && data?.DescId == 301 && data?.ReadOnly == true)
            || (this.propListe)
        )
        && elem != 'info'
    ) {
        return false;
    }

    let newData = "";
    let oRGPD = {};
    let position;
    let animationDelay;
    let animationDuration;
    let msgTooltip = "";
    let msgError = "";
    let typeModal = "tooltip"; // permet de savoir si on est sur une modale de type tooltip ou pas ( utilisé aussi dans le cas des cats multiples)
    let parentElement = this.$parent.$options.name.toLowerCase();
    let editMode = !readonly && !icon;
    let readOnly = readonly && !icon;
    let sms = icon && data?.DisplaySmsBtn;
    let currentElm = (Object.keys(FieldType)
        .find(key => FieldType[key] == data?.Format) || "")
        .toLowerCase();

    if (currentElm == "phone" && data?.DisplaySmsBtn)
        currentElm = "sms";

    if (data) {
        let displayValue = data?.DisplayValue || data?.Value || "";

        if (data?.Format == FieldType.Date && dateFormat)
            displayValue = dateFormat;

        if ([FieldType.Relation, FieldType.AliasRelation].includes(data?.Format) && data?.Value == "0")
            displayValue = "";

        newData = (displayValue && displayValue.includes(";"))
            ? displayValue.split(';').join(',')
            : displayValue;

        oRGPD = data?.RGPD;
    }
    
    // texte spécial pour la barre de racourcis
    if (elem == "shortcut") {
        if (data?.Format == FieldType.Phone) {
            newData = this.getRes(8893);
        } else if (data?.Format == FieldType.HyperLink) {
            newData = this.getRes(8894);
        } else if (data?.Format == FieldType.MailAddress) {
            newData = this.getRes(8807);
        } else if (data?.Format == FieldType.Geolocation) {
            
        } else if (data?.Format == FieldType.User) {
            
        } else if (data?.Format == FieldType.SocialNetwork) {
            newData = this.getRes(8895);
        } 
    }

    if (data?.ValueToolTipText)
        msgTooltip = data.ValueToolTipText;
    else if (data?.ToolTipText)
        msgTooltip = data?.ToolTipText;

    //Si l'on vient d'une valeur de catalogue multiple, on met ce type afin de mieux le cibler dans tooltipModale
    if (this.$options.name == "eCatalogFileMultiple")
        typeModal = 'catTooltip'

    if (data?.ErrorMessage?.ErrorMessages?.length > 0)
        msgError = data.ErrorMessage.ErrorMessages.map(m => m.Description).join("; ");
    /*
    let dvPosition = (elem == "info")
        ? this.$refs['label_info' + data?.DescId].find(lb => lb.offsetParent)
        // si on a une infobulle dans une valeur de cat multiple
        : this.$refs[elem] && this.dataInput?.ValueToolTipText != "" && this.$options.name == "eCatalogFileMultiple" ?
            this.$refs[elem][0]
            : (!this.$refs[elem]?.length)
                ? this.$refs[elem]
                : this.$refs['catalog'];
    */
    let dvPosition;
    if (elem == "info") {
        dvPosition = this.$refs['label_info' + data?.DescId].find(lb => lb.offsetParent);
    } else if (elem == "shortcut") {
        dvPosition = this.$refs['shortcut-' + data?.DescId].find(lb => lb.offsetParent);
    } else {
        if (this.$refs[elem] && this.dataInput?.ValueToolTipText != "" && this.$options.name == "eCatalogFileMultiple") {
            dvPosition = this.$refs[elem][0];
        } else {
            if (!this.$refs[elem]?.length) {
                dvPosition = this.$refs[elem];
            } else {
                dvPosition = this.$refs['catalog'];
            }
        }
    }

    if (emailVerif) {
        dvPosition = this.$refs[elem];
        animationDelay = '0ms';
        animationDuration = '100ms';
    }

    position = dvPosition
        ? dvPosition.getBoundingClientRect()
        : { "x": 0, "y": 0, "width": 0, "height": 0, "top": 0, "right": 0, "bottom": 0, "left": 0 };

    /** Si la tooltip est complètement vide.
     * Si c'est un header (ou que le message est vide)
     * Si les valeurs sont vides
     * Si ce n'est un élément à astuce
     *
     * Bref, avec tout ca on a une belle popup vide, donc on sort.
     * */
    let { tabAstuces, tabParentForbidden } = await import(AddUrlTimeStampJS("../methods/eComponentConst.js"));

    if ((elem != emailVerif?.name) && (!oRGPD || Object.keys(oRGPD).length < 1) &&
        ((tabParentForbidden.includes(parentElement) || (!msgTooltip || msgTooltip == ""))
            && (!msgError || msgError == "")
            && (!newData || newData == "")
            && (!tabAstuces[currentElm] || readonly)))
        return false;

    let tooltipObj = {
        visible: val,
        elem: elem,
        icon: icon,
        readonly: readonly,
        newData: newData,
        msgTooltip: msgTooltip,
        msgError: msgError,
        typeModal: typeModal,
        position: position,
        info: (elem == "info"),
        rgpd: oRGPD,
        parentElement: this.$parent.$options.name.toLowerCase(),
        currentElement: currentElm,
        emailVerif: emailVerif,
        animationDelay: animation?.animationDelay,
        animationDuration: animation?.animationDuration
    }

    this.setTooltipObj(tooltipObj);
}
/**
 * ELAIZ - reprise de la méthode showTooltip mais avec les params dans un obj pour avoir quelquechose de plus clair
 * à l'appel - Pour le moment, cette méthode n'est appellée que dans le cadre du menu mru
 * @param {any} obj.visible bool qui permet de savoir si on doit ou non afficher le tooltip
 * @param {any} obj.elem cible qui permet de positionner le tooltip
 * @param {any} obj.icon savoir si nous sommes en édition ou ouverture (lien)
 * @param {any} obj.readonly savoir si l'élément est en lecture seule ou non
 * @param {any} obj.data dataInput afin de vérifier si l'élément à loption SMS ou son format
 * @param {any} obj.dateFormat dataInput afin de vérifier si l'élément à loption SMS ou son format
 * @param {any} obj.dataMru Permet de vérifier que nous sommes en train de survoler 1 valeur mru (liste)
 */
export async function showTooltipObj(obj) {

    let objData = obj.data;
    let ctx = obj.ctx;

    //n'affiche pas le tooltip si nous sommes sur les relations et que la minifiche est active ( double infobulle sinon)
    /*ELAIZ- demande 80028 - pas de tooltip si on passe la souris sur le champs Raison social pour une
    société*/
    if (obj.data && ((objData.Format == FieldType.AliasRelation && objData.IsMiniFileEnabled)
        || (objData.Format == FieldType.Relation && objData.Value != "")
        || (objData.Format == FieldType.AliasRelation && objData.DescId == 301 && obj.data?.ReadOnly == true))
    ) {
        return false;
    }


    let newData = "";
    let position;
    let msgTooltip = "";
    let msgError = "";
    let parentElement = ctx.$parent.$options.name.toLowerCase();
    let currentElm = (obj.data) ? (Object.keys(FieldType)
        .find(key => FieldType[key] == obj.data?.Format) || "")
        .toLowerCase() : '';


    if (obj.data) {
        let displayValue = obj.data?.Format == 14 && obj.dateFormat ? obj.dateFormat : obj.data?.DisplayValue ? obj.data?.DisplayValue : obj.data?.Value ? obj.data?.Value : "";
        newData = (obj.dataMru || obj.emptyMru || obj.allMru || obj.merginButton) ? obj.label : (displayValue && displayValue.includes(";")) ? displayValue.split(';').join(',') : displayValue;

        if (obj.data?.ToolTipText && !(obj.dataMru || obj.emptyMru || obj.allMru || obj.merginButton))
            msgTooltip = obj.data?.ToolTipText;

        if (obj.data?.ErrorMessage != "")
            msgError = obj.data?.ErrorMessage;
    } else if (obj.bkmScroll) {
        newData = ctx.getRes(2669)
    }

    let dvPosition;
    if (obj.elem == "info") {
        dvPosition = ctx.$refs['label_info' + obj.data?.DescId].find(lb => lb.offsetParent);
    } else if ((ctx.$refs['mruCatalog'] && obj.dataMru) || obj.merginButton) {
        dvPosition = ctx.$refs[obj.elem][obj.id];
    } else if (obj.emptyMru || obj.allMru || !ctx.$refs[obj.elem].length) {
        dvPosition = ctx.$refs[obj.elem]
    } else if (obj.bkmScroll) {
        dvPosition = ctx.$parent.$el
    } else {
        dvPosition = ctx.$refs['catalog'];
    }

    position = dvPosition
        ? dvPosition.getBoundingClientRect()
        : { "x": 0, "y": 0, "width": 0, "height": 0, "top": 0, "right": 0, "bottom": 0, "left": 0 };


    let mousePosition = (obj.dataMru || obj.emptyMru || obj.allMru || obj.merginButton) ? { "mouseY": event.clientY, "mouseX": event.clientX } : "";

    /** Si la tooltip est complètement vide.
        * Si c'est un header (ou que le message est vide)
        * Si les valeurs sont vides
        * Si ce n'est un élément à astuce
        *
        * Bref, avec tout ca on a une belle popup vide, donc on sort.
        * */
    let { tabAstuces, tabParentForbidden } = await import(AddUrlTimeStampJS("../methods/eComponentConst.js"));

    if ((tabParentForbidden.includes(parentElement) || (!msgTooltip || msgTooltip == "") || (!msgError || msgError == ""))
        && (!newData || newData == "")
        && (!tabAstuces[currentElm] || readonly))
        return false;

    let tooltipObj = {
        visible: obj.visible,
        elem: obj.elem,
        icon: obj.icon,
        readonly: obj.readonly,
        newData: newData,
        msgTooltip: msgTooltip,
        msgError: msgError,
        typeModal: "tooltip",
        position: position,
        info: (obj.elem == "info"),
        parentElement: ctx.$parent.$options.name.toLowerCase(),
        parentElementDOM: ctx.$parent.$el,
        currentElement: currentElm,
        mousePosition: mousePosition
    }

    ctx.setTooltipObj(tooltipObj);

    //let emitEvent = (!obj.bkmScroll) ? 'tooltipModal' : 'tooltipModalInComponent'

    //EventBus.$emit(emitEvent, objTooltip);
}

/**
 * Ouvre une popup pour changer d'utilisateur.
 * @param {any} descId le descID du champ
 * @param {any} catalogTitle le titre du catalogue
 * @param {any} fullUserList on veut la liste des utilisateurs complète
 * @param {any} multi multi ou non
 * @param {any} cancelDialog fonction de fermeture pour annulation
 * @param {any} validateDialog fonction de validation.
 * @param {any} selected la valeur sélectionnée
 * @param {any} libelle un libelle? J'ai bon?
 */
export function openUserDialog(
    descId,
    catalogTitle,
    fullUserList,
    multi,
    cancelDialog,
    validateDialog,
    selected,
    libelle
) {

    top.setWait(true);

    this.selectedValues = new Array();
    this.selectedLabels = new Array();
    this.currentValues = new Array();
    this.currentLabels = new Array();


    var showEmptyGroup = false; // TODO? this.sourceElement.getAttribute("showemptygroup");

    var showUserOnly = false; //TODO? this.sourceElement.getAttribute("showuseronly");
    var showCurrentGroupFilter = false;// this.sourceElement.getAttribute("showcurrentgroupfilter");
    var showCurrentUserFilter = false; //this.sourceElement.getAttribute("showcurrentuserfilter");
    var useGroup = false;// this.sourceElement.getAttribute("usegroup");
    var bShowCurrentUser = false // this.sourceElement.getAttribute("showcurrentuser") != "" ? this.sourceElement.getAttribute("showcurrentuser") == "1" : true;    //Vrai => Propose dans le catalogue : l'utilisateur en cours



    var maxWidth = 550; //Taille max à l'écran (largeur)
    var maxHeight = (multi == 1) ? 640 : 590; //Taille max à l'écran (hauteur)
    var oTabWH = top.getWindowWH(top);
    var nWidth = oTabWH[0];
    var nHeight = oTabWH[1];
    if (nWidth > maxWidth)   //si largeur de l'écran plus grande que largeur max alors il faut utiliser la largeur max
        nWidth = maxWidth;
    else
        nWidth = nWidth - 10;   //marge de "sécurité"
    if (nHeight > maxHeight)   //si hauteur de l'écran plus grande que largeur max alors il faut utiliser la largeur max
        nHeight = maxHeight;
    else
        nHeight = nHeight - 10;   //marge de "sécurité"

    if (this.advancedDialog != null) {
        try {
            this.advancedDialog.hide();
            if (!(this.advancedDialog.bScriptOk && this.advancedDialog.bBodyOk))
                top.setWait(false);
        }
        catch (e) {
            debugger;
        }
    }

    this.advancedDialog = new eModalDialog(catalogTitle, 0, "eCatalogDialogUser.aspx", nWidth, nHeight, "userdialog");

    this.advancedDialog.ErrorCallBack = function () { setWait(false); }
    this.advancedDialog.TargetObject = obj;

    this.advancedDialog.addParam("multi", multi ? "1" : "0", "post");
    this.advancedDialog.addParam("selected", selected, "post");
    this.advancedDialog.addParam("descid", descId, "post");

    this.advancedDialog.addParam("showemptygroup", showEmptyGroup ? "1" : "0", "post");
    this.advancedDialog.addParam("showuseronly", showUserOnly ? "1" : "0", "post"); //si à 1 => la liste sera toujours sans groupes d'affichés
    this.advancedDialog.addParam("fulluserlist", fullUserList ? "1" : "0", "post");
    this.advancedDialog.addParam("modalvarname", this.jsVarName, "post");
    //On ajoute le finder à la liste des catalogues utilisateurs OUVERT
    top.eTabCatUserModalObject.Add(this.advancedDialog.iframeId, this.advancedDialog);
    this.advancedDialog.addParam("iframeId", this.advancedDialog.iframeId, "post");


    this.advancedDialog.addParam("showcurrentuser", (bShowCurrentUser ? "1" : "0"), "post");
    this.advancedDialog.addParam("showcurrentgroupfilter", showCurrentGroupFilter, "post"); //Si à 1 => Proposition dans le catalogue : <le groupe de l'utilisateur en cours> pour filtre avancé
    this.advancedDialog.addParam("showcurrentuserfilter", showCurrentUserFilter, "post");   //Si à 1 => Proposition dans le catalogue : <utilisateur en cours> pour filtre avancé
    this.advancedDialog.addParam("usegroup", useGroup, "post"); //si à 1 => Autorise la sélection de groupe pour le catatalogue simple

    this.advancedDialog.addParam("showvalueempty", "0", "post"); //si à 1 => Proposition dans le catalogue : <Vide> sur le catalogue simple
    this.advancedDialog.addParam("showvaluepublicrecord", "1", "post"); //si à 1 => Proposition dans le catalogue : <Fiche Publique> sur le catalogue simple

    this.advancedDialog.onIframeLoadComplete = function () { top.setWait(false); };
    this.advancedDialog.ErrorCallBack = function () { top.setWait(false); };

    //SPH : 24667
    // il faut masquer la liste des mru avant d'afficher la popup de recherche sinon les actions des mru restent partiellement actifs
    //if (this.parentPopup && this.parentPopup.hide)
    //    this.parentPopup.hide();

    this.advancedDialog.show();

    //Appel des fonctions cancelDialog et validateDialog via des fonctions anonymes auto-appelantes pour être compatible sur safari 
    //Remplacement de l'appel à la méthode addButton par addButtonFct car elle est plus approprié au contexte ( pas de relation avec eFieldEditor)
    //Remplacement de  cancelDialog par this.advancedDialog.hide) () car la 1ère appelait une méthode provenant de eFieldEditor

    this.advancedDialog.addButtonFct(top._res_29, (() => this.advancedDialog.hide)(), "button-gray", "cancel");
    this.advancedDialog.addButtonFct(top._res_5003, (() => validateDialog)(), "button-green", "ok");
}

/**
 *
 * @param {any} val bool qui dit au parent (fileDetail) si le champs est en focus ou non
 */
export async function hideTooltip(val) {
    //this.$emit('hide-tooltip',val);
    EventBus.$emit('tooltipFocus', val);
}


/**
 * Permet d'afficher le tooltip personnalisé si paramétré en admin ( i au niveau du label )
 * @param {any} input représente les valeurs de chaque rubrique (inputs, dataInput...)
 * @param {any} val bool tooltip visible ou non
 * @param {any} event cible pour positionner le tooltip
 */
export async function infoTooltip(input, val, event) {

    let newData = input.ToolTipText;
    let position = event.target.getBoundingClientRect();


    EventBus.$emit('tooltipModal', {
        visible: val,
        elem: event.target,
        icon: false,
        readonly: false,
        newData: newData,
        msgTooltip: '',
        msgError: '',
        typeModal: "tooltip",
        position: position,
        info: true
    });

}

/**
 * Permet de MAJ toutes les rubriques liées au parent sur un signet (ex rubriques de Contact sur 1 signet sur l'onglet Contact)
 * @param {any} that repéresente le composant
 * @param {any} target event.target
 * @param {any} newVal nouvelle valeur après sortie du champs
 * @param {any} oldVal ancienne valeur avant modifs
 * @param {any} responseJson la réponse de Api aprés modification
 */
export function updateListValue(that, target, newVal, oldVal, responseJson) {

    let findresult = responseJson?.RefreshFieldNewValues?.
        find(fld => fld.Field && fld.Field.Descid == that.dataInput.DescId && Array.isArray(fld.List) && fld.List.length > 0);

    if (findresult) {
        let extended = { ...findresult.List[0].ExtendedProperties }
        updateListVal(that, target, newVal, oldVal, extended);
    }
    else {
        
        // envoyer sNewDisplay si elle existe, pour eRelation
        let extendedProps = {};
        if (responseJson.sNewDisplay) {
            extendedProps['sNewDisplay'] = responseJson.sNewDisplay;
        }
        
        // Mise à jour seulement s'il n'y a pas d'erreur
        updateListVal(that, target, newVal, oldVal, extendedProps);
    }

}
/**
 * Permet de MAJ toutes les rubriques liées au parent sur un signet (ex rubriques de Contact sur 1 signet sur l'onglet Contact)
 * @param {any} that repéresente le composant
 * @param {any} target event.target
 * @param {any} newVal nouvelle valeur après sortie du champs
 * @param {any} oldVal ancienne valeur avant modifs
 * @param {any} extendedProps propiétés additionelles
 */
export async function updateListVal(that, target, newVal, oldVal, extendedProps) {

    if (!that._isVue || (that.dataInput === null || that.dataInput === undefined)) {
        return;
    }

    //Vérifie que la rubrique MAJ est lié au parent ( pas un signet d'un autre onglet) et que le nombre de rubriques liées est sup à 1
    if (getTabDescid(that.dataInput.DescId) != that.$root.$children.find(app => app.$options.name == "App").$children.find(app => app.$options.name == "v-app").$children.map(x => x.$children)[0][0].did && document.querySelectorAll(`div[fileid='${that.dataInput.FileId}'][divdescid='${that.dataInput.DescId}']`).length < 2) {
        return;
    }


    that.bEmptyDisplayPopup = false;
    let borderErr = that.$parent.$children.filter(x => x.dataInput?.DescId == that.dataInput.DescId)
        .map(x => x.$el.parentElement.parentElement.classList.contains("border-error"));

    // enlève la bordure d'erreur si existante
    if (that.$parent.$el.querySelectorAll('.border-error')) {
        [].slice.call(that.$parent.$el.querySelectorAll(`[divdescid="${that.dataInput.DescId}"].border-error`)).map(x => x.classList.remove("border-error"));
    }

    let treated = false;
    let objLab = []
    let objVal = []
    let editedValObj = {
        NewDisp: null,
        NewValue: '',
        DescId: that.dataInput.DescId,
        FileId: that.dataInput.FileId
    };

    switch (that.dataInput.Format) {
        case FieldType.Character: case FieldType.File: case FieldType.Memo:
            editedValObj.NewValue = newVal
            break;
        case FieldType.Catalog: case FieldType.User: case FieldType.Relation:
            if (that.dataInput.DisplayValue == "" && that.dataInput.Value != "") {
                editedValObj.NewDisp = that.selectedLabels.toString().replace(/ *, */g, ";")
                editedValObj.NewValue = that.dataInput.Value;
            } else {
                editedValObj.NewDisp = that.dataInput.DisplayValue;
                editedValObj.NewValue = that.dataInput.Value;
            }
            break;
        case FieldType.Phone: case FieldType.SocialNetwork:
        case FieldType.Geolocation: case FieldType.Button: case FieldType.Logic: case FieldType.Image: case FieldType.HyperLink:
            editedValObj.NewValue = newVal;
            break;
        case FieldType.Money: case FieldType.Numeric:
            editedValObj.NewDisp = newVal;
            editedValObj.NewValue = newVal;
            break;
        case FieldType.Date:
            editedValObj.NewDisp = newVal;
            editedValObj.NewValue = that.dataInput.Value;
            break;
        case FieldType.MailAddress:
            editedValObj.NewValue = newVal;
            if (extendedProps) {
                editedValObj.MailStatusEudo = extendedProps.MailStatusEudo;
                editedValObj.MailStatusTech = extendedProps.MailStatusTech;
                editedValObj.MailStatusSubTech = extendedProps.MailStatusSubTech;
            }
            break;
        case FieldType.AliasRelation:
            editedValObj.NewValue = newVal;
            editedValObj.NewDisp = extendedProps?.sNewDisplay;
            break;
        default:
            break;
    }

    let fic = that.$root.$children.find(app => app.$options.name == "App")
        ?.$children.find(app => app.$options.name == "v-app")
        ?.$children.find(vapp => vapp.$options.name == "v-main")
        ?.$children.find(vapp => vapp.$options.name == "fiche")
    let prop = fic?.propStep;


    if (prop && prop?.DescId == that.dataInput.DescId) {
        prop.Value = editedValObj.NewValue;
        prop.DisplayValue = editedValObj.NewDisp;

        fic.counterReload++;
    }

    EventBus.$emit('valueEdited', editedValObj);

}


/**
 * Gestion de la maj des propriétés étendues d'un champ
 * @param {any} item repéresente le champ
 * @param {any} extendedProp ensemble des propriétés
 
 */
export function handleExtendedProperties(item, extendedProp) {

    if (item.Format === FieldType.MailAddress) {
        item.MailStatusEudo = extendedProp.MailStatusEudo;
        item.MailStatusTech = extendedProp.MailStatusTech;
        item.MailStatusSubTech = extendedProp.MailStatusSubTech;
    }

}


export function verifyValChanged(event) {
    if (event.target.classList.contains('changed')) {
        // this.dataInput.Value = event.target.value;
        event.target.classList.remove('changed');
    }
}

export function Observer(that, test) {
    that.tables.Test = test;
    that.tables.Test.observe(that.$refs.row[that.$refs.row.length - 1]);
}


/**
 * Permet de savoir si on doit afficher l'icone d'information ou non.
 * */
export function showInformationIco() {
    return this.dataInput.ToolTipText.length > 1 && !this.propHead
}


/**
 * Permet de d'afficher l'icone d'information.
 * S'il y a un titre, on fait gauche + taille du titre.
 * Si l'étoile est présente, on fait gauche + taille de l'étoile.
 * Comme la gauche de l'étoile est à droite du titre, on écrase la valeur précédente...
 * Enfin, on met le calcul dans la gauche du i, qui devrait donc se trouver à droite
 * de tout le reste.
 * */
export function displayInformationIco() {
    let offset = 0;

    if (this.$parent.$refs["label_" + this.dataInput.DescId]
        && this.$parent.$refs["label_" + this.dataInput.DescId].length > 0) {

        let tabLab = this.$parent.$refs["label_" + this.dataInput.DescId]
            .filter(nd => !nd.classList.contains("labelHidden"))

        if (tabLab && tabLab.length > 0)
            offset = tabLab
                .map(nd => nd.offsetLeft + nd.offsetWidth)
                .reduce((accu, val) => accu + val);
    }

    if (this.$parent.$refs["hidden_" + this.dataInput.DescId]
        && this.$parent.$refs["hidden_" + this.dataInput.DescId].length > 0) {

        let offsetTemp = 0;
        let tabMrHide = this.$parent.$refs["hidden_" + this.dataInput.DescId]

        if (tabMrHide && tabMrHide.length > 0)
            offsetTemp = tabMrHide
                .map(nd => nd.offsetLeft + nd.offsetWidth)
                .reduce((accu, val) => accu + val);

        if (offsetTemp > 0)
            offset = offsetTemp;
    }

    if (this.$refs["info"]) {

        this.$refs["info"].style.left = offset + "px";
    }
};

/** détermine l'ellipsis des tags des catalogues simples
    * on utilise un canvas afin de déterminer  la taille que peut occuper le texte avec une famille de police et une taille donnée
* utilise l'id catval utilisé dans chaque catalogue simple modifiable */
export function setCatalogEllipsis() {

    /* US 3398  -> tache 4949  Trop gourmand au niveau des perfs a voir pour autre choses comme un tooltip */
    /*

    if (this.propListe
        || !(document.getElementById(this.getUniqueId)))
        return false;

    if (this.$refs.catVal)
        this.catVal = this.$refs.catVal;
    else if (!this.$refs.catVal && !this.dataInput.Multiple)
        this.catVal = this.$refs['field' + this.dataInput.DescId];
    else {
        return false;
    }

    let txt = this.dataInput.DisplayValue != '' ? this.dataInput.DisplayValue : this.catVal.innerText;
    let catValStyle = getComputedStyle(this.catVal);
    let catValWidth = this.catVal.getBoundingClientRect().width;

    let txtFont = `
            ${catValStyle.fontWeight}
            ${catValStyle.fontSize}
            ${catValStyle.fontFamily} `;
    let txtSize = getTextWidth(txt, true, txtFont, null, null, true);

    let tabsBar = this.$root.$children.find(app => app.$options.name == "App")
        .$children.find(file => file.$options.name == "fiche")
        .$children.find(tab => tab.$options.name == "tabsBar");

    let details = tabsBar ? tabsBar.$refs.details?.find(detail => detail) : null;
    let stepContent = this.$parent.$options.name == "stepsBar" ? this.$parent.$refs.stepContent : null;
    let stepContentHidden = stepContent ? stepContent.filter(stepbar => stepbar.classList.contains('displayNone')) : null;

    //ELAIZ - je met un largeur auto car la classe css truncated-text empêche de mesurer correctement la largeur du tag
    if (this.$refs.catLi)
        this.$refs.catLi.style.width = 'auto';

    /* ELAIZ  - Comme la zone détails, il faut enlever le display none sur les étapes cachées afin de cacluler la largeur des champs
    this.containerWidth = catValWidth;

    if (stepContentHidden) {
        stepContentHidden.forEach(stepbar => stepbar.classList.remove('displayNone'));
        stepContentHidden.forEach(stepbar => stepbar.classList.add('displayNone'));
    } else if (this.$parent.$options.name == "fileDetail") {
        details.style.display = 'block';
        details.style.visibility = 'hidden';
        details.style.display = 'none';
        details.style.visibility = 'visible';
        /* ELAIZ  - ajout d'une vérification, si nous ouvrons la fiche pour la première fois, l'image sur la gauche n'est pas présente et modifie la largeur des champs. Ainsi, on enlève la différence pour calculer la bonne largeur
    }

    if (txtSize > this.containerWidth) {
        this.truncatedTxt = true;
        this.truncatedSize[this.getUniqueId] = txtSize;
    } else {
        this.truncatedTxt = false;
    }
    if (this.$refs.catLi)
        this.$refs.catLi.style.width = '';

        */
}

/**
 * Calcule la largeur en pixels d'un texte donné en fonction d'une police de caractères donnée. Utilise la méthode HTML5 canvas.measureText
 *
 * @param {String} text Le texte à mesurer
 * @param {Boolean} roundValue Indique si la valeur doit être arrondie (true) ou non (false)
 * @param {String} textFont La police avec laquelle mesurer le texte, en langage de description CSS (exemple : "bold 14px verdana").
 * @param {String} textAlign Alignement à utiliser pour le texte. Peut avoir une influence sur le calcul. Optionnel
 * @param {String} textBaseline Positionnement vertical relatif à utiliser pour le texte. Peut avoir une influence sur le calcul. Optionnel
 * @param {Boolean} appendToBody Indique si le canevas créé doit être attaché au corps de la page. Peut être nécessaire sur certains navigateurs ?
 *
 * @see https://stackoverflow.com/questions/118241/calculate-text-width-with-javascript/21015393#21015393
 */
export function getTextWidth(text, roundValue, textFont, textAlign, textBaseline, appendToBody) {
    // Si le texte à mesurer est vide, on renvoie 0
    if (text?.length == 0)
        return 0;
    // Création - Réutilisation de l'objet canvas, si déjà créé, pour des questions de performance
    var canvas = getTextWidth.canvas || (getTextWidth.canvas = document.createElement("canvas"));
    canvas.style.position = "fixed";
    if (appendToBody)
        document.body.append(canvas);
    // Personnalisation
    let context = canvas.getContext("2d");
    context.font = textFont || `Lato sans-serif 15px`;
    if (textAlign)
        context.textAlign = textAlign;
    if (textBaseline)
        context.textBaseline = textBaseline;
    // Mesure
    let metrics = context.measureText(text);
    // Nettoyage
    if (appendToBody)
        document.body.removeChild(canvas);
    // Renvoi du résultat
    return roundValue ? Math.round(metrics.width) : metrics.width;
}

/**Permet de stopper complètement la transition si l'on sort du tag catalogue */
export function stopTransition() {
    if (this.catVal && event.relatedTarget != this.$refs.changeDir) {
        this.translationVal = 0;
        this.catVal.style.transition = ""
        this.catVal.style.direction = ""
        document.documentElement.style.setProperty('--textIndentEnd', '0px');
        this.textReverse = false;
    }
};

/**
 * Fonction PRIVEE exécutée par onCatalogValueMouseLeave ou onCatalogValueMouseOver
 * @param {boolean} bShowTooltip Indique si on doit afficher ou masquer l'infobulle
 * @param {any} ctx Contexte déclencheur (this)
 * @param {any} event Evènement déclencheur
 * @param {any} value Valeur de catalogue concernée (pour les catalogues multiples)
 * @param {any} dataInput Catalogue source
 */
function onCatalogValueMouseAction(bShowTooltip, ctx, event, value, dataInput, elem='') {
    let targetValue = value;
    // Dans le cas des valeurs de catalogue simple, le parent ne passe pas le paramètre value, puisque la valeur affichée dans le catalogue est celle du catalogue lui-même (une seule valeur possible)
    // On prend alors la value du champ dans ce cas de figure (et seulement celui-ci)
    if (!targetValue)
        targetValue = {
            id: dataInput.Value, value: dataInput.DisplayValue
        };
    if (targetValue) {
        if (dataInput && dataInput.CatalogValues) {
            let targetValueFromSource = dataInput?.CatalogValues.find(f => f.DbValue == targetValue.id);
            let target; // cible de l'infobulle
            // Si on est dans le cas du catalogue multiple, on ne cible pas l'élément global mais seulement la valeur sur laquelle on pointe
            if (elem) {
                target = elem
            } else if (dataInput.Multiple) {
                target = event?.target?.id != '' ? event?.target?.id : event.target?.parentElement?.id;
            } else {
                target = 'field' + dataInput.DescId;
            }
                
            if (targetValueFromSource?.ToolTipText?.trim() != "") {
                // On inscrit l'infobulle à afficher dans la propriété "ValueToolTipText" de dataInput, pour que la fonction centralisée showTooltip gère le cas sans modification
                dataInput.ValueToolTipText = targetValueFromSource?.ToolTipText;
                ctx.showTooltip(bShowTooltip, target, false, ctx.IsDisplayReadOnly, dataInput);
            }
        }
    }
};

/**
 * Fonction exécutée au passage du curseur de la souris sur une valeur de catalogue
 * @param {any} event Evènement déclencheur
 * @param {any} value Valeur de catalogue concernée (pour les catalogues multiples)
 * @param {any} dataInput Catalogue source
 */
export function onCatalogValueMouseOver(event, value, dataInput, elem='') {
    return onCatalogValueMouseAction(true, this, event, value, dataInput, elem);
};

/**
 * Fonction exécutée à la SORTIE du curseur de la souris d'une valeur de catalogue
 * @param {any} event Evènement déclencheur
 * @param {any} value Valeur de catalogue concernée (pour les catalogues multiples)
 * @param {any} dataInput Catalogue source
 */
export function onCatalogValueMouseLeave(event, value, dataInput) {
    return onCatalogValueMouseAction(false, this, event, value, dataInput);
};

/**
 * Fonction qui permet d'effectuer une translation du texte si celui-ci est plus long que le container
 * Il est lié à une animation css, nous modifions la valeur d'indentation du texte et la transition en fonction de la longueur du texte
 * J'ai rajouté un setTimeout car on ne peut pas mettre de transition sur un changement de direction en css( direction : ltr par ex)
 * Ainsi cela permet d'avoir la fin du défilement avant le changement de direction
 * */
export function translateText() {
    this.once = false;
    let revertText = () => {
        setTimeout(() => {
            if (!this.textReverse && !this.once) {
                this.textReverse = true;
            } else if (this.textReverse && !this.once) {
                this.textReverse = false;
            }
            this.once = true;
            this.catVal.removeEventListener('animationend', revertText);
        }, 500)
    }

    let translationValue = -(this.truncatedSize[this.getUniqueId] - this.containerWidth);
    let transitionValue = Math.abs(translationValue) > 100 ? (Math.floor(Math.abs(translationValue / 100)) * 2) : 1;
    this.catVal.style.animationDuration = `${transitionValue}s`
    document.documentElement.style.setProperty('--textIndentEnd', translationValue + 'px');
    this.catVal.addEventListener('animationend', revertText, true);
};

/**
 * Defilement du texte des tags des catalogue multiples
 * @param {any} e l'evenement
 */
export function translateTextForMultiple(e) {
    let elem = e.target;
    let elemComputedStyle = getComputedStyle(elem);
    let canvas = document.createElement('canvas');
    canvas.style.position = "fixed";
    var txt = elem.innerText;
    document.body.appendChild(canvas);
    var ctx = canvas.getContext("2d");
    ctx.font = `${elemComputedStyle?.fontWeight}  ${elemComputedStyle?.fontSize} ${elemComputedStyle?.fontFamily}`;
    let txtSize = Math.round(ctx.measureText(txt).width);
    let elemWidth = elem.clientWidth - getNumber(elemComputedStyle?.paddingRight);
    if (elemWidth - txtSize < 0) {
        document.documentElement.style.setProperty('--textIndentEnd', elemWidth - txtSize + 'px');
    }
    document.body.removeChild(canvas);
};

/**
 * On stop Defilement du texte des tags des catalogue multiples
 * @param {any} e l'evenement
 */
export function translateTextForMultipleStop(e) {
    document.documentElement.style.setProperty('--textIndentEnd', '0px');
};

/**
 * Affiche ou masque la mini-fiche
 * @param {object} event Evènement déclencheur
 * @param {boolean} show true pour afficher la minifiche, false pour la masquer
 * */
export function showVCardOrMiniFile(event, show) {
    if (this.dataInput.IsMiniFileEnabled || (this.dataInput.AliasSourceField && this.dataInput.AliasSourceField.IsMiniFileEnabled))
        shvc(event.target, show);
}

/**
 * Permet de MAJ la rubrique après sortie du champs
 * @param {any} that repéresente le composant
 * @param {any} newVal nouvelle valeur après sortie du champs
 * @param {any} oldVal nouveau libellé après sortie du champs
 */
export async function updateDataVal(that, newVal, newDisplayVal) {

    if (!that._isVue || (that.dataInput === null || that.dataInput === undefined)) {
        return;
    }

    let editedValObj = {
        NewDisp: null,
        NewValue: '',
        DescId: that.dataInput.DescId,
        FileId: that.dataInput.FileId
    };

    switch (that.dataInput.Format) {
        case FieldType.Character: case FieldType.File: case FieldType.Memo:
            editedValObj.NewValue = newVal
            break;
        case FieldType.Catalog: case FieldType.User: case FieldType.Relation:
            // that.dataInput.Value = newVal;
            that.dataInput.DisplayValue = newDisplayVal;
            if (that.dataInput.DisplayValue == "" && that.dataInput.Value != "") {
                editedValObj.NewDisp = that.selectedLabels.toString().replace(/ *, */g, ";")
                editedValObj.NewValue = that.dataInput.Value;
            } else {
                editedValObj.NewDisp = that.dataInput.DisplayValue;
                editedValObj.NewValue = that.dataInput.Value;
            }
            break;
        case FieldType.MailAddress: case FieldType.Phone: case FieldType.SocialNetwork:
        case FieldType.Geolocation: case FieldType.Button: case FieldType.Logic: case FieldType.Image: case FieldType.HyperLink:
            editedValObj.NewValue = newVal;
            break;
        case FieldType.Money: case FieldType.Numeric:
            editedValObj.NewDisp = that.dataInput.DisplayValue;
            editedValObj.NewValue = that.dataInput.Value;
            break;
        case FieldType.Date:
            editedValObj.NewDisp = newVal;
            editedValObj.NewValue = that.dataInput.Value;
            break;
        default:
            return;
    }

    EventBus.$emit('valueEdited', editedValObj);
}



/**
 * Calcul la longeur d'un champs en fonction de la value
 * @param {string} value Valeur que l'on doit calculer
 * */
export function sizeForm(value) {
    const letterMin = [
        {
            txt: ['a', 'b', 'c', 'd', 'e', 'g', 'h', 'k', 'n', 'o', 'p', 'q', 'r', 's', 'u', 'v', 'w', 'x', 'y', 'z'],
            lengthSize: 11
        },
        {
            txt: ['i', 'l', 'f', 'j', 't', '1'],
            lenghtSize: 5
        },
        {
            txt: 'm',
            lenghtSize: 12
        },
        {
            txt: ['A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'J', 'K', 'L', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'],
            lenghtSize: 13
        },
        {
            txt: 'I',
            lenghtSize: 10
        },
        {
            txt: 'M',
            lenghtSize: 14
        },
        {
            txt: ' ',
            lenghtSize: 13
        },
        {
            txt: ':',
            lenghtSize: 4
        }
    ]
    let size = 0;
    size = Array.from(value).reduce((accumulator, currentValue) =>
        accumulator + (letterMin.find(elem => elem.txt.includes(currentValue))?.lenghtSize || 14) / 14, 0)
    return Math.round(size);
}

/**
 * Permet de savoir si on affiche le i informationnel.
 * CE n'est le cas qu'en cas de tooltip ou de RGPD.
 * @param {any} input
 */
export function isShowingInformation(input) {
    return ((input?.ToolTipText && input?.ToolTipText?.length > 1) || input?.RGPD)
}

/**
 * Si on affiche ou masque le tooltip dans le cas où on a mis une infobulle en admin.
 * @param {any} bToDisplay
 * @param {object} input
 */
 export function isDisplayingToolTip(bToDisplay, input) {
    this.showTooltip(bToDisplay, 'info', false, input?.ReadOnly, input);
}

export function reloadBkm(ctx){
    let callSignet = {
        bkmLoaded: false,
        id: `id_${ctx.propSignet.DescId}_${ctx.propSignet.Label}`,
        nbLine: 10,
        pageNum: 1,
        scrollLeft: 0,
        signet: ctx.propSignet.DescId
    }
    EventBus.$emit('reloadSignet_' + callSignet.id, callSignet);
}