var singletonLink;
var singletonFileLayout;
var singletonFileDetail;
var singletonFileBookmark;
var singletonFileCatalog;
var singletonFileCreate;
var singletonFileUsers;
var singletonFileMenu;
var singletonPinnedBookmark;
var singletonStatusNewFile;
var singletonComputedValue;
var singletonMRU;
var singletonUploadFile;
var singletonUploadImage;

var eInitNewErgo = {
    currentTab: 0,
    currentFileId: 0
};

var irisHomeVue = null;

/**
 * Permet d'horodater une URL javascript.
 * @param {any} sUrl
 */
function AddUrlTimeStampJS(sUrl) {
    if (top._jsVer)
        return sUrl + '?ver=' + top._jsVer;

    return sUrl + '?ver=' + Date.now();
}


/**
 * On ajoute le menu de droite de manière asynchrone.
 * @param {any} nTab l'onglet
 * @param {any} nFileId l'element en cours d'affichage
 * @param {any} nType le type d'affichage.
 * @returns {Promise} une promesse sur le chargement du menu de droite.
 */
function AddMenuOnRight(nTab, nFileId, nType) {
    var bNonPopUp = !isPopup();
    var bIsFileInBkm = bNonPopUp && isBkmFile(nTab, nFileId);

    if (bIsFileInBkm)
        return false;

    try {
        if (bNonPopUp && nTab != 101000)
            loadFileMenu(nTab, nType, nFileId, InitNavigateButton);
        else
            InitNavigateButton();

    } catch (e) {
        return false;
    }
}

/**
 * Pour supprimer tous les éléments enfants d'une div. 
 * @param {HTMLElement} div la div à partir de laquelle on dégage tous les enfants.
 * */
function SuppressChildren(div) {
    if (div) {
        while (div.firstChild) {
            div.removeChild(div.firstChild);
        }
    }
};

/** Permet de supprimer tous les widgets qui se mettent 
 * en surimpression du CRM. 
 * */
function SupressWidgetsPanel() {

    var oGlobalWidget = document.querySelector("#gw-container");
    if (oGlobalWidget)
        SuppressChildren(oGlobalWidget);
}

/**
 * Singleton javascript à la papa pour permettre de gérer les données depuis l'appelant
 * et les transférer vers le projet transpilé.
 * */
var singleton = function () {
    var instance;

    function createInstance(url, params) {
        var link = { url, params };
        return link;
    }

    return {
        setInstance: function (url, params) {
            if (!instance) {
                instance = createInstance(url, params);
            }
            return instance;
        },
        getInstance: function () {
            return instance;
        }
    };
};


/**
 * retourne une promesse des données et de la structure de la page
 * promesse qui ne sera tenue que bien plus tard (comme toujours...)
 * @param {any} helper
 * @param {any} sLien
 */
function linkToCall(helper, params) {
    return helper.GetAsync({
        params: params,
        responseType: 'json'
    });
}

/**
 * Permet de déterminer suivant le résultat de l'appel au layout,
 * si on doit récupérer la wizard bar à afficher, et lance l'appel.
 * @param {any} params
 */
function checkCatalogWizard(params) {

    params.store.getters.getTkStructPage.results.then(layout => {

        if (typeof layout == "string")
            layout = JSON.parse(layout);

        let jsonWiz = layout.JsonWizardBar;
        if (typeof jsonWiz == "string")
            jsonWiz = JSON.parse(jsonWiz);

        let descid = jsonWiz?.DescId;

        if (descid > params.nTab && descid < params.nTab + 99) {
            params.store.commit("setTkStructCat", {
                link: new params.eAxiosHelper(params.sLien + '/api/catalogvalues'),
                params: {
                    descid: descid
                },
                results: linkToCall(new params.eAxiosHelper(params.sLien + '/api/catalogvalues'), {
                    descid: descid,
                })
            });
        }
    });
}

/**
 * Initialisation du store VueJs avec les numéros de l'onglet  et de la ligne à appeler.
 * @param {object} store le store VueX
 * @param  {int} nTab l'onglet (ou la table, plus) à appeler
 * @param  {int} nFileId l'élément à appeler.
 * @param  {DateTime} date de la demande (pour permettre aux templates de se rafraîchir si on demande les mêmes informations)
 * @param {object} eAxiosHelper
 */
