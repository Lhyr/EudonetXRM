
/// <namespace>nsAdminGridSubMenu</namespace>
/// <summary>Regroupe l'ensemble de fonctions permettant d'intéragir avec le sous-menu des grilles</summary>
/// <authors>MOU</authors>
/// <date>2017-08-28</date>
var nsAdminGridSubMenu = {

    /// <summary>Parametrage du signet Grille</summary>
    /// <param name="gridId">identifiant des grilles</param>
    /// <param name="evt">event click</param>
    editGrid: function (mouseEvent) {

        // En admin, on vide le dom des toutes les grilles et widgets
        GridSystem.reset();

        mouseEvent = mouseEvent || window.event;
        var source = mouseEvent.srcElement || mouseEvent.target;

        // Le container de la grille avec le sous-menu
        fromSource(source).findParentById("admWebTab", function (parent) {

            Array.prototype.slice.call(parent.querySelectorAll("li[active='1']"))
                   .forEach(function (li) { setAttributeValue(li, "active", "0"); });


            var gid = getAttributeValue(source, "gid") * 1;
            var newSelected = parent.querySelector("li[fid='" + gid + "']");
            if (newSelected) setAttributeValue(newSelected, "active", "1");

            var gridType = getAttributeValue(parent, "gridType");
            if (gridType == "tab") {
                var header = parent.parentElement;
                fromSource(parent).findParentById("divFilePart1", function (divFilePart1) {

                    while (header.nextElementSibling != null)
                        divFilePart1.removeChild(header.nextElementSibling);

                    while (divFilePart1.nextElementSibling != null)
                        divFilePart1.parentElement.removeChild(divFilePart1.nextElementSibling);
                });
            } else
            {
                gridType = gridType + "";
            }

            // position de la grille 
            var child = parent.querySelector("#listheader")
            if (child) {
                var oContext = new eGridContext();
                oContext.tab = getAttributeValue(source, "did") * 1;
                oContext.type = gridType.toLowerCase();
                oContext.alias = "Tab";
                oContext.fileId = 0;
                oContext.gridId = gid,
                oContext.ref = child;
                oContext.width = 0;
                oContext.height = 0;
                oContext.parentTab = nGlobalActiveTab;
                if (nGlobalActiveTab > 0)
                    oContext.parentFileId = top.GetCurrentFileId(nGlobalActiveTab);
                    oContext.gridLocation = getAttributeValue(source, "data-gridlocation");

                    oAdminGridMenu.loadMenu(oGridController.grid.tab, oContext.gridId, oContext, function () {
                        manager = oGridManager.resetGrid(oContext.gridId, oContext);
                        oGridManager.refreshVisibility(oContext.tab);
                        manager.load();
                    });
            };
        });
    },

    /// <summary>Mise à jour du DOM</summary>
    /// <param name="oRes">Contenu du BKM retourné par le serveur</param>
    editGridSuccess: function (result) {


    },

    /// <summary>Erreur lors de l'appel serveur</summary>
    editGridError: function () {
        eAlert(0, "Erreur", "Chargement échoué", "nsAdminGridSubMenu.editGridError");
    },

    /// <summary>Parametrage de la grille</summary>
    /// <param name="nGridId">l'identifiant de la grille</param>
    deleteGrid: function (evt) {
        evt = evt || window.event;
        var src = evt.srcElement || evt.target;
        nsAdmin.deleteGrid(getAttributeValue(src, "gid"));
    }
};

