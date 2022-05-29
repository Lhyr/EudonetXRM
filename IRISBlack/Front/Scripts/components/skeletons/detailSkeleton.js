export default {
    name: "detailSkeleton",
    data() {
        return {
            nInput: 20,
            nTab: 3
        };
    },
    computed: {},
    mounted() { },
    methods: {},
    props: ["detailProps"],
    template: `
    <div class="detail-skeletons-wrapper">
        <div class="row detail-header-skeletons-wrapper detail-header-skeletons-tab justify-start">
            <div v-for="tab in nTab" class="skeleton detail-skeletons-header"></div>
        </div>
        <div class="row detail-content-skeletons-wrapper">
            <div v-for="input in nInput" class="col-6 detail-input-skeletons skeleton"></div>
        </div>
    </div>
    `
};