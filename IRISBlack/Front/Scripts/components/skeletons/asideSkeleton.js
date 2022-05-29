export default {
    name: "asideSkeleton",
    data() {
        return {
            nTab: 2
        };
    },
    computed: {},
    mounted() { },
    methods: {},
    props: ["asideProps"],
    template: `
    <div class="aside-skeletons-wrapper d-flex flex-column">
        <div class="aside-skeletons-header-wrapper d-flex">
            <div v-for="tab in nTab" class="skeleton aside-skeletons-header"></div>
            <div class="skeleton skeleton-btn"></div>
        </div>
        <div class="skeleton note-skeleton"></div>
    </div>
    `
};
