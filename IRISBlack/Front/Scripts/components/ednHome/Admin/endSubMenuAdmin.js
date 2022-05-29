import { getColor, getUserInfos, fnForceChckNwThm, getCurrentTab } from '../../../shared/XRMWrapperModules.js?ver=803000';
import {
    setStatusNewFile, callbackFloatButton, getAuthorizedAdminUser,
    getUserAuth, linkToPost
} from "../../../methods/eFileMethods.js?ver=803000";

export default {
    name: "app-menu",
    data: function () {
        return {
            floatingButtons: {
                align: 'left',
                alignVertical: '30vh',
                zIndex: 20,
                actions: [
                    {
                        text: top._res_1460,
                        icon: "mdi-check-circle-outline",
                        sizeIcon: "18",
                        colorBtn: this.getColor('activeNewFileMode', this.oProps?.nTab),
                        sClassBtn: "white--text elevation-0 px-3",
                        function: 'activeNewFileMode',
                        disabled: false,
                    },
                    {
                        text: top._res_3122,
                        icon: "mdi-eye-outline",
                        sizeIcon: "18",
                        colorBtn: this.getColor('activeNewFileModePreview', this.oProps?.nTab),
                        sClassBtn: "white--text elevation-0 px-3",
                        function: 'activeNewFileModePreview',
                        disabled: false,
                    },
                    {
                        text: top._res_1459,
                        icon: "mdi-close-circle-outline",
                        sizeIcon: "18",
                        colorBtn: this.getColor('desactiveNewFileMode', this.oProps?.nTab),
                        sClassBtn: "white--text elevation-0 px-3",
                        function: 'desactiveNewFileMode',
                        disabled: false,
                    },
                    {
                        text: top._res_1625,
                        icon: "mdi-cog",
                        sizeIcon: "18",
                        colorBtn: "blue-grey",
                        sClassBtn: "white--text elevation-0 px-3",
                        function: 'openDialogSetting',
                        disabled: true,

                    }
                ]
            },
            openDialogSetting: false,
        }
    },
    components: {
        modalWrapper: () => import(AddUrlTimeStampJS("../../modale/modalWrapper.js")),
        floatingButtons: () => import(AddUrlTimeStampJS("../../floatingButtons/floatingButtons.js")),
        fileSettings: () => import(AddUrlTimeStampJS("../../fiche/fileSettings.js")),
    },
    computed: {
        getUserInfos,
        getAuthorizedAdminUser,
        getUserAuth,
        getCurrentTab,
        /** Tricks pour les fonctions d'Iris balck qui se servent du Store. */
        getTab: function () { return this.oProps.nTab; },
        getFileId: function () { return this.oProps.nFileId; }
    },
    methods: {
        getColor,
        setStatusNewFile,
        callbackFloatButton,
        linkToPost,
        fnForceChckNwThm,
        /** Ferme la modale */
        closeFileSettings: function () {
            this.openDialogSetting = false;
        },
        getFileSettings: function () { },
        saveFileSettings: function () {},
        /** Permet d'activated le new mode fiche Lvl 2 */
        activateNewErgo: function () {

        }
    },
    props: {
        oProps: Object
    },
    template: `
    <v-app class="edn--home--wrapper edn--home--wrapper--floating--button edn--home--admin" id="app-menu">
        <floatingButtons :prop-tab="getTab" v-if="getUserAuth" @callback="callbackFloatButton" :props-floating-button="floatingButtons" />
        <modalWrapper v-if="getAuthorizedAdminUser" width="1100px" v-model="openDialogSetting">
            <template v-slot:content>
                <fileSettings :objItm="getFileSettings" @close="closeFileSettings" @save="saveFileSettings" />
            </template>
        </modalWrapper>
    </v-app>`
};
