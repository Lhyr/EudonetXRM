/**
 *  Chargement des JS essentiels au fonctionnement de la nouvelle fiche.
 *  @return {Promise} une promesse sur le chargement des JS.
 */
async function LoadJS() {
    let { addScriptIrisBlack, addScriptsIrisBlack } = await import(AddUrlTimeStampJS("../Scripts/methods/eFile.js"));

    let tabScript = new Array();
    tabScript.push("eFile");
    tabScript.push("eAutoCompletion");
    tabScript.push("ePopup");
    tabScript.push("eFieldEditor");
    tabScript.push("eGrapesJSEditor");
    tabScript.push("eMemoEditor");
    tabScript.push("ckeditor/ckeditor");
    tabScript.push("grapesjs/grapes.min");
    tabScript.push("grapesjs/grapesjs-plugin-ckeditor.min"); // plugin d'interfaçage grapesjs <=> CKEditor
    tabScript.push("grapesjs/grapesjs-blocks-basic.min");
    tabScript.push("grapesjs/grapesjs-preset-newsletter.min");
    tabScript.push("grapesjs/grapesjs-preset-webpage.min");

    /** Chargement JS */
    let PromiseJs = Promise.all([
        new Promise((resolve) =>
            resolve(
                /*
                US #3 209 - Tâche #4 784 - On ajoute twitter-bootstrap avec un paramètre stype
                ne correspondant PAS à une valeur utilisée sur les multiples appels à clearHeader()
                de l'application. On utilise son propre contexte/typage "BOOTSTRAP". Le but étant
                d'empêcher que clearHeader() supprime systématiquement la balise <script> correspondant
                à twitter-bootstrap, et de le faire que dans les cas où c'est absolument nécessaire.
                Bootstrap ajoutant des event listeners sur de nombreux éléments de la page,
                supprimer la balise <script> migre le script dans une VM qui continue d'exécuter
                les évènements. Par conséquent, on se retrouve avec autant d'appels à ces évènements,
                que de déchargements de balise <script> lorsqu'on recharge plusieurs fois la même page,
                entraînant des conflits d'évènements, comme un clic interprété deux fois sur un bouton
                avec menu dropdown (empêchant l'affichage dudit menu)
                */
                addScriptIrisBlack(
                    "../IRISBlack/Front/Scripts/libraries/twitter-bootstrap/js/bootstrap.min",
                    "BOOTSTRAP"
                )
            )
        ).then(
            new Promise((resolve) =>
                resolve(
                    addScriptIrisBlack(
                        "../IRISBlack/Front/Scripts/libraries/momentjs/moment-with-locales.min",
                        "FICHEIRIS",
                        () =>
                            addScript(
                                "../IRISBlack/Front/Scripts/libraries/bootstrap-datetimepicker/js/bootstrap-datetimepicker.min",
                                "FICHEIRIS"
                            )
                    )
                )
            )
        ),
        new Promise((resolve) =>
            resolve(
                addScriptIrisBlack(
                    "../IRISBlack/Front/Scripts/libraries/bootstrap-daterangepicker/moment.min",
                    "FICHEIRIS",
                    () =>
                        addScript(
                            "../IRISBlack/Front/Scripts/libraries/bootstrap-daterangepicker/daterangepicker.min",
                            "FICHEIRIS"
                        )
                )
            )
        ),
        new Promise((resolve) =>
            resolve(
                addScriptIrisBlack(
                    "../IRISBlack/Front/Scripts/libraries/select2/js/select2.full",
                    "FICHEIRIS"
                )
            )
        ),
        new Promise((resolve) =>
            resolve(
                addScriptIrisBlack(
                    "../IRISBlack/Front/Scripts/libraries/dexie/dexie.min",
                    "FICHEIRIS"
                )
            )
        ),       
        new Promise((resolve) =>
            resolve(
                // Ajoute les scripts communs E17/Eudonet x dans l'espace FILE, mais en vérifiant au préalable qu'ils n'y soient pas déjà
                // On passe par une méthode addScriptsIrisBlack, qui appelera eTools.addScripts() en lui précisant d'utiliser une
                // autre méthode que addScript() pour faire l'ajout, qui vérifiera l'existence du script.
                addScriptsIrisBlack(tabScript, "FILE", function () {
                    var oParamGoTabList = {
                        to: 3,
                        nTab: top.nGlobalActiveTab,
                        context: "LoadIrisScripts.LoadJS"
                    }

                    //Appel le waiter
                    setWait(false, undefined, undefined, isIris(top.getTabFrom()), oParamGoTabList);
                })
            )
        ),
    ]);

    return PromiseJs;
}

