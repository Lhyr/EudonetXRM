import EventBus from '../../bus/event-bus.js?ver=803000';
import { RemoveBorderSuccessError, verifCharacter, verifComponent, showTooltip, showInformationIco, displayInformationIco } from '../../methods/eComponentsMethods.js?ver=803000';
import { getTabDescid } from "../../methods/eMainMethods.js?ver=803000";
import { PropType } from '../../methods/Enum.min.js?ver=803000';
import { eFileComponentsMixin } from '../../mixins/eFileComponentsMixin.js?ver=803000';
import { store } from '../../../Scripts/store/store.js?ver=803000';

export default {
    name: "ePJ",
    data() {
        return {
            messageError: top._res_2469,
            messageSuccess: "",
            modif: false,
            bRegExeSuccess: true,
            bDisplayPopup: false,
            bEmptyDisplayPopup: false,
            that: this,
            patternVerif: /^(\d|\ )+(\.|\,)?\d*$/i,
            inputHovered: false,            
            focusIn: false,
            icon: false,
            PropType
        };
    },

    computed: {
        IsDisplayReadOnly: function () {
            return (this.dataInput.ReadOnly)
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
    },
    methods: {
        getTabDescid,
        displayInformationIco,
        showInformationIco,       
        showTooltip,       
        RemoveBorderSuccessError,
        verifComponent,       
        verifNumeric(event, that) {
            verifComponent(this.patternVerif, event, this.dataInput.Value, that, this.dataInput);
        },
        goAction(ev) {
            let nTab = this.getTabDescid(this.dataInput.DescId);
            let nFileId = this.dataInput.FileId;
            let nSourceTpl = "tpl";
            let pjSpan = null;
            let bEmailing = null;
            let idsOfPj = "";

            var afterValidate = (function () {
                var options = {
                    id: this.propSignet?.id,
                    signet: nTab,
                    nbLine: 9,
                    pageNum: 1
                };
                EventBus.$emit('reloadSignet_' + options.id, options);
            }).bind(this);

            this.showPJDialog(nTab, nFileId, nSourceTpl, pjSpan, bEmailing, idsOfPj,afterValidate);
        },
            showPJDialog(nTab, nFileId, nSourceTpl, pjSpan, bEmailing, idsOfPj, callBack) {


            var _titreFen = top._res_6316;
            var _width = 927;
            var _height = 550;

            oModalPJAdd = new eModalDialog(_titreFen, 0, 'ePjAddFromTpl.aspx', _width, _height, "modalPJAdd");

            oModalPJAdd.addParam("nTab", nTab, "post");
            oModalPJAdd.addParam("nFileID", nFileId, "post");
            oModalPJAdd.addParam("iDsOfPj", idsOfPj, "post");
            oModalPJAdd.addParam("width", _width, "post");
            oModalPJAdd.addParam("height", _height, "post");

            if (idsOfPj && idsOfPj.length > 0 && idsOfPj != "0")
                oModalPJAdd.addParam("viewtype", "checkedonly", "post");


            oModalPJAdd.fileId = nFileId;
            oModalPJAdd.tab = nTab;

            //Ajout de pj depuis ckeditor de l'assistant d'emailing
            if (bEmailing != "undefined" && bEmailing && oMailing != null) {
                oModalPJAdd.addParam("parentEvtFileId", oMailing._nParentFileId, "post");
                oModalPJAdd.addParam("parentEvtTabId", oMailing._nParentTabId, "post");
                oModalPJAdd.addParam("selectonclick", "1", "post");
                oModalPJAdd.oMailing = oMailing;
            }
            else if (nSourceTpl == "tpl" || nSourceTpl == "tplmail") {

                oModalPJAdd.addParam("fromtpl", "1", "post");
                // div qui contient le span de la Liste des noms des pièces jointes pour les tplMail
                var forward = document.getElementById('mailForward');
                if (forward)
                    oModalPJAdd.addParam("mailForward", forward.value, "post");

                AddTplContext(oModalPJAdd);
            }
            else if (nSourceTpl == "mailtemplate") {
                oModalPJAdd.addParam("frommailtemplate", "1", "post");
            }

            oModalPJAdd.pjSpan = pjSpan;
            oModalPJAdd.sourcePJ = nSourceTpl;

            oModalPJAdd.onHideFunction = function () { top.oModalPJAdd = null; };
            oModalPJAdd.ErrorCallBack = launchInContext(oModalPJAdd, oModalPJAdd.hide);

            oModalPJAdd.show();
            if (nSourceTpl == "tpl" || nSourceTpl == "tplmail") {
                oModalPJAdd.addButton(top._res_29, cancelPJAdd, "button-gray", null, "cancel");
                oModalPJAdd.addButton(top._res_5003,  () => {
                        AddPjfromTpl();
                         callBack();
                   }, "button-green"); //Valider    return oModalPJAdd;
            }
            else {
                //  oModalPJAdd.addButton(top._res_29, cancelPJAdd, "button-gray",null);
                oModalPJAdd.addButton(top._res_5003, "button-green", "", "cancel"); //Valider    return oModalPJAdd;
            }

            top.oModalPJAdd = oModalPJAdd;
            return oModalPJAdd;
        }
    },
    props: ["dataInput", "propHead", "propListe", "propSignet", "propIndexRow", "propAssistant", "propDetail", "propAssistantNbIndex", "propResumeEdit"],
    template: `

<div class="globalDivComponent">
   <!-- FICHE -->

    <div ref="PJ" v-on:mouseout="showTooltip(false,'PJ',icon,IsDisplayReadOnly,dataInput)" v-on:mouseover="showTooltip(true,'PJ',icon,IsDisplayReadOnly,dataInput)"  v-if="!propListe" v-bind:class="[focusIn ? 'focusIn': '', IsDisplayReadOnly? 'headReadOnly read-only' : '', 'ellips input-group hover-input']"
        :title="!dataInput.ToolTipText ? dataInput.DisplayValue : dataInput.ToolTipText">      
        <a @click="goAction($event)" v-bind:style="{ color: dataInput.ValueColor}" class="readOnly">{{dataInput.DisplayValue}}</a>

        <eAlertBox v-if="this.bDisplayPopup" :b-success="this.bRegExeSuccess">
            <p v-if="!this.bRegExeSuccess">{{messageError}}</p>
        </eAlertBox>

    	<!-- Message d'erreur si champs obligatoire est vide -->
        <eAlertBox v-if="dataInput.Required && this.bEmptyDisplayPopup && !(dataInput.ReadOnly)" >
            <p>{{getRes(2471)}}</p>
        </eAlertBox>
    </div>

    <!-- LISTE -->

    <div ref="PJ" @click="goAction($event)" v-on:mouseout="showTooltip(false,'PJ',icon,IsDisplayReadOnly,dataInput)" v-on:mouseover="showTooltip(true,'PJ',icon,IsDisplayReadOnly,dataInput)" v-if="propListe" v-bind:class="[propListe ? 'listRubriqueCaractere' : '', 'ellips input-group hover-input',focusIn ? 'focusIn' : '']">
        <a v-bind:style="{ color: dataInput.ValueColor}" v-if="dataInput.DisplayValue!=0"type="text" class="ellipsDiv form-control input-line fname">
            {{dataInput.DisplayValue}}
        </a>      
    </div>

</div>
`
};