import { FieldType } from "./Enum.min.js?ver=803000";
import { JSONTryParse, getTabDescid  } from "./eMainMethods.js?ver=803000"
import { linkToCall } from "./eFileMethods.js?ver=803000"
import { loadFileMRU } from "../shared/XRMWrapperModules.js?ver=803000";

/**
 * Recherche depuis les MRU
 * @param {any} dataInput objet représentant le champ.
 * @param {string} search chaine de recherche.
 * 
 * 
 */
async function GetMruSearch(dataInput, search) {
    let param = null;
    if ([FieldType.AliasRelation, FieldType.Relation].indexOf(dataInput.Format) > -1) {
        //param = {
        //    targetTab: dataInput.TargetTab,
        //    descid: dataInput.DescId,
        //    search: search
        //};
        param ={
            TargetTab: dataInput.TargetTab,
            nDesc: dataInput.DescId,
            sSearch: search,
            nTabFrom: getTabDescid(dataInput.DescId),
            nFileId: dataInput.nFileId,
            bSearchAllUserDefined: true,
        }
    }
    else {
        param = {
            descid: dataInput.PopupDescId || dataInput.DescId,
            search: search,
            popupType: dataInput.PopupType,
            fieldType: dataInput.Format,
            parentValue: dataInput.Pdbv
        };
    }

    return GetMruFromParam(param);
}
/**
 * Chargement des MRU
 * @param {int} descId le descid du champs du catalogue.
 */
async function GetMru(descId) {
    let param = {
        descid: descId
    };

    return GetMruFromParam(param);
}

/**
 * Méthode privée/interne (non exportée) de chargement ou Recherche depuis les MRU, en fonction des paramètres donnés par GetMru ou GetMruSearch
 * @param {any} param Paramètres d'appel au back
 * 
 * 
 */
async function GetMruFromParam(param) {
    let oDataMRU = loadFileMRU();

    if (!(oDataMRU && param))
        return;

    return JSONTryParse(
        await linkToCall(
            oDataMRU.url,
            { ...oDataMRU.params, ...param }
        )
    )?.Values;
}

export { GetMru, GetMruSearch };

