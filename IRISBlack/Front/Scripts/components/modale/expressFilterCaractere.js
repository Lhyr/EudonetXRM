import { FieldType, Operator, EdnType, TableType } from '../../methods/Enum.min.js?ver=803000';
import { eExpressFilterMixin } from '../../mixins/eExpressFilterMixin.js?ver=803000';
import { showCatalogGeneric, openUserDialog } from '../../methods/eComponentsMethods.js?ver=803000';
import { LoadCatalog } from '../../methods/eFileMethods.js?ver=803000';
import { selectValue, cancelAdvancedDialog, validateUserDialog, validateCatGenericIris, cancelCatGenericIris, adjustColWidth, NewWidthCol, EnlargeColsIfNeeded, adjustColsWidth } from '../../methods/eFieldEditorMethods.js?ver=803000';

export default {
    name: "expressFilterCaractere",
    mounted() {
    },
    data() {
        return {
            top: 0,
            timeOut: null,
            DataCatalogue: null,
            selectedValues: null,
            selectedLabels: null,
            advancedDialog: null
        };
    },
    mixins: [eExpressFilterMixin],
    components: {
        expressFilterFooter: () => import(AddUrlTimeStampJS("./expressFilterFooter.js")),
        expressFilterSearch: () => import(AddUrlTimeStampJS("./expressFilterSearch.js"))
    },
    computed:
    {
        //pour l'instant ce sont les mêmes mais cela pourrait changer
        isSearch: function () {
            return true;
        },
        isStatistic: function () {
            return this.propData.datas.Format == FieldType.Catalog
                || this.propData.datas.Format == FieldType.User
                || this.propData.datas.Format == FieldType.Relation
                || this.propData.datas.Format == FieldType.AliasRelation;
        },
        isAdvancedSelection: function () {
            return this.propData.datas.Format == FieldType.Catalog
                || this.propData.datas.Format == FieldType.User
                || this.propData.datas.Format == FieldType.Relation;
        },
        isMailStatusSelection: function () {
            return this.propData.datas.Format == FieldType.MailAddress;
        }
    },
    methods: {
        selectValue,
        LoadCatalog,
        showCatalogGeneric,
        openUserDialog,
        cancelAdvancedDialog,
        adjustColWidth,
        NewWidthCol,
        EnlargeColsIfNeeded,
        adjustColsWidth,
        validateUserDialog,
        validateCatGenericIris,
        cancelCatGenericIris,
        /**
         * Sélection avancée.
         * Affiche un catalogue. Dans le même genre que pour les components.
         * Donc, le même...
         * */
        advancedSelection: async function () {
            let defautValue = ["", ""];

            if (this.propData.datas.ExpressFilterActived) {
                let tabValue = /\#\$\|\#\$/i[Symbol.split](this.propData.datas.ExpressFilterActived.Value);

                if (defautValue.length > 2)
                    defautValue = tabValue;
            }

            /** Si c'est un catalogue de type utilisateur. */
            if (this.propData.datas.Format == FieldType.User) {

                this.openUserDialog(
                    this.propData.datas.DescId,
                    this.propData.datas.Label,
                    1,
                    1,
                    this.cancelAdvancedDialog,
                    () => {
                        this.validateUserDialog(() => this.updatePref(Operator.OP_IN_LIST + ";|;" + this.selectedValues.join(';')));
                    },
                    defautValue[0],
                    defautValue[1]
                );

                return;
            }


            /** Si c'est un catalogue d'un autre type. */
            this.showCatalogGeneric(
                this.propData.datas.EAction,
                this.propData.datas.DataDescT,
                this.propData.datas.DataEnumT,
                true,                                // this.propData.datas.Multiple, //KHA pour la selection avancée on est forcément sur du multiple cf eExpressFilter.js > méthode openCatalog 
                this.propData.datas.IsTree,
                defautValue[0],
                "",
                "",
                this.propData.datas.PopupDescId,
                this.propData.datas.PopupType,
                this.propData.datas.BoundDescId,
                this.propData.datas.BoundFieldPopup,
                this.propData.datas.Pdbv,
                this.propData.datas.Label,
                "eCatalogEditorObject",
                false,
                () => {
                    this.validateCatGenericIris(() => this.updatePref(Operator.OP_IN_LIST + ";|;" + this.selectedValues.join(';')));
                },
                this.cancelCatGenericIris,
                LOADCATFROM.EXPRESSFILTER
            );

        },


        /**
         * Choix d un status de mail
         * Affiche un catalogue de type enum
         * */
        mailStatusSelection: async function () {
            let defautValue = ["", ""];

            if (this.propData.datas.ExpressFilterActived) {
                let tabValue = /\#\$\|\#\$/i[Symbol.split](this.propData.datas.ExpressFilterActived.Value);

                if (defautValue.length > 2)
                    defautValue = tabValue;
            }
            this.showCatalogGeneric(
                "LNKCATENUM",
                0,
                7,
                true,
                false,
                defautValue[0],
                "",
                "",
                this.propData.datas.DescId,
                POPUPTYPE.ENUM,
                0,
                0,
                "",
                this.propData.datas.Label,
                "eCatalogEditorObject",
                false,
                () => {
                    this.validateCatGenericIris(() => this.updatePref(42 + ";|;" + this.selectedValues.join(';')));
                },
                this.cancelCatGenericIris,
                LOADCATFROM.EXPRESSFILTER
            );

        },

        /**
     * Retourne une res concaténée de la valeur recherchée.
     * @param {string} numRes la res que l'on souhaite.
     * @returns {string} la valeur concaténée
     */
        GetResValue: function (numRes) {
            return numRes.concat(' ', this.SearchValue);
        },
        /**
        * Filtre contient 
        * */
        ContainsFilter: function () {
            this.updatePref(Operator.OP_CONTAIN + ";|;" + this.SearchValue);
        },
        /**
         * Filtre débute par
         * */
        BeginWithFilter: function () {
            this.updatePref(Operator.OP_START_WITH + ";|;" + this.SearchValue);
        },
        /**
         * Récupère la bonne valeur a envoyer au back quand on filtre depuis les mrus
         * Pour 01 (Nom, Raison Sociale...), prendre que le nom et non la concaténation du nom et du prénom
         * @param {object} dtMru élément sélectionné dans les mrus
         * @returns {string} la valeur de l'élément sélectionné
         */
        getMruSearchVal: function (dtMru) {
            let concatenatedName = this.getTabDescid(dtMru.DescId) == TableType?.PP && dtMru?.DescId?.toString()?.endsWith('01');
            if (this.isAdvancedSelection) {
                return dtMru?.Value;
            } else if (concatenatedName) {
                return this.getMruValue(dtMru)?.split(' ')?.find(value => value);
            }
            return this.getMruValue(dtMru);
        },
        /**
         * Renvoie la displayValue sinon la Value
         * @param {object} dtMru 
         * @returns 
         */
        getMruValue(dtMru) {
            return dtMru?.DisplayValue || dtMru?.Value;
        }
    },
    template: `

 <ul :format="this.propData.datas.Format" class="filterContent" v-bind:style="{ top: this.top, left: this.propData.posLeft + 'px' }">
    <expressFilterSearch v-if="isSearch" />
    <li v-if="SearchValue == ''" class="data-mru">
        <ul class="xprss-filter__list">
            <li class="actionItem xprss-filter__li" 
                :title="getMruValue(dtMru)" 
                v-for="dtMru in this.DataMRU"
                @click="searchFromValue(getMruSearchVal(dtMru))">{{getMruValue(dtMru)}}
            </li>
        </ul>
    </li>
 
    <li v-else class="data-mru">
            <ul class="xprss-filter__list">
                <!-- 2009 -   contient -->
                <li v-if="!isAdvancedSelection" class="actionItem xprss-filter__li" :title="GetResValue(getRes(2009))">
                    <span @click="ContainsFilter">{{ GetResValue(getRes(2009)) }}</span></li>
                <!-- 2006 -   débute par -->
                <li class="actionItem" :title="GetResValue(getRes(2006))">
                    <span @click="BeginWithFilter">{{ GetResValue(getRes(2006)) }}</span></li>               
            </ul>
        </li>


    <li v-if="isAdvancedSelection" class="filter-adv-selection">
        <ul class="xprss-filter__list"> 
            <li class="xprss-filter__li" v-on:click="advancedSelection"><span :title="getRes(6221)"><i class="icon-search"></i></span>{{getRes(6221)}}</li>
        </ul>
    </li>

    <li v-if="isMailStatusSelection" class="filter-adv-selection">
        <ul class="xprss-filter__list"> 
            <li class="xprss-filter__li" v-on:click="mailStatusSelection"><span :title="getRes(2880)"><i class="icon-search"></i></span>{{getRes(2880)}}</li>
        </ul>
    </li>

    <li v-if="isStatistic" class="filter-stats">
        <ul class="xprss-filter__list"> 
            <li class="xprss-filter__li" v-on:click="doLogicChart()"><span :title="getRes(1395)"><i class="icon-stats"></i></span>{{getRes(1395)}}</li>
        </ul>
    </li>   
   
    <expressFilterFooter :prop-data="propData" />
</ul>
`,
};