//*****************************************************************************************************//
//*****************************************************************************************************//
//*** MAB - 06/2012 - surcouche JS permettant d'afficher un champ Mémo HTML (via CKEditor) ou texte brut
//*** Permet de paramétrer ou d'ajouter des fonctionnalités à CKEditor pour les besoins d'Eudonet XRM
//*** Nécessite :
//*** - Scripts/eTools.js
//*** - Scripts/ckeditor/ckeditor.js
//*** - themes/default/css/eMemoEditor.css (pour l'affichage en mode Texte Brut)
//*****************************************************************************************************//
//*****************************************************************************************************//

// ------------------------------------------------------------------------------------------------
// OBJET eMEMOEDITOR
// Le constructeur est situé en fin de fonction
// ------------------------------------------------------------------------------------------------


function eMemoEditor(strInstanceName, bHTML, oContainer, oParentFieldEditor, strValue, bCompactMode, strJSVarName) {

    eGrapesJSEditor.call(this, strInstanceName, bHTML, oContainer, oParentFieldEditor, strValue, strJSVarName)
    if (typeof (strInstanceName) != "undefined") {
        if (top) {

            if (!top.window['_medt'])
                top.window['_medt'] = [];


            //l instance est edtCOL_ + tab + _+descidd _0_0_0']
            top.window['_medt'][strInstanceName] = this;
        }
    }

    var that = this; // pointeur vers l'objet eMemoEditor lui-même, à utiliser à la place de this dans les évènements onclick, setTimeout... (où this correspond alors à l'objet cliqué, à window...)

    // --------------------------------------------------------------------------
    // Propriétés
    // --------------------------------------------------------------------------

    this.pjListEnabled = true; //DEV fonctionalité n'est pas encore terminée
    this.automationEnabled = false; //mode automatisme
    this.formularEnabled = true;  // GCH - 2014/09/29 - #33619 : tant que le dev n'est pas terminé on ajoute une variable pour activer/désactiver les formulaires
    this.speechEnabled = ("webkitSpeechRecognition" in window); //detection si le composant de speech est compatible avec le navigateur
    // Caractéristiques du champ
    this.helpUrl = "./help/emailing/<lang>.index.html"; // Backlog #354 - URL pour l'affichage d'un tutorial d'aide
    this.helpDialog = null; // Backlog #354 - Handler permettant de gérer la fenêtre d'aide ouverte
    this.showOnlyMerged = false;//Permet d'afficher que les champs de fusion dans le toolbar
    // Affichage et apparence
    this.compactMode = bCompactMode; // à mettre à true pour réduire au maximum l'espace occupé par le champ Mémo (barre d'outils plus petite, skin simplifié...)
    // ----------------------------------------------------------------------------------------
    // /!\ Les deux options ci-dessous ne doivent bien entendu pas être mises à true toutes les deux (contradictoires) : en cas de conflit, preventCompactMode est privilégiée
    this.forceCompactMode = false; // à mettre à true pour forcer le mode compact (barre d'outils réduite) même si l'éditeur détecte qu'il a suffisament d'espace pour afficher tous les boutons
    this.preventCompactMode = false; // à mettre à true pour empêcher l'éditeur de s'initialiser en mode compact (barre d'outils réduite) même s'il détecte ne pas avoir suffisamment d'espace pour afficher tous les boutons
    // ----------------------------------------------------------------------------------------
    this.skin = "eudonet"; // skin à utiliser pour l'affichage du champ Mémo ; principalement pour CKEditor, mais est également utilisé en mode Texte brut pour adapter l'affichage
    this.reducedToolbar = false; // à mettre à true pour afficher un nombre minimum de boutons (signature + fullscreen) indépendemment de la barre d'outils spécifiée
    this.borderlessMode = false; // à mettre à true pour afficher un champ Mémo sans aucun habillage (pas de barres d'outils, de barre de statut...) - prend le pas sur compactMode
    this.nbRows = 0; // Nombre de lignes du champ Mémo (lorsque ce n'est pas un champ HTML)
    this.uaoz = false;  // Update Allowed On Zoom, dans le cas de l'affichage des notes des fiches parentes dans le pied de page, les notes ne sont modifiables que dans le mode zoom
    this.disc = false; // signet de type discussion--> rajout d'un commentaire à la volée
    this.fromParent = false; // Indique si le champ note vient de la table parente (note d'affaire depuis planning par exemple)
    this.toolbarHeightOffset = 45; // nombre de pixels à réserver pour l'affichage de la barre d'outils (sauf si mode Compact)
    this.inlineHeightOffset = 0; // nombre de pixels à retirer de la zone de texte en hauteur lorsqu'on l'affiche en mode Inline
    this.inlineWidthOffset = 0; // nombre de pixels à retirer de la zone de texte en largeur lorsqu'on l'affiche en mode Inline

    // ----
    // Ecouteurs d'événements CKEditor à câbler après initialisation complète du composant
    this.listeners = new Array();

    // Comportement et fonctionnalités
    this.updateOnBlur = false; // à mettre à true pour que la mise à jour en base soit prise en charge automatiquement, en interne par eMemoEditor, lorsque le curseur sort du champ
    this.waitingForUpdate = false; // booléen à usage interne uniquement. Indique qu'un appel à update() est en cours et permet éventuellement à validate() de forcer les MAJ en base
    this.focusOnShow = false; // à mettre à false si on ne souhaite pas placer le curseur dans le champ lors de son ouverture
    // indique si l'on doit remettre à null ("destroy") ou remettre à zéro ("reset") l'objet eMemoEditor en cours après édition d'une valeur (flagAsEdited).
    // ATTENTION, si "destroy" est indiqué comme valeur pour cette propriété, il est dans ce cas de la responsabilité de l'appelant de recréer l'objet via un new
    this.autoResetMode = '';
    //ajouter des fonts à ckeditor
    this.addCustomfont = false;

    /*
    //Canceled by KHA le 31/01/2013 et remplacé par une fonction asynchrone cf emain.js updateFile

    this.scrollIntoViewId = null; // indique l'ID de l'objet sur lequel on doit aligner l'ascenseur de la page lors d'un appel à this.scrollIntoView()
    this.scrollOnShow = false; // si ce paramètre est à true, on fera un appel à scrollIntoView() sur l'objet dont l'ID est indiqué par this.scrollIntoViewId, afin de placer 
    */
    // l'ascenseur de la page au niveau de cet objet, une fois que le champ Mémo aura été complètement instancié et affiché
    this.mergeFields = null;   // liste des champs de fusion utilisables dans la combobox "Champs de fusion" (si la fonctionnalité est activée)
    this.oMergeHyperLinkFields = null;  // liste des champs de fusion utilisables dans la combobox "Champs de fusion" pour les liens
    this.specialMergeFields = null; // champs de fucion sepciaux pour le mémo
    this.oTracking = null; //objet contenant le lien tracking et les rubriques d'information {link:'', fields:{}}
    this.toolbarButtonKeyStrokes = new Object();
    this.addStartSpaceWithMergeFields = true; // #72 278 - Indique s'il faut insérer, ou non, un espace insécable & nbsp ; avant un champ de fusion. Résout l'anomalie d'insertion du champ au sein de la balise <label> d'un autre
    this.addEndSpaceWithMergeFields = false; // #72 278 - Indique s'il faut insérer, ou non, un espace insécable & nbsp ; après un champ de fusion. Résout l'anomalie d'insertion du champ au sein de la balise <label> d'un autre

    this.externalTrackingEnabled = false; // indique si le tracking externalisé est activé. Conditionne certaines options (liens de visu interdit...)

    this.useNewUnsubscribeMethod = false; // indique si la gestion des consentements est activée

    this.partialContentsEdited = null; // Backlog #92 - Indique si on édite une portion du contenu du champ, auquel cas setData() doit modifier cette portion et non tout

    this.showXrmFormularBtn = "0";//indique si le btn liste des formualaires a été ajouté ou non

    this.paramWindow = getParamWindow(); // pointeur vers eParamIFrame pour récupérer certains paramètres (notamment ceux de Mon Eudonet : signatures, taille de police...)

    // ------------------------------------------------------------------------
    // Méthodes
    // ------------------------------------------------------------------------

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    // PARAMETRAGE
    // Ces méthodes sont généralement appelées pour paramétrer le champ avant affichage (show()), soit en interne, soit depuis la page qui instancie eMemoEditor
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------

    // Complète les variables de langue intégrées à CKEditor avec les ressources additionnelles de XRM
    // TODO...
    this.getLanguageResources = function () {
        if (this.isHTML && typeof (CKEDITOR) != "undefined" && typeof (CKEDITOR.lang) != "undefined") {
            var CKEditorCurrentLang = eval("CKEDITOR.lang." + this.language);
            if (typeof (CKEditorCurrentLang) != "undefined") {
                this.trace("Chargement des ressources de langue personnalisées pour CKEditor...");

                // Ressources de langues pour les plugins personnalisés
                this.trace("--> Ressources de langue pour les plugins");
                if (typeof (top._res_57) != 'undefined') { eval("CKEDITOR.lang." + this.language + ".xrmUserMessage = top._res_57;"); }
                eval("CKEDITOR.lang." + this.language + ".xrmUserSignature = top._res_6593;");
                eval("CKEDITOR.lang." + this.language + ".xrmUploadFiles = top._res_5042;");
                eval("CKEDITOR.lang." + this.language + ".xrmLinkVisuOption = top._res_6594;");

                eval("CKEDITOR.lang." + this.language + ".xrmLinkFormular = top._res_1142;");
                eval("CKEDITOR.lang." + this.language + ".xrmDisableTrackingLink = top._res_6596;");
                eval("CKEDITOR.lang." + this.language + ".xrmLinkName = top._res_6597;");
                eval("CKEDITOR.lang." + this.language + ".xrmTrackingField = top._res_6598;"); //Champ du lien

                eval("CKEDITOR.lang." + this.language + ".xrmMergeField = top._res_2914;");

                eval("CKEDITOR.lang." + this.language + ".xrmLinkVisualization = top._res_6925;");
                eval("CKEDITOR.lang." + this.language + ".xrmMergeFieldHyperLink = top._res_2913;");
                eval("CKEDITOR.lang." + this.language + ".xrmHelp = top._res_6187;");

                if (this.useNewUnsubscribeMethod) {
                    if (this.toolbarType == "smsing") {
                        eval("CKEDITOR.lang." + this.language + ".xrmLinkUnsubOption = top._res_8760;"); //Stop SMS
                        eval("CKEDITOR.lang." + this.language + ".xrmLinkUnsubscribe = top._res_8760;"); //Stop SMS
                    }
                    else {
                        eval("CKEDITOR.lang." + this.language + ".xrmLinkUnsubOption = top._res_1847;"); //Paramétrages d'abonnements
                        eval("CKEDITOR.lang." + this.language + ".xrmLinkUnsubscribe = top._res_1861;");
                    }

                }
                else {
                    if (this.toolbarType == "smsing") {
                        eval("CKEDITOR.lang." + this.language + ".xrmLinkUnsubOption = top._res_8760;"); //Stop SMS
                        eval("CKEDITOR.lang." + this.language + ".xrmLinkUnsubscribe = top._res_8760;"); //Stop SMS
                    }
                    else {
                        eval("CKEDITOR.lang." + this.language + ".xrmLinkUnsubOption = top._res_6595;"); //Désinscription
                        eval("CKEDITOR.lang." + this.language + ".xrmLinkUnsubscribe = top._res_6924;");
                    }

                }


                eval("CKEDITOR.lang." + this.language + ".xrmMergeFields = {" +
                    "label: top._res_6484," +
                    "voiceLabel: top._res_6484," +
                    "panelTitle: top._res_6599" +
                    "};");

                eval("CKEDITOR.lang." + this.language + ".xrmSpecialMergeFields = {" +
                    "label: top._res_716," +
                    "voiceLabel: top._res_6484," +
                    "panelTitle: top._res_6599" +
                    "};");

                eval("CKEDITOR.lang." + this.language + ".xrmFormularMergeFields = CKEDITOR.lang." + this.language + ".xrmMergeFields;");
                eval("CKEDITOR.lang." + this.language + ".xrmActions = {" +
                    "label: top._res_296," + // Actions
                    "voiceLabel: top._res_296," + // Actions
                    "panelTitle: top._res_6600" + // Actions utilisateur et modèles de formulaires
                    "};");
                eval("CKEDITOR.lang." + this.language + ".xrmFullScreen = top._res_6601;"); // Plein écran
                eval("CKEDITOR.lang." + this.language + ".xrmFullScreenDialog = top._res_6602;"); // Plein écran (nouvelle fenêtre)
                eval("CKEDITOR.lang." + this.language + ".xrmImage = top._res_712;"); // Insérer une image
                eval("CKEDITOR.lang." + this.language + ".xrmFormular = top._res_6610;"); // Formulaires

                if (typeof (top._res_6198) != 'undefined') {
                    eval("CKEDITOR.lang." + this.language + ".xrmLink = {" +
                        "cookieTracking: top._res_6198" +
                        "};");
                }
                eval("CKEDITOR.lang." + this.language + ".xrmInsertCSS = top._res_6603;"); // Feuilles de style
                eval("CKEDITOR.lang." + this.language + ".xrmFormularMergeFieldsWarnings = {" +
                    "mergeFieldExists: top._res_6926," +
                    "buttonExists: top._res_6927" +
                    "};");
                eval("CKEDITOR.lang." + this.language + ".xrmFormularReadWriteDialog = {" +
                    "title: top._res_6484," +
                    "radioLabel: top._res_6928," +
                    "radioButtonRead: top._res_1599," +
                    "radioButtonWrite: top._res_1600" +
                    "};");
                if (this.speechEnabled)
                    eval("CKEDITOR.lang." + this.language + ".ckWebSpeech = {" +
                        "buttonTooltip: top._res_6742," +
                        "settings: top._res_6743," +
                        "enable: top._res_6744," +
                        "disable: top._res_6745," +
                        "settingsLanguage: top._res_6746," +
                        "settingsCulture: top._res_6747," +
                        "settingsDialogTitle: top._res_6748," +
                        "settingsDialogTabBasic: top._res_6749," +
                        "settingsDialogTabAdvanced: top._res_6750" +
                        "};");
                eval("CKEDITOR.lang." + this.language + ".xrmBrowse = top._res_6498;"); // Backlog #315

                // Création de variables "raccourcis" vers les noms de certaines commandes pour la fonction getCommandDescription
                // A définir lorsqu'il n'existe pas, dans les fichiers de langue CKEditor, de variable portant directement le nom de la commande
                // Ces variables additionnelles sont préfixées par un objet "xrmToolTip" afin que leur définition ne masque pas une autre propriété
                // déclarée avec le même nom dans le code source de CKEditor
                this.trace("--> Ressources de langue pour les raccourcis clavier");
                eval("CKEDITOR.lang." + this.language + ".xrmToolTip = { };"); // création de l'objet
                eval("CKEDITOR.lang." + this.language + ".xrmToolTip.bold = CKEDITOR.lang." + this.language + ".basicstyles.bold;");
                eval("CKEDITOR.lang." + this.language + ".xrmToolTip.italic = CKEDITOR.lang." + this.language + ".basicstyles.italic;");
                eval("CKEDITOR.lang." + this.language + ".xrmToolTip.underline = CKEDITOR.lang." + this.language + ".basicstyles.underline;");
                eval("CKEDITOR.lang." + this.language + ".xrmToolTip.justifyleft = (CKEDITOR.lang." + this.language + ".justify ? CKEDITOR.lang." + this.language + ".justify.left : CKEDITOR.lang." + this.language + ".common.left);");
                eval("CKEDITOR.lang." + this.language + ".xrmToolTip.justifycenter = (CKEDITOR.lang." + this.language + ".justify ? CKEDITOR.lang." + this.language + ".justify.center : CKEDITOR.lang." + this.language + ".common.center);");
                eval("CKEDITOR.lang." + this.language + ".xrmToolTip.justifyright = (CKEDITOR.lang." + this.language + ".justify ? CKEDITOR.lang." + this.language + ".justify.right : CKEDITOR.lang." + this.language + ".common.right);");
                eval("CKEDITOR.lang." + this.language + ".xrmToolTip.justifyblock = (CKEDITOR.lang." + this.language + ".justify ? CKEDITOR.lang." + this.language + ".justify.block : CKEDITOR.lang." + this.language + ".common.justify);");
                eval("CKEDITOR.lang." + this.language + ".xrmToolTip.source = CKEDITOR.lang." + this.language + ".sourcearea.toolbar;");
                eval("CKEDITOR.lang." + this.language + ".xrmToolTip.replace = CKEDITOR.lang." + this.language + ".find.replace;");
                eval("CKEDITOR.lang." + this.language + ".xrmToolTip.redo = CKEDITOR.lang." + this.language + ".undo.redo;");

                // Affichage des raccourcis clavier disponibles en tant que tooltip du champ Mémo
                this.trace("--> Ressources de langue pour les plugins - infobulle");
                var strMemoTooltip = 'CKEDITOR.lang.' + this.language + '.xrmHelpContents = "';
                // Les raccourcis clavier sont censés être définis à ce stade, puisque la fonction getLanguageResources() doit être appelée en différé après le chargement initial des
                // fichiers de langue de CKEditor, ce qui intervient après la phase d'initialisation de l'objet eMemoEditor où est définie la propriété keystrokes
                if (this.htmlConfig && this.htmlConfig.keystrokes && this.htmlConfig.keystrokes.length > 0) {
                    for (var i = 0; i < this.htmlConfig.keystrokes.length; i++) {
                        var nMaxCharCode = 65536;
                        var oKS = this.htmlConfig.keystrokes[i];
                        var nKS = oKS[0];
                        var strKS = "";
                        var bUsesCTRL = (((nKS - CKEDITOR.CTRL) > 0) && ((nKS - CKEDITOR.CTRL) < nMaxCharCode));
                        var bUsesALT = (((nKS - CKEDITOR.ALT) > 0) && ((nKS - CKEDITOR.ALT) < nMaxCharCode));
                        var bUsesSHIFT = (((nKS - CKEDITOR.SHIFT) > 0) && ((nKS - CKEDITOR.SHIFT) < nMaxCharCode));
                        if (bUsesCTRL) { nKS -= CKEDITOR.CTRL; strKS += "CTRL + "; }
                        if (bUsesALT) { nKS -= CKEDITOR.ALT; strKS += "ALT + "; }
                        if (bUsesSHIFT) { nKS -= CKEDITOR.SHIFT; strKS += "SHIFT + "; }
                        var strKeyDesc = strKS + getKeyDescFromCharCode(nKS);
                        var strCommandDesc = this.getCommandDescription(oKS[1]); // getKeyDescFromCharCode : fonction dans eTools.js
                        this.toolbarButtonKeyStrokes[oKS[1].toLowerCase()] = strKeyDesc; // on ajoute le nom de la commande en minuscules, car
                        // la fonction qui utilisera ce tableau de chaînes obtiendra le nom de la commande à partir de la classe CSS du bouton
                        // de barre d'outils, qui est en minuscules
                        strMemoTooltip += strKeyDesc + " => " + strCommandDesc + "<br>";
                    }
                }
                strMemoTooltip += '";';
                eval(strMemoTooltip);
                CKEDITOR.config.title = '';
                this.trace("--> Ressources de langue pour les plugins - descriptif des raccourcis clavier généré");

                // Ajout de l'objet lang sur l'objet eMemoEditor pour y accéder depuis un CKEditor non initialisé (ex : chargé par grapesjs)
                if (!this.htmlEditorLang)
                    this.htmlEditorLang = [];
                this.htmlEditorLang[this.language] = eval("CKEDITOR.lang." + this.language);
            }
        }

        this.getGrapesJSLanguageResources();
    };

    this.getCommandDescription = function (strCommand) {
        var strCommandDesc = strCommand;
        eval(
            "if (CKEDITOR.lang." + this.language + ") {" +
            "if (CKEDITOR.lang." + this.language + ".xrmToolTip && CKEDITOR.lang." + this.language + ".xrmToolTip." + strCommand + ") {" +
            "strCommandDesc = CKEDITOR.lang." + this.language + ".xrmToolTip." + strCommand + ";" +
            "}" +
            "else if (CKEDITOR.lang." + this.language + "." + strCommand + ") {" +
            "if (CKEDITOR.lang." + this.language + "." + strCommand + ".title) {" +
            "strCommandDesc = CKEDITOR.lang." + this.language + "." + strCommand + ".title;" +
            "}" +
            "if (CKEDITOR.lang." + this.language + "." + strCommand + ".toolbar) {" +
            "strCommandDesc = CKEDITOR.lang." + this.language + "." + strCommand + ".toolbar;" +
            "}" +
            "if (CKEDITOR.lang." + this.language + "." + strCommand + "." + strCommand + ") {" +
            "strCommandDesc = CKEDITOR.lang." + this.language + "." + strCommand + "." + strCommand + ";" +
            "}" +
            "if (strCommandDesc == strCommand) {" +
            "strCommandDesc = CKEDITOR.lang." + this.language + "." + strCommand + ";" +
            "}" +
            "}" +
            "}"
        );
        return strCommandDesc;
    }

    // Indique les plugins CKEditor à charger
    // Déclarer ici tous les plugins Eudo nécessaires sur tous les champs Mémo pour TOUTE la page
    // ATTENTION : il faut éviter, ici, d'ajouter des plugins en fonction d'une condition (ex : if (this.inlineMode)) car le chargement des plugins n'est fait
    // qu'une seule fois, via le premier champ Mémo chargé. Or, si celui-ci ne correspond pas à la condition indiquée, le plugin concerné par la condition ne serait
    // pas chargé du tout, y compris pour les autres champs Mémo de la page qui répondraient à cette fonction
    // TODO: à optimiser éventuellement, en ne chargeant que les plugins réellement utilisés en fonction de la page où on se trouve
    this.loadPlugins = function (strCustomPluginList) {
        if (this.isHTML) {
            if (!strCustomPluginList || strCustomPluginList == '') {
                // En full screen inutile d'afficher le bouton zoom
                var xrmFullScreen = this.isFullScreen ? '' : ',xrmFullScreen';

                // Backlog #653 - Le bouton CSS n'est plus affiché sous CKEditor si on utilise grapesjs, qui gère les CSS autrement
                // On doit tester la compatibilité navigateur EN plus de tester enableTemplateEditor, car il s'agit ici de l'activer sur le CKEditor non lié à
                // grapesjs, sur lequel cette variable est donc à false
                // Demande #72 138/#72 207 - Le bouton reste finalement
                var xrmInsertCSS = (
                    this.editorType == 'mail'
                    || this.editorType == 'mailing'
                    || this.editorType == 'formular'
                    || this.editorType == 'formularsubmission'
                    || this.editorType == 'mailtemplate'
                    || this.editorType == 'mailingtemplate') ? ',xrmInsertCSS' : '';

                strCustomPluginList = 'sourcedialog,nbsp' + xrmFullScreen + ',xrmUserMessage,xrmUserSignature,xrmUploadFiles,xrmImage,print';

                if (this.formularEnabled)
                    strCustomPluginList = strCustomPluginList + ',xrmFormular';

                if (this.automationEnabled)
                    strCustomPluginList = strCustomPluginList + ',xrmMergeFields';
                else
                    strCustomPluginList = strCustomPluginList + ',xrmFormularReadWrite' + xrmInsertCSS + ',xrmMergeFields,xrmSpecialMergeFields,xrmLinkAdapter,xrmMergeFieldHyperLink,xrmLinkUnsubscribe,xrmLinkVisualization,xrmImageAdapter';

                if (this.speechEnabled)
                    strCustomPluginList = strCustomPluginList + ',ckwebspeech';

                strCustomPluginList = strCustomPluginList + ',xrmHelp';


            }
            this.trace("Chargement des plugins personnalisés suivants : " + strCustomPluginList);
            CKEDITOR.config.extraPlugins = strCustomPluginList;
        }
    };

    // Active ou désactive le plugin 'elementspath' qui sert de barre d'état
    this.setStatusBarEnabled = function (bEnable) {
        if (this.isHTML) {
            this.addRemovePlugin('elementspath', bEnable);

            this.trace("Barre de statut (plugin elementspath) : " + (bEnable ? "Activée" : "Désactivée"));
        }
    };

    this.addPlugin = function (strPlugin) {
        this.addRemovePlugin(strPlugin, true);
    };

    this.removePlugin = function (strPlugin) {
        this.addRemovePlugin(strPlugin, false);
    };



    this.addRemovePlugin = function (strPlugin, bAdd) {
        if (this.isHTML) {

            var disabledPlugins = new Array();

            if (typeof (this.htmlConfig.removePlugins) == "string")
                disabledPlugins = this.htmlConfig.removePlugins.split();

            if (bAdd) {
                disabledPlugins.removeValue(strPlugin);
            }
            else {
                disabledPlugins.push(strPlugin);
            }

            this.htmlConfig.removePlugins = disabledPlugins.join(',');

            this.trace("Plug-in (" + strPlugin + ") " + (bAdd ? "activé" : "désactivé"));
            this.trace("Removed plugins : " + disabledPlugins.join(','));
        }
    };

    // Renvoie ou paramètre la barre d'outils à afficher sur le champ Mémo
    // - En mode HTML, le seul appel à la méthode paramètre l'objet config à destination de CKEditor
    // - En mode Texte brut, la fonction renvoie le code HTML des boutons à afficher sur la barre d'outils
    this.setToolBar = function () {
        var returnValue = false;
        // Barre d'outils HTML (syntaxe CKEditor) - personnalisable en fonction de la valeur de la propriété toolbarType
        // Ajouter ici les différentes barres d'outils souhaitées aux différents endroits de l'application, et renseigner depuis la page appelante la propriété toolbarType
        // pour afficher la barre d'outils en question
        if (this.isHTML) {
            // En mode "sans bordures", on masque la barre d'outils en retirant tous ses boutons, sauf si le mode inline est activé
            var bCanDisplayFullToolbar = true;
            if (!this.inlineMode) {
                if (this.borderlessMode) {
                    bCanDisplayFullToolbar = false;
                    this.htmlConfig.toolbar = [];
                }
                // Sinon, on affiche la barre d'outils souhaitée selon le mode d'affichage
                /*
                else if (this.compactMode && this.reducedToolbar) {
                    bCanDisplayFullToolbar = false;
                    if (this.isFullScreen) {
                        this.htmlConfig.toolbar = [
                            ['xrmUserMessage']
                        ];
                    }
                    else {
                        this.htmlConfig.toolbar = [
                            ['xrmUserMessage', 'xrmFullScreenDialog']
                        ];
                    }
                }
                */
            }
            if (bCanDisplayFullToolbar) {
                var sourcePlugin = 'Source';
                if (this.inlineMode) {
                    sourcePlugin = 'Sourcedialog'; // https://dev.ckeditor.com/ticket/9713 - http://ckeditor.com/addon/sourcedialog
                }
                var speechPlugin = '';
                if (this.speechEnabled)
                    speechPlugin = 'Webspeech';

                switch (this.toolbarType) {
                    case "mailing":
                    case "mailingtemplate":
                    case "mailtemplate":
                        var bAdvancedMode = true;

                        // Définition des groupes de boutons de base
                        var toolbarDocumentButtons = ['xrmFullScreenDialog'];
                        var toolbarEditButtons = ['PasteFromWord'];
                        var toolbarStyleButtons = ['Font', 'FontSize', 'Bold', 'Italic', 'Underline', 'Strike']
                        var toolbarColorStylesButtons = ['BGColor', 'TextColor'];
                        var toolbarAdditionalStylesButtons = ['Subscript', 'Superscript', 'RemoveFormat'];
                        var toolbarParagraphButtons = ['JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock', 'NumberedList', 'BulletedList', 'Outdent', 'Indent'];
                        var toolbarInsertButtons = null;




                        if (!that.externalTrackingEnabled) {
                            switch (this.toolbarType) {
                                case "mailingtemplate":
                                    toolbarInsertButtons = ['Table', 'xrmImage', 'xrmUploadFiles', 'xrmUserSignature', 'Link', 'xrmMergeFieldHyperLink', 'xrmLinkUnsubscribe', 'xrmLinkVisualization'];
                                    break;
                                case "mailtemplate":
                                    toolbarInsertButtons = ['Table', 'xrmImage', 'xrmUserSignature', 'Link', 'xrmMergeFieldHyperLink'];
                                    break;
                                default:
                                    toolbarInsertButtons = ['Table', 'xrmImage', 'xrmUploadFiles', 'xrmUserSignature', 'Link', 'xrmMergeFieldHyperLink', 'xrmLinkUnsubscribe', 'xrmLinkVisualization'];
                                    break;
                            }
                        }
                        else {
                            switch (this.toolbarType) {
                                case "mailingtemplate":
                                    toolbarInsertButtons = ['Table', 'xrmImage', 'xrmUserSignature', 'Link', 'xrmMergeFieldHyperLink', 'xrmLinkUnsubscribe'];
                                    break;
                                case "mailtemplate":
                                    toolbarInsertButtons = ['Table', 'xrmImage', 'xrmUserSignature', 'Link', 'xrmMergeFieldHyperLink'];
                                    break;
                                default:
                                    toolbarInsertButtons = ['Table', 'xrmImage', 'xrmUserSignature', 'Link', 'xrmMergeFieldHyperLink', 'xrmLinkUnsubscribe'];
                                    break;
                            }
                        }

                        if ((//Emailing
                            (typeof (oCurrentWizard) != "undefined"
                                && typeof (oCurrentWizard._nParentTabId) != "undefined" && oCurrentWizard._nParentTabId > 0
                                && typeof (oCurrentWizard._nParentFileId) != "undefined" && oCurrentWizard._nParentFileId > 0
                                && typeof (oCurrentWizard._tab) != "undefined" && oCurrentWizard._tab > 0)
                            ||//Modèles
                            (typeof (this._nParentTabId) != "undefined" && this._nParentTabId > 0
                                && typeof (this._nParentFileId) != "undefined" && this._nParentFileId > 0
                                && typeof (this._tab) != "undefined" && this._tab > 0)
                            || this.showXrmFormularBtn === "1"
                        ) && (this.formularEnabled && !that.externalTrackingEnabled)) {
                            toolbarInsertButtons.push('xrmFormular');  //Formulaire
                            //Demande #45 664, ce flag sera utlisé pour affiché le btn formulaires dans la barre de CKEdirtor.
                            //TODO: vérifier si la conftion this.formularEnabled est suffsante pour permettre d'afficher le boutton
                            this.showXrmFormularBtn = "1";
                        }
                        toolbarInsertButtons.push('xrmMergeFields', 'xrmInsertCSS');

                        // Tâche #2407 - Bouton Caractères Spéciaux
                        toolbarInsertButtons.push('SpecialChar');

                        // Ajout de boutons additionnels si on affiche une barre d'outils "avancée"
                        if (bAdvancedMode) {
                            toolbarDocumentButtons.push('Print', 'Preview');
                            toolbarEditButtons = ['Cut', 'Copy', 'Paste', 'PasteText', 'PasteFromWord', 'SelectAll', '-', 'Undo', 'Redo', '-', 'Find', 'Replace', '-'];
                            toolbarStyleButtons.unshift('Styles', 'Format'); // ajout des boutons DEVANT ceux existants
                            toolbarStyleButtons = toolbarStyleButtons.concat(toolbarAdditionalStylesButtons); // puis APRES ceux existants
                            toolbarParagraphButtons.push('Blockquote');
                        }
                        else {
                            toolbarInsertButtons = toolbarEditButtons.concat(toolbarInsertButtons); // fusion des boutons du groupe "Edition" avec ceux du groupe "Insertion"
                            toolbarEditButtons = []; // pas de groupe de boutons "Edition" séparé
                        }

                        // Fusion de certains groupes de boutons
                        toolbarStyleButtons = toolbarStyleButtons.concat(toolbarColorStylesButtons); // ajout des boutons "Couleurs"

                        // Construction de la barre d'outils définitive
                        this.htmlConfig.toolbar = [
                            toolbarDocumentButtons,
                            toolbarStyleButtons,
                            toolbarParagraphButtons,
                            toolbarEditButtons,
                            toolbarInsertButtons,
                            [speechPlugin],
                            [sourcePlugin]
                        ];

                        break;
                    case "mail":
                        this.htmlConfig.toolbar = [
                            ['xrmFullScreenDialog', 'Print'], ['Font', 'FontSize'], ['Bold', 'Italic', 'Underline', 'Strike'], ['BGColor', 'TextColor'], ['JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock'], ['NumberedList', 'BulletedList', 'Outdent', 'Indent'], ['PasteFromWord', 'Table', 'xrmImage', 'xrmUserSignature', 'Link', 'xrmMergeFieldHyperLink', 'xrmMergeFields', 'xrmInsertCSS', 'SpecialChar'], [speechPlugin], [sourcePlugin]
                        ];
                        break;
                    case "automation":
                        this.htmlConfig.toolbar = [['Bold', 'Italic', 'Underline', 'Strike'], ['TextColor', 'xrmMergeFields'], ['SpecialChar'], [speechPlugin], [sourcePlugin]];
                        this.htmlConfig.removePlugins = 'elementspath'; // Le bottom de ckeditor
                        break;
                    case "mailSubject":
                        //KJE, tâche #2 551: si l'extension envoie de Mail par Mapp est activée, on masque les champs de fusion
                        if (this.showOnlyMerged)
                            this.htmlConfig.toolbar = [['xrmMergeFields']];
                        else
                            this.htmlConfig.toolbar = [];
                        this.htmlConfig.removePlugins = 'elementspath'; // Le bottom de ckeditor
                        break;
                    case "smsing":
                        if (
                            typeof (oCurrentWizard) != "undefined"
                            && typeof (oCurrentWizard.GetVersionPresta) == "function"
                            && oCurrentWizard.GetVersionPresta() == 'LM_REST'
                        ) {

 
                        if (this.formularEnabled) {
                            this.htmlConfig.toolbar = [['xrmMergeFields'], ['xrmFormular'], ['xrmLinkUnsubscribe']];
 
                                this.showXrmFormularBtn = "1";
                            }
                            else{
                                this.htmlConfig.toolbar = [['xrmMergeFields'], ['xrmLinkUnsubscribe']];
                            }
                        }
                        else {

                            this.htmlConfig.toolbar = [[]]
                          
                        }

                        this.htmlConfig.removePlugins = 'elementspath'; // Le bottom de ckeditor

                        break;
                    case "formular":
                        /*KHA le 28/04/2015 les CSS ne sont pas fonctionnelles sur les formulaires. on masque le bouton pour l'instant*/
                        this.htmlConfig.toolbar = [
                            ['xrmFullScreenDialog', 'Print'], ['Font', 'FontSize'], ['Bold', 'Italic', 'Underline', 'Strike'], ['BGColor', 'TextColor'], ['JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock'], ['NumberedList', 'BulletedList', 'Outdent', 'Indent'], ['PasteFromWord', 'Table', 'xrmImage', 'Link', 'xrmMergeFieldHyperLink', 'xrmMergeFields', 'xrmInsertCSS', 'SpecialChar'], [speechPlugin], [sourcePlugin]
                        ];
                        break;
                    case "adminusersign":
                        this.htmlConfig.toolbar = [
                            { name: 'firstButtons', groups: ['firstButtons'], items: ['xrmFullScreenDialog'] },
                            { name: 'fonts', items: ['Font', 'FontSize'] },
                            { name: 'fontSizes', items: ['Bold', 'Italic', 'Underline', 'Strike'] },
                            { name: 'colors', items: ['BGColor', 'TextColor'] },
                            { name: 'paragraphs', items: ['JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock'] },
                            { name: 'lists', items: ['NumberedList', 'BulletedList', 'Outdent', 'Indent'] },
                            { name: 'insert', items: ['PasteFromWord', 'Table', 'xrmImage', 'Link', 'SpecialChar'] },
                            { name: 'source', items: [sourcePlugin] },
                            { name: 'help', items: ['xrmHelp'] }
                        ];
                        break;
                    case "adminusermemo":
                        this.htmlConfig.toolbar = [
                            { name: 'firstButtons', groups: ['firstButtons'], items: ['xrmFullScreenDialog'] },
                            { name: 'fonts', items: ['Font', 'FontSize'] },
                            { name: 'fontSizes', items: ['Bold', 'Italic', 'Underline', 'Strike'] },
                            { name: 'colors', items: ['BGColor', 'TextColor'] },
                            { name: 'paragraphs', items: ['JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock'] },
                            { name: 'lists', items: ['NumberedList', 'BulletedList', 'Outdent', 'Indent'] },
                            { name: 'insert', items: ['PasteFromWord', 'Table', 'xrmImage', 'Link', 'SpecialChar'] },
                            { name: 'merge', items: ['xrmSpecialMergeFields'] },
                            { name: 'source', items: [sourcePlugin] },
                            { name: 'help', items: ['xrmHelp'] }
                        ];
                        break;
                    default:
                        if (this.isFullScreen) {
                            this.htmlConfig.toolbar = [
                                { name: 'firstButtons', groups: ['firstButtons'], items: ['xrmUserMessage', 'Print'] },
                                { name: 'fonts', items: ['Font', 'FontSize'] },
                                { name: 'fontSizes', items: ['Bold', 'Italic', 'Underline', 'Strike'] },
                                { name: 'colors', items: ['BGColor', 'TextColor'] },
                                { name: 'paragraphs', items: ['JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock'] },
                                { name: 'lists', items: ['NumberedList', 'BulletedList', 'Outdent', 'Indent'] },
                                { name: 'insert', items: ['PasteFromWord', 'Table', 'xrmImage', 'Link', 'SpecialChar'] },
                                { name: 'webSpeech', items: [speechPlugin] },
                                { name: 'source', items: [sourcePlugin] },
                                { name: 'help', items: ['xrmHelp'] }
                            ];


                        }
                        else {
                            this.htmlConfig.toolbar = [
                                { name: 'firstButtons', groups: ['firstButtons'], items: ['xrmUserMessage', 'xrmFullScreenDialog', 'Print'] },
                                { name: 'fonts', items: ['Font', 'FontSize'] },
                                { name: 'fontSizes', items: ['Bold', 'Italic', 'Underline', 'Strike'] },
                                { name: 'colors', items: ['BGColor', 'TextColor'] },
                                { name: 'paragraphs', items: ['JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock'] },
                                { name: 'lists', items: ['NumberedList', 'BulletedList', 'Outdent', 'Indent'] },
                                { name: 'insert', items: ['PasteFromWord', 'Table', 'xrmImage', 'Link', 'SpecialChar'] },
                                { name: 'webSpeech', items: [speechPlugin] },
                                { name: 'source', items: [sourcePlugin] },
                                { name: 'help', items: ['xrmHelp'] }
                            ];
                        }
                        break;
                }
            }
            returnValue = true;
        }
        // Barre d'outils Texte Brut
        else if (!this.borderlessMode) {
            var strUserMessageButton = '';
            var strFullScreenButton = '';
            var strStyle = '';
            if (!this.readOnly && this.toolbarType != 'css') {
                strUserMessageButton =
                    '<a onclick="' + this.jsVarName + '.insertUserMessage();">' +
                    '<div class="eME_user icon-avatar">' +
                    '</div>' +
                    '</a>';
            }
            if (!this.isFullScreen) {
                strFullScreenButton =
                    '<a onclick="' + this.jsVarName + '.switchFullScreen(true);">' +
                    '<div class="icon-ck_full eME_full">' +
                    '</div>' +
                    '</a>';
            }
            if (strUserMessageButton != '' && strFullScreenButton != '') {
                strStyle = ' style="display: inline-block;"';
            }
            if (strUserMessageButton != '' || strFullScreenButton != '') {
                returnValue =
                    '<div class="eME_btn"' + strStyle + '>' +
                    strUserMessageButton +
                    strFullScreenButton +
                    '</div>';
            }
            else {
                returnValue = '';
            }
        }
        else {
            returnValue = '';
        }

        return returnValue;
    };

    // Backlog #356 - Paramètre et renvoie la barre d'outils de base à afficher sur le champ Mémo lorsqu'il est instancié via grapesjs, et désactive les commandes inutilisées
    // en fonction du type de bloc édité (Texte, Image, Bouton) afin de faire disparaître
    this.setToolBarForTemplateEditor = function (blockType) {
        if (!blockType)
            blockType = "text"; // type de barre d'outils par défaut : mode Texte

        this.trace("Type de barre d'outils grapesjs : " + blockType);
        var displayedToolbarButtons = new Array();
        var hiddenToolbarButtons = new Array();

        // Définition des boutons affichables sous conditions de disponibilité de la fonctionnalité
        var canDisplaySpeech = this.speechEnabled && blockType == "text";
        var canDisplayXrmMergeFields = blockType != "image";
        //var canDisplayXrmLinkVisualization = !that.externalTrackingEnabled && blockType != "image";
        var canDisplayXrmLinkVisualization = false;  // Backlog #573 - Plus d'édition de liens sur CKEditor grapesjs
        var canDisplayXrmFormular = false; // Backlog #573 - Plus d'édition de liens sur CKEditor grapesjs
        /*
            ((//Emailing
                (typeof (oCurrentWizard) != "undefined"
                    && typeof (oCurrentWizard._nParentTabId) != "undefined" && oCurrentWizard._nParentTabId > 0
                    && typeof (oCurrentWizard._nParentFileId) != "undefined" && oCurrentWizard._nParentFileId > 0
                    && typeof (oCurrentWizard._tab) != "undefined" && oCurrentWizard._tab > 0)
                ||//Modèles
                (typeof (this._nParentTabId) != "undefined" && this._nParentTabId > 0
                    && typeof (this._nParentFileId) != "undefined" && this._nParentFileId > 0
                    && typeof (this._tab) != "undefined" && this._tab > 0)
            ) && (this.formularEnabled && !that.externalTrackingEnabled));
        */
        // Définition des groupes de boutons de base
        var toolbarStyleButtons = ['Format', 'Font', 'FontSize', 'Bold', 'Italic', 'Underline', 'Strike'];
        var toolbarColorStylesButtons = ['BGColor', 'TextColor'];
        var toolbarParagraphButtons = ['JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock', 'NumberedList', 'BulletedList', 'Outdent', 'Indent'];
        var toolbarInsertButtons = ['Table', 'SpecialChar'/*, 'xrmUploadFiles', 'Link', 'xrmLinkUnsubscribe'*/]; // Backlog #573 - Plus d'édition de liens sur CKEditor grapesjs
        if (canDisplayXrmLinkVisualization) { toolbarInsertButtons.push('xrmLinkVisualization'); } else { hiddenToolbarButtons = hiddenToolbarButtons.concat('xrmLinkVisualization'); }
        if (canDisplayXrmFormular) { toolbarInsertButtons.push('xrmFormular'); } else { hiddenToolbarButtons = hiddenToolbarButtons.concat('xrmFormular'); }
        if (canDisplayXrmMergeFields) { toolbarInsertButtons.push('xrmMergeFields'); } else { hiddenToolbarButtons = hiddenToolbarButtons.concat('xrmMergeFields'); }

        // Regroupement des boutons en tableaux globaux
        // 1 : Array à plusieurs niveaux(groupes)
        var baseToolbar = new Array(
            toolbarStyleButtons,
            toolbarColorStylesButtons,
            toolbarParagraphButtons,
            toolbarInsertButtons
        );
        // 2 : Array mono-niveau (concat)
        displayedToolbarButtons =
            displayedToolbarButtons
                .concat(toolbarStyleButtons)
                .concat(toolbarColorStylesButtons)
                .concat(toolbarParagraphButtons)
                .concat(toolbarInsertButtons);
        // Ajout du plugin Reconnaissance vocale si supporté
        if (canDisplaySpeech) {
            baseToolbar.push(['Webspeech']); // Array multi-niveaux
            displayedToolbarButtons.push('Webspeech'); // Array mono-niveau
        }

        // Puis on personnalise l'affichage, si la barre d'outils a déjà été créée
        // Il faut cibler le CKEditor lié à l'instance de grapesjs. Pour cela, on extrait son ID en ciblant sa toolbar via getHTMLTemplateEditorLinkedRTEInstances()
        // Il ne faut surtout pas utiliser this.htmlEditor ici, qui représente la dernière instance de CKEditor affichée et connue (donc pas forcément celle correspondant
        // à l'élément grapesjs que l'on vient de sélectionner, bien au contraire)
        var allLinkedInstances = this.htmlTemplateEditor ? this.getHTMLTemplateEditorLinkedRTEInstances(this.htmlTemplateEditor.RichTextEditor) : null;
        var currentAssociatedInstance = allLinkedInstances ? allLinkedInstances.associatedInstance : null;
        var currentCKEditor = null;
        if (currentAssociatedInstance)
            currentCKEditor = document.getElementById("cke_" + currentAssociatedInstance.name);
        if (currentCKEditor) {
            var currentCKEditorToolbar = currentCKEditor.querySelector(".cke_toolbar");
            if (currentCKEditorToolbar) {
                blockType = blockType.replace('eudonet-extended-', ''); // un bloc "eudonet-extended-button" sera considéré comme "button" ci-dessous, idem pour "image"
                switch (blockType) {
                    case "image":
                        hiddenToolbarButtons = hiddenToolbarButtons.concat(toolbarStyleButtons);
                        hiddenToolbarButtons = hiddenToolbarButtons.concat(toolbarColorStylesButtons);
                        hiddenToolbarButtons = hiddenToolbarButtons.concat(toolbarParagraphButtons);
                        hiddenToolbarButtons = hiddenToolbarButtons.concat(toolbarInsertButtons.splice(0, 1)); // Pas de bouton Tableau pour les blocs autres que Texte
                        break;
                    case "button":
                    case "link":
                        hiddenToolbarButtons = hiddenToolbarButtons.concat(toolbarColorStylesButtons.splice(0, 1)); // Pas de bouton Couleur de fond pour le bloc Bouton (géré par grapesjs)
                        hiddenToolbarButtons = hiddenToolbarButtons.concat(toolbarInsertButtons.splice(0, 1)); // Pas de bouton Tableau pour les blocs autres que Texte
                        break;
                    case "text":
                    default:
                        // On ne désactive rien dans ce cas
                        break;
                    // Rappel : splice() renvoie les éléments supprimés du tableau, et la variable contient les éléments conservés. On se sert de la valeur de retour (éléments supprimés) ci-dessus
                }
                if (!canDisplaySpeech)
                    hiddenToolbarButtons.push('Webspeech');

                // Puis on passe en revue tous les boutons de CKEditor pour les activer/désactiver
                // On commence d'abord par tous les réactiver, avant de désactiver ceux qui ne nous intéressent pas
                for (var i = 0; i < displayedToolbarButtons.length; i++)
                    this.setToolBarControlDisplay(currentCKEditorToolbar, false, displayedToolbarButtons[i]);
                for (var i = 0; i < hiddenToolbarButtons.length; i++)
                    this.setToolBarControlDisplay(currentCKEditorToolbar, true, hiddenToolbarButtons[i]);

                // Backlog #573 - De la même manière, on bloque ensuite toutes les barres d'outils précédemment affichées,
                // puis on affiche l'actuelle si elle comporte des contrôles visibles
                // Pour ne pas cibler la barre d'outils d'un CKEditor instancié sans grapesjs sur la même fenêtre, on cible tous les .cke_top rattachées à des div
                // La barre d'outils d'un CKEditor instancié indépendamment étant un span, elle ne sera ainsi pas ciblée
                var allCKEditorInstanceToolbars = document.querySelectorAll("div.cke_top");
                for (var i = 0; i < allCKEditorInstanceToolbars.length; i++)
                    allCKEditorInstanceToolbars[i].style.display = 'none';

                var allButtons = currentCKEditorToolbar.querySelectorAll(".cke_button");
                var allCombos = currentCKEditorToolbar.querySelectorAll(".cke_combo");
                var hiddenButtons = currentCKEditorToolbar.querySelectorAll(".cke_button[hidden]");
                var hiddenCombos = currentCKEditorToolbar.querySelectorAll(".cke_combo[hidden]");
                if (
                    (allButtons && hiddenButtons && allButtons.length > hiddenButtons.length) ||
                    (allCombos && hiddenCombos && allCombos.length > hiddenCombos.length)
                )
                    currentCKEditor.querySelector(".cke_top").style.display = '';
            }
        }




        // On mémorise la barre d'outils paramétrée pour l'évènement rteToolbarPosUpdate
        this.toolbarTypeForTemplateEditor = blockType;

        // Puis on renvoie dans tous les cas la barre d'outils de base souhaitée
        return baseToolbar;
    };

    this.setToolBarControlDisplay = function (currentCKEditorToolbar, hidden, controlId) {
        var controlCount = 0;
        if (typeof (controlId) == "string") {
            controlId = controlId.toLowerCase();
            var controls = currentCKEditorToolbar.querySelectorAll("[class*='__" + controlId + "']");
            for (var i = 0; i < controls.length; i++)
                if (controls[i]) {
                    controls[i].hidden = hidden;
                    if (controls[i].style) {
                        controls[i].style.display = hidden ? 'none' : '';
                        this.trace("Contrôle/bouton " + controlId + " " + (hidden ? "masqué" : "affiché"));
                        controlCount++;
                    }
                }
        }
        if (controlCount == 0)
            this.trace("Aucun contrôle n'a pu être affiché/masqué sur la barre d'outils. Elle n'a peut-être pas encore été créée.");
    };

    // Paramètre la propriété fullPage de la config CKEditor pour conserver (ou non) les entêtes de HTML : Doctype, head, meta...
    // Utile dans certains contextes mais indésirable dans d'autres
    // Se fait en fonction du type d'éditeur (valeur de this.editorType) ou de la valeur passée en paramètre
    this.setFullPageMode = function (strEditorType) {
        // #53 136 - Il faut appliquer ce mode uniquement sur les CKEditor nécessitant d'éditer un code source complet/une page Web avec entêtes et CSS
        if (!strEditorType)
            strEditorType = this.editorType;

        if (
            strEditorType == "mail" ||
            strEditorType == "mailing"
        ) {
            //this.config.fullPage = true; // Valeur < 10.413
            this.config.fullPage = false; // Backlog #616 - Valeur utilisée depuis l'intégration de grapesjs et la gestion des styles/entêtes séparément pour le responsive
        }

        // Il ne faut pas activer ce mode dans les autres cas (ex : signature de l'utilisateur au CTRL+E, rubriques de type Mémo) car cela provoque l'ajout
        // systématique de balises <html><head><body> au code généré, qui devient alors invalide dès qu'il s'agit de l'incorporer à d'autres contenus
        if (
            strEditorType == "adminusersign" ||
            strEditorType == "adminusermemo" ||
            strEditorType == "formular" ||
            strEditorType == "formularsubmission"
        )
            this.config.fullPage = false;

        // Puis on laissera la valeur par défaut (non définie) dans les autres cas (ex : rubriques de type Mémo)
    };

    // Demande #54 068 et Backlog #616 - Positionne une police par défaut sur le canvas de l'éditeur, sauf pour les cas où la mise en forme doit pouvoir être contrôlée
    // Notamment la signature utilisateur et l'édition de mails
    // Demande #78 433 - US #2 925 - Tâche #4 326 - Ajout d'une classe supplémentaire eMEFontEudoSize_X pour appliquer la taille de police définie dans Mon Eudonet
    this.setFontClass = function (strEditorType) {
        if (!strEditorType)
            strEditorType = this.editorType;

        if (
            strEditorType != "adminusersign" &&
            strEditorType != "mail" &&
            strEditorType != "mailing"
        )
            this.htmlConfig.bodyClass += ' eMEFontEudo eMEFontEudoSize_' + this.paramWindow.GetParam("fontsize");
    };
    /**
    Applique, sur le conteneur du champ Mémo (div), les mêmes classes CSS que sur la propriété .config.bodyClass, mise à jour par setFontClass() ci-dessus
    Sur les CKEditor instanciés en inline, il semble nécessaire de l'ajouter également sur le conteneur après l'initialisation de l'instance (instanceReady). cf. setFontClassOnContainer()
    La propriété bodyClass ne semblant pas être prise en compte lorsqu'on instancie un CKEditor inline (ce qui est logique, puisque cette bodyClass est censée s'appliquer sur l'iframe interne de CKEditor, qui n'est pas utilisée en mode inline)
    Source : https://ckeditor.com/docs/ckeditor4/latest/api/CKEDITOR_config.html#cfg-bodyClass
    * */
    this.setFontClassOnContainer = function () {
        addClass(this.htmlEditor.container.$, this.htmlConfig.bodyClass);
    },

        //liste avec les champs de fusion pour le message utilisateur dans les mémos
        this.loadSpecialMergeFields = function () {

            if (this.toolbarType == "adminusermemo") {
                this.specialMergeFields = [
                    { "label": top._res_6763, "value": "MergeField;special;NULL;avatar;false" },
                    { "label": top._res_198, "value": "MergeField;special;NULL;username;false" },
                    { "label": top._res_411, "value": "MergeField;special;NULL;user;false" },
                    { "label": top._res_367, "value": "MergeField;special;NULL;date;false" },
                    { "label": top._res_368, "value": "MergeField;special;NULL;now;false" },
                    { "label": top._res_822, "value": "MergeField;special;NULL;weekday;false" },
                    { "label": top._res_921, "value": "MergeField;special;NULL;shortweekday;false" },
                    { "label": top._res_406, "value": "MergeField;special;NULL;year;false" },
                    { "label": top._res_613, "value": "MergeField;special;NULL;pagebreak;false" },
                ];
            }
        };

    // Gère l'affichage initial de la barre d'outils
    this.setToolBarDisplay = function (bShowToolBar, bToolBarCanCollapse) {
        if (this.isHTML) {
            if (!this.inlineMode) {
                this.trace("Barre d'outils " + (bShowToolBar ? "affichée" : "masquée") + ", " + (bToolBarCanCollapse ? "repliable" : "non repliable"));
                if (typeof (bShowToolBar) != 'undefined' && bShowToolBar != null)
                    this.htmlConfig.toolbarStartupExpanded = bShowToolBar;
                if (typeof (bToolBarCanCollapse) != 'undefined' && bToolBarCanCollapse != null)
                    this.htmlConfig.toolbarCanCollapse = bToolBarCanCollapse;
            }
        }
        else {
            var eME_head = document.getElementById("eME_head");
            if (eME_head != null)
                eME_head.style.display = (bShowToolBar ? "inline-block" : "none");
        }
    };

    // Affiche ou masque la barre d'outils à la demande
    this.hideShowToolBar = function (show) {
        if (this.isHTML) {
            if (this.inlineMode) {
                var oToolbar = this.getToolBarContainer();
                if (oToolbar) {
                    if (typeof (show) == 'undefined' || show == null) {
                        show = (oToolbar.style.display == 'none');
                    }
                    oToolbar.style.display = (show ? '' : 'none');
                }
            }
        }
    };

    // Renvoie un pointeur vers l'élément HTML de la barre d'outils, principalement dans le but d'agir sur son affichage en mode inline
    this.getToolBarContainer = function () {
        // Parcours des noeuds enfant à la recherche de la barre d'outils, qui a un ID variable
        // On ne peut pas utiliser getElementsByClassName qui n'est pas supporté par IE 8 : http://caniuse.com/getelementsbyclassname
        try {
            var oInstanciatedObject = document.getElementById('cke_' + this.name);
            if (oInstanciatedObject) {
                return oInstanciatedObject.querySelector('div[class="cke_top"]');
            }
            else {
                return null;
            }
        }
        catch (e) {
            return null;
        }

    }

    // Redimensionne la barre d'outils à la taille spécifiée, ou en fonction de celle du champ Mémo (utile pour le mode Inline)
    this.resizeToolBar = function (nNewWidth) {
        var oToolbar = this.getToolBarContainer();
        if (oToolbar) {
            if (isNaN(nNewWidth)) {
                // Taille du champ Mémo définie en pixels : on l'utilise directement pour le calcul de la taille
                if (this.config.width.indexOf('%') == -1) {
                    nNewWidth = getNumber(this.config.width) - 16;
                }
                // Taille du champ Mémo définie en pourcentage : on tente de récupérer la taille réelle
                // (calculée en pixels par le navigateur) de l'objet via eTools.getAbsolutePosition();
                else {
                    var realContainerSize = getAbsolutePosition(this.htmlEditor.container.$);
                    if (realContainerSize.w > 0) {
                        nNewWidth = getNumber(realContainerSize.w) - 16;
                    }
                }
            }

            if (!isNaN(nNewWidth) && nNewWidth > 0) {
                oToolbar.style.width = nNewWidth + 'px';
            }
        }
    }

    // Affecte un skin spécifique au composant en fonction de ses modes d'affichage
    this.setSkin = function (strSkin) {
        if (!strSkin || strSkin == '') {
            // TODO: skin spécifique compactMode ?
            if (this.borderlessMode) {
                strSkin = 'eudonet-mini';
            }
            else {
                strSkin = 'eudonet';
            }
        }

        this.skin = strSkin;

        if (this.isHTML) {
            this.htmlConfig.skin = strSkin;
        }
    };

    // Passe le champ Mémo en lecture seule ou en écriture
    this.setReadOnly = function (bReadOnly) {

        this.readOnly = bReadOnly;
        if (this.isHTML) {
            this.htmlConfig.readOnly = bReadOnly;
            this.htmlEditor.setReadOnly(bReadOnly);
        }
        else {
            if (bReadOnly) {
                this.textEditor.setAttribute("readonly", "readonly");
                this.textEditor.setAttribute("ero", "1");
            }
            else {
                this.textEditor.removeAttribute("readonly");
                this.textEditor.setAttribute("ero", "0");
            }
        }
    };

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    // LECTURE ET ECRITURE DE DONNEES
    // Ces méthodes sont appelées, soit pour récupérer du contenu, soit pour le mettre à jour en base
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------

    // Backlog #619 - Récupère la couleur de fond actuellement positionnée sur l'éditeur (indépendamment de son contenu)
    // Attention, il s'agit ici de la couleur de fond du canevas, et pas de celle de l'interface (uiColor sur CKEditor)
    this.getColor = function () {
        var color = "";

        if (this.isHTML) {
            // grapesjs
            if (this.htmlTemplateEditor) {
                var wrapper = this.htmlTemplateEditor.getWrapper();
                if (wrapper && wrapper.getEl()) {
                    color = wrapper.getEl().style.backgroundColor;
                }
            }
            // CKEditor
            if (this.htmlEditor && !this.htmlEditor.isFake && this.htmlEditor.container && this.htmlEditor.container.$) {
                // On récupère d'abord la couleur de l'iframe interne, et à défaut, celle du canevas
                var editorIFrame = this.htmlEditor.container.$.querySelector(".cke_wysiwyg_frame");
                if (editorIFrame)
                    color = editorIFrame.style.backgroundColor;
                if (!color || color == "") {
                    var editorCanvas = this.htmlEditor.container.$.querySelector(".cke_contents");
                    if (editorCanvas)
                        color = editorCanvas.style.backgroundColor;
                }
            }
        }
        // Texte brut
        else if (this.textEditor)
            color = this.textEditor.style.backgroundColor;

        return color;
    }

    // Récupère le message utilisateur (par défaut : nom d'utilisateur et date du jour) à insérer via le bouton dédié, depuis eParamIFrame
    this.getUserMessage = function () {
        // Initialisation
        var strUserMessage = '';
        var now = new Date();

        // MOU/MAB - Récupération de la date
        var day = now.getDate();
        if (day < 10) day = '0' + day;
        var month = now.getMonth() + 1; // le mois commence à 0 en JS 
        if (month < 10) month = '0' + month;
        var year = now.getFullYear();
        var localDate = day + "/" + month + "/" + year;

        // GCH/MOI - Récupération de l'heure au moment de l'appel à la fonction (#22 232)
        var localTime = now.toLocaleTimeString();
        if (CKEDITOR.env.ie) {
            // GCH - Bug IE11 demande #31571 : Insertion intempestive du caractère " ? " dans le champ fusion Date
            // chez Microsoft : https://connect.microsoft.com/IE/feedback/details/863366/ie11-javascript-date-tolocaletimestring-length-incorrect
            // lorsque sur ie11 on appelle toLocaleTimeString il est rajouté des caractères de type marqueur de gauche à droite (numéro 8206)
            // le patch ici est donc de remplacer les 8206 par rien !
            // Backlog #451 - Utilisation d'une fonction centralisée qui remplace ce caractère-là, mais aussi d'autres
            //localTime = localTime.replace(/\u200E/g, "");   //\u200E (8206 in hex)
            localTime = eTools.removeHiddenSpecialChars(localTime);
        }

        // Récupération d'un éventuel formatage du UserMessage paramétré en base
        try {
            strUserMessage = this.paramWindow.GetParam('UserMessage');

            if (!this.isHTML)
                strUserMessage = removeHTML(strUserMessage); // depuis eTools.js
            else {
                // #57407 : Correctif du mauvais encodage du &amp;nbsp; car trop d'impacts si on corrige à la source
                strUserMessage = strUserMessage.replace(/&amp;nbsp;/g, '&nbsp;');
            }
        }
        catch (ex) {
            strUserMessage = '';
        }

        // S'il n'y a pas de formatage en base, on utilise le format par défaut "JJ/MM/AAAA USERNAME :" sans heure
        if (!strUserMessage || typeof (strUserMessage) == 'undefined' || strUserMessage == '') {
            // Internationalisation : on convertit la date seule au format défini dans la base
            localDate = eDate.ConvertBddToDisplay(localDate);

            if (this.isHTML)
                strUserMessage = "<strong>" + localDate + "</strong>&nbsp;:";
            else
                strUserMessage = localDate + " :";
        }
        // Sinon, on utilise le formatage défini en base
        // @ClientDateTime et @ClientDate sont des mots-clés transmis par le serveur, on les remplace par la date et/ou l'heure courante
        // Internationalisation : on convertit le couple date et heure au format défini dans la base
        else {
            strUserMessage = strUserMessage.replace(/@ClientDateTime/g, eDate.ConvertBddToDisplay(localDate + " " + localTime));
            strUserMessage = strUserMessage.replace(/@ClientDate/g, eDate.ConvertBddToDisplay(localDate));
        }

        return strUserMessage;
    };

    // Insère le message utilisateur (par défaut : date et nom)
    this.insertUserMessage = function () {
        if (this.readOnly)
            return;

        var strUserMessage = this.getUserMessage();
        if (this.isHTML) {
            strUserMessage = decode(strUserMessage) + '&nbsp;';
            /*
            L'insertion du code se fait de manière unique sur tous les navigateurs depuis la demande #31 732.
            Auparavant, ça n'était pas le cas. cf. implémentation initiale d'aôût 2012, on utilisait insertAdjacentHTML sur IE qui était incompatible avec la méthode
            CKEditor à l'époque.
            Toutefois, aujourd'hui encore, pour pouvoir faire passer le contenu existant à la ligne et insérer le texte avant, il faut faire l'insertion en 2 fois
            */

            /*
            this.setCursorPosition(0);
            this.htmlEditor.insertHtml();
            this.setCursorPosition(0);
            this.htmlEditor.insertHtml(strUserMessage);
            */

            this.insertData('<br /><br />', true, false, 0);
            this.insertData(strUserMessage, true, false, 0);
        }
        else {
            strUserMessage = strUserMessage + ' ';
            this.setData(strUserMessage + '\n\n' + this.getData());
            this.focus();
            this.setCursorPosition(strUserMessage.length);
        }
    };

    this.parseStringToDebug = function (s) {
        var retour = "";
        for (var i = 0; i < s.length; i++) {
            if (retour != "")
                retour = retour + "$|$";
            retour = retour + "[" + s[i] + "]-[" + s[i].charCodeAt() + "]";
            this.trace("Caractère : [" + s[i] + "] - Code : [" + s[i].charCodeAt() + "]");
        }
        return retour;
    };

    // Ouvre une fenêtre dialog en sélectionnant l'option de visualisation
    this.visualization = function (editor) {

        if (this.readOnly)
            return;
        // MAB/GMA - 2014/01/06

        // Backlog #328
        if (!CKEDITOR.currentInstance)
            return;

        // On indique qu'il faut exécuter du code après l'ouverture de la fenêtre
        CKEDITOR.currentInstance.openDialog('link', function () {
            //On indique que cette ouverture est réalisée depuis un bouton custom (cf. plus bas)
            this.fromVisualization = true;

            // Pour pouvoir sélectionner "Visualisation" dès l'ouverture de la fenêtre dans la combobox "Type de lien",
            // il faut définir l'index de cette combobox lors du focus sur la fenêtre, et non pas avant (show, etc.)
            // car le code interne de CKEditor change lui-même l'élément sélectionné de la combobox pour le positionner sur
            // "URL" lors du focus sur la fenêtre.

            // On redéfinit donc la fonction onFocus de la fenêtre en la complétant (et surtout pas en l'écrasant) afin que
            // le code standard de CKEditor s'exécute dans tous les autres cas où on appelle la fenêtre Lien.
            that.getLinkDialogFocus(this);
        });
    };

    // Ouvre une fenêtre dialog en sélectionnant l'option Champ de fusion
    this.mergeFieldHyperLink = function (editor) {

        if (this.readOnly)
            return;

        if (!CKEDITOR.currentInstance)
            return;

        CKEDITOR.currentInstance.openDialog('link', function () {
            this.fromMergeFieldHyperLink = true;

            that.getLinkDialogFocus(this);
        });
    };

    // Ouvre une fenêtre dialog en sélectionnant l'option de désinscription
    this.unsubscribe = function (editor) {

        if (this.readOnly)
            return;
        // MAB/GMA - 2014/01/06

        // Backlog #328
        if (!CKEDITOR.currentInstance)
            return;

        if (this.toolbarType == "smsing") {
            if (this.readOnly)
                return;

            var htmlLink = "<label/>";
            var link = CKEDITOR.dom.element.createFromHtml(htmlLink);

            link.setHtml("STOP $$STOP$$");

            try {

                this.focus();//pour pouvoir inserer l'element a la position actuelle...

            }
            catch (Ex) {
                alert(Ex);
            }
            this.htmlEditor.insertElement(link);
        }
        else {
            // On indique qu'il faut exécuter du code après l'ouverture de la fenêtre
            CKEDITOR.currentInstance.openDialog('link', function () {
                //On indique que cette ouverture est réalisée depuis un bouton custom (cf. plus bas)
                this.fromUnsubscribe = true;

                // Pour pouvoir sélectionner "Désinscription" dès l'ouverture de la fenêtre dans la combobox "Type de lien",
                // il faut définir l'index de cette combobox lors du focus sur la fenêtre, et non pas avant (show, etc.)
                // car le code interne de CKEditor change lui-même l'élément sélectionné de la combobox pour le positionner sur
                // "URL" lors du focus sur la fenêtre.

                // On redéfinit donc la fonction onFocus de la fenêtre en la complétant (et surtout pas en l'écrasant) afin que
                // le code standard de CKEditor s'exécute dans tous les autres cas où on appelle la fenêtre Lien.
                that.getLinkDialogFocus(this);
            });
        }
    };


    this.disableForExternal = function () {

    };

    // MAB/GMA - 2014/01/09
    // Redéfinit la fonction onFocus de la fenêtre dialog
    this.getLinkDialogFocus = function (myContext) {

        // Backlog #328
        if (!CKEDITOR.currentInstance)
            return;

        // On ne fait la redéfinition qu'une seule fois pour ne pas rajouter du code à chaque fois que l'on appelle la fenêtre.
        // Pour cela, on définit une variable linkDialogFocusCustomized sur l'instance de CKEditor afin de ne le faire que la
        // première fois où l'on rencontre ce code.
        if (!CKEDITOR.currentInstance.linkDialogFocusCustomized) {
            CKEDITOR.currentInstance.linkDialogFocusCustomized = true;
            CKEDITOR.currentInstance.linkDialogFocusFunction = myContext.definition.onFocus;
            myContext.definition.onFocus = (function () {
                // On récupère le code existant de CKEditor dans une variable
                var existingFunction = CKEDITOR.currentInstance.linkDialogFocusFunction;
                // Et on l'appelle dans une fonction anonyme avec return et apply (prototype JavaScript) afin que le contexte soit
                // précisé/passé au code existant, et que "this" corresponde bien à notre objet CKEDITOR.dialog tel qu'attendu par le
                // code existant de CKEditor (autrement, dans une fonction anonyme, "this" correspondrait à l'objet Window).
                // http://stackoverflow.com/questions/9134686/adding-code-to-a-javascript-function-programatically
                return function () {
                    existingFunction.apply(this, arguments);
                    // Si l'appel de la fenêtre a été effectué depuis la fonction unsubscribe de eMemoEditor...
                    if (this.fromVisualization || this.fromUnsubscribe || this.fromMergeFieldHyperLink) {


                        var selectLinkType = this.getContentElement('info', 'linkType');
                        var mySel = selectLinkType.getInputElement().$;

                        // On récupère la position de la valeur à afficher dans la combobox selon la provenance de l'appel
                        var myIndex = 0;
                        var myIndexOp;

                        if (this.fromMergeFieldHyperLink) {
                            myIndexOp = mySel.querySelector("option[value='mergehyperlink']");
                            if (!myIndexOp)
                                myIndex = 0;
                            else
                                myIndex = myIndexOp.index;

                            this.fromMergeFieldHyperLink = false;
                        }

                        if (this.fromVisualization) {
                            myIndexOp = mySel.querySelector("option[value='visu']");
                            if (!myIndexOp)
                                myIndex = 0;
                            else
                                myIndex = myIndexOp.index;

                            this.fromVisualization = false;
                        }

                        if (this.fromUnsubscribe) {
                            myIndexOp = mySel.querySelector("option[value='unsub']");
                            if (!myIndexOp)
                                myIndex = 0;
                            else
                                myIndex = myIndexOp.index;

                            this.fromUnsubscribe = false;
                        }

                        // On peut exécuter ce code, mais on passe tout de suite la propriété à false afin que les autres appels à
                        // cette fenêtre n'aient pas à le faire explicitement (la fenêtre pouvant être appelée par des moyens inconnus)

                        // On récupère la combobox "Type de lien"

                        // On accède à l'élément du DOM avec getInputElement() et on change l'élément sélectionné
                        selectLinkType.getInputElement().$.selectedIndex = myIndex;
                        // Et on déclenche manuellement le onChange() de CKEditor sur cet élément car la modification de l'index sur
                        // l'objet du DOM ne déclenche pas cette fonction.
                        selectLinkType.onChange();



                        //GCH #33296 : Lors du clic sur le lien de visu/unsub, désormais il n'y a plus les options de tracking
                        var oDialog = this;// CKEDITOR.currentInstance.getDialog();
                        CKEDITOR.ManageTrackingButtons(oDialog, selectLinkType.items[myIndex][1]);

                        /*
                        if (that.externalTrackingEnabled) {

                            // On accède à l'élément du DOM avec getInputElement() et on change l'élément sélectionné
                            var mySel = selectLinkType.getInputElement().$;
                            var myOpt = mySel.options[mySel.selectedIndex];

                            if (myOpt.value == "unsub")
                                mySel.disabled = true;

                            if (myOpt.value == "visu")
                                mySel.selectedIndex = 0;

                            for (var nMyTrak = mySel.options.length - 1; nMyTrak >= 0; nMyTrak--) {
                                if (mySel.options[nMyTrak].value == "visu") {

                                    mySel.options[nMyTrak].disabled = true;
                                    mySel.options[nMyTrak].style.color = "light-grey";
                                    break;
                                }
                            }
                        }
                        */

                    }




                };
            }());
        }
    }

    // Ouvrir la liste des formulaires
    this.openFormular = function (formulartype) {
        ShowFormularList(that._tab, that._nParentFileId, that, formulartype);
    }

    // Insère la signature utilisateur 
    this.insertUserSignature = function () {

        if (this.readOnly)
            return;

        var strUserSignature = '';
        try {
            strUserSignature = this.paramWindow.GetParam('UserSignature');
            if (!this.isHTML)
                strUserSignature = removeHTML(strUserSignature); // depuis eTools.js
        }
        catch (ex) {
            strUserSignature = '';
        }


        if (this.isHTML) {

            strUserSignature = decode(strUserSignature) + '&nbsp;';
            /*
            Sur IE, il est difficile de positionner le curseur au tout début du champ pour réaliser une insertion si le début du contenu commence par une balise HTML
            Exemple : <b><u>Mon texte</u></b> : appeler this.setCursorPosition(0) positionnera le curseur après les balises : <b><u>|Mon texte</u></b>
            Ce qui provoquera l'insertion à l'intérieur des balises, et non devant. Pour éviter ce phénomène, il faudrait insérer un espace avant le message, ce qui se voit ;
            On utilise donc un algorithme spécifique pour IE, qui utilise insertAdjacentHTML pour incruster le code juste après la balise <body> de l'iframe CKEditor
            cf. correctifs réalisés sur RTEF en v7
            */
            if (CKEDITOR.env.ie) {
                var oObj = this.getMemoBody();
                oObj.insertAdjacentHTML("afterBegin", "<br /><br />" + strUserSignature);
                this.setCursorPosition(0); // removeHTML se trouve dans eTools.js
            }
            else {
                /*
                Avec les autres navigateurs, le support de insertAdjacentHTML est récent et n'est pas garanti sur toutes les versions (ex : Firefox 8 minimum).
                On utilise donc l'API de CKEditor pour gérer l'insertion, vu que sur ces autres navigateurs, le positionnement du curseur se fait bien en-dehors des balises.
                Pour pouvoir faire passer le contenu existant à la ligne et insérer le texte avant, il faut faire l'insertion en 2 fois
                */


                this.setData(this.getData() + "<br /><br />" + strUserSignature);
                this.setCursorPosition(0);
            }
        }
        else {
            strUserSignature = strUserSignature + ' ';
            this.setData(this.getData() + '\n\n' + strUserSignature);
            this.focus();
            this.setCursorPosition(0);

        }
    };

    // Ajouter des annexes
    this.uploadFiles = function () {
        // On affiche la liste des pj que si on vient depuis un signet ou 
        // on ouvre une campagne existante
        // TODOMAB : depuis les modèles de mails unitaires également ?
        if (this.pjListEnabled && this.toolbarType == "mailing" && oMailing != null && typeof (oMailing) == "object") {
            showPJFromTpl('tplmailing', null, true);
        }
        else if (this.toolbarType == "mailingtemplate") {
            var fileDiv = document.getElementById("fileDiv_" + nGlobalActiveTab);
            if (fileDiv)
                showTemplatePJList(nGlobalActiveTab, getAttributeValue(fileDiv, "fid"), "mailingtemplate");
        } else {
            showAddPJ(that.name,undefined,{ width:650 , height:550 });
        }
    };

    // Recupère les infos de la pj et insere l'url de celle-ci dans ce mémo 
    this.LoadPjInfoAndInsertPjLink = function (pjId) {

        var oUpdater = new eUpdater("mgr/ePjManager.ashx", 0);

        oUpdater.addParam("pjid", pjId, "post");
        oUpdater.addParam("action", "info", "post");

        oUpdater.ErrorCallBack = launchInContext(oUpdater, oUpdater.hide);

        setWait(true);
        oUpdater.send(function (oDoc) {

            that.insertPJLink({

                id: getXmlTextNode(oDoc.getElementsByTagName("id")[0]),
                name: getXmlTextNode(oDoc.getElementsByTagName("name")[0]),
                src: getXmlTextNode(oDoc.getElementsByTagName("src")[0])
            });
            setWait(false);
        });
    }

    //inserer le lien annexe
    this.insertPJLink = function (pj) {

        // TODOMAB : également pour les modèles de mails unitaires ?
        if (this.toolbarType == "mailing" || this.toolbarType == "mailingtemplate") {
            this.insertLink("lnk", pj.src, pj.id, pj.name, true);
        }
        else
            this.insertLink("", pj.src, pj.id, pj.name, false);
    };

    this.insertFormularField = function (id, name) {
        if (this.toolbarType != "smsing")
          this.insertLink("formu", "", id, name, true, id);
        else
         this.insertLinkFormuSMS(name, true, id);
    };



    // sFormName : nom du fornulaire
    // bTrackEnabled (tracking actif ou non )
    // sFormId : id du formulaire
    this.insertLinkFormuSMS = function (  sFormName, bTrackEnabled,sFormId) {
 
        
        if (this.readOnly)
            return;

        var htmlLink = "<label/>";
        var link = CKEDITOR.dom.element.createFromHtml(htmlLink);

        setAttributeValue(link, "contenteditable", "false");
        setAttributeValue(link, "ednc", "formu");
        setAttributeValue(link, "ednl", "formu");
        setAttributeValue(link, "ednt", (bTrackEnabled) ? "on" : "off");
        setAttributeValue(link, "ednn", sFormName);
        setAttributeValue(link, "edne", sFormId);

        link.setHtml('{' + sFormName + '}');

        try { 
             
          this.focus();//pour pouvoir inserer l'element a la position actuelle...
          
        }
        catch (Ex) {
            alert(Ex);
        }
        this.htmlEditor.insertElement(link);
    };

    // sLinkType : (lien classique : lnk - lien formulaire : formu)
    // sLink : url du lien (si vide est remplacé par #)
    // sLink : nom du lien
    // bTrackEnabled (tracking actif ou non )
    // sEdne
    this.insertLink = function (sLinkType, sLink, sLinkId, sLinkName, bTrackEnabled, sEdne) {
 
        if (typeof (sLink) == "undefined" || sLink == "") sLink = "#";
        if (this.readOnly)
            return;
        var htmlLink = "<a />";
        var link = CKEDITOR.dom.element.createFromHtml(htmlLink);
        setAttributeValue(link, "href", sLink);
        if (sLinkType != "") {
            setAttributeValue(link, "ednc", sLinkType);
            setAttributeValue(link, "ednt", (bTrackEnabled) ? "on" : "off");
            setAttributeValue(link, "ednl", sLinkType);
            if (sLinkName != "")
                setAttributeValue(link, "ednn", sLinkName);
            if (sEdne && sEdne != "")
                setAttributeValue(link, "edne", sEdne);
        }
        else {
            var strMergeFieldId = sLinkId + '_' + (new Date).getTime();
            link.id = strMergeFieldId;
        }

        try {

            var oElem = this.htmlEditor.getSelection();
            if (oElem)
                oElem = oElem.getStartElement();

            if (oElem && oElem.$.tagName.toLowerCase() == "img") {

                var s = oElem.getOuterHtml();
                link.setHtml(s);
                this.focus(); //pour pouvoir inserer l'element a la position actuelle...
                //oElem.appendTo(link);
                this.htmlEditor.insertElement(link);

                return;
            }

            if (this.htmlEditor.getSelection() == null || this.htmlEditor.getSelection().getSelectedText().length == 0) {
                //pas de selection, le nom de la pj sera le nom du lien
                link.setHtml(sLinkName);
                this.focus();//pour pouvoir inserer l'element a la position actuelle...
            }
            else {
                //on recupere l'element selectioné
                var frag = this.htmlEditor.getSelection().getRanges()[0].extractContents();
                frag.appendTo(link);
            }
        }
        catch (Ex) {
            alert(Ex);
        }
        this.htmlEditor.insertElement(link);
    };

    // Insère une image à l'intérieur du champ Mémo
    this.insertImage = function (bSetDialogURL) {
        // Backlog #315 : récupération de l'URL pour l'insérer dans la fenêtre Image de CKEditor
        if (bSetDialogURL)
            doGetImage(this, 'MEMO_SETDIALOGURL');
        // Sinon, insertion directe de l'image dans l'éditeur
        else
            doGetImage(this, 'MEMO');
    };

    // Insère une feuille de style CSS personnalisée à l'intérieur du champ Mémo
    this.insertCSS = function () {
        // Ajout des marges texte brut : 12 et 50
        var lWidth = top.document.body.scrollWidth - 150 - 12;
        var lHeight = top.document.body.scrollHeight - 150 + 50;
        if (lHeight < 450)
            lHeight = 450;

        if (lWidth < 600)
            lWidth = 600;

        this.childDialog = new eModalDialog(
            top._res_6078, // Titre
            0,                          // Type
            "eMemoDialog.aspx",              // URL
            lWidth,                        // Largeur
            lHeight);                       // Hauteur

        this.childDialog.ErrorCallBack = launchInContext(this.childDialog, this.childDialog.hide);

        // Ajustement de la taille du champ en fonction de sa nature Texte brut/HTML
        if (!this.isHTML) {
            lWidth = lWidth - 12;
            lHeight = lHeight + 50;
        }

        // Récupération de la CSS actuelle, ou création de la propriété
        var strCustomCSS = '';
        if (this.getCss())
            strCustomCSS = this.getCss();
        else
            this.setCss(strCustomCSS);

        this.childDialog.addParam("Title", "CSS", "post"); // TODO: titre
        if (typeof (this.parentFrameId) != 'undefined')
            this.childDialog.addParam("ParentFrameId", this.parentFrameId, "post");

        this.childDialog.addParam("DescId", "0", "post"); // DescID du champ concerné par l'édition (non applicable ici)
        this.childDialog.addParam("EditorJsVarName", this.jsVarName, "post"); // Nom de la variable objet eMemoEditor instanciée (pour le fonctionnement de certaines méthodes
        this.childDialog.addParam("Value", encode(strCustomCSS), "post"); // valeur initiale du champ Mémo sur la fenêtre fille : la CSS personnalisée actuelle (si existante)
        this.childDialog.addParam("width", lWidth - 12, "post"); // 12 : marge intérieure par rapport au conteneur de l'eModalDialog
        this.childDialog.addParam("height", lHeight - 150, "post"); // 150 : espace réservé à la barre de titre + boutons de l'eModalDialog
        this.childDialog.addParam("IsHTML", "0", "post"); // le champ Mémo affiché ne doit servir qu'à la saisie de code HTML. On l'affiche donc en texte brut

        this.childDialog.addParam("ReadOnly", (this.readOnly && !this.uaoz ? "1" : "0"), "post");
        // Paramètres spécifiques à l'upload de fichiers
        this.childDialog.addParam("ToolbarType", "css", "post"); // sur le champ mémo réservé à la saisie de CSS, barre d'outils spécifique sans bouton "Insérer infos utilisateur"
        this.childDialog.addParam("UploadContentEnabled", "1", "post"); // afficher le formulaire permettant d'uploader un fichier
        this.childDialog.addParam("UploadContentLabel", top._res_6078, "post"); // libellé à afficher à côté du champ d'upload
        this.childDialog.addParam("UploadContentFileFilter", "css", "post"); // extensions acceptées pour l'upload de fichiers
        this.childDialog.addParam("UploadContentLimit", "1048576", "post"); // limite de taille des fichiers uploadés en octets : 1 Mo
        this.childDialog.addParam("UploadContentAppend", "1", "post"); // le contenu est ajouté à l'existant
        this.childDialog.show();

        var dialog = this.childDialog;

        //MOU #38642 Dans le context d'ajout de css, on redemensionne la fenetre pour tenir compte des la partie header : bouton file, option d'upload
        this.childDialog.onIframeLoadComplete = function () { dialog.getIframe().eMemoDialogEditorObject.resize(lWidth - 5, lHeight - 280); };

        this.childDialog.addButton(top._res_29, cancelCSSMemoDialog, "button-gray", this.jsVarName, "cancel"); // Annuler
        this.childDialog.addButton(top._res_28, validateCSSMemoDialog, "button-green", this.jsVarName, "ok"); // Valider

    };

    this.injectCSS = function (sNewCSSValue, isNativeCSS, appendIfExists) {
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
                        addCSSText(sNewCSSValue[i], editorFrame.contentDocument, "eME_extCss_" + this.name, appendIfExists ? appendIfExists : false);
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

    //Message utilisateur pour le champ memo
    this.insertSpecialMergeField = function (spMergeFieldsComboBox, value, readWrite) {
        if (value == "") {
            return;
        }

        var sHtml = '';
        var sText = spMergeFieldsComboBox._.items[value];

        // Ordre des valeurs séparées par ;
        // 0 : class css
        // 1 : ednfieldtype
        // 2 : ednfielddescid
        // 3 : ednfieldname
        // 4 : contenteditable  

        if (this.addStartSpaceWithMergeFields)
            sHtml = '&nbsp;';

        var aField = value.split("\;");

        var strFieldLabel = '{' + sText + '}';
        var labelId = aField[2] + '_' + (new Date).getTime(); //id unique

        // 3 : ednfieldname
        var style = "";
        if (aField[3] == 'pagebreak')
            style = "style=\"page-break-after:always;\"";

        sHtml += "<label " + style +
            " class='" + aField[0] +
            "' contenteditable='" + aField[4] +
            "' ednfielddescid='" + aField[2] +
            "' ednfieldname='" + aField[3] +
            "' ednfieldtype='" + aField[1] +
            "' id='" + labelId + "'>" + strFieldLabel + "</label>";

        /*
        this.focus()

        if (CKEDITOR.env.ie) {
            this.htmlEditor.insertHtml(sHtml);
        }
        else {
            sHtml += '&nbsp;';
            var oLabel = CKEDITOR.dom.element.createFromHtml(sHtml);
            this.htmlEditor.insertElement(oLabel);
        }
        */

        // 68 132 - Passage par insertData qui prend en charge les différences entre navigateurs, et plusieurs types d'éditeurs (CKEditor, grapesjs, texte...)
        // Backlog #320 : on passe le dernier paramètre bSkipHTMLTemplateEditor à true afin d'effectuer l'insertion directement via CKEditor, comme avant l'implémentation
        // de grapesjs, sans passer par grapesjs, même si l'éditeur utilisé est grapesjs. Ceci, car l'appel de execCommand("insertHTML") via grapesjs semble provoquer
        // l'insertion des champs de fusion en double dans de nombreux cas
        var bInsertRawHTML = CKEDITOR.env.ie;
        var bFocusBeforeInsert = true;
        // TOCHECK: Dans ce cas de figure, on ajoute l'espace insécable sur les autres navigateurs qu'IE, et pas l'inverse (cf. insertMergeField)
        // #72 278 - Cette condition étant remplacée par la nouvelle variable this.addEndSpaceWithMergeFields, on le fait donc explicitement ici si ça n'a pas été fait auparavant
        //if (bInsertRawHTML == false)
        if (!this.addEndSpaceWithMergeFields)
            sHtml += '&nbsp;';
        this.insertData(sHtml, bInsertRawHTML, true, -1, true);

        spMergeFieldsComboBox.setValue("");
    };

    // #29 319 - Affichage de tous les raccourcis utilisables dans une fenêtre séparée (CKEditor)
    // Backlog #354 - Affichage d'un tutorial d'utilisation (grapesjs)
    this.helpTutorial = function (open) {
        if (open && this.helpUrl.indexOf("./") === 0) {


            var url = this.helpUrl.replace("<lang>", this.language);
            var fallbackUrl = this.helpUrl.replace("<lang>", "fr")


            var ctx = this
            var fctCall = function (myurl) {

                ctx.helpDialog = new eModalDialog(
                    top._res_6187, // Titre
                    0,                          // Type
                    "blank",   // Url Blank : on va charger l'url via le src de l'iframe pour éviter les pb cors (si serveur différent)/405 (appel d'une statique via pos)
                    800,                        // Largeur
                    600);                       // Hauteur


                ctx.helpDialog.ErrorCallBack = launchInContext(ctx.helpDialog, ctx.helpDialog.hide);
                ctx.helpDialog.show();
                ctx.helpDialog.MaxOrMinModal();
                ctx.helpDialog.getIframeTag().src = myurl
                ctx.helpDialog.addButton(top._res_30, ctx.helpDialog.hide, "button-gray", this.jsVarName, top._res_30); // Annuler
            }


            if (typeof fetch != "function") {
                fctCall(url)
            }
            else {


                // Utilisation de fetch pour utiliser l'url de fallback si besoin
                //fetch(url)
                //    .then(resp => {
                //        if (resp.status == 200) {
                //            return resp;    // on renvoie telquel
                //        }
                //        else {
                //            return fetch(fallbackUrl); // fallback fr 
                //        }
                //    })
                //    .then(resp => {
                //        if (resp.status == 200)
                //            return resp.url
                //        else
                //            throw new Error("Aide Indisponible")
                //    })
                //    .then(url => {
                //        fctCall(url)
                //    }
                //    );

                //SHA : correction bug #71 668

                fetch(url)
                    .then(function (resp) {
                        if (resp.status == 200) {
                            return resp; // on renvoie telquel
                        } else {
                            return fetch(fallbackUrl); // fallback fr 
                        }
                    }).then(function (resp) {
                        if (resp.status == 200)
                            return resp.url;
                        else
                            throw new Error("Aide Indisponible");
                    }).then(function (url) {
                        fctCall(url);
                    }).catch(function (z) {
                        eAlert(0, top._res_72, top._res_6735);
                    })
                    ;
            }

        }
        else if (!open && this.helpWindow) {
            this.helpDialog.hide();
            this.helpDialog = null;
        }
    };

    this.help = function () {
        if (this.htmlEditor && this.htmlEditor.lang.xrmHelpContents && this.htmlEditor.lang.xrmHelpContents != '') {
            // On masque la barre d'outils, afin qu'elle ne reste pas affichée au-dessus de l'eAlert
            // REMARQUE : On pourrait utiliser blur() pour faire sortir le curseur du champ, mais ce n'est pas forcément utile ici vu que l'on maîtrise à quel moment on réaffichera la
            // barre d'outils, contrairement au mécanisme similaire implémenté sur l'évènement onScroll (cf. implémentation dans on('instanceReady') plus bas)
            //this.blur();
            this.hideShowToolBar(false);
            var onOkFct = (function (eMemoEditorTargetObject) {
                return function () {
                    // Le fait de masquer la barre d'outils plus haut nécessite de la réafficher explicitement ensuite, car CKEditor ne le fait pas de lui-même (il ne modifie pas style.display
                    // en interne, seulement le positionnement lors de la saisie ou du focus)
                    eMemoEditorTargetObject.hideShowToolBar(true);
                    // Puis on remet le focus dans le champ, qui est forcément perdu dès lors que l'on cliquera sur l'eAlert pour la fermer (bouton Fermer ou X)
                    // Sans ce focus, la barre d'outils serait de nouveau masquée, cette fois par CKEditor lui-même, vu que le curseur ne serait plus dedans
                    eMemoEditorTargetObject.focus();
                }
            })(this);
            // Puis on affiche la popup d'aide, en câblant sur le bouton OK un appel à la fonction qui réactivera la barre d'outils
            eAlert(3, top._res_6187, top._res_6187, this.htmlEditor.lang.xrmHelpContents, 500, 500, onOkFct);
        }
    };

    // Place le curseur à l'intérieur du textarea
    this.focus = function () {
        if (this.isHTML && this.htmlEditor && this.htmlEditor.focus) {
            this.htmlEditor.focus(); // appel de la fonction de CKEditor
        }
        else {
            if (this.textEditor) {
                this.textEditor.focus();
            }
        }
    };

    // Sort le curseur de l'intérieur du textarea (et déclenche donc la sauvegarde si updateOnBlur est à true)
    this.blur = function () {
        if (this.isHTML) {
            if (this.htmlEditor && this.htmlEditor.container && this.htmlEditor.container.$ && typeof (this.htmlEditor.container.$.blur) == 'function') {
                this.htmlEditor.container.$.blur(); // CKEditor ne prévoit pas de méthode blur. On déclenche donc la fonction système sur le textarea interne
            }
            else {
                window.focus(); // à défaut, on met le focus sur la fenêtre
            }
        }
        else {
            if (this.textEditor) {
                this.textEditor.blur();
            }
        }
    };

    // Prépare la valeur du champ Mémo pour sa mise à jour en base
    // S'assure que la valeur ait réellement changé avant d'envoyer en base
    // newValueFromCancel : nouvelle valeur à valider (venant des dernières valeurs saisies)
    this.validate = function (newValueFromCancel) {
        this.trace("Validation de la valeur du champ Mémo pour mise à jour en base");

        var oldValue = this.value;

        var memoData = this.value;


        if (typeof (newValueFromCancel) != "undefined") {
            newValue = newValueFromCancel;

        }
        else {
            memoData = this.getData();
            newValue = memoData;

            try {
                if (oldValue != newValue) {
                    var nodeSrcElement = this.getSrcElement();
                    var headerElement = document.getElementById(nodeSrcElement.getAttribute("ename"));
                    //Champ Obligatoire
                    //Demande #75 678
                    var obligat = getAttributeValue(nodeSrcElement, "obg") == "1"
                    if (obligat && newValue == '') {

                        eAlert(0, top._res_372, top._res_373.replace('<ITEM>', getAttributeValue(headerElement, "lib")));
                        this.setData(oldValue);
                        return;
                    }
                    if (getAttributeValue(headerElement, "cclval") == "1")
                        LastValuesManager.addValue(nodeSrcElement.id, null, getAttributeValue(headerElement, 'did'), getAttributeValue(headerElement, "lib"), oldValue, oldValue);
                }
            }
            catch (e) {

            }
        }


        if (!this.updateOnBlur && !this.uaoz)
            return;

        var bHasBeenModified = true;
        if (this.isHTML) {
            if (this.htmlEditor && !this.htmlEditor.isFake) {
                // vérification via la méthode interne de CKEditor qui compare la valeur initiale du composant avec celle éditée sur l'iframe interne de CKEditor
                bHasBeenModified = this.htmlEditor.checkDirty();
                if (this.debugMode && !bHasBeenModified) {
                    this.trace("L'éditeur HTML indique que sa valeur n'a pas été modifiée.");
                }
            }
            else {
                // sinon, pour les objets eMemoEditor instanciés sans affichage de CKEditor dans la page HTML, comparaison directe
                bHasBeenModified = newValue != oldValue;
                if (this.debugMode && !bHasBeenModified) {
                    this.trace("La comparaison de la valeur initiale avec la valeur actuelle indique que le contenu (HTML) n'a pas été modifié.");
                }
            }
        }
        else {
            bHasBeenModified = newValue != oldValue;
            if (this.debugMode && !bHasBeenModified) {
                this.trace("La comparaison de la valeur initiale avec la valeur actuelle indique que le contenu n'a pas été modifié.");
            }
        }

        // Si le contenu du champ a changé, ou si le dernier appel à update() a échoué, on envoie une demande de MAJ en base
        if (bHasBeenModified || this.waitingForUpdate) {
            this.trace("Envoi de la demande de mise à jour en base");

            // Demandes #43 882 et #49 341 : IE semble parfois s'emmêler les pinceaux entre les différents évènements (onblur/onfocus), ce qui peut provoquer un blocage
            // de la fenêtre Plein Ecran lors de la sauvegarde. Pour éviter cela, on empêche la sauvegarde des notes depuis la fenêtre Plein Ecran sous IE (correctif
            // #43 882) mais en s'assurant, au préalable :
            // - qu'on soit bien sous IE (isIE)
            // - que la sauvegarde soit bien activée à la sortie du champ (updateOnBlur) pour qu'elle puisse être effectuée lorsqu'on valide la fenêtre Plein Ecran (qui remet
            // alors le curseur dans le champ Mémo parent, en comptant justement sur le fait que la sauvegarde soit assurée à sa sortie du champ)
            // - que l'on traite bien le cas d'un champ Mémo affiché en fenêtre Plein Ecran (isBeingZoomed)
            // - qu'on ait pas explicitement demandé à déclencher la sauvegarde depuis la fenêtre Plein Ecran (uaoz = Update Allowed On Zoom = cas du champ Mémo de la
            // fiche Event liée lors de l'édition d'une fiche Planning en fenêtre popup - cf. #49 341)
            var browser = new getBrowser();
            var isIE = browser.isIE;
            var bPreventMultipleUpdatesOnIE = (isIE && this.updateOnBlur && this.isBeingZoomed && !this.uaoz);
            if (!bPreventMultipleUpdatesOnIE)
                this.update(newValue, oldValue);
            else
                this.trace("La mise à jour en base n'est pas effectuée sous Internet Explorer depuis la fenêtre Plein Ecran, pour éviter un blocage de ce navigateur en raison de multiples tentatives de mises à jour. Le contenu sera mis à jour lorsque le curseur sortira du champ Mémo situé sur la fenêtre principale/parente.");

            this.isBeingZoomed = false;
        }
        else {
            this.trace("La mise à jour en base n'a pas été envoyée");
        }

        // #47 223 : on réautorise le changement de signet éventuellement interdit au focus sur le champ
        if (typeof (setPreventLoadBkmList) == 'function')
            setPreventLoadBkmList(false);
    };

    // Déclenche la mise à jour en base du contenu du champ en faisant un appel à eEngineUpdater
    this.update = function (newValue, oldValue) {
        // On indique à l'éditeur qu'une mise à jour du contenu en base a été demandée.
        // Ainsi, si la MAJ échoue pour une raison quelconque (sortie de fonction ou autre), ce booléen indiquera à la fonction validate()
        // de ne pas vérifier si le contenu du champ a changé, ce qui forcera l'envoi d'une nouvelle demande de MAJ en base
        this.waitingForUpdate = true;

        // Récupération des paramètres à passer à l'engine pour la mise à jour : descId, fileId, etc.
        // S'ils n'ont pas été paramétrés directement sur l'objet eMemoEditor, on recherche ces informations sur les éléments HTML de la page
        var descId = this.descId;
        var fileId = this.fileId;

        this.trace("DescID du champ à mettre à jour : " + this.descId);
        this.trace("FileID du fichier à mettre à jour : " + this.fileId);


        if ((!descId || descId == '') || ((!fileId || fileId == '') && !this.disc)) {
            this.trace("Un des deux paramètres n'est pas renseigné. Détection de ces paramètres via le contexte de la page...");

            var nodeSrcElement = this.getSrcElement();
            if (!nodeSrcElement) {
                this.trace("L'élément source contenant les informations n'a pas été trouvé sur la page. La mise à jour ne sera pas effectuée.");
                return;
            }
            else {
                this.trace("Elément source trouvé : " + nodeSrcElement.id + ". Poursuite de l'analyse...");
            }

            this.trace("Détection du FileID....");
            fileId = GetFieldFileId(nodeSrcElement.id);
            // Récupération du fileId autrement, notamment pour la mise à jour du champ Mémo en signet
            if (!fileId || fileId == '') {
                fileId = getAttributeValue(document.getElementById("fileDiv_" + nGlobalActiveTab), "fid");
                this.trace("La recherche du FileID à partir de l'élément source n'a rien donné. Recherche en fonction de fileDiv (mode signet)...");
            }

            if (fileId == '') {
                this.trace("La recherche du FileID à partir de l'élément fileDiv n'a rien donné. La mise à jour ne sera pas effectuée.");
                return;
            }
            else {
                this.trace("FileID détecté dans le contexte de la page : " + fileId);
            }

            this.trace("Détection du DescID....");

            var headerElement = document.getElementById(nodeSrcElement.getAttribute("ename"));
            if (!headerElement) {
                this.trace("L'élément d'en-tête contenant les informations de DescID n'a pas été trouvé sur la page. La mise à jour ne sera pas effectuée.");
                return;
            }
            descId = getAttributeValue(headerElement, 'did')

            if (descId == '') {
                this.trace("La recherche du DescID à partir de l'élément en-tête n'a rien donné. La mise à jour ne sera pas effectuée.");
                return;
            }
            else {
                this.trace("DescID détecté dans le contexte de la page : " + descId);
            }
        }

        this.trace("Initialisation du moteur de mise à jour...");
        var eEngineUpdater = new eEngine();
        eEngineUpdater.Init();

        eEngineUpdater.AddOrSetParam('fldEditorType', 'memoEditor');
        eEngineUpdater.AddOrSetParam('catNewVal', true);
        eEngineUpdater.AddOrSetParam('jsEditorVarName', this.jsVarName);
        eEngineUpdater.AddOrSetParam('fileId', fileId);

        // HLA - On averti qu'on est en sorti de champs - Dev #45363
        eEngineUpdater.AddOrSetParam('onBlurAction', '1')

        eEngineUpdater.AddOrSetParam('fromparent', that.fromParent ? "1" : "0");

        var fld = new fldUpdEngine(descId);
        fld.newValue = newValue;
        fld.newLabel = newValue;

        eEngineUpdater.AddOrSetField(fld);

        if (this.disc) {
            eEngineUpdater.AddOrSetParam('tab', getTabDescid(descId));

            var fldPrt = new fldUpdEngine(nGlobalActiveTab);
            fldPrt.newValue = GetCurrentFileId(nGlobalActiveTab);
            eEngineUpdater.AddOrSetField(fldPrt);
        }

        if (this.uaoz) {
            var oFileModalDialog = null;
            if (parent.eModFile)
                oFileModalDialog = parent.eModFile;
            //else if (parent.tplFileModal)                     //tplFileModal est remplacé par eModFile
            //    oFileModalDialog = parent.tplFileModal;

            if (oFileModalDialog != null) {
                eEngineUpdater.ModalDialog = { oModFile: oFileModalDialog, modFile: oFileModalDialog.getIframe(), pupClose: false };
            }
        }

        // Mode
        this.trace("Moteur de mise à jour initialisé. Envoi de la demande de mise à jour...");

        //Comme dans eFieldEditor, après validation du champ mémo en blur on click sur l'element actif.
        afterValidate = (
            function () {
                return function (params) {

                    //#58086 : Pas de fin de chargement lorsqu'on modifie un champ note de Planning
                    setWait(false);

                    if ((document.activeElement)) {
                        var bIsTablet = false;
                        try {
                            if (typeof (isTablet) == 'function')
                                bIsTablet = isTablet();
                            else if (typeof (top.isTablet) == 'function')
                                bIsTablet = top.isTablet();
                        }
                        catch (e) {

                        }

                        // if (!bIsTablet)
                        //   document.activeElement.click();
                    }
                }
            }
        )();


        if (typeof (afterValidate) == 'function' && this.updateOnBlur)
            eEngineUpdater.SuccessCallbackFunction = afterValidate;

        eEngineUpdater.UpdateLaunch();



        // On assigne à présent la nouvelle valeur mise à jour en tant que valeur initiale du champ Mémo, qui sera utilisée par la
        // fonction validate() comme point de comparaison pour savoir si le contenu a été modifié depuis la dernière tentative de MAJ
        this.value = newValue;
        this.virtualTextAreaValue = newValue;
        if (this.isHTML && this.htmlEditor && !this.htmlEditor.isFake) {
            this.htmlEditor.resetDirty();
        }
        if (this.debugLevel > 1)
            this.trace("La nouvelle valeur initiale du champ Mémo est désormais la suivante : " + newValue);

        // Et on indique que la demande de mise à jour a été envoyée
        this.waitingForUpdate = false;
    };

    this.flagAsEdited = function (flagAsEdited, noEdit) {
        if (typeof (noEdit) == 'undefined' || !noEdit || noEdit == '')
            var editedClass = "eFieldEditorEdited";
        else
            var editedClass = "eFieldEditorNoEdited";

        var flagEditIFrame = document.getElementById(this.name);
        if (this.isHTML) {
            flagEditIFrame = document.getElementById('cke_contents_' + this.name);
        }

        if (!flagEditIFrame)
            return;

        // Etape 1 : on matérialise la fin de l'édition
        if (flagAsEdited) {
            flagEditIFrame.setAttribute('class', editedClass);

            // Utilisation d'un timer pour faire disparaître l'effet au bout de X secondes
            window.setTimeout(function () { if (that) that.flagAsEdited(false, ''); }, 500);
        }
        // Etape 2 : on restaure l'aspect visuel d'origine
        else {
            //flagEditIFrame.setAttribute('class', 'eME');
            flagEditIFrame.setAttribute('class', 'eMemoEditor'); // #54107
            this.resetOrDestroy();
        }
    };

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    // AFFICHAGE ET COMPORTEMENTS VISUELS
    // Ces méthodes permettent de gérer l'affichage du champ Mémo
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------

    // Affiche le champ Notes en mode Zoom dans une fenêtre modale ou agrandit le champ Mémo sur sa propre fenêtre
    this.switchFullScreen = function (bUseNewWindow, bGetValueFromDB) {
        this.trace("Ouverture du champ Mémo en plein écran...");
        if (bUseNewWindow) {
            this.trace("Ouverture dans une nouvelle fenêtre de zoom...");

            var oWinSize = top.getWindowSize();

            lWidth = Math.round(oWinSize.w * 0.95);
            //GCH commenté (et utilisation de la ligne ci-dessus) car dans le cas ou les signet ont un scroll et que l'on ajoute depuis un signet, la fenêtre et décalée voir non visible.
            //lWidth = top.document.body.scrollWidth - 150; //largeur SUR TOP pour que la dimension soit celle de la page principale et ne soit pas plus petite dans les frame

            lHeight = Math.round(oWinSize.h * 0.95);
            //GCH commenté (et utilisation de la ligne ci-dessus) car dans le cas ou les signet ont un scroll et que l'on ajoute depuis un signet, la fenêtre et décalée voir non visible.
            //lHeight = top.document.body.scrollHeight - 150; //hauteur SUR TOP pour que la dimension soit celle de la page principale et ne soit pas plus petite dans les frame           

            this.childDialog = new eModalDialog(
                this.title, // Titre
                0,                          // Type
                "eMemoDialog.aspx",              // URL
                lWidth,                        // Largeur
                lHeight);                       // Hauteur

            // Ajustement de la taille du champ en fonction de sa nature Texte brut/HTML
            if (!this.isHTML) {
                lWidth = lWidth - 12;
                lHeight = lHeight + 50;
            }

            this.childDialog.ErrorCallBack = launchInContext(this.childDialog, this.childDialog.hide);

            this.childDialog.addParam("DescId", this.descId, "post");
            this.childDialog.addParam("FileId", this.fileId, "post");
            this.childDialog.addParam("TabId", getTabDescid(this.descId), "post");
            this.childDialog.addParam("Title", encode(this.title), "post");
            if (typeof (this.parentFrameId) != 'undefined')
                this.childDialog.addParam("ParentFrameId", this.parentFrameId, "post");

            this.childDialog.addParam("showXrmFormularBtn", this.showXrmFormularBtn, "post");

            this.childDialog.addParam("EditorJsVarName", this.jsVarName, "post");

            this.childDialog.addParam("ParentEditorJsVarName", this.name, "post");

            // Pour indiquer à la boîte de dialogue d'aller rechercher la valeur du champ Mémo en base (ex : affichage direct en mode Plein écran depuis le mode Liste),
            // on omet simplement le paramètre Value ; elle ira alors chercher la valeur à partir du DescID en base
            if (!bGetValueFromDB) {
                // Backlog #92 - Si on utilise grapesjs, le bouton Fullscreen de CKEditor doit uniquement servir à éditer, en fullscreen, le bloc/composant actuellement
                // sélectionné dans l'éditeur, et non l'intégralité du contenu (le bouton Fullscreen de grapesjs est là pour ça)
                var currentSelection = null;
                if (this.htmlTemplateEditor)
                    currentSelection = this.htmlTemplateEditor.getSelected();
                if (currentSelection && typeof (currentSelection.toHTML) == "function") {
                    // Lorsque l'élément sélectionné/édité est du texte, on cible non pas celui-ci, mais l'ensemble de ses parents éditables, afin qu'on puisse le retrouver
                    // dans l'éditeur Plein écran avec l'ensemble de ses balises englobantes (pouvant notamment être utilisées pour gérer la mise en forme)
                    while (
                        (currentSelection.attributes.type == "text" || currentSelection.attributes.type == "") && /* élément de type Texte ou non défini (sans mise en forme) */
                        currentSelection.parent() && ( /* disposant d'un parent... */
                            (currentSelection.parent().attributes.editable || currentSelection.parent().attributes.stylable) && /* soit éditable, soit stylisable... */
                            currentSelection.parent().attributes.type != "wrapper" /* Et qui ne soit pas de type conteneur (div, td, etc.) */
                        )
                    )
                        currentSelection = currentSelection.parent(); // on cible l'élément parent
                    // On indique au composant que la portion actuellement éditée est la sélection en cours, afin que l'appel à setData(), via validateMemoDialog(), fasse
                    // une MAJ spécifique de cette portion, et non la mise à jour de tout le contenu du canevas (ce qui, de plus, pourrait planter complètement)
                    this.partialContentsEdited = currentSelection;
                    this.childDialog.addParam("Value", encode(currentSelection.toHTML()), "post");
                }
                // Sinon, sur CKEditor sans grapesjs (ou si le ciblage de la sélection en cours échoue sur grapesjs), on édite tout le contenu en fullscreen
                else {
                    // Si on tombe sur ce cas alors qu'on utilise grapesjs, le ciblage de la sélection a probablement échoué. On le remonte
                    if (this.htmlTemplateEditor) {
                        this.trace(
                            "Le ciblage de l'élément sélectionné à éditer en plein écran a échoué : " +
                            currentSelection +
                            ". L'ensemble du contenu de l'éditeur sera affiché en plein écran, et non l'élément sélectionné."
                        );
                    }
                    this.childDialog.addParam("Value", encode(this.getData()), "post");
                }
            }

            this.childDialog.addParam("EditorType", this.editorType, "post");
            this.childDialog.addParam("ToolbarType", this.toolbarType, "post");
            this.childDialog.addParam("CustomCSS", encode(this.getCss()), "post");
            //this.childDialog.addParam("CustomOnShow", encode("function() { this.injectCSS(this.getCss()); }"), "post"); // l'insertion de CSS est désormais gérée automatiquement par createHTMLEditor() qui appelle d'ailleurs ensuite le customOnShow()
            this.childDialog.addParam("width", lWidth - 12, "post"); // 12 : marge intérieure par rapport au conteneur de l'eModalDialog
            this.childDialog.addParam("height", lHeight - 150, "post"); // 150 : espace réservé à la barre de titre + boutons de l'eModalDialog
            this.childDialog.addParam("IsHTML", (this.isHTML ? "1" : "0"), "post");
            //this.uaoz : meme en readonly, les notes peuvent être mises à jour depuis le mode zoom
            this.childDialog.addParam("ReadOnly", (this.readOnly && !this.uaoz ? "1" : "0"), "post");


            // la taille est en pleine ecran, pas de bouton zoom
            this.childDialog.hideMaximizeButton = true;

            this.trace("Affichage de la fenêtre de zoom...")
            this.childDialog.show();

            // Libellé du bouton de validation : "Enregistrer" si le champ actuel est défini comme pouvant être mis à jour uniquement depuis le mode Zoom
            // (UAOZ = Update Allowed on Zoom)
            var validateButtonLabel = top._res_28; // Valider
            if (this.uaoz)
                validateButtonLabel = top._res_286; // Enregistrer

            this.childDialog.addButton(top._res_29, cancelMemoDialog, "button-gray", this.jsVarName, "cancel"); // Annuler
            this.childDialog.addButton(validateButtonLabel, validateMemoDialog, "button-green", this.jsVarName, "ok"); // Valider
        }
    };

    // Déclenche la fonction Maximiser de la fenêtre Zoom
    this.maximizeFullScreenDialog = function () {
        if (this.childDialog && typeof (this.childDialog.MaxOrMinModal) == 'function') {
            this.childDialog.MaxOrMinModal();
        }

    }

    // Redimensionne le champ Notes à la taille indiquée
    this.resize = function (nNewWidth, nNewHeight) {
        if (this.isHTML && this.htmlEditor && !this.htmlEditor.isFake) {

            //Pour info sur le site ckeditor
            /*
            CKEDITOR.editor.status : String READ ONLY
            Indicates editor initialization status. The following statuses are available:

            unloaded: The initial state — the editor instance was initialized, but its components (configuration, plugins, language files) are not loaded yet.
            loaded: The editor components were loaded — see the loaded event.
            ready: The editor is fully initialized and ready — see the instanceReady event.
            destroyed: The editor was destroyed — see the destroy method.
             
            Defaults to: 'unloaded'
            */
            // Le statut doit etre a ready avant de faire le resize, sinon on attend que ckeditor soit pres
            if (this.htmlEditor.status == "ready")
                this.htmlEditor.resize(nNewWidth, nNewHeight, false, false);
            else
                this.htmlEditor.on('instanceReady', function (evt) { evt.editor.resize(nNewWidth, nNewHeight, false, false); });

        }
        else if (this.textEditor) {
            var nTextAreaHeightOffset = this.toolbarHeightOffset;
            if (this.compactMode)
                nTextAreaHeightOffset = -this.toolbarHeightOffset;

            var strGlobalDivWidth = (nNewWidth - 8) + '';
            var strGlobalDivHeight = nNewHeight + '';
            if (strGlobalDivWidth.indexOf('%') == -1)
                strGlobalDivWidth = getNumber(strGlobalDivWidth) + 'px';
            if (strGlobalDivHeight.indexOf('%') == -1)
                strGlobalDivHeight = getNumber(strGlobalDivHeight) + 'px';

            var strTextAreaWidth = strGlobalDivWidth;
            var strTextAreaHeight = strGlobalDivHeight;
            if (strTextAreaWidth.indexOf('%') == -1) {
                strTextAreaWidth = (getNumber(nNewWidth) - 10) + '';
                if (strTextAreaWidth.indexOf('%') == -1)
                    strTextAreaWidth = getNumber(strTextAreaWidth) + 'px';
            }
            if (strTextAreaHeight.indexOf('%') == -1) {
                strTextAreaHeight = (getNumber(nNewHeight) - nTextAreaHeightOffset) + '';
                if (strTextAreaHeight.indexOf('%') == -1)
                    strTextAreaHeight = getNumber(strTextAreaHeight) + 'px';
            }

            var oGlobalDiv = document.getElementById('eMEG_' + this.name);
            oGlobalDiv.style.width = strGlobalDivWidth;
            oGlobalDiv.style.height = strGlobalDivHeight;
            this.textEditor.style.width = strTextAreaWidth;
            this.textEditor.style.height = strTextAreaHeight;
        }
    };

    // Place l'ascenseur de la page au niveau de l'élément HTML dont l'ID a été renseigné via this.scrollIntoViewId
    /*
    //Canceled by KHA le 31/01/2013 et remplacé par une fonction asynchrone cf emain.js updateFile
    this.scrollIntoView = function () {
    if (this.scrollIntoViewId && document.getElementById(this.scrollIntoViewId))
    document.getElementById(this.scrollIntoViewId).scrollIntoView(true);
    };
    */
    // JLA & MAB - Place le curseur à une position précise par rapport au texte visible, sans tenir compte des tags HTML
    // Exemple : le texte contient <br><br>Test<br><br>, un positionnement à 2 placera le curseur entre "e" et "s" de "Test"
    this.setCursorPosition = function (nPos, offsetNodes) {

        var oObj = null;
        var oSel = null;
        var bUseAlternateMethod = false;

        if (!nPos) { nPos = 0; }

        if (this.isHTML && this.htmlEditor && this.htmlEditor.getSelection() && this.htmlEditor.getSelection().getNative()) {
            oObj = this.getMemoBody();
            bUseAlternateMethod = CKEDITOR.env.ie;
        }
        else {
            oObj = this.textEditor;
            var sUserAgent = navigator.userAgent.toLowerCase();
            bUseAlternateMethod = false;
        }

        if (bUseAlternateMethod) {
            try {
                var sTextRange = null;
                // CKEditor mode classique (oObj = body de l'iframe)
                if (typeof (oObj.createTextRange) == "function") {
                    sTextRange = oObj.createTextRange();
                }
                // CKEditor mode inline (oObj = élément HTML directement éditable)
                else {
                    sTextRange = document.body.createTextRange();
                    sTextRange.moveToElementText(oObj);
                }
                sTextRange.moveStart('character', nPos);
                //-- Déplace le curseur
                sTextRange.collapse();
                sTextRange.select();
            }
            catch (ex) {
                bUseAlternateMethod = false;
                try {
                    this.trace("Une erreur s'est produite lors du positionnement du curseur à l'emplacement " + nPos + " - Utilisation de la méthode standard - Erreur : " + ex);
                }
                catch (ex2) { }
            }
        }

        if (!bUseAlternateMethod) {
            try {
                // Champs Mémo mode Texte
                if (this.isHTML) {
                    oSel = this.htmlEditor.getSelection().getNative();

                    if (nPos == 0 && oSel && oSel.collapse) {
                        oSel.collapse(oObj, 0);
                    }
                    // MAB - 20090319 - bug #9494
                    else {
                        // Recherche du tag HTML (node) situé immédiatement après la position passée en paramètre
                        var referenceNode = null;
                        var charCount = 0;
                        for (var i = 0; i < oObj.childNodes.length; i++) {
                            var currentNodeLength = NaN;

                            // Navigateurs normaux
                            if (oObj.childNodes[i].textContent && oObj.childNodes[i].textContent.length)
                                currentNodeLength = oObj.childNodes[i].textContent.length;
                            // IE 8
                            if (isNaN(currentNodeLength) && oObj.childNodes[i].length)
                                currentNodeLength = oObj.childNodes[i].length;

                            if (!isNaN(currentNodeLength))
                                charCount += currentNodeLength;

                            // si on a dépassé la position indiquée en paramètre, on se positionne au début du tag courant
                            // 20110310 - en appliquant si demandé un décalage de X tags/nodes HTML pour pouvoir poursuivre la saisie après l'insertion de certains codes
                            // ex : insertion d'un champ de fusion -> <label></label> -> il faut se positionner après le tag de fin </label>
                            if (charCount >= nPos) {
                                if (offsetNodes && !isNaN(offsetNodes) && ((i + offsetNodes) < oObj.childNodes.length)) {
                                    referenceNode = oObj.childNodes[i + offsetNodes];
                                }
                                else {
                                    referenceNode = oObj.childNodes[i];
                                }
                                i = oObj.childNodes.length; // sortie
                            }
                        }
                        // Positionnement
                        var sTextRange = null;
                        var oDoc = this.getMemoDocument();
                        if (oDoc) {
                            sTextRange = oDoc.createRange();
                        }
                        this.focus();

                        sTextRange = getElementSelectionRangeAt(oDoc, 0);

                        if (sTextRange != null && referenceNode != null) {
                            sTextRange.setStartBefore(referenceNode);
                            if (oSel.collapseToStart) { oSel.collapseToStart(); }
                            if (oSel.collapse) { oSel.collapse(oObj, 2); }
                        }
                    }
                }

                // Champs Mémo mode Texte brut
                else {
                    if (this.textEditor.setSelectionRange) {
                        this.textEditor.focus();
                        this.textEditor.setSelectionRange(nPos, nPos);
                    }
                    else if (this.textEditor.createTextRange) {
                        var range = this.textEditor.createTextRange();
                        range.collapse(true);
                        range.moveEnd('character', nPos);
                        range.moveStart('character', nPos);
                        range.select();
                    }
                }
            }
            catch (ex) {
                try {
                    this.trace("Une erreur s'est produite lors du positionnement du curseur à l'emplacement " + nPos + " avec la méthode standard - Erreur : " + ex);
                }
                catch (ex2) { }
            }
        }
    };

    // Crée une instance de CKEditor en remplacement d'un champ <textarea>
    // Vérifie au préalable que l'instance n'existe pas déjà et la détruit si c'est le cas
    this.show = function (bCreateTextArea, bInlineMode) {
     
        this.enableTemplateEditor = this.canUseHTMLTemplateEditor(true, '');

        if (bCreateTextArea == null || typeof (bCreateTextArea) == "undefined")
            bCreateTextArea = true;

        if (bInlineMode == null || typeof (bInlineMode) == "undefined")
            bInlineMode = this.inlineMode;

        // CALCUL DE LA TAILLE DES ELEMENTS CONCERNES
        // Permet d'ajouter l'unité 'px' qui est nécessaire sur un textarea, si aucune unité n'est précisée, et de convertir les tailles de % vers px

        this.trace('Taille demandée : ' + this.config.width + ' x ' + this.config.height);

        var strGlobalDivWidth = this.config.width + '';
        var strGlobalDivHeight = this.config.height + '';
        var strTextAreaWidth = this.config.width + '';
        var strTextAreaHeight = this.config.height + '';
        var nTextAreaHeightOffset = this.toolbarHeightOffset;
        if (this.compactMode)
            nTextAreaHeightOffset = -this.toolbarHeightOffset;

        // Récupération de la largeur du conteneur parent si la taille demandée est en %
        if (strGlobalDivWidth.indexOf('%') != -1 && this.container) {
            var currentParentElement = this.container;
            var currentParentLevel = 1;
            var currentParentElementSizeData = getAbsolutePosition(currentParentElement);
            /* Commenté par MAB - 2014-03-07 - Demande #28 631 - Ajustement de la taille de CKEditor sur les fenêtres d'email pour les résolutions étriquées
            while (currentParentElementSizeData && getNumber(currentParentElementSizeData.w) == 0) {
                currentParentElement = currentParentElement.parentElement;
                currentParentLevel++;
                currentParentElementSizeData = getAbsolutePosition(currentParentElement);
            }
            if (!currentParentElement) {
                currentParentElement = this.container;
                currentParentElementSizeData = getAbsolutePosition(currentParentElement);
            }
            */
            if (currentParentElementSizeData && getNumber(currentParentElementSizeData.w) > 0) {
                // Et conversion en pixels en appliquant ce pourcentage à la taille demandée
                if (getNumber(strGlobalDivWidth) > 0 && getNumber(strGlobalDivWidth) <= 100) {
                    this.trace('Conteneur parent le plus proche ayant une largeur définie (' + currentParentElementSizeData.w + ') : ' + currentParentElement.id + ', ' + currentParentElement.tagName + ' (' + currentParentLevel + ' niveau(x) au-dessus)');
                    this.trace('Pourcentage à appliquer sur la largeur du conteneur parent : ' + getNumber(strGlobalDivWidth));
                    strGlobalDivWidth = currentParentElementSizeData.w * (getNumber(strGlobalDivWidth) / 100) + '';
                }
                else {
                    strGlobalDivWidth = currentParentElementSizeData.w + '';
                }
                this.trace('Nouvelle largeur calculée à partir du conteneur parent : ' + strGlobalDivWidth);
            }
        }
        // Ajustement
        if (strGlobalDivWidth.indexOf('%') == -1) {
            strGlobalDivWidth = getNumber(strGlobalDivWidth) + '';
            strTextAreaWidth = (getNumber(strGlobalDivWidth) - 10) + '';
        }
        // Récupération de la hauteur du conteneur parent si la taille demandée est en %
        if (strGlobalDivHeight.indexOf('%') != -1 && this.container) {
            var currentParentElement = this.container;
            var currentParentLevel = 1;
            var currentParentElementSizeData = getAbsolutePosition(currentParentElement);
            /* Commenté par MAB - 2014-03-07 - Demande #28 631 - Ajustement de la taille de CKEditor sur les fenêtres d'email pour les résolutions étriquées
            while (currentParentElementSizeData && getNumber(currentParentElementSizeData.h) == 0) {
                currentParentElement = currentParentElement.parentElement;
                currentParentLevel++;
                currentParentElementSizeData = getAbsolutePosition(currentParentElement);
            }
            if (!currentParentElement) {
                currentParentElement = this.container;
                currentParentElementSizeData = getAbsolutePosition(currentParentElement);
            }
            */
            /* #60 634, #61 981, #62 056, #62 057 (et d'autres ?) - En admin E17, une hauteur fixe est définie sur la cellule parente du conteneur (cf. eMasterFileRenderer.DrawInHTMLTable())
            Afin d'afficher les champs Mémo en tenant compte du nombre de "lignes" défini en admin (1 ligne = eConst.FILE_LINE_HEIGHT = 24) 
            On utilise donc uniquement cette hauteur dans ce cas précis, et seulement pour l'admin E17 pour l'instant (pour éviter des régressions).
            On conserve les dimensions détectées sur le conteneur immédiatement parent pour les autres cas (toujours dans l'optique d'éviter des régressions) */
            if (typeof (getCurrentView) == "function" && getCurrentView(document) == "ADMIN_FILE" && currentParentElement.parentElement?.style?.height) {
                // On indique l'ajustement à faire dans instanceReady() à l'affichage de la zone de texte en mode Inline, pour réduire la hauteur de cette zone et tenir dans le conteneur prévu par l'admin E17
                this.inlineHeightOffset = 4; // nombre de pixels à retrancher pour tenir compte des marges du conteneur dans le cas de l'admin E17
                // Et à l'inverse, on annule les ajustements faits par rapport à la barre d'outils de CKEditor, qui ne sont valables qu'en utilisation
                // En admin, ils interfèrent avec l'affichage du champ lorsqu'il doit occuper un grand nombre de lignes (= quand le champ fait plus de 150 pixels de hauteur
                // Ou à l'inverse, quand on veut afficher le champ sur 0 ou 1 ligne, ça affiche un champ plus grand qu'indiqué en admin.
                // cf. section "AJUSTEMENTS AUTOMATIQUES DE TAILLE SUR LA LARGEUR" plus bas
                this.toolbarHeightOffset = 0;
                nTextAreaHeightOffset = 0;
                
                currentParentElement = currentParentElement.parentElement;
                currentParentLevel++;
                //currentParentElementSizeData = getAbsolutePosition(currentParentElement);
                if (!currentParentElementSizeData)
                    currentParentElementSizeData = { };
                currentParentElementSizeData.h = getNumber(currentParentElement.style.height);
                this.trace("Le rendu étant effectué en administration, la hauteur du conteneur sera celle de la cellule parente soit " + currentParentElementSizeData.h + ", moins " + this.inlineHeightOffset + " pixels pour la zone de texte.");
            }
            if (currentParentElementSizeData && getNumber(currentParentElementSizeData.h) > 0) {
                // Et conversion en pixels en appliquant ce pourcentage à la taille demandée
                if (getNumber(strGlobalDivHeight) > 0 && getNumber(strGlobalDivHeight) <= 100) {
                    this.trace('Conteneur parent le plus proche ayant une hauteur définie (' + currentParentElementSizeData.h + ') : ' + currentParentElement.id + ', ' + currentParentElement.tagName + ' (' + currentParentLevel + ' niveau(x) au-dessus)');
                    this.trace('Pourcentage à appliquer sur la hauteur du conteneur parent : ' + getNumber(strGlobalDivHeight));
                    strGlobalDivHeight = currentParentElementSizeData.h * (getNumber(strGlobalDivHeight) / 100) + '';
                }
                else {
                    strGlobalDivHeight = currentParentElementSizeData.h + '';
                }
                this.trace('Nouvelle hauteur calculée à partir du conteneur parent : ' + strGlobalDivHeight);
            }
        }
        // Ajustement
        if (strGlobalDivHeight.indexOf('%') == -1) {
            strGlobalDivHeight = getNumber(strGlobalDivHeight) + '';
            strTextAreaHeight = (getNumber(strGlobalDivHeight) - nTextAreaHeightOffset) + '';
        }

        this.trace(
            'Taille recalculée du conteneur global : ' + strGlobalDivWidth + ' x ' + strGlobalDivHeight +
            '\nTaille recalculée de la zone de texte : ' + strTextAreaWidth + ' x ' + strTextAreaHeight
        );

        // AJUSTEMENTS AUTOMATIQUES DE TAILLE SUR LA HAUTEUR

        // 20 pixels : hauteur minimale requise pour afficher au moins une ligne de zone de texte dans un champ type textarea
        // On considère donc qu'on passe en affichage compact (barre d'outils repliée + pas de barre d'état) si on ne peut pas afficher au moins 5 lignes de texte (100 pixels)
        // soit l'équivalent d'une ligne avec barre d'outils + barre d'état (20 + 77)
        if (!this.compactMode && strGlobalDivHeight.indexOf('%') == -1 && getNumber(strTextAreaHeight) < 100) {
            this.trace("Hauteur de la zone de texte trop faible (" + getNumber(strTextAreaHeight) + ") : mode compact activé");
            this.compactMode = true;
            strTextAreaHeight = (getNumber(strTextAreaHeight) + nTextAreaHeightOffset) + '';
            nTextAreaHeightOffset = -this.toolbarHeightOffset;
            this.trace("Hauteur de la zone de texte après réajustement : " + strTextAreaHeight);
        }
        // Si on affiche le champ Mémo via eFieldEditor, on le réajuste par rapport au conteneur parent si sa taille est trop faible
        if (this.parentFieldEditor && this.parentFieldEditor.parentPopup && strTextAreaHeight.indexOf('%') == -1 && getNumber(strTextAreaHeight) < 50) {
            this.trace("Hauteur de la zone de texte toujours trop faible (" + getNumber(strTextAreaHeight) + ") : ajustement selon le parent");
            if (this.isHTML) {
                this.config.height = this.parentFieldEditor.parentPopup.height - 25; // 25 : contours réservés à la barre d'outils repliée
                strGlobalDivHeight = this.config.height + '';
                strTextAreaHeight = (this.config.height - nTextAreaHeightOffset) + '';
            }
            else {
                this.config.height = this.parentFieldEditor.parentPopup.height - 35; // 25 : contours réservés à la barre d'outils du champ Mémo mode texte brut
                strGlobalDivHeight = this.config.height + '';
                strTextAreaHeight = (this.config.height - nTextAreaHeightOffset) + '';
            }
            this.trace("Hauteur du conteneur global après réajustement : " + strGlobalDivHeight);
            this.trace("Hauteur de la zone de texte après réajustement : " + strTextAreaHeight);
        }
        // Si la hauteur est toujours trop faible, on impose une hauteur minimale, quitte à ce que le champ dépasse de son parent
        if (this.config.height < 60) {
            this.trace("Hauteur de la zone de texte toujours trop faible (" + getNumber(strTextAreaHeight) + ") : hauteur globale du champ forcée à 60");
            this.config.height = 60;
            strGlobalDivHeight = this.config.height + '';
            strTextAreaHeight = (this.config.height - nTextAreaHeightOffset) + '';
            this.trace("Hauteur du conteneur global après réajustement : " + strGlobalDivHeight);
            this.trace("Hauteur de la zone de texte après réajustement : " + strTextAreaHeight);
        }

        // AJUSTEMENTS AUTOMATIQUES DE TAILLE SUR LA LARGEUR

        // TODO: lorsque plusieurs ensembles de barre d'outils seront utilisés, ajuster la largeur minimale requise en fonction du nombre de boutons affichés
        var nMinRequiredWidth = 770;

        // Si la largeur de l'éditeur est trop faible pour afficher tous les boutons de la barre d'outils, on force le mode compact et/ou la barre d'outils réduite (2 boutons seulement)
        if (getNumber(strGlobalDivWidth) < nMinRequiredWidth) {
            // Et seulement si la hauteur est estimée insuffisante pour afficher une barre d'outils sur plusieurs lignes, ou si la largeur ne permet pas d'afficher un
            // bloc insécable de la barre d'outils en entier, tel que le bloc B I U Barré CouleurFond CouleurPolice Gauche Centré Droite Justifié Puces1 Puces2 DimRetrait AugRetrait Word Tableau Lien Source qui nécessite minimum 480 pixels de large
            if (!this.compactMode && ((strTextAreaHeight.indexOf('%') == -1 && getNumber(strTextAreaHeight) < 150) || (strTextAreaWidth.indexOf('%') == -1 && getNumber(strTextAreaWidth) < 480))) {
                this.trace("Largeur (" + getNumber(strGlobalDivWidth) + ") et hauteur (" + getNumber(strGlobalDivHeight) + ") du conteneur insuffisante pour afficher la barre d'outils : mode compact activé");
                this.compactMode = true;
                strTextAreaHeight = (getNumber(strTextAreaHeight) + nTextAreaHeightOffset) + '';
                nTextAreaHeightOffset = -this.toolbarHeightOffset;
                if (getNumber(strTextAreaWidth) < 480) {
                    this.trace("Largeur (" + getNumber(strGlobalDivWidth) + ") du conteneur insuffisante pour afficher tous les boutons : barre d'outils réduite activée");
                    this.reducedToolbar = true;
                }
            }
        }
        else {
            // A l'inverse, si la largeur de l'éditeur est suffisante pour afficher tous les boutons de la barre d'outils, on les affiche, même si on est en mode compact, sauf si forceCompactMode est défini à true, ou si la hauteur du conteneur est insuffisante pour afficher la barre d'outils + 5 lignes de texte (100 pixels)
            if (this.compactMode && !this.forceCompactMode && (strTextAreaHeight.indexOf('%') == -1 && getNumber(strTextAreaHeight) > 99)) {
                this.trace("Largeur et hauteur du conteneur suffisantes pour afficher une barre d'outils complète (" + getNumber(strGlobalDivWidth) + ", " + getNumber(strGlobalDivHeight) + ") : mode compact désactivé");
                this.compactMode = false;
                nTextAreaHeightOffset = this.toolbarHeightOffset;
                strTextAreaHeight = (getNumber(strTextAreaHeight) - nTextAreaHeightOffset) + '';
                this.trace("Hauteur de la zone de texte après réajustement : " + strTextAreaHeight);
            }
        }

        if (strTextAreaHeight)
            strTextAreaHeight = strTextAreaHeight + "px";
        if (strTextAreaWidth)
            strTextAreaWidth = strTextAreaWidth + "px";

        // Valeurs définitives : ajout de 'px' si la taille n'est pas en %
        // Sera également fait sur la zone de texte à la création du champ, plus bas
        if (strGlobalDivWidth.indexOf('%') == -1) {
            strGlobalDivWidth = getNumber(strGlobalDivWidth) + 'px';
        }
        if (strGlobalDivHeight.indexOf('%') == -1) {
            strGlobalDivHeight = getNumber(strGlobalDivHeight) + 'px';
        }

        // Si l'une des deux options ci-dessous a été définie dans le paramétrage du champ Mémo, on applique l'option demandée
        // Si les deux options ont été mises à true, c'est preventCompactMode qui a le dernier mot
        if (this.forceCompactMode)
            this.compactMode = true;
        if (this.preventCompactMode)
            this.compactMode = false;

        // MISE A JOUR DE LA BARRE D'OUTILS EN FONCTION DU MODE D'AFFICHAGE RETENU
        if (this.compactMode || this.borderlessMode) {
            this.trace("Mode compact ou sans bordures actif : masquage de la barre d'outils et de la barre de statut");
            this.setToolBarDisplay(false, !this.borderlessMode);
            this.setStatusBarEnabled(false);
            if (this.borderlessMode) {
                this.trace("Mode sans bordures activé : utilisation du skin mini");
                this.setSkin('eudonet-mini');
            }
        }

        // MISE A JOUR DE LA TAILLE DEMANDEE AVEC LES NOUVELLES VALEURS CALCULEES
        this.config.width = strGlobalDivWidth;
        this.config.height = strGlobalDivHeight;
        this.config.resize_minWidth = strGlobalDivWidth;
        this.config.resize_maxWidth = strGlobalDivWidth;
        this.config.resize_minHeight = strGlobalDivHeight;
        this.config.resize_maxHeight = strGlobalDivHeight;

        // CREATION DU CHAMP

        var canInitTextAreaWithValue = false; // Backlog #617 et demande #70 208
        if (this.container && bCreateTextArea) {
            // Ajustement de la zone de texte par rapport à son conteneur
            // Et ajout de l'unité 'px' si non précisée
            if (!this.isHTML) {
                var nCompactPercentOffset = 0;
                var nCompactPixelOffset = 0;
                if (this.compactMode) {
                    var nCompactPercentOffset = 1;
                    var nCompactPixelOffset = 5;
                }
                if (strTextAreaWidth.indexOf('%') == -1)
                    strTextAreaWidth = (getNumber(strTextAreaWidth) - nCompactPixelOffset) + 'px';
                else
                    strTextAreaWidth = (getNumber(strTextAreaWidth) - nCompactPercentOffset) + '%';
                if (strTextAreaHeight.indexOf('%') == -1)
                    strTextAreaHeight = (getNumber(strTextAreaHeight) - nCompactPixelOffset) + 'px';
                else
                    strTextAreaHeight = (getNumber(strTextAreaHeight) - nCompactPercentOffset) + '%';
            }
            var strAdditionalStyle = '';
            if (this.isHTML)
                strAdditionalStyle = '; display: none;';


            // Initialisation de la variable par une fin de chaine.
            var strReadOnly = '"';

            // Si le memo n'est pas modifiable, il va rajouter la chaine de caractère qui va inclure la css du readonly.
            if (this.readOnly)
                strReadOnly = " readonly\" ero=\"1\" readonly=\"readonly\"";

            var strInnerHTML = '';
            // Ajout du conteneur pour l'éditeur de templates avancé si souhaité
            if (this.enableTemplateEditor && typeof (grapesjs) != "undefined") {
                strInnerHTML =
                    '<div id="templateEditorRow_' + this.name + '" class="editor-row">' +
                    '<div id="templateEditorCanvas_' + this.name + '" class="editor-canvas">' +
                    '<div id="templateEditor_' + this.name + '"></div>' +
                    '</div>' +
                    '<div id="templateEditorPanelViews_' + this.name + '" class="panel__views"></div>' +
                    /*
                        '<div id="templateEditorPanelRight_' + this.name + '" class="panel__right">' +
                            '<div id="templateEditorLayersContainer_' + this.name + '" class="layers-container"></div>' +
                            '<div id="templateEditorSelectorsContainer_' + this.name + '" class="selectors-container"></div>' +
                            '<div id="templateEditorStylesContainer_' + this.name + '" class="styles-container"></div>' +
                            '<div id="templateEditorTraitsContainer_' + this.name + '" class="traits-container"></div>' +
                            '<div id="templateEditorBlocks_' + this.name + '"></div>' +
                        '</div>' +
                    */
                    '</div>';
            }
            // Ajout du textarea accueillant CKEditor
            strInnerHTML += "<textarea id='" + this.name + "' class=\"eMemoEditor " + strReadOnly;
            // Affichage spécifique - BBA - Les champs informations de Systeme et appartenance sont affiches sur une seule ligne
            if (this.name.split("_")[2] % 100 == 93) {
                strInnerHTML += " rows=1 ";
            }
            strInnerHTML += " style=\"width: " + strTextAreaWidth + "; height: " + strTextAreaHeight + strAdditionalStyle + "\"></textarea>";

            // Ajout des composants au conteneur parent
            this.container.innerHTML = strInnerHTML;
            this.container.style.width = strTextAreaWidth;
            this.container.style.height = strTextAreaHeight;

            // Backlog #617 et demande #70 208 - Indique de paramétrer le contenu initial du textarea au moment opportun selon le contexte (cf. ci-dessous)
            canInitTextAreaWithValue = true;
        }

        // Si le paramétrage de l'objet eMemoEditor n'autorise pas la création d'un textarea permettant de stocker la valeur (ex : objet instancié uniquement pour effectuer la mise à jour en base d'un champ Mémo affiché autrement)
        // On stocke la valeur "actuelle" du champ Mémo (celle qui peut être modifiée par l'utilisateur) dans une propriété séparée, qui sera ensuite comparée à this.value lors de la mise à jour (validate/update) pour vérifier si
        // la valeur du champ a été modifiée
        else
            this.virtualTextAreaValue = document.getElementById(this.name).value;
        //this.value = document.getElementById(this.name).value;

        // Backlog #617 et demande #70 208
        // Dans le cas où le textarea a été créé, on encapsule l'alimentation de son contenu initial dans une fonction, qui sera appelée :
        // - AVANT la création du champ Mémo HTML dans le cas d'un champ CKEditor ou grapesjs
        // - APRES la création du champ dans son conteneur dans le cas d'un champ texte brut
        // Il est nécessaire de l'appeler après la création du champ Texte brut depuis le correctif #70 208, qui remplit textarea.value plutôt que textarea.textContent
        // en priorité pour éviter des problématiques étranges sur certains navigateurs (IE, notamment). Dans ce cas de figure, le remplissage du contenu initial échouait
        // dans certains contextes si on l'effectuait avant d'encapsuler le textarea dans son conteneur (ex : alimenter la fenêtre Edition de CSS de CKEditor avec la
        // CSS en cours dans le cas de l'édition d'un mail)
        var initTextAreaValueFct = function () { };
        if (canInitTextAreaWithValue) {
            initTextAreaValueFct = function (eMemoEditorObject) {
                // Ajout du contenu initial du composant
                // CKEditor
                var textarea = document.getElementById(eMemoEditorObject.name);
                if (textarea) {
                    try {
                        SetText(textarea, eMemoEditorObject.value);
                    }
                    catch (e) {
                        // IE
                        textarea.value = eMemoEditorObject.value;
                    }
                }
                // Editeur de templates additionnel
                if (eMemoEditorObject.enableTemplateEditor && typeof (grapesjs) != "undefined") {
                    var templateEditor = document.getElementById('templateEditor_' + eMemoEditorObject.name);
                    if (templateEditor)
                        templateEditor.innerHTML = eMemoEditorObject.value;
                }
            };
        }

        if (this.isHTML) {
            // #72 278 - Indique s'il faut insérer, ou non, un espace insécable & nbsp ; avant/après un champ de fusion. Résout l'anomalie d'insertion du champ au sein de la balise <label> d'un autre
            // Avant la demande en question (04/2019), true si IE uniquement. A compter d'avril 2019, true dans tous les cas
            this.addStartSpaceWithMergeFields = true; // CKEDITOR.env.ie;
            this.addEndSpaceWithMergeFields = false; // CKEDITOR.env.ie;

            // Paramétrage des boutons de la barre d'outils selon le contexte
            this.setToolBar();

            // Paramétrage de l'insertion automatique de balises html/head/body/... (config.fullPage) en fonction du contexte
            this.setFullPageMode();

            // Ajout de la classe css pour que la police d'écriture soit cohérente avec le reste de l'application
            this.setFontClass();

            // Valeur initiale du champ Mémo - AVANT l'initialisation de CKEditor ou grapesjs
            initTextAreaValueFct(this);

            // L'instanciation de CKEditor est faite dans une fonction séparée afin que celle-ci puisse être rappelée en différé au cas où la création
            // échouerait (ex : textarea pas encore créé dans le DOM ci-dessus)
            this.createHTMLEditor();
        }
        else {
            // #72 278 - Indique s'il faut insérer, ou non, un espace insécable & nbsp ; avant/après un champ de fusion. Résout l'anomalie d'insertion du champ au sein de la balise <label> d'un autre
            this.addStartSpaceWithMergeFields = false; // texte brut, donc N/A
            this.addEndSpaceWithMergeFields = false; // texte brut, donc N/A

            // Design spécifique champ Texte brut (sauf si mode sans bordures : on affiche alors le textarea sans habillage)
            if (this.container && bCreateTextArea && !this.borderlessMode) {
                // En mode compact, on utilise des classes différentes pour gérer un affichage spécifique (classes cglobl & chead)
                // Dans ce mode, pas de barre d'outils (ses fonctions ne seront accessibles que via raccourcis clavier ou en mode Zoom)

                //BBA - Les champs informations de Systeme et appartenance sont affiches sans les icones head
                if (this.name.split("_")[2] % 100 == 93) {
                    this.compactMode = true;
                }

                if (this.compactMode) {
                    /*
                    '<div class="eME_chead" id="eME_cheadId">'
                    this.setToolBar()
                    '</div>'
                    */
                    this.container.innerHTML =
                        '<div id="eMEG_' + this.name + '" class="eME_cglobl" style="width: ' + strGlobalDivWidth + '; height: ' + strGlobalDivHeight + ';">' +
                        this.container.innerHTML +
                        '</div>';
                }
                // En mode normal, on crée une barre d'outils similaire à celle de CKEditor
                else {
                    this.container.innerHTML =
                        '<div id="eMEG_' + this.name + '" class="eME_globl" style="width: ' + strGlobalDivWidth + '; height: ' + strGlobalDivHeight + ';">' +
                        '<div class="eME_head" id="eME_head">' +
                        this.setToolBar() +
                        '</div>' +
                        this.container.innerHTML +
                        '</div>';
                }
            }

            // Référence vers le textarea (caché par CKEditor en mode HTML ou utilisé en mode Texte Brut)
            this.textEditor = document.getElementById(this.name);

            // Valeur initiale du champ Mémo - APRES la création du champ Mémo texte brut dans son conteneur
            initTextAreaValueFct(this);

            // #37575 CRU : Nombre lignes paramétré pour champ textarea
            if (!this.isHTML && this.nbRows) {
                this.textEditor.setAttribute("rows", this.nbRows);
            }

            // Mise à jour automatique à la sortie du champ si activé
            //if (this.updateOnBlur) {

            // On va valider dans tous les cas. La vérification de updateOnBlur se fera dans la fonction validate()
            var eMemoEditorObject = this;
            var oUpdateFct = (function (eMemoEditorTargetObject) { return function () { eMemoEditorTargetObject.validate(); } })(eMemoEditorObject);

            // oFldActionManger = this.textEditor;
            // oFldActionManger.runOnBlur = oUpdateFct;
            setEventListener(this.textEditor, 'blur', oUpdateFct);
            //}
            if (getCurrentView(document) == "ADMIN_FILE") {
                var eMemoEditorObject = this;
                var oUpdateFct = (function (eMemoEditorTargetObject) { return function () { nsAdminField.setMemoDefaultValue(eMemoEditorTargetObject); } })(eMemoEditorObject);

                // oFldActionManger = this.textEditor;
                // oFldActionManger.runOnBlur = oUpdateFct;
                setEventListener(this.textEditor, 'blur', oUpdateFct);
            }

            // Affectation de l'ouverture du mode Zoom lors du focus sur le champ Mémo
            // #28 591 - Avec ce mode de fonctionnement, il faut impérativement désactiver l'action par défaut sur l'élément qui contient le champ Mémo
            // si l'action est LNKOPENMEMOPOPUP, afin d'éviter que le navigateur déclenche deux évènements (clic sur la cellule ET focus dans le champ Mémo)
            // qui déclencheraient tous deux l'ouverture de la fenêtre de Zoom via switchFullScreen, entraînant l'affichage de 2 fenêtres l'une sur l'autre
            // (celle du dessous étant alors incapable de fonctionner correctement et de mettre à jour le champ Mémo source)
            if (this.uaoz) {
                // Recherche de l'action sur l'élément conteneur
                var nodeSrcElement;
                if (document.getElementById(this.name))
                    nodeSrcElement = document.getElementById(this.name).parentNode;
                else if (document.getElementById('cke_' + this.name))
                    nodeSrcElement = document.getElementById('cke_' + this.name).parentNode;
                else if (document.getElementById('eMEG_' + this.name))
                    nodeSrcElement = document.getElementById('eMEG_' + this.name).parentNode;
                // Suppression de l'action LNKOPENMEMOPOPUP si elle est affectée au conteneur
                if (nodeSrcElement && getAttributeValue(nodeSrcElement, "eaction") == "LNKOPENMEMOPOPUP")
                    setAttributeValue(nodeSrcElement, "eaction", "");
                var eMemoEditorObject = this;
                // Affectation de l'ouverture du mode Zoom lors du focus sur le champ Mémo
                var oZoomFct = (function (eMemoEditorTargetObject) { return function () { eMemoEditorTargetObject.switchFullScreen(true); } })(eMemoEditorObject);
                setEventListener(this.textEditor, 'focus', oZoomFct);
            }

            // EVENEMENTS POST-CREATION

            // Focus à l'ouverture du champ, sauf si explicitement interdit
            if (this.focusOnShow) {
                //this.setCursorPosition(this.value.length + 1);
                this.focus();
            }

            // Exécution d'une éventuelle fonction après affichage du champ Mémo, si précisée
            if (typeof (this.customOnShow) == 'function') {
                this.customOnShow();

            }

            //On s'abonne aux évènements DE TEXTAREA               
            for (var i = 0; i < this.listeners.length; i++) {
                var handler = this.listeners[i];
                if (typeof (handler) == "object" && typeof (handler.event) == "string" && typeof (handler.handle) == "function")
                    if (handler.event.toLowerCase() != "focus" && handler.event.toLowerCase() != "blur")
                        setEventListener(this.textEditor, handler.event, handler.handle);

            }
        }
    };



    // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    // METHODES DE GENERATION DES EDITEURS (CKEDITOR/GRAPESJS)
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------

    // CKEditor
    // --------

    // Demande #71 789 - Paramétrage de la propriété timestamp de CKEditor, afin de lui affecter le numéro de version actuel de l'application
    // Cette variable est ajoutée en queryString des plugin.js par CKEditor (?t=<TIMESTAMP>) afin de court-circuiter le cache navigateur lorsque le plugin est mis à jour
    // au même titre que notre variable ver= ajoutée sur tous les JS via addScript()
    // https://stackoverflow.com/questions/14940452/force-ckeditor-to-refresh-config
    // Cette méthode doit être appelée dès que possible, avant le chargement des plugins et des langues, pour que tous les fichiers chargés par CKEditor utilisent ce timestamp
    this.updateHTMLEditorTimestamp = function (newTimestamp) {
        if (!CKEDITOR) {
            this.trace("Attention, l'objet CKEDITOR n'est pas (encore) disponible. Le timestamp de contournement de cache ne sera pas paramétré ! Les fichiers de CKEditor seront issus du cache navigateur et pourront être obsolètes ! Par mesure de précaution, et pour éviter tout dysfonctionnement, il est recommandé de vider le cache de votre navigateur.");
            return;
        }

        if (!newTimestamp && (typeof (top._jsVer) != 'undefined' && top._jsVer != ""))
            newTimestamp = top._jsVer;

        if (newTimestamp)
            CKEDITOR.timestamp = newTimestamp;
        // Si newTimestamp n'est pas disponible pour une raison X, Y, ou Z, on peut également affecter une valeur aléatoire (date en cours, Math.random)
        // Mais cela court-circuiterait alors complètement le cache et forcerait le rechargement de chaque plugin à chaque affichage, ce qui peut être à l'origine de grosses
        // lenteurs au chargement. On n'adoptera donc pas cette solution, et on préférera, dans ce cas, indiquer au client de vider le cache du navigateur
        else
            this.trace("Attention, l'objet CKEDITOR n'a pas pu être paramétré avec un nouveau timestamp de contournement de cache. Les fichiers de CKEditor seront issus du cache navigateur et pourront être obsolètes ! Par mesure de précaution, et pour éviter tout dysfonctionnement, il est recommandé de vider le cache de votre navigateur.");
    };

    this.createHTMLEditor = function (strTextAreaId, bInlineMode) {
        // Demande #71 789 - Paramétrage de la propriété timestamp de CKEditor, afin de lui affecter le numéro de version actuel de l'application
        // Cette variable est ajoutée en queryString des plugin.js par CKEditor (?t=<TIMESTAMP>) afin de court-circuiter le cache navigateur lorsque le plugin est mis à jour
        // au même titre que notre variable ver= ajoutée sur tous les JS via addScript()
        this.updateHTMLEditorTimestamp(top._jsVer);

        if (strTextAreaId == null || typeof (strTextAreaId) == "undefined")
            strTextAreaId = this.name;

        if (bInlineMode == null || typeof (bInlineMode) == "undefined")
            bInlineMode = this.inlineMode;

        if (document.getElementById(strTextAreaId)) {
            // Destruction de toute instance existante avec ce nom
            // On englobe ceci dans un try/catch car CKEditor 4 semble parfois provoquer une erreur JS, comme s'il procédait
            // lui-même, de temps en temps, à la destruction d'instances inutilisées.
            // Sur CKEditor 3, la création d'un CKEditor via CKEDITOR.replace ne pouvait pas se faire si l'instance nommée existait déjà ;
            // Sur CKEditor 4 à priori, pas de blocage à l'idée de recréer une instance déjà existante. Sauf qu'en réalité, cela provoque
            // la génération, dans le DOM, de plusieurs objets ayant le même ID. Ce qui pose problème lorsqu'on fait des getElementById()
            // pour récupérer des éléments de CKEditor sans passer par son API (notamment pour getToolBarContainer())
            try {
                if (CKEDITOR.instances[strTextAreaId]) {
                    this.trace("Destruction de l'instance existante...");
                    CKEDITOR.instances[strTextAreaId].destroy(true);
                    this.trace("Instance " + strTextAreaId + " détruite");
                }
            }
            catch (ex) {
                this.trace("ERREUR lors de la destruction de l'instance : " + ex);
                this.trace("L'objet sera peut-être dupliqué sur la page.");
            }

            // Référence vers cette classe eMemoEditor pour les fonctions internes des initialisateurs de composants ci-dessous
            // pour lesquelles "this" a une autre signification (portée locale)
            var eMemoEditorObject = this;

            // Si on doit instancier un éditeur spécifique supplémentaire pour ajouter une surcouche d'édition de templates (ex : assistant d'e-mailing), on
            // réalise d'abord son initialisation, puis c'est lui qui déclenchera ensuite celle de CKEDITOR (via inline ou replace)
            // Mais on vérifie au préalable que l'instanciation ne soit pas déjà en cours, car sinon, le rappel à cette fonction createHTMLEditor() via le setTimeout
            // plus bas provoquerait la recréation infinie du composant
            if (this.enableTemplateEditor && typeof (grapesjs) != "undefined") {
                if (!this.templateEditorCreationInProgress && !this.htmlTemplateEditor) {
                    this.createHTMLTemplateEditor(strTextAreaId);
                }
            }
            else {
                this.trace("Instanciation de CKEditor en mode " + (bInlineMode ? "inline" : "normal") + " sans surcouche d'édition de templates...");
                if (bInlineMode)
                    CKEDITOR.inline(strTextAreaId, this.htmlConfig);
                else
                    CKEDITOR.replace(strTextAreaId, this.htmlConfig);
            }

            // Après initialisation de tous les composants, l'instance est référencée dans ce tableau global
            if (CKEDITOR.instances[strTextAreaId]) {
                // Maintenant que l'instance est créée, on supprime l'éventuel timer s'il a été utilisé
                if (this.instanceTimer) {
                    this.trace("Instance créée avec succès, avec utilisation de timer");
                    window.clearTimeout(this.instanceTimer);
                }
                else {
                    this.trace("Instance créée avec succès, sans utilisation de timer");
                }

                // Spécifique XRM : on ajoute à la config de l'instance CK une référence vers cet objet eMemoEditor
                // Ceci, afin de pouvoir appeler les méthodes de cette classe depuis les plugins de CKEditor, par ex.
                CKEDITOR.instances[strTextAreaId].xrmMemoEditor = this;
                // Et inversement, plus pratique que d'appeler systématiquement CKEDITOR.instances[strTextAreaId]
                this.htmlEditor = CKEDITOR.instances[strTextAreaId];

                var innerEditorObject = eMemoEditorObject.htmlEditor; // référence vers CKEditor pour la fonction interne, pour laquelle "this" a une autre signification (portée locale)

                // Fonction à exécuter lors de la prise de focus sur le champ Mémo
                var oFocusFct = (function (eMemoEditorTargetObject) {
                    return function (e) {
                        // Fermeture de la popup eFieldEditor existante (si affichée) à l'affichage du champ Mémo - ex : éventuel menu MRU
                        if (parent.ePopupObject && typeof (parent.ePopupObject.hide) == "function") {
                            parent.ePopupObject.hide();
                        }
                        // 29 860 et 31 389 - Redimensionnement de la barre d'outils si le champ Mémo est affiché en mode Inline
                        // On le fait à chaque prise de focus au cas où le champ Mémo aurait été redimensionné depuis le dernier affichage
                        if (eMemoEditorTargetObject.inlineMode)
                            eMemoEditorTargetObject.resizeToolBar();
                        // #47 223 : Si le champ Mémo est en MAJ à la sortie de champ, et situé dans un signet, on interdit le changement de signet tant que le focus se trouve à l'intérieur.
                        // Cela permet de s'assurer que le contenu soit bien sauvegardé à la sortie de champ sur tous les navigateurs (IE en particulier)
                        // et d'empêcher que la MAJ échoue lorsque le navigateur déclenche le chargement de signet avant que la sauvegarde ne soit faite
                        if (
                            eMemoEditorTargetObject.updateOnBlur &&
                            typeof (isParentElementOf) == 'function' && (isParentElementOf(document.getElementById(eMemoEditorTargetObject.name), document.getElementById('blockBkms')) || isParentElementOf(document.getElementById(eMemoEditorTargetObject.name), document.getElementById('divBkmCtner'))) &&
                            typeof (setPreventLoadBkmList) == 'function'
                        )
                            setPreventLoadBkmList(true);
                    }
                }
                )(eMemoEditorObject);

                // Fonction permettant de remettre le focus dans le champ Mémo lors d'un passage Source <> WYSIWYG
                var oModeSwitchFct = function (e) {
                    var previousMode = this.mode;
                    if (e && e.data) {
                        // ancienne syntaxe CKEditor 3 (cf. http://dev.ckeditor.com/ticket/7931)
                        if (e.data.prevMode)
                            previousMode = e.data.prevMode;
                        // nouvelle syntaxe
                        else {
                            if (e.data.previousMode)
                                previousMode = e.data.previousMode;
                        }
                    }
                    // Sous CKEditor 4, impossible d'accéder à e.data (?). Utilisation d'une propriété custom XRM ajoutée à l'instanciation de CKEditor ci-dessus
                    else {
                        if (this.xrmMemoEditor && this.xrmMemoEditor.previousMode) {
                            previousMode = this.xrmMemoEditor.previousMode;
                        }
                    }
                    // Si on change de mode (WYSIWYG vs. Source), on remet le focus dans le champ Mémo
                    if (this.mode != previousMode) {
                        this.focus();
                    }
                    // Et on met à jour la propriété interne avec le mode "en cours" (qui est en fait l'ancien mode, car this.mode n'est pas encore à jour lorsque l'évènement mode est déclenché)
                    this.xrmMemoEditor.previousMode = this.mode;
                };

                var oInstanceReadyFct = (
                    function (eMemoEditorTargetObject, innerEditorTargetObject) {
                        return function (event) {
                            eMemoEditorTargetObject.trace('Instance ' + innerEditorTargetObject.name + ' prête');

                            eMemoEditorTargetObject.setReadOnly(eMemoEditorTargetObject.readOnly);

                            eMemoEditorTargetObject.applyFixes(); // corrige d'éventuels problèmes connus de CKEditor (cf. fonction applyFixes())



                            // Comportements spécifiques appliqués aux champs Mémo affichés en mode Inline
                            if (eMemoEditorTargetObject.inlineMode) {
                                // En mode Inline, le composant (devenu un div) voit sa taille s'ajuster automatiquement en fonction du contenu.
                                // Pour empêcher ce phénomène, il faut affecter une taille au div avec overflow pour afficher un ascenseur
                                var inlineHeight = eMemoEditorTargetObject.config.height;
                                var inlineWidth = eMemoEditorTargetObject.config.width;
                                /* #60 634, #61 981, #62 056, #62 057 (et d'autres ?) - En admin E17, la hauteur de la zone de texte doit être réduite de quelques pixels pour tenir dans son conteneur à hauteur fixe */
                                if (eMemoEditorTargetObject.inlineHeightOffset > 0 && inlineHeight.indexOf('%') == -1) {
                                    inlineHeight = (getNumber(inlineHeight) - eMemoEditorTargetObject.inlineHeightOffset) + "px";
                                    eMemoEditorTargetObject.trace("La hauteur de la zone de saisie en affichage Inline a été réduite de " + eMemoEditorTargetObject.config.height + " à " + inlineHeight + " pixels pour s'ajuster à son conteneur.");
                                }
                                if (eMemoEditorTargetObject.inlineWidthOffset > 0 && inlineWidth.indexOf('%') == -1) {
                                    inlineWidth = (getNumber(inlineWidth) - eMemoEditorTargetObject.inlineWidthOffset) + "px";
                                    eMemoEditorTargetObject.trace("La largeur de la zone de saisie en affichage Inline a été réduite de " + eMemoEditorTargetObject.config.width + " à " + inlineWidth + " pixels pour s'ajuster à son conteneur.");                                    
                                }
                                // Pour la taille
                                eMemoEditorTargetObject.htmlEditor.container.$.style.height = inlineHeight;
                                eMemoEditorTargetObject.htmlEditor.container.$.style.width = inlineWidth;
                                eMemoEditorTargetObject.trace("Dimensions retenues pour la zone de saisie en affichage Inline : " + inlineWidth + " x " + inlineHeight + ".");
                                // Pour l'overflow
                                addClass(eMemoEditorTargetObject.htmlEditor.container.$, "inlineMemoEditor");

                                // #29 860 - On force également la largeur de la barre d'outils par rapport à celle du champ Mémo
                                eMemoEditorTargetObject.resizeToolBar();

                                var oToolbar = eMemoEditorTargetObject.getToolBarContainer();
                                /**/
                                if (eMemoEditorTargetObject.readOnly) {
                                    addClass(eMemoEditorTargetObject.htmlEditor.container.$, "readonly");

                                    if (oToolbar) {
                                        oToolbar.style.display = 'none';
                                    }
                                }


                                // #30 023 - on indique au document de masquer la barre d'outils lorsqu'on fait défiler la page/certains éléments scrollables
                                var oScrollFct = (function (eMemoEditorTargetObject) {
                                    return function (e) {
                                        var oScrolledObj = null;
                                        if (e) {
                                            if (e.target)
                                                oScrolledObj = e.target;
                                            else {
                                                if (e.srcElement)
                                                    oScrolledObj = e.srcElement;
                                            }
                                        }
                                        else {
                                            if (window.event)
                                                oScrolledObj = window.event.srcElement;
                                        }
                                        // #30 202 : Si l'élément subissant le défilement n'est pas CKEditor lui-même, on agit ;
                                        // Si le défilement a lieu sur un objet indéterminé, ou au sein même de CKEditor, on ne fait rien
                                        if (oScrolledObj && oScrolledObj != eMemoEditorTargetObject.htmlEditor.container.$) {
                                            // On pourrait ici appeler hideShowToolBar() pour masquer la barre d'outils, mais il faudrait alors forcer son réaffichage lors du clic ou du focus dans le champ
                                            // Faire un hideShowToolBar(true) sur le CKEDITOR.on('focus') est tout simple et fonctionne très bien...
                                            // Mais l'évènement "clic" dans le champ est fortement restreint par CKEditor 4 : http://stackoverflow.com/questions/17045329/ckeditor-how-to-add-permanent-onclick-event
                                            // et n'est pas toujours détecté. Or, il est indispensable en plus de focus() car comme le curseur reste dans le champ lors du défilement de la page,
                                            // l'évènement focus n'est pas redéclenché lorsqu'on reclique dans le champ en espérant faire apparaître la barre d'outils.
                                            // On préférera donc, ici, déclencher le onblur() du champ Mémo pour retirer le curseur du champ et, ainsi, déclencher le masquage de la barre d'outils avec le fonctionnement interne de CKEditor.
                                            // Inconvénient : cela pourra déclencher la sauvegarde du champ et d'autres mécanismes lourds liés à la sortie de champ si définis.
                                            eMemoEditorTargetObject.blur();
                                            //eMemoEditorTargetObject.hideShowToolBar(false);
                                        }
                                    }
                                })(eMemoEditorObject);
                                // On détecte/indique ici dans quels environnements, et sur quels éléments, surveiller le scroll pour enlever la barre d'outils
                                // TODO: l'idéal serait de pouvoir le faire avec un querySelector qui récupérerait tous les éléments en overflow auto.
                                // Pour le moment, on préferera cibler les éléments pour lesquels on aura déterminé que ce cas doit être géré
                                var oScrollableParentElements = new Array();
                                oScrollableParentElements.push(document.getElementById('divDetailsBkms'));
                                oScrollableParentElements.push(document.getElementById('md_pl-base'));
                                // Parcours des éléments sur lesquels rattacher l'évènement
                                for (var i = 0; i < oScrollableParentElements.length; i++) {
                                    if (
                                        oScrollableParentElements[i] != null &&
                                        typeof (oScrollableParentElements[i]) != 'undefined' /*&&
                                        (
                                            oScrollableParentElements[i].style.overflow == 'auto' ||
                                            oScrollableParentElements[i].style.overflowX == 'auto' ||
                                            oScrollableParentElements[i].style.overflowY == 'auto'
                                        )
                                        */
                                    ) {
                                        eMemoEditorTargetObject.trace("La barre d'outils sera automatiquement masquée lorsqu'un défilement aura lieu sur l'objet " + oScrollableParentElements[i].id);
                                        setEventListener(oScrollableParentElements[i], 'scroll', oScrollFct, true);
                                    }
                                }
                            }

                            // On mémorise le mode d'affichage actuel de CKEditor (Source/WYSIWYG) pour les besoins de la fonction oModeSwitchFct ci-dessous
                            eMemoEditorTargetObject.previousMode = innerEditorTargetObject.mode;

                            var cssToLoad = eMemoEditorTargetObject.getCss();
                            if (cssToLoad != '') {
                                eMemoEditorTargetObject.injectCSS(cssToLoad);
                                // Backlog #652 - Injection de la couleur de fond
                                eMemoEditorTargetObject.setColor(eMemoEditorTargetObject.getColorFromCSS(cssToLoad));
                            }

                            // Demande #78 433 - US #2 925 - Tâche #4 326
                            // Applique, sur le conteneur du champ Mémo (div), les mêmes classes CSS que sur la propriété .config.bodyClass, mise à jour par setFontClass()
                            // Sur les CKEditor instanciés en inline, il semble nécessaire de l'ajouter également sur le conteneur après l'initialisation de l'instance (instanceReady). cf. setFontClassOnContainer()
                            // La propriété bodyClass ne semblant pas être prise en compte lorsqu'on instancie un CKEditor inline (ce qui est logique, puisque cette bodyClass est censée s'appliquer sur l'iframe interne de CKEditor, qui n'est pas utilisée en mode inline)
                            // Source : https://ckeditor.com/docs/ckeditor4/latest/api/CKEDITOR_config.html#cfg-bodyClass
                            eMemoEditorTargetObject.setFontClassOnContainer();

                            if (eMemoEditorTargetObject.focusOnShow) {
                                eMemoEditorTargetObject.focus();
                            }
                            /*
                            //Canceled by KHA le 31/01/2013 et remplacé par une fonction asynchrone cf emain.js updateFile
    
                            if (eMemoEditorTargetObject.scrollOnShow) {
                            eMemoEditorTargetObject.scrollIntoView();
                            }
                            */
                            // Exécution d'une éventuelle fonction après affichage du champ Mémo, si précisée
                            if (typeof (eMemoEditorTargetObject.customOnShow) == 'function') {
                                eMemoEditorTargetObject.customOnShow();


                            }



                            // Affichage du champ Mémo en mode Zoom lorsqu'on clique dessus, si la modification est prise en charge par la fenêtre de zoom (uaoz)
                            // #28 591 - Avec ce mode de fonctionnement, il faut impérativement désactiver l'action par défaut sur l'élément qui contient le champ Mémo
                            // si l'action est LNKOPENMEMOPOPUP, afin d'éviter que le navigateur déclenche deux évènements (clic sur la cellule ET focus dans le champ Mémo)
                            // qui déclencheraient tous deux l'ouverture de la fenêtre de Zoom via switchFullScreen, entraînant l'affichage de 2 fenêtres l'une sur l'autre
                            // (celle du dessous étant alors incapable de fonctionner correctement et de mettre à jour le champ Mémo source)
                            var oZoomFct;
                            if (eMemoEditorTargetObject.uaoz) {
                                // Recherche de l'action sur l'élément conteneur
                                var nodeSrcElement;
                                if (document.getElementById(this.name))
                                    nodeSrcElement = document.getElementById(this.name).parentNode;
                                else if (document.getElementById('cke_' + this.name))
                                    nodeSrcElement = document.getElementById('cke_' + this.name).parentNode;
                                else if (document.getElementById('eMEG_' + this.name))
                                    nodeSrcElement = document.getElementById('eMEG_' + this.name).parentNode;
                                // Suppression de l'action LNKOPENMEMOPOPUP si elle est affectée au conteneur
                                if (nodeSrcElement && getAttributeValue(nodeSrcElement, "eaction") == "LNKOPENMEMOPOPUP")
                                    setAttributeValue(nodeSrcElement, "eaction", "");
                                // Affectation de l'ouverture du mode Zoom lors du focus sur le champ Mémo
                                oZoomFct = (function (eMemoEditorTargetObject) { return function () { eMemoEditorTargetObject.switchFullScreen(true); } })(eMemoEditorObject);
                                innerEditorTargetObject.on('focus', oZoomFct);
                            }
                            // Sinon, on applique la fonction définie par défaut plus haut
                            else {

                                //Avec les tablettes Android, le champ mémo est masqué par le clavier virtuel au focus.
                                //#38119 le click permet de l'afficher en fullscreen
                                if (isTabletAndroid() && !eMemoEditorTargetObject.isFullScreen) {
                                    var ZoomFct = (function (eMemoEditorTargetObject) {
                                        return function () {
                                            eMemoEditorTargetObject.blur();
                                            eMemoEditorTargetObject.uaoz = eMemoEditorTargetObject.updateOnBlur;
                                            eMemoEditorTargetObject.switchFullScreen(true);
                                        }
                                    })(eMemoEditorObject);

                                    setEventListener(eMemoEditorTargetObject.container, 'click', ZoomFct);
                                }

                                innerEditorTargetObject.on('focus', oFocusFct);
                            }

                            // Mise à jour automatique à la sortie du champ si activé
                            if (eMemoEditorTargetObject.updateOnBlur) {
                                var oUpdateFct = (function (eMemoEditorTargetObject) { return function () { eMemoEditorTargetObject.validate(); } })(eMemoEditorObject);
                                innerEditorTargetObject.on(
                                    'blur',
                                    oUpdateFct
                                );
                                innerEditorTargetObject.on(
                                    'mode',
                                    oUpdateFct
                                );
                            }
                            else if (getCurrentView(document) == "ADMIN_FILE") {
                                var oUpdateFct = (function (eMemoEditorTargetObject) { return function () { nsAdminField.setMemoDefaultValue(eMemoEditorTargetObject); } })(eMemoEditorObject);
                                innerEditorTargetObject.on(
                                    'blur',
                                    oUpdateFct
                                );
                                innerEditorTargetObject.on(
                                    'mode',
                                    oUpdateFct
                                );
                            }
                            // On force le focus sur le contenu du champ lorsqu'on passe du mode Design au mode Source et inversement
                            //innerEditorTargetObject.on('mode', oModeSwitchFct);

                            // On réinjecte les css quand on switch entre mode 'wysiwig' et 'source'
                            // Backlog #619 - Ainsi que la couleur de fond
                            innerEditorTargetObject.on('mode', function (evt) {
                                if (evt.editor.mode == 'wysiwyg') {
                                    eMemoEditorObject.injectCSS(eMemoEditorObject.getCss());
                                    eMemoEditorObject.setColor(eMemoEditorObject.getColor());
                                }
                            });

                            //On ajoute les raccourcis en tooltip de chaque bouton de la barre d'outils
                            var oToolbarButtons = document.querySelectorAll('.cke_button');
                            for (var i = 0; i < oToolbarButtons.length; i++) {
                                // On récupère le nom de la commande liée à ce bouton à partir de sa classe CSS
                                var strButtonCommand = oToolbarButtons[i].className;
                                strButtonCommand = strButtonCommand.replace('cke_button cke_button__', '');
                                strButtonCommand = strButtonCommand.substring(0, strButtonCommand.indexOf(' '));

                                // Si un descriptif de raccourci clavier a été généré pour la commande du bouton dans la fonction
                                // getLanguageResources(), on l'ajoute au tooltip existant
                                if (eMemoEditorTargetObject.toolbarButtonKeyStrokes[strButtonCommand] &&
                                    eMemoEditorTargetObject.toolbarButtonKeyStrokes[strButtonCommand] != ''
                                )
                                    oToolbarButtons[i].title += ' (' + eMemoEditorTargetObject.toolbarButtonKeyStrokes[strButtonCommand] + ')';
                            }
                        }
                    }
                )
                    (eMemoEditorObject, innerEditorObject); // Paramètres de la fonction mise en variable : objet eMemoEditor, objet CKEditor

                this.htmlEditor.on('instanceReady', oInstanceReadyFct);
                this.htmlEditor.on('doubleclick', function (evt) {

                    var override = getAttributeValue(evt.data.element.$, 'override');
                    if (override == "1") {
                        var ntab = getAttributeValue(evt.data.element.$, 'tab');
                        var fileid = getAttributeValue(evt.data.element.$, 'fileid');
                        if (ntab && fileid) {

                            var percent = 90;
                            var size = getWindowSize();
                            size.h = size.h * percent / 100;
                            size.w = size.w * percent / 100;
                            shFileInPopup(ntab, fileid, "CKEditor", size.w, size.h, 0);
                            evt.cancel();
                        }
                    }
                });


                //On s'abonne aux évènements ckeditor               
                for (var i = 0; i < this.listeners.length; i++) {
                    var handler = this.listeners[i];
                    if (typeof (handler) == "object" && typeof (handler.event) == "string" && typeof (handler.handle) == "function")
                        this.htmlEditor.on(handler.event, handler.handle);
                }

                /*
                this.htmlEditor.on(CKEDITOR_EVENT.instanceReady, function (event) { alert("instanceReady"); eTools.log.inspect(event); });
                this.htmlEditor.on(CKEDITOR_EVENT.dataReady, function (event) { alert("dataReady"); eTools.log.inspect(event); });
                this.htmlEditor.on(CKEDITOR_EVENT.change, function (event) { alert("change"); eTools.log.inspect(event); });
                this.htmlEditor.on(CKEDITOR_EVENT.click, function (event) { alert("click"); eTools.log.inspect(event); });
                this.htmlEditor.on(CKEDITOR_EVENT.dialogDefinition, function (event) { alert("dialogDefinition"); eTools.log.inspect(event); });
                this.htmlEditor.on(CKEDITOR_EVENT.instanceCreated, function (event) { alert("instanceCreated"); eTools.log.inspect(event); });
                this.htmlEditor.on(CKEDITOR_EVENT.load, function (event) { alert("load"); eTools.log.inspect(event); });
                this.htmlEditor.on(CKEDITOR_EVENT.ready, function (event) { alert("ready"); eTools.log.inspect(event); });
                this.htmlEditor.on(CKEDITOR_EVENT.loaded, function (event) { alert("loaded"); eTools.log.inspect(event); });
                this.htmlEditor.on(CKEDITOR_EVENT.state, function (event) { alert("state"); eTools.log.inspect(event); });
                this.htmlEditor.on(CKEDITOR_EVENT.mode, function (event) { alert("mode"); eTools.log.inspect(event); });
                this.htmlEditor.on(CKEDITOR_EVENT.uiReady, function (event) { alert("uiReady"); eTools.log.inspect(event); });
                */


            }
            // Si l'instanciation de CKEditor (CKEDITOR.replace) a échoué, on relance la fonction via un timeout
            // Sauf si l'instanciation s'est faite via grapesjs. Dans ce cas, c'est lui qui gère le processus, et l'ID d'instance ne sera pas celui attendu
            else if (!this.htmlTemplateEditor) {
                var oCreateHTMLEditorFct = (
                    function (eMemoEditorTargetObject) {
                        return function () {
                            eMemoEditorTargetObject.createHTMLEditor(eMemoEditorTargetObject.name, eMemoEditorTargetObject.inlineMode);
                        }
                    }
                )(that);
                this.trace("Instanciation de CKEditor différée, une nouvelle tentative sera effectuée dans 100 ms");
                this.instanceTimer = window.setTimeout(oCreateHTMLEditorFct, 100);
            }
        }
        // Si le textarea à remplacer par CKEditor n'est pas encore prêt dans le DOM, on relance la fonction via un timeout
        else {
            var oCreateHTMLEditorFct = (
                function (eMemoEditorTargetObject) {
                    return function () {
                        eMemoEditorTargetObject.createHTMLEditor(eMemoEditorTargetObject.name, eMemoEditorTargetObject.inlineMode);
                    }
                }
            )(that);
            this.trace("Zone de saisie non prête, instanciation de CKEditor différée, une nouvelle tentative sera effectuée dans 100 ms");
            this.instanceTimer = window.setTimeout(oCreateHTMLEditorFct, 100);
        }
    };

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    // METHODES SPECIFIQUES A GRAPESJS
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    // Méthodes utilitaires
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------


    // MAB - il semblerait que CKEditor refuse par défaut de s'instancier en mode Inline sur certains éléments
    // Dans ce cas de figure, on fait croire à CKEditor que le type d'élément sélectionné est éditable
    this.enableEditingOnElement = function (tagName) {
        // https://github.com/artf/grapesjs-plugin-ckeditor/issues/12
        // https://github.com/ckeditor/ckeditor-dev/issues/1135
        // https://stackoverflow.com/questions/14575036/enable-ckeditor4-inline-on-span-and-other-inline-tags
        if (typeof (eval("CKEDITOR.dtd.$editable." + tagName.toLowerCase())) == "undefined")
            eval("CKEDITOR.dtd.$editable." + tagName.toLowerCase() + " = 1;");
    };



    // Corrige d'éventuels bugs de CKEditor lors de son initialisation
    // cf. tickets sur le site de CKEditor :
    //	 - #5342 (http://dev.ckeditor.com/ticket/5342)
    // A supprimer lorsque ces fixes auront été intégrés dans le code source de CKEditor
    this.applyFixes = function () {
        /* Fixes mis en commentaire pour CKEditor 4 - les tickets sont indiqués comme résolus sur le site de CKEditor */
        /*
            if (this.isHTML && CKEDITOR.env.webkit) {
                this.trace('Application des correctifs WebKit...');
                var eMemoEditorObject = this;
    
                // Corrige un problème d'affichage sous WebKit (Safari/Chrome) qui ne dessine pas correctement l'éditeur s'il fait moins de 300 pixels
                if (parseInt(this.htmlEditor.config.width) < 310) {
                    this.resizeWebKitFix();
    
                    var oAfterSetDataFct = (
                    function (eMemoEditorTargetObject) {
                        return function (event) {
                            eMemoEditorTargetObject.resizeWebKitFix();
                        }
                    }
                    )(eMemoEditorObject); // Paramètres de la fonction mise en variable : objet eMemoEditor
    
                    var oModeFct = (
                    function (eMemoEditorTargetObject) {
                        if (eMemoEditorTargetObject.htmlEditor.mode === 'source') {
                            var txtar = document.getElementById('cke_' + eMemoEditorTargetObject.htmlEditor.name).getElementsByTagName('table')[0].getElementsByTagName('textarea')[0];
                            txtar.style.display = 'block';
                            txtar.style.height = 'inherit';
                        }
                        else {
                            eMemoEditorTargetObject.resizeWebKitFix();
                        }
                    }
                    )(eMemoEditorObject); // Paramètres de la fonction mise en variable : objet eMemoEditor
    
                    this.htmlEditor.on('afterSetData', oAfterSetDataFct);
    
                    this.htmlEditor.on('mode', oModeFct);
                }
    
                // Au premier affichage de la page, coller du contenu dans un champ Mémo vide ne passe pas toujours sous Chrome.
                // Pour éviter cela, on exécute une fonction X temps après un "coller" pour vérifier que le contenu à coller ait bien été inséré, et on le colle le cas échéant.
                document.oTimedPasteFct = function () {
                    if (document.timedPasteFctObj && document.timedPasteFctEvt) {
                        if (document.timedPasteFctEvt.editor.getData() == '') {
                            document.timedPasteFctObj.setCursorPosition(0);
                            document.timedPasteFctEvt.editor.insertHtml(document.timedPasteFctEvt.data.html);
                            document.timedPasteFctObj.setCursorPosition(document.timedPasteFctEvt.data.html.length);
                        }
                    }
                };
                var oPasteFct = (function (eMemoEditorTargetObject) {
                    return function (ev) {
                        try {
                            document.timedPasteFctEvt = ev;
                            document.timedPasteFctObj = eMemoEditorTargetObject;
                            document.timedPasteFct = window.setTimeout(document.oTimedPasteFct, 100);
                        }
                        catch (e) { }
                    }
                })(eMemoEditorObject);
                this.htmlEditor.on('paste', oPasteFct);
            }
            */

        //Demande #93 116
        if (this.toolbarType === 'mailSubject')
            this.htmlEditor.on('paste', function (evt) {
                evt.data.dataValue = decodeHTMLEntities(evt.data.dataValue);
                evt.data.dataValue = evt.data.dataValue.replace(/<[^>]+>/g, '');
            });
    }

    // Corrige un problème d'affichage sous WebKit (Safari/Chrome) qui ne dessine pas correctement l'éditeur s'il fait moins de 300 pixels
    // cf. ticket #5342 sur le site de CKEditor : http://dev.ckeditor.com/ticket/5342
    // A supprimer lorsque ce fix aura été intégré dans le code source de CKEditor
    this.resizeWebKitFix = function () {
        /* Fixes mis en commentaire pour CKEditor 4 - les tickets sont indiqués comme résolus sur le site de CKEditor */
        /*
        // 310 : taille minimale affichée par Safari/Chrome sans le fix (300) + 10 px de paddings
        if (this.isHTML && CKEDITOR.env.webkit && parseInt(this.htmlEditor.config.width) < 310) {
            this.trace('Application du correctif de redimensionnement WebKit...');
            var iframe = document.getElementById('cke_contents_' + this.htmlEditor.name).firstChild;
            iframe.style.display = 'none';
            iframe.style.display = 'block';
        }
        */
    };

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

    /**
     * Met à jour l'objet eMemoEditor enfant de l'objet courant
     * Utilisé dans le cas où on ouvre la fenêtre Plein écran : l'instance en cours dispose alors d'une référence vers l'instance se trouvant dans la fenêtre modale séparée, pour les interactions de MAJ
     * @param {any} Objet eMemoEditor à positionner comme eMemoEditor enfant de l'objet eMemoEditor courant
     */
    this.setChildMemoEditor = function (memoEditorObject) {
        if (memoEditorObject) {
            this.childMemoEditor = memoEditorObject;
            if (this.debugLevel > 1)
                this.trace("Objet Mémo enfant affecté à " + memoEditorObject.name);
        }
        else if (this.debugLevel > 1)
            this.trace("/!\ Objet Mémo enfant affecté à " + memoEditorObject + " !");
    };

    /**
     * Met à jour l'objet eMemoEditor parent de l'objet courant
     * Utilisé dans le cas où on ouvre la fenêtre Plein écran : l'instance de la fenêtre séparée dispose alors d'une référence vers l'instance se trouvant sur la page parente appelante, pour les interactions de MAJ
     * @param {any} Objet eMemoEditor à positionner comme eMemoEditor parent de l'objet eMemoEditor courant
     */
    this.setParentMemoEditor = function (memoEditorObject) {
        if (memoEditorObject) {
            this.parentMemoEditor = memoEditorObject;
            if (this.debugLevel > 1)
                this.trace("Objet Mémo parent affecté à " + memoEditorObject.name);
        }
        else if (this.debugLevel > 1)
            this.trace("/!\ Objet Mémo parent affecté à " + memoEditorObject + " !");
    };

    /**
     * Met à jour l'objet eModalDialog enfant de l'objet courant
     * Utilisé dans le cas où on ouvre la fenêtre Plein écran : l'instance de la fenêtre séparée est stockée sur l'instance eMemoEditor appelante, pour les interactions de MAJ
     * @param {any} Objet eModalDialog à positionner comme eModalDialog fille de l'objet eMemoEditor courant
     */
    this.setChildDialog = function (childDialogObject) {
        if (childDialogObject) {
            this.childDialog = childDialogObject;
            if (this.debugLevel > 1)
                this.trace("Objet fenêtre enfant affecté à " + childDialogObject.id);
        }
        else if (this.debugLevel > 1)
            this.trace("/!\ Objet fenêtre enfant affecté à " + childDialogObject + " !");
    };

    /**
     * Récupère l'objet eMemoEditor enfant de l'objet courant
     * Utilisé dans le cas où on ouvre la fenêtre Plein écran : l'instance en cours dispose alors d'une référence vers l'instance se trouvant dans la fenêtre modale séparée, pour les interactions de MAJ
     */
    this.getChildMemoEditor = function () {
        if (this.childMemoEditor) {
            if (this.debugLevel > 1)
                this.trace("Objet Mémo enfant retourné (depuis childMemoEditor) : " + this.childMemoEditor.name);
            return this.childMemoEditor;
        }
        else if (this.childDialog) {
            if (this.debugLevel > 1)
                this.trace("Objet Mémo enfant retourné : " + this.childDialog.getIframe().eMemoDialogEditorObject.name);
            return this.childDialog.getIframe().eMemoDialogEditorObject;
        }
        else {
            if (this.debugLevel > 1)
                this.trace("/!\ Impossible de retourner l'objet Mémo enfant !");
            return null;
        }
    };

    /**
     * Récupère l'objet eMemoEditor parent de l'objet courant
     * Utilisé dans le cas où on ouvre la fenêtre Plein écran : l'instance de la fenêtre séparée dispose alors d'une référence vers l'instance se trouvant sur la page parente appelante, pour les interactions de MAJ
     */
    this.getParentMemoEditor = function () {
        this.trace("Objet Mémo parent retourné : " + this.parentMemoEditor.name);
        return this.parentMemoEditor;
    };

    /**
     * Récupère l'objet eModalDialog enfant de l'objet courant
     * Utilisé dans le cas où on ouvre la fenêtre Plein écran : l'instance de la fenêtre séparée est stockée sur l'instance eMemoEditor appelante, pour les interactions de MAJ
     */
    this.getChildDialog = function () {
        this.trace("Objet fenêtre dialogue enfant retourné : " + this.childDialog);
        return this.childDialog;
    };

    // ------------------------------------------------------------------------
    // Constructeur
    // ------------------------------------------------------------------------

    this.trace("Instanciation d'un nouvel objet eMemoEditor");

    if (this.isHTML) {
        this.trace("Mode HTML activé");

        // Récupération des ressources de langue Eudo pour les ajouter à celles de CKEditor
        // --------------------------------------------------------------------------------

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

        // MAJ de la variable globale côté CKEditor
        CKEDITOR.config.language = this.language;

        this.trace("Langue sélectionnée : " + this.language);

        // Initialisation configuration générale (globale à toutes les instances)
        //CKEDITOR.basePath = "scripts/ckeditor/";
        CKEDITOR.config.fontSize_sizes = '8/8pt;9/9pt;10/10pt;11/11pt;12/12pt;14/14pt;16/16pt;18/18pt;20/20pt;22/22pt;24/24pt;26/26pt;28/28pt;36/36pt;48/48pt;72/72pt';
        // Création d'une configuration spécifique à l'instance à partir de la configuration globale
        // Il faut pour cela créer un nouvel objet vide, qui ne contiendra que les paramètres de configuration spécifiques à l'instance ;
        // Lorsqu'il créera l'instance, CKEditor prendra la config de base et la surchargera avec les propriétés définies ci-dessous.
        // Il n'est donc pas nécessaire de créer un objet déjà initialisé avec toutes les propriétés possibles.
        // En revanche, il est indispensable de déclarer ici les propriétés utilisées par le champ Mémo en mode texte brut
        this.htmlConfig = {
            width: CKEDITOR.config.width,
            height: CKEDITOR.config.height
        };

        this.trace("Paramétrage du skin...");

        // Configuration spécifique à l'instance : skin
        this.setSkin();

        // Désactivation du redimensionnement du contrôle à la volée
        this.htmlConfig.resize_enabled = false;

        // Gestion de la réglette permettant d'insérer des paragraphes dans des endroits difficiles
        // #31 732 - on désactive ce plugin sur IE 10 et antérieurs, car il provoque une multitude d'erreurs JS
        if (CKEDITOR.env.ie && CKEDITOR.env.version < 11) {
            this.removePlugin("magicline");
        }
        else if (CKEDITOR.env.mobile) {
            // #44357 : Activer la compatibilité pour l'environnement mobile (notamment Android)
            CKEDITOR.env.isCompatible = true;
        }
        // Sur les autres (vrais) navigateurs, on change simplement la couleur de la réglette
        else {
            // http://ckeditor.com/ckeditor_4.3_beta/samples/plugins/magicline/magicline.html
            this.htmlConfig.magicline_color = '#42A7DC';
        }

        // Désactivation du plugin Scayt (qui n'est pas utilisé) car conflit avec les styles sur Ckeditor
        // TODO: retirer cette ligne lorsque ckeditor sera mis à jour (>= 4.6.0)
        this.removePlugin("scayt");

        /*
        if (isTablet()) {
            this.htmlConfig.resize_enabled = false;
        }
        else { this.htmlConfig.resize_enabled = true; }
        this.htmlConfig.autoGrow_onStartup = true;
        */
        this.htmlConfig.resize_enabled = false;

        

        this.trace("Paramétrage des raccourcis clavier...");

        // Raccourcis clavier de base
        this.htmlConfig.keystrokes = [
            //[121 /* F10 */, 'source'],
            [CKEDITOR.CTRL + 68 /*D*/, 'source'],
            [CKEDITOR.CTRL + 69 /*E*/, 'xrmUserMessage'],
            [CKEDITOR.CTRL + 79 /*O*/, 'xrmUserSignature'],
            [CKEDITOR.CTRL + 65 /*A*/, 'selectall'],
            [CKEDITOR.CTRL + 66 /*B*/, 'bold'],
            [CKEDITOR.CTRL + 71 /*G*/, 'bold'],
            [CKEDITOR.CTRL + 83 /*S*/, 'underline'],
            [CKEDITOR.CTRL + 85 /*U*/, 'underline'],
            [CKEDITOR.CTRL + 73 /*I*/, 'italic'],
            [CKEDITOR.CTRL + 74 /*J*/, 'justifyblock'],
            [CKEDITOR.CTRL + 75 /*K*/, 'justifycenter'],
            [CKEDITOR.CTRL + 76 /*L*/, 'justifyleft'],
            [CKEDITOR.CTRL + 82 /*R*/, 'justifyright'],
            [CKEDITOR.CTRL + 90 /*Z*/, 'undo'],
            [CKEDITOR.CTRL + 89 /*Y*/, 'redo'],
            [CKEDITOR.CTRL + 70 /*F*/, 'find'],
            [CKEDITOR.CTRL + 72 /*H*/, 'replace'],
            [CKEDITOR.CTRL + 81 /*Q*/, 'pastetext'],
            [CKEDITOR.ALT + 87 /*W*/, 'pastefromword'],
            [CKEDITOR.CTRL + 13 /*ENTER*/, 'pagebreak'],
            [CKEDITOR.ALT + 13 /*ENTER*/, 'pagebreak']
        ];

        // Appuyer sur Entrée insère un retour à la ligne simple
        this.htmlConfig.enterMode = CKEDITOR.ENTER_BR;

        // TODO: à adapter à la langue de l'utilisateur
        this.htmlConfig.ckwebspeech = { 'culture': 'fr-FR' };

        // Full Page Mode : pour conserver les entêtes de HTML : Doctype, head, meta...
        // #53 136 - Il faut appliquer ce mode uniquement sur les CKEditor nécessitant d'éditer un code source complet/une page Web avec entêtes et CSS
        // Exemples : modèles de formulaires, modèles de mails...
        // Il ne faut pas activer ce mode dans les autres cas (ex : signature de l'utilisateur au CTRL+E, rubriques de type Mémo) car cela provoque l'ajout
        // systématique de balises <html><head><body> au code généré, qui devient alors invalide dès qu'il s'agit de l'incorporer à d'autres contenus
        // Comme l'editorType n'est pas connu au moment de l'initialisation d'eMemoEditor (la propriété editorType est paramétrée après l'appel à new eMemoEditor dans
        // eMain.js), la propriété fullPage sera paramétrée au moment de l'instanciation de CKEditor, dans la méthode show(), par un appel à setFullPageMode()
        // this.htmlConfig.fullPage = true;

        // GMA - 20140117 (src : http://stackoverflow.com/questions/18185627/ckeditor-changing-content-automatically)
        // Evite la modification du contenu HTML après appui sur le bouton Source
        // MAB - #52 816 - 20170202 - On interdit l'interprétation du tag <base> qui provoque la redirection de toutes les URLs relatives de la page vers la racine
        // indiquée par ce tag. Ex : <base href="https://www.eudonet.com/" target="_self" /> provoquerait la redirection de tout lien type mgr/eManager.ashx vers
        // https://www.eudonet.com/mgr/eManager.ashx au lieu de https://serveur/xrm/mgr/eManager.ashx
        // Toutefois, passer allowedContent à true désactive complètement l'ACF (Advanced Content Filter) de CKEditor 4, et il est donc impossible
        // de tout autoriser sauf certains tags en utilisant disallowedContent (qui nécessite que l'ACF soit actif).
        // Pour autoriser l'ajout de contenu normalement filtré tout en excluant d'autres tags, il faut donc conserver l'ACF en définissant une règle autorisant
        // tout, puis utiliser disallowedContent pour lister les tags à filtrer.
        // http://docs.ckeditor.com/#!/guide/dev_acf
        // http://docs.ckeditor.com/#!/guide/dev_disallowed_content
        //this.htmlConfig.allowedContent = true;
        this.htmlConfig.allowedContent = {
            $1: {
                // Use the ability to specify elements as an object.
                elements: CKEDITOR.dtd,
                attributes: true,
                styles: true,
                classes: true
            }
        };
        this.htmlConfig.disallowedContent = 'base';
        this.htmlConfig.extraAllowedContent = 'style';
        // CRU - #51 978 - Désactivation de SCAYT car non utilisé et en conflit avec le mode FullPage qui retire la balise <style>
        this.htmlConfig.scayt_autoStartup = false;

        // Ajout des CSS de l'application à l'iframe pour que son style (dont la police par défaut) soit le même que celle de l'application
        // On ajoute une classe au body de l'iframe CKEditor pour pouvoir déclarer dans les CSS des styles à appliquer au corps de page sans écraser ceux de l'application (document courant)
        var css = new Array();
        var nb = 0; var nbMax = 2;
        if (oContainer && oContainer.parentElement && oContainer.parentElement.className.indexOf('formularAdvMemo') > -1) {
            this.addCustomfont = true;
            nbMax = 3;
            for (var i = 0; i < document.styleSheets.length; i++) {
                if (this.addCustomfont && document.styleSheets[i].href != null && document.styleSheets[i].href.indexOf("advForm.css") > -1) {
                    this.trace("Application de la CSS " + document.styleSheets[i].href + " au contenu interne du champ Mémo");
                    css[nb] = document.styleSheets[i].href;
                    nb++;
                    break;
                }
            }
        }

        if (typeof (document.styleSheets) != 'undefined') {
            for (var i = 0; i < document.styleSheets.length; i++) {

                if (document.styleSheets[i].href != null && document.styleSheets[i].href.indexOf("eMemoEditor.css") > -1) {
                    this.trace("Application de la CSS " + document.styleSheets[i].href + " au contenu interne du champ Mémo");
                    css[nb] = document.styleSheets[i].href;
                    nb++;

                    if (nb == nbMax)
                        break;
                }
                else
                    if (document.styleSheets[i].href != null && document.styleSheets[i].href.indexOf("theme.css") > -1) {
                        this.trace("Application de la CSS " + document.styleSheets[i].href + " au contenu interne du champ Mémo");
                        css[nb] = document.styleSheets[i].href;
                        nb++;

                        if (nb == nbMax)
                            break;
                    }
            }

            this.htmlConfig.contentsCss = css;

        }
        //Ajout des fonts
        if (this.addCustomfont)
            CKEDITOR.config.font_names = 'Andale Mono/Andale Mono,AndaleMono,monospace;Arial/Arial, Helvetica, sans-serif;Arial Black/Arial Black, Gadget, sans-serif;Brush Script MT;Cabin;Comic Sans MS/Comic Sans MS, cursive;Concert One;Courier New/Courier New, Courier, monospace;Georgia/Georgia, serif;Helvetica/Helvetica, serif;Impact/Impact, Charcoal, sans-serif;Lato;Lora;Lucida Sans Unicode/Lucida Sans Unicode, Lucida Grande, sans-serif;Merriweather Sans;Montserrat/Montserrat, sans-serif;Nunito Sans;Open Sans/Open Sans, sans-serif;Open Sans Condensed;Oswald;Playfair Display;Prompt;PT Sans;Raleway;Roboto Condensed;Source Sans Pro;Space Mono;Tahoma/Tahoma, Geneva, sans-serif;Times New Roman/Times New Roman, Times, serif;Trebuchet MS/Trebuchet MS, Helvetica, sans-serif;Verdana/Verdana, Geneva, sans-serif;Work Sans/Work Sans, sans-serif;';

        // #32 646
        this.trace("Paramétrage des polices additionnelles...");
        var strNewFonts =
            'Calibri/Calibri, Helvetica, sans-serif;' +
            'Cambria/Cambria, serif;' +
            'Microsoft Sans Serif/Microsoft Sans Serif, Helvetica, sans-serif;' +
            'Century Gothic/Century Gothic, Verdana, sans-serif'; // #52544 CRU
        this.htmlConfig.font_names = CKEDITOR.config.font_names + ';' + strNewFonts;

        this.htmlConfig.bodyClass = 'eME';

        this.trace("Configuration définie. Application des ressources Eudonet et chargement des plugins...");

        // Pour passer un paramètre (ici, this, indiqué en fin de ligne) à l'intérieur d'une fonction anonyme, il faut l'encapsuler dans une variable JS
        // qui sera un pointeur vers un appel de fonction comportant le paramètre en question
        var afterLangLoad = (
            function (eMemoEditorObject) {
                return function () {
                    eMemoEditorObject.getLanguageResources();
                    eMemoEditorObject.loadPlugins();
                }
            }
        )(this); // Paramètre de la fonction renseignant eMemoObject dans la fonction interne

        // On force le chargement via CKEDITOR.lang.load pour les plugins
        CKEDITOR.lang.load(this.language, this.language, afterLangLoad);

        this.trace("Chargement des langues demandées");


    }
    else {
        this.trace("Mode HTML désactivé - Champ Mémo affiché en texte brut");

        this.textConfig = new Object();
    }

    this.setStatusBarEnabled(false);
    // Renvoie la configuration de l'instance
    this.config = this.isHTML ? this.htmlConfig : this.textConfig;

    // Paramètres de configuration communs aux deux modes (HTML ou texte)
    // Il peut s'agir de propriétés de CKEditor ou de propriétés implémentées par eMemoEditor.js
    this.config.width = '100%';
    this.config.height = '100%';


    //// When opening a dialog, its "definition" is created for it, for
    //// each editor instance. The "dialogDefinition" event is then
    //// fired. We should use this event to make customizations to the
    //// definition of existing dialogs.
    //CKEDITOR.on('dialogDefinition', function (ev) {
    //    // Take the dialog name and its definition from the event
    //    // data.
    //    var dialogName = ev.data.name;
    //    var dialogDefinition = ev.data.definition;

    //    // Check if the definition is from the dialog we're
    //    // interested on (the "Link" dialog).
    //    if (dialogName == 'link') {
    //        // Get a reference to the "Link Info" tab.
    //        var infoTab = dialogDefinition.getContents('info');

    //        // Add a checkbox field to the "info" tab.
    //        infoTab.add({
    //            type: 'checkbox',
    //            label: 'Désctiver le lien tracking', //TODORES
    //            id: 'trackedisabled',
    //            'default': '0',
    //            validate: function () {

    //                alert("Tracking ...");
    //                //if (/\d/.test(this.getValue()))
    //                //    return 'My Custom Field must not contain digits';
    //            }
    //        });

    //        // Add a text field to the "info" tab.
    //        infoTab.add({
    //            type: 'text',
    //            label: 'Nom du lien',
    //            id: 'tracklinkname',
    //            'default': '',
    //            validate: function () {
    //                if (/\d/.test(this.getValue()))
    //                    return 'My Custom Field must not contain digits';
    //            }
    //        });

    //        // Add a text field to the "info" tab.
    //        infoTab.add({
    //            type: 'select',
    //            id: 'tracklink',
    //            label: 'Champ de tracking',
    //            items: [['Invitation.Inscription confirmée'], ['Invitation.Nombre de personnes (compteur)'], ['... etc']],
    //            'default': 'Invitation.Inscription confirmée',
    //            onChange: function (api) {
    //                // this = CKEDITOR.ui.dialog.select
    //                alert('Current value: ' + this.getValue());
    //            }
    //        });


    //        // Remove the "Link Type" combo and the "Browser
    //        // Server" button from the "info" tab.
    //        //infoTab.remove('linkType');
    //        //infoTab.remove('browse');

    //        // Set the default value for the URL field.
    //       // var urlField = infoTab.get('url');
    //       // urlField['default'] = 'www.eudonet.com';

    //        // Remove the "Target" tab from the "Link" dialog.
    //        //dialogDefinition.removeContents('target');

    //        // Add a new tab to the "Link" dialog.
    //        //dialogDefinition.addContents({
    //        //    id: 'customTab',
    //        //    label: 'My Tab',
    //        //    accessKey: 'M',
    //        //    elements: [
    //		//		{
    //		//		    id: 'myField1',
    //		//		    type: 'text',
    //		//		    label: 'My Text Field'
    //		//		},
    //		//		{
    //		//		    id: 'myField2',
    //		//		    type: 'text',
    //		//		    label: 'Another Text Field'
    //		//		}
    //        //    ]
    //        //});

    //        // Rewrite the 'onFocus' handler to always focus 'url' field.
    //        dialogDefinition.onFocus = function () {
    //            var urlField = this.getContentElement('info', 'url');
    //            urlField.select();
    //        };
    //    }
    //});

}

// ------------------------------------------------------------------------------------------------
// FONCTIONS HORS OBJET eMEMOEDITOR
// ------------------------------------------------------------------------------------------------

function getMemoDialogData(jsVarName) {
    var memoObject = eval(jsVarName);
    var childMemoObject = memoObject.getChildMemoEditor();
    return childMemoObject.getData();
}

// TODO: fonction hors objet, requis par eModalDialog
// Ferme la fenêtre du champ Mémo en mode Zoom sans tenir compte des modifications
// #83 032 - L'UID de la fenêtre (transmis nativement par eModalDialog) est récupéré afin de pouvoir toujours au moins fermer la fenêtre,
// même en cas de problème de récupération du champ eMemoEditor
function cancelMemoDialog(jsVarName, windowUid) {
    var memoObject = eval(jsVarName); // eval et non window[] car la variable objet que l'on recherche peut être de type eFieldEditorObject.memoEditor
    if (memoObject.parentFieldEditor)
        memoObject.parentFieldEditor.cancel();
    if (memoObject.childDialog) {
        memoObject.childDialog.hide();
    }
    if (memoObject.parentFieldEditor)
        memoObject.parentFieldEditor.filter = '';
    // On remet le focus sur le champ Mémo afin que son contenu soit sauvegardé lorsqu'on en sort si la sauvegarde automatique est activée (onBlur)
    if (!memoObject.uaoz)
        memoObject.focus();

    if (typeof (ModalDialogs) == "object" && ModalDialogs[windowUid]) {
        ModalDialogs[windowUid].hide();
        console.log("Attention, la fenêtre de Mémo Plein écran '" + windowUid + "' a dû être fermée sans avoir pu interagir sur le champ Mémo parent ('" + (memoObject ? memoObject.name : '(inconnu)') + "') - Cas 1");
    }
    if (top.window['_mdName'] && top.window['_mdName'][windowUid]) {
        top.window['_mdName'][windowUid].hide();
        console.log("Attention, la fenêtre de Mémo Plein écran '" + windowUid + "' a dû être fermée sans avoir pu interagir sur le champ Mémo parent ('" + (memoObject ? memoObject.name : '(inconnu)') + "') - Cas 2");
    }
}


// TODO: fonction hors objet, requis par eModalDialog
// Ferme la fenêtre du champ Mémo en mode Zoom en tenant compte des modifications
function validateMemoDialog(jsVarName) {

    var memoObject = eval(jsVarName); // eval et non window[] car la variable objet que l'on recherche peut être de type eFieldEditorObject.memoEditor

    //MOU-07/05/2014 l'enfant et dans l'iframe de la modale
    var childMemoObject = memoObject.getChildMemoEditor();

    // Transfert du contenu du champ Mémo de la fenêtre modale vers celui de la fenêtre parente, et enregistrement
    // setData met à jour le contenu du champ via un appel asynchrone. Il faut donc passer le reste du code à exécuter en paramètre, pour qu'il soit exécuté en callback
    // une fois que l'appel asynchrone a abouti
    if (childMemoObject) {

        var newValue = childMemoObject.getData();

        // Fonction à exécuter après la sauvegarde, en callback (CKEditor) ou en synchrone (grapesjs)
        var oAfterSetDataFct = function () {
            //SHA : correction bug #71 412
            var updateExec = false;

            // mise à jour depuis le mode zoom uniquement        
            if (memoObject.uaoz) {
                memoObject.validate();
            }
            else if (memoObject.parentFieldEditor) {

                // Mise à jour de l'éditeur d'origine si la fenêtre de Zoom a été ouverte depuis le mode Liste        
                // Il faut mettre à jour les valeurs internes des objets eMemoEditor et eFieldEditor en même temps que l'on envoie la mise à jour à CKEditor lui-même.
                // Autrement, l'appel à validate() effectué ensuite tenterait de récupérer la valeur via CKEditor alors que celle-ci serait encore en cours de mise à jour.
                memoObject.parentFieldEditor.value = newValue;
                memoObject.parentFieldEditor.memoEditor.value = newValue;
                // Cet appel à validate() est fait afin de marquer le champ, visuellement, comme ayant été édité et sauvegardé correctement, avec l'entourage vert
                memoObject.parentFieldEditor.validate();
                // On réinitialise le filtre interne de eFieldEditor (bien que non utilisé dans le cas d'un champ Mémo)
                memoObject.parentFieldEditor.filter = '';
                //SHA : correction bug #71 412
                updateExec = true;
            }

            if (!memoObject.uaoz) {
                // On remet le focus sur le champ Mémo afin que son contenu soit sauvegardé lorsqu'on en sort si la sauvegarde automatique est activée (onBlur)
                // sph : #27491
                // Ajout d'un test sur uaoz 
                // si on focus, le hide ne se fait pas sur IE
                // dans ce cas, le champ a été sauvegardé ( memoObject.validate() a été appelé)
                memoObject.focus();
            }

            // Si on n'est pas en mode popup (template), on met à jour à la validation
            //SHA : correction bug #71 412 (!updateExec)
            if (typeof (memoObject.parentFrameId) === 'undefined' && !updateExec)
                memoObject.update(newValue);

            // On ferme la fenêtre champ Mémo en mode Zoom
            if (memoObject.childDialog) {
                memoObject.childDialog.hide();
            }

            // On recharge la fenetre Planning avec les mises a jours 
            if ((getCurrentView(document) == "CALENDAR") || (getCurrentView(document) == "CALENDAR_LIST")) { goTabList(nGlobalActiveTab, 1); }
        };

        // Backlog #92 - Cas de CKEditor, instancié via grapesjs, affichant en mode Zoom uniquement l'élément sélectionné
        // Dans ce cas de figure, et uniquement celui-ci, on fait appel à l'API de grapesjs pour mettre à jour uniquement la portion concernée
        // https://github.com/artf/grapesjs/issues/906
        if (memoObject.partialContentsEdited) {
            // Ce type de MAJ n'est pris en charge que via grapesjs
            if (memoObject.htmlTemplateEditor) {
                memoObject.partialContentsEdited.components(newValue);
            }

            // Puis on termine le traitement et on ferme la fenêtre, dans tous les cas
            oAfterSetDataFct();
        }
        // Dans tous les autres cas, MAJ du champ Mémo "source" complet (masqué en mode Liste, ou affiché en mode fiche) et MAJ des objets eMemoEditor/eFieldEditor liés
        else {
            memoObject.setData(
                newValue,
                oAfterSetDataFct
            );

            //MOU-07/05/2014 - Recupère la css du mémo enfant et l'injecte dans le parent
            //Note : il est essentiel d'appeler injectCSS après qu'on ait mis à jour le contenu avec setData pour que
            //les styles s'appliquent.
            memoObject.injectCSS(childMemoObject.getCss());
        }
    }

}

// TODO: fonction hors objet, requis par eModalDialog
// Ferme la fenêtre d'insertion CSS en insérant la feuille de style saisie
function validateCSSMemoDialog(jsVarName) {
    var memoObject = eval(jsVarName); // eval et non window[] car la variable objet que l'on recherche peut être de type eFieldEditorObject.memoEditor
    var childMemoObject = memoObject.getChildMemoEditor();
    if (childMemoObject) {
        var sNewCSS = childMemoObject.getData();
        var sNewColor = childMemoObject.getColorFromCSS(sNewCSS);
        // Transfert du contenu
        memoObject.injectCSS(sNewCSS);
        // Backlog #619/#652 - Mise à jour de la couleur de fond
        memoObject.setColor(sNewColor);
    }

    // Fermeture de la fenêtre
    if (memoObject.childDialog) {
        memoObject.childDialog.hide();
    }
    // On remet le focus sur le champ Mémo afin que son contenu soit sauvegardé lorsqu'on en sort si la sauvegarde automatique est activée (onBlur)
    memoObject.focus();
}

// TODO: fonction hors objet, requis par eModalDialog
// Ferme la fenêtre du champ Mémo en mode Zoom sans tenir compte des modifications
function cancelCSSMemoDialog(jsVarName) {
    var memoObject = eval(jsVarName); // eval et non window[] car la variable objet que l'on recherche peut être de type eFieldEditorObject.memoEditor
    if (memoObject.childDialog) {
        memoObject.childDialog.hide();
    }
    // On remet le focus sur le champ Mémo afin que son contenu soit sauvegardé lorsqu'on en sort si la sauvegarde automatique est activée (onBlur)
    memoObject.focus();
}

function openChildDialogOnTablet(e, element) {

    var bIsTablet = false;
    try {
        if (typeof (isTablet) == 'function')
            bIsTablet = isTablet();
        else if (typeof (top.isTablet) == 'function')
            bIsTablet = top.isTablet();
    }
    catch (e) {

    }

    if (bIsTablet) {
        var icon = element.nextSibling;
        if (getAttributeValue(icon, "eaction") == "LNKOPENMEMOPOPUP") {
            icon.click();
        }
    }
}

//TODO LA LISTE DES EVENEMENTS pour le champ mémo .pas exhaustive
var CKEDITOR_EVENT = (function () {
    return {
        afterCommandExec: "afterCommandExec",
        afterRedo: "afterRedo",
        afterSetData: "afterSetData",
        afterUndo: "afterUndo",
        beforeCleanWord: "beforeCleanWord",
        beforeCommandExec: "beforeCommandExec",
        beforeGetData: "beforeGetData",
        beforeModeUnload: "beforeModeUnload",
        beforepaste: "beforepaste",
        blur: "blur",
        cancel: "cancel",
        change: "change",
        click: "click",
        configLoaded: "configLoaded",
        contentDom: "contentDom",
        contentDomUnload: "contentDomUnload",
        contextmenu: "contextmenu",
        currentInstance: "currentInstance",
        customConfigLoaded: "customConfigLoaded",
        dataReady: "dataReady",
        destroy: "destroy",
        dialogDefinition: "dialogDefinition",
        dialogHide: "dialogHide",
        dialogShow: "dialogShow",
        download: "download",
        editingBlockReady: "editingBlockReady",
        focus: "focus",
        getData: "getData",
        getSnapshot: "getSnapshot",
        hide: "hide",
        insertElement: "insertElement",
        insertHtml: "insertHtml",
        instanceCreated: "instanceCreated",
        instanceDestroyed: "instanceDestroyed",
        instanceReady: "instanceReady",
        key: "key",
        load: "load",
        loaded: "loaded",
        loadSnapshot: "loadSnapshot",
        menuShow: "menuShow",
        mode: "mode",
        mouseout: "mouseout",
        mouseover: "mouseover",
        ok: "ok",
        on: "on",
        paste: "paste",
        pasteDialog: "pasteDialog",
        pluginsLoaded: "pluginsLoaded",
        ready: "ready",
        resize: "resize",
        saveSnapshot: "saveSnapshot",
        scaytDialog: "scaytDialog",
        scaytReady: "scaytReady",
        selectionChange: "selectionChange",
        setData: "setData",
        shiftTab: "shiftTab",
        show: "show",
        showScaytState: "showScaytState",
        state: "state",
        tab: "tab",
        testEvent: "testEvent",
        themeLoaded: "themeLoaded",
        themeSpace: "themeSpace",
        uiReady: "uiReady",
        undefined: "undefined",
        unreg: "unreg",
        value: "value"
    };
})();

//On charge les extra-plugin (CKEditor)
eMemoEditor.prototype.loadExtraPlugins = function () {
    if (CKEDITOR.config.extraPlugins == "")
        this.loadPlugins();
}

//on ajoute le type image custom 
eMemoEditor.prototype.addCustomImageComponentType = function (that) {
    var componentType = "image";
    that.trace("Personnalisation de l'éditeur avancé après initialisation - Paramétrage des composants et des modèles - Surcharge du type " + componentType);
    var baseType = this.htmlTemplateEditor.DomComponents.getType(componentType);
    if (baseType) {
        this.htmlTemplateEditor.DomComponents.addType(componentType, {
            model: baseType.model.extend(
                {
                    // Backlog #42 - Object.assign() n'est pas supporté par IE de base. Il est assuré ici par un polyfill déclaré dans eTools
                    defaults: Object.assign({}, baseType.model.prototype.defaults, {
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
            view: baseType.view/*.extend({
						events: {
							click: function () {
								return false;
							},
							dblclick: function () {
								return false;
							}
						},
					})*/
        });
    }
}

// on définit une méthode abstraite pour l'ajout d'un type 'mergefield' dans grapesJS
eMemoEditor.prototype.addMergefieldComponentType = function () { };

//méthode spécifique pour grapesJS pour charger les composants de type 'mergefield'
eMemoEditor.prototype.loadMergefieldComponentElement = function (memoData) { return memoData; }