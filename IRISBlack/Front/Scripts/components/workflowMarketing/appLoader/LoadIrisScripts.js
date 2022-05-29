/**
 *  Chargement des JS essentiels au fonctionnement de la nouvelle fiche.
 *  @return {Promise} une promesse sur le chargement des JS.
 */
async function LoadJS() {
    // Maj de JS
    top.clearHeader("WORKFLOWMARKETING", "JS");
    var tabScript = new Array();
    //tabScript.push("eGrapesJSEditor");

    var PromiseJS = Promise.all([
        new Promise(resolve => resolve(addScripts(tabScript, "WORKFLOWMARKETING"))),
        new Promise(resolve => resolve(addScript("../IRISBlack/Front/scripts/components/workflowMarketing/flowy/flowy", "WORKFLOWMARKETING")))
    ]);
    return PromiseJS;
}

/**
 *  Chargement des CSS essentiels au fonctionnement de la nouvelle fiche.
 *  @return {Promise} une promesse sur le chargement des CSS.
 */
async function LoadCSS() {
    /** Chargement CSS */


    let sPathDecal = "../../../";

    var PromiseCss = Promise.all([

        new Promise(resolve => resolve(top.clearHeader("WORKFLOWMARKETING", "CSS"))),

        new Promise(resolve => resolve(top.addCss(sPathDecal + "IRISBlack/Front/Assets/CSS/ficheGrid", "WORKFLOWMARKETING"))),
        /* Ne pas changer l'ordre des css 
        TODO : vérifier avec un front, l'ordre ne devrait pas être important. 
        A vérifier également, même si promise all ordonne les retours correctement, la résolution
        effective dans l'ordre n'est pas garanti et donc, le rendu non plus
        */

        new Promise(resolve => resolve(top.addCss(sPathDecal + "IRISBlack/Front/Assets/CSS/font-awesome/css/all", "WORKFLOWMARKETING"))),
        new Promise(resolve => resolve(top.addCss(sPathDecal + "IRISBlack/Front/Scripts/Libraries/vuetify/vuetify.min", "WORKFLOWMARKETING"))), //Chargement des css pour vuetify
        new Promise(resolve => resolve(top.addCss(sPathDecal + "IRISBlack/Front/Scripts/components/workflowMarketing/flowy/flowy.min", "WORKFLOWMARKETING"))),

        new Promise(resolve => resolve(top.addCss(sPathDecal + "themes/default/css/grapesjs/grapes.min", "WORKFLOWMARKETING"))),
        new Promise(resolve => resolve(top.addCss(sPathDecal + "themes/default/css/eMemoEditor", "WORKFLOWMARKETING"))),
        new Promise(resolve => resolve(top.addCss(sPathDecal + "IRISBlack/Front/Assets/CSS/advForm/advForm", "WORKFLOWMARKETING"))),
        new Promise(resolve => resolve(top.addCss(sPathDecal + "IRISBlack/Front/Assets/CSS/flowy/flowy", "WORKFLOWMARKETING"))),

        new Promise(resolve => resolve(top.addCss(sPathDecal + "IRISBlack/Front/Assets/CSS/advForm/materialdesign/materialdesignicons.min", "WORKFLOWMARKETING"))), //Chargement des fonts Google
        new Promise(resolve => resolve(top.addCss(sPathDecal + "IRISBlack/Front/Assets/CSS/advForm/googleFonts", "WORKFLOWMARKETING"))), //Chargement des fonts Google
        new Promise(resolve => resolve(top.addCss(sPathDecal + "IRISBlack/Front/Assets/CSS/advForm/eudoFront", "WORKFLOWMARKETING")))//Chargement des css pour eudoFront
    ]);

    return PromiseCss;
}

export { LoadJS, LoadCSS };