async function loadStore(store, eAxiosHelper, nTab, nFileId, lastUpdate, path) {
    let { getUrl } = await import(AddUrlTimeStampJS(path + "../Scripts/shared/XRMWrapperModules.js"));
    var sLien = getUrl();

    store.commit("setFileId", nFileId);
    store.commit("setnTab", nTab);
    store.commit("setLastUpdate", lastUpdate);
    store.commit("setTkStructPage", {
        link: new eAxiosHelper(sLien + '/api/FileLayout'),
        params: {
            nTab: nTab
        },
        results: linkToCall(new eAxiosHelper(sLien + '/api/FileLayout'), {
            nTab: nTab
        })
    });

    store.commit("setTkDataPage", {
        link: new eAxiosHelper(sLien + '/api/detail'),
        params: {
            nTab: nTab,
            nFileId: nFileId
        },
        results: linkToCall(new eAxiosHelper(sLien + '/api/detail'), {
            nTab: nTab,
            nFileId: nFileId
        })
    });

    store.commit("setTkStructBkm", {
        link: new eAxiosHelper(sLien + '/api/bookmark'),
        params: {
            nTab: nTab,
            nFileId: nFileId
        },
        results: linkToCall(new eAxiosHelper(sLien + '/api/bookmark'), {
            nTab: nTab,
            nFileId: nFileId
        })
    });

    store.commit("setBkmPage", {});

    checkCatalogWizard({
        eAxiosHelper: eAxiosHelper,
        sLien: sLien,
        store: store,
        nTab: nTab
    });
};

/**
 * Prend une div et lui assigne tous les attibuts qui sont dans l'objet.
 * @param  {DOMElement} div l'onglet (ou la table, plus) à appeler
 * @param  {Object} obj l'élément à appeler.
 */
function initAttributesDiv(div, obj) {
    for (var key in obj) {
        div.setAttribute(key, obj[key]);
    }
};


/**
 * Actiev ou désactive les grilles.
 * @param {any} gwTab le composant des grilles.
 * @param {any} activation active / désactive (un string)
 */
function activateGrid(gwTab, activation) {
    if (gwTab) {
        gwTab.forEach((x, i) => {
            gwTab[i].setAttribute("active", activation);
            if (x.childNodes && x.childNodes.length > 0)
                gwTab[i].childNodes.forEach((nd) => {
                    nd.setAttribute("active", activation);
                    nd.removeAttribute("style");
                });
            else
                gwTab[i].remove();
        });
    }
};

/**
 * Chargement des JS et CSS du nouveau mode Fiche
 * @param {string} path
 */
async function LoadIrisJSCSS(path) {
    if (typeof (LoadJS) != "function" || typeof (LoadCSS) != "function")
        var { LoadJS, LoadCSS } = await import(AddUrlTimeStampJS(path + "LoadIrisScripts.js"));
    /** Ici le chargement des 2 fonctions pour le JS et le CSS du nouveau mode fiche. */
    await Promise.all([LoadJS(), LoadCSS()]);
    activateCSSIrisFile("FICHEIRIS", !isIris(nGlobalActiveTab));
}

/**
 * Met à jour le conteneur Fiche avec les nouvelles informations de la fiche demandée
 * Initialise l'environnement VueJS du nouveau mode Fiche s'il n'existe pas déjà
 * @param {any} eAxiosHelper Pour effectuer l'import du contexte VueJS si besoin. Doit être importé par le parent
 * @param {any} nTab DescID de l'onglet à afficher
 * @param {any} nFileId FileID du fichier à afficher
 * @param {string} path Chemin des fichiers à importer. Doit être donné par le parent
 */
