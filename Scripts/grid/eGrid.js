
///
/// Gestionnaires des grilles sur toutes les tables
///
var eGrid = function (eGridContext) {

    var _masterContainer;
    var _gridContainer;
    var _context;
    var _gridLayout;
    var _started = false;

    // Constructeur de l'objet
    // Ajoute les div manquants au DOM
    function init() {

        _context = eGridContext;
        if (typeof (_context.width) == "undefined" || typeof (_context.height) == "undefined") {
            _context.width = 0; // sera claculé dynamiquement au moment d'appel serveur 
            _context.height = 0; // seraclaculé dynamiquement au moment d'appel serveur
        }

        // container des tables
        _masterContainer = document.getElementById("gw-container");
        var font = document.getElementById("container").classList[1];

        if (_masterContainer == null) {

            _masterContainer = document.createElement("div");
            //_masterContainer.id = "gw-container2";
            document.body.appendChild(_masterContainer);
        }
        _masterContainer.className = font;

        // container des grilles
        var gridtabId = "gw-" + _context.type + "-" + _context.tab + "-" + _context.fileId;

        _gridContainer = document.getElementById(gridtabId);
        if (_gridContainer == null) {
            _gridContainer = document.createElement("div");
            _gridContainer.id = gridtabId;
            _gridContainer.className = "gw-tab";

            updateParentLink();

            _masterContainer.appendChild(_gridContainer);
        }
    }

    function updateParentLink() {
        if (_context.type == "bkm" && _context.parentTab > 0 && _context.parentTab != 115200) {
            setAttributeValue(_gridContainer, "parent-tab", _context.parentTab);
            setAttributeValue(_gridContainer, "parent-file", _context.parentFileId);
        }
    }

    // mis à jour la grille
    function setGrid(container) {
        append(container.innerHTML);
    }

    function load() {

        var grid = document.getElementById("gw-grid-" + _context.gridId);
        if (grid != null) {
            // si la grille est déjà dans le DOM, on initialise les objets
            start();

            _started = true;

        } else {

            _started = false;

            // sinon on la charge depuis la base
            eGridServer(_context).load(append);
        }
    }

    // Ajoute la grille au DOM
    function append(html) {
        if (html) {

            var div = document.createElement("div");
            div.innerHTML = html;

            var grid = document.createElement("div");
            grid.id = "gw-grid-" + _context.gridId;
            grid.className = "gw-grid";
            grid.innerHTML = html;

            setAttributeValue(grid, "active", '1');

            //SHA
            if (_gridContainer != null) {
                init();
                _gridContainer.appendChild(grid);
            }
            start();
        }
    }

    function start() {

        // Mise à jour de la liaison parente
        updateParentLink();

        // masque la grille précedente, pour la coéh
        resetVisibility();

        // Créer un objet qui gère la disposition des widgets sur la grille
        if (_gridLayout == null)
            _gridLayout = new eGridLayout(_context);
        else {
            // _gridLayout.resize();
            _gridLayout.autoRefresh();
        }

        // Affiche les widgets sur la grille
        _gridLayout.show();

        _started = true;
    }

    // Remettre les grilles visible en non visible
    function resetVisibility() {
        _gridContainer = document.getElementById("gw-" + _context.type + "-" + _context.tab + "-" + _context.fileId);
        if (_gridContainer == null)
            return;

        var visibleGrids = _gridContainer.querySelectorAll(".gw-grid[active='1']");
        Array.prototype.slice.call(visibleGrids).forEach(function (current) { setAttributeValue(current, "active", "0"); });
    }

    // Lance le constructeur de l'objet
    init();

    function dispose() {
        if (_gridLayout)
            _gridLayout.dispose();
    }

    function hasParent() {


        var pTab = getAttributeValue(_gridContainer, "parent-tab") * 1;
        var pfid = getAttributeValue(_gridContainer, "parent-file") * 1;

        return pTab == _context.parentTab && pfid == _context.parentFileId;

    }

    // Methodes publiques
    return {
        'id': _context.gridId,
        'setContext': function (ctx) {
            _context.type = ctx.type;
            _context.tab = ctx.tab;
            _context.fileId = ctx.fileId;
            _context.gridId = ctx.gridId;
            _context.parentTab = ctx.parentTab;
            _context.parentFileId = ctx.parentFileId;
            _context.ref = ctx.ref;
            _context.height = ctx.height;
            _context.width = ctx.width;
        },
        'getContext': function () { return _context; },
        'load': load,
        'display': function () {

            resetVisibility();

            if (hasParent()) {
                // affichage de la grille
                setAttributeValue(_gridContainer, "active", "1");

                if (_gridLayout == null || typeof (_gridLayout) == "undefined")
                    _gridLayout = new eGridLayout(_context);

                // Affiche les widgets sur la grille
                _gridLayout.show();

                //SHA
                document.getElementsByClassName('icon-refresh')[0].click();
            }
        },
        'setGrid': setGrid,
        'dispose': dispose
    }
    
}

// Enum pour la place de la grille
var eGridLocation = {
    DEFAULT: 0,
    BKM: 1
}

/// Context global du chargement d'une grille
var eGridContext = function () {
    // type : tab ou bkm
    this.type = 'tab';

    // DescId de la table à laquelle la grille est associée
    this.tab;

    // Fiche à laquelle la grille est liée : id > 0 pour les pages d'accueil (115200) - id = 0 pour les autres tables
    this.fileId;

    // Identifiant de la grille
    this.gridId;

    //Element de référence pour la postion de la grille sur la page principale
    this.ref;

    // largeur de la grille
    this.width = 0;

    // hauteur de la grille  
    this.height = 0;

    // Origine de la grille
    this.gridLocation = eGridLocation.DEFAULT;

    // Table parente
    this.parentTab = 0;

    // Fiche parente
    this.parentFileId = 0;

    // Savoir si on est en admin ou pas
    this.isAdminMode = function () { return GridSystem.isAdminMode(); };
}