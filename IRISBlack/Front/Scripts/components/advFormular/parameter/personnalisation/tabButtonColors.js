//tâche #4 115 création du composant Couleurs des boutons
export default {
    name: "ButtonColors",
    data() {
        return {
        };
    },
    components: {
        contentHeader: () => import("../../shared/contentHeaderComponent.js"),
    },

    mounted() { },
    computed: {
        getRes() {
            return function (resid) {
                return this.$store.getters.getRes(resid);
            };
        },
        //Couleur de fond
        getButtonBackgroundColorLabel() {
            return this.$store.getters.getRes(1978);
        },

        //Couleur de la police
        getButtonPoliceColorLabel() {
            return this.$store.getters.getRes(2757);
        },

        buttonBackgroundColor: {
            get() {
                return this.$store.state.ButtonBackgroundColor;
            },
            set(buttonBackgroundColor) {
                this.$store.commit("setButtonBackgroundColor", buttonBackgroundColor);
            },
        },

        buttonPoliceColor: {
            get() {
                return this.$store.state.ButtonPoliceColor;
            },
            set(buttonPoliceColor) {
                this.$store.commit("setButtonPoliceColor", buttonPoliceColor);
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
        
            <edn-color :label="getButtonBackgroundColorLabel" v-model="buttonBackgroundColor"></edn-color>

            <edn-color :label="getButtonPoliceColorLabel" v-model="buttonPoliceColor"></edn-color>

        </section>
    </div>
`,

};