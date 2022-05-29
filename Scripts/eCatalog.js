var Sort = "";
function eCatalog(jsVarName, pageName, catDescId, catParentId, catBoundPopup, catBoundDescId, catPopupType, catMultiple, catTreeView) {
    var that = this; // pointeur vers l'objet eCatalog lui-même, à utiliser à la place de this dans les évènements onclick, setTimeout... (où this correspond alors à l'objet cliqué, à window...)

    this.catDescId = catDescId;
    this.catParentId = catParentId;
    this.catBoundPopup = catBoundPopup;
    this.catBoundDescId = catBoundDescId;
    this.catPopupType = catPopupType; // Type de catalogue
    this.catMultiple = (catMultiple == '1' ? true : false);

    this.treeview = (catTreeView == '1' ? true : false);
    this.catalogTitle = ''; // Titre du catalogue

    this.iFrameId = ''; //iFrame contenant l'objet eFieldEditor 

    this.addAllowed = true;
    this.updateAllowed = true;
    this.delAllowed = true;
    this.syncAllowed = true;

    this.catSelectedValues = null;
    this.langUsed = '';
    this.dataEnabled = false; // Droit de modif du code de valeur

    this.jsVarName = jsVarName;
    this.jsVarNameEditor = '';
    this.pageName = pageName;
    this.advAction = '';

    this.displayFilter = "All";
    this.searchFilter = '';
    this.initialListElem = '';
    this.selectedListValues = new Array();
    this.highlightedListValues = new Array();
    this.highlightedSelListValues = new Array();
    this.searchTimer = null;

    //this.fromFilter = false;
    //this.fromTreat = false;
    //this.fromAdmin = false;
    this.from = LOADCATFROM.UNDEFINED;
    this.onlyAdmin = false;

    this.eCEDLblEditorPopup = null;
    this.eCEDLblEditor = null;
    this.dataEdit = false;
    this.bScrollIntoV = false;

    this.selectedClassName = "eTVS";
    this.initEditors = function () {
        // Pour le positionnement du efieldeditor, on position le ePopup directement dans le body du document (de iframe)
        //var parentElement = document.getElementById('eCEDValues');

        if (!this.eCEDLblEditorPopup) {
            this.eCEDLblEditorPopup = new ePopup('eC.eCEDLblEditorPopup', '75%', '25px', 0, 0, document.body, false);
            this.eCEDLblEditor = new eFieldEditor('inlineEditor', this.eCEDLblEditorPopup, 'eC.eCEDLblEditor', 'catEditLbl');
            this.eCEDLblEditor.action = 'renameCatalogValue';
        }
    };

    // Renomme une valeur
    // lineId : id de la TR ; cellId : id de la TD ; openerElem : element this
    this.renameVal = function (lineId, cellId, openerElem) {
        if (this.catPopupType == 3) {        // POPUP_DATA
            var oLine;
            if (!this.treeview) {
                oLine = document.getElementById(lineId);
            }
            else {
                oLine = document.getElementById(this.highlightedListValues[0]);
                if ((oLine) && (typeof (oLine) != 'undefined')) {
                    oLine = oLine.querySelector("span[ednid]");
                }
            }
            if (!oLine || !oLine.getAttribute('ednid'))
                return;
            this.editValueCatAdv('edit', oLine);
        }
        else {
            eC.initEditors();
            eC.eCEDLblEditor.isNewValue = true; // empêche le déclenchement d'un update en base sur une valeur encore inexistante
            eC.eCEDLblEditor.action = "renameCatalogValue";
            eC.eCEDLblEditor.onClick(document.getElementById(cellId), openerElem);
        }
    };

    // Ajoute une valeur
    // openerElem : element this
    this.addVal = function (openerElem) {
        if (this.treeview) {
            eC.editValCatTreeView('add');
        }
        else if (this.catPopupType == 3)        // POPUP_DATA
            eC.editValueCatAdv('add');
        else
            eConfirm(1, top._res_1486, (top._res_6194 + '').replace('<VALUE>', this.searchFilter) + ' ?', '', 500, 200, function () { eC.addCatVal(this.searchFilter); }, function () { });
    };

    //exporter catalogue
    this.exportCat = function () {

        var url = "eda/Mgr/eAdminExportCatalogManager.ashx";
        window.open(url + "?descid=" + this.catDescId);

    };

    //importer catalogue
    this.openImportCat = function () {
        var lWidth = 800;
        var lHeight = 550;
        var strTitre = top._res_6340;
        eC.eModalImport = new eModalDialog(
            strTitre,
            0,
            "eImportCatalogDialog.aspx",
            lWidth,
            lHeight);
        eC.eModalImport.addParam("CatDescId", this.catDescId, "post");
        eC.eModalImport.addParam("CatTitle", strTitre, "post");
        eC.eModalImport.show();
        eC.eModalImport.addButton(top._res_29, cancelImportCat, "button-gray", ""); // Annuler
        eC.eModalImport.addButton(top._res_28, validateImportCat, "button-green", ""); // Valider
    };

    cancelImportCat = function () {
        eC.eModalImport.hide();
    };

    validateImportCat = function () {

        var mondoc = eC.eModalImport.getIframe();

        var importvalue = mondoc.document.getElementById("eTextImportCat").value;

        var lines = importvalue.split("\n");

        if (lines.length <= 101 && lines.length >= 2 && lines[0] != "") {
            var url = "eda/Mgr/eAdminImportCatalogManager.ashx";
            var oUpdater = new eUpdater(url, 1);
            var descidcat = eC.eModalImport.getParam("CatDescId");
            oUpdater.addParam("CatDescId", descidcat, "post");
            oUpdater.addParam("importvalue", importvalue, "post");

            oUpdater.ErrorCallBack = function () { };

            oUpdater.send(afterImport);
        }
        else if (lines.length > 101) {
            eAlert(0, top._res_8479, top._res_2446, "", 500, 200);
        }
        else {
            // si il y a pas des données dans la fenetre d'import
            eAlert(0, top._res_8479, top._res_2460, "", 500, 200);
        }
    };

    afterImport = function (sRes) {

        try {
            var result = JSON.parse(sRes);
            if (result.Success && result.ErrorMsg.length != 0) {
                eAlert(4, top._res_8479, top._res_2447, result.ErrorMsg, 500, 200);
                eC.eModalImport.hide();

                updatelistcat();
            }
            else if (result.Success && result.ErrorMsg.length == 0) {
                eAlert(4, top._res_8479, top._res_2447, '', 500, 200);
                eC.eModalImport.hide();
                updatelistcat();
            }
            else {
                eAlert(0, top._res_8479, top._res_2470, result.ErrorMsg, 500, 200);
            }
        }
        catch (e) {

        }
    }


    updatelistcat = function () {
        var catalogObject = window[jsVarName];
        catalogObject.startSearch(function (highlightedValues) {

            for (var i = 0; i < highlightedValues.length; i++) {
                catalogObject.highlightListValue(highlightedValues[i], true);
            }
            // Called function after search
            var id = (getXmlTextNode(oRes.getElementsByTagName("id")[0]));
            var valueElt;
            if (!catalogObject.treeview) {
                valueElt = document.getElementById("val_" + id);
            }
            else {
                valueElt = document.getElementById("eTVBLV_" + id);
            }
            if (valueElt)
                valueElt.click();
        })
    }


    // Evenement click sur une valeur
    // event : evenement ; [selectedObject] : element this (représente le TR) (cat arbo) ; [setAsSelected] : indicateur si mise à jour de this.selectedListValues necessaire (cat arbo)
    this.clickVal = function (e, selectedObject, setAsSelected) {
        var clickedObjectId = '';

        if (typeof (setAsSelected) == 'undefined') {
            // Catalogue multiple : cliquer sur un élément le met en surbrillance uniquement. Sinon, on met en surbrillance ET on sélectionne la valeur
            // Catalogue arbo : setAsSelected est défini par la fonction appelante, on ne tient donc pas compte de catTreeView ici
            setAsSelected = !this.catMultiple;
        }

        if (typeof (selectedObject) === 'undefined' && e) {
            clickedObjectId = this.getClickedObjectId(e);
        }
        else {
            clickedObjectId = selectedObject.id;
        }



        if (clickedObjectId != '') {
            if (setAsSelected) {
                this.selectListValue(clickedObjectId);

                if (this.treeview) {
                    this.onTreeSelect(clickedObjectId);
                }
            }
            this.highlightListValue(clickedObjectId);
        }

        if (e) {
            var oClickObj = this.getClickedObject(e);
            if (oClickObj && clickedObjectId != "") {

                setFocus();
            }
        }

    };



    // Evenement double click sur une valeur
    // event : evenement
    this.dblClickVal = function (e) {
        if (!this.catMultiple || !this.treeview) {
            var oModal = top.eTabCatModalObject[this.iFrameId];
            oModal.CallOnOk(this.jsVarNameEditor, e, this.iFrameId);
        }
        else {
            var lineId = this.getClickedObjectId(e);
            this.highlightListValue(lineId, false);  // Retire la surbrillance de la valeur
            this.selectListValue(lineId);
        }
    };


    this.getClickedObject = function (e) {
        var clickedObjectId = '';

        if (e && e.target)
            var oOrigNode = e.target;
        else
            var oOrigNode = window.event.srcElement;

        if (!oOrigNode)
            return clickedObjectId;
        var oClickedObject = null;
        if (this.treeview)
            oClickedObject = this.findUp(oOrigNode, "TR");
        else {
            oClickedObject = this.findUp(oOrigNode, "LI");
            if (!oClickedObject.id || oClickedObject.id.indexOf("val") != 0)
                oClickedObject = this.findUp(oClickedObject.parentElement, "LI");
        }

        return oClickedObject;
    };

    // Récupère l'id de la TR depuis l'évenement
    // event : evenement
    this.getClickedObjectId = function (e) {
        var clickedObjectId = '';

        if (e && e.target)
            var oOrigNode = e.target;
        else
            var oOrigNode = window.event.srcElement;

        if (!oOrigNode)
            return clickedObjectId;

        var oClickedObject = null;
        if (this.treeview)
            oClickedObject = this.findUp(oOrigNode, "TR");
        else {
            oClickedObject = this.findUp(oOrigNode, "LI");
            if (!oClickedObject.id || oClickedObject.id.indexOf("val") != 0)
                oClickedObject = this.findUp(oClickedObject.parentElement, "LI");
        }

        if (oClickedObject)
            clickedObjectId = oClickedObject.id;

        return clickedObjectId;
    };
    // Paramètre le filtre d'affichage (boutons radio en entête) pour le treeview
    this.setDisplayFilterET = function (forceSearch) {
        if (!forceSearch) {
            var rbOnlySelected = document.getElementById('displayFilterOnlySelected');
            var txtSearch = document.getElementById('eTxtSrch');
            if (rbOnlySelected && (rbOnlySelected.checked) && (txtSearch)) {
                txtSearch.value = '';
            }
        }
        this.setDisplayFilter(forceSearch);

    }

    // Paramètre le filtre d'affichage (boutons radio en entête)
    this.setDisplayFilter = function (forceSearch) {
        var previousFilter = this.displayFilter;
        var rbAll = document.getElementById('displayFilterAll');
        var rbOnlySelected = document.getElementById('displayFilterOnlySelected');
        var rbSearch = document.getElementById('displayFilterSearch');
        var rbCollapse = document.getElementById('displayFilterCollapse');
        var txtSearch = document.getElementById('eTxtSrch');

        // forceSearch = Si l'appel à la fonction a été déclenché par la saisie dans le champ de recherche
        if (forceSearch && txtSearch && txtSearch.value != '') {
            this.displayFilter = "Search";
            if (rbSearch)
                rbSearch.checked = true;
        }
        // Si l'appel a été déclenché par un clic sur les radio buttons
        else {
            if (rbOnlySelected && rbOnlySelected.checked)
                this.displayFilter = "OnlySelected";
            else {
                // !forceSearch = le filtre de recherche est laissé activé si on a coché le bouton
                if (rbSearch && rbSearch.checked && !forceSearch) {
                    this.displayFilter = "Search";
                    setFocus();
                }
                else if ((rbCollapse) && (rbCollapse.checked)) {
                    this.displayFilter = "Collapse";
                    this.CollapseAll("none");
                }
                // Sinon, dans tous les autres cas, on coche "Tous"
                else {
                    this.displayFilter = "All";
                    if (rbAll)
                        rbAll.checked = true; // force le cochage du bouton "Tous" dans le cas, notamment, où on vide le champ de recherche
                }
            }
        }

        // Action suite à nouveau filtre
        if (this.displayFilter != previousFilter) {
            this.applyDisplayFilter(previousFilter);
        }
    };

    this.applyDisplayFilter = function (previousFilter) {
        // Lance la recherche si le filtre a été activé
        if (this.displayFilter == "Search") {
            var textbox = document.getElementById('eTxtSrch');
            this.findValues(null, textbox.value, getAttributeValue(textbox, "data-searchlimit"));
        }
        else {
            // Relance la recherche pour retrouver toutes les valeurs si le filtre de recherche a été désactivé
            if (previousFilter == "Search") {
                //appel de start search directement pour ne pas avoir de temps d'attente qui "zap" les traitements qui suivent
                this.searchFilter = '';
                this.startSearch();
            }

            // Affiche/masque les éléments non sélectionnés sinon (catalogue arbo)
            if (this.treeview) {
                var branchDisplay = "block";
                if (this.displayFilter == 'OnlySelected') {
                    branchDisplay = "none";
                }

                this.ViewHideValues(branchDisplay, null);

                if (this.displayFilter == 'Collapse') {
                    branchDisplay = "none";
                    this.CollapseAll(branchDisplay);
                }
                else if (this.displayFilter == 'All') {
                    branchDisplay = "block";
                    this.CollapseAll(branchDisplay);
                }
            }
        }
    };

    this.ViewHideValues = function (displayMode, branchRoot) {
        var checkBoxes = this.getTreeChildrenCheckBoxes(branchRoot);
        for (var i = 0; i < checkBoxes.length; i++) {
            if (checkBoxes[i].getAttribute("chk") == "0") {
                var parentLi = this.findUp(checkBoxes[i], "LI");
                if (parentLi && !this.findCheckedChild(parentLi)) {
                    parentLi.style.display = displayMode;
                }
                //méthodes Affichant / Masquant les elements de la listes Arborescente.
                this.ViewHideValues(displayMode, checkBoxes[i]);
            }
        }
    }

    //Catalogue Arborescent - Déplie (paramètre à true) ou Replie toute les branches de l'arborescence
    this.CollapseAll = function (displayMode) {
        if (eTV) {
            var oDivResult = document.getElementById("eCEDValues");

            if ((typeof (oDivResult) == 'undefined') || !oDivResult)
                oDivResult = document.getElementById("DivResult");
            if ((typeof (oDivResult) == 'undefined') || !oDivResult)
                return;

            var oUl = oDivResult.getElementsByTagName("ul");

            //on inverse l'affichage pour les ul qui en ont besoin
            //et si ce n'est pas le niveau le plus haut ou le second niveau
            for (var i = 0; i < oUl.length; i++) {
                if ((((oUl[i].style.display == "none") ? "none" : "block") != displayMode) && ((oUl[i].parentNode != oDivResult) && (this.findUp(oUl[i].parentNode, "UL").parentNode != oDivResult))) {
                    var oAnchor;
                    var oAList = oUl[i].parentNode.getElementsByTagName("a");
                    for (var j = 0; j < oAList.length; j++) {
                        if (eTV.isClassName(oAList[j], eTV.foldIconClassName)) {
                            var oSpan = oAList[j].getElementsByTagName("span");
                            if (oSpan.length > 0) {
                                oAnchor = oAList[j];
                                eTV.liAClicked(oAnchor);
                                if (eTV.isClassName(oSpan[0], eTV.openIndicatorClassName)) {
                                    oAnchor = oAList[j];
                                    j = oAList.length + 1;
                                }
                            }

                        }
                    }
                    //if (oAnchor)
                    //    eTV.liAClicked(oAnchor);

                }

            }

        }
    }

    // Déclenche le cochage/décochage des branches filles ou mères sur le catalogue arbo
    this.onTreeSelect = function (selObjectId) {


        var selObject = document.getElementById(selObjectId);
        var selCB = document.getElementById("chkValue_" + selObjectId.substring(selObjectId.indexOf("_") + 1));
        var parentCBs = this.getAllTreeParentCheckBox(selCB);
        var isChecked = (selCB.getAttribute("chk") == "1");



        //Si on vient d'un filtre, les fonctions de cochage/décochage
        // ne doivent pas être activé
        if ([LOADCATFROM.EXPRESSFILTER, LOADCATFROM.FILTER].some(function(t){ return t == that.from})) {
            if (isChecked && !that.catMultiple) {
                //Dans ce cas, on ne doit cocher qu'une case du treeview

                that.UnselTreeAll(selCB);
            }
            return;
        }

        // enfant coché : on coche les parents
        if (isChecked && parentCBs) {
            for (var i = 0; i < parentCBs.length; i++) {
                this.selectListValue("eTVBLVT_" + parentCBs[i].id.substring(parentCBs[i].id.indexOf("_") + 1), true);
            }
        }

        // parent décoché : on décoche les enfants
        this.UnselTreeChildCB(selCB.id);

        // enfant décoché : on décoche les parents si plus aucun enfants de coché
        if (!isChecked && selCB) {
            // on décoche le parent si plus aucun enfant n'est coché
            var hasCheckedChildren = false;
            var i = 0;
            //Récupérer les checkbox parents (directement)
            var direct_parentCB = this.getTreeParentCheckBox(selCB);
            if (direct_parentCB) {
                //Vérifier que le parent n'a plus d'enfant de cocher
                var parentsChildCBs = this.getTreeChildrenCheckBoxes(direct_parentCB);
                while (!hasCheckedChildren && i < parentsChildCBs.length) {
                    if (parentsChildCBs[i].getAttribute("chk") == "1")
                        hasCheckedChildren = true;
                    else
                        i++;
                }
                //si aucun enfant de coché on décoche le parent et pour le parent on revérifié toutes les règles de cochage
                if ((!hasCheckedChildren) && (direct_parentCB)) {
                    this.selectListValue(direct_parentCB.id, false);
                    this.onTreeSelect(direct_parentCB.id);
                }
            }
        }

    };

    //déselectionne toutes les checkbox du treeview sauf celle en param
    this.UnselTreeAll = function (icdB) {


        var cur_checkbox = icdB;
        var isChecked = (cur_checkbox.getAttribute("chk") == "1");

        var nKeepId = 1 * icdB.id.substring(icdB.id.indexOf("_") + 1);

        var childCBs = this.getTreeChildrenCheckBoxes(null);
        var isParent = (childCBs.length > 0);
        if (isParent) {
            for (var i = 0; i < childCBs.length; i++) {
                if (childCBs[i].getAttribute("chk") == 1) {
                    var ncurI = 1 * childCBs[i].id.substring(childCBs[i].id.indexOf("_") + 1);
                    cur_checkbox = document.getElementById("eTVBLVT_" + ncurI);


                    if (cur_checkbox && nKeepId != ncurI) {
                        this.selectListValue(cur_checkbox.id, false);
                        this.UnselTreeChildCB(cur_checkbox.id);
                    }
                }
            }
        }
    }

    //Déselectionne tous les enfants de la checkbox dont l'id est passé en paramètre
    this.UnselTreeChildCB = function (idCB) {

        var cur_checkbox = document.getElementById(idCB);
        var isChecked = (cur_checkbox.getAttribute("chk") == "1");
        var childCBs = this.getTreeChildrenCheckBoxes(cur_checkbox);
        var isParent = (childCBs.length > 0);
        if (isParent && !isChecked) {
            for (var i = 0; i < childCBs.length; i++) {
                if (childCBs[i].getAttribute("chk") == 1) {
                    cur_checkbox = document.getElementById("eTVBLVT_" + childCBs[i].id.substring(childCBs[i].id.indexOf("_") + 1));
                    if (cur_checkbox) {
                        this.selectListValue(cur_checkbox.id, false);
                        this.UnselTreeChildCB(cur_checkbox.id);
                    }
                }
            }
        }
    }

    // Retourne les cases à cocher de la branche parentes d'une branche de catalogue arborescent (TOUS LES PARENTS)
    this.getAllTreeParentCheckBox = function (checkBox) {
        var parentCBs = new Array();
        var cur_object;
        //Lorsque l'on atteint la racine c'est quu tous les parents ont été récupéré.
        cur_object = checkBox;
        while (cur_object) {
            cur_object = this.getTreeParentCheckBox(cur_object);
            if (cur_object) {
                parentCBs.push(cur_object);
            }
        }
        return parentCBs;
    };

    // Retourne la case à cocher parente de la branche parente d'une branche de catalogue arborescent
    this.getTreeParentCheckBox = function (checkBox) {
        var parentUl = this.findUp(checkBox, "UL"); // img (checkBox) > span (ensemble checkBox/libellé) > li (branche) > ul (parent)
        var parentLi = this.findUp(parentUl, "LI"); // ul (parent) > li (branche parente)
        var isRoot = ((!parentUl.id) || parentUl.id.substring(parentUl.id.indexOf("_") - 1) == "0" || parentLi.id.substring(parentLi.id.indexOf("_") - 1) == "0");

        //Prendre les cases parentes si on est pas à la racine
        var parent_cb = (!isRoot) ? parentLi.querySelectorAll("a[chk]") : null;


        //Prendre la première case à cocher parentes car il ne peut y en avoir qu'une.
        parent_cb = (parent_cb && parent_cb.length > 0)
            ? parent_cb[0]
            : null;
        return parent_cb;
    }

    // Retourne le tableau de cases à cocher filles d'une branche de catalogue arborescent
    this.getTreeChildrenCheckBoxes = function (checkBox) {
        var childCBs = new Array();
        var parentLi = this.findUp(checkBox, "LI"); // ul (parent) > li (branche parente)
        if (!parentLi) {
            /*Cas ou on est à la racine, le premier élement n'est pas une CheckBox, par conséquent, on change de fonctionnement
            On charge une liste des UL enfants et on les traite.*/
            parentLi = document.getElementById("eTVB_0");
            ChildUlList = parentLi.getElementsByTagName("UL");
            for (var i = 0; i < ChildUlList.length; i++) {
                var childUl = ChildUlList[i];
                var childCB;
                for (var j = 0; j < childUl.childNodes.length; j++) {
                    var childCB = document.getElementById("chkValue_" + childUl.childNodes[j].id.substring(childUl.childNodes[j].id.indexOf("_") + 1));
                    if (childCB) {
                        childCBs.push(childCB);
                    }
                }
            }
        }
        else {
            var childUl = document.getElementById("eTVBC_" + parentLi.id.substring(parentLi.id.indexOf("_") + 1));
            if (childUl) {
                var childCB;
                for (var i = 0; i < childUl.childNodes.length; i++) {
                    childCB = document.getElementById("chkValue_" + childUl.childNodes[i].id.substring(childUl.childNodes[i].id.indexOf("_") + 1));
                    if (childCB) {
                        childCBs.push(childCB);
                    }
                }
            }
        }
        return childCBs;
    };

    // selectedObject : id de la TR ; setAsSelected : indicateur si mise à jour de this.selectedListValues necessaire
    this.hlgltSelVal = function (selectedObjectId, setAsSelected) {
        this.selectListValue(selectedObjectId, setAsSelected);
        this.highlightListValue(selectedObjectId);
    };

    // Met en surbrillance une valeur de catalogue
    // selectedObject : id de la TR ; alwaysHighlight : indicateur si on force ou pas la la surbrillance
    this.highlightListValue = function (selectedObjectId, alwaysHighlight) {
        if (!this.catMultiple || this.treeview) {
            // TODO HLA ne pas faire le boulot si le click est sur la colonne déjà selectionné
            // Suppression de la surbrillance sur l'élément précédent
            var oLine = document.getElementById(this.highlightedListValues[0]);
            if (parent.removeClass && oLine) {
                parent.removeClass(oLine, this.selectedClassName);

            }

            // Vide le tableau
            this.highlightedListValues = new Array();

            // Ajout de la surbrillance sur l'élément selectionné
            var oLine = document.getElementById(selectedObjectId);
            if (parent.addClass && oLine) {
                parent.addClass(oLine, this.selectedClassName);
                this.showItem(selectedObjectId);
            }

            // Mise à jour du tableau contenant l'éléments en surbrillance
            this.highlightedListValues.push(selectedObjectId);
        }
        else {
            //JAS
            if (selectedObjectId == "")
                return;
            var oLine = document.getElementById(selectedObjectId);
            var oTable = this.findUp(oLine, "UL");

            if (!oTable)
                return;

            var workList;
            if (oTable.id == 'tbCatVal')
                workList = this.highlightedListValues;
            else if (oTable.id == 'tbCatSelVal')
                workList = this.highlightedSelListValues;

            // Mise à jour du tableau contenant les éléments en surbrillance
            var bAdd = false;

            if (alwaysHighlight)
                bAdd = true;
            else if ((!alwaysHighlight && typeof alwaysHighlight !== "undefined"))
                bAdd = false;
            else if (workList.length == 0 || workList.indexOf(selectedObjectId) == -1)
                bAdd = true;


            if (bAdd) {
                var oLine = document.getElementById(selectedObjectId);
                if (parent.addClass && oLine) {
                    parent.addClass(oLine, this.selectedClassName);
                    this.showItem(selectedObjectId);
                }

                if (workList.indexOf(selectedObjectId) == -1)
                    workList.push(selectedObjectId);
            }
            else {
                var oLine = document.getElementById(selectedObjectId);
                if (parent.removeClass && oLine) {
                    parent.removeClass(oLine, this.selectedClassName);
                }

                workList.splice(workList.indexOf(selectedObjectId), 1);
            }
        }
    };



    this.selectedListValuesDico = new Dictionary();


    // Définit une valeur de catalogue comme sélectionnée - Ajout de la valeur sélectionnée dans le tableau des selections
    // selectedObject : id de la TR
    this.selectListValue = function (selectedObjectId, alwaysSelect) {



        if (!this.catMultiple)
            this.selectedListValuesDico = new Dictionary();


        if (selectedObjectId + "" == "")    //le getElementById vide plante sous IE8
            return;
        var oSelectedObject = document.getElementById(selectedObjectId);

        if (this.treeview) {

            var selectedObjCheckBox = document.getElementById("chkValue_" + selectedObjectId.substring(selectedObjectId.indexOf("_") + 1));

            var selectedObj = document.getElementById("eTVBLVT_" + selectedObjectId.substring(selectedObjectId.indexOf("_") + 1));

            if (alwaysSelect || (typeof (alwaysSelect) == 'undefined' && (this.selectedListValues.length == 0 || this.selectedListValues.indexOf(selectedObj.id) == -1))) {
                this.selectedListValues.push(selectedObj.id);

                var sEdnId = getAttributeValue(selectedObj, "ednid");
                if (this.selectedListValuesDico.Keys.indexOf(sEdnId) < 0) {

                    var sLbl = selectedObj.innerText || selectedObj.textContent;
                    this.selectedListValuesDico.Add(getAttributeValue(selectedObj, "ednid"), sLbl);
                }


                // on matérialise la sélection en cochant la case si ce n'est pas déjà fait (permet d'éviter de déclencher son onclick)
                if (selectedObjCheckBox)
                    chgChk(selectedObjCheckBox, true);
            }
            else {

                this.selectedListValues.splice(this.selectedListValues.indexOf(selectedObj.id), 1);



                var sEdnId = getAttributeValue(selectedObj, "ednid");
                this.selectedListValuesDico.Delete(sEdnId);

                // on matérialise la désélection en décochant la case si ce n'est pas déjà fait (permet d'éviter de déclencher son onclick)
                if (selectedObjCheckBox)
                    chgChk(selectedObjCheckBox, false);
            }
        }
        else if (this.catMultiple) {
            if (!oSelectedObject)
                return;

            var oTable = this.findUp(oSelectedObject, "UL");

            if (!oTable)
                return;
            if (oTable.id == 'tbCatVal')
                this.multSelItem(oSelectedObject);
            else
                this.multUnSelItem(oSelectedObject);
            this.multAdjustHighLitghLine();
        }
        else {
            // Vide le tableau. Nous sommes dans le cas d'une selection unique
            this.selectedListValues = new Array();
            this.selectedListValues.push(selectedObjectId);
        }
    };

    this.multAdjustHighLitghLine = function () {

        var oTableSource = document.getElementById("tbCatVal");
        var oTableTarget = document.getElementById("tbCatSelVal");

        var i = 0;
        forEach(oTableSource.children, function (oLine) {
            parent.removeClass(oLine, "odd");
            if ((i % 2) != 1)
                parent.addClass(oLine, "odd");
            if (oLine.style.display != "none")
                i++;
        });
        i = 0;
        forEach(oTableTarget.children, function (oLine) {
            parent.removeClass(oLine, "odd");
            if ((i % 2) != 1)
                parent.addClass(oLine, "odd");
            if (oLine.style.display != "none")
                i++;
        });
    }

    // Evenement lors de la saisie dans la zone de recherche
    // event : evenement
    this.srch = function (e) {
        var textbox = document.getElementById('eTxtSrch');
        this.findValues(e, textbox.value, getAttributeValue(textbox, "data-searchlimit"));
    };

    // Bouton pour lancer ou annuler la recherche
    this.btnSrch = function () {
        var oBtnSrch = document.getElementById("eBtnSrch");

        if (oBtnSrch && oBtnSrch.getAttribute('srchState') == 'on')
            document.getElementById('eTxtSrch').value = '';

        this.findValues(null, document.getElementById('eTxtSrch').value);
    };

    // Gestionnaire du lancement de la recherche 
    // event : evenement ; val : mot de recherche
    this.findValues = function (e, val, searchLimit) {

        if (typeof (searchLimit) === "undefined" || Number(searchLimit) == 0)
            searchLimit = 3;
        else
            searchLimit = Number(searchLimit);

        var oBtnSrch = document.getElementById("eBtnSrch");

        if (oBtnSrch) {
            if (val.length < searchLimit && e != null && e.keyCode != 13 && oBtnSrch.getAttribute('srchState') == 'off')
                return false;
        }
        else {
            if (val.length != 0 && val.length < searchLimit && e != null && e.keyCode != 13)
                return false;
        }

        if (oBtnSrch) {
            if (oBtnSrch.getAttribute('srchState') != 'on' && val != '') {
                oBtnSrch.className = "logo-search-croix icon-edn-cross";
                oBtnSrch.setAttribute('srchState', 'on');
            }
            else if (oBtnSrch.getAttribute('srchState') == 'on' && val == "") {
                oBtnSrch.className = "logo-search";
                oBtnSrch.setAttribute('srchState', 'off');
                setFocus();
            }
        }

        if (e != null) {
            if (e.keyCode == 27) {
                // Echap : annuler la fenêtre ?
            }

            if (e.keyCode == 13) {
                window.clearTimeout(this.searchTimer);
                this.searchFilter = val;
                this.startSearch();
                return true;
            }

            if (e.keyCode == 38)   //Haut
            {

            }
            if (e.keyCode == 40)   //bas
            {

            }
        }

        // Pas de recherche si la valeur a rechercher n'a pas changé
        if (val == this.searchFilter)
            return false;

        window.clearTimeout(this.searchTimer);
        this.searchFilter = val;
        this.searchTimer = window.setTimeout(that.startSearch, 500);
    };

    // Lance la recherche
    this.startSearch = function (callbackFct) {
        // En mode recherche, on effectue un appel AJAX pour récupérer les valeurs depuis la base.
        // A la suite de cet appel AJAX, la fonction catalogSearchTreatment() récupère les valeurs (XML), les ajoute à l'objet eCatalog (addValue()) et rappelle renderValues()
        // pour rafraîchir l'affichage avec les valeurs récupérées
        top.setWait(true);

        var lWidth = document.body.scrollWidth;
        var lHeight = document.body.scrollHeight;

        // Appel AJAX pour récupérer toutes les valeurs
        var catalogUpdater = new eUpdater(that.pageName, null);
        catalogUpdater.addParam("CatDescId", that.catDescId, "post");
        catalogUpdater.addParam("CatParentId", that.catParentId, "post");
        catalogUpdater.addParam("CatBoundPopup", that.catBoundPopup, "post");
        catalogUpdater.addParam("CatBoundDescId", that.catBoundDescId, "post");
        catalogUpdater.addParam("CatAction", "RefreshDialog", "post");
        catalogUpdater.addParam("sort", Sort, "post");
        catalogUpdater.addParam("CatPopupType", that.catPopupType, "post");
        catalogUpdater.addParam("CatMultiple", (that.catMultiple ? 1 : 0), "post");
        catalogUpdater.addParam("treeview", that.treeview, "post");
        catalogUpdater.addParam("CatSearch", that.searchFilter, "post");
        catalogUpdater.addParam("CatInitialValues", that.initialListElem, "post");
        catalogUpdater.addParam("CatSelectedValues", that.getSelectedListValues(), "post");
        catalogUpdater.addParam("CatEditorJsVarName", that.jsVarNameEditor, "post");



        catalogUpdater.addParam("CatTitle", encode(that.catalogTitle), "post");
        catalogUpdater.addParam("displayFilter", escape(that.displayFilter), "post");
        catalogUpdater.addParam("width", lWidth, "post");
        catalogUpdater.addParam("height", lHeight, "post");
        // HLA - Propage l'information de fromadmin
        catalogUpdater.addParam("From", that.from, "post");

        catalogUpdater.ErrorCallBack = function () { top.setWait(false); };

        catalogUpdater.send(function (oRes, jsVarName, bSilent) {
            dialogCatalogSearchTreatment(oRes, jsVarName, bSilent);
            if (typeof callbackFct === "function") {
                callbackFct(that.highlightedListValues);
            }

        }, that.jsVarName, true);
    };

    // Récupère les lignes de la table de selection à droite
    this.getTableSelItemObject = function () {
        var items = new Array();
        var oTableTarget = document.getElementById("tbCatSelVal");

        if (!oTableTarget)
            return items;

        //var tableRows = oTableTarget.rows;
        var tableRows = oTableTarget.children;

        // Parcours le tableau des selections des valeurs (tableau à droite)
        for (i = 0; i < tableRows.length; i++) {
            var oTrgRow = tableRows[i];
            var trgRowId = oTrgRow.id;

            if (typeof (trgRowId) == 'undefined')
                continue;

            /*if (srcRowId.length < 4)
            continue;
            if (srcRowId.substr(0, 4) != 'sel_')
            continue;*/

            items.push(oTrgRow);
        }

        return items;
    };

    // Retourne les valeurs sélectionnées
    this.getSelectedListValues = function () {
        if (this.treeview) {
            var result = '';

            forEach(this.selectedListValues, function (oItemId) {
                if (result.length > 0)
                    result += ';';
                result += oItemId.substring(oItemId.indexOf("_") + 1);
            });

            return result;
        }
        else if (this.catMultiple) {
            var result = '';

            forEach(this.getTableSelItemObject(), function (oItem) {
                if (result.length > 0)
                    result += ';';
                result += oItem.getAttribute('ednval');
            });

            return result;
        }
        else
            return null;
    };

    // Retourne la liste des valeurs selectionnées
    this.getSelectedListId = function () {
        if (this.catMultiple && !this.treeview) {
            var list = new Array();
            forEach(this.getTableSelItemObject(), function (oItem) {
                list.push(oItem.id);
            });
            return list;
        }
        else {
            return this.selectedListValues;
        }
    };

    // Selection en cat multiple (deplace un item d'une table à une autre)
    // oSrcRow : element de la ligne
    this.multSelItem = function (oSrcRow) {
        var oTableSource = document.getElementById("tbCatVal");
        var oTableTarget = document.getElementById("tbCatSelVal");

        if (!oSrcRow || !oTableSource || !oTableTarget)
            return;

        // Ajoute une nouvelle colonne au tableau de selection à droite sauf si la valeur existe déjà
        if (!document.getElementById(oSrcRow.id + '_sel')) {
            var newRow = this.fullCopy(oSrcRow, true);
            newRow.id += '_sel';
            // Supprime la derniere colonnes (boutons d'actions)
            newRow.childNodes[0].removeChild(newRow.childNodes[0].children[newRow.childNodes[0].children.length - 1]);
            oTableTarget.appendChild(newRow);
        }

        // Rend invisible la colonne source
        oSrcRow.style.display = 'none';
    };

    // Déselection en cat multiple (deplace un item d'une table à une autre)
    // oTrgRow : element de la ligne
    this.multUnSelItem = function (oTrgRow) {
        var oTableSource = document.getElementById("tbCatVal");
        var oTableTarget = document.getElementById("tbCatSelVal");

        if (!oTrgRow || !oTableSource || !oTableTarget)
            return;

        var srcColId = oTrgRow.id.replace('_sel', '');
        // Rend visible la colonne source
        var oSrcRow = document.getElementById(srcColId);
        if (oSrcRow)
            oSrcRow.style.display = 'table-row';
        oTableTarget.removeChild(oTrgRow);
    };

    // Bouton de sélection des valeurs en cat multiple à partir des valeurs en surbrillance
    //  Paramètre e evennement appelant (pour récupérer la valeurs dernièrement sélectionnée suite à demande #19139 : Sur IE8, la sélection d'une valeur de catalogue MULTIPLE par double-clic n'est pas prise en compte.)
    this.btnSelItem = function (e) {
        if (typeof (e) != "undefined")
            this.highlightedListValues.push(this.getClickedObjectId(e));
        // Duplique le tableau pour pouvoir vider le tableau pendant le parcours des lignes
        var selItems = new Array();
        selItems = selItems.concat(this.highlightedListValues);
        forEach(selItems, function (itemId) {
            that.highlightListValue(itemId, false);  // Retire la surbrillance de la valeur
            that.selectListValue(itemId);
        });
    };

    // Bouton de désélection des valeurs en cat multiple à partir des valeurs en surbrillance
    //  Paramètre e evennement appelant (pour récupérer la valeurs dernièrement sélectionnée suite à demande #19139 : Sur IE8, la sélection d'une valeur de catalogue MULTIPLE par double-clic n'est pas prise en compte.)
    this.btnUnSelItem = function (e) {
        if (typeof (e) != "undefined")
            this.highlightedSelListValues.push(this.getClickedObjectId(e));
        // Duplique le tableau pour pouvoir vider le tableau pendant le parcours des lignes
        var selItems = new Array();
        selItems = selItems.concat(this.highlightedSelListValues);

        forEach(selItems, function (itemId) {
            that.highlightListValue(itemId, false);  // Retire la surbrillance de la valeur
            that.selectListValue(itemId);
        });
    };

    // Bouton de sélection de toutes les valeurs
    this.btnAllSelItem = function () {
        var oTableSource = document.getElementById("tbCatVal");

        if (!oTableSource)
            return;

        if (oTableSource.children.length <= 1)   //inférieur ou égal à 1 pour l'entête
            return;

        forEach(oTableSource.children, function (oRow) {
            var oHd = oRow.getAttribute("hd");
            that.highlightListValue(oRow.id, false);  // Retire la surbrillance de la valeur
            that.selectListValue(oRow.id);
        });
    };

    // Bouton de désélection de toutes les valeurs
    this.btnAllUnSelItem = function () {
        forEach(this.getTableSelItemObject(), function (oItem) {
            that.highlightListValue(oItem.id, false);  // Retire la surbrillance de la valeur
            that.selectListValue(oItem.id);
        });
    };

    // Ajout de valeur
    // value : valeur à ajouter
    this.addCatVal = function (value) {

        var url = "mgr/eCatalogManager.ashx";

        var oUpdater = new eUpdater(url, null);
        oUpdater.ErrorCallBack = function () { }
        //Ajout des params
        oUpdater.addParam("operation", catOperationInsert, "post");
        oUpdater.addParam("descid", this.catDescId, "post");
        oUpdater.addParam("pop", this.catPopupType, "post");
        oUpdater.addParam("newlabel", value, "post");
        oUpdater.addParam("id", 0, "post");
        oUpdater.addParam("parentid", this.catParentId, "post");
        oUpdater.send(afterAdding, this.jsVarName);
    };

    // Fonction pour l'edition des libellés catalogue avancé ou l'ajout de nouvelles valeurs
    this.editValueCatAdv = function (action, oLine) {

        if (action == 'edit')
            var strTitre = top._res_151;
        else
            var strTitre = top._res_18;

        this.advAction = action;

        var nHeight = 350;
        //Gestion de la taille de la fenêtre selon le nombre de langues utilisées
        if (this.langUsed) {
            var tabLng = this.langUsed.split(';');
            var i;
            nHeight = 220;

            for (i = 1; i < tabLng.length; i++) {
                nHeight = nHeight + 30;
            }
        }

        var strType = 0;
        var nWidth = 560;
        var textIcon = '0';
        var strUrl = "eCatalogAdvEdit.aspx";

        this.eModalAdvEdit = new eModalDialog(
            strTitre,  // Titre
            strType,   // Type
            strUrl,    // URL
            nWidth,    // Largeur
            nHeight);  // Hauteur
        this.eModalAdvEdit.ErrorCallBack = function () { setWait(false); }

        var valDataId = '0';
        if (oLine) {
            valDataId = oLine.getAttribute('ednid'); // Le dataid de la valeur
            if (((!valDataId) || (typeof (valDataId) == 'undefined') || (valDataId <= 0)) && (action == 'edit')) {
                // Ce cas se produit si on sélectionne la racine (nom de la rubrique) et que l'on clic sur modifier
                eAlert(0, top._res_1487, top._res_6244, '', 500, 200);
                return;
            }
        }
        this.eModalAdvEdit.addParam("CatDescId", this.catDescId, "post");
        this.eModalAdvEdit.addParam("dataID", valDataId, "post");
        // Recup de la zone de recherche lors de l'ajout
        this.eModalAdvEdit.addParam("TxtSearch", this.searchFilter, "post");
        this.eModalAdvEdit.addParam("langUsed", this.langUsed, "post");
        this.eModalAdvEdit.addParam("dataEnabled", this.dataEnabled, "post");
        this.eModalAdvEdit.addParam("dataEdit", this.dataEdit, "post"); // Droit d'edition du dataID
        this.eModalAdvEdit.addParam("fromAdmin", this.from == LOADCATFROM.ADMIN ? "1" : "0", "post"); // Droit d'edition du dataID
        this.eModalAdvEdit.addParam("action", this.advAction, "post");

        this.eModalAdvEdit.show();

        this.eModalAdvEdit.addButton(top._res_29, cancelEditAdv, "button-gray", this.jsVarName); // Annuler
        this.eModalAdvEdit.addButton(top._res_28, validEditAdv, "button-green", this.jsVarName); // Valider
    };

    // Ajout de valeur en cat arbo : création de la branche, instanciation de l'éditeur, et appel de la fonction pour création en base
    // value : valeur à ajouter ; openerElem : element this
    this.editValCatTreeView = function (action, openerElem) {
        var oLine = document.getElementById(this.highlightedListValues[0]);
        if ((oLine) && (typeof (oLine) != 'undefined')) {
            oLine = oLine.querySelector('span[ednid]');
        }
        this.editValueCatAdv(action, oLine);
    };

    this.GetNbOccur = function (lineId, functAfter) {
        top.setWait(true);

        var oSelectedValue = document.getElementById(lineId);
        var oSelectedLbl = document.getElementById(lineId.replace('val_', 'lbl_'));


        var label = "";

        if (oSelectedLbl.innerText != null)
            label = oSelectedLbl.innerText;
        else
            label = oSelectedLbl.textContent;


        var catId = oSelectedValue.getAttribute("ednid");

        var url = "mgr/eCatalogManager.ashx";

        var oUpdater = new eUpdater(url, null);
        oUpdater.ErrorCallBack = function () { }
        //Ajout des params
        oUpdater.addParam("operation", catOperationSearchOcc, "post");
        oUpdater.addParam("descid", this.catDescId, "post");
        oUpdater.addParam("pop", this.catPopupType, "post");
        oUpdater.addParam("label", label, "post");
        oUpdater.addParam("id", catId, "post");
        oUpdater.addParam("langUsed", this.langUsed, "post");
        oUpdater.addParam("treeview", this.treeview, "post");
        oUpdater.send(functAfter, lineId);

    }

    //Permet l'affichage de la PopUp de confirmation de suppression d'une valeurs du catalogue
    this.cfmDelCatVal = function (resNbOccur, lineId) {
        //ALISTER => Demande/Request 88997 Il y a peut-être une meilleure solution que 3 "replace" /There will maybe have a better solution than 3 "replace"
        var msgDisplayVal = document.getElementById(lineId.replace("val_", "lbl_")).innerHTML.replace("&gt;", ">").replace("&lt;","<").replace("&amp;", "&");
        var message = (top._res_6233 + '').replace('<VALUE>', msgDisplayVal);
        var sOccMsg = "";
        var aSentences = resNbOccur.getElementsByTagName("sentence");

        for (var i = 0; i < aSentences.length; i++) {
            sOccMsg += getXmlTextNode(aSentences[i]) + '<br>';
        }
        top.setWait(false);

        eConfirm(1, top._res_1488, message, sOccMsg + ((!this.treeview) ? '' : top._res_6234), 500, 200, function () { window[jsVarName].delCatVal(lineId); }, function () { });
    }

    // Suppression de valeur
    // lineId : id de la TR
    this.delCatVal = function (lineId) {
        var oSelectedValue = document.getElementById(lineId);
        var oSelectedLbl = document.getElementById(lineId.replace('val_', 'lbl_'));

        var label = "";

        if (oSelectedLbl.innerText != null)
            label = oSelectedLbl.innerText;
        else
            label = oSelectedLbl.textContent;


        var catId = oSelectedValue.getAttribute("ednid");

        var url = "mgr/eCatalogManager.ashx";

        var oUpdater = new eUpdater(url, null);
        oUpdater.ErrorCallBack = function () { }
        //Ajout des params
        oUpdater.addParam("operation", catOperationDelete, "post");
        oUpdater.addParam("descid", this.catDescId, "post");
        oUpdater.addParam("pop", this.catPopupType, "post");
        oUpdater.addParam("label", label, "post");
        oUpdater.addParam("id", catId, "post");
        oUpdater.addParam("langUsed", this.langUsed, "post");
        oUpdater.addParam("treeview", this.treeview, "post");
        if (!this.treeview)
            oUpdater.send(afterDelete, oSelectedValue);
        else
            oUpdater.send(afterDelTreeView, this.jsVarName);

    };

    //Suppression d'une valeurs sélectionnée sur un catalogue ARBO
    this.delCatTreeViewVal = function () {
        var oSelValue = document.getElementById(this.highlightedListValues[0]);
        if ((oSelValue) && (typeof (oSelValue) != 'undefined')) {
            oSelValue = oSelValue.querySelector('SPAN[ednid]');
            if ((oSelValue) && (typeof (oSelValue) != 'undefined')) {
                var catId = oSelValue.getAttribute("ednid");
                if ((!catId) || (typeof (catId) == 'undefined') || (catId == 0)) {
                    eAlert(0, top._res_1488, top._res_6243);
                    return;
                }

                eC.GetNbOccur(oSelValue.id, this.cfmDelCatVal);
            }
        }
    };

    // Synchronisation de valeur en cat simple
    this.syncCat = function () {

        var url = "mgr/eCatalogManager.ashx";

        var oUpdater = new eUpdater(url, null);
        oUpdater.ErrorCallBack = function () { }
        //Ajout des params
        oUpdater.addParam("operation", catOperationSynchro, "post");
        oUpdater.addParam("descid", this.catDescId, "post");
        oUpdater.addParam("pop", this.catPopupType, "post");

        oUpdater.send(afterSynchro, this.jsVarName);
    };

    // Affiche la fenêtre d'édition de corps de mail pour les modèles de mail
    this.editMT = function () {
        var oSelValue = document.getElementById(this.highlightedListValues[0]);
        if ((oSelValue) && (typeof (oSelValue) != 'undefined')) {
            var catId = oSelValue.getAttribute("ednid");
            if ((!catId) || (typeof (catId) == 'undefined') || (catId == 0)) {
                eAlert(0, top._res_1488, top._res_6243);
                return;
            }

            top.getMemo(
                "MAIL_TEMPLATE",
                this.openEditMTWindow,
                null,
                null,
                getTabDescid(eC.catDescId),
                eC.catDescId,
                0,
                catId
            );
        }
    };

    this.openEditMTWindow = function (
        strDescId,
        strMailTemplateId,
        strTargetId,
        strMailTemplateName,
        strMemoBody,
        strMemoBodyIsHTML,
        strMemoBodyCSS
    ) {
        var lWidth = document.body.scrollWidth - 150;
        var lHeight = document.body.scrollHeight - 150;

        eC.eModalMTEdit = new eModalDialog(
            eC.catalogTitle,            // Titre
            0,                          // Type
            "eMemoDialog.aspx",         // URL
            lWidth,                     // Largeur
            lHeight);                   // Hauteur

        // Ajustement de la taille du champ en fonction de sa nature Texte brut/HTML
        var bIsHTML = (strMemoBodyIsHTML == "1");
        if (!bIsHTML) {
            lWidth = lWidth - 12;
            lHeight = lHeight + 55;
        }

        eC.eModalMTEdit.ErrorCallBack = launchInContext(eC.eModalMTEdit, eC.eModalMTEdit.hide);
        eC.eModalMTEdit.addParam("DescId", strDescId, "post");
        eC.eModalMTEdit.addParam("CatId", strMailTemplateId, "post");
        eC.eModalMTEdit.addParam("TargetId", strTargetId, "post");
        eC.eModalMTEdit.addParam("Title", encode(eC.catalogTitle), "post");
        eC.eModalMTEdit.addParam("Name", encode(strMailTemplateName), "post");
        eC.eModalMTEdit.addParam("Value", encode(strMemoBody), "post");
        eC.eModalMTEdit.addParam("ValueCSS", encode(strMemoBodyCSS), "post");
        eC.eModalMTEdit.addParam("width", lWidth - 12, "post"); // 12 : marge intérieure par rapport au conteneur de l'eModalDialog
        eC.eModalMTEdit.addParam("height", lHeight - 150, "post"); // 150 : espace réservé à la barre de titre + boutons de l'eModalDialog
        eC.eModalMTEdit.addParam("IsHTML", (bIsHTML ? "1" : "0"), "post");
        eC.eModalMTEdit.addParam("EditorType", "mail", "post");
        eC.eModalMTEdit.addParam("ToolbarType", "mail", "post");
        eC.eModalMTEdit.addParam("EnableTemplateEditor", "0", "post"); // TODO #68 13x - A câbler si besoin

        eC.eModalMTEdit.show();

        eC.eModalMTEdit.addButton(top._res_29, cancelEditMT, "button-gray", eC.jsVarName); // Annuler
        eC.eModalMTEdit.addButton(top._res_28, validateEditMT, "button-green", eC.jsVarName); // Valider
    };

    // Recherche l'élement parent du tag voulu
    // elt : element à dupliquer ; tag : tag de l'élement recherché
    this.findUp = function (elt, tag) {
        if (elt == null)
            return null;

        do {
            if (elt.nodeName && elt.nodeName.search(tag) != -1)
                return elt;
        } while (elt = elt.parentNode);

        return null;
    };

    // Recherche l'élement enfant du tag voulu
    // elt : element à dupliquer ; tag : tag de l'élement recherché
    this.findCheckedChild = function (elt) {
        if (elt == null)
            return null;

        var listChild = elt.getElementsByTagName("*");
        var i;

        for (i = 0; i < listChild.length; i++) {
            if (listChild[i].hasAttribute("chk") && listChild[i].getAttribute("chk") == "1") {
                return listChild[i];
            }
        }

        return null;
    };

    // Recopie un element HTML avec ses classes
    // elt : element à dupliquer ; deep : indicateur de recopie des enfants
    this.fullCopy = function (elt, deep) {
        if (elt == null)
            return;

        var new_elt = elt.cloneNode(deep);
        new_elt.className = elt.className;
        forEach(elt.style,
            function (value, key, object) {
                if (value == null || !value) return;    //!value => ie8 retour autre chose que null !
                if (typeof (value) == "string" && value.length == 0) return;

                new_elt.style[key] = elt.style[key];
            });
        return new_elt;
    };

    // Se positionne sur l'elément selectionné de la liste
    this.showItem = function (p_ID) {
        if (!this.bScrollIntoV) {

            var eltSel = document.getElementById(p_ID);
            var posElement = getAbsolutePosition(eltSel, true);

            var oDiv = document.getElementById("eCEDValues");
            var posParentDiv = getAbsolutePosition(oDiv);

            //On ne met le focus sur l'élément seulement si l'élément n'est pas dans la zone affichée. (GCH : demande #24675)
            if (posElement.y > posParentDiv.h || posElement.y < 0) {
                eltSel.scrollIntoView(false); // le poisitionne au debut
                this.bScrollIntoV = true;
            }
        }
    };

    this.doSearch = function (srch) {
        if (srch.value == "")
            return;

        clearTimeout(toRstSrch);

        //var oDivList = document.getElementById(getAttributeValue(srch, "lst"));
        //if (!oDivList)
        //    return;

        //var oList = oDivList.firstElementChild;
        var oList = document.getElementById(getAttributeValue(srch, "lst"));
        if (!oList)
            return;

        var aList = oList.children[0].children;
        var selElt;
        var selIdx = getNumber(getAttributeValue(document.getElementById(this.selectedListValues[0]), "index"));
        for (var j = 0; j < aList.length; j++) {
            var i = (j + selIdx + 1) % aList.length;
            if (GetText(document.getElementById(aList[i].id.replace("val", "lbl"))).toLowerCase().indexOf(srch.value.toLowerCase()) == 0) {
                selElt = aList[i];
                this.clickVal(null, selElt);

                //var oDivScroll = oList.parentElement;
                //if (getAttributeValue(aList[i], "shlb") == "")
                //    oDivScroll = oList;
                //var posDivScroll = getAbsolutePositionWithScroll(oDivScroll);


                //var posElt = getAbsolutePosition(selElt);
                ////si l'élément sélectionné se trouve en dessous de la zone visible 
                //if (posElt.y + posElt.h > posDivScroll.y + posDivScroll.h) {
                //    newY = posDivScroll.y + posDivScroll.h - posElt.h;
                //    scrollEltToY(oDivScroll, selElt, newY);
                //}
                //    //si l'élément sélectionné se trouve au dessus de la zone visible 
                //else if (posElt.y < posDivScroll.y) {
                //    scrollEltToY(oDivScroll, selElt, posDivScroll.y);
                //}

                break;
            }
        }

        toRstSrch = setTimeout(resetSearch, 500);
    };

}

