import EventBus from '../../bus/event-bus.js?ver=803000';
import { loadFileBookmark } from "../../shared/XRMWrapperModules.js?ver=803000";
import { LoadSignet } from "../../methods/eFileMethods.js?ver=803000";
import { eListMixin } from '../../mixins/eListMixin.js?ver=803000';
import { handleExtendedProperties } from '../../methods/eComponentsMethods.js?ver=803000';

export default {
    name: "listContent",
    data() {
        return {
            DataCatalogue: null,
            DataJson: null,
            tables: [],
            error: false,
            load: false,
            reload: false,
            diffX: 0,
            nbPage: 2,
            maxRow: 60,
            nbLineCall: 10,
            pagination: false,
            scroll: false,
            tableObj: new Array(),
            nb: 0,
            nbBefore: 0,
            loadNewLine: false,
            pageInt: null,
            stopInfinity: false,
            isForced: false,
            noReloadForce: false,
            stopInfinityTop: false,
            obser: false,
            paginationOpen: false,
            bkmHeight: 5,
            iSetTimeout: 200,
            tableHeight: 28.125,
            listHeaderHeight: 3.125,
            listFooterHeight: 8.3125,
            cellHeight: 2.5,
            bkmHeaderHeight: 2.625,
            tableHeaderHeight: 2.6875,
            cellHeaderHeight: 2.6875,
            expressFilterMsgHeight: 3.125,
            cssStyle: {
            },
            setLoaderClass: this.propSignet.TableType == 11 ? 'bkmLoader' : '',
            mtxRelease: null,
            sortActivated: null,
            dragNDropMargin: 0.625
        };
    },
    props: {
        propSignet: Object,
        blocked: Boolean,
        forceRefreshTabsBar:Number
    },
    components: {

        eEmptyErrorList: () => import(AddUrlTimeStampJS("./listContent/eEmptyErrorList.js")),
        eHeadList: () => import(AddUrlTimeStampJS("./listContent/eHeadList.js")),
        eBodyList: () => import(AddUrlTimeStampJS("./listContent/eBodyList.js")),
        eFooterList: () => import(AddUrlTimeStampJS("./listContent/eFooterList.js")),
        ePagingList: () => import(AddUrlTimeStampJS("./listContent/ePagingList.js")),
        eErrorMessages: () => import(AddUrlTimeStampJS("../subComponents/eErrorMessages.js"))

    },
    computed: {
        /**
         * En cas d'erreur, d'information vide...
         * */
        displayAlert: function () {
            return this.load &&
                (this.error
                    || !this.DataJson
                    || this.DataJson == null
                    || this.DataJson?.PagingInfo?.NbTotalRows == 0);
        },

        /**
         * Détermine si on doit afficher la pagination.
         * */
        displayPaging: function () {
            return !this.error && this.load && this.DataJson != null && this.DataJson.PagingInfo.NbPages > 1 && this.pagination
        },

        /** Permet de savoir si on doit afficher certains éléments du footer. */
        displayBtnFooter: function () {
            return this.pagination && !this.scroll
        },

        myHeight: function () {
            let height = (this.scroll) ? "29.375rem" : "auto";

            if ((this.DataJson?.Data?.length > 0)
                && !this.scroll) {

                if (this.DataJson?.Data[0]?.LstDataFields?.length > 0)
                    height = this.tableHeaderHeight + (this.bkmHeaderHeight * this.DataJson.Data.length) + 'rem';
                else if (this.DataJson.Data[0].LstDataFields.length < 1)
                    height = 0.875 + (this.bkmHeaderHeight * this.DataJson.Data.length) + 'rem';

                if (this.propSignet.TableType == 11 && !this.pagination) {
                    height = (this.cellHeight * this.DataJson.PagingInfo.NbTotalRows) + this.cellHeaderHeight + 'rem';
                } else if (this.propSignet.TableType == 11 && this.pagination) {
                    height = (this.cellHeight * this.DataJson.PagingInfo.RowsPerPage) + this.cellHeaderHeight + 'rem';
                }

                if (this.pagination && this.propSignet.TableType != 11) {
                    height = "27.688rem";

                    if (this.loadNewLine) {
                        height = "27.688rem";
                        let signet = this.$parent.$refs[this.propSignet.id];

                        if (signet) {
                            signet.style.height = "37.5rem";
                            this.calHeight();
                        }
                    }
                }
            }

            if (this.propSignet.TableType == 11
                && this.DataJson?.Data?.length < 1
                && this.DataJson?.Structure?.ExpressFilterActivated) {
                height = this.expressFilterMsgHeight + this.dragNDropMargin + 'px'; //+5 pour créer une marge
            }
            return height;
        },

        /**
         * Est-ce qu'on en est � la derni�re page, ou est ce que le scroll infini est arr�t�.
         * */
        getEndPageOrInf() {
            if (this.DataJson?.PagingInfo)
                return this.DataJson.PagingInfo.Page == this.DataJson.PagingInfo.NbPages

            return this.stopInfinity;
        },
        /**
         * On regarde si la taille du tableau de donn�es est inf�rieur au nombre maximum de 
         * lignes � afficher.
         * */
        getDataLengthltMaxRow() {
            return this.DataJson?.Data?.length < this.maxRow
        },

        /**
         * S'il y a des donn�es � montrer dans la liste.
         * */
        getHasDataToShow() {
            return this.DataJson != null && this.getFlatTableToDisplay && this.getFlatTableToDisplay.length > 0;
        },
        /** retourne si le nombre d'element excède une page. */
        isNbRowsExceedPage: function () {
            return this.DataJson?.PagingInfo?.NbTotalRows > 10
        },
        /** 
         *  Détermine dynamiquement la taille d'un bookmark.
         * */
        setBkmSize() {
            let iSize = this.bkmHeight;

            if (!this.DataJson) {
                return;
            }


            if (this.propSignet.TableType == 11 && this.pagination) {
                // Demande 83733 Pb Affichage Signet Annexes en lecture seule
                if (this.propSignet.Actions.Add) {
                    // droit d'ajout
                    iSize = this.tableHeight + this.listHeaderHeight + this.listFooterHeight + this.dragNDropMargin/*- 10*/; // -10 marge d'erreur
                } else {
                    // pas de droit droit d'ajout
                    iSize = this.tableHeight + this.listHeaderHeight + this.listFooterHeight - 5.813;// -5.813  marge d'erreur + hauteur du drag and drop des annexes;
                }
            }
            else if (this.pagination) {
                iSize = 35;
            }
            else if (this.propSignet.TableType == 11 && this.DataJson.Data.length > 0) {
                // Demande 83733 Pb Affichage Signet Annexes en lecture seule
                if (this.propSignet.Actions.Add) {
                    // droit d'ajout
                    iSize = (this.listHeaderHeight + this.listFooterHeight) + (2.5 * this.DataJson.Data.length) + this.dragNDropMargin;
                } else {
                    // pas de droit droit d'ajout
                    iSize = (this.listHeaderHeight + this.listFooterHeight) + (2.5 * this.DataJson.Data.length) - 5 // -5 hauteur du drag and drop des annexes;
                }

            }
            else if (this.propSignet.TableType == 11 && this.DataJson.Data.length < 1) {
                iSize = this.listFooterHeight;
                //US 3906 T 6134
                if (this.DataJson?.Structure?.ExpressFilterActivated) {
                    iSize += this.expressFilterMsgHeight;
                    if (this.propSignet.Actions.Add)
                        iSize += this.dragNDropMargin;
                }

            }
            else if (this.DataJson.Data.length > 0 && this.DataJson.Data[0].LstDataFields.length > 0)
                iSize = 6.438 + (2.5 * this.DataJson.Data.length);
            else if (this.DataJson.Data.length > 0 && this.DataJson.Data[0].LstDataFields.length < 1)
                iSize = 5.75 + (2.5 * this.DataJson.Data.length);
            else if (this.DataJson.Data.length <= 0 && this.DataJson?.Structure?.ExpressFilterActivated)
                iSize += this.expressFilterMsgHeight - 1.875;   //dans un système qui fait des calculs avec des valeurs en dur et des éléments en position absolute j'ai pas de meilleure idée.
            return iSize + "rem";
        },

        /** Retourne la taille r�elle du tableau � afficher dans le signet. */
        getFlatTableToDisplay() {
            return this.tables.flatMap(n => n.Data);
        },
        getAlertStyle() {
            return !this.DataJson?.Structure?.ExpressFilterActivated ? 'padding : 0; height: auto; top: 60px;' : 'padding : 0 0 10px 0!important; height: auto; top: 60px;'
        }

    },
    mixins: [eListMixin],
    watch: {
        //load: function () {
        //    this.$emit('loading', this.load);
        //},
        "DataJson": function () {

            let tblAdresse = 400;
            let tblPM = 300;
            let descAdrh = tblAdresse + 92;
            let descPMName = tblPM + 1;


            if (this.DataJson != null) {
                this.$set(this.propSignet, 'ExpressFilterActived', this.DataJson?.Structure?.ExpressFilterActivated);
                this.$set(this.propSignet, 'ViewMainTab', this.DataJson?.Structure?.ViewMainTab);
            }

            this.$set(this.propSignet, 'nbRows', this.DataJson?.PagingInfo?.NbTotalRows ?? 0);

            /*ELAIZ - demande 80208 - mettre la soci�t� en lecture seule sur un signet adresse 
            lorsque la ligne est une adresse personnelle */
            if (this.propSignet.DescId == tblAdresse) {
                let homeAdress = [];

                //On v�rifie sur la ligne que l'adresse personnelle (492) est coch�e
                if (this.DataJson != undefined) {
                    homeAdress = [...this.DataJson.Data]
                        .flatMap((key) => key.LstDataFields)
                        .filter(df => df.DescId == descAdrh && df.Value == 1)
                        .map(df => df.FileId);

                    //Sur chaque champs relation Raison Social nous mettons le champs en lecture seule
                    homeAdress.forEach(key => {

                        let fldRela = [...this.DataJson.Data]
                            .filter(data => data.MainFileId == key);

                        if (!fldRela)
                            return false;

                        fldRela = fldRela.flatMap(data => data.LstDataFields)
                            .find(df => df.DescId == descPMName)

                        if (!fldRela)
                            return false;

                        [...this.DataJson.Data]
                            .filter(data => data.MainFileId == key)
                            .flatMap(data => data.LstDataFields)
                            .find(df => df.DescId == descPMName).ReadOnly = true;
                    });
                }
            }

        }
    },

    async created() {

        if (typeof EventBus._events['reloadSignet_' + this.propSignet.id] !== "undefined")
            EventBus.$off('reloadSignet_' + this.propSignet.id)

        EventBus.$on('reloadSignet_' + this.propSignet.id, (options, fromReload = false) => {
            if (options.id == this.propSignet.id) {
                this.load = true;
                options.nbLine = this.nbLineCall;
                options.pageNum = 1;
                this.tables = new Array();
                this.callSignet(options);

                if (this.propSignet.id.startsWith("pinned")) {
                    let arIdToReload = this.propSignet.id.split("_");
                    arIdToReload.shift();
                    options.id = arIdToReload.join("_");
                }
                else
                    options.id = "pinned_" + this.propSignet.id;

                if (!fromReload)
                    EventBus.$emit('reloadSignet_' + options.id, options, true);

            }
        });

        EventBus.$on('valueEdited', (options) => {
            let ppFstName = 202;
            let ppLstName = 201;

            if (!(this.getFlatTableToDisplay && this.getFlatTableToDisplay.length > 0))
                return false;

            //Maj de tous les champs affectés
            this.getFlatTableToDisplay
                .flatMap(a => a.LstDataFields)
                .filter(c => c.DescId == options.DescId && c.FileId == options.FileId)
                .forEach(findItemData => {

                    if (findItemData)
                        findItemData.Value = options.NewValue;

                    if (findItemData && (findItemData.DisplayValue != null || findItemData.DisplayValue))
                        findItemData.DisplayValue = options.NewDisp;

                    //gestion des propriétés étendues
                    handleExtendedProperties(findItemData, options)


                });

            let findRelation = this.getFlatTableToDisplay
                .flatMap(a => a.LstDataFields)
                .find(c => c.DescId == ppLstName)

            if (findRelation && options.DescId == ppFstName)
                findRelation.DisplayValue = findRelation.DisplayValue.split(' ')[0] + ' ' + options.NewValue;

        });
    },
    async mounted() {
        try {
            await this.callSignet();
        } catch (e) {

        } finally {

        }
    },
    methods: {
        LoadSignet,
        loadFileBookmark,
        /**
         * ouvre la fiche épinglée.
         * @param {any} finalCountDown
         */
        setPinnedBkm: function (finalCountDown, promFinalLayout, bkmViewMode, fileId, idxRow, aFileId) {
            this.$emit("setPinnedBkm", finalCountDown, promFinalLayout, bkmViewMode, fileId, idxRow, aFileId);
        },
        /**
         * Permet d'aller d'une page a une autre via
         * Si la page est deja charger alors on va sur la 1er ligne de celle ci
         **/
        getPage(NewPage) {
            let content = this.$refs.listContent;
            let page = this.$children
                .map(n => n.$refs["page_" + NewPage])
                .find(n => n);
            this.paginationOpen = false;

            let oPropPage = {};
            oPropPage[this.propSignet.id] = {
                pageNum: NewPage,
                nbLine: this.nbLineCall
            };
            this.setBkmPage(Object.assign(this.getBkmPage, oPropPage));

            if (page != null) {
                content.scrollTo({
                    top: page.offsetTop,
                    behavior: 'smooth'
                });
            } else {

                let objPage = {
                    pageNum: NewPage,
                    nbLine: this.nbLineCall
                }
                this.callSignet(objPage);
            }
        },

        /**
         * Quand on applique un filtre, on réinitialise l'affichage.
         * @param {any} options
         */
        async callSignetWithFilter(options) {
            this.tables = new Array();
            this.load = true;
            await this.callSignet(options);
        },

        /**
         * Premier appel lors de l'affichage du signet pour la liste
         * du contenu.
         **/
        async callSignet(newPageCall) {
            this.obser = false;

            newPageCall = this.initNewPageCall(newPageCall);

            this.truePage = parseInt(newPageCall.pageNum);
            const constPageInit = parseInt(newPageCall.pageNum);
            this.constPage = constPageInit;

            this.stopInfinity = false;
            this.load = this.noReloadForce;
            this.$emit('loading', this.load);
            this.$emit('bkmReload', { bkmLoaded: false });
            var that = this;
            var signet = this.$parent.$refs[this.propSignet.id];

            if (!signet || signet == null)
                return false;

            await this.bkmListLoader(newPageCall);

            if (!this.DataJson)
                return false;

            this.DataJson.Data.forEach(a => {
                a.LstDataFields.forEach((b, idx) => {
                    var findItemData = this.DataJson.Structure.LstStructFields.find(c => c.DescId == b.DescId);
                    findItemData["StructType"] = this.DataJson.Structure.StructType;
                    this.$set(a.LstDataFields, idx, { ...b, ...findItemData });
                });
            });

            if (this.DataJson.Data.length == this.nbLineCall + 1) {
                this.DataJson.Data = this.DataJson.Data.slice(0, -1);
            }

            this.pagination = (this.DataJson.PagingInfo.NbPages > 1);
            if (this.pagination) {
                this.$emit('pagination', this.pagination)
            }
            signet.style.height = this.setBkmSize;

            this.load = true;
            this.$emit('loading', this.load);
            this.noReloadForce = false;
            this.loadNewLine = false;

            const page = that.DataJson.PagingInfo.Page;
            that.pageInt = page;

            // #83 877 - Repositionnement du scroll à l'endroit précédent avant MAJ, si indiqué
            if (newPageCall?.scrollLeft) { this.$el.scrollLeft = newPageCall.scrollLeft; }
            if (newPageCall?.scrollLTop) { this.$el.scrollTop = newPageCall.scrollTop; }

            this.calHeight();

            // #US 3 108 retour 5 146  - Problème d'enregistrement du scroll
            this.$emit('bkmReload', {
                bkmLoaded: true,
                scrollLeft: newPageCall?.scrollLeft || 0
            });
        },
        calHeight() {
            let options = { timeoutCalculHeight: this.iSetTimeout };
            EventBus.$emit('timeOutRightNav', options);
        },

        /**
         * Charge la page suivante
         * @param {any} newPageCall
         * @param {any} split
         */
        async getMore(newPageCall, split) {

            if (this.DataJson.PagingInfo.Page == this.DataJson.PagingInfo.NbPages) {
                this.stopInfinity = true;
                return false;
            }

            this.truePage += 1;
            let that = this;
            this.loadNewLine = true;

            await this.bkmListLoader(this.initNewPageCall(newPageCall));

            this.DataJson.Data.forEach(a => {
                a.LstDataFields.forEach((b, idx) => {
                    var findItemData = this.DataJson.Structure.LstStructFields.find(c => c.DescId == b.DescId);
                    this.$set(a.LstDataFields, idx, { ...b, ...findItemData });
                });
            });

            this.load = true;
            this.$emit('loading', this.load);
            this.loadNewLine = false;
        },

        /**
         * Initialise l'objet � envoyer en param�tre de la requete pour 
         * charger les pages dans le signet.
         * @param {any} oNewPageCall
         */
        initNewPageCall(oNewPageCall) {

            if (!oNewPageCall) {
                oNewPageCall = {
                    pageNum: ((this.DataJson && this.DataJson.PagingInfo && this.DataJson.PagingInfo.Page + 1) || 1),
                    nbLine: this.nbLineCall
                };
            }

            return oNewPageCall;
        },

        /**
         * Chargement d'une page de la liste des signets.
         * @param {any} newPageCall
         */
        async bkmListLoader(newPageCall) {

            this.fid = this.getFileId;
            this.did = this.getTab;
            let signet = this.$parent.$refs[this.propSignet.id];
            let numPage = newPageCall.pageNum;
            let bForceReturnOrigin = newPageCall.forceReturnOrigin;

            if (!signet || signet == null)
                return false;

            if (this.getBkmPage[this.propSignet.id])
                newPageCall = this.getBkmPage[this.propSignet.id];

            if (bForceReturnOrigin)
                newPageCall.pageNum = 1;

            try {
                this.DataJson = await this.LoadSignet(this.propSignet.DescId, newPageCall.nbLine, newPageCall);
            } catch (e) {
                EventBus.$emit('globalModal', {
                    typeModal: "alert", color: "danger", type: "zoom",
                    close: true, maximize: false, id: 'alert-modal', title: this.getRes(7050), msgData: e, width: 600,
                    btns: [{ lib: this.getRes(30), color: 'default', type: 'left' }], datas: this.getRes(6432).replace('<BKMRES>', this.propSignet.Label)
                });

                this.load = true;
                this.$emit('loading', this.load);
                this.error = true;
            }

            if (this.DataJson == null) {
                signet.style.height = this.bkmHeight + "px";
                this.load = true;
                this.$emit('loading', this.load);
                return false;
            }

            this.$set(this.tables, 0, {
                Page: this.truePage,
                Data: this.DataJson.Data,
                dateRecord: new Date(),
            });
        },
        /** Envoie au parent la fiche épinglée */
        pinFile(oPinnedFile) {
            this.$emit('pinFile', oPinnedFile);
        }
    },
    template: `
	<div @dragover="$emit('listDragDover')" :class="pagination ? 'paginationTrue' : ''" id="listContent">
		<div :id="'refreshirisbkm_'+ propSignet.DescId" style="display:none" @click="callSignet"></div>
		<div :style="{'height':myHeight}" v-cloak>
			<eEmptyErrorList v-show="displayAlert" :style="getAlertStyle" :data-json="DataJson" :prop-signet="propSignet" :error="error" />
			<template v-if="getHasDataToShow">
				<table ref="listContent" id="exemple-infinite-scroll" class="table-bordered table-hover dataTable no-footer" role="grid">
					<eHeadList 
							:sortActivated.sync="sortActivated"
							:data-json="DataJson"
							:prop-signet="propSignet"
							:nb-line-call="nbLineCall"
							@callSignetWithFilter="callSignetWithFilter" :new-line="stopInfinityTop && loadNewLine" />
					<template v-for="(tbl, idx) in tables">
						<eBodyList 
                            :forceRefreshTabsBar="forceRefreshTabsBar"
						    :tables="tbl" 
							:prop-signet="propSignet" 
							:load-new-line="loadNewLine" 
							:paging-info="DataJson.PagingInfo"
							:nb-line-call="nbLineCall"
							@getMore="getMore" 
							@callSignet="callSignet" 
							@getPage="getPage"
                            @setPinnedBkm="setPinnedBkm"/>
					</template>
					<eFooterList @getMore="getMore" 
                        :pagination="pagination"
						:data-lengthlt-max-row="getDataLengthltMaxRow" 
						:list-type="propSignet.TableType"
						:end-page-or-inf="getEndPageOrInf" 
						:load-new-line="loadNewLine"
						:display-btn="false">
						<template v-if="isNbRowsExceedPage" #sltPaging>
							<ePagingList  @callSignet="callSignet" @getPage="getPage" :pagination-open="paginationOpen" :paging-info="DataJson.PagingInfo" />
						</template>
					</eFooterList>
				</table>
			</template>
		</div>
	</div>
`
}