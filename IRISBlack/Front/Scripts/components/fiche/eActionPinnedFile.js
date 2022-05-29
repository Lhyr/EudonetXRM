import EventBus from "../../bus/event-bus.js?ver=803000";
import { eFileMixin } from '../../mixins/eFileMixin.js?ver=803000';
import containerModal from '../modale/containerModal.js?ver=803000';
import { LoadStructBkm, linkToPost, setViewMode, observeRightMenu } from "../../methods/eFileMethods.js?ver=803000";
import { store } from '../../../Scripts/store/store.js?ver=803000';
import eAxiosHelper from "../../helpers/eAxiosHelper.js?ver=803000";
import { loadPinnedBookmark, openFileGuidedTyping } from "../../shared/XRMWrapperModules.js?ver=803000";
import { shareFile, openPlainFile, emitPropertiesDialog, reloadBkm, changeViewMode } from "../../methods/eActionPinnedFileMethods.js?ver=803000";
import { EdnType } from '../../methods/Enum.min.js?ver=803000';

export default {
    name: "eActionPinnedFile",
    data() {
        return {
            IframeBkmCol: null,
            propertyFiche: [],
            jsonFront: [
                {
                    "index": 2,
                    "name": this.getRes(2067),
                    "idGrp": "display",
                    actions: []
                },
                {
                    "index": 3,
                    "name": this.getRes(296),
                    "idGrp": "action",
                    actions: []
                }
            ]
        };
    },
    components: {},
    props: {
        propAction: {
            type: Object,
            default: {}
        },
        propDetail: {
            type: Array,
            default: []
        },
        propLayout: {
            type: Object,
            default: {}
        }
    },
    mixins: [eFileMixin],
    template: `
<div
    class="d-inline-flex justify-end align-center"
    id="pinned_file_button"
    @mouseover.stop="$emit('hoverOnActionBtn')"
>
    <div v-for="tpAction in jsonFront" 
        :class="['btn-group','btn-' + tpAction.idGrp]" 
        :key="tpAction.id"
        v-on="tpAction.actionMainBtn ? { ...tpAction.actionMainBtn } : {}"
        >
        <div v-if="tpAction.actions.length || tpAction.actionMainBtn" class="btn-group dropdown drop-100 ">
            <li class="dropdown drop-100">
                <button type="button" class="btn btn-default drop-100 pinned-file--btn" data-toggle="dropdown" aria-expanded="false">
                    <span class="title">{{tpAction.name}}
                        <span v-if="tpAction.actions.length" class="fas fa-chevron-down pinned-file--dropdown"></span>
                    </span>
                </button>
                <div :id="'fitres_nav_' + tpAction.name" class="dropdown-content-action" role="menu">
                    <template v-for="lnkAction in tpAction.actions">
                        <a :class="[lnkAction.type == 4 && propAction.HistoricActived ? 'actived' : '']" v-on:click="getAction(lnkAction)" href="#!">
                            <i :class="['marg-right ' + lnkAction.icon]"></i>
                            {{lnkAction.name}}
                        </a>
                    </template>
                </div>
            </li>
        </div>        
    </div>
</div>`,

    methods: {
        LoadStructBkm,
        linkToPost,
        loadPinnedBookmark,
        shareFile,
        openPlainFile,
        emitPropertiesDialog,
        reloadBkm,
        changeViewMode,
        setViewMode,
        observeRightMenu,
        openFileGuidedTyping,
        rewriteJson: function () {
            this.jsonFront = [
                {
                    "index": 0,
                    "name": this.getRes(18),
                    "idGrp": "add",
                    actions: []
                },
                {
                    "index": 2,
                    "name": this.getRes(2067),
                    "idGrp": "display",
                    actions: []
                },
                {
                    "index": 3,
                    "name": this.getRes(296),
                    "idGrp": "action",
                    actions: []
                }
            ];

            for (var i = 0; i < this.jsonFront.length; i++) {
                if (this.jsonFront[i].idGrp == 'add') {
                    this.jsonFront[i].actions.push({
                        "DescIdSignet": this.propAction.DescId,
                        "type": 7,
                        "name": this.getRes(31),
                        "icon": "fa fa-plus-square",
                        "id": this.propAction?.id
                    })
                    this.jsonFront[i].actions.push({
                        "DescIdSignet": this.propAction.DescId,
                        "type": 8,
                        "name": this.getRes(534),
                        "icon": "fas fa-copy"
                    })
                } else if (this.jsonFront[i]?.idGrp == 'display') {
                    this.jsonFront[i].actions.push({
                        "DescIdSignet": this.getStructFile?.DescId,
                        "type": 1,
                        "name": this.getRes(8800),
                        "icon": "fa fa-eye"
                    });
                }
                else if (this.jsonFront[i]?.idGrp == 'action') {
                    this.jsonFront[i].actions.push({
                        "DescIdSignet": this.getStructFile?.DescId,
                        "type": 2,
                        "name": this.getRes(3070),
                        "icon": "fas fa-eye"
                    });

                    this.jsonFront[i].actions.push({
                        "DescIdSignet": this.getStructFile?.DescId,
                        "type": 3,
                        "name": this.getRes(8801),
                        "icon": "fas fa-user"
                    });

                    if (this.propAction?.actDetail?.AddPurpleFileFromMenu) {
                        this.jsonFront[i].actions.push({
                            "DescIdSignet": this.getStructFile?.DescId,
                            "type": 4,
                            "name": this.getRes(3069),
                            "icon": "fas fa-hat-wizard"
                        });
                    }
                    this.jsonFront[i].actions.push({
                        "DescIdSignet": this.getStructFile?.DescId,
                        "type": 6,
                        "name": this.getRes(19),
                        "icon": "fas fa-trash-alt"
                    });

                    this.jsonFront[i].actions.push({
                        "DescIdSignet": this.getStructFile?.DescId,
                        "type": 5,
                        "name": this.getRes(8027),
                        "icon": "fas fa-share-alt"
                    });
                }
            }
        },
        getAction: function (tpAction) {

            switch (tpAction.type) {
                case 1: return this.ShowPinnedBkm(tpAction); // Voir la liste épinglée 8800
                case 2: return this.ShowRecord(tpAction); // Naviguer 3070
                case 3: return this.ShowProperties(tpAction); // Afficher les propriétés 8801
                case 4: return this.ModifyWithAssistant(tpAction); // Modifier avec l'assistant 3069
                case 5: return this.Share(tpAction); // Partager 8027
                case 6: return this.DeleteFile(tpAction); // Supprimer 19
                case 7: return this.goCreateFileWithFinder(tpAction); // add
                case 8: return this.goDuplicateFile(tpAction); // Dupliquer
            }

        },
        /** Ouvrir la fenêtre de création de fiche depuis le Finder (PP/PM) */
        goCreateFileWithFinder: function (tpAction) {
            this.$emit('createFileWithFinder', tpAction)
        },
        /** Pour la duplication de la fiche */
        goDuplicateFile: function (tpAction) {
            this.$emit('duplicateFile', tpAction)
        },
        /**
         * Change le mode du signet épinglé.
         * @param {any} tpAction
         */
        ShowPinnedBkm(tpAction) {
            this.$emit('showParentSignet', this.propAction.Structure?.StructFile?.DescId);
        },
        /**
         * affiche la popup de modification.
         * @param {any} tpAction
         */
        ShowRecord (tpAction) {
            this.openPlainFile(this.getStructFile?.DescId, this.getFileId);
        },
        /**
         * affiche les propriétés de l'enregistrement
         * @param {any} tpAction
         */
        async ShowProperties (tpAction) {
            await this.ConstructPropertyFile();
            this.emitPropertiesDialog();
        },
        /**
         * Iris Purple/Saisie guidée.
         * @param {any} tpAction
         */
        ModifyWithAssistant: function (tpAction) {
            this.openFileGuidedTyping(this.getStructFile?.DescId, this.getFileId, "", 2);
        },
        /**
         * Partage.
         * @param {any} tpAction
         */
        Share: function (tpAction) {
            this.shareFile(this.getStructFile?.DescId, this.getFileId, this.getFileHash);
        },
        /**
         * Suppression.
         * @param {any} tpAction
         */
        DeleteFile: function (tpAction) {
            this.$emit('deleteFile', tpAction);
        },
        /**
         * Construit les propriétés de la fiche.
         * @param {any} EdnType un enum dont j'ai besoin plutot que de l'importer
         * */
       ConstructPropertyFile: function () {
            let tabIDMainFile = [99, 88, 84, 95, 97, 93, 96, 98, 74];
            let tabIDElseType = [99, 90, 84, 95, 97, 93, 96, 98, 74];
            //Utilisation de la méthode sort() pour mettre dans l'ordre souhaité les champs (par rapport à sortingArr)
            let sortingArr = [99, 88, 90, 96, 98, 84, 95, 97, 74, 93];

            // DEBUT ZONE PROPRIETE DE LA FICHE //
            // Set value proppriété de la fiche
           this.propertyFiche = this.propDetail.filter(a => (
                (this.propAction?.Structure?.StructFile?.EdnType == EdnType.FILE_MAIN
                   && tabIDMainFile.map(num => parseInt(this.propLayout?.Tab) + num).includes(a.DescId))
               || tabIDElseType.map(num => parseInt(this.propLayout?.Tab) + num).includes(a.DescId)
            )).sort((a, b) => {
                return sortingArr.indexOf(a.DescId - this.propLayout?.Tab) - sortingArr.indexOf(b.DescId - this.propLayout?.Tab);
            });
        }
    },
    computed: {
        /** récupère le hash de la fiche en cours, pour le partage de fiche ou l'affichage de Infos Debug */
        getFileHash() {
            return this.propAction?.Structure?.StructFile?.FileHash;
        },
        getStructFile() {
            return this.propAction?.Structure?.StructFile
        },
        getFileId() {
            return this.propAction?.Data?.MainFileId
        }
    },
    watch: {
        propAction: function (newVal) {
            if (newVal) {
                this.rewriteJson();
                this.$emit('JsonFront', this.jsonFront);
            }
        }
    }
};