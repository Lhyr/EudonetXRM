export default {
    name: "ongletNavBarArrow",
    data() {
        return {
            getRight: 0,
            widthBarNav: 0,
            allOngletsWidth: 0,
            interation: 0,
            OngletsData: Object
        };
    },

    mounted: function() {
        this.OngletsData = this.propOngletsDatas;
        this.getWidthNav()
        window.addEventListener('resize', this.getWidthNav);
        let that = this
        Vue.nextTick(function() {
            that.initWidthDataOnglet();
        })
    },
    beforeDestroy() {
        window.removeEventListener('resize', this.getWidthNav);
    },
    methods: {
        getWidthNav() {
            let navWidth = this.$refs.navBarOngletsArrow.parentElement.parentElement.clientWidth
            let logoWidth = this.$refs.navBarOngletsArrow.parentElement.children[0].children[0].clientWidth;
            let ongletsNavBarWidth = this.$refs.navBarOngletsArrow.parentElement.children[1];
            let customNavBar = this.$refs.navBarOngletsArrow.parentElement.children[2].clientWidth;
            this.widthBarNav = navWidth - (logoWidth + customNavBar + 85)
            ongletsNavBarWidth.children[1].style.width = this.widthBarNav + 'px'
            for (var i = 0; i < ongletsNavBarWidth.length; i++) {
                this.allOngletsWidth = ongletsNavBarWidth[i].clientWidth + this.allOngletsWidth
            }

        },
        initWidthDataOnglet() {
            let ongletsNavBar = this.$refs.navBarOngletsArrow.parentElement.children[1].children[1].children;
            for (var i = 0; i < ongletsNavBar.length; i++) {
                this.OngletsData.Onglets[i].width = ongletsNavBar[i].clientWidth;
                this.allOngletsWidth = this.OngletsData.Onglets[i].width + this.allOngletsWidth
            }
        },
        runRight() {
            let ongletsNavBarWidth = this.$refs.navBarOngletsArrow.parentElement.children[1].children[1];
            if (this.getRight + ongletsNavBarWidth.clientWidth >= this.allOngletsWidth) {
                return;
            } else {
                this.interation += 1;
                this.getRight = this.getRight + this.OngletsData.Onglets[this.interation - 1].width
                ongletsNavBarWidth.style.right = this.getRight + 'px';
            }
        },
        runLeft() {
            let ongletsNavBarWidth = this.$refs.navBarOngletsArrow.parentElement.children[1].children[1];
            if (this.interation == 0) {
                return;
            } else {
                this.interation--;
                this.getRight = this.getRight - this.OngletsData.Onglets[this.interation].width
                ongletsNavBarWidth.style.right = this.getRight + 'px';
            }
        },
    },
    props: ['propOngletsDatas'],
    template: `
    <div ref="navBarOngletsArrow" id="NavBar" class="nav_main collapse navbar-collapse pull-left navBarArrow">
    <button v-on:click="runLeft" class="btn arrowNavBarLeft"><i class="fa fa-angle-left"></i></button>
        <ul id="NavBarSub" class="nav navbar-nav">
            <li v-if="onglet.inner" v-bind:class="{ 'active': onglet.active}" class="dropdown rubonNav" v-for="onglet in OngletsData.Onglets" :key="onglet.id">
                <a :href="onglet.lien" class="dropbtn">{{onglet.name}}</a>
                <div v-if="onglet.drop" class="dropdown-content" role="menu">
                    <div v-if="onglet.recherche" class="has-feedback">
                        <input class="form-control input-sm" type="textOnglets" placeholder="Recherche" oninput="searchFiltre(this)">
                        <span class="glyphicon glyphicon-search form-control-feedback"></span>
                    </div>
                    <div v-if="onglet.recherche" role="presentation" class="divider firstDivider"></div>
                    <div v-if="onglet.recherche" class="contentSearchResult" style="display: none;">
                        <div class="divSearchSimple">
                            <span style="padding: 5px 15px;">Aucune fiche trouvée</span>
                        </div>
                        <div role="presentation" class="divider"></div>
                        <div class="divSearchBlock">
                            <a>Débute par "<span class="valueSearchResult"></span>" </a>
                            <a>Contient "<span class="valueSearchResult"></span>"</a>
                        </div>
                        <div role="presentation" class="divider"></div>
                    </div>
                    <div v-if="onglet.mru" class="divHistorique" style="display: block;">
                        <a :href="mru.lien" v-for="mru in onglet.mru" :key="mru.id">{{mru.name}}</a>
                        <div role="presentation" class="divider"></div>
                    </div>
                    <a :href="menu.lien" v-for="menu in onglet.dropdown" :key="menu.id">{{menu.name}}</a>
                </div>
            </li>
        </ul>
        <button style="position:absolute;right:-53px" v-on:click="runRight" class="btn arrowNavBarLeft"><i class="fa fa-angle-right"></i></button>
        
    </div>
    

`
}