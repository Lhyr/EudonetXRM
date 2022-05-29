import { eMotherClassMixin } from './eMotherClassMixin.js?ver=803000';

/**
 * Mixin commune aux composants des fiches (mais pas aux listes).
 * */
export const eListComponentsMixin = {
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
        /** Ajoute une class si la modification en signet de la saisie guidée est activé */
        getAddPurplFileClass: function () {
            if (!this.getAddPurpleFile(this.dataJson))
                return "IrisModeGuidDisabled";
        }

    },
    props: {
        propSignet: Object,
    },
    methods: {
        /** Verifie si la modification en signet de la saisie guidée est activée pour chaque ligne de l'onglet */
        getBkmEditRight: function (row) {
            return row?.RightIsUpdatable && this.propSignet?.Actions?.AddPurpleFile
        },
        /** Verifie si la modification en signet de la saisie guidée est activée */
        getAddPurpleFile: function (data) {
            if (!data?.Data)
                return;

            return data?.Data.filter(row => row.RightIsUpdatable)?.length > 0 && this.propSignet?.Actions?.AddPurpleFile
        }
    },
}