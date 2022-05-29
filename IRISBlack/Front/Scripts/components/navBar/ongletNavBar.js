export default {
    name: "ongletNavBar",
    data() {
        return {
            OngletsData: Object
        };
    },

    mounted: function() {
        this.OngletsData = this.propOngletsDatas
    },
    methods: {},
    props: ['propOngletsDatas'],
    template: `
    <div ref="navBarOnglets" id="NavBar" class="nav_main collapse navbar-collapse pull-left navNormal">
        <ul id="NavBarSub" class="nav navbar-nav">
            <li v-bind:class="{ 'active': onglet.active}" v-if="onglet.inner" class="dropdown rubonNav" v-for="onglet in OngletsData.Onglets" :key="onglet.id">
                <a :title="onglet.name" :href="onglet.lien" class="dropbtn">{{onglet.name}}</a>
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
                        <a :title="'(' + onglet.name + ') ' + mru.name" :href="mru.lien" v-for="mru in onglet.mru" :key="mru.id">{{mru.name}}</a>
                        <div role="presentation" class="divider"></div>
                    </div>
                    <a :title="'(' + onglet.name + ') ' + menu.name" :href="menu.lien" v-for="menu in onglet.dropdown" :key="menu.id">{{menu.name}}</a>
                </div>
            </li>
        </ul>
        <ul id="navUl">
            <li id="navLi" class="dropdown rubonNav">
                <a id="addMenu" href="#" class="dropbtn">
                    <i class="fa fa-plus"></i>
                </a>
                <div class="dropdown-content" role="menu">
                    <a href="#" data-toggle="modal" data-target="#modal-wizard-choix-onglets">Choix des onglets</a>
                    <a class="addOnglet" href="#">Nouvelle vue</a>
                </div>
            </li>
        </ul>
    </div>
`
}