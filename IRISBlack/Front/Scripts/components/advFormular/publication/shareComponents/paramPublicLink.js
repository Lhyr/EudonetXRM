export default {
    name: "publicLink",

    data() {
        return {
            copied: false,
            regexUrl: /(ftp|http|https).*/,
            inputPublicURL: this.$store.getters.getRes(2659, ''),
        };
    },

    components: {
        contentHeader: () => import("../../shared/contentHeaderComponent.js"),
    },

    computed: {
        getRes() {
            return function (resid) {
                return this.$store.getters.getRes(resid);
            };
        },

        published() {
            return this.$store.state.Published;
        },

      
        linkValue: {
            get() {
                if (this.published) {
                    return this.$store.state.FormularLink;
                } else {
                    return (
                        "<" +
                        this.getRes(
                            2637,
                            "<Publier le formulaire pour afficher le lien public>"
                        ) +
                        ">"
                    );
                }
            },
            set() {
                    
            },

            integrationScriptValue: {
                get() {
                    if (this.published) {
                        return this.$store.state.FormularIntegrationScript;
                    } else {
                        return (
                            "<" +
                            this.getRes(
                                2637,
                                "<Publier le formulaire pour afficher le script d'intégration>"
                            ) +
                            ">"
                        );
                    }
                },
                set() {
                        
                },
            }

        }
    },

    props: ["dataTab"],

    methods: {

        async copyToClipboard() {
            if (this.$store.state.Published) {
                let vue = this
                vue.copied = true

                let inpt = this.$refs.valueLink.$el.querySelector("input")
                inpt.select();
                document.execCommand("copy");
                setTimeout(() => { vue.copied = false }, 1500)
            }
        },
    },


   

template: `
    <div class="moduleContent">

        <section class="content-header">

            <contentHeader :title=dataTab.title :subtitle=dataTab.txtSubTitle />

            <edn-url :label="inputPublicURL" v-model="linkValue" readonly :regexUrl=regexUrl />           
        
        </section>

    </div>
`,
};
