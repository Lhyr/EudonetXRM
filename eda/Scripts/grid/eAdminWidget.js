var nsAdminKanban = {};

nsAdminKanban.configureSwimlanes = function (widgetId) {
    var modalSwimlanes = new eModalDialog(top._res_8444, 0, "eda/mgr/eAdminKanbanMgr.ashx", 985, 750, "modalSwimlane");
    modalSwimlanes.widgetId = widgetId;
    modalSwimlanes.addParam("wid", widgetId, "post");
    modalSwimlanes.addParam("action", "1", "post");
    modalSwimlanes.addParam("context", JSON.stringify(nsAdmin.getWidgetContext(widgetId)), "post");
    modalSwimlanes.show();
    modalSwimlanes.addButton(top._res_30, function () { modalSwimlanes.hide(); }, 'button-gray', null);
    modalSwimlanes.addButton(top._res_28, function () { nsAdminKanban.saveSwimlanes(modalSwimlanes); }, 'button-green', null);

    nsAdmin.modalResizeAndMove(modalSwimlanes);


};

nsAdminKanban.saveSwimlanes = function (modalSwimlanes) {
    var doc = modalSwimlanes.getIframe().document;

    var sets = nsAdminKanban.getSettings(doc);

    var upd = new eUpdater("eda/mgr/eAdminKanbanMgr.ashx");
    upd.addParam("wid", modalSwimlanes.widgetId, "post");
    upd.addParam("sets", JSON.stringify(sets), "post");
    upd.addParam("action", "2", "post");

    upd.send(function () {
        var widget = document.getElementById("widget-wrapper-" + modalSwimlanes.widgetId);
        if (widget) {
            eTools.setWidgetWait(modalSwimlanes.widgetId, true);
            nsAdminGrid.reloadWidget(widget);
        }
        modalSwimlanes.hide();
    });

};

nsAdminKanban.getSettings = function (doc) {
    var sets = new KanbanSettings();

    var swimlanes = doc.querySelectorAll("#swimlanes .swimlaneBlock");
    for (var i = 0; i < swimlanes.length; i++) {
        var sl = swimlanes[i];
        var select = sl.querySelector("select");
        var sSwimLane = eTools.getSelectedValue(select);

        if (sSwimLane == "")
            continue;

        var swimLane = JSON.parse(sSwimLane);
        swimLane.DisplayEmptyLane = getAttributeValue(sl.querySelector("a.rChk"), "chk") == "1";

        swimLane.SelectedValues = getAttributeValue(sl.querySelector(".txtSLValues"), "dbv");

        sets.Swimlanes.push(swimLane);
    }

    var aAggregates = doc.querySelectorAll("div.ColAggregate");
    for (var i = 0; i < aAggregates.length; i++) {
        var aggDiv = aAggregates[i];
        var selectValue = aggDiv.querySelector("select");
        var agg = new KanbanColAggregate();
        agg.Value = eTools.getSelectedValue(selectValue);

        if (agg.Value == "")
            continue;

        sets.Aggregates.push(agg);

        var aColAggSettings = aggDiv.querySelectorAll("div.AggSetting");
        for (var j = 0; j < aColAggSettings.length; j++) {
            var colAggSet = aColAggSettings[j];

            var ddlOperation = colAggSet.querySelector("select[role='op']");
            var ddlOperationFields = colAggSet.querySelector("select[role='opfield']");
            var ddlUnitPosition = colAggSet.querySelector("select[role='unitposition']");
            var inptUnit = colAggSet.querySelector("input[role='unit']");
            var inptLabel = colAggSet.querySelector("input[role='label']");

            var AggSet = new KanbanColAggregateSetting();
            AggSet.Operation = getNumber(eTools.getSelectedValue(ddlOperation));
            if (AggSet.Operation > 1) //1: nombre de fiches
                AggSet.OperationField = eTools.getSelectedValue(ddlOperationFields);
            AggSet.UnitPosition = eTools.getSelectedValue(ddlUnitPosition);
            AggSet.Unit = inptUnit.value;
            AggSet.UnitResCode = getAttributeValue(inptUnit, "data-rcode") || "0";
            AggSet.UnitResLoc = getAttributeValue(inptUnit, "data-rloc");
            AggSet.Label = inptLabel.value;
            AggSet.LabelResCode = getAttributeValue(inptLabel, "data-rcode") || "0";
            AggSet.LabelResLoc = getAttributeValue(inptLabel, "data-rloc");

            if (AggSet.Operation != "")
                agg.Settings.push(AggSet);

        }

    }

    // Tri des cartes
    var ddlSortField = doc.getElementById("ddlSortField");
    var ddlSortOrder = doc.getElementById("ddlSortOrder");
    sets.Sort = {
        field: ddlSortField.value,
        order: ddlSortOrder.value
    }

    return sets;

}

