//const axios = require('axios');

export default {
    name: "ongletNavBar",
    data() {
        return {
            OngletsData: Object
        };
    },

    mounted: function() {
        $('.sidebar-menu').tree()
        this.OngletsData = this.propOngletsDatas
    },
    methods: {},
    props: ['propOngletsDatas'],
    template: `
    <aside id="menuAsideNav" class="main-sidebar">
        <section class="sidebar">
            <ul class="sidebar-menu" data-widget="tree">
                <li class="header">
                    <a href="#" title="Choix des onglets">
                        <i class="fa fa-plus"></i><span>Choix des onglets</span>
                    </a>
                </li>
                <li class="header">
                    <a href="#" title="Nouvelle vue">
                        <i class="fa fa-plus"></i><span>Nouvelle vue</span>
                    </a>
                </li>
                <li v-if="onglet.inner" v-bind:class="{ 'active menu-open': onglet.active}" class="treeview" v-for="onglet in OngletsData.Onglets" :key="onglet.id">
                    <a href="#" :title="onglet.name">
                        <i :class="onglet.icon"></i> <span>{{onglet.name}}</span>
                        <span v-if="onglet.mru || onglet.dropdown" class="pull-right-container">
                            <i class="fa fa-angle-left pull-right"></i>
                        </span>
                    </a>
                    <ul v-if="onglet.mru || onglet.dropdown" class="treeview-menu">
                        <div v-if="onglet.recherche" class="searchNavMobile">
                            <input placeholder="Recherche" type="text"></input>
                        </div>
                        <li v-bind:class="{ 'active': mru.active}" v-for="mru in onglet.mru" :key="mru.id" ><a :title="'(' + onglet.name + ') ' + mru.name" :href="mru.lien">{{mru.name}}</a></li>
                        <ul class="btn-tree">
                            <li v-for="dropdown in onglet.dropdown" ><a :title="'(' + onglet.name + ') ' + dropdown.name" :href="dropdown.lien"> {{dropdown.name}}</a></li>
                        </ul>
                    </ul>
                </li>
            </ul>
        </section>
        
    </aside>
`
}