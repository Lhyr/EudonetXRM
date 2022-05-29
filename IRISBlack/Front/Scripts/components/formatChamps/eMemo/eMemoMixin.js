import { eFileComponentsMixin } from '../../../mixins/eFileComponentsMixin.js?ver=803000'
import { RemoveBorderSuccessError, verifComponent, updateMethod, showInformationIco, displayInformationIco } from '../../../methods/eComponentsMethods.js?ver=803000';
import { onUpdateCallback } from '../../eFieldEditorMethods.js?ver=803000';
import { insertSignature, getUserMessage, insertMessage, insertData, setData, getData, setCursorPosition, getMemoDocument, getMemoBody, trace, insertImg, help, hideShowToolBar, focus } from '../../CKEditorMethods.js?ver=803000';
import EventBus from '../../../bus/event-bus.js?ver=803000';
import { verifComponentModal } from '../../../methods/eModalComponents.js?ver=803000';

/**
 * Mixin commune aux composants eMemo.
 * */
export const eMemoMixin = {
    mixins: [eFileComponentsMixin],
    data() {
        return {
            htmlEditor: {},
            textEditor: {},
            editor: {},
            res: top,
            bEmptyDisplayPopup: false,
            bShowOnFocus: false,
            that: this,
            modif: false,
            value: "",
            focusOnShow: false,
            htmlTemplateEditor: false,
            modalOptions: {},
            bmaximize: true
        };
    },
    components: {
        eAlertBox: () => import(AddUrlTimeStampJS("../../modale/alertBox.js"))
    },
    mounted() {
        this.displayInformationIco();
        if (!this.propListe) {
            this.$refs["dvEMemo"].addEventListener('resize', this.autoResize);


            // Représente des valeurs internes au composant eMemo, en mode Texte brut ET HTML
            this.editor = {};
            this.editor.parentElement = findUpByClass(document.getElementById(this.GetComponentId), "rubriqueEmemo");
            this.editor.parentElementLabel = this.editor.parentElement ? this.editor.parentElement.querySelector(".left") : null;
            this.textEditor = document.getElementById(this.GetComponentId);
			this.paramWindow = getParamWindow(); // pointeur vers eParamIFrame pour récupérer certains paramètres (notamment ceux de Mon Eudonet : signatures, taille de police...)

            if (this.dataInput.IsHtml) {

                // Représente des valeurs internes au composant eMemo, en mode HTML uniquement
                this.htmlEditor = {};
                this.htmlEditor.inlineMode = true;
                this.htmlEditor.compactMode = false;
                this.htmlEditor.borderlessMode = false;
                this.htmlEditor.toolbarButtonKeyStrokes = new Object();
                this.htmlEditor.language = 'fr';
                this.htmlEditor.mainColor = getComputedStyle(document.documentElement).getPropertyValue('--main-color').trim(); // cf. CSS des thèmes 2019
                this.htmlEditor.baseWidth = CKEDITOR.config.width;
                this.htmlEditor.baseHeight = CKEDITOR.config.height;

                // Récupération des ressources de langue Eudo pour les ajouter à celles de CKEditor
                // --------------------------------------------------------------------------------

                switch (this.getUserLangID) {
                    case 1:
                        this.htmlEditor.language = 'en';
                        break;
                    case 2:
                        this.htmlEditor.language = 'de';
                        break;
                    case 3:
                        this.htmlEditor.language = 'nl';
                        break;
                    case 4:
                        this.htmlEditor.language = 'es';
                        break;
                    case 5:
                        this.htmlEditor.language = 'it';
                        break;
                    case 6:
                    case 7:
                    case 8:
                    case 9:
                    case 10:
                    case 0:
                    default:
                        this.htmlEditor.language = 'fr';
                        break;
                }

                // MAJ de la variable globale côté CKEditor
                CKEDITOR.config.language = this.htmlEditor.language;

                // Initialisation configuration générale (globale à toutes les instances)
                CKEDITOR.config.fontSize_sizes = '8/8pt;9/9pt;10/10pt;11/11pt;12/12pt;14/14pt;16/16pt;18/18pt;20/20pt;22/22pt;24/24pt;26/26pt;28/28pt;36/36pt;48/48pt;72/72pt';

                // Création d'une configuration spécifique à l'instance à partir de la configuration globale
                // Il faut pour cela créer un nouvel objet vide, qui ne contiendra que les paramètres de configuration spécifiques à l'instance ;
                // Lorsqu'il créera l'instance, CKEditor prendra la config de base et la surchargera avec les propriétés définies ci-dessous.
                // Il n'est donc pas nécessaire de créer un objet déjà initialisé avec toutes les propriétés possibles.
                // En revanche, il est indispensable de déclarer ici les propriétés utilisées par le champ Mémo en mode texte brut
                this.htmlEditor.config = {
                    width: this.htmlEditor.baseWidth,
                    height: this.htmlEditor.baseHeight
                };

                // Paramétrage de la barre d'outils
                this.setToolbar();

                // Configuration spécifique à l'instance : skin
                this.setSkin();

                // Désactivation du redimensionnement du contrôle à la volée
                this.htmlEditor.config.resize_enabled = false;

                // Désactivation du redimensionnement en fonction du contenu
                this.htmlEditor.config.autoGrow_onStartup = false,

                    // Lecture seule
                    this.htmlEditor.config.readOnly = this.dataInput.ReadOnly;

                // #44357 : Activer la compatibilité pour l'environnement mobile (notamment Android)
                if (CKEDITOR.env.mobile) {
                    CKEDITOR.env.isCompatible = true;
                }
                // On définit la couleur de la réglette
                // http://ckeditor.com/ckeditor_4.3_beta/samples/plugins/magicline/magicline.html
                this.htmlEditor.config.magicline_color = this.htmlEditor.mainColor;

                this.htmlEditor.config.resize_enabled = false;

                // #32 646 - Ajout de polices à CKEditor
                var strNewFonts =
                    'Calibri/Calibri, Helvetica, sans-serif;' +
                    'Cambria/Cambria, serif;' +
                    'Microsoft Sans Serif/Microsoft Sans Serif, Helvetica, sans-serif;' +
                    'Century Gothic/Century Gothic, Verdana, sans-serif'; // #52544 CRU
                this.htmlEditor.config.font_names = CKEDITOR.config.font_names + ';' + strNewFonts;

                // Raccourcis clavier de base
                this.htmlEditor.config.keystrokes = [
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
                this.htmlEditor.config.enterMode = CKEDITOR.ENTER_BR;

                // Plugin de dictée vocale
                // TOCHECK: On considère arbitrairement qu'il existe et que l'on utilise une culture correspondant à la langue sélectionnée, sous la forme lang-LANG
                this.htmlEditor.config.ckwebspeech = { 'culture': this.htmlEditor.language + '-' + this.htmlEditor.language.toUpperCase() };

                // Full Page Mode : pour conserver les entêtes de HTML : Doctype, head, meta...
                // #53 136 - Il faut appliquer ce mode uniquement sur les CKEditor nécessitant d'éditer un code source complet/une page Web avec entêtes et CSS
                // Exemples : modèles de formulaires, modèles de mails...
                // Il ne faut pas activer ce mode dans les autres cas (ex : signature de l'utilisateur au CTRL+E, rubriques de type Mémo) car cela provoque l'ajout
                // systématique de balises <html><head><body> au code généré, qui devient alors invalide dès qu'il s'agit de l'incorporer à d'autres contenus
                // Comme l'editorType n'est pas connu au moment de l'initialisation d'eMemoEditor (la propriété editorType est paramétrée après l'appel à new eMemoEditor dans
                // eMain.js), la propriété fullPage sera paramétrée au moment de l'instanciation de CKEditor, dans la méthode show(), par un appel à setFullPageMode()
                // this.htmlEditor.config.fullPage = true;

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
                //this.htmlEditor.config.allowedContent = true;
                this.htmlEditor.config.allowedContent = {
                    $1: {
                        // Use the ability to specify elements as an object.
                        elements: CKEDITOR.dtd,
                        attributes: true,
                        styles: true,
                        classes: true
                    }
                };
                this.htmlEditor.config.disallowedContent = 'base';
                this.htmlEditor.config.extraAllowedContent = 'style';
                // CRU - #51 978 - Désactivation de SCAYT car non utilisé et en conflit avec le mode FullPage qui retire la balise <style>
                this.htmlEditor.config.scayt_autoStartup = false;

                // Ajout des CSS de l'application à l'iframe pour que son style (dont la police par défaut) soit le même que celle de l'application
                // On ajoute une classe au body de l'iframe CKEditor pour pouvoir déclarer dans les CSS des styles à appliquer au corps de page sans écraser ceux de l'application (document courant)
                var css = new Array();
                var nb = 0;
                if (typeof (document.styleSheets) != 'undefined') {
                    for (var i = 0; i < document.styleSheets.length; i++) {
                        if (document.styleSheets[i].href != null && document.styleSheets[i].href.indexOf("eMemoEditor.css") > -1) {
                            css[nb] = document.styleSheets[i].href;
                            nb++;

                            if (nb == 2)
                                break;
                        }
                        else
                            if (document.styleSheets[i].href != null && document.styleSheets[i].href.indexOf("theme.css") > -1) {
                                css[nb] = document.styleSheets[i].href;
                                nb++;

                                if (nb == 2)
                                    break;
                            }
                    }

                    this.htmlEditor.config.contentsCss = css;
                }
				
                this.htmlEditor.config.bodyClass = 'eME';
				
				// Ajout de la classe CSS pour que la police d'écriture soit cohérente avec le reste de l'application
				// NB : sur les CKEditor instanciés en inline, il semble nécessaire de l'ajouter également sur le conteneur après l'initialisation de l'instance (instanceReady). cf. setFontClassOnContainer()
				// La propriété bodyClass ne semblant pas être prise en compte lorsqu'on instancie un CKEditor inline (ce qui est logique, puisque cette bodyClass est censée s'appliquer sur l'iframe interne de CKEditor, qui n'est pas utilisée en mode inline)
				// Source : https://ckeditor.com/docs/ckeditor4/latest/api/CKEDITOR_config.html#cfg-bodyClass
				this.setFontClass();

                // Pour passer un paramètre (ici, this, indiqué en fin de ligne) à l'intérieur d'une fonction anonyme, il faut l'encapsuler dans une variable JS
                // qui sera un pointeur vers un appel de fonction comportant le paramètre en question
                var afterLangLoad = (
                    function (vueJsObject) {
                        return function () {
                            vueJsObject.getHTMLEditorLanguageResources();
                            vueJsObject.loadHTMLEditorPlugins();
                        };
                    }
                )(this); // Paramètre de la fonction renseignant l'objet dans la fonction interne

                // On force le chargement via CKEDITOR.lang.load pour les plugins
                CKEDITOR.lang.load(this.htmlEditor.language, this.htmlEditor.language, afterLangLoad);

                // On détruit toute instance de CKEditor préalablement existante avec le même ID
                // Evite des erreurs JS
                var strTextAreaId = this.GetComponentId;
                if (document.getElementById(strTextAreaId)) {
                    // Destruction de toute instance existante avec ce nom
                    // On englobe ceci dans un try/catch car CKEditor 4 semble parfois provoquer une erreur JS, comme s'il procédait
                    // lui-même, de temps en temps, à la destruction d'instances inutilisées.
                    // Sur CKEditor 3, la création d'un CKEditor via CKEDITOR.replace ne pouvait pas se faire si l'instance nommée existait déjà ;
                    // Sur CKEditor 4 à priori, pas de blocage à l'idée de recréer une instance déjà existante. Sauf qu'en réalité, cela provoque
                    // la génération, dans le DOM, de plusieurs objets ayant le même ID. Ce qui pose problème lorsqu'on fait des getElementById()
                    // pour récupérer des éléments de CKEditor sans passer par son API (notamment pour getToolbarContainer())

                    // Sur VueJS et autres, la non-destruction préalable entraîne des erreurs JS
                    // https://stackoverflow.com/questions/19328548/ckeditor-uncaught-typeerror-cannot-call-method-unselectable-of-null-in-emberj/30668990

                    try {
                        if (CKEDITOR.instances[strTextAreaId]) {
                            CKEDITOR.instances[strTextAreaId].removeAllListeners();
                            CKEDITOR.remove(CKEDITOR.instances[strTextAreaId]);
                            //CKEDITOR.instances[strTextAreaId].destroy(true);
                        }
                    }
                    catch (ex) {
                        //console.log(ex);
                    }
                }

                // ET ENFIN, MESDAMES, MESSIEURS, CKEDITOR
                if (document.getElementById(this.GetComponentId)) {
                    if (this.htmlEditor.inlineMode) {
                        CKEDITOR.inline(this.GetComponentId, this.htmlEditor.config);

                    }
                    else
                        CKEDITOR.replace(this.GetComponentId, this.htmlEditor.config);
                }

                else {
                    // Le composant n'est pas encore prêt, en attente de la prochaine tentative automatique de VueJS
                }


                // A l'instanciation prête du composant, exécution d'autres traitements
                var oInstanceReadyFct = (
                    function (vueJsObject) {
                        return function () {
                            vueJsObject.autoResize();
                            vueJsObject.hideToolbarOnScroll();
							vueJsObject.setFontClassOnContainer();
                        };
                    }
                )(this); // Paramètre de la fonction renseignant l'objet dans la fonction interne

                // Fonction à exécuter lors de la prise de focus sur le champ Mémo
                // 29 860 et 31 389 - Redimensionnement de la barre d'outils si le champ Mémo est affiché en mode Inline
                // On le fait à chaque prise de focus au cas où le champ Mémo aurait été redimensionné depuis le dernier affichage
                var oFocusFct = (
                    function (vueJsObject) {
                        return function () {
                            vueJsObject.RemoveBorderSuccessError()
                            vueJsObject.bEmptyDisplayPopup = false;
                            if (vueJsObject.htmlEditor.inlineMode)
                                vueJsObject.resizeToolbar();
                        };
                    }
                )(this); // Paramètre de la fonction renseignant l'objet dans la fonction interne

                var oBlurFct = (
                    function (vueJsObject) {
                        return function () {
                            let event = {
                                target: vueJsObject.CKEditorInstance.container.$
                            }
                            event.target.value = vueJsObject.CKEditorInstance.container.$.innerHTML
                            vueJsObject.verifMemo(event, vueJsObject, "blur")
                        };
                    }
                )(this); // Paramètre de la fonction renseignant l'objet dans la fonction interne
                if (this.CKEditorInstance != undefined) {
                    this.CKEditorInstance.xrmMemoEditor = this;
                    this.CKEditorInstance.on('instanceReady', oInstanceReadyFct);
                    this.CKEditorInstance.on('focus', oFocusFct);
                    this.CKEditorInstance.on('blur', oBlurFct);

                }
            }
        }
    },
    computed: {
        GetComponentId: function () {
            if (!this.GUID)
                this.GUID = Math.random().toString(36).substring(2, 15) + Math.random().toString(36).substring(2, 15); //http://stackoverflow.com/questions/105034/how-to-create-a-guid-uuid-in-javascript
            let ComponentId = 'eME_' + this.dataInput.DescId + "_" + this.GUID;
            return ComponentId;
        },
        IsDisplayReadOnly: function () {
            return this.propHead || this.dataInput.ReadOnly;
        },
        CKEditorInstance: function () {
            // Référence vers l'objet CKEditor instancié (mode HTML uniquement)
            // Tant que l'objet n'est pas initialisé, cette propriété est initialisée avec une fonction minimale permettant de recevoir les appels à eMemoEditor.htmlEditor.on()
            // alors que le composant n'est pas encore chargé. Puis, lorsque celui-ci le sera, les évènements stockés en attente y seront ensuite rattachés.
            // Cette référence est plus pratique que d'appeler systématiquement CKEDITOR.instances[strTextAreaId]
            return CKEDITOR.instances[this.GetComponentId];
        },
        /**
         * Permet d'afficher la valeur dans la textArea et de forcer le 
         * rafraichissement à chaque changement de valeur.
         * */
        ValueToDisplay() {
            return this.dataInput.Value
        },
        /**
         * Une div, mais sans HTML dedans.
         * Parce que sans html, la fête est plus folle...
         * */
        noHtml() {
            var StrippedString = this.dataInput.Value.replace(/(<([^>]+)>)/ig, "");
            return StrippedString
        }
    },
    methods: {
        updateMethod,
        insertSignature,
        insertMessage,
        getUserMessage,
        insertData,
        setData,
        getData,
        setCursorPosition,
        getMemoBody,
        getMemoDocument,
        trace,
        insertImg,
        verifComponent,
        /**
         * Quand une textArea reçoit le focus
         * */
        TextAreaFocus() {
            this.RemoveBorderSuccessError();
            this.bEmptyDisplayPopup = false;
            this.bShowOnFocus = true;
        },
        /**
         * Quand une textArea n'a plus le focus
         * */
        TextAreaBlur(event) {
            let that = this;
            this.verifMemo(event, this.that, 'blur');
            this.bShowOnFocus = false;
        },
        openMemo: function () {
            if (!this.dataInput.ReadOnly) {


                let options = {
                    id: "MotherOfAllModals",
                    class: "modal-motherofAll",
                    style: {
                        //heigth: document.querySelector('#MainWrapper') ? document.querySelector('#MainWrapper').offsetHeight + "px" : "",
                        width: document.querySelector('#MainWrapper') ? document.querySelector('#MainWrapper').offsetWidth + "px" : ""
                    },
                    actions: [],
                    header: {
                        text: this.dataInput.Label ? this.dataInput.Label : this.getRes(805),
                        class: "modal-header-motherofAll modal-header-motherofAll-Max",
                        btn: [
                            { name: 'maximize', class: "icon-restore titleButtonsAlignement", action: () => { this.resizeModale(); } },
                            {
                                name: 'close', class: "icon-edn-cross titleButtonsAlignement", action: () => {

                                    if (!(this.$root.$children && this.$root.$children.length > 0))
                                        return false;

                                    let appRoot = [...this.$root.$children].find(nd => nd.$options.name == "App")

                                    if (!(appRoot && appRoot.$children && appRoot.$children.length > 0))
                                        return false;

                                    let ficRoot = appRoot.$children.find(nd => nd.$options.name == "fiche");

                                    if (!ficRoot)
                                        return false;

                                    ficRoot.showMotherModal = false;
                                }
                            }
                        ]
                    },
                    main: {
                        class: "detailContent modal-content-motherofAll modal-content-motherofAll-Max",
                        componentsClass: "grid-container form-group ",
                        lstComponents: [
                            { input: this.dataInput, class: "input-line fname" },
                        ]
                    },
                    footer: {
                        class: "modal-footer-motherofAll modal-footer-motherofAll-Max",
                        btn: [
                            {
                                title: this.getRes(28), class: "btnvalid eudo-button btn btn-success", action: ($event) => {


                                    if (!(this.$root.$children && this.$root.$children.length > 0))
                                        return false;

                                    let appRoot = [...this.$root.$children].find(nd => nd.$options.name == "App")

                                    if (!(appRoot && appRoot.$children && appRoot.$children.length > 0))
                                        return false;

                                    let ficRoot = appRoot.$children.find(nd => nd.$options.name == "fiche");

                                    if (!(ficRoot && ficRoot.$children && ficRoot.$children.length > 0))
                                        return false;

                                    let MoMod = ficRoot.$children.find(nd => nd.$options.name == "MotherOfAllModals")

                                    /** Pour tous les composants trouvés dans Momod, on met à jour.
                                     * C'est pas très propre et il faudra revoir le système, mais pour le moment
                                     * on s'en contente. */
                                    let isHappyEnd = true;
                                    MoMod.$children.map(nd => nd.dataInput.IsHtml
                                        ? nd.CKEditorInstance.container.$.innerHTML
                                        : document.getElementById(nd.GetComponentId).value)
                                        .forEach((ndValue) => {
                                            if (ndValue) {
                                                try {
                                                    verifComponentModal(undefined, ndValue, this.dataInput.Value, this, this.dataInput)

                                                    /** Si on est dans une liste, on n'a pas de ckeditor à mettre à jour. */
                                                    if (!this.propListe) {
                                                        if (this.dataInput.IsHtml && this.CKEditorInstance && this.CKEditorInstance.container)
                                                            this.CKEditorInstance.container.$.innerHTML = ndValue;
                                                        else
                                                            document.getElementById(this.GetComponentId).value = ndValue;
                                                    }
                                                }
                                                catch (e) {
                                                    isHappyEnd = false;
                                                }
                                            }
                                        });

                                    ficRoot.showMotherModal = !isHappyEnd;

                                }
                            },
                            {
                                title: this.getRes(29), class: "btncancel eudo-button btn btn-default", action: () => {

                                    if (!(this.$root.$children && this.$root.$children.length > 0))
                                        return false;

                                    let appRoot = [...this.$root.$children].find(nd => nd.$options.name == "App")

                                    if (!(appRoot && appRoot.$children && appRoot.$children.length > 0))
                                        return false;

                                    let ficRoot = appRoot.$children.find(nd => nd.$options.name == "fiche");

                                    if (!ficRoot)
                                        return false;

                                    ficRoot.showMotherModal = false;
                                }
                            }
                        ]
                    },
                };
                this.modalOptions = options;
                // if (this.dataInput.IsHtml) {
                EventBus.$emit("MotherOfAllModals", options);
                //} else {
                //    alert("Open popup Memo NORMAL")
                //}
            }
        },

        // Redimensionne automatiquement l'éditeur et la barre d'outils au redimensionnement de la fenêtre et/ou affichage/masquage du menu droit
        autoResize: function () {
            var that = this;
            var rightMenu = document.getElementById('rightMenu');
            var observer = new MutationObserver(function (event) {
                if (event[0].target.classList.contains("FavLnkOpen")) {
                    setTimeout(function () {
                        that.resize();
                    }, 900);
                } else {
                    setTimeout(function () {
                        that.resize();
                    }, 750);
                }
            })

            observer.observe(rightMenu, {
                attributes: true
            })

            setTimeout(function () {
                that.resize();
            }, 900);
        },
        // Redimensionne l'éditeur et la barre d'outils aux dimensions indiquées (à la taille du conteneur si non précisées)
        resize: function (nNewWidth, nNewHeight) {
            this.resizeEditor(nNewWidth, nNewHeight);
            this.resizeToolbar(nNewWidth, nNewHeight);
        },
        /// Redimensionne l'éditeur aux dimensions données (à la taille du conteneur si non précisées)
        resizeEditor: function (nNewWidth, nNewHeight) {
            if (this.dataInput.IsHtml) {
                // Redimensionnement automatique en fonction du conteneur parent si les dimensions ne sont pas passées en paramètres

                if (this.CKEditorInstance && this.editor && this.editor.parentElement && this.editor.parentElementLabel) {
                    if (!nNewWidth)
                        nNewWidth = getNumber(getComputedStyle(this.editor.parentElement).width) - getNumber(getComputedStyle(this.editor.parentElementLabel).width);
                    if (!nNewHeight)
                        nNewHeight = getNumber(getComputedStyle(this.editor.parentElement).height) - getNumber(getComputedStyle(this.editor.parentElementLabel).height);
                }


                if (this.CKEditorInstance && this.CKEditorInstance.container && this.CKEditorInstance.container.$) {

                    // Redimensionnement via l'API CKEditor
                    try {
                        this.CKEditorInstance.resize(nNewWidth, nNewHeight);
                    }
                    catch (ex) {
                        // Avec VueJS, le resize() est parfois appelé alors que le composant n'est pas tout à fait prêt
                    }

                    // Et enfin, on affecte la nouvelle taille à la configuration, pour que'on puisse se baser dessus
                    this.htmlEditor.config.width = nNewWidth;
                    this.htmlEditor.config.height = nNewHeight;
                    // En complément, en mode Inline, le composant (devenu un div) voit sa taille s'ajuster automatiquement en fonction du contenu.
                    // Pour empêcher ce phénomène, il faut affecter une taille au div avec overflow pour afficher un ascenseur
                    if (this.htmlEditor.inlineMode) {
                        this.CKEditorInstance.container.$.style.height = this.htmlEditor.config.height;
                        this.CKEditorInstance.container.$.style.width = this.htmlEditor.config.width;
                        addClass(this.CKEditorInstance.container.$, "inlineMemoEditor");
                    }
                }
            }
        },
        // Masque la barre d'outils au scroll
        hideToolbarOnScroll: function () {
            // #30 023 - on indique au document de masquer la barre d'outils lorsqu'on fait défiler la page/certains éléments scrollables
            var oScrollFct = (function (vueJsObject) {
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
                    if (oScrolledObj && (this.CKEditorInstance && this.CKEditorInstance.container && (oScrolledObj != this.CKEditorInstance.container.$))) {
                        // On pourrait ici appeler hideShowToolbar() pour masquer la barre d'outils, mais il faudrait alors forcer son réaffichage lors du clic ou du focus dans le champ
                        // Faire un hideShowToolbar(true) sur le CKEDITOR.on('focus') est tout simple et fonctionne très bien...
                        // Mais l'évènement "clic" dans le champ est fortement restreint par CKEditor 4 : http://stackoverflow.com/questions/17045329/ckeditor-how-to-add-permanent-onclick-event
                        // et n'est pas toujours détecté. Or, il est indispensable en plus de focus() car comme le curseur reste dans le champ lors du défilement de la page,
                        // l'évènement focus n'est pas redéclenché lorsqu'on reclique dans le champ en espérant faire apparaître la barre d'outils.
                        // On préférera donc, ici, déclencher le onblur() du champ Mémo pour retirer le curseur du champ et, ainsi, déclencher le masquage de la barre d'outils avec le fonctionnement interne de CKEditor.
                        // Inconvénient : cela pourra déclencher la sauvegarde du champ et d'autres mécanismes lourds liés à la sortie de champ si définis.
                        vueJsObject.blur();
                        //eMemoEditorTargetObject.hideShowToolbar(false);
                    }
                };
            })(this);
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
                    setEventListener(oScrollableParentElements[i], 'scroll', oScrollFct, true);
                }
            }
        },
        // Passe la barre d'outils en mode invisible ou grisé si lecture seule
        setToolbarReadOnly: function () {
            var oToolbar = this.getToolbarContainer();
            /**/
            if (this.dataInput.ReadOnly) {
                addClass(this.CKEditorInstance.container.$, "readonly");

                if (oToolbar) {
                    oToolbar.style.display = 'none';
                }
            }
        },
        // Renvoie un pointeur vers l'élément HTML de la barre d'outils, principalement dans le but d'agir sur son affichage en mode inline
        getToolbarContainer: function () {
            // Parcours des noeuds enfant à la recherche de la barre d'outils, qui a un ID variable
            try {
                var oInstanciatedObject = document.getElementById('cke_' + this.GetComponentId);
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

        },
        // Redimensionne la barre d'outils à la taille spécifiée, ou en fonction de celle du champ Mémo (utile pour le mode Inline)
        resizeToolbar: function (nNewWidth) {
            var oToolbar = this.getToolbarContainer();
            if (oToolbar) {
                if (isNaN(nNewWidth)) {
                    // Taille du champ Mémo définie en pixels : on l'utilise directement pour le calcul de la taille
                    if (this.htmlEditor.config.width && typeof (this.htmlEditor.config.width) != "number" && this.htmlEditor.config.width.indexOf('%') == -1) {
                        nNewWidth = getNumber(this.htmlEditor.config.width);
                    }
                    // Taille du champ Mémo définie en pourcentage : on tente de récupérer la taille réelle
                    // (calculée en pixels par le navigateur) de l'objet via eTools.getAbsolutePosition();
                    else if (this.CKEditorInstance && this.CKEditorInstance.container) {
                        var realContainerSize = getAbsolutePosition(this.CKEditorInstance.container.$);
                        if (realContainerSize.w > 0) {
                            nNewWidth = getNumber(realContainerSize.w);
                        }
                    }
                }

                if (!isNaN(nNewWidth) && nNewWidth > 0) {
                    oToolbar.style.width = nNewWidth + 'px';
                }
            }
        },
        // Renvoie ou paramètre la barre d'outils à afficher sur le champ Mémo
        // - En mode HTML, le seul appel à la méthode paramètre l'objet config à destination de CKEditor
        // - En mode Texte brut, la fonction renvoie le code HTML des boutons à afficher sur la barre d'outils
        setToolbar: function () {
            var returnValue = false;
            // Barre d'outils HTML (syntaxe CKEditor) - personnalisable en fonction de la valeur de la propriété toolbarType
            // Ajouter ici les différentes barres d'outils souhaitées aux différents endroits de l'application, et renseigner depuis la page appelante la propriété toolbarType
            // pour afficher la barre d'outils en question
            if (this.dataInput.IsHtml) {
                // En mode "sans bordures", on masque la barre d'outils en retirant tous ses boutons, sauf si le mode inline est activé
                var bCanDisplayFullToolbar = true;
                if (!this.htmlEditor.inlineMode) {
                    if (this.htmlEditor.borderlessMode) {
                        bCanDisplayFullToolbar = false;
                        this.htmlEditor.config.toolbar = [];
                    }
                }
                if (bCanDisplayFullToolbar) {
                    var sourcePlugin = 'Source';
                    if (this.htmlEditor.inlineMode) {
                        sourcePlugin = 'Sourcedialog'; // https://dev.ckeditor.com/ticket/9713 - http://ckeditor.com/addon/sourcedialog
                    }
                    var speechPlugin = '';
                    if (this.htmlEditor.speechEnabled)
                        speechPlugin = 'Webspeech';

                    switch (this.htmlEditor.toolbarType) {
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

                            if (!this.htmlEditor.externalTrackingEnabled) {
                                switch (this.htmlEditor.toolbarType) {
                                    case "mailingtemplate":
                                        toolbarInsertButtons = ['Table', 'xrmImage', 'xrmUploadFiles', 'xrmUserSignature', 'Link', 'xrmLinkUnsubscribe', 'xrmLinkVisualization'];
                                        break;
                                    case "mailtemplate":
                                        toolbarInsertButtons = ['Table', 'xrmImage', 'xrmUserSignature', 'Link'];
                                        break;
                                    default:
                                        toolbarInsertButtons = ['Table', 'xrmImage', 'xrmUploadFiles', 'xrmUserSignature', 'Link', 'xrmLinkUnsubscribe', 'xrmLinkVisualization'];
                                        break;
                                }
                            }
                            else {
                                switch (this.htmlEditor.toolbarType) {
                                    case "mailingtemplate":
                                        toolbarInsertButtons = ['Table', 'xrmImage', 'xrmUserSignature', 'Link', 'xrmLinkUnsubscribe'];
                                        break;
                                    case "mailtemplate":
                                        toolbarInsertButtons = ['Table', 'xrmImage', 'xrmUserSignature', 'Link'];
                                        break;
                                    default:
                                        toolbarInsertButtons = ['Table', 'xrmImage', 'xrmUserSignature', 'Link', 'xrmLinkUnsubscribe'];
                                        break;
                                }
                            }

                            if (this.htmlEditor.formularEnabled && !this.htmlEditor.externalTrackingEnabled)
                                toolbarInsertButtons.push('xrmFormular');  //Formulaire

                            toolbarInsertButtons.push('xrmMergeFields', 'xrmInsertCSS');

                            // Tâche #2407 et #2447 - Bouton Caractères Spéciaux
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
                            this.htmlEditor.config.toolbar = [
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
                            this.htmlEditor.config.toolbar = [
                                ['xrmFullScreenDialog', 'Print'], ['Font', 'FontSize'], ['Bold', 'Italic', 'Underline', 'Strike'], ['BGColor', 'TextColor'], ['JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock'], ['NumberedList', 'BulletedList', 'Outdent', 'Indent'], ['PasteFromWord', 'Table', 'xrmImage', 'xrmUserSignature', 'Link', 'xrmMergeFields', 'xrmInsertCSS', 'SpecialChar'], [speechPlugin], [sourcePlugin]
                            ];
                            break;
                        case "automation":
                            this.htmlEditor.config.toolbar = [['Bold', 'Italic', 'Underline', 'Strike'], ['TextColor', 'xrmMergeFields', 'SpecialChar'], [speechPlugin], [sourcePlugin]];
                            this.htmlEditor.config.removePlugins = 'elementspath'; // Le bottom de ckeditor
                            break;
                        case "formular":
                            /*KHA le 28/04/2015 les CSS ne sont pas fonctionnelles sur les formulaires. on masque le bouton pour l'instant*/
                            this.htmlEditor.config.toolbar = [
                                ['xrmFullScreenDialog', 'Print'], ['Font', 'FontSize'], ['Bold', 'Italic', 'Underline', 'Strike'], ['BGColor', 'TextColor'], ['JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock'], ['NumberedList', 'BulletedList', 'Outdent', 'Indent'], ['PasteFromWord', 'Table', 'xrmImage', 'Link', 'xrmMergeFields', 'xrmInsertCSS', 'SpecialChar'], [speechPlugin], [sourcePlugin]
                            ];
                            break;
                        case "adminusersign":
                            this.htmlEditor.config.toolbar = [
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
                            this.htmlEditor.config.toolbar = [
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
                            if (this.htmlEditor.config.isFullScreen) {
                                this.htmlEditor.config.toolbar = [
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
                                this.htmlEditor.config.toolbar = [
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
            // Barre d'outils Texte Brut : géré du côté du template VueJS

            return returnValue;
        },
		/**
		* Demande #54 068 et Backlog #616 - Positionne une police par défaut sur le canvas de l'éditeur, sauf pour les cas où la mise en forme doit pouvoir être contrôlée
		* Notamment la signature utilisateur et l'édition de mails
		* Demande #78 433 - US #2 925 - Tâche #4 326 - Ajout d'une classe supplémentaire eMEFontEudoIRISSize_X pour appliquer la taille de police définie dans Mon Eudonet
		* @param {string} Si nécessaire, permet de surcharger la propriété editorType de l'objet en cours (censée être positionnée par le parent du composant), pour adapter le retour de la fonction selon le type d'éditeur (ex : "mail", "mailing", "adminusersign"
        * */
		setFontClass: function (strEditorType) {
			if (!strEditorType)
				strEditorType = this.editorType;

			if (
				strEditorType != "adminusersign" &&
				strEditorType != "mail" &&
				strEditorType != "mailing"
			)
				this.htmlEditor.config.bodyClass += ' eMEFontEudoIRIS eMEFontEudoIRISSize_' + this.paramWindow.GetParam("fontsize");
		},
		/**
		Applique, sur le conteneur du champ Mémo (div), les mêmes classes CSS que sur la propriété .config.bodyClass, mise à jour par setFontClass() ci-dessus
		Sur les CKEditor instanciés en inline, il semble nécessaire de l'ajouter également sur le conteneur après l'initialisation de l'instance (instanceReady). cf. setFontClassOnContainer()
		La propriété bodyClass ne semblant pas être prise en compte lorsqu'on instancie un CKEditor inline (ce qui est logique, puisque cette bodyClass est censée s'appliquer sur l'iframe interne de CKEditor, qui n'est pas utilisée en mode inline)
		Source : https://ckeditor.com/docs/ckeditor4/latest/api/CKEDITOR_config.html#cfg-bodyClass
		* */
		setFontClassOnContainer: function () {
			addClass(this.CKEditorInstance.container.$, this.CKEditorInstance.config.bodyClass);
		},		
	
        // Charge les ressources de l'application au sein du composant CKEditor (évite de modifier ses fichiers de ressources en .JS)
        getHTMLEditorLanguageResources: function () {
            if (this.dataInput.IsHtml && typeof (CKEDITOR) != "undefined" && typeof (CKEDITOR.lang) != "undefined") {
                var CKEditorCurrentLang = eval("CKEDITOR.lang." + this.htmlEditor.language);
                if (typeof (CKEditorCurrentLang) != "undefined") {

                    // Ressources de langues pour les plugins personnalisés
                    if (typeof (top._res_57) != 'undefined') { eval("CKEDITOR.lang." + this.htmlEditor.language + ".xrmUserMessage = top._res_57;"); }
                    eval("CKEDITOR.lang." + this.htmlEditor.language + ".xrmUserSignature = top._res_6593;");
                    eval("CKEDITOR.lang." + this.htmlEditor.language + ".xrmUploadFiles = top._res_5042;");
                    eval("CKEDITOR.lang." + this.htmlEditor.language + ".xrmLinkVisuOption = top._res_6594;");

                    eval("CKEDITOR.lang." + this.htmlEditor.language + ".xrmLinkFormular = top._res_1142;");
                    eval("CKEDITOR.lang." + this.htmlEditor.language + ".xrmDisableTrackingLink = top._res_6596;");
                    eval("CKEDITOR.lang." + this.htmlEditor.language + ".xrmLinkName = top._res_6597;");
                    eval("CKEDITOR.lang." + this.htmlEditor.language + ".xrmTrackingField = top._res_6598;");

                    eval("CKEDITOR.lang." + this.htmlEditor.language + ".xrmLinkVisualization = top._res_6925;");
                    eval("CKEDITOR.lang." + this.htmlEditor.language + ".xrmHelp = top._res_6187;");

                    if (this.htmlEditor.useNewUnsubscribeMethod) {
                        eval("CKEDITOR.lang." + this.htmlEditor.language + ".xrmLinkUnsubOption = top._res_1847;"); //Paramétrages d'abonnements
                        eval("CKEDITOR.lang." + this.htmlEditor.language + ".xrmLinkUnsubscribe = top._res_1861;");
                    }
                    else {
                        eval("CKEDITOR.lang." + this.htmlEditor.language + ".xrmLinkUnsubOption = top._res_6595;"); //Désinscription
                        eval("CKEDITOR.lang." + this.htmlEditor.language + ".xrmLinkUnsubscribe = top._res_6924;");
                    }

                    eval("CKEDITOR.lang." + this.htmlEditor.language + ".xrmMergeFields = {" +
                        "label: top._res_6484," +
                        "voiceLabel: top._res_6484," +
                        "panelTitle: top._res_6599" +
                        "};");

                    eval("CKEDITOR.lang." + this.htmlEditor.language + ".xrmSpecialMergeFields = {" +
                        "label: top._res_716," +
                        "voiceLabel: top._res_6484," +
                        "panelTitle: top._res_6599" +
                        "};");

                    eval("CKEDITOR.lang." + this.htmlEditor.language + ".xrmFormularMergeFields = CKEDITOR.lang." + this.htmlEditor.language + ".xrmMergeFields;");
                    eval("CKEDITOR.lang." + this.htmlEditor.language + ".xrmActions = {" +
                        "label: top._res_296," + // Actions
                        "voiceLabel: top._res_296," + // Actions
                        "panelTitle: top._res_6600" + // Actions utilisateur et modèles de formulaires
                        "};");
                    eval("CKEDITOR.lang." + this.htmlEditor.language + ".xrmFullScreen = top._res_6601;"); // Plein écran
                    eval("CKEDITOR.lang." + this.htmlEditor.language + ".xrmFullScreenDialog = top._res_6602;"); // Plein écran (nouvelle fenêtre)
                    eval("CKEDITOR.lang." + this.htmlEditor.language + ".xrmImage = top._res_712;"); // Insérer une image
                    eval("CKEDITOR.lang." + this.htmlEditor.language + ".xrmFormular = top._res_6610;"); // Formulaires

                    if (typeof (top._res_6198) != 'undefined') {
                        eval("CKEDITOR.lang." + this.htmlEditor.language + ".xrmLink = {" +
                            "cookieTracking: top._res_6198" +
                            "};");
                    }
                    eval("CKEDITOR.lang." + this.htmlEditor.language + ".xrmInsertCSS = top._res_6603;"); // Feuilles de style
                    eval("CKEDITOR.lang." + this.htmlEditor.language + ".xrmFormularMergeFieldsWarnings = {" +
                        "mergeFieldExists: top._res_6926," +
                        "buttonExists: top._res_6927" +
                        "};");
                    eval("CKEDITOR.lang." + this.htmlEditor.language + ".xrmFormularReadWriteDialog = {" +
                        "title: top._res_6484," +
                        "radioLabel: top._res_6928," +
                        "radioButtonRead: top._res_1599," +
                        "radioButtonWrite: top._res_1600" +
                        "};");
                    if (this.htmlEditor.speechEnabled)
                        eval("CKEDITOR.lang." + this.htmlEditor.language + ".ckWebSpeech = {" +
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
                    eval("CKEDITOR.lang." + this.htmlEditor.language + ".xrmBrowse = top._res_6498;"); // Backlog #315

                    // Création de variables "raccourcis" vers les noms de certaines commandes pour la fonction getCommandDescription
                    // A définir lorsqu'il n'existe pas, dans les fichiers de langue CKEditor, de variable portant directement le nom de la commande
                    // Ces variables additionnelles sont préfixées par un objet "xrmToolTip" afin que leur définition ne masque pas une autre propriété
                    // déclarée avec le même nom dans le code source de CKEditor
                    eval("CKEDITOR.lang." + this.htmlEditor.language + ".xrmToolTip = { };"); // création de l'objet
                    eval("CKEDITOR.lang." + this.htmlEditor.language + ".xrmToolTip.bold = CKEDITOR.lang." + this.htmlEditor.language + ".basicstyles.bold;");
                    eval("CKEDITOR.lang." + this.htmlEditor.language + ".xrmToolTip.italic = CKEDITOR.lang." + this.htmlEditor.language + ".basicstyles.italic;");
                    eval("CKEDITOR.lang." + this.htmlEditor.language + ".xrmToolTip.underline = CKEDITOR.lang." + this.htmlEditor.language + ".basicstyles.underline;");
                    eval("CKEDITOR.lang." + this.htmlEditor.language + ".xrmToolTip.justifyleft = (CKEDITOR.lang." + this.htmlEditor.language + ".justify ? CKEDITOR.lang." + this.htmlEditor.language + ".justify.left : CKEDITOR.lang." + this.htmlEditor.language + ".common.left);");
                    eval("CKEDITOR.lang." + this.htmlEditor.language + ".xrmToolTip.justifycenter = (CKEDITOR.lang." + this.htmlEditor.language + ".justify ? CKEDITOR.lang." + this.htmlEditor.language + ".justify.center : CKEDITOR.lang." + this.htmlEditor.language + ".common.center);");
                    eval("CKEDITOR.lang." + this.htmlEditor.language + ".xrmToolTip.justifyright = (CKEDITOR.lang." + this.htmlEditor.language + ".justify ? CKEDITOR.lang." + this.htmlEditor.language + ".justify.right : CKEDITOR.lang." + this.htmlEditor.language + ".common.right);");
                    eval("CKEDITOR.lang." + this.htmlEditor.language + ".xrmToolTip.justifyblock = (CKEDITOR.lang." + this.htmlEditor.language + ".justify ? CKEDITOR.lang." + this.htmlEditor.language + ".justify.block : CKEDITOR.lang." + this.htmlEditor.language + ".common.justify);");
                    eval("CKEDITOR.lang." + this.htmlEditor.language + ".xrmToolTip.source = CKEDITOR.lang." + this.htmlEditor.language + ".sourcearea.toolbar;");
                    eval("CKEDITOR.lang." + this.htmlEditor.language + ".xrmToolTip.replace = CKEDITOR.lang." + this.htmlEditor.language + ".find.replace;");
                    eval("CKEDITOR.lang." + this.htmlEditor.language + ".xrmToolTip.redo = CKEDITOR.lang." + this.htmlEditor.language + ".undo.redo;");

                    // Affichage des raccourcis clavier disponibles en tant que tooltip du champ Mémo
                    var strMemoTooltip = 'CKEDITOR.lang.' + this.htmlEditor.language + '.xrmHelpContents = "';
                    // Les raccourcis clavier sont censés être définis à ce stade, puisque la fonction getLanguageResources() doit être appelée en différé après le chargement initial des
                    // fichiers de langue de CKEditor, ce qui intervient après la phase d'initialisation de l'objet eMemoEditor où est définie la propriété keystrokes
                    if (this.htmlEditor.config && this.htmlEditor.config.keystrokes && this.htmlEditor.config.keystrokes.length > 0) {
                        for (var i = 0; i < this.htmlEditor.config.keystrokes.length; i++) {
                            var nMaxCharCode = 65536;
                            var oKS = this.htmlEditor.config.keystrokes[i];
                            var nKS = oKS[0];
                            var strKS = "";
                            var bUsesCTRL = (((nKS - CKEDITOR.CTRL) > 0) && ((nKS - CKEDITOR.CTRL) < nMaxCharCode));
                            var bUsesALT = (((nKS - CKEDITOR.ALT) > 0) && ((nKS - CKEDITOR.ALT) < nMaxCharCode));
                            var bUsesSHIFT = (((nKS - CKEDITOR.SHIFT) > 0) && ((nKS - CKEDITOR.SHIFT) < nMaxCharCode));
                            if (bUsesCTRL) { nKS -= CKEDITOR.CTRL; strKS += "CTRL + "; }
                            if (bUsesALT) { nKS -= CKEDITOR.ALT; strKS += "ALT + "; }
                            if (bUsesSHIFT) { nKS -= CKEDITOR.SHIFT; strKS += "SHIFT + "; }
                            var strKeyDesc = strKS + getKeyDescFromCharCode(nKS);
                            var strCommandDesc = this.getHTMLEditorCommandDescription(oKS[1]);
                            this.htmlEditor.toolbarButtonKeyStrokes[oKS[1].toLowerCase()] = strKeyDesc; // on ajoute le nom de la commande en minuscules, car
                            // la fonction qui utilisera ce tableau de chaînes obtiendra le nom de la commande à partir de la classe CSS du bouton
                            // de barre d'outils, qui est en minuscules
                            strMemoTooltip += strKeyDesc + " => " + strCommandDesc + "<br>";
                        }
                    }
                    strMemoTooltip += '";';
                    eval(strMemoTooltip);
                    CKEDITOR.config.title = '';
                }
            }
        },
        // Charge une description des commandes/raccourcis gérés par l'éditeur, pour les infobulles d'aide
        getHTMLEditorCommandDescription: function (strCommand) {
            var strCommandDesc = strCommand;
            eval(
                "if (CKEDITOR.lang." + this.htmlEditor.language + ") {" +
                "if (CKEDITOR.lang." + this.htmlEditor.language + ".xrmToolTip && CKEDITOR.lang." + this.htmlEditor.language + ".xrmToolTip." + strCommand + ") {" +
                "strCommandDesc = CKEDITOR.lang." + this.htmlEditor.language + ".xrmToolTip." + strCommand + ";" +
                "}" +
                "else if (CKEDITOR.lang." + this.htmlEditor.language + "." + strCommand + ") {" +
                "if (CKEDITOR.lang." + this.htmlEditor.language + "." + strCommand + ".title) {" +
                "strCommandDesc = CKEDITOR.lang." + this.htmlEditor.language + "." + strCommand + ".title;" +
                "}" +
                "if (CKEDITOR.lang." + this.htmlEditor.language + "." + strCommand + ".toolbar) {" +
                "strCommandDesc = CKEDITOR.lang." + this.htmlEditor.language + "." + strCommand + ".toolbar;" +
                "}" +
                "if (CKEDITOR.lang." + this.htmlEditor.language + "." + strCommand + "." + strCommand + ") {" +
                "strCommandDesc = CKEDITOR.lang." + this.htmlEditor.language + "." + strCommand + "." + strCommand + ";" +
                "}" +
                "if (strCommandDesc == strCommand) {" +
                "strCommandDesc = CKEDITOR.lang." + this.htmlEditor.language + "." + strCommand + ";" +
                "}" +
                "}" +
                "}"
            );
            return strCommandDesc;
        },
        // Charge les plugins utilisés par l'éditeur
        loadHTMLEditorPlugins: function (strCustomPluginList) {
            if (this.dataInput.IsHtml) {
                if (typeof (strCustomPluginList) == 'undefined' || !strCustomPluginList || strCustomPluginList == '') {
                    // En mode Plein écran, inutile d'afficher le bouton Zoom
                    // TOOD: variables isFullScreen, editorType, formularEnabled, automationEnabled, speechEnabled à adapter après cablage
                    var xrmFullScreen = this.isFullScreen ? '' : ',xrmFullScreen';

                    // Backlog #653 - Le bouton CSS n'est plus affiché sous CKEditor si on utilise grapesjs, qui gère les CSS autrement
                    // On doit tester la compatibilité navigateur EN plus de tester enableTemplateEditor, car il s'agit ici de l'activer sur le CKEditor non lié à
                    // grapesjs, sur lequel cette variable est donc à false
                    // Demande #72 138/#72 207 - Le bouton reste finalement
                    var xrmInsertCSS = (
                        this.htmlEditor.editorType == 'mail'
                        || this.editorType == 'mailing'
                        || this.editorType == 'formular'
                        || this.editorType == 'formularsubmission'
                        || this.editorType == 'mailtemplate'
                        || this.editorType == 'mailingtemplate') ? ',xrmInsertCSS' : '';

                    strCustomPluginList = 'sourcedialog,nbsp' + xrmFullScreen + ',xrmUserMessage,xrmUserSignature,xrmUploadFiles,xrmImage,print';

                    if (this.htmlEditor.formularEnabled)
                        strCustomPluginList = strCustomPluginList + ',xrmFormular';

                    if (this.htmlEditor.automationEnabled)
                        strCustomPluginList = strCustomPluginList + ',xrmMergeFields';
                    else
                        strCustomPluginList = strCustomPluginList + ',xrmFormularReadWrite' + xrmInsertCSS + ',xrmMergeFields,xrmSpecialMergeFields,xrmLinkAdapter,xrmLinkUnsubscribe,xrmLinkVisualization,xrmImageAdapter';

                    if (this.speechEnabled)
                        strCustomPluginList = strCustomPluginList + ',ckwebspeech';

                    strCustomPluginList = strCustomPluginList + ',xrmHelp';
                }

                // Ajout des plugins sur la configuration globale de CKEditor
                CKEDITOR.config.extraPlugins = strCustomPluginList;
            }
        },
        // Affecte un skin spécifique au composant en fonction de ses modes d'affichage
        setSkin: function (strSkin) {
            if (this.dataInput.IsHtml) {
                if (!strSkin || strSkin == '') {
                    // TODO: skin spécifique compactMode ?
                    if (this.htmlEditor.borderlessMode) {
                        strSkin = 'eudonet-mini';
                    }
                    else {
                        strSkin = 'bootstrapck';
                    }
                }
            }

            this.htmlEditor.config.skin = strSkin;
        },
        insertUserMessage: function () {
            this.insertMessage();
        },
        switchFullScreen: function () {
            this.openMemo();
        },
        insertImage: function () {
            this.insertImg(true);
        },
        verifMemo(event, that, from) {
            // #96 278 - Si la demande de mise à jour se fait parce que le curseur est sorti du champ pour afficher la popup d'ajout d'image ou de plein écran, on court-circuite la demande
            // Le rafraîchissement qui résulte de cette MAJ pouvant alors faire complètement échouer les actions menées par la popup ouverte.
            // La mise à jour sera redéclenchée à la fin du traitement de la popup en question, ou lors de n'importe quelle autre sortie du champ.
            // Il n'y a donc pas lieu d'en faire une au moment d'ouvrir une fenêtre destinée à effectuer des modifications sur le contenu
            if (from == "blur" && (this.cptFromModal || this.isImageModalOpened())) {
                return false;
            }

            verifComponent(undefined, event, this.dataInput.Value, that, this.dataInput);
        },
        focus,
        help,
        hideShowToolBar,
        onUpdateCallback,
        RemoveBorderSuccessError,
        showInformationIco,
        displayInformationIco,
        resizeModale: function () {
            this.modalOptions.header.class = "modal-header-motherofAll modal-header-motherofAll-" + (!this.bmaximize ? 'Max' : 'Min');
            this.modalOptions.header.btn.find(t => t.name == 'maximize').class = (!this.bmaximize ? "icon-restore" : "icon-maximize") + " titleButtonsAlignement";
            this.modalOptions.main.class = "detailContent modal-content-motherofAll modal-content-motherofAll-" + (!this.bmaximize ? 'Max' : 'Min');
            this.modalOptions.footer.class = "modal-footer-motherofAll modal-footer-motherofAll-" + (!this.bmaximize ? 'Max' : 'Min');
            this.bmaximize = !this.bmaximize;
        },
        /**
         * Indique si la popup d'ajout d'image dans le champ Mémo (E17) est actuellement instanciée ET ouverte (ou non).
         * Instanciée : le pointeur top.modalImage est un objet eModalDialog (= la popup a été créée au moins une fois)
         * Visible/ouverte : l'UID de cette eModalDialog doit être référencée dans le tableau global E17 top.ModalDialogs (quand on fait .hide() sur eModalDialog, cette référence est remise à null, mais l'objet top.modalImage ne n'est pas forcément, car il est sous la responsabilité du parent déclarant).
         * */
        isImageModalOpened() {
            return top.modalImage && top.ModalDialogs && top.ModalDialogs[top.modalImage.UID];
        }
    },
    props: ["dataInput", "propHead", "propListe", "propSignet", "propIndexRow", "propDetail", "cptFromModal"],
}