// Au changement de la rubrique sélectionnée en ligne de couloir, on vide les valeurs
nsAdminKanban.onSLFieldChange = function (select, index) {
    var input = document.getElementById("slValues" + index);
    if (input) {
        nsAdminKanban.validSLValues(input, "", "");
    }
}

nsAdminKanban.AddAggregate = function () {

    var wid = document.getElementById("wid").value;

    var upd = new eUpdater("eda/mgr/eAdminKanbanMgr.ashx", 1);
    upd.addParam("wid", wid, "post");
    upd.addParam("cols", document.getElementById("CatValues").value, "post");
    upd.addParam("OpFields", document.getElementById("OpFields").value, "post");
    upd.addParam("context", JSON.stringify(nsAdmin.getWidgetContext(wid)), "post");
    upd.addParam("action", "4", "post");

    upd.send(nsAdminKanban.AddAggregateRdr);
};
nsAdminKanban.AddAggregateRdr = function (oRes) {
    var divAggregates = document.getElementById("ColsAggregates");
    var divtmp = document.createElement("div");
    divAggregates.appendChild(divtmp);
    divtmp.outerHTML = oRes;

    nsAdminKanban.PreventColDuplicate();

};
nsAdminKanban.DeleteAggregate = function (icondelete) {
    var divAggregate = icondelete.parentElement;
    divAggregate.parentElement.removeChild(divAggregate);
    nsAdminKanban.PreventColDuplicate();

};

nsAdminKanban.onChangeOperation = function (ddlOperation) {
    var iValue = getNumber(eTools.getSelectedValue(ddlOperation));
    var parent = findUp(ddlOperation, "UL");
    var liFields = parent.querySelector("li[role='opfield']");
    if (iValue > 1) {
        liFields.style.display = "";
    }
    else {
        liFields.style.display = "none";
    }
};

nsAdminKanban.PreventColDuplicate = function () {
    var aDisabledOptions = document.querySelectorAll("select[role='colvalue']>Option[disabled]");
    for (var i = 0; i < aDisabledOptions.length; i++) {
        aDisabledOptions[i].disabled = false;
    }

    var aSelectColumn = document.querySelectorAll("select[role='colvalue']");
    for (var i = 0; i < aSelectColumn.length; i++) {

        var selectColumn = aSelectColumn[i];
        var value = eTools.getSelectedValue(selectColumn);

        for (var j = 0; j < aSelectColumn.length; j++) {
            if (aSelectColumn[j] != selectColumn) {
                eTools.disableItemValue(aSelectColumn[j], value);
            }
        }
    }
};

// cet objet javascript correspond à l'objet eAdminKanbanSettings en C#
//A deplacer  dans un fichier à parti si on a besoin de cette info coté utilisation
function KanbanSettings() {
    this.Swimlanes = new Array();
    this.Aggregates = new Array();

    function Swimlane() {
        this.DescId = 0;
        this.IsGroup = false;
        this.Tab = 0;
        this.LinkField = 0;
        this.DisplayEmptyLane = false;

    }


}
function KanbanColAggregate() {
    this.Value = 0;
    this.Settings = new Array();

}
function KanbanColAggregateSetting() {
    this.Operation = 0;
    this.OperationField = 0;
    this.UnitPosition = 0;
    this.Unit = "";
    this.Label = "";
}



