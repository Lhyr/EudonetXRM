export default {
    name: "summarySkeleton",
    data() {
        return {
            //largeur des colonnes pour les champs du skeleton (max 6)
            headerInputCol: {
                lg: [3, 5, 3, 3, 2, 6],
                md: [3, 5, 4, 3, 2, 6],
                sm: [4, 6, 4, 4, 4, 8]
            },
            nFields:4
        };
    },
    computed: {
        /**
         * Permet d'ajouter une largeur différente à chaque champ du skeleton de la zone résumé
         * */
        getHeaderInputCol: function () {
            let arrCol = []
            if (this.summaryProps?.inputs?.length > 1)
                this.summaryProps?.inputs?.forEach((col, id) => {
                    arrCol.push(' col-lg-' + this.headerInputCol.lg[id] + ' col-md-' + this.headerInputCol.md[id] + ' col-sm-' + this.headerInputCol.sm[id]);
                })
            else
                arrCol.push('col-12');

            return arrCol;
        },
    },
    mounted() { },
    methods: {},
    props: ["summaryProps"],
    template: `
    <div class="summary-skeletons-wrapper">
        <div class="row head-summary-skeletons-wrapper">
            <div class="d-flex flex-column p-2 col col-2 pb-0 titles-skeleton-wrapper">
                <div class="skeleton mb-2 skeleton-title"></div>
                <div class="skeleton skeleton-subtitle"></div>
            </div>
            <div class="col col-10 d-flex justify-content-end p-2 params-skeleton-wrapper">
                <div class="skeleton mb-2 skeleton-params mr-5"></div>
                <div class="skeleton skeleton-params"></div>
            </div>
        </div>
        <div class="row p-4 content-skeleton-wrapper">
            <div class="col col-lg-1 col-md-2 col-sm-2 avatar-field-wrapper">
                <div class="skeleton avatar-skeleton"></div>
            </div>
            <div class="col row col-lg-11 col-md-10 col-sm-10 skeleton-field-wrapper">
            <div :class="['col',getHeaderInputCol[id]]" v-for="(input,id) in nFields">
                <div :class="['skeleton','skeleton-' + (id + 1)]"></div>
            </div>
            </div>
        </div>
    </div>
    `
};
