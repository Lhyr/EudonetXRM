import { RemoveBorderSuccessError, verifComponent, updateMethod, showInformationIco, displayInformationIco } from '../../methods/eComponentsMethods.js?ver=803000';
import { onUpdateCallback } from '../../methods/eFieldEditorMethods.js?ver=803000';
import { getTabDescid } from "../../methods/eMainMethods.js?ver=803000";
import { insertSignature, getUserMessage, insertMessage, insertData, setData, getData, setCursorPosition, getMemoDocument, getMemoBody, trace, insertImg, help, hideShowToolBar, focus, injectCSS, setCss } from '../../methods/CKEditorMethods.js?ver=803000';
import EventBus from '../../bus/event-bus.js?ver=803000';
import { verifComponentModal } from '../../methods/eModalComponents.js?ver=803000';
import { eFileComponentsMixin } from '../../mixins/eFileComponentsMixin.js?ver=803000';
import eAxiosHelper from "../../helpers/eAxiosHelper.js?ver=803000";
import { getValueWithoutHTML } from '../../shared/XRMWrapperModules.js?ver=803000';

export default {
    name: "eMemo",
    data() {
        return {
            nPaddingOfRubrique: 12,
            nHeightOfRubrique: 46,
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
            bmaximize: true,
            oBlurFct: null,
            customCSS: ""
        };
    },
    watch: {
        memoOpenedFullMode: function (val) {
            if(val)
                this.openMemo();
        }
    },
    components: {
        eAlertBox: () => import(AddUrlTimeStampJS("../modale/alertBox.js")),
        eMemoFile: () => import(AddUrlTimeStampJS("./eMemo/eMemoFile.js")),
        eMemoList: () => import(AddUrlTimeStampJS("./eMemo/eMemoList.js")),

    },
    mixins: [eFileComponentsMixin],
    mounted() {
        this.setContextMenu();
        this.displayInformationIco();
        if (!this.propListe) {
            this.$refs["dvEMemo"].addEventListener('resize', this.autoResize);


            // Repr??sente des valeurs internes au composant eMemo, en mode Texte brut ET HTML
            this.editor = {};
            this.editor.parentElement = findUpByClass(document.getElementById(this.GetComponentId), "rubriqueEmemo");
            this.editor.parentElementLabel = this.editor.parentElement ? this.editor.parentElement.querySelector(".left") : null;
            this.textEditor = document.getElementById(this.GetComponentId);
			this.paramWindow = getParamWindow(); // pointeur vers eParamIFrame pour r??cup??rer certains param??tres (notamment ceux de Mon Eudonet : signatures, taille de police...)

            if (this.dataInput.IsHtml) {

                // Repr??sente des valeurs internes au composant eMemo, en mode HTML uniquement
                this.htmlEditor = {};
                this.htmlEditor.inlineMode = true;
                this.htmlEditor.compactMode = false;
                this.htmlEditor.borderlessMode = false;
                this.htmlEditor.toolbarButtonKeyStrokes = new Object();
                this.htmlEditor.language = 'fr';
                this.htmlEditor.mainColor = getComputedStyle(document.documentElement).getPropertyValue('--main-color').trim(); // cf. CSS des th??mes 2019
                this.htmlEditor.baseWidth = CKEDITOR.config.width;
                this.htmlEditor.baseHeight = CKEDITOR.config.height;

                // R??cup??ration des ressources de langue Eudo pour les ajouter ?? celles de CKEditor
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

                // MAJ de la variable globale c??t?? CKEditor
                CKEDITOR.config.language = this.htmlEditor.language;

                // Initialisation configuration g??n??rale (globale ?? toutes les instances)
                CKEDITOR.config.fontSize_sizes = '8/8pt;9/9pt;10/10pt;11/11pt;12/12pt;14/14pt;16/16pt;18/18pt;20/20pt;22/22pt;24/24pt;26/26pt;28/28pt;36/36pt;48/48pt;72/72pt';

                // Cr??ation d'une configuration sp??cifique ?? l'instance ?? partir de la configuration globale
                // Il faut pour cela cr??er un nouvel objet vide, qui ne contiendra que les param??tres de configuration sp??cifiques ?? l'instance ;
                // Lorsqu'il cr??era l'instance, CKEditor prendra la config de base et la surchargera avec les propri??t??s d??finies ci-dessous.
                // Il n'est donc pas n??cessaire de cr??er un objet d??j?? initialis?? avec toutes les propri??t??s possibles.
                // En revanche, il est indispensable de d??clarer ici les propri??t??s utilis??es par le champ M??mo en mode texte brut
                this.htmlEditor.config = {
                    width: this.htmlEditor.baseWidth,
                    height: this.htmlEditor.baseHeight,
                    removePlugins: "magicline,elementspath",
                    resize_enabled: false
                };

                // Param??trage de la barre d'outils
                this.setToolbar();

                // Configuration sp??cifique ?? l'instance : skin
                this.setSkin();

                // D??sactivation du redimensionnement du contr??le ?? la vol??e
                this.htmlEditor.config.resize_enabled = false;

                // D??sactivation du redimensionnement en fonction du contenu
                this.htmlEditor.config.autoGrow_onStartup = false,

                    // Lecture seule
                    this.htmlEditor.config.readOnly = this.dataInput.ReadOnly;

                // #44357 : Activer la compatibilit?? pour l'environnement mobile (notamment Android)
                if (CKEDITOR.env.mobile) {
                    CKEDITOR.env.isCompatible = true;
                }
                // On d??finit la couleur de la r??glette
                // http://ckeditor.com/ckeditor_4.3_beta/samples/plugins/magicline/magicline.html
                //this.htmlEditor.config.magicline_color = this.htmlEditor.mainColor;

                this.htmlEditor.config.resize_enabled = false;

                // #32 646 - Ajout de polices ?? CKEditor
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

                // Appuyer sur Entr??e ins??re un retour ?? la ligne simple
                this.htmlEditor.config.enterMode = CKEDITOR.ENTER_BR;

                // Plugin de dict??e vocale
                // TOCHECK: On consid??re arbitrairement qu'il existe et que l'on utilise une culture correspondant ?? la langue s??lectionn??e, sous la forme lang-LANG
                this.htmlEditor.config.ckwebspeech = { 'culture': this.htmlEditor.language + '-' + this.htmlEditor.language.toUpperCase() };

                // Full Page Mode : pour conserver les ent??tes de HTML : Doctype, head, meta...
                // #53 136 - Il faut appliquer ce mode uniquement sur les CKEditor n??cessitant d'??diter un code source complet/une page Web avec ent??tes et CSS
                // Exemples : mod??les de formulaires, mod??les de mails...
                // Il ne faut pas activer ce mode dans les autres cas (ex : signature de l'utilisateur au CTRL+E, rubriques de type M??mo) car cela provoque l'ajout
                // syst??matique de balises <html><head><body> au code g??n??r??, qui devient alors invalide d??s qu'il s'agit de l'incorporer ?? d'autres contenus
                // Comme l'editorType n'est pas connu au moment de l'initialisation d'eMemoEditor (la propri??t?? editorType est param??tr??e apr??s l'appel ?? new eMemoEditor dans
                // eMain.js), la propri??t?? fullPage sera param??tr??e au moment de l'instanciation de CKEditor, dans la m??thode show(), par un appel ?? setFullPageMode()
                // this.htmlEditor.config.fullPage = true;

                // GMA - 20140117 (src : http://stackoverflow.com/questions/18185627/ckeditor-changing-content-automatically)
                // Evite la modification du contenu HTML apr??s appui sur le bouton Source
                // MAB - #52 816 - 20170202 - On interdit l'interpr??tation du tag <base> qui provoque la redirection de toutes les URLs relatives de la page vers la racine
                // indiqu??e par ce tag. Ex : <base href="https://www.eudonet.com/" target="_self" /> provoquerait la redirection de tout lien type mgr/eManager.ashx vers
                // https://www.eudonet.com/mgr/eManager.ashx au lieu de https://serveur/xrm/mgr/eManager.ashx
                // Toutefois, passer allowedContent ?? true d??sactive compl??tement l'ACF (Advanced Content Filter) de CKEditor 4, et il est donc impossible
                // de tout autoriser sauf certains tags en utilisant disallowedContent (qui n??cessite que l'ACF soit actif).
                // Pour autoriser l'ajout de contenu normalement filtr?? tout en excluant d'autres tags, il faut donc conserver l'ACF en d??finissant une r??gle autorisant
                // tout, puis utiliser disallowedContent pour lister les tags ?? filtrer.
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
                // CRU - #51 978 - D??sactivation de SCAYT car non utilis?? et en conflit avec le mode FullPage qui retire la balise <style>
                this.htmlEditor.config.scayt_autoStartup = false;

                // Ajout des CSS de l'application ?? l'iframe pour que son style (dont la police par d??faut) soit le m??me que celle de l'application
                // On ajoute une classe au body de l'iframe CKEditor pour pouvoir d??clarer dans les CSS des styles ?? appliquer au corps de page sans ??craser ceux de l'application (document courant)
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

                this.htmlEditor.config.bodyClass = `eME rowspan-${this.dataInput?.Rowspan}`;
				
				// Ajout de la classe CSS pour que la police d'??criture soit coh??rente avec le reste de l'application
				// NB : sur les CKEditor instanci??s en inline, il semble n??cessaire de l'ajouter ??galement sur le conteneur apr??s l'initialisation de l'instance (instanceReady). cf. setFontClassOnContainer()
				// La propri??t?? bodyClass ne semblant pas ??tre prise en compte lorsqu'on instancie un CKEditor inline (ce qui est logique, puisque cette bodyClass est cens??e s'appliquer sur l'iframe interne de CKEditor, qui n'est pas utilis??e en mode inline)
				// Source : https://ckeditor.com/docs/ckeditor4/latest/api/CKEDITOR_config.html#cfg-bodyClass
				this.setFontClass();

                // Pour passer un param??tre (ici, this, indiqu?? en fin de ligne) ?? l'int??rieur d'une fonction anonyme, il faut l'encapsuler dans une variable JS
                // qui sera un pointeur vers un appel de fonction comportant le param??tre en question
                var afterLangLoad = (
                    function (vueJsObject) {
                        return function () {
                            vueJsObject.getHTMLEditorLanguageResources();
                            vueJsObject.loadHTMLEditorPlugins();
                        };
                    }
                )(this); // Param??tre de la fonction renseignant l'objet dans la fonction interne

                // On force le chargement via CKEDITOR.lang.load pour les plugins
                CKEDITOR.lang.load(this.htmlEditor.language, this.htmlEditor.language, afterLangLoad);

                // On d??truit toute instance de CKEditor pr??alablement existante avec le m??me ID
                // Evite des erreurs JS
                var strTextAreaId = this.GetComponentId;
                if (document.getElementById(strTextAreaId)) {
                    // Destruction de toute instance existante avec ce nom
                    // On englobe ceci dans un try/catch car CKEditor 4 semble parfois provoquer une erreur JS, comme s'il proc??dait
                    // lui-m??me, de temps en temps, ?? la destruction d'instances inutilis??es.
                    // Sur CKEditor 3, la cr??ation d'un CKEditor via CKEDITOR.replace ne pouvait pas se faire si l'instance nomm??e existait d??j?? ;
                    // Sur CKEditor 4 ?? priori, pas de blocage ?? l'id??e de recr??er une instance d??j?? existante. Sauf qu'en r??alit??, cela provoque
                    // la g??n??ration, dans le DOM, de plusieurs objets ayant le m??me ID. Ce qui pose probl??me lorsqu'on fait des getElementById()
                    // pour r??cup??rer des ??l??ments de CKEditor sans passer par son API (notamment pour getToolbarContainer())

                    // Sur VueJS et autres, la non-destruction pr??alable entra??ne des erreurs JS
                    // https://stackoverflow.com/questions/19328548/ckeditor-uncaught-typeerror-cannot-call-method-unselectable-of-null-in-emberj/30668990

                    try {
                        if (CKEDITOR.instances[strTextAreaId]) {
                            CKEDITOR.instances[strTextAreaId].removeAllListeners();
                            CKEDITOR.remove(CKEDITOR.instances[strTextAreaId]);
                            //CKEDITOR.instances[strTextAreaId].destroy(true);
                        }
                    }
                    catch (ex) {
                        console.log(ex);
                    }
                }

                // On met Ckeditor en lecture seule si le composant l'est. Pr??c??demment, on passait par du css mais ce n'est pas optimal
                // car cela bloque aussi le scroll si le texte d??passe
                if (this.dataInput.isHtml && this.dataInput.ReadOnly) {
                    this.htmlEditor.config.readonly = true;
                }
                
                // ET ENFIN, MESDAMES, MESSIEURS, CKEDITOR
                if (document.getElementById(this.GetComponentId)) {
                    if (this.htmlEditor.inlineMode && this.$parent.$options.name != "MotherOfAllModals") {                       
                        CKEDITOR.inline(this.GetComponentId, this.htmlEditor.config);
                    }
                    else
                        CKEDITOR.replace(this.GetComponentId, this.htmlEditor.config);
                }

                // A l'instanciation pr??te du composant, ex??cution d'autres traitements
                var oInstanceReadyFct = (
                    function (vueJsObject) {
                        return function () {
                            // A l'instanciation de CKEditor, on met ?? jour dataInput.Value avec la valeur du champ telle que r??interpr??t??e par CKEditor,
                            // pour ne pas consid??rer que la valeur ait pu changer ?? la sortie du champ (oBlurFct) si la seule diff??rence entre la valeur
                            // pr??sente en base, et la valeur renvoy??e par CKEditor, est d??e ?? son interpr??tation de la valeur en HTML (ex : "" en base
                            // devient "<br>" une fois pass??e ?? CKEditor)
                            if (typeof (vueJsObject.CKEditorInstance) != "undefined") {
                                vueJsObject.dataInput.Value = vueJsObject.CKEditorInstance.getData();
                            }
                            vueJsObject.autoResize();
                            vueJsObject.hideToolbarOnScroll();
							vueJsObject.setFontClassOnContainer();
                        };
                    }
                )(this); // Param??tre de la fonction renseignant l'objet dans la fonction interne

                // Fonction ?? ex??cuter lors de la prise de focus sur le champ M??mo
                // 29 860 et 31 389 - Redimensionnement de la barre d'outils si le champ M??mo est affich?? en mode Inline
                // On le fait ?? chaque prise de focus au cas o?? le champ M??mo aurait ??t?? redimensionn?? depuis le dernier affichage
                var oFocusFct = (
                    function (vueJsObject) {
                        return function () {
                            vueJsObject.RemoveBorderSuccessError()
                            vueJsObject.bEmptyDisplayPopup = false;
                            if (vueJsObject.htmlEditor.inlineMode)
                                vueJsObject.resizeToolbar();
                        };
                    }
                )(this); // Param??tre de la fonction renseignant l'objet dans la fonction interne

                this.oBlurFct = (
                    function (vueJsObject) {
                        return function () {
                            let event = {
                                target: vueJsObject.CKEditorInstance.container.$
                            }
                            event.target.value = vueJsObject.CKEditorInstance.getData();
                            try {
                                vueJsObject.verifMemo(event, vueJsObject, "blur")
                            } catch (e) {
                                top.eAlert(1, top._res_416, e.Message, e.Stack);
                                return;
                            }
                        };
                    }
                )(this); // Param??tre de la fonction renseignant l'objet dans la fonction interne
                if (typeof (this.CKEditorInstance) != "undefined") {
                    this.CKEditorInstance.xrmMemoEditor = this;
                    this.CKEditorInstance.on('instanceReady', oInstanceReadyFct);
                    this.CKEditorInstance.on('focus', oFocusFct);
                    this.CKEditorInstance.on('blur', this.oBlurFct);

                }
            }


            if (this.cssClassText)
                this.injectCSS(this.cssClassText);
        }
    },
    computed: {
        getHeight: function () {
            return this.cptFromModal == undefined && this.cptFromAside == undefined ? (this.dataInput.Rowspan * this.nHeightOfRubrique) + this.nPaddingOfRubrique + 'px': '';
        },
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
            // R??f??rence vers l'objet CKEditor instanci?? (mode HTML uniquement)
            // Tant que l'objet n'est pas initialis??, cette propri??t?? est initialis??e avec une fonction minimale permettant de recevoir les appels ?? eMemoEditor.htmlEditor.on()
            // alors que le composant n'est pas encore charg??. Puis, lorsque celui-ci le sera, les ??v??nements stock??s en attente y seront ensuite rattach??s.
            // Cette r??f??rence est plus pratique que d'appeler syst??matiquement CKEDITOR.instances[strTextAreaId]
            return CKEDITOR.instances[this.GetComponentId];
        },
        /**
         * Permet d'afficher la valeur dans la textArea et de forcer le 
         * rafraichissement ?? chaque changement de valeur.
         * */
        ValueToDisplay() {
            return this.dataInput.Value
        },
        /**
         * Une div, mais sans HTML dedans.
         * Parce que sans html, la f??te est plus folle...
         * Utilisation d'une fonction wrapper d??clar??e dans XRMWrapperModules, qui utilisera le code d??j?? employ?? ailleurs dans l'application pour
         * ce type de traitement
         * */
        noHtml() {
            return this.getValueWithoutHTML(this.dataInput.Value);
        },
        isResizeable() {
            return this.$attrs.resize ? this.$attrs.resize : 'none';
        }
    },
    methods: {
        getTabDescid,
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
        injectCSS,
        setCss,
        verifComponent,
        getValueWithoutHTML,
        /**
         * Quand une textArea re??oit le focus
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
        async openMemo(evt) {

                // Request 90 164 - Appel avec une promesse de la m??thode verifComponent afin de d??clencher cette derni??re et udpateMethod avant de r??up??rer les valeurs du back via getFullMemoValue
                // #94 050 - Ne surtout pas faire ??a si l'??l??ment sur lequel on clique n'est pas un champ de saisie ??ditable (ex : champ M??mo affich?? en signet) !
                // Sinon, la m??thode verifComponent() risque d'appeler updateMethod() avec un contenu potentiellement alt??r?? (car rendu en lecture seule), ce qui modifiera irr??m??diablement le contenu du champ M??mo => PERTE DE DONNEES !
                if (!this.dataInput.ReadOnly && (evt?.target?.tagName.toUpperCase() == "INPUT" || evt?.target?.tagName.toUpperCase() == "TEXTAREA")) {
                    await (async () => {
                        await this.verifComponent(undefined, evt, this.dataInput.Value, this, this.dataInput, null, 'fullModeBtn');
                    })();
                }

                let dataInputWithFullValue = this.dataInput;
                dataInputWithFullValue.Value = await this.getFullMemoValue(this.dataInput);

                //this.CKEditorInstance?.focusManager.blur(true);
                this.hideOldEditorToolbar(true);

                let options = {
                    id: "MotherOfAllModals",
                    class: this.dataInput.ReadOnly ? 'modal-motherofAll read-only-memo' : "modal-motherofAll",
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

                                    let appRoot = [...this.$root.$children]?.find(nd => nd.$options.name == "App")
                                    ?.$children?.find(child => child.$options.name == "v-app")
                                    ?.$children.find(child => child.$options.name == "v-main");

                                    if (!(appRoot && appRoot.$children && appRoot.$children.length > 0))
                                        return false;

                                    let ficRoot = appRoot.$children.find(nd => nd.$options.name == "fiche");

                                    if (!ficRoot)
                                        return false;

                                    ficRoot.showMotherModal = false;
                                    this.hideOldEditorToolbar(false);
                                }
                            }
                        ]
                    },
                    main: {
                        class: "detailContent memo-modal modal-content-motherofAll modal-content-motherofAll-Max",
                        componentsClass: "grid-container form-group ",
                        lstComponents: [
                            { input: dataInputWithFullValue, class: "input-line fname" },
                        ]
                    },
                    footer: {
                        class: "modal-footer-motherofAll modal-footer-motherofAll-Max",
                        btn: [ 
                            !this.dataInput.ReadOnly ? {
                                title: this.getRes(28), class: "btnvalid eudo-button btn btn-success", action: ($event) => {


                                    if (!(this.$root.$children && this.$root.$children.length > 0))
                                        return false;

                                    let appRoot = [...this.$root.$children]?.find(nd => nd.$options.name == "App")
                                    ?.$children?.find(child => child.$options.name == "v-app")
                                    ?.$children.find(child => child.$options.name == "v-main");

                                    if (!(appRoot && appRoot.$children && appRoot.$children.length > 0))
                                        return false;

                                    let ficRoot = appRoot.$children.find(nd => nd.$options.name == "fiche");

                                    if (!(ficRoot && ficRoot.$children && ficRoot.$children.length > 0))
                                        return false;

                                    let MoMod = ficRoot.$children.find(nd => nd.$options.name == "MotherOfAllModals")

                                    /** Pour tous les composants trouv??s dans Momod, on met ?? jour.
                                     * C'est pas tr??s propre et il faudra revoir le syst??me, mais pour le moment
                                     * on s'en contente. */
                                    let isHappyEnd = true;
                                    MoMod.$children.map(nd => nd.dataInput.IsHtml
                                        ? (nd.CKEditorInstance?.getData() || " ")
                                        : document.getElementById(nd.GetComponentId).value)
                                        .forEach((ndValue) => {
                                            if (ndValue) {
                                                try {
                                                    verifComponentModal(undefined, ndValue, this.dataInput.Value, this, this.dataInput)

                                                    /** Si on est dans une liste, on n'a pas de ckeditor ?? mettre ?? jour. */
                                                    if (!this.propListe) {
                                                        if (this.dataInput.IsHtml && this.CKEditorInstance && this.CKEditorInstance.container)
                                                            this.CKEditorInstance.setData(ndValue);
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
                                    this.hideOldEditorToolbar(!isHappyEnd);

                                }
                            } : '',
                            {
                                title: !this.dataInput.ReadOnly ? this.getRes(29) : this.getRes(30), class: [!this.dataInput.ReadOnly ? "btncancel" : "btnvalid" , "eudo-button btn btn-default"], action: () => {

                                    if (!(this.$root.$children && this.$root.$children.length > 0))
                                        return false;

                                    let appRoot = [...this.$root.$children]?.find(nd => nd.$options.name == "App")
                                    ?.$children?.find(child => child.$options.name == "v-app")
                                    ?.$children.find(child => child.$options.name == "v-main");

                                    if (!(appRoot && appRoot.$children && appRoot.$children.length > 0))
                                        return false;

                                    let ficRoot = appRoot.$children.find(nd => nd.$options.name == "fiche");

                                    if (!ficRoot)
                                        return false;

                                    ficRoot.showMotherModal = false;
                                    this.hideOldEditorToolbar(false);
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
            
        },
        /**
         *  # 82 638 - R??cup??re la valeur compl??te du champ M??mo en base, comme le faisait XRM/E17 (cf. eMemoDialog.aspx et eGetFieldManager.ashx)
         */
        async getFullMemoValue(dataInput) {
            let url = "mgr/eGetFieldManager.ashx";
            let helper = new eAxiosHelper(url);
            try {
                let formData = new FormData();
                formData.append("action", "FIELD_VALUE");
                formData.append("tabDescId", this.getTabDescid(dataInput.DescId));
                formData.append("fileId", dataInput.FileId);
                formData.append("fieldDescId", dataInput.DescId);
                formData.append("memoId", this.GetComponentId);
                let config = {
                    responseType: 'document'
                };
                let oRes = await helper.PostAsyncWHeader(formData, config);
                if (oRes && oRes.data) {
                    let strSuccess = getXmlTextNode(oRes.data.getElementsByTagName("success")[0]);
                    if (strSuccess == "1")
                        return getXmlTextNode(oRes.data.getElementsByTagName("dbvalue")[0]);
                    else {
                        let strError = getXmlTextNode(oRes.data.getElementsByTagName("error")[0]);
                        throw strError;
                    }
                }
                else
                    throw this.getRes(72); // Une erreur est survenue

            } catch (e) {
                throw e;
            }
        },
        // Redimensionne automatiquement l'??diteur et la barre d'outils au redimensionnement de la fen??tre et/ou affichage/masquage du menu droit
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
        // Redimensionne l'??diteur et la barre d'outils aux dimensions indiqu??es (?? la taille du conteneur si non pr??cis??es)
        resize: function (nNewWidth, nNewHeight) {
            this.resizeEditor(nNewWidth, nNewHeight);
            this.resizeToolbar(nNewWidth, nNewHeight);
        },
        /// Redimensionne l'??diteur aux dimensions donn??es (?? la taille du conteneur si non pr??cis??es)
        resizeEditor: function (nNewWidth, nNewHeight) {
            if (this.dataInput.IsHtml) {
                // Redimensionnement automatique en fonction du conteneur parent si les dimensions ne sont pas pass??es en param??tres

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
                        // Avec VueJS, le resize() est parfois appel?? alors que le composant n'est pas tout ?? fait pr??t
                    }

                    // Et enfin, on affecte la nouvelle taille ?? la configuration, pour que'on puisse se baser dessus
                    this.htmlEditor.config.width = nNewWidth;
                    this.htmlEditor.config.height = nNewHeight;
                    // En compl??ment, en mode Inline, le composant (devenu un div) voit sa taille s'ajuster automatiquement en fonction du contenu.
                    // Pour emp??cher ce ph??nom??ne, il faut affecter une taille au div avec overflow pour afficher un ascenseur
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
            // #30 023 - on indique au document de masquer la barre d'outils lorsqu'on fait d??filer la page/certains ??l??ments scrollables
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
                    // #30 202 : Si l'??l??ment subissant le d??filement n'est pas CKEditor lui-m??me, on agit ;
                    // Si le d??filement a lieu sur un objet ind??termin??, ou au sein m??me de CKEditor, on ne fait rien
                    if (oScrolledObj && (
                        vueJsObject.CKEditorInstance && vueJsObject.CKEditorInstance.container && vueJsObject.CKEditorInstance.editable() && vueJsObject.CKEditorInstance.editable().$ &&
                            (oScrolledObj != vueJsObject.CKEditorInstance.container.$)
                        )
                    ) {
                        // On pourrait ici appeler hideShowToolbar() pour masquer la barre d'outils, mais il faudrait alors forcer son r??affichage lors du clic ou du focus dans le champ
                        // Faire un hideShowToolbar(true) sur le CKEDITOR.on('focus') est tout simple et fonctionne tr??s bien...
                        // Mais l'??v??nement "clic" dans le champ est fortement restreint par CKEditor 4 : http://stackoverflow.com/questions/17045329/ckeditor-how-to-add-permanent-onclick-event
                        // et n'est pas toujours d??tect??. Or, il est indispensable en plus de focus() car comme le curseur reste dans le champ lors du d??filement de la page,
                        // l'??v??nement focus n'est pas red??clench?? lorsqu'on reclique dans le champ en esp??rant faire appara??tre la barre d'outils.
                        // On pr??f??rera donc, ici, d??clencher le onblur() de l'??diteur interne du champ M??mo pour retirer le curseur du champ et, ainsi, d??clencher le masquage de la barre d'outils
                        // avec le fonctionnement interne de CKEditor.
                        // Inconv??nient (ou avantage) : cela pourra d??clencher la sauvegarde du champ et d'autres m??canismes lourds li??s ?? la sortie de champ si d??finis.
                        vueJsObject.CKEditorInstance.editable().$.blur();
                        //eMemoEditorTargetObject.hideShowToolbar(false);
                    }
                };
            })(this);
            // On d??tecte/indique ici dans quels environnements, et sur quels ??l??ments, surveiller le scroll pour enlever la barre d'outils
            // TODO: l'id??al serait de pouvoir le faire avec un querySelector qui r??cup??rerait tous les ??l??ments en overflow auto.
            // Pour le moment, on pr??ferera cibler les ??l??ments pour lesquels on aura d??termin?? que ce cas doit ??tre g??r??
            var oScrollableParentElements = new Array();
            var parentContainer = this.CKEditorInstance.container.$;
            while (parentContainer != null) {
                var parentContainerRealStyle = getComputedStyle(parentContainer);
                if (parentContainerRealStyle && parentContainerRealStyle.overflow == 'auto') {
                    oScrollableParentElements.push(parentContainer);
                }
                parentContainer = parentContainer.parentElement;
            }
            // Parcours des ??l??ments sur lesquels rattacher l'??v??nement
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
        // Passe la barre d'outils en mode invisible ou gris?? si lecture seule
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
        // Renvoie un pointeur vers l'??l??ment HTML de la barre d'outils, principalement dans le but d'agir sur son affichage en mode inline
        getToolbarContainer: function () {
            // Parcours des noeuds enfant ?? la recherche de la barre d'outils, qui a un ID variable
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
        // Redimensionne la barre d'outils ?? la taille sp??cifi??e, ou en fonction de celle du champ M??mo (utile pour le mode Inline)
        resizeToolbar: function (nNewWidth) {
            var oToolbar = this.getToolbarContainer();
            if (oToolbar) {
                if (isNaN(nNewWidth)) {
                    // Taille du champ M??mo d??finie en pixels : on l'utilise directement pour le calcul de la taille
                    if (this.htmlEditor.config.width && typeof (this.htmlEditor.config.width) != "number" && this.htmlEditor.config.width.indexOf('%') == -1) {
                        nNewWidth = getNumber(this.htmlEditor.config.width);
                    }
                    // Taille du champ M??mo d??finie en pourcentage : on tente de r??cup??rer la taille r??elle
                    // (calcul??e en pixels par le navigateur) de l'objet via eTools.getAbsolutePosition();
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
        // Renvoie ou param??tre la barre d'outils ?? afficher sur le champ M??mo
        // - En mode HTML, le seul appel ?? la m??thode param??tre l'objet config ?? destination de CKEditor
        // - En mode Texte brut, la fonction renvoie le code HTML des boutons ?? afficher sur la barre d'outils
        setToolbar: function () {
            var returnValue = false;
            // Barre d'outils HTML (syntaxe CKEditor) - personnalisable en fonction de la valeur de la propri??t?? toolbarType
            // Ajouter ici les diff??rentes barres d'outils souhait??es aux diff??rents endroits de l'application, et renseigner depuis la page appelante la propri??t?? toolbarType
            // pour afficher la barre d'outils en question
            if (this.dataInput.IsHtml) {
                // En mode "sans bordures", on masque la barre d'outils en retirant tous ses boutons, sauf si le mode inline est activ??
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

                            // D??finition des groupes de boutons de base
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

                            // T??che #2407 et #2447 - Bouton Caract??res Sp??ciaux
                            toolbarInsertButtons.push('SpecialChar');

                            // Ajout de boutons additionnels si on affiche une barre d'outils "avanc??e"
                            if (bAdvancedMode) {
                                toolbarDocumentButtons.push('Print', 'Preview');
                                toolbarEditButtons = ['Cut', 'Copy', 'Paste', 'PasteText', 'PasteFromWord', 'SelectAll', '-', 'Undo', 'Redo', '-', 'Find', 'Replace', '-'];
                                toolbarStyleButtons.unshift('Styles', 'Format'); // ajout des boutons DEVANT ceux existants
                                toolbarStyleButtons = toolbarStyleButtons.concat(toolbarAdditionalStylesButtons); // puis APRES ceux existants
                                toolbarParagraphButtons.push('Blockquote');
                            }
                            else {
                                toolbarInsertButtons = toolbarEditButtons.concat(toolbarInsertButtons); // fusion des boutons du groupe "Edition" avec ceux du groupe "Insertion"
                                toolbarEditButtons = []; // pas de groupe de boutons "Edition" s??par??
                            }

                            // Fusion de certains groupes de boutons
                            toolbarStyleButtons = toolbarStyleButtons.concat(toolbarColorStylesButtons); // ajout des boutons "Couleurs"

                            // Construction de la barre d'outils d??finitive
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
                                    { name: 'firstButtons', groups: ['firstButtons'], items: ['xrmUserMessage', this.$parent.$options.name != "MotherOfAllModals" ? 'xrmFullScreenDialog': '', 'Print'] },
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
            // Barre d'outils Texte Brut : g??r?? du c??t?? du template VueJS

            return returnValue;
        },
		/**
		* Demande #54 068 et Backlog #616 - Positionne une police par d??faut sur le canvas de l'??diteur, sauf pour les cas o?? la mise en forme doit pouvoir ??tre contr??l??e
		* Notamment la signature utilisateur et l'??dition de mails
		* Demande #78 433 - US #2 925 - T??che #4 326 - Ajout d'une classe suppl??mentaire eMEFontEudoIRISSize_X pour appliquer la taille de police d??finie dans Mon Eudonet
		* @param {string} Si n??cessaire, permet de surcharger la propri??t?? editorType de l'objet en cours (cens??e ??tre positionn??e par le parent du composant), pour adapter le retour de la fonction selon le type d'??diteur (ex : "mail", "mailing", "adminusersign"
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
		Applique, sur le conteneur du champ M??mo (div), les m??mes classes CSS que sur la propri??t?? .config.bodyClass, mise ?? jour par setFontClass() ci-dessus
		Sur les CKEditor instanci??s en inline, il semble n??cessaire de l'ajouter ??galement sur le conteneur apr??s l'initialisation de l'instance (instanceReady). cf. setFontClassOnContainer()
		La propri??t?? bodyClass ne semblant pas ??tre prise en compte lorsqu'on instancie un CKEditor inline (ce qui est logique, puisque cette bodyClass est cens??e s'appliquer sur l'iframe interne de CKEditor, qui n'est pas utilis??e en mode inline)
		Source : https://ckeditor.com/docs/ckeditor4/latest/api/CKEDITOR_config.html#cfg-bodyClass
		* */
		setFontClassOnContainer: function () {
			addClass(this.CKEditorInstance.container.$, this.CKEditorInstance.config.bodyClass);
		},	
        /**
         * Affiche ou masque la toolbar. Cela permet de masquer la toolbar lorsque l'on ouvre en grand celle-ci depuis un champs car sinon la toolbar du champs reste
         * affich??e sur le m??mo dans la modale.
         * @param {string} ajoute ou non la classe css
         */
        hideOldEditorToolbar: function (bVal) {
            if (this.CKEditorInstance == undefined && this.CKEditorInstance == null)
                return false

            if(bVal)
                document.querySelector(`#cke_${this.CKEditorInstance.name}`).classList.add('cke_opacity')
            else
                document.querySelector(`#cke_${this.CKEditorInstance.name}`).classList.remove('cke_opacity')
        },
		
        // Charge les ressources de l'application au sein du composant CKEditor (??vite de modifier ses fichiers de ressources en .JS)
        getHTMLEditorLanguageResources: function () {
            if (this.dataInput.IsHtml && typeof (CKEDITOR) != "undefined" && typeof (CKEDITOR.lang) != "undefined") {
                var CKEditorCurrentLang = eval("CKEDITOR.lang." + this.htmlEditor.language);
                if (typeof (CKEditorCurrentLang) != "undefined") {

                    // Ressources de langues pour les plugins personnalis??s
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
                        eval("CKEDITOR.lang." + this.htmlEditor.language + ".xrmLinkUnsubOption = top._res_1847;"); //Param??trages d'abonnements
                        eval("CKEDITOR.lang." + this.htmlEditor.language + ".xrmLinkUnsubscribe = top._res_1861;");
                    }
                    else {
                        eval("CKEDITOR.lang." + this.htmlEditor.language + ".xrmLinkUnsubOption = top._res_6595;"); //D??sinscription
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
                        "panelTitle: top._res_6600" + // Actions utilisateur et mod??les de formulaires
                        "};");
                    eval("CKEDITOR.lang." + this.htmlEditor.language + ".xrmFullScreen = top._res_6601;"); // Plein ??cran
                    eval("CKEDITOR.lang." + this.htmlEditor.language + ".xrmFullScreenDialog = top._res_6602;"); // Plein ??cran (nouvelle fen??tre)
                    eval("CKEDITOR.lang." + this.htmlEditor.language + ".xrmImage = top._res_712;"); // Ins??rer une image
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

                    // Cr??ation de variables "raccourcis" vers les noms de certaines commandes pour la fonction getCommandDescription
                    // A d??finir lorsqu'il n'existe pas, dans les fichiers de langue CKEditor, de variable portant directement le nom de la commande
                    // Ces variables additionnelles sont pr??fix??es par un objet "xrmToolTip" afin que leur d??finition ne masque pas une autre propri??t??
                    // d??clar??e avec le m??me nom dans le code source de CKEditor
                    eval("CKEDITOR.lang." + this.htmlEditor.language + ".xrmToolTip = { };"); // cr??ation de l'objet
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

                    // Affichage des raccourcis clavier disponibles en tant que tooltip du champ M??mo
                    var strMemoTooltip = 'CKEDITOR.lang.' + this.htmlEditor.language + '.xrmHelpContents = "';
                    // Les raccourcis clavier sont cens??s ??tre d??finis ?? ce stade, puisque la fonction getLanguageResources() doit ??tre appel??e en diff??r?? apr??s le chargement initial des
                    // fichiers de langue de CKEditor, ce qui intervient apr??s la phase d'initialisation de l'objet eMemoEditor o?? est d??finie la propri??t?? keystrokes
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
                            // la fonction qui utilisera ce tableau de cha??nes obtiendra le nom de la commande ?? partir de la classe CSS du bouton
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
        // Charge une description des commandes/raccourcis g??r??s par l'??diteur, pour les infobulles d'aide
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
        // Charge les plugins utilis??s par l'??diteur
        loadHTMLEditorPlugins: function (strCustomPluginList) {
            if (this.dataInput.IsHtml) {
                if (typeof (strCustomPluginList) == 'undefined' || !strCustomPluginList || strCustomPluginList == '') {
                    // En mode Plein ??cran, inutile d'afficher le bouton Zoom
                    // TOOD: variables isFullScreen, editorType, formularEnabled, automationEnabled, speechEnabled ?? adapter apr??s cablage
                    var xrmFullScreen = this.isFullScreen ? '' : ',xrmFullScreen';

                    // Backlog #653 - Le bouton CSS n'est plus affich?? sous CKEditor si on utilise grapesjs, qui g??re les CSS autrement
                    // On doit tester la compatibilit?? navigateur EN plus de tester enableTemplateEditor, car il s'agit ici de l'activer sur le CKEditor non li?? ??
                    // grapesjs, sur lequel cette variable est donc ?? false
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
        // Affecte un skin sp??cifique au composant en fonction de ses modes d'affichage
        setSkin: function (strSkin) {
            if (this.dataInput.IsHtml) {
                if (!strSkin || strSkin == '') {
                    // TODO: skin sp??cifique compactMode ?
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
            this.openMemo(event);
        },
        insertImage: function () {
            this.insertImg(true);
        },
        verifMemo(event, that, from) {
            // #96 278 - Si la demande de mise ?? jour se fait parce que le curseur est sorti du champ pour afficher la popup d'ajout d'image ou de plein ??cran, on court-circuite la demande
            // Le rafra??chissement qui r??sulte de cette MAJ pouvant alors faire compl??tement ??chouer les actions men??es par la popup ouverte.
            // La mise ?? jour sera red??clench??e ?? la fin du traitement de la popup en question, ou lors de n'importe quelle autre sortie du champ.
            // Il n'y a donc pas lieu d'en faire une au moment d'ouvrir une fen??tre destin??e ?? effectuer des modifications sur le contenu
            if (from == "blur" && (this.cptFromModal || this.isImageModalOpened())) {
                return false;
            }

            verifComponent(undefined, event, this.dataInput.Value, that, this.dataInput, null, from);
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
            this.modalOptions.main.class = "detailContent memo-modal modal-content-motherofAll modal-content-motherofAll-" + (!this.bmaximize ? 'Max' : 'Min');
            this.modalOptions.footer.class = "modal-footer-motherofAll modal-footer-motherofAll-" + (!this.bmaximize ? 'Max' : 'Min');
            this.bmaximize = !this.bmaximize;
        },
        /**
         * Indique si la popup d'ajout d'image dans le champ M??mo (E17) est actuellement instanci??e ET ouverte (ou non).
         * Instanci??e : le pointeur top.modalImage est un objet eModalDialog (= la popup a ??t?? cr????e au moins une fois)
         * Visible/ouverte : l'UID de cette eModalDialog doit ??tre r??f??renc??e dans le tableau global E17 top.ModalDialogs (quand on fait .hide() sur eModalDialog, cette r??f??rence est remise ?? null, mais l'objet top.modalImage ne n'est pas forc??ment, car il est sous la responsabilit?? du parent d??clarant).
         * */
        isImageModalOpened() {
            return top.modalImage && top.ModalDialogs && top.ModalDialogs[top.modalImage.UID];
        }
    },
    props: ["dataInput", "propHead", "propListe", "propSignet", "propIndexRow", "propDetail", "cptFromModal", "cssClassText", "propAside", "memoOpenedFullMode","cptFromAside"],
    // sur les champs de type M??mo, qu'on soit en lecture seule ou non, on ajoute le champ de type textarea ainsi que le bouton, pour pouvoir afficher le contenu en plein ??cran. L'affichage sera simplement gris??
    template: `

<div ref="dvEMemo" class="globalDivComponent flex-grow-1" style="width: 100%;padding:0">
    <!-- FICHE -->
    <template v-if="false">
        <eMemoFile />
    </template>
    <template v-else>
    <!-- <div v-if="!propListe" v-bind:class="[dataInput.ReadOnly ? 'readOnlyTxt' : '', 'rubriqueEmemo ellips input-group hover-input']"
        :title="!dataInput.ToolTipText ? dataInput.DisplayValue : dataInput.ToolTipText">-->
    <div v-if="!propListe" v-bind:class="[dataInput.ReadOnly ? 'readOnlyTxt' : '', 'rubriqueEmemo ellips input-group hover-input']"
        :title="!dataInput.ToolTipText ? dataInput.DisplayValue : dataInput.ToolTipText">

        <textarea @focus="TextAreaFocus()" @blur="TextAreaBlur($event)" :IsDetail="propDetail" v-if="dataInput.IsHtml" :id="GetComponentId" :readonly="dataInput.ReadOnly" class="form-control input-line fname">{{ValueToDisplay}}</textarea>
        
        <textarea @focus="TextAreaFocus()" @blur="TextAreaBlur($event)" v-if="!dataInput.IsHtml" :resize="isResizeable" :rows="$attrs.rows ? $attrs.rows : dataInput.Rowspan" :disabled="dataInput.ReadOnly" :id="GetComponentId" :readonly="dataInput.ReadOnly" class="textareaRubrique form-control input-line fname">{{ValueToDisplay}}</textarea>
        <div ref="dvToolbar" v-if="!dataInput.IsHtml && !dataInput.ReadOnly" class="ToolsBarCK">
            <div class="cke_chrome cke_float"  role="application">
                <div class="cke_inner">
                    <div class="cke_top" role="presentation">
                        <span class="cke_toolbox" role="group">
                            <span  class="cke_toolbar" role="toolbar">
                                <span class="cke_toolgroup" role="presentation">
                                    <a @click="insertMessage()" class="cke_button cke_button__xrmusermessage cke_button_off" href="#!" :title="getRes(57)" role="button">
                                        <span class="cke_button_icon fas fa-user">&nbsp;</span>
                                    </a>
                                    <a  @click="openMemo($event)" class="cke_button cke_button__xrmfullscreendialog cke_button_off" :title="getRes(6602)" role="button">
                                        <span class="cke_button_icon cke_button__xrmfullscreendialog_icon fas fa-expand">&nbsp;</span> 
                                    </a>
                                </span>
                            </span>
                        </span>
                    </div>
                </div>
            </div>
        </div>
        <!-- Message d'erreur apr??s la saisie dans le champs -->
        <span v-show="!bShowOnFocus" v-if="!cptFromModal && !cptFromAside" class="polePositionFullScreen fas fa-expand" @click="openMemo($event)"></span>
    </div>
    <eAlertBox v-if="this.bEmptyDisplayPopup && !(dataInput.ReadOnly)" >
        <p>{{getRes(2471)}}</p>
    </eAlertBox>
    </template>
    <!-- LISTE -->
    <template v-if="false">
        <eMemoList />
    </template>

    <template v-else>
        <div style="padding:0" v-if="propListe" v-bind:class="['listRubriqueMemo ellips input-group hover-input', IsDisplayReadOnly?'read-only':'']" >

            <div style="height:30px;padding:0;min-height: 30px" :title="noHtml">
                <span @click="openMemo($event)" class="textareaRubrique form-control input-line fname">{{noHtml}}</span>
            </div>

            <!-- Icon -->
            <span v-on:click="!IsDisplayReadOnly ? openMemo($event) : '' " class="input-group-addon"><a href="#!" class="hover-pen"><i :class="[IsDisplayReadOnly?'mdi mdi-lock':'fas fa-pencil-alt']"></i></a></span>
        </div> 

    </template>
</div>
`
};