/**
 *  Chargement des CSS essentiels au fonctionnement de la nouvelle fiche.
 *  @return {Promise} une promesse sur le chargement des CSS.
 */
async function LoadCSS() {
    let { addCSSIrisBlack } = await import(AddUrlTimeStampJS("../Scripts/methods/eFile.js"));

    /** Chargement CSS */
    let PromiseCss = Promise.all([
        new Promise((resolve) => resolve(clearHeader("ADMIN", "CSS"))),
        new Promise((resolve) =>
            resolve(
                addCSSIrisBlack(
                    "../../../IRISBlack/Front/Scripts/libraries/vuetify/vuetify.min",
                    "FICHEIRIS"
                )
            )
        ),
        new Promise((resolve) =>
            resolve(
                addCSSIrisBlack(
                    "../../../IRISBlack/Front/Scripts/libraries/select2/css/select2.min",
                    "FICHEIRIS"
                )
            )
        ),

        new Promise((resolve) =>
            resolve(
                addCSSIrisBlack(
                    "../../../IRISBlack/Front/Scripts/libraries/twitter-bootstrap/css/bootstrap.min",
                    "FICHEIRIS"
                )
            )
        ),
        new Promise((resolve) =>
            resolve(
                addCSSIrisBlack(
                    "../../../IRISBlack/Front/Scripts/libraries/bootstrap3-wysiwyg/bootstrap3-wysihtml5.min",
                    "FICHEIRIS"
                )
            )
        ),
        new Promise((resolve) =>
            resolve(
                addCSSIrisBlack(
                    "../../../IRISBlack/Front/Scripts/libraries/datatables/css/dataTables.bootstrap.min",
                    "FICHEIRIS"
                )
            )
        ),
        new Promise((resolve) =>
            resolve(
                addCSSIrisBlack(
                    "../../../IRISBlack/Front/Scripts/libraries/bootstrap-datetimepicker/css/bootstrap-datetimepicker.min",
                    "FICHEIRIS"
                )
            )
        ),
        new Promise((resolve) =>
            resolve(
                addCSSIrisBlack(
                    "../../../IRISBlack/Front/Scripts/libraries/bootstrap-daterangepicker/daterangepicker.min",
                    "FICHEIRIS"
                )
            )
        ),
        new Promise((resolve) =>
            resolve(
                addCSSIrisBlack(
                    "../../../IRISBlack/Front/Assets/CSS/eudo-modal.min",
                    "FICHEIRIS"
                )
            )
        ),
        new Promise((resolve) =>
            resolve(
                addCSSIrisBlack(
                    "../../../IRISBlack/Front/Assets/CSS/eProgressBarv2.min",
                    "FICHEIRIS"
                )
            )
        ),
        new Promise((resolve) =>
            resolve(
                addCSSIrisBlack(
                    "../../../IRISBlack/Front/Assets/CSS/font-awesome/css/all.min",
                    "FICHEIRIS"
                )
            )
        ),
        new Promise((resolve) =>
            resolve(
                addCSSIrisBlack(
                    "../../../IRISBlack/Front/Assets/CSS/ionicons.min",
                    "FICHEIRIS"
                )
            )
        ),
        new Promise((resolve) =>
            resolve(
                addCSSIrisBlack("../../../IRISBlack/Front/Assets/CSS/main.min",
                    "FICHEIRIS"
                )
            )
        ),
        new Promise((resolve) =>
            resolve(
                addCSSIrisBlack(
                    "../../../IRISBlack/Front/Assets/CSS/ficheGrid.min",
                    "FICHEIRIS"
                )
            )
        ),
        new Promise((resolve) =>
            resolve(
                addCSSIrisBlack(
                    "../../../IRISBlack/Front/Assets/CSS/main-etienne.min",
                    "FICHEIRIS"
                )
            )
        ),
        new Promise((resolve) =>
            resolve(
                addCSSIrisBlack(
                    "../../../IRISBlack/Front/Assets/CSS/Eudonet.min",
                    "FICHEIRIS"
                )
            )
        ),
        new Promise((resolve) =>
            resolve(
                addCSSIrisBlack(
                    "../../../IRISBlack/Front/Assets/CSS/skin.min",
                    "FICHEIRIS"
                )
            )
        ),
        new Promise((resolve) =>
            resolve(
                addCSSIrisBlack("../../../IRISBlack/Front/Assets/CSS/lato.min", "FICHEIRIS")
            )
        ),
        new Promise((resolve) =>
            resolve(
                addCSSIrisBlack(
                    "../../../IRISBlack/Front/Assets/CSS/vollkorn.min",
                    "FICHEIRIS"
                )
            )
        ),
        new Promise((resolve) =>
            resolve(
                addCSSIrisBlack(
                    "../../../IRISBlack/Front/Assets/CSS/dropZone.min",
                    "FICHEIRIS"
                )
            )
        ),
        new Promise((resolve) =>
            resolve(
                addCSSIrisBlack("../../../IRISBlack/Front/Assets/CSS/blue.min", "FICHEIRIS")
            )
        ),
        new Promise((resolve) =>
            resolve(
                addCSSIrisBlack(
                    "../../../IRISBlack/Front/Assets/CSS/checkBox.min",
                    "FICHEIRIS"
                )
            )
        ),
        new Promise((resolve) =>
            resolve(
                addCSSIrisBlack(
                    "../../../IRISBlack/Front/Assets/CSS/modal-user-list.min",
                    "FICHEIRIS"
                )
            )
        ),
        new Promise((resolve) =>
            resolve(
                addCSSIrisBlack(
                    "../../../IRISBlack/Front/Assets/CSS/sweetalert.min",
                    "FICHEIRIS"
                )
            )
        ),
        new Promise((resolve) =>
            resolve(
                addCSSIrisBlack(
                    "../../../IRISBlack/Front/Assets/CSS/ModalsStyle.min",
                    "FICHEIRIS"
                )
            )
        ),
        //new Promise((resolve) =>
        //    resolve(
        //        addCSSIrisBlack(
        //            "../../../IRISBlack/Front/Assets/CSS/Google_Fonts",
        //            "FICHEIRIS"
        //        )
        //    )
        //),
        new Promise((resolve) =>
            resolve(
                addCSSIrisBlack("../../../IRISBlack/Front/Assets/CSS/chat.min", "FICHEIRIS")
            )
        ),
        new Promise((resolve) =>
            resolve(
                addCSSIrisBlack(
                    "../../../IRISBlack/Front/Assets/CSS/eAccordionPane.min",
                    "FICHEIRIS"
                )
            )
        ),
        new Promise((resolve) =>
            resolve(
                addCSSIrisBlack(
                    "../../../IRISBlack/Front/Assets/CSS/TabBarAside.min",
                    "FICHEIRIS"
                )
            )
        ),
        new Promise((resolve) =>
            resolve(
                addCSSIrisBlack(
                    "../../../IRISBlack/Front/Assets/CSS/design-general-v2.min",
                    "FICHEIRIS"
                )
            )
        ),
        new Promise((resolve) =>
            resolve(
                addCSSIrisBlack(
                    "../../../IRISBlack/Front/Assets/CSS/advForm/materialdesign/materialdesignicons.min",
                    "FICHEIRIS"
                )
            )
        ),
        new Promise((resolve) =>
            resolve(
                addCSSIrisBlack(
                    "../../../IRISBlack/Front/Assets/CSS/ePopOver.min",
                    "FICHEIRIS"
                )
            )
        ),
        new Promise((resolve) =>
            resolve(
                addCSSIrisBlack(
                    "../../../IRISBlack/Front/Assets/CSS/eList.min",
                    "FICHEIRIS"
                )
            )
        ),
        new Promise((resolve) =>
            resolve(addCSSIrisBlack("../../../themes/default/css/syncFusion/ej.web.all.min", "FICHEIRIS"))
        ),
    ]);

    return PromiseCss;
}

