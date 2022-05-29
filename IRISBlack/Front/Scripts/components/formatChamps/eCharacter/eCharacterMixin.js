import { eFileComponentsMixin } from '../../../mixins/eFileComponentsMixin.js?ver=803000'
import EventBus from '../../../bus/event-bus.js?ver=803000';
import { updateMethod, verifComponent, focusInput, RemoveBorderSuccessError, verifCharacter, showTooltip, hideTooltip, updateListVal, showInformationIco, displayInformationIco } from '../../eComponentsMethods.js?ver=803000';
import { PropType, FieldType } from '../../../Enum.js?ver=803000';
import eAxiosHelper from "../../../helpers/eAxiosHelper.js?ver=803000";
import { eAutoCompletionHelper, tabUrl } from "../../../helpers/eAutoCompletionHelper.js?ver=803000";

/**
 * Mixin commune aux composants eCharacter.
 * */
export const eCharacterMixin = {
    mixins: [eFileComponentsMixin],
    data() {
        return {
            catalogDialog: null,
            PropType,
            FieldType,
            bDisplayPopup: false,
            bEmptyDisplayPopup: false,
            that: this,
            modif: false,
            focusIn: false,
            icon: false,
            bing: false,
            sirene: false,
            dataGouv: false,
            objAutoCompletion: [],
            objAutoCompletionComplement: null,
            openAutoCompletion: false,
            OnUpdateAutocompletion: false,
            triggerPredictiveAddressMapping: this.$root.$children[0].$children[0].DataStruct.PredictiveAddressMapping != null ? this.$root.$children[0].$children[0].DataStruct.PredictiveAddressMapping.Triggers[0] : null,
            triggerSireneMapping:
                this.$root.$children[0].$children[0].DataStruct.SireneMapping != null &&
                    this.$root.$children[0].$children[0].DataStruct.SireneMapping.Triggers != null &&
                    this.$root.$children[0].$children[0].DataStruct.SireneMapping.Triggers.length > 0 ?
                    this.$root.$children[0].$children[0].DataStruct.SireneMapping.Triggers[0] + ";" +
                    (
                        this.$root.$children[0].$children[0].DataStruct.SireneMapping.Triggers.length > 1 ?
                            this.$root.$children[0].$children[0].DataStruct.SireneMapping.Triggers[1] + ";" :
                            ""
                    ) :
                    null,
            provider: this.$root.$children[0].$children[0].DataStruct.PredictiveAddressMapping != null &&
                this.$root.$children[0].$children[0].DataStruct.PredictiveAddressMapping.Triggers[0] == this.dataInput.DescId &&
                this.$root.$children[0].$children[0].DataStruct.PredictiveAddressMapping.ProviderURL == "" ? "bing" :
                this.$root.$children[0].$children[0].DataStruct.PredictiveAddressMapping != null &&
                    this.$root.$children[0].$children[0].DataStruct.PredictiveAddressMapping.Triggers[0] == this.dataInput.DescId &&
                    this.$root.$children[0].$children[0].DataStruct.PredictiveAddressMapping.ProviderURL != "" ? "datagouv" :
                    this.$root.$children[0].$children[0].DataStruct.SireneMapping != null &&
                        (
                            this.$root.$children[0].$children[0].DataStruct.SireneMapping.Triggers[0] == this.dataInput.DescId ||
                            this.$root.$children[0].$children[0].DataStruct.SireneMapping.Triggers[1] == this.dataInput.DescId
                        ) ? 'sirene' : ''



        };
    },
    mounted() {
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
        eAlertBox: () => import(AddUrlTimeStampJS("../../modale/alertBox.js"))
    },
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

        verifChar(event, that) {
            setTimeout(function () {

                if (!that.OnUpdateAutocompletion) {
                    verifComponent(undefined, event, that.dataInput.Value, that, that.dataInput);
                } else {
                    that.dataInput.Value = event.target.value;
                }

                that.openAutoCompletion = false;
                that.OnUpdateAutocompletion = false
            }, 200);



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
                tabUrl[this.provider].params.q = elem.target.value;
                this.$root.$children[0].$children[0].DataStruct.SireneMapping != null ? tabUrl["sirene"].url = this.$root.$children[0].$children[0].DataStruct.SireneMapping.ProviderURL + (this.isInt(elem.target.value) ? '/Search/Etablissement/' + elem.target.value : '/Search/EtablissementEntreprise?value=' + elem.target.value) : ''
                if (elem.target.value == "")
                    return;
                let eAutocompletion = new eAutoCompletionHelper(new eAxiosHelper(tabUrl[this.provider].url), elem, tabUrl[this.provider].params, this.provider, this.dataInput);

                await eAutocompletion.loadAutoCompletion();
                this.objAutoCompletion = eAutocompletion.retrieveJson();

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
                    let findItemData = this.$root.$children[0].$children[0].DataStruct.SireneMapping.Mapping.find(b => b.Source == a.FieldAlias);
                    updateData = this.getUpdateDataAutocompletion(updateData, this, findItemData, a.Value);
                });
            } else {
                if (this.provider == "bing") {
                    tabUrl["BingLocation"].params.q = address.label
                    let eAutocompletion = new eAutoCompletionHelper(new eAxiosHelper(tabUrl["BingLocation"].url), elem, tabUrl["BingLocation"].params, "BingLocation", this.dataInput, address);
                    await eAutocompletion.loadAutoCompletion();
                    this.objAutoCompletionComplement = eAutocompletion.retrieveJson();
                } else {
                    this.objAutoCompletionComplement = address
                }

                for (let [key, value] of Object.entries(this.objAutoCompletionComplement)) {
                    let findItemData = this.$root.$children[0].$children[0].DataStruct.PredictiveAddressMapping.Mapping.find(a => a.Source == key);
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
                let tabDetailItem = context.$root.$children[0].$children[0].tabAllDetail.find(a => a.DescId == mappingItemData.DescId);
                let compoContext;
                compoContext = context.$root.$children.find(app => app.$options.name == "App").$children.find(fiche => fiche.$options.name == "fiche").$children.find(tabsBar => tabsBar.$options.name == "tabsBar").$children.find(fileDetail => fileDetail.$options.name == "fileDetail")
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
            var er = /^-?[0-9]+$/;
            return er.test(value);
        },

    },
    computed: {
        IsDisplayReadOnly: function () {
            return (this.propHead || this.dataInput.ReadOnly)
        }
    },
    watch: {
        'dataInput.Value': function () {
            if (this.$refs['field' + this.dataInput.DescId])
                this.$refs['field' + this.dataInput.DescId].value = this.dataInput.Value
        }
    },
    props: ["dataInput", "propHead", "propListe", "propSignet", "propIndexRow", "propAssistant", "propDetail", "propAssistantNbIndex"],
}