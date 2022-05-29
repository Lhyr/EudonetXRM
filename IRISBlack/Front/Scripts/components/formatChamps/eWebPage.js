import { showInformationIco } from '../../methods/eComponentsMethods.js?ver=803000';
import { eFileComponentsMixin } from '../../mixins/eFileComponentsMixin.js?ver=803000';

export default {
    name: "eWebPage",
    data() {
        return {
            resError: this.getRes(2436).replace("<FieldType>", this.getRes(1543)),
            modif: false
        };
    },
    mounted() {
        this.setContextMenu();
    },
    methods: {
        showInformationIco
    },
    props: ["dataInput", "propHead","propListe"],
    mixins: [eFileComponentsMixin],
    template: `
    <div>

    <!-- <span ref="info" v-if="showInformationIco()" class="icon-info-circle info-tooltip"></span> -->

    <!-- Champ -->
    <div v-bind:class="['ellips input-group hover-input web-page-input']"  :title="resError">
        <!-- Si le champ webPage n'est pas modifiable -->
        <span v-bind:style="{ color: '#bb1515'}" class="linkHead readOnly notsupported">{{resError}}</span>
        <!-- Icon -->
        <span  v-if="(!propListe && !(dataInput.ReadOnly)) || !propListe" class="input-group-addon"><a class="hover-pen"><i class="fas"></i></a></span>
    </div>
</div>
`
};