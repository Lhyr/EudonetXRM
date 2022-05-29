var nsAdminFieldsList = nsAdminFieldsList || {};


nsAdminFieldsList.load = function (tab) {
    nsAdminFieldsList.tab = tab;
    nsAdminFieldsList.setEventListeners();
}

// Evénements
nsAdminFieldsList.setEventListeners = function () {
    // Double clic sur les cellules éditables
    var editableTd = document.querySelectorAll("td[data-editable='1']");
    for (var i = 0; i < editableTd.length; i++) {
        editableTd[i].addEventListener("dblclick", function () {
            nsAdminFieldsList.editField(this);
        });
    }

    // Clic sur l'icône crayon
    var buttons = document.getElementsByClassName("btnEdit");
    for (var i = 0; i < buttons.length; i++) {
        buttons[i].addEventListener("click", function () {
            nsAdminFieldsList.clickIconToEdit(this);
        });
    }
}

// Appel manager au changement d'onglet
nsAdminFieldsList.changeTab = function (element) {


    var upd = new eUpdater("eda/Mgr/eAdminFieldsListManager.ashx", 1);
    upd.addParam("tab", element.value, "post");

    upd.ErrorCallBack = function () { eAlert("Chargement échoué"); };

    top.setWait(true);
    upd.send(function (oRes) {
        nsAdminFieldsList.refreshTable(oRes);
        nsAdminFieldsList.load(element.value);
    });

    //var conf = top.eConfirm(1, top._res_1264, top._res_7924, "", null, null,
    //    function () {
            
    //    },
    //    function () {
    //        element.value = nsAdminFieldsList.tab;
    //    },
    //    true,
    //    false);
    
}

// Rafraîchissement du tableau à partir des résultats
nsAdminFieldsList.refreshTable = function (oRes) {
    var container = document.getElementById("fieldsListContainer");
    container.innerHTML = oRes;
    top.setWait(false);
}

// Au clic sur l'icône d'édition
nsAdminFieldsList.clickIconToEdit = function (element) {
    var td = findUp(element, "TD");

    nsAdminFieldsList.editField(td);
}

// Edition de la colonne
nsAdminFieldsList.editField = function (td) {

    var oldValue = "";

    var cellContent = td.querySelector(".cellContent");
    var input = td.querySelector(".txtValueEdit");

    addClass(cellContent, "hidden");

    addClass(input, "visible");
    input.value = cellContent.innerText;
    oldValue = input.value;
    input.focus();
    input.select();

    input.onchange = function () {
        nsAdminFieldsList.updateValue(this, this.value, oldValue);
    };
    input.onblur = function () {
        nsAdminFieldsList.updateValue(this, this.value, oldValue);
    };

}

// Mise à jour de la valeur de l'input 
nsAdminFieldsList.updateValue = function (input, value, oldValue) {
    var td = findUp(input, "TD");
    if (td) {
        var cellContent = td.querySelector(".cellContent");
        var cellValue = td.querySelector(".cellValue");

        removeClass(cellContent, "hidden");
        cellValue.innerText = value;
        setAttributeValue(td, "value", value);
        removeClass(input, "visible");

        if (oldValue != value) {
            nsAdmin.sendJson(input, true);
            addClass(td, "edited");
        }
           


    }
}

// Tri du tableau
nsAdminFieldsList.sortTable = function (idTable, colNum, order, isNum) {
    var table = document.getElementById(idTable);
    var tbody = table.querySelector("tbody");
    var rows = tbody.rows;
    var arr = new Array();
    var cellObj = null;

    // Construction de l'array
    for (i = 0; i < rows.length; i++) {
        cells = rows[i].cells;
        arr[i] = new Array();
        for (j = 0; j < cells.length; j++) {
            cellObj = { content: cells[j].outerHTML, value: cells[j].getAttribute("value") };
            arr[i][j] = cellObj;
        }

    }

    // Tri de l'array
    arr.sort(function (a, b) {

        var colA = a[colNum].value.toLowerCase();
        var colB = b[colNum].value.toLowerCase();

        if (isNum) {
            var nColA = Number(colA);
            var nColB = Number(colB);
            return (nColA == nColB) ? 0 : ((nColA > nColB) ? order : -1 * order);
        }
        else {
            return (colA == colB) ? 0 : ((colA > colB) ? order : -1 * order);
        }

    })

    // Reconstruction du tableau HTML
    var rowHtml;
    for (i = 0; i < rows.length; i++) {
        rowHtml = "";
        for (j = 0; j < cells.length; j++) {
            rowHtml = rowHtml + arr[i][j].content;
        }
        rows[i].innerHTML = rowHtml;
    }

    nsAdminFieldsList.setEventListeners();
}


nsAdminFieldsList.updateTabIndex = function () {

    var oUpdater = new eUpdater("eda/Mgr/eAdminFieldsListManager.ashx", 1);

    oUpdater.ErrorCallBack = function () { setWait(false); };

    oUpdater.addParam("action", "updTabindex", "post");
    oUpdater.addParam("tab", nsAdminFieldsList.tab, "post");

    setWait(true);
    oUpdater.send(nsAdminFieldsList.refreshTable);
}



//// Sauvegarde des modifications
//nsAdminFieldsList.saveModifications = function () {

//    var editedFields = document.querySelectorAll("#tableFieldsList td.edited");
//    var field, value, updAttr;
//    nsAdminFieldsList.capsule = new Capsule(nsAdminFieldsList.tab);

//    for (var i = 0; i < editedFields.length; i++) {
//        field = editedFields[i];

//        nsAdmin.addPropertyToCapsule(field, nsAdminFieldsList.capsule);
//    }
//}