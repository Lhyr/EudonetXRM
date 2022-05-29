import { store } from "../store/store.js?ver=803000";


/**
 * Permet de convertir une url relative en url absolue.
 * @param {string} href l'url à convertir
 * @returns {string} l'url convertie.
 */
function convertHrefRelativeToAbsolute(href) {
    var domaine = window.location.href.split("/");
    domaine.pop();
    var urlSpec = domaine.join("/");

    return new URL(href, urlSpec + "/").href;
}


/**
 * Fonction qui teste si un script a déjà été intégré dans la page.
 * @param {string} href le lien à rechercher dans les scripts
 * @returns {boolean} les balises scripts contiennent un source avec la lien en paramètre.
 */
function isScriptAlreadyIncluded(href) {
    var scripts = document.getElementsByTagName("script");

    // #68 13x - Prise en charge de l'éditeur de templates avancé - Inclusion de scripts distants
    if (!(href.substring(0, 7) == "http://" || href.substring(0, 8) == "https://" || href.substring(0, 2) == "//")) {

        href = "scripts/" + href + ".js"

        if (store.getters.getVersionJs != "")
            href += '?ver=' + store.getters.getVersionJs;

        href = convertHrefRelativeToAbsolute(href);
    }

    return [...scripts].some(src => src.src == href);

}


/**
 * ajoute une série de scripts à Iris Black, en vérifiant les doublons, de manière séquentielle (et non parallèle comme le ferait un Promise.all())
 * on fait pour cela un appel à eTools.addScripts(), en lui rpécisant d'utiliser addScriptIrisBlackAndAlwaysCallback plus bas, plutôt que addScript.
 * addScriptIrisBlackAndAlwaysCallback étant un raccourci vers addScriptIrisBlack() avec le paramètre bRunCallbackIfAlreadyIncluded à true.
 * Ce paramètre indiquant d'exécuter la fonction de callback passée en paramètre systématiquement, même si le script n'est pas ajouté parce qu'il existe
 * déjà (le callback étant alimenté par addScripts pour ajouter chaque script après avoir complété le précédent, séquentiellement, il faut
 * donc l'exécuter systématiquement, même si le script n'est pas ajouté)

 * @param {string} hrefArray le tableau des scripts à ajouter (relatif, absolu ou externe)
 * @param {string} stype le marqueur de script à ajouter
 * @param {function} callback la fonctoin de callback. 
 * @param {any} oDoc Objet document cible
 */
export function addScriptsIrisBlack(hrefArray, stype, callback, oDoc) {
    addScripts(hrefArray, stype, callback, oDoc, addScriptIrisBlackAndAlwaysCallback);
}


/**
 * ajoute un script à Iris Black, en vérifiant les doublons.
 * @param {string} href le lien du script (relatif, absolu ou externe)
 * @param {string} stype le marqueur de script à ajouter
 * @param {function} callback la fonction de callback. 
* @param {any} oDoc Objet document cible
 * @param {boolean} bRunCallbackIfAlreadyIncluded indique si on doit tout de même exécuter la fonction de callback si le script est déjà inclus. false par défaut
 */
export function addScriptIrisBlack(href, stype, callback, oDoc, bRunCallbackIfAlreadyIncluded) {
    if (!isScriptAlreadyIncluded(href))
        addScript(href, stype, callback, oDoc);
    else if (bRunCallbackIfAlreadyIncluded)
        callback();
}

/**
 * ajoute un script à Iris Black, en vérifiant les doublons, et en exécutant le callback systématiquement, même si le script est déjà existant.
 * @param {string} href le lien du script (relatif, absolu ou externe)
 * @param {string} stype le marqueur de script à ajouter
 * @param {function} callback la fonction de callback. 
 * @param {any} oDoc Objet document cible
 */
export function addScriptIrisBlackAndAlwaysCallback(href, stype, callback, oDoc) {
    addScriptIrisBlack(href, stype, callback, oDoc, true);
}

