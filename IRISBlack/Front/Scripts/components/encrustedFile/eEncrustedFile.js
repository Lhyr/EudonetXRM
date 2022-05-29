import { eFileMixin } from '../../mixins/eFileMixin.js?ver=803000';

export default {
    name: "eEncrustedFile",
    data() {
        return {
            DataJson: new Object(),
        };
    },
    components: {
        eFile: () => import(AddUrlTimeStampJS("../fiche/fileDetail.js")),
        eFileLoader: () => import(AddUrlTimeStampJS("../fiche/FileLoader.js")),
    },
    computed: {
        myHeight: function () {
            let height = (this.scroll) ? "470px" : "auto";

            return height;
        },
    },
    created() {
        this.DataJson = this.propDataDetail;
    },
    methods: {

    },
    props: {
        propDataDetail: Object,
        propCols: Number,
    },
    mixins: [eFileMixin],
    template: `
    <div style="overflow:auto;">
        <!--<eFileLoader class="ListLoader" css-style="{ position: 'absolute', top: '45%', left: '45%', z-index: '100'}" v-show="!this.load" />-->
        <div :style="{'height':myHeight}" v-cloak>
            <eFile :prop-data-detail="propDataDetail" :prop-cols="propCols"></eFile>
        </div>
    </div>
`
};