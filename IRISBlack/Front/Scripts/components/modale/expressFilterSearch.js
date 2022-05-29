import { eMotherClassMixin } from '../../mixins/eMotherClassMixin.js?ver=803000';

export default {
    name: "expressFilterSearch",
    data() {
        return {
            nTab: this.getTab,
            fileId: this.$store.state.fileId,
        };
    },
    computed: {
        idKeySearch: function () {
            return "mru_search_" + this.nTab;
        }

    },
    mixins: [eMotherClassMixin],
    components: {
    },
    template: `
    <li class="searchExpress">
        <input type="text" id="mru_search" ref="mru_search" @keyup="doOnKeyUpSearch($event);" maxlength="100" class="navSearch" />
        <span id="btnSrch" class="fas fa-search navSearchSpan" ref="btnSrch" :title="getRes(1040)"  @click="doOnKeyUpSearch($event);"></span>
    </li>
`,
    methods: {
        /**
         * Permet d'affecter à SearchValue qui se trouve dans le parent, une valeur,
         * si au moins 3 lettres sont tapées.
         * Permet également de réinitialiser le input si on clique sur la croix.
         * @param {event} e un événement
         */
        doOnKeyUpSearch: function (e) {
            var q = this.$refs["mru_search"].value;
            var btnSearch = this.$refs["btnSrch"];

            if ((btnSearch.classList.contains("fa-times") && !e.keyCode) || q == "") {
                this.$refs["mru_search"].value = null;
                this.$parent.SearchValue = "";

                btnSearch.classList.remove("fa-times");
                btnSearch.classList.add("fa-search");
                btnSearch.setAttribute("title", this.getRes(1040));

                return;
            }

            if (q != "") {
                btnSearch.classList.remove("fa-search");
                btnSearch.classList.add("fa-times");
                btnSearch.setAttribute("title", this.getRes(19));
            }

            if ((e.keyCode != 13 && q.length < 3) || (q.length == 0))
                return;


            //if (isFormatNumeric(oExpressFilter.nFieldType))
            if (this.$parent.$options.name == "expressFilterNumerique")
                q = eNumber.ConvertDisplayToBdd(q, true);

            this.$parent.SearchValue = q;
        }

    },
    mounted() {
       
    },
};