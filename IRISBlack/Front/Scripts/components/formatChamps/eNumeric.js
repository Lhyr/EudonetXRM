import EventBus from '../../bus/event-bus.js?ver=803000';
import { RemoveBorderSuccessError, verifCharacter, verifComponent, focusInput, showTooltip, hideTooltip, showInformationIco, displayInformationIco  } from '../../methods/eComponentsMethods.js?ver=803000';
import { onUpdateCallback } from '../../methods/eFieldEditorMethods.js?ver=803000';
import { PropType } from '../../methods/Enum.min.js?ver=803000';
import { eFileComponentsMixin } from '../../mixins/eFileComponentsMixin.js?ver=803000';

export default {
    name: "eNumeric",
    data() {
        return {
            margingOfValue:3,
            messageError: top._res_2469,
            messageSuccess: "",
            modif: false,
            bRegExeSuccess: true,
            bDisplayPopup: false,
            bEmptyDisplayPopup: false,
            that:this,
            patternVerif: /^(\-)?(\d|\ )+(\.|\,)?\d*$/i,
            inputHovered: false,
            focusIn: false,
            icon: false,
            PropType,
            dataInputValue: "",
            sBgColor:"rgba(0,0,0,.10)",
            oNumInputHeight:{
                readWrite:1.8,
                readOnly:2.738,
                list:1.563
            },
            patternNum: /[^.,\d ]/gu
        };
    },
    created(){
        this.dataInputValue = this.dataInput?.DisplayValue || this.dataInput?.Value;
    },
    computed: {
        IsDisplayReadOnly: function () {
            return (this.dataInput.ReadOnly)
        },
        whichIcoToDisplay() {
            return (!this.dataInput.ReadOnly) ? 'fas fa-pencil-alt' : 'fas fa-lock'
        },
        getCssClass() {
            return this.propListe ? 'pa-0 ma-0' : 'ma-0';
        },
        getHideDetails() {
            return this.propListe ? 'auto' : undefined
        },
        /** hauteur du champs */
        getHeight() {
            return (this.propListe ? this.oNumInputHeight.list 
            : this.dataInput.ReadOnly ? this.oNumInputHeight.readOnly 
            : this.oNumInputHeight.readWrite)
            + 'rem' 
        },
        getTooltip() {
            return this.dataInput?.ToolTipText ? this.dataInput?.ToolTipText + ' - ' + this.dataInput?.DisplayValue : ''
        },
        getLabel() {
            return !this.dataInput?.LabelHidden ? this.dataInput?.Label : ''
        },
        /** Retourne le fond si readonly */
        getBgColor() {
            return this.dataInput?.ReadOnly ? this.sBgColor : ''
        }
    },
    mixins: [eFileComponentsMixin],
    components: {
        eAlertBox: () => import(AddUrlTimeStampJS("../modale/alertBox.js"))
    },
    mounted() {
        this.setContextMenu();
        this.displayInformationIco();
        Vue.nextTick(() => {
            if (this.dataInput.Required && this.dataInput.Value === "")
                verifCharacter(this.dataInput, this.that)
        });

        /** Permet de calculer la largeur du champs en fonction de la valeur */
        this.getWidth();
    },
    methods: {
        displayInformationIco,
        showInformationIco,
        hideTooltip,
        showTooltip,
        focusInput,
        RemoveBorderSuccessError,
        verifComponent,
        onUpdateCallback,
        verifNumeric(event, that) {
            verifComponent(this.patternVerif, event, this.dataInput.Value, that, this.dataInput);
        },

        /** Permet de calculer la largeur du champs en fonction de la valeur */
        getWidth() {
            var input = this.$refs.ednNum.$el.getElementsByTagName("input")[0];
            if (input.value != "") {
                input.style.maxWidth = this.getTextWidth(input.value, getComputedStyle(input).font) + this.margingOfValue + 'px';
            } else {
                if (input.placeholder) {
                    input.style.maxWidth = this.getTextWidth(input.placeholder, getComputedStyle(input).font) + this.margingOfValue + 'px';
                } else {
                    input.style.maxWidth = "100%"
                }
            }
        },
        /** Calcule la largeur d'un string en widht :) */
        getTextWidth(text, font) {
            const canvas = document.createElement('canvas');
            const context = canvas.getContext('2d');
            context.font = font || getComputedStyle(document.body).font;
            return context.measureText(text).width;
}
    },
    props: [
        "dataInput", 
        "propHead", 
        "propListe", 
        "propSignet", 
        "propIndexRow", 
        "propAssistant", 
        "propDetail", 
        "propAssistantNbIndex" , 
        "propResumeEdit"
    ],
    template: `

<div class="globalDivComponent d-flex justify-start align-start full-width">
    <edn-num
        ref="ednNum"
        :class="getCssClass"
        :single-line="propListe"
        :hide-details="getHideDetails"
        :height="getHeight"
        :prefix="dataInput?.Unit?.Position == 1 ? dataInput?.Unit?.Unit : ''"
        :suffix="(dataInput?.Unit?.Position ?? 0) == 0 ? dataInput?.Unit?.Unit : ''"
        :tooltip="getTooltip" 
        :value="dataInputValue"
        :label="getLabel"
        :placeholder="dataInput?.Watermark"
        v-on:blur="verifNumeric($event,that)"
        @input="getWidth()"
        :rdonly="dataInput?.ReadOnly"
        dense
        :backgroundColor="getBgColor"
        :pattern="patternNum"
    />
</div>
`
};