// fonction hors objet, requis par eUpdater

//Fonction de retour appelée après le lancement de la recherche, cette fonction modifie l’icône de recherche en croix grise, et affiche le contenu de la recherche.
function dialogCatalogSearchTreatment(oRes, jsVarName, bSilent) {
    top.setWait(false);
    var strCatalogSearchSuccess = parent.getXmlTextNode(oRes.getElementsByTagName("result")[0]);
    if (strCatalogSearchSuccess != "SUCCESS") {
        if (!bSilent) {
            eAlert(0, top._res_225, top._res_6235);
        }
        return;
    }

    // Parent car getXmlTextNode de eTools.js est incluse par eMain.aspx
    var strUpdatedHTML = parent.getXmlTextNode(oRes.getElementsByTagName("html")[0]);
    var strUpdatedJS = parent.getXmlTextNode(oRes.getElementsByTagName("js")[0]);

    if (document.getElementById('eCEDValues') && strUpdatedHTML != '') {
        document.getElementById('eCEDValues').innerHTML = strUpdatedHTML;
    }

    var strUpdatedHTMLRight = parent.getXmlTextNode(oRes.getElementsByTagName("htmlRight")[0]);
    if (document.getElementById('eCEDSelValues') && strUpdatedHTMLRight != '') {
        document.getElementById('eCEDSelValues').innerHTML = strUpdatedHTMLRight;
    }

    var oBtnSrch = document.getElementById("eBtnSrch");
    if (oBtnSrch) {
        if (document.getElementById("eTxtSrch").value == '')
            document.getElementById("eBtnSrch").className = "logo-search icon-magnifier";
        else if (parent.getXmlTextNode(oRes.getElementsByTagName("nbResults")[0]) == "0" && parent.getXmlTextNode(oRes.getElementsByTagName("searchValue")[0]) != "")
            document.getElementById("eBtnSrch").className = "logo-search-croix icon-edn-cross";
        else
            oBtnSrch.className = "logo-search-croix icon-edn-cross";
    }

    // Lance le JS
    eval(strUpdatedJS);
}



