export default {
    name: "blocksComponent",
    data() {
        return {
        };
    },
    props: ['icon', 'title', 'description'],
    computed: {
        getRes() {
            return function (resid) { return this.$store.getters.getRes(resid) }
        },
    },
    template: `
    <v-card outlined tile  class="blockin rounded">
        <v-list-item>
          <v-list-item-icon class="blockico"><v-icon>{{ icon }}</v-icon></v-list-item-icon>
          <v-list-item-content class="blocktext">
            <v-list-item-title class="blocktitle">{{ getRes(title) }}</v-list-item-title>
            <v-list-item-subtitle class="blockdesc">{{ getRes(description) }}</v-list-item-subtitle>
          </v-list-item-content>
        </v-list-item>
    </v-card>
`,
};