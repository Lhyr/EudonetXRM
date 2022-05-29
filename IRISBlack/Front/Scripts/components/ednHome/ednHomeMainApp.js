/**
 *  Application principale
 * 
 * 
 */
export default {
    name: "App_edn_home",
    data() {
        return {
            initOpenDialog: true,
            sHeight: "90%",
            sWidth: "90%",
            sColor: "rgba(0, 0, 0, 0.8)",
            sClass: "rounded-0",
            sUrl: "https://fr.eudonet.com/news-admin/News-Admin.html",
            aLinks: null,
            sButtonText: null,
            bDark: true,
            bFullscreen: true,
            bScrollable: true,
            bHideOverlay: true,
            bPersistent: true,
        };
    },
    components: {
        ednDialog: () => import(AddUrlTimeStampJS("./ednDialog.js"))
    },
    props: {
        oProps: Object
    },
    methods: {},
    computed: {
        /** retourne une props pour la modale dialog */
        getPropsDialog: function () {
            return {
                sHeight: this.sHeight,
                sWidth: this.sWidth,
                sColor: this.sColor,
                sUrl: this.sUrl,
                aLinks: this.aLinks,
                sButtonText: this.sButtonText,
                bDark: this.bDark,
                bFullscreen: this.bFullscreen,
                bScrollable: this.bScrollable,
                bHideOverlay: this.bHideOverlay,
                bPersistent: this.bPersistent,
            }
        },
        openDialog: {
            set: function (value) {
                this.initOpenDialog = value;
                /*Demande 84622 - Suppression des css et du composant car à la fermeture le composant reste ainsi que le css de vuetify,
                /qui rentre en conflit avec le css de e17 notamment*/
                if (!value) {
                    top.clearHeader('EDNHOME');
                    //this.$destroy();
                }
            },
            get: function () {
                return this.initOpenDialog;
            }
        }
    },
    created() {
        this.sUrl = this.oProps?.sUrl || this.sUrl;
        this.aLinks = this.oProps?.aLinks || this.aLinks;
        this.sButtonText = this.oProps?.sButtonText || this.sButtonText;
    },
    template: `
    <v-app v-if="openDialog" class="edn--home--wrapper edn--home--wrapper--temporary">
        <div class="edn--dialog--start">
            <ednDialog :openDialog.sync="openDialog" :oProps="getPropsDialog"></ednDialog>
        </div>
    </v-app>
`,
};