//Traitement après ajout d'une valeur de catalogue 
function afterAdding(oRes, jsVarName) {
    var catalogObject = window[jsVarName];
    var success = (getXmlTextNode(oRes.getElementsByTagName("success")[0]) == "1");

    if (catalogObject && success) {
        // Mode edition de catalogue avancé on lance la methode "afterRenameCatAdv" pour mettre a jour la liste et les MRU
        if (catalogObject.catPopupType == '3' && catalogObject.advAction == 'edit')
            afterRenameCatAdv(oRes, jsVarName);

        catalogObject.startSearch(function (highlightedValues) {

            for (var i = 0; i < highlightedValues.length; i++) {
                catalogObject.highlightListValue(highlightedValues[i], true);
            }
            // Called function after search
            var id = (getXmlTextNode(oRes.getElementsByTagName("id")[0]));
            var valueElt;
            if (!catalogObject.treeview) {
                valueElt = document.getElementById("val_" + id);
            }
            else {
                valueElt = document.getElementById("eTVBLV_" + id);
            }
            if (valueElt)
                valueElt.click();
        });
    }

    if (success != "1") {
        alert(getXmlTextNode(oRes.getElementsByTagName("message")[0]));
        return;
    }
}
// Traitement après suppression d'une valeur d'un catalogue arborescent
function afterDelTreeView(oRes, jsVarName) {
    var catalogObject = window[jsVarName];
    var success = (getXmlTextNode(oRes.getElementsByTagName("success")[0]) == "1");

    if (catalogObject && success) {
        catalogObject.startSearch();
    }

    if (success != "1") {
        alert(getXmlTextNode(oRes.getElementsByTagName("message")[0]));
        return;
    }
}

