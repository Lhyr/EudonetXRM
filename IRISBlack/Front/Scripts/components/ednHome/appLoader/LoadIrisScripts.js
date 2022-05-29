/**
 *  Chargement des JS essentiels au fonctionnement de la home
 *  @return {Promise} une promesse sur le chargement des JS.
 */
async function LoadJS() {
    // Maj de JS
    top.clearHeader("EDNHOME", "JS");
    var tabScript = new Array();
   
    var PromiseJS = Promise.all([
        new Promise(resolve => resolve(addScripts(tabScript, "EDNHOME")))
    ]);
    return PromiseJS;
}

/**
 *  Chargement des CSS essentiels au fonctionnement de la page home.
 *  @return {Promise} une promesse sur le chargement des CSS.
 */
async function LoadCSS() {
    /** Chargement CSS */
    let sPathDecal ="../../../"; // Racine
    var PromiseCss = Promise.all([
        new Promise(resolve => resolve(top.clearHeader("EDNHOME", "CSS"))),
        new Promise(resolve => resolve(top.addCss(sPathDecal + "IRISBlack/Front/Scripts/Libraries/vuetify/vuetify.min", "EDNHOME"))), //Chargement des css pour vuetify
        new Promise(resolve => resolve(top.addCss(sPathDecal + "IRISBlack/Front/Assets/CSS/ednHome/ednHome", "EDNHOME"))), //Chargement des css pour la homePage
        new Promise(resolve => resolve(top.addCss(sPathDecal + "IRISBlack/Front/Assets/CSS/advForm/materialdesign/materialdesignicons.min", "EDNHOME"))), //Chargement des icons material design
        new Promise(resolve => resolve(top.addCss(sPathDecal + "IRISBlack/Front/Assets/CSS/advForm/googleFonts", "EDNHOME"))), //Chargement des fonts Google
        new Promise(resolve => resolve(top.addCss(sPathDecal + "IRISBlack/Front/Assets/CSS/advForm/eudoFront", "EDNHOME")))//Chargement des css pour eudoFront
    ]);
    return PromiseCss;
}
export { LoadJS, LoadCSS };