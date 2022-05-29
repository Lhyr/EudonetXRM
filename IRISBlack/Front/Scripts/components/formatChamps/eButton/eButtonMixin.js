import { eFileComponentsMixin } from '../../../mixins/eFileComponentsMixin.js?ver=803000'
import { verifComponent, showInformationIco, displayInformationIco } from '../../eComponentsMethods.js?ver=803000';
import EventBus from '../../../bus/event-bus.js?ver=803000';

/**
 * Mixin commune aux composants eButton.
 * */
export const eButtonMixin = {
    mixins: [eFileComponentsMixin],
    data() {
        return {
            modif: false,
            that: this
        };
    },
    computed: {},
    mounted() {

        this.displayInformationIco();
    },
    methods: {
        showInformationIco,
        displayInformationIco,
        onUpdateCallback() {
            let options = {
                reloadSignet: false,
                reloadHead: false,
                reloadAssistant: false,
                reloadAll: true
            }
            if (this.dataInput.IsInRules) {
                await Promise.all([

                ]);

                EventBus.$emit('emitLoadAll', options);
            }
        },
        verifButton(event, that) {
            event.target.value = (this.dataInput.Value == "0") ? "1" : "0";
            verifComponent(undefined, event, this.dataInput.Value, that, this.dataInput);
        },
    },
    props: ["dataInput", "propHead", "propListe", "propSignet", "propIndexRow", "propAssistant", "propDetail", "propAssistantNbIndex"],
}