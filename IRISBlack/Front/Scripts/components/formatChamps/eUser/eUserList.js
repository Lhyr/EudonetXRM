import { eUserMixin } from './eUserMixin.js?ver=803000'

export default {
    name: "eUserList",
    data() {
        return {};
    },
    components: {
        eUserListMultipleEditable: () => import(AddUrlTimeStampJS("./eUserListMultipleEditable.js")),
        eUserListMultipleNotEditable: () => import(AddUrlTimeStampJS("./eUserListMultipleNotEditable.js")),
        eUserListSimpleEditable: () => import(AddUrlTimeStampJS("./eUserListSimpleEditable.js")),
        eUserListSimpleNotEditable: () => import(AddUrlTimeStampJS("./eUserListSimpleNotEditable.js")),
    },
    mounted() { },
    methods: {},
    props: [],
    mixins: [eUserMixin],
    template: `
<div v-if="propListe" v-bind:class="[propListe ? 'listRubriqueRelation' : '', 'ellips input-group hover-input']" 
        :title="!dataInput.ToolTipText ? dataInput.DisplayValue : dataInput.ToolTipText">

        <!-- Si le champ multiple et  modifiable -->
        <eUserListMultipleEditable />

        <!-- Si le champ multiple et pas modifiable-->
        <eUserListMultipleNotEditable />

        <!-- Si le champ simple et modifiable -->
        <eUserListSimpleEditable />

        <!-- Si le champ simple et pas modifiable -->
        <eUserListSimpleNotEditable />

        <!-- Icon -->
        <span v-on:click="openDial" v-if="!dataInput.ReadOnly" class="input-group-addon"><a  href="#!" class="hover-pen"><i class="fas fa-pencil-alt"></i></a></span>
    </div>
`
};
