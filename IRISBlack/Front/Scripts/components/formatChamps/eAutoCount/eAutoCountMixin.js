import { eFileComponentsMixin } from '../../../mixins/eFileComponentsMixin.js?ver=803000'
import { showInformationIco } from '../../eComponentsMethods.js?ver=803000';

/**
 * Mixin commune aux composants eAutoCount.
 * */
export const eAutoCountMixin = {
    mixins: [eFileComponentsMixin],
    data() {
        return {
            modif: false,
            inputHovered: false,
            //IsDisplayReadOnly: (this.propHead || this.dataInput.ReadOnly)
        };
    },
    computed: {
        IsDisplayReadOnly: function () {
            return (this.propHead || this.dataInput.ReadOnly)
        }
    },
    mounted() { },
    methods: {
        showInformationIco
    },
    props: ["dataInput", "propHead", "propListe", "propSignet", "propIndexRow", "propAssistant", "propDetail", "propAssistantNbIndex"],
}