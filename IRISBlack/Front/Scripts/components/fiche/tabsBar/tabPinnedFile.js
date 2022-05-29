import { EdnType, FieldType,BKMVIEWMODE } from '../../../methods/Enum.min.js?ver=803000';
import { eFileMixin } from '../../../mixins/eFileMixin.js?ver=803000';
import EventBus from '../../../bus/event-bus.js?ver=803000';
import { linkToCall, setViewMode, linkToPost } from "../../../methods/eFileMethods.js?ver=803000";
import { JSONTryParse } from "../../../methods/eMainMethods.js?ver=803000";
import { loadFileBookmark, getIrisPurpleActived, openFileGuidedTyping, loadFileDetail, loadPinnedBookmark, loadFileLayout } from '../../../shared/XRMWrapperModules.js?ver=803000'
import { TableType } from '../../../methods/Enum.min.js?ver=803000';
import { eFileComponentsMixin } from '../../../mixins/eFileComponentsMixin.js?ver=803000';

export default {
    name: "tabPinnedFile",
    data() {
        return {
            tabAllDetail: [],
            oFileDetailIncrustBkm: {},
            oFileLayoutIncrustBkm: {},
            aFileNavigationDrawer: [],
            EdnType,
            nMinDescSys: 0,
            nMaxDescSys: 70,
            file: 1,
            aFilesidIncrement: [],
            nbRow : 0,
            fileUpdate:0,
            memoUpdate: 1,
            shortCutType: 'pinnedBkm'
        };
    },
    components: {
        eBkmUnit: () => import(AddUrlTimeStampJS("../bkmUnit.js")),
        eBkmAnnexe: () => import(AddUrlTimeStampJS("../BkmAnnexe.js")),
        tabPinnedNavigationDrawer: () => import(AddUrlTimeStampJS("../tabsBar/tabPinnedNavigationDrawer.js")),
        fileDetail: () => import(AddUrlTimeStampJS("../fileDetail.js")),
        eActionPinnedFile: () => import(AddUrlTimeStampJS("../eActionPinnedFile.js")),
        ePagination: () => import(AddUrlTimeStampJS("../../subComponents/ePagination.js")),
        shortcutBar: () => import(AddUrlTimeStampJS("../headFile/shortcutBar.js"))
    },
    computed: {
        /** Retourne les éléments non système pour FileDetail */
        getTabAllDetailFileDetail: function () {
            return this.tabAllDetail.filter(tb => tb.DescId > this.propBkm.DescId + this.nMinDescSys && tb.DescId < this.propBkm.DescId + this.nMaxDescSys)
        },
        /** Si un bkm n'est pas en cours de chargement et qu'on a 0 signet. */
        getDisplayBkm: function () {
            return !this.propBkm;
        },
        /** Permet de savoir si on a des séparateur dans la fiche */
        showDetailForSeparator: function(){
            let sepFields = this.tabAllDetail.filter(tab => tab.Format == FieldType.Separator);
             return sepFields.length > 1;
        },
        /** Permet de récupérer les informations sur la table */
        getStructTable: function () {
            return this.oFileDetailIncrustBkm?.Structure?.StructFile;
        },
        /** descid de la fiche incrustée */
        getPinnedFileDescId: function(){
            return this.oFileDetailIncrustBkm?.Structure?.StructFile?.DescId;
        }
    },
    methods: {
        loadFileDetail,
        loadFileLayout,
        linkToCall,
        loadFileBookmark,
        JSONTryParse,
        /** Permet de récupérer le nombre de fiche du signet */
        async getnbFiles() {
            if (this.propBkm.nbRows || this.propNbRow) {
                this.nbRow = this.propBkm.nbRows || this.propNbRow
            } else {
                if (this.getStructTable?.DescId) {
                    var nbRow;
                    let oDataBookmark = this.loadFileBookmark();
                    let promBkmData = this.linkToCall(oDataBookmark.url, { ...oDataBookmark.params, ParentTab: parseInt(this.getTab), ParentFileId: parseInt(this.getFileId), Bkm: parseInt(this.getStructTable?.DescId), RowsPerPage: 10, Page: 1 });
                    await promBkmData.then((results) => {
                        nbRow = JSON.parse(results)?.PagingInfo?.NbTotalRows
                    });
                    this.nbRow = nbRow
                }
            }
        },

        /**
         * Permet de retourner un overflow sur la div parente, si on est en mode fiche.
         * @param {any} signet
         */
        getStyleViewMode: function (signet) {
            return signet.ViewMode == 1 ? { 'overflow': 'auto' } : '';
        },
        /**
         * Permet de récupérer le detail de la fiche incruster.
        */
        getDetailBkm: async function () {
            // Le detail de la fiche incruster
            var detailsFile = await this.propDetailBkm;
            detailsFile = this.JSONTryParse(detailsFile);
            //Si on une liste de fiche c'est qu'on ne sait pas sur quelle fiche aller, alors on choisie la première
            if (!detailsFile || ( Array.isArray(detailsFile) && !detailsFile.length) ) {
                this.oFileDetailIncrustBkm = {};
            } else if (Array.isArray(detailsFile) && detailsFile.length) {
                this.file = 1;
                //this.getFileByFileId(true)
                this.oFileDetailIncrustBkm = detailsFile[0];
            } else {
                this.oFileDetailIncrustBkm = detailsFile;
                this.file = this.propBkm.idxRow;
            }

        },

        /**
         * Permet de récupérer le File Layout de la fiche, notament le NbCols
        */
        getDetailBkmFileLayout: async function () {
            await this.getDetailBkm();
            let layoutFile = await this.propBkmFileLayout;
            layoutFile = this.JSONTryParse(layoutFile);
            // if the file is not exist, give a empty value
            if (!layoutFile) {
                this.oFileLayoutIncrustBkm = {};
            } else {
                this.oFileLayoutIncrustBkm = layoutFile;
            }
        },

        /**
         * Permet de récupérer le detail et la strucutre de la fiche et d'en faire une fusion
        */
        setDetailStruc: async function (reload) {
            this.$emit('displayDetailSkeleton',true)
            if (!reload) {
                await this.getDetailBkmFileLayout();
            }
            this.oFileDetailIncrustBkm?.Data?.LstDataFields.forEach((a, idx) => {
                let findItemData = this.oFileDetailIncrustBkm.Structure.LstStructFields.find(
                    (b) => b.DescId == a.DescId
                );
                let objSetting = null;
                objSetting = { ...findItemData, ...a }
                Vue.set(this.tabAllDetail, idx, objSetting);
            });
            this.$emit('displayDetailSkeleton',false);


        },

        /** Initialisation du zoon note & description. 
        * @param {any} update permet de savoir si on est en maj ou initialisation
        */
        initNavigationDrawer: async function (update = false) {
            if(!update){
                await this.setDetailStruc();
                await this.getnbFiles();
            }
            let iNotes = 94;
            let icoNotes = { IconIn: "fas fa-outdent", IconOut: "fas fa-indent" };
            let iDescription = 89;
            let icoDescription = { IconIn: "fas fa-file-alt", IconOut: "fas fa-file-alt" };

            this.aFileNavigationDrawer = [];

            let note = this.tabAllDetail.find(a => a.DescId == parseInt(this.oFileLayoutIncrustBkm?.Tab) + iNotes);

            if (note != undefined && note.IsVisible) {
                this.aFileNavigationDrawer.push(Object.assign(note, icoNotes));
            }

            if ((this.getStructTable?.EdnType == EdnType.FILE_MAIN || this.getStructTable?.EdnType == EdnType.FILE_ADR) && this.getStructTable?.DescId != TableType.CAMPAIGN) {
                let description = this.tabAllDetail.find(a => a.DescId == parseInt(this.oFileLayoutIncrustBkm?.Tab) + iDescription);
                if (description != undefined && description.IsVisible) {
                    this.aFileNavigationDrawer.push(Object.assign(description, icoDescription));
                }
            }
        },
        /**
         * listen the event from eActionPinnedFile, and send the deskid to tabsBar
         */
        showPinnedBkm: function ($event) {
            if ($event) {
                this.$emit('showPinnedBkm', $event);
            }
        },

        /**
        * Détérmine la fiche incruster à afficher
        */
        async getFileByFileId(needFirst) {
            var findElement = this.aFilesidIncrement.find(a => a.pos == this.file);
            if (findElement && !needFirst) {
                let oDataDetail = this.loadFileDetail();
                let oFileLayout = this.loadFileLayout();
                let finalCountDown = this.linkToCall(oDataDetail.url, { ...oDataDetail.params, nTab: parseInt(this.propBkm?.DescId), nFileId: parseInt(findElement.fileId)});
                let oFinalLayout = this.linkToCall(oFileLayout.url, { ...oFileLayout.params, nTab: parseInt(this.propBkm?.DescId) })
                await finalCountDown.then((results) => {
                    this.oFileDetailIncrustBkm = JSON.parse(results);
                });
                await oFinalLayout.then((results) => {
                    this.oFileLayoutIncrustBkm = JSON.parse(results);
                });
                this.setDetailStruc(true)
            } else {
                let oDataBookmark = await loadFileBookmark();

                if (!(oDataBookmark))
                    return;

                let oFileLayout = this.loadFileLayout();
                let oFinalLayout = this.linkToCall(oFileLayout.url,{ ...oFileLayout.params, nTab: parseInt(this.propBkm?.DescId) });


                await oFinalLayout.then((results) => {
                    this.oFileLayoutIncrustBkm = JSON.parse(results);
                });

                var neededPage = Math.ceil(this.file / 10)
                let number = 10;
                let param = {
                    ParentTab: this.getTab,
                    ParentFileId: this.getFileId,
                    Bkm: this.oFileLayoutIncrustBkm.Tab,
                    RowsPerPage: number,
                    Page: neededPage,
                };

                let oDataJson = this.linkToCall( oDataBookmark.url,{ ...oDataBookmark.params, ...param })
                await oDataJson.then((results) => {
                    JSON.parse(results).Data.forEach((row, idx) => {
                        var findElement = this.aFilesidIncrement.find(a => a.fileId == row.MainFileId);
                        if (!findElement) {
                            this.aFilesidIncrement.push({
                                pos: ((neededPage * 10 ) + idx+1) - 10,
                                fileId: row.MainFileId
                            })
                        }
                    });
                });

                // On fait un sort sur le tableau pour avoir une cohérence sur les position des fileId
                this.aFilesidIncrement.sort((a, b) => (a.pos > b.pos) ? 1 : ((b.pos > a.pos) ? -1 : 0));

                var findElement = this.aFilesidIncrement.find(a => a.pos == this.file);
                let oDataDetail = this.loadFileDetail();
                let finalCountDown = this.linkToCall(oDataDetail.url, { ...oDataDetail.params, nTab: parseInt(this.propBkm?.DescId), nFileId: parseInt(findElement.fileId) });
                await finalCountDown.then((results) => {
                    this.oFileDetailIncrustBkm = JSON.parse(results);
                });
                // Une fois le tableau remple avec les nouvelles valeurs on refait un tour dans se tableau pour afficher la fiche qu'on a retrouver
                this.getFileByFileId();
            }
            this.fileUpdate++;
            this.memoUpdate++;
            this.initNavigationDrawer(true)
        },

         /**
         * Permet de changer de fiche avec la pagination
         * @param {string} both prev / next / searchFile
         * 
         */
         getFile(both) {
            if (both == 'prev' && this.file >= 2) {
                this.file = this.file - 1;
                this.getFileByFileId();
            } else if (both == 'next' && this.file < this.nbRow) {
                this.file = this.file + 1;
                this.getFileByFileId();
            } else if (both == 'searchFile') {
                var fileNeeded = parseInt(this.$refs.ePagination.$refs.searchFileValue.value);
                if (fileNeeded <= this.nbRow && fileNeeded > 0 && fileNeeded != this.file) {
                    this.file = parseInt(this.$refs.ePagination.$refs.searchFileValue.value);
                    this.getFileByFileId();
                } else {
                    this.$refs.ePagination.$refs.searchFileValue.value = this.file;
                }
            }
        },
        /** Supprime la fiche incrustée */
        deleteFile(e){
            let oPinnedBkm = {
                ...this.propBkm,
                pin: {
                    type: "pinned-bkm",
                    val: true
                },
                descId: this.getPinnedFileDescId,
                ViewMode: BKMVIEWMODE?.LIST
            };
            let callBack = ()=> {             
                this.$emit('pinnedBkm', oPinnedBkm);
            }
            deleteFile(this.getPinnedFileDescId,this.oFileDetailIncrustBkm?.Data?.MainFileId,undefined,undefined,callBack)
        },
        /** Ouvrir la fenêtre de création de fiche depuis le Finder (PP/PM) */
        createFileWithFinder($event) {
            this.openLnkFileDialog(1, this.oFileDetailIncrustBkm?.Structure?.StructFile?.DescId, null, 1);
        },
        /** Pour la duplication de la fiche */
        duplicateFile($event) {
            this.shFileInPopup(
                this.oFileDetailIncrustBkm?.Structure?.StructFile?.DescId,
                this.oFileDetailIncrustBkm?.Data?.MainFileId,
                this.getRes(534),
                null,
                null,
                0,
                '',
                true,
                null,
                6
            );
        }
    },
    props: {
        propNbRow: Number,
        fileIdPinned: [Number, String],
        propBkm: Object,
        loadAnotherBkm: {
            type: Boolean,
            default: false
        },
        propDetailBkm: {
            type: Promise
        },
        propBkmFileLayout: {
            type: Promise
        },
        isDisplayed:{
            type:Boolean
        },
        nForceRefreshTabsBar:{
            type:Number
        }
    },
    watch: {
        fileIdPinned: async function () {
            await this.initNavigationDrawer();

            /**
            * Construit un tableau de position et de fileId
            */
            this.propBkm.aFileId?.forEach((row, idx) => {
                var findElement = this.aFilesidIncrement.find(a => a.pos == row.pos);
                if (!findElement) {
                    this.aFilesidIncrement.push({
                        pos: row.pos,
                        fileId: row.fileId
                    })
                }
            })
        }
    },
    mixins: [eFileMixin, eFileComponentsMixin],
    async created() {
        await this.initNavigationDrawer();
        this.aFilesidIncrement = this.propBkm.aFileId || [];
    },
    template: `
<div :id="'bkmTitle_' + oFileDetailIncrustBkm?.Structure?.StructFile?.DescId" vm="1">
    <v-row class="topActionsIncrustedFile d-flex pb-3">
        <ePagination
            v-if="nbRow > 1"
            ref="ePagination"
            @actions="getFile"
            :porp-nb-files="nbRow"
            :prop-file="file"
        />
        <shortcutBar
            class="pinnedShortcutBar"
            :props-avatars="getShortcutMenuElements"
        />
        <eActionPinnedFile
            :prop-action="oFileDetailIncrustBkm"
            :prop-detail="tabAllDetail"
            :prop-layout="oFileLayoutIncrustBkm"
            @showParentSignet="showPinnedBkm($event)"
            @deleteFile="deleteFile"
            @createFileWithFinder="createFileWithFinder($event)"
            @duplicateFile="duplicateFile($event)"
        />
    </v-row>
    <fileDetail
        :isDisplayed="isDisplayed"
        :key="fileUpdate"
        :show-detail-for-separator="showDetailForSeparator"
        :prop-reload-detail-function="false"
        :prop-cols="oFileLayoutIncrustBkm?.NbCols || 2"
        :prop-data-detail="getTabAllDetailFileDetail"
    />
    <tabPinnedNavigationDrawer :key="memoUpdate" v-if="aFileNavigationDrawer.length" :propTabs="aFileNavigationDrawer" />
</div>`
};