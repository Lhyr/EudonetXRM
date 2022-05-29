/** Le type de requete HTTP possible. */
var TypeRequete = { GET: 1, POST: 2, PUT: 3, DELETE: 4 };
var TypeModale = { alerte: 1, popup: 2, box: 3 };


/**
 * Permet le chargement des scripts et des css utiles au nouveau mode fiche.
 * */
async function LoadScriptsAndCSS() {
    var { LoadJS, LoadCSS } = await import(AddUrlTimeStampJS("./LoadIrisScripts.js"));
    await Promise.all([LoadJS(), LoadCSS()]);
}

/**
 * Permet d'appeler simplement le Helper Axios.
 * @param {any} requete le type de requete que l'on souhaite appliquer
 * @param {any} url l'url de la requete.
 * @param {any} data une objet de données supplémentaire.
 * @param {any} fnCallBack une fonction de callback
 * @returns {any} un json des données retournées..
 */
async function LoadAxiosHelper(requete, url, data, fnCallBack) {
    let { default: eAxiosHelper } = await import(AddUrlTimeStampJS("./helpers/eAxiosHelper.js"));
    let helper = new eAxiosHelper(url);
    let updateReturn;

    switch (requete) {
        case TypeRequete.GET: updateReturn = helper.GetAsync.bind(helper);
            break;
        case TypeRequete.POST: updateReturn = helper.PostAsync.bind(helper);
            break;
        case TypeRequete.PUT: updateReturn = helper.PutAsync.bind(helper);
            break;
        case TypeRequete.DELETE: updateReturn = helper.PutAsync.bind(helper);
            break;
    }
    
    try {
        return JSON.parse(await updateReturn(data, fnCallBack));
    }
    catch (e) {
        throw e;
    }
}

/**
 * retourne un composant alerte suivant ce qu'on désire.
 * @param {any} modale le type de modale que l'on souhaite
 * @returns {any} une popup
 */
async function LoadModal(modale) {
    switch (modale) {
        case TypeModale.alerte: return import(AddUrlTimeStampJS("./components/modale/alertModale.js"));
        case TypeModale.box: return import(AddUrlTimeStampJS("./components/modale/alertBox.js"));
        case TypeModale.popup: return import(AddUrlTimeStampJS("./components/modale/popupModale.js"));
    }
}

/**
 * Permet de retourner le bon champ, suivant ce qu'on attend.
 * @param {any} field le champ que l'on souhaite retourner.
 * @returns {any} le champ que l'on souhaite retourner.
 */
async function LoadDynamicFormatChamps(field) {
    var { dynamicFormatChamps } = await import(AddUrlTimeStampJS("../index.js"));

    return dynamicFormatChamps({ Format: field });
}
