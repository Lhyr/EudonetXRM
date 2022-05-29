import { eMailAddressMixin } from './eMailAddressMixin.js?ver=803000'

export default {
    name: "eMailAddressList",
    data() {
        return {};
    },
    mounted() { },
    methods: {},
    props: [],
    mixins: [eMailAddressMixin],
    template: `
   <div v-if="propListe" v-bind:class="[propListe ? 'listRubriqueRelation' : '', 'ellips input-group hover-input']" 
        :title="!dataInput.ToolTipText ? dataInput.Value : dataInput.ToolTipText">

        <!-- Si le champ mailAdress n'est pas dans le head et est modifiable -->
        <input v-if="!IsDisplayReadOnly && this.modif" :IsDetail="propDetail" :IsAssistant="propAssistant" :IsHead="propHead" :IsList="propListe"  :class="'class_liste_rubrique_mailAdress_' + propSignet.DescId + '_' + propIndexRow + '_' + dataInput.DescId" v-bind:disabled="dataInput.ReadOnly" v-on:blur="verifEmail($event,that);" v-bind:style="{ color: dataInput.ValueColor}"  :value="dataInput.Value"
                type="email" :title="bRegExeSuccess ? dataInput.Value : messageError"
                class="ellipsDiv form-control input-line fname"
                :placeholder="dataInput.Watermark">
        
        <div v-on:click="goAction($event)" v-if="!IsDisplayReadOnly && !this.modif" class="divInput ellipsDiv form-control input-line fname">
            <a href="#!" class="targetIsTrue" v-bind:style="{ color: dataInput.ValueColor}">{{dataInput.Value}}</a>
        </div>

        <!-- Si le champ mailAdress n'est pas modifiable ou dans le head -->
        <a href="#!" v-if="IsDisplayReadOnly" v-bind:style="{ color: dataInput.ValueColor}" v-on:click="goAction($event)" class="targetIsTrue linkHead readOnly">{{dataInput.Value}}</a>
         
        <!-- Icon -->
        <span v-on:click="goAction($event)" v-if="!IsDisplayReadOnly" class="input-group-addon"><a href="#!" class="hover-pen"><i class="fas fa-pencil-alt"></i></a></span>
       
    </div>
`
};
