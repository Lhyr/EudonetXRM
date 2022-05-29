import { mapGetters, mapMutations, mapActions } from '../libraries/vuex/vuex.esm.browser.js?ver=803000';
import {
    setRunSpec, adjustDivContainerXRM, shFileInPopup, loadFile, deleteFile, printFile, reportList,
    getUrl, getRes, getUserLangID, getUserID, getUserInfos, getVersionCSS, getVersionJs, getIsTablet, getParamWindow, getInfoDate, getStorage,
    setUserBkmPref, setIrisUpdateValFromBkmModal, openLnkFileDialog
} from '../shared/XRMWrapperModules.js?ver=803000';
import { Lang, LangJson } from "../methods/Enum.min.js?ver=803000"
import ErrorService from "../error/ErrorService.js?ver=803000"

/**
 * Une classe contenant tout ce dont on a besoin pour
 * l'ensemble des classes.
 * On la récupère au travers d'autres Mixin qui en héritent.
 * */
export const eMotherClassMixin = {
    data: function () {
        return {
            Lang,
            LangJson,
        }
    },

    computed: {
        getUrl,
        getUserLangID,
        getUserID,
        getUserInfos,
        getVersionCSS,
        getIsTablet,
        getParamWindow,
        getInfoDate,
        getStorage,
        /**
         * permet de récupérer une resssources, le fileid et le ntab depuis le store.
         * */
        ...mapGetters({
            getFileId: "getFileId", getTab: "getTab", getLastUpdate: "getLastUpdate", getEvtid: "getEvtid", getPPid: "getPPid", getPMid: "getPMid", getFileValue: "getFileValue",
            getBaseUrl: "getBaseUrl", getTkStructPage: "getTkStructPage",
            getTkDataPage: "getTkDataPage", getTkStructBkm: "getTkStructBkm", getTkStructCat: "getTkStructCat",
            getMtxBookmarks: "getMtxBookmarks", getTooltipObj: "getTooltipObj", getBkmPage: "getBkmPage",
            getHostName: "getHostName", getBaseName: "getBaseName",
            getRevision: "getRevision", getFileMenu: "getFileMenu",getFileRoot: "getFileRoot",
        }),
        /** Propriété pour l'appel au back du menu du fichier */
        FileMenu: {
            get: function () {
                return this.getFileMenu;
            },
            set: function (value) {
                this.setFileMenu(value);
            }
        },
        /** Propriété qui encapsule la variable du chargement du mode Fiche guidé ; indique si un élément est en cours de chargement. Accessible de partout */
        Loading: {
            get: function () {
                return this.getFileLoading;
            },
            set: function (value) {
                this.setFileLoading(value);
            },
        },
    },

    methods: {
        setRunSpec,
        adjustDivContainerXRM,
        shFileInPopup,
        loadFile,
        deleteFile,
        printFile,
        reportList,
        openPurpleFile,
        goTabList,
        getRes,
        setUserBkmPref,
        setIrisUpdateValFromBkmModal,
        openLnkFileDialog,
        /**
         * Recupere la variable de la couleur de fond en CSS
         */
        getBackgroundCSSColor: function () {
            return getComputedStyle(document.documentElement)
                .getPropertyValue("--main-color");
        },

        /**
         * Recupere la variable de la couleur de la police en CSS
         */
        getForeCSSColor: function (state) {
            return getComputedStyle(document.documentElement)
                .getPropertyValue("--fore-color");
        },
        /** Fonction que l'on appelle pour récupérer la langue au format locale */
        getLangLocalize() {
            return this.Lang[this.getUserLangID];
        },
        /** Fonction que l'on appelle pour récupérer la langue. */
        initLangComponents: function () {
            return this.LangJson[this.getLangLocalize()];
        },
        /**
         * Toutes les mutations pour les variables du state.
         * */
        ...mapMutations({
            setnTab: "setnTab", setnEvtid: "setnEvtid", setnPPid: "setnPPid", setnPMid: "setnPMid", setFileId: "setFileId", setBaseUrl: "setBaseUrl", setFileValue: "setFileValue",
            setTkStructPage: "setTkStructPage", setTkDataPage: "setTkDataPage", setTkStructBkm: "setTkStructBkm",
            setTkStructCat: "setTkStructCat", setBaseName: "setBaseName",
            setMtxBookmarks: "setMtxBookmarks", setFileMenu: "setFileMenu", setTooltipObj: "setTooltipObj", setBkmPage: "setBkmPage", setRevision: "setRevision",setFileRoot: "setFileRoot"
        }),

        /**
         * Toutes les actions pour les mutations du state.
         * */
        ...mapActions({
            setnTab: "setnTab", setnEvtid: "setnEvtid", setnPPid: "setnPPid", setnPMid: "setnPMid", setFileId: "setFileId", setBaseUrl: "setBaseUrl", setFileValue: "setFileValue",
            setTkStructPage: "setTkStructPage", setTkDataPage: "setTkDataPage", setTkStructBkm: "setTkStructBkm",
            setTkStructCat: "setTkStructCat", setBaseName: "setBaseName",
            setMtxBookmarks: "setMtxBookmarks", setFileMenu: "setFileMenu", setTooltipObj: "setTooltipObj", setBkmPage: "setBkmPage", setRevision: "setRevision",setFileRoot: "setFileRoot"
        }),
    },
    errorCaptured(err, vm, info) {
        ErrorService.initErrorServiceVue(err, vm, info).ToString();
        return false;
    },
}