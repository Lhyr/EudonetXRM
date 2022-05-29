export default {
    name: "bkmSkeleton",
    data() {
        return {
            nBtn: 3,
            nCol: 5,
            nRow: 2
        };
    },
    computed: {},
    mounted() { },
    methods: {},
    props: ["bkmProps"],
    template: `
    <div class="bkm-skeleton-wrapper flex-column">
        <div class="bkm-skeleton-table">
            <div class="row bkm-skeleton-header-wrapper">
                <div class="col-8">
                    <div class="skeleton bkm-skeletons-header"></div>
                </div>
                <div class="row col-4 justify-end">
                    <div  v-for="btn in nBtn" class=" col-3 skeleton bkm-skeletons-btn"></div>
                </div>
            </div>
	        <div class="row bkm-skeleton-theader">
		        <div v-for="col in nCol" class="col col-2 bkm-skeleton-theader-col">
			        <div class="skeleton bkm-skeletons-table-header"></div>
		        </div>
	        </div>
	        <div class="row bkm-skeleton-row" v-for="row in nRow">
		        <div v-for="col in nCol" class="col col-2">
			        <div class="skeleton"></div>
		        </div>
	        </div>
        </div>
	    <div v-if="bkmProps.pagination" class="row bkm-skeleton-footer flex-1 justify-center">
		    <div  class="col col-3  d-flex flex-column justify-end">
			    <div class="skeleton bkm-skeletons-table-footer"></div>
		    </div>
	    </div>
    </div>
    `
};
