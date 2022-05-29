import {
    LoadStructPage,
    LoadDataPage,
    LoadStructBkm,
    LoadWzCatalog,
    ConstructHeaderFile,
    ConstructPropertyFile,
    ConstructCatalog,
    observeRightMenu,
    manageDatabaseIrisBlack,
    initDatabaseIrisBlack,
    initDatabase,
    specialCSS,
    forbiddenFormatHead,
    linkToCall,
    callbackFloatButton,
    getAuthorizedAdminUser,
    getUserAuth,
    linkToPost,
    setStatusNewFile
} from "../methods/eFileMethods.js?ver=803000";
import { getTabDescid } from "../methods/eMainMethods.js?ver=803000";
import { tabFormatForbidHeadEdit, summaryFldsMaxNb, tabFormatForbidPopup, relationFormats } from "../methods/eFileConst.js?ver=803000";
import { initIndexedDB, manageIndexedDB, setDataDbAsync, getDataDbAsync, mergeDataDbAsync, filterDataDbAsync, countDataDbAsync, firstDataDbAsync } from "../methods/eBkmMethods.js?ver=803000";
import { eFileMixin } from "../mixins/eFileMixin.js?ver=803000";
import { TableType, LangList, FieldType, UserLevel } from '../methods/Enum.min.js?ver=803000';
import EventBus from "../bus/event-bus.js?ver=803000";
import { loadIrisFileMenu, loadFileLayout, getColor, getParamWindow, getUserInfos, fnForceChckNwThm, getCurrentTab } from '../shared/XRMWrapperModules.js?ver=803000';

import { createJson } from "../methods/eDialogAdminMethods.js?ver=803000";
import { JSONTryParse } from "../methods/eMainMethods.js?ver=803000";