/**
 * Fonction qui teste si un link a déjà été intégré dans la page.
 * @param {string} href le lien vers du link à précharger.
 * @returns {boolean} les balises style contiennent un source avec la lien en paramètre.
 */
function isLinkAlreadyIncluded(href) {
    var link = document.getElementsByTagName("link");

    return [...link]
        .filter(stl => stl.rel == "preload")
        .some(stl => stl.href.includes(href));
}


/**
 * ajoute un script à Iris Black, en vérifiant les doublons.
 * @param {string} href le lien du script (relatif, absolu ou externe)
 * @param {string} relationship le rel du link
 * @param {string} typContent le type de contenu, le as du link
 * @param {strign} stype un tag permettant le nettoyage des éléments.
 */
export function addLinkScriptIrisBlack(href, relationship, typContent, stype) {
    if (store.getters.getVersionJs != "")
        href += '?ver=' + store.getters.getVersionJs;

    if (isLinkAlreadyIncluded(href))
        return;

    let hLink = document.createElement("link");
    hLink.href = href;
    hLink.rel = relationship;
    hLink.as = typContent;
    hLink.setAttribute("eType", stype);

    document.head.append(hLink);
}

/**
 * ajoute un script à Iris Black, en vérifiant les doublons.
 * @param {string} href le lien du script (relatif, absolu ou externe)
 * @param {string} stype un tag permettant le nettoyage des éléments.
 * @param {object} tag le tag auquel on doit ajouter la balise script.
 */
export function addScriptIrisBlackInTag(href, stype, tag) {
    if (store.getters.getVersionJs != "")
        href += '?ver=' + store.getters.getVersionJs;

    let sLink = document.createElement("script");
    sLink.src = href;
    sLink.setAttribute("eType", stype);

    tag.append(sLink);
}

/**
 * Fonction qui teste si un css a déjà été intégré dans la page.
 * @param {string} href le lien vers la feuille de style.
 * @returns {boolean} les balises style contiennent un source avec la lien en paramètre.
 */
function isCSSAlreadyIncluded(href) {
    var style = document.getElementsByTagName("link");

    if (!(href.substring(0, 7) == "http://" || href.substring(0, 8) == "https://" || href.substring(0, 2) == "//")) {
        href = "themes/default/css/" + href + ".css";

        if (store.getters.getVersionCSS != "")
            href += '?ver=' + store.getters.getVersionCSS;

        href = convertHrefRelativeToAbsolute(href);
    }    
 
    return [...style]
        .filter(stl => stl.rel == "stylesheet")
        .some(stl => stl.href == href);
}

/**
 * Monte les css pour Iris Black, en vérifiant les doublons.
 * @param {string} href le lien vers la feuille de style.
 * @param {string} stype le type de css à monter.
 */
export function addCSSIrisBlack(href, stype, oDoc) {

    if (!isCSSAlreadyIncluded(href))
        addCss(href, stype, oDoc);
}



///ajout un script JS "Brut"

/**
 * Ajout d'un script dans le dom.
 * On repère ce script via un identifiant, si besoin.
 * Si le script est déjà présent, on ne le rajoute pas.
 * @param {any} sScript le script
 * @param {any} identifiant l'identifiant du script
 * @param {any} oDoc l'endroit dans le dom.
 */
export function addScriptText(sScript, identifiant, oDoc) {
    let oScript;

    if (!oDoc || typeof (oDoc) == 'undefined') {
        oDoc = document;
    }

    if (identifiant)
        oScript = oDoc.querySelector("#" + identifiant);

    if (!oScript) {
        oScript = document.createElement('script');
        oScript.type = 'text/javascript';
        oScript.text = sScript;

        if (identifiant)
            oScript.id = identifiant;

        var oBody = oDoc.getElementsByTagName('body');

        if (oBody && oBody.length > 0)
            oBody[0].appendChild(oScript);
    }

}