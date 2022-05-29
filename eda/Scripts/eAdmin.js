//Modal utilisées par l'admin
var modalRights;
var modalTreatmentRights;
var modalIP;
var modalIPCat;
var modalIPCatTreatment;
var modalUserCat;
var modalUserCatTreatment;
var modalAdvSearch;
var modalRGPDConditions;
var FIELD_DID = 0;
var iaf = 0;
var RGPDRuleType = { ARCHIVING: 0, DELETING: 1, PSEUDONYM: 2, PJDELETING: 3 };
var MiniFileType = { Undefined: -1, File: 0, Kanban: 1 };
var enumPages = { XRMHOMEPAGE: 115200 }

// NameSpace pour les fonctions propre à l'admin
var nsAdmin = nsAdmin || {
    AdminMode: false,
    CalledCheck: false
};

if (!top.nsAdmin) {

    top.nsAdmin = {
        AdminMode: false,
        CalledCheck: false
    };

}

function Capsule(d) {

    this.DescId = d;
    this.ListProperties = new Array();
    this.Confirmed = true;

    this.AddProperty = function (cat, prop, val, advCat, bConfirmed, userid, bForceAdd) {
        if (typeof (cat) == "undefined" || cat === "")
            return;
        if (typeof (bForceAdd) == "undefined" || cat === "")
            bForceAdd = false;

        var capProp = new CapsuleProperties(cat, prop, val, advCat, bConfirmed, userid)

        // Si bForceAdd = true, on force l'ajout de la propriété même si elle existe déjà dans la capsule
        if (!bForceAdd) {
            var index = findPropertyIndex(this.ListProperties, cat, prop);
            if (index != null) {
                this.ListProperties[index] = capProp;
            }
            else {
                this.ListProperties.push(capProp);
            }
        }
        else {
            this.ListProperties.push(capProp);
        }
    }

    this.SetCapsuleConfirmed = function (bConfirmed) {
        this.Confirmed = bConfirmed;
    }

    function findPropertyIndex(listPties, cat, prop) {
        for (var i = 0; i < listPties.length; i++) {
            if (listPties[i].Category == cat && listPties[i].Property == prop)
                return i;
        }
        return null;
    }
}

function CapsuleProperties(cat, prop, val, advCat, bConfirmed, userid) {

    this.Category = cat;
    this.Property = prop;
    this.Value = val;
    this.CAdvCategory = advCat;

    if (typeof (bConfirmed) === 'undefined') bConfirmed = true;

    this.Confirmed = bConfirmed;

    this.UserId = userid;
}

nsAdmin.ModeDebug = true;


nsAdmin.UpdateProductStatus = function (elem) {

    var current = getAttributeValue(elem, "ednval")
    if (current == "1")
        setAttributeValue(elem, "ednval", 0);
    else
        setAttributeValue(elem, "ednval", 1);

    current = getAttributeValue(elem, "ednval")
    var descId = 0;
    if (elem) {
        var parrentnode = elem.parentNode;
        descId = getAttributeValue(parrentnode, "did");
    }
    var caps = new Capsule(0);
    caps.DescId = descId;
    caps.AddProperty('16', '56', current);

    var json = JSON.stringify(caps);
    var upd = new eUpdater("eda/Mgr/eAdminDescManager.ashx", 1);

    upd.json = json;
    upd.ErrorCallBack = function () { alert('La mise à jour du Status de produit a échoué'); };

    upd.send(function (oRes) {
        var res = JSON.parse(oRes);

        if (!res.Success) {
            top.eAlert(1, top._res_416, res.UserErrorMessage);
        }
    });
}


//récuppère la propriété d'une liste
nsAdmin.findProperty = function (listPties, cat, prop) {

    var found = listPties.filter(function (ppty) { return ppty.Category == cat && ppty.Property == prop });

    if (found.length > 0)
        return found[0];

    return null;
}


nsAdmin.loadSliders = function (noUiSlider, sliders, onSetFct, onSlideFct) {
    if (noUiSlider) {

        try {
            var step, max, value, bIsLevelSlider = false;

            for (var i = 0; i < sliders.length; i++) {

                slider = sliders[i];

                step = slider.getAttribute("data-step");
                max = getNumber(slider.getAttribute("data-max"));
                value = getNumber(slider.getAttribute("data-value"))

                noUiSlider.create(slider, {
                    range: {
                        'min': (slider.getAttribute("data-min") != "") ? getNumber(slider.getAttribute("data-min")) : 0,
                        'max': max
                    },
                    connect: [true, false],
                    step: Number(step),
                    start: value
                });

                if (typeof onSetFct === "function")
                    slider.noUiSlider.on("set", function () {
                        onSetFct(this);
                    });

                if (typeof onSlideFct === "function")
                    slider.noUiSlider.on("slide", function () {
                        onSlideFct(this);
                    });

                // Quand la valeur est sur max, on exécute la méthode "set" pour exécuter la fonction associée pour griser la barre
                if (max == value) {
                    slider.noUiSlider.set(value);
                }


            }
        }
        catch (err) {
            console.log(err.message);
        }
    }

}

nsAdmin.loadPerfSliders = function () {
    var oParamGoTabList = {
        to: 7,
        nTab: nsAdmin.tab,
        context: "eAdmin.loadPerfSliders"
    }

    top.setWait(true, undefined, undefined, isIris(top.getTabFrom()), oParamGoTabList);

    var slider, sliders, frameContent = document;

    sliders = document.getElementsByClassName('nouislider');

    nsAdmin.loadSliders(noUiSlider, sliders, function (slider) {
        nsAdmin.perfSliderOnChange(slider);
    }, null);

    // close the skeleton from iris
    top.setWait(false, undefined, undefined, isIris(top.getTabFrom()), oParamGoTabList);
}



nsAdmin.tryLoadAdminResizer = function () {
    if (nsAdmin.menuUpdated && nsAdmin.navbarUpdated && nsAdmin.contentUpdated) {
        try {
            var resizer = new nsAdminResizer.AdminResizer(); // TODO: Passer en paramètre les largeurs définies en base
            resizer.setStopDragCallback(function () {
                var width = resizer.getListColWidth();
                nsAdmin.updateListColWidth(width);
            });
            resizer.setChangeColCallback(function () {
                var width = resizer.getListColWidth();
                nsAdmin.updateListColWidth(width);


                nsAdmin.loadContentModule(USROPT_MODULE_ADMIN_TAB, null, { tab: nGlobalActiveTab })

            })
        }
        catch (ex) {
        }
    }
}

/** Pour les admins, on purge IndexedDB. 
 * Ca leur permet de faire des modifications et de les tester. */
nsAdmin.purgeIndexedDB = function () {
    window.indexedDB = window.indexedDB || window.mozIndexedDB || window.webkitIndexedDB || window.msIndexedDB;

    if (!window.indexedDB)
        return;

    if (window.indexedDB.databases) {
        try {
            window.indexedDB.databases().then(function (t) {
                t.forEach(function (n) {
                    window.indexedDB.deleteDatabase(n.name)
                })
            });

            return;

        } catch (e) {
            eAlert(0, "", e.message);
        }
    }

    var oeParam = getParamWindow();
    var sGetBaseName = "";

    if (typeof (oeParam.GetParam) != "undefined" && oeParam.GetParam('sBaseName') != '')
        sGetBaseName = oeParam.GetParam('sBaseName');

    var nmDb = ["IrisPurple_" + window.location.hostname + "_" + sGetBaseName + "_" + nGlobalCurrentUserid,
    "IrisPurple_" + window.location.hostname + "_undefined_" + nGlobalCurrentUserid];

    try {
        nmDb.forEach(function (db) { window.indexedDB.deleteDatabase(db) });
    } catch (e) {
        eAlert(0, "", e.message);
    }
}

/** purge des boutons admin. */
nsAdmin.purgeAdminVue = function () {

    /** On crée les div pour pouvoir initialiser Vue dessus */
    var oMainDiv = document.getElementById("app-menu");

    if (!oMainDiv) {
        oMainDiv = document.createElement("div");
        oMainDiv.id = "app-menu";
    }

    oMainDiv.innerHTML = "";

    clearHeader("EDNHOME", "CSS");

    /****** La visu de l'admin *****/
    if (top.irisHomeAdminVue)
        top.irisHomeAdminVue.$destroy();
}

//Chargement admin mode fiche
// le param sPartId n'est a priori jamais utilisé.
nsAdmin.loadAdminFile = function (nTab, sPartId) {
    let oParamGoTabList = {
        to: 7,
        nTab: nTab,
        context: "eAdmin.loadAdminFile"
    }
    top.setWait(true, undefined, undefined, isIris(top.getTabFrom()), oParamGoTabList);

    nsAdmin.purgeAdminVue();
    nsAdmin.purgeIndexedDB();

    nsAdmin.loadAdminModule(USROPT_MODULE_ADMIN_TAB, null, { tab: nTab, PartId: sPartId });

    if (top.oGridManager) {
        top.oGridManager.addTabToStack(nTab);
    }

    top.setWait(false, undefined, undefined, isIris(top.getTabFrom()), oParamGoTabList);
}

nsAdmin.switchAdminModeMode = function (oParam) {
    var upd = new eUpdater("eda/mgr/eAdminAccessPrefManager.ashx", 1);
    upd.addParam("action", "connect", "post");
    upd.send(function (sRes) {

        try {
            var oRes = JSON.parse(sRes);
            top.nsAdmin.AdminMode = oRes.Success;


            if (top.nsAdmin.AdminMode) {
                nsAdmin.loadAdminModule(oParam.module, oParam.anchor, oParam.moduleparam);
                GridSystem.adminMode();
                //SHA : backlog #1 588
                hideSwitchNewTheme();
                //SHA : backlog #1 657
                hideSpanImgNewTheme();
            }

        }
        catch (e) {

        }

    });


    upd.ErrorCallBack = function () {
    }
}

//SHA : backlog #1 588
function hideSwitchNewTheme() {
    var switchNewThemeWrap = document.getElementsByClassName('switch-new-theme-wrap')[0];
    if (typeof (switchNewThemeWrap) !== 'undefined' && switchNewThemeWrap != null && top.nsAdmin.AdminMode)
        switchNewThemeWrap.style.visibility = top.nsAdmin.AdminMode ? 'hidden' : 'visible';
}

//SHA : backlog #1 657
function hideSpanImgNewTheme() {
    var spanNewTheme = document.getElementsByClassName('switch-new-theme-spn')[0];
    var imgNewTheme = document.getElementsByClassName('switch-new-theme-img')[0];
    if (typeof (spanNewTheme) !== 'undefined' && spanNewTheme != null && top.nsAdmin.AdminMode)
        spanNewTheme.style.visibility = top.nsAdmin.AdminMode ? 'hidden' : 'visible';
    if (typeof (imgNewTheme) !== 'undefined' && imgNewTheme != null && top.nsAdmin.AdminMode)
        imgNewTheme.style.visibility = top.nsAdmin.AdminMode ? 'hidden' : 'visible';
}

nsAdmin.onUnloadBrowser = function (event) {
    event.preventDefault();
    nsAdmin.switchUserMode(undefined, 0, 0);
}

//Charge la page d'admin issue d'un module (module + menu module + navbar)
nsAdmin.loadAdminModule = function (module, anchor, moduleparam) {

    nsAdmin.purgeAdminVue();
    nsAdmin.purgeIndexedDB();

    window.onbeforeunload = nsAdmin.onUnloadBrowser;

    var reloadNavBar = true;
    var reloadMenu = true;
    if (!top.nsAdmin.CalledCheck) {

        var paramCallBack = {};
        paramCallBack.module = module;
        paramCallBack.anchor = anchor;
        paramCallBack.moduleparam = moduleparam;
        top.nsAdmin.CalledCheck = true;

        nsAdmin.switchAdminModeMode(paramCallBack);

        return;
    }

    if (!top.nsAdmin.AdminMode)
        return;
    if (typeof moduleparam == "undefined")
        moduleparam = { tab: 0 };

    // En fonction du module, utilisation d'une fonction de chargement spécifique, ou appel des fonctions génériques
    switch (module) {

        case USROPT_MODULE_ADMIN_TAB_USER:
            moduleparam.tab = 101000;


            if (!nsAdminUsers.hasOwnProperty("ListUserId")) {
                nsAdminUsers.ListUserId = {
                    Page: -1,
                    ListId: []
                }
            };

            nsAdminUsers.loadUsersId(nsAdminUsers.ListUserId.Page);

            nsAdmin.tab = moduleparam.tab;
            nsMain.SetGlobalActiveTab(nsAdmin.tab);

            break;
        case USROPT_MODULE_ADMIN_HOME:
        case USROPT_MODULE_ADMIN_HOME_XRM_HOMEPAGE:
        case USROPT_MODULE_ADMIN_HOME_XRM_HOMEPAGES:
            nsAdmin.tab = 115200;
            moduleparam.tab = 115200;

            //nsMain.SetGlobalActiveTab(nsAdmin.tab);
            break;
        case USROPT_MODULE_ADMIN_ACCESS:
        case USROPT_MODULE_ADMIN_ACCESS_USERGROUPS:
            moduleparam.tab = 101000;
            nsAdmin.tab = moduleparam.tab;
            break;
        case USROPT_MODULE_ADMIN_DASHBOARD_RGPDTREATMENTLOG:
            moduleparam.tab = 117000;
            nsAdmin.tab = moduleparam.tab;

            break;
        case USROPT_MODULE_ADMIN_HOME_XRM_HOMEPAGE:
        case USROPT_MODULE_ADMIN_TAB_GRID:
        case USROPT_MODULE_ADMIN_TAB:
            nsAdmin.tab = moduleparam.tab;

            nsAdmin.menuUpdated = false;
            nsAdmin.navbarUpdated = false;
            nsAdmin.contentUpdated = false;

            nsMain.SetGlobalActiveTab(nsAdmin.tab);

            break;
        case USROPT_MODULE_ADMIN_EXTENSIONS:

            if (moduleparam.notReloadMenu)
                reloadMenu = false;
            if (moduleparam.notReloadNavBar)
                reloadNavBar = false;
            break;
        default:
            nsAdmin.tab = 0;
            break;

    }
    //SHA : #73 223
    if (typeof (module) !== 'undefined') {

        if (module.indexOf("ADMIN_EXTENSION") < 0) {
            nsAdmin.loadAdminCss();
            nsAdmin.loadAdminJs();
            //JS/CSS spécifique
            nsAdmin.loadModuleCssScripts(module);
        }
    }

    //Charge le menu du module
    if (reloadMenu)
        nsAdmin.loadMenuModule(module, moduleparam);

    //Charge la navbar du module
    if (!moduleparam.hasOwnProperty("tab"))
        moduleparam.tab = 0;

    if (!moduleparam.hasOwnProperty("bkm"))
        moduleparam.bkm = 0;

    if (!moduleparam.hasOwnProperty("extensionFileId")) {
        if (document.getElementById("extensionFileId"))
            moduleparam.extensionFileId = getNumber(document.getElementById("extensionFileId").value);
        else
            moduleparam.extensionFileId = 0;
    }

    // 56 608 - Si le FileID et le libellé de l'extension (extraits depuis la connexion à l'EudoStore via l'API) n'ont pas été transmis,
    // on tente de les extraire depuis les champs cachés ajoutés sur la page par eAdminNavBarRenderer
    if (!moduleparam.hasOwnProperty("extensionLabel")) {
        if (document.getElementById("extensionLabel")) {
            moduleparam.extensionLabel = document.getElementById("extensionLabel").value;
        }
        else
            moduleparam.extensionLabel = "";
    }

    if (!moduleparam.hasOwnProperty("extensionCode")) {
        if (document.getElementById("extensionCode")) {
            moduleparam.extensionCode = document.getElementById("extensionCode").value;
        }
        else
            moduleparam.extensionCode = "";
    }

    //charge le contenu du module
    nsAdmin.loadContentModule(module, anchor, moduleparam);

    if (reloadNavBar)
        nsAdmin.loadNavBar(module, moduleparam);

    // On ferme la carto pour l'admin
    oEvent.fire("load-admin");

    //clearHeader("FICHEIRIS", "ALL");
    activateCSSIrisFile("FICHEIRIS", true);
    clearHeader("FICHEIRIS", "CSS");
    clearHeader("LISTIRIS", "ALL");
    clearHeader("THEMEBASE", "CSS");
}



///Charge les scripts / css propre au module
nsAdmin.loadModuleCssScripts = function (module) {
    if (module == USROPT_MODULE_ADMIN_TABS || module.indexOf(USROPT_MODULE_ADMIN_HOME) != -1) {

        addCss("eList", "ADMINLIST");

        if (module.indexOf(USROPT_MODULE_ADMIN_HOME) != -1) {
            nsAdmin.addScript("eAdminHomepages", "ADMIN");
            addScript("ckeditor/ckeditor", "ADMIN");
            addScript("ePopup", "ADMIN");
            addScript("eGrapesJSEditor", "ADMIN");
            addScript("eMemoEditor", "ADMIN");
            /* Admin des grilles */
            nsAdmin.addScript("grid/eAdminGridSubMenu", "ADMINFILE");
            nsAdmin.addScript("grid/eAdminWidget", "ADMINFILE");
            nsAdmin.addScript("grid/eAdminWidgetToolbar", "ADMINFILE");
            nsAdmin.addScript("grid/eAdminWidgetPrototype", "ADMINFILE");
            nsAdmin.addScript("grid/eAdminGridMenu", "ADMINFILE");
            nsAdmin.addScript("grid/eAdminGrid", "ADMINFILE");
        }
    }

    if (module === USROPT_MODULE_ADMIN_ACCESS_PREF) {
        nsAdmin.addScript("eAdminPref", "ADMINPREF");
    }

    //SHA : #73 223
    if (module.indexOf("ADMIN_EXTENSION") == 0) {
        if (module == USROPT_MODULE_ADMIN_EXTENSIONS)
            addCss("eAdminStoreList", "ADMINSTOREFILE");
        else
            addCss("store/eAdminStoreFile", "ADMINSTOREFILE");

        addScript("store/eStoreFile", "ADMINSTOREFILE");
    }

    if (module.indexOf("ADMIN_DASHBOARD") == 0) {
        addCss("eAdminDashboard", "ADMIN");
        nsAdmin.addScript("eAdminDashboard", "ADMIN");

        addCss("odometer/odometer", "ADMIN");
        nsAdmin.addScript("odometer/odometer", "ADMIN");

        // Chargement des dépendances
        addScript("https://cdn.syncfusion.com/ej2/dist/ej2.min.js", "ADMIN");


    }

    if (module == USROPT_MODULE_ADMIN_ACCESS_USERGROUPS || module == USROPT_MODULE_ADMIN_ACCESS) {
        addCss("eCatalog", "ADMINUSERLIST");
        addCss("eControl", "ADMINUSERLIST");
        addCss("eAdminList", "ADMINUSERLIST");

        addScript("eCatalog", "ADMINUSERLIST");
        addScript("eCatalogUser", "ADMINUSERLIST");
        addScript("eTreeView", "ADMINUSERLIST");

        //Script ADMIN
        nsAdmin.addScript("eAdminUsers", "ADMINUSERLIST");
    }
    else if (module == USROPT_MODULE_ADMIN_TAB) {
        addCss("eEditFile", "ADMINFILE");
        addCss("eFile", "ADMINFILE");
        addCss("eAdminFile", "ADMINFILE");


        addScript("eColorPicker", "ADMINFILE");
        addScript("eFile", "ADMINFILE");
        addScript("eAdminFile", "ADMINFILE");
        addScript("eAdminResizer", "ADMINFILE");
        addScript("ckeditor/ckeditor", "ADMINFILE");
        addScript("eGrapesJSEditor", "ADMINFILE");
        addScript("eMemoEditor", "ADMINFILE");

        /* Admin des grilles */
        nsAdmin.addScript("grid/eAdminGridSubMenu", "ADMINFILE");
        nsAdmin.addScript("grid/eAdminWidget", "ADMINFILE");
        nsAdmin.addScript("grid/eAdminWidgetToolbar", "ADMINFILE");
        nsAdmin.addScript("grid/eAdminWidgetPrototype", "ADMINFILE");
        nsAdmin.addScript("grid/eAdminGridMenu", "ADMINFILE");
        nsAdmin.addScript("grid/eAdminGrid", "ADMINFILE");
    }
    else if (module == USROPT_MODULE_ADMIN_TAB_USER) {
        var tabScript = new Array();

        tabScript.push("eFile");
        tabScript.push("eAutoCompletion");
        tabScript.push("ePopup");
        tabScript.push("eFieldEditor");
        tabScript.push("eMemoEditor");
        tabScript.push("ckeditor/ckeditor");

        top.addCss("eFile", "ADMINUSERFILE");
        top.addCss("eEditFile", "ADMINUSERFILE");
        top.addScripts(tabScript, "ADMINUSERFILE");

        nsAdmin.addScript("eAdminUsers");

        setTimeout(function () { top.removeCSS("eAdmin") }, 100);

        top.clearHeader("FILE");
        top.clearHeader("LIST");
        top.clearHeader("HOMEPAGE");
    }
    else if (module.indexOf(USROPT_MODULE_ADMIN_EXTENSIONS) != -1) {
        addCss("eFile", "ADMINFILE");
        addCss("eAdminFile", "ADMINFILE");
        if (module == USROPT_MODULE_ADMIN_EXTENSIONS)
            addCss("eAdminStoreList", "ADMINSTOREFILE");
        else
            addCss("store/eAdminStoreFile", "ADMINSTOREFILE");

        addScript("eFile", "ADMINFILE");
        addScript("eAdminFile", "ADMINFILE");
        addScript("carousel.min", "ADMIN");
    }
    else if (module == USROPT_MODULE_ADMIN_HOME_XRM_HOMEPAGES) {
        addCss("eAdminFile", "ADMINFILE");

        /* Admin des grilles */
        nsAdmin.addScript("grid/eAdminGridSubMenu", "ADMINFILE");
        nsAdmin.addScript("grid/eAdminWidget", "ADMINFILE");
        nsAdmin.addScript("grid/eAdminWidgetToolbar", "ADMINFILE");
        nsAdmin.addScript("grid/eAdminWidgetPrototype", "ADMINFILE");
        nsAdmin.addScript("grid/eAdminGridMenu", "ADMINFILE");
        nsAdmin.addScript("grid/eAdminGrid", "ADMINFILE");
    }
    else {
        addCss("eFile", "ADMINFILE");
        addCss("eAdminFile", "ADMINFILE");

        addScript("eFile", "ADMINFILE");
        addScript("eAdminFile", "ADMINFILE");
        addScript("eAdminResizer", "ADMINFILE");
    }
}

//Ajoute les css propre à l'admin
nsAdmin.loadAdminCss = function () {
    //Clear les anciens css
    nsAdmin.clearAdminScript("CSS");

    addCss("eAdmin", "ADMIN");
    addCss("eAdminMenu", "ADMIN");
    addCss("eAdminExtensions", "ADMIN");
    addCss("nouislider", "ADMIN");
}

///Charge les JS "basique" de l'admin
nsAdmin.loadAdminJs = function () {
    nsAdmin.clearAdminScript("JS");

    nsAdmin.addScript("eAdminEnum", "ADMIN");
    nsAdmin.addScript("eAdminPicto", "ADMIN");
    nsAdmin.addScript("noUiSlider.min", "ADMIN", function () { nsAdmin.loadPerfSliders(); });
}

/****************  MENU DROITE  ****************/
//Menu spécifique User - appel manager
nsAdmin.loadMenuUsers = function (nTab, module, moduleParam) {
    //eda / mgr / eAdminFileMenuManager.ashx
    if (module == USROPT_MODULE_ADMIN_TAB_USER)
        top.loadFileMenu(nTab, 3, moduleParam.userid, '', module); // mode modif
    else
        top.loadFileMenu(nTab, 1, moduleParam.userid, '', module); // mode liste
}

//Menu Module - Générique - appel manager
nsAdmin.loadMenuModule = function (module, moduleParam) {
    // paramètre globaux
    var menuManager = "eda/mgr/eAdminFileMenuManager.ashx";
    var aParam = [];


    var winSize = getWindowSize();

    // Si on revient vers l'accueil, on n'epingle pas le menu, on prend toute la largeur
    if (nGlobalActiveTab == 0)
        winSize.dw = winSize.w;

    aParam.push({ key: "module", value: getUserOptionsModuleHashCode(module), meth: "post" });
    aParam.push({ key: "H", value: winSize.h, meth: "post" });
    aParam.push({ key: "W", value: winSize.dw, meth: "post" });


    // En fonction du module, utilisation d'une fonction de chargement spécifique, ou appel des fonctions génériques
    switch (module) {

        case USROPT_MODULE_ADMIN_TAB:
            nsAdmin.tab = moduleParam.tab;
            menuManager = "eda/mgr/eAdminMainMenuManager.ashx";

            aParam.push({ key: "tab", value: nsAdmin.tab, meth: "post" });

            var sOpenedBlocks = '';
            //if (moduleParam.OpenedBlock) // a priori, on réouvre systématiquement les blocs ouverts ce param n'est plus utilisé
            {
                // On dresse la liste des blocs ouverts par l'utilisateur, pour les afficher de nouveau dépliés après chargement du menu
                var aOpenedBlocks = document.querySelectorAll("div[class^='paramPartContent'][data-active='1']");
                for (var i = 0; i < aOpenedBlocks.length; i++) {
                    var parentDiv = aOpenedBlocks[i].parentNode || aOpenedBlocks[i].parentElement;
                    if (parentDiv && parentDiv.id)
                        sOpenedBlocks += parentDiv.id + ';';
                }
            }

            aParam.push({ key: "openedblocks", value: sOpenedBlocks, meth: "post" });


            break;
        case USROPT_MODULE_ADMIN_ACCESS_USERGROUPS:
        case USROPT_MODULE_ADMIN_ACCESS:

            //cas particulier : on charge un menu "hybride"

            nsAdmin.tab = 101000;
            nsAdmin.loadMenuUsers(nsAdmin.tab, USROPT_MODULE_ADMIN_ACCESS_USERGROUPS, moduleParam);


            return;
            break;
        case USROPT_MODULE_ADMIN_TAB_USER:

            //cas particulier : on charge un menu "hybride"

            nsAdmin.tab = 101000;
            nsAdmin.loadMenuUsers(nsAdmin.tab, USROPT_MODULE_ADMIN_TAB_USER, moduleParam);


            return;
            break;

        case USROPT_MODULE_ADMIN_DASHBOARD_RGPDTREATMENTLOG:
            // Mode liste
            top.loadFileMenu(117000, 1, moduleParam.userid, '', module);
            return;
            break;
        default:

            aParam.push({ key: "tab", value: 0, meth: "post" });
            aParam.push({ key: "type", value: 7, meth: "post" });
            aParam.push({ key: "fileid", value: 0, meth: "post" });
            aParam.push({ key: "xsltserver", value: 1, meth: "post" });

            break;
    }

    var callbackFunction;

    //Appel le waiter
    var oParamGoTabList = {
        to: 7,
        nTab: moduleParam.tab,
        context: "eAdmin.loadMenuModule"
    }
    top.setWait(true, undefined, undefined, isIris(top.getTabFrom()), oParamGoTabList);

    top.bIsNavBarLoaded = 0;


    // Les modules utilisent un menu standard de l'application géré par XSLT
    var oMenuUpdater = new eUpdater(menuManager, 1);

    oMenuUpdater.ErrorCallBack = function () { setWait(false); };
    //oMenuUpdater.asyncFlag = false;


    //Param Spécifique
    aParam.forEach(function (p) {
        oMenuUpdater.addParam(p.key, p.value, p.meth);
    });
    oMenuUpdater.send(nsAdmin.updateMenuModule, callbackFunction);
}

///Met à jour le dom a partir du retour du LoadMenuModule 
nsAdmin.updateMenuModule = function (oRes, callback) {
    // close the new skeleton
    var oParamGoTabList = {
        to: 7,
        nTab: nsAdmin.tab,
        context: "eAdmin.updateMenuModule"
    }
    top.setWait(false, undefined, undefined, isIris(top.getTabFrom()), oParamGoTabList);

    // On commence par vider Gérard, le conteneur du menu actuel, après avoir pris soin de récupérer l'encart avec le nom d'utilisateur
    var bIsTabsMenu = oRes.indexOf('ul id="navTabs"') != -1; // identifie si le menu à afficher est celui de l'administration d'un onglet
    var oMainDiv = document.getElementById("Gerard");

    var omenuBar = document.getElementById("menuBar");
    setAttributeValue(menuBar, "adminmode", 1);

    var oUser = oMainDiv.querySelector(".encartProfil").cloneNode(true);

    // On ajoute une classe supplémentaire sur l'encart utilisateur selon la page d'administration que l'on affiche
    if (!bIsTabsMenu && oUser.className == "encartProfil")
        oUser.className = "encartProfil encartProfilH";
    else if (bIsTabsMenu && oUser.className == "encartProfil encartProfilH")
        oUser.className = "encartProfil";
    oMainDiv.innerHTML = "";
    oMainDiv.appendChild(oUser);

    // Puis on incorpore le nouveau contenu dans un div temporaire pour filtrage et analyse
    var oDiv = document.createElement("div");
    oDiv.innerHTML = oRes;

    // Par défaut, on considère que le premier noeud renvoyé contient le contenu à injecter...
    var monContent = oDiv.firstChild;
    // ... Sauf si on traite un menu issu de eFileMenuManager, qui renvoie tout le menu, dont la punaise et Gérard.
    // Dans ce cas, on récupère donc le contenu à l'intérieur du nouveau Gérard en ignorant la punaise et Gérard lui-même (le conteneur)
    if (oDiv.querySelector(".Gerard"))
        monContent = oDiv.querySelector(".Gerard");

    // Et on ajoute les éléments du nouveau contenu un par un, en les supprimant du div temporaire après ajout
    while (monContent.firstChild) {
        var oElem = monContent.firstChild.cloneNode(true);
        monContent.removeChild(monContent.firstChild);
        // Même chose que pour Gérard : si on traite un menu issu de eFileMenuManager, on filtre l'encart Nom d'utilisateur qui a déjà été réinjecté plus haut
        if (typeof (oElem.getAttribute) != "function" || oElem.getAttribute("class") != "encartProfil")
            oMainDiv.appendChild(oElem);
    }

    //Resize du menu

    //createCss("customCss", "rightMenuWidth", "width:240px ; ", true);
    //createCss("customCss", "gerardWidth", "width:240px; ", true);

    if (document.getElementById("paramTab3")) {
        document.getElementById("paramTab3").style.display = "block";
        nsAdmin.resizeBlockContent("paramTab3"); // resize vertical
        nsAdmin.pictoBlock("paramTab3");
    }

    // Evénements
    nsAdmin.addTitleClickEvent("paramTab1");
    nsAdmin.addTitleClickEvent("paramTab3");

    nsAdmin.setAction();

    try { nsAdmin.adjustContentWidth(); } catch (e) { }

    if (typeof (callback) == "function")
        callback();

    nsAdmin.menuUpdated = true;

    nsAdmin.tryLoadAdminResizer();

    //ne pas lancer l'init des slider si l'objet slider n'est pas dispo
    if (typeof (noUiSlider) != "undefined") {
        if (noUiSlider)
            nsAdmin.loadPerfSliders();
    }

    //Handler de drag & drop
    nsAdmin.setDDHandlers();
}

/*****     NAVBAR                   *************/
nsAdmin.loadNavBar = function (module, moduleParam) {

    // Initioalisation des grilles
    GridSystem.reset();

    var nTab = moduleParam.tab;
    var nBkm = moduleParam.bkm;


    var extensionFileId = 0;
    if (moduleParam.extensionFileId)
        extensionFileId = moduleParam.extensionFileId;

    var extensionLabel = "";
    if (moduleParam.extensionLabel)
        extensionLabel = moduleParam.extensionLabel;

    var extensionCode = "";
    if (moduleParam.extensionCode)
        extensionCode = moduleParam.extensionCode;

    //Appel le waiter
    let oParamGoTabList = {
        to: 7,
        nTab: nTab,
        context: "eAdmin.loadNavBar"
    }
    top.setWait(true, undefined, undefined, isIris(top.getTabFrom()), oParamGoTabList);

    top.bIsNavBarLoaded = 0;

    //Navbar (Mode HTML)
    var oNavBarUpdater = new eUpdater("eda/mgr/eAdminNavBarManager.ashx", 1);

    oNavBarUpdater.ErrorCallBack = function () { setWait(false); };
    var winSize = getWindowSize();

    // Si on revient vers l'accueil, on n'epingle pas le menu, on prend toute la largeur
    if (nGlobalActiveTab == 0)
        winSize.dw = winSize.w;


    oNavBarUpdater.addParam("H", winSize.h, "post");
    oNavBarUpdater.addParam("W", winSize.dw, "post");
    oNavBarUpdater.addParam("tab", nTab, "post");


    oNavBarUpdater.addParam("bkm", nBkm, "post");
    oNavBarUpdater.addParam("module", getUserOptionsModuleHashCode(module), "post");

    // necessaire dans le cas des extensions et les grilles
    oNavBarUpdater.addParam("extensionFileId", extensionFileId, "post");
    oNavBarUpdater.addParam("extensionLabel", extensionLabel, "post");
    oNavBarUpdater.addParam("extensionCode", extensionCode, "post");


    switch (module) {
        case USROPT_MODULE_ADMIN_HOME_XRM_HOMEPAGE:
            oNavBarUpdater.addParam("pageId", moduleParam.pageId, "post");
            break;
        case USROPT_MODULE_ADMIN_HOME_XRM_HOMEPAGE_GRID:
            oNavBarUpdater.addParam("pageId", moduleParam.pageId, "post");
            oNavBarUpdater.addParam("gridId", moduleParam.gridId, "post");
            oNavBarUpdater.addParam("gridLabel", moduleParam.gridLabel, "post");
            break;
        default:
            break;
    }

    oNavBarUpdater.send(nsAdmin.updateNavBar);
}

nsAdmin.updateNavBar = function (oRes) {
    top.setWait(false);

    var oMainDiv = document.getElementById("globalNav");
    oMainDiv.innerHTML = oRes;

    // Chargement des MRU
    LoadMru(nsAdmin.tab, LOADFILEFROM.ADMIN);

    try { nsAdmin.adjustContentWidth(); } catch (e) { }

    nsAdmin.navbarUpdated = true;

    nsAdmin.tryLoadAdminResizer();
}

/********   CONTENU    Fiche            **************/
///Met à jour le contenu d'un mode fiche admin de puis le retour du loadContentModule
nsAdmin.updateContentFile = function (oRes) {

    top.setWait(false);
    var mainDiv = document.getElementById("mainDiv");
    mainDiv.innerHTML = oRes;

    // Pour ajuster la largeur, on a besoin de la navbar
    try { nsAdmin.adjustContentWidth(); } catch (e) { alert(e.message); }

    //Pour les contenu de type fiche autre que user
    if (nsAdmin.tab > 0 && nsAdmin.tab != 101000) {
        iaf = 0;
        nsAdmin.initAdminFile();

        // Evénements
        nsAdmin.addContentEventListeners();

        //Handler de drag & drop
        nsAdmin.setDDHandlers();
    }

    // Mettre en surbrillance la rubrique en cours
    if (typeof (FIELD_DID) !== 'undefined' && FIELD_DID > 0) {
        var cell = document.getElementById("COL_" + nsAdmin.tab + "_" + FIELD_DID);
        selectCell(cell, true, "active");
    }


    //check error admin
    var errDEf = mainDiv.querySelector("div[warningdefault='1']");
    if (errDEf) {
        var lstFormula = getAttributeValue(errDEf, "warningdefaultlst");
        top.eAlert(1, top._res_416, top._res_2579, top._res_2580 + lstFormula);
    }

    nsAdmin.contentUpdated = true;
    nsAdmin.tryLoadAdminResizer();
}

/********* CONTENU LIST ******************************/
nsAdmin.updateContentList = function (oRes, module, anchor, moduleParam) {
    top.setWait(false);

    // Pour ajuster la largeur, on a besoin de la navbar
    try { nsAdmin.adjustContentWidth(); } catch (e) { }

    try {
        var oJsonRes = JSON.parse(oRes);
        if (oJsonRes.Success) {
            var callBack = function () { top.updateContent(oJsonRes, moduleParam.tab, true); };

            top.nsMain.InitList(callBack);
        } else {
            eAlert(0, "", oJsonRes.ErrorMsg);
        }
    }
    catch (e) {
        //
        eAlert(0, "", e.message);
    }
}

nsAdmin.loadContent = function (nTab) {

    nsAdmin.loadContentModule(USROPT_MODULE_ADMIN_TAB, null, { tab: nTab });
}

/// <summary>
/// Charge le contenu d'un signet en admin : cas de Signet Grille
/// </summary>
/// <param name="options">
///     tab : descid de la table
///     width : largeur du rendu    
///     height : hauteur du rendu
///     onError : en cas d'erreur
///     onSuccess : en cas de succès
/// </param>
nsAdmin.loadBkmContent = function (options) {
    // le descid de la table doit etre valide
    if (typeof (options.bkm) == "undefined" || options.bkm <= 0 || options.bkm % 100 != 0) {
        console.log("Descid du signet doit être valide !");
        return;
    }

    // Valeurs par défaut si largeur ou hauteur non définit
    if (typeof (options.width) == "undefined" || typeof (options.height) == "undefined") {
        var tabMainDivWH = GetMainDivWH();
        options.width = tabMainDivWH[0];
        options.height = 300;
    }

    // Callback onSuccess après retour serveur
    if (typeof (options.onSuccess) != "function")
        options.onSuccess = function () { console.log("onSuccess"); };

    // Callback onError après retour serveur
    if (typeof (options.onError) != "function")
        options.onError = function () { console.log("onError"); };

    oUpdater = new eUpdater("eda/mgr/eAdminFileManager.ashx", 1);

    oUpdater.addParam("tab", options.bkm, "post");
    oUpdater.addParam("w", options.width, "post");
    oUpdater.addParam("h", options.height, "post");
    oUpdater.addParam("module", getUserOptionsModuleHashCode(USROPT_MODULE_ADMIN_TAB), "post");

    oUpdater.ErrorCallBack = function () { top.setWait(false); options.onError(); };
    oUpdater.send(function (oRes) {
        top.setWait(false);
        options.onSuccess(oRes);
    });

}



/********* CONTENU MODULES ******************************/
/// <summary>
/// Charge la page d'administration issue d'un module
/// </summary>
nsAdmin.loadContentModule = async function (module, anchor, moduleParam) {
    // add class name for the skeleton, it will be removed when the page left
    let pnWaiterSkltonfile = document.getElementById('pnWaiterSkltonfile');
    if (pnWaiterSkltonfile) {
        addClass(pnWaiterSkltonfile, "adminMode")
    }

    var oParamGoTabList = {
        to: 7,
        nTab: moduleParam.tab,
        context: "eAdmin.loadContentModule"
    }
    top.setWait(true, undefined, undefined, isIris(top.getTabFrom()), oParamGoTabList);

    // dimension
    var oMainDiv = document.getElementById("mainDiv");
    var tabMainDivWH = GetMainDivWH();
    var height = tabMainDivWH[1];
    var divWidth = tabMainDivWH[0];

    var oContentUpdater; // object manager
    var moduleManager = "";  // type manager
    var aParam = []; // Paramètres manager

    //Fct de maj par défaut
    var fctUpdate = nsAdmin.updateContentModule;

    //Params Fixes
    aParam.push({ key: "w", value: divWidth, meth: "post" });
    aParam.push({ key: "h", value: height, meth: "post" });
    aParam.push({ key: "module", value: getUserOptionsModuleHashCode(module), meth: "post" });
    // En fonction du module, utilisation d'une fonction de chargement spécifique, ou appel des fonctions génériques
    switch (module) {
        case USROPT_MODULE_ADMIN_TAB_USER:
            oContentUpdater = getFileUpdater(101000, moduleParam.userid, 3, false);
            aParam.push({ key: "mainheight", value: document.getElementById("mainDiv").offsetHeight, meth: "post" });
            fctUpdate = function (oRes) { top.updateFile(oRes, 101000, moduleParam.userid, 3); nsAdminUsers.initPagging(); };
            break;

        case USROPT_MODULE_ADMIN_TAB:
            moduleManager = "eAdminFileManager";
            aParam.push({ key: "tab", value: moduleParam.tab, meth: "post" });
            if (moduleParam.hasOwnProperty("sys"))
                aParam.push({ key: "sys", value: moduleParam.sys, meth: "post" });
            fctUpdate = nsAdmin.updateContentFile;
            break;

        // On passe par eAdminModuleManager pour la liste des pages d'accueil XRM
        case USROPT_MODULE_ADMIN_HOME:
        case USROPT_MODULE_ADMIN_HOME_XRM_HOMEPAGES:
            moduleManager = "eAdminModuleManager";

            var oeParam = getParamWindow();

            var nRows = 0;
            if (typeof (oeParam.GetParam) != "undefined" && oeParam.GetParam('Rows') != '')
                nRows = oeParam.GetParam('Rows');

            aParam.push({ key: "r", value: nRows, meth: "post" });
            aParam.push({ key: "f", value: "1", meth: "post" });

            fctUpdate = function (oRes, module, anchor, moduleParam) { nsAdmin.updateContentList(oRes, module, anchor, moduleParam); initFldClick(115200); };
            break;

        case USROPT_MODULE_ADMIN_ACCESS:
        case USROPT_MODULE_ADMIN_ACCESS_USERGROUPS:
        case USROPT_MODULE_ADMIN_DASHBOARD_RGPDTREATMENTLOG:
            moduleManager = "eAdminModuleManager";

            var oeParam = getParamWindow();

            var nRows = 0;
            if (typeof (oeParam.GetParam) != "undefined" && oeParam.GetParam('Rows') != '')
                nRows = oeParam.GetParam('Rows');

            aParam.push({ key: "r", value: nRows, meth: "post" });
            aParam.push({ key: "f", value: "1", meth: "post" });
            fctUpdate = nsAdmin.updateContentList;
            break;

        case USROPT_MODULE_ADMIN_DASHBOARD:
            moduleManager = "eAdminModuleManager";
            if (moduleParam.year)
                aParam.push({ key: "year", value: moduleParam.year, meth: "post" });
            break;

        case USROPT_MODULE_ADMIN_TABS:
            moduleManager = "eAdminTabsManager";

            fctUpdate = function (oRes, module, anchor, moduleParam) {
                //met à jour le contentu
                nsAdmin.updateContentModule(oRes, module, anchor, moduleParam);

                //tri suivant les params
                var paramSort = Object.assign({ currentSortCol: "", currentSortDir: "", currentSearch: "" }, moduleParam)

                if ((paramSort.currentSortCol !== "") || paramSort.currentSearch !== "") {
                    nsAdminTabsList.sort(paramSort.currentSortCol, paramSort.currentSearch, paramSort.currentSortDir, false);
                }
            };
            break;

        case USROPT_MODULE_ADMIN_HOME_V7_HOMEPAGES:
        case USROPT_MODULE_ADMIN_HOME_EXPRESS_MESSAGE:
            moduleManager = "eAdminHomepagesManager";
            break;

        case USROPT_MODULE_ADMIN_EXTENSIONS:
            moduleManager = "eAdminModuleManager";

            /**
             * On se sert de la même fonction pour afficher la liste des modules, avec
             * la liste infinie, et pour recharger la page.
             * On force la réinitialisation de la première page pour les cas où
             * on n'est pas dans la liste infinie.
             */
            if (!moduleParam.notFirstLoad)
                nsAdminStoreList.currentPage = 1;

            if (nsAdmin.ModeDebug) {
                aParam.push({ key: "typ", value: moduleParam.type, meth: "post" });
                aParam.push({ key: "p", value: nsAdminStoreList.currentPage, meth: "post" });
                aParam.push({ key: "r", value: nsAdminStoreList.rows, meth: "post" });
                aParam.push({ key: "fs", value: nsAdminStoreList.currentFilterSearch, meth: "post" });
                aParam.push({ key: "fc", value: nsAdminStoreList.currentFilterCategory, meth: "post" });
                aParam.push({ key: "fo", value: nsAdminStoreList.currentFilterOffers, meth: "post" });
                aParam.push({ key: "fd", value: nsAdminStoreList.currentFilterDisplay, meth: "post" });
                aParam.push({ key: "fof", value: nsAdminStoreList.currentFilterOther, meth: "post" });
            } else {
                if (moduleParam.page) { aParam.push({ key: "p", value: moduleParam.page, meth: "post" }); } else { aParam.push({ key: "p", value: nsAdmin.currentExtensionPage, meth: "post" }); }
                if (moduleParam.rows) { aParam.push({ key: "r", value: moduleParam.rows, meth: "post" }); } else { aParam.push({ key: "r", value: nsAdmin.extensionsPerPage, meth: "post" }); }
                if (moduleParam.search) { aParam.push({ key: "fs", value: moduleParam.search, meth: "post" }); } else { aParam.push({ key: "fs", value: '', meth: "post" }); }
                if (moduleParam.filterCategory) { aParam.push({ key: "fc", value: moduleParam.filterCategory, meth: "post" }); } else { aParam.push({ key: "fc", value: '', meth: "post" }); }
                if (moduleParam.filterNotation) { aParam.push({ key: "fn", value: moduleParam.filterNotation, meth: "post" }); } else { aParam.push({ key: "fn", value: '', meth: "post" }); }
            }
            break;

        case USROPT_MODULE_ADMIN_EXTENSIONS_FROMSTORE:
        case USROPT_MODULE_ADMIN_EXTENSIONS_MOBILE:
        case USROPT_MODULE_ADMIN_EXTENSIONS_OUTLOOKADDIN:
        case USROPT_MODULE_ADMIN_EXTENSIONS_SIRENE:
        case USROPT_MODULE_ADMIN_EXTENSIONS_SYNCHRO:
        case USROPT_MODULE_ADMIN_EXTENSIONS_SYNCHRO_OFFICE365:
        case USROPT_MODULE_ADMIN_EXTENSIONS_SMS:
        case USROPT_MODULE_ADMIN_EXTENSIONS_CTI:
        case USROPT_MODULE_ADMIN_EXTENSIONS_API:
        case USROPT_MODULE_ADMIN_EXTENSIONS_EXTERNALMAILING:
        case USROPT_MODULE_ADMIN_EXTENSIONS_CARTO:
        case USROPT_MODULE_ADMIN_EXTENSIONS_VCARD:
        case USROPT_MODULE_ADMIN_EXTENSIONS_GRID:
        case USROPT_MODULE_ADMIN_EXTENSIONS_SNAPSHOT:
        case USROPT_MODULE_ADMIN_EXTENSIONS_EMAILING:
        case USROPT_MODULE_ADMIN_EXTENSIONS_NOTIFICATIONS:
        case USROPT_MODULE_ADMIN_EXTENSIONS_POWERBI:
        case USROPT_MODULE_ADMIN_EXTENSIONS_ACCOUNTING_BUSINESSSOFT:
        case USROPT_MODULE_ADMIN_EXTENSIONS_ACCOUNTING_CEGID:
        case USROPT_MODULE_ADMIN_EXTENSIONS_ACCOUNTING_SAGE:
        case USROPT_MODULE_ADMIN_EXTENSIONS_ACCOUNTING_EBP:
        case USROPT_MODULE_ADMIN_EXTENSIONS_ACCOUNTING_SIGMA:
        case USROPT_MODULE_ADMIN_EXTENSIONS_IN_UBIFLOW:
        case USROPT_MODULE_ADMIN_EXTENSIONS_IN_HBS:
        case USROPT_MODULE_ADMIN_EXTENSIONS_DOCUSIGN:
        case USROPT_MODULE_ADMIN_EXTENSIONS_SMS_NETMESSAGE:
        case USROPT_MODULE_ADMIN_EXTENSIONS_SYNCHRO_EXCHANGE2016ONPREMISE:
        case USROPT_MODULE_ADMIN_EXTENSIONS_ZAPIER:
        case USROPT_MODULE_ADMIN_EXTENSIONS_EXTRANET:
        //SHA : tâche #1 873
        case USROPT_MODULE_ADMIN_EXTENSIONS_ADVANCED_FORM:
        case USROPT_MODULE_ADMIN_EXTENSIONS_DEDICATED_IP:
        case USROPT_MODULE_ADMIN_EXTENSIONS_WORLDLINE_PAYMENT:
        case USROPT_MODULE_ADMIN_EXTENSIONS_MAIL_STATUS_VERIFICATION:
        case USROPT_MODULE_ADMIN_EXTENSIONS_LINKEDIN:
            moduleManager = "eAdminModuleManager";
            if (moduleParam.extensionFileId) { aParam.push({ key: "extid", value: moduleParam.extensionFileId, meth: "post" }); }
            else { aParam.push({ key: "extid", value: '0', meth: "post" }); }

            if (moduleParam.initialTab) { aParam.push({ key: "it", value: moduleParam.initialTab, meth: "post" }); }
            else { aParam.push({ key: "it", value: 'description', meth: "post" }); }
            break;

        //case USROPT_MODULE_ADMIN_DASHBOARD_RGPD:

        //    moduleManager = "eAdminModuleManager";

        //    break;

        default:
            moduleManager = "eAdminModuleManager";
    }

    if (typeof oContentUpdater == "undefined")
        oContentUpdater = new eUpdater("eda/mgr/" + moduleManager + ".ashx", 1);

    //Param  
    aParam.forEach(function (p) {
        oContentUpdater.addParam(p.key, p.value, p.meth);
    });

    oContentUpdater.ErrorCallBack = function () {
        top.setWait(false, undefined, undefined, isIris(top.getTabFrom()), oParamGoTabList);
    };

    oContentUpdater.send(function (oRes) {
        top.setWait(false, undefined, undefined, isIris(top.getTabFrom()), oParamGoTabList);
        fctUpdate(oRes, module, anchor, moduleParam);
        switchUserThemeToDefaultTheme(module);

        if (typeof moduleParam.callback === "function")
            moduleParam.callback();
    });

    await nsMain.SetGlobalActiveTab(nsAdmin.tab);
}

var ContentModeRefresh = { REPLACE: 0, ADD_CHILDS: 1 };
nsAdmin.updateContentModule = function (oRes, module, anchor, moduleParam) {

    //SHA : #73 223
    //JS/CSS spécifique
    nsAdmin.loadAdminJs();
    nsAdmin.loadAdminCss();
    nsAdmin.loadModuleCssScripts(module);
    //TODO : surcharger loadAdminCss et loadAdminJs pour charger CSS et JS propres au module Extension
    //nsAdmin.loadAdminCss(module);
    //nsAdmin.loadAdminJs(module);

    // Pour ajuster la largeur, on a besoin de la navbar
    try { nsAdmin.adjustContentWidth(); } catch (e) { }

    try {
        var oJsonRes = JSON.parse(oRes);
        if (oJsonRes.Success) {
            var mainDiv = document.getElementById("mainDiv");

            if (oJsonRes.Html.length > 0) {
                mainDiv.innerHTML = oJsonRes.Html;
            } else {
                var parts = oJsonRes.MultiPartContent;
                for (var key in parts) {
                    if (parts.hasOwnProperty(key)) {
                        var myPart = parts[key];

                        var oDivparts = document.getElementById(myPart.ID);
                        if (!oDivparts)
                            continue;

                        if (myPart.Mode == ContentModeRefresh.ADD_CHILDS) {
                            // Ajout de childs
                            var tempDiv = document.createElement("div");
                            tempDiv.innerHTML = myPart.Content;

                            var arrAllWebTab = [].slice.call(tempDiv.firstChild.children);
                            arrAllWebTab.forEach(function (oChild) {
                                oDivparts.appendChild(oChild);
                            });
                        } else {
                            // Remplace le contenu
                            oDivparts.outerHTML = myPart.Content;
                        }
                        // On ne gére pas ici l'option Full et CallBack du part
                    }
                }
            }

            // Titres dépliables bleus
            nsAdmin.addStepTitleEventListeners();

            // Ancre
            if (anchor) {
                document.getElementById(anchor).scrollIntoView();
            }

            if (module && module.indexOf(USROPT_MODULE_ADMIN_HOME) != -1 && typeof (nsAdminHomepages) == "object") {
                nsAdminHomepages.load();
            }
            else if (module && module == USROPT_MODULE_ADMIN_EXTENSIONS) {
                if (oJsonRes.iCountModule && oJsonRes.iCountModule != 0) {
                    nsAdminStoreList.totalModule = oJsonRes.iCountModule;
                    nsAdminStoreList.totalPages = oJsonRes.iPagesModules;
                }

                nsAdmin.StoreResizeContent();
            }
            else if (module && module.indexOf(USROPT_MODULE_ADMIN_DASHBOARD) != -1) {
                //nsAdminDashboard.init(module); // Sera lancé côté eAdminDashboardRenderer
            }
            else if (module && module.indexOf(USROPT_MODULE_ADMIN_EXTENSIONS) != -1) {
                if (nsAdmin.ModeDebug) {

                }
                else {
                    var carousel = new Carousel({
                        elem: 'screenshotsCarousel',    // id of the carousel container
                        autoplay: false,     // starts the rotation automatically
                        interval: 1500,      // interval between slide changes
                        initial: 1,          // slide to start with
                        dots: true,          // show navigation dots
                        arrows: true,        // show navigation arrows
                        buttons: false,      // hide play/stop buttons,
                        btnStopText: 'Pause', // STOP button text
                        crslArrowPrevClass: "icon-caret-left",
                        crslArrowNextClass: "icon-caret-right",
                        arrPrevText: "&nbsp;",
                        arrNextText: "&nbsp;"
                    });
                }
            }

            if (moduleParam) {
                if (moduleParam.initialTab) {
                    nsAdmin.displayExtensionTab(moduleParam.initialTab);
                }
                //if (moduleParam.scrollSaved) {
                //    moduleParam.scrollSaved.element.scrollTop = moduleParam.scrollSaved.scrollTop;
                //}
            }


            if (oJsonRes.CallBack) {
                /* -> ici, on place le script dans le scope global - iso eMain.js */
                addScriptText(oJsonRes.CallBack, top.document);
            }

        } else {
            eAlert(0, "", oJsonRes.ErrorMsg);
        }
    }
    catch (e) {
        console.log(e);
        alert(e.message);
    }
}

nsAdmin.fctCloseStep = function () {
    // Modif icône
    icon = this.children[0];
    if (icon.className.indexOf("icon") > -1) {
        icon.className = (icon.className == "icon-unvelop") ? "icon-develop" : "icon-unvelop";
    }
    // Affichage/masquage contenu
    content = this.nextSibling;
    if (hasClass(content, "stepContent")) {
        if (getAttributeValue(content, "data-active") != "1") {
            setAttributeValue(content, "data-active", "1");
        }
        else {
            setAttributeValue(content, "data-active", "0");
        }
    }
}

nsAdmin.addStepTitleEventListeners = function () {
    var content, icon;
    var titles = document.getElementsByClassName("paramStep");



    for (var i = 0; i < titles.length; i++) {

        titles[i].removeEventListener("click", nsAdmin.fctCloseStep, true);
        titles[i].addEventListener("click", nsAdmin.fctCloseStep, true);
    }
}

nsAdmin.addNewTab = function () {
    var modalNewTab = new eModalDialog(top._res_7203, 0, "eda/eAdminNewTabDialog.aspx", 500, 150, "modalAdminNewTab");

    modalNewTab.noButtons = false;
    modalNewTab.hideMaximizeButton = true;
    modalNewTab.addParam("iframeScrolling", "no", "post");
    modalNewTab.onIframeLoadComplete = (function (modal) {
        return function () {
            modal.getIframe().document.getElementById("txtNewTabName").select();
            setEventListener(modal.getIframe().document.getElementById("formAdminNewTab"), "submit", function (e) {
                e.preventDefault(); // #62854 : Pour éviter de recharger eMain.aspx et passer pref.AdminMode à false
                top.nsAdmin.onValidAddNewTab(modalNewTab);
            });
        }
    })(modalNewTab);
    modalNewTab.NoScrollOnMainDiv = true;
    modalNewTab.show();


    modalNewTab.addButton(top._res_29, function () { modalNewTab.hide(); }, 'button-gray', null);
    modalNewTab.addButton(top._res_28, function () {
        top.setWait(true);
        nsAdmin.onValidAddNewTab(modalNewTab);
    }, 'button-green', null, "ok");
}

//Méthode appelée à la validation d'une nouvelle table
nsAdmin.onValidAddNewTab = function (modalNewTab) {
    if (!modalNewTab) {
        top.setWait(false);
        return;
    }

    var doc = modalNewTab.getIframe().document;
    var ddlType = doc.getElementById("ddlNewTabType");
    var newTabName = doc.getElementById("txtNewTabName").value;
    var newTabType = ddlType.value;

    var upd = new eUpdater("eda/Mgr/eAdminNewTabDialogManager.ashx", 1);
    upd.addParam("newTabName", newTabName, "post");
    upd.addParam("newTabType", newTabType, "post");

    var specType = getAttributeValue(eTools.getSelectedItem(ddlType), "specType");
    if (specType)
        upd.addParam("specType", specType, "post");

    upd.ErrorCallBack = function () {
        top.setWait(false)
    };
    upd.send(function (oRes) {
        top.setWait(false); nsAdmin.onAddedNewTab(oRes, modalNewTab);
    });
}

nsAdmin.onAddedNewTab = function (oRes, modalNewTab) {
    var result = JSON.parse(oRes);
    if (!result.Success) {
        if (result.Criticity == 2)
            top.eAlert(result.Criticity, top._res_92, result.UserErrorMessage);
        else
            top.eAlert(1, top._res_416, result.UserErrorMessage);

        return;
    }

    nsAdmin.loadAdminFile(result.Descid);

    if (modalNewTab)
        modalNewTab.hide();
};

/*****   adaptation des tailles des conteur ********/
nsAdmin.adjustContentWidth = function () {

    var menu = document.getElementById("Gerard");
    var menuBar = document.getElementById("menuBar");
    var mainDiv = document.getElementById("mainDiv");
    var adminNav = document.getElementById("adminNav");

    var gerardWidth = menu.scrollWidth + menuBar.offsetWidth;

    var winSize = getWindowSize();
    var nInnerWidth = winSize.w;

    if (adminNav && mainDiv) {
        adminNav.style.width = (nInnerWidth - gerardWidth) + "px";
        mainDiv.style.height = (winSize.h - (adminNav.offsetHeight + 40)) + "px";
    }

    createCss("customCss", "mainDivWidth", "width: " + (nInnerWidth - gerardWidth - 10) + "px !important", true);
    //mainDiv.innerHTML = "<img src='themes/default/images/AdminRubriques.jpg' alt='' title='' width='" + (nInnerWidth - gerardWidth) + "px' height='" + mainDiv.style.height + "' />";
}

nsAdmin.initAdminFile = function () {
    if (typeof (initAdminFile) == "function"
        && typeof (adjustScrollFile) == "function"
        && typeof (nsAdminMoveField.init) == "function"
        && typeof (nsAdminMoveBkmBar.init) == "function") {
        nsAdminMoveField.init();
        nsAdminMoveBkmBar.init();

        initAdminFile();
        adjustScrollFile();
        if (nsAdminMoveField.ScrollTop)
            nsAdminMoveField.DivBkmPres.scrollTop = nsAdminMoveField.ScrollTop;
    }
    else if (iaf <= 12) {
        iaf++;
        setTimeout(nsAdmin.initAdminFile, 250);
    }
}

// Appel du manager permettant de mettre à jour [DESC].[COLUMNS]
nsAdmin.updateListColWidth = function (listColWidth) {
    var descid = getNumber(getAttributeValue(document.getElementById("navTabs"), "did"));
    if (!descid > 0)
        return;

    var hidListColWidth = document.getElementById("hidListColWidth");
    var aDesc = getAttributeValue(hidListColWidth, "dsc").split("|");

    if (aDesc.length < 2)
        return;

    var caps = new Capsule(descid);
    caps.AddProperty("0", aDesc[1], listColWidth, aDesc.length > 2 ? aDesc[2] : null);

    var json = JSON.stringify(caps);
    var upd = new eUpdater("eda/Mgr/eAdminDescManager.ashx", 0);

    upd.json = json;
    upd.ErrorCallBack = function () { alert('La mise à jour des tailles de colonnes a échoué.'); };
    upd.send();
}

//Ajoute un script d'amin (répertoir script admin)
nsAdmin.addScript = function (href, stype, callback, oDoc) {
    if (!oDoc || typeof (oDoc) == 'undefined') {
        oDoc = document;
    }
    if (typeof (stype) != "undefined" && stype != "") {
        var oHead = oDoc.getElementsByTagName("head")[0];
        var oScript = oDoc.createElement('script');

        oScript.type = 'text/javascript';
        oScript.src = "eda/scripts/" + href + ".js";

        if (typeof (top._jsVer) != 'undefined' && top._jsVer != "")
            oScript.src += '?ver=' + top._jsVer;

        oScript.setAttribute("eType", stype);

        if (typeof (callback) == "function") {
            // mza avec rma on a decemmentés cette ligne suite a une erreur quand on veux ajouter un nouveau contact
            oScript.async = 'true';

            //Le code suivant fonctionne avec IE8 et supérieur :
            //  onload => Ne fonctionne pas avec IE8
            //  onreadystatechange => Fonctionne avec IE8 et autre
            oScript.onload = oScript.onreadystatechange = function () {
                var rs = this.readyState;
                if (rs && rs != 'complete' && rs != 'loaded')
                    return;

                oScript.onload = oScript.onreadystatechange = null; //Pas 2 fois le même script

                try {
                    callback();
                }
                catch (e) {
                    alert('addScript Error - ' + callback);
                    alert('Exception: ' + e);
                    alert('Description: ' + e.message);
                }
            };
        }
        oHead.appendChild(oScript);
    }
}

// Afficher/cacher une partie
nsAdmin.showHidePart = function (titleElement, bForceOpen) {
    var iShow = 0;

    var paramPart = titleElement.parentElement;
    var paramPartContent = paramPart.querySelector(".paramPartContent");
    var titleIcon = paramPart.querySelector("header [class^='icon']");
    var paramBlockContent = paramPart.parentElement;

    if (paramPart.className == "btnLink") {
        var rulesAndAutomatisme = paramPart.id;
    }

    if (typeof (bForceOpen) === "undefined")
        bForceOpen = false;

    // Fermé
    if (paramPartContent.getAttribute("data-active") == "1" && !bForceOpen) {
        paramPartContent.setAttribute("data-active", "0");
        paramPartContent.setAttribute("eactive", "0");
        if (rulesAndAutomatisme != null && rulesAndAutomatisme != undefined &&
            rulesAndAutomatisme == "adminRules" || rulesAndAutomatisme == "adminBkmRules" || rulesAndAutomatisme == "adminAutomatismes" || rulesAndAutomatisme == "adminFileRules") {

            titleIcon.className = "icon-develop";
            titleElement.className = "";
        }

        else
            titleIcon.className = "icon-caret-right";
    }
    // Ouvert
    else {
        paramPartContent.setAttribute("data-active", "1");
        paramPartContent.setAttribute("eactive", "1");

        if (rulesAndAutomatisme != null && rulesAndAutomatisme != undefined &&
            rulesAndAutomatisme == "adminRules" || rulesAndAutomatisme == "adminBkmRules" || rulesAndAutomatisme == "adminAutomatismes" || rulesAndAutomatisme == "adminFileRules") {
            titleIcon.className = "icon-unvelop";
            titleElement.className = "blueHeader";
        }

        else
            titleIcon.className = "icon-caret-down";

        iShow = 1;
    }
    return iShow;
}

// Afficher/cacher sous-partie
nsAdmin.showHideSubPart = function (element) {
    var spanIcon = element.querySelector("span[class^='icon-']");
    var nextDiv = element.nextSibling;
    if (nextDiv.className == "subPart") {
        if (nextDiv.style.display == "none") {
            nextDiv.style.display = "block";
            spanIcon.className = "icon-unvelop";
        }
        else {
            nextDiv.style.display = "none";
            spanIcon.className = "icon-develop";
        }
    }
}

//Gestion du fond des pictos des bloc d administration
nsAdmin.pictoBlock = function (id) {
    var pictoId = 'paramTabPicto' + id.slice(-1);
    var blocksPicto = document.getElementsByClassName("paramTabPicto");
    for (var i = 0; i < blocksPicto.length; i++) {
        blocksPicto[i].style.color = "#8DD1F3";
    }
    var picto = document.getElementById(pictoId);
    picto.style.color = "white";
}

// Affichage des blocs suivant l'icône
nsAdmin.showBlock = function (id) {

    nsAdmin.pictoBlock(id);
    var blocks = document.getElementsByClassName("paramBlock");

    for (var i = 0; i < blocks.length; i++) {
        blocks[i].style.display = "none";
    }

    var destPosition = "5px";
    var position = id[id.length - 1];
    var nSize = eTools.GetFontSize();

    if (nSize >= 14) {
        switch (position) {

            case "1": destPosition = "44px"; break;
            case "2": destPosition = "160px"; break;
            case "3": destPosition = "277px"; break;
        }
    }
    else {
        switch (position) {

            case "1": destPosition = "27px"; break;
            case "2": destPosition = "110px"; break;
            case "3": destPosition = "193px"; break;
        }
    }


    nsAdmin.animateLeft(document.getElementById("slidingArrow"), destPosition);

    var block = document.getElementById(id);

    block.style.display = "block";
    nsAdmin.resizeBlockContent(id);

    nsAdmin.setAction();
}

// Redimensionnement du bloc par rapport à la résolution
nsAdmin.resizeBlockContent = function (blockId) {
    var blockHeight = window.innerHeight - 40 - 50 - 40 - 40; // on enlève la partie user + navigation + titre + bouton déconnexion

    var blockContent = document.querySelector("#" + blockId + " .paramBlockContent");
    if (blockContent) {
        blockContent.style.display = "block";
        blockContent.style.height = blockHeight + "px";
    }
}

// Animation gauche (pour la flèche)
nsAdmin.animateLeft = function (obj, leftPosition) {
    obj.style.transition = "0.5s";
    obj.style.left = leftPosition;
}

// Slider - onchange : va cocher/décocher les cases à cocher par rapport à la valeur sélectionnée du slider
nsAdmin.perfSliderOnChange = function (slider) {
    var sliderValue = slider.get();
    var advOptions = document.getElementById("advOptions");
    if (advOptions != null) {
        var checkedCheckboxes = advOptions.querySelectorAll(".rChk[chk='1']");
        var nbCheckedCheckboxes = checkedCheckboxes.length;
        if (nbCheckedCheckboxes < sliderValue) {
            // Cas où on coche des cases en plus
            var nbToCheck = sliderValue - nbCheckedCheckboxes;
            var checkboxes = advOptions.querySelectorAll(".rChk");
            var counter = 0;
            while (nbToCheck > 0 && counter < checkboxes.length) {
                if (checkboxes[counter]) {
                    if (getAttributeValue(checkboxes[counter], "chk") == "0") {
                        chgChk(checkboxes[counter]);
                        top.nsAdmin.onCheckboxClick(checkboxes[counter]);
                        nbToCheck--;
                    }
                }
                counter++;
            }
        }
        else if (nbCheckedCheckboxes > sliderValue) {
            // Cas où doit décocher des cases
            var nbToUncheck = nbCheckedCheckboxes - sliderValue;
            var counter = nbCheckedCheckboxes - 1;
            while (nbToUncheck > 0 && counter >= 0) {
                if (checkedCheckboxes[counter]) {
                    chgChk(checkedCheckboxes[counter]);
                    top.nsAdmin.onCheckboxClick(checkedCheckboxes[counter]);
                    nbToUncheck--;
                    counter--;
                }
            }
        }
    }


}

// Mise à jour du slider par rapport aux cases cochées de la partie "Options avancées"
nsAdmin.updateSliderValue = function () {
    var perfSlider = document.getElementById("perfSlider");
    var slider = perfSlider.noUiSlider;

    //var rangeSlider__handle = document.getElementsByClassName("rangeSlider__handle")[0];
    //var rangeSlider__fill = document.getElementsByClassName("rangeSlider__fill")[0];
    //var rangeSlider = document.getElementsByClassName("rangeSlider")[0];
    var nbCheckboxes = document.querySelectorAll("#advOptions .rChk").length.toString();
    var nbChecked = document.querySelectorAll("#advOptions .rChk[chk='1']").length;

    if (nbChecked.toString() == slider.value)
        return;
    else {
        //Mis a jour de la value
        slider.set(nbChecked);
        //var rangeSliderWidth = Number(parseInt(rangeSlider.offsetWidth));
        //var rangeSliderFillWidth = (nbChecked / parseInt(nbCheckboxes)) * rangeSliderWidth;
        //rangeSlider__fill.style.width = rangeSliderFillWidth + "px";
        //rangeSlider__handle.style.left = (rangeSliderFillWidth - 8.5) + "px";
    }
}

nsAdmin.bkmOrTabNameOnClick = function (e, descId, elementType) {
    var hoveredElements = eTools.getElementsUnderMouseCursor(e);
    var bRename = false;
    for (var i = 0; i < hoveredElements.length; i++) {
        if (hoveredElements[i].className == "icon-edn-pen") {
            bRename = true;
            break;
        }
    }

    if (bRename) {
        var input = document.getElementById("divInput" + elementType + "Name");
        var divLabel = document.getElementById("divLabel" + elementType + "Name");
        if (input.style.display == "none") {
            divLabel.style.display = "none";
            input.style.display = "inline-block";
            document.getElementById("input" + elementType + "Name").focus();
        }
        else {
            divLabel.style.display = "block";
            input.style.display = "none";
        }

        nsAdmin.showBlock('paramTab3'); // affichage de la section Paramètres de l'onglet ou signet à droite
    }
    else {
        if (elementType == "Tab")
            nsAdmin.loadAdminFile(descId);
        else
            nsAdminField.editFieldProperties(descId);
    }
}

//Active ou desactive un element en fonction du click sur l element source de type checkbox
nsAdmin.disableOnSelectedValue = function (e, value, targetId) {
    var target = document.getElementById(targetId);

    if (e && target) {
        if (e.value == value) {
            target.setAttribute("disabled", true);
            target.value = '-1';
            if (target.parentElement)
                target.parentElement.style.display = "none"
        } else {
            target.removeAttribute("disabled");
            target.value = '0';
            if (target.parentElement)
                target.parentElement.style.display = "block"
        }
        target.onchange();
    }
}

nsAdmin.tabNameOnClick = function (e, descId) {
    nsAdmin.bkmOrTabNameOnClick(e, descId, "Tab");
}

nsAdmin.updateTabNameOnBlur = function (element) {
    nsAdmin.sendJson(element);
}

nsAdmin.bkmNameOnClick = function (e, descId) {
    nsAdmin.bkmOrTabNameOnClick(e, descId, "Bkm");
}

nsAdmin.updateBkmNameOnBlur = function (element) {
    nsAdmin.sendJson(element);
}

///valide la valeur du paramètre par rapport au format perf
nsAdmin.validParam = function (obj) {


    var errorMessage = "";
    var bIsCheckbox = false;

    var format = getAttributeValue(obj, "format");
    if (format == "")
        return true;

    var value = obj.value;
    if (obj.type == "checkbox") {
        value = obj.checked ? "1" : "0";
        bIsCheckbox = true;
    }
    else if (getAttributeValue(obj, "chk") != "") {
        value = getAttributeValue(obj, "chk");
        bIsCheckbox = true;
    }
    else if (obj.type == "radio") {
        var selectedVal = obj.id.split("_");
        value = selectedVal[1];
    }
    else if (obj.id == "buttonBold" || obj.id == "buttonItalic" || obj.id == "buttonUnderline") {
        value = getAttributeValue(obj, "dbvalue"); // Pour IE
    }

    /*  Obligatoire */
    if (getAttributeValue(obj, "opt") != "1" && value == "" && !bIsCheckbox) {
        errorMessage = top._res_372;

        //reprise valeur précédante
        obj.value = getAttributeValue(obj, "lastvalid");
        eAlert(1, "", errorMessage);
        return false;
    }

    /******************************************************************/
    /*  Validation de la valeur */
    if (!eValidator.isValueValid(getNumber(format), value)) {
        //Si la valeur est vide et que le champ n'est pas obligatoire, on quitte
        if (value == "" && (getAttributeValue(obj, "opt") == "0"))
            return true;

        //6275, "Format incorrect"
        var title = top._res_6275;

        //2021,"La valeur saisie ""<VALUE>"" ne correspond pas au format et/ou à la longueur de la rubrique <FIELD>"
        errorMessage = top._res_2021.replace("<VALUE>", value).replace("<FIELD>", "");

        //reprise valeur précédante
        obj.value = getAttributeValue(obj, "lastvalid");
        eAlert(1, title, errorMessage);
        return false;
    }

    var bValid = true;

    //Check Range
    if (format == eValidator.format.NUMERIC) {
        var rngMin = getAttributeValue(obj, "erngmin");
        var rngMax = getAttributeValue(obj, "erngmax");

        if (rngMax != "" && rngMin != "")
            errorMessage = top._res_6961;
        else if (rngMax != "")
            errorMessage = top._res_6962;
        else if (rngMin != "")
            errorMessage = top._res_6963;

        if (rngMin != "") {
            rngMin = getNumber(rngMin);
            if (obj.value < rngMin) {
                obj.value = getAttributeValue(obj, "lastvalid");
                bValid = false;
            }
        }

        if (rngMax != "") {
            rngMax = getNumber(rngMax);
            if (obj.value > rngMax) {
                obj.value = getAttributeValue(obj, "lastvalid");
                bValid = false;
            }
        }

        if (!bValid) {
            errorMessage = errorMessage.replace("<MIN>", rngMin);
            errorMessage = errorMessage.replace("<MAX>", rngMax);
        }
    }


    if (!bValid) {
        eAlert(1, top._res_1760, errorMessage); // Mise à jour non effectuée
        return false;
    }

    /******************************************************************/

    return true;
}

nsAdmin.getCapsule = function (obj, bConfirm) {

    // On récupère les identifiants de la donnée à mettre à jour (table et propriété)
    var aDesc = getAttributeValue(obj, "dsc").split("|");
    if (aDesc.length < 2)
        return;

    var advCat = aDesc.length > 2 ? aDesc[2] : "0"; // on utilise 0 et non null la mise à jour de valeurs de CONFIG/PREF/etc. null fait échouer la désérialisation

    // On récupère le DescID du champ à mettre à jour (si applicable)
    var descid;
    if (obj.hasAttribute("did"))
        descid = getNumber(getAttributeValue(obj, "did"));
    else
        descid = getNumber(getAttributeValue(document.getElementById("navTabs"), "did"));


    // On récupère la valeur du champ selon le type de champ (Checkbox, Combobox, etc.)
    var value = obj.value;
    var subCaps;

    // Création de la capsule
    var caps = new Capsule(descid);

    // Checkbox XRM
    if (obj.tagName == "A" && getAttributeValue(obj, "chk") != "") {

        value = getAttributeValue(obj, "chk");

        if (typeof (obj.id) != "undefined" && obj.id == "chk43") {
            value = value == "1" ? "0" : "1";
        }
    }
    // Checkbox standard
    else if (obj.type == "checkbox")
        value = obj.checked ? "1" : "0";
    // Bouton radio
    else if (obj.type == "radio") {
        var selectedVal = obj.id.split("_");
        value = selectedVal[1];
    }
    // Boutons XRM
    else if (obj.id == "buttonBold" || obj.id == "buttonItalic" || obj.id == "buttonUnderline") {
        value = getAttributeValue(obj, "dbvalue"); // Pour IE
    }
    // Valeur par défaut sur catalogue avancé, champ de saisie, user et date
    else if (obj.tagName == "INPUT" && getAttributeValue(obj, "dbv")) {
        value = getAttributeValue(obj, "dbv");
    }
    else if (obj.tagName == "SELECT") {
        var option = obj.options[obj.selectedIndex];
        var sParamCplt = getAttributeValue(option, "cplt");
        if (sParamCplt)
            subCaps = JSON.parse(sParamCplt);
    }
    else if (aDesc[0] == "3" && aDesc[1] == "15") {
        var parentElem = obj.parentNode;
        value = getAttributeValue(parentElem, "data-hpid");
        var userList = getAttributeValue(obj, "dbv");
        caps.ListUser = userList.length == 0 ? "0" : userList;
    }
    else if (obj.tagName == "A" && obj.className == "stepValue") {
        // Catalogues étape
        if (hasClass(obj.parentElement, "selectedValue"))
            value = getAttributeValue(obj, "dbv");
        else
            value = null;
    }
    else if (getAttributeValue(obj, "eaction") == "LNKSTEPCAT") {
        // Si on renvoie pas une valeur du catalogue étape mais le TD parent, c est que la valeur est vide
        value = "";
    }

    caps.AddProperty(aDesc[0], aDesc[1], value, advCat, bConfirm);

    if (subCaps && subCaps.ListProperties && subCaps.ListProperties.length > 0) {
        for (var i = 0; i < subCaps.ListProperties.length; i++) {
            caps.ListProperties.push(subCaps.ListProperties[i]);
        }
    }
    return caps;
};


nsAdmin.ormUpgrade = function () {
    var d = document.getElementById("rbFullUnicode");
    var allRadio = d.querySelectorAll("[name='rbFullUnicode']");
    for (var i = 0; i <= allRadio.length - 1; i++) {
        if (allRadio[i].checked) {
            nsAdmin.sendJson(allRadio[i], false, true, true);
            return;
        }
    }
}



nsAdmin.sendJson = function (obj, bConfirm, bSkipDescIDCheck, bSkipRefreshField) {
    if (typeof (bConfirm) === 'undefined' || !bConfirm)
        bConfirm = false;

    if (typeof (bSkipDescIDCheck) === 'undefined' || !bSkipDescIDCheck)
        bSkipDescIDCheck = false;

    var caps = nsAdmin.getCapsule(obj, bConfirm);
    caps.SetCapsuleConfirmed(bConfirm);

    // Le DescID n'est pas vérifié si cela est indiqué en paramètre (cas de la mise à jour d'un paramètre global)
    if (!bSkipDescIDCheck && !(caps.DescId > 0))
        return;

    // On détermine quel manager appeler pour MAJ la propriété en base grâce à cette clé
    var tabfld = getAttributeValue(obj, "tabfld");
    // Envoi de la capsule au manager ciblé
    var json = JSON.stringify(caps);
    var upd;

    if (tabfld == "config" || tabfld == "configdefault") {
        upd = new eUpdater("eda/Mgr/eAdminConfigManager.ashx", 1);


        upd.ErrorCallBack = function () {
            setWait(false);
        };

    }
    else if (tabfld == "cfgadv") {
        upd = new eUpdater("eda/Mgr/eAdminConfigAdvManager.ashx", 1);


        upd.ErrorCallBack = function () {
            nsAdmin.revertLastValue(obj);
            setWait(false);
        };


    }
    else {
        upd = new eUpdater("eda/Mgr/eAdminDescManager.ashx", 1);



        upd.ErrorCallBack = function () {
            setWait(false);
        };

    }

    upd.json = json;

    setWait(true);
    upd.send(function (oRes) {
        setWait(false);

        // Si sendJson fait appel à un manager qui ne renvoie pas de JSON en retour, on ne fait rien de plus
        if (typeof (oRes) == "undefined" || oRes == null || oRes == "")
            return;

        var res = JSON.parse(oRes);
        res = Object.assign({}, { UserErrorTitle: top._res_8240 }, res)
        if (res.NeedConfirm && !bConfirm) {


            var conf = top.eConfirm(1, res.UserErrorTitle, res.UserErrorMessage, "", 600, 225,
                function () {

                    nsAdmin.sendJson(obj, true, bSkipDescIDCheck, bSkipRefreshField);
                },
                function () {

                    conf.hide();

                    nsAdmin.revertLastValue(obj);
                },
                true,
                true);
        }
        else {
            if (!res.Success) {
                if (res.Criticity == 2)
                    top.eAlert(res.Criticity, top._res_92, res.UserErrorMessage);
                else
                    top.eAlert(1, top._res_416, res.UserErrorMessage);
                nsAdmin.revertLastValue(obj);
            }
            else {
                if (bSkipRefreshField != true) {
                    nsAdmin.afterUpdateField(obj);
                    if (res.Descid % 100 != 0) {
                        var paramPart = eTools.FindClosestElementWithClass(obj, "paramPart");
                        if (paramPart)
                            nsAdminField.refreshFieldPropertiesPart(res.Descid, paramPart.id);
                    }
                }

                if (res.Capsule != null && res.Capsule.ListProperties != null && res.Capsule.ListProperties.length > 0 && res.Capsule.ListProperties[0].Category == 3 && res.Capsule.ListProperties[0].Property == 15) {
                    nsAdmin.loadAdminModule(USROPT_MODULE_ADMIN_HOME_EXPRESS_MESSAGE);
                }


                if (obj.id == "chkCalendarEnabled") {
                    var btnGraphicMode = document.getElementById("graphicModePref");
                    if (getAttributeValue(obj, "chk") == "1") {
                        btnGraphicMode.style.display = "block";
                    }
                    else {
                        btnGraphicMode.style.display = "none";
                    }
                }
                else if (obj.id == "ddlRefPostalAddresses") {
                    // Le changement du fournisseur de la saisie prédictive nécessite le rechargement de la page pour inclure le script de BingMaps
                    var conf = top.eConfirm(1, top._res_201, top._res_8286, "", null, null,
                        function () {
                            top.window.location.reload(true);
                        },
                        function () {
                            conf.hide();
                        },
                        true,
                        true);
                }
                else if (obj.id == "ddlSyncOffice365MappingTab") {
                    // Le changement de l'onglet synchronisé doit recharger le signet "Paramètres" de l'extension
                    nsAdmin.loadAdminModule(USROPT_MODULE_ADMIN_EXTENSIONS_SYNCHRO_OFFICE365, "", { initialTab: "settings" });
                }
                //ALISTER Demande #64 862, Vérifie si l'on change un paramètre de groupe
                else if (obj.id == "rbGroupPolicyNone" || obj.id == "rbGroupPolicyEnabled" ||
                    obj.id == "ddlGroupPolicyRestrictions" || obj.id == "rbGroupPolicyGroupTreeDisplay" ||
                    obj.id == "rbGroupPolicyGroupTreeDisplay" || obj.id == "rbGroupPolicy_0" ||
                    obj.id == "rbGroupPolicyGroupTreeDisplay_0" || obj.id == "rbGroupPolicyGroupTreeDisplay_1") {
                    top.eAlert(3, top._res_6733, res.UserMessage);
                }

                if (obj.name == "admWebTabLabel") {
                    var nTab = getNumber(getAttributeValue(document.getElementById("navTabs"), "did"));
                    nsAdmin.loadContentModule(USROPT_MODULE_ADMIN_TAB, null, { tab: nTab });
                }
                else if (obj.name == "txtLicenseKey") {
                    nsAdmin.loadAdminModule(USROPT_MODULE_ADMIN_DASHBOARD);
                }

                //
                if (res.Result?.length > 0) {
                    res.Result.map(r => { return { dsc: r.Category + "|" + r.Property, value: r.Value }; })
                        .forEach(r => {
                            var adminInput = document.querySelector(`input[dsc='${r.dsc}']`);
                            if (adminInput) {
                                adminInput.value = r.value;
                            }
                        });
                }

            }
        }
    });
}

// Permet de revenir à la valeur précédente, lorsqu'une mise à jour n'a pas été faite
nsAdmin.revertLastValue = function (obj) {
    if (obj.type == "radio") {
        // Type radiobutton
        var radio;
        if (getAttributeValue(obj, "lastvalid") == "1") {
            radio = document.getElementById(obj.name + "_0");
        }
        else {
            radio = document.getElementById(obj.name + "_1");
        }

        if (radio)
            radio.checked = true;
    }
    else if (hasClass(obj, "chk")) {
        chgChk(obj);
    }
    else {
        obj.value = getAttributeValue(obj, "lastvalid");

    }
}

//Mise à jour de spécifs
nsAdmin.sendJsonSpecif = function (obj, cat) {

    var nSpecifId = getAttributeValue(obj, "fid");

    if (nSpecifId == "")
        return;

    var aDesc = getAttributeValue(obj, "dsc").split("|");
    if (aDesc.length < 2)
        return;

    var nTab = getNumber(getAttributeValue(document.getElementById("navTabs"), "did"));
    if (!nTab > 0)
        return;

    var caps = new Capsule(nTab);
    var category = aDesc[0];
    var field = aDesc[1];

    caps.SpecifId = nSpecifId;
    caps.AddProperty(category, field, obj.value, null);

    var json = JSON.stringify(caps);
    var upd;

    upd = new eUpdater("eda/Mgr/eAdminSpecifManager.ashx", 1);
    upd.json = json;
    upd.ErrorCallBack = function () {
        //alert('Erreur');
    };

    upd.send(
        function (oRes) {
            var oResult = JSON.parse(oRes);
            if (oResult.Success) {
                nsAdmin.afterUpdateField(obj);

                if (cat == nsAdmin.Category.SPECIFS) {
                    nsAdmin.updateNavBarSpecif(oResult);
                    nsAdmin.updateWebLinksBlock(oResult);
                }
            }
            else {
                top.setWait(false);
                eAlert(0, top._res_92, oResult.ErrorMessage, "");
            }
        });
}

nsAdmin.afterUpdateField = function (obj) {
    //Mémo valeur

    if (typeof obj.type == "string" && obj.type == "password")
        setAttributeValue(obj, "lastvalid", "");
    else if (obj.hasAttribute("chk"))
        setAttributeValue(obj, "lastvalid", getAttributeValue(obj, "chk"));
    else
        setAttributeValue(obj, "lastvalid", obj.value);

    if (obj.hasAttribute("dsc")) {
        var dsc = obj.getAttribute("dsc");
        var did = getAttributeValue(obj, "did");
        var bFormatBtn = (obj.id != "buttonBold" && obj.id != "buttonItalic" && obj.id != "buttonUnderline") ? false : true;

        // Cas : Nom de l'onglet ou du signet
        if (obj.id == "inputTabName" || obj.id == "txtTabName" || obj.id == "inputBkmName" || obj.id == "txtBkmName") {
            // Ciblage des éléments
            var divInput = document.getElementById("divInputTabName");
            var divLabel = document.getElementById("divLabelTabName");
            var input = document.getElementById("inputTabName");
            if (obj.id.indexOf("Bkm") != -1) {
                divInput = document.getElementById("divInputBkmName");
                divLabel = document.getElementById("divLabelBkmName");
                input = document.getElementById("inputTabName");
            }

            // Mise à jour du libellé
            var spanLabelTab = document.getElementById("labelTabName");
            var spanLabelBkm = document.getElementById("labelBkmName");
            // Si on affiche également les propriétés d'un signet : on préfixe le nom de l'onglet avec Onglet "" dans la navbar, puis celui du signet avec Signet "" (#51 135)
            if (spanLabelBkm != null) {
                // Edition du nom du signet
                if (obj.id.indexOf("Bkm") != -1) {
                    // Signet ""
                    if (spanLabelBkm.innerText)
                        spanLabelBkm.innerText = top._res_7587 + ' "' + obj.value + '"';
                    else
                        spanLabelBkm.textContent = top._res_7587 + ' "' + obj.value + '"'; // Pour FF
                }
                // Edition du nom de l'onglet
                else {
                    // Onglet ""
                    if (spanLabelTab.innerText)
                        spanLabelTab.innerText = top._res_264 + ' "' + obj.value + '"';
                    else
                        spanLabelTab.textContent = top._res_264 + ' "' + obj.value + '"'; // Pour FF
                }
            }
            // Si seul un onglet est en cours d'édition : la navbar mentionne uniquement le nom de l'onglet sans préfixe
            // 52 279 : il faut également mettre à jour le contenu du champ input utilisé pour le renommage de l'onglet/signet depuis la navBar (fil d'Ariane)
            else {
                SetText(spanLabelTab, obj.value);
                input.value = obj.value;
            }

            // Masquage du contrôle d'édition
            obj.className = "updatedField";
            window.setTimeout(function () {
                removeClass(obj, "updatedField");
                divInput.style.display = "none";
                divLabel.style.display = "block";
            }, 500);
        }

        // Autres cas/champs
        else {

            if (!bFormatBtn) {
                addClass(obj, "updatedField");
                window.setTimeout(function () {
                    removeClass(obj, "updatedField");
                }, 500);
            }

            // Si c'est un paramètre de rubrique, on met à jour la partie gauche
            if (getNumber(did) % 100 > 0) {

                // Reload sur les champs 
                var parentTable = findUp(obj, "TABLE");
                if (parentTable == null || (parentTable != null && parentTable.id != "tableFieldsList")) {

                    var params = { tab: nsAdmin.tab };

                    // Si champ système
                    if (parentTable == null) {
                        var divParent = findUpByClass(obj, "paramBlockContent");
                        if (getAttributeValue(divParent, "sys") == "1") {
                            params.sys = "1";
                        }
                    }
                    else if (parentTable.id == "tabAdminFileProp") {
                        params.sys = "1";
                    }

                    nsAdmin.loadContentModule(USROPT_MODULE_ADMIN_TAB, null, params);


                    if (obj.id == "ddlFieldType") {
                        nsAdminField.editFieldProperties(did);
                    }
                }
            }
            else if (getNumber(did) % 100 == 0) {
                if (obj.id == "ddlInterEVT"
                    || obj.id == "ddlInterPP"
                    || obj.id == "ddlInterPM"
                ) {

                    nsAdmin.loadAdminFile(nsAdmin.tab, "RelationsPart");
                }
            }
            if (obj.id == "admNbCols" || obj.id == "admBreakLine" || obj.name == "rbHTML") {

                nsAdmin.loadContentModule(USROPT_MODULE_ADMIN_TAB, null, { tab: nsAdmin.tab })

            }
        }

        // Si on a plusieurs fois le même champ, on recopie la valeur mise à jour sur les autres
        var sameFields = new Array();
        if (obj.id == "inputBkmName" || obj.id == "txtBkmName" || obj.id == "inputTabName" || obj.id == "txtTabName") {
            if (obj.id == "inputBkmName" || obj.id == "txtBkmName") {
                // Particularités pour le renommage de signet : modifier les noms dans la barre des signets
                var oBkmNameInBkmBar = document.getElementById("BkmHead_" + did);
                if (oBkmNameInBkmBar) {
                    if (oBkmNameInBkmBar.childNodes[0].innerText)
                        oBkmNameInBkmBar.childNodes[0].innerText = obj.value;
                    else
                        oBkmNameInBkmBar.childNodes[0].textContent = obj.value;
                }
            }
            // On recharge le menu droit pour actualiser la multitude de libellés obsolètes (y compris les infobulles);


            nsAdmin.loadMenuModule(USROPT_MODULE_ADMIN_TAB, { tab: getAttributeValue(document.getElementById("navTabs"), "did"), OpenedBlock: false });
        }
        else {
            if (getNumber(did) % 100 > 0) {
                sameFields = document.querySelectorAll("[dsc='" + dsc + "'][did='" + did + "']");
            }
            else if (obj.name != "admWebTabURL") {
                //TODO : vérifier dans quel cas cela est utile. Dans certains cas (signet web externe par ex, ce selecteur est trop large est provoque des effets non voulu)
                //Canceled by KHA : au retourjs  de la modification de l'url des signets web externe, les valeurs par défauts des rubriques sont affectées à la dite URL
                //sameFields = document.querySelectorAll("[dsc='" + dsc + "']");
            }
        }
        for (var i = 0; i < sameFields.length; i++) {
            if (sameFields[i] != null) {
                if (sameFields[i] != obj) {
                    if (!bFormatBtn)
                        sameFields[i].value = obj.value;
                    else
                        setAttributeValue(sameFields[i], "dbvalue", getAttributeValue(obj, "dbvalue"));
                }
            }
        }
    }
}

nsAdmin.setAction = function () {

    var menuGerard = document.getElementById("Gerard");

    //Recherche de tous les input de params
    var aDesc = menuGerard.querySelectorAll(".paramBlock [dsc]");
    for (var j = 0; j < aDesc.length; j++) {
        var obj = aDesc[j];

        //initialisation de lastvalid avec les valeurs initiales
        if (obj.hasAttribute("chk"))
            setAttributeValue(obj, "lastvalid", getAttributeValue(obj, "chk"));
        else
            setAttributeValue(obj, "lastvalid", obj.value);

        //Affectation onchange seulement si fonction pas deja défini
        if (obj.onchange == null) {
            //Affectation de la fonction en change - via retour de fonction anonyme pour que éviter les pb d'affectation dans une boucle
            obj.onchange = (function (elt) {
                var cat = getAttributeValue(elt, "dsc").split('|')[0] * 1;

                if (cat == nsAdmin.Category.SPECIFS) {
                    //Cas onglet web
                    return function (event) {

                        if (!nsAdmin.validParam(elt))
                            return;

                        nsAdmin.sendJsonSpecif(elt, cat);
                    };
                }
                else {
                    //Cas générique
                    return function (event) {
                        nsAdmin.onChangeGenericAction(elt);
                    };
                }
            })(obj);
        }


        if (obj.id == "buttonBold" || obj.id == "buttonItalic" || obj.id == "buttonUnderline") {
            obj.onclick = (function (elt) {
                return function (event) {
                    nsAdmin.updateLabelFormat(event, elt);
                };
            })(obj);
        }
    }

    // Permet de mettre à jour le champ texte à la sélection d'une valeur suggérée dans la liste
    var autocplInputs = menuGerard.querySelectorAll("input:not([list=''])");
    var field, input;
    for (var i = 0; i < autocplInputs.length; i++) {
        autocplInputs[i].addEventListener("input", function () {

            if (this.value) {
                var datalist = document.getElementById(getAttributeValue(this, "list"));
                if (datalist) {
                    var options = datalist.options;
                    for (var i = 0; i < options.length; i++) {
                        if (options[i].value === this.value && this.value != getAttributeValue(this, "lastvalid")) {
                            if (this.onchange)
                                this.onchange();
                            break;
                        }
                    }
                }
            }


        });

    }
}

nsAdmin.onChangeGenericAction = function (elt, bSkipRefreshField, bSkipDescIDCheck) {
    if (!nsAdmin.validParam(elt))
        return;

    //if (elt.id == "ddlFieldType" || elt.id == "txtLength") {
    //    nsAdmin.controlFieldChange(elt);
    //}
    //else {
    if (elt.getAttribute("chkAdvOptions") && elt.getAttribute("chkAdvOptions") == "1") {
        nsAdmin.updateSliderValue();
    }

    if (getAttributeValue(elt, "noupdate") != "1")
        nsAdmin.sendJson(elt, undefined, bSkipDescIDCheck, bSkipRefreshField);
    //}
}

nsAdmin.onCheckboxClick = function (elt) {
    var cat = getAttributeValue(elt, "dsc").split('|')[0] * 1;
    if (!nsAdmin.validParam(elt))
        return;

    if (getAttributeValue(elt, "noupdate") != "1")
        nsAdmin.sendJson(elt);
}

nsAdmin.updateLabelFormat = function (event, elt) {
    event.stopPropagation();
    if (getAttributeValue(elt, "dbvalue") == "0" || getAttributeValue(elt, "dbvalue") == "")
        setAttributeValue(elt, "dbvalue", "1");
    else
        setAttributeValue(elt, "dbvalue", "0");
    nsAdmin.sendJson(elt);
}

nsAdmin.reloadHome = function () {
    if (nsAdminStoreMenu)
        nsAdminStoreMenu.ResetSelectedFilters();

    oGridManager.resetStack();

    nsAdmin.goTabList(0);
}

///retire les scripts liées à l'admin
nsAdmin.clearAdminScript = function (type) {

    if (typeof type == "undefined")
        type = "ALL";

    top.clearHeader("ADMIN", type);
    top.clearHeader("ADMINFILE", type);
    top.clearHeader("ADMINLIST", type);
    top.clearHeader("ADMINPREF", type);
    top.clearHeader("ADMINSTOREFILE", type);
    top.clearHeader("ADMINUSERFILE", type);
    top.clearHeader("ADMINUSERLIST", type);
}

//Revient en mode user : retire les scripts/css admin et repasse sur le theme de l'utilisateur
nsAdmin.switchUserMode = function (fctCallBack, nTab, to) {
    // On créait un object pour renvoyer au setWait ou es ce qu'on veux aller
    var oParamGoTabList = {
        to: to,
        nTab: nTab,
        context: "eAdmin.switchUserMode"
    }

    if (top.nsAdmin) {
        top.nsAdmin.AdminMode = false;
        top.nsAdmin.CalledCheck = false;

        top.GridSystem.userMode();
    }

    var clbk = function () {
        switchDefaultThemeToUserTheme();

        top.setWait(false, undefined, undefined, isIris(top.getTabFrom()), oParamGoTabList);
    }

    if (typeof fctCallBack == "function") {
        clbk = function () {
            fctCallBack();
            switchDefaultThemeToUserTheme();
            top.setWait(false, undefined, undefined, isIris(top.getTabFrom()), oParamGoTabList);
        }
    }

    top.setWait(true, undefined, undefined, isIris(top.getTabFrom()), oParamGoTabList);
    nsAdmin.clearAdminScript();
    var upd = new eUpdater("eda/mgr/eAdminAccessPrefManager.ashx", 1);
    upd.addParam("action", "leave", "post");
    upd.send(clbk);
    upd.ErrorCallBack = function () {
        top.setWait(false, undefined, undefined, isIris(top.getTabFrom()), oParamGoTabList);
    }
}


nsAdmin.goTabList = function (nTab) {

    var to;
    if (nTab == 0) {
        to = 0
    } else {
        to = 1 // LIST
    }

    nsAdmin.switchUserMode(undefined, nTab, to);
    window.removeEventListener("beforeunload", nsAdmin.onUnloadBrowser)

    top.bReloadMRU = 1;

    // #56877
    // Pas besoin de recharger la navbar si on est en admin (hors admin table) ou si on revient sur une table en utilisation
    //SHA : backlog 1 638
    //if (nTab > 0 || nsAdmin.tab == 0)
    top.loadNavBar(to);

    top.goTabList(nTab, true, function () {

        var mainDiv = document.getElementById("mainDiv");
        mainDiv.innerHTML = "";


    }, null, true);
}

nsAdmin.loadFile = function (nTab, nFileId) {

    var to = 3 // FILE

    var callback = function () {
        top.bReloadMRU = 1;
        top.loadNavBar(to);
        top.loadFile(nTab, nFileId, 3, false, LOADFILEFROM.ADMIN);
    }

    nsAdmin.switchUserMode(callback, nTab, to);
}

nsAdmin.confRights = function (tab, from, types) {

    top.modalRights = new eModalDialog(top._res_6850, 0, "eda/eAdminRightsDialog.aspx", 1000, 652, "modalAdminRights");

    top.modalRights.noButtons = false;
    top.modalRights.addParam("tab", tab, "post");
    from = getNumber(from);
    if (from > 0)
        top.modalRights.addParam("from", from, "post");
    if (types != null)
        top.modalRights.addParam("types", types, "post");
    top.modalRights.addParam("iframeScrolling", "yes", "post");
    top.modalRights.onIframeLoadComplete = function () {
        nsAdmin.LoadFunctionsValues();
    }
    top.modalRights.show();

    top.modalRights.addButton(top._res_30, function () { top.modalRights.hide(); }, 'button-gray', null);

    nsAdmin.modalResizeAndMove(top.modalRights);
    top.modalRights.resizeToMaxWidth();

}

nsAdmin.LoadFunctionsValues = function () {

    var modalDoc = (top.modalRights) ? top.modalRights.getIframe().document : document;
    var table = modalDoc.getElementById("tableFilters");
    var labels = table.querySelectorAll("label[col='fct']");

    var ddl = modalDoc.getElementById("ddlListFct");
    var nbFirstOptionsToKeep = 2;
    while (ddl.length > nbFirstOptionsToKeep) {
        ddl.remove(nbFirstOptionsToKeep);
    }

    var hid = modalDoc.getElementById("hidFctSelected");
    var selectedFunction = hid.hasAttribute("value") ? hid.getAttribute("value") : "";

    var functions = new Array();

    for (var i = 0; i < labels.length; i++) {
        var sFunction = GetText(labels[i]);
        if (functions.indexOf(sFunction) > -1)
            continue;
        functions.push(sFunction)
    }

    if (selectedFunction != "" && functions.indexOf(sFunction) == -1)
        functions.push(selectedFunction)

    functions.sort();

    for (var i = 0; i < functions.length; i++) {
        var option = document.createElement("option");
        option.text = functions[i];
        option.value = option.text;
        ddl.add(option);
    }

    if (selectedFunction != "") {
        var options = ddl.querySelectorAll("option[value='" + selectedFunction.replace(/["\\']/g, '\\$&') + "']");
        if (options && options.length > 0)
            ddl.selectedIndex = options[0].index
    }
    else if (functions.length == 1)
        ddl.selectedIndex = nbFirstOptionsToKeep;
};

//update params field in Extension table
nsAdmin.updateParams = function (obj, idExtension) {
    var upd = new eUpdater("eda/Mgr/eAdminExtensionParamManager.ashx", 1);
    upd.addParam("params", obj.value, "post");
    upd.addParam("idExtension", idExtension, "post");
    upd.ErrorCallBack = function () {
        top.setWait(false)
    };
    top.setWait(true);
    upd.send(
        function (oRes) {
            top.setWait(false);
            try {
                var oResult = JSON.parse(oRes);
                if (!oResult.Success) {
                    eAlert(0, top._res_92, oResult.ErrorMessage, "");
                }
            }
            catch (ee) {
                eAlert(0, top._res_92, "", "");
            }
        });
};

//Ajouter une notif
nsAdmin.createNotif = function (tab, field, type) {

    // scale : mise à l'echelle de 0.80 % de la taille totale de l'ecran (top)
    // require : taille minimale pour la fenetre, pour que le rendu soit optimisé
    // tablet : taille minimale pour les tablettes
    var size = top.getWindowSize().scale(0.85).min({ w: 1024, h: 600 }).tablet({ w: 750 });


    var oAutomationFileModal = new eModalDialog(top._res_7479, 0, "eda/eAdminAutomationFileDialog.aspx", size.w, size.h, "modalAautomationFile");
    oAutomationFileModal.noButtons = false;
    oAutomationFileModal.addParam("tab", tab, "post");
    oAutomationFileModal.addParam("field", field, "post");
    oAutomationFileModal.addParam("id", 0, "post");
    oAutomationFileModal.addParam("type", type, "post");
    oAutomationFileModal.onIframeLoadComplete = function () { oAutomationFileModal.getIframe().oAutomation.init(); }
    oAutomationFileModal.show();
    oAutomationFileModal.addButton(top._res_29, function () { oAutomationFileModal.hide(); }, 'button-gray', null); // annuler
    oAutomationFileModal.addButton(top._res_28, function () {
        top.setWait(false);
        oAutomationFileModal.getIframe().oAutomation.validate(function (oRes) {
            var success = false;
            if (oRes)
                success = getXmlTextNode(oRes.getElementsByTagName("success")[0]) == "1";
            if (success) {
                var notifNumber = top.document.getElementById("buttonAdminListAutomations");
                if (notifNumber) {
                    var cpt = getAttributeValue(notifNumber, "rows"); cpt++;
                    setAttributeValue(notifNumber, "rows", cpt);
                    notifNumber.innerHTML = (top._res_7485 + "").replace("<COUNT>", cpt);
                }
            }
            oAutomationFileModal.hide();
        });
    }, 'button-green', null); // valider
}

// Affiche la fenêtre des automatismes filtré par le type = notification
nsAdmin.confShowAutomationList = function (tab, field, type) {

    var size = new eWinSize({ w: 950, h: 600 }).tablet({ w: 750 });

    var oAutomationListModal = new eModalDialog(top._res_7480, 0, "eda/eAdminAutomationListDialog.aspx", size.w, size.h, "modalAdminAutomationList");

    oAutomationListModal.noButtons = false;
    oAutomationListModal.addParam("tab", tab, "post");
    oAutomationListModal.addParam("field", field, "post");
    oAutomationListModal.addParam("type", type, "post");
    oAutomationListModal.addParam("width", size.w, "post");
    oAutomationListModal.addParam("height", size.h, "post");
    oAutomationListModal.show();
    oAutomationListModal.addButton(top._res_30, function () { oAutomationListModal.hide(); }, 'button-gray', null);
}

//Liste des comportements conditionnel
nsAdmin.listConditions = function (nDescId, nParentTab) {


    if (typeof (nDescId) == "undefined" || nDescId == null)
        nDescId = top.nsAdmin.tab;

    if (typeof (nParentTab) == "undefined" || nParentTab == null)
        nParentTab = top.nsAdmin.tab;

    var nPopupWidth = 900;

    //var bTabletMode = true;
    if (isTablet() && Number(size.w) < 1024)
        nPopupWidth = 750;

    // Liste des comportements conditionnels
    var oConditionListModal = new eModalDialog(top._res_7899, 0, "eda/eAdminConditionsListDialog.aspx", nPopupWidth, 600, "oConditionListModal");
    oConditionListModal.noButtons = false;
    oConditionListModal.addParam("descid", nDescId, "post");
    oConditionListModal.addParam("parenttab", nParentTab, "post");
    oConditionListModal.addParam("width", nPopupWidth, "post");
    oConditionListModal.addParam("height", 600, "post");
    oConditionListModal.show();

    oConditionListModal.addButton(top._res_30, function () { oConditionListModal.hide(); }, 'button-gray', null);
}


nsAdmin.confRules = function (tab) {
    //eTools.AlertNotImplementedFunction();
    var options = new Object();
    options.adminMode = true;
    filterListObjet(2, options);
}

//Gestion des traitements/droits conditionnels
nsAdmin.confConditions = function (nType, nTab, nParentTab, from) {
    //Le titre va être calculé dynamiquement
    modalConditions = new eModalDialog("", 0, "eda/eAdminConditionsDialog.aspx", 985, 625, "modalConditions");


    if (typeof (nTab) == "undefined" || nTab == null)
        nTab = nsAdmin.tab;

    if (typeof (nParentTab) == "undefined" || nParentTab == null || nParentTab == 0)
        nParentTab = nsAdmin.tab;


    if (typeof (from) == "undefined" || from == null)
        from = "MENU";
    else
        from = "LIST";

    modalConditions.noButtons = false;
    modalConditions.addParam("starttab", nsAdmin.tab, "post");
    modalConditions.addParam("tab", nTab, "post");
    modalConditions.addParam("parenttab", nParentTab, "post");
    modalConditions.addParam("from", from, "post");


    modalConditions.addParam("type", nType, "post");
    modalConditions.show();
    modalConditions.addButton(top._res_30, function () { modalConditions.hide(); }, 'button-gray', null);
    modalConditions.addButton(top._res_28,
        function () {
            if (modalConditions.getIframe())
                modalConditions.getIframe().nsConditions.validateConditions(nTab, from);
            else {

            }

        },
        'button-green', null);
}

nsAdmin.getLevelOutputText = function (value) {
    switch (value) {
        case "1": return top._res_199 + " 1";
        case "2": return top._res_199 + " 2";
        case "3": return top._res_199 + " 3";
        case "4": return top._res_199 + " 4";
        case "5": return top._res_199 + " 5";
        case "6": return top._res_194;
        case "7": return top._res_7414;
        case "8": return top._res_7559;
        case "9": return top._res_8324;
    }
    return top._res_7415;
}


// Obtient le libellé à mettre dans la colonne "Utilisateurs et groupes" lorsqu'aucun utilisateur n'est sélectionné : "Aucun utilisateur" ou "Tous les utilisateurs"
nsAdmin.getNullUserLabel = function (uvalue) {
    var label = "";

    if (!uvalue || uvalue.length == 0) {
        label = top._res_513;
    } else {
        label = top._res_6869
    }

    return label;
}




nsAdmin.showUsersCatInIP = function (element) {
    if (element.getAttribute("active") == "1") {
        modalUserCatIP = new eModalDialog(top._res_246, 0, "eCatalogDialogUser.aspx", 550, 610);
        modalUserCatIP.addParam("iframeId", modalUserCatIP.iframeId, "post");
        modalUserCatIP.ErrorCallBack = function () { setWait(false); }
        modalUserCatIP.addParam("multi", "1", "post");

        var topModal = document.getElementById(modalIP.iframeId);
        var topModalContent = topModal.contentDocument || topModal.contentWindow.document;
        var oTarget = topModalContent.getElementById("lbIPUsers");
        modalUserCatIP.addParam("selected", oTarget.getAttribute("ednvalue"), "post");

        modalUserCatIP.addParam("modalvarname", "modalUserCatIP", "post");
        modalUserCatIP.show();

        modalUserCatIP.addButton(top._res_29, function () {
            modalUserCatIP.hide();
        }, "button-gray", null, null, true);
        modalUserCatIP.addButton(top._res_28, onUsersCatIPOk, "button-green");
    }
}


function onUsersCatIPOk() {
    var strReturned = modalUserCatIP.getIframe().GetReturnValue();
    var vals = strReturned.split('$|$')[0];
    var libs = strReturned.split('$|$')[1];
    var topModal = document.getElementById(modalIP.iframeId);
    var topModalContent = topModal.contentDocument || topModal.contentWindow.document;
    var oTarget = topModalContent.getElementById("lbIPUsers");

    if (libs == "")
        libs = top._res_141;
    oTarget.value = libs;
    oTarget.setAttribute("title", libs);
    oTarget.setAttribute("ednvalue", vals);
    modalUserCatIP.hide();
}

// Ajout des événements
nsAdmin.addTitleClickEvent = function (idPart) {
    var headers = document.querySelectorAll("#" + idPart + " header");
    for (var i = 0; i < headers.length; i++) {



        var myHeader = headers[i];

        myfct = (function (header) {
            return function () {

                var paramPart = header.parentElement;
                var iShowMode = nsAdmin.showHidePart(header);

                if (paramPart.id != "") {
                    nsAdmin.clearSelectedParts();

                    if (iShowMode == 1) {
                        switch (paramPart.id) {
                            case "partHeaderFields":
                                eTools.SetClassName(document.getElementById("ftp_" + nsAdmin.tab), "selectedArea");
                                break;
                            case "partFileFields":
                                eTools.SetClassName(document.getElementById("ftm_" + nsAdmin.tab), "selectedArea");
                                eTools.SetClassName(document.getElementById("ftdbkm_" + nsAdmin.tab), "selectedArea");
                                break;
                            case "partNbColumns":
                                eTools.SetClassName(document.getElementById("adminFieldsContent"), "selectedArea");
                                eTools.SetClassName(document.getElementById("ruleGrips"), "selectedArea");
                                break;
                            case "partWebBkm":
                                eTools.SetClassName(document.getElementById("bkmTab_" + nsAdmin.tab), "selectedArea");
                                break;
                            case "partWebTab":
                                eTools.SetClassName(document.getElementById("admWebTab"), "selectedArea");
                                break;
                            case "partBelonging":
                                eTools.SetClassName(document.getElementById("fth_" + nsAdmin.tab), "selectedArea");
                                eTools.SetClassName(document.getElementById("tabAdminFileProp"), "selectedArea");
                                break;
                        }
                    }

                    // Si titre cliqué = "Appartenance et traçabilité", on affiche/masque le bloc correspondant
                    if (paramPart.id == "partBelonging") {
                        var ownerTitle = document.getElementById("ownerTitle");
                        ownerTitle.click();
                    }
                }
            }
        }
            (myHeader)
        );


        removeAllListener(myHeader, "click");
        setEventListener(myHeader, 'click', myfct, false);
    }

}


// Ajout des événements sur le contenu de la fiche Admin
nsAdmin.addContentEventListeners = function () {
    var rule = document.getElementById("ruleGrips");
    var bar = document.getElementById("adminFieldsContent");

    if (rule) {
        rule.addEventListener("mouseover", function () {
            eTools.SetClassName(rule, "selectedArea");
            eTools.SetClassName(bar, "selectedArea");
        });
        rule.addEventListener("mouseout", function () {
            eTools.RemoveClassName(rule, "selectedArea");
            eTools.RemoveClassName(bar, "selectedArea");
        });
    }

    if (bar) {
        bar.addEventListener("mouseover", function () {
            eTools.SetClassName(rule, "selectedArea");
            eTools.SetClassName(bar, "selectedArea");
        });
        bar.addEventListener("mouseout", function () {
            eTools.RemoveClassName(rule, "selectedArea");
            eTools.RemoveClassName(bar, "selectedArea");
        });
    }

    var ownerTitle = document.getElementById("ownerTitle");
    if (ownerTitle) {
        ownerTitle.addEventListener("click", nsAdmin.ToggleAdminFileProp);
    }
}

nsAdmin.ToggleAdminFileProp = function () {
    var table = document.getElementById("tabAdminFileProp");
    var link = document.getElementById("btnSystemFields");
    if (table.getAttribute("data-active") == "1") {
        table.setAttribute("data-active", "0");
        if (link != null)
            link.className = link.className.replace("activeLinkButton", "").trim();
    }
    else {
        table.setAttribute("data-active", "1");
        if (link != null)
            link.className = (link.className + " " + "activeLinkButton").trim();
    }
}

// Désélection des zones
nsAdmin.clearSelectedParts = function () {
    var selectedAreas = document.querySelectorAll("table.selectedArea");
    for (var i = 0; i < selectedAreas.length; i++) {
        eTools.RemoveClassName(selectedAreas[i], "selectedArea");
    }
    eTools.RemoveClassName(document.getElementById("ruleGrips"), "selectedArea");
}

nsAdmin.highLightSpecif = function (specifId) {
    //retire la spécif sélectionnée précédement et active celle clickée
    var arrAllWebTab = [].slice.call(document.getElementById("ulWebTabs").querySelectorAll("li"));
    arrAllWebTab.forEach(function (maTab) {
        //retire la classe
        eTools.RemoveClassName(maTab, "selectedTab");

        //met la classe si c'est l'id clické. Pour l'instant, eTools.SetClassName ne vérifie pas si la classe existe déjà, contrairement à "addClass", aussi dans etools.js
        // il faudrait unifier les fct addClass/eTools.SetClassName et removeClass et eTools.RemoveClassName qui font pratiquement la même chose.
        // en attendant, on remove systématiquement la classe même si c'est pour l'adder aprés (si on click plusieurs fois de suite sur le même tab)
        if (getAttributeValue(maTab, "fid") == specifId + "")
            eTools.SetClassName(maTab, "selectedTab");
    });
}

nsAdmin.accessSecurityGenericOptionChange = function (element) {
    // TODO: à mutualiser puis remplacer avec sendJson si cette dernière est modifiée pour accepter des MAJ non liées à un DescID ?
    //nsAdmin.sendJson(element, false);

    var cat = getAttributeValue(element, "cat");
    var prop = getAttributeValue(element, "prop");
    var tabfld = getAttributeValue(element, "tabfld");
    var value = "";

    // Valeur stockée dans un champ de saisie
    if (element.tagName == "INPUT")
        value = element.value;
    // Valeur stockée dans une combobox
    if (element.tagName == "SELECT")
        value = element.options[element.selectedIndex].value;
    // Valeur stockée dans une case à cocher XRM
    else {
        var chk = element.querySelector(".rChk");
        if (chk != null && chk.getAttribute("chk") != null)
            value = getAttributeValue(chk, "chk");
    }
    var bConfirm = false;
    var advCat = "0"; // non utilisé pour la mise à jour de valeurs de CONFIG/PREF/etc. Ne pas valoriser à null (fait échouer la désérialisation)

    // Préparation de la capsule
    var caps = new Capsule(0);
    caps.AddProperty(cat, prop, value, advCat, bConfirm);

    var upd;
    if (tabfld == "config" || tabfld == "configdefault") {
        upd = new eUpdater("eda/Mgr/eAdminConfigManager.ashx", 1);
    }
    else if (tabfld == "cfgadv") {
        upd = new eUpdater("eda/Mgr/eAdminConfigAdvManager.ashx", 1);
    }
    else {
        upd = new eUpdater("eda/Mgr/eAdminDescManager.ashx", 1);
    }
    var json = JSON.stringify(caps);

    upd.json = json;
    upd.ErrorCallBack = function () {
        //alert('Erreur');
        setWait(false);
    };
    setWait(true);

    var afterDurationUpdateFct = function () { setWait(false); }

    // TODO: à câbler lors eAdminConfigManager.ashx sera mis à jour pour envoyer un retour JSON (oRes non vide)
    /*
    var afterDurationUpdateFct = function (oRes) {
        setWait(false);
        var res = JSON.parse(oRes);

        if (res.NeedConfirm && !bConfirm) {
            var conf = top.eConfirm(1, "", res.UserErrorMessage, "", null, null,
                function () {
                    nsAdmin.sendJson(obj, true);
                },
                function () {
                    conf.hide();
                    element.selectedIndex = Number(getAttributeValue(element, "currentselection"));
                },
                true,
                true);
        }
        else {
            if (!res.Success) {
                if (res.Criticity == 2)
                    top.eAlert(res.Criticity, top._res_92, res.UserErrorMessage);
                else
                    top.eAlert(1, top._res_416, res.UserErrorMessage);
                element.selectedIndex = Number(getAttributeValue(element, "currentselection"));
            }
            else {
            }
        }
    }
    */

    upd.send(afterDurationUpdateFct);
}


nsAdmin.accessSecurityIPAdd = function () {
    return nsAdmin.accessSecurityIPShowDialog(-1, '');
}
nsAdmin.accessSecurityIPEditLabel = function (ipId) {
    return nsAdmin.accessSecurityIPShowDialog(ipId, 'label');
}
nsAdmin.accessSecurityIPEditAddress = function (ipId) {
    return nsAdmin.accessSecurityIPShowDialog(ipId, 'address');
}
nsAdmin.accessSecurityIPEditMask = function (ipId) {
    return nsAdmin.accessSecurityIPShowDialog(ipId, 'mask');
}
nsAdmin.accessSecurityIPEditUserGroups = function (ipId) {
    return nsAdmin.accessSecurityIPShowDialog(ipId, 'usergroups');
}
nsAdmin.accessSecurityIPEditLevel = function (ipId) {
    return nsAdmin.accessSecurityIPShowDialog(ipId, 'level');
}

nsAdmin.accessSecurityIPSetAccessEnabled = function (checkbox) {
    var bChecked = false;

    if (getAttributeValue(checkbox, "chk") == "1")
        bChecked = true;

    // Clic sur une case à cocher cochée = on décoche la case = on désactive la fonctionnalité
    var bDisable = bChecked;


    var upd = new eUpdater("eda/Mgr/eAdminAccessSecurityIPManager.ashx", 0);
    upd.ErrorCallBack = function () { };
    upd.addParam("action", "setIPAccessEnabled", "post");
    upd.addParam("setipaccessvalue", bDisable ? "0" : "1", "post");
    upd.send(function () { nsAdmin.loadAdminModule(USROPT_MODULE_ADMIN_ACCESS_SECURITY); });
}

nsAdmin.accessSecurityIPShowDialog = function (ipId, fieldToFocus) {
    modalIP = new eModalDialog("Adresses IP autorisées", 0, "eda/eAdminAccessSecurityIPDialog.aspx", 600, 320, "modalIPDialog");
    modalIP.addParam("ipId", ipId, "post");
    modalIP.addParam("fieldToFocus", fieldToFocus, "post");


    modalIP.addScript("../eda/scripts/eAdminRights");
    modalIP.addScript("eTools");
    modalIP.noButtons = false;
    modalIP.show();

    modalIP.addButton(top._res_29, function () { modalIP.hide(); }, 'button-gray', null);
    modalIP.addButton(top._res_28, function () { nsAdmin.accessSecurityIPUpdate(ipId > -1 ? "update" : "add"); }, 'button-green', null);
    if (ipId > -1)
        modalIP.addButton(top._res_19, function () { nsAdmin.accessSecurityIPUpdate("delete"); modalIP.hide(); }, "button-red"); // Supprimer
}

nsAdmin.accessSecurityIPUpdate = function (action, listTid) {
    var modalDocument = modalIP.getIframe().document;

    var nIPId = modalDocument.getElementById("ipId").value;
    var sIPLabel = modalDocument.getElementById("inputIPLabel").value;
    var sIPAddress = modalDocument.getElementById("inputIPAddress").value;
    var sIPMask = modalDocument.getElementById("inputIPMask").value;
    var bLevel = getAttributeValue(modalDocument.getElementById("chkLevel"), "chk");
    var bUsers = getAttributeValue(modalDocument.getElementById("chkGroupsAndUsers"), "chk");
    var lValue = modalDocument.getElementById("ddlIPLevel").value;
    var uValue = getAttributeValue(modalDocument.getElementById("lbIPUsers"), "ednvalue");
    var nPermId = modalDocument.getElementById("permId").value;

    if (lValue == "0" && uValue == "0") {
        // à remplacer par un eConfirm
        alert("Aucun choix sélectionné");
    }
    else {
        var upd = new eUpdater("eda/Mgr/eAdminAccessSecurityIPManager.ashx", 0);
        upd.ErrorCallBack = function () { };
        upd.addParam("action", action, "post");
        upd.addParam("ipid", nIPId, "post");
        upd.addParam("iplabel", sIPLabel, "post");
        upd.addParam("ipaddress", sIPAddress, "post");
        upd.addParam("ipmask", sIPMask, "post");
        upd.addParam("permid", nPermId, "post");
        upd.addParam("updatelevel", bLevel, "post");
        upd.addParam("updateusers", bUsers, "post");
        upd.addParam("lvalue", lValue, "post");
        upd.addParam("uvalue", uValue, "post");
        upd.send();

        modalIP.hide();

        nsAdmin.loadAdminModule(USROPT_MODULE_ADMIN_ACCESS_SECURITY);
    }
}



/**
 * Permet de masquer tout ou partie des options d'une dropdown, suivant
 * une chaine de caractère passée en paramètre.
 * @param {any} jsDuplication le JSON a parser.
 * @param {any} ItemSelect le select dont on veut masquer les éléments.
 * @param {any} stringMask le masque de saisie.
 */
nsAdmin.FnMasqueElementsSelectByValue = function (jsDuplication, ItemSelect, stringMask) {

    var fid = getAttributeValue(document.querySelector(".fileDiv"), "fid");
    var did = getAttributeValue(document.querySelector(".fileDiv"), "did");

    ItemSelect.options.length = 0;
    /** Premiere option vide */
    //var option = document.createElement('option');
    //option.text = top._res_8118;
    //option.value = "0";
    //ItemSelect.add(option)



    Object.keys(jsDuplication).forEach(function (clef) {
        if (clef.toLowerCase().search(stringMask.toLowerCase()) > -1) {
            option = document.createElement('option');
            option.text = jsDuplication[clef];
            option.value = clef;
            option.selected = (clef == stringMask + "_" + (did == enumPages.XRMHOMEPAGE ? fid : did));
            option.classList.add("eTitleClone");
            ItemSelect.add(option);
        }
    });
}



/**
 * Crée un type inputà l'aide des éléments envoyés en paramètre.
 * @param {string} id l'identifiant de l'input
 * @param {string} type le type de l'input
 * @param {string} nom le nom de l'input
 * @param {string} value la valeur que prendra l'élément. !Attention! attribut value.
 * @param {string} action une chaine représentant l'action a effectuer.
 * @param {function} callback fonction a appeler pour l'action.
 * @returns {input} l'input voulu.
 * */
nsAdmin.FnDuplicateGridCreateInput = function (id, type, name, value, action, callback) {
    var iInput = document.createElement("input");
    iInput.id = id;
    iInput.type = type;

    if (name)
        iInput.name = name;

    iInput.value = value;

    /** On ajoute une action avec la fonction ajoutée en paramètre. */
    if (action) {
        iInput.addEventListener(action, function () { callback(); })
    }

    return iInput;
}

/**
 * Crée un type input à l'aide des éléments envoyés en paramètre, et ajoute une valeur avec une attribut value.
 * @param {string} htmlfor l'identifiant de l'input pour le label
 * @param {string} text le texte du label
 * @returns {Label} le label voulu.
 * */
nsAdmin.FnDuplicateGridLabel = function (htmlfor, text) {
    var lbl = document.createElement("label");

    lbl.htmlFor = htmlfor;
    lbl.innerHTML = text;
    lbl.classList.add("eTitleClone");

    return lbl;
}

/**
 * Fonction qui va permettre de dupliquer une grille passée en paramètre.
 * @param Grid {Grille} la grille a dupliquer.
 * */
nsAdmin.FnDuplicateGrid = function (Grille) {

    addCss("eAdminMenu", "MODALGRID");
    addCss("eMain", "MODALGRID");
    addCss("eButtons", "MODALGRID");
    addCss("eControl", "MODALGRID");
    addCss("eAdminMenu", "MODALGRID");

    var jsDuplication;
    var did = getAttributeValue(document.querySelector(".fileDiv"), "did");

    var modalConfigFile = new eModalDialog(top._res_2438, 10, null, 800, 300);
    modalConfigFile.noButtons = false;
    modalConfigFile.hideMaximizeButton = true;
    var divContent = document.createElement("div");
    divContent.id = "dvCntCloneGrid";

    divContent.className = "edaAcaDuplicate";

    var h3Title = document.createElement("h3");
    h3Title.innerHTML = top._res_2439
    h3Title.classList.add("edaMpTtListTab");
    h3Title.classList.add("eTitleClone");


    /** Checkbox de l'activation de la nouvelle ergonomie. */
    var divRadClnGrid = document.createElement("div");
    divRadClnGrid.id = "dvSwitchCloneGrid";
    divRadClnGrid.classList.add("subtitle");

    var iRadClnGridHP = nsAdmin.FnDuplicateGridCreateInput("radCloneGridHP", "radio", "radCloneGrid", "XRMHOMEPAGE", "click", function () {
        nsAdmin.FnMasqueElementsSelectByValue(jsDuplication, selClnGrid, iRadClnGridHP.value);
    });

    var iRadClnGridDC = nsAdmin.FnDuplicateGridCreateInput("radCloneGridDC", "radio", "radCloneGrid", "DESC", "click", function () {
        nsAdmin.FnMasqueElementsSelectByValue(jsDuplication, selClnGrid, iRadClnGridDC.value);
    });

    /** Si on est sur les accueils, on sélectionne les pages d'accueil, sinon les onglets ... */
    if (did == 115200)
        iRadClnGridHP.checked = "checked";
    else
        iRadClnGridDC.checked = "checked";

    iRadClnGridDC.classList.add("eCloneRad");
    iRadClnGridHP.classList.add("eCloneRad");

    lblRadClnGridHP = nsAdmin.FnDuplicateGridLabel("radCloneGridHP", top._res_8071);
    lblRadClnGridDC = nsAdmin.FnDuplicateGridLabel("radCloneGridDC", top._res_217);

    var spanRadClnGridHP = document.createElement("span");
    spanRadClnGridHP.classList.add("eCloneflist");

    var spanRadClnGridDC = document.createElement("span");
    spanRadClnGridDC.classList.add("eCloneflist");


    spanRadClnGridHP.appendChild(iRadClnGridHP);
    spanRadClnGridHP.appendChild(lblRadClnGridHP);
    spanRadClnGridDC.appendChild(iRadClnGridDC);
    spanRadClnGridDC.appendChild(lblRadClnGridDC);


    var divSelClnGrid = document.createElement("div");
    divSelClnGrid.id = "divSelClnGrid";
    divSelClnGrid.classList.add("field");

    var selClnGrid = document.createElement("select");
    selClnGrid.id = "selClnGrid";

    var spanSelClnGrid = document.createElement("span");
    spanSelClnGrid.classList.add("eCloneflist");
    spanSelClnGrid.appendChild(selClnGrid);

    var divTxtClnGridName = document.createElement("div");
    divTxtClnGridName.id = "divTxtClnGridName";
    divTxtClnGridName.classList.add("field");

    var txtCloneGridName = nsAdmin.FnDuplicateGridCreateInput("txtCloneGridName", "text", "", top._res_7593 + " " + Grille.parentElement.previousElementSibling.innerHTML);
    txtCloneGridName.classList.add("eCloneIpt");

    var spanTxtCloneGridName = document.createElement("span");
    spanTxtCloneGridName.classList.add("eCloneflist");
    spanTxtCloneGridName.appendChild(txtCloneGridName);

    upd = new eUpdater("eda/mgr/eAdminListeOngletsEtAccueil.ashx", 1);
    upd.send(function (oRes) {
        jsDuplication = JSON.parse(oRes);
        nsAdmin.FnMasqueElementsSelectByValue(jsDuplication, selClnGrid, (did == enumPages.XRMHOMEPAGE) ? iRadClnGridHP.value : iRadClnGridDC.value);
    });


    var spanRadClnGridTxt = document.createElement("span");
    spanRadClnGridTxt.innerHTML = top._res_2442;
    spanRadClnGridTxt.classList.add("edaCloneLabel");

    divRadClnGrid.appendChild(spanRadClnGridTxt);
    divRadClnGrid.appendChild(spanRadClnGridHP);
    divRadClnGrid.appendChild(spanRadClnGridDC);

    var spanLabSelClnGrid = document.createElement("span");
    spanLabSelClnGrid.innerHTML = top._res_2450;
    spanLabSelClnGrid.classList.add("edaCloneLabel");

    divSelClnGrid.appendChild(spanLabSelClnGrid);
    divSelClnGrid.appendChild(spanSelClnGrid);

    var spantxtCloneGridNameLbl = document.createElement("span");
    spantxtCloneGridNameLbl.innerHTML = top._res_2443;
    spantxtCloneGridNameLbl.classList.add("edaCloneLabel");

    divTxtClnGridName.appendChild(spantxtCloneGridNameLbl);
    divTxtClnGridName.appendChild(spanTxtCloneGridName);

    divContent.appendChild(h3Title);
    divContent.appendChild(document.createElement("br"));
    divContent.appendChild(document.createElement("br"));
    divContent.appendChild(divRadClnGrid);
    divContent.appendChild(document.createElement("br"));
    divContent.appendChild(document.createElement("br"));
    divContent.appendChild(divSelClnGrid);
    divContent.appendChild(document.createElement("br"));
    divContent.appendChild(document.createElement("br"));
    divContent.appendChild(divTxtClnGridName);

    modalConfigFile.setElement(divContent);

    modalConfigFile.show();

    modalConfigFile.addButton(top._res_30, function () {
        modalConfigFile.hide();
    }, 'button-gray', null);

    /**
     * Si on clique sur ok, on enregistre les données via le
     * manager qui permet d'enregistrer dans descadv.
     */
    modalConfigFile.addButton(top._res_28, function () {

        top.setWait(true);

        if (selClnGrid.selectedIndex < 0) {
            eAlert(0, "", top._res_8118);
            selClnGrid.focus();
            top.setWait(false);
            return;
        }

        var tabTabbar = selClnGrid.value.split("_");

        if (tabTabbar.length < 2) {
            eAlert(0, "", top._res_2440);
            top.setWait(false);
            return;
        }

        if (txtCloneGridName.value.trim() == "") {
            eAlert(0, "", top._res_2440);
            txtCloneGridName.focus();
            top.setWait(false);
            return;
        }

        var updGrille = new eUpdater("eda/mgr/eAdminDuplicateGrid.ashx", 1);

        updGrille.addParam("grid", Grille.getAttribute("gid"), "post");


        if (tabTabbar[0].toLowerCase() != "xrmhomepage")
            updGrille.addParam("tab", tabTabbar[tabTabbar.length - 1], "post");
        else {
            updGrille.addParam("tab", enumPages.XRMHOMEPAGE, "post");
            updGrille.addParam("file", tabTabbar[tabTabbar.length - 1], "post");
        }

        updGrille.addParam("title", txtCloneGridName.value.trim(), "post");

        updGrille.send(function (oRes) {

            var result;

            try {
                result = JSON.parse(oRes);
            } catch (e) {
                eAlert(0, "", e.message);
            }

            if (!result.Success) {
                eAlert(result.Criticity, top._res_2452, result.ErrorMessage);
                return;
            }

            clearHeader("MODALGRID", "CSS");
            eAlert(4, "", top._res_2451, '', null, null, function () {

                if (tabTabbar[0].toLowerCase() == "xrmhomepage") {

                    var id = updGrille.getParam("file");
                    nsAdmin.loadNavBar('ADMIN_HOME_XRM_HOMEPAGE',
                        {
                            'tab': enumPages.XRMHOMEPAGE,
                            'pageId': id,
                            'gridId': 0,
                            'gridLabel': ""
                        });

                    if (typeof (oAdminGridMenu) != "undefined")
                        oAdminGridMenu.loadMenu(enumPages.XRMHOMEPAGE, id, null, function (res) { });

                    var winSize = top.getWindowSize();

                    var oFile = getFileUpdater(enumPages.XRMHOMEPAGE, id, 2, true);
                    oFile.addParam("part", 10, "post");
                    oFile.addParam("height", winSize.h, "post");
                    oFile.send(function (oRes) {

                        var divId = "mainDiv";
                        var mainDiv = document.getElementById(divId);

                        if (mainDiv != null)
                            mainDiv.innerHTML = oRes;

                        nsAdminFile.browser = new getBrowser();
                        nsAdmin.setDDHandlers();

                        top.setWait(false);

                    });
                }
                else
                    nsAdmin.loadAdminFile(updGrille.getParam("tab"))
            });


            modalConfigFile.hide();
            top.setWait(false);
        });


    }, 'button-green', null);
}

///Edition des propriétes d'un onglet web
nsAdmin.editWebTabProperties = function (tab, specifId) {

    var activeTd = document.querySelectorAll(".mTabFile .active, #tabAdminFileProp .active");
    for (var i = 0; i < activeTd.length; i++)
        selectCell(activeTd[i], false, "active");


    //Load Web Tab Properties
    var upd = new eUpdater("eda/mgr/eWebTabManager.ashx", 1);
    upd.addParam("action", nsAdmin.WebTabManagerAction.GETINFOS, "post");
    upd.addParam("tab", tab, "post");
    upd.addParam("id", specifId, "post");
    var ednType = getAttributeValue(document.getElementById("fileDiv_" + tab), "ednType")
    if (ednType)
        upd.addParam("ednType", ednType, "post");

    upd.send(

        //Maj du tab
        // oRes contient le html de l'admin des propriété du webtab
        function (oRes) {
            nsAdmin.refreshTab(2, oRes);

            //Affecte/réaffecte les handler de drag&drop
            nsAdmin.setDDHandlers();

            nsAdmin.highLightSpecif(specifId);
        }

    );
}


/// Supprime le sous-onglet web
nsAdmin.deleteWebTab = function (tab, specifId) {

    var webTabLabel = "";
    var webTabSpan = document.querySelector("#ulWebTabs li[fid='" + specifId + "'] span");
    if (webTabSpan)
        webTabLabel = GetText(webTabSpan);

    eAdvConfirm({
        'criticity': eAdvConfirm.CRITICITY.MSG_EXCLAM,
        'title': top._res_806,
        'message': (top._res_7636 + "").replace("<WEBTAB>", webTabLabel), //"Suppression de sous-onglet "<WEBTAB>""
        'details': top._res_7637, // "Etes-vous supprimer le sous-onglet  ?"       
        'cssOk': 'button-red',
        'cssCancel': 'button-green',
        'resOk': top._res_19,
        'resCancel': top._res_29,
        'okFct': function () { nsAdmin.deleteWebTabConfirmed(tab, specifId); }
    });
}

/// L'utilisateur à confirmé la suppression
nsAdmin.deleteWebTabConfirmed = function (tab, specifId) {

    //Load Web Tab Properties
    var upd = new eUpdater("eda/mgr/eWebTabManager.ashx", 1);
    upd.addParam("action", nsAdmin.WebTabManagerAction.DELETE, "post");
    upd.addParam("tab", tab, "post");
    var ednType = getAttributeValue(document.getElementById("fileDiv_" + tab), "ednType")
    if (ednType)
        upd.addParam("ednType", ednType, "post");

    upd.addParam("id", specifId, "post");
    upd.send(nsAdmin.deleteWebTabReturn);
}

/// Retour serveur après suppression ou pas
nsAdmin.deleteWebTabReturn = function (oRes) {

    var result = JSON.parse(oRes);
    if (!result.Success) {
        eAlert(2, top._res_806, result.ErrorMessage, "", 500, 200, null);
    } else {
        var webTab = document.querySelector("li[fid='" + result.SpecifId + "']");
        webTab.parentNode.removeChild(webTab);

        nsAdmin.loadMenuModule(USROPT_MODULE_ADMIN_TAB, { tab: result.Tab, OpenedBlock: false });

        nsAdmin.setDDHandlers();
    }
};

nsAdmin.editGridProperties = function (gridId, evt) {


    var tabContainer = document.getElementById("admWebTab");
    var parentTab = getAttributeValue(tabContainer, "pTab");
    if (parentTab == "")
        parentTab = 0;

    var parentFileId = getAttributeValue(tabContainer, "pfId");
    if (parentFileId == "")
        parentFileId = 0;

    // Container des sous-onglets de type grille
    var tabsEntries = document.getElementById("ulWebTabs");
    var gridLabel = tabsEntries.querySelector("li[fid='" + gridId + "'] > span");
    if (gridLabel == null)
        return;

    if (parentTab == 115200) {
        if (parentFileId > 0) {
            // Rechargement du fil d'ariane
            nsAdmin.loadNavBar(USROPT_MODULE_ADMIN_HOME_XRM_HOMEPAGE_GRID,
                {
                    'tab': nGlobalActiveTab,
                    'pageId': parentFileId,
                    'gridId': gridId,
                    'gridLabel': GetText(gridLabel),
                });
        }

    } else {
        // Rechargement du fil d'ariane
        nsAdmin.loadNavBar(USROPT_MODULE_ADMIN_TAB_GRID,
            {
                'tab': nGlobalActiveTab,
                'extensionFileId': gridId,
                'extensionLabel': GetText(gridLabel),
            });
    }

    // reset de la barre des sous-onglet
    Array.prototype.slice.call(tabsEntries.querySelectorAll("li[active='1']"))
        .forEach(function (entry) { setAttributeValue(entry, "active", "0"); });

    // mise en surbrillance de l'entrée correspondante
    var gridEntry = tabsEntries.querySelector("li[fid='" + gridId + "']");
    if (gridEntry)
        setAttributeValue(gridEntry, "active", "1");

    // Charge le contenu de la grille
    oGridController.grid.load(gridId);

};

nsAdmin.deleteGrid = function (gridId) {
    oGridController.grid.delete(gridId, function () {

        var tabContainer = document.getElementById("admWebTab");
        var parentTab = getAttributeValue(tabContainer, "pTab");
        if (parentTab == "")
            parentTab = 0;

        var parentFileId = getAttributeValue(tabContainer, "pfId");
        if (parentFileId == "")
            parentFileId = 0;
        if (parentTab == 115200) {
            oGridController.page.load(parentFileId);

        } else {
            nsAdmin.loadAdminFile(nGlobalActiveTab);

        }
    });
};

///Recharge la barre de navbar des spécifs
nsAdmin.updateNavBarSpecif = function (oSpecInfos) {
    //console.log("updateNavBarSpecif");

    var upd = new eUpdater("eda/mgr/eWebTabManager.ashx", 1);
    upd.addParam("action", nsAdmin.WebTabManagerAction.GETNAVBAR, "post");
    upd.addParam("tab", oSpecInfos.Tab, "post");
    var ednType = getAttributeValue(document.getElementById("fileDiv_" + oSpecInfos.Tab), "ednType")
    if (ednType)
        upd.addParam("ednType", ednType, "post");
    upd.send(
        function (oRes) {
            nsAdmin.updateNavBarSpecifMaj(oRes, oSpecInfos.SpecifId)
        }

    );
}

nsAdmin.updateNavBarSpecifMaj = function (oRes, specId) {
    //Maj du tab
    // oRes contient le html de l'admin des propriété du webtab

    var oNavBar

    oNavBar = document.getElementById("admWebTab");
    if (oNavBar) {
        var oNew;

        //conteneur
        var divTmp = document.createElement("div");
        divTmp.innerHTML = oRes;
        var oContentTab = divTmp.getElementsByTagName("div")[0];

        //retire tous les node actuel
        while (oNavBar.firstChild) {
            oNavBar.removeChild(oNavBar.firstChild);
        }

        //remplace par les nouveaux nodes
        while (oContentTab.childNodes.length > 0) {
            oNavBar.appendChild(oContentTab.childNodes[0]);
        }

        //réaffecte les handlers
        nsAdmin.setDDHandlers();

        if (specId)
            //met le focus sur la bonne
            nsAdmin.highLightSpecif(specId);
    }
}

/// <summary>
/// met à jour le contenu d'un tab du menu droite
/// </summary>
/// <param name="id">num du tab a mettre à jour
///  1 : contenu de l'onglet (ajout de rubrique, mise en page...)
///  2 : propriété du champ, du signet web...
///  3 : Paramètre de l'onglet (nom, relations, droits...)
///</param>
/// <param name="oContent">Inner contenu a maj</param>
/// <param name="bNoFocus">Indique si la tab ne doit pas prendre le focus</param>
nsAdmin.refreshTab = function (id, oContent, bNoFocus) {
    if (typeof (bNoFocus) == 'undefined')
        bNoFocus = false;

    //Il y a 3 tab
    if (!/[1-3]/.test(id))
        return;

    var sParamTab = 'paramTab' + getNumber(id);

    var oTab = document.getElementById(sParamTab);

    if (oTab) {
        //conteneur
        var divTmp = document.createElement("div");
        divTmp.innerHTML = oContent;
        var oContentTab = divTmp.getElementsByTagName("div")[0];

        //retire tous les node actuel
        while (oTab.firstChild) {
            oTab.removeChild(oTab.firstChild);
        }

        //remplace par les nouveaux nodes
        while (oContentTab.childNodes.length > 0) {
            oTab.appendChild(oContentTab.childNodes[0]);
        }

        //Ouverture du tab
        if (!bNoFocus)
            nsAdmin.showBlock(sParamTab);

        //Set Action
        nsAdmin.setAction();
    }
}

var modalPmAdrMapping;
nsAdmin.confPmAdrMapping = function () {
    modalPmAdrMapping = new eModalDialog(top._res_1866, 0, "eda/mgr/eAdminPmAdrMapping.ashx", 985, 650, "modalAdminPmAdrMapping");
    modalPmAdrMapping.show();

    modalPmAdrMapping.addButton(top._res_30, function () { modalPmAdrMapping.hide(); }, 'button-gray', null);
    modalPmAdrMapping.addButton(top._res_28, function () { modalPmAdrMapping.getIframe().nsAdminPmAdrMapping.Update(); }, 'button-green', null);

};

nsAdmin.updatePmAdrMapping = function () {

};

var modalRelations;
nsAdmin.confRelations = function (readOnly) {

    modalRelations = new eModalDialog(top._res_8075, 0, "eda/eAdminRelationsDialog.aspx", 985, 650, "modalAdminRelations");

    modalRelations.noButtons = false;
    modalRelations.addParam("iframeScrolling", "yes", "post");
    modalRelations.addParam("tab", nsAdmin.tab, "post");
    modalRelations.show();

    modalRelations.addButton(top._res_30, function () { modalRelations.hide(); }, 'button-gray', null);

    if (!readOnly) {
        modalRelations.addButton(top._res_28, function () { nsAdmin.updateRelations(false, true, false); }, 'button-green', null);
    }


    nsAdmin.modalResizeAndMove(modalRelations);
}

var modalRelationsSQLFilters;
nsAdmin.confRelationsSQLFilters = function (strInterEventNum) {
    modalRelationsSQLFilters = new eModalDialog(top._res_8076, 0, "eda/eAdminRelationsSQLFiltersDialog.aspx", 500, 300, "modalAdminRelationsSQLFilters");

    modalRelationsSQLFilters.noButtons = false;
    modalRelationsSQLFilters.addParam("iframeScrolling", "yes", "post");

    if (typeof (strInterEventNum) == "undefined" || strInterEventNum == null)
        strInterEventNum = document.getElementById("ddlInterTables").value;

    modalRelationsSQLFilters.addParam("interEventNum", strInterEventNum, "post");
    modalRelationsSQLFilters.addParam("tab", parent.nsAdmin.tab, "post");

    modalRelationsSQLFilters.show();

    modalRelationsSQLFilters.addButton(top._res_30, function () { modalRelationsSQLFilters.hide(); }, 'button-gray', null);
    modalRelationsSQLFilters.addButton(top._res_28, function () { parent.nsAdmin.updateRelationsSQLFilters(); }, 'button-green', null);
}

nsAdmin.updateRelations = function (bKeepOpened, bUpdateFields, bConfirm, callback) {
    var adminRelations = modalRelations.getIframe().nsAdminRelations;
    if (adminRelations) {
        adminRelations.setCapsule(bUpdateFields);
        adminRelations.updCaps.SetCapsuleConfirmed(bConfirm);
        if (adminRelations.updCaps.ListProperties.length > 0) {
            var json = JSON.stringify(adminRelations.updCaps);

            var upd = new eUpdater("eda/Mgr/eAdminDescManager.ashx", 1);
            upd.json = json;
            upd.send(function (oRes) {
                var res = JSON.parse(oRes);
                if (res.NeedConfirm && !bConfirm) {
                    var conf = top.eConfirm(1, "", res.UserErrorMessage, "", null, null,
                        function () {
                            nsAdmin.updateRelations(bKeepOpened, bUpdateFields, true, callback);
                        },
                        function () {
                            conf.hide();
                            if (bKeepOpened) {
                                modalRelations.getIframe().document.getElementById("stepTitle1").click();
                            }
                        },
                        true,
                        true);
                }
                else {
                    if (!res.Success) {
                        top.eAlert(1, top._res_416, res.UserErrorMessage);
                    }
                    else {

                        if (!bKeepOpened) {
                            modalRelations.hide();

                            nsAdmin.loadContentModule(USROPT_MODULE_ADMIN_TAB, null, { tab: nsAdmin.tab })
                            // on rouvre la section après rechargement - #50 298
                            nsAdmin.loadMenuModule(USROPT_MODULE_ADMIN_TAB, { tab: nsAdmin.tab, OpenedBlock: true });
                        }

                        if (typeof (callback) == 'function') {
                            callback();
                        }
                    }
                }
            });
        }
    }
    else {
        eAlert(1, "Mise à jour impossible", "Données non retrouvées");
    }
}

nsAdmin.updateRelationsSQLFilters = function () {
    var adminRelationsSQLFilters = modalRelations.getIframe().modalRelationsSQLFilters.getIframe().nsAdminRelationsSQLFilters;
    if (adminRelationsSQLFilters) {
        adminRelationsSQLFilters.setCapsule();
        if (adminRelationsSQLFilters.updCaps.ListProperties.length > 0) {
            var json = JSON.stringify(adminRelationsSQLFilters.updCaps);

            var upd = new eUpdater("eda/Mgr/eAdminDescManager.ashx", 1);
            upd.json = json;
            upd.send(function (oRes) {
                var res = JSON.parse(oRes);

                if (!res.Success) {
                    top.eAlert(1, top._res_416, res.UserErrorMessage);
                }
                else {
                    top.eAlert(4, "Relations d'entête - Filtres SQL avancés", "Mise à jour effectuée", '', null, null, function () { modalRelations.getIframe().modalRelationsSQLFilters.hide(); });
                }
            });
        }
    }
    else {
        eAlert(1, "Mise à jour impossible", "Données non retrouvées");
    }
}

// Ouverture de la popup d'administration des appartenances
var modalBelonging;
nsAdmin.showBelongingPopup = function () {
    var width = 985;
    var height = 650;
    modalBelonging = new eModalDialog(top._res_7401, 0, "eda/eAdminBelongingDialog.aspx", width, height, "modalBelonging");

    modalBelonging.noButtons = false;
    modalBelonging.addParam("iframeScrolling", "yes", "post");
    modalBelonging.addParam("tab", nsAdmin.tab, "post");
    modalBelonging.addParam("w", width, "post");
    modalBelonging.addParam("h", height, "post");
    modalBelonging.show();

    modalBelonging.addButton(top._res_30, function () {
        modalBelonging.hide();
    }, 'button-gray', null);
    modalBelonging.addButton(top._res_28, function () {
        var capsule = modalBelonging.getIframe().nsAdminBelonging.getUpdCapsule(nsAdmin.tab);
        if (capsule != null) {
            if (capsule.ListProperties.length > 0) {
                var json = JSON.stringify(capsule);
                var upd = new eUpdater("eda/Mgr/eAdminDescManager.ashx", 1);
                upd.json = json;
                upd.send(function (oRes) {
                    var res = JSON.parse(oRes);
                    if (!res.Success) {
                        top.eAlert(1, top._res_416, res.UserErrorMessage);
                    }
                    else {
                        modalBelonging.hide();
                    }
                });
            }
        }
        //nsAdmin.updateRelations(false, true);
    }, 'button-green', null);
}

//Enum
nsAdmin.WebTabManagerAction = {
    UNDEFINED: 0,
    GETINFOS: 1, // Recupère le html de la tab d'édition de l'onglet web
    UPDATEINFO: 2, //met à jour les infos d'un signet/onglet web
    CREATE: 3, // création d'un signet web
    DELETE: 4, // suppression d'un signet/onglet web
    GETNAVBAR: 5, // Obtient la barre de navigation des signets webs
    MOVETAB: 6, // déplace un sous onglet web dans la barre des sous onglets
    MOVEGRID: 7, // déplace une grille dans la barre des sous onglets
    GETSUBTAB: 8// Renvoi le menu SUBTAB 
}

nsAdmin.WebLinkManagerAction = {
    UNDEFINED: 0,
    GETINFOS: 1, // Recupère le html de la tab d'édition du lien web
    CREATE: 2, // création d'un lien web
    UPDATEMENU: 3, // Refresh menu
    DELETE: 4 // suppression d'un lien web
}

/// <summary>
/// catégorie de mise à jour d'admin
///  A METTRE A JOUR DANS eAdminCapsule.cs en cas d'ajout
/// </summary>
nsAdmin.Category = {
    DESC: 0,
    PREF: 1,
    RES: 2,
    CONFIG: 3,
    CONFIGDEFAULT: 4,
    CONFIGADV: 5,
    WEBTAB: 6,
    FILEDATAPARAM: 7,
    BKMPREF: 8,
    RESADV: 9,
    USERVALUE: 10,
    COLSPREF: 11,
    CONDITIONALSENDINGRULES: 12,
    COLOR_RULES: 13,
    MAPLANG: 14,
    SPECIFS: 15,
    DESCADV: 16,
    SYNC: 17
}

/// <summary>
/// Type de drag&drop
///  A METTRE A JOUR DANS eAdminRenderer.cs en cas d'ajout
/// </summary>
nsAdmin.DragType =
{
    OTHER: 0,
    FIELD: 1,
    WEB_TAB_MENU: 2,
    WEB_SIGNET: 3,
    HEAD_LINK: 4,
    WEB_TAB: 5,
    GRID_MENU: 6,
    GRID: 7,
    WEB_TAB_LIST: 8
}

// Onglet du menu de droite
nsAdmin.RightMenuTab = {
    ALL: 0,
    TAB_CONTENT: 1,
    FIELD_PARAM: 2,
    TAB_PARAM: 3
}

nsAdmin.webBkmDragStartHandler = function (ev) {
    ev.dataTransfer.dropEffect = "copy";
}

nsAdmin.dragSrcElt = null;

nsAdmin.initDragDrop = function (elem) {
    nsAdmin.dragSrcElt = elem;
}

nsAdmin.resetDragDrop = function () {
    nsAdmin.dragSrcElt = null;

    if (document.getElementById("webtabshadowelem"))
        document.getElementById("webtabshadowelem").remove();
}

/// <summary>
/// Affecte tous les handers de drag&drop - Pour les web tab
/// </summary>
nsAdmin.setDDHandlers = function () {
    //console.log("nsAdmin.setDDHandlers");

    var oAllDraggable = document.querySelectorAll("[draggable][edragtype]");

    var oAllDraggableArr = [].slice.call(oAllDraggable);

    oAllDraggableArr.forEach(function (monDDElem) {
        var type = getNumber(getAttributeValue(monDDElem, "edragtype"));

        switch (type) {
            case nsAdmin.DragType.GRID_MENU:
            case nsAdmin.DragType.WEB_TAB_MENU:
                nsAdminWebTagDragDrop.addHandlerDropNewTab(monDDElem);
                break;
            case nsAdmin.DragType.GRID:
            case nsAdmin.DragType.WEB_TAB:
            case nsAdmin.DragType.WEB_TAB_LIST:
                nsAdminWebTagDragDrop.addHandlerMoveTab(monDDElem);
                break;
            default:
        }
    });
}

nsAdmin.isTypeChar = function (format) {
    if (format == FLDTYP.CHAR || format == FLDTYP.EMAIL || format == FLDTYP.WEB || format == FLDTYP.PHONE) {
        return true;
    }
    return false;
}

//
nsAdmin.controlFieldChange = function (element) {
    var elementType, elementLength;
    var newType, oldType, newLength, oldLength;

    if (element.id == "ddlFieldType") {
        elementType = element;
        elementLength = document.getElementById("txtLength");
    }
    else if (element.id == "txtLength") {
        elementLength = element;
        elementType = document.getElementById("ddlFieldType");
    }

    newType = elementType.value;
    oldType = getAttributeValue(elementType, "lastvalid");

    // Si on fait type char-> type char et taille plus petite
    var bMultiple = false; // TODO: à récupérer
    if (nsAdmin.isTypeChar(oldType) && (nsAdmin.isTypeChar(newType) && !bMultiple)) {
        newLength = getNumber(elementLength.value);
        oldLength = getNumber(getAttributeValue(elementLength, "lastvalid"));
        if (oldLength > newLength) {
            eConfirm(0, top._res_68, top._res_284, '', 500, 200, function () { nsAdmin.sendJson(element); }, function () { });
        }
        else
            nsAdmin.sendJson(element);
    }
    else if (oldType != newType) {
        // Si type de rubrique différent
        eConfirm(0, top._res_68, top._res_282, '', 500, 200, function () { nsAdmin.sendJson(element); }, function () { });
    }
    else if (newType == FLDTYP.AUTOINC) {
        // Compteur automatique
        newLength = getNumber(elementLength.value);
        oldLength = getNumber(getAttributeValue(elementLength, "lastvalid"));
        if (newLength != oldLength)
            eConfirm(0, top._res_68, top._res_2028, '', 500, 200, function () { nsAdmin.sendJson(element); }, function () { });
    }
    else {
        nsAdmin.sendJson(element);
    }
}

// Ouverture de la popup de choix du picto et de sa couleur
nsAdmin.openPictoPopup = function (descid) {
    var oPicto = new ePictogramme("selectedPicto", {
        tab: nsAdmin.tab,
        descid: descid,
        title: "Administrer le pictogramme",
        width: 750,
        height: 675,
        callback: nsAdmin.updatePicto
    });

    oPicto.show();
}

// Appel du manager pour mettre à jour le picto
nsAdmin.updatePicto = function (picto, descid) {
    var upd = new eUpdater("eda/mgr/eAdminPictoManager.ashx", 1);
    upd.addParam("tab", nsAdmin.tab, "post");
    upd.addParam("descid", descid, "post");
    upd.addParam("color", picto.color, "post");
    upd.addParam("icon", picto.key, "post");

    upd.send(
        // Résultat/Rafraîchissement
        function (oRes) {
            var res = JSON.parse(oRes);
            if (!res.Success) {
                eAlert(1, "Mise à jour pictogramme", res.Error);
            }
            else {
                // Rafraîchissement dans le menu de droite
                var btnPicto = document.getElementById("selectedPicto");
                if (btnPicto) {
                    btnPicto.className = res.PictoClassName;
                    btnPicto.style.color = res.PictoColor;
                }
                // Rafraîchissement sur la partie gauche
                var fileLogo = document.querySelector(".file99 .fileLogo");
                fileLogo.className = "fileLogo " + res.PictoClassName;
                fileLogo.style.color = res.PictoColor;
            }
        }
    );
}

nsAdmin.openColorPicker = function (colorPicker, colorPickerText, onchangeFunction) {
    var pickerOptions = {
        picker: colorPicker,
        pickerTxt: colorPickerText,
        color: (colorPickerText) ? colorPickerText.value : "",
        onHide: onchangeFunction
    };
    var oPicker = new eColorPicker(pickerOptions);
    oPicker.show();
}


/**
 * On regarde suivant le source qui doit être une checkbox, si celle-ci est cochée ou non.
 * On retourne ensuite une valeur suivant que c'est coché ou non.
 * */
verifCkErgoChecked = function (source, valChecked, valUnChecked) {

    if (!((source instanceof HTMLInputElement)
        && (source.getAttribute('type').toLowerCase() == 'checkbox')))
        return "undefined";

    return ((source.checked) ? valChecked : valUnChecked);
}

/**
 * On regarde suivant le source qui doit être une combobox, si celle-ci comporte la valeur donnée ou non
 * On retourne ensuite un libellé donné en fonction de cette valeur
 * */
verifDdlErgoEnabled = function (source, valueLabels) {

    if (!(source instanceof HTMLSelectElement))
        return "undefined";

    return valueLabels[source.options[source.selectedIndex].value];
}

/**
 * Permet de créer un switch pour l'activation du nouveau mode Fiche/Liste
 * Avec en paramètre un id qui sert à compléter les éléments uniques
 * et une fonction qui permet de changer le libellé du switch.
 * @param {any} id
 * @param {boolean} check la position du switch
 * @param {any} fnLabel
 * @param {any} fnClick
 */
nsAdmin.createSwitchPref = function (id, check, fnLabel, fnClick, switchBtnTooltip) {
    dvSwitch = document.createElement("div");
    dvSwitch.id = "dvSwitch" + id;
    dvSwitch.className = "switch-new-theme-wrap";

    ickSwitch = document.createElement("input");
    ickSwitch.id = "chckNw" + id;
    ickSwitch.type = "checkbox";
    ickSwitch.className = "switch-new-theme";
    ickSwitch.checked = check;
    ickSwitch.addEventListener("click", fnClick);

    lblSwitch = document.createElement("label");
    lblSwitch.htmlFor = "chckNw" + id;
    lblSwitch.title = switchBtnTooltip;

    spnSwitch = document.createElement("span");
    spnSwitch.id = "spnNw" + id;
    spnSwitch.style.color = "#7A716E";
    spnSwitch.innerHTML = fnLabel.apply();
    spnSwitch.title = spnSwitch.innerHTML;

    dvSwitch.appendChild(ickSwitch);
    dvSwitch.appendChild(lblSwitch);
    dvSwitch.appendChild(spnSwitch);

    return dvSwitch;
}

/**
 * Permet de créer un switch multi-positions pour l'activation de plusieurs statuts (Activé, Désactivé, Mi-Activé, Mi-Désactivé...)
 * Avec en paramètre un id qui sert à compléter les éléments uniques
 * et une fonction qui permet de changer le libellé du switch.
 * @param {any} id
 * @param {any} values les valeurs disponibles et sélectionnées, sous la forme Array( { value: <valeur>, text: <libellé>, selected: <valeurSélectionéeParDéfaut> })
 * @param {any} fnLabel Fonction renvoyant le libellé du composant
 * @param {any} fnClick Fonction exécutée au clic sur le composant
 * @param {any} fnChange Fonction exécutée au changement de valeur
 * @param {string} switchBtnTooltip Infobulle du contrôle
 */
nsAdmin.createMultiSwitchPref = function (id, values, fnLabel, fnClick, fnChange, switchBtnTooltip) {
    dvSwitch = document.createElement("div");
    dvSwitch.id = "dvSwitch" + id;
    dvSwitch.className = "switch-new-theme-wrap";

    ddlSwitch = document.createElement("select");
    ddlSwitch.id = "ddlNw" + id;
    ddlSwitch.className = "switch-new-theme";
    ddlSwitch.addEventListener("click", fnClick);
    ddlSwitch.addEventListener("change", fnChange);

    if (values?.length > 0) {
        for (var i = 0; i < values.length; i++) {
            var option = document.createElement("option");
            option.value = values[i]?.value;
            option.text = values[i]?.text;
            option.selected = values[i]?.selected;
            ddlSwitch.appendChild(option);
        }
    }

    lblSwitch = document.createElement("label");
    lblSwitch.htmlFor = "ddlNw" + id;
    lblSwitch.title = switchBtnTooltip;
    lblSwitch.style.display = "none"; // TODO SWITCH 3 POSITIONS

    spnSwitch = document.createElement("span");
    spnSwitch.id = "spnNw" + id;
    spnSwitch.style.color = "#7A716E";
    spnSwitch.innerHTML = fnLabel.apply();
    spnSwitch.title = spnSwitch.innerHTML;

    dvSwitch.appendChild(ddlSwitch);
    dvSwitch.appendChild(lblSwitch);
    dvSwitch.appendChild(spnSwitch);

    return dvSwitch;
}

/**
 * Le champs texte et son titre qui permettront d'enregister le JSON.
 * Et des boutons si définis (US #3 205 - TK #4 876)
 * Retourne un objet de dom
 * @param {any} obj
 * @returns
 */
nsAdmin.createTextAreaJson = function (obj) {
    var lblTitle = document.createElement("h3");
    lblTitle.innerHTML = obj.title;

    var divBtnContainer = document.createElement("div");
    if (obj.buttons) {
        for (var i = 0; i < obj.buttons.length; i++) {
            var currentBtn = obj.buttons[i];
            if (currentBtn.function) {
                var divBtn = document.createElement("div");
                var divBtnLink = document.createElement("a");
                divBtn.className = "field linkButton";
                divBtn.id = currentBtn.id;
                divBtnLink.onclick = function () { currentBtn.function(obj); }
                divBtnLink.innerHTML = currentBtn.label;
                divBtn.appendChild(divBtnLink);
                divBtnContainer.appendChild(divBtn);
            }
        }
    }
    var divTitleContainer = null;
    if (divBtnContainer.hasChildNodes()) {
        divTitleContainer = document.createElement("div");
        divTitleContainer.appendChild(lblTitle);
        divTitleContainer.appendChild(divBtnContainer);
    }

    var txtTitle = document.createElement("textArea");
    txtTitle.id = obj.id;
    txtTitle.rows = obj.rows;
    txtTitle.cols = obj.cols;
    txtTitle.spellcheck = obj.spellcheck;
    txtTitle.disabled = obj.disabled;
    txtTitle.value = obj.value;

    var divToolTip = document.createElement("div");
    divToolTip.id = obj.id + "ToolTip";
    divToolTip.innerText = obj.tooltip;

    return {
        title: divTitleContainer ? divTitleContainer : lblTitle,
        txt: txtTitle,
        tooltip: divToolTip
    };
}

/**
 * Transforme une String en objet JSON et, si la conversion échoue, renvoie une exception plus précise que celle de JSON.parse, en précisant le message donné en paramètre
 * @param {any} value String JSON à transformer
 * @param {any} customExceptionMessage Message à renvoyer en complément dans l'exception si la transformation échoue
 * @param {any} reviverFct Fonction de transformation à appliquer sur la valeur analysée avant de la renvoyer
 * @returns {any} l'objet correspondant au JSON analysé, ou une exception détaillée en cas d'erreur
 */
nsAdmin.parseJson = function (value, customExceptionMessage, reviverFct) {
    try {
        value = JSON.parse(value, reviverFct);
    }
    catch (e) {
        if (customExceptionMessage)
            throw new Error(customExceptionMessage + " - " + e.message);
        else
            throw e;
    }

    return value;
}

/**
 * Génère un JSON automatique pour la zone de saisie visée, et l'insère à l'intérieur de ladite zone
 */
nsAdmin.generateJson = function (oSource, bCanErase) {

    if (!oSource)
        return;

    var oTargetTextArea = document.getElementById(oSource.id);
    if (!oTargetTextArea)
        return;

    // Si le contenu de la zone de texte cible est vide, ou si l'utilisateur a déjà confirmé son écrasement, on poursuit
    if (bCanErase || oTargetTextArea.value.trim().length == 0 || oTargetTextArea.value.replace(" ", "").trim() == "{}") {
        switch (oSource.id) {
            case "txtTitleSummary": return nsAdmin.generateJsonSummary(oTargetTextArea, false);
            case "txtTitleWizard": return nsAdmin.generateJsonWizard(oTargetTextArea, false);
            // TODO: ajouter ici les autres cas au fur et à mesure de leur développement
        }
    }
    // Sinon, on affiche le message de confirmation
    // <Titre de la zone> - Etes-vous sûr de vouloir charger le modèle ? Les données existantes seront écrasées
    else {
        eConfirm(1, oSource.title, top._res_6666, top._res_6667, 450, 180, function () {
            nsAdmin.generateJson(oSource, true);
        });
    }
}

/**
 * Génère le modèle de JSON pour la zone de saisie "Résumé"
 * Appelle un contrôleur backend via AJAX pour récupérer la liste des champs, puis génère le JSON et le place dans le champ textarea passé en paramètre
 * @param {any} oTargetTextArea le champ cible dans lequel insérer le JSON généré
 * @param {boolean} bDontRefreshFieldList Indique si la fonction peut récupérer les données et s'auto-rappeler. Evite les boucles infinies
 */
nsAdmin.generateJsonSummary = function (oTargetTextArea, bDontRefreshFieldList) {
    // Si cette fonction a été appelée depuis generateJson() sans avoir récupéré les informations, on les récupère 
    // puis on auto-rappelle cette fonction en callback
    if (!nsAdmin.displayPreferencesGenerateJSONFieldList && !bDontRefreshFieldList) {
        nsAdmin.refreshFieldList(nGlobalActiveTab, function (oRes) { nsAdmin.generateJsonSummary(oTargetTextArea, true); });
        return;
    }
    // Si après appel du contrôleur, les résultats ne sont pas exploitables, on ne fait rien
    else if (
        !oTargetTextArea ||
        !nsAdmin.displayPreferencesGenerateJSONFieldList ||
        nsAdmin.displayPreferencesGenerateJSONFieldList.length < 1
    ) {
        eAlert(0, top._res_2962, top._res_1603, top._res_1603); // Aucun champ disponible
        return;
    }

    var oRes = nsAdmin.displayPreferencesGenerateJSONFieldList;

    // Objet à insérer dans le textarea
    var oJsonModel = {};

    // On récupère les champs de type Caractère (affichables en Titre ou Sous-titre)
    var oCharacterFields = oRes.Data.filter(function (field) { return field.Format == eTools.FieldFormat.TYP_CHAR; });
    // On récupère égaleement les catalogues à partir de la variable globale déjà alimentée à l'initialisation (de la popup ou de cette fonction)
    var oCatalogFields = nsAdmin.displayPreferencesGenerateJSONCatalogList;
    // Et enfin, les rubriques autres que 01 et 02
    var oFieldsWithout01 = oRes.Data.filter(function (field) { return field.DescId > nGlobalActiveTab + 1; });
    var oFieldsWithout01And02 = oRes.Data.filter(function (field) { return field.DescId > nGlobalActiveTab + 2; });
    // Pour avoir un champ avatar dans le JSON, on doit être sur PP ou PM, et le champ Avatar doit avoir été renvoyé parmi la liste des champs
    var oAvatarField = (nGlobalActiveTab == TAB_PP || nGlobalActiveTab == TAB_PM) ? oRes.Data.filter(function (field) { return field.DescId == nGlobalActiveTab + 75; }) : null;

    // TITRE ET SOUS-TITRE
    if (oFieldsWithout01And02 && oFieldsWithout01And02.length > 0) {
        switch (nGlobalActiveTab) {
            // Sur PP, on met en titre le 01, et en sous-titre le premier champ catalogue disponible (ou à défaut, le premier de la liste hors 01 et 02 - Nom / Prénom)
            case TAB_PP:
                oJsonModel.title = TAB_PP + 1;
                oJsonModel.sTitle = oCatalogFields && oCatalogFields.length > 0 ? oCatalogFields[0].DescId : oFieldsWithout01And02[0].DescId;
                break;

            // Dans les autres cas, on met en sous-titre le premier champ catalogue disponible, ou à défaut, le premier de la liste hors 01
            default:
                oJsonModel.title = oCharacterFields && oCharacterFields.length > 0 ? oCharacterFields[0].DescId : null;
                oJsonModel.sTitle = oCatalogFields && oCatalogFields.length > 0 ? oCatalogFields[0].DescId : oFieldsWithout01[0].DescId;
                break;
        }
    }

    // AVATAR
    if (oAvatarField && oAvatarField.length > 0)
        oJsonModel.avatar = oAvatarField[0].DescId;

    // RUBRIQUES AFFICHEES
    // On prend les 6 premières disponibles:
    // - hors 01 et 02
    // - qui n'aient pas encore été utilisées en titre ou sous-titre ou avatar
    oJsonModel.inputs =
        oFieldsWithout01And02
            .filter(function (field) { return field.DescId != oJsonModel.title && field.DescId != oJsonModel.sTitle && field.DescId != oJsonModel.avatar }) /* on exclut les rubriques précédemment utilisées */
            .slice(0, 5) /* les 6 premières renvoyées dans le tableau filtré */
            .map(function (field) { return { DescId: field.DescId } }); /* on veut une liste des DescIDs uniquement */

    // Et enfin, on implante le JSON dans le textarea cible
    oTargetTextArea.value = JSON.stringify(oJsonModel, null, 4); // 4 : nombre d'espaces à utiliser pour l'indentation à l'affichage
}

/**
 * Génère le modèle de JSON pour la zone de saisie "Assistant"
 * Appelle un contrôleur backend via AJAX pour récupérer la liste des champs, puis génère le JSON et le place dans le champ textarea passé en paramètre
  * @param {any} oTargetTextArea le champ cible dans lequel insérer le JSON généré
  * @param {boolean} bDontRefreshFieldList Indique si la fonction peut récupérer les données et s'auto-rappeler. Evite les boucles infinies
 */
nsAdmin.generateJsonWizard = function (oTargetTextArea, bDontRefreshFieldList) {
    // Si cette fonction a été appelée depuis generateJson() sans avoir récupéré les informations, on les récupère 
    // puis on auto-rappelle cette fonction en callback
    if (!nsAdmin.displayPreferencesGenerateJSONFieldList && !bDontRefreshFieldList) {
        nsAdmin.refreshFieldList(nGlobalActiveTab, function (oRes) { nsAdmin.generateJsonWizard(oTargetTextArea, true); });
        return;
    }
    // Si après appel du contrôleur, les résultats ne sont pas exploitables, on ne fait rien
    else if (
        !oTargetTextArea ||
        !nsAdmin.displayPreferencesGenerateJSONCatalogList ||
        nsAdmin.displayPreferencesGenerateJSONCatalogList.length < 1
    ) {
        eAlert(0, top._res_2962, top._res_1603, top._res_1603); // Aucun champ disponible
        return;
    }

    var oCatalogFields = nsAdmin.displayPreferencesGenerateJSONCatalogList;

    var modalPromptSourceCatalog = new eModalDialog(top._res_7373, 10, null, 450, 150); // 10 = ModalType.DisplayContent
    modalPromptSourceCatalog.noButtons = false;

    var oSourceContainer = document.createElement("div");
    oSourceContainer.id = "generateJsonWizardSourceContainer";
    var oSourceSelect = document.createElement("select");
    oSourceSelect.id = "generateJsonWizardSource";
    var oSourceLabel = document.createElement("label");
    oSourceLabel.for = oSourceSelect.id;
    oSourceLabel.innerHTML = top._res_2974; // Veuillez choisir la rubrique Catalogue à utiliser dans la zone Assistant :
    oSourceLabel.className = "text-msg-quote";
    oSourceContainer.appendChild(oSourceLabel);
    oSourceContainer.appendChild(oSourceSelect);
    var oSourceSelectOption = document.createElement("option");
    oSourceSelectOption.value = "0";
    oSourceSelectOption.innerHTML = "&lt;" + top._res_96 + "&gt;";
    for (var i = 0; i < oCatalogFields.length; i++) {
        oSourceSelectOption = document.createElement("option");
        oSourceSelectOption.value = oCatalogFields[i].DescId;
        oSourceSelectOption.innerHTML = oCatalogFields[i].Labels[top._userLangId] + " (" + oCatalogFields[i].DescId + ")";
        oSourceSelect.appendChild(oSourceSelectOption);
    }

    modalPromptSourceCatalog.setElement(oSourceContainer);
    modalPromptSourceCatalog.show();
    modalPromptSourceCatalog.addButton(top._res_30, function () {
        modalPromptSourceCatalog.hide();
    }, 'button-gray', null);
    modalPromptSourceCatalog.addButton(top._res_28, function () {
        var oModalPromptSourceSelect = modalPromptSourceCatalog.eltToDisp.querySelector("#generateJsonWizardSource")
        var nSourceDescId = oModalPromptSourceSelect.options[oModalPromptSourceSelect.selectedIndex].value;
        if (nSourceDescId == 0) {
            eAlert(0, top._res_2962, top._res_1603, top._res_1603);
            return;
        }
        modalPromptSourceCatalog.hide();
        nsAdmin.generateJsonWizardFromSource(null, oTargetTextArea, nSourceDescId);
    }, 'button-green', null);
}

/**
 * Génère le modèle de JSON pour la zone de saisie "Assistant" en fonction du champ sélectionné
 * Génère le JSON et le place dans le champ textarea passé en paramètre
 */
nsAdmin.generateJsonWizardFromSource = function (oRes, oTargetTextArea, nSourceDescId) {
    // Si cette fonction a été appelée depuis generateJsonWizard() sans avoir récupéré les informations, on les récupère
    // puis on auto-rappelle cette fonction en callback
    if (!oRes) {
        nsAdmin.getCatalogData(nSourceDescId, function (oRes) {
            nsAdmin.generateJsonWizardFromSource(oRes, oTargetTextArea, nSourceDescId);
        });
        return;
    }
    // Si après appel du contrôleur, les paramètres ne sont pas exploitables, on ne fait rien
    else if (!oTargetTextArea || !oRes.Data || oRes.Data.length < 1 || !nSourceDescId) {
        eAlert(0, top._res_2962, top._res_1603, top._res_1603); // Aucun champ disponible
        return;
    }

    // Objet à insérer dans le textarea
    var oJsonModel = {
        DescId: nSourceDescId,
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

    // Remplissage de Title et Body avec une ressource par défaut dans la langue de l'utilisateur en cours
    oJsonModel.WelcomeBoard.Title["lang_" + top._userLangId.toString().padStart(2, '0')] = top._res_2573;
    oJsonModel.WelcomeBoard.Body["lang_" + top._userLangId.toString().padStart(2, '0')] = top._res_2574;

    //var MAX_FIELDS = 7;
    //var nMaxValues = MAX_FIELDS;
    //if (oRes.Data.Values.length < MAX_FIELDS)
    var nMaxValues = oRes.Data.Values.length;

    for (var i = 0; i < nMaxValues; i++) {
        var oPreviousValue = (i == 0 ? null : oRes.Data.Values[i - 1]);
        var oNextValue = (i == nMaxValues - 1 ? null : oRes.Data.Values[i + 1]);
        var oField = {
            DataId: oRes.Data.Values[i] ? oRes.Data.Values[i].Id : 0,
            DataIdPrevious: oPreviousValue ? [oPreviousValue.Id] : [],
            DataIdNext: oNextValue ? [oNextValue.Id] : [],
            DisplayedFields: []
        };
        oJsonModel.FieldsById.push(oField);
    }

    // Et enfin, on implante le JSON dans le textarea cible
    oTargetTextArea.value = JSON.stringify(oJsonModel, null, 4); // 4 : nombre d'espaces à utiliser pour l'indentation à l'affichage
}

/**
 * Récupère la liste complète des champs correspondant à un fichier/onglet (nTab) avec toutes leurs caractéristiques, et alimente les variables locales
 * contenant ces données, pour éviter d'avoir à faire plusieurs appels
 * @param {any} nTab TabID de l'onglet dont il faut récupérer les champs
 * @param {any} oCallback En callback, exécute la fonction donnée
 */
nsAdmin.refreshFieldList = function (nTab, oCallback) {
    nsAdmin.getFieldList(nTab, function (oRes) {
        nsAdmin.displayPreferencesGenerateJSONFieldList = oRes;
        if (oRes && oRes.Data)
            nsAdmin.displayPreferencesGenerateJSONCatalogList = oRes.Data.filter(function (field) { return field.PopupType == POPUPTYPE.DATA; });
        else
            nsAdmin.displayPreferencesGenerateJSONCatalogList = null;
        oCallback();
    });
}

/**
 * Récupère la liste complète des champs correspondant à un fichier/onglet (nTab) avec toutes leurs caractéristiques
 * @param {any} nTab TabID de l'onglet dont il faut récupérer les champs
 * @param {any} oCallback En callback, exécute la fonction donnée
 */
nsAdmin.getFieldList = function (nTab, oCallback) {
    // Effectue un appel AJAX pour récupérer la liste des champs, puis effectue une action ensuite (en appelant la fonction passû?e en callback)
    setWait(true);

    if (!nTab)
        nTab = nGlobalActiveTab;

    var url = "eda/mgr/eAdminGetFieldManager.ashx";
    var fieldListGetter = new eUpdater(url, 1);
    fieldListGetter.ErrorCallBack = nsAdmin.getFieldManagerError;

    fieldListGetter.addParam("action", "FIELD_LIST", "post");
    fieldListGetter.addParam("tabDescId", nTab, "post");
    fieldListGetter.send(nsAdmin.getFieldManagerReturn, oCallback);
}

/**
 * Récupère les informations d'un catalogue correspondant à un DescId passé en paramètre
 * @param {any} nDescId DescID du catalogue dont il faut récupérer les informations
 * @param {any} oCallback En callback, exécute la fonction donnée
 */
nsAdmin.getCatalogData = function (nDescId, oCallback) {
    // Effectue un appel AJAX pour récupérer les données, puis effectue une action ensuite (en appelant la fonction passû?e en callback)
    setWait(true);

    if (!nDescId) {
        setWait(false);
        return;
    }

    var url = "eda/mgr/eAdminGetFieldManager.ashx";
    var catalogDataGetter = new eUpdater(url, 1);
    catalogDataGetter.ErrorCallBack = nsAdmin.getFieldManagerError;

    catalogDataGetter.addParam("action", "CATALOG_DATA", "post");
    catalogDataGetter.addParam("descId", nDescId, "post");
    catalogDataGetter.send(nsAdmin.getFieldManagerReturn, oCallback);
}

/**
 * Fonction de callback utilisée en cas d'erreur sur getFieldList ou getCatalogData
 * @param {any} oError Capsule d'erreur renvoyée par getFieldList ou getCatalogData
 */
nsAdmin.getFieldManagerError = function (oError) {
    setWait(false);
}

/**
 * Fonction de callback appelée après l'appel du contrôleur par getFieldList ou getCatalogData
 * @param {any} oRes résultat de l'appel au contrôleur
 * @param {any} oCallback En callback, exécute la fonction donnée
 */
nsAdmin.getFieldManagerReturn = function (oRes, oCallback) {
    try {
        var result = JSON.parse(oRes);

        if (result.Success) {
            if (typeof (oCallback) == "function") {
                oCallback(result);
            }

            setWait(false);
        }
        else {
            setWait(false);
            var sErrorMsg = result.Error;
            eAlert(0, top._res_72, top._res_6237, sErrorMsg + '<br>' + top._res_6236);
        }
    }
    catch (e) {
        var sErrorMsg = e.message;
        eAlert(0, top._res_72, top._res_6237, sErrorMsg + '<br>' + top._res_6236);
    }
    finally {
        setWait(false);
    }
}

/**
 * Récupère un Json et éventuellement corrige les caractères htmlencodés, par un &amp;.
 * Correctif, assez baroque, mais il n'existe pas de moyen simple d'encoder une chaine de caractères.
 * @param {any} key
 * @param {any} value
 * @returns {any} un Json html encodé.
 */
nsAdmin.checkJsonAndCorrect = function (key, value) {
    if (typeof value === "string") {
        const regex = /&/gi;
        value = value.replace(regex, '&amp;');
    }

    return value;
}

/**
 * Mémorisation de la liste des champs utilisée pour la génération des JSON sur la fenêtre des préférences d'affichage
 * (évite de faire plusieurs fois le même appel si on génère plusieurs JSON sur la même fenêtre) */
nsAdmin.displayPreferencesGenerateJSONFieldList = null;
nsAdmin.displayPreferencesGenerateJSONCatalogList = null;

/*
 * Renvoie la liste des libellés disponibles pour le changement du mode d'affichage de la fiche Eudonet X (IRIS Black)
 */
nsAdmin.getEudonetXIrisBlackStatusValues = function (selectedValueOrText) {
    var returnList = new Array(
        {
            value: EUDONETX_IRIS_BLACK_STATUS.DISABLED,
            text: top._res_1459, // Désactivé
            selected: false
        },
        {
            value: EUDONETX_IRIS_BLACK_STATUS.ENABLED,
            text: top._res_1460, // Activé
            selected: false
        },
        {
            value: EUDONETX_IRIS_BLACK_STATUS.PREVIEW,
            text: top._res_3122, // Prévisualisation
            selected: false
        }
    );

    // Sélection de la valeur par défaut
    var selectedValue = returnList.find(v => v.value == selectedValueOrText || v.text == selectedValueOrText);
    if (selectedValue) {
        selectedValue.selected = true;
    }

    return returnList;
}


/**
 * fonction qui va permettre de mettre ou enelver directement dans eParamIframe, 
 * l'onglet sur lequel on vient d'activer l'élément.
 * @type {string} l'élément à traiter.
 * @param {array} ViceEtVersa permet de savoir si on ajoute ou enlève l'élément.
 * @param {int} nTab L'onglet pour lequel effectuer la modification. Si non précisé, prend l'onglet en cours (nGlobalActiveTab)
 */
nsAdmin.fnInsertNewTab = function (typeIris, ViceEtVersa, nTab) {
    let paramWin = top.getParamWindow();
    let nOneDirection = parseInt(ViceEtVersa);
    nTab = parseInt(nTab) || nGlobalActiveTab;

    if (!Array.isArray(typeIris))
        typeIris = [typeIris];

    typeIris.forEach((type) => {
        let arTabsIris = paramWin.GetParam(type)
            ?.split(";")
            ?.map(tab => parseInt(tab))
            ?.filter(tab => !isNaN(tab));

        let nPosTab = arTabsIris?.indexOf(nTab) || 0;

        if (arTabsIris?.length > -1 && nPosTab < 0 && nOneDirection > 0) {
            arTabsIris.push(nTab);
            arTabsIris.sort(((tabA, tabB) => parseInt(tabA) - parseInt(tabB)));
        }

        if (arTabsIris?.length > 0 && nPosTab > -1 && nOneDirection < 1)
            arTabsIris.splice(nPosTab, 1);

        let sTabsIris = arTabsIris.join(";");
        paramWin.SetParam(type, sTabsIris);
    });
}



/**
 * Ici on clique sur le bouton Administrer les préférences.
 * On affiche une popup avec un bouton switch permettant d'activer
 * ou non la nouvelle ergonomie.
 * @param {JSON} JSonObj un objet qui contient tous les éléments à afficher.
 * @param {Boolean} IsDebuggingEnabled variable provenant du back pour savoir si on est en débogage ou non. Certains affichages dépendent de cette valeur.
 * @param {boolean} bDontRefreshFieldList Indique si la fonction peut récupérer les données pour la génération des JSON, et s'auto-rappeler. Evite les boucles infinies
 **/
nsAdmin.confPreferences = function (JSonObj, IsDebuggingEnabled, bDontRefreshFieldList) {
    // A l'ouverture de la popup, on réinitialise la liste des champs utilisée pour générer les JSON
    // puis on auto-rappelle cette fonction en callback, une seule fois maximum
    if (!bDontRefreshFieldList) {
        nsAdmin.refreshFieldList(nGlobalActiveTab, function (oRes) { nsAdmin.confPreferences(JSonObj, IsDebuggingEnabled, true); });
        return;
    };

    var obj;

    try {
        obj = JSON.parse(JSonObj);
    } catch (e) {
        eAlert(0, "", e.message);
        obj.sJsonGuidedsJSonGuidedIrisPurple = {};
        obj.sJSonPageIrisBlack = {
            Page: {
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
            }
        };
    }

    var oldIrisBlackStatus = obj.EudonetXIrisBlackStatus;

    modalConfigFile = new eModalDialog(top._res_7019, 10, null, 985, 652);
    modalConfigFile.noButtons = false;

    var divContent = document.createElement("div");
    divContent.id = "dvCntPref";

    divContent.className = "comms-new-theme-container";

    /** Checkbox de l'activation de la nouvelle ergonomie. */
    var eudonetXIrisBlackValueLabels = new Array();
    eudonetXIrisBlackValueLabels[EUDONETX_IRIS_BLACK_STATUS.DISABLED] = top._res_2888; // Le mode fiche Eudonet x est désactivé
    eudonetXIrisBlackValueLabels[EUDONETX_IRIS_BLACK_STATUS.ENABLED] = top._res_2887; // Le mode fiche Eudonet x est activé
    eudonetXIrisBlackValueLabels[EUDONETX_IRIS_BLACK_STATUS.PREVIEW] = top._res_3129; // Le mode fiche Eudonet x est en prévisualisation
    var divSwitch = nsAdmin.createMultiSwitchPref("Ergo",
        nsAdmin.getEudonetXIrisBlackStatusValues(obj.EudonetXIrisBlackStatus),
        function () {
            return verifDdlErgoEnabled(ddlSwitch, eudonetXIrisBlackValueLabels);
        },
        null,
        function () {
            var spn = document.getElementById("spnNwErgo");
            var ddl = document.getElementById("ddlNwErgo");
            var switchBtn = document.querySelector("label[for='ddlNwErgo']");

            var dvGuided = document.getElementById("dvSwitchErgoGuided");
            var chckGuided = document.getElementById("chckNwErgoGuided");
            var lblGuided = document.getElementById("lblTitleGuide");

            var dvGuidedFromMenu = document.getElementById("dvSwitchErgoGuidedFromMenu");
            var chckGuidedFromMenu = document.getElementById("chckNwErgoGuidedFromMenu");

            var dvGuidedFromBookmark = document.getElementById("dvSwitchErgoGuidedFromBookmark");
            var chckGuidedFromBookmark = document.getElementById("chckNwErgoGuidedFromBookmark");

            var bErgoChecked = ddl.options[ddl.selectedIndex].value != EUDONETX_IRIS_BLACK_STATUS.DISABLED;

            spn.innerHTML = verifDdlErgoEnabled(this, eudonetXIrisBlackValueLabels); // Le mode fiche Eudonet x est activé / Le mode fiche Eudonet x est désactivé / Le mode fiche Eudonet x est en prévisualisation
            switchBtn.title = !bErgoChecked ? top._res_2404 : top._res_2405; // Activer le mode fiche Eudonet x / Désactiver le mode fiche Eudonet x - TODO SWITCH 3 POSITIONS
            ddl.title = switchBtn.title;

            var txt = document.querySelectorAll("#dvCntPref textarea, #dvCntPref input[type='text']");
            for (var i = 0; i < txt.length; i++) {
                if (txt[i].id != "txtTitleGuided" || !bErgoChecked)
                    txt[i].disabled = !bErgoChecked
            }

            dvGuided.style.display = "";
            //dvGuidedFromMenu.style.display = "";
            //dvGuidedFromBookmark.style.display = "";

            if (!bErgoChecked) {
                chckGuided.checked = bErgoChecked;
                dvGuided.style.display = "none";
                lblGuided.style.display = "none";

                chckGuidedFromMenu.checked = bErgoChecked;
                dvGuidedFromMenu.style.display = "none";

                chckGuidedFromBookmark.checked = bErgoChecked;
                dvGuidedFromBookmark.style.display = "none";

            }

        },
        obj.EudonetXIrisBlackStatus == EUDONETX_IRIS_BLACK_STATUS.DISABLED ? top._res_2404 : top._res_2405 // Activer le mode fiche Eudonet x / Désactiver le mode fiche Eudonet x - TODO SWITCH 3 POSITIONS
    );


    /** Checkbox pour savoir si on affiche la liste */
    var divSwitchList = nsAdmin.createSwitchPref("ErgoList",
        obj.EudonetXIrisCrimsonListStatus,
        function () {
            return verifCkErgoChecked(ickSwitch, top._res_2885, top._res_2886);// Le mode liste Eudonet x est activé / Le mode liste Eudonet x est désactivé
        },
        function () {
            var spn = document.querySelector("#spnNwErgoList");
            var chck = document.getElementById("chckNwErgoList");
            var switchBtn = document.querySelector("label[for='chckNwErgoList']");
            var bErgoChecked = chck.checked;

            spn.innerHTML = verifCkErgoChecked(this, top._res_2885, top._res_2886); // Le mode liste Eudonet x est activé / Le mode liste Eudonet x est désactivé
            switchBtn.title = bErgoChecked ? top._res_2673 : top._res_2672; // Désactiver le mode liste Eudonet x / Activer le mode liste Eudonet X
        },
        obj.EudonetXIrisCrimsonListStatus == EUDONETX_IRIS_CRIMSON_LIST_STATUS.ENABLED ? top._res_2673 : top._res_2672 // Désactiver le mode liste Eudonet x / Activer le mode liste Eudonet X
    );


    /** Checkbox pour savoir si on affiche le mode téléguidé */
    var divSwitchGuided = nsAdmin.createSwitchPref("ErgoGuided",
        obj.EudonetXIrisPurpleGuidedStatus,
        function () {
            var bErgoChecked = obj.EudonetXIrisBlackStatus != EUDONETX_IRIS_BLACK_STATUS.DISABLED; // TODO SWITCH 3 POSITIONS

            dvSwitch.style.display = ""

            if (!bErgoChecked) {
                dvSwitch.style.display = "none";
                ickSwitch.checked = bErgoChecked;
            }

            return verifCkErgoChecked(ickSwitch, top._res_2844, top._res_2845); // L'ajout par saisie guidée est activé / L'ajout par saisie guidée est désactivé
        },
        function () {
            var spn = document.querySelector("#spnNwErgoGuided");
            var chck = document.getElementById("chckNwErgoGuided");
            var switchBtn = document.querySelector("label[for='chckNwErgoGuided']");
            var txt = document.getElementById("txtTitleGuided");
            var lbl = document.getElementById("lblTitleGuide");

            var dvGuidedFromMenu = document.getElementById("dvSwitchErgoGuidedFromMenu");
            var chckGuidedFromMenu = document.getElementById("chckNwErgoGuidedFromMenu");

            var dvGuidedFromBookmark = document.getElementById("dvSwitchErgoGuidedFromBookmark");
            var chckGuidedFromBookmark = document.getElementById("chckNwErgoGuidedFromBookmark");


            var bErgoGuidedChecked = chck.checked;

            spn.innerHTML = verifCkErgoChecked(this, top._res_2844, top._res_2845); // L'ajout par saisie guidée est activé / L'ajout par saisie guidée est désactivé
            switchBtn.title = bErgoGuidedChecked ? top._res_2890 : top._res_2889; // Désactiver l'ajout par saisie guidée / Activer l'ajout par saisie guidée

            if (chck) {
                if (txt)
                    txt.disabled = !chck.checked;

                if (lbl) {
                    // US #2 979 : pour l'instant, ce libellé n'est pas affiché. Il le sera lorsque la zone assistant sera proposée sur le mode Saisie guidée.
                    //lbl.style.display = !chck.checked ? "none" : "";
                    lbl.style.display = "none";
                }
            }
            dvGuidedFromMenu.style.display = "";
            dvGuidedFromBookmark.style.display = "";

            if (!bErgoGuidedChecked) {

                chckGuidedFromMenu.checked = false;
                dvGuidedFromMenu.style.display = "none";

                chckGuidedFromBookmark.checked = false;
                dvGuidedFromBookmark.style.display = "none";
            }

        },
        obj.EudonetXIrisPurpleGuidedStatus == EUDONETX_IRIS_PURPLE_GUIDED_STATUS.ENABLED ? top._res_2890 : top._res_2889 // Désactiver l'ajout par saisie guidée / Activer l'ajout par saisie guidée
    );

    var isPurpleActivatedForMenu = obj.PurpleActivatedFrom.some(i => i == obj.LOCATION_PURPLE_ACTIVATED.NAVBAR) && obj.PurpleActivatedFrom.some(i => i == obj.LOCATION_PURPLE_ACTIVATED.MENU)
    /** Checkbox pour savoir si on affiche le mode téléguidé depuis le menu et la navbar */
    var divSwitchGuidedFromMenu = nsAdmin.createSwitchPref("ErgoGuidedFromMenu",
        isPurpleActivatedForMenu,
        function () {
            var bErgoChecked = obj.EudonetXIrisBlackStatus != EUDONETX_IRIS_BLACK_STATUS.DISABLED; // TODO SWITCH 3 POSITIONS

            dvSwitch.style.display = ""

            if (!bErgoChecked) {
                dvSwitch.style.display = "none";
                ickSwitch.checked = bErgoChecked;
            }

            //3012	L'ajout par saisie guidée est disponible depuis le menu et la barre de navigation
            //3013	L'ajout par saisie guidée n'est pas disponible depuis le menu et la barre de navigation
            return verifCkErgoChecked(ickSwitch, top._res_3012, top._res_3013); // L'ajout par saisie guidée est activé depuis le menu de droite et la barre de navigation / L'ajout par saisie guidée est désactivé
        },
        function () {
            var spn = document.querySelector("#spnNwErgoGuidedFromMenu");
            var chck = document.getElementById("chckNwErgoGuidedFromMenu");
            var switchBtn = document.querySelector("label[for='chckNwErgoGuidedFromMenu']");
            var txt = document.getElementById("txtTitleGuidedFromMenu");
            var lbl = document.getElementById("lblTitleGuideFromMenu");
            var bErgoChecked = chck.checked;

            //3012	L'ajout par saisie guidée est disponible depuis le menu et la barre de navigation
            //3013	L'ajout par saisie guidée n'est pas disponible depuis le menu et la barre de navigation
            spn.innerHTML = verifCkErgoChecked(this, top._res_3012, top._res_3013);

            //3010	Activer la saisie guidée depuis le menu et la barre de navigation
            //3011	Désactiver la saisie guidée depuis le menu et la barre de navigation
            switchBtn.title = bErgoChecked ? top._res_3011 : top._res_3010;

            if (chck) {
                if (txt)
                    txt.disabled = !chck.checked;

                if (lbl) {
                    // US #2 979 : pour l'instant, ce libellé n'est pas affiché. Il le sera lorsque la zone assistant sera proposée sur le mode Saisie guidée.
                    //lbl.style.display = !chck.checked ? "none" : "";
                    lbl.style.display = "none";
                }
            }
        },
        //3010	Activer la saisie guidée depuis le menu et la barre de navigation
        //3011	Désactiver la saisie guidée depuis le menu et la barre de navigation
        isPurpleActivatedForMenu ? top._res_3011 : top._res_3010 // Désactiver l'ajout par saisie guidée / Activer l'ajout par saisie guidée
    );

    var isPurpleActivatedForBookmark = obj.PurpleActivatedFrom.some(i => i == obj.LOCATION_PURPLE_ACTIVATED.BOOKMARK);
    /** Checkbox pour savoir si on affiche le mode téléguidé depuis le menu et la navbar */
    var divSwitchGuidedFromBookmark = nsAdmin.createSwitchPref("ErgoGuidedFromBookmark",
        isPurpleActivatedForBookmark,
        function () {
            var bErgoChecked = obj.EudonetXIrisBlackStatus != EUDONETX_IRIS_BLACK_STATUS.DISABLED; // TODO SWITCH 3 POSITIONS

            dvSwitch.style.display = ""

            if (!bErgoChecked) {
                dvSwitch.style.display = "none";
                ickSwitch.checked = bErgoChecked;
            }

            //3008	L'ajout par saisie guidée est disponible depuis un signet
            //3009	L'ajout par saisie guidée n'est pas disponible depuis un signet
            return verifCkErgoChecked(ickSwitch, top._res_3008, top._res_3009);
        },
        function () {
            var spn = document.querySelector("#spnNwErgoGuidedFromBookmark");
            var chck = document.getElementById("chckNwErgoGuidedFromBookmark");
            var switchBtn = document.querySelector("label[for='chckNwErgoGuidedFromBookmark']");
            var txt = document.getElementById("txtTitleGuidedFromBookmark");
            var lbl = document.getElementById("lblTitleGuideFromBookmark");
            var bErgoChecked = chck.checked;

            //3008	L'ajout par saisie guidée est disponible depuis un signet
            //3009	L'ajout par saisie guidée n'est pas disponible depuis un signet
            spn.innerHTML = verifCkErgoChecked(this, top._res_3008, top._res_3009);

            //3006	Activer la saisie guidée depuis un signet
            //3007	Désactiver la saisie guidée depuis un signet
            switchBtn.title = bErgoChecked ? top._res_3007 : top._res_3006;

            if (chck) {
                if (txt)
                    txt.disabled = !chck.checked;

                if (lbl) {
                    // US #2 979 : pour l'instant, ce libellé n'est pas affiché. Il le sera lorsque la zone assistant sera proposée sur le mode Saisie guidée.
                    //lbl.style.display = !chck.checked ? "none" : "";
                    lbl.style.display = "none";
                }
            }
        },
        //3006	Activer la saisie guidée depuis un signet
        //3007	Désactiver la saisie guidée depuis un signet
        isPurpleActivatedForBookmark ? top._res_3007 : top._res_3006
    );

    var sTooltip = "";
    if (obj.parents) {
        var sParents = "";
        for (const [key, value] of Object.entries(obj.parents)) {
            if (sParents.length > 0)
                sParents += ", ";
            sParents += top._res_3120.replace("{DESCID}", key).replace("{TABNAME}", value);
        }

        sTooltip = top._res_3119.replace("{PARENTS}", sParents);
    }


    /** JSON de la structure de la zone résumé. */
    var titleSummary = nsAdmin.createTextAreaJson({
        title: top._res_2425,
        id: "txtTitleSummary",
        rows: "20",
        cols: "120",
        spellcheck: false,
        disabled: divSwitch.querySelector("#ddlNwErgo").options[divSwitch.querySelector("#ddlNwErgo").selectedIndex].value == EUDONETX_IRIS_BLACK_STATUS.DISABLED, // TODO SWITCH 3 POSITIONS
        value: (obj.sJSonPageIrisBlack && obj.sJSonPageIrisBlack["Page"] ? JSON.stringify(obj.sJSonPageIrisBlack.Page["Summary"] || obj.sJSonPageIrisBlack.Page["JsonSummary"], null, 4) : ""),
        buttons: [{
            id: "btnGenerateJSONSummary",
            label: top._res_2962, // Générer un modèle par défaut
            function: nsAdmin.generateJson
        }],
        tooltip: sTooltip
    });


    /* JSON de la structure de la zone assistant. */
    // Le bouton de génération de ce JSON n'est affiché que si des catalogues sont disponibles
    // US #4315 : et si l'utilisateur a le droit de modifier la zone Assistant
    var titleWizardButtons = null;
    if (obj.CanUpdateWizardBar && nsAdmin.displayPreferencesGenerateJSONCatalogList && nsAdmin.displayPreferencesGenerateJSONCatalogList.length > 0) {
        titleWizardButtons = [{
            id: "btnGenerateJSONWizard",
            label: top._res_2962, // Générer un modèle par défaut
            function: nsAdmin.generateJson
        }];
    }
    var titleWizard = nsAdmin.createTextAreaJson({
        title: top._res_2426,
        id: "txtTitleWizard",
        rows: "20",
        cols: "120",
        spellcheck: false,
        disabled: !obj.CanUpdateWizardBar || (divSwitch.querySelector("#ddlNwErgo").options[divSwitch.querySelector("#ddlNwErgo").selectedIndex].value == EUDONETX_IRIS_BLACK_STATUS.DISABLED), // TODO SWITCH 3 POSITIONS
        value: (obj.sJSonPageIrisBlack && obj.sJSonPageIrisBlack["Page"] ? JSON.stringify(obj.sJSonPageIrisBlack.Page["WizardBar"] || obj.sJSonPageIrisBlack.Page["JsonWizardBar"], null, 4) : ""),
        buttons: titleWizardButtons
    });

    /** JSON de la structure du mode guidé. */
    var txtGuided = nsAdmin.createTextAreaJson({
        title: top._res_2847,
        id: "txtTitleGuided",
        rows: "20",
        cols: "120",
        spellcheck: false,
        disabled: !(divSwitchGuided.querySelector("#chckNwErgoGuided").checked),
        value: (obj.sJsonGuidedsJSonGuidedIrisPurple ? JSON.stringify(obj.sJsonGuidedsJSonGuidedIrisPurple, null, 4) : "")
    });

    var lblTitleGuide = document.createElement("b");
    lblTitleGuide.innerHTML = top._res_2846;
    lblTitleGuide.id = "lblTitleGuide";
    // US #2 979 : pour l'instant, ce libellé n'est pas affiché. Il le sera lorsque la zone assistant sera proposée sur le mode Saisie guidée.
    //lblTitleGuide.style.display = obj.EudonetXIrisPurpleGuidedStatus == EUDONETX_IRIS_PURPLE_GUIDED_STATUS.ENABLED ? "" : "none";
    lblTitleGuide.style.display = "none";

    if (IsDebuggingEnabled == true) {
        divContent.appendChild(divSwitchList);
        divContent.appendChild(document.createElement("br"));
        divContent.appendChild(document.createElement("br"));
    }

    divContent.appendChild(divSwitch);
    divContent.appendChild(document.createElement("br"));
    divContent.appendChild(document.createElement("br"));

    //if (IsDebuggingEnabled == true) {
    divContent.appendChild(divSwitchGuided);
    divContent.appendChild(document.createElement("br"));
    divContent.appendChild(document.createElement("br"));

    divContent.appendChild(divSwitchGuidedFromMenu);
    divContent.appendChild(document.createElement("br"));
    divContent.appendChild(document.createElement("br"));

    divContent.appendChild(divSwitchGuidedFromBookmark);
    divContent.appendChild(document.createElement("br"));
    divContent.appendChild(document.createElement("br"));
    //}

    divContent.appendChild(titleSummary.title);
    divContent.appendChild(document.createElement("br"))
    divContent.appendChild(titleSummary.txt);
    divContent.appendChild(document.createElement("br"))
    divContent.appendChild(titleSummary.tooltip);
    divContent.appendChild(document.createElement("br"));
    divContent.appendChild(document.createElement("br"));
    divContent.appendChild(titleWizard.title);
    divContent.appendChild(document.createElement("br"));
    divContent.appendChild(titleWizard.txt);

    //if (IsDebuggingEnabled == true) {
    divContent.appendChild(lblTitleGuide);

    divContent.appendChild(document.createElement("br"));
    divContent.appendChild(document.createElement("br"));
    divContent.appendChild(txtGuided.title);
    divContent.appendChild(document.createElement("br"));
    divContent.appendChild(txtGuided.txt);
    //}
    modalConfigFile.setElement(divContent);

    modalConfigFile.show();

    modalConfigFile.addButton(top._res_30, function () {
        modalConfigFile.hide();
    }, 'button-gray', null);

    /**
     * Si on clique sur ok, on enregistre les données via le
     * manager qui permet d'enregistrer dans descadv.
     */
    modalConfigFile.addButton(top._res_28, function () {
        txtTitleSummary = document.getElementById("txtTitleSummary");
        txtTitleWizard = document.getElementById("txtTitleWizard");
        var ddlSwitch = document.getElementById("ddlNwErgo");
        var ickSwitchList = document.getElementById("chckNwErgoList");
        var ickSwitchGuided = document.getElementById("chckNwErgoGuided")
        var ickSwitchGuidedFromMenu = document.getElementById("chckNwErgoGuidedFromMenu")
        var ickSwitchGuidedFromBookmark = document.getElementById("chckNwErgoGuidedFromBookmark")


        try {
            obj.sJSonPageIrisBlack = {
                Page: {
                    JsonSummary: (!txtTitleSummary.value) ? {} : nsAdmin.parseJson(txtTitleSummary.value.replace("`", ""), top._res_2425, function (key, value) {
                        return nsAdmin.checkJsonAndCorrect(key, value);
                    }),
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
                    JsonWizardBar: (!txtTitleWizard.value) ? {} : nsAdmin.parseJson(txtTitleWizard.value.replace("`", ""), top._res_2426, function (key, value) {
                        return nsAdmin.checkJsonAndCorrect(key, value);
                    }),
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
                }
            };
        } catch (e) {
            eAlert(0, top._res_8665, e.message); // Erreur de validation des informations
            return;
        }

        var caps = new Capsule(obj.DescId);


        // if (IsDebuggingEnabled == true) {
        var objGuided;

        try {
            if (txtTitleGuided && txtTitleGuided.value)
                objGuided = nsAdmin.parseJson(txtTitleGuided.value.replace("`", ""), top._res_2847, function (key, value) {
                    return nsAdmin.checkJsonAndCorrect(key, value);
                });
        } catch (e) {
            eAlert(0, top._res_8665, e.message); // Erreur de validation des informations
            return;
        }
        // }

        if (IsDebuggingEnabled == true)
            caps.AddProperty(obj.CATEGORY, obj.DESCADV_PARAMETER.ERGONOMICS_LIST_IRIS_CRIMSON, (!ickSwitchList.checked ? "0" : "1"));

        caps.AddProperty(obj.CATEGORY, obj.DESCADV_PARAMETER.ERGONOMICS_IRIS_BLACK, ddlSwitch.options[ddlSwitch.selectedIndex].value);
        caps.AddProperty(obj.CATEGORY, obj.DESCADV_PARAMETER.JSON_STRUCTURE_IRIS_BLACK, JSON.stringify(obj.sJSonPageIrisBlack));

        //if (IsDebuggingEnabled == true) {
        caps.AddProperty(obj.CATEGORY, obj.DESCADV_PARAMETER.ERGONOMICS_GUIDED_IRIS_PURPLE, (!ickSwitchGuided.checked ? "0" : "1"));
        caps.AddProperty(obj.CATEGORY, obj.DESCADV_PARAMETER.JSON_STRUCTURE_GUIDED_IRIS_PURPLE, JSON.stringify(objGuided));
        var arPurpleActivatedFrom = [];
        if (ickSwitchGuidedFromMenu.checked) {
            arPurpleActivatedFrom.push(obj.LOCATION_PURPLE_ACTIVATED.NAVBAR);
            arPurpleActivatedFrom.push(obj.LOCATION_PURPLE_ACTIVATED.MENU);
        }
        if (ickSwitchGuidedFromBookmark.checked) {
            arPurpleActivatedFrom.push(obj.LOCATION_PURPLE_ACTIVATED.BOOKMARK);
        }
        caps.AddProperty(obj.CATEGORY, obj.DESCADV_PARAMETER.PURPLE_ACTIVATED_FROM, arPurpleActivatedFrom.join());
        //}


        var upd = new eUpdater("eda/Mgr/eAdminDescManager.ashx", 1);
        upd.json = JSON.stringify(caps);

        upd.ErrorCallBack = function () {
            setWait(false);
            modalConfigFile.hide();
        };
        setWait(true);

        upd.send(function (oRes) {
            var buttonAdminPreferences = document.getElementById("buttonAdminPreferences");

            if (buttonAdminPreferences) {
                obj.EudonetXIrisBlackStatus = ddlSwitch.options[ddlSwitch.selectedIndex].value;

                obj.EudonetXIrisPurpleGuidedStatus = ickSwitchGuided.checked ? EUDONETX_IRIS_PURPLE_GUIDED_STATUS.ENABLED : EUDONETX_IRIS_PURPLE_GUIDED_STATUS.DISABLED;
                try {
                    if (txtTitleGuided && txtTitleGuided.value)
                        obj.sJsonGuidedsJSonGuidedIrisPurple = txtTitleGJsonGuidedsJSonGuidedIrisPurple = txtTitleGuided.value.replace("`", "") != ""
                            ? JSON.parse(txtTitleGuided.value.replace("`", "")) : {};
                } catch (e) {
                    eAlert(0, "", e.message);
                    obj.sJsonGuidedsJSonGuidedIrisPurple = {};
                    setWait(false);
                    modalConfigFile.hide();
                    return;
                }

                if (IsDebuggingEnabled == true) {
                    obj.EudonetXIrisCrimsonListStatus = ickSwitchList.checked ? EUDONETX_IRIS_CRIMSON_LIST_STATUS.ENABLED : EUDONETX_IRIS_CRIMSON_LIST_STATUS.DISABLED;
                }

                try {
                    obj.sJSonPageIrisBlack = {
                        Page: {
                            JsonSummary: (!txtTitleSummary.value) ? {} : nsAdmin.parseJson(txtTitleSummary.value.replace("`", ""), top._res_2425),
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
                            JsonWizardBar: (!txtTitleWizard.value) ? {} : nsAdmin.parseJson(txtTitleWizard.value.replace("`", ""), top._res_2426),
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
                        }
                    };

                    let TypeIris;

                    nsAdmin.fnInsertNewTab(["dvIrisBlackInput", "dvIrisBlackInputPreview"], EUDONETX_IRIS_CRIMSON_LIST_STATUS.DISABLED);

                    if (obj.EudonetXIrisBlackStatus == EUDONETX_IRIS_BLACK_STATUS.PREVIEW)
                        TypeIris = "dvIrisBlackInputPreview";

                    if (obj.EudonetXIrisBlackStatus == EUDONETX_IRIS_BLACK_STATUS.ENABLED)
                        TypeIris = "dvIrisBlackInput";

                    if (TypeIris)
                        nsAdmin.fnInsertNewTab(TypeIris, obj.EudonetXIrisBlackStatus);


                } catch (e) {
                    eAlert(0, top._res_8665, e.message); // Erreur de validation des informations
                    obj.sJSonPageIrisBlack = {};
                    setWait(false);
                    modalConfigFile.hide();
                    return;
                }

                buttonAdminPreferences.onclick = function () { nsAdmin.confPreferences(JSON.stringify(obj), IsDebuggingEnabled) };
            }

            if (ddlSwitch.options[ddlSwitch.selectedIndex].value != oldIrisBlackStatus)
                nsAdmin.loadAdminFile(obj.DescId);

            setWait(false);
            modalConfigFile.hide();
        });


    }, 'button-green', null);

    nsAdmin.modalResizeAndMove(modalConfigFile);
}

nsAdmin.confMiniFile = function () {
    modalMiniFile = new eModalDialog(top._res_7020, 0, "eda/eAdminMiniFileDialog.aspx", 985, 652, "modalAdminMiniFile");

    modalMiniFile.noButtons = false;
    modalMiniFile.addParam("tab", nsAdmin.tab, "post");

    modalMiniFile.show();

    modalMiniFile.addButton(top._res_30, function () { modalMiniFile.hide(); }, 'button-gray', null);
    modalMiniFile.addButton(top._res_28, function () {
        modalMiniFile.getIframe().nsAdminMiniFile.updateMiniFile(nsAdmin.tab, MiniFileType.File);
    }, 'button-green', null);

    nsAdmin.modalResizeAndMove(modalMiniFile);
}

nsAdmin.confAutocompleteAddress = function () {
    var modalHeight = 440;
    //  if (nsAdmin.tab == 300)
    //     modalHeight += 55;
    modalAutocompleteAddress = new eModalDialog(top._res_7024, 0, "eda/eAdminAutocompleteAddressDialog.aspx", 880, 530, "modalAutocompleteAddress");

    modalAutocompleteAddress.noButtons = false;
    modalAutocompleteAddress.addParam("tab", nsAdmin.tab, "post");
    //modalAutocompleteAddress.addParam("iframeScrolling", "yes", "post");
    modalAutocompleteAddress.show();

    modalAutocompleteAddress.addButton(top._res_30, function () { modalAutocompleteAddress.hide(); }, 'button-gray', null);
    modalAutocompleteAddress.addButton(top._res_28, function () { nsAdmin.updateAutocompleteAddress(); }, 'button-green', null);

    nsAdmin.modalResizeAndMove(modalAutocompleteAddress);
}

nsAdmin.updateAutocompleteAddress = function () {
    var modal = document.getElementById(modalAutocompleteAddress.iframeId);
    var modalContent = modal.contentDocument || modal.contentWindow.document;

    var obj = {};
    var rubrique = {};

    rubrique = 0;
    var elSearchField = modalContent.getElementById("ddl" + "searchField");
    if (elSearchField != null)
        rubrique = elSearchField.value;
    obj.searchField = rubrique;

    var fields = ["houseNumber", "streetName", "postalCode", "city", "cityCode", "departmentNumber", "department", "region", "country", "geography", "label"];
    fields.forEach(function (value, index, array) {
        var field = value;

        rubrique = { value: 0, id: 0, addcatvalue: false };

        var elField = modalContent.getElementById("ddl" + field);
        if (elField != null) {
            rubrique.value = elField.value;

            if (elField.hasAttribute("edaAcaMpid")) {
                rubrique.id = elField.getAttribute("edaAcaMpid");
            }

            var divField = findUpByClass(elField, "field");
            if (divField) {
                var chk = divField.querySelector(".edaAcaAddCatalogValue .chk");
                rubrique.addcatvalue = getAttributeValue(chk, "chk") == "1";
            }
        }

        obj[field] = rubrique;
    });

    var fields = ["midFormula", "pmAutomation"];
    fields.forEach(function (value, index, array) {
        var field = value;

        rubrique = true;

        var radios = modalContent.getElementsByName("rbl" + field);

        for (var i = 0, length = radios.length; i < length; ++i) {
            if (radios[i].checked) {
                if (radios[i].value == "true")
                    rubrique = true;

                if (radios[i].value == "false")
                    rubrique = false;

                break;
            }
        }

        obj[field] = rubrique;
    });

    var upd = new eUpdater("eda/Mgr/eAdminAutocompleteAddressDialogManager.ashx", 1);
    upd.addParam("tab", nsAdmin.tab, "post");
    upd.addParam("mappingsJson", JSON.stringify(obj), "post");

    upd.send(
        // Résultat
        function (oRes) {
            var res = JSON.parse(oRes);
            if (!res.Success) {
                eAlert(1, "Mise à jour mapping Recherche d'adresses prédictive", res.Error);
            }
            else {
                //
            }
        }
    );

    modalAutocompleteAddress.hide();
}

nsAdmin.adminAutocompletionToggleSearch = function (elSearchField) {
    var newValue = elSearchField.value;

    var fields = ["midFormula", "pmAutomation"];
    fields.forEach(function (value, index, array) {
        var field = value;

        var radios = document.getElementsByName("rbl" + field);

        for (var i = 0, length = radios.length; i < length; ++i) {
            if (elSearchField.value == "0")
                radios[i].disabled = true;
            else
                radios[i].disabled = false;
        }
    });
}

nsAdmin.adminAutocompletionToggleField = function (elToggleField) {
    var newValue = elToggleField.value;

    // Si la rubrique sélectionnée est un champ de type catalogue avancé, on active la case à cocher
    var isAdvCatField = getAttributeValue(elToggleField.options[elToggleField.selectedIndex], "data-advcat") == "1";
    var divField = findUpByClass(elToggleField, "field");
    if (divField) {
        var chk = divField.querySelector(".edaAcaAddCatalogValue .chk");
        if (chk) {
            disChk(chk, !isAdvCatField);
            if (!isAdvCatField)
                chgChk(chk, false);
        }
    }

    var oldValue = 0;
    if (elToggleField.hasAttribute("edaAcaOldvalue")) {
        oldValue = elToggleField.getAttribute("edaAcaOldvalue");
    }

    var fields = ["houseNumber", "streetName", "postalCode", "city", "cityCode", "departmentNumber", "department", "region", "geography"];
    fields.forEach(function (value, index, array) {
        var field = value;

        var elField = document.getElementById("ddl" + field);
        if (elField != null && elField != elToggleField) {
            var ops = elField.getElementsByTagName("option");

            for (var i = 0; i < ops.length; ++i) {
                var op = ops[i];
                if (newValue != "0" && op.value == newValue)
                    op.disabled = true;

                if (oldValue != "0" && op.value == oldValue)
                    op.disabled = false;
            }
        }
    });

    elToggleField.setAttribute("edaAcaOldvalue", newValue);
}

nsAdmin.confSirene = function () {
    var modalHeight = 530;
    //  if (nsAdmin.tab == 300)
    //     modalHeight += 55;
    modalSirene = new eModalDialog(top._res_8546, 0, "eda/eAdminSireneDialog.aspx", 880, modalHeight, "modalSirene");

    modalSirene.noButtons = false;
    modalSirene.addParam("tab", nsAdmin.tab, "post");
    //modalSirene.addParam("iframeScrolling", "yes", "post");
    modalSirene.show();

    modalSirene.addButton(top._res_30, function () { modalSirene.hide(); }, 'button-gray', null);
    modalSirene.addButton(top._res_28, function () { nsAdmin.updateSirene(); }, 'button-green', null);

    nsAdmin.modalResizeAndMove(modalSirene);
}


nsAdmin.updateSirene = function () {
    var modal = document.getElementById(modalSirene.iframeId);
    var modalContent = modal.contentDocument || modal.contentWindow.document;

    var obj = {};
    var fieldData = {};
    var arrayMappings = {};

    fieldData = 0;

    var elSearchField1 = modalContent.getElementById("ddl" + "searchField1");
    if (elSearchField1 != null)
        fieldData = elSearchField1.value;
    obj.searchField1 = fieldData;

    var elSearchField2 = modalContent.getElementById("ddl" + "searchField2");
    if (elSearchField2 != null)
        fieldData = elSearchField2.value;
    obj.searchField2 = fieldData;

    var elResultLabelField = modalContent.getElementById("ddl" + "resultLabelField");
    if (elResultLabelField != null)
        fieldData = elResultLabelField.value;
    obj.resultLabelField = fieldData;

    var fields = modalContent.querySelectorAll('select[id^=ddlE]');
    var arrayMappings = [];

    // MAB - #60 518 et #63 849 - 2018-03-02 - Attention, forEach ne fonctionne pas sur les objets de type NodeList sur les anciens navigateurs et IE
    // Source: https://stackoverflow.com/questions/13433799/why-doesnt-nodelist-have-foreach
    // Utiliser, dans ce cas, Array.prototype.slice.call(nodelist).forEach(...)
    // On pourrait utiliser la copie (mutation) de prototype, comme ceci, mais ça n'est pas recommandé pour IE (encore lui) :
    // NodeList.prototype.forEach = Array.prototype.forEach;

    Array.prototype.slice.call(fields).forEach(function (elField) {
        //fields.forEach(function (elField, index, array) {
        fieldData = { field: '', descid: 0, addnewvalue: false };

        fieldData.descid = elField.value;

        if (elField.hasAttribute("edaSireneMpid")) {
            fieldData.field = elField.getAttribute("edaSireneMpid");
        }

        var divField = findUpByClass(elField, "field");
        if (divField) {
            var chk = divField.querySelector(".edaSireneAddCatalogValue .chk");
            fieldData.addnewvalue = getAttributeValue(chk, "chk") == "1";
        }

        arrayMappings.push(fieldData);
    });

    var fields = ["midFormula", "pmAutomation"];
    fields.forEach(function (value, index, array) {
        var field = value;

        fieldData = true;

        var radios = modalContent.getElementsByName("rbl" + field);

        for (var i = 0, length = radios.length; i < length; ++i) {
            if (radios[i].checked) {
                if (radios[i].value == "true")
                    fieldData = true;

                if (radios[i].value == "false")
                    fieldData = false;

                break;
            }
        }

        obj[field] = fieldData;
    });

    obj.mappings = arrayMappings;

    var upd = new eUpdater("eda/Mgr/eAdminSireneDialogManager.ashx", 1);
    upd.addParam("tab", nsAdmin.tab, "post");
    upd.addParam("mappingsJson", JSON.stringify(obj), "post");

    upd.send(
        // Résultat
        function (oRes) {
            var res = JSON.parse(oRes);
            if (!res.Success) {
                eAlert(1, "Mise à jour mapping Sirene", res.Error);
            }
            else {
                //
            }
        }
    );

    modalSirene.hide();
}

nsAdmin.adminSireneToggleSearch = function (elSearchField) {
    var newValue = elSearchField.value;

    var fields = ["midFormula", "pmAutomation"];
    fields.forEach(function (value, index, array) {
        var field = value;

        var radios = document.getElementsByName("rbl" + field);

        for (var i = 0, length = radios.length; i < length; ++i) {
            if (elSearchField.value == "0")
                radios[i].disabled = true;
            else
                radios[i].disabled = false;
        }
    });
}

nsAdmin.adminSireneToggleField = function (elToggleField) {
    var newValue = elToggleField.value;

    // Si la rubrique sélectionnée est un champ de type catalogue avancé, on active la case à cocher
    var isAdvCatField = getAttributeValue(elToggleField.options[elToggleField.selectedIndex], "data-advcat") == "1";
    var divField = findUpByClass(elToggleField, "field");
    if (divField) {
        var chk = divField.querySelector(".edaSireneAddCatalogValue .chk");
        if (chk) {
            chk.style.visibility = isAdvCatField ? 'visible' : 'hidden';
            chk.parentElement.style.visibility = isAdvCatField ? 'visible' : 'hidden';
            disChk(chk, !isAdvCatField);
            if (!isAdvCatField)
                chgChk(chk, false);
        }
    }

    var oldValue = 0;
    if (elToggleField.hasAttribute("edaSireneOldvalue")) {
        oldValue = elToggleField.getAttribute("edaSireneOldvalue");
    }

    var fields = document.querySelectorAll('select[id^=ddlE]');
    fields.forEach(function (elField, index, array) {
        if (elField != elToggleField) {
            var ops = elField.getElementsByTagName("option");

            for (var i = 0; i < ops.length; ++i) {
                var op = ops[i];
                if (newValue != "0" && op.value == newValue)
                    op.disabled = true;

                if (oldValue != "0" && op.value == oldValue)
                    op.disabled = false;
            }
        }
    });

    elToggleField.setAttribute("edaSireneOldvalue", newValue);

    // Si le champ a été marqué en conflit au chargement de la page, on rétablit son aspect normal
    // (les options déjà mappées étant désactivées, on ne peut théoriquement plus resélectionner d'option en conflit)
    var fieldLine = findUp(elToggleField, "div");
    removeClass(elToggleField, "mappingConflict");
}

nsAdmin.confMapTooltip = function () {
    modalMapTooltip = new eModalDialog(top._res_7106, 0, "eda/eAdminMapTooltipDialog.aspx", 985, 520, "modalMapTooltip");

    modalMapTooltip.noButtons = false;
    modalMapTooltip.addParam("tab", nsAdmin.tab, "post");
    //modalMapTooltip.addParam("iframeScrolling", "yes", "post");
    modalMapTooltip.show();

    modalMapTooltip.addButton(top._res_30, function () { modalMapTooltip.hide(); }, 'button-gray', null);
    modalMapTooltip.addButton(top._res_28, function () { nsAdmin.updateMapTooltip(); }, 'button-green', null);

    nsAdmin.modalResizeAndMove(modalMapTooltip);
}

nsAdmin.updateMapTooltip = function () {
    var modal = document.getElementById(modalMapTooltip.iframeId);
    var modalContent = modal.contentDocument || modal.contentWindow.document;

    var obj = {};
    var rubrique = {};

    var fields = ["Geography", "Title", "Subtitle", "Image"];

    for (var i = 1; i <= 5; ++i) {
        fields.push("Rubrique" + i);
    }

    fields.forEach(function (value, index, array) {
        var field = value;

        rubrique = { value: 0, id: 0, label: false };

        var elField = modalContent.getElementById("ddlFields" + field);
        if (elField != null) {
            rubrique.value = elField.value;

            if (elField.hasAttribute("edaMpTtMpid")) {
                rubrique.id = elField.getAttribute("edaMpTtMpid");
            }
        }

        var elRubLib = modalContent.getElementById("chkbxLib" + field);
        if (elRubLib != null && elRubLib.hasAttribute("chk")) {
            rubrique.label = elRubLib.getAttribute("chk") == "1" ? true : false;
        }

        obj[field] = rubrique;
    });

    var upd = new eUpdater("eda/Mgr/eAdminMapTooltipDialogManager.ashx", 1);
    upd.addParam("tab", nsAdmin.tab, "post");
    upd.addParam("mappingsJson", JSON.stringify(obj), "post");

    upd.send(
        // Résultat
        function (oRes) {
            var res = JSON.parse(oRes);
            if (!res.Success) {
                eAlert(1, "Mise à jour mapping Infobulle de position", res.Error);
            }
            else {
                //
            }
        }
    );

    modalMapTooltip.hide();
}

nsAdmin.adminMapTooltipHoverField = function (divId, display) {
    var div = document.getElementById(divId);

    if (div != null) {
        if (display == true)
            div.style.display = "block";
        else
            div.style.display = "none";
    }
}

nsAdmin.adminMapTooltipToggleTab = function (elToggleTab, nameField) {

    var browser = new getBrowser();
    var isIE = browser.isIE;

    elField = document.getElementById(nameField);

    if (elToggleTab != null && elField != null) {
        var tabValue = elToggleTab.value;

        var ops = elField.getElementsByTagName("option");

        for (var i = 0; i < ops.length; ++i) {
            var op = ops[i];
            var opTab = 0;
            if (op.hasAttribute("edaOptTab"))
                opTab = op.getAttribute("edaOptTab");
            else
                opTab = op.value - (op.value % 100);

            if (op.value == 0 || (tabValue != 0 && tabValue == opTab)) {
                op.style.display = "block";
                if (isIE)
                    op.disabled = false;
            }
            else {
                if (elField.value == op.value)
                    elField.value = "0";
                op.style.display = "none";
                if (isIE)
                    op.disabled = true;
            }
        }
    }
}

nsAdmin.adminMapTooltipToggleField = function (elToggleField) {
    var newValue = elToggleField.value;

    var oldValue = 0;
    if (elToggleField.hasAttribute("edaMpTtOldvalue")) {
        oldValue = elToggleField.getAttribute("edaMpTtOldvalue");
    }

    var namePattern = "ddlFields";
    var fields = document.getElementsByTagName("select");

    for (var i = 0; i < fields.length; ++i) {
        var field = fields[i];

        if (field.id.substring(0, namePattern.length) == namePattern) {
            var ops = field.getElementsByTagName("option");

            for (var j = 0; j < ops.length; ++j) {
                var op = ops[j];
                if (newValue != "0" && op.value == newValue)
                    op.disabled = true;

                if (oldValue != "0" && op.value == oldValue)
                    op.disabled = false;
            }
        }
    }

    elToggleField.setAttribute("edaMpTtOldvalue", newValue);
}

nsAdmin.confAdvancedAutomatisms = function (descid) {


    if (descid > 0) {

        modalAutomatisms = new eModalDialog(top._res_7213, 0, "eda/eAdminAutomatismsDialog.aspx", 985, 625, "modalAutomatisms");

        modalAutomatisms.noButtons = false;
        modalAutomatisms.addParam("tab", nsAdmin.tab, "post");
        modalAutomatisms.addParam("field", descid, "post");
        //modalAutomatisms.addParam("iframeScrolling", "yes", "post");
        modalAutomatisms.onIframeLoadComplete = function () {
            var advAutomation = document.getElementById("buttonAdminAdvAutomations");
            if (advAutomation)
                setAttributeValue(advAutomation, "show", "1")

        };
        modalAutomatisms.show();

        modalAutomatisms.addButtonFct(top._res_30,
            function () {
                modalAutomatisms.hide();
                nsAdmin.onCloseAdvAutomatisme();
            }, 'button-gray', 'cancel');

        modalAutomatisms.addButton(top._res_28, function () {
            nsAdmin.updateAdvancedAutomatisms();
            nsAdmin.onCloseAdvAutomatisme();
        }, 'button-green', null);

        nsAdmin.modalResizeAndMove(modalAutomatisms);
    }
}

/// Compatibilité v7 : ouvereture de la fenetre des formules avec la touche F10
nsAdmin.onOpenAdvAutomatisme = function (target) {
    // Déjà ouvert, on fait rien
    if (getAttributeValue(target, "show") != "1") {
        target.click();
        setAttributeValue(target, "show", "1");
    }

}
/// on mis à jour l'attribut show
nsAdmin.onCloseAdvAutomatisme = function () {
    var advAutomation = document.getElementById("buttonAdminAdvAutomations");
    if (advAutomation)
        setAttributeValue(advAutomation, "show", "0");
}


nsAdmin.onKeyDown = function (evt) {

    if (!evt)
        evt = window.event;

    // on met le racourcis sur <a> pour l'instant
    // var target = document.querySelector("a[keycode='" + evt.keyCode + "']");

    // if (target) {
    if (evt.keyCode == 121) {
        var target = document.getElementById("buttonAdminAdvAutomations");
        //    if (getAttributeValue(target, "ctlkey") == "1" && !evt.ctlKey) return;
        //    else if (getAttributeValue(target, "altkey") == "1" && !evt.altKey) return;
        //    else if (getAttributeValue(target, "shiftkey") == "1" && !evt.shiftKey) return;
        //        if (target.id = "buttonAdminAdvAutomations")
        nsAdmin.onOpenAdvAutomatisme(target);
        //  else
        //  target.click();

    }
}
nsAdmin.updateAdvancedAutomatisms = function () {
    var modal = document.getElementById(modalAutomatisms.iframeId);
    var modalContent = modal.contentDocument || modal.contentWindow.document;

    var obj = {};

    var fields = ["top", "middle", "bottom"];

    fields.forEach(function (value, index, array) {
        var field = value;

        var elField = modalContent.getElementById("edaFormulaSQL" + field);
        if (elField != null) {
            obj[field + "Formula"] = elField.value;
        }
    });

    var fieldId = 0;
    var elFieldId = modalContent.getElementById("edaFieldId");
    if (elFieldId != null) {
        fieldId = elFieldId.value;
    }

    var upd = new eUpdater("eda/Mgr/eAdminAutomatismsDialogManager.ashx", 1);
    upd.addParam("tab", nsAdmin.tab, "post");
    upd.addParam("field", fieldId, "post");
    upd.addParam("formulasJson", JSON.stringify(obj), "post");

    upd.send(
        // Résultat
        function (oRes) {
            var res = JSON.parse(oRes);
            if (!res.Success) {
                eAlert(1, "Mise à jour automatismes avancés", res.Error);
            }
            else {
                //
            }
        }
    );

    modalAutomatisms.hide();

    // Rafraîchissement de la partie gauche, notamment pour actualiser l'affichage des astérisques de couleur sur chaque champ
    nsAdmin.loadContentModule(USROPT_MODULE_ADMIN_TAB, null, { tab: nsAdmin.tab })
}

nsAdmin.adminAutomatismsAnchors = function (elLink) {
    if (elLink != null && elLink.hasAttribute("edaDivId")) {
        var divId = elLink.getAttribute("edaDivId");
        var divToScroll = document.getElementById(divId);
        if (divToScroll != null) {
            divToScroll.scrollIntoView({ block: "start", behavior: "smooth" });
        }
    }

    var divContent = document.getElementById("automatismsAdminModalContent");
    if (divContent != null) {
        nsAdmin.adminAutomatismsScroll(divContent);
    }
}

nsAdmin.adminAutomatismsScroll = function (elDiv) {
    var scrollTop = elDiv.scrollTop;
    var scrollHeight = elDiv.scrollHeight;
    var clientHeight = elDiv.clientHeight;
    var scrollBottom = scrollHeight - (scrollTop + clientHeight);

    var anchorTop = document.getElementById("spanAnchorTop");
    var anchorMiddle = document.getElementById("spanAnchorMiddle");
    var anchorBottom = document.getElementById("spanAnchorBottom");

    anchorTop.className = anchorTop.className.replace("active", "").trim();
    anchorMiddle.className = anchorTop.className.replace("active", "").trim();
    anchorBottom.className = anchorTop.className.replace("active", "").trim();

    var scrollMax = scrollHeight - clientHeight;

    if (scrollTop < (scrollMax / 3)) {
        anchorTop.className += " active";
    }
    else if (scrollTop > (scrollMax * 2 / 3)) {
        anchorBottom.className += " active";
    }
    else {
        anchorMiddle.className += " active";
    }
}

// Crée une capsule avec la valeur de l'élément (l'élément doit avoir une valeur .value et comporter l'attribut "dsc")
nsAdmin.addPropertyToCapsule = function (element, capsule) {
    var desc = getAttributeValue(element, "dsc");
    if (desc != "") {
        var aDesc = desc.split("|");
        if (aDesc.length != 2)
            return;
        var value = (typeof element.value !== "undefined") ? element.value : getAttributeValue(element, "value");
        capsule.AddProperty(aDesc[0], aDesc[1], value, aDesc[2]);
    }
    else
        return;
}

// Crée une capsule avec la valeur d'une liste de radio boutons
nsAdmin.addRbPropertyToCapsule = function (name, capsule) {
    var value = "";
    var dsc = "";
    var radios = document.getElementsByName(name);
    for (var i = 0, length = radios.length; i < length; i++) {
        if (radios[i].checked) {
            value = radios[i].value;
            dsc = getAttributeValue(radios[i], "dsc");
            break;
        }
    }

    if (dsc != "") {
        var aDesc = dsc.split("|");
        if (aDesc.length != 2)
            return;
        capsule.AddProperty(aDesc[0], aDesc[1], value);
    }
    else
        return;
}
// Crée une capsule avec la valeur d'un checkbox
nsAdmin.addCbPropertyToCapsule = function (checkbox, capsule) {
    var desc = getAttributeValue(checkbox, "dsc");
    if (desc != "") {
        var aDesc = desc.split("|");
        if (aDesc.length != 2)
            return;
        capsule.AddProperty(aDesc[0], aDesc[1], getAttributeValue(checkbox, "chk"));
    }
    else
        return;
}

// Comportement spécifique câblé au changement d'une option d'authentification sur le module "Accès"
nsAdmin.accessSecurityAuthChange = function (radioButton) {
    // On commence par envoyer la MAJ de l'option cochée
    nsAdmin.sendJson(radioButton, false, true);

    // Puis on "annule" les autres
    var rbAuthenticationMethodEudonet = document.getElementById("rbAuthenticationMethodEudonet").querySelector("input");
    var rbAuthenticationMethodSSO = document.getElementById("rbAuthenticationMethodSSO").querySelector("input");
    var rbAuthenticationMethodLDAP = document.getElementById("rbAuthenticationMethodLDAP").querySelector("input");
    var rbAuthenticationMethodCAS = document.getElementById("rbAuthenticationMethodCAS").querySelector("input");
    var rbAuthenticationMethodADFS = document.getElementById("rbAuthenticationMethodADFS").querySelector("input");
    var rbAuthenticationMethodSAML2 = document.getElementById("rbAuthenticationMethodSAML2").querySelector("input");


    // TODO Se baser uniquement sur l'enum AuthenticationMode et centraliser le mode de connexion
    if (radioButton != rbAuthenticationMethodLDAP) {
        // Astuce sale pour mettre à jour la valeur en base : renommer l'ID avec la valeur 0, puis le rétablir.
        // TODO: trouver mieux...
        var rbAuthenticationMethodLDAPOriginalId = rbAuthenticationMethodLDAP.id;
        rbAuthenticationMethodLDAP.id = "rbAuthenticationMethod_0";
        nsAdmin.sendJson(rbAuthenticationMethodLDAP, false, true);
        rbAuthenticationMethodLDAP.id = rbAuthenticationMethodLDAPOriginalId;
    }
    if (radioButton != rbAuthenticationMethodCAS) {
        // Astuce sale pour mettre à jour la valeur en base : renommer l'ID avec la valeur 0, puis le rétablir.
        // TODO: trouver mieux...
        var rbAuthenticationMethodCASOriginalId = rbAuthenticationMethodCAS.id;
        rbAuthenticationMethodCAS.id = "rbAuthenticationMethod_0";
        nsAdmin.sendJson(rbAuthenticationMethodCAS, false, true);
        rbAuthenticationMethodCAS.id = rbAuthenticationMethodCASOriginalId;
    }

    // TODO Refactoring pour n'utiliser que l'enum AuthenticationMode
    var samlSettingsContainer = document.getElementById("samlcontainer");
    if (radioButton != rbAuthenticationMethodSAML2) {

        var rbAuthenticationMethodSAMLOriginalId = rbAuthenticationMethodSAML2.id;
        rbAuthenticationMethodSAML2.id = "rbAuthenticationMethod_0";
        nsAdmin.sendJson(rbAuthenticationMethodSAML2, false, true);
        rbAuthenticationMethodSAML2.id = rbAuthenticationMethodSAMLOriginalId;
        if (samlSettingsContainer)
            setAttributeValue(samlSettingsContainer, "data-active", "0");

        nsAdmin.Saml.enable(false);

    } else {
        if (samlSettingsContainer) {
            setAttributeValue(samlSettingsContainer, "data-active", "1");
            nsAdmin.Saml.enable(true);
        }
    }

    var adfsSettingsContainer = document.getElementById("AdfsContainer");
    if (radioButton == rbAuthenticationMethodADFS) {
        setAttributeValue(adfsSettingsContainer, "data-active", "1");

    }
    else if (radioButton != rbAuthenticationMethodADFS) {
        setAttributeValue(adfsSettingsContainer, "data-active", "0");

    }

    // Et on active ou désactive les options LDAP
    // Case à cocher "Création automatique"
    var chkAuthenticationMethodLDAPOptionsAutoCreate = document.getElementById("chkAuthenticationMethodLDAPAutoCreate");
    if (chkAuthenticationMethodLDAPOptionsAutoCreate != null && chkAuthenticationMethodLDAPOptionsAutoCreate != undefined)
        disChk(chkAuthenticationMethodLDAPOptionsAutoCreate, !rbAuthenticationMethodLDAP.checked);

    var chkAuthenticationMethodLDAPOptionsLDAPS = document.getElementById("chkAuthenticationMethodLDAPS");
    if (chkAuthenticationMethodLDAPOptionsLDAPS != null && chkAuthenticationMethodLDAPOptionsLDAPS != undefined)
        disChk(chkAuthenticationMethodLDAPOptionsLDAPS, !rbAuthenticationMethodLDAP.checked);


    // Sélecteur de version LDAP
    var ddlAuthenticationMethodLDAPOptionsVersion = document.getElementById("ddlAuthenticationMethodLDAPVersion");
    if (rbAuthenticationMethodLDAP.checked)
        ddlAuthenticationMethodLDAPOptionsVersion.removeAttribute("disabled");
    else
        setAttributeValue(ddlAuthenticationMethodLDAPOptionsVersion, "disabled", "disabled");
    // Autres options de type "Zone de texte"
    if (document.querySelector(".adminLDAPOptions") != null) {
        var grpAuthenticationMethodLDAPOptions = document.querySelector(".adminLDAPOptions").querySelectorAll("input");
        for (var i = 0; i < grpAuthenticationMethodLDAPOptions.length; i++) {
            if (rbAuthenticationMethodLDAP.checked)
                grpAuthenticationMethodLDAPOptions[i].removeAttribute("disabled");
            else
                setAttributeValue(grpAuthenticationMethodLDAPOptions[i], "disabled", "disabled");
        }
    }

}

// Comportement spécifique câblé au changement d'une option de gestion de groupes sur le module "Accès"
nsAdmin.accessSecurityGroupChange = function (control) {
    // On référence les options gérées par cette fonction
    var rbGroupPolicyNone = document.getElementById("rbGroupPolicyNone").querySelector("input");
    var rbGroupPolicyEnabled = document.getElementById("rbGroupPolicyEnabled").querySelector("input");
    var ddlGroupPolicyRestrictions = document.getElementById("ddlGroupPolicyRestrictions");
    var rbGroupPolicyGroupTreeDisplayUserOnly = document.getElementById("rbGroupPolicyGroupTreeDisplay").querySelectorAll("input")[0];
    var rbGroupPolicyGroupTreeDisplayUserGroups = document.getElementById("rbGroupPolicyGroupTreeDisplay").querySelectorAll("input")[1];

    // Si on coche le radio bouton "Permettre le regroupement hiérarchique des utilisateurs", on envoie en base la valeur sélectionnée dans la liste "Restrictions d'accès aux fiches"
    if (control == rbGroupPolicyEnabled)
        nsAdmin.sendJson(ddlGroupPolicyRestrictions, false, true);
    // Sinon, on envoie la MAJ de l'option cochée (directement câblée)
    else
        nsAdmin.sendJson(control, false, true);

    // Et on active ou désactive les options de regroupement
    if (rbGroupPolicyNone.checked)
        setAttributeValue(ddlGroupPolicyRestrictions, "disabled", "disabled");
    else
        ddlGroupPolicyRestrictions.removeAttribute("disabled");
    var grpGroupPolicyOptions = document.querySelector(".adminGroupPolicyOptions").querySelectorAll("input");
    for (var i = 0; i < grpGroupPolicyOptions.length; i++) {
        if (rbGroupPolicyNone.checked)
            setAttributeValue(grpGroupPolicyOptions[i], "disabled", "disabled");
        else
            grpGroupPolicyOptions[i].removeAttribute("disabled");
    }
}

// Désactive les options du nombre de tentatives avant envoi
//nsAdmin.maxAttemptChange = function (element) {
//    var eltMaxWarning = document.getElementById("ddlMaxAttemptWarning");

//    if (!element || !eltMaxWarning)
//        return;

//    var nMaxAttempt = getNumber(element.value);

//    for (var i = 0; i < eltMaxWarning.options.length; i++) {
//        eltMaxWarning.options[i].disabled = false;
//        if (getNumber(eltMaxWarning.options[i].value) >= nMaxAttempt)
//            eltMaxWarning.options[i].disabled = true;
//    }
//}

nsAdmin.deleteTable = function (e, elt, bConfirmed) {
    e.stopPropagation();

    if (!bConfirmed) {

        var label = "";

        var tr = findUp(elt, "TR");
        if (tr) {
            var labelCell = tr.querySelector(".adminTabsLabel .contentCellLAb");
            label = getAttributeValue(labelCell, "data-label");
        }

        var conf = eAdvConfirm({
            'criticity': eAdvConfirm.CRITICITY.MSG_EXCLAM,
            'title': top._res_8723.replace("{0}", label),
            'message': top._res_285,
            'details': top._res_68,
            'cssOk': 'button-red',
            'cssCancel': 'button-green',
            'resOk': top._res_19,
            'resCancel': top._res_29,
            'okFct': function () { nsAdmin.deleteTable(e, elt, true); },
            'cancelFct': function () { conf.hide(); }
        });

        return;
    }

    var tr = findUp(elt, "TR");
    if (!tr)
        return;

    var did = getNumber(getAttributeValue(tr, "did"));
    if (!(did > 0))
        return;

    var tabType = getNumber(getAttributeValue(tr, "tabtype"));
    var edntype = getNumber(getAttributeValue(tr, "edntype"));

    var upd = new eUpdater("eda/Mgr/eAdminNewTabDialogManager.ashx", 1);
    upd.addParam("action", 1, "post");
    upd.addParam("tab", did, "post");
    upd.addParam("tabtype", tabType, "post");
    upd.addParam("edntype", edntype, "post");

    upd.ErrorCallBack = function () { setWait(false); };
    upd.send(function (oRes) {
        nsAdmin.onDeletedTable(oRes);
    });
    top.setWait(true);
};

nsAdmin.onDeletedTable = function (oRes) {
    top.setWait(false);
    var result = JSON.parse(oRes);
    if (!result.Success) {
        if (result.Criticity == 0)
            top.eAlert(result.Criticity, top._res_72, result.UserErrorMessage);
        else
            top.eAlert(result.Criticity, "", result.UserErrorMessage);

        return;
    }

    //Param de filter/tri
    var paramSort = Object.assign({ currentSortCol: "", currentSortDir: "", currentSearch: "" }, nsAdminTabsList)

    nsAdmin.loadContentModule(USROPT_MODULE_ADMIN_TABS, null, paramSort);
};

nsAdmin.toggleSubMenuTitle = function (element) {
    var ulParent = element.parentElement;
    if (ulParent) {
        var arrow = element.querySelector("[class^='icon-caret']");
        var childLinks = ulParent.nextElementSibling;
        if (childLinks) {
            // Ouvert
            if (!childLinks.classList.contains("closed")) {
                addClass(childLinks, "closed");
                arrow.className = "icon-caret-right";
            }
            // Fermé
            else {
                removeClass(childLinks, "closed");
                arrow.className = "icon-caret-down";
            }
        }
    }
}

nsAdmin.confAdvancedCatalog = function (elLink, fieldDescid, fieldName) {
    if (fieldDescid != null && fieldDescid != 0) {
        modalAdvancedCatalog = new eModalDialog(top._res_7286 + " « " + fieldName + " ».", 0, "eda/eAdminAdvancedCatalogDialog.aspx", 985, 625, "modalAdvancedCatalog");

        modalAdvancedCatalog.addScript("eTools");

        modalAdvancedCatalog.noButtons = false;
        modalAdvancedCatalog.addParam("tab", nsAdmin.tab, "post");
        modalAdvancedCatalog.addParam("field", fieldDescid, "post");
        //modalAdvancedCatalog.addParam("iframeScrolling", "yes", "post");
        modalAdvancedCatalog.show();

        modalAdvancedCatalog.addButton(top._res_30, function () { modalAdvancedCatalog.hide(); }, 'button-gray', null);
        modalAdvancedCatalog.addButton(top._res_28, function () { nsAdmin.updateAdvancedCatalog(); }, 'button-green', null);

        nsAdmin.modalResizeAndMove(modalAdvancedCatalog);
    }
}

nsAdmin.updateAdvancedCatalog = function () {
    var modal = document.getElementById(modalAdvancedCatalog.iframeId);
    var modalContent = modal.contentDocument || modal.contentWindow.document;

    var obj = {};

    //radio button
    var properties = ["DataEnabled", "Multiple", "TreeView", "NoAutoLoad", "TreeViewOnlyLastChildren", "StepMode", "SequenceMode"];
    properties.forEach(function (value, index, array) {
        var property = value;

        var propertyValue = false;

        var radios = modalContent.getElementsByName("rbl" + property);

        for (var i = 0, length = radios.length; i < length; ++i) {
            if (radios[i].checked) {
                if (radios[i].value == "1")
                    propertyValue = true;

                if (radios[i].value == "0")
                    propertyValue = false;

                break;
            }
        }

        obj[property] = propertyValue;
    });

    //dropdownlist
    properties = ["DisplayMask", "SortBy", "PopupDescId", "BoundDescId"];
    properties.forEach(function (value, index, array) {
        var property = value;

        var dropDownList = modalContent.getElementById("ddl" + property);
        if (dropDownList != null)
            obj[property] = dropDownList.value;
    });

    //input text
    properties = ["SearchLimit", "DataAutoStart", "DataAutoFormula", "SelectedValueColor"];
    properties.forEach(function (value, index, array) {
        var property = value;

        var textbox = modalContent.getElementById("txt" + property);
        if (textbox != null)
            obj[property] = textbox.value;
    });

    //checkbox DataAutoEnabled
    properties = ["DataAutoEnabled"];
    properties.forEach(function (value, index, array) {
        var property = value;

        var checkbox = modalContent.getElementById("chx" + property);
        if (checkbox != null && checkbox.hasAttribute("chk"))
            obj[property] = checkbox.getAttribute("chk") == "1";
    });

    var fieldId = 0;
    var elFieldId = modalContent.getElementById("edaFieldId");
    if (elFieldId != null) {
        fieldId = elFieldId.value;
    }

    var upd = new eUpdater("eda/Mgr/eAdminAdvancedCatalogDialogManager.ashx", 1);
    upd.addParam("tab", nsAdmin.tab, "post");
    upd.addParam("field", fieldId, "post");
    upd.addParam("propertiesJson", JSON.stringify(obj), "post");

    upd.send(
        // Résultat
        function (oRes) {
            var res = JSON.parse(oRes);
            if (!res.Success) {
                eAlert(1, "Mise à jour options catalogue", res.Error);
            }
            else {
                nsAdmin.loadContentModule(USROPT_MODULE_ADMIN_TAB, null, { tab: nsAdmin.tab });
            }
        }
    );

    modalAdvancedCatalog.hide();

}

// Ouverture de la popup de paramétrage de la recherche avancée
nsAdmin.showAdvSearchPopup = function () {
    modalAdvSearch = new eModalDialog(top._res_7316, 0, "eda/eAdminAdvSearchDialog.aspx", 985, 520, "modalAdvSearch");

    modalAdvSearch.noButtons = false;
    modalAdvSearch.addParam("tab", nsAdmin.tab, "post");
    //modalAdvancedCatalog.addParam("iframeScrolling", "yes", "post");
    modalAdvSearch.show();

    modalAdvSearch.addButton(top._res_30, function () { modalAdvSearch.hide(); }, 'button-gray', null);
    modalAdvSearch.addButton(top._res_28, function () { nsAdmin.updateAdvSearch(); }, 'button-green', null);

    nsAdmin.modalResizeAndMove(modalAdvSearch);
}

//
nsAdmin.updateAdvSearch = function () {
    var modal = document.getElementById(modalAdvSearch.iframeId);
    var modalContent = modal.contentDocument || modal.contentWindow.document;

    if (modal.contentWindow.objFieldsSelect) {
        var objFieldsSelect = modal.contentWindow.objFieldsSelect;
        var list = objFieldsSelect.getSelectedFieldsList();

        var hidCols = modalContent.getElementById("hidCols");
        var dsc = getAttributeValue(hidCols, "dsc");
        if (dsc != "") {
            var aDsc = dsc.split("|");
            var updCaps = new Capsule(nsAdmin.tab);
            updCaps.AddProperty(aDsc[0], aDsc[1], list);
            var json = JSON.stringify(updCaps);

            var upd = new eUpdater("eda/Mgr/eAdminDescManager.ashx", 1);
            upd.json = json;
            upd.send(function (oRes) {
                var res = JSON.parse(oRes);

                if (!res.Success) {
                    top.eAlert(1, top._res_416, res.UserErrorMessage);
                }
                else {
                    modalAdvSearch.hide();
                }
            });
        }
    }
}

// Ouverture de la popup de paramétrage des relations additionnelles
nsAdmin.confBkmRelationsSQLFilters = function (bkmTab) {
    modalBkmRelationsSQLFilters = new eModalDialog(top._res_7400, 0, "eda/eAdminBkmRelationsSQLFiltersDialog.aspx", 800, 600, "modalBkmRelationsSQLFilters");

    modalBkmRelationsSQLFilters.noButtons = false;
    modalBkmRelationsSQLFilters.addParam("tab", nsAdmin.tab, "post");
    modalBkmRelationsSQLFilters.addParam("bkmTab", bkmTab, "post");
    //modalAdvancedCatalog.addParam("iframeScrolling", "yes", "post");
    modalBkmRelationsSQLFilters.show();

    modalBkmRelationsSQLFilters.addButton(top._res_30, function () { modalBkmRelationsSQLFilters.hide(); }, 'button-gray', null);
    modalBkmRelationsSQLFilters.addButton(top._res_28, function () { nsAdmin.updateBkmRelationsSQLFilters(bkmTab); }, 'button-green', null);

    nsAdmin.modalResizeAndMove(modalBkmRelationsSQLFilters);
}

nsAdmin.updateBkmRelationsSQLFilters = function (bkmTab) {
    var errors = [];

    var modal = document.getElementById(modalBkmRelationsSQLFilters.iframeId);
    var modalContent = modal.contentDocument || modal.contentWindow.document;

    var updateCategory = modalContent.getElementById("updateCategory"); // 8 = eAdminUpdateProperty.CATEGORY.BKMPREF
    var updateFieldTab = modalContent.getElementById("updateFieldTab"); // 0 = eConst.PREF_BKMPREF.TAB
    var updateFieldBkm = modalContent.getElementById("updateFieldBkm"); // 1 = eConst.PREF_BKMPREF.BKM
    var updateFieldAddedBkmWhere = modalContent.getElementById("updateFieldAddedBkmWhere"); // 10 = eConst.PREF_BKMPREF.ADDEDBKMWHERE

    var valueBkm = modalContent.getElementById("valueBkm");

    sections = ["PP", "PM", "EVT"];
    sections.forEach(function (value, index, array) {
        var section = value;

        var updCaps = new Capsule(bkmTab);

        var valueTab = modalContent.getElementById("valueTab" + section);
        var valueAddedBkmWhere = modalContent.getElementById("valueAddedBkmWhere" + section);

        if (valueTab != null && valueAddedBkmWhere != null) {
            updCaps.AddProperty(updateCategory.value, updateFieldTab.value, valueTab.value); // 8 = BKMPREF, 0 = eConst.PREF_BKMPREF.TAB
            updCaps.AddProperty(updateCategory.value, updateFieldBkm.value, valueBkm.value); // 8 = BKMPREF, 1 = eConst.PREF_BKMPREF.BKM
            updCaps.AddProperty(updateCategory.value, updateFieldAddedBkmWhere.value, valueAddedBkmWhere.value); // 8 = BKMPREF, 10 = eConst.PREF_BKMPREF.ADDEDBKMWHERE
        }

        if (updCaps.ListProperties.length > 0) {
            var json = JSON.stringify(updCaps);

            var upd = new eUpdater("eda/Mgr/eAdminDescManager.ashx", 1);
            upd.json = json;
            upd.send(function (oRes) {
                var res = JSON.parse(oRes);

                if (!res.Success) {
                    errors.push(res.UserErrorMessage);
                }
            });
        }
    });

    if (errors.length > 0) {
        var strErrors = "";
        errors.forEach(function (value, index, array) {
            if (strErrors != "")
                strErrors += "\n";
            strErrors += value;
        });
        top.eAlert(1, top._res_416, strErrors);
    }
    else {
        modalBkmRelationsSQLFilters.hide();
    }
}

// Ouverture de la popup de paramétrage des mappings d'un onglet cible étendue
var modalExtendedTargetMappings
nsAdmin.confExtendedTargetMappings = function (bkmTab) {
    modalExtendedTargetMappings = new eModalDialog(top._res_7623, 0, "eda/eAdminExtendedTargetMappingsDialog.aspx", 985, 650, "modalExtendedTargetMappings");

    modalExtendedTargetMappings.noButtons = false;
    modalExtendedTargetMappings.addParam("tab", nsAdmin.tab, "post");
    //modalExtendedTargetMappings.addParam("iframeScrolling", "yes", "post");
    modalExtendedTargetMappings.show();

    modalExtendedTargetMappings.addButton(top._res_30, function () { modalExtendedTargetMappings.hide(); }, 'button-gray', null);
    //modalExtendedTargetMappings.addButton(top._res_28, function () { nsAdmin.updateExtendedTargetMappings(); }, 'button-green', null);

    nsAdmin.modalResizeAndMove(modalExtendedTargetMappings);
}

nsAdmin.updateExtendedTargetMappings = function (did) {

    top.setWait(true);
    var obj = {};

    //selects
    var fields = document.getElementsByTagName("select");
    var arrayRub = [];
    for (var i = 0; i < fields.length; ++i) {
        var field = fields[i];

        if (field != null && !field.disabled
            && field.hasAttribute("did") && !isNaN(parseInt(field.getAttribute("did")))
            && parseInt(field.getAttribute("did")) != 0
            && !isNaN(parseInt(field.value))
            && parseInt(field.value) != 0
            && parseInt(field.value) != 1
        ) {
            var rubrique = { descid: parseInt(field.getAttribute("did")), value: parseInt(field.value) };
            arrayRub.push(rubrique);
        }
    }

    obj.mappings = arrayRub;

    var upd = new eUpdater("eda/Mgr/eAdminExtendedTargetMappingsDialogManager.ashx", 1);
    var tab = top.nsAdmin.tab;
    upd.addParam("tab", tab, "post");
    upd.addParam("mappingsJson", JSON.stringify(obj), "post");
    upd.addParam("descidfield", did, "post");

    upd.send(
        // Résultat
        function (oRes) {
            var res = JSON.parse(oRes);
            if (!res.Success) {
                eAlert(1, top._res_6343, res.UserErrorMessage);
            }
            else {

                // #51763 on rafraichit le content de la fiche car le mapping a changé
                top.nsAdmin.loadContentModule(USROPT_MODULE_ADMIN_TAB, null, { tab: tab });
            }

            top.setWait(false);
        }

    );

    //modalExtendedTargetMappings.hide();
}

nsAdmin.adminExtendedTargetMappingsToggleField = function (elToggleField) {

    var newValue = elToggleField.value;

    var oldValue = 0;
    if (elToggleField.hasAttribute("edaExtmpOldvalue")) {
        oldValue = elToggleField.getAttribute("edaExtmpOldvalue");
    }

    nsAdmin.updateExtendedTargetMappings(getAttributeValue(elToggleField, 'did'));

    var fields = document.getElementsByTagName("select");

    for (i = 0; i < fields.length; ++i) {
        var elField = fields[i];

        if (elField != null && elField != elToggleField) {
            var ops = elField.getElementsByTagName("option");
            for (var j = 0; j < ops.length; ++j) {
                var op = ops[j];
                if (newValue != "1" && op.value == newValue)
                    op.disabled = true;

                if (oldValue != "1" && op.value == oldValue)
                    op.disabled = false;
            }
        }
    }

    elToggleField.setAttribute("edaExtmpOldvalue", newValue);


}


nsAdmin.adminExtendedTargetCheckUsedField = function (elToggleField) {
    top.setWait(true);

    var did = getAttributeValue(elToggleField, 'did');
    var valDid = elToggleField.value;
    var upd = new eUpdater("eda/Mgr/eAdminExtendedTargetMappingsDialogManager.ashx", 1);
    upd.addParam("tab", nsAdmin.tab, "post");
    upd.addParam("descidfield", did, "post");
    upd.addParam('action', 'updatefield', 'post');

    upd.send(
        // Résultat
        function (oRes) {
            var res = JSON.parse(oRes);
            if (!res.Success) {


                top.eAlert(res.Criticity, top._res_92, res.UserErrorMessage);
                nsAdmin.CancelAdminExtendedTargetMappingsToggleField(elToggleField);
            }
            else {

                var msg = top._res_8668
                if (valDid == "1") {
                    msg = top._res_2653
                }

                top.eConfirm(eMsgBoxCriticity.MSG_INFOS, top._res_6343, msg, '', 600, 200,
                    //Valider
                    function () {
                        nsAdmin.adminExtendedTargetMappingsToggleField(elToggleField);
                    },
                    //Cancel
                    function () {
                        nsAdmin.CancelAdminExtendedTargetMappingsToggleField(elToggleField);
                    },
                    false, true);
            }
            top.setWait(false);
        }
    );


}


nsAdmin.CancelAdminExtendedTargetMappingsToggleField = function (elToggleField) {

    var oldValue = 0;
    if (elToggleField.hasAttribute("edaExtmpOldvalue")) {
        oldValue = elToggleField.getAttribute("edaExtmpOldvalue");
        if (oldValue != 0 && oldValue != '') {
            var ops = elToggleField.getElementsByTagName("option");

            for (var j = 0; j < ops.length; ++j) {
                var op = ops[j];

                if (op.value == oldValue)
                    op.selected = true;
                else
                    op.selected = false;
            }
        }
    }
}

nsAdmin.confirmAdminExtendedTargetMappingsToggleField = function (elToggleField) {

    var oldValue = 0;
    var newValue = elToggleField.value;
    var origineFieldLenght;
    var origineFieldName;
    var sourceFieldLenght = getAttributeValue(elToggleField, 'edafieldlenght');
    var bConfirmed = getAttributeValue(elToggleField, 'bConfirmed');
    var sourceFieldName = getAttributeValue(elToggleField, 'edaFieldName');
    if (elToggleField.hasAttribute("edaExtmpOldvalue")) {
        oldValue = elToggleField.getAttribute("edaExtmpOldvalue");
        if (oldValue != 0 && oldValue != '') {
            origineFieldLenght = getAttributeValue(elToggleField.options[elToggleField.selectedIndex], 'edafieldlenght');
            origineFieldName = elToggleField.options[elToggleField.selectedIndex].text;

            if (!isNaN(parseInt(origineFieldLenght)) && !isNaN(parseInt(sourceFieldLenght)) && parseInt(origineFieldLenght) > parseInt(sourceFieldLenght)) {

                top.eAlert(0, top._res_6343, top._res_8501.replace('<SOURCEFIELD>', sourceFieldName)
                    .replace('<SOURCEFIELDLENGHT>', sourceFieldLenght)
                    .replace('<ORIGINEFIELD>', origineFieldName)
                    .replace('<ORIGINEFIELDLENGHT>', origineFieldLenght), null, null, null);
                nsAdmin.CancelAdminExtendedTargetMappingsToggleField(elToggleField);
            } else {
                if (!(oldValue != 1 && bConfirmed != '0'))
                    setAttributeValue(elToggleField, 'bConfirmed', '0');

                //Check que la rubrique n'est pas utilisée
                nsAdmin.adminExtendedTargetCheckUsedField(elToggleField);
            }
        }
    }
}


/* Redimensionnement et déplacement de la modale par rapport à l'emplacement de la navbar pour que la modale soit calée en-dessous */
nsAdmin.modalResizeAndMove = function (objModal) {
    var marginTop = 5;
    var marginBottom = 5;

    var adminNavBottom = 0;
    var oAdminNav = document.getElementById("adminNav");
    if (oAdminNav != null) {
        adminNavBottom = oAdminNav.getBoundingClientRect().bottom;
    }

    var maxHeight = objModal.docHeight - adminNavBottom - marginTop - marginBottom;
    if (objModal.height > maxHeight)
        objModal.resizeTo(objModal.width, maxHeight);

    if (objModal.getDivContainer().getBoundingClientRect().top < adminNavBottom)
        objModal.moveTo(objModal.getDivContainer().style.left, adminNavBottom + marginTop);
}

//nsAdmin.resizeElementToWindow = function (modal, idElement) {
//    if (modal) {
//        var doc = modal.getIframe().document;
//        var element = doc.getElementById(idElement);

//        var elementH = element.offsetHeight;
//        var body = findUp(element, "BODY");BE

//        var occupiedHeight = body.offsetHeight - elementH;

//        element.style.height = (modal.getIframeTag().offsetHeight -occupiedHeight) + "px";
//    }

//}

//nsAdmin.resizeElementToModalBelonging = function () {
//    nsAdmin.resizeElementToWindow(modalBelonging, "tableBelongings");

//}

// Modale liste des rubriques
var modalFieldsList;
nsAdmin.confFieldsList = function () {
    modalFieldsList = new eModalDialog(top._res_7694, 0, "eda/eAdminFieldsListDialog.aspx", 985, 555, "modalFieldsList");

    modalFieldsList.noButtons = false;
    modalFieldsList.addParam("tab", nsAdmin.tab, "post");
    modalFieldsList.show();

    modalFieldsList.addButton(top._res_30, function () { modalFieldsList.hide(); nsAdmin.loadContent(nsAdmin.tab); }, 'button-green', null);
    modalFieldsList.addButton(top._res_16, function () { nsAdmin.exportFieldsList() }, 'button-gray', null);
    modalFieldsList.addButton(top._res_6190, function () { modalFieldsList.getIframe().print(); }, 'button-gray', null);

    //nsAdmin.modalResizeAndMove(modalFieldsList);
    modalFieldsList.MaxOrMinModal();
};


nsAdmin.exportFieldsList = function () {

    var baseurl = "eda/mgr/eAdminFieldsExportList.ashx";
    var nTab = modalFieldsList.getIframe().document.getElementById('ddlTabsList');


    var form = CreateElement('form', ["method", "action", "style", "target"], ["post", baseurl, "display:hidden;", "_blank"]);
    var format = CreateElement("input", ["type", "name", "value"], ["hidden", "nTab", nTab.value]);
    form.appendChild(format);
    modalFieldsList.getIframe().document.body.appendChild(form);
    form.submit();
    modalFieldsList.getIframe().document.body.removeChild(form);

}

nsAdmin.applyCurrency = function () {

    var oCurrencyUpdater = new eUpdater("eda/mgr/eAdminOverwriteCurrency.ashx", 0);

    oCurrencyUpdater.ErrorCallBack = function () { setWait(false); };

    oCurrencyUpdater.addParam("action", "request", "post");

    oCurrencyUpdater.send(nsAdmin.onAppliedCurrency);
};


nsAdmin.onAppliedCurrency = function (oRes) {

    var success = getXmlTextNode(oRes.getElementsByTagName("success")[0]) == "1";
    if (success) {
        eAlert(getXmlTextNode(oRes.getElementsByTagName("criticity")[0])
            , getXmlTextNode(oRes.getElementsByTagName("title")[0])
            , getXmlTextNode(oRes.getElementsByTagName("msg")[0])
            , getXmlTextNode(oRes.getElementsByTagName("details")[0])
        );
    }
    else {
        oErrorObj = errorXML2Obj(createXmlDoc(oRes));
        eAlertError(oErrorObj);
    }
};


var modalTranslations;
nsAdmin.openTranslations = function (descid, nature, resid, fileid) {
    modalTranslations = new eModalDialog(top._res_7716, 0, "eda/mgr/eAdminTranslationsMgr.ashx", 985, 650, "modalTranslations");
    modalTranslations.addParam("action", 0, "post");
    if (descid) {
        modalTranslations.addParam("descid", descid, "post");
    }
    if (nature) {
        modalTranslations.addParam("nature", nature, "post");
    }
    if (resid) {
        modalTranslations.addParam("resid", resid, "post");
    }
    if (fileid) {
        modalTranslations.addParam("fileid", fileid, "post");
    }
    if (!descid && !nature) {
        //si aucun filtre on n'affiche que les traductions de l'anglais (pour des raisons de performances)
        modalTranslations.addParam("lang", 1, "post");
    }

    modalTranslations.show();

    modalTranslations.addButton(top._res_30, function () { modalTranslations.hide(); }, 'button-green', null);

    nsAdmin.modalResizeAndMove(modalTranslations);
    modalTranslations.resizeToMaxWidth();
};

nsAdmin.extensionsPerPage = 10;
nsAdmin.currentExtensionPage = 1;
nsAdmin.currentExtensionSearch = '';
nsAdmin.currentExtensionFilterCategory = '';
nsAdmin.currentExtensionFilterNotation = '';

// Activation ou désactivation d'une extension
nsAdmin.enableExtension = function (module, enable, listMode, additionalParams) {
    //SHA : #74 325
    if (typeof (module) !== 'undefined') {
        if (module.indexOf("ADMIN_EXTENSION") == 0) {
            nsAdmin.loadAdminCss();
            nsAdmin.loadAdminJs();
            nsAdmin.loadModuleCssScripts(module);
        }
    }

    var oExtensionUpdater = new eUpdater("eda/mgr/eAdminExtensionManager.ashx", 1);

    oExtensionUpdater.ErrorCallBack = function () { setWait(false); };

    oExtensionUpdater.addParam("action", enable ? "enable" : "disable", "post");
    oExtensionUpdater.addParam("module", getUserOptionsModuleHashCode(module), "post");
    oExtensionUpdater.addParam("listmode", listMode ? "1" : "0", "post");

    if (typeof (additionalParams) == "object") {
        if (Array.isArray(additionalParams)) {
            for (var i = 0; i < additionalParams.length; i++) {
                oExtensionUpdater.addParam(additionalParams[i].key, additionalParams[i].value, "post");
            }
        }
        else {

            for (var prop in additionalParams) {
                if (additionalParams.hasOwnProperty(prop)) {

                    oExtensionUpdater.addParam(prop, additionalParams[prop], "post");
                }
            }
        }
    }

    //SHA : tâche #1 873
    if (document.getElementById("extensionCode").value == "ADVANCEDFORM" && enable == false) {
        var sUninstAdvancFormMsg = top._res_2448; // Attention, les formulaires avancés en cours d'utilisation ne seront plus accessibles. Confirmez-vous cette opération ?

        eAdvConfirm({
            'criticity': 1,
            'title': top._res_6536,
            'message': sUninstAdvancFormMsg,
            'width': 550,
            'height': 200,
            'okFct': function () {
                top.setWait(true);
                oExtensionUpdater.send(nsAdmin.updateExtensionPanel);
            },
            'cancelFct': null,
            'bOkGreen': false,
            'bHtml': false,
            'resOk': top._res_2278,
            'resCancel': top._res_30,
            'cssOk': 'button-red'
        });
        //problème de css du bouton button-red
        document.getElementsByClassName("button-red")[0].style.height = "auto";
    }
    else {
        top.setWait(true);
        oExtensionUpdater.send(nsAdmin.updateExtensionPanel);
    }
}

nsAdmin.updateExtensionPanel = function (oRes, callback) {
    top.setWait(false);

    try {
        var oJsonRes = JSON.parse(oRes);
        if (oJsonRes.Success) {

            // Cas 1 : Mise à jour de la liste des extensions
            var bUpdateExtensionFile = false;
            oParentContainer = document.getElementById("blockListExtensions");
            // Cas 2 : Mise à jour de la fiche d'une extension
            if (!oParentContainer) {
                oParentContainer = document.getElementById("mainDiv").firstChild;
                bUpdateExtensionFile = true;
            }

            // Soit on rafraîchit toute la page si l'extension l'exige explicitement, soit on rafraîchit uniquement l'encart d'infos principales
            // si le contexte le permet
            var bFullRefresh = !oParentContainer || (bUpdateExtensionFile && oJsonRes.NeedsFullRefresh);

            // Mise à jour de l'encart d'infos principales
            if (!bFullRefresh) {
                oTargetModulePanel = document.getElementById("extensionBlock_" + oJsonRes.Module + "_" + oJsonRes.ExtensionFileId);
                if (oTargetModulePanel) {
                    var oNewPanel = document.createElement("div");
                    oNewPanel.innerHTML = oJsonRes.Html;
                    oParentContainer.replaceChild(oNewPanel.firstChild, oTargetModulePanel);
                }
                // Puis mise à jour de la page selon contexte
                if (bUpdateExtensionFile) {
                    // Si on active une extension : on affiche son onglet Paramètres en mode Fiche et on le sélectionne, si l'extension dispose de paramètres
                    if (oJsonRes.Result == "1" && oJsonRes.ShowParametersTab) {
                        nsAdmin.setExtensionTabVisibility('settings', true);

                        //nsAdmin.displayExtensionTab('settings'); // SPH : pour l'instant, pas de param, on reste sur description
                        nsAdmin.displayExtensionTab('description');
                    }
                    // Si on désactive une extension : on masque cet onglet Paramètres et on bascule sur Description
                    else {
                        nsAdmin.setExtensionTabVisibility('settings', false);
                        nsAdmin.displayExtensionTab('description');
                    }
                }
                else {
                    // Mise à jour du compteur d'extensions activées en full JavaScript pour éviter un A/R serveur
                    // TODO/TOCHECK: A supprimer si on redirige vers le mode Fiche au clic sur le bouton
                    var spanNbElem = document.getElementById("SpanNbElem");
                    if (spanNbElem) {
                        var currentCount = Number(spanNbElem.innerHTML);
                        if (oJsonRes.Result == "1")
                            currentCount++;
                        else
                            currentCount--;
                        spanNbElem.innerHTML = currentCount;
                    }
                }
            }
            else {
                if (bUpdateExtensionFile) {
                    nsAdmin.loadAdminModule(oJsonRes.Module, null, { extensionFileId: oJsonRes.ExtensionFileId, extensionCode: oJsonRes.ExtensionCode, extensionLabel: oJsonRes.ExtensionLabel });
                }
                else
                    nsAdmin.loadAdminModule(USROPT_MODULE_ADMIN_EXTENSIONS);
            }
        } else {
            eAlert(0, "", oJsonRes.ErrorMsg);
        }
    }
    catch (e) {
        //
        eAlert(0, "", e.message);
    }
}

nsAdmin.setExtensionTabVisibility = function (tabId, visible) {
    // On masque ou affiche l'onglet concerné et son séparateur associé
    var tab = document.getElementById('extensionBkm_' + tabId);
    if (tab)
        tab.style.display = visible ? 'table-cell' : 'none';
    var sep = document.getElementById('extensionBkmSep_' + tabId);
    if (sep && !visible)
        sep.style.display = 'none';
    // On masque le dernier élément visible du tableau s'il s'agit d'un séparateur, et on affiche tous les autres
    try {
        var bkmTrAllSep = document.getElementById("bkmtr").querySelectorAll(".bkmSep");
        for (var i = 0; i < bkmTrAllSep.length; i++)
            bkmTrAllSep[i].removeAttribute("style");
        var bkmTrVisSep = document.getElementById("bkmtr").querySelectorAll("td.bkmSep:not([style*='display:none']):not([style*='display: none'])");
        var bkmTrVisAll = document.getElementById("bkmtr").querySelectorAll("td:not([style*='display:none']):not([style*='display: none'])");
        if (bkmTrVisAll && bkmTrVisAll.length > 0 && bkmTrVisSep && bkmTrVisSep.length > 0)
            if (bkmTrVisAll[bkmTrVisAll.length - 1] == bkmTrVisSep[bkmTrVisSep.length - 1])
                bkmTrVisSep[bkmTrVisSep.length - 1].style.display = 'none';
    }
    catch (ex) { }
}
nsAdmin.displayExtensionTab = function (tabId) {
    var containers = document.querySelectorAll('[id^=extensionCnt_');
    for (var i = 0; i < containers.length; i++) {
        if (containers[i].id == 'extensionCnt_' + tabId) {
            switchClass(document.getElementById('extensionBkm_' + tabId), 'bkmHead', 'bkmHeadSel');
            containers[i].style.display = 'block';
            // On redimensionne les contenus des signets
            //if (tabId == "screenshots") {
            var main = document.getElementById("mainDiv");
            var blockExtensionH = main.querySelector(".blockExtensionFile").offsetHeight;
            var bkmH = document.getElementById("extensionTabs").offsetHeight;

            if (containers[i].children.length > 0) {
                containers[i].children[0].style.width = (main.offsetWidth - 20) + "px";
                containers[i].children[0].style.height = (main.offsetHeight - (blockExtensionH + bkmH) - 20) + "px";
            }

        }
        else {
            switchClass(document.getElementById('extensionBkm_' + containers[i].id.replace('extensionCnt_', '')), 'bkmHeadSel', 'bkmHead');
            containers[i].style.display = 'none';
        }
    }
}
nsAdmin.extPageFirst = function (nTab) {
    if (!nTab)
        nTab = 0;

    var oTable = document.getElementById("mt_" + nTab);
    var cPage = nsAdmin.currentExtensionPage; //Number(oTable.getAttribute("cPage"));
    var bof = Number(oTable.getAttribute("bof"));

    if (bof == "1")
        return;

    nsAdmin.loadAdminModule(USROPT_MODULE_ADMIN_EXTENSIONS, null, { page: 1, rows: nsAdmin.extensionsPerPage, search: nsAdmin.currentExtensionSearch, filterCategory: nsAdmin.currentExtensionFilterCategory, filterNotation: nsAdmin.currentExtensionFilterNotation });
}
nsAdmin.extPagePrev = function (nTab) {
    if (!nTab)
        nTab = 0;

    var oTable = document.getElementById("mt_" + nTab);
    var cPage = nsAdmin.currentExtensionPage; //Number(oTable.getAttribute("cPage"));
    var eof = Number(oTable.getAttribute("eof"));

    if (cPage > 1)
        cPage--;
    else
        return;

    nsAdmin.loadAdminModule(USROPT_MODULE_ADMIN_EXTENSIONS, null, { page: cPage, rows: nsAdmin.extensionsPerPage, search: nsAdmin.currentExtensionSearch, filterCategory: nsAdmin.currentExtensionFilterCategory, filterNotation: nsAdmin.currentExtensionFilterNotation });
}
nsAdmin.extPageNext = function (nTab) {
    if (!nTab)
        nTab = 0;

    var oTable = document.getElementById("mt_" + nTab);
    var cPage = nsAdmin.currentExtensionPage; //Number(oTable.getAttribute("cPage"));
    var eof = Number(oTable.getAttribute("eof"));

    if (eof != "1")
        cPage++;
    else
        return;

    nsAdmin.loadAdminModule(USROPT_MODULE_ADMIN_EXTENSIONS, null, { page: cPage, rows: nsAdmin.extensionsPerPage, search: nsAdmin.currentExtensionSearch, filterCategory: nsAdmin.currentExtensionFilterCategory, filterNotation: nsAdmin.currentExtensionFilterNotation });
}
nsAdmin.extPageLast = function (nTab) {
    if (!nTab)
        nTab = 0;

    var oTable = document.getElementById("mt_" + nTab);
    var cPage = Number(oTable.getAttribute("nbpage"));
    var eof = Number(oTable.getAttribute("eof"));

    if (eof == "1")
        return;

    nsAdmin.loadAdminModule(USROPT_MODULE_ADMIN_EXTENSIONS, null, { page: cPage, rows: nsAdmin.extensionsPerPage, search: nsAdmin.currentExtensionSearch, filterCategory: nsAdmin.currentExtensionFilterCategory, filterNotation: nsAdmin.currentExtensionFilterNotation });
}
nsAdmin.extPageSelect = function (nTab, oNewPage) {
    if (!nTab)
        nTab = 0;

    var newPage = 1;

    var oTable = document.getElementById("mt_" + nTab);
    var cPage = nsAdmin.currentExtensionPage; //Number(oTable.getAttribute("cPage"));
    var nbPage = Number(oTable.getAttribute("nbPage"));

    if (oNewPage && !isNaN(oNewPage.value))
        newPage = oNewPage.value;

    //changement de page seulement si la page demandée n'est pas la page actuellement affichée, page demandée > 1 et page demandée < au nombre de pages de la liste en cours 
    if ((newPage == cPage) || (newPage < 1) || (newPage > nbPage) || !isNumeric(newPage)) {
        oNewPage.value = cPage;
        return;
    }

    nsAdmin.loadAdminModule(USROPT_MODULE_ADMIN_EXTENSIONS, null, { page: newPage, rows: nsAdmin.extensionsPerPage, search: nsAdmin.currentExtensionSearch, filterCategory: nsAdmin.currentExtensionFilterCategory, filterNotation: nsAdmin.currentExtensionFilterNotation });
}

var searchTimerExt;
var previousExtSearch; //Dernière recherche d'extension
nsAdmin.extSearch = function (sSearch, e) {
    var bEnter = (e != null && e.keyCode && e.keyCode == 13);

    clearTimeout(searchTimerExt);
    if (typeof (sSearch) === "string") {

        if (sSearch.length > 2 || (bEnter && sSearch.length > 0)) { // CNA - Demande #52864 - La recherche loupe ne doit se déclencher que sur entrer
            var fct = function () {

                var eFSInput = document.getElementById("eFSInput");
                var eFSStatus = document.getElementById("eFSStatus");

                //Affiche le wait champ filtre
                eFSStatus.className = "eFSWait";

                // sort du focus
                eFSInput.blur();

                // Recherche
                nsAdmin.loadAdminModule(USROPT_MODULE_ADMIN_EXTENSIONS, null, { page: nsAdmin.currentExtensionPage, rows: nsAdmin.extensionsPerPage, search: sSearch, filterCategory: nsAdmin.currentExtensionFilterCategory, filterNotation: nsAdmin.currentExtensionFilterNotation });
            };

            // Temporisation de la recherche
            searchTimerExt = window.setTimeout(function () { fct() }, 500);
        }
        else if ((sSearch.length == 0) && (sSearch != previousExtSearch)) {
            nsAdmin.resetExtSearch(true);
        }
    }
    previousSearch = sSearch;
}
nsAdmin.resetExtSearch = function (bReload) {
    // retire l'event on click et le bouton
    var eFSStatus = document.getElementById("eFSStatus");
    if (eFSStatus != null) {
        eFSStatus.onclick = "";
        eFSStatus.className = "";
    }

    // Remise à zéro du style
    var eFSContainer = document.getElementById("eFS");
    switchClass(eFSContainer, "eFSContainerSearchActive", "eFSContainerSearch");

    //vide le champ 
    var eFSInput = document.getElementById("eFSInput");
    eFSInput.value = "";

    // Rechargement de la page si demandé
    if (bReload)
        nsAdmin.loadAdminModule(USROPT_MODULE_ADMIN_EXTENSIONS, null, { page: nsAdmin.currentExtensionPage, rows: nsAdmin.extensionsPerPage, search: '', filterCategory: nsAdmin.currentExtensionFilterCategory, filterNotation: nsAdmin.currentExtensionFilterNotation });
}

nsAdmin.resetExtFilters = function () {
    nsAdmin.resetExtSearch(false);

    nsAdmin.loadAdminModule(USROPT_MODULE_ADMIN_EXTENSIONS, null, { page: nsAdmin.currentExtensionPage, rows: nsAdmin.extensionsPerPage, search: '', filterCategory: '', filterNotation: '' });
}

nsAdmin.setExtFilter = function (oNode) {
    var sFilterDescId = oNode.id.replace("QuickF_", "");
    var nIndex = oNode.getAttribute("ednIdx");

    if (oNode.selectedIndex < 0)
        return;

    var sValue = oNode.options[oNode.selectedIndex].value;

    //selection de Tous
    if (sValue == '-1')
        removeClass(oNode, 'activeQF');
    else
        addClass(oNode, 'activeQF');

    var currentFilterCategory = sFilterDescId == 'Category' ? sValue : nsAdmin.currentExtensionFilterCategory;
    var currentFilterNotation = sFilterDescId == 'Notation' ? sValue : nsAdmin.currentExtensionFilterNotation;

    // Application du filtre
    nsAdmin.loadAdminModule(USROPT_MODULE_ADMIN_EXTENSIONS, null, { page: nsAdmin.currentExtensionPage, rows: nsAdmin.extensionsPerPage, search: nsAdmin.currentExtensionSearch, filterCategory: currentFilterCategory, filterNotation: currentFilterNotation });
}

nsAdmin.initExtensionFile = function () {
    nsAdmin.initHtmlContent("Description");
    nsAdmin.initHtmlContent("Installation");
    nsAdmin.initHtmlContent("Version");
}

//SHA
nsAdmin.openPage = function (pageName, elmnt) {
    var moduleInstalled = document.querySelector('.store-content-container').classList.contains('installed');
    if (moduleInstalled) {
        //SHA : correction bug 71 446
        var optionTableEmail = document.getElementById("mobileFieldListDdl_outlookAddinMain");
        if (optionTableEmail != null && typeof (optionTableEmail) != "undefined") {
            if (optionTableEmail.value != null && typeof (optionTableEmail.value) != "undefined" && optionTableEmail.value != "") {
                var optionTableNom = document.getElementById("mobileFieldListDdl_main_NAME");
                var optionTablePrenom = document.getElementById("mobileFieldListDdl_main_FIRSTNAME");
                var optionTableAdresses = document.getElementById("mobileFieldListDdl_main_EMAIL");
                var optionTableContacts = document.getElementById("mobileFieldListDdl_main_FREE_3");
                if (optionTableEmail != null && optionTableNom != null && optionTablePrenom != null && optionTableAdresses != null && optionTableContacts != null) {
                    if (optionTableEmail.value == "-1" || optionTableNom.value == "-1" || optionTablePrenom.value == "-1" || optionTableAdresses.value == "-1" || optionTableContacts.value == "-1") {
                        eAlert(2, top._res_2328, top._res_2329);
                        //return;
                    }
                }
            }
        }

        var j, tabcontent, tablinks;
        tabcontent = document.getElementsByClassName("store-tabcontent");
        for (j = 0; j < tabcontent.length; j++) {
            tabcontent[j].classList.remove("store-visible");
        }
        tablinks = document.getElementsByClassName("store-tablink");
        for (j = 0; j < tablinks.length; j++) {
            tablinks[j].classList.remove('active');
        }
        document.getElementById(pageName).classList.add("store-visible");
        elmnt.classList.add('active');
    }

}

nsAdmin.initHtmlContent = function (sID) {
    // Chargement du contenu des l'onglet conteneur HTML
    try {
        var descriptionContainer = document.getElementById("extension" + sID + "ContentsContainer");
        var descriptionContents = document.getElementById("extension" + sID + "Contents");
        if (descriptionContainer && descriptionContents) {
            var innerDocument = null;
            var body = null;
            if (descriptionContainer.contentDocument) { // FF
                innerDocument = descriptionContainer.contentDocument;
            }
            else if (descriptionContainer.contentWindow) { // IE
                innerDocument = descriptionContainer.contentWindow;
            }
            if (innerDocument) {
                // Ajout des CSS de l'application à l'iframe pour que son style (dont la police par défaut) soit le même que celle de l'application
                if (typeof (document.styleSheets) != 'undefined') {
                    for (var i = 0; i < document.styleSheets.length; i++) {
                        if (document.styleSheets[i].href != null && document.styleSheets[i].href.indexOf("eAdminExtensions.css") > -1) {
                            var newStyleSheet = innerDocument.createElement("link");
                            newStyleSheet.type = "text/css";
                            newStyleSheet.rel = "stylesheet";
                            newStyleSheet.href = document.styleSheets[i].href;
                            innerDocument.head.appendChild(newStyleSheet);
                        }
                    }
                }
                body = innerDocument.getElementsByTagName('body')[0];
            }
            // On ajoute une classe au body de l'iframe pour pouvoir déclarer dans les CSS des styles à appliquer au corps de page sans écraser ceux de l'application (document courant)
            if (body) {
                body.id = "extension" + sID + "ContentsBody";
                descriptionContents.style.display = "";
                body.appendChild(descriptionContents);
            }
        }
    }
    catch (e) {
        console.log(e.message);
    }
}

var extensionScreenshotScrollTimer = null;
nsAdmin.extensionScreenshotScroll = function (direction, timer) {
    var fctScroll = function (direction) {
        var container = document.getElementById("extensionScreenshotsSubContainer");
        if (!container)
            return;

        if (direction == 0) {
            container.scrollLeft -= 600;
        }
        else {
            container.scrollLeft += 600;
        }
    }

    if (extensionScreenshotScrollTimer == null) {
        extensionScreenshotScrollTimer = window.setTimeout(fctScroll, timer);
    }
    else {
        fctScroll(direction);
    }
}
nsAdmin.extensionScreenshotScrollEnd = function () {
    window.clearTimeout(extensionScreenshotScrollTimer);
    extensionScreenshotScrollTimer = null;
}

/* Crée une demande d'information sur HOTCOM */
nsAdmin.sendInfoRequest = function () {
    //window.open("http://www.eudonet.fr", '_blank');
    window.open(top.EudoWebSite, '_blank');

    var oExtensionUpdater = new eUpdater("eda/Mgr/eAdminExtensionManager.ashx", 1);

    oExtensionUpdater.ErrorCallBack = function () { setWait(false); };

    oExtensionUpdater.addParam("action", "request", "post");

    oExtensionUpdater.send();
}

var StoreListTypeRefresh = { FULL: 0, LIST: 1, ADD_ITEM: 2 };
var nsAdminStoreList = nsAdminStoreList || {
    totalModule: 0,
    totalPages: 0,
    rows: 0,
    currentPage: 1,
    currentFilterSearch: '',
    currentFilterCategory: '',
    currentFilterOffers: '',
    currentFilterDisplay: '',
    currentFilterOther: '',
    isRefreshing: false,
    setTimeoutId: null
};

nsAdmin.StoreRefresh = function (typeRefresh) {
    nsAdminStoreList.isRefreshing = true;

    // rafraichis la zone (criteres de filtres qui change, criteres de recherche, etc.)
    nsAdmin.loadAdminModule(USROPT_MODULE_ADMIN_EXTENSIONS, null, {
        notReloadNavBar: true,
        notReloadMenu: true,
        notFirstLoad: true,
        type: typeRefresh,
        callback: function () {
            nsAdminStoreList.isRefreshing = false;
        }
    });
}

nsAdmin.FilterChange = function () {
    // On se repositionne sur la page 1
    nsAdminStoreList.currentPage = 1;

    // rafraichis la zone (criteres de filtres qui change, criteres de recherche, etc.)
    nsAdmin.StoreRefresh(StoreListTypeRefresh.LIST);
}

nsAdmin.StoreNextPage = function () {
    // On increment la page
    nsAdminStoreList.currentPage++;

    nsAdmin.StoreRefresh(StoreListTypeRefresh.ADD_ITEM);
}

nsAdmin.StoreListScroll = function (sender) {
    if (nsAdminStoreList.isRefreshing)
        return;

    // On controle si nous ne sommes pas deja à la fin
    /*var fukol = document.getElementsByClassName("fukol")[0];
    var allnew = fukol.querySelectorAll("figure");
    if (allnew.length >= nsAdminStoreList.totalModule)
        return;*/

    if (nsAdminStoreList.currentPage >= nsAdminStoreList.totalPages)
        return;

    var scrollTop = parseInt(sender.scrollTop);
    var scrollHeight = parseInt(sender.scrollHeight);
    var clientHeight = parseInt(sender.clientHeight);
    var scrollBottom = scrollHeight - (scrollTop + clientHeight);

    var lag = 500; //temps où ne doit pas bouger la scroll bar avant qu'on déclenche vraiment le rafraichissement

    if (scrollBottom < 5) {
        nsAdminStoreList.setTimeoutId = setTimeout(nsAdmin.StoreNextPage, lag);
    }
    else if (scrollBottom > 5 && nsAdminStoreList.setTimeoutId != null) {
        clearTimeout(nsAdminStoreList.setTimeoutId);
        nsAdminStoreList.setTimeoutId = null;
    }
}

var storeSearchTimerExt;
var storePreviousSearch; //Dernière recherche d'extension
nsAdmin.StoreSearch = function (sSearch, e) {
    var bEnter = (e != null && e.keyCode && e.keyCode == 13);

    clearTimeout(storeSearchTimerExt);
    if (typeof (sSearch) === "string") {

        if (sSearch.length > 2 || (bEnter && sSearch.length > 0)
            || (sSearch.length == 0 && sSearch != previousExtSearch)) {
            var fct = function () {
                var eFSInput = document.getElementById("storeSearch");
                eFSInput.blur();

                // On actualise la donnée en local
                nsAdminStoreList.currentPage = 1;
                nsAdminStoreList.currentFilterSearch = sSearch;

                // On lance la recherche
                nsAdmin.StoreRefresh(StoreListTypeRefresh.LIST);
            };

            // Temporisation de la recherche
            storeSearchTimerExt = window.setTimeout(function () { fct() }, 800);
        }
    }

    storePreviousSearch = sSearch;
}

var storeInt = 0;
var storeTimeout;
var storeArrow = document.getElementsByClassName("slideArrow");
nsAdmin.StoreSliderAutoPlayAdvance = function () {
    // Fonction de calcul de l'autoplay
    clearTimeout(storeTimeout);
    storeTimeout = setTimeout(function () {
        if (storeInt < storeArrow.length - 2) {
            StoreSliderAutoPlay(storeInt += 1);
        } else {
            storeInt = 1;
            StoreSliderAutoPlay(storeInt);
        }
    }, 5000);
}

nsAdmin.StoreSliderAutoPlay = function (int) {
    // Fonction pour lancer l'autoplay
    var select_selected = document.getElementsByClassName("select-selected");
    if (select_selected[0] != undefined && select_selected[0].classList.contains("select-arrow-active")) {
        return;
    } else {
        storeArrow[int].click();
        StoreSliderAutoPlayAdvance();
    }
}

// Calcul de la height de la liste des Extension pour ne pas avoir de scrollBar sur le body
nsAdmin.StoreResizeContent = function () {
    //console.log('StoreResizeContent');
    var fukol;

    if ((document.getElementsByClassName("fukol")) && (document.getElementsByClassName("fukol").length > 0)) {
        fukol = document.getElementsByClassName("fukol")[0];
        fukol.style.height = window.innerHeight - 80 + "px";
    }
}

nsAdmin.LoadStoreList = function () {
    // Reprends les valeurs de filtres précédent choisi
    nsAdminStoreMenu.InitSelectedFilters();

    // Charge la combo des catégories sur les filtres du menu droite
    nsAdminStoreMenu.initFilterCategory();

    // On retire cette evenement car en régle générale l'application ne gére pas l'action utilisateur de resize de fenetre
    // Si il fallait gérer ce type d'action, il faudrait prendre en charge le "removeEventListener" après n'importe quelle action de la page
    // Calcul de la height de la liste des Extension pour ne pas avoir de scrollBar sur le body
    //document.addEventListener("resize", nsAdmin.StoreResizeContent);
}

nsAdmin.initFilterCategory = function (elParent, onChangeFunct) {
    if (!elParent)
        elParent = document;
    var x, i, j, selElmnt, a, b, c;
    /* Recherche tous les éléments de la classe "custom-select": */
    x = elParent.getElementsByClassName("custom-select");
    for (i = 0; i < x.length; i++) {
        selElmnt = x[i].getElementsByTagName("select")[0];
        if (typeof onChangeFunct === "function")
            selElmnt.onchange = function () { onChangeFunct(selElmnt) };
        /* Pour chaque élément, créer une nouvelle DIV qui agira comme l'élément sélectionné: */
        a = document.createElement("DIV");
        a.setAttribute("class", "select-selected");
        a.innerHTML = selElmnt.options[selElmnt.selectedIndex].innerHTML;
        a.title = selElmnt.options[selElmnt.selectedIndex].innerHTML;
        x[i].appendChild(a);
        /* Pour chaque élément, créer une nouvelle DIV qui contiendra la liste des options: */
        b = document.createElement("DIV");
        b.setAttribute("class", "select-items select-hide");
        for (j = 0; j < selElmnt.length; j++) {
            /* Pour chaque option de l'élément de sélection d'origine, créez une nouvelle DIV qui agira comme un élément d'option: */
            c = document.createElement("DIV");
            c.innerHTML = selElmnt.options[j].innerHTML;
            c.title = selElmnt.options[j].innerHTML;
            c.setAttribute("data-value", selElmnt.options[j].getAttribute("value"))
            c.addEventListener("click", function (e) {
                /* Lorsqu'un élément est cliqué, met à jour la zone de sélection d'origine et l'élément sélectionné: */
                var y, i, k, s, h;
                s = this.parentNode.parentNode.getElementsByTagName("select")[0];
                h = this.parentNode.previousSibling;
                for (i = 0; i < s.length; i++) {
                    if (s.options[i].getAttribute("value") == this.getAttribute("data-value")) {
                        s.selectedIndex = i;
                        h.innerHTML = this.innerHTML;
                        h.title = this.innerHTML;
                        y = this.parentNode.getElementsByClassName("same-as-selected");
                        for (k = 0; k < y.length; k++) {
                            y[k].removeAttribute("class");
                        }
                        this.setAttribute("class", "same-as-selected");
                        break;
                    }
                }
                h.click();
                s.onchange();
            });
            b.appendChild(c);
        }
        x[i].appendChild(b);
        a.addEventListener("click", function (e) {
            /* Lorsque la case de sélection est cliquée, fermez toutes les autres cases de sélection et ouvrez/fermez sur la case de sélection actuelle: */
            e.stopPropagation();
            nsAdmin.closeAllSelect(this);
            this.nextSibling.classList.toggle("select-hide");
            this.classList.toggle("select-arrow-active");

            // Si les valeurs de la combo sont affichées, on s'abonne au click pour les faire disparaitre au clique sur la fenêtre
            if (!this.nextSibling.classList.contains("select-hide")) {
                document.addEventListener("click", nsAdmin.closeAllSelect);
            }
        });
    }
}

nsAdmin.closeAllSelect = function (elmnt) {
    /* Une fonction qui ferme toutes les cases de sélection du document, sauf la boîte de sélection actuelle: */
    var x, y, i, arrNo = [];
    x = document.getElementsByClassName("select-items");
    y = document.getElementsByClassName("select-selected");

    for (i = 0; i < y.length; i++) {
        if (elmnt == y[i]) {
            arrNo.push(i)
        } else {
            y[i].classList.remove("select-arrow-active");
        }
    }

    for (i = 0; i < x.length; i++) {
        if (arrNo.indexOf(i)) {
            x[i].classList.add("select-hide");
        }
    }

    document.removeEventListener("click", nsAdmin.closeAllSelect);
}

/********************************************/

// Action au check de la case à cocher "Pas de séparateur" de milliers
nsAdmin.checkNoSep = function (element) {

    var txtSep = document.getElementById("textSectionsDelimiter");
    txtSep.disabled = getAttributeValue(element, "chk") == "1";
    if (txtSep.disabled) {
        txtSep.value = "";
        nsAdmin.sendJson(txtSep, false, true);
    }

    nsAdmin.sendJson(element, false, true);
}

var modalLanguages;
nsAdmin.confLanguages = function () {
    modalLanguages = new eModalDialog(top._res_7718, 0, "eda/eAdminLanguagesDialog.aspx", 700, 500, "modalLanguages");

    modalLanguages.noButtons = false;
    modalLanguages.show();

    modalLanguages.addButton(top._res_30, function () { modalLanguages.hide(); }, 'button-gray', null);

    nsAdmin.modalResizeAndMove(modalLanguages);
}

nsAdmin.addWebLink = function (element) {
    //var model = document.getElementById("weblink_template");
    //var clone = model.cloneNode(true);
    //clone.id = "";
    //var divParent = element.parentElement;
    //divParent.parentElement.insertBefore(clone, divParent);
    //// pouet
    var upd = new eUpdater("eda/mgr/eWebLinkManager.ashx", 1);
    upd.addParam("action", nsAdmin.WebLinkManagerAction.CREATE, "post");
    upd.addParam("tab", nsAdmin.tab, "post");

    upd.send(
        function (oRes) {
            try {
                var res = JSON.parse(oRes);

                if (res.Success) {
                    nsAdmin.editWebLinkProperties(res.Tab, res.SpecifId);
                    nsAdmin.updateWebLinksBlock(res);
                } else {
                    eAlert(1, res.ErrTitle, res.ErrorMessage);
                    return false;
                }
            }

            catch (e) {
                eAlert(1, "Une erreur est survenue.", oRes);
                return false;
            }
        }
    );
}

/// Recharge le bloc de menu "Liens web et traitements spécifiques"
nsAdmin.updateWebLinksBlock = function (oSpecInfos) {
    //console.log("updateNavBarSpecif");

    var upd = new eUpdater("eda/mgr/eWebLinkManager.ashx", 1);
    upd.addParam("action", nsAdmin.WebLinkManagerAction.UPDATEMENU, "post");
    upd.addParam("tab", oSpecInfos.Tab, "post");

    upd.send(
        function (oRes) {
            nsAdmin.refreshWebLinksBlock(oRes, oSpecInfos.SpecifId)
        }
    );
}

nsAdmin.refreshWebLinksBlock = function (oRes, specifId) {
    var paramPartContent = document.querySelector("#WebLinksPart .paramPartContent");

    var newElement = document.createElement("div");
    newElement.innerHTML = oRes;

    paramPartContent.innerHTML = newElement.querySelector(".paramPartContent").innerHTML;
}

// Edition des propriétes d'un lien web
nsAdmin.editWebLinkProperties = function (tab, specifId) {

    //Load Web Tab Properties
    var upd = new eUpdater("eda/mgr/eWebLinkManager.ashx", 1);
    upd.addParam("action", nsAdmin.WebTabManagerAction.GETINFOS, "post");
    upd.addParam("tab", tab, "post");
    upd.addParam("id", specifId, "post");

    upd.send(
        //Maj du tab
        // oRes contient le html de l'admin des propriété du weblink
        function (oRes) {
            nsAdmin.refreshTab(2, oRes);
        }

    );
}

/// Supprime le lien web
nsAdmin.deleteWebLink = function (tab, specifId) {

    var webLink = "";
    var webLink = document.querySelector("#weblink_" + specifId + " a");
    if (webLink)
        webLinkLabel = GetText(webLink);

    eAdvConfirm({
        'criticity': eAdvConfirm.CRITICITY.MSG_EXCLAM,
        'title': top._res_806,
        'message': (top._res_7818 + "").replace("<WEBLINK>", webLinkLabel), // "Suppression du lien "<WEBTAB>""
        'details': top._res_7637, // "Etes-vous sûr de vouloir le supprimer ?"
        'cssOk': 'button-red',
        'cssCancel': 'button-green',
        'resOk': top._res_19,
        'resCancel': top._res_29,
        'okFct': function () { nsAdmin.deleteWebLinkConfirmed(tab, specifId); }
    });
}

/// L'utilisateur à confirmé la suppression
nsAdmin.deleteWebLinkConfirmed = function (tab, specifId) {

    var upd = new eUpdater("eda/mgr/eWebLinkManager.ashx", 1);
    upd.addParam("action", nsAdmin.WebLinkManagerAction.DELETE, "post");
    upd.addParam("tab", tab, "post");
    upd.addParam("id", specifId, "post");
    upd.send(nsAdmin.deleteWebLinkReturn);
}

/// Retour serveur après suppression ou pas
nsAdmin.deleteWebLinkReturn = function (oRes) {

    var result = JSON.parse(oRes);
    if (!result.Success) {
        eAlert(2, top._res_806, result.ErrorMessage, "", 500, 200, null);
    } else {
        var webLink = document.getElementById("weblink_" + result.SpecifId)
        webLink.parentNode.removeChild(webLink);
    }
}



// Ouverture du lien du champ texte précédent
nsAdmin.openCloudLink = function (element, nMode) {
    var link = element.previousSibling;
    if (getAttributeValue(element, "ebtnparam") == "1" && getAttributeValue(link, "ehasbtn") == "1") {
        if (link && link.value != "") {
            var url = link.value;

            if (nMode == 1) {
                var nSpecid = getAttributeValue(link, "fid");
                var nTab = getAttributeValue(link, "did");
                runSpec(nSpecid, nTab, undefined, undefined, function (oRes) { nsAdmin.openAdminLink(oRes, url) });
            }
            else {
                if (!url.match(/^https?:\/\//i)) {
                    url = 'http://' + url;
                }
                window.open(url);
            }
        }
    }

};

nsAdmin.openAdminLink = function (oRes, adminUrl) {
    if (getXmlTextNode(oRes.getElementsByTagName("success")[0]) != "1") {
        var sErr = getXmlTextNode(oRes.getElementsByTagName("error")[0]);
        if (sErr != "")
            eAlert(0, "", sErr);
        else
            eAlert(0, "", top._res_6700);

        return;
    }

    var sToken = getXmlTextNode(oRes.getElementsByTagName("token")[0]);
    var sUrl = getXmlTextNode(oRes.getElementsByTagName("url")[0]);
    var sUrlParam = getXmlTextNode(oRes.getElementsByTagName("urlparam")[0]);
    var sOpenMode = getXmlTextNode(oRes.getElementsByTagName("openmode")[0]);
    var sLabel = "";
    var width;
    var height;
    var framesizeauto = true;

    var n = sUrl.lastIndexOf("?");
    if (n == -1)
        sUrl += "?xedntimer=" + timeStamp();
    else
        sUrl += "&xedntimer=" + timeStamp();

    var bNewWin = sOpenMode == OpenMode_NEW_WINDOW;

    sLabel = getXmlTextNode(oRes.getElementsByTagName("label")[0]);
    /*
    var aUrlParams = sUrlParam.split("&");
    for (var i = 0; i < aUrlParams.length; i++) {
        var aUrlParam = aUrlParams[i].split("=");
        if (aUrlParam[0] == "w")
            width = getNumber(aUrlParam[1]);
        else if (aUrlParam[0] == "h")
            height = getNumber(aUrlParam[1]);
        else if (aUrlParam[0] == "framesizeauto")
            framesizeauto = aUrlParam[1] != "0";
    }
    */

    openSpecMd(sUrl, sToken, sLabel, width, height, bNewWin, framesizeauto);
}


nsAdmin.updateVCardMappings = function (elSelect, mappingName) {
    //alert(mappingName);
    //alert(elSelect.value);

    var upd = new eUpdater("eda/Mgr/eAdminVCardMappingManager.ashx", 1);
    upd.addParam("action", "updateMapping", "post");
    upd.addParam("typeMapping", mappingName, "post");
    upd.addParam("descid", parseInt(elSelect.value), "post");

    upd.send(
        // Résultat
        function (oRes) {
            var res = JSON.parse(oRes);
            if (!res.Success) {
                eAlert(1, "Mise à jour mapping VCard", res.Error);
            }
            else {
                //todo
            }
        }
    );
}

// Ouverture de la popup de paramétrage du filtre du compteur signet
nsAdmin.openBkmCountFilterModal = function (element, nTab) {
    var options = {
        tab: nTab,
        value: getAttributeValue(element, "value"),
        deselectAllowed: true,
        adminMode: true,
        onApply: function (modal) {
            var currentFilterId = "0";
            var currentFilter = modal.getIframe()._eCurentSelectedFilter;
            if (currentFilter) {
                var oId = currentFilter.getAttribute("eid").split('_');
                currentFilterId = oId[oId.length - 1];
            }

            var btn = document.getElementById("btnCountFilter");
            if (btn) {
                btn.innerText = (currentFilterId != "0") ? top._res_8163 + " (1)" : top._res_8163;
            }

            nsAdmin.updateCountFilter(modal, element, nTab, currentFilterId);
        }
    }
    filterListObjet(0, options);

}

// Mise à jour du filtre du compteur signet
nsAdmin.updateCountFilter = function (modal, obj, nTab, value) {
    var capsule = new Capsule(nTab);
    var dsc = getAttributeValue(obj, "dsc");
    var arrDsc = dsc.split('|');
    capsule.AddProperty(arrDsc[0], arrDsc[1], value);

    var json = JSON.stringify(capsule);
    var upd = new eUpdater("eda/Mgr/eAdminDescManager.ashx", 1);
    upd.json = json;
    upd.send(function (oRes) {
        var res = JSON.parse(oRes);
        if (!res.Success) {
            top.eAlert(1, top._res_416, res.UserErrorMessage);
        }
        else {
            setAttributeValue(obj, "value", value);
            modal.hide();
        }

    });

}

// Règles d'archive, de suppression ou de pseudonymisation des fiches
nsAdmin.confRGPDConditions = function (nType) {

    var modalLabel;
    switch (nType) {
        case RGPDRuleType.ARCHIVING:
            modalLabel = top._res_8353;
            break;
        case RGPDRuleType.DELETING:
            modalLabel = top._res_8354;
            break;
        case RGPDRuleType.PSEUDONYM:
            modalLabel = top._res_8510;
            break;
        case RGPDRuleType.PJDELETING:
            modalLabel = top._res_8566;
            break;
    }

    modalRGPDConditions = new eModalDialog(modalLabel, 0, "eda/eAdminRGPDConditions.aspx", 1000, 350, "modalRGPDConditions");

    modalRGPDConditions.addParam("tab", nsAdmin.tab, "post");
    modalRGPDConditions.addParam("ruleType", nType, "post");

    modalRGPDConditions.noButtons = false;
    modalRGPDConditions.show();

    modalRGPDConditions.addButton(top._res_29, function () { modalRGPDConditions.hide(); }, 'button-gray', null);
    modalRGPDConditions.addButton(top._res_28, function () { nsAdmin.saveRGPDConditions(nsAdmin.tab); }, 'button-green', null);

    nsAdmin.modalResizeAndMove(modalRGPDConditions);
}

nsAdmin.saveRGPDConditions = function (nDescid) {
    var modal = top.eTools.GetModal('modalRGPDConditions');
    var modalDoc = modal.getIframe().document;

    var caps = new Capsule(nDescid);
    nsAdmin.addPropertyToCapsule(modalDoc.getElementById("ddlActive"), caps);
    nsAdmin.addPropertyToCapsule(modalDoc.getElementById("txtNbMonths"), caps);
    nsAdmin.addPropertyToCapsule(modalDoc.getElementById("hidRuleType"), caps);

    var upd = new eUpdater("eda/Mgr/eAdminDescManager.ashx", 1);
    var json = JSON.stringify(caps);
    upd.json = json;
    upd.ErrorCallBack = function () { };
    upd.send(function (oRes) {
        var res = JSON.parse(oRes);
        if (!res.Success) {
            top.eAlert(1, top._res_416, res.UserErrorMessage);

            modal.hide();
        }
        else {
            modal.fade(100);
        }
    });

}

nsAdmin.loadDashboardGrid = function (nTab, nFileId, nGridId) {
    var mainDiv = document.getElementById("mainDiv");

    var oContext = new eGridContext();
    oContext.tab = nTab;
    oContext.fileId = nFileId;
    oContext.gridId = nGridId;
    oContext.ref = mainDiv;
    oContext.height = mainDiv.offsetHeight;
    oContext.width = mainDiv.offsetWidth;

    oGridManager.refreshVisibility(oContext.tab);

    var grid = oGridManager.getGrid(nGridId, oContext);
    grid.load();
}

// Retourne le contexte d'un widget
nsAdmin.getWidgetContext = function (wid) {
    var widgetWrapper = top.document.getElementById("widget-wrapper-" + wid);
    if (!widgetWrapper)
        return null;

    var gridContainer = findUpByClass(widgetWrapper, "widget-grid-container");
    if (!gridContainer)
        return null;
    var gid = getAttributeValue(gridContainer, "gid");
    if (!gid || gid == "0")
        return null;

    var gwTab = findUpByClass(gridContainer, "gw-tab");
    var parentTab = getAttributeValue(gwTab, "parent-tab");
    if (!parentTab && gwTab) {
        var arrId = gwTab.id.split('-');
        if (arrId.length >= 3)
            parentTab = arrId[2];
    }
    var parentFileId = getAttributeValue(gwTab, "parent-file");

    var oWidgetContext = {
        GridId: gid,
        ParentTab: parentTab || "0",
        ParentFileId: parentFileId || "0",
        WidgetId: wid
    };
    return oWidgetContext;
}

// Paramètrage de saml
nsAdmin.Saml = (function () {
    var url = "eda/Mgr/eAdminSamlManager.ashx";
    function getConfig() {
        var textarea = document.getElementById("samlsettings");
        if (textarea == null)
            return null;

        try {
            return JSON.parse(textarea.value);
        } catch (e) {
            console.log("JSON Invalid : " + e);
            return null;
        }
    }
    function setConfig(config) {
        var textarea = document.getElementById("samlsettings");
        if (textarea == null)
            return;

        textarea.value = JSON.stringify(config, null, "\t");
        nsAdmin.sendJson(textarea, false, true);
    }

    function sendReturn(oRes) {
        if (oRes == null)
            return;

        var result = JSON.parse(oRes);
        if (result == null)
            return;

        if (!result.Success) {
            top.eAlert(1, result.ErrorTitle, result.ErrorMsg);
            return;
        }

        if (result.Html) {
            var div = document.createElement("div");
            div.innerHTML = result.Html;

            var target = document.getElementById("samlcontainer");
            var source = div.querySelector("#samlcontainer");

            if (target != null && source != null)
                target.innerHTML = source.innerHTML;
            if (target != null) {
                var tab = target.querySelector("#" + result.Action);
                if (tab)
                    tab.click();
            }

        }
    }

    function trace(id, value) {
        var updater = new eUpdater(url, 1);
        updater.addParam("action", "sp-config-trace", "post");
        updater.addParam("id", id, "post");
        updater.addParam("value", value, "post");
        updater.send(sendReturn);
    }

    return {
        enable: function (val) {

            var updater = new eUpdater(url, 1);
            updater.addParam("action", "sp-activation", "post");
            updater.addParam("saml2enable", val ? "1" : "0", "post");
            updater.send(sendReturn);

        },
        setValue: function (id, value) {
            var config = getConfig();
            if (config == null)
                return;

            config[id] = value;
            setConfig(config);

            trace(id, value);
        },
        map: function (index) {
            var config = getConfig();
            if (config == null)
                return;

            var attr = document.getElementById("Saml2Attribute_" + index);
            var field = document.getElementById("DescId_" + index);
            var checkbox = document.getElementById("IsKey_" + index);
            if (field == null || attr == null || checkbox == null)
                return;

            config.MappingAttributes[index]["Saml2Attribute"] = attr.value;
            config.MappingAttributes[index]["DescId"] = field.options[field.selectedIndex].value;
            config.MappingAttributes[index]["IsKey"] = getAttributeValue(checkbox, "chk") == "1";

            setConfig(config);
            trace(field.options[field.selectedIndex].value, attr.value + ":" + getAttributeValue(checkbox, "chk"));
        },
        setTab: function (evt) {
            var i, tabcontent, tablinks;
            tabcontent = document.getElementsByClassName("tabcontent");
            for (i = 0; i < tabcontent.length; i++) {
                tabcontent[i].style.display = "none";
            }
            tablinks = document.getElementsByClassName("tablinks");
            for (i = 0; i < tablinks.length; i++) {
                tablinks[i].className = tablinks[i].className.replace(" active", "");
            }
            var tar = document.getElementById(getAttributeValue(evt.currentTarget, 'val'));
            if (tar != null)
                tar.style.display = "block";

            evt.currentTarget.className += " active";

        },
        resetConfig: function () {
            var handler = eConfirm(1, top._res_201, top._res_6863, top._res_8143, 450, 200,
                function () {
                    var updater = new eUpdater(url, 1);
                    updater.addParam("action", "reset-metadata", "post");
                    updater.send(sendReturn);
                },
                function () { if (handler != null) handler.hide(); });
        },
        uploadMetadata: function (fileElement) {
            if (fileElement == null || fileElement.files == null || fileElement.files.length == 0)
                return;

            var handler = eConfirm(1, top._res_201, top._res_6863, top._res_8143, 450, 200,
                function () {
                    var uploader = new eUploader(url, true);
                    uploader.addParam("action", "idp-metadata");
                    uploader.addParam("file", fileElement.files[0]);
                    uploader.onProgress(function (current) { });
                    uploader.send(sendReturn);
                },
                function () { if (handler != null) handler.hide(); });
        }
    }
})();

///***************************
/// Objet permettant de gérer le poste d'un ou de plusieurs fichiers independemment du type ou du l'url
/// 
/// Il vérifie si la session n'est pas expirée
/// Il vérifie s'il y a une erreur serveur, elle est affichée.
/// Il déclenche les evenements onsuccess et onprogress pour le client
///
/// PS : Cet objet ne sert qu'à gérer la partie infrastructure
///
///*****************************
var eUploader = function (url, showProgress) {

    var externalSuccess = function () { };
    var externalProgress = function () { };
    var externalError = function () { };

    var request = new XMLHttpRequest();
    var formData = new FormData();
    var modal = null;

    var _url = url;
    var _showProgress = showProgress;

    // Post des fichiers
    function send() {

        if (_showProgress && modal == null)
            modal = createProgress();

        request.open('POST', _url, true);
        request.onreadystatechange = stateChanged;
        request.upload.onprogress = updateProgress;
        request.send(formData);
    }

    // Un event a changé l'etat de la requete
    function stateChanged(evt) {
        if (request.readyState == 4) {
            if (request.status == 200) {
                success(request.responseText);
            } else {
                error(evt, request.status);
            }
        }
    }

    // Le statut est 200
    function success(response) {
        checkSession();
        externalSuccess(response);

        if (_showProgress && modal != null)
            modal.hide();

        //Libération des ressources
        delete request;
        request = null;
        modal = null;
    }

    // Upload en cours
    function updateProgress(evt) {
        if (evt.lengthComputable) {
            var percent = (evt.loaded / evt.total * 100 | 0);
            externalProgress(percent);

            if (_showProgress && modal != null && percent >= 0 && percent <= 100)
                modal.updateProgressBar(prog);
        }
    }

    // Erreur le cas echeant
    function error(evt, status) {
        // Gestion des erreurs status != 200
        oErrorObj = new Object();
        oErrorObj.Type = "1";
        oErrorObj.Title = top._res_416; // Erreur
        oErrorObj.Msg = top._res_72; // Une erreur est survenue
        oErrorObj.DetailMsg = top._res_544; // cette erreur a été transmise à notre équipe technique.
        oErrorObj.DetailDev = "Code erreur " + status + " onreadystatechange\n" + request.responseText;
        that.trace(oErrorObj.DetailDev);
        eAlertError(oErrorObj, okErrorFct);
    }

    // Valide la session
    function checkSession() {
        var bError = request.getResponseHeader("X-EDN-ERRCODE") == "1";
        var bSessionLost = request.getResponseHeader("X-EDN-SESSION") == "1";
        //Gestion d'erreur
        if (bError || bSessionLost) {
            // si type de retour txt, transformation du flux txt en xml
            oErrorObj = errorXML2Obj(createXmlDoc(request.responseText), bSessionLost);
            if (bSessionLost) {
                //Message d'avertissement - Valeur "par défaut"                         
                oErrorObj.Type = (oErrorObj.Type == "") ? "1" : oErrorObj.Type;
                oErrorObj.Title = (oErrorObj.Title == "") ? top._res_503 : oErrorObj.Title; // votre session a expiré...
                oErrorObj.Msg = (oErrorObj.Msg == "") ? top._res_6068 : oErrorObj.Msg; // votre session a expiré...détail
                oErrorObj.DetailMsg = oErrorObj.DetailMsg;
                oErrorObj.DetailDev = oErrorObj.DetailDev;

                // remplace le callback intial par un retour à l'accueil
                internalErrorCallBack = function () {
                    top.document.location = "elogin.aspx";
                }
            }
        }

        if (bError || bSessionLost) {
            //  Affichage de l'erreur
            eAlertError(oErrorObj, okErrorFct);
            return;
        }
    }

    // Retir le setwait
    function okErrorFct() {
        setWait(false);
        if (_showProgress && modal != null)
            modal.hide();

        externalError(request.status);
    }

    // modal
    function createProgress() {
        modalProgress = null;
        prog = 0;
        modalProgress = new eModalDialog(top._res_6545, 4, top._res_6546, 550, 160, "modalProgressUpload");
        modalProgress.noButtons = true;
        modalProgress.hideMaximizeButton = true;
        modalProgress.show();
        return modalProgress;
    }

    return {
        // Ajout de param au post
        addParam: function (key, value) { formData.append(key, value); },

        // callback de progression
        onProgress: function (onProgress) { externalProgress = onProgress; },

        // callback en cas d'erreur
        onError: function (onError) { externalError = onError; },

        // Post le formulaire
        send: function (onSuccess) { externalSuccess = onSuccess; send(); }
    }
};



// Appel du manager permettant de mettre à jour DESCADV, param DEFAULT_HOMEPAGE_ID
nsAdmin.updateDefaultHomepage = function (element) {

    var nHpId = getNumber(getAttributeValue(element, "data-hpid"));
    if (!nHpId > 0)
        return;

    // Décoche les autres pages d'accueil
    var chks = document.querySelectorAll(".chkDefaultHP[chk='1']");
    for (var i = 0; i < chks.length; i++) {
        if (getAttributeValue(chks[i], "data-hpid") != nHpId)
            chgChk(chks[i], false);
    }

    var aDesc = getAttributeValue(element, "dsc").split("|");
    if (aDesc.length < 2)
        return;

    var caps = new Capsule(0);
    if (getNumber(getAttributeValue(element, "chk")) === 0)
        nHpId = 0

    caps.AddProperty(aDesc[0], aDesc[1], nHpId);

    var json = JSON.stringify(caps);
    var upd = new eUpdater("eda/Mgr/eAdminConfigAdvManager.ashx", 1);

    upd.json = json;
    upd.ErrorCallBack = function () { alert('La mise à jour de la page d\'accueil par défaut a échoué'); };
    upd.send();
}

nsAdmin.toggleSMTPServerParams = function (rb) {
    var defaultParams = document.getElementById("blockServerDefaultParams");
    var params = document.getElementById("blockServerParams");
    if (rb.value == "1") {
        defaultParams.style.display = "block";
        params.style.display = "none";
    }
    else {
        params.style.display = "block";
        defaultParams.style.display = "none";
    }
}

var modalTeamsMapping;
nsAdmin.confTeamsMapping = function (nTab) {
    //Administrer la création de Rendez-vous dans Teams
    modalTeamsMapping = new eModalDialog(top._res_3026, 0, "eda/mgr/eAdminTeamsMapping.ashx", 985, 650, "modalTeamsMapping");
    modalTeamsMapping.addParam("tab", nTab, "post");
    modalTeamsMapping.onIframeLoadComplete = function () {
        modalTeamsMapping.getIframe().nsAdminTeamsMapping.init();
    };
    modalTeamsMapping.show();

    modalTeamsMapping.addButton(top._res_30, function () { modalTeamsMapping.hide(); }, 'button-gray', null);
    modalTeamsMapping.addButton(top._res_28,
        function () {
            var frame = modalTeamsMapping.getIframe();
            var mapping = frame.nsAdminTeamsMapping.getMapping();
            if (!frame.nsAdminTeamsMapping.isMappingValid(mapping)) {
                eAlert(eAdvConfirm.CRITICITY.MSG_EXCLAM, "", top._res_3058);                //Merci de saisir les informations obligatoires indiquées par un astérisque rouge
                return;
            }
            nsAdmin.saveTeamsMapping(nTab, mapping);
        }, 'button-green', null);

};


nsAdmin.saveTeamsMapping = function (nTab, mapping) {

    var upd = new eUpdater("eda/mgr/eAdminTeamsMapping.ashx", "1");

    upd.addParam("action", 1, "post");
    upd.addParam("tab", nTab, "post");
    //upd.addParam("mapping", JSON.stringify(nsAdminTeamsMapping.getMapping()), "post");
    upd.addParam("mapping", JSON.stringify(mapping), "post");

    upd.send(nsAdmin.saveTeamsMappingCallBack);
};

nsAdmin.saveTeamsMappingCallBack = function (sRes) {
    var oRes = JSON.parse(sRes);
    if (oRes.Success) {
        modalTeamsMapping.hide();
        return;
    }


    top.eAlert(oRes.Criticity, oRes.UserMessage, oRes.UserFullMessage + " " + oRes.UserErrorMessage);



};


// END NSADMIN
// -----------------------------------------
/******************************************* ADMIN WEB TAB *************************************************/

nsAdminWebTagDragDrop = {};

nsAdminWebTagDragDrop.debug = false;
nsAdminWebTagDragDrop.handlerSet = false;

nsAdminWebTagDragDrop.handleDragStart = function (e) {
    if (nsAdminWebTagDragDrop.debug) { console.log("handleDragStart "); }

    e.dataTransfer.effectAllowed = 'move';
    if (nsAdminFile.browser.isFirefox)
        e.dataTransfer.setData("text", e.target);

    // au début d'un d&d, on "sauvegarde" l'élement d&d
    nsAdmin.initDragDrop(this);

    this.classList.add('dragElem');

    var initElem = nsAdmin.dragSrcElt;
    var dragType = getNumber(getAttributeValue(initElem, "edragtype"));
    if (dragType == nsAdmin.DragType.WEB_TAB_MENU || dragType == nsAdmin.DragType.GRID_MENU)
        addClass(document.getElementById("admWebTabDropArea"), "ondragwebtab");
}

nsAdminWebTagDragDrop.handleDragOver = function (e) {
    if (nsAdminWebTagDragDrop.debug) { console.log("handleDragOver "); }

    //faut faire un preventDefault pour le d&d,
    if (e.preventDefault) {
        e.preventDefault();
    }

    //l'élément survoler passe en over
    // -> pour l'instant , on met juste un "taquet" pour signaler la position

    //this.classList.add('over');

    nsAdminWebTagDragDrop.ShadowElement(this, true);

    e.dataTransfer.dropEffect = 'move';

    return false;
}

nsAdminWebTagDragDrop.ShadowElement = function (elem, bOn) {
    //console.log("ShadowElement ");

    if (bOn) {
        elem.classList.add('over');

        if (document.getElementById("webtabshadowelem"))
            document.getElementById("webtabshadowelem").remove();

        return;
        //<li onclick="nsAdmin.editWebTabProperties(200,11);" edragtype="5" draggable="true" fid="11" class="selectedTab"><span>Nouveau Onglet Web</span></li>
        var li = document.createElement("li");
        li.id = "webtabshadowelem";
        li.className = "selectedTab webtabdrophere";
        setAttributeValue(li, "edragtype", "5");

        var span = document.createElement("span");
        span.innerHTML = "&nbsp;";
        li.appendChild(span);
        if (elem.nextSibling) {
            elem.parentNode.insertBefore(li, elem.nextSibling);
        }
        else {
            elem.parentNode.appendChild(li);
        }
    }
    else {
        elem.classList.remove('over');
    }
}

Element.prototype.remove = function () {
    this.parentElement.removeChild(this);
}

NodeList.prototype.remove = HTMLCollection.prototype.remove = function () {
    for (var i = this.length - 1; i >= 0; i--) {
        if (this[i] && this[i].parentElement) {
            this[i].parentElement.removeChild(this[i]);
        }
    }
}

nsAdminWebTagDragDrop.handleDragEnter = function (e) {
    if (nsAdminWebTagDragDrop.debug) { console.log("handleDragEnter"); }
}

nsAdminWebTagDragDrop.handleDragLeave = function (e) {
    if (nsAdminWebTagDragDrop.debug) { console.log("handleDragLeave"); }

    //   this.classList.remove('over');
    nsAdminWebTagDragDrop.ShadowElement(this, false);
}

nsAdminWebTagDragDrop.handleDrop = function (e) {
    if (nsAdminWebTagDragDrop.debug) { console.log("handleDrop"); }

    if (e.preventDefault) { e.preventDefault(); }
    if (e.stopPropagation) { e.stopPropagation(); }

    removeClass(document.getElementById("admWebTabDropArea"), "ondragwebtab");

    //on sauvegarde l'élément de départ
    var initElem = nsAdmin.dragSrcElt;

    //this.classList.remove('over');
    nsAdminWebTagDragDrop.ShadowElement(this, false);

    var nTypeDrop = getNumber(getAttributeValue(this, "edragtype"));
    var nTypeDropped = getNumber(getAttributeValue(initElem, "edragtype"));
    var nSubType = getNumber(getAttributeValue(initElem, "esubtype"));
    var ednType = getNumber(getAttributeValue(document.getElementById("fileDiv_" + nGlobalActiveTab), "edntype"));
    //console.log("DropZone Type : " + nTypeDrop);
    //console.log("Drop Type : " + nTypeDropped);
    //console.log("Drop SubType : " + nSubType);

    if (document.getElementById("webtabshadowelem"))
        document.getElementById("webtabshadowelem").remove();

    //invalide dropzone.
    if (nTypeDrop != nsAdmin.DragType.WEB_TAB && nTypeDrop != nsAdmin.DragType.GRID && nTypeDrop != nsAdmin.DragType.WEB_TAB_LIST)
        return;

    //Cet handle corresspond à la drop zone des web tab.
    // on ne peut que d&d des webtab ou l'icone d'ajout
    if (nTypeDropped != nsAdmin.DragType.WEB_TAB_LIST && nTypeDropped != nsAdmin.DragType.WEB_TAB && nTypeDropped != nsAdmin.DragType.WEB_TAB_MENU && nTypeDropped != nsAdmin.DragType.GRID_MENU && nTypeDropped != nsAdmin.DragType.GRID)
        return;

    if (initElem == this)
        return;

    if (nTypeDropped == nsAdmin.DragType.WEB_TAB_MENU || nTypeDropped == nsAdmin.DragType.GRID_MENU) {
        //Ajout nouveau élément
        var newPosElem = this;

        //Position
        var ls = [].slice.call(document.getElementById("ulWebTabs").querySelectorAll("li[edragtype='" + nsAdmin.DragType.WEB_TAB + "'], li[edragtype='" + nsAdmin.DragType.GRID + "'] , li[edragtype='" + nsAdmin.DragType.WEB_TAB_LIST + "']"));

        // dans le cas des onglets web et des onglets "Grille" il n'y a pas le sous onglet "liste des fiches" on rajoute donc une case factice dans le tableau
        if (ednType == EDNTYPE_GRID || ednType == EDNTYPE_WEBTAB) {
            ls.unshift(null);
        }

        var idxNew = ls.indexOf(newPosElem);

        if (idxNew == ls.indexOf(document.getElementById("admWebTabDropArea")))
            idxNew--;

        if (idxNew < 0)
            idxNew = 0;
        else
            idxNew++;

        if (nTypeDropped == nsAdmin.DragType.WEB_TAB_MENU) {
            nsAdminWebTagDragDrop.createWebTab(idxNew, nSubType);
        }
        else if (nTypeDropped == nsAdmin.DragType.GRID_MENU) {
            oGridController.grid.new(nGlobalActiveTab, idxNew);
        }
    }
    else if (nTypeDropped == nsAdmin.DragType.WEB_TAB || nTypeDropped == nsAdmin.DragType.GRID || nTypeDropped == nsAdmin.DragType.WEB_TAB_LIST) {
        //déplacement élément

        var newPosElem = this;
        var movedFID = getAttributeValue(initElem, "fid");
        var moveFID = getAttributeValue(this, "fid"); //fid de l element a remplacer
        //nouvelle position

        var ls = [].slice.call(document.getElementById("ulWebTabs").querySelectorAll("li[edragtype='" + nsAdmin.DragType.WEB_TAB + "'], li[edragtype='" + nsAdmin.DragType.GRID + "'], li[edragtype='" + nsAdmin.DragType.WEB_TAB_LIST + "']"));

        // dans le cas des onglets web et des onglets "Grille" il n'y a pas le sous onglet "liste des fiches" on rajoute donc une case factice dans le tableau
        if (ednType == EDNTYPE_GRID || ednType == EDNTYPE_WEBTAB) {
            ls.unshift(null);
        }

        var idxNew = ls.indexOf(newPosElem);

        var idxOld = ls.indexOf(initElem);

        // if (idxNew < idxOld)
        //    idxNew++;

        if (idxNew < 0)
            idxNew = 0;
        else if (idxNew >= ls.length)
            idxNew = ls.length - 1;


        if (nTypeDropped == nsAdmin.DragType.WEB_TAB)
            nsAdminWebTagDragDrop.moveWebTab(movedFID, idxNew);
        else if (nTypeDropped == nsAdmin.DragType.WEB_TAB_LIST) //Dans le cas de la liste des fiches on utilise le deplacement des grilles "a l envers" => Evite du code surperflus
            oGridController.grid.move(nGlobalActiveTab, moveFID, idxNew, idxOld);
        else if (nTypeDropped == nsAdmin.DragType.GRID)
            oGridController.grid.move(nGlobalActiveTab, movedFID, idxOld, idxNew);

    }

    return false;
}

///fin de d&d
nsAdminWebTagDragDrop.handleDragEnd = function (e) {
    if (nsAdminWebTagDragDrop.debug) { console.log("handleDragEnd"); }

    removeClass(document.getElementById("admWebTabDropArea"), "ondragwebtab");

    //on retire la classe de over
    //this.classList.remove('over');
    nsAdminWebTagDragDrop.ShadowElement(this, false);

    //on reinit l'élément d&d
    nsAdmin.resetDragDrop();
}

///Gestion des handler de drag&drop des webtab
nsAdminWebTagDragDrop.addHandlerMoveTab = function (elem) {
    //console.log("addHandlerMoveTab" + " " + elem.getAttribute("id"));

    //retire tous les listener
    var elemTmp = elem.cloneNode(true);
    elem.parentNode.replaceChild(elemTmp, elem);

    elem = elemTmp;

    //affecte les listener
    // les webtab sont à la fois des éléments a d&d et des zones de drop
    elem.addEventListener('dragstart', nsAdminWebTagDragDrop.handleDragStart, false);
    elem.addEventListener('dragenter', nsAdminWebTagDragDrop.handleDragEnter, false)
    elem.addEventListener('dragover', nsAdminWebTagDragDrop.handleDragOver, false);
    elem.addEventListener('dragleave', nsAdminWebTagDragDrop.handleDragLeave, false);
    elem.addEventListener('drop', nsAdminWebTagDragDrop.handleDrop, false);
    elem.addEventListener('dragend', nsAdminWebTagDragDrop.handleDragEnd, false);
}

nsAdminWebTagDragDrop.addHandlerDropNewTab = function (elem) {
    //console.log("addHandlerDropNewTab" + " " + elem.getAttribute("id"));

    //retire tous les listener
    var elemTmp = elem.cloneNode(true);
    elem.parentNode.replaceChild(elemTmp, elem);
    elem = elemTmp;

    //
    elem.addEventListener('dragstart', nsAdminWebTagDragDrop.handleDragStart, false);
    elem.addEventListener('dragend', nsAdminWebTagDragDrop.handleDragEnd, false);
}
//
nsAdminWebTagDragDrop.moveWebTab = function (nMovedId, nMoveToId) {
    //Load Web Tab Properties
    var upd = new eUpdater("eda/mgr/eWebTabManager.ashx", 1);
    upd.addParam("action", nsAdmin.WebTabManagerAction.MOVETAB, "post");
    var ednType = getAttributeValue(document.getElementById("fileDiv_" + nGlobalActiveTab), "ednType")
    if (ednType)
        upd.addParam("ednType", ednType, "post");

    upd.addParam("moveto", nMoveToId, "post");
    upd.addParam("id", nMovedId, "post");
    upd.addParam("tab", nGlobalActiveTab, "post");

    upd.send(

        //Maj du tab
        // oRes contient le html de l'admin des propriété du webtab
        function (oRes) {
            //met à jour la navbar et la barre de propriété
            nsAdmin.updateNavBarSpecifMaj(oRes, nMovedId);
            nsAdmin.editWebTabProperties(nGlobalActiveTab, nMovedId)
        }

    );
}

nsAdminWebTagDragDrop.createWebTab = function (nPos, nType) {
    //Load Web Tab Properties
    var upd = new eUpdater("eda/mgr/eWebTabManager.ashx", 1);
    upd.addParam("action", nsAdmin.WebTabManagerAction.CREATE, "post");
    var ednType = getAttributeValue(document.getElementById("fileDiv_" + nGlobalActiveTab), "ednType")
    if (ednType)
        upd.addParam("ednType", ednType, "post");

    upd.addParam("createat", nPos, "post");
    upd.addParam("tab", nGlobalActiveTab, "post");
    upd.addParam("type", nType, "post");

    upd.send(

        //Maj du tab
        // oRes contient le résulat de la demande
        function (oRes) {
            try {
                var res = JSON.parse(oRes);

                if (res.Success) {
                    nsAdmin.updateNavBarSpecif(res);
                    nsAdmin.editWebTabProperties(res.Tab, res.SpecifId)
                } else {
                    eAlert(1, res.ErrTitle, res.ErrorMessage);
                    return false;
                }
            }

            catch (e) {
                eAlert(1, "Une erreur est survenue.", oRes);
                return false;
            }
        }

    );
};


var nsAdminTabsList = {};

///summary
///Tri de la liste des onglets en admin
///#56 749 - Le tri est désormais effectué côté client, mais mémorisé côté serveur, au même titre que la recherche
///On fait donc un tri JavaScript, puis un appel serveur pour mémoriser celui-ci de manière transparente
///
nsAdminTabsList.sort = function (sCol, iSort, sSearchValue, saveSortOrder) {

    // Si le tri actuel est le même que celui demandé (ex : clic multiple sur le bouton de tri) avec le même critère de recherche, on ignore
    if (sCol == nsAdminTabsList.currentSortCol && iSort == nsAdminTabsList.currentSortDir && sSearchValue == nsAdminTabsList.currentSearch)
        return;

    // Tri côté client - (C) & (TM) SPH
    // --------------------------------

    // Conversion ID de colonne <> index
    var sortColumnIndex = Array.from(document.getElementById("tabListTable").querySelectorAll("th")).indexOf(document.getElementById("tabListTableHeader" + sCol)); // TK #6745
    var fctGetContent = function (myCell) {

        if (myCell.hasAttribute("ednval"))
            return getAttributeValue(myCell, "ednval")

        var mycontent = myCell.querySelector(".contentCellLAb")
        if (mycontent)
            return getAttributeValue(mycontent, "data-label")


        return myCell.innerHTML;
    }

    // Récupération des éléments à trier : lignes dont la classe CSS débute par "line"
    var arrAllEntry = getTabLinesAsArray("tabListTable", "line");

    // Filtrage du contenu
    elem1 = fctGetContent(arrAllEntry[0].cells[sortColumnIndex]).trim();

    // Récupération de la fonction de comparaison adaptée au contenu de la colonne à trier (numérique ou non) via eTools
    var comp = getContentsComparerFunction(elem1);

    // On dispose à présent d'un type Array sur lequel on peut trier, en passant une fonction locale en paramètre
    // (tous les navigateurs gérés par l'admin E2017 implémentent cette fonctionnalité)
    arrAllEntry.sort(
        // Fonction interne de tri du tableau
        function sort(tr1, tr2) {

            elem1 = fctGetContent(tr1.cells[sortColumnIndex]).toLowerCase().trim();
            elem2 = fctGetContent(tr2.cells[sortColumnIndex]).toLowerCase().trim();

            if (iSort === 0) // 0 = ascendant, 1 = descendant
                return (comp(elem1, elem2));
            else
                return (comp(elem2, elem1));
        }
    );

    nsAdminTabsList.afterSortOrSearch(arrAllEntry, true);

    // Mémorisation du tri actuel (pour interaction avec la recherche, et ne pas le refaire si nouveau clic sur le même bouton de tri)
    nsAdminTabsList.currentSortCol = sCol;
    nsAdminTabsList.currentSortDir = iSort;

    // Mémorisation du tri côté serveur, sans setWait (action non visible pour l'utilisateur)
    // --------------------------------------------------------------------------------------

    if (saveSortOrder) {
        var oFileUpdater = new eUpdater("eda/mgr/eAdminTabsManager.ashx", 1);
        oFileUpdater.addParam("module", getUserOptionsModuleHashCode(USROPT_MODULE_ADMIN_TABS), "post");
        oFileUpdater.addParam("action", "1"); // UPDATE
        oFileUpdater.addParam("sortcol", sCol, "post");
        oFileUpdater.addParam("sort", iSort, "post");
        oFileUpdater.ErrorCallBack = function () { };
        oFileUpdater.send();
    }
};

///summary
///Recherche sur la liste des onglets en admin
///#56 749 - La recherche est désormais mémorisée côté serveur, au même titre que le tri
///On fait donc la recherche via la méthode centralisée launchSearch() commune à toutes les listes (eList.js), puis un appel serveur pour mémoriser la dernière valeur de
///recherche prise en compte, de manière transparente
///
nsAdminTabsList.search = function (sSearchValue, event, sTableId, saveSearch) {

    // Tri côté client - via eList.launchSearch

    launchSearch(sSearchValue, event, document.getElementById(sTableId));

    // Mémorisation de la recherche actuelle (pour interaction avec le tri)
    nsAdminTabsList.currentSearch = previousSearch; // variable globale mise à jour par launchSearch

    // On redéclenche le tri actuel sur les colonnes nouvellement filtrées, sans mettre à jour l'information côté serveur
    if (typeof (nsAdminTabsList.currentSortCol) != "undefined" && typeof (nsAdminTabsList.currentSortDir) != "undefined")
        nsAdminTabsList.sort(nsAdminTabsList.currentSortCol, nsAdminTabsList.currentSortDir, false);
    // Si on ne redéclenche pas le tri, on déclenche malgré tout le réajustement des CSS, pour alterner les couleurs des lignes, du fait que certaines ont pu être
    // masquées par la recherche
    nsAdminTabsList.afterSortOrSearch(getTabLinesAsArray("tabListTable", "line"), false);

    // Mémorisation de la recherche côté serveur, sans setWait (action non visible pour l'utilisateur)

    if (saveSearch) {
        var oFileUpdater = new eUpdater("eda/mgr/eAdminTabsManager.ashx", 1);
        oFileUpdater.addParam("module", getUserOptionsModuleHashCode(USROPT_MODULE_ADMIN_TABS), "post");
        oFileUpdater.addParam("action", "1"); // UPDATE
        oFileUpdater.addParam("search", previousSearch, "post"); // variable globale mise à jour par launchSearch
        oFileUpdater.ErrorCallBack = function () { };
        oFileUpdater.send();
    }
};

///summary
/// Après un tri ou une recherche, on réinsère les éléments dans le bon ordre (tri uniquement), et/ou on réajuste les CSS des lignes pour réalterner les couleurs après avoir
/// modifié leur emplacement (tri) ou en avoir masqué (recherche) en appelant la méthode readjustTableLineStyle de eList.js
nsAdminTabsList.afterSortOrSearch = function (arrAllEntry, reinsertElementOnParent) {
    if (!arrAllEntry)
        arrAllEntry = getTabLinesAsArray(oTargetTable);

    arrAllEntry.forEach(function (myElt) {
        // Après le tri, il faut dû?placer chaque ligne
        if (reinsertElementOnParent)
            myElt.parentElement.appendChild(myElt);

        // On réajuste les styles CSS pour rétablir correctement l'alternance de couleurs entre les lignes, en tenant compte des lignes masquées
        // (la précédente ligne comparée doit être la précédente visible, qui n'est pas forcément la précédente dans le DOM si une recherche l'a masquée)
        readjustTableLineStyle(myElt, "line1", "line2");
    });
};

/// Entre en édition dans la cellule cliquée de la liste des onglets
nsAdminTabsList.edit = function (e, cell) {
    if (cell.className.indexOf(" edit") > -1) {
        e.preventDefault(); // on interrompt l'évènement de clic sur la cellule pour laisser le contrôle d'édition tranquille
        return;
    }
    var cellID = cell.className.replace("cell adminTabs", "");
    switch (cellID) {
        case "EudonetX":
            var editControlID = "TabsListEudonetX" + cell.parentNode.getAttribute("did");
            var editControl = nsAdmin.createMultiSwitchPref(
                editControlID,
                nsAdmin.getEudonetXIrisBlackStatusValues(GetText(cell)),
                // Fonction renvoyant le libellé à afficher à côté du contrôle : rien
                function () { return ""; },
                // Fonction a exécuter au clic sur le contrôle : rien
                null,
                // Fonction exécutée au changement de valeur : sauvegarde et remise de la cellule en lecture seule
                function () {
                    var ddl = document.getElementById("ddlNw" + editControlID);
                    var targetTab = cell.parentNode.getAttribute("did");
                    var newValue = ddl.options[ddl.selectedIndex].value;
                    var caps = new Capsule(targetTab);
                    caps.AddProperty(nsAdmin.Category.DESCADV, document.getElementById("tabListTableHeaderEudonetX").getAttribute("descadvparameter"), newValue);
                    var upd = new eUpdater("eda/Mgr/eAdminDescManager.ashx", 1);
                    upd.json = JSON.stringify(caps);

                    upd.ErrorCallBack = function () {
                        setWait(false);
                    };
                    setWait(true);

                    upd.send(function (oRes) {
                        var ddl = document.getElementById("ddlNw" + editControlID);
                        cell.innerHTML = ddl.options[ddl.selectedIndex].text;
                        cell.className = cell.className.replace(" edit", "");

                        let TypeIris;

                        nsAdmin.fnInsertNewTab(["dvIrisBlackInput", "dvIrisBlackInputPreview"], EUDONETX_IRIS_CRIMSON_LIST_STATUS.DISABLED);

                        if (newValue == EUDONETX_IRIS_BLACK_STATUS.PREVIEW)
                            TypeIris = "dvIrisBlackInputPreview";

                        if (newValue == EUDONETX_IRIS_BLACK_STATUS.ENABLED)
                            TypeIris = "dvIrisBlackInput";

                        if (TypeIris)
                            nsAdmin.fnInsertNewTab(TypeIris, newValue, targetTab); // #94 445 - MAJ dans eParamIFrame pour que la modification soit prise en compte sans recharger toute l'application

                        setWait(false);
                    });
                }
            );
            editControl.className = "dvSwitchTabsListEudonetX";
            cell.innerHTML = "";
            cell.appendChild(editControl);
            cell.className += " edit";
    }
};

////////////////////////////////////////////////////

var nsAdminApi = nsAdminApi || {};
nsAdminApi.changeRate = function (obj) {

    top.setWait(true);

    var value = obj.options[obj.selectedIndex].value
    var upd = new eUpdater("eda/Mgr/eAdminExtensionAPIManager.ashx", 1);

    upd.ErrorCallBack = function () {
        top.setWait(false);
    };

    var objMapp = {
        action: "update",
        param: "rate",
        value: value
    }

    upd.json = JSON.stringify(objMapp);
    upd.send(function (oRes) { nsAdminApi.ProcessResult(oRes, objMapp, obj); })
}


nsAdminApi.ProcessResult = function (oRes, objMapp, oSelect) {
    top.setWait(false);

    var result = JSON.parse(oRes)
    if (!result.Success) {
        //on remet la dernière valeur connue
        var lastValid = getAttributeValue(oSelect, "elastvalid");
        if (lastValid) {
            var myOpt = oSelect.querySelector("option[value='" + lastValid + "']")
            oSelect.selectedIndex = myOpt.index
        }
        else
            oSelect.selectedIndex = 0;

        top.eAlert(0, result.ErrorTitle, result.ErrorMsg);
    }
    else {
        setAttributeValue(oSelect, "elastvalid", objMapp.value)
    }
}

var nsAdminAddin = nsAdminAddin || {};
nsAdminAddin.changeMapping = function (obj, key) {

    top.setWait(true);

    var value = obj.options[obj.selectedIndex].value
    var upd = new eUpdater("eda/Mgr/eAdminExtensionMappingManager.ashx", 1);

    upd.ErrorCallBack = function () { top.setWait(false); };


    var objMapp = {
        mappingid: "outlookAddin",
        action: "update",
        mappingflds: []
    }

    objMapp.mappingflds.push(
        {
            key: key,
            value: value
        }
    );

    upd.json = JSON.stringify(objMapp);

    upd.send(function (oRes) { nsAdminAddin.ProcessResult(oRes, obj, objMapp); });
}

nsAdminAddin.ProcessResult = function (oRes, oSelect, objMapp) {

    top.setWait(false);

    var result = JSON.parse(oRes)
    if (!result.Success) {
        //on remet la dernière valeur connue

        var lastValid = getAttributeValue(oSelect, "elastvalid");
        if (lastValid) {
            var myOpt = oSelect.querySelector("option[value='" + lastValid + "']")
            oSelect.selectedIndex = myOpt.index
        }


        var label = document.querySelector("div.DropDownMapping[key='" + objMapp.mappingflds[0].key + "'] > label").innerText;
        var val = oSelect.querySelector("option[value='" + objMapp.mappingflds[0].value + "']").innerText;



        var msg = result.ErrorMsg.replace("{RUB}", label).replace("{VAL}", val);




        top.eAlert(0, result.ErrorTitle, msg);
    }
    else {
        setAttributeValue(oSelect, "elastvalid", objMapp.mappingflds[0].value)
    }
}

var nsAdminLinkedin = nsAdminLinkedin || {};
nsAdminLinkedin.changeMapping = function (obj, key) {

    top.setWait(true);

    var value = obj.options[obj.selectedIndex].value
    var upd = new eUpdater("eda/Mgr/eAdminExtensionMappingManager.ashx", 1);

    upd.ErrorCallBack = function () { top.setWait(false); };


    var objMapp = {
        mappingid: "linkedincontact",
        action: "update",
        mappingflds: []
    }

    objMapp.mappingflds.push(
        {
            key: key,
            value: value
        }
    );

    upd.json = JSON.stringify(objMapp);

    upd.send(function (oRes) { nsAdminLinkedin.ProcessResult(oRes, obj, objMapp); });
}

nsAdminLinkedin.ProcessResult = function (oRes, oSelect, objMapp) {

    top.setWait(false);

    var result = JSON.parse(oRes)
    if (!result.Success) {
        //on remet la dernière valeur connue

        var lastValid = getAttributeValue(oSelect, "elastvalid");
        if (lastValid) {
            var myOpt = oSelect.querySelector("option[value='" + lastValid + "']")
            oSelect.selectedIndex = myOpt.index
        }
        else {
            var myOpt = oSelect.querySelector("option[value='-1']")
            oSelect.selectedIndex = myOpt.index
        }


        var label = document.querySelector("div.DropDownMapping[key='" + objMapp.mappingflds[0].key + "'] > label").innerText;
        var val = oSelect.querySelector("option[value='" + objMapp.mappingflds[0].value + "']").innerText;



        var msg = result.ErrorMsg.replace("{RUB}", label).replace("{VAL}", val);




        top.eAlert(0, result.ErrorTitle, msg);
    }
    else {
        setAttributeValue(oSelect, "elastvalid", objMapp.mappingflds[0].value)
    }
}

var nsAdminMobile = nsAdminMobile || {};

nsAdminMobile.currentExtensionModule = top.USROPT_MODULE_ADMIN_EXTENSIONS_MOBILE; // par défaut, on gère l'extension eudo touch / Eudonet Mobile

nsAdminMobile.interEventDescIds = new Array();

nsAdminMobile.fieldsToUpdate = 0;
nsAdminMobile.updatedFieldCount = 0;

nsAdminMobile.changeTab = function (obj) {

   
    var parentObj = obj.parentNode || obj.parentElement;
    parentObj = parentObj.parentNode || parentObj.parentElement;
     
    // Si on modifie une table, on masque les champs ne lui correspondant pas dans les listes de champs qui lui sont liées

    let sTabType = obj.id.replace("mobileTabListDdl_", "");
    let selecMainTab = obj.options[obj.selectedIndex].value * 1
    var childLists = parentObj.querySelectorAll("select[id^='mobileFieldListDdl_']");
    var refreshFields = new Array();
    for (var i = 0; i < childLists.length; i++) {
        // Démarrage à l'index 2 pour ne pas agir sur les libellés système <Sélectionner une rubrique> et --------
        childLists[i].options.selectedIndex = 0;
        for (var j = 2; j < childLists[i].options.length; j++) {


            let selectMainTabFld = childLists[i].options[j].value - childLists[i].options[j].value % 100;
            if (selecMainTab === selectMainTabFld ) {
                childLists[i].options[j].style.display = 'initial';
               
            }
            else {
                childLists[i].options[j].style.display = 'none';
          
            }
        }
    }

   
    top.setWait(true);
 
 
    var upd = new eUpdater("eda/Mgr/eAdminExtensionMobileManager.ashx", 1);
    upd.ErrorCallBack = function () { top.setWait(false); };
    upd.addParam("action", "changeTab", "post");
    upd.addParam("type", sTabType, "post");
    upd.addParam("tab", selecMainTab, "post");
 
    upd.send(nsAdminMobile.updateField);
 
};

nsAdminMobile.changeField = function (obj, bNoRefresh) {
   
    top.setWait(true);
    nsAdminMobile.fieldsToUpdate++;
    var parentObj = obj.parentNode || obj.parentElement;
    var upd = new eUpdater("eda/Mgr/eAdminExtensionMobileManager.ashx", 1);
    upd.ErrorCallBack = function () { top.setWait(false); };
    upd.addParam("action", "changeField", "post");
    upd.addParam("key", getAttributeValue(parentObj, "key"), "post");
    upd.addParam("tab", getAttributeValue(parentObj, "tab"), "post");
    upd.addParam("userid", getAttributeValue(parentObj, "userid"), "post");
    upd.addParam("descid", obj.options[obj.selectedIndex].value, "post");
    upd.addParam("field", getAttributeValue(parentObj, "field"), "post");
    upd.addParam("readonly", getAttributeValue(document.getElementById(parentObj.id + "_readOnly"), "chk"), "post");
    upd.addParam("customized", getAttributeValue(parentObj, "customized"), "post");
    upd.send(nsAdminMobile.updateField);
};

nsAdminMobile.updateField = function (oRes, callback) {

   

    // On masque le setWait() ajouté en début de MAJ
    top.setWait(false);
 

    var oResJSON = JSON.parse(oRes);


    var bNeedReload =  oResJSON.Success == false;


    if (bNeedReload) {

        if (!oResJSON.Success) {
            eAlert(0, oResJSON.ErrorTitle, oResJSON.ErrorMsg);
        }

        // #56 608 - On indique de recharger la page en réaffichant l'onglet Paramètres
        nsAdmin.loadAdminModule(nsAdminMobile.currentExtensionModule, null, { initialTab: "settings" });
    }
     

}

var nsAdminSirene = nsAdminSirene || {};

nsAdminSirene.interEventDescIds = new Array();

nsAdminSirene.fieldsToUpdate = 0;
nsAdminSirene.updatedFieldCount = 0;

nsAdminSirene.changeTab = function (obj) {
    var parentObj = obj.parentNode || obj.parentElement;
    parentObj = parentObj.parentNode || parentObj.parentElement;

    // Si on modifie la table Evènement, on rafraîchit les contrôles associés au fichier Standard/Invitation
    var refreshStandardTab = false;
    var childStandardList = document.getElementById('sireneTabListDdl_standard');
    if (obj.id == 'sireneTabListDdl_event') {
        // Démarrage à l'index 2 pour ne pas agir sur les libellés système <Sélectionner une rubrique> et --------
        for (var i = 2; i < childStandardList.options.length; i++) {
            if (
                nsAdminSirene.interEventDescIds[childStandardList.options[i].value] &&
                obj.options[obj.selectedIndex].value == nsAdminSirene.interEventDescIds[childStandardList.options[i].value]
            )
                childStandardList.options[i].style.display = 'initial';
            else {
                childStandardList.options[i].style.display = 'none';
                if (childStandardList.selectedIndex == i) {
                    childStandardList.selectedIndex = 0;
                    refreshStandardTab = true;
                }
            }
        }
    }

    // Si on modifie une table, on masque les champs ne lui correspondant pas dans les listes de champs qui lui sont liées
    var childLists = parentObj.querySelectorAll("select[id^='sireneFieldListDdl_']");
    var refreshFields = new Array();
    for (var i = 0; i < childLists.length; i++) {
        // Démarrage à l'index 2 pour ne pas agir sur les libellés système <Sélectionner une rubrique> et --------
        for (var j = 2; j < childLists[i].options.length; j++) {
            if (
                // Si aucune table n'est sélectionnée, on affiche tous les champs de toutes les tables, sauf pour les champs de Standard, dont la liaison de la table
                // doit impérativement être conditionnée à celle sélectionnée dans Evènements
                // Condition finalement non retenue pour permettre de remettre à zéro le mapping des champs
                //(obj.id != 'sireneTabListDdl_standard' && obj.selectedIndex < 2) ||
                childLists[i].options[j].text.indexOf(obj.options[obj.selectedIndex].text) == 0
            ) {
                childLists[i].options[j].style.display = 'initial';
                // On rétablit le champ initialement sélectionné lors du chargement initial du mapping
                if (getAttributeValue(childLists[i].options[j], "selected") == "selected")
                    childLists[i].selectedIndex = j;
            }
            else {
                childLists[i].options[j].style.display = 'none';
                // Réinitialisation de la rubrique si on la masque et mise en file d'attente de la MAJ en base
                if (childLists[i].selectedIndex == j) {
                    childLists[i].selectedIndex = 0;
                    refreshFields.push(childLists[i]);
                }
            }
        }
    }

    // Si on a masqué la table Standard actuellement sélectionnée, on déclenche son onchange() afin de rafraîchir les listes de champs liées,
    // qui déclencheront ensuite une MAJ en base
    if (refreshStandardTab)
        nsAdminSirene.changeTab(childStandardList);

    // Lorsqu'une table est changée et que les champs mappés liés sont modifiés, on effectue une MAJ en base des champs
    // mappés/démappés. Cela permet de réinitialiser le mapping des champs en cas de liaison et d'éviter des incohérences.
    // Pour éviter d'effectuer un refresh à chaque MAJ en base, on comptabilise le nombre de champs à mettre à jour, et 
    // on incrémente un compteur à chaque MAJ reçue. Lorsque la fonction de MAJ constate que le nombre de champs à mettre à
    // jour a été atteint, elle effectuera alors un seul et unique refresh global
    if (refreshFields.length > 0) {
        for (var i = 0; i < refreshFields.length; i++)
            nsAdminSirene.changeField(refreshFields[i]);
    }
};

nsAdminSirene.changeField = function (obj, refreshPageAfterUpdate) {
    top.setWait(true);
    nsAdminSirene.fieldsToUpdate++;
    var parentObj = obj.parentNode || obj.parentElement;
    var upd = new eUpdater("eda/Mgr/eAdminExtensionSireneManager.ashx", 1);
    upd.ErrorCallBack = function () { top.setWait(false); };
    upd.addParam("action", "changeField", "post");
    upd.addParam("key", getAttributeValue(parentObj, "key"), "post");
    upd.addParam("tab", getAttributeValue(parentObj, "tab"), "post");
    upd.addParam("userid", getAttributeValue(parentObj, "userid"), "post");
    upd.addParam("descid", obj.options[obj.selectedIndex].value, "post");
    upd.addParam("field", getAttributeValue(parentObj, "field"), "post");
    upd.addParam("readonly", getAttributeValue(document.getElementById(parentObj.id + "_readOnly"), "chk"), "post");
    upd.addParam("customized", getAttributeValue(parentObj, "customized"), "post");
    upd.send(nsAdminSirene.updateField);
};

nsAdminSirene.updateField = function (oRes, callback) {
    // On masque le setWait() ajouté en début de MAJ
    top.setWait(false);

    // Si on a envoyé toutes les MAJ en base de tous les champs que l'on devait modifier, on effectue un refresh global
    nsAdminSirene.updatedFieldCount++;
    if (nsAdminSirene.updatedFieldCount >= nsAdminSirene.fieldsToUpdate) {
        // #56 608 - On indique de recharger la page en réaffichant l'onglet Paramètres
        nsAdmin.loadAdminModule(USROPT_MODULE_ADMIN_EXTENSIONS_SIRENE, null, { initialTab: "settings" });
    }
}

nsAdmin.openThemeChoice = function (userid) {
    //alert('nsAdmin.openThemeChoice');

    //ouverture du menu de droite
    var openMenuTime = 0;
    var oRightMenu = document.getElementById("rightMenu");
    if (oRightMenu.className.indexOf("FavLnkOpen") == -1) {
        ocMenu(true, null);
        openMenuTime = 900;
    }

    //pin du menu de droite
    var pinMenuTime = 0;
    var oMenuBar = document.getElementById("menuBar");
    var menuPinned = getAttributeValue(oMenuBar, "pinned") == "1";
    if (!menuPinned) {
        setTimeout(function () { pinMenu(userid); }, openMenuTime);
        pinMenuTime = 150;
    }

    //highlight des choix de couleur
    setTimeout(function () { nsAdmin.highLightThemeChoiceLauncher(); }, openMenuTime + pinMenuTime);
}

nsAdmin.highLightThemeChoiceLauncher = function () {
    var blink = 0;
    var blinkMax = 2;

    setTimeout(function () { nsAdmin.highLightThemeChoice(null, true, blink, blinkMax); }, 0);
}

nsAdmin.highLightThemeChoice = function (elColorPick, highLight, blink, blinkMax) {
    var transitionTime = 200;

    if (elColorPick == null)
        elColorPick = document.getElementById("colorPick");

    if (elColorPick != null) {
        if (highLight) {
            ++blink;
            addClass(elColorPick, "highLightThemeChoice");

            setTimeout(function () { nsAdmin.highLightThemeChoice(elColorPick, !highLight, blink, blinkMax); }, transitionTime + 50);
        }
        else {
            removeClass(elColorPick, "highLightThemeChoice");

            if (blink < blinkMax) {
                setTimeout(function () { nsAdmin.highLightThemeChoice(elColorPick, !highLight, blink, blinkMax); }, transitionTime + 50);
            }
        }
    }
}

/////// Admin Propriétés RGPD sur les rubriques //////////////

nsAdmin.RGPDCustomEventDetail = nsAdmin.RGPDCustomEventDetail || { bSkipRefreshField: true };

nsAdmin.onChangeRGPDGenericAction = function (sender, event) {
    /*
    console.log("event.type: " + event.type);
    console.log("event.detail: " + event.detail);
    if (event.detail) {
        console.log("event.detail.bSkipRefreshField: " + event.detail.bSkipRefreshField);
    }
    */

    var bSkipRefreshField = false;
    if (event.detail != null && event.detail.bSkipRefreshField != null && event.detail.bSkipRefreshField == true)
        bSkipRefreshField = true;

    nsAdmin.onChangeGenericAction(sender, bSkipRefreshField);
}

nsAdmin.toggleRGPDPropertiesPanel = function (sender, natureDefaultValue, personalDefaultValue) {
    var selectedVal = sender.id.split("_");
    var isRPGDEnabled = selectedVal[1] == "1";

    var divPanelRGPD = document.getElementById("panelRGPDProperties");
    if (divPanelRGPD != null) {
        divPanelRGPD.style.display = isRPGDEnabled ? "block" : "none";
    }

    //todo
    //clear all properties
    if (!isRPGDEnabled) {
        //nature
        var divPanelNature = document.getElementById("panelRGPDNature");
        if (divPanelNature != null) {
            var inputsRadio = divPanelNature.querySelectorAll("input[name='rgpdNature']");
            if (inputsRadio != null) {
                var inputRadioPersonal = null;
                for (var i = 0; i < inputsRadio.length; ++i) {
                    inputsRadio[i].removeAttribute("checked");
                    inputsRadio[i].checked = false;

                    if (inputsRadio[i].id.split("_")[1] == natureDefaultValue)
                        inputRadioPersonal = inputsRadio[i];
                }

                if (inputRadioPersonal != null) {
                    inputRadioPersonal.setAttribute("checked", "checked");
                    inputRadioPersonal.checked = true;
                    inputRadioPersonal.dispatchEvent(new CustomEvent("change", { detail: nsAdmin.RGPDCustomEventDetail }));
                }
            }
        }

        //category
        var divPanelPersonal = document.getElementById("panelRGPDPersonalCategory");
        if (divPanelPersonal != null) {
            var selectCategory = divPanelPersonal.querySelector("select");
            if (selectCategory != null && selectCategory.value != personalDefaultValue) {
                selectCategory.value = personalDefaultValue;
                selectCategory.dispatchEvent(new CustomEvent("change", { detail: nsAdmin.RGPDCustomEventDetail }));
            }
        }

        //finalité
        var divPanelDataPurpose = document.getElementById("panelRGPDDataPurpose");
        if (divPanelDataPurpose != null) {
            var inputDataPurpose = divPanelDataPurpose.querySelector("input");
            if (inputDataPurpose != null && inputDataPurpose.value != "") {
                inputDataPurpose.value = "";
                inputDataPurpose.dispatchEvent(new CustomEvent("change", { detail: nsAdmin.RGPDCustomEventDetail }));
            }
        }

        //pseudo
        var divPanelPseudo = document.getElementById("panelRGPDPseudoEnabled");
        if (divPanelPseudo != null) {
            var inputsRadio = divPanelPseudo.querySelectorAll("input[name='rgpdPseudoEnabled']");
            if (inputsRadio != null) {
                var inputRadioNo = null;
                for (var i = 0; i < inputsRadio.length; ++i) {
                    inputsRadio[i].removeAttribute("checked");
                    inputsRadio[i].checked = false;

                    if (inputsRadio[i].id.split("_")[1] == "0")
                        inputRadioNo = inputsRadio[i];
                }

                if (inputRadioNo != null) {
                    inputRadioNo.setAttribute("checked", "checked");
                    inputRadioNo.checked = true;
                    inputRadioNo.dispatchEvent(new CustomEvent("change", { detail: nsAdmin.RGPDCustomEventDetail }));
                }
            }
        }

        //users
        var divPanelResponsibles = document.getElementById("panelRGPDResponsibles");
        if (divPanelResponsibles != null) {
            var selectResponsibles = divPanelResponsibles.querySelectorAll("select");
            if (selectResponsibles != null) {
                for (var i = 0; i < selectResponsibles.length; ++i) {
                    selectResponsibles[i].value = "";
                    selectResponsibles[i].dispatchEvent(new CustomEvent("change", { detail: nsAdmin.RGPDCustomEventDetail }));
                }
            }
        }
    }
}

nsAdmin.toggleRGPDNature = function (sender, personalValue, sensitiveValue, personalUnspecifiedValue, personalOtherValue, sensitiveUnspecifiedValue, sensitiveOtherValue) {
    var selectedVal = sender.id.split("_");
    var value = selectedVal[1];

    var divPanelPersonal = document.getElementById("panelRGPDPersonalCategory");
    if (divPanelPersonal != null) {
        divPanelPersonal.style.display = value == personalValue ? "block" : "none";
    }

    var divPanelSensitive = document.getElementById("panelRGPDSensitiveCategory");
    if (divPanelSensitive != null) {
        divPanelSensitive.style.display = value == sensitiveValue ? "block" : "none";
    }

    var selectCategory = null;
    var unspecifiedValue = null;
    var otherValue = null;
    var selectCategoryToClear = null;
    var unspecifiedValueToClear = null;
    if (value == personalValue && divPanelPersonal != null) {
        selectCategory = divPanelPersonal.querySelector("select");
        unspecifiedValue = personalUnspecifiedValue;
        otherValue = personalOtherValue;

        if (divPanelSensitive != null) {
            selectCategoryToClear = divPanelSensitive.querySelector("select");
            unspecifiedValueToClear = sensitiveUnspecifiedValue;
        }
    }
    else if (value == sensitiveValue && divPanelSensitive != null) {
        selectCategory = divPanelSensitive.querySelector("select");
        unspecifiedValue = sensitiveUnspecifiedValue;
        otherValue = sensitiveOtherValue;

        if (divPanelPersonal != null) {
            selectCategoryToClear = divPanelPersonal.querySelector("select");
            unspecifiedValueToClear = personalUnspecifiedValue;
        }
    }

    if (selectCategory != null)
        nsAdmin.toggleRGPDCategory(selectCategory, otherValue);

    if (selectCategoryToClear != null && selectCategoryToClear.value != unspecifiedValueToClear) {
        selectCategoryToClear.value = unspecifiedValueToClear;
        selectCategoryToClear.dispatchEvent(new CustomEvent("change", { detail: nsAdmin.RGPDCustomEventDetail }));
    }
}

nsAdmin.toggleRGPDCategory = function (sender, otherValue) {
    var otherValueSelected = sender.value == otherValue;

    var divPanelCategoryOther = document.getElementById("panelRGPDCategoryOther");
    if (divPanelCategoryOther != null) {
        divPanelCategoryOther.style.display = otherValueSelected ? "block" : "none";

        if (!otherValueSelected) {
            var inputCategoryOther = divPanelCategoryOther.querySelector("input");
            if (inputCategoryOther != null && inputCategoryOther.value != "") {
                inputCategoryOther.value = "";
                inputCategoryOther.dispatchEvent(new CustomEvent("change", { detail: nsAdmin.RGPDCustomEventDetail }));
            }
        }
    }
}

nsAdmin.toggleRGPDPseudoRulesPanel = function (sender) {
    var selectedVal = sender.id.split("_");
    var isPseudo = selectedVal[1] == "1";

    var divPanelPseudoRules = document.getElementById("panelRGPDPseudoRules");
    if (divPanelPseudoRules != null) {
        divPanelPseudoRules.style.display = isPseudo ? "block" : "none";

        //vider champ règles
        if (!isPseudo) {
            var selectPseudoRules = divPanelPseudoRules.querySelector("select");
            if (selectPseudoRules != null && selectPseudoRules.value != "") {
                selectPseudoRules.value = "";
                selectPseudoRules.dispatchEvent(new CustomEvent("change", { detail: nsAdmin.RGPDCustomEventDetail }));
            }
        }

    }
}

nsAdmin.toggleRGPDPseudoReplaceValuePanel = function (sender, replaceValue) {
    var replaceValueSelected = sender.value == replaceValue;

    var divPanelPseudoReplaceValue = document.getElementById("panelRGPDPseudoReplaceValue");
    if (divPanelPseudoReplaceValue != null) {
        divPanelPseudoReplaceValue.style.display = replaceValueSelected ? "block" : "none";

        if (!replaceValueSelected) {
            var inputPseudoReplaceValue = divPanelPseudoReplaceValue.querySelector("input");
            if (inputPseudoReplaceValue != null && inputPseudoReplaceValue.value != "") {
                inputPseudoReplaceValue.value = "";
                inputPseudoReplaceValue.dispatchEvent(new CustomEvent("change", { detail: nsAdmin.RGPDCustomEventDetail }));
            }
        }
    }
}

nsAdmin.toggleRGPDResponsibleOtherPanel = function (sender, otherValue) {
    var otherValueSelected = false;

    var divPanelResponsibles = document.getElementById("panelRGPDResponsibles");
    if (divPanelResponsibles != null) {
        var selects = divPanelResponsibles.querySelectorAll("select");
        if (selects != null) {
            for (var i = 0; i < selects.length; ++i) {
                if (selects[i].value == otherValue) {
                    otherValueSelected = true;
                    break;
                }
            }
        }
    }

    var divPanelResponsibleOther = document.getElementById("panelRGPDResponsibleOther");
    if (divPanelResponsibleOther != null) {
        divPanelResponsibleOther.style.display = otherValueSelected ? "block" : "none";

        //vider champ autre
        if (!otherValueSelected) {
            var inputResponsibleOther = divPanelResponsibleOther.querySelector("input");
            if (inputResponsibleOther != null && inputResponsibleOther.value != "") {
                inputResponsibleOther.value = "";
                inputResponsibleOther.dispatchEvent(new CustomEvent("change", { detail: nsAdmin.RGPDCustomEventDetail }));
            }
        }
    }
}

var modalFieldsRGPDList;
nsAdmin.openRGPDReport = function () {
    modalFieldsRGPDList = new eModalDialog(top._res_8323, 0, "eda/eAdminFieldsRGPDListDialog.aspx", 985, 555, "modalFieldsRGPDList");

    modalFieldsRGPDList.noButtons = false;
    modalFieldsRGPDList.addParam("tab", nsAdmin.tab, "post");
    modalFieldsRGPDList.show();

    modalFieldsRGPDList.addButton(top._res_30, function () { modalFieldsRGPDList.hide(); nsAdmin.loadContent(nsAdmin.tab); }, 'button-green', null);
    modalFieldsRGPDList.addButton(top._res_16, function () { nsAdmin.exportRGPDReport() }, 'button-gray', null);
    modalFieldsRGPDList.addButton(top._res_6190, function () { modalFieldsRGPDList.getIframe().print(); }, 'button-gray', null);

    nsAdmin.modalResizeAndMove(modalFieldsRGPDList);
}

nsAdmin.exportRGPDReport = function () {

    var baseurl = "mgr/eAdminFieldsRGPDExportList.ashx";
    var nTab = modalFieldsRGPDList.getIframe().document.getElementById('ddlTabsList');


    var form = CreateElement('form', ["method", "action", "style", "target"], ["post", baseurl, "display:hidden;", "_blank"]);
    var format = CreateElement("input", ["type", "name", "value"], ["hidden", "nTab", nTab.value]);
    form.appendChild(format);
    modalFieldsRGPDList.getIframe().document.body.appendChild(form);
    form.submit();
    modalFieldsRGPDList.getIframe().document.body.removeChild(form);

}

/////// Fin Admin Propriétés RGPD sur les rubriques //////////////


/////// Admin Extension Emailing Externe //////////////

var nsAdminExternalEmailing = nsAdminExternalEmailing || {};

nsAdminExternalEmailing.ActionAdd = "add";
nsAdminExternalEmailing.ActionUpdate = "update";
nsAdminExternalEmailing.ActionDelete = "delete";

nsAdminExternalEmailing.AddServerAliasDomain = function (sender) {
    nsAdminExternalEmailing.CRUDServerAliasDomain(nsAdminExternalEmailing.ActionAdd);
}

nsAdminExternalEmailing.UpdateServerAliasDomain = function (sender, parameter) {
    nsAdminExternalEmailing.CRUDServerAliasDomain(nsAdminExternalEmailing.ActionUpdate, sender, parameter);
}

nsAdminExternalEmailing.DeleteServerAliasDomain = function (sender, parameter) {
    nsAdminExternalEmailing.CRUDServerAliasDomain(nsAdminExternalEmailing.ActionDelete, sender, parameter);
}

nsAdminExternalEmailing.CRUDServerAliasDomain = function (action, sender, parameter) {
    if (action == null || action == "")
        return;

    if (action == nsAdminExternalEmailing.ActionUpdate && (sender == null || parameter == null || parameter == ""))
        return;

    if (action == nsAdminExternalEmailing.ActionDelete && (parameter == null || parameter == ""))
        return;

    var upd = new eUpdater("eda/Mgr/eAdminExtensionExternalEmailingManager.ashx", 1);
    upd.addParam("category", "senderAliasDomain", "post");
    upd.addParam("action", action, "post");
    if (action == nsAdminExternalEmailing.ActionDelete || action == nsAdminExternalEmailing.ActionUpdate)
        upd.addParam("parameter", parameter, "post");
    if (action == nsAdminExternalEmailing.ActionUpdate)
        upd.addParam("value", sender.value, "post");
    upd.send(function (oRes) { nsAdminExternalEmailing.ProcessResult(oRes, action, sender, parameter); });
}

nsAdminExternalEmailing.ProcessResult = function (oRes, action, sender, parameter) {
    if (oRes == null)
        return;

    oRes = JSON.parse(oRes);

    if (oRes.Success == true) {
        if (action == nsAdminExternalEmailing.ActionUpdate) {
            if (sender != null)
                sender.setAttribute("value", oRes.Value);
        }
        else if (action == nsAdminExternalEmailing.ActionAdd && oRes.SenderAliasDomainPanel != "") {
            var elContainer = document.getElementById("senderAliasDomainSubContainer");
            if (elContainer != null) {
                elContainer.innerHTML += oRes.SenderAliasDomainPanel;
            }
        }
        else if (action == nsAdminExternalEmailing.ActionDelete) {
            var elContainer = document.getElementById("senderAliasDomainSubContainer");
            var elChild = document.getElementById(parameter + "_LineContainer");

            if (elContainer != null && elChild != null) {
                elContainer.removeChild(elChild);
            }
        }
    }
}

/////// Fin Admin Extension Emailing Externe //////////////


/////// Admin Extension Synchro Exchange E2017 //////////////
var nsAdminSynchroExchange = nsAdminSynchroExchange || {};



// Affichage d'un catalogue utilisateur 
nsAdminSynchroExchange.showUsersCat = function (obj) {

    var chkEnable = document.getElementById("chkActiveSync");
    if (getAttributeValue(chkEnable, "chk") != "1") {
        eAlert(0, top._res_8531, top._res_8541);
        return;
    }


    var modalUserCat = new eModalDialog(top._res_246, 0, "eCatalogDialogUser.aspx", 550, 610, "modalSyncUsers");
    modalUserCat.addParam("iframeId", modalUserCat.iframeId, "post");
    modalUserCat.ErrorCallBack = function () { setWait(false); }
    modalUserCat.addParam("multi", "1", "post");
    modalUserCat.addParam("showallusersoption", "1", "post");

    modalUserCat.addParam("selected", getAttributeValue(document.getElementById("txtSyncUsers"), "dbv"), "post");

    modalUserCat.show();

    modalUserCat.addButton(top._res_29, function () {
        modalUserCat.hide();
    }, "button-gray", null, null, true);
    modalUserCat.addButton(top._res_28, function () { nsAdminSynchroExchange.onUsersCatOk(obj); }, "button-green");
}

nsAdminSynchroExchange.onUsersCatOk = function (obj) {

    var modal = eTools.GetModal("modalSyncUsers");
    if (modal) {
        var strReturned = modal.getIframe().GetReturnValue();
        var vals = strReturned.split('$|$')[0];
        var libs = strReturned.split('$|$')[1];
        var userToDeactivate = [];
        var userToActivate = [];

        var oTarget = document.getElementById("txtSyncUsers");
        var oldVals = getAttributeValue(oTarget, "dbv");

        if (oTarget.innerText)
            oTarget.innerText = libs;
        else
            oTarget.textContent = libs;

        oTarget.setAttribute("title", libs);
        oTarget.value = libs;
        oTarget.setAttribute("dbv", vals);

        oTarget.onchange();

        var scrollTop = 0;
        var scrollElt = document.querySelector("#extensionCnt_settings div");
        if (scrollElt) {
            scrollTop = scrollElt.scrollTop;
        }

        //On fait un diff entre les deux listes pour avoir les utilisateurs a activer et desactiver et on appel le manager
        vals.split(';').forEach(function (element) {
            if (!oldVals.split(';').includes(element)) {
                userToActivate.push(element);
            }
        });
        oldVals.split(';').forEach(function (element) {
            if (!vals.split(';').includes(element)) {
                userToDeactivate.push(element);
            }
        });

        top.setWait(true);
        var upd = new eUpdater("eda/Mgr/eAdminExtensionSynchroExchangeManager.ashx", 1);
        upd.addParam("userListToActivate", userToActivate.join(), "post");
        upd.addParam("userListToDeactivate", userToDeactivate.join(), "post");
        upd.send(function (oRes) { nsAdminSynchroExchange.ProcessResult(oRes); });

    }
}

nsAdminSynchroExchange.ProcessResult = function (oRes) {
    top.setWait(false);
    if (oRes == null)
        return;

    oRes = JSON.parse(oRes);

    //Au retour de l'activation, on recharge la page
    nsAdmin.loadAdminModule(USROPT_MODULE_ADMIN_EXTENSIONS_SYNCHRO_OFFICE365, "", { initialTab: "settings" });

    var modal = eTools.GetModal("modalSyncUsers");
    if (modal) {
        modal.hide();
    }

    if (oRes.Success == false) {
        eAlert(0, top._res_8536, oRes.ErrorMsg);
    }
}
// Ouverture de la popup pour configurer le mapping de la synchro
nsAdminSynchroExchange.confMapping = function (module) {

    var tab = "0";
    var ddl = document.getElementById("ddlSyncOffice365MappingTab");
    if (ddl) {
        tab = ddl.value;
    }

    if (tab == "0")
        eAlert(0, top._res_8536, top._res_8540);

    var modal = new eModalDialog(
        module == USROPT_MODULE_ADMIN_EXTENSIONS_SYNCHRO_EXCHANGE2016ONPREMISE ? top._res_2180 : top._res_8526,
        0, "eda/eAdminSynchroExchangeMappingDialog.aspx", 600, 280, "modalSyncExchMapping");
    modal.addParam("tab", tab, "post");
    modal.noButtons = false;
    modal.show();

    modal.addButton(top._res_29, function () { modal.hide(); }, 'button-gray', null);
    modal.addButton(top._res_28, function () { nsAdminSynchroExchange.updateMapping(tab); }, 'button-green', null);
}

// Mise à jour du mapping dans CONFIGADV
nsAdminSynchroExchange.updateMapping = function (tab) {

    var modal = eTools.GetModal("modalSyncExchMapping");
    var doc = modal.getIframe().document;

    var obj = {
        "OrganizerDescid": doc.getElementById("ddlORGANIZER").value,
        "AttendeesDescid": doc.getElementById("ddlATTENDEES").value,
        "StartDescid": doc.getElementById("ddlDATE_START").value,
        "EndDescid": doc.getElementById("ddlDATE_END").value,
        "SubjectDescid": doc.getElementById("ddlSUBJECT").value,
        "LocationDescid": doc.getElementById("ddlLOCATION").value,
        "BodyDescid": doc.getElementById("ddlDESCRIPTION").value,
        "SensitivityDescid": doc.getElementById("ddlCONFIDENTIAL").value,
    };

    var capsule = new Capsule(tab);
    var desc = getAttributeValue(doc.getElementById("hidMapping"), "dsc");
    if (desc != "") {
        var aDesc = desc.split("|");
        if (aDesc.length < 2)
            return;
        var value = JSON.stringify(obj);
        capsule.AddProperty(aDesc[0], aDesc[1], value, aDesc[2]);
    }
    else
        return;

    upd = new eUpdater("eda/Mgr/eAdminConfigAdvManager.ashx", 1);

    var json = JSON.stringify(capsule);
    upd.json = json;

    upd.ErrorCallBack = function () {
        //alert('Erreur');
        setWait(false);
    };
    setWait(true);
    upd.send(function (oRes) {
        var res = JSON.parse(oRes);

        if (!res.Success) {
            top.eAlert(1, top._res_416, res.UserErrorMessage);
        }
        else {
            modal.hide();
        }
        setWait(false);
    });
}

/*****************************/

var nsAdminResCode = nsAdminResCode || {};

nsAdminResCode.createNewResCode = function (value, sJsonLocation, callbackFct) {

    var upd = new eUpdater("eda/Mgr/eResCodeManager.ashx", 1);

    upd.addParam("action", 1, "post");
    upd.addParam("location", sJsonLocation, "post");
    upd.addParam("value", value, "post");

    upd.ErrorCallBack = function () {
        //alert('Erreur');
        setWait(false);
    };
    upd.send(callbackFct);
}

nsAdminResCode.updateResCode = function (code, value, callbackFct) {

    if (!code)
        return;

    var upd = new eUpdater("eda/Mgr/eResCodeManager.ashx", 1);

    upd.addParam("action", 2, "post");
    upd.addParam("code", code, "post");
    upd.addParam("value", value, "post");

    upd.ErrorCallBack = function () {
        //alert('Erreur');
        setWait(false);
    };
    upd.send(callbackFct);
}


var nsAdminUpgradeInProcess = {};

nsAdminUpgradeInProcess.URL = "eda/mgr/eAdminUpgradeInProgressManager.ashx";
nsAdminUpgradeInProcess.Action = {
    UNDEFINED: 0,
    ENABLE: 1,
    DISABLE: 2
};

nsAdminUpgradeInProcess.enable = function (bImmediate) {
    var upd = new eUpdater(nsAdminUpgradeInProcess.URL, 1);
    upd.addParam("action", nsAdminUpgradeInProcess.Action.ENABLE, "post");
    upd.addParam("immediate", bImmediate ? "1" : "0", "post");
    upd.send();
};

nsAdminUpgradeInProcess.disable = function () {
    var upd = new eUpdater(nsAdminUpgradeInProcess.URL, 1);
    upd.addParam("action", nsAdminUpgradeInProcess.Action.DISABLE, "post");
    upd.send();
};


/*****************************/
var nsAdminStoreMenu = {};

nsAdminStoreMenu.ResetSelectedFilters = function () {
    nsAdminStoreList.currentFilterCategory = "";
    nsAdminStoreList.currentFilterOffers = "";
    nsAdminStoreList.currentFilterOther = "";
    nsAdminStoreList.currentFilterDisplay = "";
}

nsAdminStoreMenu.InitSelectedFilters = function () {
    var menuGerard = document.getElementById("Gerard");
    if (menuGerard) {
        var selectCategory = menuGerard.querySelector("select[data-category]");
        if (selectCategory != null) {
            if (nsAdminStoreList.currentFilterCategory != "")
                selectCategory.value = nsAdminStoreList.currentFilterCategory;
            else
                selectCategory.value = "all";
        }

        var inputsOffers = menuGerard.querySelectorAll("input[data-offer]");
        var arrayOffers = nsAdminStoreList.currentFilterOffers.split(";");
        for (var i = 0; i < inputsOffers.length; ++i) {
            var currentOffer = inputsOffers[i].getAttribute("data-offer");
            if (currentOffer != "" && arrayOffers.indexOf(currentOffer) > -1)
                inputsOffers[i].checked = true;
        }

        var inputsOther = menuGerard.querySelectorAll("input[data-otherfilter]");
        var arrayOther = nsAdminStoreList.currentFilterOther.split(";");
        for (var i = 0; i < inputsOther.length; ++i) {
            var currentOther = inputsOther[i].getAttribute("data-otherfilter");
            if (currentOther != "" && arrayOther.indexOf(currentOther) > -1)
                inputsOther[i].checked = true;
        }

        var inputsStatus = menuGerard.querySelectorAll("input[data-status]");
        for (var i = 0; i < inputsStatus.length; ++i) {
            var currentStatus = inputsStatus[i].getAttribute("data-status")
            if ((currentStatus != "" && currentStatus == nsAdminStoreList.currentFilterDisplay)
                || (nsAdminStoreList.currentFilterDisplay == "" && currentStatus == "0"))
                inputsStatus[i].checked = true;
            else
                inputsStatus[i].checked = false;
        }
    }
}

nsAdminStoreMenu.initFilterCategory = function () {
    var menuGerard = document.getElementById("Gerard");
    if (menuGerard)
        nsAdmin.initFilterCategory(menuGerard, nsAdminStoreMenu.onChangeCategory);
};

nsAdminStoreMenu.onChangeCategory = function (sender) {
    // On transmet l'info à notre objet local
    nsAdminStoreList.currentFilterCategory = sender.value;

    nsAdmin.FilterChange();
};

nsAdminStoreMenu.onCheckOffer = function (sender) {
    var checkedOffers = [];
    var menuGerard = document.getElementById("Gerard");
    if (menuGerard) {
        var inputs = menuGerard.querySelectorAll("input[data-offer]");
        for (var i = 0; i < inputs.length; ++i) {
            if (inputs[i].checked)
                checkedOffers.push(inputs[i].getAttribute("data-offer"));
        }
    }

    // On transmet l'info à notre objet local
    nsAdminStoreList.currentFilterOffers = checkedOffers.join(';');

    nsAdmin.FilterChange();
};

nsAdminStoreMenu.onCheckOtherFilter = function (sender) {
    var checkedOtherFilters = [];
    var menuGerard = document.getElementById("Gerard");
    if (menuGerard) {
        var inputs = menuGerard.querySelectorAll("input[data-otherFilter]");

        for (var i = 0; i < inputs.length; ++i) {
            if (inputs[i].checked)
                checkedOtherFilters.push(inputs[i].getAttribute("data-otherFilter"));
        }
    }

    // On transmet l'info à notre objet local
    nsAdminStoreList.currentFilterOther = checkedOtherFilters.join(';');

    nsAdmin.FilterChange();
};

nsAdminStoreMenu.onChangeStatus = function (sender) {
    // On transmet l'info à notre objet local
    nsAdminStoreList.currentFilterDisplay = sender.getAttribute("data-status");

    nsAdmin.FilterChange();
};
