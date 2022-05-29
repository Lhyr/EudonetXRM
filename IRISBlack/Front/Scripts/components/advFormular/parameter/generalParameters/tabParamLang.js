const contentHeader = () => import("../../shared/contentHeaderComponent.js");

export default {
    name: "ParamLang",

    data() {
        return { cp: 0, };
    },

    components: {
        contentHeader,
    },

    computed: {

        getRes() {
            return function (resid) {
                return this.$store.getters.getRes(resid);
            };
        },
        getUserLangID() {
            return function () {
                return this.$store.getters.getUserLangID;
            };
        },

        optLng() {
            that = this;
            return {
                optionSelected: that.$store.state.LangId < 0 ? this.getUserLangID().toString() : that.$store.state.LangId.toString(),
                opt: Object.keys(that.$store.state.AllAvailableLng).map(function (key) {
                    return { text: that.$store.state.AllAvailableLng[key], value: key };
                }),
            };
        },

        langid: {
            get() {
                return this.$store.state.LangId.toString();
            },
            set(langid) {
                this.$store.commit("setLangdid", langid);
                //on met à jour les res dans grapesJS
                UpdateFormularRes(langid, 28);
            },
        },
    },
    mounted() {
        if (
            this.$store.state.nFormularId === 0 &&
            this.$store.state.UserInfos &&
            this.langid === -1
        )
            this.langid = this.$store.state.UserInfos.LangId;
    },
    methods: {},
    props: ["dataTab"],
    template: `
      <div class="moduleContent">          
          <section class="content-header">
            <contentHeader  :title=dataTab.title :subtitle=dataTab.txtSubTitle />
            <edn-cat v-model="langid" 
              :label="getRes(dataTab.title)" 
              :items="optLng.opt" 
              item-value="value" 
              item-text="name"></edn-cat>
          </section>  
      </div>
  `,
};