async function LoadIrisFileApp(eAxiosHelper, nTab, nFileId, path) {
    var oFileDiv = document.querySelector(".fileDiv");

    // Si on a pas de FileDiv existant dans MainDiv avec un wrapper VueJS existant, c'est qu'on passe d'un contexte
    // autre que le nouveau mode Fiche (admin, E17...) au nouveau mode Fiche. Donc, dans ce cas, on vide le conteneur
    // principal et on recrée un nouveau FileDiv
    //if (!oFileDiv?.querySelector("#MainWrapper")) {
    var oMainDiv = document.querySelector(".mainDiv");
    SuppressChildren(oMainDiv);

    oFileDiv = document.createElement("div");
    oMainDiv.appendChild(oFileDiv);
    //}

    // On met à jour l'ID, les classes et les attributs de fileDiv avec les nouveaux FileId/DescID/type de contenu
    oFileDiv.id = "fileDiv_" + nTab.toString();
    oFileDiv.classList.add("fileDiv", "fs_8pt", "NewFiche");
    initAttributesDiv(oFileDiv, {
        "fid": nFileId.toString(),
        "did": nTab.toString(),
        "ftrdr": "3" // pour permettre à getCurrentView() d'identifier le mode Fiche
    });

    let { getUrl } = await import(AddUrlTimeStampJS(path + "../Scripts/shared/XRMWrapperModules.js"));

    // Chargement des singletons pour IRIS Black (nouveau mode Fiche)
    singletonFileDetail = singleton();
    singletonFileLayout = singleton();
    singletonFileCreate = singleton();
    singletonFileBookmark = singleton();
    singletonFileUsers = singleton();
    singletonFileCatalog = singleton();
    singletonFileMenu = singleton();
    singletonMRU = singleton();
    singletonComputedValue = singleton();
    singletonUploadFile = singleton();
    singletonPinnedBookmark = singleton();
    singletonStatusNewFile = singleton();

    singletonFileLayout.setInstance(getUrl() + '/api/FileLayout', {
        nTab: nTab,
        nFileId: 0
    });

    singletonFileDetail.setInstance(getUrl() + '/api/detail', {
        nTab: nTab,
        nFileId: nFileId
    });

    singletonFileCreate.setInstance(getUrl() + '/api/createupdate', {});
    singletonFileBookmark.setInstance(getUrl() + '/api/bookmark', {});

    singletonFileMenu.setInstance(getUrl() + '/api/FileMenu', {
        DescId: nTab,
        FileId: nFileId
    });

    singletonPinnedBookmark.setInstance(getUrl() + '/api/PinnedBookmark', {});
    singletonStatusNewFile.setInstance(getUrl() + '/api/EudonetXStatus', {});
    singletonFileUsers.setInstance(getUrl() + '/api/UserValues', {});
    singletonFileCatalog.setInstance(getUrl() + '/api/catalogvalues', {});
    singletonMRU.setInstance(getUrl() + '/api/Mru', {})
    singletonComputedValue.setInstance(getUrl() + '/api/ComputedValue', {});
    singletonUploadFile.setInstance(getUrl() + '/api/UploadFile', {});

    // On vérifie si FileDiv a déjà un wrapper et une app VueJS chargés. Si tel est le cas, inutile de le recréer
    if (!top.oNewFileMainVueJSApp) {
        // On charge le contexte JavaScript (notamment les composants)
        top.oNewFileMainVueJSApp = await import(AddUrlTimeStampJS(path + "../index.js"));

        if (!top.oNewFileMainVueJSApp) {
            throw top._res_8651;
        }
    }

    // if (!oFileDiv.querySelector("#MainWrapper") || !document.querySelector("#MainWrapper > #app")) {

    // On charge la page de base de l'application, avec le conteneur cible
    let info;

    let hlpBaseFile = new eAxiosHelper(path + "../index.html");
    info = await hlpBaseFile.GetAsync({ responseType: "document" });

    if (!info) {
        throw top._res_8651;
    }

    // On ajoute ce conteneur cible au FileDiv existant
    oFileDiv.append(info.querySelector('#mainContentWrapper *')); // ID du conteneur dans index.html
    //}

    // Et on lance l'initialisation de VueJS
    top.oNewFileMainVueJSApp.initVue();
}

/**
 * Chargement d'une fiche avec le nouveau format VueJS
 * Met à jour les infos sur la fiche demandée dans le Store et dans le DOM, puis charge le contexte VueJS s'il n'existe pas déjà
 * @param {any} nTab
 * @param {any} nFileId
 * @param {string} path
 */
