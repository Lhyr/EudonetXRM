import { showInformationIco } from '../../methods/eComponentsMethods.js?ver=803000';

export default {
    name: "eUndefined",
    data() {
        return {
            modif: false
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
        <input v-if="!dataInput.ReadOnly" :value="dataInput.Value" type="text" class="form-control input-line fname bold">
        <span class="readOnly" v-else>{{dataInput.Value}}</span>
        <span v-if="!dataInput.ReadOnly" class="input-group-addon"><a href="#" class="hover-pen"><i class="fa fa-pencil"></i></a></span>
    </div>
`
};