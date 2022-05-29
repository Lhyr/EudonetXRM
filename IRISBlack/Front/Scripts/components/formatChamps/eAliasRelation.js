import { showInformationIco } from '../../methods/eComponentsMethods.js?ver=803000';
import { eFileComponentsMixin } from '../../mixins/eFileComponentsMixin.js?ver=803000';

export default {
    name: "eCharacter",
    data() {
        return {
            modif: false,
        };
    },
    mounted() {
        this.setContextMenu();
    },
    methods: {
        showInformationIco
    },
    props: ["dataInput"],
    mixins: [eFileComponentsMixin],
    template: `
    <div class="input-group hover-input">
        <!-- <span ref="info" v-if="showInformationIco()" class="icon-info-circle info-tooltip"></span> -->
        <input v-if="!dataInput.ReadOnly" :value="dataInput.Value" type="text" class="form-control input-line fname bold" :placeholder="dataInput.Watermark">
        <span class="readOnly" v-else>{{dataInput.Value}}</span>
        <span v-if="!dataInput.ReadOnly" class="input-group-addon"><a href="#" class="hover-pen"><i class="fa fa-pencil"></i></a></span>
    </div>
`
};