function LoadIrisFile(nTab, nFileId, path) {
    var nType = 3;
    var iMaxChamps = 99;

    if (!path)
        path = "./";

    // US #4291 - TK #6962/6955/6966 - Pour info, on utilise toujours un skeleton au rechargement d'une fiche Nouveau Mode Fiche, même s'il s'agit de la même que précédemment.
    // Car son rechargement provoque toujours un rechargement des CSS, donc visible sans skeleton
    var oParamGoTabList = {
        to: nType,
        nTab: nTab,
        context: "eInitNewErgo.LoadIrisFile"
    }

    //Appel le waiter
    setWait(true, undefined, undefined, isIris(top.getTabFrom()), oParamGoTabList);

    //setWait(true);


    if (isNaN(nTab) || !nTab > 0) {
        //setWait(false);
        eAlert(0, top._res_416, top._res_6237, "undefined tab");
        return;
    }

    if (isNaN(nFileId) || !nFileId > 0) {
        //setWait(false);
        eAlert(0, top._res_416, top._res_6237, "undefined id");
        return;
    }

    // #TK 5030 - On réinitialise le conteneur de grilles de la page d'accueil
    // s'il est toujours présent
    //SuppressChildren(document.getElementById("gw-container"));
    let gwTab = document.querySelectorAll("[id^='gw-tab'], [id^='gw-bkm']");
    this.activateGrid(gwTab, "0");

    (async () => {

        clearHeader("FICHEIRIS", "CSS");
        let { LoadJS, LoadCSS } = await import(AddUrlTimeStampJS(path + "LoadIrisScripts.js"));
        /** Ici le chargement des 2 fonctions pour le JS et le CSS du nouveau mode fiche. */
        await Promise.all([LoadJS(), LoadCSS()]);

        try {
            let fileSkeleton = document.querySelector('.SktOn-file');
            if (!fileSkeleton) setWait(true, undefined, undefined, isIris(top.getTabFrom()), oParamGoTabList);
            const { default: eAxiosHelper } = await import(AddUrlTimeStampJS(path + "helpers/eAxiosHelper.js"));
            const { store } = await import(AddUrlTimeStampJS(path + "store/store.js"));
            let lastUpdate = new Date();
            await loadStore(store, eAxiosHelper, nTab, nFileId, lastUpdate, path);
            // Initialise le contexte VueJS, sauf s'il n'existe pas déjà
            LoadIrisFileApp(eAxiosHelper, nTab, nFileId, path);
            setWait(false, undefined, undefined, isIris(top.getTabFrom()), oParamGoTabList);
        }
        catch (e) {
            throw e;
        }

        try {
            // #36 771 - Et on sauvegarde la navigation vers le mode fiche en cours
            // #83 119 - La liste précédemment affichée sera, elle, sauvegardée juste après par loadFile() qui a appelé LoadIrisFile(). cf. eMain.js
            oNavManager.SaveMode('File', nTab, nFileId);

            // Utile pour le retour de l'Engine
            modeList = false;

        } catch (e) {
            throw e;
        }
    })();

    AddMenuOnRight(nTab, nFileId, nType);
}


/**
 * Chargement d'une liste avec le nouveau format VueJS
 * @param {any} nTab
 */
