function eCatalogUser(jsVarName, modalVarName, sPageName, catMultiple, descId, fullUserList, showUserOnly, showEmptyGroup, useGroup, showValuePublicRecord, showValueEmpty, selectAllUsersOption) {
    var that = this; // pointeur vers l'objet eCatalog lui-même, à utiliser à la place de this dans les évènements onclick, setTimeout... (où this correspond alors à l'objet cliqué, à window...)


    this.paramValues = { "set": "0" };

    this._jsVarName = jsVarName;
    this.jsVarNameEditor = '';
    this._modalVarName = modalVarName;
    this._pageName = sPageName;
    that.onlyAdmin = false;

     
    this._bMultiple = (catMultiple == "1");
    this._descId = descId;
    this._bFullUserList = (fullUserList == "1");
    this._bShowUserOnly = (showUserOnly == "1");
    this._bShowEmptyGroup = (showEmptyGroup == "1");
    this._bUseGroup = (useGroup == "1");
    this._showValuePublicRecord = (showValuePublicRecord == "1");
    this._showValueEmpty = (showValueEmpty == "1");
    this._bSelectAllUsers = (selectAllUsersOption == "1");
    //Valeurs sélectionnée
    this._selectedListValues = new Dictionary();

    //Classe affichée lorsqu'une valeurs est sélectionnée
    this._selectedClassName = "eTVS";
    this._searchTimer = null;
    this.iFrameId = "";
    /*
    nDisplayMode :
    0 - Afficher tous les utilisateurs    
    1 - Afficher les utilisateurs sélectionnés
    */
    this._nDisplayMode = 0;
    //variable afficher les utilisateurs masqués
    this._bDisplayMsk = false;

    // Fonction appelée lors d'une recherche de valeurs
    this.FindValues = function (e, val) {

        var oBtnSrch = document.getElementById("eBtnSrch");

        if (oBtnSrch) {
            if (val.length < 3 && e != null && e.keyCode != 13 && getAttributeValue(oBtnSrch, 'srchState') == 'off')
                return false;
        }
        else {
            if (val.length != 0 && val.length < 3 && e != null && e.keyCode != 13)
                return false;
        }

        if (oBtnSrch) {
            if (getAttributeValue(oBtnSrch, 'srchState') != 'on' && val != '') {
                oBtnSrch.className = "icon-edn-cross srchFldImg";
                oBtnSrch.setAttribute('srchState', 'on');
            }
            else if (getAttributeValue(oBtnSrch, 'srchState') == 'on' && val == "") {
                oBtnSrch.src = "icon-edn-cross srchFldImg";
                oBtnSrch.setAttribute('srchState', 'off');
                setFocus();
            }
        }

        if (e != null) {
            if (e.keyCode == 27) {
                // Echap : annuler la fenêtre ?
            }

            if (e.keyCode == 13) {
                window.clearTimeout(this._searchTimer);
                this._searchFilter = val;
                this.StartSearch();
                return true;
            }

        }

        // Pas de recherche si la valeur a rechercher n'a pas changé
        if (val == this._searchFilter)
            return false;

        window.clearTimeout(this._searchTimer);
        this._searchFilter = val;
        this._searchTimer = window.setTimeout(this.StartSearch, 500);
    };

    // Bouton pour lancer ou annuler la recherche
    this.BtnSrch = function () {
        var oBtnSrch = document.getElementById("eBtnSrch");

        if (oBtnSrch && getAttributeValue(oBtnSrch, 'srchState') == 'on')
            document.getElementById('eTxtSrch').value = '';

        this.FindValues(null, document.getElementById('eTxtSrch').value);
    };

    // Lance la recherche
    this.StartSearch = function () {


        // En mode recherche, on effectue un appel AJAX pour récupérer les valeurs depuis la base.
        // A la suite de cet appel AJAX, la fonction catalogSearchTreatment() récupère les valeurs (XML), les ajoute à l'objet eCatalog (addValue()) et rappelle renderValues()
        // pour rafraîchir l'affichage avec les valeurs récupérées        
        // Appel AJAX pour récupérer toutes les valeurs
        var catalogUpdater = new eUpdater(that._pageName, null);
        top.setWait(true);

        catalogUpdater.ErrorCallBack = function () { top.setWait(false); };
        catalogUpdater.addParam("multi", (that._bMultiple) ? "1" : "0", "post");
        catalogUpdater.addParam("selected", GetSelectedIds(that._jsVarName), "post");
        catalogUpdater.addParam("modalvarname", this._modalVarName, "post");
        catalogUpdater.addParam("action", "REFRESH_DIALOG", "post");
        catalogUpdater.addParam("CatSearch", that._searchFilter, "post");
        catalogUpdater.addParam("descid", that._descId, "post");
        catalogUpdater.addParam("fulluserlist", (that._bFullUserList) ? "1" : "0", "post");
        catalogUpdater.addParam("showemptygroup", (that._bShowEmptyGroup) ? "1" : "0", "post");
        catalogUpdater.addParam("showuseronly", (that._bShowUserOnly) ? "1" : "0", "post");
        catalogUpdater.addParam("usegroup", (that._bUseGroup) ? "1" : "0", "post");
        catalogUpdater.addParam("showvaluepublicrecord", that._showValuePublicRecord, "post");
        catalogUpdater.addParam("showvalueempty", that._showValueEmpty, "post");
        catalogUpdater.addParam("onlyadmin", (that.onlyAdmin) ? "1" : "0", "post");
        catalogUpdater.addParam("displaymode", (that._nDisplayMode), "post"); //ALISTER => Demande 67 080

        if (that.paramValues.ShowGroupOnly)
            catalogUpdater.addParam("showgrouponly", "1", "post");

        catalogUpdater.send(DialogCatalogSearchTreatment, that._jsVarName, true);

    };


    this.ClickLabel = function (oSpan, dontSelChild, chkCustomOpt) {

        dontSelChild = dontSelChild || false;
        chkCustomOpt = chkCustomOpt || false;

        var chkID = "";
        var suffixCM = (chkCustomOpt) ? "CM" : "";

        var oEdnId = getAttributeValue(oSpan.parentNode, "ednId");
        if (oEdnId) {
            chkID = "chkValue" + suffixCM + "_" + oEdnId;
            var oChkBx = document.getElementById(chkID);
            if (oChkBx) {
                chgChk(oChkBx);
                that.ClickValM(oChkBx, dontSelChild, chkCustomOpt);
                that.SetSel(oSpan);

            }
        }
    };


    this.selectedItem = null;

    //Click sur un élément en mode sélection unique
    this.SetSel = function (oDiv, sel) {

        if ((sel == 'undefined') || (typeof (sel) == 'undefined'))
            sel = (oDiv.className.indexOf(this._selectedClassName) < 0);

        this.selectedItem = null;
        if (sel) {
            tabDiv = "ResultDivCustomValues;ResultDiv".split(";");

            for (var i = 0; i < tabDiv.length; i++) {

                currentDiv = document.getElementById(tabDiv[i]);
                if (currentDiv) {
                    var oAllList = currentDiv.getElementsByTagName("span");

                    for (var j = 0; j < oAllList.length; j++) {
                        this.SetSel(oAllList[j], false);
                    }
                }
            }

            if (oDiv.className.indexOf(this._selectedClassName) < 0)
                oDiv.className += " " + this._selectedClassName;

            this.selectedItem = getAttributeValue(oDiv, "ednid");
        }
        else {
            removeClass(oDiv, this._selectedClassName);

        }
    };


    //Méthode appelée au double clique sur une valeur du catalogue utilisateur
    // e : evennement de l'appelant
    this.SetSelDblClick = function (e) {
        /*Sélection de la valeur double clicquée*/
        var oOrigNode = null;
        if (e && e.target)
            oOrigNode = e.target;
        else
            oOrigNode = window.event.srcElement;
        /********RECUPERATION DU CONTE XTE********/
        /*exemple de structure : <li id="eTVB_25"><span class="eTVSBusr">    </span><span class="eTV_LCL">    </span><span ednmsk="0" ednid="25" onclick="eCU.ClickValS(this);" id="eTVBLV_25" class="eTVP">  <span onclick="eCU.ClickLabel(this);" id="eTVBLVT_25">RAPHAEL</span></span></li>*/
        if ((oOrigNode.nodeName.toLowerCase() == "span") && (getAttributeValue(oOrigNode, "ednid")) == ""
            && oOrigNode.parentNode.nodeName.toLowerCase() == "li")  //Si on a sélectionné des élément de la branche on sélectionne son li parent pour ensuite passé dans le "if" qui suit
            oOrigNode = oOrigNode.parentNode;
        if (oOrigNode.nodeName.toLowerCase() == "li") { //si on a cliqué sur l'élément LI conteneur du span de l'utilisateur on sélectionne le span principal
            oOrigNode = oOrigNode.childNodes[oOrigNode.childNodes.length - 1];
        }
        else if ((oOrigNode.nodeName.toLowerCase() == "span") && (getAttributeValue(oOrigNode, "ednid")) == "") //Si on a cliqué dans le libellé, on selectionne son parent
            oOrigNode = oOrigNode.parentNode;
        /*****************************************/
        if (!this.IsSelectionable(oOrigNode))
            return;
        var oDiv = oOrigNode;
        this.SetSel(oDiv, true);
        this.ClickVal(oDiv, true);
        /***************************************/
        /*Validation, fermeture,... et traitement si besoin*/
        var oUserModal = top.eTabCatUserModalObject[this.iFrameId];

        oUserModal.CallOnOk(oUserModal._argsOnOk);
        /***************************************/

    };


    this.IsSelectionable = function (oOrigNode) {
        if ((getAttributeValue(oOrigNode, "ednid")).indexOf("G") == 0 && (!that._bUseGroup && !that._bMultiple))
            return false;
        return true;
    };


    this.DisplayChange = function (bchecked, nSelDisplayMode, bApplyChange) {
        //ALISTER => Demande 67 080, bApplyChange pour vérifier si l'on fait une recherche, si oui on rafraîchit
        if (bchecked && (nSelDisplayMode != this._nDisplayMode) || bApplyChange) {
            /*
            nDisplayMode :
            0 - Afficher tous les utilisateurs
            1 - Ne pas afficher les utilisateurs masqués
            2 - Afficher les utilisateurs non sélectionnés
            */
            switch (nSelDisplayMode) {
                case 0: //Afficher tous les utilisateurs
                    this.DisplayModeAllUser();
                    break;
                case 2: // Afficher les utilisateurs non sélectionnés
                    this.DisplayModeSelectedUser("0");
                    break;
                case 1: //Afficher les utilisateurs sélectionnés
                default:
                    this.DisplayModeSelectedUser("1");
                    break;
            }
            this.DisplayUserMasked();
            this._nDisplayMode = nSelDisplayMode;
        }
    };
    //nDisplayMode : 0 - Afficher tous les utilisateurs
    this.DisplayModeAllUser = function () {

        var oSpanList = document.getElementById("ResultDiv").getElementsByTagName("span");
        for (i = 0; i < oSpanList.length; i++) {
            if (oSpanList[i].parentNode.style.display == "none")
                oSpanList[i].parentNode.style.display = "";
        }
    };

    //nDisplayMode : 1 - Afficher les utilisateurs sélectionnés
    this.DisplayModeSelectedUser = function (chkValue) {

        if (!chkValue)
            chkValue = "1";

        var oCBList = document.getElementById("ResultDiv").getElementsByTagName("A");
        //Masque toutes les valeurs tout en récupèrant les valeurs cochées
        var oChkList = new Array();
        for (i = 0; i < oCBList.length; i++) {

            if (oCBList[i].className.indexOf("rChk") >= 0) {

                if (oCBList[i].parentNode.parentNode.style.display != "none")
                    oCBList[i].parentNode.parentNode.style.display = "none";

                if (getAttributeValue(oCBList[i], "chk") == chkValue) {
                    oChkList.push(oCBList[i]);
                }

            }
        }
        //récupère les valeurs parentes des valeurs cochés
        var oNewChkList = new Array();
        for (i = 0; i < oChkList.length; i++) {
            oCurentParentChkList = this.GetTreeParentCheckBox(oChkList[i]);
            for (j = 0; j < oCurentParentChkList.length; j++) {
                oNewChkList.push(oCurentParentChkList[j]);
            }
            oNewChkList.push(oChkList[i]);
        }
        //affiches les valeurs cochées et leurs parentes
        var el;
        for (i = 0; i < oNewChkList.length; i++) {
            // Le premier parent doit être visible pour que le dernier parent soit visible
            el = this.FindUpTag(oNewChkList[i], "LI");
            if (el != null) {
                if (el.style.display == "none")
                    el.style.display = "block";
            }

            //if (oNewChkList[i].parentNode.parentNode.parentNode.parentNode.style.display == "none")
            //    oNewChkList[i].parentNode.parentNode.parentNode.parentNode.style.display = "block";

            if (oNewChkList[i].parentNode.parentNode.style.display == "none")
                oNewChkList[i].parentNode.parentNode.style.display = "block";
        }
    };

    this.FindUpTag = function (el, tag) {
        while (el.parentNode) {
            el = el.parentNode;
            if (el.tagName === tag)
                return el;
        }
        return null;
    }


    //bDisplayMsk : Afficher les utilisateurs masqués
    this.DisplayUserMasked = function (oSrc) {
        var oCB;
        if (!oSrc || typeof (oSrc) == 'undefined') {
            oCB = document.getElementById("chkUnmsk");
        }
        else if (oSrc.className.indexOf("rChk") >= 0)
            oCB = oSrc;
        else {
            oCB = document.getElementById("chkUnmsk");
            chgChk(oCB);
        }

        var bchecked = (getAttributeValue(oCB, "chk") == "1");
        var bDisplayAllUsers = document.getElementById("rbAll") && document.getElementById("rbAll").checked;
        var bDisplaySelectedUsers = document.getElementById("rbSel") && document.getElementById("rbSel").checked;
        var bDisplayUnSelectedUsers = document.getElementById("rbUnsel") && document.getElementById("rbUnsel").checked;

        var oSpanList = document.getElementById("ResultDiv").getElementsByTagName("span");
        for (i = 0; i < oSpanList.length; i++) {
            if (getAttributeValue(oSpanList[i], "ednmsk") == "1") {

                //var itemCheck = oSpanList[i].firstChild;
                var bSelected = oSpanList[i].querySelector("a[chk='1']") != null;
                if ((bchecked && !(bDisplayUnSelectedUsers && bSelected)) || ((bDisplayAllUsers || bDisplaySelectedUsers) && bSelected)) {

                    if (oSpanList[i].parentNode.style.display == "none")
                        oSpanList[i].parentNode.style.display = "";
                }
                else {
                    if (oSpanList[i].parentNode.style.display != "none")
                        oSpanList[i].parentNode.style.display = "none";
                }
            }
        }
        this._bDisplayMsk = bchecked;
    };

    //Selection de tous les utilisateurs (ou pas selon si le paramètre est à vrai ou à faux)
    this.OnChkSelectAll = function (sel) {
        var tabDiv = "ResultDivCustomValues;ResultDiv".split(";");

        for (var i = 0; i < tabDiv.length; i++) {
            currentDiv = document.getElementById(tabDiv[i]);
            if (currentDiv) {

                var oChkList = currentDiv.getElementsByTagName("A");
                for (j = 0; j < oChkList.length; j++) {
                    if (oChkList[j].className.indexOf("rChk") >= 0) {
                        if ((sel && getAttributeValue(oChkList[j], "chk") != "1") && (oChkList[j].parentNode.parentNode.style.display != "none")) {
                            chgChk(oChkList[j]);
                            this.ClickValM(oChkList[j]);
                        }
                        if (!sel && getAttributeValue(oChkList[j], "chk") == "1") {
                            chgChk(oChkList[j]);
                            this.ClickValM(oChkList[j]);
                        }
                    }
                }
            }
        }

        // IE8 ne ressine pas le contenu (cocjhe/décoche les cases) s'il n'a pas l'impression qu'il le faut
        // si on ajoute une "fausse" classe css à un élémént, il va bien rafraichir le contenu
        currentDiv.className += " ie8fail";

    };


    //Au chargement du catalog, on stocke les valeurs à enregistrer dans la base
    this.Load = function (oSelectedCB) {
        var isChecked = getAttributeValue(oSelectedCB, "chk") == "1";
        this.ClickVal(oSelectedCB.parentNode, isChecked);

        // Si c'est un groupe on coche ses utilisateurs  (selection des users au premier niveau seulement, pas de sélection des sous-groupe
        chgChk(oSelectedCB, isChecked); // Pour re-forcer le changement de classe CSS
        this.SelTreeChildCB(oSelectedCB, isChecked, false);
    };

    //Action au clic du catalogue utilisateur multiple sur une checkbox d'un utilisateur
    this.ClickValM = function (oSelectedCB, dontSelChild, chkCustomOpt) {

        if (!oSelectedCB) {
            return;
        }

        // Debut - on homogeneise avec l'option user en cours customisée]
        if (chkCustomOpt) {
            var id = oSelectedCB.id.toString();

            //si on est sur l'option non customisée 
            if (id.indexOf("CM_") < 0) {
                chgChk(document.getElementById(id.replace("_", "CM_")));
            }
            else {

                //Si on coche la checkbox custom, on la remplace par son equivalent dans l'arbre
                var idSplit = id.split('_');
                if (idSplit[1] != "0") {
                    oSelectedCB = document.getElementById(id.replace("CM", ""));
                    if (!oSelectedCB)
                        return;
                    chgChk(oSelectedCB);
                }
                else {
                    // Cas de l'option "Tous les utilisateurs"
                    var bChk = getAttributeValue(oSelectedCB, "chk") == "1";
                    this.OnChkSelectAll(false);
                    // On recoche l'option si cétait coché
                    if (bChk)
                        chgChk(oSelectedCB, bChk);
    
                    //return;
                }
                    
            }
        }
        // Fin - on homogeneise avec l'option user en cours customisée


        var bchecked = (getAttributeValue(oSelectedCB, "chk") == "1");
        var oSelectedObject = oSelectedCB.parentNode;

        //on stocke la valeur parmi les valeurs à enregistrer
        this.ClickVal(oSelectedObject, bchecked);

        //Groupe coché on coche ses enfants et groupe décoché on décoche ses enfants
        // sauf dans le cas ou l'on a demandé de ne pas le faire        
        if (!dontSelChild) {
            this.SelTreeChildCB(oSelectedCB, bchecked, true);
        }

        //l'enfant est délectionné on déselection le groupe parent
        if (!bchecked) {
            this.unCheckParentCB(oSelectedCB);
        } else { //On essaye de decocher l entree custom "tout les utilisateurs" si on a coché un enfant
            var id = oSelectedCB.id.toString();
            var chkAllId = id.substring(0, id.indexOf("_")) + "CM_0";
            var chkAll = document.getElementById(chkAllId);
            if (chkAll && getAttributeValue(chkAll, "chk") == "1") {
                chgChk(chkAll);
                this._selectedListValues.Delete(0);
            }
        }


    };

    //Désélection le parent de l'enfant si l'enfant n'est pas un parent
    this.unCheckParentCB = function (oSelectedCB) {

        if (oSelectedCB == null)
            //(oSelectedCB != null && oSelectedCB.id.toString().indexOf("_G") > 0)) 
            return;

        //on récupère le parent direct
        var parentCB = this.GetParentCheckBox(oSelectedCB, true);
        var parentSelected = parentCB != null && (getAttributeValue(parentCB, "chk") == "1");

        //On déselectionne le parent
        if (parentSelected) {
            chgChk(parentCB);
            this.ClickVal(parentCB.parentNode, false);
            // On sélectionne réellement les enfants :-)  en les ajoutant à la list "_selectedListValues" avec ClickVal
            var children = this.GetTreeChildrenCheckBoxes(parentCB);
            for (var i = 0; i < children.length; i++)
                this.ClickVal(children[i].parentNode, getAttributeValue(children[i], "chk") == "1");

            this.unCheckParentCB(parentCB);
        }
    };

    //Multiple - Groupe coché on coche ses enfants et groupe décoché on décoche ses enfants
    this.SelTreeChildCB = function (oSelectedCB, isChecked, bInDepth) {
        var childCBs = this.GetTreeChildrenCheckBoxes(oSelectedCB);
        var isParent = (childCBs.length > 0);

        if (!isParent)
            return;

        for (var i = 0; i < childCBs.length; i++) { 
            // [MOU-04-08-2014 cf. 32396] Selectionner tous les sous niveaux du groupe selectionné         
            if (childCBs[i].id.toString().indexOf("_G") < 0) {

                /************************************************************************************************
                // Si le groupe est coché, ses enfants non groupe sont cochés mais sans être stockés dans la base
                *************************************************************************************************/
                if ((getAttributeValue(childCBs[i], "chk") == "1" && !isChecked) || (getAttributeValue(childCBs[i], "chk") == "0" && isChecked)) {
                    
                    if (getAttributeValue(childCBs[i].parentNode, "ednmsk") == "1" || getAttributeValue(childCBs[i].parentNode, "edndsbld") == "1") {
                        if (isChecked && bInDepth && (childCBs[i].parentNode.parentNode.style.display != "none")) { //bInDepth == false -> on vient du Load, on ne coche pas les enfants masqués dans ce cas
                            chgChk(childCBs[i]);
                            this.ClickValM(childCBs[i]);
                        }
                        else if (!isChecked) {
                            chgChk(childCBs[i]);
                        }
                    }
                    else
                        chgChk(childCBs[i]);

                    //On mis à jour la checkbox custom si elle existe 
                    var customCB = document.getElementById(childCBs[i].id.toString().replace("_", "CM_"));
                    if (customCB) {
                        chgChk(customCB);
                    }
                }

                this.ClickVal(childCBs[i].parentNode, getAttributeValue(childCBs[i], "chk") == "1" && !isChecked);

            }
            //BSE:#51060 : Modification dans le comportement validé par GBO : ne pas coché\décoché les sous groupe ainssi que les utilisateurs enfants de ces sous groupes

            //else {
            //    if (bInDepth) {

            //        /************************************************************************************************
            //         // Si le groupe est coché (décoché), ses enfants groupe sont cochés (decochés) et sont stockés dans la base 
            //         *************************************************************************************************/
            //        if ((getAttributeValue(childCBs[i], "chk") == "1" && !isChecked) || (getAttributeValue(childCBs[i], "chk") == "0" && isChecked)) {
            //            chgChk(childCBs[i]);

            //            //On mis à jour la checkbox custom si elle existe 
            //            var customCB = document.getElementById(childCBs[i].id.toString().replace("_", "CM_"));
            //            if (customCB) {
            //                chgChk(customCB);
            //            }
            //        }

            //        this.ClickVal(childCBs[i].parentNode, isChecked);
            //        this.SelTreeChildCB(childCBs[i], isChecked, true);
            //    }
            //}


        }
    };

    //Gerer mode mois coché sous group et ses enfants c tout!
    this.CheckUser = function (node, isChecked) {
        var childCBs = this.GetTreeChildrenCheckBoxes(node);
        var isParent = (childCBs.length > 0);

        if (isParent) {
            for (var i = 0; i < childCBs.length; i++) {
                if ((getAttributeValue(childCBs[i], "chk") != ((isChecked) ? "1" : "0"))
                    && (childCBs[i].id.toString().indexOf("_G") < 0)) {
                    childCBs[i].setAttribute("chk", node.attributes.chk.value);
                    //chgChk(childCBs[i], ((isChecked) ? "1" : "0"));
                    //on stocke la valeur parmi les valeurs à enregistrer
                    this.ClickVal(childCBs[i].parentNode, isChecked);
                }
            }
        }
    };

    //Indique si l'objet à au moins un enfant non groupe de décoché
    this.HasUncheckedChild = function (direct_parentCB) {
        var hasUnCheckedChildren = false;   //au moin un enfant non groupe de non coché
        var parentsChildCBs = this.GetTreeChildrenCheckBoxes(direct_parentCB);
        for (var i = 0; (!hasUnCheckedChildren && i < parentsChildCBs.length) ; i++) {
            if (
                    (getAttributeValue(parentsChildCBs[i], "chk") == "0")
                    && (parentsChildCBs[i].id.toString().indexOf("_G") < 0)
                    )
                hasUnCheckedChildren = true;
        }

        return hasUnCheckedChildren;
    };

    //Action au clic du catalogue utilisateur simple sur une ligne d'un utilisateur
    this.ClickValS = function (oSelectedObject) {
        var classElm = getAttributeValue(oSelectedObject, "class");
        if (typeof (classElm) == 'undefined' || classElm == null)
            classElm = "";
        var tabElm = classElm.split(" ");
        var bAlreadyChecked = false;
        for (i = 0; i < tabElm.length; i++) {
            if (tabElm[i].toString().indexOf(this._selectedClassName) >= 0) {
                bAlreadyChecked = true;
                break;
            }
        }
        this.ClickVal(oSelectedObject, !bAlreadyChecked);
    };

    this.ClickVal = function (oSelectedObject, bChecked) {
        if (!oSelectedObject)
            return;
        if (!that.IsSelectionable(oSelectedObject))
            return;
        var clickedObjectId = oSelectedObject.id;
        if (clickedObjectId != '') {
            this.SelectListValue(clickedObjectId, bChecked);
            if (!this._bMultiple)
                this.SetSel(oSelectedObject, bChecked);
        }
    };

    // Définit une valeur de catalogue comme sélectionnée - Ajout de la valeur sélectionnée dans le tableau des selections
    // selectedObject : id de la TR
    this.SelectListValue = function (selectedObjectId, bChecked) {
        if (!this._bMultiple) {
            this._selectedListValues = new Dictionary();
        }
        var oElmList = document.getElementById(selectedObjectId);
        var elmId = null;
        var elmLib = null;
        if (oElmList) {
            elmId = getAttributeValue(oElmList, "ednid");
            elmLib = GetText(oElmList);
        }
        if (bChecked) {

            //si l'objet n'est pas dans la liste alors on l'ajoute bug cf.22516
            if (this._selectedListValues.Keys.indexOf(elmId) < 0) {
                this._selectedListValues.Add(elmId, elmLib);
            }
        }
        else if (this._bMultiple) {
            this._selectedListValues.Delete(elmId);
        }
    };

    //Retourne la checkbox parente
    this.GetParentCheckBox = function (checkBox) {
        var parentUl = this.FindUp(checkBox, "UL"); // img (checkBox) > span (ensemble checkBox/libellé) > li (branche) > ul (parent)
        var parentLi = this.FindUp(parentUl, "LI"); // ul (parent) > li (branche parente)

        //si pas de parent
        if (parentUl == null || parentLi == null)
            return null;

        var isRoot = ((!parentUl.id) || parentUl.id.substring(parentUl.id.indexOf("_") - 1) == "0" || parentLi.id.substring(parentLi.id.indexOf("_") - 1) == "0");

        //Prendre les cases parentes si on est pas à la racine
        var parent_cb = (!isRoot) ? parentLi.querySelectorAll("a[chk]") : null;

        //Prendre la première case à cocher parentes car il ne peut y en avoir qu'une.
        parent_cb = (parent_cb && parent_cb.length > 0)
                ? parent_cb[0]
                : null;
        return parent_cb;
    }


    // Retourne la case à cocher de la branche parente de la case à cocher courante
    // ca retourne exatement le signe plus et non pas  la checkbox
    this.GetTreeParentCheckBox = function (checkBox, bOnlyDirectParent) {
        if (typeof (bOnlyDirectParent) == 'undefined') bOnlyDirectParent = false;

        var parentUl = this.FindUp(checkBox, "UL"); // img (checkBox) > span (ensemble checkBox/libellé) > li (branche) > ul (parent)
        var parentLi = this.FindUp(parentUl, "LI"); // ul (parent) > li (branche parente)
        var isRoot = ((!parentUl) || (cur_parentLi) || (!parentUl.id) || (parentUl.id == "eTVBC_0"));
        var parentCBs = new Array();

        var cur_parentLi = parentLi;
        var cur_parentUl = parentUl;
        var cur_object;
        //Lorsque l'on atteint la racine c'est quu tous les parents ont été récupéré.
        while (!isRoot) {
            cur_object = cur_parentLi.getElementsByTagName("A");
            if (cur_object) {
                parentCBs.push(cur_object[0]);
                cur_parentUl = this.FindUp(cur_object[0], "UL"); // img (checkBox) > span (ensemble checkBox/libellé) > li (branche) > ul (parent)
                cur_parentLi = this.FindUp(cur_parentUl, "LI"); // ul (parent) > li (branche parente)
            }
            else
                break;

            isRoot = ((!parentUl) || (!cur_parentLi) || (!parentUl.id) || (parentUl.id == "eTVBC_0") || bOnlyDirectParent);
        }

        return parentCBs;
    };
    /*FCT commune à at arbo classiques*/

    // Multiple - Retourne le tableau de cases à cocher filles de la branche de la case à cocher courante.
    this.GetTreeChildrenCheckBoxes = function (checkBox) {
        var childCBs = new Array();
        var parentLi = this.FindUp(checkBox, "LI"); // ul (parent) > li (branche parente)
        if (!parentLi) {
            /*Cas ou on est à la racine, le premier élement n'est pas une CheckBox, par conséquent, on change de fonctionnement
            On charge une liste des UL enfants et on les traite.*/
            parentLi = document.getElementById("eTVB_0");
            if (parentLi) {
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

    // Recherche l'élément parent du tagName voulu
    // elt : element à dupliquer ; tag : tag de l'élement recherché
    this.FindUp = function (elt, tag) {
        if (elt == null)
            return null;

        do {
            if (elt.nodeName && elt.nodeName.search(tag) != -1)
                return elt;
        } while (elt = elt.parentNode);

        return null;
    };


    this.GetSelectedIds = function () {

        var tabSelectedIds = this._selectedListValues.Keys;
        var retId = "";
        for (var i = 0; i < tabSelectedIds.length; i++) {

            var elmId = tabSelectedIds[i];

            if (elmId != null) {
                if ((";" + retId + ";").indexOf(";" + elmId + ";") < 0) {
                    if (retId != "")
                        retId += ";";
                    retId += elmId;
                }
            }

        }

        return retId;
    };
}


// fonction hors objet, requis par eUpdater
// Fonction post recherche qui permet la mise à jour visuelle de la liste de valeurs affichée.
function DialogCatalogSearchTreatment(oRes, jsVarName, bSilent) {

    top.setWait(false);
    var strCatalogSearchSuccess = parent.getXmlTextNode(oRes.getElementsByTagName("result")[0]);
    if (strCatalogSearchSuccess != "SUCCESS") {
        if (!bSilent) {
            eAlert(0, top._res_246, top._res_6235);
        }
        return;
    }

    // Parent car getXmlTextNode de eTools.js est incluse par eMain.aspx
    var strUpdatedHTML = parent.getXmlTextNode(oRes.getElementsByTagName("html")[0]);
    var strUpdatedJS = parent.getXmlTextNode(oRes.getElementsByTagName("js")[0]);

    if (document.getElementById('ResultDiv') && strUpdatedHTML != '') {
        document.getElementById('ResultDiv').innerHTML = strUpdatedHTML;
    }

    var oBtnSrch = document.getElementById("eBtnSrch");
    if (oBtnSrch) {
        if (document.getElementById("eTxtSrch").value == '')
            document.getElementById("eBtnSrch").className = "icon-magnifier srchFldImg";
        else if (parent.getXmlTextNode(oRes.getElementsByTagName("nbResults")[0]) == "0" && parent.getXmlTextNode(oRes.getElementsByTagName("searchValue")[0]) != "")
            document.getElementById("eBtnSrch").className = oBtnSrch.className = "icon-edn-cross srchFldImg";
        else
            oBtnSrch.className = "icon-edn-cross srchFldImg";
    }

    // Lance le JS
    eval(strUpdatedJS);
};

//Retourne la liste de valeurs (id et label) sélectionnées sous la forme suivante : id1;id2;idn…$|$label1;label2;labeln…
function GetReturnValue(jsVarname) {
    if (typeof (jsVarname) == 'undefined' || jsVarname == "")
        jsVarname = 'eCU';

    var oCatUser = window[jsVarname];
    var retId = "";
    var retLbl = "";
    var tabSelectedIds = oCatUser._selectedListValues.Keys;
    for (i = 0; i < tabSelectedIds.length; i++) {
        var elmId = tabSelectedIds[i];
        var elmLib = oCatUser._selectedListValues.Lookup(tabSelectedIds[i]);
        if (elmId != null) {
            if ((";" + retId + ";").indexOf(";" + elmId + ";") < 0) {
                if (retId != "")
                    retId += ";";
                retId += elmId;
                if (retLbl != "")
                    retLbl += ";";
                retLbl += elmLib.replace(/^\s+/g, '').replace(/\s+$/g, ''); //TRIM pour virer les espaces avant et après le libellé
            }
        }
    }
    return retId + "$|$" + retLbl;
}

function IsAllUserSelected() {
    var chkBoxList = document.querySelectorAll('*[id^="chkValue_"]');
    var arrayChkBoxList = [].slice.apply(chkBoxList);
    var allChkBoxList = arrayChkBoxList.filter(function (elem) {
        return elem.parentNode.parentNode.getAttribute('style') != 'display: none;';
    })
    var checkChkBoxList = arrayChkBoxList.filter(function (elem) {
        return elem.getAttribute('chk') == '1';
    })

    if (allChkBoxList.length == checkChkBoxList.length) {
        return true;
    } else {
        return false;
    }
}

// L'option "Tous les utilisateurs" est-elle cochée ?
function IsAllUsersOptionSelected() {
    if (getAttributeValue(document.getElementById("chkValueCM_0"), "chk") == "1")
        return true;
    return false;
}


function GetSelectedIds(jsVarname) {
    if (typeof (jsVarname) == 'undefined' || jsVarname == "")
        jsVarname = 'eCU';
    var oCatUser = window[jsVarname];
    var retId = "";
    //Si Multiple - Parcours de tous les éléments Sélectionnés
    var tabSelectedIds = oCatUser._selectedListValues.Keys;
    for (i = 0; i < tabSelectedIds.length; i++) {
        var elmId = tabSelectedIds[i];
        if (elmId != null) {
            if ((";" + retId + ";").indexOf(";" + elmId + ";") < 0) {
                if (retId != "")
                    retId += ";";
                retId += elmId;
            }
        }

    }
    return retId;
};

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