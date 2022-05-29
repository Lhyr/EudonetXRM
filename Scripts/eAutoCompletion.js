/// <reference path="eTools.js" />
/*****
 *    Recherche prédictive uniquement pour le mode fiche
 ******/
var oAutoCompletion = (function () {

    // Constantes
    var SEARCH_INTERVAL_SIRENE = 500; // pour Sirene : limite en dur utilisée en 312
    var SEARCH_INTERVAL_OPENDATAGOUVFR = 0; // pour DataGouv : pas de limite en 312
    var SEARCH_INTERVAL_BINGMAPSV8 = 0; // pour Bing Mpas : pas de limite en 312

    var SEARCH_MINCHAR_SIRENE = 3; // pour Sirene : limite en dur utilisée en 312
    var SEARCH_MINCHAR_OPENDATAGOUVFR = 3; // pour DataGouv : limite en dur utilisée en 312
    var SEARCH_MINCHAR_BINGMAPSV8 = 0; // pour Bing Maps : pas de minimum de recherche en 312 (TOCHECK)

    var SEARCH_MAXRESULTS_SIRENE = 0; // pour Sirene : pas de limite en 312
    var SEARCH_MAXRESULTS_OPENDATAGOUVFR = 15; // pour DataGouv : limite en dur utilisée en 312 (dans l'URL)
    var SEARCH_MAXRESULTS_BINGMAPSV8 = 10; // pour Bing Maps : Entre 1 et 10

	// Remarque/TODO : la taille des éléments du menu ne tenant pas compte (pour l'instant) du paramètre "Taille de la police de caractère" dans "Mon Eudonet",
	// l'utilisation de ces constantes pour le positionnement du menu ne pose pas de problème pour une taille de police plus élevée
    var MENU_HEIGHT = 275; // Hauteur en PX du menu (à modifier également dans eMain.css)
    var MENURESULTITEM_HEIGHT = 24; // Hauteur en PX d'un résultat affiché dans le menu (à modifier également dans eMain.css) 

    var DEBUG = false;

    // Action manager
    var MgrAction = {
        Undefined: 0,
        GetDataGouvSuggestions: 1,
        GetAddressMapping: 2,
        GetFieldsToUpdate: 3,
        GetSireneSuggestions: 4,
        GetSireneMapping: 5
    }

    // Référentiel de l'autocomplétion
    // Le référentiel Sirene n'est pas considéré comme un remplaçant d'autres fournisseurs ; il doit pouvoir être utilisé indépendamment, champ par champ, même si un autre référentiel plus global est utilisé pour d'autres champs
    // D'où son absence de référencement dans cette enum. Il est géré et activé par un attribut séparé "sirene" dans le renderer
    // En revanche, son fonctionnement est le même que les autres fournisseurs
    var PredictiveAddressesRef = {
        OpenDataGouvFr: 0,
        BingMapsV8: 1
    }

    var menuContainer = null;
    var oAutoCompletionWaiterTime = null;
    var oAutoCompletionWaiter = null;
    var oAutoCompletionWaiterContent = null;
    var menu = null;
    var lastSearch = null;
    var autoRunNextSearch = false; // lorsque l'exécution de la recherche actuelle sera terminée, vérifier si le terme de recherche saisi correspond toujours aux résultats affichés, et sinon, relancer automatiquement
    var currentSearch = null;
    var searchTimeout = null;
    var descid = 0;
    var input = null;
    var header = null;
    var items = null;
    var oDoc = null;
    var itemIndex = -1;
    var highlightedItem = null;
    //var originValue = null;
    var nFileId = 0;
    var nTab = 0;
    var autoSuggestMgr = null

    var provider = PredictiveAddressesRef.OpenDataGouvFr;
    var aSireneFields = null;
    var sireneEnabled = false;

    // Gestion des limitations de recherche, en fonction du moteur (Sirene, DataGouv, Bing Maps) et du contexte (clic sur le champ déclencheur ou saisie)
    // Ex : intervalle avant déclenchement, nombre maximum de résultats, nombre minimum de caractères requis
    // TOCHECK: application des limitations adoptées en 312, à valider fonctionnellement
    // TODO: le paramètre maxSearchResults n'est pas pris en compte pour Sirene et DataGouv, à valider fonctionnellement
    var interval = 0; // valeur par défaut pour Bing Maps et DataGouv : pas de temporisation        
    var minSearchCharCount = 0;
    var maxSearchResults = 0;

    var setInput = function (evtOrInput) {
        // Si le champ déclencheur n'est pas initialisé, ou n'est pas un champ de saisie...
        if (!input || input.tagName.toLowerCase() != "input") {
            // On s'appuie sur le paramètre
            if (evtOrInput) {
                // Si le paramètre est passé, et correspond à un champ input, on l'utilise directement
                if (evtOrInput.tagName && evtOrInput.tagName.toLowerCase() == "input") {
                    input = evtOrInput;
                }
                    // Sinon, s'il est passé, on considère qu'il s'agit d'un objet Event et on récupère le champ source
                else {
                    input = evtOrInput.srcElement || evtOrInput.target;
                    // S'il ne s'agit toujours pas d'un champ input, on abandonne
                    if (input && input.tagName.toLowerCase() != "input") {
                        input = null;
                        return;
                    }
                }
            }
        }

        header = document.getElementById(getAttributeValue(input, "ename"));
    }

    // Fonction commune déclenchée au chargement de la fenêtre, au clic ou à la saisie
    var initSearch = function (evtOrInput, refreshContext, showMenu, useInterval, preventIdenticalSearches, checkMinLength, initOnLoad) {
        // Vérification du champ déclencheur
        setInput(evtOrInput);
        if (!input)
            return;

        // US #2 224 - Demande #82 074 - Désactivation de l'autocomplétion système/navigateur sur les champs affichant des résultats de recherche
        setAttributeValue(input, "autocomplete", "off");

        // Initialisation du contexte de recherche, uniquement si demandé ou non présent
        // L'initialisation n'est pas effectuée systématiquement, car le parcours des champs déclencheurs induit un très léger temps de latence avant apparition du menu
        if (refreshContext || nTab == 0 || descid == 0 || !aSireneFields) {
            setAttributeValue(header, "ignoreblur", "0");
            provider = getNumber(getAttributeValue(header, "data-provider"));
            descid = getAttributeValue(header, "did");
            nTab = getTabDescid(descid);
            var oFileDiv = document.getElementById("fileDiv_" + nTab);
            if (oFileDiv == null || typeof (oFileDiv) == 'undefined') {
                var oAllFileDiv = document.querySelectorAll('*[id^="fileDiv_"]');
                if (oAllFileDiv && oAllFileDiv.length > 0)
                    oFileDiv = oAllFileDiv[0];
            }
            // Réinitialisation de la valeur avant de redétecter le moteur à déclencher. Sinon, une fois déclenché au moins une fois, le moteur Sirene s'active sur tous les champs
            sireneEnabled = false;
            var aSireneFields = getAttributeValue(oFileDiv, "sirenefields").split(';');
            for (var i = 0; i < aSireneFields.length; i++) {
                if (aSireneFields[i] == descid)
                    sireneEnabled = true;
                if (sireneEnabled)
                    break;
            }
        }

        // Gestion des limitations de recherche, en fonction du moteur (Sirene, DataGouv, Bing Maps) et du contexte (clic sur le champ déclencheur ou saisie)
        // Ex : intervalle avant déclenchement, nombre maximum de résultats, nombre minimum de caractères requis
        // TOCHECK: application des limitations adoptées en 312, à valider fonctionnellement
        // TODO: le paramètre maxSearchResults n'est pas pris en compte pour Sirene et DataGouv, à valider fonctionnellement
        if (sireneEnabled) {
            if (useInterval) { interval = SEARCH_INTERVAL_SIRENE; }
            minSearchCharCount = SEARCH_MINCHAR_SIRENE;
            maxSearchResults = SEARCH_MAXRESULTS_SIRENE;
        }
        else {
            switch (provider) {
                case PredictiveAddressesRef.OpenDataGouvFr:
                    if (useInterval) { interval = SEARCH_INTERVAL_OPENDATAGOUVFR };
                    minSearchCharCount = SEARCH_MINCHAR_OPENDATAGOUVFR;
                    maxSearchResults = SEARCH_MAXRESULTS_OPENDATAGOUVFR;
                    break;
                case PredictiveAddressesRef.BingMapsV8:
                    if (useInterval) { interval = SEARCH_INTERVAL_BINGMAPSV8; }
                    minSearchCharCount = SEARCH_MINCHAR_BINGMAPSV8; // pas de limite sur Bing Maps en 312
                    maxSearchResults = SEARCH_MAXRESULTS_BINGMAPSV8;
                    break;
            }
        }

        // Il semblerait que lors de saisies trop rapides, la valeur ne soit pas encore mise à jour dans le DOM au moment où on récupère la référence à l'élément source via setInput() plus haut.
        // Par précaution, on fait donc un getElementById de l'ID de cet élément pour récupérer la toute dernière valeur réellement saisie.
        //var text = input.value;
        var text = document.getElementById(input.id).value;
        if (text != null)
            text = text.trim();

        // Vérification de la longueur minimale de la saisie si demandé
        if (checkMinLength && text.length < minSearchCharCount) {
            if (DEBUG)
                console.log("La recherche ne sera pas déclenchée car le nombre de caractères du terme à rechercher est inférieur au nombre de caractères minimum requis (" + minSearchCharCount + ").");

            deleteMenu();
            return;
        }
        else {
            // On affiche le menu avant de déclencher la recherche afin de montrer à l'utilisateur que l'action est bien prise en compte
            addOrDisplayMenu(input, !showMenu, header);
        }

        // Si le menu est masqué (ex : initialisation au focus), pas de lancement de la recherche
        if (showMenu) {
            if (sireneEnabled || provider == PredictiveAddressesRef.OpenDataGouvFr) {
                if (!sireneEnabled) {
                    if (provider == PredictiveAddressesRef.OpenDataGouvFr) {
                        search({ "text": text, 'descid': descid, 'action': MgrAction.GetDataGouvSuggestions, 'maxResults': maxSearchResults, 'token': '' }, interval, null, null, null);
                    }
                }
                else
                    // La recherche peut être redéclenchée avec les mêmes mots-clés si on appuie sur Entrée
                    if (text != '')
                        search({ "text": text, 'descid': descid, 'action': MgrAction.GetSireneSuggestions, 'maxResults': maxSearchResults, 'token': getGlobalParam('sireneToken') }, interval, true, preventIdenticalSearches, true);
            }

            // Pour Bing Maps V8, le déclenchement de la recherche se fera via le module lui-même, lors de la saisie, du fait qu'il se soit rattaché aux champs déclencheurs
            // Il dispose de ses propres évènements de recherche. On ne fait donc rien pour ce moteur ici, le menu ayant déjà été affiché plus haut afin d'accueillir les résultats
            // de recherche
        }
        else {
            if (DEBUG)
                console.log("La recherche ne sera pas déclenchée car le menu est masqué.");
        }
    }

    // Fonction au clic sur le champ
    var onClick = function (evt) {

        initSearch(evt, true, true, false, true, false, false);

        // Cas particulier pour le clic : on interrompt la propagation de l'évènement click vers ePopup.document.onclick
        // ePopup est utilisé pour afficher le menu, et définit son propre évènement onClick. Lors de la création du menu via initSearch -> addOrDisplayMenu, on a donc un évènement
        // onClick qui s'ajoute dans la pile.
        // Or, comme l'évènement onClick d'ePopup est créé alors qu'on exécute toujours le onClick() en cours sur notre champ déclencheur, le onClick d'ePopup se déclenche par
        // propagation.
        // Ce dernier considère alors qu'on a cliqué ailleurs sur la page alors que le menu est affiché (ce qui est techniquement vrai) et que, donc, le menu doit être fermé
        // (cf. cas particulier si eAutoCompletion existe dans ePopup.document.onclick)
        // Il faut donc interrompre la propagation de l'évènement lors du clic sur le champ déclencheur pour ne pas provoquer la fermeture immédiate du menu lors de son tout premier
        // affichage (le cas ne se reproduit plus lors des clics suivants, puisque l'objet ePopup existe déjà, l'évènement n'est pas redéfini).
        stopEvent(evt);
    }

    // Fonction à la saisie dans le champ
    var onKeyUp = function (evt) {
        initSearch(evt, false, true, true, evt.keyCode != 13, true, false);
    }

    // Cache le menu
    var hideMenu = function () {
        //ELAIZ : remplacement menu par menuContainer pour masquer le conteneur
        if (menu) {
            menuContainer.style.display = "none";
        }
    }

    // Récupère une valeur stockée dans eParamIFrame
    function getGlobalParam(keyName) {
        var value = '';
        var oeParam = top.document.getElementById('eParam').contentWindow;
        if (typeof (oeParam) != "undefined" && oeParam && typeof (oeParam.GetParam) == "function" && oeParam.GetParam(keyName) != '')
            value = oeParam.GetParam(keyName);
        return value;
    }
    // Met à jour une valeur stockée dans eParamIFrame
    function setGlobalParam(keyName, value) {
        var oeParam = top.document.getElementById('eParam').contentWindow;
        if (typeof (oeParam) != "undefined" && oeParam) {
            var container = oeParam.document.getElementById("GLOBAL");
            if (typeof (oeParam.AddOrUpdInput) == "function")
                oeParam.AddOrUpdInput(container, keyName, value);
        }
    }

    // Recherche (data.gouv.fr)
    var search = function (args, interval, preventSimultaneousSearches, preventIdenticalSearches, displayWaiter) {

        if (!args.text || args.text == "") {
            if (DEBUG)
                console.log("oAutoCompletion.search : pas de texte à rechercher. Affichage du menu");
            addOrDisplayMenu(input, false, header);
            return;
        }

        if (preventSimultaneousSearches && currentSearch && currentSearch != "") {
            if (DEBUG)
                console.log("oAutoCompletion.search : une recherche est déjà en cours - " + currentSearch);
            autoRunNextSearch = true;
            addOrDisplayMenu(input, false, header);
            return;
        }

        // On vérifie que la recherche affichée dans le menu soit celle réellement enregistrée par le JS lors de la dernière recherche
        // Si ça n'est pas le cas, c'est le contenu du menu affiché qui fait foi
        var menuLastSearch = getAttributeValue(menu, "lastsearch")
        if (lastSearch && lastSearch != menuLastSearch) {
            if (DEBUG)
                console.log("oAutoCompletion.search : la dernière recherche mémorisée (" + lastSearch + ") ne correspond pas à celle affichée dans le menu (" + menuLastSearch + "). Mise à jour de la dernière recherche mémorisée.");
            lastSearch = menuLastSearch;
        }

        if (preventIdenticalSearches && args.text == lastSearch) {
            if (DEBUG)
                console.log("oAutoCompletion.search : le texte à rechercher n'a pas été modifié - " + lastSearch);
            addOrDisplayMenu(input, false, header);
            return;
        }

        var updater = new eUpdater("mgr/eAutoCompletionManager.ashx");
        updater.addParam("search", args.text, "post");
        updater.addParam("descid", args.descid, "post");
        updater.addParam("action", args.action, "post");
        updater.addParam("token", args.token, "post");
        updater.addParam("fromSirene", (args.action == MgrAction.GetSireneSuggestions ? "1" : "0"), "post");
        var runSearch = function () {
            if (displayWaiter)
                setWait(true);
            currentSearch = args.text;
            if (DEBUG)
                console.log("oAutoCompletion.search - Démarrage de la recherche suivante : " + currentSearch + " (heure : " + new Date() + ")");
            updater.send(updateMenu);
        };

        if (interval && interval > 0) {
            if (DEBUG)
                console.log("oAutoCompletion.search - Démarrage de la recherche dans " + interval + " millisecondes");
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(runSearch, interval);
        }
        else
            runSearch();
    }


    // Met à jour le menu contextuel en fonction des données récupérées de Data.gouv.fr
    var updateMenu = function (oRes) {

        if (DEBUG)
            console.log("oAutoCompletion.updateMenu - Recherche terminée : " + currentSearch + " (heure : " + new Date() + ")");

        lastSearch = currentSearch;
        currentSearch = "";

        setWait(false);

        oDoc = oRes;

        if (input == null)
            return;

        addOrDisplayMenu(input, false, header);
        setAttributeValue(menu, "lastsearch", lastSearch);

        if (autoRunNextSearch == true && lastSearch != document.getElementById(input.id).value) {
            autoRunNextSearch = false;
            if (DEBUG)
                console.log("La dernière recherche (" + lastSearch + ") ne correspond plus à la saisie actuelle (" + document.getElementById(input.id).value + "). Lancement d'une nouvelle recherche...");
            input.click();
            return;
        }
        autoRunNextSearch = false;

        if (oRes == null)
            return;

        var success = getXmlTextNode(oDoc.getElementsByTagName("success")[0]);
        var error = getXmlTextNode(oDoc.getElementsByTagName("error")[0]);
        var debug = getXmlTextNode(oDoc.getElementsByTagName("debug")[0]);

        var token = getXmlTextNode(oDoc.getElementsByTagName("token")[0]);
        var count = getXmlTextNode(oDoc.getElementsByTagName("count")[0]);
        var noResults = getXmlTextNode(oDoc.getElementsByTagName("noResults")[0]);
        var searchInfo = getXmlTextNode(oDoc.getElementsByTagName("searchInfo")[0]);

        if (token != "") {
            setGlobalParam('sireneToken', token);
        }

        if (success == "0" || error != "" || debug != "") {
            var searchErrorValues = new Array();
            searchErrorValues[0] = error;
            var searchDebugValues = new Array();
            searchDebugValues[0] = debug;

            RemoveOldItems();
            addMenuItem({
                'id': 'searchError',
                'values': searchErrorValues,
                'tooltip': error,
                'badge': '',
                'badgeClass': '',
                'className': "searchError",
                'srcHeader': header,
                'clickable': false
            });
            if (DEBUG)
                addMenuItem({
                    'id': 'searchErrorDebug',
                    'values': searchDebugValues,
                    'tooltip': debug,
                    'badge': '',
                    'badgeClass': '',
                    'className': "searchErrorDebug",
                    'srcHeader': header,
                    'clickable': false
                });

            return;
        }

        var searchInfoValues = new Array();
        if (searchInfo != "")
            searchInfoValues[0] = searchInfo;

        if (count == "0") {
            if (!noResults || noResults == '')
                return;
            else {
                var values = new Array();
                values[0] = noResults;
                items = oDoc.getElementsByTagName("line");
                if (items != null) {
                    RemoveOldItems();
                    addMenuItem({
                        'id': 'searchInfo',
                        'values': searchInfoValues,
                        'tooltip': searchInfo,
                        'badge': '',
                        'badgeClass': '',
                        'className': "searchInfo",
                        'srcHeader': header,
                        'clickable': false
                    });
                    addMenuItem({
                        'id': 'noResults',
                        'values': values,
                        'tooltip': noResults,
                        'badge': '',
                        'badgeClass': '',
                        'className': "searchNoResults",
                        'srcHeader': header,
                        'clickable': false
                    });
                }
            }
        }
        else {
            items = oDoc.getElementsByTagName("line");
            if (items != null) {
                RemoveOldItems();
                addMenuItem({
                    'id': 'searchInfo',
                    'values': searchInfoValues,
                    'tooltip': searchInfo,
                    'badge': '',
                    'badgeClass': '',
                    'className': "searchInfo",
                    'srcHeader': header,
                    'clickable': false
                });
                var item;
                for (var i = 0; i < items.length; i++) {
                    item = items[i];
                    var valueItems = item.getElementsByTagName("values");
                    if (valueItems != null)
                        valueItems = valueItems[0]; // récupération du noeud racine "values"
                    var values = new Array();
                    if (valueItems) {
                        for (var j = 0; j < valueItems.childNodes.length; j++)
                            values[values.length] = getXmlTextNode(valueItems.childNodes[j]);
                    }
                    var tooltip = item.getElementsByTagName("tooltip");
                    if (tooltip != null)
                        tooltip = tooltip[0];

                    addMenuItem({
                        'id': getAttributeValue(item, "key"),
                        'badge': getAttributeValue(item, "badge"),
                        'badgeClass': getAttributeValue(item, "badgeClass"),
                        'values': values,
                        'tooltip': getXmlTextNode(tooltip),
                        'className': i % 2 == 0 ? "search1" : "search2",
                        'srcHeader': header,
                        'clickable': true
                    });
                }
            }
        }
    }

    /// Affiche/Masque le div d'attente
    var setWait = function (bOn) {

        // A chaque fois que l'on rafraîchit la page, si la surveillance des mouvements sur tablette est activée,
        // on met à jour un booléen indiquant de ne pas déclencher les fonctions liées à la détection des mouvements
        // pour ne pas qu'elles se déclenchent en boucle tant que le traitement n'est pas terminé
        //if (window.DeviceOrientationEvent) {
        //    window.preventTabletMoveFunctions = bOn;
        //}

        var maxZIndex = 100;	//zIndex min à 100 pour iso à eModal
        if (oAutoCompletionWaiter) {
            addOrDelAttributeValue(oAutoCompletionWaiter, "caller", name, bOn, true);

            if (bOn) {
                clearTimeout(oAutoCompletionWaiterTime);

                switchClass(oAutoCompletionWaiter, "waitOff", "waitOn");
                oAutoCompletionWaiterContent.style.display = "";

                var zIndex = GetMaxZIndex(menuContainer, maxZIndex) + 1;
                oAutoCompletionWaiter.style.zIndex = zIndex;
            }
            else {
                oAutoCompletionWaiterTime = setTimeout(function () {
                    switchClass(oAutoCompletionWaiter, "waitOn", "waitOff");
                    oAutoCompletionWaiterContent.style.display = "none";
                }, 1);
            }
        }
    }

    var RemoveOldItems = function () {
        if (menu == null)
            return;

        itemIndex = -1;
        highlightedItem = null;

        menu.innerHTML = "";// "<span class='search-beta'>BETA</span><span class='provider-info color-theme'>data.gouv.fr</span>";
        //setAttributeValue(menu, "class", 'border-theme');
    }

    // Création du menu si inexistant, et/ou affichage
    // bHidden à true pour créer le menu caché
    var addOrDisplayMenu = function (input, bHidden, header) {

        // Si le menu est toujours existant aux yeux du JS mais absent du DOM, on le recrée
        if (menu != null) {
            if (document.getElementById(menu.id) == null) {
                menu = null;
                if (DEBUG)
                    console.log("oAutoCompletion.addOrDisplayMenu : le menu a déjà été affiché par le passé, mais ne se trouve plus rattaché sur la page. Il va être recréé.");
            }
            else if (menuContainer != null && this.input && this.input != input) {
                menu = null;
                if (DEBUG)
                    console.log("oAutoCompletion.addOrDisplayMenu : le menu a été précédemment affiché pour un champ déclencheur différent. Il va être recréé.");
            }
        }

        if (menu == null) {
            if (menuContainer != null && menuContainer.parentElement != null)
                menuContainer.parentElement.removeChild(menuContainer);

            menuContainer = document.createElement("div");

            var inputParent = input.parentElement;
            inputParent.appendChild(menuContainer);

            // #67 882 et #75 581 - Ajustement de la taille et de la position du menu selon contexte
			// Remarque/TODO : la taille des éléments du menu ne tenant pas compte (pour l'instant) du paramètre "Taille de la police de caractère" dans "Mon Eudonet",
			// l'utilisation de ces constantes pour le positionnement du menu ne pose pas de problème pour une taille de police plus élevée
            var inputParentPosition = getAbsolutePosition(inputParent);
            var nRequiredSpace = inputParentPosition.y + MENU_HEIGHT;
            var nAvailableSpace = getWindowSize().h;
            // Si on se trouve en signet (ex : Adresses depuis Contacts), on a un conteneur avec ascenseur
            // Il faut donc tenir explicitement compte de ce cas particulier
            // Même chose si on est en popup
            var oBkmContainer = document.getElementById("divBkmPres");
            var oMainDivContainer = document.getElementById("mainDiv");
            var subContainerPosition = null;
            var bIsFromBkm = oBkmContainer && isParentElementOf(inputParent, oBkmContainer);
            var bIsFromPopup = isPopup() && oMainDivContainer && isParentElementOf(inputParent, oMainDivContainer);
            if (bIsFromBkm || bIsFromPopup) {
                inputParentPosition = getAbsolutePosition(inputParent, true); // true pour prise en compte de la position relative
                subContainerPosition = bIsFromBkm ? getAbsolutePosition(oBkmContainer) : getAbsolutePosition(oMainDivContainer); // on ne tient pas compte d'une position relative, on veut le positionnement absolu du conteneur dans la page
                nRequiredSpace = inputParentPosition.y + MENU_HEIGHT;
                nAvailableSpace = subContainerPosition.h;
                if (DEBUG)
                    console.log("Le menu semble être affiché dans" + (bIsFromBkm ? " un signet" : "") + (bIsFromPopup ? " une fenêtre modale" : "") + ". Calcul de l'espace disponible en fonction de la taille du conteneur de signets.");
            }
            // On considère que le menu doit faire au minimum 2 fois la hauteur du champ
            // déclencheur (on ne peut pas mesurer la hauteur à partir d'une ligne de résultat,
            // car à ce stade de l'affichage du menu, les résultats ne sont pas encore affichés
            // et donc indisponibles dans le DOM pour examen)
            var nMinimalMenuHeight = inputParentPosition.h * 2;

            // #79 775 - On considère également qu'il faut appliquer une hauteur maximale, correspondant au nombre de résultats maximum affichable selon le fournisseur
            var nMaximalMenuHeight = nAvailableSpace;
            var nResultItemHeight = MENURESULTITEM_HEIGHT;
            var menuResultItem = document.querySelector("#autocompletemenu li");
            if (menuResultItem) {
                nResultItemHeight = getNumber(getComputedStyle(menuResultItem).height);
            }
            if (maxSearchResults > 0)
                nMaximalMenuHeight = nResultItemHeight * maxSearchResults;

			// Si on estime qu'il n'y a pas assez d'espace pour afficher le menu sous
			// le champ déclencheur, on l'affiche au-dessus avec un décalage (bottom)
			// pour ne pas masquer le champ déclencheur lui-même
            var bRevertMenuOrientation = nRequiredSpace > nAvailableSpace;
            
            // Informations de debug dans la console si DEBUG activé
            if (DEBUG) {
                console.log('Position absolue (y) du champ parent : ' + inputParentPosition.y);
                if (subContainerPosition)
                    console.log('Position absolue (y) du sous-conteneur parent : ' + subContainerPosition.y);
                console.log('Hauteur du champ parent : ' + inputParentPosition.h);
                console.log('Hauteur par défaut du menu : ' + MENU_HEIGHT);
                console.log('Hauteur minimale du menu : ' + nMinimalMenuHeight);
                console.log('Hauteur maximale du menu : ' + nMaximalMenuHeight);
                console.log('Espace nécessaire pour affichage : ' + nRequiredSpace);
                console.log('Hauteur disponible pour affichage : ' + nAvailableSpace);
                console.log('Affichage du menu inversé : ' + bRevertMenuOrientation);
            }

			// Affichage en mode inversé si manque de place (menu au-dessus du champ)
            var nCorrectedMenuHeight = MENU_HEIGHT;
            if (bRevertMenuOrientation) {
            	// En affichage inversé, on positionne le menu par rapport au parent
                inputParent.style.overflow = "visible";
                inputParent.style.position = "relative";
                // Si on passe en positionnement relatif et en affichage inversé, il faut recalculer l'espace requis et disponible
                nRequiredSpace = nCorrectedMenuHeight;
                // L'espace disponible correspondant à celui situé entre le container et le bord haut du champ déclencheur
                nAvailableSpace = getAbsolutePosition(inputParent).y;
                // En tenant compte du sous-conteneur pour déterminer l'espace disponible (jusqu'au bord haut de celui-ci) si on est dans ce cas
                if (subContainerPosition)
                    nAvailableSpace -= subContainerPosition.y;
                if (DEBUG) {
                    console.log('Espace nécessaire pour affichage inversé : ' + nRequiredSpace);
                    console.log('Hauteur disponible pour affichage inversé : ' + nAvailableSpace);
                }
                // On réduit le menu d'autant de pixels manquants pour afficher celui-ci
                // dans l'espace initialement souhaité (en-dessous)
                nCorrectedMenuHeight = nCorrectedMenuHeight - (nRequiredSpace - nAvailableSpace);
                // Informations de debug
                if (DEBUG)
                    console.log('Hauteur corrigée du menu avant application de minimum/maximal : ' + nCorrectedMenuHeight);
                // En imposant tout de même une hauteur minimale
                if (nCorrectedMenuHeight < nMinimalMenuHeight) {
                    nCorrectedMenuHeight = nMinimalMenuHeight;
                    // Informations de debug
                    if (DEBUG)
                        console.log('Hauteur corrigée du menu après application de hauteur minimale : ' + nCorrectedMenuHeight);
                }
                // #79 775 : et une hauteur maximale, pour ne pas se retrouver avec un menu démesurément grand, ce qui est pénalisant en affichage inversé, où le premier résultat se retrouve tout en haut (donc
                // potentiellement loin de l'élément déclencheur)
                if (nCorrectedMenuHeight > nMaximalMenuHeight) {
                    nCorrectedMenuHeight = nMaximalMenuHeight;
                    // Informations de debug
                    if (DEBUG)
                        console.log('Hauteur corrigée du menu après application de hauteur maximale : ' + nCorrectedMenuHeight);
                }
                // Informations de debug
                if (DEBUG)
                    console.log('Hauteur corrigée du menu finalement retenue : ' + nCorrectedMenuHeight);
            }

            if (!oAutoCompletionWaiterContent && top.document.getElementById("contentWait"))
                oAutoCompletionWaiterContent = top.document.getElementById("contentWait").cloneNode(true);
            if (!oAutoCompletionWaiter && top.document.getElementById("waiter"))
                oAutoCompletionWaiter = top.document.getElementById("waiter").cloneNode(true);

            if (oAutoCompletionWaiterContent && oAutoCompletionWaiter) {
                if (bRevertMenuOrientation) {
                    oAutoCompletionWaiterContent.style.bottom = inputParentPosition.h + "px";
                    if (nCorrectedMenuHeight != MENU_HEIGHT)
                        oAutoCompletionWaiterContent.style.height = nCorrectedMenuHeight + "px";
                }
                menuContainer.appendChild(oAutoCompletionWaiterContent);
                oAutoCompletionWaiterContent.appendChild(oAutoCompletionWaiter);
            }

            menu = document.createElement("div");
            if (bRevertMenuOrientation) {
                menu.style.bottom = inputParentPosition.h + "px";
                if (nCorrectedMenuHeight != MENU_HEIGHT)
                    menu.style.height = nCorrectedMenuHeight + "px";
            }
            menuContainer.appendChild(menu);

            menu.innerHTML = "";//  "<span class='search-beta'>BETA</span><span class='provider-info color-theme'>data.gouv.fr</span>";
            //setAttributeValue(menu, "class", 'search-box'); //border-theme 

            menu.id = "autocompletemenu";
            menuContainer.id = "menu-container";

            // MAB - 2018-01-29 - On initialise ici Bing Maps v8 sur le champ que l'on vient de créer ou recréer.
            // Auparavant, cette initialisation se faisait uniquement au chargement de la page, ce qui posait problème lorsqu'on utilisait plusieurs champs déclencheurs d'adresses
            // prédictives, avec ou sans fournisseurs différents : le menu était toujours positionné sur le dernier champ initialisé au chargement initial de la page
            // #63 480 : Normalement, provider a été initialisée par initSearch avec le bon fournisseur. Mais par mesure de précaution, on récupère l'information à partir du
            // paramètre header passé à la fonction. Sinon, provider pourra potentiellement rester initialisé avec sa valeur par défaut (DataGouv) si le premier déclencheur
            // d'adresse prédictive sur la page est Bing Maps (si le premier déclencheur est Sirene, provider est initialisée correctement)
            provider = getNumber(getAttributeValue(header, "data-provider"));
            if (provider == PredictiveAddressesRef.BingMapsV8) {
                var options = { maxResults: SEARCH_MAXRESULTS_BINGMAPSV8 };

                try {
                    autoSuggestMgr = new Microsoft.Maps.AutosuggestManager(options);
                    if (this.DEBUG) {
                        console.log("Rattachement de la recherche Bing Maps sur le champ " + input.id);
                    }
                }
                catch (xx) {
                    console.log("Erreur lors du rattachement de la  recherche Bing Maps sur le champ ");
                    console.log(xx);
                    console.log("Veuillez vérifier que le serveur n'est pas configuré pour fonctionner sans Internet.");
                    return;
                }
                autoSuggestMgr.attachAutosuggest('#' + input.id, '#autocompletemenu', function (result) {
                    onSelectSuggestion(result, header);
                });

                // #66 489 - Ajout de ignoreblur="1" sur le menu d'autocomplétion Bing Maps comme on le fait pour DataGouv sur addMenuItem()
                // ignoreblur="1" permet d'ignorer la MAJ en base de la valeur saisie dans le champ pour la recherche ;
                // Dans le cas où on clique sur une suggestion du menu, il faut que ce soit cette valeur qui soit enregistrée en base, et non les caractères saisis pour
                // la recherche, comme lorsqu'on sort du champ sans sélectionner de suggestion (cliquer sur un résultat de recherche provoque la sortie du curseur
                // du champ source et donc son onblur, donc validate() puis UpdateLaunch()). 
                if (menu && menu.firstChild)
                    setEventListener(menu.firstChild, "mousedown", function (evt) { setAttributeValue(header, "ignoreblur", "1"); });
            }
        }

        if (typeof bHidden === "undefined")
            bHidden = false;

        //ELAIZ : remplacement menu par menuContainer pour masquer le conteneur

        if (bHidden) {
            setWait(false);
            menuContainer.style.display = "none";
        }
        else
            menuContainer.style.display = "block";

        this.input = input;
    }

    // Ajoute un item dans le menu
    var addMenuItem = function (itemOptions) {

        var menuItem = document.createElement("div");
        menu.appendChild(menuItem);

        var menuItemLine = '';
        var menuItemBadge = "";
        var mainLineClassName = "mainLine";
        var subLineClassName = "subLine";
        var withBadgeClassName = "with";
        if (itemOptions.badge != '') {
            menuItemBadge = "<span class=\"badge " + itemOptions.badgeClass + "\">" + itemOptions.badge + "</span>";
            //if (itemOptions.values && itemOptions.values.length == 1)
            mainLineClassName += " " + withBadgeClassName + itemOptions.badgeClass;
            //else
            //subLineClassName += " " + withBadgeClassName + itemOptions.badgeClass;
        }
        if (itemOptions.values && itemOptions.values.length > 0) {
            menuItemLine = "<div class=\"" + mainLineClassName + "\">" + itemOptions.values[0] + "</div>"; // Première ligne
            if (itemOptions.values.length > 1) {
                // S'il y a plusieurs lignes et sous-lignes : on positionne le badge après la ligne principale dans le DOM
                //menuItemLine += menuItemBadge;
                for (var i = 1; i < itemOptions.values.length; i++) {
                    if (itemOptions.values[i].length > 0) {
                        // Sur les sous-lignes suivantes, plus nécessaire de réserver de l'espace au badge
                        if (i > 1)
                            subLineClassName = "subLine";
                        menuItemLine += "<div class=\"" + subLineClassName + "\">" + itemOptions.values[i] + "</div>";
                    }
                }
                itemOptions.className += " multiLineHeight" + (itemOptions.values.length);
            }
            // S'il n'y a qu'une seule ligne et pas de sous-ligne : le badge doit être positionné avant dans le DOM
            //else
            menuItemLine = menuItemBadge + menuItemLine;
        }

        menuItem.innerHTML = menuItemLine;

        // Ceci ne fonctionne pas toujours quand on clique trop vite : la fonction n'a pas le temps d'être affecté à l'item
        if (itemOptions.clickable) {
            setEventListener(menuItem, "click", function (evt) {
                if (DEBUG)
                    console.log("Suggestion click");

                var menuItem = evt.srcElement || evt.target;
                updateFields(itemOptions.srcHeader, menuItem, itemOptions.badgeClass == "SireneBadge");
                stopEvent(evt);
                deleteMenu();
            });
            // ignoreblur="1" permet d'ignorer la MAJ en base de la valeur saisie dans le champ pour la recherche ;
            // Dans le cas où on clique sur une suggestion du menu, il faut que ce soit cette valeur qui soit enregistrée en base, et non les caractères saisis pour
            // la recherche, comme lorsqu'on sort du champ sans sélectionner de suggestion (cliquer sur un résultat de recherche provoque la sortie du curseur
            // du champ source et donc son onblur, donc validate() puis UpdateLaunch()). 
            setEventListener(menuItem, "mousedown", function () { setAttributeValue(itemOptions.srcHeader, "ignoreblur", "1"); });
        }

        setAttributeValue(menuItem, "id", itemOptions.id);
        setAttributeValue(menuItem, "title", itemOptions.tooltip);
        setAttributeValue(menuItem, "class", "background-hover-theme color-hover-theme autocompleteResult " + itemOptions.className);


        //SetText(menuItem,);
    }

    // TODO Mettre la fonction dans emain.js pour que ca soit en commun
    var getModalDialog = function () {

        if (!isPopup())
            return null;

        //le top.eModFile n'est pas renseigné lorsque la popup a été ouverte depuis une popup de recherche
        // eModFile est en effet renseigné lors de l'ouverture de la fiche en modal dialog et dans ce cas, la variable est attachée à la fenêtre de recherche
        // et plus à la fenêtre principale (top)

        var myModFile = eTools.getModalFromWindowName(window.name, nTab);

        if (myModFile == null) {
            if (top.eModFile && top.eModFile.isModalDialog)
                myModFile = top.eModFile;
            else
                myModFile = top.window['_md']["popupFile"];
        }
        return { oModFile: myModFile, modFile: myModFile.getIframe(), pupClose: false, bPlanning: false, docTop: top };

    };

    var getTreatedFieldsToUpdate = function (fieldsList) {

    }
    // Appel Engine pour mettre à jour depuis un Array (descid, valeur)
    var executeUpdate = function (header, arrValues, listDescId) {

        if (DEBUG) {
            if (!header)
                console.log("oAutoCompletion.executeUpdate : header null");
        }

        nTab = getTabDescid(getAttributeValue(header, "did"));

        if (!nTab) {
            if (DEBUG)
                console.log("oAutoCompletion.executeUpdate : nTab = 0");
            deleteMenu();
            return;
        }

        // Récupération de la fiche en cours
        var currentFile = document.getElementById("fileDiv_" + nTab);
        if (currentFile && currentFile.tagName && currentFile.tagName.toLowerCase() != "div")
            return;

        //DescId & Id de la ficher en cours
        nFileId = getNumber(currentFile.getAttribute("fid"));

        // Vérification
        var nFileTab = getNumber(currentFile.getAttribute("did"));
        if (nFileTab != nTab) {
            deleteMenu();
            return;
        }

        var fields = new Array();
        if (listDescId.length > 0)
            fields = getFieldsInfos(nTab, nFileId, listDescId.join(";"));

        // Appel manager pour traitement des valeurs à mettre à jour
        var oUpdater = new eUpdater("mgr/eAutoCompletionManager.ashx", 1);
        oUpdater.addParam("descid", nTab, "post");
        oUpdater.addParam("action", MgrAction.GetFieldsToUpdate, "post");
        oUpdater.addParam("fromSirene", sireneEnabled ? "1" : "0", "post");
        oUpdater.addParam("fid", nFileId, "post");
        var nbFld = 0;
        var fld;
        for (var i = 0; i < fields.length; i++) {
            fld = fields[i];
            fld.newValue = arrValues[fld.descId];
            fld.newLabel = fld.newValue;
			// #72 075 - Mise à jour de catalogues liés à un catalogue parent dont les informations sont manquantes
			// Dans le cas où le catalogue à mettre à jour est lié à un catalogue parent (BoundDescId renseigné),
			// on vérifie si son BoundValue a été renseigné par getFieldsInfos() à partir de son attribut pbdv (ParentBoundValue).
			// Si ça n'est pas le cas, on tente de récupérer cette information (requise par eTools.AddNewValueInCatalog côté back)
			// depuis le catalogue lié parent, si celui-ci est présent dans la page.
			// Le faire ici permet de gérer le cas explicitement pour l'autocomplétion uniquement, sans risquer d'effets de bord
			// à le faire dans la fonction (très) centrale getFldEngFromElt (utilisée par getFieldsInfos plus haut) qui, elle,
			// est utilisée pour N'IMPORTE quel cas de mise à jour de champ sur l'application
            if (Number(fld.boundDescId) > 0 && fld.boundValue == "") {
                var boundField = getFieldsInfos(nTab, nFileId, fld.boundDescId);
                if (boundField && boundField.length > 0 && boundField[0].boundValue != "")
                    fld.boundValue = boundField[0].boundValue;
            }
            if (DEBUG) {
                console.log("New value : " + fld.newValue);
                console.log("New label : " + fld.newLabel);
            }
            oUpdater.addParam('fld_' + nbFld, fld.GetSerialize(), "post");
            nbFld++;
        }
        oUpdater.send(function (oRes) {
            executeEngineTreatment(oRes, nTab, nFileId, header, fields);
        });

    }

    // Mise à jour Engine
    var executeEngineTreatment = function (oRes, nTab, nFileId, fieldHeader, fields) {

        if (oRes) {
            var result = JSON.parse(oRes);
            if (result && result.length > 0) {

                var fld = null;
                var fieldResult;
                var action, autoCpl;
                var fieldsTrigger = "";
                var bHasMidFormula = false;

                var eEngineUpdater = new eEngine();
                eEngineUpdater.ModalDialog = getModalDialog();

                eEngineUpdater.Init();

                eEngineUpdater.AddOrSetParam('tab', nTab);
                eEngineUpdater.AddOrSetParam('fileId', nFileId);
                eEngineUpdater.AddOrSetParam('jsEditorVarName', "eInlineEditorObject");

                autoCpl = parseInt(getAttributeValue(fieldHeader, "autocpl"));
                if (isNaN(autoCpl))
                    autoCpl = 0;

                action = XrmCruAction.NONE;
                if (autoCpl == 1) {
                    action = XrmCruAction.UPDATE;
                    if (CheckOnlyMiddle())
                        action = XrmCruAction.CHECK_ONLY_MIDDLE;

                } else {
                    // on devrait pas tomber dans ce cas
                    // On lance pas de MAJ si recherche pas activée
                    deleteMenu();
                    top.setWait(false);
                    return;
                }

                // Si pas d'action pour vérification adr ou mf ou update on zappe;
                if (action == XrmCruAction.NONE) {
                    return;
                }

                for (var i = 0; i < result.length; i++) {

                    fieldResult = result[i];

                    var arrFields = fields.filter(function (f) {
                        return f.descId == fieldResult.DescID;
                    });
                    if (arrFields.length > 0)
                        fld = arrFields[0];
                    else
                        fld = fields[0];


                    bHasMidFormula = bHasMidFormula || fld.hasMidFormula;

                    fieldsTrigger += fld.descId + ';';
                    fld.newValue = fieldResult.DbValue;
                    fld.newLabel = fieldResult.DisplayValue;

                    if (action == XrmCruAction.UPDATE) {
                        fld.readOnly = false;
                        fld.forceUpdate = '1';
                    }

                    eEngineUpdater.AddOrSetField(fld);
                    editField(fld);
                }

                setAttributeValue(fieldHeader, "ignoreblur", "1");

                // Focus du champ de départ
                var input = document.querySelector("[ename='" + fieldHeader.id + "']");
                if (input) {
                    //document.getElementById(input.id).focus();
                    eEngineUpdater.AddOrSetParam('parentPopupSrcId', input.id);
                }

                if (action == XrmCruAction.CHECK_ONLY_MIDDLE && !bHasMidFormula) {
                    deleteMenu(); top.setWait(false);
                    return;
                }


                setAttributeValue(header, "ignoreblur", "1");

                // Si pas d'action pour vérification adr ou mf ou update on zappe;
                if (action == XrmCruAction.NONE) {
                    deleteMenu();
                    top.setWait(false);
                    return;
                }

                eEngineUpdater.AddOrSetParam("engAction", action);
                eEngineUpdater.AddOrSetParam("autoComplete", "1");
                eEngineUpdater.AddOrSetParam("fieldTrigger", fieldsTrigger);

                eEngineUpdater.ErrorCallBack = function (oRes) { deleteMenu(); top.setWait(false); };
                eEngineUpdater.SuccessCallbackFunction = function (engResult) {
                    deleteMenu();
                    top.setWait(false);
                };

                try {
                    eEngineUpdater.UpdateLaunch();
                }
                catch (ex) {
                    deleteMenu();
                    top.setWait(false);
                }

                return;
            }

            deleteMenu();
            top.setWait(false);
        }

    }

    // Mise à jour des champs (via suggestion DataGouv)
    var updateFields = function (fieldHeader, menuItem) {

        var key = getAttributeValue(menuItem, "id");
        // Si on clique sur une sous-ligne d'un élément de menu, l'élément source renvoyé ne sera pas la ligne de résultat globale, mais la sous-ligne elle-même.
        // Dans ce cas de figure, on tente de récupérer l'id sur l'élément parent
        if (key == "")
            key = getAttributeValue(menuItem.parentElement, "id");

        if (oDoc != null) {
            var items = oDoc.getElementsByTagName(key);
            var dico = new Array();
            var listDescId = new Array();
            var descid;

            for (var i = 0; i < items.length; i++) {
                descid = getAttributeValue(items[i], "descid");
                listDescId.push(descid);
                dico[descid] = getXmlTextNode(items[i]);
            }

            executeUpdate(fieldHeader, dico, listDescId);

        }
        else if (DEBUG) {
            console.log("oAutoCompletion.updateFields : oDoc null");
        }
    };

    var editField = function (field) {
        var editField = document.getElementById(field.cellId);
        if (editField)
            editInnerField(editField, field);
    };

    // En popup en mode création ou mode modif avec la validation
    // on vérifie juste les formules de milieu
    var CheckOnlyMiddle = function () {
        if (!isPopup())
            return false;

        var maindiv = document.getElementById("mainDiv");
        if (maindiv != null && (getAttributeValue(maindiv, "autosv") != "1" || nFileId == 0))
            return true;

        return false;
    };


    var deleteMenu = function () {

        // enlève le focus sur l'input
        //if (header)
        //    header.click();

        hideMenu();

        //if (menuContainer != null && menuContainer.parentElement != null)
        //    menuContainer.parentElement.removeChild(menuContainer);

        //menuContainer = null;
        //menu = null;
        input = null;
        header = null;
        items = null;
        itemIndex = -1;
        highlightedItem = null;
        //originValue = null;
        //nFileId = 0;
        //nTab = 0;

    };

    var select = function () {
        if (highlightedItem)
            updateFields(header, highlightedItem);

        deleteMenu();
    };

    var highlight = function () {

        if (itemIndex <= -1)
            return;

        var indexMax = 0;
        if (items != null)
            indexMax = items.length - 1;

        if (itemIndex > indexMax)
            return;

        if (highlightedItem != null) {
            switchClass(highlightedItem, "background-theme", "background-hover-theme");
            switchClass(highlightedItem, "color-theme", "color-hover-theme");
        }

        highlightedItem = document.getElementById("item" + itemIndex);

        if (highlightedItem) {
            highlightedItem.scrollIntoView(false);
            switchClass(highlightedItem, "background-hover-theme", "background-theme");
            switchClass(highlightedItem, "color-hover-theme", "color-theme");
        }
    };

    var indexUp = function () {
        var indexMax = 0;
        if (items != null)
            indexMax = items.length - 1;

        if (itemIndex <= indexMax)
            itemIndex++;

        if (itemIndex > indexMax)
            itemIndex = indexMax;

    };
    var indexDown = function () {

        if (itemIndex > 0)
            itemIndex--;

        if (itemIndex < 0)
            itemIndex = 0;
    };

    var restoreValue = function () {
        if (input != null) {
            var oldVal = getAttributeValue(input, "oldVal");
            input.value = oldVal;

            header = document.getElementById(getAttributeValue(input, "ename"));
            setAttributeValue(header, "ignoreblur", "1");
        }
    };

    var manageKeyUp = function (evt) {

        switch (evt.keyCode) {

            // flèche haut, index déminue
            case eTools.keyCode.UP_ARROW:
                indexDown();
                highlight();
                break;
                // flèche bas, index augment
            case eTools.keyCode.DOWN_ARROW:
                indexUp();
                highlight();
                break;
            case eTools.keyCode.ENTER:
                select();
                break;
            case eTools.keyCode.ESCAPE:
                restoreValue();
                deleteMenu();
                break;
            default:
                onKeyUp(evt);
                break;
        };
    };

    var checkSrc = function (evt) {

        var src = evt.target || evt.srcElement;

        // Si c'est un nouveau element on supprime le menu existant
        if (input && input.id != src.id)
            deleteMenu();

    };

    // Initialisation de l'autosuggestion sur les champs
    var initAutoSuggestOnFields = function (listHeaders) {
        var header, input, menu;

        for (var i = 0; i < listHeaders.length; i++) {
            header = listHeaders[i];
            if (getAttributeValue(header, "autocpl") == "1" && getAttributeValue(header, "data-provider") == "1") {

                input = document.querySelector("input[ename='" + header.id + "']:not([ero='1'])");
                if (input) {
                    // MAB - 2018-01-29 : c'est addOrDisplayMenu, appelée via initSearch(), qui se chargera désormais d'attacher l'autosuggestion de Bing Maps v8
                    // Ceci, afin de pouvoir repositionner correctement le menu lorsqu'on utilise les adresses prédictives sur plusieurs champs avec plusieurs fournisseurs
                    // (le précédent système ne pouvait fonctionner qu'avec un seul déclencheur et aucun autre fournisseur que Bing Maps v8 utilisé sur la page)
                    initSearch(input, true, false, true, true, true, true);
                }

            }
        }


    }

    // Au clic sur la suggestion (provider : BingMaps)
    var onSelectSuggestion = function (result, header) {

        var address = result.address.addressLine;
        var country = result.address.countryRegion;
        var locality = result.address.locality;
        var postalCode = result.address.postalCode;
        var district = result.address.adminDistrict || "";

        address = (typeof address !== "undefined") ? address : "";
        country = (typeof country !== "undefined") ? country : "";
        locality = (typeof locality !== "undefined") ? locality : "";
        postalCode = (typeof postalCode !== "undefined") ? postalCode : "";

        // Bing ne transmet pas toujours la localisation #63753
        var geo = "";
        if (typeof (result.location) != 'undefined') {
            var lat = result.location.latitude;
            var lon = result.location.longitude;
            geo = (lat && lon) ? "POINT(" + lon + " " + lat + ")" : "";
        }

        var tab = getTabDescid(getAttributeValue(header, "did"));

        var updater = new eUpdater("mgr/eAutoCompletionManager.ashx", 1);
        updater.addParam("descid", tab, "post");
        updater.addParam("action", MgrAction.GetAddressMapping, "post");
        updater.addParam("fromSirene", "0", "post");
        updater.send(function (oRes) {
            var result = JSON.parse(oRes);
            if (result.Success) {
                var arrList = result.ListMapping;
                var arrValues = new Array();
                var arrDescid = new Array();
                var descid, source, objValue, sValue;
                // Parcours du mapping pour mettre à jour les différents champs par rapport aux résultats
                for (var i = 0; i < arrList.length; i++) {
                    descid = arrList[i].DescID;
                    source = arrList[i].Source;
                    arrDescid.push(descid);

                    sValue = "";

                    if (source == "streetName")
                        sValue = address;
                    else if (source == "postalCode")
                        sValue = postalCode;
                    else if (source == "city")
                        sValue = locality;
                    else if (source == "country")
                        sValue = country;
                    else if (source == "geography")
                        sValue = geo;
                    else if (source == "region")
                        sValue = district;
                    // US #1224 - "Adresse complète" : Format retenu pour Bing Maps v8 : Adresse (rue), CODEPOSTAL Ville, Région, Pays
                    else if (source == "label") {
                        sValue = address;
                        if (postalCode != "" || locality != "")
                            sValue += ", " + postalCode + " " + locality;
                        if (district != "")
                            sValue += ", " + district;
                        if (country != "")
                            sValue += ", " + country;
                    }

                    arrValues[descid] = sValue;

                }
                executeUpdate(header, arrValues, arrDescid);
            }
            else {
                eAlert(0, top._res_1760, result.Error, "");
            }

        });

    }

    var onError = function () {

    }

    return {

        enabled: function (src) {

            var bEnabled = true;
            // uniqument champ text
            if (getAttributeValue(src, "eaction") != "LNKFREETEXT")
                bEnabled = false;
            else {
                var autoCpl = 0;
                if (getAttributeValue(src, "ename") != "") {
                    var header = document.getElementById(getAttributeValue(src, "ename"));
                    autoCpl = getAttributeValue(header, "autocpl");
                    if (isNaN(autoCpl))
                        autoCpl = 0;
                }

                bEnabled = (autoCpl == 1 && getAttributeValue(src, "ero") != '1');
            }
            // On supprime le menu si on click ailleurs
            if (!bEnabled)
                deleteMenu();

            return bEnabled;
        },

        search: function (evt, trigger, action) {

            checkSrc(evt);

            switch (trigger) {
                case "KEYUP":
                    manageKeyUp(evt);
                    break;
                case "LCLICK":
                    onClick(evt);
                    break;
                default:
                    deleteMenu();
            }


        },

        loadBingAutoSuggest: function (fieldsHeaders) {
            initAutoSuggestOnFields(fieldsHeaders);
        },

        isBingAutoSuggestResultElement: function (srcElement) {
            var result = false;
            try {
                // Détection de l'élément du DOM créé par le JS Bing Maps comme conteneur à l'intérieur de autocompletemenu
                var microsoftMapContainer = document.querySelector(".MicrosoftMap");
                // Cas où le clic est intercepté sur un élément de résultat (ul, li, p...)
                if (isParentElementOf(srcElement, microsoftMapContainer))
                    result = true;
                    // Cas où le clic est intercepté sur le menu lui-même (autocompletemenu)
                else if (isParentElementOf(microsoftMapContainer, srcElement))
                    result = true;
            }
            catch (e) { }
            return result;
        },

        dispose: function () {
            deleteMenu();
        }
    }
})();


var XrmCruAction =
{
    NONE: 0,
    /// <summary>Mise à jour de rubrique (avant la mise à jour en base, vérification des adresses identiques et vérification de formule de milieu)</summary>
    UPDATE: 1,
    /// <summary>Exécution après avoir validé la formule du milieu</summary>
    CHECK_MIDDLE_OK: 2,
    /// <summary>Exécution après avoir validé la mise à jour des adresses</summary>
    CHECK_ADR_OK: 3,
    /// <summary>Vérifie uniquement les formules du milieu</summary>
    CHECK_ONLY_MIDDLE: 4
};