import { eModalComponentsMixin } from '../../../Scripts/mixins/eModalComponentsMixin.js?ver=803000';

export default {
    name: "eLinksOptions",
    mixins: [eModalComponentsMixin],
    data() {
        return {

        };
    },
    computed: {

    },
    mounted() {
    },
    methods: {

    },
    /* Objet attendu par la liste des liens */
    props: {
        propLinks: Array
    },
    template: `
        <ul class="links-list">
            <li v-for="link in propLinks"><a @click="link.action" href="#!">{{link.label}}</a></li>
        </ul>
`
};