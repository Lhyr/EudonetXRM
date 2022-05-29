import { eFileMixin } from '../../../mixins/eFileMixin.js';
export default {
    name: "eListMenuDropDown",
    data() {
        return {
           
        }
    },
    components: {},
    mixins: [eFileMixin],
    props: ['propsActions'],
    mounted() {},
    updated() {},
    methods: {

        /**
        * On renvoie l'action.
        * @param {any} item
        */
        actionMenu(item) {
            this.$emit('menu-action', item)
        }
    },
    computed: {},
    template: 
        `
    <v-list class="py-0">
        <template v-for="(action, idxAction) in propsActions">
            <v-tooltip :disabled="!action.tooltip || action.tooltip == ''" left nudge-bottom="-82">
                <template v-slot:activator="{ on, attrs }">
                    <v-list-item v-on="on" @click="actionMenu(action)" dense link :key="idxAction">
                        <v-list-item-icon class="mx-0">
                            <v-icon size="12">{{action.icon}}</v-icon>
                        </v-list-item-icon>
                        <v-list-item-title>{{action.name}}</v-list-item-title>
                    </v-list-item>
                </template>
                <span>{{action.tooltip}}</span>
            </v-tooltip>
        </template>
    </v-list>
    `
}