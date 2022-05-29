/// <reference path="eFilterReportListDialog.js" />
/// <reference path="eTools.js" />
var modalAdvancedCatalog = null;

// demande 36826 : MCR :  appel entrant du CTI, nombre de modales ouvertes
var nModalLnkFileLoaded = 0;


function eFieldEditor(type, parentPopup, jsVarName, monitoredClass) {


    var that = this; // pointeur vers l'objet eFieldEditor lui-même, à utiliser à la place de this dans les évènements onclick (où this correspond alors à l'objet cliqué)

    this.debugMode = false;  // debugMode : mettre à true pour empêcher la disparition du menu si la souris n'est plus dessus et permettre le debug de son contenu sous Firebug ou autre
    this.parentPopup = parentPopup;
    this.sourceElement = null; //Element source de l'événement
    this.jsVarName = jsVarName;
    this.monitoredClass = monitoredClass;
    this.theme = 'default'; // TODO: getParamWindow().GetParam("Theme")
    this.validateOnHide = true; // TODO: à conditionner selon catalogue saisie libre ou non	
    // indique si l'on doit remettre à null ("destroy") ou remettre à zéro ("reset") l'objet eFieldEditor en cours après édition d'une valeur (flagAsEdited).
    // ATTENTION, si "destroy" est indiqué comme valeur pour cette propriété, il est dans ce cas de la responsabilité de l'appelant de recréer l'objet via un new
    this.autoResetMode = '';

    this.type = type;
    this.action = 'editValue';
    this.headerElement = null;      // pointeur vers la cellule contenant l'entete de colonne qui contient en attribut les propriétés du champ
    this.headerElementId = "";      // pointeur vers la cellule contenant l'entete de colonne qui contient en attribut les propriétés du champ
    this.switchFullScreen = false;  // à mettre à true pour éditer directement le champ en mode Zoom/Plein écran (pris en charge par certains types de champs, ex. eMemoEditor)

    this.validateLaunch = false;

    this.tab = 0;
    this.fileId = 0;

    // Spécifique inlineEditor
    this.value = '';
    this.isNewValue = false; // mode "Edition de nouvelle valeur" : mettre à true pour ne pas déclencher d'update en base sur une valeur non encore créée
    this.maxLength = 0;

    this.obligat = false;

    // Spécifique catalogue	
    this.catalogTitle = '';
    this.isFreeText = false;
    this.multiple = false;
    this.treeview = false;
    this.bndDescId = null;     // DescId du catalogue parent
    this.initialLabels = new Array(); 	// libellés non filtrés, initialement proposés
    this.labels = new Array(); 	  	    // libellés filtrés selon le critère de recherche
    this.selectedLabels = new Array();  // libellés sélectionnés dans le catalogue
    this.currentLabels = new Array();   // libellés actuellement présents en base/dans la cellule de tableau
    this.initialValues = new Array();   // Valeurs non filtrés, initialement proposés
    this.values = new Array(); 	  	    // Valeurs filtrés selon le critère de recherche
    this.selectedValues = new Array();  // Valeurs sélectionnés dans le catalogue
    this.currentValues = new Array();   // Valeurs actuellement présents en base/dans la cellule de tableau
    this.filter = '';
    this.searchTimer = null;
    this.selectedCatalogIndex = -1;
    this.mruParamValue = null;        // valeurs des MRU du field
    this.catPopupType = '';     // Type de catalogue
    this.SearchLimit = 0;   //Nombre de caractère minimum avant recherche
    this.bAutobuildName = false;   //Nombre de caractère minimum avant recherche
    this.SearchLimit = 0;   //Nombre de caractère minimum avant recherche
    this.bEditCatAsInline = false; // Indique si l'on doit éditer certains types de catalogue comme un champ de saisie simple (ex : catalogue saisie libre)
    this.bRevert = false;   //Test pour positionner le div (Haut ou bas)

    this.MinimumMruWidth = 200; //CONSTANTE qui définit la largeur minimale d'une MRU (demande #20943).

    this.advancedDialog = null;

    this.SpecificErrorMessge = ''; // message d'erreur sur le format de saisie d'un champ

    // spécifique date
    this.contextMenu;

    this.UpdateOnBlur = true;

    if (typeof (isUpdateOnBlur) == "function")
        this.UpdateOnBlur = isUpdateOnBlur();

    // TODO: optimiser gestion des classes
    this.checkClass = function (obj, className) {
        if (obj) {
            return new RegExp('\\b' + className + '\\b').test(obj.className);
        }
        else {
            return false;
        }
    };

    this.addClass = function (obj, className) {
        if (obj) {
            if (!this.checkClass(obj, className))
                obj.className += " " + className;
        }
    };

    this.removeClass = function (obj, className) {
        if (obj) {
            if (this.checkClass(obj, className)) {
                obj.className = obj.className.replace(className, "");
                obj.className = obj.className.replace("  ", " ");
            }
        }
    };

    this.addOnClick = function (obj) {
        if (obj) {
            setEventListener(obj, 'click', function (event) { that.onClick(event); });
        }
    };

    this.addOnFocus = function (obj) {
        if (obj && obj.tagName == 'INPUT') {
            setEventListener(obj, 'focus', function (event) { that.onClick(event); });
        }
    };

    this.addOnKeyUp = function (obj) {
        if (obj && obj.tagName == 'INPUT') {
            setEventListener(obj, 'keyup', function (event) { that.onClick(event); });
        }
    };

    //GetSourceElement : Retourne l'élément source, en utilisant de préférence directement la référence dans l'objet en cours et pas parentPopUp (car conflit avec IE et les tabulations)
    this.GetSourceElement = function () {
        if (this.sourceElement)
            return this.sourceElement;
        else
            return this.parentPopup.sourceElement;
    };

    //Indique si l'objet champ est déjà en cours d'édition ( isEditing à true si onclick et a false si onblur)
    this.isEditing = false;
    this.setIsEditing = function (bEditing) {
        this.isEditing = bEditing;
    }

    //#53 635 - Indique à la fenêtre parente que des modifications potentiellement non sauvegardées ont eu lieu
    this.setParentModalUnsavedChanges = function (bUnsavedChanges) {
        try {
            var sourceElement = this.GetSourceElement();
            if (sourceElement && sourceElement.ownerDocument && typeof (sourceElement.ownerDocument.parentModalDialog) == "object")
                sourceElement.ownerDocument.parentModalDialog.unsavedChanges = bUnsavedChanges;
        }
        catch (ex) { }
    }

    ///Initialisation du field editor avec les prorpiétés du champ clické
    /// Si un field editor a déjà été ouvert, on doit valider celui-ci avant tout
    this.onClick = function (target, openerElement, strActionTrigger, event) {
        //Click successif sur le même champ de type saisie libre
        // => ne rien faire
        if (this.sourceElement == target && (this.type == "inlineEditor" || this.type == "memoEditor") && this.parentPopup.childElement == this) {
            this.resetOrDestroy();
            return;
        }

        // SPH : 26270 remise en place du test sur isShown + ajout du test additionnel sur inlineeditor (cf 25962)
        // this.parentPopup.childElement représente le fieldeditor du ePopupObject courant.
        // Si c'est le 1er click de la session, il est vide. Sinon, à ce niveau, il représente le dernier eFieldEditor
        // S'il y en a un, il faut vérifier le statut de ce dernier pour éventuellement le valider
        //  => Si le popup est ouvert, on le valide sur le click avant de réaffecter les valeurs
        //  => Si le popup est de type "inlineeditor" (sans "vrai" popup), on valide aussi
        if (this.parentPopup.childElement
            && this.parentPopup.isShown
            && typeof (this.parentPopup.childElement.validate) == "function"
            && (!this.isEditing || this.type == "inlineEditor")
            && (this.type != "stepCatalogEditor")
        ) {
            this.parentPopup.childElement.validate(this.isNewValue, strActionTrigger == "LASTVALUE_CLICK");
        }

        if (target) {

            //Initialisation des variable du field editor
            that.validateLaunch = false;
            this.setIsEditing(true); //Flag le fieldeditor comme en cours d'édition
            this.headerElement = target.ownerDocument.getElementById(getAttributeValue(target, "ename"));
            this.headerElementId = getAttributeValue(target, "ename");
            this.isOpen = true;

            //Récupération d'information complémentaire
            if (this.headerElement) {
                var descId = this.headerElement.getAttribute("did");

                // le libelle du champ
                this.catalogTitle = this.headerElement.getAttribute("lib");
                // Est Catalogue Multiple ?
                this.multiple = (this.headerElement.getAttribute("mult") == "1");
                // Est arborescent ?
                this.treeview = (this.headerElement.getAttribute("tree") == "1");
                // Il a un parent
                this.bndDescId = this.headerElement.getAttribute("bndid");
                if (this.bndDescId == "0")
                    this.bndDescId = null;

                this.catPopupType = this.headerElement.getAttribute('pop');
                if (this.catPopupType > 1)
                    this.validateOnHide = false;
            }

            this.parentPopup.sourceElement = target; // objet actuellement édité (auquel est rattaché l'éditeur et sa popup)
            this.sourceElement = target;
            this.parentPopup.childElement = this; // référence vers l'objet éditeur pour le rendre accessible depuis l'objet popup


            this.obligat = getAttributeValue(target, "obg") == "1";

            this.bEditCatAsInline = false;
            if (typeof (openerElement) != 'undefined' && openerElement != null) {

                this.parentPopup.openerElement = openerElement; // objet ayant provoqué l'ouverture de l'éditeur (pas forcément sourceElement, ex : bouton déporté)
                this.isFreeText = ((getAttributeValue(parentPopup.sourceElement, "eaction") == "LNKFREECAT") && target == openerElement);
            }
            else {
                this.parentPopup.openerElement = this.GetSourceElement();
                // Si l'on édite un catalogue texte libre et que l'édition n'a pas été provoquée par un objet externe (bouton déporté), on vérifie si on doit l'éditer
                // comme un champ de saisie libre (ex : clic sur le champ INPUT, sélection, tabulation...)
                var bTriggeredWithEnter = (strActionTrigger == "KEYUP" && event.keyCode == 13);
                this.bEditCatAsInline = (parentPopup.sourceElement.getAttribute("eaction") == "LNKFREECAT" && !bTriggeredWithEnter);
                this.isFreeText = (parentPopup.sourceElement.getAttribute("eaction") == "LNKFREECAT");
            }

            if ((!this.bEditCatAsInline && this.type == 'catalogEditor') || this.type == 'linkCatFileEditor' || this.type == 'catalogUserEditor') {

                if ((this.type == 'linkCatFileEditor') && (descId % 100 == 1))
                    dbVal = "lnkid";
                else
                    dbVal = "dbv";

                this.selectedValues = new Array();
                this.selectedLabels = new Array();
                this.currentValues = new Array();
                this.currentLabels = new Array();
                // Paramétrage de la valeur actuelle du catalogue
                if (!target.getAttribute(dbVal)) {
                    if (target.tagName == "TD") {
                        if (target.innerText != " " && target.innerText && target.innerText != "" && target.innerText != "__")
                            target.setAttribute(dbVal, trim(target.innerText));

                        if (target.textContent != " " && target.textContent && target.textContent != "" && target.textContent != "__")
                            target.setAttribute(dbVal, trim(target.textContent));
                    }
                    else if (target.tagName == "INPUT") {
                        if (target.value != " " && target.value && target.value != "" && target.value != "__")
                            target.setAttribute(dbVal, trim(target.value));
                    }
                }

                if (target.getAttribute(dbVal)) {
                    this.currentValues = target.getAttribute(dbVal).split(';');
                    if (this.currentValues == "")
                        this.currentValues = new Array(target.getAttribute(dbVal));

                    if (target.tagName == "TD") {
                        if (target.innerText != null) {
                            this.currentLabels = target.innerText.split(';');
                            if (this.currentLabels == "")
                                this.currentLabels = target.innerText;
                        }
                        else {
                            this.currentLabels = target.textContent.split(';');
                            if (this.currentLabels == "")
                                this.currentLabels = target.textContent;
                        }
                    }

                    if (target.tagName == "INPUT") {
                        this.currentLabels = target.value.split(';');
                        if (this.currentLabels == "")
                            this.currentLabels = target.value;
                    }

                    for (var i = 0; i < this.currentValues.length; i++)
                        this.selectValue(this.currentValues[i], (this.currentLabels && this.currentLabels != "") ? this.currentLabels[i] : "", true);
                }
            }
            else if (this.type == 'stepCatalogEditor') {

                this.selectedValues = [];
                this.selectedLabels = [];
                this.selectValue(getAttributeValue(target, "dbv"), target.innerText, !hasClass(target.parentElement, "selectedValue"));
                setAttributeValue(this.headerElement, "dbv", getAttributeValue(target, "dbv"));
            }
            else if (this.type == 'dateEditor') {
            }
            else if (this.type == "fileEditor") {

                if (target.tagName == 'INPUT')
                    this.value = target.value;
                else
                    this.value = GetText(target);

            }
            else if (this.type == 'memoEditor') {
                if (target.getAttribute("eEmpty") != 1) {
                    // Mode Fiche
                    var oIFrame = document.getElementById(target.id.replace('ifr', '') + 'ifr');
                    if (oIFrame) {
                        var oDoc = oIFrame.contentWindow || oIFrame.contentDocument;
                        if (oDoc.document) {
                            oDoc = oDoc.document;
                        }

                        if (oDoc.body) {
                            this.value = oDoc.body.innerHTML;
                        }
                    }
                    // Mode Liste
                    else if (this.value == '') {
                        // En mode Liste, à la première initialisation (champ vide), on indique à l'objet de s'afficher directement en plein écran
                        // On affecte à l'eFieldEditor une autre action que celle par défaut, pour éviter certains traitements
                        // Notamment le fait de valider le contenu du champ si on clique en-dehors de celui-ci (ce qui se déclenche lorsqu'on clique en-dehors du champ Mémo affiché
                        // en plein écran, sur la structure de la fenêtre par ex.
                        this.action = 'editFullScreenValue';
                        // L'affectation de cette propriété indiquera à la méthode render() d'appeler directement la fonction switchFullScreen() de eMemoEditor à la place de show()
                        // switchFullScreen() se chargera d'afficher le champ Mémo dans une grande fenêtre, qui prendra le soin de récupérer le contenu directement en base,
                        // évitant ainsi un appel AJAX
                        eMemoEditorObject.switchFullScreen = true;
                        /*
                        // Appel AJAX pour récupérer la valeur du champ mémo, puis appel de la fonction locale afterGettingMemoValue pour mettre à jour sur l'application                        
                        // fonction réencapsuler pour garder le 1er paramètres oRes utilisé dans la fonction
                        this.waitingForValue = true; // empêche d'envoyer une mise à jour en base tant que l'on n'a pas récupéré la valeur actuelle, sera passé à false par afterGettingMemoValue
                        var callbackFunction = (
                        function (jsVarName) {
                        return function (params) {
                        afterGettingMemoValue(params, jsVarName);
                        }
                        }
                        )(this.jsVarName);

                        getMemo("FIELD_VALUE", callbackFunction, this.headerElement, target, null, null, null, null);
                        */
                    }
                }
                else {
                    try {
                        if (this.debugMode) {
                            this.trace('ATTENTION, la valeur du champ va être réinitialisée car la cellule source (' + target.id + ') a un attribut eEmpty à 1.');
                            this.trace('Valeur actuelle : "' + this.value + '"');
                        }
                        this.value = '';
                    }
                    catch (ex) {

                    }
                }
            }
            else if (this.type == 'mailEditor') {
                // Avant modification de la rubrique, on conserve la valeur
                target.setAttribute('oldval', target.value);

                //Dans le cas d'un champ email on cherche les suggestions emailing
                //Si pas de suggestion ou qu'un des champs nom/prenom/societe n'est pas present ou renseigné
                //alors on retourne a l'affichage classique 
                //Si la valeur est vide alors on peut essayer d'afficher les suggestions
                if (target.value != " " && target.value && target.value != "" && target.value != "__") {
                    target.setAttribute('dbv', target.value);
                } else if (this.isEditing && getAttributeValue(target, "data-suggenabled") == "1") {
                    //setAttributeValue(this.headerElement, "ignoreblur", "1");
                    this.SearchEmailSuggestList();
                }


                target.onblur = function () {


                    if (getAttributeValue(that.headerElement, "ignoreblur") == "1") {
                        setAttributeValue(that.headerElement, "ignoreblur", "0");
                        return;
                    }
                    that.setIsEditing(false);

                    var oldValue = target.getAttribute('oldval');
                    oldValue = oldValue ? oldValue : "";
                    //Pas d'interet a valider sans modification de valeur. Permet de gerer ce double cas de "catalogue" et saisie libre
                    if (oldValue != target.value) {
                        that.validate(that.isNewValue);
                    }

                };
                return;
            }
            else if (target.tagName == 'INPUT') {
                // mode Edition (modif et crea)
                // dans ce cas on n'a pas besoin d'éditeur.
                // Ce mode prend également en charge les catalogues saisie libre (this.bEditCatAsInline = true)
                if (target.value != " " && target.value && target.value != "" && target.value != "__")
                    target.setAttribute('dbv', target.value);

                // Avant modification de la rubrique, on conserve la valeur
                target.setAttribute('oldval', target.value);

                fieldEditorInputBlurFunction = function () {
                    if (this.debugMode) { console.log("eFieldEditor.fieldEditorInputBlurFunction()"); }

                    if (getAttributeValue(that.headerElement, "ignoreblur") == "1") {
                        setAttributeValue(that.headerElement, "ignoreblur", "0");
                        return;
                    }

                    that.setIsEditing(false);
                    that.validate(that.isNewValue);
                };
                if (target.hasAttribute("data-ehidden") && target.getAttribute("data-ehidden") == "1") {
                    target.onchange = fieldEditorInputBlurFunction;
                } else {
                    target.onblur = fieldEditorInputBlurFunction;
                }
                return;
            }
            else {
                if (getAttributeValue(target, "eEmpty") != 1) {
                    this.value = trim(GetText(target));
                }
                else {
                    try {
                        if (this.debugMode) {
                            this.trace('ATTENTION, la valeur du champ va être réinitialisée car la cellule source (' + target.id + ') a un attribut eEmpty à 1.');
                            this.trace('Valeur actuelle : "' + this.value + '"');
                        }
                        this.value = '';
                    }
                    catch (ex) {

                    }
                }

                // #29 518 - L'attribut eEmpty n'est pas toujours ajouté sur toutes les cellules en mode Liste, probablement pour gagner en poids de page.
                // Le champ est alors initialisé avec un espace comme seule valeur, et lorsqu'on édite cette valeur et que l'on sort du champ sans la modifier,
                // la MAJ se fait car validate() considère que oldValue = ' ' et newValue = '' => valeur différente => MAJ
                // On considère donc aue l'utilisateur ne devrait pas être amené à saisir un seul espace comme valeur de champ,
                // et qu'une cellule est vide s'il n'y a qu'un espace dedans (&nbsp; ajouté par le renderer).
                // Attention : innerText renvoie une valeur "espace" qui n'est pas égale à " " lorsqu'il s'agit d'une cellule contenant &nbsp;
                // Pour pouvoir vérifier la valeur dans ce cas, on réalise donc un comptage du nombre de caractères et un trim()
                if (this.value.length == 1 && eTrim(this.value) == '')
                    this.value = '';

                if (this.action == 'renameFile')
                    this.value = eTrim(this.value);
            }

            // Calcul de la position du target
            var targetLeft = 0;
            var targetTop = 0;
            var targetZIndex = 1;
            var currentTarget = target;
            var rootElementFound = false;

            // On positionne par rapport à tous les éléments parents. On boucle jusqu'à remonter tout le DOM
            var testPos = getAbsolutePosition(currentTarget);

            // On ajuste également la superposition du champ
            // Les z-index ne sont pas absolus. S'il n'est pas défini sur l'élément parent, il suffit de le mettre à 1 pour que l'éditeur apparaîsse au-dessus,
            // même si l'élément parent est posé sur un élément ayant un z-index plus élevé (ex : popups modales)
            if (!isNaN(currentTarget.style.zIndex) && currentTarget.style.zIndex > targetZIndex)
                targetZIndex = currentTarget.style.zIndex + 1;

            this.parentPopup.left = testPos.x;
            this.parentPopup.top = testPos.y;
            this.parentPopup.zIndex = targetZIndex;

            var showPopup = false; // en mode catalogue, si aucune MRU n'est trouvée, on affiche directement la boîte de dialogue Catalogue, pas la popup

            if (strActionTrigger == "LASTVALUE_CLICK") {
                showPopup = false;
            }
            else if (this.type == 'catalogEditor' || this.type == 'catalogUserEditor') {
                // #44565 : Valeurs MRU sur catalogues liés
                if (this.bndDescId != null && !this.multiple) {
                    this.parentPopup.width = this.parentPopup.initialWidth;
                    this.parentPopup.height = this.parentPopup.initialHeight;
                    showPopup = true;
                }
                else if (this.catPopupType == "5") {
                    showPopup = true;
                }
                else if ((this.loadMRU(target) || this.isFreeText)) {
                    this.parentPopup.width = this.parentPopup.initialWidth;
                    this.parentPopup.height = this.parentPopup.initialHeight;
                    showPopup = true;
                }
                else
                    showPopup = false;
            }
            else if (this.type == "fileEditor") {
                this.openFilesMgrDialog();
                showPopup = false;
            }
            else if (this.type == 'linkCatFileEditor') {
                var assFieldValue = this.getAssociateFieldValue();
                if (assFieldValue)
                    this.filter = assFieldValue;
                if (strActionTrigger != "LASTVALUE_CLICK" && this.loadMRULinkFile(target)) {
                    this.parentPopup.width = this.parentPopup.initialWidth;
                    this.parentPopup.height = this.parentPopup.initialHeight;
                    showPopup = true;
                }
                else
                    showPopup = false;
            }
            else if (this.type == 'eCheckBox' || this.type == 'eBitButton' || this.type == 'stepCatalogEditor') {
                showPopup = false;
            }
            else {
                var referenceElement = null;
                if (this.GetSourceElement()) {
                    referenceElement = this.GetSourceElement();

                    var newWidth = referenceElement.width;
                    if (!newWidth || newWidth == '' || (typeof (newWidth) == 'undefined'))
                        newWidth = referenceElement.offsetWidth;

                    var newHeight = referenceElement.height;
                    if (!newHeight || newHeight == '' || (typeof (newHeight) == 'undefined'))
                        newHeight = referenceElement.offsetHeight;

                    if (isNaN(newWidth) || isNaN(newHeight)) {
                        if (document.getElementById(referenceElement.getAttribute('ename')))
                            referenceElement = document.getElementById(referenceElement.getAttribute('ename'));

                        newWidth = referenceElement.width;
                        if (!newWidth || newWidth == '' || (typeof (newWidth) == 'undefined'))
                            newWidth = referenceElement.offsetWidth;

                        newHeight = referenceElement.height;
                        if (!newHeight || newHeight == '' || (typeof (newHeight) == 'undefined'))
                            newHeight = referenceElement.offsetHeight;
                    }
                }

                // la fonction getNumber est définie dans eTools.js
                try {
                    if (getNumber) {
                        newWidth = getNumber(newWidth);
                        newHeight = getNumber(newHeight);
                    }
                    else if (parent.getNumber) {
                        newWidth = parent.getNumber(newWidth);
                        newHeight = parent.getNumber(newHeight);
                    }
                }
                catch (e) {
                }

                if (!isNaN(newWidth) && newWidth > 0) {
                    this.parentPopup.width = newWidth;
                }

                if (!isNaN(newHeight) && newHeight > 0) {
                    this.parentPopup.height = newHeight;
                }

                showPopup = true;
            }


            if (showPopup) {
                this.render();

                if (this.type != "dateEditor" && this.type != 'linkCatFileEditor')
                    this.parentPopup.show();

                // Pour donner le focus en fin de textbox de recherche
                var ctrlSch;

                try {

                    ctrlSch = document.getElementById("eCatalogEditorSearch");

                    if (ctrlSch) {
                        if (!isTablet())
                            getFocusAfter(ctrlSch);
                    }
                    else {
                        ctrlSch = document.getElementById("eInlineEditor");
                        if (ctrlSch)
                            getFocusAfter(ctrlSch);
                    }
                }
                catch (_e) { }
            }
        }

        if (strActionTrigger != "LASTVALUE_CLICK")
            this.resetOrDestroy();
    };


    //Recherche au niveau de la liste
    this.initValues = function (searchMode, newValues, newLabels) {

        this.values = new Array();
        this.labels = new Array();
        if (searchMode || ((this.headerElement.getAttribute('bndId') != "0") && (this.headerElement.getAttribute('bndId') != null))) {
            if (this.type == 'linkCatFileEditor')
                this.SearchLnkFile();
            else if (this.type == 'catalogUserEditor')
                this.SearchUser();
            else
                this.SearchCatalog(searchMode, newValues, newLabels);


        }
        // Si la réinitialisation des valeurs est demandée par le .renderValues interne (pour afficher une liste filtrée ou non)
        // on reprend toutes les valeurs de la base de données, puis on les filtre à l'ajout
        else {
            this.filter = '';

            if (document.getElementById("ImgSrchCatalog")) {
                var iconClass = (getAttributeValue(this.sourceElement, "dbv") != "") ? "icon-edn-cross sprite-croix-grise" : "icon-magnifier sprite-loupe";
                document.getElementById("ImgSrchCatalog").className = "sprite-Editor " + iconClass;
                document.getElementById("ImgSrchCatalog").setAttribute("close", (getAttributeValue(this.sourceElement, "dbv") != "") ? "1" : "0");
            }

            // Valeurs initiales : les MRU
            if (!newValues && !newLabels) {
                newValues = this.initialValues;
                newLabels = this.initialLabels;
            }

            if (newValues && newLabels) {
                for (var j = 0; j < newValues.length; j++) {
                    this.addValue(newValues[j], newLabels[j], false);
                }
            }
        }
    };

    ///
    this.SearchUser = function () {
        var userUpdater = new eUpdater("eCatalogDialogUser.aspx", null);
        userUpdater.addParam("action", "MRU", "post");
        userUpdater.addParam("CatSearch", this.filter, "post");

        setWait(true);
        userUpdater.send(catalogSearchTreatment, this.jsVarName, true);

    };

    ///
    this.SearchCatalog = function (searchMode, newValues, newLabels) {
        // En mode recherche, on effectue un appel AJAX pour récupérer les valeurs depuis la base.
        // A la suite de cet appel AJAX, la fonction catalogSearchTreatment() récupère les valeurs (XML), les ajoute à l'objet eCatalog (addValue()) et rappelle renderValues()
        // pour rafraîchir l'affichage avec les valeurs récupérées
        var catDescId = "";
        var catUrl = "";

        var catSelectedValues = this.selectedValues.join(";");
        if (this.headerElement) {
            catDescId = this.headerElement.getAttribute('popid');
            this.catPopupType = this.headerElement.getAttribute('pop');
            catBoundDescId = this.headerElement.getAttribute('bndId');
            catBoundPopup = this.headerElement.getAttribute('bndPop');
        }

        var catParentValue = this.GetSourceElement().getAttribute('pdbv');

        catUrl = "eCatalogDialog.aspx";

        // Appel AJAX pour récupérer toutes les valeurs
        var catalogUpdater = new eUpdater(catUrl, null);
        catalogUpdater.ErrorCallBack = catalogErrorSearch;
        catalogUpdater.addParam("CatDescId", catDescId, "post");
        catalogUpdater.addParam("CatParentValue", catParentValue, "post");
        catalogUpdater.addParam("CatBoundPopup", catBoundPopup, "post");
        catalogUpdater.addParam("CatBoundDescId", catBoundDescId, "post");
        catalogUpdater.addParam("CatAction", "GetAllValues", "post");
        catalogUpdater.addParam("CatPopupType", this.catPopupType, "post");
        catalogUpdater.addParam("CatMultiple", (this.multiple ? "1" : "0"), "post");
        catalogUpdater.addParam("treeview", this.treeview, "post");
        catalogUpdater.addParam("CatSearch", this.filter, "post");
        catalogUpdater.addParam("CatEditorJsVarName", this.jsVarName, "post");

        catalogUpdater.send(catalogSearchTreatment, this.jsVarName, true);

    }
    this.bBeginLnkFile = false;    //Indique que l'on vient du début de la recherche depuis MRU
    //Fonction appelée lors d'une recherche depuis une MRU d'un champ de liaison
    //Appel eFinderManager, le résultat étant découpé par la fonction commune catalogSearchTreatment
    //  bBegin : Indique que c'est la première recherche depuis les MRU
    this.SearchLnkFile = function (bBegin) {
        // En mode recherche, on effectue un appel AJAX pour récupérer les valeurs depuis la base.
        // A la suite de cet appel AJAX, la fonction catalogSearchTreatment() récupère les valeurs (XML), les ajoute à l'objet eCatalog (addValue()) et rappelle renderValues()
        // pour rafraîchir l'affichage avec les valeurs récupérées

        if (!bBegin || typeof (bBegin) == "undefined")
            this.bBeginLnkFile = false;
        else
            this.bBeginLnkFile = true;

        var descId = getAttributeValue(this.headerElement, 'did');
        var targetTab = getAttributeValue(this.headerElement, 'popId');
        if (targetTab <= 0)
            targetTab = descId;
        targetTab = targetTab - (targetTab % 100);
        this.tab = targetTab;
        var nFileId = GetMasterFileId(this.GetSourceElement().id);

        /*Uservalue*/
        //Récup des info des champs affichés actuellement sur la fiche en cours

        var nFieldTab = nGlobalActiveTab;
        if (this.headerElement)
            nFieldTab = GetMainTableDescId(this.headerElement.id);


        var aUvFldValue = getFieldsInfos(nFieldTab, nFileId);
        /*Fin Uservalue*/
        //MRU détaillée dans le cas de pm01 depuis adresse
        var bSearchMRUdetail = (((nFieldTab == 400) || (nFieldTab == 200)) && (descId == 300));
        if (bSearchMRUdetail) {
            addCss("eudoFont", "FINDER");
            addCss("eControl", "FINDER");
            addCss("eMain", "FINDER");
            addCss("eIcon", "FINDER");
            addCss("eList", "FINDER");
            addCss("eTitle", "FINDER");
            addCss("eContextMenu", "FINDER");
            addCss("eModalDialog", "FINDER");
            addCss("eMemoEditor", "FINDER");
            addCss("eFinder", "FINDER");

            addScript("eFinder", "FINDER");
            addScript("eTools", "FINDER");

            //Initialisation 
            addScript("eList", "FINDER", function () { initHeadEvents(); });

            addScript("eUpdater", "FINDER");
        }
        // Appel AJAX pour récupérer toutes les valeurs
        var catalogUpdater = new eUpdater("mgr/eFinderManager.ashx", (bSearchMRUdetail) ? 1 : null);
        catalogUpdater.ErrorCallBack = catalogErrorSearch;

        //Liste des séparateurs utilisé pour construire des liste, des listes de tableaux...
        var SEPARATOR_LVL1 = "#|#";
        var SEPARATOR_LVL2 = "#$#";
        /*Début - Uservalue - Envoi des informations de d'uservalue à la modale*/
        var uvflstDetail = "";

        //Liste des champs mémo
        //ne pas transférer les champs note pour les uservalue


        var oLstMemoField = document.getElementById("memoIds_" + nFieldTab)
        var aLstMemoField = new Array();
        if (oLstMemoField != null && oLstMemoField.value)
            aLstMemoField = oLstMemoField.value.split(";");

        var bCheckMemo = (Array.prototype.indexOf && typeof (Array.prototype.indexOf) == "function" && aLstMemoField.length > 0);


        //Liste des champ de l'uservalue dont la valeurs a été récupérée
        for (var i = 0; i < aUvFldValue.length; i++) {
            //Nom du param = (IsFound$|$Parameter$|$Value$|$Label)
            if (!aUvFldValue[i])
                continue;

            //ne pas transférer les champs note pour les uservalue
            if (bCheckMemo && aLstMemoField.indexOf(aUvFldValue[i].cellId) != -1) {
                continue;
            }


            if (uvflstDetail != "")
                uvflstDetail = uvflstDetail + SEPARATOR_LVL1;
            try {
                uvflstDetail = uvflstDetail + "1" + SEPARATOR_LVL2 + aUvFldValue[i].descId + SEPARATOR_LVL2 + aUvFldValue[i].newValue + SEPARATOR_LVL2 + aUvFldValue[i].newLabel;
            }
            catch (e) {
                continue;
            }
        }

        //Liste des champs concerné par le uservalue
        catalogUpdater.addParam('UserValueFieldList', encode(uvflstDetail), "post");
        /*Fin - Uservalue - Envoi des informations de d'uservalue à la modale*/

        //Type d'action
        catalogUpdater.addParam("action", (bSearchMRUdetail) ? "searchMRUdetail" : "search", "post");

        //MODE :
        catalogUpdater.addParam("eMode", "2", "post");

        //MRU :
        catalogUpdater.addParam("MRU", encode(this.mruParamValue), "post");

        //Table sur laquelle on recherche
        catalogUpdater.addParam("targetTab", encode(targetTab), "post");
        //id de la fiche de départ
        catalogUpdater.addParam("FileId", nFileId, "post");
        //Champ catalogue sur la fiche de départ
        catalogUpdater.addParam("targetfield", descId, "post");
        //Table de départ
        //catalogUpdater.addParam("tabfrom", nGlobalActiveTab, "post");
        catalogUpdater.addParam("tabfrom", GetMainTableDescId(this.headerElement.id), "post");

        //Recherche
        catalogUpdater.addParam("Search", encode(this.filter), "post");

        catalogUpdater.addParam("type", "lnkfile", "post");

        catalogUpdater.addParam("jsvarname", this.jsVarName, "post");
        //Largeur de la MRU
        var nMRUWidth = this.getMRUWidth();
        if (nMRUWidth > 0)
            catalogUpdater.addParam("width", nMRUWidth, "post");

        if (bSearchMRUdetail) {
            catalogUpdater.send(catalogSearchDetailTreatment, this.jsVarName, true);
        }
        else {
            setWait(true);
            catalogUpdater.send(catalogSearchTreatment, this.jsVarName, true);
        }
    }

    this.addValue = function (value, label, applyFilter) {
        if ((value != '') && (label != '') && (this.values.indexOf(value) == -1 || this.labels.indexOf(label) == -1)) {
            if ((applyFilter && label.toLowerCase().indexOf(this.filter.toLowerCase()) > -1) || !applyFilter) {
                // filtre sur le libellé de la valeur
                this.values.push(value);
                this.labels.push(label);
            }

        }
    };

    //Appel AJAX pour la recherche des suggestions email
    this.SearchEmailSuggestList = function () {

        var fldNom = top.document.querySelector('input[value][ename="COL_200_201"]') || document.querySelector('input[value][ename="COL_200_201"]') || document.querySelector('input[value][ename="COL_400_201"]');
        var fldPrenom = top.document.querySelector('input[value][ename="COL_200_202"]') || document.querySelector('input[value][ename="COL_200_202"]');
        var fldPm = top.document.querySelector('input[value][ename="COL_400_301"]') || document.querySelector('input[value][ename="COL_400_301"]');
        var fldPp = top.document.getElementById('fileDiv_200') || document.getElementById('fileDiv_200');
        var fldAdr = top.document.getElementById('fileDiv_400') || document.getElementById('fileDiv_400');


        //Recuperation informations PM, nom et prenom

        var isPmExist = false;
        var sCurrentView = getCurrentView(document);
        //En création il faut qu'une société soit saisie, en modification on va requêter les PM du PP
        if ((sCurrentView == "FILE_CREATION" && fldPm) || (sCurrentView == "FILE_MODIFICATION" && (fldPp || fldAdr))) {
            isPmExist = true;
        }

        //Si les au moins nom ou prenomt renseigné et qu'une adresse mail n'est pas déjà renseigné
        if (this.sourceElement.value == "" && (fldNom || fldPrenom) && isPmExist) {
            // Appel AJAX pour récupérer toutes les valeurs
            var emailSuggestUpdater = new eUpdater("mgr/eMailSuggestMgr.ashx", null);
            emailSuggestUpdater.ErrorCallBack = emailSuggestUpdater;
            //Type d'action
            emailSuggestUpdater.addParam("action", "searchEmailSuggest", "post");
            //JS var name
            emailSuggestUpdater.addParam("jsvarname", this.jsVarName, "post");
            //PM de liaison 
            emailSuggestUpdater.addParam("pmid", fldPm ? fldPm.getAttribute("lnkid") : 0, "post");
            //Nom
            emailSuggestUpdater.addParam("nomEmail", fldNom ? fldNom.value : "", "post");
            //Prenom
            emailSuggestUpdater.addParam("prenomEmail", fldPrenom ? fldPrenom.value : "", "post");
            //PP id
            emailSuggestUpdater.addParam("ppid", fldPp ? fldPp.getAttribute("fid") : 0, "post");
            //Adr id
            emailSuggestUpdater.addParam("adrid", fldAdr ? fldAdr.getAttribute("fid") : 0, "post");

            emailSuggestUpdater.send(this.emailSuggestTreatment, this.jsVarName, true);
        } else {
            return;
        }
    };

    //Traitement retour appel AJAX suggestion email
    this.emailSuggestTreatment = function (oList, jsVarName, bSilent) {
        //Récuperation de la liste des email
        var oEmailElements = oList.getElementsByTagName("mail");
        var emailObject = window[jsVarName];
        //On efface les précédentes suggestions
        emailObject.values = new Array();
        emailObject.labels = new Array();
        //Si aucun element a afficher on affiche le champs normalement sans popup
        if (oEmailElements.length > 0) {
            for (var i = 0; i < oEmailElements.length; i++) {
                var oCatalogElement = oEmailElements[i];
                var strValue = getXmlTextNode(oCatalogElement.childNodes[0]);
                emailObject.addValue(strValue, strValue, false);
            }
            //Construction de la popup
            emailObject.render();
            //Repositionnement popup
            var currentTarget = emailObject.GetSourceElement();
            var testPos = getAbsolutePosition(currentTarget);
            var targetZIndex = 1;
            if (currentTarget && !isNaN(currentTarget.style.zIndex) && currentTarget.style.zIndex > targetZIndex)
                targetZIndex = currentTarget.style.zIndex + 1;
            emailObject.parentPopup.left = testPos.x;
            emailObject.parentPopup.top = testPos.y + currentTarget.clientHeight + 3; //positionnement sous le champ parent
            emailObject.parentPopup.zIndex = targetZIndex;
            //Affichage popup (ajout du test sur la value pour corriger un bug d affichage popup apres selection valeur
            if (!emailObject.sourceElement.value) {
                emailObject.parentPopup.show();
            } else {
                emailObject.parentPopup.hide();
            }
        } else {
            try {
                if (emailObject.sourceElement.value != " " && emailObject.sourceElement.value && emailObject.sourceElement.value != "" && emailObject.sourceElement.value != "__")
                    emailObject.sourceElement.setAttribute('dbv', target.value);

                // Avant modification de la rubrique, on conserve la valeur
                emailObject.sourceElement.setAttribute('oldval', emailObject.sourceElement.value);
                emailObject.sourceElement.onblur = function () {
                    if (getAttributeValue(that.headerElement, "ignoreblur") == "1") {
                        setAttributeValue(that.headerElement, "ignoreblur", "0");
                        return;
                    }

                    that.setIsEditing(false);
                    that.validate();
                };
            }
            catch (e) {

            }
        }
    };

    this.selectValue = function (value, label, selected) {
        if (selected) {
            if (this.selectedValues.length == 0 || this.selectedValues.indexOf(value) == -1) {
                this.selectedValues.push(value);
                this.selectedLabels.push(label);
            }
        }
        else {
            if (this.selectedValues.length > 0 && this.selectedValues.indexOf(value) > -1) {
                this.selectedValues.splice(this.selectedValues.indexOf(value), 1);
                this.selectedLabels.splice(this.selectedLabels.indexOf(label), 1);
            }
        }
    };


    this.loadMRU = function (target) {
        var descId = this.headerElement.getAttribute("did");
        this.initialLabels = new Array();
        this.initialValues = new Array();
        var oMruInput = null;
        var returnValue = false;

        try {
            // Pas de MRU pour les catalogues mutiples et liés
            if ((this.multiple || this.bndDescId != null) || getAttributeValue(target, "eaction") == "LNKSTEPCAT") {
                if (this.type == "catalogUserEditor")
                    this.OpenUserDialog();
                else
                    this.openAdvancedDialog();

                return false;
            }

            var oeParam = getParamWindow();
            if (oeParam.GetMruParam)
                this.mruParamValue = oeParam.GetMruParam(descId);

            // liste des valeurs
            if (typeof (this.mruParamValue) == 'undefined' || this.mruParamValue == '') {

                if (this.type == "catalogUserEditor")
                    this.OpenUserDialog();
                else if (!this.isFreeText)
                    this.openAdvancedDialog();

                return false;
            }

            var aValues = this.mruParamValue.split('$|$');
            for (var i = 0; i < aValues.length; i++) {
                var aValue = aValues[i].split(';');

                // catalogue avancé : on prend le dataid
                // catalogue simple : on ne prend que la valeur en caractères
                // TODO : remplacer 3 par la constante du catalogue avancé
                if (this.headerElement.getAttribute('pop') == 3 || this.type == "catalogUserEditor")
                    this.initialValues.push(aValue[0]);
                else
                    this.initialValues.push(aValue[1]);

                this.initialLabels.push(aValue[1]);

                returnValue = true;
            }
        }
        catch (ee) {
            returnValue = false;
        }

        return returnValue;
    };

    //Chargement des mru dans les valeurs à afficher
    this.loadMRULinkFile = function (target) {
        var targetTab = getTabDescid(getAttributeValue(this.headerElement, 'popId'));
        var descId = getAttributeValue(this.headerElement, 'did');
        //MRU pour les champs de liaisons principales
        if ((targetTab <= 0) && (descId > 0))
            targetTab = getTabDescid(descId);

        this.initialLabels = new Array();
        this.initialValues = new Array();
        var oMruInput = null;
        var returnValue = false;

        try {
            var oeParam = getParamWindow();
            if (oeParam.GetMruParam)
                this.mruParamValue = oeParam.GetMruParam(targetTab);

            // liste des valeurs
            if (typeof (this.mruParamValue) == 'undefined' || this.mruParamValue == '') {
                this.openLnkFileDialog(FinderSearchType.Link);
                return false;
            }
            //Filtrage des mru avec le uservalue et les fiches liées
            this.SearchLnkFile(true);
            returnValue = true;
        }
        catch (ee) {
            returnValue = false;
        }

        return returnValue;
    };
    //LARGEUR DYNAMIQUE DE LA MRU
    this.getMRUWidth = function () {
        var srcElement = this.GetSourceElement();
        if (this.parentPopup && srcElement && (srcElement.offsetWidth > 0)) {
            if (srcElement.offsetWidth > this.MinimumMruWidth)
                return srcElement.offsetWidth;
            else
                return this.MinimumMruWidth;
        }
        return -1;
    };

    // Effectue le rendu (affichage) de l'éditeur dans sa popup/div parente
    this.render = function () {
        var srcElement = this.GetSourceElement();
        var availableWidth = this.parentPopup.relativeTo.scrollWidth;
        var availableHeight = 0;
        if (this.parentPopup.relativeTo.parentElement)
            availableHeight = this.parentPopup.relativeTo.parentElement.clientHeight;
        else
            availableHeight = this.parentPopup.relativeTo.parentNode.clientHeight;      // Compatibilité FF < 9
        //LARGEUR DYNAMIQUE DE LA MRU
        var nMRUWidth = this.getMRUWidth();
        //ELAIZ - régression 77 152 - popupdiv pour la modification de champs sur les cats
        if (sTheme.indexOf('2019') > -1 && this.sourceElement.classList.contains("catEditRub")) {
            this.parentPopup.left = 35;
            nMRUWidth = parseInt(getComputedStyle(this.sourceElement.ownerDocument.querySelector('#eCEDValues'), null).getPropertyValue("width")) - 30
        }

        if (nMRUWidth > 0)
            this.parentPopup.width = nMRUWidth;
        if (isNaN(parseInt(this.parentPopup.width)))
            this.parentPopup.width = 0;
        if (isNaN(parseInt(this.parentPopup.height)))
            this.parentPopup.height = 0;
        if (isNaN(parseInt(this.parentPopup.top)))
            this.parentPopup.top = (availableHeight - this.parentPopup.height) / 2;
        if (isNaN(parseInt(this.parentPopup.left)))
            this.parentPopup.left = (availableWidth - this.parentPopup.width) / 2;

        var strPopupWidth = 'width: ' + this.parentPopup.width + 'px;';
        var strPopupHeight = 'height: ' + this.parentPopup.height + 'px;';

        if (this.parentPopup.width == 0)
            strPopupWidth = '';

        if (this.parentPopup.height == 0)
            strPopupHeight = '';

        //Test pour positionner le div 
        // Pas de mise en forme dans le cas d'un champ Mémo
        this.bRevert = false;
        //        var font = document.getElementById("container").classList[1];

        var font = eTools.GetFontSizeClassName();

        if (this.type != 'memoEditor') {
            if (((this.type == 'catalogEditor' || this.type == 'catalogUserEditor' || this.type == 'linkCatFileEditor') && (this.parentPopup.top + this.parentPopup.height > availableHeight))
                || this.catPopupType == "5") {
                this.bRevert = true;
                this.parentPopup.div.className = "ePopup ePopupRevert " + font;
            }
            else {
                this.parentPopup.div.className = "ePopup " + font;
            }
        }

        this.parentPopup.div.style.position = 'absolute';
        if (this.bRevert) {
            this.parentPopup.top = this.parentPopup.top - this.parentPopup.height + srcElement.offsetHeight - 3; // TODO: offsetHeight pas forcément défini ?
            //GCH : Si il n'y a pas la place en hauteur on ouvre la popup
            if (this.parentPopup.top < 0) {
                switch (this.type) {
                    case "linkCatFileEditor":
                        this.openLnkFileDialog(FinderSearchType.Link);
                        return;
                        break;
                    case "catalogUserEditor":
                        this.OpenUserDialog();
                        return;
                        break;
                    case 'catalogEditor':
                        this.openAdvancedDialog(2);
                        return;
                        break;
                }
            }
        }
        this.parentPopup.div.style.top = this.parentPopup.top + 'px';
        this.parentPopup.div.style.left = this.parentPopup.left + 'px';
        if (this.parentPopup.width > 0)
            this.parentPopup.div.style.width = this.parentPopup.width + 'px';
        if (this.parentPopup.height > 0)
            this.parentPopup.div.style.height = this.parentPopup.height + 'px';

        var sCpltClass = "";
        if (this.obligat)
            sCpltClass = " obg";

        switch (this.type) {
            case 'catalogEditor':
            case 'catalogUserEditor':
            case 'linkCatFileEditor':
                this.parentPopup.div.style.overflowX = "hidden";
                if (this.type == 'linkCatFileEditor')
                    this.parentPopup.div.style.overflowY = "hidden";
                else
                    this.parentPopup.div.style.overflowY = "auto";

                //enlève l'ancien élément
                if (document.getElementById("eCatalogEditorSearch") != null) {
                    try {

                        document.getElementById("eCatalogEditorSearch").parent.removeChild("eCatalogEditorSearch");
                    }
                    catch (ee) { }
                }

                var iconClass = (this.currentLabels.length > 0) ? "icon-edn-cross sprite-croix-grise" : "icon-magnifier sprite-loupe";
                var closeValue = (this.currentLabels.length > 0) ? "1" : "0";

                var strSearchTR = '<tr>' +
                    '<td style="height:5%;">' +
                    '<div class="searchFieldWrapper">' +
                    '<input style="width:' + (this.parentPopup.width - 25) + 'px;" autocomplete="off" class="eCatalogEditorSearch' + sCpltClass + '" type="text" id="eCatalogEditorSearch" value="" onFocus="if (typeof (select) == \'function\') { select(); }" onblur="" onkeyup="' + this.jsVarName + '.findValues(event, this.value, \'eCatalogEditorValues\');">' +
                    '<div id="ImgSrchCatalog" class="sprite-Editor ' + iconClass + '" close="' + closeValue + '" onclick="if(this.getAttribute(\'close\') == \'1\'){document.getElementById(\'eCatalogEditorSearch\').value=\'\'; ' +
                    this.jsVarName + '.currentLabels = \'\';' +
                    this.jsVarName + '.renderValues(true,true); }"></div>' +
                    '</div>' +
                    '</td>' +
                    '</tr>';

                var jsOpenDialogName = this.jsVarName + '.';
                switch (this.type) {
                    case "linkCatFileEditor":
                        jsOpenDialogName += "openLnkFileDialog(" + FinderSearchType.Link + ")";
                        break;
                    case "catalogUserEditor":
                        jsOpenDialogName += "OpenUserDialog()";
                        break;
                    default:
                        jsOpenDialogName += "openAdvancedDialog(2)";
                        break;
                }

                var strAdvancedTR = '<tr id="eCatalogEditorAdvancedTr" style="" class="eCatalogEditorAdvanced" title="">' +
                    '<td id="eCatalogEditorAdvanced" style="width:' + this.parentPopup.width + 'px;overflow-x: hidden; overflow-y: auto;" class="eCatalogEditorMenuItemAdv"  onClick="' + jsOpenDialogName + '">' +
                    '<span class="sprite-loupe-liste icon-magnifier"></span><span class="eCatalogEditorMenuItemAdvSpan">' + top._res_6193 + '</span>' +
                    '' +
                    '</td>' +
                    '</tr>';
                if (!this.bRevert) {
                    this.parentPopup.div.innerHTML = '<table cellpadding=0 cellspacing=0 style="width:' + this.parentPopup.width + 'px;">' +
                        strSearchTR +
                        '<tr>' +
                        '<td><div id="eCatalogEditorValues" style="width: ' + this.parentPopup.width + 'px; height: ' + (this.parentPopup.height - 49) + 'px; overflow: none; overflow-y: auto; overflow-x: hidden;"></div></td>' +
                        '</tr>' +
                        '<tfoot>' +
                        strAdvancedTR +
                        '<tfoot>' +
                        '</table>';
                }
                else {
                    this.parentPopup.div.innerHTML = '<table cellpadding=0 cellspacing=0 style="width:' + this.parentPopup.width + 'px;">' +
                        '<thead>' +
                        strAdvancedTR +
                        '</thead>' +
                        '<tr>' +
                        '<td><div id="eCatalogEditorValues" style="width: ' + this.parentPopup.width + 'px; height: ' + (this.parentPopup.height - 49) + 'px; overflow-x: none; overflow-y: auto; overflow-x: hidden;"></div></td>' +
                        '</tr>' +
                        strSearchTR +
                        '</table>';
                }

                var doRenderValue = true;
                if (this.type == 'linkCatFileEditor') {
                    var assFieldValue = this.getAssociateFieldValue();
                    if (assFieldValue) {
                        document.getElementById("eCatalogEditorSearch").value = assFieldValue;
                        this.filter = assFieldValue;
                        this.startSearch();
                        doRenderValue = false;
                    }
                }
                if (doRenderValue)
                    this.renderValues(false, true);

                break;
            case 'memoEditor':
                // Paramétrage de la configuration du champ Mémo
                var bHTML = (srcElement.getAttribute('html') == 1);
                var bCompactMode = true;
                var strSkin = 'eudonet';
                var nEditorWidth = this.parentPopup.width;
                var nEditorHeight = this.parentPopup.height - 77; // 77 : hauteur barre d'outils + barre d'état
                var bShowToolBar = true;
                var bToolBarCanCollapse = true;
                var bShowStatusBar = true;

                // Création et instanciation du champ Mémo
                this.memoEditor = new eMemoEditor(
                    'eMemoEditor',
                    bHTML,
                    this.parentPopup.div,
                    this,
                    this.value,
                    bCompactMode,
                    this.jsVarName + '.memoEditor'
                );
                this.memoEditor.autoResetMode = this.autoResetMode;
                this.memoEditor.title = this.catalogTitle;

                // Mise à jour de la configuration de base du champ Mémo (mode HTML) avec les propriétés ci-dessus
                if (bHTML) {
                    this.memoEditor.setSkin(strSkin);
                    // On gère l'affichage de la barre d'outils via une méthode spécifique de memoEditor qui permet d'affecter plusieurs propriétés
                    this.memoEditor.setToolBarDisplay(bShowToolBar, bToolBarCanCollapse);
                    // L'affichage de la barre de statut se fait par activation/désactivation de plugin. On gère ce comportement via une méthode spécifique de memoEditor
                    // car elle ne fait pas partie de celles proposées par l'objet config de CKEditor

                    if (this.memoEditor.toolbarType == "mail") {
                        this.memoEditor.setStatusBarEnabled(true);
                    } else
                        this.memoEditor.setStatusBarEnabled(false);

                }

                // Mise à jour de la configuration globale (HTML ET texte)
                this.memoEditor.config.width = nEditorWidth;
                this.memoEditor.config.height = nEditorHeight;

                // Mode lecture seule ou écriture

                this.memoEditor.readOnly = (srcElement.getAttribute('ero') == "1");
                this.memoEditor.uaoz = (srcElement.getAttribute('uaoz') == "1");
                this.fromParent = (srcElement.getAttribute("fromparent") == "1");

                // Affichage direct du champ Mémo en plein écran
                if (this.switchFullScreen) {
                    this.memoEditor.descId = this.headerElement.getAttribute("did");
                    this.memoEditor.fileId = GetFieldFileId(srcElement.id);
                    this.memoEditor.title = this.headerElement.getAttribute("lib");
                    this.memoEditor.switchFullScreen(true, true);
                }
                else {
                    // Ou création du <textarea> dans le container passé à l'initialisation de eMemoEditor et affichage
                    this.memoEditor.show();
                }
                break;
            case 'dateEditor':
                this.openDateEditor(srcElement, availableWidth, availableHeight);

                break;
            case 'mailEditor':
                var emailObject = window[jsVarName];
                emailObject.renderValues(false, false);
                break;
            default:
                var oInlineEditor = document.createElement('input');
                oInlineEditor.id = "eInlineEditor";
                oInlineEditor.setAttribute("type", "text");
                oInlineEditor.setAttribute("class", "eInlineEditor" + sCpltClass);
                oInlineEditor.setAttribute("onfocus", "this.select();");
                oInlineEditor.setAttribute("onkeydown", this.jsVarName + ".findValues(event, '', '');");
                if (this.maxLength > 0)
                    oInlineEditor.setAttribute("maxlength", this.maxLength);
                oInlineEditor.value = this.value;
                this.parentPopup.div.innerHTML = "";    //On supprimmer les input précédent avant d'en ajouter un nouveau.
                this.parentPopup.div.appendChild(oInlineEditor);
                break;
        }
    };
    this.getAssociateFieldValue = function () {
        var popid = getNumber(this.headerElement.getAttribute("popid"));
        if (popid % 100 == 0)
            popid += 1;
        var sAssFieldParam = "[" + this.headerElement.getAttribute("did") + "]_[" + popid + "]";
        var assField = document.querySelector("[assfld='" + sAssFieldParam + "']");
        if (!assField)
            return "";
        var dbv = getAttributeValue(assField, "dbv");
        return dbv ? dbv : assField.value;
    };
    ///Affichage du calendrier en mode fiche et liste
    //GCH - #36019 - Internationnalisation - Choix de dates
    this.openDateEditor = function (srcElement, availableWidth, availableHeight) {
        var srcObj = srcElement;

        //Récupération des valeurs à afficher en popup avec leurs options*******************************
        var sHour = "";
        var origDate = that.GetCurrentDBValue();
        if (origDate != "") {
            var dtOrigDate = eDate.Tools.GetDateFromString(origDate);
            if (dtOrigDate.getHours() > 0 || dtOrigDate.getMinutes() > 0 || dtOrigDate.getSeconds() > 0)
                sHour += " " + eDate.Tools.MakeTwoDigit(dtOrigDate.getHours()) + ":" + eDate.Tools.MakeTwoDigit(dtOrigDate.getMinutes()) + ":" + eDate.Tools.MakeTwoDigit(dtOrigDate.getSeconds());
        }

        var today = new Date();
        var sToday = eDate.Tools.GetStringFromDate(today, true, false) + sHour;
        var tabItm = new Array();

        // Ouvrir la fenêtre de choix de date
        tabItm.push(new itemDateEditor(top._res_5017, this.jsVarName + ".openAdvancedCalendar(); " + this.jsVarName + ".contextMenu.hide();", "actionItem calendarItem"));

        tabItm.push(new separatorDateEditor());

        tabItm.push(new itemDateEditor(top._res_143, this.jsVarName + ".updDate('" + eDate.ConvertBddToDisplay(sToday) + "'); " + this.jsVarName + ".contextMenu.hide();", "actionItem calendarItem"));

        var tomorrow = addDays(today, 1);
        var sTomorrow = eDate.Tools.GetStringFromDate(tomorrow, true, false) + sHour;;
        tabItm.push(new itemDateEditor(top._res_145, this.jsVarName + ".updDate('" + eDate.ConvertBddToDisplay(sTomorrow) + "'); " + this.jsVarName + ".contextMenu.hide();", "actionItem calendarItem"));
        for (var i = 2; i <= 5; i++) {
            var dt = addDays(today, i);
            var sDt = eDate.Tools.GetStringFromDate(dt, true, false) + sHour;
            tabItm.push(new itemDateEditor(getDayName(dt).substr(0, 3) + " " + eDate.ConvertBddToDisplay(sDt), this.jsVarName + ".updDate('" + eDate.ConvertBddToDisplay(sDt) + "'); " + this.jsVarName + ".contextMenu.hide();", "actionItem calendarItem"));
        }
        tabItm.push(new separatorDateEditor());

        //Une semaine
        var dtWeek = addDays(today, 7);
        var sDtWeek = eDate.Tools.GetStringFromDate(dtWeek, true, false) + sHour;

        tabItm.push(new itemDateEditor(top._res_146, this.jsVarName + ".updDate('" + eDate.ConvertBddToDisplay(sDtWeek) + "'); " + this.jsVarName + ".contextMenu.hide();", "actionItem calendarItem"));

        //15 jours
        var dtFifteen = addDays(today, 15);
        var sDtFifteen = eDate.Tools.GetStringFromDate(dtFifteen, true, false) + sHour;
        tabItm.push(new itemDateEditor(top._res_147, this.jsVarName + ".updDate('" + eDate.ConvertBddToDisplay(sDtFifteen) + "'); " + this.jsVarName + ".contextMenu.hide();", "actionItem calendarItem"));

        //Un mois
        var dtMonth = addDays(today, 30);
        var sDtMonth = eDate.Tools.GetStringFromDate(dtMonth, true, false) + sHour;
        tabItm.push(new itemDateEditor(top._res_148, this.jsVarName + ".updDate('" + eDate.ConvertBddToDisplay(sDtMonth) + "'); " + this.jsVarName + ".contextMenu.hide();", "actionItem calendarItem"));

        tabItm.push(new separatorDateEditor());

        //Aucune date        
        tabItm.push(new itemDateEditor(top._res_314, this.jsVarName + ".updDate(''); " + this.jsVarName + ".contextMenu.hide();", "actionItem calendarItem")); //todores

        //*******************************************************************************************

        //Rendu de haut en bas***********************************************************************
        var obj_pos = getAbsolutePosition(srcObj);
        this.contextMenu = new eContextMenu(null, obj_pos.y, obj_pos.x, this.parentPopup);
        this.contextMenu.autoAdjust = false;    //on ne laisse pas l'autoadjust car on gère ici le fait que le calendrier soit trop grand.

        for (var i = 0; i < tabItm.length; i++) {
            if (tabItm[i].typeName == "itemDateEditor")
                this.contextMenu.addItem(tabItm[i].Libelle, tabItm[i].Js, 0, 1.5, tabItm[i].Css, tabItm[i].Libelle);
            else
                this.contextMenu.addSeparator(0);
        }
        //*******************************************************************************************
        var bShowPopup = false; //si à faux : MRU - si à vrai : Calendrier
        var oPopUpSize = getAbsolutePosition(this.contextMenu.mainDiv);
        var bPopUpTooLong = oPopUpSize && (oPopUpSize.y + oPopUpSize.h > availableHeight);
        var bPopUpTooLarge = oPopUpSize && (oPopUpSize.x + oPopUpSize.w > availableWidth);
        //Si pas la place en largeur on affiche le calendrier
        if (bPopUpTooLarge) {
            bShowPopup = true;
        }
        else if (bPopUpTooLong) {
            //Si pas la place en bas on affiche vers le haut
            var posDwnToUp = (obj_pos.y + obj_pos.h) - oPopUpSize.h; //(Position de l'objet source+sa taille) - la hauteur de la popup actuelle
            var oMainDiv = document.getElementById("mainDiv");
            var bPopUpTooUp = oMainDiv && getAbsolutePosition(oMainDiv).y > posDwnToUp;
            //Si pas la place en hauteur pour afficher de bas en haut on affiche le calendrier.
            if (!bPopUpTooUp) {
                //Rendu de bas en haut***********************************************************************
                this.contextMenu = new eContextMenu(null, posDwnToUp, obj_pos.x, this.parentPopup);
                this.contextMenu.autoAdjust = false;    //on ne laisse pas l'autoadjust car on gère ici le fait que le calendrier soit trop grand.
                for (var i = 0; i < tabItm.length; i++) {//#83 001
                    if (tabItm[i].typeName == "itemDateEditor")
                        this.contextMenu.addItem(tabItm[i].Libelle, tabItm[i].Js, 0, 1.5, tabItm[i].Css, tabItm[i].Libelle);
                    else
                        this.contextMenu.addSeparator(0);
                }
                //*******************************************************************************************
            }
            else
                bShowPopup = true;
        }
        //Si on affiche la popup au lieu de la MRU on ouvre le calendrier et on cache la mru 
        if (bShowPopup) {
            this.openAdvancedCalendar();
            this.contextMenu.hide();
        }
        else {
            //sinon on cable l'évènement de fermeture dès que le curseur est ailleurs que sur la MRU
            if (!this.debugMode) {
                var eDtEdtObj = this;
                var oDateMenuMouseOver = function () {
                    var FltOut = setTimeout(
                        function () {
                            //Masque le menu
                            eDtEdtObj.contextMenu.hide();
                            eDtEdtObj.parentPopup.hide();
                        }
                        , 200);
                    //Annule la disparition
                    setEventListener(eDtEdtObj.contextMenu.mainDiv, "mouseover", function () { clearTimeout(FltOut) });
                };
                //Faire disparaitre le menu
                setEventListener(this.contextMenu.mainDiv, "mouseout", oDateMenuMouseOver);
            }
        }
    };
    ///summary
    /// TODO : A refaire, construire le html en js pas en string
    ///<param name="searchMode">Booleen qualifiant l'etat en mode recherche</param>
    ///<param name="initValues">booleen qualifiant l'initialisation des valeurs</param>
    ///<param name="addAllowed">booleen qualifiant l'autorisation d'ajout d'une nouvelle valeur</param>
    ///summary
    this.renderValues = function (searchMode, initValues, addAllowed, bStart) {

        try {
            var ctrlSearch = document.getElementById("eCatalogEditorSearch");
            if ((ctrlSearch) && (!searchMode) && !(getAttributeValue(this.headerElement, "prt") == "1" && this.GetCurrentDBValue() == "-1"))
                ctrlSearch.value = this.currentLabels;
        }
        catch (ee) { }



        // initValues est passée à false suite à l'appel AJAX de initValues() pour rafraîchir le catalogue à partir des infos récupérées par l'appel AJAX et empêcher leur réinitialisation
        if (initValues)
            this.initValues(searchMode);

        var strList = "<table>";
        var valuesOutput = '';
        var onClickFunction = 'setValue';
        if (this.type == 'mailEditor') {
            onClickFunction = 'setValueMail';
        }

        for (var i = 0; i < this.values.length; i++) {
            if (this.values[i] != "" && this.values[i] != null) {
                valuesOutput += '<tr style="" class="row catalogRow" title=""><td id="CatValueResult_'
                    + i + '" width="' + this.parentPopup.width
                    + '" class="eCatalogEditorMenuItem" onmouseover="ste(event, \'CatValueResult_' + i + '\');" onmouseout="ht();"'
                    + 'dbv="' + this.values[i].replace(/'/g, "\'").replace(/"/g, "&quot;")
                    + '"  onClick="' + this.jsVarName + '.' + onClickFunction + '(\''
                    + encode(this.values[i]).replace(/'/g, "\\\'") + '\', \''
                    + encode(this.labels[i]).replace(/'/g, "\\\'") + '\');ht();">' + encodeHTMLEntities(this.labels[i]) + '</td></tr>';
            }
        }


        // Si aucune valeur n'est à afficher : proposition de création de la valeur
        if (valuesOutput == '') {

            if (bStart)
                var empty_msg = top._res_644;
            else
                var empty_msg = (this.bBeginLnkFile) ? top._res_644 : top._res_6195;    //Si début de recherche depuis champ de liaison on affichae chargement en cours car on attend le retour de l'appel ajax

            valuesOutput += '<tr style="" class="row catalogRow" title=""><td id="CatValueNoResults" width="' + this.parentPopup.width + '" class="eCatalogMenuItemNoRes" dbv="">' + empty_msg + '</td></tr>';

            // On vérifie si c'est un catalogue avancé pour ne pas afficher "ajouter" en mode MRU 
            if (this.headerElement.getAttribute('pop')) {
                this.catPopupType = this.headerElement.getAttribute('pop');
            }

            // "Ajouter" n'apparrait pas pour les catalogues avancés ni pour les Champs de liaison)
            if (addAllowed == true && (this.catPopupType != 3) && (this.type != 'linkCatFileEditor')) {
                var _valtoAdd = encode(document.getElementById('eCatalogEditorSearch').value);
                valuesOutput += '<tr style="" class="row catalogRow" title=""><td id="CatValueAddNew" width="' + this.parentPopup.width + '" class="eCatalogMenuItemAddNew" dbv="" onClick="' + this.jsVarName + '.addCatVal(\'' + _valtoAdd.replace(/'/g, "\\\'") + '\', function (oRes) {  if(getXmlTextNode(oRes.getElementsByTagName(\'success\')[0]) != \'1\'){return;}; ' + this.jsVarName + '.setValue(\'' + _valtoAdd.replace(/'/g, "\\\'") + '\', \'' + _valtoAdd.replace(/'/g, "\\\'") + '\'); } );">' + top._res_6194.replace('<VALUE>', encodeHTMLEntities(document.getElementById('eCatalogEditorSearch').value).replace(/'/g, "\'")) + '</td></tr>';
            }

        }


        //SI catalogue champ de liaison et valeurs déjà sélectionnée on affiche Dissocier
        if (this.type == 'linkCatFileEditor') {
            if ((this.selectedLabels.length > 0) && (this.selectedLabels[0] != ""))
                valuesOutput = '<tr style="" class="row catalogRow" title=""><td id="CatValueAddNew" width="' + this.parentPopup.width + '" class="eCatalogMenuItemAddNew" dbv="" onClick="' + this.jsVarName + '.setValue(\'\', \'\',true);">' + top._res_6333 + '</td></tr>' + valuesOutput;
        }
        else if (this.type == 'catalogEditor' || this.type == 'catalogUserEditor') {
            if ((this.selectedLabels.length > 0) && (this.selectedLabels[0] != ""))
                valuesOutput = '<tr style="" class="row catalogRow" title=""><td id="CatValueAddNew" width="' + this.parentPopup.width + '" class="eCatalogMenuItemAddNew" dbv="" onClick="' + this.jsVarName + '.setValue(\'\', \'\',true);">' + top._res_6211 + '</td></tr>' + valuesOutput;
        }

        var popupId = 'eCatalogEditorValues';
        if (this.type == 'mailEditor') {
            popupId = 'ePopupDiv';
        }

        try {
            document.getElementById(popupId).innerHTML =
                '<table id="eCatalogEditorSearchResults">' +
                '<tbody id="eCatalogEditorSearchResultsBody">' +
                valuesOutput +
                '</tbody>' +
                '</table>';
        }
        catch (ee) { }

        try {

            // ASY (26890) [MRU] - après une recherche tous le texte est sélectionné ce qui empêche de continuer la saisie sans écrase ce que l'on a déjà saisie.
            // Ajout d'un parametre et de la condition pour ne pas perturber l existant ainsi que le dev de MZA-26346
            if (ctrlSearch && !isTablet()) {
                getFocusAfter(ctrlSearch, 1); // ASY : Ajout d'un parametre
            }
        }
        catch (ee) { }

    };

    // Recherche : Fonction appelée lorsque le délai imparti après saisie de caractères est expiré afin de déclencher la recherche
    this.startSearch = function () {
        that.renderValues(true, true, null, true);
    };

    this.selectAndUpdateVal = function (val) {

        //
        var lab = null;
        var bSetValue = false;

        if (this.headerElement.getAttribute('pop') < 2) {
            lab = val;
            bSetValue = true;
        }
        else if (this.selectedCatalogIndex > -1 && this.values.length > this.selectedCatalogIndex && this.labels.length > this.selectedCatalogIndex) {
            val = this.values[this.selectedCatalogIndex];
            lab = this.labels[this.selectedCatalogIndex];
            bSetValue = true;
        }

        if (bSetValue)
            this.setValue(encode(val), encode(lab));
    }
    //
    this.findValues = function (e, val) {

        //ESCAPE => on ferme
        if (e.keyCode == 27) {
            this.cancel();
            this.parentPopup.hide();
            return false;
        }
        //ENTER
        if (e.keyCode == 13) {
            //Si catalogue ou champ de liaison on valide la valeurs sélectionnée
            if ((this.type == 'catalogEditor') || (this.type == 'linkCatFileEditor')) {
                this.selectAndUpdateVal(val);
            }
            else {
                var bDontHide = false;
                switch (this.action) {
                    /// /!\ !!!! TOUTES CES ACTIONS DOIVENT AUSSI ETRE PRESENTES DANS ePopup.js sur hideonblur !!!! /!\

                    case "GAdate":

                        break;
                    // répartition des fiches lors du traitement de fiche sur appartient à
                    case 'GARepNb':
                        updNb(this.sourceElement)
                        break;
                    case 'renameCatalogValue':
                        this.renameCatalogValue();
                        break;
                    case 'renameMarkedFileValue':
                        this.renameMarkedFileValue();
                        break;
                    case 'renameTreeViewValue':
                        this.renameTreeViewValue();
                        break;
                    case 'renameFilter':
                        // -> pas de confirme sur le rename filter
                        //permet de garder le contexte d'exécution
                        //  var myFunct = (function (obj) { return function () { obj.renameFilter(); } })(that);
                        //eConfirm(1, "Renommer", "Renommer" + "?", "", 500, 200, myFunct );
                        that.renameFilter();
                        break;
                    case 'renameFormular':
                        that.renameFormular();
                        break;
                    case 'renameMailTpl':
                        // -> pas de confirm sur le rename de modèle de mail, permet de garder le contexte d'exécution
                        that.renameMailTpl();
                        break;
                    case 'renameView':

                        //permet de garder le contexte d'exécution                                
                        var sViewName = this.value;
                        var sNewViewName = "";

                        var inptEditor = document.getElementById('eInlineEditor');
                        if (inptEditor)
                            sNewViewName = inptEditor.value;

                        if (sNewViewName != sViewName) {
                            var myFunct = (function (obj) { return function () { obj.renameView(); obj.parentPopup.hide(); } })(that);
                            eConfirm(1, top._res_86, top._res_268.replace("<OLDNAME>", sViewName).replace("<NEWNAME>", sNewViewName), "", null, null, myFunct, function () { that.parentPopup.hide() });
                            bDontHide = true;
                        }


                        break;


                    case 'renameReport':
                        // -> pas de confirm sur le rename Report, permet de garder le contexte d'exécution
                        that.renameReport();
                        break;
                    case 'editFullScreenValue':
                        // Avec eFieldEditor en mode Edition plein écran (ex : champ Mémo en mode Liste), il ne faut PAS déclencher la validation du champ affiché en plein écran lorsqu'on en sort
                        // Cette validation doit être gérée par la fenêtre Plein écran elle-même, avec ses boutons Valider/Annuler

                        break;
                    case 'renameFile':
                        // renameFile se trouve dans pj.js
                        if (typeof (renameFile) == "function") {
                            var sFileName = this.value;
                            var sNewFileName = "";

                            var inptEditor = document.getElementById('eInlineEditor');
                            if (inptEditor)
                                sNewFileName = inptEditor.value;

                            if (sFileName != sNewFileName) {
                                var myFunct = (function (obj) { return function () { renameFile(obj); } })(that);
                                eConfirm(1, top._res_86, top._res_6692.replace("<OLDNAME>", sFileName).replace("<NEWNAME>", sNewFileName), "", null, null, myFunct, function () { that.parentPopup.hide() });
                                bDontHide = true;
                            }
                        }
                        break;
                    case 'renameImportTemplate':
                        // -> pas de confirm sur le rename du modèle d'import, permet de garder le contexte d'exécution
                        var idTemp = GetFieldFileId(this.sourceElement.id);
                        var inptEditor = document.getElementById('eInlineEditor');
                        var sNewFileName = '';
                        if (inptEditor)
                            sNewFileName = inptEditor.value;
                        if (trim(sNewFileName) != '')
                            eCurrentSelectedTemplateImport.ImportTemplateInternal.RenameTemplate(this.sourceElement, idTemp, sNewFileName);
                        break;
                    default:
                        this.validate();
                        break;
                }
                if (!bDontHide)
                    this.parentPopup.hide();
            }
            return false;
        }

        if ((this.type == 'catalogEditor') || (this.type == 'catalogUserEditor') || (this.type == 'linkCatFileEditor')) {

            if (e.keyCode == 38)   //Haut
            {
                if (document.getElementById("CatValueResult_" + (this.selectedCatalogIndex - 1)) != null) {
                    try {
                        document.getElementById("CatValueResult_" + this.selectedCatalogIndex).className = "eCatalogEditorMenuItem";
                    }
                    catch (ee) { }
                    this.selectedCatalogIndex--;
                    document.getElementById("CatValueResult_" + this.selectedCatalogIndex).className = "eCatalogMenuSelected";
                }
                try {
                    document.getElementById("eCatalogEditorSearch").value = document.getElementById("CatValueResult_" + this.selectedCatalogIndex).innerHTML;
                }
                catch (ee) { }

                // this.parentPopup.hide();

                return false;
            }
            if (e.keyCode == 40)   //bas
            {

                if (document.getElementById("CatValueResult_" + (this.selectedCatalogIndex + 1)) != null) {
                    try {
                        document.getElementById("CatValueResult_" + this.selectedCatalogIndex).className = "eCatalogEditorMenuItem";
                    }
                    catch (ee) { }
                    this.selectedCatalogIndex++;
                    document.getElementById("CatValueResult_" + this.selectedCatalogIndex).className = "eCatalogMenuSelected";
                }
                try {
                    document.getElementById("eCatalogEditorSearch").value = document.getElementById("CatValueResult_" + this.selectedCatalogIndex).innerHTML;
                }
                catch (ee) { }

                //this.parentPopup.hide();

                return false;
            }

            // Pas de recherche si la valeur a recherché n'a pas changé
            if ((val == this.filter) || ((this.filter == '') && (val == this.currentLabels))) // TODO catalogue multiple
                return false;

            if (document.getElementById("ImgSrchCatalog")) {
                var iconClass = (val.length > 0) ? "icon-edn-cross sprite-croix-grise" : "icon-magnifier sprite-loupe";
                document.getElementById("ImgSrchCatalog").className = "sprite-Editor " + iconClass;
                document.getElementById("ImgSrchCatalog").setAttribute("close", "1");
            }

            this.filter = val;

            clearTimeout(this.searchTimer);
            this.searchTimer = window.setTimeout(this.startSearch, 500);
        }
    };

    // mise à jour de la valeur date
    this.updDate = function (value) {
        this.setValue(value, value);
    };

    /// MOU 22-10-2014 #33153
    /// Met à jour la nouvelle valeur saisie quand on clique à l'exterieur du catalog editor (catalog saisie libre ..)
    this.setValueOnHide = function () {
        var sourceElement = this.GetSourceElement();
        var eAction = sourceElement.getAttribute("eaction");
        switch (eAction) {
            //catalog simple à saisie libre
            case "LNKFREECAT":
                var element = findElementById(this.parentPopup.div, "eCatalogEditorSearch");
                if (element != null)
                    this.setValue(encode(element.value), encode(element.value));
                break;
            default:
        }
    }

    // Déclenche la mise à jour de la valeur du champ (en interne au sein de la fonction puis en base)
    this.setValue = function (value, label, bNew, bFromLastValue) {


        if (typeof (bNew) == 'undefined') {
            bNew = this.isNewValue;
        }

        value = decode(value);
        label = decode(label);
        // En mode simple, on appelle selectValue directement, qui équivaut à "cocher" la valeur de catalogue, puis on valide la popup
        if (!this.multiple) {
            this.selectedValues = new Array();
            this.selectedLabels = new Array();
            this.selectValue(value, label, true);
            // Puis on met à jour la valeur sur la page et en base
            this.validate(bNew, bFromLastValue);
            this.parentPopup.hide();
            this.filter = '';
        }
        else
            this.selectValue(value, label, true);
    };

    // Déclenche la mise à jour de la valeur du champ email (en interne au sein de la fonction puis en base)
    this.setValueMail = function (value, label, bNew) {

        var srcElement = this.GetSourceElement();
        if (typeof (bNew) == 'undefined') {
            bNew = this.isNewValue;
        }

        value = decode(value);
        label = decode(label);
        // En mode simple, on appelle selectValue directement, qui équivaut à "cocher" la valeur de catalogue, puis on valide la popup
        srcElement.value = value;
        srcElement.setAttribute('oldval', value);
        // Puis on met à jour la valeur sur la page et en base
        this.validate(bNew);
        this.parentPopup.hide();
        this.filter = '';
    };

    // Renomme une valeur de catalogue
    this.renameCatalogValue = function () {
        var newLabel = "";

        if (document.getElementById('eInlineEditor'))
            newLabel = document.getElementById('eInlineEditor').value;
        else
            return;



        var oCell = this.GetSourceElement();
        var catId = oCell.parentNode.parentNode.getAttribute('ednid');
        var parentid = oCell.parentNode.parentNode.getAttribute('parentid');
        var oldLabel = oCell.innerHTML;
        var oTable = oCell.parentNode.parentNode.parentNode;
        var descId = oTable.getAttribute('descid');
        var popup = oTable.getAttribute('pop');

        var url = "mgr/eCatalogManager.ashx";

        var oUpdater = new eUpdater(url, null);
        oUpdater.ErrorCallBack = function () { };
        //Ajout des params
        oUpdater.addParam("operation", 1, "post"); // 1 = eCatalog.Operation.change
        oUpdater.addParam("descid", descId, "post");
        oUpdater.addParam("pop", popup, "post");
        oUpdater.addParam("parentid", parentid, "post");
        oUpdater.addParam("label", oldLabel, "post");
        oUpdater.addParam("newlabel", newLabel, "post");
        oUpdater.addParam("id", catId, "post");

        oUpdater.send(afterRenameCatalogValue, oCell);
    };

    // Renomme une valeur de catalogue
    this.addCatVal = function (value, callbackFunction) {
        value = decode(value);
        var newLabel = value;

        var oCell = this.GetSourceElement();
        var descId = this.headerElement.getAttribute('did');
        var popupdescid = getAttributeValue(this.headerElement, 'popid');

        if (popupdescid === "undefined" || popupdescid === null)
            popupdescid = descId;

        var popupType = this.headerElement.getAttribute('pop');
        var parentValue = oCell.getAttribute('pdbv');
        var catBoundDescId = this.headerElement.getAttribute('bndid');
        var catBoundPopup = this.headerElement.getAttribute('bndpop');
        var url = "mgr/eCatalogManager.ashx";

        var oUpdater = new eUpdater(url, null);
        oUpdater.ErrorCallBack = function () { };
        //Ajout des params
        oUpdater.addParam("operation", 0, "post"); // 0 = Operation.new
        oUpdater.addParam("descid", popupdescid, "post");
        //oUpdater.addParam("pop", 1, "post");
        oUpdater.addParam("pop", popupType, "post");
        oUpdater.addParam("parentid", -1, "post");
        oUpdater.addParam("parentvalue", parentValue, "post");
        oUpdater.addParam("catbounddescid", catBoundDescId, "post");
        oUpdater.addParam("catboundpopup", catBoundPopup, "post");
        oUpdater.addParam("newlabel", newLabel, "post");
        oUpdater.addParam("id", 0, "post");

        oUpdater.send(callbackFunction, oCell);
    };

    // Renomme une sélection de fiches marquées
    this.renameMarkedFileValue = function () {
        var newLabel = "";

        if (document.getElementById('eInlineEditor'))
            newLabel = document.getElementById('eInlineEditor').value;
        else
            return;

        var oCell = this.GetSourceElement();
        var nMarkedFileId = oCell.parentNode.getAttribute('ednid');
        var oldLabel = oCell.innerHTML;

        var oUpdater = new eUpdater("mgr/eMarkedFilesManager.ashx", null);
        oUpdater.ErrorCallBack = function () { };
        //Ajout des params
        oUpdater.addParam("oldlabel", oldLabel, "post");
        oUpdater.addParam("label", newLabel, "post");
        //Ajout des params
        oUpdater.addParam("type", 6, "post");
        oUpdater.addParam("markedfileid", nMarkedFileId, "post");

        oUpdater.send(afterRenameMarkedFileValue, oCell);
    };

    // Renomme une valeur de catalogue arborescent
    this.renameTreeViewValue = function () {
        var srcElement = this.GetSourceElement();
        // Indication visuelle de mise à jour
        if (srcElement.innerText)
            srcElement.innerText = this.parentPopup.div.childNodes[0].value;
        else
            srcElement.textContent = this.parentPopup.div.childNodes[0].value;

        // Mise à jour en base
        var oCell = srcElement;
        var catId = oCell.parentNode.getAttribute('ednid');
        var parentid = oCell.parentNode.getAttribute('parentid');
        var oldLabel = oCell.innerHTML;
        var oTable = oCell.parentNode.parentNode.parentNode;
        var descId = oTable.getAttribute('descid');
        var popup = oTable.getAttribute('pop');
        var url = "mgr/eCatalogManager.ashx";
        var oUpdater = new eUpdater(url, null);
        oUpdater.ErrorCallBack = function () { }
        // Ajout des params
        oUpdater.addParam("operation", 1, "post"); // 1 = eCatalog.Operation.change
        oUpdater.addParam("descid", descId, "post");
        oUpdater.addParam("pop", popup, "post");
        oUpdater.addParam("parentid", parentid, "post");
        oUpdater.addParam("newlabel", oldLabel, "post");
        //        oUpdater.addParam("newlabel", newLabel, "post");
        oUpdater.addParam("id", catId, "post");

        oUpdater.send(afterRenameTreeViewValue, oCell);

    };

    // Renommage de filtre
    this.renameFilter = function () {
        var newName = '';
        if (document.getElementById('eInlineEditor'))
            newName = document.getElementById('eInlineEditor').value;

        if (newName.length == 0) {

        }
        else {


            if (parentPopup.sourceElement && parentPopup.sourceElement.getAttribute("ename") == "COL_104000_104001") {

                var nFilterID = GetFieldFileId(parentPopup.sourceElement.id);
                var url = "mgr/eFilterWizardManager.ashx";
                var ednu = new eUpdater(url, 0);
                ednu.addParam("action", "rename", "post");
                ednu.addParam("filtername", newName, "post");
                ednu.addParam("filterid", nFilterID, "post");
                ednu.ErrorCallBack = onRenErrTreatment;
                ednu.send(onRenOkTreatment);        // fonction dans eTabsFieldsSelect.js
            }
        }

    };
    //Renommage du template d'import
    this.renameImportTemplate = function () {
        var newName = '';
        if (document.getElementById('eInlineEditor'))
            newName = document.getElementById('eInlineEditor').value;

        if (newName.length > 0 && trim(newName) != '') {
            var nFilterID = GetFieldFileId(parentPopup.sourceElement.id);
            eCurrentSelectedTemplateImport.ImportTemplateInternal.RenameTemplate(parentPopup.sourceElement, nFilterID, newName);
        }
    };
    // Renommage de formulaire
    this.renameFormular = function () {
        var newName = '';
        if (document.getElementById('eInlineEditor'))
            newName = document.getElementById('eInlineEditor').value;

        if (newName.length == 0) {

        }
        else {
            if (parentPopup.sourceElement && parentPopup.sourceElement.getAttribute("ename") == "COL_113000_113001") {

                var nFormularId = GetFieldFileId(parentPopup.sourceElement.id);
                //top.setWait(true);
                var url = "mgr/eFormularManager.ashx";
                var ednu = new eUpdater(url, 0);
                ednu.addParam("operation", "rename", "post");
                ednu.addParam("id", nFormularId, "post");
                ednu.addParam("label", newName, "post");
                ednu.ErrorCallBack = function () { top.setWait(false); };
                ednu.send(onRenameFormularTrait);

            }
        }

    };

    // Renommage de vue d'onglets
    this.renameView = function () {


        var url = "eTabsSelectDiv.aspx";
        var selectionname;
        if (document.getElementById('eInlineEditor'))
            selectionname = document.getElementById('eInlineEditor').value;
        else
            return;

        if (selectionname == "")
            return;

        var oCell = this.GetSourceElement();
        var aTrId = oCell.getAttribute('id').split('_')
        var selId;
        if (aTrId.length >= 3) {
            selId = aTrId[2];
        }

        var ednu = new eUpdater(url, 0);
        ednu.addParam("action", "rename", "post");
        ednu.addParam("selname", selectionname, "post");
        ednu.addParam("selid", selId, "post");
        ednu.ErrorCallBack = function () { };
        ednu.send(onRenOkTreatment);        // fonction dans eTabsFieldsSelect.js
    };



    ///Retourne la valeur du champ actuellement stockée
    // Il s'agit en général de la valeur d'origine sauf si un validate a été appelé
    this.GetCurrentDBValue = function () {

        var srcElement = this.GetSourceElement();
        if (srcElement == null)
            return "";

        var origValue = "";

        //Récupération de la valeur d'origine
        if (srcElement.getAttribute("dbv")) {
            // Cas général : une DataBaseValue est présente
            origValue = srcElement.getAttribute("dbv");
        }
        else if (srcElement.getAttribute("eaction") && srcElement.getAttribute("eaction") == "LNKDATE" && srcElement.getAttribute("value")) {
            //Type date
            origValue = srcElement.getAttribute("value");
        }
        else if (this.type == 'eCheckBox' && srcElement.firstChild && srcElement.firstChild.tagName == "A") {
            // Type de champ checkbox
            origValue = srcElement.firstChild.getAttribute("chk");

            //La checkbox est un cas particulier : la valeur est changée" dans l'élément de façon extérieur a eFieldEditor
            // il faut donc inverser la oldvalue
            if (origValue == "1")
                origValue = "0";
            else
                origValue = "1";
        }
        else if (this.type == 'eBitButton') {
            origValue = getAttributeValue(srcElement.querySelector("a"), "chk");

            if (origValue == "1")
                origValue = "0";
            else
                origValue = "1";
        }
        else if (this.type == 'stepCatalogEditor') {
            origValue = getAttributeValue(srcElement.querySelector("ul.chevronCatalog li.selectedValue a"), "dbv");
        }
        else if (this.type != 'catalogEditor' && srcElement.innerHTML) {
            // TOCHECK : type memo ??
            origValue = srcElement.innerHTML;
        }


        return origValue;
    }


    // Vérifie que la nouvelle valeur saisie soit différente de la valeur actuelle du champ,
    // et si tel est le cas, déclenche la mise à jour du champ sur la page et en base
    // bNew : indique si la valeur est une nouvelle valeur de catalogue
    this.validate = function (bNew, bFromLastValue) {




        if (this.debugMode) { console.log("eFieldEditor.validate() :\r\n bNew : " + bNew + " - bFromLastValue : " + bFromLastValue); }
        var cleanOnBlur = false;
        var bDate = false;	//GCH - #35859 - Internationnalisation Date - Fiche
        var bNumerique = false; //GCH - #36022 - Internationalisation Numerique - Fiche
        // Test si le champ est toujours accessible

        if (!this.headerElement)
            this.headerElement = document.getElementById(this.headerElementId);

        if (document.getElementById(this.headerElementId) && document.getElementById(this.headerElementId) !== this.headerElement) {

            this.headerElement = document.getElementById(this.headerElementId);
        }

        var srcElement = this.GetSourceElement();
        if (srcElement == null)
            return;

        if (srcElement.id !== document.getElementById(srcElement.id) && document.getElementById(srcElement.id)) {
            srcElement = document.getElementById(srcElement.id)
        }

        this.setIsEditing(false);
        that.validateLaunch = true;


        if (getAttributeValue(this.headerElement, "sys") == "1")
            this.UpdateOnBlur = false;
        else if (typeof (isUpdateOnBlur) == "function")
            this.UpdateOnBlur = isUpdateOnBlur();

        if (this.debugMode) { console.log("eFieldEditor.validate() : UpdateOnBlur : " + this.UpdateOnBlur); }

        //GCH - #35859 - Internationnalisation Date - Fiche
        bDate = (getAttributeValue(this.headerElement, "frm") == FLDTYP.DATE);
        //GCH - #36869 , #36022 - Internationalisation Numerique - Fiche
        bNumerique = (isFormatNumeric(getAttributeValue(this.headerElement, "frm")));

        var valueSaved = false;
        var oldValue = '';

        if (typeof (bNew) == 'undefined') {
            bNew = this.isNewValue;
        }

        if (typeof (bFromLastValue) == 'undefined') {
            bFromLastValue = false;
        }

        //Récupération de la valeur d'origine
        if (srcElement.getAttribute("dbv") || this.type == "stepCatalogEditor") {
            // Cas général : une DataBaseValue est présente
            oldValue = getAttributeValue(srcElement, "dbv");
        }
        else if (hasClass(srcElement, "LNKDATE") && srcElement.getAttribute("value")) {
            //Type date
            oldValue = srcElement.getAttribute("value");
        }
        else if (this.type == 'eCheckBox' && srcElement.firstChild && srcElement.firstChild.tagName == "A") {
            // Type de champ checkbox
            oldValue = document.getElementById(srcElement.id).firstChild.getAttribute("chk");

            //La checkbox est un cas particulier : la valeur est changer dans l'élément de façon extérieur a eFieldEditor
            // il faut donc inverser la oldvalue
            if (oldValue == "1") {
                oldValue = "0";
            }
            else {
                oldValue = "1";
            }

        }
        else if (this.type == 'eBitButton') {
            oldValue = getAttributeValue(document.getElementById(srcElement.id).querySelector("a"), "chk");

            if (oldValue == "1")
                oldValue = "0";
            else
                oldValue = "1";

        }
        else if (this.type != 'catalogEditor' && srcElement.innerHTML) {
            // TOCHECK : type memo ??
            oldValue = srcElement.innerHTML;
        }
        if (this.debugMode) { console.log("eFieldEditor.validate() :\r\n oldValue : " + oldValue); }

        var sCurrentView = getCurrentView(document);

        //Récupération de la valeur affichable d'origine
        var oldLabel = '';

        if (srcElement.tagName == 'INPUT') {

            oldLabel = srcElement.value;
            // Inutile de sauvegarder la oldvalue dans le cas d'un catalogue car la valeur est choisi dans une popup et non en saisie dans l'input directement.
            if (this.type != 'catalogEditor') {
                if (srcElement.getAttribute('oldval'))
                    oldLabel = srcElement.getAttribute('oldval');
                else if (oldValue == "")
                    oldLabel = "";
            }
            else if (this.catPopupType == '3' || this.catPopupType == '2') {
                // HLA - Ajout du type catalogue simple pour transmettre le libelle pour ainsi conserver la valeur si jamais on rencontre une erreur a la saisie
                //Dans le catalogue avancé, il faut tracké le libellé de la valeur pour réactualiser en case de renommage
                oldLabel = srcElement.getAttribute('value');
            }
        }
        else if (this.type == "eCheckBox" || this.type == "eBitButton") {
            oldLabel = (oldValue == "1") ? top._res_58 : top._res_59;
        }
        else if (sCurrentView == "LIST" && (this.catPopupType == '3' || this.catPopupType == '2')) {
            if (srcElement.tagName == 'TD') {
                oldLabel = srcElement.innerHTML;
            }
        }
        else if (this.type == "stepCatalogEditor") {
            var a = srcElement.querySelector("ul li a[dbv='" + oldValue + "']");
            if (a) {
                oldLabel = a.innerHTML;
            }

        }
        else if (this.type == "catalogUserEditor") {
            oldLabel = srcElement.innerText;
        }

        // Gestion de la valeur vide
        // TOCHECK : Effet du removehtml sur l'annulation de validation
        // le eEngine va replacer dans le champ la valeur  d'origine avec un trim + un removehtml, 
        // ce qui peut faire des différences...
        if (oldValue != null)
            oldValue = removeHTML(oldValue);

        /**************  Init  ***************************/
        var descid = this.headerElement.getAttribute('did'); // DescId du champ
        this.catPopupType = this.headerElement.getAttribute('pop');

        var newValue = '';
        var newLabel = '';



        /****************************************************************************************/
        /***** recherche de la nouvelle valeur à affecter *******************************/
        if (
            (this.type == 'catalogEditor' && !this.bEditCatAsInline)
            || (this.type == 'catalogUserEditor')
            || (this.type == 'linkCatFileEditor')
            || (this.type == 'dateEditor')
            || (this.type == 'fileEditor')
        ) {
            var oId = srcElement.id.split('_');
            var nTab = oId[1];
            var tabAdr = "";
            var nAdrId = "";
            var sAdr01 = "";

            //catalogue
            for (var i = 0; i < this.selectedValues.length; i++) {
                if (i > 0) {
                    newValue += ';';
                    newLabel += ';';
                }

                newValue += this.selectedValues[i];
                newLabel += this.selectedLabels[i];
            }



            //Traitement des retour de valeur de champs de liaison avec cascade sur PM - ADR et autres
            if (newValue.indexOf(";|;") >= 0) {
                var tabNewValue = newValue.split(";|;");
                var bDoCascade = true;
                if (descid == 200) {    //MRU de contact on récupère Les infos d'adresses et sociétés liées

                    var nPpId = tabNewValue[0];
                    newValue = nPpId;
                    var sPp01 = newLabel;

                    var tabPm = tabNewValue[2].split('$|$');
                    var nPmId = tabPm[0];
                    var sPm01 = tabPm[1];

                    if (tabNewValue.length > 3) {
                        tabAdr = tabNewValue[3].split('$|$');
                        nAdrId = tabAdr[0];
                        sAdr01 = tabAdr[1];
                    }
                    else {
                        tabAdr = tabNewValue[1].split('$|$');
                        nAdrId = tabAdr[0];
                        sAdr01 = tabAdr[1];
                    }

                    var elt = GetField(nTab, 201);
                    if (elt && getAttributeValue(elt, "nocc") == "1")
                        bDoCascade = false;

                }
                else if (descid == 300) {    //MRU de contact on récupère Les infos d'adresses et sociétés liées


                    nPmId = tabNewValue[0];
                    newValue = nPmId;
                    sPm01 = newLabel;


                    var tabPp = tabNewValue[1].split('$|$');
                    var nPpId = tabPp[0];
                    var sPp01 = tabPp[1];

                    if (tabNewValue.length > 3) {
                        tabAdr = tabNewValue[3].split('$|$');
                        nAdrId = tabAdr[0];
                        sAdr01 = tabAdr[1];
                    }

                    var elt = GetField(nTab, 301);
                    if (elt && getAttributeValue(elt, "nocc") == "1")
                        bDoCascade = false;


                }
                else {    //sinon MRU de event on récupère Les infos de Contact et Société liées


                    newValue = tabNewValue[0];

                    if (descid == 400) {
                        nAdrId = newValue;
                        sAdr01 = newLabel;
                    }
                    else if (tabNewValue.length > 3) {
                        tabAdr = tabNewValue[3].split('$|$');
                        nAdrId = tabAdr[0];
                        sAdr01 = tabAdr[1];
                    }

                    var tabPp = tabNewValue[1].split('$|$');
                    var nPpId = tabPp[0];
                    var sPp01 = tabPp[1];

                    var tabPm = tabNewValue[2].split('$|$');
                    var nPmId = tabPm[0];
                    var sPm01 = tabPm[1];



                }

                //kha le 11/08/2015 demande 39 624 uniquement sur les liaisons systèmes
                // sph 20/06/2016 : cascade devient optionnelle
                if (descid % 100 == 0 && bDoCascade)
                    setLinkedFiles(descid, nAdrId, sAdr01, nPmId, sPm01, nPpId, sPp01, nTab);


                //Force la maj pour la liaison dont on vient
                //if (descid == 200) {
                //    setLinkedFile(nTab, 201, newValue, newLabel, true);
                //}
                //else if (descid == 300) {
                //    setLinkedFile(nTab, 301, newValue, newLabel, true);
                //}
                //else if (descid == 400) {
                //    setLinkedFile(nTab, 401, newValue, newLabel, true);
                //}
                //BSE #52 580
                if (getNumber(descid) % 100 == 0 && getNumber(descid) != 200 && getNumber(descid) != 300 && getNumber(descid) != 400)
                    setLinkedFile(nTab, getNumber(descid) + 1, newValue, newLabel, true);
            }
            else if (this.type == 'linkCatFileEditor' && this.tab == newLabel) {
                newLabel = "";  //on met vide plutôt que le tabid
            }
            else if (descid == 200) {

                setLinkedFile(nTab, 201, newValue, newLabel, true);
                //GCH #36048 : Si contact sans adresse sélectionnée on vide l'adresse sélectionnée
                // CRU : on vide aussi le PM
                // SPH : Seulement si ++ et si newValue != oldvalue et pour les ++
                if (newValue != oldValue) {

                    var oDiv = document.querySelector("div[did='" + nTab + "'][id^='fileDiv']");

                    if (oDiv) {

                        var tabType = getNumber(getAttributeValue(oDiv, "edntype"));
                        //56189 on ne vide pas pm si on vide pp
                        if (newValue != "" && (tabType == 2 || tabType == 1)) {
                            setLinkedFile(nTab, 301, null, null, true);
                            setLinkedFile(nTab, 401, null, null, true);
                        }
                    }
                }
            }
            else if (descid == 300) {
                setLinkedFile(nTab, 301, newValue, newLabel, true);
            }
            else if (descid == 400) {
                setLinkedFile(nTab, 401, newValue, newLabel, true);
            }
        }
        else if (this.type == "geolocEditor" && !this.bEditCatAsInline) {
            if (this.selectedValues && this.selectedValues.length > 0) {
                newValue = this.selectedValues[0];
                newLabel = this.selectedLabels[0];
            }

        }
        else if (this.type == 'eCheckBox' && srcElement.firstChild && srcElement.firstChild.tagName == "A") {
            //Checkbox
            newValue = document.getElementById(srcElement.id).firstChild.getAttribute("chk");
            newLabel = newValue;
        }
        else if (this.type == 'eBitButton') {
            newValue = getAttributeValue(document.getElementById(srcElement.id).querySelector("a"), "chk");
            newLabel = newValue;
        }
        else if (this.type == 'stepCatalogEditor') {
            var selectedItem = srcElement.querySelector(".chevronCatalog li.selectedValue a");
            if (selectedItem) {
                newValue = getAttributeValue(selectedItem, "dbv");
                newLabel = GetText(selectedItem);
            }

        }
        else if (this.type == 'memoEditor') {
            // Memo
            var memoData = this.value;
            memoData = this.memoEditor.getData();
            newValue = memoData;
            newLabel = memoData;
        }
        else if (srcElement.tagName == 'INPUT') {
            // Champ standard du Mode Edition Fiche
            newValue = srcElement.value;
            newLabel = newValue;
        }
        else if (this.parentPopup.div.childNodes[0] && this.parentPopup.div.childNodes[0].value) {
            //Champ standard
            newValue = this.parentPopup.div.childNodes[0].value;
            newLabel = this.parentPopup.div.childNodes[0].value;
        }

        //Pour les formats dates/num, on doit garder la nouvelle valeur original avant de la transformer au format bdd
        var origNewValue = newValue;

        //GCH - #35859 - Internationnalisation - on permet l'identification des champs au format date pour les convertir au format de la Base de données
        if (bDate) {

            newValue = eDate.ConvertDisplayToBdd(newValue);
            newLabel = eDate.ConvertBddToDisplay(newValue);

            oldValue = eDate.ConvertDisplayToBdd(oldValue);
            oldLabel = eDate.ConvertBddToDisplay(oldValue);


        }
        else if (bNumerique) {
            newValue = eNumber.ConvertDisplayToBdd(newValue, true);
            newLabel = eNumber.ConvertBddToDisplayFull(newValue);

            oldValue = eNumber.ConvertDisplayToBdd(oldValue, true);
            oldLabel = eNumber.ConvertBddToDisplayFull(oldValue);
        }
        else if (srcElement.getAttribute("eaction") == "LNKMAIL") {
            //trim des valeurs
            if (typeof newValue == "string" && newValue.length > 0) {
                var trimedval = newValue.split(';').reduce(function (a, b) { return a.trim() + ';' + b.trim() })
                if (trimedval != newValue) {
                    newValue = trimedval;

                }
            }

        }



        /*********************************************************************/

        /**********  ACTION  **************************************************/
        this.tab = getTabDescid(descid);
        this.fileId = GetFieldFileId(srcElement.id);

        //Champ Obligatoire
        if (this.obligat && newValue == '' && this.fileId > 0) {

            eAlert(0, top._res_372, top._res_373.replace('<ITEM>', getAttributeValue(this.headerElement, "lib")));
            if (this.type == "inlineEditor") {
                srcElement.value = getAttributeValue(srcElement, "oldval");
            }
            return;
        }

        //Validation
        if (!that.IsValueValid(newValue, origNewValue)) {

            var sGenericErrorMessage = top._res_2021.replace("<VALUE>", origNewValue).replace("<FIELD>", this.headerElement.getAttribute("lib"));

            eAlert(0, top._res_92, sGenericErrorMessage, that.SpecificErrorMessge);

            if (this.type == "inlineEditor") {
                srcElement.value = getAttributeValue(srcElement, "oldval");
            }
            return;
        }



        if ((sCurrentView == "FILE_CREATION" || sCurrentView == "FILE_MODIFICATION") && getAttributeValue(srcElement, "adrpro") == "1") {
            //#37334  - écrasement des adresse postal par l'adr pro
            //Si changement adresse pro, on affiche fenêtre de confirmation
            // si on change de PM en cours de création.
            if (getAttributeValue(srcElement, "adrproneedconfirm") == "1") {


                eConfirm(1, '', top._res_6739, '', null, null, function () {
                    setAttributeValue(srcElement, "adrproneedconfirm", "0");
                    setAttributeValue(srcElement, "adrprooverwrite", "1");
                    that.validate();


                }, function () { return; });


                return;
            }
        }


        //SPH #36195 : ajout de la condition sur adrperso : dans ce cas, le champ n'est pas un lien vers pm
        if (getAttributeValue(this.headerElement, "prt") == "1" && !(getAttributeValue(srcElement, 'adrperso') == "1")) {

            this.tab = GetMainTableDescId(srcElement.id);
            if (!this.tab)
                this.tab = nGlobalActiveTab;

            this.fileId = GetMasterFileId(srcElement.id)
            if (!this.fileId)
                this.fileId = getAttributeValue(document.getElementById("fileDiv_" + this.tab), "fid");

            if (!this.fileId)
                this.fileId = 0;

            //MODE LISTE le did est à 1 vu que champ de liaison on doit mettre à 0
            //			est le fileid n'est pas dans filediv
            if (this.fileId <= 0 && (this.tab != descid)) {
                this.fileId = GetMasterFileId(srcElement.id);
                if (getTabDescid(descid) != this.tab)
                    descid = getTabDescid(descid);

            }

        }

        // Mode création ou Popup - JS ET HTML UNIQUEMENT - PAS DE MAJ EN BASE
        if (!this.UpdateOnBlur || getAttributeValue(srcElement, "calconflict") == "1") {

            // Mode création ou Popup
            //  On met juste le html/dom du champ à jour
            //  le traitement des règles/reload/champ lié eventuel est gérer en dessous

            var fldEngine = getFldEngFromElt(srcElement);
            if (fldEngine != null) {
                //GCH - #35859 - Internationnalisation - on permet l'identification des champs au format date pour les convertir au format de la Base de données
                // #40 182 : Cas particulier de la date : il faut prendre la valeur issue de la base (dd/MM/yyyy) et non la valeur d'affichage,
                // pour être sûr du format utilisé en vue, justement, de convertir la date pour l'affichage.
                // La valeur en base pouvant contenir l'heure en plus de la date, on récupère une partie de la valeur correspondant à la longueur
                // de la valeur d'affichage. Exemple :
                // - valeur d'affichage en MM/DD/YYYY = 08/12/2015 (12 août 2015)
                // - valeur en base : 12/08/2015 00:00:00
                // => valeur retenue : substring de "12/08/2015 00:00:00" de 0 jusqu'à "08/12/2015".length caractères = "12/08/2015"
                // Valeur qui sera ensuite reconvertie au format d'affichage de la base par eDate.ConvertBddToDisplay() via editInnerField() plus bas
                // ALE/CRU/SPH
                // 16/09/2015
                // La date était déjà transformée plus haut. Du coup en fonction du mode de modif (liste, créa, ...) il pouvait encore y avoir double transfo 
                if (bDate) {
                    fldEngine.newValue = newValue;
                    fldEngine.newLabel = eDate.ConvertBddToDisplay(newValue);
                }
                else {
                    fldEngine.newLabel = newLabel;
                    fldEngine.newValue = newValue;
                }

                // GMA 20140325 #27333
                if ((this.type == 'linkCatFileEditor') && (newValue != '') && (newValue != '-1')) {
                    var newClass = srcElement.getAttribute("class").replace('LNKCATFILE', 'LNKGOFILE');
                    newClass = newClass.replace('edit', 'gofile');
                    srcElement.setAttribute("class", newClass);
                    srcElement.setAttribute("eAction", 'LNKGOFILE');
                }

                editInnerField(srcElement, fldEngine);
            }
        }

        // GESTION DES CAS AVEC MAJ ET/OU REGLES/CHAMP LIES/FORMULES

        var nType = 0;

        switch (sCurrentView) {
            case "FILE_CREATION":
                nType = 5;
                break;
            case "FILE_MODIFICATION":
                nType = 3;
                break;
            case "FILE_CONSULTATION":
                nType = 2;
                break;
            default:
                break;
        }

        // HLA - Gestion de l'autobuildname en mode création en popup - Dev #33529
        if (getAttributeValue(srcElement, 'chgedval') == "0" && newValue != oldValue)
            srcElement.setAttribute('chgedval', "1");

        if (this.debugMode) { console.log("eFieldEditor.validate() :\r\n oldValue : " + oldValue + " - newValue : " + newValue); }

        // #53 635 - Cas où l'on indique à la modal parente que le contenu a été modifié, que la MAJ se fasse ou non
        // A compléter ou spécialiser au fur et à mesure des cas détectés
        var bUpdatedValue = newValue != oldValue || (this.catPopupType == '3' && oldLabel != newLabel);
        if (bUpdatedValue)
            this.setParentModalUnsavedChanges(true);

        if ((this.UpdateOnBlur) || this.headerElement.getAttribute('mf') == "1") {

            //Gestion des modes consultation/edition ou des champs avec formule du milieu
            // TOCHECK :  les 2 conditions mènent au même appel JS...
            // Si c'est un champ de liaison parent on force l'update même si valeur identique car il peut y avoir les valeur des autres champs de liaison identiques
            if (newValue != oldValue || bNew || (this.tabLnk && this.tabLnk.indexOf(getNumber(descid)) >= 0)) {

                if (typeof (setConflictInfos) == "function" && getAttributeValue(srcElement, "calconflict") == "1") {
                    top.setWait(true);
                    var jsEditor = this;

                    setConflictInfosInModeList(function () {
                        top.setWait(false);

                        var bConflicted = false;
                        var eltConflictIndicator = document.getElementById("conflictIndicator");
                        if (eltConflictIndicator)
                            bConflicted = eltConflictIndicator.value == "1";
                        else {
                            var attrConflictIndicator = srcElement.getAttribute("conflictIndicator");
                            bConflicted = attrConflictIndicator == "1";
                        }

                        if (bConflicted) {
                            eConfirm(1, '', top._res_6305, '', null, null, function () { top.setWait(false); jsEditor.update(newValue, newLabel, bNew, oldValue, oldLabel); }, function () { top.setWait(false); });
                        }
                        else {
                            jsEditor.update(newValue, newLabel, bNew, oldValue, oldLabel);
                        }
                    }, getTabDescid(getAttributeValue(this.headerElement, "did")), getNumber(descid), this.fileId, newValue, srcElement.id);
                }
                else {
                    cleanOnBlur = true;
                    // SPH+CRU : Gestion des règles + gestion des catalogues liés
                    if (
                        this.headerElement.getAttribute('mf') == "1"
                        &&
                        !this.UpdateOnBlur
                        && getAttributeValue(this.headerElement, "sys") != "1"
                        && (this.headerElement.getAttribute('rul') == "1"
                            || hasChildrenFields(this.tab, descid, this.fileId)
                            || this.headerElement.getAttribute("did") % 100 == 0)
                        && newValue != oldValue) {
                        if (this.tab == 400 && this.fileId == 0) {
                            if (document.getElementById("fileDiv_200") && getAttributeValue(document.getElementById("fileDiv_200"), "did") == "200" && getAttributeValue(document.getElementById("fileDiv_200"), "fid") == "0") {
                                applyRuleOnBlank(200, null, this.fileId, nType, srcElement.id, null, LOADFILEFROM.REFRESH);
                            }
                            else
                                applyRuleOnBlank(this.tab, null, this.fileId, nType, srcElement.id, null, LOADFILEFROM.REFRESH);
                        }
                        else
                            applyRuleOnBlank(this.tab, null, this.fileId, nType, srcElement.id, null, LOADFILEFROM.REFRESH);
                    }


                    this.update(newValue, newLabel, bNew, oldValue, oldLabel);

                }

                if ((this.type == 'linkCatFileEditor') && (newValue != '') && (newValue != '-1')) {
                    var newClass = srcElement.getAttribute("class").replace('LNKCATFILE', 'LNKGOFILE');
                    newClass = newClass.replace('edit', 'gofile');
                    srcElement.setAttribute("class", newClass);
                    srcElement.setAttribute("eAction", 'LNKGOFILE');
                }
            }
            else if (this.catPopupType == '3' && oldLabel != newLabel) {
                // cas de renomage de catalogue avancé
                this.update(newValue, newLabel, bNew, oldValue, oldLabel);
            }
        }
        else if ((!this.UpdateOnBlur) && getAttributeValue(this.headerElement, "sys") != "1"
            && (this.headerElement.getAttribute('rul') == "1" || hasChildrenFields(this.tab, descid, this.fileId) || this.headerElement.getAttribute("did") % 100 == 0)
            && (newValue != oldValue || getAttributeValue(srcElement, "adrprooverwrite") == "1")) {

            // Gestion des règles sans sauvegarde du champ automatique (cas des modes créations et popup dans l'option "enregistrer automatique")
            // + gestion des catalogues liés


            // SPH : #33607
            // Dans le cas de création d'un contact avec une adresse et qu'il y a un reload via ApplyRuleonblank sur un champ d'adress,
            // le reload de l'apply rule on blank se basait sur la table du champ, d'ou le reload qui reload l'adresse sans la société.
            if (this.tab == 400 && this.fileId == 0) {
                if (document.getElementById("fileDiv_200") && getAttributeValue(document.getElementById("fileDiv_200"), "did") == "200" && getAttributeValue(document.getElementById("fileDiv_200"), "fid") == "0") {
                    applyRuleOnBlank(200, null, this.fileId, nType, srcElement.id, null, LOADFILEFROM.REFRESH);
                }
                else
                    applyRuleOnBlank(this.tab, null, this.fileId, nType, srcElement.id, null, LOADFILEFROM.REFRESH);
            }
            else
                applyRuleOnBlank(this.tab, null, this.fileId, nType, srcElement.id, null, LOADFILEFROM.REFRESH);
        }
        else if (getAttributeValue(srcElement, "calconflict") == "1") {
            //Gestion des conflits            
            applyRuleOnBlank(this.tab, null, this.fileId, nType, srcElement.id, null, LOADFILEFROM.REFRESH);
        }

        // Ajout dans les dernières valeurs saisies
        if (!bFromLastValue
            && newValue != oldValue
            && getAttributeValue(this.headerElement, "cclval") == "1")
            LastValuesManager.addValue(this.sourceElement.id, this.tab, descid, getAttributeValue(this.headerElement, "lib"), oldValue, oldLabel);

        this.filter = '';
        if (this.parentPopup && this.parentPopup.hide)
            this.parentPopup.hide();

        cleanOnBlur = cleanOnBlur && srcElement != null && typeof (srcElement) != 'undefined';

        if (cleanOnBlur) {
            //this.sourceElement = null;
            srcElement.onblur = function () { };
        }
    };

    // Fonction à appeler lorsqu'on annule la saisie dans le champ (ex : touche Echap)
    // On remet alors le filtre de recherche à zéro
    this.cancel = function () {
        this.filter = '';
        var srcElt = this.GetSourceElement();
        if (srcElt && this.sourceElement && this.sourceElement.id == srcElt.id) {
            this.sourceElement = null;
        }
        if (this.parentPopup) {
            this.parentPopup.sourceElement = null;
            this.parentPopup.hide();
        }

        this.resetOrDestroy();
    }

    // Déclenche la mise à jour en base de la valeur du champ en faisant appel à eUpdater/eEngine
    this.update = function (newValue, newLabel, bNew, oldValue, oldLabel) {
        // Pas de mise à jour en base si l'on a pas encore récupéré la valeur actuelle du champ Mémo via un appel AJAX
        // Evite que la valeur du champ soit vidée lorsque, par exemple, on double-clique sur un champ Mémo (le deuxième clic provoque un deuxième appel, qui valide le premier, avec une valeur qui n'a pas forcément pu être récupérée à temps)
        if (this.waitingForValue)
            return;

        var nodeSrcElement = this.GetSourceElement();
        if (!nodeSrcElement)
            return;

        var eEngineUpdater = new eEngine();
        eEngineUpdater.Init();


        if (this.ForceRefresh) {
            eEngineUpdater.AddOrSetParam('refresh', '1');
            this.ForceRefresh = false;
        }



        if (isPopup() && (getCurrentView(document) == "FILE_MODIFICATION" || getCurrentView(document) == "FILE_CREATION")) {
            var bPlan = getAttributeValue(document.getElementById("fileDiv_" + this.tab), "ecal") == "1";

            //le top.eModFile n'est pas renseigné lorsque la popup a été ouverte depuis une popup de recherche
            // eModFile est en effet renseigné lors de l'ouverture de la fiche en modal dialog et dans ce cas, la variable est attachée à la fenêtre de recherche
            // et plus à la fenêtre principale (top)

            var myModFile = eTools.getModalFromField(this);
            if (myModFile == null) {
                if (top.eModFile && top.eModFile.isModalDialog)
                    myModFile = top.eModFile;
                else
                    myModFile = top.window['_md']["popupFile"];
            }

            eEngineUpdater.ModalDialog = { oModFile: myModFile, modFile: myModFile.getIframe(), pupClose: false, bPlanning: bPlan, docTop: top };

            var parentTab = getTabFrom(myModFile.myOpenerWin);
            eEngineUpdater.AddOrSetParam('parenttab', parentTab);
        }

        var afterValidate = null;
        if (this.type != 'eCheckBox' && this.type != 'eBitButton')
            afterValidate = (
                function (oSrcElementP) {
                    return function (params) {

                        var activeElt = document.activeElement;

                        if ((activeElt) && (oSrcElementP != activeElt) && activeElt.className != "stepValue") {

                            // #43537 CRU : Si l'élément suivant est une checkbox, on ne clique pas pour ne pas la cocher. On garde le clic pour le reste (notamment pour les catalogues)
                            if (activeElt.tagName.toLowerCase() == 'a' && activeElt.getAttribute("chk")) {
                                activeElt.focus();
                            }
                            else {
                                activeElt.click();
                            }

                        }
                    }
                }
            )(nodeSrcElement);


        if (getAttributeValue(nodeSrcElement, "calconflict") == "1")
            afterValidate = (
                function (oSrcElementP) {
                    return function (params) {

                        var params = Object.assign({ tab: 0, isMiddleFormula: false }, params);
                        // pas de refresh complet si appel depuis formule milieu
                        if (params.isMiddleFormula)
                            return;

                        if (typeof (isBkmFile) == 'function' && !isBkmFile(params.tab)) {
                            RefreshFile(window);
                            //top.loadList();
                        }
                    }
                }
            )(nodeSrcElement);

        if (typeof (afterValidate) == 'function')
            eEngineUpdater.SuccessCallbackFunction = afterValidate;

        eEngineUpdater.AddOrSetParam('fldEditorType', this.type);
        eEngineUpdater.AddOrSetParam('catNewVal', bNew);
        eEngineUpdater.AddOrSetParam('jsEditorVarName', this.jsVarName);

        // On indique la table à l'engine car le premier Field peut ne pas correspondre à la table de la fiche en cours de création
        eEngineUpdater.AddOrSetParam('tab', this.tab);

        // Rubrique liaisons direct en-tête ou pied de page
        if (getAttributeValue(this.headerElement, "prt") == "1") {
            eEngineUpdater.AddOrSetParam('fileId', this.fileId);

            //MODE LISTE le did est à 1 vu que champ de liaison on doit mettre à 0
            //			est le fileid n'est pas dans filediv
            if (this.fileId <= 0 && (this.tab != descid)) {
                this.fileId = GetMasterFileId(nodeSrcElement.id);
            }
        }
        else {
            this.fileId = GetFieldFileId(nodeSrcElement.id);
            eEngineUpdater.AddOrSetParam('fileId', this.fileId);
        }

        // HLA - le fileid est à 0 dans le cas d'une création de fiche
        if (eEngineUpdater.GetParam('fileId') == '')
            return;

        // HLA - On averti qu'on est en sorti de champs - Dev #45363
        eEngineUpdater.AddOrSetParam('onBlurAction', '1');


        var descid = getAttributeValue(this.headerElement, 'did');
        var tab = getTabDescid(descid);

        if (!this.UpdateOnBlur) {
            // En mode création on check seulement les formules du milieu, engAction = 4
            eEngineUpdater.AddOrSetParam('engAction', 4);


            // Indique à l'engine sur quelle rubrique la vérification de la formule du milieu doit s'executer
            eEngineUpdater.AddOrSetParam('fieldTrigger', descid);

            var aFld = getFieldsInfos(tab, this.fileId);
            for (var i = 0; i < aFld.length; i++) {
                var fld = aFld[i];
                if (fld.descId == getAttributeValue(this.headerElement, 'did') && nodeSrcElement.tagName == 'INPUT') {
                    fld.cellId = nodeSrcElement.id;
                    fld.oldValue = oldValue;
                    fld.oldLabel = oldLabel;
                    fld.prevValue = oldValue;
                }

                eEngineUpdater.AddOrSetField(fld);
            }
        }
        else {

            var bPrt = false;

            // SPH #36195 : ajout de la condition sur adresse perso.
            // dans ce cas, le champ n'est pas un lien vers la pm mais un catalogue saisie libre
            if (getAttributeValue(this.headerElement, 'prt') == "1" && !(getAttributeValue(nodeSrcElement, 'adrperso') == "1")) {
                bPrt = true;
                descid = tab;
            }

            // Indique à l'engine sur quelle rubrique la vérification de la formule du milieu doit s'executer
            eEngineUpdater.AddOrSetParam('fieldTrigger', descid);

            var fld = new fldUpdEngine(descid);
            fld.newValue = newValue;
            fld.newLabel = newLabel;

            //Dans le cas des case à cocher, le source élément n'est pas un input
            if (nodeSrcElement.tagName == 'INPUT'
                || getAttributeValue(nodeSrcElement, "eaction") == "LNKCHECK"
                || getAttributeValue(nodeSrcElement, "eaction") == "LNKSTEPCAT"
            ) {
                fld.cellId = nodeSrcElement.id;
                fld.oldValue = oldValue;
                fld.oldLabel = oldLabel;
            }

            fld.prevValue = oldValue;
            fld.multiple = getAttributeValue(this.headerElement, 'mult') == "1";
            fld.popId = getAttributeValue(this.headerElement, 'popid');
            fld.popupType = getAttributeValue(this.headerElement, 'pop');

            fld.boundDescId = getAttributeValue(this.headerElement, 'bndId');
            fld.boundPopup = getAttributeValue(this.headerElement, 'bndPop');
            fld.boundValue = getAttributeValue(nodeSrcElement, 'pdbv');
            eEngineUpdater.AddOrSetField(fld);
        }

        /*********************************************************************/
        //        On propose de créer une adresse si celle ci n'existe pas
        /*********************************************************************/
        if (bPrt && (descid == 200 || descid == 300) && this.tab != 400 && this.tab != 200) {
            var elt200 = GetField(this.tab, 201);
            var elt300 = GetField(this.tab, 301);
            var elt400 = GetField(this.tab, 401);

            if (elt200 && elt300) {
                var efldeng200 = getFldEngFromElt(elt200);
                var efldeng300 = getFldEngFromElt(elt300);
                if (descid == 200) {
                    efldeng200 = fld;
                    eEngineUpdater.AddOrSetField(efldeng300);
                }
                else if (descid == 300) {
                    efldeng300 = fld;
                }
                if (efldeng200 != null && efldeng300 != null && efldeng200.newValue != "" && efldeng300.newValue != "")
                    eEngineUpdater.SuccessCallbackFunction = function () {
                        afterValidate();

                        chkAdr0(efldeng200, efldeng300);
                    };
            }
            else if (elt200 && elt400) {

                var efldeng200 = getFldEngFromElt(elt200);
                var efldeng400 = getFldEngFromElt(elt400);

                if (descid == 200) {
                    efldeng200 = fld;
                    eEngineUpdater.AddOrSetField(efldeng400);
                }
                else if (descid == 400) {
                    efldeng400 = fld;
                }

                if (efldeng200 != null && efldeng400 != null && efldeng200.newValue != "" && efldeng400.newValue != "")
                    eEngineUpdater.SuccessCallbackFunction = function () {
                        afterValidate();
                    };
            }
        }
        /*********************************************************************/

        /*********************************************************************/
        //        A la modif d'un champ de liaison
        //         Si les autres liaisons on été reprises du champ de liaison
        //         On les enregistre en bdd
        //         (exemple si selection de PP => recopie de sa société/adresse)
        /*********************************************************************/
        if (bPrt && (this.tabLnk != null && this.tabLnk.indexOf(descid) >= 0)) {
            for (var cpt = 0; cpt < this.tabLnk.length; cpt++) {
                if (this.tabLnk[cpt] != descid) {
                    var currentDescId = this.tabLnk[cpt] + 1;
                    var oPrtFld = GetField(this.tab, currentDescId);
                    if (!oPrtFld)
                        continue;
                    var oPrtHead = document.getElementById("COL_" + this.tab + "_" + currentDescId);
                    if (!oPrtHead)
                        continue;
                    var prtOldLabel = oPrtFld.getAttribute("oldvalue");
                    var prtOldDbv = oPrtFld.getAttribute("olddbv");
                    if (prtOldDbv == null || prtOldLabel == null)
                        continue;
                    var prtNewLabel = oPrtFld.value;
                    var prtNewDbv = getAttributeValue(oPrtFld, "dbv");
                    if (prtOldLabel == prtNewLabel && prtOldDbv == prtNewDbv)
                        continue;
                    var fldPrt = new fldUpdEngine(this.tabLnk[cpt]);
                    fldPrt.cellId = oPrtFld.id;
                    fldPrt.newValue = prtNewDbv;
                    fldPrt.newLabel = prtNewLabel;
                    fldPrt.oldValue = prtOldDbv;
                    fldPrt.oldLabel = prtOldLabel;

                    fldPrt.multiple = getAttributeValue(oPrtHead, 'mult') == "1";
                    fldPrt.popId = getAttributeValue(oPrtHead, 'popid');
                    fldPrt.popupType = getAttributeValue(oPrtHead, 'pop');

                    fldPrt.boundDescId = getAttributeValue(oPrtHead, 'bndId');
                    fldPrt.boundPopup = getAttributeValue(oPrtHead, 'bndPop');
                    eEngineUpdater.AddOrSetField(fldPrt);

                }
            }
        }

        /*********************************************************************/
        eEngineUpdater.UpdateLaunch();



        //if (this.parentPopup && this.parentPopup.hide)
        //    this.parentPopup.hide();

    };

    //GCH #19497 : Variable pour forcer les champs qui ont été vidé à &nbsp; pour que le flagAsEdited (liseré) soit correctement affiché
    this.bEmptyDisplay = false;
    // Applique un liséré de couleur autour du champ de saisie pour indiquer si la mise à jour a bien été prise en compte (ou non)
    this.flagAsEdited = function (flagAsEdited, noEdit, srcEltId) {
        if (typeof (srcEltId) == 'undefined' || !srcEltId || srcEltId == '')
            var srcElt = this.GetSourceElement();
        else
            var srcElt = document.getElementById(srcEltId);

        if (typeof (srcElt) == 'undefined' || !srcElt)
            return;

        if (typeof (noEdit) == 'undefined' || !noEdit || noEdit == '')
            var editedClass = "eFieldEditorEdited";
        else
            var editedClass = "eFieldEditorNoEdited";

        // Etape 1 : On fait apparaître le liseré
        if (flagAsEdited) {
            if (srcElt.tagName.toUpperCase() == 'INPUT') {
                addClass(srcElt, editedClass);
            }
            else if (srcElt.getAttribute('eaction') == 'LNKOPENMEMO' && srcElt.getAttribute('html') == '1' && document.getElementById(srcEltId + 'ifr')) {
                var flagEditIFrame = document.getElementById(srcEltId + 'ifr');
                flagEditIFrame.setAttribute('class', editedClass);
            }
            else {
                var flagEditDiv = srcElt.ownerDocument.createElement('div');
                flagEditDiv.setAttribute('id', 'tempFlagEdit');
                flagEditDiv.setAttribute('class', editedClass);

                // HLA - Ajout du conteneur du flag
                var spanData = flagEditDiv.ownerDocument.createElement('span');
                flagEditDiv.appendChild(spanData);

                // HLA - Ajout de l'image GHOST du flag
                var imgGhost = flagEditDiv.ownerDocument.createElement('img');
                imgGhost.setAttribute('src', 'ghost.gif');
                flagEditDiv.appendChild(imgGhost);

                spanData.innerHTML = eTrim(srcElt.innerHTML);
                //#19497 : Si le champ a été vidé, on force l'affichage d'un espace pour que le liseré ne soit pas applatit
                if (spanData.innerHTML == "") {
                    this.bEmptyDisplay = true;
                    spanData.innerHTML = "&nbsp;";
                }

                srcElt.innerHTML = "";
                srcElt.appendChild(flagEditDiv);
            }

            // Utilisation d'un timer pour faire disparaître l'effet au bout de X secondes
            window.setTimeout(function () { if (that) that.flagAsEdited(false, noEdit, srcElt.id); }, 500);


        }
        // Etape 2 : on fait disparaître le liseré après édition
        else {
            //SPH : TODO : vérifier la logique du test.
            // On vide la variable si l'utilisateur est reste sur la même rubrique d'édition
            if (srcElt && this.sourceElement && this.sourceElement.id == srcElt.id) {
                this.sourceElement = null;
            }
            if (this.parentPopup)
                this.parentPopup.sourceElement = null;

            if (srcElt.tagName.toUpperCase() == 'INPUT') {
                removeClass(srcElt, editedClass);
            }
            else if (srcElt.getAttribute('eaction') == 'LNKOPENMEMO' && srcElt.getAttribute('html') == '1' && document.getElementById(srcEltId + 'ifr')) {
                var flagEditIFrame = document.getElementById(srcEltId + 'ifr');
                flagEditIFrame.setAttribute('class', 'eME');
            }
            else {
                if (srcElt.firstChild && srcElt.firstChild.tagName == 'DIV' && srcElt.firstChild.id == 'tempFlagEdit') {
                    srcElt.innerHTML = eTrim(srcElt.firstChild.firstChild.innerHTML);
                    //#19497 : Si le champ a été vidé et que l'affichage d'un espace a été forcé pour que le liseré ne soit pas applatit, on enlève l'espace
                    if (this.bEmptyDisplay && srcElt.innerHTML == "&nbsp;") {
                        this.bEmptyDisplay = false;
                        srcElt.innerHTML = "";
                    }
                }
            }

            this.resetOrDestroy();
        }
    };

    // Affiche une popup de saisie supplémentaire pour les types de champs avancés (ex : catalogues)
    this.openAdvancedDialog = function () {

        var catDescId = "";
        var catMultiple = "0";
        var catBoundDescId = "";
        var catUrl = "";
        var catWidth = 800;
        var catHeight = 550;

        top.setWait(true);

        if (this.multiple) {
            catMultiple = "1";
            catWidth = 850;
            catHeight = 530;
        }
        if (this.treeview) {
            catWidth = 850;
            catHeight = 614;
        }

        var catSelectedValues = this.selectedValues.join(";");


        if (this.headerElement) {
            catDescId = this.headerElement.getAttribute('popid');
            this.catPopupType = this.headerElement.getAttribute('pop');
            catBoundDescId = this.headerElement.getAttribute('bndId');
            catBoundPopup = this.headerElement.getAttribute('bndPop');
        }

        var catParentValue = this.GetSourceElement().getAttribute('pdbv');

        this.parentPopup.hide();

        top.setWait(false);

        if (this.advancedDialog != null) {
            try {
                this.advancedDialog.hide();
                if (!(this.advancedDialog.bScriptOk && this.advancedDialog.bBodyOk))
                    top.setWait(false);

            }
            catch (e) {
                //                debugger;
            }
        }

        /*    p_bMulti, p_btreeView, p_defValue, p_sourceFldId
        , p_targetFldId, p_catDescId, p_catPopupType, p_catBoundDescId
        , p_catBoundPopup, p_catParentValue, p_CatTitle,p_JsVarName
        , p_bMailTemplate, p_partOfAfterValidate
        */

        var fctValidate = function (catalogDialog, srcId, trgId, tabSelectedLabels, tabSelectedValues, selectedIDs) {

            if (srcId.indexOf("117022") > 0) {
                setAttributeValue(document.querySelector("[ename='COL_117000_117023']"), "data-desctids", selectedIDs);
            }

            partOfValidateCatDlg_FldEdt(catalogDialog, srcId, trgId, tabSelectedLabels, tabSelectedValues, selectedIDs);
        }

        this.advancedDialog = showCatGeneric(this.multiple, this.treeview, catSelectedValues, this.GetSourceElement().id
            , null, catDescId, this.catPopupType, catBoundDescId
            , catBoundPopup, catParentValue, encode(this.catalogTitle), this.jsVarName
            , false, fctValidate, partOfCancelCatDlg_FldEdt
        );

    }


    ///Ouvre une popup de choix de date pour maj de champ
    this.openAdvancedCalendar = function () {
        // on transmet via nodeId le nom de l'objet eFieldEditor utilisé pour effectuer la modification de la date
        var oNode = this.GetSourceElement();
        if (!oNode)
            return;
        var nodeId = oNode.id;

        // Valeur d'origine de la rubrique en cours de modification
        var date = '';
        if (oNode.tagName == 'INPUT')
            date = oNode.value;
        else if (oNode.innerHTML != null)
            date = oNode.innerHTML;
        else
            date = oNode.textContent;

        if (this.advancedDialog != null) {
            try {
                this.advancedDialog.hide();
            }
            catch (e) {
                debugger;
            }
        }

        var iframeId = "";

        var widgetIdInput = document.getElementById("hidWid");
        if (widgetIdInput) {
            // Si on est dans un widget
            iframeId = "widgetIframe_" + widgetIdInput.value;
        }

        //GCH - #36019 - Internationnalisation - Choix de dates - REFACTORISATION
        this.advancedDialog = createCalendarPopUp("updDateFromCalendar", 0, 0, top._res_5017, top._res_5003, "onCalendarEditorOk", top._res_29, null, null, iframeId, this.jsVarName, date);
    }



    //Finder permet de forcer l'affichage du nom seul en attribut de la cellule
    this.AddNameOnly = false;
    this.oModalLnkFile = null;  //objet ou est stocké la modale du champ de liaison
    this.tabLnk = null;
    /*
    Méthode permettant l'ouverture de la modal du champ de liaison
    nSearch : type de champ de recherche demandé (nouveau, recherche avancé ou champ de liaison), voir FinderSearchType
    targetTab : est redéfinit si l'on vient de l'exterieur de efieldeditor
    bBkm : Vrai si ajout depuis un signet
    oSrc : objet appelant
    onOkCustom : methode appelé à la validation
    paramSup : ajout de addparam specifique AllRecord#$#1#|#...#$#...
    sSearchValue : Permet de prédéfinir la recherche avec la valeur inscrite
    nCallFrom : enum (voir eMain.js) pour indiquer d'où on vient
    bNoLoadFileAfterValid : permet de ne pas rediriger vers la fiche à la validation
    */
    this.openLnkFileDialog = function (nSearchType, targetTab, bBkm, onCustomOk, paramSup, sSearchValue, nCallFrom, bNoLoadFileAfterValid) {
        top.setWait(true);

        //#33286
        if (document.getElementById("eCatalogEditorSearch")) {
            sSearchValue = document.getElementById("eCatalogEditorSearch").value;
        }
        if (typeof sSearchValue == "undefined")
            sSearchValue = "";

        this.fileId = 0;
        var descId = targetTab;
        if (this.headerElement && !descId) {
            descId = getAttributeValue(this.headerElement, 'did');
            targetTab = getAttributeValue(this.headerElement, 'popId');
            targetTab = getTabDescid(targetTab);
            if ((targetTab <= 0) && (descId > 0))
                targetTab = getTabDescid(descId);
            this.fileId = GetMasterFileId(this.GetSourceElement().id);
        }

        /*Uservalue*/
        //Récup des info des champs affichés actuellement sur la fiche en cours

        var nFieldTab;
        nFieldTab = nGlobalActiveTab;

        if (this.headerElement)
            nFieldTab = GetMainTableDescId(this.headerElement.id);

        var aUvFldValue = getFieldsInfos(nFieldTab, this.fileId);
        /*Fin Uservalue*/

        var nMode = "1";    //Chercher
        var strTitle = top._res_10; //Chercher
        if (nSearchType == 1) {
            strTitle = top._res_18; //Ajouter
            if (bBkm)
                var nMode = "3";    //Ajouté depuis bkm
        }
        else if (nSearchType == 2 || nSearchType == 3 || nSearchType == 4) {
            strTitle = top._res_73; //Associer
            nMode = "0";
        }
        else if (nSearchType == 6) {
            strTitle = top._res_8747; // Sélectionner
            nMode = "4";
        }
        else if (nSearchType == 5) {
            //-------------- demande 36826 : MCR/RMA :  Pour gérer les appels entrants des CTIs, barre de titre modale        
            if (top.nModalLnkFileLoaded > 0) // si des modales sont deja ouvertes alors affichage message alert !!
            {
                setWait(false);
                // MCR 39400 : ajout de libelles dans res
                eAlert(0, top._res_6771, top._res_6761, '<br>');
                return;
            }


            strTitle = strTitle + " : " + sSearchValue;

        }


        var oTabWH = getWindowWH(top);
        var maxWidth = 1000; //Taille max à l'écran (largeur)
        var maxHeight = 550; //Taille max à l'écran (hauteur)
        var width = oTabWH[0];
        var height = oTabWH[1];

        if (width > maxWidth)   //si largeur de l'écran plus grande que largeur max alors il faut utiliser la largeur max
            width = maxWidth;
        else
            width = width - 10;   //marge de "sécurité"

        if (height > maxHeight)   //si hauteur de l'écran plus grande que largeur max alors il faut utiliser la largeur max
            height = maxHeight;
        else
            height = height - 10;   //marge de "sécurité"

        if (this.parentPopup)
            this.parentPopup.hide();

        if (this.oModalLnkFile != null) {
            try {
                this.oModalLnkFile.hide();
                if (!(this.oModalLnkFile.bScriptOk && this.oModalLnkFile.bBodyOk))
                    top.setWait(false);

            }
            catch (e) {
                debugger;
            }
        }
        this.oModalLnkFile = new eModalDialog(strTitle, 0, 'mgr/eFinderManager.ashx', width, height, "modalFinder");
        this.oModalLnkFile.NoGlobalCLose = true;
        this.oModalLnkFile.addScript("eTools");
        this.oModalLnkFile.addScript("eUpdater");
        this.oModalLnkFile.addScript("eGrapesJSEditor");
        this.oModalLnkFile.addScript("eMemoEditor");
        this.oModalLnkFile.addScript("eExpressFilter");
        this.oModalLnkFile.addScript("eContextMenu");
        this.oModalLnkFile.addScript("eModalDialog");
        this.oModalLnkFile.addScript("eEngine");
        this.oModalLnkFile.addScript("eMain");
        this.oModalLnkFile.addScript("eList");
        this.oModalLnkFile.addScript("eFinder");
        this.oModalLnkFile.addScript("ePopup");
        this.oModalLnkFile.addCss("eMain");
        this.oModalLnkFile.addCss("eIcon");
        this.oModalLnkFile.addCss("eControl");
        this.oModalLnkFile.addCss("eTitle");
        this.oModalLnkFile.addCss("eContextMenu");
        this.oModalLnkFile.addCss("eModalDialog");
        this.oModalLnkFile.addCss("eMemoEditor");
        this.oModalLnkFile.addCss("eList");
        if (this.multiple)
            this.oModalLnkFile.addCss("eActionList");
        this.oModalLnkFile.addCss("eFinder");
        this.oModalLnkFile.addCss("eudoFont");
        this.oModalLnkFile.addCss("theme");

        /** ICI, si on est sur le nouveau thème Eudonet X, on charge un fichier CSS supplémentaire
          * qui sert de base aux CSS des thèmes Eudonet X. G.L */
        var paramWin = top.getParamWindow();
        var objThm = JSON.parse(paramWin.GetParam("currenttheme").toString());

        if (objThm.Version > 1)
            this.oModalLnkFile.addCss("../../Theme2019/css/theme");


        var myBrowser = null;
        if (getBrowser)
            myBrowser = new getBrowser();
        if (myBrowser != null && myBrowser.isIE && myBrowser.version == 8) {
            this.oModalLnkFile.addCss("ie8-styles");
        }


        var myFunct = (function (obj) {
            return function () {
                obj.hide();
                top.setWait(false);
            }
        })(this.oModalLnkFile);

        this.oModalLnkFile.ErrorCallBack = myFunct;

        //On ajoute le finder à la liste des finder OUVERT
        top.eTabLinkCatFileEditorObject.Add(this.oModalLnkFile.iframeId, this);

        /*Début - Uservalue - Envoi des informations de d'uservalue à la modale*/
        var uvflstDetail = "";
        //Liste des séparateurs utilisé pour construire des liste, des listes de tableaux...
        var SEPARATOR_LVL1 = "#|#";
        var SEPARATOR_LVL2 = "#$#";

        //ne pas transférer les champs note pour les uservalue
        var oLstMemoField = document.getElementById("memoIds_" + nFieldTab)
        var aLstMemoField = new Array();
        if (oLstMemoField != null && oLstMemoField.value)
            aLstMemoField = oLstMemoField.value.split(";");

        var bCheckMemo = (Array.prototype.indexOf && typeof (Array.prototype.indexOf) == "function" && aLstMemoField.length > 0);

        //Nom du param = (IsFound$|$Parameter$|$Value$|$Label)

        for (var i = 0; i < aUvFldValue.length; i++) {
            if (!aUvFldValue[i])
                continue;

            //ne pas transférer les champs note pour les uservalue
            if (bCheckMemo && aLstMemoField.indexOf(aUvFldValue[i].cellId) != -1)
                continue;

            if (uvflstDetail != "")
                uvflstDetail = uvflstDetail + SEPARATOR_LVL1;
            try {
                uvflstDetail = uvflstDetail + "1" + SEPARATOR_LVL2 + aUvFldValue[i].descId + SEPARATOR_LVL2 + aUvFldValue[i].newValue + SEPARATOR_LVL2 + aUvFldValue[i].newLabel;
            }
            catch (e) {
                continue;
            }
        }
        try {
            //Ecran de duplication en masse.
            if (aUvFldValue.length == 0 && this.sourceElement) {
                var divDupli = this.sourceElement.ownerDocument.getElementById("dupliTreatSelectFields");
                if (divDupli) {
                    var oTrs = divDupli.querySelectorAll("TR");
                    for (var i = 0; i < oTrs.length; i++) {
                        var tr = oTrs[i];
                        var cb = tr.querySelector("a.chk[id^='chk_chkDup'][chk='1']");
                        if (!cb) {
                            continue;
                        }
                        var edndescid = getNumber(getAttributeValue(cb.parentElement, "edndescid"));
                        if (getTabDescid(edndescid) != getNumber(getAttributeValue(divDupli, "tab")))
                            edndescid = getTabDescid(edndescid);
                        var input = tr.querySelector("INPUT[ednvalue]");
                        var newValue = getAttributeValue(input, "ednvalue");
                        var newLabel = input.value;
                        if (uvflstDetail != "")
                            uvflstDetail = uvflstDetail + SEPARATOR_LVL1;
                        try {
                            uvflstDetail = uvflstDetail + "1" + SEPARATOR_LVL2 + edndescid + SEPARATOR_LVL2 + newValue + SEPARATOR_LVL2 + newLabel;
                        }
                        catch (e) {
                            continue;
                        }

                    }
                }
            }
        }
        catch (ex) {
        }

        // #47175 : On ajoute la liaison haute si elle existe
        if (top.nGlobalActiveTab) {
            if (typeof (top.GetCurrentFileId) !== 'undefined') {
                if (top.GetCurrentFileId(top.nGlobalActiveTab) != "") {
                    if (uvflstDetail != "")
                        uvflstDetail = uvflstDetail + SEPARATOR_LVL1;
                    uvflstDetail = uvflstDetail + "1" + SEPARATOR_LVL2 + top.nGlobalActiveTab + SEPARATOR_LVL2 + top.GetCurrentFileId(top.nGlobalActiveTab) + SEPARATOR_LVL2 + "";
                }
            }

        }

        //Liste des champs concerné par le uservalue
        this.oModalLnkFile.addParam('UserValueFieldList', encode(uvflstDetail), "post");
        /*Fin - Uservalue - Envoi des informations de d'uservalue à la modale*/

        if (paramSup && paramSup != "") {
            var tabParamSup = paramSup.split(SEPARATOR_LVL1);
            for (cpt = 0; cpt < tabParamSup.length; cpt++) {
                var currentValues = tabParamSup[cpt];
                var tabCurrentParamSup = currentValues.split(SEPARATOR_LVL2);
                if (tabCurrentParamSup[0] != "" && tabCurrentParamSup.length > 1) {
                    this.oModalLnkFile.addParam(tabCurrentParamSup[0], tabCurrentParamSup[1], "post");
                }
            }
        }


        //on rajoute ici un objet contenant les valeurs à insérer dans une nouvelle fiche.
        var assDescId = descId.toString() == "400" ? "200" : descId.toString()
        var arrAssFields = document.querySelectorAll("[assfld^='[" + assDescId + "]_']");

        var defValues = new Object();
        for (var i = 0; i < arrAssFields.length; i++) {
            var assFldElt = arrAssFields[i];
            try {
                var assFldDid = getAttributeValue(assFldElt, "assfld").split('_')[1].replace("[", "").replace("]", "");
            }
            catch (e) {
                continue;
            }

            //Formaté pour être desérialisé en Dictionaire C# dans eFileDisplayer.
            var value = getAttributeValue(assFldElt, "dbv");
            if (getAttributeValue(assFldElt, "eaction") == "LNKCHECK") {
                value = assFldElt.querySelector("a[chk]").getAttribute("chk");
            }
            defValues[assFldDid] = value ? value : assFldElt.value;

            if (assFldDid == getNumber(descId) + 1 && sSearchValue == "") {
                sSearchValue = defValues[assFldDid];
            }
        }

        this.oModalLnkFile.addParam("defvalues", JSON.stringify(defValues), "post");

        //Table sur laquelle on recherche
        this.oModalLnkFile.addParam("targetTab", targetTab, "post");
        //id de la fiche de départ
        this.oModalLnkFile.addParam("FileId", this.fileId, "post");
        //Champ catalogue sur la fiche de départ
        this.oModalLnkFile.addParam("targetfield", descId, "post");

        //Table de départ
        if (this.headerElement)
            this.oModalLnkFile.addParam("tabfrom", GetMainTableDescId(this.headerElement.id), "post");
        else
            this.oModalLnkFile.addParam("tabfrom", nGlobalActiveTab, "post");
        //Table de départ
        this.oModalLnkFile.addParam("NameOnly", this.AddNameOnly ? "1" : "0", "post");

        // valeur recherchée 		#33286
        this.oModalLnkFile.addParam("Search", sSearchValue, "post");


        //-------------- demande 36826 : MCR/RMA :  Pour gérer les appels entrants des CTIs
        if (nSearchType == 5) {
            this.oModalLnkFile.addParam("action", "cti", "post");   // CAS du 'CTI'
            this.oModalLnkFile.addParam("pn", sSearchValue, "post");   // ajout du phone number : pn comme parametre du post
        }
        else
            this.oModalLnkFile.addParam("action", "dialog", "post");

        //Si a 1 chaque ligne est cliquable et permet de rediriger vers la fiche correspondant à la ligne sélectionnée
        this.oModalLnkFile.addParam("eMode", nMode, "post");
        //Type de recherche demandée
        this.oModalLnkFile.addParam("SearchType", nSearchType, "post");

        // si le champ se trouve dans une popup, on transmet l'id de la frame
        if (this.sourceElement && this.sourceElement.ownerDocument != top.document) {

            for (var i = 0; i < top.frames.length; i++) {
                // kha le 27 juin 2016 je rajoute un try catch car dans le cas où  une iframe contient une page exterieure 
                // cela provoque une erreur cross-origin
                try {
                    if (top.frames[i].document == this.sourceElement.ownerDocument) {
                        this.oModalLnkFile.addParam("origframeid", top.frames[i]._parentIframeId, "post");
                        break;
                    }
                }
                catch (exc) {
                }
            }
        }

        //Récup des MRU si vide (cas de la recherche étendue et de nouveau)
        if (nMode == "1") {
            var oeParam = getParamWindow();
            if (oeParam.GetMruParam)
                this.mruParamValue = oeParam.GetMruParam(descId);
        }
        //MRU :
        this.oModalLnkFile.addParam("MRU", encode(this.mruParamValue), "post");

        this.oModalLnkFile.addParam("callfrom", nCallFrom, "post");
        this.oModalLnkFile.addParam("noloadfile", bNoLoadFileAfterValid ? "1" : "0", "post");

        this.oModalLnkFile.onIframeLoadComplete = (function (iframeId, nTab) { return function () { onLnkFileLoad(iframeId, nTab); } })(this.oModalLnkFile.iframeId, targetTab);

        // demande 36826 : MCR :  Pour gérer les appels entrants des CTIs
        top.nModalLnkFileLoaded = top.nModalLnkFileLoaded + 1;

        this.oModalLnkFile.show();

        this.tabLnk = getTabFileLnkId(nGlobalActiveTab);
        if (typeof (onCustomOk) == "undefined" || onCustomOk == "")
            onCustomOk = validateLnkFile;

        if ((nMode == "1") || (nMode == "3")) {
            //Recherche ou ajouter pas de sélection de valeurs donc pas de bouton valider
            this.oModalLnkFile.addButton(top._res_30, cancelLnkFile, 'button-gray', this.oModalLnkFile.iframeId, "cancel");   //Fermer
        }
        else {
            //Champ de liaison
            this.oModalLnkFile.addButton(top._res_29, cancelLnkFile, 'button-gray', this.oModalLnkFile.iframeId, "cancel");   //Annuler
            this.oModalLnkFile.addButton(top._res_28, onCustomOk, "button-green", this.oModalLnkFile.iframeId, "ok"); // Valider

            // Si catalogue champ de liaison et valeurs déjà sélectionnée on affiche Dissocier
            if (this.type == 'linkCatFileEditor' && this.selectedLabels.length > 0 && this.selectedLabels[0] != "")
                this.oModalLnkFile.addButton(top._res_6333,
                    (function (frmId, okFct) { return function () { dissociateLnkFile(frmId, okFct); } })(this.oModalLnkFile.iframeId, onCustomOk),
                    "button-red", null, "dissociate"); // Dissocier
        }

        return this.oModalLnkFile;
    };

    // Affiche la popup de catalogue utilisateur
    this.OpenUserDialog = function (obj, customOk, customAbort) {

        top.setWait(true);

        this.selectedValues = new Array();
        this.selectedLabels = new Array();
        this.currentValues = new Array();
        this.currentLabels = new Array();
        //S'il n'est pas précisé c'est qu'il est déjà renseigné
        if (typeof (obj) != "undefined" && obj != null) {
            this.parentPopup.sourceElement = obj; // objet actuellement édité (auquel est rattaché l'éditeur et sa popup)
            this.sourceElement = obj;
            this.headerElement = obj.ownerDocument.getElementById(obj.getAttribute("ename"));
            if (this.headerElement)
                this.headerElementId = this.headerElement.id;
        }

        if (typeof (this.sourceElement) == "undefined" || this.sourceElement == null) {
            return;
        }


        var descId = "0";
        if (this.headerElement) {
            // le descid du champ
            descId = this.headerElement.getAttribute("did");

            // le libelle du champ
            this.catalogTitle = this.headerElement.getAttribute("lib");
        }

        var fullUserList = this.sourceElement.getAttribute("fulluserlist");
        var showEmptyGroup = this.sourceElement.getAttribute("showemptygroup");
        var multi = this.sourceElement.getAttribute("mult");
        var selected = this.sourceElement.getAttribute("dbv");
        var libelle = this.sourceElement.innerHTML;

        var showUserOnly = this.sourceElement.getAttribute("showuseronly");
        var showCurrentGroupFilter = this.sourceElement.getAttribute("showcurrentgroupfilter");
        var showCurrentUserFilter = this.sourceElement.getAttribute("showcurrentuserfilter");
        var useGroup = this.sourceElement.getAttribute("usegroup");
        var bShowCurrentUser = this.sourceElement.getAttribute("showcurrentuser") != "" ? this.sourceElement.getAttribute("showcurrentuser") == "1" : true;    //Vrai => Propose dans le catalogue : l'utilisateur en cours
        var bOnlyProfil = this.sourceElement.getAttribute("onlyprofil") != "" ? this.sourceElement.getAttribute("onlyprofil") == "1" : false;
        var bDisplayProfil = this.sourceElement.getAttribute("profil") != "" ? this.sourceElement.getAttribute("profil") == "1" : false;

        if (fullUserList == null) fullUserList = "";
        if (showEmptyGroup == null) showEmptyGroup = "";
        if (showUserOnly == null) showUserOnly = "";
        if (showCurrentGroupFilter == null) showCurrentGroupFilter = "";
        if (showCurrentUserFilter == null) showCurrentUserFilter = "";
        if (useGroup == null) useGroup = "";
        if (multi == null) multi = "";
        if (selected == null) selected = "";
        if (libelle == null) libelle = "";

        if (descId == 101036) {
            bOnlyProfil = "1";
            bDisplayProfil = "1";
        }

        var maxWidth = 550; //Taille max à l'écran (largeur)
        var maxHeight = (multi == 1) ? 640 : 590; //Taille max à l'écran (hauteur)
        var oTabWH = getWindowWH(top);
        var nWidth = oTabWH[0];
        var nHeight = oTabWH[1];
        if (nWidth > maxWidth)   //si largeur de l'écran plus grande que largeur max alors il faut utiliser la largeur max
            nWidth = maxWidth;
        else
            nWidth = nWidth - 10;   //marge de "sécurité"
        if (nHeight > maxHeight)   //si hauteur de l'écran plus grande que largeur max alors il faut utiliser la largeur max
            nHeight = maxHeight;
        else
            nHeight = nHeight - 10;   //marge de "sécurité"

        if (this.advancedDialog != null) {
            try {
                this.advancedDialog.hide();
                if (!(this.advancedDialog.bScriptOk && this.advancedDialog.bBodyOk))
                    top.setWait(false);
            }
            catch (e) {
                debugger;
            }
        }

        this.advancedDialog = new eModalDialog(this.catalogTitle, 0, "eCatalogDialogUser.aspx", nWidth, nHeight, "userdialog");

        if (typeof (customAbort) == "undefined" || customAbort == "")
            customAbort = function () {
                cancelAdvancedDialog(that);
            }



        if (typeof (customOk) == "undefined" || customOk == "")
            customOk = function () {
                ValidateUserDialog(that);
            }



        this.advancedDialog.ErrorCallBack = function () { setWait(false); }
        this.advancedDialog.TargetObject = obj;

        this.advancedDialog.addParam("multi", multi, "post");
        this.advancedDialog.addParam("selected", selected, "post");
        this.advancedDialog.addParam("descid", descId, "post");

        this.advancedDialog.addParam("showemptygroup", showEmptyGroup, "post");
        this.advancedDialog.addParam("showuseronly", showUserOnly, "post"); //si à 1 => la liste sera toujours sans groupes d'affichés
        this.advancedDialog.addParam("fulluserlist", fullUserList, "post");
        this.advancedDialog.addParam("modalvarname", this.jsVarName, "post");
        //On ajoute le finder à la liste des catalogues utilisateurs OUVERT
        top.eTabCatUserModalObject.Add(this.advancedDialog.iframeId, this.advancedDialog);
        this.advancedDialog.addParam("iframeId", this.advancedDialog.iframeId, "post");


            this.advancedDialog.addParam("showcurrentuser", (bShowCurrentUser ? "1" : "0"), "post");
            this.advancedDialog.addParam("showcurrentgroupfilter", showCurrentGroupFilter, "post"); //Si à 1 => Proposition dans le catalogue : <le groupe de l'utilisateur en cours> pour filtre avancé
            this.advancedDialog.addParam("showcurrentuserfilter", showCurrentUserFilter, "post");   //Si à 1 => Proposition dans le catalogue : <utilisateur en cours> pour filtre avancé
            this.advancedDialog.addParam("usegroup", useGroup, "post"); //si à 1 => Autorise la sélection de groupe pour le catatalogue simple

            this.advancedDialog.addParam("showvalueempty", "0", "post"); //si à 1 => Proposition dans le catalogue : <Vide> sur le catalogue simple
            this.advancedDialog.addParam("showvaluepublicrecord", "1", "post"); //si à 1 => Proposition dans le catalogue : <Fiche Publique> sur le catalogue simple
            this.advancedDialog.addParam("onlyprofil", (bOnlyProfil ? "1" : "0"), "post");
            this.advancedDialog.addParam("profil", (bDisplayProfil ? "1" : "0"), "post");

        this.advancedDialog.onIframeLoadComplete = function () { top.setWait(false); };
        this.advancedDialog.ErrorCallBack = function () { top.setWait(false); };

        //SPH : 24667
        // il faut masquer la liste des mru avant d'afficher la popup de recherche sinon les actions des mru restent partiellement actifs
        if (this.parentPopup && this.parentPopup.hide)
            this.parentPopup.hide();

        this.advancedDialog.show();

        this.advancedDialog.addButton(top._res_29, customAbort, "button-gray", this.jsVarName, "cancel", true);
        this.advancedDialog.addButton(top._res_5003, customOk, "button-green", this.jsVarName, "ok");
    };

    // Affiche la popup de définition de la géolocalisation
    this.openGeolocDialog = function (element) {

        if (element && typeof (element) !== "undefined") {
            this.parentPopup.sourceElement = element; // objet actuellement édité (auquel est rattaché l'éditeur et sa popup)
            this.sourceElement = element;
            this.headerElement = element.ownerDocument.getElementById(element.getAttribute("ename"));
            if (this.headerElement)
                this.headerElementId = this.headerElement.id;
        }

        if (typeof (this.sourceElement) == "undefined" || this.sourceElement == null) {
            return;
        }
        if (!this.headerElement)
            return;

        this.fileId = GetFieldFileId(this.sourceElement.id);

        var descId = "0";
        if (this.headerElement) {
            // le descid du champ
            descId = this.headerElement.getAttribute("did");
            // le libelle du champ
            this.catalogTitle = this.headerElement.getAttribute("lib");
        }

        this.advancedDialog = new eModalDialog(this.catalogTitle, 0, "eGeolocDialog.aspx", 950, 750, "modalGeoloc");
        this.advancedDialog.addParam("wkt", getAttributeValue(this.sourceElement, "dbv"), "post");
        this.advancedDialog.show();
        this.advancedDialog.addButton(top._res_29, function () {

            var modal = eTools.GetModal("modalGeoloc");
            modal.hide();

        }, "button-gray", null, "cancel");

        var srcElt = this.sourceElement;
        var fldEditor = this;

        this.advancedDialog.addButton(top._res_5003, function () {

            var modal = eTools.GetModal("modalGeoloc");
            var modalDoc = modal.getIframe().document;
            var wkt = modalDoc.getElementById("wkt");

            if (wkt) {
                var wktValue = wkt.value;


                fldEditor.selectedValues = [];
                fldEditor.selectedLabels = [];
                fldEditor.selectValue(wktValue, wktValue, true);
                fldEditor.validate();

                modal.hide();

            }




        }, "button-green", null, "ok");
    }

    //boite de dialog pour les champs de type fichier
    this.openFilesMgrDialog = function () {
        if (this.advancedDialog != null) {
            try {
                this.advancedDialog.hide();
            }
            catch (e) {
                debugger;
            }
        }

        this.advancedDialog = new eModalDialog(top._res_103, 0, 'eFieldFiles.aspx', 850, 500);
        this.advancedDialog.addParam("descid", getAttributeValue(this.headerElement, "did"), "post");
        this.advancedDialog.addParam("folder", getAttributeValue(this.GetSourceElement(), "pdbv"), "post");
        this.advancedDialog.addParam("files", this.value, "post");
        this.advancedDialog.addParam("mult", this.multiple ? "1" : "0", "post");

        this.advancedDialog.show();

        //  oModalPJAdd.addButton(top._res_29, cancelPJAdd, "button-gray",null);
        var myFunct = (function (obj) { return function () { validFileField(obj); } })(this);
        this.advancedDialog.addButton(top._res_5003, myFunct, "button-green", "", "ok"); //Valider


    };
    //#Region ##REPORTS##
    //Renommer un rapport
    this.renameReport = function () {
        var newName = '';
        if (document.getElementById('eInlineEditor'))
            newName = document.getElementById('eInlineEditor').value;

        if (newName.length == 0 || newName == trim(this.GetCurrentDBValue())) {
            return;
        }


        if (parentPopup.sourceElement && parentPopup.sourceElement.getAttribute("ename") == "COL_105000_105001") {

            var nReportId = GetFieldFileId(parentPopup.sourceElement.id);
            var url = "mgr/eReportManager.ashx";
            var ednu = new eUpdater(url, 0);
            ednu.ErrorCallBack = function () { };
            ednu.addParam("operation", 2, "post");
            ednu.addParam("reportname", newName, "post");
            ednu.addParam("reportid", nReportId, "post");
            ednu.send(onReportRenameTrait);        // fonction dans eTabsFieldsSelect.js
        }

    };
    //#EndRegion ##REPORTS##

    //#Region ##Modèle de mailing##
    this.renameMailTpl = function () {
        var newName = '';
        if (document.getElementById('eInlineEditor'))
            newName = document.getElementById('eInlineEditor').value;

        if (newName.length > 0) {

            if (parentPopup.sourceElement && parentPopup.sourceElement.getAttribute("ename") == "COL_107000_107001") {
                var MailTemplateId = GetFieldFileId(parentPopup.sourceElement.id);
                var url = "mgr/eMailingTemplateManager.ashx";
                var ednu = new eUpdater(url, 0);
                ednu.ErrorCallBack = function () { };
                ednu.addParam("operation", 2, "post");
                ednu.addParam("lbl", newName, "post");
                ednu.addParam("MailTemplateId", MailTemplateId, "post");
                ednu.send(onMailTplRenameTrait);
            }
        }

    };
    //#EndRegion ##Modèle de mailing##

    // Détruit ou remet à zéro l'objet courant
    this.resetOrDestroy = function () {
        this.trace("Remise à zéro de l'objet : " + this.autoResetMode);
        if (this.autoResetMode == 'destroy' || this.autoResetMode == 'reset') {
            var resetTimeout = (function (jsVarName) {
                return function () {
                    var existingObject = eval(jsVarName);
                    if (existingObject != null) {
                        if (existingObject.autoResetMode == 'reset') {
                            existingObject = new eFieldEditor(existingObject.type, existingObject.parentPopup, existingObject.jsVarName, existingObject.monitoredClass);
                            existingObject.trace("Objet réinitialisé !");
                        }
                        else if (existingObject.autoResetMode == 'destroy') {
                            existingObject = null;
                            existingObject.trace("Objet détruit !");
                        }
                    }
                };
            })(this.jsVarName);

            window.setTimeout(resetTimeout, 500);
        }

    };


    //vérifie si le champ est au bon format
    /// sValueToTest : valeur a tester
    /// sValueToTestRawFormat : valeur a tester avant transformation au format bdd (cas de  date/num)
    this.IsValueValid = function (sValueToTest, sValueToTestRawFormat) {

        var srcElement = that.GetSourceElement()
        if (srcElement == null)
            return false;

        sValueToTest = sValueToTest + "";
        if (sValueToTest == '')
            return true;
        switch (srcElement.getAttribute("eaction")) {
            case "LNKMAIL":
                that.SpecificErrorMessge = top._res_1023.replace('<EMAIL>', '[' + sValueToTest + ']');

                // HLA - Envoi Emiailing avec plusieurs emails séparés par ";" dans le champ email - On considère la première adresse mail comme principale et les suivantes en CC - #39682
                var mailValid = true;
                forEach(sValueToTest.split(';'), function (param) {
                    var mail = trim(param);
                    if (mail.length != 0)
                        mailValid = mailValid && eValidator.isEmail(mail);
                });

                return mailValid;
            case "LNKNUM":
                that.SpecificErrorMessge = top._res_673;
                return isValidNumber(sValueToTest);

            case "LNKFREETEXT":

                bIsDate = (getAttributeValue(that.headerElement, "frm") == FLDTYP.DATE);
                if (bIsDate) {

                    that.SpecificErrorMessge = top._res_1304 + " : " + eDate.CultureInfoDate();

                    //  that.SpecificErrorMessge = top._res_1023.replace('<EMAIL>', '[' + sValueToTest + ']');
                    return eValidator.isDateJS(sValueToTestRawFormat);
                }
                return true;
            case "LNKGEO":
                return eValidator.isGeo(sValueToTest);
                break;
        }



        return true;
    };

    // Génère des messages de diagnostic ; uniquement si this.debugMode = true
    this.trace = function (strMessage) {
        if (this.debugMode) {
            try {
                strMessage = 'eFieldEditor [' + this.jsVarName + '] -- ' + strMessage;

                if (typeof (console) != "undefined" && console && typeof (console.log) != "undefined") {
                    console.log(strMessage);
                }
                else {
                    alert(strMessage); // TODO: adopter une solution plus discrète que alert()
                }
            }
            catch (ex) {

            }
        }
    };
};
///Information d'une entrée de MRU de date
function itemDateEditor(sLibelle, sJs, sCss, sParam) {
    this.typeName = "itemDateEditor";
    this.Libelle = sLibelle;
    this.Js = sJs;
    this.Css = sCss;
    this.Param = sParam;
}
///séparateur de MRU de date
function separatorDateEditor() {
    this.separatorDateEditor = "itemDateEditor";
}

/*********************/
/* FONCTIONS ANNEXES */
/*********************/
//Catalogues : Partie de code éxécuté au clique sur annulé juste avant la fermeture du catalogue avancé
function partOfCancelCatDlg_FldEdt(catalogDialog, srcId) {
    var catalogObject = window[catalogDialog.getIframe().eC.jsVarNameEditor];
    catalogObject.cancel();
    catalogObject.filter = '';
}
//Catalogues : Partie de code éxécuté au clique sur valider juste avant la fermeture du catalogue avancé
function partOfValidateCatDlg_FldEdt(catalogDialog, srcId, trgId, tabSelectedLabels, tabSelectedValues, selectedIDs) {
    var catalogObject = window[catalogDialog.getIframe().eC.jsVarNameEditor];
    catalogObject.selectedValues = new Array();
    catalogObject.selectedLabels = new Array();
    var cntFld = 0;
    for (cntFld = 0; cntFld < tabSelectedValues.length; cntFld++) {
        var val = tabSelectedValues[cntFld];
        var label = tabSelectedLabels[cntFld];
        catalogObject.selectValue(val, label, true);
    }
    catalogObject.validate(); // Enregistrement des valeurs sélectionnées en base et de son rafraichissement
    catalogObject.filter = '';
}

//Pour les cat utilisateurs (et anciennement les autres catalogues);
function cancelAdvancedDialog(catalogObject) {


    //var catalogObject = window[jsVarName];
    catalogObject.cancel();
    catalogObject.advancedDialog.hide();
    catalogObject.filter = '';
}

function ValidateUserDialog(oCatEditor) {


    var modalObject = null;
    var oFrame = null;
    //var oCatEditor = window[jsVarNameEditor];



    modalObject = oCatEditor.advancedDialog;
    if (modalObject)
        oFrame = modalObject.getIframe();

    if (oCatEditor && modalObject && oFrame) {
        var strReturned = oFrame.GetReturnValue();
        modalObject.hide();
        var tabReturned = strReturned.split('$|$');
        var vals = tabReturned[0];
        var libs = tabReturned[1];

        oCatEditor.selectValue(vals, libs, true);
        oCatEditor.validate(); // Enregistrement des valeurs sélectionnées en base et de son rafraichissement



    }
    else
        alert("erreur : oFrame " + oFrame + " - modalObject " + modalObject + " - oFrame " + oFrame);
}

function catalogErrorSearch(oError) {
    //eAlert(0, top._res_225, top._res_6235, oError.UserDesc);
}


///summary
/// Méthode de callBack de la recherche sur un champ catalogue, depuis la MRU
///Traite les éléments de retours pour construire la liste des valeurs et la passer à renderValues
///<param name="oRes"></param>
///<param name="jsVarName"></param>
///<param name="bSilent"></param>
///summary
function catalogSearchTreatment(oRes, jsVarName, bSilent) {

    setWait(false);

    var strCatalogSearchSuccess = getXmlTextNode(oRes.getElementsByTagName("result")[0]);

    //Flag de permission d'ajouter sur le catalogue, conditionnant l'affichage de l'option ajouter dans la MRU
    var addAllowed = false;

    if (strCatalogSearchSuccess != "SUCCESS") {
        var errorDesc = getXmlTextNode(oRes.getElementsByTagName("errordescription")[0]);
        if (!bSilent) {
            eAlert(0, top._res_225, top._res_6235, errorDesc);
        }
        return;
    }

    var permissionNode = oRes.getElementsByTagName("addpermission");
    if (permissionNode != null && getXmlTextNode(permissionNode[0]) == "1")
        addAllowed = true;
    var nSearchLimit = oRes.getElementsByTagName("eSearchLimit");
    if (nSearchLimit != null)
        nSearchLimit = getNumber(getXmlTextNode(nSearchLimit[0]));

    var oCatalogElements = oRes.getElementsByTagName("element");

    var catalogObject = window[jsVarName];

    if (catalogObject.bBeginLnkFile) {  //une seule fois
        //Ajout du bouton Ajouter
        if (addAllowed) {
            AddBtn(catalogObject);
        }
        if (nSearchLimit != null)
            catalogObject.SearchLimit = nSearchLimit;    //Limite de car minimum avant recherche
        if (catalogObject.type == "linkCatFileEditor")
            addScript("eFinder", "FINDER"); //Methode d'ajout se trouve dans finder
    }
    if (catalogObject) {
        if (oCatalogElements.length > 0) {
            for (var i = 0; i < oCatalogElements.length; i++) {
                var oCatalogElement = oCatalogElements[i];

                var strValue = getXmlTextNode(oCatalogElement.childNodes[0]);
                var strLabel = getXmlTextNode(oCatalogElement.childNodes[1]);
                //Gestion des liaison PPID/ADRID/PMID - Champs de liaison uniquement
                var bMruLinkMode = false;
                if (oCatalogElement.childNodes.length > 3) {
                    var strAdrId = getXmlTextNode(oCatalogElement.childNodes[2]);
                    var strPmId = getXmlTextNode(oCatalogElement.childNodes[3]);
                    var strAdr01 = getXmlTextNode(oCatalogElement.childNodes[4]);
                    var strPm01 = getXmlTextNode(oCatalogElement.childNodes[5]);

                    strValue = strValue + ";|;" + strAdrId + "$|$" + strAdr01 + ";|;" + strPmId + "$|$" + strPm01;
                    //bMruLinkMode = true;
                }

                // Remplissage des valeurs
                //if (bMruLinkMode == true)
                //    catalogObject.addMruValue(strValue + "|" + strAdrId + "|" + strPmId, strLabel, false); // false car les valeurs sont déjà filtrées
                //else
                catalogObject.addValue(strValue, strLabel, false); // false car les valeurs sont déjà filtrées
            }
        }
        else {

            //Pas de résultat - croix-rouge
            if (document.getElementById("ImgSrchCatalog")) {
                document.getElementById("ImgSrchCatalog").className = "sprite-Editor sprite-no-valeur icon-edn-cross";
                document.getElementById("ImgSrchCatalog").setAttribute("close", "1");
            }
        }


        if (catalogObject.bBeginLnkFile && (typeof catalogObject.values == "undefined" || (catalogObject.values.length <= 0))) {
            //Si pas de résultat rétourné et que c'est le début de la recherche pour le champ de liaison on ouvre le champ de liaison car ça veut dire que la liste filtrée n'a pas de valeurs
            catalogObject.openLnkFileDialog(FinderSearchType.Link);
        } else {
            catalogObject.parentPopup.show();   //On affiche la pop up de mru
        }
        // mode recherche = true (empêche l'initialisation du champ de recherche avec la valeur sélectionnée)
        // initValues = false (ne réinitialise pas les valeurs, mais s'appuie sur celles ajoutées ci-dessus)
        catalogObject.renderValues(true, false, addAllowed);


    }
    else {
        // TODO
    }

}

///summary
/// Méthode de callBack de la recherche sur un champ de liaison, depuis la MRU de société d'une adresse PRO
///Traite les éléments de retours pour construire la liste des valeurs et la passer à renderValues
///<param name="oRes"></param>
///<param name="jsVarName"></param>
///<param name="bSilent"></param>
///summary
function catalogSearchDetailTreatment(oList, jsVarName, bSilent) {
    var oContent = document.getElementById("eCatalogEditorValues");
    removeClass(oContent, "CatValueNoResults");

    setWait(false);
    //SHA
    if (oContent != null) {
        var oldContent = oContent.innerHTML;    //on conserve le contenu de la cellule dans le cas ou pas de résultat renvoyé avant remplissage.
        oContent.innerHTML = oList;
        addClass(oContent, "MRUdetail");
    }

    mainDiv.style.overflowX = 'hidden';
    var catalogObject = window[jsVarName];
    if (!catalogObject)
        return;

    if (oContent.document)
        var oTab = oContent.document.getElementById("mt_" + catalogObject.tab);
    else
        var oTab = oContent.querySelector("table[id='mt_" + catalogObject.tab + "']");

    if (!oTab)
        return;
    adjustLastCol(catalogObject.tab);
    var nResult = getAttributeValue(oTab, "eNbResult");
    var bAddAllowed = (getAttributeValue(oTab, "addpermission") == "1");


    if (catalogObject.bBeginLnkFile && bAddAllowed) {  //une seule fois
        //Ajout du bouton Ajouter  
        AddBtn(catalogObject);
    }

    catalogObject.SearchLimit = getAttributeValue(oTab, "eSearchLimit");
    if (catalogObject.bBeginLnkFile && nResult <= 0) {
        //Si pas de résultat rétourné et que c'est le début de la recherche pour le champ de liaison
        //on ouvre le champ de liaison car ça veut dire que la liste filtrée n'a pas de valeurs
        catalogObject.openLnkFileDialog(FinderSearchType.Link);
    }
    else if (nResult <= 0) {
        removeClass(oContent, "MRUdetail");
        oContent.innerHTML = oldContent;

        // #33209 - "Aucune fiche trouvée" si pas de résultats
        document.getElementById("CatValueNoResults").innerHTML = top._res_78;

        catalogObject.parentPopup.show();   //On affiche la pop up de mru
    }
    else
        catalogObject.parentPopup.show();   //On affiche la pop up de mru

    /*Gestion d'erreur*/
    var tbError = document.getElementById("tbErrorUpdate");
    if (tbError) {
        showErr(tbError.value);
    }

    //#33199
    //On donne le focus à la recherche
    // Pour donner le focus en fin de textbox de recherche
    var ctrlSch;

    try {

        ctrlSch = document.getElementById("eCatalogEditorSearch");


        if (ctrlSch) {
            if (!isTablet())
                getFocusAfter(ctrlSch, 1);
        }
        else {
            ctrlSch = document.getElementById("eInlineEditor");
            if (ctrlSch)
                getFocusAfter(ctrlSch, 1);
        }
    }
    catch (_e) { }
}

function AddBtn(catalogObject) {
    //SHA
    //document.addEventListener('DOMContentLoaded', function () {
    var oAdvancedTR = document.getElementById("eCatalogEditorAdvancedTr");
    var oTFOOT = oAdvancedTR.parentNode;
    //SHA
    //var oTFOOT = (oAdvancedTR != null ||undefined) ? oAdvancedTR.parentNode : null;

    var oTR = document.createElement('TR');
    oTR.className = "eCatalogEditorAdvanced";
    oTR.id = "eCatalogEditorAdvancedAddTR";

    var oTD = document.createElement('TD');
    oTD.ID = "eCatalogEditorAdvancedAdd";
    oTD.className = "eCatalogEditorMenuItemAdv";
    oTD.setAttribute("style", "overflow-x: hidden; overflow-y: auto;");
    oTD.setAttribute("onclick", "AddFileFromMRU('" + top._res_18 + "','" + catalogObject.jsVarName + "');"); //ONCLICK
    oTR.appendChild(oTD);

    var oDIV = document.createElement('DIV');
    oTD.appendChild(oDIV);

    var oICN = document.createElement('SPAN');
    oICN.className = "mruAddBtn icon-add";
    oDIV.appendChild(oICN);
    var oSPAN = document.createElement('SPAN');
    SetText(oSPAN, top._res_18);    //Ajouter
    oSPAN.className = "eCatalogEditorMenuItemAdvSpan";
    oDIV.appendChild(oSPAN);

    var oDivContent = document.getElementById("eCatalogEditorValues");
    var oPopUp = document.getElementById("ePopupDiv");

    var bAdded = false;
    //SHA
    if (oTFOOT.querySelector("tr#eCatalogEditorAdvancedAddTR") != null) {
        bAdded = true;
    }
    // Si le bouton n'a pas été encore ajouté
    if (!bAdded) {
        if (catalogObject.bRevert) {  //Pop up en haut du champ
            oTFOOT.appendChild(oTR);
            //SHA
            if (oPopUp.style != null) {
                oPopUp.style.height = (getNumber(oPopUp.style.height) + 25) + "px";
                oPopUp.style.top = (getNumber(oPopUp.style.top) - 23) + "px";   //On décalle la pop up vers le haut vu que plus grande
            }
        }
        else {
            oTFOOT.insertBefore(oTR, oTFOOT.childNodes[0]);
            oDivContent.style.height = (getNumber(oDivContent.style.height) - 25) + "px";
        }
    }
    //});
}

function afterRenameCatalogValue(oRes, target) {

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

            var strChildrenDescid = getXmlTextNode(xmlCatalog.getElementsByTagName("ChildrenDescid")[0]);

            var strDbv = xmlRenamedValue.getAttribute("Dbv");
            var strLabel = xmlRenamedValue.getAttribute("Label");
            var strNewLabel = xmlRenamedValue.getAttribute("NewLabel");
            var strId = xmlRenamedValue.getAttribute("Id");

            var strNewDbv = xmlRenamedValue.getAttribute("NewDbv");

            var strParentDbv = xmlRenamedValue.getAttribute("ParentDbv");

            for (j = 0; j < parent.document.getElementsByTagName("td").length; j++) {

                var cell = parent.document.getElementsByTagName("td")[j];

                if (cell.getAttribute('ename') == null || cell.getAttribute('ename') == "") {
                    continue;
                }

                var aEName = cell.getAttribute('ename').split('_');
                var descid = aEName[aEName.length - 1];

                if (isNaN(descid))
                    continue;

                if ((';' + strImpactedDescid + ';').indexOf(';' + descid + ';') > -1 &&
                    ((';' + cell.getAttribute('dbv') + ';').indexOf(';' + strDbv + ';') > -1
                        || (!cell.getAttribute('dbv') && (';' + cell.innerHTML + ';').indexOf(';' + strDbv + ';') > -1)
                    ) &&
                    (cell.getAttribute('pdbv') == null || cell.getAttribute('pdbv') == strParentDbv)
                ) {
                    cell.innerHTML = (';' + cell.innerHTML + ';').replace(';' + strLabel + ';', ';' + strNewLabel + ';');
                    cell.innerHTML = cell.innerHTML.substring(1, cell.innerHTML.length - 1);

                    if (cell.getAttribute('dbv') != null) {
                        var strNew = (';' + cell.getAttribute('dbv') + ';').replace(';' + strDbv + ';', ';' + strNewDbv + ';');
                        strNew = strNew.substring(1, strNew.length - 1)
                        cell.setAttribute('dbv', strNew);
                    }
                }

                else {
                    if ((';' + strChildrenDescid + ';').indexOf(';' + descid + ';') > -1
                        && cell.getAttribute('pdbv') == strDbv
                    ) {

                        cell.setAttribute('pdbv', strNewDbv);
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
        }

        //renomme la valeur dans liste de valeurs complètes
        target.innerHTML = strNewLabel;
        target.parentNode.parentNode.setAttribute('ednval', strNewDbv);

    }
    else {
        var sErrorMsg = getXmlTextNode(oRes.getElementsByTagName("message")[0]);
        eAlert(0, top._res_92, top._res_6237, sErrorMsg + '<br>' + top._res_6236);
    }

}

function afterRenameMarkedFileValue(oRes, target) {
    var strSuccess = getXmlTextNode(oRes.getElementsByTagName("success")[0]);

    if (strSuccess == "1") {

        var strNewLabel = getXmlTextNode(oRes.getElementsByTagName("markedfilename")[0]);
        //renomme la valeur dans liste de valeurs complètes
        target.innerHTML = strNewLabel;
        target.parentNode.setAttribute('ednval', strNewLabel);

    }
    else {
        var sErrorMsg = getXmlTextNode(oRes.getElementsByTagName("message")[0]);
        eAlert(0, top._res_92, top._res_6237, sErrorMsg + '<br>' + top._res_6236);

    }
}

function afterGettingMemoValue(oRes, jsVarName) {
    var strSuccess = getXmlTextNode(oRes.getElementsByTagName("success")[0]);

    if (strSuccess == "1") {
        var strValue = getXmlTextNode(oRes.getElementsByTagName("value")[0]);
        var fieldEditorObject = window[jsVarName];
        if (fieldEditorObject) {
            fieldEditorObject.value = strValue;         // Affectation de la valeur à la propriété interne de eFieldEditor
            if (fieldEditorObject.memoEditor) {
                fieldEditorObject.memoEditor.setData(strValue, null);         // Affectation de la valeur dans le contrôle instancié entre temps
                fieldEditorObject.waitingForValue = false; // une fois la valeur récupérée, on autorise de nouveau les mises à jour en base
            }
        }
        setWait(false);
    }
    else {
        setWait(false);
        var sErrorMsg = getXmlTextNode(oRes.getElementsByTagName("message")[0]);
        eAlert(0, top._res_225, top._res_6235, sErrorMsg + '<br>' + top._res_6236);
    }
}

function getFocusAfter(_Ctrl, notSelect) {


    var _length = _Ctrl.value.length;
    _Ctrl.focus();

    if (_Ctrl.setSelectionRange) {


        // ASY (26890) [MRU] - après une recherche tous le texte est sélectionné ce qui empêche de continuer la saisie sans écrase ce que l'on a déjà saisie.
        // Ajout d'un parametre et de la condition pour ne pas perturber l existant ainsi que le dev de MZA-26346
        if ((notSelect == 'undefined') || (notSelect == null))
            _Ctrl.setSelectionRange(0, _length);

    }
    else if (_Ctrl.createTextRange) {
        var range = _Ctrl.createTextRange();
        range.collapse(true);
        range.moveEnd('character', _length);
        range.moveStart('character', 0);
        range.select();
    }
    if (_Ctrl.value == ' ')
        _Ctrl.value = '';
}





/// <summary>
///Permet de recharger une fiche en recalculant les valeurs des règles/conditions
// a partir de celles saisies par l'utilisateur*
// Ceci est utilisé pour le mode création et popup dans enregistrement auto
// afin de pouvoir calculer les règles de saisie (obligatoire/conditionnelle) à la volée
/// </summary>
/// <param name="nTab">Table à recharger</param>
/// <param name="aFields">Collection des champs de la fiche en cours (array descid/value)</param>
/// <param name="nFileid">Id de la fiche en cours (0 pour nouvelle fiche)</param>
/// <param name="nType">Paramètre type pour le filemanager. ex : accueil, liste, consult, vcard... cf enum eConst.eFileType </param>
/// <param name="eltId">Elément déclencheur de l'appel. Nécessaire pour le repositionnement de la fiche après rechargement</param>
function applyRuleOnBlank(nTab, aFields, nFileid, nType, eltId, arrParam, loadFrom) {



    if (typeof (aFields) == 'undefined' || aFields == null) {
        if (typeof (nTab) == 'undefined' || nTab == null || nTab == 0)
            return;

        aFields = getFieldsInfos(nTab, nFileid);
    }
    else {
        if (typeof (nTab) == 'undefined' || nTab == null || nTab == 0)
            nTab = getTabDescid(aFields[0].descId);
    }

    setWait(true);

    if (typeof (nType) == 'undefined' || nType == null)
        nType = 5; // Mode création 

    if (typeof (nFileid) == 'undefined' || nFileid == null)
        nFileid = 0; // Nouvelle fiche

    var oFileUpdater = getFileUpdater(nTab, nFileid, nType);
    for (var i = 0; i < aFields.length; i++) {
        var sFieldDesc = aFields[i].GetSerialize();

        oFileUpdater.addParam("field" + i, sFieldDesc, "post");
    }

    //      On récupère l'id de l'élément actuel et sa position verticale actuelle 
    //      pour ajuster le scroll à la fin de la mise à jour du code html

    var lblEltId = eltId;
    var headerElt = null
    var valElt = document.getElementById(eltId);

    if (eltId) {
        var aId = eltId.split('_');
        if (aId.length >= 3) {
            var nDescid = aId[2];
            oFileUpdater.addParam("trgdid", nDescid, "post");
        }

        lblEltId = getAttributeValue(valElt, "ename");
        headerElt = document.getElementById(lblEltId)
        if (headerElt)
            eltId = lblEltId;

    }



    var oEltScroll = valElt;
    var oMainDiv = document.getElementById("mainDiv");
    var oEltScrollY;
    try {
        oEltScrollY = getAbsolutePosition(oEltScroll).y;
    }
    catch (exp) { }


    var tmpDescId = getAttributeValue(headerElt, 'did');
    var bPrt = false;
    var efldeng200 = null;
    var efldeng300 = null;

    if (getAttributeValue(headerElt, 'prt') == "1") {
        bPrt = true;
        tmpDescId = getTabDescid(tmpDescId);
    }

    if (bPrt && (tmpDescId == 200 || tmpDescId == 300) && this.tab != 400 && this.tab != 200) {
        var elt200 = GetField(nTab, 201);
        var elt300 = GetField(nTab, 301);

        if (elt200 && elt300) {
            efldeng200 = getFldEngFromElt(elt200);
            efldeng300 = getFldEngFromElt(elt300);
        }
    }

    // création PP en popup avec ou sans adresse
    if (nTab == 200 && nFileid == 0) {
        // Par défaut on les ajoute
        var oWithoutAdrRadio = document.getElementById("COL_400_492_2");
        var withoutAdrRadioChecked = oWithoutAdrRadio != null && typeof (oWithoutAdrRadio) != "undefined" && oWithoutAdrRadio.tagName == "INPUT" && oWithoutAdrRadio.checked;
        oFileUpdater.addParam("withoutadr", withoutAdrRadioChecked ? "1" : "0", "post");
    }

    var oCtrl = document.querySelector("div#hv_" + nTab + " > input#ctrlId_" + nTab);
    if (oCtrl) {
        oFileUpdater.addParam("crtldescid", oCtrl.value, "post");
    }


    //informations relatives au traitement de masse

    if (nFileid == 0 && isPopup()) {
        // HLA - Dans le cas du globalinvit le eModFile n'existe pas - Problème ajout d'inscriptions en masse - signet ++ en XRM - reload de la fenêtre en création ++ - #38743
        if (top.modalWizard && top.modalWizard.getParam("wizardtype") == "invit"
            && top.modalWizard.getIframe()
            && top.modalWizard.getIframe().oAffectFile && top.modalWizard.getIframe().oAffectFile.bIsInvit) {
            oFileUpdater.addParam("globalaffect", "1", "post");
            oFileUpdater.addParam("globalinvit", "1", "post");
        }
        else if (top.eModFile && (top.eModFile.GlobalAffect || top.eModFile.GlobalInvit)) {
            oFileUpdater.addParam("globalaffect", "1", "post");
            oFileUpdater.addParam("globalinvit", top.eModFile.GlobalInvit ? "1" : "0", "post");
        }
    }

    //
    //#37334  - écrasement des adresse postal par l'adr pro -> fourniture du parm a l'applyrules
    if (nFileid == 0 && isPopup() && (nTab == 200 || nTab == 300 || nTab == 400)) {
        if (getAttributeValue(valElt, "adrpro") && getAttributeValue(valElt, "adrprooverwrite") == "1") {
            oFileUpdater.addParam("adrprooverwrite", "1", "post");
        }
    }

    if (Array.isArray(arrParam)) {

        arrParam.forEach(

            function (ez) {

                oFileUpdater.addParam(ez.name, ez.value, "post");

            }
        )
    }


    oFileUpdater.send(function (oRes) {
        updateFile(oRes, nTab, nFileid, nType, false, eltId, oEltScrollY, true, loadFrom);
        if (nTab != 400) {
            chkAdr0(efldeng200, efldeng300);
        }
    });

}


function getChildrenFields(nTab, nDescid, nFileid) {

    if (typeof (nTab) == "undefined" || typeof (nDescid) == "undefined")
        return;

    if (typeof (nFileid) == "undefined")
        nFileid = 0;

    var aFields = getFieldsInfos(nTab, nFileid)
    var aChildrenFields = new Array();

    for (var i = 0; i < aFields.length; i++) {
        var f = aFields[i];
        if (f.boundDescId == nDescid)
            aChildrenFields.push(f);
    }

    return aChildrenFields;

}

function hasChildrenFields(nTab, nDescid, nFileid) {
    if (typeof (nTab) == "undefined" || typeof (nDescid) == "undefined")
        return false;

    if (typeof (nFileid) == "undefined")
        nFileid = 0;

    var aFields = getFieldsInfos(nTab, nFileid);
    var aChildrenFields = new Array();

    for (var i = 0; i < aFields.length; i++) {
        var f = aFields[i];
        if (f.boundDescId == nDescid)
            return true;
    }

    return false;

}

/*********KHA validation de champs de type fichier***********/
function validFileField(objEditor) {

    var aReturnValue = objEditor.advancedDialog.getIframe().getSelectedFiles();

    var oDoc = objEditor.advancedDialog.getIframe().document;

    var divList = oDoc.getElementById("divLstFiles");
    if (divList != null) {
        var lstDiv = divList.children;
        for (var i = 0; i < lstDiv.length; i++) {
            var ref = lstDiv[i].getAttribute("ref");
            if (ref != null) {
                objEditor.ForceRefresh = ref == "1" ? true : false;
                break;
            }
        }
    }

    objEditor.advancedDialog.hide();
    objEditor.selectedValues = new Array();
    objEditor.selectedLabels = new Array();

    for (i = 0; i < aReturnValue.length; i++) {
        objEditor.selectValue(aReturnValue[i], aReturnValue[i], true);
    }

    objEditor.validate();

}

/*
Methode de fermeture de la modal du champ de liaison
*/
function cancelLnkFile(iframeId) {
    var catalogObject = top.eTabLinkCatFileEditorObject[iframeId]

    // demande 36826 : MCR :  Pour gérer les appels entrants des CTIs
    top.nModalLnkFileLoaded = top.nModalLnkFileLoaded - 1;

    catalogObject.oModalLnkFile.hide();
}

/*
Methode de dissociation de fiche de la modal du champ de liaison
*/
function dissociateLnkFile(iframeId, onCustomOk) {
    // On vide la/les valeur(s) selectionnée(s)
    var oFrm = top.document.getElementById(iframeId);
    var oFrmDoc = oFrm.contentDocument;
    var oFrmWin = oFrm.contentWindow;
    oFrmWin._selectedListValues = new Array();

    onCustomOk(iframeId);
}

function validateLnkMRU(jsVarName) {
    var selectedListValues = _selectedListValues;
    var catalogObject = window[jsVarName];
    if (!catalogObject)
        return;
    //REMISE A ZERO
    catalogObject.selectedValues = new Array();
    catalogObject.selectedLabels = new Array();
    if (selectedListValues.length > 0) {
        for (var i = 0; i < selectedListValues.length; i++) {
            var oItem = document.getElementById(selectedListValues[i]);

            if (!oItem)
                continue;

            // id  
            var oId = oItem.getAttribute("eid").split('_');
            var nTab = oId[0];
            var nId = oId[oId.length - 1];

            var tabTd = oItem.getElementsByTagName("td");
            //Libellé de la première colonne
            var label = GetText(tabTd[0]);
        }

        catalogObject.selectValue(nId, label, true);
    }
    else
        catalogObject.selectValue("", "", true);


    catalogObject.validate(); // Enregistrement des valeurs sélectionnées en base et de son rafraichissement

    /*********************************************************************/
}
// TODO: fonction hors objet, requis par eModalDialog
function validateLnkFile(iframeId) {



    var catalogObject = top.eTabLinkCatFileEditorObject[iframeId];

    var oFrm = top.document.getElementById(iframeId);
    if (!oFrm)
        return;

    var oFrmDoc = oFrm.contentDocument;
    var oFrmWin = oFrm.contentWindow;
    var selectedListValues = oFrmWin._selectedListValues;
    if (typeof (selectedListValues) == 'undefined')
        selectedListValues = new Array();


    //REMISE A ZERO
    catalogObject.selectedValues = new Array();
    catalogObject.selectedLabels = new Array();
    if (selectedListValues.length > 0)
        for (var i = 0; i < selectedListValues.length; i++) {
            var oItem = oFrmDoc.getElementById(selectedListValues[i]);

            if (!oItem)
                continue;

            var oMainDiv = oFrmDoc.getElementById("mainDiv");
            var nTabFrom = getNumber(getAttributeValue(oMainDiv, "tabFrom"));
            var nTargetTab = getNumber(getAttributeValue(oMainDiv, "tab"));
            var nSourceDescId = getNumber(getAttributeValue(oMainDiv, "did"));
            //Table parente PP
            var bPpInFile = catalogObject.tabLnk && catalogObject.tabLnk.indexOf("200") && nSourceDescId == "200";
            //Table parente PM
            var bPmInFile = catalogObject.tabLnk && catalogObject.tabLnk.indexOf("300") && nSourceDescId == "300";
            //La table ciblée est une liaison haute de la table parente mas n'est pas PP ou PM
            var bEventInFile = (catalogObject.tabLnk && catalogObject.tabLnk.indexOf(nTargetTab) >= 0 && nSourceDescId == nTargetTab && nTargetTab != "200" && nTargetTab != "300");

            // id  
            var oId = oItem.getAttribute("eid").split('_');
            var nTab = oId[0];
            var nId = oId[oId.length - 1];

            //kha : depuis un future catalogue à choix multiple, cela ne fonctionnera plus à cause des cases à cocher
            // on pourra alors tester ceci : 
            var tabTd = oItem.getElementsByTagName("td");
            //var ename = "COL_" + nTab + "_" + (nTab+1);
            //var tabTd = oItem.querySelector("td[ename='"+ename+"']");

            //Libellé de la première colonne
            var label = GetText(tabTd[0]);
            /*Depuis Contact/Société Récupération des informations sur l'adresse et la Société/Contact sélectionnée*/
            var nAdrId = -1;
            var sAdr01 = "";
            var nPmId = -1;
            var sPm01 = "";
            var nPpId = -1;
            var sPp01 = "";
            if (bPpInFile || bPmInFile || bEventInFile) {
                for (var j = 0; j < tabTd.length; j++) {
                    //GCH : on ne peut affecter que les fiches dont on a les droits de visu.
                    if (getAttributeValue(tabTd[j], "eNotV") != "1") {
                        //Recherche 400
                        if (nAdrId < 0 && tabTd[j].id && getAttributeValue(tabTd[j], "ename").search("^COL_[2-3]00_4[0-9]{2}$") == 0) { //ADDRESS depuis PP ou PM
                            nAdrId = GetFieldFileId(tabTd[j].id);
                            sAdr01 = GetText(tabTd[j]);
                        }
                        //Recherche 300
                        else if (nPmId < 0 && tabTd[j].id
                            && (
                                (getAttributeValue(tabTd[j], "ename").search("^COL_[2-3]{1}00_400_3[0-9]{2}$") == 0)    //PM depuis PP
                                ||
                                (bEventInFile && getAttributeValue(tabTd[j], "ename").search("^COL_" + nTargetTab + "_301$") == 0)    //PM depuis EVENT

                            )
                        ) {
                            nPmId = GetFieldFileId(tabTd[j].id);
                            sPm01 = GetText(tabTd[j]);
                        }
                        //Recherche 200
                        else if (nPpId < 0 && tabTd[j].id
                            && (
                                (getAttributeValue(tabTd[j], "ename").search("^COL_[2-3]{1}00_400_2[0-9]{2}$") == 0)    //PP depuis PM
                                ||
                                (bEventInFile && getAttributeValue(tabTd[j], "ename").search("^COL_" + nTargetTab + "_201$") == 0)  //PP depuis EVENT
                            )

                        ) {
                            nPpId = GetFieldFileId(tabTd[j].id);
                            sPp01 = GetText(tabTd[j]);
                        }
                    }

                }
            }



            //// Si on choisit PM à partir d'ADDRESS, on n'inclut pas le ppid
            //if (!(nTargetTab == 300 && nTabFrom == 400))
            nId = nId + ";|;" + nPpId + '$|$' + sPp01;
            nId = nId + ";|;" + nPmId + '$|$' + sPm01;
            nId = nId + ";|;" + nAdrId + '$|$' + sAdr01;

            catalogObject.selectValue(nId, label, true);
        }
    else
        catalogObject.selectValue("", "", true);



    catalogObject.validate(); // Enregistrement des valeurs sélectionnées en base et de son rafraichissement

    // demande 36826 : MCR :  Pour gérer les appels entrants des CTIs
    top.nModalLnkFileLoaded = top.nModalLnkFileLoaded - 1;

    catalogObject.oModalLnkFile.hide();
}


/*setLinkedFiles : Force la mise à jours des id et libellé pour les champs de liaisons
nAdrId, sAdr01  Adresse : id et son libellé
nPmId, sPm01  Contact : id et son libellé
nPpId, sPp01  Société : id et son libellé
nTabFrom : table de la fiche en cours
*/
function setLinkedFiles(descidTrigger, nAdrId, sAdr01, nPmId, sPm01, nPpId, sPp01, nTabFrom) {
    /*******************************************************************************************************/
    //Affectation de la société/Contact/Adresse lié de la valeurs sélectionné à la fiche appelante si la liaison n'est pas déjà renseignée.
    if (nTabFrom > 0 && (nAdrId > 0 || nPmId > 0 || nPpId > 0)) {
        setLinkedFile(nTabFrom, 201, nPpId, sPp01, descidTrigger == 200);
        setLinkedFile(nTabFrom, 301, nPmId, sPm01, descidTrigger == 300);
        if (nTabFrom != 400)
            setLinkedFile(nTabFrom, 401, nAdrId, sAdr01, descidTrigger == 200 || descidTrigger == 400);	////GCH #36048 : même si pas d'adresse de sélectionnée, on vide l'adresse
    }
    /*********************************************************************/
}
//
//bForceUpdate : Met à jour la valeurs même si vide
function setLinkedFile(nTabFrom, nDescid, nFileId, sLibelle, bForceUpdate) {
    nFileId = getNumber(nFileId);
    if (isNaN(nFileId) || nFileId <= 0) {
        nFileId = "";
        sLibelle = "";
    }
    var elt = GetField(nTabFrom, nDescid);
    if (elt && (nFileId > 0 || bForceUpdate)) {
        var dbv = getAttributeValue(elt, "dbv");
        elt.setAttribute("oldvalue", elt.value);
        elt.setAttribute("olddbv", dbv);
        if (dbv <= 0 || dbv == '' || bForceUpdate) {
            elt.value = sLibelle;
            elt.setAttribute("lnkid", nFileId);
            elt.setAttribute("dbv", nFileId);

            if (nFileId > 0) {
                elt.setAttribute("eaction", "LNKGOFILE");
                top.switchClass(elt, "LNKCATFILE", "LNKGOFILE gofile");
            }
        }
    }
}


//JS a chargé à l'ouverture du finder
function onLnkFileLoad(iFrameId, nTab) {
    try {
        var catalogObject = top.eTabLinkCatFileEditorObject[iFrameId];

        var oFrm = catalogObject.oModalLnkFile.getIframe();
        var oFrmDoc = oFrm.document;
        var oFrmWin = oFrm.window;

        // Donne le focus à la textbox de recherche
        if (oFrmDoc.getElementById("eTxtSrch")) {

            var oInpt = oFrmDoc.getElementById("eTxtSrch");

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
                oInpt.focus();
            }

            //SPH : conserver ka recherche MRU et mettre la focus dedans (car le controle n'est pas affiché lorsque l'on fait le focus())
            var val = oInpt.value;
            oInpt.value = '';
            oInpt.value = val;

        }
        var oMainDiv = oFrmDoc.getElementById("mainDiv");

        oFrmWin.thtabevt.init();
        oFrmWin.adjustLastCol(nTab, oMainDiv);
        oFrmWin.initHeadEvents();
        oFrmWin.afterListLoaded();
        oFrmWin._parentIframeId = iFrameId;
        /*Gestion d'erreur*/
        var tbError = oFrmWin.document.getElementById("tbError");
        if (tbError) {
            oFrmWin.showErr(tbError.value);
        }
    }
    finally {
        top.setWait(false);
    }
}

//Fonction de validation du calendrier pour les rubirques de type date - début
// dans le cas de eFieldEditor, les arguments op et frmId sont inutilisés.
function updDateFromCalendar(date, op, objName, frmId) {
    /*cas de la popup calendar appelée depuis un mode popup
    on récupère en plus l'id de la popup pour conserver le contexte d'exécution
    */
    var oDateEditor = window[objName];
    if (oDateEditor == null || typeof (oDateEditor) == "undefined")
        return;
    oDateEditor.updDate(date);
    if (oDateEditor.advancedDialog)
        oDateEditor.advancedDialog.hide();
}
function onCalendarEditorOk(objName) {
    var oDateEditor = window[objName];
    var oAdvEdictor = null;
    if (oDateEditor == null || typeof (oDateEditor) == "undefined") {

        if (!oDateEditor.getIframe)
            oAdvEdictor = oDateEditor;
        return;
    }
    else
        oAdvEdictor = oDateEditor.advancedDialog;

    var date = oAdvEdictor.getIframe().eCalendarControl.GetDate("1");

    updDateFromCalendar(date, "0", objName);
    oAdvEdictor.hide();
}

// Fonction de validation du calendrier pour les rubirques de type date - Fin
function getChkAdrUpd(ppid, pmid) {
    var chkAdrUpdater = new eUpdater("mgr/eAdrChkManager.ashx", 0);
    chkAdrUpdater.ErrorCallBack = function () { }
    chkAdrUpdater.addParam("ppid", ppid, "post");
    chkAdrUpdater.addParam("pmid", pmid, "post");
    return chkAdrUpdater;
}

function chkAdr0(ppfldeng, pmfldeng) {
    if (!ppfldeng || !pmfldeng
        || ppfldeng.newValue == "" || pmfldeng.newValue == ""
        || ppfldeng.newValue == "0" || pmfldeng.newValue == "0")
        return;

    var chkAdrUpdater = getChkAdrUpd(ppfldeng.newValue, pmfldeng.newValue);
    chkAdrUpdater.send(function (oRes) { chkAdr1(oRes, ppfldeng, pmfldeng); });
}

function chkAdr1(oRes, ppfldeng, pmfldeng) {
    var sError = getXmlTextNode(oRes.getElementsByTagName("Error")[0]);
    if (sError != "") {
        eAlert(0, top._res_72, sError);
        return;
    }

    var doesAdrExst = (getXmlTextNode(oRes.getElementsByTagName("AdrExists")[0]) == 1);
    if (!doesAdrExst) {
        var sLabel200 = getXmlTextNode(oRes.getElementsByTagName("Lbl200")[0]);
        var sLabel300 = getXmlTextNode(oRes.getElementsByTagName("Lbl300")[0]);
        var sQuestion = top._res_94.replace("<PPNAME>", sLabel200).replace("<PMNAME>", sLabel300);
        eConfirm(1, "", sQuestion, "", null, null, function () { autoCreate(400, new Array({ tab: 200, fid: ppfldeng.newValue }, { tab: 300, fid: pmfldeng.newValue }), true); }, null);
    }
}
