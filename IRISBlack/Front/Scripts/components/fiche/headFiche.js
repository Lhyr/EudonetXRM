import { dynamicFormatChamps } from '../../../index.js?ver=803000';
import { infoTooltip, isShowingInformation, isDisplayingToolTip, showTooltip } from '../../methods/eComponentsMethods.js?ver=803000';
import { OpenModalPicture, doGetImageGeneric, ErrorImageDeleteReturn, ImageDeleteReturn, onImageCancel } from '../../methods/eComponentImageMethods.js?ver=803000';
import { specialCSS, forbiddenFormatHead, observeRightMenu } from "../../methods/eFileMethods.js?ver=803000";
import { tabFormatForbid, tabFormatBtnLbl, tabFormatBtnSep, tabFormatForbidHeadEdit } from "../../methods/eFileConst.js?ver=803000";
import EventBus from '../../bus/event-bus.js?ver=803000';
import { eFileMixin } from '../../mixins/eFileMixin.js?ver=803000';
import { FieldType } from '../../methods/Enum.min.js?ver=803000'
import { getIrisPurpleActived, openFileGuidedTyping, openLnkFileDialog, printFile } from '../../shared/XRMWrapperModules.js?ver=803000'
import { shareFile, emitPropertiesDialog, getShareFileURL } from "../../methods/eActionPinnedFileMethods.js?ver=803000"
import { JSONTryParse } from "../../methods/eMainMethods.js?ver=803000"
import { eFileComponentsMixin } from '../../mixins/eFileComponentsMixin.js?ver=803000';

