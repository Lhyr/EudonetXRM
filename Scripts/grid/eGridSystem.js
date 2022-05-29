



///**********************************************************
/// <summary>
/// Objet permettant de lancer le système de grille
/// chargement des fichiers css et js
/// initialisation du gestionnaire des grilles
/// </summary> 
///**********************************************************
var GridSystem = (function () {

    /// une seule instance sur toute l'application
    if (top && top.GridSystem)
        return top.GridSystem;

    /// <summary>
    /// Mode d'utilisation de l'application
    /// AdminMode ou UserMode
    /// </summary> 
    var _isAdminMode = false;

    /// <summary>
    /// Les fichiers js des grilles et widgets sont complètement chargés
    /// </summary> 
    var _isReady = false;

    /// <summary>
    /// fonction de Callback une fois les fichiers js sont chargés
    /// </summary> 
    var _onReady = function () { };

    /// <summary>
    /// Objet permettant le chargement/déchargement des fichiers JS nécessaires
    /// </summary> 
    var _scriptManager = {

        /// <summary>
        /// Liste des fichiers JS nécessaires
        /// ajouter de nouveaux dans le tableau
        /// </summary> 
        'scripts': [
                "grid/eGridHelpers",
                "grid/eGridEnum",
                "grid/eGridController",
                "grid/eGridLayout",
                "grid/eGridToolbar",
                "grid/eGrid",
                "grid/eGridManager",
                "grid/eWidgetToolbar",
                "grid/eWidgetViews",
                "grid/eWidget",
                "grid/widget/eWidgetKanban",
                "grid/widget/eWidgetCartoSelection"
        ],

        /// <summary>
        /// Chargement des fichiers
        /// </summary> 
        'load': function () {
            addScripts(this.scripts, "GRID", run);
        },

        /// <summary>
        /// Suppression des fichiers
        /// </summary> 
        'clearHeader': function () {
            clearHeader("GRID", "JS");
        }
    }

    /// <summary>
    /// Objet permettant le chargement/déchargement des fichiers CSS
    /// </summary> 
    var _cssManager = {

        /// <summary>
        /// Chargement des fichiers CSS
        /// </summary> 
        'load': function () {
            addCss("eXrmHomePage", "GRID");
            addCss("eGrid", "GRID");
        },

        /// <summary>
        /// Suppression des fichiers JS
        /// </summary>
        'clearHeader': function () {
            clearHeader("GRID", "CSS");
        }
    }

    /// <summary>
    /// Execute le callback après chargement des fichiers js
    /// </summary>
    function run() {

        try {
            _isReady = true;
            
            if (typeof (_onReady) == "function")
                _onReady();
            
            oGridManager.init();
        } catch (e) {

            _isReady = false;
            console.log(e);
        }
    };

    return {

        /// <summary>
        /// Savoir si tous les scripts ont été chargés
        /// </summary> 
        'isReady': function () { return _isReady; },

        /// <summary>
        /// Recoit la fonction du callback
        /// </summary> 
        'onReady': function (onReady) {
            _onReady = onReady;

            return this;
        },

        /// <summary>
        /// Lance le chargement des fichiers css et des fichiers js
        /// </summary> 
        'start': function () {
            _cssManager.load();
            _scriptManager.load();
        },

        /// <summary>
        /// Retire les fichiers css et des fichiers js du DOM
        /// </summary> 
        'stop': function () {

            oGridManager.dispose();

            _scriptManager.clearHeader();
            _cssManager.clearHeader();

            _isReady = false;
            _onReady = function () { };
        },

        /// <summary>
        /// On réinitialise le DOM des grilles/widgets
        /// </summary>
        'reset': function () {
            oGridManager.reset();
        },

        /// <summary>
        /// On switche de adminMode en userMode
        /// </summary>
        'userMode': function () {


            _isAdminMode = false;
            this.reset();
        },


        /// <summary>
        /// On switche de userMode en adminMode
        /// </summary>
        'adminMode': function () {
            _isAdminMode = true;
            this.reset();
        },

        'isAdminMode': function () { return _isAdminMode; },
        /// <summary>
        /// namespace contient des fonctions de création des objets pour les modes : UserMode ou AdminMode
        /// </summary> 
        'factory': {


            /// <summary>
            /// Crée la barre d'outils en fonction de mode : UserMode ou AdminMode
            /// En adminMode il y a la gestion des actions de paramétrage/suppression, etc.
            /// </summary> 
            'createWidgetToolbar': function (element) {

                if (!_isReady)
                    throw "Les scripts des grilles ne sont pas chargés !";

                var toolbar = new eWidgetToolbar(element);
                if (_isAdminMode && typeof (eAdminWidgetToolbar) != 'undefined')
                    return new eAdminWidgetToolbar(toolbar);

                return toolbar;
            },

            /// <summary>
            /// Retourne la liste des widget de bibliothèque en mode admin uniqumenet
            /// </summary> 
            'widget': {
                'prototype': function () {

                    if (!_isReady) {
                        console.log("Les scripts des grilles ne sont pas chargés !");
                        return null;// vide
                    }

                    if (!_isAdminMode)
                        return null;// vide

                    var libContainer = document.getElementById("menu-widget-ctn");
                    if (!libContainer) {
                        console.log("La bibliothèque des widgets n'est pas disponible !");
                        return null;// vide
                    }

                    return new eAdminWidgetPrototype(libContainer);
                }
            }
        }
    }
})();


