function ePopup(jsVarName, width, height, ptop, left, relativeTo, modalPopup, hideOnScroll) {

    this.childElement = null;
    this.div = null;
    this.backgroundDiv = null;
    this.jsVarName = jsVarName;

    this.width = width;
    this.height = height;
    this.top = ptop;
    this.left = left;
    this.zIndex = 0;
    this.isShown = false;

    // Conservation des paramètres de taille/position initialement passés à la popup, afin de pouvoir les restaurer depuis les contrôles enfant (this.parentPopup) si la taille/position
    // réelle a été modifiée
    this.initialWidth = width;
    this.initialHeight = height;
    this.initialTop = ptop;
    this.initialLeft = left;
    this.initialZIndex = 0;

    this.relativeTo = relativeTo;
    this.modalPopup = modalPopup;
    this.hideOnScroll = hideOnScroll;

    var that = this;

    this.sourceElement = null; // objet auquel est rattachée la popup
    this.openerElement = null; // objet ayant déclenché l'ouverture de la popup (qui n'est pas forcément sourceElement, ex. un bouton déporté)

    if (!this.relativeTo)
        this.relativeTo = document.body;

    var bAppendToParent = (this.div == null);
    //var font = document.getElementById("container").classList[1];
    if (bAppendToParent) {
        if (document.getElementById("ePopupDiv")) {
            this.div = document.getElementById('ePopupDiv');
        }
        else {
            this.div = document.createElement('div');
            this.div.id = "ePopupDiv";
            this.div.className = "popup "/* + font*/;
        }
        this.div.style.display = "none";
    }

    //Image chargement
    this.div.innerHTML = "";

    /*
    lors de la construction de l'objet popup on affect à body le onclick qui va permettre de cacher le popup 
    lorsqu'on clic à l'exterieur
    */

    if (document.body) {
        var obj = document.body;
    }

    if (document.body) {

        var oldClickFct = document.onclick;
        document.onclick = function (event) {

            if (oldClickFct)
                oldClickFct();

            // kha le 27/06/2014
            // le clic dans une modal dialog (ealert) ne doit pas impliquer la disparition d'un menu (mru)
            //c'est malheureusement le seul test que j'ai trouvé étant donné que la dite modal a été fermée au moment du déclenchement...

            if (event) {
                var src = event.target || event.srcElement;
                if (src.parentElement)
                    that.hideOnBlur(event);
            }
            else {
                that.hideOnBlur(event);
            }

            // Fermeture de menu au clic à l'extérieur
            if (typeof (oAutoCompletion) != "undefined")
                oAutoCompletion.dispose();
        };

        // #67 822 - De la même manière, affectation au onscroll si demandé (ex : adresses prédictives)
        // Inspiré de eMain.initEditorsOnScroll();
        if (this.hideOnScroll) {
            var aScrollableElementIds = new Array("divBkmPres", "mainDiv", "md_pl-base", "divDetailsBkms"); // liste de conteneurs scrollables connus sur XRM
            var aScrollableElements = new Array(document.body); // on place une surveillance sur le corps de la page
            for (var i = 0; i < aScrollableElementIds.length; i++)
                aScrollableElements.push(document.getElementById(aScrollableElementIds[i]));
            for (var i = 0; i < aScrollableElements.length; i++) {
                if (aScrollableElements[i]) {
                    var oldScrollFct = aScrollableElements[i].onscroll;
                    aScrollableElements[i].onscroll = function (event) {
                        if (oldScrollFct)
                            oldScrollFct();
                        that.hideOnBlur(event);
                        // Fermeture de menu au clic à l'extérieur
                        if (typeof (oAutoCompletion) != "undefined")
                            oAutoCompletion.dispose();
                    }
                }
            }
        }
    }


    if (bAppendToParent)
        this.relativeTo.appendChild(this.div);

    this.show = function (width, height, ptop, left, zIndex) {
        if (width)
            this.width = width;
        if (height)
            this.height = height;
        if (ptop)
            this.top = ptop;
        if (left)
            this.left = left;
        if (zIndex)
            this.zIndex = zIndex;

        this.div.style.display = 'block';

        this.div.style.width = this.width + 'px';
        //this.div.style.height = this.height + 'px';
        this.div.style.height = '';
        this.div.style.top = this.top + 'px';
        this.div.style.left = this.left + 'px';
        this.div.style.zIndex = this.zIndex ? this.zIndex : '';


        this.div.style.position = 'absolute';


        if (this.modalPopup && (this.backgroundDiv == null)) {


            this.backgroundDiv = document.createElement('div');
            this.backgroundDiv.id = "ePopupDivBackground";
            this.backgroundDiv.className = "ePopupBackground";
            this.backgroundDiv.style.position = 'absolute';
            this.backgroundDiv.style.left = 0 + "px";
            this.backgroundDiv.style.top = 0 + "px";
            this.backgroundDiv.style.width = document.body.scrollWidth + "px";
            this.backgroundDiv.style.height = document.body.scrollHeight + "px";

            this.backgroundDiv.style.opacity = 0.20;
            this.backgroundDiv.style.MozOpacity = 0.20;
            this.backgroundDiv.style.KhtmlOpacity = 0.20;
            this.backgroundDiv.onclick = this.onClick;
            document.body.appendChild(this.backgroundDiv);
        }

        this.isShown = true;
    }

    this.hide = function () {
        if (this && this.div && this.isShown) {
            this.div.style.display = 'none';
            this.isShown = false;
            this.div.innerHTML = '';
            this.childElement = null;

        }
        this.relativeTo = document.body;
    };


    this.hideOnBlur = function (evt) {
        if (this.childElement) {
            if (window.event)
                evt = window.event;

            var clickedElement = null;
            if (evt) {
                if (evt.target)
                    clickedElement = evt.target;
                else if (evt.srcElement)
                    clickedElement = evt.srcElement;
            }

            if (clickedElement) {

                // On ne masque pas la popup si le clic a été effectué sur un élément contenu à l'intérieur de la popup
                // ou sur celui ayant déclenché son ouverture 

                if (clickedElement == this.sourceElement || this.sourceElement == null || clickedElement == this.openerElement || this.openerElement == null || !this.isShown)
                    return;


                var bValidateOnBlur = true;

                if (clickedElement.getAttribute("ednNoValidate") && clickedElement.getAttribute("ednNoValidate") == "1")
                    bValidateOnBlur = false;

                var isPopupChildElement = false;
                var currentTarget = clickedElement;
                while (!isPopupChildElement && currentTarget.parentNode != undefined && currentTarget.parentNode != null) {
                    isPopupChildElement = (currentTarget.parentNode == this.div);
                    currentTarget = currentTarget.parentNode;
                }

                if (!isPopupChildElement) {
                    // Et lorsqu'on masque la popup, on valide la nouvelle valeur si paramétré comme tel
                    // si l'on est en train de modifier la valeur d'un catalogue 
                    var bDontHide = false;
                    switch (this.childElement.action) {
                        /// /!\ !!!! TOUTES CES ACTIONS DOIVENT ETRE IDENTIQUES A CELLES PRESENTES DANS LA FONCTION findValues de eFieldEditor.js !!!! /!\

                        case "GAdate":

                            break;
                            // répartition des fiches lors du traitement de fiche sur appartient à   
                        case 'GARepNb':
                            updNb(this.childElement.sourceElement);
                            break;
                        case 'renameCatalogValue':
                            this.childElement.renameCatalogValue();
                            break;
                        case 'renameMarkedFileValue':
                            this.childElement.renameMarkedFileValue();
                            break;
                        case 'renameTreeViewValue':
                            this.childElement.renameTreeViewValue();
                            break;
                        case 'renameFilter':
                            this.childElement.renameFilter();
                            break;
                        case 'renameFormular':
                            this.childElement.renameFormular();
                            break;
                        case 'renameMailTpl':
                            this.childElement.renameMailTpl();
                            break;
                        case 'renameView':
                            //permet de garder le contexte d'exécution                         

                            var sViewName = this.childElement.value;
                            var sNewViewName = "";

                            var inptEditor = document.getElementById('eInlineEditor');
                            if (inptEditor)
                                sNewViewName = inptEditor.value;

                            if (sNewViewName != sViewName) {
                                var myFunct = (function (obj) { return function () { obj.renameView(); obj.parentPopup.hide(); } })(that.childElement);
                                eConfirm(1, top._res_86, top._res_268.replace("<OLDNAME>", sViewName).replace("<NEWNAME>", sNewViewName), "", null, null, myFunct, function () { that.hide() });
                                bDontHide = true;
                            }

                            break;
                        case 'renameReport':
                            // -> pas de confirm sur le rename Report, permet de garder le contexte d'exécution
                            this.childElement.renameReport();
                            break;
                        case 'editFullScreenValue':
                            // Avec eFieldEditor en mode Edition plein écran (ex : champ Mémo en mode Liste), il ne faut PAS déclencher la validation du champ affiché en plein écran lorsqu'on en sort
                            // Cette validation doit être gérée par la fenêtre Plein écran elle-même, avec ses boutons Valider/Annuler

                            break;
                        case 'renameFile':
                            // renameFile se trouve dans pj.js

                            if (typeof (renameFile) == "function") {
                                var sFileName = this.childElement.value;
                                var sNewFileName = "";

                                var inptEditor = document.getElementById('eInlineEditor');
                                if (inptEditor)
                                    sNewFileName = inptEditor.value;

                                if (sFileName != sNewFileName) {
                                    var myFunct = (function (obj) { return function () { renameFile(obj.childElement); } })(that);
                                    eConfirm(1, top._res_86, top._res_6692.replace("<OLDNAME>", sFileName).replace("<NEWNAME>", sNewFileName), "", null, null, myFunct, function () { that.hide(); });
                                    bDontHide = true;
                                }

                            }
                            break;
                        case 'renameImportTemplate':
                            this.childElement.renameImportTemplate();
                            break;
                        default:

                            if (bValidateOnBlur && typeof (this.childElement.validate) == "function") {
                                // Valide la valeur de l'input du catalog editor, quand on clique à l'exterieur  (MOU 22-10-2014 #33153)
                                this.childElement.setValueOnHide();
                                this.childElement.validate(this.childElement.validateOnHide);
                            }
                            break;
                    }
                    if (!bDontHide)
                        this.hide();
                }
            }
        }
    };


    this.onClick = function (evt) {
        var clickEvent = evt;
        if (window.event)
            clickEvent = window.event;

        var target = null;
        if (clickEvent) {
            if (clickEvent.srcElement)
                target = clickEvent.srcElement;
            else
                target = clickEvent.target;
        }

        if (target == document.getElementById("divBackground")) {
            target.style.display = 'none';
            this.div.style.display = 'none';
        }
    };

    this.addOnClick = function (obj) {
        var existingOnClickContents = obj.getAttribute("onclick");
        if (existingOnClickContents && (existingOnClickContents != ''))
            existingOnClickContents += '; ';
        else
            existingOnClickContents = '';

        obj.setAttribute("onclick", existingOnClickContents + this.jsVarName + '.onClick(event);');
    };

}






