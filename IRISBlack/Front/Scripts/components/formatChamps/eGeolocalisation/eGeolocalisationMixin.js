import { eFileComponentsMixin } from '../../../mixins/eFileComponentsMixin.js?ver=803000'
import { verifComponent, showTooltip, hideTooltip, showInformationIco, displayInformationIco } from '../../eComponentsMethods.js?ver=803000';
import { onUpdateCallback } from '../../eFieldEditorMethods.js?ver=803000';

/**
 * Mixin commune aux composants eGeolocalisation.
 * */
export const eGeolocalisationMixin = {
    mixins: [eFileComponentsMixin],
    data() {
        return {
            //IsDisplayReadOnly: (this.propHead || this.dataInput.ReadOnly),
            modif: false
        };
    },
    computed: {
        IsDisplayReadOnly: function () {
            return (this.propHead || this.dataInput.ReadOnly)
        }
    },
    mounted() {
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
                    // Récupération des coordonnées saisies et transfert vers le champ <input> source
                    var wkt = modalDoc.getElementById("wkt");
                    if (wkt) {
                        var oldvalue = vueJsObject.dataInput.Value;
                        vueJsObject.dataInput.Value = wkt.value;
                        vueJsObject.dataInput.DisplayValue = wkt.value;
                        //Dans le cas de géoloc pour l'instant est unique on charge le event.target.value sans modifier la méthode central verifComponent
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
}