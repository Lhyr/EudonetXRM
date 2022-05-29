//tâche #3 458 création du composant Police par défaut
export default {
    name: "FormFont",
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
        getFontSizeLabel() {
            return this.$store.getters.getRes(7982); //Taille de la police de caractère
        },

        getFontNameLabel() {
            return this.$store.getters.getRes(1510); //Police
        },

        getFontSizeTooltip() {
            return this.$store.getters.getRes(2761); //Taille en pixels
        },

        fontSize: {
            get() { return this.$store.state.FontSize.toString(); },
            set(fontSize) { this.$store.commit("setFontSize", fontSize); }
        },

        fontName: {
            get() {
                return this.$store.state.FontName;
            },
            set(fontName) {
                this.$store.commit("setFontName", fontName);
            },
        },
        optFontSize() {
            that = this;
            var _AllAvailableFontSize = [];
            for (var i = 10; i <= 20; i++) {
                _AllAvailableFontSize[i.toString()] = i.toString();
            }

            return {
                optionSelected: that.$store.state.FontSize == "" ? "16" : that.$store.state.FontSize.toString(),
                opt: Object.keys(_AllAvailableFontSize).map(function (key) {
                    return { name: _AllAvailableFontSize[key], value: key };
                })
            }
        },
        optFont() {
            that = this;
            return {
                optionSelected: that.$store.state.FontName == "" ? "Arial" : that.$store.state.FontName,
                opt: Object.keys(that.$store.state.AllAvailableFonts).map(function (key) {
                    return { name: that.$store.state.AllAvailableFonts[key].name, value: that.$store.state.AllAvailableFonts[key].value };
                }),
            };
        }
    },
    methods: {
        isInteger(e) {
            const char = String.fromCharCode(e.keyCode)
            if (!/[0-9]/.test(char)) {
                e.preventDefault()  
            }
        }
    },
    props: ["dataTab"],

    template: `
    <div class="moduleContent" >
        <section class="content-header">
            <contentHeader :title=dataTab.title :subtitle=dataTab.txtSubTitle />
            
            <edn-cat v-model="fontSize" :label="getFontSizeLabel" :items="optFontSize.opt" item-value="value" item-text="name"></edn-cat>
            <edn-cat v-model="fontName" :label="getFontNameLabel" :items="optFont.opt" item-value="value" item-text="name"></edn-cat>
        </section>
    </div>
`,

};