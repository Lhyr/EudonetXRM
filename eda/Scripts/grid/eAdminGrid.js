
/// <namespace>nsAdminGrid</namespace>
/// <summary>Regroupe l'ensemble de fonction permettant de gérer l'administration des grilles en onglet et en signet</summary>
/// <authors>MOU</authors>
/// <date>2017-08-28</date>
var nsAdminGrid = {

    /// <summary>Parametrage du signet Grille</summary>
    /// <param name="nBkm">DescID du signet</param>
    editBkm: function (nBkm) {

        // On met à jour le menu de droite
        nsAdminBkm.editBkmProperties(nBkm);

        setWait(true);
        // On charge le sous-menu grille
        nsAdmin.loadBkmContent({
            'bkm': nBkm,
            'onSuccess': nsAdminGrid.editBkmSuccess,
            'onError': nsAdminGrid.editBkmError
        });
    },

    /// <summary>Mise à jour du DOM</summary>
    /// <param name="oRes">Contenu du BKM retourné par le serveur</param>
    editBkmSuccess: function (oRes) {
        setWait(false);
        // Div temporaire pour avoir la structure html en élements
        var tmp = document.createElement("div");
        tmp.innerHTML = oRes;

        // on requete sur la élement
        var submenu = tmp.querySelector("div[id='admWebTab']");
        if (submenu) {
            nsAdminGrid.addBkmGridSubMenu(submenu);
        }
    },

    /// <summary>Erreur lors de l'appel serveur</summary>
    editBkmError: function () {
        eAlert(0, "Erreur", "Chargement échoué", "nsAdminGrid.editBkmError");
    },

    /// <summary>on masque le detail et on ajoute la grille</summary>
    addBkmGridSubMenu: function (subMenu) {
        // On masque le détail de la fiche
        var dtlsTable = document.querySelector("#FlDtlsBkm table.mTab.mTabFile");
        if (dtlsTable)
            dtlsTable.style.display = "none";

        // On enleve menu
        var header = document.querySelector("#FlDtlsBkm header");
        if (header)
            header.parentElement.removeChild(header);

        // On affiche le menu
        var FlDtlsBkm = document.getElementById("FlDtlsBkm");
        if (FlDtlsBkm) {
            var header = document.createElement("header");
            header.appendChild(subMenu);
            FlDtlsBkm.appendChild(header);
        }
    },

    /// <summary>On vire la div et on affiche le détail</summary>
    removeBkmGrid: function (nEdnType) {

        // On enleve la grille et son menu
        var header = document.querySelector("#FlDtlsBkm header");
        if (header)
            header.parentElement.removeChild(header);

        nEdnType = nEdnType * 1;
        if (nEdnType != 24) {
            // on affiche le detail de la fiche
            var dtlsTable = document.querySelector("#FlDtlsBkm table.mTab.mTabFile");
            if (dtlsTable)
                dtlsTable.style.display = "";
        }
    },

    /// <summary>Lance le refresh du widget</summary>
    reloadWidget: function (container) {
        var freshDiv = container.querySelector("#xrm-widget-reload");
        if (freshDiv)
            freshDiv.click();

    }
};