/// Objet de
var oDicGridRefreshDates = {};
var eGridServer = function (eGridContext) {
    var REQ_PART_XRM_HOMEPAGE = 10;
    var REQ_PART_XRM_HOMEPAGE_GRID = 11;
    var MODIF_MODE = 2;
    var upd;
    var gridId = eGridContext.gridId;
    function init() {

        // si la taille n'est pas fourni on la calcule
        if (typeof (eGridContext.width) == "undefined" || eGridContext.width == 0 || isNaN(eGridContext.width) ||
            typeof (eGridContext.height) == "undefined" || eGridContext.height == 0 || isNaN(eGridContext.height)) {

            // Pour le signet, on a la meme largeur mais pas la meme hauteur avec l'onglet
            var tabMainDivWH = GetMainDivWH();
            if (eGridContext.type == "bkm") {
                var bkm = eTools.getBookmarkContainer(eGridContext.tab);
                var h;
                if (bkm) {
                    h = (parseInt(bkm.style.height) - 4);
                    tabMainDivWH[0] -= 15;
                }

                if (!bkm || isNaN(h))
                {
                    bkm = document.getElementById("divBkmPres");
                    if (bkm)
                        h = (parseInt(bkm.style.height) - 70);

                    if (isNaN(h))
                        h = 450;//minimum
                }

                tabMainDivWH[1] = h;
            }

            if (typeof (eGridContext.height) == "undefined" || eGridContext.height == 0 || isNaN(eGridContext.height))
                eGridContext.height = tabMainDivWH[1];

            if (typeof (eGridContext.width) == "undefined" || eGridContext.width == 0 || isNaN(eGridContext.width))
                eGridContext.width = tabMainDivWH[0];
        }
        upd = new eUpdater("mgr/eFileAsyncManager.ashx", 1);
        upd.addParam("tab", eGridContext.tab, "post");
        upd.addParam("fileid", eGridContext.gridId, "post");
        upd.addParam("parenttab", eGridContext.parentTab, "post");
        upd.addParam("parentfid", eGridContext.parentFileId, "post");
        upd.addParam("part", REQ_PART_XRM_HOMEPAGE_GRID, "post");
        upd.addParam("type", MODIF_MODE, "post");//
        upd.addParam("height", eGridContext.height, "post");
        upd.addParam("width", eGridContext.width, "post");
    

        upd.addParam("callby", "eGridManager", "post");
        upd.addParam("callbyarguments", "no-args", "post");
    }

    init();
    return {
        'setParam': function (key, value) {
            upd.addParam(key, value, "post");
            return this;
        },
        'load': function (callback) {
            setWait(true);
            upd.send(function (oRes) {
                setWait(false);
                callback(oRes);
            });
            //On enregriste la date/heure actuelle de mise à jour
            oDicGridRefreshDates[gridId] = new Date();
        }

    }
}

