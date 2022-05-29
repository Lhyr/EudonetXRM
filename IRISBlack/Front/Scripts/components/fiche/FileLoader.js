

export default {
    name: "eFileLoader",
    data() {
        return {
            iNbDiv: 8,
        }
    },
    props: {
        cssClass: String,
        cssStyle: Object,
        cssSubClass: String,
        cssSubStyle: Object,
        nbInternalDiv: {
            default: 8
        },
    },
    watch: {},
    methods: {},
    created() {
        this.iNbDiv = this.nbInternalDiv;
    },
    template: `
        <div :style="cssStyle" :class="cssClass" v-cloak>
            <div :style="cssSubStyle" :class="cssSubClass || 'lds-roller'">
                <div v-for="i in iNbDiv"></div>
            </div>
        </div>
`
};