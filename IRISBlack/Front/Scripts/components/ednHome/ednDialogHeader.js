import { eDialogMixin } from '../../mixins/eDialogMixin.js?ver=803000';

export default {
    name: "ednDialogHeader",
    data() {
        return {
            sColor: "rgba(0, 0, 0, 0)",
            bLogo: true,
            bCloseButton: true,
            aLinks: []
        }
    },
    props: {
        oProps: Object,
    },   
    components: {
        ednDialogLogo: () => import(AddUrlTimeStampJS("./ednDialog/ednDialogLogo.js")),
        ednDialogLinkContainer: () => import(AddUrlTimeStampJS("./ednDialog/ednDialogLinkContainer.js")),
        ednDialogCloseButton: () => import(AddUrlTimeStampJS("./ednDialog/ednDialogCloseButton.js")),
        ednDialogToolBar: () => import(AddUrlTimeStampJS("./ednDialog/ednDialogToolBar.js")),
    },
    computed: {
        /** props pour la toolbar. */
        getToolBarProps: function () {
            return {
                sColor: this.sColor,
                sClass: "elevation-0",
            }
        },

        /** Props pour le bouton de fermeture */
        getCloseButtonProps: function () {
            return {
                sIcon: "mdi-close-circle",
                sColor: "white",
                sClass: "edn--home--dialog--closebutton",
                sId: "ednHomeDialogCloseButton",
                bIcon: true,
                bRight: true,
            }

        },

        /** Props pour les liens */
        getLinksContainerProps: function () {
            return {
                sColor: "white",
                bText: true,
                aLinks: this.aLinks,
            }

        },
        /** Détermine si on doit afficher des liens ou non. */
        getDisplayLinks: function () {
            return this.aLinks?.length > 0;
        }
    },
    methods: {

    },
    created() {
        this.sColor = this.oProps?.sColor || this.sColor;
        this.bLogo = this.oProps?.bLogo || this.bLogo;
        this.bCloseButton = this.oProps?.bCloseButton || this.bCloseButton;
        this.aLinks = this.oProps?.aLinks || this.aLinks;
    },
    mixins: [eDialogMixin],
    template: `
        <v-card-title :color="sColor" class="pt-10 px-10 edn--home--dialog--header">
        <ednDialogToolBar :oProps="getToolBarProps">
            <ednDialogLogo v-if="bLogo"></ednDialogLogo>
            <v-spacer></v-spacer>
            <ednDialogLinkContainer v-if="getDisplayLinks" :oProps="getLinksContainerProps"></ednDialogLinkContainer>
            <ednDialogCloseButton :oProps="getCloseButtonProps" v-if="bCloseButton" @action="action"></ednDialogCloseButton>
        </ednDialogToolBar>
        </v-card-title>
    `,

};