import { FieldType, Operator } from '../methods/Enum.min.js?ver=803000';
import { getTabDescid } from '../methods/eMainMethods.js?ver=803000'
import { eMotherClassMixin } from './eMotherClassMixin.js?ver=803000';
import EventBus from "../bus/event-bus.js?ver=803000";


/**
 * Mixin commune à tous les express filters.
 * */
export const eExpressFilterMixin = {
    data() {
        return {
            SearchValue: "",
            res: top,
            updPref: "tab=" + this.getTab + ";$;bkm=" + this.propData.propSignet.DescId + ";$;filterExpress=" + this.propData.datas.DescId + ";|;",
        }
    },
    mixins: [eMotherClassMixin],
    props: ["cssClass", "propData", "filterTab"],
    components: {
        expressFilterSearch: () => import(AddUrlTimeStampJS("../components/modale/expressFilterSearch.js")),
        expressFilterFooter: () => import(AddUrlTimeStampJS("../components/modale/ExpressFilterFooter.js"))
    },
    created() {
        let h = this.propData.maxHeight + 27 * [this.DataMRU.length > 4 ? 4 : this.DataMRU.length]
        if (h + this.propData.posTop > window.innerHeight) {
            this.top = (this.propData.posTop - h) + 'px'
        } else {
            this.top = this.propData.posTop + 'px'
        }
    },
    computed: {
        /**
         * Permet de récupérer les données affichées dans une colonnne.
         * @returns {array} un tableau unique des données les plus utilisées.
         * */
        DataMRU: function () {
            return this.propData.fullDatas.Data
                .flatMap(a => a.LstDataFields)
                .filter((value, index, arr) =>
                    value.DescId == this.propData.datas.DescId
                    && index == arr.findIndex(val => val.Value == value.Value));
        }
    },
    /**
    * Permet de récupérer la valeur du filtre activée (bool)
    * @param {string} val valeur (bool) du filtre
    * @param {string} action action à effectuer (maj, annulation)
    * */
    methods: {
        getTabDescid,
        async doLogicFilter(val, action) {

            var descId = this.propData.datas.DescId;

            let options = {
                value: val,
                type: 'LOGIC',
                did: descId,
                action: action
            };

            EventBus.$emit('updateBkmExpressFilter', options);
            this.$emit('closeFilterLogic');
        },
        doLogicChart() {
            let nTabType = this.propData.datas.Format == FieldType.Button ? '25' : '3';            
            //let DescIdsignet = this.getTabDescid(this.propData.fullDatas.Structure.MainFieldId);
            let DescIdsignet = this.propData.fullDatas.Structure.ViewMainTab;
            let fldFormat = this.propData.fullDatas
                ?.Data?.map(n => n.LstDataFields
                    ?.find(ldf => ldf.DescId == this.propData.datas.DescId))
                ?.shift()
                ?.FldFormat;

            var expressFilter = new eExpressFilter("expressFilter", this.propData.datas.DescId, nTabType, DescIdsignet, fldFormat, this.getTab, "", "", null);

            doStats(expressFilter);

        },

        /**
         * Met à jour les pref, et accessoirement sélectionne les données du
         * tableau par rapport aux filtres.
         * @param {any} sFilterParam le filtre
         */
        async updatePref(sFilterParam) {
            //ajout de la var localement car this.updPref n'est pas complet au moment de l'appel (this.getTab renvoie undefined)
            let updPref = "tab=" + this.getTab + ";$;bkm=" + this.propData.propSignet.DescId + ";$;filterExpress=" + this.propData.datas.DescId + ";|;";

            this.setUserBkmPref({ updatePref : updPref + sFilterParam})

            var options = {
                id: this.propData.propSignet.id,
                signet: this.propData.propSignet.DescId,
                forceReturnOrigin: true,
            };
            EventBus.$emit('reloadSignet_' + this.propData.propSignet.id, options);
            this.$emit('closeFilterModal');

        },

        /**
         * Effectue une recherche sur tous les éléments du tableau.
         * @param {any} val la valeur à trouver (non, parce que rechercher, ça tout le monde peut le faire).
         */
        searchFromValue: function (val) {
            switch (this.$options.name) {
                case "expressFilterNumerique": val = eNumber.ConvertDisplayToBdd(val, true);
                    break;
                //case "expressFilterDate": val = eDate.ConvertDisplayToBdd(val);
                //    break;
            }

            this.updatePref(Operator.OP_EQUAL + ";|;" + val);
        },

    }
}