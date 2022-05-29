import { dynamicFormatChamps } from '../../../index.js?ver=803000';
import {
    specialCSS, forbiddenFormatHead,
    LoadWzCatalog,
    ConstructCatalog,
} from "../../methods/eFileMethods.js?ver=803000";
import { tabFormatForbid, tabFormatBtnLbl, tabFormatBtnSep,wizardFldsMaxNb  } from "../../methods/eFileConst.js?ver=803000";
import EventBus from '../../bus/event-bus.js?ver=803000';
import { updateMethod, showTooltip, infoTooltip } from '../../methods/eComponentsMethods.js?ver=803000';
import { eFileMixin } from '../../mixins/eFileMixin.js?ver=803000';
import { stepBarStatus } from '../../methods/eComponentConst.js?ver=803000';
import eDropDownButton from '../subComponents/eDropDownButton.js?ver=803000';

export default {
    name: "stepsBar",
    data() {
        return {
            interval: '',
            reloadStep: true,
            stepWidth: 0,
            activeBar: null,
            collapsTab: '',
            initRenderers: Object,
            interation: 5,
            newHeight: 0,
            lib: "",
            valideLast: false,
            idxValue: 2,
            stepClear: [],
            tabFormatForbid,
            tabFormatBtnLbl,
            tabFormatBtnSep,
            previousStepAvailable: false,
            nextStepAvailable: false,
            emptyValue: false,
            
            minimumDropDownStepCount: 2,
            stepFocus: null,
            stepDetail: {},
            wizardFldsMaxNb 
        };
    },
    mixins: [eFileMixin],
    components: { eDropDownButton },
    props: {
        nCounterReload: Number,
        propDetail: {
            type: Array
        },
        propWizardBarEmpty: {
            type: Boolean
        },
        JsonWizardBar: {
            type: Object,
                default: {}
        },
        propStep: {
            type: Object,
            default: {}
        }
    },
    async created() {
        // TK #6 113 - Lors d'un changmeent d'onglet, par exemple un clic sur une fiche en signet (entre autres), le catalogue source de la barre d'étapes change.
        // Il faut alors forcer le rechargement des données de catalogue dans ce cas, en faisant un appel au serveur (paramètre bForceReload à true pour LoadWzCatalog).
        // Pour faire cette vérification, on vérifie que le DescID du catalogue à afficher dans la barre d'étapes corresponde à celui dont les données figurent dans le store (getTkStructCat)
        // Si ça n'est pas le cas, on demande un rechargement forcé, en fournissant le DescID du catalogue à charger (dans le cas contraire, le chargement se ferait avec le DescID du précédent catalogue chargé).
        // Demande #94 037 - Il ne faut donc pas faire ce traitement si on a pas de barre d'étapes, ça renvoie un message d'erreur...
        if (this.JsonWizardBar?.DescId) {
            let bMustReloadCatalogData = this.JsonWizardBar?.DescId != this.getTkStructCat?.params?.descid;
            let oNewParams = { descid: this.JsonWizardBar?.DescId };
            this.$emit("getReloadCatalog", bMustReloadCatalogData, oNewParams)
        }

        // get stepsBar css from propDetail
        this.stepDetail = this.propDetail?.find(tab => tab?.DescId == this.JsonWizardBar?.DescId);

        EventBus.$off("checkStepBarField");
        EventBus.$on('checkStepBarField', (options) => {
            if (options && options.inputData && options.inputData.DescId == this.propStep.DescId) {
                let newStep = this.propStep.FieldsById.find(step => step.DbValue == options.newValue);
                if (newStep) {
                    let isStepAlreadySelected =
                        newStep.active
                        || document.getElementById('id_' + newStep.DbValue)?.classList.contains("inProgress")
                        || document.getElementById('id_' + newStep.DbValue)?.classList.contains("complete") && newStep == this.propStep.FieldsById[this.propStep.FieldsById.length - 1];
                    if (!isStepAlreadySelected) {
                        this.idxValue = stepBarStatus.GoToStep;
                        this.validateStep(newStep, this.propStep.FieldsById, false, options.alreadyUpdated);
                    }
                }
                // Cas de la sélection d'aucune valeur : passage du premier paramètre à null
                else {
                    let firstStepElt = document.getElementById('id_' + this.propStep.FieldsById[0].DbValue);
                    let isStepAlreadySelected = !firstStepElt?.classList.contains("inProgress") && !firstStepElt?.classList.contains("complete");
                    if (!isStepAlreadySelected) {
                        this.idxValue = stepBarStatus.UnknownStep;
                        this.validateStep(null, this.propStep.FieldsById, true, options.alreadyUpdated);
                    }
                }
            }
        });


    },
    mounted() {
        //this.getHeightGS();
        var that = this;
        this.interval = setInterval(function () { that.getTabsToElHaZard() }, 500);
    },
    updated() {
        //this.getTabsToElHaZard();
    },
    computed: {
        /** Permet de rafraichir le compostant. */
        CounterReload: {
            get: function () {
                this.ConstructCatalog(this.$parent)
                Vue.nextTick(() => this.getTabsToElHaZard());
                return this.nCounterReload
            },
            set: function (value) {
                this.$emit("update:nCounterReload", value)
            }
        },
        /**
         * Renvoie le titre de la zone start de l'assistant en fonction de la langue de l'utilisateur. */
        getTitleStartTxtAssistant: function () {
            return this.getStartTxtAssistant(this.JsonWizardBar.WelcomeBoard?.Title, 2573);
        },
        /**
         * Renvoie le corps de la zone start de l'assistant en fonction de la langue de l'utilisateur. */
        getBodyStartTxtAssistant: function () {
            return this.getStartTxtAssistant(this.JsonWizardBar.WelcomeBoard?.Body, 2574);
        },
        /**
         * Indique si la barre d'étapes est à afficher en lecture seule : soit en raison de droits sur le catalogue (propStep.ReadOnly), soit parce qu'on a mis "Hidden" dans le JSON
         * MAJ Tâche #4 651 : le paramètre Hidden est finalement ignoré. Son but était de masquer le champ Source de la zone Détails en outrepassant tout autre paramétrage.
         * Face aux risques de régressions, on n'en tient plus compte. */
        isReadOnly: function () {
            return this.propStep.ReadOnly/* || this.JsonWizardBar.Hidden*/;
        },
        /**
         * Indique si le clic sur les pastilles doit valider l'étape, ou non. Ce mode est uniquement activé si les boutons Suivant et Précédent sont explicitement masqués par
         * l'administrateur en déclarant HidePreviousButton et HideNextButton à true dans le JSON. Il ne s'applique pas si les boutons Suivant et Précédent sont masquées pour une
         * autre raison (= pas d'étapes disponibles ou catalogue en lecture seule)
        */
        validateOnClick: function () {
            return !this.isReadOnly && this.JsonWizardBar.HidePreviousButton && this.JsonWizardBar.HideNextButton;
        },
        /**
         * Indique si on doit masquer le bouton Précédent ou non ("HidePreviousButton" dans le JSON, ou aucune étape précédente à afficher) */
        displayPreviousButton: function () {
            return !this.isReadOnly && !this.JsonWizardBar.HidePreviousButton && this.previousStepAvailable;
        },
        /**
        * Indique si on doit masquer le bouton Suivant ou non ("HideNextButton" dans le JSON, ou aucune étape suivante à afficher) */
        displayNextButton: function () {
            return !this.isReadOnly && !this.JsonWizardBar.HideNextButton && this.nextStepAvailable;
        },
        /**
         * Renvoie les classes CSS à appliquer en complément sur la stepBar en fonction du contexte (lecture seule, taille en fonction des boutons affichés... */
        getAdditionalStepBarClasses: function () {
            let sClasses = "";
            if (this.propStep.IsSequenceMode)
                sClasses += "suite";
            if (this.displayPreviousButton)
                sClasses += " previousButtonProgress";
            if (this.displayNextButton)
                sClasses += " nextButtonProgress";
            return sClasses;
        },
        /** Renvoie les classes CSS à appliquer aux boutons Précédent et Suivant */
        /* cf. US #2 926 - Tâche #4 650 - Application du style style par défaut des boutons Vuetify */
        getButtonClass: function () {
            return "d-flex justify-start dropdown-toggle pa-2";
        },
        getLabelClass() {
            return {
                'italicLabel': this.stepDetail?.Italic,
                'boldLabel': this.stepDetail?.Bold,
                'underLineLabel': this.stepDetail?.Underline,
                'labelHidden': this.stepDetail?.labelHidden
            }
        }
    },
    methods: {
        showTooltip,
        infoTooltip,
        updateMethod,
        specialCSS,
        forbiddenFormatHead,
        LoadWzCatalog,
        ConstructCatalog,
        
        /** Remonte l'apel du spinner */
        eWaiterDetail: function (oValue) {
            this.$emit("setWaitIris", oValue.bOn, oValue.nOpacity);
        },
        /**
        * Renvoie le contenu d'un texte de la zone start de l'assistant en fonction de la langue de l'utilisateur.
        * @param {any} source Element source du texte (tableau de string["lang_XX"] ou string directe)
        * @param {int} defaultRes Numéro de ressource à utiliser si la source n'est pas conforme aux attentes
        */
        getStartTxtAssistant: function (source, defaultRes) {
            if (this.JsonWizardBar.WelcomeBoard?.Display == "hide")
                return '';

            let startTxt = '';
            if (this.JsonWizardBar.WelcomeBoard?.Display == "custom" && source)
                startTxt = typeof source === 'string' ?
                    source.trim() :
                    source["lang_" + this.$store.getters.getUserLangID?.toString()?.padStart(2, '0')]?.trim();
            return startTxt ? startTxt : this.getRes(defaultRes); // choix par défaut : si Display == autre chose que "hide" ou "custom" (donc "show", true...)
        },
        /**
         * Permet de retourner le JSON dans les langues suivant
         * la langue sélectionnée dans l'interface.
         * @param {any} input
         */
        getNoteTxt: function (input) {
            return input[this.initLangComponents()];
        },
        /**
         * Renvoie la classe CSS à appliquer sur le conteneur de la zone Assistant, en fonction de la configuration des rubriques indiquée dans le JSON (avec ou sans zone d'aide, nombre...)
         * @param {any} step Etape concernée
         */
        getClassFormat(step) {
            // US #3977 - Possibilité de fixer le nombre de colonnes d'affichage de la zone d'aide
            let aOverridenGridSize = [3, 2]; // si cols = 1, on simule 3 champs affichés à côté (donc input-3). Si cols = 2, on simule 2 champs affichés à côté (donc input-2)
            let nCols = Number(step.DisplayedDescId.find(a => a.cols)?.cols);
            let bHasNote = step.DisplayedDescId.find(a => a.note);
            let nFieldCount = step.DisplayedDescId.filter(a => Number(a.DescId) > 0)?.length;

            // Elimination des cas ignorés :
            // - cols n'est pas un chiffre
            // - cols différent de 1, 2, ou 3
            // - cols == 3 mais au moins une rubrique affichée (> 1 car la propriété 
            // - cols == 2 mais au moins 4 rubriques affichées
            if (isNaN(nCols) || nCols < 1 || nCols > 3 || (nCols == 3 && nFieldCount > 0) || (nCols == 2 && nFieldCount > 3))
                nCols = null;

            // Aucune rubrique affichée (la propriété DisplayedDescId contient uniquement un noeud { note: true, cols: x... })
            if (nFieldCount == 0) {
                // Aide présente
                if (bHasNote) {
                    return 'OnlyHelp ' + (nCols != null ? 'OnlyHelp-' + nCols : ''); /* affichage de l'aide seule sur spécifiquement (nCols) colonnes de largeur */
                    // Aide NON présente
                } else {
                    return 'input-1';
                }
            } else {
                // Aide présente :
                // - on applique la largeur surchargée définie plus haut, si cols est d'une valeur acceptée pour le cas
                // - si la valeur de cols n'est pas acceptée/mise à null plus haut, on applique une classe par défaut input-X avec X = nombre de rubriques affichées
                if (bHasNote) {
                    return 'Help ' + 'input-' + (nCols != null && nCols <= aOverridenGridSize.length ? aOverridenGridSize[nCols - 1] : nFieldCount);
                    // Aide NON présente
                } else {
                    return 'input-' + nFieldCount;
                }
            }
        },


        /**
         * Action exécutée au clic sur une pastille/étape, en fonction du mode activé (validateOnClick à true ou false)
         * Si les conditions le permettent (= catalogue en écriture, étape proposée dans le catalogue/le JSON...)
         * @param {any} step Etape cliquée
         */
        onStepClick(step) {

            if (this.validateOnClick)
                this.validateStep(step, this.propStep.FieldsById, true);
            else
                this.showStepFields(step);
        },

        /**
        * L'action qui va s'éxecuter lorsqu'une action provient du bouton du composant dropdown.
        * @param {any} action
        */
        validateStepDropDown(action) {
            this.validateStep(action, this.propStep.FieldsById, true);
        },
        /**
         * Permet de savoir si une étape doit être affichée ou non.
         * @param {any} step l'étape à afficher ou non.
         */
        getDisplayForStep(step) {
            return step.Hidden && step.active || !step.Hidden;
        },

        /**
         * Limite maximum de composants que l'on peut afficher
         * @param {any} list la liste dont on doit déterminer si elle a 6 ou 9 éléments.
         * */
        GetLimitComponentsDisplay(list) {
            return list.some(st => st.note) ? 6 : 9;

        },

        callBackEmitLoadAll(options) {
            let that = this;
            if (options)
                if (options.reloadAll) {
                    that.reloadStep = false;
                    Vue.nextTick(() => that.reloadStep = true);
                }
        },

        dynamicFormatChamps,

        /**
         * Affiche les rubriques d'une étape, sans la valider
         * @param {any} step Etape pour laquelle afficher les rubriques
         */
        showStepFields: function (step) {
            this.activeBar = true;
            var li = document.getElementsByClassName("listStep");
            Vue.nextTick(() => {
                var elem = this.$refs.stepContent;
                for (var i = 0; i < li.length; i++) {
                    if (this.$refs.stepContent)
                        elem[i]?.classList?.add("displayNone");

                    if (li[i]?.classList?.contains("active")) {
                        li[i]?.classList?.remove("active")
                    }
                }
                elem = document.getElementsByClassName('id_' + step.DbValue);
                if (elem)
                    elem[0]?.classList?.remove("displayNone");

                step.focus = true;
                this.$refs["step_" + step.DbValue][0]?.classList.add("active")
                this.calHeightNote();
            })
        },

        /**
        * Valide l'étape targetStep, et si stayOnTargetStep est à false, passe à l'étape suivante (sinon, reste sur l'étape validée)
        * @param {any} targetStep Etape à valider
        * @param {any} allSteps Liste des étapes disponibles
        * @param {any} stayOnTargetStep Si true, on ne passe pas à l'étape suivant séquentiellement targetStep. Si false, on passe à l'étape suivante dans la séquence
        * @param {any} alreadyUpdated Indique si on a déjà fait appel au contrôleur de MAJ en base pour lui envoyer la nouvelle valeur
        */
        validateStep: function (targetStep, allSteps, stayOnTargetStep, alreadyUpdated) {
            this.eWaiterDetail({ bOn: true, nOpacity: 0.3 });
            this.activeBar = true;
            let that = this;

            // #81 758 - Cas de la sélection d'aucune valeur dans le catalogue source : on affiche le bouton jaune "Démarrer à l'étape <Première étape>"
            this.emptyValue = false;
            if (!targetStep) {
                targetStep = allSteps[0];
                this.emptyValue = true;
                this.activeBar = false
            }

            // Si on a toujours pas d'étape cible utilisable, on ne fait rien
            if (!targetStep)
                return;

            // #4 578 - Si on clique sur une étape du dropdown, on a pas l'objet targetStep complet, seulement son dataid et son label. On récupère donc l'objet complet depuis le tableau
            if (typeof (targetStep.DbValue) == "undefined" && typeof (targetStep.dataid) != "undefined")
                targetStep = allSteps.find(x => x.DbValue == targetStep.dataid);

            let tabClear = [];
            allSteps.forEach((a, idx) => {
                if (a.Hidden != true) {
                    tabClear.push(a)
                }
            });

            var li = document.getElementsByClassName("listStep")
            for (var i = 0; i < li.length; i++) {
                li[i].classList.remove('active');
                li[i].classList.remove('complete');
                li[i].classList.remove('validate');
                li[i].classList.remove('inProgress');
            }
            for (var x = 0; x < this.propStep.FieldsById.length; x++) {
                this.propStep.FieldsById[x].active = false;
                this.propStep.FieldsById[x].validate = false;
                this.propStep.FieldsById[x].focus = false;
            }

            targetStep.active = true
            targetStep.validate = true;
            targetStep.focus = true;


            let stepOn;


            if (stayOnTargetStep) {
                stepOn = targetStep;
            } else {
                tabClear.forEach((a, idx) => {
                    if (a.active == true) {
                        if (idx == tabClear.length - 1) {
                            stepOn = tabClear[idx];
                        } else {
                            if (that.idxValue == stepBarStatus.BackToStep || that.idxValue == stepBarStatus.GoToStep) {
                                stepOn = tabClear[idx];
                            } else {
                                stepOn = tabClear[idx + 1];
                            }
                        }
                    }
                });
            }

            targetStep.active = false
            targetStep.validate = false;
            targetStep.focus = false;

            stepOn.active = true
            stepOn.validate = true;
            stepOn.focus = true;

            // Si on a atteint et validé la toute dernière étape, il n'y a plus de bouton Valider
            let aNextStepsInJSON = this.getSteps(tabClear.find(x => x.DbValue == stepOn.DbValue)?.DataIdNext);
            if (
                (aNextStepsInJSON == null && tabClear.findIndex(x => x.DbValue == stepOn.DbValue) >= tabClear.length - 1) ||
                aNextStepsInJSON?.length < 1
            ) {
                this.nextStepAvailable = false;
            } else {
                this.nextStepAvailable = true;
            }
            // On vérifie également si on peut retourner à une étape précédente
            let aPreviousStepsInJSON = this.getSteps(tabClear.find(x => x.DbValue == stepOn.DbValue)?.DataIdPrevious);
            if (
                (aPreviousStepsInJSON == null && tabClear.findIndex(x => x.DbValue == stepOn.DbValue) < 1) ||
                aPreviousStepsInJSON?.length < 1
            ) {
                this.previousStepAvailable = false;
            } else {
                this.previousStepAvailable = true;
            }

            // Sauf si on a demandé à revenir au point de départ (targetStep à null à l'entrée de la fonction), on flag l'étape en cours comme étant... en cours
            if (!this.emptyValue) {
                var elem = document.getElementById('id_' + stepOn.DbValue)
                elem.classList.add('inProgress');
            }

            // On masque tout le contenu de l'étape (= détails sous la stepBar) sauf celle en cours
            Vue.nextTick(function () {
                var elem = document.getElementsByClassName('stepContent')
                for (var i = 0; i < elem.length; i++) {
                    elem[i].classList.add("displayNone")
                }

                elem = document.getElementsByClassName('id_' + stepOn.DbValue);
                if (elem && elem.length > 0)
                    elem[0].classList.remove("displayNone")

                stepOn.focus = true
                that.calHeightNote();
            })

            //ELAIZ - Demande 89 558 - Déplacement du changement d'étape en cours en dehors de la condition ci-dessous car elle ne fonctionnait pas ou mal dans le cas ou l'on changeait d'étape via les boutons suivant et précédent.
            // TK #6148 - Sauf en cas de sélection d'aucune valeur dans le catalogue source : dans ce cas, stepFocus doit être remise à null pour que le calcul des étapes d'initialisation (getStepsForInitialization)
            // puisse se faire correctement
            if (this.emptyValue == true && this.activeBar == false)
                this.stepFocus = null;
            else
                this.stepFocus = stepOn;

            // Déplacement également de l'ajout de la classe css complete sur les étapes précédent celle en cours car il ne s'effectuait que si stayOntargetStep était false.

            for (var i = 0; i < tabClear.length; i++) {
                var li = document.getElementById("id_" + tabClear[i].DbValue)
                if (!tabClear[i].Hidden) {
                    if (tabClear[i].active && tabClear[i].validate && tabClear[i].focus) {
                        break;
                    } else {
                        li.classList.add("complete")
                    }
                }
            }

            if (!stayOnTargetStep) {

                let idxSelect;
                let idxFocus;
                tabClear.forEach((a, idx) => {
                    if (this.stepFocus.DbValue == a.DbValue) {
                        idxFocus = idx;
                    }
                    if (a.active == true) {
                        idxSelect = idx;
                    }
                });

                let IsHiddenOn;

                this.propStep.FieldsById.forEach(a => {
                    if (a.Hidden) {
                        IsHiddenOn = true;
                    }
                });

                that.idxValue = stepBarStatus.UnknownStep;
                if (idxFocus != 0) {
                    if (!IsHiddenOn) {
                        idxFocus = idxFocus;
                    } else {
                        idxFocus = idxFocus + 1;
                    }
                }


                if (!this.isReadOnly) {
                    that.lib = stepOn.DisplayLabel;
                    that.updateButtonLabel("btnNext", stepBarStatus.ValidateStep, idxFocus, that.lib);
                }
            } else {
                if (!this.isReadOnly)
                    Vue.nextTick(() => {
                        this.lib = stepOn.DisplayLabel;
                        this.updateButtonLabel("btnNext", this.emptyValue ? stepBarStatus.UnknownStep : stepBarStatus.ValidateStep, 0, this.lib);
                    })
            }

            let MainComponent = this.$root.$children.find(app => app.$options.name == "App").$children.find(app => app.$options.name == "v-app").$children.find(app => app.$options.name == "v-main").$children.find(app => app.$options.name == "fiche").$children.find(tabsBar => tabsBar.$options.name == "tabsBar").$children.find(fileDetail => fileDetail.$options.name == "fileDetail")

            let context = MainComponent.$refs.fields.find(a => a.dataInput.DescId == this.propStep.DescId);
            // #81 758 - Si le rafraîchissement de l'affichage a été déclenché depuis updateMethod() elle-même à la modification du champ concerné par la stepBar, on ne redéclenche pas une nouvelle mise à jour
            if (!alreadyUpdated) {
                try {
                    this.updateMethod(context, stepOn.DbValue, undefined, { stepBarUpdateTriggered: true }, context.dataInput);
                } catch (e) {
                    console.log(e)
                    /*
                    EventBus.$emit('globalModal', {
                        typeModal: "alert", color: "danger", type: "zoom",
                        close: true, maximize: false, id: 'alert-modal', title: this.getRes(6576), msgData: e, width: 600,
                        btns: [{ lib: this.getRes(30), color: 'default', type: 'left' }], datas: this.getRes(7050)
                    });
                    */
                    return;
                }
            }
            this.eWaiterDetail({ bOn: false, nOpacity: 0 });

        },
        /**
        * Ouvre un modal avec la note de la step
        * @param {any} e le contenue du modal en html
        */
        getModalNote: function (e) {
            let options = {
                typeModal: "info",
                type: "zoom",
                close: true,
                isHtml: true,
                content: e,
                maximize: true,
                id: "note-step-modal",
                title: this.getRes(6187),
                titleIcon: 'fa fa-info-circle',
                btns: [{ lib: this.getRes(30), color: "default", type: "left" }],
            };
            EventBus.$emit('globalModal', options);

        },

        getHeightGS: function () {
            var stepsContainer = document.getElementById("steps--container");
            var options = {
                dataGsHeightGrid: 0,
            };
            if (stepsContainer.offsetHeight == 150) {
                options.dataGsHeightGrid = 8
            } else if (stepsContainer.offsetHeight == 210) {
                options.dataGsHeightGrid = 10
            } else if (stepsContainer.offsetHeight == 90) {
                options.dataGsHeightGrid = 6
            } else if (stepsContainer.offsetHeight == 0) {
                options.dataGsHeightGrid = 3
            }

            EventBus.$emit('newHeightContent', options);
        },

        calHeightNote: function () {
            var boxCtep = document.getElementById("box-step");
            var that = this
            setTimeout(function () {
                if (boxCtep && boxCtep.parentElement)
                    that.newHeight = boxCtep.parentElement.offsetHeight
            }, 100);
        },
        setHeight: function () {
            let note = document.getElementsByClassName('note-steps');
            if (note.length > 0) {
                for (var i = 0; i < note.length; i++) {
                    note[i].style.maxHeight = this.newHeight - 200 + 'px';
                }
            }
        },
        GetSliceArray: function (list, howMany) {
            if (!list)
                return;

            let popped = false;
            let maxComponents = this.GetLimitComponentsDisplay(list);
            var result = []
            let notes = list.find(lst => lst.note);

            let amanuensis = list
                .filter(lst => lst.Format
                    && lst.IsVisible
                    && lst.Format != 0
                    && !this.forbiddenFormatHead(lst.Format)
                    && !lst.note)
                .slice(0, maxComponents)


            if (notes) {
                result.html = true
                amanuensis.push(notes);
            }

            let noteArea = amanuensis.find(x => x.note);
            let noteAreaArray = [];
            noteAreaArray.push(noteArea);

            if (noteArea &&
                amanuensis &&
                amanuensis != undefined &&
                amanuensis != null &&
                amanuensis.length % 3 === 0 &&
                amanuensis[amanuensis.length - 1] == noteArea) {
                amanuensis.pop()
                popped = true;
            }

            result = Array
                .from(amanuensis, (v, i) => amanuensis.slice(i * howMany, (i + 1) * howMany))
                .filter(res => res.length > 0);

            if (popped) {
                result.push(noteAreaArray);
            }

            if (result.length == 0) {
                var msg = []
                msg.push({ msg: true })
                result.push(msg)
            }

            if (result.length == 1) {
                list.DataLength = 'col-md-12'
            } else if (result.length == 2) {
                list.DataLength = 'col-md-6'
            } else {
                list.DataLength = 'col-md-4'
            }

            return result
        },
        /**
         * Renvoie les étapes dont les DataIDs sont passés en paramètre, sous forme de tableau d'objets [{dataid, label}]
         * @param {any} stepDataIds DataIDs des étapes à renvoyer
         */
        getSteps(stepDataIds) {
            if (!stepDataIds)
                return null;
            // Si le tableau source n'est pas un tableau source, on tente de le corriger (si ça n'est qu'un DataId en int, on le transforme en tableau)
            // Sinon, on abandonne, filter() ne fonctionnera pas plus bas
            if (!Array.isArray(stepDataIds)) {
                if (Number(stepDataIds) > 0)
                    stepDataIds = [stepDataIds];
                else
                    return null;
            }

            let availableSteps = this.propStep.FieldsById;
            // On ne retourne l'étape que si elle existe réellement dans le catalogue. cf. descriptif de l'US #2 926
            let filteredStepDataIds = stepDataIds.filter(dataId => availableSteps.find(availableStep => availableStep.DbValue == dataId) != null);
            let stepsWithLabel = filteredStepDataIds.map(dataId => {
                return {
                    dataid: dataId,
                    label: availableSteps.find(step => step.DbValue == dataId)?.DisplayLabel
                }
            });
            return stepsWithLabel;
        },
        /**
         * Retourne la liste des IDs des étapes considérées comme utilisables pour initialiser la valeur du catalogue lorsque celui-ci n'a aucune valeur
         * Autrement dit, les étapes proposées sur le bouton Suivant affiché lorsqu'aucune valeur n'est définie dans le catalogue
         * Il s'agit des étapes n'ayant aucune étape précédente définie dans le JSON, mais ayant au moins une étape suivante définie
         * @param {any} oDataSource Tableau source contenant les étapes à analyser
         */
        getStepsForInitialization(oDataSource) {
            return oDataSource?.filter((x) => {
                let noDataIdPrevious = !x.DataIdPrevious || !x.DataIdPrevious?.length || !x.DataIdPrevious?.find(y => oDataSource.find(x => y == x.DbValue));
                return noDataIdPrevious && x.DataIdNext?.length >= 1;
            }).flatMap(y => y.DbValue);
        },
        /**
        * Pour le bouton Précédent uniquement, retourne la liste des étapes sélectionnables selon le contexte :
        * - si une étape est actuellement sélectionnée, renvoie la liste des étapes précéedentes déclarées dans le JSON pour ladite étape
        * - si aucune étape n'est actuellement sélectionnée, ne renvoie rien
        * @param {any} stepFocus Etape actuellement sélectionnée
        */
        getStepsForPreviousButtonMenu(stepFocus) {
            return stepFocus?.DataIdPrevious;
        },
        /**
        * Pour le bouton Suivant uniquement, retourne la liste des étapes sélectionnables selon le contexte :
        * - si une étape est actuellement sélectionnée, renvoie la liste des étapes suivantes déclarées dans le JSON pour ladite étape
        * - si aucune étape n'est actuellement sélectionnée, renvoie la liste des étapes éligibles pour une initialisation du catalogue (voir getStepsForInitialization)
        * cf. US #3 862
        * @param {any} stepFocus Etape actuellement sélectionnée
        */
        getStepsForNextButtonMenu(stepFocus) {
            // Cas où une étape est actuellement sélectionnée ; on regarde la liste des étapes suivantes dans le JSON 
            if (stepFocus)
                return stepFocus.DataIdNext;
            // Cas où aucune étape n'est actuellement sélectionnée : on vérifie s'il existe des étapes éligibles pour initialiser la valeur du catalogue ("premières étapes possibles")
            else
                return this.getStepsForInitialization(this.propStep.FieldsById);
        },
        /**
         * Indique si le nombre d'étapes affichables dans le menu dropdown du bouton Précédent UNIQUEMENT, correspond au minimum requis
         * Pas de cas particulier pour le bouton Précédent : on regarde uniquement si l'étape en cours dispose d'étapes précédentes déclarées dans le JSON
         * @param {any} stepFocus Etape actuellement sélectionnée
         */
        hasRequiredPreviousStepCount(stepFocus) {
            return this.hasRequiredStepCount(this.getStepsForPreviousButtonMenu(stepFocus));
        },
        /**
         * Indique si le nombre d'étapes affichables dans le menu dropdown du bouton Suivant UNIQUEMENT, correspond au minimum requis
         * Cas particulier pour le bouton Suivant : si aucune étape n'est sélectionnée, la comparaison se fait vis-à-vis des étapes considérées comme prenières étapes sélectionnables pour initialiser le catalogue
         * (voir getStepsForInitialization)
         * @param {any} stepFocus Etape actuellement sélectionnée
         */
        hasRequiredNextStepCount(stepFocus) {
            return this.hasRequiredStepCount(this.getStepsForNextButtonMenu(stepFocus));
        },
        /**
        * Indique si le nombre d'étapes affichables dans le menu dropdown des boutons Suivant/Précédent correspond au minimum requis
        * @param {any} stepDataIds DataIDs des étapes à vérifier
        */
        hasRequiredStepCount(stepDataIds) {
            return this.getSteps(stepDataIds)?.length >= this.minimumDropDownStepCount;
        },
        /**
         * Indique si, selon le contexte, il doit être possible de cliquer sur une pastille d'étape ou non, par rapport à son accessibilité depuis l'étape en cours.
         * Utilisée dans le template pour activer ou non le clic, ainsi que le hover
         * @param {any} step
         */
        isStepClickable(step) {
            // Tout d'abord, si le mode Lecture seule est activé, rien n'est cliquable, que ça soit pour voir les rubriques (valideOnClick == false) ou non
            if (this.isReadOnly)
                return false;

            // Si la pastille à afficher est celle correspondant à la valeur actuelle du catalogue, elle n'est pas cliquable
            if (this.validateOnClick && this.stepFocus == step)
                return false;

            // Ensuite, si le mode "validation au clic" n'est pas activé, ou si aucune valeur n'est sélectionnée sur le catalogue source,
            // toutes les pastilles sont cliquables (y compris celle en cours)
            if (!this.validateOnClick || this.stepFocus == null)
                return true;

            // Si le catalogue est en écriture, que la pastille à afficher n'est pas celle correspondant à la valeur en cours, et que le mode "validation au clic" est actif,
            // Il faut vérifier si la pastille visée correspond à une étape qui serait sélectionnable en temps normal avec les boutons Suivant ou Précédent...
            let aPreviousStepsInJSON = this.getSteps(this.stepFocus?.DataIdPrevious);
            let aNextStepsInJSON = this.getSteps(this.stepFocus?.DataIdNext);
            let bIsStepAvailableBeforeActiveStep =
                /* Soit l'étape à afficher figure dans la liste de celles accessibles via le bouton Précédent avec menu... */
                (aPreviousStepsInJSON != null && aPreviousStepsInJSON.findIndex(x => Number(x.dataid) == Number(step.DbValue)) > -1) ||
                /* Soit l'étape à afficher est celle située immédiatement avant, séquentiellement, dans le catalogue affiché en stepbar
                * SAUF dans le cas où on a déterminé qu'on ne doit pas pouvoir passer à l'étape précédente (= cas où le bouton Précédent serait masqué
                * si affiché). Exemple d'un tel cas = si l'administrateur a explicitement défini qu'aucune étape précédente ne devait être proposée
                * pour l'étape en cours (DataIdPrevious = []) */
                (step == this.getPreviousStep(this.stepFocus, true) && this.previousStepAvailable);
            let bIsStepAvailableAfterActiveStep =
                /* Soit l'étape à afficher figure dans la liste de celles accessibles via le bouton Suivant avec menu... */
                (aNextStepsInJSON != null && aNextStepsInJSON.findIndex(x => Number(x.dataid) == Number(step.DbValue)) > -1) ||
                /* Soit l'étape à afficher est celle située immédiatement après, séquentiellement, dans le catalogue affiché en stepbar
                * SAUF dans le cas où on a déterminé qu'on ne doit pas pouvoir passer à l'étape suivante (= cas où le bouton Suivant serait masqué
                * si affiché). Exemple d'un tel cas = si l'administrateur a explicitement défini qu'aucune étape suivante ne devait être proposée
                * pour l'étape en cours (DataIdNext = []) */
                (step == this.getNextStep(this.stepFocus, true) && this.nextStepAvailable);
            // La pastille devient alors cliquable si elle représente une étape qui serait accessible soit via le bouton Suivant, soit via le bouton Précédent
            return bIsStepAvailableBeforeActiveStep || bIsStepAvailableAfterActiveStep;
        },
        /**
         * Change le libellé du bouton correspondant à l'étape stepIndex, si présent
         * Si le bouton ciblé n'est pas disponible, ignore sans provoquer d'erreur
         * @param {any} targetBtn Bouton à cibler (BtnPrevious ou BtnNext)
         * @param {any} stepBarStatus Statut de l'action effectuée par le bouton, modifiant la nature du libellé (GoToStep, BackToStep, ValidateStep...)
         * @param {any} stepIndex Numéro de l'étape concernée
         * @param {any} newStepLabel Libellé de la nouvelle étape ciblée par l'action du bouton
         */
        updateButtonLabel: function (targetBtn, stepBarStatus, stepIndex, newStepLabel) {
            let oBtn = this.$refs[targetBtn + "_" + stepIndex];
            if (oBtn && oBtn.length > 0)
                oBtn[0].innerHTML = this.getButtonLabel(stepBarStatus, newStepLabel);
        },
        /**
         * Renvoie le libellé à utiliser pour le bouton correspondant à l'étape stepIndex, si présent
         * Si aucune information n'est transmise, on renvoie "Précédent" pour le bouton Précédent, et "Suivant" pour le bouton Suivant
         * @param {any} buttonId Identifiant du bouton concerné ("Previous", "Next"...). Permet d'adapter le libellé en fonction
         * @param {any} newStepBarStatus Statut de l'action effectuée par le bouton, modifiant la nature du libellé (GoToStep, BackToStep, ValidateStep...)
         * @param {any} newStepLabel Libellé de la nouvelle étape ciblée par l'action du bouton
         */
        getButtonLabel: function (buttonId, newStepBarStatus, newStepLabel) {
            let bAddStepLabel = false; // cf. tâche #4 649. Les boutons n'affichent pas le libellé de l'étape
            let bUseDescriptiveLabel = false; // cf. tâche #4 649.
            if (bUseDescriptiveLabel) {
                let sLabel = "";
                switch (newStepBarStatus) {
                    case stepBarStatus.BackToStep: sLabel = this.getRes(2570); break; // Retour à l'étape
                    case stepBarStatus.ValidateStep: sLabel = this.getRes(2571); break; // Valider l'étape
                    case stepBarStatus.GoToStep: sLabel = this.getRes(2572); break; // Aller à l'étape
                    case stepBarStatus.UnknownStep: sLabel = this.getRes(2577); break; // Démarrer à l'étape
                }
                return sLabel + " " + bAddStepLabel ? newStepLabel : "";
            }
            else
                return buttonId?.startsWith("btnPrevious") ? this.getRes(25) : this.getRes(26); // Précédent / Suivant
        },
        /** Renvoie les classes CSS à appliquer aux conteneurs boutons Précédent et Suivant
        * cf. US #2 926 - Tâche #4 650 - Application du style style par défaut des boutons Vuetify
        * @param {any] sOrientation "right" ou "left"
        * @param {any} bHasDropDown Indique si le bouton est pourvu d'un menu dropdown ou non
        */
        getButtonContainerClass: function (sOrientation, bHasDropDown) {
            // La classe .v-application permet d'appliquer sur l'élément <button> enfant, certaines classes comme elevation-2, lui permettant d'être en 3D avec ombre
            return "v-application edn-group-" + sOrientation + " modal-footer steps-footer-button px-5 " + (bHasDropDown ? "edn-wrap-btn-dropdown" : "");
        },
        /**
         * Renvoie l'étape séquentiellement placée avant l'étape en cours dans le JSON, soit (en fonction de bIgnoreDataIdPrevious) :
         * - indépendamment de DataIdPrevious pour le bouton Revenir à l'étape précédente séquentiel (= bouton sans menu d'étapes)
         * - en fonction de DataIdPrevious pour le bouton Revenir à l'étape précédente non séquentiel (= bouton ne comportant qu'une seule étape, donc sans menu affiché) - Demande #90 749
         * @param {any} step Etape actuelle
         * @param {boolean} bIgnoreDataIdPrevious Indique si on doit ignorer les étapes indiquées dans DataIdPrevious (et donc, cibler l'étape précédente séquentiellement)
         */
        getPreviousStep(step, bIgnoreDataIdPrevious) {
            if (!step)
                return null;
            let availableSteps = this.propStep.FieldsById;
            let currentStepIndex = availableSteps.findIndex(s => s.DbValue == step.DbValue);
            if (bIgnoreDataIdPrevious || typeof (step.DataIdPrevious) == "undefined")
                return currentStepIndex > 0 ? availableSteps[currentStepIndex - 1] : null;
            // Demande #90 749 - Si on a une seule étape précédente définie dans le JSON, et seulement dans ce cas, c'est elle qui est ciblée
            // Si on a plusieurs étapes définies, on considère qu'on ne peut pas en désigner une comme précédente arbitrairement, et on renvoie null            
            else
                return step?.DataIdPrevious?.length == 1 ? availableSteps.find(s => s.DbValue == step.DataIdPrevious[0]) : null;
        },
        /**
         * Renvoie l'étape séquentiellement placée après l'étape en cours dans le JSON, soit (en fonction de bIgnoreDataIdNext) :
         * - indépendamment de DataIdNext pour le bouton Aller à l'étape suivante séquentiel (= bouton sans menu d'étapes)
         * - en fonction de DataIdNext pour le bouton Aller à l'étape suivante non séquentiel (= bouton ne comportant qu'une seule étape, donc sans menu affiché) - Demande #90 749
         * @param {any} step Etape actuelle
         * @param {boolean} bIgnoreDataIdNext Indique si on doit ignorer les étapes indiquées dans DataIdNext (et donc, cibler l'étape suivante séquentiellement)         
         */
        getNextStep(step, bIgnoreDataIdNext) {
            if (!step)
                return null;
            let availableSteps = this.propStep.FieldsById;
            let currentStepIndex = availableSteps.findIndex(s => s.DbValue == step.DbValue);
            if (bIgnoreDataIdNext || typeof (step.DataIdNext) == "undefined")
                return currentStepIndex < availableSteps.length - 1 ? availableSteps[currentStepIndex + 1] : null;
            // Demande #90 749 - Si on a une seule étape suivante définie dans le JSON, et seulement dans ce cas, c'est elle qui est ciblée
            // Si on a plusieurs étapes définies, on considère qu'on ne peut pas en désigner une comme suivante arbitrairement, et on renvoie null
            else
                return step?.DataIdNext?.length == 1 ? availableSteps.find(s => s.DbValue == step.DataIdNext[0]) : null;
        },
        /**
         * Indique si l'objet source dispose de toutes les caractéristiques requises pour être affiché dans la zone Assistant
         * @param {any} input
         */
        isDisplayableField: function (input) {
            let nDescId = Number(input?.DescId);
            return !input?.note && !input?.msg && input?.Format != 17 && !isNaN(nDescId) && nDescId > 0;
        },
        isDisplayableNote: function (input, step) {
            return input?.note && !input?.msg && input?.Format != 17 && step?.DisplayedDescId?.length <= 7;
        },
        /** Initialise les éléments de la zone assistant. */
        getTabsToElHaZard: function () {
            if (this.propStep.FieldsById) {
                clearInterval(this.interval);
                let tabClearMount = [];

                this.propStep.FieldsById?.forEach(a => {
                    if (!a.Hidden) {
                        this.stepClear.push(a)
                        tabClearMount.push(a)
                    }

                    if (a.active && a.validate && a.focus) {
                        this.stepFocus = a;
                    }
                });

                if (this.stepFocus) {
                    let aPreviousStepsInJSON = this.getSteps(tabClearMount.find(x => x.DbValue == this.stepFocus.DbValue)?.DataIdPrevious);
                    this.previousStepAvailable =
                        (aPreviousStepsInJSON == null && tabClearMount.findIndex(x => x.DbValue == this.stepFocus.DbValue) > 0) ||
                        aPreviousStepsInJSON?.length > 0;
                    let aNextStepsInJSON = this.getSteps(tabClearMount.find(x =>  x.DbValue == this.stepFocus.DbValue)?.DataIdNext);
                    this.nextStepAvailable =
                        (aNextStepsInJSON == null && tabClearMount.findIndex(x => x.DbValue == this.stepFocus.DbValue) < tabClearMount.length - 1) ||
                        aNextStepsInJSON?.length > 0;

                    let idxSelect;
                    let idxFocus;
                    this.propStep.FieldsById?.forEach((a, idx) => {
                        if (this.stepFocus.DbValue == a.DbValue) {
                            idxFocus = idx;
                        }
                        if (a.active == true) {
                            idxSelect = idx;
                        }
                    });

                    this.lib = this.stepFocus.DisplayLabel;
                    if (idxSelect > idxFocus) {
                        this.idxValue = stepBarStatus.BackToStep;
                    } else if (idxSelect == idxFocus) {
                        this.idxValue = stepBarStatus.ValidateStep;
                    } else if (idxSelect + 1 == idxFocus) {
                        this.idxValue = stepBarStatus.GoToStep;
                    } else {
                        this.idxValue = stepBarStatus.GoToStep;
                    }

                    if (!this.isReadOnly)
                        Vue.nextTick(() => {
                            this.updateButtonLabel(this.idxValue == stepBarStatus.BackToStep && this.displayPreviousButton ? "btnPrevious" : "btnNext", this.idxValue, idxFocus, this.lib);
                        })
                }
                // US #3 862 - TK #6 112 - En mode "Etendu" (= avec boutons Suivant et Précédent) si le catalogue n'a aucune valeur définie, on affiche le bouton Suivant si une étape suivante est sélectionnable, pour initialiser le catalogue
                else if (!this.validateOnClick) {
                    let aStepsForInitialization = this.getSteps(this.getStepsForInitialization(tabClearMount));
                    let bAllowFirstDisplayedStepSelection = false; // Indique si on autorise la sélection de la première étape AFFICHEE dans la barre d'étapes si aucune étape éligible pour l'initialisation n'a été trouvée dans le JSON
                    this.nextStepAvailable = aStepsForInitialization?.length > 0 || (bAllowFirstDisplayedStepSelection && tabClearMount?.length > 0); // Soit on a au moins plusieurs étapes éligibles pour l'initialisation, soit on vise la première étape affichée (si autorisé)
                }

                var val = this.propStep.Value;
                this.stepWidth = this.propStep.FieldsById
                    ?.filter(fld => ((val == fld.DbValue && fld.Hidden) || (!fld.Hidden)))
                    .length;

                for (var i = 0; i < this.propStep.FieldsById?.length; i++) {
                    var li = document.getElementById("id_" + this.propStep.FieldsById[i].DbValue);
                    if (li && (!this.propStep.FieldsById[i].Hidden || val == this.propStep.FieldsById[i].DbValue && this.propStep.FieldsById[i].Hidden)) {
                        if (this.propStep.FieldsById[i].active && this.propStep.FieldsById[i].validate && this.propStep.FieldsById[i].focus) {
                            li.classList.add("inProgress");
                            this.activeBar = true
                            this.calHeightNote();
                            return;
                        } else {
                            if (li) {
                                li.classList.add("complete")
                            }
                        }
                    }
                }
                this.calHeightNote();
                $(function () {
                    $('.select2').select2()
                })

                for (var i = 0; i < this.propStep.FieldsById?.length; i++) {
                    if (!this.propStep.FieldsById[i].active && !this.propStep.FieldsById[i].validate && !this.propStep.FieldsById[i].focus) {
                        var li = document.getElementById("id_" + this.propStep.FieldsById[i].DbValue)
                        if (!this.propStep.FieldsById[i].Hidden || val == this.propStep.FieldsById[i].DbValue && this.propStep.FieldsById[i].Hidden) {
                            if (li == null)
                                return;
                            li.classList.remove("complete")
                        }
                        this.activeBar = false
                    } else {
                        this.activeBar = true
                        return;
                    }
                }

            }
        },
        /** Retourne les champs dans les barres d'étapes */
        getDisplayedDescIdContent(stpid){
            let arr = [];
            this.propStep?.FieldsById[stpid]?.DisplayedDescId?.forEach(fld => {
                fld?.note ? arr.push(fld) : arr.push(this.propDetail?.find(c => c?.DescId == fld?.DescId));  
            })
            return arr.slice(0, this.wizardFldsMaxNb);
        }
    },
    template: `
    <div v-if="reloadStep && Object.keys(propStep).length">
        <h3
            v-if="propStep.Label && !propStep.LabelHidden"
            :style="{ color: stepDetail.StyleForeColor + '!important'}"
            :class="getLabelClass"
            class="progressbar-title"
        >
            {{propStep.Label}}
            <span
                v-if="propStep.ToolTipText?.length > 1"
                ref="progressbarTitle"
                v-on:mouseout="showTooltip(false, 'progressbarTitle', false, propStep.ReadOnly, propStep)"
                v-on:mouseover="showTooltip(true, 'progressbarTitle', false, propStep.ReadOnly, propStep)"
                class="icon-info-circle info-tooltip"
             />
        </h3>
        <div :key="CounterReload" id="box-step" v-bind:class="[this.propWizardBarEmpty ? 'emptyDataInput' : '', 'box-header steps-header']">
            <ul v-bind:class="getAdditionalStepBarClasses" ref="progressbar" class="progressbar">
				<li
                    v-if="getDisplayForStep(step)"
                    v-for="step in propStep.FieldsById"
                    :key="step.id"
                    :title="step.DisplayLabel"
                    :ref="'step_' + step.DbValue"
                    class="fa listStep"
                    :id="'id_' + step.DbValue"
                    v-bind:style="{ width: 100/stepWidth + '%' }"
                    :data-step="'id_' + step.DbValue"
                >
					<div class="circle-container">
						<a href="#!">
							<div v-if="isStepClickable(step)" v-on:click.self="onStepClick(step)" class="circle-button"></div>
                            <div v-else class="circle-button inactive"></div>
						</a>
					</div><p class="text--step two-lines">{{step.DisplayLabel}}</p>
				</li>
			</ul>
            <div 
                v-if="this.JsonWizardBar?.JsonWizardBar && this.JsonWizardBar?.WelcomeBoard?.Display != 'hide' && (!this.activeBar || this.activeBar == null) && !this.propWizardBarEmpty" 
                class="steps-text not-draggable"
            >
                <div class="startTxtAssistant">
                    <div class="col-md-12   vertical-scroll startTxtAssistantContent">
                        <h3 v-html="getTitleStartTxtAssistant"></h3>
                        <p v-html="getBodyStartTxtAssistant"></p>
                        <p v-if="!this.JsonWizardBar.WelcomeBoard?.Body">{{getRes(2575)}}</p>
                    </div>
                </div>
            </div>
            <div id="steps--container" class="steps-text not-draggable detailContent">
                <div
                    v-for="(step, idxs) in propStep.FieldsById"
                    :key="step.id"
                    v-if="getDisplayForStep(step) && step.DisplayedDescId.length"
                    ref="stepContent"
                    :class="[
                        getClassFormat(step),
                        !step.focus ? 'displayNone' : '',
                        'stepContent id_' + step.DbValue
                    ]"
                >
                
                    <div         
                        v-for="(input,index) in getDisplayedDescIdContent(idxs)"
                        :key="input.id"
                        v-if="isDisplayableField(input)"
                        class="fileInput"
                        :FileId="input.FileId"
                        :DivDescId="input.DescId"
                        :divdescidAssitZone="input.DescId + '_' + idxs "
                        :bf="input.Formula ? '1' : '0'"
                        :mf="input.HasMidFormula || input.HasORMFormula ? '1' : '0'"
                    >
                        <div 
                            :tp="input.Format" 
                            class="form-group" 
                            :class="[input.Format === 11 ? 'reverse':null,  input.ReadOnly ||  input.Format == 26 ? 'readOnlyComponent' : '',getComponentType(input)]">
                            <div 
                                v-if="!specialCSS(vuetifyInput, input?.Format)"
                                :class="setFileLabelClass(input)"
                                class="left d-flex align-center"
                            >
                                <div
                                    v-if="input.Format != 11" 
                                    :ref="'label_' + input.DescId"
                                    :style="{ color: input.StyleForeColor}"
                                    class="left-label text-truncate text-muted"
                                    :class="setFileLabelClass(input)"
                                >
                                    {{input.Label}}
                                </div>
                                <label v-else :for="input.DescId + '_Assistant'" v-bind:style="{ color: input.StyleForeColor}" class="left-label text-muted" v-bind:class="{ 'italicLabel': input.Italic,'boldLabel': input.Bold,'underLineLabel': input.Underline,'labelHidden': input.LabelHidden || specialCSS(tabFormatBtnLbl, input.Format) ,'no-label':specialCSS(tabFormatBtnSep, input.Format) , 'readonly':input.ReadOnly }">{{input.Label}}</label>
                                <span :ref="'hidden_' + input.DescId"  v-bind:class="{'labelHidden': input.LabelHidden || specialCSS(tabFormatBtnLbl, input.Format) ,'no-label':specialCSS(tabFormatBtnSep, input.Format)}" v-if="input.Required" class="requiredRubrique">*</span>
                                <template v-if="false">
                                    <span ref="info" v-if="input.ToolTipText?.length > 1" v-on:mouseout="infoTooltip(input,false,$event)" v-on:mouseover="infoTooltip(input,true,$event)" class="icon-info-circle info-tooltip"  ></span>
                                    <span ref="info" v-if="input.ToolTipText?.length > 1" 
                                        @mouseout="showTooltip(false,  Object.keys($children[index - 1].$refs)[1] ,true, input.ReadOnly, input)" 
                                        @mouseover="showTooltip(true,  Object.keys($children[index - 1].$refs)[1],true, input.ReadOnly, input)" 
                                        class="icon-info-circle info-tooltip"></span>
                                </template>
                                <span :ref="'label_info' + input.DescId" v-on:mouseout="showTooltip(false, 'info', false, input.ReadOnly, input)" v-on:mouseover="showTooltip(true, 'info', false, input.ReadOnly, input)" ref="info" v-if="input.ToolTipText?.length > 1" class="icon-info-circle info-tooltip"></span>
                            </div>
                            <component @setWaitIris="eWaiterDetail" :prop-assistant-nb-index="idxs" :prop-assistant="true" :data-input="input" :is="dynamicFormatChamps(input)"></component>
                        </div>
                    </div>
                    <div v-if="isDisplayableNote(input, step)" v-for="(input,noteid) in getDisplayedDescIdContent(idxs)" :key="noteid" class="box-body note-steps">
                        <strong @click="getModalNote(getNoteTxt(input))" class="title-note">
                            <i class="fas fa-expand margin-r-5"></i>
                        </strong>
                        <div v-html="getNoteTxt(input)"></div>
                    </div>
                </div>

                <!-- Bouton Précédent avec Dropdown étapes -->
                <div v-bind:class="!previousStepAvailable ? 'DisabledBtn' : '' " :class="getButtonContainerClass('left', true)" v-if="displayPreviousButton && hasRequiredPreviousStepCount(stepFocus)">
                    <eDropDownButton ref="btnPreviousWithDropDown" id="btnPreviousWithDropDown" @actionDropDownClicked="validateStepDropDown" :sAlign="'dropdown-menu-left'" :sButtonClass="getButtonClass" :sButtonTitle="getButtonLabel('btnPreviousWithDropDown', 0, getPreviousStep(stepFocus, false)?.DisplayLabel)" sLeftIcon="fas fa-chevron-left steps-footer-button-left" :step="stepFocus" :PropStep="propStep" :aStepDrop="getSteps(getStepsForPreviousButtonMenu(stepFocus))">
                        <template v-slot:prepend>
                            <i class="fas fa-chevron-left steps-footer-button-left"></i>
                        </template>
                        <template v-slot:content>
                            <span class="text-truncate">{{getButtonLabel('btnPreviousWithDropDown', 0, getPreviousStep(stepFocus, false)?.DisplayLabel)}}</span>
                        </template>
                    </eDropDownButton>
                </div>
                <!-- Bouton Précédent SANS Dropdown étapes -->
                <div v-bind:class="!previousStepAvailable ? 'DisabledBtn' : '' " :class="getButtonContainerClass('left', false)" v-if="displayPreviousButton && !hasRequiredPreviousStepCount(stepFocus)"> 
                    <v-btn ref="btnPrevious" id="btnPrevious" v-on:click="validateStep(getPreviousStep(stepFocus, false), propStep.FieldsById, true)" :data-step="stepFocus?.id" type="button" :class="getButtonClass"  text light height="100%"><i class="fas fa-chevron-left steps-footer-button-left"></i>{{getButtonLabel('btnPrevious', 0, getPreviousStep(stepFocus, false)?.DisplayLabel)}}</v-btn>
                </div>
                <!-- Bouton Suivant avec Dropdown étapes -->
                <div v-bind:class="!nextStepAvailable ? 'DisabledBtn' : '' " :class="getButtonContainerClass('right', true)" v-if="displayNextButton && hasRequiredNextStepCount(stepFocus)">
                    <eDropDownButton ref="btnNextWithDropDown" id="btnNextWithDropDown" @actionDropDownClicked="validateStepDropDown" :sAlign="'dropdown-menu-right'" :sButtonClass="getButtonClass" :sButtonTitle="getButtonLabel('btnNext', 0, getNextStep(stepFocus, false)?.DisplayLabel)" sRightIcon="fas fa-chevron-right steps-footer-button-right" :step="stepFocus" :PropStep="propStep" :aStepDrop="getSteps(getStepsForNextButtonMenu(stepFocus))">
                        <template v-slot:append>
                            <i class="fas fa-chevron-right steps-footer-button-right"></i>
                        </template>
                        <template v-slot:content>
                            <span class="text-truncate">{{getButtonLabel('btnNext', 0, getNextStep(stepFocus, false)?.DisplayLabel)}}</span>
                        </template>
                    </eDropDownButton>
                </div>
                <!-- Bouton Suivant SANS Dropdown étapes -->
                <div  :class="[getButtonContainerClass('right', false),!nextStepAvailable ? 'DisabledBtn' : '' ]" v-if="displayNextButton && !hasRequiredNextStepCount(stepFocus)">
                    <v-btn ref="btnNext" id="btnNext" @click="validateStep(stepFocus ? getNextStep(stepFocus, false) : stepClear[0], propStep.FieldsById, true)" :data-step="stepFocus?.id" type="button"  :class="getButtonClass" text light height="100%">{{getButtonLabel('btnNext', 1, stepFocus?.DisplayLabel)}}<i class="fas fa-chevron-right steps-footer-button-right"></i></v-btn>
                </div>
            </div>
        </div>
    </div>
`
};