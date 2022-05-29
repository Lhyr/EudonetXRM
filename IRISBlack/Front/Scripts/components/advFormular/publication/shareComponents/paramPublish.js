export default {
    name: "publish",
    data() {
        return {
            Libelle: this.$store.getters.getRes(2633, ''),
            LibelleBis: this.$store.getters.getRes(2634, ''),
        };
    },
    components: {
        contentHeader: () => import("../../shared/contentHeaderComponent.js")
    },

    watch: {
        published: async function (newPub, oldPub) {

            if (newPub && !oldPub) {

                this.$store.commit("formularStore/setFormularTS")

                await SaveFormular({
                    formularName: this.$store.state.formularName,
                    url: this.$store.state.url,
                    nFormularId: this.$store.state.nFormularId,
                    tab: this.$store.state.tab,
                    isNewPublication: true
                },
                    (responseJson) => {
                        if (responseJson == null || !responseJson.Success) {
                            //on annule l'action du switch
                            this.$store.commit("setPublished", false);
                            return;
                        }
                    }
                );
            }
        }
    },

    computed: {
        published: {
            get() {
                return this.$store.state.Published;
            },
            set(published) {
                this.$store.commit("setPublished", published);
            },
        },
    },
    mounted() { },
    methods: {},
    props: ["dataTab"],
    template: `
	<div class="moduleContent">
		<section class="content-header">            
			<contentHeader  :title=dataTab.title :subtitle=dataTab.txtSubTitle />
		</section>
		<div class="publishContent" >
		<div><div for="publishSwitch"  class="publishLabel">
			 {{ Libelle }}
			</div>
			<div for="publishSwitch" class="publishSwitchContent">
			  {{ LibelleBis }}
			</div></div>
		   <edn-switch  
			  id="publishSwitch"
			  v-model="published" />
		</div>
	</div>
`,

};


