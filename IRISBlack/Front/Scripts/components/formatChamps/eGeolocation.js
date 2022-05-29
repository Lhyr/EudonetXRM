import { updateMethod, focusInput, verifComponent, showTooltip, hideTooltip, showInformationIco, displayInformationIco } from '../../methods/eComponentsMethods.js?ver=803000';
import { onUpdateCallback } from '../../methods/eFieldEditorMethods.js?ver=803000';
import { PropType } from '../../methods/Enum.min.js?ver=803000';
import { eFileComponentsMixin } from '../../mixins/eFileComponentsMixin.js?ver=803000';

export default {
    name: "eGeolocation",
    data() {
        return {
            //IsDisplayReadOnly: (this.propHead || this.dataInput.ReadOnly),
            modif: false,
            dataInputValue: ""
        };
    },
    computed: {
        IsDisplayReadOnly: function () {
            return (this.dataInput.ReadOnly)
        },
        /** Récupère ou remplace la valeur si on coche le composant et met à jour le back */
        getInputValue:{
            get:function(){
                return this.dataInputValue;
            },
            set:function(val){
                this.dataInputValue = val;
            }
        }
    },
    created(){
        this.dataInputValue = this.dataInput?.Value;
    },
    mounted() {
        this.setContextMenu();
        this.displayInformationIco();
    },
    methods: {
        displayInformationIco,
        showInformationIco,
        showTooltip,
        hideTooltip,
        onUpdateCallback,
        verifComponent,
        goAction: function () {          
            var that = this;
            advancedDialog = new eModalDialog(this.dataInput.Label, 0, "eGeolocDialog.aspx", 950, 750, "modalGeoloc");
            advancedDialog.addParam("wkt", this.dataInput.Value, "post");
            advancedDialog.show();
            advancedDialog.addButton(this.getRes(29), function () {
                advancedDialog.hide();
            }, "button-gray", null, "cancel");

            var vueJsObject = this;
            advancedDialog.addButton(this.getRes(5003), function () {
                var modal = advancedDialog.getIframe();
                var fldEditor = this;
                var modalDoc = null;
               
                if (modal)
                    modalDoc = modal.document;
                if (modalDoc) {
                    // Recuperation des coordonnees saisies et transfert vers le champ <input> source
                    var wkt = modalDoc.getElementById("wkt");
                    if (wkt) {
                        var oldvalue = vueJsObject.getInputValue;
                        vueJsObject.dataInput.Value = wkt.value;
                        vueJsObject.dataInput.DisplayValue = wkt.value;
                        //Dans le cas de g�oloc pour l'instant est unique on charge le event.target.value sans modifier la m�thode central verifComponent
                        event.target.value = wkt.value;
                        verifComponent(undefined, event, oldvalue, vueJsObject, that.dataInput);
                        //verifComponent(undefined, event, this.dataInput.Value, vueJsObject);                        
                        advancedDialog.hide();                       
                    }                   
                }  
            }, "button-green", null, "ok");           
        }
    },
    props: ["dataInput", "propHead", "propListe", "propSignet", "propIndexRow", "propAssistant", "propDetail", "propAssistantNbIndex"],
    mixins: [eFileComponentsMixin],
    template: `
<div class="globalDivComponent">
    <!-- FICHE -->

    <div ref="geolocation" v-on:mouseout="showTooltip(false,'geolocation',false,IsDisplayReadOnly,dataInput)" v-on:mouseover="showTooltip(true,'geolocation',false,IsDisplayReadOnly,dataInput)" v-if="!propListe" v-bind:class="[dataInput.ReadOnly ? 'headReadOnly' : '', 'ellips input-group hover-input']"
        :title="!dataInput.ToolTipText ? dataInput.DisplayValue : dataInput.ToolTipText">


        <!-- Si le champ catalogue est modifiable et pas multiple-->
        <!-- Si le champ catalogue n'est pas multiple ou si il est pas multiple et pas modifiable -->
        <div v-on:click="[!dataInput.ReadOnly ?  goAction() : '']" v-bind:style="{ color: dataInput.ValueColor}"  type="text" class="ellipsDiv form-control input-line fname">
            {{ dataInputValue }}
        </div>

        <!-- Icon -->
        <span v-on:click="goAction" class="input-group-addon"><a href="#!" class="hover-pen"><i :class="[IsDisplayReadOnly?'mdi mdi-lock':'fas fa-pencil-alt']"></i></a></span>
    </div>


    <!-- LISTE -->

    <div ref="geolocation" v-on:mouseout="showTooltip(false,'geolocation',false,IsDisplayReadOnly,dataInput)" v-on:mouseover="showTooltip(true,'geolocation',false,IsDisplayReadOnly,dataInput)" v-if="propListe" v-bind:class="[IsDisplayReadOnly ? 'read-only' : '' ,propListe ? 'listRubriqueRelation' : '', 'ellips input-group hover-input']">

        <!-- Si le champ geolocation et est modifiable -->
        <div v-on:click="goAction($event)" v-if="!IsDisplayReadOnly"  type="text" class="ellipsDiv form-control input-line fname">
            <a class="targetIsTrue" href="#!" v-bind:style="{ color: dataInput.ValueColor}">
                {{ dataInputValue }}
            </a>
        </div>

        <!-- Si le champ geolocation n'est pas modifiable -->
        <div v-if="IsDisplayReadOnly"  type="text" class="ellipsDiv form-control input-line fname">
            <a 
                v-bind:style="{ color: dataInput.ValueColor}" 
                v-on:click="goAction($event)" 
                class="targetIsTrue linkHead readOnly"
            >
                {{ dataInputValue }}
            </a>
         </div>

    
        <!-- Icon -->
        <span 
            v-on:click="!IsDisplayReadOnly ? goAction($event) : '' "
            class="input-group-addon"
         >
            <a href="#!" class="hover-pen">
                <i :class="[IsDisplayReadOnly?'mdi mdi-lock':'fas fa-pencil-alt']"></i>
            </a>
        </span>

    </div>

</div>
`
};