// ---------------SUPPRESSION DE VALEUR
function afterDelete(oRes, oSelectedValue, bDontCheckContent) {

    var strDeleteSuccess = getXmlTextNode(oRes.getElementsByTagName("success")[0]);

    if (strDeleteSuccess != "1") {
        alert(getXmlTextNode(oRes.getElementsByTagName("message")[0]));
        return;
    }

    // Suppression de la ligne de la valeur dans liste de valeurs complètes
    oSelectedValue.parentNode.removeChild(oSelectedValue);

    var xmlDeleteReport = oRes.getElementsByTagName("ednOperationReport")[0];

    //Suppression de la valeur et des valeurs enfants dans le tableau html de la grille de résultat.
    // On fait une boucle qui part du bas et remonte pour mettre à jour les cellules enfant avant les parents
    for (k = xmlDeleteReport.getElementsByTagName("Delete").length - 1; k >= 0; k--) {

        var xmlCatalog = xmlDeleteReport.getElementsByTagName("Delete")[k];

        var xmlDeletedValue = xmlCatalog.getElementsByTagName("DeletedValue")[0];
        var xmlImpactedDescid = xmlDeletedValue.getElementsByTagName("ImpactedDescid")[0];


        var strImpactedDescid = getXmlTextNode(xmlImpactedDescid);
        var aImpactedDescid = strImpactedDescid.split(';');

        var strDisplayValue = xmlDeletedValue.getAttribute("DisplayValue");
        var strDeletedId = xmlDeletedValue.getAttribute("Id");

        var strDbv = xmlDeletedValue.getAttribute("Dbv");

        var strParentDbv = xmlDeletedValue.getAttribute("ParentDbv");

        var strChildrenDescid = getXmlTextNode(xmlCatalog.getElementsByTagName("ChildrenDescid")[0]);


        //bDontCheckContent est utilisé lors de l'appel suite à la synchro
        // en effet la synchro peut supprimer des valeurs inutilisées et il est alors inutile de parcourir le tableau html
        if (strDbv != "" && !bDontCheckContent) {
            for (j = 0; j < parent.document.getElementsByTagName("td").length; j++) {
                var cell = parent.document.getElementsByTagName("td")[j];
                if (cell.getAttribute('ename') == null || cell.getAttribute('ename') == "") {
                    continue;
                }

                var aEName = cell.getAttribute('ename').split('_');
                var descid = aEName[aEName.length - 1];


                if ((';' + strImpactedDescid + ';').indexOf(';' + descid + ';') > -1 &&
                    ((';' + cell.getAttribute('dbv') + ';').indexOf(';' + strDbv + ';') > -1
                        || (!cell.getAttribute('dbv') && (';' + cell.innerHTML + ';').indexOf(';' + strDbv + ';') > -1)
                    ) &&
                    (cell.getAttribute('pdbv') == null || cell.getAttribute('pdbv') == strParentDbv)
                ) {


                    if (cell.innerHTML == strDisplayValue) {
                        cell.innerHTML = '';
                    }
                    else {
                        cell.innerHTML = (';' + cell.innerHTML + ';').replace(';' + strDisplayValue + ';', ';');
                        cell.innerHTML = cell.innerHTML.substring(1, cell.innerHTML.length - 1);
                    }

                    if (cell.getAttribute('dbv') == strDbv) {
                        cell.setAttribute('dbv', '');
                    }
                    else {
                        var strNew = (';' + cell.getAttribute('dbv') + ';').replace(';' + strDbv + ';', ';');
                        strNew = strNew.substring(1, strNew.length - 1)
                        cell.setAttribute('dbv', strNew);
                    }
                }
                else {
                    if ((';' + strChildrenDescid + ';').indexOf(';' + descid + ';') > -1
                        && cell.getAttribute('pdbv') == strDbv) {
                        cell.setAttribute('pdbv', '');
                    }
                }
            }
        }


        // Suppression de la / des valeur(s) dans les mru.
        var eParam = parent.document.getElementById("eParam");

        if (!eParam) { return; }

        if (eParam.contentWindow && typeof (eParam.contentWindow.GetMruParam) == "function") {
            var eParamWindow = eParam.contentWindow;
        }

        if (!eParamWindow) { return; }

        for (i = 0; i < aImpactedDescid.length; i++) {

            var mruValues = eParamWindow.GetMruParam(aImpactedDescid[i]);

            if (mruValues == '') { continue; }

            var aCurrentMru = mruValues.split("$|$");
            var mruNewValue = '';
            for (j = 0; j < aCurrentMru.length; j++) {
                if (aCurrentMru[j].split(';')[0] != strDeletedId) {
                    if (mruNewValue != '') {
                        mruNewValue += "$|$";
                    }

                    mruNewValue += aCurrentMru[j];
                }
            }

            eParamWindow.SetMruParam(aImpactedDescid[i], mruNewValue);
        }
    }
}
// --------------- SUPPRESSION DE VALEUR FIN

