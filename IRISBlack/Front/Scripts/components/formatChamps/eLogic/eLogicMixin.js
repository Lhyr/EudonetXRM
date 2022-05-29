import { eFileComponentsMixin } from '../../../mixins/eFileComponentsMixin.js?ver=803000'
import {  verifComponent, showInformationIco, displayInformationIco } from '../../eComponentsMethods.js?ver=803000';

/**
 * Mixin commune aux composants eLogic.
 * */
export const eLogicMixin = {
    mixins: [eFileComponentsMixin],
    data() {
        return {
            ComponentId: "",
            modif: false,
            that: this
        };
    },
    mounted() {

        this.displayInformationIco();
        this.ComponentId = this.GetComponentId()
    },
    computed: {
        IsDisplayReadOnly: function () {
            return (this.propHead || this.dataInput.ReadOnly)
        }
    },
    methods: {
        displayInformationIco,
        showInformationIco,
        onUpdateCallback() {
            let options = {
                reloadSignet: false,
                reloadHead: false,
                reloadAssistant: false,
                reloadAll: true
            }
            if (this.dataInput.IsInRules)
                EventBus.$emit('emitLoadAll', options);
        },
        verifLogic(event, that) {
            let newVal;
            if (this.dataInput.Value == "0") {
                newVal = "1"
            } else {
                newVal = "0"
            }
            event.target.value = newVal
            verifComponent(undefined, event, this.dataInput.Value, that, this.dataInput);
        },
        GetComponentId: function () {
            let ComponentId = this.dataInput.DescId
            if (this.propHead) {
                ComponentId += "_Head"
            } else if (this.propAssistant) {
                ComponentId += "_Assistant"
            }
            else if (this.propDetail) {
                ComponentId += "_Detail"
            }
            else if (this.propListe) {
                ComponentId += "_List_" + this.propIndexRow
            }
            return ComponentId
        },

    },
    props: ["dataInput", "propHead", "propListe", "propSignet", "propIndexRow", "propAssistant", "propDetail", "propAssistantNbIndex"],
}