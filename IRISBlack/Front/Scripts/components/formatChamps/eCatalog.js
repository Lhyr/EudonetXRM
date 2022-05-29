import EventBus from '../../bus/event-bus.js?ver=803000';
import { selectValue, onUpdateCallback, validateCatGenericIris, cancelCatGenericIris, adjustColWidth, NewWidthCol, EnlargeColsIfNeeded, adjustColsWidth } from '../../methods/eFieldEditorMethods.js?ver=803000';
import { updateMethod, showCatalogGeneric, showTooltip, hideTooltip, updateListVal, verifComponent, showInformationIco, displayInformationIco, removeOneValue, setCatalogEllipsis, translateText, onCatalogValueMouseOver, onCatalogValueMouseLeave, stopTransition, verifCharacter } from '../../methods/eComponentsMethods.js?ver=803000';
import { eFileComponentsMixin } from '../../mixins/eFileComponentsMixin.js?ver=803000';
import { } from '../../directives/eOutSideClick.js?ver=803000';
import { emptyValue } from '../../methods/Enum.js?ver=803000'

export default {
    name: "eCatalog",
    components: {
        eAlertBox: () => import(AddUrlTimeStampJS("../modale/alertBox.js")),
        eMru: () => import(AddUrlTimeStampJS("../eMru/eMru.js")),
       /* eCatalogFile: () => import(AddUrlTimeStampJS("./eCatalog/eCatalogFile.js")),*/
        eCatalogList: () => import(AddUrlTimeStampJS("./eCatalog/eCatalogList.js")),
        eCatalogFileMultiplePane: () => import(AddUrlTimeStampJS("./eCatalog/eCatalogFileMultiplePane.js")),
        eCatalogBtnBase: () => import(AddUrlTimeStampJS("./eCatalog/eCatalogBtnBase.js")),
    },
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
            truncatedTxt: false,
            truncatedSize: Object,
            translationVal: 0,
            catVal: '',
            containerWidth: '',
            textReverse: false,
            time: '',
            newTime: '',
            once: false,
            accordionOpened:false
        };
    },
    watch: {
        /**
         * Tâche 3036 - Ajout d'un watcher afin de déterminer si le contenu du texte est plus long que le container
         * On passe par un canvas pour simuler la longueur du texte
         * On affiche puis cache la zone détail unqiuement pour avoir la tailles des container des champs catalogue( display:none l'empêche)
         * 
         */
        'dataInput.DisplayValue': {
            immediate: true,
            handler(val, oldVal) {
                this.bEmptyDisplayPopup = (this.dataInput.Required && val === "");

                this.$nextTick(() => {
                    this.setCatalogEllipsis();
                });
            }
        }
    },
    computed: {
        IsDisplayReadOnly: function () {
            return (this.dataInput.ReadOnly)
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
        /** valeur de translation du text (en fonction de sa largeur) */
        translationValue() {
            //return `${this.translationVal}px`
            return ''
        },
        /** indique si il s'agit d'un tag troinquée ou non */
        isTruncated() {
            return (this.truncatedTxt) ? 'truncated-text' : '';
        },
        isReversed() {
            return (this.textReverse) ? 'reversed' : '';
        },
        /** Détermine si on doit afficher l'icone de modification. */
        isIcoToDisplay: function () {
            return !(this.showMru || (this.dataInput.Multiple && this.propDetail) || (this.dataInput.Multiple && this.propHead) || this.dataInput.ReadOnly) || this.valueMultiple.length < 1;
        },
        isEmpty: function () {
            return this.dataInput.DisplayValue == '' ? 'emptyCatalog' : ''
        },
        isMultiple: function () {
            return this.dataInput.Multiple == true ? 'multiple' : ''
        },
        /** Doit-on afficher le bouton de modification ? */
        isCatBtnDisplay: function () {

            if (this.dataInput.Multiple)
                return this.valueMultiple.length < 1;

            return !this.accordionOpened;
        },
        /** Permet de savoir, lorsqu'on est sur un catalogue multiple, si ce dernier est ouvert ou non  */
        getAccordionState() {
            return this.accordionOpened && this.dataInput.Multiple ? 'accordionOpened' : !this.accordionOpened && this.dataInput.Multiple ? 'accordionClosed' : '';
        },
        /** Affiche ou non les chips si pas de contenu */
        hideChips(){
            return emptyValue.includes(this.dataInput.DisplayValue);
        },
        getInput:{
            get: function(){
                return this.dataInput;
            },
            set:function(input){
                let dataInput = {...this.dataInput,...input}
                this.$emit('update:data-input',dataInput)
            }
        },
    },
    mixins: [eFileComponentsMixin],
    async mounted() {
        this.setContextMenu();
        this.displayInformationIco();
    },
    beforeDestroy() { },
    filters: {},
    methods: {
        stopTransition,
        verifCharacter,
        translateText,
        onCatalogValueMouseOver,
        onCatalogValueMouseLeave,
        setCatalogEllipsis,
        removeOneValue,
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

        /**
         * Chargement des catalogues.
         * */
        async LoadMruCatalog() {

            if (!this.$refs.MRU) {
                this.showCatalogGenericViewIris();

                return false;
            }

            if (this.IsDisplayReadOnly)
                return false;

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

            if (this.$refs.MRU?.DataMru && this.$refs.MRU?.DataMru.length > 0) {
                this.showMru = true;
            } else {
                this.showMru = false;
                this.showCatalogGenericViewIris();
            }
        },

        /**
         * Appelle une fonction qui a été déportée.
         * */
        showCatalogGenericViewIris: function () {

            if (this.IsDisplayReadOnly)
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
                            EventBus.$emit('emitLoadAll', {
                                reloadAll: true
                            });
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
        }
    },
    props: ["dataInput", "propHead", "propListe", "propSignet", "propIndexRow", "propAssistant", "propDetail", "propAssistantNbIndex", "propDataDetail"],
    template: `
<div v-click-outside-iris="showMru" :class="['globalDivComponent catComponent',isMultiple]">
    <!-- FICHE -->
    <div 
        v-if="!propListe"
        ref="catalog" 
        @mouseout="!(showMru || dataInput.Multiple) ? showTooltip(false,'catalog',false,IsDisplayReadOnly,dataInput) : ''"  
        @mouseover="!(showMru || dataInput.Multiple) ? showTooltip(true,'catalog',false,IsDisplayReadOnly,dataInput):''" 
        :class="[IsDisplayReadOnly? 'headReadOnly read-only' : '', 'ellips input-group hover-input cat-group align-center', mruMode, getAccordionState, bEmptyDisplayPopup ? 'display-alert' : '']"
    >
        <!-- Si le champ multiple -->
        <eCatalogFileMultiplePane @accordionOpened="accordionOpened = $event" v-if="dataInput.Multiple" :data-input="dataInput" />

        <!-- Si le champ simple et modifiable -->
        <span v-show="showMru && propHead" @mouseleave.stop :id="getUniqueId" ref="catVal" class="catalog-val mru-cat-width text--transparent">{{dataInput.DisplayValue}}</span>
        <div
            v-if="!(dataInput.ReadOnly) && !dataInput.Multiple"
            id="drop-recherche"
            type="text"
            :class="[classMru,'ellipsDiv input-line fname']"
            @mouseover.stop="">
                       
            <eMru v-show="showMru"
                ref="MRU"
                :dataInput.sync="getInput"
                :placeholder="dataInput.Watermark"
                :focusSearch="showMru"
                @openSpecificDialog="showCatalogGenericViewIris"
                @closeMru="closeMru">
            </eMru>
            <span  @mouseleave.stop class="cat-placeholder text-truncate" v-if="this.dataInput.Value == ''  && dataInput.Watermark != null && !showMru" >{{dataInput.Watermark}}</span>
            <ul  @mouseleave="stopTransition" v-show="!showMru" :ref="'field'+dataInput.DescId" :field="'field'+dataInput.DescId" @click="LoadMruCatalog();" >
                <li :class="[isTruncated,'catalogVal']" ref="catLi" v-if="dataInput.DisplayValue">
                    <span v-if="truncatedTxt && !IsDisplayReadOnly" @mouseleave.stop ref="changeDir" @mouseover="translateText()" :class="['left-side',isReversed]"></span>
                    <span
                        :style="{textIndent:translationValue}"
                        @mouseover="onCatalogValueMouseOver($event, null, dataInput, 'catalog')"
                        @mouseleave.stop="onCatalogValueMouseLeave($event, null, dataInput)"
                        :id="getUniqueId" ref="catVal"
                        :class="['catalog-val',isReversed]"
                    >{{dataInput.DisplayValue}}</span>
                    <span
                        ref="uniqueval" 
                        v-on:mouseout.stop="showTooltip(false,'uniqueval',false,IsDisplayReadOnly,dataInput)" 
                        v-on:mouseover.stop="showTooltip(true,'uniqueval',false,IsDisplayReadOnly,dataInput)" 
                        v-if="dataInput.DisplayValue" 
                        class="multiple_choice_remove fas fa-times-circle cursor_pointeur" 
                        role="presentation" 
                        @click="removeOneValue($event, valueMultiple)"
                    ></span>
                </li>
            </ul>
       </div>

        <!-- Si le champ simple et pas modifiable -->
        <div 
        v-if="(dataInput.ReadOnly) && !dataInput.Multiple" 
        :style="{ color: dataInput.ValueColor}" 
        type="text" 
        class="ellipsDiv form-control input-line fname">
            <div
                v-if="!hideChips" 
                :class="[isTruncated,'catalogVal']" 
                ref="catLi"
            >
                <div :id="getUniqueId" class="targetIsTrue">
                    <span 
                        :id="getUniqueId" ref="catVal" 
                        :class="[isReversed,'cat-readonly-val']" 
                        @mouseover="onCatalogValueMouseOver($event, null, dataInput, 'catalog')" 
                        @mouseleave="onCatalogValueMouseLeave($event, null, dataInput)"
                    >{{ dataInput.DisplayValue }}</span>
                    <span class="multiple_choice_remove" role="presentation"></span>
                </div>
            </div>
        </div>

        <!-- Icon -->
        <eCatalogBtnBase v-if="isCatBtnDisplay"
            :data-input="dataInput"
            
            @sendAction="LoadMruCatalog" />

		<!-- Message d'erreur après la saisie dans le champs -->
		<eAlertBox
            v-if="this.bEmptyDisplayPopup && !(dataInput.ReadOnly)"
        >
			<p>{{getRes(2471)}}</p>
		</eAlertBox>
    </div>

    <!-- LISTE -->
    <div
        v-if="propListe"
        ref="catalog"
        v-on:mouseout="showTooltip(false,'catalog',false,IsDisplayReadOnly,dataInput)"
        v-on:mouseover="showTooltip(true,'catalog',false,IsDisplayReadOnly,dataInput)"
        v-bind:class="[propListe ? 'listRubriqueRelation' : '', 'ellips input-group hover-input cat-group', IsDisplayReadOnly?'read-only':'',isEmpty]"
    >
        <!-- Si le champ multiple et  modifiable -->
        <span
            v-if="dataInput.Multiple && !dataInput.ReadOnly"
            @click="showCatalogGenericViewIris"
            class="Modif multiRenderer form-control input-line fname"
        >
            <ul
                :ref="'field'+dataInput.DescId"
                :field="'field'+dataInput.DescId"
            >
                <li class="catalogVal" v-bind:style="{ background: dataInput.ValueColor, borderColor:dataInput.ValueColor}" v-for="value in valueMultipleGTMax" :key="value.id">
                    <span 
                        class="multiple-catalog-val" 
                        @mouseover="onCatalogValueMouseOver($event, value, dataInput, 'catalog')" 
                        @mouseleave="onCatalogValueMouseLeave($event, value, dataInput)" 
                    >{{value.value}}</span>
                    <span
                        v-if="!dataInput.IsTree"
                        :testid="value.id"
                        @click="removeOneValue($event, value);"
                        class="multiple_choice_remove fas fa-times-circle cursor_pointeur"
                        role="presentation"
                    ></span>
                </li>
            </ul>
        </span>
        </span>

        <!-- Si le champ multiple et pas modifiable-->
        <span v-if="dataInput.Multiple && dataInput.ReadOnly" :id="'ID_' + dataInput.DescId" class=" noModif multiRenderer form-control input-line fname">
            <ul :ref="'field'+dataInput.DescId" :field="'field'+dataInput.DescId">
                <li class="catalogVal" v-bind:style="{ background: dataInput.ValueColor, borderColor:dataInput.ValueColor}" v-for="value in valueMultipleGTMax" :key="value.id">
                    <span
                        @mouseover="onCatalogValueMouseOver($event, value, dataInput, 'catalog')" 
                        @mouseleave="onCatalogValueMouseLeave($event, value, dataInput)"
                    >{{value.value}}</span>
                    <span class="multiple_choice_remove" role="presentation"></span>
                </li>
            </ul>
        </span>

        <!-- Si le champ simple et modifiable -->
        <div :class="[isTruncated,'catalogVal']"  @mouseleave="stopTransition" ref="catLi" @click="showCatalogGenericViewIris" v-if="!dataInput.ReadOnly && !dataInput.Multiple" v-bind:style="{ color: dataInput.ValueColor}" type="text" class="modifSimple ellipsDiv form-control input-line fname">
           
            <div v-if="dataInput.DisplayValue != ''" @mouseleave.stop :id="getUniqueId"   :field="'field'+dataInput.DescId" class="targetIsTrue">
                <span v-if="truncatedTxt" @mouseleave.stop ref="changeDir" @mouseover="translateText()" :class="['left-side',isReversed]"></span>
                <span
                    :ref="'field'+dataInput.DescId" 
                    :class="['catalog-val',isReversed]" 
                    @mouseover="onCatalogValueMouseOver($event, null, dataInput, 'catalog')"
                    @mouseleave="onCatalogValueMouseLeave($event, null, dataInput)"
                 >{{dataInput.DisplayValue}}</span> 
                <span
                    v-if="dataInput.DisplayValue"
                    ref="uniqueval"
                    class="multiple_choice_remove fas fa-times-circle cursor_pointeur"
                    role="presentation"
                    @click="removeOneValue($event, valueMultiple)"
                ></span>
            </div> 

        </div>


        <!-- Si le champ simple et pas modifiable -->
        <div @mouseleave="stopTransition" v-bind:style="{ color: dataInput.ValueColor}" v-if="dataInput.ReadOnly && !dataInput.Multiple"  ref="catLi" type="text" :class="[isTruncated,'catalogVal NoModifSimple ellipsDiv form-control input-line fname testdf']">
            <div v-if="dataInput.DisplayValue != '' " :id="getUniqueId" :style="{ minWidth:containerWidth + 35 + 'px'}"  class="targetIsTrue">
                <span v-if="truncatedTxt" @mouseleave.stop ref="changeDir" @mouseover="translateText()" :class="['left-side',isReversed]"></span>
                <span
                    :ref="'field'+dataInput.DescId" 
                    :class="['cat-readonly-val',isReversed]" 
                    @mouseover="onCatalogValueMouseOver($event, null, dataInput, 'catalog')" 
                    @mouseleave="onCatalogValueMouseLeave($event, null, dataInput)"
                >{{dataInput.DisplayValue}}</span> 
                <span class="multiple_choice_remove" role="presentation"></span>
            </div> 
        </div>

        <!-- Icon -->
        <eCatalogBtnBase 
            v-if="isIcoToDisplay"
            :data-input="dataInput"
            @sendAction="showCatalogGenericViewIris" />

    </div>


</div>
`
};