//---------------- SYNCHRO
function afterSynchro(oRes, jsVarName) {
    var catalogObject = window[jsVarName];

    var success = (getXmlTextNode(oRes.getElementsByTagName("success")[0]) == "1");

    if (catalogObject && success) {
        // on utilise cette fonction pour reloader;
        eAlert(4, top._res_225, top._res_6227, '', null, null, function () { catalogObject.startSearch(); });
    }
    else {
        eAlert(0, top._res_225, top._res_72, getXmlTextNode(oRes.getElementsByTagName("message")[0]));
    }
}

//---------------- SYNCHRO FIN

// Fonctions de la fenêtre d'édition de modèle de mail
function cancelEditMT(jsVarName) {
    var catalogObject = window[jsVarName];
    catalogObject.eModalMTEdit.hide();
    return false;
};

// Fonctions de la fenêtre d'édition de modèle de mail
function validateEditMT(jsVarName) {

    //var mtUpdater = new eUpdater("mgr/eMailManager.ashx", 0);
    // TODO: passage des params
    mtUpdater.send(afterMTUpdate, jsVarName);
}

function afterMTUpdate(oRes, jsVarName) {
    var strSuccess = getXmlTextNode(oRes.getElementsByTagName("success")[0]);

    if (strSuccess == "1") {
        var catalogObject = window[jsVarName];
        catalogObject.eModalMTEdit.hide();

        //setWait(false);
    }
    else {
        //setWait(false);
        var sErrorMsg = getXmlTextNode(oRes.getElementsByTagName("message")[0]);
        eAlert(0, top._res_225, top._res_6235, sErrorMsg + '<br>' + top._res_6236);
    }
};