// Ouverture de la popup pour administrer la carte Kanban
nsAdminKanban.configureKanbanMinifile = function (tab, wid) {
    modalMiniFile = new eModalDialog(top._res_8443, 0, "eda/eAdminMiniFileDialog.aspx", 985, 652, "modalAdminKanbanMap");

    modalMiniFile.noButtons = false;
    modalMiniFile.addParam("tab", tab, "post");
    modalMiniFile.addParam("type", MiniFileType.Kanban, "post");
    modalMiniFile.addParam("wid", wid, "post");

    modalMiniFile.show();

    modalMiniFile.addButton(top._res_30, function () { modalMiniFile.hide(); }, 'button-gray', null);
    modalMiniFile.addButton(top._res_28, function () { nsAdminKanban.saveKanbanMapping(document.getElementById("btnAdminKanbanMap"), wid); }, 'button-green', null);

    nsAdmin.modalResizeAndMove(modalMiniFile);
}

// Mise à jour du mapping de la carte Kanban
nsAdminKanban.saveKanbanMapping = function (btnElement, wid) {
    if (btnElement) {
        var sJSON = "";
        var modal = top.eTools.GetModal('modalAdminKanbanMap');
        if (modal) {
            var mDoc = modal.getIframe().document;
            var fields = mDoc.querySelectorAll(".field[edamftype]");
            var arrJSON = new Array();
            var field, mftype, elt, value, chkLabel, bDisplayLabel, objMapping;

            for (var i = 0; i < fields.length; i++) {
                field = fields[i];

                mftype = getAttributeValue(field, "edamftype");
                if (mftype != "3") {
                    // Type "select"
                    elt = field.querySelector(".edamflist select");
                    value = elt.value || "0";
                }
                else {
                    // Type séparateur
                    elt = field.querySelector(".edamflabel .chk");
                    if (getAttributeValue(elt, "chk") == "1") {
                        value = getAttributeValue(field, "edamfvalue");
                    }
                    else {
                        value = "0";
                    }
                }

                // Option affichage du libellé
                bDisplayLabel = false;
                chkLabel = field.querySelector(".edamflibelle .chk");
                if (chkLabel) {
                    bDisplayLabel = getAttributeValue(chkLabel, "chk") == "1";
                }

                objMapping = new Object();
                objMapping.DisplayType = mftype;
                objMapping.Order = getAttributeValue(field, "edamforder") || "0";
                objMapping.Value = value;
                objMapping.DisplayLabel = bDisplayLabel;
                objMapping.ParentTab = getAttributeValue(field, "edamfparenttab") || "0";

                arrJSON.push(objMapping);
            }

            sJSON = JSON.stringify(arrJSON);
        }

        oAdminGridMenu.updateParamValue(btnElement, wid, sJSON);
        modal.hide();
    }

}

// Action au clic sur l'icône catalogue de sélection des valeurs en ligne de couloir
nsAdminKanban.selectSLBtnOnClick = function (btn) {
    var inputID = getAttributeValue(btn, "data-for");
    var input = document.getElementById(inputID);
    if (input)
        input.click();
}

// Ouverture de la popup correspondante pour sélectionner les valeurs en ligne de couloir
nsAdminKanban.selectSLValues = function (input) {

    var id = getAttributeValue(input, "data-slindex");

    var selectedValues = getAttributeValue(input, "dbv");

    var select = document.getElementById("swimlaneDescid" + id);
    if (select) {
        var value = select.value;

        if (!value) {
            top.eAlert(2, top._res_8471, top._res_8671);
            return;
        }

        var oParam = JSON.parse(value);

        if (oParam.FieldFormat == FLDTYP.USER) {
            nsAdminKanban.showUserCat(input, selectedValues);
        }
        else if (oParam.FieldFormat == FLDTYP.CHAR) {
            var fctValidate = function (catalogDialog, srcId, trgId, tabSelectedLabels, tabSelectedValues, selectedIDs) {
                var input = document.getElementById(trgId);
                if (input) {
                    var displayValue = tabSelectedLabels.join(';');
                    nsAdminKanban.validSLValues(input, selectedIDs, displayValue);
                }
            };
            var dialog = showCatGeneric(true, false, selectedValues, null, input.id, oParam.DescId, "3", null, null, null, top._res_8670, "advCatalog", false, fctValidate, function () { }, "0", "0", "1");
        }
    }

}

