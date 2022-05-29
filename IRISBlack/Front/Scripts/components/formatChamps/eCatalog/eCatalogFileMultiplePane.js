import { eCatalogMixin } from './eCatalogMixin.js?ver=803000'

export default {
    name: "eCatalogFileMultiplePane",
    data() {
        return {
            isDetailOpen: false,
            nLvlUp: 0,
        };
    },
    components: {
        eCatalogFileMultiple: () => import(AddUrlTimeStampJS("./eCatalogFileMultiple.js")),
        eAccordionPane: () => import(AddUrlTimeStampJS("../../subComponents/eAccordionPane.js")),
        eCatalogNbElementPlus: () => import(AddUrlTimeStampJS("./eCatalogNbElementPlus.js")),
        eButton: () => import(AddUrlTimeStampJS("../../subComponents/eButton.js")),
        eIcon: () => import(AddUrlTimeStampJS("../../subComponents/eIcon.js")),
    },
    // Au tout premier rendu, ou à la modification d'une donnée réactive, on réactualise l'affichage adaptatif des chips valeurs de catalogue.
    updated() {
        this.$nextTick(() => {
            this.checkCatalogContainerAvailability();
        });
    },
    mounted() {
        // Cet Observer sera utilisé pour détecter les changements de rendu de composant après que le rendu soit fait, y compris dans les cas où updated()
        // n'est pas redéclenché 
        // A noter qu'au tout premier rendu, this.$el === #comment. On ne peut donc pas poser d'Observer dessus, il faut donc le tester.
        // C'est pour ce cas que le updated() reste nécessaire pour déclencher la fonction au tout premier rendu
        if (this.$el instanceof Element) {
            let resizeO = new ResizeObserver(([e]) => this.checkCatalogContainerAvailability(e));
            resizeO.observe(this.$el);
        }
    },
    methods: {
        /**
         * Si le detail de l'accordion pane est ouvert, on n'affiche
         * plus le nombre d'éléments en plus.
         * @param {any} event
         */
        displayNbElementPlus: function (event) {
            this.isDetailOpen = event.target.open;
            this.nLvlUp += 1;
        },

        /**
         * Fonction qui va indiquer si on doit déployer l'accordeon ou non.
         * @param {any} bToShow
         */
        showZeAccordion(bToShow) {
            /*
            this.isDetailOpen = bToShow;
            this.nLvlUp += 1;
            */
        },
        /**
         * Ce qui se passe lorsqu'on effectue une action dans le navigation drawer.
         * On est d'accord, un switch, sur un événement, c'est un peu du gachis.
         * Mais il peut faire tellement, tellement plus...
         * @param {any} event
         */
        getPaneAction(event) {
            switch (event?.type) {
                case "click": this.showCatalogGenericViewIris(); break;
            }

            return false;
        },
    },
    computed: {
        /**
         * Affiche ou non le nombre d'éléments masqués.
         * */
        isDisplayNbElementPlus: function () {
            return this.isNbCatalogGTMax && !this.isDetailOpen
        },
		
		/**
		 * Indique si on peut procéder au calcul de l'affichage des valeurs de catalogue adaptatives, une fois que le DOM est prêt 
         * et dispose de valeurs de style exploitables sur les conteneurs (width en px)
		 * */
        canComputeAdaptativeValues: function () {
            if (!this.adaptativeValuesReady) {
                // #4081 - On câble, en complément, un timer, qui ira vérifier après chargement que les valeurs aient bien été initialisées
                // Permet de palier le fait que, parfois, le updated() ne soit pas déclenché si un évènement extérieur (exemple : erreur JS)
                // empêche le composant de s'afficher immédiatement au premier chargement, et donc, de déclencher l'affichage des valeurs
                this.adaptativeValuesTimer = window.setTimeout(this.checkCatalogContainerAvailability, 1000);
            }
            return this.adaptativeValuesReady;
        },
    },
    props: {
        dataInput: Object,
    },
    mixins: [eCatalogMixin],
    template: `
    <eCatalogFileMultiple v-if="valueMultiple.length < 1" :data-input="dataInput"  />
    <eAccordionPane v-else
        :is-detail-open="isDetailOpen"
        css-class-pane="clsDetailIndex multiRenderer fname"
        css-class-ftr="input-group fname clsBtnWrap"
        css-class-elt="hiddenCatValue"
        :readOnly="dataInput.ReadOnly"
        @setDetailsOpen="showZeAccordion"
        @toggle="$emit('accordionOpened', $event.target.open)"
        :key="nLvlUp">
        <template #header :data-input="dataInput" v-if="canComputeAdaptativeValues">
            <eCatalogFileMultiple :data-input="dataInput" :value-to-display="valueMultipleGTMax" />
            <eCatalogNbElementPlus :n-max-catalog-elm="nMaxCatalogDisplay" v-if="isDisplayNbElementPlus" :data-input="dataInput" />
        </template>
        <template #main :data-input="dataInput" v-if="canComputeAdaptativeValues">
            <eCatalogFileMultiple :data-input="dataInput" :value-to-display="valueMultipleFromMax" />
        </template>
        <template #footer>
            <eButton @action="getPaneAction">
                {{getRes(151)}}
                <eIcon v-if="dataInput.ReadOnly">eudo-lock fa fa-lock</eIcon>
            </eButton>
        </template>
    </eAccordionPane>
`
};