import { selectValue, onUpdateCallback, cancelAdvancedDialog, validateUserDialog } from '../../methods/eFieldEditorMethods.js?ver=803000';
import { openUserDialog, updateMethod, showTooltip, hideTooltip, updateListVal, showInformationIco, displayInformationIco, removeOneValue, setCatalogEllipsis, stopTransition, translateText, updateDataVal } from '../../methods/eComponentsMethods.js?ver=803000';
import { eFileComponentsMixin } from '../../mixins/eFileComponentsMixin.js?ver=803000';
import EventBus from '../../bus/event-bus.js?ver=803000';

//import { LoadUserValues } from '../../methods/eFileMethods.js';

export default {
    name: "eUser",
    data() {
        return {
            selectedValues: null,
            selectedLabels: null,
            advancedDialog: null,
            modif: false,
            showMru: false,
            tabsMru: Object,
            truncatedTxt: false,
            truncatedSize: Object,
            textReverse: false
        };
    },
    components: {
        eMru: () => import(AddUrlTimeStampJS("../eMru/eMru.js"))
    },
    watch: {
        /**
         * T�che 3036 - Ajout d'un watcher afin de d�terminer si le contenu du texte est plus long que le container
         * On passe par un canvas pour simuler la longueur du texte
         * On affiche puis cache la zone d�tail unqiuement pour avoir la tailles des container des champs catalogue( display:none l'emp�che)
         * 
         */
        'dataInput.DisplayValue': {
            immediate: true,
            handler(val, oldVal) {
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
        classMru: function () {
            return (this.showMru) ? 'mru-opened' : 'multiRenderer form-control';
        },
        mruMode: function () {
            return (this.showMru) ? 'mru-mode' : '';
        },
        /** indique si il s'agit d'un tag troinqu�e ou non */
        isTruncated() {
            return (this.truncatedTxt) ? 'truncated-text' : '';
        },
        isReversed() {
            return (this.textReverse) ? 'reversed' : '';
        },
        /**R�cup�re l'id unique du composant */
        getUniqueId() {
            return 'cat-val' + this._uid;
        }
    },
    mounted() {
        this.setContextMenu();
        this.displayInformationIco();
    },
    beforeDestroy() { },
    methods: {
        removeOneValue,
        displayInformationIco,
        showInformationIco,
        hideTooltip,
        showTooltip,
        selectValue,
        onUpdateCallback,
        openUserDialog,
        cancelAdvancedDialog,
        validateUserDialog,
        updateMethod,
        updateListVal,
        setCatalogEllipsis,
        stopTransition,
        translateText,
        //LoadUserValues,

        showMruClick() {
            LoadMruUser();
        },


        async LoadMruUser() {
            if (!this.$refs.MRU) {
                this.openDial();

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
            }
            else {
                this.openDial();
            }
        },
        /**
         * Va rechercher la fonction d'ouverture des dialogues
         * des utilisateurs.
         * */
        openDial: async function () {
            // #85 993 - A l'ouverture d'une bo�te de dialogue, fermeture des MRU
            this.closeMru();			
			
            var that = this;
            this.openUserDialog(
                this.dataInput.DescId,
                this.dataInput.Label,
                this.dataInput.FullUserList,
                this.dataInput.Multiple,
                this.cancelAdvancedDialog,
                () => {
                    this.validateUserDialog(() => {
                        if (this.propUpdateInDatabase == null || this.propUpdateInDatabase)
                            updateMethod(this, this.selectedValues.join(';'), undefined, undefined, this.dataInput);
                        else
                            updateDataVal(this, this.selectedValues.join(';'), this.selectedLabels.join(';'))
                    });
                    this.closeMru();
                },
                this.dataInput.Value,
                this.dataInput.DisplayValue,
            );
        }

    },
    props: ["dataInput", "propHead", "propListe", "propSignet", "propIndexRow", "propAssistant", "propDetail", "propAssistantNbIndex", "propDataDetail", "propUpdateInDatabase"],
    mixins: [eFileComponentsMixin],
    template: `
<div 
    v-click-outside-iris="showMru" 
    class="globalDivComponent userComponent"
>
    <!-- FICHE -->
    <div 
        v-if="!propListe"
        ref="user" 
        v-on:mouseout="showTooltip(false,'user',false,IsDisplayReadOnly,dataInput)" 
        v-on:mouseover="showTooltip(true,'user',false,IsDisplayReadOnly,dataInput)"  
        v-bind:class="[
            dataInput.ReadOnly ? 'headReadOnly' : '', 
            'ellips input-group hover-input user-group d-flex align-center'
        ]" 
    >

        <!-- Si le champ multiple et modifiable -->
       <span 
            v-if="!(dataInput.ReadOnly) && dataInput.Multiple"
            v-on:click="openDial()"  
            class="Modif multiRenderer form-control input-line fname bold"
        >
            <ul>
                <li        
                    v-for="value in this.valueMultiple" 
                    :key="value.id"
                    v-if="value!=''" 
                    v-bind:style="{ background: dataInput.ValueColor, borderColor:dataInput.ValueColor}" 
                >
                    {{value.value}}
                    <span 
                        :testid="value.id" 
                        @click="removeOneValue($event, value);" 
                        class="multiple_choice_remove fas fa-times-circle cursor_pointeur" 
                        role="presentation"
                    ></span>
                </li>
            </ul>
        </span>

        <!-- Si le champ multiple et pas modifiable -->
        <span 
            v-if="(dataInput.ReadOnly) && dataInput.Multiple " 
            :id="'ID_' + dataInput.DescId" 
            class="noModif multiRenderer form-control input-line fname bold "
        >
            <ul>
                <li 
                    v-for="value in this.valueMultiple" 
                    :key="value.id"
                    v-bind:style="{ background: dataInput.ValueColor, borderColor:dataInput.ValueColor}" 
                >
                    {{value.value}}
                    <span 
                        class="multiple_choice_remove fas fa-lock" 
                        role="presentation"
                    ></span>
                </li>
            </ul>
        </span>            

         <!-- Si le champ simple et modifiable -->
        <div 
            v-if="!(dataInput.ReadOnly) && !dataInput.Multiple"
            id="drop-recherche"   
            type="text" class="ellipsDiv input-line fname"           
            :class="classMru"
            @mouseover.stop=""
        >
            <eMru v-show="showMru"
                :dataInput="dataInput"
                :ref="'MRU'"
                @openSpecificDialog="openDial"
                @closeMru="closeMru"
            />
            <ul
                v-show="!showMru"
                @mouseleave="stopTransition"   
                :ref="'field'+dataInput.DescId" 
                :field="'field'+dataInput.DescId"
                 @click="LoadMruUser();"
             >
                <li 
                    v-if="this.dataInput.Value == ''  && dataInput.Watermark != null" 
                    @mouseleave.stop 
                    class="cat-placeholder" 
                >{{dataInput.Watermark}}</li>
                <li 
                    v-if="dataInput.DisplayValue"
                    @mouseleave.stop 
                    :class="[isTruncated,'userVal']" 
                >
                    <span 
                        ref="catVal" 
                        :class="['catalog-val',isReversed]"
                    >{{dataInput.DisplayValue}}</span>
                    <span 
                        v-if="dataInput.DisplayValue" 
                        :id="getUniqueId" 
                        ref="uniqueval" 
                        v-on:mouseout.stop="showTooltip(false,'uniqueval',false,IsDisplayReadOnly,dataInput)" 
                        v-on:mouseover.stop="showTooltip(true,'uniqueval',false,IsDisplayReadOnly,dataInput)" 
                        class="multiple_choice_remove fas fa-times-circle cursor_pointeur" 
                        role="presentation" 
                        @click="removeOneValue($event, valueMultiple)"
                    >
                    </span>
                </li>
            </ul>
       </div>

        <!-- Si le champ simple et pas modifiable -->
        <div 
            v-if="dataInput.ReadOnly && !dataInput.Multiple"
            @mouseleave="stopTransition" 
            v-bind:style="{ color: dataInput.ValueColor}"   
            type="text"  
            ref="catLi" 
            :class="[isTruncated,'userVal NoModifSimple ellipsDiv form-control input-line fname']"
        >         
            <div 
                v-if="dataInput.DisplayValue.length > 1"
                :id="getUniqueId" 
                :field="'field'+dataInput.DescId"  
                class="targetIsTrue"
            >
                <span 
                    ref="catVal" 
                    :class="['cat-readonly-val d-flex align-center',isReversed]"
                >{{dataInput.DisplayValue}}</span>
            </div> 
        </div>

        <!-- Icon -->
        <span 
            v-if="!showMru || !dataInput.ReadOnly" 
            @click=" !IsDisplayReadOnly ? LoadMruUser() :''" 
            class="input-group-addon"
        >
            <a href="#!" class="hover-pen">
                <i :class="[IsDisplayReadOnly?'mdi mdi-lock':'fas fa-pencil-alt']"></i>
            </a>
        </span>
    </div>

    <!-- LISTE -->
    <div 
        v-if="propListe"
        ref="user" 
        v-on:mouseout="showTooltip(false,'user',false,IsDisplayReadOnly,dataInput)" 
        v-on:mouseover="showTooltip(true,'user',false,IsDisplayReadOnly,dataInput)"  
        v-bind:class="[
            propListe ? 'listRubriqueRelation' : '', 
            IsDisplayReadOnly ? 'read-only' : '', 
            'ellips input-group hover-input'
        ]"
    >

        <!-- Si le champ multiple et  modifiable -->
        <span
            v-if="dataInput.Multiple && !dataInput.ReadOnly"
            v-on:click="openDial()"  
            class="Modif multiRenderer form-control input-line fname"
        >
            <ul>
                <li 
                    v-for="value in this.valueMultiple" 
                    :key="value.id"
                    v-bind:style="{ background: dataInput.ValueColor, borderColor:dataInput.ValueColor}"
                >
                    <span 
                        class="multiple-user-val"
                    >{{value.value}}</span>
                    <span 
                        :testid="value.id" 
                        @click="removeOneValue($event, value);" 
                        class="multiple_choice_remove fas fa-times-circle cursor_pointeur" 
                        role="presentation"
                    ></span>
                </li>
            </ul>
        </span>

        <!-- Si le champ multiple et pas modifiable-->
        <span 
            v-if="dataInput.Multiple && dataInput.ReadOnly" 
            :id="'ID_' + dataInput.DescId" 
            class="noModif multiRenderer form-control input-line fname"
        >
            <ul>
                <li 
                    v-for="value in this.valueMultiple" 
                    :key="value.id"
                    v-bind:style="{ background: dataInput.ValueColor, borderColor:dataInput.ValueColor}" 
                >
                    {{value.value}}
                    <span 
                        class="multiple_choice_remove fas fa-lock" 
                        role="presentation"
                    ></span>
                </li>
            </ul>
        </span>

        <!-- Si le champ simple et modifiable -->
        <div 
            v-if="!dataInput.ReadOnly && !dataInput.Multiple"
            :class="[isTruncated,'catalogVal']"  
            @mouseleave="stopTransition" 
            ref="catLi"  
            v-bind:style="{ color: dataInput.ValueColor}" 
            type="text" 
            class="modifSimple ellipsDiv form-control input-line fname"
        >
           
            <div 
                v-if="dataInput.DisplayValue != ''" 
                @mouseleave.stop :id="getUniqueId"   
                :field="'field'+dataInput.DescId" 
                class="targetIsTrue"
            >
                <span 
                    v-if="truncatedTxt" 
                    @mouseleave.stop 
                    ref="changeDir" 
                    @mouseover="translateText()" 
                    :class="['left-side',isReversed]"
                ></span>
                <span
                    :ref="'field'+dataInput.DescId" 
                    :class="['catalog-val',isReversed]"
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
        <div 
            v-if="dataInput.ReadOnly && !dataInput.Multiple"
            @mouseleave="stopTransition" 
            v-bind:style="{ color: dataInput.ValueColor}"   
            ref="catLi" 
            type="text" 
            :class="[isTruncated,'catalogVal NoModifSimple ellipsDiv form-control input-line fname testdf']"
        >
            <div 
                v-if="dataInput.DisplayValue != '' " 
                :id="getUniqueId" 
                :style="{ minWidth:35 + 'px'}"  
                class="targetIsTrue"
            >
                <span 
                    v-if="truncatedTxt" 
                    @mouseleave.stop 
                    ref="changeDir"
                    @mouseover="translateText()" 
                    :class="['left-side',isReversed]"
                ></span>
                <span
                    :ref="'field'+dataInput.DescId" 
                    :class="['cat-readonly-val d-flex align-center',isReversed]"
                >{{dataInput.DisplayValue}}</span> 
                <span 
                    class="multiple_choice_remove fas fa-lock" 
                    role="presentation"
                ></span>
            </div> 
        </div>

        <!-- Icon -->
         <span 
            v-on:click="!IsDisplayReadOnly ? openDial() : '' " 
            class="input-group-addon"
        >
            <a 
                href="#!" 
                class="hover-pen"
            >
                <i :class="[IsDisplayReadOnly?'':'fas fa-pencil-alt']"></i>
            </a>
        </span> 
    </div>

</div>
`
};