export default {
    name: "headFiche",
    data() {
        return {
            nInputs: Number,
            res: top,
            reloadHead: true,
            maxFieldCountInHead: 6, /* US #1 333, Tâche #2 022 - La Zone Résumé est limitée à X rubriques maximum */
            timerHeadElm: null,
            nValMinScroll: [1, 100] /** Position de bascule minimum, suivant le type de navbar qu'on a. */,
            disabled: false,
            tabFormatForbid,
            tabFormatBtnLbl,
            tabFormatBtnSep,
            reloadImg: true,
            catalogInfos: false,
            colSize: 'col',
            newResume: false,
            observer: null,
            tabFormatForbidHeadEdit,
            FieldType,
            tabFormatForbidLabel: [],
            geolocCoord: {},
            urlDomainRegex:/(www[.]|[.]com)/gm,
            reduceSummary: false,
            shortCutType: 'headFile'
        };
    },
    components: {
        eFileLoader: () => import(AddUrlTimeStampJS("./FileLoader.js")),
        eImage: () => import(AddUrlTimeStampJS("../formatChamps/eImage.js")),
        eMenuDropDown: () => import(AddUrlTimeStampJS("../subComponents/menus/eMenuDropDown.js")),
        eListMenuDropDown: () => import(AddUrlTimeStampJS("../subComponents/menus/eListMenuDropDown.js")),
        shortcutBar: () => import(AddUrlTimeStampJS("./headFile/shortcutBar.js"))
    },
    mixins: [eFileMixin, eFileComponentsMixin],
    props: {
        propHead: Object,
        propGo: Boolean,
        propertyFiche: Array,
        IsUpdatable: Boolean,
        oFileActions: Object,
        DataStruct: Object
    },
    mounted() {

        EventBus.$on('loadFinish', options => {
            this.callBackEmitLoadAll(options);
        });

        this.$parent.$refs["mainContentWrap"].addEventListener('scroll', async (event) => {

            let currentPosYScroll = await event.target.scrollTop;
            let nPosMax = this.$parent?.$refs?.propertyFiche?.classList?.contains("isStickyHead") ? this.nValMinScroll[0] : this.nValMinScroll[1];

            this.$parent.$refs.propertyFiche?.classList.toggle("isStickyHead", currentPosYScroll > nPosMax);
            this.changeAvatarBySize(currentPosYScroll > nPosMax, "image-fiche-reduce");
            this.reduceSummary = currentPosYScroll > nPosMax;
            this.colSize = currentPosYScroll > nPosMax ? 'col-auto' : 'col';

            if (this.$refs["hTitle"])
                this.$refs["hTitle"].classList.toggle('profile-username-title-reduce', currentPosYScroll > nPosMax);

            clearTimeout(this.timerHeadElm);
            this.timerHeadElm = null;
        });


        /** En cas de resize, si on descend de trop pour le size,
         * on ajoute une classe qui fait passer la big picture en 
         * mobile picture. Truc comme ça... 
         * Et bien sûr petit hack pour l'ami Saf, sans qui nos développements
         * seraient ce chemin pavé de pétales de roses... */
        try {
            var ro = new ResizeObserver(entries => {
                for (let entry of entries) {
                    this.changeAvatarBySize(window.matchMedia("(max-width: 1100px)").matches);
                }
            });

            /** On observe la box du résumé. */
            ro.observe(this.$refs["boxResume"]);
        }
        catch {
            this.$refs["headFiche"]?.addEventListener("resize", () => this.changeAvatarBySize(window.matchMedia("(max-width: 1100px)").matches));
        }

        /** On initialise l'affichage de l'avatar. */
        this.$nextTick(() => this.changeAvatarBySize(window.matchMedia("(max-width: 1100px)").matches));
    },
    updated() {
        this.changeAvatarBySize(window.matchMedia("(max-width: 1100px)").matches);
    },
    methods: {
        openFileGuidedTyping,
        getIrisPurpleActived,
        openLnkFileDialog,
        shareFile,
        emitPropertiesDialog,
        getShareFileURL,
        /**
         * METHOD POUR EMIT LES OPTIONS DU MODAL (PROPRIETE DE LA FICHE)
         * */
        async emitMethod() {
            this.emitPropertiesDialog();
        },
        observeRightMenu,
        specialCSS,
        forbiddenFormatHead,
        /**
         * Permet de modifier les classes de l'avatar (ou des avatars...),
         * suivant qu'on est en mobile ou miséra... ou en desktop.
         * @param {any} bResize si on ajoute la classe.
         * @param {any} cssClass la classe 
         * @returns {any} rien, c'est une procedure, mais bon jslint...
         */
        changeAvatarBySize(bResize, cssClass = "imgMobile") {
            let imgFiche = this.$refs["imgFiche"];

            if (!imgFiche)
                return false;

            if (!imgFiche.classList)
                imgFiche = imgFiche.$el;

            if (!imgFiche.classList)
                return false;

            imgFiche.classList.toggle(cssClass, bResize);
        },

        callBackEmitLoadAll(options) {
            let that = this;
            if (options)
                if (options.reloadHead || options.reloadAll) {
                    that.reloadHead = false;
                    Vue.nextTick(() => that.reloadHead = true);
                }
        },
        failImgs(source) {
            source.target.src = "IRISBlack/Imgs/avatarBroken.jpg";
        },
        dynamicFormatChamps,
        infoTooltip,
        isShowingInformation,
        isDisplayingToolTip,
        showTooltip,
        OpenModalPicture,
        getModifTitle() {

            let options = {
                typeModal: "info",
                type: "zoom",
                close: true,
                maximize: true,
                col: 'col-md-12',
                width: 600,
                id: 'modifHead',
                observeMenu: (bVal, ctx) => {
                    this.observeRightMenu(bVal, ctx)
                },
                rightMenuWidth: window.GetRightMenuWidth(),
                title: this.getRes(7216) + ' & ' + this.getRes(7109),
                btns: [{ lib: this.getRes(30), color: 'default', type: 'left' }],
                datas: this.propHead
            };
            EventBus.$emit('globalModal', options);

        },
        /**
         * Permet l'édition d'une fiche à l'aide de la saisie filoguidée.
         * */
        editFile() {
            this.openFileGuidedTyping(this.getTab, this.getFileId, "", 2);
        },
        /**
         * Récupère un objet et en retourne la valeur à afficher.
         * @param {any} obj
         */
        getTextFromObject: function (obj) {

            if ((!obj)
                || (parseInt(obj.Value) == 0 && !obj.DisplayValue))
                return "";

            let sTitle = obj.DisplayValue || obj.Value || "";

            if (Array.isArray(sTitle))
                return sTitle.join(",");

            return sTitle;
        },
        /**
        * Cette fonction consiste à récupérer du back les informations du menu.
        */
        async getMenus() {
            let oDataJson = JSONTryParse(
                await this.FileMenu
            );

            return oDataJson;
        },
        /**
        * On récupère l'action du bouton cliquer dans le menu.
        * @param {any} item
        */
        menuAction(item) {
            // Récupère la fonction correspondant au nom donné dans la propriété "action" sur le contexte actuel (this) et l'exécute si on a bien une fonction à ce nom
            let targetFctName = item.action;
            let targetFctArgs = "";
            if (targetFctName.indexOf("(") > -1) {
                targetFctArgs = targetFctName.substring(targetFctName.indexOf("(") + 1, targetFctName.indexOf(")")).split(",").map(
                    function (argument) {
                        // Convertit chaque argument du tableau en type simple (boolean, int) plutôt qu'en string lorsqu'applicable
                        // ATTENTION : nécessite qu'un paramètre String soit passé entre "" et non ''
                        let parsedArgument = null;
                        try {
                            parsedArgument = JSON.parse(argument);
                        }
                        catch (ex) {
                            parsedArgument = argument;
                        }
                        return parsedArgument;
                    });
                targetFctName = targetFctName.substring(0, targetFctName.indexOf("("));
            }
            let targetFct = this[targetFctName];
            if (typeof targetFct == "function")
                targetFct.apply(targetFctArgs);
        },

        /** Permet d'afficher le monde liste */
        goList: function () {
            this.goTabList(this.getTab, true, null, () => false, true);
        },

        /** Ouvrir la fenêtre de création de fiche (hors PP/PM) */
        goCreateFile: function () {
            this.shFileInPopup(this.getTab);
        },
        /** Ouvrir la fenêtre de création de fiche (hors PP/PM) pour les fichiers principaux
         * @param {boolean} bApplyCloseOnly Indique si on doit uniquement afficher les boutons Appliquer et Fermer */
        goCreateMainFile: function (bApplyCloseOnly) {
            // #41277 : Modification paramètre nCallFrom 3 -> 1 pour ne pas garder de liaison PP/PM
            this.shFileInPopup(this.getTab, 0, this.getRes(31), null, null, 0, null, bApplyCloseOnly, null, 1);
        },
        /** Ouvrir la fenêtre de création de fiche (hors PP/PM) pour les fichiers principaux
         * @param {boolean} bApplyCloseOnly Indique si on doit uniquement afficher les boutons Appliquer et Fermer */
        goCreateUserFile: function (bApplyCloseOnly) {
            // #41277 : Modification paramètre nCallFrom 3 -> 1 pour ne pas garder de liaison PP/PM
            this.shFileInPopup(this.getTab, 0, this.getRes(31), null, null, 0, null, false, null, 2, null, null, "nsAdminUsers.InitDefault");
        },
        /** Ouvrir l'assistant création en création (hors PP/PM) */
        goCreatePurpleFile: function () {
            this.openPurpleFile(this.getTab, 0, '', 18);
        },
        /** Ouvrir la fenêtre de création de fiche depuis le Finder (PP/PM) */
        goCreateFileWithFinder: function () {
            this.openLnkFileDialog(1, this.getTab, null, 1);
        },
        /** Ouvrir l'assistant création en création depuis le Finder (PP/PM) */
        goCreatePurpleFileWithFinder: function () {
            this.openLnkFileDialog(1, this.getTab, null, 18);
        },
        /** Ouvrir l'assistant création en création depuis le Finder (PP/PM) */
        goAutoCreate: function () {
            this.autoCreate(this.getTab);
        },
        goEditPurpleFile: function () {
            this.editFile();
        },

        /** Pour la duplication de la fiche */
        goDuplicateFile: function () {
            this.shFileInPopup(this.getTab, this.getFileId, this.getRes(534), null, null, 0, '', true, null, 6);
        },

        /** permet de supprimer les fichiers. */
        goDeleteFile: function () {
            this.deleteFile(this.getTab, this.getFileId);
        },
        /** Publipostage */
        goPublipostage: function () {
            this.reportList(3, 0);
        },
        /** Export */
        goExport: function () {
            this.reportList(2, 0);
        },
        /** Report */
        goReport: function () {
            this.reportList(0, 0);
        },
        /** Pour afficher les graphics. */
        goGraphics: function () {
            this.reportList(6, 0);
        },
        /** Pour épingler */
        goPinFile: function () {
            window.open(this.getShareFileURL(this.getTab, this.getFileId, this.getFileHash), "_blank");
        },
        /** Pour imprimer */
        goPrintFile: function () {
            this.printFile();
        },
        /** Pour afficher les propriétés de la fiche */
        goProperties: function () {
            this.emitMethod();
        },
        /** Pour partager la fiche */
        goShareFile: function () {
            this.shareFile(this.getTab, this.getFileId, this.getFileHash);
        }
    },
    computed: {
        /**
         * Retourne un tableau tronqué des éléments à afficher.
         * On enlève les formats interdits.
         * @returns {Array} un tableau de 1 à 6 des éléments à afficher.
         * */
        getElementsToDisplay() {
            let tabINputs = this.propHead.inputs
                .filter(ip => !this.tabFormatForbid.includes(ip.Format))
                .slice(0, this.maxFieldCountInHead);

            this.nInputs = tabINputs.length;

            return tabINputs;
        },
        /**
         * Tout est en place pour afficher le template.
         * */
        displayTemplate() {
            return this.reloadHead;
        },
        /**
         * Renvoie le type d'image à mettre à jour lors du clic sur un Avatar, en fonction du TabId en cours (cf. eRender.RenderImageFieldFormat) 
         * */
        getAvatarOpenModalPictureType() {
            return getTab == /*TableType.USER*/ "101000" ? "LNKOPENUSERAVATAR" : "LNKOPENAVATAR";
        },
        getCatSbTitle() {
            if (this.propHead.sTitle.Format == FieldType.Catalog && this.propHead.sTitle.DisplayValue != "")
                return this.propHead.sTitle.DisplayValue.split(";");
            else if (this.propHead.sTitle.Format == FieldType.Catalog && this.propHead.sTitle.Value != "")
                return this.propHead.sTitle.Value.split(";");
        },
        isCatalog() {
            return (this.catalogInfos) ? 'catalog-infos' : ''
        },
        spaceBetweenElem() {
            return (this.propHead.sTitle) ? 'spaceBetweenElem' : ''
        },
        headerContentCol() {
            return (this.propHead.avatar) ? 'col' : 'col'
        },
        imgCssClass() {
            return (this.getTab == eTools.DescIdEudoModel.TBL_PP) ? 'contact-img' : 'resume-img';
        },
        reloadImgComp() {
            return !this.reloadImg ? 'reloadingImg' : '';
        },
        /** Déterminer si on peut afficher l'icone de la saisie guidée.
         * Il faut que l'admin ait activé la saisie guidée en modification
         * et que la modification soit permise.
         * */
        getDisplayModifyFile: function () {
            return this.oFileActions?.AddPurpleFileFromMenu && this.IsUpdatable;
        },
        getFileHash() {
            let hash = "";
            try {
                hash = this.DataStruct?.Structure?.StructFile?.FileHash;
            }
            catch (e) {
                console.log(e);
            }
            return hash;
        },
        /** classe css de la div summary-fields--wrapper qui englobe les champs de la zone résumé */
        summaryFldWrpClass: function(){
            return {
                'pl-5':this.propHead.avatar,
                'd-none':this.reduceSummary
            }
        }
    },
    directives: {
        loaded: {
            inserted: function (el, binding, vnode) {
                if (vnode.context.propHead.sTitle.Format == 2) {
                    vnode.context.catalogInfos = true;
                }
                else
                    vnode.context.catalogInfos = false;
            }

        }
    },
    template: `


<div
    id="headFiche"
    ref="headFiche"
    :class="[
        isCatalog,
        'box box-primary',
        !(propHead.avatar) ? 'avatarEmpty' : '',
        !(nInputs > 0) ? 'inputsEmpty' : ''
    ]"
>
    <v-row ref="boxResume" class="box-body box-profile boxResume px-3">
        <v-container fluid
            class="pa-0 ma-0"
            v-if="displayTemplate"
        >
            <v-row class="right btn-modifier-header-fiche pt-2">
                <shortcutBar
                    :props-avatars="getShortcutMenuElements"
                />
                <eMenuDropDown
                    :props-menus="getMenus()" 
                    :pseudo-slot="(props) => props.menu"
                >
                    <template slot-scope="props">
                        <eListMenuDropDown @menu-action="menuAction" :props-actions="props.menu.actions" />
                    </template>
                </eMenuDropDown>
            </v-row>
            <v-row :class="[!(propHead.avatar) ? 'noAvatar' : '','grpTitleName group grp-respon d-flex flex-column  justify-center']">
                <div :class="[spaceBetweenElem,'contentImgResum']">
                    <h3 @click="getModifTitle()" ref="hTitle"  class="profile-username-title">
                        {{ ((propHead.titleComplementValue) ? getTextFromObject(propHead.titleComplementValue) + ' ' : '') + ( getTextFromObject(propHead.title))}}
                    </h3>
                    <span @click="getModifTitle()" class="modifIcoTitle"><a href="#!" class="hover-pen"><i class="fas fa-pencil-alt"></i></a></span>
                </div>
                <div v-loaded v-if="propHead.sTitle && !forbiddenFormatHead(propHead.sTitle.Format, tabFormatForbidHeadEdit)" class="text-muted">
                    <template v-if="propHead.sTitle.Format != FieldType.Catalog">
                        <span @click="getModifTitle()">{{ getTextFromObject(propHead.sTitleComplementValue) }}</span>
                        <span @click="getModifTitle()">{{ getTextFromObject(propHead.sTitle) }}</span>
                    </template>
                    <template v-else>
                        <span v-on:click="getModifTitle()" class="sb-title" v-for="sb in getCatSbTitle">{{sb}}</span>
                    </template>
                </div>
            </v-row>
            <template v-if="propHead.avatar">
                <eImage
                    ref="imgFiche"
                    :class="[
                        'image-fiche d-flex justify-center align-center',
                        colSize,
                        imgCssClass,
                        !(nInputs > 0) ? 'image-fiche-reduce-noInput' : ''
                    ]"
                    :prop-head="true"
                    :prop-head-avatar="true"
                    :data-input="propHead.avatar"
                />
            </template>

            <div :class="['d-flex flex-column header-content justify-center py-2 pr-0 align-self-center summary-fields--wrapper',summaryFldWrpClass]">
                <div
                    v-if="propHead.inputs"
                    class="container--resume detailContent"
                >
                    <div
                        :class="input.ReadOnly ? 'readOnlyComponent' : ''"
                        :format="input.Format"
                        :bf="input.Formula ? '1' : '0'"
                        :mf="input.HasMidFormula || input.HasORMFormula ? '1' : '0'"
                        :FileId="input.FileId"
                        :DivDescId="input.DescId"
                        v-if="input.IsVisible && !forbiddenFormatHead(input.Format)"
                        v-for="input in getElementsToDisplay"
                        :key="input.id" class="button--resume"
                    >
                        <div :class="{'emptyField':!input?.Value || !input?.DisplayValue}" class="group-inner">
                            <div 
                                v-if="input.Format != 18" 
                                :class="{
                                    'labelHidden': input.LabelHidden || specialCSS(tabFormatBtnLbl, input.Format),
                                    'no-label': specialCSS(tabFormatBtnSep, input.Format)
                                }" 
                                class="left"
                            >
                                <div
                                    :ref="'label_' + input.DescId"
                                    :style="{ color: input.StyleForeColor}" 
                                    class="left-label text-muted" 
                                    :class="{
                                        'italicLabel': input.Italic,
                                        'boldLabel': input.Bold,
                                        'underLineLabel': input.Underline,
                                        'labelHidden': input.LabelHidden || specialCSS(tabFormatBtnLbl, input.Format),
                                        'no-label':specialCSS(tabFormatBtnSep, input.Format)
                                    }"
                                >
                                    {{input.Label}}
                                </div>
                                <span
                                    v-if="input.Required"
                                    :ref="'hidden_' + input.DescId"
                                    :class="{'labelHidden': input.LabelHidden || specialCSS(tabFormatBtnLbl, input.Format) ,'no-label':specialCSS(tabFormatBtnSep, input.Format)}"
                                    class="requiredRubrique"
                                >*</span>
                                <span
                                    v-if="isShowingInformation(input)"
                                    :ref="'label_info' + input.DescId"
                                    class="icon-info-circle info-tooltip"
                                    @mouseover="isDisplayingToolTip(true, input)"
                                    @mouseout="isDisplayingToolTip(false, input)"
                                ></span>
                            </div>
                                
                            <component
                                :prop-head="true"
                                :data-input="input"
                                :is="dynamicFormatChamps(input)"
                            />
                        </div>
                    </div>
                </div>
            </div>
        </v-container>
    </v-row>
</div>
`,
}