// Annuler fenetre Catalogues Avancés
function cancelEditAdv(jsVarName, frmId) {
    var catalogObject = window[jsVarName];
    catalogObject.eModalAdvEdit.hide();
    return false;
}

//Validation fenetre modif et ajout dans les catalogues avancés
function validEditAdv(jsVarName, frmId) {
    var catalogObject = window[jsVarName];

    // Recuperation de l'Iframe de modif e d'ajout
    var oFrmEditAdv = catalogObject.eModalAdvEdit.getIframe();

    var url = "mgr/eCatalogManager.ashx";
    var oUpdater = new eUpdater(url, null);
    oUpdater.ErrorCallBack = function () { }

    // recuperation des champs à partir de l^'Iframe de modi ou d'ajout
    var oId_lbl = ''; //ID de l'input libellé
    var oId_tip = ''; //ID de l'input ToolTip
    var oId_tipFormat = ''; //ID du checkbox Format
    var olabel_tip = ''; // Valeur du libellé recupéré
    var olabel_tip = ''; // Valeur du ToolTip recupéré
    //  var otipFormat = ''; // Format du ToolTip recupéré

    var tabLang = catalogObject.langUsed.split(';');

    // on va boucler pour récuperer les langues utilisées
    for (var i = 0; i < tabLang.length; i++) {
        if (tabLang[i].toString() == '10') {
            oId_lbl = 'lbl_' + tabLang[i].toString();
            oId_tip = 'tip_' + tabLang[i].toString();
            //    oId_tipFormat = 'chkFormat_' + tabLang[i].toString();
        }
        else {
            oId_lbl = 'lbl_0' + tabLang[i].toString();
            oId_tip = 'tip_0' + tabLang[i].toString();
            //    oId_tipFormat = 'chkFormat_0' + tabLang[i].toString()
        }

        if (oFrmEditAdv.document.getElementById(oId_lbl)) {
            olabel_lbl = oFrmEditAdv.document.getElementById(oId_lbl).value;
            olabel_tip = encode(oFrmEditAdv.document.getElementById(oId_tip).value);
            //        if (oFrmEditAdv.document.getElementById(oId_tipFormat).checked)
            //            otipFormat = '1';
            //        else
            //            otipFormat = '0';

            //Parametres de l'updater
            oUpdater.addParam(oId_lbl, olabel_lbl, "post");
            oUpdater.addParam(oId_tip, olabel_tip, "post");
            // oUpdater.addParam(oId_tipFormat, otipFormat, "post");
        }

    }

    // champ code
    var oData = (oFrmEditAdv.document.getElementById('inpCode')) ? oFrmEditAdv.document.getElementById('inpCode').value : "";
    // champ désactivé     // En mode administrateur on à la case "désactivé"
    var oChkActive = (oFrmEditAdv.document.getElementById('chkActiv')) ? oFrmEditAdv.document.getElementById('chkActiv').getAttribute('chk') : "";
    //Champ DataId
    var catId = (oFrmEditAdv.document.getElementById('hDataId')) ? oFrmEditAdv.document.getElementById('hDataId').value : "0";

    // recuperation des elements pour fabriquer la requette
    if (catalogObject.advAction == 'edit') {
        oUpdater.addParam("operation", catOperationChange, "post");
    }
    // Mode ajout de nouvelle valeur
    else if (catalogObject.advAction == 'add') {
        oUpdater.addParam("operation", catOperationInsert, "post");
    }

    //Ajout des params
    oUpdater.addParam('langUsed', catalogObject.langUsed, "post");
    oUpdater.addParam('data', oData, "post");
    oUpdater.addParam('hidden', oChkActive, "post");
    oUpdater.addParam("descid", catalogObject.catDescId, "post");
    oUpdater.addParam("pop", catalogObject.catPopupType, "post");
    oUpdater.addParam("parentid", catalogObject.catParentId, "post");
    oUpdater.addParam("id", catId, "post");
    oUpdater.addParam("treeview", catalogObject.treeview, "post");

    oUpdater.send(afterAdding, jsVarName);

    catalogObject.eModalAdvEdit.hide();
    return true;
}

