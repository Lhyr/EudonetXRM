import { eListComponentsMixin } from '../../../mixins/eListComponentsMixin.js?ver=803000';

export default {
    name: "eFooterList",
    data() {
        return {
        };
    },
    mixins: [eListComponentsMixin],
    components: {
        eLoaderTable: () => import(AddUrlTimeStampJS("./eLoaderTable.js")),
    },
    computed: {
        attachmentList() {
            return (this.listType == 11) ? 'attachment-list-footer' : '';
        }
    },
    methods: {

    },
    props: {
        endPageOrInf: Boolean,
        loadNewLine: Boolean,
        dataLengthltMaxRow: Boolean,
        displayBtn: Boolean,
        listType: Number,
        pagination:Boolean
    },
    template: `
    <tfoot id="tfoot-infinite-scroll" :class="['clsBkmFooterPosition',attachmentList]">
        <tr v-show="pagination">
            <td class="button-container">
                <slot name="sltPaging"></slot>
            </td>
        </tr>
    </tfoot>
`
};