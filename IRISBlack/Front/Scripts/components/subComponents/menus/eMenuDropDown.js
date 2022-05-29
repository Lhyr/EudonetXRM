export default {
    name: "eMenuDropDown",
    data() {
        return {
            menus: Object
        }
    },
    components: {},
    mixins: [],
    props: ['propsMenus'],
    mounted() {
        // MAB & QBO - Au montage du composant, on résout la props qui est en asynchrone pour récupérer les données en provenance du contrôleur, et on place le résultat dans les data
        this.propsMenus.then(result => this.menus = result);
    },
    updated() {},
    methods: {},
    computed: {},
    template: 
        `
<v-row>
    <v-menu
        v-for="(menu, idxMenu) in menus.items"
        :key="idxMenu"
        left 
        rounded="0" 
        content-class="elevation-0" 
        nudge-bottom="-82"
        open-on-hover 
        offset-y
    >
        <template v-slot:activator="{ on, attrs }">
            <v-btn x-large height="28" v-bind="attrs" v-on="on" text class="ma-0 px-1 elevation-0 text-capitalize">
                {{menu.name}}
                <v-icon size="13" right>fas fa-chevron-down</v-icon>
            </v-btn>
        </template>
        <slot :menu="menu"></slot>
    </v-menu>
</v-row>
    `
}