/**
 * Chargement des javascript provenant du projet tanspilé
 * Iris
 * @param dir 
 * @param tag
 * */
async function loadJSTrans(dir = "IrisCrimsonList", tag = "LISTIRIS") {
    let { addLinkScriptIrisBlack } = await import(AddUrlTimeStampJS("../Scripts/methods/eFile.js"));

    clearHeader("ADMIN", "JS");
    clearHeader("FILE", "JS");

    /** Chargement JS */
    let PromiseJs = Promise.all([
        new Promise((resolve) =>
            resolve(
                addLinkScriptIrisBlack(`IRISBlack/Front/Scripts/components/${dir}/js/app.js`, "preload", "script", tag)
            )
        ),
    ]);
    return PromiseJs;
}

/**
 * Chargement des CSS provenant du projet tanspilé
 * Iris
 * @param dir
 * @param tag
 * */
async function loadCSSTrans(dir = "IrisCrimsonList", tag = "LISTIRIS") {
    let { addCSSIrisBlack, addLinkScriptIrisBlack } = await import(AddUrlTimeStampJS("../Scripts/methods/eFile.js"));

    clearHeader("ADMIN", "CSS");
    /* Suppression des CSS du nouveau mode fiche, notamment à cause des effets de bords liés à Boostrap */
    activateCSSIrisFile("FICHEIRIS", true);
    clearHeader("FICHEIRIS", "CSS");

    let PromiseCss = Promise.all([
        new Promise((resolve) =>
            resolve(addLinkScriptIrisBlack(`IRISBlack/Front/Scripts/components/${dir}/css/app.css`, "preload", "style", tag))
        ),
        new Promise((resolve) =>
            resolve(addCSSIrisBlack(`../../../IRISBlack/Front/Scripts/components/${dir}/css/app`, tag))
        ),
        new Promise((resolve) =>
            resolve(addCSSIrisBlack(`../../../IRISBlack/Front/Assets/CSS/${dir}/XrmInteract`, tag))
        ),
    ]);

    return PromiseCss;
}

