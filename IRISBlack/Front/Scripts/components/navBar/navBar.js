const logoNavBar = () =>
    import(AddUrlTimeStampJS("./logoNavBar.js"))

const ongletNavBar = () =>
    import(AddUrlTimeStampJS("./ongletNavBar.js"))

const navBarCustom = () =>
    import(AddUrlTimeStampJS("./navBarCustom.js"))

const ongletNavBarArrow = () =>
    import(AddUrlTimeStampJS("./ongletNavBarArrow.js"))

export default {
    name: "navBar",
    data() {
        return {
            OngletsData: Object,
            onglets: Object,
            navArrow: null,
            widthBase: 0,
            visibility: null,
            mobileNav: false
        };
    },

    components: {
        logoNavBar,
        ongletNavBar,
        navBarCustom,
        ongletNavBarArrow
    },
    mounted() {
        window.addEventListener('resize', this.getNavBar);
        this.OngletsData = this.propOngletsDatas;
    },

    beforeDestroy() {
        window.removeEventListener('resize', this.getNavBar);
    },
    methods: {
        initNavbarCustom() {
            this.widthBase = this.$refs.navBar.children[1].clientWidth;
            this.getNavBar()

        },
        getNavBar() {
            if (window.innerWidth < 768) {
                this.mobileNav = true
                return;
            } else {
                this.mobileNav = false
                let navWidth = this.$refs.navBar.clientWidth
                let logoWidth = this.$refs.navBar.children[0].children[0].clientWidth;
                let ongletsNavBarWidth = this.$refs.navBar.children[1].clientWidth;
                let customNavBar = this.$refs.navBar.children[2].clientWidth;
                let ongletsNavBar = this.$refs.navBar.children[1];
                if (navWidth < logoWidth + this.widthBase + customNavBar + 50) {
                    this.navArrow = true;
                } else if (navWidth > logoWidth + this.widthBase + customNavBar + 50) {
                    this.navArrow = false
                    this.visibility = 'visibility'
                }
            }
        }
    },
    props: ['propOngletsDatas'],
    template: `
      <nav class="navbar navbar-static-top">
        <div ref="navBar" id="contenairNavBar" class="container">
            <logoNavBar></logoNavBar>
            <ongletNavBar :prop-onglets-datas="this.OngletsData" style="visibility:hidden" v-bind:class="{ 'visibility': this.visibility == 'visibility'}" v-if='!this.navArrow && !this.mobileNav'></ongletNavBar>
            <ongletNavBarArrow :prop-onglets-datas="this.OngletsData" v-if='this.navArrow && !this.mobileNav'></ongletNavBarArrow>
            <a v-if='this.mobileNav' href="#" class="sidebar-toggle" data-toggle="push-menu" role="button">
                <span class="sr-only">Toggle navigation</span>
            </a>
          <navBarCustom :post-init="this.navArrow" @init-navbar-custom="initNavbarCustom"></navBarCustom>
        </div>
      </nav>
`,
};