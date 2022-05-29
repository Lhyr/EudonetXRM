import EventBus from '../../bus/event-bus.js?ver=803000';
import { updateMethod, verifComponent, focusInput, RemoveBorderSuccessError, verifCharacter, showTooltip, hideTooltip, updateListVal, showInformationIco, displayInformationIco } from '../../methods/eComponentsMethods.js?ver=803000';
import { PropType, FieldType } from '../../methods/Enum.min.js?ver=803000';
import eAxiosHelper from "../../helpers/eAxiosHelper.js?ver=803000";
import { eAutoCompletionHelper, tabUrl } from "../../helpers/eAutoCompletionHelper.js?ver=803000";
import { eMotherClassMixin } from '../../mixins/eMotherClassMixin.js?ver=803000';
import { eFileComponentsMixin } from '../../mixins/eFileComponentsMixin.js?ver=803000';

export default {
    name: "eAutoComplete",
    data() {
        return {
            emptyAutoCompletion: true,
            catalogDialog: null,
            PropType,
            FieldType,
            bDisplayPopup: false,
            bEmptyDisplayPopup: false,
            that: this,
            modif: false,
            focusIn: false,
            DisplayValue: null,
            icon: false,
            bing: false,
            sirene: false,
            dataGouv: false,
            objAutoCompletion: [],
            objAutoCompletionComplement: null,
            openAutoCompletion: false,
            OnUpdateAutocompletion: false,
            triggerPredictiveAddressMapping: this.$root.$children.find(x => x.$options.name == "App").$refs.file.DataStruct?.PredictiveAddressMapping != null ? this.$root.$children.find(x => x.$options.name == "App").$refs.file.DataStruct?.PredictiveAddressMapping?.Triggers[0] : null,
            triggerSireneMapping:
                this.$root.$children.find(x => x.$options.name == "App").$refs.file?.DataStruct?.SireneMapping != null &&
                    this.$root.$children.find(x => x.$options.name == "App").$refs.file?.DataStruct?.SireneMapping?.Triggers != null &&
                    this.$root.$children.find(x => x.$options.name == "App").$refs.file?.DataStruct?.SireneMapping?.Triggers.length > 0 ?
                    this.$root.$children.find(x => x.$options.name == "App").$refs.file?.DataStruct?.SireneMapping?.Triggers[0] + ";" +
                    (
                        this.$root.$children.find(x => x.$options.name == "App").$refs.file?.DataStruct?.SireneMapping?.Triggers?.length > 1 ?
                            this.$root.$children.find(x => x.$options.name == "App").$refs.file?.DataStruct?.SireneMapping?.Triggers[1] + ";" :
                            ""
                    ) :
                    null,
            provider: this.$root.$children.find(x => x.$options.name == "App").$refs.file?.DataStruct?.PredictiveAddressMapping != null &&
                this.$root.$children.find(x => x.$options.name == "App").$refs.file?.DataStruct?.PredictiveAddressMapping?.Triggers[0] == this.dataInput.DescId &&
                this.$root.$children.find(x => x.$options.name == "App").$refs.file?.DataStruct?.PredictiveAddressMapping?.ProviderURL == "" ? "bing" :
                this.$root.$children.find(x => x.$options.name == "App").$refs.file?.DataStruct?.PredictiveAddressMapping != null &&
                    this.$root.$children.find(x => x.$options.name == "App").$refs.file?.DataStruct?.PredictiveAddressMapping?.Triggers[0] == this.dataInput.DescId &&
                    this.$root.$children.find(x => x.$options.name == "App").$refs.file?.DataStruct?.PredictiveAddressMapping?.ProviderURL != "" ? "datagouv" :
                    this.$root.$children.find(x => x.$options.name == "App").$refs.file?.DataStruct?.SireneMapping != null &&
                        (
                            this.$root.$children.find(x => x.$options.name == "App").$refs.file?.DataStruct?.SireneMapping?.Triggers[0] == this.dataInput.DescId ||
                            this.$root.$children.find(x => x.$options.name == "App").$refs.file?.DataStruct?.SireneMapping?.Triggers[1] == this.dataInput.DescId
                        ) ? 'sirene' : ''



        };
    },
    mounted() {

        this.setContextMenu();
        this.displayInformationIco();

        if (this.$refs['field' + this.dataInput.DescId]) {
            this.$refs['field' + this.dataInput.DescId].value = this.dataInput.Value;
        }
        Vue.nextTick(() => {
            if (this.dataInput.Required && this.dataInput.Value === "")
                verifCharacter(this.dataInput, this.that)
        });
    },
    components: {
        eAlertBox: () => import(AddUrlTimeStampJS("../modale/alertBox.js")),
        eCharacterFile: () => import(AddUrlTimeStampJS("./eCharacter/eCharacterFile.js")),
        eCharacterFileBing: () => import(AddUrlTimeStampJS("./eCharacter/eCharacterFileBing.js")),
        eCharacterFileSirene: () => import(AddUrlTimeStampJS("./eCharacter/eCharacterFileSirene.js")),
        eCharacterList: () => import(AddUrlTimeStampJS("./eCharacter/eCharacterList.js")),
    },
    mixins: [eMotherClassMixin, eFileComponentsMixin],
    methods: {
        showInformationIco,
        displayInformationIco,
        updateMethod,
        eAxiosHelper,
        eAutoCompletionHelper,
        showTooltip,
        hideTooltip,
        focusInput,
        RemoveBorderSuccessError,
        updateListVal,
        verifComponent,
        verifCharacter,

        onUpdateCallback() {
            let options = {
                reloadSignet: false,
                reloadHead: false,
                reloadAssistant: false,
                reloadAll: true
            }
            if (this.dataInput.IsInRules)
                EventBus.$emit('emitLoadAll', options);
        },
       
        goAction(event) {
            if (this.dataInput?.AliasSourceField?.Format == 3 && !IsDisplayReadOnly) {
                var objParentInfo = { parentTab: this.getTab, parentFileId: this.getFileId }
                selectFileMail(getParamWindow().document.getElementById("MLFiles"), this.dataInput.Value, objParentInfo, TypeMailing.MAILING_UNDEFINED);
            }            
        },

        verifChar(event, that) {
            /*ELAIZ - demande 82 321 - Vérification de l'appartenance du composant ( signet/fiche) afin de ne pas tomber
             * dans le setTimeout car il remet l'ancienne valeur */
            if (this.propListe) {
                verifComponent(undefined, event, that.dataInput.Value, that, that.dataInput);
            }
            else {
                setTimeout(function () {
                    if (!that.OnUpdateAutocompletion) {
                        verifComponent(undefined, event, that.dataInput.Value, that, that.dataInput);
                    } else {

                        that.dataInput.Value = event.target.value;
                    }
                    that.openAutoCompletion = false;
                    that.OnUpdateAutocompletion = false
                }, 200);
            }
        },

        verifyValChanged(event) {
            if (event.target.classList.contains('changed')) {
                this.dataInput.Value = event.target.value;
                event.target.classList.remove('changed');
            }
        },


        /**
         * /
         * @param {any} elem // Input de recherche
         */
        async getDataAutocompletion(elem) {
            if (this.timer) {
                window.clearTimeout(this.timer);
            }
            this.timer = window.setTimeout(async () => {

                if (!this.provider)
                    return;

                tabUrl[this.provider].params.q = elem.target.value;
                this.$root.$children.find(x => x.$options.name == "App").$refs.file?.DataStruct?.SireneMapping != null ? tabUrl["sirene"].url = this.$root.$children.find(x => x.$options.name == "App").$refs.file?.DataStruct?.SireneMapping.ProviderURL + (this.isInt(elem.target.value) ? '/Search/Etablissement/' + elem.target.value.replace(/ /g, "") : '/Search/EtablissementEntreprise?value=' + elem.target.value) : ''
                if (elem.target.value == "")
                    return;
                let eAutocompletion = new eAutoCompletionHelper(new eAxiosHelper(tabUrl[this.provider].url), elem, tabUrl[this.provider].params, this.provider, this.dataInput);

                await eAutocompletion.loadAutoCompletion();
                this.objAutoCompletion = eAutocompletion.retrieveJson();

                if (this.objAutoCompletion && this.objAutoCompletion.length > 0) {
                    this.emptyAutoCompletion = false;
                } else {
                    this.emptyAutoCompletion = true;
                }

            }, 300);
        },

        /**
         * /
         * @param {any} address // object address cliquer
         * @param {any} elem // element cliquer (<li>)
         */
        async setValueAutocompletion(address, elem) {
            this.OnUpdateAutocompletion = true;
            this.openAutoCompletion = true;
            let updateData = null;

            if (address.Fields) {
                address.Fields.forEach(a => {
                    let findItemData = this.$root.$children.find(x => x.$options.name == "App").$refs.file?.DataStruct?.SireneMapping.Mapping.find(b => b.Source == a.FieldAlias);
                    updateData = this.getUpdateDataAutocompletion(updateData, this, findItemData, a.Value);
                });
            } else {

                if (!this.provider)
                    return;

                if (this.provider == "bing") {
                    tabUrl["BingLocation"].params.q = address.label
                    let eAutocompletion = new eAutoCompletionHelper(new eAxiosHelper(tabUrl["BingLocation"].url), elem, tabUrl["BingLocation"].params, "BingLocation", this.dataInput, address);
                    await eAutocompletion.loadAutoCompletion();
                    this.objAutoCompletionComplement = eAutocompletion.retrieveJson();
                } else {
                    this.objAutoCompletionComplement = address
                }

                for (let [key, value] of Object.entries(this.objAutoCompletionComplement)) {
                    let findItemData = this.$root.$children.find(x => x.$options.name == "App").$refs.file?.DataStruct?.PredictiveAddressMapping.Mapping.find(a => a.Source == key);
                    updateData = this.getUpdateDataAutocompletion(updateData, this, findItemData, value);
                }
            }

            if (updateData && updateData.ctx && updateData.additionalData && updateData.dataInput) {
                let triggerFieldNewValue = null;
                let triggerFieldOldValue = null;
                if (updateData.additionalData.Fields && updateData.additionalData.Fields.length > 0) {
                    triggerFieldNewValue = updateData.additionalData.Fields[0].NewValue;
                    triggerFieldOldValue = updateData.additionalData.Fields[0].OldValue;
                }
                updateMethod(updateData.ctx, triggerFieldNewValue, triggerFieldOldValue, updateData.additionalData, updateData.dataInput);
            }
            this.objAutoCompletion = [];
        },

        getUpdateDataAutocompletion(updateData, context, mappingItemData, value) {
            // Première initialisation de l'objet de retour (sinon, MAJ de celui passé en paramètre)
            if (!updateData) {
                updateData = {};
                updateData.ctx = null;
                updateData.dataInput = context.dataInput;
                updateData.additionalData = {
                    'Fields': []
                };
            }

            if (mappingItemData && mappingItemData.DescId != 0) {
                // Récupération des informations actuelles du champ à mettre à jour
                //let tabDetailItem = context.$root.$children[0].$children[0].tabAllDetail.find(a => a.DescId == mappingItemData.DescId);


                let oRootFile = context?.$root?.$children;

                let app = oRootFile?.find(f => f.$options.name == 'App')?.$children;

                if (app)
                    oRootFile = app;

                let vap = oRootFile?.find(vapp => vapp.$options.name == "v-app")?.$children;

                if (vap)
                    oRootFile = vap;

                let vmain = oRootFile?.find(vapp => vapp.$options.name == "v-main")?.$children;

                if (vmain)
                    oRootFile = vmain;

                let fiche = oRootFile?.find(vapp => vapp.$options.name == "fiche");

                if (fiche)
                    oRootFile = fiche;

                let tabDetailItem = oRootFile?.tabAllDetail.find(a => a.DescId == mappingItemData.DescId);

                let compoContext;
                compoContext = oRootFile.$children.find(tabsBar => tabsBar.$options.name == "tabsBar").$children.find(fileDetail => fileDetail.$options.name == "fileDetail")
                // Préparation de l'objet de MAJ à destination d'updateMethod > eEngine, et MAJ de l'affichage ensuite
                compoContext.$refs.fields.find(c => {
                    let itemComponent = compoContext.$refs.fields.find(a => a.$refs['field' + mappingItemData.DescId]);
                    if (!updateData.ctx && itemComponent)
                        updateData.ctx = itemComponent.that;

                    // ---------------------------------------------------------------------------------------------
                    let updFieldData = {};

                    // Copie sélective des propriétés du champ (tabDetailItem) dans updFieldData
                    // On ne peut pas faire cette copie en une fois, car ce ne sont pas les mêmes objets représentés
                    // tabDetailItem étant un EudoQuery.Field, et updFieldData étant un eUpdateField

                    // Propriétés non câblées (pour l'instant)
                    //updFieldData.ForceUpdate = false;
                    //updFieldData.ReadOnly = false;
                    //updFieldData.ChangedValue = null;
                    //updFieldData.IsB64 = false;
                    //updFieldData.BoundPopup = tabDetailItem.BoundFieldPopup; // TOCHECK
                    //updFieldData.BoundValue = tabDetailItem.BoundFieldPopup; // TOCHECK

                    // Propriétés spécifiques aux catalogues ou aux champs relationnels
                    updFieldData.Multiple = tabDetailItem.Multiple;
                    updFieldData.SetPopupDescId = tabDetailItem.PopupDescId;
                    updFieldData.SetPopup = tabDetailItem.PopupType;
                    updFieldData.Popup = tabDetailItem.PopupType; // BUG 86 435
                    updFieldData.SetBoundDescId = tabDetailItem.BoundDescId;
                    updFieldData.SetIsTreeView = tabDetailItem.IsTree;

                    // Propriétés passées pour tous les types de champs
                    updFieldData.AutoComplete = true; // route Engine vers StrategyCruAutoCompletionXrm au lieu de StrategyCruXrm par défaut
                    updFieldData.Descid = tabDetailItem.DescId;
                    updFieldData.Format = tabDetailItem.Format;
                    // Propriétés concernant les valeurs à mettre à jour
                    let oldValue = tabDetailItem.Value;
                    let oldDisplayValue = tabDetailItem.DisplayValue;
                    // Cas de la MAJ Catalogue
                    // On ne connaît pas le DataID de la valeur insérée dans le catalogue.
                    // On laissera donc le contrôleur back se charger de la MAJ à partir de la seule valeur textuelle à insérer dans le catalogue, à partir de laquelle il recherchera
                    // le DataID correspondant (ou le créera si nécessaire)
                    if (tabDetailItem.Format == FieldType.Catalog) {
                        updFieldData.OldValue = oldValue;
                        updFieldData.OldDisplay = oldDisplayValue;
                        updFieldData.NewValue = -1;
                        updFieldData.NewDisplay = value;
                        updFieldData.AddValueInCatalog = mappingItemData.CreateCatalogValue; // Autoriser ou non l'ajout de la valeur dans le catalogue si inexistante
                    }
                    // Autres cas
                    else {
                        updFieldData.OldValue = oldValue;
                        updFieldData.OldDisplay = oldDisplayValue;
                        updFieldData.NewValue = value;
                        updFieldData.NewDisplay = value;
                    }

                    // Ajout des infos concernant le champ à mettre à jour
                    // Si le champ a déjà été ajouté dans la liste (erreur de mapping ou autre raison), on met à jour le mapping précédemment ajouté
                    let existingFieldIndex = updateData.additionalData.Fields.findIndex(f => f.Descid === updFieldData.Descid);
                    if (existingFieldIndex >= 0)
                        updateData.additionalData.Fields[existingFieldIndex] = updFieldData;
                    else
                        updateData.additionalData.Fields.push(updFieldData);
                    // ---------------------------------------------------------------------------------------------

                });

                // Mise à jour de l'affichage du champ, pour feedback visuel avant envoi en base
                // En cas d'erreur, la valeur d'origine sera restaurée via cancelUpdate()
                if (tabDetailItem.Format == FieldType.Catalog) {
                    tabDetailItem.DisplayValue = value;
                } else {
                    tabDetailItem.Value = value;
                }
            }
            return updateData;
        },

        isInt(value) {
            var er = /^-?[0-9, / /]+$/;
            return er.test(value);
        },
        adjustInputSize() {
            event.target.parentNode.dataset.value = event.target.value;
            
        },
        focusEvt(event) {
            this.openAutoCompletion = true;
            this.dataInput.AutoComplete && ((this.triggerSireneMapping != null && this.triggerSireneMapping.indexOf(this.dataInput.DescId + ';') != -1) || (this.triggerPredictiveAddressMapping == this.dataInput.DescId)) ? this.getDataAutocompletion(event) : '';
            this.bDisplayPopup = false;
            this.RemoveBorderSuccessError();
            this.bEmptyDisplayPopup = false;
            this.focusIn = true;
            this.verifyValChanged(event);
        },
        keyUpEvt(event) {
            this.dataInput.AutoComplete && ((triggerSireneMapping != null && triggerSireneMapping.indexOf(this.dataInput.DescId + ';') != -1) || (triggerPredictiveAddressMapping == this.dataInput.DescId)) ? this.getDataAutocompletion(event) : ''
        }

      

    },
    computed: {
        IsDisplayReadOnly: function () {
            return (this.dataInput.ReadOnly)
        },
        characterFieldCssClass() {
            return this.propHead ? 'class_liste_rubrique_caractere_header_' + this.dataInput.DescId : this.propDetail ? 'class_liste_rubrique_caractere_detail_' + this.dataInput.DescId : this.propAssistant ? 'class_liste_rubrique_caractere_assistant_' + this.propAssistantNbIndex + '_' + this.dataInput.DescId : '';
        }
    },
    watch: {
        'dataInput.Value': function () {
            if (this.$refs['field' + this.dataInput.DescId])
                this.$refs['field' + this.dataInput.DescId].value = this.dataInput.Value
        }
    },
    props: ["dataInput", "propHead", "propListe", "propSignet", "propIndexRow", "propAssistant", "propDetail", "propAssistantNbIndex"],
    template: `


<div class="globalDivComponent">

    <template v-if="false">
        <eCharacterFile />
    </template>
    <template v-else>
	<!-- FICHE -->
	<div ref="character" v-on:mouseout="showTooltip(false,'character',icon,IsDisplayReadOnly,dataInput)" v-on:mouseover="showTooltip(true,'character',icon,IsDisplayReadOnly,dataInput)" v-if="!propListe" v-bind:class="[IsDisplayReadOnly?'read-only':'', focusIn ? 'focusIn' : '' , 'ellips input-group hover-input']">
   
        <!-- <span ref="info" v-if="showInformationIco()" class="icon-info-circle info-tooltip"></span>  -->

        <!-- Si le champ eCharactere est modifiable et dans la zone résumé -->
        <label :data-value="dataInput.Value" v-on:blur="focusIn = false" v-if="propHead && !dataInput.ReadOnly" class="input-sizer">
            <input spellcheck="false" autocomplete="off" type="text" @input="adjustInputSize()" size="4" 
                :value="dataInput.DisplayValue ? dataInput.DisplayValue:dataInput.Value" 
                @keyup="keyUpEvt($event)" :ref="'field'+dataInput.DescId" :field="'field'+dataInput.DescId" 
                v-on:focus="focusEvt($event)" 
		        v-on:blur="verifChar($event,that);" :IsDetail="propDetail" :IsAssistant="propAssistant" :IsHead="propHead"
                v-on:mouseover="verifyValChanged($event)" :IsList="propListe" 
                :class="[characterFieldCssClass,'form-control input-line fname']" :style="{ color: dataInput.ValueColor}" 
			  :placeholder="dataInput.Watermark">
        </label>

		<!-- Si le champ eCharactere est modifiable -->
		<input spellcheck="false" autocomplete="off" :ref="" v-on:keyup="dataInput.AutoComplete && ((triggerSireneMapping != null && triggerSireneMapping.indexOf(dataInput.DescId + ';') != -1) || (triggerPredictiveAddressMapping ==  dataInput.DescId)) ? getDataAutocompletion($event) : ''" :ref="'field'+dataInput.DescId" :field="'field'+dataInput.DescId" 
        v-on:focus="openAutoCompletion = true; dataInput.AutoComplete && ((triggerSireneMapping != null && triggerSireneMapping.indexOf(dataInput.DescId + ';') != -1) || (triggerPredictiveAddressMapping ==  dataInput.DescId)) ? getDataAutocompletion($event) : ''; bDisplayPopup = false; RemoveBorderSuccessError();bEmptyDisplayPopup = false; focusIn = true;verifyValChanged($event)" 
		v-on:blur="verifChar($event,that); focusIn = false" :IsDetail="propDetail" :IsAssistant="propAssistant" :IsHead="propHead"
        v-on:mouseover="verifyValChanged($event)"
			:IsList="propListe" v-bind:class="[propHead ? 'class_liste_rubrique_caractere_header_' + dataInput.DescId  : '', propDetail ? 'class_liste_rubrique_caractere_detail_' + dataInput.DescId  : '', propAssistant ? 'class_liste_rubrique_caractere_assistant_' + propAssistantNbIndex + '_' + dataInput.DescId  : '', 'form-control input-line fname']" v-bind:style="{ color: dataInput.ValueColor}" v-if="!(dataInput.ReadOnly) && !propHead" 
			 type="text" :placeholder="dataInput.Watermark">

		<!-- Si le champ eCharactere n'est pas modifiable -->
		<span v-bind:style="{ color: dataInput.ValueColor}" v-if="dataInput.ReadOnly" class="readOnly">{{dataInput.Value}}</span>

		<!-- Icon -->
		<span v-on:click="focusInput('caractere', {props: propAssistant ? PropType.Assistant : propDetail ? PropType.Detail : propListe ? PropType.Liste : propHead ? PropType.Head : PropType.Defaut,
		propAssistantNbIndex: propAssistantNbIndex,
		propIndexRow: propIndexRow,
		dataInput: dataInput,
		propSignet: propSignet
	}); goAction($event)"   class="input-group-addon"><a  href="#!" class="hover-pen"><i :class="[IsDisplayReadOnly?'mdi mdi-lock':'fas fa-pencil-alt']"></i></a></span>

		
		<!-- Message d'erreur après la saisie dans le champs -->
		<eAlertBox v-if="this.bEmptyDisplayPopup && !(dataInput.ReadOnly)" >
			<p>{{getRes(2471)}}</p>
		</eAlertBox>
	</div>
    </template>
    <template v-if="false">
        <eCharacterFileBing />
    </template>
    <template v-else>
<div>
    <div class="contentAutoComplete" v-if="emptyAutoCompletion && openAutoCompletion">
        <ul>
            <li>
                <a class="suggestLink">
                    <div class="as_suggestion_root_inside empytAutoCompletion">
                        <div class="as_lines_root">
                            <span>{{getRes(2807)}}</span>
                        </div>
                    </div>
                </a>
            </li>
        </ul>
    </div>
    <div :class="[provider == 'bing' ? 'bingAutoComplete' : provider == 'datagouv' ? 'dataGouvAutoComplete' : '', 'contentAutoComplete']" v-if="(provider == 'datagouv' || provider == 'bing') && objAutoCompletion && objAutoCompletion.length > 0 && openAutoCompletion" >
        <ul>
            <li @click="setValueAutocompletion(address, $event);  OnUpdateAutocompletion = true" v-for="address in objAutoCompletion">
                <a class="suggestLink">
                    <div class="as_suggestion_root_inside">
                        <div class="as_img_maps_address"><i class="fas fa-map-marker-alt"></i></div>
                        <div class="as_lines_root">
                            <p v-if="address.label" class="autoComplete_line1">{{address.label ? address.label : ''}}</p>
                            <p v-if="(address.postalCode || address.city || address.country) && provider == 'bing' " class="autoComplete_line2">{{address.postalCode ? address.postalCode : ''}} {{address.city ? address.city : '' }} {{ address.country ? address.country : ''}}</p>
                            <p v-if="(address.region || address.department || address.country) && provider == 'datagouv' " class="autoComplete_line2">{{address.department ? address.department : '' }} {{ address.region ? '(' + address.region + ')' : ''}} {{ address.country ? address.country : ''}}</p>
                        </div>
                    </div>
                </a>
            </li>
        </ul>
    </div>
</div>
    </template>
    <template v-if="false">
        <eCharacterFileSirene />
    </template>
    <template v-else>
    <div>
    <div class="contentAutoComplete" v-if="emptyAutoCompletion && openAutoCompletion">
        <ul>
            <li>
                <a class="suggestLink">
                    <div class="as_suggestion_root_inside empytAutoCompletion">
                        <div class="as_lines_root">
                            <span>{{getRes(2807)}}</span>
                        </div>
                    </div>
                </a>
            </li>
        </ul>
    </div>
    <div class="sireneAutoComplete contentAutoComplete" v-if="provider == 'sirene' && objAutoCompletion && objAutoCompletion.length > 0 && openAutoCompletion" >
        <ul>
            <li @click="setValueAutocompletion(address, $event); OnUpdateAutocompletion = true" v-for="address in objAutoCompletion">
                <a class="suggestLink">
                    <div class="as_suggestion_root_inside">
                        <div class="as_img_maps_address"><i class="fas fa-building"></i></div>
                        <div class="as_lines_root">
                            <span v-bind:class="address.Fields.find(a => a.FieldAlias == 'DFERMETUREET').Value == '' ? 'openEtab' : 'closeEtab'">{{address.Fields.find(a => a.FieldAlias == 'DFERMETUREET').Value != '' ? getRes(8558) + ' ' + address.Fields.find(a => a.FieldAlias == 'DFERMETUREET').Value : ''}}</span>
                            <p class="autoComplete_line1">{{address.Fields.find(a => a.FieldAlias == "L1_NORMALISEE").Value}}<span class="sirenFront"> - {{address.Fields.find(a => a.FieldAlias == "SIREN").Value}}</span></p>
                            <p class="autoComplete_line2">{{address.Fields.find(a => a.FieldAlias == "L4_NORMALISEE").Value}} {{address.Fields.find(a => a.FieldAlias == "L6_DECLAREE").Value}} {{address.Fields.find(a => a.FieldAlias == "L7_NORMALISEE").Value != "" ? "(" + address.Fields.find(a => a.FieldAlias == "L7_NORMALISEE").Value + ")" : ""}}</p>
                            <p class="autoComplete_line2"><span>{{address.Fields.find(a => a.FieldAlias == "SIEGE").Value == "0" ? getRes(8557) : getRes(8556)}}</span> - <span class="etabType">{{address.Fields.find(a => a.FieldAlias == "LIBAPET").Value}}</span> </p>         
                        </div>
                    </div>
                </a>
            </li>
        </ul>
    </div>
</div>
    </template>
    <template v-if="false">
        <eCharacterList />
    </template>
    <template v-else>
	<!-- LISTE -->
	<div ref="character" v-on:mouseout="showTooltip(false,'character',icon,IsDisplayReadOnly,dataInput)" v-on:mouseover="showTooltip(true,'character',icon,IsDisplayReadOnly,dataInput)" v-if="propListe" v-bind:class="[IsDisplayReadOnly ? 'read-only' : '',  propListe ? 'listRubriqueCaractere' : '', 'ellips input-group hover-input',focusIn ? 'focusIn' : '']">

		<!-- Si le champ eCharactere est modifiable -->
		<input spellcheck="false" :ref="'field'+dataInput.DescId" :field="'field'+dataInput.DescId" :IsDetail="propDetail" :IsAssistant="propAssistant" :IsHead="propHead" :IsList="propListe" v-if="!dataInput.ReadOnly" 
			v-on:blur="verifChar($event,that);focusIn = false" @focus="focusIn = true" :class="'class_liste_rubrique_caractere_' + propSignet.DescId + '_' + propIndexRow + '_' + dataInput.DescId" v-bind:disabled="dataInput.ReadOnly"
			v-on:click="this.bDisplayPopup = false" v-bind:style="{ color: dataInput.ValueColor}" type="text" class="form-control input-line fname"
			:placeholder="dataInput.Watermark">
	   
		<!-- Si le champ eCharactere est pas modifiable --> 
		<div v-bind:style="{ color: dataInput.ValueColor}" v-if="dataInput.ReadOnly" class="NoModifSimple ellipsDiv form-control input-line fname">
			<div class="targetIsTrue">{{dataInput.DisplayValue ? dataInput.DisplayValue : dataInput.Value }}</div> 
		</div>


<!-- Icon -->
		<span v-on:click="!IsDisplayReadOnly ? focusIn = true : focusIn = false , !IsDisplayReadOnly ? focusInput('caractere', {
		props: propAssistant ? PropType.Assistant : propDetail ? PropType.Detail : propListe ? PropType.Liste : PropType.Defaut,
		propAssistantNbIndex: propAssistantNbIndex,
		propIndexRow: propIndexRow,
		dataInput: dataInput,
		propSignet: propSignet
	}) : '' " class="input-group-addon"><a  href="#!" class="hover-pen"><i :class="[IsDisplayReadOnly?'mdi mdi-lock':'fas fa-pencil-alt']"></i></a></span>

	</div>
    </template>
</div>
`
};