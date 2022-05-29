import { updateMethod, focusInput, verifComponent, showInformationIco, displayInformationIco, showTooltip, refreshNewValues, refreshRulesExecution } from '../../methods/eComponentsMethods.js?ver=803000';
import { PropType, BtnSpecificAction } from '../../methods/Enum.min.js?ver=803000';
import EventBus from '../../bus/event-bus.js?ver=803000';
import { eFileComponentsMixin } from '../../mixins/eFileComponentsMixin.js?ver=803000';
import { setTeamsEvent, deleteTeamsEvent } from '../../methods/eTeamsMethods.js?ver=803000';

export default {
    name: "eButton",
    data() {
        return {
            modif: false,
            that: this
        };
    },
    computed: {
        IsDisplayReadOnly: function () {
            return (this.dataInput.ReadOnly)
        },
        buttonStyle() {
            let style = {
                //width: '100%!important',
                //color: this.dataInput.StyleForeColor + '!important',
                //background: this.dataInput.BackgroundColor,
                //borderColor: this.dataInput.BackgroundColor + '!important'
            }
            if (this.dataInput.ReadOnly) {
                style.background = '',
                    style.borderColor = ''
            }
            return style;
        }
    },
    mounted() {
        this.setContextMenu();
        this.displayInformationIco();
    },
    methods: {
        showTooltip,
        showInformationIco,
        displayInformationIco,
        refreshNewValues,
        refreshRulesExecution,
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
        async saveTeamsEvent() {
            let s = await setTeamsEvent(this);
            let response = JSON.parse(s);

            if (!response.IsAuthenticated) {
                let sLoginURL = "Azure/AzureLogin.ashx";
                window.open(sLoginURL);
                return;
            }

            if (response.Result) {
                refreshNewValues(this, response.Result);
                refreshRulesExecution(this, response.Result);


                let notifTeamsUpdated = {};
                notifTeamsUpdated.id = "TeamsUpdated";
                notifTeamsUpdated.title = this.getRes(3057); // "Votre rendez-vous a �t� mis � jour sur Teams", 
                notifTeamsUpdated.color = "green";
                notifToast(notifTeamsUpdated);
            }
            else {
                let notifTeamsUpdated = {};
                notifTeamsUpdated.id = "TeamsUpdated";
                notifTeamsUpdated.title = this.getRes(72); // "Une erreur est survenue", // // TODORESKHA
                notifTeamsUpdated.color = "red";
                notifToast(notifTeamsUpdated);

            }
        },
        verifButton(event, that) {
            // les actions spécifiques interceptent le fonctionnement natif du bouton
            switch (this.dataInput.SpecificAction) {
                case BtnSpecificAction.Undefined:
                    // Si pas d'action specifique attribuée on continue comme d'habitude
                    break;
                case BtnSpecificAction.CreateSaveTeamsEvent:
                    this.saveTeamsEvent();
                    return;
                case BtnSpecificAction.DeleteTeamsEvent:
                    return;
                default:
                    break;
            }


            //fin actions spécifiques


            let newVal;
            if (this.dataInput.Value == "0") {
                newVal = "1"
            } else {
                newVal = "0"
            }
            event.target.value = newVal
            verifComponent(undefined, event, this.dataInput.Value, that, this.dataInput);
        }
    },
    props: ["dataInput", "propHead", "propListe", "propSignet", "propIndexRow", "propAssistant", "propDetail", "propAssistantNbIndex"],
    mixins: [eFileComponentsMixin],
    template: `

<div>
    <!-- FICHE -->
    <div v-if="!propListe" v-bind:class="['ellips input-group hover-input btn_eudo_content']" :title="!dataInput.ToolTipText ? dataInput.Label : dataInput.ToolTipText">

        <!-- <span ref="info" v-if="showInformationIco()" class="icon-info-circle info-tooltip"></span> -->
        <!-- modification -->
        <button v-on:click="verifButton($event, that)" v-if="!(dataInput.ReadOnly)" v-bind:style="buttonStyle" :value="dataInput.Value" type="button" class="btn_eudo btn btn-default nouveau-contact">{{dataInput.Label}}</button>
        
        <!-- non modification -->
        <button v-if="dataInput.ReadOnly" disabled :style="buttonStyle"  :value="dataInput.Label" type="button" class="btn_eudo btn btn-default nouveau-contact d-flex">
            <span class="flex-grow-1">{{dataInput.Label}}</span>
        <i class="eudo-lock mdi mdi-lock"></i></button>
       
    </div>

    <!-- LISTE -->
    <div v-if="propListe" v-bind:style="{ width: '100%!important', textAlign: 'center'}"  v-bind:class="[propListe ? 'listRubriqueBtn' : '', 'btn_eudo_content ellips input-group hover-input']">
        
        <!-- modification -->
        <button v-if="!dataInput.ReadOnly" v-on:click="verifButton($event, that)"  v-bind:style="{ width: 'auto!important', color: dataInput.StyleForeColor+'!important', background: dataInput.BackgroundColor+'!important', borderColor: dataInput.BackroundColor+'!important'}"   :value="dataInput.Value" type="button" class="btn_eudo btn btn-default nouveau-contact">{{dataInput.Label}}</button>
        
        <!-- non modification -->
        <button v-if="dataInput.ReadOnly" disabled v-bind:style="{ width: 'auto!important', color: dataInput.StyleForeColor+'!important', background: dataInput.BackgroundColor+'!important', borderColor: dataInput.BackroundColor+'!important'}"  :value="dataInput.Value" type="button" class="btn_eudo btn btn-default nouveau-contact">{{dataInput.Label}}<i class="eudo-lock mdi mdi-lockk"></i></button>
       
    </div>

</div>
`
};