import { verifComponent, showInformationIco, displayInformationIco } from '../../methods/eComponentsMethods.js?ver=803000';
import EventBus from '../../bus/event-bus.js?ver=803000';
import { eFileComponentsMixin } from '../../mixins/eFileComponentsMixin.js?ver=803000';

export default {
    name: "eLogic",
    data() {
        return {
            ComponentId: "",
            modif: false,
            that : this,
            bValue:false
        }
    },
    created(){
        this.bValue = Boolean(parseInt(this.dataInput.Value));
    },
    mounted() {
        this.setContextMenu();
        this.displayInformationIco();
        this.ComponentId = this.GetComponentId()
    },
    computed: {
        IsDisplayReadOnly: function () {
            return (this.dataInput.ReadOnly);
        },
        /** Récupère ou remplace la valeur si on coche le composant et met à jour le back */
        getBoolValue:{
            get:function(){
                return this.bValue;
            },
            set:function(val){
                this.bValue = val;
                let ctx = this;
                verifComponent(undefined, undefined, !val, ctx, this.dataInput,undefined,'eudofront');
            }
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
            let value = this.dataInput.Value ? "1" : "0"
            verifComponent(undefined, event, this.dataInput.Value, that, this.dataInput,undefined,'eudofront');
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
                ComponentId = 'id_liste_rubrique_checkbox' + this.propSignet.DescId + '_' + this.propIndexRow + '_' + this.dataInput.DescId
            }
            return ComponentId
        }
    },
    mixins: [eFileComponentsMixin],
    props: ["dataInput", "propHead", "propListe", "propSignet", "propIndexRow", "propAssistant", "propDetail", "propAssistantNbIndex"],
    template: `
<div class="logicCompo">
    <div
        :class="[
            propListe ? 'listRubriqueLogic' : '', 
            'ellips input-group hover-input'
        ]"
        :title="!dataInput.ToolTipText ? dataInput.DisplayValue : dataInput.ToolTipText"
    >
        <edn-logic 
            :logicType="dataInput.DisplayType" 
            :label="!propListe ? dataInput.Label : ''" 
            :disabled="IsDisplayReadOnly" 
            :readonly="IsDisplayReadOnly" 
            :required="dataInput.Required" 
            v-model="getBoolValue"
            :id="ComponentId"
            :placeholder="dataInput.Watermark"
        />
    </div>
</div>
`
};