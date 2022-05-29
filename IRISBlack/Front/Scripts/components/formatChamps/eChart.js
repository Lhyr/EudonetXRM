import { focusInput, RemoveBorderSuccessError, verifComponent, showInformationIco, displayInformationIco } from '../../methods/eComponentsMethods.js?ver=803000';
import { PropType } from '../../methods/Enum.min.js?ver=803000';
import { eFileComponentsMixin } from '../../mixins/eFileComponentsMixin.js?ver=803000';


export default {
    name: "eChart",
    data() {
        return {
            resError: this.getRes(2436).replace("<FieldType>", this.getRes(1005)),
            modif: false,
        };
    },
    mounted() {
        this.setContextMenu();
        this.displayInformationIco();
    },
    methods: {
        verifComponent,
        showInformationIco,
        displayInformationIco,
    },
    props: ["dataInput"],
    mixins: [eFileComponentsMixin],
    template: `
    <div class="input-group d-flex hover-input">
        <span class="readOnly notsupported">{{resError}}</span>
    </div>
`
};