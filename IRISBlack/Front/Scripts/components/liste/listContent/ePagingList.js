import { eListComponentsMixin } from '../../../mixins/eListComponentsMixin.js?ver=803000';

export default {
    name: "ePagingList",
    data() {
        return {
            pagingOpen: Boolean,
            heightNumberPage : 21,
            maxHeightNumberPage: 106,
            defaultHeightNumberPage: 150,
        };
    },
    props: {
        pagingInfo: Object,
        paginationOpen: { type: Boolean, default:false},
    },
    mixins: [eListComponentsMixin],
    computed: {
    },
    methods: {

        /**
         * Une fois les 60 lignes afficher avec le scroll infie, cette fonction permet d'aller sur la page suivante
         **/
        getNextPage() {
            this.pagingOpen = false;
            let objPage = {
                pageNum: (this.pagingInfo.RowsPerPage / this.nbLineCall) + this.pagingInfo.Page,
                nbLine: this.nbLineCall
            }

            this.$emit(callSignet, objPage);
        },
        /***
         * ouvre la liste des pages
         * */
        getPaging() {
            this.pagingOpen = true;
            document.addEventListener('click', this.closePagination);
        },

        /**
         * ferme la liste des pages
         * @param {any} e
         */
        closePagination(e) {
            let el = this.$refs.paginationContent;
            if (el && !el.contains(e.target)) {
                this.pagingOpen = false;
                document.removeEventListener('click', this.closePagination);
            }
        },

        /** 
        * partie droite de la pagination. 
        * @param {bool} last dernière page
        */
        getEndPaging(last) {
            if (last) {
                this.$emit('getPage', this.pagingInfo.NbPages);
            } else if(!last && this.pagingInfo.Page < this.pagingInfo.NbPages) {
                this.$emit('getPage', this.pagingInfo.Page + 1);
            }
        },

        /** 
         * partie gauche de la pagination. 
         * @param {bool} first première page
         * */
        getBeginPaging(first) {
            if (first) {
                this.$emit('getPage', 1);
            } else if(!first && this.pagingInfo.Page > 1) {
                this.$emit('getPage', this.pagingInfo.Page - 1);
            }
        },
    },
    computed: {

        /**
         * Si on est à la première, on désactive le bouton.
         * */
        getClassDisablingFirstPage() {
            return { disabledPagging: this.pagingInfo.Page == 1 };
        },

        /**
         * Si on est à la dernière page, on désactive le bouton.
         * */
        getClassDisablingLastPage() {
            return { disabledPagging: this.pagingInfo.Page >= this.pagingInfo.NbPages };
        },

        /** Style pour l'affichage de la liste des pages. */
        getStylePaging() {
            return (this.pagingInfo.NbPages * this.heightNumberPage) + 2 < this.maxHeightNumberPage
                ? { top: (- this.pagingInfo.NbPages * this.heightNumberPage + -44) + 'px' }
                : { top: -this.defaultHeightNumberPage + 'px' };

        }
    },
    mounted() {
        this.pagingOpen = this.paginationOpen;
    },
    beforeDestroy() {
        document.removeEventListener('click', this.closePagination);
    },
    template: `
    <div ref="pagination" id="pagination">
        <a @click="getBeginPaging(true)" :class="getClassDisablingFirstPage" class="firstPage" href="#!" >1</a>
        <span :class="getClassDisablingFirstPage" class="firstPointPagination pointPagination" >...</span>
        <span @click="getBeginPaging(false)" :class="getClassDisablingFirstPage" class="arrowPage">
            <a class="firstArrowPage" href="#!">
                <i class="fas fa-chevron-left"></i>
            </a>
        </span>
        <span ref="paginationContent" class="noClick btn-pagination-content">
            <button @click="getPaging()" class="noClick btn-pagination">
                <span class="noClick">{{getRes(341) + " " + pagingInfo.Page}}</span>
            </button>
            <div :style="getStylePaging" v-if="pagingOpen" class="noClick layout-page">
                <input @keyup.enter="$emit('getPage', $event.target.value),pagingOpen = false" 
                    :placeholder="pagingInfo.NbPages + '...' " 
                    class="paginationInput" type="number" min="1" :max="pagingInfo.NbPages">
                <div :style="{height: (pagingInfo.NbPages * this.heightNumberPage) + 2 + 'px'}" class="noClick content-page">
                    <ul class="list-content-pagination-page">
                        <li @click="$emit('getPage', idx+1), pagingOpen = false" v-for="(num, idx) in pagingInfo.NbPages" class="page-pagination">{{idx+1}}</li>
                    </ul>
                </div>
                <span class="popover-arrow"></span>
            </div>
        </span>
        <span @click="getEndPaging(false)" :class="getClassDisablingLastPage" class="arrowPage">
            <a class="lastArrowPage" href="#!">
                <i class="fas fa-chevron-right"></i>
            </a>
        </span>
        <span :class="getClassDisablingLastPage" class="lastPointPagination pointPagination">...</span>
        <a @click="getEndPaging(true)" :class="getClassDisablingLastPage" class="lastPage" href="#!" >
            {{pagingInfo.NbPages}}
        </a>
    </div>
`
};