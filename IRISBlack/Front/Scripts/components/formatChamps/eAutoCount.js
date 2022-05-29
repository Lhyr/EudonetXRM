import { showInformationIco } from '../../methods/eComponentsMethods.js?ver=803000';
import { eFileComponentsMixin } from '../../mixins/eFileComponentsMixin.js?ver=803000';

export default {
    name: "eAutoCount",
    data() {
        return {
            modif: false,
            inputHovered: false,
            //IsDisplayReadOnly: (this.propHead || this.dataInput.ReadOnly)
        };
    },
    components: {
        eAutoCountFile: () => import(AddUrlTimeStampJS("./eAutoCount/eAutoCountFile.js")),
        eAutoCountList: () => import(AddUrlTimeStampJS("./eAutoCount/eAutoCountList.js")),
    },
    computed: {
        IsDisplayReadOnly: function () {
            return (this.dataInput.ReadOnly)
        }
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
    <template v-if="false">
    <eAutoCountFile />
    </template>
    <template v-else>
    <!-- FICHE -->
    <div v-on:mouseover="inputHovered = true" v-on:blur="inputHovered = false" v-if="!propListe" v-bind:class="['ellips input-group hover-input', IsDisplayReadOnly? 'headReadOnly read-only' : '']"
        :title="!dataInput.ToolTipText ? dataInput.DisplayValue : dataInput.ToolTipText">
        <!-- <span ref="info" v-if="showInformationIco()" class="icon-info-circle info-tooltip"></span> -->

        <!-- Si le champ autCount n'est pas modifiable -->
        <span v-bind:style="{ color: dataInput.ValueColor}" class="readOnly" >{{dataInput.Value}}</span>

        <!-- Icon -->
        <span :class="[inputHovered ? 'editing-mode':'','input-group-addon']"><a  href="#!" class="hover-pen"><i :class="[IsDisplayReadOnly?'mdi mdi-lock':'fas fa-pencil-alt']"></i></a></span>

    </div>
    </template>
    <template v-if="false">
    <eAutoCountList />
    </template>
    <template v-else>
    <!-- LISTE -->
    <div v-if="propListe" v-bind:class="[propListe ? 'listRubriqueCaractere' : '', 'ellips input-group hover-input']"
        :title="!dataInput.ToolTipText ? dataInput.DisplayValue : dataInput.ToolTipText">

        <!-- Si le champ autCount n'est modifiable -->        
        <div v-bind:class="[dataInput.ReadOnly ? 'readOnlyList' : '', 'ellipsDiv form-control input-line fname']" v-bind:style="{ color: dataInput.ValueColor}">
            <div class="targetIsTrue">{{dataInput.Value}}</div>
        </div>

    </div>
    </template>
</div>
`
};