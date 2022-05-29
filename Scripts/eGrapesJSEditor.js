//*****************************************************************************************************//
//*****************************************************************************************************//
//*** KJE - 04/2020 - surcouche JS permettant d'afficher un champ Mémo HTML (via GrapesJS)
//*** Nécessite :
//*** - Scripts/eTools.js
//*** - themes/default/css/eMemoEditor.css (pour l'affichage en mode Texte Brut)
//*****************************************************************************************************//
//*****************************************************************************************************//




// ------------------------------------------------------------------------------------------------
// OBJET eGRAPESJSEDITOR
// Le constructeur est situé en fin de fonction
// ------------------------------------------------------------------------------------------------

function eGrapesJSEditor(strInstanceName, bHTML, oContainer, oParentFieldEditor, strValue, strJSVarName) {

    var that = this; // pointeur vers l'objet eMemoEditor lui-même, à utiliser à la place de this dans les évènements onclick, setTimeout... (où this correspond alors à l'objet cliqué, à window...)

    // --------------------------------------------------------------------------
    // Propriétés
    // --------------------------------------------------------------------------

    this.debugMode = false; // à mettre à true pour afficher certains messages de diagnostic - NE PAS UTILISER EN PRODUCTION !
    this.debugLevel = 1; // indique le niveau de traces ajoutées en mode debug. Plus le nombre est élevé = plus précises sont les traces. N'a aucun effet si debugMode = false
    // Caractéristiques du champ
    this.name = strInstanceName; // nom d'instance de l'objet créé en interne (notamment CKEditor)
    this.title = ''; // libellé du champ Mémo utilisé, par ex., en mode Zoom comme titre de fenêtre modale
    this.value = strValue; // texte initial contenu dans le champ Mémo
    this.virtualTextAreaValue = strValue; // propriété utilisée pour stocker la valeur "actuelle" du champ Mémo si celui-ci n'a pas été instancié sous forme de textarea par .show
    this.customCSS = ''; // feuille de styles personnalisée à utiliser pour l'affichage du contenu du champ Mémo (mode HTML uniquement)
    this.isHTML = bHTML; // true pour mode HTML, false pour texte brut
    this.helpDialog = null; // Backlog #354 - Handler permettant de gérer la fenêtre d'aide ouverte
    // Affichage et apparence
    this.language = "fr"; // Langue du composant. Utilisée aussi bien pour CKEditor que grapesjs
    this.inlineMode = false; // à mettre à true pour activer le mode in-line editing de CKEditor 4 (édition des éléments HTML en mode WYSIWYG en cliquant dessus, avec barres d'outils flottante)
    // ----------------------------------------------------------------------------------------
    // ----------------------------------------------------------------------------------------
    this.isFullScreen = false; // à mettre à true pour ne pas faire apparaître le bouton "Zoom/Plein écran" (ex. sur la fenêtre modale)
    this.editorType = ""; // mot-clé indiquant dans quel contexte est affiché le champ Mémo ("mail", "mailing", "formular" "formularsubmission", "adminusermemo", "adminusersign", etc. : permet d'adapter le comportement de certains traitements à la fenêtre appelante
    this.toolbarType = ""; // mot-clé spécifiant une variante de barre d'outils à afficher ; déclarer cette variante et les boutons à afficher dans la fonction setToolBar()
    this.toolbarTypeForTemplateEditor = "text"; // Backlog #356 - mot-clé spécifiant une variante de barre d'outils à afficher lorsque grapesjs instancie CKEditor
    this.dockToolbar = true; // Backlog #258 - lors de l'intégration de CKEditor via grapesjs, indique si on doit positionner (forcer) l'affichage de la barre d'outils CKEditor en haut de grapesjs
    this.readOnly = false; // true pour afficher un champ Mémo en lecture seule, false sinon
    this.permanentBadgeEnabled = false; // Backlog #457 - Demande d'ALEB du 14/05/2019 - Badge permanent non affiché pour le moment (nécessite d'être finalisé)

    // Environnement
    this.parentFieldEditor = oParentFieldEditor; // pointeur vers l'objet eFieldEditor ayant éventuellement instancié cet objet eMemoEditor
    this.container = oContainer; // pointeur vers un élément HTML (ex : div) destiné à accueillir le champ Mémo	
    this.jsVarName = strJSVarName; // contient le nom de la variable JavaScript référençant cet objet eMemoEditor ; indispensable pour certaines parties de code auto-générées
    // ----
    // Référence vers l'objet CKEditor instancié (mode HTML uniquement)
    // Tant que l'objet n'est pas initialisé, cette propriété est initialisée avec une fonction minimale permettant de recevoir les appels à eMemoEditor.htmlEditor.on()
    // alors que le composant n'est pas encore chargé. Puis, lorsque celui-ci le sera, les évènements stockés en attente y seront ensuite rattachés.
    this.htmlEditor = function () { }
    this.htmlEditor.isFake = true;
    this.htmlEditor.on = function (eventId, callbackFct) { that.listeners.push({ "event": eventId, "handle": callbackFct }); };
    // ----
    this.textEditor = null; // référence vers le textarea (mode Text Brut uniquement - en mode HTML, ce textarea est remplacé par le composant CKEditor)
    this.htmlTemplateEditor = null; // référence vers une éventuelle surcouche permettant l'édition plus avancée de modèles/templates HTML (ex : grapesjs)
    this.htmlTemplateEditorBlocks = new Array(); // Backlog #349, #352 - Variable à alimenter depuis les renderers, définissant les blocs à afficher à partir de eudonet.json (blocs système) et la BDD (blocs utilisateur)
    this.enableTemplateEditor = false; // indique si la surcouche d'édition plus avancée de modèles/templates HTML doit être instanciée
    this.templateEditorCreationInProgress = false; // booléen permettant d'éviter l'appel multiple à la fonction createHTMLTemplateEditor()
    this.templateEditorCustomizationsLoaded = false; // booléen indiquant à quel moment déclencher le rendu du composant (soit après le chargement de nos personnalisations)
    this.descId = 0; // DescID du champ édité ; utilisé uniquement lorsqu'on instancie l'objet à partir du mode Liste pour afficher directement le champ en mode Zoom
    this.fileId = 0; // FileID de la fiche liée au champ édité ; utilisé uniquement lorsqu'on instancie l'objet à partir du mode Liste pour afficher directement le champ en mode Zoom
    this.templateEditorRootContainer = null;
    this.nTabFrom = 0;//L'id de la table;
    /*
    //Canceled by KHA le 31/01/2013 et remplacé par une fonction asynchrone cf emain.js updateFile

    this.scrollIntoViewId = null; // indique l'ID de l'objet sur lequel on doit aligner l'ascenseur de la page lors d'un appel à this.scrollIntoView()
    this.scrollOnShow = false; // si ce paramètre est à true, on fera un appel à scrollIntoView() sur l'objet dont l'ID est indiqué par this.scrollIntoViewId, afin de placer 
    */
    // l'ascenseur de la page au niveau de cet objet, une fois que le champ Mémo aura été complètement instancié et affiché
    this.mergeFields = null;   // liste des champs de fusion utilisables dans la combobox "Champs de fusion" (si la fonctionnalité est activée)
    this.addStartSpaceWithMergeFields = true; // #72 278 - Indique s'il faut insérer, ou non, un espace insécable & nbsp ; avant un champ de fusion. Résout l'anomalie d'insertion du champ au sein de la balise <label> d'un autre
    this.addEndSpaceWithMergeFields = false; // #72 278 - Indique s'il faut insérer, ou non, un espace insécable & nbsp ; après un champ de fusion. Résout l'anomalie d'insertion du champ au sein de la balise <label> d'un autre

    this.enableAdvancedFormular = false;// tâche #2 458 - Indique si eMemoEditor est appelé pour le formulaire avancé
    this.grapesJSBlocks = null;// tâche #2 573 - la liste de blocs GrapesJS en string
    this.wordlineBlocs = null;
    this.isMemoInstance = false; //true pour le mode CKEditor, false pour le mode grapesJS seulement
    this.linkResult = null; //fonction pour insérer un lien
    this.helpDialog = null; // US -2 875 - gestion de la fenetre d'aide
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    // PARAMETRAGE
    // Ces méthodes sont généralement appelées pour paramétrer le champ avant affichage (show()), soit en interne, soit depuis la page qui instancie eMemoEditor
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------

    //US 2 875 - Accéder à l'aide depuis l'onglet Création graphique de l'assistant des formulaires avancés
    this.helpTutorial = function (open) {
        if (open) {
            this.helpDialog();
        }

    }

    // Complète les variables de langue intégrées à CKEditor avec les ressources additionnelles de XRM
    // TODO...
    this.getGrapesJSLanguageResources = function () {

        if (this.enableTemplateEditor && typeof (grapesjs) != "undefined") {
            this.trace("Chargement des ressources de langue personnalisées pour l'éditeur de templates avancés...");

            // Création d'une propriété lang préfixée, "xrmLang", pour éviter tout éventuel conflit en cas de prise en charge native des langues sur grapesjs à l'avenir
            if (!grapesjs.xrmLang) {
                grapesjs.xrmLang = [];
                grapesjs.xrmLang[this.language] = {};

                grapesjs.xrmLang[this.language].stylesSectorDimension = top._res_1976; // // "Dimension et marge";
                grapesjs.xrmLang[this.language].stylesSectorExtendedProperties = top._res_1977; // "Propriétés étendues";
                grapesjs.xrmLang[this.language].stylesSectorExtendedPropertyBackgroundColor = top._res_1978; // "Couleur de fond";
                grapesjs.xrmLang[this.language].stylesSectorExtendedPropertyPredefinedFontSize = top._res_2079; // "Taille de police prédéfinie";
                grapesjs.xrmLang[this.language].stylesSectorExtendedPropertyPredefinedFontSizeSmall = top._res_1979; // "Petite";
                grapesjs.xrmLang[this.language].stylesSectorExtendedPropertyPredefinedFontSizeMedium = top._res_1980; // "Moyenne";
                grapesjs.xrmLang[this.language].stylesSectorExtendedPropertyPredefinedFontSizeLarge = top._res_1981; // "Grande";
                grapesjs.xrmLang[this.language].stylesSectorPropertyWidth = top._res_1508; // "Largeur";
                grapesjs.xrmLang[this.language].stylesSectorPropertyHeight = top._res_1507; // "Hauteur";
                grapesjs.xrmLang[this.language].stylesSectorPropertyMaxWidth = top._res_2080; // "Largeur max.";
                grapesjs.xrmLang[this.language].stylesSectorPropertyMaxHeight = top._res_2081; // "Hauteur max.";
                grapesjs.xrmLang[this.language].stylesSectorPropertyMinWidth = top._res_2082; // "Largeur min.";
                grapesjs.xrmLang[this.language].stylesSectorPropertyMinHeight = top._res_2083; // "Hauteur min.";
                grapesjs.xrmLang[this.language].stylesSectorPropertyPadding = top._res_2084; // "Marge int.";
                grapesjs.xrmLang[this.language].dialogImportTitle = top._res_2085; // "Mode Expert : Code HTML "; // User Story #268/Backlog #212 - La fonctionnalité Importer un modèle est nommée Source puis "Mode Expert : Code HTML" pour se rapprocher de la vision CKEditor
                grapesjs.xrmLang[this.language].dialogImportButton = top._res_28; // "Valider"; // User Story #268/Backlog #212
                grapesjs.xrmLang[this.language].dialogImportLabel = top._res_2086; // "Voici le code HTML contenu dans l'éditeur. Vous pouvez le copier-coller et le mettre à jour en cliquant sur Valider";
                grapesjs.xrmLang[this.language].dialogExportTitle = top._res_15; // "Exporter";
                grapesjs.xrmLang[this.language].dialogExportLabel = top._res_2087; // "Vous pouvez récupérer le code HTML/CSS de votre modèle ici";
                grapesjs.xrmLang[this.language].dialogEmptyCanvasConfirm = top._res_2088; // "Êtes-vous sûr(e) de vouloir tout effacer ?";
                grapesjs.xrmLang[this.language].buttonEmptyCanvas = top._res_2089; // "Effacer tout";
                grapesjs.xrmLang[this.language].buttonUndo = top._res_29; // "Annuler";
                grapesjs.xrmLang[this.language].buttonRedo = top._res_2090; // "Refaire";
                grapesjs.xrmLang[this.language].buttonFullScreen = top._res_6601; // "Plein écran";
                grapesjs.xrmLang[this.language].buttonImport = top._res_28; // "Valider";
                grapesjs.xrmLang[this.language].buttonCode = top._res_2091; // "Code HTML/CSS";
                grapesjs.xrmLang[this.language].buttonJSON = "JSON";
                grapesjs.xrmLang[this.language].buttonToggleBorders = top._res_2092; // "Afficher/Masquer les bordures";
                grapesjs.xrmLang[this.language].buttonHelp = top._res_6187; // "Aide" - Backlog #354
                grapesjs.xrmLang[this.language].buttonDevicesDesktop = top._res_2093; // "Bureau";
                grapesjs.xrmLang[this.language].buttonDevicesMobile = top._res_2094; // "Mobile";
                grapesjs.xrmLang[this.language].buttonDevicesTablet = top._res_2095; // "Tablette";
                grapesjs.xrmLang[this.language].buttonHideShowRightPanel = top._res_2096; // "Afficher/Masquer le panneau droit";

                // Backlog #38
                grapesjs.xrmLang[this.language].blockNativeCategories = new Array();
                grapesjs.xrmLang[this.language].blockNativeCategories["basic"] = top._res_2097; // "Blocs";
                grapesjs.xrmLang[this.language].blockNativeCategories["additional"] = top._res_2098; // "Interactivité";
                grapesjs.xrmLang[this.language].blockNativeCategories["structures"] = top._res_2099; // "Structures";
                grapesjs.xrmLang[this.language].blockNativeLabels = new Array();
                grapesjs.xrmLang[this.language].blockNativeLabels["text-section"] = top._res_2100; // "Section";
                grapesjs.xrmLang[this.language].blockNativeLabels["text"] = top._res_1001; // "Texte";
                grapesjs.xrmLang[this.language].blockNativeLabels["textnode"] = top._res_1001; // "Texte";
                grapesjs.xrmLang[this.language].blockNativeLabels["image"] = top._res_1216; // "Image";
                grapesjs.xrmLang[this.language].blockNativeLabels["sect100"] = top._res_2101; // "1 Colonne";
                grapesjs.xrmLang[this.language].blockNativeLabels["sect50"] = top._res_2102; // "2 Colonnes";
                grapesjs.xrmLang[this.language].blockNativeLabels["sect37"] = top._res_2102 + " 30%/70%"; // "2 Colonnes 30%/70%"
                grapesjs.xrmLang[this.language].blockNativeLabels["sect30"] = top._res_2103; // "3 Colonnes";
                grapesjs.xrmLang[this.language].blockNativeLabels["link"] = top._res_1500; // "Lien";
                grapesjs.xrmLang[this.language].blockNativeLabels["video"] = top._res_2104; // "Vidéo";
                grapesjs.xrmLang[this.language].blockNativeLabels["map"] = top._res_2069; // "Carte";
                grapesjs.xrmLang[this.language].blockNativeLabels["button"] = top._res_7693; // "Bouton";
                grapesjs.xrmLang[this.language].blockNativeLabels["divider"] = top._res_7292; // "Séparateur";
                grapesjs.xrmLang[this.language].blockNativeLabels["quote"] = top._res_2105; // "Citation";
                grapesjs.xrmLang[this.language].blockNativeLabels["link-block"] = top._res_2106; // "Bloc de Liens";
                grapesjs.xrmLang[this.language].blockNativeLabels["grid-items"] = top._res_7977; // "Grille";
                grapesjs.xrmLang[this.language].blockNativeLabels["list-items"] = top._res_179; // "Liste";
                grapesjs.xrmLang[this.language].blockNativeLabels["container-image"] = top._res_2107; // "Conteneur d'image";
                grapesjs.xrmLang[this.language].blockNativeLabels["container-button"] = top._res_7693; // "Conteneur de bouton";
                grapesjs.xrmLang[this.language].blockNativeLabels["container-map"] = top._res_2109; // "Conteneur de carte";
                grapesjs.xrmLang[this.language].blockNativeLabels["container-video"] = top._res_2110; // "Conteneur de vidéo";
                grapesjs.xrmLang[this.language].blockNativeLabels["container-canvas"] = top._res_2498; // "Conteneur de canvas";
                grapesjs.xrmLang[this.language].blockNativeLabels["eudonet-extended-image"] = top._res_1216; // "Image";
                grapesjs.xrmLang[this.language].blockNativeLabels["eudonet-extended-button"] = top._res_7693; // "Bouton";
                grapesjs.xrmLang[this.language].blockNativeLabels["eudonet-extended-map"] = top._res_2069; // "Carte";
                grapesjs.xrmLang[this.language].blockNativeLabels["eudonet-extended-video"] = top._res_2104; // "Vidéo";
                grapesjs.xrmLang[this.language].blockNativeLabels["eudonet-extended-canvas"] = top._res_2498; // "canvas";
                grapesjs.xrmLang[this.language].blockNativeLabels["table"] = top._res_1512; // "Tableau";
                grapesjs.xrmLang[this.language].blockNativeLabels["tbody"] = top._res_2960;// Corps du tableau
                grapesjs.xrmLang[this.language].blockNativeLabels["row"] = top._res_2961;// Ligne de tableau
                grapesjs.xrmLang[this.language].blockNativeLabels["cell"] = top._res_2308; // "Cellule";
                grapesjs.xrmLang[this.language].blockNativeLabels["wrapper"] = top._res_5091; // "Corps";
                grapesjs.xrmLang[this.language].blockNativeLabels["body"] = top._res_5091; // "Corps";
                grapesjs.xrmLang[this.language].blockNativeContents = new Array();
                grapesjs.xrmLang[this.language].blockNativeContents["text"] = '<div data-gjs-type="text">' + top._res_2111 + '</div>';
                grapesjs.xrmLang[this.language].blockNativeContents["list-items-title"] = top._res_2112; // 'Titre de votre section';
                grapesjs.xrmLang[this.language].blockNativeContents["list-items-text"] = top._res_2113; // 'Contenu de la section';
                grapesjs.xrmLang[this.language].blockCustomRes = new Array(); // Backlog #349 et #352 - Ressources déclarées dans les sources de blocs externalisées (JSON ou BDD)
                grapesjs.xrmLang[this.language].blockCustomRes["categoryBlocks"] = top._res_2097; // "Blocs";
                grapesjs.xrmLang[this.language].blockCustomRes["basicBlocks"] = top._res_2602; // "Basique
                grapesjs.xrmLang[this.language].blockCustomRes["blockSimpleText"] = top._res_1001; // "Texte";
                grapesjs.xrmLang[this.language].blockCustomRes["blockImage"] = top._res_1216; // "Image";
                grapesjs.xrmLang[this.language].blockCustomRes["blockCanvas"] = top._res_2498; // "Canvas";
                grapesjs.xrmLang[this.language].blockCustomRes["block1Col"] = top._res_2101; // "1 Colonne";
                grapesjs.xrmLang[this.language].blockCustomRes["block2Col"] = top._res_2102; // "2 Colonnes";
                grapesjs.xrmLang[this.language].blockCustomRes["block3Col"] = top._res_2103; // "2 Colonnes
                grapesjs.xrmLang[this.language].blockCustomRes["block2Col37"] = top._res_2102 + " 30%/70%"; // "2 Colonnes 30%/70%"
                grapesjs.xrmLang[this.language].blockCustomRes["block2Col73"] = top._res_2102 + " 70%/30%"; // "2 Colonnes 30%/70%"
                grapesjs.xrmLang[this.language].blockCustomRes["blockButton"] = top._res_7693; // "Bouton";
                grapesjs.xrmLang[this.language].blockCustomRes["blockList1Col"] = top._res_2220; // "Article";
                grapesjs.xrmLang[this.language].blockCustomRes["blockDivider"] = top._res_7292; // "Séparateur";
                grapesjs.xrmLang[this.language].blockCustomRes["blockHeader"] = top._res_8263; // "Entête";
                grapesjs.xrmLang[this.language].blockCustomRes["blockBody"] = top._res_5091; // "Corps";
                grapesjs.xrmLang[this.language].blockCustomRes["blockFooter"] = top._res_2267; // "Pied de page";
                grapesjs.xrmLang[this.language].blockCustomRes["contentSimpleText"] = top._res_2196; // "Votre texte ici";
                grapesjs.xrmLang[this.language].blockCustomRes["contentButton"] = top._res_7870; // "En savoir plus";
                grapesjs.xrmLang[this.language].blockCustomRes["contentList1Col"] = top._res_2197; // "Votre titre ici
                grapesjs.xrmLang[this.language].blockCustomRes["contentLinkLeftHeader"] = top._res_2272; // "Si vous ne parvenez pas à visualiser cet email, veuillez";
                grapesjs.xrmLang[this.language].blockCustomRes["contentLinkRightHeader"] = top._res_2271; // "cliquer ici";
                grapesjs.xrmLang[this.language].blockCustomRes["contentTitreFooter"] = top._res_2197; // "Votre titre ici";
                grapesjs.xrmLang[this.language].blockCustomRes["contentTextFooter"] = top._res_2268; // "Pour retrouver toutes nos offres et nos actualités...";
                grapesjs.xrmLang[this.language].blockCustomRes["contentLinkLeftFooter"] = top._res_2269; // "Si vous ne souhaitez plus recevoir de ce courriel";
                grapesjs.xrmLang[this.language].blockCustomRes["contentLinkRightFooter"] = top._res_2270; // "vous pouvez gérer vos préférences ici.";

                //Tâche 2703
                grapesjs.xrmLang[this.language].blockCustomRes["blockExtendedInputText"] = top._res_2599; // "Libellé de la rubrique";
                grapesjs.xrmLang[this.language].blockCustomRes["blockExtendedInput"] = top._res_2601; // "Texte court";
                //grapesjs.xrmLang[this.language].blockCustomRes["inputFields"] = top._res_2600; // "champs de saisie";

                //Tâche #2 860
                grapesjs.xrmLang[this.language].blockCustomRes["blockExtendedInputNum"] = top._res_236; // "Numérique";

                //Tâche #3 231
                grapesjs.xrmLang[this.language].blockCustomRes["blockExtendedInputDate"] = top._res_231; // "Date";

                //Tâche #3 233
                grapesjs.xrmLang[this.language].blockCustomRes["blockExtendedInputMemo"] = top._res_2688; // "Memo";

                //Tâche #2 770
                grapesjs.xrmLang[this.language].blockCustomRes["blockMailInput"] = top._res_2365; // "Courriel";

                //Tâche #2 778
                grapesjs.xrmLang[this.language].blockCustomRes["blockPhoneInput"] = top._res_5138; // "Téléphone";

                //Tâche #2 850
                grapesjs.xrmLang[this.language].blockCustomRes["blockCheckboxInput"] = top._res_2204; // "Case à cocher";

                //Tâche #3 825
                grapesjs.xrmLang[this.language].blockCustomRes["blockExtendedDropdown"] = top._res_2806; // "Liste à choix unique";

                grapesjs.xrmLang[this.language].blockCustomRes["blockExtendedMultiselect"] = top._res_247; // "Liste à choix multiple";

                //Tâche #2 031
                grapesjs.xrmLang[this.language].blockCustomRes["blockButtonSubmit"] = top._res_28.toUpperCase(); //"Valider";

                //tâche #2 683
                grapesjs.xrmLang[this.language].blockCustomRes["formBlocks"] = top._res_6610; // "Formulaires";
                grapesjs.xrmLang[this.language].blockCustomRes["blocForm"] = top._res_1142; // "Formulaire";
                grapesjs.xrmLang[this.language].blockCustomRes["blocWorldlinePaiment"] = top._res_8771; // "Worldline Payment";
                grapesjs.xrmLang[this.language].blockCustomRes["blocWorldline"] = top._res_8773; // "Submit and pay";


                grapesjs.xrmLang[this.language].buttonToggleImages = top._res_2114; // "Afficher/Masquer les images";
                grapesjs.xrmLang[this.language].buttonMove = top._res_2115; // "Déplacer";
                grapesjs.xrmLang[this.language].buttonOpenBlocks = top._res_2116; // "Blocs"; // User Story #297
                grapesjs.xrmLang[this.language].buttonOpenLayers = top._res_2117; // "Arborescence des blocs"; // User Story #297
                grapesjs.xrmLang[this.language].buttonStyleManager = top._res_2118; // "Styles du bloc"; // User Story #297
                grapesjs.xrmLang[this.language].buttonTraitManager = top._res_2119; // "Paramètres du bloc"; // User Story #297


                grapesjs.xrmLang[this.language].dialogAssetsTitle = top._res_2595; // "Ajouter une ressource";
                grapesjs.xrmLang[this.language].styleManagerSelectElement = top._res_2121; // "Sélectionnez un élément afin d'afficher ses propriétés de style ici";
                grapesjs.xrmLang[this.language].styleManagerSectorsGeneral = top._res_224; // "Général";
                grapesjs.xrmLang[this.language].styleManagerSectorsLayout = top._res_2122; // "Disposition";

                grapesjs.xrmLang[this.language].styleManagerSectorsDecorations = top._res_2140; // "Couleur et bordure"; // Backlog #432
                grapesjs.xrmLang[this.language].styleManagerSectorsDecorationsBackground = top._res_1978; // Couleur de fond; // top._res_2141; // "Arrière-plan"; // Backlog #432
                grapesjs.xrmLang[this.language].styleManagerSectorsDecorationsBorder = top._res_1509; // "Bordure";
                grapesjs.xrmLang[this.language].styleManagerSectorsDecorationsBorderRadius = top._res_2142; // "Bords arrondis";
                grapesjs.xrmLang[this.language].styleManagerSectorsDecorationsBorderRadiusTop = top._res_1044; // "Haut";
                grapesjs.xrmLang[this.language].styleManagerSectorsDecorationsBorderRadiusRight = top._res_2126; // "Droite";
                grapesjs.xrmLang[this.language].styleManagerSectorsDecorationsBorderRadiusBottom = top._res_1045; // "Bas";
                grapesjs.xrmLang[this.language].styleManagerSectorsDecorationsBorderRadiusLeft = top._res_2124; // "Gauche";
                grapesjs.xrmLang[this.language].styleManagerSectorsDecorationsBorderCollapse = top._res_2143; // "Fusion des bordures"
                grapesjs.xrmLang[this.language].styleManagerSectorsDecorationsBorderCollapseNo = top._res_59; // "Non"
                grapesjs.xrmLang[this.language].styleManagerSectorsDecorationsBorderCollapseYes = top._res_58; // "Oui";

                grapesjs.xrmLang[this.language].styleManagerSectorsDecorationsBorderWidth = top._res_1508; // "Largeur";
                grapesjs.xrmLang[this.language].styleManagerSectorsDecorationsBorderStyle = top._res_7926; // "Style";
                grapesjs.xrmLang[this.language].styleManagerSectorsDecorationsBorderColor = top._res_2128; // "Couleur";
                grapesjs.xrmLang[this.language].styleManagerSectorsDecorationsBackgroundImage = top._res_2145; // "Image de fond";
                grapesjs.xrmLang[this.language].styleManagerSectorsDecorationsBackgroundRepeat = top._res_1528; // "Répéter";
                grapesjs.xrmLang[this.language].styleManagerSectorsDecorationsBackgroundPosition = top._res_2146; // "Position";
                grapesjs.xrmLang[this.language].styleManagerSectorsDecorationsBackgroundAttachment = top._res_2147; // "Ancrage";
                grapesjs.xrmLang[this.language].styleManagerSectorsDecorationsBackgroundSize = top._res_106; // "Taille";

                grapesjs.xrmLang[this.language].styleManagerSectorsDimensions = top._res_1976; // "Dimension et marge";
                grapesjs.xrmLang[this.language].styleManagerSectorsDimensionsSize = top._res_106; // "Taille";
                grapesjs.xrmLang[this.language].styleManagerSectorsDimensionsSizeWidth = top._res_1508; // "Largeur";
                grapesjs.xrmLang[this.language].styleManagerSectorsDimensionsSizeHeight = top._res_1507; // "Hauteur";
                grapesjs.xrmLang[this.language].styleManagerSectorsDimensionsSizeMaxWidth = top._res_2080; // "Largeur max";
                grapesjs.xrmLang[this.language].styleManagerSectorsDimensionsSizeMaxHeight = top._res_2081; // "Hauteur max";
                grapesjs.xrmLang[this.language].styleManagerSectorsDimensionsSizeMinWidth = top._res_2082; // "Largeur min";
                grapesjs.xrmLang[this.language].styleManagerSectorsDimensionsSizeMinHeight = top._res_2083; // "Hauteur min";
                grapesjs.xrmLang[this.language].styleManagerSectorsDimensionsMargin = top._res_1513; // "Marge";
                grapesjs.xrmLang[this.language].styleManagerSectorsDimensionsMarginTop = top._res_2123; // "Haute";
                grapesjs.xrmLang[this.language].styleManagerSectorsDimensionsMarginLeft = top._res_2124; // "Gauche";
                grapesjs.xrmLang[this.language].styleManagerSectorsDimensionsMarginBottom = top._res_2125; // "Basse";
                grapesjs.xrmLang[this.language].styleManagerSectorsDimensionsMarginRight = top._res_2126; // "Droite";
                grapesjs.xrmLang[this.language].styleManagerSectorsDimensionsPadding = top._res_2084; // "Marge int.";
                grapesjs.xrmLang[this.language].styleManagerSectorsDimensionsPaddingTop = top._res_2123; // "Haute";
                grapesjs.xrmLang[this.language].styleManagerSectorsDimensionsPaddingRight = top._res_2126; // "Droite";
                grapesjs.xrmLang[this.language].styleManagerSectorsDimensionsPaddingBottom = top._res_2125; // "Basse";
                grapesjs.xrmLang[this.language].styleManagerSectorsDimensionsPaddingLeft = top._res_2124; // "Gauche";
                grapesjs.xrmLang[this.language].styleManagerSectorsTypography = top._res_2129; // "Alignement"; // top._res_1108; // "Propriétés"; //"Typographie"; // Backlog #38 puis #432
                grapesjs.xrmLang[this.language].styleManagerSectorsTypograph = top._res_2586;
                grapesjs.xrmLang[this.language].styleManagerSectorsTypographyFontFamily = top._res_1510; // "Police";
                grapesjs.xrmLang[this.language].styleManagerSectorsTypographyFontWeight = top._res_2127; // "Epaisseur"
                grapesjs.xrmLang[this.language].styleManagerSectorsTypographyFontColor = top._res_2128; // "Couleur";
                grapesjs.xrmLang[this.language].styleManagerSectorsTypographyTextAlign = top._res_2212; // "Alignement du texte"; // top._res_2129; // "Alignement"; // Backlog #38 puis #432
                grapesjs.xrmLang[this.language].styleManagerSectorsTypographyTextAlignLeft = top._res_2124; // "Gauche";
                grapesjs.xrmLang[this.language].styleManagerSectorsTypographyTextAlignCenter = top._res_2130; // "Centré";
                grapesjs.xrmLang[this.language].styleManagerSectorsTypographyTextAlignRight = top._res_2126; // "Droite";
                grapesjs.xrmLang[this.language].styleManagerSectorsTypographyTextAlignJustify = top._res_2131; // "Justifié";
                grapesjs.xrmLang[this.language].styleManagerSectorsTypographyTextAlignUnderline = top._res_701; // "Souligné";
                grapesjs.xrmLang[this.language].styleManagerSectorsTypographyTextAlignLineThrough = top._res_2132; // "Barré";
                grapesjs.xrmLang[this.language].styleManagerSectorsTypographyTextDecorationNone = top._res_238; // "Aucune";
                grapesjs.xrmLang[this.language].styleManagerSectorsTypographyFontSize = top._res_1836; // "Style de police";
                grapesjs.xrmLang[this.language].styleManagerSectorsTypographyFontStyle = top._res_1837; // "Style de police";
                grapesjs.xrmLang[this.language].styleManagerSectorsTypographyFontStyleNormal = top._res_6056; // "Normal";
                grapesjs.xrmLang[this.language].styleManagerSectorsTypographyFontStyleItalic = top._res_700; // "Italique";
                grapesjs.xrmLang[this.language].styleManagerSectorsTypographyVerticalAlign = top._res_2133; // "Alignement vertical";
                grapesjs.xrmLang[this.language].styleManagerSectorsTypographyVerticalAlignBaseline = top._res_2134; // "Ligne de base";
                grapesjs.xrmLang[this.language].styleManagerSectorsTypographyVerticalAlignTop = top._res_1044; // "Haut";
                grapesjs.xrmLang[this.language].styleManagerSectorsTypographyVerticalAlignMiddle = top._res_2135; // "Milieu";
                grapesjs.xrmLang[this.language].styleManagerSectorsTypographyVerticalAlignBottom = top._res_1045; // "Bas";
                grapesjs.xrmLang[this.language].styleManagerSectorsTypographyTextShadow = top._res_2136; // "Ombrage du texte";
                grapesjs.xrmLang[this.language].styleManagerSectorsTypographyTextShadowX = top._res_2137; // "Position X";
                grapesjs.xrmLang[this.language].styleManagerSectorsTypographyTextShadowY = top._res_2138; // "Position Y";
                grapesjs.xrmLang[this.language].styleManagerSectorsTypographyTextShadowBlur = top._res_2139; // "Flou";
                grapesjs.xrmLang[this.language].styleManagerSectorsTypographyTextShadowColor = top._res_2128; // "Couleur";

                grapesjs.xrmLang[this.language].styleManagerSectorsTypographyBoxShadowX = top._res_2137; // "Position X"
                grapesjs.xrmLang[this.language].styleManagerSectorsTypographyBoxShadowY = top._res_2138; // "Position Y";
                grapesjs.xrmLang[this.language].styleManagerSectorsTypographyBoxShadowBlur = top._res_2139; // "Flou";
                grapesjs.xrmLang[this.language].styleManagerSectorsTypographyBoxShadowSpread = top._res_2144; // "Répartition";
                grapesjs.xrmLang[this.language].styleManagerSectorsTypographyBoxShadowColor = top._res_2128; // "Couleur";
                grapesjs.xrmLang[this.language].styleManagerSectorsTypographyBoxShadowType = top._res_105; // "Type";
                grapesjs.xrmLang[this.language].styleManagerSectorsTypographyWeight = top._res_2587;//Weight
                grapesjs.xrmLang[this.language].styleManagerSectorsTypographySpacing = top._res_2588;//Espacement
                grapesjs.xrmLang[this.language].styleManagerSectorsTypographyLineHeight = top._res_2589;//Font Color
                grapesjs.xrmLang[this.language].styleManagerSectorsTypographyTextDecoration = top._res_2590;//Effet

                grapesjs.xrmLang[this.language].traitManagerSelectElement = top._res_2148; // "Sélectionnez un élément afin d'afficher ses paramètres ici";
                grapesjs.xrmLang[this.language].traitManagerContainerLabel = top._res_2149; // "Paramètres de l'élément";
                grapesjs.xrmLang[this.language].traitManagerPlaceHolderLabel = top._res_223; // "Libellé";
                grapesjs.xrmLang[this.language].traitManagerPlaceHolderHref = top._res_2150; // "https://www.exemple.com";
                grapesjs.xrmLang[this.language].traitManagerOptionsTargetThis = top._res_2151; // "Fenêtre courante";
                grapesjs.xrmLang[this.language].traitManagerOptionsTargetBlank = top._res_2152; // "Nouvelle fenêtre";

                grapesjs.xrmLang[this.language].selectorManagerClasses = top._res_2153; // "Classes";
                grapesjs.xrmLang[this.language].selectorManagerSelected = top._res_187; // "Sélection"
                grapesjs.xrmLang[this.language].selectorManagerState = "- " + top._res_2154 + " -"; // "- Etat -";
                grapesjs.xrmLang[this.language].selectorManagerStateHover = top._res_2155; // "Survol";
                grapesjs.xrmLang[this.language].selectorManagerStateActive = top._res_2156; // "Cliqué/Actif";
                grapesjs.xrmLang[this.language].selectorManagerStateEvenOdd = top._res_2157; // "Pair/Impair";

                ////AABBA Tache #2 681
                grapesjs.xrmLang[this.language].uploadText = top._res_2593;
                grapesjs.xrmLang[this.language].addButton = top._res_2594;
                grapesjs.xrmLang[this.language].inputPath = top._res_2597;

            }
        }
    };

    //tâche #2 573 - affecter la liste de blocs GrapesJS
    this.setGrapesJSCustomInfos = function (strGrapesJSBlocks, nTabFrom, wordlineBlocs) {
        this.grapesJSBlocks = strGrapesJSBlocks;
        this.nTabFrom = nTabFrom;
        if (wordlineBlocs)
            this.wordlineBlocs = wordlineBlocs;

    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    // LECTURE ET ECRITURE DE DONNEES
    // Ces méthodes sont appelées, soit pour récupérer du contenu, soit pour le mettre à jour en base
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------

    // Renvoie un pointeur vers l'iframe affichant le champ Mémo
    this.getMemoFrame = function () {
        var oFrame = null;
        if (this.isHTML) {
            // CKEditor
            if (this.htmlEditor && !this.htmlEditor.isFake && (this.htmlEditor.container || this.htmlEditor.window)) {
                if (this.inlineMode) {
                    oFrame = this.htmlEditor.container.$;
                }
                else {
                    if (this.htmlEditor.window) {
                        oFrame = this.htmlEditor.window.$.frameElement;
                        if (!oFrame) {
                            if (this.htmlEditor.window.$)
                                oFrame = this.htmlEditor.window.$;
                            else
                                oFrame = this.htmlEditor.window;
                        }
                    }
                }
            }
            // grapesjs
            if (this.htmlTemplateEditor) {
                oFrame = this.htmlTemplateEditor.Canvas.getFrameEl();
            }
        }
        else {
            oFrame = this.textEditor;
        }
        return oFrame;
    };

    // Renvoie un pointeur vers l'objet JS document affichant le contenu du champ Mémo (intérieur de la balise <body> pour les mémos HTML)
    this.getMemoDocument = function () {
        var oDoc = null;
        if (this.isHTML) {
            // CKEditor
            if (this.htmlEditor && !this.htmlEditor.isFake && (this.htmlEditor.container || this.htmlEditor.window)) {
                if (this.inlineMode) {
                    oDoc = this.htmlEditor.container.$;
                }
                else {
                    var oEditorFrame = this.htmlEditor.window.$.frameElement;
                    if (oEditorFrame)
                        oDoc = oEditorFrame.contentWindow.document;
                    else
                        oDoc = this.htmlEditor.window.$.document;
                }
            }

            // grapesjs
            if (this.htmlTemplateEditor) {
                oDoc = this.htmlTemplateEditor.Canvas.getBody().ownerDocument;
            }
        }
        else {
            oDoc = this.textEditor;
        }
        return oDoc;
    }

    // Renvoie un pointeur vers l'intérieur du contenu du champ Mémo (intérieur de la balise <body> pour les mémos HTML)
    this.getMemoBody = function () {
        var oDoc = this.getMemoDocument();
        if (!this.inlineMode) {
            if (oDoc && oDoc.getElementsByTagName)
                oDoc = oDoc.getElementsByTagName("body")[0];
        }

        return oDoc;
    }

    // Renvoie le contenu du champ Mémo
    this.getData = function () {
        var memoData = this.value;
        if (this.isHTML) {
            if (this.htmlEditor && !this.htmlEditor.isFake) {
                try {
                    this.htmlEditor.updateElement(); // mise à jour du champ <textarea> original avec le contenu modifié
                    memoData = this.htmlEditor.getData();
                }
                catch (ex) {
                    if (!this.inlineMode) {
                        if (this.htmlEditor.document &&
                            this.htmlEditor.document.$ &&
                            this.htmlEditor.document.$.body &&
                            this.htmlEditor.document.$.body.innerHTML
                        ) {
                            memoData = this.htmlEditor.document.$.body.innerHTML;
                        }
                    }
                }
            }
            else {
                memoData = this.virtualTextAreaValue;
            }
        }
        else {
            if (this.textEditor)
                memoData = this.textEditor.value;
            else
                memoData = this.virtualTextAreaValue;
        }

        // #68 13x - Prise en charge de l'éditeur de templates avancé
        if (this.htmlTemplateEditor) {
            // https://github.com/artf/grapesjs/wiki/API-Editor#gethtml
            // https://github.com/artf/grapesjs/issues/557
            // Backlog #261, #295, #445 - Le contenu renvoyé par l'API grapesjs ne se met à jour que lorsque le curseur sort du composant édité
            // Pour forcer la mise à jour lors d'un appel à getData() alors que le curseur s'y trouve toujours (cas de l'édition d'un modèle et d'un clic sur le bouton
            // Valider sans cliquer au préalable ailleurs), on désactive l'édition en cours avant de renvoyer le contenu
            // https://github.com/artf/grapesjs/issues/1767
            // https://github.com/artf/grapesjs/issues/1327
            // https://github.com/artf/grapesjs/issues/319
            // https://github.com/artf/grapesjs/issues/179
            this.disableEditing();
            // Backlog #427 - Le contenu HTML du canevas doit être renvoyé avec des styles inline pour rester compatible avec Outlook
            // Il faut pour cela utiliser la commande gjs-get-inlined-html de grapesjs, plutôt que sa fonction getHtml()
            // Source : https://github.com/artf/grapesjs-preset-newsletter/issues/4
            // Dans ce cas précis, il n'y a pas besoin de renvoyer les styles définis par grapesjs séparément.
            // Notre CSS, contenant entre autres le nécessaire pour le responsive, sera, quand à elle, réinjectée via injectCSS()
            //memoData = this.htmlTemplateEditor.getHtml(); // Avec grapesjs, les CSS inline (dans le body) ne sont PAS renvoyées par getHtml(). Pour récupérer les CSS, utiliser eMemoEditor.getCss()
            // Demande #72 138/#72 207 - Nettoyage des CSS grapesjs également à la lecture (ex : pour l'envoi du mail définitif)
            memoData = this.cleanEditorCss(this.htmlTemplateEditor.runCommand("gjs-get-inlined-html"));
        }

        // Backlog #451 - Suppression des caractères Zero-width Space (Unicode) parfois présents dans le contenu, notamment à l'insertion de champs de fusion,
        // donnant des ??? à l'interprétation. Et autres caractères inutiles de ce style
        // cf. correctif précédemment effectué sur UserMessage pour la même raison : #31 571
        // Les insertions de ce genre de caractères sont souvent provoquées par les méthodes de manipulation de Sélections/Ranges en JavaScript
        // https://stackoverflow.com/questions/11305797/remove-zero-width-space-characters-from-a-javascript-string
        memoData = eTools.removeHiddenSpecialChars(memoData);

        if (this.debugLevel > 1) {
            this.trace("Valeur initiale du champ Mémo (getData) : " + this.value);
            this.trace("Valeur actuelle du champ Mémo (getData) : " + memoData);
        }

        return memoData;
    };

    // Met à jour le contenu du champ Mémo
    this.setData = function (memoData, callback, resetColor) {
        setWait(true);

        // Backlog #451 - Suppression des caractères Zero-width Space (Unicode) parfois insérés, notamment avec des champs de fusion, donnant des ??? à l'interprétation
        // Et autres caractères inutiles de ce style
        // cf. correctif précédemment effectué sur UserMessage pour la même raison : #31 571
        // Les insertions de ce genre de caractères sont souvent provoquées par les méthodes de manipulation de Sélections/Ranges en JavaScript
        // https://stackoverflow.com/questions/11305797/remove-zero-width-space-characters-from-a-javascript-string
        memoData = eTools.removeHiddenSpecialChars(memoData);

        if (this.isHTML) {
            // #68 13x - Mise à jour de l'éditeur de templates avancé
            if (this.htmlTemplateEditor) {

                // Backlog #450 - Reset du canevas et de ses CSS existantes avant injection de memoData
                // Semble poser de nombreux problèmes, donc désactivé pour l'instant. On ne fait que remplacer le contenu via setComponents()
                // https://github.com/artf/grapesjs/issues/351
                // https://github.com/artf/grapesjs/issues/1115
                // https://github.com/artf/grapesjs/issues/1357
                // https://github.com/artf/grapesjs/issues/986
                // https://github.com/artf/grapesjs/issues/552
                // #71 938 - Suite à la mise en place d'options influençant la gestion des CSS (ex : forceClass),
                // reprise du reset des CSS pour corriger les différences de rendu entre 2 grapesjs de contexte différent
                // (ex : édition de modèle/utilisation) : https://github.com/artf/grapesjs/issues/488
                this.htmlTemplateEditor.setStyle('');
                this.htmlTemplateEditor.CssComposer.getAll().reset();
                this.htmlTemplateEditor.DomComponents.getWrapper().setStyle('');
                this.htmlTemplateEditor.DomComponents.getWrapper().set('content', '');
                this.htmlTemplateEditor.DomComponents.clear();

                memoData = this.loadMergefieldComponentElement(memoData);

                //Injection des CSS des fonts du nouvel éditeur des formulaires dans GrapesJS
                if (this.enableAdvancedFormular)
                    this.loadFontsCss();

                // Demande #71 938/72 070 - Nettoyage des classes CSS c*** de l'éditeur de templates HTML avancé (grapesjs)
                // US #918 - Demande #72 814 - On appelle ici une version surchargée de setComponents (cf. createHTMLTemplateEditor) qui renverra une eAlert si le code à charger comporte des erreurs provoquant une DOMException
                this.htmlTemplateEditor.setComponents(this.cleanEditorCss(memoData));

                //verrouiller les champs de fusion 'mergefield'
                lockEditableElement(this.getMemoFrame().contentWindow.document.body, false);
                //this.htmlTemplateEditor.setStyle(memoData.trim());
            }

            // Mise à jour d'un champ CKEditor physiquement présent sur la page via appel asynchrone (CKEDITOR.setData)
            // qui exécutera ensuite la fonction passée en callback une fois que la mise à jour de CKEditor sera effective
            else if (this.htmlEditor && !this.htmlEditor.isFake) {
                try {
                    this.htmlEditor.setData(memoData, callback);
                    if (this.debugLevel > 1)
                        this.trace("Valeur envoyée à CKEditor pour mise à jour : " + memoData);
                }
                catch (ex) {
                    if (this.htmlEditor.document &&
                        this.htmlEditor.document.$ &&
                        this.htmlEditor.document.$.body &&
                        this.htmlEditor.document.$.body.innerHTML
                    ) {
                        this.htmlEditor.document.$.body.innerHTML = memoData;
                    }
                }
            }

            // Sinon, mise à jour de la variable interne représentant le contenu d'un champ Mémo HTML
            else {
                this.virtualTextAreaValue = memoData;
                if (this.debugLevel > 1)
                    this.trace("Valeur mise à jour sur le champ Mémo HTML : " + memoData);

                // Exécution du code de sortie passé à la fonction
                if (callback) {
                    callback();
                }
            }

            // Backlog #619 - Remise à zéro de la couleur de fond
            if (resetColor) {
                this.setColor("");
            }
        }
        else {
            // Mise à jour directe du textarea physiquement présent sur la page
            if (this.textEditor) {
                this.textEditor.value = memoData;
            }
            // Sinon, mise à jour de la variable interne représentant le contenu d'un champ Mémo texte brut
            else {
                this.virtualTextAreaValue = memoData;
            }
            if (this.debugLevel > 1)
                this.trace("Valeur mise à jour sur le champ Mémo : " + memoData);

            // Exécution du code de sortie passé à la fonction
            if (callback) {
                callback();
            }
        }

        setWait(false);
    };

    // Backlog #652 - Récupère la couleur de fond à utiliser à partir d'une chaîne CSS
    this.getColorFromCSS = function (sCss) {
        return eTools.getCssRuleFromString(sCss, "body", "background-color", true);
    }

    this.injectCSS = function (sNewCSSValue, isNativeCSS) {
        if (!this.isHTML || !sNewCSSValue)
            return;

        // Si la valeur passée en paramètre est une chaîne unique, on la transforme en tableau
        if (typeof (sNewCSSValue) == "string") {
            sNewCSSValue = new Array(sNewCSSValue);
        }
        // On parcourt chaque CSS à ajouter
        for (var i = 0; i < sNewCSSValue.length; i++) {
            // Si la valeur se termine par .css, on considère que c'est une référence à un fichier externe à ajouter comme telle
            if (sNewCSSValue[i].indexOf(".css") == sNewCSSValue[i].length - 4) {
                // CKEditor
                if (this.htmlEditor && !this.htmlEditor.isFake) {
                    //#32178 On applique la css qu'on mode 'wysiwyg' car le mode 'source' ne dispose pas de balise style
                    if (this.htmlEditor.mode == 'wysiwyg') {
                        // Ajout des styles sans passer par l'API CKEditor pour que les styles soient pris en compte immédiatement
                        // Backlog #267 - TODO si nécessaire

                        // Puis ajout de ces styles avec l'API pour qu'ils soient rechargés lors d'un switch entre mode design et mode HTML
                        // Nouvelle méthode CKEditor 4 https://ckeditor.com/docs/ckeditor4/latest/api/CKEDITOR_dom_document.html#method-appendStyleSheet
                        if (this.htmlEditor.document && this.htmlEditor.document.appendStyleSheet)
                            this.htmlEditor.document.appendStyleSheet(sNewCSSValue[i]);
                        /* Générateur de formulaires */
                        // TODO XRM
                    }
                }
                // grapesjs - Backlog #92 et #267
                if (this.htmlTemplateEditor) {
                    // L'ajout d'une CSS externe via l'API grapesjs peut se faire en réalité via addComponents() ou getComponents().add(),
                    // en ajoutant directement la balise <link> : https://github.com/artf/grapesjs/issues/66#issuecomment-293246613
                    // Mais cette méthode ajoute alors cette balise dans <body>, qui est alors écrasé lorsqu'on fait appel à setData() pour par ex. charger un modèle
                    // sans compter que ça permet à l'utilisateur de supprimer la feuille de style via le bouton Source.
                    // On injecte alors directement notre CSS dans le <head> de l'iframe, ce qui lui permet de ne pas être contrôlée par l'éditeur, ce qui en réalité,
                    // est exactement ce qu'on recherche
                    //this.htmlTemplateEditor.addComponents('<link rel="stylesheet" href="' + sNewCSSValue[i] + '" type="text/css">', false);
                    //this.htmlTemplateEditor.getComponents().add('<link rel="stylesheet" href="' + sNewCSSValue[i] + '" type="text/css">', false);
                    var editorFrame = this.htmlTemplateEditor.Canvas.getFrameEl();
                    // On passe par eTools.addCss afin d'ajouter la CSS avec le chemin parent correspondant à l'application
                    // Mais il faut alors retirer le .css du chemin, car il est justement ajouté par addCss()
                    addCss(sNewCSSValue[i].substring(0, sNewCSSValue[i].length - 4), "MEMO", editorFrame.contentDocument);
                }
            }
            // Sinon, on injecte le contenu en inline <style>
            else {
                // CKEditor
                if (this.htmlEditor && !this.htmlEditor.isFake) {
                    //#32178 On applique la css qu'on mode 'wysiwyg' car le mode 'source' ne dispose pas de balise style
                    if (this.htmlEditor.mode == 'wysiwyg') {
                        // Ajout des styles sans passer par l'API CKEditor pour que les styles soient pris en compte immédiatement
                        var oStyles = this.htmlEditor.document.$.getElementsByTagName("style");
                        if (oStyles[0].styleSheet) {
                            // IE6,7 et 8
                            oStyles[0].styleSheet.cssText = sNewCSSValue[i];
                        } else {
                            // Firefox 3.x
                            oStyles[0].innerHTML = sNewCSSValue[i];
                        }

                        // Puis ajout de ces styles avec l'API pour qu'ils soient rechargés lors d'un switch entre mode design et mode HTML
                        // Nouvelle méthode CKEditor 4 ? https://ckeditor.com/docs/ckeditor4/latest/api/CKEDITOR_dom_document.html#method-appendStyleText
                        if (this.htmlEditor.document && this.htmlEditor.document.appendStyleText)
                            this.htmlEditor.document.appendStyleText(sNewCSSValue[i]);
                        // Ancienne méthode CKEditor 3 ? https://docs-old.ckeditor.com/ckeditor_api/symbols/CKEDITOR.editor.html#addCss
                        else if (this.htmlEditor.addCss)
                            this.htmlEditor.addCss(sNewCSSValue[i]);
                        /* Générateur de formulaires */
                        // TODO XRM
                    }
                }
                // grapesjs - Backlog #92
                // https://github.com/artf/grapesjs/issues/66
                if (this.htmlTemplateEditor) {
                    var addInBody = false;
                    this.trace("Injection de CSS au sein de l'éditeur avancé : " + sNewCSSValue[i]);
                    setWait(true);
                    if (addInBody) {
                        // Le second paramètre indique de mettre à jour les styles existants du document avec ceux passés en paramètre
                        // Si on le passe à true, les styles ne seront pas mis à jour s'ils existent déjà
                        // https://github.com/artf/grapesjs/wiki/API-Editor#addcomponents
                        this.htmlTemplateEditor.addComponents("<style>" + sNewCSSValue[i] + "</style>", false);
                    }
                    // Demande #72 138/#72 207 - Ajout dans <head>
                    else {
                        var editorFrame = this.htmlTemplateEditor.Canvas.getFrameEl();
                        // On passe par eTools.addCSSText en lui précisant le document de notre iframe, et un id pour pouvoir y réaccéder (et MAJ si existant)
                        // On écrase le contenu si déjà définie (paramètre à false)
                        addCSSText(sNewCSSValue[i], editorFrame.contentDocument, "eME_extCss_" + this.name, false);
                    }
                    setWait(false);
                    this.trace("Injection de CSS terminée au sein de l'éditeur avancé !");
                }
            }

            // Si on appelle injectCSS pour ajouter une seule CSS personnalisable par l'utilisateur, on la déclare dans customCSS
            // On considère alors qu'un seul appel à injectCSS a été fait (par les modal dialogs) et que la CSS actuelle remplacera toute éventuelle CSS précédemment
            // rattachée à cette propriété customCSS
            // A l'inverse, si injectCSS est appelée en interne pour injecter une CSS native de l'application dans l'iframe de l'éditeur, on ne le fait surtout pas
            if (!isNativeCSS)
                this.setCss(sNewCSSValue[i]);
        }
    };

    // Backlog #619 - Paramètre la couleur de fond actuellement positionnée sur l'éditeur (indépendamment de son contenu)
    this.setColor = function (color) {
        if (!color || color == "")
            color = "white"; // couleur par défaut définie dans editor.css pour cke_wysiwyg_frame (CKEditor)

        if (this.isHTML) {
            // grapesjs
            if (this.htmlTemplateEditor) {
                var wrapper = this.htmlTemplateEditor.getWrapper();
                if (wrapper && wrapper.getEl()) {
                    wrapper.getEl().style.backgroundColor = color;
                }
            }
            // CKEditor
            if (this.htmlEditor && !this.htmlEditor.isFake) {
                var editorIFrame = this.htmlEditor.container.$.querySelector(".cke_wysiwyg_frame");
                var editorCanvas = this.htmlEditor.container.$.querySelector(".cke_contents");
                if (editorIFrame)
                    editorIFrame.style.backgroundColor = color;
                if (editorCanvas)
                    editorCanvas.style.backgroundColor = color;
            }
        }
        // Texte brut
        else if (this.textEditor)
            this.textEditor.style.backgroundColor = color;
    }

    // Insère du code HTML ou du texte dans le champ Mémo, en prenant le soin d'effectuer des traitements en interne si nécessaire
    // strData = code HTML ou texte à insérer
    // bInsertRawHTML = indique si on insère directement le code (true, méthode recommandée sur IE),
    // ou s'il faut d'abord créer un élément HTML dans le DOM avant de l'insérer en tant qu'élément (false, recommandée sur les autres navigateurs)
    // ATTENTION : Ce paramètre doit être passé à true si on insère uniquement du texte
    // bFocusBeforeInsert : si true, on positionne le focus dans le champ AVANT insertion (peut être requis par certains navigateurs)
    // nCursorPositionBeforeInsert : si défini à autre chose que -1, on positionne le curseur à cette position AVANT de faire l'insertion
    // bSkipHTMLTemplateEditor : si true, effectue l'insertion directement via CKEditor sans passer par grapesjs, même si l'éditeur utilisé est grapesjs
    // -> cas de l'insertion des champs de fusion, cf. backlog #320
    this.insertData = function (strData, bInsertRawHTML, bFocusBeforeInsert, nCursorPositionBeforeInsert, bSkipHTMLTemplateEditor) {
        // Backlog #451 - Suppression des caractères Zero-width Space (Unicode) parfois insérés, notamment avec des champs de fusion, donnant des ??? à l'interprétation
        // Et autres caractères inutiles de ce style
        // cf. correctif précédemment effectué sur UserMessage pour la même raison : #31 571
        // Les insertions de ce genre de caractères sont souvent provoquées par les méthodes de manipulation de Sélections/Ranges en JavaScript
        // https://stackoverflow.com/questions/11305797/remove-zero-width-space-characters-from-a-javascript-string
        strData = eTools.removeHiddenSpecialChars(strData);

        if (bFocusBeforeInsert)
            this.focus();

        if (typeof (nCursorPositionBeforeInsert) != "undefined" && nCursorPositionBeforeInsert > -1)
            this.setCursorPosition(nCursorPositionBeforeInsert);

        // Pour l'éditeur de templates avancé
        if (this.htmlTemplateEditor && !bSkipHTMLTemplateEditor) {
            // https://github.com/artf/grapesjs/issues/111
            var canvasDoc = this.htmlTemplateEditor.Canvas.getBody().ownerDocument;
            // Insert text at the current pointer position
            canvasDoc.execCommand("insertHTML", false, strData);
        }

        // Pour CKEditor
        else if (this.isHTML && this.htmlEditor && !this.htmlEditor.isFake) {
            if (bInsertRawHTML) {
                try {
                    //this.addEndSpaceWithMergeFields = true;
                    this.htmlEditor.insertHtml(strData);
                }
                finally {
                    //this.setCursorPosition(strData.length + 1);
                }
            }
            else {
                // #72 278 - Si le code à insérer débute par &nbsp; (cas si this.addStartSpaceWithMergeFields = true), il faut insérer ce &nbsp; à part pour ne pas provoquer
                // de plantage sur l'API CKEditor
                if (strData.indexOf("&nbsp;") == 0) {
                    this.htmlEditor.insertHtml("&nbsp;");
                    strData = strData.substring(6);
                }
                var oElt = CKEDITOR.dom.element.createFromHtml(strData);
                this.htmlEditor.insertElement(oElt);
                // #72 278 - Même chose si le code finit par &nbsp;, il faut l'insérer explicitement car createFromHtml() l'ignore
                if (strData.indexOf("&nbsp;") == strData.length - 6) {
                    this.htmlEditor.insertHtml("&nbsp;");
                }
            }
        }

        // Pour l'éditeur de texte brut
        else {
            var newValue = this.getData() + strData;
            var returnValue = this.setData(newValue);
            this.setCursorPosition(newValue.length);
            this.focus();
        }
    };

    // Récupérer les styles CSS injectés dans les différents éditeurs
    // editorType =
    // 'CKEDITOR', false, null ou undefined pour récupérer les styles de CKEditor aka.this.htmlEditor (défaut)
    // 'GRAPESJS' pour récupérer ceux de grapesjs aka.this.htmlTemplateEditor,
    // autre valeur non false / null / undefined pour récupérer une concaténation des deux
    // cleanup = si true, les CSS seront dédoublonnées avant renvoi
    this.getCss = function (editorType, cleanup) {
        // Backlog #617 et #648 - Renvoi des CSS grapesjs non activé par défaut
        if (!editorType)
            editorType = 'CKEDITOR';

        var htmlEditorCSS = this.customCSS;
        var htmlTemplateEditorCSS = '';

        // Backlog #339 - Renvoyer les styles CSS inline gérés par grapesjs
        // Backlog #617 et #648 - Uniquement si explicitement demandé
        // Demande #72 138/#72 207
        if (this.htmlTemplateEditor)
            htmlTemplateEditorCSS = this.htmlTemplateEditor.getCss();
        if (this.enableAdvancedFormular)
            htmlTemplateEditorCSS = this.getMemoFrame().contentWindow.document.getElementsByTagName("STYLE")[0].innerHTML + htmlTemplateEditorCSS;

        switch (editorType) {
            case "CKEDITOR": sCss = htmlEditorCSS; break;
            case "GRAPESJS": sCss = htmlTemplateEditorCSS; break;
            default: sCss = htmlEditorCSS + htmlTemplateEditorCSS; break;
        }

        if (cleanup)
            return eTools.cleanCss(sCss);
        else
            return sCss;
    };

    this.setCss = function (sCSS) {
        this.customCSS = sCSS;
    }

    // Demande #71 938/72 070 - Nettoyage des classes CSS c*** de l'éditeur de templates HTML avancé (grapesjs)
    // RegEx by HLA
    this.cleanEditorCss = function (sCSS) {
        //dans le cadre d'un formulaire avancé, on retourne le css récupéré das grape
        if (this.enableAdvancedFormular)
            return sCSS;
        /*
        return sCSS
            .replace(/class="[^"]*?((?: *c[0-9]+)+)[^"]*"/g, '')
            .replace(/class='[^"]*?((?: *c[0-9]+)+)[^"]*'/g, '');
        */

        // On boucle pour remplacer plusieurs occurrences de cXXXXX par class si ces cXXXX sont dans un ordre disparate
        var loop = 0;
        var maxLoop = 10;
        var regex = /(class=")((?:c[0-9]+)+)([^"]*")/gm;
        var regex2 = /(class="[^"]*?)((?: c[0-9]+)+)([^"]*")/gm;
        var subst = "$1$3";

        // The substituted value will be contained in the result variable
        var result = sCSS.replace(regex, subst);
        while (regex2.test(result) && loop < maxLoop) {
            result = result.replace(regex2, subst);
            loop++;
        }

        return result;
    };

    // Insère un champ de fusion sélectionné depuis la combobox (plugin xrmMergeFields)
    this.insertMergeField = function (mergeFieldsComboBox, value, readWrite, extendedType, islnkadv) {
        if (value == "") {
            return;
        }

        var strFieldDescIdAttr = '';
        var strFieldNameAttr = '';
        var strFieldTypeAttr = '';
        var strFieldROAttr = '';
        var strFieldClass = '';
        var strFieldLabel = '';
        var sHtml = '';
        var nFieldLength = 0;
        var strEventOnclick = '';
        var attributes = '';

        if (that.enableAdvancedFormular) {
            function HTMLEncode(str) {

                str = str.replace(/&/g, '&amp;')
                    .replace(/</g, '&lt;')
                    .replace(/>/g, '&gt;')
                    .replace(/'/g, '&#39;')
                    .replace(/"/g, '&#34;');;

                var i = str.length,
                    aRet = [];
                while (i--) {
                    var iC = str[i].charCodeAt();
                    if (iC > 127) {
                        aRet[i] = '&#' + iC + ";";
                    } else {
                        aRet[i] = str[i];
                    }
                }
                return aRet.join("")
            }


            let _MergeField = JSON.parse(this.mergeFields);
            var sText = value.replace("[[", "").replace("]]", "");
            //On associe un événement on click au label pour empêcher l'insertion en double dans merge field
            strEventOnclick = " onclick='top.eTools.setCurrentCursorPosition(top.grapesJsEditor.getMemoFrame().contentWindow, this.parentElement, 0);'";

            var sValue = _MergeField[HTMLEncode(sText)];
            //demande #80 331
            if (!sValue)
                sValue = _MergeField[sText];
        }
        else {

            var sText = mergeFieldsComboBox._.items[value];
            var sValue = value;
        }
        var oMergeFieldData = new eTools.eMergeFieldData(sText, sValue, this.descId);


        // obtention d'un ID unique
        var strMergeFieldId = oMergeFieldData.DescId + '_' + (new Date).getTime();

        // nom du champ avec des espaces insécables (nowrap)
        var strMergeFieldName = '{' + oMergeFieldData.Text + '}';

        // Si le champ est soit de liaison, soit lié, soit appartenant à une autre rubrique que le template ciblé par le formulaire, on l'insère en lecture seule.
        // Sinon, on propose de choisir son mode de fonctionnement (lecture/écriture) depuis une popup séparée, sauf si la fonction insertMergeField
        // a justement été appelée par cette popup après que l'utilisateur ait choisi "Lecture" ou "Ecriture" (paramètre readWrite renseigné)
        var bReadOnly = false;
        if (readWrite != 'R' && readWrite != 'W') {

            bReadOnly = !oMergeFieldData.IsEditable();

            if (bReadOnly == false && this.editorType == 'formular') {
                this.selectedMergeField = value;
                this.htmlEditor.openDialog('xrmFormularReadWriteDialog');
                return false;
            }
        }
        else
            bReadOnly = (readWrite == 'R');

        // Insertion automatique du bouton Valider lorsqu'on insère le premier champ en écriture
        var bInsertSubmitMergeField = false;

        var oLabel = null;
        if (oMergeFieldData.Name == 'pagebreak') {
            // Ajout d'un espace pour forcer le curseur à se positionner en-dehors de la balise <label> sur IE (le faire sous Firefox empêche par contre l'insertion du code)
            // Sous IE 9 en mode natif uniquement, il faut en insérer un avant pour éviter que le <label> soit tronqué lors d'un clic dans le champ Notes (bug de CKEditor ?)
            // #72 278 - On le fait désormais systématiquement
            //if (CKEDITOR.env.ie)
            if (this.addStartSpaceWithMergeFields)
                sHtml = '&nbsp;';
            sHtml += "<label contenteditable=\"false\" style=\"page-break-after:always;\" ednn=\"pagebreak\" ednc=\"special\" >" + strMergeFieldName + "</label> ";
            //if (CKEDITOR.env.ie)
            if (this.addEndSpaceWithMergeFields)
                sHtml += '&nbsp;';
        } else {
            // Ajout d'un espace pour forcer le curseur à se positionner en-dehors de la balise <label> sur IE (le faire sous Firefox empêche par contre l'insertion du code)
            // Sous IE 9 en mode natif uniquement, il faut en insérer un avant pour éviter que le <label> soit tronqué lors d'un clic dans le champ Notes (bug de CKEditor ?)
            //if (CKEDITOR.env.ie)
            if (this.addStartSpaceWithMergeFields)
                sHtml = '&nbsp;';

            //MOU le bouton de validation formulaire a un descid = 000 du coup on test >=
            if (!isNaN(getNumber(oMergeFieldData.DescId)) && getNumber(oMergeFieldData.DescId) >= 0) {
                strFieldDescIdAttr = " ednd=\'" + oMergeFieldData.DescId + "\'";
                if (attributes != '')
                    attributes += ',';
                if (islnkadv)
                    attributes += '"ednd":"0"';
                else
                    attributes += '"ednd":"' + oMergeFieldData.DescId + '"';
            }
            if (oMergeFieldData.Name != '' && oMergeFieldData.Name.toLowerCase() != 'mergefield') {
                strFieldNameAttr = " ednn=\'" + oMergeFieldData.Name + "\'";
                if (attributes != '')
                    attributes += ',';
                if (islnkadv)
                    attributes += '"ednn":"0"';
                else
                    attributes += '"ednn":"' + oMergeFieldData.Name + '"';
            }           
            if (oMergeFieldData.Type != '') {
                strFieldTypeAttr = " ednl=\'" + oMergeFieldData.Type + "\'";
                if (attributes != '')
                    attributes += ',';
                if (islnkadv)
                    attributes += '"ednl":"' + oMergeFieldData.DescId + '", "href":"", "ednlt":"mrg" ';
                else
                    attributes += '"ednl":"' + oMergeFieldData.Type + '"';
            }
            if (oMergeFieldData.Type != '') {
                strFieldTypeAttr = " ednc=\'" + oMergeFieldData.Type + "\'";
                if (attributes != '')
                    attributes += ',';
                if (islnkadv)
                    attributes += '"ednc":"lnk"';
                else
                    attributes += '"ednc":"' + oMergeFieldData.Type + '"';
            }


            if (this.editorType == 'formular' && !islnkadv) {
                strFieldROAttr = " ednro=\'" + (readWrite == "W" ? "0" : "1") + "\'";
                if (attributes != '')
                    attributes += ',';
                attributes += '"ednro":"' + readWrite == "W" ? "0" : "1" + '"';

                if (readWrite == "W") {
                    if (sText.indexOf('.') > -1) {
                        var fieldLabelArray = oMergeFieldData.Text.split('\.');
                        var fieldLabelArrayTemp = [];
                        for (var index = 0; index < fieldLabelArray.length; ++index) {
                            if (fieldLabelArray[index].trim() != "") {
                                fieldLabelArrayTemp.push(fieldLabelArray[index]);
                            }
                        }
                        if (fieldLabelArrayTemp.length > 0)
                            strFieldLabel = fieldLabelArrayTemp[fieldLabelArrayTemp.length - 1] + '&nbsp;';
                    }
                    strFieldClass = ' style="color:red"';
                    // Lorsqu'on insère un champ en Ecriture, on insère également le bouton Valider s'il n'a pas déjà été ajouté
                    var tmpCurrentCodeContainer = document.createElement('div');
                    tmpCurrentCodeContainer.innerHTML = this.getData();
                    var insertedFields = tmpCurrentCodeContainer.querySelectorAll('label');
                    var i = 0;
                    var foundSubmitMergeField = false;
                    while (!foundSubmitMergeField && i < insertedFields.length) {
                        if (getAttributeValue(insertedFields[i], 'ednc') == 'submit')
                            foundSubmitMergeField = true;
                        else
                            i++;
                    }
                    if (!foundSubmitMergeField)
                        bInsertSubmitMergeField = true;
                }
            }

            if (strFieldLabel != '' & oMergeFieldData.Format != oMergeFieldData.FieldFormat.TYP_BIT)
                this.insertData(strFieldLabel, true);

            sHtml += "<label" + strFieldClass + "  data-gjs-type=\"mergefield\" contenteditable=\'false\'" + strFieldDescIdAttr + strFieldNameAttr + strFieldTypeAttr + strFieldROAttr + strEventOnclick + ">" + strMergeFieldName + "</label> ";
            if (extendedType && extendedType === true) {//Tâche #2 707: on retourne la liste des attributs après le choix de champ de fusion avec l'information obligatoire ou non
                return {
                    attributes: attributes
                }
            }

            if (this.addEndSpaceWithMergeFields)
                sHtml += '&nbsp;';
        }
        // 68 132 - Passage par insertData qui prend en charge les différences entre navigateurs, et plusieurs types d'éditeurs (CKEditor, grapesjs, texte...)
        // Backlog #320 : on passe le dernier paramètre bSkipHTMLTemplateEditor à true afin d'effectuer l'insertion directement via CKEditor, comme avant l'implémentation
        // de grapesjs, sans passer par grapesjs, même si l'éditeur utilisé est grapesjs. Ceci, car l'appel de execCommand("insertHTML") via grapesjs semble provoquer
        // l'insertion des champs de fusion en double dans de nombreux cas
        var bInsertRawHTML = true;

        if (!that.enableAdvancedFormular)
            var bInsertRawHTML = CKEDITOR.env.ie;

        var bFocusBeforeInsert = true; // CKEDITOR.env.ie;
        //if (bInsertRawHTML == true)
        if (this.addEndSpaceWithMergeFields)
            sHtml += '&nbsp;';
        if (!that.enableAdvancedFormular)
            this.insertData(sHtml, bInsertRawHTML, true, -1, true);
        else
            this.insertData(sHtml, true);

        //libellé de la case a coché se met tjrs a la fin
        if (strFieldLabel != '' && oMergeFieldData.Format == oMergeFieldData.FieldFormat.TYP_BIT)
            this.insertData(strFieldLabel, true);

        if (!that.enableAdvancedFormular) {
            mergeFieldsComboBox.setValue("");
            this.selectedMergeField = "";


            if (bInsertSubmitMergeField) {
                var mergeFieldsComboBox = this.htmlEditor.ui.get('xrmMergeFields');
                if (mergeFieldsComboBox) {
                    // Récupération de l'élément Bouton Valider depuis la combobox des champs de fusion
                    var submitMergeFieldValue = null;
                    for (var elementKey in mergeFieldsComboBox._.items) {
                        if (elementKey.indexOf('submit;button') != -1) {
                            submitMergeFieldValue = elementKey;
                        }
                    }
                    if (submitMergeFieldValue != null) {
                        this.insertData('<br />', CKEDITOR.env.ie);
                        this.insertData('<br />', CKEDITOR.env.ie);
                        this.insertMergeField(mergeFieldsComboBox, submitMergeFieldValue, 'R');
                    }
                }
            }
        }
    };


    this.getSrcElement = function () {
        var nodeSrcElement;
        if (document.getElementById(this.name))
            nodeSrcElement = document.getElementById(this.name).parentNode;
        else if (document.getElementById('cke_' + this.name))
            nodeSrcElement = document.getElementById('cke_' + this.name).parentNode;
        else if (document.getElementById('eMEG_' + this.name))
            nodeSrcElement = document.getElementById('eMEG_' + this.name).parentNode;
        else if (document.getElementById(this.templateEditorRootContainer))
            nodeSrcElement = document.getElementById(this.templateEditorRootContainer).parentNode;
        return nodeSrcElement;
    }

    // Backlog #325 - Fonction d'affichage/désactivation des images de grapesjs, tenant compte du composant eudonet-extended-image
    this.toggleImages = function (components, on) {
        if (!components || !components.models)
            return;

        var srcPlh = '##'; //'themes/default/images/ui/picture_grayed.png'; // image de remplacement lors de la désactivation des images - il ne faut rien mettre si on veut libérer l'espace occupé par une image en taille fixe

        for (var i = 0; i < components.models.length; i++) {
            var component = components.models[i];
            if (component.get('type') === 'image') {
                var source = component.get('src');

                if (on) {
                    if (source === srcPlh) {
                        component.set('src', component.get('src_bkp'));
                    }
                } else if (source !== srcPlh) {
                    component.set('src_bkp', component.get('src'));
                    component.set('src', srcPlh);
                }
            }
            // Dans le cas de nos composants Image avec conteneur, le composant renvoyé est le conteneur, et son contenu (la balise <img>) est injecté en tant que code HTML non manipulable
            // via component.get. Il faut donc répliquer le même comportement que ci-dessus en utilisant un DOMParser
            else if (component.get('type') === 'eudonet-extended-image') {
                var oExtendedImageDoc = eTools.stringToHTMLDocument(component.get('content'));

                // il n'y a qu'une seule image par composant eudonet-extended-image, mais on boucle par précaution au cas où le composant change
                var sources = oExtendedImageDoc.querySelectorAll("img");

                for (var j = 0; j < sources.length; j++) {
                    // Attention, il faut ici utiliser getAttribute/setAttribute et non les accesseurs directs de propriété (sources[i].src ou sources[i].bkp_src) car ces accesseurs
                    // modifient l'attribut src en URL complète (le ## se transformerait en http://serveur/eMain.aspx##) et ne gèrent pas les attributs personnalisés (bkp_src ici).
                    var source = sources[j].getAttribute("src");

                    if (on) {
                        if (source === srcPlh) {
                            sources[j].setAttribute("src", sources[j].getAttribute("src_bkp"));
                        }
                    } else if (source !== srcPlh) {
                        sources[j].setAttribute("src_bkp", sources[j].getAttribute("src"));
                        sources[j].setAttribute("src", srcPlh);
                    }
                }

                component.set('content', oExtendedImageDoc.body.innerHTML);
            }

            // Exécution sur les composants enfants
            that.toggleImages(component.get('components'), on);
        }
    };
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    // METHODES DE GENERATION DES EDITEURS (CKEDITOR/GRAPESJS)
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------

    // grapesjs
    // --------

    // Backlog #653 - Indique si le navigateur utilisé supporte grapesjs
    this.browserSupportsHTMLTemplateEditor = function () {
        var browser = new getBrowser();
        return !browser.isIE && !browser.isEdge;
    }

    // Indique si grapesjs peut être utilisé
    this.canUseHTMLTemplateEditor = function (bDisplayWarning, strTextAreaId) {
        // Si l'utilisation de l'éditeur avancé n'a pas été demandée, inutile de tester quoi que ce soit ou d'alerter
        if (!this.enableTemplateEditor)
            return false;

        // Si son ID est précisé, on masque le conteneur réservé à grapesjs en cas de refus d'instanciation
        if (!strTextAreaId)
            strTextAreaId = '';

        var bUnsupportedBrowser = !this.browserSupportsHTMLTemplateEditor();
        var bCantInitialize = typeof (grapesjs) == "undefined";
        if (bCantInitialize || bUnsupportedBrowser) {
            // On désactive
            this.enableTemplateEditor = false;
            // On avertit
            if (bCantInitialize)
                this.trace("Les composants JavaScript nécessaires à l'instanciation de l'éditeur de templates HTML avancé n'ont pas été chargés. Sa création sera ignorée.");
            if (bUnsupportedBrowser) {
                this.trace(top._res_2198 + " - " + navigator.userAgent);
                //if (bDisplayWarning)
                //  eAlert(0, top._res_6455, top._res_2198, ""); // Navigateur - Pour bénéficier du nouvel éditeur d’e-mail, veuillez utiliser l’un des navigateurs suivants : Chrome, Firefox, Edge ou Safari.
            }
            // On masque le conteneur censé accueillir grapesjs au départ
            if (document.getElementById("templateEditor_" + strTextAreaId))
                document.getElementById("templateEditor_" + strTextAreaId).style.display = 'none';
        }
        return this.enableTemplateEditor;
    }

    this.createHTMLTemplateEditor = function (strTextAreaId) {

        // Backlog #408, #498 - Si le contexte JS/client (ex : IE, Edge) ne permet pas d'utiliser grapesjs malgré l'information envoyée par les renderers, on instancie CKEditor à la place
        if (!this.canUseHTMLTemplateEditor(false, strTextAreaId)) {
            this.createHTMLEditor(strTextAreaId, this.inlineMode);
            return;
        }

        // Demande #71 789 - Paramétrage de la propriété timestamp de CKEditor (pour l'instance créée ensuite par grapesjs), afin de lui affecter le numéro de version actuel de l'application
        // Cette variable est ajoutée en queryString des plugin.js par CKEditor (?t=<TIMESTAMP>) afin de court-circuiter le cache navigateur lorsque le plugin est mis à jour
        // au même titre que notre variable ver= ajoutée sur tous les JS via addScript()
        if (this.updateHTMLEditorTimestamp)
            this.updateHTMLEditorTimestamp(top._jsVer);

        // On indique que la création va être effectuée, et qu'il ne faut surtout pas rappeler de nouveau cette fonction via un timer
        this.templateEditorCreationInProgress = true;

        // Référence vers cette classe eMemoEditor pour les fonctions internes des initialisateurs de composants ci-dessous
        // pour lesquelles "this" a une autre signification (portée locale)
        var eMemoEditorObject = this;

        // Si les ressources de langue n'ont pas déjà été chargées par un précédent appel via CKEditor, on les charge
        if (!grapesjs.xrmLang)
            this.getGrapesJSLanguageResources();

        // De même, si les plugins spécifiques n'ont pas été chargés, on fait de même
        this.loadExtraPlugins();

        // On définit la taille du composant en tenant compte de l'espace à réserver aux barres d'outils additionnelles
        // On ajuste donc la taille passée en paramètre
        var templateEditorWidth = eMemoEditorObject.config.width;
        var templateEditorHeight = eMemoEditorObject.config.height;
        /*
        var templateEditorSideToolbarWidth = 230;
        if (templateEditorWidth.indexOf('%') == -1) {
            templateEditorWidth = (getNumber(templateEditorWidth) - templateEditorSideToolbarWidth) + 'px';
        }
        else {
            templateEditorWidth = 'calc(' + templateEditorWidth + ' - ' + templateEditorSideToolbarWidth + 'px)';
        }
        */

        /*
        var imageAssets = [
            'themes/default/images/slogan-xrm.png'
        ];
        */
        var imageAssets = [];

        // Backlog #450 - Si this.value n'a pas été renseignée avec le contenu à injecter dans l'éditeur, on la renseigne maintenant avec le contenu de l'élément HTML
        // rattaché à l'éditeur, afin de simuler, à la toute fin de l'initialisation du composant (cf. fonction injectHTMLTemplateEditorData())
        // l'équivalent de fromElement: true dans le grapesjs.init(), ci-dessous, qui initialise l'éditeur avec le contenu (innerHTML) de l'élément HTML qui lui est
        // rattaché (propriété container)
        // L'injection des données initiales est effectuée à la toute fin du chargement du composant pour s'assurer que la reconnaissance du code HTML injecté se fasse
        // avec toutes les personnalisations effectuées ici concernant l'interprétation du code HTML par l'éditeur (fonctions customizeHTMLTemplateEditor*())
        this.templateEditorRootContainer = 'templateEditor_' + strTextAreaId;
        if (!this.value || this.value.trim() == '')
            this.value = document.getElementById(this.templateEditorRootContainer).innerHTML;
        this.isMemoInstance = this instanceof eMemoEditor;//si c'est une instance de eMemoEditor ou eGrapesJSEditor, on masque certaines fonctionnalités
        this.trace("Initialisation de l'éditeur avancé...");
        var templateEditor = grapesjs.init({
            // Initialise le composant dans ce conteneur. On peut aussi passer un HTMLElement
            container: "#" + this.templateEditorRootContainer,
            // Taille de l'éditeur
            height: templateEditorHeight,
            width: templateEditorWidth,
            // Position du color picker - Backlog #580 + https://github.com/artf/grapesjs/issues/596
            colorPicker: { appendTo: 'parent', offset: { top: 20, left: -175 } },
            /*
                Backlog #89 et #499 - Théoriquement, il faudrait désactiver l'autorendu au démarrage, afin de s'assurer que le rendu soit effectué uniquement lorsque toutes
                nos personnalisations auront été effectuées(notamment via customizeHTMLTemplateEditor*() plus bas).On déclenchera ainsi le rendu manuellement.
                Sur les fenêtres où le contenu est envoyé à l'éditeur à l'initialisation(ex : fenêtre des modèles de mail), cela évite le cas où le contenu est interprété
                AVANT que tout notre contexte personnalisé soit prêt.Exemple avec nos conteneurs qui seraient reconnus avec le type "default" sur les images, entraînant
                l'absence de déclenchement de CKEditor au double-clic sur les images, au profit de l'éditeur d'images par défaut (pourtant désactivé par notre extension du
                model "image" dans customizeHTMLTemplateEditorComponents())
                https://grapesjs.com/docs/modules/Components.html#hints
                https://github.com/artf/grapesjs/issues/171
                Cependant, il semblerait qu'effectuer un render() après coup à la place de l'autorender provoque le chargement incomplet du composant, ainsi que des conflits
                avec les plugins.Par mesure de précaution, on préférera donc réinjecter le contenu(passé à components ci - dessus) lors de l'appel à injectHTMLTemplateEditorData()
                après chargement complet des personnalisations
            */
            // autorender: 0,
            /*
                Backlog #450 :
                En temps normal, on indiquerait de récupérer le contenu directement depuis le canevas / innerHTML de cet élément (fromElement = true)
                ou à partir de eMemoEditor.value(components = '<h1>Mon contenu</h1>')
                - si eMemoEditor.value est définie, on utilise la variable components pour injecter le contenu exact de cette valeur sans s'appuyer sur le parseur
                de grapesjs.
                - à l'inverse, activer fromElement incite grapesjs à créer des classes et des styles correspondant au conteneur nommé ci-dessus, qui peut
                alors contenir des informations de style non désirées (ex: une couleur de fond), entraînant une mise en forme de base sur l'éditeur qu'il est très difficile
                voire impossible de remettre à zéro (cf.setData())
                Même chose pour les styles, transmission de eMemoEditor.getCss() à grapesjs.init.style
                Ce traitement est, dans notre cas, effectué via injectHTMLTemplateEditorData(), qui injectera le contenu après chargement complet de toutes les personnalisations
            */
            //fromElement: (!this.value || this.value.trim() == ''),
            //components: (this.value && this.value.trim() != '' ? this.value : ''),
            //style: (this.getCss() && this.getCss().trim() != '' ? this.getCss() : ''),
            // Backlog #618 - On positionne forceClass à false afin que grapesjs ne recrée pas une classe sur chaque élément, mais positionne le style en inline
            // Backlog #647 - Pose visiblement un conflit de conversion inline <=> classe, notamment sur les marges. Laissé à true à défaut
            forceClass: true,  // [default: true] : On creation of a new Component (via object), if the 'style' attribute is not empty, all those roles will be moved in its new class
            // Backlog #450, #617 et #618 - On ne positionne pas avoidInlineStyle à true, mais on le laisse à sa valeur par défaut à l'heure actuelle (0.14.52) soit false
            // Le positionner à true force grapesjs à mettre les styles dans les classes CSS, ce qui est théoriquement plus logique, mais moins bien supporté par les
            // clients mail (d'où l'utilisation de gjs-get-inlined-html dans getData(). De plus, cette option pose problème dans le cas de la modification de la couleur de
            // Body dans grapesjs : si elle est à true, la couleur est positionnée sur la classe gjs-dashed de la balise <body> du document situé à l'intérieur du canevas,
            // balise qui n'est pas exportée lors du passage grapesjs <=> CKEditor (cf. option fullPage dans la fonction setFullPage() de ce fichier).
            // On préférera donc garder avoidInlineStyle à false, et pour la couleur de fond du body, l'exporter autrement (notamment en rajoutant manuellement le wrapper
            // lors de l'appel à getData()
            // https://github.com/artf/grapesjs/issues/1246
            avoidInlineStyle: false,
            //avoidInlineStyle: true, // [default: false] : Usually when you update the `style` of the component this changes the element's `style` attribute. Unfortunately, inline styling doesn't allow use of media queries (@media) or even pseudo selectors (eg. :hover). When `avoidInlineStyle` is true all styles are inserted inside the css rule
            //domComponents: { storeWrapper: 1 }, // https://github.com/artf/grapesjs/issues/1357
            /*
                 Backlog #91 - Pour activer l'upload d'images, il suffit, à priori, de simplement déclarer l'AssetManager comme suit, même sans banque d'images prédéfinie
                 cf. code source HTML de la démo "Webpage"
                 https://github.com/artf/grapesjs/wiki/Assets#uploading-assets
            */
            assetManager: {
                embedAsBase64: 1,
                assets: imageAssets,

                //AABBA Tache #2 681 
                // Content to add where there is no assets to show
                // eg. 'No <b>assets</b> here, drag to upload'
                noAssets: '',

                // Upload endpoint, set `false` to disable upload
                // upload: 'https://endpoint/upload/assets',
                // upload: false,
                upload: 0,

                // The name used in POST to pass uploaded files
                uploadName: 'files',

                // Allow uploading multiple files per request.
                multiUpload: true,

                // Text on upload input
                uploadText: grapesjs.xrmLang[this.language].uploadText,

                // Label for the add button
                addBtnText: grapesjs.xrmLang[this.language].addButton,

                //Default placeholder for input
                inputPlaceholder: grapesjs.xrmLang[this.language].inputPath,

            },
            /* Plugins :
             * https://github.com/artf/grapesjs-blocks-basic
             * https://github.com/artf/grapesjs-plugin-ckeditor
            */
            plugins: [this.isMemoInstance ? 'gjs-plugin-ckeditor' : '', 'gjs-blocks-basic', 'gjs-preset-newsletter', 'gjs-preset-webpage'],
            pluginsOpts: {
                'gjs-plugin-ckeditor': this.isMemoInstance ? {
                    position: 'center',
                    /* On reprend certaines options définies sur eMemoEditor, mais pas toutes. Certaines semblent faire planter l'initialisation via grapesjs */
                    options: {
                        customConfig: '',
                        language: this.language,
                        skin: 'eudonet',
                        startupFocus: true,
                        // MAB - Backlog #377
                        // Reprise du correctif #52 816 appliquée sur le constructeur d'eMemoEditor, pour grapesjs. Concernant le fonctionnement de l'ACF
                        // Passer allowedContent à true (comme le fait grapesjs) désactive complètement l'ACF (Advanced Content Filter) de CKEditor 4, et il est donc impossible
                        // de tout autoriser sauf certains tags en utilisant disallowedContent (qui nécessite que l'ACF soit actif).
                        // Pour autoriser l'ajout de contenu normalement filtré tout en excluant d'autres tags, il faut donc conserver l'ACF en définissant une règle autorisant
                        // tout, puis utiliser disallowedContent pour lister les tags à filtrer.
                        // http://docs.ckeditor.com/#!/guide/dev_acf
                        // http://docs.ckeditor.com/#!/guide/dev_disallowed_content
                        // allowedContent: true, // Disable auto-formatting, class removing, etc. - Configuration de base de grapesjs
                        allowedContent: {
                            $1: {
                                // Use the ability to specify elements as an object.
                                elements: CKEDITOR.dtd,
                                attributes: true,
                                styles: true,
                                classes: true
                            }
                        },
                        extraAllowedContent: '*(*);*{*};img[width,height]', // Allows any class and any inline style - MAB : pour image, height/width autorisés uniquement en attributs (Backlog #377) - [] représentent les attributs
                        // https://ckeditor.com/old/forums/Support/How-to-prevent-CKEditor-4.x-from-setting-image-dimensions-in-styles-rather-that
                        // https://ckeditor.com/docs/ckeditor4/latest/guide/dev_allowed_content_rules.html
                        // Backlog #377 : width et height interdits en attribut "style" pour les images - {} représentent les styles
                        // Cette modification recommandée par les développeurs de CKEditor via la directive img{width,height} désactive les options Largeur et Hauteur
                        // sur la boîte de dialogue Image.
                        // Cette problématique sera donc gérée en post-édition, en retransformant tous les img style = "width/height" en img width="" height=""
                        //disallowedContent: 'img{width,height}',
                        disallowedContent: '',
                        enterMode: CKEDITOR.ENTER_BR,
                        extraPlugins: CKEDITOR.config.extraPlugins,
                        removePlugins: "magicline,link,xrmLinkAdapter,xrmLinkUnsubscribe,xrmLinkVisualization,xrmUploadFiles,xrmFormular,xrmFormularReadWrite", // Backlog #326 (magicline) et #573 (liens)
                        toolbar: that.setToolBarForTemplateEditor(), // Backlog #356
                        forcePasteAsPlainText: true // Demande #71 938 - Pas de collage avec mise en forme sur les éléments grapesjs édités avec CKEditor - https://ckeditor.com/old/forums/CKEditor-3.x/Force-paste-Plain-Text
                    }
                } : {},
                'gjs-blocks-basic': {}, /* ces options sont gérées par le plugin webpage, cf. plus bas */
                'gjs-preset-newsletter': {
                    cmdOpenImport: 'gjs-open-import-template',
                    cmdTglImages: 'gjs-toggle-images',
                    cmdInlineHtml: 'gjs-get-inlined-html',
                    codeViewerTheme: 'hopscotch',
                    cmdBtnDesktopLabel: grapesjs.xrmLang[this.language].buttonDevicesDesktop,
                    cmdBtnMobileLabel: grapesjs.xrmLang[this.language].buttonDevicesMobile,
                    cmdBtnTabletLabel: grapesjs.xrmLang[this.language].buttonDevicesTablet,
                    cmtTglImagesLabel: grapesjs.xrmLang[this.language].buttonToggleImages,
                    cmdBtnMoveLabel: grapesjs.xrmLang[this.language].buttonMove,
                    cmdBtnUndoLabel: grapesjs.xrmLang[this.language].buttonUndo,
                    cmdBtnRedoLabel: grapesjs.xrmLang[this.language].buttonRedo,
                    modalTitleImport: grapesjs.xrmLang[this.language].dialogImportTitle,
                    modalTitleExport: grapesjs.xrmLang[this.language].dialogExportTitle,
                    modalLabelImport: grapesjs.xrmLang[this.language].dialogImportLabel,
                    modalLabelExport: grapesjs.xrmLang[this.language].dialogExportLabel,
                    modalBtnImport: grapesjs.xrmLang[this.language].buttonImport,
                    openBlocksBtnTitle: grapesjs.xrmLang[this.language].buttonOpenBlocks,
                    openLayersBtnTitle: grapesjs.xrmLang[this.language].buttonOpenLayers,
                    openSmBtnTitle: grapesjs.xrmLang[this.language].buttonStyleManager,
                    openTmBtnTitle: grapesjs.xrmLang[this.language].buttonTraitManager,
                    expTplBtnTitle: grapesjs.xrmLang[this.language].buttonCode,
                    fullScrBtnTitle: grapesjs.xrmLang[this.language].buttonFullScreen,
                    swichtVwBtnTitle: grapesjs.xrmLang[this.language].buttonToggleBorders,
                    categoryLabel: grapesjs.xrmLang[this.language].blockNativeCategories["basic"],
                    sect100BlkLabel: grapesjs.xrmLang[this.language].blockNativeLabels["sect100"],
                    sect50BlkLabel: grapesjs.xrmLang[this.language].blockNativeLabels["sect50"],
                    sect30BlkLabel: grapesjs.xrmLang[this.language].blockNativeLabels["sect30"],
                    sect37BlkLabel: grapesjs.xrmLang[this.language].blockNativeLabels["sect37"],
                    buttonBlkLabel: grapesjs.xrmLang[this.language].blockNativeLabels["button"],
                    dividerBlkLabel: grapesjs.xrmLang[this.language].blockNativeLabels["divider"],
                    textBlkLabel: grapesjs.xrmLang[this.language].blockNativeLabels["text"],
                    textSectionBlkLabel: grapesjs.xrmLang[this.language].blockNativeLabels["text-section"],
                    imageBlkLabel: grapesjs.xrmLang[this.language].blockNativeLabels["image"],
                    quoteBlkLabel: grapesjs.xrmLang[this.language].blockNativeLabels["quote"],
                    linkBlkLabel: grapesjs.xrmLang[this.language].blockNativeLabels["link"],
                    linkBlockBlkLabel: grapesjs.xrmLang[this.language].blockNativeLabels["link-block"],
                    gridItemsBlkLabel: grapesjs.xrmLang[this.language].blockNativeLabels["grid-items"],
                    listItemsBlkLabel: grapesjs.xrmLang[this.language].blockNativeLabels["list-items"],
                    assetsModalTitle: grapesjs.xrmLang[this.language].dialogAssetsTitle,
                    // Backlog #212 - Précharger le contenu de la fenêtre avec le code HTML actuel du composant
                    // Le plugin Newsletter ne supporte pas (pour le moment) l'utilisation d'une fonction comme valeur de importPlaceholder pour alimenter le contenu de la
                    // fenêtre lors du clic sur le bouton Importer (en faisant un appel à grapesjs.editor.getHtml() ou eMemoEditor.getData()), contrairement au plugin
                    // Webpage. cf. openImport.js et openImportCommand.js sur ces 2 plugins :
                    // - l'un avec "const cnt = typeof importCnt == 'function' ? importCnt(editor) : importCnt" puis "codeViewer.setContent(cnt || '');" (plugin Webpage);
                    // - l'autre sans : "codeViewer.setContent(opt.importPlaceholder || '');" (plugin Newsletter)
                    // On ruse donc avec un getter JavaScript pour que ça soit une fonction qui soit implicitement appelée lorsque le plugin utilisera cette propriété
                    // https://developer.mozilla.org/fr/docs/Web/JavaScript/Reference/Fonctions/get
                    // A l'intérieur de ce getter, this représente le contexte d'exécution de la fonction qui l'appelle. Soit, ici, la fonction run() d'une commande grapesjs qui
                    // dispose, en paramètre, de l'objet editor correspondant à l'instance de grapesjs sur laquelle on travaille. Pour récupérer le contenu du champ, on appelle
                    // donc sa méthode getHtml()
                    //importPlaceholder: '',
                    get importPlaceholder() {
                        // #72 031 - On initialise un timer vérifiant si la modale d'import de code est ouverte, et qui, une fois fermée, se chargera de rétablir le
                        // statut "Non exécutée" de la commande liée. Afin que la protection de grapesjs > 0.14.55 (empêchant l'exécution de commandes en boucle) ne croie pas
                        // que la fenêtre est restée ouverte, et empêche donc son exécution
                        this.editor.monitorImportModalPending = true;
                        this.editor.importModalTimer = window.setTimeout(editor.monitorImportModal, 1000);
                        // Backlog #427 - Le contenu HTML du canevas doit être renvoyé avec des styles inline pour rester compatible avec Outlook
                        // Il faut pour cela utiliser la commande gjs-get-inlined-html de grapesjs, plutôt que sa fonction getHtml()
                        // Source : https://github.com/artf/grapesjs-preset-newsletter/issues/4
                        // Dans ce cas précis, il n'y a pas besoin de renvoyer les styles définis par grapesjs séparément.
                        // Notre CSS, contenant entre autres le nécessaire pour le responsive, sera, quand à elle, réinjectée via injectCSS()
                        //return this.editor.getHtml() + "<style>" + this.editor.getCss() + "</style>";
                        // Demande #71 938/72 070 - Nettoyage des classes CSS c*** de l'éditeur de templates HTML avancé (grapesjs)
                        eTools.removeHiddenSpecialChars(this.cleanEditorCss(this.editor.runCommand("gjs-get-inlined-html")));
                    },
                    defaultTemplate: '', // Default template in case the canvas is empty
                    inlineCss: 1,
                    cellStyle: {
                        padding: 0,
                        margin: 0,
                        'vertical-align': 'top',
                    },
                    tableStyle: {
                        height: '150px',
                        margin: '0 auto 10px auto',
                        padding: '5px 5px 5px 5px',
                        width: '100%'
                    },
                    styleManagerSectors: this.getPlugingPresetNewsletterStyleManager(this.isMemoInstance)
                },
                'gjs-preset-webpage': {
                    // Which blocks to add
                    blocks: ['link-block', 'quote', 'text-basic'], // Which blocks to add
                    modalImportTitle: grapesjs.xrmLang[this.language].dialogImportTitle, // Modal import title
                    modalImportButton: grapesjs.xrmLang[this.language].dialogImportButton, // Modal import button text
                    modalImportLabel: grapesjs.xrmLang[this.language].dialogImportLabel,  // Import description inside import modal
                    // Default content to setup on import model open.
                    // Could also be a function with a dynamic content return (must be a string)
                    modalImportContent: function (editor) {
                        // #72 031 - On initialise un timer vérifiant si la modale d'import de code est ouverte, et qui, une fois fermée, se chargera de rétablir le
                        // statut "Non exécutée" de la commande liée. Afin que la protection de grapesjs > 0.14.55 (empêchant l'exécution de commandes en boucle) ne croie pas
                        // que la fenêtre est restée ouverte, et empêche donc son exécution
                        editor.monitorImportModalPending = true;
                        editor.importModalTimer = window.setTimeout(editor.monitorImportModal, 1000);

                        // Backlog #427 - Le contenu HTML du canevas doit être renvoyé avec des styles inline pour rester compatible avec Outlook
                        // Il faut pour cela utiliser la commande gjs-get-inlined-html de grapesjs, plutôt que sa fonction getHtml()
                        // Source : https://github.com/artf/grapesjs-preset-newsletter/issues/4
                        // Dans ce cas précis, il n'y a pas besoin de renvoyer les styles définis par grapesjs séparément.
                        // Notre CSS, contenant entre autres le nécessaire pour le responsive, sera, quand à elle, réinjectée via injectCSS()
                        //return editor.getHtml() + "<style>" + editor.getCss() + "</style>";
                        // Demande #71 938/72 070 - Nettoyage des classes CSS c*** de l'éditeur de templates HTML avancé (grapesjs)
                        return eTools.removeHiddenSpecialChars(that.cleanEditorCss(editor.runCommand("gjs-get-inlined-html")));
                    },
                    importViewerOptions: {}, // Code viewer (eg. CodeMirror) options
                    textCleanCanvas: grapesjs.xrmLang[this.language].dialogEmptyCanvasConfirm, // // Confirm text before cleaning the canvas
                    showStylesOnChange: 1,                     // Show the Style Manager on component change
                    textGeneral: grapesjs.xrmLang[this.language].styleManagerSectorsGeneral,                     // Text for General sector in Style Manager
                    textLayout: grapesjs.xrmLang[this.language].styleManagerSectorsLayout,                     // Text for Layout sector in Style Manager
                    textTypography: grapesjs.xrmLang[this.language].styleManagerSectorsTypography,   // Text for Typography sector in Style Manager
                    textDecorations: grapesjs.xrmLang[this.language].styleManagerSectorsDecorations, // Text for Decorations sector in Style Manager
                    textExtra: 'Extra',                     // Text for Extra sector in Style Manager
                    customStyleManager: [],                     // Use custom set of sectors for the Style Manager
                    // `grapesjs-blocks-basic` plugin options
                    // By setting this option to `false` will avoid loading the plugin
                    blocksBasicOpts: {
                        blocks: [
                            'column1',
                            'column2',
                            //'column3-7',
                            //'column3',
                            //'text',
                            //'link',
                            //'image',
                            //'video',
                            //'map'
                        ],
                        category: grapesjs.xrmLang[this.language].blockNativeCategories["additional"],
                        flexGrid: false,
                        stylePrefix: 'gjs-',
                        addBasicStyle: true,
                        labelColumn1: grapesjs.xrmLang[this.language].blockNativeLabels["sect100"],
                        labelColumn2: grapesjs.xrmLang[this.language].blockNativeLabels["sect50"],
                        labelColumn3: grapesjs.xrmLang[this.language].blockNativeLabels["sect30"],
                        labelColumn37: grapesjs.xrmLang[this.language].blockNativeLabels["sect37"],
                        labelText: grapesjs.xrmLang[this.language].blockNativeLabels["text"],
                        labelLink: grapesjs.xrmLang[this.language].blockNativeLabels["link"],
                        labelImage: grapesjs.xrmLang[this.language].blockNativeLabels["image"],
                        labelVideo: grapesjs.xrmLang[this.language].blockNativeLabels["video"],
                        labelMap: grapesjs.xrmLang[this.language].blockNativeLabels["map"],
                    },
                    // `grapesjs-navbar` plugin options
                    // By setting this option to `false` will avoid loading the plugin
                    navbarOpts: false,
                    // `grapesjs-component-countdown` plugin options
                    // By setting this option to `false` will avoid loading the plugin
                    countdownOpts: false,
                    // `grapesjs-plugin-forms` plugin options
                    // By setting this option to `false` will avoid loading the plugin
                    formsOpts: {},
                    // `grapesjs-plugin-export` plugin options
                    // By setting this option to `false` will avoid loading the plugin
                    exportOpts: {},
                    // `grapesjs-aviary` plugin options, disabled by default
                    // Aviary library should be included manually
                    // By setting this option to `false` will avoid loading the plugin
                    aviaryOpts: 0,
                    // `grapesjs-plugin-filestack` plugin options, disabled by default
                    // Filestack library should be included manually
                    // By setting this option to `false` will avoid loading the plugin
                    filestackOpts: 0,
                }
            },
            storageManager: this.getHTMLTemplateEditorStorageManager(),
            selectorManager: this.getHTMLTemplateEditorSelectorManager(),
            styleManager: this.getHTMLTemplateEditorStyleManager(),
            traitManager: this.getHTMLTemplateEditorTraitManager(),
            //deviceManager: this.getHTMLTemplateEditorDeviceManager(),
            blockManager: this.getHTMLTemplateEditorBlockManager(),
            commands: this.getHTMLTemplateEditorCommands(),
            panels: this.getHTMLTemplateEditorPanels(), // Avoid any default panel, eg. panels: { defaults: [] },
            // MAB : attention, si les éléments référencées par les managers dans leur propriété appendTo n'existent pas à l'initialisation,
            // l'instanciation de grapesjs plantera complètement avec une
            // Uncaught TypeError: Cannot read property 'appendChild' of null at Object.postRender (grapes.js:41971) at grapes.js:36941
            i18n: this.getHTMLTemplateEditorMessage()
        });
        that.trace("Initialisation de base de l'éditeur avancé terminée.");

        this.htmlTemplateEditor = templateEditor;

        //On ne garde que les champs de fusion dans le cas des composants eudofront
        if (that.enableAdvancedFormular)
            that.addMergeFieldsToHtmlTemplateEditor();

        // #72 031 - Surveille si la fenêtre d'import de code est en cours d'exécution, et à sa fermeture, déclenche des traitements spécifiques
        // Ces exécutions ne déclenchant pas d'évènements interceptables
        this.htmlTemplateEditor.monitorImportModal = function () {
            if (that.htmlTemplateEditor.Modal && that.htmlTemplateEditor.Modal.getTitle() == grapesjs.xrmLang[that.language].dialogImportTitle) {
                // Si la fenêtre est toujours ouverte, on redéclenchera la vérification dans X secondes
                if (that.htmlTemplateEditor.Modal.isOpen()) {
                    that.htmlTemplateEditor.monitorImportModalPending = false;

                    that.htmlTemplateEditor.importModalTimer = window.setTimeout(that.htmlTemplateEditor.monitorImportModal, 1000);
                    that.trace("La fenêtre modale " + that.htmlTemplateEditor.Modal.getTitle() + " est toujours ouverte.");
                }
                // Sinon
                else {
                    // On déclenche les traitements post-fermeture
                    if (!that.htmlTemplateEditor.monitorImportModalPending) {
                        // #72 031 : il faut forcer la commande Code en statut inactif à la fermeture des modales, car pour une raison étrange, elle est considérée
                        // comme active par défaut, et le mécanisme de vérification d'exécution des commandes introduit en 0.14.55 empêche le déclenchement d'une commande
                        // considérée comme déjà activée. Et donc, de recliquer sur le bouton Code après avoir été cliqué une première fois
                        // https://github.com/artf/grapesjs/issues/1881
                        that.trace("La fenêtre modale " + that.htmlTemplateEditor.Modal.getTitle() + " a été fermée après avoir été ouverte et surveillée. La commande associée va être inactivée.");
                        var importButton = that.htmlTemplateEditor.Panels.getButton("options", "gjs-open-import-webpage");
                        if (importButton) {
                            that.htmlTemplateEditor.stopCommand("gjs-open-import-webpage");
                            importButton.set("active", false);
                            // Demande #71 938/72 070 - Nettoyage des classes CSS c*** de l'éditeur de templates HTML avancé (grapesjs)
                            setWait(true);
                            that.setData(that.cleanEditorCss(that.getData()));
                            setWait(false);
                        }
                        window.clearTimeout(that.htmlTemplateEditor.importModalTimer);
                    }
                    else {
                        that.htmlTemplateEditor.importModalTimer = window.setTimeout(that.htmlTemplateEditor.monitorImportModal, 1000);
                        that.trace("La fenêtre modale " + that.htmlTemplateEditor.Modal.getTitle() + " n'est pas encore ouverte, mais le déclenchement de sa surveillance est toujours d'actualité.");
                    }
                }
            }
        };

        // Demande #71 785 / Ticket grapesjs 1915
        // Passage de l'option "keepEmptyTextNodes" à true sur le Parser grapesjs pour ne pas supprimer les &nbsp; entre 2 champs de fusion
        // When false, removes empty text nodes when parsed, unless they contain a space
        // https://github.com/artf/grapesjs/issues/1915
        this.htmlTemplateEditor.Parser.getConfig().keepEmptyTextNodes = true;

        // Backlog #295 - On indique à l'objet interne que le CKEditor instancié par grapesjs est en mode Inlne
        this.inlineMode = true;

        // Tout comme pour CKEditor, on rattache le contexte d'eMemoEditor à l'objet global de grapesjs pour pouvoir y accéder depuis les plugins instanciés dans
        // les champs CKEditor enfants
        grapesjs.xrmMemoEditor = this;

        // US #918 - Demande #72 814 - Interception globale des erreurs de mise à jour du contenu à cause de code HTML invalide
        // Utilisation d'une closure pour encapsuler la fonction originale d'un try/catch pour gérer les erreurs, sans modifier le code de grapesjs
        // https://stackoverflow.com/a/10427757
        // https://github.com/artf/grapesjs/issues/2029
        this.htmlTemplateEditor.setComponents = (function (originalFct) {
            return function (components) {
                try {
                    originalFct(components);
                }
                catch (ex) {
                    // Le message d'erreur n'est affiché que si l'éditeur est visible aux yeux de l'utilisateur, et uniquement s'il n'a pas déjà été affiché
                    if (!that.badHTMLCodeAlertDisplayed && that.isDisplayed()) {
                        that.badHTMLCodeAlertDisplayed = true; // On empêche l'affichage du message 2 fois de suite (par ex. à la validation de la fenêtre d'import de code, qui provoque setComponents() puis setData() appelant ensuite setComponents())
                        eAlert(0, top._res_8021 + ' - ' + top._res_2091, top._res_2390, ex, null, null, function () { that.badHTMLCodeAlertDisplayed = false; }); // Editeur HTML - Code HTML/CSS - Une erreur s'est produite lors du chargement de votre modèle
                    }
                    that.trace(top._res_2390 + "\n\n" + ex);
                }
            };
        })(this.htmlTemplateEditor.setComponents);

        // this.htmlEditor (correspondant à l'instance CKEditor reliée à eMemoEditor) sera renseignée par l'appel à customizeHTMLTemplateEditorEvents() ci-dessous
        // Cela permettra d'accéder à toutes les fonctionnalités de CKEditor via eMemoEditor.htmlEditor, même si celui-ci est instancié via grapesjs

        /* APRES INITIALISATION */

        // On ajoute des éléments personnalisés aux managers initialisés via le init() ci-dessous avec les méthodes this.getHTMLTemplateEditor*()
        // Ces initialisations pouvant être surchargées par les plugins ajoutés, il faut faire des appels après-coup si on souhaite ajouter des éléments à ceux déjà
        // alimentés par les plugins.
        // Mais, les données ajoutées par les plugins ne sont pas encore disponibles à ce stade (init()), elles le seront une fois le composant complètement chargé.
        // On fait donc uniquement un appel à customizeHTMLTemplateEditorEvents() ici, qui, elle, définira une fonction après chargement (.on("load")) qui se chargera
        // d'appeler toutes les autres méthodes customizeHTMLTemplateEditor*() une fois que tout le contexte sera disponible
        this.customizeHTMLTemplateEditorEvents(); // Evènements

        // On ajoute un ID au conteneur principal afin de le rendre identifiable via getFldEngFromElt(), tout comme CKEditor
        var templateEditorMainContainer = document.querySelector("#templateEditorRow_" + this.name);
        if (templateEditorMainContainer) {
            templateEditorMainContainer.id = "eMEG_" + this.name;
        }

        /*
        // On met un overflow sur la barre d'outils latérale et on la fixe à la même hauteur que l'éditeur,
        // pour ne pas qu'elle agrandisse le conteneur lors de l'ajout d'options ou de blocs
        var templateEditorSidePanel = document.querySelector("#templateEditorPanelRight_" + this.name);
        if (templateEditorSidePanel) {
            templateEditorSidePanel.style.height = templateEditorHeight;
        }
        */

        // Et enfin, une fois que TOUTES les personnalisations ont été initialisées (via customizeHTMLTemplateEditorEvents()), on injecte le contenu initial de l'éditeur
        // Ceci, afin de s'assurer que tout le contenu soit reconnu avec les personnalisations nécessaires AVANT rendu (ex : conteneurs d'images via customizeHTMLTemplateEditorComponents())
        // Etrangement, déclencher l'injection dans l'évènement on("load") (ce qui semblerait logique pour le faire une fois que tout est prêt) n'a pas toujours d'effet
        // au bon moment.
        // Par mesure de précaution, on déclenchera donc l'injection une fois qu'on aura valorisé une variable à la fin de toute la chaîne déclenchée par on("load"),
        // cf. customizeHTMLTemplateEditorEvents()
        // #71 938 : ATTENTION, il faut faire ici strictement la même chose que dans eMailingTpl.ManageFeedback, sinon, des différences de rendu auront lieu entre l'édition du modèle et son utilisation !
        // https://grapesjs.com/docs/modules/Components.html#hints
        // https://github.com/artf/grapesjs/issues/171
        setWait(true, "eMemoEditor_injectHTMLTemplateEditorData");
        var injectHTMLTemplateEditorData = function () {
            if (that.templateEditorCustomizationsLoaded) {
                that.trace("Injection du contenu initial de l'éditeur avancé...");
                // Suppression du timer
                window.clearTimeout(that.templateEditorInitialDataTimer);
                // Injection du contenu initial de l'éditeur, exactement comme le ferait grapesjs.init(), en passant par setComponents()
                // On passe donc par notre fonction setData() qui fait un appel à setComponents(), en prenant soin au préalable de faire d'autres traitements tels que la
                // suppression de caractères indésirables
                that.setData(that.value, null, true);
                that.injectCSS(that.getCss()); // #71 938
                // Backlog #652 - Injection de la couleur de fond
                that.setColor(that.getColorFromCSS(that.getCss()));
                // La création est terminée, on remet le booléen à false
                that.templateEditorCreationInProgress = false;
                that.trace("Le contenu initial de l'éditeur avancé a été injecté");
                setWait(false, "eMemoEditor_injectHTMLTemplateEditorData");

                //tâche #2 459, KJE: dans le cas d'un formulaire avancé, il n'y a que le composant canvas qui peut être déposé dans le body
                if (that.enableAdvancedFormular) {
                    var wrapper = that.htmlTemplateEditor.getWrapper();
                    wrapper.attributes.droppable = '.Canvas';
                    //Tâche #2 462: on masque le bouton d'import/export du code HTML/CSS pour le formulaire avancé.
                    var panelManager = that.htmlTemplateEditor.Panels;
                    panelManager.removeButton('options', 'gjs-open-import-webpage');

                }
            }
            else {
                that.trace("La personnalisation de l'éditeur avancé n'est pas encore terminée, le contenu initial sera injecté en différé");
                that.templateEditorInitialDataTimer = window.setTimeout(injectHTMLTemplateEditorData, 100);
            }
        };
        // On prévoit une fonction qui désactive le setWait au bout de X secondes au cas où un problème surviendrait sur le code de injectHTMLTemplateEditorData() ci-dessus
        var cancelInjectHTMLTemplateEditorDataWaiter = function () {
            window.clearTimeout(that.templateEditorInitialDataWaiterTimer);
            var oWaiter = top.document.getElementById("waiter");
            if (oWaiter) {
                var waiterCaller = oWaiter.attributes["caller"]; // Le caller est en fait placé en propriété de... attributes par addOrDelAttributeValue sous la forme attributes["cle"]
                if (waiterCaller.indexOf("eMemoEditor_injectHTMLTemplateEditorData") > -1) {
                    setWait(false, "eMemoEditor_injectHTMLTemplateEditorData");
                }
            }
        };
        that.templateEditorInitialDataTimer = window.setTimeout(injectHTMLTemplateEditorData, 100);
        that.templateEditorInitialDataWaiterTimer = window.setTimeout(cancelInjectHTMLTemplateEditorDataWaiter, 10000);
    };

    this.showHTMLTemplateEditor = function () {
        let strTextAreaId = null;
        if (strTextAreaId == null || typeof (strTextAreaId) == "undefined")
            strTextAreaId = this.name;
        this.createHTMLTemplateEditor(strTextAreaId);


    }
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    // METHODES SPECIFIQUES A GRAPESJS
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    // Méthodes utilitaires
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------

    // Récupère l'instance de CKEditor associée à l'instance de grapesjs.RichTextEditor passée en paramètre
    // Permet ensuite d'interagir avec le CKEditor lié via l'API CKEditor, par exemple pour en récupérer l'élément actuellement sélectionné
    /// https://grapesjs.com/docs/api/rich_text_editor.html#gettoolbarel
    this.getHTMLTemplateEditorLinkedRTEInstances = function (richTextEditor) {
        var associatedInstance = null;
        var otherInstances = new Array();
        if (richTextEditor) {
            // On récupère l'élément HTML correspondant à la barre d'outils du CKEditor instancié via l'API de grapesjs
            var linkedInstanceToolbarsContainer = richTextEditor.getToolbarEl();
            if (linkedInstanceToolbarsContainer) {
                var associatedInstanceToolbar = null;
                var otherInstanceToolbars = new Array();
                // Puis on récupère l'enfant portant la classe CSS cke (dont l'ID est cke_<nomInstanceCKEditor>),
                // et on se sert de cet ID pour récupérer l'objet dans le tableau global CKEDITOR.instances
                // Backlog #428 : il faut cibler uniquement l'enfant dont la visibilité n'est pas à none, car une nouvelle instance de CKEditor est crée pour chaque bloc éditable cliqué.
                // this.htmlTemplateEditor.RichTextEditor.getToolbarEl() renvoie alors bien un seul conteneur (dont la classe CSS est gjs-rte-toolbar), mais pouvant potentiellement
                // contenir plusieurs div de barres d'outils CKEditor, correspondant à chaque instance créée (classe CSS cke_*). Il faut donc parcourir toutes les instances et
                // sélectionner celle qui n'est PAS en display:none.
                // Le support du ciblage de display: none via querySelector* pouvant varier selon les navigateurs, on préfèrera un ciblage JS via element.style.display
                var linkedInstanceToolbarsContainerChildren = linkedInstanceToolbarsContainer.querySelectorAll(".cke");
                if (linkedInstanceToolbarsContainerChildren.length > 1) {
                    for (var i = 0; i < linkedInstanceToolbarsContainerChildren.length; i++)
                        // L'instance qui n'est pas en display: none est celle affichée par grapesjs
                        if (linkedInstanceToolbarsContainerChildren[i].style.display != "none")
                            associatedInstanceToolbar = linkedInstanceToolbarsContainerChildren[i];
                        // On référence également les instances créées par grapesjs, mais non utilisées à cet instant précis
                        else
                            otherInstanceToolbars.push(linkedInstanceToolbarsContainerChildren[i]);
                }
                else if (linkedInstanceToolbarsContainerChildren.length == 1)
                    associatedInstanceToolbar = linkedInstanceToolbarsContainerChildren[0];

                if (associatedInstanceToolbar) {
                    var associatedInstanceToolbarId = associatedInstanceToolbar.id;
                    if (associatedInstanceToolbarId)
                        associatedInstanceToolbarId = associatedInstanceToolbarId.substring(4); // retrait du préfixe "cke_" pour obtenir l'ID de l'instance CKEditor
                    associatedInstance = CKEDITOR.instances[associatedInstanceToolbarId];
                }
                for (var i = 0; i < otherInstanceToolbars.length; i++) {
                    var otherInstanceToolbarId = otherInstanceToolbars[i].id;
                    if (otherInstanceToolbarId)
                        otherInstanceToolbarId = otherInstanceToolbarId.substring(4); // retrait du préfixe "cke_" pour obtenir l'ID de l'instance CKEditor
                    otherInstances.push(CKEDITOR.instances[otherInstanceToolbarId]);
                }
            }
        }
        return { associatedInstance: associatedInstance, otherInstances: otherInstances };
    };

    // Renvoie l'instance de l'éditeur HTML associée à cet eMemoEditor.
    // Soit this.htmlEditor si la variable est définie, soit en effectuant une recherche dans les éléments JS dans le cas de grapesjs
    // Dans les fonctions utilisées à la fois par un CKEditor indépendant, et un CKEditor encapsulé via grapesjs, on doit remplacer this.htmlEditor par un appel à cette fonction
    this.getHTMLEditor = function (initHTMLEditorVar, model) {
        var htmlEditor = null;

        // Si non définie en paramètre, mais déjà définie sur eMemoEditor, on renvoie l'instance CKEditor telle quelle
        // Backlog #428 : l'instance de CKEditor associée à grapesjs peut changer à chaque édition de composant (une instance par composant édité), il ne faut donc pas se baser là-dessus
        //if (this.htmlEditor && this.htmlEditor.id && this.htmlEditor.id.indexOf("cke_") == 0)
        //    htmlEditor = this.htmlEditor;
        // Si non définie mais que grapesjs l'est, on renvoie l'instance après recherche via l'API de grapesjs
        //else 
        if (this.htmlTemplateEditor)
            htmlEditor = this.getHTMLTemplateEditorLinkedRTEInstances(this.htmlTemplateEditor.RichTextEditor).associatedInstance;

        // Si le ciblage via getHTMLTemplateEditorLinkedRTEInstances() n'a pas fonctionné (ce qui est souvent le cas au premier affichage), on renvoie une fonction qui fera
        // appel de façon récursive à cette même fonction, afin que le prochain appel à this.htmlEditor renvoie bien l'objet CKEditor, et non null ou autre chose
        if (!htmlEditor) {
            var oGetHTMLEditorFct = (
                function (eMemoEditorTargetObject, initHTMLEditorVar, model) {
                    return function () {
                        htmlEditor = eMemoEditorTargetObject.getHTMLEditor(initHTMLEditorVar, model);
                        // Quand on appelle la fonction via un setTimeout(), la valeur est renvoyée hors contexte. Il faut donc l'affecter explicitement
                        if (initHTMLEditorVar) {
                            eMemoEditorTargetObject.htmlEditor = htmlEditor;
                            eMemoEditorTargetObject.trace("La propriété htmlEditor a été initialisée avec l'instance de CKEditor ciblée, en différé");
                        }
                    }
                }
            )(this, initHTMLEditorVar, model);
            this.trace("L'instance de CKEditor n'a pas pu être ciblée après initialisation via grapesjs. Une nouvelle tentative de ciblage sera effectuée dans 100 ms.");
            this.instanceTimer = window.setTimeout(oGetHTMLEditorFct, 100);
        }
        else {
            if (this.instanceTimer) {
                this.trace("Instance de CKEditor ciblée avec succès, avec utilisation de timer");
                window.clearTimeout(this.instanceTimer);
            }
            else {
                this.trace("Instance de CKEditor ciblée avec succès, sans utilisation de timer");
            }
        }

        if (initHTMLEditorVar && htmlEditor) {
            this.htmlEditor = htmlEditor;
            this.trace("La propriété htmlEditor a été initialisée avec l'instance de CKEditor ciblée, avant retour de getHTMLEditor()");
        }

        return htmlEditor;
    };

    // Backlog #445 - Désactive le mode Edition sur l'éditeur en cours - Uniquement pour grapesjs
    this.disableEditing = function (currentEditor) {
        var isEditing = false;

        if (!currentEditor)
            currentEditor = this.htmlTemplateEditor;

        if (!currentEditor)
            return true; // On considère qu'aucune édition n'est en cours si aucune information de contexte n'est disponible

        // Cas où currentEditor == objet déjà passé en tant que model par une fonction this.htmlTemplateEditor.on()
        var currentEditorModel = currentEditor;
        // Cas où currentEditor == this.htmlTemplateEditor
        if (typeof (currentEditor.getModel) == "function" && typeof (currentEditor.getModel().isEditing) == "function")
            currentEditorModel = currentEditor.getModel();
        var isEditing = currentEditorModel.isEditing();
        if (isEditing) {
            if (currentEditor.getSelected() && currentEditor.getSelected().getView() &&
                typeof (currentEditor.getSelected().getView().disableEditing) == "function"
            ) {
                currentEditor.getSelected().getView().disableEditing();
                isEditing = currentEditorModel.isEditing();
            }
        }

        if (isEditing) {
            this.trace("La validation du texte en cours d'édition n'a pas pu être effectuée. Il se peut que le contenu renvoyé ne tienne pas compte du texte actuellement édité.");
        }

        return !isEditing; // renvoie true si l'édition n'est plus en cours (= désactivée avec succès)
    }

    // Backlog #261, #295, #446, #447, #453 : liste des éléments sur lequel on câble une fonction d'édition au simple clic
    // Penser également à câbler dans canEnableEditingOnElementType et canToggleEditingOnElementType ci-dessous
    this.getEditableElementTypes = function () {
        return new Array(
            "text",
            "label",
            "eudonet-extended-image",
            "eudonet-extended-button"
        );
    };

    // Backlog #453 : renvoie une liste de types non éditables nativement, qu'il faut rendre éditables sous conditions
    // Penser également à câbler getNonNativeEditableElementTagNames ci-dessous
    // Attention, toutefois, à ne pas le faire sans vérification : inclure "td" dans getNonNativeEditableElementTagNames
    // provoquerait dans ce cas l'éditabilité (C) de toutes les cellules de tableaux, même vides, alors que le but est ici
    // de rendre éditable un élément enfant de type "span" ou autre (défini dans getNonNativeEditableElementTagNames)
    // qui serait, lui, contenu dans un élément non éditable, mais dont le type serait, du coup, celui du parent (ici, "cell" par ex.)
    // Scénario :
    // on pose un élément de type Text (avec sa structure en tableaux)
    // on édite la couleur, la police... avec CKEditor => il insère un élément de type <span> dans ce bloc de type Text
    // on insère un champ de fusion : encore plus complexe (rajoute un label)
    // on passe en mode Code grapesjs et on valide
    // => cela provoque l'analyse des éléments par grapesjs, qui les scinde alors en types différents
    // => quand on clique sur un élément enfant (le label ou le span), l'évènement onclick déclenché sera celui de la cellule
    // parente, donc son type sera rapporté comme "cell". D'où le fait de référencer "cell" ci-dessous comme étant un type non
    // éditable, mais de ne pas référencer "td" sur getNonNativeEditableElementTagNames ci-dessous, car on souhaite rendre éditable
    // le label ou le span enfant, mais pas td.
    this.getNonNativeEditableElementTypes = function () {
        return new Array(
            "default",
            "cell",
            "link"
        );
    };

    // Backlog #453 : de la même manière, renvoie la liste des tags HTML non éditables nativement, devant bénéficier de ce caractère éditable
    // Penser également à câbler getNonNativeEditableElementTypes ci-dessous
    this.getNonNativeEditableElementTagNames = function () {
        return new Array(
            "div", "span", "font", "p", "h1", "h2", "h3", "h4", "h5", "h6", "h7", "strong", "em", "b", "i", "u", "a"
        );
    };

    // Backlog #261 et #295 - Indique si on doit explicitement rendre le composant éditable aux yeux de CKEditor, selon son type
    // Penser également à câbler dans canToggleEditingOnElementType ci-dessous et getEditableElementTypes ci-dessus
    this.canEnableEditingOnElementType = function (elementType) {
        switch (elementType) {
            case "text":
            case "label":
            case "eudonet-extended-button":
            case "eudonet-extended-image":
                return true;
            default:
                return false;
        }
    };

    // Backlog #295 - Indique si on doit explicitement déclencher le mode Edition d'un composant aux yeux de grapesjs, selon son type
    // Penser également à câbler dans canEnableEditingOnElementType et getEditableElementTypes ci-dessus
    // Attention, déclencher l'édition sur certains éléments et certains évènements (ex : au clic) peut provoquer d'autres anomalies (ex : impossible de sélectionner et donc supprimer l'élément)
    // Par contre, cela permet, pour d'autres (ex : les images) de les rendre éditables/modifiables via les mécanismes de grapesjs (ex : afficher les contrôles de redimensionnement)
    this.canToggleEditingOnElementType = function (elementType) {
        switch (elementType) {
            case "text":
            case "label":
            case "eudonet-extended-button":
            case "eudonet-extended-image":
                return true;
            default:
                return false;
        }
    };

    /// Détermine si l'éditeur HTML se situe dans un conteneur parent potentiellement invisible/masqué
    /// Permet de ne pas effectuer certains traitements si l'éditeur est supposé être masqué aux yeux de l'utilisateur
    this.isDisplayed = function () {
        // On considère l'éditeur comme affiché jusqu'à preuve du contraire
        var isDisplayed = true;
        try {
            // Identification de l'élément HTML représentant l'iframe du canevas de l'éditeur
            var memoFrameEl = this.getMemoFrame();
            // Parcours des éléments parents jusqu'à la racine, à la recherche du premier élément parent explicitement masqué
            if (memoFrameEl) {
                var parent = memoFrameEl.parentNode;
                while (parent) {
                    if (parent.style && (parent.style.display === "none" || parent.style.visibility === "hidden")) {
                        isDisplayed = false;
                        parent = null;
                    }
                    else
                        parent = parent.parentNode;
                }
            }
        }
        catch (ex) {
            isDisplayed = true; // on considère que l'éditeur est affiché, faute d'avoir pu le vérifier
        }
        // Renvoi du résultat
        return isDisplayed;
    };

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    // Initialisation de base des gestionnaires
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------

    // Persistance des données
    // Le gestionnaire de stockage de grapesjs n'est pas utilisé, les données étant déjà sauvegardées via eMemoEditor.js.setData() qui appelle nos managers
    this.getHTMLTemplateEditorStorageManager = function () {
        return { type: null };
    };

    // Sélecteurs
    this.getHTMLTemplateEditorSelectorManager = function () {
        //return { appendTo: '#templateEditorSelectorsContainer_' + this.name };
        // Backlog #41, #316 + #343 - Ressources et affichage des libellés
        // https://github.com/artf/grapesjs/commit/df44e2c3beaeedbb11b6f6116edcca65706411ed
        // /src/selector_manager/config/config.js
        return {
            // stylePrefix: 'clm-', // Style prefix
            // appendTo: '',
            // selectors: [], // Default selectors
            label: grapesjs.xrmLang[this.language].selectorManagerClasses, // Label for selectors
            statesLabel: grapesjs.xrmLang[this.language].selectorManagerState, // Label for states
            selectedLabel: grapesjs.xrmLang[this.language].selectorManagerSelected, // Label for selected element
            // States
            states: [
                { name: 'hover', label: grapesjs.xrmLang[this.language].selectorManagerStateHover },
                { name: 'active', label: grapesjs.xrmLang[this.language].selectorManagerStateActive },
                { name: 'nth-of-type(2n)', label: grapesjs.xrmLang[this.language].selectorManagerStateEvenOdd }
            ]
        }
    }

    // Styles - Manager pris en charge par le plugin Newsletter
    this.getHTMLTemplateEditorStyleManager = function () {
        //return { appendTo: '#templateEditorStylesContainer_' + this.name };
        // Backlog #41, #316 - Ressources
        return {
            textNoElement: grapesjs.xrmLang[this.language].styleManagerSelectElement
        };
    };

    // Styles - Manager pris en charge par le plugin Newsletter
    this.getPlugingPresetNewsletterStyleManager = function (isMemoInstance) {
        var styleManagerElements = [
            {
                id: "alignement",
                name: grapesjs.xrmLang[this.language].styleManagerSectorsTypography,
                open: false,
                buildProps: ['text-align'],
                properties: [
                    {
                        name: grapesjs.xrmLang[this.language].styleManagerSectorsTypographyTextAlign,
                        property: 'text-align',
                        type: 'radio',
                        defaults: 'left',
                        list: [
                            { value: 'left', name: grapesjs.xrmLang[this.language].styleManagerSectorsTypographyTextAlignLeft, className: 'fa fa-align-left' },
                            { value: 'center', name: grapesjs.xrmLang[this.language].styleManagerSectorsTypographyTextAlignCenter, className: 'fa fa-align-center' },
                            { value: 'right', name: grapesjs.xrmLang[this.language].styleManagerSectorsTypographyTextAlignRight, className: 'fa fa-align-right' },
                            { value: 'justify', name: grapesjs.xrmLang[this.language].styleManagerSectorsTypographyTextAlignJustify, className: 'fa fa-align-justify' }
                        ],
                    }
                ]
            },
            {
                id: "colorborders",
                name: grapesjs.xrmLang[this.language].styleManagerSectorsDecorations,
                open: false,
                //SHA : backlog #822
                buildProps: ['background', 'background-color', 'border', 'border-radius', 'border-collapse'],
                properties: [
                    {
                        property: 'background',
                        name: grapesjs.xrmLang[this.language].styleManagerSectorsDecorationsBackground,
                        properties: [
                            { name: grapesjs.xrmLang[this.language].styleManagerSectorsDecorationsBackgroundImage, property: 'background-image' },
                            { name: grapesjs.xrmLang[this.language].styleManagerSectorsDecorationsBackgroundRepeat, property: 'background-repeat' },
                            { name: grapesjs.xrmLang[this.language].styleManagerSectorsDecorationsBackgroundPosition, property: 'background-position' },
                            { name: grapesjs.xrmLang[this.language].styleManagerSectorsDecorationsBackgroundAttachment, property: 'background-attachment' },
                            { name: grapesjs.xrmLang[this.language].styleManagerSectorsDecorationsBackgroundSize, property: 'background-size' }
                        ],
                    },
                    {
                        property: 'background-color',
                        name: grapesjs.xrmLang[this.language].styleManagerSectorsDecorationsBackground,
                    },
                    {
                        property: 'border',
                        name: grapesjs.xrmLang[this.language].styleManagerSectorsDecorationsBorder,
                        properties: [
                            { name: grapesjs.xrmLang[this.language].styleManagerSectorsDecorationsBorderWidth, property: 'border-width', defaults: '0' },
                            { name: grapesjs.xrmLang[this.language].styleManagerSectorsDecorationsBorderStyle, property: 'border-style' },
                            { name: grapesjs.xrmLang[this.language].styleManagerSectorsDecorationsBorderColor, property: 'border-color' },
                        ],
                    },
                    {
                        property: 'border-radius',
                        name: grapesjs.xrmLang[this.language].styleManagerSectorsDecorationsBorderRadius,
                        properties: [
                            { name: grapesjs.xrmLang[this.language].styleManagerSectorsDecorationsBorderRadiusTop, property: 'border-top-left-radius' },
                            { name: grapesjs.xrmLang[this.language].styleManagerSectorsDecorationsBorderRadiusRight, property: 'border-top-right-radius' },
                            { name: grapesjs.xrmLang[this.language].styleManagerSectorsDecorationsBorderRadiusBottom, property: 'border-bottom-left-radius' },
                            { name: grapesjs.xrmLang[this.language].styleManagerSectorsDecorationsBorderRadiusLeft, property: 'border-bottom-right-radius' }
                        ],
                    },
                    {
                        property: 'border-collapse',
                        name: grapesjs.xrmLang[this.language].styleManagerSectorsDecorationsBorderCollapse,
                        type: 'radio',
                        defaults: 'separate',
                        list: [
                            { value: 'separate', name: grapesjs.xrmLang[this.language].styleManagerSectorsDecorationsBorderCollapseNo },
                            { value: 'collapse', name: grapesjs.xrmLang[this.language].styleManagerSectorsDecorationsBorderCollapseYes }
                        ],
                    }
                    /* Remarque MAB : les propriétés box-shadow sont désactivées par défaut dans le plugin Newsletter, car trop peu supportées par les navigateurs */
                    // Backlog #38 : on ne conserve pas non plus cette propriété
                    /*
                    {
                      property: 'box-shadow',
                      properties: [
                        { name: grapesjs.xrmLang[this.language].styleManagerSectorsTypographyBoxShadowX, property: 'box-shadow-h'},
                        { name: grapesjs.xrmLang[this.language].styleManagerSectorsTypographyBoxShadowY, property: 'box-shadow-v'},
                        { name: grapesjs.xrmLang[this.language].styleManagerSectorsTypographyBoxShadowBlur, property: 'box-shadow-blur'},
                        { name: grapesjs.xrmLang[this.language].styleManagerSectorsTypographyBoxShadowSpread, property: 'box-shadow-spread'},
                        { name: grapesjs.xrmLang[this.language].styleManagerSectorsTypographyBoxShadowColor, property: 'box-shadow-color'},
                        { name: grapesjs.xrmLang[this.language].styleManagerSectorsTypographyBoxShadowType, property: 'box-shadow-type'}
                      ],
                    },
                    */


                ],
            },
            {
                id: "dimensions",
                name: grapesjs.xrmLang[this.language].styleManagerSectorsDimensions,
                //open: true, /* Backlog #343 - On ouvre par défaut cette section, vu que le bloc Classes est masqué via un display: none dans eMemoEditor.css */
                open: false, /* Demande #72 071 - Finalement, on laisse fermée par défaut */
                buildProps: [
                    'width',
                    'height',
                    'max-width',
                    'min-height',
                    'margin',
                    'padding'
                ],
                properties: [
                    {
                        property: 'width',
                        name: grapesjs.xrmLang[this.language].styleManagerSectorsDimensionsSizeWidth
                    },
                    {
                        property: 'height',
                        name: grapesjs.xrmLang[this.language].styleManagerSectorsDimensionsSizeHeight
                    },
                    {
                        property: 'max-width',
                        name: grapesjs.xrmLang[this.language].styleManagerSectorsDimensionsSizeMaxWidth
                    },
                    {
                        property: 'min-height',
                        name: grapesjs.xrmLang[this.language].styleManagerSectorsDimensionsSizeMinHeight
                    },
                    {
                        property: 'margin',
                        name: grapesjs.xrmLang[this.language].styleManagerSectorsDimensionsMargin,
                        properties: [
                            { name: grapesjs.xrmLang[this.language].styleManagerSectorsDimensionsMarginTop, property: 'margin-top' },
                            { name: grapesjs.xrmLang[this.language].styleManagerSectorsDimensionsMarginLeft, property: 'margin-left' },
                            { name: grapesjs.xrmLang[this.language].styleManagerSectorsDimensionsMarginRight, property: 'margin-right' },
                            { name: grapesjs.xrmLang[this.language].styleManagerSectorsDimensionsMarginBottom, property: 'margin-bottom' }
                        ],
                    },
                    {
                        property: 'padding',
                        name: grapesjs.xrmLang[this.language].styleManagerSectorsDimensionsPadding,
                        properties: [
                            { name: grapesjs.xrmLang[this.language].styleManagerSectorsDimensionsPaddingTop, property: 'padding-top' },
                            { name: grapesjs.xrmLang[this.language].styleManagerSectorsDimensionsPaddingRight, property: 'padding-right' },
                            { name: grapesjs.xrmLang[this.language].styleManagerSectorsDimensionsPaddingBottom, property: 'padding-bottom' },
                            { name: grapesjs.xrmLang[this.language].styleManagerSectorsDimensionsPaddingLeft, property: 'padding-left' }
                        ],
                    }
                ],
            }
        ];
        //dans le cas d'un formulaire avancé,on ajoute le bloc typographie
        if (!isMemoInstance)
            styleManagerElements.push({
                //Tâche #2 678
                id: "typographie",
                name: grapesjs.xrmLang[this.language].styleManagerSectorsTypograph,
                open: true,
                buildProps: [
                    'font-family',
                    'font-size',
                    'font-weight',
                    'letter-spacing',
                    'color',
                    'line-height',
                    'text-decoration',
                    'text-shadow'
                ],
                properties: [
                    {
                        property: 'font-family',
                        type: 'select',
                        name: grapesjs.xrmLang[this.language].styleManagerSectorsTypographyFontFamily,
                        defaults: 'Arial, Helvetica, sans-serif',
                        list: [
                            { value: '"Andale Mono", AndaleMono, monospace', name: 'Andale Mono' },
                            { value: 'Arial, Helvetica, sans-serif', name: 'Arial' },
                            { value: '"Arial Black", Gadget, sans-serif', name: 'Arial Black' },
                            { value: '"Brush Script MT"', name: 'Brush Script MT' },
                            { value: 'Cabin', name: 'Cabin' },
                            { value: '"Comic Sans MS", cursive, sans-serif', name: 'Comic Sans MS' },
                            { value: '"Concert One"', name: 'Concert One' },
                            { value: 'Courier New, Courier, monospace', name: 'Courier New' },
                            { value: 'Georgia, serif', name: 'Georgia' },
                            { value: 'Helvetica, serif', name: 'Helvetica' },
                            { value: 'Impact, Charcoal, sans-serif', name: 'Impact' },
                            { value: 'Lato', name: 'Lato' },
                            { value: 'Lora', name: 'Lora' },
                            { value: '"Lucida Sans Unicode"', name: 'Lucida Sans Unicode' },
                            { value: 'Merriweather', name: 'Merriweather' },
                            { value: '"Merriweather Sans"', name: 'Merriweather Sans' },
                            { value: 'Montserrat, sans-serif', name: 'Montserrat' },
                            { value: '"Nunito Sans"', name: 'Nunito Sans' },
                            { value: '"Open Sans", sans-serif', name: 'Open Sans' },
                            { value: '"Open Sans Condensed"', name: 'Open Sans Condensed' },
                            { value: 'Oswald', name: 'Oswald' },
                            { value: '"Playfair Display"', name: 'Playfair Display' },
                            { value: '"Prompt"', name: 'Prompt' },
                            { value: '"PT Sans"', name: 'PT Sans' },
                            { value: 'Raleway', name: 'Raleway' },
                            { value: 'Roboto', name: 'Roboto' },
                            { value: '"Roboto Condensed"', name: 'Roboto Condensed' },
                            { value: '"Source Sans Pro"', name: 'Source Sans Pro' },
                            { value: '"Space Mono"', name: 'Space Mono' },
                            { value: 'Tahoma, Geneva, sans-serif', name: 'Tahoma' },
                            { value: '"Times New Roman", Times, serif', name: 'Times New Roman' },
                            { value: '"Trebuchet MS"', name: 'Trebuchet MS' },
                            { value: 'Verdana, Geneva, sans-serif', name: 'Verdana' },
                            { value: '"Work Sans", sans-serif', name: 'Work Sans' }
                        ]
                    },
                    {
                        property: 'font-size',
                        defaults: 'medium',
                        name: grapesjs.xrmLang[this.language].styleManagerSectorsTypographyFontSize
                    },
                    {
                        property: 'font-weight',
                        name: grapesjs.xrmLang[this.language].styleManagerSectorsTypographyWeight,
                        defaults: '500',
                        type: 'select',
                        list: [
                            { value: '100', name: 'Thin' },
                            { value: '200', name: 'Extra-Light' },
                            { value: '300', name: 'Light' },
                            { value: '400', name: 'Normal' },
                            { value: '500', name: 'Medium' },
                            { value: '600', name: 'Semi-Bold' },
                            { value: '700', name: 'Bold' },
                            { value: '800', name: 'Extra-Bold' },
                            { value: '900', name: 'Ultra-Bold' }
                        ],
                    },
                    {
                        property: 'letter-spacing',
                        name: grapesjs.xrmLang[this.language].styleManagerSectorsTypographySpacing
                    },
                    {
                        property: 'color',
                        name: grapesjs.xrmLang[this.language].styleManagerSectorsTypographyFontColor
                    },
                    {
                        property: 'line-height',
                        name: grapesjs.xrmLang[this.language].styleManagerSectorsTypographyLineHeight
                    },
                    {
                        property: 'text-decoration',
                        name: grapesjs.xrmLang[this.language].styleManagerSectorsTypographyTextDecoration,
                        type: 'radio',
                        defaults: 'none',
                        list: [
                            { value: 'none', name: '', className: 'fa fa-times' },
                            { value: 'underline', name: '', className: 'fa fa-underline' },
                            { value: 'line-through', name: '', className: 'fa fa-strikethrough' },
                        ],
                    },
                    {
                        property: 'text-shadow',
                        name: grapesjs.xrmLang[this.language].styleManagerSectorsTypographyTextShadow,
                        properties: [
                            { name: grapesjs.xrmLang[this.language].styleManagerSectorsTypographyTextShadowX, property: 'text-shadow-h' },
                            { name: grapesjs.xrmLang[this.language].styleManagerSectorsTypographyTextShadowY, property: 'text-shadow-v' },
                            { name: grapesjs.xrmLang[this.language].styleManagerSectorsTypographyTextShadowBlur, property: 'text-shadow-blur' },
                            { name: grapesjs.xrmLang[this.language].styleManagerSectorsTypographyTextShadowColor, property: 'text-shadow-color' }
                        ],
                    }
                ]
            });
        return styleManagerElements;
    };

    // Traits (paramètres et comportements des composants de grapesjs)
    this.getHTMLTemplateEditorTraitManager = function () {
        //return { appendTo: '#templateEditorTraitsContainer_' + this.name };
        // Backlog #41, #316 + #299 - Ressources
        // /src/selector_manager/config/config.js
        return {
            // stylePrefix: 'trt-', // Style prefix
            // appendTo: '',
            labelContainer: grapesjs.xrmLang[this.language].traitManagerContainerLabel,
            labelPlhText: grapesjs.xrmLang[this.language].traitManagerPlaceHolderLabel, // Placeholder label for text input types
            labelPlhHref: grapesjs.xrmLang[this.language].traitManagerPlaceHolderHref, // Placeholder label for href input types
            // Default options for the target input
            optionsTarget: [
                { value: '', name: grapesjs.xrmLang[this.language].traitManagerOptionsTargetThis },
                { value: '_blank', name: grapesjs.xrmLang[this.language].traitManagerOptionsTargetBlank }
            ],
            textNoElement: grapesjs.xrmLang[this.language].traitManagerSelectElement // Text to show in case no element selected
        };
    };

    // Périphériques d'affichage - Manager pris en charge par le plugin Newsletter
    this.getHTMLTemplateEditorDeviceManager = function () {
        /*
        return {
            appendTo: '#templateEditorPanelDevices_' + this.name,
            devices: [{
                name: grapesjs.xrmLang[this.language].buttonDevicesDesktop,
                width: '', // default size
            }, {
                name: grapesjs.xrmLang[this.language].buttonDevicesMobile,
                width: '320px', // this value will be used on canvas width
                widthMedia: '480px', // this value will be used in CSS @media
            }]
        };
        */
        return {};
    };

    // Blocs
    // On instancie un Manager à vide, le panneau sera alimenté par le plugin blocks-basic
    // Puis on ajoutera ensuite nos propres blocs via un appel séparé à customizeHTMLTemplateEditorBlocs();
    this.getHTMLTemplateEditorBlockManager = function () {
        /*
         * return {
            appendTo: '#templateEditorBlocks_' + this.name,
            blocks: null,
        };
        */
        return {};
    };

    // Panneaux
    // On instancie un objet à vide, le contenu sera alimenté par les plugins
    // Puis on effectuera nos personnalisations après instanciation via un appel séparé à customizeHTMLTemplateEditorPanels();
    this.getHTMLTemplateEditorPanels = function () {
        return {
            /*
            defaults: [
                // Panneau requis par le plugin Newsletter
                {
                    id: 'views',
                    el: '#templateEditorPanelViews_' + this.name,
                    buttons: [
                        {
                            id: 'open-blocks',
                            command: 'open-blocks',
                            className: 'fa fa-th-large',
                            active: true, // active by default
                            attributes: { title: grapesjs.xrmLang[this.language].buttonOpenBlocks },
                        },
                        // ...
                    ],
                }
            ]
        */
        };
    };

    // On définit les messages grapesJs
    //TODO gestion de la localization dans grapesJs
    this.getHTMLTemplateEditorMessage = function () {
        return {
            locale: 'en',
            localeFallback: 'en',
            messages: {
                en: {
                    assetManager: {
                        addButton: grapesjs.xrmLang[this.language].addButton,
                        inputPlh: grapesjs.xrmLang[this.language].inputPath,
                        uploadTitle: grapesjs.xrmLang[this.language].uploadText
                    },
                    traitManager: {
                        label: grapesjs.xrmLang[this.language].traitManagerContainerLabel
                    }
                }
            }
        }
    };


    // Commandes exécutables lors de clics sur les boutons
    // Surcharge de la commande par défaut pour personnaliser le message
    this.getHTMLTemplateEditorCommands = function () {
        return {
            defaults: [
                {
                    id: 'canvas-clear-reinject',
                    run: function (editor, sender) {
                        if (sender && typeof (sender.set) == "function")
                            sender.set('active', false);
                        // Puis on exécute la commande
                        if (confirm(grapesjs.xrmLang[that.language].dialogEmptyCanvasConfirm)) {
                            // Backlog #450 - Reset du canevas et de ses CSS existantes
                            // Semble poser de nombreux problèmes, donc désactivé pour l'instant. On ne fait que remplacer le contenu via setComponents()
                            // https://github.com/artf/grapesjs/issues/351
                            // https://github.com/artf/grapesjs/issues/1115
                            // https://github.com/artf/grapesjs/issues/1357
                            // https://github.com/artf/grapesjs/issues/986
                            // https://github.com/artf/grapesjs/issues/552
                            // #71 938 - Suite à la mise en place d'options influençant la gestion des CSS (ex : forceClass),
                            // reprise du reset des CSS pour corriger les différences de rendu entre 2 grapesjs de contexte différent
                            // (ex : édition de modèle/utilisation) : https://github.com/artf/grapesjs/issues/488
                            editor.setStyle('');
                            editor.CssComposer.getAll().reset();
                            editor.DomComponents.getWrapper().setStyle('');
                            editor.DomComponents.getWrapper().set('content', '');
                            var comps = editor.DomComponents.clear();
                            // Stockage des données (par défaut : en localStorage) - N'est pas utilisé, la prise en charge se faisant via nos managers comme CKEditor
                            // Par précaution, on ne vide donc pas le localStorage ici comme le fait la fonction native de grapesjs, le localStorage pouvant être utilisé
                            // ailleurs sur XRM (exemple : CTI)
                            /*
                            setTimeout(function () {
                                localStorage.clear();
                            }, 0);
                            */
                            that.setColor(""); // Backlog #619 - Remise à zéro de la couleur de fond après RAZ
                            that.updatePermanentBadgeDisplay(false); // Backlog #457 : On efface le badge permanent après RAZ
                            that.injectCSS("grapesjs/grapesjs-eudonet.css", true); // Réinjection de la CSS de personnalisation après RAZ
                        }
                    },
                    stop: function (editor, sender) {

                    }
                },
                /* Backlog #279 - Bouton permettant d'afficher/masquer le menu droit
                 * Attention au sens de fonctionnement : si le bouton est cliqué/enfoncé/actif, on retire la classe pour masquer le menu
                 * S'il est inactif, on l'ajoute pour afficher le menu, ce qui permet de le garder ouvert lorsqu'on clique sur les autres boutons (ce qui désactive celui-ci)
                 * #72 031 : Cette "entorse" au fonctionnement natif de grapesjs nécessite de passer le paramètre force à 1 depuis grapesjs 0.14.55 à l'exécution (ou l'arrêt)
                 * de la commande, car, autrement, étant déjà en statut actif correspondant au statut du bouton, grapesjs refuse de l'exécuter, sauf si force: 1 ou si la
                 * config globale des commandes comporte l'option "strict" à 0 (elle est à 1 par défaut depuis la 0.14.57)
                 */
                {
                    id: 'hideshow-rightpanel',
                    run: function (editor, sender) {
                        if (sender && typeof (sender.set) == "function")
                            sender.set('active', true);
                        removeClass(that.getSrcElement(), "openMenu");
                    },
                    stop: function (editor, sender) {
                        if (sender && typeof (sender.set) == "function")
                            sender.set('active', false);
                        addClass(that.getSrcElement(), "openMenu");
                        // A la désactivation (= affichage du menu), on active le bouton "Blocs" pour ne pas laisser un menu vide
                        var openBlocksButton = that.htmlTemplateEditor.Panels.getButton("views", "open-blocks");
                        openBlocksButton.set("active", true);
                    },
                },
                /* Backlog #325 - Afficher/Masquer les images en tenant compte des images étendues Eudonet */
                {
                    id: 'toggle-images-extended',
                    run: function (editor, sender) {
                        var components = editor.getComponents();
                        that.toggleImages(components);
                    },
                    stop: function (editor) {
                        var components = editor.getComponents();
                        that.toggleImages(components, 1);
                    },
                },
                /* Backlog #342 - Mode Prévisualisation et Plein écran combiné */
                {
                    id: 'preview-fullscreen',
                    run: function (editor, sender) {
                        if (sender && typeof (sender.set) == "function")
                            sender.set('active', true);
                        // Exécution des commandes natives
                        // L'activation du fullscreen provoque une erreur dans la console de Chrome 71 si le mode Fullscreen est déjà activé,
                        // ou si le document n'est pas actif ("Document not active"). On vérifie donc le statut en JS au préalable
                        // https://github.com/jpilfold/ngx-image-viewer/issues/23
                        // https://github.com/artf/grapesjs/issues/1667
                        var isFullscreen = document.fullscreen || document.fullscreenElement || document.webkitFullscreenElement || document.mozFullScreenElement || window.fullScreen;
                        if (!isFullscreen)
                            editor.runCommand("fullscreen");
                        editor.runCommand("preview");
                        // Remplacement du bouton natif permettant de sortir du mode, par un autre bouton
                        // Remplacer uniquement le click() du bouton natif semblant déclencher des commandes en boucle
                        var editorEl = editor.getEl();
                        var pfx = editor.Config.stylePrefix;
                        var newPreviewOffButtonId = "grapesjs-preview-off-eudonet";
                        var existingPreviewOffButtons = document.querySelectorAll("span." + pfx + "off-prv");
                        var nativePreviewOffButton = null;
                        for (var i = 0; i < existingPreviewOffButtons.length; i++) {
                            if (existingPreviewOffButtons[i].id != newPreviewOffButtonId)
                                nativePreviewOffButton = existingPreviewOffButtons[i];
                        }
                        var newPreviewOffButton = document.getElementById(newPreviewOffButtonId);
                        if (nativePreviewOffButton)
                            nativePreviewOffButton.style.display = "none";
                        if (!newPreviewOffButton) {
                            newPreviewOffButton = document.createElement('span');
                            newPreviewOffButton.id = newPreviewOffButtonId;
                            newPreviewOffButton.className = (nativePreviewOffButton ? nativePreviewOffButton.className : pfx + 'off-prv fa fa-eye-slash'); // ISO bouton natif
                            editorEl.appendChild(newPreviewOffButton);
                            newPreviewOffButton.onclick = function () {
                                editor.stopCommand('preview-fullscreen');
                            };
                        }
                        newPreviewOffButton.style.display = 'inline-block';
                    },
                    stop: function (editor, sender) {
                        if (sender && typeof (sender.set) == "function")
                            sender.set('active', false);
                        else
                            editor.Panels.getButton("options", "preview-fullscreen").set("active", false);

                        // Exécution des commandes natives
                        editor.stopCommand("preview");
                        // La désactivation du fullscreen provoque une erreur dans la console de Chrome 71 si le mode Fullscreen n'est pas réellement activé,
                        // a été désactivé par un moyen externe (ex : Echap),  ou si le document n'est pas actif ("Document not active"). On vérifie donc le statut en JS au préalable
                        // https://github.com/jpilfold/ngx-image-viewer/issues/23
                        // https://github.com/artf/grapesjs/issues/1667
                        var isFullscreen = document.fullscreen || document.fullscreenElement || document.webkitFullscreenElement || document.mozFullScreenElement || window.fullScreen;
                        if (isFullscreen)
                            editor.stopCommand("fullscreen");
                        // Masquage du bouton custom créé dans run()
                        var newPreviewOffButton = document.getElementById("grapesjs-preview-off-eudonet");
                        if (newPreviewOffButton)
                            newPreviewOffButton.style.display = 'none';
                    }
                },
                /* Backlog #354 - Affichage de l'aide */
                {
                    id: 'help-tutorial',
                    run: function (editor, sender) {
                        that.helpTutorial(true);
                        // On relâche directement le bouton
                        editor.Panels.getButton("options", "help-tutorial").set("active", false);
                    },
                    stop: function (editor) {
                        // On relâche directement le bouton
                        editor.Panels.getButton("options", "help-tutorial").set("active", false);
                    },
                },
            ]
        };
    };

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    // Méthodes permettant d'enrichir grapesjs après initialisation

    // On ajoute des éléments personnalisés aux managers initialisés via le init() ci-dessous avec les méthodes this.getHTMLTemplateEditor*()
    // Ces initialisations pouvant être surchargées par les plugins ajoutés, il faut faire des appels après-coup si on souhaite ajouter des éléments à ceux déjà
    // alimentés par les plugins.
    // L'ordre des appels doit être modifié en fonction des dépendances de chaque élément vis-à-vis de l'autre (par ex. si un bouton ajouté via
    // customizeHTMLTemplateEditorButtons utilise une commande définie par customizeHTMLTemplateEditorCommands(), appeler d'abord customizeHTMLTemplateEditorCommands())
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------

    // Evènements déclenchés lors d'interactions avec grapesjs
    // Cette méthode définit notamment .on('load'), qui appellera ensuite toutes les autres méthodes customizeHTMLTemplateEditor*
    // Ceci, afin que l'ajout se fasse après l'intervention des plugins alimentant eux-mêmes le composant
    // (Si l'appel est fait lors de grapesjs.init(), les composants des managers de grapesjs ne sont pas encore chargés)
    this.customizeHTMLTemplateEditorEvents = function () {
        that.trace("Personnalisation de l'éditeur avancé après initialisation - Câblage des évènements");

        // Liste des évènements disponibles (sortof) : https://github.com/artf/grapesjs/blob/dev/src/editor/index.js#L33

        this.htmlTemplateEditor.on('change:device', function () { that.trace("Périphérique actuellement sélectionné pour l'édition : ", that.htmlTemplateEditor.getDevice()) });

        // Evènement déclenché à la sélection d'un bloc dans grapesjs
        this.htmlTemplateEditor.on('component:selected', function (model) {
            that.trace("Nouvel élément sélectionné sur l'éditeur de modèles avancé " + (model ? " : " + model.get("type") + " -- " + model.get("content") : ""));

            var currentlySelectedElementOrModel = model;

            // Backlog #295 et #446 - Si grapesjs sélectionne un élément susceptible d'être inclus dans un conteneur personnalisé (ex : image), on force la sélection du conteneur
            // à sa place pour le rendre éditable directement via CKEditor, mais uniquement si c'est sur ce conteneur que CKEditor doit agir (cas de Image)
            if (currentlySelectedElementOrModel && currentlySelectedElementOrModel.getEl().parentElement && currentlySelectedElementOrModel.getEl().parentElement.getAttribute("data-gjs-type") == "eudonet-extended-image") {
                that.htmlTemplateEditor.select(currentlySelectedElementOrModel.getEl().parentElement);
                currentlySelectedElementOrModel = that.htmlTemplateEditor.getSelected();
            }

            if (currentlySelectedElementOrModel) {
                // Backlog #356 : On définit la toolbar à afficher en fonction du bloc sélectionné. C'est l'évènement rteToolbarPosUpdate qui se chargera de la MAJ de l'affichage grâce à cette variable
                that.toolbarTypeForTemplateEditor = currentlySelectedElementOrModel.get("type").replace('eudonet-extended-', ''); // un bloc "eudonet-extended-button" sera considéré comme "button" ci-dessous, idem pour "image"
                // Backlog #261/#410/#446/#447/#453 - Déclenchement de l'édition lors de la sélection d'un bloc, et non plus au clic
                that.setEditingFct(currentlySelectedElementOrModel);
            }


        });

        this.htmlTemplateEditor.on('component:toggled', function (model) {
            that.trace("Nouvel élément activé sur l'éditeur de modèles avancé " + (model ? " : " + model.get("type") + " -- " + model.get("content") : ""));

            // Backlog #457 : On maintient le badge affiché sur l'élément sélectionné
            that.updatePermanentBadgeDisplay(true, this, model);
        });

        this.htmlTemplateEditor.on('component:update:content', function (model) {
            that.trace("Nouvel élément de l'éditeur de modèles avancé " + (model ? " : " + model.get("type") + " -- " + model.get("content") : ""));
        });

        // Cette méthode centralise tous les appels aux autres méthodes customizeHTMLTemplateEditor* - cf. commentaire de la méthode customizeHTMLTemplateEditorEvents()
        this.htmlTemplateEditor.on('load', function (model) {

            that.trace("Personnalisation de l'éditeur avancé après initialisation - Chargement de personnalisations supplémentaires sur l'éditeur avancé...");

            // On ajoute des éléments personnalisés aux managers initialisés via grapesjs.init() ci-dessous avec les méthodes this.getHTMLTemplateEditor*()
            // Ces initialisations pouvant être surchargées par les plugins ajoutés, il faut faire des appels après-coup si on souhaite ajouter des éléments à ceux déjà
            // alimentés par les plugins.
            // L'ordre des appels doit être modifié en fonction des dépendances de chaque élément vis-à-vis de l'autre (par ex. si un bouton ajouté via
            // customizeHTMLTemplateEditorButtons utilise une commande définie par customizeHTMLTemplateEditorCommands(), appeler d'abord customizeHTMLTemplateEditorCommands())

            that.customizeHTMLTemplateEditorStorage(model); // Stockage des données (par défaut : en localStorage) - N'est pas utilisé, la prise en charge se faisant via nos managers comme CKEditor
            that.customizeHTMLTemplateEditorSelectors(model); // Sélecteurs CSS
            that.customizeHTMLTemplateEditorStyles(model); // Styles
            that.customizeHTMLTemplateEditorTraits(model); // Traitements (Paramètres des composants)
            that.customizeHTMLTemplateEditorDevices(model); // Périphériques d'affichage
            that.customizeHTMLTemplateEditorComponents(model); // Composants (utilisés par les blocs)
            that.customizeHTMLTemplateEditorBlocks(model); // Blocs
            that.customizeHTMLTemplateEditorButtons(model); // Boutons
            that.customizeHTMLTemplateEditorPanels(model); // Panneaux latéraux
            that.customizeHTMLTemplateEditorCommands(model); // Commandes exécutables par les boutons

            // Puis on ajoute une "fausse" classe icon-* sur tous les boutons, afin de leur appliquer le style du thème Eudonet en cours
            var allGrapesJsButtons = document.querySelectorAll(".gjs-pn-btn");
            for (var i = 0; i < allGrapesJsButtons.length; i++) {
                if (allGrapesJsButtons[i].className.indexOf("icon-") == -1)
                    allGrapesJsButtons[i].className += " icon-grapesjs";
            }

            // Backlog #267 - Injection d'une CSS spécifique sur l'iframe de l'éditeur pour rendre les blocs "responsive" - QBO/MAB
            // Backlog #304 - Lors de l'édition d'un e-mailing, ce point est pris en charge par eMailingTemplateManager qui injecte le contenu du fichier en même temps que le modèle
            // l'injection passe alors par eMailingTpl.Load > eMailingTpl.ManageFeedback > eMain.updateMemoData > eMain.updateMailWithTemplate > eMemoEditor.injectCSS avec le contenu
            // de la CSS
            // Pour tous les autres modes qui ne sont pas alimentés avec un modèle, il faut toutefois réaliser cette injection à l'initialisation de l'éditeur, côté client
            // Pour vérifier si le contenu a déjà été injecté ou non, on vérifie à la fois l'existence de la CSS à partir de son URL, mais également à partir d'une fraction de son
            // contenu (cas où le contenu même de la CSS est injecté sur la page, et non pas à partir de son URL)
            // Backlog #581 - Très étrangement, sur Firefox, le chargement de la fenêtre des modèles de mail n'est pas encore tout à fait terminé quand cette fonction est
            // exécutée, entraînant une erreur "InvalidAccessError: A parameter or an operation is not supported by the underlying object" empêchant l'injection de la CSS
            // avec nos personnalisations. On englobe donc l'injection dans une fonction que l'on réexécute en différé en cas d'exception
            var customGrapesJsCss = "grapesjs/grapesjs-eudonet";
            var checkCssFctTimer = null;
            var checkCssFct = function () {
                try {
                    if (!hasCss(customGrapesJsCss, document, true) && !hasCssRule(".eudonet-extended-container", document, true)) {
                        that.injectCSS(customGrapesJsCss + ".css", true);
                        that.trace("La CSS personnalisée " + customGrapesJsCss + " a été chargée.");
                    }
                    if (checkCssFctTimer)
                        window.clearTimeout(checkCssFctTimer);
                }
                catch (ex) {
                    if (ex && ex.name == "InvalidAccessError") {
                        checkCssFctTimer = window.setTimeout(checkCssFct, 100);
                        that.trace("La vérification des feuilles de style a échoué en raison d'une erreur " + ex.name + ", elle sera réitérée en différé.");
                    }
                    else
                        that.trace("La vérification des feuilles de style a échoué en raison d'une erreur non gérée : " + ex.name + ". Il se peut que la page présente des défauts d'affichage.");
                }
            }

            //  checkCssFct();

            // On met à jour le booléen autorisant le rendu définitif du composant dans createHTMLTemplateEditor()
            that.templateEditorCustomizationsLoaded = true;


            that.trace("L'éditeur avancé a été chargé.");
        });


        if (!that.enableAdvancedFormular) {
            // User Story #296/Backlog #258 - On force le positionnement de la toolbar de CKEditor sur le haut du canevas de grapesjs
            this.htmlTemplateEditor.on('rteToolbarPosUpdate', function (pos, config) {
                if (that.dockToolbar) {
                    // On cible la barre d'outils de CKEditor dans le DOM
                    var grapesjsCanvas = document.querySelector(".gjs-cv-canvas");
                    var grapesjsTopPanels = document.querySelector(".gjs-pn-panels");
                    var rteToolbar = document.querySelector(".gjs-rte-toolbar");
                    // On altère ensuite le style de la barre d'outils CKEditor pour les propriétés non exposées/gérées par grapesjs
                    var rteToolbarHeight = rteToolbar.getBoundingClientRect().height;
                    // La largeur de la barre d'outils est gérée en CSS via eMemoEditor.css, en fonction de la présence
                    // ou non du volet de droite
                    //rteToolbar.style.width = grapesjsCanvas.getBoundingClientRect().width + "px";
                    // Puis on modifie les coordonnées gérées en interne par grapesjs pour positionner la toolbar
                    /*
                     * Les coordonnées ci-dessous seraient pour un positionnement en fixed si on ne déplaçait pas l'élément en-dehors du canevas ci-dessous.
                     * Mais ça maintinent alors la barre au même endroit, même en cas de scroll sur la fenêtre mère.
                    pos.top = that.htmlTemplateEditor.initialCanvasBoundingRect.y;
                    pos.left = that.htmlTemplateEditor.initialCanvasBoundingRect.x;
                    */
                    // On déplace donc la toolbar dans le DOM afin qu'elle soit positionnée en-dehors du canevas, et reste ancrée à cet endroit lorsqu'on scrolle sur la
                    // fenêtre de l'assistant
                    if (grapesjsCanvas.previousElementSibling != rteToolbar)
                        var rteToolbarMoved = grapesjsCanvas.parentNode.insertBefore(rteToolbar, grapesjsCanvas);
                    // Puis on ajuste les coordonnées à lui appliquer, qui sont alors celles d'un positionnement relatif : à 0 à gauche par rapport à son parent, et en-dessous
                    // de la hauteur du panneau supérieur de grapesjs qui fait 40px
                    pos.top = grapesjsTopPanels.firstChild.getBoundingClientRect().height; // on cible le premier élément dimensionné du panneau supérieur (qui n'est qu'un conteneur sans taille)
                    pos.left = 0; // coordonnée en int ici, elle sera convertie et appliquée en px par grapesjs
                    // On court-circuite l'auto-réajustement de la toolbar opéré par grapes.js (cf. source de la méthode "udpatePosition" (avec la faute d'orthographe)
                    // en lui faisant croire qu'on n'a pas encore atteint la bordure du canevas
                    // Il faudrait idéalement modifier le paramètre config.adjustToolbar et le passer à 0 sur cette méthode pour désactiver cette fonctionnalité, mais
                    // cet objet config ne semble pas exposé du tout par grapesjs
                    pos.canvasTop = pos.top - 1;
                    // Et on décale le canevas pour laisser la place à la toolbar, en utilisant la position initiale mémorisée dans rte:enable pour la restaurer à la désactivation de CKEditor
                    // Backlog #357 : désactivation, cela pose problème à grapesjs pour positionner ses encadrés de sélection. On positionnera désormais CKEditor dans un espace qui lui est réservé
                    /*
                    var canvas = document.querySelector(".gjs-cv-canvas");
                    // Attention, ici, il faut utiliser rteToolbarHeight détectée plus haut, et non pas pos.elementHeight, qui correspond en fait à la hauteur de la petite barre
                    // d'outils grapesjs positionnée autour de chaque composant sélectionné
                    canvas.style.top = getNumber(that.htmlTemplateEditor.initialCanvasTop) + rteToolbarHeight + "px";
                    canvas.style.height = that.htmlTemplateEditor.initialCanvasBoundingRect.height - rteToolbarHeight + "px";
                    */
                }

                // Backlog #356 : Mise à jour des boutons de la barre d'outils en fonction du dernier composant sélectionné
                if (that.setToolBarForTemplateEditor)
                    that.setToolBarForTemplateEditor(that.toolbarTypeForTemplateEditor);

                // Backlog #428 - On vérifie si l'instance de CKEditor affichée a déjà été affectée à this.htmlEditor, afin de faire fonctionner toutes les méthodes déjà 
                // câblées pour CKEditor dans eMemoEditor.js, sur l'instance gérée par grapesjs.
                // Il faut faire ce traitement à chaque réaffichage et non pas dans rte:enable, car une nouvelle instance de CKEditor est crée pour chaque bloc éditable cliqué.
                // this.htmlTemplateEditor.RichTextEditor.getToolbarEl() renvoie alors bien un seul conteneur (dont la classe CSS est gjs-rte-toolbar), mais pouvant potentiellement
                // contenir plusieurs div de barres d'outils CKEditor, correspondant à chaque instance créée (classe CSS cke_*). Il faut donc parcourir toutes les instances et
                // sélectionner celle qui n'est PAS en display:none. D'où la nécessité de faire ce traitement sur rteToolbarPosUpdate et non rte:enable
                // Le paramètre model n'est pas passé ici, car cet évènement rteToolbarPosUpdate ne le transmet pas.
                that.htmlEditor = that.getHTMLEditor(true);
            });
        }
        // Lorsque grapesjs affiche CKEditor...
        this.htmlTemplateEditor.on('rte:enable', function (model) {
            // Backlog #356 : On définit la toolbar à afficher en fonction du bloc sélectionné. C'est l'évènement rteToolbarPosUpdate qui se chargera de la MAJ de l'affichage grâce à cette variable
            that.toolbarTypeForTemplateEditor = model.getChildrenContainer().getAttribute("data-gjs-type").replace('eudonet-extended-', ''); // un bloc "eudonet-extended-button" sera considéré comme "button" ci-dessous, idem pour "image"

            // Lorsque la barre d'outils de CKEditor sera positionnée par rteToolbarPosUpdate(), on mettra à jour la variable this.htmlEditor avec la référence de CKEditor
            // réellement affichée. Si on le fait dès le déclenchement de l'évènement rte:enable, la barre d'outils ne sera pas forcément encore affichée, ce qui pourra
            // provoquer un mauvais ciblage de l'instance CKEditor (cf. traitement effectué dans rteToolbarPosUpdate)
            //that.htmlEditor = that.getHTMLEditor(true, model);

            //if (that.dockToolbar) {
            // User Story #296/Backlog #258 - A la première instanciation de CKEditor, on mémorise la position du canevas, pour pouvoir la modifier en positionnant la barre d'outils via rteToolbarPosUpdate
            // Backlog #357 : désactivation, cela pose problème à grapesjs pour positionner ses encadrés de sélection. On positionnera désormais CKEditor dans un espace qui lui est réservé
            //    var canvas = document.querySelector(".gjs-cv-canvas");
            //    if (typeof (that.htmlTemplateEditor.initialCanvasBoundingRect) == "undefined") {
            //        that.htmlTemplateEditor.initialCanvasBoundingRect = canvas.getBoundingClientRect();
            //        that.htmlTemplateEditor.initialCanvasTop = window.getComputedStyle(canvas).getPropertyValue("top") /*|| canvas.style.top*/ || 0; // La position "top" du canevas est en fait définie en CSS. Il faut donc aller la requêter via getComputedStyle()
            //        that.htmlTemplateEditor.initialCanvasHeight = canvas.style.height;
            //    }
            //}
        });

        // User Story #296/Backlog #258 - Lorsque CKEditor est désactivé, on rétablit la position du canevas modifiée par rteToolbarPosUpdate
        // Backlog #357 : désactivation, cela pose problème à grapesjs pour positionner ses encadrés de sélection. On positionnera désormais CKEditor dans un espace qui lui est réservé
        this.htmlTemplateEditor.on('rte:disable', function (model) {
            /*
            if (that.dockToolbar) {
                that.trace("Editeur désactivé, restauration de la position initiale du canevas");
                var canvas = document.querySelector(".gjs-cv-canvas");
                canvas.style.top = that.htmlTemplateEditor.initialCanvasTop || "";
                canvas.style.height = that.htmlTemplateEditor.initialCanvasHeight || "";
            }
            */
        });

        // Backlog #271 - Ajout de classes CSS lors de l'ajout de blocs sur le canevas
        // On surveille block:drag:start car c'est le seul qui semble transmettre des informations utilisables (notamment le type et le libellé du bloc sélectionné)
        // On mémorise alors l'élément concerné, et on se servira de ses attributs dans l'évènement qui gère le dépôt, en considérant que le dernier élément détecté
        // sera bien celui déposé
        this.htmlTemplateEditor.on('block:drag:start', function (model) {
            that.htmlTemplateEditor.currentlyDraggedElement = model;
        });

        //tâche #3 238
        this.checkIfParentIsComponentWithClassName = function (model, classeName) {
            var parentElement = model.collection;
            if (model.collection)
                parentElement = model.collection.parent;
            if (parentElement && parentElement.collection) {
                if (parentElement.getClasses().indexOf('canvas') > -1)
                    return false;
                else if (parentElement.getClasses().indexOf(classeName) > -1)
                    return true;
                else
                    return this.checkIfParentIsComponentWithClassName(model.collection.parent, classeName);
            }
        }

        //Après le chargement de composants, on perd quelques props comme le 'name', 'droppble'... 
        //Cette fonction permet de mettre à jour un composant après le chargement dans grapesJS
        this.cleanComponentsAfterReload = function (model) {
            if (model.attributes["type"] == "input" || model.attributes["type"] == "checkbox" || model.attributes["type"] == "label" || model.attributes["type"] == "eudonet-extended-input") {
                model.droppable = false;
            }
            else if (model.getClasses().indexOf('formCanvas') > -1)
                model.set("droppable", ".inputCanvas, .worldlinePaiment,.struct,.basic,.image");
            else if (model.collection && model.collection != undefined && model.collection.parent.getClasses().indexOf('formCanvas') > -1) {

                model.set("droppable", ".inputCanvas, .worldlinePaiment,.struct,.basic,.image");

            }

            if (model.attributes["tagName"] == "textarea")
                model.set("type", "input");
            
            if (model.attributes.name == undefined || model.attributes.name == "")
                model.set("name", grapesjs.xrmLang[that.language].blockNativeLabels["cell"]);
            if (model.collection && model.collection != undefined) {
                if (model.collection.parent && model.collection.parent.getClasses().indexOf('struct') > -1 && model.getClasses().indexOf('inputCanvas') <= -1) {
                    model.set("type", "cell");
                    model.set("editable", "false");
                    model.set("name", grapesjs.xrmLang[that.language].blockNativeLabels["cell"]);
                }
            }

            if (model.getClasses().indexOf('inputCanvas') > -1) {
                model.set("type", "eudonet-extended-input");
            }
            else if (model.getClasses().indexOf('eudonet-extended-cat') > -1) {
                model.set("type", "eudonet-extended-select");
            }
            if (model.attributes["tagName"] == "extended-worldline-btn") {
                model.copyable = false;
                model.set("type", "button-worldline");
            }

            if (model.getClasses().indexOf('struct') > -1 || model.getClasses().indexOf('eudonet-extended-btn') > -1
                || model.getClasses().indexOf('inputCanvas') > -1 || model.getClasses().indexOf('eudonet-extended-label') > -1
                || model.getClasses().indexOf('canvas') > -1) {
                model.set("droppable", "false");
            }

            //#3 328
            //ToDo: ajouter d'autres éléments
            if (model.getClasses().indexOf('inputCanvas') > -1) {
                var canvasLabel = '';
                if (model.getClasses().indexOf('email') > -1)
                    canvasLabel = top._res_2365;
                else if (model.getClasses().indexOf('phone') > -1)
                    canvasLabel = top._res_5138;
                else if (model.getClasses().indexOf('champSaisie-num') > -1)
                    canvasLabel = top._res_236;
                else if (model.getClasses().indexOf('champSaisie-date') > -1)
                    canvasLabel = top._res_231;
                else if (model.getClasses().indexOf('champSaisie-memo') > -1)
                    canvasLabel = top._res_2688;
                else if (model.getClasses().indexOf('champSaisie') > -1)
                    canvasLabel = top._res_2601;
                else if (model.getClasses().indexOf('checkbox') > -1)
                    canvasLabel = top._res_2204;
                else if (model.getClasses().indexOf('champSaisie-dropdown-multiple') > -1)
                    canvasLabel = top._res_247;
                else if (model.getClasses().indexOf('champSaisie-dropdown') > -1)
                    canvasLabel = top._res_2806;


                model.set("name", canvasLabel);
                model.get('components').models.forEach(function (item) {
                    var prefixLabel = "";
                    if (item.attributes["type"] == "text" || item.attributes["type"] == "label")
                        prefixLabel = top._res_223; //Libellé
                    else if (model.collection != undefined && model.collection) {
                        if ((item.collection.parent.attributes["type"] == "eudonet-extended-input" || item.collection.parent.attributes["type"] == "eudonet-extended-select")
                            && (item.attributes["type"] == "input" || item.attributes["type"] == "checkbox" || item.attributes["type"] == "eudonet-extended-select"))
                            prefixLabel = top._res_2816;//Zone de saisie
                    }
                    else
                        prefixLabel = item.attributes["type"];

                    item.set("name", canvasLabel + " - " + prefixLabel);
                });
            }

            else if (model.getClasses().indexOf('formCanvas') > -1)
                model.set("name", top._res_1142);

            if (model.getClasses().indexOf('inputCanvas') > -1 || model.collection != undefined && model.collection
                || model.attributes["type"] == "label" || that.checkIfParentIsComponentWithClassName(model, "eudonet-extended-cat")) {
                if (model.collection.parent.getClasses().indexOf('inputCanvas') > -1) {
                    model.attributes.highlightable = false;
                }
            }
            else
                model.attributes.highlightable = true;

            if (that.checkIfParentIsComponentWithClassName(model, "eudonet-extended-cat")) {
                model.attributes.selectable = model.attributes.highlightable = model.attributes.hoverable = model.attributes.badgable = model.attributes.layerable = false;
            }

        }
        //Tâche #3 238: Autoriser le dépôt de bloc de structure dans un formulaire / emailing
        this.htmlTemplateEditor.on("component:create", function (model) {
            if (that.enableAdvancedFormular) {
                if (that.checkIfParentIsComponentWithClassName(model, 'formCanvas') && model.getClasses().indexOf('cell') > -1)
                    model.attributes.droppable = ".inputCanvas, .champSaisie, .worldlinePaiment, .email, .phone, .checkbox, .champSaisie-num, .simpleText, .basic, .struct, .image";
                else if (model.getClasses().indexOf('cell') > -1 && model.attributes.droppable && model.attributes.droppable.indexOf("worldlinePaiment") < 0) {
                    model.attributes.droppable = model.attributes.droppable + ", .worldlinePaiment";
                }

                //On ajoute les bons props
                that.cleanComponentsAfterReload(model);
            }
            else {
                //if ((that.checkIfParentIsComponentWithClassName(model, 'header') || that.checkIfParentIsComponentWithClassName(model, 'body')
                //    || that.checkIfParentIsComponentWithClassName(model, 'footer') || that.checkIfParentIsComponentWithClassName(model, 'article')
                //    || that.checkIfParentIsComponentWithClassName(model, 'texte')) && model.getClasses().indexOf('cell') > -1) {
                //    model.attributes.draggable = false;
                //    model.attributes.removable = false;
                //    model.attributes.copyable = false;
                //}

                //On attribue le bon libellé aux composants de type 'bloc de structure'
                switch (model.attributes["tagName"]) {
                    case "table":
                    case "tbdoy":
                    case "row":
                    case "tr":
                        model.set("name", grapesjs.xrmLang[that.language].blockNativeLabels[model.attributes["type"]]);
                        break;
                    case "td":
                        if (model.getClasses().indexOf('Column') > -1 && model.attributes.components.models && model.attributes.components.models.length == 1
                            && model.attributes.components.models[0].attributes.tagName == "" && model.attributes.components.models[0].attributes.type == "textnode" && model.attributes.components.models[0].attributes.name == "" && model.attributes.components.models[0].attributes.content.trim() == "") {
                            model.attributes.components.models = [];
                        }
                        break;
                    case "div": {
                        if (model.getClasses().indexOf('DivTxt') > -1) {
                            model.set("type", "text");
                            model.attributes.highlightable = true;
                            //model.attributes.droppable = model.attributes.draggable = model.attributes.removable = model.attributes.copyable = false;
                            //if (model.collection.parent && model.collection.parent.attributes["type"] == 'cell')
                            //    model.collection.parent.attributes.droppable = model.collection.parent.attributes.draggable = model.collection.parent.attributes.removable = model.collection.parent.attributes.copyable = false;
                            //if (model.collection.parent.collection.parent && model.collection.parent.collection.parent.attributes["type"] == 'row')
                            //    model.collection.parent.collection.parent.attributes.droppable = model.collection.parent.collection.parent.attributes.draggable = model.collection.parent.collection.parent.attributes.removable = model.collection.parent.collection.parent.attributes.copyable = false;
                            model.set("name", grapesjs.xrmLang[that.language].blockNativeLabels[model.attributes["type"]]);
                        }

                        //if (model.collection.parent && model.collection.parent.attributes["type"] == 'cell')
                        //    model.attributes.droppable = ".column1,.column2,.column37,.texte,.button,.image,.article,.separator";

                        //if (model.collection.parent && model.collection.parent.getClasses().indexOf('NonDroppableCell') > -1)
                        //    model.attributes.droppable = false;

                      
                        //if (model.getClasses().indexOf('eudonet-extended-container-image') > -1)
                        //    model.attributes.draggable = false;
                    }
                        break;
                    case "img":
                        //if (model.collection.parent.getClasses().indexOf('eudonet-extended-container-image') > -1)
                        //    model.attributes.draggable = false;
                        model.attributes.droppable = model.attributes.highlightable = false;
                        break;
                    case "br":
                    case "a":
                        //if (model.attributes.type == 'link') {
                            //model.attributes.droppable = model.attributes.draggable = model.attributes.removable = model.attributes.copyable = false;
                            //if (model.collection.parent && model.collection.parent.attributes["type"] == 'eudonet-extended-button')
                            //    model.collection.parent.set("type", "text");
                            //if (model.collection.parent && model.collection.parent.attributes["type"] == 'text')
                            //    model.collection.parent.attributes.droppable = model.collection.parent.attributes.draggable = model.collection.parent.attributes.removable = model.collection.parent.attributes.copyable = false;
                            //if (model.collection.parent.collection.parent && model.collection.parent.collection.parent.attributes["type"] == 'cell')
                            //    model.collection.parent.collection.parent.attributes.droppable = model.collection.parent.collection.parent.attributes.draggable = model.collection.parent.collection.parent.attributes.removable = model.collection.parent.collection.parent.attributes.copyable = false;
                            //if (model.collection.parent.collection.parent.collection.parent && model.collection.parent.collection.parent.collection.parent.attributes["type"] == 'row')
                            //    model.collection.parent.collection.parent.collection.parent.attributes.droppable = model.collection.parent.collection.parent.collection.parent.attributes.draggable = model.collection.parent.collection.parent.collection.parent.attributes.removable = model.collection.parent.collection.parent.collection.parent.attributes.copyable = false;
                            //if (model.collection.parent.collection.parent.collection.parent.collection.parent && model.collection.parent.collection.parent.collection.parent.collection.parent.attributes["type"] == 'table')
                            //    model.collection.parent.collection.parent.collection.parent.collection.parent.attributes.droppable = model.collection.parent.collection.parent.collection.parent.collection.parent.attributes.draggable = model.collection.parent.collection.parent.collection.parent.collection.parent.attributes.removable = model.collection.parent.collection.parent.collection.parent.collection.parent.attributes.copyable = false;
                            //if (model.collection.parent.collection.parent.collection.parent.collection.parent.collection.parent && model.collection.parent.collection.parent.collection.parent.collection.parent.collection.parent.attributes["type"] == 'cell')
                            //    model.collection.parent.collection.parent.collection.parent.collection.parent.collection.parent.attributes.droppable = model.collection.parent.collection.parent.collection.parent.collection.parent.collection.parent.attributes.draggable = model.collection.parent.collection.parent.collection.parent.collection.parent.collection.parent.attributes.removable = model.collection.parent.collection.parent.collection.parent.collection.parent.collection.parent.attributes.copyable = false;
                            //if (model.collection.parent.collection.parent.collection.parent.collection.parent.collection.parent.collection.parent && model.collection.parent.collection.parent.collection.parent.collection.parent.collection.parent.collection.parent.attributes["type"] == 'row')
                            //    model.collection.parent.collection.parent.collection.parent.collection.parent.collection.parent.collection.parent.attributes.droppable = model.collection.parent.collection.parent.collection.parent.collection.parent.collection.parent.collection.parent.attributes.draggable = model.collection.parent.collection.parent.collection.parent.collection.parent.collection.parent.collection.parent.attributes.removable = model.collection.parent.collection.parent.collection.parent.collection.parent.collection.parent.collection.parent.attributes.copyable = false;
                        //}
                        //break;
                    case "span":
                    case "p":
                    case "b":
                    case "u":
                    case "label":
                    case "s":
                    case "u":
                    case "strong":
                        model.attributes.droppable = model.attributes.selectable = model.attributes.highlightable = model.attributes.hoverable = model.attributes.badgable = model.attributes.layerable = false;
                        break;
                    default: break;
                }
            }
        });


        // Backlog #271 - Ajout de classes CSS lors de l'ajout de blocs sur le canevas
        // Avec un préfixe eudonet- pour éviter les conflits avec d'autres classes déjà existantes (ex : divider)
        this.htmlTemplateEditor.on('canvas:drop', function (dataTransfer, model) {
            that.trace("Dropped element: " + JSON.stringify(dataTransfer));

            // Backlog #360 - Si le drop n'est pas issu d'un drag de bloc grapesjs (ex : drag d'une image depuis l'explorateur), on se sert du model passé en paramètre
            var droppedEltAttr = null;
            var droppedEltType = null;
            if (that.htmlTemplateEditor.currentlyDraggedElement) {
                droppedEltAttr = that.htmlTemplateEditor.currentlyDraggedElement.attributes;
                droppedEltType = droppedEltAttr.type || droppedEltAttr.id; // à défaut, id du bloc droppé = son type
            }
            else if (model && model.length == 1 && model[0].attributes)
                droppedEltType = model[0].attributes.childTagName; // "image" dans le cas d'une image dans notre conteneur personnalisé

            // Support des composants multi-enfants (ex : list-items) : si le composant est seul, on l'englobe dans une Array, et on parcourt toujours en tableaux
            if (!model.length)
                model = new Array(model);
            for (var i = 0; i < model.length; i++) {
                if (model[i]) {
                    var newID = that.getNewElementID(model[i]);
                    // Backlog #618 - On n'affecte pas d'ID à l'élément si l'option forceClass de grapesjs est à false, car cette option ajoute alors elle-même un ID
                    if (this.getEditor().getConfig().forceClass) {
                        model[i].id = newID;
                    }
                    model[i].addClass("eudonet-" + droppedEltType);
                    model[i].addClass("eudonet-" + droppedEltType + "-" + newID); // Backlog #286, #358 - Ajout d'une classe spécifique à chaque nouvelle instance de composant, pour que l'utilisateur puisse surcharger ses styles
                    // Backlog #356 : On définit la toolbar à afficher en fonction du bloc sélectionné. C'est l'évènement rteToolbarPosUpdate qui se chargera de la MAJ de l'affichage grâce à cette variable
                    that.toolbarTypeForTemplateEditor = model[i].get("type").replace('eudonet-extended-', ''); // un bloc "eudonet-extended-button" sera considéré comme "button" ci-dessous, idem pour "image"
                }
            }
            that.htmlTemplateEditor.currentlyDraggedElement = null;
        });


        this.htmlTemplateEditor.on('canvas:dragenter', function (dataTransfer) {
            that.trace("Drag Enter - " + JSON.stringify(dataTransfer));
        });

        this.htmlTemplateEditor.on('canvas:dragover', function (dragEvent) {
            that.trace("Drag Over - " + JSON.stringify(dragEvent));
        });

        this.htmlTemplateEditor.on('canvas:dragend', function (dragEvent) {
            that.trace("Drag End - " + JSON.stringify(dragEvent));
        });

        // Backlog #360 et #502 - On vérifie la validité des éléments déposés avant d'accepter le drop
        this.htmlTemplateEditor.on('canvas:dragdata', function (dataTransfer, result) {
            that.trace("Drag Data - " + JSON.stringify(dataTransfer), " - Result: " + JSON.stringify(result));
            if (result && result.content) {
                // Backlog #502 - Si result.content est "null" (en valeur de type string) et que d'autres données ont été transmises dans un autre format
                // Bug déclaré chez grapesjs : https://github.com/artf/grapesjs/issues/1797
                if (typeof (result.content) == "string" && result.content === "null" && dataTransfer.getData("text/html") === result.content) {
                    result.content = "";
                    /*
                    Le code ci-dessous, en cas d'anomalie, renvoie la valeur textuelle la plus proche (sans mise en forme) correspondant au code HTML déplacé à la souris
                    Demande #71 818 - On désactive finalement cette alternative, car elle pose problème dans la plupart des cas avec un contenu mixte
                    notamment les images, où l'URL est renvoyée, sans possibilité d'identifier si on a déplacé une image en amont ou non.
                    Et même en interdisant le déplacement sur ce type d'éléments, cela ne résout pas la problématique d'origine qui peut se poser dès qu'on sélectionne
                    un ensemble de contenu mixte (texte, images...) après avoir instancié au moins une fois CKEditor. Et dans ce cas de figure, impossible de bloquer
                    le déplacement à la souris dès le départ, car le contenu mixte n'est pas identifiable comme un modèle de composant où on peut positionner draggable,
                    highlightable, droppable... à false. Donc, si l'anomalie "null" se produit, on ignore complètement le contenu renvoyé par le navigateur
                    Il faudra attendre un correctif côté grapesjs sur le ticket 1797
                    */
                    /*
                    if (dataTransfer.types && dataTransfer.types.length > 0) {
                        for (var i = 0; i < dataTransfer.types.length; i++) {
                            // Evitons justement de reprendre la valeur null à partir du format text/html...
                            if (dataTransfer.types[i] == "text/plain") {
                                var altValue = dataTransfer.getData(dataTransfer.types[i]);
                                if (altValue && altValue != "") {
                                    result.content = altValue;
                                    that.trace("Attention, la valeur de l'élément déposé à été renvoyée en HTML, mais valorisée à null. Utilisation de la valeur " + dataTransfer.types[i] + " à la place, dont le contenu est " + altValue);
                                    break;
                                }
                            }
                        }
                    }
                    */
                    if (result.content == "")
                        that.trace("Attention, la valeur de l'élément déposé à été renvoyée en HTML, mais valorisée à null. Annulation de l'action.");
                }
                // Backlog #360 - On remplace toute image déposée depuis l'ordinateur en drag & drop par une image avec conteneur
                else if (typeof (result.content) == "object" && result.content.length == 1 && result.content.file) {
                    var content = result.content[0];
                    if (content) {
                        var contentAttributes = content.attributes;
                        var contentFile = content.file;
                        var contentType = content.type;

                        // On ajoute la source de l'image parmi les attributs
                        contentAttributes.src = contentFile.name;

                        // TODO: il faudrait ici uploader l'image dans DATAS via nos managers/handlers, puis renvoyer l'URL de l'image uploadée.
                        // En attendant, on remplace l'image par l'image par défaut du bloc Image
                        if (contentAttributes.src.indexOf("http") != 0)
                            contentAttributes.src = "themes/default/images/image_GrapesJS_150.png";

                        if (contentType == "image") {
                            // Backlog #360 - On remplace toute image déposée depuis l'ordinateur en drag & drop par une image avec conteneur
                            var componentType = "eudonet-extended-image";
                            result.content = [
                                {
                                    tagName: "div",
                                    type: componentType,
                                    activeOnRender: 1,
                                    removable: that.isComponentRemovable(componentType),
                                    draggable: that.isComponentDraggable(componentType),
                                    droppable: that.isComponentDroppable(componentType),
                                    copyable: that.isComponentCopyable(componentType),
                                    editable: that.isComponentEditable(componentType),
                                    stylable: that.isComponentStylable(componentType),
                                    resizable: that.isComponentResizable(componentType),
                                    selectable: that.isComponentSelectable(componentType),
                                    highlightable: that.isComponentHighlightable(componentType),
                                    hoverable: that.isComponentHoverable(componentType),
                                    layerable: that.isComponentLayerable(componentType),
                                    badgable: that.isComponentBadgable(componentType),
                                    classes: ['eudonet-extended-container-image'],
                                    childTagName: 'image', // Permet de récupérer le code HTML du composant depuis son init()
                                    extendedComponentParameters: contentAttributes
                                }
                            ];
                        }
                    }
                }
            }
        });

        this.htmlTemplateEditor.on("sorter:drag", function (data) {
            that.trace("SorterDrag - " + (data.targetModel ? data.targetModel.attributes.tagName + ", " + data.targetModel.attributes.type : "(no model: " + JSON.stringify(data.target)) + ", content: " + (data.target ? data.target.innerText : "<unknown>") + ", pos: " + JSON.stringify(data.pos));
        }
        );

        // Backlog #261, #295, #445 - Désactivation du mode Edition avant de procéder à la suppression d'un composant
        // Permet de faire fonctionner le bouton Supprimer même si le composant est en cours d'édition
        // Notamment utile pour les composants dont on a forcé l'édition au simple clic (ex : blocs Texte, ou Conteneur d'image)
        this.htmlTemplateEditor.on("run:core:component-delete:before", function () {
            that.disableEditing(this);
        });
        //demande #92 225
        this.htmlTemplateEditor.on('run:core:component-delete', function (options) {
            if (that.enableAdvancedFormular && (options[0].getClasses().indexOf('eudonet-extended-label') > -1 || options[0].attributes["type"] == "input")) {
                options[0].opt.collection.parent.remove();
            }
        });

        // Backlog #261, #295, #445 et Demande #71 935 - Désactivation du mode Edition avant de procéder à la copie
        // Permet de faire fonctionner le bouton Dupliquer même si le composant est en cours d'édition
        // Notamment utile pour les composants dont on a forcé l'édition au simple clic (ex : blocs Texte, ou Conteneur d'image)
        this.htmlTemplateEditor.on("run:core:copy:before", function () {
            that.disableEditing(this);
        });

        that.trace("Personnalisation de l'éditeur avancé après initialisation - Câblage des évènements terminé");
    };

    // Stockage des données (par défaut : en localStorage) - N'est pas utilisé, la prise en charge se faisant via nos managers comme CKEditor
    this.customizeHTMLTemplateEditorStorage = function (model) { };

    // Sélecteurs CSS
    this.customizeHTMLTemplateEditorSelectors = function (model) { };

    // Styles
    this.customizeHTMLTemplateEditorStyles = function (model) {
        // https://grapesjs.com/docs/modules/Style-manager.html
        // https://github.com/artf/grapesjs/wiki/API-Style-Manager#addproperty
        var styleManager = this.htmlTemplateEditor.StyleManager;

        if (styleManager) {
            // Liste de tailles de polices prédéfinies sans notion chiffrée (Petite, Moyenne, Grande)
            // On enrichit le secteur "Typographie" du plugin Newsletter
            // TOCHECK: Etrangement, le plugin ne donne pas d'IDs aux secteurs qu'il ajoute. Ceux-ci semble donc avoir un ID correspondant à leur nom affiché
            // cf. Backlog #432, ticket déclaré sur le GitHub grapesjs
            // Backlog #341 et #432 - Personnalisation finalement non retenue
            /*
            styleManager.addProperty(
                "typography",
                {
                    id: 'predefined-font-size',
                    name: grapesjs.xrmLang[this.language].stylesSectorExtendedPropertyPredefinedFontSize,
                    property: 'font-size',
                    type: 'select',
                    defaults: '32px',
                    // List of options, available only for 'select' and 'radio'  types
                    options: [
                        { value: '12px', name: grapesjs.xrmLang[this.language].stylesSectorExtendedPropertyPredefinedFontSizeSmall },
                        { value: '18px', name: grapesjs.xrmLang[this.language].stylesSectorExtendedPropertyPredefinedFontSizeMedium },
                        { value: '32px', name: grapesjs.xrmLang[this.language].stylesSectorExtendedPropertyPredefinedFontSizeLarge },
                    ],
                }
            );
            */

            // Backlog #341 - Suppression de la section "Arrière-plan" du groupe "Habillage" dans le gestionnaire de styles
            styleManager.removeProperty("decorations", "background");
        }
    };

    // Traitements (Paramètres des composants)
    this.customizeHTMLTemplateEditorTraits = function (model) { };

    // Périphériques d'affichage
    this.customizeHTMLTemplateEditorDevices = function (model) { };

    // Blocs
    this.customizeHTMLTemplateEditorBlocks = function (model) {
        that.trace("Personnalisation de l'éditeur avancé après initialisation - Paramétrage des blocs");

        // https://grapesjs.com/docs/modules/Blocks.html
        var blockManager = this.htmlTemplateEditor.BlockManager;

        // Backlog #349, #352 - Les blocs affichés sont récupérés depuis un paramètre externe alimenté soit par les renderers, soit depuis eParamIFrame à défaut
        // (lus depuis le fichier blocks / eudonet.json pour les blocs natifs, et la base de données pour les blocs utilisateur)
        var addedBlocksCount = 0;
        var allBlocksFromParam = this.htmlTemplateEditorBlocks;
        // Si non défini par le renderer/appelant, utilisation du paramètre d'eParamIFrame chargé par eParam.cs
        if (!allBlocksFromParam || !allBlocksFromParam.length || allBlocksFromParam.length < 1) {
            try {
                var oeParam = getParamWindow();
                allBlocksFromParam = this.enableAdvancedFormular ? eval(this.grapesJSBlocks) : eval(oeParam.GetParam("HTMLTemplateEditorBlocks"));
                if (this.wordlineBlocs) {
                    var allWorldLineblocs = eval(this.wordlineBlocs);
                    // Puis on rajoute nos blocs
                    for (var i = 0; i < allWorldLineblocs.length; i++) {
                            allBlocksFromParam.push(allWorldLineblocs[i]);
                    }
                }
            }
            catch (ex) { }
        }
        // Backlog #349 et #352 - Si le paramètre est un tableau exploitable, on ajoute tous les blocs qu'il contient via l'API
        if (allBlocksFromParam && allBlocksFromParam.length && allBlocksFromParam.length > 0) {
            blockManager.render(new Array()); // on supprime tous les blocs natifs existants avec un render() à vide : https://github.com/artf/grapesjs/issues/499
            // Puis on rajoute nos blocs
            for (var i = 0; i < allBlocksFromParam.length; i++) {
                if (allBlocksFromParam[i].id != '' && allBlocksFromParam[i].opts) {
                    blockManager.add(allBlocksFromParam[i].id, allBlocksFromParam[i].opts);
                    addedBlocksCount++;
                }
            }
        }
        // Backlog #38 : Si le tableau de blocs passé en paramètre, ou récupéré via eParamIFrame, est inexploitable, on utilise les blocs natifs de grapesjs qu'on personnalise : Texte, Image, Bouton, Séparateur, Liste, 1 Colonne, 2 Colonnes
        if (addedBlocksCount == 0) {
            var allBlocksNative = new Array();
            var allBlocksNativeIds = new Array("text", "image", "button", "divider", "list-items", "sect100", "sect50");
            // Construction du nouveau tableau avec les blocs filtrés, dans l'ordre souhaité : https://github.com/artf/grapesjs/issues/780
            // En appelant une méthode spécifique pour récupérer le bloc complet à partir de l'existant ET le personnaliser au passage
            for (var i = 0; i < allBlocksNativeIds.length; i++) {
                var currentBlock = this.getCustomizedBlock(blockManager.get(allBlocksNativeIds[i]));
                if (currentBlock)
                    allBlocksNative.push(currentBlock);
            }
            // Et on redéclenche le rendu des blocs à partir du tableau d'objets déjà préparés par l'API (via blockManager.get) : https://github.com/artf/grapesjs/issues/499
            that.trace("Personnalisation de l'éditeur avancé après initialisation - Paramétrage des blocs - Rafraîchissement");
            blockManager.render(allBlocksNative);
            that.trace("Personnalisation de l'éditeur avancé après initialisation - Paramétrage des blocs terminé");


        }


    };

    // Boutons
    this.customizeHTMLTemplateEditorButtons = function (model) {
        that.trace("Personnalisation de l'éditeur avancé après initialisation - Paramétrage des boutons");

        // https://github.com/artf/grapesjs/blob/gh-pages/js/grapesjs-preset-webpage.js#L198
        var panelManager = this.htmlTemplateEditor.Panels;

        // Backlog #212 - On supprime le bouton Exporter qui, au final, fait double emploi avec le bouton Importer aka. Source qui reprend le code HTML actuel du composant
        panelManager.removeButton('options', 'export-template');

        // Backlog #212 - On remplace le bouton Importer par un identique, mais avec une icône spécifique (l'icône n'étant actuellement pas personnalisable sur ce plugin)
        // getButton() renvoie un objet child grapesjs, dont les propriétés sont contenues dans un tableau nommé attributes.
        // Pour accéder aux propriétés du bouton attendues par addButton(), il faut donc utiliser Button.attributes
        // Backlog #430 : on opte finalement pour un remplacement de l'icône via CSS/JS direct, car remplacer le bouton provoque des effets de bord
        // En le remplaçant par le module d'import du plugin Newsletter, il semblerait, par exemple, que l'ouverture de la fenêtre câble un comportement étrange sur la
        // touche Entrée, qui fait planter le navigateur - Ce bug a toutefois pu être corrigé entre temps par la release 0.14.55/0.14.57 de grapesjs et sa protection contre
        // l'exécution en boucle de commandes
        var importButton = this.htmlTemplateEditor.Panels.getButton("options", "gjs-open-import-webpage");
        if (importButton) {
            importButton.set("className", "fa fa-code");
            // #72 031 : il faut forcer la commande en statut inactif au chargement, car pour une raison étrange, elle est considérée
            // comme active par défaut, et le mécanisme de vérification d'exécution des commandes introduit en 0.14.55 empêche le déclenchement d'une commande considérée comme
            // déjà activée
            importButton.set("active", false);
        }
        /*
        var nativeImportButtonProperties = panelManager.getButton('options', 'gjs-open-import-webpage').attributes; // La fonctionnalité d'import du plugin Webpage remplace celle du plugin Newsletter (le dernier plugin chargé fait foi). C'est donc le bouton du plugin Webpage qu'il faut cibler
        // Et pour effectuer un remplacement, il faut supprimer puis rajouter le bouton (pas de replaceButton dans l'API Panels à l'heure actuelle)
        panelManager.removeButton('options', nativeImportButtonProperties.id);
        panelManager.addButton(
            'options', {
                id: "gjs-open-import-template-custom", // Il ne faut SURTOUT pas mettre le même ID que le bouton natif, car ce bouton peut déclencher sa commande. Lui affecter son ID déclencherait donc des commandes en boucle infinie lorsqu'on appelerait stopCommmand("xxx") ou runCommand("xxx")
                className: nativeImportButtonProperties.className.replace('fa-download', 'fa-code'), // on conserve les autres classes CSS, on ne remplace que celle concernant l'icône
                command: "gjs-open-import-template", // On déclenche la commande native du plugin Newsletter. La fonctionnalité d'import du plugin Newsletter gère mieux les styles supportés par les messageries (CSS inline). Pour Webpage, la fenêtre d'import serait gjs-open-import-webpage
                attributes: nativeImportButtonProperties.attributes
            }
        );
        */

        // Affectation d'une méthode personnalisée au bouton "Effacer tout" afin d'effectuer quelques traitements personnalisés
        var nativeCleanAllButtonProperties = panelManager.getButton('options', 'canvas-clear').attributes;
        // Et pour effectuer un remplacement, il faut supprimer puis rajouter le bouton (pas de replaceButton dans l'API Panels à l'heure actuelle)
        panelManager.removeButton('options', nativeCleanAllButtonProperties.id);
        panelManager.addButton(
            'options', {
            id: "canvas-clear-reinject", // Il ne faut SURTOUT pas mettre le même ID que le bouton natif, car ce bouton peut déclencher sa commande. Lui affecter son ID déclencherait donc des commandes en boucle infinie lorsqu'on appelerait stopCommmand("xxx") ou runCommand("xxx")
            className: nativeCleanAllButtonProperties.className,
            command: "canvas-clear-reinject",
            attributes: nativeCleanAllButtonProperties.attributes
        }
        );

        // Backlog #342 - Bouton combinant les modes Prévisualisation et Plein écran - On conserve les propriétés du bouton Prévisualisation
        var nativeFullscreenButtonProperties = panelManager.getButton('options', 'fullscreen').attributes;
        var nativePreviewButtonProperties = panelManager.getButton('options', 'preview').attributes;
        // Et pour effectuer un remplacement, il faut supprimer puis rajouter le bouton (pas de replaceButton dans l'API Panels à l'heure actuelle)
        panelManager.removeButton('options', nativeFullscreenButtonProperties.id);
        panelManager.removeButton('options', nativePreviewButtonProperties.id);
        panelManager.addButton(
            'options', {
            id: "preview-fullscreen", // Il ne faut SURTOUT pas mettre le même ID que le bouton natif, car ce bouton déclenche sa commande. Lui affecter son ID déclencherait donc des commandes en boucle infinie lorsqu'on appelerait stopCommmand("preview") ou runCommand("preview")
            className: nativePreviewButtonProperties.className,
            command: "preview-fullscreen",
            attributes: nativePreviewButtonProperties.attributes
        }
        );

        // Rajout du bouton Afficher/Masquer les images du plugin Newsletter, qui est supprimé par le plugin Webpage
        panelManager.addButton(
            'options', {
            id: 'gjs-toggle-images',
            className: 'fa fa-image',
            command: 'toggle-images-extended',
            attributes: { title: grapesjs.xrmLang[this.language].buttonToggleImages },
            active: false
        }
        );

        // Backlog #354 - Rajout du bouton Aide/Tutorial pour grapesjs
        panelManager.addButton(
            'options', {
            id: 'help-tutorial',
            className: 'fa fa-question',
            command: 'help-tutorial',
            attributes: { title: grapesjs.xrmLang[this.language].buttonHelp },
            active: false
        }
        );

        that.trace("Personnalisation de l'éditeur avancé après initialisation - Paramétrage des boutons terminé");
    };

    // Panneaux latéraux
    this.customizeHTMLTemplateEditorPanels = function (model) {
        that.trace("Personnalisation de l'éditeur avancé après initialisation - Paramétrage des panneaux");

        // https://github.com/artf/grapesjs/wiki/API-Panels

        // User Story #297 - Réagencement des boutons dans un ordre différent et modification des icônes
        // Backlog #281
        // L'API de grapesjs ne dispose pas actuellement de méthode pour réordonner les boutons.
        // On doit donc les supprimer puis les recréer dans l'ordre souhaité

        // On commence par récupérer les boutons ajoutés par le plugin
        var openStyleManagerButton = this.htmlTemplateEditor.Panels.getButton("views", "open-sm");
        var openTraitManagerButton = this.htmlTemplateEditor.Panels.getButton("views", "open-tm");
        var openLayersButton = this.htmlTemplateEditor.Panels.getButton("views", "open-layers");
        var openBlocksButton = this.htmlTemplateEditor.Panels.getButton("views", "open-blocks");

        // Backlog #282 - Pour le bouton Calques, on change l'icône proposée par défaut par le plugin grapesjs
        openLayersButton.attributes.className = "icon-table_bases";

        // Puis on supprime tout
        this.htmlTemplateEditor.Panels.removeButton("views", "open-sm");
        this.htmlTemplateEditor.Panels.removeButton("views", "open-tm");
        this.htmlTemplateEditor.Panels.removeButton("views", "open-layers");
        this.htmlTemplateEditor.Panels.removeButton("views", "open-blocks");

        // Et on rajoute tout dans l'ordre souhaité
        // Backlog #279 - Ajout d'un bouton permettant d'afficher/masquer le volet droit de grapesjs
        var hideShowRightPanelButton = this.htmlTemplateEditor.Panels.addButton("views",
            {
                id: 'hideShowPanel',
                className: 'fa fa-bars grapesjs-hideshowbutton',
                command: 'hideshow-rightpanel',
                attributes: { title: grapesjs.xrmLang[this.language].buttonHideShowRightPanel },
                active: false,  // Backlog #340 - Attention au sens (cf. commande hideshow-rightpanel) : on active le menu par défaut, donc on passe le bouton en statut "relevé",
                // car l'activer a pour fonction de masquer le menu en passant les autres boutons en statut "inactif" ("relevé")
                // Comme le bouton est créé en "inactif" sans que ce statut soit positionné après avoir été "actif", la commande liée au bouton ne sera pas automatiquement
                // déclenchée, et le menu ne sera donc pas affiché automatiquement. La commande sera donc exécutée manuellement depuis customizeHTMLTemplateEditorCommands()
                // #72 031 : avec l'option force: 1 depuis grapesjs 0.14.55 qui empêche par défaut le déclenchement de commandes dont le statut correspond déjà à celui demandé
            }
        );
        this.htmlTemplateEditor.Panels.addButton("views", hideShowRightPanelButton);
        this.htmlTemplateEditor.Panels.addButton("views", openBlocksButton);
        this.htmlTemplateEditor.Panels.addButton("views", openStyleManagerButton);
        if (this.enableAdvancedFormular)//On affiche le panel 'Settings' dans le cadre du formaulaire avancé
            this.htmlTemplateEditor.Panels.addButton("views", openTraitManagerButton); /* Backlog #299 - On masque ce panneau */
        this.htmlTemplateEditor.Panels.addButton("views", openLayersButton);

        // Backlog #322 - Activation de l'option "Afficher les bordures" par défaut
        this.htmlTemplateEditor.Panels.getButton("options", "sw-visibility").set("active", true);
        //Tâche #2 708: on masque par défaut le panel 'Settings'
        this.showHideSettingsElement(false);

        // Contrairement au BlockManager, il n'est pas nécessaire de faire appel à render() ici.
        // (cf. customizeHTMLTemplateEditorBlocks())

        that.trace("Personnalisation de l'éditeur avancé après initialisation - Paramétrage des panneaux terminé");
    };

    // Commandes exécutables par les boutons
    this.customizeHTMLTemplateEditorCommands = function (model) {
        that.trace("Personnalisation de l'éditeur avancé après initialisation - Paramétrage des commandes");

        // Backlog #340 - Déclenchement manuel de la commande d'affichage/masquage du menu, en mode "relevé" (cf. commentaire plus haut dans customizeHTMLTemplateEditorPanels())
        // La commande n'étant pas déclenchée automatiquement, du fait que le bouton soit créé en mode "relevé" dès le départ : il n'y a pas de changement de statut provoquant la commande
        // Demande #72 031 - La commande doit être forcée depuis la v0.14.55 de grapesjs, qui introduit une protection empêchant le déclenchement en boucle de commandes
        // si elle est déjà en statut correspondant à celui souhaité (soit active = true si on appelle runCommand, ou active = false si on appelle stopCommand)
        // https://github.com/artf/grapesjs/releases/tag/v0.14.55
        // https://github.com/artf/grapesjs/issues/1881
        that.htmlTemplateEditor.stopCommand("hideshow-rightpanel", { force: 1 });

        that.trace("Personnalisation de l'éditeur avancé après initialisation - Paramétrage des commandes terminé");
    };

    // Etend les propriétés de certains composants
    // Backlog #286
    this.customizeHTMLTemplateEditorComponents = function (model) {
        that.trace("Personnalisation de l'éditeur avancé après initialisation - Paramétrage des composants et des modèles");

        // 1/3 : Ajout de types spécifiques, pouvant être inclus dans un conteneur
        // -----------------------------------------------------------------------

        // https://github.com/artf/grapesjs/issues/1155
        var extendedTypes = new Array("image", "button", "video", "map", "canvas");
        var extendedTypesContainers = new Array("text", "text", "default", "default", "default");

        // Exemple basé sur https://github.com/artf/grapesjs/issues/1255
        for (var i = 0; i < extendedTypes.length; i++) {
            that.trace("Personnalisation de l'éditeur avancé après initialisation - Paramétrage des composants et des modèles - Paramétrage du type " + extendedTypes[i]);

            //var baseType = this.htmlTemplateEditor.DomComponents.getType(extendedTypes[i]);
            var baseType = this.htmlTemplateEditor.DomComponents.getType(extendedTypesContainers[i]);
            if (baseType) {
                this.htmlTemplateEditor.DomComponents.addType('eudonet-extended-' + extendedTypes[i], {
                    //model: this.htmlTemplateEditor.DomComponents.getType(extendedTypes[i]).model.extend(
                    model: baseType.model.extend(
                        {
                            // Backlog #42 - Object.assign() n'est pas supporté par IE de base. Il est assuré ici par un polyfill déclaré dans eTools
                            defaults: Object.assign({}, baseType.model.prototype.defaults, {
                                name: grapesjs.xrmLang[this.language].blockNativeLabels["container-" + extendedTypes[i]], // Nom affiché sur le contour du composant
                            }),

                            // use init instead of initialize, so you avoid `defaultType.model.prototype...`
                            init: function () {
                                // Si le composant n'a pas encore de contenu
                                // #71 935 : il faut vérifier à la fois components().length, mais aussi le contenu HTML via attributes.content, car le composant peut avoir
                                // du contenu HTML sans pour autant avoir été encore analysé par le Parser (donc components().length == 0)
                                // Exemple : cas du clic sur le bouton Dupliquer d'un conteneur d'image
                                if ((!this.components() || this.components().length == 0) && (!this.attributes || !this.attributes.content || this.attributes.content.trim() == "")) {
                                    this.components(
                                        that.getExtendedComponentContents(this.get("childTagName"), this.get("extendedComponentParameters"))
                                    );
                                }
                            },

                            /*
                            toHTML: function () {
                                return that.getExtendedComponentContents(this.get("childTagName"), this.get("extendedComponentParameters"));
                            },
                            */
                        },
                        {
                            isComponent: function (el) {
                                try {

                                    // https://github.com/artf/grapesjs/issues/1155
                                    // Backlog #379 - si on tombe sur un élément comportant des classes CSS, on les analyse à la recherche de celles identifiant nos composants
                                    // on pourrait tout aussi bien se baser sur des attributs HTML et d'autres choses, mais comme le code est susceptible d'être modifié par
                                    // l'utilisateur final, on considèrera que les classes CSS font foi (puisque ce sont elles qui gèrent notamment la mise en forme)
                                    if (el && el.classList) {
                                        for (var i = 0; i < el.classList.length; i++) {
                                            // Nos composants étant des conteneurs contenant eux-mêmes d'autres éléments, ce sont ces conteneurs eux-mêmes qu'il faut cibler,
                                            // et non leurs éléments enfants. Sinon, l'élément enfant serait considéré comme conteneur
                                            // C'est le conteneur qui est le composant, et non son contenu
                                            // cf. addType() dans customizeHTMLTemplateEditorComponents() et getCustomizedBlock() pour le cas où les blocs ne sont pas issus de eudonet.json)
                                            if (el.classList[i].indexOf('eudonet-extended-container-') > -1) {
                                                var elementType = el.classList[i].replace('eudonet-extended-container-', ''); // donnera par ex. "image"
                                                // Si le tag HTML de l'élément correspond à celui attendu pour ce type de composant, on renvoit le type associé
                                                // Permet d'éviter par ex. de renvoyer à tort un type de composant sur un élément HTML incompatible où l'utilisateur aurait positionné la classe
                                                // identifiant le composant
                                                // ex : <a href="#" class="eudonet-extended-image">Lien</a> au lieu de <img src="test.png class="eudonet-extended-image">
                                                if (el.tagName && el.tagName.toLowerCase() === that.getExtendedComponentElementTag(el.classList[i])) {
                                                    var componentType = 'eudonet-extended-' + elementType;
                                                    return {
                                                        //tagName: that.getExtendedComponentElementTag(el.classList[i]),
                                                        type: componentType,
                                                        activeOnRender: 1,
                                                        removable: that.isComponentRemovable(componentType),
                                                        draggable: that.isComponentDraggable(componentType),
                                                        droppable: that.isComponentDroppable(componentType),
                                                        copyable: that.isComponentCopyable(componentType),
                                                        editable: that.isComponentEditable(componentType),
                                                        stylable: that.isComponentStylable(componentType),
                                                        resizable: that.isComponentResizable(componentType),
                                                        selectable: that.isComponentSelectable(componentType),
                                                        highlightable: that.isComponentHighlightable(componentType),
                                                        hoverable: that.isComponentHoverable(componentType),
                                                        layerable: that.isComponentLayerable(componentType),
                                                        badgable: that.isComponentBadgable(componentType),
                                                        classes: ['eudonet-extended-container-' + elementType],
                                                        childTagName: elementType, // Permet de récupérer le code HTML du composant depuis son init()
                                                    };
                                                }
                                            }
                                            // Dans le cas où le composant serait directement le contenu (image, etc.), il faudrait utiliser le code ci-dessous et modifier
                                            // les critères d'identification du contenu comme composant
                                            else if (el.classList[i] == "image" && el.tagName && el.tagName.toLowerCase() === "img")
                                                return { type: 'image', activeOnRender: 1 }

                                            else if (el.classList[i] == "text" && el.tagName && el.tagName.toLowerCase() === "div" && el.getElementsByClassName("link").length > 0 && el.getElementsByTagName("a").length > 0)
                                                return { type: 'text', activeOnRender: 1 }

                                            /*
                                            else if (el.classList[i].indexOf('eudonet-extended-') > -1) {
                                                var elementType = el.classList[i].replace('eudonet-extended-', ''); // donnera par ex. "image"
                                                // Si le tag HTML de l'élément correspond à celui attendu pour ce type de composant, on renvoie le type associé
                                                // Permet d'éviter par ex. de renvoyer à tort un type de composant sur un élément HTML incompatible où l'utilisateur aurait positionné la classe
                                                // identifiant le composant
                                                // ex : <a href="#" class="eudonet-extended-image">Lien</a> au lieu de <img src="test.png class="eudonet-extended-image">
                                                if (el.tagName && el.tagName.toLowerCase() === that.getExtendedComponentElementTag(elementType)) {
                                                    return { type: 'eudonet-extended-' + elementType, activeOnRender: 1 };
                                                }
                                            }
                                            */
                                        }
                                    }
                                }
                                catch (ex) {
                                    //setWait(false);
                                }
                                finally {
                                }
                            }
                        }
                    ),
                    view: baseType.view/*.extend({
						// Bind events
						
						events: {
							// If you want to bind the event to children elements
							// 'click .someChildrenClass': 'methodName',
							click: 'handleClick',
							dblclick: function () {
							}
						},
						*/

                    /*
                    customMethod: function () {
                        return "#BB1515";
                    },
                    */
                    /*
                    handleClick: function (e) {
                        //this.model.set('style', { color: this.customMethod() }); // <- Affects the final HTML code
                        //this.el.style.backgroundColor = this.customMethod(); // <- Doesn't affect the final HTML code
                        // Tip: updating the model will reflect the changes to the view, so, in this case,
                        // if you put the model change after the DOM one this will override the backgroundColor
                        // change made before
                    },
                    */
                    // The render() should return 'this'
                    /*
                        render: function () {
                            // Extend the original render method
                            var baseType = that.htmlTemplateEditor.DomComponents.getType(that.getExtendedComponentTypeFromTag(this.el.tagName.toLowerCase()));
                            if (!baseType)
                                baseType = that.htmlTemplateEditor.DomComponents.getType("text");
                            if (baseType)
                                baseType.view.prototype.render.apply(this, arguments);
                            //this.el.placeholder = 'Text here'; // <- Doesn't affect the final HTML code
                            return this;
                        },
                    })*/
                })
            }
        }

        // 2/3 : Modification simple de certains types : surcharge des propriétés par défaut de certains composants, sans autre modification
        // ---------------------------------------------------------------------------------------------------------------------------------

        // Backlog #89, #499 - Blocage de l'édition des images
        this.addCustomImageComponentType(that);


        //tâche #2 459, KJE:  - On ajoute le type custom canvas à la liste des types de composants
        componentType = "eudonet-extended-canvas";
        that.trace("Personnalisation de l'éditeur avancé après initialisation - Paramétrage des composants et des modèles - Surcharge du type " + componentType);
        var baseTypeCanevas = this.htmlTemplateEditor.DomComponents.getType(componentType);
        if (baseTypeCanevas) {
            this.htmlTemplateEditor.DomComponents.addType(componentType, {
                model: baseTypeCanevas.model.extend(
                    {
                        defaults: Object.assign({}, baseTypeCanevas.model.prototype.defaults, {
                            name: grapesjs.xrmLang[this.language].blockNativeLabels[componentType], // Nom affiché sur le contour du composant
                            removable: that.isComponentRemovable(componentType),
                            draggable: that.isComponentDraggable(componentType),
                            droppable: that.isComponentDroppable(componentType),
                            copyable: that.isComponentCopyable(componentType),
                            editable: that.isComponentEditable(componentType),
                            stylable: that.isComponentStylable(componentType),
                            resizable: that.isComponentResizable(componentType),
                            selectable: that.isComponentSelectable(componentType),
                            highlightable: that.isComponentHighlightable(componentType),
                            hoverable: that.isComponentHoverable(componentType),
                            layerable: that.isComponentLayerable(componentType),
                            badgable: that.isComponentBadgable(componentType)
                        })
                    }
                ),
                view: baseTypeCanevas.view
            });
        }
        //Tâche #2 749: on, ajoute un nouveau component type 'mergefield' dans grapesJS
        this.addMergefieldComponentType();


        // 3/3 : Modification avancée de certains types : activation/désactivation de l'édition en surchargeant les valeurs par défaut + la vue ou la méthode isComponent()
        // ----------------------------------------------------------------------------------------------------------------------------------------------------------------

        that.trace("Personnalisation de l'éditeur avancé après initialisation - Paramétrage des composants et des modèles - Paramétrage avancé - Modalités d'édition");

        // Backlog #261/#295/#410/#446/#447/#453 - Activation de l'édition d'un composant sur simple clic, plutôt que double clic par défaut
        // Fonctionnalité à activer sous conditions, cela empêche l'exécution de certaines actions par défaut qui sont bloquées en mode Edition (ex : suppression de composant)
        // Il faut alors intercepter un évènement exécuté avant une action vérifiant si editor.getModel().isEditing() = true, et passer le mode Edition à faux via getSelected().getView().disableEditing()
        // cf. évènement run:core:component-delete:before rajouté via customizeHTMLTemplateEditorEvents()
        // https://github.com/artf/grapesjs-plugin-ckeditor/issues/3
        that.setEditingFct = function (currentlySelectedElementOrModel, retryCount) {
            // Nombre de tentatives d'activation du mode Edition
            // Si le contexte n'est pas encore prêt côté grapesjs pour activer le mode Edition (= élément sélectionné mais pas encore matérialisé côté grapesjs), on 
            // retente l'activation jusqu'à un certain nombre de fois
            var maxRetryCount = 10;
            if (!retryCount)
                retryCount = 1;
            else
                retryCount++;
            // Par défaut, l'édition se fait toujours sur l'élément sélectionné via grapesjs (càd.comportant sa barre d'outils et un badge décrivant le type de composant : "Text", "Cell")
            if (!currentlySelectedElementOrModel) {
                that.trace("Le mode Edition va être activé sur la sélection en cours.");
                currentlySelectedElementOrModel = that.htmlTemplateEditor.getSelected();
            }
            else {
                that.trace("Le mode Edition va être activé sur l'élément suivant : " + currentlySelectedElementOrModel + ".");
            }
            var selectedElement = currentlySelectedElementOrModel.el ? currentlySelectedElementOrModel.el : currentlySelectedElementOrModel.getEl();
            var selectedElementModel = currentlySelectedElementOrModel.model ? currentlySelectedElementOrModel.model : currentlySelectedElementOrModel;
            var selectedElementType = selectedElementModel.get("type");
            // On vérifie si on autorise l'activation en fonction du type d'élément initialement ciblé par grapesjs			
            var enableEditing = that.canEnableEditingOnElementType(selectedElementType);
            var toggleEditing = that.canToggleEditingOnElementType(selectedElementType);
            that.trace("L'activation du caractère éditable est " + (enableEditing ? "autorisée" : "INTERDITE") + " sur les éléments de type " + selectedElementType);
            that.trace("L'activation du mode Edition est " + (toggleEditing ? "autorisée" : "INTERDITE") + " sur les éléments de type " + selectedElementType);
            // Si l'élément n'a pas encore été complètement sélectionné par grapesjs, on diffère l'activation du mode Edition. 2 cas sont actuellement identifiés :
            // - le clic/le mode Edition n'a encore jamais été déclenché sur un élément ; dans ce cas, l'objet rapporté comme sélectionné par grapesjs est undefined
            // - le mode Edition a déjà été déclenché, mais aucun objet n'a été resélectionné depuis. Dans ce cas, l'objet rapporté comme sélectionné sera le wrapper grapesjs lui-même
            // => Dans ce deuxième cas de figure, il s'agit d'un div en contenteditable=true, donc il est potentiellement rapporté comme éditable par
            // canEnableEditingOnElementType et canToggleEditingOnElementType alors qu'en réalité, on ne veut pas déclencher l'édition dessus.
            // On diffère donc le déclenchement du mode Edition si l'un des 2 cas se présente, et on attendra que grapesjs finisse d'exécuter les évènements liés au clic
            // sur un élément réellement sélectionné et éditable pour activer réellement l'édition via CKEditor.
            // Ceci, car dans la chronologie d'exécution de grapesjs, le "onclick" se déclenche, notre traitement est exécuté en premier, puis tous les autres évènements
            // systèmes de grapesjs normalement déclenchés au clic s'exécutent ensuite, dont... la sélection du composant (apposition du badge et de la barre d'outils autour)
            var isSelectionReady = (selectedElement && selectedElement.getAttribute("data-gjs-type") != "wrapper");
            if (!isSelectionReady) {
                if (retryCount < maxRetryCount) {
                    that.trace("L'édition d'un composant a été demandé, mais aucun élément sélectionné n'a été retourné. Une nouvelle tentative va être effectuée (" + retryCount + " sur " + maxRetryCount + ".");
                    var oDelayedSetEditingFct = (
                        function (currentlySelectedElementOrModel, retryCount) {
                            return function () {
                                that.setEditingFct(currentlySelectedElementOrModel, retryCount);
                            }
                        }
                    )(currentlySelectedElementOrModel, retryCount);
                    that.enableEditingTimer = window.setTimeout(oDelayedSetEditingFct, 100);
                    return;
                }
                else {
                    that.trace("L'édition d'un composant a été demandé, mais aucun élément sélectionné n'a été retourné. Abandon après " + maxRetryCount + " tentatives d'activation.");
                    return;
                }
            }
            else
                window.clearTimeout(that.enableEditingTimer);

            if (that.isInPreviewMode()) {
                that.trace("L'éditeur est en mode Prévisualisation. L'édition n'est pas autorisée dans ce mode. Abandon");
                return;
            }

            if (enableEditing) {
                that.trace("Activation du caractère éditable sur l'élément " + (selectedElement ? " : " + selectedElementType + " -- " + selectedElement.innerHTML : ""));
                // On rend l'élément cliqué éditable aux yeux de CKEditor
                if (that.enableEditingOnElement)
                    that.enableEditingOnElement(typeof (currentlySelectedElementOrModel.tagName) == "function" ? currentlySelectedElementOrModel.tagName() : currentlySelectedElementOrModel.get("tagName")); // équivalent à event.srcElement, mais en utilisant le child (this) passé par grapesjs
                // Backlog #410 - Et on force contenteditable à true sur l'élément cliqué pour autoriser le déclenchement de CKEditor au double-clic sur les liens
                selectedElement.setAttribute("contenteditable", "true");
            }

            if (toggleEditing) {
                that.trace("Activation de l'édition sur l'élément " + (selectedElement ? " : " + selectedElementType + " -- " + selectedElement.innerHTML : ""));

                // Puis on déclenche l'édition de grapesjs (fonction normalement câblée sur dblclick)
                // A partir de grapesjs 0.14.43, la fonction à déclencher se nomme onActive
                if (typeof (selectedElementModel.getView().onActive) != "undefined")
                    selectedElementModel.getView().onActive();
                // Auparavant, elle se nommait enableEditing
                // https://github.com/artf/grapesjs-plugin-ckeditor/issues/3
                // https://github.com/artf/grapesjs/issues/293
                else if (typeof (selectedElementModel.getView().enableEditing) != "undefined")
                    selectedElementModel.getView().enableEditing();
                else
                    that.trace("Impossible d'activer l'édition sur l'élément " + currentlySelectedElementOrModel);
            }
        };

        // Backlog #410/#446/#447/#453 : Activation de l'édition sur des blocs/éléments non considérés comme éditables par grapesjs nativement
        // Pour cela, on étend le modèle des éléments concernés (souvent "default" et on modifie le retour de la fonction
        // isComponent() afin qu'elle renvoie "text" pour ce type d'élément, ce qui activera toutes les fonctionnalités relatives
        // à l'élément de type "text", notamment le côté éditable
        // https://github.com/artf/grapesjs/issues/1374
        var enableEditingOnTypes = that.getEditableElementTypes();
        for (var i = 0; i < enableEditingOnTypes.length; i++) {
            that.trace("Personnalisation de l'éditeur avancé après initialisation - Paramétrage des composants et des modèles - Paramétrage avancé - Activation de l'édition sur le type " + enableEditingOnTypes[i]);
            var baseType = that.htmlTemplateEditor.DomComponents.getType(enableEditingOnTypes[i]);
            if (baseType) {
                that.htmlTemplateEditor.DomComponents.addType(enableEditingOnTypes[i], {
                    model: baseType.model,
                    view: baseType.view.extend({
                        events: {
                            'click': function (event) {
                                if (that.isMemoInstance)
                                    that.setEditingFct(this);
                            }
                        },
                    }),
                });
            }
        }

        // Backlog #582 - A l'inverse, on désactive le côté éditable des balises qu'on ne souhaite pas manipuler via grapesjs
        var disableEditingOnTypes = that.getNonNativeEditableElementTypes();
        for (var i = 0; i < disableEditingOnTypes.length; i++) {
            that.trace("Personnalisation de l'éditeur avancé après initialisation - Paramétrage des composants et des modèles - Paramétrage avancé - Désactivation de l'édition sur le type " + disableEditingOnTypes[i]);
            var baseType = that.htmlTemplateEditor.DomComponents.getType(disableEditingOnTypes[i]);
            if (baseType) {
                that.htmlTemplateEditor.DomComponents.addType(disableEditingOnTypes[i], {
                    model: baseType.model.extend(
                        {
                            // Backlog #42 - Object.assign() n'est pas supporté par IE de base. Il est assuré ici par un polyfill déclaré dans eTools
                            defaults: Object.assign({}, baseType.model.prototype.defaults, {
                                name: grapesjs.xrmLang[this.language].blockNativeLabels[disableEditingOnTypes[i]], // Nom affiché sur le contour du composant
                                // Allow to edit the content of the component (used on Text components)
                                // https://github.com/artf/grapesjs/issues/263
                                removable: that.isComponentRemovable(disableEditingOnTypes[i]),
                                draggable: that.isComponentDraggable(disableEditingOnTypes[i]),
                                droppable: that.isComponentDroppable(disableEditingOnTypes[i]),
                                copyable: that.isComponentCopyable(disableEditingOnTypes[i]),
                                editable: that.isComponentEditable(disableEditingOnTypes[i]),
                                stylable: that.isComponentStylable(disableEditingOnTypes[i]),
                                resizable: that.isComponentResizable(disableEditingOnTypes[i]),
                                selectable: that.isComponentSelectable(disableEditingOnTypes[i]),
                                highlightable: that.isComponentHighlightable(disableEditingOnTypes[i]),
                                hoverable: that.isComponentHoverable(disableEditingOnTypes[i]),
                                layerable: that.isComponentLayerable(disableEditingOnTypes[i]),
                                badgable: that.isComponentBadgable(disableEditingOnTypes[i]),
                            }),
                        },
                        {
                            // On définit une propriété custom indiquant quel est le type de composant qu'on est en train d'étendre
                            // afin de pouvoir le retrouver dans isComponent(), à laquelle cette information n'est pas transmise
                            modelType: baseType,
                            // On redéfinit ici la fonction qui permet à grapesjs d'interpréter une balise HTML et de la considérer comme tel ou tel contenu
                            // L'idée, ici, étant de lui dire de ne pas matérialiser les éléments concernés sous forme de composants sélectionnables (ex : span)
                            // https://github.com/artf/grapesjs/issues/1262
                            isComponent: function (el) {
                                // Dans tous les autres cas non pris en charge par notre surcharge, il faut renvoyer quelque chose afin que la méthode parseNode de grapesjs
                                // sache qu'il faut utiliser un objet avec le type par défaut du modèle surchargé. cf. parseNode de parser/model/ParserHtml.js dans les
                                // sources de grapesjs
                                // '' = retour par défaut utilisé dans la plupart des surcharges de isComponent, cf. grapes.js
                                // Autres retours probablement gérés : true, false, 0
                                var result = '';

                                // https://github.com/artf/grapesjs/issues/1155
                                // Il faut explicitement redéfinir le retour attendu pour les noeuds non HTML de type Text afin que
                                // leur contenu ne soit pas supprimé à la réanalyse. On reprend donc l'implémentation de dom_components/model/ComponentTextNode.js
                                // d'après les sources de grapesjs
                                if (el && el.nodeType === 3) {
                                    result = {
                                        type: 'textnode',
                                        content: el.textContent
                                    };
                                }
                                //garder les commentaires
                                else if (el.nodeType == 8) {
                                    return {
                                        tagName: 'NULL',
                                        type: 'comment',
                                        content: el.textContent
                                    };
                                }
                                // "a" ("link") fait partie de ce cas
                                else if (el && el.tagName && that.getNonNativeEditableElementTagNames().indexOf(el.tagName.toLowerCase()) > -1) {
                                    // Backlog #645 - On ne considère en Texte que le dernier élément de la hiérarchie, pour ne pas entraîner de boucle au sein de grapesjs faisant
                                    // planter le navigateur dans certains cas, et éviter de déclencher le mode Editable sur plusieurs éléments à la fois
                                    // Backlog #649 - Sauf dans le cas des liens, où il faut renvoyer le type par défaut pour qu'ils ne soient pas cliquables
                                    if (el.tagName == "A" || (el.children && el.children.length == 0)) {
                                        var substitutionType = 'text';
                                        if (el.tagName == "A")
                                            substitutionType = 'link';

                                        result = {
                                            type: substitutionType,
                                            removable: that.isComponentRemovable(substitutionType, el.tagName.toLowerCase(), !that.enableAdvancedFormular),
                                            draggable: that.isComponentDraggable(substitutionType, el.tagName.toLowerCase(), !that.enableAdvancedFormular),
                                            droppable: that.isComponentDroppable(substitutionType, el.tagName.toLowerCase()),
                                            copyable: that.isComponentCopyable(substitutionType, el.tagName.toLowerCase(), !that.enableAdvancedFormular),
                                            editable: that.isComponentEditable(substitutionType, el.tagName.toLowerCase()),
                                            stylable: that.isComponentStylable(substitutionType, el.tagName.toLowerCase()),
                                            resizable: that.isComponentResizable(substitutionType, el.tagName.toLowerCase()),
                                            selectable: that.isComponentSelectable(substitutionType, el.tagName.toLowerCase(), that.enableAdvancedFormular),
                                            highlightable: that.isComponentHighlightable(substitutionType, el.tagName.toLowerCase(), !that.enableAdvancedFormular),
                                            hoverable: that.isComponentHoverable(substitutionType, el.tagName.toLowerCase()),
                                            layerable: that.isComponentLayerable(substitutionType, el.tagName.toLowerCase()),
                                            badgable: that.isComponentBadgable(substitutionType, el.tagName.toLowerCase()),
                                        };
                                    }
                                    else {
                                        that.trace("L'élément " + JSON.stringify(el) + " doit être considéré de type Texte " + (that.isComponentEditable("text", el.tagName.toLowerCase()) ? "éditable" : "NON éditable") + ", mais il a des enfants potentiellement éditables. Il ne sera donc pas considéré comme tel.");
                                    }
                                }
                                // Si on tombe sur un composant personnalisé Eudonet, il faut renvoyer null afin que la méthode parseNode de grapesjs
                                // teste tous les cas de sa boucle et appelle le isComponent() de notre composant personnalisé pour le reconnaître comme tel (cf.plus haut)tag HTML non considéré comme éditable par grapesjs à la base, on le considère comme un noeud de type Text.
                                else if (el && el.className && el.className.indexOf('eudonet-extended-') > -1) {
                                    result = null;
                                }
                                else if (el && el.tagName == "TABLE") {
                                    result = {
                                        type: "table",
                                        droppable: false,
                                        tagName: el.tagName.toLowerCase()
                                    };
                                }
                                else if (el && el.tagName == "EXTENDED-WORLDLINE-BTN") {
                                    result = {
                                        type: "button-worldline",
                                        content: el.textContent,
                                        copyable: false,
                                        tagName: el.tagName.toLowerCase()
                                    };
                                }
                                else if (el && el.tagName == "TBODY") {
                                    result = {
                                        type: "tbody",
                                        droppable: false,
                                        /*removable: false,*/
                                        tagName: el.tagName.toLowerCase()
                                    };
                                }
                                else if (el && el.tagName == "TR") {
                                    result = {
                                        type: "row",
                                        droppable: false,
                                        /*removable: false,*/
                                        tagName: el.tagName.toLowerCase()
                                    };
                                }
                                else if (el && el.tagName == "TD") {
                                    result = {
                                        type: "cell",
                                        droppable: ".column1,.column2,.column37,.texte,.button,.image,.article,.separator",
                                        /*removable: false,*/
                                        tagName: el.tagName.toLowerCase()
                                    };
                                    if (el.className && el.className.indexOf('NonDroppableCell') > -1)
                                        result.droppable  = false;
                                }
                                else {
                                    if (el.tagName) {
                                        result = {
                                            type: this.modelType.id,
                                            tagName: el.tagName.toLowerCase() /* Backlog #615 : il est IMPERATIF de renvoyer le tagName en lowercase pour qu'il soit correctement interprété en interne par grapesjs (ex : dans component.toHTML()) */
                                        };
                                    }
                                    // Backlog #620 - Certains éléments HTML reconnus séparément, comme les commentaires, n'ont pas de tagName (nodeType === 8)
                                    // Et comme ils ne sont pas reconnus par grapesjs (aucun componentType associé), on se contentera de renvoyer l'objet tel quel à grapesjs, sans tagName
                                    else {
                                        result = {
                                            type: this.modelType.id
                                        };
                                    }
                                }

                                if (!that.htmlTemplateEditor.parsedComponentCount)
                                    that.htmlTemplateEditor.parsedComponentCount = 1;
                                else
                                    that.htmlTemplateEditor.parsedComponentCount++;
                                that.trace('[' + that.htmlTemplateEditor.parsedComponentCount + '] Retour de isComponent pour ' + (el.tagName ? el.tagName : '<élément sans tag>') + ' (nodeType ' + el.nodeType + ') : ' + JSON.stringify(result));

                                // Si on renvoie un résultat correspondant à une définition objet de composant, on personnalise la propriété name avec une ressource personnalisée
                                // (si existante) pour traduire le badge affiché
                                if (result && result !== true && result !== '' && result.type) {
                                    var customizedBadgeLabel = grapesjs.xrmLang[that.language].blockNativeLabels[result.type]; // Nom affiché sur le contour du composant
                                    result.name = customizedBadgeLabel;
                                }

                                return result;
                            }
                        }
                    ),
                    view: baseType.view
                });
            }
        }

        that.trace("Personnalisation de l'éditeur avancé après initialisation - Paramétrage des composants et des modèles terminé");
    };

    // Backlogs #38 et #286 - Transforme le bloc natif de grapesjs passé en paramètre en fonction de nos besoins
    this.getCustomizedBlock = function (block) {
        if (!block)
            return undefined;

        // Backlog #38 - Suppression de la catégorie
        // Backlog #301 - Sauf pour les structures. Mais comme grapesjs affiche d'abord les blocs avec catégories, si on veut afficher les blocs "sans catégorie" en premier, il faut
        // quand même leur affecter... une catégorie
        var blockCategory = grapesjs.xrmLang[this.language].blockNativeCategories["basic"];

        // Backlog #286 - Positionnement de certains blocs dans un conteneur pour pouvoir les centrer
        switch (block.id) {
            case "text":
                block.set("content", grapesjs.xrmLang[this.language].blockNativeContents["text"]);
                break;
            case "list-items":
                // Backlogs #292 et #323 : on modifie les images du bloc d'origine pour y insérer la nôtre (#292) en leur ajoutant un conteneur pour les rendre éditables via CKEditor (#323)

                // Malheureusement, il semble impossible de modifier le code HTML existant pour y insérer des composants devant être reconnus comme tels par grapesjs (ex : notre image
                // avec conteneur) car dès lors, les composants personnalisés ne sont pas reconnus à la sélection, ainsi que leurs comportements spécifiques (ouverture de CKEditor dans le
                // cas de notre conteneur). Exemple initialement utilisé ci-dessous :
                /*
                var oListblockNativeContentsDoc = eTools.stringToHTMLDocument(block.get("content"));
                var oListBlockImageContainers = oListblockNativeContentsDoc.querySelectorAll("td.list-cell-left");
                for (var i = 0; i < oListBlockImageContainers.length; i++)
                    oListBlockImageContainers[i].innerHTML =
                        "<div class=\"eudonet-extended-container-image eudonet-image eudonet-image-" + this.getNewElementID() + "\" " +
                            "data-gjs-type=\"eudonet-extended-container-image\" " +
                            "data-gjs-removable=\"1\" " +
                            "data-gjs-draggable=\"1\" " +
                            "data-gjs-copyable=\"1\" " +
                            "data-gjs-editable=\"1\" " +
                        ">"
                            + this.getExtendedComponentContents("image")
                        "</div>";
                var oListBlockTitles = oListblockNativeContentsDoc.querySelectorAll("h1.card-title");
                // Backlog #316 - Ressources
                for (var i = 0; i < oListBlockTitles.length; i++)
                    oListBlockTitles[i].innerHTML = grapesjs.xrmLang[this.language].blockNativeContents["list-items-title"];
                var oListBlockSubtexts = oListblockNativeContentsDoc.querySelectorAll("p.card-text");
                for (var i = 0; i < oListBlockSubtexts.length; i++)
                    oListBlockSubtexts[i].innerHTML = grapesjs.xrmLang[this.language].blockNativeContents["list-items-text"];
                // Utilisation du contenu modifié
                block.set("content", oListblockNativeContentsDoc.body.innerHTML);
                */

                // On va donc recréer le code du bloc d'origine sous forme d'une suite de composants grapesjs imbriqués, ce qui permettra de retrouver le comportement grapesjs souhaité
                // sur tout l'ensemble d'éléments insérés, notamment l'édition des images via CKEditor
                // cf. commentaire de l'auteur ici : https://github.com/artf/grapesjs/issues/100#issuecomment-305157472

                // Le bloc list-items du plugin Newsletter consiste en deux tableaux strictement identiques disposés côte-à-côte :
                /*
                <table class="list-item">
                    <tr>
                      <td class="list-item-cell">
                        <table class="list-item-content">
                          <tr class="list-item-row">
                            <td class="list-cell-left">
                              <img class="list-item-image" src="http://placehold.it/150x150/78c5d6/fff/" alt="Image"/>
                            </td>
                            <td class="list-cell-right">
                              <h1 class="card-title">Title here</h1>
                              <p class="card-text">Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt</p>
                            </td>
                          </tr>
                        </table>
                      </td>
                    </tr>
                </table>
                <table class="list-item">
                <tr>
                  <td class="list-item-cell">
                    <table class="list-item-content">
                      <tr class="list-item-row">
                        <td class="list-cell-left">
                          <img class="list-item-image" src="http://placehold.it/150x150/78c5d6/fff/" alt="Image"/>
                        </td>
                        <td class="list-cell-right">
                          <h1 class="card-title">Title here</h1>
                          <p class="card-text">Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt</p>
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>
                </table>
                */

                // On recrée donc cette structure tableau à l'identique sous forme de composants grapesjs
                var imageComponentType = "eudonet-extended-image";
                var listTable =
                {
                    type: "table",
                    classes: ["list-item"],
                    components: [
                        {
                            type: "row",
                            components: [
                                {
                                    type: "cell",
                                    classes: ["list-item-cell"],
                                    components: [
                                        {
                                            type: "table",
                                            classes: ["list-item-content"],
                                            components: [
                                                {
                                                    type: "row",
                                                    classes: ["list-item-row"],
                                                    components: [
                                                        {
                                                            type: "cell",
                                                            classes: ["list-cell-left"],
                                                            components: [
                                                                {
                                                                    tagName: "div",
                                                                    type: imageComponentType,
                                                                    activeOnRender: 1,
                                                                    removable: that.isComponentRemovable(imageComponentType),
                                                                    draggable: that.isComponentDraggable(imageComponentType),
                                                                    droppable: that.isComponentDroppable(imageComponentType),
                                                                    copyable: that.isComponentCopyable(imageComponentType),
                                                                    editable: that.isComponentEditable(imageComponentType),
                                                                    stylable: that.isComponentStylable(imageComponentType),
                                                                    resizable: that.isComponentResizable(imageComponentType),
                                                                    selectable: that.isComponentSelectable(imageComponentType),
                                                                    highlightable: that.isComponentHighlightable(imageComponentType),
                                                                    hoverable: that.isComponentHoverable(imageComponentType),
                                                                    layerable: that.isComponentLayerable(imageComponentType),
                                                                    badgable: that.isComponentBadgable(imageComponentType),
                                                                    classes: ['eudonet-extended-container-image'],
                                                                    childTagName: 'image', // Permet de récupérer le code HTML du composant depuis son init()
                                                                    extendedComponentParameters: {
                                                                        src: "themes/default/images/image_150.png" /* Pour l'image du composant Liste, on utilise les thumbnails de 150 pixels */
                                                                    }
                                                                }
                                                            ]
                                                        },
                                                        {
                                                            type: "cell",
                                                            classes: ["list-cell-right"],
                                                            components: [
                                                                {
                                                                    tagName: "h1",
                                                                    classes: ["card-title"],
                                                                    type: 'text',
                                                                    content: grapesjs.xrmLang[this.language].blockNativeContents["list-items-title"]
                                                                },
                                                                {
                                                                    tagName: "p",
                                                                    classes: ["card-text"],
                                                                    type: 'text',
                                                                    content: grapesjs.xrmLang[this.language].blockNativeContents["list-items-text"]
                                                                },
                                                            ]
                                                        }
                                                    ]
                                                }
                                            ]
                                        }
                                    ]
                                }
                            ]
                        }
                    ]
                };

                // Puis on dispose deux copies de cette structure côte-à-côte, comme le bloc d'origine
                block.set(
                    "content",
                    [
                        listTable, listTable
                    ]
                );
                break;
            case "sect100":
            case "sect50":
                blockCategory = grapesjs.xrmLang[this.language].blockNativeCategories["structures"];
                break;
            case "image":
            case "video":
            case "map":
            case "button":
                var componentType = "eudonet-extended-" + block.id;
                block.set("content",
                    //'<div class="eudonet-extended-container-' + block.id + '">' + this.getExtendedComponentContents(block.id) + '</div>'
                    {
                        tagName: "div",
                        type: componentType,
                        activeOnRender: 1,
                        removable: that.isComponentRemovable(componentType),
                        draggable: that.isComponentDraggable(componentType),
                        droppable: that.isComponentDroppable(componentType),
                        copyable: that.isComponentCopyable(componentType),
                        editable: that.isComponentEditable(componentType),
                        stylable: that.isComponentStylable(componentType),
                        resizable: that.isComponentResizable(componentType),
                        selectable: that.isComponentSelectable(componentType),
                        highlightable: that.isComponentHighlightable(componentType),
                        hoverable: that.isComponentHoverable(componentType),
                        layerable: that.isComponentLayerable(componentType),
                        badgable: that.isComponentBadgable(componentType),
                        classes: ['eudonet-extended-container-' + block.id],
                        childTagName: block.id, // Permet de récupérer le code HTML du composant depuis son init()
                    }
                );
                break;
        }


        block.set("category", blockCategory);

        return block;
    };

    // Backlog #286 - Renvoie le tag HTML correspondant à l'élément principal d'un composant/bloc étendu
    this.getExtendedComponentElementTag = function (id) {
        var correspondingTag = id;
        switch (id) {
            case "image": correspondingTag = "img"; break;
            case "button": correspondingTag = "a"; break;
            case "map": correspondingTag = "iframe"; break;
            case "video": correspondingTag = "video"; break;
            // Backlog #379 - Conteneurs de ces éléments
            case "eudonet-extended-container-image":
            case "eudonet-extended-container-button":
            case "eudonet-extended-container-map":
            case "eudonet-extended-container-video":
                correspondingTag = "div";
                break;
        };
        return correspondingTag;
    };

    // Backlog #286 - Renvoie le tag HTML correspondant à l'élément principal d'un composant/bloc étendu
    this.getExtendedComponentTypeFromTag = function (tagName) {
        tagName = tagName.toLowerCase();
        var correspondingType = tagName;
        switch (tagName) {
            case "img": correspondingType = "image"; break;
            case "a": correspondingType = "button"; break;
            case "iframe": correspondingType = "map"; break;
            case "video": correspondingType = "video"; break;
        };
        return correspondingType;
    };


    this.getExtendedComponentContents = function (id, extendedComponentAdditionalParameters) {
        var newID = this.getNewElementID(); // Backlog #286, #358 - Ajout d'une classe spécifique à chaque nouvelle instance de composant, pour que l'utilisateur puisse surcharger ses styles
        var tagName = null;
        if (extendedComponentAdditionalParameters)
            tagName = extendedComponentAdditionalParameters.tagName;
        if (!tagName)
            tagName = this.getExtendedComponentElementTag(id);

        switch (id) {
            case "image":
                if (!extendedComponentAdditionalParameters)
                    // Backlog #360 - Prise en charge d'un alt renseigné via extendedComponentAdditionalParameters
                    extendedComponentAdditionalParameters = {
                        "data-gjs-highlightable": this.isComponentHighlightable("eudonet-ImageEudo"),
                        src: "themes/default/images/image.png",
                        alt: grapesjs.xrmLang[this.language].blockNativeLabels["image"]
                    };
                break;
            case "video":
                // Backlog #360 - Prise en charge d'un alt renseigné via extendedComponentAdditionalParameters
                if (!extendedComponentAdditionalParameters)
                    extendedComponentAdditionalParameters = {
                        allowfullscreen: "allowfullscreen",
                        src: "http://techslides.com/demos/sample-videos/small.webm",
                        controls: "controls"
                    };
                break;
            case "map":
                // Backlog #360 - Prise en charge d'un alt renseigné via extendedComponentAdditionalParameters
                if (!extendedComponentAdditionalParameters)
                    extendedComponentAdditionalParameters = {
                        frameborder: "0",
                        src: "https://maps.google.com/maps?&z=1&t=q&output=embed",
                    };
                break;
            case "button":
                if (!extendedComponentAdditionalParameters) {
                    // Backlog #618 - On n'affecte pas d'ID à l'élément si l'option forceClass de grapesjs est à false, car cette option ajoute alors elle-même un ID
                    var elementID = "";
                    if (this.htmlTemplateEditor.getConfig().forceClass) {
                        elementID = ' id="' + newID + '" ';
                    }
                    // Backlog #360 - Prise en charge d'un alt renseigné via extendedComponentAdditionalParameters
                    extendedComponentAdditionalParameters = {
                        "data-gjs-highlightable": this.isComponentHighlightable("eudonet-extended-button"),
                        // Backlog #355 - Le texte du bouton est matérialisé par une balise <div> en contenteditable=true, comme les blocs Texte de grapesjs, pour pouvoir être directement éditable via CKEditor
                        content: '<div ' + elementID + ' class="eudonet-extended-button-text" data-gjs-highlightable="' + this.isComponentHighlightable("eudonet-extended-button-text") + '" contenteditable="true">' + grapesjs.xrmLang[this.language].blockNativeLabels["button"] + '</div>'
                    };
                }
                break;
        }

        // Balise ouvrante avec ID, classes
        // Backlog #618 - On n'affecte pas d'ID à l'élément si l'option forceClass de grapesjs est à false, car cette option ajoute alors elle-même un ID
        var elementID = "";
        if (this.htmlTemplateEditor.getConfig().forceClass) {
            elementID = ' id="' + newID + '" ';
        }
        var contents = '<' + tagName + elementID + ' class="' + id + ' eudonet-extended-' + id + ' eudonet-extended-' + id + '-' + newID + '"';
        // Attributs personnalisés
        for (var key in extendedComponentAdditionalParameters) {
            // le paramètre "content" est réservé au contenu à l'intérieur de la balise (innerHTML)
            if (key != "content") {
                if (typeof (extendedComponentAdditionalParameters[key]) == "string")
                    contents += ' ' + key + '="' + extendedComponentAdditionalParameters[key] + '"';
                else {
                    var parameterValue = "";
                    for (var subKey in extendedComponentAdditionalParameters[key]) {
                        parameterValue += subKey + ": " + extendedComponentAdditionalParameters[key][subKey] + "; ";
                    }
                    contents += ' ' + key + '="' + parameterValue + '"';
                }
            }
        }
        // Balise fermante
        var innerHTML = null;
        if (extendedComponentAdditionalParameters)
            innerHTML = extendedComponentAdditionalParameters.content;
        if (!innerHTML)
            innerHTML = '';
        contents += '>' + innerHTML + '</' + tagName + '>';

        return contents;
    };

    // Renvoie un nouvel ID affectable à un élément HTML présent dans le canevas, en vérifiant au préalable qu'il ne soit pas utilisé
    this.getNewElementID = function (referenceNode, referenceDocument) {
        var newID = null;
        if (!referenceDocument)
            referenceDocument = this.getMemoDocument();
        while (!newID || referenceDocument.getElementById(newID) != null)
            newID = "c" + Math.floor(Math.random() * Math.floor(9999));
        return newID;
    };

    // Backlog #329 - Indique si grapesjs est en mode Prévisualisation ou non
    // grapesjs n'offrant pas encore de moyen d'identifier ce mode (au 13/12/2018, cf. commande Preview.js), on se base sur la visibilité de l'élément matérialisant ce mode
    this.isInPreviewMode = function () {
        var previewModeElt = document.getElementById("grapesjs-preview-off-eudonet") || document.querySelector('.gjs-off-prv');
        return previewModeElt && previewModeElt.style.display != "none";
    };

    //Tâche #2 708: selon le type de l'élément sélectionné, on affiche/masque le panel 'Settings' à droite
    //Si le descid du champ de fusion est renseigné, on actualise les infos de fusion dans le menu 'Settings'
    this.showHideSettingsElement = function (show, model) {
        var that = this;
        var settingsElt = document.getElementById(".fa-cog") || document.querySelector('.fa-cog');
        if (settingsElt) {
            if (show) {
                settingsElt.style.display = "block";
                settingsElt.click();
                var selectElem = document.getElementById('mergeFieldSelect');
                if (model && (model.view.$el.hasClass('checkbox') || (model.collection.parent && (model.collection.parent.view.$el.hasClass('champSaisie-memo')
                    || model.collection.parent.view.$el.hasClass('champSaisie-date'))) || model.view.$el.hasClass('champSaisie-date')
                    || (model.view.attr && model.view.attr.type && model.view.attr.type == 'checkbox') || model.view.$el.hasClass('image')
                    || model.view.$el.hasClass('eudonet-extended-cat'))) {
                    setTimeout(function () {
                        var settings = document.querySelector('.gjs-trt-traits');
                        var checkboxTrait = settings.querySelector('.gjs-field-checkbox');

                        //Tâche #2 926: On gère la propriété cochée par défaut en chargement, ça doit être fait par grapesJS
                        if (model.view.attr && model.view.attr.type && model.view.attr.type == 'checkbox' && model.view.attr.checked) {
                            var checkboxTrait = settings.querySelectorAll('input[type="checkbox"]');
                            if (checkboxTrait && checkboxTrait.length > 1) {
                                checkboxTrait[1].checked = true;
                            }
                        }
                    }, 100);
                }

                if (model && model.view.el.tagName == 'EXTENDED-WORLDLINE-BTN') {
                    var amountAttr = model.view.el.getAttribute('edndpa');
                    var transactionAttr = model.view.el.getAttribute('edndtr');
                    var indicatorAttr = model.view.el.getAttribute('edndpi');
                    if (amountAttr && amountAttr != '') {
                        setTimeout(function () {
                            //amountAttr = '[[' + amountAttr + ']]';
                            document.getElementById('amountFieldSelect').querySelector('[value="' + decodeHTMLEntities(amountAttr) + '"]').setAttribute('selected', true);                           
                        }, 100);
                    }
                    if (transactionAttr && transactionAttr != '') {
                        setTimeout(function () {
                            //transactionAttr = '[[' + transactionAttr + ']]';
                            document.getElementById('transactionReferenceField').querySelector('[value="' + decodeHTMLEntities(transactionAttr) + '"]').setAttribute('selected', true);
                        }, 100);
                    }
                    if (indicatorAttr && indicatorAttr != '') {
                        setTimeout(function () {
                            //indicatorAttr = '[[' + indicatorAttr + ']]';
                            document.getElementById('paymentIndicatorField').querySelector('[value="' + decodeHTMLEntities(indicatorAttr) + '"]').setAttribute('selected', true);
                        }, 100);
                    }
                }

                //On positionne la liste de champs de fusion, selon l'attribut descid s'il est renseigné
                var descIdAttr;
                if (model && model.collection.parent && model.collection.parent.view.el.tagName == 'EUDONET-EXTENDED-INPUT')
                    descIdAttr = model.collection.parent.view.el.getAttribute('ednd');
                else if (model && model.view.el.tagName == 'A')
                    descIdAttr = model.view.el.getAttribute('ednl');

                if (descIdAttr && descIdAttr != '') {
                    var strMergeField = eTools.getTextMergeFieldFromDescId(JSON.parse(this.mergeFields), descIdAttr);
                    if (strMergeField && strMergeField != '') {
                        setTimeout(function () {
                            strMergeField = '[[' + strMergeField + ']]';
                            document.getElementById('mergeFieldSelect').querySelector('[value="' + decodeHTMLEntities(strMergeField) + '"]').setAttribute('selected', true);
                            //selectElem.onchange();//on applique le même traitement du choix d'un champ de fusion lors du chargement //Tâche #2 923: lors du chargement, on selectionne la rubrique associée mais on n'écrase pas la valeur de l'attribut 'Obligatoire'
                        }, 100);
                    }
                }
            }
            else
                settingsElt.style.display = "none";
        }
    };

    // When `true` the component is removable from the canvas, default: `true`
    this.isComponentRemovable = function (componentType, tagName, showIcon) {
        switch (componentType) {
            case "link":
                return showIcon;
            default:
                return true;
        }
    };

    // Indicates if it's possible to drag the component inside others.
    // You can also specify a query string to indentify elements,
    // eg. `'.some-class[title=Hello], [data-gjs-type=column]'` means you can drag the component only inside elements
    // containing`some-class` class and `Hello` title, and`column` components. Default: `true`
    this.isComponentDraggable = function (componentType, tagName, showIcon) {
        switch (componentType) {
            case "link":
                return showIcon;
            default:
                return true;
        }
    };

    // Indicates if it's possible to drop other components inside. You can use
    // a query string as with `draggable`. Default: `true`
    this.isComponentDroppable = function (componentType, tagName) {
        switch (componentType) {
            case "eudonet-extended-image":
                return false;
            default:// tâche 2 459: on ne peut pas déposer un composant de type canvas dans un canvas
                return '.Col1,.Col2,.simpleText,.Col3,.formCanvas,.image,' + '.list-item, eudonet-extended-container-text, eudonet-extended-container-image, eudonet-extended-container-video, eudonet-extended-container-map, eudonet-extended-container-button, eudonet-extended-container-divider, eudonet-extended-container-list-items, eudonet-extended-container-sect100, eudonet-extended-container-sect50';
        }
    };

    // True if it's possible to clone the component. Default: `true`
    this.isComponentCopyable = function (componentType, tagName, showIcon) {
        switch (componentType) {
            case "link":
                return showIcon;
            default:
                return true;
        }
    };


    // Allow to edit the content of the component (used on Text components). Default: `false`
    this.isComponentEditable = function (componentType, tagName) {
        switch (componentType) {
            case "image":
            case "link":
                return false;
                break;
            default:
                return true;
        }
    };

    // True if it's possible to style the component.
    // You can also indicate an array of CSS properties which is possible to style, eg. `['color', 'width']`, all other properties
    // will be hidden from the style manager. Default: `true`
    this.isComponentStylable = function (componentType, tagName) {
        switch (componentType) { default: return true; }
    };

    // Indicates if it's possible to resize the component.
    // It's also possible to pass an object as [options for the Resizer](https://github.com/artf/grapesjs/blob/master/src/utils/Resizer.js). Default: `false`
    this.isComponentResizable = function (componentType, tagName) {
        switch (componentType) {
            // Backlog #503 - Pas de redimensionnement sur les cellules de tableau, non interprété sur le mail envoyé
            // Backlog #579 - Pas de redimensionnement pour les textes et images non plus. Donc, pas de redimensionnement pour beaucoup de choses, en fait
            default:
                return false;
        }
    };

    // Allow component to be selected when clicked. Default: `true`
    this.isComponentSelectable = function (componentType, tagName, showSettings) {
        switch (componentType) {
            case "link":
                return showSettings;
            default:
                return true;
        }
    };

    // Shows a highlight outline when hovering on the element if `true`. Default: `true`
    this.isComponentHighlightable = function (componentType, tagName, showIcon) {
        switch (componentType) {
            case "link":
                return showIcon;
            default:
                /* Backlog #582 - Si on analyse une balise HTML considérée comme conteneur de texte (strong, p...) mais potentiellement non éditable par grapesjs, on ne la
                 * matérialise pas comme composant dans grapesjs (mais on lui laisse toutes ses autres caractéristiques pour permettre, notamment, son édition comme Texte) */
                return this.getNonNativeEditableElementTagNames().indexOf(tagName) == -1;
        }
    };

    // Shows a highlight outline when hovering on the element if `true`. Default: `true`
    this.isComponentHoverable = function (componentType, tagName) {
        switch (componentType) {
            case "link":
                return false;
            default:
                return true;
        }
    };

    // Set to `false` if you need to hide the component inside Layers. Default: `true`
    this.isComponentLayerable = function (componentType, tagName) {
        switch (componentType) {
            default:
                return true;
        }
    };

    // Set to false if you don't want to see the badge (with the name) over the component. Default: `true`
    this.isComponentBadgable = function (componentType, tagName) {
        switch (componentType) {
            case "link":
                return false;
            default:
                return true;
        }
    };

    // Backlog #457 - Mise à jour du badge permanent
    this.updatePermanentBadgeDisplay = function (display, currentContext, model) {
        if (!this.permanentBadgeEnabled)
            return;

        // Affichage
        if (display) {

            // On détermine l'élément servant de référent pour le positionnement du badge permanent
            var currentBadgeElement = currentContext.getEditor().Canvas.getBadgeEl();
            var currentToolbarElement = currentContext.getEditor().Canvas.getToolbarEl();
            var selectedElement = currentContext.getEditor().getSelected() || model;
            var referenceElement = null;
            var targetTop = "0px";
            var targetLeft = "0px";
            // Si le badge n'est pas affiché faute de place, on se positionne par rapport à la toolbar qui est affichée prioritairement par rapport à grapesjs
            if (!currentBadgeElement || currentBadgeElement.style.display == "none" || currentBadgeElement.style.display == "") {
                referenceElement = currentToolbarElement;
                targetTop = (getAbsolutePosition(selectedElement.getEl()).y - 18) + "px"; // 18 : hauteur du badge (14 pixels + 2 de padding haut & bas)
                targetLeft = getAbsolutePosition(selectedElement.getEl()).x + "px";
            }
            // Sinon, on positionne le badge permanent au même endroit que le badge classique (il passe par-dessus)
            else {
                referenceElement = currentBadgeElement;
                targetTop = currentBadgeElement.style.top;
                targetLeft = currentBadgeElement.style.left;
            }

            // Puis on agit
            if (referenceElement && selectedElement) {
                if (referenceElement.style.display != "none") {
                    // Création de l'élément Badge permanent dans le DOM si inexistant
                    if (!that.permanentBadgeElement) {
                        that.permanentBadgeElement = currentBadgeElement.cloneNode();
                        that.permanentBadgeElement.id = 'permanentBadge_' + that.name;
                        currentBadgeElement.parentElement.appendChild(that.permanentBadgeElement);
                    }

                    // Définition du libellé : on récupère la ressource liée au type d'élément sélectionné
                    var selectedItemType = selectedElement.get("type");
                    if (!selectedItemType || selectedItemType.trim().length == 0)
                        selectedItemType = selectedElement.get("tagName"); // cas de P, SPAN...
                    var permanentBadgeLabel = grapesjs.xrmLang[this.language].blockNativeLabels[selectedItemType];
                    if (!permanentBadgeLabel) {
                        if (selectedItemType && selectedItemType.length > 1) {
                            permanentBadgeLabel = selectedItemType.charAt(0).toUpperCase() + selectedItemType.slice(1);
                        }
                    }

                    // Paramétrage du badge
                    that.permanentBadgeElement.innerHTML = permanentBadgeLabel;
                    that.permanentBadgeElement.style.top = targetTop;
                    that.permanentBadgeElement.style.left = targetLeft;
                    that.permanentBadgeElement.style.display = display ? 'block' : 'none';
                    addClass(that.permanentBadgeElement, 'gjs-badge-permanent');
                }
                else
                    this.updatePermanentBadgeDisplay(false, currentContext, model);
            }
            else
                this.updatePermanentBadgeDisplay(false, currentContext, model);
        }
        // Masquage
        else if (that.permanentBadgeElement) {
            that.permanentBadgeElement.style.display = "none";
        }
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    // METHODES UTILITAIRES
    // Sauf mention contraire, à utiliser en interne uniquement
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------

    // Génère des messages de diagnostic ; uniquement si this.debugMode = true
    this.trace = function (strMessage) {
        if (this.debugMode) {
            try {
                strMessage = new Date() + ' - ' + this.constructor.name + ' [' + this.name + '] -- ' + strMessage;

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

    //Tâche #2 749:fonction qui permet le dévérouille sur les éléments mergefield
    function unlockEditableElement(elementHtml) {
        if (elementHtml.tagName == 'LABEL')
            elementHtml = elementHtml.parentElement;
        var sources = elementHtml.querySelectorAll("label");

        for (var j = 0; j < sources.length; j++) {
            var source = sources[j].getAttribute("data-gjs-type");
            if (source && source === 'mergefield') {
                sources[j].removeAttribute("contenteditable");
            }
        }
    }

    //Tâche #2 749: fonction qui permet le verouille sur les mergefield avec contenteditable='false'
    function lockEditableElement(elementHtml, updateTextEditorElem) {
        if (elementHtml.tagName == 'LABEL')
            elementHtml = elementHtml.parentElement;
        var sources = elementHtml.querySelectorAll("label");

        for (var j = 0; j < sources.length; j++) {
            var source = sources[j].getAttribute("data-gjs-type");
            if (source && source === 'mergefield') {
                sources[j].setAttribute("contenteditable", "false");
                if (updateTextEditorElem) {
                    if (sources[j].outerHTML.indexOf('<b>') > 0 || sources[j].outerHTML.indexOf('<b class="" contenteditable="false">') > 0)
                        sources[j].outerHTML = '<b>' + sources[j].outerHTML.replace('<b>', '').replace('<b class="" contenteditable="false">', '').replace('</b>', '') + '</b>';
                    if (sources[j].outerHTML.indexOf('<u>') > 0 || sources[j].outerHTML.indexOf('<u class="" contenteditable="false">') > 0)
                        sources[j].outerHTML = '<u>' + sources[j].outerHTML.replace('<u>', '').replace('<u class="" contenteditable="false">', '').replace('</u>', '') + '</u>';
                    if (sources[j].outerHTML.indexOf('<i>') > 0 || sources[j].outerHTML.indexOf('<i class="" contenteditable="false">') > 0)
                        sources[j].outerHTML = '<i>' + sources[j].outerHTML.replace('<i>', '').replace('<i class="" contenteditable="false">', '').replace('</i>', '') + '</i>';
                    if (sources[j].outerHTML.indexOf('<strike>') > 0 || sources[j].outerHTML.indexOf('<strike class="" contenteditable="false">') > 0)
                        sources[j].outerHTML = '<strike>' + sources[j].outerHTML.replace('<strike>', '').replace('<strike class="" contenteditable="false">', '').replace('</strike>', '') + '</strike>';
                    if (sources[j].outerHTML.indexOf('<a>') > 0 || sources[j].outerHTML.indexOf('<a class="" contenteditable="false">') > 0)
                        sources[j].outerHTML = '<a>' + sources[j].outerHTML.replace('<a>', '').replace('<i class="" contenteditable="false">', '').replace('</i>', '') + '</i>';
                }
            }
        }
    }

    this.ObjMergeFields = function () {

        var obj = {}
        var _MergeField = JSON.parse(this.mergeFields);
        for (var i in _MergeField) {
            if (typeof i != "string")
                continue;

            if (_MergeField[i] == null)
                continue;
            //On récupère la tabId du Desc depuis eTools ainsi que l'id du field dans la table
            var _descId = _MergeField[i].split(';')[0];
            var _descTabId = getTabDescid(_descId);
            var format = _MergeField[i].split(';')[3]
            obj[_descId] = {
                label: i,
                tab: _descTabId,
                format: format
            }
        }
    }

    //Tâche #2 708: On construit la liste déroulante des champs de fusion à partir du json
    this.addGetMergeFieldsSelectTemplate = function (mergeFieldParams) {
        var strXrmMergeField = '';
        var currentGroup = '';
        var firstElement = top._res_6484;//Première option
        //on récupère le json des champs de fusion
        var _MergeField = JSON.parse(this.mergeFields);
        //on récupére la liste des champs de fusion déjà aossociés à d'autres champs de saisie
        var descIdsAssigned = [];
        if (mergeFieldParams && mergeFieldParams.isExtendedInput) {
            var extendedComponents = this.htmlTemplateEditor.DomComponents.getWrapper().view.el.querySelectorAll('eudonet-extended-input');
            extendedComponents.forEach(function (item) {
                var descIdAttr = item.getAttribute('ednd');
                if (descIdAttr)
                    descIdsAssigned.push(descIdAttr);
            });
        }

        for (var i in _MergeField) {
            if (typeof i != "string")
                continue;

            if (_MergeField[i] == null)
                continue;
            //On récupère la tabId du Desc depuis eTools ainsi que l'id du field dans la table
            var _mergedFieldArrayInfos = _MergeField[i].split(';');
            var _descId = _mergedFieldArrayInfos[0];
            var _descIdInTable = getDescid(_descId);
            var _descTabId = getTabDescid(_descId);
            //on ne charge que les champs de fusion avec un type spécifié si le type est renseigné
            if (mergeFieldParams && mergeFieldParams.fieldFormat) {//Tâche #2 828
                firstElement = top._res_6211;
                if (mergeFieldParams.fieldFormat.indexOf(parseInt(_mergedFieldArrayInfos[3])) < 0 || (mergeFieldParams.isExtendedInput
                    && (!mergeFieldParams.wordlinePaimentActivated || (mergeFieldParams.wordlinePaimentActivated && mergeFieldParams.fieldFormat.indexOf(eTools.FieldFormat.TYP_NUMERIC) < 0))
                    && (_descTabId == eTools.DescIdEudoModel.TBL_PP || _descTabId == eTools.DescIdEudoModel.TBL_PM || _descTabId == eTools.DescIdEudoModel.TBL_Adress || _descTabId != this.nTabFrom)))
                    continue;

                if (descIdsAssigned.indexOf(_descId) > -1 && !mergeFieldParams.wordlinePaimentActivated && (!mergeFieldParams.descIdAssigned || (mergeFieldParams.descIdAssigned != _descId)))
                    continue;

                if (_mergedFieldArrayInfos[10] == "1")//ne pas autoriser les rubriques de type html
                    continue;

                if (_mergedFieldArrayInfos[5] == "2" || (mergeFieldParams.isPopup && (_mergedFieldArrayInfos[5] != "3" || _mergedFieldArrayInfos[7] != "0"))
                    || (!mergeFieldParams.isPopup && _mergedFieldArrayInfos[5] == "3"))//si c'est un filed de type popup, on retire les catalogues liés en V1
                    continue;

                if ((!mergeFieldParams.isMultiple && _mergedFieldArrayInfos[4] == "True") || (mergeFieldParams.isMultiple && _mergedFieldArrayInfos[4] != "True"))//si c'est un filed non multiple
                    continue;

                if (mergeFieldParams && mergeFieldParams.wordlinePaimentActivated && mergeFieldParams.fieldFormat[0] == 1 && parseInt(_mergedFieldArrayInfos[11]) < 35)//Rubrique Référence Transaction : Liste déroulante des rubriques caractères d'au moins 35 caractères
                    continue;
            }

            //Tâche #2 934: on ignore certaines rubriques qui servent à autres choses (planning par exemple)
            if (mergeFieldParams && mergeFieldParams.ignoredFields && mergeFieldParams.ignoredFields.indexOf(_descIdInTable) > -1)
                continue;

            //On regroupe par type de table
            if ((i.indexOf('.') > 0) && (i.substring(0, i.indexOf('.')) != currentGroup)) {
                currentGroup = i.substring(0, i.indexOf('.'));
                if (mergeFieldParams && mergeFieldParams.wordlinePaimentActivated)
                    strXrmMergeField += '<option class="mergefieldGroup" disabled">' + currentGroup + '</option>';
                else
                    strXrmMergeField += '<option class="mergefieldGroup" disabled value="[[' + currentGroup + ']]">' + currentGroup + '</option>';
            }



            //on créé l'option du champ de fusion
          strXrmMergeField += '<option class="xrmMergeFields"  edescid="' + _descId + '" value="' + (mergeFieldParams && mergeFieldParams.wordlinePaimentActivated ? _descId : '[['+i+']]') + '">' + i + '</option>';
        }

        return '<select class="gjs-field custom-mergefields">' +
            '<option value="">' + firstElement + '</option>' +
            strXrmMergeField +
            '</select>';
    }

    //On ajoute la liste des champs de fusion lors de l'édition d'un bloc de type texte
    this.addMergeFieldsToHtmlTemplateEditor = function () {
        var rte = this.htmlTemplateEditor.RichTextEditor;

        //on récupère la fonction result du lien pour l'utiliser après
        if (rte.get('link'))
            linkResult = rte.get('link').result;
        //on vire les options existantes
        rte.remove('bold');
        rte.remove('italic');
        rte.remove('underline');
        rte.remove('strikethrough');
        rte.remove('link');
        rte.remove('custom-mergefields');
        rte.remove('toggleAnchor');


        //on rédéfinit les actions:bold,italic... pour prendre en compte les éléments de type contenteditable=false
        var action = rte.get('bold');
        if (!action) {
            rte.add('bold', {
                icon: '<b>B</b>',
                attributes: { title: 'Bold', class: 'gjs-rte-action gjs-rte-action-default' },
                result: function (rte) {
                    unlockEditableElement(rte.el);
                    rte.exec('bold');
                    lockEditableElement(rte.el, true);
                }
            });
        }

        action = rte.get('italic');
        if (!action) {
            rte.add('italic', {
                icon: '<i>I</i>',
                attributes: { title: 'Italic', class: 'gjs-rte-action gjs-rte-action-default' },
                result: function (rte) {
                    unlockEditableElement(rte.el);
                    rte.exec('italic');
                    lockEditableElement(rte.el, true);
                }
            });
        }

        action = rte.get('underline');
        if (!action) {
            rte.add('underline', {
                icon: '<u>U</u>',
                attributes: { title: 'underline', class: 'gjs-rte-action gjs-rte-action-default' },
                result: function (rte) {
                    unlockEditableElement(rte.el);
                    rte.exec('underline');
                    lockEditableElement(rte.el, true);
                }
            });
        }

        action = rte.get('strikethrough');
        if (!action) {
            rte.add('strikethrough', {
                icon: '<strike>S</strike>',
                attributes: { title: 'strikethrough', class: 'gjs-rte-action gjs-rte-action-default' },
                result: function (rte) {
                    unlockEditableElement(rte.el);
                    rte.exec('strikeThrough');
                    lockEditableElement(rte.el, true);
                }
            });
        }

        action = rte.get('link');
        if (!action) {
            rte.add('toggleAnchor', {
                attributes: { title: 'link', class: 'gjs-rte-action gjs-rte-action-default' },
                icon: '<span style="transform:rotate(45deg)">&supdsub;</span>',
                result: linkResult
            })
        }

        //Tâche #2 717
        //https://github.com/artf/grapesjs/issues/481
        //On construit un élément custom dans RichTextEditor
        this.htmlTemplateEditor.RichTextEditor.add('custom-mergefields', {
            icon: that.addGetMergeFieldsSelectTemplate(),
            result: function (rte, action) {
                var elem = action.btn.firstChild;
                //On n'insère pas un élément de type regroupement dans le html
                if (!elem.options[elem.selectedIndex].classList.contains('mergefieldGroup')) {
                    //tache #2 593 injection des champs de fusion dans le bloc de texte
                    //Tâche #2 749: on verouille/déverouille les élements de type contenteditable='false' pour permmettre l'insertion pour eviter d'ajouter des espaces entre les champs de fusion
                    unlockEditableElement(rte.el);
                    that.insertMergeField(rte, action.btn.firstChild.value, 'R');
                    lockEditableElement(rte.el, true);
                }

            },
            // Reset the select on change
            update: function (rte, action) {
                action.btn.firstChild.value = "";
                lockEditableElement(rte.el, false);
            }
        })
    }
    // ------------------------------------------------------------------------
    // Constructeur
    // ------------------------------------------------------------------------

    this.trace("Instanciation d'un nouvel objet eGrapesJSEditor");

    switch (top._userLangId) {
        case 1:
            this.language = 'en';
            break;
        case 2:
            this.language = 'de';
            break;
        case 3:
            this.language = 'nl';
            break;
        case 4:
            this.language = 'es';
            break;
        case 5:
            this.language = 'it';
            break;
        case 6:
        case 7:
        case 8:
        case 9:
        case 10:
        case 0:
        default:
            this.language = 'fr';
            break;
    }

    this.htmlConfig = {
        width: '100%',
        height: '100%'
    };
    // Renvoie la configuration de l'instance
    this.config = this.isHTML ? this.htmlConfig : this.textConfig;
}

//on définit la méthode de prototype loadExtraPlugins qui fait rien dans le cadre de Grape
eGrapesJSEditor.prototype.loadExtraPlugins = function () {
    return;
}

//on définit une méthode abstraite pour la customisation du type image qui sera redefinie dans eMemoEditor
eGrapesJSEditor.prototype.addCustomImageComponentType = function (that) {
}

//on défini une méthode abstraite pour l'ajout d'un type 'mergefield' dans grapesJS
eGrapesJSEditor.prototype.addMergefieldComponentType = function (that) {
    if (that === undefined)
        that = this;
    var componentType = "mergefield";
    var baseTypeCanevas = this.htmlTemplateEditor.DomComponents.getType('eudonet-extended-canvas');
    if (baseTypeCanevas) {
        this.htmlTemplateEditor.DomComponents.addType(componentType, {
            model: baseTypeCanevas.model.extend(
                {
                    defaults: Object.assign({}, baseTypeCanevas.model.prototype.defaults, {
                        name: '', // Nom affiché sur le contour du composant
                        removable: false,
                        draggable: false,
                        droppable: false,
                        badgable: false,
                        stylable: false,
                        highlightable: false,
                        copyable: false,
                        resizable: false,
                        editable: false,
                        selectable: false,
                        hoverable: false
                    })
                }
            ),
            view: baseTypeCanevas.view.extend({
                events: {
                    mouseover: function (e) {
                        e.target.className = "";
                        e.target.setAttribute("contenteditable", "false");
                        setTimeout(function () {
                            e.target.className = "";
                        }, 100)
                        e.preventDefault();
                    },
                    focus: function (e) {
                        e.target.className = "";
                        e.target.setAttribute("contenteditable", "false");
                        setTimeout(function () {
                            e.target.className = "";
                        }, 100);
                        e.preventDefault();
                    },
                    click: function (e) {
                        e.preventDefault();
                        e.target.setAttribute("contenteditable", "false");
                        eTools.setCurrentCursorPosition(that.getMemoFrame().contentWindow, e.target.parentElement, 0);
                        e.target.className = "";
                        setTimeout(function () {
                            e.target.className = "";
                        }, 100)
                    },
                    dblclick: function () {
                        return false;
                    }
                },
            })
        });
    }

    //Tâche #2 708: on ajoute un type custom dans grapesJs pour le bloc champ de saisie 
    var componentTypeInput = "eudonet-extended-input";
    var baseTypeCanevas = this.htmlTemplateEditor.DomComponents.getType('eudonet-extended-canvas');
    if (baseTypeCanevas) {
        this.htmlTemplateEditor.DomComponents.addType(componentTypeInput, {
            model: baseTypeCanevas.model.extend(
                {
                    defaults: Object.assign({}, baseTypeCanevas.model.prototype.defaults, {
                        removable: true,
                        draggable: true,
                        droppable: false,
                        badgable: true,
                        stylable: true,
                        highlightable: true,
                        copyable: true,
                        resizable: true,
                        editable: true,
                        selectable: true,
                        hoverable: true
                    })
                }
            ),
            view: baseTypeCanevas.view.extend({
                events: {
                    click: function (e) {
                        if (e.target.tagName == "INPUT")
                            that.showHideSettingsElement(true, this.model);
                    },
                    dblclick: function () {
                        return false;
                    }
                },
            })
        });
    }

    this.htmlTemplateEditor.DomComponents.addType('button-worldline', {
        isComponent(el) {
            if (el.tagName === 'EXTENDED-WORLDLINE-BTN') {
                return {
                    type: 'button-worldline',
                    content: el.textContent
                };
            }
        },
        model: {
            defaults: {
                tagName: 'extended-worldline-btn',
                copyable: false,
               // draggable: 'struct, formCanvas',
                droppable: false, // Can't drop other elements inside
                traits: [
                    {
                        type: 'content',
                        label: top._res_223
                    },
                    {
                        type: 'worldline-paiment-amount',
                        name: 'edndpa',
                        label: top._res_8774
                    },
                    {
                        type: 'transaction-reference-field',
                        name: 'edndtr',
                        label: top._res_8775
                    }, 
                    {
                       type: 'payment-indicator-field',
                       name: 'edndpi',
                       label: top._res_8776
                    }
                ],
            }
        },
        view: {
            events: {
                click: function (e) {
                    that.showHideSettingsElement(true, this.model);
                },
                dblclick: function () {
                    return false;
                }
            },
        }
    });
    var componentType = "eudonet-extended-select";
    var baseTypeCanevas = this.htmlTemplateEditor.DomComponents.getType('eudonet-extended-canvas');
    if (baseTypeCanevas) {
        this.htmlTemplateEditor.DomComponents.addType(componentType, {
            model: baseTypeCanevas.model.extend(
                {
                    defaults: Object.assign({}, baseTypeCanevas.model.prototype.defaults, {
                        removable: true,
                        draggable: true,
                        droppable: false,
                        badgable: true,
                        stylable: true,
                        highlightable: true,
                        copyable: true,
                        resizable: true,
                        editable: true,
                        selectable: true,
                        hoverable: true
                    })
                }
            ),
            view: baseTypeCanevas.view.extend({
                events: {
                    click: function (e) {
                        that.showHideSettingsElement(true, this.model);
                    },
                    dblclick: function () {
                        return false;
                    }
                },
            })
        });
    }

    //Tâche #2 708: on affiche le panel 'Settings' que pour les composants de type 'input'
    this.htmlTemplateEditor.on("component:selected", function (model) {

        if (model.getClasses().indexOf('eudonet-extended-label') > -1) {
            model.set("droppable", "false");
            var rteElem = document.querySelector('.gjs-rte-toolbar')
            if (rteElem)
                rteElem.style.display = 'none';
        }

        //On affiche les éléments du toolbar du RichTextEditor selon le type de l'élement
        var lstRteElem = document.querySelectorAll('.gjs-rte-action-default');
        if (lstRteElem) {
            for (var i = 0; i < lstRteElem.length; i++) {
                if (model.getClasses().indexOf('eudonet-extended-label') > -1)
                    lstRteElem[i].style.display = 'none';
                else
                    lstRteElem[i].style.display = '';
            }
        }

        if (model.getClasses().indexOf('date') > -1 && !model.getTrait('tminf')) {
            //model.removeTrait('placeholder');
            model.addTrait({
                type: 'checkbox',
                name: 'tminf',
                label: top._res_2696
            });
        }
        else if (model.getClasses().indexOf('memo') > -1 && !model.getTrait('rows'))
            model.addTrait({
                type: 'number',
                name: 'rows',
                min: 1,
                label: top._res_6373
            });
        else if (model.attributes["tagName"] && model.attributes["tagName"].toLowerCase() == "button" && model.view && model.view.$el)
            model.set('content', model.view.$el.text());

        //Tâche #3 223
        var sm = document.getElementsByClassName('gjs-sm-sectors')[0];
        var styleElt = document.getElementById(".fa-paint-brush") || document.querySelector('.fa-paint-brush');
        model.attributes["type"] == "eudonet-extended-input" || model.attributes["type"] == "button" || model.getClasses().indexOf('eudonet-extended-btn') >= 0 || model.attributes["type"] == "button-worldline" ||
            (model.collection.parent && model.collection.parent.attributes["type"] == "eudonet-extended-input") ? sm.style.display = styleElt.style.display = 'none' : sm.style.display = styleElt.style.display = 'block';

        if (model.attributes["type"] == "input" || model.attributes["type"] == "checkbox" || model.attributes["type"] == "button" || model.attributes["type"] == "link"
            || model.attributes["type"] == "image" || model.attributes["type"] == "eudonet-extended-select" || model.attributes["type"] == "button-worldline")
            that.showHideSettingsElement(true, model);
        else if (model.attributes["type"] == "label") {//Tâche #2 823: la partie typographie à droite doit être dépliée pour ce type de composant
            var sector = that.htmlTemplateEditor.StyleManager.getSector('typographie');
            if (sector)
                sector.set('open', true);
        }

        //Ne pas proposer les options de typographies pour les autres éléments que les blocs Textes
        if (model.attributes["type"] != "text") {
            setTimeout(function () {
                var typographySector = document.querySelector('#gjs-sm-typographie');
                if (typographySector)
                    typographySector.style.display = 'none';
            }, 100)
        }

        if (!that.enableAdvancedFormular && model.attributes["tagName"] == "div") {
            if (model.collection.parent && model.collection.parent.attributes["type"] == 'cell')
                model.attributes.droppable = model.attributes.draggable = ".column1,.column2,.column37,.texte,.button,.image,.article,.separator";

            if (model.collection.parent && model.collection.parent.getClasses().indexOf('NonDroppableCell') > -1)
                model.attributes.droppable = false;
        }
    });
    this.htmlTemplateEditor.on("component:deselected", function (model) {
        that.showHideSettingsElement(false);
    });

    //Tâche #2 708: Dans le panel 'Settings', on ajoute un nouveau type de traitement 'MergeField', ça sera une liste déroulante des champs de fusion
    this.htmlTemplateEditor.TraitManager.addType('mergefield-input', {
        getInputEl: function getInputEl() {
            var modelSelect = this;
            var mergeFieldParams = {
                fieldFormat: [],
                ignoredFields: [],
                isPopup: false,
                isMultiple: false,
                isExtendedInput: true,
                descIdAssigned: -1
            }

            //on intialise le descid déjà utilisé
            if (modelSelect.target.collection.parent.view.el.classList.contains('inputCanvas'))
                mergeFieldParams.descIdAssigned = modelSelect.target.collection.parent.view.el.getAttribute('ednd');
            //A partir de la classe du parent, on récupére la liste de champs de fsion selon le type de block (champ de saisie, mail...)
            if (modelSelect.target.collection.parent.view.el.classList.contains('champSaisie')) {
                mergeFieldParams.fieldFormat.push(eTools.FieldFormat.TYP_CHAR);
                mergeFieldParams.ignoredFields.push(eTools.DescIdEudoPlanningField.Alerts_Sound, eTools.DescIdEudoPlanningField.Calendar_Color);
                mergeFieldParams.isPopup = false;
            }
            //US #1 951: on ne charge que les champs de type Mail
            else if (modelSelect.target.collection.parent.view.el.classList.contains('email')) {
                mergeFieldParams.fieldFormat.push(eTools.FieldFormat.TYP_EMAIL);
            }
            //US #1 950: on ne charge que les champs de type Téléphone
            else if (modelSelect.target.collection.parent.view.el.classList.contains('phone')) {
                mergeFieldParams.fieldFormat.push(eTools.FieldFormat.TYP_PHONE);
            }
            //tâche #2 851 : on ne charge que les cases à cocher et boutons
            else if (modelSelect.target.collection.parent.view.el.classList.contains('checkbox')) {
                mergeFieldParams.fieldFormat.push(eTools.FieldFormat.TYP_BIT, eTools.FieldFormat.TYP_BITBUTTON);
                mergeFieldParams.ignoredFields.push(eTools.DescIdEudoPlanningField.Alerts);
            }
            //tâche #2 861 : on ne charge que les types numériques et numéraires
            else if (modelSelect.target.collection.parent.view.el.classList.contains('champSaisie-num')) {
                mergeFieldParams.fieldFormat.push(eTools.FieldFormat.TYP_NUMERIC, eTools.FieldFormat.TYP_MONEY);
                mergeFieldParams.ignoredFields.push(eTools.DescIdEudoPlanningField.Alerts_Hour, eTools.DescIdEudoPlanningField.Periodicity, eTools.DescIdEudoPlanningField.Type);
            }
            else if (modelSelect.target.collection.parent.view.el.classList.contains('champSaisie-date')) {
                mergeFieldParams.fieldFormat.push(eTools.FieldFormat.TYP_DATE);
                mergeFieldParams.ignoredFields.push(eTools.DescIdEudoPlanningField.CreatedOn, eTools.DescIdEudoPlanningField.ModifiedOn);
            }
            else if (modelSelect.target.collection.parent.view.el.classList.contains('champSaisie-memo')) {
                mergeFieldParams.fieldFormat.push(eTools.FieldFormat.TYP_MEMO);
                mergeFieldParams.ignoredFields.push(eTools.DescIdEudoPlanningField.Informatios, eTools.DescIdEudoPlanningField.Notes);
            }
            else if (modelSelect.target.getClasses().indexOf('eudonet-extended-cat') > -1) {
                mergeFieldParams.fieldFormat.push(eTools.FieldFormat.TYP_CHAR);
                mergeFieldParams.ignoredFields.push(eTools.DescIdEudoPlanningField.Alerts_Sound, eTools.DescIdEudoPlanningField.Calendar_Color);
                mergeFieldParams.isPopup = true;
                mergeFieldParams.isMultiple = modelSelect.target.getClasses().indexOf('eudonet-extended-catMultiple') > -1 ? true : false;
            }
            else if (modelSelect.target.attributes.type === 'link') {
                mergeFieldParams.fieldFormat.push(eTools.FieldFormat.TYP_WEB);
                mergeFieldParams.isExtendedInput = false;
                modelSelect.target.collection.parent.view.el.getAttribute('ednd');
            }


            //on récupére le descid du champ s'il est djà renseigné
            var document = eTools.stringToHTMLDocument(that.addGetMergeFieldsSelectTemplate(mergeFieldParams));
            var selectList = document.body.children[0];
            selectList.id = "mergeFieldSelect";
            selectList.onchange = function () {
                if (!selectList.options[selectList.selectedIndex].classList.contains('mergefieldGroup')) {
                    attributes = '{}';
                    modelSelect.target.collection.parent.set('attributes', JSON.parse(attributes));
                    var isLnkField = modelSelect.target.attributes["type"] === 'link';
                    var infoMergeField = that.insertMergeField(selectList, selectList.value, 'R', true, isLnkField);

                    if (infoMergeField) {
                        attributes = '{' + infoMergeField.attributes + '}';
                        
                        if (isLnkField) {
                            //on écrase pas les attributs existants
                            const mergedObject = {
                                ...modelSelect.target.attributes.attributes,
                                ...JSON.parse(attributes)
                            };
                            modelSelect.target.set('attributes', mergedObject);
                        }
                        else
                            modelSelect.target.collection.parent.set('attributes', JSON.parse(attributes));
                    }
                }
            };
            return selectList;
        }
    });
    //US #3836 : change the dropdown from Amount field to be numeric fields
    this.htmlTemplateEditor.TraitManager.addType('worldline-paiment-amount', {
        getInputEl: function getInputEl() {
            var modelSelect = this;
            var mergeFieldParams = {
                fieldFormat: [],
                ignoredFields: [],
                isPopup: false,
                isMultiple: false,
                isExtendedInput: true,
                descIdAssigned: -1
            }

            if (modelSelect.target.attributes.type === 'button-worldline') {
                mergeFieldParams.fieldFormat.push(eTools.FieldFormat.TYP_NUMERIC, eTools.FieldFormat.TYP_MONEY);
                mergeFieldParams.ignoredFields.push(eTools.DescIdEudoPlanningField.Alerts_Hour, eTools.DescIdEudoPlanningField.Periodicity, eTools.DescIdEudoPlanningField.Type);
            }

            mergeFieldParams.wordlinePaimentActivated = true;

            //on récupére le descid du champ s'il est djà renseigné
            var document = eTools.stringToHTMLDocument(that.addGetMergeFieldsSelectTemplate(mergeFieldParams));
            var selectList = document.body.children[0];
            selectList.id = "amountFieldSelect";
            selectList.onchange = function () {
                if (!selectList.options[selectList.selectedIndex].classList.contains('mergefieldGroup')) {
                    if ((!selectList.value || selectList.value == "") && modelSelect.target.view && modelSelect.target.view.el) {
                        modelSelect.target.view.el.removeAttribute('edndpa');
                        const attr = modelSelect.target.getAttributes();
                        delete attr.edndpa;
                        modelSelect.target.setAttributes(attr);
                    }
                }
            };
            return selectList;
        }
    });

    //US #3836 : change the dropdown from Transaction Reference field (right menu) to be character fields
    this.htmlTemplateEditor.TraitManager.addType('transaction-reference-field', {
        getInputEl: function getInputEl() {
            var modelSelect = this;
            var mergeFieldParams = {
                fieldFormat: [],
                ignoredFields: [],
                isPopup: false,
                isMultiple: false,
                isExtendedInput: true,
                descIdAssigned: -1
            }

            if (modelSelect.target.attributes.type === 'button-worldline') {
                mergeFieldParams.fieldFormat.push(eTools.FieldFormat.TYP_CHAR);
                mergeFieldParams.ignoredFields.push(eTools.DescIdEudoPlanningField.Alerts_Sound, eTools.DescIdEudoPlanningField.Calendar_Color);
                mergeFieldParams.isPopup = false;
            }
            mergeFieldParams.wordlinePaimentActivated = true;

            //on récupére le descid du champ s'il est djà renseigné
            var document = eTools.stringToHTMLDocument(that.addGetMergeFieldsSelectTemplate(mergeFieldParams));
            var selectList = document.body.children[0];
            selectList.id = "transactionReferenceField";
            selectList.onchange = function () {
                if (!selectList.options[selectList.selectedIndex].classList.contains('mergefieldGroup')) {
                    if ((!selectList.value || selectList.value == "") && modelSelect.target.view && modelSelect.target.view.el) {
                        modelSelect.target.view.el.removeAttribute('edndtr');
                        const attr = modelSelect.target.getAttributes();
                        delete attr.edndtr;
                        modelSelect.target.setAttributes(attr);
                    }
                }
            };
            return selectList;
        }
    });

    //US #3836 : change the dropdown from Payment Indicator field (right menu) to be logic block
    this.htmlTemplateEditor.TraitManager.addType('payment-indicator-field', {
        getInputEl: function getInputEl() {
            var modelSelect = this;
            var mergeFieldParams = {
                fieldFormat: [],
                ignoredFields: [],
                isPopup: false,
                isMultiple: false,
                isExtendedInput: true,
                descIdAssigned: -1
            }

            if (modelSelect.target.attributes.type === 'button-worldline') {
                mergeFieldParams.fieldFormat.push(eTools.FieldFormat.TYP_BIT, eTools.FieldFormat.TYP_BITBUTTON);
                mergeFieldParams.ignoredFields.push(eTools.DescIdEudoPlanningField.Alerts);
            }

            mergeFieldParams.wordlinePaimentActivated = true;
            //on récupére le descid du champ s'il est djà renseigné
            var document = eTools.stringToHTMLDocument(that.addGetMergeFieldsSelectTemplate(mergeFieldParams));
            var selectList = document.body.children[0];
            selectList.id = "paymentIndicatorField";
            selectList.onchange = function () {
                if (!selectList.options[selectList.selectedIndex].classList.contains('mergefieldGroup')) {
                    if ((!selectList.value || selectList.value == "") && modelSelect.target.view && modelSelect.target.view.el) {
                        modelSelect.target.view.el.removeAttribute('edndpi');
                        const attr = modelSelect.target.getAttributes();
                        delete attr.edndpi;
                        modelSelect.target.setAttributes(attr);
                    }
                }
            };
            return selectList;
        }
    });

    //https://github.com/artf/grapesjs/issues/185
    //#demande #91946
    this.htmlTemplateEditor.TraitManager.addType('content', {
        events: {
            'keyup': 'onChange',  // trigger parent onChange method on keyup
        },
        getInputEl: function () {
            if (!this.inputEl) {
                var input = document.createElement('input');
                input.value = this.target.get('content');
                this.inputEl = input;
            }

            var that = this;
            this.inputEl.addEventListener('change', (event) => {
                that.target.view.el.innerHTML = that.model.get('value');
                that.target.attributes.content = that.model.get('value');
                if (that.target.components().models.length != 0 && that.target.components().models[0].attributes.type == 'textnode')
                    that.target.components().models[0].set('content', that.model.get('value'));
            });
            return this.inputEl;

        },
        onValueChange: function () {
            this.target.view.el.innerHTML = this.model.get('value');
            this.target.attributes.content = this.model.get('value');
            if (this.target.components().models.length != 0 && this.target.components().models[0].attributes.type == 'textnode')
                this.target.components().models[0].set('content', this.model.get('value'));
        }
    });

    //Dans le panel 'Settings', on ajoute un nouveau type de traitement 'mergeFieldLink' pour les liens
    this.htmlTemplateEditor.TraitManager.addType('mergeFieldLink', {
        getInputEl: function getInputEl() {
            var modelLink = this;
            const el = document.createElement('div');
            el.className = 'gjs-radio-items';
            //Bloc Lien Web
            el.innerHTML = '<div class="gjs-radio-item gjs-webLink"><input type="radio" class="gjs-sm-radio" id="webLink" name="webLink" value="link"><label class="gjs-radio-item-label" style="color:#fff" for="webLink">' + top._res_7314 + '</label></div>';
            el.innerHTML += '<div class="gjs-radio-item gjs-mergeFieldLink"><input type="radio" class="gjs-sm-radio" id="mergeFieldLink" name="mergeFieldLink" value="mergeField"><label class="gjs-radio-item-label" style="color:#fff" for="mergeFieldLink">' + top._res_2914 + '</label></div>';

            var webLink = el.querySelector('.gjs-webLink');
            var mergeFieldLink = el.querySelector('.gjs-mergeFieldLink');
            var webLinkInput = el.querySelector('#webLink');
            var mergeFieldLinkInput = el.querySelector('#mergeFieldLink');

            webLink.onclick = function () {
                modelLink.target.getTrait("href").view.$el.show();
                modelLink.target.getTrait("mergefield-input").view.$el.hide();

                if (mergeFieldLinkInput)
                    mergeFieldLinkInput.checked = false;
                modelLink.target.addAttributes({ ednlt: 'lnk', ednc: 'lnk', ednd: "0", ednn: "" });
            };
            mergeFieldLink.onclick = function () {
                modelLink.target.getTrait("href").view.$el.hide();
                modelLink.target.getTrait("mergefield-input").view.$el.show();

                if (webLinkInput)
                    webLinkInput.checked = false;
                modelLink.target.addAttributes({ ednlt: 'mrg', ednc: 'lnk', ednd: "0", ednn: "" });
            };

            var oldAttr = modelLink.target.attributes.attributes.ednlt;
            if (oldAttr && oldAttr === "mrg") {
                mergeFieldLinkInput.checked = true;
                webLinkInput.checked = false;
                setTimeout(function () { mergeFieldLink.click(); }, 100);
            }
            else {
                mergeFieldLinkInput.checked = false;
                webLinkInput.checked = true;
                setTimeout(function () { webLink.click(); }, 100);
            }

            return el;
        }
    });

    //Tâche #2 708: on fait une customisation du panel 'Settings' 
    this.htmlTemplateEditor.DomComponents.getType('input').model
        .prototype.defaults.traits.splice(0, 1);
    this.htmlTemplateEditor.DomComponents.getType('textarea').model
        .prototype.defaults.traits.splice(0, 1);
    this.htmlTemplateEditor.DomComponents.getType('input').model
        .prototype.defaults.traits[0].label = top._res_2769; //Texte en filigrane
    this.htmlTemplateEditor.DomComponents.getType('input').model
        .prototype.defaults.traits.splice(1, 1);
    this.htmlTemplateEditor.DomComponents.getType('input').model
        .prototype.defaults.traits.splice(1, 0, {
            type: 'mergefield-input',
            name: 'mergefield-input',
            label: top._res_8186
        });


    this.htmlTemplateEditor.DomComponents.getType('textarea').model
        .prototype.defaults.traits.splice(1, 0, {
            id: 'mergefield-input',
            type: 'mergefield-input',
            name: 'mergefield-input',
            label: top._res_8186
        });

    if (this.htmlTemplateEditor.DomComponents.getType('link').model.prototype.defaults.traits.length <= 3) {
        this.htmlTemplateEditor.DomComponents.getType('link').model
            .prototype.defaults.traits.splice(1, 0, {
                type: 'mergeFieldLink',
                name: '',
                label: false
            });

        this.htmlTemplateEditor.DomComponents.getType('link').model
            .prototype.defaults.traits.splice(3, 0, {
                type: 'mergefield-input',
                name: 'mergefield-input',
                label: top._res_8186
            });
    }

    //demande 83 876
    this.htmlTemplateEditor.DomComponents.getType('image').model.prototype.defaults.traits = [];
    this.htmlTemplateEditor.DomComponents.getType('image').model
        .prototype.defaults.traits.push({
            type: 'text',
            name: 'alt',
            label: top._res_2799,
            placeholder: top._res_2796,
        });

    this.htmlTemplateEditor.DomComponents.getType('input').model
        .prototype.defaults.traits[2].label = top._res_2770; //Rendre cette rubrique obligatoire

    //Traits checkbox
    this.htmlTemplateEditor.DomComponents.getType('checkbox').model
        .prototype.defaults.traits.splice(1, 0, {
            type: 'mergefield-input',
            name: 'mergefield-input',
            label: top._res_8186
        });
    this.htmlTemplateEditor.DomComponents.getType('checkbox').model
        .prototype.defaults.traits.splice(0, 1);
    this.htmlTemplateEditor.DomComponents.getType('checkbox').model
        .prototype.defaults.traits.splice(1, 1);
    this.htmlTemplateEditor.DomComponents.getType('checkbox').model
        .prototype.defaults.traits.splice(1, 1);
    this.htmlTemplateEditor.DomComponents.getType('checkbox').model
        .prototype.defaults.traits[2].label = top._res_308; //Cochée
    //var required = this.htmlTemplateEditor.DomComponents.getType('checkbox').getTrait('required');
    //Traits button 
    this.htmlTemplateEditor.DomComponents.getType('button').model
        .prototype.defaults.traits.splice(1, 1);
    this.htmlTemplateEditor.DomComponents.getType('button').model
        .prototype.defaults.traits[0].label = top._res_1001;
    //#3 828
    this.htmlTemplateEditor.DomComponents.getType('eudonet-extended-select').model
        .prototype.defaults.traits.splice(0, 2);
    this.htmlTemplateEditor.DomComponents.getType('eudonet-extended-select').model
        .prototype.defaults.traits.push({
            type: 'mergefield-input',
            name: 'mergefield-input',
            label: top._res_8186
       });
    this.htmlTemplateEditor.DomComponents.getType('eudonet-extended-select').model
        .prototype.defaults.traits.push({
            type: 'checkbox',
            name: 'required',
            label: top._res_2770
        });
}

//cette méthode permet de charger les types champs de fusion eudonet
eGrapesJSEditor.prototype.loadMergefieldComponentElement = function (memoData) {
    memoData = eTools.addAttributesToMemoData(memoData);
    this.injectCSS("grapesjs/grapesjs-responsive-eudonet.css", true);
    this.injectCSS("../../../IRISBlack/Front/Scripts/Libraries/vuetify/vuetify.min.css", true);
    this.injectCSS("../../../IRISBlack/Front/Assets/CSS/advForm/materialdesign/materialdesignicons.min.css", true);
    return memoData;
}

//cette méthode permet de charger les polices pour l'éditeur des formulaires avancés
eGrapesJSEditor.prototype.loadFontsCss = function () {
    this.injectCSS("../../../IRISBlack/Front/Assets/CSS/advForm/advForm.css", true);
}

//Contrôle sur les champs de saisie 
eGrapesJSEditor.prototype.validateGrapesJSComponents = function (errorValidation) {
    var extendedComponents = this.htmlTemplateEditor.DomComponents.getWrapper().view.el.querySelectorAll('eudonet-extended-input');
    extendedComponents.forEach(function (item) {
        var codeRes = '';
        if (item.classList.contains('champSaisie'))
            codeRes = eTools.FormularValidationErrorCode.InputTextError;
        else if (item.classList.contains('email'))
            codeRes = eTools.FormularValidationErrorCode.InputMailError;
        else if (item.classList.contains('phone'))
            codeRes = eTools.FormularValidationErrorCode.InputPhoneError;
        else if (item.classList.contains('checkbox'))
            codeRes = eTools.FormularValidationErrorCode.InputCheckboxError;
        else if (item.classList.contains('champSaisie-num'))
            codeRes = eTools.FormularValidationErrorCode.InputNumError;
        else if (item.classList.contains('champSaisie-date'))
            codeRes = eTools.FormularValidationErrorCode.InputDateError;
        else if (item.classList.contains('champSaisie-memo'))
            codeRes = eTools.FormularValidationErrorCode.InputMemoError;
        else if (item.classList.contains('champSaisie-dropdown-simple'))
            codeRes = eTools.FormularValidationErrorCode.CatalogError;
        else if (item.classList.contains('champSaisie-dropdown-multiple'))
            codeRes = eTools.FormularValidationErrorCode.MultipleChoiceError;

        var descIdAttr = item.getAttribute('ednd');

        //si la rubrique associée (descId dans la balise) n'est pas renseignée et le type d'erreur n'est pas ajouté dans le dictionnaire de l'erreur, on ajoute l'erreur 
        if ((!descIdAttr || descIdAttr == '') && !errorValidation.ErrorIsAlreadyAdded(codeRes)) {
            errorValidation.addNewError(codeRes, eTools.getRes(2646) + ' "' + eTools.getRes(codeRes) + '" ' + eTools.getRes(2647) + '.');//permet de construire la chaine de caractère de l'erreur à partir du res
        }
    });

    var extendedbtnComponents = this.htmlTemplateEditor.DomComponents.getWrapper().view.el.querySelectorAll('extended-worldline-btn');
    extendedbtnComponents.forEach(function (item) {
        var codebtnRes = '';
        var error = new Array();

        if (item.innerHTML == '')
            error.push(eTools.getRes(3072));

        if (item.classList.contains('worldlinePaiment'))
            codebtnRes = eTools.FormularValidationErrorCode.WorldLinePaymentBtn;

        var descIdAttrpa = item.getAttribute('edndpa');
        var descIdAttrtr = item.getAttribute('edndtr');
        var descIdAttrpi = item.getAttribute('edndpi');

        if (!errorValidation.ErrorIsAlreadyAdded(codebtnRes)) {

            if (!descIdAttrpa || descIdAttrpa == '')
                error.push(eTools.getRes(3073));                

            if (!descIdAttrtr || descIdAttrtr == '')
                error.push(eTools.getRes(3074));

            if (!descIdAttrpi || descIdAttrpi == '')
                error.push(eTools.getRes(3075));
        }

        if (error != null) {
            if (error.length == 4) {
                errorValidation.addNewError(codebtnRes, eTools.getRes(3076).replace('{0}', error[0]).replace('{1}', error[1]).replace('{2}', error[2]).replace('{3}', error[3]).capitalize());
            }
            else if (error.length == 3) {
                errorValidation.addNewError(codebtnRes, eTools.getRes(3077).replace('{0}', error[0]).replace('{1}', error[1]).replace('{2}', error[2]).capitalize());
            }
            else if (error.length == 2) {
                errorValidation.addNewError(codebtnRes, eTools.getRes(3078).replace('{0}', error[0]).replace('{1}', error[1]).capitalize());
            }
            else if (error.length == 1) {
                errorValidation.addNewError(codebtnRes, eTools.getRes(3079).replace('{0}', error[0]).capitalize());
            }           
        }

    });

    return true;
}

//capitilaze first letter from string
String.prototype.capitalize = function () {
    return this.charAt(0).toUpperCase() + this.slice(1); 
}

eGrapesJSEditor.prototype.UpdateRes = function (btnSubmitRes) {
    grapesjs.xrmLang[this.language].blockCustomRes["blockButtonSubmit"] = btnSubmitRes.toUpperCase();
    this.htmlTemplateEditor.BlockManager.get('eudonet-form').attributes.content.components[0].components[0].content = btnSubmitRes.toUpperCase();//TODO: on peut crèer un type custom button et en modifie la prop Name dynamiquement
}

