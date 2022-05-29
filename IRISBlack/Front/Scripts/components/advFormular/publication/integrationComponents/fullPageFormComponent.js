export default {
    name: "FullPageForm",

    data() {
        return {
            copied: false,
            inputFullPageCode: this.$store.getters.getRes(2752, ''), //Code à copier/coller dans la balise Body de votre page web
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

        published() {
            return this.$store.state.Published;
        },

        codeValue: {
            get() {
                if (this.published) {
                    return this.$store.state.FormularIntegrationScript;
                } else {
                    return ("<" + this.getRes(2753) + ">");
                }
            },
            set() { },
        }
    },
    methods: {
        async copyToClipboard() {
            if (this.$store.state.Published) {
                let vue = this;
                vue.copied = true;
                let inpt = this.$refs.valueLink.$el.querySelector("input");
                inpt.select();
                document.execCommand("copy");
                setTimeout(() => { vue.copied = false }, 1500)
            }
        },
    },
    props: ["dataTab"],

    template: `
    <div class="moduleContent" >
        <section class="content-header">
            <contentHeader :title=dataTab.title :subtitle=dataTab.txtSubTitle />
            <edn-copy-paste :label="inputFullPageCode" v-model="codeValue" readonly /> 
            
        </section>
    </div>
`,

};