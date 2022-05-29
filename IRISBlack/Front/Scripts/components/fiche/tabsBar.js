import EventBus from '../../bus/event-bus.js?ver=803000';
import { EdnType, PinnedBookmarkControllerOperation, BKMVIEWMODE } from '../../methods/Enum.min.js?ver=803000';
import { LoadStructBkm, ConstructBookmarkFile, initDatabase, initDatabaseIrisBlack, manageDatabaseIrisBlack, linkToPost, setClosePinnedBkm, setViewMode, saveScrollPosition, setWaitIris } from "../../methods/eFileMethods.js?ver=803000";
import { eFileMixin } from '../../mixins/eFileMixin.js?ver=803000';
import { initIndexedDB, manageIndexedDB, setDataDbAsync, getDataDbAsync, mergeDataDbAsync, filterDataDbAsync, countDataDbAsync, firstDataDbAsync, whereDataDbAsync } from "../../methods/eBkmMethods.js?ver=803000";
import { loadPinnedBookmark } from "../../shared/XRMWrapperModules.js?ver=803000";
import eDirectDragAndDrop from '../../directives/eDirectDragAndDrop.js?ver=803000';
import { setNormalizeString } from "../../methods/eMainMethods.js?ver=803000";

export default {
    name: "tabsBar",
    data() {
        return {
            heigthContentTab: null,
            timeoutCalculHeight: 0,
            iLimitHeight: 205,
            iScrollHeightMax: 250,
            iScrollHeightStep: 41,
            iMaxHeightSignet: 52,
            mobile: false,
            optionsExpressFilter: Object,
            showExpressFilterDate: false,
            modalListBkm: null,
            DataStruct: null,
            tabAllDetail: null,
            DataJson: null,
            reloadingSignet: true,
            reloadingDetails: true,
            IsShowGrid: false,
            tabType: ["detail", "signet", "grille"],
            loadAnotherBkm: true,
            lstNewBkm: [],
            previousBkm: null,
            currentBkm: null,
            bkmDBCurrent: null,
            dbBkm: null,
            scrolledBkms: new Array(),
            tabs: [
                {
                    id: "signet",
                    Label: top._res_859,
                    active: false,
                    type: 'signet',
                    signets: [],
                    removable: false,
                    pinnedTab: false
                },
                {
                    id: "details",
                    Label: top._res_860,
                    active: false,
                    type: 'detail',
                    removable: false,
                    pinnedTab: false
                }
            ],
            tabsFiltered: null,
            emptyTabsResult: false,
            searchValue: null,
            stickyNavSignet: false,
            reloadDetailFunction: false,
            navPrevDisabled: true,
            navNextDisabled: true,
            activeBkmContent: [],
            dragAndDropSource: "tabs",
            fixedTabs: [],
            pinnedTabs: [],
            BKMVIEWMODE,
            fileIdPinned: null,
            showDetailForSeparator: false
        };
    },
    props: {
        propDetail: {
            type: Array
        },
        propCols: {
            type: Number
        },
        activityCompoExpanded: {
            type: Boolean
        },
        navTabsSticky: {
            type: Boolean
        },
        endResizing: {
            type: Boolean
        },
        isDisplayed: {
            type: Boolean
        },
        nForceRefreshTabsBar: Number,
    },
    components: {
        tabGrille: () => import(AddUrlTimeStampJS("./tabGrille.js")),
        tabSignets: () => import(AddUrlTimeStampJS("./tabSignets.js")),
        tabPinnedBkm: () => import(AddUrlTimeStampJS("./tabsBar/tabPinnedBkm.js")),
        fileDetail: () => import(AddUrlTimeStampJS("./fileDetail.js")),
        expressFilterGlobal: () => import(AddUrlTimeStampJS("../modale/expressFilter.js")),
        tabPinnedFile: () => import(AddUrlTimeStampJS("./tabsBar/tabPinnedFile.js"))
    },
    directives: { dragndrop: eDirectDragAndDrop },
    mixins: [eFileMixin],
    computed: {
        /** Nombre de signets dans l'onglet signet. */
        getNbBkm: function () {
            return this.tabs
                .filter(signet => signet.id == 'signet')
                .map(signet => signet.signets.length)
                .reduce((accumulator, currentValue) => accumulator + currentValue);
        },

        /** Taille du skeleton en function du nombre de signets.
         * On compte 200px un signet */
        getSkeletonSize: function () {
            return 200 * this.getNbBkm + "px";

        },
        /** détermine la taille de la dropdow des signets. */
        setHeightDropDownSignet: function () {

            let heightDropDownSignet = this.getNbBkm * this.iScrollHeightStep + this.iScrollHeightStep;

            if (heightDropDownSignet - this.iScrollHeightStep >= this.iLimitHeight) {
                heightDropDownSignet = this.iScrollHeightMax;
            }

            return heightDropDownSignet * ((this.tabs
                .find(signet => signet.id == 'signet').signets.length <= 0) ? 2 : 1) + 'px';
        },
        getSignet() {
            if (this.tabsFiltered == null)
                return this.tabs.find(x => x.id == "signet").signets;
            else
                return this.tabsFiltered
        },
        getSearchIcon() {
            if (this.searchValue == '')
                return 'glyphicon-search'
            else if (this.searchValue != '' && this.searchValue != null)
                return 'glyphicon-remove';
            else
                return 'glyphicon-search'
        },
        getStickyNavSignet() {
            return this.navTabsSticky ? 'stickyNavSignet' : '';
        },
        getPropDetail: {
            get() {
                return this.propDetail;
            },
            set(val) {
                this.$emit('update:propDetail', val)
            }
        },
        /** Computed pour forcer le rafraichissemnt des composants,
         * à partir du composant parent. */
        ForceRefreshTabsBar: {
            get: function () {
                return this.nForceRefreshTabsBar;
            },
            set: function (value) {
                this.$emit('update:nForceRefreshTabsBar', value)
            }
        },
        statusSeparator() {
            return this.showDetailForSeparator;
        }
    },
    watch: {
        // watch the modification of the tabs, and update the fixedTabs / pinnedTabs
        tabs: {
            deep: true,
            handler() {
                this.fixedTabs = this.tabs?.filter(tab => !tab?.pinnedTab);
                this.pinnedTabs = this.tabs?.filter(tab => tab?.pinnedTab);

                this.getNavBtnStatus();
            }
        },
        // watch when detail area is resizing to compute again the tab nav buttons
        endResizing: {
            deep: true,
            handler() {
                this.getNavBtnStatus();
                this.$emit('update:endResizing', false);
            }
        },
        /** Permet de repositionner correctement vers l'onglet épinglé
         *  suite à un raffraichissement.
         * */
        isDisplayed: {
            deep: true,
            handler(bVal) {
                if (bVal) {
                    let currentTab = this.tabs.find(tab => tab.active);;
                    this.getNavBtnStatus();
                    this.repositionTab(currentTab)
                }

            }
        },
    },
    methods: {
        LoadStructBkm,
        ConstructBookmarkFile,
        initIndexedDB,
        manageIndexedDB,
        whereDataDbAsync,
        setDataDbAsync,
        getDataDbAsync,
        mergeDataDbAsync,
        filterDataDbAsync,
        countDataDbAsync,
        firstDataDbAsync,
        initDatabase,
        initDatabaseIrisBlack,
        manageDatabaseIrisBlack,
        loadPinnedBookmark,
        linkToPost,
        setViewMode,
        setClosePinnedBkm,
        setNormalizeString,
        saveScrollPosition,
        setWaitIris,
        getDetailBlankSeparator(tab) {
            if (tab.id == 'details') {
                this.reloadDetailFunction = true;
            }
        },
        getnbRow(descId) {
            return this.tabs.find(signet => signet.id == 'signet').signets.find(a => a.DescId == descId).nbRows
        },

        /** Initialisation de la base de donnée locale avec le signet en cours.
         * Si c'est la première fois qu'on visite cet onglet, alors, rien.
         * Sinon, on reprend les signets précédemment enregistrés. */
        initLocalDatabase: async function () {
            try {
                let oSearch = { nTab: parseInt(this.getTab) };
                this.dbBkm = await this.initDatabaseIrisBlack();
                let bkmDBCount = await this.countDataDbAsync(this.dbBkm, "Bookmark", oSearch);
                let arBkm = this.tabs?.find(bkm => bkm.id == "signet")?.signets;

                if (bkmDBCount < 1) {
                    //await this.mergeDataDbAsync(this.dbBkm, "Bookmark", { nTab: parseInt(this.getTab), type: "signet", bkm: arBkm?.find(n => n)?.id });
                    await this.mergeDataDbAsync(this.dbBkm, "Bookmark", { nTab: parseInt(this.getTab), type: "detail", bkm: "detail" });
                }

                this.bkmDBCurrent = await this.firstDataDbAsync(this.dbBkm, "Bookmark", oSearch);

                //this.backToBookmark(this.bkmDBCurrent?.bkm);
                this.contentLoaded();
                if (this.bkmDBCurrent?.type == "signet") {
                    let targetBkm = arBkm?.find(lst => lst.id == this.bkmDBCurrent?.bkm);
                    this.scrollIn(undefined, targetBkm, "init");
                }
            }
            catch (e) {
                this.manageDatabaseIrisBlack(e);
                this.contentLoaded();
            }
        },

        /**
         * Une fois que le contenu est affiché à l'utilisateur, retire la classe CSS du template indiquant qu'il est en cours de chargement
         * */
        contentLoaded: function () {
            EventBus.$emit('showDetail', true);
            this.showDetailForSeparator = true;
        },

        /**
         * Permet un (re-)chargement du composant en cours, en appelant le controleur.
         * @param {any} bForceReload
         */
        selfReloadBkm: async function (bForceReload = false) {
            await this.LoadStructBkm(bForceReload);
            await this.ConstructBookmarkFile(EdnType, bForceReload);
            this.ForceRefreshTabsBar += 1;
        },
        /** Permet de rendre actif le signet. */
        async backToBookmark(bkm) {

            let activeTab = this.tabs?.find(bkm => bkm.active == true);

            if (activeTab && activeTab.id != "signet") {
                let bkmTab = this.tabs.find(bkm => bkm.id == "signet");
                activeTab.active = false;
                bkmTab.active = true;

                this.dbBkm = this.dbBkm || await this.initDatabaseIrisBlack();
                this.mergeDataDbAsync(this.dbBkm, "Bookmark", { nTab: parseInt(this.getTab), type: bkmTab.type, bkm: bkmTab.id });
            }

            this.contentLoaded();
        },

        /** Détermine la position du bouton de sélection des signets. */
        setBtnSelectSignet: function () {
            let btn_select_signet = this.$refs.btn_select_signet;

            if (btn_select_signet?.style)
                btn_select_signet.style.top = this.setHeightDropDownSignet;
        },
        /**
         * 
         * @param {any} fromNoteDisplay vérifie que le redimensionnement se fait suite à l'affichage ou non de la zone note
         */
        redimensionnement: async function (fromNoteDisplay = false) {
            let nav = this.$refs["tabsNav"];
            let gridTabs = this.$parent.$refs["gridTabs"];

            await Vue.nextTick();

            if (nav && gridTabs && !fromNoteDisplay) {
                nav.style.width = gridTabs.clientWidth + 'px';
            }

            if (nav && gridTabs && fromNoteDisplay) {

                let widthPercent = 0.745;
                let margins = 47;

                if (this.activityCompoExpanded)
                    nav.style.width = Math.round((this.$parent.$el.offsetWidth - margins) * widthPercent) + 'px';
                else
                    nav.style.width = (this.$parent.$el.offsetWidth - margins) + 'px';
                //this.stickyNavSignet = this.navTabsSticky;
            }

            if ("matchMedia" in window) {
                this.mobile = window.matchMedia("(min-width:1100px)").matches;
                this.calHeightTabs();
            }
        },
        /**
         * A vérifier, mais a priori permet de changer d'onglet.
         * Je rajoute l'enregistrement en base locale.
         * @param {any} tab
         */
        async calHeightTabs(tab) {

            if (tab && !this.tabType.includes(tab.type))
                return false;

            let tabId = tab?.id || "";

            if (tab) {
                this.dbBkm = this.dbBkm || await this.initDatabaseIrisBlack();
                this.mergeDataDbAsync(this.dbBkm, "Bookmark", { nTab: parseInt(this.getTab), type: tab.type, bkm: tabId });
            }

            var options = {
                timeoutCalculHeight: 200,
                mobileDesign: this.mobile,
                gridId: tab ? tab.id : ''
            };
            EventBus.$emit('timeOutRightNav', options);
        },
        /** change les tabs et le contenu lié ( active ou non ) 
         * @param {any} tab
        */
        async changeTab(tab) {
            // this.activeBkmContent = []
            this.tabs.forEach(tabElm => tabElm.active = false);
            let tabActive = this.tabs.find(tabElm => tabElm.Label == tab.Label)

            if (tabActive) {
                tabActive.active = true;
                this.repositionTab(tabActive);
            }

            this.dbBkm = this.dbBkm || await this.initDatabaseIrisBlack();
            this.mergeDataDbAsync(this.dbBkm, "Bookmark", { nTab: parseInt(this.getTab), type: tab.type, bkm: tab?.id });
            // this.tabs.forEach(tabElm => this.activeBkmContent.push({ id: tabElm.id, active : tabElm.active}));
            //this.activeBkmContent = this.tabs.map(tabElm => tabElm?.active == true);
        },
        /**  reposition tab après avoir choisi un nouveau onglet
         * @param {any} tab
        */
        repositionTab(tab) {
            if (!tab || tab?.type == 'signet' || tab?.type == 'detail')
                return;

            let currentBkm = this.$refs['tab_' + tab.id][0];
            let nav = this.$refs.tabsNav;

            if (nav.scrollWidth - nav.offsetWidth - currentBkm.offsetLeft + nav.offsetLeft > 0) {
                this.scroll(currentBkm.offsetLeft - nav.offsetLeft - nav.scrollLeft);
            } else {
                this.scroll(nav.scrollWidth - nav.offsetWidth - nav.scrollLeft);
            }
        },

        /**
         * Permet d'épingler un signet depuis la selection des signets
         * @param {any} signet
         */
        setPinnedBkmDropDown: async function (signet) {
            if (signet.IsPinned) {
                let oPinnedParams = {
                    Actions: signet.Actions,
                    DescId: signet.DescId,
                    ExpressFilterActived: signet.ExpressFilterActived,
                    FileId: undefined,
                    HistoricActived: signet.HistoricActived,
                    Label: signet.Label,
                    TableType: signet.TableType,
                    ViewMainTab: signet.ViewMainTab,
                    ViewMode: signet.ViewMode,
                    aFileId: undefined,
                    active: !this.tabs.find(tab => tab.id == "signet")?.active,
                    id: 'pinned_' + signet.id,
                    idxRow: undefined,
                    loaded: signet.loaded,
                    nbRows: signet.nbRows,
                    pinnedTab: true,
                    preventLoad: signet.preventLoad,
                    promBkmData: undefined,
                    promBkmFileLayout: undefined,
                    removable: true,
                    type: "signet",
                }

                this.setNewtab(oPinnedParams);
                this.setClosePinnedBkm(oPinnedParams);

            } else {
                signet.descId = signet.DescId;
                signet.pin = true;
                this.pinBkm(signet);
            }
        },

        /**
         * On scrolle vers le bon signet.
         * Comme on est en vue js, on doit d'abord attendre la fin
         * de la fusion entre le dom virtuel et le vrai dom.
         * @param {any} event
         * @param {any} element
         */
        scrollIn: async function (event, element, emitter) {
            var that = this;

            if (emitter == "signet") {
                await this.changeTab(this.tabs.find(x => x.id == emitter));
            }

            this.backToBookmark();

            if (emitter == "init" && element)
                this.mergeDataDbAsync(this.dbBkm, "Bookmark", { nTab: parseInt(this.getTab), type: "signet", bkm: element?.id });

            await this.$nextTick();

            // ELAIZ - Demande 89 566 - On vérifie au chargement de la page (emitter = 'init') que l'on est en haut de la fiche ou pas. On vérifie ceci grace à navTabsSticky qui nous indique si la navigation des signets/détails est fixée suite au scroll de la page. Dans ce cas, on ne lance pas la méthode pour scroller vers le signet

            //if (!this.navTabsSticky && emitter == 'init')
            //    return false;

            let currentBkm = (element) ? document.getElementById(element.id) : null;
            let topScroll = currentBkm && currentBkm != null ? currentBkm.offsetTop : 0;
            let topGridTabs = this.$parent?.$refs["gridTabs"].offsetTop - 30;
            let leftScroll = currentBkm && currentBkm != null ? currentBkm.offsetLeft : 0;
            let contentWrap = this.$parent?.$refs?.mainContentWrap;
            let lstBkm = this.tabs?.find(bkm => bkm.id == 'signet');
            let indexBkm = lstBkm?.signets?.indexOf(element);
            let previousBkm = null;

            if (indexBkm > 0) {
                previousBkm = lstBkm.signets[indexBkm - 1];

                // #82 905 - si le signet précédant celui choisi est chargé, on paramètre les variables pour que scrollIn() soit déclenché une dernière fois
                // par fiche.js > getHeight() une fois que la hauteur aura été ajustéee
                // On temporise ici aussi pour laisser le temps à la hauteur de s'ajuster visuellement
                if (previousBkm?.loaded && that.scrolledBkms?.indexOf(element) == -1 && this.bkmDBCurrent?.type == "signet") {
                    setTimeout(function () {
                        if (contentWrap.scrollTop <= (topScroll + topGridTabs)) {
                            that.previousBkm = previousBkm;
                            that.currentBkm = element;
                            that.scrolledBkms.push(element);
                            that.scrollIn(event, element);
                        }
                    }, 2000);
                }
                //sinon, on retente cette vérification en différé, on attend que le signet précédent soit chargé
                if (!previousBkm?.loaded && this.bkmDBCurrent?.type == "signet") {
                    setTimeout(function () {
                        if (contentWrap.scrollTop <= (topScroll + topGridTabs)) {
                            that.scrollIn(event, element);
                        }
                    }, 500);
                }
            }
            // et en attendant, on effectue le scroll vers le signet choisi en l'état actuel des choses, pour donner un premier feedback visuel à
            // l'utilisateur
            //ELAIZ - bloquable du scroll horizontal car sinon le contenu à tendance à scroller vers la gauche même si il n'y a pas de scroll horizontal
            if (this.tabs.find(tab => tab.id == "signet")?.active) {
                contentWrap.scrollTo({
                    top: topScroll + topGridTabs,
                    left: 0,
                    behavior: 'smooth'
                });
            }
        },

        OpenBkmSelect() {
            let ntab = this.getTab;
            this.modalListBkm = new eModalDialog(this.getRes(366), 0, "eSelectBkm.aspx", 850, 520);
            this.modalListBkm.bBtnAdvanced = true;

            this.modalListBkm.addParam("tab", ntab, "post");
            this.modalListBkm.ErrorCallBack = () => this.cancelBkmSelect();
            this.modalListBkm.show();

            this.modalListBkm.addButtonFct(this.getRes(29), () => this.cancelBkmSelect(), "button-gray");
            this.modalListBkm.addButtonFct(this.getRes(28), () => this.reloadSignet(ntab), "button-green");

        },

        /** On ferme la popup de séléction des signets. */
        cancelBkmSelect() {
            this.loadAnotherBkm = false;
            this.modalListBkm.hide();
        },

        reloadSignet(nTab) {
            this.setWaitIris(true, 0.3);
            var oDoc = this.modalListBkm.getIframe().document;
            var _itemsUsed = oDoc.getElementById("TabSelectedList").getElementsByTagName("div");

            var strListCol = "";
            for (var i = 0; i < _itemsUsed.length; i++) {

                var nDescId = getNumber(_itemsUsed[i].getAttribute("DescId"));
                if (nDescId <= 0 || isNaN(nDescId))
                    continue;

                if (strListCol != "")
                    strListCol = strListCol + ";";
                strListCol = strListCol + nDescId;
            }

            var updatePref = "tab=" + nTab + ";$;bkmorder=" + strListCol;

            this.DataStruct = this.$parent.DataStruct;
            this.tabAllDetail = this.$parent.tabAllDetail;
            this.DataJson = this.$parent.DataJson;

            let options = {
                reloadSignet: true,
                reloadHead: false,
                reloadAssistant: false,
                reloadAll: false
            }

            this.loadAnotherBkm = true;

            this.lstNewBkm = / ?; ?/g[Symbol.split](strListCol)
                .filter(col => !this.tabs
                    .find(bkm => bkm.id == "signet")
                    .signets
                    .map(bkm => bkm.DescId)
                    .includes(parseInt(col)));

            updateUserPref(updatePref, async () => {
                await this.selfReloadBkm(true);
                this.backToBookmark();
                this.callBackEmitLoadAll(options);
                this.setWaitIris(false, 0);
            });

            this.modalListBkm.hide();
        },

        callBackEmitLoadAll(options) {
            if (options)
                if (options.reloadSignet || options.reloadAll) {
                    this.reloadingSignet = false;
                    Vue.nextTick(() => this.reloadingSignet = true);
                    this.setBtnSelectSignet();

                    this.loadAnotherBkm = false;

                    if (options.reloadSignet) {
                        this.scrollIn(undefined,
                            this.tabs
                                .find(bkm => bkm.id == "signet")
                                .signets
                                .find(bkm => this.lstNewBkm
                                    .find(nwBkm => nwBkm == bkm.DescId)));
                    }

                } else if (options.reloadDetails || options.reloadAll) {
                    this.reloadingDetails = false;
                    Vue.nextTick(() => this.reloadingDetails = true);
                }

        },
        /**
        * ELAIZ - Refonte de la méthode searchSignet qui ciblait avant les éléments dans le DOM - cablage avec la valeur du champs de recherche
        * */
        SearchSignet: function () {
            let inputValue = new RegExp(this.searchValue, 'i')

            let signet = this.tabs.find(signet => signet.id == "signet").signets

            if (this.searchValue != undefined)
                this.tabsFiltered = signet.filter(signet => inputValue.test(signet.Label));

            this.emptyTabsResult = (this.tabsFiltered.length < 1 && this.tabsFiltered != null) ? true : false;

        },

        setExpressFilter(options) {
            this.optionsExpressFilter = options;
            if (options.typeModal == "date") {
                this.showExpressFilterDate = true;
            }
        },
        deleteSearchValue() {
            if (this.searchValue != '') {
                this.searchValue = '';
                this.tabsFiltered = null;
                this.emptyTabsResult = false;
            }
        },
        async savePinnedBkm(oPinnedBkm) {

            let arTabsPinned = this.tabs?.flatMap(tb => tb.signets)?.filter(bkm => bkm && bkm.IsPinned)?.map(bkm => bkm.DescId) || [];

            let oPinnedBookmark = this.loadPinnedBookmark();

            if (!(oPinnedBookmark && this.getTab && this.getFileId))
                return;

            try {
                await this.linkToPost(oPinnedBookmark.url, { nTab: parseInt(this.getTab), nFileId: parseInt(this.getFileId), nPinnedBookmarkDescId: parseInt(oPinnedBkm.descId), aPinnedBookmarkDescIdList: arTabsPinned, oOperation: PinnedBookmarkControllerOperation.ADD, oViewMode: oPinnedBkm.ViewMode });
            } catch (e) {
                console.log(e);
            }
        },
        /** Récupère de eActionSignet du bouton punaise 
         * si le signet est epinglé ou non et là 2ème fois 
         * redirige vers la tab en question 
         * * @param {any} oPinnedBkm signet concerné
         * */
        async pinBkm(oPinnedBkm) {
            let sameTab = this.tabs.find(tab => tab.DescId == oPinnedBkm.descId);
            let oPinnedId = 'pinned_id_' + oPinnedBkm?.DescId + '_' + setNormalizeString(oPinnedBkm?.Label);
            if (oPinnedBkm) {
                this.dbBkm = this.dbBkm || await this.initDatabaseIrisBlack();
                this.mergeDataDbAsync(this.dbBkm, "Bookmark", { nTab: parseInt(this.getTab), type: "pinned-bkm", bkm: oPinnedId });
            }

            let oPinnedParams = {
                DescId: oPinnedBkm.descId,
                FileId: oPinnedBkm?.fileId,
                nbRows: oPinnedBkm?.nbRows,
                idxRow: oPinnedBkm?.idxRow,
                aFileId: oPinnedBkm?.aFileId,
                id: oPinnedId,
                Label: oPinnedBkm?.Label,
                active: false,
                type: oPinnedBkm?.type,
                Actions: oPinnedBkm?.Actions,
                removable: true,
                pinnedTab: true,
                TableType: oPinnedBkm?.TableType,
                ViewMode: oPinnedBkm?.ViewMode,
                ExpressFilterActived: oPinnedBkm?.ExpressFilterActived,
                HistoricActived: oPinnedBkm?.HistoricActived,
                promBkmData: oPinnedBkm?.promBkmData,
                promBkmFileLayout: oPinnedBkm?.promBkmFileLayout
            }

            if (oPinnedBkm.pin) {
                // check if the tab exist in tabs or not
                if (!sameTab) {
                    // add pinned Bkm in the tabs
                    this.tabs.push(oPinnedParams);
                    // send the order of the tabs to back
                    await this.savePinnedBkm(oPinnedBkm);
                    // init pinned Bkm
                    this.tabs.forEach(tab => {
                        if (tab.signets) {
                            tab.signets.forEach(bkm => {
                                if (bkm.DescId == oPinnedBkm.descId)
                                    bkm.IsPinned = true;
                            })
                        }
                    })
                } else {
                    // send the order of the tabs to back
                    await this.savePinnedBkm(oPinnedBkm);
                    // await this.setViewMode(oPinnedBkm.ViewMode,oPinnedBkm.descId);
                    // the tab exist in tabs, but it could be a file or a list
                    let tabIndex = this.tabs.indexOf(sameTab);
                    if (tabIndex > 1) {
                        this.tabs.splice(tabIndex, 1);
                        this.tabs.push(oPinnedParams);
                    }
                }

                // actived the pinned tab
                this.tabs.forEach(tab => tab.active = false);
                let currentTab = this.tabs.find(tab => tab.DescId == oPinnedBkm.descId)
                currentTab.active = true;
                // wait the dom refresh
                await this.$nextTick();
                // reposition tab, to display the button on the bar
                this.repositionTab(currentTab);
            } else {
                // if the oPinnedBkm.pin not exist, it should be a request to close the pinned Bkm
                oPinnedBkm.id = 'pinned_' + oPinnedBkm.id;
                this.setClosePinnedBkm(oPinnedBkm);
            }

            this.fileIdPinned = oPinnedBkm?.fileId;
            this.repositionBkm();
        },
        /**
        * reposition the tabsbar after pinned Bkm reloaded
        **/
        repositionBkm() {
            this.$emit('repositionBkm', true);
        },
        /**
        * Faire la translation à droite lorsque le texte dépasse dans le tabsBar
        **/
        scrollRight() {
            if (!this.navNextDisabled) {
                this.scroll(250)
            }
        },
        /*
        * Faire la translation à gauche lorsque le texte dépasse dans le tabsBar
        **/
        scrollLeft() {
            if (!this.navPrevDisabled) {
                this.scroll(-250)
            }
        },
        /** Faire la translation de le tabsBars
         * * @param {int} n : Définit ou obtient le nombre de pixels dont le contenu est défilé vers la gauche
         **/
        async scroll(n) {
            await this.$nextTick();
            let nav = this.$refs.tabsNav
            // the scroll position
            let curPosition = nav.scrollLeft + n;
            nav.scrollLeft += n;
            // vérifier si le bouton doit être désactivé
            // scrollWidth & offsetWidth retourne la valeur d'un nombreà l'entier le plus proche, la taille réelle sera plus grande
            this.navNextDisabled = (curPosition >= nav.scrollWidth - nav.offsetWidth - 1);
            this.navPrevDisabled = (curPosition <= 0);
        },
        /*
        * listen the event "Drop" from eDirectDragAndDrop, and send the order of tabs to back
        **/
        async onDrop() {
            let pinnedBookmarkDescIdList = [];

            this.tabs.forEach(function (item) {
                if (item?.DescId) {
                    pinnedBookmarkDescIdList.push(item.DescId);
                }
            });

            let oPinnedBookmark = this.loadPinnedBookmark();
            try {
                await this.linkToPost(
                    oPinnedBookmark.url,
                    {
                        nTab: parseInt(this.getTab),
                        nFileId: parseInt(this.getFileId),
                        aPinnedBookmarkDescIdList: pinnedBookmarkDescIdList
                    }
                );
            } catch (e) {
                console.log(e);
            }
        },
        /**
         * Quand on clique sur la croix d'un signet épinglé.
         * on affiche le contenu de l'onglet placé immédiatement à sa droite, si aucun, le contenu de l'onglet immédiatement à sa gauche.
         * @param {any} tab
         */
        setNewtab(tab) {
            // only works for the actived tab
            if (tab.active) {
                // get index for currnt tab
                let index = this.tabs.findIndex(tb => tb.id == tab.id);
                let curPoistion = index < (this.tabs.length - 1) ? index + 1 : index - 1;
                // set tab actived
                this.changeTab(this.tabs[curPoistion]);
                this.calHeightTabs(this.tabs[curPoistion]);
                this.getDetailBlankSeparator(this.tabs[curPoistion]);
            }
        },
        /**
         * Permet de vérifier si on est à la fin ou au début du scroll de la navigation
         * et désactive les boutons si c'est le cas
         */
        getNavBtnStatus() {
            //this.scroll(0) ne peut pas utiliser la même fonction ici, ou il fonctionnera 2 fois, avant et après, la valeur n'est pas correcte
            let nav = this.$refs.tabsNav
            this.navNextDisabled = (nav.scrollLeft >= nav.scrollWidth - nav.offsetWidth - 1);
            this.navPrevDisabled = (nav.scrollLeft <= 0);
        },
        /**
         * re-pin the bookmark
         */
        showPinnedBkm($event) {
            // dispaly list from file
            let pinnedTab = this.tabs.find(tab => tab.DescId == $event);
            if (pinnedTab) {
                let oPinnedParams = {
                    DescId: $event,
                    descId: $event,
                    FileId: null,
                    id: 'id_' + pinnedTab?.DescId + '_' + setNormalizeString(pinnedTab?.Label),
                    Label: pinnedTab?.Label,
                    active: false,
                    type: pinnedTab?.type,
                    Actions: pinnedTab.Actions,
                    removable: true,
                    pinnedTab: true,
                    ViewMode: 0,
                    pin: true
                }
                this.pinBkm(oPinnedParams);
            }
        },
        showPinnedFile($event) {
            // dispaly file from list
            this.pinBkm($event);
        },
        /** Envoie au parent si l'on doit afficher le skeleton détail ou pas */
        displayDetailSkeleton(bVal) {
            this.$emit('displayDetailSkeleton', bVal);
        },
        /** Classes CSS des li */
        getTabsNavLiClass(pinned) {
            return {
                'minWidthSignet': pinned.type == 'signet',
                'active': pinned?.active,
                'nav-tabs__li': true
            }
        }
    },
    async created() {
        try {
            await this.selfReloadBkm();
            await this.initLocalDatabase();

            // Modal
            EventBus.$on('loadFinish', options => {
                this.callBackEmitLoadAll(options)
            });

            EventBus.$on('globalExpressFilter', (options) => {
                this.setExpressFilter(options);
            });
            // check scrollLeft / scrollRight button disabled or not
            await this.$nextTick();
            let navOffsetWidth = this.$refs.tabsNav?.offsetWidth;
            let navScrollWidth = this.$refs.tabsNav?.scrollWidth;
            this.navNextDisabled = navOffsetWidth >= navScrollWidth;

        } catch (e) {
            console.log(e);
        }
    },
    async updated() {
        let fromAsideDisplay = this.$attrs.tabsBarKey > 0;
        await this.redimensionnement(fromAsideDisplay);
    },
    async mounted() {

        await this.redimensionnement();

        let resizeO = new ResizeObserver(([e]) => this.redimensionnement());
        resizeO.observe(this.$root.$el);

        // calcul de la position du boutton 'Selectionner les signets à afficher'
        this.setBtnSelectSignet();
    },
    template: `
<div id="tabsBar" ref="navTabsCustom" class="nav-tabs-custom">
    <div :class="[getStickyNavSignet,'tabs-nav--container d-flex']">
        <div class="tabs-nav--visible-area flex-1">
            <ul class="nav nav-tabs d-flex">
                <li @click="changeTab(tab);calHeightTabs(tab);getDetailBlankSeparator(tab)"
                    :class="getTabsNavLiClass(tab)"
                    :ref="'tab_' + tab.id"
                    v-for="tab in fixedTabs"
                    :key="tab.id"
                >
                    <a class="nav-tab--btn" :aria-expanded="tab.active" data-toggle="tab" :href="'#' + tab.id">
                    {{tab.Label}}
                    <i v-if="tab.type == 'signet'" class="fa fa-ellipsis-v bkm-menu"></i>
                    </a>
                </li>
                <li v-if="tab.type == 'signet'" v-for="tab in tabs" class="box-title dropdown">
                    <a data-toggle="collapse" data-parent="#accordion" aria-expanded="true" class="pointer">
                        <i class="fa fa-ellipsis-v dropdown-menu-icon"></i>
                    </a>
                    <div ref="searchSignets" id="drop-recherche-signet" class="dropdown-content txt-size" role="menu">
                        <div id="top_nav_rech">
                            <div id="recherche" class="has-feedback">
                                <input v-model="searchValue" autocomplete="off" ref="searchSignet" id="searchSignet" @keyup="SearchSignet" type="text" class="form-control input-sm" :placeholder="getRes(595)">
                                <span @click="deleteSearchValue()" :class="[getSearchIcon,'glyphicon form-control-feedback search-input-icon']"></span>
                            </div>
                            <div role="presentation" class="divider"></div>
                        </div>
                        <a class="item-bookmark" @click.self="scrollIn($event,signet, 'signet')" v-for="signet in getSignet" :key="signet.id">{{signet.Label}}
                            <v-btn @click="setPinnedBkmDropDown(signet)" :ripple="false" plain heigth="24" width="24" icon :color="signet.IsPinned ? 'green' : ''">
                                <v-icon size="17">mdi-pin</v-icon>
                            </v-btn>
                        </a>
                        <span v-if="emptyTabsResult" class="NoSignet">{{ getRes(2643) }}</span>
                    </div>
                    <div ref="btn_select_signet" class="dropdown-content txt-size btn_select_signet" role="menu">
                        <div id="select">
                            <div role="presentation" class="divider"></div>
                            <a @click="OpenBkmSelect" id="select_signet" ><i class="far fa-file-alt"></i>{{ getRes(2641) }}</a>
                        </div>
                    </div>
                </li>  
            </ul>
        </div>
        <a
            v-if="pinnedTabs.length"
            class="d-flex align-center nav-arrow nav-prev"
            :class="navPrevDisabled ? 'disabled' : ''"
            @click="scrollLeft"
        >
            <i class="fas fa-chevron-left" />
        </a>
        <ul
            id="tabsNav"
            class="nav nav-tabs d-flex dropzone"
            ref="tabsNav"
        >
            <li
                v-for="pinned in pinnedTabs"
                :key="pinned.id"
                :class="getTabsNavLiClass(pinned)"
                @click.self="changeTab(pinned);calHeightTabs(pinned);getDetailBlankSeparator(pinned)"
                :ref="'tab_' + pinned.id"
                v-dragndrop="dragAndDropSource"
                @dropEvent="onDrop($event)"
            >
                <a
                    :aria-expanded="pinned.active"
                    data-toggle="pinned"
                    @click.self="changeTab(pinned);calHeightTabs(pinned);getDetailBlankSeparator(pinned)"
                >
                    {{pinned.Label}}<i v-if="pinned.removable" @click="setNewtab(pinned);setClosePinnedBkm(pinned)" class="fas fa-times-circle close-pinned-bkm"></i>
                </a>
            </li>
        </ul>
        <a
            v-if="pinnedTabs.length"
            class="d-flex align-center nav-arrow nav-next"
            :class="navNextDisabled ? 'disabled' : ''"
            @click="scrollRight"
        >
            <i class="fas fa-chevron-right" />
        </a>
    </div>

    <div id="tabContent" class="tab-content not-draggable" ref="tabContent">
        <div 
            v-for="(tabContent,id) in tabs" 
            :key="tabContent.id" 
            :ref="tabContent.id" 
            class="tab-pane bkm-tab"
            :class="{'active': tabContent?.active }" 
            :id="tabContent.id"
        >
            <div v-if="tabContent.type == 'signet' && reloadingSignet">
                <!--<eFileLoader css-class="skeleton-file" :css-sub-style="{height: getSkeletonSize}" css-sub-class="clsPlaceholderBkm" v-show="loadAnotherBkm" nb-internal-div="1"></eFileLoader>-->
                <tabSignets
                    @pinnedBkm="pinBkm"
                    :load-another-bkm="loadAnotherBkm"
                    :prop-data-detail="propDetail"
                    :prop-signet="tabContent"
                    :forceRefreshTabsBar="ForceRefreshTabsBar"
                    @callBackEmitLoadAll="callBackEmitLoadAll"
                />
            </div>
            <fileDetail
                v-if="tabContent.type == 'detail' && reloadingDetails"
                :is-displayed="statusSeparator && tabContent.active" 
                :prop-reload-detail-function="reloadDetailFunction"
                :prop-data-detail.sync="getPropDetail"
                :prop-cols="propCols"
                :nForceRefreshTabsBar.sync="nForceRefreshTabsBar"
                @saveScrollPosition="saveScrollPosition"
                @setWaitIris="setWaitIris"
            />
            <div class="gridLayout" :divdescid="tabContent.DescId" v-if="tabContent.type == 'grille'">
                <!-- Mode grille E2017 -->
                <tabGrille :prop-grille="tabContent"></tabGrille>
            </div>
            <div
                v-if="tabContent.removable"
                :class="{'active':tabContent?.active}"
            >
                <tabPinnedBkm
                    v-if="tabContent?.ViewMode == BKMVIEWMODE.LIST || tabContent?.empty"
                    class="bkm-tab"
                    :promBkmData="tabContent.promBkmData"
                    :propBkm="tabContent"
                    @pinnedBkm="showPinnedFile($event)"
                    :key="ForceRefreshTabsBar + '_' + tabContent.id"
                />
                <!-- for the moment keep like this, put them in one component with condition -->
                <tabPinnedFile
                    v-if="tabContent?.ViewMode == BKMVIEWMODE.FILE && !tabContent?.empty"
                    :key="'pinned_file_' + tabContent.id"
                    :nForceRefreshTabsBar.sync="nForceRefreshTabsBar"
                    :isDisplayed="tabContent?.active"
                    :propNbRow="getnbRow(tabContent.DescId)"
                    @displayDetailSkeleton="displayDetailSkeleton"
                    class="file-tab"
                    :propBkm="tabContent"
                    :propDetailBkm="tabContent.promBkmData"
                    :propBkmFileLayout="tabContent.promBkmFileLayout"
                    :fileIdPinned="fileIdPinned"
                    @showPinnedBkm="showPinnedBkm($event)"
                    @pinnedBkm="pinBkm"
                />
            </div>
        </div>
    </div>
    <expressFilterGlobal :filter-prop-tab="tabs" :prop-options-express-filter="optionsExpressFilter" v-if="showExpressFilterDate" @closeFilter="showExpressFilterDate = false"></expressFilterGlobal>
</div>`,

};