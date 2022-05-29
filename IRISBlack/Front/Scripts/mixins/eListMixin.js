import { eMotherClassMixin } from './eMotherClassMixin.js?ver=803000';

/**
 * Mixin commune aux listes et aux composants directs (mais pas leurs sous-composants).
 * */
export const eListMixin = {
    data() {
        return {
        }
    },
    mixins: [eMotherClassMixin],
    components: {
    },
    created() {
    },
    computed: {
    },
    methods: {
    }
}