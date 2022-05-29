import fiche from "./fiche.js?ver=803000";

export default {
    name: "App",
    data() {
        return {
            go: null,
            bDisplayed:true,
            fileRoot:null
        }
    },
    components: {
        fiche
    },
    async mounted() {
        // calcul d'un hauteur minimum de la class .content-wrapper
        var container = document.getElementById("container");
        var contentWrapper = document.getElementById("MainWrapper").children[0];
        contentWrapper.style.minHeight = container.style.height;
        this.$refs["wrapper"].addEventListener('resize', this.contentHeight);
        this.fileRoot = this;
    },
    methods: {
        contentHeight() {
            if (this.$refs["wrapper"].children?.length > 0) {
                var contentWrapper = this.$refs["wrapper"].children[0];
                var body = document.getElementsByTagName("body")[0];
                contentWrapper.style.minHeight = body.clientHeight + 'px';
            }
        }
    },
    template: `
    <div id="MainWrapper" ref="wrapper" class="wrapper skin-red layout-top-nav Bodyfiche">
        <div class="content-wrapper">
            <section class="content">
                <div class="row">
                    <v-app v-if="bDisplayed">
                        <v-main>
                            <fiche :fileRoot="fileRoot" @isDisplayed="bDisplayed = $event" ref="file"></fiche>
                        </v-main>
                    </v-app>
                </div>
            </section>
        </div>
    </div>
`,
};