function LoadIrisFileNew(nTab, nFileId, path, oCtxInfos) {
    //setWait(true);
    switchActiveTab(nTab);

    let prjName = "IrisPurpleFile";
    let prjTag = "IrisFile";

    if (!path)
        path = "./";

    if (isNaN(nTab) || !nTab > 0) {
        // setWait(false);
        eAlert(0, top._res_416, top._res_6237, "undefined tab");
        return;
    }

    eInitNewErgo.currentTab = nTab;
    eInitNewErgo.oCtxInfos = oCtxInfos;
    eInitNewErgo.currentFileId = nFileId;

    // #TK 5030 - On réinitialise le conteneur de grilles de la page d'accueil
    // s'il est toujours présent
    //SuppressChildren(document.getElementById("gw-container"));
    let gwTab = document.querySelectorAll("[id^='gw-tab'], [id^='gw-bkm']");
    this.activateGrid(gwTab, "0");

    (async () => {

        let info;

        try {
            let { loadJSTrans, loadCSSTrans } = await import(AddUrlTimeStampJS(path + "LoadIrisScripts.js"));
            let { getUrl } = await import(AddUrlTimeStampJS(path + "../Scripts/shared/XRMWrapperModules.js"));

            // Chargement des singletons pour IRIS Purple (saisie guidée)
            singletonFileLayout = singleton();
            singletonFileDetail = singleton();
            singletonFileCatalog = singleton();
            singletonFileCreate = singleton();
            singletonFileUsers = singleton();
            singletonMRU = singleton();
            singletonUploadFile = singleton();
            singletonUploadImage = singleton();

            singletonFileLayout.setInstance(getUrl() + '/api/FileLayout', {
                nTab: nTab,
                nFileId: 0
            });

            singletonFileDetail.setInstance(getUrl() + '/api/detail', {
                ...oCtxInfos,
                nTab: nTab,
                nFileId: nFileId
            });

            singletonFileCatalog.setInstance(getUrl() + '/api/catalogvalues', {});
            singletonFileUsers.setInstance(getUrl() + '/api/UserValues', {});
            singletonFileCreate.setInstance(getUrl() + '/api/createupdate', {});
            singletonMRU.setInstance(getUrl() + '/api/mru', {});
            singletonUploadFile.setInstance(getUrl() + '/api/UploadFile', {});
            singletonUploadImage.setInstance(getUrl() + '/api/UploadImage', {});

            SupressWidgetsPanel();

            /** Ici le chargement des 2 fonctions pour le JS et le CSS du nouveau mode fiche. */
            await Promise.all([loadJSTrans(prjName, prjTag), loadCSSTrans(prjName, prjTag)]);

            var oFileContent = document.querySelector("#mainFileContent");
            var oMainDiv = document.querySelector("#mainDiv");
            oMainDiv.style.overflow = "auto";

            if (!oFileContent) {
                SuppressChildren(oMainDiv);

                oFileContent = document.createElement("div");
                oMainDiv.appendChild(oFileContent);
                oFileContent.id = "mainFileContent";
            }

            let mainFile = document.querySelector("#mainFileContent");
            if (mainFile) {
                SuppressChildren(mainFile);

                let dvApp = document.createElement("div");
                dvApp.id = "app";
                mainFile.appendChild(dvApp);

                /** Permet d'importer uniquement la fonction addScriptIrisBlackInTag depuis le module eFile.js */
                let { addScriptIrisBlackInTag } = await import(AddUrlTimeStampJS(path + "../Scripts/methods/eFile.js"));

                ["IRISBlack/Front/Scripts/components/IrisPurpleFile/js/app.js"]
                    .forEach(strHref => addScriptIrisBlackInTag(strHref, prjTag, mainFile));

                let menuPinned = getAttributeValue(document.getElementById("menuBar"), "pinned") == "1";

                if (menuPinned)
                    pinMenu(getAttributeValue(document.getElementById("menuPin"), "userid"));

                ocMenu(false, null);
            }
        }
        catch (e) {
            throw e;
        }
        finally {
            //setWait(false);
        }
    })();

}

/**
 * Chargement d'une liste avec le nouveau format VueJS
 * @param {any} nTab
 */
function LoadIrisList(nTab, path) {
    setWait(true);

    if (!path)
        path = "./";

    if (isNaN(nTab) || !nTab > 0) {
        setWait(false);
        eAlert(0, top._res_416, top._res_6237, "undefined tab");
        return;
    }

    (async () => {

        let info;

        try {
            let { loadJSTrans, loadCSSTrans } = await import(AddUrlTimeStampJS(path + "LoadIrisScripts.js"));
            let { getUrl } = await import(AddUrlTimeStampJS(path + "../Scripts/shared/XRMWrapperModules.js"));

            // Chargement des singletons pour IRIS Crimson (nouveau mode Liste)
            singletonLink = singleton();

            singletonLink.setInstance(getUrl() + '/api/detail', {
                nTab: nTab
            });


            /** Ici le chargement des 2 fonctions pour le JS et le CSS du nouveau mode fiche. */
            await Promise.all([loadJSTrans(), loadCSSTrans()]);

            var oListContent = document.querySelector("#mainListContent");
            var oMainDiv = document.querySelector("#mainDiv");
            oMainDiv.style.overflow = "auto";

            if (!oListContent) {
                SuppressChildren(oMainDiv);

                oListContent = document.createElement("div");
                oMainDiv.appendChild(oListContent);
                oListContent.id = "mainListContent";
            }

            let mainList = document.querySelector("#mainListContent");
            if (mainList) {
                SuppressChildren(mainList);

                let dvApp = document.createElement("div");
                dvApp.id = "app";
                mainList.appendChild(dvApp);

                /** Permet d'importer uniquement la fonction addScriptIrisBlackInTag depuis le module eFile.js */
                let { addScriptIrisBlackInTag } = await import(AddUrlTimeStampJS(path + "../Scripts/methods/eFile.js"));

                ["IRISBlack/Front/Scripts/components/IrisCrimsonList/js/app.js"]
                    .forEach(strHref => addScriptIrisBlackInTag(strHref, "LISTIRIS", mainList));

            }
        }
        catch (e) {
            throw e;
        }
        finally {
            setWait(false);
        }
    })();

}

