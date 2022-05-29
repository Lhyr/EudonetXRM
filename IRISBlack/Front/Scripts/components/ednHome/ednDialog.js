import { eDialogMixin } from '../../mixins/eDialogMixin.js?ver=803000';

export default {
    name: "ednDialog",
    data() {
        return {
            sHeight: "90%",
            sWidth: "90%",
            sColor: "rgba(0, 0, 0, 0.8)",
            sClass: "rounded-0",
            sUrl: "https://fr.eudonet.com/news-admin/News-Admin.html",
            bDark: true,
            bFullscreen: true,
            bScrollable: true,
            bHideOverlay: true,
            bPersistent: true,
        };
    },
    components: {
        ednDialogHeader: () => import(AddUrlTimeStampJS("./ednDialogHeader.js")),
        ednDialogBody: () => import(AddUrlTimeStampJS("./ednDialogBody.js")),
        ednDialogFooter: () => import(AddUrlTimeStampJS("./ednDialogFooter.js")),
    },
    computed: {
        /** Envoie la Props pour le Header **/
        getPropsHeader: function () {
            return {
                bLogo: true,
                bCloseButton: true,
                aLinks: this.oProps?.aLinks,
            }
        },
        /** Envoie la Props pour le Footer **/
        getPropsFooter: function () {
            return {
                sBtnText: this.oProps?.sButtonText || this.getRes(2768),
            }
        },
        OpenedDialog: {
            get: function () {
                return this.openDialog;
            },
            set: function (value) {
                this.$emit('update:openDialog', value);
            }
        }

    },
    mixins: [eDialogMixin],
    props: {
        oProps: Object,
        openDialog: Boolean,
    },
    methods: {
        /** Méthode qui permet de fermer la modale */
        closeModal: function () {
            this.OpenedDialog = false;
        },
        /** Methods qui permet de fermer la modale à tout jamais ! */
        setNotDisplayAnymore: function () {
            this.setNewsMessageStopDisplay();
            this.closeModal();
        },
    },
    created() {
        this.sHeight = this.oProps?.sHeight || this.sHeight;
        this.sWidth = this.oProps?.sWidth || this.sWidth;
        this.sColor = this.oProps?.sColor || this.sColor;
        this.sUrl = this.oProps?.sUrl || this.sUrl;
        this.bDark = this.oProps?.bDark;
        this.bFullscreen = this.oProps?.bFullscreen;
        this.bScrollable = this.oProps?.bScrollable;
        this.bHideOverlay = this.oProps?.bHideOverlay;
        this.bPersistent = this.oProps?.bPersistent;
    },
    template: `
<v-row justify="center">
    <v-dialog v-model="OpenedDialog" :hide-overlay="bHideOverlay" 
              :fullscreen="bFullscreen" :scrollable="bScrollable" 
              :width="sWidth" :height="sHeight" :persistent="bPersistent">
        <v-card :class="sClass" :dark="bDark" :color="sColor">
            <ednDialogHeader :oProps="getPropsHeader" @action="closeModal"></ednDialogHeader>
            <ednDialogBody :sUrl="sUrl"></ednDialogBody>
            <ednDialogFooter :oProps="getPropsFooter" @action="setNotDisplayAnymore"></ednDialogFooter>
        </v-card>
    </v-dialog>
</v-row>
`,

};