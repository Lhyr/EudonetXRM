
//*****************************************************************************************************//
//*****************************************************************************************************//
//*** JBE - 09/2011 - Classe pour afficher les fenêtres modales en div
//** title: Titre de la fenêtre
//** type: type (modale ou msgbox)
//** url: url de la page si modal
//** width: largeur
//** height: hauter
//** handle : Nom unique de la modal pour la récupérer dans le tableau global ( top.window['_md'][handle] )
//** bDontIgnoreSetWait : Permet de forcer l'affichage au dessus des set wait
//*****************************************************************************************************//
//*****************************************************************************************************//

var activeModalId;

function eModalDialog(title, type, url, width, height, handle, bDontIgnoreSetWait, bIsExternal, bIsAvancedFormular) {

    if (top && !top.window['_md'])
        top.window['_md'] = [];

    if (top && !top.window['_mdName'])
        top.window['_mdName'] = [];

    this.debugMode = false; // à mettre à true pour afficher certains messages de diagnostic - NE PAS UTILISER EN PRODUCTION !

    // Génère des messages de diagnostic ; uniquement si this.debugMode = true
    this.trace = function (strMessage) {
        if (this.debugMode) {
            try {
                strMessage = 'eModalDialog [' + this.UID + '] -- ' + strMessage;

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

    this.onHideFunction = null; //fonction exécutée après fermeture de la modaldialog

    //Ajout de le menu contextuel dans le tableau global des modal dialog (_md)
    if (typeof handle != "undefined" && handle != null) {
        if (top) {
            top.window['_md'][handle] = this;
        }
    }

    var that = this; // pointeur vers l'objet eModalDialog lui-même, à utiliser à la place de this dans les évènements onclick (ou this correspond alors à l'objet cliqué)

    this.myOpenerDoc = document;    // Mémorisation du doc de l'ouvrant
    this.myOpenerWin = window;    // Mémorisation du doc de l'ouvrant
    this.openMaximized = false;
    this.isStatic = false

    //Type du message
    var ModalType =
    {
        ModalPage: 0,   //Page en popup
        MsgBoxLocal: 1, //Messagebox avec un simple message à afficher
        Prompt: 2,      //Fenêtre pour introduire une valeur texte
        Waiting: 3,       //Attente - TODO
        ProgressBar: 4,   //Barre de progression - TODO
        ToolTip: 5,  //Fenetre ToolTip simplifié (mini fenetre de résumer pour les planning)
        ToolTipSync: 6,  //Fenetre ToolTip simplifié (mini fenetre de résumer pour les planning)
        VCard: 7,   //Fenêtrfe vcard
        ColorPicker: 8, //Selecteur de couleurs
        SelectList: 9, //Liste de sélection (boutons radio) - A Supprimmer, maintenant les boutons sont ajoutable tous le temps.
        DisplayContent: 10, //affiche un élément du DOM
        DisplayContentWithoutTitle: 11 //affiche un élément du DOM sans le titre
    };



    ///Contenu Eudo de la modale (Wizard, Liste, sélection de champ...)
    var oldMouseMove;
    var oldMouseUp;
    var CONST_TOOLTIP_ARROW_WIDTH = 12;

    this.marginDialogMaxSize = 10;

    //Attribut permettant d'identifer un objet comme étant une fenêtre modal
    this.isModalDialog = true;

    //Url de la page à appeler   
    this.url = url;

    this.bIsAvancedFormular = bIsAvancedFormular;

    this.type = type; // type "générique" de fenêtre modale. cf enum ModalType

    this.isSpecif = false;

    this.EudoType = ModalEudoType.UNSPECIFIED.toString();
    this.bBtnAdvanced = false;    //Indique si la popup est avancée est a un espace plus grand à droite au niveau des boutons
    this.Handle = handle;

    this.tabScript = new Array();
    this.tabCss = new Array();

    this.Buttons = new Array();

    this.NoScrollOnMainDiv = false;

    // Sur tous les supports sauf les tablettes, on masque la fenêtre en détruisant l'objet eModalDialog du DOM
    // pour libérer la mémoire
    // On appelle isTablet depuis plusieurs contextes car eTools n'est pas forcément présent sur toutes les modal dialogs (notamment les eConfirm)
    this.bIsTablet = false;
    try {
        if (typeof (isTablet) == 'function')
            this.bIsTablet = isTablet();
        else if (typeof (top.isTablet) == 'function')
            this.bIsTablet = top.isTablet();
    }
    catch (e) {

    }

    // Taille de la fenêtre spécifiée par l'appelant
    // Convertie en String pour les vérifications et calculs automatiques ci-dessous
    if (typeof width == "undefined" || width == null || typeof height == "undefined" || height == null) {
        this.width = (1024 - 40) + '';
        this.height = (768 - 40) + '';
    }
    else {
        this.width = width + '';
        this.height = height + '';
    }
    this.trace("Taille demandée pour la fenêtre : " + this.width + " x " + this.height);


    // Taille de la page
    this.initSize = function () {
        this.docWidth = top.getWindowWH()[0];
        this.docHeight = top.getWindowWH()[1];

        // #58 123 : Calcul de la taille totale du document, scroll compris
        this.maxDocWidth = top.getMaxDocWidth();
        this.maxDocHeight = top.getMaxDocHeight();

        /*
        //GCH #33929 : il faut en plus ajouter le scroll pour que lorsque l'on est en zoom sur la tablette, il soit compté les dimension de la fenêtre complète
        /*
        #58 123 : la valeur de scroll est désormais utilisée pour positionner le coin haut gauche de la fenêtre (cf. pLeft et pTop dans show())
        De plus, à ce jour (10/2017), il semblerait que les navigateurs aient uniformisé ce comportement, que ce soit sur PC et tablettes
        (les valeurs de scroll sont à 0, même si on zoome - testé sur une Galaxy Tab 4 avec Android 5.0.2 et Chrome)
        Ce correctif #33 929 ne semble donc plus nécessaire, y compris dans le contexte décrit sur la demande (affichage des détails d'une fiche Planning lorsqu'on clique dessus en mode graphique)
        Les popups semblent bien positionnées.
        */

        /*
        if (this.bIsTablet) {
        */
        var scrlPos = [0, 0];
        var topScrlPos = [0, 0];
        if (typeof (getScrollXY) == 'function')
            scrlPos = getScrollXY();
        if (typeof (top.getScrollXY) == 'function')
            topScrlPos = top.getScrollXY();
        /*
            this.docWidth = this.docWidth + scrlPos[0];
            this.docHeight = this.docHeight + scrlPos[1];
        }
        */

        // En revanche, on conserve une partie du correctif pour calculer et exposer les valeurs de scroll
        this.scrollWidth = scrlPos[0];
        this.scrollHeight = scrlPos[1];
        this.topScrollWidth = topScrlPos[0];
        this.topScrollHeight = topScrlPos[1];

        this.trace("Largeur et hauteur de document racine visibles (hors scroll) : " + getNumber(this.docWidth) + ", " + getNumber(this.docHeight));
        this.trace("Largeur et hauteur de document racine maximales (scroll compris) : " + getNumber(this.maxDocWidth) + ", " + getNumber(this.maxDocHeight));
        this.trace("Valeurs de scroll du document racine (horizontal, vertical) : " + getNumber(this.topScrollWidth) + ", " + getNumber(this.topScrollHeight));
    };
    this.initSize();

    this.tabScript = new Array();
    this.tabCss = new Array();

    this.Buttons = new Array();

    // Taille de la fenêtre recalculée en valeur absolue si %
    this.initAbsSize = function () {
        if (this.width.indexOf("%") != -1) {
            var widthPercentValue = getNumber(this.width);
            var containerWidth = getNumber(this.docWidth);
            this.trace("Largeur de la fenêtre en pourcentage : " + widthPercentValue + " (conteneur : " + containerWidth + ")");
            if (widthPercentValue > 0 && containerWidth > 0)
                this.absWidth = containerWidth * (widthPercentValue / 100);
            else
                this.absWidth = getNumber(this.width);
        }
        else {
            // On convertit la chaine de caractères en nombre pour que les calculs suivants restent corrects
            this.absWidth = Math.round(getNumber(this.width));
        }
        this.trace("Largeur absolue recalculée : " + this.absWidth);

        if (this.height.indexOf("%") != -1) {
            var heightPercentValue = getNumber(this.height);
            var containerHeight = getNumber(this.docHeight);
            this.trace("Hauteur de la fenêtre en pourcentage : " + heightPercentValue + " (conteneur : " + containerHeight + ")");
            if (heightPercentValue > 0 && containerHeight > 0)
                this.absHeight = containerHeight * (heightPercentValue / 100);
            else
                this.absHeight = getNumber(this.height);
        }
        else {
            // On convertit la chaine de caractères en nombre pour que les calculs suivants restent corrects
            this.absHeight = Math.round(getNumber(this.height));
        }
        this.trace("Hauteur absolue recalculée : " + this.absHeight);

        // Ajustement de la taille de la fenêtre si elle est trop grande par rapport à la taille de l'écran/du document
        if (this.absWidth > getNumber(this.docWidth)) {
            this.trace("Fenêtre trop large par rapport à l'espace disponible à l'écran (demandé : " + this.width + " (" + this.absWidth + "), disponible : " + getNumber(this.docWidth) + ")");
            this.absWidth = getNumber(this.docWidth) - 20;
            this.trace("Nouvelle largeur : " + this.absWidth);
        }
        if (this.absHeight > getNumber(this.docHeight)) {
            this.trace("Fenêtre trop haute par rapport à l'espace disponible à l'écran (demandé : " + this.height + " (" + this.absHeight + "), disponible : " + getNumber(this.docHeight) + ")");
            this.absHeight = getNumber(this.docHeight) - 20;
            this.trace("Nouvelle hauteur : " + this.absHeight);
        }
    };
    this.initAbsSize();

    var containerDiv = null;
    var backgroundDiv = null;       //Div de background
    var mainDiv = null;             //DivPrincipal
    var tdMsgDetails = null;
    var mainDivLibelleToolTip = null    //TOOLTIP DivPrincipal
    var titleDiv = null;            //DivPrincipal
    var buttonsDiv = null;          //DivPrincipal
    var toolbarDivLeft = null;
    var toolbarDivRight = null;
    var uId = randomID(10);

    activeModalId = uId;


    this.UID = uId;

    this.iframeId = "frm_" + uId;   // MAB - pointeur vers l'iframe




    if (top) {
        top.window['_mdName'][this.iframeId] = this;
    }


    var parameters = new Array(); 	//Paramètres (tableau de updaterParameter)
    var msg = "";                   //Message à afficher
    var msgDetails = "";            //Message détaillé à afficher
    var msgType = 0;                //Type de l'icone à afficher (critical, info....)
    var ednuPopup = null;
    this.arrFct = new Array();
    this.title = title; //  + "  [" + uId + "]";

    this.eltToDisp;
    this.sOldHTML;

    //Pour la génération de case à cocher/bouton radio dynamique :
    //  - Si à true => plusieurs réponses possible donc cases à cocher
    //  - Si à false => réponse unique donc bouton radio
    this.isMultiSelectList = false;

    this.toString = function () {
        return 'eModalDialog';
    }

    var divResize = null;

    var promptLabel = "";
    var promptDefValue = "";

    var tdButtons = null; // right
    var tdButtonsLeft = null;
    var tdButtonsMid = null;
    var ulToolbarLeft = null;
    var ulToolbarRight = null;

    //Avec ou sans le div des boutons
    this.noButtons = false;
    //Avec ou sans le div du title
    this.noTitle = true;
    //Avec ou sans le div de la barre d'outils
    this.noToolbar = true;
    // Masquer le bouton "Agrandir"
    this.hideMaximizeButton = false;
    // Masquer le bouton "Fermer"
    this.hideCloseButton = false;
    // Fermer la fenêtre lors de l'appui sur la touche Echap
    // 0 ou false ou undefined : désactivé
    // 1 : activé, avec message de confirmation avant fermeture
    // 2 : activé, sans message de confirmation avant fermeture
    this.closeOnEscKey = 1;
    // Variable à mettre à true lorsque l'utilisateur a effectué des modifications sur la fenêtre
    this.unsavedChanges = false;
    // cette variable sera mise à true lorsque la popup de confirmation sera affichée, afin de ne pas pouvoir être affichée plusieurs fois lors
    // d'appuis successifs sur la touche Echap
    this.confirmChangesModalDisplayed = false;

    this.onIframeLoadComplete = function () { }; //par défaut, fonction vide

    // Arguments à passer
    this.inputArgs = new Array();

    this.addArg = function (val) {
        this.inputArgs.push(val);
    };

    // Gestion de la taille de la popup (maximiser)
    this.sizeStatus = "";
    this.initialTitleWidth = 0;
    this.initialContainerTop = 0;
    this.initialContainerLeft = 0;
    this.initialContainerWidth = 0;
    this.initialContainerHeight = 0;
    this.initialMainWidth = 0;
    this.initialMainHeight = 0;
    this.initialButtonsWidth = 0;
    this.initialButtonsHeight = 0;
    this.initialContainerMargin = 0;

    this.trace("Calcul du z-index...");
    this.level = 100;// z-index de base = 100
    this.level = GetMaxZIndex(top.document, this.level, !bDontIgnoreSetWait);
    // On se place à un z-index au-dessus du plus haut élément positionné sur la fenêtre parente
    this.level++;
    this.trace("z-index retenu pour la modal dialog : " + this.level);

    //Classe Css  / Icone de la fenêtre
    this.IconClass = "";
    //Classe Css  : message
    this.textClass = "";

    // Stockage des informations concernant le navigateur sur l'objet
    this.browser = new getBrowser();

    this.setMessage = function (_msg, _msgDetails, _msgType, bAllowHtml) {

        if (!bAllowHtml || typeof (bAllowHtml) == "undefined" || bAllowHtml == null) {
            _msg = encodeHTMLEntities(_msg);
            _msgDetails = encodeHTMLEntities(_msgDetails);
        }

        msg = this.processEudoTag(_msg + "");
        msgDetails = this.processEudoTag(_msgDetails + "");
        msgType = _msgType;


    };



    this.processEudoTag = function (value) {

        value += "";

        //remplace pseudo-tag
        // tag [XURL url='monurl']monlien[/XURL]
        //#42 597 Permettre les  simples cotes dans l'URL
        value = value.replace(/\[XURL url='(.+?)'\](.+?)\[\/XURL\]/g, "<a target=\"_blank\" href=\"$1\">$2</a>");

        //Tag [[BR]]
        value = value.replace(/\[\[BR\]\]/g, "<br/>");

        value = value.replace(/\[\[SPAN onclick='(.+?)' class='(.+?)'\]\]/g, "<span onclick=\"$1\" class=\"$2\">");
        value = value.replace(/\[\[\/SPAN\]\]/g, "</span>");

        value = value.replace(/\[\[UL\]\]/g, "<ul>");
        value = value.replace(/\[\[\/UL\]\]/g, "</ul>");

        value = value.replace(/\[\[LI\]\]/g, "<li>");
        value = value.replace(/\[\[\/LI\]\]/g, "</li>");


        //Tag [[SPACE]]
        value = value.replace(/\[\[SPACE\]\]/g, "&nbsp;");

        // Commentaire html
        value = value.replace(/&lt;!--/gi, "<!--");
        value = value.replace(/--&gt;/gi, "-->");

        //permet de restaurer certains <br>
        return value.replace(/\n/g, "<br/>").replace(/&#10;/g, "<br/>").replace(/&lt;br\s*\/&gt;/gi, "<br/>").replace(/&lt;br\s*&gt;/gi, "<br/>");
    };

    this.setElement = function (oElt) {
        this.eltToDisp = oElt;
        this.sOldHTML = oElt.outerHTML;

    }

    this.TargetObject = null;

    if (this.url != null) {
        //Appel Ajax
        ednuPopup = new eUpdater(this.url, 1);
    };

    this.ErrorCallBack = null;
    this._callOnCancel = '';
    this.CallOnOk = '';
    this._argsOnOk = '';
    /*
    buttonId : si "cancel" execute la fonction définit sur le bouton dès que l'on ferme avec la croix
    si "ok" execute la fonction définit sur le bouton sur la variable this.CallOnOk.
    */

    this.addButton = function (btnLabel, actionFunction, cssName, args, buttonId, position) {

        /*
        <div class="button-green" id="buttonId" >
        <div class="button-green-left"></div>
        <div class="button-green-mid">un libéllé très très très long</div>
        <div class="button-green-right"></div>
        </div>
        */

        if (this.noButtons == true)
            return;

        if (position != 'left') {
            if (position != 'mid') {
                position = 'right';
            }
        }

        var btn = top.document.createElement('div');
        //Ajout d'un flag sur le boutton pour identification
        setAttributeValue(btn, "ednmodalBtn", 1);

        btn.className = cssName;

        if (position == 'left') {
            btn.className += ' button-position-left';
        }
        else if (position == 'mid') {
            btn.className += ' button-position-mid'

        }

        btn.style.align = position;

        if (typeof buttonId != "undefined")
            btn.id = buttonId;


        var btnLeft = top.document.createElement('div');

        btnLeft.className = cssName + "-left";
        btn.appendChild(btnLeft);

        var btnMid = top.document.createElement('div');
        btnMid.className = cssName + "-mid";
        btnMid.innerHTML = btnLabel;

        if (typeof buttonId != "undefined")
            btnMid.id = buttonId + "-mid";

        btn.appendChild(btnMid);

        var btnRight = top.document.createElement('div');
        btnRight.className = cssName + "-right";
        btn.appendChild(btnRight);


        if (typeof (actionFunction) == "function") {
            var functionName = getFunctionName(actionFunction);

            if (typeof (functionName) == 'undefined' || functionName == '') {
                // Anonymous function
                btn.onclick = actionFunction;
            }
            else {
                btn.onclick = new Function(functionName + "(\"" + (args && (typeof args === 'string' || args instanceof String) ? args.replace(/\"/g, '\\\"') : args) + "\",'" + uId + "')");
            }
        }
        else if (actionFunction != null)
            btn.onclick = new Function(actionFunction + "(\"" + (args && (typeof args === 'string' || args instanceof String) ? args.replace(/\"/g, '\\\"') : args) + "\",'" + uId + "')");

        this.Buttons.push(btn);

        if (position == 'left') {
            tdButtonsLeft.appendChild(btn); //En premier pour qu'il apparraisse en dernier
        }
        else if (position == 'mid') {
            tdButtonsMid.appendChild(btn); //button au Millieu 
        }
        else {
            tdButtons.appendChild(btn); //En premier pour qu'il apparraisse en dernier
            //tdButtons.appendChild(emptyBtn);  //GCH pour PNO : retiré car sera géré dans la css des bouton direct
        }
        // NBA 03-12-2012
        // Si le l'id du bouton est "cancel" ou "cancel_btn" dans ce cas à la fermeture sur la croix en haut à droite on lance la fonction passée
        if (buttonId == "cancel" || buttonId == "cancel_btn") {
            if (typeof btn.onclick == "function" && btn.onclick != null) {
                if (!this.hideCloseButton) {
                    this.AddCancelMethode(btn.onclick);
                }
            }
            else {
                btn.onclick = this.hide;
            }
            this._callOnCancel = btn.onclick;
        }
        if (buttonId == "ok") {
            this.CallOnOk = btn.onclick;
            this._argsOnOk = args;
        }

    };

    ///summary
    // #60 193 - Si aucun élément n'a le focus à l'ouverture de la fenêtre, on le place sur le contrôle indiqué, ou sur un contrôle factice créé à la volée
    // Permet de câbler la fonction Echap sur la fenêtre active et non sur la fenêtre parente si la fenêtre active n'a pas de contrôle prenant automatiquement le focus
    // (ex : fenêtre de choix de fichier, eFieldFiles.aspx)
    ///summary
    this.setFocusOnWindowTimer = null;
    this.setFocusOnWindow = function (targetControl) {
        // Si le contenu de la fenêtre n'est pas encore complètement chargé, on diffère l'exécution de la fonction d'une seconde, jusqu'à ce qu'elle ait pu s'exécuter
        if (!this.getIframe() || !this.getIframe().document || !this.getIframe().document.body) {
            this.setFocusOnWindowTimer = setTimeout(function () { that.setFocusOnWindow(targetControl); }, 1000);
        }
        else {
            clearTimeout(this.setFocusOnWindowTimer);
            // Si le contrôle comportant actuellement le focus n'est pas un élement focusable (souvent <body>), on en crée un, ou on utilise celui indiqué en paramètre
            if (!isFocusableElement(this.getIframe().document.activeElement)) {
                // Si un contrôle à focuser a été précisé, on l'utilise
                if (targetControl && isFocusableElement(targetControl))
                    targetControl.focus();
                // Si le faux contrôle de prise de focus a déjà été créé lors d'un précédent appel à la fonction, on le réutilise
                else if (this.getIframe().document.getElementById("fakeFocusElement_" + this.UID))
                    this.getIframe().document.getElementById("fakeFocusElement_" + this.UID).focus();
                // Sinon, on le crée
                else {
                    // Création d'un élément de saisie factice, positionné hors du champ de vision, pour que les raccourcis clavier s'exécutent sur la fenêtre active,
                    // et non la précédente. Il est ajouté dans un conteneur déporté pour ne pas être cliquable
                    // Il ne doit pas être mis en display: none, visibility: hidden ou disabled car les navigateurs modernes refuseront alors de lui donner le focus
                    var fakeFocusElementContainer = this.getIframe().document.createElement("div");
                    var fakeFocusElement = this.getIframe().document.createElement("input");
                    // -------------------
                    // Conteneur
                    fakeFocusElementContainer.id = "fakeFocusElementContainer_" + this.UID;
                    fakeFocusElementContainer.style.position = "absolute"; // pas de dépendance aux autres contrôles de la page
                    // On positionne malgré tout le contrôle au sein de la fenêtre active pour se souvenir qu'il y est rattaché, et ne pas se heurter à une future
                    // éventuelle restriction navigateur sur les éléments positionnés en négatif
                    fakeFocusElementContainer.style.left = "0px";
                    fakeFocusElementContainer.style.top = "0px";
                    // Les attributs ci-dessous le rendent non visible, mais non masqué pour autant. Empêche de cliquer dans le champ de saisie, mais autorise le focus
                    fakeFocusElementContainer.style.width = "0px";
                    fakeFocusElementContainer.style.height = "0px";
                    fakeFocusElementContainer.style.overflow = "hidden";
                    fakeFocusElementContainer.style.opacity = "0";
                    // -------------------
                    // Champ de saisie
                    fakeFocusElement.id = "fakeFocusElement_" + this.UID;
                    fakeFocusElement.attributes["tabindex"] = "0"; // peut être nécessaire pour autoriser le focus
                    // On le rend en readonly pour empêcher toute saisie réelle dedans
                    // Attention : il ne faut pas le mettre en disabled, car le navigateur refuse alors le focus
                    fakeFocusElement.readOnly = true;
                    // le contrôle doit rester dans la zone de son conteneur positionné en absolute pour ne pas avoir d'incidence sur la mise en page
                    fakeFocusElement.style.position = "relative";
                    // On positionne malgré tout le contrôle au sein de la fenêtre active pour se souvenir qu'il y est rattaché, et ne pas se heurter à une future
                    // éventuelle restriction navigateur sur les éléments positionnés en négatif
                    fakeFocusElement.style.left = "0px";
                    fakeFocusElement.style.top = "0px";
                    // La plupart des navigateurs traceront malgré tout un contrôle visible de quelques pixels
                    fakeFocusElement.style.width = "0px";
                    fakeFocusElement.style.height = "0px";
                    // L'attribut opacity rendra toutefois le contrôle non visible à coup sûr, mais non masqué pour autant
                    fakeFocusElement.style.opacity = "0";
                    // -------------------
                    // Rattachement au DOM
                    fakeFocusElementContainer.appendChild(fakeFocusElement);
                    this.getIframe().document.body.appendChild(fakeFocusElementContainer);
                    // -------------------
                    // Et enfin, focus sur l'élément de saisie
                    fakeFocusElement.focus();
                }
            }
        }
    };


    this.switchButtonDisplay = function (handle, bHide) {

        if (typeof bHide == "unedefined")
            bHide = false;

        var myButt = this.Buttons.filter(function (btn) {
            return btn.id == handle
        })

        if (myButt.length > 0) {
            myButt[0].style.display = bHide ? "none" : "";
        }



    }

    ///summary
    ///Cache les boutons sur la modal afin qu'on ne les vois pas au premier chargement et qu'on affiche uniquement le nécessaire par la suite
    /// TODO : Déplacer cette fonction dans eModal.js
    ///summary
    this.hideButtons = function () {
        try {

            var buttonModal = window.parent.document.getElementById("ButtonModal_" + this.UID);
            ///On parcours les div du conteneur des boutons, et pour chaque div ayant un ID de type ******_btn , donc un bouton.
            ///Et on les masque.
            var buttons = buttonModal.getElementsByTagName("div");
            for (iBtn = 0; iBtn < buttons.length; iBtn++) {
                if (buttons[iBtn].id.indexOf("_btn") > 0 && buttons[iBtn].id.indexOf("_btn") + 4 == buttons[iBtn].id.length)
                    buttons[iBtn].style.display = "none";
            }

        } catch (ex) { }
    };


    this.romovebtnClose = function () {
        try {
            var buttonModal = top.document.getElementById("ImgControlBoxClose_" + this.UID);
            buttonModal.style.display = "none";


        } catch (ex) { }
    };


    this.addButtons = function (aBtnsArray) {
        if (typeof (aBtnsArray) != "object")
            return;

        for (var i = 0; i < aBtnsArray.length; i++) {
            var btn = aBtnsArray[i];
            this.addButton(btn.label, btn.fctAction, btn.css);
        }
    }

    ///affiche ou masque les boutons de la modal
    this.switchButtonsDisplay = function (bShow) {

        if (buttonsDiv) {
            var buttons = buttonsDiv.querySelectorAll("[ednmodalbtn='1']");
            for (iBtn = 0; iBtn < buttons.length; iBtn++) {
                if (bShow)
                    buttons[iBtn].style.display = "";
                else
                    buttons[iBtn].style.display = "none";
            }
        }
    };



    //
    this.addScript = function (scriptName) {
        if (this.tabScript.indexOf(scriptName))
            this.tabScript.push(scriptName);
    };

    this.addCss = function (cssName) {
        if (this.tabCss.indexOf(cssName))
            this.tabCss.push(cssName);
    };


    //Permet de définir la méthode à appeler au click sur le bouton de croix (qui est suivit par la fermeture réeel
    this.AddCancelMethode = function (callOnCancel) {
        this._callOnCancel = callOnCancel;
        top.document.getElementById("ImgControlBoxClose_" + uId).onclick = callOnCancel;
    }

    this.addToolbarButton = function (strBtnId, strBtnLabel, strBtnToolTip, strMainCSSName, strLabelCSSName, strImgCSSName, strLinkCSSName, bCreateLabel, bCreateImg, strPosition, oActionFunction, args) {

        if (this.noToolbar == true)
            return;

        var btn = top.document.createElement('li');
        btn.style.display = "none";
        btn.style.visibility = "hidden";
        btn.className = strMainCSSName;
        btn.id = strBtnId;
        btn.title = strBtnToolTip;
        var onClickFunction;
        if (typeof (oActionFunction) == "function") {
            var functionName = getFunctionName(oActionFunction);
            if (typeof (functionName) == 'undefined' || functionName == '')      // Anonymous function
                onClickFunction = oActionFunction;
            else
                onClickFunction = new Function(getFunctionName(oActionFunction) + "(\"" + args + "\",'" + uId + "')");
        }
        else
            onClickFunction = new Function(oActionFunction + "(\"" + args + "\",'" + uId + "')");

        var oTargetToolbar = ulToolbarRight;
        if (strPosition == 'left')
            oTargetToolbar = ulToolbarLeft;
        oTargetToolbar.appendChild(btn);

        var strToolTip = strBtnToolTip;
        if (strToolTip == null || strToolTip == '')
            strToolTip = strBtnLabel;

        // Si les classes spécifiques des éléments enfants ne sont pas précisées, on leur applique la classe "principale"
        // sauf si les classes spécifiques sont précisées à vide ("")
        if (strLinkCSSName == null)
            strLinkCSSName = strMainCSSName;
        if (strImgCSSName == null)
            strImgCSSName = strMainCSSName;
        if (strLabelCSSName == null)
            strLabelCSSName = strMainCSSName;

        // Si on crée un bouton avec libellé, on crée une structure lien > image > texte pour que l'ensemble réagisse à l'interaction avec la souris (surbrillance, clic...)
        // ou, s'il n'y a pas de lien/fonction a exécuter, simplement un libellé
        var oParentElt = btn;
        if (typeof (oActionFunction) == 'function' && bCreateLabel && strBtnLabel != '') {
            var btnLabelLink = top.document.createElement("a");
            oParentElt = btnLabelLink;
            btnLabelLink.className = strLinkCSSName;
            btnLabelLink.id = strBtnId + "_lbl";
            btnLabelLink.title = strToolTip;
            btnLabelLink.onclick = onClickFunction;
            btn.appendChild(btnLabelLink);
        }
        if (bCreateImg) {
            var btnGhostImage = top.document.createElement("span");
            btnGhostImage.className = strImgCSSName;
            btnGhostImage.id = strBtnId + "_img";
            btnGhostImage.title = strToolTip;
            oParentElt.appendChild(btnGhostImage);
        }
        if (bCreateLabel && strBtnLabel != '') {
            var btnLabelText = top.document.createElement("span");
            btnLabelText.className = strLabelCSSName;
            btnLabelText.id = strBtnId + "_text";
            btnLabelText.title = strToolTip;
            btnLabelText.innerHTML = strBtnLabel;
            oParentElt.appendChild(btnLabelText);
        }
        // Sinon, on affecte juste le clic sur l'élément li ; l'affichage de son image devra alors être géré sur sa classe CSS
        else {
            btn.onclick = onClickFunction;
        }

        return btn;
    };


    //retourne vrai si la modal est disponible.
    // si le top n'est plus dispo, c'est que le contexte à changer par rapport à sa disponibilité
    // notament si elle a été hide ou ouverte sur un autre signet
    // cf 37135
    this.IsAvailable = function () {
        return top != null;
    }



    ///Retourne l'objet Iframe incluant le tag contrairement a getIframe qui retourne le contentwindow
    this.getIframeTag = function () {


        if (top.document.getElementById("frm_" + uId))
            return top.document.getElementById("frm_" + uId);

        return null;
    };

    ///Retourne la window de l'iframe
    this.getIframe = function () {
        if (top.document.getElementById("frm_" + this.UID))
            return top.document.getElementById("frm_" + this.UID).contentWindow;

        return null;
    };

    this.getDivContainer = function () {
        if (top.document.getElementById("ContainerModal_" + uId))
            return top.document.getElementById("ContainerModal_" + uId);

        return null;
    }

    this.getDivButton = function () {
        if (top.document.getElementById("ButtonModal_" + uId))
            return top.document.getElementById("ButtonModal_" + uId);

        return null;
    }

    //  Ajoute un bouton
    //  btnLabel : label du bouton
    //  actionFunction : Objet function sur le onclick
    //  cssName : CSS syr le bouton
    //  buttonId : si "cancel" execute la fonction définit sur le bouton dès que l'on ferme avec la croix
    //      si "ok" execute la fonction définit sur le bouton sur la variable this.CallOnOk.

    this.addButtonFct = function (btnLabel, actionFunction, cssName, buttonId) {



        if (this.noButtons == true)
            return;

        var btn = top.document.createElement('div');

        //Ajout d'un flag sur le boutton pour identification
        setAttributeValue(btn, "ednmodalBtn", 1);

        btn.className = cssName;
        btn.style.align = "right";



        if (typeof buttonId != "undefined") {
            btn.id = buttonId;
            btn.handle = buttonId;
        }

        var btnLeft = top.document.createElement('div');
        btnLeft.className = cssName + "-left";
        btn.appendChild(btnLeft);

        var btnMid = top.document.createElement('div');
        btnMid.className = cssName + "-mid";
        btnMid.innerHTML = btnLabel;
        if (typeof buttonId != "undefined")
            btnMid.id = buttonId + "-mid";
        btn.appendChild(btnMid);
        var btnRight = top.document.createElement('div');
        btnRight.className = cssName + "-right";
        btn.appendChild(btnRight);

        if (typeof (actionFunction) == "function")
            btn.onclick = actionFunction;

        this.Buttons.push(btn);

        tdButtons.appendChild(btn); //En premier pour qu'il apparraisse en dernier

        // Si le l'id du bouton est "cancel" dans ce cas à la fermeture sur la croix en haut à droite on lance la fonction passée
        if (buttonId == "cancel") {
            this._callOnCancel = btn.onclick;
            if (!this.hideCloseButton) {
                this.AddCancelMethode(this._callOnCancel);
            }
        }
    };

    //Ajoute une fonction à la fenêtre modale
    this.addFunction = function (sFctName, fct) {
        if (typeof (that.arrFct[fct]) == "undefined" && typeof (fct) == "function") {
            that.arrFct[sFctName] = fct;
        }
    }

    // Commenté pour des raisons d'effets indésirables sur la fermeture des pop-up des champs obligatoires.
    //this.hide = function (afterHide) {
    //    containerDiv.className = "containerModal closeTransition";

    //    setTimeout(function () { that.hideMe(afterHide); }, 200);

    //};


    ///Fait progressivement disparaitre la modal en 10 étapes (en nTime ms )
    this.fade = function (nTime) {

        var element = that.getDivContainer();

        var nOp = 1;

        if (nTime == 0)
            that.hide();

        if (nTime < 50)
            nTime = 50;

        var nInter = nTime / 10;

        var step = 0.1;

        var timer = setInterval(function () {
            try {

                if (nOp <= step) {
                    clearInterval(timer);
                    that.hide();
                }

                element.style.opacity = nOp;
                nOp -= nOp * step;
            }
            catch (e) {
                clearInterval(timer);
                that.hide();
            }

        }, nInter);


    };



    this.hide = function (afterHide) {
        globalModalFile = false;
        if (that.debugMode)
            that.trace("Masquage de la fenêtre : " + uId);

        if (!that.bIsTablet && !(that.browser.isIE && that.browser.isIE8)) {
            try {


                that.removeAllChild(backgroundDiv);
            }
            catch (exp) {
                that.trace("ERREUR 1 lors du masquage : " + exp.Description);
            }

            try {
                if (containerDiv)
                    that.removeAllChild(containerDiv);
            }
            catch (exp) {
                that.trace("ERREUR 2 lors du masquage : " + exp.Description);
            }

            try {
                top.document.body.style.overflow = oldoverflow;
            }
            catch (exp) {
                that.trace("ERREUR 3 lors du masquage : " + exp.Description);
            }
            //Retire la modal du tableau global
            try {
                if (typeof that.Handle != "undefined" && top.window['_md'])
                    delete (top.window['_md'][that.Handle]);

                delete (top.window['_mdName'][that.iframeId]);
            }
            catch (exp) {
                that.trace("ERREUR 4 lors du masquage : Impossible de retirer le handle du tableau global : " + exp.Description);
            }
        }

        // Sur les tablettes, supprimer l'objet du DOM peut entraîner un crash brutal du navigateur
        // On se contente donc juste, sur ces supports, de masquer visuellement la modal dialog sans la détruire en mémoire
        // On altère leurs IDs afin que d'éventuels getElementById() sur les composantes de la fenêtre ne puissent plus pointer vers
        // des fenêtres masquées, mais uniquement vers les dernières fenêtres instanciées (qui auront l'ID initialement souhaité par le
        // développeur, jusqu'à ce que la fenêtre soit masquée à son tour)
        else {
            if (backgroundDiv) {
                backgroundDiv.style.display = 'none';
                backgroundDiv.id = that.iframeId + "_" + backgroundDiv.id;
                var backgroundDivChildren = backgroundDiv.querySelectorAll("*");
                for (var i = 0; i < backgroundDivChildren.length; i++) {
                    backgroundDivChildren[i].id = that.iframeId + "_" + backgroundDivChildren[i].id;
                }
            }
            if (containerDiv) {
                containerDiv.style.display = 'none';
                containerDiv.id = that.iframeId + "_" + containerDiv.id;
                var containerDivChildren = containerDiv.querySelectorAll("*");
                for (var i = 0; i < containerDivChildren.length; i++) {
                    containerDivChildren[i].id = that.iframeId + "_" + containerDivChildren[i].id;
                }
            }
            // On met à jour certaines propriétés pour que les scripts qui les utilisent considèrent que la modal dialog n'existe plus
            that.isModalDialog = false;
            //top.window['_md'][that.Handle] = null;
            if (typeof (that.getIframe) == 'function' && that.getIframe() && that.getIframe().document) {
                that.getIframe().document["_ismodal"] = 0;
                that.getIframe().document.parentModalDialog = null;
            }
        }
        if (typeof (afterHide) == "function")
            afterHide();

        if (typeof (that.onHideFunction) == "function")
            that.onHideFunction();

        if (typeof (top) != "undefined" && top != null && top.ModalDialogs)
            top.ModalDialogs[that.UID] = null;

        // #60 193 - A la fermeture (via Echap ou autre), on repositionne le focus sur une éventuelle fenêtre parente
        var parentModalDialog = that.getParentModalDialog();
        if (parentModalDialog)
            parentModalDialog.setFocusOnWindow();
    };


    this.removeAllChild = function (obj) {

        if (typeof obj == 'undefined' || obj == null)
            return;

        try {
            if (obj.childNodes) {
                var child = obj.childNodes
                var nb = child.length;
                for (var i = nb; i > 0; i--) {
                    try {
                        this.removeAllChild(child[i - 1]);
                    }
                    catch (ex) {
                        this.trace("ERREUR 6 lors du masquage : Impossible de retirer le handle du tableau global : " + ex.Description);
                    }
                }
            }
        }
        catch (exp) {
            this.trace("ERREUR 7 lors du masquage : Impossible de retirer le handle du tableau global : " + exp.Description);
        }
        try {
            var oParent = obj.parentElement;
            if (oParent && oParent.removeChild)
                oParent.removeChild(obj);
        }
        catch (exp) {
            this.trace("ERREUR 8 lors du masquage : Impossible de retirer le handle du tableau global : " + exp.Description);

        }
    };

    this.displayContent = function () {
        mainDiv.appendChild(this.eltToDisp);
    };
    var mainMsgDiv;
    this.createLocalMsgBox = function () {
        //Nom de la classe CSS
        var cssClass = "";
        var cssFont = "";


        if (msgType == null) {
            msgType = "0";
        }

        switch (msgType.toString()) {

            case MsgType.MSG_CRITICAL.toString():
                cssClass = "error";
                cssFont = "icon-times-circle ";
                break;
            case MsgType.MSG_QUESTION.toString():
                cssClass = "quote";
                cssFont = "icon-question-circle";
                break;
            case MsgType.MSG_EXCLAM.toString():
                cssClass = "warn";
                cssFont = "icon-exclamation-triangle";
                break;
            case MsgType.MSG_INFOS.toString():
                cssClass = "info";
                cssFont = "icon-info-circle";
                break;
            case MsgType.MSG_SUCCESS.toString():
                cssClass = "success";
                cssFont = "icon-check-circle";
                break;
        }
        mainMsgDiv = top.document.createElement("div");




        mainMsgDiv.className = "msg-container";
        mainDiv.appendChild(mainMsgDiv);
        setAttributeValue(containerDiv, "msgtype", msgType);

        mainMsgDiv.style.height = mainDiv.style.height;
        mainMsgDiv.style.width = containerDiv.style.width;
        var msgTable = top.document.createElement("table");
        mainMsgDiv.appendChild(msgTable)
        var tBody = top.document.createElement("tbody");
        msgTable.appendChild(tBody);

        var tr = top.document.createElement("tr");
        tBody.appendChild(tr);

        //td logo
        var tdLogo = top.document.createElement("td");
        tr.appendChild(tdLogo);
        tdLogo.className = "td-logo";
        tdLogo.rowSpan = 2;
        /*
        var imgLogo = top.document.createElement("img");
        tdLogo.appendChild(imgLogo);
        imgLogo.className = "logo-" + cssClass;
        imgLogo.src = "ghost.gif";
        */
        var spanLogo = top.document.createElement("span");
        tdLogo.appendChild(spanLogo);
        spanLogo.className = cssFont + " logo-" + cssClass;

        //td message
        var tdMsg = top.document.createElement("td");
        tdMsg.id = "msgbox_msg_" + uId;
        tdMsg.innerHTML = msg + "<br>";
        tdMsg.className = "text-alert-" + cssClass + " " + this.textClass;
        tr.appendChild(tdMsg);

        tr = top.document.createElement("tr");
        tBody.appendChild(tr);

        //td message détaillé
        tdMsgDetails = top.document.createElement("td");
        tdMsgDetails.id = "msgbox_msgdetails_" + uId;
        tdMsgDetails.className = "text-msg-" + cssClass;
        tdMsgDetails.innerHTML = msgDetails;
        tr.appendChild(tdMsgDetails);
        var oldoverflow = "";
    };

    this.setPrompt = function (msgPrompt, defValue) {
        promptLabel = msgPrompt;
        promptDefValue = defValue;
    };

    this.createPrompt = function () {
        //Nom de la classe CSS

        var block = document.createElement("div");
        block.className = "prompt-container";

        var line = document.createElement("div");
        block.appendChild(line);

        var label = document.createElement("div");
        label.innerHTML = promptLabel;
        label.className = "promptLabel";
        line.appendChild(label);

        var textbox = document.createElement("input");
        textbox.setAttribute("id", "InputPrompt_" + uId);
        textbox.className = "promptText";
        textbox.value = promptDefValue;

        line.appendChild(textbox);

        mainDiv.appendChild(block);


    };


    this.createControl = function (sType, sName, sId, sValue, sLabel, sCSS, oDivGbl, bCkecked) {

        //KHA - pas fait de res car ces messages ne sont pas censés apparaitre à l'utilisateur
        //si les appels sont fait correctement
        if (sType.indexOf("'") > -1 || sType.indexOf("\\") > -1 || sType.indexOf(" ") > -1) {
            eAlert(0, "Controle Invalide", "Paramètre \"type\"");
            return;
        }
        if (sName.indexOf("'") > -1 || sName.indexOf("\\") > -1 || sName.indexOf(" ") > -1) {
            eAlert(0, "Controle Invalide", "Paramètre \"name\"");
            return;
        }
        if (sId.indexOf("'") > -1 || sId.indexOf("\\") > -1 || sId.indexOf(" ") > -1) {
            eAlert(0, "Controle Invalide", "Paramètre \"id\"");
            return;
        }
        if (sCSS.indexOf("'") > -1 || sCSS.indexOf("\\") > -1 || sCSS.indexOf(" ") > -1) {
            eAlert(0, "Controle Invalide", "Paramètre \"classe\"");
            return;
        }


        var div = this.createDiv(sCSS, oDivGbl);

        if (sType == "checkbox") {
            AddEudoCheckBox(top.document, div, bCkecked, sId, sLabel)
        }
        else if (sType == "literal") {
            div.innerHTML += sLabel;
        }
        else {

            var sValue = sValue.replace(/'/g, "\'").replace(/\"/g, "\\\"").replace(/\\/g, "\\\\");

            var strControl = "<input type='" + sType + "' name='" + sName + "' id='" + sId + "' value='" + sValue + "'" + (sType == "radio" && bCkecked ? " checked" : "") + ">";
            var strLabelCtrl = "<label for='" + sId + "'>" + sLabel + "</label>";

            if (sType == "radio")
                div.innerHTML += strControl + strLabelCtrl;
            else
                div.innerHTML += strLabelCtrl + strControl;
        }
    };

    this.createDiv = function (sCSS, oDiv, sLabel) {
        var divContainer = mainDiv;
        if (oDiv)
            divContainer = oDiv;
        else if (mainMsgDiv)
            divContainer = mainMsgDiv;

        var div = top.document.createElement("div");
        divContainer.appendChild(div);
        div.className = sCSS;

        if (sLabel && sLabel != "")
            div.innerHTML = sLabel;

        return div;
    }

    var docEventKey = null; //Document auquel sont attachés les événements

    this.createIframe = function (resp) {
        //Création de l'iframe
        var tmpFrm = top.document.createElement('iframe');
        tmpFrm.id = "frm_" + uId;
        tmpFrm.name = "frm_" + uId;

        tmpFrm.style.left = 0 + "px";
        tmpFrm.style.top = 0 + "px";
        tmpFrm.style.width = "100%";
        tmpFrm.style.height = "98%"; // NBA modif du 27-04-2012 pb affichage d'une scrollbar verticale
        tmpFrm.style.border = "0px";
        tmpFrm.style.margin = "0px";
        // 41590 CRU : Pouvoir définir l'attribut "scrolling" de l'iframe
        // Par défaut à Oui
        var scrolling = "yes";
        if (this.getParam("iframeScrolling") != "" && this.getParam("iframeScrolling") == "no") {
            scrolling = "no";
        }
        tmpFrm.scrolling = scrolling;
        tmpFrm.frameBorder = "0";

        if (mainDivLibelleToolTip != null)
            mainDivLibelleToolTip.appendChild(tmpFrm);
        else
            mainDiv.appendChild(tmpFrm);


        if (this.isStatic) {
            tmpFrm.src = this.url
        }

        if ((tmpFrm.contentWindow) && (tmpFrm.contentWindow.document)) {

            tmpFrm.contentWindow.document.open();
            tmpFrm.contentWindow.document.write(resp);

            // MCR SPH 39993, faire un set de la variable : _ismodal apres le document.write() sinon l affectation n est pas faite avec IE !!
            tmpFrm.contentWindow.document["_ismodal"] = 1;

            tmpFrm.contentWindow.document.parentModalDialog = that;

            /*On attache la fonctions de touche native à l'evennement d'appuie sur une touche*/
            docEventKey = tmpFrm.contentWindow.document;    //On sauvegarde le document dont l'événements a été setté afin de le supprimmer ensuite
            setEventListener(tmpFrm.contentWindow.document, "keydown", KeyPressNativeFunction, false);
            /*********************************************************************************/
            setEventListener(tmpFrm, "load", that.DoFctLoadBody, false);

            //Attache les fonctions custom à l'objet document
            for (var fct in that.arrFct) {
                if (typeof (tmpFrm.contentWindow.document[fct]) == "undefined" && typeof (that.arrFct[fct]) == "function") {

                    //alert(fct + ' ' +that.arrFct[fct]);
                    tmpFrm.contentWindow.document[fct] = that.arrFct[fct];
                }
            }
            // Compatibilité IE : edge
            setEdgeCompatibility(tmpFrm.contentWindow.document);
            //Ajout des Css et Script
            if (that.tabCss.length > 0) {
                for (var ii = 0; ii < that.tabCss.length; ii++) {
                    addCss(that.tabCss[ii], "MODAL", tmpFrm.contentWindow.document);
                }
            }

            addScripts(that.tabScript, "MODAL", that.DoFctLoadScript, tmpFrm.contentWindow.document);

            addScript("../IRISBlack/Front/Scripts/Libraries/vue/vue", "MODAL", that.DoFctLoadScript, tmpFrm.contentWindow.document);
            addScript("../IRISBlack/Front/Scripts/Libraries/vuex/vuex", "MODAL", that.DoFctLoadScript, tmpFrm.contentWindow.document);
            addScript("../IRISBlack/Front/Scripts/Libraries/axios/axios", "MODAL", that.DoFctLoadScript, tmpFrm.contentWindow.document);
            addScript("../IRISBlack/Front/scripts/Libraries/vuetify/vuetify.min", "MODAL", that.DoFctLoadScript, tmpFrm.contentWindow.document);
            addScript("../IRISBlack/Front/scripts/Libraries/eudofront/eudoFront.umd", "MODAL", that.DoFctLoadScript, tmpFrm.contentWindow.document);

            tmpFrm.contentWindow.document.close();

        }
    };
    //Permet d'appeler la fonction définit dans onIframeLoadComplete
    //Elle permet de lancer la fonction qu'après le JS ET BODY chargée
    this.bScriptOk = false;
    this.bBodyOk = false;
    this.DoFctLoadBody = function () {
        that.bBodyOk = true;
        try {

            var oFrame = that.getIframeTag();
            if (!that.isSpecif)
                top.eTools.UpdateDocCss(oFrame.contentWindow.document);
        }
        catch (e) {

        }

        that.DoFctLoad();
    };
    this.DoFctLoadScript = function () {
        that.bScriptOk = true;
        that.DoFctLoad();
        /*
                var browser = new getBrowser();
                if (browser.isIE && browser.isIE8) {
                    var aIcon = document.querySelectorAll('[class^="icon-"]');
                    //alert(aIcon.length);
                    for (var nIcmptIds = 0; aIcon < aIcon.length; nIcmptIds++) {
                        var myIcon = aIcon[nIcmptIds];
                        addClass(myIcon, 'ie8fonts');
                    }
                    //alert("icon reloadé");
                }
        */
    };
    this.DoFctLoad = function () {

        // #60 193 - Si aucun élément du <body> ne prend le focus automatiquement à l'ouverture de la fenêtre, on simule le focus sur un élément donné, ou sur un faux élément
        // Permet de câbler la fonction Echap sur la fenêtre active et non sur la fenêtre parente si la fenêtre active n'a pas de contrôle prenant automatiquement le focus
        // (ex : fenêtre de choix de fichier, eFieldFiles.aspx)
        this.setFocusOnWindow();

        if (that.bScriptOk && that.bBodyOk && typeof (that.onIframeLoadComplete) == "function") {

            that.onIframeLoadComplete();

        };

        //top.document.getElementById("ContainerModal_" + uId).className = "ContainerModal";
    };

    //Fonction interne à la modale appelée lorsque l'on presse une touche
    function KeyPressNativeFunction(e) {
        var oFrm = that.getIframe();
        if (!oFrm)
            return;
        if (!e)
            var e = oFrm.event;
        if (!e)
            return;
        //Touche échap : on quitte ferme modale :
        if (e.keyCode == 27) {
            if (that.closeOnEscKey > 0) {
                var oModalClose = function () {
                    if (typeof (docEventKey) !== "undefined")
                        unsetEventListener(docEventKey, "keydown", KeyPressNativeFunction, false);

                    cancelAndDeletePJ(oFrm);
                    cancelAndDeleteImages(oFrm);
                    that.hide();

                    return false;
                };

                // Avec confirmation
                if (that.unsavedChanges && that.closeOnEscKey == 1) {
                    if (!that.confirmChangesModalDisplayed) {
                        // On déclenche la sortie de curseur sur les champs de saisie pour éviter les conflits liés au setWait (quitte à enregistrer les modifications effectuées)
                        if (document.activeElement)
                            document.activeElement.blur();
                        // #60 193 - Egalement sur la fenêtre réellement active, et pas uniquement sur le document racine (top)
                        if (oFrm.document.activeElement)
                            oFrm.document.activeElement.blur();

                        that.confirmChangesModalDisplayed = true;
                        return eAdvConfirm({
                            'criticity': 1,
                            'title': top._res_30,
                            'message': top._res_926,
                            'details': '',
                            'width': 500,
                            'height': 200,
                            'okFct': oModalClose,
                            'cancelFct': function () { that.confirmChangesModalDisplayed = false; },
                            'bOkGreen': false,
                            'bHtml': false,
                            'resOk': top._res_30,
                            'resCancel': top._res_29
                        });
                    }
                    else
                        return;
                }
                // Sans confirmation
                else
                    oModalClose(that, docEventKey, KeyPressNativeFunction);
            }
        }
        else {
            // On vérifie si la touche appuyée correspond à un caractère "imprimable" et non à une touche système/de fonction
            if (isWritableCharCode(e.keyCode))
                that.unsavedChanges = true; // on indique que l'utilisateur a saisi des données susceptibles d'être perdues à la fermeture de la fenêtre
            ScanString(e.keyCode);
        }
    }
    ///summary
    ///Parcours la chaine str et la compare au mot de passe paramétré
    ///pour afficher le son secret
    ///summary
    function ScanString(keyPress) {
        try {


        }
        catch (ex) {

        }
    }



    this.loadPage = function () {
        //Envoi de l'id de l'iframe
        ednuPopup.addParam("_parentiframeid", "frm_" + uId, "post");
        //TODO en cas de besoin - Passer le nom de la variable - ednuPopup.addParam("_parentmodalvarname", modalvarname);

        // Mise à jour de la variable éventuellement passée en POST pour que la nouvelle taille soit connue de la page affichée par la Modal Dialog
        this.setParam("width", Math.round(this.absWidth), "post");
        this.setParam("height", Math.round(this.absHeight), "post");
        this.setParam("divMainWidth", this.getDivMainWidth(), "post");
        this.setParam("divMainHeight", this.getDivMainHeight(), "post");

        if (ModalType.ToolTip.toString() == type.toString()) {
            this.InitToolTipDivContener();
        }

        if (typeof (this.ErrorCallBack) == 'function')
            ednuPopup.ErrorCallBack = this.ErrorCallBack;

        if (typeof (this.ErrorCustomAlert) == 'function')
            ednuPopup.ErrorCustomAlert = this.ErrorCustomAlert;

        if (this.url == "blank")
            this.createIframe("loading");
        else if (this.isStatic)
            this.createIframe("loading");
        else
            ednuPopup.send(this.createIframe, null);
    };

    this.addParam = function (pName, pValue, pMethod) {
        if (ednuPopup)
            ednuPopup.addParam(pName, pValue, pMethod);
    };

    // Recupération d'un paramètre posté
    this.getParam = function (name) {
        if (ednuPopup)
            return ednuPopup.getParam(name);
    };

    // Recupération d'un paramètre posté
    this.setParam = function (name, newValue, requestType) {
        ednuPopup.setParam(name, newValue, requestType);
    };

    this.createBackGround = function () {
        //Div de background
        backgroundDiv = top.document.createElement('div');
        backgroundDiv.id = "Bg_" + uId;
        backgroundDiv.className = "BackgroundModal";
        backgroundDiv.style.position = 'absolute';
        backgroundDiv.style.left = 0 + "px";
        backgroundDiv.style.top = "-13px";
        // #58 123 - remplacement du 100% par la dimension absolue totale du document (scroll compris)
        // Permet d'afficher le calque sur toute la surface et non pas seulement sur la surface visible avant scroll
        backgroundDiv.style.width = this.maxDocWidth + "px";
        backgroundDiv.style.height = this.maxDocHeight + "px";
        backgroundDiv.style.backgroundColor = "gray";

        backgroundDiv.style.opacity = (30 / 100);
        backgroundDiv.style.MozOpacity = (30 / 100);
        backgroundDiv.style.KhtmlOpacity = (30 / 100);
        backgroundDiv.style.filter = "alpha(opacity=" + 30 + ")";
        backgroundDiv.style.zIndex = this.level;
        top.document.body.appendChild(backgroundDiv);
        oldoverflow = top.document.body.style.overflow;
        top.document.body.style.overflow = "hidden";
    };

    this.createDivContainer = function (docWidth, docHeight, pLeft, pTop, formularType) {
        this.trace("Création du conteneur général, gauche : " + pLeft + ", haut : " + pTop + ", largeur du document : " + docWidth + ", hauteur : " + docHeight);

        //DivContainer
        // var font = document.getElementById("gw-container").classList[0]
        var fsize = 8;
        var oeParam = top.getParamWindow();

        if (oeParam && typeof (oeParam.GetParam) == 'function')
            fsize = oeParam.GetParam('fontsize');

        fsize = "fs_" + fsize + "pt";
        containerDiv = top.document.createElement('div');
        containerDiv.id = "ContainerModal_" + uId;
        containerDiv.className = "ContainerModal openTransition " + fsize;
        containerDiv.style.position = 'absolute';
        containerDiv.style.left = pLeft + "px";
        containerDiv.style.top = pTop + "px";
        containerDiv.style.width = (formularType == 1) ? "100%" : this.absWidth + "px";
        containerDiv.style.height = (formularType == 1) ? "100%" : this.absHeight + "px";
        containerDiv.style.zIndex = this.level;
        //containerDiv.style.opacity = 0;
        //containerDiv.style.filter = 'alpha(opacity = 0)';

        top.document.body.appendChild(containerDiv);
    };

    //TOOLTIP - Permet de créer le div conteneur de la modal
    this.createDivContainerToolTip = function (docWidth, docHeight, pLeft, pTop) {
        this.trace("Création de l'infobulle du conteneur général, gauche : " + pLeft + ", haut : " + pTop + ", largeur du document : " + docWidth + ", hauteur : " + docHeight);

        //DivContainer
        containerDiv = top.document.createElement('div');
        containerDiv.id = "ContainerModal_" + uId;
        //containerDiv.className = "ContainerModal";
        containerDiv.style.position = 'absolute';
        containerDiv.style.left = pLeft + "px";
        containerDiv.style.top = pTop + "px";
        containerDiv.style.width = this.absWidth + "px";
        containerDiv.style.height = this.absHeight + "px";
        containerDiv.style.zIndex = this.level;
        //containerDiv.style.opacity = 0;
        //containerDiv.style.filter = 'alpha(opacity = 0)';

        top.document.body.appendChild(containerDiv);
    };

    this.createDivTitle = function (docWidth, docHeight, pLeft, pTop) {
        this.trace("Création de la barre de titre, gauche : " + pLeft + ", haut : " + pTop + ", largeur du document : " + docWidth + ", hauteur : " + docHeight);

        if (that.title == null || that.title == "")
            that.title = "&nbsp;";
        else {

            if (typeof (encodeHTMLEntities) == "function")
                that.title = encodeHTMLEntities(that.title);
        }


        this.noTitle = false;

        //DivTitle
        titleDiv = top.document.createElement('div');
        titleDiv.id = "TitleModal_" + uId;
        //titleDiv.className = "TitleModal";
        titleDiv.style.position = 'absolute';
        titleDiv.style.left = "0px";
        titleDiv.style.top = "-5px";
        titleDiv.style.width = "100%"; // width + "px";
        titleDiv.style.height = "100%";// 26 + "px";
        var strImgControlBoxMaximize = '';
        var strImgControlBoxClose = '';

        if (!this.hideMaximizeButton) {
            strImgControlBoxMaximize = "<div class='icon-maximize' id='ImgControlBox_" + uId + "'></div>";
        }
        if (!this.hideCloseButton) {
            strImgControlBoxClose = "<div class='icon-edn-cross' id='ImgControlBoxClose_" + uId + "'></div>";
        }
        titleDiv.innerHTML = "<table class='tbHtileModal' cellspacing=0 cellpadding=0>"
            + "<tr>"
            + "<td id='td_title_" + uId + "' class='TitleModal' onmousedown='doOnmouseDownModal(event);'>" + that.title + "&nbsp;" + (this.debugMode ? uId : "") + "</td>"
            + "<td id='td_ctrl_" + uId + "' class='ControlBox' align=right>"
            + strImgControlBoxClose
            + strImgControlBoxMaximize
            + "</td>"
            + "</tr>"
            + "</table>";
        titleDiv.style.opacity = 1;
        titleDiv.style.MozOpacity = 1;
        titleDiv.style.KhtmlOpacity = 1;


        containerDiv.appendChild(titleDiv);

        if (this.IconClass != "") {
            var oTitleTd = top.document.getElementById("td_title_" + uId);
            oTitleTd.className = oTitleTd.className + " " + this.IconClass;
        }

        if ((this.type == ModalType.ModalPage || ModalType.DisplayContent) && !this.hideMaximizeButton) {
            top.document.getElementById("ImgControlBox_" + uId).onclick = this.MaxOrMinModal;
        }

        if (!this.hideCloseButton) {
            top.document.getElementById("ImgControlBoxClose_" + uId).onclick = this.hide;
        }




    };

    this.getDivMainWidth = function (customWidth) {
        this.trace("Récupération de la largeur du conteneur principal (paramètre : " + customWidth + ")");

        var mainDivWidth = (typeof (customWidth) == 'undefined' || customWidth == null) ? this.absWidth : customWidth;

        this.trace("Largeur renvoyée : " + mainDivWidth);

        return mainDivWidth;
    };

    this.getDivMainHeight = function (customHeight) {
        this.trace("Récupération de la hauteur du conteneur principal (paramètre : " + customHeight + ")");

        var mainDivHeight = (typeof (customHeight) == 'undefined' || customHeight == null) ? this.absHeight : customHeight;

        // Si bouton, le main ne prend pas les 50px
        if (this.noButtons != true)
            mainDivHeight -= 50;        // taille buttonsDiv (50)

        // Si titre, le main ne prend pas les 20px
        if (this.noTitle != true)
            mainDivHeight -= 20;        // taille titleDiv (20)

        // Si barre d'outils, le main ne prend pas les 35px
        if (this.noToolbar != true)
            mainDivHeight -= 35;        // taille toolbarDiv (35

        this.trace("Hauteur renvoyée : " + mainDivHeight);

        return mainDivHeight;
    };


    this.getDivMain = function () {
        return mainDiv;
    };


    this.createDivMain = function (docWidth, docHeight, pLeft, pTop) {
        this.trace("Création du conteneur principal, gauche : " + pLeft + ", haut : " + pTop + ", largeur du document : " + docWidth + ", hauteur : " + docHeight);

        //Div principal
        mainDiv = top.document.createElement('div');

        mainDiv.id = "MainModal_" + uId;
        mainDiv.className = "MainModal";
        mainDiv.style.position = 'absolute';
        // mainDiv.style.overFlow = 'absolute';
        mainDiv.style.overflowX = 'hidden';
        if (this.NoScrollOnMainDiv)
            mainDiv.style.overflowY = 'hidden';

        var nTopOffset = 0;
        if (this.noTitle != true) { nTopOffset += 20; }
        if (this.noToolbar != true) { nTopOffset += 35; }

        mainDiv.style.left = 0 + "px";
        mainDiv.style.top = nTopOffset + "px"; // offset par rapport à titleDiv
        mainDiv.style.width = "100%"; // width + "px";

        mainDiv.style.height = this.getDivMainHeight() + "px"; // 70 = taille titleDiv (20) + taille buttonsDiv (50)

        mainDiv.style.opacity = 1;
        mainDiv.style.MozOpacity = 1;
        mainDiv.style.KhtmlOpacity = 1;

        //Creation du contenu
        switch (type.toString()) {
            //Page classique en popup               
            case ModalType.ModalPage.toString():
                this.loadPage();
                break;
            //Messagebox local // avec icone               
            case ModalType.MsgBoxLocal.toString():
                this.createLocalMsgBox();
                break;
            //Messagebox local // avec icone               
            case ModalType.Prompt.toString():
                this.createPrompt();
                break;
            case ModalType.Waiting.toString():
                this.createWaiting();
                break;
            case ModalType.ProgressBar.toString():
                this.createProgressBar();
                break;
            case ModalType.ToolTip.toString():
                this.loadPage();
                break;
            case ModalType.VCard.toString():
                mainDiv.style.top = 0;
                containerDiv.className = "";
                mainDiv.className = "VCardTT openTransition";
                this.loadPage();
                break;
            case ModalType.ToolTipSync.toString():
                //this.loadPage();
                //TODO
                break;
            case ModalType.ColorPicker.toString():
                this.createColorPicker();
                break;
            case ModalType.SelectList.toString():
                this.createSelectList();
                break;
            case ModalType.DisplayContent.toString():
                this.displayContent();
                break;
            case ModalType.DisplayContentWithoutTitle.toString():
                this.displayContent();
                break;
        }

        // Backlog #1659 - Ajout du type de modal en attribut pour le ciblage sur les nouveaux thèmes
        // Au même titre que cette containerDiv contient le msgtype (WARNING, SUCCESS...) pour les modal dialogs de type MsgBoxLocal (2)
        setAttributeValue(containerDiv, "modaltype", type);

        containerDiv.appendChild(mainDiv);

    };


    this.targetPicker = null;
    this.targetPickerText = null;
    this.targetPickerOnChange = null;
    this.srcFrameId = null;
    this.bgColor = null;


    this.createColorPicker = function () {

        var tr, td;

        var colors = "#1785bf;#2d9662;#f1a504;#910101;#553399;#535151;" +
            "#42a7dc;#3fa371;#f9b21a;#bb1515;#800080;#6e6c6c;" +
            "#69c0ee;#69bf69;#f9b931;#dc4646;#954cce;#878585;" +
            "#90d1f3;#8bd48b;#fdce69;#e86c6c;#c48af2;#9a9898;" +
            "#a9ddf8;#a7dfa7;#fbd75b;#f09494;#dbadff;#bbb8b8;" +
            "#c1e8fc;#c4e9c4;#ffec86;#f8c4c4;#e9cdff;#d1d0d0;" +
            "#ceedfd;#daf2da;#fff1b0;#ffe5e5;#f0ddfe;#e7e4e4;" +
            "#e4f8f8;#e8f6e8;#fef6d2;#fceded;#f6ebfe;#efefef";

        var aCol = colors.split(';');

        var tabColors = top.document.createElement("table");
        tabColors.className = "tabColors";
        mainDiv.appendChild(tabColors);

        var nColByLine = 6;

        // Bouton "Couleur par défaut"
        tr = top.document.createElement("tr");
        tabColors.appendChild(tr);
        td = top.document.createElement("td");;
        td.setAttribute("colspan", nColByLine);
        td.className = "btnDefaultColor";
        SetText(td, top._res_7975);
        td.onclick = function () {
            that.targetPicker.style.backgroundColor = "";
            that.targetPicker.setAttribute("value", "");
            if (that.targetPickerText) {
                that.targetPickerText.value = "";
                if (that.targetPickerText.onchange)
                    that.targetPickerText.onchange();
                if (that.targetPickerOnChange && typeof that.targetPickerOnChange === 'function')
                    that.targetPickerOnChange();
            }
            that.hide();
        }

        tr.appendChild(td);

        for (var i = 0; i < aCol.length / nColByLine; i++) {
            tr = top.document.createElement("tr");
            tabColors.appendChild(tr);
            for (var j = 0; j < nColByLine; j++) {

                var sClass = "PersColor";
                var colIdx = i * nColByLine + j;
                if (colIdx > aCol.length)
                    return;
                td = top.document.createElement("td");;
                tr.appendChild(td);

                if (aCol[colIdx] != "") {
                    if (aCol[colIdx] == that.targetPicker.getAttribute("value"))
                        sClass = "selectedColCell";
                    var divCol = top.document.createElement("div");
                    td.appendChild(divCol);
                    divCol.className = sClass;
                    divCol.setAttribute("value", aCol[colIdx]);
                    divCol.setAttribute("title", aCol[colIdx]);
                    divCol.style.backgroundColor = aCol[colIdx];

                    var myFct = (function (a) {
                        return function () {
                            that.targetPicker.style.backgroundColor = aCol[a];
                            that.targetPicker.setAttribute("value", aCol[a]);
                            if (that.targetPickerText) {
                                that.targetPickerText.value = aCol[a];
                                if (that.targetPickerText.onchange)
                                    that.targetPickerText.onchange();
                                if (that.targetPickerOnChange && typeof that.targetPickerOnChange === 'function')
                                    that.targetPickerOnChange();
                            }
                            that.hide();
                        };
                    })(colIdx);

                    divCol.onclick = myFct;
                }
            }

        }
    };

    //Appelcontenu
    this.createDivMainToolTip = function (docWidth, docHeight, pLeft, pTop, pContainerDiv) {

        //Div principal
        mainDiv = top.document.createElement('div');
        mainDiv.id = "MainModal_" + uId;
        mainDiv.className = "MainModal";
        //mainDiv.style.position = 'absolute';
        // mainDiv.style.overFlow = 'absolute';
        mainDiv.style.overflowX = 'hidden';
        mainDiv.style.overflowY = 'auto';


        //        mainDiv.style.left = 0 + "px";
        //        mainDiv.style.top = 20 + "px"; // offset par rapport à titleDiv
        mainDiv.style.width = "100%"; // width + "px";

        mainDiv.style.height = this.getDivMainHeight() + "px";

        mainDiv.style.opacity = 1;
        mainDiv.style.MozOpacity = 1;
        mainDiv.style.KhtmlOpacity = 1;
        //Creation du contenu
        switch (type.toString()) {
            //Page classique en popup               
            case ModalType.ModalPage.toString():
                this.loadPage();
                break;
            //Messagebox local // avec icone               
            case ModalType.MsgBoxLocal.toString():
                this.createLocalMsgBox();
                break;
            //Messagebox local // avec icone               
            case ModalType.Prompt.toString():
                this.createPrompt();
                break;
            case ModalType.Waiting.toString():
                this.createWaiting();
                break;
            case ModalType.ProgressBar.toString():
                this.createProgressBar();
                break;
            case ModalType.ToolTip.toString():
                this.loadPage();
                break;
            case ModalType.VCard.toString():
                this.loadPage();
                break;
            case ModalType.ToolTip.toString():
                this.loadPage();
                break;
        }
        if (pContainerDiv != null)
            pContainerDiv.appendChild(mainDiv);
        else
            containerDiv.appendChild(mainDiv);

    };

    this.createWaiting = function () {
        var mainMsgDiv = top.document.createElement("div");
        mainDiv.appendChild(mainMsgDiv);
        //Div logo
        var divLogo = top.document.createElement("div");
        divLogo.className = "waiting-image";
        mainMsgDiv.appendChild(divLogo);
        //Div message
        var divMsg = top.document.createElement("div");
        divMsg.id = "waiting-message";
        divMsg.className = "text-alert-info";
        mainMsgDiv.appendChild(divMsg);

    }

    var divProgressContainer = null;
    var spanProgressText = null;
    var divProgress = null;

    this.createProgressBar = function () {
        divMainProgressContainer = top.document.createElement("div");
        mainDiv.appendChild(divMainProgressContainer);
        divMainProgressContainer.className = "divMainProgressContainer";

        divProgressContainer = top.document.createElement("div");
        divMainProgressContainer.appendChild(divProgressContainer);
        divProgressContainer.className = "divProgressContainer";

        divProgress = top.document.createElement("div");
        divProgress.className = "divProgress";
        divProgressContainer.appendChild(divProgress);

        spanProgressText = top.document.createElement("span");
        spanProgressText.className = "spanProgressText";
        spanProgressText.innerHTML = "&nbsp;";
        divProgressContainer.appendChild(spanProgressText);

        divMainProgressContainer.style.marginTop = (getNumber(mainDiv.style.height.replace('px', '')) / 2 - getNumber(top.getCssSelector("eModalDialog.css", ".divProgressContainer").style.height.replace('px', '')) / 2) + 'px';
    };

    this.updateProgressBar = function (val) {
        divProgress.style.width = val + "%";
        spanProgressText.innerHTML = val + "%...";
    };

    this.createDivButtons = function (docWidth, docHeight, pLeft, pTop, formularType) {
        //Ajout du div de redimentionnement
        if (this.noButtons == true)
            return;

        //Div Boutons
        buttonsDiv = top.document.createElement('div');
        buttonsDiv.id = "ButtonModal_" + uId;
        // ELAIZ - vérification si le type est 2, on vient alors de showCal dans eFilterWizardLight.js
        buttonsDiv.className = formularType == 2 ? "ButtonModal calButtonModal" : "ButtonModal";
        buttonsDiv.style.position = 'absolute';
        buttonsDiv.style.left = 0 + "px";
        buttonsDiv.style.top = (this.absHeight - 50) + "px";
        buttonsDiv.style.width = "100%";
        buttonsDiv.style.height = 50 + "px";
        /*  buttonsDiv.style.paddingTop = "10px"; */ /* - Pierre - Je me suis permis de l'enlever car elle était de trop lors de mes ajustements après avoir rajouté un border sur tout les modals */

        var table = top.document.createElement("table");
        table.setAttribute("width", "100%");
        table.setAttribute("cellpadding", "0");
        table.setAttribute("cellspacing", "0");

        var tBody = top.document.createElement("tbody");

        var tr = top.document.createElement("tr");

        tdButtonsLeft = top.document.createElement("td");
        tdButtonsLeft.id = "tdButtonsLeft";
        tr.appendChild(tdButtonsLeft);

        tdButtonsMid = top.document.createElement("td");
        tdButtonsMid.id = "tdButtonsMid";
        tr.appendChild(tdButtonsMid);

        tdButtons = top.document.createElement("td");
        tdButtons.id = "tdButtons";
        tr.appendChild(tdButtons);

        var tdGhost = top.document.createElement("td");
        tdGhost.innerHTML = "&nbsp;";
        if (this.btnSpacerClass) {
            tdGhost.setAttribute("class", this.btnSpacerClass);
        }
        else {
            if (this.bBtnAdvanced)
                tdGhost.setAttribute("class", "actBtnLstAdv");
            else
                tdGhost.setAttribute("class", "actBtnLst");
        }
        tr.appendChild(tdGhost);

        tBody.appendChild(tr)
        table.appendChild(tBody);
        buttonsDiv.appendChild(table);
        containerDiv.appendChild(buttonsDiv);
    };

    this.createDivToolbar = function (docWidth, docHeight, pLeft, pTop) {
        if (this.noToolbar == true)
            return;

        //Div barre d'outils gauche
        toolbarDivLeft = top.document.createElement('div');
        toolbarDivLeft.id = "ToolbarModalLeft_" + uId;
        toolbarDivLeft.className = "ToolbarModal ToolbarModalLeft";
        toolbarDivLeft.style.position = 'absolute';
        toolbarDivLeft.style.left = 0 + "px";
        toolbarDivLeft.style.top = (this.noTitle ? 0 : 20) + "px";
        toolbarDivLeft.style.width = "50%";
        toolbarDivLeft.style.height = 35 + "px";

        //Div barre d'outils droite
        toolbarDivRight = top.document.createElement('div');
        toolbarDivRight.id = "ToolbarModalRight_" + uId;
        toolbarDivRight.className = "ToolbarModal ToolbarModalRight";
        toolbarDivRight.style.position = 'absolute';
        toolbarDivRight.style.right = 0 + "px";
        toolbarDivRight.style.top = (this.noTitle ? 0 : 20) + "px";
        toolbarDivRight.style.width = "50%";
        toolbarDivRight.style.height = 35 + "px";

        var ul = top.document.createElement("ul");
        ul.className = "ToolbarModal";

        ulToolbarLeft = top.document.createElement("ul");
        ulToolbarLeft.className = "ToolbarModal ToolbarModalLeft";
        ulToolbarLeft.id = "ulToolbarButtons";
        toolbarDivLeft.appendChild(ulToolbarLeft);

        ulToolbarRight = top.document.createElement("ul");
        ulToolbarRight.className = "ToolbarModal ToolbarModalRight";
        ulToolbarRight.id = "ulToolbarButtons";
        toolbarDivRight.appendChild(ulToolbarRight);

        containerDiv.appendChild(toolbarDivLeft);
        containerDiv.appendChild(toolbarDivRight);
    };

    //TOOLTIP - Permet de créer le div conteneur de boutons en bas de la modal
    this.createDivButtonsToolTip = function (docWidth, docHeight, pLeft, pTop) {
        if (this.noButtons == true)
            return;

        //Div Boutons
        buttonsDiv = top.document.createElement('div');

        buttonsDiv.id = "ButtonModal_" + uId;
        buttonsDiv.className = "ButtonModal";
        buttonsDiv.style.position = 'absolute';
        buttonsDiv.style.left = 0 + "px";
        buttonsDiv.style.top = (this.absHeight - 30) + "px";
        buttonsDiv.style.width = "100%";
        buttonsDiv.style.height = 30 + "px";

        var table = top.document.createElement("table");
        table.setAttribute("width", "100%");
        table.setAttribute("cellpadding", "0");
        table.setAttribute("cellspacing", "0");

        var tBody = top.document.createElement("tbody");

        var tr = top.document.createElement("tr");

        tdButtons = top.document.createElement("td");
        tdButtons.id = "tdButtons";
        tdButtons.setAttribute("width", "90%");
        tr.appendChild(tdButtons);

        var td2 = top.document.createElement("td");

        tr.appendChild(td2);

        tBody.appendChild(tr)
        table.appendChild(tBody);
        buttonsDiv.appendChild(table);
        containerDiv.appendChild(buttonsDiv);
    };

    var SelectionObjet = function (lbl, value, checked) {
        this.Label = lbl;
        this.Value = value;
        this.Checked = checked;
    };

    this.selList = new Array();
    this.titleSelectOption = "";

    this.addSelectOption = function (lbl, value, checked) {

        // lbl = encodeHTMLEntities(lbl);

        this.selList.push(new SelectionObjet(lbl, value, checked));
    };

    //Retourne la liste des value des valeurs sélectionnées, séparés par des ;
    this.getSelectedValue = function () {
        var ret = "";
        for (var i = 0; i < this.selList.length; i++) {
            var radio = top.document.getElementById("radio_" + i);
            if (this.isMultiSelectList) {
                if (radio.getAttribute("chk") == "1") {
                    if (ret != "")
                        ret += ";";
                    ret += radio.getAttribute("value");
                }
            }
            else {
                if (radio.checked) {
                    ret = radio.value;
                    break;
                }
            }
        }
        return ret;
    };
    //Retourne un tableau de valeurs sélectionné,
    //  chaque entrée du tableau étant un objet composé de :
    //      - lib étant le libellé de la case sélectionné
    //      - val étant la value de la case sélectionné
    this.getSelected = function () {
        var tabRet = new Array();
        for (var i = 0; i < this.selList.length; i++) {
            var radio = top.document.getElementById("radio_" + i);
            var obj = new Object();
            if (this.isMultiSelectList) {
                obj.lib = GetText(radio);
                obj.val = radio.getAttribute("value");
                if (radio.getAttribute("chk") == "1") {
                    tabRet.push(obj);
                }
            }
            else {
                obj.lib = GetText(top.document.getElementById("lib_" + i));
                obj.val = radio.value;
                if (radio.checked) {
                    tabRet.push(obj);
                    break;
                }
            }
        }
        return tabRet;
    };

    this.createSelectList = function () {
        var divTitle = top.document.createElement("div");
        divTitle.setAttribute("class", "divListTitle");
        divTitle.innerHTML = this.titleSelectOption;
        mainDiv.appendChild(divTitle);
        this.createSelectListCheckOpt();
    };

    //Génère le rendu des case à cocher demandée
    this.createSelectListCheckOpt = function () {
        var divList = top.document.createElement("div");
        divList.setAttribute("class", "divListChoice");
        if (type.toString() == ModalType.SelectList.toString() || (tdMsgDetails == null))
            mainDiv.appendChild(divList);
        else
            tdMsgDetails.appendChild(divList);

        for (var i = 0; i < this.selList.length; i++) {
            var divSelect = top.document.createElement("div");
            divSelect.setAttribute("class", "divSelectRadio");

            var value = this.selList[i].Value;
            var label = this.selList[i].Label;
            var checked = this.selList[i].Checked;
            var radio = null;
            if (this.isMultiSelectList) {
                var attributes = new Dictionary();
                attributes.Add("value", value);
                attributes.Add("name", "SelectionList");
                radio = AddEudoCheckBox(top.document, divSelect, checked, "radio_" + i, label, attributes);
            }
            else {
                radio = top.document.createElement("input");
                radio.id = "radio_" + i;
                radio.setAttribute("type", "radio");
                if (checked == true) {
                    radio.setAttribute("checked", "checked");
                }
                radio.setAttribute("value", value);
                radio.setAttribute("name", "SelectionList");
                divSelect.appendChild(radio);

                var span = top.document.createElement("label");
                span.id = "lib_" + i;
                span.setAttribute("for", "radio_" + i);
                span.innerHTML = label;
                divSelect.appendChild(span);
            }

            divList.appendChild(divSelect);
        }
    };

    //Ajuste la Hauteur de la modale au contenu de la div principale.
    //  nBottomMargin : si l'on souhaite avoir une marge en bas de la liste
    this.adjustModalToContent = function (nBottomMargin) {
        if (!nBottomMargin)
            nBottomMargin = 0;
        var oPos = getAbsolutePosition(mainDiv);
        var divHeight = oPos.h;
        var divWidth = oPos.w;
        var divScrollHeight = mainDiv.scrollHeight;
        var divScrollWidth = mainDiv.scrollWidth;
        if (divScrollHeight > divHeight || divScrollWidth > divWidth) {
            // TODO - REVOIR l'utilisation de height et width, on devrai pas s'appuyer sur absHeight et absWidth
            var newHeight = getNumber(this.height) + (divScrollHeight - divHeight) + nBottomMargin;
            if (newHeight > top.document.body.offsetHeight)
                newHeight = top.document.body.offsetHeight - 10;

            var newWidth = getNumber(this.width) + (divScrollWidth - divWidth)
            if (newWidth > top.document.body.offsetWidth)
                newWidth = top.document.body.offsetWidth;

            this.resizeTo(newWidth, newHeight);
        }
    }

    //Ajuste la Hauteur de la modale au contenu de l'iframe.
    // doit être appelé après le chargement de l'iframe
    this.adjustModalToContentIframe = function (nOffset) {
        if (typeof (nOffset) != "number")
            nOffset = 0;

        oFrame = that.getIframeTag();
        if (oFrame) {
            var oBody = oFrame.contentWindow.document.body;
            var oHtml = oFrame.contentWindow.document.documentElement;

            var height = Math.max(oBody.scrollHeight, oBody.offsetHeight, oHtml.clientHeight, oHtml.scrollHeight, oHtml.offsetHeight);
            oFrame.style.height = (height + nOffset) + "px";
        }
    };

    // Taille de départ : 90% de la fenêtre
    this.forceWindowMaxSize = function () {
        // On désactive l'agrandissement de la fenêtre car on souhait qu'elle prenne toute la taille de l'écran
        that.hideMaximizeButton = true;

        // Détuit de la taille de la fenetre la marge*2 pour chaque côtés
        that.width = (that.docWidth - that.marginDialogMaxSize * 2) + '';
        that.height = (that.docHeight - that.marginDialogMaxSize * 2) + '';

        that.trace("Force les dimensions de la modal dialog au max - width : " + that.width + ", height : " + that.height + ", margin : " + that.marginDialogMaxSize);

        // On relance les calcules sur les dimensions abs
        that.initAbsSize();
    };

    this.show = function (posLeft, posTop, bLeftOrRight, formularType) {

        if (!top.ModalDialogs)
            top.ModalDialogs = [];

        top.ModalDialogs[this.UID] = this;

        this.trace("Affichage de la modal dialog - posLeft : " + posLeft + ", posTop : " + posTop + ", bLeftOrRight : " + bLeftOrRight);

        this.initSize();

        this.trace("Affichage de la modal dialog - Largeur de la fenêtre : " + this.absWidth + ", hauteur : " + this.absHeight);
        this.trace("Affichage de la modal dialog - Largeur du document : " + this.docWidth + ", hauteur : " + this.docHeight);

        // #58 123 (remplace #33 929) - positionnement de la fenêtre en tenant compte de la position du scroll sur la page racine
        // Permet d'afficher les popups au bon endroit lorsqu'on affiche une page avec scroll (ex : formulaire affiché par la page externe en dehors d'eMain.aspx)
        // Devrait également concerner les tablettes lorsque celles-ci affichent du contenu zoomé (deux doigts), d'où la refactorisation du correctif #33 929 dans initSize()
        var pLeft = ((typeof (posLeft) == "undefined") ? ((this.docWidth - this.absWidth) / 2) + this.topScrollWidth : posLeft);
        var pTop = ((typeof (posTop) == "undefined") ? ((this.docHeight - this.absHeight) / 2) + this.topScrollHeight : posTop);

        this.trace("Affichage de la modal dialog - Position recalculée : gauche " + pLeft + ", haut " + pTop);

        if (pTop < 0)
            pTop = -pTop;

        if (type.toString() == ModalType.VCard.toString()) {
            this.createDivContainer(this.docWidth, this.docHeight, pLeft, pTop);
            this.createDivMain(this.docWidth, this.docHeight, pLeft, pTop);
        }
        else if ((type.toString() != ModalType.ToolTip.toString()) && (type.toString() != ModalType.ToolTipSync.toString())) {
            //Creation des divs
            this.createBackGround();
            this.createDivContainer(this.docWidth, this.docHeight, pLeft, pTop, formularType);
            if (type.toString() != ModalType.DisplayContentWithoutTitle.toString())
                this.createDivTitle(this.docWidth, this.docHeight, pLeft, pTop);
            this.createDivToolbar(this.docWidth, this.docHeight, pLeft, pTop);
            this.createDivMain(this.docWidth, this.docHeight, pLeft, pTop);
            this.createDivButtons(this.docWidth, this.docHeight, pLeft, pTop, formularType);
        }
        else if (type.toString() == ModalType.ToolTip.toString()) {  //SI TOOLTIP
            //Position
            if (bLeftOrRight) {
                pLeft = pLeft - this.absWidth;
                pLeft = pLeft - CONST_TOOLTIP_ARROW_WIDTH; //12 étant la largeur de la flèche à gauche de la tooltip
            }
            else
                pLeft = pLeft + CONST_TOOLTIP_ARROW_WIDTH; //12 étant la largeur de la flèche à gauche de la tooltip

            this.createToolTipBorder(this.docWidth, this.docHeight, pLeft, pTop, bLeftOrRight);
        }
        else {
            //Autres types
        }

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
            if (top.document.getElementById("InputPrompt_" + uId) != null)
                top.document.getElementById("InputPrompt_" + uId).focus();
        }


        if (this.openMaximized)
            this.MaxOrMinModal();
        //fadeThis(containerDiv.id, top.document);
    };

    //TOOLTIP - Permet de créer le pourtout le la tooltip (fleche à gauche ou droite et cadre)
    //bLeftOrRight correspond à la position de la fleche true = gauche et false = droite
    this.createToolTipBorder = function (docWidth, docHeight, pLeft, pTop, bLeftOrRight) {

        //MOU demande cf.21089 : carte de visite tronquée en bas
        var pTopArrow = pTop;
        if (pTop + this.absHeight > docHeight) {

            pTop = (docHeight - this.absHeight) - 9; //9px pour l espacement en bas 
        }
        pTopArrow = pTopArrow - pTop;


        this.createDivContainerToolTip(docWidth, docHeight, pLeft, pTop);

        var mainTab = top.document.createElement('table');
        mainTab.setAttribute("cellpadding", "0");
        mainTab.setAttribute("cellspacing", "0");
        mainTab.className = "tt_background_bulle";

        var mainTabTR = mainTab.appendChild(document.createElement("tr"));
        var mainTabTD;

        if (!bLeftOrRight) {
            /*FLECHE A DROITE*/
            mainTabTD = mainTabTR.appendChild(document.createElement("td"));
            mainTabTD.className = "tt_arrow_td";
            mainTabTD.style.verticalAlign = "Top";

            var flecheDiv = mainTabTD.appendChild(document.createElement("div"));
            flecheDiv.className = "tt_arrow_right";


            flecheDiv.style.top = pTopArrow + "px"; //MOU demande cf.21089
            flecheDiv.appendChild(document.createTextNode(" "));
            /********/
        }
        mainTabTD = mainTabTR.appendChild(document.createElement("td"));
        mainTabTD.style.verticalAlign = "Top";
        mainTabTD.className = "tt_mid_tab";
        this.createDivMainToolTip(docWidth, docHeight, pLeft, pTop, mainTabTD);

        if (bLeftOrRight) {
            /*FLECHE A GAUCHE*/
            mainTabTD = mainTabTR.appendChild(document.createElement("td"));
            mainTabTD.className = "tt_arrow_td";
            mainTabTD.style.verticalAlign = "Top";
            var flecheDiv = mainTabTD.appendChild(document.createElement("div"));
            flecheDiv.className = "tt_arrow_left";
            flecheDiv.style.top = pTopArrow + "px"; //MOU demande cf.21089
            flecheDiv.appendChild(document.createTextNode(" "));
            /********/
        }

        containerDiv.appendChild(mainTab);
    };

    this.getControl = function (sId) {
        return top.document.getElementById(sId);
    }

    this.getPromptValue = function () {
        var oPrompt = top.document.getElementById('InputPrompt_' + uId);
        if (oPrompt != null) {
            return oPrompt.value;
        }
    };

    this.getPromptIdTextBox = function () {
        var idInput = 'InputPrompt_' + uId;
        return idInput;
    }

    //Fonction d'encodage
    this.encode = function (strValue) {
        var strReturnValue;

        if (strValue == "" || strValue == null)
            return "";

        try {
            var strReturnValue = encodeURIComponent(strValue);
            strReturnValue = strReturnValue.replace(/'/g, "%27");
        }
        catch (e) {
            strReturnValue = escape(strValue);
        }
        return strReturnValue;
    };

    this.decode = function (strValue) {
        var strReturnValue;

        if (strValue == "" || strValue == null)
            return "";

        //strValue = strValue.replace( /\%27/g, "'" );
        try {
            strReturnValue = decodeURIComponent(strValue);
        }
        catch (e) {
            strReturnValue = unescape(strValue);
        }
        return strReturnValue;
    };
    this.resizeToMaxWidth = function () {
        var nDivMainWidth = that.getDivMainWidth(that.docWidth - (that.marginDialogMaxSize * 2));
        // Title
        if (!that.noTitle) {
            that.initialTitleWidth = titleDiv.style.width;
            titleDiv.style.width = nDivMainWidth + "px";
        }
        containerDiv.style.width = (that.docWidth - (that.marginDialogMaxSize * 2)) + "px";        // -20 pour le margin right et left de 10
        mainDiv.style.width = nDivMainWidth + "px";
        containerDiv.style.left = that.marginDialogMaxSize + "px";


    };
    this.MaxOrMinModal = function () {
        var resizeButton = top.document.getElementById("ImgControlBox_" + uId);
        var maximized = false;
        var modalIframe = document.querySelector('.MainModal > iframe');
        var mainDivContent = modalIframe ? modalIframe.contentWindow.document.querySelector('#mainDiv') : null;

        if (that.sizeStatus != "max")
        //Maximiser la fenêtre
        {
            maximized = true;
            that.initialContainerTop = containerDiv.style.top;
            that.initialContainerLeft = containerDiv.style.left;
            that.initialContainerWidth = containerDiv.style.width;
            that.initialContainerHeight = containerDiv.style.height;
            that.initialContainerMargin = containerDiv.style.margin;

            that.initialMainWidth = mainDiv.style.width;
            that.initialMainHeight = mainDiv.style.height;

            that.initSize();
            var nDivMainWidth = that.getDivMainWidth(that.docWidth - (that.marginDialogMaxSize * 2));
            var nDivMainHeight = that.getDivMainHeight(that.docHeight - (that.marginDialogMaxSize * 2));

            // Title
            if (!that.noTitle) {
                that.initialTitleWidth = titleDiv.style.width;
                titleDiv.style.width = nDivMainWidth + "px";
            }

            var nTitleOffsetTop = (that.noTitle ? 0 : 20);
            var nToolbarOffsetTop = (that.noToolbar ? 0 : 35);

            // Barre d'outils
            if (!that.noToolbar) {
                that.initialToolbarLeftWidth = toolbarDivLeft.style.width;
                that.initialToolbarRightWidth = toolbarDivRight.style.width;
                toolbarDivLeft.style.width = "50%";
                toolbarDivRight.style.width = "50%";
            }

            containerDiv.style.top = "0px";
            containerDiv.style.left = "0px";
            containerDiv.style.width = (that.docWidth - (that.marginDialogMaxSize * 2)) + "px";        // -20 pour le margin right et left de 10
            containerDiv.style.height = (that.docHeight - (that.marginDialogMaxSize * 2)) + "px";        // -20 pour le margin top et bottom de 10
            containerDiv.style.margin = that.marginDialogMaxSize + "px";
            mainDiv.style.width = nDivMainWidth + "px";
            mainDiv.style.height = nDivMainHeight + "px"

            // Btn
            if (!that.noButtons) {
                that.initialButtonsTop = buttonsDiv.style.top;
                that.initialButtonsWidth = buttonsDiv.style.width;
                buttonsDiv.style.width = "100%";
                var nTop = nTitleOffsetTop + nToolbarOffsetTop + nDivMainHeight;
                buttonsDiv.style.top = nTop + "px";
            }

            that.sizeStatus = "max";
            if (resizeButton)
                resizeButton.className = 'icon-restore';

            mainDivContent && mainDivContent.classList.add('maximized');
        }
        else
        //Remise à la taille normale
        {
            maximized = false;
            containerDiv.style.top = that.initialContainerTop;
            containerDiv.style.left = that.initialContainerLeft;
            containerDiv.style.width = that.initialContainerWidth;
            containerDiv.style.height = that.initialContainerHeight;
            containerDiv.style.margin = that.initialContainerMargin;

            // Title
            if (!that.noTitle) {
                titleDiv.style.width = that.initialTitleWidth;
            }

            if (!that.noToolbar) {
                toolbarDivLeft.style.width = that.initialToolbarLeftWidth;
                toolbarDivRight.style.width = that.initialToolbarRightWidth;
            }

            mainDiv.style.width = that.initialMainWidth;
            mainDiv.style.height = that.initialMainHeight;

            if (!that.noButtons) {
                buttonsDiv.style.top = that.initialButtonsTop;
                buttonsDiv.style.width = that.initialButtonsWidth;
            }

            that.sizeStatus = "";
            if (resizeButton)
                resizeButton.className = 'icon-maximize';

            mainDivContent && mainDivContent.classList.remove('maximized');
        }

        //abonnement depuis l'iframe 
        if (top.document.getElementById("frm_" + uId) != null) {
            var frm = top.document.getElementById("frm_" + uId).contentWindow;
            if (frm.onFrameSizeChange != null) {
                var mWidth = mainDiv.offsetWidth;
                var mHeight = mainDiv.offsetHeight;
                frm.onFrameSizeChange(mWidth, mHeight * 0.98); // #48584 : On met la hauteur à 98% de la hauteur de la div pour correspondre aux 98% en CSS de l'iframe
            }
        }
    };

    this.moveTo = function (newLeft, newTop) {
        if (newLeft != null)
            containerDiv.style.left = newLeft + "px";

        if (newTop != null)
            containerDiv.style.top = newTop + "px";
    };

    //fait un decalage de la fenetre de deltaX et deltaY
    this.moveBy = function (deltaX, deltaY) {
        if (deltaX != null)
            containerDiv.style.left = (parseInt(containerDiv.style.left) + deltaX) + "px";

        if (deltaY != null)
            containerDiv.style.top = (parseInt(containerDiv.style.top) + deltaY) + "px";

        return this;
    };

    this.resizeTo = function (newWidth, newHeight) {
        this.initSize();

        if (newWidth != null) {
            var pLeft = (this.docWidth - newWidth) / 2;
            containerDiv.style.left = pLeft + "px";
            containerDiv.style.width = newWidth + "px";
            titleDiv.style.width = newWidth + "px";
            mainDiv.style.width = "100%";
        }

        if (newHeight != null) {
            var pTop = (getDocHeight() - newHeight) / 2;
            containerDiv.style.top = pTop + "px";
            containerDiv.style.height = newHeight + "px";
            mainDiv.style.height = that.getDivMainHeight(newHeight) + "px";
            if (mainMsgDiv)
                mainMsgDiv.style.height = mainDiv.style.height; //On ajuste le contenu de la confirme aussi
            if (buttonsDiv)
                buttonsDiv.style.top = (newHeight - 50) + "px";
        }

        //abonnement depuis l'iframe 
        if (top.document.getElementById("frm_" + uId) != null) {
            var frm = top.document.getElementById("frm_" + uId).contentWindow;
            if (frm.onFrameSizeChange != null) {
                var mWidth = mainDiv.offsetWidth;
                var mHeight = mainDiv.offsetHeight;
                frm.onFrameSizeChange(mWidth, mHeight);
            }
        }
    };

    this.tabLibelleToolTip = null;  //TOOLTIP   :   conteneur de libellés de détail (pour addLibelleToolTip)

    this.InitToolTipDivContener = function () {
        mainDivLibelleToolTip = mainDiv.appendChild(document.createElement('div'));
        //mainDivLibelleToolTip.className = "pl_tt_mid_tab pl_tt_fields";

        if (!this.noButtons) {
            mainDivLibelleToolTip.style.width = (this.absWidth - CONST_TOOLTIP_ARROW_WIDTH) + "px";
        }
        else {
            mainDivLibelleToolTip.style.overflowX = 'hidden';
            mainDivLibelleToolTip.style.overflowY = 'hidden';
            mainDivLibelleToolTip.style.height = "100%"; // 30 = taille titleDiv (30) 
            mainDivLibelleToolTip.style.width = "100%";
        }
    }

    //TOOLTIP - Ajoute une ligne de contenu dans une modal de type ToolTip
    this.addLibelleToolTip = function (key, value) {
        if (this.tabLibelleToolTip == null) {
            this.InitToolTipDivContener();
            this.tabLibelleToolTip = mainDivLibelleToolTip.appendChild(document.createElement('table'));
        }
        if (this.tabLibelleToolTip != null) {
            var mainTabTR = this.tabLibelleToolTip.appendChild(document.createElement("tr"));
            var mainTabTD = mainTabTR.appendChild(document.createElement("td"));
            mainTabTD.appendChild(document.createTextNode(key));
            mainTabTD = mainTabTR.appendChild(document.createElement("td"));
            mainTabTD.innerHTML = value;
        }
    };

    this.buttonTabTr = null;    //TOOLTIP   :   conteneur de boutons (pour addButtonToolTip)
    //TOOLTIP - Ajoute une ligne de contenu dans une modal de type ToolTip
    this.addButtonToolTip = function (toolTipTag, cssClass, jsAction) {
        if (this.buttonTabTr == null) {
            var buttonTab = mainDiv.appendChild(document.createElement('table'));
            buttonTab.setAttribute("cellpadding", "0");
            buttonTab.setAttribute("cellspacing", "0");
            buttonTab.className = "pl_tt_bot_icon";
            this.buttonTabTr = buttonTab.appendChild(document.createElement("tr"));
        }
        if (this.buttonTabTr != null) {
            var mainTabTD = this.buttonTabTr.appendChild(document.createElement("td"));
            mainTabTD.className = cssClass;
            mainTabTD.setAttribute("onclick", jsAction);
            mainTabTD.appendChild(document.createTextNode(" "));
        }
    };

    this.ToolbarButtonType =
    {
        PrintButton: 0,
        DeleteButton: 1,
        PropertiesButton: 2,
        MandatoryButton: 3,
        PjButton: 4,
        SendMailButton: 5,
        DeleteCalendarButton: 6,
        CancelLastValuesButton: 7
        /*TODO - Compléter en cas de besoin*/
    };

    // Crée tous les boutons de barre d'outils susceptibles d'être affichés sur les fichiers de type Template en popup (dont Planning)
    this.addTemplateButtons = function (nTab, fileid, isCalendar, openSerie) {
        // #54537 : Pas de "Propriétés de la fiche" pour la fiche Utilisateur
        if (![TAB_USER, TAB_PJ].includes(nTab))
            this.addToolbarButton("ToolbarButton_" + this.ToolbarButtonType.PropertiesButton + "_" + this.iframeId, top._res_54, top._res_54, "iProp", "iProp", "iProp iconDef_" + nTab, "iProp", true, true, 'left', function () { that.onToolBarClick(that.ToolbarButtonType.PropertiesButton + "_" + this.iframeId, nTab, fileid); });

        this.addToolbarButton("ToolbarButton_" + this.ToolbarButtonType.MandatoryButton + "_" + this.iframeId, "* " + top._res_6304, top._res_6304, "iMnd", "iMnd", "", "iMnd", true, false, 'right', null);

        if (![TAB_PJ].includes(nTab)) {
            var btnCancel = this.addToolbarButton("ToolbarButton_" + this.ToolbarButtonType.CancelLastValuesButton + "_" + this.iframeId, " ", top._res_8223, "btnCancelLastModif", "icon-undo", "", "btnCancelLastModif", true, false, 'right', null);
            btnCancel.onmouseover = (function (oModalDialog) {
                return function () {
                    var frame = oModalDialog.getIframe();
                    if (frame)
                        frame.LastValuesManager.openContextMenu(this, nTab, frame.arrLastValues, new eContextMenu(frame.LastValuesManager.menuWidth, -999, -999, null, null, "lastvalues_contextmenu"), true);
                }

            })(this);

            this.addToolbarButton("ToolbarButton_" + this.ToolbarButtonType.PjButton + "_" + this.iframeId, "(0)", top._res_5042, "", null, "icon-annex", null, true, true, 'right', function () { that.onToolBarClick(that.ToolbarButtonType.PjButton + "_" + this.iframeId, nTab, fileid); });

            //Pas d'impression sur fiche en cours de créa
            if (typeof (fileid) != "undefined" && fileid)
                this.addToolbarButton("ToolbarButton_" + this.ToolbarButtonType.PrintButton + "_" + this.iframeId, top._res_13, top._res_13, "", null, "icon-print2", null, false, true, 'right', function () { that.onToolBarClick(that.ToolbarButtonType.PrintButton + "_" + this.iframeId, nTab, fileid); });

        }


        // TODO #29 959 - Fonctionnalité "Envoi par e-mail" à coder
        //this.addToolbarButton("ToolbarButton_" + this.ToolbarButtonType.SendMailButton+ "_" + this.iframeId, top._res_1390, top._res_1390, "iMl", null, null, null, false, true, 'right', function () { that.onToolBarClick(that.ToolbarButtonType.SendMailButton+ "_" + this.iframeId, nTab, fileid); });

        if (typeof (isCalendar) == 'undefined')
            isCalendar = false;
        if (typeof (openSerie) == 'undefined')
            openSerie = false;

        if (this.CallFrom != CallFromDuplicate && this.fileId != 0) {
            if (isCalendar) {
                this.addToolbarButton("ToolbarButton_" + this.ToolbarButtonType.DeleteCalendarButton + "_" + this.iframeId, top._res_19, top._res_19, "", null, "icon-delete", null, false, true, 'right', function () { that.onToolBarClick(that.ToolbarButtonType.DeleteCalendarButton + "_" + this.iframeId, nTab, fileid, openSerie); });
            }
            else {
                this.addToolbarButton("ToolbarButton_" + this.ToolbarButtonType.DeleteButton + "_" + this.iframeId, top._res_19, top._res_19, "", null, "icon-delete", null, false, true, 'right', function () { that.onToolBarClick(that.ToolbarButtonType.DeleteButton + "_" + this.iframeId, nTab, fileid, false); });
            }
        }
    };

    this.onToolBarClick = function (btn, nTab, fileId, openSerie) {
        var frameModal = this.getIframe();
        var nBtn = parseInt(btn);
        switch (nBtn) {
            case this.ToolbarButtonType.PrintButton:
                if (typeof (frameModal.onPrintButton) == "function")
                    frameModal.onPrintButton(nTab, fileId);
                break;
            case this.ToolbarButtonType.DeleteButton:
                if (typeof (frameModal.onDeleteButton) == "function")
                    frameModal.onDeleteButton(nTab, fileId, this);
                break;
            case this.ToolbarButtonType.DeleteCalendarButton:
                if (typeof (frameModal.onDeleteCalendarButton) == "function")
                    frameModal.onDeleteCalendarButton(nTab, fileId, this, openSerie);
                break;
            case this.ToolbarButtonType.PropertiesButton:
                if (typeof (frameModal.onPropertiesButton) == "function")
                    frameModal.onPropertiesButton(nTab, fileId);
                break;
            case this.ToolbarButtonType.MandatoryButton:
                //Aucun traitement
                break;
            case this.ToolbarButtonType.PjButton:
                if (typeof (frameModal.onPjButton) == "function")
                    frameModal.onPjButton("ToolbarButton_" + this.ToolbarButtonType.PjButton + "_" + this.iframeId + "_text");
                break;
            case this.ToolbarButtonType.SendMailButton:
                if (typeof (frameModal.onSendMailButton) == "function")
                    frameModal.onSendMailButton('', nTab);
                break;


        }
    };

    this.setButtonLabel = function (nBtn, lbl) {
        if (document.getElementById("ToolbarButton_" + nBtn + "_" + this.iframeId + "_text"))
            document.getElementById("ToolbarButton_" + nBtn + "_" + this.iframeId + "_text").innerHTML = lbl;
    };

    this.setToolBarVisible = function (strButtons, bVisible) {
        strButtons = strButtons.toString();
        var aButtons = strButtons.split(';');

        //On affiche les boutons demandés
        for (var i = 0; i < aButtons.length; i++) {
            var strButton = aButtons[i];
            if (strButton && strButton != "") {
                var oToolBarBtn = top.document.getElementById("ToolbarButton_" + strButton + "_" + this.iframeId);
                if (oToolBarBtn) {
                    if (bVisible) {
                        oToolBarBtn.style.display = "block";
                        oToolBarBtn.style.visibility = "visible";
                    } else {
                        oToolBarBtn.style.display = "none";
                        oToolBarBtn.style.visibility = "hidden";
                    }
                }
            }
        }
    };

    ///summary
    ///Retourne la fenêtre modale parente de la fenêtre modale en cours, si existante
    ///On se base sur le tableau global référençant toutes les modal dialogs, pour vérifier si l'objet Window d'une modal dialog correspond, ou non, à celui stocké dans
    ///la propriété myOpenerWin de la modal dialog en cours. Auquel cas, on considère que la modale actuellement examinée est bien la parente de celle en cours
    ///summary
    this.getParentModalDialog = function () {
        try {
            if (!top.ModalDialogs)
                return null;

            for (var modalUID in top.ModalDialogs) {
                var modalDialog = top.ModalDialogs[modalUID];
                if (modalDialog && typeof (modalDialog.getIframe) != "undefined" && modalDialog.getIframe() == this.myOpenerWin) {
                    return modalDialog;
                }
            }

            return null;
        }
        catch (ex) {
            return null;
        }
    };
}

function eModalButton(sLabel, fctAction, sCss) {
    this.label = sLabel;
    this.fctAction = fctAction;
    this.css = sCss;
}

function getDocWidth() {
    var nWidth = 0;

    try {
        nWidth = top.getWindowWH()[0]
    }
    catch (eeee) {

    }
    if (nWidth <= 0) {
        var D = top.document;
        nWidth = Math.max(
            Math.max(D.body.scrollWidth, D.documentElement.scrollWidth),
            Math.max(D.body.offsetWidth, D.documentElement.offsetWidth),
            Math.max(D.body.clientWidth, D.documentElement.clientWidth)
        );
    }
    return nWidth;
}

function getDocHeight() {
    var nHeight = 0;
    try {
        nHeight = top.getWindowWH()[1]
    }
    catch (eeee) {

    }
    if (nHeight <= 0) {
        var D = top.document;
        nHeight = Math.max(
            Math.max(D.body.scrollHeight, D.documentElement.scrollHeight),
            Math.max(D.body.offsetHeight, D.documentElement.offsetHeight),
            Math.max(D.body.clientHeight, D.documentElement.clientHeight)
        );
    }
    return nHeight;
}

// #58 123 : Calcul de la taille totale du document, scroll compris
// Permet d'afficher le calque sur toute la surface et non pas seulement sur la surface visible avant scroll
// Source : https://stackoverflow.com/questions/1145850/how-to-get-height-of-entire-document-with-javascript
// Auteur : Andrii Verbytskyi - https://stackoverflow.com/users/2768917/andrii-verbytskyi
function getMaxDocWidth(currentWidth, nodesList) {
    if (!currentWidth)
        currentWidth = 0;

    if (!nodesList)
        nodesList = document.documentElement.childNodes;

    for (var i = nodesList.length - 1; i >= 0; i--) {
        if (nodesList[i].scrollWidth && nodesList[i].clientWidth) {
            var elWidth = Math.max(nodesList[i].scrollWidth, nodesList[i].clientWidth);
            currentWidth = Math.max(elWidth, currentWidth);
        }
        if (nodesList[i].childNodes.length)
            currentWidth = getMaxDocWidth(currentWidth, nodesList[i].childNodes);
    }

    return currentWidth;
}

function getMaxDocHeight(currentHeight, nodesList) {

    if (!currentHeight)
        currentHeight = 0;

    if (!nodesList)
        nodesList = document.documentElement.childNodes;

    for (var i = nodesList.length - 1; i >= 0; i--) {

        if (nodesList[i].scrollHeight && nodesList[i].clientHeight) {
            var elHeight = Math.max(nodesList[i].scrollHeight, nodesList[i].clientHeight);
            currentHeight = Math.max(elHeight, currentHeight);
        }
        if (nodesList[i].childNodes.length) {
            currentHeight = getMaxDocHeight(currentHeight, nodesList[i].childNodes);
        }
    }

    return currentHeight;
}

//Drag&Drop fenêtres
var modalId = "";
var dragEnabled = false;
var resizeEnabled = false;
var dashedDiv;
var bgDashedDiv;
var divCtnr;
var divCtnrInitOpacity = null;
var divMove;

//position de la souris pour mémorisation
var xMouseMem = 0;
var yMouseMem = 0;

var winWidth = 0;
var winHeight = 0;
var aWH = [200, 200];
if (typeof (top.getWindowWH) == "function") {
    aWH = top.getWindowWH();
}
else if (typeof (getWindowWH) == "function") {
    aWH = getWindowWH();
}

winWidth = aWH[0];
winHeight = aWH[1];

function doOnmouseDownModal(e) {
    oldMouseMove = top.document.onmousemove;
    oldMouseUp = top.document.onmouseup;

    top.document.onmouseup = onmouseUpModal;
    top.document.onmousemove = onmouseMoveModal;

    if (e && e.target)
        modalId = e.target.id.replace("td_title_", "");
    else
        modalId = window.event.srcElement.id.replace("td_title_", "");

    divCtnr = top.document.getElementById("ContainerModal_" + modalId);
    // Mémorisation de l'opacité initiale de la fenêtre
    if (divCtnr && divCtnrInitOpacity == null)
        divCtnrInitOpacity = divCtnr.style.opacity;

    if (dashedDiv == null) {
        dashedDiv = top.document.createElement("div");
        dashedDiv.id = "dashedDiv";
        top.document.body.appendChild(dashedDiv);
    }

    if (divMove == null) {
        divMove = top.document.createElement("div");
        divMove.id = "divMove";
        top.document.body.appendChild(divMove);
    }

    dashedDiv.style.top = divCtnr.style.top;
    dashedDiv.style.left = divCtnr.style.left;
    dashedDiv.style.width = divCtnr.style.width;
    dashedDiv.style.height = divCtnr.style.height;
    dashedDiv.style.position = 'absolute';
    dashedDiv.style.border = '2px dashed gray';
    dashedDiv.style.display = "block";

    dashedDiv.style.zIndex = parseInt(divCtnr.style.zIndex) + 1;
    dashedDiv.style.cursor = "move";


    divMove.style.top = "0px";
    divMove.style.left = "0px";
    divMove.style.width = "100%";
    divMove.style.height = "100%";
    divMove.style.position = 'absolute';

    divMove.style.display = "block";
    divMove.style.zIndex = parseInt(divCtnr.style.zIndex) + 2;




    dragEnabled = true;

    if (!e)
        var e = window.event;
    x = e.clientX;
    y = e.clientY;

    //MOU on initilise la position de la souris
    if (xMouseMem == 0 && yMouseMem == 0) {
        //au premier click on sauvegarde les coordonnées de la souris.
        xMouseMem = x;
        yMouseMem = y;
    }

    // Bonus : Si on appuie sur Shift/Maj tout en déplaçant la fenêtre, celle-ci devient semi-transparente
    // afin de pouvoir lire le contenu qui se trouve sur la fenêtre parente.
    if (e.shiftKey)
        divCtnr.style.opacity = 0.3;
    else
        divCtnr.style.opacity = divCtnrInitOpacity;
}

function onmouseUpModal() {
    try {
        dragEnabled = false;

        top.document.onmouseup = oldMouseUp;
        top.document.onmousemove = oldMouseMove;

        var iTop = parseInt(dashedDiv.style.top);
        var iLeft = parseInt(dashedDiv.style.left);
        /*
        if (iTop < 0)
            divCtnr.style.top = "0px";
        else if (iTop + divCtnr.clientHeight > winHeight)
            divCtnr.style.top = (winHeight - divCtnr.clientHeight) + "px";
        else
        */
        divCtnr.style.top = dashedDiv.style.top;
        /*
        if (iLeft < 0)
            divCtnr.style.left = "0px";
        else if (iLeft + divCtnr.clientWidth > winWidth)
            divCtnr.style.top = (winWidth - divCtnr.clientWidth) + "px";
        else
        */
        divCtnr.style.left = dashedDiv.style.left;


        dashedDiv.style.display = "none";
        divMove.style.display = "none";

        //remise à zéro de la position sauvegardée de la souris (on en a plus besoin)
        xMouseMem = 0;
        yMouseMem = 0;

        // Remise à zéro de l'opacité de la fenêtre
        divCtnr.style.opacity = divCtnrInitOpacity;
    }
    catch (exp) { }
}

function onmouseMoveModal(e) {
    //Calcul des coordonnées
    if (!e)
        var e = window.event;

    var mouse = getClickPositionXY(e);
    x = mouse[0];
    y = mouse[1];





    if (dragEnabled == true) {
        divCtnr = top.document.getElementById("ContainerModal_" + modalId);


        if (divCtnr != null) {

            // La fenêtre peut être déplacée  en dehors de l'écran, à gauche, à droite et vers le bas (PAS VERS LE HAUT) 
            // avec un coefficient de visibilité
            // 1.0 : la fenêtre ne peut etre masquée
            // 0.5 : au max, la moitié de la fenêtre est masquée 
            // 0.0 : la fenêtre peut être masquée entièrement (pas recommandé)       
            var VISIBLE_WIN_COEF = 0.33;


            //Si on fait un dragndrop sur un element vers l'exterieur de l'ecran, on désactive le move pour eviter que IE agrandit le viewport
            if (y + divCtnr.clientHeight * VISIBLE_WIN_COEF > winHeight || x > winWidth) {

                var fctToRemove = divCtnr.onmousemove;
                divCtnr.removeEventListener("mousemove", fctToRemove);
                onmouseUpModal(e);
                stopEvent(e);
                dragEnabled = false;
                return;
            }

            //MOU On calcule les distances deltaX et deltaY entre l' ancienne et la nouvelle position de la souris 
            var deltaX = xMouseMem - x;
            var deltaY = yMouseMem - y;


            //on déplace le dashedDiv en tenant compte des déplacement deltaX et deltaY
            var dashedTop = parseInt(parseInt(dashedDiv.style.top) - deltaY);
            if (dashedTop >= 0 && (dashedTop + divCtnr.clientHeight * VISIBLE_WIN_COEF < winHeight))
                dashedDiv.style.top = dashedTop + "px";

            var dashedLeft = parseInt(parseInt(dashedDiv.style.left) - deltaX)
            if ((dashedLeft + divCtnr.clientWidth * (1 - VISIBLE_WIN_COEF) > 0) && (dashedLeft + divCtnr.clientWidth * VISIBLE_WIN_COEF < winWidth))
                dashedDiv.style.left = dashedLeft + "px";

            // Pour ne pas déclancher l'évenement de sélection sur certains champs
            stopEvent(e);

            //on mémorise les nouvelles coordonées
            xMouseMem = x;
            yMouseMem = y;

            // Bonus : Si on appuie sur Shift/Maj tout en déplaçant la fenêtre, celle-ci devient semi-transparente
            // afin de pouvoir lire le contenu qui se trouve sur la fenêtre parente.
            if (e.shiftKey)
                divCtnr.style.opacity = 0.3;
            else
                divCtnr.style.opacity = divCtnrInitOpacity;

        } else {
            dragEnabled = false;

        }
    }
}

function resizeModal(uid) {
    oldMouseMove = top.document.onmousemove;
    oldMouseUp = top.document.onmouseup;

    divCtnr = top.document.getElementById("ContainerModal_" + uid);

    top.document.onmouseup = onmouseUpResize;
    top.document.onmousemove = onmouseMoveResize;

    if (bgDashedDiv == null) {
        bgDashedDiv = top.document.createElement("div");
        bgDashedDiv.style.position = "absolute";
        bgDashedDiv.id = "bgDashedDiv";
        top.document.body.appendChild(bgDashedDiv);

        bgDashedDiv.style.left = 0 + "px";
        bgDashedDiv.style.top = 0 + "px";
        bgDashedDiv.style.width = "100%"; // document.body.scrollWidth + "px";
        bgDashedDiv.style.height = "100%"; // getDocHeight() + "px";
        bgDashedDiv.style.backgroundColor = "gray";

        bgDashedDiv.style.opacity = (30 / 100);
        bgDashedDiv.style.MozOpacity = (30 / 100);
        bgDashedDiv.style.KhtmlOpacity = (30 / 100);
        bgDashedDiv.style.filter = "alpha(opacity=" + 30 + ")";

        bgDashedDiv.style.zIndex = parseInt(divCtnr.style.zIndex) + 1;
    }

    if (dashedDiv == null) {
        dashedDiv = top.document.createElement("div");
        dashedDiv.style.position = "absolute";
        dashedDiv.id = "dashedDiv";
        top.document.body.appendChild(dashedDiv);
        dashedDiv.style.position = 'absolute';
        dashedDiv.style.border = '2px dashed gray';
        dashedDiv.style.zIndex = parseInt(divCtnr.style.zIndex) + 2;
    }
    bgDashedDiv.style.display = "block";
    dashedDiv.style.display = "block";
    resizeEnabled = true;

    dashedDiv.style.top = divCtnr.style.top;
    dashedDiv.style.left = divCtnr.style.left;
    dashedDiv.style.width = divCtnr.style.width;
    dashedDiv.style.height = divCtnr.style.height;


}

function onmouseUpResize() {
    top.document.onmouseup = oldMouseUp;
    top.document.onmousemove = oldMouseMove;
    dashedDiv.style.display = "none";
    resizeEnabled = false;
    bgDashedDiv.style.display = "none";
    divCtnr.style.width = dashedDiv.style.width;
    divCtnr.style.height = dashedDiv.style.height;
}

function onmouseMoveResize(e) {

    //Calcul des coordonnées
    if (!e)
        var e = window.event;
    x = e.clientX;
    y = e.clientY;
    window.status = "x:" + (x - parseInt(divCtnr.style.left)) + "px      + y=" + (y - parseInt(divCtnr.style.top)) + "px";

    dashedDiv.style.width = (x - parseInt(divCtnr.style.left)) + "px";
    dashedDiv.style.height = (y - parseInt(divCtnr.style.top)) + "px";
}


var ModalEudoType =
{
    WIZARD: 0,

    LIST: 1,
    CHART: 2,
    SELECTFIELD: 3,
    SELECTTAB: 4,

    UNSPECIFIED: 99
};


var MsgType =
{
    MSG_CRITICAL: 0,
    MSG_QUESTION: 1,
    MSG_EXCLAM: 2,
    MSG_INFOS: 3,
    MSG_SUCCESS: 4
};