// Donne le focus à la textbox de recherche
function setFocus() {
    var bIsTablet = false;
    try {
        if (typeof (isTablet) == 'function')
            bIsTablet = isTablet();
        else if (typeof (top.isTablet) == 'function')
            bIsTablet = top.isTablet();
    }
    catch (e) {

    }

    if (!bIsTablet) {
        if (document.getElementById('eTxtSrch'))
            document.getElementById('eTxtSrch').focus();

    }
}

// Cette fonction est lancée après avoir renommé une valeur de catalogue avancé
// Elle à pour but de mettre à jour la liste affichée car la mise à jour en base à déja été effectuée à ce moment
// Elle renomme donc les valeurs de la liste et modifie aussi les MRU
// NBA 10-04-2012
function afterRenameCatAdv(oRes, jsVarName) {

    var catalogObject = window[jsVarName];

    var strSuccess = getXmlTextNode(oRes.getElementsByTagName("success")[0]);

    if (strSuccess == "1") {

        var xmlReport = oRes.getElementsByTagName("ednOperationReport")[0];
        //Suppression de la valeur et des valeurs enfants dans le tableau html de la grille de résultat.
        for (k = 0; k < xmlReport.getElementsByTagName("Rename").length; k++) {
            var xmlCatalog = xmlReport.getElementsByTagName("Rename")[k];

            var xmlRenamedValue = xmlCatalog.getElementsByTagName("RenamedValue")[0];
            var xmlImpactedDescid = xmlRenamedValue.getElementsByTagName("ImpactedDescid")[0];

            var strImpactedDescid = getXmlTextNode(xmlImpactedDescid);
            var aImpactedDescid = strImpactedDescid.split(';');

            // TODO GESTION DES ENFANTS
            var strChildrenDescid = getXmlTextNode(xmlCatalog.getElementsByTagName("ChildrenDescid")[0]);
            var strNewLabel = xmlRenamedValue.getAttribute("NewLabel");
            var strOldLabel = xmlRenamedValue.getAttribute("Label");
            var strId = xmlRenamedValue.getAttribute("Id");
            var strDbv = xmlRenamedValue.getAttribute("Dbv");
            var strNewDbv = xmlRenamedValue.getAttribute("NewDbv");

            var strParentDbv = xmlRenamedValue.getAttribute("ParentDbv");

            // recuperation de la table principale avec les valeurs pour reduire le champs de recherche todo
            //  var _idTable ='mt_'+ parent.nGlobalActiveTab;
            //  var _table = parent.document.getElementById(_idTable);

            //   for (j = 0; j < _table.cells.length; j++) {
            //   var cell = _table.cells[j];

            // on recupère toutes les TD de la liste !!!
            var _allTD = parent.document.getElementsByTagName("td");
            for (j = 0; j < _allTD.length; j++) {

                var cell = _allTD[j];

                if (cell.getAttribute('ename') == null || cell.getAttribute('ename') == "") {
                    continue;
                }

                var aEName = cell.getAttribute('ename').split('_');
                var descid = aEName[aEName.length - 1];

                if (isNaN(descid))
                    continue;

                if ((';' + cell.getAttribute('dbv') + ';').indexOf(';' + strId + ';') > -1) {
                    cell.innerHTML = cell.innerHTML.replace(strOldLabel, strNewLabel);

                    if (cell.getAttribute('dbv') != null) {
                        var strNew = (';' + cell.getAttribute('dbv') + ';').replace(';' + strId + ';', ';' + strNewDbv + ';');
                        strNew = strNew.substring(1, strNew.length - 1)
                        cell.setAttribute('dbv', strNew);
                    }
                }

                else {
                    // Mise à jour des enfants dans la liste
                    if ((';' + strChildrenDescid + ';').indexOf(';' + descid + ';') > -1
                        && cell.getAttribute('pdbv') == strDbv
                    ) {

                        cell.setAttribute('pdbv', strNewDbv);
                    }
                }
            }
        }

        //Renomme la valeur dans les mru.
        var eParam = parent.document.getElementById("eParam");

        if (!eParam) { return; }

        if (eParam.contentWindow && typeof (eParam.contentWindow.GetMruParam) == "function") {
            var eParamWindow = eParam.contentWindow;
        }

        if (!eParamWindow) { return; }

        for (i = 0; i < aImpactedDescid.length; i++) {
            /* Dans le cas ou il n'y a pas de descid impacté, getXmlNode renvoie "", qui splitté retourne un tableau de 1 valeur vide
            Il faut donc gérer le cas ou aImpactedDescid a une length de 1, mais une valeur à chaine vide;
            */
            if (aImpactedDescid[i] == "")
                continue;

            var mruValues = eParamWindow.GetMruParam(aImpactedDescid[i]);

            if (mruValues == '') { continue; }

            var aCurrentMru = mruValues.split("$|$");
            var mruNewValue = '';
            for (j = 0; j < aCurrentMru.length; j++) {
                if (j > 0)
                    mruNewValue += "$|$";

                var aValues = aCurrentMru[j].split(';')
                if (aValues[0] == strId) {
                    aValues[1] = strNewLabel;
                    mruNewValue += aValues[0] + ';' + aValues[1];
                }
                else {
                    mruNewValue += aCurrentMru[j];
                }
            }
            eParam.contentWindow.SetMruParam(aImpactedDescid[i], mruNewValue);
        }
        //}

        //renomme la valeur dans liste de valeurs complètes
        //   target.innerHTML = strNewLabel;
        //    target.parentNode.setAttribute('ednval', strNewDbv);
    }
    else {
        var sErrorMsg = getXmlTextNode(oRes.getElementsByTagName("message")[0]);
        alert(top._res_6237 + "\n" + sErrorMsg + "\n" + top._res_6236);
    }
}

