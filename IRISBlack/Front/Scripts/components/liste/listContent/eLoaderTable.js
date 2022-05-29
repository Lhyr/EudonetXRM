import { eListComponentsMixin } from '../../../mixins/eListComponentsMixin.js?ver=803000';

export default {
    name: "eLoaderTable",
    data() {
        return {
        };
    },
    mixins: [eListComponentsMixin],
    components: {
        eFileLoader: () => import(AddUrlTimeStampJS("../../fiche/FileLoader.js")),
    },
    computed: {
    },
    methods: {

    },
    props: {
        tables: Object,
        newLine: Boolean,
        cssClass: String,
        cssStyle: String,
    },
    template: `
        <tr :class="cssClass" :style="cssStyle">
            <td class="button-container">
                <eFileLoader css-class="clsTblTDLnLoader"/>
            </td>
        </tr>
`
};