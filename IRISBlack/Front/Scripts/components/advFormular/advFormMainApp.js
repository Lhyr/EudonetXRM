/**
 *  Application principale
 * 
 * 
 */
const headNav = () =>   import ("./headNav.js");
const contentTab = () =>   import ("./contentTab.js")

export default {
    name: "App_assist_form",
    data() {
        return {
            ActiveTab : ""
        };
    },
    components: {
        headNav,
        contentTab
    },
    mounted() {},
    methods: {},
    computed: { },
    template: `
    <v-app ref="wrapper" class="wrapper">
        <div class="content-wrapper">
            <section class="content">
                <div class="row">
                    <headNav></headNav>
                    <contentTab></contentTab>
                </div>
            </section>
        </div>
    </v-app>
`,
};