//eTabsFieldsSelectjs a regrouper :

function initDragOpt() {
    dragOpt.bCssCustom = true;  //Pour indiquer que cet appel de drag and drop gère lui même les classes css
    dragOpt.customSourceElement = function (oSrcElm) {
        return oSrcElm.parentNode.parentNode;
    };
    dragOpt.SrcList = document.getElementById("eCEDValues");    //ONGLET de GAUCHE
    dragOpt.TrgtList = document.getElementById("eCEDSelValues");  //ONGLET de DROITE
    dragOpt.FldSel = false;
    dragOpt.init();
    dragOpt.customDestMouseUp = function (e) {
        var oParent = eC.findUp(dragOpt.origElt, "DIV");
        if (oParent != dragOpt.DestList)
            eC.clickVal(e, dragOpt.origElt, true);
        return false;
    };
}

function strtDrag(e) {
    dragOpt.dragStart(e);
}

function setCssList(oList, css1, css2) {
    if (oList == null)
        return;
    var css = css1;
    var oElements = oList.getElementsByTagName("div");
    for (var i = 0; i < oElements.length; i++) {
        if (oElements[i].getAttribute("syst") == null) {
            if (css == css1)
                css = css2;
            else /*if (css == css2)*/
                css = css1;

            oElements[i].className = css;
            oElements[i].setAttribute("oldCss", css);
        }
    }
}

function doInitSearch(oList, e) {

    var bIsTablet = false;
    try {
        if (typeof (isTablet) == 'function')
            bIsTablet = isTablet();
        else if (typeof (top.isTablet) == 'function')
            bIsTablet = top.isTablet();
    }
    catch (e) {

    }

    if (bIsTablet)
        return;

    var eltSearch = document.getElementById("srch");
    if (!eltSearch) {
        return;
    }

    eltSearch.focus();
    eltSearch.value = "";

    eltSearch.setAttribute("lst", oList.id);
}

var toRstSrch;

function resetSearch() {
    document.getElementById("srch").value = "";
}


function adjustColWidth(sValSelector, nDefaultWidth) {
    var oRule = getCssSelector("eCatalog.css", sValSelector);

    try {
        oRule.style.width = "";
    }
    catch (e) {
    }

    var aLiVals = document.querySelectorAll(sValSelector);
    var nWidth = 0;
    if (nDefaultWidth)
        nWidth = nDefaultWidth;

    if (aLiVals.length == 0) {
        return;
    }
    for (var i = 0; i < aLiVals.length; i++) {
        var li = aLiVals[i];
        if (li.offsetWidth > nWidth)
            nWidth = li.offsetWidth;
    }

    try {
        oRule.style.maxWidth = nWidth + "px";
        oRule.style.minWidth = nWidth + "px";
    }
    catch (e) {
    }
}


function adjustColsWidth() {
    adjustColWidth("li.valwidth", 150);
    adjustColWidth("li.maskwidth", 150);
    adjustColWidth("li.datawidth");
    adjustColWidth("li.idwidth");
    adjustColWidth("li.diswidth");

    var tbCatVal = document.getElementById("tbCatVal");
    var eCEDValues = document.getElementById("eCEDValues");
    EnlargeColsIfNeeded(eCEDValues, tbCatVal, true);

    //pour les catalogues multiples
    tbCatVal = document.getElementById("tbCatSelVal");
    eCEDValues = document.getElementById("eCEDSelValues");
    if (tbCatVal && eCEDValues)
        EnlargeColsIfNeeded(eCEDValues, tbCatVal, false);

}

function EnlargeColsIfNeeded(eCEDValues, tbCatVal, bBtn) {

    if (eCEDValues.scrollWidth <= tbCatVal.offsetWidth)
        return;

    // on agrandit le contenu pour qu'il s'ajuste au conteneur
    var aLiHead = tbCatVal.querySelectorAll("li[hd]>ul>li");
    if (aLiHead.length > 0) {
        var nbCol = aLiHead.length;                              //-1 parce que la dernière colonne est celle des boutons
        if (bBtn)
            nbCol--;
        var increment = Math.floor((eCEDValues.scrollWidth - tbCatVal.offsetWidth) / nbCol);

        for (var i = 0; i < nbCol; i++) {
            var sClass = aLiHead[i].className;
            var oRule = getCssSelector("eCatalog.css", "li." + sClass);
            var nWidth = getNumber(oRule.style.width.replace("px", ""));
            nWidth += increment;
            oRule.style.width = nWidth + "px";
        }
    }
    else {
        var increment = eCEDValues.scrollWidth - tbCatVal.offsetWidth - 1;
        // catalogues V7 : 
        var oRule = getCssSelector("eCatalog.css", "li.maskwidth");
        var nWidth = getNumber(oRule.style.width.replace("px", ""));
        nWidth += increment;
        oRule.style.width = nWidth + "px";
    }

}