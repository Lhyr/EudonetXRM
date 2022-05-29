import { eMotherClassMixin } from './eMotherClassMixin.js?ver=803000';
import { showTooltipObj} from '../methods/eComponentsMethods.js?ver=803000';

/**
 * Mixin commune aux composants des fiches (mais pas aux listes).
 * */
export const eMruComponentsMixin = {
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
        /**
        * ELAIZ - Méthode intermédiaire pour appeler showTooltip ( en effet elle envoie beaucoup de paramètre ce qui la rend illisible sur le template)
        * J'envoie le contexte du composant avec ctx car il n'est pas possible de binder this à la méthode;
        * @param {any} visible - affiche ou non l'infobulle
        * @param {any} elem - la ref vue de l'élément ciblé
        * @param {any} label - le libellé à afficher dans l'infobulle
        * @param {any} idRow - index de la ligne concernée
        */
        hoveringElm(visible, elem, label, index) {
            let obj = {
                label: (label ? label : 'close'),
                elem: elem,
                visible: visible,
                icon: false,
                readonly: false,
                data: this.dataInput,
                ctx: this,
                id: index
            };
            if (elem == 'mruEmptyCatalog')
                showTooltipObj(Object.assign(obj, { emptyMru: true }));
            else if (elem == 'mruCatalog' || elem == 'mruUser')
                showTooltipObj(Object.assign(obj, { dataMru: true }));
            else
                showTooltipObj(Object.assign(obj, { allMru: true }));
        }
    }
}