export default {
    name: "fiche",
    data() {
        return {
            stepInfoSkeleton: true,
            stepInfo: false,
            summaryGoSkeleton: true,
            summaryGo: false,
            detailsGoSkeleton: true,
            detailsGoShow: false,
            detailsGo: false,
            activityGoSkeleton: true,
            activityGo: false,
            sLocal: 'fr',
            iStepHeight: 4,
            iMultHeight: 30,
            iReduceHeight: 10,
            iMaxChamps: 99,
            timeoutCalculHeight: 200,
            wizardBarEmpty: false,
            activityComponent: true,
            activityIndex: 0,
            propertyFiche: [],
            showModal: false,
            showModalAlert: false,
            showMotherModal: false,
            optionsModal: Object,
            optionsModalGlobal: Object,
            optionsMotherModal: Object,
            stepInfoLoad: false,
            detailGo: false,
            grilles: [],
            DataJson: null,
            JsonSummary: Object,
            JsonWizardBar: Object,
            CanUpdateWizardBar: true,
            NbCols: 0,
            DataStruct: null,
            DataCatalogue: null,
            clickCount: 0,
            heigthContentTab: null,
            currentHeight: null,
            fid: this.getFileId,
            did: this.getTab,
            lastUpdate: this.getLastUpdate,
            layoutVueJsVariable: Object,
            tabAllDetail: [],
            tabHeader: Object,
            tabAside: [],
            showModalTooltip: false,
            opened: false,
            focus: false,
            HeaderHeight: 75,
            mainDivAdjusted: false,
            observer: null,
            counterReload: 0,
            dbActivity: null,
            asideWidth: [3, 0],
            asideX: [9, 12],
            activityExpanded: false,
            memoOpenedFullModeObj: {
                value: false,
                index: null
            },
            navTabsSticky: false,
            rightMenuWidth: null,
            tabsBarKey: 0,
            nForceRefreshTabsBar: 0,
            tabFormatForbidHeadEdit,
            tabFormatForbidPopup,
            tabletDisplay: false,
            endResizing: false,
            LangList,
            bDataLoaded: true,
            nbLvlHead: 0,
            dialog: true,
            openDialogSetting: false,
            floatingButtons: {
                align: 'left',
                alignVertical: '30vh',
                zIndex: 21,
                actions: [
                    {
                        text: this.getRes(1460),
                        icon: "mdi-check-circle-outline",
                        sizeIcon: "18",
                        colorBtn: this.getColor('activeNewFileMode', getCurrentTab()),
                        sClassBtn: "white--text elevation-0 px-3",
                        function: 'activeNewFileMode',
                        disabled: false,
                    },
                    {
                        text: this.getRes(3122),
                        icon: "mdi-eye-outline",
                        sizeIcon: "18",
                        colorBtn: this.getColor('activeNewFileModePreview', getCurrentTab()),
                        sClassBtn: "white--text elevation-0 px-3",
                        function: 'activeNewFileModePreview',
                        disabled: false,
                    },
                    {
                        text: this.getRes(1459),
                        icon: "mdi-close-circle-outline",
                        sizeIcon: "18",
                        colorBtn: this.getColor('desactiveNewFileMode', getCurrentTab()),
                        sClassBtn: "white--text elevation-0 px-3",
                        function: 'desactiveNewFileMode',
                        disabled: false,
                    },
                    {
                        text: this.getRes(1625),
                        icon: "mdi-cog",
                        sizeIcon: "16",
                        colorBtn: "blue-grey",
                        sClassBtn: "white--text elevation-0 px-3",
                        function: 'openDialogSetting',
                        disabled: false,

                    }
                ]
            },
            propStep: {},
            bDisplaySpinner: false,
            nOpacitySpinner: 0.9,
            sBackgroundSpinner: "background",
        };
    },
    mixins: [eFileMixin],
    components: {
        headFiche: () => import(AddUrlTimeStampJS("./fiche/headFiche.js")),
        stepsBar: () => import(AddUrlTimeStampJS("./fiche/stepsBar.js")),
        popupModale: () => import(AddUrlTimeStampJS("./modale/popupModale.js")),
        tabsBar: () => import(AddUrlTimeStampJS("./fiche/tabsBar.js")),
        alertModale: () => import(AddUrlTimeStampJS("./modale/alertModale.js")),
        tabsBarAside: () => import(AddUrlTimeStampJS("./fiche/tabsBarAside.js")),
        eFileLoader: () => import(AddUrlTimeStampJS("./fiche/FileLoader.js")),
        tooltipModale: () => import(AddUrlTimeStampJS("./modale/tooltipModale.js")),
        MotherOfAllModals: () => import(AddUrlTimeStampJS("./modale/MotherOfAllModals.js")),
        summarySkeleton: () => import(AddUrlTimeStampJS("./skeletons/summarySkeleton.js")),
        asideSkeleton: () => import(AddUrlTimeStampJS("./skeletons/asideSkeleton.js")),
        detailSkeleton: () => import(AddUrlTimeStampJS("./skeletons/detailSkeleton.js")),
        stepsBarSkeleton: () => import(AddUrlTimeStampJS("./skeletons/stepsBarSkeleton.js")),
        modalWrapper: () => import(AddUrlTimeStampJS("./modale/modalWrapper.js")),
        floatingButtons: () => import(AddUrlTimeStampJS("./floatingButtons/floatingButtons.js")),
        fileSettings: () => import(AddUrlTimeStampJS("./fiche/fileSettings.js")),
        eProgressSpinner: () => import(AddUrlTimeStampJS("./subComponents/eProgressSpinner.js"))
    },
    watch: {
        /** Si le skeleton disparait, on vérifie que l'on a enregsitré la position avant rafraîchissement  */
        async detailsGoSkeleton(val) {
            let getDataDb = await this.getDataDb();
            if (getDataDb?.arrPinnedBkmTab?.scrollPos && !val && val != undefined) {
                this.$refs?.mainContentWrap?.scrollTo({
                    top: getDataDb?.arrPinnedBkmTab?.scrollPos
                });
                this.mergeDataDbAsync(getDataDb?.dbBkm, "Bookmark", { bkm: 'details', nTab: parseInt(this.getTab), type: getDataDb?.arrPinnedBkmTab?.type });
            }
        }
    },
    computed: {
        getUserInfos,
        getAuthorizedAdminUser,
        getUserAuth,
        /**
         * Rajoute une classe CSS au conteneur principal si la fiche est en train de charger
        * */
        getLoadingClass: function () {
            return this.detailsGoSkeleton ? 'is-loading' : ''
        },
        /** Computed qui retourne les liaisons hautes de la fiche. */
        getFkLinks: function () {
            return this.DataStruct?.FdLinksId;
        },
        activiyAreaCssClass() {
            return this.activityExpanded ? 'activity-expanded' : this.activityComponent ? 'activityComponent' : ''
        },
        getStackingContext() {
            return this.navTabsSticky ? 'is-sticky' : ''
        },
        /** Positionne la tirette avec ou sans le menu de droite (rightMenuWidth)
         * Vu que la tirette passe en dessous de la scrollbar sur Safari, on rajoute 20px pour qu'elle soit entièrement visible
         * */
        getRightMenuWidth() {
            let safari = /Apple Computer/i.test(navigator.vendor);
            if (!safari)
                return `right:${0 + this.rightMenuWidth}px`;
            else
                return `right:${20 + this.rightMenuWidth}px`;
        },
        /** recupère les informations des liaisons pour la création via les signets */
        getLnkIdInfos() {

            if (!this.getFkLinks)
                return "";

            return `${TableType.PP}=${(this.getFkLinks["ParentPP"] || 0)};${TableType.PM}=${(this.getFkLinks["ParentPM"] || 0)};${TableType.ADR}=${(this.getFkLinks["ParentAdr"] || 0)};${this.getFkLinks["ParentEvtDescId"]}=${(this.getFkLinks["ParentEvt"] || 0)}`;
        },
        /** récupère le hash de la fiche en cours, pour le partage de fiche ou l'affichage de Infos Debug */
        getFileHash() {
            return this.DataStruct?.Structure?.StructFile?.FileHash;
        },
        /** Renvoie une classe css à la racine en fonction de la langue */
        getlangLocal() {
            //FR | EN | DE | NL | ES | IT | INTER
            return this.LangList[this.sLocal];
        },
        /** La fiche est-elle modifiable ? */
        getIsUpdatable: function () {
            return this.DataStruct?.Data?.RightIsUpdatable;
        },
        /** Les actions permises sur la fiche. */
        getActions: function () {
            return this.DataStruct?.actDetail;
        },
        /** Classes de la zone Notes/Aside */
        getAsideCssClass: function () {
            let cssClass = [];
            cssClass.push(this.getStackingContext);
            cssClass.push(this.activityComponent || this.tabletDisplay ?
                'p-visible col-3'
                : 'p-hidden col-3'
            );
            cssClass.push(this.detailsGoSkeleton ? 'no-transition' : '');
            return cssClass.toString().replaceAll(',', ' ');
        },
        /** Pour les besoins du scénario on retire les liaisons hautes (Parent Links)  */
        getTabAllDetailMinusPLinks: {
            get() {
                return this.tabAllDetail
                    ?.filter(tb => this.getTabDescid(tb.DescId) == this.getTab) || [];
            },
            async set(val) {
                this.tabAllDetail = val;
                await this.ConstructHeaderFile(this);
                this.nbLvlHead += 1;
            }
        },
        /** Renvoie le nom de l'onglet courant */
        getTabName: function () {
            return this.DataStruct?.Structure?.StructFile?.Label
        },
        /**
         * Renvoie les champs complémentaire, titres et sous-titres
         */
        getFileStgTitles: function () {
            let allowedTabFormat = Object.values(FieldType).filter(tab => !tabFormatForbidPopup.includes(tab));
            return this.getFileSettingsFields(allowedTabFormat);
        },
        /**
         * Renvoie les champs images
         */
        getFileStgAvatar: function () {
            let allowedTabFormat = [FieldType?.Image];
            return this.getFileSettingsFields(allowedTabFormat);
        },
        /**
         * Renvoie les catalogues sauf arbo
         */
        getFileStgSteps: function () {
            let allowedTabFormat = [FieldType?.Catalog];
            let condition = field => { return !field.IsTree };
            return this.getFileSettingsFields(allowedTabFormat, condition);
        },
        /** Renvoie les propriétés du paramétrage de la fiche (zone résumé, zone assistant) */
        getFileSettings: function () {
            let oContent = {
                col: 12,
                values: this.getFileStgTitles
            }

            let oProps = {
                type: 'fields',
                divider: true,
            }

            let oInputs = [];

            let tabHeaderCopy = Object.assign({}, this.tabHeader)

            tabHeaderCopy?.inputs?.slice(0, summaryFldsMaxNb)?.forEach(input => {
                oInputs.push(
                    {
                        ...oContent,
                        ...{
                            label: this.getRes(20),
                            value: this.getFileStgValue(input?.DescId)
                        }
                    }
                )
            })


            if (!tabHeaderCopy?.inputs || tabHeaderCopy?.inputs?.length < summaryFldsMaxNb) {
                for (i = 0; i < (summaryFldsMaxNb - (tabHeaderCopy?.inputs?.length || 0)); i++) {
                    oInputs.push(
                        {
                            ...oContent,
                            ...{
                                label: this.getRes(20),
                                value: {}
                            }
                        }
                    )
                }
            }

            return {
                content: [
                    {
                        name: 'header',
                        type: 'layout',
                        content: this.getRes(3137)
                    },
                    {
                        ...oProps,
                        ...{
                            name: 'summaryFields',
                            title: this.getRes(2425),
                            divider: false,
                            content: [
                                {
                                    ...oContent,
                                    ...{
                                        label: this.getRes(7216),
                                        value: this.getFileStgHead('title')
                                    }
                                },
                                {
                                    ...oContent,
                                    ...{
                                        label: this.getRes(7109),
                                        value: this.getFileStgHead('sTitle')
                                    }
                                },
                                {
                                    ...oContent,
                                    ...{
                                        label: this.getRes(6763),
                                        value: this.getFileStgHead('avatar'),
                                        values: this.getFileStgAvatar
                                    }
                                }
                            ]
                        }
                    },
                    {
                        ...oProps,
                        ...{
                            name: 'additionnalFields',
                            title: this.getRes(3135),
                            content: oInputs
                        }
                    },
                    {
                        ...oProps,
                        ...{
                            name: 'stepBarFields',
                            title: this.getRes(2426),
                            readOnly: !this.hasUpdateRight('stepBarFields'),                          
                            content: [
                                {
                                    ...oContent,
                                    ...{
                                        label: this.getRes(3136),
                                        value: this.getFileStgValue(this.JsonWizardBar?.DescId),
                                        values: this.getFileStgSteps
                                    }
                                }
                            ]
                        }
                    },
                    {
                        name: 'footer',
                        type: 'layout',
                        content: [
                            {
                                title: this.getRes(30),
                                action: 'close',
                                color: 'grey darken-1'
                            },
                            {
                                title: this.getRes(286),
                                action: 'save',
                                color: 'green darken-1'
                            }
                        ]
                    }
                ]
            }
        },
        /** retourne la Res en fonction du format */
        getTypeRes: function () {
            return {
                [FieldType?.AliasRelation]: this.getRes(8246),
                [FieldType?.Relation]: this.getRes(7299)
            }
        },
        /** Props pour afficher le spinner */
        getProgressSpinnerProps: function () {
            return {
                nOpacity: this.nOpacitySpinner,
                sBackgroundColor: this.sBackgroundSpinner,
            };
        },
    },
    props:{
        fileRoot:Object
    },
    template: `
    <div
        v-if="GotNewData()"
        v-bind:class="[showModal ? 'hiddenOver' : '', getlangLocal, getLoadingClass] "
        v-on:scroll=" stikyNav($event)"
        ref="mainContentWrap"
        id="mainContentWrap"
        class="col-md-12 pr-2 pl-6"
        :hsh="getFileHash"
    >
        <eProgressSpinner :oTblItm="getProgressSpinnerProps" :display="bDisplaySpinner" @setWaitIris="setWaitIris" />
        <floatingButtons :prop-tab="getTab" v-if="getUserAuth" @callback="callbackFloatButton" :props-floating-button="floatingButtons" />
        <modalWrapper
        v-if="getAuthorizedAdminUser"      
        width="1100px"     
        v-model="openDialogSetting">
            <template v-slot:content>
                <fileSettings
                    :objItm="getFileSettings" 
                    @close="closeFileSettings" 
                    @save="saveFileSettings" 
                />
            </template>
        </modalWrapper>

        <input type="hidden" :id="'lnkid_' + getTab" :ref="'lnkid_' + getTab" :value="getLnkIdInfos" />
        <div ref="propertyFiche" @click="emitMethod();" id="propertyFiche">
            <div>
                <i class="fas fa-user-tag"></i>
                <span class="falseA">{{getRes(54)}}</span>
            </div>
        </div>
        <template>
            <headFiche 
                v-if="!summaryGoSkeleton" 
                :property-fiche="this.propertyFiche" 
                ref="headFiche" 
                :prop-go="this.summaryGo" 
                :DataStruct="DataStruct" 
                :prop-head="tabHeader" 
                :IsUpdatable="getIsUpdatable" 
                :oFileActions="getActions" 
                :key="'lvl_' + nbLvlHead"
            />
            <summarySkeleton v-else :summary-props="JsonSummary" />
        </template>
        <div class="box no-border">
    <v-container fluid class="px-0 overflow-visible">
    <v-row>
        <div ref="gridContainer" id="grid-container" class="row grid-stack outer grid-stack-12">
            <v-col
                v-show="Object.keys(propStep).length"
                ref="gridStepline"
                id="grid-stepline"
                class="col-12"
            >
                <div
                    v-if="!stepInfoSkeleton"
                    class="grid-stack-item-content"
                >
                    <div v-bind:class="[wizardBarEmpty ? 'emptyDataInputContent' : '', 'box box-primary box-profile box-body box_body_acc_20']">
                        <stepsBar
                            :prop-wizard-bar-empty="wizardBarEmpty"
                            :prop-detail.sync="getTabAllDetailMinusPLinks"
                            :JsonWizardBar="JsonWizardBar"
                            :propStep="propStep"
                            :nCounterReload.sync="counterReload"
                            @getReloadCatalog="getReloadCatalog"
                            @setWaitIris="setWaitIris"
                        />
                    </div>
                </div>
                <stepsBarSkeleton
                    v-else
                    :steps-props="JsonWizardBar"
                />
            </v-col>
            <v-col @transitionend.self="areaResizingEnd($event)" ref="gridTabs" id="grid-tabs" :class="[activityComponent ? 'col-9' : 'col-12', detailsGoSkeleton ?  'no-transition':''] ">
                <div class="grid-stack-item-content content-tabs-main">
                    <div class="box box-body box_body_acc_not box_body_acc_tabs signet_tabs">
                        <tabsBar
                            :tabsBarKey="tabsBarKey"
                            @displayDetailSkeleton="displayDetailSkeleton"
                            :isDisplayed="!detailsGoSkeleton"
                            :endResizing.sync="endResizing" 
                            :nForceRefreshTabsBar.sync="nForceRefreshTabsBar"
                            :navTabsSticky="navTabsSticky" 
                            ref="tabsBar"
                            :prop-detail.sync="getTabAllDetailMinusPLinks"
                            :prop-cols="NbCols"
                            @repositionBkm="repositionContent"
                            @saveScrollPosition="saveScrollPosition"
                            @setWaitIris="setWaitIris"
                        />
                    </div>
                </div>
                <detailSkeleton v-show="detailsGoSkeleton" />
            </v-col>
            <v-col
                ref="asideTabs"
                id="aside-tabs"
                :class="getAsideCssClass"
            >
                <div v-if="getDisplayActivity()" class="grid-stack-item-content content-tabs-main">
                    <div class="box box-body box_body_acc_not box_body_acc_tabs">
                        <tabsBarAside
                            :tabletDisplay="tabletDisplay"
                            :btnDrawerInlineStyle="getRightMenuWidth"
                            :navTabsSticky="navTabsSticky"
                            :memoOpenedFullModeObj="memoOpenedFullModeObj"
                            @iconClick="openMemoFullSize"
                            :class="[activiyAreaCssClass,'d-flex justify-end align-start']"
                            @activityClosed="activityExpanded = $event;"
                            :bActivityComponent="activityComponent"
                            :iBookmark="activityIndex"
                            @update:bActivityComponent="setActivityComponent"
                            id="aside-tabs-children"
                            ref="asideTabsChildren"
                            :prop-tab-aside="tabAside"
                        />
                    </div>
                </div>
                <asideSkeleton v-if="detailsGoSkeleton" :aside-props="tabAside" />
            </v-col>
        </div>
    </v-row>
    </v-container fluid>
</div>
        <tooltipModale
            v-if="getTooltipObj.visible && getTooltipObj.elem != 'bkmScroll'"
            @closeTooltipModale="showModalTooltip = false"
            :prop-options-modal="getTooltipObj"
        />
        <popupModale :prop-options-modal="this.optionsModalGlobal" v-if="showModal" @close="showModal = false" ></popupModale>
        <alertModale :prop-options-modal="this.optionsModal" v-if="showModalAlert" @close="showModalAlert = false" ></alertModale>
        <MotherOfAllModals :prop-options-modal="this.optionsMotherModal" v-if="showMotherModal" @close="showMotherModal = false" />
    </div>
`,
    methods: {
        observeRightMenu,
        LoadStructPage,
        LoadDataPage,
        LoadWzCatalog,
        LoadStructBkm,
        ConstructHeaderFile,
        ConstructPropertyFile,
        ConstructCatalog,
        initIndexedDB,
        manageIndexedDB,
        setDataDbAsync,
        getDataDbAsync,
        mergeDataDbAsync,
        filterDataDbAsync,
        countDataDbAsync,
        firstDataDbAsync,
        manageDatabaseIrisBlack,
        initDatabaseIrisBlack,
        initDatabase,
        specialCSS,
        forbiddenFormatHead,
        getTabDescid,
        loadIrisFileMenu,
        loadFileLayout,
        linkToCall,
        linkToPost,
        createJson,
        setStatusNewFile,
        getColor,
        callbackFloatButton,
        fnForceChckNwThm,
        /** Verifie si on a obtenu les informations actualisées du store. Si ça n'est pas le cas, on les demande */
        GotNewData: async function () {
            if (this.getFileId != this.fid || this.getLastUpdate != this.lastUpdate) {
                await this.getData();
                return true;
            }
            else
                return true;
        },
        /** Initialisation de la base de donnée locale avec le signet en cours.
         * Si c'est la première fois qu'on visite cet onglet, alors, rien.
         * Sinon, on reprend les signets précédemment enregistrés. */
        initLocalDatabase: async function () {
            try {
                let oSearch = { nTab: parseInt(this.getTab) };
                this.sLocal = this.getUserLangID;

                this.dbActivity = await this.initDatabaseIrisBlack();

                let activityDBCount = await this.countDataDbAsync(this.dbActivity, "Activity", oSearch);

                if (activityDBCount < 1)
                    await this.mergeDataDbAsync(this.dbActivity, "Activity", { nTab: parseInt(this.getTab), active: this.activityComponent, index: this.activityIndex });

                let dtDb = await this.firstDataDbAsync(this.dbActivity, "Activity", oSearch);

                if (dtDb) {
                    this.activityComponent = dtDb.active;
                    this.activityIndex = dtDb.index;
                }
            }
            catch (e) {
                this.manageDatabaseIrisBlack(e);
            }
        },
        /**
         * Permet à partir de l'id de savoir si on affiche la zone Grille ou non
         * @param {any} IdGrid
         */
        getDisplayActivity(IdGrid) {
            return this.tabAside.length > 0
        },
        /**
         * Permet à partir de l'id de savoir si on affiche la zone signet ou non
         * @param {any} IdGrid
         */
        getDisplaySignet(IdGrid) {
            return !this.detailsGoSkeleton
        },

        /**
         * On fait remonter la modification de l'affichage de l'activité.
         * Et on l'enregistre en base.
         * @param {any} bActivity
         */
        setActivityComponent: async function (bActivity, iIdxActivity) {
            await this.mergeDataDbAsync(this.dbActivity, "Activity", { nTab: parseInt(this.getTab), active: bActivity, index: iIdxActivity });
            this.activityComponent = bActivity;
            this.activityExpanded = bActivity;
            //this.displayActivityComponent(bActivity);
            this.tabsBarKey += 1;

        },
        /**
         * Affichage du composant activité, suivant ce qui est dans IndexedDB.
         * @param {any} bActivity
         */
        displayActivityComponent: function (bActivity) {
            let aside = this.$refs["asideTabs"]?.find(n => n);
            let gridTabs = this.$refs["gridTabs"]?.find(n => n);

            if (aside) {
                aside.setAttribute("data-gs-x", bActivity ? this.asideX[0] : this.asideX[1]);
                aside.setAttribute("data-gs-width", bActivity ? this.asideWidth[0] : this.asideWidth[1]);
                aside.setAttribute("data-gs-min-width", bActivity ? this.asideWidth[0] : this.asideWidth[1]);
            }

            if (gridTabs) {
                gridTabs.setAttribute("data-gs-width", bActivity ? this.asideX[0] : this.asideX[1]);
            }
        },
        /**
         * Permet de sticky la nav
         * A priori, bloque les annexes sur le coté en position sticky.
         * @param {any} e un événement.
         */
        stikyNav: async function (e) {
            let iHeightBasTop = -35;
            let marginTabNav = 2;

            if (!(this.$refs["tabsBar"] && this.$refs["gridTabs"] && this.$refs["headFiche"]))
                return;

            let tabsNav = this.$refs["tabsBar"].$refs["tabsNav"];
            let aside = this.$refs["asideTabs"];

            if (!tabsNav) return false;

            let nav = tabsNav;
            if (aside)
                aside = aside.children[0]

            let gridTabs = this.$refs["gridTabs"];
            let headFiche = this.$refs["headFiche"].$el;
            let baseTop = gridTabs.offsetTop + headFiche.clientHeight + iHeightBasTop;
            let topNav = e.target.scrollTop - baseTop;

            nav.classList.toggle("stickyNavSignet", topNav >= 0);
            this.navTabsSticky = topNav >= 0;


            /*ELAIZ- position de asideTab et TabsBar en fonction de la taille de Headfiche 
                  En effet Heafiche est plus grande si elle comporte des catalogues en sous-titres
                  HeaderHeight = taille du menu XRM
                  */
            nav.style.top = "";
            if (aside)
                aside.style.top = "";

            if (topNav >= 0) {
                let headFicheHeight = await this.$refs["headFiche"].$el.offsetHeight;
                nav.style.top = headFicheHeight + this.HeaderHeight + this.marginTabNav + "px";
                let mainHeight = await this.$refs["mainContentWrap"].offsetHeight;
                if (aside) {
                    aside.style.top = headFicheHeight + "px";
                    aside.style.height = `calc( ${mainHeight - headFicheHeight - 40}px)`;
                }
            }
        },

        /**
         * permet d'aligner à droite la div princiapl avec une transition (?)
         * @param {any} mainDiv la div à aligner avec une transition
         * @param {any} e la div contenant la classe FavLinkOpen
         */
        FavLinkOpenAlign(mainDiv, e) {
            // #82 545
            if (!this.mainDivAdjusted) {
                adjustDivContainer();
                this.mainDivAdjusted = true;
            }
        },
        getHeight(options) {

            if (
                !(
                    this.$refs.tabsBar && this.$refs.tabsBar.length > 0
                ) /* || !(this.$refs.gridStepline && this.$refs.gridStepline.length > 0) || !(this.$refs.asideTabs && this.$refs.asideTabs.length > 0)*/
            )
                return;

            if (
                !(
                    this.$refs.tabsBar[0].$refs.details &&
                    this.$refs.tabsBar[0].$refs.details.length > 0
                ) ||
                !(
                    this.$refs.tabsBar[0].$refs.signet &&
                    this.$refs.tabsBar[0].$refs.signet.length > 0
                )
            )
                return;

            let grid;
            let bGridlActif;

            let details = this.$refs.tabsBar[0].$refs.details[0];
            let signet = this.$refs.tabsBar[0].$refs.signet[0];
            if (options && options.gridId) {
                grid = this.$refs.tabsBar[0].$refs[options.gridId][0];
                bGridlActif = grid.classList.contains("active");
            }
            let gridStep;
            if (this.$refs.gridStepline) {
                gridStep = this.$refs.gridStepline[0];
            }
            let asideTabs;
            if (this.$refs.asideTabs && this.$refs.asideTabs.length > 0) {
                asideTabs = this.$refs.asideTabs[0];
            }

            if (!(details && signet)) return;

            // #82 905 - Lorsqu'on recalcule la hauteur des signets, on repositionne le scroll sur le signet choisi par l'utilisateur dans la liste
            if (this.$refs.tabsBar[0].previousBkm != null && this.$refs.tabsBar[0].currentBkm != null && this.$refs.tabsBar[0].bkmDBCurrent.type == "signet") {
                this.$refs.tabsBar[0].scrollIn(null, this.$refs.tabsBar[0].currentBkm);
                this.$refs.tabsBar[0].currentBkm = null;
                this.$refs.tabsBar[0].previousBkm = null;
            }

            /*
            if (this.tabAside.length > 0)
                this.displayActivityComponent(this.activityComponent);
            */
        },
        /** Positionnement des éléments dans Gridstack (on set le grid ou je ne sais plus quoi...) */
        setGridPositionning: async function () {

            let fldById = this.JsonWizardBar.FieldsById;
            this.stepInfoSkeleton = false;

            //let iDataGsY = 3;
            if (!fldById || fldById.length < 1 || (this.DataStruct.Data.LstDataFields.findIndex(fld => fld.DescId == this.JsonWizardBar.DescId) < 0)) {
                this.stepInfoLoad = false;
                return;
            }

            this.stepInfoLoad = false;
        },
        /**
         * Initialisation de tabAllDetail qui servira dans toute l'application.
         * @param {any} FieldType
         */
        initTabAllDetail: function (FieldType) {
            // Verifie si la valeur de tabAllDetail est première fois ou le mise, peut-être que cela causera le problème si la valeurs modifier par back, voir apres
            // Suppression des doublons
            this.tabAllDetail = [...new Map([...this.DataStruct.Structure.LstStructFields, ...this.tabAllDetail].map(item => [item.DescId, item])).values()];
                
            let parentLinks = this.DataStruct.Data.LstDataFields.filter(
                (f) => this.getTab != this.getTabDescid(f.DescId) && f.DescId % 100 == 1
            );

            this.tabAllDetail.forEach((a, idx) => {
                let findItemData = this.DataStruct.Data.LstDataFields.find(
                    (b) => b.DescId == a.DescId
                );
                let objSetting = null;

                //ne pas rajouter cette condition dans le switch suivant car ceci doit être exécuté avant le switch
                if (a.Format == FieldType.Alias) {
                    objSetting = { ...findItemData, ...a.AliasSourceField, ...a };
                    this.tabAllDetail[idx].Format = a.AliasSourceField.Format;
                } else objSetting = { ...a, ...findItemData };

                switch (a.Format) {
                    case FieldType.Date:
                        ///saisie par daterangepicker

                        //retrouver la date de fin
                        if (objSetting.DateEndDescId > 0) {
                            var dateEndItem = this.DataStruct.Data.LstDataFields.find(
                                (b) => b.DescId == a.DateEndDescId
                            );
                            //si on ne trouve pas le champs dans la liste, l'annulation suivante annule le daterangepicker
                            if (dateEndItem == null || dateEndItem.ReadOnly) {
                                objSetting.DateEndDescId = 0;
                            } else {

                                objSetting.DateEndValue = dateEndItem.Value;
                                objSetting.DateStartValue = objSetting.Value;
                            }
                        }
                        //retrouver la date de début
                        if (objSetting.DateStartDescId > 0) {
                            var dateStartItem = this.DataStruct.Data.LstDataFields.find(
                                (b) => b.DescId == a.DateStartDescId
                            );
                            //si on ne trouve pas le champs dans la liste, l'annulation suivante annule le daterangepicker
                            if (dateStartItem == null || dateStartItem.ReadOnly) {
                                objSetting.DateStartDescId = 0;
                            } else {
                                objSetting.DateStartValue = dateStartItem.Value;
                                objSetting.DateEndValue = objSetting.Value;
                            }
                        }

                        break;
                    case FieldType.AliasRelation:
                    case FieldType.Relation:
                        //Rubriques parentes
                        objSetting.ParentLinks = parentLinks;

                        //Rubriques associées
                        let RelationFieldDescId = 0;
                        if (a.Format == FieldType.AliasRelation)
                            RelationFieldDescId = a.TargetTab;
                        else RelationFieldDescId = a.DescId;

                        if (RelationFieldDescId > 0) {
                            //je dois récupérer dans datafield
                            //les champs qui le bon aliasparam dans datastructure
                            let getAssociateFieldDescId = (structf) => {
                                if (structf.AssociateField == null) return false;

                                let match = structf.AssociateField.match(/\[(\d+)\]_\[(\d+)\]/);

                                if (
                                    match != null &&
                                    match.length > 2 &&
                                    match[1] == RelationFieldDescId
                                ) {
                                    structf.AssociateFieldDescId = match[2];
                                    return structf;
                                }
                            };

                            let setAssociateFieldDescId = (datafield, structFields) => {
                                let structField = structFields.find(
                                    (structf) => structf.DescId == datafield.DescId
                                );
                                if (structField == null) return null;

                                datafield.AssociateFieldDescId =
                                    structField.AssociateFieldDescId;
                                return datafield;
                            };

                            let structFields = this.DataStruct.Structure.LstStructFields.map(
                                (structf) => getAssociateFieldDescId(structf)
                            ).filter((structf) => structf != null);

                            let dataFields = this.DataStruct.Data.LstDataFields.map(
                                (datafield) => setAssociateFieldDescId(datafield, structFields)
                            ).filter((datafield) => datafield != null);
                            //structf.DescId == datafield.DescId
                            //on transmet au datainput relations les datafield correspondant aux rubriques associées qui en dépendent
                            objSetting.AssociateFields = dataFields;
                        }

                        break;
                    case FieldType.Catalog:
                        let bndMyCat = this.tabAllDetail.find(tb => tb.DescId == a.BoundDescId);
                        if (bndMyCat)
                            objSetting.Pdbv = bndMyCat.Value;
                        break;
                    default:
                }

                Vue.set(this.tabAllDetail, idx, objSetting);
            });


            //on taggue les champs en auto completion
            //Sirene
            if (this.DataStruct.SireneMapping?.Triggers?.length > 0) {
                this.tabAllDetail
                    .filter(f => this.DataStruct.SireneMapping.Triggers.some(i => i == f.DescId))
                    .forEach(f => f.AutoComplete = true);
            }
            //adresse predictive
            if (this.DataStruct.PredictiveAddressMapping?.Triggers?.length > 0) {
                this.tabAllDetail
                    .filter(f => this.DataStruct.PredictiveAddressMapping.Triggers.some(i => i == f.DescId))
                    .forEach(f => f.AutoComplete = true);
            }
        },
        /** Initialisation du positionement des éléments. */
        initGridAside: async function () {

            let iNotes = 94;
            let icoNotes = { IconIn: "fas fa-outdent", IconOut: "fas fa-indent" };
            let iDescription = 89;
            let icoDescription = { IconIn: "fas fa-file-alt", IconOut: "fas fa-file-alt" };
            let iDataGsWidth = 12;
            this.tabAside = [];

            let note = this.tabAllDetail.find(a => a.DescId == parseInt(this.did) + iNotes);

            if (note != undefined && note.IsVisible) {
                this.tabAside.push(Object.assign(note, icoNotes));
            }

            let description = this.tabAllDetail.find(a => a.DescId == parseInt(this.did) + iDescription);
            if (description != undefined && description.IsVisible) {
                this.tabAside.push(Object.assign(description, icoDescription));
            }

            this.grilles = [];
            //if (this.DataJson.Activity && this.tabAside.length != 0) {
            if (this.tabAside.length > 0) {
                this.asideWidth[0] = this.DataJson.ActivityArea?.DataGsWidth;
                this.asideX[0] = this.DataJson.ActivityArea?.DataGsX;

                this.DataJson.WizardBarArea.DataGsHeight = 10;
                this.DataJson.DetailArea.DataGsY = 10;
                this.DataJson.ActivityArea.DataGsY = 10;

                this.grilles.push(this.DataJson?.WizardBarArea, this.DataJson?.DetailArea, this.DataJson?.ActivityArea);
            } else {
                this.DataJson.DetailArea.DataGsWidth = iDataGsWidth;
                this.activityComponent = false;
                await this.mergeDataDbAsync(this.dbActivity, "Activity", { nTab: parseInt(this.getTab), active: this.activityComponent });
                this.grilles.push(this.DataJson?.WizardBarArea, this.DataJson?.DetailArea);
            }

            this.detailsGoSkeleton = false;

            this.activityGoSkeleton = false;
        },

        async getData(options) {

            this.bDataLoaded = false;

            this.detailsGoSkeleton = options?.reloadDetail; // skeleton de la zone détail
            this.summaryGoSkeleton = options?.reloadHead; // skeleton de la zone résumé
            this.stepInfoSkeleton = options?.reloadAssistant; // skeleton de l'assistant
            this.detailsGoShow = !options?.reloadAll; // tous les skeletons

            let tabPromises = [];
            window.scroll(0, 0);

            if (!options) {
                this.propertyFiche = [];

                this.stepInfoSkeleton = true; // Indique au template de ne pas se rafraîchir tant que setGridPositionning() n'a pas fini. Alors on affiche le skeleton 

                this.summaryGoSkeleton = true; // Indique au template de ne pas se rafraîchir tant que ConstructHeaderFile() n'a pas fini. Alors on affiche le skeleton 

                this.activityGoSkeleton = true;  // Indique au template de ne pas se rafraîchir tant que initGridAside() n'a pas fini. Alors on affiche le skeleton 

                this.detailsGoSkeleton = true;  // Indique au template de ne pas se rafraîchir tant que initGridAside() n'a pas fini. Alors on affiche le skeleton 

                this.detailsGoShow = false;

                this.fid = this.getFileId;
                this.did = this.getTab;
                this.lastUpdate = this.getLastUpdate;
                this.tabHeader = {};
                this.tabStep = {};
                this.tabAllDetail = [];


            }

            let arLstdata = this.DataStruct?.Data?.LstDataFields;

            try {
                await Promise.all([
                    this.LoadStructPage(),
                    this.LoadDataPage(),
                ]);
            } catch (e) {
                EventBus.$emit("globalModal", {
                    typeModal: "alert",
                    color: "danger",
                    type: "zoom",
                    close: true,
                    maximize: false,
                    id: "alert-modal",
                    title: this.getRes(6576),
                    msgData: e,
                    width: 600,
                    btns: [{ lib: this.getRes(30), color: "default", type: "left" }],
                    datas: this.getRes(7050),
                });

                return;
            }

            if (arLstdata)
                this.DataStruct.Data.LstDataFields = this.DataStruct.Data.LstDataFields.map((data, idx) => {
                    return { ...data, ...arLstdata[idx] }
                });

            this.setBaseUrl(this.DataStruct.Structure.WebDataPath);
            this.setBaseName(this.DataStruct.BaseName);
            this.setRevision(this.DataStruct.Revision);


            this.bDataLoaded = true;

            let dtFields = this.DataStruct?.Data?.LstDataFields;
            let FdLinks = this.DataStruct?.FdLinksId;

            this.setFileValue(this.DataStruct?.Data?.MainFileLabel);

            if (FdLinks) {
                let FdLinksEvt = FdLinks?.ParentEvt;
                let FdLinksPP = FdLinks?.ParentPP;
                let FdLinksPM = FdLinks?.ParentPM;
                let dtFieldEvt = dtFields?.find(n => n.DescId == FdLinks?.ParentEvtDescId + 1)?.DisplayValue || "";
                let dtFieldPP = dtFields?.find(n => n.DescId == 201)?.DisplayValue || "";
                let dtFieldPM = dtFields?.find(n => n.DescId == 301)?.DisplayValue || "";

                if (FdLinks?.ParentEvtDescId) {
                    this.setnEvtid({ descid: FdLinks?.ParentEvtDescId, value: FdLinksEvt, displayvalue: dtFieldEvt });
                }

                this.setnPPid({ descid: 200, value: FdLinksPP, displayvalue: dtFieldPP });
                this.setnPMid({ descid: 300, value: FdLinksPM, displayvalue: dtFieldPM });
            }


            await this.initLocalDatabase();

            let { FieldType, EdnType } = await import(AddUrlTimeStampJS("../methods/Enum.min.js"));
            this.initTabAllDetail(FieldType);
            this.initGridAside();

            // Rafraîchissement des MRU
            var oeParam = getParamWindow();
            if (oeParam.RefreshMRU)
                oeParam.RefreshMRU(this.getTab);

            tabPromises.push(await this.ConstructHeaderFile(this));
            tabPromises.push(await this.ConstructPropertyFile(EdnType));
            tabPromises.push(this.setGridPositionning());
            this.counterReload++;

            if (options?.reloadHead || options?.reloadAll)
                this.nbLvlHead += 1;

            if (this.openDialogSetting)
                this.closeFileSettings();

            try {
                await Promise.all(tabPromises);


            } catch (e) {
                EventBus.$emit("globalModal", {
                    typeModal: "alert",
                    color: "danger",
                    type: "zoom",
                    close: true,
                    maximize: false,
                    id: "alert-modal",
                    title: this.getRes(6576),
                    msgData: e,
                    width: 600,
                    btns: [{ lib: this.getRes(30), color: "default", type: "left" }],
                    datas: this.getRes(7050),
                });
            }

            await Vue.nextTick();
            this.setWaitIris(false);

            //this.getHeight(EventBus.$emit("loadFinish", options));
        },
        /**
         * METHOD POUR SET LES OPTIONS DU MODAL (PROPRIETE DE LA FICHE)
         * @param {any} options un objet contenant les options pour la modale.
         */
        setModalGlobal(options) {
            if (options.typeModal == "alert") {
                this.showModalAlert = true;
                this.optionsModal = options;
            } else {
                this.showModal = true;
                this.optionsModalGlobal = options;
            }
        },
        setModal(options) {
            this.optionsModal = options;
            if (options.typeModal == "alert") {
                this.showModalAlert = true;
            } else if (options.typeModal == "tooltip") {
                //this.showModalTooltip = options.visible || this.opened;
                this.showModalTooltip = store.state.tooltipObj.visible;
            }
        },

        /**
         * METHOD POUR EMIT LES OPTIONS DU MODAL (PROPRIETE DE LA FICHE)
         * */
        async emitMethod() {
            let options = {
                typeModal: "info",
                type: "zoom",
                close: true,
                maximize: true,
                id: "prop-fiche",
                observeMenu: (bVal, ctx) => {
                    this.observeRightMenu(bVal, ctx)
                },
                rightMenuWidth: window.GetRightMenuWidth(),
                title: this.getRes(54),
                btns: [{ lib: this.getRes(30), color: "default", type: "left" }],
                datas: this.propertyFiche,
            };

            EventBus.$emit("globalModal", options);
        },
        openMemoFullSize(id) {
            this.memoOpenedFullModeObj = { value: !this.memoOpenedFullModeObj.value, index: id }
        },
        getResizeContent() {
            const myObserver = new ResizeObserver(entries => {
                entries.forEach(entry => {
                    //on vérifie si la largeur de la fenêtre passe en dessous de 1000px pour mettre la zone note en dessous et masquer la tirette
                    this.checkDisplayMode(entry.contentRect);

                    this.adjustDivContainerXRM();
                });
            });
            const someEl = document.querySelector('#container.contentMaster');
            myObserver.observe(someEl);
        },
        /**
         * Vérifie le mode d'affichage ( tablette, mobile). Il n'y a que le format tablette qui est vérifié pour l'instant
         * @param {any} window objet qui contient les props de la fenêtre (width, height)
         */
        checkDisplayMode(window) {
            if (!this.tabletDisplay && window.width < 1000)
                this.tabletDisplay = true;
            else if (this.tabletDisplay && window.width > 1000)
                this.tabletDisplay = false;
        },
        /** Détecte la fin du resizing de la zone détail quand on affiche ou masque la zone note
         * @param {any} $event 
         */
        areaResizingEnd(evt) {
            this.endResizing = true;
        },
        /** Affiche ou non le skeleton détail */
        displayDetailSkeleton(bVal) {
            this.detailsGoSkeleton = bVal;
        },
        /**
        * reposition the tabsbar after pinned Bkm reloaded
        **/
        repositionContent() {
            let contentWrap = this.$refs?.mainContentWrap;
            let gridTabs = this.$refs?.gridTabs;
            if (contentWrap.scrollTop && gridTabs.offsetTop) {
                contentWrap.scrollTop = gridTabs.offsetTop - 30;
            }
        },
        /** Récupère les données de l'onglet details dans indexdb */
        async getDataDb() {
            let dbBkm = await this.initDatabaseIrisBlack();
            let savedPinnedBkmTab = await this.getDataDbAsync(dbBkm, 'Bookmark');
            let arrPinnedBkmTab = await savedPinnedBkmTab?.toArray();
            return {
                arrPinnedBkmTab: arrPinnedBkmTab[0],
                dbBkm: dbBkm
            };
        },
        /** Enregistre dans indexDb la position du scroll avant mise à jour et destroy de fiche.js */
        async saveScrollPosition() {
            let contentWrap = this.$refs?.mainContentWrap;
            let getDataDb = await this.getDataDb();
            this.mergeDataDbAsync(getDataDb?.dbBkm, "Bookmark", { bkm: 'details', nTab: parseInt(this.getTab), type: getDataDb?.arrPinnedBkmTab?.type, scrollPos: contentWrap?.scrollTop });
        },
        /**
         * Permet de trier les champs
         * @param {*} first premier élément du tableau à comparer
         * @param {*} sec second élément du tableau à comparer
         * @returns 
         */
        sortBy: function (first, sec, key) {
            first = first?.[key]?.toUpperCase();
            sec = sec?.[key]?.toUpperCase();
            if (first < sec) {
                return -1;
            }
            if (first > sec) {
                return 1;
            }

            return 0;
        },
        /**
         * Permet de trier les champs par leur type
         * @param {*} first premier élément du tableau à comparer
         * @param {*} sec second élément du tableau à comparer
         * @returns 
         */
        sortFieldsByType: function (list) {
            let filtered = list?.filter(li => li?.type == this.getTabName);
            let filter = list?.filter(li => li?.type != this.getTabName);
            filter?.forEach(filt => filtered.push(filt));
            return filtered;
        },
        /**
         * Permet de trier les champs par leur libellé
         * @param {*} first premier élément du tableau à comparer
         * @param {*} sec second élément du tableau à comparer
         * @returns 
         */
        sortFieldsByName: function (first, sec) {
            return this.sortBy(first, sec, 'text');
        },
        /**
         * Renvoie les champs possibles en fonction du format
         * @param {*} allowedTabFormat format autorisé
         * @param {*} condition condition supplémentaire si nécéssaire
         * @returns 
         */
        getFileSettingsFields: function (allowedTabFormat, condition = () => true) {
            let fileSettings = this.tabAllDetail?.filter(field => allowedTabFormat?.includes(field.Format) && condition(field));
            let arrFileSettings = [];
            fileSettings.forEach(field => {
                let type;
                if (getTabDescid(field?.DescId) != this.getTab
                    && field?.DescId % 100 == 1
                    && !relationFormats.includes(field?.Format)) {
                    type = this.getRes(3138)
                } else if (field?.TargetTab != this.getTab
                    && relationFormats.includes(field?.Format)) {
                    type = this.getTypeRes[field?.Format];
                } else if (getTabDescid(field?.DescId) == this.getTab) {
                    type = this.getTabName;
                } else {
                    type = "Hidden"
                }

                let tabLabel = field?.TargetTabLabel || this.getTabName;
                if (type != "Hidden") {
                    arrFileSettings.push({
                        text: `${tabLabel}.${field?.Label}`,
                        val: field?.DescId,
                        type: type
                    })
                }
            })
            arrFileSettings.sort(this.sortFieldsByName);
            arrFileSettings = this.sortFieldsByType(arrFileSettings);

            let arrStgCopy = arrFileSettings;
            arrStgCopy.forEach((settings, id) => {
                if (settings?.type != this.getTabName
                    && arrFileSettings.filter(arr => arr?.header && arr?.header == settings?.type).length < 1
                ) {
                    arrFileSettings.splice(id, 0, {
                        header: settings?.type
                    });
                }
            })
            return arrFileSettings;
        },
        /** Récupère les valeurs actuels de la zone résumé
        * @param {any} elem avatar,stitre ou titre
        *  */
        getFileStgHead: function (elem, idx) {
            let tabLabel = this.tabHeader[elem]?.TargetTabLabel || this.getTabName;
            let label = elem != 'inputs' ? this.tabHeader[elem]?.Label : this.tabHeader[elem][idx]?.Label;
            return {
                text: `${tabLabel}.${label}`,
                val: this.tabHeader[elem]?.DescId
            };
        },
        /**
         * Renvoie la valeur actuelle paramétrée dans le json pour les champs de la zone résumé / assistant
         * @param {*} descId 
         * @returns 
         */
        getFileStgValue: function (descId) {
            return {
                text: `${this.getTabName}.${this.tabAllDetail?.find(tab => tab.DescId == descId)?.Label}`,
                val: parseInt(descId)
            };
        },
        /**
        * Cette fonction consiste à récupérer du back les informations du menu.
        */
        getMenus: function () {

            if (this.getFileId != this.fid || this.getLastUpdate != this.lastUpdate) {
                let oDataFileMenu = this.loadIrisFileMenu();

                let param = {
                    DescId: Number(this.getTab),
                    FileId: Number(this.getFileId)
                };

                this.FileMenu = this.linkToCall(
                    oDataFileMenu.url,
                    { ...oDataFileMenu.params, ...param }
                );
            }
        },
        /** Ferme la modale */
        closeFileSettings: function () {
            this.openDialogSetting = false;
        },
        /** Enregistre les nouveaux params du mode fiche
        * @param {any} objet avec la valeur des champs (descid)
         */
        saveFileSettings: async function (val) {

            let jsonToReturn = await this.createJson(val);
            jsonToReturn["Tab"] = this.getTab;

            let oFileLayout = this.loadFileLayout();

            try {
                this.setTkStructPage({ link: this.getTkStructPage.link, params: this.getTkStructPage.params, results: this.linkToPost(oFileLayout.url, jsonToReturn) });
                await Promise.all([this.LoadDataPage(true), this.LoadWzCatalog(true, { descid: jsonToReturn?.JsonWizardBar?.DescId || 0 }), this.LoadStructBkm(true)]);

                await this.getData({
                    reloadSignet: false,
                    reloadHead: false,
                    reloadAssistant: false,
                    reloadAll: true
                });

            } catch (e) {
                console.log(e);
            }
            finally {
                if (this.openDialogSetting)
                    this.closeFileSettings();
            }
        },
        /**
         * rechargement du catalogue.
         * @param {any} bForcereload
         * @param {any} oNewParams
         */
        getReloadCatalog: async function (bForcereload, oNewParams) {
            await this.LoadWzCatalog(bForcereload, oNewParams).then(() => this.ConstructCatalog(this));
        },
        /** affiche ou masque le eProgressSpinner local (setWait sauce IRIS)
        * @param {boolean} bOn true si on doit l'afficher, false si on doit le masquer
        * @param {any} nOpacity opacité à appliquer à la place de celle par défaut, de 0 à 1 par pas de 0.1
        */
        setWaitIris: function (bOn, nOpacity) {
            this.bDisplaySpinner = bOn;
            if (nOpacity)
                this.nOpacitySpinner = nOpacity;
        },
        /**
         * Renvoie si l'utiilisateur a le droit de modifier la rubrique en question
         * Par défaut, si pas de droit défini, on lui octroie
         */
        hasUpdateRight: function (sourceField) {
            switch (sourceField) {
                case "stepBarFields":
                    return this?.DataStruct?.Data?.CanUpdateWizardBar === true;
                default:
                    return true;
            }
        }
    },
    /**
     * On charge les données avant la création.
     * */
    async created() {

        EventBus.$off("loadFinish");
        EventBus.$off("globalExpressFilter");
        EventBus.$off("timeOutRightNav");

        EventBus.$off("globalModal");
        EventBus.$off("newHeightContentTab");
        EventBus.$off("tooltipModal");
        EventBus.$off("tooltipFocus");
        EventBus.$off("MotherOfAllModals");
        EventBus.$off("emitLoadAll");
        this.getMenus();
        await this.getData();

    },
    async mounted() {

        // On resize le contenue de la fiche au resize de la window
        this.getResizeContent();

        //let { efakeDialogAdmin } = await import("../FakeRecords/eFakeDialogAdmin.js");
        //this.saveFileSettings(efakeDialogAdmin);

        // Si le menu droit est ouvert alors on met un padding sur la mainDiv
        var e = document.getElementById("rightMenu");
        var mainDiv = document.getElementById("mainDiv");

        if (!(mainDiv && e)) throw this.getRes(7050);

        var observer = new MutationObserver((event) =>
            this.FavLinkOpenAlign(mainDiv, event[0].target)
        );

        observer.observe(e, {
            attributes: true,
        });

        if (this.observeRightMenu)
            this.observeRightMenu(true, this)

        this.FavLinkOpenAlign(mainDiv, e);

        EventBus.$on("timeOutRightNav", (options) => {
            //this.timeoutCalculHeight = options.timeoutCalculHeight;
            //setTimeout(this.getHeight, this.timeoutCalculHeight, options);
        });

        // CALCUL DE LA HAUTEUR DU CONTENU DE LA ZONE SIGNET/DETAIL
        EventBus.$on("newHeightContent", (options) => {
            //this.getHeight(options)
        });

        // CALCUL DE LA HAUTEUR DU CONTENU DE LA ZONE SIGNET/DETAIL
        EventBus.$on("newHeightContentTab", (newHeight) => {
            //this.heigthContentTab = newHeight.gsHeight;
            //this.currentHeight = newHeight.dataClientHeight;
        });

        EventBus.$on('valueEditedDataPicker', (dateRangePicker) => {


            //Date Start
            let dtStart = this.DataStruct.Data.LstDataFields.find(x => x.DescId === dateRangePicker.DateStart.DescId && x.FileId == dateRangePicker.FileId)
            if (dtStart) {
                dtStart.Value = dateRangePicker.DateStart.Value;

            }

            let dtEnd = this.DataStruct.Data.LstDataFields.find(x => x.DescId === dateRangePicker.DateEnd.DescId && x.FileId == dateRangePicker.FileId)
            if (dtEnd) {
                dtEnd.Value = dateRangePicker.DateEnd.Value;
            }

        });

        // Modal
        EventBus.$on("globalModal", (options) => {
            this.setModalGlobal(options);
        });

        // Tooltip
        EventBus.$on("tooltipModal", (options) => {
            if (!this.opened && !this.focus && !this.showModalAlert)
                this.setModal(options);
        });

        EventBus.$on("tooltipFocus", (val) => {
            this.focus = val;
            this.showModalTooltip = val ? false : true;
        });

        EventBus.$on("MotherOfAllModals", (options) => {
            this.optionsMotherModal = options;
            this.showMotherModal = true;
        });

        EventBus.$on("emitLoadAll", async (options) => {


            if (options.reloadHead) {
                this.LoadDataPage(true);
            } else if (options.reloadSignet) {
                this.LoadStructBkm(true);
            } else if (options.reloadAll) {
                this.LoadDataPage(true);
                this.LoadWzCatalog(true);
                this.LoadStructBkm(true);
            }

            await this.getData(options);


            if (this.$refs["lnkid_" + this.getTab])
                this.$refs["lnkid_" + this.getTab].value = this.getLnkIdInfos;

            this.nForceRefreshTabsBar += 1;
        });

        // Permet de savoir si on doit afficher ou non la zone detail et donc enlever le skeleton
        EventBus.$on("showDetail", (options) => {
            this.detailsGoSkeleton = false;
            this.setWaitIris(false);
        });

        EventBus.$on('valueEdited', (options) => {
            let findItemData = this.DataStruct?.Data?.LstDataFields.find(c => c.DescId == options.DescId && c.FileId == options.FileId);
            if (findItemData) {
                findItemData.Value = options?.NewValue ? options?.NewValue : '';
                findItemData.DisplayValue = options?.NewDisp ? options?.NewDisp : '';
            }

        });

        //Regression 84 284 : Mini fiche intempestive on force à fermer les modals 
        //qui sont ouvert quand t'en charge le nouveau mode fiche pour la première fois
        await this.$nextTick();
        closeAllModals();

        this.setFileRoot(this.fileRoot);
        await this.$nextTick();
        this.adjustDivContainerXRM();
    },
};