/**
 * Chargement de la home en VueJs
  */
function LoadIrisHome(oProps) {
    setWait(true);
    (async () => {
        try {
            /** Ici le chargement des 2 fonctions pour le JS et le CSS de la page home. */
            clearHeader("FICHEIRIS", "CSS");
            let { LoadJS, LoadCSS } = await import(AddUrlTimeStampJS("./components/ednHome/appLoader/LoadIrisScripts.js"));
            await Promise.all([LoadJS(), LoadCSS()]);

            /** On crée les div pour pouvoir initialiser Vue dessus */
            var oMainDiv = document.body;
            var oDialogDiv = document.createElement("div");
            oDialogDiv.id = "app_edn_home";
            oDialogDiv.className = "edn--home";

            /** On importe le fichier main.js qui initialize vue et les components */
            let mainJS = await import(AddUrlTimeStampJS("./components/ednHome/appLoader/main.js"));

            if (!mainJS) {
                throw top._res_8651;
            }

            /** On insert la div (oDialogDiv) comme premier éléments dans le body (important pour l'ordre d'affichage du html) */
            oMainDiv.insertBefore(oDialogDiv, document.body.firstChild);

            /** On initialise mainJS (vue) */
            irisHomeVue = mainJS.initVue(oProps);
        }
        catch (e) {
            throw e;
        }
        finally {
            setWait(false);
        }
    })();
}


/**
 * Chargement de la home en VueJs
  */
function LoadIrisSubMenuAdmin(path, oProps) {

    if (!path)
        path = "./";

    (async () => {
        try {
            /** Ici le chargement des 2 fonctions pour le JS et le CSS de la page home. */
            clearHeader("FICHEIRIS", "CSS");
            let { LoadJS, LoadCSS } = await import(AddUrlTimeStampJS("./components/ednHome/appLoader/LoadIrisScripts.js"));
            let { getUrl } = await import(AddUrlTimeStampJS(path + "../Scripts/shared/XRMWrapperModules.js"));

            await Promise.all([LoadJS(), LoadCSS()]);

            singletonStatusNewFile = singleton();
            singletonStatusNewFile.setInstance(getUrl() + '/api/EudonetXStatus', {});

            /** On crée les div pour pouvoir initialiser Vue dessus */
            var oMainDiv = document.getElementById("app-menu");

            if (!oMainDiv) {
                oMainDiv = document.createElement("div");
                oMainDiv.id = "app-menu";
            }

            oMainDiv.innerHTML = "";

            //SuppressChildren(oMainDiv);
            document.body.insertBefore(oMainDiv, document.body.firstChild);

            /** On importe le fichier main.js qui initialize vue et les components */
            let mainJS = await import(AddUrlTimeStampJS("./components/ednHome/Admin/eMainAdmin.js"));

            if (!mainJS) {
                throw top._res_8651;
            }

            if (top.irisHomeAdminVue)
                top.irisHomeAdminVue.$destroy();

            /** On initialise mainJS (vue) */
            top.irisHomeAdminVue = mainJS.initVue(oProps);
        }
        catch (e) {
        throw e;
    }
}) ();
}

function DestroyIrisHome() {
    var ednHomeWrapper = document.getElementById("ednHomeWrapper");
    if (irisHomeVue && ednHomeWrapper) {
        irisHomeVue.$destroy();
        document.body.removeChild(document.getElementById("ednHomeWrapper"));
    }
}