import { eCatalogMixin } from './eCatalogMixin.js?ver=803000';
import { onCatalogValueMouseOver, onCatalogValueMouseLeave, translateTextForMultiple, translateTextForMultipleStop } from '../../../methods/eComponentsMethods.js?ver=803000';

export default {
    name: "eCatalogFileMultiple",
    data() {
        return {};
    },
    methods: {
        onCatalogValueMouseLeave,
        onCatalogValueMouseOver,
        translateTextForMultiple,
        translateTextForMultipleStop
    },
    computed: {
        /** Dans le cas d'un non-arbre, on met les classes css pour 
         * supprimer des éléments. */
        clsNotTreeViewAndLocked() {
            // #3834 - Si on est sur un catalogue multiple en lecture seule, on ne renvoie pas de classe
            if (this.IsDisplayReadOnly)
                return "";

            // Sinon, dans tous les autres cas, une croix de fermeture
            let sClass = "clsEltSummaryCross multiple_choice_remove";
            // Avec spécificités pour les catalougues arborescents
            if (!this.dataInput.IsTree)
                sClass += " fas fa-times-circle cursor_pointeur";
            return sClass;
        },
        /** Classe CSS pour l'élément li */
        clsLi() {
            let sClass = "clsEltSummary d-flex align-center";

            // #3834 - Si on est sur un catalogue multiple en lecture seule, on ajoute une classe spécifique
            if (this.IsDisplayReadOnly)
                sClass += " clsEltSummaryReadOnly";

            return sClass;
        },
    },
    props: {
        dataInput: Object,
        valueToDisplay: Array,
    },
    mixins: [eCatalogMixin],

    async mounted() {
        this.displayInformationIco();

        if (!(this.dataInput.ReadOnly || this.dataInput.Multiple || this.showMru))
            await this.setCatalogEllipsis();
    },
    template: `
         <span @mouseleave.stop @mouseover.stop :class="[IsDisplayReadOnly ? 'noModif' : 'Modif']" class="multiRenderer form-control input-line fname clsCatFileMult">
            <ul @mouseleave.stop @mouseover.stop :ref="'field'+dataInput.DescId" :field="'field'+dataInput.DescId" class="clsCatalogSummary">
                <li
                    v-if="dataInput.Value == '' && dataInput.Watermark != null"
                    @mouseleave.stop
                    @mouseover.stop
                    class="cat-placeholder"
                >
                    {{dataInput.Watermark}}
                </li>
                <li
                    v-else
                    v-for="value in valueToDisplay"
                    :key="value.id"
                    @mouseleave="onCatalogValueMouseLeave($event, value, dataInput)"
                    @mouseover="onCatalogValueMouseOver($event, value, dataInput)"
                    :id="'catVal' + value.id"
                    :ref="'catVal' + value.id"
                    :class="clsLi"
                    :style="{ background: dataInput.ValueColor, borderColor:dataInput.ValueColor}"         
                >
                    <span>{{value.value}}</span>
                    <span @mouseleave.stop @mouseover.stop ref="uniqueval" :class="clsNotTreeViewAndLocked" role="presentation" @click="removeOneValue($event, value)" ></span>
                </li>
            </ul>
        </span>
`
};