// Affichage du catalogue utilisateur pour sélection des valeurs en ligne de couloir
nsAdminKanban.showUserCat = function (input, selectedValues) {
    var modalUserCat = new eModalDialog(top._res_246, 0, "eCatalogDialogUser.aspx", 550, 640, "modalSelectSLUsers");
    modalUserCat.addParam("iframeId", modalUserCat.iframeId, "post");
    modalUserCat.ErrorCallBack = function () { setWait(false); }
    modalUserCat.addParam("multi", "1", "post");
    modalUserCat.addParam("showallusersoption", "1", "post");
    modalUserCat.addParam("showuseronly", "1", "post"); //si à 1 => la liste sera toujours sans groupes d'affichés

    modalUserCat.addParam("selected", selectedValues, "post");

    modalUserCat.addParam("modalvarname", "modalUserCat", "post");
    modalUserCat.show();

    modalUserCat.addButton(top._res_29, function () {
        modalUserCat.hide();
    }, "button-gray", null, null, true);
    modalUserCat.addButton(top._res_28, function () {
        var modal = eTools.GetModal("modalSelectSLUsers");
        if (modal) {
            var strReturned = modal.getIframe().GetReturnValue();
            var vals = strReturned.split('$|$')[0];
            var libs = strReturned.split('$|$')[1];
            nsAdminKanban.validSLValues(input, vals, libs);
            modal.hide();
        }
    }, "button-green");
}

// Affection des valeurs sélectionnées en ligne de couloir à l'input text correspondant
nsAdminKanban.validSLValues = function (input, values, labels) {
    input.value = labels;
    setAttributeValue(input, "title", labels);
    setAttributeValue(input, "dbv", values);
}



// Gestion de l'admin du widget Carto
var nsAdminCartoSelection = {};
nsAdminCartoSelection.modal = null;
nsAdminCartoSelection.sourceBuilder = null;
nsAdminCartoSelection.mappingBuilder = null;

// Ouvre la fenêtre de paramétrage du widget
nsAdminCartoSelection.loadSettings = function (wid) {

    // scale : mise à l'echelle de 0.80 % de la taille totale de l'ecran (top) 
    var size = top.getWindowSize().scale(0.85);

    nsAdminCartoSelection.widgetId = wid;
    nsAdminCartoSelection.modal = new eModalDialog(top._res_2034, 0, "eda/eAdminCartoSelectionDialog.aspx", size.w, size.h, "AdminCartoSelectionModal");
    nsAdminCartoSelection.modal.noButtons = false;
    nsAdminCartoSelection.modal.hideMaximizeButton = true;
    nsAdminCartoSelection.modal.addParam("wId", wid, "post");
    nsAdminCartoSelection.modal.addParam("wHeight", Math.floor(size.h - 150), "post");
    nsAdminCartoSelection.modal.onIframeLoadComplete = nsAdminCartoSelection.init;
    nsAdminCartoSelection.modal.show();



    nsAdminCartoSelection.modal.addButton(top._res_30, nsAdminCartoSelection.modal.hide, 'button-gray', null);
    nsAdminCartoSelection.modal.addButton(top._res_28, nsAdminCartoSelection.validate, 'button-green', null);

    nsAdminCartoSelection.modal.addButton("Exemple", nsAdminCartoSelection.reset, 'button-green', null, "Réinitialiser", "left");
    nsAdminCartoSelection.modal.addButton("Effacer", nsAdminCartoSelection.delete, 'button-green', null, "effacer", "left");
    
};

// Initialise le contenu interne de la fenetre
nsAdminCartoSelection.init = function () {
    nsAdminCartoSelection.InternalAdminCartoSelection = nsAdminCartoSelection.modal.getIframe().AdminCartoSelection;
    nsAdminCartoSelection.InternalAdminCartoSelection.init(nsAdminCartoSelection.widgetId);
};

nsAdminCartoSelection.validate = function () {
    nsAdminCartoSelection.InternalAdminCartoSelection.save(nsAdminCartoSelection.modal.hide);
};
nsAdminCartoSelection.reset = function () {
    nsAdminCartoSelection.InternalAdminCartoSelection.reset();
};
nsAdminCartoSelection.delete = function () {
    nsAdminCartoSelection.InternalAdminCartoSelection.delete();
};

