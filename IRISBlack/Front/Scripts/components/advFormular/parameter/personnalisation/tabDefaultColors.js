//tâche #3 459 création du composant Couleurs par défaut
export default {
    name: "DefaultColors",
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
       
        getLinkColorLabel() {
            return this.$store.getters.getRes(2758);
        },

        linkColor: {
            get() {
                return this.$store.state.LinkColor;
            },
            set(linkColor) {
                this.$store.commit("setLinkColor", linkColor);
            },
        }
        
    },
    methods: {
    },
    props: ["dataTab"],

    template: `
    <div class="moduleContent" >
        <section class="content-header">
            <contentHeader :title=dataTab.title :subtitle=dataTab.txtSubTitle />

            <edn-color :label="getLinkColorLabel" v-model="linkColor"></edn-color>

        </section>
    </div>
`,

};