import { store } from "../Scripts/store/store.js?ver=803000";
import { FieldType, EdnType } from "../Scripts/methods/Enum.min.js?ver=803000";
import eMailAdress from "../Scripts/components/formatChamps/eMailAdress.js?ver=803000";
import eCharacter from "../Scripts/components/formatChamps/eCharacter.js?ver=803000";
import eAutoComplete from "../Scripts/components/formatChamps/eAutoComplete.js?ver=803000";
import ePhone from "../Scripts/components/formatChamps/ePhone.js?ver=803000";
import eRelation from "../Scripts/components/formatChamps/eRelation.js?ver=803000";
import eSocialNetwork from "../Scripts/components/formatChamps/eSocialNetwork.js?ver=803000";
import eHyperLink from "../Scripts/components/formatChamps/eHyperLink.js?ver=803000";
import eFile from "../Scripts/components/formatChamps/eFile.js?ver=803000";
import eCatalog from "../Scripts/components/formatChamps/eCatalog.js?ver=803000";
import eGeolocation from "../Scripts/components/formatChamps/eGeolocation.js?ver=803000";
import eLogic from "../Scripts/components/formatChamps/eLogic.js?ver=803000";
import eDate from "../Scripts/components/formatChamps/eDate.js?ver=803000";
import eButton from "../Scripts/components/formatChamps/eButton.js?ver=803000";
import eMemo from "../Scripts/components/formatChamps/eMemo.js?ver=803000";
import eUser from "../Scripts/components/formatChamps/eUser.js?ver=803000";
import eNumeric from "../Scripts/components/formatChamps/eNumeric.js?ver=803000";
import eAutoCount from "../Scripts/components/formatChamps/eAutoCount.js?ver=803000";
import eMoney from "../Scripts/components/formatChamps/eMoney.js?ver=803000";
import eChart from "../Scripts/components/formatChamps/eChart.js?ver=803000";
import eImage from "../Scripts/components/formatChamps/eImage.js?ver=803000";
import ePassword from "../Scripts/components/formatChamps/ePassword.js?ver=803000";
import eWebPage from "../Scripts/components/formatChamps/eWebPage.js?ver=803000";
import eLabel from "../Scripts/components/formatChamps/elabel.js?ver=803000";
import eSeparator from "../Scripts/components/formatChamps/eSeparator.js?ver=803000";
import ePJ from "../Scripts/components/formatChamps/ePJ.js?ver=803000";

export {
    LoadJS,
    LoadCSS,
    loadJSTrans,
    loadCSSTrans,
    store,
    FieldType,
    EdnType,
    eMailAdress,
    eCharacter,
    eAutoComplete,
    ePhone,
    eRelation,
    eSocialNetwork,
    eHyperLink,
    eFile,
    eCatalog,
    eGeolocation,
    eLogic,
    eDate,
    eButton,
    eMemo,
    eUser,
    eNumeric,
    eAutoCount,
    eMoney,
    eChart,
    eImage,
    ePassword,
    eWebPage,
    eLabel,
    eSeparator,
    ePJ
};





