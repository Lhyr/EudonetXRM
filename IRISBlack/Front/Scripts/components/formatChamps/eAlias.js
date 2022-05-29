import { showInformationIco } from '../../methods/eComponentsMethods.js?ver=803000';
import { eFileComponentsMixin } from '../../mixins/eFileComponentsMixin.js?ver=803000';

export default {
    name: "eAlias",
    data() {
        return {
            modif: false,
        };
    },
    mounted() {
        this.setContextMenu();
    },
    methods: {
        showInformationIco,
          goAction(event) {
            var objParentInfo = { parentTab: this.getTab, parentFileId: this.getFileId }
            selectFileMail(getParamWindow().document.getElementById("MLFiles"), this.dataInput.Value, objParentInfo, TypeMailing.MAILING_UNDEFINED);
        },
    },
    props: ["dataInput"],
    mixins: [eFileComponentsMixin],
    template: `
    <div class="input-group hover-input">
        <input
            v-if="!dataInput.ReadOnly"
            :value="dataInput.Value"
            type="text"
            class="form-control input-line fname bold"
            :placeholder="dataInput.Watermark"
        >
        <span class="readOnly" v-else>{{dataInput.Value}}</span>
        <span
            v-if="!dataInput.ReadOnly"
            @click="goAction($event)"
            class="input-group-addon"
        >
            <a href="#" class="hover-pen"><i class="fa fa-pencil"></i></a>
        </span>
    </div>
`
};