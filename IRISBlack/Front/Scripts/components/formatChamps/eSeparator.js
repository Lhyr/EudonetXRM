import EventBus from '../../bus/event-bus.js?ver=803000';
import { showInformationIco, displayInformationIco } from '../../methods/eComponentsMethods.js?ver=803000';
import { eFileComponentsMixin } from '../../mixins/eFileComponentsMixin.js?ver=803000';
import { initDatabaseIrisBlack, initDatabase, manageDatabaseIrisBlack } from "../../methods/eFileMethods.js?ver=803000";
import { initIndexedDB, manageIndexedDB, setDataDbAsync, getDataDbAsync, mergeDataDbAsync, filterDataDbAsync, countDataDbAsync, firstDataDbAsync } from "../../methods/eBkmMethods.js?ver=803000";
import { FieldType } from '../../methods/Enum.min.js?ver=803000';

export default {
    name: "eSeparator",
    data() {
        return {
            closed: null,
            options: Object,
            modif: false
        };
    },
    async created() {
        EventBus.$off("showDetailForSeparator");
        if(this.isDisplayed){
            await this.initLocalDatabase();                           
            this.getUiDetail(); 
        }

    },
    updated() { },    
    watch:{
        async isDisplayed(val){
            if(val){
                await this.initLocalDatabase();
                // si elle est fermée par défaut, il n'est pas nécessaire d'ajouter la fausse colonne
                if (!this.closed) {
                    // si c'est le premier fois, cree les fakeSepElem
                    await this.fillFreeFieldHoles(); 
                    this.getUiDetail();
                }  
            }
        }
    },
    mounted() {
        this.setContextMenu();
        this.displayInformationIco();
        // Au premier affichage du composant, on prend la valeur ouvert/fermé à partir du contrôleur et on l'applique
        // On la récupérera/créera ensuite en même temps depuis IndexedDB en asynchrone (depuis le created > initLocalDatabase()),
        // et c'est à la récupération que "closed" sera remise à jour
        // closed (la data) étant utilisée pour l'affichage. La valeur issue du contrôleur n'étant utilisée que pour l'initialiser
        if (this.isClosedValue && this.isDisplayed) {
            this.fnHideElements(false); // false indique de ne pas effectuer la mise à jour en base. Elle sera effectuée par l'appel à initLocalDatabase() plus haut
            this.closed = true;
        } else {
            this.closed = false;
        }
    },
    computed: {

        // Permet de lancer la fonction de calcul pour les titre séparateur, si on click sur le tabsBar detail
        async isReloadUi() {
            if (this.propReloadDetailFunction) {
                // on ajoute un timout controller pour laisser le temps en css de bootstrap de display bloc tout le detail.
                setTimeout(async () => {
                    await  this.getUiDetail() 
                }, 200);
            }

            return this.propReloadDetailFunction ? 'reloadingUi' : '';
        },

        /**
        * Renvoie responsiveMobile si la props mobile passe à true, dans le cas ou le parent (fileDetail) détecte un changement au resize de la fenêtre.
        * Cette fonction nous permet de déclancher l'updated, même si la class renvoyée n'est pas utilisée.
        */
        isMobile() {
            return this.mobile ? 'responsiveMobile' : ''
        },

        getSepCssClass() {
            return {
                'headReadOnly': this.propHead,
            }
        },
        /**
         * Renvoie true si la valeur donnée par le contrôleur indique que le séparateur doit être FERME
         * ATTENTION, ici, True renvoyé par le contrôleur = séparateur FERME (False = ouvert)
         * Value est à interpréter dans le sens "Closed"
         * Donc si True renvoyé par le contrôleur (ou valeur équivalente, "true", "1"...) = séparateur fermé
         */
        isClosedValue() {
            return this.dataInput.Value ? (this.dataInput.Value.toLowerCase() == 'true' || Number(this.dataInput.Value) == 1) : false;
        },
        /**
         * Renvoie un tableau des éléments du DOM dépendant au titre séparateur
         */
    },
    methods: {
        // Permet de lancer la fonction de calcul pour les titre séparateur
        async getUiDetail() {
            
            this.colorizeChildrenFields();
            /**
            * Une fois que le DOM est monté on supprime les divs ".fakeSepElem" qui ne servent pas.
            */
            this.$nextTick(() => {
                let fakers = document.querySelectorAll(".bkm-tab.active div.detailContent > div.fakeSepElem");
                fakers.forEach(
                    elem => {
                        if (elem.clientWidth <= 1 || elem.clientHeight <= 1) {
                            elem.remove();
                        }
                    }
                );
            })
        },
        showInformationIco,
        displayInformationIco,
        /**
         * Fonction qui masque ou affiche les éléments en-dessous d'une barre de séparation, sur le clic.
         * */
        getHeight() {

            EventBus.$emit('timeOutRightNav', this.options);
        },
        /**
        * Renvoie un tableau des éléments du DOM dépendant au titre séparateur
        */
        getChildrenFields() {

            /** US 3800 - task 5 849 */

            let arrChildrenElements = []; // éléments du séparateur

            let arrSeparators = this.$parent.$children.filter(child => child.dataInput.Format == FieldType.Separator); //Tous les séparateurs

            let iActualSepPosY = this.dataInput.PosY + 1; // position du 1er champs du séparateur

            let iActualSep = arrSeparators.indexOf(arrSeparators.find(child => child.dataInput.PosY == this.dataInput.PosY)); // index du séparateur ciblé

            let arrGlobalFields = [...document.querySelectorAll(".bkm-tab.active .detailContent div[y]")]; // tous les champs de détail

            let lastPosY = Math.max(...arrGlobalFields.map(field => field.getAttribute('y'))) + 1 //dernier champs de détail en ordonnées

            let iNextSepr = iActualSep < arrSeparators.length - 1 ?  // Position Y du séparateur suivant
                arrSeparators[iActualSep + 1]?.dataInput.PosY
                : lastPosY;

            for (let i = iActualSepPosY; i < iNextSepr; i++) {
                // tous les champs de la zone détails entre le séparateur actuel et le prochain
                let arrFields = [...document.querySelectorAll(".bkm-tab.active .detailContent div[y='" + i + "']")];

                arrFields.forEach(elem => {
                    arrChildrenElements.push(elem)
                })
            }

            // Et on renvoie l'ensemble
            return arrChildrenElements;
        },
        /**
         * Vérifie si les champs du titre séparateur combient tous les emplacements de la grille de la zone Détails.
         * S'il y a des "trous" (emplacements libres), on crée alors des champs factices aux emplacements en question,
         * pour permettre leur colorisation via colorizeChildrenFields
         * */
        fillFreeFieldHoles() {
            // On commence par rapatrier tous les champs situés en-dessous du titre séparateur ciblé
            let oChildrenElements = this.getChildrenFields();
            // S'il n'y en a pas, on sort tout de suite
            if (!oChildrenElements || oChildrenElements.length == 0)
                return;
            // Puis l'élément correspondant au titre séparateur
            let oSepElement = document.querySelector(".bkm-tab.active div[DivDescId='" + this.dataInput.DescId + "']");
            // On récupère les coordonnées minimales et aximales
            let nMinY = this.dataInput.PosY + 1;
            let nMaxX = this.$parent.propCols - 1;
            let nMaxY = Math.max(...new Set(oChildrenElements.map(f => {
                let ny = parseInt(f.getAttribute("y"));
                let nh = (parseInt(f.getAttribute("rowspan")) || 1);

                return ny + nh;

            })));

            // Puis on les parcourt un à un, et on cible ceux qui n'ont pas de voisin, ou un voisin invisible
            // On commence par la fin, afin de cibler également les éléments masqués en display: none par fileDetail, et vérifier s'ils sont situés
            // au milieu de champs visibles. Auquel cas, on supprime leur display: none pour ne pas générer d'espace blanc indésirable
            let oCurrentElement = null; // on stocke l'élément du DOM en cours d'examen, afin de positionner l'élément factice par rapport à lui
            for (let lineIndex = nMaxY; lineIndex >= nMinY; lineIndex--) {
                let nVisibleFieldsOnThisLine = 0;
                let nHiddenFieldsOnThisLine = 0;
                for (let colIndex = nMaxX; colIndex > -1; colIndex--) {
                    let oExistingFieldInSlot = oChildrenElements.find(f => {
                        let nx = parseInt(f.getAttribute("x"));
                        let ny = parseInt(f.getAttribute("y"));
                        let nh = ny + (parseInt(f.getAttribute("rowspan")) || 1);
                        let nw = nx + (parseInt(f.getAttribute("colspan")) || 1);

                        return (colIndex >= nx && colIndex < nw)
                            && (lineIndex >= ny && lineIndex < nh)
                    });

                    if (!oExistingFieldInSlot) {
                        let oNewFakeElement = document.createElement("div");
                        oNewFakeElement.className = "fakeSepElem";
                        oNewFakeElement.setAttribute("fileid", this.getFileId);
                        oNewFakeElement.setAttribute("SepDescId", this.dataInput.DescId);
                        oNewFakeElement.setAttribute("x", colIndex);
                        oNewFakeElement.setAttribute("y", lineIndex);
                        oNewFakeElement.setAttribute("style", `grid-area: ${lineIndex + 1} / ${colIndex + 1}`);
                        // Si on a pas encore repéré d'élément voisin, on insère l'élément factice à la fin de la grille des champs (= l'élémment
                        // parent du titre séparateur)
                        // Sinon, on l'insère à côté de son voisin, juste avant lui (puisqu'on parcourt la boucle en sens inverse)

                        if (!oCurrentElement) {
                            oNewFakeElement.innerHTML = "&nbsp;"; /** sous chrome un contenu est nécessaire.*/
                            oSepElement?.parentElement?.appendChild(oNewFakeElement);
                        }
                        else {
                            oCurrentElement.insertAdjacentElement("beforebegin", oNewFakeElement);
                        }


                        // Le dernier élément connu (pour le positionnement du prochain) devient alors l'élément tout juste inséré
                        oCurrentElement = oNewFakeElement;
                    }
                    else {
                        oCurrentElement = oExistingFieldInSlot;
                        // Si on tombe sur un champ existant et visible sur la ligne en cours (en partant de la droite, puisque la boucle est en
                        // sens inverse), on le comptabilise
                        if (oExistingFieldInSlot.style.display != "none")
                            nVisibleFieldsOnThisLine++;
                        else
                            nHiddenFieldsOnThisLine++;
                        // Et si...
                        if (
                            colIndex == 0 && /* on a analysé la ligne complète */
                            nVisibleFieldsOnThisLine > 0 && /* on a déterminé que la ligne comportait au moins un champ visible */
                            nHiddenFieldsOnThisLine > 0 /* on a déterminé que la ligne comportait au moins un champ masqué */
                        ) {
                            // On rend tous les champs visibles sur la ligne pour éviter les trous
                            oChildrenElements.filter(f => f.getAttribute("y") == lineIndex && f.style.display == "none")?.forEach(invisibleElement => invisibleElement.style.display = '');
                        }
                    }
                }
            }
        },

        /**
         * Ajoute une couleur de fond sur les champs dépendant du titre séparateur
         */
        colorizeChildrenFields() {
            // Si le titre séparateur n'a pas de couleur définie en admin, on ne fait rien
            let sColor = this.getCustomForeColor;
            if (!sColor)
                sColor = "#000000";
            // On définit l'opacité à appliquer sur la couleur : 5% en code hexadécimal
            let sBackgroundOpacity = '0d';
            // On commence par rapatrier tous les champs situés en-dessous du titre séparateur ciblé
            let oChildrenElements = this.getChildrenFields();
            // Et on les colore
            oChildrenElements.forEach(
                elem => {
                    // On pose la couleur sur l'élément
                    elem.style.backgroundColor = sColor + sBackgroundOpacity;

                    // Et une bordure
                    let bAddBorder = this.mobile || elem.getAttribute("x") == 0;
                    elem.style.borderLeft = (bAddBorder ? "1" : "0") + "px solid " + sColor;

                }
            );

        },

        /**
         * Affiche ou masque le titre séparateur, et si demandé, envoie le nouveau statut ouvert/fermé en base pour mémorisation
         * @param {any} bUpdateInDb si true, envoie en base la nouvelle valeur (nouveau statut replié/ouvert) du séparateur
         */
        fnHideElements(bUpdateInDb) {
            if(!this.isDisplayed){
                return false;
            }
            try {
                // On commence par rapatrier tous les champs situés en-dessous du titre séparateur ciblé
                let oChildrenElements = this.getChildrenFields();
                //Régression #84 265 :Les titres séparateurs permettent l'édition des rubriques quand ils sont fermés
                //oChildrenElements.map(elem => this.closed ? elem.classList.remove("fileInputDisplayNone") : elem.classList.add("fileInputDisplayNone"));
                // Et on les masque
                oChildrenElements.forEach(elem => elem.hidden = !this.closed);
                this.options = this.closed ? { timeoutCalculHeight: 200 } : { timeoutCalculHeight: 350 };
                this.closed = !this.closed;
                // Puis on met à jour en base le statut de fermeture/ouverture du titre séparateur
                if (bUpdateInDb) {
                    let oUpdatedSeparator = { DescId: this.dataInput.DescId, Closed: this.closed };
                    this.setSeparatorStateInDb(oUpdatedSeparator);
                }
                if (this.closed == false) {
                    this.getUiDetail();
                }
            }
            catch (e) {
                EventBus.$emit('globalModal', {
                    typeModal: "alert", color: "danger", type: "zoom",
                    close: true, maximize: false, id: 'alert-modal', title: this.getRes(6576), msgData: e, width: 600,
                    btns: [{ lib: this.getRes(30), color: 'default', type: 'left' }], datas: this.getRes(7050)
                });

                return;
            }
        },

        initIndexedDB,
        manageIndexedDB,
        setDataDbAsync,
        getDataDbAsync,
        mergeDataDbAsync,
        filterDataDbAsync,
        countDataDbAsync,
        firstDataDbAsync,
        manageDatabaseIrisBlack,
        initDatabaseIrisBlack,
        initDatabase,

        /** Initialisation de la base de donnée locale avec le statut d'ouverture/fermeture du titre séparateur **/
        initLocalDatabase: async function () {
            try {
                // On initialise l'accès à la base
                this.dbSeparator = await this.initDatabaseIrisBlack();
                // On crée l'objet qui va permettre de faire la MAJ en base, avec les informations renvoyées par le contrôleur pour commencer
                let oUpdatedSeparator = { DescId: this.dataInput.DescId, Closed: this.isClosedValue };
                // On vérifie si le DescID du séparateur en cours figure dans la base
                let oSearch = { tab: this.getTab, descId: this.dataInput.DescId };
                let separatorDataCount = await this.countDataDbAsync(this.dbSeparator, "Separator", oSearch);
                if (separatorDataCount > 0) {
                    // On récupère l'enregistrement correspondant dans IndexedDB
                    let oSeparatorDataInDb = await this.firstDataDbAsync(this.dbSeparator, "Separator", oSearch);
                    // On prépare l'objet pour la mise à jour en base
                    oUpdatedSeparator.Closed = oSeparatorDataInDb?.closed;
                    // Et on met à jour le titre séparateur actuellement affiché à partir de ce qu'on a trouvé en base, si l'affichage doit être modifié
                    // Visuellement (dans le DOM) puis en base
                    if (this.closed != oSeparatorDataInDb?.closed)
                        this.fnHideElements(true);
                }
            }
            catch (e) {
                this.manageDatabaseIrisBlack(e);
            }
        },
        /**
         * Met à jour la base de données IndexedDB avec le statut d'activation du titre séparateur
         */
        async setSeparatorStateInDb(oUpdatedSeparator) {
            // Si aucune donnée de MAJ n'est transmise en paramètre, on prend les valeurs en cours (renvoyées par le contrôleur)
            if (!oUpdatedSeparator)
                oUpdatedSeparator = { DescId: this.dataInput.DescId, Closed: this.isClosedValue };
            // On construit l'objet au format base de données, contenant la valeur mise à jour
            // La base ne stockant pas toutes les données passées en paramètre, on ne garde que ce dont on a besoin
            let oDbData = { tab: this.getTab, descId: oUpdatedSeparator.DescId, closed: oUpdatedSeparator.Closed };
            // Et on met à jour les infos d'activation dans IndexedDB
            try {
                await this.mergeDataDbAsync(this.dbSeparator, "Separator", oDbData);
            }
            catch (ex) {
                console.log("Erreur de mise à jour de l'information du titre séparateur dans IndexedDB : " + ex.message);
            }
        },
    },
    props:{
        dataInput:{
            type:Object
        },
        propHead:{
            type:Boolean
        },
        mobile:{
            type:Boolean
        },
        propReloadDetailFunction:{
            type:Boolean
        },
        isDisplayed:{
            type:Boolean
        }
    },
    mixins: [eFileComponentsMixin],
    template: `
<div
    :class="{
        'closeSep': closed,
        isMobile,
        isReloadUi,
        'text-truncate':true
    }"
    @click="fnHideElements(true), getHeight()"
>
    <!-- Champ -->
    <div :style="{borderBottomColor:getCustomForeColor}" class="ellips input-group hover-input separator-field cursor-pointer" :class="getSepCssClass" 
        :title="getTitle">
        <span :class="getCssClass" :style="{color:getCustomForeColor}">{{dataInput.Label}}</span>
        <i class="fas fa-chevron-down" :style="{color:getCustomForeColor}"></i>
    </div>
</div>
`
};