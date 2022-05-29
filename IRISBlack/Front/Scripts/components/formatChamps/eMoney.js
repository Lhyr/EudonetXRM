import { updateMethod, RemoveBorderSuccessError, verifCharacter, focusInput, verifComponent, showTooltip, hideTooltip, updateListVal, verifyValChanged, showInformationIco, displayInformationIco } from '../../methods/eComponentsMethods.js?ver=803000';
import { onUpdateCallback } from '../../methods/eFieldEditorMethods.js?ver=803000';
import { PropType } from '../../methods/Enum.min.js?ver=803000';
import { eFileComponentsMixin } from '../../mixins/eFileComponentsMixin.js?ver=803000';

export default {
    name: "eMoney",
    data() {
        return {
            bEmptyDisplayPopup: false,
            that: this,
            modif: false,
            //IsDisplayReadOnly: (this.propHead || this.dataInput.ReadOnly),
            focusIn: false,
            icon: false,
            patternVerif: /^(\d|\ )+(\.|\,)?\d*$/i,
            bRegExeSuccess: true,
            messageError: this.getRes(2469),
            PropType
        };
    },
    components: {
        eMoneyFile: () => import(AddUrlTimeStampJS("./eMoney/eMoneyFile.js")),
        eMoneyList: () => import(AddUrlTimeStampJS("./eMoney/eMoneyList.js")),
        eAlertBox: () => import(AddUrlTimeStampJS("../modale/alertBox.js"))
    },
    mounted() {
        
        this.setContextMenu();
        this.displayInformationIco();
        Vue.nextTick(() => {
            if (this.dataInput.Required && this.dataInput.Value === "")
                verifCharacter(this.dataInput, this.that)
        });
    },
    methods: {
        displayInformationIco,
        showInformationIco,
        focusInput,
        verifyValChanged,
        hideTooltip,
        updateListVal,
        showTooltip,
        onUpdateCallback,
        RemoveBorderSuccessError,
        updateMethod,
        verifComponent,
        verifMoney(event, that) {
            verifComponent(this.patternVerif, event, this.dataInput.Value, that, this.dataInput);
        }
    },
    computed: {
        IsDisplayReadOnly: function () {
            return (this.dataInput.ReadOnly)
        },
        whichIcoToDisplay() {
            return (!this.dataInput.ReadOnly) ? 'fas fa-pencil-alt' :'fas fa-lock'
        }
    },
    mixins: [eFileComponentsMixin],
    props: ["dataInput", "propHead", "propListe", "propSignet", "propIndexRow", "propAssistant", "propDetail", "propAssistantNbIndex", "propResumeEdit"],
    template: `
<div class="globalDivComponent">

    <!-- FICHE -->
    <template v-if="false">
        <eMoneyFile />
    </template>
    <template v-else>
    <div
        v-if="!propListe"
        ref="money"
        v-on:mouseout="showTooltip(false,'money',icon,IsDisplayReadOnly,dataInput)"
        v-on:mouseover="showTooltip(true,'money',icon,IsDisplayReadOnly,dataInput)"
        v-bind:class="[IsDisplayReadOnly? 'headReadOnly read-only' : '', focusIn ? 'focusIn' : '', 'ellips input-group hover-input', bEmptyDisplayPopup ? 'display-alert' : '']"
        :title="!dataInput.ToolTipText ? dataInput.DisplayValue : dataInput.ToolTipText"
    >

        <!-- <span ref="info" v-if="showInformationIco()" class="icon-info-circle info-tooltip"></span> -->
        <!-- Si le champ eNumeric est modifiable -->
        <input :field="'field'+dataInput.DescId" :IsDetail="propDetail" :IsAssistant="propAssistant" :IsHead="propHead" :IsList="propListe" 
            v-bind:class="[propHead ? 'class_liste_rubrique_money_header_' + dataInput.DescId  : '', propDetail ? 'class_liste_rubrique_money_detail_' + dataInput.DescId  : '', propAssistant ? 'class_liste_rubrique_money_assistant_' + propAssistantNbIndex + '_' + dataInput.DescId  : '', 'form-control input-line fname']" 
            v-on:focus="RemoveBorderSuccessError();bEmptyDisplayPopup = false; focusIn = true;verifyValChanged($event)" v-on:blur="verifMoney($event,that); focusIn = false"
            v-bind:style="{ color: dataInput.ValueColor}" v-if="!(dataInput.ReadOnly)" :value="dataInput.DisplayValue" type="text"
            :placeholder="dataInput.Watermark">

        <!-- Si le champ eNumeric n'est pas modifiable -->
        <span v-bind:style="{ color: dataInput.ValueColor}" v-if="dataInput.ReadOnly" class="readOnly">{{dataInput.DisplayValue}}</span>

        <!-- Icon -->
        <span v-on:click="focusInput('money', {
                    props: this.propAssistant ? PropType.Assistant : this.propDetail ? PropType.Detail : this.propListe ? PropType.Liste : PropType.Defaut,
		            propAssistantNbIndex: this.propAssistantNbIndex,
                    propIndexRow: this.propIndexRow,
                    dataInput: this.dataInput,
                    propSignet: this.propSignet
                })"  class="input-group-addon"><a  href="#!" class="hover-pen"><i :class="[IsDisplayReadOnly?'mdi mdi-lock':'fas fa-pencil-alt']"></i></a></span>
            

        <eAlertBox v-if="this.bDisplayPopup" :b-success="this.bRegExeSuccess">
            <p v-if="!this.bRegExeSuccess">{{messageError}}</p>
        </eAlertBox>

            <!-- Message d'erreur apr�s la saisie dans le champs -->
                <eAlertBox v-if="this.bEmptyDisplayPopup && !(dataInput.ReadOnly)" >
                    <p>{{getRes(2471)}}</p>
                </eAlertBox>


    </div>
    </template>

    <!-- LISTE -->
    <template v-if="false">
        <eMoneyList />
    </template>
    <template v-else>
        <div ref="money" v-on:mouseout="showTooltip(false,'money',icon,IsDisplayReadOnly,dataInput)" v-on:mouseover="showTooltip(true,'money',icon,IsDisplayReadOnly,dataInput)" v-if="propListe" v-bind:class="{'numberListField':propListe, 'ellips input-group hover-input':true,'focusIn':focusIn }
        ">
            <!-- Si le champ numeric est modifiable -->
            <!--<div v-if="!dataInput.ReadOnly">-->
                <input v-if="!dataInput.ReadOnly" :field="'field'+dataInput.DescId" v-on:focus="RemoveBorderSuccessError();bEmptyDisplayPopup = false;verifyValChanged($event);focusIn = true; " 
                    v-on:blur="verifMoney($event,that); focusIn = false" :IsDetail="propDetail" :IsAssistant="propAssistant" 
                    :IsHead="propHead" :IsList="propListe"  :class="'class_liste_rubrique_money_' + propSignet.DescId + '_' + propIndexRow + '_' + dataInput.DescId " 
                    v-bind:disabled="dataInput.ReadOnly" v-bind:style="{ color: dataInput.ValueColor}"  :value="dataInput.Value" type="text" class="form-control input-line fname"
                    :placeholder="dataInput.Watermark"/>   
            <!--</div>-->
            <!-- Si le champ numeric n'est pas modifiable -->        
            <div v-bind:style="{ color: dataInput.ValueColor}" v-else type="text" class="ellipsDiv form-control input-line fname justify-end align-center">
                {{dataInput.Value}}
            </div>

            <!-- Icon -->
            <span 
                v-on:click="focusInput('money',{
                props: propAssistant ? PropType.Assistant : propDetail ? PropType.Detail : propListe ?PropType.Liste :PropType.Defaut,
		        propAssistantNbIndex: propAssistantNbIndex,
                propIndexRow: propIndexRow,
                dataInput: dataInput,
                propSignet: propSignet});
                focusIn = !dataInput.ReadOnly" 
                :class="{'input-group-addon':true,'ReadOnly':dataInput.ReadOnly}">
                <a  href="#!" class="hover-pen">
                    <i :class="whichIcoToDisplay"></i>
                </a>
            </span>
        </div>
    </template>
</div>
`
};