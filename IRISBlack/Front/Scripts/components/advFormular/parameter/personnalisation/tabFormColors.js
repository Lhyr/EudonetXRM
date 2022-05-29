//tâche #3 459 création du composant Couleurs par défaut
export default {
    name: "FormColors",
    data() {
        return {
        };
    },
    components: {
        contentHeader: () => import("../../shared/contentHeaderComponent.js"),
    },

    mounted() {
    },
    computed: {
        getRes() {
            return function (resid) {
                return this.$store.getters.getRes(resid);
            };
        },
        //Libellés
        getAccentuationColorLabel() {
            return this.$store.getters.getRes(2759);
        },
        
        getPoliceColorLabel() {
            return this.$store.getters.getRes(2757);
        },

        accentuationColor: {
            get() {
                return this.$store.state.AccentuationColor;
            },
            set(accentuationColor) {
                this.$store.commit("setAccentuationColor", accentuationColor);
            },
        },
        
        policeColor: {
            get() {
                return this.$store.state.PoliceColor;
            },
            set(policeColor) {
                this.$store.commit("setPoliceColor", policeColor);
            }
        }
    },
    methods: {
    },
    props: ["dataTab"],

    template: `
    <div class="moduleContent" >
        <section class="content-header">
            <contentHeader :title=dataTab.title :subtitle=dataTab.txtSubTitle />
        
            <edn-color :label="getAccentuationColorLabel" v-model="accentuationColor"></edn-color>

            <edn-color :label="getPoliceColorLabel" v-model="policeColor"></edn-color>

        </section>
    </div>
`,

};