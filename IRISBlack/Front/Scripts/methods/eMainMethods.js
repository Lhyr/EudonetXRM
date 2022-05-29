/**
 * Recupere la table depuis le DescId
 * @param {int} descid le descid dont on veut la table.
 * @returns {int} le numéro de la table.
 */
function getTabDescid(descid) {
    return descid - descid % 100;
}


/**
 * Permet de retourner une chaine, sans accent, sans espace...
 * Les espaces sont remplacés par des soulignés
 * @param {string} str
 */
function setNormalizeString(str) {
    return str.normalize("NFD").replace(/[\u0300-\u036f]/g, "").replace(/\W+/g, "_")
}

/**
 * Méthode permettant la recherche de caractère insensible aux accents et à la casse
 * @param {string} sContainer
 * @param {string} sContent
 */
function includesCIAI(sContainer, sContent) {
    let sContainer2 = setNormalizeString(sContainer).toLowerCase();
    let sContent2 = setNormalizeString(sContent).toLowerCase();

    return sContainer2.includes(sContent2);
}

/**
 * Permet d'essayer de transformer une string en objet JSON, 
 * ou un objet si c'est déjà un objet.
 * @param {any} sObject
 * @returns undefined ou l'objet.
 */
function JSONTryParse(sObject) {
    try {

        if (typeof sObject == "string")
            return JSON.parse(sObject);

        if (typeof sObject == "object")
            return sObject;

        return undefined;
    } catch (e) {
        return undefined;
    }
}

export { getTabDescid, setNormalizeString, includesCIAI, JSONTryParse };