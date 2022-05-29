
import { getRes, getUserLangID } from "../shared/XRMWrapperModules.js?ver=803000"
import { JSONTryParse } from "./eMainMethods.js?ver=803000"

/** Initialise le Json à renvoyer au back */
function initJsonAdmin() {
    return {
        Tab: 0,
        JsonSummary: {},
        WizardBarArea: {
            id: "stepsLine",
            type: "stepsLine",
            dataGsMinWidth: 7,
            dataGsMinHeight: 7,
            dataGsX: 0,
            dataGsY: 0,
            dataGsWidth: 12,
            dataGsHeight: 11,
            NoResize: true,
            NoDraggable: true,
            NoLock: true
        },
        JsonWizardBar: {},
        DetailArea: {
            id: "signets",
            type: "signets",
            dataGsMinWidth: 7,
            dataGsMinHeight: 7,
            dataGsX: 0,
            dataGsY: 11,
            dataGsWidth: 9,
            dataGsHeight: 11,
            NoResize: true,
            NoDraggable: true,
            NoLock: true
        },
        Activity: true,
        ActivityArea: {
            id: "activite",
            type: "activite",
            dataGsMinWidth: 3,
            dataGsMinHeight: 11,
            dataGsX: 9,
            dataGsY: 11,
            dataGsWidth: 3,
            dataGsHeight: 11,
            NoResize: true,
            NoDraggable: true,
            NoLock: true
        }
    };
}


/**
 * Permet de générer le Json de la zone résumé.
 * @param {any} dicElements
 */
function createJsonSummary(dicElements) {

    if (!dicElements)
        return;

    if (!Array.isArray(dicElements))
        dicElements = [dicElements];


    let result;

    try {

        let tabElementsUtils = dicElements.map(elem => {
            if (elem.value.val > 0)
                return { label: elem?.label, DescId: elem?.value?.val }
        });

        result = {
            "title": tabElementsUtils.find(elm => elm?.label == "Titre")?.DescId,
            "sTitle": tabElementsUtils.find(elm => elm?.label == "Sous-titre")?.DescId,
            "avatar": tabElementsUtils.find(elm => elm?.label == "Avatar")?.DescId,
        };
    } catch (e) {
        result = {
            "title": 0,
            "sTitle": 0,
            "avatar": 0,
        }
    }


    return result;
}


/**
 * Permet de générer le Json pour les composants de la zone résumé.
 * @param {any} dicElements
 */
function createJsonFieldsSummary(dicElements) {

    if (!dicElements)
        return;

    if (!Array.isArray(dicElements))
        dicElements = [dicElements];

    let result;

    try {
        result = dicElements.map(elem => {
            if (elem?.value?.val > 0)
                return {
                    DescId: elem?.value?.val
                }
        }).filter(n => n);
    } catch (e) {
        result = [];
    }

    return result;
}



/**
 * Permet de générer le Json pour les composants de la zone résumé.
 * @param {any} dicElements
 */
async function createJsonWizard(dicElements) {
    if (!dicElements)
        return {};

    if (!Array.isArray(dicElements))
        dicElements = [dicElements];

    let result;
    let nDescId = dicElements.find(elm => elm)?.value?.val;


    try {
        let { CallCatalogBack } = await import(AddUrlTimeStampJS("./eFileMethods.js"));
        let oRes = JSONTryParse(await CallCatalogBack(nDescId));
        let valls = oRes?.Values;
        let nMaxValues = valls?.length || 0;

        if (nDescId < 1)
            return {};

        result = {
            DescId: nDescId,
            HidePreviousButton: true,
            HideNextButton: true,
            WelcomeBoard: {
                Display: "hide",
                Body: {
                    lang_00: "",
                    lang_01: ""
                },
                Title: {
                    lang_00: "",
                    lang_01: ""
                }
            },
            FieldsById: []
        };

        result.WelcomeBoard.Title[`lang_${getUserLangID().toString().padStart(2, '0')}`] = getRes(2573);
        result.WelcomeBoard.Body[`lang_${getUserLangID().toString().padStart(2, '0')}`] = getRes(2574);

        result.FieldsById = valls.map((val, idx, arVal) => {
            let oPreviousValue = [];
            let oNextValue = [];

            if (idx < nMaxValues - 1
                && arVal[idx + 1]?.DbValue)
                oNextValue = arVal.filter((elm, id) => id > idx).map(ar => ar.DbValue);

            if (idx > 0
                && arVal[idx - 1]?.DbValue)
                oPreviousValue = arVal.filter((elm, id) => id < idx).map(ar => ar.DbValue);

            if (oNextValue?.length < 1)
                oNextValue = [0];

            if (oPreviousValue?.length < 1)
                oPreviousValue = [0];

            return {
                DataId: val?.DbValue || 0,
                DataIdPrevious: oPreviousValue,
                DataIdNext: oNextValue,
                DisplayedFields: []
            };
        });

    } catch (e) {
        console.error(e);
        result = {
            DescId: 0,
            HidePreviousButton: true,
            HideNextButton: true,
            WelcomeBoard: {
                Display: "hide",
                Body: {
                    lang_00: "",
                    lang_01: ""
                },
                Title: {
                    lang_00: "",
                    lang_01: ""
                }
            },
            FieldsById: []
        };
    }

    return result;

}


/**
 *  Permet de générer le Json de la zone résumé et assistant.
 * @param {any} arDicElements
 */
async function createJson(arDicElements) {

    if (arDicElements?.length < 1)
        return;

    let JsonToReturn = initJsonAdmin();


    let dicElement = arDicElements;
    if (!Array.isArray(arDicElements) && Array.isArray(arDicElements?.content))
        dicElement = arDicElements?.content;


    if (!dicElement)
        return;

    for (let nElements in dicElement) {
        switch (dicElement[nElements]?.name) {
            case "summaryFields": JsonToReturn.JsonSummary = Object.assign(JsonToReturn.JsonSummary, createJsonSummary(dicElement[nElements]?.content)); break;
            case "additionnalFields": JsonToReturn.JsonSummary["inputs"] = createJsonFieldsSummary(dicElement[nElements]?.content); break;
            case "stepBarFields": JsonToReturn.JsonWizardBar = await createJsonWizard(dicElement[nElements]?.content); break;
            default: continue;
        }
    }

    return JsonToReturn;

}


export { createJson };