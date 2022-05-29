import { eFileComponentsMixin } from '../../../mixins/eFileComponentsMixin.js?ver=803000'
import EventBus from '../../../bus/event-bus.js?ver=803000';
import { selectValue, onUpdateCallback, validateCatGenericIris, cancelCatGenericIris, adjustColWidth, NewWidthCol, EnlargeColsIfNeeded, adjustColsWidth } from '../../../methods/eFieldEditorMethods.js?ver=803000';
import { updateMethod, showCatalogGeneric, showTooltip, hideTooltip, updateListVal, verifComponent, showInformationIco, displayInformationIco, removeOneValue, getTextWidth } from '../../../methods/eComponentsMethods.js?ver=803000';
/**
 * Mixin commune aux composants eCatalog.
 * */
export const eCatalogMixin = {
    mixins: [eFileComponentsMixin],
    data() {
        return {

            selectedValues: new Array(),
            selectedLabels: new Array(),
            partOfAfterValidate: null,
            partOfAfterCancel: null,
            catalogDialog: null,
            modif: false,
            that: this,
            bEmptyDisplayPopup: false,
            showMru: false,
            tabsMru: Object,
            showAllValues: true,
            hiddenVal: false,
            textOnLeft: false,
            textOnRight: false,
            blocked: false,
            truncatedTxt: false,
            truncatedSize: Object,
            translationVal: 0,
            startLine: 130,
            elemOnRight: 65,
            elemOnLeft: 20,
            bulletEllipsis: [
                { direction: 'right', class: 'before-bullet' },
                { direction: 'left', class: "after-bullet" }
            ],
            catVal: '',
            adaptativeValuesReady: false,
            adaptativeValuesTimer: null, // # 4081 - Timer utilisé pour redéclencher en différé les valeurs adaptatives si elles n'ont pas pu être déclenchées via updated(),
            catalogCaretWidth: 10,
            nbElementPlusWidth: 20
        };
    },
    components: {
        eAlertBox: () => import(AddUrlTimeStampJS("../../modale/alertBox.js"))
    },
    computed: {
        IsDisplayReadOnly: function () {
            return (this.propHead || this.dataInput.ReadOnly)
        },
        /**
         * Calcule le nombre maximum de valeurs affichables dans le conteneur, en fonction de la longueur textuelle des valeurs et de la police
         * */
        nMaxCatalogDisplay: function () {
            let oCatalogData = this.getCatalogMaxPossibleValueCountAndLargestValueWidth();
            // if (oCatalogData?.maxPossibleValueCount)
                return oCatalogData?.maxPossibleValueCount || 0;
            // else
                //return 2; // on renvoie toujours une valeur par défaut au cas où (remarque/TODO : sur certains catalogues, elle était parfois à 4.)
        },
        /**
         * Permet de savoir sur le nombre de catalogues affichés est supérieur
         * au maximum autorisé (variable nMaxCatalogDisplay).
         * */
        isNbCatalogGTMax: function () {
            return (this.valueMultiple.length > this.nMaxCatalogDisplay)
        },
        /** Fonction calcul�e. Si displayvalue n'est pas vide et qu'on est dans un cas de valeurs
        * multiples, on d�coupe, � l'aide d'une expression r�guli�re.
        * @returns {string} ou {string[]} le r�sultat */
        valueMultiple: function () {

            if (this.dataInput.DisplayValue == "")
                return [];

            if (!this.dataInput.Multiple)
                return { id: this.dataInput.Value, value: this.dataInput.DisplayValue };

            let val = / *; */g[Symbol.split](this.dataInput.Value);
            let dispVal = / *; */g[Symbol.split](this.dataInput.DisplayValue);

            let res = val.map(function (e, i) {
                return { id: e, value: dispVal[i] };
            });

            return res;
        },

        /** retourne les valeurs en fonctions du nombre maximal à afficher. */
        valueMultipleGTMax: function () {
            return this.isNbCatalogGTMax
                ? this.valueMultiple.slice(0, this.nMaxCatalogDisplay)
                : this.valueMultiple;
        },

        /** retourne les valeurs à partir du nombre maximal à afficher. */
        valueMultipleFromMax: function () {
            return this.isNbCatalogGTMax
                ? this.valueMultiple.slice(this.nMaxCatalogDisplay)
                : [];
        },

        classMru: function () {
            return (this.showMru) ? 'mru-opened' : 'multiRenderer form-control';
        },
        mruMode: function () {
            return (this.showMru) ? 'mru-mode' : '';
        },
        /**Récupère l'id unique du composant */
        getUniqueId() {
            return 'cat-val' + this._uid;
        },
        /** Indique la direction du texte qui défile (si trop long) */
        textPosition() {
            return (this.textOnLeft) ? 'text-on-left' : (this.textOnRight) ? 'text-on-right' : ''
        },
        /** bloque le texte si l'on sort de l'elippsis pour afficher le texte */
        isBlocked() {
            return (this.blocked) ? 'blocked' : ''
        },
        /** affiche l'ellipsis de gauche pour revenir en arrière dans le défilement */
        isVisible() {
            return (this.blocked || this.textOnLeft || this.textOnRight) ? 'visible' : ''
        },
        /** valeur de translation du text (en fonction de sa largeur) */
        translationValue() {
            return `translateX(${this.translationVal}px)`
        },
        /** indique si il s'agit d'un tag troinquée ou non */
        isTruncated() {
            return (this.truncatedTxt) ? 'truncated-text' : '';
        },

        /** Détermine si on doit afficher l'icone de modification. */
        isIcoToDisplay: function () {
            return !(this.showMru || this.dataInput.Multiple) || this.valueMultiple < 1;
        },
    },
    mixins: [eFileComponentsMixin],
    async mounted() {
        this.displayInformationIco();
        EventBus.$on('label', options => {
            console.log(options)
        });
    },
    beforeDestroy() { },
    filters: {},
    methods: {
        showInformationIco,
        displayInformationIco,
        verifComponent,
        updateListVal,
        hideTooltip,
        showTooltip,
        selectValue,
        adjustColWidth,
        NewWidthCol,
        EnlargeColsIfNeeded,
        adjustColsWidth,
        onUpdateCallback,
        validateCatGenericIris,
        cancelCatGenericIris,
        updateMethod,
        showCatalogGeneric,
        removeOneValue,
        getTextWidth,

        /**
         * Lorsque le DOM est prêt, avec des valeurs exploitables pour le calcul des valeurs adaptatives (width en px), on autorise l'exécution des computed
         * calculant les valeurs (valueMultiple, valueMultipleGTMax, nMaxCatalogDisplay, etc.)
         */
        enableAdaptativeValues() {
            this.adaptativeValuesReady = true;
            window.clearTimeout(this.adaptativeValuesTimer); // #4 081 - on supprime l'éventuel Timer utilisé pour redéclencher la vérification
        },
        /**
         * Appelle une fonction qui a été déportée.
         * */
        showCatalogGenericViewIris: function () {

            if (this.dataInput.ReadOnly)
                return false;

            this.bEmptyDisplayPopup = false;
            this.$el.parentElement.parentElement.classList.remove("border-error");
            this.modif = true;

            this.showCatalogGeneric(
                this.dataInput.EAction,
                this.dataInput.DataDescT,
                this.dataInput.DataEnumT,
                this.dataInput.Multiple,
                this.dataInput.IsTree,
                this.dataInput.Value,
                "",
                "",
                this.dataInput.PopupDescId,
                this.dataInput.PopupType,
                this.dataInput.BoundDescId,
                this.dataInput.BoundFieldPopup,
                this.dataInput.Pdbv,
                this.dataInput.Label,
                "eCatalogEditorObject",
                false,
                () => {
                    this.validateCatGenericIris(() => {
                        try {
                            this.updateMethod(this, this.selectedValues.join(';'), undefined, undefined, this.dataInput);
                            this.closeMru();
                        } catch (e) {
                            EventBus.$emit('globalModal', {
                                typeModal: "alert", color: "danger", type: "zoom",
                                close: true, maximize: false, id: 'alert-modal', title: this.getRes(6576), msgData: e, width: 600,
                                btns: [{ lib: this.getRes(30), color: 'default', type: 'left' }], datas: this.getRes(7050)
                            });

                            return;
                        }
                    });
                },
                () => { this.cancelCatGenericIris() },
                LOADCATFROM.UNDEFINED

            );

        },
        async editVal(val) {
            // Tooltip
            EventBus.$emit('editvalcat', val);
        },

        /**
         * Chargement des catalogues.
         * */
        async  LoadMruCatalog() {

            if (!this.$refs.MRU) {
                this.showCatalogGenericViewIris();

                return false;
            }

            try {
                await this.$refs.MRU.LoadMru();
            } catch (e) {
                EventBus.$emit('globalModal', {
                    typeModal: "alert", color: "danger", type: "zoom",
                    close: true, maximize: false, id: 'alert-modal', title: this.getRes(6576), msgData: e, width: 600,
                    btns: [{ lib: this.getRes(30), color: 'default', type: 'left' }], datas: this.getRes(7050)
                });
                return;
            }

            if (this.$refs.MRU.DataMru && this.$refs.MRU.DataMru.length > 0) {
                this.showMru = true;
            } else {
                this.showMru = false;
                this.showCatalogGenericViewIris();
            }
        },

        /**Permet de stopper complètement la transition si l'on sort du tag */
        stopTransition() {
            if (this.catVal != "") {
                this.catVal.style.transform = ""
                this.catVal.style.transition = ""
            }
            this.textOnRight = false;
            this.textOnLeft = false;
            this.blocked = false;
        },
        /**
         * Méthode qui permet d'effectuer une translation soit à droite soir à gauche si le texte est tronquée
         * Si l'on reste sur le tag mais que l'on quitte l'ellipsis (avant ou après) alors le translate se stoppe  (blocked)
         * ( on obtient la valeur avec translatePosition)
         * @param {any} direction direction souhaité (droite ou gauche)
         * @param {any} bStatus translation active ou non 
         */
        translateText(direction, bStatus) {
            let catValWidth = this.$refs.catLi.offsetWidth;
            if (direction == "right") {
                this.textOnRight = bStatus;
                this.translationVal = 0 + this.elemOnLeft;
                this.catVal.style.transition = `background .3s ease-in, transform 5s linear, opacity .3s linear .8s`
            } else {
                this.textOnLeft = bStatus;
                this.translationVal = -((this.truncatedSize[this.getUniqueId]) - (catValWidth - this.elemOnRight));
                //Ajout d'une transition dynamique pour afficher à la même vitesse le texte en fonction de la taille du champs
                this.catVal.style.transition = `background .3s ease-in, transform ${5 * Math.floor(Math.abs(this.translationVal / 100))}s linear, opacity .3s linear .8s`

            }
            if (bStatus == false) {
                let translatePosition = getComputedStyle(this.catVal).transform.split(',')[4];
                this.translationVal = translatePosition;
                this.blocked = true;
            } else {
                this.blocked = false;
            }
        },
        /** Détermine l'ellipsis des tags des catalogues simples
         * On utilise un canvas afin de déterminer  la taille que peut occuper le texte avec une famille de police et une taille donnée
        * Utilise l'id catVal utilisé dans chaque catalogue simple modifiable */
        async setCatalogEllipsis() {
            await this.$nextTick();
            if (!document.getElementById(this.getUniqueId)) {
                // console.log(this.getUniqueId);
            }
            else {
                let canvas = document.createElement('canvas');
                //let catVal;
                if (this.$refs.catVal)
                    this.catVal = this.$refs.catVal;
                else if (!this.$refs.catVal && !this.dataInput.Multiple)
                    this.catVal = this.$refs['field' + this.dataInput.DescId];
                else
                    return false;
                canvas.style.position = "fixed";
                var txt = this.catVal.innerText;
                document.body.appendChild(canvas);
                var ctx = canvas.getContext("2d");
                //ctx.font = `600 12.5px Source Sans Pro`;
                ctx.font = `
                ${getComputedStyle(this.catVal).fontWeight} 
                ${getComputedStyle(this.catVal).fontSize} 
                ${getComputedStyle(this.catVal).fontFamily} `;
                let txtSize = ctx.measureText(txt).width;
                if (txtSize > ((this.catVal.offsetWidth) ? this.catVal.offsetWidth : 250)) {
                    this.truncatedTxt = true;
                    this.truncatedSize[this.getUniqueId] = txtSize;
                }
                document.body.removeChild(canvas);
            }
        },

        /**
         * Renvoie l'élément du DOM HTML correspondant au conteneur du catalogue
        * @param {any} sender Evènement appelant la fonction (dans le cas d'un Observer). Peut permettre de moduler le comportement de la fonction
         * */
        getCatalogContainer(sender) {
            let currentElement = this.$el ? this.$el : sender?.target;
            // Dans le cas où on accède à cette fonction depuis un computed d'un composant enfant (NbElementPlus), il faut cibler le parent
            if (!currentElement)
                currentElement = this.$parent?.$el;
            let aContainers = Array.from(document.querySelectorAll("div[divdescid='" + this.dataInput.DescId + "']"));
            // On boucle sur tous les conteneurs trouvés, correspondant au DescID courant, pour tester quel est le parent réel
            // du composant Catalogue en cours d'analyse (this.$el ou sender à défaut)
            // La fonction isParentElementOf (dans eTools.js, globale) renvoie true si correspondance
            let nElementIndex = 0;
            let parentContainerElement = null;
            while (parentContainerElement == null && nElementIndex < aContainers.length) {
                let containerElement = aContainers[nElementIndex];
                if (isParentElementOf(currentElement, containerElement))
                    parentContainerElement = containerElement;
                else
                    nElementIndex++; // on continue de boucler sur tous les candidats potentiels jusqu'à épuisement
            }
            return parentContainerElement;
        },
        /**
         * Renvoie l'élément du DOM HTML correspondant à la liste ul contenant les valeurs de catalogue
         * */
        getCatalogValueLists() {
            return this.getCatalogContainer()?.querySelectorAll("ul[field='field" + this.dataInput.DescId + "']");
        },
        /**
         * Renvoie la largeur, en pixels, du conteneur du catalogue, disponible pour l'affichage des valeurs
         * */
        getCatalogContainerWidth() {
            let oCatalogContainer = this.getCatalogContainer();
            let oCatalogContainerDetails = oCatalogContainer?.querySelector("details");
            let oCatalogContainerSpan = oCatalogContainerDetails?.querySelector("summary span.multiRenderer");
            let oTargetContainerComputedStyle = oCatalogContainer ? getComputedStyle(oCatalogContainer) : null;
            let oTargetContainerDetailsComputedStyle = oCatalogContainerDetails ? getComputedStyle(oCatalogContainerDetails) : null;
            let oTargetContainerSpanComputedStyle = oCatalogContainerSpan ? getComputedStyle(oCatalogContainerSpan) : null;
            let nWidth = 0;
            // On tente d'abord d'obtenir l'élément dont la taille est la plus proche de celle que l'on peut exploiter, sans les enrobages autour
            if (oTargetContainerSpanComputedStyle?.width?.indexOf("px") > 0)
                nWidth = getNumber(oTargetContainerSpanComputedStyle.width);
            // Ou bien, depuis l'encart "Détails" qui contient le bouton "Modifier", sans compter les marges entre
            // valeurs et l'espace réservé à l'affichage du nombre de valeurs supplémentaires (5% de la largeur du conteneur)
            else if (oTargetContainerDetailsComputedStyle?.width?.indexOf("px") > 0) {
                let oTargetCatalogValues = document.querySelector(".multiRenderer ul li");
                let oTextComputedStyle = oTargetCatalogValues ? getComputedStyle(oTargetCatalogValues) : null;
                let nCatalogChipMarginLeft = getNumber(oTextComputedStyle?.marginLeft);
                let nCatalogChipMarginRight = getNumber(oTextComputedStyle?.marginRight);
                let nCatalogPlusCountWidth = (getNumber(oTargetContainerDetailsComputedStyle.width) * (5 / 100));
                nWidth = getNumber(oTargetContainerDetailsComputedStyle.width) - nCatalogChipMarginLeft - nCatalogChipMarginRight - nCatalogPlusCountWidth;
            }
            // Et sinon, à défaut, on renvoie la taille du conteneur global avec son enrobage, mais sans ses marges
            //Ajout de la largeur du chevron de la liste déroulante (catalogCaretWidth) et de l'indicateur numérique (nbElementPlusWidth) dans le calcul afin d'avoir la bonne largeur dispo dans le champs
            else if (oTargetContainerComputedStyle?.width?.indexOf("px") > 0)
                nWidth =
                    getNumber(oTargetContainerComputedStyle.width) -
                    getNumber(oTargetContainerComputedStyle.paddingLeft) -
                    getNumber(oTargetContainerComputedStyle.paddingRight) -
                    getNumber(oTargetContainerComputedStyle.marginLeft) -
                    getNumber(oTargetContainerComputedStyle.marginRight) -
                    (this.catalogCaretWidth + this.nbElementPlusWidth);
            return nWidth;
        },
        /**
         * Calcule le nombre maximal de valeurs affichables dans la liste du conteneur du catalogue, en fonction de la largeur (texte) des valeurs à afficher
         * et de leur police. Renvoie un objet contenant les 2 informations (nombre maximal de valeurs affichables + largeur de la plus grande valeur)
         * */
        getCatalogMaxPossibleValueCountAndLargestValueWidth() {
            let nMaxPossibleValueCount = 0;
            let nCurrentFittedWidth = 0;
            let nLargestValueWidth = 0;
            let aSourceValues = this.valueMultiple?.value ? new Array(this.valueMultiple.value) : this.valueMultiple;
            // On récupère le style calculé d'une bulle de valeur de catalogue pour déterminer sa police d'affichage et son alignement
            // On cible une chips "Catalogue" pour obtenir son style (police, marges)
            // .clsEltSummary serait préférable pour les catalogues multiples, mais n'est pas toujours disponible lors du calcul
            let oTargetCatalogValues = Object.keys(this.$parent?.$children[0]?.$refs).length > 1 && this.$parent?.$children[0]?.$refs['catVal' + parseInt(aSourceValues.find(cat => cat)?.id)][0]  || document.querySelector(".multiRenderer ul li:not(.mru-li):not(.catalogVal)");
            let oTextComputedStyle = oTargetCatalogValues && !oTargetCatalogValues?.classList?.contains('mru-li')? getComputedStyle(oTargetCatalogValues) : null;
            let nCatalogChipMarginRight = getNumber(oTextComputedStyle?.marginRight);
            let nCatalogChipPaddingLeft = getNumber(oTextComputedStyle?.paddingLeft);
            let oTargetCatalogValuesCross = document.querySelector(".multiple_choice_remove");
            let oTextCrossComputedStyle = oTargetCatalogValuesCross ? getComputedStyle(oTargetCatalogValuesCross) : null;
            let nCatalogChipCrossPaddingRight = getNumber(oTextComputedStyle?.paddingRight);
            // Correction des styles calculés avec des valeurs par défaut si besoin
            if (isNaN(nCatalogChipMarginRight))
                nCatalogChipMarginRight = 5;
            if (isNaN(nCatalogChipPaddingLeft))
                nCatalogChipPaddingLeft = 8;
            if (isNaN(nCatalogChipCrossPaddingRight))
                nCatalogChipCrossPaddingRight = 16;
            // Récupération de la largeur du conteneur
            let nCatalogContainerWidth = this.getCatalogContainerWidth();
            // On boucle sur chaque valeur de catalogue pour déterminer sa largeur, que l'on stocke si elle est la plus large détectée jusqu'alors.
            // Puis on ajoute sa largeur au total de pixels requis pour afficher les valeurs examinées jusqu'alors, et on incrémente le compteur de valeurs
            // affichables dans le conteneur
            // Si la largeur totale des valeurs examinées est inférieure à l'espace offert par le conteneur, on continue l'examen.
            // Si la largeur totale des valeurs examinées excède l'espace disponible, on interrompt la boucle            
            let nCatalogValueIndex = 0;
            let bMaxCapacityReached = false;
            while (!bMaxCapacityReached) {
                let catalogValue = aSourceValues[nCatalogValueIndex];
                // Si catalogValue est on objet de type { id: 0, value: ""), on prend uniquement .value en compte
                catalogValue = catalogValue?.value ? catalogValue.value : catalogValue;
                // On appelle getTextWidth() de eComponentsMethods pour calculer la largeur en pixels de la valeur en fonction du texte et de la police
                let nCurrentWidth = getTextWidth(catalogValue, true, oTextComputedStyle?.font, oTextComputedStyle?.textAlign, oTextComputedStyle?.textBaseline, false);
                // On rajoute à la longueur mesurée, l'enrobage de la chips : padding de 6 pixels à gauche + 19 à droite + marge de 5 pixels à droite
                nCurrentWidth += nCatalogChipPaddingLeft + nCatalogChipCrossPaddingRight + nCatalogChipMarginRight;
                // On met à jour l'espace occupé par l'ensemble des valeurs examinées jusqu'ici
                nCurrentFittedWidth += nCurrentWidth;
                // Puis on vérifie si on a atteint la largeur du conteneur, sans tenir compte de la dernière marge de droite (qui ne sera pas visible)
                if (nCurrentFittedWidth - nCatalogChipMarginRight < nCatalogContainerWidth) {
                    // la largeur du conteneur n'est pas encore dépassée : on met à jour le nombre de valeurs possibles
                    nMaxPossibleValueCount++;
                    if (nCurrentWidth > nLargestValueWidth)
                        nLargestValueWidth = nCurrentWidth;
                    // Et on continue la vérification, sauf si on est arrivés au bout des valeurs à traiter
                    nCatalogValueIndex++;
                    bMaxCapacityReached = nCatalogValueIndex == aSourceValues.length;
                }
                 // le conteneur ne peut pas accueillir davantage de valeurs : on interrompt l'itération
                else
                    bMaxCapacityReached = true;
            }
            return {
                maxPossibleValueCount: nMaxPossibleValueCount,
                largestValueWidth: nLargestValueWidth
            }
        },
        /**
         * Vérifie si le conteneur de catalogue dispose d'une largeur calculée en pixels, signifiant ainsi que le DOM est prêt, et que l'on peut procéder
         * au rendu des valeurs de catalogue
         * @param {any} sender Evènement appelant la fonction (dans le cas d'un Observer). Peut permettre de moduler le comportement de la fonction
         */
        checkCatalogContainerAvailability(sender) {
            let oTargetContainer = this.getCatalogContainer(sender);
            let oTargetContainerComputedStyle = oTargetContainer ? getComputedStyle(oTargetContainer) : null;
            if (oTargetContainerComputedStyle?.width?.indexOf("px") > 0)
                this.enableAdaptativeValues(); // on indique au template du composant que le calcul des valeurs adaptatives peut désormais se faire
        },

    },
    props: ["dataInput", "propHead", "propListe", "propSignet", "propIndexRow", "propAssistant", "propDetail", "propAssistantNbIndex"],
}