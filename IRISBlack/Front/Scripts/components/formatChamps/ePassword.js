import { updateMethod, focusInput, verifComponent, showInformationIco} from '../../methods/eComponentsMethods.js?ver=803000';
import { PropType } from '../../methods/Enum.min.js?ver=803000';
import { eFileComponentsMixin } from '../../mixins/eFileComponentsMixin.js?ver=803000';

export default {
    name: "ePassword",
    data() {
        return {
            modif: false,
            IsDisplayReadOnly: true,
        };
    },
    mounted() {
        this.setContextMenu();
    },
    methods: {
        showInformationIco
    },
    props: ["dataInput", "propHead", "propListe", "propSignet", "propIndexRow", "propAssistant", "propDetail", "propAssistantNbIndex"],
    mixins: [eFileComponentsMixin],
    template: `
<div class="passwordInput">

    <!-- FICHE -->
    <div v-if="!propListe" v-bind:class="[IsDisplayReadOnly? 'headReadOnly read-only' : '','ellips input-group hover-input']" 
        :title="!dataInput.ToolTipText ? dataInput.DisplayValue : dataInput.ToolTipText">

        <!-- <span ref="info" v-if="showInformationIco()" class="icon-info-circle info-tooltip"></span> -->

        <!-- Si le champ ePassowrd n'est pas modifiable -->
        <span v-bind:style="{ color: dataInput.ValueColor}" class="readOnly">&bull;&bull;&bull;&bull;&bull;&bull;&bull;</span>

        <!-- Icon -->
		<span class="input-group-addon">
            <a  href="#!" class="hover-pen">
                <i :class="[IsDisplayReadOnly?'mdi mdi-lock':'fas fa-pencil-alt']"></i>
            </a>
        </span>
    </div>

</div>
`
};