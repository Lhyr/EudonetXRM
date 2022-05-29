import { updateMethod, focusInput, verifComponent, showInformationIco} from '../../methods/eComponentsMethods.js?ver=803000';
import { PropType } from '../../methods/Enum.min.js?ver=803000';
import { eFileComponentsMixin } from '../../mixins/eFileComponentsMixin.js?ver=803000';

export default {
    name: "eLabel",
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
    props: ["dataInput", "propHead", "propListe", "propSignet", "propIndexRow", "propAssistant", "propDetail", "propAssistantNbIndex"],
    mixins: [eFileComponentsMixin],
    template: `
    <div>
    <!-- Champ -->
    <div :style="{borderBottomColor:getCustomForeColor}" class="ellips input-group hover-input label-field" :title="getTitle">
        <span :style="{color:getCustomForeColor}" :class="getCssClass">{{dataInput.Label}}</span>
    </div>
</div>
`
};